

using AngularJSAuthentication.API.Controllers.External.Other;
using AngularJSAuthentication.API.Managers;
using AngularJSAuthentication.BusinessLayer.Managers.Masters;
using AngularJSAuthentication.DataContracts.Transaction.ClusterDashboard;
using AngularJSAuthentication.DataContracts.Transaction.MessageQueue;
using AngularJSAuthentication.Model;
using Nito.AspNetBackgroundTasks;
using NLog;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using static AngularJSAuthentication.API.Controllers.External.Other.SellerStoreController;

namespace AngularJSAuthentication.API.Controllers
{
    [RoutePrefix("api/cluster")]
    public class clusterController : ApiController
    {

        private static Logger logger = LogManager.GetCurrentClassLogger();
        private static TimeZoneInfo INDIAN_ZONE = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
        DateTime indianTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE);
        [Authorize]
        [Route("all")]
        public dynamic Get()
        {
            logger.Info("start Warehouse: ");

            List<Cluster> ass = new List<Cluster>();
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
                //ass = context.AllCluster(compid).ToList();
                //var ass1 = (from c in db.Clusters
                //            join a in db.Peoples on c.AgentCode equals a.PeopleID
                //            into ps
                //            from a in ps.DefaultIfEmpty()
                //            select new
                //            {

                //                ClusterId = c.ClusterId,
                //                ClusterName = c.ClusterName,
                //                DisplayName = a == null ? "" : a.DisplayName,
                //                WarehouseName = c.WarehouseName,
                //                CreatedDate = c.CreatedDate,


                //            }).ToList();

                //string sqlquery = "select a.CityId, a.ClusterId,a.WorkingCityName,a.ClusterName,a.WarehouseId,a.WarehouseName + ' - ' + a.CityName as WarehouseName,a.CreatedDate, isnull(STRING_AGG(agent.DisplayName,', '),'') DisplayName, (select count(d.customerid)  from Customers d where a.ClusterId = d.ClusterId) customercount, (select count(d.customerid)  from Customers d where a.ClusterId = d.ClusterId and d.Active=1 and d.Deleted=0) activecustomercount from Clusters a  Outer apply (" +
                //                  " Select p.DisplayName from clusteragents e  inner  join People p on e.AgentId=p.PeopleID and  e.ClusterId = a.ClusterId and e.active=1) agent " +
                //                  " where a.Active=1 and a.Deleted=0 group by a.ClusterId,a.ClusterName,a.WarehouseId,a.CreatedDate,a.WarehouseName,a.CityName,a.WorkingCityName, a.CityId ";

                string sqlquery = "exec GetClusterReport";

                List<ClustDTO> clustDTOs = new List<ClustDTO>();
                using (AuthContext context = new AuthContext())
                {
                    clustDTOs = context.Database.SqlQuery<ClustDTO>(sqlquery).ToList();
                    foreach (var item in clustDTOs)
                    {
                        if (!string.IsNullOrEmpty(item.DisplayName))
                            item.IsAgentadded = true;
                        else
                            item.IsAgentadded = false;

                        item.IsVehicleadded = context.ClusterVehicle.Any(x => x.ClusterId == item.ClusterId && x.active);
                        if (item.ExecutiveName != null)
                        {
                            List<string> lstExective = item.ExecutiveName.Split(',').ToList();
                            if (lstExective != null && lstExective.Any())
                                item.ExecutiveName = string.Join(",", lstExective.Distinct().ToList());
                        }
                        string query = "select v.OwnerName as DBoy from ClusterVehicles cv inner join Vehicles v on cv.VehicleID = v.VehicleId where cv.ClusterId = " + item.ClusterId;
                        var dboy = context.Database.SqlQuery<dboylist>(query).ToList();

                        //List<string> lstdboy = dboy.spl
                        if (dboy != null && dboy.Any())
                            item.dboyname = string.Join(",", dboy.Select(x => x.DBoy).ToList());
                    }

                }
                return clustDTOs;

            }
            catch (Exception ex)
            {
                logger.Error("Error in Warehouse " + ex.Message);
                logger.Info("End  Warehouse: ");
                return null;
            }
        }




        [Authorize]
        [Route("GetCitWise")]
        public dynamic GetCitWise(int cityId, int? active)
        {
            logger.Info("start Warehouse: ");

            List<Cluster> ass = new List<Cluster>();
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
                //ass = context.AllCluster(compid).ToList();
                //var ass1 = (from c in db.Clusters
                //            join a in db.Peoples on c.AgentCode equals a.PeopleID
                //            into ps
                //            from a in ps.DefaultIfEmpty()
                //            select new
                //            {

                //                ClusterId = c.ClusterId,
                //                ClusterName = c.ClusterName,
                //                DisplayName = a == null ? "" : a.DisplayName,
                //                WarehouseName = c.WarehouseName,
                //                CreatedDate = c.CreatedDate,


                //            }).ToList();

                string condition = active.HasValue ? " and a.Active=" + active.Value : "";

                string sqlquery = "exec GetClusterReportCityWise " + cityId;

                //string sqlquery = "select a.Active,a.CityId, a.ClusterId,a.WorkingCityName,a.ClusterName,a.WarehouseId,a.WarehouseName + ' - ' + a.CityName as WarehouseName,a.CreatedDate, isnull(STRING_AGG(agent.DisplayName,', '),'') DisplayName, (select count(d.customerid)  from Customers d where a.ClusterId = d.ClusterId and d.CityId='" + cityId + "') customercount, (select count(d.customerid)  from Customers d where a.ClusterId = d.ClusterId and d.Active=1 and d.Deleted=0 and d.CityId='" + cityId + "') activecustomercount from Clusters a  Outer apply (" +
                //                  " Select p.DisplayName from clusteragents e  inner  join People p on e.AgentId=p.PeopleID and  e.ClusterId = a.ClusterId and e.active=1) agent " +
                //                  " where a.CityId='" + cityId + "' " + condition + " and a.Deleted=0 group by a.ClusterId,a.ClusterName,a.WarehouseId,a.CreatedDate,a.WarehouseName,a.CityName,a.WorkingCityName, a.CityId, a.Active ";

                //string sqlquery = "select a.CityId, a.ClusterId,a.WorkingCityName,a.ClusterName,a.WarehouseId,a.WarehouseName + ' - ' + a.CityName as WarehouseName,a.CreatedDate, isnull(STRING_AGG(agent.DisplayName,', '),'') DisplayName, (select count(d.customerid)  from Customers d where a.ClusterId = d.ClusterId) customercount, (select count(d.customerid)  from Customers d where a.ClusterId = d.ClusterId and d.Active=1 and d.Deleted=0) activecustomercount from Clusters a  Outer apply (" +
                //                  " Select p.DisplayName from clusteragents e  inner  join People p on e.AgentId=p.PeopleID and  e.ClusterId = a.ClusterId and e.active=1) agent " +
                //                  " where a.Active=1 and a.Deleted=0 group by a.ClusterId,a.ClusterName,a.WarehouseId,a.CreatedDate,a.WarehouseName,a.CityName,a.WorkingCityName, a.CityId ";

                List<ClustDTO> clustDTOs = new List<ClustDTO>();
                using (AuthContext context = new AuthContext())
                {
                    clustDTOs = context.Database.SqlQuery<ClustDTO>(sqlquery).ToList();

                    if (active.HasValue)
                    {
                        clustDTOs = clustDTOs.Where(x => x.Active == Convert.ToBoolean(active)).ToList();
                    }

                    foreach (var item in clustDTOs)
                    {
                        if (!string.IsNullOrEmpty(item.DisplayName))
                            item.IsAgentadded = true;
                        else
                            item.IsAgentadded = false;

                        item.IsVehicleadded = context.ClusterVehicle.Any(x => x.ClusterId == item.ClusterId && x.active);

                        if (item.ExecutiveName != null)
                        {
                            List<string> lstExective = item.ExecutiveName.Split(',').ToList();
                            if (lstExective != null && lstExective.Any())
                                item.ExecutiveName = string.Join(",", lstExective.Distinct().ToList());
                        }


                        string query = "select v.OwnerName as DBoy from ClusterVehicles cv inner join Vehicles v on cv.VehicleID = v.VehicleId where cv.ClusterId = " + item.ClusterId;
                        var dboy = context.Database.SqlQuery<dboylist>(query).ToList();

                        if (dboy != null && dboy.Any())
                            item.dboyname = string.Join(",", dboy.Select(x => x.DBoy).ToList());
                    }

                }
                return clustDTOs;

            }
            catch (Exception ex)
            {
                logger.Error("Error in Warehouse " + ex.Message);
                logger.Info("End  Warehouse: ");
                return null;
            }
        }



        [Route("hubwise")]
        public IEnumerable<Cluster> Get(int WarehouseId)
        {
            logger.Info("start Warehouse: ");
            using (AuthContext db = new AuthContext())
            {
                List<Cluster> ass = new List<Cluster>();
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
                    ass = db.Clusters.Where(p => p.WarehouseId == WarehouseId && p.CompanyId == compid && p.Deleted == false).ToList();
                    logger.Info("End Cluster: ");
                    return ass;
                }
                catch (Exception ex)
                {
                    logger.Error("Error in Warehouse " + ex.Message);
                    logger.Info("End Warehouse: ");
                    return null;
                }
            }
        }

        #region get city  based cluster
        /// <summary>
        /// Created Date 19/04/2019
        /// created by Raj
        /// get data city based  cluster
        /// </summary>
        /// <param name="cityid"></param>
        /// <returns>ass</returns>
        [Route("Citybased")]
        public IEnumerable<Cluster> GetCitybased(int cityid)
        {
            logger.Info("start Warehouse: ");


            List<Cluster> ass = new List<Cluster>();
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

                using (var db = new AuthContext())
                {

                    ass = db.Clusters.Where(x => x.CityId == cityid && x.Deleted == false && x.Active == true).ToList();
                    var clusterIds = ass.Select(x => x.ClusterId).ToList();

                    var clusterAgents = clusterIds != null && clusterIds.Any() ? (from a in db.ClusterAgent
                                                                                  join p in db.Peoples on a.AgentId equals p.PeopleID
                                                                                  where !a.Deleted && a.active
                                                                                  && a.CompanyId == compid && clusterIds.Contains(a.ClusterId)
                                                                                  select new AgentDTO
                                                                                  {
                                                                                      AgentCode = p.AgentCode,
                                                                                      AgentName = p.DisplayName,
                                                                                      ClusterId = a.ClusterId
                                                                                  }).ToList() : new List<AgentDTO>();

                    ass.ForEach(x =>
                    {
                        x.Agents = clusterAgents.Any(z => z.ClusterId == x.ClusterId) ? clusterAgents.Where(z => z.ClusterId == x.ClusterId).ToList() : new List<AgentDTO>();
                    });

                }

                return ass;
            }
            catch (Exception ex)
            {
                logger.Error("Error in Warehouse " + ex.Message);
                logger.Info("End  Warehouse: ");
                return null;
            }
        }

        #endregion

        [Route("AgentsClusterBased")]
        [AcceptVerbs("GET")]
        public async Task<List<AgentDTO>> AgentsClusterBased(int clusterId)
        {
            using (AuthContext db = new AuthContext())
            {
                try
                {
                    var clusterAgents = await (from a in db.ClusterAgent
                                               join p in db.Peoples on a.AgentId equals p.PeopleID
                                               where !a.Deleted && a.active
                                               && a.ClusterId == clusterId
                                               select new AgentDTO
                                               {
                                                   AgentId = p.PeopleID,
                                                   AgentCode = p.AgentCode,
                                                   AgentName = p.DisplayName,
                                                   ClusterId = a.ClusterId
                                               }).ToListAsync();


                    return clusterAgents;
                }
                catch (Exception ex)
                {
                    logger.Error("Error in Warehouse " + ex.Message);
                    logger.Info("End  Warehouse: ");
                    return null;
                }
            }
        }

        #region
        //[HttpGet]
        //[Route("GetClusterWiseWareHouse")]
        //public dynamic GetClusterWiseCustomer(int clusterid)
        //{
        //    using (AuthContext db = new AuthContext())
        //    {
        //        var clusterwiseWarehouse = db.Clusters.Where(a => a.ClusterId == clusterid).ToList();

        //        select new AgentDTO
        //        {
        //            AgentCode = p.AgentCode,
        //            AgentName = p.DisplayName,
        //            ClusterId = a.ClusterId
        //        }).ToListAsync();



        //        return clusterwiseWarehouse;

        //    }
        //}
        [Route("GetClusterWiseWareHouse")]
        [AcceptVerbs("GET")]
        public async Task<List<WareHouseDTo>> ClusterWiseWarehouse(int warehouseid)
        {
            using (AuthContext db = new AuthContext())
            {
                try
                {
                    var clusterWarehouse = await (from a in db.Clusters
                                                      //join p in db.Peoples on a.AgentId equals p.PeopleID
                                                  where !a.Deleted && a.Active
                                                  && a.WarehouseId == warehouseid
                                                  select new WareHouseDTo
                                                  {
                                                      ClusterId = a.ClusterId,
                                                      ClusterName = a.ClusterName,
                                                      WarehouseId = a.WarehouseId
                                                  }).ToListAsync();


                    return clusterWarehouse;
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

        //Pravesh

        [ResponseType(typeof(Cluster))]
        [Route("add")]
        [AcceptVerbs("POST")]
        public HttpResponseMessage add(List<Cluster> items)
        {
            logger.Info("start cluster: ");
            using (AuthContext db = new AuthContext())
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
                    foreach (var item in items)
                    {
                        item.CompanyId = compid;
                        if (item == null)
                        {
                            throw new ArgumentNullException("item");
                        }
                        logger.Info("User ID : {0} , Company Id : {1}", compid, userid);
                        //context.Addcluster(item);

                        using (AuthContext context = new AuthContext())
                        {

                            if (item.LtLng != null && item.LtLng.Any())
                            {
                                var polygon = string.Join(",", item.LtLng.Select(x => x.latitude.ToString() + " " + x.longitude.ToString()).ToList()) + "," + item.LtLng.FirstOrDefault().latitude.ToString() + " " + item.LtLng.FirstOrDefault().longitude.ToString();
                                var sqlquery = "exec [dbo].[ValidateClusterPolygon] '" + polygon + "'";
                                var validPolygon = context.Database.SqlQuery<bool>(sqlquery).FirstOrDefault();

                                if (!validPolygon)
                                {
                                    var Polyres = new
                                    {
                                        status = false,
                                        Message = "Polygon Invalid please correct."
                                    };
                                    return Request.CreateResponse(HttpStatusCode.OK, Polyres);
                                }
                            }



                            int clustercount = context.Clusters.Where(x => x.CityId == item.CityId).Count();
                            item.ClusterName = item.ClusterName.Substring(0, 3) + (clustercount + 1).ToString();

                            if (item.WarehouseId > 0)
                            {
                                var wh = context.Warehouses.FirstOrDefault(x => x.WarehouseId == item.WarehouseId && x.CompanyId == item.CompanyId);
                                if (wh != null)
                                {
                                    item.WarehouseName = wh.WarehouseName;
                                }
                            }

                            item.CreatedDate = indianTime;
                            item.UpdatedDate = indianTime;
                            context.Clusters.Add(item);
                            int id = context.Commit();

                        }
                    }

                    if (items.Any(x => x.ClusterId > 0))
                    {
                        SellerStoreController sellerController = new SellerStoreController();
                        sellerController.AddClusterToSeller(items.Where(x => x.ClusterId > 0).ToList());
                    }

                    var res = new
                    {
                        status = true,
                        Message = "Successfully saved."
                    };
                    return Request.CreateResponse(HttpStatusCode.OK, res);
                }
                catch (Exception ex)
                {
                    logger.Error("Error in addQuesAns " + ex.Message);
                    logger.Info("End  AddCluster: ");
                    return Request.CreateResponse(HttpStatusCode.OK, new
                    {
                        status = false,
                        Message = "Some error occurred during save cluster."
                    });
                }
            }
        }


        [ResponseType(typeof(Cluster))]
        [Route("addV7")]
        [AcceptVerbs("POST")]
        public bool addV7(Cluster item)
        {
            logger.Info("start cluster: ");
            using (AuthContext db = new AuthContext())
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
                    //foreach (var item in items)
                    //{
                    item.CompanyId = compid;
                    if (item == null)
                    {
                        throw new ArgumentNullException("item");
                    }
                    logger.Info("User ID : {0} , Company Id : {1}", compid, userid);
                    //context.Addcluster(item);

                    using (AuthContext context = new AuthContext())
                    {
                        int clustercount = context.Clusters.Where(x => x.CityId == item.CityId).Count();
                        // item.ClusterName = item.ClusterName.Substring(0, 3) + (clustercount + 1).ToString();

                        if (item.WarehouseId > 0)
                        {
                            var wh = context.Warehouses.FirstOrDefault(x => x.WarehouseId == item.WarehouseId && x.CompanyId == item.CompanyId);
                            if (wh != null)
                            {
                                item.WarehouseName = wh.WarehouseName;
                            }
                        }

                        item.CreatedDate = indianTime;
                        item.UpdatedDate = indianTime;
                        context.Clusters.Add(item);
                        int id = context.Commit();
                    }
                    //}

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

        [ResponseType(typeof(GeoFence))]
        [Route("addgeofence")]
        [AcceptVerbs("POST")]
        public GeoFence addgeoFence(GeoFence geofence)
        {
            logger.Info("start googlemap: ");
            using (AuthContext context = new AuthContext())
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
                    geofence.CompanyId = compid;
                    if (geofence == null)
                    {
                        throw new ArgumentNullException("geofence");
                    }
                    logger.Info("User ID : {0} , Company Id : {1}", compid, userid);
                    context.AddGeoFence(geofence);
                    logger.Info("End  geofence: ");
                    return geofence;
                }
                catch (Exception ex)
                {
                    logger.Error("Error in addQuesAns " + ex.Message);
                    logger.Info("End  GoogleMap: ");
                    return null;
                }
            }
        }



        [ResponseType(typeof(Cluster))]
        [Route("update")]
        [AcceptVerbs("PUT")]
        public Cluster put(Cluster item)
        {
            logger.Info("start cluster: ");
            using (AuthContext context = new AuthContext())
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
                    if (item == null)
                    {
                        throw new ArgumentNullException("item");
                    }
                    logger.Info("User ID : {0} , Company Id : {1}", compid, userid);
                    context.UpdateCluster(item);

                    if (item.Active)
                    {
                        SellerStoreController sellerController = new SellerStoreController();
                        sellerController.UpdateClusterToSeller(item);
                    }
                    else
                    {
                        SellerStoreController sellerController = new SellerStoreController();
                        sellerController.InActiveClusterToSeller(item.ClusterId);
                    }
                    return item;
                }
                catch (Exception ex)
                {
                    logger.Error("Error in addQuesAns " + ex.Message);
                    logger.Info("End  AddCluster: ");
                    return null;
                }
            }
        }

        //Pravesh: Get Coordinate to create polygons in Cluster.

        [HttpGet]
        [Route("GetCoordinate")]
        public dynamic GetCoordinate(int clstid)
        {
            using (AuthContext db = new AuthContext())
            {
                Cluster c = db.Clusters.Include("LtLng").Where(a => a.ClusterId == clstid).FirstOrDefault();


                foreach (LtLng l in c.LtLng)
                {

                }
                return c.LtLng;
            }
        }


        [HttpGet]
        [Route("GetClusterWiseCustomer")]
        public dynamic GetClusterWiseCustomer(int clstid, int cityid)
        {
            using (AuthContext db = new AuthContext())
            {
                var clusterwisecust = db.Customers.Where(a => a.ClusterId == clstid && a.Cityid == cityid).ToList();
                return clusterwisecust;

            }
        }


        [HttpGet]
        [Route("GetClusterWiseCustomerIndividual")]
        public dynamic GetClusterWiseCustomerIndividual(int clustid)
        {
            List<ClustCustomerDTO> customers = new List<ClustCustomerDTO>();
            using (AuthContext context = new AuthContext())
            {
                //string sqlquery = "select a.* from (select CustomerId,lat,lg,Warehouseid,City,Skcode,ShopName from Customers where ClusterId is null and lat is not null and lg is not null and lat<>0 and lg<>0 and (lat >= -90 and lat <= 90) and (lg >= -180 and lg <= 180) and lg<90) a " +
                //                  "where [dbo].[GetClusterFromLatLng](a.lat, a.lg) = " + clustid;
                string sqlquery = "select CustomerId,lat,lg,Warehouseid,City,Skcode,ShopName from Customers where ClusterId=" + clustid;
                customers = context.Database.SqlQuery<ClustCustomerDTO>(sqlquery).ToList();

            }
            return customers;

            //var clusterwisecust = db.Customers.Where(a => a.ClusterId == clustid && a.Active ==true).Select(x=> new { x.lat ,x.lg,x.ShopName,x.Skcode }).ToList();
            //return clusterwisecust;

            //var query = "select CustomerId,lat,lg,Warehouseid,City from customers where customerid in (153,154)";

            //var res = db.Database.SqlQuery<object>(query).ToList();

            //return res;  

        }



        //Pravesh: Get Map city wise in cluster.
        [HttpGet]
        [Route("GetCentreLtLg")]
        public dynamic GetCentreLtLg(int clustid)
        {
            using (AuthContext db = new AuthContext())
            {

                var cty = (from c in db.Clusters
                           join a in db.Cities on c.CityId equals a.Cityid
                           where c.ClusterId == clustid
                           select new
                           {
                               CityLatitude = a.CityLatitude,
                               CityLongitude = a.CityLongitude
                           }).SingleOrDefault();

                return cty;
            }
        }

        [HttpGet]
        [Route("GetMapCityWise")]
        public dynamic GetMapCityWise(int cityid)
        {
            using (AuthContext db = new AuthContext())
            {
                List<Cluster> c = db.Clusters.Include("LtLng").Where(a => a.CityId == cityid).ToList();
                City cty = db.Cities.Where(a => a.Cityid == cityid).SingleOrDefault();
                ArrayList arryList1 = new ArrayList();
                arryList1.Add(cty);
                foreach (var d in c)
                {
                    foreach (var l in d.LtLng)
                    {
                        if (l.CityId == cityid)
                        {
                            arryList1.Add(l);


                        }

                    }
                }
                return arryList1;
            }
        }

        [HttpGet]
        [Route("GetClusterInfo")]
        public dynamic GetClusterInfo(int clustrId)
        {
            using (AuthContext db = new AuthContext())
            {
                var getClstInfo = db.Clusters.Where(x => x.ClusterId == clustrId).SingleOrDefault();
                return getClstInfo;

            }
        }

        [HttpGet]
        [Route("GetCustomerLatLong")]
        public dynamic GetCustomerLatLong(int cityid)
        {
            using (AuthContext db = new AuthContext())
            {
                var lstcustLatLong = db.Customers.Where(a => a.Cityid == cityid && a.lat != 0 && a.lg != 0).Select(x => new { x.lat, x.lg, x.Active, x.Skcode, x.Name, x.ShopName, x.ShippingAddress }).ToList();

                return lstcustLatLong;
            }
        }


        [HttpGet]
        [Route("GetwarehouseLatLong")]
        public dynamic GetwarehouseLatLong(int cityid)
        {
            using (AuthContext db = new AuthContext())
            {
                var lstwaretLatLong = db.Warehouses.Where(a => a.Cityid == cityid && a.latitude != 0 && a.longitude != 0).ToList();

                return lstwaretLatLong;
            }
        }


        //Pravesh : Get Max Value from db for creating cluster in Add Cluster.
        [HttpGet]
        [Route("GetMax")]
        public dynamic GetMax(int cid)
        {
            using (AuthContext db = new AuthContext())
            {
                int? a;
                var data = (from i in db.Clusters
                            where i.CityId == cid
                            select (int?)i.ClusterId).Max();
                if (data == null)
                {
                    a = 1;

                }
                else
                {
                    a = data + 1;
                }


                return a;
            }
        }

        [ResponseType(typeof(Cluster))]
        [Route("delete")]


        [AcceptVerbs("PUT")]
        public bool delete(int id)
        {
            logger.Info("start cluster: ");

            using (AuthContext context = new AuthContext())
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
                    int CompanyId = compid;
                    if (CompanyId == 0)
                    {
                        throw new ArgumentNullException("item");
                    }
                    context.DeleteCluster(id, CompanyId);
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


        [Route("GetVehicles")]
        [HttpGet]


        public dynamic GetVehicles(int clusterId, int warehouseId)
        {
            try
            {
                using (AuthContext db = new AuthContext())
                {
                    var GetVehicle = (from c in db.VehicleDb.Where(x => x.isActive == true && x.WarehouseId == warehouseId)
                                      join p in db.ClusterVehicle.Where(x => x.ClusterId == clusterId && x.active)
                                      on c.VehicleId equals p.VehicleID into ps
                                      from p in ps.DefaultIfEmpty()
                                      select new
                                      {
                                          id = c.VehicleId,
                                          label = c.VehicleName,
                                          Selected = p == null ? false : true
                                      }).ToList();

                    return GetVehicle;

                }

            }


            catch (Exception ex)
            {
                logger.Error("Error in Customer " + ex.ToString());
                logger.Info("End  Customer: ");
                return 0;
            }

        }


        //public int ClusterAgentID { get; set; }
        //public int ClusterId { get; set; }
        //public int AgentId { get; set; }
        //public bool active { get; set; }
        //public int CompanyId { get; set; }
        //public DateTime CreatedDate { get; set; }
        //public DateTime UpdatedDate { get; set; }
        //public bool Deleted { get; set; }

        [ResponseType(typeof(ClusterAgent))]
        [Route("addAgentCluster")]
        [AcceptVerbs("POST")]
        public bool addAgentCluster(List<ClusterAgent> cla)
        {
            bool result = false;
            logger.Info("start addAgentCluster: ");
            try
            {
                using (AuthContext context = new AuthContext())
                {
                    List<int> existagentid = new List<int>();
                    //var agentids = cla.Select(x => x.AgentId).ToList();

                    var clusterid = cla.FirstOrDefault().ClusterId;
                    var items = context.ClusterAgent.Where(x => x.ClusterId == clusterid).ToList();
                    foreach (var item in items)
                    {
                        if (cla.Any(x => x.AgentId == item.AgentId))
                        {
                            existagentid.Add(item.AgentId);
                            item.active = true;
                            item.Deleted = false;
                            item.UpdatedDate = indianTime;
                        }
                        else
                        {
                            item.active = false;
                            item.Deleted = true;
                            item.UpdatedDate = indianTime;
                        }
                        //context.ClusterAgent.Attach(item);
                        context.Entry(item).State = System.Data.Entity.EntityState.Modified;
                    }

                    foreach (var item in cla.Where(x => !existagentid.Any(y => y == x.AgentId)))
                    {
                        item.active = true;
                        item.Deleted = false;
                        item.UpdatedDate = indianTime;
                        item.CreatedDate = indianTime;
                        context.ClusterAgent.Add(item);
                    }

                    result = context.Commit() > 0;

                }
                return result;
            }
            catch (Exception ex)
            {
                logger.Error("Error in addAgentCluster " + ex.Message);
                logger.Info("End  addAgentCluster: ");
                return false;
            }
        }

        [ResponseType(typeof(ClusterAgent))]
        [Route("addAgentClusterV7")]
        [AcceptVerbs("POST")]
        public List<ClusterAgent> addAgentClusterV7(List<ClusterAgent> cla)
        {
            bool result = false;
            logger.Info("start addAgentCluster: ");
            try
            {
                using (AuthContext context = new AuthContext())
                {
                    List<int> existagentid = new List<int>();
                    //var agentids = cla.Select(x => x.AgentId).ToList();
                    var clusterid = cla.FirstOrDefault().ClusterId;
                    var items = context.ClusterAgent.Where(x => x.ClusterId == clusterid).ToList();
                    foreach (var item in items)
                    {
                        if (cla.Any(x => x.AgentId == item.AgentId))
                        {
                            existagentid.Add(item.AgentId);
                            item.active = true;
                            item.Deleted = false;
                            item.UpdatedDate = indianTime;
                        }
                        else
                        {
                            item.active = false;
                            item.Deleted = true;
                            item.UpdatedDate = indianTime;
                        }
                        //context.ClusterAgent.Attach(item);
                        context.Entry(item).State = System.Data.Entity.EntityState.Modified;
                    }

                    foreach (var item in cla.Where(x => !existagentid.Any(y => y == x.AgentId)))
                    {
                        item.active = true;
                        item.Deleted = false;
                        item.UpdatedDate = indianTime;
                        item.CreatedDate = indianTime;
                        context.ClusterAgent.Add(item);
                    }

                    result = context.Commit() > 0;
                }
                return cla;
            }
            catch (Exception ex)
            {
                logger.Error("Error in addAgentCluster " + ex.Message);
                logger.Info("End  addAgentCluster: ");
                return null;
            }
        }

        [ResponseType(typeof(ClusterVehicle))]
        [Route("addClusterVehicle")]
        [AcceptVerbs("POST")]
        public bool addClusterVehicle(List<ClusterVehicle> clv)
        {
            logger.Info("start addClusterVehicle: ");
            bool result = false;
            try
            {
                using (AuthContext context = new AuthContext())
                {
                    List<int> existVehicleid = new List<int>();

                    var clusterid = clv.FirstOrDefault().ClusterId;
                    var items = context.ClusterVehicle.Where(x => x.ClusterId == clusterid).ToList();
                    foreach (var item in items)
                    {
                        if (clv.Any(x => x.VehicleID == item.VehicleID))
                        {
                            existVehicleid.Add(item.VehicleID);
                            item.active = true;
                            item.Deleted = false;
                            item.UpdatedDate = indianTime;
                        }
                        else
                        {
                            item.active = false;
                            item.Deleted = true;
                            item.UpdatedDate = indianTime;
                        }
                        context.ClusterVehicle.Attach(item);
                        context.Entry(item).State = System.Data.Entity.EntityState.Modified;
                    }

                    foreach (var item in clv.Where(x => !existVehicleid.Any(y => y == x.VehicleID)))
                    {
                        item.active = true;
                        item.Deleted = false;
                        item.UpdatedDate = indianTime;
                        item.CreatedDate = indianTime;
                        context.ClusterVehicle.Add(item);
                    }

                    result = context.Commit() > 0;
                }
                return result;
            }
            catch (Exception ex)
            {
                logger.Error("Error in addClusterVehicle " + ex.Message);
                logger.Info("End  addClusterVehicle: ");
                return false;
            }
        }
        [ResponseType(typeof(ClusterVehicle))]
        [Route("addClusterVehicleV1")]
        [AcceptVerbs("POST")]
        public bool addClusterVehiclev1(List<ClusterPeopleDc> clv)
        {
            logger.Info("start addClusterVehicle: ");
            bool result = false;
            try
            {
                using (AuthContext context = new AuthContext())
                {
                    List<int> existVehicleid = new List<int>();

                    var clusterid = clv.FirstOrDefault().ClusterID;
                    var items = context.ClusterVehicle.Where(x => x.ClusterId == clusterid).ToList();
                    foreach (var item in items)
                    {
                        if (clv.Any(x => x.id == item.VehicleID))
                        {
                            existVehicleid.Add(item.VehicleID);
                            item.active = true;
                            item.Deleted = false;
                            item.UpdatedDate = indianTime;
                        }
                        else
                        {
                            item.active = false;
                            item.Deleted = true;
                            item.UpdatedDate = indianTime;
                        }
                        context.ClusterVehicle.Attach(item);
                        context.Entry(item).State = System.Data.Entity.EntityState.Modified;
                    }

                    foreach (var item in clv.Where(x => !existVehicleid.Any(y => y == x.id)))
                    {
                        ClusterVehicle clusterVehicle = new ClusterVehicle();
                        clusterVehicle.VehicleID = item.id;
                        clusterVehicle.ClusterId = clusterid;
                        clusterVehicle.CreatedDate = DateTime.Now;
                        clusterVehicle.UpdatedDate = DateTime.Now;
                        clusterVehicle.Deleted = false;
                        clusterVehicle.active = true;
                        context.ClusterVehicle.Add(clusterVehicle);
                        context.Commit();
                    }

                    result = context.Commit() > 0;
                }
                return result;
            }
            catch (Exception ex)
            {
                logger.Error("Error in addClusterVehicle " + ex.Message);
                logger.Info("End  addClusterVehicle: ");
                return false;
            }
        }

        public dynamic GetPeoples(int clusterId, int warehouseId)
        {
            try
            {
                using (AuthContext db = new AuthContext())
                {

                    //clusterid
                    //var GetPeople = db.Peoples.Where(x => x.Active == true).Select(x => new { x.PeopleID, x.DisplayName }).ToList();
                    string query = "select distinct p.PeopleID id,p.DisplayName label,cast(case when c.ClusterId is not null then 1 else 0 end as bit) Selected from People p inner join AspNetUsers u on p.Email=u.Email inner join AspNetUserRoles ur on u.Id=ur.UserId inner join AspNetRoles r on ur.RoleId=r.Id  left join  ClusterAgents c on p.PeopleID =c.AgentId and  c.ClusterId=" + clusterId + "  and c.active=1 and c.Deleted=0 where p.WarehouseId = " + warehouseId + " and  r.Name='Agent' and ur.isActive=1 and p.Active=1 and p.Deleted=0 ";

                    var GetPeople = db.Database.SqlQuery<ClusterPeopleDc>(query).ToList().OrderBy(x => x.label).ToList();

                    //var GetPeople = (from c in db.Peoples.Where(x => x.Active == true && x.WarehouseId == warehouseId && x.Type == "Agent")
                    //                 join p in db.ClusterAgent.Where(x => x.ClusterId == clusterId && x.active && !x.Deleted)
                    //                 on c.PeopleID equals p.AgentId into ps
                    //                 from p in ps.DefaultIfEmpty()
                    //                 select new
                    //                 {
                    //                     id = c.PeopleID,
                    //                     label = c.DisplayName,
                    //                     Selected = p == null ? false : true
                    //                 }).ToList();


                    return GetPeople;

                }

            }


            catch (Exception ex)
            {
                logger.Error("Error in Customer " + ex.ToString());
                logger.Info("End  Customer: ");
                return 0;
            }

        }

        [HttpGet]
        [Route("GetCityLatLong")]
        public dynamic GetCityLatLong(int cid)
        {
            using (AuthContext db = new AuthContext())
            {

                var data = db.Cities.Where(x => x.Cityid == cid).Select(x => new { x.CityLatitude, x.CityLongitude }).SingleOrDefault();

                return data;
            }
        }


        [HttpGet]
        [Route("GetWarehouseByCity")]
        public dynamic GetWarehouseByCity(int cid)
        {
            using (AuthContext db = new AuthContext())
            {

                var dataW = db.Warehouses.Where(x => x.Cityid == cid && x.active == true).ToList();

                return dataW;
            }
        }

        [HttpGet]
        [Route("GetClusterCityWise")]
        public dynamic GetClusterCityWise(int cityid)
        {
            using (AuthContext db = new AuthContext())
            {
                List<Cluster> c = db.Clusters.Include("LtLng").Where(a => a.CityId == cityid).ToList();
                City cty = db.Cities.Where(a => a.Cityid == cityid).SingleOrDefault();
                var warehouse = db.Warehouses.Where(x => x.Cityid == cityid && x.active == true && x.Deleted == false).Select(x => new
                {
                    x.WarehouseId,
                    x.IsStore
                }).ToList();
                return new
                {
                    city = cty,
                    clusters = c.Select(x =>
                  new
                  {

                      x.ClusterId,
                      x.ClusterName,
                      clusterlatlng = x.LtLng.Select(y => new { lat = y.latitude, lng = y.longitude }).ToList(),
                      IsStore = warehouse != null && warehouse.Any(y => y.WarehouseId == x.WarehouseId) ? warehouse.FirstOrDefault(y => y.WarehouseId == x.WarehouseId).IsStore : false
                  }).ToList()
                };
            }
        }
        [HttpPost]
        [Route("updatepolygon")]
        public HttpResponseMessage updatepolygon(Clusterpolygon clusterpolygon)
        {
            bool result = false;
            using (AuthContext mycontext = new AuthContext())
            {
                string polygonname = "";
                var cluster = mycontext.Clusters.Include("LtLng").FirstOrDefault(a => a.ClusterId == clusterpolygon.ClusterId);

                if (clusterpolygon.polygon != null && clusterpolygon.polygon.Length > 0)
                {

                    var polygon = string.Join(",", clusterpolygon.polygon.Select(x => x[0].ToString() + " " + x[1].ToString())) + "," + clusterpolygon.polygon.FirstOrDefault()[0].ToString() + " " + clusterpolygon.polygon.FirstOrDefault()[1].ToString();
                    var sqlquery = "exec [dbo].[ValidateClusterPolygon] '" + polygon + "'";
                    var validPolygon = mycontext.Database.SqlQuery<bool>(sqlquery).FirstOrDefault();
                    if (!validPolygon)
                    {
                        //Polygon Invalid please correct. 
                        var Polyres = new
                        {
                            status = false,
                            Message = "Polygon Invalid please correct."
                        };
                        return Request.CreateResponse(HttpStatusCode.OK, Polyres);
                    }
                }

                if (cluster != null)
                {
                    if (cluster.LtLng != null && cluster.LtLng.Any())
                    {
                        polygonname = cluster.LtLng.FirstOrDefault().polygon;
                        foreach (var item in cluster.LtLng.ToList())
                        {
                            mycontext.Entry(item).State = EntityState.Deleted;
                        }
                    }
                    if (clusterpolygon.polygon != null && clusterpolygon.polygon.Length > 0)
                    {
                        foreach (var item in clusterpolygon.polygon)
                        {
                            cluster.LtLng.Add(new LtLng
                            {
                                CityId = cluster.CityId,
                                latitude = item[0],
                                longitude = item[1],
                                polygon = polygonname
                            });
                        }
                    }
                    cluster.UpdatedDate = indianTime;
                    mycontext.Entry(cluster).State = EntityState.Modified;
                    result = mycontext.Commit() > 0;

                }
            }
            var res = new
            {
                status = result,
                Message = "Successfully Updated."
            };
            return Request.CreateResponse(HttpStatusCode.OK, res);
        }

        [HttpPost]
        [Route("updatepolygonList")]
        public HttpResponseMessage updatepolygonList(List<Clusterpolygon> clusterpolygonlist)
        {
            bool result = false;
            using (AuthContext mycontext = new AuthContext())
            {
                bool polygonvalid = true;
                string invalidCluster = "";
                foreach (var clusterpolygon in clusterpolygonlist)
                {
                    if (clusterpolygon.polygon != null && clusterpolygon.polygon.Length > 0)
                    {
                        var polygon = string.Join(",", clusterpolygon.polygon.Select(x => x[0].ToString() + " " + x[1].ToString())) + "," + clusterpolygon.polygon.FirstOrDefault()[0].ToString() + " " + clusterpolygon.polygon.FirstOrDefault()[1].ToString();
                        var sqlquery = "exec [dbo].[ValidateClusterPolygon] '" + polygon + "'";
                        var validPolygon = mycontext.Database.SqlQuery<bool>(sqlquery).FirstOrDefault();
                        if (!validPolygon)
                        {
                            polygonvalid = false;
                            invalidCluster += string.IsNullOrEmpty(invalidCluster) ? invalidCluster : "," + invalidCluster;
                        }
                    }
                }

                if (!polygonvalid)
                {
                    var Polyres = new
                    {
                        status = false,
                        Message = "Polygon Invalid for cluster " + invalidCluster + " Please correct"
                    };
                    return Request.CreateResponse(HttpStatusCode.OK, Polyres);
                }

                foreach (var clusterpolygon in clusterpolygonlist)
                {
                    string polygonname = "";
                    var cluster = mycontext.Clusters.Include("LtLng").FirstOrDefault(a => a.ClusterId == clusterpolygon.ClusterId);

                    if (cluster != null)
                    {
                        if (cluster.LtLng != null && cluster.LtLng.Any())
                        {
                            polygonname = cluster.LtLng.FirstOrDefault().polygon;
                            foreach (var item in cluster.LtLng.ToList())
                            {
                                mycontext.Entry(item).State = EntityState.Deleted;
                            }
                        }
                        if (clusterpolygon.polygon != null && clusterpolygon.polygon.Length > 0)
                        {
                            foreach (var item in clusterpolygon.polygon)
                            {
                                cluster.LtLng.Add(new LtLng
                                {
                                    CityId = cluster.CityId,
                                    latitude = item[0],
                                    longitude = item[1],
                                    polygon = polygonname
                                });
                            }
                        }
                        cluster.UpdatedDate = indianTime;
                        mycontext.Entry(cluster).State = EntityState.Modified;
                    }

                    result = mycontext.Commit() > 0;

                    if (cluster != null)
                    {
                        var whCustomers = mycontext.Customers.Where(x => x.Warehouseid == cluster.WarehouseId && (x.lat != 0 || x.lg != 0) && (!x.ClusterId.HasValue || x.ClusterId.Value == 0)).ToList();

                        if (whCustomers != null && whCustomers.Any())
                        {
                            CustomersManager manager = new CustomersManager();
                            BackgroundTaskManager.Run(() =>
                            {
                                manager.UpdateCustomersCluster(whCustomers);
                            });
                        }
                    }
                }
            }
            var res = new
            {
                status = result,
                Message = ""
            };
            return Request.CreateResponse(HttpStatusCode.OK, res);
        }


        [Route("addCity")]
        [HttpPost]
        public bool addCity(Cluster data)
        {
            bool result = false;
            logger.Info("start Add Page: ");
            try
            {
                var identity = User.Identity as ClaimsIdentity;
                int compid = 0, userid = 0;

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))
                    compid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                    userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);


                using (AuthContext db = new AuthContext())
                {
                    City citydata = db.Cities.Where(x => x.Cityid == data.CityId).FirstOrDefault();
                    Cluster dataclus = db.Clusters.Where(x => x.ClusterId == data.ClusterId).FirstOrDefault();

                    dataclus.WorkingCityName = citydata.CityName;
                    db.Entry(dataclus).State = EntityState.Modified;
                    db.Commit();

                }
                return true;
            }

            catch (Exception ex)

            {
                logger.Error("Error in Addcity " + ex.Message);

                return false;
            }
        }


        [Route("GetActiveCount")]
        [HttpGet]
        public dynamic GetActiveCount(int cityid)
        {
            using (var con = new AuthContext())
            {
                var query = "select count(cs.active) as activecust from Clusters cl inner join Customers cs on cl.ClusterId = cs.ClusterId where cl.CityId = '" + cityid + "' and cs.Active = 1";


                var count = con.Database.SqlQuery<countCls>(query).ToList();

                return count;

            }


        }

        [Route("GetcustCount")]
        [HttpGet]
        public dynamic GetcustCount(int cityid)
        {
            using (var con = new AuthContext())
            {
                var query = "select count(a.CustomerId) as custcount from Customers  a inner join Clusters c on a.ClusterId =c.ClusterId where a.cityid='" + cityid + "'";

                var custcount = con.Database.SqlQuery<countcust>(query).ToList();

                return custcount;


            }


        }

        [Route("GetagentCount")]
        [HttpGet]
        public dynamic GetagentCount(int cityid)
        {
            using (var con = new AuthContext())
            {
                var query = "select count(ca.agentid) as cntagent from ClusterAgents ca inner join clusters c on ca.ClusterId =c.ClusterId where c.cityid='" + cityid + "'";

                var agntcount = con.Database.SqlQuery<countAgent>(query).ToList();

                return agntcount;


            }


        }



        [HttpGet]
        [Route("UpdateActiveClusters")]
        public HttpResponseMessage UpdateActiveClusters(int ClusterId)
        {
            using (AuthContext db = new AuthContext())
            {
                var getClstActive = db.Clusters.Where(x => x.ClusterId == ClusterId).FirstOrDefault();
                if (getClstActive != null && getClstActive.Active == true)
                {
                    getClstActive.Active = false;
                    db.Entry(getClstActive).State = EntityState.Modified;
                    db.Commit();
                }
                else
                {
                    getClstActive.Active = true;
                    db.Entry(getClstActive).State = EntityState.Modified;
                    db.Commit();

                }

                if (getClstActive.Active)
                {
                    SellerStoreController sellerController = new SellerStoreController();
                    sellerController.UpdateClusterToSeller(getClstActive);
                }
                else
                {
                    SellerStoreController sellerController = new SellerStoreController();
                    sellerController.InActiveClusterToSeller(getClstActive.ClusterId);
                }
                return Request.CreateResponse(HttpStatusCode.OK, getClstActive);

            }
        }


        //[HttpGet]
        //[Route("UpdateActiveClustersV7")]
        //public HttpResponseMessage UpdateActiveClustersV7(int ClusterId)
        //{
        //    using (AuthContext db = new AuthContext())
        //    {
        //        var getClstActive = db.Clusters.Where(x => x.ClusterId == ClusterId).FirstOrDefault();
        //        if (getClstActive != null)
        //        {
        //            getClstActive.Active = false;
        //            db.Entry(getClstActive).State = EntityState.Modified;
        //            db.Commit();
        //        }
        //        else
        //        {
        //            getClstActive.Active = true;
        //            db.Entry(getClstActive).State = EntityState.Modified;
        //            db.Commit();

        //        }
        //        return Request.CreateResponse(HttpStatusCode.OK, getClstActive);

        //    }
        //}


        [HttpGet]
        [Route("RefereshCityCluster")]
        public HttpResponseMessage RefereshCityCluster(int cityId)
        {
            var identity = User.Identity as ClaimsIdentity;
            int userid = 0;

            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

            int Refereshid = 0;
            using (AuthContext mycontext = new AuthContext())
            {
                var insertquery = "Insert into ClusterRefreshRequests values(GETDATE(), " + cityId + ", 0, 0, " + "null," + userid + ");SELECT SCOPE_IDENTITY(); ";
                var val = mycontext.Database.SqlQuery<decimal>(insertquery).FirstOrDefault();
                Refereshid = Convert.ToInt32(val);
            }

            var result = true;
            BackgroundTaskManager.Run(() =>
            {
                using (AuthContext mycontext = new AuthContext())
                {
                    var sqlquery = "exec [dbo].[RefereshClusterCustomer] " + cityId + "," + userid;
                    mycontext.Database.CommandTimeout = 600;
                    int intresult = mycontext.Database.ExecuteSqlCommand(sqlquery);

                    if (Refereshid > 0)
                    {
                        string query = "select * from ClusterRefershCustomers where orderid is not null and ClusterRefreshRequestId=" + Refereshid;
                        List<ClusterRefershCustomer> ClusterRefershCustomers = mycontext.Database.SqlQuery<ClusterRefershCustomer>(query).ToList();

                        foreach (var item in ClusterRefershCustomers.GroupBy(x => new { x.NewWarehouseId, x.NewWarehouseName }))
                        {
                            string orderids = string.Join(",", item.Select(x => x.OrderId).ToList());

                            var order = orderids.Split(',').Select(x => new OrderUpdateQueue
                            {
                                OrderId = Convert.ToInt32(x),
                                Status = "Pending",
                                UpdateDate = indianTime
                            }).ToList();


                            OrderMasterChangeDetectManager orderMasterChangeDetectManager = new OrderMasterChangeDetectManager();
                            order.ForEach(x =>
                                {
                                    orderMasterChangeDetectManager.UpdateOrderInMongo(x);
                                });

                        }


                        List<int> customerIds = ClusterRefershCustomers.Select(x => x.CustomerId).ToList();

                        var skCustomers = mycontext.Customers.Where(x => customerIds.Contains(x.CustomerId) && x.ClusterId.HasValue).Select(x => new SkCustomer
                        {
                            ClusterId = x.ClusterId.Value,
                            Skcode = x.Skcode
                        }).ToList();

                        SellerStoreController sellerController = new SellerStoreController();
                        sellerController.RefereshClusterToSeller(skCustomers);
                    }

                }
            });


            var res = new
            {
                status = result,
                Message = "Your Cluster customer referesh request proceed please check after some time."
            };
            return Request.CreateResponse(HttpStatusCode.OK, res);



        }


        [HttpGet]
        [Route("GetClusterRefreshData")]
        public HttpResponseMessage GetClusterRefreshData(int cityId)
        {
            using (AuthContext mycontext = new AuthContext())
            {
                var data = mycontext.ClusterRefreshRequest.Where(x => x.CityId == cityId).Select(x => new ClusterRefreshRequestDc
                {
                    CityId = x.CityId,
                    Id = x.Id,
                    RefereshBy = x.RefereshBy,
                    RefreshCustomerCount = x.RefreshCustomerCount,
                    RefreshDate = x.RefreshDate,
                    Status = x.Status
                }).ToList();

                if (data != null)
                {
                    var cityids = data.Select(x => x.CityId).Distinct().ToList();
                    var peopleids = data.Select(x => x.RefereshBy).Distinct().ToList();
                    var citydetail = mycontext.Cities.Where(x => cityids.Contains(x.Cityid)).Select(x => new { x.Cityid, x.CityName }).ToList();

                    var Peopledetail = mycontext.Peoples.Where(x => peopleids.Contains(x.PeopleID)).Select(x => new { x.PeopleID, x.DisplayName }).ToList();
                    data.ForEach(x =>
                    {
                        if (citydetail != null && citydetail.Any(y => y.Cityid == x.CityId))
                            x.CityName = citydetail.FirstOrDefault(y => y.Cityid == x.CityId).CityName;

                        if (peopleids != null && Peopledetail.Any(y => y.PeopleID == x.RefereshBy))
                            x.RefereshByName = Peopledetail.FirstOrDefault(y => y.PeopleID == x.RefereshBy).DisplayName;
                    });
                }


                data = data.OrderByDescending(x => x.RefreshDate).ToList();

                return Request.CreateResponse(HttpStatusCode.OK, data);

            }

        }

        [Route("GetClusterRefreshDataValidation")]
        [HttpGet]
        public HttpResponseMessage GetClusterRefreshDataValidation(int cityid)
        {
            using (var con = new AuthContext())
            {
                var result = con.ClusterRefreshRequest.Any(x => x.CityId == cityid && x.Status == 0);
                var res = new
                {
                    status = result,
                    Message = "please check after some time."
                };
                return Request.CreateResponse(HttpStatusCode.OK, res);
            }
        }

        [Route("getExportCluster")]
        [HttpGet]
        public dynamic getExportCluster(int clusterId)
        {
            using (var db = new AuthContext())
            {

                var ExportData = db.Customers.Where(x => x.ClusterId == clusterId).ToList();
                var warehouseIds = ExportData.Select(x => x.Warehouseid).Distinct().ToList();
                var warehouses = db.Warehouses.Where(x => warehouseIds.Contains(x.WarehouseId)).ToList();
                ExportData.ForEach(x =>
                {
                    x.WarehouseName = x.Warehouseid.HasValue ? warehouses.FirstOrDefault(z => z.WarehouseId == x.Warehouseid.Value)?.WarehouseName : "";
                });
                return ExportData;
            }
        }

        [Route("UpdateClusterCustomer")]
        [HttpGet]
        public bool UpdateClusterCustomer(int clusterid, bool isChangeExecute)
        {
            bool result = false;
            var identity = User.Identity as ClaimsIdentity;
            string userName = "";

            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "username"))
                userName = identity.Claims.FirstOrDefault(x => x.Type == "username").Value;
            using (var db = new AuthContext())
            {
                int PeopleId = 0;
                string PeopleName = "";
                var customers = db.Customers.Where(x => x.ClusterId == clusterid).ToList();
                var cluster = db.Clusters.FirstOrDefault(x => x.ClusterId == clusterid);
                if (isChangeExecute)
                {
                    var clusteragent = db.ClusterAgent.FirstOrDefault(x => x.ClusterId == clusterid && x.active && !x.Deleted);
                    if (clusteragent != null)
                    {
                        var people = db.Peoples.Where(x => x.PeopleID == clusteragent.AgentId).Select(x => new { x.PeopleID, x.DisplayName }).FirstOrDefault();
                        if (people != null)
                        {
                            PeopleId = people.PeopleID;
                            PeopleName = people.DisplayName;
                        }
                    }
                }

                customers.ForEach(x =>
                {
                    x.Warehouseid = cluster.WarehouseId;
                    if (isChangeExecute && PeopleId > 0)
                    {
                        //x.ExecutiveId = PeopleId;
                        //x.ExecutiveName = PeopleName;
                        x.AgentCode = PeopleId.ToString();
                    }
                    x.UpdatedDate = indianTime;
                    x.LastModifiedBy = userName;
                    db.Entry(x).State = System.Data.Entity.EntityState.Modified;
                });
                List<int> customerIds = customers.Select(x => x.CustomerId).ToList();
                if (customerIds != null && customerIds.Any())
                {
                    var Orders = db.DbOrderMaster.Where(x => customerIds.Contains(x.CustomerId) && x.Status == "Pending").Include(x => x.orderDetails).ToList();
                    if (Orders != null && Orders.Any())
                    {
                        List<string> itemSellingSku = Orders.SelectMany(x => x.orderDetails.Select(y => y.SellingSku)).ToList();
                        var itemmasters = db.itemMasters.Where(x => x.WarehouseId == cluster.WarehouseId && itemSellingSku.Contains(x.SellingSku)).Select(x => new { x.SellingSku, x.ItemId }).ToList();
                        foreach (var order in Orders)
                        {
                            order.WarehouseId = cluster.WarehouseId.Value;
                            order.WarehouseName = cluster.WarehouseName;
                            order.orderDetails.ToList().ForEach(x =>
                              {
                                  x.WarehouseId = cluster.WarehouseId.Value;
                                  x.WarehouseName = cluster.WarehouseName; ;
                                  if (itemmasters.Any(y => y.SellingSku == x.SellingSku))
                                  {
                                      x.ItemId = itemmasters.FirstOrDefault(y => y.SellingSku == x.SellingSku).ItemId;
                                  }
                              });
                            db.Entry(order).State = System.Data.Entity.EntityState.Modified;
                        }
                    }
                    result = db.Commit() > 0;
                }


            }

            return result;
        }


        [Route("getCurrentClusterAgent")]
        [HttpGet]
        public ClusterAgent getCurrentClusterAgent(int clusterId)
        {
            using (var db = new AuthContext())
            {
                var clusterAgent = db.ClusterAgent.FirstOrDefault(x => x.ClusterId == clusterId && x.active == true & x.Deleted == false);
                return clusterAgent;
            }
        }

        [Route("setSalesExecutiveToCluster")]
        [HttpGet]
        public bool setSalesExecutiveToCluster(int executiveId, int clusterid, bool updateAllCustomersExecutives)
        {
            bool result = false;
            var identity = User.Identity as ClaimsIdentity;
            string userName = "";

            try
            {

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "username"))
                    userName = identity.Claims.FirstOrDefault(x => x.Type == "username").Value;
                using (var db = new AuthContext())
                {

                    var cluster = db.ClusterAgent.FirstOrDefault(x => x.ClusterId == clusterid && x.active == true & x.Deleted == false); ;
                    var executive = db.Peoples.FirstOrDefault(x => x.PeopleID == executiveId && x.Active == true && x.Deleted == false);

                    //cluster.ExecutiveId = executive.PeopleID;
                    //cluster.ExecutiveName = executive.DisplayName;
                    cluster.UpdatedDate = indianTime;

                    db.Entry(cluster).State = System.Data.Entity.EntityState.Modified;



                    if (updateAllCustomersExecutives == true)
                    {

                        // Customer Update

                        var customers = db.Customers.Where(x => x.ClusterId == clusterid).ToList();
                        var clusters = db.Clusters.FirstOrDefault(x => x.ClusterId == clusterid);
                        customers.ForEach(x =>
                        {
                            x.Warehouseid = clusters.WarehouseId;
                            if (executive != null)
                            {
                                //x.ExecutiveId = executive.PeopleID;
                                //x.ExecutiveName = executive.DisplayName;
                                //x.AgentCode = executive.AgentCode;
                            }
                            x.UpdatedDate = indianTime;
                            x.LastModifiedBy = userName;
                            db.Entry(x).State = System.Data.Entity.EntityState.Modified;
                        });
                        List<int> customerIds = customers.Select(x => x.CustomerId).ToList();

                    }
                    result = db.Commit() > 0;
                }

                return result;

            }
            catch (Exception e)
            {
                throw e;
            }

        }

        [Route("getActiveExecutivesByClusterID")]
        public List<People> getActiveExecutivesByClusterID(int ClusterID)
        {
            //logger.Info("start get all Sales Executive: ");
            using (var db = new AuthContext())
            {
                try
                {
                    string query = "select distinct p.* from People p inner join AspNetUsers u on p.Email=u.Email inner join AspNetUserRoles ur on u.Id=ur.UserId inner join AspNetRoles r on ur.RoleId=r.Id inner join Customers cs on cs.Warehouseid = p.WarehouseId where cs.ClusterId=" + ClusterID + " and r.Name='Sales Executive' and ur.isActive=1 and p.Active=1 and p.Deleted=0";
                    var displist = db.Database.SqlQuery<People>(query).ToList();
                    //displist = db.Peoples.Where(x => x.Department == "Sales Executive" && x.Active == true && x.CompanyId == compid && x.WarehouseId == Warehouse_id).ToList();
                    logger.Info("End  Sales Executive: ");
                    return displist;
                }
                catch (Exception ex)
                {
                    logger.Error("Error in getall Sales Executive " + ex.Message);
                    logger.Info("End getall Sales Executive: ");
                    return null;
                }
            }
        }




    }

    public class dboylist
    {
        public string DBoy { get; set; }
    }

    public class Clusterpolygon
    {
        public int ClusterId { get; set; }
        public double[][] polygon { get; set; }
    }

    public class countCls
    {
        public int activecust { get; set; }

    }
    public class countcust
    {
        public int custcount { get; set; }

    }
    public class countAgent
    {
        public int cntagent { get; set; }

    }

    public class ClusterPeopleDc
    {
        public int id { get; set; }
        public int ClusterID { get; set; }
        public string label { get; set; }
        public bool Selected { get; set; }
    }
}

