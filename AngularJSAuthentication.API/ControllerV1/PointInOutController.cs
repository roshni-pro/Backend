using AngularJSAuthentication.Model;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Web.Http;

namespace AngularJSAuthentication.API.ControllerV1
{
    [RoutePrefix("api/PointInOut")]
    public class PointInOutController : ApiController
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        [Route("")]
        [AcceptVerbs("Get")]
        public HttpResponseMessage Get(DateTime start, DateTime end, int WarehouseId)
        {
            logger.Info("start WalletList: ");
            using (var context = new AuthContext())
            {
                try
                {
                    var identity = User.Identity as ClaimsIdentity;
                    int compid = 0, userid = 0;
                    int Warehouse_id = 0;
                    List<OrderMaster> data = new List<OrderMaster>();
                    List<int> CustomerId = new List<int>();
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

                    data = context.DbOrderMaster.Where(x => x.CreatedDate >= start && x.CreatedDate <= end && (x.walletPointUsed != null || x.walletPointUsed != '0') && x.WarehouseId == WarehouseId).ToList();

                    logger.Info("End  wallet: ");
                    return Request.CreateResponse(HttpStatusCode.OK, data);

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
