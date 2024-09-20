using AngularJSAuthentication.Model;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web.Http;

namespace AngularJSAuthentication.API.Controllers.Reporting
{
    [RoutePrefix("api/StockReporting")]
    public class StockReportingController : ApiController
    {
        [Route("GetByName/{name}")]
        [HttpGet]
        public IHttpActionResult GetByName(string name)
        {
            using (var authContext = new AuthContext())
            {
                var itemMasterList = authContext.itemMasters.Where(x => x.itemname.ToLower().Contains(name.ToLower())).Select(y => new
                { Number = y.Number, itemname = (y.itemname + " - " + y.SellingSku) }).Distinct().Take(50).ToList();
                return Ok(itemMasterList);
            }
        }

        [Route("GetReport")]
        [HttpPost]
        public IHttpActionResult GetReport(StockReportingViewModel viewModel)
        {
            DataTable datatable = DailyStockReport(viewModel);
            //UpdateDable(datatable);
            return Ok(datatable);
        }


        private static DataTable DailyStockReport(StockReportingViewModel viewModel)
        {
            string warehouseList = String.Join(",", viewModel.WarehouseList).ToString();
            string itemList = String.Join(",", viewModel.ItemList.Select(x => x.Number)).ToString();
            DataTable table = new DataTable();
            string connectionString = ConfigurationManager.ConnectionStrings["authcontext"].ToString();
            using (var connection = new SqlConnection(connectionString))
            {

                string itemNumber = viewModel.ItemList.FirstOrDefault()?.Number;
                if (!string.IsNullOrEmpty(itemNumber))
                {
                    SqlCommand command = new SqlCommand();
                    command.Connection = connection;
                    command.CommandTimeout = 60 * 30; //30 minutes
                    command.CommandText = "StockTransactionReport";
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@StartDate", viewModel.StartDate.Date));
                    command.Parameters.Add(new SqlParameter("@EndDate", viewModel.EndDate.Date));
                    command.Parameters.Add(new SqlParameter("@WarehouseList", warehouseList));
                    command.Parameters.Add(new SqlParameter("@ItemNumber", itemList));

                    try
                    {
                        if (connection.State != ConnectionState.Open)
                        {
                            connection.Open();
                        }

                        using (var da = new SqlDataAdapter(command))
                        {
                            command.CommandType = CommandType.StoredProcedure;
                            da.Fill(table);
                        }

                        //result = command.ExecuteNonQuery() > 0;
                        connection.Close();
                    }




                    catch (Exception ex)
                    {
                        //LogHelper.LogError(ex.ToString(), true);
                        throw ex;
                    }
                }
            }
            return table;
        }

        private static void UpdateDable(DataTable table)
        {
            if (table != null && table.Columns.Count > 0)
            {

                using (var authContext = new AuthContext())
                {
                    var list = authContext.Warehouses.ToList();

                    foreach (DataColumn column in table.Columns)
                    {
                        if (column.ColumnName != "ItemMultiMRPId" && column.ColumnName != "ItemName")
                        {
                            var item = list.Where(x => x.WarehouseId == int.Parse(column.ColumnName)).FirstOrDefault();
                            column.ColumnName = item.WarehouseName + " - " + item.CityName;

                        }

                    }
                }

            }
        }

    }


    public class StockReportingViewModel
    {
        public List<int> WarehouseList { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public List<ItemMaster> ItemList { get; set; }
    }

}
