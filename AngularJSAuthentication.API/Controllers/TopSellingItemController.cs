using AngularJSAuthentication.Model;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Web.Http;

namespace AngularJSAuthentication.API.Controllers
{
    [AllowAnonymous]
    [RoutePrefix("api/TopSellingItem")]
    public class TopSellingItemController : ApiController
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        [Route("CustomerTopFiveItems/{customerId}")]
        [HttpGet]
        public IHttpActionResult CustomerTopFiveItem(int customerId)
        {
            using (AuthContext context = new AuthContext())
            {
                List<TopSellingItem> topSellingItemList = new List<TopSellingItem>();

                try
                {
                    var identity = User.Identity as ClaimsIdentity;
                    int compid, userid, Warehouse_id = 1;
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
                    var list = context.TopSellingItem
                           .Join(context.itemMasters, tsi => tsi.ItemId, im => im.ItemId, (tsi, im) => new { tsi, im })
                           .Where(x => x.tsi.CustomerId == customerId && x.tsi.WarehouseId == Warehouse_id)
                           .GroupBy(x => new { x.tsi.ItemId, x.im.itemname })
                           .Select(n => new
                           {
                               ItemID = n.Key.ItemId,
                               Name = n.Key.itemname,
                               Total = n.Sum(x => x.tsi.TotalItemPurchaseCount)
                           })
                           .OrderByDescending(x => x.Total)
                           .Take(5)
                           .ToList();


                    return Ok(list);
                }
                catch (Exception ex)
                {
                    logger.Error("Error in getting Peoples " + ex.Message);
                    return InternalServerError(new Exception("Something went wrong, please try later"));
                }
            }
        }

        [Route("TopFiveItems")]
        [HttpGet]
        public IHttpActionResult TopFiveItems()
        {
            using (AuthContext context = new AuthContext())
            {
                List<TopSellingItem> topSellingItemList = new List<TopSellingItem>();

                try
                {
                    var identity = User.Identity as ClaimsIdentity;
                    int compid, userid, Warehouse_id = 1;
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
                    var list = context.TopSellingItem
                           .Join(context.itemMasters, tsi => tsi.ItemId, im => im.ItemId, (tsi, im) => new { tsi, im })
                           .Where(x => x.tsi.WarehouseId == Warehouse_id)
                           .GroupBy(x => new { x.tsi.ItemId, x.im.itemname })
                           .Select(n => new
                           {
                               ItemID = n.Key.ItemId,
                               Name = n.Key.itemname,
                               Total = n.Sum(x => x.tsi.TotalItemPurchaseCount)
                           })
                           .OrderByDescending(x => x.Total)
                           .Take(5)
                           .ToList();


                    return Ok(list);
                }
                catch (Exception ex)
                {
                    logger.Error("Error in getting Peoples " + ex.Message);
                    return InternalServerError(new Exception("Something went wrong, please try later"));
                }
            }

        }
    }
}

