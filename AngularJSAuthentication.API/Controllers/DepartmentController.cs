using AngularJSAuthentication.API.Controllers.Base;
using AngularJSAuthentication.DataContracts.APIParams;
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
    [RoutePrefix("api/Department")]
    public class DepartmentController : BaseAuthController
    {
        
        private static Logger logger = LogManager.GetCurrentClassLogger();
        [HttpGet]
        [Route("GetDepartment")]
        public IEnumerable<Department> Get()
        {

            logger.Info("start State: ");
            List<Department> ass = new List<Department>();
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

                    logger.Info("User ID : {0} , Company Id : {1}", compid, userid);
                    ass = context.Alldepartment().ToList();
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
        // GET api/values/5
        [Route("GetById")]
        [HttpGet]
        public Department Get(int? id)
        {
            using (AuthContext db = new AuthContext())
            {
                return db.Departments.Find(id);
                //return "value";
            }
        }
        // POST api/values
        [ResponseType(typeof(Department))]
        [Route("Post")]
        [AcceptVerbs("POST")]
        [HttpPost]
        public void Post(Department Data)
        {
            using (AuthContext db = new AuthContext())
            {
                try
                {
                    Department obj = new Department();
                    obj.DepName = Data.DepName;
                    obj.Description = Data.Description;




                    db.Departments.Add(obj);
                    db.Commit();
                }
                catch (Exception ex)
                {
                    logger.Error("Error in Department " + ex.Message);

                }
            }
        }

        //////[ResponseType(typeof(Department))]
        //////[Route("PostV7")]
        //////[AcceptVerbs("POST")]
        //////[HttpPost]
        //////public IHttpActionResult PostV7(Department Data)
        //////{
        //////    using (AuthContext db = new AuthContext())
        //////    {
        //////        try
        //////        {
        //////            Department obj = new Department();
        //////            obj.DepName = Data.DepName;
        //////            obj.Description = Data.Description;




        //////            db.Departments.Add(obj);
        //////            db.SaveChanges();
        //////        }
        //////        catch (Exception ex)
        //////        {
        //////            logger.Error("Error in Department " + ex.Message);

        //////        }
        //////    }
        //////    return Ok();
        //////}TEMP COMMENT
        ///.
        ///


        [ResponseType(typeof(Department))]
        [Route("PostV7")]
        [AcceptVerbs("POST")]
        [HttpPost]
        public IHttpActionResult PostV7(Department Data)
        {
            using (AuthContext db = new AuthContext())
            {
                try
                {
                    var Department = db.Departments.Where(c => c.DepName == Data.DepName).FirstOrDefault();
                    Department obj = new Department();
                    if (Department == null)
                    {
                        obj.DepName = Data.DepName;
                        obj.Description = Data.Description;
                        db.Departments.Add(obj);
                        db.Commit();
                        return Ok(obj);

                    }
                    else
                    {
                        return Ok("Data Already Exist");
                    }
                }
                catch (Exception ex)
                {
                    logger.Error("Error in Department " + ex.Message);
                    return InternalServerError();
                }
            }
            return Ok();
        }




        ////[Route("Put")]
        ////[HttpPut]

        ////public IHttpActionResult Put(Department d)
        ////{
        ////    if (!ModelState.IsValid)
        ////        return BadRequest("Not a valid model");

        ////    using (var ctx = new AuthContext())
        ////    {
        ////        var ExistingSkill = ctx.Departments.Where(s => s.DepId == d.DepId)
        ////                                                .FirstOrDefault<Department>();

        ////        if (ExistingSkill != null)
        ////        {

        ////            ExistingSkill.DepName = d.DepName;
        ////            ExistingSkill.Description = d.Description;

        ////            ctx.SaveChanges();
        ////        }
        ////        else
        ////        {
        ////            return NotFound();
        ////        }
        ////    }

        ////    return Ok();
        ////}


        [Route("Put")]
        [HttpPut]

        public IHttpActionResult Put(Department d)
        {
            if (!ModelState.IsValid)
                return BadRequest("Not a valid model");

            using (var ctx = new AuthContext())
            {
                var ExistingSkill = ctx.Departments.Where(s => s.DepId == d.DepId)
                                                        .FirstOrDefault<Department>();

                var ExistingSkills = ctx.Departments.Where(c => c.DepName == d.DepName).FirstOrDefault();
                if (ExistingSkills == null)
                {


                    ExistingSkill.DepName = d.DepName;
                    ExistingSkill.Description = d.Description;

                    ctx.Commit();
                    return Ok(ExistingSkill);
                }
                else
                {
                    return Ok("Data is Already Exists!");
                }
            }


        }

        [Route("DELETEById")]
        [AcceptVerbs("Delete")]
        public void Delete(int id)
        {
            logger.Info("start deleteState: ");
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
                    context.DeleteDepartment(id);
                    logger.Info("End  delete State: ");
                }
                catch (Exception ex)
                {

                    logger.Error("Error in deleteSTate " + ex.Message);


                }
            }
        }


        [Route("DeleteV7")]
        [AcceptVerbs("Delete")]
        public Boolean DeleteV7(int id)
        {
            logger.Info("start deleteState: ");
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
                    context.DeleteDepartment(id);
                    logger.Info("End  delete State: ");
                    return true;
                }
                catch (Exception ex)
                {

                    logger.Error("Error in deleteSTate " + ex.Message);
                    return false;

                }
            }
        }

        [HttpGet]
        [Route("GeAll")]
        public List<DropDown> GeAll()
        {
            using (AuthContext context = new AuthContext())
            {
                var list = context.Departments
                     .Where(x => x.Deleted != true)
                     .Select(x => new DropDown
                     {
                         ID = x.DepId,
                         Label = x.DepName
                     })
                     .ToList();
                return list;
            }
        }
    }
}
