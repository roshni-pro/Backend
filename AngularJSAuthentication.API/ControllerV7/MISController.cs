using AngularJSAuthentication.BusinessLayer.Managers.Reports;
using AngularJSAuthentication.DataContracts.Transaction.Reports;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Http;

namespace AngularJSAuthentication.API.ControllerV7
{
    [RoutePrefix("api/MIS")]
    public class MISController : ApiController
    {
        #region Open Orders
        [Route("GetRedispatchAndCancellationReport")]
        [HttpPost]
        public async Task<List<RedispatchAndCancellationReportDc>> GetRedispatchAndCancellationReportAsync(RDAndCancellationFilterDc filter)
        {
            MISManager mISManager = new MISManager();
            return await mISManager.GetRedispatchAndCancellationReportAsync(filter);
        }

        [Route("GetPostRTDOrdersReport")]
        [HttpPost]
        public async Task<List<PostRTDOrdersReportDc>> GetPostRTDOrdersReportAsync(PostRTDOrdersFilterDc filter)
        {
            MISManager mISManager = new MISManager();
            return await mISManager.GetPostRTDOrdersReportAsync(filter);
        }
        [Route("GetDeliveryBoyReport")]
        [HttpPost]
        public async Task<List<DeliveryBoyReportDc>> GetDeliveryBoyReportAsync(DeliveryBoyFilterDc filter)
        {
            MISManager mISManager = new MISManager();
            return await mISManager.GetDeliveryBoyReportAsync(filter);
        }
        [Route("GetWarehouseReport")]
        [HttpPost]
        public async Task<List<WarehouseReportDc>> GetWarehouseReportAsync(WarehouseReportFilterDc filter)
        {
            MISManager mISManager = new MISManager();
            return await mISManager.GetWarehouseReportAsync(filter);
        }
        [Route("GetDayWiseReport")]
        [HttpPost]
        public async Task<List<DayWiseReportDc>> GetDayWiseReportAsync(DayWiseFilterDc filter)
        {
            MISManager mISManager = new MISManager();
            return await mISManager.GetDayWiseReportAsync(filter);
        }
        #endregion
        #region EPay / ETA
        [Route("GetEPayReport")]
        [HttpPost]
        public async Task<List<EPayReportDc>> GetEPayReportAsync(EPayFilterDc filter)
        {
            MISManager mISManager = new MISManager();
            return await mISManager.GetEPayReportAsync(filter);
        }

        [Route("GetETAReport")]
        [HttpPost]
        public async Task<List<ETAReportDc>> GetETAReportAsync(ETAFilterDc filter)
        {
            MISManager mISManager = new MISManager();
            return await mISManager.GetETAReportAsync(filter);
        }
        #endregion
        #region Delivery Reports
        [Route("GetDeliveryT0T1Report")]
        [HttpPost]
        public async Task<List<DeliveryT0T1ReportDc>> GetDeliveryT0T1ReportAsync(DeliveryT0T1FilterDc filter)
        {
            MISManager mISManager = new MISManager();
            return await mISManager.GetDeliveryT0T1ReportAsync(filter);
        }

        [Route("GetDeliveryT0T4Report")]
        [HttpPost]
        public async Task<List<DeliveryT0T4ReportDc>> GetDeliveryT0T4ReportAsync(DeliveryT0T4FilterDc filter)
        {
            MISManager mISManager = new MISManager();
            return await mISManager.GetDeliveryT0T4ReportAsync(filter);
        }
        #endregion
        #region Cancellation Reports
        [Route("GetSalesVsCancellationReport")]
        [HttpPost]
        public async Task<List<SalesVsCancellationReportDc>> GetSalesVsCancellationReportAsync(SalesVsCancellationFilterDc filter)
        {
            MISManager mISManager = new MISManager();
            return await mISManager.GetSalesVsCancellationReportAsync(filter);
        }
        [Route("GetTop30ExecutiveReport")]
        [HttpPost]
        public async Task<List<Top30ExecutiveReportDc>> GetTop30ExecutiveReportAsync(Top30ExecutiveFiterDc filter)
        {
            MISManager mISManager = new MISManager();
            return await mISManager.GetTop30ExecutiveReporAsync(filter);
        }
        [Route("GetTop30DBoyReport")]
        [HttpPost]
        public async Task<List<GetTop30DBoyReportDc>> GetTop30DBoyReportAsync(GetTop30DBoyFilterDc filter)
        {
            MISManager mISManager = new MISManager();
            return await mISManager.GetTop30DBoyReportAsync(filter);
        }
        #endregion
        #region TAT & KPI Reports
        [Route("GetTATMasterReport")]
        [HttpPost]
        public async Task<List<TATMasterReportDc>> GetTATMasterReportAsync(TATMasterFilterDc filter)
        {
            MISManager mISManager = new MISManager();
            return await mISManager.GetTATMasterReportAsync(filter);
        }
        [Route("GetDailyKPIReport")]
        [HttpPost]
        public async Task<List<DailyKPIReportDc>> GetDailyKPIReportAsync(DailyKPIFilterDc filter)
        {
            MISManager mISManager = new MISManager();
            return await mISManager.GetDailyKPIReportAsync(filter);
        }
        [Route("GetSpillOrderReport")]
        [HttpPost]
        public async Task<List<SpillOrderReportDc>> GetSpillOrderReportAsync(SpillOrderFilterDc filter)
        {
            MISManager mISManager = new MISManager();
            return await mISManager.GetSpillOrderReportAsync(filter);
        }
        #endregion
    }
}
