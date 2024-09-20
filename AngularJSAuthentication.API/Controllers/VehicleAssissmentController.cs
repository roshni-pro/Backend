using AgileObjects.AgileMapper;
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
    [RoutePrefix("api/vehicleassissment")]
    public class VehicleAssissmentControllerController : ApiController
    {

        private static Logger logger = LogManager.GetCurrentClassLogger();

        [Route("")]
        [HttpGet]
        public HttpResponseMessage Get(string ids, int DeliveryIssuanceId)//get all 
        {
            using (AuthContext context = new AuthContext())
            {
                List<DBoySummary> SUmmarylist = new List<DBoySummary>();
                int[] OrderIds = ids.Split(',').Select(n => Convert.ToInt32(n)).ToArray();
                if (OrderIds != null && OrderIds.Any() && DeliveryIssuanceId > 0)
                {
                    var OrderDeliveryMasters = context.OrderDeliveryMasterDB.Where(x => OrderIds.Contains(x.OrderId) && x.DeliveryIssuanceId == DeliveryIssuanceId).ToList();
                    SUmmarylist = Mapper.Map(OrderDeliveryMasters).ToANew<List<DBoySummary>>();
                }
                return Request.CreateResponse(HttpStatusCode.OK, SUmmarylist);
            }
        }

        [Route("")]
        [HttpGet]
        public HttpResponseMessage Getorders(string ids, int DeliveryIssuanceId, string test)//get all 
        {
            using (AuthContext context = new AuthContext())
            {
                List<OrderDispatchedMaster> Ordeersss = new List<OrderDispatchedMaster>();
                int[] OrderIds = ids.Split(',').Select(n => Convert.ToInt32(n)).ToArray();
                if (OrderIds != null && OrderIds.Any() && DeliveryIssuanceId > 0)
                {
                    Ordeersss = context.OrderDispatchedMasters.Where(x => OrderIds.Contains(x.OrderId)).Include("orderDetails").ToList();
                    var OrderDeliveryMasters = context.OrderDeliveryMasterDB.Where(x => OrderIds.Contains(x.OrderId) && x.DeliveryIssuanceId == DeliveryIssuanceId).ToList();
                    foreach (var Item in Ordeersss) 
                    {
                        Item.Status = OrderDeliveryMasters.FirstOrDefault(x => x.OrderId == Item.OrderId).Status;
                    }
                }
                return Request.CreateResponse(HttpStatusCode.OK, Ordeersss);
            }

        }
    }
}



