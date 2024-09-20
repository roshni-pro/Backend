using AngularJSAuthentication.Common.Helpers;
using System.Threading.Tasks;
using System.Web.Http;

namespace AngularJSAuthentication.API.Controllers
{
    [RoutePrefix("api/HDFCPayment")]
    public class HDFCPaymentController : ApiController
    {
        [Route("GetRSA")]
        [HttpGet]
        public async Task<string> GetRSA(string hdfcOrderId, double amount, bool? IsCredit)
        {
            CCAvenueHelper cCAvenueHelper = new CCAvenueHelper();
            if (!IsCredit.HasValue) IsCredit = false;
            var rsaKey = cCAvenueHelper.GetRsaKey(hdfcOrderId, amount, IsCredit.Value);
            return rsaKey;
        }

    }
}
