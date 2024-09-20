using AngularJSAuthentication.API;

using GenricEcommers.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Web.Http;

namespace CallLog.Controllers
{
    [RoutePrefix("api/CallHistory")]

    public class CallHistoryController : ApiController
    {

        #region  Post api for app side data post
        /// <summary>
        /// app side data post for Call  Log Data
        /// </summary>
        /// Creatd Date 28/003/2019
        /// <param name="cdata"></param>
        /// <returns>CHData</returns>
        [Route("CallLog")]
        [AcceptVerbs("POST")]
        public HttpResponseMessage addData(CallHistroy cdata)
        {
            using (var db = new AuthContext())
            {
                CustomerCallHistoryDTO CHData;
                try
                {

                    CallHistroy data = new CallHistroy();
                    data.phNumber = cdata.phNumber;
                    data.callType = cdata.callType;
                    data.OtherphNumber = cdata.OtherphNumber;
                    data.callDate = cdata.callDate;
                    data.callDuration = cdata.callDuration;
                    data.callDayTime = cdata.callDayTime;
                    db.CallHistoryDB.Add(data);
                    db.Commit();

                    CHData = new CustomerCallHistoryDTO()
                    {
                        chdata = data,
                        Status = true,
                        Message = "Save  successfully."
                    };
                    return Request.CreateResponse(HttpStatusCode.OK, CHData);
                }
                catch (Exception ex)
                {
                    CHData = new CustomerCallHistoryDTO()
                    {
                        chdata = null,
                        Status = false,
                        Message = "  Unsuccessfully."
                    };
                    return Request.CreateResponse(HttpStatusCode.BadRequest, CHData);
                }
            }
        }

        [HttpGet]
        [Route("getAllHistroy")]
        public List<CallHistroy> getAllHistroy()
        {
            using (var db = new AuthContext())
            {
                try
                {
                    var identity = User.Identity as ClaimsIdentity;
                    int compid = 0, userid = 0;

                    // Access claims
                    foreach (Claim claim in identity.Claims)
                    {
                        if (claim.Type == "compid")
                        {
                            compid = int.Parse(claim.Value);
                        }
                        if (claim.Type == "userid")
                        {
                            userid = int.Parse(claim.Value);
                        }
                    }
                    List<CallHistroy> gr = db.CallHistoryDB.ToList();
                    return gr;

                }
                catch (Exception ex)
                {
                    return null;
                }
            }
        }

        #endregion
        public class CustomerCallHistoryDTO
        {
            public CallHistroy chdata { get; set; }
            public bool Status { get; set; }
            public string Message { get; set; }
        }
    }
}
