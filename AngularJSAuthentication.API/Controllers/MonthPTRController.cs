using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Http;

namespace AngularJSAuthentication.API.Controllers
{
    [RoutePrefix("api/MonthPTR")]
    public class MonthPTRController : ApiController
    {
        [Route("InsertMonthPTRData")]
        [HttpGet]
        [AllowAnonymous]
        public bool InsertMonthPTRData()
        {
            if (DateTime.Now.Day == 1)
            {
                using (var db = new AuthContext())
                {
                    if (db.Database.Connection.State != ConnectionState.Open)
                        db.Database.Connection.Open();

                    db.Database.CommandTimeout = 1200;
                    var jobData = db.Database.ExecuteSqlCommand("EXEC InsertMonthPTRData");
                }
            }
            return true;
        }
    }
}