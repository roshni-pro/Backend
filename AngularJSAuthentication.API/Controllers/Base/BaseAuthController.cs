using System.Web.Http;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using System.Security.Claims;
using AngularJSAuthentication.Model;
using System.Linq;

namespace AngularJSAuthentication.API.Controllers.Base
{



    [Authorize]
    public class BaseAuthController : ApiController
    {
        protected int GetLoginUserId()
        {
            var identity = User.Identity as ClaimsIdentity;
            int userid = 0;
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
            return userid;
        }

    }

}

