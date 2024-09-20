using AngularJSAuthentication.Model;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Web.Http;


namespace AngularJSAuthentication.API.Controllers
{
    [RoutePrefix("api/AssignBulkcustomers")]
    public class AssignBulkcustomersController : ApiController
    {

        private static Logger logger = LogManager.GetCurrentClassLogger();

        [Authorize]
        [Route("GetCustomer")]
        public IEnumerable<Customer> Get(int Warehouseid)
        {
            using (var context = new AuthContext())
            {
                logger.Info("start Warehouse: ");
                List<Customer> customer = new List<Customer>();
                List<Customer> CustomerUnmapped = new List<Customer>();
                try
                {

                    logger.Info("User ID : {0} , Company Id : {1}");

                    Warehouse wh = context.Warehouses.Where(x => x.WarehouseId == Warehouseid).SingleOrDefault();



                    var query = from c in context.Customers
                                where !(from o in context.Customers
                                        where o.CompanyId != c.CustomerId
                                        select o.CustomerId).Contains(c.CustomerId)
                                        && c.Deleted == false && c.Cityid == wh.Cityid && (c.Warehouseid == null || c.Warehouseid == 0)
                                select c;
                    return query;

                }
                catch (Exception ex)
                {
                    logger.Error("Error in Warehouse " + ex.Message);
                    logger.Info("End  Warehouse: ");
                    return null;
                }
            }
        }
        [Route("AssignCustomer")]
        [HttpPost]
        public HttpResponseMessage post(List<CustSupplier> obj)
        {
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
                    //if (Warehouse_id > 0)
                    //{
                    var CustSuppliers = context.addcustsuppliermappingForAll(obj, compid, Warehouse_id);
                    if (CustSuppliers == null)
                    {
                        return Request.CreateResponse(HttpStatusCode.BadRequest, "Error");
                    }
                    return Request.CreateResponse(HttpStatusCode.OK, CustSuppliers);
                    //}
                    //else
                    //{
                    //    return Request.CreateResponse(HttpStatusCode.BadRequest, "got error");
                    //}
                }
                catch (Exception ex)
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, ex.Message);
                }
            }
        }
    }
}



