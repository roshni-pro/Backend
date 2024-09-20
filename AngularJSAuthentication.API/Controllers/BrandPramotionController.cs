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
    [RoutePrefix("api/BrandPramotion")]
    public class BrandPramotionController : ApiController
    {

        private static Logger logger = LogManager.GetCurrentClassLogger();
        [Route("")]
        public List<SubsubCategory> Get(int warehouse, int id)
        {
            using (var context = new AuthContext())
            {
                logger.Info("start subcategorybyWarehouse: ");
                List<SubsubCategory> ass = new List<SubsubCategory>();
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

                    ass = context.subcategorybyPramotion(warehouse, compid).ToList();
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

        [Authorize]
        [Route("")]
        public List<SubsubCategory> Get(string recordtype, int warehouse)
        {
            using (var context = new AuthContext())
            {
                if (recordtype == "warehouse")
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
                        ass = context.subcategorybyWarehouse(warehouse, compid).ToList();
                        logger.Info("End  WarehouseCategory: ");
                        return ass;
                    }
                    catch (Exception ex)
                    {
                        logger.Error("Error in WarehouseCategory " + ex.Message);
                        logger.Info("End  WarehouseCategory: ");

                    }
                }
            }
            return null;
        }

        [Authorize]
        [Route("")]
        public List<SubsubCategory> Getbycity(int city)
        {
            using (var context = new AuthContext())
            {
                logger.Info("start Category: ");
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

                    logger.Info("User ID : {0} , Company Id : {1}", compid, userid);
                    ass = context.subcategorybycity(city, compid).ToList();
                    logger.Info("End  WarehouseCategory: ");
                    return ass;
                }
                catch (Exception ex)
                {
                    logger.Error("Error in WarehouseCategory " + ex.Message);
                    logger.Info("End  WarehouseCategory: ");
                    return null;
                }
            }

        }
        [ResponseType(typeof(SubsubCategory))]
        [Route("")]
        [AcceptVerbs("PUT")]
        public List<SubsubCategory> Put(List<SubsubCategory> item)
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

                    logger.Info("User ID : {0} , Company Id : {1}", compid, userid);
                    return context.Updatebrands(item, compid);
                }
                catch
                {
                    return null;
                }
            }
        }


        [ResponseType(typeof(SubsubCategory))]
        [Route("PutExclusivebrand")]
        [AcceptVerbs("PUT")]
        public List<SubsubCategory> PutExclusivebrand(List<SubsubCategory> item)
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

                    logger.Info("User ID : {0} , Company Id : {1}", compid, userid);
                    return context.UpdateExclusivebrands(item, compid);
                }
                catch
                {
                    return null;
                }
            }
        }



    }
}
