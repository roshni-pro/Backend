using AngularJSAuthentication.API.App_Code.InternalTransfer;
using AngularJSAuthentication.BusinessLayer.InternalTransfer.BO;
using AngularJSAuthentication.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Web.Http;

namespace AngularJSAuthentication.API.Controllers.InternalTransfer
{
    [RoutePrefix("api/InternalTransfer")]
    public class InternalTransferController : ApiController
    {

        private static TimeZoneInfo INDIAN_ZONE = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
        DateTime indianTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE);
        private InternalTransferRepostory InternalTransferRepostory;
        public InternalTransferController()
        {
            this.InternalTransferRepostory = new InternalTransferRepostory(new AuthContext());
        }

        public InternalTransferController(InternalTransferRepostory InternalTransferRepostory)
        {
            this.InternalTransferRepostory = InternalTransferRepostory;
        }
        [Authorize]
        [HttpGet]
        [Route("GetInternalTransferDetails")]
        public HttpResponseMessage GetInternalTransferDetails(int Warehouseid)
        {
            try
            {
                List<InternalTransferDetails> ObjInternalTransferDetails = InternalTransferRepostory.GetInternalTransferDetails(Warehouseid);
                return Request.CreateResponse(HttpStatusCode.OK, ObjInternalTransferDetails);
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex.GetBaseException().Message.ToString());
            }
        }
      
        private int GetUserId()
        {
            var identity = User.Identity as ClaimsIdentity;
            int userid = 0;
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
            return userid;
        }

        private int GetCompanyId()
        {
            var identity = User.Identity as ClaimsIdentity;
            int CompId = 0;
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))
                CompId = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);
            return CompId;
        }
        [Authorize]
        [HttpPost]
        [Route("PostTransferData")]
        public HttpResponseMessage PostTransferData(PostTransferData objPostTransferData)
        {
            try
            {
                objPostTransferData.Peopleid = GetUserId();
                objPostTransferData.CompanyId = GetCompanyId();
               
                bool Res = InternalTransferRepostory.PostItemTransfer(objPostTransferData);
                return Request.CreateResponse(HttpStatusCode.OK, Res);
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex.GetBaseException().Message.ToString());
            }

        }

    }
}
