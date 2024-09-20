using AngularJSAuthentication.Common.Helpers;
using AngularJSAuthentication.Model;
using Nito.AsyncEx;
using NLog;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Entity;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Web;
using System.Web.Http;

namespace AngularJSAuthentication.API.ControllerV1
{
    [RoutePrefix("api/SellerOrderStock")]
    public class SellerOrderStockController : ApiController
    {

        public static Logger logger = LogManager.GetCurrentClassLogger();

        [Route("Sync")]
        [HttpGet]
        public HttpResponseMessage Sync()
        {
            using (var db = new AuthContext())
            {

                List<SellerOrderDelivered> ChangeOrderLists = db.SellerOrderDelivereds.Where(x => x.IsProcessed == false).ToList();
                List<PostOrderDc> PostOrders = new List<PostOrderDc>();
                foreach (var ord in ChangeOrderLists)
                {
                    ord.IsProcessed = true;
                    ord.UpdatedDate = DateTime.Now;
                    db.Entry(ord).State = EntityState.Modified;
                    PostOrderDc item = new PostOrderDc
                    {
                        OrderId = ord.DispatchDetailId
                    };
                    PostOrders.Add(item);
                }
                var tradeUrl = ConfigurationManager.AppSettings["TradeURL"] + "/api/Bid/stock";
                using (GenericRestHttpClient<List<PostOrderDc>, string> memberClient = new GenericRestHttpClient<List<PostOrderDc>, string>(tradeUrl, "", null))
                {
                    var traderesult = AsyncContext.Run(() => memberClient.PostAsync<string>(PostOrders));
                    if (traderesult != null)
                    {
                        db.Commit();
                    }
                }
                return Request.CreateResponse(HttpStatusCode.OK, true);
            }
        }

        public class PostOrderDc
        {
            public int OrderId { get; set; }

        }

    }


}
