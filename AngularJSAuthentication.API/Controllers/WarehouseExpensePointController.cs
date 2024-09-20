using NLog;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Web.Http;

namespace AngularJSAuthentication.API.Controllers
{
    [RoutePrefix("api/WarehouseExpensePoint")]
    public class WarehouseExpensePointController : ApiController
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        //[Authorize]
        [Route("")]
        public HttpResponseMessage Get()
        {
            logger.Info("start WalletList: ");
            using (AuthContext context = new AuthContext())
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

                        var pointList = context.WarehousePointDB.Where(x => x.WarehouseId == Warehouse_id).ToList();
                        logger.Info("End  wallet: ");
                        return Request.CreateResponse(HttpStatusCode.OK, pointList);
                    }
                    else
                    {
                        var pointList = context.WarehousePointDB.Where(x => x.CompanyId == compid).ToList();
                        logger.Info("End  wallet: ");
                        return Request.CreateResponse(HttpStatusCode.OK, pointList);
                    }

                }
                catch (Exception ex)
                {
                    logger.Error("Error in WalletList " + ex.Message);
                    logger.Info("End  WalletList: ");
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex.Message);
                }
            }
        }
    }
}
