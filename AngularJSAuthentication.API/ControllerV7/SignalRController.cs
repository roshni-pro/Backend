using AngularJSAuthentication.API.Helper;
using AngularJSAuthentication.Common.Constants;
using Newtonsoft.Json;
using System;
using System.Web.Http;

namespace AngularJSAuthentication.API.ControllerV7
{
    [RoutePrefix("api/Location")]
    public class SignalRController : ApiController
    {

        [HttpPost]
        [Route("GetLatLong")]
        public IHttpActionResult GetLatLong(LatLng location)
        {
            var client = new SignalRMasterClient(DbConstants.URL + "signalr");
            // Send message to server.
            string message = JsonConvert.SerializeObject(location);
            client.SayHello(message, "");
            Console.ReadKey();
            client.Stop();
            return Ok();
        }


        public class LatLng
        {
            public string Lat { get; set; }
            public string Lng { get; set; }
            public string SalesPersonId { get; set; }
            public string Name { get; set; }
            public int WarehouseID { get; set; }
            public int CityID { get; set; }
            public string Moblie { get; set; }
        }
    }
}
