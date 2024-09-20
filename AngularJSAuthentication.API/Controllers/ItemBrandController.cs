using AngularJSAuthentication.Model;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Web.Http;
using System.Web.Http.Description;


namespace AngularJSAuthentication.API.Controllers
{
    [RoutePrefix("api/ItemBrand")]
    public class ItemBrandController : ApiController
    {
       
        private static Logger logger = LogManager.GetCurrentClassLogger();

        [Authorize]
        [Route("")]
        public IEnumerable<ItemBrand> Get()
        {
            using (var context = new AuthContext())
            {
                logger.Info("start ItemBrand: ");
                List<ItemBrand> ass = new List<ItemBrand>();
                try
                {
                    var identity = User.Identity as ClaimsIdentity;
                    int compid = 0, userid = 0;
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
                    }

                    logger.Info("User ID : {0} , Company Id : {1}", compid, userid);
                    ass = context.AllItemBrand(compid).ToList();
                    logger.Info("End  ItemBrand: ");
                    return ass;
                }
                catch (Exception ex)
                {
                    logger.Error("Error in ItemBrand " + ex.Message);
                    logger.Info("End  ItemBrand: ");
                    return null;
                }
            }
        }


        [ResponseType(typeof(ItemBrand))]
        [Route("")]
        [AcceptVerbs("POST")]
        public ItemBrand add(ItemBrand item)
        {
            using (var context = new AuthContext())
            {
                logger.Info("start ItemBrand: ");
                try
                {
                    var identity = User.Identity as ClaimsIdentity;
                    int compid = 0, userid = 0;
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
                    }
                    item.CompanyId = compid;
                    if (item == null)
                    {
                        throw new ArgumentNullException("item");
                    }
                    logger.Info("User ID : {0} , Company Id : {1}", compid, userid);
                    context.AddItemBrand(item);
                    logger.Info("End  ItemBrand: ");
                    return item;
                }
                catch (Exception ex)
                {
                    logger.Error("Error in AddItemBrand " + ex.Message);
                    logger.Info("End  AddItemBrand: ");
                    return null;
                }
            }
        }

        [ResponseType(typeof(ItemBrand))]
        [Route("")]
        [AcceptVerbs("PUT")]
        public ItemBrand Put(ItemBrand item)
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
                    logger.Info("User ID : {0} , Company Id : {1}", compid, userid);
                    return context.PutItemBrand(item);
                }
                catch
                {
                    return null;
                }
            }
        }


        [ResponseType(typeof(ItemBrand))]
        [Route("")]
        [AcceptVerbs("Delete")]
        public void Remove(int id)
        {
            using (var context = new AuthContext())
            {
                logger.Info("start delete ItemBrand: ");
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
                    logger.Info("User ID : {0} , Company Id : {1}", compid, userid);
                    context.DeleteItemBrand(id, compid);
                    logger.Info("End  delete ItemBrand: ");
                }
                catch (Exception ex)
                {

                    logger.Error("Error in delete ItemBrand " + ex.Message);


                }
            }
        }
    }
}



