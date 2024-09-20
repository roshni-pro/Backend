using AngularJSAuthentication.Model;                         //CONTROLLERCS
using NLog;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using static AngularJSAuthentication.API.Controllers.OrderMasterrController;

namespace AngularJSAuthentication.API.Controllers
{
    [RoutePrefix("api/AutoProcessorder")]
    public class AutoProcessorderController : ApiController
    {


        public static Logger logger = LogManager.GetCurrentClassLogger();

        [Authorize]
        [HttpPost]
        [Route("priority")]
        public HttpResponseMessage Post(ooo oo)
        {
            using (var context = new AuthContext())
            {
                logger.Info("Order Automation");
                try
                {
                    List<OrderMaster> assignedorders = new List<OrderMaster>();

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
                    if (Warehouse_id > 0)
                    {
                        assignedorders = oo.assignedorders;
                        string MobileNumber = oo.mobile;
                        logger.Info("User ID : {0} , Company Id : {1}", compid, userid);
                        bool status = true;//context.AllOrderMasterspriority(assignedorders, Warehouse_id, MobileNumber);
                        logger.Info("End OrderMaster: ");
                        if (status == true)
                        {
                            return Request.CreateResponse(HttpStatusCode.OK);
                        }
                        else
                        {
                            return Request.CreateResponse(HttpStatusCode.BadRequest);
                        }
                    }
                    else
                    {
                        logger.Info("End  OrderMaster: ");
                        return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Got an error");
                    }
                }
                catch (Exception ex)
                {
                    logger.Error("Error in OrderMaster " + ex.Message);
                    logger.Info("End  OrderMaster: ");
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex.Message); ;
                }
            }
        }

        /// <summary>
        /// ExportForSelfOrders
        /// </summary>
        /// <param name="29/03/2019"></param>
        /// <param name="enddate"></param>
        /// <returns></returns>
        /// 
        [Route("getselforder")]
        [HttpGet]
        public IEnumerable<OrderMaster> getselfdata(DateTime start, DateTime end, int WarehouseId)
        {
            using (var db = new AuthContext())
            {
                logger.Info("start OrderMaster: ");

                List<OrderMaster> ass = new List<OrderMaster>();
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
                    logger.Info("User ID : {0} , Company Id : {1}", compid, userid);
                    var list = db.DbOrderMaster.Where(p => p.Deleted == false && p.OrderTakenSalesPersonId == 0 && p.CreatedDate >= start && p.CreatedDate <= end && p.WarehouseId == WarehouseId).ToList();
                    logger.Info("End OrderMaster: ");
                    return list;
                }
                catch (Exception ex)
                {
                    logger.Error("Error in OrderMaster " + ex.Message);
                    logger.Info("End OrderMaster: ");
                    return null;
                }
            }
        }

        [Route("GetOrderData")]
        public IEnumerable<OrderMaster> Getp(DateTime startdate, DateTime enddate)
        {
            using (var context = new AuthContext())
            {
                logger.Info("Order Automation");
                List<OrderMaster> orderdata = new List<OrderMaster>();
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
                    if (Warehouse_id > 0)
                    {
                        logger.Info("User ID : {0} , Company Id : {1}", compid, userid);
                        orderdata = context.PendingOrderByDate(startdate, enddate, Warehouse_id).ToList();
                        logger.Info("End OrderMaster: ");
                        return orderdata;
                    }
                    else
                    {
                        return orderdata;
                    }


                }
                catch (Exception ex)
                {
                    logger.Error("Error in OrderMaster " + ex.Message);
                    logger.Info("End  OrderMaster: ");
                    return orderdata;
                }
            }
        }

        [Route("")]
        public IEnumerable<OrderMaster> Get()
        {
            using (var context = new AuthContext())
            {
                logger.Info("start OrderMaster: ");
                List<OrderMaster> ass = new List<OrderMaster>();
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
                    ass = context.AllOrderMasters(compid).ToList();
                    logger.Info("End OrderMaster: ");
                    return ass;
                }
                catch (Exception ex)
                {
                    logger.Error("Error in OrderMaster " + ex.Message);
                    logger.Info("End  OrderMaster: ");
                    return null;
                }
            }
        }

        [Route("")]
        public OrderMaster Get(int id)
        {
            using (var context = new AuthContext())
            {
                logger.Info("start OrderMaster: ");

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


                    var ass = context.GetOrderMaster(id, compid);
                    logger.Info("End OrderMaster: ");
                    return ass;
                }
                catch (Exception ex)
                {
                    logger.Error("Error in OrderMaster " + ex.Message);
                    logger.Info("End  OrderMaster: ");
                    return null;
                }
            }
        }

        [Route("")]
        public PaggingData Get(int list, int page, int WarehouseId)
        {
            using (var context = new AuthContext())
            {
                logger.Info("start OrderMaster: ");
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
                    Warehouse_id = WarehouseId;
                    if (Warehouse_id > 0)
                    {

                        logger.Info("User ID : {0} , Company Id : {1}", compid, userid);
                        var ass = context.AllOrderMasterWid(list, page, compid, Warehouse_id);
                        logger.Info("End OrderMaster: ");
                        return ass;
                    }
                    else
                    {

                        logger.Info("User ID : {0} , Company Id : {1}", compid, userid);
                        var ass = context.AllOrderMaster(list, page, compid);
                        logger.Info("End OrderMaster: ");
                        return ass;
                    }
                }
                catch (Exception ex)
                {
                    logger.Error("Error in OrderMaster " + ex.Message);
                    logger.Info("End  OrderMaster: ");
                    return null;
                }
            }

        }

        [Route("")]
        public IEnumerable<OrderDispatchedMaster> Get(string OrderStatus, string t)
        {
            using (var context = new AuthContext())
            {
                logger.Info("start OrderMaster: ");
                List<OrderDispatchedMaster> ass = new List<OrderDispatchedMaster>();
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
                    ass = context.AllDispatchedOrderMaster(compid).ToList();
                    logger.Info("End OrderMaster: ");
                    return ass;
                }
                catch (Exception ex)
                {
                    logger.Error("Error in OrderMaster " + ex.Message);
                    logger.Info("End  OrderMaster: ");
                    return null;
                }
            }
        }

        [Route("StatusPendingData")]
        public IEnumerable<OrderMaster> GetPending(int WarehouseId)
        {

            logger.Info("start AutoProcessorder: ");
            PaggingData paggingData = new PaggingData();
            List<OrderDispatchedMaster> ass = new List<OrderDispatchedMaster>();
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
                // by neha : min stock quntity color show in red   12/07/2019:
                logger.Info("User ID : {0} , Company Id : {1}", compid, userid);
                List<OrderMaster> OrderMasters = new List<OrderMaster>();
                using (var myContext = new AuthContext())
                {

                    OrderMasters = myContext.DbOrderMaster.Where(x => x.Deleted != true && x.CompanyId == compid && x.WarehouseId == WarehouseId && x.Status == "Pending").Include(x => x.orderDetails).OrderByDescending(x => x.OrderId).ToList();

                }
                if (OrderMasters != null && OrderMasters.Any())
                {

                    List<int> orderids = OrderMasters.Select(x => x.OrderId).ToList();
                    using (var authContext = new AuthContext())
                    {
                        var itemorderstock = (from d in authContext.DbOrderDetails.Where(x => orderids.Contains(x.OrderId))
                                              join p in authContext.DbCurrentStock.Where(k => k.Deleted != true)
                                              on new
                                              {
                                                  ItemNumber = d.itemNumber,
                                                  d.WarehouseId,
                                                  d.ItemMultiMRPId
                                              } equals new
                                              {
                                                  ItemNumber = p.ItemNumber,
                                                  p.WarehouseId,
                                                  p.ItemMultiMRPId
                                              } into fg
                                              from fgi in fg.DefaultIfEmpty()
                                              select new
                                              {
                                                  Orderid = d.OrderId,
                                                  itemid = d.ItemId,
                                                  qty = d.qty,
                                                  Currentinventory = fgi != null ? fgi.CurrentInventory : 0
                                              });

                        foreach (var item in OrderMasters)
                        {
                            if (itemorderstock.Any(x => x.Orderid == item.OrderId && x.qty > x.Currentinventory))
                            {
                                if (item.Status == "Pending")
                                {
                                    item.IsLessCurrentStock = true;
                                }
                                else
                                {
                                    item.IsLessCurrentStock = false;
                                }

                            }
                        }
                    }
                }

                return OrderMasters;
            }
            catch (Exception ex)
            {
                logger.Error("Error in AutoProcessorder " + ex.Message);
                logger.Info("End  AutoProcessorder: ");
                return null;
            }
        }



        [Route("SearchData")]
        public async Task<PaggingData> getSearch(int WarehouseId, int list, int pageNo, int? clusterid)
        {
            PaggingData data = new PaggingData();

            logger.Info("start AutoProcessorder: ");
            PaggingData paggingData = new PaggingData();
            List<OrderDispatchedMaster> ass = new List<OrderDispatchedMaster>();
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
                // by neha : min stock quntity color show in red   12/07/2019:
                logger.Info("User ID : {0} , Company Id : {1}", compid, userid);
                List<OrderMaster> OrderMasters = new List<OrderMaster>();
                using (var myContext = new AuthContext())
                {

                    if (clusterid.HasValue)
                    {
                        data.total_count = await myContext.DbOrderMaster.Where(x => x.Deleted != true && x.WarehouseId == WarehouseId && x.ClusterId == clusterid.Value && x.Status == "Pending").CountAsync();
                        OrderMasters = await myContext.DbOrderMaster.Where(x => x.Deleted != true && x.WarehouseId == WarehouseId && x.ClusterId == clusterid.Value && x.Status == "Pending").Include(x => x.orderDetails).OrderByDescending(x => x.OrderId).Skip((pageNo - 1) * list).Take(list).ToListAsync();

                        foreach (var item in OrderMasters)
                        {
                            item.SalesPerson = string.Join(",", item.orderDetails.Where(z => !string.IsNullOrEmpty(z.ExecutiveName)).Select(z => z.ExecutiveName).Distinct());

                        }
                    }
                    else
                    {
                        data.total_count = await myContext.DbOrderMaster.Where(x => x.Deleted != true && x.WarehouseId == WarehouseId && x.Status == "Pending").CountAsync();
                        OrderMasters = await myContext.DbOrderMaster.Where(x => x.Deleted != true && x.WarehouseId == WarehouseId && x.Status == "Pending").Include(x => x.orderDetails).OrderByDescending(x => x.OrderId).Skip((pageNo - 1) * list).Take(list).ToListAsync();
                        foreach (var item in OrderMasters)
                        {
                            item.SalesPerson = string.Join(",", item.orderDetails.Where(z => !string.IsNullOrEmpty(z.ExecutiveName)).Select(z => z.ExecutiveName).Distinct());

                        }
                    }

                }

                if (OrderMasters != null && OrderMasters.Any())
                {

                    List<int> orderids = OrderMasters.Select(x => x.OrderId).ToList();
                    using (var authContext = new AuthContext())
                    {
                        var itemorderstock = (from d in authContext.DbOrderDetails.Where(x => orderids.Contains(x.OrderId) && !x.IsFreeItem)
                                              join p in authContext.DbCurrentStock.Where(k => k.Deleted != true)
                                              on new
                                              {
                                                  ItemNumber = d.itemNumber,
                                                  d.WarehouseId,
                                                  d.ItemMultiMRPId
                                              } equals new
                                              {
                                                  ItemNumber = p.ItemNumber,
                                                  p.WarehouseId,
                                                  p.ItemMultiMRPId
                                              } into fg
                                              from fgi in fg.DefaultIfEmpty()
                                              select new
                                              {
                                                  Orderid = d.OrderId,
                                                  itemid = d.ItemId,
                                                  qty = d.qty,
                                                  Currentinventory = fgi != null ? fgi.CurrentInventory : 0
                                              });

                        foreach (var item in OrderMasters)
                        {
                            if (itemorderstock.Any(x => x.Orderid == item.OrderId && x.qty > x.Currentinventory))
                            {
                                if (item.Status == "Pending")
                                {
                                    item.IsLessCurrentStock = true;
                                }
                                else
                                {
                                    item.IsLessCurrentStock = false;
                                }

                            }
                        }
                    }





                }
                data.ordermaster = OrderMasters;
                return data;
            }
            catch (Exception ex)
            {
                logger.Error("Error in AutoProcessorder " + ex.Message);
                logger.Info("End  AutoProcessorder: ");
                return null;
            }
        }


        [Route("")]
        [HttpPut]
        public OrderMaster Put(OrderMaster item)
        {
            using (var context = new AuthContext())
            {
                try
                {
                    var identity = User.Identity as ClaimsIdentity;
                    int compid = 0, userid = 0;
                    string UserName = null;
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
                        if (claim.Type == "username")
                        {
                            UserName = (claim.Value);
                        }
                    }
                    item.CompanyId = compid;
                    item.userid = userid;
                    logger.Info("User ID : {0} , Company Id : {1}", compid, userid);
                    return context.PutOrderMaster(item);
                }
                catch (Exception ex)
                {
                    return null;
                }
            }
        }


        [ResponseType(typeof(OrderMaster))]
        [Route("")]
        public IEnumerable<OrderMaster> Get(string Cityid, string Warehouseid, DateTime datefrom, DateTime dateto, string search, string status, string deliveryboy)
        {
            using (var context = new AuthContext())
            {
                logger.Info("start OrderMaster: ");
                List<OrderMaster> ass = new List<OrderMaster>();
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
                    ass = context.filteredOrderMaster(Cityid, Warehouseid, datefrom, dateto, search, status, deliveryboy, compid).ToList();
                    logger.Info("End OrderMaster: ");
                    return ass;
                }
                catch (Exception ex)
                {
                    logger.Error("Error in OrderMaster " + ex.Message);
                    logger.Info("End  OrderMaster: ");
                    return null;
                }
            }

        }

        [ResponseType(typeof(OrderMaster))]
        [Route("")]
        public IEnumerable<OrderMaster> Get(string mobile)
        {
            using (var context = new AuthContext())
            {
                logger.Info("start OrderMaster: ");
                List<OrderMaster> ass = new List<OrderMaster>();
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

                    ass = context.OrderMasterbymobile(mobile, compid).ToList();
                    logger.Info("End OrderMaster: ");
                    return ass;
                }
                catch (Exception ex)
                {
                    logger.Error("Error in OrderMaster " + ex.Message);
                    logger.Info("End  OrderMaster: ");
                    return null;
                }
            }
        }

        [Authorize]
        [ResponseType(typeof(OrderMaster))]
        [Route("")]
        [AcceptVerbs("POST")]
        //[Route("api/ItemMaster")]
        public OrderMaster add(OrderMaster item)
        {
            using (var context = new AuthContext())
            {
                logger.Info("start add OrderMaster: ");
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

                    item.CompanyId = compid;
                    if (item == null)
                    {
                        throw new ArgumentNullException("item");
                    }

                    context.AddOrderMaster(item);
                    logger.Info("User ID : {0} , Company Id : {1}", compid, userid);
                    logger.Info("End  add OrderMaster: ");
                    return item;

                }
                catch (Exception ex)
                {
                    logger.Error("Error in AddOrderMaster " + ex.Message);

                    return null;
                }
            }
        }

        [ResponseType(typeof(OrderMaster))]
        [Route("")]
        [AcceptVerbs("Delete")]
        public void Remove(int id)
        {
            using (var context = new AuthContext())
            {
                logger.Info("start delete OrderMaster: ");
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
                    logger.Info("User ID : {0} , Company Id : {1}", compid, userid);
                    context.DeleteOrderMaster(id, compid);
                    logger.Info("End  delete OrderMaster: ");
                }
                catch (Exception ex)
                {
                    logger.Error("Error in delete OrderMaster " + ex.Message);
                }
            }
        }

        [Route("")]
        public IEnumerable<OrderMaster> Get(string Warehouseid, DateTime datefrom, DateTime dateto)
        {
            using (var context = new AuthContext())
            {
                logger.Info("start OrderMaster: ");
                List<OrderMaster> ass = new List<OrderMaster>();
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
                    ass = context.filteredOrderMasters1(Warehouseid, datefrom, dateto, compid).ToList();
                    logger.Info("End OrderMaster: ");
                    return ass;
                }
                catch (Exception ex)
                {
                    logger.Error("Error in OrderMaster " + ex.Message);
                    logger.Info("End  OrderMaster: ");
                    return null;
                }
            }
        }
        #region get data first time order customer
        /// <summary>
        /// Get all customer detail first time customer
        /// create date 05/03/2019
        /// </summary>
        /// <param name="salespersonid"></param>
        /// <returns></returns>
        [Route("getdetail")]
        [HttpGet]
        public HttpResponseMessage get(int Warehouseid)
        {
            using (var db = new AuthContext())
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
                    List<FirstTimeOrderDTO> fto = new List<FirstTimeOrderDTO>();
                    DateTime startdate = DateTime.Now.Date.AddDays(-5);

                    var Customer = db.Customers.Where(x => x.Deleted == false && x.ordercount == 0 && x.Warehouseid == Warehouseid).ToList();
                    foreach (var cItems in Customer)
                    {
                        OrderMaster todaydata = db.DbOrderMaster.Where(x => x.Skcode == cItems.Skcode && x.Status == "Pending" && x.CreatedDate > startdate).SingleOrDefault();

                        //   var ftoc = db.DbOrderMaster.GroupBy(c => c.Skcode).Where(grp => grp.Count() == 1).ToList();
                        //  foreach (var cItems in ftoc)
                        //  {
                        //  OrderMaster  todaydata = db.DbOrderMaster.Where(x => x.Skcode== cItems.Key && x.Status == "Pending" && x.CreatedDate >startdate && x.WarehouseId== Warehouseid).SingleOrDefault();

                        if (todaydata != null)
                        {
                            City cityname = db.Cities.Where(x => x.Cityid == todaydata.CityId).FirstOrDefault();

                            FirstTimeOrderDTO BData = new FirstTimeOrderDTO()
                            {

                                ShopName = todaydata.ShopName,
                                Skcode = todaydata.Skcode,
                                WarehouseId = todaydata.WarehouseId,
                                WarehouseName = todaydata.WarehouseName,
                                CityName = cityname.CityName,
                                CustomerId = todaydata.CustomerId,
                                CustomerName = todaydata.CustomerName,
                                Mobile = todaydata.Customerphonenum,
                                Address = todaydata.ShippingAddress,
                                amount = todaydata.TotalAmount,
                                DateOfPurchase = todaydata.CreatedDate
                            };

                            fto.Add(BData);

                        }

                    }
                    return Request.CreateResponse(HttpStatusCode.OK, fto);

                }
                catch (Exception ex)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex.Message);
                }
            }
        }
        #endregion
        #region get orderhistory
        [Route("orderhistory")]
        [HttpGet]
        public dynamic orderhistory(int orderId)
        {
            using (var odd = new AuthContext())
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
                    logger.Info("User ID : {0} , Company Id : {1}", compid, userid);

                    var data = odd.OrderMasterHistoriesDB.Where(x => x.orderid == orderId).ToList();
                    return data;
                }
                catch (Exception ex)
                {
                    return false;
                }
            }
        }
        #endregion
        #region get Itemhistory
        [Route("Itemhistory")]
        [HttpGet]
        public dynamic Itemhistory(int orderId)
        {
            using (var context = new AuthContext())
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
                    logger.Info("User ID : {0} , Company Id : {1}", compid, userid);

                    var data = context.OrderItemHistoryDB.Where(x => x.orderid == orderId).ToList();
                    return data;
                }
                catch (Exception ex)
                {
                    return false;
                }
            }
        }
        #endregion


        #region Order Tranaction Processing update  
        /// <summary>
        /// Created Date 19/03/2019
        /// </summary>
        /// <param name="OrderId"></param>
        /// <returns>RD</returns>
        public OrderTransactionProcessing AddOrderTranaction(int OrderId, bool isdata)
        {
            using (var db = new AuthContext())
            {
                try
                {
                    OrderTransactionProcessing OTP = db.OrderTransactionProcessingDB.Where(X => X.OrderId == OrderId).FirstOrDefault();
                    OrderTransactionProcessing TP = new OrderTransactionProcessing();
                    if (OTP == null)
                    {

                        TP.OrderId = OrderId;
                        TP.IsDispatched = isdata;
                        TP.CreatedDate = DateTime.Now;
                        db.OrderTransactionProcessingDB.Add(TP);
                        db.Commit();


                    }
                    return TP;

                }
                catch (Exception ex)
                {
                    logger.Error("Error in put Complain " + ex.Message);
                    return null;

                }
            }
        }
        #endregion

        [Route("GetDelBoy")]
        [HttpGet]
        public dynamic GetDelBoy(int warehouseId)
        {
            using (var context = new AuthContext())
            {
                try

                {
                    var query = "select PeopleId as delboyId, DisplayName as DelboyName from people where Department='Delivery Boy' and Active=1 and Deleted=0 and WarehouseId=" + warehouseId;

                    var data = context.Database.SqlQuery<GetDelBoys>(query).ToList();

                    return data;

                }

                catch (Exception ex)
                {

                    ex.Message.ToString();
                    return null;

                }
            }
        }

    }
    public class GetDelBoys
    {
        public int delboyId { get; set; }
        public string DelboyName { get; set; }


    }
}

