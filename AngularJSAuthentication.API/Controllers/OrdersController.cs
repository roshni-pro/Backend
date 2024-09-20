using AngularJSAuthentication.API.Controllers.Base;
using AngularJSAuthentication.BusinessLayer.Managers.Masters;
using AngularJSAuthentication.DataContracts.Masters;
using AngularJSAuthentication.Model;
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
    [RoutePrefix("api/Orders")]
    public class OrdersController : BaseAuthController
    {

        public static Logger logger = LogManager.GetCurrentClassLogger();


        [Authorize]
        [Route("")]
        public IHttpActionResult Get()
        {
            return Ok(Order.CreateOrders());
        }


        [Route("GullakPaymentOptionEnable/{CustomerId}/{OrderId}")]
        [HttpGet]
        public async Task<bool> GullakPaymentOptionEnable(int CustomerId, int OrderId)
        {
            bool IsGullakEnabled = false;

            if (CustomerId > 0 && OrderId > 0)
            {
                using (var context = new AuthContext())
                {
                    var customerId = new SqlParameter("@customerId", CustomerId);
                    var orderId = new SqlParameter("@orderId", OrderId);
                    IsGullakEnabled = context.Database.SqlQuery<bool>("Exec GullakPaymentOptionEnable  @customerId, @orderId", customerId, orderId).FirstOrDefault();
                }
            }
            return IsGullakEnabled;
        }

        //[Route("")]
        //public IEnumerable<OrderMaster> Get(int salespersonid, int Skip, int Take)
        //{
        //    logger.Info("start OrderMaster: ");
        //    List<OrderMaster> ass = new List<OrderMaster>();
        //    using (var context = new AuthContext())
        //    {
        //        try
        //        {
        //            ass = context.OrderMasterbySalesPersonId(salespersonid, Skip, Take).ToList();
        //            logger.Info("End OrderMaster: ");
        //            return ass;
        //        }
        //        catch (Exception ex)
        //        {
        //            logger.Error("Error in OrderMaster " + ex.Message);
        //            logger.Info("End  OrderMaster: ");
        //            return null;
        //        }
        //    }
        //}

        [Route("")]
        public IEnumerable<OrderMaster> Get(string Mobile)
        {
            logger.Info("start OrderMaster: ");
            List<OrderMaster> ass = new List<OrderMaster>();
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


                    ass = context.OrderMasterbymobile(Mobile, compid).ToList();
                    logger.Info("End OrderMaster: ");
                    return ass;
                }
                catch (Exception ex)
                {
                    logger.Error("Error in OrderDispatchedMaster " + ex.Message);
                    logger.Info("End  OrderDispatchedMaster: ");
                    return null;
                }
            }
        }


        /// <summary>
        /// Updated by 22/12/2018
        /// </summary>
        /// <param name="CustomerId"></param>
        /// <returns></returns>
        [Route("")]
        [HttpGet]
        public HttpResponseMessage GetOrder(int CustomerId)
        {
            logger.Info("start OrderMaster: ");
            List<OrderMaster> ass = new List<OrderMaster>();
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
                    ass = context.DbOrderMaster.Include("orderDetails").Where(c => c.CustomerId == CustomerId && c.Deleted == false && c.active == true).ToList();
                    //confirmorder res;
                    //res = new confirmorder()
                    //{
                    //    om = ass,
                    //    status = true,
                    //    message = "Confirmed order."
                    //};
                    logger.Info("End OrderMaster: ");
                    return Request.CreateResponse(HttpStatusCode.OK, ass);
                }
                catch (Exception ex)
                {
                    logger.Error("Error in OrderDispatchedMaster " + ex.Message);
                    logger.Info("End  OrderDispatchedMaster: ");
                    object res = null;
                    return Request.CreateResponse(HttpStatusCode.OK, res);
                }
            }
        }

        /// <summary>
        /// Get Unconformed Order
        /// Created By 16/01/2019
        /// </summary>
        /// <param name="CustomerId"></param>
        /// <returns></returns>
        [Route("GetUnconformedOrder")]
        [HttpGet]
        public HttpResponseMessage GetUnconformedOrder(int CustomerId)
        {
            logger.Info("start Unconfirmed OrderMaster: ");
            List<InActiveCustomerOrderMaster> azz = new List<InActiveCustomerOrderMaster>();
            List<OrderMaster> ass = new List<OrderMaster>();
            using (var context = new AuthContext())
            {
                try
                {
                    azz = context.InActiveCustomerOrderMasterDB.Include("orderDetails").Where(c => c.CustomerId == CustomerId && c.Status == "Dummy Order" && c.Deleted == false).ToList();
                    ass = context.DbOrderMaster.Include("orderDetails").Where(c => c.CustomerId == CustomerId && c.Deleted == false).ToList();

                    notconfirmorder res = new notconfirmorder()
                    {
                        nco = azz,
                        co = ass
                    };
                    logger.Info("End Unconfirmed OrderMaster: ");
                    return Request.CreateResponse(HttpStatusCode.OK, res);
                }
                catch (Exception ex)
                {
                    logger.Error("Error in Unconfirmed " + ex.Message);
                    notconfirmorder res = new notconfirmorder()
                    {
                        nco = null,
                        co = null
                    };
                    return Request.CreateResponse(HttpStatusCode.OK, res);
                }
            }
        }

        [Route("MyOrders")]
        [HttpGet]
        public async Task<List<MyOrders>> MyOrders(int customerId)
        {
            List<MyOrders> myOrders = new List<MyOrders>();
            using (var myContext = new AuthContext())
            {

                var orders = await myContext.DbOrderMaster.Include("orderDetails").Where(x => x.CustomerId == customerId).ToListAsync();
                if (orders != null && orders.Any())
                {
                    if (orders.Any(x => x.Status.ToLower() == "sattled" || x.Status.ToLower() == "issued" || x.Status.ToLower() == "account settled"
                         || x.Status.ToLower() == "post order canceled"
                      || x.Status.ToLower() == "ready to dispatch"
                      || x.Status.ToLower() == "delivery canceled"
                      || x.Status.ToLower() == "partial receiving -bounce"
                      || x.Status.ToLower() == "delivered"
                      || x.Status.ToLower() == "partial settled"
                      || x.Status.ToLower() == "delivery redispatch"
                      || x.Status.ToLower() == "shipped"
                     ))
                    {
                        var dispatchOrderIds = orders.Where(x => x.Status.ToLower() == "sattled" || x.Status.ToLower() == "issued" || x.Status.ToLower() == "account settled"
                         || x.Status.ToLower() == "post order canceled"
                      || x.Status.ToLower() == "ready to dispatch"
                      || x.Status.ToLower() == "delivery canceled"
                      || x.Status.ToLower() == "partial receiving -bounce"
                      || x.Status.ToLower() == "delivered"
                      || x.Status.ToLower() == "partial settled"
                      || x.Status.ToLower() == "delivery redispatch"
                      || x.Status.ToLower() == "shipped"
                     ).Select(x => x.OrderId);


                        var dispatchMasters = await myContext.OrderDispatchedMasters.Include("orderDetails").
                              Where(x => dispatchOrderIds.Contains(x.OrderId)
                          ).ToListAsync();

                        myOrders = dispatchMasters.Select(x => new MyOrders
                        {
                            BillingAddress = x.BillingAddress,
                            CreatedDate = x.CreatedDate,
                            CustomerName = x.CustomerName,
                            DeliveredDate = orders.FirstOrDefault(z => z.OrderId == x.OrderId).DeliveredDate,
                            Deliverydate = x.Deliverydate,
                            EnablePayNowButton = false,
                            GrossAmount = x.GrossAmount,
                            OrderId = x.OrderId,
                            ReadytoDispatchedDate = orders.FirstOrDefault(z => z.OrderId == x.OrderId).ReadytoDispatchedDate,
                            ShippingAddress = x.ShippingAddress,
                            status = x.Status,
                            UpdatedDate = x.UpdatedDate,
                            walletPointUsed = orders.FirstOrDefault(z => z.OrderId == x.OrderId).walletPointUsed,
                            RewardPoint = x.RewardPoint,
                            itemDetails = x.orderDetails.Select(z => new MyOrderDetail
                            {
                                CompanyId = z.CompanyId,
                                ItemId = z.ItemId,
                                qty = z.qty,
                                UnitPrice = z.UnitPrice,
                                WarehouseId = z.WarehouseId,
                                ItemName = z.itemname,
                                LogoUrl = z.Itempic
                            }).ToList()
                        }).ToList();

                        orders.RemoveAll(x => dispatchMasters.Select(z => z.OrderId).Contains(x.OrderId));
                    }



                    myOrders.AddRange(orders.Select(x => new MyOrders
                    {
                        BillingAddress = x.BillingAddress,
                        CreatedDate = x.CreatedDate,
                        CustomerName = x.CustomerName,
                        DeliveredDate = orders.FirstOrDefault(z => z.OrderId == x.OrderId).DeliveredDate,
                        Deliverydate = x.Deliverydate,
                        EnablePayNowButton = false,
                        PayNowOption = "",
                        GrossAmount = x.GrossAmount,
                        OrderId = x.OrderId,
                        ReadytoDispatchedDate = orders.FirstOrDefault(z => z.OrderId == x.OrderId).ReadytoDispatchedDate,
                        ShippingAddress = x.ShippingAddress,
                        status = x.Status,
                        UpdatedDate = x.UpdatedDate,
                        walletPointUsed = x.walletPointUsed,
                        RewardPoint = x.RewardPoint,
                        itemDetails = x.orderDetails.Select(z => new MyOrderDetail
                        {
                            CompanyId = z.CompanyId,
                            ItemId = z.ItemId,
                            qty = z.qty,
                            UnitPrice = z.UnitPrice,
                            WarehouseId = z.WarehouseId,
                            ItemName = z.itemname,
                            LogoUrl = z.Itempic
                        }).ToList()

                    }));

                    var orderIds = myOrders.Select(x => x.OrderId).ToList();

                    var payments = myContext.PaymentResponseRetailerAppDb.Where(x => orderIds.Contains(x.OrderId) && x.status == "Success");

                    myOrders.ForEach(x =>
                    {
                        var orderPayment = payments.Where(z => z.OrderId == x.OrderId).ToList();
                        if (x.status.ToLower() == "failed" || x.status.ToLower() == "payment pending" || x.status.ToLower() == "pending" || x.status.ToLower() == "shipped")
                        {
                            if (orderPayment == null || !orderPayment.Any() || orderPayment.Any(z => z.PaymentFrom.ToLower() == "cash"))
                            {
                                x.EnablePayNowButton = true;
                                x.PayNowOption = x.status.ToLower() == "failed" || x.status.ToLower() == "payment pending" ? "Both"
                                        : x.status.ToLower() == "pending" || x.status.ToLower() == "shipped" ? "Online"
                                        : "";
                                x.RemainingAmount = x.GrossAmount - orderPayment.Where(z => z.IsOnline /*z.PaymentFrom.ToLower() != "cash"*/).Sum(z => z.amount);

                            }
                            else
                                x.EnablePayNowButton = false;
                        }

                        x.OrderPayments = orderPayment.Where(z => z.IsOnline /*z.PaymentFrom.ToLower() != "cash"*/).Select(z => new OrderPayments
                        {
                            Amount = z.amount,
                            PaymentFrom = z.PaymentFrom,
                            TransactionNumber = z.GatewayTransId,
                            TransactionDate = z.CreatedDate
                        }).ToList();

                    });


                }
            }

            return myOrders != null && myOrders.Any() ? myOrders.OrderByDescending(x => x.OrderId).ToList() : myOrders;
        }


        [Route("MyOrdersWithPage")]
        [HttpGet]
        [AllowAnonymous]
        public async Task<List<CustomerOrders>> MyOrdersWithPage(int customerId, int page = 0, int totalOrder = 10, string type = "")
        {
            string ConsumerApptype = new API.Helper.OrderPlaceHelper().GetCustomerAppType();
            List<CustomerOrders> customerOrders = new List<CustomerOrders>();
            List<MyOrders> myOrders = new List<MyOrders>();
            List<DialValuePoint> DialValues = new List<DialValuePoint>();
            using (var myContext = new AuthContext())
            {
                if (page > 0) page = page - 1;

                if (string.IsNullOrEmpty(type))
                    type = "ALL";
                //var orders = await myContext.DbOrderMaster.Include("orderDetails").Where(x => x.CustomerId == customerId).ToListAsync();
                CustomersManager manager = new CustomersManager();
                myOrders = manager.GetCustomerOrder(customerId, page, totalOrder, type, ConsumerApptype);

                if (myOrders != null && myOrders.Any())
                {
                    myOrders.ForEach(x =>
                    {
                        var orderPayment = x.OrderPayments;

                        if (x.status.ToLower() == "failed" || x.status.ToLower() == "payment pending" || x.status.ToLower() == "intransit" || x.status.ToLower() == "pending" || x.status.ToLower() == "shipped" || x.status.ToLower() == "ready to dispatch" || x.status.ToLower() == "issued")
                        {
                            if (x.GrossAmount > 0 && x.IsQRExpire == false && (orderPayment == null || !orderPayment.Any() || orderPayment.Any(z => z.PaymentFrom.ToLower() == "cash") || orderPayment.Any(z => z.PaymentFrom.ToLower() == "paylater")))
                            {
                                x.EnablePayNowButton = true;
                                x.PayNowOption = x.status.ToLower() == "failed" || x.status.ToLower() == "payment pending" ? "Both"
                                        : x.status.ToLower() == "pending" || x.status.ToLower() == "shipped" || x.status.ToLower() == "intransit" || x.status.ToLower() == "ready to dispatch" || x.status.ToLower() == "issued" ? "Online"
                                        : "";
                                if (x.IsPayLater == false)
                                {
                                    x.RemainingAmount = x.GrossAmount - orderPayment.Where(z => z.PaymentFrom.ToLower() != "cash").Sum(z => z.Amount);
                                }



                            }
                            else
                                x.EnablePayNowButton = false;
                        }

                        if (x.IsPayLater == true)
                        {

                            double getamount = 0;
                            if (orderPayment != null && orderPayment.Any())
                            {
                                var list = orderPayment.Where(a => a.IsPayLater == true && x.OrderId== a.OrderId).ToList();
                                if (list != null && list.Any())
                                {
                                    List<PayLaterAmountUpdateDC> lists = new List<PayLaterAmountUpdateDC>();
                                    foreach (var item in list)
                                    {
                                        PayLaterAmountUpdateDC singlepayment = new PayLaterAmountUpdateDC();
                                        singlepayment.Amount = item.Amount;
                                        singlepayment.Comment = item.Comment;
                                        lists.Add(singlepayment);
                                    }
                                    CurrencyManagementController currencyManagementController = new CurrencyManagementController();
                                    getamount = currencyManagementController.ReturnAmount(lists);

                                }
                            }
                            x.RemainingAmount = x.PaylaterAmount - getamount;

                            if (x.RemainingAmount > 0 && (x.status.ToLower() != "order canceled" && x.status.ToLower() != "failed" && x.status.ToLower() != "inactive"
                            && x.status.ToLower() != "intransit" && x.status.ToLower() != "delivery canceled" && x.status.ToLower() != "dummy order cancelled"
                            && x.status.ToLower() != "post order canceled" && x.status.ToLower() != "payment pending"))
                            {
                                x.EnablePayNowButton = true;
                                x.PayNowOption = x.status.ToLower() == "failed" || x.status.ToLower() == "payment pending" ? "Both"
                                        : x.status.ToLower() == "pending" || x.status.ToLower() == "shipped" || x.status.ToLower() == "intransit" || x.status.ToLower() == "ready to dispatch" || x.status.ToLower() == "issued" ? "Online"
                                        : "";
                            }
                            else
                            {
                                x.EnablePayNowButton = false;
                            }

                        }

                        x.OrderPayments = orderPayment.Where(z => z.IsPayLater == false && z.PaymentFrom.ToLower() != "cash").Select(z => new OrderPayments
                        {
                            Amount = z.Amount,
                            PaymentFrom = z.PaymentFrom,
                            TransactionNumber = z.TransactionNumber,
                            TransactionDate = z.TransactionDate
                        }).ToList();



                    });

                    var orderIds = myOrders.Select(x => x.OrderId).ToList();
                    var fromDate = DateTime.Now.AddHours(-24);
                    var toDate = DateTime.Now;
                    DialValues = myContext.DialValuePointDB.Where(x => x.OrderId.HasValue && orderIds.Contains(x.OrderId.Value) && !x.IsPlayWeel
                                                      && x.CreatedDate >= fromDate && x.CreatedDate <= toDate).ToList();
                }
            }

            myOrders = myOrders != null && myOrders.Any() ? myOrders.OrderByDescending(x => x.OrderId).ToList() : myOrders;



            customerOrders = myOrders.Select(x => new CustomerOrders
            {

                BillingAddress = x.BillingAddress,
                CreatedDate = x.CreatedDate,
                CustomerName = x.CustomerName,
                DeliveredDate = x.DeliveredDate.HasValue ? x.DeliveredDate.Value.ToString("dd'/'MM'/'yyyy hh:mm:ss") : "",
                Deliverydate = x.Deliverydate,
                EnablePayNowButton = x.EnablePayNowButton,
                GrossAmount = x.GrossAmount,
                itemDetails = x.itemDetails,
                OrderId = x.OrderId,
                OrderPayments = x.OrderPayments,
                PayNowOption = x.PayNowOption,
                ReadytoDispatchedDate = x.ReadytoDispatchedDate.HasValue ? x.ReadytoDispatchedDate.Value.ToString("dd'/'MM'/'yyyy hh:mm:ss") : "",
                RemainingAmount = x.RemainingAmount,
                RewardPoint = x.RewardPoint,
                ShippingAddress = x.ShippingAddress,
                status = x.status,
                UpdatedDate = x.UpdatedDate,
                walletPointUsed = x.walletPointUsed,
                IsPlayWeel = DialValues != null && DialValues.Any(y => y.OrderId == x.OrderId && !string.IsNullOrEmpty(y.EarnWheelList)),
                WheelCount = DialValues != null && DialValues.Any(y => y.OrderId == x.OrderId && !string.IsNullOrEmpty(y.EarnWheelList)) ? DialValues.FirstOrDefault(y => y.OrderId == x.OrderId).EarnWheelCount : 0,
                WheelList = DialValues != null && DialValues.Any(y => y.OrderId == x.OrderId && !string.IsNullOrEmpty(y.EarnWheelList)) ? DialValues.FirstOrDefault(y => y.OrderId == x.OrderId).EarnWheelList.Split(',').Select(y => Convert.ToInt32(y)).ToList() : new List<int>(),
                RebookOrder = x.RebookOrder,
                DeliveryOtp = x.DeliveryOtp,
                Rating = x.Rating,
                SalesPerson = x.SalesPerson,
                SalesPersonMobile = x.SalesPersonMobile,
                SalesPersonProfilePic = x.SalesPersonProfilePic,
                DeliveryPerson = x.DeliveryPerson,
                DeliveryPersonMobile = x.DeliveryPersonMobile,
                DboyProfilePic = x.DboyProfilePic,
                IsCustomerRaiseConcern = x.IsCustomerRaiseConcern,
                OrderType = x.OrderType,
                IsPayLater = x.IsPayLater
                //userOrderRatingDcs=x.userOrderRatingDcs,
                //userOrderStatusDC = x.userOrderStatusDC
            }).ToList();

            return customerOrders;
        }

        [Route("GetOrderStatusDetailByOrderId")]
        [HttpGet]
        [AllowAnonymous]
        public async Task<List<UserOrderStatusDC>> GetOrderStatusDetailByOrderId(int OrderId)
        {
            using (var myContext = new AuthContext())
            {
                var orderIdParam = new SqlParameter("@OrderId", OrderId);

                var result = myContext.Database.SqlQuery<UserOrderStatusDC>("GetOrderStatusDetail @OrderId", orderIdParam).ToList();
                return result;
            }
        }

        #region OrderConcern
        [Route("GetOrderConcernDataByOrderId")]
        [HttpGet]
        //[AllowAnonymous]
        public OrderConcernDataDc GetOrderConcernDataByOrderId(int OrderId)
        {
            CustomersManager manager = new CustomersManager();
            var result = manager.GetOrderConcernDataByOrderId(OrderId);
            return result;
        }
        #endregion
        #region PostOrderConcern
        [Route("PostOrderConcern")]
        [HttpPost]
        //[AllowAnonymous]
        public bool PostOrderConcern(CustomerRaiseCommentDc customerRaiseCommentDc)
        {
            using (var db = new AuthContext())
            {
                bool res = false;
                var list = db.OrderConcernDB.FirstOrDefault(x => x.OrderId == customerRaiseCommentDc.OrderId);
                if (list != null)
                {
                    //list.CreatedDate = DateTime.Now;
                    list.Status = "Pending";
                    list.IsCustomerRaiseConcern = true;
                    list.CustComment = customerRaiseCommentDc.CustComment;

                    db.Entry(list).State = EntityState.Modified;
                    var a = db.Commit();
                    if (a > 0)
                    {
                        list.Msg = "Data Added Sucesfully";
                        res = true;
                    }
                    else
                    {
                        list.Msg = "Something went wrong!";
                        res = false;
                    }

                }
                return res;
            }
        }
        #endregion
    }


    #region Helpers

    public class Order
    {
        public int OrderID { get; set; }
        public string CustomerName { get; set; }
        public string ShipperCity { get; set; }
        public Boolean IsShipped { get; set; }


        public static List<Order> CreateOrders()
        {
            List<Order> OrderList = new List<Order>
            {
                new Order {OrderID = 10248, CustomerName = "Taiseer Joudeh", ShipperCity = "Amman", IsShipped = true },
                new Order {OrderID = 10249, CustomerName = "Ahmad Hasan", ShipperCity = "Dubai", IsShipped = false},
                new Order {OrderID = 10250,CustomerName = "Tamer Yaser", ShipperCity = "Jeddah", IsShipped = false },
                new Order {OrderID = 10251,CustomerName = "Lina Majed", ShipperCity = "Abu Dhabi", IsShipped = false},
                new Order {OrderID = 10252,CustomerName = "Yasmeen Rami", ShipperCity = "Kuwait", IsShipped = true}
            };

            return OrderList;
        }
    }

    public class confirmorder
    {
        public List<OrderMaster> om { get; set; }
        public bool status { get; set; }
        public string message { get; set; }

    }
    public class notconfirmorder
    {
        public List<InActiveCustomerOrderMaster> nco { get; set; }
        public List<OrderMaster> co { get; set; }
    }
    #endregion
}
