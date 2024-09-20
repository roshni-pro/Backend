
using System;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using AngularJSAuthentication.API.App_Code.ECollection;
using AngularJSAuthentication.BusinessLayer.Ecollection;

namespace AngularJSAuthentication.API.Controllers.Ecollection
{

    [RoutePrefix("api/Ecollection")]
    public class EcollectionController : ApiController
    {
        private MisRepository MisRepository;
        private static DateTime CurrentDatetime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("India Standard Time"));
        public EcollectionController()
        {
            this.MisRepository = new MisRepository(new AuthContext());
        }

        public EcollectionController(MisRepository MisRepository)
        {
            this.MisRepository = MisRepository;
        }
//        [HttpGet]
//        [Route("GetData")]
//        public HttpResponseMessage GetData()
//        {
//            ECollectionBusiness.ECollectionBusinessClient client = new

//ECollectionBusiness.ECollectionBusinessClient();

//            ClientRequest objClientRequest = new ClientRequest();
//            ClientResponse objClientResponse = new ClientResponse();
//            Root obj = new Root();
//            obj.ClientCode = "SKETPLSK19294";
//            objClientRequest.strRequest = obj.ClientCode;
//            //objClientRequest.strRequest = "<Root><ClientCode> SKETPLSK19294 </ClientCode></Root>";
//            objClientRequest.ClientRequestType = RequestType.Json;
//            objClientRequest.ClientResponseType = ResponseType.Json;
//            ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls11;
//            ECollectionBusiness.ClientResponse
//objClientResponse1 = client.GetData(objClientRequest);
//            return Request.CreateResponse(HttpStatusCode.OK, objClientResponse1);
//        }
        [HttpPost]
        [Route("PushData")]
        public HttpResponseMessage PushData(MISData objMisData)
        {
            try
            {
                objMisData.CreatedDate = CurrentDatetime;

                Response objResponse = MisRepository.PushMisData(objMisData);
                return Request.CreateResponse(HttpStatusCode.OK, objResponse);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, ex.GetBaseException().Message.ToString());
            }
        }
    }

}
