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
    [RoutePrefix("api/FinancialYear")]
    public class FinancialYearController : ApiController
    {

        private static Logger logger = LogManager.GetCurrentClassLogger();

        [Authorize]
        [Route("")]
        public IEnumerable<FinancialYear> Get()
        {
            logger.Info("start FinancialYear: ");
            List<FinancialYear> ass = new List<FinancialYear>();
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
                    ass = context.AllFinancialYear(compid).ToList();
                    logger.Info("End  ItemBrand: ");
                    return ass;
                }
                catch (Exception ex)
                {
                    logger.Error("Error in FinancialYear " + ex.Message);
                    logger.Info("End  FinancialYear: ");
                    return null;
                }
            }
        }


        [ResponseType(typeof(FinancialYear))]
        [Route("")]
        [AcceptVerbs("POST")]
        public FinancialYear add(FinancialYear item)
        {
            logger.Info("start FinancialYear: ");
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
                    item.CompanyId = compid;
                    if (item == null)
                    {
                        throw new ArgumentNullException("item");
                    }
                    logger.Info("User ID : {0} , Company Id : {1}", compid, userid);
                    context.AddFinancialYear(item);
                    logger.Info("End  FinancialYear: ");
                    return item;
                }
                catch (Exception ex)
                {
                    logger.Error("Error in Add FinancialYear " + ex.Message);
                    logger.Info("End  Add FinancialYear: ");
                    return null;
                }
            }
        }

        [ResponseType(typeof(FinancialYear))]
        [Route("")]
        [AcceptVerbs("PUT")]
        public FinancialYear Put(FinancialYear item)
        {
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
                    return context.PutFinancialYear(item);
                }
                catch
                {
                    return null;
                }
            }
        }


        [ResponseType(typeof(FinancialYear))]
        [Route("")]
        [AcceptVerbs("Delete")]
        public void Remove(int id)
        {
            logger.Info("start delete FinancialYear: ");
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
                    context.DeleteFinancialYear(id);
                    logger.Info("End  delete FinancialYear: ");
                }
                catch (Exception ex)
                {

                    logger.Error("Error in delete FinancialYear " + ex.Message);


                }
            }
        }
    }
}



