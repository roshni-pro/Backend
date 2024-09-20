using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Web.Http;

namespace AngularJSAuthentication.API.ControllerV1
{
    [RoutePrefix("api/Redispatch")]
    public class RedispatchController : ApiController
    {
        [Route("")]
        [HttpGet]
        public HttpResponseMessage get() //get orders for Redispatch 
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
                    if (Warehouse_id == 0)
                    {
                        var RDorders = context.OrderDispatchedMasters.Where(x => x.Status == "Delivery Redispatch").ToList();
                        return Request.CreateResponse(HttpStatusCode.OK, RDorders);
                    }
                    else
                    {
                        var RDorders = context.OrderDispatchedMasters.Where(x => x.Status == "Delivery Redispatch" && x.WarehouseId == Warehouse_id).ToList();
                        return Request.CreateResponse(HttpStatusCode.OK, RDorders);
                    }


                }
                catch (Exception ex)
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, ex.Message);
                }
            }
        }
        #region get data Warehouse based
        [Route("getwarehouseId")]
        [HttpGet]
        public HttpResponseMessage getwarehouseId(int WarehouseId) //get orders for Redispatch 
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
                    Warehouse_id = WarehouseId;
                    if (Warehouse_id > 0)
                    {
                        var RDorders = context.OrderDispatchedMasters.Where(x => x.Status == "Delivery Redispatch" && x.WarehouseId == Warehouse_id).ToList();
                        return Request.CreateResponse(HttpStatusCode.OK, RDorders);
                    }
                    else
                    {
                        return null;
                    }


                }
                catch (Exception ex)
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, ex.Message);
                }
            }
        }
        #endregion
        [Route("")]
        [HttpGet]
        public HttpResponseMessage getbyDboy(string mob) //get orders Redispatch by DBoy mob
        {
            using (var context = new AuthContext())
            {
                try
                {
                    // var RDorders = context.OrderDispatchedMasters.Where(x =>x.Status == "Delivery Redispatch" && x.DboyMobileNo == mob).ToList();
                    var RDorders = context.getRedispatchordersbyboy(mob);

                    return Request.CreateResponse(HttpStatusCode.OK, RDorders);
                }
                catch (Exception ex)
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, ex.Message);
                }
            }
        }
        [Route("auto")]
        [HttpGet]
        public HttpResponseMessage getredispatachcedauto(string mob) //get orders Redispatch by DBoy mob
        {
            using (var context = new AuthContext())
            {
                try
                {
                    var RDorders = context.OrderDispatchedMasters.Where(x => x.Status == "Delivery Canceled" && x.DboyMobileNo == mob && x.ReDispatchCount >= 3).ToList();

                    return Request.CreateResponse(HttpStatusCode.OK, RDorders);
                }
                catch (Exception ex)
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, ex.Message);
                }
            }
        }


        [Route("auto1")]
        [HttpGet]
        public HttpResponseMessage getredispatachcedauto1(string mob, int WarehouseId) //get orders Redispatch by 
        {
            using (var context = new AuthContext())
            {
                try
                {
                    var RDorders = context.OrderDispatchedMasters.Where(x => x.Status == "Delivery Canceled" && x.DboyMobileNo == mob && x.ReDispatchCount >= 3 && x.WarehouseId == WarehouseId).ToList();

                    return Request.CreateResponse(HttpStatusCode.OK, RDorders);
                }
                catch (Exception ex)
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, ex.Message);
                }
            }
        }

    }
}
