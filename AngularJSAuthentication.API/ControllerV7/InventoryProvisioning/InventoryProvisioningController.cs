using AngularJSAuthentication.DataContracts.InventoryProvisioning;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace AngularJSAuthentication.API.ControllerV7.InventoryProvisioning
{
    [RoutePrefix("api/InventoryProvisioning")]
    public class InventoryProvisioningController : ApiController
    {
        [HttpGet]
        [Route("InsertData")]
        [AllowAnonymous]
        public bool InsertData()
        {

            if (DateTime.Today.Day == 1)
            {
                using (var myContext = new AuthContext())
                {
                    DateTime date = DateTime.Today.AddDays(-1);
                    var param = new SqlParameter
                    {
                        ParameterName = "calculationDate",
                        DbType = DbType.DateTime,
                        Value = new DateTime(date.Year, date.Month, 1)
                    };

                    int Result = myContext.Database.ExecuteSqlCommand("InventoryProvisioningDataInsert @calculationDate", param);
                    myContext.Commit();
                }
            }
            return false;
        }

        [HttpPost]
        [Route("GetProvisioningData")]
        [AllowAnonymous]
        public DataTable GetProvisioningData(InventoryProvisioningInputDc input)
        {
            if (input.CalculationDate == null)
            {
                input.CalculationDate = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            }

            using (var myContext = new AuthContext())
            {
                string connectionString = ConfigurationManager.ConnectionStrings["AuthContext"].ConnectionString;
                DataTable DT = new DataTable();
                using (var connection = new SqlConnection(connectionString))
                {
                    using (var command = new SqlCommand("EXEC GetInventoryProvisioningData @calcultionDate,@warehouseId,@brandIdList,@Keyword", connection))
                    {

                        command.Parameters.AddWithValue("@calcultionDate", input.CalculationDate);
                        command.Parameters.AddWithValue("@warehouseId", input.WarehouseId);

                        DataTable dt = new DataTable();
                        dt.Columns.Add("IntValue");
                        if (input.BrandIdList == null)
                        {
                            input.BrandIdList = new List<int>();
                        }
                        foreach (var item in input.BrandIdList)
                        {
                            var dr = dt.NewRow();
                            dr["IntValue"] = item;
                            dt.Rows.Add(dr);
                        }

                        SqlParameter tvpParam = command.Parameters.AddWithValue("@brandIdList", dt);
                        tvpParam.SqlDbType = SqlDbType.Structured;
                        tvpParam.TypeName = "dbo.IntValues";

                        command.Parameters.AddWithValue("@Keyword", input.Keyword);


                        command.CommandTimeout = 300;
                        if (connection.State != ConnectionState.Open)
                            connection.Open();

                        SqlDataAdapter da = new SqlDataAdapter(command);

                        da.Fill(DT);
                        da.Dispose();
                        connection.Close();

                        return DT;
                    }
                }
            }
        }

        [HttpPost]
        [Route("InventoryProvisioningGraphAllData")]
        [AllowAnonymous]
        public InventoryProvisioningGraphAllData GetProvisioningGraphData(InventoryProvisioningGraphInputDc input)
        {
            List<DateTime> monthList = new List<DateTime>();
            DateTime startDate = input.FromDate;
            while (startDate <= input.ToDate)
            {
                monthList.Add(startDate);
                startDate = startDate.AddMonths(1);
            }

            InventoryProvisioningGraphAllData data = new InventoryProvisioningGraphAllData();

            data.labels = new List<string>();
            foreach (var item in monthList)
            {
                data.labels.Add(item.ToString("MMMM"));
            }

            List<InventoryProvisioningGraphDataDc> Result = null;
            using (var context = new AuthContext())
            {
                var fromDateParam = new SqlParameter
                {
                    ParameterName = "FromDate",
                    DbType = DbType.DateTime,
                    Value = input.FromDate
                };

                var toDateParam = new SqlParameter
                {
                    ParameterName = "ToDate",
                    DbType = DbType.DateTime,
                    Value = input.ToDate
                };


                var warehouseIdDts = new DataTable();
                warehouseIdDts.Columns.Add("IntValue");
                if (input.WarehouseIdList != null && input.WarehouseIdList.Any())
                {
                    foreach (var item in input.WarehouseIdList)
                    {
                        var dr = warehouseIdDts.NewRow();
                        dr["IntValue"] = item;
                        warehouseIdDts.Rows.Add(dr);
                    }
                }
                var warehouseIdparam = new SqlParameter("warehouseIdList", warehouseIdDts);
                warehouseIdparam.SqlDbType = SqlDbType.Structured;
                warehouseIdparam.TypeName = "dbo.IntValues";


                var brandIdDts = new DataTable();
                brandIdDts.Columns.Add("IntValue");
                if (input.BrandIdList != null && input.BrandIdList.Any())
                {
                    foreach (var item in input.BrandIdList)
                    {
                        var dr = brandIdDts.NewRow();
                        dr["IntValue"] = item;
                        brandIdDts.Rows.Add(dr);
                    }
                }
                var brandIdparam = new SqlParameter("brandIdList", brandIdDts);
                brandIdparam.SqlDbType = SqlDbType.Structured;
                brandIdparam.TypeName = "dbo.IntValues";
                Result = context.Database.SqlQuery<InventoryProvisioningGraphDataDc>("InventoryProvisioningGraphDataGet @FromDate, @ToDate, @warehouseIdList, @brandIdList", fromDateParam, toDateParam, warehouseIdparam, brandIdparam).ToList();
            }

            if (Result != null && Result.Any())
            {
                var group = Result.GroupBy(x => x.WarehouseName);
                data.datasets = new List<InventoryProvisioningGraphAllDataSet>();
                foreach (var grp in group)
                {
                    InventoryProvisioningGraphAllDataSet dataSet = new InventoryProvisioningGraphAllDataSet();
                    dataSet.label = grp.Key;
                    dataSet.data = new List<double>();
                    foreach (var item in monthList)
                    {
                        var tempData = grp.FirstOrDefault(x => x.CalculationDate == item);
                        if (tempData == null)
                        {
                            dataSet.data.Add(0);
                        }
                        else
                        {
                            dataSet.data.Add(tempData.ProvisioningAmount);
                        }
                    }
                    data.datasets.Add(dataSet);
                }
            }
            return data;
        }


        [HttpGet]
        [Route("InQueueInDateCorrection")]
        [AllowAnonymous]
        public int InQueueInDateCorrection()
        {

            using (var context = new AuthContext())
            {
                var query = @"select distinct w.WarehouseId from Warehouses w with(nolock)     
                  inner join GMWarehouseProgresses b with(nolock) on w.WarehouseId = b.WarehouseID and b.IsLaunched=1
                  and w.active=1 and w.Deleted=0 and w.IsKPP=0 and w.CityName not like '%test%'";

                var AllactiveWarehouselist = context.Database.SqlQuery<int>(query).ToList();

                ParallelLoopResult result = Parallel.ForEach(AllactiveWarehouselist, new ParallelOptions { MaxDegreeOfParallelism = 2 }, (wid) =>
                {
                    //    foreach (var wid in AllactiveWarehouselist)
                    //{
                    //var query1 = $"select  ItemMultiMRPId from ItemMasters where  WarehouseId={wid} and active=1 and Deleted=0";
                    //var multimrpIdList = context.Database.SqlQuery<int>(query1).ToList();

                    //ParallelLoopResult result = Parallel.ForEach(multimrpIdList, new ParallelOptions { MaxDegreeOfParallelism = 10 }, (multimrpId) =>
                    //{
                    //    var Result = context.Database.SqlQuery<dynamic>("InQueueInDateCorrection @startdate, @enddate, @itemMultiMRPId, @warehouseId", fromDateParam, toDateParam, multimrpId, wid).ToList();

                    //});
                    //if (result.IsCompleted) { }
                    //foreach (var multimrpId in multimrpIdList)
                    //{
                    var fromDateParam = new SqlParameter
                    {
                        ParameterName = "startdate",
                        DbType = DbType.DateTime,
                        Value = new DateTime(DateTime.Now.Date.AddDays(-1).Year, DateTime.Now.Date.AddDays(-1).Month, 1)
                    };

                    var toDateParam = new SqlParameter
                    {
                        ParameterName = "enddate",
                        DbType = DbType.DateTime,
                        Value = DateTime.Now.Date.AddSeconds(-1)
                    };

                    var widParam = new SqlParameter
                    {
                        ParameterName = "warehouseId",
                        DbType = DbType.Int32,
                        Value = wid
                    };


                    context.Database.CommandTimeout = 180;
                    var Result = context.Database.SqlQuery<long>("exec InQueueInDateCorrection @startdate, @enddate, @warehouseId", fromDateParam, toDateParam, widParam).ToList();
                    //}
                    //}
                });
                if (result.IsCompleted)
                {

                }
            }
            return 0;
        }
        /// <summary>
        /// by Sudhir test
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("ReGenerateInQueueInDateCorrection")]
        [AllowAnonymous]
        public int ReGenerateInQueueInDateCorrection(int month, int year)
        {

            using (var context = new AuthContext())
            {
                var query = @"select distinct w.WarehouseId from Warehouses w with(nolock)     
                  inner join GMWarehouseProgresses b with(nolock) on w.WarehouseId = b.WarehouseID and b.IsLaunched=1
                  and w.active=1 and w.Deleted=0 and w.IsKPP=0 and w.CityName not like '%test%'";

                var AllactiveWarehouselist = context.Database.SqlQuery<int>(query).ToList();

                ParallelLoopResult result = Parallel.ForEach(AllactiveWarehouselist, new ParallelOptions { MaxDegreeOfParallelism = 2 }, (wid) =>
                {
                    //    foreach (var wid in AllactiveWarehouselist)
                    //{
                    //var query1 = $"select  ItemMultiMRPId from ItemMasters where  WarehouseId={wid} and active=1 and Deleted=0";
                    //var multimrpIdList = context.Database.SqlQuery<int>(query1).ToList();

                    //ParallelLoopResult result = Parallel.ForEach(multimrpIdList, new ParallelOptions { MaxDegreeOfParallelism = 10 }, (multimrpId) =>
                    //{
                    //    var Result = context.Database.SqlQuery<dynamic>("InQueueInDateCorrection @startdate, @enddate, @itemMultiMRPId, @warehouseId", fromDateParam, toDateParam, multimrpId, wid).ToList();

                    //});
                    //if (result.IsCompleted) { }
                    //foreach (var multimrpId in multimrpIdList)
                    //{
                    var fromDateParam = new SqlParameter
                    {
                        ParameterName = "startdate",
                        DbType = DbType.DateTime,
                        Value = new DateTime(year, month, 1)
                    };

                    var toDateParam = new SqlParameter
                    {
                        ParameterName = "enddate",
                        DbType = DbType.DateTime,
                        Value = new DateTime(year, month + 1, 1)
                    };

                    var widParam = new SqlParameter
                    {
                        ParameterName = "warehouseId",
                        DbType = DbType.Int32,
                        Value = wid
                    };


                    context.Database.CommandTimeout = 180;
                    var Result = context.Database.SqlQuery<long>("exec InQueueInDateCorrection @startdate, @enddate, @warehouseId", fromDateParam, toDateParam, widParam).ToList();
                    //}
                    //}
                });
                if (result.IsCompleted)
                {

                }
            }
            return 0;
        }
    }
}
