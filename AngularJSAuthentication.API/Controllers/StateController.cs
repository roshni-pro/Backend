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
    [RoutePrefix("api/States")]
    public class StateController : ApiController
    {
       
        private static Logger logger = LogManager.GetCurrentClassLogger();


        [Authorize]
        [Route("")]
        public IEnumerable<State> Get()
        {

            logger.Info("start State: ");
            List<State> ass = new List<State>();
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
                using (var db = new AuthContext())
                {
                    ass = db.Allstates().ToList();
                }
                logger.Info("End  Stste: ");
                return ass;
            }
            catch (Exception ex)
            {
                logger.Error("Error in state " + ex.Message);
                logger.Info("End  state: ");
                return null;
            }
        }


        //////////[ResponseType(typeof(State))]
        //////////[Route("")]
        //////////[AcceptVerbs("POST")]
        //////////public State add(State item)
        //////////{
        //////////    logger.Info("start addState: ");
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
        //////////        item.CompanyId = compid;
        //////////        if (item == null)
        //////////        {
        //////////            throw new ArgumentNullException("item");
        //////////        }
        //////////        logger.Info("User ID : {0} , Company Id : {1}", compid, userid);
        //////////        using (var db = new AuthContext())
        //////////        {
        //////////            db.AddState(item);
        //////////        }
        //////////        logger.Info("End  AddState: ");
        //////////        return item;
        //////////    }
        //////////    catch (Exception ex)
        //////////    {
        //////////        logger.Error("Error in AddState " + ex.Message);
        //////////        logger.Info("End  AddState: ");
        //////////        return null;
        //////////    }
        //////////}temp comment



        [ResponseType(typeof(State))]
        [Route("")]
        [AcceptVerbs("POST")]
        public IHttpActionResult add(State item)
        {
            using (var context = new AuthContext())
            {
                logger.Info("start addState: ");
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

                    if (context.IsStateExists(item))
                    {
                        return Ok("Already Exists");
                    }
                    else
                    {
                        context.AddState(item, userid);
                        return Ok(item);
                    }

                }
                catch (Exception ex)
                {
                    logger.Error("Error in AddState " + ex.Message);
                    logger.Info("End  AddState: ");
                    return null;
                }
            }
        }


        [ResponseType(typeof(State))]
        [Route("")]
        [AcceptVerbs("PUT")]
        public State Put(State item)
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
                using (var db = new AuthContext())
                {
                    return db.PutState(item);
                }
            }
            catch
            {
                return null;
            }
        }


        [ResponseType(typeof(State))]
        [Route("")]
        [AcceptVerbs("Delete")]
        public void Remove(int id)
        {
            logger.Info("start deleteState: ");
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
                using (var db = new AuthContext())
                {
                    db.DeleteState(id);
                }
                logger.Info("End  delete State: ");
            }
            catch (Exception ex)
            {

                logger.Error("Error in deleteSTate " + ex.Message);


            }
        }

        [ResponseType(typeof(State))]
        [Route("DeleteV7")]
        [AcceptVerbs("Delete")]
        public Boolean DeleteV7(int id)
        {
            logger.Info("start deleteState: ");
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
                using (var db = new AuthContext())
                {
                    db.DeleteState(id);
                }
                logger.Info("End  delete State: ");
                return true;
            }
            catch (Exception ex)
            {

                logger.Error("Error in deleteSTate " + ex.Message);
                return false;

            }
        }

        #region get the supplier State
        /// <summary>
        /// get supplier State
        /// Created Date:18/06/2019
        /// Created By Raj
        /// </summary>
        /// <returns></returns>
        [Route("supplierstate")]
        [HttpGet]
        public IEnumerable<State> Getsupplierstate()
        {
            logger.Info("start State: ");
            List<State> ass = new List<State>();
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
                using (var db = new AuthContext())
                {
                    logger.Info("User ID : {0} , Company Id : {1}", compid, userid);
                    ass = db.States.Where(x => x.Deleted == false && x.IsSupplier == true).ToList();
                    logger.Info("End  City: ");
                    return ass;
                }
            }
            catch (Exception ex)
            {
                logger.Error("Error in State " + ex.Message);
                logger.Info("End  State: ");
                return null;
            }
        }
        #endregion


        [AllowAnonymous]
        [Route("GetV7")]
        public IEnumerable<State> GetV7()
        {

            using (var context = new AuthContext())
            {
                logger.Info("start State: ");
                List<State> ass = new List<State>();
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
                    ass = context.Allstates().OrderBy(x => x.StateName).ToList();
                    logger.Info("End  Stste: ");
                    return ass;
                }
                catch (Exception ex)
                {
                    logger.Error("Error in state " + ex.Message);
                    logger.Info("End  state: ");
                    return null;
                }
            }
        }

        [AllowAnonymous]
        [Route("GetstatebyCity")]
        public dynamic GetstatebyCity(int StateId)
        {

            using (var context = new AuthContext())
            {
                try
                {
                    var ass = context.Cities.Where(x => x.Stateid == StateId && x.Deleted == false && x.active == true).OrderBy(x => x.CityName).ToList();
                    return ass;
                }
                catch (Exception ex)
                {
                    return null;
                }
            }
        }

        ////////[ResponseType(typeof(State))]
        ////////[Route("addV7")]
        ////////[AcceptVerbs("POST")]
        ////////public State addV7(State item)
        ////////{
        ////////    logger.Info("start addState: ");
        ////////    try
        ////////    {
        ////////        var identity = User.Identity as ClaimsIdentity;
        ////////        int compid = 0, userid=0;
        ////////        // Access claims
        ////////        foreach (Claim claim in identity.Claims)
        ////////        {
        ////////            if (claim.Type == "compid")
        ////////            {
        ////////                compid = int.Parse(claim.Value);
        ////////            }
        ////////            if (claim.Type == "userid")
        ////////            {
        ////////                userid = int.Parse(claim.Value);
        ////////            }
        ////////        }
        ////////        item.CompanyId = compid;
        ////////        if (item == null)
        ////////        {
        ////////            throw new ArgumentNullException("item");
        ////////        }
        ////////        logger.Info("User ID : {0} , Company Id : {1}", compid, userid);
        ////////        context.AddState(item);
        ////////        logger.Info("End  AddState: ");
        ////////        return item;
        ////////    }
        ////////    catch (Exception ex)
        ////////    {
        ////////        logger.Error("Error in AddState " + ex.Message);
        ////////        logger.Info("End  AddState: "); 
        ////////        return null;
        ////////    }
        ////////}Temp Comment


        [ResponseType(typeof(State))]
        [Route("addV7")]
        [AcceptVerbs("POST")]
        public IHttpActionResult addV7(State item)
        {
            using (var context = new AuthContext())
            {
                logger.Info("start addState: ");
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
                    if (context.IsStateExists(item))
                    {
                        return Ok("Already Exists");
                    }
                    else
                    {
                        context.AddState(item, userid);
                        return Ok(item);
                    }

                }
                catch (Exception ex)
                {
                    logger.Error("Error in AddState " + ex.Message);
                    logger.Info("End  AddState: ");
                    //return null;
                    return InternalServerError();
                }
            }
        }






        [ResponseType(typeof(State))]
        [Route("addPutV7")]
        [AcceptVerbs("PUT")]
        public State addPutV7(State item)
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
                    //logger.Info("User ID : {0} , Company Id : {1}", compid, userid);
                    return context.PutStates(item,userid);
                }
                catch
                {
                    return null;
                }
            }
        }




        #region get Agent and Dbay Devicehistory
        /// <summary>
        /// tejas 28-05-2019
        /// </summary>
        /// <param name="CustomerId"></param>
        /// <returns></returns>
        [Route("GetActiveAgentsForStateV7")]
        [HttpGet]
        public dynamic GetActiveAgentsForStateV7()
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



        [ResponseType(typeof(State))]
        [Route("RemoveV7")]
        [AcceptVerbs("Delete")]
        public void RemoveV7(int id)
        {
            using (var context = new AuthContext())
            {
                logger.Info("start deleteState: ");
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
                    context.DeleteState(id);
                    logger.Info("End  delete State: ");
                }
                catch (Exception ex)
                {

                    logger.Error("Error in deleteSTate " + ex.Message);


                }
            }
        }
    }
}



