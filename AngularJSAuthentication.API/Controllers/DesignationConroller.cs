using AngularJSAuthentication.Model;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Web.Http;
using System.Web.Http.Description;

namespace AngularJSAuthentication.API.Controllers
{
    [RoutePrefix("api/Designation")]
    public class DesignationController : ApiController
    {
       
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private static TimeZoneInfo INDIAN_ZONE = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
        DateTime indianTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE);

        // [Authorize]
        [Route("")]
        [HttpGet]
        public List<Designation> Get()
        {

            logger.Info("start Designation: ");
            List<Designation> ass = new List<Designation>();
            using (AuthContext Ac = new AuthContext())
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
                    //ass = context.getDesignations(, string UserName, string DesignationType, string Department) .ToList();
                    ass = Ac.DesignationsDB.ToList();
                    logger.Info("End  Designation: ");
                    return ass;
                }
                catch (Exception ex)
                {
                    logger.Error("Error in Designation " + ex.Message);
                    logger.Info("End  Designation: ");
                    return null;
                }
            }
        }

        [Route("DesignationName")]
        [HttpGet]
        public HttpResponseMessage CheckDesignation(string DesignationName)
        {
            using (AuthContext db = new AuthContext())
            {
                try
                {
                    logger.Info("Get Peoples: ");
                    int compid = 0, userid = 0;
                    int Warehouse_id = 0;
                    string Designation = "";
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
                        if (claim.Type == "DesignationName")
                        {
                            DesignationName = claim.Value;
                        }
                    }
                    logger.Info("End Get Company: ");
                    if (Warehouse_id > 0)
                    {
                        var RDDesignationName = db.DesignationsDB.Where(x => x.DesignationName == DesignationName).FirstOrDefault();

                        return Request.CreateResponse(HttpStatusCode.OK, RDDesignationName);
                    }
                    else
                    {
                        var RDDesignationName = db.DesignationsDB.Where(x => x.DesignationName == DesignationName).FirstOrDefault();

                        return Request.CreateResponse(HttpStatusCode.OK, RDDesignationName);


                    }

                }
                catch (Exception ex)
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, false);
                }
            }
        }


        [ResponseType(typeof(Designation))]
        [Route("")]

        [AcceptVerbs("POST")]
        public Designation add(Designation item)
        {
            logger.Info("start addDesignation: ");
            using (AuthContext Ac = new AuthContext())
            {
                try
                {
                    Designation ad = Ac.DesignationsDB.Where(X => X.DesignationName == item.DesignationName).FirstOrDefault();
                    if (ad != null)
                    {
                        return null;
                    }


                    Designation att = new Designation();
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

                    att.DesignationName = item.DesignationName;
                    att.Description = item.Description;
                    att.Level = item.Level;
                    att.CreatedDate = indianTime;
                    att.UpdatedDate = indianTime;
                    att.CreatedBy = item.CreatedBy;
                    att.UpdateBy = item.UpdateBy;
                    att.Deleted = false;
                    att.CompanyId = compid;
                    Ac.DesignationsDB.Add(att);
                    Ac.Commit();

                    return att;

                }
                catch (Exception ex)
                {
                    logger.Error("Error in AddDesignation " + ex.Message);
                    logger.Info("End  AddDesignation: ");
                    return null;
                }
            }
        }

        [ResponseType(typeof(Designation))]
        [Route("cv")]
        [AcceptVerbs("PUT")]
        [HttpPut]
        public Designation Put(Designation item)
        {
            using (AuthContext Ac = new AuthContext())
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
                    Designation ad = Ac.DesignationsDB.Where(X => X.DesignationName == item.DesignationName && X.Designationid != item.Designationid).FirstOrDefault();
                    if (ad != null)
                    {
                        return null;
                    }
                    ad = Ac.DesignationsDB.Where(X => X.Designationid == item.Designationid).FirstOrDefault();
                    ad.DesignationName = item.DesignationName;
                    ad.Level = item.Level;
                    ad.Description = item.Description;
                    ad.CompanyId = item.CompanyId;
                    Ac.Commit();
                    return ad;

                }
                catch (Exception ex)
                {
                    logger.Error("Error in PutDesignation " + ex.Message);
                    logger.Info("End  PutDesignation: ");
                    return null;
                }
            }

        }

        [ResponseType(typeof(Designation))]
        [Route("")]
        [AcceptVerbs("Delete")]
        [HttpDelete]
        public void Remove(int id)
        {

            logger.Info("start deleteDesignation: ");
            using (AuthContext Ac = new AuthContext())
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
                    Designation aaa = Ac.DesignationsDB.Where(x => x.Designationid == id).SingleOrDefault();
                    Ac.DesignationsDB.Remove(aaa);
                    Ac.Commit();

                    logger.Info("End  delete Designation: ");
                }
                catch (Exception ex)
                {

                    logger.Error("Error in deleteDesignation " + ex.Message);


                }
            }
        }

        [ResponseType(typeof(Designation))]
        [Route("DeleteV7")]
        [AcceptVerbs("Delete")]
        [HttpDelete]
        public Boolean DeleteV7(int id)
        {

            logger.Info("start deleteDesignation: ");
            using (AuthContext Ac = new AuthContext())
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
                    Designation aaa = Ac.DesignationsDB.Where(x => x.Designationid == id).SingleOrDefault();
                    Ac.DesignationsDB.Remove(aaa);
                    Ac.Commit();

                    logger.Info("End  delete Designation: ");
                    return true;
                }
                catch (Exception ex)
                {

                    logger.Error("Error in deleteDesignation " + ex.Message);
                    return false;

                }
            }
        }


    }
}

