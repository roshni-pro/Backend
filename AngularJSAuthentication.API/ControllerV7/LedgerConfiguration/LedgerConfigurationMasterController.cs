using AngularJSAuthentication.Accounts.Managers;
using AngularJSAuthentication.Accounts.vm;
using AngularJSAuthentication.API.Helper;
using AngularJSAuthentication.Model.Account;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Web.Http;

namespace AngularJSAuthentication.API.ControllerV7.LedgerConfiguration
{
    [RoutePrefix("api/LedgerConfigurationMaster")]
    public class LedgerConfigurationMasterController : ApiController
    {
        [Route("PostLedger")]
        [HttpPost]
        public LedgerConfigurationMaster PostLedgerData(LedgerConfigurationMasterVM ledgerConfigurationMasterVM)
        {
            LedgerConfigurationManager manager = new LedgerConfigurationManager();
            var list = manager.PostData(ledgerConfigurationMasterVM);
            return list;
        }

        [Route("GetByID/{ledgerConfigurationMasterId}")]
        [HttpGet]
        public LedgerConfigurationMasterVM GetByID(int ledgerConfigurationMasterId)
        {
            LedgerConfigurationManager manager = new LedgerConfigurationManager();
            LedgerConfigurationMasterVM master = manager.GetById(ledgerConfigurationMasterId);
            LadgerHelper ladgerHelper = new LadgerHelper();
            if (master != null && master.ledgerConfigurationDetails != null && master.ledgerConfigurationDetails.Any())
            {
                foreach (var item in master.ledgerConfigurationDetails)
                {
                    if (item.IsFixedDebitLedger)
                    {
                        item.DebitLedgerName = ladgerHelper.GetByLedgerID(item.DebitLedgerId.Value);
                    }
                    if (item.IsFixedCreditLedger)
                    {
                        item.CreditLedgerName = ladgerHelper.GetByLedgerID(item.CreditLedgerId.Value);
                    }
                }
            }
            return master;
        }
        [Route("SaveDetails")]
        [HttpPost]
        public LedgerConfigurationDetail SaveLedgerDetails(LedgerConfigurationDetailsVM ledgerConfigurationDetailsVM)
        {

            LedgerConfigurationManager manager = new LedgerConfigurationManager();
            LedgerConfigurationDetail list = manager.SaveDetails(ledgerConfigurationDetailsVM);
            return list;
        }
        [Route("UpdateEntity")]
        [HttpPut]
        public LedgerConfigurationMaster UpdateEntityName(LedgerConfigurationMasterVM ledgerConfigurationMasterVM)
        {

            var identity = User.Identity as ClaimsIdentity;
            int userid = 0;
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

            LedgerConfigurationManager manager = new LedgerConfigurationManager();
            LedgerConfigurationMaster list = manager.UpdateEntityName(ledgerConfigurationMasterVM, userid);
            return list;
        }
        [Route("AddParamEntry")]
        [HttpPost]
        public LedgerConfigurationParmameter AddParamEntry(LedgerConfigurationParameterVM ledgerConfigurationParameterVM)
        {

            var identity = User.Identity as ClaimsIdentity;
            int userid = 0;
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

            LedgerConfigurationDetailManager manager = new LedgerConfigurationDetailManager();
            LedgerConfigurationParmameter list = manager.AddParamEntry(ledgerConfigurationParameterVM, userid);
            return list;
        }
        [Route("SaveCondition")]
        [HttpPost]
        public LedgerConfigurationMasterCondition SaveCondition(LedgerConfigurationMasterConditionVM ledgerConfigurationMasterConditionVM)
        {
            var identity = User.Identity as ClaimsIdentity;
            int userid = 0;
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
            ledgerConfigurationMasterConditionVM.CreatedBy = userid;
            LedgerConfigurationMasterConditionManager manager = new LedgerConfigurationMasterConditionManager();
            LedgerConfigurationMasterCondition condition = manager.SaveCondition(ledgerConfigurationMasterConditionVM);
            return condition;
        }

        [Route("GetList")]
        [HttpGet]
        public List<LedgerConfigurationMaster> GetList()
        {
            LedgerConfigurationManager manager = new LedgerConfigurationManager();
            List<LedgerConfigurationMaster> master = manager.GetList();

            return master;
        }


        [Route("Delete")]
        [HttpGet]
        public LedgerConfigurationParmameter Delete(int Id)
        {
            using (AuthContext context = new AuthContext())
            {
                LedgerConfigurationParmameter LedgerConfiguration = context.LedgerConfigurationParmameterDB.Where(c => c.Id.Equals(Id) && c.IsDeleted == false).SingleOrDefault();

                context.LedgerConfigurationParmameterDB.Remove(LedgerConfiguration);
                context.Commit();
                return LedgerConfiguration;
            }
        }

    }
}
