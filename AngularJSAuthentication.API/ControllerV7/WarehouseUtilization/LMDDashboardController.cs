using AngularJSAuthentication.Common.Helpers;
using AngularJSAuthentication.Controllers;
using AngularJSAuthentication.DataContracts.WarehouseUtilization;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace AngularJSAuthentication.API.ControllerV7.WarehouseUtilization
{
    [RoutePrefix("api/LMDDashboard")]
    public class LMDDashboardController : BaseApiController
    {
        #region Vehicle Data
        private async Task<List<LMDDashboardDc>> GetListLMDDashboardPvt(LMDInputDc request)
        {
            using (var context = new AuthContext())
            {
                if (context.Database.Connection.State != ConnectionState.Open)
                    context.Database.Connection.Open();

                var WhIdDts = new DataTable();
                WhIdDts.Columns.Add("IntValue");

                if (request.warehouseId != null && request.warehouseId.Any())
                {
                    foreach (var item in request.warehouseId)
                    {
                        var dr = WhIdDts.NewRow();
                        dr["IntValue"] = item;
                        WhIdDts.Rows.Add(dr);
                    }
                }
                var Whparam = new SqlParameter("WarehouseId", WhIdDts);
                Whparam.SqlDbType = SqlDbType.Structured;
                Whparam.TypeName = "dbo.IntValues";

                var cmd = context.Database.Connection.CreateCommand();
                cmd.CommandTimeout = 900;
                cmd.CommandText = "[dbo].[GetAllVehicleReportOfDate_03]";
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.Parameters.Add(Whparam);
                cmd.Parameters.Add(new SqlParameter("@StartDate", request.startdate));
                cmd.Parameters.Add(new SqlParameter("@EndDate", request.enddate));
                var reader = cmd.ExecuteReader();
                var data = ((IObjectContextAdapter)context).ObjectContext.Translate<LMDDashboardDc>(reader).ToList();
                return data;
            }
        }

        [Route("GetListLMDDashboard")]
        [HttpPost]
        public async Task<List<LMDDashboardDc>> Get(LMDInputDc request)
        {
            return await GetListLMDDashboardPvt(request);
        }

        [Route("ExportLMDDashboard")]
        [HttpPost]
        public async Task<string> ExportLMDDashboard(LMDInputDc request)
        {
            string fileUrl = string.Empty;
            var LMDDashboardList = await GetListLMDDashboardPvt(request);
            if (LMDDashboardList != null)
            {
                string zipfilename = "_" + DateTime.Now.ToString("yyyyddMHHmmss") + "_LMDDashboardListExport.zip";
                var fileName = "_" + DateTime.Now.ToString("yyyyddMHHmmss") + "_LMDDashboardListExport.csv";
                DataTable dt = ListtoDataTableConverter.ToDataTable(LMDDashboardList);

                // rearrange DataTable columns
                dt.Columns["VehicleNo"].SetOrdinal(0);
                dt.Columns["VehicleType"].SetOrdinal(1);
                dt.Columns["CreatedDate"].SetOrdinal(2);
                dt.Columns["HubName"].SetOrdinal(3);
                dt.Columns["TransportName"].SetOrdinal(4);
                dt.Columns["NumberOfTrips"].SetOrdinal(5);
                dt.Columns["TripNos"].SetOrdinal(6);
                dt.Columns["ThresholdOrderCount"].SetOrdinal(7);
                dt.Columns["OrderCount"].SetOrdinal(8);
                dt.Columns["UtilizationPercentageOnOrderCount"].SetOrdinal(9);
                dt.Columns["ThresholdOrderAmount"].SetOrdinal(10);
                dt.Columns["OrderValue"].SetOrdinal(11);
                dt.Columns["UtilizationPercentageOnOrderValue"].SetOrdinal(12);
                dt.Columns["InProcessOrderCount"].SetOrdinal(13);
                dt.Columns["InProcessOrderAmount"].SetOrdinal(14);
                dt.Columns["DeliverdCount"].SetOrdinal(15);
                dt.Columns["ThresholdVSDeliveryPercentageOnOrderCount"].SetOrdinal(16);
                dt.Columns["CarriedVSDeliveryPercentageOnOrderCount"].SetOrdinal(17);
                dt.Columns["ThresholdVSDeliveryPercentOnOrderValue"].SetOrdinal(18);
                dt.Columns["CarriedVSDeliveryPercentageOnOrderValue"].SetOrdinal(19);
                dt.Columns["DeliverdValue"].SetOrdinal(20);
                dt.Columns["RedispatchCount"].SetOrdinal(21);
                dt.Columns["RedispatchValue"].SetOrdinal(22);
                dt.Columns["DCCount"].SetOrdinal(23);
                dt.Columns["DCValue"].SetOrdinal(24);
                dt.Columns["ReattemptCount"].SetOrdinal(25);
                dt.Columns["ReattemptValue"].SetOrdinal(26);
                dt.Columns["WorkingDuration"].SetOrdinal(27);
                dt.Columns["Assignments"].SetOrdinal(28);
                dt.Columns["TotalKmExcludingRunningTrip"].SetOrdinal(29);

                string path = Path.Combine(HttpContext.Current.Server.MapPath("~/ExcelGeneratePath"), fileName);
                dt.WriteToCsvFile(path);


                return $"/ExcelGeneratePath/{fileName}";
            }
            return "";
        }

        private async Task<List<FTLFullDataDc>> FTLDataFullPvt(LMDInputDc request)
        {
            var data = await GetListLMDDashboardPvt(request);
            if (data != null && data.Any())
            {
                var output = data
                    .Select(y => new FTLFullDataDc
                    {
                        HubName = y.HubName,
                        Date = y.CreatedDate,
                        DCCount = y.DCCount,
                        DCValue = y.DCValue,
                        DeliverdCount = y.DeliverdCount,
                        DeliverdValue = y.DeliverdValue,
                        OrderCount = y.OrderCount,
                        OrderValue = y.OrderValue,
                        ReattemptCount = y.ReattemptCount,
                        ReattemptValue = y.ReattemptValue,
                        RedispatchCount = y.RedispatchCount,
                        RedispatchValue = y.RedispatchValue,
                        VehicleNumber = y.VehicleNo,
                        ThresholdOrderCount = y.ThresholdOrderCount,
                        ThresholdOrderAmount = y.ThresholdOrderAmount,
                        FTLCount = y.DeliverdCount + y.RedispatchCount + y.DCCount,
                        FTLValue = y.DeliverdValue + y.RedispatchValue + y.DCValue,
                        FTLCountPerCentage = decimal.Round((decimal)((y.DeliverdCount + y.RedispatchCount + y.DCCount) / (double)y.ThresholdOrderCount * 100.0), 2),
                        FTLAmountPerCentage = decimal.Round((decimal)((y.DeliverdValue + y.RedispatchValue + y.DCValue) / (double)y.ThresholdOrderAmount * 100.0), 2),
                        IsFTL = false
                    }).ToList();

                foreach (var item in output)
                {
                    item.IsFTL = (item.FTLCountPerCentage >= 100 || item.FTLAmountPerCentage >= 100) ? true : false;
                }
                return output;
            }
            else
            {
                return null;
            }
        }

        [Route("ExportFTLFullData")]
        [HttpPost]
        public async Task<string> ExportFTLFullData(LMDInputDc request)
        {
            string fileUrl = string.Empty;
            var FTLDataList = await FTLDataFullPvt(request);
            if (FTLDataList != null)
            {
                string zipfilename = "_" + DateTime.Now.ToString("yyyyddMHHmmss") + "_FTLDataListExport.zip";
                var fileName = "_" + DateTime.Now.ToString("yyyyddMHHmmss") + "_FTLDataListExport.csv";
                DataTable dt = ListtoDataTableConverter.ToDataTable(FTLDataList);

                // rearrange DataTable columns
                dt.Columns["HubName"].SetOrdinal(0);
                dt.Columns["VehicleNumber"].SetOrdinal(1);
                dt.Columns["Date"].SetOrdinal(2);
                dt.Columns["IsFTL"].SetOrdinal(3);
                dt.Columns["FTLCount"].SetOrdinal(4);
                dt.Columns["FTLValue"].SetOrdinal(5);
                dt.Columns["FTLCountPerCentage"].SetOrdinal(6);
                dt.Columns["FTLAmountPerCentage"].SetOrdinal(7);
                dt.Columns["ThresholdOrderCount"].SetOrdinal(8);
                dt.Columns["OrderCount"].SetOrdinal(9);
                dt.Columns["DeliverdCount"].SetOrdinal(10);
                dt.Columns["RedispatchCount"].SetOrdinal(11);
                dt.Columns["DCCount"].SetOrdinal(12);
                dt.Columns["ReattemptCount"].SetOrdinal(13);
                dt.Columns["ThresholdOrderAmount"].SetOrdinal(14);
                dt.Columns["OrderValue"].SetOrdinal(15);
                dt.Columns["DeliverdValue"].SetOrdinal(16);
                dt.Columns["RedispatchValue"].SetOrdinal(17);
                dt.Columns["DCValue"].SetOrdinal(18);
                dt.Columns["ReattemptValue"].SetOrdinal(19);
                string path = Path.Combine(HttpContext.Current.Server.MapPath("~/ExcelGeneratePath"), fileName);
                dt.WriteToCsvFile(path);

                return $"/ExcelGeneratePath/{fileName}";
            }
            return "";
        }

        private async Task<List<FTLDataDc>> FTLDataPvt(LMDInputDc request)
        {
            var data = await GetListLMDDashboardPvt(request);
            if (data != null && data.Any())
            {
                var output = data.GroupBy(x => x.HubName)
                    .Select(y => new FTLDataDc
                    {
                        HubName = y.Key,
                        TotalVehicle = y.Count(),
                        FTLVehiclesByCount = y.Count(x => x.ThresholdOrderCount <= (x.DeliverdCount + x.RedispatchCount + x.DCCount)),
                        FTLVehiclesByAmount = y.Count(x => x.ThresholdOrderAmount <= (x.DeliverdValue + x.RedispatchValue + x.DCValue)),
                        FTLVehiclesOverAll = y.Count(x => x.ThresholdOrderCount <= (x.DeliverdCount + x.RedispatchCount + x.DCCount) || x.ThresholdOrderAmount <= (x.DeliverdValue + x.RedispatchValue + x.DCValue))
                    }).ToList();

                foreach (var item in output)
                {
                    item.FTLVehiclesByCountPerCentage = Math.Round((decimal)item.FTLVehiclesByCount / (decimal)item.TotalVehicle * 100, 2);
                    item.FTLVehiclesByAmountPerCentage = Math.Round((decimal)item.FTLVehiclesByAmount / (decimal)item.TotalVehicle * 100, 2);
                    item.FTLVehiclesOverAllPerCentage = Math.Round((decimal)item.FTLVehiclesOverAll / (decimal)item.TotalVehicle * 100, 2);
                }
                return output;
            }
            else
            {
                return null;
            }
        }

        [Route("GetFTLData")]
        [HttpPost]
        public async Task<List<FTLDataDc>> GetFTLDataList(LMDInputDc request)
        {
            return await FTLDataPvt(request);
        }

        [Route("ExportFTLData")]
        [HttpPost]
        public async Task<string> ExportFTLData(LMDInputDc request)
        {
            string fileUrl = string.Empty;
            var FTLDataList = await FTLDataPvt(request);
            if (FTLDataList != null)
            {
                string zipfilename = "_" + DateTime.Now.ToString("yyyyddMHHmmss") + "_FTLDataListExport.zip";
                var fileName = "_" + DateTime.Now.ToString("yyyyddMHHmmss") + "_FTLDataListExport.csv";
                DataTable dt = ListtoDataTableConverter.ToDataTable(FTLDataList);

                // rearrange DataTable columns
                dt.Columns["HubName"].SetOrdinal(0);
                dt.Columns["TotalVehicle"].SetOrdinal(1);
                dt.Columns["FTLVehiclesByCount"].SetOrdinal(2);
                dt.Columns["FTLVehiclesByAmount"].SetOrdinal(3);
                dt.Columns["FTLVehiclesOverAll"].SetOrdinal(4);
                dt.Columns["FTLVehiclesByCountPerCentage"].SetOrdinal(5);
                dt.Columns["FTLVehiclesByAmountPerCentage"].SetOrdinal(6);
                dt.Columns["FTLVehiclesOverAllPerCentage"].SetOrdinal(7);

                string path = Path.Combine(HttpContext.Current.Server.MapPath("~/ExcelGeneratePath"), fileName);
                dt.WriteToCsvFile(path);

                return $"/ExcelGeneratePath/{fileName}";
            }
            return "";
        }
        #endregion

        #region Warehouse Data
        private async Task<List<LMDDashboardChartDc>> GetListLMDChartPvt(LMDInputDc request)
        {
            using (var context = new AuthContext())
            {
                if (context.Database.Connection.State != ConnectionState.Open)
                    context.Database.Connection.Open();

                var WhIdDts = new DataTable();
                WhIdDts.Columns.Add("IntValue");

                if (request.warehouseId != null && request.warehouseId.Any())
                {
                    foreach (var item in request.warehouseId)
                    {
                        var dr = WhIdDts.NewRow();
                        dr["IntValue"] = item;
                        WhIdDts.Rows.Add(dr);
                    }
                }
                var Whparam = new SqlParameter("WarehouseId", WhIdDts);
                Whparam.SqlDbType = SqlDbType.Structured;
                Whparam.TypeName = "dbo.IntValues";

                var cmd = context.Database.Connection.CreateCommand();
                cmd.CommandTimeout = 900;
                cmd.CommandText = "[dbo].[GetAllVehicleReportOfDate_04]";
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.Parameters.Add(Whparam);
                cmd.Parameters.Add(new SqlParameter("@StartDate", request.startdate));
                cmd.Parameters.Add(new SqlParameter("@EndDate", request.enddate));
                var reader = cmd.ExecuteReader();
                var data = ((IObjectContextAdapter)context).ObjectContext.Translate<LMDDashboardChartDc>(reader).ToList();
                return data;
            }
        }

        [Route("GetListLMDChart")]
        [HttpPost]
        public async Task<List<LMDDashboardChartDc>> GetListLMDChart(LMDInputDc request)
        {
            return await GetListLMDChartPvt(request);
        }

        [Route("ExportLMDChart")]
        [HttpPost]
        public async Task<string> ExportLMDChart(LMDInputDc request)
        {
            string fileUrl = string.Empty;
            var LMDChartList = await GetListLMDChartPvt(request);
            if (LMDChartList != null)
            {
                string zipfilename = "_" + DateTime.Now.ToString("yyyyddMHHmmss") + "_LMDDashboardListExport.zip";
                var fileName = "_" + DateTime.Now.ToString("yyyyddMHHmmss") + "_LMDDashboardListExport.csv";
                DataTable dt = ListtoDataTableConverter.ToDataTable(LMDChartList);

                // rearrange DataTable columns
                dt.Columns["HubName"].SetOrdinal(0);
                dt.Columns["NumberOfTrips"].SetOrdinal(1);
                dt.Columns["ThresholdOrderCount"].SetOrdinal(2);
                dt.Columns["OrderCount"].SetOrdinal(3);
                dt.Columns["UtilizationPercentageOnOrderCount"].SetOrdinal(4);
                dt.Columns["ThresholdOrderAmount"].SetOrdinal(5);
                dt.Columns["OrderValue"].SetOrdinal(6);
                dt.Columns["UtilizationPercentageOnOrderValue"].SetOrdinal(7);
                dt.Columns["OverallUtilizationPercentage"].SetOrdinal(8);
                dt.Columns["InProcessOrderCount"].SetOrdinal(9);
                dt.Columns["InProcessOrderAmount"].SetOrdinal(10);
                dt.Columns["DeliverdCount"].SetOrdinal(11);
                dt.Columns["ThresholdVSDeliveryPercentageOnOrderCount"].SetOrdinal(12);
                dt.Columns["CarriedVSDeliveryPercentageOnOrderCount"].SetOrdinal(13);
                dt.Columns["DeliverdValue"].SetOrdinal(14);
                dt.Columns["ThresholdVSDeliveryPercentOnOrderValue"].SetOrdinal(15);
                dt.Columns["CarriedVSDeliveryPercentageOnOrderValue"].SetOrdinal(16);
                dt.Columns["RedispatchCount"].SetOrdinal(17);
                dt.Columns["RedispatchValue"].SetOrdinal(18);
                dt.Columns["DCCount"].SetOrdinal(19);
                dt.Columns["DCValue"].SetOrdinal(20);
                dt.Columns["ReattemptCount"].SetOrdinal(21);
                dt.Columns["ReattemptValue"].SetOrdinal(22);
                dt.Columns["TotalKmExcludingRunningTrip"].SetOrdinal(23);

                string path = Path.Combine(HttpContext.Current.Server.MapPath("~/ExcelGeneratePath"), fileName);
                dt.WriteToCsvFile(path);


                return $"/ExcelGeneratePath/{fileName}";
            }
            return "";
        }

        #endregion



        #region decipline data


        [AllowAnonymous]
        [Route("GetDesciplineRawData")]
        [HttpPost]
        public async Task<List<DesciplineDataDc>> GetDesciplineRawData(LMDInputDc request)
        {
            var result = await GetDesciplineDataPvt(request);
            return result;
        }


        [AllowAnonymous]
        [Route("GetDesciplineDataReport")]
        [HttpPost]
        public async Task<string> GetDesciplineDataReport(LMDInputDc request)
        {
            var result = await GetDesciplineDataPvt(request);
            if (result != null)
            {
                string zipfilename = "_" + DateTime.Now.ToString("yyyyddMHHmmss") + "_LMDDashboardListExport.zip";
                var fileName = "_" + DateTime.Now.ToString("yyyyddMHHmmss") + "_LMDDashboardListExport.csv";
                DataTable dt = ListtoDataTableConverter.ToDataTable(result);

                // rearrange DataTable columns
                dt.Columns["HubName"].SetOrdinal(0);
                dt.Columns["VehicleNo"].SetOrdinal(1);
                dt.Columns["AttendanceDate"].SetOrdinal(2);
                dt.Columns["ThresholdOrders"].SetOrdinal(3);
                dt.Columns["T10"].SetOrdinal(4);
                dt.Columns["T11"].SetOrdinal(5);
                dt.Columns["T12"].SetOrdinal(6);
                dt.Columns["T13"].SetOrdinal(7);
                dt.Columns["T14"].SetOrdinal(8);
                dt.Columns["T15"].SetOrdinal(9);
                dt.Columns["T16"].SetOrdinal(10);
                dt.Columns["T17"].SetOrdinal(11);
                dt.Columns["T18"].SetOrdinal(12);
                dt.Columns["T19"].SetOrdinal(13);
                dt.Columns["IsDisciplined"].SetOrdinal(14);
                dt.Columns["DisciplinePercentage"].SetOrdinal(15);
                dt.Columns["VehichleMasterId"].SetOrdinal(16);

                dt.Columns.Remove("VehichleMasterId");
                string path = Path.Combine(HttpContext.Current.Server.MapPath("~/ExcelGeneratePath"), fileName);
                dt.WriteToCsvFile(path);


                return $"/ExcelGeneratePath/{fileName}";
            }
            return null;
        }

        [AllowAnonymous]
        [Route("GetDesciplineChartData")]
        [HttpPost]
        public async Task<List<DesciplineChartDataDc>> GetDesciplineChartData(LMDInputDc request)
        {
            var result = await GetDesciplineDataPvt(request);
            if (result != null && result.Any())
            {
                var newResult = result
                                    .GroupBy(x => x.HubName)
                                    .Select(y => new DesciplineChartDataDc
                                    {
                                        HubName = y.Key,
                                        DesciplineVehicleDays = y.Count(z => z.IsDisciplined == true),
                                        TotalVehicleDays = y.Count()
                                    }).ToList();
                return newResult;
            }
            return null;
        }


        private async Task<List<DesciplineDataDc>> GetDesciplineDataPvt(LMDInputDc request)
        {
            using (var context = new AuthContext())
            {
                if (context.Database.Connection.State != ConnectionState.Open)
                    context.Database.Connection.Open();

                var WhIdDts = new DataTable();
                WhIdDts.Columns.Add("IntValue");

                if (request.warehouseId != null && request.warehouseId.Any())
                {
                    foreach (var item in request.warehouseId)
                    {
                        var dr = WhIdDts.NewRow();
                        dr["IntValue"] = item;
                        WhIdDts.Rows.Add(dr);
                    }
                }
                var Whparam = new SqlParameter("WarehouseId", WhIdDts);
                Whparam.SqlDbType = SqlDbType.Structured;
                Whparam.TypeName = "dbo.IntValues";

                var cmd = context.Database.Connection.CreateCommand();
                cmd.CommandTimeout = 900;
                cmd.CommandText = "[Operation].[VehicleDeciplineReport]";
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.Parameters.Add(Whparam);
                cmd.Parameters.Add(new SqlParameter("@StartDate", request.startdate));
                cmd.Parameters.Add(new SqlParameter("@EndDate", request.enddate));
                var reader = cmd.ExecuteReader();
                var data = ((IObjectContextAdapter)context).ObjectContext.Translate<DesciplineDataDc>(reader).ToList();
                return data;
            }
        }

        #endregion

    }
}
