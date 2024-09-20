using AngularJSAuthentication.API.Helper;
using AngularJSAuthentication.Controllers;
using AngularJSAuthentication.DataContracts.Transaction.PurchaseOrder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace AngularJSAuthentication.API.ControllerV7
{
    [RoutePrefix("PORequestPayment")]
    public class PORequestPaymentController : BaseApiController
    {
        [Route("GetPageData")]
        [HttpPost]
        public PORequestPaymentPage GetPageData(PORequestPager pager)
        {
            PORequestPaymentHelper helper = new PORequestPaymentHelper();
            PORequestPaymentPage page = helper.GetPageData(pager);
            return page;
        }


        [Route("MakePayment")]
        [HttpPost]
        public void MakePayment(PORequestPaymentDc payment)
        {
            int userid = GetLoginUserId();
            PORequestPaymentHelper helper = new PORequestPaymentHelper();
            helper.MakePayment(payment, userid);

        }
    }
}
