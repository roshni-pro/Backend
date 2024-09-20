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
    [RoutePrefix("api/Roles")]
    public class RolesController : ApiController
    {
       
        private static Logger logger = LogManager.GetCurrentClassLogger();

        //////////[Authorize]
        //////////////[Route("GetPeopleRole")]
        //////////////public IHttpActionResult GetPeopleRole()
        //////////////{
        //////////////    logger.Info("start Role: ");
        //////////////    List<Role> ass = new List<Role>();
        //////////////    try
        //////////////    {
        //////////////        var identity = User.Identity as ClaimsIdentity;
        //////////////        int compid = 1, userid = 0; string identityuserid = "";
        //////////////        // Access claims
        //////////////        foreach (Claim claim in identity.Claims)
        //////////////        {
        //////////////            if (claim.Type == "compid")
        //////////////            {
        //////////////                compid = int.Parse(claim.Value);
        //////////////            }
        //////////////            if (claim.Type == "userid")
        //////////////            {
        //////////////                userid = int.Parse(claim.Value);
        //////////////            }
        //////////////            if (claim.Type == "identityuserid")
        //////////////            {
        //////////////                identityuserid = claim.Value;
        //////////////            }
        //////////////        }

        //////////////        logger.Info("User ID : {0} , Company Id : {1}", compid, userid);
        //////////////        using (var context = new AuthContext())
        //////////////        {
        //////////////            string sqlquery = "Exec GetUserAllRoles '" + identityuserid + "'";
        //////////////            var UserRoleDc = context.Database.SqlQuery<RoleViewModel>(sqlquery).ToList();
        //////////////            //ass = context.AllRoles(compid).ToList();
        //////////////            return Ok(UserRoleDc);
        //////////////        }
        //////////////        logger.Info("End  roles: ");

        //////////////    }
        //////////////    catch (Exception ex)
        //////////////    {
        //////////////        logger.Error("Error in Role " + ex.Message);
        //////////////        logger.Info("End  Role: ");
        //////////////        return null;
        //////////////    }
        //////////////}

        //////////[Authorize]
        ////////[Route("")]
        ////////public IEnumerable<Role> Get()
        ////////{
        ////////    logger.Info("start Role: ");
        ////////    List<Role> ass = new List<Role>();
        ////////    try
        ////////    {
        ////////        var identity = User.Identity as ClaimsIdentity;
        ////////        int compid = 1, userid = 0;
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

        ////////        logger.Info("User ID : {0} , Company Id : {1}", compid, userid);
        ////////        using (var context = new AuthContext())
        ////////        {
        ////////            ass = context.AllRoles(compid).ToList();
        ////////        }
        ////////        logger.Info("End  roles: ");
        ////////        return ass;
        ////////    }
        ////////    catch (Exception ex)
        ////////    {
        ////////        logger.Error("Error in Role " + ex.Message);
        ////////        logger.Info("End  Role: ");
        ////////        return null;
        ////////    }
        ////////}changes done for people module


        //[Authorize]



        [Route("")]
        public IEnumerable<Role> Get()
        {
            logger.Info("start Role: ");
            List<Role> ass = new List<Role>();
            try
            {
                var identity = User.Identity as ClaimsIdentity;
                int compid = 1, userid = 0;
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
                using (var context = new AuthContext())
                {
                    ass = context.AllRoles(compid).ToList();
                }
                logger.Info("End  roles: ");
                return ass;
            }
            catch (Exception ex)
            {
                logger.Error("Error in Role " + ex.Message);
                logger.Info("End  Role: ");
                return null;
            }
        }


        //////[ResponseType(typeof(Role))]
        //////[Route("")]
        //////[AcceptVerbs("POST")]
        //////public Role add(Role item)
        //////{
        //////    logger.Info("start addRole: ");
        //////    try
        //////    {
        //////        var identity = User.Identity as ClaimsIdentity;
        //////        int compid = 0, userid=0;
        //////        // Access claims
        //////        foreach (Claim claim in identity.Claims)
        //////        {
        //////            if (claim.Type == "compid")
        //////            {
        //////                compid = int.Parse(claim.Value);
        //////            }
        //////            if (claim.Type == "userid")
        //////            {
        //////                userid = int.Parse(claim.Value);
        //////            }

        //////        }
        //////        item.CompanyId = compid;
        //////        if (item == null)
        //////        {
        //////            throw new ArgumentNullException("item");
        //////        }
        //////        logger.Info("User ID : {0} , Company Id : {1}", compid, userid);
        //////        using (var context = new AuthContext())
        //////        {
        //////            context.AddRole(item);
        //////        }
        //////        logger.Info("End  AddRole: ");
        //////        return item;
        //////    }
        //////    catch (Exception ex)
        //////    {
        //////        logger.Error("Error in AddRole " + ex.Message);
        //////        logger.Info("End  AddRole: ");
        //////        return null;
        //////    }
        //////}


        //[ResponseType(typeof(Role))]
        //[Route("")]
        //[AcceptVerbs("POST")]
        //public Role add(Role item)
        //{
        //    logger.Info("start addRole: ");
        //    try
        //    {
        //        var identity = User.Identity as ClaimsIdentity;
        //        int compid = 0, userid = 0;
        //        // Access claims
        //        foreach (Claim claim in identity.Claims)
        //        {
        //            if (claim.Type == "compid")
        //            {
        //                compid = int.Parse(claim.Value);
        //            }
        //            if (claim.Type == "userid")
        //            {
        //                userid = int.Parse(claim.Value);
        //            }

        //        }
        //        item.CompanyId = compid;
        //        if (item == null)
        //        {
        //            throw new ArgumentNullException("item");
        //        }
        //        logger.Info("User ID : {0} , Company Id : {1}", compid, userid);
        //        using (var context = new AuthContext())
        //        {
        //            context.AddRole(item);
        //        }
        //        logger.Info("End  AddRole: ");
        //        return item;
        //    }
        //    catch (Exception ex)
        //    {
        //        logger.Error("Error in AddRole " + ex.Message);
        //        logger.Info("End  AddRole: ");
        //        return null;
        //    }
        //}

        [ResponseType(typeof(Role))]
        [Route("")]
        [AcceptVerbs("POST")]
        public IHttpActionResult add(Role item)
        {
            using (var context = new AuthContext())
            {
                logger.Info("start addRole: ");
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

                    if (context.IsRoleExists(item))
                    {
                        return Ok("Already Exists");
                    }
                    else
                    {
                        context.AddRole(item);
                        return Ok(item);
                    }

                }
                catch (Exception ex)
                {
                    logger.Error("Error in AddRole " + ex.Message);
                    logger.Info("End  AddRole: ");
                    //return null;
                    return InternalServerError();
                }
            }
        }




        [ResponseType(typeof(Role))]
        [Route("")]
        [AcceptVerbs("PUT")]
        public Role Put(Role item)
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
                using (var context = new AuthContext())
                {
                    return context.PutRoles(item);
                }
            }
            catch
            {
                return null;
            }
        }


        [ResponseType(typeof(Role))]
        [Route("")]
        [AcceptVerbs("Delete")]
        public void Remove(string id)
        {
            logger.Info("start deleteRole: ");
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
                using (var context = new AuthContext())
                {
                    context.DeleteRole(Int32.Parse(id));
                }
                logger.Info("End  delete Role: ");
            }
            catch (Exception ex)
            {
                logger.Error("Error in deleteRole " + ex.Message);
            }
        }

        [ResponseType(typeof(Role))]
        [Route("DeleteV7")]
        [AcceptVerbs("Delete")]
        public Boolean DeleteV7(string id)
        {
            logger.Info("start deleteRole: ");
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
                using (var context = new AuthContext())
                {
                    context.DeleteRole(Int32.Parse(id));
                }
                logger.Info("End  delete Role: ");
                return true;
            }
            catch (Exception ex)
            {
                logger.Error("Error in deleteRole " + ex.Message);
                return false;
            }
        }
    }

    public class RoleViewModel
    {
        public string UserId { get; set; }
        public string RoleID { get; set; }
        public string RoleName { get; set; }
    }
}



