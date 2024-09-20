using AngularJSAuthentication.API.Helper;
using AngularJSAuthentication.API.Models;
using AngularJSAuthentication.DataContracts.Transaction.MasterExport;
using NLog;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace AngularJSAuthentication.API.ControllerV7
{
    [RoutePrefix("api/MasterExportRequest")]
    public class MasterExportRequestController : ApiController
    {

        private static Logger logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>


        [Route("GetList")]
        [HttpPost]
        public MasterExportRequestOutput GetList(MasterExportRequestPaginator paginator)
        {
            using (var db = new AuthContext())
            {
                var obj = new MasterExportRequestHelper();
                var MasterExportRequestOutput = obj.GetList(paginator);
                return MasterExportRequestOutput;
            }
        }


        [Route("UpdateMasterExport")]
        [AcceptVerbs("PUT")]
        public HttpResponseMessage UpdateStatus(MasterExportRequestDC masterExportRequest)
        {
            using (var db = new AuthContext())
            {
                var obj = new MasterExportRequestHelper();
                var MasterExportUpdateData = obj.UpdateStatus(masterExportRequest);
                return Request.CreateResponse(HttpStatusCode.OK, MasterExportUpdateData);
            }
        }

    }

}
