using AngularJSAuthentication.Model;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Web.Http;


namespace AngularJSAuthentication.API.Controllers
{
    [RoutePrefix("api/area")]
    public class AreaController : ApiController
    {

        private static Logger logger = LogManager.GetCurrentClassLogger();

        [Route("all")]
        public IEnumerable<Area> Get()
        {
            using (var context = new AuthContext())
            {
                logger.Info("start Warehouse: ");
                List<Area> ass = new List<Area>();
                try
                {
                    var identity = User.Identity as ClaimsIdentity;
                    int compid = 0, userid = 0, Warehouse_id = 0;

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

                    logger.Info("User ID : {0} , Company Id : {1}", compid, userid);
                    if (Warehouse_id > 0)
                    {
                        var city = context.Warehouses.Where(x => x.WarehouseId == Warehouse_id).SingleOrDefault();

                        ass = context.AreaDb.Where(x => x.Deleted == false && x.CityId == city.Cityid).ToList();
                    }
                    else
                    {
                        ass = context.AreaDb.Where(x => x.Deleted == false).ToList();
                    }


                    logger.Info("End  Cluster: ");
                    return ass;
                }
                catch (Exception ex)
                {
                    logger.Error("Error in Warehouse " + ex.Message);
                    logger.Info("End  Warehouse: ");
                    return null;
                }
            }
        }

        [Route("add")]
        [AcceptVerbs("POST")]
        public Area add(Area area)
        {
            using (var context = new AuthContext())
            {
                logger.Info("start cluster: ");
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

                    if (area == null)
                    {
                        throw new ArgumentNullException("area");
                    }
                    logger.Info("User ID : {0} , Company Id : {1}", compid, userid);
                    context.AddArea(area);
                    logger.Info("End  Cluster: ");
                    return area;
                }
                catch (Exception ex)
                {
                    logger.Error("Error in addQuesAns " + ex.Message);
                    logger.Info("End  AddCluster: ");
                    return null;
                }
            }
        }

        [Route("put")]
        [AcceptVerbs("PUT")]
        public Area Put(Area item)
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
                    return context.Putarea(item);
                }
                catch
                {
                    return null;
                }
            }
        }

        [Route("delete")]
        public bool delete(int id)
        {
            using (var context = new AuthContext())
            {
                logger.Info("start cluster: ");
                try
                {
                    context.DeleteArea(id);
                    logger.Info("End  Cluster: ");
                    return true;
                }
                catch (Exception ex)
                {
                    logger.Error("Error in addQuesAns " + ex.Message);
                    logger.Info("End  AddCluster: ");
                    return false;
                }
            }
        }
        #region Get City Id Based Area Data
        /// <summary>
        /// Created date 19/04/2019
        /// </summary>
        /// <param name="CityId"></param>
        /// <returns>ass</returns>

        [Route("GetArea")]
        public IEnumerable<Area> Getarea(int CityId)
        {
            using (var context = new AuthContext())
            {
                logger.Info("start Warehouse: ");
                List<Area> ass = new List<Area>();
                try
                {
                    var identity = User.Identity as ClaimsIdentity;
                    int compid = 0, userid = 0, Warehouse_id = 0;
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

                    logger.Info("User ID : {0} , Company Id : {1}", compid, userid);
                    if (Warehouse_id > 0)
                    {
                        ass = context.AreaDb.Where(x => x.Deleted == false && x.CityId == CityId).ToList();
                        return ass;
                    }
                    else
                    {
                        ass = context.AreaDb.Where(x => x.Deleted == false && x.CityId == CityId).ToList();
                    }
                    logger.Info("End  Cluster: ");
                    return ass;
                }
                catch (Exception ex)
                {
                    logger.Error("Error in Warehouse " + ex.Message);
                    logger.Info("End  Warehouse: ");
                    return null;
                }
            }
        }
        #endregion
    }
}



