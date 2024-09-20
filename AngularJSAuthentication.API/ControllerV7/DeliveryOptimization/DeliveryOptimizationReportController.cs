using AngularJSAuthentication.BusinessLayer.Managers.TripPlanner;
using AngularJSAuthentication.Common.Helpers;
using AngularJSAuthentication.Controllers;
using AngularJSAuthentication.DataContracts.DeliveryOptimization;
using AngularJSAuthentication.DataLayer.Repositories.Transactions.TripPlanner;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace AngularJSAuthentication.API.ControllerV7.DeliveryOptimization
{
    [RoutePrefix("api/DeliveryOptimizationReport")]
    public class DeliveryOptimizationReportController : BaseApiController
    {
        [Route("DboyPerformanceList")]
        [HttpPost]
        public async Task<List<DboyPerformaceOutputDc>> DboyPerformanceList(DboyPerformanceInputDC input)
        {
            DboyPerformanceManager manager = new DboyPerformanceManager();
            var list = await manager.DboyPerformanceReport(input);
            return list;
        }

        [Route("DboyPerformanceExport")]
        [HttpPost]
        public async Task<string> DboyPerformanceExport(DboyPerformanceInputDC input)
        {
            DboyPerformanceManager manager = new DboyPerformanceManager();
            var path = await manager.DboyPerformanceExport(input, HttpRuntime.AppDomainAppPath);
            return path;
        }

        [Route("VehiclePerformanceReport")]
        [HttpPost]
        public async Task<List<VehiclePerformanceOutputDc>> VehiclePerformanceReport(VehiclePerformanceInputDc input)
        {
            VehiclePerformanceManager manager = new VehiclePerformanceManager();
            var list = await manager.VehiclePerformanceReport(input);
            return list;
        }

        [Route("VehiclePerformanceExport")]
        [HttpPost]
        public async Task<string> VehiclePerformanceExport(VehiclePerformanceInputDc input)
        {
            VehiclePerformanceManager manager = new VehiclePerformanceManager();
            var path = await manager.VehiclePerformanceExport(input, HttpRuntime.AppDomainAppPath);
            return path;
        }

        [Route("TripSummaryDashboardSummary")]
        [HttpPost]
        public async Task<TripSummaryReportSummaryDc> TripSummary(TripSummaryReportInputDc input)
        {
            TripSummaryReportManager manager = new TripSummaryReportManager();
            var summary = await manager.Summary(input);
            return summary;
        }

        [Route("TripSummaryDashboardCost")]
        [HttpPost]
        public async Task<TripSummaryReportCostDc> Cost(TripSummaryReportInputDc input)
        {
            TripSummaryReportManager manager = new TripSummaryReportManager();
            var cost = await manager.Cost(input);
            return cost;
        }

        [Route("TripSummaryDashboardLineChart")]
        [HttpPost]
        public async Task<TripSummaryReportLineChartDc> LineChartData(TripSummaryReportInputDc input)
        {
            TripSummaryReportManager manager = new TripSummaryReportManager();
            return await manager.LineChartDataWithIteration(input);
        }

        [Route("TripReportOverview")]
        [HttpPost]
        public async Task<List<TripReportOverviewDc>> TripReportOverview(TripReportInputDc input)
        {
            TripSummaryReportManager manager = new TripSummaryReportManager();
            return await manager.TripReportOverview(input);
        }

        [Route("TripReportOrderOverview")]
        [HttpPost]
        public async Task<List<TripReportOrderOverviewDc>> TripReportOrderOverview(TripReportInputDc input)
        {
            TripSummaryReportManager manager = new TripSummaryReportManager();
            return await manager.TripReportOrderOverview(input);
        }


        [Route("TripReportExport")]
        [HttpPost]
        public async Task<string> TripReportExport(TripReportInputDc input)
        {
            TripSummaryReportManager manager = new TripSummaryReportManager();
            string path = await manager.TripReportExport(input, HttpRuntime.AppDomainAppPath);
            return path;
        }

        [Route("LMDDashboardPart1")]
        [HttpPost]
        public async Task<List<TripSummaryReportSummaryDc>> LMDDashboardPart1(LMDDashboardPart1InputDc input)
        {
            TripSummaryReportManager manager = new TripSummaryReportManager();
            var List = await manager.LMDDashboardPart1(input);
            return List;
        }

        [Route("LMDDashboardPart2")]
        [HttpPost]
        public async Task<List<TripSummaryReportCostDc>> LMDDashboardPart2(LMDDashboardPart1InputDc input)
        {
            TripSummaryReportManager manager = new TripSummaryReportManager();
            var List = await manager.LMDDashboardPart2(input);
            return List;
        }

        [Route("LMDTransporterGet")]
        [HttpPost]
        public async Task<List<TransporterList>> LMDTransporterGet(List<int> WarehouseIds)
        {
            TripSummaryReportManager manager = new TripSummaryReportManager();
            var List = await manager.LMDTransporterGet(WarehouseIds);
            return List;
        }

        [Route("LMDDashboardPart3")]
        [HttpPost]
        public async Task<LMDDashboardPart3DC> LMDDashboardPart3(LMDDashboardPart1InputDc input)
        {
            TripSummaryReportManager manager = new TripSummaryReportManager();
            var List = await manager.LMDDashboardPart3(input);
            return List;
        }

        [Route("LMDDashboardPart4")]
        [HttpPost]
        public async Task<LMDDashboardPart4DC> LMDDashboardPart4(LMDDashboardPart1InputDc input)
        {
            TripSummaryReportManager manager = new TripSummaryReportManager();
            var List = await manager.LMDDashboardPart4(input);
            return List;
        }


        [Route("LMDDashboardExportAll")]
        [HttpPost]
        public async Task<string> LMDDashboardExport(LMDDashboardPart1InputDc input)
        {
            TripSummaryReportManager manager = new TripSummaryReportManager();
            var ExportPart5 = await manager.LMDDashboardPart5(input);

            DataTable dt = ListtoDataTableConverter.ToDataTable(ExportPart5);
            var fileName = "_" + DateTime.Now.ToString("yyyyddMHHmmss") + "_LMDExportAll.csv";
            string path = Path.Combine(HttpContext.Current.Server.MapPath("~/ExcelGeneratePath"), fileName);
            dt.WriteToCsvFile(path);
            return $"/ExcelGeneratePath/{fileName}";
        }

    

        [Route("LMDDashboardExportSummary")]
        [HttpPost]
        public async Task<string> LMDDashboardExportSummary(LMDDashboardPart1InputDc input)
        {
            TripSummaryReportManager manager = new TripSummaryReportManager();
            var ExportPart6 = await manager.LMDDashboardPart6(input);

            DataTable dt = ListtoDataTableConverter.ToDataTable(ExportPart6);
            var fileName = "_" + DateTime.Now.ToString("yyyyddMHHmmss") + "_LMDExportSummary.csv";
            string path = Path.Combine(HttpContext.Current.Server.MapPath("~/ExcelGeneratePath"), fileName);
            dt.WriteToCsvFile(path);
            return $"/ExcelGeneratePath/{fileName}";
        }

      
        [Route("LMDDashboardExportOrder")]
        [HttpPost]
        public async Task<string> LMDDashboardExportOrder(LMDDashboardPart1InputDc input)
        {
            TripSummaryReportManager manager = new TripSummaryReportManager();
            var ExportPart7 = await manager.LMDDashboardPart7(input);

            DataTable dt = new DataTable();
            dt.Columns.Add("Trip Sheet No/Area");
            dt.Columns.Add("OrderId");
            dt.Columns.Add("ClusterName");
            dt.Columns.Add("ExecutiveName");
            dt.Columns.Add("DboyName");
            dt.Columns.Add("RA/RD/Cancel");
            dt.Columns.Add("DB Comments");
            dt.Columns.Add("OrderDate");
            dt.Columns.Add("ETADate");
            if(ExportPart7 != null && ExportPart7.Any() && ExportPart7.Count() > 0)
            {
                foreach(var a in ExportPart7)
                {
                    DataRow dr = dt.NewRow();
                    dr["Trip Sheet No/Area"] = a.TripPlannerMasterId;
                    dr["OrderId"] = a.OrderId;
                    dr["ClusterName"] = a.ClusterName;
                    dr["ExecutiveName"] = a.ExecutiveName;
                    dr["DboyName"] = a.DboyName;
                    dr["RA/RD/Cancel"] = a.OrderStatus;
                    dr["DB Comments"] = a.comments;
                    dr["OrderDate"] = a.OrderedDate;
                    dr["ETADate"] = a.ETADate;
                    dt.Rows.Add(dr);
                }
            }

            //DataTable dt = ListtoDataTableConverter.ToDataTable(ExportPart7);
            var fileName = "_" + DateTime.Now.ToString("yyyyddMHHmmss") + "_LMDExportOrder.csv";
            string path = Path.Combine(HttpContext.Current.Server.MapPath("~/ExcelGeneratePath"), fileName);
            dt.WriteToCsvFile(path);
            return $"/ExcelGeneratePath/{fileName}";
        }
      
        [Route("LMDChart")]
        [HttpPost]
        public async Task<TripSummaryReportLineChartDc> LMDChart(LMDDashboardPart1InputDc input)
        {
            TripSummaryReportManager manager = new TripSummaryReportManager();
            return await manager.LMDLineChart(input);
        }


    }
}
