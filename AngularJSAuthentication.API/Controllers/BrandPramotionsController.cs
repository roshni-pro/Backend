using AngularJSAuthentication.Model;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Web.Http;

namespace AngularJSAuthentication.API.Controllers
{
    [RoutePrefix("api/BrandPramotions")]
    public class BrandPramotionsController : ApiController
    {

        private static Logger logger = LogManager.GetCurrentClassLogger();

        [Route("")]
        public List<SubsubCategory> Getbywarehouse(int warehouseid)
        {
            //if (recordtype == "city")
            // {
            using (var context = new AuthContext())
            {
                logger.Info("start Category: ");
                List<SubsubCategory> ass = new List<SubsubCategory>();
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
                    ass = context.PramotionalBrand(warehouseid, compid).ToList();
                    logger.Info("End  WarehouseCategory: ");
                    //return ass;
                    return null;
                }
                catch (Exception ex)
                {
                    logger.Error("Error in WarehouseCategory " + ex.Message);
                    logger.Info("End  WarehouseCategory: ");
                    return null;
                }
            }
            // }
        }

        // removed by Harry : 21 May 2019
        [Route("Exclusivebrand")]
        public List<SubsubCategory> Get()
        {
            using (var context = new AuthContext())
            {
                logger.Info("start subcategorybyWarehouse: ");
                List<SubsubCategory> ass = new List<SubsubCategory>();
                try
                {

                    var identity = User.Identity as ClaimsIdentity;
                    int compid = 1, userid = 0;

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

                    //ass = context.subcategorybyPramotionExlusive().ToList();

                    logger.Info("End  subcategorybyWarehouse: ");
                    return ass;
                }
                catch (Exception ex)
                {
                    logger.Error("Error in subcategorybyWarehouse " + ex.Message);
                    logger.Info("End  subcategorybyWarehouse: ");
                    return null;
                }
            }
        }
    }
}
