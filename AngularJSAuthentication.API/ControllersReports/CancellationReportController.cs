using AgileObjects.AgileMapper;
using AngularJSAuthentication.DataContracts.Transaction.Reports;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace AngularJSAuthentication.API.ControllersReports
{
    [RoutePrefix("api/CancellationReport")]
    public class CancellationReportController : ApiController
    {
        [Route("GetDboy")]
        [HttpGet]
        public async Task<CancellationReportResDc> GetGetDboy(int WarehouseId, string MobileNo)
        {
            CancellationReportResDc result = new CancellationReportResDc();
            using (var authContext = new AuthContext())
            {
                if (authContext.Database.Connection.State != ConnectionState.Open)
                    authContext.Database.Connection.Open();
                var cmd = authContext.Database.Connection.CreateCommand();
                cmd.CommandText = "[Cancellation].[DboyReport]";
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.Parameters.Add(new SqlParameter("@WarehouseId", WarehouseId));
                cmd.Parameters.Add(new SqlParameter("@DboyMobileNo", MobileNo));

                var reader = cmd.ExecuteReader();
                var CancellationReport = ((IObjectContextAdapter)authContext)
                 .ObjectContext
                 .Translate<CancellationReportDc>(reader).FirstOrDefault();
                if (CancellationReport != null)
                {

                    //on amount Cancellation
                    result.CancelAmount = Math.Round(CancellationReport.CurrentMonthCancelValue, 2);  //Current Month Cancellation amount   
                    result.CancelAmountDiff = result.CancelAmount - CancellationReport.LastMonthCancelValue;
                    double currentCancelAmountPercent = Math.Round(CancellationReport.CurrentMonthCancelValue > 0 ? Convert.ToDouble(CancellationReport.CurrentMonthCancelValue) / CancellationReport.CurrentMonthTotalValue * 100 : 0, 2);
                    double lastCancelAmountPercent = Math.Round(CancellationReport.LastMonthCancelValue > 0 ? Convert.ToDouble(CancellationReport.LastMonthCancelValue) / CancellationReport.LastMonthTotalValue * 100 : 0, 2);
                    result.CompareAmountPercent = Math.Round(currentCancelAmountPercent - lastCancelAmountPercent, 2);

                    // on count Cancellation
                    result.CancelCount = CancellationReport.CurrentMonthCancelCount; // Current month Cancellation count
                    result.CancelCountDiff = result.CancelCount - CancellationReport.LastMonthCancelCount;
                    double currentCancelCountPercent = Math.Round(CancellationReport.CurrentMonthCancelCount > 0 ? Convert.ToDouble(CancellationReport.CurrentMonthCancelCount) / CancellationReport.CurrentMonthTotalCount * 100 : 0, 2);
                    double lastCancelCountPercent = Math.Round(CancellationReport.LastMonthCancelCount > 0 ? Convert.ToDouble(CancellationReport.LastMonthCancelCount) / CancellationReport.LastMonthTotalCount * 100 : 0, 2);
                    result.CompareCountPercent = Math.Round(currentCancelCountPercent - lastCancelCountPercent, 2);

                    //Cancellation  Percent on value
                    result.CancellationPercant = Math.Round(CancellationReport.CurrentMonthCancelValue > 0 ? Convert.ToDouble(CancellationReport.CurrentMonthCancelValue) / CancellationReport.CurrentMonthTotalValue * 100 : 0, 2);
                    double lastCancellationPercant = CancellationReport.LastMonthCancelValue > 0 ? (Convert.ToDouble(CancellationReport.LastMonthCancelValue) / CancellationReport.LastMonthTotalValue * 100) : 0;
                    result.CompareCancellationPercant = Math.Round(result.CancellationPercant - lastCancellationPercant, 2);


                    if (result.CancellationPercant >= 0 && result.CancellationPercant <= 5)
                    {
                        result.Backgroundcolor = "#FFFFFF"; //white
                        result.WarningCount = 0;
                    }
                    else if (result.CancellationPercant > 5 && result.CancellationPercant < 10)
                    {
                        result.Backgroundcolor = "#FFFF00"; //yellow
                        result.WarningCount = 0;
                    }
                    else
                    {
                        result.Backgroundcolor = "#FF0000"; //red
                        result.WarningCount = Convert.ToInt32(result.CancellationPercant / 10);
                    }

                }
            }
            return result;
        }

        [Route("WarehouseDashboardById")]
        [HttpGet]
        public async Task<CancellationReportBackendDc> GetWarehouseDashboardById(int WarehouseId)
        {
            CancellationReportBackendDc result = new CancellationReportBackendDc();
            using (var authContext = new AuthContext())
            {
                if (authContext.Database.Connection.State != ConnectionState.Open)
                    authContext.Database.Connection.Open();
                var cmd = authContext.Database.Connection.CreateCommand();
                cmd.CommandText = "[Cancellation].[WarehouseDashboardById]";
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.Parameters.Add(new SqlParameter("@WarehouseId", WarehouseId));
                var reader = cmd.ExecuteReader();
                var CancellationWarehouseReport = ((IObjectContextAdapter)authContext)
                 .ObjectContext
                 .Translate<CancellationWarehouseDc>(reader).FirstOrDefault();
                if (CancellationWarehouseReport != null)
                {
                    //total
                    result.TotalCount = CancellationWarehouseReport.CurrentTotalCount;
                    result.TotalCountDiff = Convert.ToInt32(CancellationWarehouseReport.LastTotalCount > 0 ? (CancellationWarehouseReport.CurrentTotalCount - CancellationWarehouseReport.LastTotalCount) : 0);

                    //total delivered
                    result.DeliveredTotalCount = CancellationWarehouseReport.CurrentTotalDeliveredCount;
                    result.DeliveredTotalCountDiff = CancellationWarehouseReport.CurrentTotalDeliveredCount - CancellationWarehouseReport.LastTotalDeliveredCount;
                    double currentDeliveredPercent = CancellationWarehouseReport.CurrentTotalDeliveredCount > 0 ? (Convert.ToDouble(CancellationWarehouseReport.CurrentTotalDeliveredCount) / CancellationWarehouseReport.CurrentTotalCount * 100) : 0;
                    double lastDeliveredPercent = CancellationWarehouseReport.LastTotalDeliveredCount > 0 ? (Convert.ToDouble(CancellationWarehouseReport.LastTotalDeliveredCount) / CancellationWarehouseReport.LastTotalCount * 100) : 0;
                    result.CompareDeliveredTotalPercent = Math.Round(currentDeliveredPercent - lastDeliveredPercent, 2);


                    //Cancellation Percent on value
                    result.CancelAmount = CancellationWarehouseReport.CurrentCancelValue;
                    result.CancelAmountDiff = CancellationWarehouseReport.CurrentCancelValue - CancellationWarehouseReport.LastCancelValue;
                    double currentCancelAmountPercent = Math.Round(CancellationWarehouseReport.CurrentCancelValue > 0 ? (Convert.ToDouble(CancellationWarehouseReport.CurrentCancelValue) / CancellationWarehouseReport.CurrentTotalValue * 100) : 0, 2);
                    double lastCancelAmountPercent = Math.Round(CancellationWarehouseReport.LastCancelValue > 0 ? (Convert.ToDouble(CancellationWarehouseReport.LastCancelValue) / CancellationWarehouseReport.LastTotalValue * 100) : 0, 2);
                    result.CompareAmountPercent = Math.Round(currentCancelAmountPercent - lastCancelAmountPercent, 2);


                    //Cancellation order count
                    result.CancelCount = CancellationWarehouseReport.CurrentCancelCount;
                    result.CancelCountDiff = CancellationWarehouseReport.CurrentCancelCount - CancellationWarehouseReport.LastCancelCount;
                    double currentCancelCountPercent = Math.Round(CancellationWarehouseReport.CurrentCancelCount > 0 ? Convert.ToDouble(CancellationWarehouseReport.CurrentCancelCount) / CancellationWarehouseReport.CurrentTotalCount * 100 : 0, 2);
                    double lastCancelCountPercent = Math.Round(CancellationWarehouseReport.LastCancelCount > 0 ? Convert.ToDouble(CancellationWarehouseReport.LastCancelCount) / CancellationWarehouseReport.LastTotalCount * 100 : 0, 2);
                    result.CompareCancelCountPercent = Math.Round(currentCancelCountPercent - lastCancelCountPercent, 2);

                    //Cancellation %
                    result.CancellationPercant = CancellationWarehouseReport.CurrentCancelValue > 0 ? (Convert.ToDouble(CancellationWarehouseReport.CurrentCancelValue) / CancellationWarehouseReport.CurrentTotalValue * 100) : 0;
                    double lastCancellationPercant = CancellationWarehouseReport.LastCancelValue > 0 ? (Convert.ToDouble(CancellationWarehouseReport.LastCancelValue) / CancellationWarehouseReport.LastTotalValue * 100) : 0;
                    result.CompareCancellationPercant = Math.Round(result.CancellationPercant - lastCancellationPercant, 2);

                    if (result.CancellationPercant >= 0 && result.CancellationPercant <= 5)
                    {
                        result.Backgroundcolor = "#FFFFFF";
                        result.WarningCount = 0;
                    }
                    else if (result.CancellationPercant > 5 && result.CancellationPercant < 10)
                    {
                        result.Backgroundcolor = "#FFFF00"; result.WarningCount = 0;
                    }
                    else
                    {
                        result.Backgroundcolor = "#FF0000"; //red
                        result.WarningCount = Convert.ToInt32(result.CancellationPercant / 10);
                    }
                    result.InBoundIncharge = CancellationWarehouseReport.InBoundIncharge;
                    result.OutBoundIncharge = CancellationWarehouseReport.OutBoundIncharge;
                    result.HubIncharge = CancellationWarehouseReport.HubIncharge;
                }
            }
            return result;
        }

        [Route("WarehouseDboyReport")]
        [HttpGet]
        public async Task<List<CancellationReportDboyDc>> WarehouseDboyReport(int WarehouseId)
        {
            List<CancellationReportDboyDc> result = new List<CancellationReportDboyDc>();
            using (var authContext = new AuthContext())
            {
                if (authContext.Database.Connection.State != ConnectionState.Open)
                    authContext.Database.Connection.Open();
                var cmd = authContext.Database.Connection.CreateCommand();
                cmd.CommandText = "[Cancellation].[WarehouseDboyReport]";
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.Parameters.Add(new SqlParameter("@WarehouseId", WarehouseId));
                var reader = cmd.ExecuteReader();
                result = ((IObjectContextAdapter)authContext)
                 .ObjectContext
                 .Translate<CancellationReportDboyDc>(reader).ToList();

                foreach (var item in result)
                {
                    if (item.CancellationPercant >= 0 && item.CancellationPercant <= 5)
                    {
                        item.Backgroundcolor = "#FFFFFF";
                        item.WarningCount = 0;
                    }
                    else if (item.CancellationPercant > 5 && item.CancellationPercant < 10)
                    {
                        item.Backgroundcolor = "#FFFF00"; item.WarningCount = 0;
                    }
                    else
                    {
                        item.Backgroundcolor = "#FF0000"; //red
                        item.WarningCount = Convert.ToInt32(item.CancellationPercant / 10);
                    }
                }
            }
            return result;
        }

        [Route("WarehouseSaleManReport")]
        [HttpGet]
        [AllowAnonymous]
        public async Task<List<CancellationReportSaleManDc>> WarehouseSaleManReport(int WarehouseId)
        {
            List<CancellationReportSaleManDc> result = new List<CancellationReportSaleManDc>();
            using (var authContext = new AuthContext())
            {
                if (authContext.Database.Connection.State != ConnectionState.Open)
                    authContext.Database.Connection.Open();
                var cmd = authContext.Database.Connection.CreateCommand();
                cmd.CommandText = "[Cancellation].[WarehouseSaleManReport]";
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.Parameters.Add(new SqlParameter("@WarehouseId", WarehouseId));

                var reader = cmd.ExecuteReader();
                result = ((IObjectContextAdapter)authContext).ObjectContext.Translate<CancellationReportSaleManDc>(reader).ToList();
                foreach (var item in result)
                {
                    if (item.CancellationPercant >= 0 && item.CancellationPercant <= 5)
                    {
                        item.Backgroundcolor = "#FFFFFF"; //white
                        item.WarningCount = 0;
                    }
                    else if (item.CancellationPercant > 5 && item.CancellationPercant < 10)
                    {
                        item.Backgroundcolor = "#FFFF00"; //yellow
                        item.WarningCount = 0;
                    }
                    else
                    {
                        item.Backgroundcolor = "#FF0000"; //red
                        item.WarningCount = Convert.ToInt32(item.CancellationPercant / 10);
                    }
                }
            }
            return result;
        }

    }
}
