using AngularJSAuthentication.Model;
using Newtonsoft.Json;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace AngularJSAuthentication.API.Controllers.External.SalesManApp
{
    [RoutePrefix("api/SalesApp")]
    public class LocationResumeController : ApiController
    {
        public static Logger logger = LogManager.GetCurrentClassLogger();
        private static TimeZoneInfo INDIAN_ZONE = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
        DateTime indianTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE);

        [Route("LocationResume")]
        [AllowAnonymous]
        [HttpPost]
        public HttpResponseMessage LocationResume(LocationResumeDC locationDc)
        {
            string Result = string.Empty;
            try
            {                 
                using (var db = new AuthContext())
                {
                    LocationResumeDetail detail = new LocationResumeDetail()
                    {
                        Latitude = locationDc.Latitude,
                        Longitude = locationDc.Longitude,
                        PeopleId = locationDc.PeopleId,
                        Status = locationDc.Status,
                        CreatedDate = DateTime.Now
                    };
                    db.LocationResumeDetails.Add(detail);
                    if (db.Commit() > 0)
                    {
                        return Request.CreateResponse(HttpStatusCode.OK, "Details Submitted");   
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.BadRequest, "Details not Submitted");
                    }                   
                }
            }
            catch (Exception ex)
            {
                logger.Error("Error in Add data salesperson " + ex.Message);
                return Request.CreateResponse(HttpStatusCode.BadRequest, ex.Message);
            }
        }
    }
    public class LocationResumeDC
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public int PeopleId { get; set; }
        public string Status { get; set; }
    }
}
