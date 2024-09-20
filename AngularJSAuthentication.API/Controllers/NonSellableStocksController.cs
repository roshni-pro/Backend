using AngularJSAuthentication.DataContracts;
using AngularJSAuthentication.BusinessLayer.Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using AngularJSAuthentication.Model;
using System.Security.Claims;
using Common.Logging;
using Microsoft.Extensions.Logging;
using NLog;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Data.SqlClient;
using System.Data.Entity.Infrastructure;
using AngularJSAuthentication.BusinessLayer.Managers.Reports;
using AngularJSAuthentication.DataContracts.Shared;

namespace AngularJSAuthentication.API.Controllers
{
    [RoutePrefix("api/NonSellableStock")]
    public class NonSellableStocksController : ApiController
    {
        private static TimeZoneInfo INDIAN_ZONE = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
        DateTime indianTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE);
        public static Logger logger = NLog.LogManager.GetCurrentClassLogger();
     
        [Route("PostWarehouseList")]
        [HttpPost]
        public async Task<HttpResponseMessage> PostWarehouseList(PostWarehouseListdc postWarehouseListdc)
        {
            using (AuthContext context = new AuthContext())
            {
                List<NonSellableStockDc> nonSellableStocks = new List<NonSellableStockDc>();
                List<ItemClassificationDC> objItemClassificationDClist = new List<ItemClassificationDC>();
                var manager = new ItemLedgerManager();
                if (context.Database.Connection.State != ConnectionState.Open)
                    context.Database.Connection.Open();

                //WarehouseIds
                var WarehouseIdDts = new DataTable();
                WarehouseIdDts.Columns.Add("IntValue");

                if (postWarehouseListdc.WarehouseId != null && postWarehouseListdc.WarehouseId.Any())
                {
                    foreach (var item in postWarehouseListdc.WarehouseId)
                    {
                        var dr = WarehouseIdDts.NewRow();
                        dr["IntValue"] = item;
                        WarehouseIdDts.Rows.Add(dr);
                    }
                }
                var Warehouseparam = new SqlParameter("WarehouseId", WarehouseIdDts);
                var Keywordparam = new SqlParameter("Keyword", postWarehouseListdc.Keyword);
                Warehouseparam.SqlDbType = SqlDbType.Structured;
                Warehouseparam.TypeName = "dbo.IntValues";

                var cmd = context.Database.Connection.CreateCommand();
                cmd.CommandTimeout = 900;
                cmd.CommandText = "[dbo].[NonSellableStockWarehouselist]";
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.Parameters.Add(Warehouseparam);
                cmd.Parameters.Add(Keywordparam);

                var reader = cmd.ExecuteReader();

                nonSellableStocks = ((IObjectContextAdapter)context).ObjectContext.Translate<NonSellableStockDc>(reader).ToList();
                List<ItemClassificationDC> _objItemClassificationDClist = await manager.GetItemClassificationsAsync(objItemClassificationDClist);
                foreach (var item in nonSellableStocks)
                {
                    if (_objItemClassificationDClist != null && _objItemClassificationDClist.Any())
                    {

                        if (_objItemClassificationDClist.Any(x => x.ItemNumber == item.ItemNumber))
                        {
                            item.ABCClassification = _objItemClassificationDClist.Where(x => x.ItemNumber == item.ItemNumber).Select(x => x.Category).FirstOrDefault();
                        }
                        else { item.ABCClassification = "N"; }
                    }
                    else
                    {
                        item.ABCClassification = "N";
                    }

                }              
                return Request.CreateResponse(HttpStatusCode.OK, nonSellableStocks);
            }
        }

        [Route("GetNonSellableStockHistoryAll")]//Export History
        [HttpPost]
        public async Task<PaggingData_NonSellableStockHistory> GetNonSellableStockHistory(List<int> WarehouseId)
        {

            using (AuthContext context = new AuthContext())
            {

                PaggingData_NonSellableStockHistory obj = new PaggingData_NonSellableStockHistory();
              
                if (context.Database.Connection.State != ConnectionState.Open)
                   context.Database.Connection.Open();

                    //WarehouseIds
                    var WarehouseIdDts = new DataTable();
                    WarehouseIdDts.Columns.Add("IntValue");

                    if (WarehouseId != null && WarehouseId.Any())
                    {
                        foreach (var item in WarehouseId)
                        {
                            var dr = WarehouseIdDts.NewRow();
                            dr["IntValue"] = item;
                            WarehouseIdDts.Rows.Add(dr);
                        }
                    }
                    var Warehouseparam = new SqlParameter("WarehouseId", WarehouseIdDts);
                    Warehouseparam.SqlDbType = SqlDbType.Structured;
                    Warehouseparam.TypeName = "dbo.IntValues";

                    var cmd = context.Database.Connection.CreateCommand();
                    cmd.CommandTimeout = 900;
                    cmd.CommandText = "[dbo].[NonSellableStockHistory]";
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.Parameters.Add(Warehouseparam);
                   
                    var reader = cmd.ExecuteReader();
                    var listOrders = ((IObjectContextAdapter)context).ObjectContext.Translate<NonSellableStockHistoryAll>(reader).ToList();
                    obj.ordermasterHistory = listOrders;
               
                return obj;
            }
        }

        [Route("getallitem")]
        [HttpGet]
        public async Task<HttpResponseMessage> GetAllItem(string key, int WarehouseId)//search item
        {

               using (AuthContext context = new AuthContext())
               {
                   if (context.Database.Connection.State != ConnectionState.Open)
                    context.Database.Connection.Open();
    
                   var cmd = context.Database.Connection.CreateCommand();
                   cmd.CommandTimeout = 900;
                   cmd.CommandText = "[dbo].[NonSellableStockwiseGetAllItem]";
                   cmd.CommandType = System.Data.CommandType.StoredProcedure;
                   cmd.Parameters.Add(new SqlParameter("@WarehouseId", WarehouseId));
                   cmd.Parameters.Add(new SqlParameter("@key", key));

                   var reader = cmd.ExecuteReader();
                   var NonSallableitemData = ((IObjectContextAdapter)context).ObjectContext.Translate<NonSellableStockWiseGetAllitem>(reader).ToList();                 
                   var manager = new ItemLedgerManager();

                    List<ItemClassificationDC> objItemClassificationDClist = new List<ItemClassificationDC>();
                    foreach (var data in NonSallableitemData)
                    {
                        ItemClassificationDC obj = new ItemClassificationDC();
                        obj.WarehouseId = data.WarehouseId;
                        obj.ItemNumber = data.ItemNumber;
                       objItemClassificationDClist.Add(obj);

                    }
                    List<ItemClassificationDC> _objItemClassificationDClist = await manager.GetItemClassificationsAsync(objItemClassificationDClist);
                    List<NonSellableStock> ObjDmgItemData = new List<NonSellableStock>();
                    foreach (var item in NonSallableitemData)
                    {
                        if (_objItemClassificationDClist != null && _objItemClassificationDClist.Any())
                        {

                            if (_objItemClassificationDClist.Any(x => x.ItemNumber == item.ItemNumber))
                            {
                                item.ABCClassification = _objItemClassificationDClist.Where(x => x.ItemNumber == item.ItemNumber).Select(x => x.Category).FirstOrDefault();
                            }
                            else { item.ABCClassification = "N"; }
                        }
                        else
                        {
                            item.ABCClassification = "N";
                        }

                    }
                    return Request.CreateResponse(HttpStatusCode.OK, NonSallableitemData);
               }            
        }

        #region
        [Route("GetNonSallableHistory")]//History per row
        [HttpGet]
        public async Task<PaggingData_NonSallableStockHistory> GetNonSallableHistory(int skip, int take, string ItemNumber, int WarehouseId, int StockId)
        {
            using (AuthContext context = new AuthContext())
            {
                PaggingData_NonSallableStockHistory obj = new PaggingData_NonSallableStockHistory();
                if (context.Database.Connection.State != ConnectionState.Open)
                    context.Database.Connection.Open();

                var cmd = context.Database.Connection.CreateCommand();
                cmd.CommandTimeout = 900;
                cmd.CommandText = "[dbo].[NonSallableHistoryperRow]";
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.Parameters.Add(new SqlParameter("@WarehouseId", WarehouseId));
                cmd.Parameters.Add(new SqlParameter("@ItemNumber", ItemNumber));
                cmd.Parameters.Add(new SqlParameter("@StockId", StockId));
                cmd.Parameters.Add(new SqlParameter("@Skip", skip));
                cmd.Parameters.Add(new SqlParameter("@Take", take));

                var reader = cmd.ExecuteReader();
                reader.NextResult();
                if (reader.Read())
                {
                    obj.total_count = Convert.ToInt32(reader["TotalCount"]);
                }
                var newdata = ((IObjectContextAdapter)context).ObjectContext.Translate<NonSallableHistoryDc>(reader).ToList();
                var manager = new ItemLedgerManager();
                List<ItemClassificationDC> objItemClassificationDClist = new List<ItemClassificationDC>();
                foreach (var nsstockdata in newdata)
                {
                    ItemClassificationDC objItemClassificationDC = new ItemClassificationDC();
                    objItemClassificationDC.WarehouseId = nsstockdata.WarehouseId;
                    objItemClassificationDC.ItemNumber = nsstockdata.Number;
                    objItemClassificationDClist.Add(objItemClassificationDC);

                }
                List<ItemClassificationDC> _objItemClassificationDClist = await manager.GetItemClassificationsAsync(objItemClassificationDClist);
                foreach (var item in newdata)
                {
                    if (_objItemClassificationDClist != null && _objItemClassificationDClist.Any())
                    {

                        if (_objItemClassificationDClist.Any(x => x.ItemNumber == item.Number))
                        {
                            item.ABCClassification = _objItemClassificationDClist.Where(x => x.ItemNumber == item.Number).Select(x => x.Category).FirstOrDefault();
                        }
                        else { item.ABCClassification = "N"; }
                    }
                    else
                    {
                        item.ABCClassification = "N";
                    }

                }
                obj.nonSellableStockHistory = newdata;

                return obj;


            }
        }
        #endregion
    }

    public class PostWarehouseListdc
    {
        public List<int> WarehouseId { get; set; }
        public string Keyword { get; set; }
    }
    public class NonSellableStockDc
    {
        public long NonSellableStockId { get; set; }
        public int Inventory { get; set; }
        public int WarehouseId { get; set; }
        public string WarehouseName { get; set; }
        public string ItemName { get; set; }
        public string ItemNumber { get; set; }
        public string ABCClassification { get; set; }
        public int ItemMultiMRPId { get; set; }
        public string Comment { get; set; }
        public double UnitPrice { get; set; }
        public DateTime CreatedDate { get; set; }
        
    }
    public class NonSellableStockHistoryAll
    {      
        public long Id { get; set; }
        public long NonSellableStockId { get; set; }
        public int Inventory { get; set; }
        public int WarehouseId { get; set; }
        public string WarehouseName { get; set; }
        public string ItemName { get; set; }
        public int ItemMultiMRPId { get; set; }
        public string Comment { get; set; }
        public bool? IsDeleted { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
        [NotMapped]
        public string UserName { get; set; }
    }

    public class NonSellableStockWiseGetAllitem
    {   
        public long NonSellableStockId { get; set; }
        public int Inventory { get; set; }
        public int WarehouseId { get; set; }
        public string WarehouseName { get; set; }
        public string ItemName { get; set; }
        public int ItemMultiMRPId { get; set; }
        public string Comment { get; set; }
        public bool? IsDeleted { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public string ItemNumber { get; set; }
        public string ABCClassification { get; set; }
    }
   
}
