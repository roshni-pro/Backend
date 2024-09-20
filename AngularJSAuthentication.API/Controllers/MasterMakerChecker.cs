using NLog;
using System;
using System.Linq;
using System.Web.Http;

namespace AngularJSAuthentication.API.Controllers
{
    [RoutePrefix("api/MasterChecker")]
    public class MasterMakerChecker : ApiController
    {
        private static TimeZoneInfo INDIAN_ZONE = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
        DateTime indianTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE);
        private static Logger logger = LogManager.GetCurrentClassLogger();

        [Route("GetCheckerList")]
        [HttpGet]
        public dynamic GetCheckerList()
        {

            using (var con = new AuthContext())
            {
                try
                {
                    var CheckerList = con.Database.SqlQuery<Checker>("exec GetMasterCheckerList").ToList();

                    return Ok(CheckerList);
                }
                catch (Exception ex)
                {
                    throw;
                }
            }
        }
    }

    //public class Checker
    //{
    //    public int Id { get; set; }
    //    public string EntityName { get; set; }
    //}
}