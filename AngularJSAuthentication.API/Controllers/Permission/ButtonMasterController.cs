using System.Linq;
using System.Security.Claims;
using System.Web.Http;

namespace AngularJSAuthentication.API.Controllers.Permission
{
    [RoutePrefix("api/ButtonMaster")]
    public class ButtonMasterController : ApiController
    {
     
        int compid = 0, userid = 0;

        public ButtonMasterController()
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


        [Route("List")]
        [HttpGet]
        public IHttpActionResult GetList()
        {
            using (AuthContext db = new AuthContext())
            {
                var buttonList = db.ButtonMaster.ToList();
                return Ok(buttonList);
            }
        }
    }
}
