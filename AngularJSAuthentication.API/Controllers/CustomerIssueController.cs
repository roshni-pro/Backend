using AngularJSAuthentication.Model;
using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Web.Http;

namespace AngularJSAuthentication.API.Controllers
{
    [RoutePrefix("api/CustomerIssue")]
    public class CustomerIssueController : ApiController
    {

        private static TimeZoneInfo INDIAN_ZONE = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
        DateTime indianTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE);

        [Authorize]
        [Route("GetAll")]
        [HttpGet]
        public HttpResponseMessage Get()
        {
            using (AuthContext db = new AuthContext())
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

                    var custisu = db.CustomerIssuedb.Where(x => x.Deleted == false).ToList();
                    return Request.CreateResponse(HttpStatusCode.OK, custisu);
                }
                catch (Exception ex)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex.Message);
                }
            }
        }
        [HttpPost]
        [Route("Add")]
        public HttpResponseMessage add(CustomerIssue custise)
        {
            using (AuthContext db = new AuthContext())
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
                    custise.Status = "Pending";
                    custise.CreatedDate = indianTime;
                    custise.UpdatedDate = indianTime;
                    custise.Active = true;
                    custise.Deleted = false;
                    var peo = db.Peoples.Where(p => p.PeopleID == custise.PeopleID).FirstOrDefault();
                    if (peo != null)
                    {
                        custise.PeopleName = peo.DisplayName;
                    }
                    var cust = db.Customers.Where(p => p.CustomerId == custise.CustomerId).FirstOrDefault();
                    if (cust != null)
                    {
                        custise.ShopName = cust.ShopName;
                        custise.Mobile = cust.Mobile;
                    }
                    db.CustomerIssuedb.Add(custise);
                    db.Commit();

                    return Request.CreateResponse(HttpStatusCode.OK, custise);
                }
                catch (Exception ex)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex.Message);
                }
            }
        }

        [HttpPut]
        [Route("Update")]
        public HttpResponseMessage put(CustomerIssue custisue)
        {
            using (AuthContext db = new AuthContext())
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
                    var CI = db.CustomerIssuedb.Where(c => c.CS_id == custisue.CS_id && c.Deleted == false).SingleOrDefault();
                    if (CI != null)
                    {
                        CI.Issue = custisue.Issue;
                        CI.Status = custisue.Status;
                        CI.UpdatedDate = indianTime;

                        db.CustomerIssuedb.Attach(CI);
                        db.Entry(CI).State = EntityState.Modified;
                        db.Commit();
                    }
                    return Request.CreateResponse(HttpStatusCode.OK, CI);
                }
                catch (Exception ex)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex.Message);
                }
            }
        }
        [HttpDelete]
        [Route("Delete")]
        public HttpResponseMessage Remove(int CS_id)
        {
            using (AuthContext db = new AuthContext())
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
                    var CSI = db.CustomerIssuedb.Where(c => c.CS_id == CS_id && c.Deleted == false).SingleOrDefault();
                    if (CSI != null)
                    {
                        CSI.Active = false;
                        CSI.Deleted = true;
                        CSI.UpdatedDate = indianTime;
                        db.CustomerIssuedb.Attach(CSI);
                        db.Commit();
                    }
                    return Request.CreateResponse(HttpStatusCode.OK, CSI);
                }
                catch (Exception ex)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex.Message);
                }
            }
        }
    }
}