using AngularJSAuthentication.Model;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Web.Http;
using System.Web.Http.Description;


namespace AngularJSAuthentication.API.ControllerV7
{
    [RoutePrefix("api/Regions")]
    public class RegionController : ApiController
    {
     
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public int PeopleID { get; private set; }

        [Authorize]
        [Route("")]
        public IEnumerable<RegionZone> Get()
        {
            using (var context = new AuthContext())
            {

                logger.Info("start Region: ");
                List<RegionZone> ass = new List<RegionZone>();
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
                    ass = context.Allregion().ToList();
                    logger.Info("End  Region: ");
                    return ass;
                }
                catch (Exception ex)
                {
                    logger.Error("Error in region " + ex.Message);
                    logger.Info("End  region: ");
                    return null;
                }
            }
        }





        //////////[ResponseType(typeof(RegionZone))]
        //////////[Route("add")]
        //////////[AcceptVerbs("POST")]
        //////////public IHttpActionResult add(RegionZone item)
        //////////{
        //////////    logger.Info("start addRegion: ");
        //////////    try
        //////////    {
        //////////        var identity = User.Identity as ClaimsIdentity;
        //////////        int compid = 0, userid = 0;
        //////////        // Access claims
        //////////        foreach (Claim claim in identity.Claims)
        //////////        {
        //////////            if (claim.Type == "compid")
        //////////            {
        //////////                compid = int.Parse(claim.Value);
        //////////            }
        //////////            if (claim.Type == "userid")
        //////////            {
        //////////                userid = int.Parse(claim.Value);
        //////////            }
        //////////        }

        //////////        if (context.IsRegionExists(item))
        //////////        {
        //////////            return Ok("Already Exists");
        //////////        }
        //////////        else
        //////////        {
        //////////            context.AddRegion(item);
        //////////            return Ok(item);
        //////////        }

        //////////    }
        //////////    catch (Exception ex)
        //////////    {
        //////////        logger.Error("Error in AddRegion " + ex.Message);
        //////////        logger.Info("End  AddRegion: ");
        //////////        return InternalServerError();
        //////////    }
        //////////}


        [ResponseType(typeof(RegionZone))]
        [Route("add")]
        [AcceptVerbs("POST")]
        public IHttpActionResult add(RegionZone item)
        {
            using (var context = new AuthContext())
            {
                logger.Info("start addRegion: ");
                try
                {
                    var identity = User.Identity as ClaimsIdentity;
                    int compid = 0, userid = 0;
                    //Access claims
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
                    if (context.IsRegionExists(item))
                    {
                        return Ok("Already Exists");
                    }
                    else
                    {
                        context.AddRegion(item);
                        return Ok(item);
                    }

                }
                catch (Exception ex)
                {
                    logger.Error("Error in AddRegion " + ex.Message);
                    logger.Info("End  AddRegion: ");
                    return null;
                }
            }
        }



        [ResponseType(typeof(RegionZone))]
        [Route("update")]
        [AcceptVerbs("PUT")]
        public RegionZone Put(RegionZone item)
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
                using (AuthContext db = new AuthContext())
                {
                    return db.PutRegion(item);
                }
            }
            catch
            {
                return null;
            }
        }



        #region get Agent and Dbay Devicehistory

        [Route("GetActiveAgentsForRegion")]
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
                    var data = db.Peoples.Where(x => x.Active == true).Select(x => new { x.DisplayName, x.PeopleID }).ToList();
                    return data;
                }
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        #endregion








        [Route("Remove")]
        [HttpPost]
        public void Remove(int id)
        {
            logger.Info("start deleteRegion: ");
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
                using (AuthContext db = new AuthContext())
                {
                    db.DeleteRegion(id);
                }
            }
            catch (Exception ex)
            {

                logger.Error("Error in deleteRegion" + ex.Message);


            }
        }


        [Route("RemoveV7")]
        [HttpDelete]
        public Boolean RemoveV7(int id)
        {
            logger.Info("start deleteRegion: ");
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
                using (AuthContext db = new AuthContext())
                {
                    db.DeleteRegion(id);

                    return true;
                }
            }
            catch (Exception ex)
            {

                logger.Error("Error in deleteRegion" + ex.Message);
                return false;

            }
        }

        [Route("GetByName/{name}")]
        [HttpGet]
        public IHttpActionResult GetByName(string name)
        {
            using (AuthContext context = new AuthContext())
            {
                context.Peoples.Where(x => x.Email.ToLower().Contains(name.ToLower())).ToList();
                return Ok(name);
            }
        }


    }
}


