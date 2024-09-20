using NLog;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Web.Http;

namespace AngularJSAuthentication.API.Controllers
{
    [RoutePrefix("api/CiMatrix")]
    public class CiMatrixController : ApiController
    {

        public static Logger logger = LogManager.GetCurrentClassLogger();
        [Authorize]
        [HttpGet]
        [Route("")]
        public HttpResponseMessage get(DateTime? start, DateTime? end)
        {
            using (AuthContext context = new AuthContext())
            {

                try
                {

                    var identity = User.Identity as ClaimsIdentity;
                    int compid = 0, WarehouseId = 0, userid = 0;

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
                            WarehouseId = int.Parse(claim.Value);
                        }
                    }
                    var DBoyorders = context.getCiMatrix(WarehouseId, start, end);
                    return Request.CreateResponse(HttpStatusCode.OK, DBoyorders);
                }
                catch (Exception ex)
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, ex.Message);
                }

            }
        }
    }
    public class CiMatrixDTOM
    {
        public int ItemId { get; set; }
        public double TotalSaleAmount { get; set; }
        public string ItemName { get; set; }
        public string ShopName { get; set; }
        public string Skcode { get; set; }
        public string ExcecutiveName { get; set; }

    }

    public class OrderDetailForCP
    {
        public double Price { get; set; }

        public int CustomerId { get; set; }

        public int ItemId { get; set; }

        public string SellingUnitName { get; set; }
    }

    public class CiMatrixDTOMCust
    {
        public List<CiMatrixDTOM> CtsList { get; set; }

    }
}