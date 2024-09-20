using System.Linq;
using System.Security.Claims;
using System.Web.Http;

namespace AngularJSAuthentication.API.ControllerV7
{
    [RoutePrefix("api/VoucherType")]
    public class VoucherTypeUIController : ApiController
    {
        int compid = 0, userid = 0;
        public VoucherTypeUIController()
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


        [HttpGet]
        [Route("GetList")]
        public IHttpActionResult GetList()
        {
            using (var authContext = new AuthContext())
            {
                var list = authContext.VoucherTypeDB.ToList();
                return Ok(list);
            }
        }



        [HttpGet]
        [Route("AllMenulEdit")]
        public IHttpActionResult AllMenulEdit()
        {
            using (var authContext = new AuthContext())
            {
                var list = authContext.VoucherTypeDB.Where(x => x.IsManualEdit == true).ToList();
                return Ok(list);
            }
        }
    }
}
