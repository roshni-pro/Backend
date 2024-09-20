using AngularJSAuthentication.Model;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Web.Http;

namespace AngularJSAuthentication.API.Controllers
{
    [RoutePrefix("api/Projects")]
    public class ProjectController : ApiController
    {
       
        private static Logger logger = LogManager.GetCurrentClassLogger();

        [Authorize]
        [Route("")]
        public IEnumerable<CaseProject> Get()
        {

            logger.Info("start Project: ");
            List<CaseProject> ass = new List<CaseProject>();
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

                    logger.Info("User ID : {0} , Company Id : {1}", compid, userid);
                    ass = context.AllProject().ToList();
                    logger.Info("End  Case: ");
                    return ass;
                }
                catch (Exception ex)
                {
                    logger.Error("Error in Project " + ex.Message);
                    logger.Info("End  Project: ");
                    return null;
                }
            }
        }


        [Route("GetIssueCategory")]
        [HttpGet]
        public IEnumerable<IssueCategory> GetIssueCategory()
        {

            logger.Info("start Project: ");
            List<IssueCategory> ass = new List<IssueCategory>();
            using (var db = new AuthContext())
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
                    ass = db.IssueCategoryDB.ToList();
                    logger.Info("End  Case: ");
                    return ass;
                }
                catch (Exception ex)
                {
                    logger.Error("Error in Project " + ex.Message);
                    logger.Info("End  Project: ");
                    return null;
                }
            }
        }

        [Route("GetIssueSubCategory")]
        [HttpGet]
        public IEnumerable<IssueSubCategory> GetIssueSubCategory(int CaseProjectId)
        {

            logger.Info("start Project: ");
            List<IssueSubCategory> ass = new List<IssueSubCategory>();
            using (var db = new AuthContext())
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
                    ass = db.IssueSubCategoryDB.Where(x => x.IssueCategoryId == CaseProjectId).ToList();
                    logger.Info("End  Case: ");
                    return ass;
                }
                catch (Exception ex)
                {
                    logger.Error("Error in Project " + ex.Message);
                    logger.Info("End  Project: ");
                    return null;
                }
            }
        }
    }
}
