using AngularJSAuthentication.API.Controllers.Base;
using AngularJSAuthentication.BusinessLayer.Managers.Reports;
using AngularJSAuthentication.DataContracts.Transaction.Reports;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http;

namespace AngularJSAuthentication.API.ControllerV7
{
    [RoutePrefix("api/PoReports")]
    public class PoReportsController : BaseAuthController
    {
        [Route("GrButNoIR")]
        [HttpGet]
        public async Task<List<GrButNoIrReportDc>> GrButNoIRReport(DateTime startDate, DateTime endDate)
        {
            var manager = new PoReportsManager();
            return await manager.GrButNoIRReport(startDate, endDate);
        }

    }
}
