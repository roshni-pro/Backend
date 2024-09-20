using AngularJSAuthentication.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Web.Http;
using System.Web.Http.Description;

namespace AngularJSAuthentication.API.Controllers
{
    //AllItemPramotion

    [RoutePrefix("api/pramotion")]
    public class ItemPramotionController : ApiController
    {
       
        private static TimeZoneInfo INDIAN_ZONE = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
        DateTime indianTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE);

        [Route("")]
        public IEnumerable<ItemPramotions> Get()
        {

            using (var context = new AuthContext())
            {
                List<ItemPramotions> ass = new List<ItemPramotions>();
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


                        ass = context.AllItemPramotionWid(compid, Warehouse_id).ToList();

                        return ass;
                    }
                    else
                    {
                        ass = context.AllItemPramotion(compid).ToList();

                        return ass;
                    }


                }
                catch (Exception ex)
                {
                    //logger.Error("Error in Item Master " + ex.Message);
                    //logger.Info("End  Item Master: ");
                    return null;
                }
            }
        }

        [Route("")]
        public IEnumerable<ItemPramotions> Get(int id)
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

                    List<ItemPramotions> itempramotion = new List<ItemPramotions>();

                    itempramotion = (from ip in context.itempramotions where ip.WarehouseId == id && ip.CompanyId == compid && (ip.StartDate < indianTime && ip.EndDate > indianTime) select ip).ToList();
                    return itempramotion;
                }
                catch (Exception ex)
                {

                    return null;
                }

            }
        }


        [ResponseType(typeof(ItemPramotions))]
        [Route("")]
        [AcceptVerbs("POST")]
        public ItemMaster Post(ItemMaster item)
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
                    item.CompanyId = compid;

                    context.AddItemPramotion(item);
                    context.AddPramotion(item);
                    return item;
                }
                catch (Exception ex)
                {

                    return null;
                }
            }
        }

    }
}
