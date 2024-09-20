using AngularJSAuthentication.Model;
using System.Collections.Generic;
using System.Linq;
using System.Web.OData;

namespace AngularJSAuthentication.API.ControllerV1
{
    public class SubsubCategoryOdataController : ODataController
    {

        [EnableQuery]
        public List<SubsubCategory> Get()
        {
            using (var authContext = new AuthContext())
            {
                return authContext.SubsubCategorys.ToList();
            }
        }
    }
}
