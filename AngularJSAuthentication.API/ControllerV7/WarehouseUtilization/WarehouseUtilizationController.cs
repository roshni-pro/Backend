using AngularJSAuthentication.API.Managers.WarehouseUtilization;
using AngularJSAuthentication.Common.Constants;
using AngularJSAuthentication.Common.Helpers;
using AngularJSAuthentication.Controllers;
using AngularJSAuthentication.DataContracts.WarehouseUtilization;
using Common.Logging;
using NLog;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace AngularJSAuthentication.API.ControllerV7.WarehouseUtilization
{
    [RoutePrefix("api/WarehouseUtilization")]
    public class WarehouseUtilizationController : BaseApiController
    {
        private static Logger logger = NLog.LogManager.GetCurrentClassLogger();
        private static TimeZoneInfo INDIAN_ZONE = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
        DateTime indianTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE);

        [AllowAnonymous]
        [HttpGet]
        [Route("SaveOldDataDayWise")]
        public async Task<UtilResponseDc> SaveOldDataDayWise()
        {
            DateTime TodayDate = DateTime.Today;
            WarehouseUtilizationManager manager = new WarehouseUtilizationManager();
            var list = await manager.SaveOldDataDayWise(TodayDate);
            return list;
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("UpdateVehicleCount")]
        public async Task<UtilResponseDc> UpdateVehicleCount(UpdateVehicleCountDc updateVehicleCountDc)
        {
            if (updateVehicleCountDc.ETADate <= DateTime.Today)
            {
                return new UtilResponseDc
                {
                    Message = "Cant update",
                    Status = false
                };
            }
            WarehouseUtilizationManager manager = new WarehouseUtilizationManager();

            var list = await manager.UpdateVehicleCount(updateVehicleCountDc);
            return list;
        }

        /// <summary>
        /// we assume if futer data want to see then
        /// </summary>
        /// <param name="warehouseUtilVm"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost]
        [Route("GetWarehouseUtilizationList")]
        public async Task<List<GetUtilizationList>> GetWarehouseUtilizationList(WarehouseUtilVm warehouseUtilVm)
        {
            WarehouseUtilizationManager manager = new WarehouseUtilizationManager();
            var list = await manager.GetWarehouseUtilizationList(warehouseUtilVm);
            return list;

        }

        [AllowAnonymous]
        [HttpPost]
        [Route("ExportWarehouseUtilizationList")]
        public async Task<string> ExportWarehouseUtilizationList(WarehouseUtilVm warehouseUtilVm)
        {
            string fileUrl = string.Empty;
            WarehouseUtilizationManager manager = new WarehouseUtilizationManager();

            var list = await manager.GetWarehouseUtilizationList(warehouseUtilVm);

            if (list != null)
            {
                List<ExportUtilizationList> exportUtilizationLists = new List<ExportUtilizationList>();
                foreach (var item in list)
                {
                    exportUtilizationLists.Add(new ExportUtilizationList
                    {
                        Date = item.ETADate.ToString("dd/MM/yyyy"),
                        WarehouseName = item.WarehouseName,
                        OrderedCount = item.DemandOrderCount,
                        CumulativePendingCount = item.CumulativePendingCount,
                        TotalDemand = item.DemandOrderCount + item.CumulativePendingCount,
                        VehicleCountAvailable = item.VehicleCountAvailable,
                        RequiredVehicleCount = item.VehicleCountRequired,
                        ExecutedOrderCount = item.ExecutedOrderCount,
                        DeliveredOrderCount = item.DeliveredOrderCount,
                        DeliveredPercentage = item.ExecutedOrderCount > 0 ? (double?)Math.Ceiling(item.DeliveredOrderCount / Convert.ToDouble(item.ExecutedOrderCount) * 100.0) : null,
                        PlannedTouchPoints = item.TouchPointCapacity,
                        VisitedTouchPoints = item.TouchPointUtilization,
                        TouchPointUtilizedPercentage = item.TouchPointCapacity > 0 ? (double?)Math.Ceiling(item.TouchPointUtilization / Convert.ToDouble(item.TouchPointCapacity) * 100.0) : null,
                        PlannedOrderAmount = item.PlannedOrderAmount,
                        VisitedOrderAmount = item.OrderAmountUtilization,
                        DboyCost = item.DboyCost
                        //OverallUtilPercentage = item.OverallUtilPercentage,


                        //ETADemand = item.CumulativePendingETACount,
                        //ETAChangedDemand = item.CumulativePendingChangeETACount,

                        //DeliveryDate = item.ETADate,
                        //VehicleCountRequired = item.ExtraVehicleCount,




                    });
                }
                string zipfilename = "_" + DateTime.Now.ToString("yyyyddMHHmmss") + "_WarehouseUtilizationListExport.zip";
                var fileName = "_" + DateTime.Now.ToString("yyyyddMHHmmss") + "_WarehouseUtilizationListExport.csv";
                DataTable dt = ListtoDataTableConverter.ToDataTable(exportUtilizationLists);

                // rearrange DataTable columns
                dt.Columns["Date"].SetOrdinal(0);
                dt.Columns["WarehouseName"].SetOrdinal(1);
                dt.Columns["OrderedCount"].SetOrdinal(2);
                dt.Columns["CumulativePendingCount"].SetOrdinal(3);
                dt.Columns["TotalDemand"].SetOrdinal(4);
                dt.Columns["VehicleCountAvailable"].SetOrdinal(5);
                dt.Columns["RequiredVehicleCount"].SetOrdinal(6);
                dt.Columns["ExecutedOrderCount"].SetOrdinal(7);
                dt.Columns["DeliveredOrderCount"].SetOrdinal(8);
                dt.Columns["DeliveredPercentage"].SetOrdinal(9);
                dt.Columns["PlannedTouchPoints"].SetOrdinal(10);
                dt.Columns["VisitedTouchPoints"].SetOrdinal(11);
                dt.Columns["TouchPointUtilizedPercentage"].SetOrdinal(12);
                dt.Columns["PlannedOrderAmount"].SetOrdinal(13);
                dt.Columns["VisitedOrderAmount"].SetOrdinal(14);
                dt.Columns["DboyCost"].SetOrdinal(15);

                string path = Path.Combine(HttpContext.Current.Server.MapPath("~/ExcelGeneratePath"), fileName);
                dt.WriteToCsvFile(path);


                return $"/ExcelGeneratePath/{fileName}";
            }
            else
            {
                return "";
            }
            // return list;
        }


        #region Mail
        [HttpGet]
        [Route("WarehouseUtilReportMail")]
        [AllowAnonymous]
        public async Task<string> WarehouseUtilReport()
        {
            DateTime TodayDate = DateTime.Today;
            //var toMail = ConfigurationManager.AppSettings["WarehouseUtilReportToMailList"];
            //var bccMail = ConfigurationManager.AppSettings["WarehouseUtilReportBCCMailList"];
            bool isSent = false;
            List<GetUtilizationListReport> exportUtilizationLists = new List<GetUtilizationListReport>();
            using (AuthContext context = new AuthContext())
            {
                string fileUrl = string.Empty;
                string pathh = string.Empty;
                WarehouseUtilizationManager manager = new WarehouseUtilizationManager();

                var list = await manager.GetWarehouseUtilizationListtt();
                if (list != null)
                {
                    foreach (var item in list)
                    {
                        exportUtilizationLists.Add(new GetUtilizationListReport
                        {

                            WarehouseName = item.WarehouseName,
                            ETADate = item.ETADate,
                            PendingETACount = item.CumulativePendingETACount,
                            PendingChangeETACount = item.CumulativePendingChangeETACount,
                            CumulativePendingCount = item.CumulativePendingCount,
                            TotalDemandOrderCount = item.DemandOrderCount,
                            VehicleCountRequired = item.VehicleCountRequired,
                            VehicleCountAvailable = item.VehicleCountAvailable,
                            PlannedThresholdOrderCount = item.PlannedThresholdOrderCount,
                            TouchPointCapacity = item.TouchPointCapacity,
                            TouchPointUtilization = item.TouchPointUtilization,
                            ExecutedOrderCount = item.ExecutedOrderCount,
                            DeliveredOrderCount = item.DeliveredOrderCount,
                            DemandOrderAmount = item.DemandOrderAmount,
                            PlannedOrderAmount = item.PlannedOrderAmount,
                            OrderAmountUtilization = item.OrderAmountUtilization,
                            DeliveredOrderAmount = item.DeliveredOrderAmount,
                            OrderCountUtilPercentage = item.OrderCountUtilPercentage,
                            DeliveredPercentage = item.DeliveredPercentage,
                            TouchPointUtilPercentage = item.TouchPointUtilPercentage,
                            OrderAmountUtilPercentage = item.OrderAmountUtilPercentage,
                            OverallUtilPercentage = item.OverallUtilPercentage,
                            ExtraVehicleCount = item.ExtraVehicleCount,
                            ExtraVehicleCapacityInKg = item.ExtraVehicleCapacityInKg,
                            CumulativePendingOrderAmount = item.CumulativePendingOrderAmount,
                            WarehouseId = item.WarehouseId,
                            ThisOrNextDayPendingETACount = item.ThisOrNextDayPendingETACount,
                            DboyCost = item.DboyCost,
                            RedOrderCount = item.RedOrderCount,
                            FixedThresholdOrderCount = item.FixedThresholdOrderCount,
                        });
                    }
                    string zipfilename = "_" + DateTime.Now.ToString("yyyyddMHHmmss") + "_WarehouseUtilizationListExport.zip";
                    var fileName = "_" + DateTime.Now.ToString("yyyyddMHHmmss") + "_WarehouseUtilizationListExport.csv";
                    DataTable dt = ListtoDataTableConverter.ToDataTable(exportUtilizationLists);

                    // rearrange DataTable columns
                    dt.Columns["WarehouseName"].SetOrdinal(0);
                    dt.Columns["ETADate"].SetOrdinal(1);
                    dt.Columns["PendingETACount"].SetOrdinal(2);
                    dt.Columns["PendingChangeETACount"].SetOrdinal(3);
                    dt.Columns["CumulativePendingCount"].SetOrdinal(4);
                    dt.Columns["TotalDemandOrderCount"].SetOrdinal(5);
                    dt.Columns["VehicleCountRequired"].SetOrdinal(6);
                    dt.Columns["VehicleCountAvailable"].SetOrdinal(7);
                    dt.Columns["PlannedThresholdOrderCount"].SetOrdinal(8);
                    dt.Columns["TouchPointCapacity"].SetOrdinal(9);
                    dt.Columns["TouchPointUtilization"].SetOrdinal(10);
                    dt.Columns["ExecutedOrderCount"].SetOrdinal(11);
                    dt.Columns["DeliveredOrderCount"].SetOrdinal(12);
                    dt.Columns["DemandOrderAmount"].SetOrdinal(13);
                    dt.Columns["PlannedOrderAmount"].SetOrdinal(14);
                    dt.Columns["OrderAmountUtilization"].SetOrdinal(15);
                    dt.Columns["DeliveredOrderAmount"].SetOrdinal(16);
                    dt.Columns["OrderCountUtilPercentage"].SetOrdinal(17);
                    dt.Columns["DeliveredPercentage"].SetOrdinal(18);
                    dt.Columns["TouchPointUtilPercentage"].SetOrdinal(19);
                    dt.Columns["OrderAmountUtilPercentage"].SetOrdinal(20);
                    dt.Columns["OverallUtilPercentage"].SetOrdinal(21);
                    dt.Columns["ExtraVehicleCount"].SetOrdinal(22);
                    dt.Columns["ExtraVehicleCapacityInKg"].SetOrdinal(23);
                    dt.Columns["CumulativePendingOrderAmount"].SetOrdinal(24);
                    dt.Columns["WarehouseId"].SetOrdinal(25);
                    dt.Columns["ThisOrNextDayPendingETACount"].SetOrdinal(26);
                    dt.Columns["DboyCost"].SetOrdinal(27);
                    dt.Columns["RedOrderCount"].SetOrdinal(28);
                    dt.Columns["FixedThresholdOrderCount"].SetOrdinal(29);

                    pathh = HttpContext.Current.Server.MapPath("~/ExcelGeneratePath/WarehouseCapacityReport");
                    if (!Directory.Exists(pathh))
                        Directory.CreateDirectory(pathh);
                    string excelPath = Path.Combine(pathh, fileName);
                    dt.WriteToCsvFile(excelPath);


                    //return $"/ExcelGeneratePath/{fileName}";
                }
                else
                {
                    return "";
                }

                //var dateParam = new SqlParameter("@TodayDate", TodayDate);
                //var excelData = context.Database.SqlQuery<GetUtilizationList>("[dbo].[WarehouseCapacityReport] @TodayDate", dateParam).ToList();
                //if (excelData != null && excelData.Any())
                //{
                //string ExcelSavePath = HttpContext.Current.Server.MapPath("~/ExcelGeneratePath/WarehouseCapacityReport");
                //if (!Directory.Exists(pathh))
                //       Directory.CreateDirectory(pathh);

                DataTable dt1 = ClassToDataTable.CreateDataTable(exportUtilizationLists);
                string fileName1 = $"DailyWarehouseUtilReport_{DateTime.Now.ToString("yyyy-dd-M_HH.mm.ss.fff")}.xlsx";
                string filePath = Path.Combine(pathh, fileName1);
                if (ExcelGenerator.DataTable_To_Excel(dt1, "WarehouseUtilReport", filePath))
                {
                    if (exportUtilizationLists.Count > 0)
                    {

                        if (!string.IsNullOrEmpty(fileName1))
                        {
                            string connectionString = ConfigurationManager.ConnectionStrings["AuthContext"].ConnectionString;
                            string To = "", From = "", Bcc = "";
                            DataTable emaildatatable = new DataTable();
                            using (var connection = new SqlConnection(connectionString))
                            {
                                using (var command = new SqlCommand("Select * from EmailRecipients where EmailType='WarehouseUtilReport'", connection))
                                {

                                    if (connection.State != ConnectionState.Open)
                                        connection.Open();

                                    SqlDataAdapter da = new SqlDataAdapter(command);
                                    da.Fill(emaildatatable);
                                    da.Dispose();
                                    connection.Close();
                                }
                            }
                            //    if (emaildatatable.Rows.Count > 0)
                            //    {
                            //        From = !string.IsNullOrEmpty(emaildatatable.Rows[0]["From"].ToString()) ? emaildatatable.Rows[0]["From"].ToString() : From;
                            //        To = !string.IsNullOrEmpty(emaildatatable.Rows[0]["To"].ToString()) ? emaildatatable.Rows[0]["To"].ToString() : To;
                            //        Bcc = !string.IsNullOrEmpty(emaildatatable.Rows[0]["Bcc"].ToString()) ? emaildatatable.Rows[0]["Bcc"].ToString() : "";
                            //    }

                            //    string subject = DateTime.Now.ToString("dd MMM yyyy") + "Warehouse Utilaztion Report ";
                            //    string FileUrl = string.Format("{0}://{1}{2}/{3}", new Uri((HttpContext.Current.Request.UrlReferrer != null ? HttpContext.Current.Request.UrlReferrer.AbsoluteUri : HttpContext.Current.Request.Url.AbsoluteUri)).Scheme
                            //                                                  , HttpContext.Current.Request.Url.DnsSafeHost
                            //                                                  , (HttpContext.Current.Request.Url.Port != 80 && HttpContext.Current.Request.Url.Port != 443 ? ":" + HttpContext.Current.Request.Url.Port : "")
                            //                                                  , "/ExcelGeneratePath/" + filePath);

                            //    string message = "Please find attach warehouse utilaztion report :-" + FileUrl;
                            //    if (!string.IsNullOrEmpty(To) && !string.IsNullOrEmpty(From))
                            //        // EmailHelper.SendMail(From, To, Bcc, subject, message, zipfilename);
                            //        EmailHelper.SendMail(From, To, Bcc, subject, message, FileUrl);

                            //    else
                            //        logger.Error("warehouseUtil Report To and From empty");
                            //}
                            if (emaildatatable.Rows.Count > 0)
                            {
                                To = !string.IsNullOrEmpty(emaildatatable.Rows[0]["To"].ToString()) ? emaildatatable.Rows[0]["To"].ToString() : To;
                                From = !string.IsNullOrEmpty(emaildatatable.Rows[0]["From"].ToString()) ? emaildatatable.Rows[0]["From"].ToString() : From;
                                Bcc = !string.IsNullOrEmpty(emaildatatable.Rows[0]["Bcc"].ToString()) ? emaildatatable.Rows[0]["Bcc"].ToString() : "";
                            }
                            StringBuilder sb = new StringBuilder();
                            string message = "Please find attach Warehouse Vehicle Planning Report";
                            if (list.Any(x => x.PlannedThresholdOrderCount < x.DemandOrderCount))
                            {
                                DataTable dt = new DataTable();
                                dt.Columns.AddRange(new DataColumn[4] {
                                    new DataColumn("S.no", typeof(int)),
                                    new DataColumn("WarehouseName", typeof(string)),
                                                                        new DataColumn("TotalDemandOrderCount", typeof(int)),
                                                                        new DataColumn("PlannedThresholdOrderCount",typeof(int)) });
                                int sNO = 1;
                                foreach (var value in list.Where(x => x.PlannedThresholdOrderCount < x.DemandOrderCount))
                                {
                                    dt.Rows.Add(sNO, value.WarehouseName, value.DemandOrderCount, value.PlannedThresholdOrderCount);
                                    sNO++;
                                };


                                sb.AppendFormat(message);
                                //Table start.
                                sb.Append("<table cellpadding='5' cellspacing='0' style='border: 1px solid #ccc;font-size: 9pt;font-family:Arial'>");

                                //Adding HeaderRow.
                                sb.Append("<tr>");
                                foreach (DataColumn column in dt.Columns)
                                {
                                    sb.Append("<th style='background-color: #B8DBFD;border: 1px solid #ccc'>" + column.ColumnName + "</th>");
                                }
                                sb.Append("</tr>");


                                //Adding DataRow.
                                foreach (DataRow row in dt.Rows)
                                {
                                    sb.Append("<tr>");
                                    foreach (DataColumn column in dt.Columns)
                                    {
                                        sb.Append("<td style='width:100px;border: 1px solid #ccc'>" + row[column.ColumnName].ToString() + "</td>");
                                    }
                                    sb.Append("</tr>");
                                }

                                //Table end.
                                sb.Append("</table>");

                            }
                            string subject = DateTime.Now.ToString("dd MMM yyyy") + "Warehouse Vehicle Planning Report";

                            //await GetBillDiscountFreeItemData();
                            if (!string.IsNullOrEmpty(To) && !string.IsNullOrEmpty(From))
                                EmailHelper.SendMail(AppConstants.MasterEmail, To, Bcc, subject, sb.ToString(), filePath);
                            else
                                logger.Error("Warehouse Vehicle Planning Report To and From empty");
                        }
                    }
                }
                //}
            }
            return "isSent";
        }
        #endregion


        [AllowAnonymous]
        [HttpGet]
        [Route("WarehouseUtilizationReport")]
        public async Task<bool> WarehouseUtilizationReport()
        {
            DateTime enddate = DateTime.Today;
            DateTime startdate = enddate.AddDays(-1);

            string fileUrl = string.Empty;
            WarehouseUtilizationManager manager = new WarehouseUtilizationManager();

            var list = await manager.WarehouseUtilizationReport(startdate, enddate);

            if (list != null)
            {
                string zipfilename = "_" + DateTime.Now.ToString("yyyyddMHHmmss") + "_WarehouseUtilReportListExport.zip";
                var fileName = "_" + DateTime.Now.ToString("yyyyddMHHmmss") + "_WarehouseUtilReportListExport.csv";
                DataTable dt = ListtoDataTableConverter.ToDataTable(list);

                // rearrange DataTable columns

                dt.Columns["WarehouseName"].SetOrdinal(0);
                dt.Columns["DEmandExcludingRedOrder"].SetOrdinal(1);
                dt.Columns["DemandOrderCount"].SetOrdinal(2);
                dt.Columns["NotProcessedOrderCount"].SetOrdinal(3);
                dt.Columns["ExecutedOrderCount"].SetOrdinal(4);
                dt.Columns["DeliveredOrderCount"].SetOrdinal(5);
                dt.Columns["DeliveryCanceledCount"].SetOrdinal(6);
                dt.Columns["DeliveryRedispatchCount"].SetOrdinal(7);
                dt.Columns["ReattemptCount"].SetOrdinal(8);
                dt.Columns["InTransitOrders"].SetOrdinal(9);

                string path = Path.Combine(HttpContext.Current.Server.MapPath("~/ExcelGeneratePath"), fileName);
                dt.WriteToCsvFile(path);

                string connectionString = ConfigurationManager.ConnectionStrings["AuthContext"].ConnectionString;
                string To = "", From = "", Bcc = "";
                DataTable emaildatatable = new DataTable();
                using (var connection = new SqlConnection(connectionString))
                {
                    using (var command = new SqlCommand("Select * from EmailRecipients where EmailType='WarehouseUtilizationReport'", connection))
                    {
                        command.CommandTimeout = 1200;
                        if (connection.State != ConnectionState.Open)
                            connection.Open();

                        SqlDataAdapter da = new SqlDataAdapter(command);
                        da.Fill(emaildatatable);
                        da.Dispose();
                        connection.Close();
                    }
                }
                if (emaildatatable.Rows.Count > 0)
                {
                    To = !string.IsNullOrEmpty(emaildatatable.Rows[0]["To"].ToString()) ? emaildatatable.Rows[0]["To"].ToString() : To;
                    From = !string.IsNullOrEmpty(emaildatatable.Rows[0]["From"].ToString()) ? emaildatatable.Rows[0]["From"].ToString() : From;
                    Bcc = !string.IsNullOrEmpty(emaildatatable.Rows[0]["Bcc"].ToString()) ? emaildatatable.Rows[0]["Bcc"].ToString() : "";
                }
                string subject = DateTime.Now.AddDays(-1).ToString("dd MMM yyyy") + " WarehouseUtilizationReport ";
                string FileUrl = string.Format("{0}://{1}{2}/{3}", new Uri((HttpContext.Current.Request.UrlReferrer != null ? HttpContext.Current.Request.UrlReferrer.AbsoluteUri : HttpContext.Current.Request.Url.AbsoluteUri)).Scheme
                                                                , HttpContext.Current.Request.Url.DnsSafeHost
                                                                , (HttpContext.Current.Request.Url.Port != 80 && HttpContext.Current.Request.Url.Port != 443 ? ":" + HttpContext.Current.Request.Url.Port : "")
                                                                , "/ExcelGeneratePath/" + fileName);
                string message = "Please find below link for Daily WarehouseUtilizationReport :" + FileUrl;
                if (!string.IsNullOrEmpty(To) && !string.IsNullOrEmpty(From))
                {
                    EmailHelper.SendMail(From, To, Bcc, subject, message, "");
                }
                else
                    logger.Error("WarehouseUtilizationReport To and From empty");


                return true;
            }
            else
            {
                return false;
            }
        }

    }
}
