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
    [RoutePrefix("api/Skill")]

    public class SkillController : ApiController
    {       
        private static Logger logger = LogManager.GetCurrentClassLogger();
        [HttpGet]
        [Route("GetSkill")]

        public IEnumerable<Skill> Get()
        {

            logger.Info("start skill: ");
            List<Skill> ass = new List<Skill>();
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
                using (var context = new AuthContext())
                {
                    ass = context.AllSkill().ToList();
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
        // GET api/values/5
        [Route("GetSkillid")]
        [HttpGet]
        public Skill GetSkillbyId(int? id)
        {
            using (var db = new AuthContext())
            {
                return db.Skills.Find(id);
            }
        }
        //return "value";

        // POST api/values
        [ResponseType(typeof(Skill))]
        [Route("Post")]
        [AcceptVerbs("POST")]
        [HttpPost]
        public void Post(Skill S)
        {
            try
            {
                Skill obj = new Skill();
                obj.Name = S.Name;

                using (var db = new AuthContext())
                {

                    db.Skills.Add(obj);
                    db.Commit();
                }
            }
            catch (Exception ex)
            {
                logger.Error("Error in Skill " + ex.Message);

            }
        }

        //////[ResponseType(typeof(Skill))]
        //////[Route("PostV7")]
        //////[AcceptVerbs("POST")]
        //////[HttpPost]
        //////public IHttpActionResult PostV7(Skill S)
        //////{
        //////    try
        //////    {
        //////        Skill obj = new Skill();
        //////        obj.Name = S.Name;

        //////        using (var db = new AuthContext())
        //////        {

        //////            db.Skills.Add(obj);
        //////            db.SaveChanges();
        //////        }
        //////    }
        //////    catch (Exception ex)
        //////    {
        //////        logger.Error("Error in Skill " + ex.Message);

        //////    }

        //////    return Ok("Data Already Exist");
        //////}

        [ResponseType(typeof(Skill))]
        [Route("PostV7")]
        [AcceptVerbs("POST")]
        [HttpPost]
        public IHttpActionResult PostV7(Skill S)
        {
            using (var context = new AuthContext())
            {
                try
                {
                    var Skill = context.Skills.Where(c => c.Name == S.Name).FirstOrDefault();
                    Skill obj = new Skill();
                    if (Skill == null)
                    {
                        obj.Name = S.Name;
                        context.Skills.Add(obj);
                        context.Commit();
                        // return Ok("Data Already Exist");
                        return Ok(obj);
                    }
                    else
                    {

                        return Ok("Data Already Exist");

                    }
                }
                catch (Exception ex)
                {
                    logger.Error("Error in Skill " + ex.Message);
                    return InternalServerError();
                }
            }


        }


        //////[Route("Put")]
        //////[HttpPut]
        //////public IHttpActionResult Put(Skill sl)
        //////{
        //////    if (!ModelState.IsValid)
        //////        return BadRequest("Not a valid model");

        //////    using (var ctx = new AuthContext())
        //////    {
        //////        var ExistingSkill = ctx.Skills.Where(s => s.SkillId == sl.SkillId)
        //////                                                .FirstOrDefault<Skill>();

        //////        if (ExistingSkill != null)
        //////        {

        //////            ExistingSkill.Name = sl.Name;

        //////            ctx.SaveChanges();
        //////        }
        //////        else
        //////        {
        //////            return NotFound();
        //////        }
        //////    }

        //////    return Ok();
        //////}temp comment



        [Route("Put")]
        [HttpPut]
        public IHttpActionResult Put(Skill sl)
        {
            if (!ModelState.IsValid)
                return BadRequest("Not a valid model");

            using (var ctx = new AuthContext())
            {
                var ExistingSkill = ctx.Skills.Where(s => s.SkillId == sl.SkillId)
                                                        .FirstOrDefault<Skill>();


                var ExistingSkills = ctx.Skills.Where(c => c.Name == sl.Name).FirstOrDefault();
                if (ExistingSkills == null)
                {

                    ExistingSkill.Name = sl.Name;

                    ctx.Commit();
                    return Ok(ExistingSkill);
                }
                else
                {
                    //return NotFound();
                    return Ok("Data Already Exist");
                }
            }


        }



        //[Route("PUT")]
        //[HttpPut]
        //public IHttpActionResult Put(Skill sl)
        //{
        //    if (!ModelState.IsValid)
        //        return BadRequest("Not a valid model");

        //    using (var ctx = new AuthContext())
        //    {
        //        var ExistingSkill = ctx.Skills.Where(s => s.SkillId == sl.SkillId)
        //                                                .FirstOrDefault<Skill>();

        //        if (ExistingSkill != null)
        //        {

        //            ExistingSkill.Name = sl.Name;

        //            ctx.SaveChanges();
        //        }
        //        else
        //        {
        //            return NotFound();
        //        }
        //    }

        //    return Ok();
        //}



        [ResponseType(typeof(Skill))]
        [Route("DELETEById")]
        [AcceptVerbs("Delete")]
        public void Delete(int id)
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
                    db.DeleteSkill(id);
                }
                logger.Info("End  delete State: ");
            }
            catch (Exception ex)
            {

                logger.Error("Error in deleteSTate " + ex.Message);


            }
        }


        [ResponseType(typeof(Skill))]
        [Route("")]
        [AcceptVerbs("Delete")]
        public void Remove(int id)
        {
            using (AuthContext context = new AuthContext())
            {

                logger.Info("start deleteSkill: ");
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
                    context.DeleteSkill(id);
                    logger.Info("End  delete Skill: ");
                }
                catch (Exception ex)
                {

                    logger.Error("Error in deleteSkill" + ex.Message);


                }
            }
        }

        [ResponseType(typeof(Skill))]
        [Route("DELETEV7")]
        [AcceptVerbs("Delete")]
        public Boolean DELETEV7(int id)
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
                    db.DeleteSkill(id);
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
    }
}


