using AgileObjects.AgileMapper;
using AngularJSAuthentication.API.Controllers;
using AngularJSAuthentication.API.Controllers.Base;
using AngularJSAuthentication.API.Helper;
using AngularJSAuthentication.API.Helpers;
using AngularJSAuthentication.Common.Enums;
using AngularJSAuthentication.Common.Helpers;
using AngularJSAuthentication.DataContracts.constants;
using AngularJSAuthentication.DataContracts.Masters;
using AngularJSAuthentication.DataContracts.Mongo;
using AngularJSAuthentication.DataContracts.Transaction.Stocks;
using AngularJSAuthentication.DataContracts.Transaction.TripPlanner;
using AngularJSAuthentication.Model;
using AngularJSAuthentication.Model.DeliveryOptimization;
using AngularJSAuthentication.Model.KisanDan;
using AngularJSAuthentication.Model.VAN;
using GenricEcommers.Models;
using LinqKit;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;
using Nito.AspNetBackgroundTasks;
using Nito.AsyncEx;
using NLog;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Mail;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using System.Web.Http;
using static AngularJSAuthentication.API.Controllers.External.Other.SellerStoreController;
using static AngularJSAuthentication.API.Controllers.NotificationController;
using AngularJSAuthentication.API.Helper.Notification;



namespace AngularJSAuthentication.API.ControllerV1
{
    [RoutePrefix("api/DeliveryTask")]
    public class DeliveryTaskController : BaseAuthController
    {
        private static TimeZoneInfo INDIAN_ZONE = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");

        DateTime indianTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE);

        public static Logger logger = LogManager.GetCurrentClassLogger();
        public static List<int> ordersToProcess = new List<int>();

        [Route("")]
        [HttpGet]
        public HttpResponseMessage getOrdersHistory(string mob, DateTime start, DateTime end, int dboyId) //get orders History by Date time
        {
            using (var context = new AuthContext())
            {
                try
                {
                    logger.Info("start Item Upload Exel File: ");
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

                    if (start != null && end != null)
                    {
                        var k = end.AddDays(1);
                        var DBoyorders = context.getDBoyOrdersHistory(mob, start, k, dboyId);
                        return Request.CreateResponse(HttpStatusCode.OK, DBoyorders);
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.BadRequest, "Select Date");
                    }
                }
                catch (Exception ex)
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, ex.Message);
                }
            }
        }

        [Route("V1")]
        [HttpGet]
        public HttpResponseMessage getOrdersHistoryV1(string mob, DateTime start, DateTime end, int dboyId) //get orders History by Date time
        {
            using (var context = new AuthContext())
            {
                try
                {
                    res res;
                    logger.Info("start Item getOrdersHistoryV1 ");
                    if (start != null && end != null)
                    {
                        var k = end.AddDays(1);
                        var DBoyorders = context.getDBoyOrdersHistory(mob, start, k, dboyId);


                        if (DBoyorders.Count == 0)
                        {
                            res = new res()
                            {
                                OrderHistory = null,
                                Message = "No record found",
                                status = false
                            };
                            return Request.CreateResponse(HttpStatusCode.BadRequest, res);
                        }
                        else
                        {
                            res = new res()
                            {
                                OrderHistory = DBoyorders,
                                Message = "Record found",
                                status = true
                            };
                            return Request.CreateResponse(HttpStatusCode.OK, res);
                        }

                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.BadRequest, "Select Date");
                    }
                }
                catch (Exception ex)
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, ex.Message);
                }
            }
        }

        //[Route("")]
        //[HttpPost]
        //public HttpResponseMessage post(OrderDispatchedMaster obj) //Order delivered or canceled from delivery app
        //{
        //    using (var context = new AuthContext())
        //    {
        //        try
        //        {
        //            var DBoyorders = context.orderdeliveredreturn(obj);
        //            if (DBoyorders == null)
        //            {
        //                return Request.CreateResponse(HttpStatusCode.BadRequest, "Not Delivered");
        //            }
        //            return Request.CreateResponse(HttpStatusCode.OK, DBoyorders);
        //        }
        //        catch (Exception ex)
        //        {
        //            return Request.CreateResponse(HttpStatusCode.BadRequest, ex.Message);
        //        }
        //    }
        //}

        [Route("OrderDetail")]
        [HttpGet]
        public HttpResponseMessage getDeliveryAppOrders(int OrderId) //get orders for delivery
        {
            using (var db = new AuthContext())
            {
                try
                {
                    var DBoyorders = (from a in db.OrderDispatchedMasters
                                      where (a.Status == "Shipped") && a.OrderId == OrderId
                                      join i in db.Customers on a.CustomerId equals i.CustomerId
                                      select new OrderDispatchedMasterDTO
                                      {
                                          lat = i.lat,
                                          lg = i.lg,
                                          ClusterId = a.ClusterId,
                                          ClusterName = a.ClusterName,
                                          active = a.active,
                                          BillingAddress = a.BillingAddress,
                                          CityId = a.CityId,
                                          comments = a.comments,
                                          CompanyId = a.CompanyId,
                                          CreatedDate = a.CreatedDate,
                                          CustomerId = a.CustomerId,
                                          CustomerName = a.CustomerName,
                                          ShopName = a.ShopName,
                                          Skcode = a.Skcode,
                                          Customerphonenum = a.Customerphonenum,
                                          DboyMobileNo = a.DboyMobileNo,
                                          DboyName = a.DboyName,
                                          Deleted = a.Deleted,
                                          Deliverydate = a.Deliverydate,
                                          DiscountAmount = a.DiscountAmount,
                                          DivisionId = a.DivisionId,
                                          GrossAmount = a.GrossAmount,
                                          invoice_no = a.invoice_no,
                                          orderDetails = a.orderDetails,
                                          OrderDispatchedMasterId = a.OrderDispatchedMasterId,
                                          OrderId = a.OrderId,
                                          RecivedAmount = a.RecivedAmount,
                                          ReDispatchCount = a.ReDispatchCount,
                                          //SalesPerson = a.SalesPerson,
                                          //SalesPersonId = a.SalesPersonId,
                                          ShippingAddress = a.ShippingAddress,
                                          Status = a.Status,
                                          TaxAmount = a.TaxAmount,
                                          TotalAmount = a.TotalAmount,
                                          UpdatedDate = a.UpdatedDate,
                                          WarehouseId = a.WarehouseId,
                                          WarehouseName = a.WarehouseName,
                                          DeliveryIssuanceId = a.DeliveryIssuanceIdOrderDeliveryMaster
                                      }).ToList();
                    return Request.CreateResponse(HttpStatusCode.OK, DBoyorders);
                }
                catch (Exception ex)
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, ex.Message);
                }
            }
        }
        #region Version 2 del app api

        //#region post Order Place not using from may2019
        //[Route("V2")]
        //[HttpPost]
        //public HttpResponseMessage postV2(OrderPlaceDTO obj) //Order delivered or canceled from delivery app
        //{
        //    ResDTO res;
        //    using (var context = new AuthContext())
        //    {
        //        try
        //        {
        //            var DBoyorders = context.orderdeliveredreturnV2(obj);
        //            if (DBoyorders == null)
        //            {
        //                res = new ResDTO()
        //                {
        //                    op = null,
        //                    Message = "Not Delivered",
        //                    Status = false
        //                };
        //                return Request.CreateResponse(HttpStatusCode.BadRequest, res);
        //            }
        //            res = new ResDTO()
        //            {
        //                op = DBoyorders,
        //                Message = "Success",
        //                Status = true
        //            };
        //            return Request.CreateResponse(HttpStatusCode.OK, res);
        //        }
        //        catch (Exception ex)
        //        {
        //            res = new ResDTO()
        //            {
        //                op = null,
        //                Message = "Failed. " + ex.Message + "",
        //                Status = false
        //            };
        //            return Request.CreateResponse(HttpStatusCode.BadRequest, res);
        //        }
        //    }
        //}
        //#endregion

        #region get order detail
        /// <summary>
        /// Created by 07-03-2019        
        /// </summary>
        /// <param name="OrderId"></param>
        /// <returns></returns>
        [Route("OrderDetail/V2")]
        [HttpGet]
        [AllowAnonymous]
        public HttpResponseMessage getDeliveryAppOrdersV2(int OrderId) //get orders for delivery
        {
            OrderDispatchedDtoObj res;
            bool IsQREnabled = false;
            using (var db = new AuthContext())
            {
                try
                {
                    OrderDispatchedMasterDTO DBoyorders = (from a in db.OrderDispatchedMasters
                                                           where a.OrderId == OrderId
                                                           join i in db.Customers on a.CustomerId equals i.CustomerId
                                                           select new OrderDispatchedMasterDTO
                                                           {
                                                               lat = i.lat,
                                                               lg = i.lg,
                                                               ClusterId = a.ClusterId,
                                                               ClusterName = a.ClusterName,
                                                               active = a.active,
                                                               BillingAddress = a.BillingAddress,
                                                               CityId = a.CityId,
                                                               comments = a.comments,
                                                               CompanyId = a.CompanyId,
                                                               CreatedDate = a.CreatedDate,
                                                               CustomerId = a.CustomerId,
                                                               CustomerName = a.CustomerName,
                                                               ShopName = a.ShopName,
                                                               Skcode = a.Skcode,
                                                               Customerphonenum = a.Customerphonenum,
                                                               DboyMobileNo = a.DboyMobileNo,
                                                               DboyName = a.DboyName,
                                                               Deleted = a.Deleted,
                                                               Deliverydate = a.Deliverydate,
                                                               DiscountAmount = a.DiscountAmount,
                                                               DivisionId = a.DivisionId,
                                                               GrossAmount = a.GrossAmount,
                                                               invoice_no = a.invoice_no,
                                                               orderDetails = a.orderDetails,
                                                               OrderDispatchedMasterId = a.OrderDispatchedMasterId,
                                                               OrderId = a.OrderId,
                                                               RecivedAmount = a.RecivedAmount,
                                                               ReDispatchCount = a.ReDispatchCount,
                                                               //SalesPerson = a.SalesPerson,
                                                               //SalesPersonId = a.SalesPersonId,
                                                               ShippingAddress = a.ShippingAddress,
                                                               Status = a.Status,
                                                               TaxAmount = a.TaxAmount,
                                                               TotalAmount = a.TotalAmount,
                                                               UpdatedDate = a.UpdatedDate,
                                                               WarehouseId = a.WarehouseId,
                                                               WarehouseName = a.WarehouseName,
                                                               Trupay = a.Trupay,
                                                               //trupay Transaction for new field
                                                               TrupayTransactionId = a.TrupayTransactionId,
                                                               paymentMode = a.paymentMode,
                                                               paymentThrough = a.paymentThrough,
                                                               DSignimg = a.DSignimg,
                                                               DeliveryIssuanceId = a.DeliveryIssuanceIdOrderDeliveryMaster
                                                           }).FirstOrDefault();
                    if (DBoyorders != null)
                    {
                        DBoyorders.orderDetails = DBoyorders.orderDetails.Where(x => x.qty > 0).ToList();
                        #region for Partial refund process
                        var Query = "exec GetRetailerOrderPayment " + OrderId;
                        var payments = db.Database.SqlQuery<RetailerOrderPaymentDc>(Query).ToList();
                        int wid = DBoyorders.WarehouseId;

                        if (payments.Any(c => c.PaymentFrom == "Cash"))
                        {
                            IsQREnabled = db.Warehouses.Any(x => x.WarehouseId == wid && x.IsQREnabled);
                        }

                        if (payments.Any(c => c.IsOnline))
                        {
                            DBoyorders.IsDeliveryCancelledEnable = false;
                            if (db.Warehouses.Any(x => x.WarehouseId == wid && x.IsOnlineRefundEnabled)/* && (!db.DbOrderMaster.Any(x => x.OrderId == DBoyorders.OrderId && x.OrderType == 8))*/)
                            {
                                var RefundDays = db.PaymentRefundApis.Where(x => x.DaysForRefundEligible > 0 && x.IsActive && x.IsDeleted == false).Select(x => new { x.ApiName, x.DaysForRefundEligible }).ToList();
                                foreach (var item in payments.Where(x => x.IsOnline == true))
                                {
                                    var PaymentRefundDays = RefundDays.FirstOrDefault(x => x.ApiName.Trim().ToLower() == item.PaymentFrom.Trim().ToLower());
                                    if (item.PaymentFrom.Trim().ToLower() != "gullak")
                                    {
                                        if (PaymentRefundDays != null && PaymentRefundDays.DaysForRefundEligible > 0 && item.TxnDate.AddDays(PaymentRefundDays.DaysForRefundEligible) < DateTime.Now)
                                        {
                                            DBoyorders.IsDeliveryCancelledEnable = false;
                                            break;
                                        }
                                        else
                                        {
                                            DBoyorders.IsDeliveryCancelledEnable = true;
                                        }
                                    }
                                    else if (item.PaymentFrom.Trim().ToLower() == "gullak")
                                    {
                                        DBoyorders.IsDeliveryCancelledEnable = true;

                                    }
                                }
                            }
                        }
                        else
                        {
                            DBoyorders.IsDeliveryCancelledEnable = true;
                        }
                        #endregion
                        #region old code
                        //var payments = db.PaymentResponseRetailerAppDb.Where(x => x.OrderId == OrderId && x.status == "Success").ToList();

                        //if (payments == null || !payments.Any())
                        //{
                        //    payments = new List<PaymentResponseRetailerApp>();
                        //    if (DBoyorders.GrossAmount > 0 && DBoyorders.ElectronicAmount == 0)
                        //    {
                        //        payments.Add(new PaymentResponseRetailerApp
                        //        {
                        //            amount = DBoyorders.GrossAmount,
                        //            PaymentFrom = "Cash",
                        //            IsRefund = false

                        //        });
                        //    }

                        //    if (DBoyorders.CheckAmount > 0)
                        //    {
                        //        payments.Add(new PaymentResponseRetailerApp
                        //        {
                        //            amount = DBoyorders.CheckAmount,
                        //            GatewayTransId = DBoyorders.CheckNo,
                        //            PaymentFrom = "Cheque",
                        //            IsRefund = false
                        //        });
                        //    }

                        //    if (DBoyorders.ElectronicAmount > 0)
                        //    {
                        //        payments.Add(new PaymentResponseRetailerApp
                        //        {
                        //            amount = DBoyorders.ElectronicAmount,
                        //            GatewayTransId = DBoyorders.ElectronicPaymentNo,
                        //            PaymentFrom = "Truepay",
                        //            IsRefund = false
                        //        });
                        //    }
                        //}
                        #endregion
                        res = new OrderDispatchedDtoObj()
                        {
                            OrderDispatchedObj = DBoyorders,
                            Payments = payments,
                            IsQREnabled = IsQREnabled,
                            status = true,
                            Message = "Success."
                        };
                    }
                    else
                    {
                        res = new OrderDispatchedDtoObj()
                        {
                            OrderDispatchedObj = DBoyorders,
                            status = false,
                            Message = "Failed---."
                        };

                    }
                    return Request.CreateResponse(HttpStatusCode.OK, res);
                }
                catch (Exception ex)
                {
                    res = new OrderDispatchedDtoObj()
                    {
                        OrderDispatchedObj = null,
                        status = false,
                        Message = "Failed--" + ex.Message + ""
                    };
                    return Request.CreateResponse(HttpStatusCode.BadRequest, res);
                }
            }
        }


        [Route("DeclineOrderDetail")]
        [HttpGet]
        public HttpResponseMessage DeclineOrderDetail(int OrderId) //get orders for delivery
        {
            OrderDispatchedDtoObj res;
            bool IsQREnabled = false;
            using (var db = new AuthContext())
            {
                try
                {
                    OrderDispatchedMasterDTO DBoyorders = (from a in db.OrderDispatchedMasters
                                                           where a.OrderId == OrderId
                                                           join i in db.Customers on a.CustomerId equals i.CustomerId
                                                           select new OrderDispatchedMasterDTO
                                                           {
                                                               lat = i.lat,
                                                               lg = i.lg,
                                                               ClusterId = a.ClusterId,
                                                               ClusterName = a.ClusterName,
                                                               active = a.active,
                                                               BillingAddress = a.BillingAddress,
                                                               CityId = a.CityId,
                                                               comments = a.comments,
                                                               CompanyId = a.CompanyId,
                                                               CreatedDate = a.CreatedDate,
                                                               CustomerId = a.CustomerId,
                                                               CustomerName = a.CustomerName,
                                                               ShopName = a.ShopName,
                                                               Skcode = a.Skcode,
                                                               Customerphonenum = a.Customerphonenum,
                                                               DboyMobileNo = a.DboyMobileNo,
                                                               DboyName = a.DboyName,
                                                               Deleted = a.Deleted,
                                                               Deliverydate = a.Deliverydate,
                                                               DiscountAmount = a.DiscountAmount,
                                                               DivisionId = a.DivisionId,
                                                               GrossAmount = a.GrossAmount,
                                                               invoice_no = a.invoice_no,
                                                               orderDetails = a.orderDetails,
                                                               OrderDispatchedMasterId = a.OrderDispatchedMasterId,
                                                               OrderId = a.OrderId,
                                                               RecivedAmount = a.RecivedAmount,
                                                               ReDispatchCount = a.ReDispatchCount,
                                                               //SalesPerson = a.SalesPerson,
                                                               //SalesPersonId = a.SalesPersonId,
                                                               ShippingAddress = a.ShippingAddress,
                                                               Status = a.Status,
                                                               TaxAmount = a.TaxAmount,
                                                               TotalAmount = a.TotalAmount,
                                                               UpdatedDate = a.UpdatedDate,
                                                               WarehouseId = a.WarehouseId,
                                                               WarehouseName = a.WarehouseName,
                                                               Trupay = a.Trupay,
                                                               //trupay Transaction for new field
                                                               TrupayTransactionId = a.TrupayTransactionId,
                                                               paymentMode = a.paymentMode,
                                                               paymentThrough = a.paymentThrough,
                                                               DSignimg = a.DSignimg,
                                                               DeliveryIssuanceId = a.DeliveryIssuanceIdOrderDeliveryMaster

                                                           }).SingleOrDefault();



                    if (DBoyorders != null)
                    {
                        #region for Partial refund process
                        var Query = "exec GetDeclineRetailerOrderPayment " + OrderId;
                        var payments = db.Database.SqlQuery<RetailerOrderPaymentDc>(Query).ToList();
                        int wid = DBoyorders.WarehouseId;

                        if (payments.Any(c => c.PaymentFrom == "Cash"))
                        {
                            IsQREnabled = db.Warehouses.Any(x => x.WarehouseId == wid && x.IsQREnabled);
                        }

                        if (payments.Any(c => c.IsOnline))
                        {
                            DBoyorders.IsDeliveryCancelledEnable = false;
                            if (db.Warehouses.Any(x => x.WarehouseId == wid && x.IsOnlineRefundEnabled) /*&& (!db.DbOrderMaster.Any(x => x.OrderId == DBoyorders.OrderId && x.OrderType == 8))*/)
                            {
                                var RefundDays = db.PaymentRefundApis.Where(x => x.DaysForRefundEligible > 0 && x.IsActive && x.IsDeleted == false).Select(x => new { x.ApiName, x.DaysForRefundEligible }).ToList();
                                foreach (var item in payments.Where(x => x.IsOnline == true))
                                {
                                    var PaymentRefundDays = RefundDays.FirstOrDefault(x => x.ApiName.Trim().ToLower() == item.PaymentFrom.Trim().ToLower());
                                    if (item.PaymentFrom.Trim().ToLower() != "gullak")
                                    {
                                        if (PaymentRefundDays != null && PaymentRefundDays.DaysForRefundEligible > 0 && item.TxnDate.AddDays(PaymentRefundDays.DaysForRefundEligible) < DateTime.Now)
                                        {
                                            DBoyorders.IsDeliveryCancelledEnable = false;
                                            break;
                                        }
                                        else
                                        {
                                            DBoyorders.IsDeliveryCancelledEnable = true;
                                        }
                                    }
                                    else if (item.PaymentFrom.Trim().ToLower() == "gullak")
                                    {
                                        DBoyorders.IsDeliveryCancelledEnable = true;

                                    }
                                }
                            }
                        }
                        else
                        {
                            DBoyorders.IsDeliveryCancelledEnable = true;
                        }
                        #endregion


                        #region old code
                        //var payments = db.PaymentResponseRetailerAppDb.Where(x => x.OrderId == OrderId && x.status == "Success").ToList();

                        //if (payments == null || !payments.Any())
                        //{
                        //    payments = new List<PaymentResponseRetailerApp>();
                        //    if (DBoyorders.GrossAmount > 0 && DBoyorders.ElectronicAmount == 0)
                        //    {
                        //        payments.Add(new PaymentResponseRetailerApp
                        //        {
                        //            amount = DBoyorders.GrossAmount,
                        //            PaymentFrom = "Cash",
                        //            IsRefund = false

                        //        });
                        //    }

                        //    if (DBoyorders.CheckAmount > 0)
                        //    {
                        //        payments.Add(new PaymentResponseRetailerApp
                        //        {
                        //            amount = DBoyorders.CheckAmount,
                        //            GatewayTransId = DBoyorders.CheckNo,
                        //            PaymentFrom = "Cheque",
                        //            IsRefund = false
                        //        });
                        //    }

                        //    if (DBoyorders.ElectronicAmount > 0)
                        //    {
                        //        payments.Add(new PaymentResponseRetailerApp
                        //        {
                        //            amount = DBoyorders.ElectronicAmount,
                        //            GatewayTransId = DBoyorders.ElectronicPaymentNo,
                        //            PaymentFrom = "Truepay",
                        //            IsRefund = false
                        //        });
                        //    }
                        //}
                        #endregion

                        res = new OrderDispatchedDtoObj()
                        {
                            OrderDispatchedObj = DBoyorders,
                            Payments = payments,
                            IsQREnabled = IsQREnabled,
                            status = true,
                            Message = "Success."
                        };
                    }
                    else
                    {
                        res = new OrderDispatchedDtoObj()
                        {
                            OrderDispatchedObj = DBoyorders,
                            status = false,
                            Message = "Failed---."
                        };

                    }
                    return Request.CreateResponse(HttpStatusCode.OK, res);
                }
                catch (Exception ex)
                {
                    res = new OrderDispatchedDtoObj()
                    {
                        OrderDispatchedObj = null,
                        status = false,
                        Message = "Failed--" + ex.Message + ""
                    };
                    return Request.CreateResponse(HttpStatusCode.BadRequest, res);
                }
            }
        }
        #endregion

        #region My Task Api
        /// <summary>
        /// Created by 07-03-2019
        /// </summary>
        /// <param name="mob"></param>
        /// <returns></returns>
        [Route("V2")]
        [HttpGet]
        public HttpResponseMessage getAppOrdersV2(string mob) //get orders for delivery
        {
            OrderDispatchedListDtoObj res;
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
                    var DBoyorders = context.getAcceptedOrders(mob);
                    if (DBoyorders.Count == 0)
                    {
                        res = new OrderDispatchedListDtoObj()
                        {
                            OrderDispatchedObj = null,
                            status = false,
                            Message = "Data not exist."
                        };
                    }
                    else
                    {
                        res = new OrderDispatchedListDtoObj()
                        {
                            OrderDispatchedObj = DBoyorders,
                            status = true,
                            Message = "Success"
                        };
                    }
                    return Request.CreateResponse(HttpStatusCode.OK, res);
                }
                catch (Exception ex)
                {
                    res = new OrderDispatchedListDtoObj()
                    {
                        OrderDispatchedObj = null,
                        status = false,
                        Message = "Failed. " + ex.Message + ""
                    };
                    return Request.CreateResponse(HttpStatusCode.BadRequest, res);
                }
            }
        }
        #endregion

        #endregion

        #region My Task Api V1 By Harry
        /// <summary>
        /// Created by 15-05-2019
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Route("MyTask/V1")]
        [HttpGet]
        public HttpResponseMessage getAppOrdersV1(int id) //get orders for delivery for task Order
        {
            OrderDispatchedListDtoObj res;
            using (var db = new AuthContext())
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
                    People People = db.Peoples.Where(x => x.PeopleID == id).FirstOrDefault();
                    var DBoyorders = db.getAcceptedOrdersV1(People.Mobile);
                    if (DBoyorders.Count == 0)
                    {
                        res = new OrderDispatchedListDtoObj()
                        {
                            OrderDispatchedObj = null,
                            status = false,
                            Message = "No Data exist."
                        };
                    }
                    else
                    {
                        res = new OrderDispatchedListDtoObj()
                        {
                            OrderDispatchedObj = DBoyorders,
                            status = true,
                            Message = "Success"
                        };
                    }
                    return Request.CreateResponse(HttpStatusCode.OK, res);
                }
                catch (Exception ex)
                {
                    res = new OrderDispatchedListDtoObj()
                    {
                        OrderDispatchedObj = null,
                        status = false,
                        Message = "Failed. " + ex.Message + ""
                    };
                    return Request.CreateResponse(HttpStatusCode.BadRequest, res);
                }
            }
        }
        #endregion


        #region post Order Place for New V2 By Anushka 3 Jan 2020 
        [AllowAnonymous]
        [Route("PostOrder/V2")]
        [HttpPost]
        public HttpResponseMessage PostOrderV2(OrderPlaceDTO obj) //Order delivered or canceled from delivery app
        {
            var identity = User.Identity as ClaimsIdentity;
            int userid = 0;

            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
            ResDTO res;
            if (ordersToProcess.Any(x => x == obj.OrderId))
            {
                res = new ResDTO()
                {
                    op = null,
                    Message = "Order #: " + obj.OrderId + " is already in process..",
                    Status = false
                };
                return Request.CreateResponse(HttpStatusCode.OK, res);
            }
            else
            {
                ordersToProcess.Add(obj.OrderId);
            }
            try
            {
                TripPlannerHelper tripPlannerHelper = new TripPlannerHelper();
                var msg = "";
                bool Status = false;
                bool IsDeliverSmsSent = false;
                List<string> chequenos = new List<string>();
                double totalChequeamt = 0;
                List<string> MposGatId = new List<string>();
                double totalMposamount = 0;
                List<string> BankNames = new List<string>();
                Guid guid = Guid.NewGuid();
                var guidData = guid.ToString();
                TransactionOptions option = new TransactionOptions();
                option.IsolationLevel = IsolationLevel.RepeatableRead;
                option.Timeout = TimeSpan.FromSeconds(90);
                using (var dbContextTransaction = new TransactionScope(TransactionScopeOption.Required, option))
                {
                    using (var context = new AuthContext())
                    {
                        OrderMaster ODMaster = null;
                        var People = context.Peoples.Where(x => x.Mobile == obj.DboyMobileNo).FirstOrDefault();
                        var DeliveryIssuance = context.DeliveryIssuanceDb.Where(x => x.DeliveryIssuanceId == obj.DeliveryIssuanceId).SingleOrDefault();
                        if (DeliveryIssuance.Status == "Accepted")
                        {
                            DeliveryIssuance.Status = "Pending";
                            DeliveryIssuance.UpdatedDate = indianTime;
                            context.Entry(DeliveryIssuance).State = EntityState.Modified;
                        }
                        else if (DeliveryIssuance.Status == "Pending")
                        {
                        }
                        else
                        {
                            Status = false;
                            res = new ResDTO()
                            {
                                op = null,
                                Message = "Not Delivered due to Assignment in status : " + DeliveryIssuance.Status,
                                Status = Status
                            };
                            ordersToProcess.RemoveAll(x => x == obj.OrderId);
                            return Request.CreateResponse(HttpStatusCode.OK, res);
                        }
                        var ODM = context.OrderDispatchedMasters.Where(x => x.OrderDispatchedMasterId == obj.OrderDispatchedMasterId).Include("orderDetails").SingleOrDefault();
                        ODMaster = context.DbOrderMaster.Where(x => x.OrderId == obj.OrderId).Include("orderDetails").SingleOrDefault();
                        if (obj != null && People != null && DeliveryIssuance.DeliveryIssuanceId > 0 && ODM != null)
                        {
                            //var ODM = context.OrderDispatchedMasters.Where(x => x.OrderDispatchedMasterId == obj.OrderDispatchedMasterId).Include("orderDetails").SingleOrDefault();
                            var OrderDeliveryMaster = context.OrderDeliveryMasterDB.Where(z => z.DeliveryIssuanceId == obj.DeliveryIssuanceId && z.OrderId == obj.OrderId).FirstOrDefault();
                            var AssignmentRechangeOrder = context.AssignmentRechangeOrder.FirstOrDefault(x => x.AssignmentId == obj.DeliveryIssuanceId && x.OrderId == obj.OrderId);
                            var Ordercancellation = context.DeptOrderCancellationDb.Where(o => o.OrderId == obj.OrderId).FirstOrDefault();
                            DeptOrderCancellation deptOrderCacellation = new DeptOrderCancellation();
                            deptOrderCacellation.OrderId = obj.OrderId;

                            if (AssignmentRechangeOrder != null)
                            {
                                AssignmentRechangeOrder.Status = 0;
                                AssignmentRechangeOrder.ModifiedDate = indianTime;
                                AssignmentRechangeOrder.ModifiedBy = People.PeopleID;
                                context.Entry(AssignmentRechangeOrder).State = EntityState.Modified;
                            }

                            #region Damage Order status Update
                            if (ODMaster.OrderType == 6 && ODM.invoice_no != null)
                            {
                                var DOM = context.DamageOrderMasterDB.Where(x => x.invoice_no == ODM.invoice_no).SingleOrDefault();

                                if (DOM != null)
                                {
                                    DOM.UpdatedDate = indianTime;
                                    DOM.Status = obj.Status;
                                    context.Entry(DOM).State = EntityState.Modified;
                                }
                            }
                            #endregion

                            if (obj.Status == "Delivered" && ODM.Status != "Delivered")
                            {
                                if (ODM.CustomerId > 0 && obj.CashAmount > 0)
                                {
                                    bool CheckCashAmount = tripPlannerHelper.CheckTodayCustomerCashAmount(ODM.CustomerId, obj.CashAmount, null, context);
                                    if (CheckCashAmount)
                                    {
                                        Status = false;
                                        msg = "Alert! You cannot exceed the cash limit of 2 lacs for this customer in a day.";
                                        res = new ResDTO()
                                        {
                                            op = obj,
                                            Message = msg,
                                            Status = Status
                                        };
                                        ordersToProcess.RemoveAll(x => x == obj.OrderId);
                                        return Request.CreateResponse(HttpStatusCode.OK, res);
                                    }
                                }
                                List<VANTransactionsDC> VANTransactionList = new List<VANTransactionsDC>();
                                if (ODM.CustomerId > 0)
                                {
                                    var customerId = new SqlParameter("@CustomerId", ODM.CustomerId);
                                    VANTransactionList = context.Database.SqlQuery<VANTransactionsDC>("EXEC GetCustomerVANTransactions @CustomerId", customerId).ToList();
                                }
                                var PaymentResponseRetailerAppList = context.PaymentResponseRetailerAppDb.Where(z => z.OrderId == obj.OrderId && z.status == "Success").ToList();
                                var RTGS_NEFTOldEntries = PaymentResponseRetailerAppList.Where(z => z.OrderId == obj.OrderId && z.PaymentFrom == "RTGS/NEFT" && z.status == "Success" && string.IsNullOrEmpty(z.GatewayTransId)).ToList();
                                if (RTGS_NEFTOldEntries != null && RTGS_NEFTOldEntries.Any())
                                {
                                    foreach (var RTGS in RTGS_NEFTOldEntries)
                                    {
                                        RTGS.status = "Failed";
                                        RTGS.statusDesc = "Due to double RTGS_NEFT request from DeliveryApp";
                                        context.Entry(RTGS).State = EntityState.Modified;
                                    }
                                }
                                #region Payments 
                                foreach (var orderdata in obj.DeliveryPayments)
                                {
                                    var ChequeOldEntries = PaymentResponseRetailerAppList.Where(z => z.OrderId == obj.OrderId && z.PaymentFrom == "Cheque" && z.status == "Success").ToList();
                                    if (ChequeOldEntries != null && ChequeOldEntries.Any())
                                    {
                                        foreach (var Cheque in ChequeOldEntries)
                                        {
                                            Cheque.status = "Failed";
                                            Cheque.statusDesc = "Due to double Cheque request from DeliveryApp";
                                            context.Entry(Cheque).State = EntityState.Modified;
                                        }
                                    }
                                    if (orderdata.PaymentFrom == "Cheque" && orderdata.amount > 0)
                                    {
                                        context.PaymentResponseRetailerAppDb.Add(new PaymentResponseRetailerApp
                                        {
                                            OrderId = obj.OrderId,
                                            status = "Success",
                                            CreatedDate = indianTime,
                                            UpdatedDate = indianTime,
                                            PaymentFrom = "Cheque",
                                            statusDesc = "Due to Delivery",
                                            amount = Math.Round(orderdata.amount, 0),
                                            GatewayTransId = orderdata.TransId,
                                            ChequeBankName = orderdata.ChequeBankName,
                                            ChequeImageUrl = orderdata.ChequeImageUrl
                                        });

                                        chequenos.Add(orderdata.TransId);
                                        totalChequeamt += Math.Round(orderdata.amount, 0);
                                    }
                                    if (orderdata.PaymentFrom == "mPos" && orderdata.amount > 0)
                                    {
                                        context.PaymentResponseRetailerAppDb.Add(new PaymentResponseRetailerApp
                                        {
                                            OrderId = obj.OrderId,
                                            status = "Success",
                                            CreatedDate = indianTime,
                                            UpdatedDate = indianTime,
                                            PaymentFrom = "mPos",
                                            statusDesc = "Due to Delivery",
                                            amount = Math.Round(orderdata.amount, 0),
                                            GatewayTransId = orderdata.TransId,
                                            IsOnline = true
                                        });
                                        MposGatId.Add(orderdata.TransId);
                                        totalMposamount += Math.Round(orderdata.amount, 0);
                                    }
                                    if (orderdata.PaymentFrom == "RTGS/NEFT" && orderdata.amount > 0)
                                    {
                                        if (orderdata.IsVAN_RTGSNEFT)
                                        {
                                            double totalPaybleAmount = orderdata.amount;
                                            foreach (var paymant in VANTransactionList.Where(x => Math.Round(x.Amount, 0) > x.UsedAmount).OrderBy(x => x.Id))
                                            {
                                                double RemainingAmount = 0;
                                                double PaidAmount = 0;
                                                RemainingAmount = Math.Round(paymant.Amount, 0) - paymant.UsedAmount;
                                                if (totalPaybleAmount == 0)
                                                {
                                                    break;
                                                }
                                                if (totalPaybleAmount <= RemainingAmount)
                                                {
                                                    paymant.UsedAmount += totalPaybleAmount;
                                                    PaidAmount = totalPaybleAmount;
                                                    totalPaybleAmount = 0;
                                                }
                                                else
                                                {
                                                    totalPaybleAmount -= RemainingAmount;
                                                    paymant.UsedAmount += RemainingAmount;
                                                    PaidAmount = RemainingAmount;
                                                }
                                                bool VANSettled = tripPlannerHelper.CheckVANSettled(paymant.UserReferenceNumber, context);
                                                context.PaymentResponseRetailerAppDb.Add(new PaymentResponseRetailerApp
                                                {
                                                    OrderId = obj.OrderId,
                                                    status = "Success",
                                                    CreatedDate = indianTime,
                                                    UpdatedDate = indianTime,
                                                    PaymentFrom = "RTGS/NEFT",
                                                    statusDesc = "Due to Delivery",
                                                    amount = Math.Round(PaidAmount, 0),
                                                    GatewayTransId = paymant.UserReferenceNumber,
                                                    IsOnline = true
                                                });
                                                context.VANTransactiones.Add(new VANTransaction
                                                {
                                                    Amount = PaidAmount * -1,
                                                    ObjectType = "Order",
                                                    ObjectId = obj.OrderId,
                                                    CustomerId = ODM.CustomerId,
                                                    CreatedDate = DateTime.Now,
                                                    IsActive = true,
                                                    IsDeleted = false,
                                                    CreatedBy = userid,
                                                    Comment = "OrderId : " + obj.OrderId,
                                                    UsedAmount = PaidAmount,
                                                    VANTransactionParentId = paymant.Id,
                                                    IsSettled = VANSettled == false ? false : true,
                                                    Settledby = VANSettled == false ? 0 : userid,
                                                    SettledDate = VANSettled == false ? null : (DateTime?)DateTime.Now
                                                });

                                                var vANTransactiones = context.VANTransactiones.Where(x => x.Id == paymant.Id).FirstOrDefault();
                                                if (vANTransactiones != null)
                                                {
                                                    vANTransactiones.UsedAmount += PaidAmount;
                                                    context.Entry(vANTransactiones).State = EntityState.Modified;
                                                }
                                                totalMposamount += Math.Round(PaidAmount, 0);
                                            }
                                        }
                                        else
                                        {
                                            context.PaymentResponseRetailerAppDb.Add(new PaymentResponseRetailerApp
                                            {
                                                OrderId = obj.OrderId,
                                                status = "Success",
                                                CreatedDate = indianTime,
                                                UpdatedDate = indianTime,
                                                PaymentFrom = "RTGS/NEFT",
                                                statusDesc = "Due to Delivery",
                                                amount = Math.Round(orderdata.amount, 0),
                                                GatewayTransId = orderdata.TransId,
                                                IsOnline = true
                                            });
                                            MposGatId.Add(orderdata.TransId);
                                            totalMposamount += Math.Round(orderdata.amount, 0);
                                        }
                                    }
                                }

                                #region refund
                                var OnlinePaymentResponseRetailer = new List<RetailerOrderPaymentDc>();
                                foreach (var item in PaymentResponseRetailerAppList.Where(x => x.IsOnline == true).GroupBy(x => new { x.OrderId, x.PaymentFrom, x.GatewayTransId, x.IsOnline, x.status }))
                                {
                                    OnlinePaymentResponseRetailer.Add(new RetailerOrderPaymentDc
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
                                var othermodeAmt = OnlinePaymentResponseRetailer.Where(x => x.IsOnline).Sum(x => x.amount);
                                #endregion
                                //var othermodeAmt = PaymentResponseRetailerAppList.Where(x => x.IsOnline).Sum(x => x.amount);

                                var totalAmount = totalChequeamt + totalMposamount + othermodeAmt;
                                if (ODM.GrossAmount != obj.CashAmount + totalAmount)
                                {
                                    obj.CashAmount = ODM.GrossAmount - totalAmount;
                                }
                                var cashpayment = PaymentResponseRetailerAppList.FirstOrDefault(x => x.OrderId == obj.OrderId && x.status == "Success" && x.PaymentFrom == "Cash");
                                if (cashpayment != null)
                                {
                                    cashpayment.amount = obj.CashAmount;
                                    cashpayment.UpdatedDate = indianTime;
                                    cashpayment.status = obj.CashAmount > 0 ? cashpayment.status : "Failed";
                                    cashpayment.statusDesc = "Due to Delivery";
                                    context.Entry(cashpayment).State = EntityState.Modified;
                                }
                                else if (cashpayment == null && obj.CashAmount > 0)
                                {
                                    context.PaymentResponseRetailerAppDb.Add(new PaymentResponseRetailerApp
                                    {
                                        OrderId = obj.OrderId,
                                        status = "Success",
                                        CreatedDate = indianTime,
                                        UpdatedDate = indianTime,
                                        statusDesc = "Due to Delivery",
                                        PaymentFrom = "Cash",
                                        amount = Math.Round(obj.CashAmount, 0)
                                    });
                                }

                                #endregion
                                #region OrderDeliveryMaster
                                OrderDeliveryMaster.Status = obj.Status;
                                OrderDeliveryMaster.comments = obj.comments;
                                OrderDeliveryMaster.RecivedAmount = obj.RecivedAmount;
                                OrderDeliveryMaster.UpdatedDate = indianTime;
                                OrderDeliveryMaster.DeliveryLat = obj.DeliveryLat;
                                OrderDeliveryMaster.DeliveryLng = obj.DeliveryLng; //added on 08/07/02019                              
                                context.Entry(OrderDeliveryMaster).State = EntityState.Modified;
                                #endregion

                                ODM.Status = obj.Status;
                                ODM.ReDispatchedStatus = obj.Status;
                                ODM.comments = obj.comments;

                                ODM.RecivedAmount = obj.RecivedAmount;
                                ODM.Signimg = obj.Signimg;
                                ODM.UpdatedDate = indianTime;


                                ODM.DeliveryLat = obj.DeliveryLat;//added on 08/07/02019 
                                ODM.DeliveryLng = obj.DeliveryLng;
                                context.Entry(ODM).State = EntityState.Modified;

                                //#endregion
                                #region Order Master History for Status Delivered

                                OrderMasterHistories OrderMasterHistories = new OrderMasterHistories();
                                OrderMasterHistories.orderid = ODM.OrderId;
                                OrderMasterHistories.Status = ODM.Status;
                                OrderMasterHistories.Reasoncancel = null;
                                OrderMasterHistories.Warehousename = ODM.WarehouseName;
                                OrderMasterHistories.DeliveryIssuanceId = obj.DeliveryIssuanceId;//by sudhir 06-06-2019
                                OrderMasterHistories.username = People.DisplayName != null ? People.DisplayName : People.PeopleFirstName; ;
                                OrderMasterHistories.userid = People.PeopleID;
                                OrderMasterHistories.CreatedDate = indianTime;
                                context.OrderMasterHistoriesDB.Add(OrderMasterHistories);
                                #endregion

                                ODMaster.Status = "Delivered";
                                ODMaster.comments = obj.comments;
                                ODMaster.DeliveredDate = indianTime;
                                ODMaster.UpdatedDate = indianTime;
                                context.Entry(ODMaster).State = EntityState.Modified;
                                IsDeliverSmsSent = true;
                                foreach (var detail in ODMaster.orderDetails)
                                {
                                    detail.Status = obj.Status;
                                    detail.UpdatedDate = indianTime;
                                    context.Entry(detail).State = EntityState.Modified;
                                }
                                #region  for Franchises
                                if (context.Customers.Any(x => x.CustomerId == ODM.CustomerId && x.IsFranchise == true))
                                {
                                    var DeliveredOrderToFranchisesdb = context.DeliveredOrderToFranchises.Where(x => x.OrderId == ODM.OrderId).FirstOrDefault();
                                    if (DeliveredOrderToFranchisesdb == null)
                                    {
                                        DeliveredOrderToFranchise FDB = new DeliveredOrderToFranchise();
                                        FDB.OrderId = ODM.OrderId;
                                        FDB.CreatedDate = indianTime;
                                        FDB.IsProcessed = false;
                                        context.DeliveredOrderToFranchises.Add(FDB);
                                    }
                                }
                                #endregion
                                #region New Delivery Optimization process
                                if (obj.DeliveryOptimizationdc != null)
                                {
                                    var tripPlannerConfirmedOrderId = new SqlParameter("@TripPlannerConfirmedOrderId", obj.DeliveryOptimizationdc.TripPlannerConfirmedOrderId);
                                    long tripPlannerConfirmedDetailId = context.Database.SqlQuery<long>("GetTripPlannerConfirmedDetailId @TripPlannerConfirmedOrderId", tripPlannerConfirmedOrderId).FirstOrDefault();
                                    var tripPlannerConfirmedDetails = context.TripPlannerConfirmedDetails.Where(x => x.Id == tripPlannerConfirmedDetailId).FirstOrDefault();
                                    var orderids = tripPlannerConfirmedDetails.CommaSeparatedOrderList.Split(',').Select(Int32.Parse).ToList();
                                    var orderDispatchedMasters = context.OrderDispatchedMasters.Where(x => orderids.Contains(x.OrderId)).ToList();
                                    bool IsShipped = true;
                                    if (orderDispatchedMasters.Any() && orderDispatchedMasters != null)
                                    {
                                        foreach (var item in orderDispatchedMasters)
                                        {
                                            if (item.Status == "Shipped")
                                            {
                                                IsShipped = false;
                                            }
                                        }
                                        tripPlannerConfirmedDetails.IsProcess = IsShipped;
                                        context.Entry(tripPlannerConfirmedDetails).State = EntityState.Modified;
                                        var tripPlannerVehicle = context.TripPlannerVehicleDb.Where(x => x.TripPlannerConfirmedMasterId == tripPlannerConfirmedDetails.TripPlannerConfirmedMasterId).FirstOrDefault();
                                        if (tripPlannerVehicle != null)
                                        {
                                            var tripPlannerConfirmedOrders = context.TripPlannerConfirmedOrders.Where(x => orderids.Contains((int)x.OrderId) && x.TripPlannerConfirmedDetailId == tripPlannerConfirmedDetailId && x.IsActive == true && x.IsDeleted == false).ToList();
                                            if (tripPlannerConfirmedDetails.IsProcess)
                                            {
                                                tripPlannerVehicle.CurrentStatus = (int)VehicleliveStatus.InTransit;
                                                tripPlannerVehicle.ModifiedDate = indianTime;
                                                tripPlannerVehicle.ModifiedBy = userid;
                                                tripPlannerVehicle.CurrentLat = obj.DeliveryOptimizationdc.lat;
                                                tripPlannerVehicle.CurrentLng = obj.DeliveryOptimizationdc.lng;
                                                tripPlannerVehicle.ReminingTime -= (tripPlannerConfirmedDetails.TotalTimeInMins - tripPlannerConfirmedOrders.Sum(x => x.TimeInMins)) + (tripPlannerConfirmedOrders.Where(x => x.OrderId == obj.OrderId).FirstOrDefault().TimeInMins);
                                                tripPlannerVehicle.TravelTime += (tripPlannerConfirmedDetails.TotalTimeInMins - tripPlannerConfirmedOrders.Sum(x => x.TimeInMins)) + (tripPlannerConfirmedOrders.Where(x => x.OrderId == obj.OrderId).FirstOrDefault().TimeInMins);
                                                tripPlannerVehicle.DistanceLeft -= tripPlannerConfirmedDetails.TotalDistanceInMeter;
                                                tripPlannerVehicle.DistanceTraveled += tripPlannerConfirmedDetails.TotalDistanceInMeter;
                                                /////////////////////
                                                context.Entry(tripPlannerVehicle).State = EntityState.Modified;
                                                TripPlannerVehicleHistory tripPlannerVehicleHistory = new TripPlannerVehicleHistory();
                                                tripPlannerVehicleHistory.TripPlannerVehicleId = tripPlannerVehicle.Id;
                                                tripPlannerVehicleHistory.CurrentServingOrderId = obj.DeliveryOptimizationdc.OrderId;
                                                tripPlannerVehicleHistory.RecordTime = indianTime;
                                                tripPlannerVehicleHistory.Lat = obj.DeliveryOptimizationdc.lat;
                                                tripPlannerVehicleHistory.Lng = obj.DeliveryOptimizationdc.lng;
                                                tripPlannerVehicleHistory.CreatedDate = indianTime;
                                                tripPlannerVehicleHistory.CreatedBy = userid;
                                                tripPlannerVehicleHistory.IsActive = true;
                                                tripPlannerVehicleHistory.IsDeleted = false;
                                                tripPlannerVehicleHistory.RecoardStatus = (int)VehicleliveStatus.InTransit;
                                                tripPlannerVehicleHistory.TripPlannerConfirmedDetailId = tripPlannerConfirmedDetails.Id;
                                                context.TripPlannerVehicleHistoryDb.Add(tripPlannerVehicleHistory);
                                            }
                                            else
                                            {
                                                tripPlannerVehicle.CurrentStatus = (int)VehicleliveStatus.InTransit;
                                                tripPlannerVehicle.ModifiedDate = indianTime;
                                                tripPlannerVehicle.ModifiedBy = userid;
                                                tripPlannerVehicle.CurrentLat = obj.DeliveryOptimizationdc.lat;
                                                tripPlannerVehicle.CurrentLng = obj.DeliveryOptimizationdc.lng;
                                                tripPlannerVehicle.ReminingTime -= tripPlannerConfirmedOrders.Where(x => x.OrderId == obj.OrderId).Select(x => x.TimeInMins).FirstOrDefault();
                                                tripPlannerVehicle.TravelTime += tripPlannerConfirmedOrders.Where(x => x.OrderId == obj.OrderId).Select(x => x.TimeInMins).FirstOrDefault();
                                                context.Entry(tripPlannerVehicle).State = EntityState.Modified;
                                                TripPlannerVehicleHistory tripPlannerVehicleHistory = new TripPlannerVehicleHistory();
                                                tripPlannerVehicleHistory.TripPlannerVehicleId = tripPlannerVehicle.Id;
                                                tripPlannerVehicleHistory.CurrentServingOrderId = obj.DeliveryOptimizationdc.OrderId;
                                                tripPlannerVehicleHistory.RecordTime = indianTime;
                                                tripPlannerVehicleHistory.Lat = obj.DeliveryOptimizationdc.lat;
                                                tripPlannerVehicleHistory.Lng = obj.DeliveryOptimizationdc.lng;
                                                tripPlannerVehicleHistory.CreatedDate = indianTime;
                                                tripPlannerVehicleHistory.CreatedBy = userid;
                                                tripPlannerVehicleHistory.IsActive = true;
                                                tripPlannerVehicleHistory.IsDeleted = false;
                                                tripPlannerVehicleHistory.RecoardStatus = (int)VehicleliveStatus.InTransit;
                                                tripPlannerVehicleHistory.TripPlannerConfirmedDetailId = tripPlannerConfirmedDetails.Id;
                                                context.TripPlannerVehicleHistoryDb.Add(tripPlannerVehicleHistory);
                                            }
                                        }
                                    }
                                }
                                #endregion
                                #region OrderConcern      
                                string OrderConcernMessageFlag = Convert.ToString(ConfigurationManager.AppSettings["OrderConcernMessageFlag"]);
                                if (!string.IsNullOrEmpty(OrderConcernMessageFlag) && OrderConcernMessageFlag == "Y")
                                {
                                    context.OrderConcernDB.Add(new Model.CustomerDelight.OrderConcern()
                                    {
                                        IsActive = true,
                                        IsDeleted = false,
                                        CreatedDate = DateTime.Now,
                                        LinkId = guidData,
                                        OrderId = ODMaster.OrderId,
                                        CreatedBy = userid,
                                        //Status = "Open",
                                        IsCustomerRaiseConcern = false
                                    });
                                }
                                #endregion
                            }
                            else if (ODM.Status == "Delivered")
                            {
                                Status = true;
                                msg = "Order already Delivered";
                                res = new ResDTO()
                                {
                                    op = obj,
                                    Message = msg,
                                    Status = Status
                                };
                                ordersToProcess.RemoveAll(x => x == obj.OrderId);
                                return Request.CreateResponse(HttpStatusCode.OK, res);
                            }
                            else if (obj.Status == "Delivery Canceled" && ODM.Status != "Delivery Canceled")
                            {
                                OrderDeliveryMaster.Status = obj.Status;
                                OrderDeliveryMaster.comments = obj.comments;
                                //OrderDeliveryMaster.ChequeDate = obj.ChequeDate;
                                OrderDeliveryMaster.UpdatedDate = indianTime;
                                OrderDeliveryMaster.DeliveryLat = obj.DeliveryLat;//added on 08/07/02019 
                                OrderDeliveryMaster.DeliveryLng = obj.DeliveryLng;
                                context.Entry(OrderDeliveryMaster).State = EntityState.Modified;
                                //done on 28/02/2020 by PoojaZ //start
                                if (OrderDeliveryMaster.comments == "Dont Want" || OrderDeliveryMaster.comments == "Late Delivery (OPS)" || OrderDeliveryMaster.comments == "Damage Expiry (Pur)")
                                {
                                    string query = "select sp.PeopleID  from OrderMasters a inner join Customers c on a.CustomerId=c.CustomerId and a.orderid = " + OrderDeliveryMaster.OrderId + "  inner join People sp on a.SalesPersonId=sp.PeopleID";
                                    var salesLeadId = context.Database.SqlQuery<int?>(query).FirstOrDefault();
                                    if (salesLeadId.HasValue && salesLeadId.Value > 0)
                                    {
                                        deptOrderCacellation.SalesLeadId = salesLeadId.Value;
                                        deptOrderCacellation.OrderId = OrderDeliveryMaster.OrderId;
                                        deptOrderCacellation.ChargePoint = 1;
                                        deptOrderCacellation.IsActive = true;
                                        deptOrderCacellation.IsDeleted = false;
                                        deptOrderCacellation.IsEmailSend = false;
                                        deptOrderCacellation.CreatedDate = DateTime.Now;
                                        deptOrderCacellation.CreatedBy = People.PeopleID;
                                        if (OrderDeliveryMaster.comments == "Dont Want")
                                        {
                                            deptOrderCacellation.DepId = 6;
                                        }
                                        else if (OrderDeliveryMaster.comments == "Late Delivery (OPS)")
                                        {
                                            deptOrderCacellation.DepId = 33;
                                        }
                                        else if (OrderDeliveryMaster.comments == "Damage Expiry (Pur)")
                                        {
                                            deptOrderCacellation.DepId = 29;
                                        }
                                        context.DeptOrderCancellationDb.Add(deptOrderCacellation);
                                    }
                                }
                                //done on 28/02/2020 by PoojaZ //end

                                if (ODM != null)
                                {
                                    ODM.Status = obj.Status;
                                    ODM.CanceledStatus = obj.Status;
                                    ODM.comments = obj.comments;
                                    ODM.Signimg = obj.Signimg;
                                    ODM.UpdatedDate = indianTime;
                                    ODM.DeliveryLat = obj.DeliveryLat;//added on 08/07/02019 
                                    ODM.DeliveryLng = obj.DeliveryLng;
                                    context.Entry(ODM).State = EntityState.Modified;

                                    //pz//start

                                    if (OrderDeliveryMaster.comments == "Dont Want" || OrderDeliveryMaster.comments == "Price Issue (sales)")
                                    {
                                        if (!context.CustomerWalletHistoryDb.Any(x => x.OrderId == ODM.OrderId && x.NewOutWAmount == -100 && x.Through == "From Order Cancelled"))
                                        {
                                            Wallet wlt = context.WalletDb.Where(c => c.CustomerId == ODM.CustomerId).SingleOrDefault();

                                            CustomerWalletHistory CWH = new CustomerWalletHistory();
                                            CWH.WarehouseId = ODM.WarehouseId;
                                            CWH.CompanyId = ODM.CompanyId;
                                            CWH.CustomerId = wlt.CustomerId;
                                            CWH.NewAddedWAmount = 0;
                                            CWH.NewOutWAmount = -100;
                                            CWH.OrderId = ODM.OrderId;
                                            CWH.Through = "From Order Cancelled";
                                            CWH.TotalWalletAmount = wlt.TotalAmount - 100;
                                            CWH.CreatedDate = indianTime;
                                            CWH.UpdatedDate = indianTime;
                                            context.CustomerWalletHistoryDb.Add(CWH);

                                            wlt.TotalAmount -= 100;
                                            wlt.TransactionDate = indianTime;
                                            context.Entry(wlt).State = EntityState.Modified;
                                        }
                                    }
                                    ///pz///             

                                    #region Order Master History for Status Delivery Canceled
                                    OrderMasterHistories OrderMasterHistories = new OrderMasterHistories();
                                    OrderMasterHistories.orderid = ODM.OrderId;
                                    OrderMasterHistories.Status = ODM.Status;
                                    OrderMasterHistories.Reasoncancel = null;
                                    OrderMasterHistories.Warehousename = ODM.WarehouseName;
                                    OrderMasterHistories.DeliveryIssuanceId = obj.DeliveryIssuanceId;
                                    OrderMasterHistories.username = People.DisplayName != null ? People.DisplayName : People.PeopleFirstName; ;
                                    OrderMasterHistories.userid = People.PeopleID;
                                    OrderMasterHistories.CreatedDate = DateTime.Now;
                                    context.OrderMasterHistoriesDB.Add(OrderMasterHistories);
                                    DeliveryCanceledRequestHistory DeliveryCanceledRequestHistoryAdd = new DeliveryCanceledRequestHistory();
                                    DeliveryCanceledRequestHistoryAdd.OrderId = ODM.OrderId;
                                    DeliveryCanceledRequestHistoryAdd.Comments = obj.CancelrequestComments;
                                    DeliveryCanceledRequestHistoryAdd.CreatedBy = userid;
                                    DeliveryCanceledRequestHistoryAdd.CreatedDate = DateTime.Now;
                                    DeliveryCanceledRequestHistoryAdd.IsActive = true;
                                    DeliveryCanceledRequestHistoryAdd.IsDeleted = false;
                                    context.DeliveryCanceledRequestHistoryDb.Add(DeliveryCanceledRequestHistoryAdd);
                                    #endregion
                                    //foreach (var detail in ODM.orderDetails)
                                    //{
                                    //    detail.Status = obj.Status;
                                    //    detail.UpdatedDate = indianTime;
                                    //    context.Entry(detail).State = EntityState.Modified;
                                    //}
                                    ODMaster.Status = obj.Status;
                                    ODMaster.comments = obj.comments;
                                    ODMaster.UpdatedDate = indianTime;
                                    context.Entry(ODMaster).State = EntityState.Modified;
                                    foreach (var detail in ODMaster.orderDetails)
                                    {
                                        detail.Status = obj.Status;
                                        detail.UpdatedDate = indianTime;
                                        context.Entry(detail).State = EntityState.Modified;
                                    }

                                }
                                #region New Delivery Optimization process
                                if (obj.DeliveryOptimizationdc != null)
                                {
                                    var tripPlannerConfirmedOrderId = new SqlParameter("@TripPlannerConfirmedOrderId", obj.DeliveryOptimizationdc.TripPlannerConfirmedOrderId);
                                    long tripPlannerConfirmedDetailId = context.Database.SqlQuery<long>("GetTripPlannerConfirmedDetailId @TripPlannerConfirmedOrderId", tripPlannerConfirmedOrderId).FirstOrDefault();
                                    var tripPlannerConfirmedDetails = context.TripPlannerConfirmedDetails.Where(x => x.Id == tripPlannerConfirmedDetailId).FirstOrDefault();
                                    var orderids = tripPlannerConfirmedDetails.CommaSeparatedOrderList.Split(',').Select(Int32.Parse).ToList();
                                    var orderDispatchedMasters = context.OrderDispatchedMasters.Where(x => orderids.Contains(x.OrderId)).ToList();
                                    bool IsShipped = true;
                                    if (orderDispatchedMasters.Any() && orderDispatchedMasters != null)
                                    {
                                        foreach (var item in orderDispatchedMasters)
                                        {
                                            if (item.Status == "Shipped")
                                            {
                                                IsShipped = false;
                                            }
                                        }
                                        tripPlannerConfirmedDetails.IsProcess = IsShipped;
                                        context.Entry(tripPlannerConfirmedDetails).State = EntityState.Modified;
                                        var tripPlannerVehicle = context.TripPlannerVehicleDb.Where(x => x.TripPlannerConfirmedMasterId == tripPlannerConfirmedDetails.TripPlannerConfirmedMasterId).FirstOrDefault();
                                        if (tripPlannerVehicle != null)
                                        {
                                            var tripPlannerConfirmedOrders = context.TripPlannerConfirmedOrders.Where(x => orderids.Contains((int)x.OrderId) && x.TripPlannerConfirmedDetailId == tripPlannerConfirmedDetailId && x.IsActive == true && x.IsDeleted == false).ToList();
                                            if (tripPlannerConfirmedDetails.IsProcess)
                                            {
                                                tripPlannerVehicle.CurrentStatus = (int)VehicleliveStatus.InTransit;
                                                tripPlannerVehicle.ModifiedDate = indianTime;
                                                tripPlannerVehicle.ModifiedBy = userid;
                                                tripPlannerVehicle.CurrentLat = obj.DeliveryOptimizationdc.lat;
                                                tripPlannerVehicle.CurrentLng = obj.DeliveryOptimizationdc.lng;
                                                tripPlannerVehicle.ReminingTime -= (tripPlannerConfirmedDetails.TotalTimeInMins - tripPlannerConfirmedOrders.Sum(x => x.TimeInMins)) + (tripPlannerConfirmedOrders.Where(x => x.OrderId == obj.OrderId).FirstOrDefault().TimeInMins);
                                                tripPlannerVehicle.TravelTime += (tripPlannerConfirmedDetails.TotalTimeInMins - tripPlannerConfirmedOrders.Sum(x => x.TimeInMins)) + (tripPlannerConfirmedOrders.Where(x => x.OrderId == obj.OrderId).FirstOrDefault().TimeInMins);
                                                tripPlannerVehicle.DistanceLeft -= tripPlannerConfirmedDetails.TotalDistanceInMeter;
                                                tripPlannerVehicle.DistanceTraveled += tripPlannerConfirmedDetails.TotalDistanceInMeter;
                                                /////////////////////
                                                context.Entry(tripPlannerVehicle).State = EntityState.Modified;
                                                TripPlannerVehicleHistory tripPlannerVehicleHistory = new TripPlannerVehicleHistory();
                                                tripPlannerVehicleHistory.TripPlannerVehicleId = tripPlannerVehicle.Id;
                                                tripPlannerVehicleHistory.CurrentServingOrderId = obj.DeliveryOptimizationdc.OrderId;
                                                tripPlannerVehicleHistory.RecordTime = indianTime;
                                                tripPlannerVehicleHistory.Lat = obj.DeliveryOptimizationdc.lat;
                                                tripPlannerVehicleHistory.Lng = obj.DeliveryOptimizationdc.lng;
                                                tripPlannerVehicleHistory.CreatedDate = indianTime;
                                                tripPlannerVehicleHistory.CreatedBy = userid;
                                                tripPlannerVehicleHistory.IsActive = true;
                                                tripPlannerVehicleHistory.IsDeleted = false;
                                                tripPlannerVehicleHistory.RecoardStatus = (int)VehicleliveStatus.InTransit;
                                                tripPlannerVehicleHistory.TripPlannerConfirmedDetailId = tripPlannerConfirmedDetails.Id;
                                                context.TripPlannerVehicleHistoryDb.Add(tripPlannerVehicleHistory);
                                            }
                                            else
                                            {
                                                tripPlannerVehicle.CurrentStatus = (int)VehicleliveStatus.InTransit;
                                                tripPlannerVehicle.ModifiedDate = indianTime;
                                                tripPlannerVehicle.ModifiedBy = userid;
                                                tripPlannerVehicle.CurrentLat = obj.DeliveryOptimizationdc.lat;
                                                tripPlannerVehicle.CurrentLng = obj.DeliveryOptimizationdc.lng;
                                                tripPlannerVehicle.ReminingTime -= tripPlannerConfirmedOrders.Where(x => x.OrderId == obj.OrderId).Select(x => x.TimeInMins).FirstOrDefault();
                                                tripPlannerVehicle.TravelTime += tripPlannerConfirmedOrders.Where(x => x.OrderId == obj.OrderId).Select(x => x.TimeInMins).FirstOrDefault();
                                                context.Entry(tripPlannerVehicle).State = EntityState.Modified;
                                                TripPlannerVehicleHistory tripPlannerVehicleHistory = new TripPlannerVehicleHistory();
                                                tripPlannerVehicleHistory.TripPlannerVehicleId = tripPlannerVehicle.Id;
                                                tripPlannerVehicleHistory.CurrentServingOrderId = obj.DeliveryOptimizationdc.OrderId;
                                                tripPlannerVehicleHistory.RecordTime = indianTime;
                                                tripPlannerVehicleHistory.Lat = obj.DeliveryOptimizationdc.lat;
                                                tripPlannerVehicleHistory.Lng = obj.DeliveryOptimizationdc.lng;
                                                tripPlannerVehicleHistory.CreatedDate = indianTime;
                                                tripPlannerVehicleHistory.CreatedBy = userid;
                                                tripPlannerVehicleHistory.IsActive = true;
                                                tripPlannerVehicleHistory.IsDeleted = false;
                                                tripPlannerVehicleHistory.RecoardStatus = (int)VehicleliveStatus.InTransit;
                                                tripPlannerVehicleHistory.TripPlannerConfirmedDetailId = tripPlannerConfirmedDetails.Id;
                                                context.TripPlannerVehicleHistoryDb.Add(tripPlannerVehicleHistory);
                                            }
                                        }
                                    }
                                }
                                #endregion
                                //}//end dt:-9/3/2020
                            }
                            else if (ODM.Status == "Delivery Canceled")
                            {
                                Status = true;
                                msg = "Order already Delivery Canceled";
                                res = new ResDTO()
                                {
                                    op = obj,
                                    Message = msg,
                                    Status = Status
                                };
                                ordersToProcess.RemoveAll(x => x == obj.OrderId);

                                return Request.CreateResponse(HttpStatusCode.OK, res);
                            }
                            else if (obj.Status == "Delivery Canceled Request" && ODM.Status != "Delivery Canceled Request")
                            {
                                OrderDeliveryMaster.Status = obj.Status;
                                OrderDeliveryMaster.comments = obj.comments;
                                //OrderDeliveryMaster.ChequeDate = obj.ChequeDate;
                                OrderDeliveryMaster.UpdatedDate = indianTime;
                                OrderDeliveryMaster.DeliveryLat = obj.DeliveryLat;//added on 08/07/02019 
                                OrderDeliveryMaster.DeliveryLng = obj.DeliveryLng;
                                context.Entry(OrderDeliveryMaster).State = EntityState.Modified;
                                //done on 28/02/2020 by PoojaZ //start
                                if (OrderDeliveryMaster.comments == "Dont Want" || OrderDeliveryMaster.comments == "Late Delivery (OPS)" || OrderDeliveryMaster.comments == "Damage Expiry (Pur)")
                                {
                                    string query = "select sp.PeopleID  from OrderMasters a inner join Customers c on a.CustomerId=c.CustomerId and a.orderid = " + OrderDeliveryMaster.OrderId + "  inner join People sp on a.SalesPersonId=sp.PeopleID";
                                    var salesLeadId = context.Database.SqlQuery<int?>(query).FirstOrDefault();
                                    if (salesLeadId.HasValue && salesLeadId.Value > 0)
                                    {
                                        deptOrderCacellation.SalesLeadId = salesLeadId.Value;
                                        deptOrderCacellation.OrderId = OrderDeliveryMaster.OrderId;
                                        deptOrderCacellation.ChargePoint = 1;
                                        deptOrderCacellation.IsActive = true;
                                        deptOrderCacellation.IsDeleted = false;
                                        deptOrderCacellation.IsEmailSend = false;
                                        deptOrderCacellation.CreatedDate = DateTime.Now;
                                        deptOrderCacellation.CreatedBy = People.PeopleID;
                                        if (OrderDeliveryMaster.comments == "Dont Want")
                                        {
                                            deptOrderCacellation.DepId = 6;
                                        }
                                        else if (OrderDeliveryMaster.comments == "Late Delivery (OPS)")
                                        {
                                            deptOrderCacellation.DepId = 33;
                                        }
                                        else if (OrderDeliveryMaster.comments == "Damage Expiry (Pur)")
                                        {
                                            deptOrderCacellation.DepId = 29;
                                        }
                                        context.DeptOrderCancellationDb.Add(deptOrderCacellation);
                                    }
                                }
                                //done on 28/02/2020 by PoojaZ //end

                                if (ODM != null)
                                {
                                    ODM.Status = obj.Status;
                                    ODM.CanceledStatus = obj.Status;
                                    ODM.comments = obj.comments;
                                    ODM.Signimg = obj.Signimg;
                                    ODM.UpdatedDate = indianTime;
                                    ODM.DeliveryLat = obj.DeliveryLat;//added on 08/07/02019 
                                    ODM.DeliveryLng = obj.DeliveryLng;
                                    context.Entry(ODM).State = EntityState.Modified;
                                    #region wallet code comments
                                    ////pz//start

                                    //if (OrderDeliveryMaster.comments == "Dont Want" || OrderDeliveryMaster.comments == "Price Issue (sales)")
                                    //{
                                    //    if (!context.CustomerWalletHistoryDb.Any(x => x.OrderId == ODM.OrderId && x.NewOutWAmount == -100 && x.Through == "From Order Cancelled"))
                                    //    {
                                    //        Wallet wlt = context.WalletDb.Where(c => c.CustomerId == ODM.CustomerId).SingleOrDefault();

                                    //        CustomerWalletHistory CWH = new CustomerWalletHistory();
                                    //        CWH.WarehouseId = ODM.WarehouseId;
                                    //        CWH.CompanyId = ODM.CompanyId;
                                    //        CWH.CustomerId = wlt.CustomerId;
                                    //        CWH.NewAddedWAmount = 0;
                                    //        CWH.NewOutWAmount = -100;
                                    //        CWH.OrderId = ODM.OrderId;
                                    //        CWH.Through = "From Order Cancelled";
                                    //        CWH.TotalWalletAmount = wlt.TotalAmount - 100;
                                    //        CWH.CreatedDate = indianTime;
                                    //        CWH.UpdatedDate = indianTime;
                                    //        context.CustomerWalletHistoryDb.Add(CWH);

                                    //        wlt.TotalAmount -= 100;
                                    //        wlt.TransactionDate = indianTime;
                                    //        context.Entry(wlt).State = EntityState.Modified;
                                    //    }
                                    //}
                                    /////pz///             
                                    #endregion
                                    #region Order Master History for Status Delivery Canceled
                                    OrderMasterHistories OrderMasterHistories = new OrderMasterHistories();
                                    OrderMasterHistories.orderid = ODM.OrderId;
                                    OrderMasterHistories.Status = ODM.Status;
                                    OrderMasterHistories.Reasoncancel = null;
                                    OrderMasterHistories.Warehousename = ODM.WarehouseName;
                                    OrderMasterHistories.DeliveryIssuanceId = obj.DeliveryIssuanceId;
                                    OrderMasterHistories.username = People.DisplayName != null ? People.DisplayName : People.PeopleFirstName; ;
                                    OrderMasterHistories.userid = People.PeopleID;
                                    OrderMasterHistories.CreatedDate = DateTime.Now;
                                    context.OrderMasterHistoriesDB.Add(OrderMasterHistories);
                                    #endregion
                                    //foreach (var detail in ODM.orderDetails)
                                    //{
                                    //    detail.Status = obj.Status;
                                    //    detail.UpdatedDate = indianTime;
                                    //    context.Entry(detail).State = EntityState.Modified;
                                    //}
                                    ODMaster.Status = obj.Status;
                                    ODMaster.comments = obj.comments;
                                    ODMaster.UpdatedDate = indianTime;
                                    context.Entry(ODMaster).State = EntityState.Modified;
                                    foreach (var detail in ODMaster.orderDetails)
                                    {
                                        detail.Status = obj.Status;
                                        detail.UpdatedDate = indianTime;
                                        context.Entry(detail).State = EntityState.Modified;
                                    }

                                }
                            }
                            else if (ODM.Status == "Delivery Canceled Request")
                            {
                                Status = true;
                                msg = "Order already Delivery Canceled Request";
                                res = new ResDTO()
                                {
                                    op = obj,
                                    Message = msg,
                                    Status = Status
                                };
                                ordersToProcess.RemoveAll(x => x == obj.OrderId);

                                return Request.CreateResponse(HttpStatusCode.OK, res);
                            }
                            else if (obj.Status == "Delivery Redispatch" && ODM.Status != "Delivery Redispatch")
                            {
                                OrderDeliveryMaster.Status = obj.Status;
                                OrderDeliveryMaster.comments = obj.comments;
                                OrderDeliveryMaster.UpdatedDate = indianTime;
                                OrderDeliveryMaster.DeliveryLat = obj.DeliveryLat;//added on 08/07/02019 
                                OrderDeliveryMaster.DeliveryLng = obj.DeliveryLng;
                                context.Entry(OrderDeliveryMaster).State = EntityState.Modified;

                                ODM.Status = "Delivery Redispatch";
                                ODM.ReDispatchedStatus = "Delivery Redispatch";
                                ODM.ReDispatchCount = ODM.ReDispatchCount + 1;
                                ODM.Signimg = obj.Signimg;
                                ODM.comments = obj.comments;
                                ODM.UpdatedDate = indianTime;
                                ODM.DeliveryLat = obj.DeliveryLat;//added on 08/07/02019 
                                ODM.DeliveryLng = obj.DeliveryLng;
                                ODM.ReDispatchedDate = obj.ReDispatchedDate;
                                context.Entry(ODM).State = EntityState.Modified;
                                #region Order Master History for Status Delivery Redispatch

                                OrderMasterHistories OrderMasterHistories = new OrderMasterHistories();

                                OrderMasterHistories.orderid = ODM.OrderId;
                                OrderMasterHistories.Status = ODM.Status;
                                OrderMasterHistories.Reasoncancel = null;
                                OrderMasterHistories.Warehousename = ODM.WarehouseName;
                                OrderMasterHistories.DeliveryIssuanceId = obj.DeliveryIssuanceId;
                                OrderMasterHistories.username = People.DisplayName != null ? People.DisplayName : People.PeopleFirstName; ;
                                OrderMasterHistories.userid = People.PeopleID;
                                OrderMasterHistories.CreatedDate = DateTime.Now;
                                context.OrderMasterHistoriesDB.Add(OrderMasterHistories);
                                if (obj.ConformationDate != null)
                                {
                                    DeliveryCanceledRequestHistory DeliveryCanceledRequestHistoryAdd = new DeliveryCanceledRequestHistory();
                                    DeliveryCanceledRequestHistoryAdd.OrderId = ODM.OrderId;
                                    DeliveryCanceledRequestHistoryAdd.DeliveryCanceledStatus = obj.DeliveryCanceledStatus;
                                    DeliveryCanceledRequestHistoryAdd.Comments = obj.CancelrequestComments;
                                    DeliveryCanceledRequestHistoryAdd.ConformationDate = obj.ConformationDate;
                                    DeliveryCanceledRequestHistoryAdd.CreatedBy = userid;
                                    DeliveryCanceledRequestHistoryAdd.CreatedDate = indianTime;
                                    DeliveryCanceledRequestHistoryAdd.IsActive = true;
                                    DeliveryCanceledRequestHistoryAdd.IsDeleted = false;
                                    DeliveryCanceledRequestHistoryAdd.ModifiedDate = indianTime;
                                    DeliveryCanceledRequestHistoryAdd.ModifiedBy = 0;
                                    context.DeliveryCanceledRequestHistoryDb.Add(DeliveryCanceledRequestHistoryAdd);
                                }
                                else
                                {
                                    DeliveryCanceledRequestHistory DeliveryCanceledRequestHistoryAdd = new DeliveryCanceledRequestHistory();
                                    DeliveryCanceledRequestHistoryAdd.OrderId = ODM.OrderId;
                                    DeliveryCanceledRequestHistoryAdd.DeliveryCanceledStatus = obj.DeliveryCanceledStatus;
                                    DeliveryCanceledRequestHistoryAdd.Comments = obj.CancelrequestComments;
                                    DeliveryCanceledRequestHistoryAdd.ConformationDate = obj.ConformationDate;
                                    DeliveryCanceledRequestHistoryAdd.CreatedBy = userid;
                                    DeliveryCanceledRequestHistoryAdd.CreatedDate = indianTime;
                                    DeliveryCanceledRequestHistoryAdd.IsActive = true;
                                    DeliveryCanceledRequestHistoryAdd.IsDeleted = false;
                                    DeliveryCanceledRequestHistoryAdd.ModifiedDate = indianTime;
                                    DeliveryCanceledRequestHistoryAdd.ModifiedBy = 0;
                                    context.DeliveryCanceledRequestHistoryDb.Add(DeliveryCanceledRequestHistoryAdd);
                                }
                                #endregion

                                //foreach (var detail in ODM.orderDetails)
                                //{
                                //    detail.Status = obj.Status;
                                //    detail.UpdatedDate = indianTime;
                                //    context.Entry(detail).State = EntityState.Modified;

                                //}

                                ODMaster.Status = obj.Status;
                                ODMaster.comments = obj.comments;
                                ODMaster.UpdatedDate = indianTime;
                                ODMaster.ReDispatchCount = ODM.ReDispatchCount;

                                context.Entry(ODMaster).State = EntityState.Modified;

                                foreach (var detail in ODMaster.orderDetails)
                                {
                                    detail.Status = obj.Status;
                                    detail.UpdatedDate = indianTime;
                                    context.Entry(detail).State = EntityState.Modified;

                                }

                                //var RO = context.RedispatchWarehouseDb.Where(x => x.OrderId == obj.OrderId && x.DboyMobileNo == obj.DboyMobileNo).FirstOrDefault();
                                //if (RO != null)
                                //{
                                //    RO.Status = obj.Status;
                                //    RO.comments = obj.comments;
                                //    RO.ReDispatchCount = ODM.ReDispatchCount;
                                //    RO.UpdatedDate = indianTime;
                                //    context.Entry(RO).State = EntityState.Modified;

                                //}
                                //else
                                //{
                                //    RO = new RedispatchWarehouse();
                                //    RO.active = true;
                                //    RO.comments = obj.comments;
                                //    RO.CompanyId = ODM.CompanyId;
                                //    RO.CreatedDate = indianTime;
                                //    RO.UpdatedDate = indianTime;
                                //    RO.DboyMobileNo = obj.DboyMobileNo;
                                //    RO.DboyName = obj.DboyName;
                                //    RO.Deleted = false;
                                //    RO.OrderDispatchedMasterId = obj.OrderDispatchedMasterId;
                                //    RO.OrderId = obj.OrderId;
                                //    RO.WarehouseId = obj.WarehouseId;
                                //    RO.ReDispatchCount = ODM.ReDispatchCount;
                                //    RO.Status = obj.Status;
                                //    context.RedispatchWarehouseDb.Add(RO);
                                //}
                                #region New Delivery Optimization process
                                if (obj.DeliveryOptimizationdc != null)
                                {
                                    var tripPlannerConfirmedOrderId = new SqlParameter("@TripPlannerConfirmedOrderId", obj.DeliveryOptimizationdc.TripPlannerConfirmedOrderId);
                                    long tripPlannerConfirmedDetailId = context.Database.SqlQuery<long>("GetTripPlannerConfirmedDetailId @TripPlannerConfirmedOrderId", tripPlannerConfirmedOrderId).FirstOrDefault();
                                    var tripPlannerConfirmedDetails = context.TripPlannerConfirmedDetails.Where(x => x.Id == tripPlannerConfirmedDetailId).FirstOrDefault();
                                    var orderids = tripPlannerConfirmedDetails.CommaSeparatedOrderList.Split(',').Select(Int32.Parse).ToList();
                                    var orderDispatchedMasters = context.OrderDispatchedMasters.Where(x => orderids.Contains(x.OrderId)).ToList();
                                    bool IsShipped = true;
                                    if (orderDispatchedMasters.Any() && orderDispatchedMasters != null)
                                    {
                                        foreach (var item in orderDispatchedMasters)
                                        {
                                            if (item.Status == "Shipped")
                                            {
                                                IsShipped = false;
                                            }
                                        }
                                        tripPlannerConfirmedDetails.IsProcess = IsShipped;
                                        context.Entry(tripPlannerConfirmedDetails).State = EntityState.Modified;
                                        var tripPlannerVehicle = context.TripPlannerVehicleDb.Where(x => x.TripPlannerConfirmedMasterId == tripPlannerConfirmedDetails.TripPlannerConfirmedMasterId).FirstOrDefault();
                                        if (tripPlannerVehicle != null)
                                        {
                                            var tripPlannerConfirmedOrders = context.TripPlannerConfirmedOrders.Where(x => orderids.Contains((int)x.OrderId) && x.TripPlannerConfirmedDetailId == tripPlannerConfirmedDetailId && x.IsActive == true && x.IsDeleted == false).ToList();
                                            if (tripPlannerConfirmedDetails.IsProcess)
                                            {
                                                tripPlannerVehicle.CurrentStatus = (int)VehicleliveStatus.InTransit;
                                                tripPlannerVehicle.ModifiedDate = indianTime;
                                                tripPlannerVehicle.ModifiedBy = userid;
                                                tripPlannerVehicle.CurrentLat = obj.DeliveryOptimizationdc.lat;
                                                tripPlannerVehicle.CurrentLng = obj.DeliveryOptimizationdc.lng;
                                                tripPlannerVehicle.ReminingTime -= (tripPlannerConfirmedDetails.TotalTimeInMins - tripPlannerConfirmedOrders.Sum(x => x.TimeInMins)) + (tripPlannerConfirmedOrders.Where(x => x.OrderId == obj.OrderId).FirstOrDefault().TimeInMins);
                                                tripPlannerVehicle.TravelTime += (tripPlannerConfirmedDetails.TotalTimeInMins - tripPlannerConfirmedOrders.Sum(x => x.TimeInMins)) + (tripPlannerConfirmedOrders.Where(x => x.OrderId == obj.OrderId).FirstOrDefault().TimeInMins);
                                                tripPlannerVehicle.DistanceLeft -= tripPlannerConfirmedDetails.TotalDistanceInMeter;
                                                tripPlannerVehicle.DistanceTraveled += tripPlannerConfirmedDetails.TotalDistanceInMeter;
                                                /////////////////////
                                                context.Entry(tripPlannerVehicle).State = EntityState.Modified;
                                                TripPlannerVehicleHistory tripPlannerVehicleHistory = new TripPlannerVehicleHistory();
                                                tripPlannerVehicleHistory.TripPlannerVehicleId = tripPlannerVehicle.Id;
                                                tripPlannerVehicleHistory.CurrentServingOrderId = obj.DeliveryOptimizationdc.OrderId;
                                                tripPlannerVehicleHistory.RecordTime = indianTime;
                                                tripPlannerVehicleHistory.Lat = obj.DeliveryOptimizationdc.lat;
                                                tripPlannerVehicleHistory.Lng = obj.DeliveryOptimizationdc.lng;
                                                tripPlannerVehicleHistory.CreatedDate = indianTime;
                                                tripPlannerVehicleHistory.CreatedBy = userid;
                                                tripPlannerVehicleHistory.IsActive = true;
                                                tripPlannerVehicleHistory.IsDeleted = false;
                                                tripPlannerVehicleHistory.RecoardStatus = (int)VehicleliveStatus.InTransit;
                                                tripPlannerVehicleHistory.TripPlannerConfirmedDetailId = tripPlannerConfirmedDetails.Id;
                                                context.TripPlannerVehicleHistoryDb.Add(tripPlannerVehicleHistory);
                                            }
                                            else
                                            {
                                                tripPlannerVehicle.CurrentStatus = (int)VehicleliveStatus.InTransit;
                                                tripPlannerVehicle.ModifiedDate = indianTime;
                                                tripPlannerVehicle.ModifiedBy = userid;
                                                tripPlannerVehicle.CurrentLat = obj.DeliveryOptimizationdc.lat;
                                                tripPlannerVehicle.CurrentLng = obj.DeliveryOptimizationdc.lng;
                                                tripPlannerVehicle.ReminingTime -= tripPlannerConfirmedOrders.Where(x => x.OrderId == obj.OrderId).Select(x => x.TimeInMins).FirstOrDefault();
                                                tripPlannerVehicle.TravelTime += tripPlannerConfirmedOrders.Where(x => x.OrderId == obj.OrderId).Select(x => x.TimeInMins).FirstOrDefault();
                                                context.Entry(tripPlannerVehicle).State = EntityState.Modified;
                                                TripPlannerVehicleHistory tripPlannerVehicleHistory = new TripPlannerVehicleHistory();
                                                tripPlannerVehicleHistory.TripPlannerVehicleId = tripPlannerVehicle.Id;
                                                tripPlannerVehicleHistory.CurrentServingOrderId = obj.DeliveryOptimizationdc.OrderId;
                                                tripPlannerVehicleHistory.RecordTime = indianTime;
                                                tripPlannerVehicleHistory.Lat = obj.DeliveryOptimizationdc.lat;
                                                tripPlannerVehicleHistory.Lng = obj.DeliveryOptimizationdc.lng;
                                                tripPlannerVehicleHistory.CreatedDate = indianTime;
                                                tripPlannerVehicleHistory.CreatedBy = userid;
                                                tripPlannerVehicleHistory.IsActive = true;
                                                tripPlannerVehicleHistory.IsDeleted = false;
                                                tripPlannerVehicleHistory.RecoardStatus = (int)VehicleliveStatus.InTransit;
                                                tripPlannerVehicleHistory.TripPlannerConfirmedDetailId = tripPlannerConfirmedDetails.Id;
                                                context.TripPlannerVehicleHistoryDb.Add(tripPlannerVehicleHistory);
                                            }
                                        }
                                    }
                                }
                                #endregion

                                if (ODM.Deliverydate.Date == DateTime.Now.Date)
                                {
                                    MongoDbHelper<DataContracts.Mongo.CustomerRedispatchCharges> mongoDbHelper = new MongoDbHelper<DataContracts.Mongo.CustomerRedispatchCharges>();
                                    var customerRedispatchCharges = mongoDbHelper.Select(x => x.MinValue < ODM.GrossAmount && x.MaxValue >= ODM.GrossAmount && x.WarehouseId == ODM.WarehouseId).FirstOrDefault();

                                    if (customerRedispatchCharges != null && customerRedispatchCharges.RedispatchCharges > 0)
                                    {
                                        Wallet wlt = context.WalletDb.FirstOrDefault(c => c.CustomerId == ODM.CustomerId);
                                        if (wlt != null)
                                        {
                                            CustomerWalletHistory CWH = new CustomerWalletHistory();
                                            CWH.WarehouseId = ODM.WarehouseId;
                                            CWH.CompanyId = ODM.CompanyId;
                                            CWH.CustomerId = wlt.CustomerId;
                                            CWH.NewAddedWAmount = 0;
                                            CWH.NewOutWAmount = -(customerRedispatchCharges.RedispatchCharges * 10);
                                            CWH.OrderId = ODM.OrderId;
                                            CWH.Through = "From Order Redispatched";
                                            CWH.TotalWalletAmount = wlt.TotalAmount - (customerRedispatchCharges.RedispatchCharges * 10);
                                            CWH.CreatedDate = indianTime;
                                            CWH.UpdatedDate = indianTime;
                                            context.CustomerWalletHistoryDb.Add(CWH);

                                            wlt.TotalAmount -= (customerRedispatchCharges.RedispatchCharges * 10);
                                            wlt.TransactionDate = indianTime;
                                            context.Entry(wlt).State = EntityState.Modified;
                                        }
                                    }
                                }

                            }
                            else if (ODM.Status == "Delivery Redispatch")
                            {
                                Status = true;
                                msg = "Order already Redispatch";
                                res = new ResDTO()
                                {
                                    op = obj,
                                    Message = msg,
                                    Status = Status
                                };
                                ordersToProcess.RemoveAll(x => x == obj.OrderId);
                                return Request.CreateResponse(HttpStatusCode.OK, res);
                            }
                        }
                        else
                        {

                            res = new ResDTO()
                            {
                                op = null,
                                Message = "Not Delivered",
                                Status = Status
                            };
                            ordersToProcess.RemoveAll(x => x == obj.OrderId);
                            return Request.CreateResponse(HttpStatusCode.OK, res);
                        }
                        if (context.Commit() > 0)
                        {
                            #region stock Hit on poc
                            //for currentstock
                            if (ODMaster != null && ODMaster.OrderType != 5 && ODM.Status == "Delivered")
                            {

                                MultiStockHelper<OnDeliveredStockEntryDc> MultiStockHelpers = new MultiStockHelper<OnDeliveredStockEntryDc>();
                                List<OnDeliveredStockEntryDc> OnDeliveredCStockList = new List<OnDeliveredStockEntryDc>();
                                foreach (var StockHit in ODM.orderDetails.Where(x => x.qty > 0))
                                {
                                    var RefStockCode = ODMaster.OrderType == 8 ? "CL" : "C";
                                    bool isFree = ODMaster.orderDetails.Any(c => c.OrderDetailsId == StockHit.OrderDetailsId && c.IsFreeItem && c.IsDispatchedFreeStock);
                                    if (isFree) { RefStockCode = "F"; }
                                    else if (ODMaster.OrderType == 6) //6 Damage stock
                                    {
                                        RefStockCode = "D";
                                    }
                                    else if (ODMaster.OrderType == 9) //9 NonSellable stock
                                    {
                                        RefStockCode = "N";
                                    }
                                    OnDeliveredCStockList.Add(new OnDeliveredStockEntryDc
                                    {
                                        ItemMultiMRPId = StockHit.ItemMultiMRPId,
                                        OrderDispatchedDetailsId = StockHit.OrderDispatchedDetailsId,
                                        OrderId = StockHit.OrderId,
                                        Qty = StockHit.qty,
                                        UserId = People.PeopleID,
                                        WarehouseId = StockHit.WarehouseId,
                                        RefStockCode = RefStockCode,

                                    });

                                }
                                if (OnDeliveredCStockList.Any())
                                {
                                    bool IsUpdated = MultiStockHelpers.MakeEntry(OnDeliveredCStockList, "Stock_OnDelivered_New", context, dbContextTransaction);
                                    if (!IsUpdated)
                                    {
                                        Status = false;
                                        dbContextTransaction.Dispose();
                                        res = new ResDTO()
                                        {
                                            op = null,
                                            Message = "Not Delivered",
                                            Status = Status
                                        };
                                        ordersToProcess.RemoveAll(x => x == obj.OrderId);
                                        return Request.CreateResponse(HttpStatusCode.OK, res);
                                    }
                                }
                            }
                            else if (ODMaster != null && ODMaster.OrderType != 5 && ODM.Status == "Delivery Canceled")
                            {

                                MultiStockHelper<OnDeliveryCanceledStockEntryDc> MultiStockHelpers = new MultiStockHelper<OnDeliveryCanceledStockEntryDc>();
                                List<OnDeliveryCanceledStockEntryDc> DeliveryCanceledCStockList = new List<OnDeliveryCanceledStockEntryDc>();
                                foreach (var StockHit in ODM.orderDetails.Where(x => x.qty > 0))
                                {
                                    var RefStockCode = ODMaster.OrderType == 8 ? "CL" : "C";
                                    bool isFree = ODMaster.orderDetails.Any(c => c.OrderDetailsId == StockHit.OrderDetailsId && c.IsFreeItem && c.IsDispatchedFreeStock);
                                    if (isFree) { RefStockCode = "F"; }
                                    else if (ODMaster.OrderType == 6) //6 Damage stock
                                    {
                                        RefStockCode = "D";
                                    }
                                    else if (ODMaster.OrderType == 9) //9 Non Sellable Stock
                                    {
                                        RefStockCode = "N";
                                    }
                                    DeliveryCanceledCStockList.Add(new OnDeliveryCanceledStockEntryDc
                                    {
                                        ItemMultiMRPId = StockHit.ItemMultiMRPId,
                                        OrderDispatchedDetailsId = StockHit.OrderDispatchedDetailsId,
                                        OrderId = StockHit.OrderId,
                                        Qty = StockHit.qty,
                                        UserId = People.PeopleID,
                                        WarehouseId = StockHit.WarehouseId,
                                        RefStockCode = RefStockCode,
                                        IsDeliveryRedispatchCancel = false
                                    });
                                }
                                if (DeliveryCanceledCStockList.Any())
                                {
                                    bool IsUpdated = MultiStockHelpers.MakeEntry(DeliveryCanceledCStockList, "Stock_OnDeliveryCancel_New", context, dbContextTransaction);
                                    if (!IsUpdated)
                                    {
                                        Status = false;
                                        dbContextTransaction.Dispose();
                                        res = new ResDTO()
                                        {
                                            op = null,
                                            Message = "Order Not Delivery Canceled",
                                            Status = Status
                                        };
                                        ordersToProcess.RemoveAll(x => x == obj.OrderId);
                                        return Request.CreateResponse(HttpStatusCode.OK, res);
                                    }
                                }

                            }
                            else if (ODMaster != null && ODMaster.OrderType != 5 && ODM.Status == "Delivery Redispatch")
                            {

                                MultiStockHelper<OnDeliveryRedispatchedStockEntryDc> MultiStockHelpers = new MultiStockHelper<OnDeliveryRedispatchedStockEntryDc>();
                                List<OnDeliveryRedispatchedStockEntryDc> DeliveryRedispatchedCStockList = new List<OnDeliveryRedispatchedStockEntryDc>();
                                foreach (var StockHit in ODM.orderDetails.Where(x => x.qty > 0))
                                {
                                    var RefStockCode = ODMaster.OrderType == 8 ? "CL" : "C";
                                    bool isFree = ODMaster.orderDetails.Any(c => c.OrderDetailsId == StockHit.OrderDetailsId && c.IsFreeItem && c.IsDispatchedFreeStock);
                                    if (isFree) { RefStockCode = "F"; }
                                    else if (ODMaster.OrderType == 6) //6 Damage stock
                                    {
                                        RefStockCode = "D";
                                    }
                                    else if (ODMaster.OrderType == 9) //9 Non Sellable Stock
                                    {
                                        RefStockCode = "N";
                                    }
                                    DeliveryRedispatchedCStockList.Add(new OnDeliveryRedispatchedStockEntryDc
                                    {
                                        ItemMultiMRPId = StockHit.ItemMultiMRPId,
                                        OrderDispatchedDetailsId = StockHit.OrderDispatchedDetailsId,
                                        OrderId = StockHit.OrderId,
                                        Qty = StockHit.qty,
                                        UserId = People.PeopleID,
                                        WarehouseId = StockHit.WarehouseId,
                                        RefStockCode = RefStockCode,
                                        IsDeliveryCancel = false
                                    });
                                }
                                if (DeliveryRedispatchedCStockList.Any())
                                {
                                    bool IsUpdated = MultiStockHelpers.MakeEntry(DeliveryRedispatchedCStockList, "Stock_OnDeliveryRedispatch", context, dbContextTransaction);
                                    if (!IsUpdated)
                                    {
                                        Status = false;
                                        dbContextTransaction.Dispose();
                                        res = new ResDTO()
                                        {
                                            op = null,
                                            Message = "Order Not Delivery Redispatch",
                                            Status = Status
                                        };
                                        ordersToProcess.RemoveAll(x => x == obj.OrderId);
                                        return Request.CreateResponse(HttpStatusCode.OK, res);
                                    }
                                }

                            }
                            else if (ODMaster != null && ODMaster.OrderType != 5 && obj.Status == "Delivery Canceled Request")
                            {
                                //ifold status is Delivery Canceled   Stock_DeliveredOnAssignmentReject

                                MultiStockHelper<OnDeliveryCanceledStockEntryDc> MultiStockHelpers = new MultiStockHelper<OnDeliveryCanceledStockEntryDc>();
                                List<OnDeliveryCanceledStockEntryDc> DeliveryCanceledCStockList = new List<OnDeliveryCanceledStockEntryDc>();
                                foreach (var StockHit in ODM.orderDetails.Where(x => x.qty > 0))
                                {
                                    var RefStockCode = ODMaster.OrderType == 8 ? "CL" : "C";
                                    bool isFree = ODMaster.orderDetails.Any(c => c.OrderDetailsId == StockHit.OrderDetailsId && c.IsFreeItem && c.IsDispatchedFreeStock);
                                    if (isFree) { RefStockCode = "F"; }
                                    else if (ODMaster.OrderType == 6) //6 Damage stock
                                    {
                                        RefStockCode = "D";
                                    }
                                    else if (ODMaster.OrderType == 9) //9 Non Sellable Stock
                                    {
                                        RefStockCode = "N";
                                    }
                                    DeliveryCanceledCStockList.Add(new OnDeliveryCanceledStockEntryDc
                                    {
                                        ItemMultiMRPId = StockHit.ItemMultiMRPId,
                                        OrderDispatchedDetailsId = StockHit.OrderDispatchedDetailsId,
                                        OrderId = StockHit.OrderId,
                                        Qty = StockHit.qty,
                                        UserId = People.PeopleID,
                                        WarehouseId = StockHit.WarehouseId,
                                        RefStockCode = RefStockCode,
                                        // IsDeliveryRedispatchCancel = true
                                    });
                                }
                                if (DeliveryCanceledCStockList.Any())
                                {
                                    bool IsUpdated = MultiStockHelpers.MakeEntry(DeliveryCanceledCStockList, "Stock_OnDeliveryCanceledRequest_New", context, dbContextTransaction);
                                    if (!IsUpdated)
                                    {
                                        Status = false;
                                        dbContextTransaction.Dispose();
                                        res = new ResDTO()
                                        {
                                            op = null,
                                            Message = "Order Not Delivery Canceled Request",
                                            Status = Status
                                        };
                                        return Request.CreateResponse(HttpStatusCode.OK, res);
                                    }
                                }
                            }
                            #endregion
                            dbContextTransaction.Complete();
                            Status = true;
                            string orderCancelMsg = "";
                            if (IsDeliverSmsSent && !string.IsNullOrEmpty(ODMaster.Customerphonenum))
                            {
                                // string message = "Hi " + ODMaster.CustomerName + " Your Order #" + ODMaster.OrderId + " is delivered on time if you have any complaint regarding your order kindly contact our customer care within next 1 Hours.";
                                string message = ""; //"Hi {#var1#} Your Order {#var2#} is delivered on time if you have any complaint regarding your order kindly contact our customer care within {#var3#}. ShopKirana";
                                string OrderConcernMessageFlag = Convert.ToString(ConfigurationManager.AppSettings["OrderConcernMessageFlag"]);
                                if (!string.IsNullOrEmpty(OrderConcernMessageFlag) && OrderConcernMessageFlag == "Y" && ODM.Status == "Delivered")
                                {
                                    // orderCancelMsg = "Hi, Your Order Number  " + ODMaster.OrderId + ", has been delivered. In case of any concerns, please click on the link below " + ConfigurationManager.AppSettings["RetailerWebviewURL"] + "order-concern/" + ODMaster.OrderId + "/" + guidData + ". ShopKirana";
                                    var dltSMS = SMSTemplateHelper.getTemplateText((int)AppEnum.RetailerApp, "Order_Delivered_Concern");
                                    message = dltSMS == null ? "" : dltSMS.Template;

                                    message = message.Replace("{#var1#}", ODMaster.ShopName);
                                    message = message.Replace("{#var2#}", ODMaster.OrderId.ToString());
                                    string shortUrl = Helpers.ShortenerUrl.ShortenUrl(ConfigurationManager.AppSettings["RetailerWebviewURL"] + "order-concern/" + ODMaster.OrderId + "/" + guidData);

                                    message = message.Replace("{#var3#}", shortUrl);
                                    if (dltSMS != null)
                                        Common.Helpers.SendSMSHelper.SendSMS(ODMaster.Customerphonenum, message, ((Int32)Common.Enums.SMSRouteEnum.Transactional).ToString(), dltSMS.DLTId);

                                }
                                else
                                {
                                    var dltSMS = SMSTemplateHelper.getTemplateText((int)AppEnum.RetailerApp, "Delivered_Order_Compaint");
                                    message = dltSMS == null ? "" : dltSMS.Template;

                                    message = message.Replace("{#var1#}", ODMaster.CustomerName);
                                    message = message.Replace("{#var2#}", ODMaster.OrderId.ToString());
                                    message = message.Replace("{#var3#}", " next 1 Hours");
                                    if (dltSMS != null)
                                        Common.Helpers.SendSMSHelper.SendSMS(ODMaster.Customerphonenum, message, ((Int32)Common.Enums.SMSRouteEnum.Transactional).ToString(), dltSMS.DLTId);

                                }
                            }

                            if (ODMaster != null && ODMaster.OrderType == 5 && obj.Status == "Delivered")
                            {
                                try
                                {
                                    UpdateConsumerOrders updateConsumerOrders = new UpdateConsumerOrders
                                    {
                                        CartStatus = "Delivered",
                                        InvoiceNo = ODMaster.invoice_no
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

                            #region Sellerstock update
                            if (ODMaster != null && ODMaster.CustomerType == "SellerStore" && obj.Status == "Delivered")
                            {
                                UpdateSellerStockOfCFRProduct Postobj = new UpdateSellerStockOfCFRProduct();
                                Postobj.OrderId = ODM.OrderId;
                                Postobj.Skcode = ODM.Skcode;
                                Postobj.ItemDetailDc = new List<SellerItemDetailDc>();
                                foreach (var item in ODM.orderDetails)
                                {
                                    SellerItemDetailDc newitem = new SellerItemDetailDc();
                                    newitem.ItemMultiMrpId = item.ItemMultiMRPId;
                                    newitem.SellingPrice = item.UnitPrice;
                                    newitem.qty = item.qty;
                                    Postobj.ItemDetailDc.Add(newitem);
                                }
                                BackgroundTaskManager.Run(() =>
                                {
                                    try
                                    {
                                        var tradeUrl = ConfigurationManager.AppSettings["SellerURL"] + "/api/sk/RetailerAppApi/UpdateStockOnDeliveryFromSkApp";
                                        using (GenericRestHttpClient<UpdateSellerStockOfCFRProduct, string> memberClient = new GenericRestHttpClient<UpdateSellerStockOfCFRProduct, string>(tradeUrl, "", null))
                                        {
                                            AsyncContext.Run(() => memberClient.PostAsync(Postobj));
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        TextFileLogHelper.LogError("Error while Update Seller Stock Of CFR Product: " + ex.ToString());
                                    }
                                });
                            }
                            #endregion
                            //else
                            //{
                            //    dbContextTransaction.Dispose();
                            //    Status = false;
                            //}
                            res = new ResDTO()
                            {
                                op = obj,
                                Message = "Success",
                                Status = Status
                            };
                            ordersToProcess.RemoveAll(x => x == obj.OrderId);
                            return Request.CreateResponse(HttpStatusCode.OK, res);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ordersToProcess.RemoveAll(x => x == obj.OrderId);
                throw ex;
            }
            return Request.CreateResponse(HttpStatusCode.OK);
        }



        /// <summary>
        /// DCR Api By sudhir 08/03/2020
        /// </summary>
        /// <returns></returns>
        [AllowAnonymous]
        [Route("PostDCROrder")]
        [HttpPost]
        public HttpResponseMessage PostDCROrder(OrderPlaceDTO obj) //Order delivered or canceled from delivery app
        {
            var identity = User.Identity as ClaimsIdentity;
            int userid = 0;

            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
            ResDTO res;
            if (ordersToProcess.Any(x => x == obj.OrderId))
            {
                res = new ResDTO()
                {
                    op = null,
                    Message = "Order #: " + obj.OrderId + " is already in process..",
                    Status = false
                };
                return Request.CreateResponse(HttpStatusCode.OK, res);
            }
            else
            {
                ordersToProcess.Add(obj.OrderId);
            }
            try
            {
                var msg = "";
                bool Status = false;
                bool IsDeliverSmsSent = false;
                List<string> chequenos = new List<string>();
                double totalChequeamt = 0;
                List<string> MposGatId = new List<string>();
                double totalMposamount = 0;
                List<string> BankNames = new List<string>();
                TransactionOptions option = new TransactionOptions();
                option.IsolationLevel = IsolationLevel.RepeatableRead;
                option.Timeout = TimeSpan.FromSeconds(90);
                using (var dbContextTransaction = new TransactionScope(TransactionScopeOption.Required, option))
                {
                    using (var context = new AuthContext())
                    {
                        OrderMaster ODMaster = null;
                        var People = context.Peoples.Where(x => x.Mobile == obj.DboyMobileNo).FirstOrDefault();
                        var DeliveryIssuance = context.DeliveryIssuanceDb.Where(x => x.DeliveryIssuanceId == obj.DeliveryIssuanceId).SingleOrDefault();
                        if (DeliveryIssuance.Status == "Accepted")
                        {
                            DeliveryIssuance.Status = "Pending";
                            DeliveryIssuance.UpdatedDate = indianTime;
                            context.Entry(DeliveryIssuance).State = EntityState.Modified;
                        }
                        //else if (DeliveryIssuance.Status == "Pending")
                        //{
                        //}
                        //else
                        //{
                        //    Status = false;
                        //    res = new ResDTO()
                        //    {
                        //        op = null,
                        //        Message = "Not Delivered due to Assignment in status : " + DeliveryIssuance.Status,
                        //        Status = Status
                        //    };
                        //    ordersToProcess.RemoveAll(x => x == obj.OrderId);
                        //    return Request.CreateResponse(HttpStatusCode.OK, res);
                        //}
                        var ODM = context.OrderDispatchedMasters.Where(x => x.OrderDispatchedMasterId == obj.OrderDispatchedMasterId).Include("orderDetails").SingleOrDefault();
                        ODMaster = context.DbOrderMaster.Where(x => x.OrderId == obj.OrderId).Include("orderDetails").SingleOrDefault();
                        if (obj != null && People != null && DeliveryIssuance.DeliveryIssuanceId > 0 && ODM != null)
                        {
                            //var ODM = context.OrderDispatchedMasters.Where(x => x.OrderDispatchedMasterId == obj.OrderDispatchedMasterId).Include("orderDetails").SingleOrDefault();
                            var OrderDeliveryMaster = context.OrderDeliveryMasterDB.Where(z => z.DeliveryIssuanceId == obj.DeliveryIssuanceId && z.OrderId == obj.OrderId).Include("orderDetails").FirstOrDefault();
                            var AssignmentRechangeOrder = context.AssignmentRechangeOrder.FirstOrDefault(x => x.AssignmentId == obj.DeliveryIssuanceId && x.OrderId == obj.OrderId);
                            var Ordercancellation = context.DeptOrderCancellationDb.Where(o => o.OrderId == obj.OrderId).FirstOrDefault();
                            DeptOrderCancellation deptOrderCacellation = new DeptOrderCancellation();
                            deptOrderCacellation.OrderId = obj.OrderId;

                            if (AssignmentRechangeOrder != null)
                            {
                                AssignmentRechangeOrder.Status = 0;
                                AssignmentRechangeOrder.ModifiedDate = indianTime;
                                AssignmentRechangeOrder.ModifiedBy = People.PeopleID;
                                context.Entry(AssignmentRechangeOrder).State = EntityState.Modified;
                            }

                            #region Damage Order status Update
                            if (ODMaster.OrderType == 6 && ODM.invoice_no != null)
                            {
                                var DOM = context.DamageOrderMasterDB.Where(x => x.invoice_no == ODM.invoice_no).SingleOrDefault();

                                if (DOM != null)
                                {
                                    DOM.UpdatedDate = indianTime;
                                    DOM.Status = obj.Status;
                                    context.Entry(DOM).State = EntityState.Modified;
                                }
                            }
                            #endregion
                            if (obj.Status == "Delivery Canceled" && ODM.Status != "Delivery Canceled")
                            {
                                OrderDeliveryMaster.Status = obj.Status;
                                OrderDeliveryMaster.comments = obj.DeliveryCanceledStatus;
                                //OrderDeliveryMaster.ChequeDate = obj.ChequeDate;
                                OrderDeliveryMaster.UpdatedDate = indianTime;
                                OrderDeliveryMaster.DeliveryLat = obj.DeliveryLat;//added on 08/07/02019 
                                OrderDeliveryMaster.DeliveryLng = obj.DeliveryLng;
                                context.Entry(OrderDeliveryMaster).State = EntityState.Modified;
                                //done on 28/02/2020 by PoojaZ //start
                                if (OrderDeliveryMaster.comments == "Dont Want" || OrderDeliveryMaster.comments == "Late Delivery (OPS)" || OrderDeliveryMaster.comments == "Damage Expiry (Pur)")
                                {
                                    string query = "select sp.PeopleID  from OrderMasters a inner join Customers c on a.CustomerId=c.CustomerId and a.orderid = " + OrderDeliveryMaster.OrderId + "  inner join People sp on a.SalesPersonId=sp.PeopleID";
                                    var salesLeadId = context.Database.SqlQuery<int?>(query).FirstOrDefault();
                                    if (salesLeadId.HasValue && salesLeadId.Value > 0)
                                    {
                                        deptOrderCacellation.SalesLeadId = salesLeadId.Value;
                                        deptOrderCacellation.OrderId = OrderDeliveryMaster.OrderId;
                                        deptOrderCacellation.ChargePoint = 1;
                                        deptOrderCacellation.IsActive = true;
                                        deptOrderCacellation.IsDeleted = false;
                                        deptOrderCacellation.IsEmailSend = false;
                                        deptOrderCacellation.CreatedDate = DateTime.Now;
                                        deptOrderCacellation.CreatedBy = People.PeopleID;
                                        if (OrderDeliveryMaster.comments == "Dont Want")
                                        {
                                            deptOrderCacellation.DepId = 6;
                                        }
                                        else if (OrderDeliveryMaster.comments == "Late Delivery (OPS)")
                                        {
                                            deptOrderCacellation.DepId = 33;
                                        }
                                        else if (OrderDeliveryMaster.comments == "Damage Expiry (Pur)")
                                        {
                                            deptOrderCacellation.DepId = 29;
                                        }
                                        context.DeptOrderCancellationDb.Add(deptOrderCacellation);
                                    }
                                }
                                //done on 28/02/2020 by PoojaZ //end

                                if (ODM != null)
                                {
                                    ODM.Status = obj.Status;
                                    ODM.CanceledStatus = obj.Status;
                                    ODM.comments = obj.DeliveryCanceledStatus;
                                    ODM.DeliveryCanceledComments = obj.CancelrequestComments;
                                    ODM.Signimg = obj.Signimg;
                                    ODM.UpdatedDate = indianTime;
                                    ODM.DeliveryLat = obj.DeliveryLat;//added on 08/07/02019 
                                    ODM.DeliveryLng = obj.DeliveryLng;
                                    context.Entry(ODM).State = EntityState.Modified;


                                    #region Order Master History for Status Delivery Canceled
                                    OrderMasterHistories OrderMasterHistories = new OrderMasterHistories();
                                    OrderMasterHistories.orderid = ODM.OrderId;
                                    OrderMasterHistories.Status = ODM.Status;
                                    OrderMasterHistories.Reasoncancel = obj.DeliveryCanceledStatus;
                                    OrderMasterHistories.Warehousename = ODM.WarehouseName;
                                    OrderMasterHistories.DeliveryIssuanceId = obj.DeliveryIssuanceId;
                                    OrderMasterHistories.username = People.DisplayName != null ? People.DisplayName : People.PeopleFirstName; ;
                                    OrderMasterHistories.userid = People.PeopleID;
                                    OrderMasterHistories.CreatedDate = DateTime.Now;
                                    context.OrderMasterHistoriesDB.Add(OrderMasterHistories);
                                    DeliveryCanceledRequestHistory DeliveryCanceledRequestHistoryAdd = new DeliveryCanceledRequestHistory();
                                    DeliveryCanceledRequestHistoryAdd.OrderId = ODM.OrderId;
                                    DeliveryCanceledRequestHistoryAdd.Comments = obj.CancelrequestComments;
                                    DeliveryCanceledRequestHistoryAdd.DeliveryCanceledStatus = obj.DeliveryCanceledStatus;
                                    DeliveryCanceledRequestHistoryAdd.CreatedBy = userid;
                                    DeliveryCanceledRequestHistoryAdd.CreatedDate = DateTime.Now;
                                    DeliveryCanceledRequestHistoryAdd.IsActive = true;
                                    DeliveryCanceledRequestHistoryAdd.IsDeleted = false;
                                    context.DeliveryCanceledRequestHistoryDb.Add(DeliveryCanceledRequestHistoryAdd);
                                    #endregion
                                    //foreach (var detail in ODM.orderDetails)
                                    //{
                                    //    detail.Status = obj.Status;
                                    //    detail.UpdatedDate = indianTime;
                                    //    context.Entry(detail).State = EntityState.Modified;
                                    //}
                                    ODMaster.Status = obj.Status;
                                    ODMaster.comments = obj.DeliveryCanceledStatus;
                                    ODMaster.UpdatedDate = indianTime;
                                    context.Entry(ODMaster).State = EntityState.Modified;
                                    foreach (var detail in ODMaster.orderDetails)
                                    {
                                        detail.Status = obj.Status;
                                        detail.UpdatedDate = indianTime;
                                        context.Entry(detail).State = EntityState.Modified;
                                    }

                                }

                                //}//end dt:-9/3/2020
                            }
                            else if (ODM.Status == "Delivery Canceled")
                            {
                                Status = true;
                                msg = "Order already Delivery Canceled";
                                res = new ResDTO()
                                {
                                    op = obj,
                                    Message = msg,
                                    Status = Status
                                };
                                ordersToProcess.RemoveAll(x => x == obj.OrderId);

                                return Request.CreateResponse(HttpStatusCode.OK, res);
                            }
                            else if (obj.Status == "Delivery Redispatch" && ODM.Status != "Delivery Redispatch")
                            {
                                OrderDeliveryMaster.Status = obj.Status;
                                OrderDeliveryMaster.comments = obj.comments;
                                OrderDeliveryMaster.UpdatedDate = indianTime;
                                OrderDeliveryMaster.DeliveryLat = obj.DeliveryLat;//added on 08/07/02019 
                                OrderDeliveryMaster.DeliveryLng = obj.DeliveryLng;
                                context.Entry(OrderDeliveryMaster).State = EntityState.Modified;

                                ODM.Status = "Delivery Redispatch";
                                ODM.ReDispatchedStatus = "Delivery Redispatch";
                                ODM.ReDispatchCount = ODM.ReDispatchCount + 1;
                                ODM.Signimg = obj.Signimg;
                                ODM.comments = obj.comments;
                                ODM.UpdatedDate = indianTime;
                                ODM.DeliveryLat = obj.DeliveryLat;//added on 08/07/02019 
                                ODM.DeliveryLng = obj.DeliveryLng;
                                ODM.DeliveryCanceledComments = obj.CancelrequestComments;
                                context.Entry(ODM).State = EntityState.Modified;
                                #region Order Master History for Status Delivery Redispatch

                                OrderMasterHistories OrderMasterHistories = new OrderMasterHistories();

                                OrderMasterHistories.orderid = ODM.OrderId;
                                OrderMasterHistories.Status = ODM.Status;
                                OrderMasterHistories.Reasoncancel = null;
                                OrderMasterHistories.Warehousename = ODM.WarehouseName;
                                OrderMasterHistories.DeliveryIssuanceId = obj.DeliveryIssuanceId;
                                OrderMasterHistories.username = People.DisplayName != null ? People.DisplayName : People.PeopleFirstName; ;
                                OrderMasterHistories.userid = People.PeopleID;
                                OrderMasterHistories.CreatedDate = DateTime.Now;
                                context.OrderMasterHistoriesDB.Add(OrderMasterHistories);
                                var DCR = context.DeliveryCanceledRequestHistoryDb.Where(x => x.OrderId == ODM.OrderId && x.IsActive == true && x.IsDeleted == false).FirstOrDefault();
                                if (DCR == null)
                                {
                                    if (obj.ConformationDate != null)
                                    {
                                        DeliveryCanceledRequestHistory DeliveryCanceledRequestHistoryAdd = new DeliveryCanceledRequestHistory();
                                        DeliveryCanceledRequestHistoryAdd.OrderId = ODM.OrderId;
                                        DeliveryCanceledRequestHistoryAdd.DeliveryCanceledStatus = obj.DeliveryCanceledStatus;
                                        DeliveryCanceledRequestHistoryAdd.Comments = obj.CancelrequestComments;
                                        DeliveryCanceledRequestHistoryAdd.ConformationDate = obj.ConformationDate;
                                        DeliveryCanceledRequestHistoryAdd.CreatedBy = userid;
                                        DeliveryCanceledRequestHistoryAdd.CreatedDate = indianTime;
                                        DeliveryCanceledRequestHistoryAdd.IsActive = true;
                                        DeliveryCanceledRequestHistoryAdd.IsDeleted = false;
                                        DeliveryCanceledRequestHistoryAdd.ModifiedDate = indianTime;
                                        DeliveryCanceledRequestHistoryAdd.ModifiedBy = 0;
                                        context.DeliveryCanceledRequestHistoryDb.Add(DeliveryCanceledRequestHistoryAdd);
                                    }
                                    else
                                    {

                                        DeliveryCanceledRequestHistory DeliveryCanceledRequestHistoryAdd = new DeliveryCanceledRequestHistory();
                                        DeliveryCanceledRequestHistoryAdd.OrderId = ODM.OrderId;
                                        DeliveryCanceledRequestHistoryAdd.DeliveryCanceledStatus = obj.DeliveryCanceledStatus;
                                        DeliveryCanceledRequestHistoryAdd.Comments = obj.CancelrequestComments;
                                        DeliveryCanceledRequestHistoryAdd.ConformationDate = obj.ConformationDate;
                                        DeliveryCanceledRequestHistoryAdd.CreatedBy = userid;
                                        DeliveryCanceledRequestHistoryAdd.CreatedDate = indianTime;
                                        DeliveryCanceledRequestHistoryAdd.IsActive = true;
                                        DeliveryCanceledRequestHistoryAdd.IsDeleted = false;
                                        DeliveryCanceledRequestHistoryAdd.ModifiedDate = indianTime;
                                        DeliveryCanceledRequestHistoryAdd.ModifiedBy = 0;
                                        context.DeliveryCanceledRequestHistoryDb.Add(DeliveryCanceledRequestHistoryAdd);
                                    }
                                }
                                else
                                {
                                    if (obj.ConformationDate != null)
                                    {
                                        DCR.IsActive = false;
                                        DCR.IsDeleted = true;
                                        context.Entry(DCR).State = EntityState.Modified;
                                        DeliveryCanceledRequestHistory DeliveryCanceledRequestHistoryAdd = new DeliveryCanceledRequestHistory();
                                        DeliveryCanceledRequestHistoryAdd.OrderId = ODM.OrderId;
                                        DeliveryCanceledRequestHistoryAdd.DeliveryCanceledStatus = obj.DeliveryCanceledStatus;
                                        DeliveryCanceledRequestHistoryAdd.Comments = obj.CancelrequestComments;
                                        DeliveryCanceledRequestHistoryAdd.ConformationDate = obj.ConformationDate;
                                        DeliveryCanceledRequestHistoryAdd.CreatedBy = userid;
                                        DeliveryCanceledRequestHistoryAdd.CreatedDate = indianTime;
                                        DeliveryCanceledRequestHistoryAdd.IsActive = true;
                                        DeliveryCanceledRequestHistoryAdd.IsDeleted = false;
                                        DeliveryCanceledRequestHistoryAdd.ModifiedDate = indianTime;
                                        DeliveryCanceledRequestHistoryAdd.ModifiedBy = 0;
                                        context.DeliveryCanceledRequestHistoryDb.Add(DeliveryCanceledRequestHistoryAdd);
                                    }
                                    else
                                    {
                                        DCR.IsActive = false;
                                        DCR.IsDeleted = true;
                                        context.Entry(DCR).State = EntityState.Modified;
                                        DeliveryCanceledRequestHistory DeliveryCanceledRequestHistoryAdd = new DeliveryCanceledRequestHistory();
                                        DeliveryCanceledRequestHistoryAdd.OrderId = ODM.OrderId;
                                        DeliveryCanceledRequestHistoryAdd.DeliveryCanceledStatus = obj.DeliveryCanceledStatus;
                                        DeliveryCanceledRequestHistoryAdd.Comments = obj.CancelrequestComments;
                                        DeliveryCanceledRequestHistoryAdd.ConformationDate = obj.ConformationDate;
                                        DeliveryCanceledRequestHistoryAdd.CreatedBy = userid;
                                        DeliveryCanceledRequestHistoryAdd.CreatedDate = indianTime;
                                        DeliveryCanceledRequestHistoryAdd.IsActive = true;
                                        DeliveryCanceledRequestHistoryAdd.IsDeleted = false;
                                        DeliveryCanceledRequestHistoryAdd.ModifiedDate = indianTime;
                                        DeliveryCanceledRequestHistoryAdd.ModifiedBy = 0;
                                        context.DeliveryCanceledRequestHistoryDb.Add(DeliveryCanceledRequestHistoryAdd);
                                    }
                                }
                                #endregion

                                //foreach (var detail in ODM.orderDetails)
                                //{
                                //    detail.Status = obj.Status;
                                //    detail.UpdatedDate = indianTime;
                                //    context.Entry(detail).State = EntityState.Modified;

                                //}

                                ODMaster.Status = obj.Status;
                                ODMaster.comments = obj.comments;
                                ODMaster.UpdatedDate = indianTime;
                                ODMaster.ReDispatchCount = ODM.ReDispatchCount;

                                context.Entry(ODMaster).State = EntityState.Modified;

                                foreach (var detail in ODMaster.orderDetails)
                                {
                                    detail.Status = obj.Status;
                                    detail.UpdatedDate = indianTime;
                                    context.Entry(detail).State = EntityState.Modified;

                                }

                                if (ODM.Deliverydate.Date == DateTime.Now.Date)
                                {
                                    MongoDbHelper<DataContracts.Mongo.CustomerRedispatchCharges> mongoDbHelper = new MongoDbHelper<DataContracts.Mongo.CustomerRedispatchCharges>();
                                    var customerRedispatchCharges = mongoDbHelper.Select(x => x.MinValue < ODM.GrossAmount && x.MaxValue >= ODM.GrossAmount && x.WarehouseId == ODM.WarehouseId).FirstOrDefault();

                                    if (customerRedispatchCharges != null && customerRedispatchCharges.RedispatchCharges > 0)
                                    {
                                        Wallet wlt = context.WalletDb.FirstOrDefault(c => c.CustomerId == ODM.CustomerId);
                                        if (wlt != null)
                                        {
                                            CustomerWalletHistory CWH = new CustomerWalletHistory();
                                            CWH.WarehouseId = ODM.WarehouseId;
                                            CWH.CompanyId = ODM.CompanyId;
                                            CWH.CustomerId = wlt.CustomerId;
                                            CWH.NewAddedWAmount = 0;
                                            CWH.NewOutWAmount = -(customerRedispatchCharges.RedispatchCharges * 10);
                                            CWH.OrderId = ODM.OrderId;
                                            CWH.Through = "From Order Redispatched";
                                            CWH.TotalWalletAmount = wlt.TotalAmount - (customerRedispatchCharges.RedispatchCharges * 10);
                                            CWH.CreatedDate = indianTime;
                                            CWH.UpdatedDate = indianTime;
                                            context.CustomerWalletHistoryDb.Add(CWH);

                                            wlt.TotalAmount -= (customerRedispatchCharges.RedispatchCharges * 10);
                                            wlt.TransactionDate = indianTime;
                                            context.Entry(wlt).State = EntityState.Modified;
                                        }
                                    }

                                }
                                //var RO = context.RedispatchWarehouseDb.Where(x => x.OrderId == obj.OrderId && x.DboyMobileNo == obj.DboyMobileNo).FirstOrDefault();
                                //if (RO != null)
                                //{
                                //    RO.Status = obj.Status;
                                //    RO.comments = obj.comments;
                                //    RO.ReDispatchCount = ODM.ReDispatchCount;
                                //    RO.UpdatedDate = indianTime;
                                //    context.Entry(RO).State = EntityState.Modified;

                                //}
                                //else
                                //{
                                //    RO = new RedispatchWarehouse();
                                //    RO.active = true;
                                //    RO.comments = obj.comments;
                                //    RO.CompanyId = ODM.CompanyId;
                                //    RO.CreatedDate = indianTime;
                                //    RO.UpdatedDate = indianTime;
                                //    RO.DboyMobileNo = obj.DboyMobileNo;
                                //    RO.DboyName = obj.DboyName;
                                //    RO.Deleted = false;
                                //    RO.OrderDispatchedMasterId = obj.OrderDispatchedMasterId;
                                //    RO.OrderId = obj.OrderId;
                                //    RO.WarehouseId = obj.WarehouseId;
                                //    RO.ReDispatchCount = ODM.ReDispatchCount;
                                //    RO.Status = obj.Status;
                                //    context.RedispatchWarehouseDb.Add(RO);
                                //}
                            }
                            else if (ODM.Status == "Delivery Redispatch")
                            {
                                Status = true;
                                msg = "Order already Redispatch";
                                res = new ResDTO()
                                {
                                    op = obj,
                                    Message = msg,
                                    Status = Status
                                };
                                ordersToProcess.RemoveAll(x => x == obj.OrderId);
                                return Request.CreateResponse(HttpStatusCode.OK, res);
                            }
                            else if (obj.Status == "Shipped" && ODM.Status != "Shipped")
                            {
                                OrderDeliveryMaster.Status = obj.Status;
                                OrderDeliveryMaster.comments = obj.comments;
                                //OrderDeliveryMaster.ChequeDate = obj.ChequeDate;
                                OrderDeliveryMaster.UpdatedDate = indianTime;
                                OrderDeliveryMaster.DeliveryLat = obj.DeliveryLat;//added on 08/07/02019 
                                OrderDeliveryMaster.DeliveryLng = obj.DeliveryLng;
                                context.Entry(OrderDeliveryMaster).State = EntityState.Modified;
                                //done on 28/02/2020 by PoojaZ //start
                                if (OrderDeliveryMaster.comments == "Dont Want" || OrderDeliveryMaster.comments == "Late Delivery (OPS)" || OrderDeliveryMaster.comments == "Damage Expiry (Pur)")
                                {
                                    string query = "select sp.PeopleID  from OrderMasters a inner join Customers c on a.CustomerId=c.CustomerId and a.orderid = " + OrderDeliveryMaster.OrderId + "  inner join People sp on a.SalesPersonId=sp.PeopleID";
                                    var salesLeadId = context.Database.SqlQuery<int?>(query).FirstOrDefault();
                                    if (salesLeadId.HasValue && salesLeadId.Value > 0)
                                    {
                                        deptOrderCacellation.SalesLeadId = salesLeadId.Value;
                                        deptOrderCacellation.OrderId = OrderDeliveryMaster.OrderId;
                                        deptOrderCacellation.ChargePoint = 1;
                                        deptOrderCacellation.IsActive = true;
                                        deptOrderCacellation.IsDeleted = false;
                                        deptOrderCacellation.IsEmailSend = false;
                                        deptOrderCacellation.CreatedDate = DateTime.Now;
                                        deptOrderCacellation.CreatedBy = People.PeopleID;
                                        if (OrderDeliveryMaster.comments == "Dont Want")
                                        {
                                            deptOrderCacellation.DepId = 6;
                                        }
                                        else if (OrderDeliveryMaster.comments == "Late Delivery (OPS)")
                                        {
                                            deptOrderCacellation.DepId = 33;
                                        }
                                        else if (OrderDeliveryMaster.comments == "Damage Expiry (Pur)")
                                        {
                                            deptOrderCacellation.DepId = 29;
                                        }
                                        context.DeptOrderCancellationDb.Add(deptOrderCacellation);
                                    }
                                }
                                //done on 28/02/2020 by PoojaZ //end

                                if (ODM != null)
                                {
                                    ODM.Status = obj.Status;
                                    ODM.CanceledStatus = obj.Status;
                                    ODM.comments = obj.comments;
                                    ODM.Signimg = obj.Signimg;
                                    ODM.UpdatedDate = indianTime;
                                    ODM.DeliveryLat = obj.DeliveryLat;//added on 08/07/02019 
                                    ODM.DeliveryLng = obj.DeliveryLng;
                                    ODM.DeliveryCanceledComments = obj.CancelrequestComments;
                                    context.Entry(ODM).State = EntityState.Modified;



                                    #region Order Master History for Status Delivery Canceled
                                    OrderMasterHistories OrderMasterHistories = new OrderMasterHistories();
                                    OrderMasterHistories.orderid = ODM.OrderId;
                                    OrderMasterHistories.Status = ODM.Status;
                                    OrderMasterHistories.Reasoncancel = null;
                                    OrderMasterHistories.Warehousename = ODM.WarehouseName;
                                    OrderMasterHistories.DeliveryIssuanceId = obj.DeliveryIssuanceId;
                                    OrderMasterHistories.username = People.DisplayName != null ? People.DisplayName : People.PeopleFirstName; ;
                                    OrderMasterHistories.userid = People.PeopleID;
                                    OrderMasterHistories.CreatedDate = DateTime.Now;
                                    context.OrderMasterHistoriesDB.Add(OrderMasterHistories);
                                    DeliveryCanceledRequestHistory DeliveryCanceledRequestHistoryAdd = new DeliveryCanceledRequestHistory();
                                    DeliveryCanceledRequestHistoryAdd.OrderId = ODM.OrderId;
                                    DeliveryCanceledRequestHistoryAdd.Comments = obj.CancelrequestComments;
                                    DeliveryCanceledRequestHistoryAdd.CreatedBy = userid;
                                    DeliveryCanceledRequestHistoryAdd.CreatedDate = DateTime.Now;
                                    DeliveryCanceledRequestHistoryAdd.IsActive = true;
                                    DeliveryCanceledRequestHistoryAdd.IsDeleted = false;
                                    context.DeliveryCanceledRequestHistoryDb.Add(DeliveryCanceledRequestHistoryAdd);
                                    #endregion
                                    //foreach (var detail in ODM.orderDetails)
                                    //{
                                    //    detail.Status = obj.Status;
                                    //    detail.UpdatedDate = indianTime;
                                    //    context.Entry(detail).State = EntityState.Modified;
                                    //}
                                    ODMaster.Status = obj.Status;
                                    ODMaster.comments = obj.comments;
                                    ODMaster.UpdatedDate = indianTime;
                                    context.Entry(ODMaster).State = EntityState.Modified;
                                    foreach (var detail in ODMaster.orderDetails)
                                    {
                                        detail.Status = obj.Status;
                                        detail.UpdatedDate = indianTime;
                                        context.Entry(detail).State = EntityState.Modified;
                                    }

                                }

                                //}//end dt:-9/3/2020
                            }
                            else if (ODM.Status == "Shipped")
                            {
                                Status = true;
                                msg = "Order already Shipped";
                                res = new ResDTO()
                                {
                                    op = obj,
                                    Message = msg,
                                    Status = Status
                                };
                                ordersToProcess.RemoveAll(x => x == obj.OrderId);

                                return Request.CreateResponse(HttpStatusCode.OK, res);
                            }
                        }
                        else
                        {

                            res = new ResDTO()
                            {
                                op = null,
                                Message = "Not Delivered",
                                Status = Status
                            };
                            ordersToProcess.RemoveAll(x => x == obj.OrderId);
                            return Request.CreateResponse(HttpStatusCode.OK, res);
                        }
                        if (context.Commit() > 0)
                        {
                            #region stock Hit on poc
                            //for currentstock
                            if (ODMaster != null && ODMaster.OrderType != 5 && ODM.Status == "Delivery Canceled")
                            {

                                MultiStockHelper<OnDeliveryCanceledStockEntryDc> MultiStockHelpers = new MultiStockHelper<OnDeliveryCanceledStockEntryDc>();
                                List<OnDeliveryCanceledStockEntryDc> DeliveryCanceledCStockList = new List<OnDeliveryCanceledStockEntryDc>();
                                foreach (var StockHit in ODM.orderDetails.Where(x => x.qty > 0))
                                {
                                    var RefStockCode = ODMaster.OrderType == 8 ? "CL" : "C";
                                    bool isFree = ODMaster.orderDetails.Any(c => c.OrderDetailsId == StockHit.OrderDetailsId && c.IsFreeItem && c.IsDispatchedFreeStock);
                                    if (isFree) { RefStockCode = "F"; }
                                    else if (ODMaster.OrderType == 6) //6 Damage stock
                                    {
                                        RefStockCode = "D";
                                    }
                                    DeliveryCanceledCStockList.Add(new OnDeliveryCanceledStockEntryDc
                                    {
                                        ItemMultiMRPId = StockHit.ItemMultiMRPId,
                                        OrderDispatchedDetailsId = StockHit.OrderDispatchedDetailsId,
                                        OrderId = StockHit.OrderId,
                                        Qty = StockHit.qty,
                                        UserId = People.PeopleID,
                                        WarehouseId = StockHit.WarehouseId,
                                        RefStockCode = RefStockCode,
                                        IsDeliveryRedispatchCancel = false
                                    });
                                }
                                if (DeliveryCanceledCStockList.Any())
                                {
                                    bool IsUpdated = MultiStockHelpers.MakeEntry(DeliveryCanceledCStockList, "Stock_OnDCRTODC_New", context, dbContextTransaction);
                                    if (!IsUpdated)
                                    {
                                        Status = false;
                                        dbContextTransaction.Dispose();
                                        res = new ResDTO()
                                        {
                                            op = null,
                                            Message = "Order Not Delivery Canceled",
                                            Status = Status
                                        };
                                        ordersToProcess.RemoveAll(x => x == obj.OrderId);
                                        return Request.CreateResponse(HttpStatusCode.OK, res);
                                    }
                                }

                            }
                            else if (ODMaster != null && ODMaster.OrderType != 5 && ODM.Status == "Delivery Redispatch")
                            {

                                MultiStockHelper<OnDeliveryRedispatchedStockEntryDc> MultiStockHelpers = new MultiStockHelper<OnDeliveryRedispatchedStockEntryDc>();
                                List<OnDeliveryRedispatchedStockEntryDc> DeliveryRedispatchedCStockList = new List<OnDeliveryRedispatchedStockEntryDc>();
                                foreach (var StockHit in ODM.orderDetails.Where(x => x.qty > 0))
                                {
                                    var RefStockCode = ODMaster.OrderType == 8 ? "CL" : "C"; ;
                                    bool isFree = ODMaster.orderDetails.Any(c => c.OrderDetailsId == StockHit.OrderDetailsId && c.IsFreeItem && c.IsDispatchedFreeStock);
                                    if (isFree) { RefStockCode = "F"; }
                                    else if (ODMaster.OrderType == 6) //6 Damage stock
                                    {
                                        RefStockCode = "D";
                                    }
                                    DeliveryRedispatchedCStockList.Add(new OnDeliveryRedispatchedStockEntryDc
                                    {
                                        ItemMultiMRPId = StockHit.ItemMultiMRPId,
                                        OrderDispatchedDetailsId = StockHit.OrderDispatchedDetailsId,
                                        OrderId = StockHit.OrderId,
                                        Qty = StockHit.qty,
                                        UserId = People.PeopleID,
                                        WarehouseId = StockHit.WarehouseId,
                                        RefStockCode = RefStockCode,
                                        IsDeliveryCancel = false
                                    });
                                }
                                if (DeliveryRedispatchedCStockList.Any())
                                {
                                    bool IsUpdated = MultiStockHelpers.MakeEntry(DeliveryRedispatchedCStockList, "Stock_OnDCRTODR_New", context, dbContextTransaction);
                                    if (!IsUpdated)
                                    {
                                        Status = false;
                                        dbContextTransaction.Dispose();
                                        res = new ResDTO()
                                        {
                                            op = null,
                                            Message = "Order Not Delivery Redispatch",
                                            Status = Status
                                        };
                                        ordersToProcess.RemoveAll(x => x == obj.OrderId);
                                        return Request.CreateResponse(HttpStatusCode.OK, res);
                                    }
                                }

                            }
                            else if (ODMaster != null && ODMaster.OrderType != 5 && ODM.Status == "Shipped")
                            {

                                MultiStockHelper<OnDeliveryRedispatchedStockEntryDc> MultiStockHelpers = new MultiStockHelper<OnDeliveryRedispatchedStockEntryDc>();
                                List<OnDeliveryRedispatchedStockEntryDc> DeliveryRedispatchedCStockList = new List<OnDeliveryRedispatchedStockEntryDc>();
                                foreach (var StockHit in ODM.orderDetails.Where(x => x.qty > 0))
                                {
                                    var RefStockCode = ODMaster.OrderType == 8 ? "CL" : "C";
                                    bool isFree = ODMaster.orderDetails.Any(c => c.OrderDetailsId == StockHit.OrderDetailsId && c.IsFreeItem && c.IsDispatchedFreeStock);
                                    if (isFree) { RefStockCode = "F"; }
                                    else if (ODMaster.OrderType == 6) //6 Damage stock
                                    {
                                        RefStockCode = "D";
                                    }
                                    DeliveryRedispatchedCStockList.Add(new OnDeliveryRedispatchedStockEntryDc
                                    {
                                        ItemMultiMRPId = StockHit.ItemMultiMRPId,
                                        OrderDispatchedDetailsId = StockHit.OrderDispatchedDetailsId,
                                        OrderId = StockHit.OrderId,
                                        Qty = StockHit.qty,
                                        UserId = People.PeopleID,
                                        WarehouseId = StockHit.WarehouseId,
                                        RefStockCode = RefStockCode,
                                        IsDeliveryCancel = false
                                    });
                                }
                                if (DeliveryRedispatchedCStockList.Any())
                                {
                                    bool IsUpdated = MultiStockHelpers.MakeEntry(DeliveryRedispatchedCStockList, "Stock_OnDCRTOShipped_New", context, dbContextTransaction);
                                    if (!IsUpdated)
                                    {
                                        Status = false;
                                        dbContextTransaction.Dispose();
                                        res = new ResDTO()
                                        {
                                            op = null,
                                            Message = "Order Not Shipped",
                                            Status = Status
                                        };
                                        ordersToProcess.RemoveAll(x => x == obj.OrderId);
                                        return Request.CreateResponse(HttpStatusCode.OK, res);
                                    }
                                }

                            }
                            #endregion
                            dbContextTransaction.Complete();
                            Status = true;
                            if (IsDeliverSmsSent && !string.IsNullOrEmpty(ODMaster.Customerphonenum))
                            {
                                // string message = "Hi " + ODMaster.CustomerName + " Your Order #" + ODMaster.OrderId + " is delivered on time if you have any complaint regarding your order kindly contact our customer care within next 1 Hours.";
                                string message = ""; //"Hi {#var1#} Your Order {#var2#} is delivered on time if you have any complaint regarding your order kindly contact our customer care within {#var3#}. ShopKirana";
                                var dltSMS = SMSTemplateHelper.getTemplateText((int)AppEnum.RetailerApp, "Delivered_Order_Compaint");
                                message = dltSMS == null ? "" : dltSMS.Template;

                                message = message.Replace("{#var1#}", ODMaster.CustomerName);
                                message = message.Replace("{#var2#}", ODMaster.OrderId.ToString());
                                message = message.Replace("{#var3#}", " next 1 Hours");
                                if (dltSMS != null)
                                    Common.Helpers.SendSMSHelper.SendSMS(ODMaster.Customerphonenum, message, ((Int32)Common.Enums.SMSRouteEnum.Transactional).ToString(), dltSMS.DLTId);

                            }

                            if (ODMaster != null && ODMaster.OrderType == 5 && obj.Status == "Delivered")
                            {
                                try
                                {
                                    UpdateConsumerOrders updateConsumerOrders = new UpdateConsumerOrders
                                    {
                                        CartStatus = "Delivered",
                                        InvoiceNo = ODMaster.invoice_no
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
                            #region Sellerstock update
                            if (ODMaster != null && ODMaster.CustomerType == "SellerStore" && obj.Status == "Delivered")
                            {
                                UpdateSellerStockOfCFRProduct Postobj = new UpdateSellerStockOfCFRProduct();
                                Postobj.OrderId = ODM.OrderId;
                                Postobj.Skcode = ODM.Skcode;
                                Postobj.ItemDetailDc = new List<SellerItemDetailDc>();
                                foreach (var item in ODM.orderDetails)
                                {
                                    SellerItemDetailDc newitem = new SellerItemDetailDc();
                                    newitem.ItemMultiMrpId = item.ItemMultiMRPId;
                                    newitem.SellingPrice = item.UnitPrice;
                                    newitem.qty = item.qty;
                                    Postobj.ItemDetailDc.Add(newitem);
                                }
                                BackgroundTaskManager.Run(() =>
                                {
                                    try
                                    {
                                        var tradeUrl = ConfigurationManager.AppSettings["SellerURL"] + "/api/sk/RetailerAppApi/UpdateStockOnDeliveryFromSkApp";
                                        using (GenericRestHttpClient<UpdateSellerStockOfCFRProduct, string> memberClient = new GenericRestHttpClient<UpdateSellerStockOfCFRProduct, string>(tradeUrl, "", null))
                                        {
                                            AsyncContext.Run(() => memberClient.PostAsync(Postobj));
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        TextFileLogHelper.LogError("Error while Update Seller Stock Of CFR Product: " + ex.ToString());
                                    }
                                });
                            }
                            #endregion
                        }
                        else
                        {
                            dbContextTransaction.Dispose();
                            Status = false;
                        }
                        res = new ResDTO()
                        {
                            op = obj,
                            Message = "Success",
                            Status = Status
                        };
                        ordersToProcess.RemoveAll(x => x == obj.OrderId);
                        return Request.CreateResponse(HttpStatusCode.OK, res);
                    }
                }
            }
            catch (Exception ex)
            {
                ordersToProcess.RemoveAll(x => x == obj.OrderId);
                throw ex;
            }

        }
        [AllowAnonymous]
        [Route("SendOrderCancellationEmail")]
        [HttpGet]
        public bool SendOrderCancellationEmail()
        {
            bool result = false;
            using (var context = new AuthContext())
            {
                //var deptOrderCancellations = context.DeptOrderCancellationDb.Where(c => c.IsActive == true && c.IsDeleted == false && !c.IsEmailSend && EntityFunctions.TruncateTime(c.CreatedDate) == EntityFunctions.TruncateTime(DateTime.Now)).ToList();
                var deptOrderCancellations = context.DeptOrderCancellationDb.Where(c => c.IsActive == true && c.IsDeleted == false && !c.IsEmailSend).ToList();
                var peopleids = deptOrderCancellations.Select(x => x.SalesLeadId).Distinct().ToList();
                var Users = context.Peoples.Where(x => peopleids.Contains(x.PeopleID) && x.Active == true).Select(x => new { x.Mobile, x.Email, x.PeopleID }).ToList();
                foreach (var user in Users.Where(x => !string.IsNullOrEmpty(x.Email)))
                {
                    var orderList = deptOrderCancellations.Where(x => x.SalesLeadId == user.PeopleID).Select(x => x.OrderId).ToList();
                    string msg = "Report Of Cancellation of All Orders at Same day Delivery is listed as - : " + string.Join(",", orderList);
                    var status = Common.Helpers.EmailHelper.SendMail("donotreply_backend@shopkirana.com", user.Email, "", "Order Cancellation Detail for :" + DateTime.Now.ToString("dd-MM-yyyy"), msg, "");
                }
                foreach (var item in deptOrderCancellations)
                {
                    item.IsEmailSend = true;
                    item.ModifiedDate = DateTime.Now;
                    context.Entry(item).State = EntityState.Modified;
                }
                context.Commit();
                result = true;
            }
            return result;
        }


        [Route("PostDeclineOrder")]
        [HttpPost]
        [AllowAnonymous]
        public HttpResponseMessage PostDeclineOrder(OrderPlaceDTO obj) //Order delivered or canceled from delivery app
        {
            TripPlannerHelper tripPlannerHelper = new TripPlannerHelper();
            string OldStatus = "";
            ResDTO res;
            bool Status = false;
            List<string> chequenos = new List<string>();
            double totalChequeamt = 0;
            List<string> MposGatId = new List<string>();
            double totalMposamount = 0;
            List<string> BankNames = new List<string>();
            TransactionOptions option = new TransactionOptions();
            option.IsolationLevel = IsolationLevel.RepeatableRead;
            option.Timeout = TimeSpan.FromSeconds(90);
            Guid guid = Guid.NewGuid();
            var guidData = guid.ToString();
            using (var dbContextTransaction = new TransactionScope(TransactionScopeOption.Required, option))
            {
                using (var context = new AuthContext())
                {

                    var identity = User.Identity as ClaimsIdentity;
                    int userid = 0;
                    if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                        userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

                    OrderMaster ODMaster = null;
                    var People = context.Peoples.Where(x => x.Mobile == obj.DboyMobileNo).FirstOrDefault();
                    var DeliveryIssuance = context.DeliveryIssuanceDb.Where(x => x.DeliveryIssuanceId == obj.DeliveryIssuanceId).SingleOrDefault();
                    var ODM = context.OrderDispatchedMasters.Where(x => x.OrderDispatchedMasterId == obj.OrderDispatchedMasterId).Include("orderDetails").SingleOrDefault();

                    if (obj != null && People != null && DeliveryIssuance.DeliveryIssuanceId > 0 && ODM != null)
                    {
                        OldStatus = ODM.Status;

                        var OrderDeliveryMaster = context.OrderDeliveryMasterDB.FirstOrDefault(z => z.DeliveryIssuanceId == obj.DeliveryIssuanceId && z.OrderId == obj.OrderId);
                        ODMaster = context.DbOrderMaster.Where(x => x.OrderId == obj.OrderId).Include("orderDetails").SingleOrDefault();
                        DeptOrderCancellation deptOrderCacellation = new DeptOrderCancellation();
                        deptOrderCacellation.OrderId = obj.OrderId;
                        var AssignmentRechangeOrder = context.AssignmentRechangeOrder.FirstOrDefault(x => x.AssignmentId == obj.DeliveryIssuanceId && x.OrderId == obj.OrderId);
                        if (AssignmentRechangeOrder != null)
                        {
                            AssignmentRechangeOrder.Status = 0;
                            AssignmentRechangeOrder.ModifiedDate = indianTime;
                            AssignmentRechangeOrder.ModifiedBy = People.PeopleID;
                            context.Entry(AssignmentRechangeOrder).State = EntityState.Modified;
                        }

                        #region Damage Order status Update
                        if (ODMaster.OrderType == 6 && ODM.invoice_no != null)
                        {
                            var DOM = context.DamageOrderMasterDB.Where(x => x.invoice_no == ODM.invoice_no).SingleOrDefault();

                            if (DOM != null)
                            {
                                DOM.UpdatedDate = indianTime;
                                DOM.Status = obj.Status;
                                context.Entry(DOM).State = EntityState.Modified;
                            }
                        }
                        #endregion

                        if (obj.Status == "Delivered")
                        {
                            if (ODM.CustomerId > 0 && obj.CashAmount > 0)
                            {
                                bool CheckCashAmount = tripPlannerHelper.CheckTodayCustomerCashAmount(ODM.CustomerId, obj.CashAmount, null, context);
                                if (CheckCashAmount)
                                {
                                    Status = false;
                                    string msg = "Alert! You cannot exceed the cash limit of 2 lacs for this customer in a day.";
                                    res = new ResDTO()
                                    {
                                        op = obj,
                                        Message = msg,
                                        Status = Status
                                    };
                                    ordersToProcess.RemoveAll(x => x == obj.OrderId);
                                    return Request.CreateResponse(HttpStatusCode.OK, res);
                                }
                            }
                            var PaymentResponseRetailerAppList = context.PaymentResponseRetailerAppDb.Where(z => z.OrderId == obj.OrderId && z.status == "Success").ToList();
                            #region Payments 
                            foreach (var orderdata in obj.DeliveryPayments)
                            {
                                var ChequeOldEntries = PaymentResponseRetailerAppList.Where(z => z.OrderId == obj.OrderId && z.PaymentFrom == "Cheque" && z.status == "Success").ToList();
                                if (ChequeOldEntries != null && ChequeOldEntries.Any())
                                {
                                    foreach (var Cheque in ChequeOldEntries)
                                    {
                                        Cheque.status = "Failed";
                                        Cheque.statusDesc = "Due to Order decline Cheque request from DeliveryApp";
                                        context.Entry(Cheque).State = EntityState.Modified;
                                    }
                                }
                                if (orderdata.PaymentFrom == "Cheque" && orderdata.amount > 0)
                                {
                                    context.PaymentResponseRetailerAppDb.Add(new PaymentResponseRetailerApp
                                    {
                                        OrderId = obj.OrderId,
                                        status = "Success",
                                        CreatedDate = indianTime,
                                        UpdatedDate = indianTime,
                                        PaymentFrom = "Cheque",
                                        statusDesc = "Due to Delivery",
                                        amount = Math.Round(orderdata.amount, 0),
                                        GatewayTransId = orderdata.TransId,
                                        ChequeBankName = orderdata.ChequeBankName,
                                        ChequeImageUrl = orderdata.ChequeImageUrl
                                    });

                                    chequenos.Add(orderdata.TransId);
                                    totalChequeamt += Math.Round(orderdata.amount, 0);
                                }

                                var mPosOldEntries = PaymentResponseRetailerAppList.Where(z => z.OrderId == obj.OrderId && z.PaymentFrom == "mPos" && z.status == "Success").ToList();
                                if (mPosOldEntries != null && mPosOldEntries.Any())
                                {
                                    foreach (var mPosentry in mPosOldEntries)
                                    {
                                        mPosentry.status = "Failed";
                                        mPosentry.statusDesc = "Due to Order decline mpose request from DeliveryApp";
                                        context.Entry(mPosentry).State = EntityState.Modified;
                                    }
                                }

                                if (orderdata.PaymentFrom == "mPos" && orderdata.amount > 0)
                                {
                                    context.PaymentResponseRetailerAppDb.Add(new PaymentResponseRetailerApp
                                    {
                                        OrderId = obj.OrderId,
                                        status = "Success",
                                        CreatedDate = indianTime,
                                        UpdatedDate = indianTime,
                                        PaymentFrom = "mPos",
                                        statusDesc = "Due to Delivery",
                                        amount = Math.Round(orderdata.amount, 0),
                                        GatewayTransId = orderdata.TransId,
                                        IsOnline = true
                                    });
                                    MposGatId.Add(orderdata.TransId);
                                    totalMposamount += Math.Round(orderdata.amount, 0);
                                }
                                #region Add for RTGS/NEFT
                                // New RTGS Add
                                var RTGSOldEntries = PaymentResponseRetailerAppList.Where(z => z.OrderId == obj.OrderId && z.PaymentFrom.ToUpper() == "RTGS/NEFT" && z.status == "Success").ToList();
                                if (RTGSOldEntries != null && RTGSOldEntries.Any())
                                {
                                    foreach (var RTGSentry in RTGSOldEntries)
                                    {
                                        RTGSentry.status = "Failed";
                                        RTGSentry.statusDesc = "Due to Order decline RTGS/NEFT request from DeliveryApp";
                                        context.Entry(RTGSentry).State = EntityState.Modified;
                                    }
                                }
                                if (orderdata.PaymentFrom.ToUpper() == "RTGS/NEFT" && orderdata.amount > 0)
                                {
                                    context.PaymentResponseRetailerAppDb.Add(new PaymentResponseRetailerApp
                                    {
                                        OrderId = obj.OrderId,
                                        status = "Success",
                                        CreatedDate = indianTime,
                                        UpdatedDate = indianTime,
                                        PaymentFrom = "RTGS/NEFT",
                                        statusDesc = "Due to Delivery",
                                        amount = Math.Round(orderdata.amount, 0),
                                        GatewayTransId = orderdata.TransId,
                                        IsOnline = true
                                    });
                                    MposGatId.Add(orderdata.TransId);
                                    totalMposamount += Math.Round(orderdata.amount, 0);
                                }
                                #endregion
                            }

                            if (obj.DeliveryPayments == null || !obj.DeliveryPayments.Any())
                            {
                                var ChequeOldEntries = PaymentResponseRetailerAppList.Where(z => z.OrderId == obj.OrderId && z.PaymentFrom == "Cheque" && z.status == "Success").ToList();
                                if (ChequeOldEntries != null && ChequeOldEntries.Any())
                                {
                                    foreach (var Cheque in ChequeOldEntries)
                                    {
                                        Cheque.status = "Failed";
                                        Cheque.statusDesc = "Due to Order decline Cheque request from DeliveryApp";
                                        context.Entry(Cheque).State = EntityState.Modified;
                                    }
                                }
                                var mPosOldEntries = PaymentResponseRetailerAppList.Where(z => z.OrderId == obj.OrderId && z.PaymentFrom == "mPos" && z.status == "Success").ToList();
                                if (mPosOldEntries != null && mPosOldEntries.Any())
                                {
                                    foreach (var mPosentry in mPosOldEntries)
                                    {
                                        mPosentry.status = "Failed";
                                        mPosentry.statusDesc = "Due to Order decline mpose request from DeliveryApp";
                                        context.Entry(mPosentry).State = EntityState.Modified;
                                    }
                                }
                                // New RTGS Add
                                var RTGSOldEntries = PaymentResponseRetailerAppList.Where(z => z.OrderId == obj.OrderId && z.PaymentFrom.ToUpper() == "RTGS/NEFT" && z.status == "Success").ToList();
                                if (RTGSOldEntries != null && RTGSOldEntries.Any())
                                {
                                    foreach (var RTGSentry in RTGSOldEntries)
                                    {
                                        RTGSentry.status = "Failed";
                                        RTGSentry.statusDesc = "Due to Order decline RTGS/NEFT request from DeliveryApp";
                                        context.Entry(RTGSentry).State = EntityState.Modified;
                                    }
                                }
                            }
                            #region refund
                            var OnlinePaymentResponseRetailer = new List<RetailerOrderPaymentDc>();
                            foreach (var item in PaymentResponseRetailerAppList.Where(x => x.IsOnline == true && (x.PaymentFrom != "mPos" && x.PaymentFrom != "RTGS/NEFT")).GroupBy(x => new { x.OrderId, x.PaymentFrom, x.GatewayTransId, x.IsOnline, x.status }))
                            {
                                OnlinePaymentResponseRetailer.Add(new RetailerOrderPaymentDc
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
                            var othermodeAmt = OnlinePaymentResponseRetailer.Where(x => x.IsOnline).Sum(x => x.amount);
                            #endregion
                            // var othermodeAmt = PaymentResponseRetailerAppList.Where(x => x.IsOnline && (x.PaymentFrom != "mPos" && x.PaymentFrom != "RTGS/NEFT")  /*x.PaymentFrom == "hdfc" || x.PaymentFrom == "ePaylater" || x.PaymentFrom == "Gullak"*/).Sum(x => x.amount);
                            var totalAmount = totalChequeamt + totalMposamount + othermodeAmt;
                            var cashpayment = PaymentResponseRetailerAppList.FirstOrDefault(x => x.OrderId == obj.OrderId && x.status == "Success" && x.PaymentFrom == "Cash");
                            obj.CashAmount = obj.CashAmount > 0 ? obj.CashAmount : (cashpayment != null ? cashpayment.amount : 0);

                            if (ODM.GrossAmount != obj.CashAmount + totalAmount)
                            {
                                obj.CashAmount = ODM.GrossAmount - totalAmount;
                            }
                            if (cashpayment != null)
                            {
                                cashpayment.amount = obj.CashAmount;
                                cashpayment.UpdatedDate = indianTime;
                                cashpayment.status = obj.CashAmount > 0 ? cashpayment.status : "Failed";
                                cashpayment.statusDesc = "Due to Delivery";
                                context.Entry(cashpayment).State = EntityState.Modified;
                            }
                            else if (cashpayment == null && obj.CashAmount > 0)
                            {
                                context.PaymentResponseRetailerAppDb.Add(new PaymentResponseRetailerApp
                                {
                                    OrderId = obj.OrderId,
                                    status = "Success",
                                    CreatedDate = indianTime,
                                    UpdatedDate = indianTime,
                                    statusDesc = "Due to Delivery",
                                    PaymentFrom = "Cash",
                                    amount = Math.Round(obj.CashAmount, 0)
                                });
                            }

                            #endregion
                            #region OrderDeliveryMaster
                            OrderDeliveryMaster.Status = obj.Status;
                            OrderDeliveryMaster.comments = obj.comments;
                            OrderDeliveryMaster.RecivedAmount = obj.RecivedAmount;
                            OrderDeliveryMaster.UpdatedDate = indianTime;
                            OrderDeliveryMaster.DeliveryLat = obj.DeliveryLat;
                            OrderDeliveryMaster.DeliveryLng = obj.DeliveryLng; //added on 08/07/02019                              
                            context.Entry(OrderDeliveryMaster).State = EntityState.Modified;
                            #endregion
                            ODM.Status = obj.Status;
                            ODM.ReDispatchedStatus = obj.Status;
                            ODM.comments = obj.comments;
                            ODM.RecivedAmount = obj.RecivedAmount;
                            ODM.Signimg = obj.Signimg;
                            ODM.UpdatedDate = indianTime;


                            ODM.DeliveryLat = obj.DeliveryLat;//added on 08/07/02019 
                            ODM.DeliveryLng = obj.DeliveryLng;
                            context.Entry(ODM).State = EntityState.Modified;

                            //#endregion
                            #region Order Master History for Status Delivered

                            OrderMasterHistories OrderMasterHistories = new OrderMasterHistories();
                            OrderMasterHistories.orderid = ODM.OrderId;
                            OrderMasterHistories.Status = ODM.Status;
                            OrderMasterHistories.Reasoncancel = null;
                            OrderMasterHistories.Warehousename = ODM.WarehouseName;
                            OrderMasterHistories.DeliveryIssuanceId = obj.DeliveryIssuanceId;//by sudhir 06-06-2019
                            OrderMasterHistories.username = People.DisplayName != null ? People.DisplayName : People.PeopleFirstName; ;
                            OrderMasterHistories.userid = People.PeopleID;
                            OrderMasterHistories.CreatedDate = indianTime;
                            context.OrderMasterHistoriesDB.Add(OrderMasterHistories);
                            #endregion

                            ODMaster.Status = "Delivered";
                            ODMaster.comments = obj.comments;
                            ODMaster.DeliveredDate = indianTime;
                            ODMaster.UpdatedDate = indianTime;
                            context.Entry(ODMaster).State = EntityState.Modified;

                            foreach (var detail in ODMaster.orderDetails)
                            {
                                detail.Status = obj.Status;
                                detail.UpdatedDate = indianTime;
                                context.Entry(detail).State = EntityState.Modified;
                            }
                            string OrderConcernMessageFlag = Convert.ToString(ConfigurationManager.AppSettings["OrderConcernMessageFlag"]);
                            if (!string.IsNullOrEmpty(OrderConcernMessageFlag) && OrderConcernMessageFlag == "Y")
                            {
                                #region OrderConcern                     
                                context.OrderConcernDB.Add(new Model.CustomerDelight.OrderConcern()
                                {
                                    IsActive = true,
                                    IsDeleted = false,
                                    CreatedDate = DateTime.Now,
                                    LinkId = guidData,
                                    OrderId = obj.OrderId,
                                    CreatedBy = userid,
                                    //Status = "Open",
                                    IsCustomerRaiseConcern = false
                                });
                                #endregion
                            }

                        }
                        else if (obj.Status == "Delivery Canceled")
                        {
                            OrderDeliveryMaster.Status = obj.Status;
                            OrderDeliveryMaster.comments = obj.comments;
                            //OrderDeliveryMaster.ChequeDate = obj.ChequeDate;
                            OrderDeliveryMaster.UpdatedDate = indianTime;
                            OrderDeliveryMaster.DeliveryLat = obj.DeliveryLat;//added on 08/07/02019 
                            OrderDeliveryMaster.DeliveryLng = obj.DeliveryLng;
                            context.Entry(OrderDeliveryMaster).State = EntityState.Modified;
                            if (ODM != null)
                            {
                                ODM.Status = obj.Status;
                                ODM.CanceledStatus = obj.Status;
                                ODM.comments = obj.comments;
                                ODM.Signimg = obj.Signimg;
                                ODM.UpdatedDate = indianTime;
                                ODM.DeliveryLat = obj.DeliveryLat;//added on 08/07/02019 
                                ODM.DeliveryLng = obj.DeliveryLng;
                                context.Entry(ODM).State = EntityState.Modified;
                                if (OrderDeliveryMaster.comments == "Dont Want" || OrderDeliveryMaster.comments == "Late Delivery (OPS)" || OrderDeliveryMaster.comments == "Damage Expiry (Pur)")
                                {
                                    string query = "select sp.PeopleID  from OrderMasters a inner join Customers c on a.CustomerId=c.CustomerId and a.orderid = " + OrderDeliveryMaster.OrderId + "  inner join People sp on a.SalesPersonId=sp.PeopleID";
                                    var salesLeadId = context.Database.SqlQuery<int?>(query).FirstOrDefault();
                                    if (salesLeadId.HasValue && salesLeadId.Value > 0)
                                    {
                                        deptOrderCacellation.SalesLeadId = salesLeadId.Value;
                                        deptOrderCacellation.OrderId = OrderDeliveryMaster.OrderId;
                                        deptOrderCacellation.ChargePoint = 1;
                                        deptOrderCacellation.IsActive = true;
                                        deptOrderCacellation.IsDeleted = false;
                                        deptOrderCacellation.IsEmailSend = false;
                                        deptOrderCacellation.CreatedDate = DateTime.Now;
                                        deptOrderCacellation.CreatedBy = userid;
                                        if (OrderDeliveryMaster.comments == "Dont Want")
                                        {
                                            deptOrderCacellation.DepId = 6;
                                        }
                                        else if (OrderDeliveryMaster.comments == "Late Delivery (OPS)")
                                        {
                                            deptOrderCacellation.DepId = 33;
                                        }
                                        else if (OrderDeliveryMaster.comments == "Damage Expiry (Pur)")
                                        {
                                            deptOrderCacellation.DepId = 29;
                                        }
                                        context.DeptOrderCancellationDb.Add(deptOrderCacellation);
                                    }
                                }
                                //pz//start

                                if (OrderDeliveryMaster.comments == "Dont Want" || OrderDeliveryMaster.comments == "Price Issue (sales)")
                                {
                                    if (!context.CustomerWalletHistoryDb.Any(x => x.OrderId == ODM.OrderId && x.NewOutWAmount == -100 && x.Through == "From Order Cancelled"))
                                    {
                                        Wallet wlt = context.WalletDb.Where(c => c.CustomerId == ODM.CustomerId).SingleOrDefault();

                                        CustomerWalletHistory CWH = new CustomerWalletHistory();
                                        CWH.WarehouseId = ODM.WarehouseId;
                                        CWH.CompanyId = ODM.CompanyId;
                                        CWH.CustomerId = wlt.CustomerId;
                                        CWH.NewAddedWAmount = 0;
                                        CWH.NewOutWAmount = -100;
                                        CWH.OrderId = ODM.OrderId;
                                        CWH.Through = "From Order Cancelled";
                                        CWH.TotalWalletAmount = wlt.TotalAmount - 100;
                                        CWH.CreatedDate = indianTime;
                                        CWH.UpdatedDate = indianTime;
                                        context.CustomerWalletHistoryDb.Add(CWH);

                                        wlt.TotalAmount -= 100;
                                        wlt.TransactionDate = indianTime;
                                        context.Entry(wlt).State = EntityState.Modified;
                                    }
                                }
                                ///pz///     
                                #region Order Master History for Status Delivery Canceled
                                OrderMasterHistories OrderMasterHistories = new OrderMasterHistories();
                                OrderMasterHistories.orderid = ODM.OrderId;
                                OrderMasterHistories.Status = ODM.Status;
                                OrderMasterHistories.Reasoncancel = null;
                                OrderMasterHistories.Warehousename = ODM.WarehouseName;
                                OrderMasterHistories.DeliveryIssuanceId = obj.DeliveryIssuanceId;
                                OrderMasterHistories.username = People.DisplayName != null ? People.DisplayName : People.PeopleFirstName; ;
                                OrderMasterHistories.userid = People.PeopleID;
                                OrderMasterHistories.CreatedDate = DateTime.Now;
                                context.OrderMasterHistoriesDB.Add(OrderMasterHistories);
                                #endregion
                                //foreach (var detail in ODM.orderDetails)
                                //{
                                //    detail.Status = obj.Status;
                                //    detail.UpdatedDate = indianTime;
                                //    context.Entry(detail).State = EntityState.Modified;
                                //}
                                ODMaster.Status = obj.Status;
                                ODMaster.comments = obj.comments;
                                ODMaster.UpdatedDate = indianTime;
                                context.Entry(ODMaster).State = EntityState.Modified;
                                foreach (var detail in ODMaster.orderDetails)
                                {
                                    detail.Status = obj.Status;
                                    detail.UpdatedDate = indianTime;
                                    context.Entry(detail).State = EntityState.Modified;
                                }

                            }
                        }
                        else if (obj.Status == "Delivery Canceled Request")
                        {
                            OrderDeliveryMaster.Status = obj.Status;
                            OrderDeliveryMaster.comments = obj.comments;
                            //OrderDeliveryMaster.ChequeDate = obj.ChequeDate;
                            OrderDeliveryMaster.UpdatedDate = indianTime;
                            OrderDeliveryMaster.DeliveryLat = obj.DeliveryLat;//added on 08/07/02019 
                            OrderDeliveryMaster.DeliveryLng = obj.DeliveryLng;
                            context.Entry(OrderDeliveryMaster).State = EntityState.Modified;
                            if (ODM != null)
                            {
                                ODM.Status = obj.Status;
                                ODM.CanceledStatus = obj.Status;
                                ODM.comments = obj.comments;
                                ODM.Signimg = obj.Signimg;
                                ODM.UpdatedDate = indianTime;
                                ODM.DeliveryLat = obj.DeliveryLat;//added on 08/07/02019 
                                ODM.DeliveryLng = obj.DeliveryLng;
                                context.Entry(ODM).State = EntityState.Modified;
                                if (OrderDeliveryMaster.comments == "Dont Want" || OrderDeliveryMaster.comments == "Late Delivery (OPS)" || OrderDeliveryMaster.comments == "Damage Expiry (Pur)")
                                {
                                    string query = "select sp.PeopleID  from OrderMasters a inner join Customers c on a.CustomerId=c.CustomerId and a.orderid = " + OrderDeliveryMaster.OrderId + "  inner join People sp on a.SalesPersonId=sp.PeopleID";
                                    var salesLeadId = context.Database.SqlQuery<int?>(query).FirstOrDefault();
                                    if (salesLeadId.HasValue && salesLeadId.Value > 0)
                                    {
                                        deptOrderCacellation.SalesLeadId = salesLeadId.Value;
                                        deptOrderCacellation.OrderId = OrderDeliveryMaster.OrderId;
                                        deptOrderCacellation.ChargePoint = 1;
                                        deptOrderCacellation.IsActive = true;
                                        deptOrderCacellation.IsDeleted = false;
                                        deptOrderCacellation.IsEmailSend = false;
                                        deptOrderCacellation.CreatedDate = DateTime.Now;
                                        deptOrderCacellation.CreatedBy = userid;
                                        if (OrderDeliveryMaster.comments == "Dont Want")
                                        {
                                            deptOrderCacellation.DepId = 6;
                                        }
                                        else if (OrderDeliveryMaster.comments == "Late Delivery (OPS)")
                                        {
                                            deptOrderCacellation.DepId = 33;
                                        }
                                        else if (OrderDeliveryMaster.comments == "Damage Expiry (Pur)")
                                        {
                                            deptOrderCacellation.DepId = 29;
                                        }
                                        context.DeptOrderCancellationDb.Add(deptOrderCacellation);
                                    }
                                }

                                #region Order Master History for Status Delivery Canceled
                                OrderMasterHistories OrderMasterHistories = new OrderMasterHistories();
                                OrderMasterHistories.orderid = ODM.OrderId;
                                OrderMasterHistories.Status = ODM.Status;
                                OrderMasterHistories.Reasoncancel = null;
                                OrderMasterHistories.Warehousename = ODM.WarehouseName;
                                OrderMasterHistories.DeliveryIssuanceId = obj.DeliveryIssuanceId;
                                OrderMasterHistories.username = People.DisplayName != null ? People.DisplayName : People.PeopleFirstName; ;
                                OrderMasterHistories.userid = People.PeopleID;
                                OrderMasterHistories.CreatedDate = DateTime.Now;
                                context.OrderMasterHistoriesDB.Add(OrderMasterHistories);
                                #endregion
                                //foreach (var detail in ODM.orderDetails)
                                //{
                                //    detail.Status = obj.Status;
                                //    detail.UpdatedDate = indianTime;
                                //    context.Entry(detail).State = EntityState.Modified;
                                //}
                                ODMaster.Status = obj.Status;
                                ODMaster.comments = obj.comments;
                                ODMaster.UpdatedDate = indianTime;
                                context.Entry(ODMaster).State = EntityState.Modified;
                                foreach (var detail in ODMaster.orderDetails)
                                {
                                    detail.Status = obj.Status;
                                    detail.UpdatedDate = indianTime;
                                    context.Entry(detail).State = EntityState.Modified;
                                }

                            }
                        }
                        else if (obj.Status == "Delivery Redispatch")
                        {
                            OrderDeliveryMaster.Status = obj.Status;
                            OrderDeliveryMaster.comments = obj.comments;
                            OrderDeliveryMaster.UpdatedDate = indianTime;
                            OrderDeliveryMaster.DeliveryLat = obj.DeliveryLat;//added on 08/07/02019 
                            OrderDeliveryMaster.DeliveryLng = obj.DeliveryLng;
                            context.Entry(OrderDeliveryMaster).State = EntityState.Modified;
                            ODM.Status = "Delivery Redispatch";
                            ODM.ReDispatchedStatus = "Delivery Redispatch";
                            ODM.ReDispatchCount = ODM.ReDispatchCount + 1;
                            ODM.Signimg = obj.Signimg;
                            ODM.comments = obj.comments;
                            ODM.UpdatedDate = indianTime;
                            ODM.DeliveryLat = obj.DeliveryLat;//added on 08/07/02019 
                            ODM.DeliveryLng = obj.DeliveryLng;
                            ODM.ReDispatchedDate = obj.ReDispatchedDate;
                            context.Entry(ODM).State = EntityState.Modified;
                            #region Order Master History for Status Delivery Redispatch

                            OrderMasterHistories OrderMasterHistories = new OrderMasterHistories();

                            OrderMasterHistories.orderid = ODM.OrderId;
                            OrderMasterHistories.Status = ODM.Status;
                            OrderMasterHistories.Reasoncancel = null;
                            OrderMasterHistories.Warehousename = ODM.WarehouseName;
                            OrderMasterHistories.DeliveryIssuanceId = obj.DeliveryIssuanceId;
                            OrderMasterHistories.username = People.DisplayName != null ? People.DisplayName : People.PeopleFirstName; ;
                            OrderMasterHistories.userid = People.PeopleID;
                            OrderMasterHistories.CreatedDate = DateTime.Now;
                            context.OrderMasterHistoriesDB.Add(OrderMasterHistories);


                            #endregion

                            //foreach (var detail in ODM.orderDetails)
                            //{
                            //    detail.Status = obj.Status;
                            //    detail.UpdatedDate = indianTime;
                            //    context.Entry(detail).State = EntityState.Modified;

                            //}

                            ODMaster.Status = obj.Status;
                            ODMaster.comments = obj.comments;
                            ODMaster.UpdatedDate = indianTime;
                            ODMaster.ReDispatchCount = ODM.ReDispatchCount;

                            context.Entry(ODMaster).State = EntityState.Modified;

                            foreach (var detail in ODMaster.orderDetails)
                            {
                                detail.Status = obj.Status;
                                detail.UpdatedDate = indianTime;
                                context.Entry(detail).State = EntityState.Modified;

                            }

                            //var RO = context.RedispatchWarehouseDb.Where(x => x.OrderId == obj.OrderId && x.DboyMobileNo == obj.DboyMobileNo).FirstOrDefault();
                            //if (RO != null)
                            //{
                            //    RO.Status = obj.Status;
                            //    RO.comments = obj.comments;
                            //    RO.ReDispatchCount = ODM.ReDispatchCount;
                            //    RO.UpdatedDate = indianTime;
                            //    context.Entry(RO).State = EntityState.Modified;

                            //}
                            //else
                            //{
                            //    RO = new RedispatchWarehouse();
                            //    RO.active = true;
                            //    RO.comments = obj.comments;
                            //    RO.CompanyId = ODM.CompanyId;
                            //    RO.CreatedDate = indianTime;
                            //    RO.UpdatedDate = indianTime;
                            //    RO.DboyMobileNo = obj.DboyMobileNo;
                            //    RO.DboyName = obj.DboyName;
                            //    RO.Deleted = false;
                            //    RO.OrderDispatchedMasterId = obj.OrderDispatchedMasterId;
                            //    RO.OrderId = obj.OrderId;
                            //    RO.WarehouseId = obj.WarehouseId;
                            //    RO.ReDispatchCount = ODM.ReDispatchCount;
                            //    RO.Status = obj.Status;
                            //    context.RedispatchWarehouseDb.Add(RO);

                            //}
                        }
                    }
                    else
                    {
                        res = new ResDTO()
                        {
                            op = null,
                            Message = "Not Delivered",
                            Status = Status
                        };
                        return Request.CreateResponse(HttpStatusCode.OK, res);
                    }

                    if (context.Commit() > 0)
                    {

                        #region stock Hit on post decline order

                        if (ODMaster != null && ODMaster.OrderType != 5 && obj.Status == "Delivered" && OldStatus != "Delivered")
                        {
                            //ifold status is Delivery Canceled  Stock_DeliveredOnAssignmentReject
                            if (OldStatus == "Delivery Canceled")
                            {
                                MultiStockHelper<OnDeliveredOnAssignmentRejectDC> MultiStockHelpers = new MultiStockHelper<OnDeliveredOnAssignmentRejectDC>();
                                List<OnDeliveredOnAssignmentRejectDC> OnDeliveredCStockList = new List<OnDeliveredOnAssignmentRejectDC>();
                                foreach (var StockHit in ODM.orderDetails.Where(x => x.qty > 0))
                                {
                                    var RefStockCode = ODMaster.OrderType == 8 ? "CL" : "C";
                                    bool isFree = ODMaster.orderDetails.Any(c => c.OrderDetailsId == StockHit.OrderDetailsId && c.IsFreeItem && c.IsDispatchedFreeStock);
                                    if (isFree) { RefStockCode = "F"; }
                                    else if (ODMaster.OrderType == 6) //6 Damage stock
                                    {
                                        RefStockCode = "D";
                                    }
                                    OnDeliveredCStockList.Add(new OnDeliveredOnAssignmentRejectDC
                                    {
                                        ItemMultiMRPId = StockHit.ItemMultiMRPId,
                                        OrderDispatchedDetailsId = StockHit.OrderDispatchedDetailsId,
                                        OrderId = StockHit.OrderId,
                                        Qty = StockHit.qty,
                                        UserId = People.PeopleID,
                                        WarehouseId = StockHit.WarehouseId,
                                        RefStockCode = RefStockCode,
                                        SourceTableName = StockTypeTableNames.DeliveryCancelStock
                                    });
                                }
                                if (OnDeliveredCStockList.Any())
                                {
                                    bool IsUpdated = MultiStockHelpers.MakeEntry(OnDeliveredCStockList, "Stock_DeliveredOnAssignmentReject", context, dbContextTransaction);
                                    if (!IsUpdated)
                                    {
                                        Status = false;
                                        dbContextTransaction.Dispose();
                                        res = new ResDTO()
                                        {
                                            op = null,
                                            Message = "Not Delivered",
                                            Status = Status
                                        };
                                        return Request.CreateResponse(HttpStatusCode.OK, res);
                                    }
                                }
                            }
                            else
                            {
                                //if old status is Delivery Redispatched  Stock_DeliveredOnAssignmentReject
                                MultiStockHelper<OnDeliveredOnAssignmentRejectDC> MultiStockHelpers = new MultiStockHelper<OnDeliveredOnAssignmentRejectDC>();
                                List<OnDeliveredOnAssignmentRejectDC> OnDeliveredCStockList = new List<OnDeliveredOnAssignmentRejectDC>();
                                foreach (var StockHit in ODM.orderDetails.Where(x => x.qty > 0))
                                {
                                    var RefStockCode = ODMaster.OrderType == 8 ? "CL" : "C";
                                    bool isFree = ODMaster.orderDetails.Any(c => c.OrderDetailsId == StockHit.OrderDetailsId && c.IsFreeItem && c.IsDispatchedFreeStock);
                                    if (isFree) { RefStockCode = "F"; }
                                    else if (ODMaster.OrderType == 6) //6 Damage stock
                                    {
                                        RefStockCode = "D";
                                    }
                                    OnDeliveredCStockList.Add(new OnDeliveredOnAssignmentRejectDC
                                    {
                                        ItemMultiMRPId = StockHit.ItemMultiMRPId,
                                        OrderDispatchedDetailsId = StockHit.OrderDispatchedDetailsId,
                                        OrderId = StockHit.OrderId,
                                        Qty = StockHit.qty,
                                        UserId = People.PeopleID,
                                        WarehouseId = StockHit.WarehouseId,
                                        RefStockCode = RefStockCode,
                                        SourceTableName = StockTypeTableNames.DeliveryRedispatchStock
                                    });
                                }
                                if (OnDeliveredCStockList.Any())
                                {
                                    bool IsUpdated = MultiStockHelpers.MakeEntry(OnDeliveredCStockList, "Stock_DeliveredOnAssignmentReject", context, dbContextTransaction);
                                    if (!IsUpdated)
                                    {
                                        Status = false;
                                        dbContextTransaction.Dispose();
                                        res = new ResDTO()
                                        {
                                            op = null,
                                            Message = "Not Delivered",
                                            Status = Status
                                        };
                                        return Request.CreateResponse(HttpStatusCode.OK, res);
                                    }
                                }
                            }
                            #region  for Franchises
                            if (context.Customers.Any(x => x.CustomerId == ODM.CustomerId && x.IsFranchise == true))
                            {
                                var DeliveredOrderToFranchisesdb = context.DeliveredOrderToFranchises.Where(x => x.OrderId == ODM.OrderId).FirstOrDefault();
                                if (DeliveredOrderToFranchisesdb == null)
                                {
                                    DeliveredOrderToFranchise FDB = new DeliveredOrderToFranchise();
                                    FDB.OrderId = ODM.OrderId;
                                    FDB.CreatedDate = indianTime;
                                    FDB.IsProcessed = false;
                                    context.DeliveredOrderToFranchises.Add(FDB);
                                }
                            }
                            #endregion

                        }
                        else if (ODMaster != null && ODMaster.OrderType != 5 && obj.Status == "Delivery Canceled" && OldStatus != "Delivery Canceled")
                        {
                            //ifold status is Delivered   Stock_DeliveredOnAssignmentReject
                            if (OldStatus == "Delivered")
                            {
                                MultiStockHelper<ONDeliveryCancelOnAssignmentRejectDc> MultiStockHelpers = new MultiStockHelper<ONDeliveryCancelOnAssignmentRejectDc>();
                                List<ONDeliveryCancelOnAssignmentRejectDc> DeliveryCanceledCStockList = new List<ONDeliveryCancelOnAssignmentRejectDc>();
                                foreach (var StockHit in ODM.orderDetails.Where(x => x.qty > 0))
                                {
                                    var RefStockCode = ODMaster.OrderType == 8 ? "CL" : "C";
                                    bool isFree = ODMaster.orderDetails.Any(c => c.OrderDetailsId == StockHit.OrderDetailsId && c.IsFreeItem && c.IsDispatchedFreeStock);
                                    if (isFree) { RefStockCode = "F"; }
                                    else if (ODMaster.OrderType == 6) //6 Damage stock
                                    {
                                        RefStockCode = "D";
                                    }
                                    DeliveryCanceledCStockList.Add(new ONDeliveryCancelOnAssignmentRejectDc
                                    {
                                        ItemMultiMRPId = StockHit.ItemMultiMRPId,
                                        OrderDispatchedDetailsId = StockHit.OrderDispatchedDetailsId,
                                        OrderId = StockHit.OrderId,
                                        Qty = StockHit.qty,
                                        UserId = People.PeopleID,
                                        WarehouseId = StockHit.WarehouseId,
                                        RefStockCode = RefStockCode,
                                    });
                                }
                                if (DeliveryCanceledCStockList.Any())
                                {
                                    bool IsUpdated = MultiStockHelpers.MakeEntry(DeliveryCanceledCStockList, "Stock_DeliveryCancelOnAssignmentReject", context, dbContextTransaction);
                                    if (!IsUpdated)
                                    {
                                        Status = false;
                                        dbContextTransaction.Dispose();
                                        res = new ResDTO()
                                        {
                                            op = null,
                                            Message = "Order Not Delivery Canceled",
                                            Status = Status
                                        };
                                        return Request.CreateResponse(HttpStatusCode.OK, res);
                                    }
                                }
                            }
                            else
                            {      //ifold status is Delivery Canceled   Stock_DeliveredOnAssignmentReject

                                MultiStockHelper<OnDeliveryCanceledStockEntryDc> MultiStockHelpers = new MultiStockHelper<OnDeliveryCanceledStockEntryDc>();
                                List<OnDeliveryCanceledStockEntryDc> DeliveryCanceledCStockList = new List<OnDeliveryCanceledStockEntryDc>();
                                foreach (var StockHit in ODM.orderDetails.Where(x => x.qty > 0))
                                {
                                    var RefStockCode = ODMaster.OrderType == 8 ? "CL" : "C";
                                    bool isFree = ODMaster.orderDetails.Any(c => c.OrderDetailsId == StockHit.OrderDetailsId && c.IsFreeItem && c.IsDispatchedFreeStock);
                                    if (isFree) { RefStockCode = "F"; }
                                    else if (ODMaster.OrderType == 6) //6 Damage stock
                                    {
                                        RefStockCode = "D";
                                    }
                                    DeliveryCanceledCStockList.Add(new OnDeliveryCanceledStockEntryDc
                                    {
                                        ItemMultiMRPId = StockHit.ItemMultiMRPId,
                                        OrderDispatchedDetailsId = StockHit.OrderDispatchedDetailsId,
                                        OrderId = StockHit.OrderId,
                                        Qty = StockHit.qty,
                                        UserId = People.PeopleID,
                                        WarehouseId = StockHit.WarehouseId,
                                        RefStockCode = RefStockCode,
                                        IsDeliveryRedispatchCancel = true
                                    });
                                }
                                if (DeliveryCanceledCStockList.Any())
                                {
                                    bool IsUpdated = MultiStockHelpers.MakeEntry(DeliveryCanceledCStockList, "Stock_OnDeliveryCancel_New", context, dbContextTransaction);
                                    if (!IsUpdated)
                                    {
                                        Status = false;
                                        dbContextTransaction.Dispose();
                                        res = new ResDTO()
                                        {
                                            op = null,
                                            Message = "Order Not Delivery Canceled",
                                            Status = Status
                                        };
                                        return Request.CreateResponse(HttpStatusCode.OK, res);
                                    }
                                }
                            }

                        }
                        else if (ODMaster != null && ODMaster.OrderType != 5 && obj.Status == "Delivery Canceled Request" && OldStatus != "Delivery Canceled Request")
                        {
                            //ifold status is Delivered   Stock_DCROnAssignmentReject
                            if (OldStatus == "Delivered")
                            {
                                MultiStockHelper<OnDeliveredOnAssignmentRejectDC> MultiStockHelpers = new MultiStockHelper<OnDeliveredOnAssignmentRejectDC>();
                                List<OnDeliveredOnAssignmentRejectDC> OnDeliveredCStockList = new List<OnDeliveredOnAssignmentRejectDC>();
                                foreach (var StockHit in ODM.orderDetails.Where(x => x.qty > 0))
                                {
                                    var RefStockCode = ODMaster.OrderType == 8 ? "CL" : "C";
                                    bool isFree = ODMaster.orderDetails.Any(c => c.OrderDetailsId == StockHit.OrderDetailsId && c.IsFreeItem && c.IsDispatchedFreeStock);
                                    if (isFree) { RefStockCode = "F"; }
                                    else if (ODMaster.OrderType == 6) //6 Damage stock
                                    {
                                        RefStockCode = "D";
                                    }
                                    OnDeliveredCStockList.Add(new OnDeliveredOnAssignmentRejectDC
                                    {
                                        ItemMultiMRPId = StockHit.ItemMultiMRPId,
                                        OrderDispatchedDetailsId = StockHit.OrderDispatchedDetailsId,
                                        OrderId = StockHit.OrderId,
                                        Qty = StockHit.qty,
                                        UserId = People.PeopleID,
                                        WarehouseId = StockHit.WarehouseId,
                                        RefStockCode = RefStockCode,
                                        SourceTableName = StockTypeTableNames.DeliveredStock
                                    });
                                }
                                if (OnDeliveredCStockList.Any())
                                {
                                    bool IsUpdated = MultiStockHelpers.MakeEntry(OnDeliveredCStockList, "Stock_DCROnAssignmentReject", context, dbContextTransaction);
                                    if (!IsUpdated)
                                    {
                                        Status = false;
                                        dbContextTransaction.Dispose();
                                        res = new ResDTO()
                                        {
                                            op = null,
                                            Message = "Not Delivered",
                                            Status = Status
                                        };
                                        return Request.CreateResponse(HttpStatusCode.OK, res);
                                    }
                                }
                            }
                            else if (OldStatus == "Delivery Canceled")
                            {
                                //if old status is Delivery Redispatched  Stock_DeliveredOnAssignmentReject
                                MultiStockHelper<OnDeliveredOnAssignmentRejectDC> MultiStockHelpers = new MultiStockHelper<OnDeliveredOnAssignmentRejectDC>();
                                List<OnDeliveredOnAssignmentRejectDC> OnDeliveredCStockList = new List<OnDeliveredOnAssignmentRejectDC>();
                                foreach (var StockHit in ODM.orderDetails.Where(x => x.qty > 0))
                                {
                                    var RefStockCode = ODMaster.OrderType == 8 ? "CL" : "C";
                                    bool isFree = ODMaster.orderDetails.Any(c => c.OrderDetailsId == StockHit.OrderDetailsId && c.IsFreeItem && c.IsDispatchedFreeStock);
                                    if (isFree) { RefStockCode = "F"; }
                                    else if (ODMaster.OrderType == 6) //6 Damage stock
                                    {
                                        RefStockCode = "D";
                                    }
                                    OnDeliveredCStockList.Add(new OnDeliveredOnAssignmentRejectDC
                                    {
                                        ItemMultiMRPId = StockHit.ItemMultiMRPId,
                                        OrderDispatchedDetailsId = StockHit.OrderDispatchedDetailsId,
                                        OrderId = StockHit.OrderId,
                                        Qty = StockHit.qty,
                                        UserId = People.PeopleID,
                                        WarehouseId = StockHit.WarehouseId,
                                        RefStockCode = RefStockCode,
                                        SourceTableName = StockTypeTableNames.DeliveryCancelStock
                                    });
                                }
                                if (OnDeliveredCStockList.Any())
                                {
                                    bool IsUpdated = MultiStockHelpers.MakeEntry(OnDeliveredCStockList, "Stock_DCROnAssignmentReject", context, dbContextTransaction);
                                    if (!IsUpdated)
                                    {
                                        Status = false;
                                        dbContextTransaction.Dispose();
                                        res = new ResDTO()
                                        {
                                            op = null,
                                            Message = "Not Delivered",
                                            Status = Status
                                        };
                                        return Request.CreateResponse(HttpStatusCode.OK, res);
                                    }
                                }
                            }
                            else if (OldStatus == "Delivery Redispatch")
                            {
                                //if old status is Delivery Redispatched  Stock_DeliveredOnAssignmentReject
                                MultiStockHelper<OnDeliveredOnAssignmentRejectDC> MultiStockHelpers = new MultiStockHelper<OnDeliveredOnAssignmentRejectDC>();
                                List<OnDeliveredOnAssignmentRejectDC> OnDeliveredCStockList = new List<OnDeliveredOnAssignmentRejectDC>();
                                foreach (var StockHit in ODM.orderDetails.Where(x => x.qty > 0))
                                {
                                    var RefStockCode = ODMaster.OrderType == 8 ? "CL" : "C";
                                    bool isFree = ODMaster.orderDetails.Any(c => c.OrderDetailsId == StockHit.OrderDetailsId && c.IsFreeItem && c.IsDispatchedFreeStock);
                                    if (isFree) { RefStockCode = "F"; }
                                    else if (ODMaster.OrderType == 6) //6 Damage stock
                                    {
                                        RefStockCode = "D";
                                    }
                                    OnDeliveredCStockList.Add(new OnDeliveredOnAssignmentRejectDC
                                    {
                                        ItemMultiMRPId = StockHit.ItemMultiMRPId,
                                        OrderDispatchedDetailsId = StockHit.OrderDispatchedDetailsId,
                                        OrderId = StockHit.OrderId,
                                        Qty = StockHit.qty,
                                        UserId = People.PeopleID,
                                        WarehouseId = StockHit.WarehouseId,
                                        RefStockCode = RefStockCode,
                                        SourceTableName = StockTypeTableNames.DeliveryRedispatchStock
                                    });
                                }
                                if (OnDeliveredCStockList.Any())
                                {
                                    bool IsUpdated = MultiStockHelpers.MakeEntry(OnDeliveredCStockList, "Stock_DCROnAssignmentReject", context, dbContextTransaction);
                                    if (!IsUpdated)
                                    {
                                        Status = false;
                                        dbContextTransaction.Dispose();
                                        res = new ResDTO()
                                        {
                                            op = null,
                                            Message = "Not Delivered",
                                            Status = Status
                                        };
                                        return Request.CreateResponse(HttpStatusCode.OK, res);
                                    }
                                }
                            }

                        }
                        else if (ODMaster != null && ODMaster.OrderType != 5 && obj.Status == "Delivery Redispatch" && OldStatus != "Delivery Redispatch")
                        {
                            //if old status is Delivered  Stock_DeliveredOnAssignmentReject
                            if (OldStatus == "Delivered")
                            {
                                MultiStockHelper<OnRedispatchOnAssignmentRejectDc> MultiStockHelpers = new MultiStockHelper<OnRedispatchOnAssignmentRejectDc>();
                                List<OnRedispatchOnAssignmentRejectDc> DeliveryRedispatchedCStockList = new List<OnRedispatchOnAssignmentRejectDc>();
                                foreach (var StockHit in ODM.orderDetails.Where(x => x.qty > 0))
                                {
                                    var RefStockCode = ODMaster.OrderType == 8 ? "CL" : "C";
                                    bool isFree = ODMaster.orderDetails.Any(c => c.OrderDetailsId == StockHit.OrderDetailsId && c.IsFreeItem && c.IsDispatchedFreeStock);
                                    if (isFree) { RefStockCode = "F"; }
                                    else if (ODMaster.OrderType == 6) //6 Damage stock
                                    {
                                        RefStockCode = "D";
                                    }
                                    DeliveryRedispatchedCStockList.Add(new OnRedispatchOnAssignmentRejectDc
                                    {
                                        ItemMultiMRPId = StockHit.ItemMultiMRPId,
                                        OrderDispatchedDetailsId = StockHit.OrderDispatchedDetailsId,
                                        OrderId = StockHit.OrderId,
                                        Qty = StockHit.qty,
                                        UserId = People.PeopleID,
                                        WarehouseId = StockHit.WarehouseId,
                                        RefStockCode = RefStockCode
                                    });
                                }
                                if (DeliveryRedispatchedCStockList.Any())
                                {
                                    bool IsUpdated = MultiStockHelpers.MakeEntry(DeliveryRedispatchedCStockList, "Stock_RedispatchOnAssignmentReject", context, dbContextTransaction);
                                    if (!IsUpdated)
                                    {
                                        Status = false;
                                        dbContextTransaction.Dispose();
                                        res = new ResDTO()
                                        {
                                            op = null,
                                            Message = "Order Not Delivery Redispatch",
                                            Status = Status
                                        };
                                        return Request.CreateResponse(HttpStatusCode.OK, res);
                                    }
                                }
                            }
                            else
                            {
                                //if old status is Delivery Canceled

                                MultiStockHelper<OnDeliveryRedispatchedStockEntryDc> MultiStockHelpers = new MultiStockHelper<OnDeliveryRedispatchedStockEntryDc>();
                                List<OnDeliveryRedispatchedStockEntryDc> DeliveryRedispatchedCStockList = new List<OnDeliveryRedispatchedStockEntryDc>();
                                foreach (var StockHit in ODM.orderDetails.Where(x => x.qty > 0))
                                {
                                    var RefStockCode = ODMaster.OrderType == 8 ? "CL" : "C";
                                    bool isFree = ODMaster.orderDetails.Any(c => c.OrderDetailsId == StockHit.OrderDetailsId && c.IsFreeItem && c.IsDispatchedFreeStock);
                                    if (isFree) { RefStockCode = "F"; }
                                    else if (ODMaster.OrderType == 6) //6 Damage stock
                                    {
                                        RefStockCode = "D";
                                    }
                                    DeliveryRedispatchedCStockList.Add(new OnDeliveryRedispatchedStockEntryDc
                                    {
                                        ItemMultiMRPId = StockHit.ItemMultiMRPId,
                                        OrderDispatchedDetailsId = StockHit.OrderDispatchedDetailsId,
                                        OrderId = StockHit.OrderId,
                                        Qty = StockHit.qty,
                                        UserId = People.PeopleID,
                                        WarehouseId = StockHit.WarehouseId,
                                        RefStockCode = RefStockCode,
                                        IsDeliveryCancel = true
                                    });
                                }
                                if (DeliveryRedispatchedCStockList.Any())
                                {
                                    bool IsUpdated = MultiStockHelpers.MakeEntry(DeliveryRedispatchedCStockList, "Stock_OnDeliveryRedispatch", context, dbContextTransaction);
                                    if (!IsUpdated)
                                    {
                                        Status = false;
                                        dbContextTransaction.Dispose();
                                        res = new ResDTO()
                                        {
                                            op = null,
                                            Message = "Order Not Delivery Redispatch",
                                            Status = Status
                                        };
                                        return Request.CreateResponse(HttpStatusCode.OK, res);
                                    }
                                }
                            }


                        }
                        #endregion
                        #region Sellerstock update                        
                        if ((ODM != null && ODM.CustomerType == "SellerStore"))
                        {
                            UpdateSellerStockOfCFRProduct Postobj = new UpdateSellerStockOfCFRProduct();
                            if ((OldStatus == "Delivery Canceled" || OldStatus == "Delivery Redispatch") && (ODM.Status == "Delivered"))
                            {
                                Postobj.OrderId = ODM.OrderId;
                                Postobj.Skcode = ODM.Skcode;
                                Postobj.ItemDetailDc = new List<SellerItemDetailDc>();
                                foreach (var item in ODM.orderDetails)
                                {
                                    SellerItemDetailDc newitem = new SellerItemDetailDc();
                                    newitem.ItemMultiMrpId = item.ItemMultiMRPId;
                                    newitem.SellingPrice = item.UnitPrice;
                                    newitem.qty = (1) * item.qty;
                                    Postobj.ItemDetailDc.Add(newitem);
                                }
                            }
                            else if ((ODM.Status == "Delivery Canceled" || ODM.Status == "Delivery Redispatch") && (OldStatus == "Delivered"))
                            {
                                Postobj.OrderId = ODM.OrderId;
                                Postobj.Skcode = ODM.Skcode;
                                Postobj.ItemDetailDc = new List<SellerItemDetailDc>();
                                foreach (var item in ODM.orderDetails)
                                {
                                    SellerItemDetailDc newitem = new SellerItemDetailDc();
                                    newitem.ItemMultiMrpId = item.ItemMultiMRPId;
                                    newitem.SellingPrice = item.UnitPrice;
                                    newitem.qty = (-1) * item.qty;
                                    Postobj.ItemDetailDc.Add(newitem);
                                }
                            }
                            BackgroundTaskManager.Run(() =>
                            {
                                try
                                {
                                    var tradeUrl = ConfigurationManager.AppSettings["SellerURL"] + "/api/sk/RetailerAppApi/UpdateStockOnDeliveryFromSkApp";
                                    using (GenericRestHttpClient<UpdateSellerStockOfCFRProduct, string> memberClient = new GenericRestHttpClient<UpdateSellerStockOfCFRProduct, string>(tradeUrl, "", null))
                                    {
                                        AsyncContext.Run(() => memberClient.PostAsync(Postobj));
                                    }
                                }
                                catch (Exception ex)
                                {
                                    TextFileLogHelper.LogError("Error while Update Seller Stock Of CFR Product: " + ex.ToString());
                                }
                            });
                        }
                        #endregion

                        dbContextTransaction.Complete();
                        Status = true;
                        if (!string.IsNullOrEmpty(ODMaster.Customerphonenum) && obj.Status == "Delivered")
                        {
                            // string orderCancelMsg = "Hi, Your Order Number  " + ODMaster.OrderId + ", has been delivered. In case of any concerns, please click on the link below " + ConfigurationManager.AppSettings["RetailerWebviewURL"] + "order-concern/" + ODMaster.OrderId + "/" + guidData + ". ShopKirana";
                            var dltSMS = SMSTemplateHelper.getTemplateText((int)AppEnum.RetailerApp, "Order_Delivered_Concern");
                            string message = dltSMS == null ? "" : dltSMS.Template;

                            message = message.Replace("{#var1#}", ",");
                            message = message.Replace("{#var2#}", ODMaster.OrderId.ToString());
                            //message = message.Replace("{#var3#}", ConfigurationManager.AppSettings["RetailerWebviewURL"] + "order-concern/" + ODMaster.OrderId + "/" + guidData);
                            string shortUrl = Helpers.ShortenerUrl.ShortenUrl(ConfigurationManager.AppSettings["RetailerWebviewURL"] + "order-concern/" + ODMaster.OrderId + "/" + guidData);
                            message = message.Replace("{#var3#}", shortUrl);
                            if (dltSMS != null)
                                Common.Helpers.SendSMSHelper.SendSMS(ODMaster.Customerphonenum, message, ((Int32)Common.Enums.SMSRouteEnum.Transactional).ToString(), dltSMS.DLTId);
                        }
                        if (ODMaster != null && ODMaster.OrderType == 5 && obj.Status == "Delivered")
                        {
                            try
                            {
                                UpdateConsumerOrders updateConsumerOrders = new UpdateConsumerOrders
                                {
                                    CartStatus = "Delivered",
                                    InvoiceNo = ODMaster.invoice_no
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
                    else
                    {
                        Status = false;
                    }
                    res = new ResDTO()
                    {
                        op = obj,
                        Message = "Success",
                        Status = Status
                    };
                    return Request.CreateResponse(HttpStatusCode.OK, res);

                }
            }
        }
        #endregion

        [Route("PaymentAssignment/V1")]
        [HttpGet]
        public HttpResponseMessage PaymentAssignment(Int32 DeliveryIssuanceId) //get orders for delivery for task Order
        {
            OrderDeliveryMasterDTODApp res;
            using (var db = new AuthContext())
            {
                try
                {
                    List<OrderDeliveryMaster> OrderDeliveryMaster = db.OrderDeliveryMasterDB.Where(x => x.DeliveryIssuanceId == DeliveryIssuanceId).ToList();
                    if (OrderDeliveryMaster.Count > 0)
                    {
                        var orderIds = OrderDeliveryMaster.Select(y => y.OrderId).Distinct();
                        var PaymentResponseRetailerApps = db.PaymentResponseRetailerAppDb.Where(x => orderIds.Contains(x.OrderId) && x.status == "Success").ToList();
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






                        foreach (var Sta in OrderDeliveryMaster)
                        {
                            List<ShortItemAssignment> ShortItemAssignment = db.ShortItemAssignmentDB.Where(x => x.DeliveryIssuanceId == Sta.DeliveryIssuanceId).ToList();
                            if (ShortItemAssignment.Count > 0)
                            {
                                Sta.ShortItemAssignment = ShortItemAssignment;
                            }

                            Sta.PaymentDetails = PaymentResponseRetailerAppList.Where(z => z.OrderId == Sta.OrderId && z.status == "Success")
                                .Select(z => new PaymentDto
                                {
                                    Amount = z.amount,
                                    PaymentFrom = z.PaymentFrom,
                                    // TransDate = z.UpdatedDate,
                                    TransRefNo = z.GatewayTransId,
                                    IsOnline = z.IsOnline
                                }).ToList();


                            if (Sta.PaymentDetails == null || !Sta.PaymentDetails.Any())
                            {
                                if (Sta.CashAmount > 0)
                                {
                                    Sta.PaymentDetails.Add(new PaymentDto
                                    {
                                        Amount = Sta.CashAmount,
                                        PaymentFrom = "Cash"
                                    });
                                }

                                if (Sta.CheckAmount > 0)
                                {
                                    Sta.PaymentDetails.Add(new PaymentDto
                                    {
                                        Amount = Sta.CashAmount,
                                        TransRefNo = Sta.CheckNo,
                                        PaymentFrom = "Cheque",
                                        ChequeImageUrl = Sta.ChequeImageUrl,
                                        ChequeBankName = Sta.ChequeBankName



                                    });
                                }

                                if (Sta.ElectronicAmount > 0)
                                {
                                    Sta.PaymentDetails.Add(new PaymentDto
                                    {
                                        Amount = Sta.ElectronicAmount,
                                        TransRefNo = Sta.ElectronicPaymentNo,
                                        PaymentFrom = "ePayLater"
                                    });
                                }
                            }


                        }
                        res = new OrderDeliveryMasterDTODApp()
                        {
                            Order = OrderDeliveryMaster,
                            status = true,
                            Message = "record found."
                        };
                    }
                    else
                    {
                        res = new OrderDeliveryMasterDTODApp()
                        {
                            Order = null,
                            status = true,
                            Message = "No record found"
                        };
                    }
                    return Request.CreateResponse(HttpStatusCode.OK, res);
                }
                catch (Exception ex)
                {
                    res = new OrderDeliveryMasterDTODApp()
                    {
                        Order = null,
                        status = false,
                        Message = "Failed. " + ex.Message + ""
                    };
                    return Request.CreateResponse(HttpStatusCode.BadRequest, res);
                }
            }
        }


        //Update form Dapp for Assignment Freezed
        [AllowAnonymous]
        [Route("PaymentSubmittedAssignment/V1")]
        [HttpPut]
        public HttpResponseMessage PaymentSubmittedAssignment(int id, Int32 DeliveryIssuanceId, string FileName)
        {
            UploadAssignment res;
            using (var db = new AuthContext())
            {
                //try
                //{
                var DBIssuance = db.DeliveryIssuanceDb.Where(x => x.DeliveryIssuanceId == DeliveryIssuanceId && x.PeopleID == id).FirstOrDefault();

                if (DBIssuance != null)
                {
                    var peopledata = db.Peoples.Where(x => x.PeopleID == id).FirstOrDefault();
                    DBIssuance.Status = "Payment Submitted";
                    DBIssuance.UploadedFileName = FileName;
                    DBIssuance.UpdatedDate = DateTime.Now;
                    db.Entry(DBIssuance).State = EntityState.Modified;

                    #region Order Delivery  Master History for Status Payment Submitted
                    OrderDeliveryMasterHistories AssginDeli = new OrderDeliveryMasterHistories();
                    AssginDeli.DeliveryIssuanceId = DBIssuance.DeliveryIssuanceId;
                    //AssginDeli.OrderId = DBIssuance.o
                    AssginDeli.Cityid = DBIssuance.Cityid;
                    AssginDeli.city = DBIssuance.city;
                    AssginDeli.DisplayName = DBIssuance.DisplayName;
                    AssginDeli.Status = DBIssuance.Status;
                    AssginDeli.WarehouseId = DBIssuance.WarehouseId;
                    AssginDeli.PeopleID = DBIssuance.PeopleID;
                    AssginDeli.VehicleId = DBIssuance.VehicleId;
                    AssginDeli.VehicleNumber = DBIssuance.VehicleNumber;
                    AssginDeli.RejectReason = DBIssuance.RejectReason;
                    AssginDeli.OrderdispatchIds = DBIssuance.OrderdispatchIds;
                    AssginDeli.OrderIds = DBIssuance.OrderIds;
                    AssginDeli.Acceptance = DBIssuance.Acceptance;
                    AssginDeli.IsActive = DBIssuance.IsActive;
                    AssginDeli.IdealTime = DBIssuance.IdealTime;
                    AssginDeli.TravelDistance = DBIssuance.TravelDistance;
                    AssginDeli.CreatedDate = indianTime;
                    AssginDeli.UpdatedDate = indianTime;
                    AssginDeli.userid = id;
                    if (peopledata != null)
                    {
                        if (peopledata.DisplayName == null)
                        {
                            AssginDeli.UpdatedBy = peopledata.PeopleFirstName;
                        }
                        else
                        {
                            AssginDeli.UpdatedBy = peopledata.DisplayName;
                        }

                    }
                    db.OrderDeliveryMasterHistoriesDB.Add(AssginDeli);

                    #endregion


                    var ODMs = db.OrderDispatchedMasters.Where(x => x.DeliveryIssuanceIdOrderDeliveryMaster == DeliveryIssuanceId && x.Status == "Delivered").ToList();
                    foreach (var ODMaster in ODMs)
                    {
                        if (ODMaster.RewardPoint > 0)
                        {
                            if (!db.CustomerWalletHistoryDb.Any(x => x.OrderId == ODMaster.OrderId && x.NewAddedWAmount == ODMaster.RewardPoint && x.Through == "From Order Delivered"))
                            {
                                Wallet wlt = db.WalletDb.Where(c => c.CustomerId == ODMaster.CustomerId).SingleOrDefault();

                                CustomerWalletHistory CWH = new CustomerWalletHistory();
                                CWH.WarehouseId = ODMaster.WarehouseId;
                                CWH.CompanyId = ODMaster.CompanyId;
                                CWH.CustomerId = wlt.CustomerId;
                                CWH.NewAddedWAmount = ODMaster.RewardPoint;
                                CWH.OrderId = ODMaster.OrderId;
                                CWH.Through = "From Order Delivered";
                                CWH.TotalWalletAmount = wlt.TotalAmount + ODMaster.RewardPoint;
                                CWH.CreatedDate = indianTime;
                                CWH.UpdatedDate = indianTime;
                                db.CustomerWalletHistoryDb.Add(CWH);

                                wlt.TotalAmount += ODMaster.RewardPoint;
                                try
                                {
                                    BackgroundTaskManager.Run(() =>
                                    {
                                        DeliveredNotification(wlt.CustomerId, ODMaster.RewardPoint, ODMaster.OrderId, db);
                                    });
                                }
                                catch (Exception ex) { logger.Error("Error loading wqewqerwqr \n\n" + ex.Message + "\n\n" + ex.InnerException + "\n\n" + ex.StackTrace); }

                                wlt.TransactionDate = indianTime;
                                db.Entry(wlt).State = EntityState.Modified;

                                var rpoint = db.RewardPointDb.Where(c => c.CustomerId == ODMaster.CustomerId).SingleOrDefault();
                                if (rpoint != null)
                                {
                                    rpoint.EarningPoint -= ODMaster.RewardPoint;
                                    if (rpoint.EarningPoint < 0)
                                        rpoint.EarningPoint = 0;
                                    rpoint.UpdatedDate = indianTime;
                                    db.Entry(rpoint).State = EntityState.Modified;
                                }
                            }
                        }

                        #region Kisan Dan Insert
                        try
                        {
                            var Orderid = ODMaster.OrderId;
                            string query = "select a.customerid,a.orderid,"
                            + " sum(case when subCategoryName = 'kisan kirana' then qty * UnitPrice else 0 end) KisanKiranaAmount, "
                            + " sum(qty * UnitPrice) OrderAmount from OrderDispatchedDetails a with(nolock)"
                            + " where OrderId =  " + Orderid
                            + " group by a.CustomerId,a.orderid having  sum(case when subCategoryName = 'kisan kirana' then qty * UnitPrice else 0 end)> 0";

                            var kisanDanMasters = db.kisanDanMaster.Where(x => x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value)).ToList();
                            var data = db.Database.SqlQuery<CustomerKisanDanDTO>(query).ToList();
                            CustomerKisanDan newdata = new CustomerKisanDan();
                            foreach (var item in data)
                            {
                                if (!db.CustomerKisanDan.Any(x => x.OrderId == item.OrderId && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value)))
                                {
                                    newdata = new CustomerKisanDan();
                                    newdata.CustomerId = item.CustomerId;
                                    newdata.OrderId = item.OrderId;
                                    newdata.KisanKiranaAmount = Convert.ToDecimal(item.KisanKiranaAmount);
                                    newdata.IsActive = true;
                                    newdata.IsDeleted = false;
                                    newdata.CreatedBy = id;
                                    newdata.CreatedDate = indianTime;
                                    if (newdata.KisanKiranaAmount > 0 && kisanDanMasters != null && kisanDanMasters.Any())
                                    {
                                        var percent = kisanDanMasters.Any(x => x.OrderFromAmount <= newdata.KisanKiranaAmount && x.OrderToAmount >= newdata.KisanKiranaAmount) ? kisanDanMasters.FirstOrDefault(x => x.OrderFromAmount <= newdata.KisanKiranaAmount && x.OrderToAmount >= newdata.KisanKiranaAmount).KisanDanPrecentage : 0;
                                        newdata.KisanDanAmount = newdata.KisanKiranaAmount * percent / 100;
                                    }
                                    db.CustomerKisanDan.Add(newdata);
                                    logger.Info("Kisan amount Add Assignment Id: " + DeliveryIssuanceId);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            string error = ex.InnerException != null ? ex.ToString() + Environment.NewLine + ex.InnerException.ToString() : ex.ToString();

                            logger.Error("Error in During Add KisanDan Point For Assignment Id: " + DeliveryIssuanceId + " Error: " + error);
                        }
                        #endregion

                    }

                    db.Commit();
                    //try
                    //{
                    //    SendMailAssignmentCopy(id, DBIssuance.UploadedFileName, DeliveryIssuanceId);
                    //}
                    //catch (Exception sds)
                    //{

                    //}


                    res = new UploadAssignment()
                    {
                        Status = true,
                        Message = "Success."
                    };

                    return Request.CreateResponse(HttpStatusCode.OK, res);
                }
                else
                {

                    res = new UploadAssignment()
                    {
                        Status = false,
                        Message = "Failed due to record not found"
                    };
                    return Request.CreateResponse(HttpStatusCode.OK, res);
                }
                //}
                //catch (Exception ex)
                //{
                //    res = new UploadAssignment()
                //    {
                //        Status = false,
                //        Message = "Failed--" + ex.Message + ""
                //    };
                //    return Request.CreateResponse(HttpStatusCode.OK, res);
                //}
            }
        }

        [AllowAnonymous]
        [Route("UpadteDCStatus")]
        [HttpPost]
        public HttpResponseMessage UpadteDCStatus(OrderPlaceDTO obj) //Order delivered or canceled from delivery app
        {
            var identity = User.Identity as ClaimsIdentity;
            int userid = 0;

            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
            ResDTO res;

            try
            {
                List<OrderDispatchedMaster> postOrderDispatch = null;
                var msg = "";
                bool Status = false;
                TransactionOptions option = new TransactionOptions();
                option.IsolationLevel = IsolationLevel.RepeatableRead;
                option.Timeout = TimeSpan.FromSeconds(90);
                using (var dbContextTransaction = new TransactionScope(TransactionScopeOption.Required, option))
                {
                    using (var context = new AuthContext())
                    {
                        OrderMaster ODMaster = null;
                        var People = context.Peoples.Where(x => x.Mobile == obj.DboyMobileNo && x.Deleted == false && x.Active == true).FirstOrDefault();
                        var DeliveryIssuance = context.DeliveryIssuanceDb.Where(x => x.DeliveryIssuanceId == obj.DeliveryIssuanceId).SingleOrDefault();
                        if (DeliveryIssuance.Status == "Accepted")
                        {
                            DeliveryIssuance.Status = "Pending";
                            DeliveryIssuance.UpdatedDate = indianTime;
                            context.Entry(DeliveryIssuance).State = EntityState.Modified;
                        }
                        else if (DeliveryIssuance.Status == "Pending")
                        {
                        }
                        var ODM = context.OrderDispatchedMasters.Where(x => x.OrderDispatchedMasterId == obj.OrderDispatchedMasterId).Include("orderDetails").SingleOrDefault();
                        ODMaster = context.DbOrderMaster.Where(x => x.OrderId == obj.OrderId).Include("orderDetails").SingleOrDefault();
                        if (obj != null && People != null && DeliveryIssuance.DeliveryIssuanceId > 0 && ODM != null)
                        {
                            //var ODM = context.OrderDispatchedMasters.Where(x => x.OrderDispatchedMasterId == obj.OrderDispatchedMasterId).Include("orderDetails").SingleOrDefault();
                            var OrderDeliveryMaster = context.OrderDeliveryMasterDB.Where(z => z.DeliveryIssuanceId == obj.DeliveryIssuanceId && z.OrderId == obj.OrderId).Include("orderDetails").FirstOrDefault();
                            var AssignmentRechangeOrder = context.AssignmentRechangeOrder.FirstOrDefault(x => x.AssignmentId == obj.DeliveryIssuanceId && x.OrderId == obj.OrderId);
                            var Ordercancellation = context.DeptOrderCancellationDb.Where(o => o.OrderId == obj.OrderId).FirstOrDefault();
                            DeptOrderCancellation deptOrderCacellation = new DeptOrderCancellation();
                            deptOrderCacellation.OrderId = obj.OrderId;

                            if (AssignmentRechangeOrder != null)
                            {
                                AssignmentRechangeOrder.Status = 0;
                                AssignmentRechangeOrder.ModifiedDate = indianTime;
                                AssignmentRechangeOrder.ModifiedBy = People.PeopleID;
                                context.Entry(AssignmentRechangeOrder).State = EntityState.Modified;
                            }

                            if (obj.Status == "Delivery Canceled" && ODM.Status != "Delivery Canceled")
                            {
                                if (OrderDeliveryMaster.comments == "Dont Want" || OrderDeliveryMaster.comments == "Late Delivery (OPS)" || OrderDeliveryMaster.comments == "Damage Expiry (Pur)")
                                {
                                    string query = "select sp.PeopleID  from OrderMasters a inner join Customers c on a.CustomerId=c.CustomerId and a.orderid = " + OrderDeliveryMaster.OrderId + "  inner join People sp on a.SalesPersonId=sp.PeopleID";
                                    var salesLeadId = context.Database.SqlQuery<int?>(query).FirstOrDefault();
                                    if (salesLeadId.HasValue && salesLeadId.Value > 0)
                                    {
                                        deptOrderCacellation.SalesLeadId = salesLeadId.Value;
                                        deptOrderCacellation.OrderId = OrderDeliveryMaster.OrderId;
                                        deptOrderCacellation.ChargePoint = 1;
                                        deptOrderCacellation.IsActive = true;
                                        deptOrderCacellation.IsDeleted = false;
                                        deptOrderCacellation.IsEmailSend = false;
                                        deptOrderCacellation.CreatedDate = DateTime.Now;
                                        deptOrderCacellation.CreatedBy = People.PeopleID;
                                        if (OrderDeliveryMaster.comments == "Dont Want")
                                        {
                                            deptOrderCacellation.DepId = 6;
                                        }
                                        else if (OrderDeliveryMaster.comments == "Late Delivery (OPS)")
                                        {
                                            deptOrderCacellation.DepId = 33;
                                        }
                                        else if (OrderDeliveryMaster.comments == "Damage Expiry (Pur)")
                                        {
                                            deptOrderCacellation.DepId = 29;
                                        }
                                        context.DeptOrderCancellationDb.Add(deptOrderCacellation);
                                    }
                                }
                                if (ODM != null)
                                {
                                    ODM.Status = obj.Status;
                                    ODM.CanceledStatus = obj.Status;
                                    ODM.comments = obj.comments;
                                    ODM.Signimg = obj.Signimg;
                                    ODM.UpdatedDate = indianTime;
                                    ODM.DeliveryLat = obj.DeliveryLat;
                                    ODM.DeliveryLng = obj.DeliveryLng;
                                    context.Entry(ODM).State = EntityState.Modified;

                                    //pz//start

                                    if (OrderDeliveryMaster.comments == "Dont Want" || OrderDeliveryMaster.comments == "Price Issue (sales)")
                                    {
                                        if (!context.CustomerWalletHistoryDb.Any(x => x.OrderId == ODM.OrderId && x.NewOutWAmount == -100 && x.Through == "From Order Cancelled"))
                                        {
                                            Wallet wlt = context.WalletDb.Where(c => c.CustomerId == ODM.CustomerId).SingleOrDefault();

                                            CustomerWalletHistory CWH = new CustomerWalletHistory();
                                            CWH.WarehouseId = ODM.WarehouseId;
                                            CWH.CompanyId = ODM.CompanyId;
                                            CWH.CustomerId = wlt.CustomerId;
                                            CWH.NewAddedWAmount = 0;
                                            CWH.NewOutWAmount = -100;
                                            CWH.OrderId = ODM.OrderId;
                                            CWH.Through = "From Order Cancelled";
                                            CWH.TotalWalletAmount = wlt.TotalAmount - 100;
                                            CWH.CreatedDate = indianTime;
                                            CWH.UpdatedDate = indianTime;
                                            context.CustomerWalletHistoryDb.Add(CWH);

                                            wlt.TotalAmount -= 100;
                                            wlt.TransactionDate = indianTime;
                                            context.Entry(wlt).State = EntityState.Modified;
                                        }
                                    }
                                    ///pz///             

                                    #region Order Master History for Status Delivery Canceled

                                    var DeliveryCanceledRequestData = context.DeliveryCanceledRequestHistoryDb.Where(x => x.OrderId == ODM.OrderId && x.DeliveryCanceledStatus == "Call back" && x.IsDeleted == false && x.IsActive == true).FirstOrDefault();

                                    if (DeliveryCanceledRequestData != null)
                                    {
                                        DeliveryCanceledRequestData.IsActive = false;
                                        DeliveryCanceledRequestData.IsDeleted = true;
                                        context.Entry(DeliveryCanceledRequestData).State = EntityState.Modified;

                                        DeliveryCanceledRequestHistory DeliveryCanceledRequestHistoryAdd = new DeliveryCanceledRequestHistory();
                                        DeliveryCanceledRequestHistoryAdd.OrderId = ODM.OrderId;
                                        DeliveryCanceledRequestHistoryAdd.Comments = obj.CancelrequestComments;
                                        DeliveryCanceledRequestHistoryAdd.CreatedBy = userid;
                                        DeliveryCanceledRequestHistoryAdd.CreatedDate = DateTime.Now;
                                        DeliveryCanceledRequestHistoryAdd.IsActive = true;
                                        DeliveryCanceledRequestHistoryAdd.IsDeleted = false;
                                        context.DeliveryCanceledRequestHistoryDb.Add(DeliveryCanceledRequestHistoryAdd);
                                    }
                                    else
                                    {
                                        DeliveryCanceledRequestHistory DeliveryCanceledRequestHistoryAdd = new DeliveryCanceledRequestHistory();
                                        DeliveryCanceledRequestHistoryAdd.OrderId = ODM.OrderId;
                                        DeliveryCanceledRequestHistoryAdd.Comments = obj.CancelrequestComments;
                                        DeliveryCanceledRequestHistoryAdd.CreatedBy = userid;
                                        DeliveryCanceledRequestHistoryAdd.CreatedDate = DateTime.Now;
                                        DeliveryCanceledRequestHistoryAdd.IsActive = true;
                                        DeliveryCanceledRequestHistoryAdd.IsDeleted = false;
                                        context.DeliveryCanceledRequestHistoryDb.Add(DeliveryCanceledRequestHistoryAdd);
                                    }
                                    #endregion

                                    ODMaster.Status = obj.Status;
                                    ODMaster.comments = obj.comments;
                                    ODMaster.UpdatedDate = indianTime;
                                    context.Entry(ODMaster).State = EntityState.Modified;
                                    foreach (var detail in ODMaster.orderDetails)
                                    {
                                        detail.Status = obj.Status;
                                        detail.UpdatedDate = indianTime;
                                        context.Entry(detail).State = EntityState.Modified;
                                    }
                                }
                                var checkAssignment = context.DeliveryIssuanceDb.Where(z => z.DeliveryIssuanceId == ODM.DeliveryIssuanceIdOrderDeliveryMaster && (z.Status == "Submitted" || z.Status == "Payment Accepted" || z.Status == "Payment Submitted" || z.Status == "Freezed")).FirstOrDefault();
                                if (checkAssignment != null)
                                {
                                    var DeliveryIssuanceCheck = context.DeliveryIssuanceDb.Where(x => x.PeopleID == DeliveryIssuance.PeopleID && x.AgentId == DeliveryIssuance.AgentId && x.IsDeliveryCancel == true && (x.Status == "Submitted" || x.Status == "SavedAsDraft")).FirstOrDefault();
                                    var orderLists = context.OrderDispatchedMasters.Where(x => x.OrderId == ODM.OrderId && x.Status == "Delivery Redispatch").Include(x => x.orderDetails).ToList();

                                    postOrderDispatch = Mapper.Map(orderLists).ToANew<List<OrderDispatchedMaster>>();
                                    // List<int> InvalidOrderId = new List<int>();
                                    if (postOrderDispatch != null && postOrderDispatch.Any())
                                    {
                                        if (DeliveryIssuanceCheck != null)
                                        {
                                            // DeliveryIssuanceCheck.details = new List<IssuanceDetails>();
                                            var OrderIds = postOrderDispatch.Select(x => x.OrderId).Distinct().ToList();
                                            //Order Dispatch dboy update
                                            foreach (var item in OrderIds)
                                            {
                                                var orderlist = orderLists.Where(x => x.OrderId == item).FirstOrDefault();
                                                orderlist.DboyMobileNo = People.Mobile;
                                                orderlist.DboyName = People.DisplayName;
                                                orderlist.DeliveryCanceledComments = obj.CancelrequestComments;
                                                context.Entry(orderlist).State = EntityState.Modified;
                                                // context.Commit();
                                            }

                                            var OrderDispatchedDetailssList = context.OrderDispatchedDetailss.Where(x => OrderIds.Contains(x.OrderId) && x.qty > 0).ToList();
                                            var Assignmentpicklist = OrderDispatchedDetailssList.GroupBy(x => x.ItemMultiMRPId).Select(t =>
                                            new IssuanceDetails
                                            {
                                                OrderId = string.Join(",", OrderDispatchedDetailssList.Where(x => x.ItemMultiMRPId == t.Key).Select(x => x.OrderId).ToArray()),
                                                OrderQty = string.Join(",", OrderDispatchedDetailssList.Where(x => x.ItemMultiMRPId == t.Key).Select(a => String.Format("{0} - {1}", a.OrderId, a.qty)).ToArray()),
                                                OrderDispatchedMasterId = OrderDispatchedDetailssList.Where(x => x.ItemMultiMRPId == t.Key).Select(x => x.OrderDispatchedMasterId).FirstOrDefault(),
                                                OrderDispatchedDetailsId = OrderDispatchedDetailssList.Where(x => x.ItemMultiMRPId == t.Key).Select(x => x.OrderDispatchedDetailsId).FirstOrDefault(),
                                                qty = OrderDispatchedDetailssList.Where(x => x.ItemMultiMRPId == t.Key).Sum(x => x.qty),
                                                itemNumber = OrderDispatchedDetailssList.FirstOrDefault(x => x.ItemMultiMRPId == t.Key).itemNumber,
                                                ItemId = OrderDispatchedDetailssList.FirstOrDefault(x => x.ItemMultiMRPId == t.Key).ItemId,
                                                itemname = OrderDispatchedDetailssList.FirstOrDefault(x => x.ItemMultiMRPId == t.Key).itemname
                                            }).ToList();

                                            DeliveryIssuanceCheck.details = Assignmentpicklist;
                                            //  deliveryIssuance.TotalAssignmentAmount = 0;
                                            DeliveryIssuanceCheck.TotalAssignmentAmount += postOrderDispatch.Sum(x => x.GrossAmount);
                                            DeliveryIssuanceCheck.OrderdispatchIds += "," + string.Join(",", OrderDispatchedDetailssList.Select(x => x.OrderDispatchedMasterId).Distinct());
                                            DeliveryIssuanceCheck.OrderIds += "," + string.Join(",", OrderDispatchedDetailssList.Select(x => x.OrderId).Distinct());
                                            context.Entry(DeliveryIssuanceCheck).State = EntityState.Modified;

                                            foreach (var item in OrderIds)
                                            {
                                                var orderlist = orderLists.Where(x => x.OrderId == item).FirstOrDefault();
                                                orderlist.DeliveryIssuanceIdOrderDeliveryMaster = DeliveryIssuanceCheck.DeliveryIssuanceId;
                                                context.Entry(orderlist).State = EntityState.Modified;
                                                // context.Commit();
                                            }
                                            #region Code For OrderDeliveryMaster

                                            var payments = context.PaymentResponseRetailerAppDb.Where(x => x.OrderId == ODM.OrderId && x.status == "success" && x.PaymentFrom.ToLower() == "epaylater");
                                            double? epaylateramt = payments.Any() ? payments.Sum(x => x.amount) : 0;

                                            OrderDeliveryMaster oDm = new OrderDeliveryMaster();
                                            oDm.OrderId = ODM.OrderId;
                                            oDm.CityId = ODM.CityId;
                                            oDm.CompanyId = ODM.CompanyId;
                                            oDm.WarehouseId = ODM.WarehouseId;
                                            oDm.WarehouseName = ODM.WarehouseName;
                                            //oDm.SalesPersonId = OrderDMaster.SalesPersonId;
                                            //oDm.SalesPerson = OrderDMaster.SalesPerson;
                                            //oDm.SalesMobile = OrderDMaster.SalesMobile;
                                            oDm.DboyMobileNo = ODM.DboyMobileNo;
                                            oDm.DboyName = ODM.DboyName;
                                            oDm.CustomerId = ODM.CustomerId;
                                            oDm.CustomerName = ODM.CustomerName;
                                            oDm.Customerphonenum = ODM.Customerphonenum;
                                            oDm.ShopName = ODM.ShopName;
                                            oDm.Skcode = ODM.Skcode;
                                            oDm.Status = "Delivery Canceled"; //OrderDMaster.Status;
                                            oDm.ShippingAddress = ODM.ShippingAddress;
                                            oDm.BillingAddress = ODM.BillingAddress;
                                            oDm.CanceledStatus = ODM.CanceledStatus;
                                            oDm.invoice_no = ODM.invoice_no;
                                            oDm.OnlineServiceTax = ODM.OnlineServiceTax;
                                            oDm.TotalAmount = ODM.TotalAmount;
                                            oDm.GrossAmount = ODM.GrossAmount;
                                            oDm.TaxAmount = ODM.TaxAmount;
                                            oDm.SGSTTaxAmmount = ODM.SGSTTaxAmmount;
                                            oDm.CGSTTaxAmmount = ODM.CGSTTaxAmmount;
                                            oDm.ReDispatchedStatus = ODM.ReDispatchedStatus;
                                            oDm.Trupay = ODM.Trupay;
                                            oDm.comments = ODM.comments;
                                            oDm.deliveryCharge = ODM.deliveryCharge;
                                            oDm.DeliveryIssuanceId = DeliveryIssuanceCheck.DeliveryIssuanceId;
                                            oDm.DiscountAmount = ODM.DiscountAmount;
                                            oDm.CheckNo = ODM.CheckNo;
                                            oDm.CheckAmount = ODM.CheckAmount;
                                            oDm.ElectronicPaymentNo = ODM.ElectronicPaymentNo;
                                            oDm.ElectronicAmount = ODM.ElectronicAmount;
                                            oDm.EpayLaterAmount = epaylateramt;
                                            oDm.CashAmount = ODM.CashAmount;
                                            oDm.OrderedDate = ODM.OrderedDate;
                                            oDm.WalletAmount = ODM.WalletAmount;
                                            oDm.RewardPoint = ODM.RewardPoint;
                                            oDm.Tin_No = ODM.Tin_No;
                                            oDm.ReDispatchCount = ODM.ReDispatchCount;
                                            oDm.UpdatedDate = indianTime;
                                            oDm.CreatedDate = indianTime;
                                            context.OrderDeliveryMasterDB.Add(oDm);
                                            #endregion
                                            OrderMasterHistories OrderMasterHistories = new OrderMasterHistories();
                                            OrderMasterHistories.orderid = ODM.OrderId;
                                            OrderMasterHistories.Status = ODM.Status;
                                            OrderMasterHistories.Reasoncancel = null;
                                            OrderMasterHistories.Warehousename = ODM.WarehouseName;
                                            OrderMasterHistories.DeliveryIssuanceId = DeliveryIssuanceCheck.DeliveryIssuanceId;
                                            OrderMasterHistories.username = People.DisplayName != null ? People.DisplayName : People.PeopleFirstName; ;
                                            OrderMasterHistories.userid = People.PeopleID;
                                            OrderMasterHistories.CreatedDate = DateTime.Now;
                                            context.OrderMasterHistoriesDB.Add(OrderMasterHistories);
                                        }
                                        else
                                        {
                                            postOrderDispatch = Mapper.Map(orderLists).ToANew<List<OrderDispatchedMaster>>();
                                            // List<int> InvalidOrderId = new List<int>();
                                            if (postOrderDispatch != null && postOrderDispatch.Any())
                                            {
                                                var peopleid = new SqlParameter("@PeopleId", People.PeopleID);
                                                var WarehouseId = new SqlParameter("@WarehoueId", People.WarehouseId);
                                                var Cityid = new SqlParameter("@CityId", People.Cityid ?? 0);
                                                var AgentId = new SqlParameter("@AgentId", DeliveryIssuance.AgentId);

                                                var newAssignmentId = context.Database.SqlQuery<Int64>("Exec GetNewAssignmentForDeliveryCancel @PeopleId,@WarehoueId,@CityId,@AgentId", peopleid, WarehouseId, Cityid, AgentId).FirstOrDefault();
                                                DeliveryIssuance AdddeliveryIssuance = context.DeliveryIssuanceDb.FirstOrDefault(x => x.DeliveryIssuanceId == newAssignmentId);
                                                AdddeliveryIssuance.userid = People.PeopleID;
                                                AdddeliveryIssuance.WarehouseId = People.WarehouseId;
                                                AdddeliveryIssuance.DisplayName = People.DisplayName;
                                                AdddeliveryIssuance.PeopleID = People.PeopleID;
                                                AdddeliveryIssuance.AgentId = DeliveryIssuance.AgentId;
                                                AdddeliveryIssuance.Cityid = People.Cityid ?? 0;
                                                AdddeliveryIssuance.WarehouseId = People.WarehouseId;
                                                AdddeliveryIssuance.CreatedDate = indianTime;
                                                AdddeliveryIssuance.UpdatedDate = indianTime;
                                                AdddeliveryIssuance.OrderdispatchIds = "";
                                                AdddeliveryIssuance.OrderIds = "";
                                                AdddeliveryIssuance.TotalAssignmentAmount = 0;
                                                AdddeliveryIssuance.IsDeliveryCancel = true;
                                                AdddeliveryIssuance.Status = "Submitted";
                                                AdddeliveryIssuance.Acceptance = true;
                                                AdddeliveryIssuance.details = new List<IssuanceDetails>();

                                                var OrderIds = postOrderDispatch.Select(x => x.OrderId).Distinct().ToList();
                                                //Order Dispatch dboy update
                                                foreach (var item in OrderIds)
                                                {
                                                    var orderlist = orderLists.Where(x => x.OrderId == item).FirstOrDefault();
                                                    orderlist.DboyMobileNo = People.Mobile;
                                                    orderlist.DboyName = People.DisplayName;
                                                    orderlist.DeliveryCanceledComments = obj.comments;
                                                    context.Entry(orderlist).State = EntityState.Modified;
                                                    // context.Commit();
                                                }
                                                var OrderDispatchedDetailssList = context.OrderDispatchedDetailss.Where(x => OrderIds.Contains(x.OrderId) && x.qty > 0).ToList();
                                                var Assignmentpicklist = OrderDispatchedDetailssList.GroupBy(x => x.ItemMultiMRPId).Select(t =>
                                                new IssuanceDetails
                                                {
                                                    OrderId = string.Join(",", OrderDispatchedDetailssList.Where(x => x.ItemMultiMRPId == t.Key).Select(x => x.OrderId).ToArray()),
                                                    OrderQty = string.Join(",", OrderDispatchedDetailssList.Where(x => x.ItemMultiMRPId == t.Key).Select(a => String.Format("{0} - {1}", a.OrderId, a.qty)).ToArray()),
                                                    OrderDispatchedMasterId = OrderDispatchedDetailssList.Where(x => x.ItemMultiMRPId == t.Key).Select(x => x.OrderDispatchedMasterId).FirstOrDefault(),
                                                    OrderDispatchedDetailsId = OrderDispatchedDetailssList.Where(x => x.ItemMultiMRPId == t.Key).Select(x => x.OrderDispatchedDetailsId).FirstOrDefault(),
                                                    qty = OrderDispatchedDetailssList.Where(x => x.ItemMultiMRPId == t.Key).Sum(x => x.qty),
                                                    itemNumber = OrderDispatchedDetailssList.FirstOrDefault(x => x.ItemMultiMRPId == t.Key).itemNumber,
                                                    ItemId = OrderDispatchedDetailssList.FirstOrDefault(x => x.ItemMultiMRPId == t.Key).ItemId,
                                                    itemname = OrderDispatchedDetailssList.FirstOrDefault(x => x.ItemMultiMRPId == t.Key).itemname
                                                }).ToList();

                                                AdddeliveryIssuance.details = Assignmentpicklist;
                                                AdddeliveryIssuance.TotalAssignmentAmount = 0;
                                                AdddeliveryIssuance.TotalAssignmentAmount += postOrderDispatch.Sum(x => x.GrossAmount);
                                                AdddeliveryIssuance.OrderdispatchIds = string.Join(",", OrderDispatchedDetailssList.Select(x => x.OrderDispatchedMasterId).Distinct());
                                                AdddeliveryIssuance.OrderIds = string.Join(",", OrderDispatchedDetailssList.Select(x => x.OrderId).Distinct());
                                                //AdddeliveryIssuance.Status = "Submitted"; //"Assigned";
                                                //AdddeliveryIssuance.IsActive = true;
                                                context.Entry(AdddeliveryIssuance).State = EntityState.Modified;
                                                // context.DeliveryIssuanceDb.Add(AdddeliveryIssuance);

                                                foreach (var item in OrderIds)
                                                {
                                                    var orderlist = orderLists.Where(x => x.OrderId == item).FirstOrDefault();
                                                    orderlist.DeliveryIssuanceIdOrderDeliveryMaster = AdddeliveryIssuance.DeliveryIssuanceId;
                                                    context.Entry(orderlist).State = EntityState.Modified;
                                                    // context.Commit();
                                                }

                                                #region Code For OrderDeliveryMaster

                                                var payments = context.PaymentResponseRetailerAppDb.Where(x => x.OrderId == ODM.OrderId && x.status == "success" && x.PaymentFrom.ToLower() == "epaylater");
                                                double? epaylateramt = payments.Any() ? payments.Sum(x => x.amount) : 0;

                                                OrderDeliveryMaster oDm = new OrderDeliveryMaster();
                                                oDm.OrderId = ODM.OrderId;
                                                oDm.CityId = ODM.CityId;
                                                oDm.CompanyId = ODM.CompanyId;
                                                oDm.WarehouseId = ODM.WarehouseId;
                                                oDm.WarehouseName = ODM.WarehouseName;
                                                //oDm.SalesPersonId = OrderDMaster.SalesPersonId;
                                                //oDm.SalesPerson = OrderDMaster.SalesPerson;
                                                //oDm.SalesMobile = OrderDMaster.SalesMobile;
                                                oDm.DboyMobileNo = ODM.DboyMobileNo;
                                                oDm.DboyName = ODM.DboyName;
                                                oDm.CustomerId = ODM.CustomerId;
                                                oDm.CustomerName = ODM.CustomerName;
                                                oDm.Customerphonenum = ODM.Customerphonenum;
                                                oDm.ShopName = ODM.ShopName;
                                                oDm.Skcode = ODM.Skcode;
                                                oDm.Status = "Delivery Canceled"; //OrderDMaster.Status;
                                                oDm.ShippingAddress = ODM.ShippingAddress;
                                                oDm.BillingAddress = ODM.BillingAddress;
                                                oDm.CanceledStatus = ODM.CanceledStatus;
                                                oDm.invoice_no = ODM.invoice_no;
                                                oDm.OnlineServiceTax = ODM.OnlineServiceTax;
                                                oDm.TotalAmount = ODM.TotalAmount;
                                                oDm.GrossAmount = ODM.GrossAmount;
                                                oDm.TaxAmount = ODM.TaxAmount;
                                                oDm.SGSTTaxAmmount = ODM.SGSTTaxAmmount;
                                                oDm.CGSTTaxAmmount = ODM.CGSTTaxAmmount;
                                                oDm.ReDispatchedStatus = ODM.ReDispatchedStatus;
                                                oDm.Trupay = ODM.Trupay;
                                                oDm.comments = ODM.comments;
                                                oDm.deliveryCharge = ODM.deliveryCharge;
                                                oDm.DeliveryIssuanceId = AdddeliveryIssuance.DeliveryIssuanceId;
                                                oDm.DiscountAmount = ODM.DiscountAmount;
                                                oDm.CheckNo = ODM.CheckNo;
                                                oDm.CheckAmount = ODM.CheckAmount;
                                                oDm.ElectronicPaymentNo = ODM.ElectronicPaymentNo;
                                                oDm.ElectronicAmount = ODM.ElectronicAmount;
                                                oDm.EpayLaterAmount = epaylateramt;
                                                oDm.CashAmount = ODM.CashAmount;
                                                oDm.OrderedDate = ODM.OrderedDate;
                                                oDm.WalletAmount = ODM.WalletAmount;
                                                oDm.RewardPoint = ODM.RewardPoint;
                                                oDm.Tin_No = ODM.Tin_No;
                                                oDm.ReDispatchCount = ODM.ReDispatchCount;
                                                oDm.UpdatedDate = indianTime;
                                                oDm.CreatedDate = indianTime;
                                                context.OrderDeliveryMasterDB.Add(oDm);
                                                #endregion
                                                OrderMasterHistories OrderMasterHistories = new OrderMasterHistories();
                                                OrderMasterHistories.orderid = ODM.OrderId;
                                                OrderMasterHistories.Status = ODM.Status;
                                                OrderMasterHistories.Reasoncancel = null;
                                                OrderMasterHistories.Warehousename = ODM.WarehouseName;
                                                OrderMasterHistories.DeliveryIssuanceId = AdddeliveryIssuance.DeliveryIssuanceId;
                                                OrderMasterHistories.username = People.DisplayName != null ? People.DisplayName : People.PeopleFirstName; ;
                                                OrderMasterHistories.userid = People.PeopleID;
                                                OrderMasterHistories.CreatedDate = DateTime.Now;
                                                context.OrderMasterHistoriesDB.Add(OrderMasterHistories);
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    res = new ResDTO()
                                    {
                                        op = null,
                                        Message = "Assignment Is not Submitted !!",
                                        Status = Status
                                    };
                                    ordersToProcess.RemoveAll(x => x == obj.OrderId);
                                    return Request.CreateResponse(HttpStatusCode.OK, res);
                                }
                            }
                            else if (ODM.Status == "Delivery Canceled")
                            {
                                Status = true;
                                msg = "Order already Delivery Canceled";
                                res = new ResDTO()
                                {
                                    op = obj,
                                    Message = msg,
                                    Status = Status
                                };
                                ordersToProcess.RemoveAll(x => x == obj.OrderId);

                                return Request.CreateResponse(HttpStatusCode.OK, res);
                            }
                        }
                        else
                        {

                            res = new ResDTO()
                            {
                                op = null,
                                Message = "Not Delivered",
                                Status = Status
                            };
                            ordersToProcess.RemoveAll(x => x == obj.OrderId);
                            return Request.CreateResponse(HttpStatusCode.OK, res);
                        }
                        if (context.Commit() > 0)
                        {
                            #region stock Hit on poc
                            //for currentstock
                            if (ODMaster != null && ODMaster.OrderType != 5 && ODM.Status == "Delivery Canceled")
                            {

                                MultiStockHelper<OnDeliveryCanceledStockEntryDc> MultiStockHelpers = new MultiStockHelper<OnDeliveryCanceledStockEntryDc>();
                                List<OnDeliveryCanceledStockEntryDc> DeliveryCanceledCStockList = new List<OnDeliveryCanceledStockEntryDc>();
                                foreach (var StockHit in ODM.orderDetails.Where(x => x.qty > 0))
                                {
                                    var RefStockCode = ODMaster.OrderType == 8 ? "CL" : "C";
                                    bool isFree = ODMaster.orderDetails.Any(c => c.OrderDetailsId == StockHit.OrderDetailsId && c.IsFreeItem && c.IsDispatchedFreeStock);
                                    if (isFree) { RefStockCode = "F"; }
                                    else if (ODMaster.OrderType == 6) //6 Damage stock
                                    {
                                        RefStockCode = "D";
                                    }
                                    DeliveryCanceledCStockList.Add(new OnDeliveryCanceledStockEntryDc
                                    {
                                        ItemMultiMRPId = StockHit.ItemMultiMRPId,
                                        OrderDispatchedDetailsId = StockHit.OrderDispatchedDetailsId,
                                        OrderId = StockHit.OrderId,
                                        Qty = StockHit.qty,
                                        UserId = People.PeopleID,
                                        WarehouseId = StockHit.WarehouseId,
                                        RefStockCode = RefStockCode,
                                        IsDeliveryRedispatchCancel = true
                                    });
                                }
                                if (DeliveryCanceledCStockList.Any())
                                {
                                    bool IsUpdated = MultiStockHelpers.MakeEntry(DeliveryCanceledCStockList, "Stock_OnDeliveryCancel_New", context, dbContextTransaction);
                                    if (!IsUpdated)
                                    {
                                        Status = false;
                                        dbContextTransaction.Dispose();
                                        res = new ResDTO()
                                        {
                                            op = null,
                                            Message = "Order Not Delivery Canceled",
                                            Status = Status
                                        };
                                        ordersToProcess.RemoveAll(x => x == obj.OrderId);
                                        return Request.CreateResponse(HttpStatusCode.OK, res);
                                    }
                                }

                            }
                            #endregion
                            dbContextTransaction.Complete();
                            Status = true;
                        }
                        else
                        {
                            dbContextTransaction.Dispose();
                            Status = false;
                        }
                        res = new ResDTO()
                        {
                            op = obj,
                            Message = "Success",
                            Status = Status
                        };
                        ordersToProcess.RemoveAll(x => x == obj.OrderId);
                        return Request.CreateResponse(HttpStatusCode.OK, res);
                    }
                }
            }

            catch (Exception ex)
            {
                ordersToProcess.RemoveAll(x => x == obj.OrderId);
                throw ex;
            }

        }

        #region SendMailCreditWalletNotification
        //SendMailCreditWalletNotification
        public static void SendMailAssignmentCopy(int id, string UploadedFileName, Int32 DeliveryIssuanceId)
        {
            using (var db = new AuthContext())
            {
                try
                {


                    People people = db.Peoples.Where(x => x.PeopleID == id && x.Deleted == false).FirstOrDefault();
                    // Warehouse Warehouses = db.Warehouses.Where(x => x.WarehouseId == people.WarehouseId && x.Deleted == false).FirstOrDefault();
                    // string Wemail = "no";
                    // if (Warehouses.Email != null)
                    // {
                    //      Wemail = Warehouses.Email;
                    // }

                    string masteremail = ConfigurationManager.AppSettings["MasterEmail"];
                    string masterpassword = ConfigurationManager.AppSettings["MasterPassword"];
                    string body = "<div style='background: #FAFAFA; color: #333333; padding-left: 30px;font-family: arial,sans-serif; font-size: 14px;'>";
                    body += "Hello,";
                    body += "<p><strong>";
                    body += " Here With attched Freezed Assignment Copy : </strong> " + DeliveryIssuanceId + "</p>";
                    body += "Thanks,";
                    body += "<br />";
                    body += "<b>IT Team</b>";
                    body += "</div>";
                    var Subj = "Alert! Freezed Assignment copy : " + DeliveryIssuanceId + "  By User " + people.DisplayName;
                    var msg = new MailMessage("donotreply_backend@shopkirana.com", " donotreply_assignment@shopkirana.com", Subj, body);
                    System.Net.Mail.Attachment attachment;
                    attachment = new System.Net.Mail.Attachment(Startup.FreezedAsignmentCopyFilePath + UploadedFileName);
                    msg.Attachments.Add(attachment);
                    // msg.To.Add("deepak@shopkirana.com");
                    /// msg.To.Add("manasi@shopkirana.com");
                    //if (Wemail != "no")
                    // { msg.To.Add(Wemail); }
                    msg.IsBodyHtml = true;
                    var smtpClient = new SmtpClient("smtp.gmail.com", 587); //if your from email address is "from@hotmail.com" then host should be "smtp.hotmail.com"
                    smtpClient.UseDefaultCredentials = true;
                    smtpClient.Credentials = new NetworkCredential(masteremail, masterpassword);
                    smtpClient.EnableSsl = true;
                    smtpClient.Send(msg);

                }
                catch (Exception ss)
                {

                }
            }
        }


        #region 
        /// <summary>
        ///Assignment Order V1 Created by 27-06-2019 Deapp 
        /// </summary>
        /// <param name="DeliveryIssuanceId"></param>
        /// <param name="mob"></param>
        /// <returns></returns>
        [Route("AssignmentOrder/V1")]
        [HttpGet]

        public HttpResponseMessage AssignmentOrderV1(Int32 DeliveryIssuanceId, string mob, int page, int list, int OrderId)
        {
            OrderDispatchedListDtoObj res;
            try
            {
                page = (page - 1) * list;
                using (AuthContext authContext = new AuthContext())
                {
                    if (mob != null && DeliveryIssuanceId > 0)
                    {
                        // var DBoyorders = authContext.getAcceptedAssignmentOrderV1(DeliveryIssuanceId, mob);
                        List<OrderDispatchedMasterDTOM> DBoyorders = new List<OrderDispatchedMasterDTOM>();
                        List<object> parameters = new List<object>();
                        using (var context = new AuthContext())
                        {
                            var DeliveryIssuanceIdparam = new SqlParameter
                            {
                                ParameterName = "AssingmentId",
                                Value = DeliveryIssuanceId
                            };
                            parameters.Add(DeliveryIssuanceIdparam);
                            var mobparam = new SqlParameter
                            {
                                ParameterName = "DboyMobileNo",
                                Value = mob
                            };
                            parameters.Add(mobparam);
                            var OrderIdparam = new SqlParameter
                            {
                                ParameterName = "OrderId",
                                Value = OrderId
                            };
                            parameters.Add(OrderIdparam);
                            var Skipparam = new SqlParameter
                            {
                                ParameterName = "skip",
                                Value = page
                            };
                            parameters.Add(Skipparam);
                            var Takeparam = new SqlParameter
                            {
                                ParameterName = "take",
                                Value = list
                            };
                            parameters.Add(Takeparam);

                            DBoyorders = context.Database.SqlQuery<OrderDispatchedMasterDTOM>("exec AssignmentOrderDetailsForDeliveryApp @OrderId,@AssingmentId,@DboyMobileNo,@skip,@take", parameters.ToArray()).ToList();
                        }



                        if (DBoyorders.Count == 0)
                        {
                            res = new OrderDispatchedListDtoObj()
                            {
                                OrderDispatchedObj = null,
                                status = false,
                                Message = "No Order Found!!"
                            };
                        }
                        else
                        {
                            var monthlycustomerlevel = new MongoDbHelper<MonthlyCustomerLevel>();

                            var customerlevel = new MonthlyCustomerLevel();

                            customerlevel = monthlycustomerlevel.Select(x => x.Month == DateTime.Now.Month && x.Year == DateTime.Now.Year).FirstOrDefault();

                            var levels = customerlevel?.CustomerLevels;

                            if (levels != null && levels.Any())
                            {
                                foreach (var item in DBoyorders)
                                {
                                    var leveldata = levels.Where(x => x.SKCode == item.Skcode).Select(x => new { LevelName = x.LevelName, ColourCode = x.ColourCode }).FirstOrDefault();
                                    if (leveldata != null)
                                    {
                                        //item.CustomerLevel = leveldata.LevelName;
                                        item.ColourCode = leveldata.ColourCode;
                                    }
                                }
                            }

                            res = new OrderDispatchedListDtoObj()
                            {
                                OrderDispatchedObj = DBoyorders,
                                status = true,
                                Message = "Success",
                                TotalOrderCount = DBoyorders.FirstOrDefault().TotalOrderCount,
                                IsShippedAssingId = DBoyorders.FirstOrDefault().IsShippedAssingId,
                            };
                        }
                    }
                    else
                    {
                        res = new OrderDispatchedListDtoObj()
                        {
                            OrderDispatchedObj = null,
                            status = false,
                            Message = "Data not exist."
                        };
                    }

                    return Request.CreateResponse(HttpStatusCode.OK, res);
                }
            }
            catch (Exception ex)
            {
                res = new OrderDispatchedListDtoObj()
                {
                    OrderDispatchedObj = null,
                    status = false,
                    Message = "Failed. " + ex.Message + ""
                };
                return Request.CreateResponse(HttpStatusCode.BadRequest, res);
            }
        }

        [Route("GetRejectedAssignment")]
        [HttpGet]
        public HttpResponseMessage GetRejectedAssignment(int id)
        {
            ResListAcceptPending res;
            try
            {
                using (AuthContext authContext = new AuthContext())
                {

                    var data = (from a in authContext.DeliveryIssuanceDb
                                where (a.IsActive == false && a.PeopleID == id)
                                join i in authContext.AssignmentRechangeOrder.Where(x => x.Status == 1 && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value))
                                on a.DeliveryIssuanceId equals i.AssignmentId
                                select new AssignmentAcceptPendingDTO
                                {
                                    DeliveryIssuanceId = a.DeliveryIssuanceId,
                                    AssignmentDate = a.CreatedDate
                                }).Distinct().ToList();

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

        [Route("AssignmentOrderStatusUpdate")]
        [HttpGet]
        public bool AssignmentOrderStatusUpdate(int assignmentId, int orderId, int status)
        {
            var identity = User.Identity as ClaimsIdentity;
            int userid = 0;
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

            using (AuthContext context = new AuthContext())
            {


                var param = new SqlParameter("DeliveryIssuanceId", assignmentId);
                bool IsTripAssignment = context.Database.SqlQuery<bool>("exec operation.IsTripAssignment @DeliveryIssuanceId", param).FirstOrDefault();

                if (context.DeliveryIssuanceDb.Any(x => x.DeliveryIssuanceId == assignmentId && x.Status == "Submitted") && !IsTripAssignment)
                {
                    //var people = context.Peoples.Where(x => x.PeopleID == userid).FirstOrDefault();
                    //var WarehouseName = context.DbOrderMaster.Where(x => x.OrderId == orderId).Select(z=>z.WarehouseName).FirstOrDefault();
                    var AssignmentRechangeOrder = context.AssignmentRechangeOrder.FirstOrDefault(x => x.AssignmentId == assignmentId && x.OrderId == orderId);
                    if (AssignmentRechangeOrder != null)
                    {

                        AssignmentRechangeOrder.Status = status;
                        AssignmentRechangeOrder.ModifiedDate = indianTime;
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
                            OrderId = orderId,
                            Status = status,
                            AssignmentId = assignmentId,
                            CreatedBy = userid
                        };
                        context.AssignmentRechangeOrder.Add(assignmentRechangeOrder);

                        //OrderMasterHistories OrderMasterHistories = new OrderMasterHistories();
                        //OrderMasterHistories.orderid = orderId;
                        //OrderMasterHistories.Status = "Shipped";
                        //OrderMasterHistories.Reasoncancel = "by Reject Assignment";
                        //OrderMasterHistories.Warehousename = WarehouseName;
                        //OrderMasterHistories.DeliveryIssuanceId = assignmentId;
                        //OrderMasterHistories.username = people.DisplayName != null ? people.DisplayName : people.PeopleFirstName; ;
                        //OrderMasterHistories.userid = people.PeopleID;
                        //OrderMasterHistories.CreatedDate = DateTime.Now;
                        //context.OrderMasterHistoriesDB.Add(OrderMasterHistories);
                    }
                    if (context.Commit() > 0)
                    {

                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }

        }

        [Route("GetDeclineAssignmentOrder")]
        [HttpGet]
        public HttpResponseMessage GetDeclineAssignmentOrder(Int32 DeliveryIssuanceId, string mob)
        {
            OrderDispatchedListDtoObj res;
            try
            {
                using (AuthContext authContext = new AuthContext())
                {
                    var OrderId = authContext.AssignmentRechangeOrder.Where(x => x.AssignmentId == DeliveryIssuanceId && x.Status == 1).Select(x => x.OrderId).ToList();

                    // var DBoyorders = authContext.getAcceptedAssignmentOrderV1(DeliveryIssuanceId, mob);
                    List<OrderDispatchedMasterDTOM> DBoyorders = new List<OrderDispatchedMasterDTOM>();
                    if (OrderId != null && OrderId.Any())
                    {
                        DBoyorders = (from a in authContext.OrderDispatchedMasters
                                      where (a.DboyMobileNo == mob && OrderId.Contains(a.OrderId))
                                      join i in authContext.Customers on a.CustomerId equals i.CustomerId
                                      select new OrderDispatchedMasterDTOM
                                      {
                                          lat = i.lat,
                                          lg = i.lg,
                                          ClusterId = a.ClusterId,
                                          ClusterName = a.ClusterName,
                                          active = a.active,
                                          BillingAddress = a.BillingAddress,
                                          CityId = a.CityId,
                                          comments = a.comments,
                                          CompanyId = a.CompanyId,
                                          CreatedDate = a.CreatedDate,
                                          CustomerId = a.CustomerId,
                                          CustomerName = a.CustomerName,
                                          ShopName = i.ShopName,
                                          Skcode = i.Skcode,
                                          Customerphonenum = a.Customerphonenum,
                                          DboyMobileNo = a.DboyMobileNo,
                                          DboyName = a.DboyName,
                                          Deleted = a.Deleted,
                                          Deliverydate = a.Deliverydate,
                                          DiscountAmount = a.DiscountAmount,
                                          DivisionId = a.DivisionId,
                                          GrossAmount = a.GrossAmount,
                                          invoice_no = a.invoice_no,
                                          OrderDetailsCount = a.orderDetails.Count,
                                          OrderDispatchedMasterId = a.OrderDispatchedMasterId,
                                          OrderId = a.OrderId,
                                          ReDispatchCount = a.ReDispatchCount,
                                          DeliveryIssuanceId = a.DeliveryIssuanceIdOrderDeliveryMaster,
                                          //SalesPerson = a.SalesPerson,
                                          //SalesPersonId = a.SalesPersonId,
                                          ShippingAddress = a.ShippingAddress,
                                          Status = a.Status,
                                          TaxAmount = a.TaxAmount,
                                          TotalAmount = a.TotalAmount,
                                          UpdatedDate = a.UpdatedDate,
                                          WarehouseId = a.WarehouseId,
                                          WarehouseName = a.WarehouseName,
                                          OrderDate = a.OrderedDate
                                      }).ToList();
                    }
                    if (DBoyorders.Count == 0)
                    {
                        res = new OrderDispatchedListDtoObj()
                        {
                            OrderDispatchedObj = null,
                            status = false,
                            Message = "Data not exist."
                        };
                    }
                    else
                    {
                        //var monthlycustomerlevel = new MongoDbHelper<MonthlyCustomerLevel>();

                        //var customerlevel = new MonthlyCustomerLevel();

                        //customerlevel = monthlycustomerlevel.Select(x => x.Month == DateTime.Now.Month && x.Year == DateTime.Now.Year).FirstOrDefault();

                        //var levels = customerlevel?.CustomerLevels;

                        //if (levels != null && levels.Any())
                        //{
                        //    foreach (var item in DBoyorders)
                        //    {
                        //        var leveldata = levels.Where(x => x.SKCode == item.Skcode).Select(x => new { LevelName = x.LevelName, ColourCode = x.ColourCode }).FirstOrDefault();
                        //        if (leveldata != null)
                        //        {
                        //            //item.CustomerLevel = leveldata.LevelName;
                        //            item.ColourCode = leveldata.ColourCode;
                        //        }
                        //    }
                        //}

                        res = new OrderDispatchedListDtoObj()
                        {
                            OrderDispatchedObj = DBoyorders,
                            status = true,
                            Message = "Success"
                        };
                    }
                    return Request.CreateResponse(HttpStatusCode.OK, res);
                }
            }
            catch (Exception ex)
            {
                res = new OrderDispatchedListDtoObj()
                {
                    OrderDispatchedObj = null,
                    status = false,
                    Message = "Failed. " + ex.Message + ""
                };
                return Request.CreateResponse(HttpStatusCode.BadRequest, res);
            }
        }
        #endregion

        #region Generate Random OTP
        [AllowAnonymous]
        [HttpGet]
        [Route("GenerateOTPForOrder")]
        public bool GenerateOTPForOrder(int OrderId, string Status, double? lat, double? lg,string VideoUrl)
        {
            int CustOtp = 0;
            int SalesOtp = 0;

            string sRandomOTP = "";
            bool result = false;
            var Notification = "";
            var sent = false;

            try
            {
                var identity = User.Identity as ClaimsIdentity;
                int userid = 0;

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                    userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
                string requireStatus = "Delivered,Delivery Canceled Request,Delivery Redispatch,Delivery Canceled";
                string NewStatus = "Delivered,Delivery Canceled Request";
                using (AuthContext context = new AuthContext())
                {
                    if (context.OrderDispatchedMasters.Any(x => x.OrderId == OrderId && x.Status == "Shipped"))
                    {
                        if (requireStatus.Split(',').ToList().Contains(Status))
                        {
                            if (NewStatus.Split(',').ToList().Contains(Status))
                            {
                                string[] saAllowedCharacters = { "1", "2", "3", "4", "5", "6", "7", "8", "9", "0" };
                                sRandomOTP = GenerateRandomOTP(4, saAllowedCharacters);

                                if (!string.IsNullOrEmpty(sRandomOTP))
                                {
                                    var orderDeliveryOTPs = context.OrderDeliveryOTP.Where(x => x.OrderId == OrderId && x.IsActive);
                                    if (orderDeliveryOTPs != null)
                                    {
                                        foreach (var orderDeliveryOTP in orderDeliveryOTPs)
                                        {

                                            orderDeliveryOTP.ModifiedDate = DateTime.Now;
                                            orderDeliveryOTP.ModifiedBy = userid;
                                            orderDeliveryOTP.IsActive = false;
                                            //context.OrderDeliveryOTP.Attach(orderDeliveryOTP);
                                            context.Entry(orderDeliveryOTP).State = EntityState.Modified;

                                        }
                                    }
                                    OrderDeliveryOTP OrderDeliveryOTP = new OrderDeliveryOTP
                                    {
                                        CreatedBy = userid,
                                        CreatedDate = DateTime.Now,
                                        IsActive = true,
                                        OrderId = OrderId,
                                        OTP = sRandomOTP,
                                        Status = Status,
                                        lat = lat,
                                        lg = lg,
                                        VideoUrl = VideoUrl
                                    };
                                    context.OrderDeliveryOTP.Add(OrderDeliveryOTP);
                                    result = context.Commit() > 0;
                                }
                            }

                            var param = new SqlParameter("@OrderId", OrderId);
                            var param1 = new SqlParameter("@orderStatus", Status);
                            var orderMobiledetail = context.Database.SqlQuery<OrderMobiledetail>("Exec GetOrderMobileDetailsOTP  @OrderId,@orderStatus", param, param1).FirstOrDefault();

                            var ODM = context.DbOrderMaster.Where(x => x.OrderId == OrderId).SingleOrDefault();
                            if (orderMobiledetail != null && ODM != null)
                            {
                                List<SalesLeadMobile> SalesLeadMobile = new List<SalesLeadMobile>();
                                MongoDbHelper<DataContracts.Mongo.RdCancellationConfiguration> mongoDb = new MongoDbHelper<DataContracts.Mongo.RdCancellationConfiguration>();
                                List<DataContracts.Mongo.RdCancellationConfiguration> MongoData = new List<RdCancellationConfiguration>();
                                Random random = new Random();
                                CustOtp = random.Next(1000, 10000);
                                SalesOtp = random.Next(1000, 10000);
                                switch (Status)
                                {
                                    case "Delivered":
                                        if (!string.IsNullOrEmpty(orderMobiledetail.customermobile))
                                        {
                                            if (orderMobiledetail != null && orderMobiledetail.OrderType == 5)
                                            {
                                                //Getotp(orderMobiledetail.customermobile, " is OTP for delivery of Order No (" + ordermaster.invoice_no + ") Shopkirana", sRandomOTP);
                                                string message = ""; //"{#var1#} is Delivery Code for delivery of Order No. {#var2#} for Total Qty {#var3#} and Value of Rs. {#var4#}. Shopkirana";
                                                var dltSMS = SMSTemplateHelper.getTemplateText((int)AppEnum.RetailerApp, "Order_Delivery_Delivered");
                                                message = dltSMS == null ? "" : dltSMS.Template;

                                                message = message.Replace("{#var1#}", sRandomOTP);
                                                message = message.Replace("{#var2#}", OrderId.ToString());
                                                message = message.Replace("{#var3#}", orderMobiledetail.TotalQty.ToString());
                                                message = message.Replace("{#var4#}", orderMobiledetail.OrderAmount.ToString());
                                                if (dltSMS != null)
                                                    Common.Helpers.SendSMSHelper.SendSMS(orderMobiledetail.customermobile, message, ((Int32)Common.Enums.SMSRouteEnum.OTP).ToString(), dltSMS.DLTId);

                                            }
                                            else
                                            {
                                                //Getotp(orderMobiledetail.customermobile, " is OTP for delivery of Order No (" + OrderId + ") Shopkirana", sRandomOTP);
                                                string message = ""; //"{#var1#} is Delivery Code for delivery of Order No. {#var2#} for Total Qty {#var3#} and Value of Rs. {#var4#}. Shopkirana";
                                                var dltSMS = SMSTemplateHelper.getTemplateText((int)AppEnum.RetailerApp, "Order_Delivery_Delivered");
                                                message = dltSMS == null ? "" : dltSMS.Template;

                                                message = message.Replace("{#var1#}", sRandomOTP);
                                                message = message.Replace("{#var2#}", OrderId.ToString());
                                                message = message.Replace("{#var3#}", orderMobiledetail.TotalQty.ToString());
                                                message = message.Replace("{#var4#}", orderMobiledetail.OrderAmount.ToString());
                                                if (dltSMS != null)
                                                    Common.Helpers.SendSMSHelper.SendSMS(orderMobiledetail.customermobile, message, ((Int32)Common.Enums.SMSRouteEnum.OTP).ToString(), dltSMS.DLTId);

                                            }
                                        }
                                        break;
                                    case "Delivery Redispatch":

                                        var ordertype = context.DbOrderMaster.Where(x => x.OrderId == OrderId).FirstOrDefault();
                                        if (ordertype.OrderType == 6 || ordertype.OrderType == 9)
                                        {

                                            sRandomOTP = "";
                                            string[] saAllowedCharacters = { "1", "2", "3", "4", "5", "6", "7", "8", "9", "0" };
                                            sRandomOTP = GenerateRandomOTP(4, saAllowedCharacters);

                                            List<int> OrderIDs = new List<int>();
                                            if (true)
                                            {
                                                var ExistsOTPs = context.OrderDeliveryOTP.Where(x => x.OrderId == OrderId && x.IsActive == true).OrderByDescending(x => x.CreatedDate).ToList();
                                                if (ExistsOTPs != null && ExistsOTPs.Any())
                                                {
                                                    foreach (var ExistsOTP in ExistsOTPs)
                                                    {
                                                        ExistsOTP.IsActive = false;
                                                        ExistsOTP.ModifiedDate = DateTime.Now;
                                                        ExistsOTP.ModifiedBy = userid;
                                                        context.Entry(ExistsOTP).State = EntityState.Modified;
                                                    }
                                                }

                                                if (!string.IsNullOrEmpty(sRandomOTP))
                                                {
                                                    OrderDeliveryOTP OrderDeliveryOTP = new OrderDeliveryOTP
                                                    {
                                                        CreatedBy = userid,
                                                        CreatedDate = DateTime.Now,
                                                        IsActive = true,
                                                        OrderId = OrderId,
                                                        OTP = sRandomOTP,
                                                        Status = "Delivery Redispatch",
                                                        UserType = "HQ Operation",
                                                        IsUsed = false,
                                                        UserId = 0,
                                                        lat = lat,
                                                        lg = lg,
                                                        IsVideoSeen = false,
                                                        VideoUrl = ""

                                                    };
                                                    context.OrderDeliveryOTP.Add(OrderDeliveryOTP);
                                                    context.Commit();
                                                    result = true;
                                                }
                                            }
                                        }
                                        else 
                                        {
                                            MongoData = mongoDb.GetAll();
                                            foreach (var data in MongoData)
                                            {
                                                if (CustOtp > 0)
                                                {
                                                    if (ODM.IsDigitalOrder == true && data.DepartmentName == "Sales")
                                                    {
                                                        data.DepartmentName = "Digital";
                                                    }
                                                    var orderDeliveryOTPs = context.OrderDeliveryOTP.Where(x => x.OrderId == OrderId && x.IsActive == true && x.UserType == data.DepartmentName).FirstOrDefault();
                                                    if (orderDeliveryOTPs != null)
                                                    {
                                                        orderDeliveryOTPs.ModifiedDate = DateTime.Now;
                                                        orderDeliveryOTPs.ModifiedBy = userid;
                                                        orderDeliveryOTPs.IsActive = false;
                                                        context.Entry(orderDeliveryOTPs).State = EntityState.Modified;
                                                    }

                                                    if (data.DepartmentName == "Sales" || data.DepartmentName == "Digital")
                                                    {

                                                        OrderDeliveryOTP OrderDeliveryOTP = new OrderDeliveryOTP
                                                        {
                                                            CreatedBy = userid,
                                                            CreatedDate = DateTime.Now,
                                                            IsActive = true,
                                                            OrderId = OrderId,
                                                            OTP = SalesOtp.ToString(),
                                                            Status = Status,
                                                            lat = lat,
                                                            lg = lg,
                                                            UserType = ODM.IsDigitalOrder == true ? "Digital" : data.DepartmentName,
                                                            UserId = orderMobiledetail.SalesId,
                                                            IsUsed = false,
                                                            VideoUrl = VideoUrl
                                                        };
                                                        context.OrderDeliveryOTP.Add(OrderDeliveryOTP);
                                                    }
                                                    else if (data.DepartmentName == "Customer")
                                                    {
                                                        OrderDeliveryOTP OrderDeliveryOTP = new OrderDeliveryOTP
                                                        {
                                                            CreatedBy = userid,
                                                            CreatedDate = DateTime.Now,
                                                            IsActive = true,
                                                            OrderId = OrderId,
                                                            OTP = CustOtp.ToString(),
                                                            Status = Status,
                                                            lat = lat,
                                                            lg = lg,
                                                            UserType = data.DepartmentName,
                                                            UserId = orderMobiledetail.customerid,
                                                            IsUsed = false,
                                                            VideoUrl = VideoUrl
                                                        };
                                                        context.OrderDeliveryOTP.Add(OrderDeliveryOTP);
                                                    }
                                                    else
                                                    {
                                                        OrderDeliveryOTP OrderDeliveryOTP = new OrderDeliveryOTP
                                                        {
                                                            CreatedBy = userid,
                                                            CreatedDate = DateTime.Now,
                                                            IsActive = true,
                                                            OrderId = OrderId,
                                                            OTP = random.Next(1000, 10000).ToString(),
                                                            Status = Status,
                                                            lat = lat,
                                                            lg = lg,
                                                            UserType = data.DepartmentName,
                                                            UserId = 0,
                                                            IsUsed = false,
                                                            VideoUrl = VideoUrl
                                                        };
                                                        context.OrderDeliveryOTP.Add(OrderDeliveryOTP);
                                                    }

                                                    result = context.Commit() > 0;
                                                }
                                                if (data.DepartmentName == "Customer")
                                                {

                                                    if (!string.IsNullOrEmpty(orderMobiledetail.customermobile))
                                                    {
                                                        if (orderMobiledetail.Deliverydate.Date == DateTime.Now.Date)
                                                        {
                                                            MongoDbHelper<DataContracts.Mongo.CustomerRedispatchCharges> mongoDbHelper = new MongoDbHelper<DataContracts.Mongo.CustomerRedispatchCharges>();
                                                            var customerRedispatchCharges = mongoDbHelper.Select(x => x.MinValue < orderMobiledetail.OrderAmount && x.MaxValue >= orderMobiledetail.OrderAmount && x.WarehouseId == orderMobiledetail.warehouseid).FirstOrDefault();
                                                            string amount = "";
                                                            if (customerRedispatchCharges != null && customerRedispatchCharges.RedispatchCharges > 0)
                                                            {
                                                                amount = customerRedispatchCharges.RedispatchCharges.ToString();
                                                            }

                                                            string message = "";
                                                            var dltSMS = SMSTemplateHelper.getTemplateText((int)AppEnum.RetailerApp, "Order_Delivery_Redispatch");
                                                            message = dltSMS == null ? "" : dltSMS.Template;

                                                            if (!string.IsNullOrEmpty(amount))
                                                            {
                                                                dltSMS = SMSTemplateHelper.getTemplateText((int)AppEnum.RetailerApp, "RedispatchCharges");
                                                                message = dltSMS == null ? "" : dltSMS.Template;
                                                            }

                                                            message = message.Replace("{#var1#}", CustOtp.ToString());
                                                            message = message.Replace("{#var2#}", OrderId.ToString());
                                                            message = message.Replace("{#var3#}", amount.ToString());
                                                            if (dltSMS != null)
                                                                Common.Helpers.SendSMSHelper.SendSMS(orderMobiledetail.customermobile, message, ((Int32)Common.Enums.SMSRouteEnum.OTP).ToString(), dltSMS.DLTId);
                                                        }
                                                        else
                                                        {
                                                            string message = "";
                                                            var dltSMS = SMSTemplateHelper.getTemplateText((int)AppEnum.RetailerApp, "Order_Delivery_Redispatch");
                                                            message = dltSMS == null ? "" : dltSMS.Template;

                                                            message = message.Replace("{#var1#}", CustOtp.ToString());
                                                            message = message.Replace("{#var2#}", OrderId.ToString());
                                                            //message = message.Replace("{#var3#}", amount.ToString());
                                                            if (dltSMS != null)
                                                                Common.Helpers.SendSMSHelper.SendSMS(orderMobiledetail.customermobile, message, ((Int32)Common.Enums.SMSRouteEnum.OTP).ToString(), dltSMS.DLTId);

                                                        }
                                                    }
                                                }

                                            }
                                            sent = SendNotification(orderMobiledetail, ODM, Status, SalesOtp, IsShowOtp: (string.IsNullOrEmpty(VideoUrl) ? true : false));
                                        }

                                       
                                        break;

                                    case "Delivery Canceled":

                                        ordertype = context.DbOrderMaster.Where(x => x.OrderId == OrderId).FirstOrDefault();
                                        if (ordertype.OrderType == 6 || ordertype.OrderType == 9)
                                        {

                                            sRandomOTP = "";
                                            string[] saAllowedCharacters = { "1", "2", "3", "4", "5", "6", "7", "8", "9", "0" };
                                            sRandomOTP = GenerateRandomOTP(4, saAllowedCharacters);

                                            List<int> OrderIDs = new List<int>();
                                            if (true)
                                            {
                                                var ExistsOTPs = context.OrderDeliveryOTP.Where(x => x.OrderId == OrderId && x.IsActive == true).OrderByDescending(x => x.CreatedDate).ToList();
                                                if (ExistsOTPs != null && ExistsOTPs.Any())
                                                {
                                                    foreach (var ExistsOTP in ExistsOTPs)
                                                    {
                                                        ExistsOTP.IsActive = false;
                                                        ExistsOTP.ModifiedDate = DateTime.Now;
                                                        ExistsOTP.ModifiedBy = userid;
                                                        context.Entry(ExistsOTP).State = EntityState.Modified;
                                                    }
                                                }

                                                if (!string.IsNullOrEmpty(sRandomOTP))
                                                {
                                                    OrderDeliveryOTP OrderDeliveryOTP = new OrderDeliveryOTP
                                                    {
                                                        CreatedBy = userid,
                                                        CreatedDate = DateTime.Now,
                                                        IsActive = true,
                                                        OrderId = OrderId,
                                                        OTP = sRandomOTP,
                                                        Status = "Delivery Canceled",
                                                        UserType = "HQ Operation",
                                                        IsUsed = false,
                                                        UserId = 0,
                                                        lat = lat,
                                                        lg = lg,
                                                        IsVideoSeen = false,
                                                        VideoUrl = ""

                                                    };
                                                    context.OrderDeliveryOTP.Add(OrderDeliveryOTP);
                                                    context.Commit();
                                                    result = true;
                                                }
                                            }
                                        }
                                        else 
                                        {
                                            MongoData = mongoDb.GetAll();
                                            foreach (var data in MongoData)
                                            {
                                                if (CustOtp > 0)
                                                {
                                                    if (ODM.IsDigitalOrder == true && data.DepartmentName == "Sales")
                                                    {
                                                        data.DepartmentName = "Digital";
                                                    }
                                                    var orderDeliveryOTPs = context.OrderDeliveryOTP.Where(x => x.OrderId == OrderId && x.IsActive == true && x.UserType == data.DepartmentName).FirstOrDefault();
                                                    if (orderDeliveryOTPs != null)
                                                    {
                                                        orderDeliveryOTPs.ModifiedDate = DateTime.Now;
                                                        orderDeliveryOTPs.ModifiedBy = userid;
                                                        orderDeliveryOTPs.IsActive = false;
                                                        context.Entry(orderDeliveryOTPs).State = EntityState.Modified;
                                                    }

                                                    if (data.DepartmentName == "Sales" || data.DepartmentName == "Digital")
                                                    {

                                                        OrderDeliveryOTP OrderDeliveryOTP = new OrderDeliveryOTP
                                                        {
                                                            CreatedBy = userid,
                                                            CreatedDate = DateTime.Now,
                                                            IsActive = true,
                                                            OrderId = OrderId,
                                                            OTP = SalesOtp.ToString(),
                                                            Status = Status,
                                                            lat = lat,
                                                            lg = lg,
                                                            UserType = ODM.IsDigitalOrder == true ? "Digital" : data.DepartmentName,
                                                            UserId = orderMobiledetail.SalesId,
                                                            IsUsed = false,
                                                            VideoUrl = VideoUrl
                                                        };
                                                        context.OrderDeliveryOTP.Add(OrderDeliveryOTP);
                                                    }
                                                    else if (data.DepartmentName == "Customer")
                                                    {

                                                        OrderDeliveryOTP OrderDeliveryOTP = new OrderDeliveryOTP
                                                        {
                                                            CreatedBy = userid,
                                                            CreatedDate = DateTime.Now,
                                                            IsActive = true,
                                                            OrderId = OrderId,
                                                            OTP = CustOtp.ToString(),
                                                            Status = Status,
                                                            lat = lat,
                                                            lg = lg,
                                                            UserType = data.DepartmentName,
                                                            UserId = orderMobiledetail.customerid,
                                                            IsUsed = false,
                                                            VideoUrl = VideoUrl
                                                        };
                                                        context.OrderDeliveryOTP.Add(OrderDeliveryOTP);
                                                    }
                                                    else
                                                    {
                                                        OrderDeliveryOTP OrderDeliveryOTP = new OrderDeliveryOTP
                                                        {
                                                            CreatedBy = userid,
                                                            CreatedDate = DateTime.Now,
                                                            IsActive = true,
                                                            OrderId = OrderId,
                                                            OTP = random.Next(1000, 10000).ToString(),
                                                            Status = Status,
                                                            lat = lat,
                                                            lg = lg,
                                                            UserType = data.DepartmentName,
                                                            UserId = 0,
                                                            IsUsed = false,
                                                            VideoUrl = VideoUrl
                                                        };
                                                        context.OrderDeliveryOTP.Add(OrderDeliveryOTP);
                                                    }

                                                    result = context.Commit() > 0;
                                                }

                                                if (data.DepartmentName == "Customer")
                                                {
                                                    if (!string.IsNullOrEmpty(orderMobiledetail.customermobile))
                                                    {
                                                        string Message = ""; //" is OTP for delivery canceled of Order No {#var2#} . ShopKirana";
                                                        var dltSMS = SMSTemplateHelper.getTemplateText((int)AppEnum.RetailerApp, "Order_Delivery_Cancellation");
                                                        Message = dltSMS == null ? "" : dltSMS.Template;
                                                        Message = Message.Replace("{#var1#}", CustOtp.ToString());
                                                        Message = Message.Replace("{#var2#}", OrderId.ToString());
                                                        if (dltSMS != null)
                                                            Common.Helpers.SendSMSHelper.SendSMS(orderMobiledetail.customermobile, Message, ((Int32)Common.Enums.SMSRouteEnum.OTP).ToString(), dltSMS.DLTId);

                                                    }
                                                }
                                            }
                                            sent = SendNotification(orderMobiledetail, ODM, Status, SalesOtp, IsShowOtp: (string.IsNullOrEmpty(VideoUrl) ? true : false));
                                        }
                                        
                                        break;
                                }

                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error("Error in GenerateOTPForOrder Method: " + ex.Message);
                result = false;
            }
            return result;
        }

        private bool SendNotification(OrderMobiledetail orderMobiledetail, OrderMaster ODM, string Status, int SalesOtp, bool IsShowOtp = true)
        {
            bool sent = false;
            var Notification = "";
            string Body = "";
            if (!string.IsNullOrEmpty(orderMobiledetail.salespersonfcmid) && ODM != null && SalesOtp > 0)
            {
                if (IsShowOtp == true)
                {
                    Body = "OrderID: " + ODM.OrderId + "  |  " + "Order Amount:" + orderMobiledetail.TotalAmt + "@" + "SKCode: " + ODM.Skcode + "  |  " + "ShopName: " + ODM.ShopName + "@" + "OTP: " + SalesOtp;
                }
                else
                {
                    Body = "OrderID: " + ODM.OrderId + "  |  " + "Order Amount:" + orderMobiledetail.TotalAmt + "@" + "SKCode: " + ODM.Skcode + "  |  " + "ShopName: " + ODM.ShopName;
                }
                Body = Body.Replace("@", System.Environment.NewLine);
                // string DeliveryCancelApproveReqBody = "OrderID: " +ODM.OrderId + "<br>" + "Order Amount" + ODM.OrderAmount + "<br>" + "SKCode: " +ODM.Skcode +"<br>" +"ShopName: " + ODM.ShopName +"<br>" ;

                string Key1 = ConfigurationManager.AppSettings["SalesFcmApiKey"];
                //var fcmdcs = new FcmidDCss();
                //fcmdcs.body = (Status == "Delivery Redispatch" || Status == "Delivery Canceled") ? Body : "";
                //fcmdcs.title = Status == "Delivery Redispatch" ? "Delivery Redispatch OTP" : Status == "Delivery Canceled" ? "Delivery Cancel OTP" : "";
                //fcmdcs.notify_type = "OTP_SMS";
                //var fcmdc = new FcmidDC();
                //fcmdc.to = orderMobiledetail.salespersonfcmid;
                //fcmdc.data = fcmdcs;

                var data = new FCMData
                {
                    title = Status == "Delivery Redispatch" ? "Delivery Redispatch OTP" : Status == "Delivery Canceled" ? "Delivery Cancel OTP" : "",
                    body = (Status == "Delivery Redispatch" || Status == "Delivery Canceled") ? Body : "",
                    icon = "",
                    typeId = 0,
                    notificationCategory = "",
                    notificationType = "",
                    notificationId = 0,
                    notify_type = "OTP_SMS",
                    // OrderId = OrderId,
                    url = "", //OrderId, PeopleId
                              // OrderStatus = OrderStatus
                };
                var firebaseService = new FirebaseNotificationServiceHelper(Key1);
                //var fcmid = "fZGIeP5dTKGd-2JBZttcDj:APA91bGINtqXqKuCcEHd4qXZMN-VtX5KC2g98KkytmGdpc28_-duDu8Ry1P6Kk_Xb9RgRG0iDWrp8DkwE1EPCOPG0OLz2uRjo-HBg-Ysg-mDaMErlYLjJGZE7ScXKIkjmWZ0xNO6UyxN";
                var result = firebaseService.SendNotificationForApprovalAsync(orderMobiledetail.salespersonfcmid, data);
                if (result != null)
                {
                    //fcmdc.MessageId = response.results.FirstOrDefault().message_id;
                    Notification = "Notification sent successfully";
                    sent = true;
                }
                else
                {
                    sent = false;
                    Notification = "Notification Not sent";
                }
                //logger.Info("OTP Notification: " + fcmdcs.title + " : " + Body);
                //WebRequest tRequest = WebRequest.Create("https://fcm.googleapis.com/fcm/send") as HttpWebRequest;
                //tRequest.Method = "POST";
                //string jsonNotificationFormat = Newtonsoft.Json.JsonConvert.SerializeObject(fcmdc);
                //Byte[] byteArray = Encoding.UTF8.GetBytes(jsonNotificationFormat);
                //tRequest.Headers.Add(string.Format("Authorization: key={0}", Key1));
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
                //                    fcmdc.MessageId = response.results.FirstOrDefault().message_id;
                //                    Notification = "Notification sent successfully";
                //                    sent = true;
                //                }
                //                else if (response.failure == 1)
                //                {
                //                    sent = false;
                //                    Notification = "Notification Not sent";
                //                }
                //            }
                //        }
                //    }
                //}
            }

            return sent;
        }

        [HttpGet]
        [Authorize]
        [Route("ValidateOTPForOrder")]
        public async Task<ResponseMsg> ValidateOTPForOrder(string otp, int OrderId, string Status)
        {
            ResponseMsg result = new ResponseMsg();
            var identity = User.Identity as ClaimsIdentity;
            int userid = 0;
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

            using (AuthContext context = new AuthContext())
            {
                var orderId = new SqlParameter("@OrderId", OrderId);
                var status = new SqlParameter("@Status", Status);
                var Otp = new SqlParameter("@OTP", otp);

                var Data = context.Database.SqlQuery<ValidateOTPofOrder>("exec ValidateOTPofOrder @OrderId,@Status,@OTP", orderId, status, Otp).FirstOrDefault();
                if (Data != null)
                {
                    if (!Data.IsActive)
                    {
                        var Minutes = DateTime.Now - Data.CreatedDate;
                        if (Minutes.TotalMinutes > 30)
                        {
                            result.Message = "OTP has been expied,Please Re-generate.";
                            result.Status = false;
                            return result;
                        }
                        else
                        {
                            result.Message = "Incorrect OTP";
                            result.Status = false;
                            return result;
                        }
                    }

                    if (Data.UserType == "Customer")
                    {
                        var Customer = context.Customers.Where(x => x.CustomerId == (Data.UserId > 0 ? Data.UserId : 0) && x.Active == true).FirstOrDefault();
                        if (Customer == null)
                        {
                            result.Message = "Customer is Inactive, Use another OTP";
                            result.Status = false;
                            return result;
                        }
                        else
                        {
                            result.Message = "Success";
                            result.Status = true;
                            return result;
                        }


                    }
                    else if (Data.UserType == "Sales" || Data.UserType == "Digital")
                    {
                        var Sales = context.Peoples.Where(x => x.PeopleID == (Data.UserId > 0 ? Data.UserId : 0) && x.Active == true).FirstOrDefault();
                        if (Sales == null)
                        {
                            result.Message = "Sales Executive is Inactive, Use another OTP";
                            result.Status = false;
                            return result;
                        }
                        else
                        {
                            result.Message = "Success";
                            result.Status = true;
                            return result;
                        }

                    }
                    else if (Data.UserType == "HQ Customer Delight" || Data.UserType == "HQ Operation" || Data.UserType == "HQ Operation(ReAttempt)")
                    {
                        result.Message = "Success";
                        result.Status = true;
                        return result;
                    }
                    //else if (Data.UserType == "Sales(Reattempt)")
                    //{
                    //    var Sales = context.Peoples.Where(x => x.PeopleID == (Data.UserId > 0 ? Data.UserId : 0) && x.Active == true).FirstOrDefault();
                    //    if (Sales == null)
                    //    {
                    //        result.Message = "Sales Executive is Inactive";
                    //        result.Status = false;
                    //        return result;
                    //    }
                    //    else
                    //    {
                    //        result.Message = "Success";
                    //        result.Status = true;
                    //        return result;
                    //    }

                    //}
                    else if (Data.Status == "Delivered")
                    {
                        result.Message = "Success";
                        result.Status = true;
                        return result;
                    }
                }
                else
                {
                    result.Message = "Incorrect OTP";
                    result.Status = false;
                    return result;
                }
            }
            return result;
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("OTPExistsForOrder")]
        public bool OTPExistsForOrder(int OrderId, string Status, string comments)
        {
            bool result = false;
            using (AuthContext context = new AuthContext())
            {

                var countOrder = context.OrderDeliveryOTP.Where(x => x.OrderId == OrderId && x.Status == Status && x.IsActive).Count();
                if (countOrder > 0)
                {
                    if (comments == "Dont Want" || comments == "Price Issue (sales)")
                    {

                        string query = "select a.warehouseid,c.Mobile as customermobile,c.fcmId as CustomerFMCID,c.CustomerId,a.OrderId from OrderMasters a inner join Customers c on a.CustomerId = c.CustomerId and a.orderid =" + OrderId;
                        var orderMobiledetail = context.Database.SqlQuery<OrderMobiledetail>(query).FirstOrDefault();

                        // var msg = "You will be Charged 100 wallet point of Delivery Cancellation of Order No(" + OrderId + ")";
                        var msg = "";//"You will be Charged {#var1#} wallet point of Delivery Cancellation of Order No {#var2#}";
                        var dltSMS = SMSTemplateHelper.getTemplateText((int)AppEnum.RetailerApp, "Wallet_Fine_Charged_On_Cancellation_Order");
                        msg = dltSMS == null ? "" : dltSMS.Template;

                        msg = msg.Replace("{#var1#}", "100");
                        msg = msg.Replace("{#var2#}", OrderId.ToString());
                        if (dltSMS != null)
                            Common.Helpers.SendSMSHelper.SendSMS(orderMobiledetail.customermobile, msg, ((Int32)Common.Enums.SMSRouteEnum.Transactional).ToString(), dltSMS.DLTId);
                        //Getotp(orderMobiledetail.customermobile, " Delivery canceled of Order No (" + OrderId + ")", msg);
                    }
                    result = true;
                }
            }
            return result;
        }

        //public OTP Getotp(string MobileNumber, string msg, string sRandomOTP, string DLTId)
        //{
        //    logger.Info("start Gen OTP: ");
        //    try
        //    {

        //        string OtpMessage = msg;
        //        string message = sRandomOTP + " :" + OtpMessage;
        //        //string CountryCode = "91";
        //        //// string Sender = "SHOPKR";
        //        //string Sender = ConfigurationManager.AppSettings["NewriseOTPSenderId"].ToString();
        //        //string username = ConfigurationManager.AppSettings["NewriseOTPUsername"].ToString();
        //        //string passwrod = ConfigurationManager.AppSettings["NewriseOTPPasswrod"].ToString();
        //        //string authkey = Startup.smsauthKey;
        //        //int route = 4;
        //        //string path = "http://www.smsjust.com/blank/sms/user/urlsms.php?username=" + username + "&pass=" + passwrod + "&senderid=" + Sender + "&dest_mobileno=" + MobileNumber + "&message=" + sRandomOTP + " :" + OtpMessage + " &response=Y";

        //        ////  string path1 = "http://bulksms.newrise.in/api/sendhttp.php?authkey=" + authkey + "&mobiles=" + MobileNumber + "&message=" + sRandomOTP + " :" + OtpMessage + " &sender=" + Sender + "&route=" + route + "&country=" + CountryCode;

        //        ////string path ="http://bulksms.newrise.in/api/sendhttp.php?authkey=100498AhbWDYbtJT56af33e3&mobiles=9770838685&message= SK OTP is : " + sRandomOTP + " &sender=SHOPKR&route=4&country=91";

        //        //var webRequest = (HttpWebRequest)WebRequest.Create(path);
        //        //webRequest.Method = "GET";
        //        //webRequest.ContentType = "application/json";
        //        //webRequest.UserAgent = "Mozilla/5.0 (Windows NT 5.1; rv:28.0) Gecko/20100101 Firefox/28.0";
        //        //webRequest.ContentLength = 0; // added per comment 
        //        //webRequest.Credentials = CredentialCache.DefaultCredentials;
        //        //webRequest.Accept = "*/*";
        //        //var webResponse = (HttpWebResponse)webRequest.GetResponse();
        //        //if (webResponse.StatusCode != HttpStatusCode.OK) Console.WriteLine("{0}", webResponse.Headers);
        //        //logger.Info("OTP Genrated: " + sRandomOTP);
        //        Common.Helpers.SendSMSHelper.SendSMS(MobileNumber, message, ((Int32)Common.Enums.SMSRouteEnum.OTP).ToString(), DLTId);
        //        OTP a = new OTP()
        //        {
        //            OtpNo = sRandomOTP
        //        };
        //        return a;
        //    }
        //    catch (Exception ex)
        //    {
        //        logger.Error("Error in OTP Genration for Order.");
        //        return null;
        //    }
        //}


        //pz
        //[AllowAnonymous]
        //[HttpPost]
        //[Route("mail")]
        //public static string getHtml(int OrderId)
        //{
        //    try
        //    {
        //        string masteremail = ConfigurationManager.AppSettings["MasterEmail"];
        //        string masterpassword = ConfigurationManager.AppSettings["MasterPassword"];

        //        string messageBody = "<font>The following are the records: </font><br><br>";
        //        if (OrderId == null) return messageBody;
        //        using (AuthContext context = new AuthContext())
        //        {
        //            var dept = context.DeptOrderCancellationDb.Where(x => x.OrderId == OrderId).ToList();

        //            string htmlTableStart = "<table style=\"border-collapse:collapse; text-align:center;\" >";
        //            string htmlTableEnd = "</table>";
        //            string htmlHeaderRowStart = "<tr style=\"background-color:#6FA1D2; color:#ffffff;\">";
        //            string htmlHeaderRowEnd = "</tr>";
        //            string htmlTrStart = "<tr style=\"color:#555555;\">";
        //            string htmlTrEnd = "</tr>";
        //            string htmlTdStart = "<td style=\" border-color:#5c87b2; border-style:solid; border-width:thin; padding: 5px;\">";
        //            string htmlTdEnd = "</td>";
        //            messageBody += htmlTableStart;
        //            messageBody += htmlHeaderRowStart;
        //            messageBody += htmlTdStart + "Sales Lead Id" + htmlTdEnd;
        //            messageBody += htmlTdStart + "DepId" + htmlTdEnd;
        //            messageBody += htmlTdStart + "OrderId" + htmlTdEnd;
        //            messageBody += htmlTdStart + "Charge Point" + htmlTdEnd;
        //            messageBody += htmlHeaderRowEnd;
        //            //Loop all the rows from grid vew and added to html td  
        //            for (int i = 0; i <= dept.Count - 1; i++)
        //            {
        //                messageBody = messageBody + htmlTrStart;
        //                messageBody = messageBody + htmlTdStart + dept[i].SalesLeadId + htmlTdEnd; //adding student name  
        //                messageBody = messageBody + htmlTdStart + dept[i].DepId + htmlTdEnd; //adding DOB  
        //                messageBody = messageBody + htmlTdStart + dept[i].OrderId + htmlTdEnd; //adding Email  
        //                messageBody = messageBody + htmlTdStart + dept[i].ChargePoint + htmlTdEnd; //adding Mobile  
        //                messageBody = messageBody + htmlTrEnd;
        //            }
        //            var Subj = "Alert! Freezed Assignment copy : " + OrderId;
        //            var msg = new MailMessage("zhambarepratibha15@shopkirana@shopkirana.com", "zambare.it@shopkirana@shopkirana.com", Subj, messageBody);
        //            System.Net.Mail.Attachment attachment;
        //            attachment = new System.Net.Mail.Attachment(Startup.FreezedAsignmentCopyFilePath);
        //            msg.Attachments.Add(attachment);
        //            // msg.To.Add("deepak@shopkirana.com");
        //            /// msg.To.Add("manasi@shopkirana.com");
        //            //if (Wemail != "no")
        //            // { msg.To.Add(Wemail); }
        //            msg.IsBodyHtml = true;
        //            var smtpClient = new SmtpClient("smtp.gmail.com", 587); //if your from email address is "from@hotmail.com" then host should be "smtp.hotmail.com"
        //            smtpClient.UseDefaultCredentials = true;
        //            smtpClient.Credentials = new NetworkCredential(masteremail, masterpassword);
        //            smtpClient.EnableSsl = true;
        //            smtpClient.Send(msg);


        //            messageBody = messageBody + htmlTableEnd;
        //            return messageBody; // return HTML Table as string from this function  
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        return null;
        //    }
        //}

        //pz

        /// <summary>
        /// Created by 18/12/2018 
        /// Create rendom otp
        /// </summary>
        /// <param name="iOTPLength"></param>
        /// <param name="saAllowedCharacters"></param>
        /// <returns></returns>
        private string GenerateRandomOTP(int iOTPLength, string[] saAllowedCharacters)
        {
            string sOTP = String.Empty;
            string sTempChars = String.Empty;
            Random rand = new Random();

            for (int i = 0; i < iOTPLength; i++)
            {
                int p = rand.Next(0, saAllowedCharacters.Length);
                sTempChars = saAllowedCharacters[rand.Next(0, saAllowedCharacters.Length)];
                sOTP += sTempChars;
            }
            return sOTP;
        }

        [HttpGet]
        [Route("GetOrderDeliveryDetail")]
        public OrderDeliveryDetail GetOrderDeliveryDetail(int OrderId)
        {
            using (AuthContext context = new AuthContext())
            {
                string query = "select orderid,GrossAmount,Deliverydate OrderedDate from OrderDispatchedMasters where OrderId=" + OrderId;
                var OrderDeliveryDetail = context.Database.SqlQuery<OrderDeliveryDetail>(query).FirstOrDefault();

                return OrderDeliveryDetail;
            }
        }
        #endregion


        #region Order Delivered Notification
        public async Task<bool> DeliveredNotification(int CustomerId, double? RewardPoint, int OrderId, AuthContext authContext)
        {
            bool Result = false;
            Notification notification = new Notification();
            notification.title = "बधाई हो ! ";
            notification.Message = "बधाई हो ! आर्डर OrderId:" + OrderId + " डिलीवरी पर आपके वॉलेट में आये " + RewardPoint + " पॉइंट";
            notification.Pic = "https://cdn4.iconfinder.com/data/icons/ionicons/512/icon-image-128.png";

            var customers = authContext.Customers.Where(x => x.fcmId != null && x.CustomerId == CustomerId).SingleOrDefault();
            //AddNotification(notification);
            string Key = ConfigurationManager.AppSettings["FcmApiKey"];
            //string id11 = ConfigurationManager.AppSettings["FcmApiId"];
            //WebRequest tRequest = WebRequest.Create("https://fcm.googleapis.com/fcm/send") as HttpWebRequest;
            //tRequest.Method = "post";


            //var objNotification = new
            //{
            //    to = customers.fcmId,
            //    notification = new
            //    {
            //        title = notification.title,
            //        body = notification.Message,
            //        icon = notification.Pic
            //    }
            //};

            //string jsonNotificationFormat = Newtonsoft.Json.JsonConvert.SerializeObject(objNotification);
            //Byte[] byteArray = Encoding.UTF8.GetBytes(jsonNotificationFormat);
            //tRequest.Headers.Add(string.Format("Authorization: key={0}", Key));
            //tRequest.Headers.Add(string.Format("Sender: id={0}", id11));
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
            //                FCMResponse response = Newtonsoft.Json.JsonConvert.DeserializeObject<FCMResponse>(responseFromFirebaseServer);
            //                if (response.success == 1)
            //                {
            //                    Console.Write(response);
            //                }
            //                else if (response.failure == 1)
            //                {
            //                    Console.Write(response);
            //                }
            //            }
            //        }
            //    }
            //}

            var data = new FCMData
            {
                title = notification.title,
                body = notification.Message,
                icon = notification.Pic
            };
            var firebaseService = new FirebaseNotificationServiceHelper(Key);
            var result = await firebaseService.SendNotificationForApprovalAsync(customers.fcmId, data);
            if (result != null)
            {
                Result = true;
            }
            else
            {
                Result = false;
            }
            return Result;
        }
        public async Task<bool> DeliveredNotificationWithoutAuth(int CustomerId, double? RewardPoint, int OrderId, Customer customers)
        {
            bool Result = false;
            Notification notification = new Notification();
            notification.title = "बधाई हो ! ";
            notification.Message = "बधाई हो ! आर्डर OrderId:" + OrderId + " डिलीवरी पर आपके वॉलेट में आये " + RewardPoint + " पॉइंट";
            notification.Pic = "https://cdn4.iconfinder.com/data/icons/ionicons/512/icon-image-128.png";

            //var customers = authContext.Customers.Where(x => x.fcmId != null && x.CustomerId == CustomerId).SingleOrDefault();
            //AddNotification(notification);
            string Key = ConfigurationManager.AppSettings["FcmApiKey"];
            //string id11 = ConfigurationManager.AppSettings["FcmApiId"];
            //WebRequest tRequest = WebRequest.Create("https://fcm.googleapis.com/fcm/send") as HttpWebRequest;
            //tRequest.Method = "post";


            //var objNotification = new
            //{
            //    to = customers.fcmId,
            //    notification = new
            //    {
            //        title = notification.title,
            //        body = notification.Message,
            //        icon = notification.Pic
            //    }
            //};

            //string jsonNotificationFormat = Newtonsoft.Json.JsonConvert.SerializeObject(objNotification);
            //Byte[] byteArray = Encoding.UTF8.GetBytes(jsonNotificationFormat);
            //tRequest.Headers.Add(string.Format("Authorization: key={0}", Key));
            //tRequest.Headers.Add(string.Format("Sender: id={0}", id11));
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
            //                FCMResponse response = Newtonsoft.Json.JsonConvert.DeserializeObject<FCMResponse>(responseFromFirebaseServer);
            //                if (response.success == 1)
            //                {
            //                    Console.Write(response);
            //                }
            //                else if (response.failure == 1)
            //                {
            //                    Console.Write(response);
            //                }
            //            }
            //        }
            //    }
            //}
            var data = new FCMData
            {
                title = notification.title,
                body = notification.Message,
                icon = notification.Pic
            };
            var firebaseService = new FirebaseNotificationServiceHelper(Key);
            var result = await firebaseService.SendNotificationForApprovalAsync(customers.fcmId, data);
            if (result != null)
            {
                Result = true;
            }
            else
            {
                Result = false;
            }
            return Result;
        }
        #endregion

        [AllowAnonymous]
        [Route("getreportdeliverycancellation")]
        [HttpGet]
        public dynamic getreportdeliverycancellation()
        {
            using (var context = new AuthContext())
            {
                var List = context.DeptOrderCancellationDb.Where(x => x.IsActive == true && x.IsDeleted == false).ToList();

                return List;
            }
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("SearchCancellationReport")]
        public dynamic SearchCancellationReport(int? DepId)
        {
            using (AuthContext context = new AuthContext())
            {
                var dept = context.DeptOrderCancellationDb.Where(x => x.IsDeleted == false).ToList();
                if (DepId == null)
                {
                    var List = context.DeptOrderCancellationDb.Where(x => x.IsActive == true && x.IsDeleted == false).ToList();

                    return List;
                }
                if (DepId != 0)
                {
                    var depts = context.DeptOrderCancellationDb.Where(x => x.DepId == DepId && x.IsActive == true && x.IsDeleted == false).ToList();
                    return depts;
                }


                return dept;
            }
        }


        [Route("VerifyDeliveryCashAmount")]
        [HttpGet]
        [AllowAnonymous]
        public double VerifyDeliveryCashAmount(int orderId)
        {
            using (var context = new AuthContext())
            {
                double sum = 0;
                var List = context.PaymentResponseRetailerAppDb.Where(x => x.OrderId == orderId && x.status == "Success" && x.PaymentFrom == "Cash").ToList();
                if (List != null && List.Any())
                {
                    sum = List.Sum(x => x.amount);
                }

                return sum;
            }
        }


        [AllowAnonymous]
        [Route("RemoveOrderIdFromStatic")]
        [HttpGet]
        public bool RemoveOrderIdInStatic(int Orderid)
        {
            if (Orderid > 0 && ordersToProcess.Any(x => x == Orderid))
            {
                ordersToProcess.RemoveAll(x => x == Orderid);
                return true;
            }
            else
            {
                var DC = new DeliveryCancelledDraftController();
                bool IsRemoved = DC.RemovedFromPoc(Orderid);
                return IsRemoved;
            }
        }

        [Route("GenerateRazorpayQrCode")]
        [HttpGet]
        public async Task<string> GenerateRazorpayQrCode(int orderId, int customerId, int cashAmount)
        {
            ReadyToDispatchHelper helper = new ReadyToDispatchHelper();
            var qrCode = helper.GenerateRazorpayQrCode(orderId, customerId, cashAmount);
            return qrCode;
        }

        [Route("AssignmentDirection")]
        [HttpGet]
        public async Task<AssignmentDirectionDc> GetAssignmentDirection(int assignmentId)
        {
            MongoDbHelper<AssignmentDirection> mongoDbHelper = new MongoDbHelper<AssignmentDirection>();
            MongoDbHelper<OrderUnloadingDuration> mongoDbHelper2 = new MongoDbHelper<OrderUnloadingDuration>();
            var predicate2 = PredicateBuilder.New<OrderUnloadingDuration>(x => x.UnloadingDuration > 0);
            var orderUnloadingDurations = mongoDbHelper2.Select(predicate2).ToList();

            var predicate = PredicateBuilder.New<AssignmentDirection>(x => x.AssignmentId == assignmentId);
            var assignmentDirection = mongoDbHelper.Select(predicate).FirstOrDefault();
            if (assignmentDirection == null)
            {
                using (var context = new AuthContext())
                {
                    var AssignmentCustomers = context.Database.SqlQuery<AssignmentCustomer>("Exec GetAssignmentCustomerLatLg " + assignmentId).ToList();
                    if (AssignmentCustomers != null && AssignmentCustomers.Any())
                    {
                        double TotalUnloadingDuration = 0;
                        if (orderUnloadingDurations != null && orderUnloadingDurations.Any())
                        {
                            foreach (var item in AssignmentCustomers)
                            {
                                TotalUnloadingDuration += orderUnloadingDurations.Where(x => x.OrderMinAmount <= item.TotalOrderAmount && x.OrderMaxAmount >= item.TotalOrderAmount).FirstOrDefault().UnloadingDuration;
                            }
                        }
                        if (AssignmentCustomers.FirstOrDefault().wlat.HasValue && AssignmentCustomers.FirstOrDefault().wlg.HasValue && AssignmentCustomers.FirstOrDefault().wlat.Value > 0 && AssignmentCustomers.FirstOrDefault().wlg.Value > 0)
                        {
                            string origin = AssignmentCustomers.FirstOrDefault().wlat.Value + "," + AssignmentCustomers.FirstOrDefault().wlg.Value;
                            string wayPoints = string.Join("|", AssignmentCustomers.Select(x => x.lat.HasValue && x.lg.HasValue && x.lat.Value > 0 && x.lg.Value > 0 ? x.lat.Value + "," + x.lg.Value : x.ShippingAddress).ToList());
                            string direction = GeoHelper.GetDirection(origin, origin, wayPoints);
                            if (!string.IsNullOrEmpty(direction))
                            {

                                var root = JsonConvert.DeserializeObject<Root>(direction);
                                double AssignmentDistance = 0, ReturnDistance = 0, AssignmentDuration = 0, ReturnDuration = 0;
                                if (root != null && root.routes.Any())
                                {
                                    var lastdirection = root.routes.Select(x => x.legs.LastOrDefault()).FirstOrDefault();
                                    AssignmentDistance = root.routes.Sum(x => x.legs.Where(y => y.end_location.lat != lastdirection.end_location.lat && y.end_location.lng != lastdirection.end_location.lng).Sum(z => z.distance.value));
                                    ReturnDistance = root.routes.Sum(x => x.legs.Where(y => y.end_location.lat == lastdirection.end_location.lat && y.end_location.lng == lastdirection.end_location.lng).Sum(z => z.distance.value));
                                    AssignmentDuration = root.routes.Sum(x => x.legs.Where(y => y.end_location.lat != lastdirection.end_location.lat && y.end_location.lng != lastdirection.end_location.lng).Sum(z => z.duration.value));
                                    ReturnDuration = root.routes.Sum(x => x.legs.Where(y => y.end_location.lat == lastdirection.end_location.lat && y.end_location.lng == lastdirection.end_location.lng).Sum(z => z.duration.value));
                                }

                                assignmentDirection = new AssignmentDirection
                                {
                                    AssignmentDirectionPath = direction,
                                    AssignmentId = assignmentId,
                                    CreatedDate = DateTime.Now,
                                    AssignmentDistance = AssignmentDistance,
                                    ReturnDistance = ReturnDistance,
                                    AssignmentDuration = AssignmentDuration,
                                    ReturnDuration = ReturnDuration,
                                    TotalUnloadingDuration = TotalUnloadingDuration
                                };
                                mongoDbHelper.Insert(assignmentDirection);
                            }
                        }
                    }
                }
            }
            var assignmentDirectionDc = Mapper.Map(assignmentDirection).ToANew<AssignmentDirectionDc>();
            if (assignmentDirectionDc != null && !string.IsNullOrEmpty(assignmentDirectionDc.AssignmentDirectionPath))
            {
                assignmentDirectionDc.Root = JsonConvert.DeserializeObject<Root>(assignmentDirectionDc.AssignmentDirectionPath);
            }
            return assignmentDirectionDc;
        }

        [Route("InsertOrderUnloadDuration")]
        [HttpGet]
        public bool InsertOrderUnloadDuration()
        {
            MongoDbHelper<OrderUnloadingDuration> mongoDbHelper = new MongoDbHelper<OrderUnloadingDuration>();
            var orderUnloadingDuration = new OrderUnloadingDuration
            {
                OrderMaxAmount = 10000,
                OrderMinAmount = 0,
                UnloadingDuration = 600
            };
            mongoDbHelper.Insert(orderUnloadingDuration);
            orderUnloadingDuration = new OrderUnloadingDuration
            {
                OrderMaxAmount = 30000,
                OrderMinAmount = 10001,
                UnloadingDuration = 800
            };
            mongoDbHelper.Insert(orderUnloadingDuration);
            orderUnloadingDuration = new OrderUnloadingDuration
            {
                OrderMaxAmount = 50000,
                OrderMinAmount = 30001,
                UnloadingDuration = 1000
            };
            mongoDbHelper.Insert(orderUnloadingDuration);
            orderUnloadingDuration = new OrderUnloadingDuration
            {
                OrderMaxAmount = 75000,
                OrderMinAmount = 50001,
                UnloadingDuration = 1200
            };
            mongoDbHelper.Insert(orderUnloadingDuration);
            orderUnloadingDuration = new OrderUnloadingDuration
            {
                OrderMaxAmount = 100000,
                OrderMinAmount = 75001,
                UnloadingDuration = 1500
            };
            mongoDbHelper.Insert(orderUnloadingDuration);
            orderUnloadingDuration = new OrderUnloadingDuration
            {
                OrderMaxAmount = 1000000,
                OrderMinAmount = 100000,
                UnloadingDuration = 2000
            };
            mongoDbHelper.Insert(orderUnloadingDuration);
            return true;
        }

        [Route("GetAcceptAssignmentDistance")]
        [HttpGet]
        [AllowAnonymous]
        public int GetAcceptAssignmentDistance()
        {
            int AcceptAssignmentDistance = Common.Constants.AppConstants.AcceptAssignmentDistance;
            return AcceptAssignmentDistance;
        }


        /// <summary>
        /// VerifyAssignmentRefNo   Payement ref no 
        /// </summary>
        /// <param name="DeliveryIssuanceId"></param>
        /// <param name="RefNo"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [Route("VerifyAssignmentRefNo")]
        [HttpGet]
        public async Task<bool> VerifyAssignmentRefNo(int DeliveryIssuanceId, string RefNo, int OrderId)
        {
            if (DeliveryIssuanceId > 0 && RefNo != null)
            {
                using (var context = new AuthContext())
                {
                    var DId = new SqlParameter("@DeliveryIssuanceId", DeliveryIssuanceId);
                    var Refn = new SqlParameter("@GatewayTransId", RefNo);
                    var OId = new SqlParameter("@OrderId", OrderId);
                    return await context.Database.SqlQuery<bool>("exec VerifyAssignmentRefNo @DeliveryIssuanceId, @GatewayTransId, @OrderId", DId, Refn, OId).FirstOrDefaultAsync();
                }
            }
            return false;
        }
    }
    #endregion
    public class res
    {
        public List<OrderHistory> OrderHistory { get; set; }
        public bool status { get; set; }
        public string Message { get; set; }
    }
    public class OrderDeliveryMasterDTODApp
    {
        public List<OrderDeliveryMaster> Order { get; set; }
        public bool status { get; set; }
        public string Message { get; set; }
    }
    public class OrderDispatchedDtoObj
    {
        public OrderDispatchedMasterDTO OrderDispatchedObj { get; set; }

        //public List<PaymentResponseRetailerApp> Payments { get; set; }
        public List<RetailerOrderPaymentDc> Payments { get; set; }
        public bool status { get; set; }
        public string Message { get; set; }
        public bool IsQREnabled { get; set; }

    }
    public class OrderDispatchedListDtoObj
    {
        public List<OrderDispatchedMasterDTOM> OrderDispatchedObj { get; set; }
        public bool status { get; set; }
        public string Message { get; set; }
        public int TotalOrderCount { get; set; }
        public bool IsShippedAssingId { get; set; }
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

        public List<DeliveryPaymentDTO> DeliveryPayments { get; set; }
        public string CancelrequestComments { get; set; } //by sudhir
        public DateTime? ConformationDate { get; set; }//bu sudhir
        public string DeliveryCanceledStatus { get; set; }//bu sudhir 
        public DeliveryOptimizationDc DeliveryOptimizationdc { get; set; }
        public DateTime? ReDispatchedDate { get; set; }
    }
    public class ResDTO
    {
        public OrderPlaceDTO op { get; set; }
        public string Message { get; set; }
        public bool Status { get; set; }
    }
    public class UploadAssignment
    {
        public string Message { get; set; }
        public bool Status { get; set; }
    }
    public class SalesLeadMobile
    {
        public string Mobile { get; set; }
        public string FcmId { get; set; }
    }
    public class OrderMobiledetail : OrderDetailForOTPTemplateDc
    {
        public string salespersonmobile { get; set; }
        public string salespersonfcmid { get; set; }
        public string agentmobile { get; set; }
        public string agentfcmid { get; set; }
        public string Salesfcmid { get; set; }
        public string customermobile { get; set; }
        public string customerfmcid { get; set; }
        public int warehouseid { get; set; }
        public string comments { get; set; }
        public double TotalAmt { get; set; }
        public long Storeid { get; set; }
        public int SalesId { get; set; }
        public int AgentId { get; set; }
        public int customerid { get; set; }
        public DateTime Deliverydate { get; set; }
    }
    public class OrderDeliveryDetail
    {
        public int OrderId { get; set; }
        public double GrossAmount { get; set; }
        public DateTime OrderedDate { get; set; }
    }
    public class OTP
    {
        public string OtpNo { get; set; }
    }
    public class DeliveryPaymentDTO
    {
        public int OrderId { get; set; }
        public string TransId { get; set; }
        public double amount { get; set; }
        public string PaymentFrom { get; set; }
        public string ChequeImageUrl { get; set; }
        public string ChequeBankName { get; set; }
        public DateTime PaymentDate { get; set; }//payemnt Date
        public bool IsVAN_RTGSNEFT { get; set; }
    }


    public class AssignmentCustomer
    {
        public int CustomerId { get; set; }
        public double? lat { get; set; }
        public double? lg { get; set; }
        public string ShippingAddress { get; set; }
        public double? wlat { get; set; }
        public double? wlg { get; set; }
        public double TotalOrderAmount { get; set; }
    }
    public class DeliveryOptimizationDc
    {
        public long TripPlannerConfirmedOrderId { get; set; }
        public int OrderId { get; set; }
        public double lat { get; set; }
        public double lng { get; set; }
    }



    #region GeoDirectionClass
    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse); 
    public class GeocodedWaypoint
    {
        public string geocoder_status { get; set; }
        public string place_id { get; set; }
        public List<string> types { get; set; }
    }

    public class Northeast
    {
        public double lat { get; set; }
        public double lng { get; set; }
    }

    public class Southwest
    {
        public double lat { get; set; }
        public double lng { get; set; }
    }

    public class Bounds
    {
        public Northeast northeast { get; set; }
        public Southwest southwest { get; set; }
    }

    public class Distance
    {
        public string text { get; set; }
        public int value { get; set; }
    }

    public class Duration
    {
        public string text { get; set; }
        public int value { get; set; }
    }

    public class EndLocation
    {
        public double lat { get; set; }
        public double lng { get; set; }
    }

    public class StartLocation
    {
        public double lat { get; set; }
        public double lng { get; set; }
    }

    public class Polyline
    {
        public string points { get; set; }
    }

    public class Step
    {
        public Distance distance { get; set; }
        public Duration duration { get; set; }
        public EndLocation end_location { get; set; }
        public string html_instructions { get; set; }
        public Polyline polyline { get; set; }
        public StartLocation start_location { get; set; }
        public string travel_mode { get; set; }
        public string maneuver { get; set; }
    }

    public class Leg
    {
        public Distance distance { get; set; }
        public Duration duration { get; set; }
        public string end_address { get; set; }
        public EndLocation end_location { get; set; }
        public string start_address { get; set; }
        public StartLocation start_location { get; set; }
        public List<Step> steps { get; set; }
        public List<object> traffic_speed_entry { get; set; }
        public List<object> via_waypoint { get; set; }
    }

    public class OverviewPolyline
    {
        public string points { get; set; }
    }

    public class Route
    {
        public Bounds bounds { get; set; }
        public string copyrights { get; set; }
        public List<Leg> legs { get; set; }
        public OverviewPolyline overview_polyline { get; set; }
        public string summary { get; set; }
        public List<object> warnings { get; set; }
        public List<int> waypoint_order { get; set; }
    }

    public class Root
    {
        public List<GeocodedWaypoint> geocoded_waypoints { get; set; }
        public List<Route> routes { get; set; }
        public string status { get; set; }
    }

    public class AssignmentDirectionDc
    {
        public int AssignmentId { get; set; }
        public string AssignmentDirectionPath { get; set; }
        public double AssignmentDistance { get; set; }
        public double ReturnDistance { get; set; }
        public DateTime CreatedDate { get; set; }
        public double AssignmentDuration { get; set; }
        public double ReturnDuration { get; set; }
        public double TotalUnloadingDuration { get; set; }
        public Root Root { get; set; }
    }

    public class ValidateOTPofOrder
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public string Status { get; set; }

        public string OTP { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public int CreatedBy { get; set; }
        public int? ModifiedBy { get; set; }
        public bool IsActive { get; set; }
        public double? lat { get; set; }
        public double? lg { get; set; }
        public String UserType { get; set; }

        public bool IsUsed { get; set; }

        public int? UserId { get; set; }

    }
    public class ResponseMsg
    {
        public bool Status { get; set; }
        public string Message { get; set; }

    }
    #endregion

}
