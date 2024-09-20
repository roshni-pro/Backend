using AngularJSAuthentication.API.Controllers.Base;
using AngularJSAuthentication.Model;
using AngularJSAuthentication.API.Helpers;
using AngularJSAuthentication.DataContracts.Mongo;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Web.Http;
using System.Web.Http.Description;
using LinqKit;
using MongoDB.Bson;
using System.Threading.Tasks;

namespace AngularJSAuthentication.API.Controllers
{
    [RoutePrefix("api/City")]
    public class CityController : BaseAuthController
    {

        private static Logger logger = LogManager.GetCurrentClassLogger();

        // [Authorize]
        [Route("")]
        [AllowAnonymous]
        public IEnumerable<City> Get()
        {
            logger.Info("start City: ");
            List<City> ass = new List<City>();
            try
            {
                var identity = User.Identity as ClaimsIdentity;
                int compid = 0, userid = 0, Warehouse_id = 0;
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

                using (var db = new AuthContext())
                {
                    if (Warehouse_id > 0)
                    {
                        var wh = db.Warehouses.Where(x => x.WarehouseId == Warehouse_id).SingleOrDefault();
                        ass = db.Cities.Where(x => x.Cityid == wh.Cityid).ToList();
                    }
                    else
                    {
                        ass = db.Cities.Where(x => x.Deleted == false).ToList().OrderBy(x => x.CityName).ToList();
                    }

                }
                return ass;
            }
            catch (Exception ex)
            {
                logger.Error("Error in City " + ex.Message);
                logger.Info("End  City: ");
                return null;
            }

        }


        [Authorize]
        [Route("GetByStateID/{stateID}")]
        public IEnumerable<City> GetByStateID(int stateID)
        {
            logger.Info("start City: ");
            using (AuthContext context = new AuthContext())
            {
                List<City> ass = new List<City>();
                try
                {
                    var identity = User.Identity as ClaimsIdentity;
                    int compid = 0, userid = 0, Warehouse_id = 0;
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

                    logger.Info("User ID : {0} , Company Id : {1}", compid, userid);
                    if (Warehouse_id > 0)
                    {
                        var wh = context.Warehouses.Where(x => x.WarehouseId == Warehouse_id).SingleOrDefault();
                        ass = context.Cities.Where(x => x.Cityid == wh.Cityid && x.Stateid == stateID).ToList();
                        //ass = ass.Where(x => x.Stateid == stateID).ToList();
                    }
                    else
                    {
                        ass = context.Cities.Where(x => x.Deleted == false && x.Stateid == stateID).ToList();
                        // ass = ass.Where(x => x.Stateid == stateID).ToList();

                    }

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

        [Authorize]
        [Route("")]
        public IEnumerable<City> Get(string id)
        {
            logger.Info("start City: ");
            using (AuthContext context = new AuthContext())
            {
                List<City> ass = new List<City>();
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
                    int idd = Int32.Parse(id);
                    ass = context.AllCity(idd).ToList();
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

        [ResponseType(typeof(City))]
        [Route("")]
        [AcceptVerbs("POST")]
        public City add(City item)
        {
            logger.Info("start add City: ");
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
                    //item.CompanyId = compid;
                    if (item == null)
                    {
                        throw new ArgumentNullException("item");
                    }
                    logger.Info("User ID : {0} , Company Id : {1}", compid, userid);
                    item.CompanyId = compid;
                    context.AddCity(item, userid);
                    logger.Info("End add City: ");
                    return item;
                }
                catch (Exception ex)
                {
                    logger.Error("Error in add City " + ex.Message);
                    logger.Info("End  addCity: ");
                    return null;
                }
            }
        }

        [ResponseType(typeof(City))]
        [Route("")]
        [AcceptVerbs("PUT")]
        public City Put(City item)
        {
            using (AuthContext context = new AuthContext())
            {
                try
                {
                    var identity = User.Identity as ClaimsIdentity;
                    int compid = 0, userid = 0;
                    string username = "";
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
                        if (claim.Type == "username")
                        {
                            username = claim.Value;
                        }
                    }
                    logger.Info("User ID : {0} , Company Id : {1}", compid, userid);
                    item.UpdateBy = username;
                    return context.PutCity(item);
                }
                catch (Exception ex)
                {
                    return null;
                }
            }
        }


        [ResponseType(typeof(City))]
        [Route("")]
        [AcceptVerbs("Delete")]
        public void Remove(int id)
        {
            logger.Info("start deleteCityy: ");
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
                    logger.Info("User ID : {0} , Company Id : {1}", compid, userid);
                    context.DeleteCity(id);
                    logger.Info("End  delete City: ");
                }
                catch (Exception ex)
                {

                    logger.Error("Error in deleteCity" + ex.Message);


                }
            }
        }


        [ResponseType(typeof(City))]
        [Route("DeleteV7")]
        [AcceptVerbs("Delete")]
        public Boolean DeleteV7(int id)
        {
            logger.Info("start deleteCityy: ");
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
                    logger.Info("User ID : {0} , Company Id : {1}", compid, userid);
                    context.DeleteCity(id);
                    logger.Info("End  delete City: ");
                    return true;
                }
                catch (Exception ex)
                {

                    logger.Error("Error in deleteCity" + ex.Message);
                    return false;

                }
            }
        }


        #region get the supplier City
        /// <summary>
        /// get supplier city
        /// Created Date:18/06/2019
        /// Created By Raj
        /// </summary>
        /// <returns></returns>
        [Route("suppliercity")]
        [HttpGet]
        public IEnumerable<City> Getsuppliercity(int Statid)
        {
            logger.Info("start City: ");
            using (AuthContext context = new AuthContext())
            {
                List<City> ass = new List<City>();
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
                    ass = context.Cities.Where(x => x.Deleted == false && x.IsSupplier == true && x.Stateid == Statid).ToList();
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
        #endregion
        #region get the Customer City
        /// <summary>
        /// get the Customer City
        /// </summary>
        /// <param name="Statid"></param>
        /// <returns></returns>
        [Route("Customercity")]
        [HttpGet]
        public IEnumerable<City> Customercity(int Statid)
        {
            using (AuthContext context = new AuthContext())
            {
                List<City> citylist = new List<City>();
                try
                {
                    citylist = context.Cities.Where(x => x.Deleted == false && x.Stateid == Statid).ToList();
                    return citylist;
                }
                catch (Exception ex)
                {
                    logger.Error("Error in City " + ex.Message);
                    logger.Info("End  City: ");
                    return null;
                }
            }
        }
        #endregion

        [Route("cityget")]
        [HttpGet]
        public List<RetailerMinOrderdc> cityget()
        {
            List<RetailerMinOrderdc> RetailerMinOrderdcs = new List<RetailerMinOrderdc>();
            using (var con = new AuthContext())
            {
                MongoDbHelper<RetailerMinOrder> mongoDbHelper = new MongoDbHelper<RetailerMinOrder>();
                var retailerMinOrder = mongoDbHelper.Select(x => x.CityId > 0).ToList();
                var warehouseCities = con.Warehouses.Where(x => !x.IsKPP).Select(x => new { x.Cityid, x.WarehouseId, x.WarehouseName, x.CityName }).Distinct().ToList();
                foreach (var item in warehouseCities)
                {
                    var mongoitem = retailerMinOrder.FirstOrDefault(x => x.CityId == item.Cityid && x.WarehouseId == item.WarehouseId);
                    var RetailerMinOrderdc = new RetailerMinOrderdc();
                    RetailerMinOrderdc.warehouseid = item.WarehouseId;
                    RetailerMinOrderdc.warehousename = item.WarehouseName;
                    RetailerMinOrderdc.cityid = item.Cityid;
                    RetailerMinOrderdc.cityname = item.CityName;
                    RetailerMinOrderdc.MinOrderValue = mongoitem != null ? mongoitem.MinOrderValue : 0;
                    if (mongoitem != null)
                        RetailerMinOrderdc.Id = mongoitem.Id;
                    RetailerMinOrderdcs.Add(RetailerMinOrderdc);
                }
                return RetailerMinOrderdcs;
            }
        }

        [Route("cityupdate")]
        [HttpPost]
        public List<RetailerMinOrder> cityupdate(List<RetailerMinOrder> RetailerMinOrderdcs)
        {
            MongoDbHelper<RetailerMinOrder> mongoDbHelper = new MongoDbHelper<RetailerMinOrder>();
            var retailerMinOrders = mongoDbHelper.Select(x => x.CityId > 0).ToList();
            foreach (var item in RetailerMinOrderdcs)
            {
                //var mongoId = new ObjectId(item.Id);
                var retailerMinOrder = retailerMinOrders.FirstOrDefault(x => x.CityId == item.CityId && x.WarehouseId == item.WarehouseId);
                if (retailerMinOrder != null)
                {
                    retailerMinOrder.MinOrderValue = item.MinOrderValue;
                    mongoDbHelper.Replace(retailerMinOrder.Id, retailerMinOrder, "RetailerMinOrder");
                }
                else
                {
                    mongoDbHelper.Insert(item);
                }
            }

            return RetailerMinOrderdcs;
        }



        [HttpGet]
        [Route("GetActiveCity")]
        public dynamic GetActiveCity()
        {
            using (AuthContext db = new AuthContext())
            {

                string sqlquery = "select * from Cities where active = 1 and Deleted = 0";
                List<City> data = db.Database.SqlQuery<City>(sqlquery).ToList();

                return data;
            }
        }

        [HttpGet]
        [Route("GetPoCityWithoutKpp")]
        [AllowAnonymous]
        public IEnumerable<City> GetPoCityWithoutKpp()
        {
            logger.Info("start City: ");
            List<City> ass = new List<City>();
            try
            {
                
                using (var db = new AuthContext())
                {
                    var query = "select * from Cities ct where ct.Cityid in (select distinct(c.Cityid) from Cities c join Warehouses w on w.Cityid = c.Cityid where w.active = 1 and w.IsKPP = 0 and w.Deleted = 0)";
                    ass = db.Database.SqlQuery<City>(query).ToList();
                }
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
    public class RetailerMinOrderdc
    {
        public ObjectId Id { get; set; }
        public int warehouseid { get; set; }
        public string warehousename { get; set; }
        public int cityid { get; set; }
        public string cityname { get; set; }

        public int MinOrderValue { get; set; }

    }
}



