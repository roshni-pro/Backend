using AgileObjects.AgileMapper;
using AngularJSAuthentication.BusinessLayer.Managers.Masters;
using AngularJSAuthentication.Common.Helpers;
using AngularJSAuthentication.DataContracts.Masters;
using AngularJSAuthentication.Model;
using NLog;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Web.Http;


namespace AngularJSAuthentication.API.ControllerV1
{
    [RoutePrefix("api/DeliveryOrder")]
    public class DeliveryOrderController : ApiController
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        [Route("")]
        [HttpGet]//get Delivery Boys list
        public HttpResponseMessage get()
        {
            logger.Info("start current stock: ");
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


                    if (Warehouse_id > 0)
                    {
                        string query = "select distinct p.* from People p inner join AspNetUsers u on p.Email=u.Email inner join AspNetUserRoles ur on u.Id=ur.UserId inner join AspNetRoles r on ur.RoleId=r.Id where p.WarehouseId=" + Warehouse_id + " and p.CompanyId=" + compid + " and r.Name='Delivery Boy' and ur.isActive=1 and p.Active=1 and p.Deleted=0";
                        var dboys = context.Database.SqlQuery<People>(query).ToList();
                        //var dboys = context.Peoples.Where(x => x.Department == "Delivery Boy" && x.Deleted == false && x.CompanyId == compid && x.WarehouseId == Warehouse_id && x.Active == true).ToList();

                        return Request.CreateResponse(HttpStatusCode.OK, dboys);
                    }
                    else
                    {
                        string query = "select distinct p.* from People p inner join AspNetUsers u on p.Email=u.Email inner join AspNetUserRoles ur on u.Id=ur.UserId inner join AspNetRoles r on ur.RoleId=r.Id where p.CompanyId=" + compid + " and r.Name='Delivery Boy' and ur.isActive=1 and p.Active=1 and p.Deleted=0";
                        var dboys = context.Database.SqlQuery<People>(query).ToList();
                        //var dboys = context.Peoples.Where(x => x.Department == "Delivery Boy" && x.Deleted == false && x.CompanyId == compid && x.Active == true).ToList();

                        return Request.CreateResponse(HttpStatusCode.OK, dboys);
                    }
                }
                catch (Exception)
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Error in getting Data");
                }
            }
        }
        [Route("")]
        [HttpGet]//get Delivery Boys list
        public HttpResponseMessage getwarehousebased(int Warehouseid)
        {
            logger.Info("start current stock: ");


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

                    string query = "select distinct p.PeopleID from People p inner join AspNetUsers u on p.Email=u.Email inner join AspNetUserRoles ur on u.Id=ur.UserId inner join AspNetRoles r on ur.RoleId=r.Id where p.WarehouseId=" + Warehouseid + " and r.Name='Delivery Boy' and ur.isActive=1 and p.Active=1 and p.Deleted=0";
                    List<int> PeopleId = context.Database.SqlQuery<int>(query).ToList();
                    var dboys = context.Peoples.Where(p => PeopleId.Contains(p.PeopleID)).ToList().OrderBy(x => x.DisplayName).ToList();
                    // var dboys = context.Peoples.Where(x => x.Department == "Delivery Boy" && x.Deleted == false && x.CompanyId == compid && x.WarehouseId == Warehouseid && x.Active == true).OrderBy(x => x.DisplayName).ToList();

                    return Request.CreateResponse(HttpStatusCode.OK, dboys);

                }
                catch (Exception)
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Error in getting Data");
                }
            }
        }


        /// <summary>
        /// Active and deactive Dboy list
        /// </summary>
        /// <param name="Warehouseid"></param>
        /// <returns></returns>
        [Route("GetDboy")]
        [HttpGet]//get Delivery Boys list
        public HttpResponseMessage getDboyActiveandDeactivebased(int Warehouseid)
        {
            logger.Info("start current stock: ");


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

                    string query = "select distinct p.PeopleID from People p inner join AspNetUsers u on p.Email=u.Email inner join AspNetUserRoles ur on u.Id=ur.UserId inner join AspNetRoles r on ur.RoleId=r.Id where p.WarehouseId=" + Warehouseid + " and r.Name='Delivery Boy'and p.Deleted=0";
                    //string query = "select distinct p.PeopleID from People p inner join AspNetUsers u on p.Email=u.Email inner join AspNetUserRoles ur on u.Id=ur.UserId inner join AspNetRoles r on ur.RoleId=r.Id where p.WarehouseId=" + Warehouseid + " and r.Name='Delivery Boy' and ur.isActive=1 and p.Active=1 and p.Deleted=0";
                    List<int> PeopleId = context.Database.SqlQuery<int>(query).ToList();
                    var dboys = context.Peoples.Where(p => PeopleId.Contains(p.PeopleID)).ToList().OrderBy(x => x.DisplayName).ToList();
                    // var dboys = context.Peoples.Where(x => x.Department == "Delivery Boy" && x.Deleted == false && x.CompanyId == compid && x.WarehouseId == Warehouseid && x.Active == true).OrderBy(x => x.DisplayName).ToList();

                    return Request.CreateResponse(HttpStatusCode.OK, dboys);

                }
                catch (Exception)
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Error in getting Data");
                }
            }
        }

        [Route("GetDboyActiveInActiveViceBasedFilter")]
        [HttpGet]//get Delivery Boys list
        public HttpResponseMessage GetDboyActiveInActiveViceBasedFilter(int Warehouseid, int Active)
        {
            logger.Info("start current stock: ");


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

                    string query = "select distinct p.PeopleID from People p inner join AspNetUsers u on p.Email=u.Email inner join AspNetUserRoles ur on u.Id=ur.UserId inner join AspNetRoles r on ur.RoleId=r.Id where p.WarehouseId=" + Warehouseid + " and r.Name='Delivery Boy'and p.Deleted=0 and p.Active=" + Active;
                    //string query = "select distinct p.PeopleID from People p inner join AspNetUsers u on p.Email=u.Email inner join AspNetUserRoles ur on u.Id=ur.UserId inner join AspNetRoles r on ur.RoleId=r.Id where p.WarehouseId=" + Warehouseid + " and r.Name='Delivery Boy' and ur.isActive=1 and p.Active=1 and p.Deleted=0";
                    List<int> PeopleId = context.Database.SqlQuery<int>(query).ToList();
                    var dboys = context.Peoples.Where(p => PeopleId.Contains(p.PeopleID)).ToList().OrderBy(x => x.DisplayName).ToList();
                    // var dboys = context.Peoples.Where(x => x.Department == "Delivery Boy" && x.Deleted == false && x.CompanyId == compid && x.WarehouseId == Warehouseid && x.Active == true).OrderBy(x => x.DisplayName).ToList();

                    return Request.CreateResponse(HttpStatusCode.OK, dboys);

                }
                catch (Exception)
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Error in getting Data");
                }
            }
        }



        [Authorize]
        [Route("ByOrdertype")]
        [HttpGet]
        public HttpResponseMessage ByOrdertype(string mob, int OrderType)
        {
            using (var context = new AuthContext())
            {
                var identity = User.Identity as ClaimsIdentity;
                int compid = 0;

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))
                    compid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);

                bool IsZaruriOrder = false;
                List<OrderDispatchedMaster> finalList = new List<OrderDispatchedMaster>();
                List<OrderDispatchedMaster> list = new List<OrderDispatchedMaster>();
                List<int> noteligibleOrder = new List<int>();
                List<int> founndList = new List<int>();

                var people = context.Peoples.Where(x => x.Mobile == mob && x.Deleted == false && x.Active == true && x.WarehouseId > 0).FirstOrDefault();
                int warehouseid = people.WarehouseId;
                if (warehouseid > 0 && people.PeopleID > 0)
                {
                    bool IsDeliveryOptimizationEnabled = context.Warehouses.FirstOrDefault(x => x.WarehouseId == warehouseid).IsDeliveryOptimizationEnabled;
                    var AllPendingAssigment = context.OrderDispatchedMasters.Where(a => a.WarehouseId == warehouseid && (a.Status == "Ready to Dispatch" || a.Status == "Delivery Redispatch") && a.Deleted == false).Select(x => new { x.WarehouseId, x.DboyMobileNo, x.OrderDispatchedMasterId, x.OrderId }).ToList();
                    var DboylistAssignment = AllPendingAssigment.Where(a => a.DboyMobileNo == mob).ToList();
                    if (DboylistAssignment.Any())
                    {
                        var OrderDispatchMasterIds = DboylistAssignment.Select(x => x.OrderDispatchedMasterId).ToList();
                        list = context.OrderDispatchedMasters.Where(x => OrderDispatchMasterIds.Contains(x.OrderDispatchedMasterId)).Include("orderDetails").ToList();
                        var OrderIds = list.Select(x => x.OrderId).ToList();

                        if (OrderType == 0) { OrderType = 1; }

                        var ordermasters = context.DbOrderMaster.Where(a => OrderIds.Contains(a.OrderId) && a.OrderType == OrderType).Select(x => new { x.OrderId, x.OrderType, x.IsPrimeCustomer });
                        IsZaruriOrder = ordermasters.Any(a => a.OrderType == 5);

                        list.ForEach(x =>
                        {
                            if (ordermasters.Any(y => y.OrderId == x.OrderId))
                            {
                                x.OrderType = ordermasters.FirstOrDefault(y => y.OrderId == x.OrderId).OrderType;
                                x.IsPrimeCustomer = ordermasters.FirstOrDefault(y => y.OrderId == x.OrderId).IsPrimeCustomer;

                            }
                        });

                        if (ordermasters.Any(a => a.OrderType == 4))
                        {
                            var RDSOrderIds = ordermasters.Where(a => a.OrderType == 4).Select(x => x.OrderId).ToList();
                            var ordePayments = context.PaymentResponseRetailerAppDb.Where(a => RDSOrderIds.Contains(a.OrderId) && (a.PaymentFrom == "Gullak" || a.PaymentFrom == "hdfc")
                                                              && a.status == "Success").ToList();

                            var RDSOrders = list.Where(x => RDSOrderIds.Contains(x.OrderId));

                            foreach (var item in RDSOrders)
                            {
                                var amount = ordePayments.Where(x => x.OrderId == item.OrderId).Sum(x => x.amount);
                                if (amount != item.GrossAmount)
                                {
                                    noteligibleOrder.Add(item.OrderId);
                                }
                            }
                        }
                    }

                    if (list.Any())
                    {
                        var Asigmentids = list.Where(x => x.DeliveryIssuanceIdOrderDeliveryMaster > 0).Select(x => x.DeliveryIssuanceIdOrderDeliveryMaster).Distinct().ToList();
                        if (Asigmentids != null && Asigmentids.Any())
                        {
                            founndList = context.DeliveryIssuanceDb.Where(x => Asigmentids.Contains(x.DeliveryIssuanceId) && (x.Status == "Submitted" || x.Status == "Payment Accepted" || x.Status == "Pending")).Select(x => x.DeliveryIssuanceId).ToList();
                        }

                        var redispatchOrderIds = list.Where(x => x.ReDispatchCount > 0).Select(x => x.OrderId).ToList();
                        var orderredispatchdatas = context.orderRedispatchCountApprovalDB.Where(x => redispatchOrderIds.Contains(x.OrderId)).Select(x => new { OrderId = x.OrderId, x.Id, IsApproved = x.IsApproved, Redispatchcount = x.Redispatchcount }).ToList();

                        var redispatchedcharge = new List<DeliveryRedispatchChargeConfDc>();
                        //for DeliveryRedispatched Charge
                        if (list.Any(x => x.ReDispatchCount > 0))
                        {
                            var CityId = list.Select(x => x.CityId).FirstOrDefault();
                            var result = context.DeliveryRedispatchChargeConfs.Where(x => x.IsActive == true && x.IsDeleted == false && x.CityId == CityId).ToList();
                            redispatchedcharge = Mapper.Map(result).ToANew<List<DeliveryRedispatchChargeConfDc>>();
                        }

                        foreach (var ast in list.Where(x => x.OrderType > 0))//.Where(x=> !noteligibleOrder.Contains(x.OrderId)))
                        {
                            ast.IsClusterStoppedManualAssignment = IsDeliveryOptimizationEnabled;

                            if (IsZaruriOrder)
                            {
                                ast.OrderType = 5;
                            }
                            else if (ast.OrderType != 4)
                            {
                                ast.OrderType = 1;
                            }
                            else if (ast.OrderType == 4 && noteligibleOrder.Any(x => x == ast.OrderId))
                            {
                                ast.IsNotCreateAssingment = true;
                            }

                            if (ast.Status == "Delivery Redispatch")
                            {
                                var ComfrimReDispatched = context.DeliveryCanceledRequestHistoryDb.Where(x => x.OrderId == ast.OrderId && x.IsActive == true && x.IsDeleted == false && x.ConformationDate != null).FirstOrDefault();
                                ast.IsCreateAssingmentReDispatched = false;
                                ast.IsNotCreateAssingmentAwaitingReDispatchedOrderId = false;
                                if (ComfrimReDispatched != null)
                                {
                                    DateTime aDate = DateTime.Now;

                                    DateTime Comfrimdata = Convert.ToDateTime(ComfrimReDispatched.ConformationDate?.ToString("yyyy-MM-dd"));
                                    DateTime todaydate = DateTime.Now;

                                    if (todaydate > Comfrimdata)
                                    {
                                        ast.IsCreateAssingmentReDispatched = true;
                                    }
                                    else
                                    {
                                        ast.IsNotCreateAssingmentAwaitingReDispatchedOrderId = true;
                                        ast.ConformationDate = ComfrimReDispatched.ConformationDate;
                                    }
                                }
                                var Callbackdata = context.DeliveryCanceledRequestHistoryDb.Where(x => x.OrderId == ast.OrderId && x.IsActive == true && x.IsDeleted == false && x.DeliveryCanceledStatus == "Call back").FirstOrDefault();
                                if (Callbackdata != null)
                                {
                                    ast.isCallbackCheck = true;
                                }
                            }
                            var founnd = founndList.Any(x => x == ast.DeliveryIssuanceIdOrderDeliveryMaster);
                            if (!founnd)
                            {
                                if (ast.ReDispatchCount > 0)
                                {
                                    var orderredispatchdata = orderredispatchdatas.FirstOrDefault(x => x.OrderId == ast.OrderId && x.Redispatchcount == ast.ReDispatchCount);
                                    if (orderredispatchdata != null)
                                    {
                                        ast.SendApproval = true;
                                        if (orderredispatchdata.IsApproved == true)
                                        {
                                            ast.IsApproved = orderredispatchdata.IsApproved;
                                        }
                                    }
                                    else
                                    {
                                        ast.SendApproval = false;
                                        ast.IsApproved = false;
                                    }
                                }
                                finalList.Add(ast);
                            }
                        }

                        //for distance Add by anushka (20/01/2020)
                        var customerids = finalList.Select(x => x.CustomerId).Distinct().ToList();
                        var warehouseids = finalList.Select(x => x.WarehouseId).Distinct().ToList();
                        CustomersManager manager = new CustomersManager();
                        var customerWarehouseLtlng = manager.GetCustomerOrder(customerids, warehouseids);
                        finalList.ForEach(x =>
                        {
                            if (customerWarehouseLtlng != null && customerWarehouseLtlng.CustomerLtlng != null && customerWarehouseLtlng.WarehouseLtlng != null)
                            {
                                var customerltlng = customerWarehouseLtlng.CustomerLtlng.FirstOrDefault(y => x.CustomerId == y.Id);
                                var warehouseltlng = customerWarehouseLtlng.WarehouseLtlng.FirstOrDefault(y => x.WarehouseId == y.Id);

                                if (warehouseltlng != null && warehouseltlng.lat != 0 && warehouseltlng.lg != 0 && customerltlng != null && customerltlng.lat != 0 && customerltlng.lat >= -180.0 && customerltlng.lat <= 180.0 && customerltlng.lg != 0 && customerltlng.lg >= -180.0 && customerltlng.lg <= 180.0)
                                {
                                    var sourceGeoCordinates = new System.Device.Location.GeoCoordinate(warehouseltlng.lat, warehouseltlng.lg);
                                    var destination = new System.Device.Location.GeoCoordinate(customerltlng.lat, customerltlng.lg);
                                    var dist = GeoHelper.AerialDistance(sourceGeoCordinates, destination);
                                    x.Distance = Math.Round(dist, 2);
                                }
                            }
                            if (x.ReDispatchCount > 0 && redispatchedcharge != null && redispatchedcharge.Any() && redispatchedcharge.Any(s => s.RedispatchCount == x.ReDispatchCount))
                            {
                                x.deliveryCharge += redispatchedcharge.FirstOrDefault(s => s.RedispatchCount == x.ReDispatchCount).RedispatchCharge;
                            }
                        });
                    }
                    finalList = finalList.OrderBy(x => x.Distance).ThenBy(x => x.CustomerId).ThenBy(x => x.CreatedDate).ToList();
                }
                return Request.CreateResponse(HttpStatusCode.OK, finalList);
            }
        }





        [Authorize]
        [Route("")]
        [HttpGet]
        public HttpResponseMessage getbyId(string mob)
        {
            using (var context = new AuthContext())
            {

                var identity = User.Identity as ClaimsIdentity;
                int compid = 0;

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))
                    compid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);
                //var DBoyorders = context.getdboysOrder(mob, compid);
                bool IsZaruriOrder = false;
                List<OrderDispatchedMaster> finalList = new List<OrderDispatchedMaster>();
                List<OrderDispatchedMaster> list = new List<OrderDispatchedMaster>();
                List<int> noteligibleOrder = new List<int>();
                List<int> founndList = new List<int>();

                var people = context.Peoples.Where(x => x.Mobile == mob && x.Deleted == false && x.Active == true && x.WarehouseId > 0).FirstOrDefault();
                int warehouseid = people.WarehouseId;
                if (warehouseid > 0)
                {
                    bool IsDeliveryOptimizationEnabled = context.Warehouses.FirstOrDefault(x => x.WarehouseId == warehouseid).IsDeliveryOptimizationEnabled;


                    var AllPendingAssigment = context.OrderDispatchedMasters.Where(a => a.WarehouseId == warehouseid && (a.Status == "Ready to Dispatch" || a.Status == "Delivery Redispatch") && a.Deleted == false).Select(x => new { x.WarehouseId, x.DboyMobileNo, x.OrderDispatchedMasterId, x.OrderId }).ToList();

                    //.Include("orderDetails").ToList();
                    var DboylistAssignment = AllPendingAssigment.Where(a => a.DboyMobileNo == mob).ToList();
                    if (DboylistAssignment.Any())
                    {
                        var OrderDispatchMasterIds = DboylistAssignment.Select(x => x.OrderDispatchedMasterId).ToList();
                        list = context.OrderDispatchedMasters.Where(x => OrderDispatchMasterIds.Contains(x.OrderDispatchedMasterId)).Include("orderDetails").ToList();
                        var OrderIds = list.Select(x => x.OrderId).ToList();
                        var ordermasters = context.DbOrderMaster.Where(a => OrderIds.Contains(a.OrderId)).Select(x => new { x.OrderId, x.OrderType, x.IsPrimeCustomer });
                        IsZaruriOrder = ordermasters.Any(a => a.OrderType == 5);
                        list.ForEach(x =>
                        {
                            if (ordermasters.Any(y => y.OrderId == x.OrderId))
                            {
                                x.OrderType = ordermasters.FirstOrDefault(y => y.OrderId == x.OrderId).OrderType;
                                x.IsPrimeCustomer = ordermasters.FirstOrDefault(y => y.OrderId == x.OrderId).IsPrimeCustomer;

                            }
                        });

                        if (ordermasters.Any(a => a.OrderType == 4))
                        {
                            var RDSOrderIds = ordermasters.Where(a => a.OrderType == 4).Select(x => x.OrderId).ToList();
                            var ordePayments = context.PaymentResponseRetailerAppDb.Where(a => RDSOrderIds.Contains(a.OrderId) && (a.PaymentFrom == "Gullak" || a.PaymentFrom == "hdfc")
                                                              && a.status == "Success").ToList();

                            var RDSOrders = list.Where(x => RDSOrderIds.Contains(x.OrderId));

                            foreach (var item in RDSOrders)
                            {
                                var amount = ordePayments.Where(x => x.OrderId == item.OrderId).Sum(x => x.amount);
                                if (amount != item.GrossAmount)
                                {
                                    noteligibleOrder.Add(item.OrderId);
                                }
                            }
                        }
                    }

                    if (list.Any())
                    {
                        var Asigmentids = list.Where(x => x.DeliveryIssuanceIdOrderDeliveryMaster > 0).Select(x => x.DeliveryIssuanceIdOrderDeliveryMaster).Distinct().ToList();
                        if (Asigmentids != null && Asigmentids.Any())
                        {
                            founndList = context.DeliveryIssuanceDb.Where(x => Asigmentids.Contains(x.DeliveryIssuanceId) && (x.Status == "Submitted" || x.Status == "Payment Accepted" || x.Status == "Pending")).Select(x => x.DeliveryIssuanceId).ToList();
                        }

                        var redispatchOrderIds = list.Where(x => x.ReDispatchCount > 0).Select(x => x.OrderId).ToList();
                        var orderredispatchdatas = context.orderRedispatchCountApprovalDB.Where(x => redispatchOrderIds.Contains(x.OrderId)).Select(x => new { OrderId = x.OrderId, x.Id, IsApproved = x.IsApproved, Redispatchcount = x.Redispatchcount }).ToList();

                        var redispatchedcharge = new List<DeliveryRedispatchChargeConfDc>();
                        //for DeliveryRedispatched Charge
                        if (list.Any(x => x.ReDispatchCount > 0))
                        {
                            var CityId = list.Select(x => x.CityId).FirstOrDefault();
                            var result = context.DeliveryRedispatchChargeConfs.Where(x => x.IsActive == true && x.IsDeleted == false && x.CityId == CityId).ToList();
                            redispatchedcharge = Mapper.Map(result).ToANew<List<DeliveryRedispatchChargeConfDc>>();
                        }


                        foreach (var ast in list)//.Where(x=> !noteligibleOrder.Contains(x.OrderId)))
                        {

                            ast.IsClusterStoppedManualAssignment = IsDeliveryOptimizationEnabled;

                            if (IsZaruriOrder)
                            {
                                ast.OrderType = 5;
                            }
                            else if (ast.OrderType != 4)
                            {
                                ast.OrderType = 1;
                            }
                            else if (ast.OrderType == 4 && noteligibleOrder.Any(x => x == ast.OrderId))
                            {
                                ast.IsNotCreateAssingment = true;
                            }
                            if (ast.Status == "Delivery Redispatch")
                            {
                                var ComfrimReDispatched = context.DeliveryCanceledRequestHistoryDb.Where(x => x.OrderId == ast.OrderId && x.IsActive == true && x.IsDeleted == false && x.ConformationDate != null).FirstOrDefault();
                                ast.IsCreateAssingmentReDispatched = false;
                                ast.IsNotCreateAssingmentAwaitingReDispatchedOrderId = false;
                                if (ComfrimReDispatched != null)
                                {
                                    DateTime aDate = DateTime.Now;

                                    DateTime Comfrimdata = Convert.ToDateTime(ComfrimReDispatched.ConformationDate?.ToString("yyyy-MM-dd"));
                                    DateTime todaydate = DateTime.Now;

                                    if (todaydate > Comfrimdata)
                                    {
                                        ast.IsCreateAssingmentReDispatched = true;
                                    }
                                    else
                                    {
                                        ast.IsNotCreateAssingmentAwaitingReDispatchedOrderId = true;
                                        ast.ConformationDate = ComfrimReDispatched.ConformationDate;
                                    }
                                }
                                var Callbackdata = context.DeliveryCanceledRequestHistoryDb.Where(x => x.OrderId == ast.OrderId && x.IsActive == true && x.IsDeleted == false && x.DeliveryCanceledStatus == "Call back").FirstOrDefault();
                                if (Callbackdata != null)
                                {
                                    ast.isCallbackCheck = true;
                                }
                            }
                            var founnd = founndList.Any(x => x == ast.DeliveryIssuanceIdOrderDeliveryMaster);
                            if (!founnd)
                            {
                                if (ast.ReDispatchCount > 0)
                                {
                                    var orderredispatchdata = orderredispatchdatas.FirstOrDefault(x => x.OrderId == ast.OrderId && x.Redispatchcount == ast.ReDispatchCount);
                                    if (orderredispatchdata != null)
                                    {
                                        ast.SendApproval = true;
                                        if (orderredispatchdata.IsApproved == true)
                                        {
                                            ast.IsApproved = orderredispatchdata.IsApproved;
                                        }
                                    }
                                    else
                                    {
                                        ast.SendApproval = false;
                                        ast.IsApproved = false;
                                    }
                                }
                                finalList.Add(ast);
                            }
                        }

                        //for distance Add by anushka (20/01/2020)
                        var customerids = finalList.Select(x => x.CustomerId).Distinct().ToList();
                        var warehouseids = finalList.Select(x => x.WarehouseId).Distinct().ToList();
                        CustomersManager manager = new CustomersManager();
                        var customerWarehouseLtlng = manager.GetCustomerOrder(customerids, warehouseids);
                        finalList.ForEach(x =>
                        {
                            if (customerWarehouseLtlng != null && customerWarehouseLtlng.CustomerLtlng != null && customerWarehouseLtlng.WarehouseLtlng != null)
                            {
                                var customerltlng = customerWarehouseLtlng.CustomerLtlng.FirstOrDefault(y => x.CustomerId == y.Id);
                                var warehouseltlng = customerWarehouseLtlng.WarehouseLtlng.FirstOrDefault(y => x.WarehouseId == y.Id);

                                if (warehouseltlng != null && warehouseltlng.lat != 0 && warehouseltlng.lg != 0 && customerltlng != null && customerltlng.lat != 0 && customerltlng.lat >= -180.0 && customerltlng.lat <= 180.0 && customerltlng.lg != 0 && customerltlng.lg >= -180.0 && customerltlng.lg <= 180.0)
                                {
                                    var sourceGeoCordinates = new System.Device.Location.GeoCoordinate(warehouseltlng.lat, warehouseltlng.lg);
                                    var destination = new System.Device.Location.GeoCoordinate(customerltlng.lat, customerltlng.lg);
                                    var dist = GeoHelper.AerialDistance(sourceGeoCordinates, destination);
                                    x.Distance = Math.Round(dist, 2);
                                }
                            }
                            if (x.ReDispatchCount > 0 && redispatchedcharge != null && redispatchedcharge.Any() && redispatchedcharge.Any(s => s.RedispatchCount == x.ReDispatchCount))
                            {
                                x.deliveryCharge += redispatchedcharge.FirstOrDefault(s => s.RedispatchCount == x.ReDispatchCount).RedispatchCharge;
                            }
                        });


                        //end
                        //return finalList;
                    }
                    finalList = finalList.OrderBy(x => x.Distance).ThenBy(x => x.CustomerId).ThenBy(x => x.CreatedDate).ToList();

                }
                return Request.CreateResponse(HttpStatusCode.OK, finalList);


            }
        }

        [Route("")]
        [HttpGet]
        public HttpResponseMessage getAppOrders(string M, string mob) //get orders for delivery
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

                    if (M == "all")
                    {
                        var DBoyorders = context.getallOrderofboy(mob);

                        return Request.CreateResponse(HttpStatusCode.OK, DBoyorders);
                    }
                    else
                    {
                        var DBoyorders = context.getAcceptedOrders(mob);
                        return Request.CreateResponse(HttpStatusCode.OK, DBoyorders);
                    }


                }
                catch (Exception ex)
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, ex.Message);
                }
            }
        }

        [Route("")]
        [HttpPost]
        public HttpResponseMessage post(List<OrderDispatchedMaster> obj, string mob) //Order change delivery boy
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

                    var DBoyorders = context.changeDBoy(obj, mob, compid, userid);
                    if (DBoyorders == null)
                    {
                        return Request.CreateResponse(HttpStatusCode.BadRequest, "Error Occured");
                    }
                    return Request.CreateResponse(HttpStatusCode.OK, DBoyorders);
                }
                catch (Exception ex)
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, ex.Message);
                }
            }
        }

        [Route("GetOfferOnOrder")]
        [HttpGet]
        public dynamic GetOfferOnOrder(int OrderId) //get orders for delivery
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
                    var OfferData = context.GetOfferOnOrder(OrderId, compid);
                    return OfferData;

                }
                catch (Exception ex)
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, ex.Message);
                }
            }
        }

    }
}
