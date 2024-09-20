using AngularJSAuthentication.Model;
using NLog;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Web.Http;

namespace AngularJSAuthentication.API.Controllers
{
    [RoutePrefix("api/OrderPending")]
    public class OrderPendingController : ApiController
    {

        public static Logger logger = LogManager.GetCurrentClassLogger();

        [Route("")]
        [HttpGet]
        public HttpResponseMessage Get(int list, int page)
        {
            logger.Info("start OrderMaster: ");
            PaggingData obj = new PaggingData();
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
                        logger.Info("User ID : {0} , Company Id : {1}", compid, userid);

                        var newdata = context.DbOrderMaster.Where(x => x.Deleted == false && (x.Status == "Pending" || x.Status == "Issued" || x.Status == "Shipped" || x.Status == "Delivery Redispatch" || x.Status == "Ready to Dispatch") && x.CompanyId == compid && x.WarehouseId == Warehouse_id).OrderByDescending(x => x.OrderId).Skip((page - 1) * list).Take(list).ToList();
                        obj.total_count = context.DbOrderMaster.Where(x => x.Deleted == false && (x.Status == "Pending" || x.Status == "Issued" || x.Status == "Shipped" || x.Status == "Delivery Redispatch" || x.Status == "Ready to Dispatch") && x.CompanyId == compid && x.WarehouseId == Warehouse_id).Count();

                        //var newdata = context.DbOrderMaster.Where(x => x.Deleted == false && x.Status == "Pending" && x.CompanyId == compid &&x.WarehouseId== Warehouse_id).OrderByDescending(x => x.OrderId).Skip((page - 1) * list).Take(list).ToList();
                        //  obj.total_count = context.DbOrderMaster.Where(x => x.Deleted == false && x.Status == "Pending" && x.CompanyId == compid && x.WarehouseId == Warehouse_id).Count();



                        obj.ordermaster = newdata;
                        logger.Info("End OrderMaster: ");
                        return Request.CreateResponse(HttpStatusCode.OK, obj);
                    }

                    else
                    {
                        logger.Info("User ID : {0} , Company Id : {1}", compid, userid);

                        var newdata = context.DbOrderMaster.Where(x => x.Deleted == false && (x.Status == "Pending" || x.Status == "Issued" || x.Status == "Shipped" || x.Status == "Delivery Redispatch" || x.Status == "Ready to Dispatch") && x.CompanyId == compid).OrderByDescending(x => x.OrderId).Skip((page - 1) * list).Take(list).ToList();
                        obj.total_count = context.DbOrderMaster.Where(x => x.Deleted == false && (x.Status == "Pending" || x.Status == "Issued" || x.Status == "Shipped" || x.Status == "Delivery Redispatch" || x.Status == "Ready to Dispatch") && x.CompanyId == compid).Count();


                        obj.ordermaster = newdata;
                        logger.Info("End OrderMaster: ");
                        return Request.CreateResponse(HttpStatusCode.OK, obj);
                    }

                }
                catch (Exception ex)
                {
                    logger.Error("Error in OrderMaster " + ex.Message);
                    logger.Info("End  OrderMaster: ");
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex.Message);
                }
            }
        }
        #region  Warehousebased Filter
        [Route("")]
        [HttpGet]
        public HttpResponseMessage Get(int list, int page, int WarehouseId)
        {
            logger.Info("start OrderMaster: ");
            PaggingData obj = new PaggingData();
            
            using (var context = new AuthContext())
            {
                var orderMasters = new List<OrderMaster>();
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
                        orderMasters = context.DbOrderMaster.Where(x => x.Deleted == false && (x.Status == "Pending" || x.Status == "Issued" || x.Status == "Shipped" || x.Status == "Delivery Redispatch" || x.Status == "Ready to Dispatch") && x.CompanyId == compid && x.WarehouseId == Warehouse_id).OrderByDescending(x => x.OrderId).Skip((page - 1) * list).Take(list).Include("orderDetails").ToList();
                        obj.total_count = context.DbOrderMaster.Where(x => x.Deleted == false && (x.Status == "Pending" || x.Status == "Issued" || x.Status == "Shipped" || x.Status == "Delivery Redispatch" || x.Status == "Ready to Dispatch") && x.CompanyId == compid && x.WarehouseId == Warehouse_id).Count();
                        foreach (var item in orderMasters)
                        {
                            item.SalesPerson = string.Join(",", item.orderDetails.Where(z => !string.IsNullOrEmpty(z.ExecutiveName)).Select(z => z.ExecutiveName).Distinct());

                        }
                        //var newdata = context.DbOrderMaster.Where(x => x.Deleted == false && x.Status == "Pending" && x.CompanyId == compid && x.WarehouseId == Warehouse_id).OrderByDescending(x => x.OrderId).Skip((page - 1) * list).Take(list).ToList();
                        //obj.total_count = context.DbOrderMaster.Where(x => x.Deleted == false && x.Status == "Pending" && x.CompanyId == compid && x.WarehouseId == Warehouse_id).Count();
                        obj.ordermaster = orderMasters;
                        logger.Info("End OrderMaster: ");
                        return Request.CreateResponse(HttpStatusCode.OK, obj);
                    }

                    else
                    {
                        logger.Info("User ID : {0} , Company Id : {1}", compid, userid);
                        orderMasters = context.DbOrderMaster.Where(x => x.Deleted == false && (x.Status == "Pending" || x.Status == "Issued" || x.Status == "Shipped" || x.Status == "Delivery Redispatch" || x.Status == "Ready to Dispatch") && x.CompanyId == compid).OrderByDescending(x => x.OrderId).Skip((page - 1) * list).Take(list).Include("orderDetails").ToList();
                        obj.total_count = context.DbOrderMaster.Where(x => x.Deleted == false && (x.Status == "Pending" || x.Status == "Issued" || x.Status == "Shipped" || x.Status == "Delivery Redispatch" || x.Status == "Ready to Dispatch") && x.CompanyId == compid).Count();
                        foreach (var item in orderMasters)
                        {
                            item.SalesPerson = string.Join(",", item.orderDetails.Where(z => !string.IsNullOrEmpty(z.ExecutiveName)).Select(z => z.ExecutiveName).Distinct());

                        }
                        //var newdata = context.DbOrderMaster.Where(x => x.Deleted == false && x.Status == "Pending" && x.CompanyId == compid).OrderByDescending(x => x.OrderId).Skip((page - 1) * list).Take(list).ToList();
                        //obj.total_count = context.DbOrderMaster.Where(x => x.Deleted == false && x.Status == "Pending" && x.CompanyId == compid).Count();
                        obj.ordermaster = orderMasters;
                        logger.Info("End OrderMaster: ");
                        return Request.CreateResponse(HttpStatusCode.OK, obj);
                    }

                }
                catch (Exception ex)
                {
                    logger.Error("Error in OrderMaster " + ex.Message);
                    logger.Info("End  OrderMaster: ");
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex.Message);
                }
            }
            #endregion
        }
    }
}