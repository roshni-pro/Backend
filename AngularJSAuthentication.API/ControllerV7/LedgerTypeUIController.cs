using System.Linq;
using System.Security.Claims;
using System.Web.Http;

namespace AngularJSAuthentication.API.ControllerV7
{
    [RoutePrefix("api/LedgerTypeUI")]
    public class LedgerTypeUIController : ApiController
    {
        int compid = 0, userid = 0;
        public LedgerTypeUIController()
        {
            using (var authContext = new AuthContext())
            {

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
                }
            }
        }

        [Route("GetAll")]
        [HttpGet]
        public IHttpActionResult GetAll()
        {
            using (var authContext = new AuthContext())
            {
                var ledgerTypeList = authContext.LadgerTypeDB.ToList();
                return Ok(ledgerTypeList);
            }

        }

    }
}
