using System;
using System.Linq;
using System.Web.Http;

namespace AngularJSAuthentication.API.Controllers
{
    [RoutePrefix("api/LogOut")]
    public class LogOutController : ApiController
    {
        [HttpPost]
        public void logoutData()
        {
            using (AuthContext dc = new AuthContext())
            {

                var data = dc.UserTrakings.Where(o => o.Id == 12 && o.PeopleId == "156").SingleOrDefault();
                data.LogOutTime = DateTime.Now;
                data.Remark = data.Remark + "=> logout page ,";
                dc.UserTrakings.Add(data);
                dc.Commit();
            }
        }

    }
}
