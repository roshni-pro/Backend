using System.Web.Http;
using System.Web.OData;

namespace AngularJSAuthentication.API.ControllerV1
{
    [RoutePrefix("api/Orderfix")]
    public class CategoryOdataController : ODataController
    {

        [Route("")]
        public string Get()
        {

            return "done";
        }
    }
}
