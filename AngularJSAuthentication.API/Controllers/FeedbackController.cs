using AngularJSAuthentication.API.Controllers.Base;
using AngularJSAuthentication.Model;
using NLog;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Web.Http;

namespace AngularJSAuthentication.API.Controllers
{
    [RoutePrefix("api/feedback")]
    public class FeedbackController : BaseAuthController
    {

        private static Logger logger = LogManager.GetCurrentClassLogger();

        [Authorize]
        [Route("")]
        [AcceptVerbs("GET")]
        public HttpResponseMessage Get()
        {
            using (AuthContext db = new AuthContext())
            {
                try
                {
                    var identity = User.Identity as ClaimsIdentity;
                    int compid = 0, userid = 0;     // Access claims
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
                    var item = db.Feedbacks.Where(x => x.CompanyId == compid).AsEnumerable();
                    return Request.CreateResponse(HttpStatusCode.OK, item);
                }
                catch (Exception ex)
                {
                    logger.Error("Error in add feedBack " + ex.Message);
                    logger.Info("End  addCity: ");
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex.Message);
                }
            }
        }

        [Route("")]
        [AcceptVerbs("POST")]
        public HttpResponseMessage add(Feedback item)
        {
            logger.Info("start add RequestItem: ");
            using (AuthContext context = new AuthContext())
            {
                try
                {
                    var identity = User.Identity as ClaimsIdentity;
                    int compid = 0, userid = 0;   // Access claims
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
                    item = context.AddFeedBack(item);
                    logger.Info("End add RequestItem: ");
                    return Request.CreateResponse(HttpStatusCode.OK, item);
                }
                catch (Exception ex)
                {
                    logger.Error("Error in add RequestItem " + ex.Message);
                    logger.Info("End  RequestItem: ");
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex.Message);
                }
            }
        }

    }
}