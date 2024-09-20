using AngularJSAuthentication.API.Helper;
using AngularJSAuthentication.Common.Helpers;
using AngularJSAuthentication.Controllers;
using AngularJSAuthentication.DataContracts.ROC;
using Microsoft.Extensions.Logging;
using NLog;
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

namespace AngularJSAuthentication.API.ControllerV7.ROC
{
    [RoutePrefix("api/ROC")]
    public class RocController : BaseApiController
    {
        private static Logger logger = NLog.LogManager.GetCurrentClassLogger();
        private static TimeZoneInfo INDIAN_ZONE = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
        DateTime indianTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE);

        [AllowAnonymous]
        [HttpGet]
        [Route("InsertROCItemRawData")]
        public async Task<RocMsgDc> InsertROCItemRawData()
        {
            DateTime now = DateTime.Today;
            now = now.AddDays(-1);
            var StartDate = new DateTime(now.Year, now.Month, 1);
            var EndDate = StartDate.AddMonths(1);
            RocMsgDc res;

            if (DateTime.Today.Day == 1)
            {
                string spName = "ROCItemRawDataInsert @startDate,@endDate";
                using (var context = new AuthContext())
                {
                    var Param = new SqlParameter
                    {
                        ParameterName = "startDate",
                        Value = StartDate
                    };
                    var Param1 = new SqlParameter
                    {
                        ParameterName = "endDate",
                        Value = EndDate
                    };
                    context.Database.CommandTimeout = 6000;
                    var list = context.Database.ExecuteSqlCommand(spName, Param, Param1);

                    res = new RocMsgDc()
                    {
                        Status = true,
                        Message = "Insert Successfully!!",
                    };
                    return res;
                }
            }
            else
            {
                return  new RocMsgDc()
                {
                    Status = true,
                    Message = "no need to run the job!!",
                };
            }
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("InsertItemTaggingData")]
        public async Task<RocMsgDc> InsertItemTaggingData()
        {
            RocMsgDc res;
            DateTime now = DateTime.Today;
            now = now.AddDays(-1);
            var ForMonth = new DateTime(now.Year, now.Month, 1);

            if (DateTime.Today.Day == 1)
            {
                string spName = "InsertItemTaggingData @ForMonth";
                using (var context = new AuthContext())
                {
                    var Param = new SqlParameter
                    {
                        ParameterName = "ForMonth",
                        Value = ForMonth
                    };

                    var list = context.Database.SqlQuery<ItemTaggingInsertDc>(spName, Param).ToList();

                    res = new RocMsgDc()
                    {
                        Status = true,
                        Message = "Insert Successfully!!",
                    };
                    return res;
                }
            }
            else
            {
                return new RocMsgDc()
                {
                    Status = true,
                    Message = "No need!!",
                };
            }
        }

        [Route("RocTagValueGet")]
        [HttpPost]
        public async Task<List<ItemWarehouseData>> RocTagValueGet(List<ItemWarehouseDc> itemWarehouseDcs)
        {
            TripPlannerHelper tripPlannerHelper = new TripPlannerHelper();
            var list = await tripPlannerHelper.RocTagValueGet(itemWarehouseDcs);
            return list;
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("ExportRocReportData")]
        public async Task<string> ExportRocReportData(DateTime ForMonthData)
        {
            string fileUrl = string.Empty;
            TripPlannerHelper tripPlannerHelper = new TripPlannerHelper();

            var list = await tripPlannerHelper.ExportRocReportData(ForMonthData);
            string zipfilename = "_" + DateTime.Now.ToString("yyyyddMHHmmss") + "_RocReportDataExport.zip";
            var fileName = "_" + DateTime.Now.ToString("yyyyddMHHmmss") + "_RocReportDataExport.csv";
            DataTable dt = ListtoDataTableConverter.ToDataTable(list);         

            string path = Path.Combine(HttpContext.Current.Server.MapPath("~/ExcelGeneratePath"), fileName);
            dt.WriteToCsvFile(path);

            return $"/ExcelGeneratePath/{fileName}";
        }
    }
}

