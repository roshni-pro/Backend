using AngularJSAuthentication.API.Controllers.Base;
using AngularJSAuthentication.BusinessLayer.Managers.Reports;
using AngularJSAuthentication.DataContracts.APIParams;
using AngularJSAuthentication.DataContracts.Shared;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http;

namespace AngularJSAuthentication.API.ControllerV7
{
    [RoutePrefix("api/ItemLedger")]
    public class ItemLedgerController : BaseAuthController
    {
        [Route("")]
        [AcceptVerbs("GET")]
        public async Task<List<ItemLedgerExport>> GetItemLedgerHubWise(DateTime startDate, DateTime endDate, string wareHouseIds)
        {
            var manager = new ItemLedgerManager();
            return await manager.GetItemLedgerHubWiseAsync(startDate, endDate, wareHouseIds);
        }

        [Route("HubPnL")]
        [AcceptVerbs("POST")]
        public async Task HubPnL(HubPnLPostParams param)
        {
            var manager = new ItemLedgerManager();

        }
    }

}