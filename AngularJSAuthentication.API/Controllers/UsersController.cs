using System.Web.Http;

namespace AngularJSAuthentication.API.Controllers
{
    [RoutePrefix("api/Users")]
    public class UsersControllerController : ApiController
    {
        [Authorize]
        [Route("")]
        public IHttpActionResult Get()
        {

            using (AuthContext context = new AuthContext())
            {

                //ClaimsPrincipal principal = Request.GetRequestContext().Principal as ClaimsPrincipal;

                //var Name = ClaimsPrincipal.Current.Identity.Name;
                //var Name1 = User.Identity.Name;

                //var userName = principal.Claims.Where(c => c.Type == "sub").Single().Value;

                return Ok(Helper.CreateUsers());
            }
        }

    }


    #region Helpers



}

#endregion

