using AngularJSAuthentication.Model.Account;
using System;
using System.Security.Claims;
using System.Web.Http;

namespace AngularJSAuthentication.API.ControllerV7
{
    [RoutePrefix("api/Voucher")]
    public class VoucherUIController : ApiController
    {

        int compid = 0, userid = 0;
        public VoucherUIController()
        {
            var identity = User.Identity as ClaimsIdentity;
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

        }

        [HttpGet]
        [Route("Generate/{code}")]
        public IHttpActionResult Generate(string code)
        {
            using (var authContext = new AuthContext())
            {
                Voucher voucher = new Voucher();
                voucher.Active = true;
                voucher.Code = code;
                voucher.CreatedBy = userid;
                voucher.CreatedDate = DateTime.Now;
                voucher.ID = 0;
                voucher.UpdatedBy = userid;
                voucher.UpdatedDate = DateTime.Now;
                authContext.VoucherDB.Add(voucher);
                authContext.Commit();
                return Ok(voucher);
            }
        }


    }
}
