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
    [RoutePrefix("api/Countries")]
    public class CountryController : ApiController
    {
       
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public int PeopleID { get; private set; }

        [Authorize]
        [Route("")]
        public IEnumerable<Country> Get()
        {
            using (var context = new AuthContext())
            {

                logger.Info("start Country: ");
                List<Country> ass = new List<Country>();
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
                    ass = context.Allcountries().ToList();
                    logger.Info("End  Country: ");
                    return ass;
                }
                catch (Exception ex)
                {
                    logger.Error("Error in country " + ex.Message);
                    logger.Info("End  country: ");
                    return null;
                }
            }
        }





        //////[ResponseType(typeof(Country))]
        //////[Route("add")]
        //////[AcceptVerbs("POST")]
        //////public Country add(Country item)
        //////{
        //////    using (var context = new AuthContext() )
        //////    {
        //////        logger.Info("start addCountry: ");
        //////        try
        //////        {
        //////            var identity = User.Identity as ClaimsIdentity;
        //////            int compid = 0, userid = 0;
        //////            // Access claims
        //////            foreach (Claim claim in identity.Claims)
        //////            {
        //////                if (claim.Type == "compid")
        //////                {
        //////                    compid = int.Parse(claim.Value);
        //////                }
        //////                if (claim.Type == "userid")
        //////                {
        //////                    userid = int.Parse(claim.Value);
        //////                }
        //////            }
        //////            item.CompanyId = compid;
        //////            if (item == null)
        //////            {
        //////                throw new ArgumentNullException("item");
        //////            }
        //////            logger.Info("User ID : {0} , Company Id : {1}", compid, userid);
        //////            context.AddCountry(item);
        //////            logger.Info("End  AddCountry: ");
        //////            return item;
        //////        }
        //////        catch (Exception ex)
        //////        {
        //////            logger.Error("Error in AddCountry " + ex.Message);
        //////            logger.Info("End  AddCountry: ");
        //////            return null;
        //////        } 
        //////    }
        //////}
        ///


        [ResponseType(typeof(Country))]
        [Route("add")]
        [AcceptVerbs("POST")]
        public IHttpActionResult add(Country item)
        {
            using (var context = new AuthContext())
            {
                logger.Info("start addCountry: ");
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
                    if (context.IsCountryExists(item))
                    {
                        return Ok("Already Exists");
                    }
                    else
                    {
                        context.AddCountry(item);
                        return Ok(item);
                    }

                }
                catch (Exception ex)
                {
                    logger.Error("Error in AddCountry " + ex.Message);
                    logger.Info("End  AddCountry: ");
                    return InternalServerError();
                }
            }
        }


        [ResponseType(typeof(Country))]
        [Route("update")]
        [AcceptVerbs("PUT")]
        public Country Put(Country item)
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
                    return context.PutCountry(item);
                }
                catch
                {
                    return null;
                }
            }
        }



        #region get Agent and Dbay Devicehistory

        [Route("GetActiveAgentsForCuntry")]
        [HttpGet]
        public dynamic AgentnDboyDevicehistory()
        {
           
            using (var odd = new AuthContext())
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
                    logger.Info("User ID : {0} , Company Id : {1}", compid, userid);

                    var data = odd.Peoples.Where(x => x.Active == true && x.Deleted == false).Select(x => new { x.DisplayName, x.PeopleID }).ToList();
                    return data;
                }
                catch (Exception ex)
                {
                    return false;
                }
            }
        }
        #endregion








        [Route("Remove")]
        [HttpPost]
        public void Remove(int id)
        {
            using (var context = new AuthContext())
            {
                logger.Info("start deleteCityy: ");
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
                    context.DeleteCountry(id);
                    // context.DeleteCountry(CountryId);
                    logger.Info("End  delete Country: ");
                }
                catch (Exception ex)
                {

                    logger.Error("Error in deleteCountry" + ex.Message);


                }
            }
        }


        [Route("RemoveV7")]
        [HttpDelete]
        public Boolean RemoveV7(int id)
        {
            using (var context = new AuthContext())
            {
                logger.Info("start deleteCityy: ");
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
                    context.DeleteCountry(id);
                    // context.DeleteCountry(CountryId);
                    logger.Info("End  delete Country: ");
                    return true;
                }
                catch (Exception ex)
                {

                    logger.Error("Error in deleteCountry" + ex.Message);
                    return false;

                }
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

