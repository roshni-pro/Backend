using AngularJSAuthentication.Model;
using NLog;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Web.Http;

namespace AngularJSAuthentication.API.ControllerV7
{
    [RoutePrefix("api/zone")]
    public class ZoneController : ApiController
    {

      
        private static Logger logger = LogManager.GetCurrentClassLogger();

        [Authorize]
        [Route("")]
        [HttpGet]
        public HttpResponseMessage Get()
        {

            logger.Info("start State: ");

            try
            {
                //var identity = User.Identity as ClaimsIdentity;
                //int compid = 0, userid = 0;
                //// Access claims
                //foreach (Claim claim in identity.Claims)
                //{
                //    if (claim.Type == "compid")
                //    {
                //        compid = int.Parse(claim.Value);
                //    }
                //    if (claim.Type == "userid")
                //    {
                //        userid = int.Parse(claim.Value);
                //    }
                //}
                using (AuthContext context = new AuthContext())
                {

                    List<Zone> zone = context.zone.Where(x => x.IsDeleted == false).ToList();
                    logger.Info("End  Stste: ");
                    return Request.CreateResponse(HttpStatusCode.OK, zone);

                }

            }
            catch (Exception ex)
            {

                return Request.CreateResponse(HttpStatusCode.BadRequest, "some thing went wrong");
            }
        }



        [Authorize]
        [Route("")]
        [HttpPost]
        public IHttpActionResult add(Zone zone)
        {
            // var data = Peoples.Where(x => x.Deleted == false && x.PeopleID == zone.ZoneManagerId).FirstOrDefault();
            logger.Info("Zone: ");
            //var identity = User.Identity as ClaimsIdentity;
            //int compid = 0, userid = 0;
            //// Access claims
            //foreach (Claim claim in identity.Claims)
            //{
            //    if (claim.Type == "compid")
            //    {
            //        compid = int.Parse(claim.Value);
            //    }
            //    if (claim.Type == "userid")
            //    {
            //        userid = int.Parse(claim.Value);
            //    }
            //}
            //item.CompanyId = compid;
            //if (item == null)
            //{
            //    throw new ArgumentNullException("item");
            //}
            using (AuthContext context = new AuthContext())
            {
                var data = context.Peoples.Where(x => x.Deleted == false && x.PeopleID == zone.ZoneManagerId).FirstOrDefault();
                Country countries = context.Countries.Where(c => c.CountryId == zone.CountryId && c.Deleted == false).FirstOrDefault();
                Zone zoneData = new Zone();
                if (data != null)
                {
                    zoneData.ZoneManager = data.DisplayName;
                }
                else
                {
                    zoneData.ZoneManager = data.PeopleFirstName;

                }
                var Zone = context.zone.Where(c => c.ZoneName == zone.ZoneName).FirstOrDefault();
                // var Zone = context.zone.Where(c => c.ZoneName == zone.ZoneName).ToList();
                if (Zone == null)
                {
                    zoneData.ZoneName = zone.ZoneName;
                    zoneData.ZoneManagerId = zone.ZoneManagerId;
                    zoneData.CountryId = zone.CountryId;
                    zoneData.CountryName = countries.CountryName;
                    zoneData.CreatedDate = DateTime.Now;
                    zoneData.UpdatedDate = DateTime.Now;
                    zoneData.ModifiedDate = DateTime.Now;
                    zoneData.IsActive = true;
                    context.zone.Add(zoneData);
                    context.Commit();
                    //return zoneData;
                    return Ok(zoneData);
                }
                else
                {

                    return Ok("Data Already Exist");

                }

            }
        }





        //////[Route("update")]
        //////[HttpPut]
        //////public Zone update(Zone Zon)
        //////{
        //////    using (AuthContext context = new AuthContext())
        //////    {

        //////        Zone zone = context.zone.Where(c => c.ZoneId.Equals(Zon.ZoneId) && c.IsDeleted == false).SingleOrDefault();
        //////        var countryname = context.Countries.Where(x => x.CountryId == Zon.CountryId).Select(y=>y.CountryName).FirstOrDefault();
        //////        //People people = context.Peoples.Where(c => c.PeopleID.Equals(Zon.CountryId) && c.Deleted == false).SingleOrDefault();

        //////        if (zone != null)
        //////        {
        //////            zone.ZoneName = Zon.ZoneName;
        //////            zone.ZoneManager = Zon.ZoneManager;
        //////            zone.CountryName = countryname;
        //////            zone.CountryId = Zon.CountryId;
        //////            zone.ZoneManagerId = Zon.ZoneManagerId;
        //////            zone.ZoneManager = Zon.ZoneManager;
        //////            zone.UpdatedDate = DateTime.Now;
        //////            //context.zone.Attach(Zon);
        //////            //context.SaveChanges();
        //////            context.Entry(zone).State = EntityState.Modified;
        //////            context.SaveChanges();
        //////            return zone;
        //////        }

        //////        return Zon;

        //////    }


        //////}


        [Route("update")]
        [HttpPut]
        public IHttpActionResult update(Zone Zon)
        {
            using (AuthContext context = new AuthContext())
            {

                Zone zone = context.zone.Where(c => c.ZoneId.Equals(Zon.ZoneId) && c.IsDeleted == false).SingleOrDefault();
                var countryname = context.Countries.Where(x => x.CountryId == Zon.CountryId).Select(y => y.CountryName).FirstOrDefault();
                //People people = context.Peoples.Where(c => c.PeopleID.Equals(Zon.CountryId) && c.Deleted == false).SingleOrDefault();

                var Zones = context.zone.Where(c => c.ZoneName == Zon.ZoneName).FirstOrDefault();
                if (Zones == null)
                {



                    zone.ZoneName = Zon.ZoneName;
                    zone.ZoneManager = Zon.ZoneManager;
                    zone.CountryName = countryname;
                    zone.CountryId = Zon.CountryId;
                    zone.ZoneManagerId = Zon.ZoneManagerId;
                    zone.ZoneManager = Zon.ZoneManager;
                    zone.UpdatedDate = DateTime.Now;
                    //context.zone.Attach(Zon);
                    //context.SaveChanges();
                    context.Entry(zone).State = EntityState.Modified;
                    context.Commit();
                    return Ok(zone);

                }
                else
                {
                    return Ok("Data Already Exist");
                }

            }


        }


        #region get Agent and Dbay Devicehistory

        [Route("GetActiveAgentsForZone")]
        [HttpGet]
        public dynamic AgentnDboyDevicehistory()
        {
           
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
                int CompanyId = compid;
                using (AuthContext db = new AuthContext())
                {
                    var data = db.Peoples.Where(x => x.Active == true && x.Deleted == false).Select(x => new { x.DisplayName, x.PeopleID }).ToList();
                    return data;
                }
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        #endregion



        [Route("Delete")]
        [HttpGet]
        public Zone delete(int id)
        {
            using (AuthContext context = new AuthContext())
            {
                Zone zone = context.zone.Where(c => c.ZoneId.Equals(id) && c.IsDeleted == false).SingleOrDefault();

                context.zone.Remove(zone);
                context.Commit();
                return zone;
            }
        }

        [Route("DeleteV7")]
        [HttpDelete]
        public Zone DeleteV7(int id)
        {
            using (AuthContext context = new AuthContext())
            {
                Zone zone = context.zone.Where(c => c.ZoneId.Equals(id) && c.IsDeleted == false).SingleOrDefault();

                context.zone.Remove(zone);
                context.Commit();
                return zone;
            }
        }

        [Route("GetByName/{name}")]
        [HttpGet]
        public IHttpActionResult GetByName(string name)
        {
            using (AuthContext context = new AuthContext())
            {
                var list = context.Peoples.Where(x => x.Email.ToLower().Contains(name.ToLower())).ToList();
                return Ok(list);
            }
        }

    }
}

