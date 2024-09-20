using AngularJSAuthentication.API.Controllers.Base;
using NLog;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace AngularJSAuthentication.API.Controllers
{
    [RoutePrefix("api/EpayLetter")]
    public class EpayLetterController : BaseAuthController
    {
     
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private static TimeZoneInfo INDIAN_ZONE = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
        DateTime indianTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE);


        [Route("GetAllDataToExport")]
        [HttpGet]
        [AllowAnonymous]
        public HttpResponseMessage GetAllDataToExport(DateTime start, DateTime end)
        {

            using (var db = new AuthContext())
            {
                var query = "  select c.Skcode,e.ShopName,e.FirmType,e.ProprietorFirstName,e.ProprietorLastName,e.Gender,e.Mobile,e.WhatsAppNumber,e.Email,e.DOB,e.PAN_No,e.Country,e.State,"
                            + " e.City,w.WarehouseName,e.PostalCode,e.CreatedDate,LicenseImagePath,GSTImagePath,FSSAIImagePath,GovtRegNumberImagePath  from EpayLaterForms e inner join Customers c"
                            + "  on e.CustomerId = c.CustomerId inner join Warehouses w on c.WarehouseId=w.WarehouseId where  e.CreatedDate >='" + start.ToString("yyyy-dd-MM  hh:mm:ss") + "' and e.CreatedDate <= '" + end.ToString("yyyy-dd-MM  hh:mm:ss") + "'";


                var getrecord = db.Database.SqlQuery<GetEpayInfo>(query).ToList().OrderBy(x => x.CreatedDate);


                return Request.CreateResponse(HttpStatusCode.OK, getrecord);

            }

        }






        public class GetEpayInfo
        {

            public string Skcode { get; set; }
            //public string ShopName { get; set; }
            public string FirmType { get; set; }
            public string ProprietorFirstName { get; set; }
            public string ProprietorLastName { get; set; }
            public string Gender { get; set; }
            //public string Mobile { get; set; }
            public string WhatsAppNumber { get; set; }
            public string Email { get; set; }
            public DateTime? DOB { get; set; }
            public string PAN_No { get; set; }
            public string Country { get; set; }
            public string State { get; set; }
            public string City { get; set; }
            public string PostalCode { get; set; }
            public string LicenseImagePath { get; set; }
            public string GSTImagePath { get; set; }
            public string FSSAIImagePath { get; set; }
            public string GovtRegNumberImagePath { get; set; }
            public DateTime CreatedDate { get; set; }
            public string WarehouseName { get; set; }


        }
    }
}
