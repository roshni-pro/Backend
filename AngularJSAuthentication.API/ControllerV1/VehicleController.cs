using AngularJSAuthentication.Model;
using NLog;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Web.Http;
using System.Web.Http.Description;

namespace AngularJSAuthentication.API.Controllers
{
    [RoutePrefix("api/Vehicles")]
    public class VehicleController : ApiController
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        [Authorize]
        [Route("")]
        public IEnumerable<Vehicle> Get()
        {
            logger.Info("start Vehicles: ");
            List<Vehicle> ass = new List<Vehicle>();
            using (var context = new AuthContext())
            {
                try
                {
                    var identity = User.Identity as ClaimsIdentity;
                    int compid = 0, userid = 0;
                    int Warehouse_id = 0;
                    string email = "";
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
                        if (claim.Type == "email")
                        {
                            email = claim.Value;
                        }
                    }

                    if (Warehouse_id > 0)
                    {

                        logger.Info("User ID : {0} , Company Id : {1}", compid, userid);
                        ass = context.AllVehiclesWid(compid, Warehouse_id).ToList();
                        logger.Info("End  Vehicle: ");
                        return ass;
                    }
                    else
                    {

                        logger.Info("User ID : {0} , Company Id : {1}", compid, userid);
                        ass = context.AllVehicles(compid).ToList();
                        logger.Info("End  Vehicle: ");
                        return ass;
                    }



                }
                catch (Exception ex)
                {
                    logger.Error("Error in Vehicle " + ex.Message);
                    logger.Info("End  Vehicle: ");
                    return null;
                }
            }
        }


        [ResponseType(typeof(Vehicle))]
        [Route("")]
        [AcceptVerbs("POST")]
        public Vehicle add(Vehicle item)
        {
            logger.Info("start add Vehicle: ");
            using (var context = new AuthContext())
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
                    item.CompanyId = compid;
                    if (item == null)
                    {
                        throw new ArgumentNullException("item");
                    }
                    logger.Info("User ID : {0} , Company Id : {1}", compid, userid);
                    context.AddVehicle(item);
                    logger.Info("End add Vehicle: ");
                    return item;
                }
                catch (Exception ex)
                {
                    logger.Error("Error in add Vehicle " + ex.Message);
                    logger.Info("End  addVehicle: ");
                    return null;
                }
            }
        }

        [ResponseType(typeof(Vehicle))]
        [Route("")]
        [AcceptVerbs("PUT")]
        public Vehicle Put(Vehicle item)
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
                    return context.PutVehicle(item);
                }
                catch
                {
                    return null;
                }
            }
        }


        [ResponseType(typeof(Vehicle))]
        [Route("")]
        [AcceptVerbs("Delete")]
        public void Remove(int id)
        {
            logger.Info("start deleteVehicley: ");
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
                    int CompanyId = compid;
                    logger.Info("User ID : {0} , Company Id : {1}", compid, userid);
                    context.DeleteVehicle(id, CompanyId);
                    logger.Info("End  delete Vehicle: ");
                }
                catch (Exception ex)
                {

                    logger.Error("Error in deleteVehicle" + ex.Message);


                }
            }
        }

        [Route("VehicleNumber")]
        [HttpGet]
        public HttpResponseMessage CheckVehicleNumber(string VehicleNumber)
        {
            using (var db = new AuthContext())
            {
                try
                {
                    logger.Info("Get Peoples: ");
                    int compid = 0, userid = 0;
                    int Warehouse_id = 0;
                    string email = "";
                    var identity = User.Identity as ClaimsIdentity;
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
                        if (claim.Type == "email")
                        {
                            email = claim.Value;
                        }
                    }
                    logger.Info("End Get Company: ");
                    if (Warehouse_id > 0)
                    {
                        var RDMobile = db.VehicleDb.Where(x => x.VehicleNumber == VehicleNumber && x.WarehouseId == Warehouse_id).FirstOrDefault();

                        return Request.CreateResponse(HttpStatusCode.OK, RDMobile);
                    }
                    else
                    {
                        var RDMobile = db.VehicleDb.Where(x => x.VehicleNumber == VehicleNumber).FirstOrDefault();

                        return Request.CreateResponse(HttpStatusCode.OK, RDMobile);

                    }

                }
                catch (Exception ex)
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, false);
                }
            }
        }

        [Route("GetPeopleData")]
        [HttpGet]
        public dynamic GetPeopleData(int warehouseId)
        {
            using (var db = new AuthContext())
            {
                var warehouseIds = new SqlParameter("@warehouseId", warehouseId);
                var result = db.Database.SqlQuery<GetPeopledataDTO>("GetPeopleVehicle @warehouseId", warehouseIds).ToList();
                return result;
            }
        }
        public class GetPeopledataDTO
        {
            public int PeopleID { get; set; }
            public string DisplayName { get; set; }
        }

    }
}



