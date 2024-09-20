using AngularJSAuthentication.Model;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Web.Http;

namespace AngularJSAuthentication.API.Controllers
{
    [RoutePrefix("api/favorites")]
    public class FavoritesController : ApiController
    {

        private static Logger logger = LogManager.GetCurrentClassLogger();

        // [Authorize]
        // [Route("")]
        [AcceptVerbs("POST")]
        public Favorites add(Favorites item)
        {
            logger.Info("start add Favorites: ");
            using (AuthContext context = new AuthContext())
            {
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
                    item.favoId = compid;
                    item.CompanyId = compid;
                    if (item == null)
                    {
                        throw new ArgumentNullException("item");
                    }
                    logger.Info("User ID : {0} , Company Id : {1}", compid, userid);
                    context.AddFavorites(item);
                    logger.Info("End add Favorites: ");
                    return item;
                }
                catch (Exception ex)
                {
                    logger.Error("Error in add Favorites " + ex.Message);
                    logger.Info("End  Favorites: ");
                    return null;
                }
            }
        }
        public List<Favorites> Get(string mob)
        {
            logger.Info("start Favorites: ");
            List<Favorites> ass = new List<Favorites>();
            using (AuthContext context = new AuthContext())
            {
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
                    ass = context.AllFavorites(mob, compid).ToList();
                    logger.Info("End  City: ");
                    return ass;
                }
                catch (Exception ex)
                {
                    logger.Error("Error in City " + ex.Message);
                    logger.Info("End  City: ");
                    return null;
                }
            }
        }

    }

}
