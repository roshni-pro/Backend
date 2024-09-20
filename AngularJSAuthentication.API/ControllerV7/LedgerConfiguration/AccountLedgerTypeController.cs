using AngularJSAuthentication.Accounts.Managers;
using AngularJSAuthentication.Accounts.vm;
using System.Collections.Generic;
using System.Web.Http;

namespace AngularJSAuthentication.API.ControllerV7.LedgerConfiguration
{
    [RoutePrefix("api/AccountLedgerType")]
    public class AccountLedgerTypeController : ApiController
    {
        [Route("GetList")]
        [HttpGet]
        public List<LadgerTypeAccountVM> GetList()
        {
            AccountLedgerTypeManager manager = new AccountLedgerTypeManager();
            var list = manager.GetList();
            return list;
        }

        [Route("GetVocherList")]
        [HttpGet]
        public List<DropDownItem> GetVocherList()
        {
            VoucherTypeManager manager = new VoucherTypeManager();
            var list = manager.GetVocherList();
            return list;
        }

    }
}
