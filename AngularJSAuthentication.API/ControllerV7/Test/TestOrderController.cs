using AngularJSAuthentication.API.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace AngularJSAuthentication.API.ControllerV7.Test
{
    [AllowAnonymous]
    [RoutePrefix("api/TestOrder")]
    public class TestOrderController : ApiController
    {
        [Route("CheckCallingOfHangfire")]
        [HttpGet]
        public IHttpActionResult CheckCallingOfHangfire()
        {
            using(var authContext = new AuthContext())
            {
                var order = authContext.OrderDispatchedMasters.FirstOrDefault(x => x.Status == "Ready to Dispatch");
                order.UpdatedDate = DateTime.Now;
                authContext.Commit();
                return Ok();
            }
        }

        [Route("Generate/{mobile}")]
        [HttpGet]
        public async Task<IHttpActionResult> Generate(string mobile)
        {
            CustomerOtpHelper otpHelper = new CustomerOtpHelper();
            await otpHelper.GenerateOtp(mobile);
            return Ok();
        }

        [Route("Validate/{mobile}/{otp}")]
        [HttpGet]
        public async Task<IHttpActionResult> Validate(string mobile, string otp)
        {
            CustomerOtpHelper otpHelper = new CustomerOtpHelper();
            bool isVerified = await otpHelper.MatchOtp(mobile, otp);
            return Ok();
        }

    }
}
