using AngularJSAuthentication.API.Controllers.Base;
using AngularJSAuthentication.API.Managers.TripPlanner;
using AngularJSAuthentication.DataContracts.TripPlanner;
using AngularJSAuthentication.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace AngularJSAuthentication.API.ControllerV7.VehicleMaster
{
    [RoutePrefix("api/SarthiTripRequest")]
    public class SarthiTripRequestController : BaseAuthController
    {
        [AllowAnonymous]
        [Route("List")]
        [HttpPost]
        public List<TripPlannerApprovalRequestDc> GetList(TripPlannerApprovalRequestInputDc input)
        {
            TripPlannerApprovalRequestManager manager = new TripPlannerApprovalRequestManager();
            var list = manager.GetList(input);
            return list;
        }

        [Route("Approve")]
        [HttpPost]
        public async Task<bool> Approve([FromUri] int peopleId, [FromBody]TripPlannerApprovalRequestDc request)
        {
            bool trueIfApproveElseReject = true;
            TripPlannerApprovalRequestManager manager = new TripPlannerApprovalRequestManager();
            bool result = await manager.ApproveReject(request, trueIfApproveElseReject, peopleId);
            return result;
        }

        [AllowAnonymous]
        [Route("Reject")]
        [HttpPost]
        public async Task<bool> Reject([FromUri] int peopleId, [FromBody]TripPlannerApprovalRequestDc request)
        {
            bool trueIfApproveElseReject = false;
            TripPlannerApprovalRequestManager manager = new TripPlannerApprovalRequestManager();
            bool result = await manager.ApproveReject(request, trueIfApproveElseReject, peopleId);
            return result;
        }
    }
}
