using AngularJSAuthentication.API.Helper;
using AngularJSAuthentication.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace AngularJSAuthentication.API.ControllerV7
{
    [RoutePrefix("api/CustomerOtp")]
    [AllowAnonymous]
    public class CustomerOtpController : BaseApiController
    {
        [Route("GenerateOtp/{mobile}/{timeInMins:int=10}")]
        [HttpGet]
        public async Task<bool> GenerateOtp (string mobile, int timeInMins)
        {
            CustomerOtpHelper helper = new CustomerOtpHelper();
            string otp = await helper.GenerateOtp(mobile, timeInMins);

            if (!string.IsNullOrEmpty(otp))
            {
                return true;
            }
            else
            {
                return false;
            }
        }


        [Route("MatchOtp/{mobile}/{otp}")]
        [HttpGet]
        public async Task<bool> MatchOtp(string mobile, string otp)
        {
            CustomerOtpHelper helper = new CustomerOtpHelper();
            bool isMatch = await helper.MatchOtp(mobile, otp);

            return isMatch;
        }
    }
}
