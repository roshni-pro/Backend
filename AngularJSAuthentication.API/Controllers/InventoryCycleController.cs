using AngularJSAuthentication.API.Helper;
using AngularJSAuthentication.API.Helpers;
using AngularJSAuthentication.Common.Helpers;
using AngularJSAuthentication.DataContracts.constants;
using AngularJSAuthentication.DataContracts.Masters.Batch;
using AngularJSAuthentication.DataContracts.Transaction.InventoryCycle;
using AngularJSAuthentication.DataContracts.Transaction.Ledger.ItemLedger;
using AngularJSAuthentication.DataContracts.Transaction.Stocks;
using AngularJSAuthentication.Model.Stocks;
using AngularJSAuthentication.Model;
using LinqKit;
using MongoDB.Bson.Serialization.Attributes;
using NLog;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Transactions;
using System.Web;
using System.Web.Http;
using System.Web.Http.Description;
using AngularJSAuthentication.Model.Stocks.Batch;
using AngularJSAuthentication.API.Managers;
using AngularJSAuthentication.BatchManager.Publishers;
using AngularJSAuthentication.DataContracts.BatchCode;
using AngularJSAuthentication.DataContracts.Mongo;
using AngularJSAuthentication.DataContracts.External.SalesAppDc;
using SqlBulkTools;

namespace AngularJSAuthentication.API.Controllers
{
    [RoutePrefix("api/InventoryCycle")]
    public class InventoryCycleController : ApiController
    {
        private static TimeZoneInfo INDIAN_ZONE = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
        string connectionString = ConfigurationManager.ConnectionStrings["AuthContext"].ConnectionString;
        DateTime indianTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE);
        private static Logger logger = LogManager.GetCurrentClassLogger();

        [Route("GetInvent")]
        [HttpGet]
        public HttpResponseMessage GetInvent(int Warehouseid, DateTime? Date)
        {
            using (var context = new AuthContext())
            {
                var pastdate = DateTime.Now.Date.AddDays(-1).ToString("MM/dd/yyyy");
                var query = "exec [GetInventoryCyclesForCounting] " + Warehouseid
                            + (!Date.HasValue ? "" : ", '" + Date.Value.Date.ToString(@"yyyy-MM-dd") + "'")
                            ;
                var list = context.Database.SqlQuery<GetInventCycledata>(query).OrderBy(x => x.WarehouseId).ToList();
                return Request.CreateResponse(HttpStatusCode.OK, list);
            }
        }


        [Route("GetInventWithPaging")]
        [HttpGet]
        public PaggingDTO GetInventWithPaging(int Warehouseid, int skip, int take, DateTime? Date)
        {
            int Skiplist = (skip - 1) * take;
            PaggingDTO result = new PaggingDTO { total_count = 0, GetInventCycledatadto = new List<GetInventCycledata>() };
            using (var context = new AuthContext())
            {
                var pastdate = Date.HasValue ? Date : DateTime.Now.Date.AddDays(-1);

                if (context.Database.Connection.State != ConnectionState.Open)
                    context.Database.Connection.Open();


                var cmd = context.Database.Connection.CreateCommand();
                cmd.CommandText = "[dbo].[GetInventoryCyclesForCountingWithPaging]";
                cmd.Parameters.Add(new SqlParameter("@warehouseid", Warehouseid));
                cmd.Parameters.Add(new SqlParameter("@createddate", pastdate));
                cmd.Parameters.Add(new SqlParameter("@skip", Skiplist));
                cmd.Parameters.Add(new SqlParameter("@take", take));
                cmd.CommandType = System.Data.CommandType.StoredProcedure;

                // Run the sproc
                var reader = cmd.ExecuteReader();
                result.GetInventCycledatadto = ((IObjectContextAdapter)context)
                .ObjectContext
                .Translate<GetInventCycledata>(reader).ToList();
                reader.NextResult();
                if (reader.Read())
                {
                    result.total_count = Convert.ToInt32(reader["itemcount"]);
                }

                if (result.GetInventCycledatadto != null && result.GetInventCycledatadto.Any())
                {
                    var ids = result.GetInventCycledatadto.Select(x => x.Id).ToList();
                    var orderIdDt = new DataTable();
                    orderIdDt.Columns.Add("IntValue");
                    foreach (var item in ids)
                    {
                        var dr = orderIdDt.NewRow();
                        dr["IntValue"] = item;
                        orderIdDt.Rows.Add(dr);
                    }

                    var param = new SqlParameter("ids", orderIdDt);
                    param.SqlDbType = SqlDbType.Structured;
                    param.TypeName = "dbo.IntValues";
                    cmd = context.Database.Connection.CreateCommand();
                    cmd.CommandText = "[dbo].[GetInventoryCyclesBatchCode]";
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.Parameters.Add(param);

                    // Run the sproc
                    var reader1 = cmd.ExecuteReader();
                    var InventoryCycleItemBatchDcs = ((IObjectContextAdapter)context)
                     .ObjectContext
                     .Translate<InventoryCycleItemBatchDc>(reader1).ToList();
                    if (InventoryCycleItemBatchDcs != null && InventoryCycleItemBatchDcs.Any())
                    {
                        foreach (var item in result.GetInventCycledatadto)
                        {
                            item.InventoryCycleItemBatchDcs = InventoryCycleItemBatchDcs.Where(x => x.InventCycle_Id == item.Id).ToList();

                            foreach (var batch in item.InventoryCycleItemBatchDcs.Where(x => !string.IsNullOrEmpty(x.DamagedImageUrl) || !string.IsNullOrEmpty(x.NonSellableImageUrl)))
                            {
                                if (!string.IsNullOrEmpty(batch.NonSellableImageUrl))
                                {
                                    batch.NonSellableImageUrl = string.Format("{0}://{1}:{2}/{3}", new Uri(HttpContext.Current.Request.Url.AbsoluteUri).Scheme
                                                                    , HttpContext.Current.Request.Url.DnsSafeHost
                                                                    , (HttpContext.Current.Request.Url.Port != 80 && HttpContext.Current.Request.Url.Port != 443 ? ":" + HttpContext.Current.Request.Url.Port : "")
                                                                    , batch.NonSellableImageUrl);
                                }
                                if (!string.IsNullOrEmpty(batch.DamagedImageUrl))
                                {
                                    batch.DamagedImageUrl = string.Format("{0}://{1}:{2}/{3}", new Uri(HttpContext.Current.Request.Url.AbsoluteUri).Scheme
                                                                    , HttpContext.Current.Request.Url.DnsSafeHost
                                                                    , (HttpContext.Current.Request.Url.Port != 80 && HttpContext.Current.Request.Url.Port != 443 ? ":" + HttpContext.Current.Request.Url.Port : "")
                                                                    , batch.DamagedImageUrl);
                                }

                            }

                        }
                    }
                }

                return result;
            }
        }


        [Route("GetInventoryCount")]
        [HttpGet]
        public HttpResponseMessage GetInventoryCount()
        {

            using (var context = new AuthContext())
            {
                var pastdate = DateTime.Now.Date.AddDays(-1).ToString("MM/dd/yyyy");
                var query = "select ic.Id,cs.ItemMultiMRPId as ItemMultiMRPId,cs.ItemName,cs.MRP,ic.RtdCount as RtdCount,ic.RtpCount,ic.DamagedQty,Ic.NonSellableQty,w.WarehouseId,w.WarehouseName as WarehouseName,ic.InventoryCount as InventoryCount,ic.Comment as Comment,cs.CurrentInventory, " +
                    "  ic.CreatedDate,ic.PastInventory from InventCycles ic inner join Warehouses w on  w.WarehouseId =ic.WarehouseId " +
                    "  inner join CurrentStocks cs  on  ic.ItemMultiMRPId=cs.ItemMultiMRPId and cs.WarehouseId = ic.WarehouseId where   Convert(date,ic.CreatedDate,120)='" + DateTime.Now.Date.ToString("MM/dd/yyyy") + "'";

                var list = context.Database.SqlQuery<GetInventCycledata>(query).ToList();

                return Request.CreateResponse(HttpStatusCode.OK, list);
            }
        }

        [Route("GetInventoryCount/V1/{warehouseId}")]
        [HttpGet]
        public HttpResponseMessage GetInventoryCountV1(int warehouseId)
        {
            using (var context = new AuthContext())
            {
                var pastdate = DateTime.Now.Date.AddDays(-1).ToString("MM/dd/yyyy");

                var list = context.Database.SqlQuery<GetInventCycledata>("exec [GetInventoryCyclesForCounting] " + warehouseId).OrderBy(x => x.WarehouseId).ToList();
                return Request.CreateResponse(HttpStatusCode.OK, list);
            }
        }

        [Route("GetInventoryApproval")]
        [HttpGet]
        public async Task<PaggingDTO> GetInventoryApproval(int WarehouseId, int skip, int take, bool IsHQ, string keyword, int status, bool isZeroDifference, DateTime? start, DateTime? end, bool Type)//int status
        {
            PaggingDTO obj = new PaggingDTO();
            var identity = User.Identity as ClaimsIdentity;
            int userid = 0;
            int Warehouse_id = 0;
            string RoleName = null;
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "Warehouseid"))
                Warehouse_id = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "Warehouseid").Value);

            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "RoleNames"))
                RoleName = identity.Claims.FirstOrDefault(x => x.Type == "RoleNames").Value.ToString();
            using (var context = new AuthContext())
            {
                List<GetInventCycledata> list = new List<GetInventCycledata>();

                if (WarehouseId > 0)
                {
                    Warehouse_id = WarehouseId;
                }
                if (Warehouse_id > 0)
                {
                    if (keyword == "undefined" || keyword == null)
                    {
                        keyword = "";
                    }

                    if (context.Database.Connection.State != ConnectionState.Open)
                        context.Database.Connection.Open();
                    context.Database.CommandTimeout = 120;
                    var stmt = context.Database.Connection.CreateCommand();
                    stmt.CommandText = "[dbo].[GetInventoryCycleToApprove]";
                    stmt.CommandType = System.Data.CommandType.StoredProcedure;
                    stmt.Parameters.Add(new SqlParameter("WarehouseId", SqlDbType.Int) { Value = Warehouse_id });
                    stmt.Parameters.Add(new SqlParameter("IsHQ", SqlDbType.Bit) { Value = IsHQ });
                    stmt.Parameters.Add(new SqlParameter("skip", SqlDbType.Int) { Value = skip });
                    stmt.Parameters.Add(new SqlParameter("take", SqlDbType.Int) { Value = take });
                    stmt.Parameters.Add(new SqlParameter("keyword", SqlDbType.VarChar) { Value = keyword });
                    stmt.Parameters.Add(new SqlParameter("status", SqlDbType.Int) { Value = status });
                    stmt.Parameters.Add(new SqlParameter("isZeroDifference", SqlDbType.Bit) { Value = isZeroDifference });
                    stmt.Parameters.Add(new SqlParameter("start", start ?? (object)DBNull.Value));
                    stmt.Parameters.Add(new SqlParameter("end", end ?? (object)DBNull.Value));
                    stmt.Parameters.Add(new SqlParameter("Type", SqlDbType.Bit) { Value = Type });

                    var reader7 = stmt.ExecuteReader();
                    list = ((IObjectContextAdapter)context)
                    .ObjectContext
                    .Translate<GetInventCycledata>(reader7).ToList();


                    //if (context.Database.Connection.State != ConnectionState.Open)
                    //    context.Database.Connection.Open();

                    //context.Database.CommandTimeout = 120;
                    var cnt = context.Database.Connection.CreateCommand();
                    cnt.CommandText = "[dbo].[GetInventoryCycleToApproveCount]";
                    cnt.CommandType = System.Data.CommandType.StoredProcedure;

                    cnt.Parameters.Add(new SqlParameter("WarehouseId", SqlDbType.Int) { Value = Warehouse_id });
                    cnt.Parameters.Add(new SqlParameter("IsHQ", SqlDbType.Bit) { Value = IsHQ });
                    cnt.Parameters.Add(new SqlParameter("keyword", SqlDbType.VarChar) { Value = keyword });
                    cnt.Parameters.Add(new SqlParameter("status", SqlDbType.Int) { Value = status });
                    cnt.Parameters.Add(new SqlParameter("isZeroDifference", SqlDbType.Bit) { Value = isZeroDifference });
                    cnt.Parameters.Add(new SqlParameter("start", start ?? (object)DBNull.Value));
                    cnt.Parameters.Add(new SqlParameter("end", end ?? (object)DBNull.Value));
                    cnt.Parameters.Add(new SqlParameter("Type", SqlDbType.Bit) { Value = Type });

                    var reader8 = cnt.ExecuteReader();
                    int count = ((IObjectContextAdapter)context)
                    .ObjectContext
                    .Translate<int>(reader8).FirstOrDefault();


                    if (list != null && list.Any())
                    {
                        //if (context.Database.Connection.State != ConnectionState.Open)
                        //    context.Database.Connection.Open();

                        //context.Database.CommandTimeout = 120;
                        var ids = list.Select(x => x.Id).ToList();
                        var orderIdDt = new DataTable();
                        orderIdDt.Columns.Add("IntValue");
                        foreach (var item in ids)
                        {
                            var dr = orderIdDt.NewRow();
                            dr["IntValue"] = item;
                            orderIdDt.Rows.Add(dr);
                        }

                        var param = new SqlParameter("ids", orderIdDt);
                        param.SqlDbType = SqlDbType.Structured;
                        param.TypeName = "dbo.IntValues";
                        var cmd = context.Database.Connection.CreateCommand();
                        cmd.CommandText = "[dbo].[GetInventoryCyclesBatchCode]";
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add(param);

                        // Run the sproc
                        var reader1 = cmd.ExecuteReader();
                        var InventoryCycleItemBatchDcs = ((IObjectContextAdapter)context)
                         .ObjectContext
                         .Translate<InventoryCycleItemBatchDc>(reader1).ToList();
                        if (InventoryCycleItemBatchDcs != null && InventoryCycleItemBatchDcs.Any())
                        {
                            var multiMrpIds = InventoryCycleItemBatchDcs.GroupBy(x => x.ItemMultiMrpId).Select(x => new { ItemMultiMrpId = x.Key }).ToList();

                            DateTime startDate = DateTime.Now.Date.AddDays(-10);
                            DateTime endDate = DateTime.Now;
                            var MrpIdDt = new DataTable();
                            MrpIdDt.Columns.Add("IntValue");
                            foreach (var item in multiMrpIds)
                            {
                                var dr = MrpIdDt.NewRow();
                                dr["IntValue"] = item.ItemMultiMrpId;
                                MrpIdDt.Rows.Add(dr);
                            }

                            var mrpparam = new SqlParameter("itemmultimrpIds", MrpIdDt);
                            mrpparam.SqlDbType = SqlDbType.Structured;
                            mrpparam.TypeName = "dbo.IntValues";
                            cmd = context.Database.Connection.CreateCommand();
                            cmd.CommandText = "[FIFO].[GetItemAPPsByMrpIds]";
                            cmd.CommandType = System.Data.CommandType.StoredProcedure;
                            cmd.Parameters.Add(new SqlParameter("@warehouseId", WarehouseId));
                            cmd.Parameters.Add(mrpparam);
                            cmd.Parameters.Add(new SqlParameter("@startDate", startDate));
                            cmd.Parameters.Add(new SqlParameter("@enddate", endDate));

                            // Run the sproc
                            var reader = cmd.ExecuteReader();
                            reader.NextResult();
                            reader.NextResult();
                            var LastMonthApps = ((IObjectContextAdapter)context)
                             .ObjectContext
                             .Translate<LastMonthApp>(reader).ToList();

                            foreach (var item in list)
                            {
                                item.InventoryCycleItemBatchDcs = InventoryCycleItemBatchDcs.Where(x => x.InventCycle_Id == item.Id).ToList();
                                if (LastMonthApps.Any(x => x.itemmultimrpid == item.ItemMultiMRPId))
                                {
                                    item.APP = LastMonthApps.FirstOrDefault(x => x.itemmultimrpid == item.ItemMultiMRPId).APP;
                                }
                                foreach (var batch in
                                    item.InventoryCycleItemBatchDcs.Where(x => !string.IsNullOrEmpty(x.NonSellableImageUrl) || !string.IsNullOrEmpty(x.NonSellableImageUrl)))
                                {

                                    if (!string.IsNullOrEmpty(batch.NonSellableImageUrl))
                                    {
                                        batch.NonSellableImageUrl = string.Format("{0}://{1}:{2}/{3}", new Uri(HttpContext.Current.Request.Url.AbsoluteUri).Scheme
                                                                    , HttpContext.Current.Request.Url.DnsSafeHost
                                                                    , (HttpContext.Current.Request.Url.Port != 80 && HttpContext.Current.Request.Url.Port != 443 ? ":" + HttpContext.Current.Request.Url.Port : "")
                                                                    , batch.NonSellableImageUrl);
                                    }
                                    if (!string.IsNullOrEmpty(batch.DamagedImageUrl))
                                    {
                                        batch.DamagedImageUrl = string.Format("{0}://{1}:{2}/{3}", new Uri(HttpContext.Current.Request.Url.AbsoluteUri).Scheme
                                                                    , HttpContext.Current.Request.Url.DnsSafeHost
                                                                    , (HttpContext.Current.Request.Url.Port != 80 && HttpContext.Current.Request.Url.Port != 443 ? ":" + HttpContext.Current.Request.Url.Port : "")
                                                                    , batch.DamagedImageUrl);
                                    }

                                }
                            }
                        }
                    }

                    obj.GetInventCycledatadto = list;
                    obj.total_count = count;

                }
                return obj;


            }
        }

        [Route("GetInventoryZeroDifference")]
        [HttpGet]
        public async Task<PaggingZeroDifferenceDTO> GetInventoryZeroDifference(int WarehouseId, int skip, int take, bool IsHQ, string keyword, int status, bool isZeroDifference, DateTime? start, DateTime? end, bool Type)//int status
        {
            PaggingZeroDifferenceDTO obj = new PaggingZeroDifferenceDTO();
            var identity = User.Identity as ClaimsIdentity;
            int userid = 0;
            int Warehouse_id = 0;
            string RoleName = null;
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "Warehouseid"))
                Warehouse_id = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "Warehouseid").Value);

            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "RoleNames"))
                RoleName = identity.Claims.FirstOrDefault(x => x.Type == "RoleNames").Value.ToString();
            using (var context = new AuthContext())
            {
                List<GetInventCycleZeroDifferencedata> list = new List<GetInventCycleZeroDifferencedata>();


                if (WarehouseId > 0)
                {
                    Warehouse_id = WarehouseId;
                }
                if (Warehouse_id > 0)
                {
                    if (keyword == "undefined" || keyword == null)
                    {
                        keyword = "";
                    }

                    if (context.Database.Connection.State != ConnectionState.Open)
                        context.Database.Connection.Open();
                    context.Database.CommandTimeout = 120;
                    var stmt = context.Database.Connection.CreateCommand();
                    stmt.CommandText = "[dbo].[GetInventoryCycleToApprove]";
                    stmt.CommandType = System.Data.CommandType.StoredProcedure;
                    stmt.Parameters.Add(new SqlParameter("WarehouseId", SqlDbType.Int) { Value = Warehouse_id });
                    stmt.Parameters.Add(new SqlParameter("IsHQ", SqlDbType.Bit) { Value = IsHQ });
                    stmt.Parameters.Add(new SqlParameter("skip", SqlDbType.Int) { Value = skip });
                    stmt.Parameters.Add(new SqlParameter("take", SqlDbType.Int) { Value = take });
                    stmt.Parameters.Add(new SqlParameter("keyword", SqlDbType.VarChar) { Value = keyword });
                    stmt.Parameters.Add(new SqlParameter("status", SqlDbType.Int) { Value = status });
                    stmt.Parameters.Add(new SqlParameter("isZeroDifference", SqlDbType.Bit) { Value = isZeroDifference });
                    stmt.Parameters.Add(new SqlParameter("start", start ?? (object)DBNull.Value));
                    stmt.Parameters.Add(new SqlParameter("end", end ?? (object)DBNull.Value));
                    stmt.Parameters.Add(new SqlParameter("Type", SqlDbType.Bit) { Value = Type });

                    var reader7 = stmt.ExecuteReader();
                    list = ((IObjectContextAdapter)context)
                    .ObjectContext
                    .Translate<GetInventCycleZeroDifferencedata>(reader7).ToList();

                    //if (context.Database.Connection.State != ConnectionState.Open)
                    //    context.Database.Connection.Open();

                    //context.Database.CommandTimeout = 120;
                    var cnt = context.Database.Connection.CreateCommand();
                    cnt.CommandText = "[dbo].[GetInventoryCycleToApproveCount]";
                    cnt.CommandType = System.Data.CommandType.StoredProcedure;

                    cnt.Parameters.Add(new SqlParameter("WarehouseId", SqlDbType.Int) { Value = Warehouse_id });
                    cnt.Parameters.Add(new SqlParameter("IsHQ", SqlDbType.Bit) { Value = IsHQ });
                    cnt.Parameters.Add(new SqlParameter("keyword", SqlDbType.VarChar) { Value = keyword });
                    cnt.Parameters.Add(new SqlParameter("status", SqlDbType.Int) { Value = status });
                    cnt.Parameters.Add(new SqlParameter("isZeroDifference", SqlDbType.Bit) { Value = isZeroDifference });
                    cnt.Parameters.Add(new SqlParameter("start", start ?? (object)DBNull.Value));
                    cnt.Parameters.Add(new SqlParameter("end", end ?? (object)DBNull.Value));
                    cnt.Parameters.Add(new SqlParameter("Type", SqlDbType.Bit) { Value = Type });

                    var reader8 = cnt.ExecuteReader();
                    int count = ((IObjectContextAdapter)context)
                    .ObjectContext
                    .Translate<int>(reader8).FirstOrDefault();


                    if (list != null && list.Any())
                    {
                        //if (context.Database.Connection.State != ConnectionState.Open)
                        //    context.Database.Connection.Open();

                        //context.Database.CommandTimeout = 120;
                        var ids = list.Select(x => x.Id).ToList();
                        var orderIdDt = new DataTable();
                        orderIdDt.Columns.Add("IntValue");
                        foreach (var item in ids)
                        {
                            var dr = orderIdDt.NewRow();
                            dr["IntValue"] = item;
                            orderIdDt.Rows.Add(dr);
                        }

                        var param = new SqlParameter("ids", orderIdDt);
                        param.SqlDbType = SqlDbType.Structured;
                        param.TypeName = "dbo.IntValues";
                        var cmd = context.Database.Connection.CreateCommand();
                        cmd.CommandText = "[dbo].[GetInventoryCyclesBatchCode]";
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add(param);

                        // Run the sproc
                        var reader1 = cmd.ExecuteReader();
                        var InventoryCycleItemBatchDcs = ((IObjectContextAdapter)context)
                         .ObjectContext
                         .Translate<InventoryCycleItemBatchDc>(reader1).ToList();
                        if (InventoryCycleItemBatchDcs != null && InventoryCycleItemBatchDcs.Any())
                        {
                            var multiMrpIds = InventoryCycleItemBatchDcs.GroupBy(x => x.ItemMultiMrpId).Select(x => new { ItemMultiMrpId = x.Key }).ToList();

                            DateTime startDate = DateTime.Now.Date.AddDays(-10);
                            DateTime endDate = DateTime.Now;
                            var MrpIdDt = new DataTable();
                            MrpIdDt.Columns.Add("IntValue");
                            foreach (var item in multiMrpIds)
                            {
                                var dr = MrpIdDt.NewRow();
                                dr["IntValue"] = item.ItemMultiMrpId;
                                MrpIdDt.Rows.Add(dr);
                            }

                            var mrpparam = new SqlParameter("itemmultimrpIds", MrpIdDt);
                            mrpparam.SqlDbType = SqlDbType.Structured;
                            mrpparam.TypeName = "dbo.IntValues";
                            cmd = context.Database.Connection.CreateCommand();
                            cmd.CommandText = "[FIFO].[GetItemAPPsByMrpIds]";
                            cmd.CommandType = System.Data.CommandType.StoredProcedure;
                            cmd.Parameters.Add(new SqlParameter("@warehouseId", WarehouseId));
                            cmd.Parameters.Add(mrpparam);
                            cmd.Parameters.Add(new SqlParameter("@startDate", startDate));
                            cmd.Parameters.Add(new SqlParameter("@enddate", endDate));

                            // Run the sproc
                            var reader = cmd.ExecuteReader();
                            reader.NextResult();
                            reader.NextResult();
                            var LastMonthApps = ((IObjectContextAdapter)context)
                             .ObjectContext
                             .Translate<LastMonthApp>(reader).ToList();

                            foreach (var item in list)
                            {
                                //item.InventoryCycleItemBatchDcs = InventoryCycleItemBatchDcs.Where(x => x.InventCycle_Id == item.Id).ToList();
                                if (LastMonthApps.Any(x => x.itemmultimrpid == item.ItemMultiMRPId))
                                {
                                    item.APP = LastMonthApps.FirstOrDefault(x => x.itemmultimrpid == item.ItemMultiMRPId).APP;
                                }
                                //foreach (var batch in
                                //    item.InventoryCycleItemBatchDcs.Where(x => !string.IsNullOrEmpty(x.NonSellableImageUrl) || !string.IsNullOrEmpty(x.NonSellableImageUrl)))
                                //{

                                //    if (!string.IsNullOrEmpty(batch.NonSellableImageUrl))
                                //    {
                                //        batch.NonSellableImageUrl = string.Format("{0}://{1}:{2}/{3}", new Uri(HttpContext.Current.Request.Url.AbsoluteUri).Scheme
                                //                                    , HttpContext.Current.Request.Url.DnsSafeHost
                                //                                    , (HttpContext.Current.Request.Url.Port != 80 && HttpContext.Current.Request.Url.Port != 443 ? ":" + HttpContext.Current.Request.Url.Port : "")
                                //                                    , batch.NonSellableImageUrl);
                                //    }
                                //    if (!string.IsNullOrEmpty(batch.DamagedImageUrl))
                                //    {
                                //        batch.DamagedImageUrl = string.Format("{0}://{1}:{2}/{3}", new Uri(HttpContext.Current.Request.Url.AbsoluteUri).Scheme
                                //                                    , HttpContext.Current.Request.Url.DnsSafeHost
                                //                                    , (HttpContext.Current.Request.Url.Port != 80 && HttpContext.Current.Request.Url.Port != 443 ? ":" + HttpContext.Current.Request.Url.Port : "")
                                //                                    , batch.DamagedImageUrl);
                                //    }

                                //}
                            }
                        }
                    }

                    obj.GetInventCycleZeroDifferencedata = list;
                    obj.total_count = count;

                }
                return obj;


            }
        }


        [ResponseType(typeof(InventCycle))]
        [Route("EditInventCount")]
        [AcceptVerbs("PUT")]
        [HttpPut]
        public HttpResponseMessage Put(List<InventCycle> count)
        {

            return Request.CreateResponse(HttpStatusCode.InternalServerError, "This funcationlity stop due to other implemented.");

            using (AuthContext context = new AuthContext())
            {
                string username = "";
                var identity = User.Identity as ClaimsIdentity;
                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "username"))
                    username = identity.Claims.FirstOrDefault(x => x.Type == "username").Value;

                var warehouseId = count.FirstOrDefault().WarehouseId;

                var warehouse = context.Warehouses.FirstOrDefault(x => x.WarehouseId == warehouseId);

                if (!warehouse.IsStopCurrentStockTrans)
                    return Request.CreateResponse(HttpStatusCode.InternalServerError, "Please stop transactions of this warehouse before inventory count");

                foreach (var item in count)
                {
                    var InventCycles = context.InventCycleDb.Where(x => x.Id == item.Id).FirstOrDefault();
                    if (InventCycles != null)
                    {

                        if (InventCycles.InventoryCount != item.InventoryCount)
                        {

                            InventCycles.UpdatedDate = indianTime;
                            InventCycles.UpadtedBy = username;
                            InventCycles.InventoryCount = item.InventoryCount;
                            InventCycles.CurrentInventory = item.CurrentInventory;
                            InventCycles.PastInventory = item.CurrentInventory;
                            InventCycles.Comment = item.Comment;
                            InventCycles.RtdCount = item.RtdCount;
                            InventCycles.IsApproved = false;
                            context.Entry(InventCycles).State = EntityState.Modified;
                            context.Commit();

                        }
                    }
                }

                return Request.CreateResponse(HttpStatusCode.OK, count);

            }
        }



        [Route("EditInventCount")]
        [HttpPost]
        public List<InventCycle> AddApproval(List<InventCycle> count)
        {
            return null;
            using (AuthContext context = new AuthContext())
            {
                try
                {
                    string username = "";
                    var identity = User.Identity as ClaimsIdentity;
                    if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "username"))
                        username = identity.Claims.FirstOrDefault(x => x.Type == "username").Value;

                    foreach (var item in count)
                    {
                        var InventCycles = context.InventCycleDb.Where(x => x.Id == item.Id).FirstOrDefault();
                        if (InventCycles != null)
                        {
                            if (item.InventoryCount != item.CurrentInventory)
                            {
                                InventCycles.UpdatedDate = indianTime;
                                InventCycles.UpadtedBy = username;
                                InventCycles.InventoryCount = item.InventoryCount;
                                InventCycles.Comment = item.Comment;
                                InventCycles.IsApproved = false;
                                context.Entry(InventCycles).State = EntityState.Modified;
                                context.Commit();
                            }
                        }
                    }
                    return count;
                }
                catch (Exception ex)
                {
                    logger.Error("Error :" + ex.Message);
                    return null;
                }
            }
        }

        [Route("UpdateInventBatch")]
        [HttpPost]
        public bool UpdateInventoryBatch(UpdateInventoryBatchDc updateInventoryBatchDc)
        {
            var identity = User.Identity as ClaimsIdentity;
            int userid = 0;
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
            bool result = false;
            using (AuthContext context = new AuthContext())
            {
                var inventoryBatch = context.InventCycleBatchDb.FirstOrDefault(x => x.Id == updateInventoryBatchDc.Id);
                if (inventoryBatch != null)
                {
                    inventoryBatch.InventoryCount = updateInventoryBatchDc.InventoryCount;
                    inventoryBatch.NonSellableQty = updateInventoryBatchDc.NonSellableQty;
                    inventoryBatch.DamagedQty = updateInventoryBatchDc.DamagedQty;
                    inventoryBatch.UpdateBy = userid;
                    inventoryBatch.UpdatedDate = indianTime;
                    context.Entry(inventoryBatch).State = EntityState.Modified;

                    var inventoryBatchs = context.InventCycleBatchDb.Where(x => x.InventCycle_Id == inventoryBatch.InventCycle_Id && x.Id != inventoryBatch.Id).Distinct().ToList();
                    if (inventoryBatchs == null)
                        inventoryBatchs = new List<InventCycleBatch>();

                    inventoryBatchs.Add(inventoryBatch);
                    var inventory = context.InventCycleDb.FirstOrDefault(x => x.Id == inventoryBatch.InventCycle_Id);
                    inventory.InventoryCount = inventoryBatchs.Sum(x => x.InventoryCount);
                    inventory.NonSellableQty = inventoryBatchs.Sum(x => x.NonSellableQty);
                    inventory.DamagedQty = inventoryBatchs.Sum(x => x.DamagedQty);
                    context.Entry(inventory).State = EntityState.Modified;
                    result = context.Commit() > 0;
                }
            }
            return result;
        }

        [Route("SaveApprovalData")]
        [HttpPost]
        public response SaveApprovalData(List<Inventoryapproval> inventoryapprovals)
        {
            var identity = User.Identity as ClaimsIdentity;
            int userid = 0;
            response result = new response { Status = false, msg = "" };
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
            if (userid > 0)
            {
                result = CurrentStockInventoryAddandSub(inventoryapprovals, userid);
            }
            return result;
        }

        //hq level reject
        [Route("SaveRejectedData")]
        [HttpPost]
        public response SaveRejected(List<Inventoryapproval> inventoryapprovals)
        {
            response result = new response { msg = "", Status = false };

            var identity = User.Identity as ClaimsIdentity;
            int userid = 0;
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

            List<int> Invcycleids = null;
            List<InventCycle> inventCyclelist = null;
            int warehouseid = 0;
            TransactionOptions option = new TransactionOptions();
            option.IsolationLevel = System.Transactions.IsolationLevel.RepeatableRead;
            option.Timeout = TimeSpan.FromSeconds(90);
            using (var dbContextTransaction = new TransactionScope(TransactionScopeOption.Required, option))
            {
                using (var context = new AuthContext())
                {
                    var People = context.Peoples.FirstOrDefault(x => x.PeopleID == userid);
                    Invcycleids = inventoryapprovals.Select(x => x.InventoryCycleId).ToList();
                    inventCyclelist = context.InventCycleDb.Where(x => Invcycleids.Contains(x.Id)).ToList();
                    warehouseid = inventCyclelist[0].WarehouseId;
                    List<InventCycle> inventCycles = new List<InventCycle>();
                    foreach (var inventory in inventoryapprovals)
                    {
                        var inventCycleApprovedlist = context.InventCycleDb.Where(x => Invcycleids.Contains(x.Id) && !x.IsApproved && x.IsDeleted == true && x.HQApprovedId > 0).Include(x => x.InventCycleBatchs).ToList();
                        if (inventCycleApprovedlist.Count > 0)
                        {
                            result.Status = false;
                            result.msg = "Already Rejected!!";
                            return result;
                        }
                        if (!inventCycles.Where(x => x.Id == inventory.InventoryCycleId).Any())
                        {
                            var inventCycle = inventCyclelist.Where(x => x.Id == inventory.InventoryCycleId && x.IsDeleted == false).FirstOrDefault();
                            //if (inventory.VerifierStatus == 1 || inventory.VerifierStatus == 2)
                            //{
                            //    inventory.Comment = inventory.VerifierStatus == 1 ? "Excess Inventory" : "Short Inventory";
                            //}
                            //else if (inventory.VerifierStatus == 3 || inventory.VerifierStatus == 4)
                            //{
                            //    inventory.Comment = inventory.VerifierStatus == 3 ? "MRP Interchange Inventory" : "Items Interchange Inventory";
                            //}
                            string query = "select VerifiedComment from InventoryVerifiedStatus where VerifiedStatus = " + inventory.VerifierStatus;
                            inventory.Comment = context.Database.SqlQuery<string>(query).FirstOrDefault();

                            if (inventCycle != null)
                            {
                                inventCycle.IsDeleted = true;
                                inventCycle.HQUpdatedDate = DateTime.Now;
                                inventCycle.HQApprovedId = userid;
                                inventCycle.HQVerifierStatus = inventory.VerifierStatus;
                                inventCycle.HQComment = inventory.Comment;
                                inventCycles.Add(inventCycle);
                            }
                        }
                    }
                    BatchMasterManager batchMasterManager = new BatchMasterManager();
                    StockTransactionHelper sthelper = new StockTransactionHelper();
                    var InventCycleBatchtolist = context.InventCycleBatchDb.Where(x => Invcycleids.Contains(x.InventCycle_Id)).ToList();
                    var InventCycleBatchIds = InventCycleBatchtolist.Where(x => Invcycleids.Contains(x.InventCycle_Id)).Select(x => x.Id).ToList();
                    var InventCycleBatchSentForApprovalist = context.InventCycleBatchSentForApprovals.Where(x => InventCycleBatchIds.Contains(x.InventCycleBatchId)).ToList();
                    foreach (var invent in inventCycles)
                    {
                        var InventCycleBatchtolists = InventCycleBatchtolist.Where(x => x.InventCycle_Id == invent.Id).ToList();
                        bool isPV = context.Warehouses.FirstOrDefault(x => x.WarehouseId == invent.WarehouseId).IsPV;
                        foreach (var item in InventCycleBatchtolists)
                        {
                            var InventCycleBatchSentForApproval = InventCycleBatchSentForApprovalist.FirstOrDefault(x => x.InventCycleBatchId == item.Id);
                            if (InventCycleBatchSentForApproval != null)
                            {
                                var InventCycleBatch = InventCycleBatchtolist.FirstOrDefault(x => x.Id == InventCycleBatchSentForApproval.InventCycleBatchId);
                                // C<B I to C
                                if (InventCycleBatchSentForApproval.Qty > 0)
                                {
                                    PhysicalStockUpdateRequestDc manualStockUpdateDc = new PhysicalStockUpdateRequestDc
                                    {
                                        ItemMultiMRPId = invent.ItemMultiMrpId,
                                        Reason = isPV == false ? ("Inventory Cycle Dated:" + invent.CreatedDate.Value.ToString("dd/MM/yyyy")) : "Physically Verification Dated:" + invent.CreatedDate.Value.ToString("dd/MM/yyyy"),
                                        StockTransferType = "ManualInventory",
                                        Qty = InventCycleBatchSentForApproval.Qty,
                                        WarehouseId = invent.WarehouseId,
                                        DestinationStockType = StockTypeTableNames.CurrentStocks,
                                        SourceStockType = StockTypeTableNames.InventoryReserveStocks,
                                    };
                                    bool isSuccess = sthelper.TransferBetweenPhysicalStocks(manualStockUpdateDc, userid, context, dbContextTransaction);
                                    if (!isSuccess)
                                    {
                                        result.msg = "Issue in inventory cycle. Please try after some time.";
                                        return result;
                                    }
                                    batchMasterManager.UpdateStockInSameBatch(InventCycleBatch.StockBatchId, context, People.PeopleID, InventCycleBatchSentForApproval.Qty * (-1));
                                }
                                else if (InventCycleBatchSentForApproval.Qty < 0)
                                {       // C>B I to V
                                    PhysicalStockUpdateRequestDc manualStockUpdateDc = new PhysicalStockUpdateRequestDc
                                    {
                                        ItemMultiMRPId = invent.ItemMultiMrpId,
                                        Reason = (isPV == false ? "Inventory Cycle Dated:" : "Physically Verification Dated:") + invent.CreatedDate.Value.ToString("dd/MM/yyyy"),
                                        StockTransferType = "ManualInventory",
                                        Qty = InventCycleBatchSentForApproval.Qty * (-1),
                                        WarehouseId = invent.WarehouseId,
                                        DestinationStockType = StockTypeTableNames.VirtualStock,
                                        SourceStockType = StockTypeTableNames.InventoryReserveStocks,
                                    };
                                    bool isSuccess = sthelper.TransferBetweenVirtualStockAndPhysicalStocks(manualStockUpdateDc, userid, context, dbContextTransaction, IsSettleAlso: true);
                                    if (!isSuccess)
                                    {
                                        result.msg = "Issue in inventory cycle. Please try after some time.";
                                        return result;
                                    }

                                }

                                var inventCycleDataHistoryData = context.InventCycleDataHistoryDB.Where(x => x.InventoryCycleId == invent.Id && x.IsDeleted == false && x.IsActive).FirstOrDefault();
                                if (inventCycleDataHistoryData != null)
                                {
                                    inventCycleDataHistoryData.IsActive = true;
                                    inventCycleDataHistoryData.ModifiedBy = userid;
                                    inventCycleDataHistoryData.ModifiedDate = DateTime.Now;
                                    inventCycleDataHistoryData.ApprovedTimestamp = DateTime.Now;
                                    inventCycleDataHistoryData.ApprovedBy = People.DisplayName;
                                    inventCycleDataHistoryData.Status = "L&P Rejected";
                                    //inventCycleDataHistoryData.InventoryCount = invent.InventCycleBatchs.Sum(x => x.InventoryCount);
                                    context.Entry(inventCycleDataHistoryData).State = EntityState.Modified;
                                }
                                else
                                {
                                    InventCycleDataHistory list = new InventCycleDataHistory();
                                    list.InventoryCycleId = invent.Id;
                                    //list.InventoryCount = invent.InventCycleBatchs.Sum(x => x.InventoryCount);
                                    list.Status = "L&P Rejected";
                                    list.ApprovedTimestamp = DateTime.Now;
                                    list.ApprovedBy = People.DisplayName;
                                    list.IsActive = true;
                                    list.CreatedDate = DateTime.Now;
                                    list.CreatedBy = People.DisplayName;

                                    context.Entry(list).State = EntityState.Added;
                                }


                            }

                        }
                        context.Entry(invent).State = EntityState.Modified;
                    }

                    if (context.Commit() > 0)
                    {
                        result.Status = true;
                        result.msg = "inventory cycle updated successfully";
                        dbContextTransaction.Complete();
                    }
                }
            }
            return result;
        }

        [Route("SaveRejectedDataForWH")]
        [HttpPost]
        public bool SaveRejectedDataForWH(List<Inventoryapproval> inventoryapprovals)
        {
            bool result = false;
            var identity = User.Identity as ClaimsIdentity;
            int userid = 0;
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

            List<int> Invcycleids = null;
            List<int> RejectedIds = null;
            List<InventCycle> inventCyclelist = null;
            int warehouseid = 0;
            bool IsPVStart = false;
            using (var context = new AuthContext())
            {
                var People = context.Peoples.Where(x => x.PeopleID == userid).SingleOrDefault();
                Invcycleids = inventoryapprovals.Select(x => x.InventoryCycleId).ToList();
                RejectedIds = inventoryapprovals.Where(x => x.IsDeleted == true && x.IsApproval == true).Select(x => x.InventoryCycleId).ToList();
                inventCyclelist = context.InventCycleDb.Where(x => Invcycleids.Contains(x.Id)).ToList();
                warehouseid = inventCyclelist[0].WarehouseId;
                IsPVStart = context.Warehouses.Any(x => x.WarehouseId == warehouseid && x.Deleted == false && x.active == true && x.IsPV == true);

                List<InventCycle> inventCycles = new List<InventCycle>();
                foreach (var inventory in inventoryapprovals.Where(x => x.IsDeleted == true && x.IsApproval == true))
                {
                    var inventCycleApprovedlist = context.InventCycleDb.Where(x => Invcycleids.Contains(x.Id) && !x.IsApproved && x.IsWarehouseApproved == 2 && x.WarehouseApproveId > 0).Include(x => x.InventCycleBatchs).ToList();
                    if (inventCycleApprovedlist.Count > 0)
                    {
                        result = false;
                        return result;
                    }
                    if (!inventCycles.Any(x => x.Id == inventory.InventoryCycleId))
                    {
                        var inventCycle = inventCyclelist.Where(x => x.Id == inventory.InventoryCycleId && x.IsDeleted == false).FirstOrDefault();
                        //if (inventory.VerifierStatus == 1 || inventory.VerifierStatus == 2)
                        //{
                        //    inventory.Comment = inventory.VerifierStatus == 1 ? "Excess Inventory" : "Short Inventory";
                        //}
                        //else if (inventory.VerifierStatus == 3 || inventory.VerifierStatus == 4)
                        //{
                        //    inventory.Comment = inventory.VerifierStatus == 3 ? "MRP Interchange Inventory" : "Items Interchange Inventory";
                        //}
                        string query = "select VerifiedComment from InventoryVerifiedStatus where VerifiedStatus = " + inventory.VerifierStatus;
                        inventory.Comment = context.Database.SqlQuery<string>(query).FirstOrDefault();

                        if (inventCycle != null)
                        {
                            inventCycle.IsWarehouseApproved = 2;
                            inventCycle.UpdatedDate = DateTime.Now;
                            inventCycle.UpadtedBy = userid.ToString();
                            inventCycle.WarehouseApproveId = userid;
                            inventCycle.VerifierStatus = inventory.VerifierStatus;
                            inventCycle.Comment = inventory.Comment;
                            inventCycle.WarehouseComment = inventory.Comment;
                            inventCycles.Add(inventCycle);
                            //
                            var inventCycleDataHistoryData = context.InventCycleDataHistoryDB.Where(x => x.InventoryCycleId == inventCycle.Id && x.IsDeleted == false && x.IsActive).FirstOrDefault();
                            if (inventCycleDataHistoryData != null)
                            {
                                inventCycleDataHistoryData.IsActive = true;
                                inventCycleDataHistoryData.ModifiedBy = userid;
                                inventCycleDataHistoryData.ModifiedDate = DateTime.Now;
                                inventCycleDataHistoryData.VerifiedBy = People.DisplayName;
                                inventCycleDataHistoryData.VerifiedTimestamp = DateTime.Now;
                                inventCycleDataHistoryData.Status = "Warehouse Rejected";
                                context.Entry(inventCycleDataHistoryData).State = EntityState.Modified;
                            }
                            else
                            {
                                InventCycleDataHistory list = new InventCycleDataHistory();
                                list.InventoryCycleId = inventCycle.Id;
                                //list.InventoryCount = inventCycle.InventCycleBatchs.Sum(x => x.InventoryCount);
                                list.Status = "Warehouse Rejected";
                                list.CreatedDate = DateTime.Now;
                                list.CreatedBy = People.DisplayName;
                                list.VerifiedTimestamp = DateTime.Now;
                                list.VerifiedBy = People.DisplayName;
                                list.IsActive = true;

                                context.Entry(list).State = EntityState.Added;
                            }
                            //

                        }
                    }
                }

                foreach (var invent in inventCycles)
                {
                    context.Entry(invent).State = EntityState.Modified;
                }
                if (context.Commit() > 0)
                {
                    result = true;
                    var Ids = inventCycles.Select(x => x.Id);
                    if (IsPVStart)
                    {
                        DateTime AssignDate = DateTime.Now.Date;
                        MongoDbHelper<SupervisorInventoryCycleDay> mongoDbHelper = new MongoDbHelper<SupervisorInventoryCycleDay>();
                        var inventoryPredicate = PredicateBuilder.New<SupervisorInventoryCycleDay>(x => x.WarehouseId == warehouseid && x.AssignDate == AssignDate && x.InventoryCycleItems.Any(y => Ids.Contains(y.Id)));
                        if (IsPVStart)
                            inventoryPredicate = inventoryPredicate.And(x => x.IsPV.HasValue && x.IsPV.Value == IsPVStart);
                        else
                            inventoryPredicate = inventoryPredicate.And(x => (!x.IsPV.HasValue || x.IsPV.Value == IsPVStart));
                        var mongosupervisors = (mongoDbHelper.Select(inventoryPredicate)).ToList();
                        foreach (var mongosupervisor in mongosupervisors)
                        {
                            //mongosupervisor.IsSubmitted = false;
                            foreach (var item in mongosupervisor.InventoryCycleItems.Where(x => Ids.Contains(x.Id)))
                            {
                                item.IsRejected = true;
                            }
                            mongoDbHelper.ReplaceWithoutFind(mongosupervisor.Id, mongosupervisor);
                        }

                    }
                }
            }
            return result;
        }

        [Route("GetExportData")]
        [HttpGet]
        public HttpResponseMessage GetExportData(int WarehouseId, int ExportId, DateTime startDate, DateTime endDate, bool IsHQ)
        {
            using (var context = new AuthContext())
            {
                List<ExportInventCycledata> list = new List<ExportInventCycledata>();

                if (WarehouseId > 0)
                {
                    if (context.Database.Connection.State != ConnectionState.Open)
                        context.Database.Connection.Open();

                    startDate = startDate.Date;
                    endDate = endDate.Date;
                    endDate = endDate.AddDays(1).AddMilliseconds(-1);

                    var cmd = context.Database.Connection.CreateCommand();
                    cmd.CommandText = "[dbo].[GetNewInventoryCycleDataForExport]";
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@warehouseId", WarehouseId));
                    cmd.Parameters.Add(new SqlParameter("@startDate", startDate));
                    cmd.Parameters.Add(new SqlParameter("@endDate", endDate));
                    cmd.Parameters.Add(new SqlParameter("@Exporttype", ExportId));
                    cmd.Parameters.Add(new SqlParameter("@IsHQ", IsHQ));

                    var reader1 = cmd.ExecuteReader();
                    list = ((IObjectContextAdapter)context)
                    .ObjectContext
                    .Translate<ExportInventCycledata>(reader1).ToList();

                    //var query = "";
                    //if (ExportId == 1)
                    //{
                    //    query = "select ic.IsDeleted,cs.ItemMultiMRPId as ItemMultiMRPId,cs.ItemName,cs.MRP,bm.BatchCode,bm.MFGDate,bm.ExpiryDate,b.InventoryCount batchInventoryCount,b.DamagedQty batchDamagedQty,b.NonSellableQty batchNonSellableQty,ic.RtdCount as RtdCount,ic.RtpCount,ic.DamagedQty,Ic.NonSellableQty,w.WarehouseId,w.WarehouseName as WarehouseName,ic.InventoryCount as InventoryCount,ic.Comment as Comment,cs.CurrentInventory, ic.CreatedDate,ic.PastInventory,ic.UpdatedDate,ic.UpadtedBy as Updatedby,ic.IsApproved,ISNULL(ICl.Category,'D') as ABCClassification  from InventCycles ic inner join Warehouses w on w.WarehouseId =ic.WarehouseId and ic.IsApproved=1 and ic.InventoryCount<> ic.PastInventory and w.WarehouseId= " + WarehouseId + " and ic.Isdeleted=0 inner join CurrentStocks cs on ic.ItemMultiMRPId=cs.ItemMultiMRPId and cs.WarehouseId = ic.WarehouseId  Inner join InventCycleBatches  b with(nolock) on ic.Id=b.InventCycle_Id   inner join BatchMasters bm with(nolock) on bm.Id=b.BatchMasterId  Left join ItemsClassification ICL on ICL.itemnumber = cs.ItemNumber and ICL.warehouseid = cs.WarehouseId";
                    //    query += " Where Ic.CreatedDate between '" + startDate.Date.ToString(@"yyyy-MM-dd") + "' And '" + endDate.Date.ToString(@"yyyy-MM-dd") + " 23:59:59'";
                    //}
                    //else if (ExportId == 2)
                    //{
                    //    query = "select ic.IsDeleted,cs.ItemMultiMRPId as ItemMultiMRPId,cs.ItemName,cs.MRP,bm.BatchCode,bm.MFGDate,bm.ExpiryDate,b.InventoryCount batchInventoryCount,b.DamagedQty batchDamagedQty,b.NonSellableQty batchNonSellableQty,ic.RtdCount as RtdCount,ic.RtpCount,ic.DamagedQty,Ic.NonSellableQty,w.WarehouseId,w.WarehouseName as WarehouseName,ic.InventoryCount as InventoryCount,ic.Comment as Comment,cs.CurrentInventory, ic.CreatedDate,ic.PastInventory,ic.UpdatedDate,ic.UpadtedBy as Updatedby,ic.IsApproved,ISNULL(ICl.Category,'D') as ABCClassification  from InventCycles ic inner join Warehouses w on w.WarehouseId =ic.WarehouseId and ic.IsApproved=0 and ic.InventoryCount<> ic.PastInventory and w.WarehouseId= " + WarehouseId + " and ic.Isdeleted=1 inner join CurrentStocks cs on ic.ItemMultiMRPId=cs.ItemMultiMRPId and cs.WarehouseId = ic.WarehouseId Inner join InventCycleBatches  b with(nolock) on ic.Id=b.InventCycle_Id   inner join BatchMasters bm with(nolock) on bm.Id=b.BatchMasterId Left join ItemsClassification ICL on ICL.itemnumber = cs.ItemNumber and ICL.warehouseid = cs.WarehouseId";
                    //    query += " Where Ic.CreatedDate between '" + startDate.Date.ToString(@"yyyy-MM-dd") + "' And '" + endDate.Date.ToString(@"yyyy-MM-dd") + " 23:59:59'";
                    //}
                    //else if (ExportId == 0)
                    //{
                    //    query = "select ic.IsDeleted,cs.ItemMultiMRPId as ItemMultiMRPId,cs.ItemName,cs.MRP,bm.BatchCode,bm.MFGDate,bm.ExpiryDate,b.InventoryCount batchInventoryCount,b.DamagedQty batchDamagedQty,b.NonSellableQty batchNonSellableQty,ic.RtdCount as RtdCount,ic.RtpCount,ic.DamagedQty,Ic.NonSellableQty,w.WarehouseId,w.WarehouseName as WarehouseName,ic.InventoryCount as InventoryCount,ic.Comment as Comment,cs.CurrentInventory, ic.CreatedDate,ic.PastInventory,ic.UpdatedDate,ic.UpadtedBy as Updatedby,ic.IsApproved,ISNULL(ICl.Category,'D') as ABCClassification  from InventCycles ic inner join Warehouses w on w.WarehouseId =ic.WarehouseId  and ic.InventoryCount<> ic.PastInventory and w.WarehouseId= " + WarehouseId + "  inner join CurrentStocks cs on ic.ItemMultiMRPId=cs.ItemMultiMRPId and cs.WarehouseId = ic.WarehouseId Inner join InventCycleBatches  b with(nolock) on ic.Id=b.InventCycle_Id   inner join BatchMasters bm with(nolock) on bm.Id=b.BatchMasterId Left join ItemsClassification ICL on ICL.itemnumber = cs.ItemNumber and ICL.warehouseid = cs.WarehouseId";
                    //    query += " Where Ic.CreatedDate between '" + startDate.Date.ToString(@"yyyy-MM-dd") + "' And '" + endDate.Date.ToString(@"yyyy-MM-dd") + " 23:59:59'";
                    //}
                    //list = context.Database.SqlQuery<ExportInventCycledata>(query).OrderByDescending(x => x.UpdatedDate).ToList();
                }
                return Request.CreateResponse(HttpStatusCode.OK, list);
            }
        }

        //export

        [Route("ExportItemWiseInventoryCycle")]
        [HttpGet]
        public async Task<List<ExportInventCycledataItemWise>> ExportItemWiseInventoryCycle(int WarehouseId, bool IsHQ, DateTime? start, DateTime? end, int status, bool IsPV)//int status
        {
            var identity = User.Identity as ClaimsIdentity;
            int userid = 0;
            int Warehouse_id = 0;
            string RoleName = null;
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "Warehouseid"))
                Warehouse_id = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "Warehouseid").Value);

            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "RoleNames"))
                RoleName = identity.Claims.FirstOrDefault(x => x.Type == "RoleNames").Value.ToString();
            using (var context = new AuthContext())
            {
                List<ExportInventCycledataItemWise> list = new List<ExportInventCycledataItemWise>();
                if (WarehouseId > 0)
                {
                    Warehouse_id = WarehouseId;
                }
                if (Warehouse_id > 0)
                {

                    if (context.Database.Connection.State != ConnectionState.Open)
                        context.Database.Connection.Open();
                    context.Database.CommandTimeout = 120;
                    var stmt = context.Database.Connection.CreateCommand();
                    stmt.CommandText = "[dbo].[ExportItemWiseInventoryCycle]";
                    stmt.CommandType = System.Data.CommandType.StoredProcedure;
                    stmt.Parameters.Add(new SqlParameter("WarehouseId", SqlDbType.Int) { Value = Warehouse_id });
                    stmt.Parameters.Add(new SqlParameter("IsHQ", SqlDbType.Int) { Value = IsHQ });
                    stmt.Parameters.Add(new SqlParameter("status", SqlDbType.Int) { Value = status });
                    stmt.Parameters.Add(new SqlParameter("start", start ?? (object)DBNull.Value));
                    stmt.Parameters.Add(new SqlParameter("end", end ?? (object)DBNull.Value));
                    stmt.Parameters.Add(new SqlParameter("IsPV", SqlDbType.Int) { Value = IsPV });


                    var reader = stmt.ExecuteReader();
                    try
                    {
                        list = ((IObjectContextAdapter)context)
                        .ObjectContext
                        .Translate<ExportInventCycledataItemWise>(reader).ToList();
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }
                }
                return list;
            }
        }


        //

        [Route("InsertTodayInventoryItem")]
        [HttpGet]
        [AllowAnonymous]
        public bool InsertTodayInventoryItem()
        {
            bool result = false;
            DateTime startDate = DateTime.Now.AddDays(-1).Date;
            DateTime endDate = startDate.AddDays(1).AddMilliseconds(-1);

            var itemData = new List<InvetoryCycleItemsForBatch>();
            var incompleteCycles = new List<InventCycleMasterForWarehouse>();

            var todayInventoryCycle = false;
            using (var authContext = new AuthContext())
            {
                var todayDate = DateTime.Now;
                todayInventoryCycle = authContext.InventCycleDb.Any(x => EntityFunctions.TruncateTime(x.CreatedDate.Value) >= EntityFunctions.TruncateTime(todayDate));
                if (!todayInventoryCycle)
                {
                    if (authContext.Database.Connection.State != ConnectionState.Open)
                        authContext.Database.Connection.Open();

                    var cmd = authContext.Database.Connection.CreateCommand();
                    cmd.CommandText = "[dbo].[GetInventoryCyclesForBatch]";
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.CommandTimeout = 1200;
                    // Run the sproc
                    var reader = cmd.ExecuteReader();
                    itemData = ((IObjectContextAdapter)authContext)
                                   .ObjectContext
                                   .Translate<InvetoryCycleItemsForBatch>(reader).ToList();
                    reader.NextResult();


                    if (reader.HasRows)
                    {
                        //DataTable dt = new DataTable();
                        //dt.Load(reader);

                        incompleteCycles = ((IObjectContextAdapter)authContext)
                                            .ObjectContext
                                            .Translate<InventCycleMasterForWarehouse>(reader).ToList();
                    }
                }
            }

            using (var authContext = new AuthContext())
            {
                if (!todayInventoryCycle)
                {
                    if (itemData != null && itemData.Any())
                    {
                        foreach (var item in itemData.GroupBy(x => x.WarehouseId))
                        {
                            var invCycleMasters = new List<InventoryCycleMaster>();

                            var incompleteCycle = incompleteCycles.Where(x => x.WarehouseId == item.Key).ToList();
                            if (!incompleteCycle.Any(x => x.Classification.StartsWith("A")))
                            {

                                invCycleMasters.Add(new InventoryCycleMaster
                                {
                                    WarehouseId = item.Key,
                                    CreatedDate = indianTime,
                                    IsComplete = false,
                                    Classification = "A"
                                });
                            }

                            if (!incompleteCycle.Any(x => x.Classification.StartsWith("B")))
                            {
                                invCycleMasters.Add(new InventoryCycleMaster
                                {
                                    WarehouseId = item.Key,
                                    CreatedDate = indianTime,
                                    IsComplete = false,
                                    Classification = "B"
                                });
                            }

                            if (!incompleteCycle.Any(x => x.Classification.StartsWith("C")))
                            {
                                invCycleMasters.Add(new InventoryCycleMaster
                                {
                                    WarehouseId = item.Key,
                                    CreatedDate = indianTime,
                                    IsComplete = false,
                                    Classification = "C"
                                });
                            }

                            if (!incompleteCycle.Any(x => x.Classification.StartsWith("D")))
                            {
                                invCycleMasters.Add(new InventoryCycleMaster
                                {
                                    WarehouseId = item.Key,
                                    CreatedDate = indianTime,
                                    IsComplete = false,
                                    Classification = "D"
                                });
                            }

                            if (incompleteCycle != null && incompleteCycle.Any())
                            {
                                var incompleteCycleIds = incompleteCycle.Select(x => x.InventoryCycleMasterId);
                                invCycleMasters.AddRange(authContext.InventoryCycleMasterDb.Where(x => incompleteCycleIds.Contains(x.InventoryCycleMasterId))
                                             .Include(x => x.InventoryCycles).ToList());

                            }

                            //var totalItems = 0;


                            foreach (var invCycleMaster in invCycleMasters.OrderBy(x => x.Classification).ToList())
                            {
                                //invCycleMaster.InventoryCycles = invCycleMaster.InventoryCycles != null ? invCycleMaster.InventoryCycles.Where(x => !x.IsApproved && x.CreatedDate.Value.Date == DateTime.Now.AddDays(-1).Date).ToList() : new List<InventCycle>();
                                List<InvetoryCycleItemsForBatch> catItems = new List<InvetoryCycleItemsForBatch>();

                                if (invCycleMaster.Classification.StartsWith("A"))
                                {
                                    //totalItems += item.Where(x => x.cat.StartsWith("A") && invCycleMasters.Where(y => y.WarehouseId == x.WarehouseId).SelectMany(y => y.InventoryCycles).Select(z => z.ItemMultiMrpId).Contains(x.ItemMultiMRPId))
                                    //                              .Skip(0).Take(10).Count();
                                    catItems = item.Where(x => x.cat.StartsWith("A") && !invCycleMasters.Where(y => y.WarehouseId == x.WarehouseId && y.InventoryCycles != null).SelectMany(y => y.InventoryCycles).Select(z => z.ItemMultiMrpId).Contains(x.ItemMultiMRPId))
                                                                  .Skip(0).Take(10).ToList();
                                }

                                if (invCycleMaster.Classification.StartsWith("B"))
                                {
                                    //totalItems += item.Where(x => x.cat.StartsWith("B") && (invCycleMaster.InventoryCycleMasterId == 0 || (invCycleMaster.InventoryCycleMasterId > 0 && !invCycleMasters.Where(y => y.WarehouseId == x.WarehouseId).SelectMany(y => y.InventoryCycles).Select(z => z.ItemMultiMrpId).Contains(x.ItemMultiMRPId))))
                                    //                              .Skip(0).Take(30).Count();
                                    catItems = item.Where(x => x.cat.StartsWith("B") && !invCycleMasters.Where(y => y.WarehouseId == x.WarehouseId && y.InventoryCycles != null).SelectMany(y => y.InventoryCycles).Select(z => z.ItemMultiMrpId).Contains(x.ItemMultiMRPId))
                                                                  .Skip(0).Take(30).ToList();
                                }

                                if (invCycleMaster.Classification.StartsWith("C"))
                                {

                                    catItems = item.Where(x => x.cat.StartsWith("C") && !invCycleMasters.Where(y => y.WarehouseId == x.WarehouseId && y.InventoryCycles != null).SelectMany(y => y.InventoryCycles).Select(z => z.ItemMultiMrpId).Contains(x.ItemMultiMRPId))
                                                                  .Skip(0).Take(20).ToList();
                                }

                                if (invCycleMaster.Classification.StartsWith("D"))
                                {
                                    catItems = item.Where(x => x.cat.StartsWith("D") && !invCycleMasters.Where(y => y.WarehouseId == x.WarehouseId && y.InventoryCycles != null).SelectMany(y => y.InventoryCycles).Select(z => z.ItemMultiMrpId).Contains(x.ItemMultiMRPId))
                                                                  .Skip(0).Take(15).ToList(); // 75-totalItems
                                }



                                if (invCycleMaster.InventoryCycleMasterId > 0)
                                {
                                    var deleted = invCycleMaster.InventoryCycles.Where(x => x.IsDeleted && x.CreatedDate.Value.Date == indianTime.Date.AddDays(-1));

                                    catItems.AddRange(invCycleMaster.InventoryCycles.Where(x => x.Id > 0).GroupBy(x => new { x.ItemMultiMrpId, x.WarehouseId })
                                        .Where(x => x.Select(z => z.UpadtedBy).All(z => string.IsNullOrEmpty(z)))
                                        .Select(z => new InvetoryCycleItemsForBatch
                                        {
                                            ItemMultiMRPId = z.Key.ItemMultiMrpId,
                                            WarehouseId = z.Key.WarehouseId
                                        }).Where(x => item.Select(y => y.ItemMultiMRPId).Contains(x.ItemMultiMRPId)).ToList());

                                    if (deleted.Any() && !catItems.Any(s => deleted.Select(d => d.ItemMultiMrpId).Contains(s.ItemMultiMRPId)))
                                    {
                                        catItems.AddRange(deleted.Where(s => !catItems.Select(d => d.ItemMultiMRPId).Contains(s.ItemMultiMrpId))
                                            .GroupBy(x => new { x.ItemMultiMrpId, x.WarehouseId })
                                            .Select(z => new InvetoryCycleItemsForBatch
                                            {
                                                ItemMultiMRPId = z.Key.ItemMultiMrpId,
                                                WarehouseId = z.Key.WarehouseId
                                            }).Where(x => item.Select(y => y.ItemMultiMRPId).Contains(x.ItemMultiMRPId)).ToList());
                                    }

                                }

                                if (invCycleMaster.InventoryCycleMasterId > 0 && (catItems == null || !catItems.Any()))
                                {
                                    invCycleMaster.IsComplete = true;
                                    invCycleMaster.CompletionDate = indianTime;

                                }

                                if (invCycleMaster.InventoryCycleMasterId > 0)
                                {
                                    authContext.Entry(invCycleMaster).State = EntityState.Modified;
                                }
                                else
                                {
                                    authContext.InventoryCycleMasterDb.Add(invCycleMaster);
                                }

                                authContext.Commit();

                                long masterId = invCycleMaster.InventoryCycleMasterId;

                                if (invCycleMaster.IsComplete)
                                {
                                    var invCycleMasterNew = new InventoryCycleMaster
                                    {
                                        WarehouseId = item.Key,
                                        CreatedDate = indianTime,
                                        IsComplete = false,
                                        Classification = invCycleMaster.Classification
                                    };

                                    authContext.InventoryCycleMasterDb.Add(invCycleMasterNew);
                                    authContext.Commit();

                                    if (invCycleMaster.Classification.StartsWith("A"))
                                    {


                                        catItems = item.Where(x => x.cat.StartsWith("A"))
                                                .Skip(0).Take(10).ToList();
                                    }

                                    if (invCycleMaster.Classification.StartsWith("B"))
                                    {

                                        catItems.AddRange(item.Where(x => x.cat.StartsWith("B"))
                                                        .Skip(0).Take(30).ToList());
                                    }
                                    if (invCycleMaster.Classification.StartsWith("C"))
                                    {
                                        catItems.AddRange(item.Where(x => x.cat.StartsWith("C"))
                                                        .Skip(0).Take(20).ToList());
                                    }

                                    if (invCycleMaster.Classification.StartsWith("D"))
                                        catItems.AddRange(item.Where(x => x.cat.StartsWith("D"))
                                                        .Skip(0).Take(15).ToList()); // 75-totalItems

                                    masterId = invCycleMasterNew.InventoryCycleMasterId;
                                }

                                if (catItems != null && catItems.Any())
                                {
                                    var invCycles = new List<InventCycle>();
                                    catItems.ForEach(x =>
                                    {
                                        invCycles.Add(new InventCycle
                                        {
                                            CreatedDate = indianTime,
                                            ItemMultiMrpId = x.ItemMultiMRPId,
                                            WarehouseId = x.WarehouseId,
                                            InventoryCycleMasterId = masterId,
                                            IsWarehouseApproved = -1
                                        });

                                    });

                                    authContext.InventCycleDb.AddRange(invCycles);
                                    result = authContext.Commit() > 0;
                                }
                            }
                        }
                    }
                }
            }


            return result;
        }

        [Route("SendEmailForInventoryNotFill")]
        [HttpGet]
        [AllowAnonymous]
        public bool SendEmailForInventoryNotFill()
        {
            bool result = false;
            try
            {
                DataTable dataTable = new DataTable();
                string ExcelSavePath = HttpContext.Current.Server.MapPath("~/ExcelGeneratePath/");
                using (var connection = new SqlConnection(connectionString))
                {
                    string sqlquery = "Select a.Warehouseid,b.WarehouseName from InventCycles a inner join Warehouses b on a.WarehouseId=b.WarehouseId where Cast(a.CreatedDate as date)=cast(getdate() as date) and a.InventoryCount is null group by a.warehouseid,b.WarehouseName";
                    using (var command = new SqlCommand(sqlquery.ToString(), connection))
                    {
                        if (connection.State != ConnectionState.Open)
                            connection.Open();

                        SqlDataAdapter da = new SqlDataAdapter(command);
                        da.Fill(dataTable);
                        da.Dispose();
                        connection.Close();
                    }
                }


                if (dataTable.Rows.Count > 0)
                {
                    string warehouse = "";
                    int warehouseid = 0;
                    for (int i = 0; i < dataTable.Rows.Count; i++)
                    {
                        // warehouse = string.IsNullOrEmpty(warehouse) ? dataTable.Rows[i]["WarehouseName"].ToString() : ", " + dataTable.Rows[i]["WarehouseName"].ToString();
                        warehouse = dataTable.Rows[i]["WarehouseName"].ToString();
                        warehouseid = Convert.ToInt32(dataTable.Rows[i]["Warehouseid"]);

                        string To = "", From = "", Bcc = "";
                        DataTable emaildatatable = new DataTable();
                        using (var connection = new SqlConnection(connectionString))
                        {
                            using (var command = new SqlCommand("Select * from EmailRecipients where EmailType='InventoryNotFill' and WarehouseId=" + warehouseid + "", connection))
                            {
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
                        string subject = "Inventory cycle form not filled for date: " + DateTime.Now.ToString("dd MMM yyyy");
                        string message = warehouse;
                        //foreach (var item in warehouse.Split(','))
                        //{
                        //    message += item + "<br/>";
                        //}
                        //string message = "Warehouse " + warehouse + " Not Fill Inventory, Please do the needfull required action.";
                        if (!string.IsNullOrEmpty(To) && !string.IsNullOrEmpty(From))
                            result = EmailHelper.SendMail(From, To, Bcc, subject, message, "");
                        else
                            logger.Error("InventoryNotFill To and From empty");
                    }

                }
                result = true;
            }
            catch (Exception ex)
            {
                logger.Error("Error in InventoryNotFill Method: " + ex.Message);
            }

            return result;
        }

        private response CurrentStockInventoryAddandSub(List<Inventoryapproval> inventoryapprovals, int userid)
        {
            response result = new response { msg = "", Status = false };
            People People = null;
            List<int> Invcycleids = null;
            List<InventCycle> inventCyclelist = null;
            List<int> inventCycleMultiMrpids = null;
            int warehouseid = 0;
            List<CurrentStock> currentstockList = null;
            List<StockBatchMaster> stockBatchMasters = new List<StockBatchMaster>();
            List<InventCycleBatchSettlement> inventCycleBatchSettlementList = new List<InventCycleBatchSettlement>();
            TransactionOptions option = new TransactionOptions();
            option.IsolationLevel = System.Transactions.IsolationLevel.RepeatableRead;
            option.Timeout = TimeSpan.FromSeconds(90);
            using (var dbContextTransaction = new TransactionScope(TransactionScopeOption.RequiresNew, option))
            {
                using (var context = new AuthContext())
                {
                    Invcycleids = inventoryapprovals.Select(x => x.InventoryCycleId).ToList();
                    inventCyclelist = context.InventCycleDb.Where(x => Invcycleids.Contains(x.Id) && !x.IsApproved).Include(x => x.InventCycleBatchs).ToList();
                    if (inventCyclelist != null && inventCyclelist.Any())
                    {
                        inventCycleMultiMrpids = inventCyclelist.Select(x => x.ItemMultiMrpId).ToList();
                        warehouseid = inventCyclelist[0].WarehouseId;
                        var warehouse = context.Warehouses.FirstOrDefault(x => x.WarehouseId == warehouseid);
                        AngularJSAuthentication.BatchManager.Helpers.ElasticBatchHelper elasticBatchHelper = new AngularJSAuthentication.BatchManager.Helpers.ElasticBatchHelper();
                        //if (!warehouse.IsStopCurrentStockTrans || elasticBatchHelper.IsAnyPendingDocExists(inventCycleMultiMrpids, warehouseid))
                        if (!warehouse.IsStopCurrentStockTrans)
                        {
                            result.msg = "Please stop transactions of this warehouse before inventory count";
                            return result;
                        }
                        People = context.Peoples.FirstOrDefault(x => x.PeopleID == userid);
                        currentstockList = context.DbCurrentStock.Where(c => inventCycleMultiMrpids.Contains(c.ItemMultiMRPId) && c.Deleted == false && c.WarehouseId == warehouseid).ToList();
                        var cStockIds = currentstockList.Select(x => Convert.ToInt64(x.StockId)).ToList();
                        stockBatchMasters = context.StockBatchMasters.Where(x => cStockIds.Contains(x.StockId) && x.StockType == "C").ToList();
                        var batchmasterids = inventCyclelist.SelectMany(x => x.InventCycleBatchs).Select(x => x.BatchMasterId).Distinct().ToList();
                        var batchmasters = context.BatchMasters.Where(x => batchmasterids.Contains(x.Id)).ToList();
                        var stockTxnTypes = context.StockTxnTypeMasters.Where(x => x.StockTxnType.Contains("InventoryCycle"));
                        var stockTxnTypeIn = stockTxnTypes.FirstOrDefault(x => x.StockTxnType == "InventoryCycleIn");
                        var stockTxnTypeOut = stockTxnTypes.FirstOrDefault(x => x.StockTxnType == "InventoryCycleOut");
                        StockTransactionHelper sthelper = new StockTransactionHelper();
                        BatchMasterManager batchMasterManager = new BatchMasterManager();
                        List<InventCycleBatchSentForApproval> InventCycleBatchSentForApprovalList = new List<InventCycleBatchSentForApproval>();

                        foreach (var inventCycle in inventCyclelist)
                        {
                            bool isHQ = inventoryapprovals.FirstOrDefault(x => x.InventoryCycleId == inventCycle.Id).IsHQ;
                            if (isHQ)
                            {
                                var inventCycleApprovedlist = context.InventCycleDb.Where(x => Invcycleids.Contains(x.Id) && !x.IsApproved && x.IsWarehouseApproved == 1 && x.HQApprovedId > 0).Include(x => x.InventCycleBatchs).ToList();
                                if (inventCycleApprovedlist.Count > 0)
                                {
                                    result.msg = "Already Submitted!!";
                                    return result;
                                }
                            }
                            if (!isHQ)
                            {
                                var inventCycleApprovedlist = context.InventCycleDb.Where(x => Invcycleids.Contains(x.Id) && !x.IsApproved && x.IsWarehouseApproved == 1 && x.WarehouseApproveId > 0).Include(x => x.InventCycleBatchs).ToList();
                                if (inventCycleApprovedlist.Count > 0)
                                {
                                    result.msg = "Already Submitted!!";
                                    return result;
                                }
                            }
                        }
                        foreach (var inventCycle in inventCyclelist)
                        {
                            //var inventCycleApprovedlist = context.InventCycleDb.Where(x => Invcycleids.Contains(x.Id) && !x.IsApproved && x.IsWarehouseApproved == 1).Include(x => x.InventCycleBatchs).ToList();                            

                            if (!warehouse.IsStopCurrentStockTrans)
                            {
                                result.msg = "Please stop transactions of this warehouse before inventory count";
                                return result;
                            }
                            var InventCycleBatchs = inventCycle.InventCycleBatchs;
                            if (InventCycleBatchs != null && InventCycleBatchs.Any())
                            {
                                var currentstock = currentstockList.FirstOrDefault(x => x.ItemMultiMRPId == inventCycle.ItemMultiMrpId && x.WarehouseId == inventCycle.WarehouseId);
                                int currentInventory = currentstock.CurrentInventory;
                                if (inventCycle.IsWarehouseApproved == 0)
                                {
                                    // Case 1 when batch and stock are same 
                                    if (currentstock.CurrentInventory == InventCycleBatchs.Sum(x => x.InventoryCount))
                                    {
                                        //for same value in batch
                                        foreach (var batchData in InventCycleBatchs.Where(x => ((x.PastInventory ?? 0)) == x.InventoryCount).ToList())
                                        {
                                            batchData.IsStockUpdated = true;
                                            batchData.UpdateBy = People.PeopleID;
                                            batchData.UpdatedDate = DateTime.Now;
                                            context.Entry(batchData).State = EntityState.Modified;
                                            inventCycleBatchSettlementList.Add(new InventCycleBatchSettlement
                                            {
                                                ToInventCycleBatchId = Convert.ToInt32(batchData.StockBatchId),
                                                FromInventCycleBatchId = batchData.StockBatchId > 0 ? Convert.ToInt32(batchData.StockBatchId) : Convert.ToInt32(batchData.StockBatchId),
                                                Qty = (-1) * batchData.InventoryCount,
                                                CreatedBy = People.PeopleID,
                                                CreatedDate = DateTime.Now
                                            });
                                        }


                                        foreach (var batchData in InventCycleBatchs.Where(x => x.IsStockUpdated == false))
                                        {
                                            var batchmaster = batchmasters.FirstOrDefault(x => x.Id == batchData.BatchMasterId);
                                            if (batchmaster != null && !batchData.IsStockUpdated)
                                            {
                                                var StockBatchMaster = stockBatchMasters.FirstOrDefault(x => x.Id == batchData.StockBatchId && x.StockType == "C");
                                                if (StockBatchMaster == null)
                                                {
                                                    long StockBatchMasterId = batchMasterManager.GetOrCreate(currentstock.ItemMultiMRPId, currentstock.WarehouseId, "C", batchData.BatchMasterId, context, userid);
                                                    StockBatchMaster = context.StockBatchMasters.FirstOrDefault(x => x.Id == StockBatchMasterId && x.StockType == "C");
                                                    stockBatchMasters.Add(StockBatchMaster);
                                                }
                                                int Diffinventory = batchData.InventoryCount - ((batchData.PastInventory ?? 0));

                                                TextFileLogHelper.TraceLog("StockBatchMaster.Qty:"+StockBatchMaster.Qty);
                                                TextFileLogHelper.TraceLog("StockBatchMaster.Qty:"+ currentstock.CurrentInventory);
                                                if (StockBatchMaster.Qty != currentstock.CurrentInventory)
                                                {
                                                    result.msg = "Current Inventory Qty and BatchCode Qty Mismatch!";
                                                    return result;
                                                }
                                                if (Diffinventory != 0)
                                                {
                                                    batchMasterManager.UpdateStockInSameBatch(StockBatchMaster.Id, context, People.PeopleID, Diffinventory * (-1));
                                                }
                                                inventCycleBatchSettlementList.Add(new InventCycleBatchSettlement
                                                {
                                                    ToInventCycleBatchId = Convert.ToInt32(StockBatchMaster.Id),
                                                    FromInventCycleBatchId = batchData.StockBatchId > 0 ? Convert.ToInt32(batchData.StockBatchId) : Convert.ToInt32(StockBatchMaster.Id),
                                                    Qty = Diffinventory,
                                                    CreatedBy = People.PeopleID,
                                                    CreatedDate = DateTime.Now
                                                });
                                                batchData.IsStockUpdated = true;
                                                batchData.UpdateBy = People.PeopleID;
                                                batchData.UpdatedDate = DateTime.Now;
                                                context.Entry(batchData).State = EntityState.Modified;
                                            }
                                        }
                                        inventCycle.IsApproved = true;
                                        var inventoryApprovaldataa = inventoryapprovals.FirstOrDefault(x => x.InventoryCycleId == inventCycle.Id);
                                        var inventCycleDataHistoryData = context.InventCycleDataHistoryDB.Where(x => x.InventoryCycleId == inventCycle.Id && x.IsDeleted == false && x.IsActive).FirstOrDefault();
                                        if (inventCycleDataHistoryData != null)
                                        {
                                            inventCycleDataHistoryData.IsActive = true;
                                            inventCycleDataHistoryData.ModifiedBy = userid;
                                            inventCycleDataHistoryData.ModifiedDate = DateTime.Now;
                                            if (inventoryApprovaldataa.IsHQ == false)
                                            {
                                                inventCycleDataHistoryData.VerifiedBy = People.DisplayName;
                                                inventCycleDataHistoryData.VerifiedTimestamp = DateTime.Now;
                                            }
                                            else
                                            {
                                                inventCycleDataHistoryData.ApprovedBy = People.DisplayName;
                                                inventCycleDataHistoryData.ApprovedTimestamp = DateTime.Now;
                                            }

                                            inventCycleDataHistoryData.Status = inventoryApprovaldataa.IsHQ == false ? "Warehouse Approved" : "HQ Approved";
                                            context.Entry(inventCycleDataHistoryData).State = EntityState.Modified;
                                        }
                                        else
                                        {
                                            InventCycleDataHistory list = new InventCycleDataHistory();
                                            list.InventoryCycleId = inventCycle.Id;
                                            list.InventoryCount = inventCycle.InventCycleBatchs.Sum(x => x.InventoryCount);
                                            list.Status = inventoryApprovaldataa.IsHQ == false ? "Warehouse Approved" : "HQ Approved";
                                            list.CreatedDate = DateTime.Now;
                                            list.CreatedBy = People.DisplayName;
                                            if (inventoryApprovaldataa.IsHQ == false)
                                            {
                                                list.VerifiedBy = People.DisplayName;
                                                list.VerifiedTimestamp = DateTime.Now;
                                            }
                                            else
                                            {
                                                list.ApprovedBy = People.DisplayName;
                                                list.ApprovedTimestamp = DateTime.Now;
                                            }
                                            list.IsActive = true;

                                            context.Entry(list).State = EntityState.Added;
                                        }
                                    }

                                    //Case 2 when batch and stock are not same ( Batch in + ) 
                                    else if (currentstock.CurrentInventory < InventCycleBatchs.Sum(x => x.InventoryCount))
                                    {
                                        //for same value in batch
                                        foreach (var batchData in InventCycleBatchs.Where(x => ((x.PastInventory ?? 0)) == x.InventoryCount).ToList())
                                        {
                                            batchData.IsStockUpdated = true;
                                            batchData.UpdateBy = People.PeopleID;
                                            batchData.UpdatedDate = DateTime.Now;
                                            context.Entry(batchData).State = EntityState.Modified;
                                            inventCycleBatchSettlementList.Add(new InventCycleBatchSettlement
                                            {
                                                ToInventCycleBatchId = Convert.ToInt32(batchData.StockBatchId),
                                                FromInventCycleBatchId = batchData.StockBatchId > 0 ? Convert.ToInt32(batchData.StockBatchId) : Convert.ToInt32(batchData.StockBatchId),
                                                Qty = (-1) * batchData.InventoryCount,
                                                CreatedBy = People.PeopleID,
                                                CreatedDate = DateTime.Now
                                            });
                                        }

                                        //preparelist for orderby 
                                        List<BatchCurrentVsInvCountDc> BatchCurrentVsInvCountList = new List<BatchCurrentVsInvCountDc>();
                                        foreach (var batchData in InventCycleBatchs.Where(x => x.IsStockUpdated == false))
                                        {
                                            var batchmaster = batchmasters.FirstOrDefault(x => x.Id == batchData.BatchMasterId);
                                            if (batchmaster != null && !batchData.IsStockUpdated)
                                            {
                                                var StockBatchMaster = stockBatchMasters.FirstOrDefault(x => x.Id == batchData.StockBatchId && x.StockType == "C");
                                                if (StockBatchMaster == null)
                                                {
                                                    long StockBatchMasterId = batchMasterManager.GetOrCreate(currentstock.ItemMultiMRPId, currentstock.WarehouseId, "C", batchData.BatchMasterId, context, userid);
                                                    StockBatchMaster = context.StockBatchMasters.FirstOrDefault(x => x.Id == StockBatchMasterId && x.StockType == "C");
                                                    stockBatchMasters.Add(StockBatchMaster);

                                                    batchData.StockBatchId = StockBatchMaster.Id;
                                                    
                                                }
                                                BatchCurrentVsInvCountList.Add(new BatchCurrentVsInvCountDc
                                                {
                                                    StockBatchMasterId = StockBatchMaster.Id,
                                                    Qty = batchData.InventoryCount - ((batchData.PastInventory ?? 0))
                                                });
                                                //new check for Batch Mismatch
                                                TextFileLogHelper.TraceLog("StockBatchMaster.Qty:" + StockBatchMaster.Qty);
                                                TextFileLogHelper.TraceLog("StockBatchMaster.Qty:" + currentstock.CurrentInventory);
                                                if (StockBatchMaster.Qty != currentstock.CurrentInventory)
                                                {
                                                    result.msg = "Current Inventory Qty and BatchCode Qty Mismatch!";
                                                    return result;
                                                }
                                            }
                                        }

                                        //then order by InventoryCount 
                                        List<InventCycleBatchExtraQty> InventCycleBatchExtraQtys = new List<InventCycleBatchExtraQty>();

                                        //Update batches as per sequence by BatchCurrentVsInvCountList
                                        int i = 0;
                                        foreach (var bsequence in BatchCurrentVsInvCountList.OrderBy(x => x.Qty))
                                        {
                                            foreach (var batchData in InventCycleBatchs.Where(x => x.IsStockUpdated == false && x.StockBatchId == bsequence.StockBatchMasterId))
                                            {
                                                var StockBatchMaster = stockBatchMasters.FirstOrDefault(x => x.Id == batchData.StockBatchId && x.StockType == "C");

                                                int CountinventoryDiff = (batchData.PastInventory ?? 0) - batchData.InventoryCount;


                                                if (CountinventoryDiff == 0)
                                                {
                                                    InventCycleBatchExtraQtys.Add(new InventCycleBatchExtraQty
                                                    {
                                                        Qty = CountinventoryDiff,
                                                        StockBatchMasterId = StockBatchMaster.Id,
                                                        Sequence = i,
                                                        InventCycleBatchId = batchData.Id,
                                                    });
                                                    batchData.IsStockUpdated = true;
                                                    batchData.UpdateBy = People.PeopleID;
                                                    batchData.UpdatedDate = DateTime.Now;
                                                    context.Entry(batchData).State = EntityState.Modified;

                                                    inventCycleBatchSettlementList.Add(new InventCycleBatchSettlement
                                                    {
                                                        ToInventCycleBatchId = Convert.ToInt32(StockBatchMaster.Id),
                                                        FromInventCycleBatchId = Convert.ToInt32(batchData.StockBatchId),
                                                        Qty = (-1) * CountinventoryDiff,
                                                        CreatedBy = People.PeopleID,
                                                        CreatedDate = DateTime.Now
                                                    });
                                                    i++;
                                                }

                                                if (CountinventoryDiff > 0)
                                                {
                                                    InventCycleBatchExtraQtys.Add(new InventCycleBatchExtraQty
                                                    {
                                                        Qty = CountinventoryDiff,
                                                        StockBatchMasterId = StockBatchMaster.Id,
                                                        Sequence = i,
                                                        InventCycleBatchId = batchData.Id,
                                                    });
                                                    batchData.IsStockUpdated = true;
                                                    batchData.UpdateBy = People.PeopleID;
                                                    batchData.UpdatedDate = DateTime.Now;
                                                    context.Entry(batchData).State = EntityState.Modified;

                                                    bool isupdated = batchMasterManager.UpdateStockInSameBatch(StockBatchMaster.Id, context, People.PeopleID, CountinventoryDiff);
                                                    if (!isupdated)
                                                    {
                                                        result.msg = "Issue in inventory cycle. Please try after some time.";
                                                        return result;
                                                    }

                                                    inventCycleBatchSettlementList.Add(new InventCycleBatchSettlement
                                                    {
                                                        ToInventCycleBatchId = Convert.ToInt32(StockBatchMaster.Id),
                                                        FromInventCycleBatchId = Convert.ToInt32(batchData.StockBatchId),
                                                        Qty = (-1) * CountinventoryDiff,
                                                        CreatedBy = People.PeopleID,
                                                        CreatedDate = DateTime.Now
                                                    });

                                                    i++;
                                                }
                                                else if (CountinventoryDiff < 0)
                                                {
                                                    foreach (var item in InventCycleBatchExtraQtys.Where(x => x.Qty >= 0).OrderBy(x => x.Sequence))
                                                    {
                                                        int ajustqty = (item.Qty - (CountinventoryDiff * (-1))) >= 0 ? (CountinventoryDiff * (-1)) : item.Qty;
                                                        CountinventoryDiff += ajustqty;
                                                        item.Qty -= ajustqty;

                                                        bool isupdated = batchMasterManager.UpdateStockInSameBatch(StockBatchMaster.Id, context, People.PeopleID, ajustqty * (-1));
                                                        if (!isupdated)
                                                        {
                                                            result.msg = "Issue in inventory cycle. Please try after some time.";
                                                            return result;
                                                        }
                                                        inventCycleBatchSettlementList.Add(new InventCycleBatchSettlement
                                                        {
                                                            ToInventCycleBatchId = Convert.ToInt32(StockBatchMaster.Id),
                                                            FromInventCycleBatchId = Convert.ToInt32(batchData.StockBatchId),
                                                            Qty = ajustqty,
                                                            CreatedBy = People.PeopleID,
                                                            CreatedDate = DateTime.Now
                                                        });

                                                        if (CountinventoryDiff == 0)
                                                        {
                                                            batchData.IsStockUpdated = true;
                                                            batchData.UpdateBy = People.PeopleID;
                                                            batchData.UpdatedDate = DateTime.Now;
                                                            context.Entry(batchData).State = EntityState.Modified;
                                                            break;
                                                        }
                                                    }

                                                    //Move stock from V to IR and send it for approval at hq ( Virtual to IRS(manual))
                                                    if (CountinventoryDiff < 0)
                                                    {
                                                        PhysicalStockUpdateRequestDc manualStockUpdateDc = new PhysicalStockUpdateRequestDc
                                                        {
                                                            ItemMultiMRPId = currentstock.ItemMultiMRPId,
                                                            Reason = warehouse.IsPV == true || inventCycle.IsPV ? "Physical Verification Dated:" + inventCycle.CreatedDate.Value.ToString("dd/MM/yyyy") : "Inventory Cycle Dated:" + inventCycle.CreatedDate.Value.ToString("dd/MM/yyyy"),
                                                            StockTransferType = "ManualInventory",
                                                            Qty = (-1) * CountinventoryDiff,
                                                            WarehouseId = currentstock.WarehouseId,
                                                            DestinationStockType = StockTypeTableNames.InventoryReserveStocks,
                                                            SourceStockType = StockTypeTableNames.VirtualStock,
                                                        };
                                                        bool isSuccess = sthelper.TransferBetweenVirtualStockAndPhysicalStocks(manualStockUpdateDc, userid, context, dbContextTransaction);
                                                        if (!isSuccess)
                                                        {
                                                            result.msg = "Issue in inventory cycle. Please try after some time.";
                                                            return result;
                                                        }
                                                        InventCycleBatchSentForApprovalList.Add(new InventCycleBatchSentForApproval
                                                        {
                                                            StockBatchMasterId = batchData.StockBatchId,
                                                            Qty = CountinventoryDiff,
                                                            CreatedBy = People.PeopleID,
                                                            CreatedDate = DateTime.Now,
                                                            IsActive = true,
                                                            IsDeleted = false,
                                                            Status = 0,// Pending
                                                            InventCycleBatchId = batchData.Id
                                                        });
                                                    }
                                                }
                                                else
                                                {
                                                    inventCycleBatchSettlementList.Add(new InventCycleBatchSettlement
                                                    {
                                                        ToInventCycleBatchId = Convert.ToInt32(StockBatchMaster.Id),
                                                        FromInventCycleBatchId = Convert.ToInt32(batchData.StockBatchId),
                                                        Qty = CountinventoryDiff,
                                                        CreatedBy = People.PeopleID,
                                                        CreatedDate = DateTime.Now
                                                    });
                                                }
                                            }
                                        }

                                    }
                                    //Case 3 when batch and stock are not same ( Batch in - ) 
                                    else if (currentstock.CurrentInventory > InventCycleBatchs.Sum(x => x.InventoryCount))
                                    {
                                        //for same value in batch
                                        foreach (var batchData in InventCycleBatchs.Where(x => (x.PastInventory ?? 0) == x.InventoryCount).ToList())
                                        {
                                            batchData.IsStockUpdated = true;
                                            batchData.UpdateBy = People.PeopleID;
                                            batchData.UpdatedDate = DateTime.Now;
                                            context.Entry(batchData).State = EntityState.Modified;
                                        }

                                        //preparelist for orderby 
                                        List<BatchCurrentVsInvCountDc> BatchCurrentVsInvCountList = new List<BatchCurrentVsInvCountDc>();
                                        foreach (var batchData in InventCycleBatchs.Where(x => x.IsStockUpdated == false))
                                        {
                                            var batchmaster = batchmasters.FirstOrDefault(x => x.Id == batchData.BatchMasterId);
                                            if (batchmaster != null && !batchData.IsStockUpdated)
                                            {
                                                var StockBatchMaster = stockBatchMasters.FirstOrDefault(x => x.Id == batchData.StockBatchId && x.StockType == "C");
                                                if (StockBatchMaster == null)
                                                {
                                                    long StockBatchMasterId = batchMasterManager.GetOrCreate(currentstock.ItemMultiMRPId, currentstock.WarehouseId, "C", batchData.BatchMasterId, context, userid);
                                                    StockBatchMaster = context.StockBatchMasters.FirstOrDefault(x => x.Id == StockBatchMasterId && x.StockType == "C");
                                                    batchData.StockBatchId = StockBatchMaster.Id;
                                                    stockBatchMasters.Add(StockBatchMaster);
                                                }
                                                //new check for Batch Mismatch
                                                TextFileLogHelper.TraceLog("StockBatchMaster.Qty:" + StockBatchMaster.Qty);
                                                TextFileLogHelper.TraceLog("StockBatchMaster.Qty:" + currentstock.CurrentInventory);
                                                if (StockBatchMaster.Qty != currentstock.CurrentInventory)
                                                {
                                                    result.msg = "Current Inventory Qty and BatchCode Qty Mismatch!";
                                                    return result;
                                                }
                                                BatchCurrentVsInvCountList.Add(new BatchCurrentVsInvCountDc
                                                {
                                                    StockBatchMasterId = StockBatchMaster.Id,
                                                    Qty = batchData.InventoryCount - ((batchData.PastInventory ?? 0))
                                                });
                                            }
                                        }

                                        //then order by InventoryCount 
                                        List<InventCycleBatchExtraQty> InventCycleBatchExtraQtys = new List<InventCycleBatchExtraQty>();

                                        //Update batches as per sequence
                                        int i = 0;
                                        foreach (var bsequence in BatchCurrentVsInvCountList.OrderBy(x => x.Qty))
                                        {
                                            foreach (var batchData in InventCycleBatchs.Where(x => x.IsStockUpdated == false && x.StockBatchId == bsequence.StockBatchMasterId))
                                            {
                                                var StockBatchMaster = stockBatchMasters.FirstOrDefault(x => x.Id == batchData.StockBatchId && x.StockType == "C");
                                                int CountinventoryDiff = (batchData.PastInventory ?? 0) - batchData.InventoryCount;

                                                if (CountinventoryDiff == 0)
                                                {
                                                    InventCycleBatchExtraQtys.Add(new InventCycleBatchExtraQty
                                                    {
                                                        Qty = CountinventoryDiff,
                                                        StockBatchMasterId = StockBatchMaster.Id,
                                                        Sequence = i,
                                                        InventCycleBatchId = batchData.Id,
                                                    });
                                                    batchData.IsStockUpdated = true;
                                                    batchData.UpdateBy = People.PeopleID;
                                                    batchData.UpdatedDate = DateTime.Now;
                                                    context.Entry(batchData).State = EntityState.Modified;

                                                    inventCycleBatchSettlementList.Add(new InventCycleBatchSettlement
                                                    {
                                                        ToInventCycleBatchId = Convert.ToInt32(StockBatchMaster.Id),
                                                        FromInventCycleBatchId = Convert.ToInt32(batchData.StockBatchId),
                                                        Qty = (-1) * CountinventoryDiff,
                                                        CreatedBy = People.PeopleID,
                                                        CreatedDate = DateTime.Now
                                                    });
                                                    i++;
                                                }
                                                if (CountinventoryDiff > 0)
                                                {
                                                    InventCycleBatchExtraQtys.Add(new InventCycleBatchExtraQty
                                                    {
                                                        Qty = CountinventoryDiff,
                                                        StockBatchMasterId = StockBatchMaster.Id,
                                                        Sequence = i,
                                                        InventCycleBatchId = batchData.Id,
                                                    });
                                                    batchData.IsStockUpdated = true;
                                                    batchData.UpdateBy = People.PeopleID;
                                                    batchData.UpdatedDate = DateTime.Now;
                                                    context.Entry(batchData).State = EntityState.Modified;
                                                    batchMasterManager.UpdateStockInSameBatch(StockBatchMaster.Id, context, People.PeopleID, CountinventoryDiff);

                                                    inventCycleBatchSettlementList.Add(new InventCycleBatchSettlement
                                                    {
                                                        ToInventCycleBatchId = Convert.ToInt32(StockBatchMaster.Id),
                                                        FromInventCycleBatchId = Convert.ToInt32(batchData.StockBatchId),
                                                        Qty = (-1) * CountinventoryDiff,
                                                        CreatedBy = People.PeopleID,
                                                        CreatedDate = DateTime.Now
                                                    });
                                                    i++;
                                                }
                                                else if (CountinventoryDiff < 0)
                                                {
                                                    foreach (var item in InventCycleBatchExtraQtys.Where(x => x.Qty > 0).OrderBy(x => x.Sequence))
                                                    {
                                                        int ajustqty = (item.Qty - (CountinventoryDiff * (-1))) >= 0 ? (CountinventoryDiff * (-1)) : item.Qty;

                                                        CountinventoryDiff += ajustqty;
                                                        item.Qty -= ajustqty;
                                                        batchMasterManager.UpdateStockInSameBatch(StockBatchMaster.Id, context, People.PeopleID, ajustqty * (-1));

                                                        inventCycleBatchSettlementList.Add(new InventCycleBatchSettlement
                                                        {
                                                            ToInventCycleBatchId = Convert.ToInt32(StockBatchMaster.Id),
                                                            FromInventCycleBatchId = Convert.ToInt32(batchData.StockBatchId),
                                                            Qty = CountinventoryDiff,
                                                            CreatedBy = People.PeopleID,
                                                            CreatedDate = DateTime.Now
                                                        });

                                                        if (CountinventoryDiff == 0)
                                                        {
                                                            batchData.IsStockUpdated = true;
                                                            batchData.UpdateBy = People.PeopleID;
                                                            batchData.UpdatedDate = DateTime.Now;
                                                            context.Entry(batchData).State = EntityState.Modified;
                                                            break;
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                        //}
                                        //Move stock from C to IR and send it for approval at hq ( Virtual to IRS(manual))
                                        if (InventCycleBatchExtraQtys != null && InventCycleBatchExtraQtys.Any(x => x.Qty > 0))
                                        {
                                            List<TransferStockDTONew> transferStockList = new List<TransferStockDTONew>();

                                            foreach (var item in InventCycleBatchExtraQtys.Where(x => x.Qty > 0))
                                            {
                                                PhysicalStockUpdateRequestDc manualStockUpdateDc = new PhysicalStockUpdateRequestDc
                                                {
                                                    ItemMultiMRPId = currentstock.ItemMultiMRPId,
                                                    Reason = (warehouse.IsPV == true || inventCycle.IsPV ? "Physical Verification Dated:" : "Inventory Cycle Dated:") + inventCycle.CreatedDate.Value.ToString("dd/MM/yyyy"),
                                                    //"Inventory Cycle Dated:" + inventCycle.CreatedDate.Value.ToString("dd/MM/yyyy"),
                                                    StockTransferType = "ManualInventory",
                                                    Qty = item.Qty,
                                                    WarehouseId = currentstock.WarehouseId,
                                                    DestinationStockType = StockTypeTableNames.InventoryReserveStocks,
                                                    SourceStockType = StockTypeTableNames.CurrentStocks,
                                                };
                                                bool isSuccess = sthelper.TransferBetweenPhysicalStocks(manualStockUpdateDc, userid, context, dbContextTransaction);
                                                if (!isSuccess)
                                                {
                                                    result.msg = "Issue in inventory cycle. Please try after some time.";
                                                    return result;
                                                }

                                                InventCycleBatchSentForApprovalList.Add(new InventCycleBatchSentForApproval
                                                {
                                                    StockBatchMasterId = item.StockBatchMasterId,
                                                    Qty = item.Qty,
                                                    CreatedBy = People.PeopleID,
                                                    CreatedDate = DateTime.Now,
                                                    IsActive = true,
                                                    IsDeleted = false,
                                                    Status = 0,// Pending,
                                                    InventCycleBatchId = item.InventCycleBatchId
                                                });

                                            }
                                        }

                                    }

                                    if (InventCycleBatchSentForApprovalList != null && InventCycleBatchSentForApprovalList.Any())
                                    {
                                        context.InventCycleBatchSentForApprovals.AddRange(InventCycleBatchSentForApprovalList);

                                        var InventCycleBatchSentForApprovalListd = InventCycleBatchSentForApprovalList.Select(s => s.InventCycleBatchId).ToList();
                                        var InventCycleBatchss = InventCycleBatchs.Where(x => InventCycleBatchSentForApprovalListd.Contains(x.Id)).ToList();
                                        foreach (var r in InventCycleBatchss)
                                        {
                                            r.IsStockUpdated = false;
                                            context.Entry(r).State = EntityState.Modified;
                                        }

                                    }
                                    if (inventCycleBatchSettlementList != null && inventCycleBatchSettlementList.Any())
                                    {
                                        context.InventCycleBatchSettlement.AddRange(inventCycleBatchSettlementList);
                                    }
                                    var inventoryApprovaldata = inventoryapprovals.FirstOrDefault(x => x.InventoryCycleId == inventCycle.Id);

                                    string query = "select VerifiedComment from InventoryVerifiedStatus where VerifiedStatus = " + inventoryApprovaldata.VerifierStatus;
                                    inventoryApprovaldata.Comment = context.Database.SqlQuery<string>(query).FirstOrDefault();

                                    inventCycle.IsWarehouseApproved = 1;

                                    inventCycle.WarehouseApproveId = userid;
                                    inventCycle.UpadtedBy = People.DisplayName;
                                    inventCycle.UpdatedDate = DateTime.Now;
                                    inventCycle.VerifierStatus = inventoryApprovaldata.VerifierStatus;
                                    inventCycle.Comment = inventoryApprovaldata.Comment;
                                    inventCycle.WarehouseComment = inventoryApprovaldata.Comment;
                                    context.Entry(inventCycle).State = EntityState.Modified;
                                    if (inventCycle.IsApproved == true)
                                    {
                                        //
                                        var inventCycleDataHistoryData = context.InventCycleDataHistoryDB.Where(x => x.InventoryCycleId == inventCycle.Id && x.IsDeleted == false && x.IsActive).FirstOrDefault();
                                        if (inventCycleDataHistoryData != null)
                                        {
                                            inventCycleDataHistoryData.IsActive = true;
                                            inventCycleDataHistoryData.ModifiedBy = userid;
                                            inventCycleDataHistoryData.ModifiedDate = DateTime.Now;
                                            if (inventoryApprovaldata.IsHQ == false)
                                            {
                                                inventCycleDataHistoryData.VerifiedBy = People.DisplayName;
                                                inventCycleDataHistoryData.VerifiedTimestamp = DateTime.Now;
                                            }
                                            else
                                            {
                                                inventCycleDataHistoryData.ApprovedBy = People.DisplayName;
                                                inventCycleDataHistoryData.ApprovedTimestamp = DateTime.Now;
                                            }

                                            inventCycleDataHistoryData.Status = inventoryApprovaldata.IsHQ == false ? "Warehouse Approved" : "HQ Approved";
                                            context.Entry(inventCycleDataHistoryData).State = EntityState.Modified;
                                        }
                                        else
                                        {
                                            InventCycleDataHistory list = new InventCycleDataHistory();
                                            list.InventoryCycleId = inventCycle.Id;
                                            list.InventoryCount = inventCycle.InventCycleBatchs.Sum(x => x.InventoryCount);
                                            list.Status = inventoryApprovaldata.IsHQ == false ? "Warehouse Approved" : "HQ Approved";
                                            list.CreatedDate = DateTime.Now;
                                            list.CreatedBy = People.DisplayName;
                                            if (inventoryApprovaldata.IsHQ == false)
                                            {
                                                list.VerifiedBy = People.DisplayName;
                                                list.VerifiedTimestamp = DateTime.Now;
                                            }
                                            else
                                            {
                                                list.ApprovedBy = People.DisplayName;
                                                list.ApprovedTimestamp = DateTime.Now;
                                            }
                                            list.IsActive = true;

                                            context.Entry(list).State = EntityState.Added;
                                        }
                                        //
                                    }
                                    else
                                    {
                                        var inventCycleDataaHistoryData = context.InventCycleDataHistoryDB.Where(x => x.InventoryCycleId == inventCycle.Id && x.IsDeleted == false && x.IsActive).FirstOrDefault();
                                        if (inventCycleDataaHistoryData != null)
                                        {
                                            inventCycleDataaHistoryData.IsActive = true;
                                            inventCycleDataaHistoryData.ModifiedBy = userid;
                                            inventCycleDataaHistoryData.ModifiedDate = DateTime.Now;
                                            inventCycleDataaHistoryData.Status = inventoryApprovaldata.IsHQ == false ? "Warehouse Approved" : "HQ Approved";
                                            //list.CreatedDate = DateTime.Now;
                                            //list.CreatedBy = People.DisplayName;
                                            if (inventoryApprovaldata.IsHQ == false)
                                            {
                                                inventCycleDataaHistoryData.VerifiedBy = People.DisplayName;
                                                inventCycleDataaHistoryData.VerifiedTimestamp = DateTime.Now;
                                            }
                                            else
                                            {
                                                inventCycleDataaHistoryData.ApprovedBy = People.DisplayName;
                                                inventCycleDataaHistoryData.ApprovedTimestamp = DateTime.Now;
                                            }
                                            context.Entry(inventCycleDataaHistoryData).State = EntityState.Modified;
                                        }
                                        else
                                        {
                                            InventCycleDataHistory list = new InventCycleDataHistory();
                                            list.InventoryCycleId = inventCycle.Id;
                                            list.InventoryCount = inventCycle.InventCycleBatchs.Sum(x => x.InventoryCount);
                                            list.Status = inventoryApprovaldata.IsHQ == false ? "Warehouse Approved" : "HQ Approved";
                                            list.CreatedDate = DateTime.Now;
                                            list.CreatedBy = People.DisplayName;
                                            if (inventoryApprovaldata.IsHQ == false)
                                            {
                                                list.VerifiedBy = People.DisplayName;
                                                list.VerifiedTimestamp = DateTime.Now;
                                            }
                                            else
                                            {
                                                list.ApprovedBy = People.DisplayName;
                                                list.ApprovedTimestamp = DateTime.Now;
                                            }
                                            list.IsActive = true;

                                            context.Entry(list).State = EntityState.Added;
                                        }
                                    }
                                    result.Status = context.Commit() > 0;
                                }
                                else if (inventCycle.IsWarehouseApproved == 1 && !inventCycle.IsApproved)
                                {
                                    var InventcyIds = InventCycleBatchs.Where(x => x.IsStockUpdated == false).Select(x => x.Id).ToList();
                                    var InventCycleBatchSentForApprovalLists = context.InventCycleBatchSentForApprovals.Where(x => InventcyIds.Contains(x.InventCycleBatchId) && x.IsDeleted == false).ToList();
                                    foreach (var batchData in InventCycleBatchs.Where(x => x.IsStockUpdated == false))
                                    {
                                        //IR to C
                                        if (InventCycleBatchSentForApprovalLists != null && InventCycleBatchSentForApprovalLists.Any(x => x.InventCycleBatchId == batchData.Id && x.Qty < 0))
                                        {
                                            var InventCycleBatchSentForApproval = InventCycleBatchSentForApprovalLists.FirstOrDefault(x => x.InventCycleBatchId == batchData.Id);
                                            PhysicalStockUpdateRequestDc manualStockUpdateDc = new PhysicalStockUpdateRequestDc
                                            {
                                                ItemMultiMRPId = currentstock.ItemMultiMRPId,
                                                Reason = (warehouse.IsPV == true || inventCycle.IsPV ? "Physical Verification Dated:" : "Inventory Cycle Dated:") + inventCycle.CreatedDate.Value.ToString("dd/MM/yyyy"),
                                                StockTransferType = "ManualInventory",
                                                Qty = InventCycleBatchSentForApproval.Qty * (-1),
                                                WarehouseId = currentstock.WarehouseId,
                                                DestinationStockType = StockTypeTableNames.CurrentStocks,
                                                SourceStockType = StockTypeTableNames.InventoryReserveStocks,
                                            };
                                            bool isSuccess = sthelper.TransferBetweenPhysicalStocks(manualStockUpdateDc, userid, context, dbContextTransaction);
                                            if (!isSuccess)
                                            {
                                                result.msg = "Issue in inventory cycle. Please try after some time.";
                                                return result;
                                            }
                                            batchMasterManager.UpdateStockInSameBatch(InventCycleBatchSentForApproval.StockBatchMasterId, context, People.PeopleID, InventCycleBatchSentForApproval.Qty);

                                        }
                                        else if (InventCycleBatchSentForApprovalLists != null && InventCycleBatchSentForApprovalLists.Any(x => x.InventCycleBatchId == batchData.Id && x.Qty > 0))
                                        {  //IR To V 
                                            var InventCycleBatchSentForApproval = InventCycleBatchSentForApprovalLists.FirstOrDefault(x => x.InventCycleBatchId == batchData.Id);

                                            PhysicalStockUpdateRequestDc manualStockUpdateDc = new PhysicalStockUpdateRequestDc
                                            {
                                                ItemMultiMRPId = currentstock.ItemMultiMRPId,
                                                Reason = (warehouse.IsPV == true || inventCycle.IsPV ? "Physical Verification Dated:" : "Inventory Cycle Dated:") + inventCycle.CreatedDate.Value.ToString("dd/MM/yyyy"),
                                                StockTransferType = "ManualInventory",
                                                Qty = InventCycleBatchSentForApproval.Qty,
                                                WarehouseId = currentstock.WarehouseId,
                                                DestinationStockType = StockTypeTableNames.VirtualStock,
                                                SourceStockType = StockTypeTableNames.InventoryReserveStocks,
                                            };
                                            bool isSuccess = sthelper.TransferBetweenVirtualStockAndPhysicalStocks(manualStockUpdateDc, userid, context, dbContextTransaction);
                                            if (!isSuccess)
                                            {
                                                result.msg = "Issue in inventory cycle. Please try after some time.";
                                                return result;
                                            }
                                        }

                                        batchData.IsStockUpdated = true;
                                        batchData.UpdateBy = People.PeopleID;
                                        batchData.UpdatedDate = DateTime.Now;
                                        context.Entry(batchData).State = EntityState.Modified;
                                    }
                                    var inventoryApprovaldata = inventoryapprovals.FirstOrDefault(x => x.InventoryCycleId == inventCycle.Id);
                                    if (inventCycleBatchSettlementList != null && inventCycleBatchSettlementList.Any())
                                    {
                                        context.InventCycleBatchSettlement.AddRange(inventCycleBatchSettlementList);
                                    }
                                    string query = "select VerifiedComment from InventoryVerifiedStatus where VerifiedStatus = " + inventoryApprovaldata.VerifierStatus;
                                    inventoryApprovaldata.Comment = context.Database.SqlQuery<string>(query).FirstOrDefault();
                                    inventCycle.IsApproved = true;
                                    inventCycle.HQApprovedId = People.PeopleID;
                                    inventCycle.HQUpdatedDate = DateTime.Now;
                                    inventCycle.HQVerifierStatus = inventoryApprovaldata.VerifierStatus; //inventoryapprovals.FirstOrDefault(x => x.InventoryCycleId == inventCycle.Id).VerifierStatus;
                                    inventCycle.HQComment = inventoryApprovaldata.Comment;
                                    context.Entry(inventCycle).State = EntityState.Modified;
                                    //
                                    var inventCycleDataHistoryData = context.InventCycleDataHistoryDB.Where(x => x.InventoryCycleId == inventCycle.Id && x.IsDeleted == false && x.IsActive).FirstOrDefault();
                                    if (inventCycleDataHistoryData != null)
                                    {
                                        inventCycleDataHistoryData.IsActive = true;
                                        inventCycleDataHistoryData.ModifiedBy = userid;
                                        inventCycleDataHistoryData.ModifiedDate = DateTime.Now;
                                        if (inventoryApprovaldata.IsHQ == false)
                                        {
                                            inventCycleDataHistoryData.VerifiedBy = People.DisplayName;
                                            inventCycleDataHistoryData.VerifiedTimestamp = DateTime.Now;
                                        }
                                        else
                                        {
                                            inventCycleDataHistoryData.ApprovedBy = People.DisplayName;
                                            inventCycleDataHistoryData.ApprovedTimestamp = DateTime.Now;
                                        }
                                        inventCycleDataHistoryData.Status = inventoryApprovaldata.IsHQ == false ? "Warehouse Approved" : "HQ Approved";
                                        context.Entry(inventCycleDataHistoryData).State = EntityState.Modified;
                                    }
                                    else
                                    {
                                        InventCycleDataHistory list = new InventCycleDataHistory();
                                        list.InventoryCycleId = inventCycle.Id;
                                        list.InventoryCount = inventCycle.InventCycleBatchs.Sum(x => x.InventoryCount);
                                        list.Status = inventoryApprovaldata.IsHQ == false ? "Warehouse Approved" : "HQ Approved";
                                        list.CreatedDate = DateTime.Now;
                                        list.CreatedBy = People.DisplayName;
                                        if (inventoryApprovaldata.IsHQ == false)
                                        {
                                            list.VerifiedBy = People.DisplayName;
                                            list.VerifiedTimestamp = DateTime.Now;
                                        }
                                        else
                                        {
                                            list.ApprovedBy = People.DisplayName;
                                            list.ApprovedTimestamp = DateTime.Now;
                                        }
                                        list.IsActive = true;

                                        context.Entry(list).State = EntityState.Added;
                                    }
                                    //
                                    result.Status = context.Commit() > 0;

                                }
                                else
                                {
                                    result.msg = "Inventory cycle not updated.";
                                    return result;
                                }
                            }
                        }

                        dbContextTransaction.Complete();
                        if (result.Status)
                            result.msg = "Inventory cycle updated successfully.";
                        else
                            result.msg = "Inventory cycle not updated.";
                    }
                    else
                    {
                        result.msg = "Already Approved!";
                    }
                }
            }
            return result;
        }

        [HttpGet]
        [Route("GetInventoryCycleHistory")]
        public async Task<List<InventCycleHistoryListDC>> GetInventoryCycleHistory(int InventCycleId)
        {
            using (var context = new AuthContext())
            {
                List<InventCycleHistoryListDC> inventCycleHistoryListDC = new List<InventCycleHistoryListDC>();
                var inventCycleIdParam = new SqlParameter("@InventCycleId", InventCycleId);
                inventCycleHistoryListDC = await context.Database.SqlQuery<InventCycleHistoryListDC>("GetInventoryCyclesHistory @InventCycleId", inventCycleIdParam).ToListAsync();
                return inventCycleHistoryListDC;
            }
        }


        [HttpGet]
        [Route("GetInventoryCycleBatchHistory")]
        public async Task<List<InventCycleBatchHistoryList>> GetInventoryCycleBatchHistory(long BatchMasterId)
        {
            List<InventCycleBatchHistoryList> inventCycleBatchHistoryList = new List<InventCycleBatchHistoryList>();
            using (var context = new AuthContext())
            {
                var BatchMasterIdParam = new SqlParameter("@BatchMasterId", BatchMasterId);

                inventCycleBatchHistoryList = await context.Database.SqlQuery<InventCycleBatchHistoryList>("GetInventCycleBatchHistory @BatchMasterId", BatchMasterIdParam).ToListAsync();
                return inventCycleBatchHistoryList;
            }
        }


        //var batchmaster = batchmasters.FirstOrDefault(x => x.Id == batchData.BatchMasterId);
        //if (batchmaster != null && !batchData.IsStockUpdated)
        //{
        //    var StockBatchMaster = stockBatchMasters.FirstOrDefault(x => x.Id == batchData.StockBatchId && x.StockType == "C");
        //    if (StockBatchMaster == null)
        //    {
        //        long StockBatchMasterId = batchMasterManager.GetOrCreate(currentstock.ItemMultiMRPId, currentstock.WarehouseId, "C", batchData.BatchMasterId, context, userid);
        //        StockBatchMaster = context.StockBatchMasters.FirstOrDefault(x => x.Id == StockBatchMasterId && x.StockType == "C");
        //    }
        //    int CountinventoryDiff = StockBatchMaster.Qty - batchData.InventoryCount;

        //    if (CountinventoryDiff != 0 && CountinventoryDiff < 0)
        //    {
        //        // Virtual to IRS (manual)
        //        List<ManualStockUpdateRequestDc> Manuallist = new List<ManualStockUpdateRequestDc>();
        //        Manuallist.Add(new ManualStockUpdateRequestDc
        //        {
        //            Qty = CountinventoryDiff,
        //            ItemMultiMRPId = currentstock.ItemMultiMRPId,
        //            DestinationStockType = StockTypeTableNames.InventoryReserveStocks,
        //            SourceStockType = StockTypeTableNames.VirtualStock,
        //            WarehouseId = currentstock.WarehouseId,
        //            dBatchMasterID = batchData.BatchMasterId,
        //            StockTransferType = "ManualInventory",
        //            Reason = "Inventory Cycle Dated:" + inventCycle.CreatedDate.Value.ToString("dd/MM/yyyy"),
        //        }); ;
        //        bool isSucess = batchMasterManager.MoveStock(Manuallist, context, userid);

        //        PhysicalStockUpdateRequestDc manualStockUpdateDc = new PhysicalStockUpdateRequestDc
        //        {
        //            ItemMultiMRPId = currentstock.ItemMultiMRPId,
        //            Reason = "Inventory Cycle Dated:" + inventCycle.CreatedDate.Value.ToString("dd/MM/yyyy"),
        //            StockTransferType = "ManualInventory",
        //            Qty = CountinventoryDiff < 0 ? (-1) * CountinventoryDiff : CountinventoryDiff,
        //            WarehouseId = currentstock.WarehouseId,
        //            DestinationStockType = StockTypeTableNames.InventoryReserveStocks,
        //            SourceStockType = StockTypeTableNames.VirtualStock,
        //        };
        //        bool isSuccess = sthelper.TransferBetweenVirtualStockAndPhysicalStocks(manualStockUpdateDc, userid, context, dbContextTransaction);

        //    }
        //    else if (CountinventoryDiff != 0 && CountinventoryDiff > 0)
        //    {
        //        // Virtual to IRS (manual)
        //        List<ManualStockUpdateRequestDc> Manuallist = new List<ManualStockUpdateRequestDc>();
        //        Manuallist.Add(new ManualStockUpdateRequestDc
        //        {
        //            Qty = CountinventoryDiff,
        //            ItemMultiMRPId = currentstock.ItemMultiMRPId,
        //            DestinationStockType = StockTypeTableNames.InventoryReserveStocks,
        //            SourceStockType = StockTypeTableNames.VirtualStock,
        //            WarehouseId = currentstock.WarehouseId,
        //            dBatchMasterID = batchData.BatchMasterId,
        //            StockTransferType = "ManualInventory",
        //            Reason = "Inventory Cycle Dated:" + inventCycle.CreatedDate.Value.ToString("dd/MM/yyyy"),
        //        }); ;
        //        bool isSucess = batchMasterManager.MoveStock(Manuallist, context, userid);

        //        PhysicalStockUpdateRequestDc manualStockUpdateDc = new PhysicalStockUpdateRequestDc
        //        {
        //            ItemMultiMRPId = currentstock.ItemMultiMRPId,
        //            Reason = "Inventory Cycle Dated:" + inventCycle.CreatedDate.Value.ToString("dd/MM/yyyy"),
        //            StockTransferType = "ManualInventory",
        //            Qty = CountinventoryDiff < 0 ? (-1) * CountinventoryDiff : CountinventoryDiff,
        //            WarehouseId = currentstock.WarehouseId,
        //            DestinationStockType = StockTypeTableNames.InventoryReserveStocks,
        //            SourceStockType = StockTypeTableNames.VirtualStock,
        //        };
        //        bool isSuccess = sthelper.TransferBetweenVirtualStockAndPhysicalStocks(manualStockUpdateDc, userid, context, dbContextTransaction);
        //    }
        //    else if (CountinventoryDiff == 0)
        //    {
        //        batchData.IsStockUpdated = true;
        //        batchData.UpdateBy = People.PeopleID;
        //        batchData.UpdatedDate = DateTime.Now;
        //        context.Entry(batchData).State = EntityState.Modified;
        //    }
        //}
        #region old code
        private response CurrentStockInventoryAddandSubold(List<Inventoryapproval> inventoryapprovals, int userid)
        {
            response result = new response { msg = "", Status = false };
            People People = null;
            List<int> Invcycleids = null;
            List<InventCycle> inventCyclelist = null;
            List<int> inventCycleMultiMrpids = null;
            int warehouseid = 0;
            List<CurrentStock> currentstockList = null;
            List<StockBatchMaster> stockBatchMasters = new List<StockBatchMaster>();
            TransactionOptions option = new TransactionOptions();
            option.IsolationLevel = System.Transactions.IsolationLevel.RepeatableRead;
            option.Timeout = TimeSpan.FromSeconds(90);
            using (var dbContextTransaction = new TransactionScope(TransactionScopeOption.Required, option))
            {
                using (var context = new AuthContext())
                {
                    Invcycleids = inventoryapprovals.Select(x => x.InventoryCycleId).ToList();
                    inventCyclelist = context.InventCycleDb.Where(x => Invcycleids.Contains(x.Id) && !x.IsApproved).Include(x => x.InventCycleBatchs).ToList();
                    if (inventCyclelist != null && inventCyclelist.Any())
                    {
                        inventCycleMultiMrpids = inventCyclelist.Select(x => x.ItemMultiMrpId).ToList();
                        warehouseid = inventCyclelist[0].WarehouseId;
                        var warehouse = context.Warehouses.FirstOrDefault(x => x.WarehouseId == warehouseid);
                        AngularJSAuthentication.BatchManager.Helpers.ElasticBatchHelper elasticBatchHelper = new AngularJSAuthentication.BatchManager.Helpers.ElasticBatchHelper();
                        //if (!warehouse.IsStopCurrentStockTrans || elasticBatchHelper.IsAnyPendingDocExists(inventCycleMultiMrpids, warehouseid))
                        if (!warehouse.IsStopCurrentStockTrans)
                        {
                            result.msg = "Please stop transactions of this warehouse before inventory count";
                            return result;
                        }

                        People = context.Peoples.Where(x => x.PeopleID == userid).SingleOrDefault();

                        currentstockList = context.DbCurrentStock.Where(c => inventCycleMultiMrpids.Contains(c.ItemMultiMRPId) && c.Deleted == false && c.WarehouseId == warehouseid).ToList();
                        var cStockIds = currentstockList.Select(x => Convert.ToInt64(x.StockId)).ToList();
                        stockBatchMasters = context.StockBatchMasters.Where(x => cStockIds.Contains(x.StockId) && x.StockType == "C").ToList();
                        var batchmasterids = inventCyclelist.SelectMany(x => x.InventCycleBatchs).Select(x => x.BatchMasterId).Distinct().ToList();
                        var batchmasters = context.BatchMasters.Where(x => batchmasterids.Contains(x.Id)).ToList();
                        var stockTxnTypes = context.StockTxnTypeMasters.Where(x => x.StockTxnType.Contains("InventoryCycle"));
                        var stockTxnTypeIn = stockTxnTypes.FirstOrDefault(x => x.StockTxnType == "InventoryCycleIn");
                        var stockTxnTypeOut = stockTxnTypes.FirstOrDefault(x => x.StockTxnType == "InventoryCycleOut");
                        StockTransactionHelper sthelper = new StockTransactionHelper();
                        BatchMasterManager batchMasterManager = new BatchMasterManager();

                        foreach (var inventCycle in inventCyclelist)
                        {

                            var InventCycleBatchs = inventCycle.InventCycleBatchs;
                            if (InventCycleBatchs != null && InventCycleBatchs.Any())
                            {
                                var currentstock = currentstockList.FirstOrDefault(x => x.ItemMultiMRPId == inventCycle.ItemMultiMrpId && x.WarehouseId == inventCycle.WarehouseId);

                                int batchinventory = 0;
                                int currentInventory = currentstock.CurrentInventory;
                                bool IsInventoryUpdate = false;
                                if (inventCycle.IsWarehouseApproved == 1 || inventCycle.PastInventory == inventCycle.InventoryCount)
                                {
                                    IsInventoryUpdate = true;
                                }

                                List<TransferStockDTONew> transferStockList = new List<TransferStockDTONew>();
                                foreach (var batchData in InventCycleBatchs)
                                {
                                    transferStockList = new List<TransferStockDTONew>();
                                    batchinventory = 0;
                                    var batchmaster = batchmasters.FirstOrDefault(x => x.Id == batchData.BatchMasterId);

                                    if (batchmaster != null)
                                    {
                                        var stock = stockBatchMasters.FirstOrDefault(x => x.Id == batchData.StockBatchId && x.StockType == "C");

                                        if (batchData.PastInventory.HasValue)
                                        {
                                            batchinventory = (-1) * (batchData.PastInventory.Value + (batchData.Rtp ?? 0) - (batchData.InventoryCount));
                                        }

                                        if (batchinventory < 0 && !batchData.IsStockUpdated)
                                        {


                                            transferStockList.Add(new TransferStockDTONew
                                            {
                                                ItemMultiMRPId = currentstock.ItemMultiMRPId,
                                                ItemMultiMRPIdTrans = currentstock.ItemMultiMRPId,
                                                StockBatchMasterId = stock.Id,
                                                Qty = (-1) * batchinventory,
                                                WarehouseId = currentstock.WarehouseId,
                                                BatchMasterId = 0
                                            });
                                            bool isBatchSuccess = batchMasterManager.MoveStock(transferStockList, context, userid, "IR");

                                            if (isBatchSuccess)
                                            {
                                                PhysicalStockUpdateRequestDc manualStockUpdateDc = new PhysicalStockUpdateRequestDc
                                                {
                                                    ItemMultiMRPId = currentstock.ItemMultiMRPId,
                                                    Reason = "Inventory Cycle Dated:" + inventCycle.CreatedDate.Value.ToString("dd/MM/yyyy"),
                                                    StockTransferType = "ManualInventory",
                                                    Qty = (-1) * batchinventory,
                                                    WarehouseId = currentstock.WarehouseId,
                                                    DestinationStockType = StockTypeTableNames.InventoryReserveStocks,
                                                    SourceStockType = StockTypeTableNames.CurrentStocks,
                                                };

                                                bool isSuccess = sthelper.TransferBetweenPhysicalStocks(manualStockUpdateDc, userid, context, dbContextTransaction);

                                                if (isSuccess)
                                                {

                                                    batchData.IsStockUpdated = true;
                                                    context.Entry(batchData).State = EntityState.Modified;
                                                    context.Commit();
                                                }
                                                else
                                                {
                                                    result.msg = "Issue in inventory cycle. Please try after some time.";
                                                    return result;
                                                }
                                            }
                                            else
                                            {
                                                result.msg = "Issue in inventory cycle. Please try after some time.";
                                                return result;
                                            }
                                        }
                                        else if (IsInventoryUpdate && batchinventory != 0 && !batchData.IsStockUpdated)
                                        {
                                            // Virtual to IRS (manual)
                                            List<ManualStockUpdateRequestDc> Manuallist = new List<ManualStockUpdateRequestDc>();
                                            Manuallist.Add(new ManualStockUpdateRequestDc
                                            {
                                                Qty = batchinventory,
                                                ItemMultiMRPId = currentstock.ItemMultiMRPId,
                                                DestinationStockType = StockTypeTableNames.InventoryReserveStocks,
                                                SourceStockType = StockTypeTableNames.VirtualStock,
                                                WarehouseId = currentstock.WarehouseId,
                                                dBatchMasterID = batchData.BatchMasterId,
                                                StockTransferType = "ManualInventory",
                                                Reason = "Inventory Cycle Dated:" + inventCycle.CreatedDate.Value.ToString("dd/MM/yyyy"),
                                            }); ;
                                            bool isSucess = batchMasterManager.MoveStock(Manuallist, context, userid);

                                            bool ssresult = sthelper.ManualStockUpdate(Manuallist, userid, context, dbContextTransaction);

                                            //IRS to Currentstock
                                            var StockBatchMasterId = batchMasterManager.GetOrCreate(currentstock.ItemMultiMRPId, currentstock.WarehouseId, "IR", batchData.BatchMasterId, context, userid);
                                            transferStockList.Add(new TransferStockDTONew
                                            {
                                                ItemMultiMRPId = currentstock.ItemMultiMRPId,
                                                ItemMultiMRPIdTrans = currentstock.ItemMultiMRPId,
                                                StockBatchMasterId = StockBatchMasterId,
                                                Qty = batchinventory,
                                                WarehouseId = currentstock.WarehouseId,
                                                BatchMasterId = 0
                                            });
                                            bool isBatchSuccess = batchMasterManager.MoveStock(transferStockList, context, userid, "C");

                                            var CurrentStockBatchMasterId = batchMasterManager.GetOrCreate(currentstock.ItemMultiMRPId, currentstock.WarehouseId, "C", batchData.BatchMasterId, context, userid);

                                            if (isBatchSuccess)
                                            {
                                                PhysicalStockUpdateRequestDc manualStockUpdateDc = new PhysicalStockUpdateRequestDc
                                                {
                                                    ItemMultiMRPId = currentstock.ItemMultiMRPId,
                                                    Reason = "Inventory Cycle Dated:" + inventCycle.CreatedDate.Value.ToString("dd/MM/yyyy"),
                                                    StockTransferType = "ManualInventory",
                                                    Qty = batchinventory,
                                                    WarehouseId = currentstock.WarehouseId,
                                                    DestinationStockType = StockTypeTableNames.CurrentStocks,
                                                    SourceStockType = StockTypeTableNames.InventoryReserveStocks,
                                                };

                                                bool isSuccess = sthelper.TransferBetweenPhysicalStocks(manualStockUpdateDc, userid, context, dbContextTransaction);

                                                if (isSuccess)
                                                {
                                                    batchData.IsStockUpdated = true;
                                                    context.Entry(batchData).State = EntityState.Modified;
                                                    context.Commit();
                                                }
                                                else
                                                {
                                                    result.msg = "Issue in inventory cycle. Please try after some time.";
                                                    return result;
                                                }
                                            }
                                            else
                                            {
                                                result.msg = "Issue in inventory cycle. Please try after some time.";
                                                return result;
                                            }
                                            batchData.StockBatchId = CurrentStockBatchMasterId;

                                            batchData.IsStockUpdated = true;
                                            context.Entry(batchData).State = EntityState.Modified;
                                            context.Commit();

                                        }
                                    }
                                }


                                inventCycle.IsWarehouseApproved = 1;
                                var inventCycleDataHistoryData = context.InventCycleDataHistoryDB.Where(x => x.InventoryCycleId == inventCycle.Id && x.IsDeleted == false && x.IsActive).FirstOrDefault();
                                if (inventCycleDataHistoryData != null)
                                {
                                    inventCycleDataHistoryData.IsActive = true;
                                    inventCycleDataHistoryData.ModifiedBy = userid;
                                    inventCycleDataHistoryData.ModifiedDate = DateTime.Now;
                                    inventCycleDataHistoryData.Status = "Warehouse Approved";
                                    context.Entry(inventCycleDataHistoryData).State = EntityState.Modified;
                                }
                                if (inventCycle.IsWarehouseApproved == 0)
                                    inventCycle.WarehouseApproveId = userid;
                                var vQty = InventCycleBatchs.Sum(x => x.PastInventory.Value - x.InventoryCount);

                                if (inventCycle.IsWarehouseApproved == 1 && IsInventoryUpdate && vQty != 0)
                                {
                                    inventCycle.IsApproved = true;
                                    PhysicalStockUpdateRequestDc manualStockUpdateDc = new PhysicalStockUpdateRequestDc
                                    {
                                        ItemMultiMRPId = currentstock.ItemMultiMRPId,
                                        Reason = "Inventory Cycle Dated:" + inventCycle.CreatedDate.Value.ToString("dd/MM/yyyy"),
                                        StockTransferType = "ManualInventory",
                                        Qty = vQty < 0 ? (-1) * vQty : vQty,
                                        WarehouseId = currentstock.WarehouseId,
                                        DestinationStockType = StockTypeTableNames.VirtualStock,
                                        SourceStockType = StockTypeTableNames.InventoryReserveStocks,
                                    };

                                    bool isSuccess = sthelper.TransferBetweenVirtualStockAndPhysicalStocks(manualStockUpdateDc, userid, context, dbContextTransaction);

                                }

                                inventCycle.CurrentInventory = currentstock.CurrentInventory;
                                inventCycle.UpadtedBy = People.DisplayName;
                                inventCycle.UpdatedDate = DateTime.Now;
                                context.Entry(inventCycle).State = EntityState.Modified;
                            }
                        }

                        result.Status = context.Commit() > 0;
                        dbContextTransaction.Complete();
                        if (result.Status)
                            result.msg = "Inventory cycle updated successfully.";
                        else
                            result.msg = "Inventory cycle not updated.";
                    }
                }
            }

            return result;

        }
        #endregion
        [Route("PastInventoryUpdate")]
        [HttpGet]
        public dynamic pastinventory()
        {


            try
            {
                XSSFWorkbook hssfwb;
                using (FileStream file = new FileStream(@"c:\newInventoryupdate.xlsx", FileMode.Open, FileAccess.Read))
                {
                    hssfwb = new XSSFWorkbook(file);
                }


                ISheet sheet = hssfwb.GetSheet("Sheet1");
                DataTable dt = new DataTable(sheet.SheetName);
                IRow headerRow = sheet.GetRow(0);
                foreach (ICell headerCell in headerRow)
                {
                    dt.Columns.Add(headerCell.ToString());
                }
                int rowIndex = 0;
                foreach (IRow row in sheet)
                {
                    // skip header row
                    if (rowIndex++ == 0) continue;
                    DataRow dataRow = dt.NewRow();
                    dataRow.ItemArray = row.Cells.Select(c => c.ToString()).ToArray();
                    dt.Rows.Add(dataRow);
                }


                DataTable dts = new DataTable();
                List<object> jj = new List<object>();



                using (var context = new AuthContext())
                {
                    foreach (DataRow row in dt.Rows)
                    {

                        int id = Convert.ToInt32(row.ItemArray[0]);
                        //string skcode = Convert.ToString(row.ItemArray[3]);
                        int pastinvent = Convert.ToInt32(row.ItemArray[10]);

                        InventCycle invent = context.InventCycleDb.Where(x => x.Id == id).FirstOrDefault();
                        invent.PastInventory = pastinvent;
                        context.Entry(invent).State = EntityState.Modified;
                        context.Commit();


                    }
                }
            }
            catch (Exception ex)
            {
                return false;
            }

            return true;



        }


        [Route("GetWarehoueAll")]
        [HttpGet]
        public dynamic GetWarehoueAll()
        {
            using (AuthContext db = new AuthContext())
            {
                var identity = User.Identity as ClaimsIdentity;
                int userid = 0;
                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                    userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

                MongoDbHelper<InventoryWarehouse> mongoDbHelper = new MongoDbHelper<InventoryWarehouse>();
                InventoryWarehouse inventoryWarehouse = new InventoryWarehouse();

                var IsPVProcessData = db.CompanyDetailsDB.Where(x => x.IsActive).OrderByDescending(x => x.Id).FirstOrDefault();

                var query = " select  w.WarehouseName,w.WarehouseId,w.IsStopCurrentStockTrans,IsDeliveryOptimizationEnabled,IsLocationEnabled,w.IsPV,w.TransactionType  from Warehouses w join " +
                                    "  (select  w.WarehouseId from Warehouses w inner join GMWarehouseProgresses b on w.WarehouseId = b.WarehouseID and b.IsLaunched = 1 " +
                                    "  and w.active = 1 and w.Deleted = 0 and w.IsKPP = 0 and ( w.WarehouseId =67 or w.CityName not like '%test%')) x on x.WarehouseId = w.WarehouseId " +
                                    "  join WarehousePermissions wp on wp.WarehouseId = w.WarehouseId and wp.PeopleID =" + userid + "  and wp.IsActive = 1";
                var list = db.Database.SqlQuery<warehouesDTO>(query).ToList();
                foreach (var item in list)
                {
                    item.IsPVProcess = IsPVProcessData.IsPVProcess;
                    var searchPredicate = PredicateBuilder.New<InventoryWarehouse>(x => x.IsDeleted == false && x.IsInventory && x.WarehouseId == item.WarehouseId);
                    inventoryWarehouse = mongoDbHelper.Select(searchPredicate).FirstOrDefault();
                    if (inventoryWarehouse != null)
                    {
                        item.IsInventory = inventoryWarehouse.IsInventory;
                    }
                }

                return list;
            }
        }


        [Route("InsertClearanceNonSaleablePrepareItem")]
        [HttpGet]
        [AllowAnonymous]
        public async Task<ResponseMsg> InsertClearanceNonSaleablePrepareItem(int WarehouseId)
        {
            var res = new ResponseMsg();
            using (var myContext = new AuthContext())
            {
                if (!myContext.Warehouses.FirstOrDefault(x => x.WarehouseId == WarehouseId).IsStopCurrentStockTrans)
                {
                    res.Status = false;
                    res.Message = "Inventory Transactions are currently enabled for this warehouse... Please stop it first and try again";
                    return res;
                }
                var widIdParams = new SqlParameter("@WarehouseId", WarehouseId);
                if (myContext.Database.SqlQuery<bool>("[Clearance].[IsInsertClearanceNonSaleable] @WarehouseId", widIdParams).FirstOrDefault())
                {
                    res.Status = true;
                    res.Message = "Clearance NonSaleable Item already Prepared";
                    return res;
                };

                var widIdParam = new SqlParameter("@WarehouseId", WarehouseId);

                int count = myContext.Database.ExecuteSqlCommand("[Clearance].[InsertClearanceNonSaleablePrepareItem] @WarehouseId", widIdParam);
                if (count > 0)
                {
                    res.Status = true;
                    res.Message = "Clearance NonSaleable Item Prepare Successfully";
                }
            }
            return res;
        }

        [Route("GetAllInventoryWarehouse")]
        [HttpGet]
        [AllowAnonymous]
        public async Task<List<inventoryWarehouseDC>> GetAllInventoryWarehouse()
        {
            using (var myContext = new AuthContext())
            {
                var identity = User.Identity as ClaimsIdentity;
                int userid = 0;
                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                    userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

                var useridParam = new SqlParameter("@userid", userid);
                DayOfWeek wk = DateTime.Today.DayOfWeek;

                var result = myContext.Database.SqlQuery<inventoryWarehouseDC>("GetAllInventoryWarehouse @userid", useridParam).ToList();

                MongoDbHelper<InventoryWarehouse> mongoDbHelper = new MongoDbHelper<InventoryWarehouse>();
                InventoryWarehouse inventoryWarehouse = new InventoryWarehouse();

                foreach (var item in result)
                {
                    if (item.saturdayflg == 1 && item.weekDay == "Saturday")
                    {
                        item.isShowBtn = true;
                    }
                    else if (item.saturdayflg == 0 && item.Fridayflg == 1 && item.weekDay == "Friday")
                    {
                        item.isShowBtn = true;
                    }
                    else if (item.Fridayflg == 0 && item.thursdayflg == 1 && item.weekDay == "Thursday")
                    {
                        item.isShowBtn = true;
                    }
                    else
                    {
                        item.isShowBtn = false;
                    }
                    var searchPredicate = PredicateBuilder.New<InventoryWarehouse>(x => x.IsDeleted == false && x.IsInventory && x.WarehouseId == item.WarehouseId);
                    inventoryWarehouse = mongoDbHelper.Select(searchPredicate).FirstOrDefault();
                    if (inventoryWarehouse != null)
                    {
                        item.IsInventory = inventoryWarehouse.IsInventory;
                    }
                }
                return result;
            }
        }

        [Route("AddInventoryWarehouse")]
        [HttpGet]
        public async Task<InventoryWarehouse> InsertInventoryWarehouse(int warehouseId)
        {
            InventoryWarehouse temp = new InventoryWarehouse();
            if (warehouseId > 0)
            {
                MongoDbHelper<InventoryWarehouse> mongoDbHelper = new MongoDbHelper<InventoryWarehouse>();
                var data = mongoDbHelper.Select(x => x.IsDeleted == false && x.WarehouseId == warehouseId).FirstOrDefault();
                if (data == null)
                {
                    temp.WarehouseId = warehouseId;
                    temp.IsInventory = true;
                    temp.IsActive = true;
                    temp.IsDeleted = false;
                    var Status = await mongoDbHelper.InsertAsync(temp);
                    temp.Msg = "Successfully Added!";
                }
                else
                {
                    temp.Msg = "Already Exist!!";
                }
            }
            return temp;
        }


        [Route("updateWarehoueInventry")]
        [HttpPost]
        public async Task<InventoryStartResDc> updateWarehoueInventry(warehouesDTO warehouesDC)
        {
            var result = new InventoryStartResDc();
            var identity = User.Identity as ClaimsIdentity;
            int userid = 0;
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

            MongoDbHelper<BatchCodeSubjectMongoQueue> mongoDbHelper = new MongoDbHelper<BatchCodeSubjectMongoQueue>();
            var BatchCodePredicate = PredicateBuilder.New<BatchCodeSubjectMongoQueue>(x => x.IsProcess == false && x.QueueName != "BatchCode" && x.WarehouseId == warehouesDC.WarehouseId);
            bool IsbatchCodeSubjectMongoQueues = (mongoDbHelper.Select(BatchCodePredicate)).Any();
            if (IsbatchCodeSubjectMongoQueues)
            {
                result.Message = "BatchCode Job is in process..please try after some time.";
                result.Status = false;
            }
            else
            {

                if (userid > 0 && warehouesDC != null)
                {
                    OrderOutPublisher Publisher = new OrderOutPublisher();
                    List<BatchCodeSubjectDc> PublisherPickerRejectStockList = new List<BatchCodeSubjectDc>();
                    TransactionOptions option = new TransactionOptions();
                    option.IsolationLevel = System.Transactions.IsolationLevel.RepeatableRead;
                    option.Timeout = TimeSpan.FromSeconds(300);
                    using (var dbContextTransaction = new TransactionScope(TransactionScopeOption.RequiresNew, option))
                    {
                        using (AuthContext db = new AuthContext())
                        {
                            var people = db.Peoples.FirstOrDefault(x => x.PeopleID == userid && x.Deleted == false);
                            if (people != null && people.Active)
                            {
                                Warehouse wh = db.Warehouses.Where(x => x.WarehouseId == warehouesDC.WarehouseId).FirstOrDefault();
                                if (wh.IsStopCurrentStockTrans == true)
                                {
                                    result.Message = "Already Running!";
                                    result.Status = false;
                                    return result;
                                }

                                //if (warehouesDC.TransactionType == "SM" && warehouesDC.IsStopCurrentStockTrans == wh.IsStopCurrentStockTrans && warehouesDC.IsPV == wh.IsPV)
                                //{
                                //    result.Message = "Already Running!";
                                //    result.Status = false;
                                //    return result;
                                //}
                                //if (warehouesDC.TransactionType == "IC" && warehouesDC.IsStopCurrentStockTrans == wh.IsStopCurrentStockTrans && warehouesDC.IsPV == wh.IsPV)
                                //{
                                //    result.Message = "Already Running!";
                                //    result.Status = false;
                                //    return result;
                                //}
                                //if (warehouesDC.TransactionType == "" && warehouesDC.IsStopCurrentStockTrans == wh.IsStopCurrentStockTrans && warehouesDC.IsPV == wh.IsPV)
                                //{
                                //    result.Message = "Already Running!";
                                //    result.Status = false;
                                //    return result;
                                //}
                                //MongoDbHelper<InventoryWarehouse> mongoDbHelper = new MongoDbHelper<InventoryWarehouse>();
                                //InventoryWarehouse inventoryWarehouse = new InventoryWarehouse();
                                //var searchPredicate = PredicateBuilder.New<InventoryWarehouse>(x => x.IsDeleted == false && x.IsInventory && x.WarehouseId == warehouesDC.WarehouseId);
                                //inventoryWarehouse = mongoDbHelper.Select(searchPredicate).FirstOrDefault();

                                if (warehouesDC.TransactionType == "IC" || warehouesDC.TransactionType == "PV")
                                {
                                    var PickerIds = db.Database.SqlQuery<long?>("IsInventoryCyclePickerPending @WarehouseId", new SqlParameter("@WarehouseId", warehouesDC.WarehouseId)).ToList();
                                    if (PickerIds != null && PickerIds.Any())
                                    {
                                        result.Message = "Can't Start Inventory Cycle, Please First Clear  following Picker :";// + string.Join(",", PickerIds);
                                        result.OrderIds = string.Join(",", PickerIds);
                                        return result;
                                    }

                                    var ClearanceIds = db.Database.SqlQuery<long?>("IsInventoryCycleClearancePending @WarehouseId", new SqlParameter("@WarehouseId", warehouesDC.WarehouseId)).ToList();
                                    if (ClearanceIds != null && ClearanceIds.Any())
                                    {
                                        result.Message = "Can't Start Inventory Cycle, Please First Clear following Clearance Picker :";// + string.Join(",", ClearanceIds);
                                        result.OrderIds = string.Join(",", ClearanceIds);
                                        return result;
                                    }

                                    var OrderIds = db.Database.SqlQuery<int?>("IsInventoryCycleAutoRTP @WarehouseId", new SqlParameter("@WarehouseId", warehouesDC.WarehouseId)).ToList();
                                    if (OrderIds != null && OrderIds.Any() && warehouesDC.IsInventoryCycleAutoRTPConfirm == false)
                                    {
                                        result.IsInventoryCycleAutoRTPFound = true;
                                        result.Message = "On your Confirmation following OrderIds: status will be change from ReadyToPick to pending.";
                                        result.OrderIds = string.Join(",", OrderIds);
                                        return result;
                                    }
                                    //new check for BatchMismatch

                                    var warehouseid = new SqlParameter("@WarehouseId", warehouesDC.WarehouseId);
                                    var BatchMismatchData = db.Database.SqlQuery<BatchMismatchDTO>("EXEC CurrentStockBatchDiff @WarehouseId", warehouseid).ToList();
                                    if (BatchMismatchData.Any() && BatchMismatchData.Count() > 0)
                                    {
                                        result.Message = "Can't Start Inventory Cycle, Please First Clear Batch Mismatch.Contact to IT Team.";
                                        result.BatchMismatchData = BatchMismatchData;
                                        return result;
                                    }

                                    if (OrderIds != null && OrderIds.Any() && warehouesDC.IsInventoryCycleAutoRTPConfirm)
                                    {
                                        var Orders = db.DbOrderMaster.Where(x => OrderIds.Contains(x.OrderId) && x.WarehouseId == warehouesDC.WarehouseId && x.Deleted == false).Include(x => x.orderDetails).ToList();
                                        var oid = Orders.Where(x => x.Status == "ReadyToPick").Select(x => x.OrderId).Distinct().ToList();
                                        var ReadyToPickOrderDetaillist = db.ReadyToPickOrderDetailDb.Where(x => oid.Contains(x.OrderId)).ToList();

                                        var roid = ReadyToPickOrderDetaillist.Select(x => x.OrderId).Distinct().ToList();
                                        if (oid.Count() != roid.Count())
                                        {
                                            result.Message = "Order RTP Vs PRTD Count Not Matched :" + oid.Count() + " Vs " + roid.Count();
                                            return result;

                                        }
                                        var longoid = oid.Select(x => Convert.ToInt64(x)).Distinct().ToList();

                                        var query = (from d in db.TripPlannerConfirmedDetails
                                                     join o in db.TripPlannerConfirmedOrders on d.Id equals o.TripPlannerConfirmedDetailId
                                                     where longoid.Contains((int)o.OrderId) && o.IsActive == true && o.IsDeleted == false
                                                     select d.TripPlannerConfirmedMasterId).ToList();


                                        MultiStockHelper<OnPickedCancelDc> MultiStockHelpers = new MultiStockHelper<OnPickedCancelDc>();
                                        List<OnPickedCancelDc> RTDOnPickedCancelList = new List<OnPickedCancelDc>();
                                        List<OrderMasterHistories> OrderHistoryList = new List<OrderMasterHistories>();
                                        foreach (var Order in Orders.Where(x => x.Status == "ReadyToPick").Distinct())
                                        {
                                            foreach (var item in Order.orderDetails)
                                            {
                                                item.Status = "Pending";
                                                item.UpdatedDate = indianTime;
                                            }

                                            Order.Status = "Pending";
                                            Order.UpdatedDate = indianTime;
                                            db.Entry(Order).State = EntityState.Modified;

                                            #region Order History
                                            OrderMasterHistories h1 = new OrderMasterHistories();
                                            h1.orderid = Order.OrderId;
                                            h1.Status = Order.Status;
                                            h1.Reasoncancel = null;
                                            h1.Warehousename = Order.WarehouseName;
                                            if (people.DisplayName != null)
                                            {
                                                h1.username = people.DisplayName;
                                            }
                                            else
                                            {
                                                h1.username = people.PeopleFirstName;
                                            }
                                            h1.userid = userid;
                                            h1.Description = "DueToInventoryCycle";
                                            h1.CreatedDate = indianTime;
                                            OrderHistoryList.Add(h1);
                                            #endregion

                                            #region stock Hit
                                            foreach (var StockHit in Order.orderDetails.Where(x => x.qty > 0))
                                            {
                                                int qty = ReadyToPickOrderDetaillist.Any(x => x.OrderDetailsId == StockHit.OrderDetailsId) ? ReadyToPickOrderDetaillist.FirstOrDefault(x => x.OrderDetailsId == StockHit.OrderDetailsId).Qty : 0;
                                                if (qty > 0)
                                                {
                                                    bool isfree = false;
                                                    string RefStockCode = (Order.OrderType == 8) ? "CL" : "C";
                                                    if (StockHit.IsFreeItem && StockHit.IsDispatchedFreeStock)
                                                    {
                                                        RefStockCode = "F";
                                                        isfree = true;
                                                    }
                                                    RTDOnPickedCancelList.Add(new OnPickedCancelDc
                                                    {
                                                        ItemMultiMRPId = StockHit.ItemMultiMRPId,
                                                        OrderDispatchedDetailsId = StockHit.OrderDetailsId,
                                                        OrderId = StockHit.OrderId,
                                                        Qty = qty,
                                                        UserId = userid,
                                                        WarehouseId = StockHit.WarehouseId,
                                                        IsFreeStock = isfree,
                                                        RefStockCode = RefStockCode
                                                    });
                                                }
                                            }
                                            #endregion

                                        }

                                        if (RTDOnPickedCancelList != null && RTDOnPickedCancelList.Any())
                                        {
                                            bool res = MultiStockHelpers.MakeEntry(RTDOnPickedCancelList, "Stock_OnPickedCancel", db, dbContextTransaction);
                                            if (!res)
                                            {
                                                dbContextTransaction.Dispose();
                                                result.Message = "Inventory not reverted on Auto Picker Canceled(InventoryCycle)";
                                                return result;
                                            }
                                            #region BatchCode
                                            foreach (var s in RTDOnPickedCancelList.Where(x => x.Qty > 0))
                                            {
                                                PublisherPickerRejectStockList.Add(new BatchCodeSubjectDc
                                                {
                                                    ObjectDetailId = s.OrderDispatchedDetailsId,  // its OrderDetailsId
                                                    ObjectId = s.OrderId,
                                                    StockType = s.RefStockCode,
                                                    Quantity = s.Qty,
                                                    WarehouseId = s.WarehouseId,
                                                    ItemMultiMrpId = s.ItemMultiMRPId
                                                });
                                            }
                                            #endregion
                                            db.OrderMasterHistoriesDB.AddRange(OrderHistoryList);
                                            if (query != null && query.Any())
                                            {
                                                var idlist = new DataTable();
                                                idlist.Columns.Add("IntValue");
                                                foreach (var item in RTDOnPickedCancelList.Select(x => x.OrderId).Distinct().ToList())
                                                {
                                                    var dr = idlist.NewRow();
                                                    dr["IntValue"] = item;
                                                    idlist.Rows.Add(dr);
                                                }
                                                var param = new SqlParameter("@OrderIdList", idlist);
                                                param.SqlDbType = SqlDbType.Structured;
                                                param.TypeName = "dbo.IntValues";
                                                int rcount = db.Database.ExecuteSqlCommand("RemoveAllOrdersFromTrips @OrderIdList", param);
                                                //if (rcount > 0) { }
                                                //else
                                                //{
                                                //    dbContextTransaction.Dispose();
                                                //    result.Message = "Inventory not reverted on Auto Picker order not Removed from Trips";
                                                //    return result;
                                                //}
                                            }
                                        }
                                    }
                                    else if (OrderIds != null && OrderIds.Any() && warehouesDC.IsInventoryCycleAutoRTPConfirm == false)
                                    {
                                        result.IsInventoryCycleAutoRTPFound = true;
                                        result.Message = "On your Confirmation following OrderIds:" + string.Join(",", OrderIds) + " status will be change from ReadyToPick to pending.";
                                        return result;
                                    }


                                }

                                if (warehouesDC.TransactionType == "PV")
                                {
                                    DateTime AssignDate = DateTime.Now.Date;
                                    List<CurrentStock> currentstockList = null;
                                    List<InventCycle> inventCycle = db.InventCycleDb.Where(x => x.WarehouseId == warehouesDC.WarehouseId && x.IsDeleted == false && EntityFunctions.TruncateTime(x.CreatedDate) == EntityFunctions.TruncateTime(AssignDate) && x.IsWarehouseApproved == 0).ToList();
                                    var inventCycleMultiMrpids = inventCycle.Select(x => x.ItemMultiMrpId).Distinct().ToList();
                                    var data = db.DbCurrentStock.Where(c => c.Deleted == false && c.WarehouseId == warehouesDC.WarehouseId).ToList();
                                    currentstockList = data.Where(c => inventCycleMultiMrpids.Contains(c.ItemMultiMRPId)).ToList();

                                    #region PvRecocillationHistory
                                    if (data.Count > 0 && data.Any())
                                    {
                                        List<PvReconcillationHistory> pvReconcillationHistories = data.Select(x => new PvReconcillationHistory
                                        {
                                            StockId = x.StockId,
                                            ItemName = x.itemname,
                                            CurrentInventory = x.CurrentInventory,
                                            MRP = x.MRP,
                                            WarehouseId = x.WarehouseId,
                                            WarehouseName = x.WarehouseName,
                                            ItemMultiMRPId = x.ItemMultiMRPId,
                                            ItemNumber = x.ItemNumber,
                                            Status = "Pending",
                                            IsActive = true,
                                            IsDeleted = false,
                                            CreatedBy = userid,
                                            CreatedDate = DateTime.Now,
                                            ModifiedBy = userid,
                                            ModifiedDate = DateTime.Now
                                        }).ToList();

                                        db.PvReconcillationHistoryDB.AddRange(pvReconcillationHistories);
                                    }
                                    #endregion


                                    db.DBWarehousePVHistory.Add(new WarehousePVHistory()
                                    {
                                        CreatedDate = DateTime.Now,
                                        WarehouseId = warehouesDC.WarehouseId,
                                        IsPV = true,
                                        CreatedBy = userid
                                    });
                                    wh.IsPV = true;
                                }


                                wh.IsStopCurrentStockTrans = warehouesDC.IsStopCurrentStockTrans;
                                wh.InventoryCycleEditby = userid;
                                wh.TransactionType = warehouesDC.TransactionType == "" ? "PV" : warehouesDC.TransactionType;
                                db.Entry(wh).State = EntityState.Modified;
                                InventoryCycleHistory add = new InventoryCycleHistory();
                                add.WarehoueId = warehouesDC.WarehouseId;
                                add.IsStopCurrentStockTrans = warehouesDC.IsStopCurrentStockTrans;
                                add.TransactionType = warehouesDC.TransactionType;
                                add.CreatedDate = indianTime;
                                add.ModifiedDate = indianTime;
                                add.IsActive = true;
                                add.IsDeleted = false;
                                add.CreatedBy = userid;
                                add.ModifiedBy = userid;
                                db.InventoryCycleHistoryDB.Add(add);

                                result.Status = db.Commit() > 0;
                                result.Message = warehouesDC.TransactionType != "PV" ? "Inventory For This Warehouse Started Successfully!" : "PV Started Successfully!";
                            }
                            else
                            {
                                result.Message = "You are not Authorize!";
                                return result;
                            }
                        }
                        if (result.Status)
                        {
                            dbContextTransaction.Complete();
                        }
                    }
                    if (PublisherPickerRejectStockList != null && PublisherPickerRejectStockList.Any() && result.Status)
                    {
                        Publisher.PlannedRejectPublish(PublisherPickerRejectStockList);
                    }
                }
            }
            return result;
        }
        [Route("StopWarehoueInventry")]
        [HttpPost]
        public string StopWarehoueInventry(warehouesDTO warehouesDC)
        {
            using (AuthContext db = new AuthContext())
            {
                DateTime AssignDate = DateTime.Now.Date;
                var identity = User.Identity as ClaimsIdentity;
                int userid = 0;
                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                    userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
                string message = "";
                bool activeUser = false;
                string TransactionType = "";
                People p = db.Peoples.Where(x => x.PeopleID == userid && x.Deleted == false).FirstOrDefault();
                if (p != null && p.Active == true)
                {
                    activeUser = true;
                }
                else
                {
                    message = "You are not Authorize!";
                    return message;
                }
                List<CurrentStock> currentstockList = null;
                Warehouse list = db.Warehouses.Where(x => x.WarehouseId == warehouesDC.WarehouseId).FirstOrDefault();
                if (list.InventoryCycleEditby != userid)
                {
                    People pDisplayName = db.Peoples.Where(x => x.PeopleID == list.InventoryCycleEditby && x.Active == true && x.Deleted == false).FirstOrDefault();
                    message = "You are not Authorized Please Connect with " + pDisplayName.DisplayName;
                    return message;
                }
                //if (list.IsStopCurrentStockTrans || list.IsPV || list.TransactionType)
                //{
                //    message = "Already Running!";
                //    return message;
                //}
                if (warehouesDC.TransactionType != "SM")
                {
                    List<InventCycle> inventCycle = db.InventCycleDb.Where(x => x.WarehouseId == warehouesDC.WarehouseId && x.IsDeleted == false && EntityFunctions.TruncateTime(x.CreatedDate) == EntityFunctions.TruncateTime(AssignDate) && x.IsWarehouseApproved == 0 && x.IsPV == list.IsPV).ToList();
                    //var inventCycleIds = inventCycle.Select(x => x.Id).Distinct().ToList();
                    var inventCycleMultiMrpids = inventCycle.Select(x => x.ItemMultiMrpId).Distinct().ToList();
                    currentstockList = db.DbCurrentStock.Where(c => inventCycleMultiMrpids.Contains(c.ItemMultiMRPId) && c.Deleted == false && c.WarehouseId == warehouesDC.WarehouseId).ToList();

                    MongoDbHelper<InventoryWarehouse> mongoDbHelper = new MongoDbHelper<InventoryWarehouse>();
                    InventoryWarehouse inventoryWarehouse = new InventoryWarehouse();
                    var searchPredicate = PredicateBuilder.New<InventoryWarehouse>(x => x.IsDeleted == false && x.IsInventory && x.WarehouseId == warehouesDC.WarehouseId);
                    inventoryWarehouse = mongoDbHelper.Select(searchPredicate).FirstOrDefault();


                    var IsPVProcessData = db.CompanyDetailsDB.Where(x => x.IsActive).OrderByDescending(x => x.Id).FirstOrDefault();
                    if (IsPVProcessData.IsPVProcess && list.TransactionType == "IC")
                    {
                        if (inventCycle.Count > 0 && list.IsPV == true)
                        {
                            message = "Can't Stop PV Inventory First Approve or Reject Todays Inventory!";
                            return message;
                        }
                    }
                    //
                    foreach (var ic in inventCycle)
                    {
                        var currentstock = currentstockList.FirstOrDefault(x => x.ItemMultiMRPId == ic.ItemMultiMrpId && x.WarehouseId == ic.WarehouseId);
                        List<InventCycleBatch> inventCycleBatches = db.InventCycleBatchDb.Where(x => x.InventCycle_Id == ic.Id).ToList();
                        if (currentstock.CurrentInventory == inventCycleBatches.Sum(x => x.InventoryCount))
                        {
                            //for same value in batch
                            var batchData = inventCycleBatches.Where(x => ((x.PastInventory ?? 0)) == x.InventoryCount).ToList();
                            if (batchData.Count > 0)
                            {

                            }
                            else
                            {
                                message = "Can't Stop Inventory First Approve or Reject Todays Inventory From Warehouse List!";
                                return message;
                            }
                            //foreach (var batchData in inventCycleBatches.Where(x => ((x.PastInventory ?? 0)) == x.InventoryCount).ToList())
                            //{

                            //}
                        }
                        else
                        {
                            //if (inventCycle.Count > 0 && inventoryWarehouse != null)
                            //{
                            //    message = "Can't Stop Inventory First Approve or Reject Todays Inventory From Warehouse List!";
                            //    return message;
                            //}
                        }
                    }
                    //
                    //if (inventCycle.Count > 0 && inventoryWarehouse != null)
                    //{
                    //    message = "Can't Stop Inventory First Approve or Reject Todays Inventory From Warehouse List!";
                    //    return message;
                    //}
                }
                if (activeUser == true && list != null)
                {
                    TransactionType = list.TransactionType;
                    if (list.InventoryCycleEditby == userid && list.IsStopCurrentStockTrans == true)
                    {

                    }
                    else
                    {
                        message = "You can't Stop Warehouse Inventory!!";
                        return message;
                    }
                    list.IsStopCurrentStockTrans = warehouesDC.IsStopCurrentStockTrans;
                    list.IsPV = false;
                    list.InventoryCycleEditby = userid;
                    //list.TransactionType = null;
                    db.Entry(list).State = EntityState.Modified;
                    InventoryCycleHistory add = new InventoryCycleHistory();
                    add.WarehoueId = warehouesDC.WarehouseId;
                    add.IsStopCurrentStockTrans = warehouesDC.IsStopCurrentStockTrans;
                    add.TransactionType = list.TransactionType;
                    add.CreatedDate = indianTime;
                    add.ModifiedDate = indianTime;
                    add.IsActive = true;
                    add.IsDeleted = false;
                    add.CreatedBy = userid;
                    add.ModifiedBy = userid;
                    db.InventoryCycleHistoryDB.Add(add);
                    if (warehouesDC.IsPV)
                    {
                        db.DBWarehousePVHistory.Add(new WarehousePVHistory()
                        {
                            CreatedDate = DateTime.Now,
                            WarehouseId = warehouesDC.WarehouseId,
                            IsPV = false,
                            CreatedBy = userid
                        });
                    }
                    db.Commit();
                    message = "Stop Warehoue Inventry Successfully!!";
                }
                else
                {
                    message = "you are not authorized !!";
                }
                return message;
            }
        }

        [Route("getInventorycyclehistory")]
        [HttpGet]
        public List<InventoryCycleHistoryDC> getInventorycycle(int WarehoueId)
        {
            using (AuthContext db = new AuthContext())
            {
                List<InventoryCycleHistoryDC> inventoryCycleHistory = (from c in db.InventoryCycleHistoryDB
                                                                       where c.WarehoueId == WarehoueId
                                                                       join cc in db.Warehouses on c.WarehoueId equals cc.WarehouseId
                                                                       join cs in db.Peoples on c.CreatedBy equals cs.PeopleID

                                                                       select new InventoryCycleHistoryDC
                                                                       {
                                                                           WarehoueName = cc.WarehouseName,
                                                                           IsStopCurrentStockTrans = c.IsStopCurrentStockTrans,
                                                                           CreatedDate = c.CreatedDate,
                                                                           CreatedBy = cs.DisplayName,
                                                                           TransactionType = c.TransactionType
                                                                       }).OrderByDescending(x => x.CreatedDate).ToList();
                return inventoryCycleHistory;
            }
        }


        [Route("getAllSupervisor")]
        [HttpGet]
        public List<Supervisors> getAllSupervisor(int warehouseId)
        {
            using (AuthContext context = new AuthContext())
            {
                string query = "exec GetInventorySupervisor " + warehouseId;
                List<Supervisors> person = context.Database.SqlQuery<Supervisors>(query).ToList();
                return person;
            }
        }

        [Route("GetWhSupervisorInventoryCycleDays")]
        [HttpGet]
        public List<SupervisorInventoryCycleDayDc> GetWhSupervisorInventoryCycleDays(int warehouseId, DateTime startDate)
        {
            List<SupervisorInventoryCycleDayDc> supervisorInventoryCycles = new List<SupervisorInventoryCycleDayDc>();
            MongoDbHelper<SupervisorInventoryCycleDay> mongoDbHelper = new MongoDbHelper<SupervisorInventoryCycleDay>();
            startDate = startDate.Date;
            var inventoryPredicate = PredicateBuilder.New<SupervisorInventoryCycleDay>(x => x.WarehouseId == warehouseId && x.AssignDate == startDate);
            supervisorInventoryCycles = mongoDbHelper.Select(inventoryPredicate).Select(x =>
                                        new SupervisorInventoryCycleDayDc { AssignDate = x.AssignDate, DisplayName = x.DisplayName, PeopleId = x.PeopleId, WarehouseName = x.WarehouseName, TotalItem = x.InventoryCycleItems != null ? x.InventoryCycleItems.Count() : 0 }
                                        ).ToList();

            return supervisorInventoryCycles;
        }

        [Route("AssignSupervisorDay")]
        [HttpPost]
        public response AssignSupervisorDay(int WarehouseId, DateTime AssignDate, List<int> supervisorIds)
        {
            response result = new response();
            AssignDate = AssignDate.Date;
            //if (supervisorIds.Count > 10)
            //{
            //    result.Status = false;
            //    result.msg = "Maximum 10 supervisor assign for inventory count";
            //    return result;
            //}
            bool IsPV = false;
            using (AuthContext context = new AuthContext())
            {
                IsPV = context.Warehouses.Any(x => x.WarehouseId == WarehouseId && x.IsPV);
            }
            List<SupervisorInventoryCycleDay> supervisorInventoryCycleDay = new List<SupervisorInventoryCycleDay>();
            MongoDbHelper<SupervisorInventoryCycleDay> mongoDbHelper = new MongoDbHelper<SupervisorInventoryCycleDay>();
            var inventoryPredicate = PredicateBuilder.New<SupervisorInventoryCycleDay>(x => x.WarehouseId == WarehouseId && x.AssignDate == AssignDate);
            if (IsPV)
                inventoryPredicate = inventoryPredicate.And(x => x.IsPV.HasValue && x.IsPV.Value == IsPV);
            else
                inventoryPredicate = inventoryPredicate.And(x => (!x.IsPV.HasValue || x.IsPV.Value == IsPV));
            var mongosupervisors = mongoDbHelper.Select(inventoryPredicate).Select(x =>
                                        new SupervisorInventoryCycleDayDc { AssignDate = x.AssignDate, DisplayName = x.DisplayName, PeopleId = x.PeopleId, WarehouseName = x.WarehouseName }
                                        ).ToList();
            if (mongosupervisors.Count() == 0)
            {
                using (AuthContext context = new AuthContext())
                {
                    var supervisors = context.Peoples.Where(x => supervisorIds.Contains(x.PeopleID)).Select(x => new { x.DisplayName, x.PeopleID }).ToList();

                    var query = "exec [GetInventoryCyclesForSupervisor] " + WarehouseId;
                    var list = context.Database.SqlQuery<GetInventCycledata>(query).ToList();
                    if (list != null && list.Any())
                    {
                        var itemnos = list.Select(x => x.ItemNumber).ToList();
                        var itemBarcode = context.ItemBarcodes.Where(x => itemnos.Contains(x.ItemNumber) && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value)).ToList();
                        int i = list.Count() > supervisorIds.Count() ? Convert.ToInt32(Math.Ceiling(list.Count() / Convert.ToDouble(supervisorIds.Count()))) : list.Count();
                        int a = 0, j = 0;
                        foreach (var people in supervisors)
                        {
                            var inventorycycleitems = list.Skip(a).Take(i);

                            if (inventorycycleitems != null && inventorycycleitems.Any())
                            {
                                var sInventoryCycleDay = new SupervisorInventoryCycleDay
                                {
                                    AssignDate = AssignDate.Date,
                                    DisplayName = people.DisplayName,
                                    PeopleId = people.PeopleID,
                                    WarehouseId = WarehouseId,
                                    WarehouseName = inventorycycleitems.FirstOrDefault().WarehouseName,
                                    IsPV = IsPV,
                                    InventoryCycleItems = inventorycycleitems.Select(x => new SupervisorInventoryCycleItem
                                    {
                                        Id = x.Id,
                                        NonSellableQty = x.NonSellableQty,
                                        ABCClassification = x.ABCClassification,
                                        Barcode = itemBarcode.Any(y => y.ItemNumber == x.ItemNumber) ? itemBarcode.Where(y => y.ItemNumber == x.ItemNumber).Select(y => y.Barcode).ToList() : new List<string>(),
                                        DamagedQty = 0,
                                        InventoryCount = 0,
                                        ItemBatch = new List<InventoryCycleItemBatch>(),
                                        ItemMultiMRPId = x.ItemMultiMRPId,
                                        ItemName = x.ItemName,
                                        ItemNumber = x.ItemNumber,
                                        MRP = x.MRP
                                    }).ToList()
                                };

                                supervisorInventoryCycleDay.Add(sInventoryCycleDay);
                                j++;
                                a = j * i;
                            }
                            else
                                break;
                        }

                        if (supervisorInventoryCycleDay != null && supervisorInventoryCycleDay.Any())
                        {
                            result.Status = mongoDbHelper.InsertMany(supervisorInventoryCycleDay);
                            result.msg = "Supervisor assign successfully.";
                        }
                    }
                    else
                    {
                        result.Status = false;
                        result.msg = "There is no item available fo inventory cycle.";
                    }
                }
            }
            else
            {
                result.Status = false;
                result.msg = "You already assign supervisor for inventory cycle.";
            }
            return result;

        }

        [Route("IsWarehouseTransactionsStop")]
        [HttpGet]
        public response IsWarehouseTransactionsStop(int warehouseId)
        {
            response result = new response();
            using (AuthContext context = new AuthContext())
            {
                var warehouse = context.Warehouses.FirstOrDefault(x => x.WarehouseId == warehouseId);
                result.Status = warehouse.IsStopCurrentStockTrans;
                if (!warehouse.IsStopCurrentStockTrans)
                    result.msg = "Please stop transactions of this warehouse before inventory count";
            }
            return result;
        }

        [Route("GetInventoryCycleItemFromBarCode")]
        [HttpGet]
        public async Task<List<SupervisorInventoryCycleItemDc>> GetInventoryCycleItemFromBarCode(int warehouseId, int PeopleId, string barcode)
        {
            List<SupervisorInventoryCycleItemDc> ScanInventoryCycleItemDcs = new List<SupervisorInventoryCycleItemDc>();
            using (AuthContext context = new AuthContext())
            {

                if (context.Database.Connection.State != System.Data.ConnectionState.Open)
                    context.Database.Connection.Open();

                var cmd = context.Database.Connection.CreateCommand();
                cmd.CommandText = "[dbo].[GetItemNumberFromBarCode]";
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.Parameters.Add(new SqlParameter("@barcode", barcode));
                cmd.Parameters.Add(new SqlParameter("@warehouseId", warehouseId));
                var number = (await cmd.ExecuteScalarAsync()).ToString();
                if (!string.IsNullOrEmpty(number))
                {
                    DateTime AssignDate = DateTime.Now.Date;
                    List<SupervisorInventoryCycleItemDc> SupervisorInventoryCycleItems = new List<SupervisorInventoryCycleItemDc>();
                    MongoDbHelper<SupervisorInventoryCycleDay> mongoDbHelper = new MongoDbHelper<SupervisorInventoryCycleDay>();
                    var inventoryPredicate = PredicateBuilder.New<SupervisorInventoryCycleDay>(x => x.WarehouseId == warehouseId && x.AssignDate == AssignDate && x.PeopleId == PeopleId && !x.IsSubmitted);
                    var mongosupervisor = (await mongoDbHelper.SelectAsync(inventoryPredicate)).FirstOrDefault();
                    if (mongosupervisor != null && mongosupervisor.InventoryCycleItems != null && mongosupervisor.InventoryCycleItems.Any())
                    {
                        ScanInventoryCycleItemDcs = mongosupervisor.InventoryCycleItems.Where(x => x.ItemNumber == number).Select(x => new SupervisorInventoryCycleItemDc
                        {
                            ABCClassification = x.ABCClassification,
                            Barcode = x.Barcode,
                            ItemMultiMRPId = x.ItemMultiMRPId,
                            ItemName = x.ItemName,
                            ItemNumber = x.ItemNumber,
                            MRP = x.MRP,
                            status = x.ItemBatch != null && x.ItemBatch.Any() ? 1 : 0,
                            ItemBatch = x.ItemBatch,
                            Id = x.Id
                        }).ToList();



                        if (context.Database.Connection.State != ConnectionState.Open)
                            context.Database.Connection.Open();

                        var orderIdDt = new DataTable();
                        orderIdDt.Columns.Add("IntValue");



                        foreach (var ItemMultiMRPId in ScanInventoryCycleItemDcs.Select(x => x.ItemMultiMRPId).Distinct().ToList())
                        {
                            var dr = orderIdDt.NewRow();
                            dr["IntValue"] = ItemMultiMRPId;
                            orderIdDt.Rows.Add(dr);
                        }


                        var param = new SqlParameter("itemMultiMrpId", orderIdDt);
                        param.SqlDbType = SqlDbType.Structured;
                        param.TypeName = "dbo.IntValues";
                        cmd = context.Database.Connection.CreateCommand();
                        cmd.CommandText = "[dbo].[GetStockBatchMasterForInventoryCycle]";
                        cmd.CommandType = System.Data.CommandType.StoredProcedure;
                        cmd.Parameters.Add(new SqlParameter("@warehouseId", warehouseId));
                        cmd.Parameters.Add(param);
                        cmd.Parameters.Add(new SqlParameter("@itemnumber", number));
                        cmd.Parameters.Add(new SqlParameter("@StockType", "C"));

                        // Run the sproc
                        var reader1 = cmd.ExecuteReader();
                        var BatchDcs = ((IObjectContextAdapter)context)
                         .ObjectContext
                         .Translate<InventoryCycleBatchDc>(reader1).ToList();

                        //if (BatchMasterIds != null && BatchMasterIds.Any())
                        //    BatchDcs = BatchDcs.Where(x => !BatchMasterIds.Contains(x.BatchMasterId)).ToList();

                        foreach (var item in ScanInventoryCycleItemDcs)
                        {
                            var BatchMasterIds = item.ItemBatch != null && item.ItemBatch.Any() ? item.ItemBatch.Select(x => x.BatchMasterId).ToList() : new List<long>();
                            item.BatchDcs = BatchDcs.Where(x => !BatchMasterIds.Contains(x.BatchMasterId) && x.ItemNumber == number && x.ItemMultiMrpId == item.ItemMultiMRPId).Distinct().ToList();


                            item.AllItemBatch = new List<ItemBatchDc>();
                            if (item.ItemBatch != null && item.ItemBatch.Any())
                            {
                                item.AllItemBatch.AddRange(item.ItemBatch.Select(x => new ItemBatchDc
                                {
                                    AvailableQty = BatchDcs.FirstOrDefault(y => y.BatchMasterId == x.BatchMasterId).Qty,
                                    BatchCode = x.BatchCode,
                                    BatchMasterId = x.BatchMasterId,
                                    InventoryCount = x.InventoryCount,
                                    ItemMultiMRPId = item.ItemMultiMRPId,
                                    StockBatchId = x.StockBatchId,
                                    ExpiryDate = x.ExpiryDate,
                                    MFGDate = x.MFGDate,
                                    DamagedImageUrl = "",
                                    DamagedQty = 0,
                                    NonSellableImageUrl = "",
                                    NonSellableQty = 0
                                }).ToList());
                            }
                            if (item.BatchDcs != null && item.BatchDcs.Any())
                            {
                                item.AllItemBatch.AddRange(item.BatchDcs.Where(x => x.Qty > 0).Select(x => new ItemBatchDc
                                {
                                    AvailableQty = x.Qty,
                                    BatchCode = x.BatchCode,
                                    BatchMasterId = x.BatchMasterId,
                                    InventoryCount = -1,
                                    ItemMultiMRPId = item.ItemMultiMRPId,
                                    StockBatchId = x.StockBatchId,
                                    ExpiryDate = x.ExpiryDate,
                                    MFGDate = x.MFGDate,
                                    DamagedImageUrl = "",
                                    DamagedQty = 0,
                                    NonSellableImageUrl = "",
                                    NonSellableQty = 0
                                }).ToList());
                            }
                        }
                    }
                }
            }

            return ScanInventoryCycleItemDcs;
        }

        [Route("GetSupervisorInventoryCycleItem")]
        [HttpGet]
        public async Task<List<SupervisorInventoryCycleItemDc>> GetSupervisorInventoryCycleItem(int WarehouseId, int PeopleId, bool IsPV = false)
        {
            IsPV = false;
            DateTime AssignDate = DateTime.Now.Date;
            List<SupervisorInventoryCycleItemDc> SupervisorInventoryCycleItems = new List<SupervisorInventoryCycleItemDc>();
            MongoDbHelper<SupervisorInventoryCycleDay> mongoDbHelper = new MongoDbHelper<SupervisorInventoryCycleDay>();
            using (AuthContext context = new AuthContext())
            {
                IsPV = context.Warehouses.Any(x => x.WarehouseId == WarehouseId && x.IsPV);
                Warehouse wh = context.Warehouses.Where(x => x.WarehouseId == WarehouseId).FirstOrDefault();
                if (wh.TransactionType == "SM")
                {
                    return SupervisorInventoryCycleItems;
                }
            }
            var inventoryPredicate = PredicateBuilder.New<SupervisorInventoryCycleDay>(x => x.WarehouseId == WarehouseId && x.AssignDate == AssignDate && x.PeopleId == PeopleId && !x.IsSubmitted);
            if (IsPV)
                inventoryPredicate = inventoryPredicate.And(x => x.IsPV.HasValue && x.IsPV.Value == IsPV);
            else
                inventoryPredicate = inventoryPredicate.And(x => (!x.IsPV.HasValue || x.IsPV.Value == IsPV));

            var mongosupervisor = (await mongoDbHelper.SelectAsync(inventoryPredicate)).FirstOrDefault();
            if (mongosupervisor != null && mongosupervisor.InventoryCycleItems != null && mongosupervisor.InventoryCycleItems.Any())
            {
                if (!mongosupervisor.IsSubmitted)
                {
                    SupervisorInventoryCycleItems = mongosupervisor.InventoryCycleItems.Select(x => new SupervisorInventoryCycleItemDc
                    {
                        Id = x.Id,
                        ABCClassification = x.ABCClassification,
                        Barcode = x.Barcode,
                        ItemMultiMRPId = x.ItemMultiMRPId,
                        ItemName = x.ItemName,
                        ItemNumber = x.ItemNumber,
                        MRP = x.MRP,
                        status = x.ItemBatch != null && x.ItemBatch.Any() ? 1 : 0
                    }).ToList();

                    if (IsPV)
                    {
                        SupervisorInventoryCycleItems = SupervisorInventoryCycleItems.Where(x => x.status == 1).ToList();
                    }


                }

            }
            return SupervisorInventoryCycleItems;
        }

        [Route("GetInventoryCycleItemDetail")]
        [HttpGet]
        public async Task<SupervisorInventoryCycleItemDc> GetInventoryCycleItemDetail(int WarehouseId, int PeopleId, int Id)
        {
            bool IsPV = false;
            using (AuthContext context = new AuthContext())
            {
                IsPV = context.Warehouses.Any(x => x.WarehouseId == WarehouseId && x.IsPV);
            }
            DateTime AssignDate = DateTime.Now.Date;
            SupervisorInventoryCycleItemDc SupervisorInventoryCycleItems = new SupervisorInventoryCycleItemDc();
            MongoDbHelper<SupervisorInventoryCycleDay> mongoDbHelper = new MongoDbHelper<SupervisorInventoryCycleDay>();
            var inventoryPredicate = PredicateBuilder.New<SupervisorInventoryCycleDay>(x => x.WarehouseId == WarehouseId && x.AssignDate == AssignDate && x.PeopleId == PeopleId);
            if (IsPV)
                inventoryPredicate = inventoryPredicate.And(x => x.IsPV.HasValue && x.IsPV.Value == IsPV);
            else
                inventoryPredicate = inventoryPredicate.And(x => (!x.IsPV.HasValue || x.IsPV.Value == IsPV));
            var mongosupervisor = (await mongoDbHelper.SelectAsync(inventoryPredicate)).FirstOrDefault();
            if (mongosupervisor != null && mongosupervisor.InventoryCycleItems != null && mongosupervisor.InventoryCycleItems.Any(x => x.Id == Id))
            {
                SupervisorInventoryCycleItems = mongosupervisor.InventoryCycleItems.Where(x => x.Id == Id).Select(x => new SupervisorInventoryCycleItemDc
                {
                    Id = x.Id,
                    ABCClassification = x.ABCClassification,
                    Barcode = x.Barcode,
                    ItemMultiMRPId = x.ItemMultiMRPId,
                    ItemName = x.ItemName,
                    ItemNumber = x.ItemNumber,
                    MRP = x.MRP,
                    status = x.ItemBatch != null && x.ItemBatch.Any() ? 1 : 0,
                    Comment = x.Comment,
                    ItemBatch = x.ItemBatch != null && x.ItemBatch.Any() ?
                      x.ItemBatch.Select(y => new InventoryCycleItemBatch
                      {
                          BatchCode = y.BatchCode,
                          BatchMasterId = y.BatchMasterId,
                          DamagedImageUrl = string.IsNullOrEmpty(y.DamagedImageUrl) ? "" : string.Format("{0}://{1}:{2}/{3}", new Uri(HttpContext.Current.Request.Url.AbsoluteUri).Scheme
                                                                    , HttpContext.Current.Request.Url.DnsSafeHost
                                                                    , (HttpContext.Current.Request.Url.Port != 80 && HttpContext.Current.Request.Url.Port != 443 ? ":" + HttpContext.Current.Request.Url.Port : "")
                                                                    , y.DamagedImageUrl),
                          DamagedQty = y.DamagedQty,
                          ExpiryDate = y.ExpiryDate,
                          InventoryCount = y.InventoryCount,
                          MFGDate = y.MFGDate,
                          NonSellableImageUrl = string.IsNullOrEmpty(y.NonSellableImageUrl) ? "" : string.Format("{0}://{1}:{2}/{3}", new Uri(HttpContext.Current.Request.Url.AbsoluteUri).Scheme
                                                                    , HttpContext.Current.Request.Url.DnsSafeHost
                                                                    , (HttpContext.Current.Request.Url.Port != 80 && HttpContext.Current.Request.Url.Port != 443 ? ":" + HttpContext.Current.Request.Url.Port : "")
                                                                    , y.NonSellableImageUrl),
                          NonSellableQty = y.NonSellableQty,
                          StockBatchId = y.StockBatchId
                      }).ToList()
                    : new List<InventoryCycleItemBatch>(),
                }).FirstOrDefault();

                var BatchMasterIds = mongosupervisor.InventoryCycleItems.Where(x => x.Id == Id && x.ItemBatch != null && x.ItemBatch.Any()).SelectMany(x => x.ItemBatch).Select(x => x.BatchMasterId).ToList();

                using (AuthContext context = new AuthContext())
                {

                    if (context.Database.Connection.State != ConnectionState.Open)
                        context.Database.Connection.Open();

                    var orderIdDt = new DataTable();
                    orderIdDt.Columns.Add("IntValue");

                    var dr = orderIdDt.NewRow();
                    dr["IntValue"] = SupervisorInventoryCycleItems.ItemMultiMRPId;
                    orderIdDt.Rows.Add(dr);

                    var param = new SqlParameter("itemMultiMrpId", orderIdDt);
                    param.SqlDbType = SqlDbType.Structured;
                    param.TypeName = "dbo.IntValues";
                    var cmd = context.Database.Connection.CreateCommand();
                    cmd.CommandText = "[dbo].[GetStockBatchMasterForInventoryCycle]";
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@warehouseId", WarehouseId));
                    cmd.Parameters.Add(param);
                    cmd.Parameters.Add(new SqlParameter("@itemnumber", SupervisorInventoryCycleItems.ItemNumber));
                    cmd.Parameters.Add(new SqlParameter("@StockType", "C"));

                    // Run the sproc
                    var reader1 = cmd.ExecuteReader();
                    var BatchDcs = ((IObjectContextAdapter)context)
                     .ObjectContext
                     .Translate<InventoryCycleBatchDc>(reader1).ToList();

                    if (BatchMasterIds != null && BatchMasterIds.Any())
                        SupervisorInventoryCycleItems.BatchDcs = BatchDcs.Where(x => !BatchMasterIds.Contains(x.BatchMasterId) && x.ItemMultiMrpId == SupervisorInventoryCycleItems.ItemMultiMRPId).ToList();
                    else
                        SupervisorInventoryCycleItems.BatchDcs = BatchDcs.Where(x => x.ItemMultiMrpId == SupervisorInventoryCycleItems.ItemMultiMRPId).ToList();


                    SupervisorInventoryCycleItems.AllItemBatch = new List<ItemBatchDc>();
                    if (SupervisorInventoryCycleItems.ItemBatch != null && SupervisorInventoryCycleItems.ItemBatch.Any())
                    {
                        SupervisorInventoryCycleItems.AllItemBatch.AddRange(SupervisorInventoryCycleItems.ItemBatch.Select(x => new ItemBatchDc
                        {
                            AvailableQty = BatchDcs.FirstOrDefault(y => y.BatchMasterId == x.BatchMasterId).Qty,
                            BatchCode = x.BatchCode,
                            BatchMasterId = x.BatchMasterId,
                            InventoryCount = x.InventoryCount,
                            ItemMultiMRPId = SupervisorInventoryCycleItems.ItemMultiMRPId,
                            StockBatchId = x.StockBatchId,
                            ExpiryDate = x.ExpiryDate,
                            MFGDate = x.MFGDate,
                            DamagedImageUrl = "",
                            DamagedQty = 0,
                            NonSellableImageUrl = "",
                            NonSellableQty = 0
                        }).ToList());
                    }
                    if (SupervisorInventoryCycleItems.BatchDcs != null && SupervisorInventoryCycleItems.BatchDcs.Any())
                    {
                        SupervisorInventoryCycleItems.AllItemBatch.AddRange(SupervisorInventoryCycleItems.BatchDcs.Where(x => x.Qty > 0).Select(x => new ItemBatchDc
                        {
                            AvailableQty = x.Qty,
                            BatchCode = x.BatchCode,
                            BatchMasterId = x.BatchMasterId,
                            InventoryCount = -1,
                            ItemMultiMRPId = SupervisorInventoryCycleItems.ItemMultiMRPId,
                            StockBatchId = x.StockBatchId,
                            ExpiryDate = x.ExpiryDate,
                            MFGDate = x.MFGDate,
                            DamagedImageUrl = "",
                            DamagedQty = 0,
                            NonSellableImageUrl = "",
                            NonSellableQty = 0
                        }).ToList());
                    }

                }
            }
            return SupervisorInventoryCycleItems;
        }

        [Route("AddBatchCode")]
        [HttpPost]
        public ItemBatchDc AddBatchCode(GRNBatchDc InsertGRNBatche, int CreatedBy, int ItemMultiMrpId)
        {
            ItemBatchDc itemBatchDc = new ItemBatchDc();
            bool result = false;
            ControllerV7.SarthiAppController SarthiApp = new ControllerV7.SarthiAppController();
            using (AuthContext context = new AuthContext())
            {
                var batchcode = SarthiApp.InsertAndGetBatch(context, InsertGRNBatche, CreatedBy);

                itemBatchDc = new ItemBatchDc
                {
                    AvailableQty = 0,
                    BatchCode = batchcode.BatchCode,
                    BatchMasterId = batchcode.Id,
                    InventoryCount = -1,
                    ItemMultiMRPId = ItemMultiMrpId,
                    StockBatchId = 0,
                    ExpiryDate = batchcode.ExpiryDate,
                    MFGDate = batchcode.MFGDate,
                    DamagedImageUrl = "",
                    DamagedQty = 0,
                    NonSellableImageUrl = "",
                    NonSellableQty = 0
                };

                result = batchcode != null && batchcode.Id > 0;
            }
            return itemBatchDc;
        }

        [Route("InsertBatchDataForItem")]
        [HttpPost]
        public async Task<response> InsertInventoryCycleForItem(List<InventoryCycleItemBatchDc> inventoryCycleItemBatchDcs)
        {
            bool IsPV = false;
            using (AuthContext context = new AuthContext())
            {
                var inventoryCycleItemBatchDc = inventoryCycleItemBatchDcs.FirstOrDefault();
                IsPV = context.Warehouses.Any(x => x.WarehouseId == inventoryCycleItemBatchDc.WarehouseId && x.IsPV);
                DateTime AssignDate = DateTime.Now.Date;
                response result = new response();
                MongoDbHelper<SupervisorInventoryCycleDay> mongoDbHelper = new MongoDbHelper<SupervisorInventoryCycleDay>();
                var inventoryPredicate = PredicateBuilder.New<SupervisorInventoryCycleDay>(x => x.WarehouseId == inventoryCycleItemBatchDc.WarehouseId && x.AssignDate == AssignDate && x.PeopleId == inventoryCycleItemBatchDc.PeopleId);
                if (IsPV)
                    inventoryPredicate = inventoryPredicate.And(x => x.IsPV.HasValue && x.IsPV.Value == IsPV);
                else
                    inventoryPredicate = inventoryPredicate.And(x => (!x.IsPV.HasValue || x.IsPV.Value == IsPV));
                var mongosupervisor = (await mongoDbHelper.SelectAsync(inventoryPredicate)).FirstOrDefault();
                if (mongosupervisor != null && mongosupervisor.InventoryCycleItems != null && mongosupervisor.InventoryCycleItems.Any(x => x.ItemMultiMRPId == inventoryCycleItemBatchDc.ItemMultiMrpId))
                {
                    mongosupervisor.IsPV = IsPV;
                    Warehouse whData = context.Warehouses.FirstOrDefault(x => x.WarehouseId == mongosupervisor.WarehouseId && x.active == true && x.Deleted == false);
                    var InventoryCycleItems = mongosupervisor.InventoryCycleItems.FirstOrDefault(x => x.Id == inventoryCycleItemBatchDc.Id);
                    if (InventoryCycleItems == null)
                    {
                        var inventoryCycle = context.InventCycleDb.FirstOrDefault(x => x.WarehouseId == inventoryCycleItemBatchDc.WarehouseId && x.ItemMultiMrpId == inventoryCycleItemBatchDc.ItemMultiMrpId && EntityFunctions.TruncateTime(x.CreatedDate) == EntityFunctions.TruncateTime(AssignDate) && x.IsPV == whData.IsPV);
                        var people = context.Peoples.FirstOrDefault(x => x.PeopleID == inventoryCycleItemBatchDc.PeopleId);
                        if (inventoryCycle != null)
                        {
                            inventoryCycle.CreatedBy = people.DisplayName;
                            inventoryCycle.IsPV = whData.IsPV;
                            context.Entry(inventoryCycle).State = EntityState.Modified;
                        }
                        else
                        {
                            inventoryCycle.CreatedBy = people.DisplayName;
                            inventoryCycle.IsPV = whData.IsPV;
                            context.InventCycleDb.Add(inventoryCycle);
                        }

                        context.Commit();


                        //var currentStock = context.DbCurrentStock.FirstOrDefault(x => x.WarehouseId == inventoryCycleItemBatchDc.WarehouseId && x.ItemMultiMRPId == inventoryCycleItemBatchDc.ItemMultiMrpId);
                        InventoryCycleItems = mongosupervisor.InventoryCycleItems.FirstOrDefault(x => x.Id == inventoryCycle.Id);
                        //var inventcycleBatch = context.InventCycleBatchDb.Where(x => x.InventCycle_Id == inventoryCycle.Id).ToList();
                        //inventoryCycle.InventoryCount = inventoryCycleItemBatchDc.InventoryCount;
                        //context.Entry(inventoryCycle).State = EntityState.Modified;
                        //context.Commit();
                    }
                    if (InventoryCycleItems != null)
                    {
                        var current = context.DbCurrentStock.FirstOrDefault(x => x.ItemMultiMRPId == inventoryCycleItemBatchDc.ItemMultiMrpId && x.WarehouseId == inventoryCycleItemBatchDc.WarehouseId);

                        foreach (var batch in inventoryCycleItemBatchDcs.Distinct())
                        {
                            if (InventoryCycleItems.ItemBatch != null && InventoryCycleItems.ItemBatch.Any(x => x.BatchMasterId == batch.BatchMasterId))
                            {
                                var batchcode = InventoryCycleItems.ItemBatch.FirstOrDefault(x => x.BatchMasterId == batch.BatchMasterId);
                                var stock = context.StockBatchMasters.FirstOrDefault(x => x.StockId == current.StockId && x.StockType == "C" && x.BatchMasterId == batch.BatchMasterId);
                                batchcode.StockBatchId = stock != null ? stock.Id : batch.StockBatchId;

                                if (!string.IsNullOrEmpty(batch.DamagedImageUrl))
                                    batchcode.DamagedImageUrl = batch.DamagedImageUrl;
                                if (!string.IsNullOrEmpty(batch.NonSellableImageUrl))
                                    batchcode.NonSellableImageUrl = batch.NonSellableImageUrl;
                                if (batch.DamagedQty >= 0)
                                    batchcode.DamagedQty = batch.DamagedQty;
                                if (batch.NonSellableQty >= 0)
                                    batchcode.NonSellableQty = batch.NonSellableQty;
                                if (batch.InventoryCount >= 0)
                                    batchcode.InventoryCount = batch.InventoryCount;

                            }
                            else
                            {
                                if (InventoryCycleItems.ItemBatch == null)
                                    InventoryCycleItems.ItemBatch = new List<InventoryCycleItemBatch>();
                                InventoryCycleItemBatch batchData = new InventoryCycleItemBatch
                                {
                                    StockBatchId = batch.StockBatchId,
                                    BatchCode = batch.BatchCode,
                                    BatchMasterId = batch.BatchMasterId,
                                    DamagedImageUrl = batch.DamagedImageUrl,
                                    DamagedQty = batch.DamagedQty,
                                    ExpiryDate = batch.ExpiryDate,
                                    InventoryCount = batch.InventoryCount,
                                    MFGDate = batch.MFGDate,
                                    NonSellableImageUrl = batch.NonSellableImageUrl,
                                    NonSellableQty = batch.NonSellableQty,

                                };
                                InventoryCycleItems.ItemBatch.Add(batchData);
                            }
                        }
                        InventoryCycleItems.DamagedQty = InventoryCycleItems.ItemBatch.Sum(x => x.DamagedQty);
                        InventoryCycleItems.NonSellableQty = InventoryCycleItems.ItemBatch.Sum(x => x.NonSellableQty);
                        InventoryCycleItems.InventoryCount = InventoryCycleItems.ItemBatch.Sum(x => x.InventoryCount);
                        InventoryCycleItems.Comment = inventoryCycleItemBatchDc.Comment;

                        result.Status = await mongoDbHelper.ReplaceAsync(mongosupervisor.Id, mongosupervisor);
                    }
                    else
                    {

                    }
                    //else
                    //{
                    //    using (AuthContext context = new AuthContext())
                    //    {
                    //        var people = context.Peoples.FirstOrDefault(x => x.PeopleID == inventoryCycleItemBatchDc.PeopleId);
                    //        var warehouse = context.Warehouses.FirstOrDefault(x => x.WarehouseId == inventoryCycleItemBatchDc.WarehouseId);
                    //        mongosupervisor.AssignDate = AssignDate;
                    //        mongosupervisor.DisplayName = people.DisplayName;
                    //        //mongosupervisor.Id = inventoryCycleItemBatchDc.Id;
                    //        mongosupervisor.PeopleId = inventoryCycleItemBatchDc.PeopleId;
                    //        mongosupervisor.WarehouseId = inventoryCycleItemBatchDc.WarehouseId;
                    //        mongosupervisor.WarehouseName = warehouse.WarehouseName;
                    //        //mongosupervisor.
                    //    }     
                    //}
                }
                if (result.Status)
                    result.msg = "Inventory cycle updated successfully.";
                else
                    result.msg = "Inventory cycle not updated.";


                return result;
            }
        }

        [Route("DeleteTodayInventoryCycleData")]
        [HttpGet]
        public async Task<response> DeleteTodayInventoryCycleData(int WarehouseId)
        {
            var date = DateTime.Today;
            response result = new response { Status = false, msg = "Inventory cycle data not deleted." };
            using (var db = new AuthContext())
            {
                if (WarehouseId > 0)
                {
                    var warehouseParam = new SqlParameter
                    {
                        ParameterName = "WarehouseId",
                        Value = WarehouseId
                    };
                    await db.Database.ExecuteSqlCommandAsync("Exec DeleteInventCycle @WarehouseId", warehouseParam);
                    MongoDbHelper<SupervisorInventoryCycleDay> mongoDbHelper = new MongoDbHelper<SupervisorInventoryCycleDay>();
                    var inventoryPredicate = PredicateBuilder.New<SupervisorInventoryCycleDay>(x => x.WarehouseId == WarehouseId && x.AssignDate == date);
                    var mongosupervisor = (await mongoDbHelper.SelectAsync(inventoryPredicate)).FirstOrDefault();
                    if (mongosupervisor != null)
                    {
                        var ss = await mongoDbHelper.DeleteAsync(mongosupervisor.Id, "SupervisorInventoryCycleDay");
                    }
                    result.Status = true;
                    result.msg = "Inventory cycle record insert successfully!!";
                }
            }
            return result;
        }

        [Route("GetWarehouseList")]//For WarehouseList
        [HttpGet]
        public HttpResponseMessage GetWarehouseList()
        {
            using (var db = new AuthContext())
            {
                var identity = User.Identity as ClaimsIdentity;
                //int Warehouse_id = 0;
                //if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "Warehouseid"))
                //    Warehouse_id = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "Warehouseid").Value);

                List<Warehouse> whList = db.Warehouses.Where(x => x.Deleted == false && x.active == true).ToList();
                return Request.CreateResponse(HttpStatusCode.OK, whList);
            }
        }

        #region New StartPVwarehouseInventory Api
        [Route("StartPVwarehouseInventory")]//Check Picker B4 For Start PV 
        [HttpPost]
        public InventoryStartResDc StartPVwarehouseInventory(warehouesDTO warehouesDC)
        {
            var result = new InventoryStartResDc();
            var identity = User.Identity as ClaimsIdentity;
            int userid = 0;
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
            //BatchCodeSubjectMongoQueue> batchCodeSubjectMongoQueues = new List<BatchCodeSubjectMongoQueue>();
            //new code for BatchCode Mismatch
            TextFileLogHelper.TraceLog("StartPVwarehouseInventory Start 1" + warehouesDC.TransactionType);
            MongoDbHelper<BatchCodeSubjectMongoQueue> mongoDbHelper = new MongoDbHelper<BatchCodeSubjectMongoQueue>();
            var BatchCodePredicate = PredicateBuilder.New<BatchCodeSubjectMongoQueue>(x => x.IsProcess == false && x.QueueName != "BatchCode" && x.WarehouseId == warehouesDC.WarehouseId);
            bool IsbatchCodeSubjectMongoQueues = (mongoDbHelper.Select(BatchCodePredicate)).Any();
            TextFileLogHelper.TraceLog(" Inside IsbatchCodeSubjectMongoQueues Start 2" + IsbatchCodeSubjectMongoQueues);

            if (IsbatchCodeSubjectMongoQueues)
            {
                TextFileLogHelper.TraceLog(" Inside IsbatchCodeSubjectMongoQueues Start 2" + IsbatchCodeSubjectMongoQueues);

                result.Message = "BatchCode Job is in process..please try after some time.";
                result.Status = false;
            }
            else
            {
                if (userid > 0 && warehouesDC != null)
                {
                    OrderOutPublisher Publisher = new OrderOutPublisher();
                    List<BatchCodeSubjectDc> PublisherPickerRejectStockList = new List<BatchCodeSubjectDc>();
                    TransactionOptions option = new TransactionOptions();
                    option.IsolationLevel = System.Transactions.IsolationLevel.RepeatableRead;
                    option.Timeout = TimeSpan.FromSeconds(300);
                    using (var dbContextTransaction = new TransactionScope(TransactionScopeOption.RequiresNew, option))
                    {
                        using (AuthContext db = new AuthContext())
                        {
                            var people = db.Peoples.FirstOrDefault(x => x.PeopleID == userid && x.Deleted == false);
                            if (people != null && people.Active)
                            {
                                DateTime AssignDate = DateTime.Now.Date;
                                Warehouse warehouse = db.Warehouses.Where(x => x.WarehouseId == warehouesDC.WarehouseId && x.Deleted == false && x.active == true && !x.IsPV).FirstOrDefault();
                                List<InventCycle> inventCycle = db.InventCycleDb.Where(x => x.WarehouseId == warehouesDC.WarehouseId && x.IsDeleted == false && EntityFunctions.TruncateTime(x.CreatedDate) == EntityFunctions.TruncateTime(AssignDate) && x.IsWarehouseApproved == 0).ToList();
                                if (!string.IsNullOrEmpty(warehouse.TransactionType))
                                {

                                    var PickerIds = db.Database.SqlQuery<long?>("IsInventoryCyclePickerPending @WarehouseId", new SqlParameter("@WarehouseId", warehouesDC.WarehouseId)).ToList();
                                    if (PickerIds != null && PickerIds.Any())
                                    {
                                        result.Message = "Can't Start Inventory Cycle, Please First Clear  following Picker :";// + string.Join(",", PickerIds);
                                        result.OrderIds = string.Join(",", PickerIds);
                                        return result;
                                    }

                                    var ClearanceIds = db.Database.SqlQuery<long?>("IsInventoryCycleClearancePending @WarehouseId", new SqlParameter("@WarehouseId", warehouesDC.WarehouseId)).ToList();
                                    if (ClearanceIds != null && ClearanceIds.Any())
                                    {
                                        result.Message = "Can't Start Inventory Cycle, Please First Clear following Clearance Picker :";// + string.Join(",", ClearanceIds);
                                        result.OrderIds = string.Join(",", ClearanceIds);
                                        return result;
                                    }

                                    var OrderIds = db.Database.SqlQuery<int?>("IsInventoryCycleAutoRTP @WarehouseId", new SqlParameter("@WarehouseId", warehouesDC.WarehouseId)).ToList();
                                    if (OrderIds != null && OrderIds.Any() && warehouesDC.IsInventoryCycleAutoRTPConfirm == false)
                                    {
                                        result.IsInventoryCycleAutoRTPFound = true;
                                        result.Message = "On your Confirmation following OrderIds: status will be change from ReadyToPick to pending.";
                                        result.OrderIds = string.Join(",", OrderIds);
                                        return result;
                                    }
                                    if (OrderIds != null && OrderIds.Any() && warehouesDC.IsInventoryCycleAutoRTPConfirm)
                                    {
                                        var Orders = db.DbOrderMaster.Where(x => OrderIds.Contains(x.OrderId) && x.WarehouseId == warehouesDC.WarehouseId && x.Deleted == false).Include(x => x.orderDetails).ToList();
                                        var oid = Orders.Where(x => x.Status == "ReadyToPick").Select(x => x.OrderId).Distinct().ToList();
                                        var ReadyToPickOrderDetaillist = db.ReadyToPickOrderDetailDb.Where(x => oid.Contains(x.OrderId)).ToList();

                                        var roid = ReadyToPickOrderDetaillist.Select(x => x.OrderId).Distinct().ToList();
                                        if (oid.Count() != roid.Count())
                                        {
                                            result.Message = "Order RTP Vs PRTD Count Not Matched :" + oid.Count() + " Vs " + roid.Count();
                                            return result;

                                        }
                                        var longoid = oid.Select(x => Convert.ToInt64(x)).Distinct().ToList();

                                        var query = (from d in db.TripPlannerConfirmedDetails
                                                     join o in db.TripPlannerConfirmedOrders on d.Id equals o.TripPlannerConfirmedDetailId
                                                     where longoid.Contains((int)o.OrderId) && o.IsActive == true && o.IsDeleted == false
                                                     select d.TripPlannerConfirmedMasterId).ToList();


                                        MultiStockHelper<OnPickedCancelDc> MultiStockHelpers = new MultiStockHelper<OnPickedCancelDc>();
                                        List<OnPickedCancelDc> RTDOnPickedCancelList = new List<OnPickedCancelDc>();
                                        List<OrderMasterHistories> OrderHistoryList = new List<OrderMasterHistories>();
                                        foreach (var Order in Orders.Where(x => x.Status == "ReadyToPick").Distinct())
                                        {
                                            foreach (var item in Order.orderDetails)
                                            {
                                                item.Status = "Pending";
                                                item.UpdatedDate = indianTime;
                                            }

                                            Order.Status = "Pending";
                                            Order.UpdatedDate = indianTime;
                                            db.Entry(Order).State = EntityState.Modified;

                                            #region Order History
                                            OrderMasterHistories h1 = new OrderMasterHistories();
                                            h1.orderid = Order.OrderId;
                                            h1.Status = Order.Status;
                                            h1.Reasoncancel = null;
                                            h1.Warehousename = Order.WarehouseName;
                                            if (people.DisplayName != null)
                                            {
                                                h1.username = people.DisplayName;
                                            }
                                            else
                                            {
                                                h1.username = people.PeopleFirstName;
                                            }
                                            h1.userid = userid;
                                            h1.Description = "DueToInventoryCycle";
                                            h1.CreatedDate = indianTime;
                                            OrderHistoryList.Add(h1);
                                            #endregion

                                            #region stock Hit
                                            foreach (var StockHit in Order.orderDetails.Where(x => x.qty > 0))
                                            {
                                                int qty = ReadyToPickOrderDetaillist.Any(x => x.OrderDetailsId == StockHit.OrderDetailsId) ? ReadyToPickOrderDetaillist.FirstOrDefault(x => x.OrderDetailsId == StockHit.OrderDetailsId).Qty : 0;
                                                if (qty > 0)
                                                {
                                                    bool isfree = false;
                                                    string RefStockCode = Order.OrderType == 8 ? "CL" : "C";
                                                    if (StockHit.IsFreeItem && StockHit.IsDispatchedFreeStock)
                                                    {
                                                        RefStockCode = "F";
                                                        isfree = true;
                                                    }
                                                    RTDOnPickedCancelList.Add(new OnPickedCancelDc
                                                    {
                                                        ItemMultiMRPId = StockHit.ItemMultiMRPId,
                                                        OrderDispatchedDetailsId = StockHit.OrderDetailsId,
                                                        OrderId = StockHit.OrderId,
                                                        Qty = qty,
                                                        UserId = userid,
                                                        WarehouseId = StockHit.WarehouseId,
                                                        IsFreeStock = isfree,
                                                        RefStockCode = RefStockCode
                                                    });
                                                }
                                            }
                                            #endregion

                                        }

                                        if (RTDOnPickedCancelList != null && RTDOnPickedCancelList.Any())
                                        {
                                            bool res = MultiStockHelpers.MakeEntry(RTDOnPickedCancelList, "Stock_OnPickedCancel", db, dbContextTransaction);
                                            if (!res)
                                            {
                                                dbContextTransaction.Dispose();
                                                result.Message = "Inventory not reverted on Auto Picker Canceled(InventoryCycle)";
                                                return result;
                                            }
                                            #region BatchCode
                                            foreach (var s in RTDOnPickedCancelList.Where(x => x.Qty > 0))
                                            {
                                                PublisherPickerRejectStockList.Add(new BatchCodeSubjectDc
                                                {
                                                    ObjectDetailId = s.OrderDispatchedDetailsId,  // its OrderDetailsId
                                                    ObjectId = s.OrderId,
                                                    StockType = s.RefStockCode,
                                                    Quantity = s.Qty,
                                                    WarehouseId = s.WarehouseId,
                                                    ItemMultiMrpId = s.ItemMultiMRPId
                                                });
                                            }
                                            #endregion
                                            db.OrderMasterHistoriesDB.AddRange(OrderHistoryList);
                                            if (query != null && query.Any())
                                            {
                                                var idlist = new DataTable();
                                                idlist.Columns.Add("IntValue");
                                                foreach (var item in RTDOnPickedCancelList.Select(x => x.OrderId).Distinct().ToList())
                                                {
                                                    var dr = idlist.NewRow();
                                                    dr["IntValue"] = item;
                                                    idlist.Rows.Add(dr);
                                                }
                                                var param = new SqlParameter("@OrderIdList", idlist);
                                                param.SqlDbType = SqlDbType.Structured;
                                                param.TypeName = "dbo.IntValues";
                                                int rcount = db.Database.ExecuteSqlCommand("RemoveAllOrdersFromTrips @OrderIdList", param);
                                                //if (rcount > 0) { }
                                                //else
                                                //{
                                                //    dbContextTransaction.Dispose();
                                                //    result.Message = "Inventory not reverted on Auto Picker order not Removed from Trips";
                                                //    return result;
                                                //}
                                            }
                                        }
                                    }
                                    else if (OrderIds != null && OrderIds.Any() && warehouesDC.IsInventoryCycleAutoRTPConfirm == false)
                                    {
                                        result.IsInventoryCycleAutoRTPFound = true;
                                        result.Message = "On your Confirmation following OrderIds:" + string.Join(",", OrderIds) + " status will be change from ReadyToPick to pending.";
                                        return result;
                                    }

                                    var warehouseid = new SqlParameter("@WarehouseId", warehouesDC.WarehouseId);
                                    var BatchMismatchData = db.Database.SqlQuery<BatchMismatchDTO>("EXEC CurrentStockBatchDiff @WarehouseId", warehouseid).ToList();
                                    if (BatchMismatchData.Any() && BatchMismatchData.Count() > 0)
                                    {
                                        result.Message = "Can't Start Inventory Cycle, Please First Clear Batch Mismatch.Contact to IT Team.";
                                        result.BatchMismatchData = BatchMismatchData;
                                        return result;
                                    }

                                }
                                //if (inventCycle.Count > 0)
                                //{
                                //    foreach (var pendingInventory in inventCycle)
                                //    {
                                //        pendingInventory.IsDeleted = true;
                                //        pendingInventory.UpadtedBy = people.DisplayName;
                                //        pendingInventory.UpdatedDate = DateTime.Now;
                                //    }
                                //    db.InventCycleDb.AddRange(inventCycle);
                                //}
                                List<CurrentStock> currentstockList = null;
                                var inventCycleMultiMrpids = inventCycle.Select(x => x.ItemMultiMrpId).Distinct().ToList();
                                var data = db.DbCurrentStock.Where(c => c.Deleted == false && c.WarehouseId == warehouesDC.WarehouseId).ToList();
                                currentstockList = data.Where(c => inventCycleMultiMrpids.Contains(c.ItemMultiMRPId)).ToList();

                                if (inventCycle.Count > 0)
                                {
                                    foreach (var ic in inventCycle)
                                    {
                                        var currentstock = currentstockList.FirstOrDefault(x => x.ItemMultiMRPId == ic.ItemMultiMrpId && x.WarehouseId == ic.WarehouseId);
                                        List<InventCycleBatch> inventCycleBatches = db.InventCycleBatchDb.Where(x => x.InventCycle_Id == ic.Id).ToList();
                                        if (currentstock.CurrentInventory == inventCycleBatches.Sum(x => x.InventoryCount))
                                        {
                                            //for same value in batch
                                            var batchData = inventCycleBatches.Where(x => ((x.PastInventory ?? 0)) == x.InventoryCount).ToList();
                                            if (batchData.Count > 0)
                                            {

                                            }
                                            else
                                            {
                                                result.Message = "Clear InventoryCycle First!";
                                                result.Status = true;
                                            }
                                            //foreach (var batchData in inventCycleBatches.Where(x => ((x.PastInventory ?? 0)) == x.InventoryCount).ToList())
                                            //{

                                            //}
                                        }
                                        else
                                        {
                                            if (inventCycle.Count > 0)
                                            {
                                                result.Message = "Clear InventoryCycle First!";
                                                result.Status = true;
                                            }
                                        }
                                    }
                                }

                                #region PvRecocillationHistory
                                if (data.Count > 0 && data.Any())
                                {
                                    List<PvReconcillationHistory> pvReconcillationHistories = data.Select(x => new PvReconcillationHistory
                                    {
                                        StockId = x.StockId,
                                        ItemName = x.itemname,
                                        CurrentInventory = x.CurrentInventory,
                                        MRP = x.MRP,
                                        WarehouseId = x.WarehouseId,
                                        WarehouseName = x.WarehouseName,
                                        ItemMultiMRPId = x.ItemMultiMRPId,
                                        ItemNumber = x.ItemNumber,
                                        Status = "Pending",
                                        IsActive = true,
                                        IsDeleted = false,
                                        CreatedBy = userid,
                                        CreatedDate = DateTime.Now,
                                        ModifiedBy = userid,
                                        ModifiedDate = DateTime.Now
                                    }).ToList();

                                    db.PvReconcillationHistoryDB.AddRange(pvReconcillationHistories);
                                }
                                #endregion

                                warehouse.IsPV = true;
                                warehouse.IsStopCurrentStockTrans = true;
                                warehouse.InventoryCycleEditby = userid;
                                warehouse.TransactionType = "PV";
                                db.Entry(warehouse).State = EntityState.Modified;
                                db.DBWarehousePVHistory.Add(new WarehousePVHistory()
                                {
                                    CreatedDate = DateTime.Now,
                                    WarehouseId = warehouse.WarehouseId,
                                    IsPV = true,
                                    CreatedBy = userid
                                });
                                InventoryCycleHistory add = new InventoryCycleHistory();
                                add.WarehoueId = warehouesDC.WarehouseId;
                                add.IsStopCurrentStockTrans = warehouesDC.IsStopCurrentStockTrans;
                                add.TransactionType = warehouse.TransactionType;
                                add.CreatedDate = indianTime;
                                add.ModifiedDate = indianTime;
                                add.IsActive = true;
                                add.IsDeleted = false;
                                add.CreatedBy = userid;
                                add.ModifiedBy = userid;
                                db.InventoryCycleHistoryDB.Add(add);
                                db.Commit();
                                result.Message = "PV Started Successfully!";
                                result.Status = true;
                                //return result;
                                result.Message = "Inventory For This Warehouse Started Successfully!";
                            }
                            else
                            {
                                result.Message = "You are not Authorize!";
                                return result;
                            }
                        }
                        if (result.Status)
                        {
                            dbContextTransaction.Complete();
                        }
                    }
                    if (PublisherPickerRejectStockList != null && PublisherPickerRejectStockList.Any() && result.Status)
                    {
                        Publisher.PlannedRejectPublish(PublisherPickerRejectStockList);
                    }
                }
            }
            return result;
        }
        #endregion

        #region Old StartPVwarehouseInventory API
        //[Route("StartPVwarehouseInventory")]//For Start PV
        //[HttpGet]
        //public response StartPVwarehouseInventory(int WarehouseId)
        //{
        //    using (var db = new AuthContext())
        //    {
        //        var identity = User.Identity as ClaimsIdentity;
        //        int userid = 0;

        //        if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
        //            userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
        //        DateTime AssignDate = DateTime.Now.Date;
        //        response result = new response { msg = "", Status = false };
        //        Warehouse warehouse = db.Warehouses.Where(x => x.WarehouseId == WarehouseId && x.Deleted == false && x.active == true && x.IsStopCurrentStockTrans == true).FirstOrDefault();
        //        InventCycle inventCycle = db.InventCycleDb.Where(x => x.WarehouseId == WarehouseId && x.IsDeleted == false && EntityFunctions.TruncateTime(x.CreatedDate) == EntityFunctions.TruncateTime(AssignDate) && x.IsWarehouseApproved == 0).FirstOrDefault();
        //        if (warehouse != null)
        //        {
        //            if (inventCycle != null)
        //            {
        //                result.msg = "Clear InventoryCycle First!";
        //                result.Status = true;
        //                return result;
        //            }
        //            else
        //            {
        //                warehouse.IsPV = true;
        //                db.Entry(warehouse).State = EntityState.Modified;
        //                db.DBWarehousePVHistory.Add(new WarehousePVHistory()
        //                {
        //                    CreatedDate = DateTime.Now,
        //                    WarehouseId = warehouse.WarehouseId,
        //                    IsPV = true,
        //                    CreatedBy = userid
        //                });
        //                db.Commit();
        //                result.msg = "PV Started Successfully!";
        //                result.Status = true;
        //                return result;
        //            }
        //        }
        //        else
        //        {
        //            result.msg = "";
        //            result.Status = false;
        //            return result;
        //        }
        //    }
        //}
        #endregion

        [Route("StopPVwarehouseInventory")]//For Stop PV
        [HttpGet]
        public response StopPVwarehouseInventory(int WarehouseId)
        {
            response result = new response { msg = "", Status = false };
            result.msg = "Currently PV not working!";
            result.Status = false;
            //return result;
            using (var db = new AuthContext())
            {
                var identity = User.Identity as ClaimsIdentity;
                int userid = 0;

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                    userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

                DateTime AssignDate = DateTime.Now.Date;

                List<InventCycle> inventCycle = db.InventCycleDb.Where(x => x.WarehouseId == WarehouseId && x.IsDeleted == false && EntityFunctions.TruncateTime(x.CreatedDate) == EntityFunctions.TruncateTime(AssignDate) && x.IsWarehouseApproved == 0 && x.IsPV == true).ToList();
                List<CurrentStock> currentstockList = null;
                var inventCycleMultiMrpids = inventCycle.Select(x => x.ItemMultiMrpId).Distinct().ToList();
                currentstockList = db.DbCurrentStock.Where(c => inventCycleMultiMrpids.Contains(c.ItemMultiMRPId) && c.Deleted == false && c.WarehouseId == WarehouseId).ToList();

                if (inventCycle.Count > 0)
                {
                    foreach (var ic in inventCycle)
                    {
                        var currentstock = currentstockList.FirstOrDefault(x => x.ItemMultiMRPId == ic.ItemMultiMrpId && x.WarehouseId == ic.WarehouseId);
                        List<InventCycleBatch> inventCycleBatches = db.InventCycleBatchDb.Where(x => x.InventCycle_Id == ic.Id).ToList();
                        if (currentstock.CurrentInventory == inventCycleBatches.Sum(x => x.InventoryCount))
                        {
                            //for same value in batch
                            var batchData = inventCycleBatches.Where(x => ((x.PastInventory ?? 0)) == x.InventoryCount).ToList();
                            if (batchData.Count > 0)
                            {

                            }
                            else
                            {
                                result.msg = "Clear PV Pending Request First!";
                                result.Status = true;
                            }
                        }
                        else
                        {
                            if (inventCycle.Count > 0)
                            {
                                result.msg = "Clear PV Pending Request First!";
                                result.Status = true;
                            }
                        }
                    }
                }
                //if (inventCycle != null)
                //{
                //    result.msg = "Can't Stop PV Inventory First Approve or Reject Todays Inventory!";
                //    result.Status = false;
                //    return result;
                //}
                Warehouse warehouse = db.Warehouses.Where(x => x.WarehouseId == WarehouseId && x.Deleted == false && x.active == true).FirstOrDefault();
                if (warehouse != null)
                {
                    warehouse.IsPV = false;
                    warehouse.IsStopCurrentStockTrans = false;
                    db.Entry(warehouse).State = EntityState.Modified;
                    db.DBWarehousePVHistory.Add(new WarehousePVHistory()
                    {
                        CreatedDate = DateTime.Now,
                        WarehouseId = warehouse.WarehouseId,
                        IsPV = false,
                        CreatedBy = userid
                    });
                    db.Commit();
                }
                result.msg = "PV Inventory Stopped succesfully!";
                result.Status = true;
                return result;
            }
        }

        [Route("getWarehousePVHistory")]
        [HttpGet]
        public List<WarehousePVHistoryDC> getWarehousePVHistory(int WarehouseId)
        {
            using (AuthContext db = new AuthContext())
            {
                List<WarehousePVHistoryDC> warehousePVHistoryDCs = (from c in db.DBWarehousePVHistory
                                                                    where c.WarehouseId == WarehouseId
                                                                    join cc in db.Warehouses on c.WarehouseId equals cc.WarehouseId
                                                                    join cs in db.Peoples on c.CreatedBy equals cs.PeopleID

                                                                    select new WarehousePVHistoryDC
                                                                    {
                                                                        WarehouseName = cc.WarehouseName,
                                                                        IsPV = c.IsPV,
                                                                        CreatedDate = c.CreatedDate,
                                                                        CreatedBy = cs.DisplayName
                                                                    }).OrderByDescending(x => x.CreatedDate).ToList();
                return warehousePVHistoryDCs;
            }
        }

        [Route("IsPVwarehouseInventory")]//For Android
        [HttpGet]
        public bool IsPVwarehouseInventory(int WarehouseId)
        {
            var result = false;
            using (var db = new AuthContext())
            {
                result = db.Warehouses.Any(x => x.WarehouseId == WarehouseId && x.Deleted == false && x.active == true && x.IsPV == true);

            }
            return result;
        }

        //Get: InventoryCycleStatistics
        [Route("GetInventoryCycleStatistics")]
        [HttpGet]
        [AllowAnonymous]
        public async Task<InventoryCycleStatisticsDC> InventoryCycleStatistics(int WarehouseId, bool IsHQ, DateTime? start, DateTime? end, bool IsPV)
        {
            InventoryCycleStatisticsDC res = new InventoryCycleStatisticsDC();
            if (WarehouseId > 0)
            {
                using (var myContext = new AuthContext())
                {
                    if (myContext.Database.Connection.State != ConnectionState.Open)
                        myContext.Database.Connection.Open();
                    myContext.Database.CommandTimeout = 120;
                    var stmt = myContext.Database.Connection.CreateCommand();
                    stmt.CommandText = "[dbo].[GetInventoryCycleStatistics]";
                    stmt.CommandType = System.Data.CommandType.StoredProcedure;
                    stmt.Parameters.Add(new SqlParameter("@WarehouseId", SqlDbType.Int) { Value = WarehouseId });
                    stmt.Parameters.Add(new SqlParameter("@IsHQ", SqlDbType.Bit) { Value = IsHQ });
                    stmt.Parameters.Add(new SqlParameter("@start", start ?? (object)DBNull.Value));
                    stmt.Parameters.Add(new SqlParameter("@end", end ?? (object)DBNull.Value));
                    stmt.Parameters.Add(new SqlParameter("@IsPV", SqlDbType.Bit) { Value = IsPV });

                    var reader = stmt.ExecuteReader();
                    res = ((IObjectContextAdapter)myContext)
                    .ObjectContext
                    .Translate<InventoryCycleStatisticsDC>(reader).FirstOrDefault();
                    /*
                    reader.NextResult();
                    if (reader.Read())
                    {
                        res.CountedLineItem = Convert.ToInt32(reader["CountedLineItem"]);
                    }
                    reader.NextResult();
                    if (reader.Read())
                    {
                        res.PendingLineItem = Convert.ToInt32(reader["PendingLineItem"]);
                    }
                    reader.NextResult();
                    if (reader.Read())
                    {
                        res.ApprovedExcessItems = Convert.ToDouble(reader["ApprovedExcessItems"]);
                    }
                    reader.NextResult();
                    if (reader.Read())
                    {
                        res.ApprovedShortItems = Convert.ToDouble(reader["ApprovedShortItems"]);
                    }
                    */
                    res.ApprovedItemsDifference = res.ApprovedExcessItems - res.ApprovedShortItems;

                }
            }
            return res;
        }


        #region PostInventoryCycleItemFromBarCode

        [Route("GetPVItemScan")]
        [HttpGet]
        [AllowAnonymous]
        public List<BarcodeItemWithBatchDcList> GetPVItemScan(int warehouseId, string barcode, int PeopleId)
        {
            using (AuthContext context = new AuthContext())
            {
                var people = context.Peoples.FirstOrDefault(x => x.PeopleID == PeopleId);
                var warehouse = context.Warehouses.FirstOrDefault(x => x.WarehouseId == warehouseId);
                List<BarcodeItemWithBatchDcList> BarcodeItemWithBatchDcs = new List<BarcodeItemWithBatchDcList>();
                List<StockBatchDc> StockBatchDcs = new List<StockBatchDc>();
                List<StockBatchDc> AllBatchDcs = new List<StockBatchDc>();
                SupervisorInventoryCycleDay mongosupervisor = new SupervisorInventoryCycleDay();
                List<string> barcodeList = new List<string>();
                if (string.IsNullOrEmpty(barcode))
                {
                    return BarcodeItemWithBatchDcs;
                }

                if (context.Database.Connection.State != System.Data.ConnectionState.Open)
                    context.Database.Connection.Open();

                var cmd = context.Database.Connection.CreateCommand();
                cmd.CommandText = "[dbo].[GetItemWithBatchDetail]";
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.Parameters.Add(new SqlParameter("@barcode", barcode));
                cmd.Parameters.Add(new SqlParameter("@warehouseId", warehouseId));


                // Run the sproc
                var reader = cmd.ExecuteReader();
                BarcodeItemWithBatchDcs = ((IObjectContextAdapter)context)
                                         .ObjectContext
                                         .Translate<BarcodeItemWithBatchDcList>(reader).ToList();
                reader.NextResult();
                if (reader.HasRows)
                {
                    StockBatchDcs = ((IObjectContextAdapter)context)
                                    .ObjectContext
                                    .Translate<StockBatchDc>(reader).ToList();
                }
                reader.NextResult();
                if (reader.HasRows)
                {
                    AllBatchDcs = ((IObjectContextAdapter)context)
                                    .ObjectContext
                                    .Translate<StockBatchDc>(reader).ToList();
                }


                context.Database.Connection.Close();
                if (BarcodeItemWithBatchDcs != null && BarcodeItemWithBatchDcs.Any())
                {
                    DateTime AssignDate = DateTime.Now.Date;
                    MongoDbHelper<SupervisorInventoryCycleDay> mongoDbHelper = new MongoDbHelper<SupervisorInventoryCycleDay>();
                    var inventoryPredicate = PredicateBuilder.New<SupervisorInventoryCycleDay>(x => x.WarehouseId == warehouseId && x.AssignDate == AssignDate && x.PeopleId == PeopleId);
                    inventoryPredicate = inventoryPredicate.And(x => x.IsPV.HasValue && x.IsPV.Value == true);
                    mongosupervisor = (mongoDbHelper.Select(inventoryPredicate)).FirstOrDefault();

                    foreach (var item in BarcodeItemWithBatchDcs)
                    {

                        if (AllBatchDcs.Any())
                        {
                            item.BatchDcs = new List<StockBatchDc>();
                            item.BatchDcs.AddRange(AllBatchDcs.Select(x => new StockBatchDc
                            {
                                BatchCode = x.BatchCode,
                                BatchMasterId = x.BatchMasterId,
                                ExpiryDate = x.ExpiryDate,
                                ItemMultiMrpId = item.ItemMultiMrpId,
                                ItemNumber = x.ItemNumber,
                                MFGDate = x.MFGDate,
                                Qty = StockBatchDcs.Any(y => y.ItemMultiMrpId == item.ItemMultiMrpId && y.BatchMasterId == x.BatchMasterId) ? StockBatchDcs.FirstOrDefault(y => y.ItemMultiMrpId == item.ItemMultiMrpId && y.BatchMasterId == x.BatchMasterId).Qty : 0,
                                StockBatchId = StockBatchDcs.Any(y => y.ItemMultiMrpId == item.ItemMultiMrpId && y.BatchMasterId == x.BatchMasterId) ? StockBatchDcs.FirstOrDefault(y => y.ItemMultiMrpId == item.ItemMultiMrpId && y.BatchMasterId == x.BatchMasterId).StockBatchId : 0
                            }));
                        }
                        bool IsPV = false;
                        IsPV = context.Warehouses.Any(x => x.WarehouseId == warehouseId && x.IsPV);
                        var inventoryCycle = context.InventCycleDb.FirstOrDefault(x => x.WarehouseId == warehouseId && x.ItemMultiMrpId == item.ItemMultiMrpId && EntityFunctions.TruncateTime(x.CreatedDate) == EntityFunctions.TruncateTime(AssignDate) && x.IsPV == IsPV);
                        if (inventoryCycle == null)
                        {
                            var classification = BarcodeItemWithBatchDcs.FirstOrDefault().ABCClassification;
                            var inventoryCycleMaster = context.InventoryCycleMasterDb.FirstOrDefault(x => x.IsComplete == false && x.WarehouseId == warehouseId && x.Classification == classification);
                            if (inventoryCycleMaster == null)
                            {
                                inventoryCycleMaster = new InventoryCycleMaster
                                {
                                    Classification = classification,
                                    CreatedDate = DateTime.Now,
                                    IsComplete = false,
                                    WarehouseId = warehouseId
                                };
                                context.InventoryCycleMasterDb.Add(inventoryCycleMaster);
                                context.Commit();
                            }
                            inventoryCycle = new InventCycle
                            {
                                InventoryCycleMasterId = inventoryCycleMaster.InventoryCycleMasterId,
                                CreatedDate = DateTime.Now,
                                CurrentInventory = 0,
                                DamagedQty = 0,
                                InventoryCount = 0,
                                IsApproved = false,
                                IsDeleted = false,
                                IsPV = true,
                                IsWarehouseApproved = -1,
                                NonSellableQty = 0,
                                ItemMultiMrpId = item.ItemMultiMrpId,
                                PastInventory = 0,
                                RtdCount = 0,
                                RtpCount = 0,
                                WarehouseApproveId = 0,
                                WarehouseId = warehouseId,
                                InventCycleBatchs = item.BatchDcs.Select(y => new InventCycleBatch
                                {
                                    BatchMasterId = y.BatchMasterId,
                                    DamagedQty = 0,
                                    InventoryCount = 0,
                                    IsStockUpdated = false,
                                    NonSellableQty = 0,
                                    PastInventory = 0,
                                    Rtp = 0,
                                    StockBatchId = y.StockBatchId,
                                }).Distinct().ToList()

                            };
                            context.InventCycleDb.Add(inventoryCycle);
                            context.Commit();
                        }


                        if (mongosupervisor != null)
                        {
                            if (!mongosupervisor.InventoryCycleItems.Any(x => x.ItemMultiMRPId == inventoryCycle.ItemMultiMrpId))
                            {
                                mongosupervisor.InventoryCycleItems.Add(new SupervisorInventoryCycleItem
                                {
                                    Id = inventoryCycle.Id,
                                    NonSellableQty = 0,
                                    ABCClassification = item.ABCClassification,
                                    Barcode = new List<string>(),
                                    DamagedQty = 0,
                                    InventoryCount = 0,
                                    ItemMultiMRPId = item.ItemMultiMrpId,
                                    ItemName = item.ItemName,
                                    ItemNumber = item.ItemNumber,
                                    MRP = item.MRP
                                });
                                mongoDbHelper.Replace(mongosupervisor.Id, mongosupervisor);
                            }

                        }
                        else
                        {
                            mongosupervisor = new SupervisorInventoryCycleDay
                            {
                                AssignDate = AssignDate.Date,
                                DisplayName = people.DisplayName,
                                PeopleId = people.PeopleID,
                                WarehouseId = people.WarehouseId,
                                WarehouseName = warehouse.WarehouseName,
                                IsSubmitted = false,
                                IsPV = true,
                                InventoryCycleItems = BarcodeItemWithBatchDcs.Where(x => x.ItemMultiMrpId == item.ItemMultiMrpId).Select(x => new SupervisorInventoryCycleItem
                                {
                                    Id = inventoryCycle.Id,
                                    NonSellableQty = 0,
                                    ABCClassification = x.ABCClassification,
                                    Barcode = new List<string>(),
                                    DamagedQty = 0,
                                    InventoryCount = 0,
                                    ItemMultiMRPId = x.ItemMultiMrpId,
                                    ItemName = x.ItemName,
                                    ItemNumber = x.ItemNumber,
                                    MRP = x.MRP
                                }).ToList()
                            };

                            mongoDbHelper.InsertAsync(mongosupervisor);
                        }

                        var mongoInventoryItems = mongosupervisor.InventoryCycleItems.FirstOrDefault(x => x.ItemMultiMRPId == inventoryCycle.ItemMultiMrpId);

                        if (mongoInventoryItems.ItemBatch != null && mongoInventoryItems.ItemBatch.Any())
                        {
                            item.ItemBatch = mongoInventoryItems.ItemBatch.Select(x => new ItemBatchDc
                            {
                                AvailableQty = item.BatchDcs.FirstOrDefault(y => y.BatchMasterId == x.BatchMasterId).Qty,
                                BatchCode = x.BatchCode,
                                BatchMasterId = x.BatchMasterId,
                                InventoryCount = x.InventoryCount,
                                ItemMultiMRPId = inventoryCycle.ItemMultiMrpId,
                                StockBatchId = x.StockBatchId,
                                ExpiryDate = x.ExpiryDate,
                                MFGDate = x.MFGDate,
                                DamagedImageUrl = "",
                                DamagedQty = 0,
                                NonSellableImageUrl = "",
                                NonSellableQty = 0
                            }).ToList();

                            var BatchMasterIds = item.ItemBatch != null && item.ItemBatch.Any() ? item.ItemBatch.Select(x => x.BatchMasterId).ToList() : new List<long>();
                            AllBatchDcs = item.BatchDcs.Where(x => x.ItemNumber == item.ItemNumber).Distinct().ToList();
                            item.BatchDcs = item.BatchDcs.Where(x => !BatchMasterIds.Contains(x.BatchMasterId) && x.ItemNumber == item.ItemNumber).Distinct().ToList();
                        }
                        item.Id = inventoryCycle.Id;
                        item.AllItemBatch = new List<ItemBatchDc>();
                        if (item.ItemBatch != null && item.ItemBatch.Any())
                        {
                            item.AllItemBatch.AddRange(item.ItemBatch.Select(x => new ItemBatchDc
                            {
                                AvailableQty = AllBatchDcs.FirstOrDefault(y => y.BatchMasterId == x.BatchMasterId).Qty,
                                BatchCode = x.BatchCode,
                                BatchMasterId = x.BatchMasterId,
                                InventoryCount = x.InventoryCount,
                                ItemMultiMRPId = inventoryCycle.ItemMultiMrpId,
                                StockBatchId = x.StockBatchId,
                                ExpiryDate = x.ExpiryDate,
                                MFGDate = x.MFGDate,
                                DamagedImageUrl = "",
                                DamagedQty = 0,
                                NonSellableImageUrl = "",
                                NonSellableQty = 0
                            }).ToList());
                        }
                        if (item.BatchDcs != null && item.BatchDcs.Any())
                        {
                            item.AllItemBatch.AddRange(item.BatchDcs.Where(x => x.Qty > 0).Select(x => new ItemBatchDc
                            {
                                AvailableQty = x.Qty,
                                BatchCode = x.BatchCode,
                                BatchMasterId = x.BatchMasterId,
                                InventoryCount = -1,
                                ItemMultiMRPId = inventoryCycle.ItemMultiMrpId,
                                StockBatchId = x.StockBatchId,
                                ExpiryDate = x.ExpiryDate,
                                MFGDate = x.MFGDate,
                                DamagedImageUrl = "",
                                DamagedQty = 0,
                                NonSellableImageUrl = "",
                                NonSellableQty = 0
                            }).ToList());
                        }

                    }
                }



                return BarcodeItemWithBatchDcs;
            }

        }

        [Route("PostInventoryCycleItemFromBarCode")]
        [HttpPost]
        public async Task<response> PostInventoryCycleItemFromBarCode(InventoryCycleItemBarcodeDc inventoryCycleItemBarcodeDc)
        {
            SupervisorInventoryCycleDayDC supervisorInventoryCycleDayDC = new SupervisorInventoryCycleDayDC();
            SupervisorInventoryCycleDay mongosupervisor = new SupervisorInventoryCycleDay();
            response result = new response();
            using (AuthContext context = new AuthContext())
            {
                if (inventoryCycleItemBarcodeDc.WarehouseId > 0 && inventoryCycleItemBarcodeDc.ItemMultiMrpId > 0)
                {
                    var people = context.Peoples.FirstOrDefault(x => x.PeopleID == inventoryCycleItemBarcodeDc.PeopleId);
                    var warehouse = context.Warehouses.FirstOrDefault(x => x.WarehouseId == inventoryCycleItemBarcodeDc.WarehouseId);
                    List<BarcodeItemWithBatchDc> BarcodeItemWithBatchDcs = new List<BarcodeItemWithBatchDc>();
                    List<StockBatchDc> StockBatchDcs = new List<StockBatchDc>();
                    if (context.Database.Connection.State != System.Data.ConnectionState.Open)
                        context.Database.Connection.Open();

                    var cmd = context.Database.Connection.CreateCommand();
                    cmd.CommandText = "[dbo].[GetItemWithBatchDetailByMRPId]";
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@itemMultiMrpId", inventoryCycleItemBarcodeDc.ItemMultiMrpId));
                    cmd.Parameters.Add(new SqlParameter("@warehouseId", inventoryCycleItemBarcodeDc.WarehouseId));


                    // Run the sproc
                    var reader = cmd.ExecuteReader();
                    BarcodeItemWithBatchDcs = ((IObjectContextAdapter)context)
                                             .ObjectContext
                                             .Translate<BarcodeItemWithBatchDc>(reader).ToList();
                    reader.NextResult();
                    if (reader.HasRows)
                    {
                        StockBatchDcs = ((IObjectContextAdapter)context)
                                        .ObjectContext
                                        .Translate<StockBatchDc>(reader).ToList();
                    }

                    if (BarcodeItemWithBatchDcs != null && BarcodeItemWithBatchDcs.Any())
                    {
                        foreach (var item in BarcodeItemWithBatchDcs)
                        {
                            if (StockBatchDcs != null && StockBatchDcs.Any(x => x.ItemMultiMrpId == item.ItemMultiMrpId))
                            {
                                item.StockBatchDcs = StockBatchDcs.Where(x => x.ItemMultiMrpId == item.ItemMultiMrpId).ToList();
                            }
                        }
                    }
                    context.Database.Connection.Close();
                    DateTime AssignDate = DateTime.Now.Date;
                    var inventoryCycle = await context.InventCycleDb.FirstOrDefaultAsync(x => x.WarehouseId == inventoryCycleItemBarcodeDc.WarehouseId && x.ItemMultiMrpId == inventoryCycleItemBarcodeDc.ItemMultiMrpId && EntityFunctions.TruncateTime(x.CreatedDate) == EntityFunctions.TruncateTime(AssignDate));

                    if (inventoryCycle == null)
                    {
                        var classification = BarcodeItemWithBatchDcs.FirstOrDefault().ABCClassification;
                        var inventoryCycleMaster = await context.InventoryCycleMasterDb.FirstOrDefaultAsync(x => x.IsComplete == false && x.WarehouseId == inventoryCycleItemBarcodeDc.WarehouseId && x.Classification == classification);
                        if (inventoryCycleMaster == null)
                        {
                            inventoryCycleMaster = new InventoryCycleMaster
                            {
                                Classification = classification,
                                CreatedDate = DateTime.Now,
                                IsComplete = false,
                                WarehouseId = inventoryCycleItemBarcodeDc.WarehouseId
                            };
                            context.InventoryCycleMasterDb.Add(inventoryCycleMaster);
                            context.Commit();
                        }
                        inventoryCycle = new InventCycle
                        {
                            InventoryCycleMasterId = inventoryCycleMaster.InventoryCycleMasterId,
                            CreatedDate = DateTime.Now,
                            CurrentInventory = 0,
                            DamagedQty = 0,
                            InventoryCount = 0,
                            IsApproved = false,
                            IsDeleted = false,
                            IsPV = true,
                            IsWarehouseApproved = -1,
                            NonSellableQty = 0,
                            ItemMultiMrpId = BarcodeItemWithBatchDcs.FirstOrDefault().ItemMultiMrpId,
                            PastInventory = 0,
                            RtdCount = 0,
                            RtpCount = 0,
                            WarehouseApproveId = 0,
                            WarehouseId = inventoryCycleItemBarcodeDc.WarehouseId,
                            InventCycleBatchs = BarcodeItemWithBatchDcs.FirstOrDefault().StockBatchDcs.Select(y => new InventCycleBatch
                            {
                                BatchMasterId = y.BatchMasterId,
                                DamagedQty = 0,
                                InventoryCount = 0,
                                IsStockUpdated = false,
                                NonSellableQty = 0,
                                PastInventory = 0,
                                Rtp = 0,
                                StockBatchId = y.StockBatchId,
                            }).ToList()

                        };
                        context.InventCycleDb.Add(inventoryCycle);
                        context.Commit();
                    }


                    MongoDbHelper<SupervisorInventoryCycleDay> mongoDbHelper = new MongoDbHelper<SupervisorInventoryCycleDay>();
                    var inventoryPredicate = PredicateBuilder.New<SupervisorInventoryCycleDay>(x => x.WarehouseId == inventoryCycleItemBarcodeDc.WarehouseId && x.AssignDate == AssignDate && x.PeopleId == inventoryCycleItemBarcodeDc.PeopleId);
                    inventoryPredicate = inventoryPredicate.And(x => x.IsPV.HasValue && x.IsPV.Value == true);
                    mongosupervisor = (await mongoDbHelper.SelectAsync(inventoryPredicate)).FirstOrDefault();
                    if (mongosupervisor != null)
                    {
                        if (!mongosupervisor.InventoryCycleItems.Any(x => x.ItemMultiMRPId == inventoryCycle.ItemMultiMrpId))
                        {
                            mongosupervisor.InventoryCycleItems.Add(new SupervisorInventoryCycleItem
                            {
                                Id = inventoryCycle.Id,
                                NonSellableQty = 0,
                                ABCClassification = BarcodeItemWithBatchDcs.FirstOrDefault().ABCClassification,
                                Barcode = new List<string>(),
                                DamagedQty = 0,
                                InventoryCount = 0,
                                ItemBatch = new List<InventoryCycleItemBatch>(),
                                ItemMultiMRPId = BarcodeItemWithBatchDcs.FirstOrDefault().ItemMultiMrpId,
                                ItemName = BarcodeItemWithBatchDcs.FirstOrDefault().ItemName,
                                ItemNumber = BarcodeItemWithBatchDcs.FirstOrDefault().ItemNumber,
                                MRP = BarcodeItemWithBatchDcs.FirstOrDefault().MRP

                            });
                            result.Status = await mongoDbHelper.ReplaceAsync(mongosupervisor.Id, mongosupervisor);
                        }
                    }
                    else
                    {
                        mongosupervisor = new SupervisorInventoryCycleDay
                        {
                            AssignDate = AssignDate.Date,
                            DisplayName = people.DisplayName,
                            PeopleId = people.PeopleID,
                            WarehouseId = people.WarehouseId,
                            WarehouseName = warehouse.WarehouseName,
                            InventoryCycleItems = BarcodeItemWithBatchDcs.Select(x => new SupervisorInventoryCycleItem
                            {
                                Id = inventoryCycle.Id,
                                NonSellableQty = 0,
                                ABCClassification = x.ABCClassification,
                                Barcode = new List<string>(),
                                DamagedQty = 0,
                                InventoryCount = 0,
                                ItemBatch = new List<InventoryCycleItemBatch>(),
                                ItemMultiMRPId = x.ItemMultiMrpId,
                                ItemName = x.ItemName,
                                ItemNumber = x.ItemNumber,
                                MRP = x.MRP
                            }).ToList()
                        };

                        result.Status = await mongoDbHelper.InsertAsync(mongosupervisor);
                    }

                    supervisorInventoryCycleDayDC.AssignDate = mongosupervisor.AssignDate;
                    supervisorInventoryCycleDayDC.DisplayName = mongosupervisor.DisplayName;
                    supervisorInventoryCycleDayDC.InventoryCycleItems = mongosupervisor.InventoryCycleItems;
                    supervisorInventoryCycleDayDC.IsSubmitted = mongosupervisor.IsSubmitted;
                    supervisorInventoryCycleDayDC.PeopleId = mongosupervisor.PeopleId;
                    supervisorInventoryCycleDayDC.WarehouseId = mongosupervisor.WarehouseId;
                    supervisorInventoryCycleDayDC.WarehouseName = mongosupervisor.WarehouseName;

                    var SupervisorInventoryCycleItems = mongosupervisor.InventoryCycleItems.Where(x => x.Id == inventoryCycle.Id).Select(x => new SupervisorInventoryCycleItemDc
                    {
                        Id = x.Id,
                        ABCClassification = x.ABCClassification,
                        Barcode = x.Barcode,
                        ItemMultiMRPId = x.ItemMultiMRPId,
                        ItemName = x.ItemName,
                        ItemNumber = x.ItemNumber,
                        MRP = x.MRP,
                        status = x.ItemBatch != null && x.ItemBatch.Any() ? 1 : 0,
                        ItemBatch = x.ItemBatch != null && x.ItemBatch.Any() ?
                      x.ItemBatch.Select(y => new InventoryCycleItemBatch
                      {
                          BatchCode = y.BatchCode,
                          BatchMasterId = y.BatchMasterId,
                          DamagedImageUrl = string.IsNullOrEmpty(y.DamagedImageUrl) ? "" : string.Format("{0}://{1}:{2}/{3}", new Uri(HttpContext.Current.Request.Url.AbsoluteUri).Scheme
                                                                    , HttpContext.Current.Request.Url.DnsSafeHost
                                                                    , (HttpContext.Current.Request.Url.Port != 80 && HttpContext.Current.Request.Url.Port != 443 ? ":" + HttpContext.Current.Request.Url.Port : "")
                                                                    , y.DamagedImageUrl),
                          DamagedQty = y.DamagedQty,
                          ExpiryDate = y.ExpiryDate,
                          InventoryCount = y.InventoryCount,
                          MFGDate = y.MFGDate,
                          NonSellableImageUrl = string.IsNullOrEmpty(y.NonSellableImageUrl) ? "" : string.Format("{0}://{1}:{2}/{3}", new Uri(HttpContext.Current.Request.Url.AbsoluteUri).Scheme
                                                                    , HttpContext.Current.Request.Url.DnsSafeHost
                                                                    , (HttpContext.Current.Request.Url.Port != 80 && HttpContext.Current.Request.Url.Port != 443 ? ":" + HttpContext.Current.Request.Url.Port : "")
                                                                    , y.NonSellableImageUrl),
                          NonSellableQty = y.NonSellableQty,
                          StockBatchId = y.StockBatchId
                      }).ToList()
                    : new List<InventoryCycleItemBatch>()
                    }).FirstOrDefault();

                    var BatchMasterIds = mongosupervisor.InventoryCycleItems.Where(x => x.Id == inventoryCycle.Id && x.ItemBatch != null && x.ItemBatch.Any()).SelectMany(x => x.ItemBatch).Select(x => x.BatchMasterId).ToList();

                    if (context.Database.Connection.State != ConnectionState.Open)
                        context.Database.Connection.Open();

                    var orderIdDt = new DataTable();
                    orderIdDt.Columns.Add("IntValue");

                    var dr = orderIdDt.NewRow();
                    dr["IntValue"] = SupervisorInventoryCycleItems.ItemMultiMRPId;
                    orderIdDt.Rows.Add(dr);

                    var param = new SqlParameter("itemMultiMrpId", orderIdDt);
                    param.SqlDbType = SqlDbType.Structured;
                    param.TypeName = "dbo.IntValues";
                    var cd = context.Database.Connection.CreateCommand();
                    cd.CommandText = "[dbo].[GetStockBatchMasterForInventoryCycle]";
                    cd.CommandType = System.Data.CommandType.StoredProcedure;
                    cd.Parameters.Add(new SqlParameter("@warehouseId", inventoryCycleItemBarcodeDc.WarehouseId));
                    cd.Parameters.Add(param);
                    cd.Parameters.Add(new SqlParameter("@itemnumber", SupervisorInventoryCycleItems.ItemNumber));
                    cd.Parameters.Add(new SqlParameter("@StockType", "C"));

                    // Run the sproc
                    var reader1 = cmd.ExecuteReader();
                    var BatchDcs = ((IObjectContextAdapter)context)
                     .ObjectContext
                     .Translate<InventoryCycleBatchDc>(reader1).ToList();

                    if (BatchMasterIds != null && BatchMasterIds.Any())
                        BatchDcs = BatchDcs.Where(x => !BatchMasterIds.Contains(x.BatchMasterId) && x.ItemMultiMrpId == SupervisorInventoryCycleItems.ItemMultiMRPId).ToList();

                    SupervisorInventoryCycleItems.BatchDcs = BatchDcs;
                    supervisorInventoryCycleDayDC.SupervisorInventoryCycleItemDc = SupervisorInventoryCycleItems;
                }

                if (result.Status)
                {
                    result.msg = "Inventory cycle updated successfully.";
                }
                else
                {
                    result.msg = "Inventory cycle not updated.";
                }
                return result;
            }


        }

        #endregion

        #region RejectedList
        [Route("GetSupervisorInventoryCycleRejectedItemList")]
        [HttpGet]
        public async Task<List<SupervisorInventoryCycleItemDc>> GetSupervisorInventoryCycleRejectedItemList(int WarehouseId, int PeopleId)
        {
            DateTime AssignDate = DateTime.Now.Date;
            List<SupervisorInventoryCycleItemDc> SupervisorInventoryCycleItems = new List<SupervisorInventoryCycleItemDc>();
            MongoDbHelper<SupervisorInventoryCycleDay> mongoDbHelper = new MongoDbHelper<SupervisorInventoryCycleDay>();
            var inventoryPredicate = PredicateBuilder.New<SupervisorInventoryCycleDay>(x => x.WarehouseId == WarehouseId && x.AssignDate == AssignDate && x.PeopleId == PeopleId && !x.IsSubmitted);
            inventoryPredicate = inventoryPredicate.And(x => x.IsPV.HasValue && x.IsPV.Value == true);
            var mongosupervisor = (await mongoDbHelper.SelectAsync(inventoryPredicate)).FirstOrDefault();
            if (mongosupervisor != null && mongosupervisor.InventoryCycleItems != null && mongosupervisor.InventoryCycleItems.Any())
            {
                if (!mongosupervisor.IsSubmitted)
                {
                    using (AuthContext context = new AuthContext())
                    {
                        var todayDate = DateTime.Now;
                        //var InventCycles = context.InventCycleDb.Where(x => x.IsWarehouseApproved == 2 && EntityFunctions.TruncateTime(x.CreatedDate.Value) >= EntityFunctions.TruncateTime(todayDate)).Include(x => x.InventCycleBatchs).ToList();
                        var inventoryCycleItems = mongosupervisor.InventoryCycleItems.Where(x => x.ItemBatch != null && x.ItemBatch.Any()).ToList();
                        int itemMultiMRPId = 0;
                        //if (InventCycles.Count > 0)
                        //{
                        //    itemMultiMRPId = InventCycles.FirstOrDefault(a => a.ItemMultiMrpId == ic.ItemMultiMRPId).ItemMultiMrpId;
                        //}
                        foreach (var ic in inventoryCycleItems)
                        {
                            bool isPV = context.InventCycleDb.FirstOrDefault(x => EntityFunctions.TruncateTime(x.CreatedDate.Value) == EntityFunctions.TruncateTime(todayDate) && x.ItemMultiMrpId == ic.ItemMultiMRPId && x.IsPV == true && x.Id == ic.Id).IsPV;
                            var InventCycles = context.InventCycleDb.Where(x => (x.IsWarehouseApproved == 2) && EntityFunctions.TruncateTime(x.CreatedDate.Value) == EntityFunctions.TruncateTime(todayDate) && x.ItemMultiMrpId == ic.ItemMultiMRPId && x.IsPV == isPV && x.Id == ic.Id).Include(x => x.InventCycleBatchs).ToList();
                            if (InventCycles.Count > 0)
                            {
                                //itemMultiMRPId = InventCycles.FirstOrDefault(a => a.ItemMultiMrpId != ic.ItemMultiMRPId).ItemMultiMrpId;

                                if (!mongosupervisor.IsSubmitted)
                                {
                                    SupervisorInventoryCycleItems.Add(new SupervisorInventoryCycleItemDc
                                    {
                                        ABCClassification = ic.ABCClassification,
                                        Barcode = ic.Barcode,
                                        ItemMultiMRPId = ic.ItemMultiMRPId,
                                        ItemName = ic.ItemName,
                                        ItemNumber = ic.ItemNumber,
                                        MRP = ic.MRP,
                                        status = ic.ItemBatch != null && ic.ItemBatch.Any() ? 1 : 0,
                                        ItemBatch = ic.ItemBatch,
                                        Id = ic.Id
                                    });
                                }
                            }
                            else
                            {
                            }

                        }
                        //SupervisorInventoryCycleItems = mongosupervisor.InventoryCycleItems.Where(x => x.IsRejected == true).Select(x => new SupervisorInventoryCycleItemDc
                        //{
                        //    Id = x.Id,
                        //    ABCClassification = x.ABCClassification,
                        //    Barcode = x.Barcode,
                        //    ItemMultiMRPId = x.ItemMultiMRPId,
                        //    ItemName = x.ItemName,
                        //    ItemNumber = x.ItemNumber,
                        //    MRP = x.MRP,
                        //    status = x.ItemBatch != null && x.ItemBatch.Any() ? 1 : 0,
                        //    ItemBatch = x.ItemBatch
                        //}).ToList();

                    }
                }

            }
            return SupervisorInventoryCycleItems;
        }

        #endregion

        [Route("DeleteInventoryCycleById")]
        [HttpGet]
        public async Task<response> DeleteInventoryCycleById(int WarehouseId, int PeopleId, int Id)
        {
            bool IsPV = false;
            using (AuthContext context = new AuthContext())
            {
                IsPV = context.Warehouses.Any(x => x.WarehouseId == WarehouseId && x.IsPV);
            }
            DateTime AssignDate = DateTime.Now.Date;
            response result = new response { Status = false, msg = "Inventory cycle batch code data not deleted." };
            MongoDbHelper<SupervisorInventoryCycleDay> mongoDbHelper = new MongoDbHelper<SupervisorInventoryCycleDay>();
            var inventoryPredicate = PredicateBuilder.New<SupervisorInventoryCycleDay>(x => x.WarehouseId == WarehouseId && x.AssignDate == AssignDate && x.PeopleId == PeopleId);
            if (IsPV)
                inventoryPredicate = inventoryPredicate.And(x => x.IsPV.HasValue && x.IsPV.Value == IsPV);
            else
                inventoryPredicate = inventoryPredicate.And(x => (!x.IsPV.HasValue || x.IsPV.Value == IsPV));

            var mongosupervisor = (await mongoDbHelper.SelectAsync(inventoryPredicate)).FirstOrDefault();
            if (mongosupervisor != null && mongosupervisor.InventoryCycleItems != null && mongosupervisor.InventoryCycleItems.Any(x => x.Id == Id))
            {
                var inventoryCycleItems = mongosupervisor.InventoryCycleItems.FirstOrDefault(x => x.Id == Id);
                if (inventoryCycleItems != null)
                {
                    //if (inventoryCycleItems.ItemBatch.Any(x => x.StockBatchId == StockBatchId))
                    //{
                    inventoryCycleItems.ItemBatch = new List<InventoryCycleItemBatch>();
                    result.Status = await mongoDbHelper.ReplaceWithoutFindAsync(mongosupervisor.Id, mongosupervisor);
                    if (result.Status)
                        result.msg = "Inventory cycle batch code data deleted successfully.";
                    //}
                }
            }
            return result;
        }

        [Route("DeleteInventoryBatchData")]
        [HttpGet]
        public async Task<response> DeleteInventoryCycle(int WarehouseId, int PeopleId, int Id, int StockBatchId)
        {
            DateTime AssignDate = DateTime.Now.Date;
            response result = new response { Status = false, msg = "Inventory cycle batch code data not deleted." };
            MongoDbHelper<SupervisorInventoryCycleDay> mongoDbHelper = new MongoDbHelper<SupervisorInventoryCycleDay>();
            var inventoryPredicate = PredicateBuilder.New<SupervisorInventoryCycleDay>(x => x.WarehouseId == WarehouseId && x.AssignDate == AssignDate && x.PeopleId == PeopleId);
            var mongosupervisor = (await mongoDbHelper.SelectAsync(inventoryPredicate)).FirstOrDefault();
            if (mongosupervisor != null && mongosupervisor.InventoryCycleItems != null && mongosupervisor.InventoryCycleItems.Any(x => x.Id == Id))
            {
                var inventoryCycleItems = mongosupervisor.InventoryCycleItems.FirstOrDefault(x => x.Id == Id);
                if (inventoryCycleItems != null)
                {
                    if (inventoryCycleItems.ItemBatch.Any(x => x.StockBatchId == StockBatchId))
                    {
                        inventoryCycleItems.ItemBatch = inventoryCycleItems.ItemBatch.Where(x => x.StockBatchId != StockBatchId).ToList();
                        result.Status = await mongoDbHelper.ReplaceAsync(mongosupervisor.Id, mongosupervisor);
                        if (result.Status)
                            result.msg = "Inventory cycle batch code data deleted successfully.";
                    }
                }
            }
            return result;
        }

        [Route("GetSupervisorSubmittedItem")]
        [HttpGet]
        public async Task<List<SupervisorInventoryCycleItemDc>> GetSupervisorSubmittedItem(int WarehouseId, int PeopleId)
        {
            bool IsPV = false;
            using (AuthContext context = new AuthContext())
            {
                IsPV = context.Warehouses.Any(x => x.WarehouseId == WarehouseId && x.IsPV);
            }
            DateTime AssignDate = DateTime.Now.Date;
            List<SupervisorInventoryCycleItemDc> SupervisorInventoryCycleItems = new List<SupervisorInventoryCycleItemDc>();
            MongoDbHelper<SupervisorInventoryCycleDay> mongoDbHelper = new MongoDbHelper<SupervisorInventoryCycleDay>();
            var inventoryPredicate = PredicateBuilder.New<SupervisorInventoryCycleDay>(x => x.WarehouseId == WarehouseId && x.AssignDate == AssignDate && x.PeopleId == PeopleId);
            if (IsPV)
                inventoryPredicate = inventoryPredicate.And(x => x.IsPV.HasValue && x.IsPV.Value == IsPV);
            else
                inventoryPredicate = inventoryPredicate.And(x => (!x.IsPV.HasValue || x.IsPV.Value == IsPV));
            var mongosupervisor = (await mongoDbHelper.SelectAsync(inventoryPredicate)).FirstOrDefault();
            using (AuthContext context = new AuthContext())
            {
                if (mongosupervisor != null && mongosupervisor.InventoryCycleItems != null && mongosupervisor.InventoryCycleItems.Any())
                {
                    var itemMultiMrpIds = mongosupervisor.InventoryCycleItems.Select(x => x.ItemMultiMRPId).Distinct().ToList();
                    if (IsPV)
                    {
                        List<int> rejectMrpIds = null;
                        if (!mongosupervisor.IsSubmitted)
                        {
                            rejectMrpIds = context.InventCycleDb.Where(x => EntityFunctions.TruncateTime(x.CreatedDate.Value) == EntityFunctions.TruncateTime(AssignDate) && itemMultiMrpIds.Contains(x.ItemMultiMrpId) && x.IsPV == IsPV).Select(x => x.ItemMultiMrpId).ToList();
                        }
                        else
                        {
                            rejectMrpIds = context.InventCycleDb.Where(x => x.IsWarehouseApproved == 2 && EntityFunctions.TruncateTime(x.CreatedDate.Value) == EntityFunctions.TruncateTime(AssignDate) && itemMultiMrpIds.Contains(x.ItemMultiMrpId) && x.IsPV == IsPV).Select(x => x.ItemMultiMrpId).ToList();
                        }

                        SupervisorInventoryCycleItems.AddRange(mongosupervisor.InventoryCycleItems.Where(x => rejectMrpIds.Contains(x.ItemMultiMRPId)).Select(ic => new SupervisorInventoryCycleItemDc
                        {
                            ABCClassification = ic.ABCClassification,
                            Barcode = ic.Barcode,
                            ItemMultiMRPId = ic.ItemMultiMRPId,
                            ItemName = ic.ItemName,
                            ItemNumber = ic.ItemNumber,
                            MRP = ic.MRP,
                            status = ic.ItemBatch != null && ic.ItemBatch.Any() ? 1 : 0,
                            ItemBatch = ic.ItemBatch,
                            Id = ic.Id
                        }).ToList());
                    }
                    else
                    {
                        if (!mongosupervisor.IsSubmitted)
                        {
                            SupervisorInventoryCycleItems.AddRange(mongosupervisor.InventoryCycleItems.Where(ic => ic.ItemBatch != null && ic.ItemBatch.Any()).Select(ic => new SupervisorInventoryCycleItemDc
                            {
                                ABCClassification = ic.ABCClassification,
                                Barcode = ic.Barcode,
                                ItemMultiMRPId = ic.ItemMultiMRPId,
                                ItemName = ic.ItemName,
                                ItemNumber = ic.ItemNumber,
                                MRP = ic.MRP,
                                status = ic.ItemBatch != null && ic.ItemBatch.Any() ? 1 : 0,
                                ItemBatch = ic.ItemBatch,
                                Id = ic.Id
                            }).ToList());
                        }
                    }

                }
                return SupervisorInventoryCycleItems;
            }
        }

        [Route("InventoryCycleImageUpload")]
        [HttpPost]
        [AllowAnonymous]
        public string InventoryCycleImageUpload()
        {
            string LogoUrl = "";
            try
            {
                var identity = User.Identity as ClaimsIdentity;
                int userid = 0;

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                    userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

                if (HttpContext.Current.Request.Files.AllKeys.Any())
                {
                    var httpPostedFile = HttpContext.Current.Request.Files["file"];

                    if (httpPostedFile != null)
                    {
                        if (!Directory.Exists(HttpContext.Current.Server.MapPath("~/InventoryCycle")))
                            Directory.CreateDirectory(HttpContext.Current.Server.MapPath("~/InventoryCycle"));

                        string extension = Path.GetExtension(httpPostedFile.FileName);

                        string fileName = httpPostedFile.FileName.Substring(0, httpPostedFile.FileName.LastIndexOf('.')) + DateTime.Now.ToString("ddMMyyyyHHmmss") + extension;

                        LogoUrl = Path.Combine(HttpContext.Current.Server.MapPath("~/InventoryCycle"), fileName);

                        httpPostedFile.SaveAs(LogoUrl);

                        AngularJSAuthentication.Common.Helpers.FileUploadHelper.Upload(httpPostedFile.FileName, "~/InventoryCycle", LogoUrl);

                        LogoUrl = "/InventoryCycle/" + fileName;
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error("Error in GetExchangeHubCashCollection Method: " + ex.Message);
            }
            return LogoUrl;
        }

        [Route("FinalSubmitSupervisorItem")]
        [HttpGet]
        public async Task<response> FinalSubmitSupervisorItem(int WarehouseId, int PeopleId)
        {
            bool IsPV = false;
            using (AuthContext context = new AuthContext())
            {
                IsPV = context.Warehouses.Any(x => x.WarehouseId == WarehouseId && x.IsPV);
            }
            response result = new response();
            DateTime AssignDate = DateTime.Now.Date;
            List<SupervisorInventoryCycleItemDc> SupervisorInventoryCycleItems = new List<SupervisorInventoryCycleItemDc>();
            MongoDbHelper<SupervisorInventoryCycleDay> mongoDbHelper = new MongoDbHelper<SupervisorInventoryCycleDay>();
            var inventoryPredicate = PredicateBuilder.New<SupervisorInventoryCycleDay>(x => x.WarehouseId == WarehouseId && x.AssignDate == AssignDate && x.PeopleId == PeopleId);
            if (IsPV)
                inventoryPredicate = inventoryPredicate.And(x => x.IsPV.HasValue && x.IsPV.Value == IsPV);
            else
                inventoryPredicate = inventoryPredicate.And(x => (!x.IsPV.HasValue || x.IsPV.Value == IsPV));

            var mongosupervisor = (await mongoDbHelper.SelectAsync(inventoryPredicate)).FirstOrDefault();

            if ((mongosupervisor != null && !mongosupervisor.IsSubmitted && mongosupervisor.InventoryCycleItems != null && mongosupervisor.InventoryCycleItems.Any(x => x.ItemBatch != null && x.ItemBatch.Any())) || (mongosupervisor != null && mongosupervisor.IsSubmitted && mongosupervisor.InventoryCycleItems != null && mongosupervisor.InventoryCycleItems.Any(x => x.ItemBatch != null && x.ItemBatch.Any() && x.IsRejected)))
            {
                List<SupervisorInventoryCycleItem> InventoryCycleItems = new List<SupervisorInventoryCycleItem>();
                if (!mongosupervisor.IsSubmitted)
                {
                    InventoryCycleItems = mongosupervisor.InventoryCycleItems.Where(x => x.ItemBatch != null && x.ItemBatch.Any()).ToList();
                }
                else
                {
                    InventoryCycleItems = mongosupervisor.InventoryCycleItems.Where(x => x.ItemBatch != null && x.ItemBatch.Any() && x.IsRejected).ToList();
                }


                var Ids = InventoryCycleItems.Select(x => x.Id);

                var orderIdDt = new DataTable();
                orderIdDt.Columns.Add("IntValue");
                foreach (var item in Ids)
                {
                    var dr = orderIdDt.NewRow();
                    dr["IntValue"] = item;
                    orderIdDt.Rows.Add(dr);
                }
                List<InventoryCycleEditDc> InventoryCycleEditDcs = new List<InventoryCycleEditDc>();
                using (AuthContext context = new AuthContext())
                {

                    var warehouse = context.Warehouses.FirstOrDefault(x => x.WarehouseId == WarehouseId);
                    if (!warehouse.IsStopCurrentStockTrans)
                    {
                        result.msg = "Please stop transactions of this warehouse before inventory count";
                        return result;
                    }

                    if (context.Database.Connection.State != ConnectionState.Open)
                        context.Database.Connection.Open();

                    var param = new SqlParameter("Ids", orderIdDt);
                    param.SqlDbType = SqlDbType.Structured;
                    param.TypeName = "dbo.IntValues";
                    var cmd = context.Database.Connection.CreateCommand();
                    cmd.CommandTimeout = 900;
                    cmd.CommandText = "[dbo].[GetInventoryCyclesForEdit]";
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@warehouseid", WarehouseId));
                    cmd.Parameters.Add(param);

                    // Run the sproc
                    var reader1 = cmd.ExecuteReader();
                    InventoryCycleEditDcs = ((IObjectContextAdapter)context)
                    .ObjectContext
                    .Translate<InventoryCycleEditDc>(reader1).ToList();
                }
                using (AuthContext context = new AuthContext())
                {

                    if (InventoryCycleEditDcs != null && InventoryCycleEditDcs.Any())
                    {
                        //var Ids = InventoryCycleEditDcs.Select(x => x.Id).ToList();
                        var InventCycles = context.InventCycleDb.Where(x => Ids.Contains(x.Id)).Include(x => x.InventCycleBatchs).ToList();
                        //var StockBatchIds = InventCycles.SelectMany(x => x.InventCycleBatchs).Where(x => x.StockBatchId > 0).Select(x => x.StockBatchId).ToList();
                        //var StockBatchs = context.StockBatchMasters.Where(x => StockBatchIds.Contains(x.Id)).ToList();
                        var StockBatchIds = InventoryCycleItems.Where(x => x.ItemBatch != null && x.ItemBatch.Any()).SelectMany(x => x.ItemBatch.Select(y => y.StockBatchId)).ToList();
                        var itemmultimrpids = InventoryCycleItems.Select(x => x.ItemMultiMRPId).Distinct().ToList();
                        var currentstocks = context.DbCurrentStock.Where(x => itemmultimrpids.Contains(x.ItemMultiMRPId) && x.WarehouseId == WarehouseId).ToList();
                        var currentStocksIds = currentstocks.Select(x => Convert.ToInt64(x.StockId)).ToList();
                        var StockBatchs = context.StockBatchMasters.Where(x => currentStocksIds.Contains(x.StockId) && x.StockType == "C").ToList();

                        var ItemWithBatchIds = (from s in StockBatchs
                                                join r in currentstocks on s.StockId equals r.StockId
                                                select new { s.BatchMasterId, s.Id, s.StockId, r.ItemMultiMRPId, r.WarehouseId, r.itemname, s.Qty }).ToList();

                        var requiredItemWithBatchIds = ItemWithBatchIds.Where(x => x.Qty > 0).ToList();
                        var fillQtyIds = InventoryCycleItems.SelectMany(d => d.ItemBatch.Select(s => new { s.BatchMasterId, d.ItemMultiMRPId, WarehouseId })).ToList();

                        if (requiredItemWithBatchIds.Any(x => !fillQtyIds.Any(y => y.ItemMultiMRPId == x.ItemMultiMRPId && y.WarehouseId == x.WarehouseId && x.BatchMasterId == y.BatchMasterId)))
                        {
                            var stoctItemName = requiredItemWithBatchIds.Where(x => !fillQtyIds.Any(y => y.ItemMultiMRPId == x.ItemMultiMRPId && y.WarehouseId == x.WarehouseId
                            && x.BatchMasterId == y.BatchMasterId)).Select(x => x.itemname).ToList();
                            var Items = string.Join(",", stoctItemName);
                            result.msg = "Please count all the batches for " + Items;
                            return result;
                        }


                        var rptorderids = context.DbOrderMaster.Where(x => x.WarehouseId == WarehouseId && x.Status == "ReadyToPick").Select(x => x.OrderId).ToList();
                        var orderids = rptorderids.Any() ? rptorderids.Select(x => Convert.ToInt64(x)) : new List<long>();
                        var OrderdetailIds = context.DbOrderDetails.Where(x => orderids.Contains(x.OrderId)).Select(x => x.OrderDetailsId).ToList().Select(x => Convert.ToInt64(x)).ToList();

                        var ZeroOrderdetailIds = context.OrderPickerDetailsDb.Any(x => OrderdetailIds.Contains(x.OrderDetailsId) && itemmultimrpids.Contains(x.ItemMultiMrpId) && x.Qty == 0) == true ? context.OrderPickerDetailsDb.Where(x => OrderdetailIds.Contains(x.OrderDetailsId) && itemmultimrpids.Contains(x.ItemMultiMrpId) && x.Qty == 0).Select(x => x.OrderDetailsId).ToList().Select(x => Convert.ToInt64(x)).ToList() : null;

                        var transId = context.StockTxnTypeMasters.FirstOrDefault(x => x.StockTxnType == "OrderPlannedOutCurrent").Id;

                        var StocksRtp = new List<RTPStocksDcBatch>();
                        if (ZeroOrderdetailIds != null && ZeroOrderdetailIds.Any())
                        {
                            StocksRtp = context.StockBatchTransactions.Where(x => orderids.Contains(x.ObjectId) && !ZeroOrderdetailIds.Contains(x.ObjectDetailId) && OrderdetailIds.Contains(x.ObjectDetailId) && StockBatchIds.Contains(x.StockBatchMasterId) && (x.StockTxnTypeId == transId)).ToList().GroupBy(x => x.StockBatchMasterId)
                               .Select(x => new RTPStocksDcBatch { StockBatchMasterId = x.Key, Qty = x.GroupBy(z => z.ObjectDetailId).Select(y => y.OrderByDescending(z => z.Id).FirstOrDefault().Qty).Sum(y => y) }).ToList();
                        }
                        else
                        {
                            StocksRtp = context.StockBatchTransactions.Where(x => orderids.Contains(x.ObjectId) && OrderdetailIds.Contains(x.ObjectDetailId) && StockBatchIds.Contains(x.StockBatchMasterId) && (x.StockTxnTypeId == transId)).ToList().GroupBy(x => x.StockBatchMasterId)
                         .Select(x => new RTPStocksDcBatch { StockBatchMasterId = x.Key, Qty = x.GroupBy(z => z.ObjectDetailId).Select(y => y.OrderByDescending(z => z.Id).FirstOrDefault().Qty).Sum(y => y) }).ToList();
                        }


                        //string rtpquery = "exec GetRTPStocks " + WarehouseId;
                        //var rptstocks = context.Database.SqlQuery<RTPStocksDc>(rtpquery).ToList();



                        bool AllDataAdd = true;
                        foreach (var item in InventoryCycleItems)
                        {
                            var InventCycle = InventCycles.FirstOrDefault(x => x.Id == item.Id);
                            var InventoryCycleEditDc = InventoryCycleEditDcs.FirstOrDefault(x => x.Id == item.Id);
                            if (InventCycle != null)
                            {
                                var totalRtp = 0;
                                foreach (var bt in item.ItemBatch)
                                {
                                    if (ItemWithBatchIds.Any(x => x.WarehouseId == WarehouseId && x.ItemMultiMRPId == item.ItemMultiMRPId && x.BatchMasterId == bt.BatchMasterId))
                                        bt.StockBatchId = ItemWithBatchIds.FirstOrDefault(x => x.WarehouseId == WarehouseId && x.ItemMultiMRPId == item.ItemMultiMRPId && x.BatchMasterId == bt.BatchMasterId).Id;

                                    var currentqty = StockBatchs.FirstOrDefault(x => x.Id == bt.StockBatchId)?.Qty;
                                    var Rtp = StocksRtp.FirstOrDefault(x => x.StockBatchMasterId == bt.StockBatchId)?.Qty;
                                    totalRtp += Rtp.HasValue ? Rtp.Value : 0;
                                    if (InventCycle.InventCycleBatchs != null && InventCycle.InventCycleBatchs.Any(x => x.BatchMasterId == bt.BatchMasterId))
                                    {
                                        var updatebatchdetail = InventCycle.InventCycleBatchs.FirstOrDefault(x => x.BatchMasterId == bt.BatchMasterId);
                                        updatebatchdetail.StockBatchId = bt.StockBatchId;
                                        updatebatchdetail.DamagedImageUrl = bt.DamagedImageUrl;
                                        updatebatchdetail.DamagedQty = bt.DamagedQty;
                                        updatebatchdetail.PastInventory = currentqty > 0 ? currentqty : 0;
                                        if (item.IsRejected)
                                        {
                                            updatebatchdetail.InventoryCount = bt.InventoryCount;
                                        }
                                        else
                                        {
                                            updatebatchdetail.InventoryCount += bt.InventoryCount;
                                        }

                                        updatebatchdetail.NonSellableImageUrl = bt.NonSellableImageUrl;
                                        updatebatchdetail.NonSellableQty = bt.NonSellableQty;
                                        updatebatchdetail.Rtp = Rtp;
                                    }
                                    else
                                    {
                                        if (InventCycle.InventCycleBatchs == null)
                                            InventCycle.InventCycleBatchs = new List<InventCycleBatch>();

                                        InventCycle.InventCycleBatchs.Add(new InventCycleBatch
                                        {
                                            DamagedImageUrl = bt.DamagedImageUrl,
                                            DamagedQty = bt.DamagedQty,
                                            InventoryCount = bt.InventoryCount,
                                            NonSellableImageUrl = bt.NonSellableImageUrl,
                                            NonSellableQty = bt.NonSellableQty,
                                            BatchMasterId = bt.BatchMasterId,
                                            StockBatchId = bt.StockBatchId,
                                            PastInventory = currentqty > 0 ? currentqty : 0,
                                            Rtp = Rtp
                                        });
                                    }

                                }


                                InventCycle.UpdatedDate = indianTime;
                                InventCycle.UpadtedBy = mongosupervisor.DisplayName;
                                InventCycle.InventoryCount = InventCycle.InventCycleBatchs.Sum(x => x.InventoryCount);
                                InventCycle.NonSellableQty = InventCycle.InventCycleBatchs.Sum(x => x.NonSellableQty);
                                InventCycle.DamagedQty = InventCycle.InventCycleBatchs.Sum(x => x.DamagedQty);
                                InventCycle.Comment = InventCycle.Comment;
                                InventCycle.RtdCount = InventoryCycleEditDc != null ? InventoryCycleEditDc.rtdQty : 0;
                                InventCycle.RtpCount = totalRtp > 0 ? InventoryCycleEditDc.rtpQty : 0;
                                InventCycle.IsWarehouseApproved = 0;
                                InventCycle.CurrentInventory = InventoryCycleEditDc != null ? InventoryCycleEditDc.CurrentInventory : 0;
                                InventCycle.IsApproved = false;
                                InventCycle.PastInventory = InventoryCycleEditDc != null ? InventoryCycleEditDc.CurrentInventory : 0;
                                InventCycle.CreatedBy = mongosupervisor.DisplayName;
                                if (InventCycle.InventoryCount == InventCycle.CurrentInventory)
                                {

                                    if (InventCycle.InventCycleBatchs.All(x => ((x.PastInventory ?? 0)) == x.InventoryCount))
                                    {
                                        InventCycle.IsApproved = true;
                                        InventCycle.IsWarehouseApproved = 1;
                                        InventCycle.Comment = "Auto Approved By System";
                                    }

                                    //foreach (var batchData in InventCycle.InventCycleBatchs.Where(x => ((x.PastInventory ?? 0)) == x.InventoryCount).ToList())
                                    //{
                                    //    bool isZerodiff = false;
                                    //    List<InventCycleBatch> inventCycleBatch = context.InventCycleBatchDb.Where(x => x.InventCycle_Id == InventCycle.Id).ToList();
                                    //    foreach (var batch in inventCycleBatch)
                                    //    {
                                    //        if (batch.InventoryCount == batchData.InventoryCount && batch.PastInventory == batchData.PastInventory)
                                    //        {

                                    //        }
                                    //        else
                                    //        {
                                    //            isZerodiff = true;
                                    //        }
                                    //    }
                                    //    if (isZerodiff == false)
                                    //    {
                                    //        InventCycle.IsApproved = true;
                                    //        InventCycle.IsWarehouseApproved = 1;
                                    //        InventCycle.Comment = "Auto Approved By System";
                                    //    }

                                    //}
                                }
                                //
                                var identity = User.Identity as ClaimsIdentity;
                                int userid = 0;
                                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                                    userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
                                var inventCycleDataHistoryData = context.InventCycleDataHistoryDB.Where(x => x.InventoryCycleId == InventCycle.Id && x.IsDeleted == false && x.IsActive).FirstOrDefault();
                                if (inventCycleDataHistoryData != null)
                                {
                                    inventCycleDataHistoryData.IsActive = true;
                                    inventCycleDataHistoryData.ModifiedBy = userid;
                                    inventCycleDataHistoryData.ModifiedDate = DateTime.Now;
                                    inventCycleDataHistoryData.Status = InventCycle.IsWarehouseApproved == 0 ? "Warehouse Pending" : InventCycle.Comment;
                                    inventCycleDataHistoryData.InventoryCount = InventCycle.InventCycleBatchs.Sum(x => x.InventoryCount);
                                    context.Entry(inventCycleDataHistoryData).State = EntityState.Modified;
                                }
                                else
                                {
                                    InventCycleDataHistory list = new InventCycleDataHistory();
                                    list.InventoryCycleId = InventCycle.Id;
                                    list.InventoryCount = InventCycle.InventCycleBatchs.Sum(x => x.InventoryCount);
                                    list.Status = InventCycle.IsWarehouseApproved == 0 ? "Warehouse Pending" : InventCycle.Comment;
                                    list.CreatedDate = DateTime.Now;
                                    list.CreatedBy = mongosupervisor.DisplayName;
                                    list.IsActive = true;

                                    context.Entry(list).State = EntityState.Added;
                                }
                                //

                                context.Entry(InventCycle).State = EntityState.Modified;
                                if (item.IsRejected)
                                {
                                    item.IsRejected = false;
                                }

                            }
                            else
                            {
                                AllDataAdd = false;
                            }

                        }
                        if (AllDataAdd)
                            result.Status = context.Commit() > 0;

                        if (result.Status)
                        {
                            mongosupervisor.IsPV = IsPV;
                            mongosupervisor.IsSubmitted = true;
                            mongoDbHelper.Replace(mongosupervisor.Id, mongosupervisor);
                            result.msg = "Inventory cycle submitted successfully.";
                        }
                        else
                        {
                            result.msg = "Inventory cycle not submitted. Please try after some time";
                        }
                    }
                    else
                    {
                        result.Status = false;
                        result.msg = "Inventory cycle already submitted";
                    }
                }
            }
            else
            {
                result.Status = false;
                result.msg = "Inventory cycle already submitted";
            }
            return result;
        }


        [HttpGet]
        [Route("SearchitemForInventoryCycle")]
        public async Task<List<ControllerV7.Stocks.MRPItemStockDTO>> SearchitemForInventoryCycle(string key, int warehouseid)
        {
            List<ControllerV7.Stocks.MRPItemStockDTO> _result = new List<ControllerV7.Stocks.MRPItemStockDTO>();

            using (var db = new AuthContext())
            {
                _result = await db.Database.SqlQuery<ControllerV7.Stocks.MRPItemStockDTO>("exec getItemForInventoryCycle '" + key + "'," + warehouseid).ToListAsync();
            }
            return _result;
        }

        [HttpGet]
        [Route("AddItemInInventoryCycle")]
        public async Task<response> AddItemInInventoryCycle(int itemMultiMrpId, int warehouseid)
        {
            var result = false;
            using (var context = new AuthContext())
            {
                bool IsPV = context.Warehouses.Any(x => x.WarehouseId == warehouseid && x.IsPV);
                var orderIdDt = new DataTable();
                orderIdDt.Columns.Add("ItemMultiMRPId");
                orderIdDt.Columns.Add("WarehouseId");
                orderIdDt.Columns.Add("IsFreeItem");

                var dr = orderIdDt.NewRow();
                dr["ItemMultiMRPId"] = itemMultiMrpId;
                dr["WarehouseId"] = warehouseid;
                dr["IsFreeItem"] = false;
                orderIdDt.Rows.Add(dr);

                if (context.Database.Connection.State != ConnectionState.Open)
                    context.Database.Connection.Open();

                var param = new SqlParameter("item", orderIdDt);
                param.SqlDbType = SqlDbType.Structured;
                param.TypeName = "dbo.itemtype";
                var cmd = context.Database.Connection.CreateCommand();
                cmd.CommandText = "[dbo].[AddItemInInventoryCycle]";
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.Parameters.Add(param);


                result = cmd.ExecuteNonQuery() > 0;

                if (result)
                {
                    DateTime AssignDate = DateTime.Now.Date;
                    var InventoryCycle = context.InventCycleDb.FirstOrDefault(x => EntityFunctions.TruncateTime(x.CreatedDate) == AssignDate && x.WarehouseId == warehouseid && x.ItemMultiMrpId == itemMultiMrpId && (!x.InventoryCount.HasValue || x.InventoryCount.Value == 0));
                    if (InventoryCycle != null)
                    {
                        var currentstock = context.DbCurrentStock.FirstOrDefault(x => x.WarehouseId == warehouseid && x.ItemMultiMRPId == itemMultiMrpId);

                        MongoDbHelper<SupervisorInventoryCycleDay> mongoDbHelper = new MongoDbHelper<SupervisorInventoryCycleDay>();
                        var inventoryPredicate = PredicateBuilder.New<SupervisorInventoryCycleDay>(x => x.WarehouseId == warehouseid && x.AssignDate == AssignDate);
                        if (IsPV)
                            inventoryPredicate = inventoryPredicate.And(x => x.IsPV.HasValue && x.IsPV.Value == IsPV);
                        else
                            inventoryPredicate = inventoryPredicate.And(x => (!x.IsPV.HasValue || x.IsPV.Value == IsPV));
                        var mongosupervisors = (mongoDbHelper.Select(inventoryPredicate)).ToList();
                        if (mongosupervisors != null && mongosupervisors.Any())
                        {
                            var super = mongosupervisors.FirstOrDefault();
                            super.InventoryCycleItems.Add(new SupervisorInventoryCycleItem
                            {
                                Id = InventoryCycle.Id,
                                NonSellableQty = InventoryCycle.NonSellableQty,
                                ABCClassification = "D",
                                Barcode = new List<string>(),
                                DamagedQty = 0,
                                InventoryCount = 0,
                                ItemBatch = new List<InventoryCycleItemBatch>(),
                                ItemMultiMRPId = InventoryCycle.ItemMultiMrpId,
                                ItemName = currentstock.itemname,
                                ItemNumber = currentstock.ItemNumber,
                                MRP = currentstock.MRP
                            });

                            mongoDbHelper.Replace(super.Id, super);
                        }
                    }
                }
            }
            return new response { msg = (result ? "Item uploaded successfully." : "Item not added."), Status = result };

        }

        [HttpPost]
        [Route("UploadItemInInventoryCycle")]
        public async Task<response> UploadItemInInventoryCycle(List<UploadInventoryCycleItem> uploadInventoryCycleItems)
        {
            var result = false;
            using (var context = new AuthContext())
            {
                var orderIdDt = new DataTable();
                orderIdDt.Columns.Add("ItemMultiMRPId");
                orderIdDt.Columns.Add("WarehouseId");
                orderIdDt.Columns.Add("IsFreeItem");
                foreach (var item in uploadInventoryCycleItems)
                {
                    var dr = orderIdDt.NewRow();
                    dr["ItemMultiMRPId"] = item.ItemMultiMrpId;
                    dr["WarehouseId"] = item.WarehouseId;
                    dr["IsFreeItem"] = false;
                    orderIdDt.Rows.Add(dr);
                }

                if (context.Database.Connection.State != ConnectionState.Open)
                    context.Database.Connection.Open();

                var param = new SqlParameter("item", orderIdDt);
                param.SqlDbType = SqlDbType.Structured;
                param.TypeName = "dbo.itemtype";
                var cmd = context.Database.Connection.CreateCommand();
                cmd.CommandTimeout = 900;
                cmd.CommandText = "[dbo].[AddItemInInventoryCycle]";
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.Parameters.Add(param);

                result = cmd.ExecuteNonQuery() > 0;

                if (result)
                {
                    DateTime AssignDate = DateTime.Now.Date;
                    var warehouseIds = uploadInventoryCycleItems.Select(x => x.WarehouseId).Distinct().ToList();
                    var itemMultiMrpIds = uploadInventoryCycleItems.Select(x => x.ItemMultiMrpId).Distinct().ToList();
                    var InventoryCycles = context.InventCycleDb.Where(x => EntityFunctions.TruncateTime(x.CreatedDate) == AssignDate && warehouseIds.Contains(x.WarehouseId) && itemMultiMrpIds.Contains(x.ItemMultiMrpId) && (!x.InventoryCount.HasValue || x.InventoryCount.Value == 0));
                    if (InventoryCycles != null && InventoryCycles.Any())
                    {
                        var currentstocks = context.DbCurrentStock.Where(x => warehouseIds.Contains(x.WarehouseId) && itemMultiMrpIds.Contains(x.ItemMultiMRPId));

                        MongoDbHelper<SupervisorInventoryCycleDay> mongoDbHelper = new MongoDbHelper<SupervisorInventoryCycleDay>();
                        var inventoryPredicate = PredicateBuilder.New<SupervisorInventoryCycleDay>(x => warehouseIds.Contains(x.WarehouseId) && x.AssignDate == AssignDate);
                        var mongosupervisors = (mongoDbHelper.Select(inventoryPredicate)).ToList();

                        if (mongosupervisors != null && mongosupervisors.Any())
                        {
                            foreach (var supers in mongosupervisors.GroupBy(x => x.WarehouseId))
                            {
                                var items = InventoryCycles.Where(x => x.WarehouseId == supers.Key).ToList();
                                var total = Convert.ToInt32(Math.Ceiling(items.Count() / Convert.ToDouble(supers.Count())));
                                int a = 0, j = 0;
                                foreach (var super in supers)
                                {
                                    var inventorycycleitems = items.Skip(a).Take(total);
                                    super.InventoryCycleItems.AddRange(inventorycycleitems.Select(x => new SupervisorInventoryCycleItem
                                    {
                                        Id = x.Id,
                                        NonSellableQty = x.NonSellableQty,
                                        ABCClassification = "D",
                                        Barcode = new List<string>(),
                                        DamagedQty = 0,
                                        InventoryCount = 0,
                                        ItemBatch = new List<InventoryCycleItemBatch>(),
                                        ItemMultiMRPId = x.ItemMultiMrpId,
                                        ItemName = currentstocks.Any(y => y.ItemMultiMRPId == x.ItemMultiMrpId && y.WarehouseId == x.WarehouseId) ? currentstocks.FirstOrDefault(y => y.ItemMultiMRPId == x.ItemMultiMrpId && y.WarehouseId == x.WarehouseId).itemname : "",
                                        ItemNumber = currentstocks.Any(y => y.ItemMultiMRPId == x.ItemMultiMrpId && y.WarehouseId == x.WarehouseId) ? currentstocks.FirstOrDefault(y => y.ItemMultiMRPId == x.ItemMultiMrpId && y.WarehouseId == x.WarehouseId).ItemNumber : "",
                                        MRP = currentstocks.Any(y => y.ItemMultiMRPId == x.ItemMultiMrpId && y.WarehouseId == x.WarehouseId) ? currentstocks.FirstOrDefault(y => y.ItemMultiMRPId == x.ItemMultiMrpId && y.WarehouseId == x.WarehouseId).MRP : 0
                                    }).ToList());
                                    mongoDbHelper.Replace(super.Id, super);
                                    j++;
                                    a = j * total;
                                }
                            }
                        }
                    }
                }
            }
            return new response { msg = (result ? "Item uploaded successfully." : "Some issue occurred. Please try after some time."), Status = result };
        }

        [Route("UpdateWarehoueAction")]
        [HttpGet]
        public bool UpdateWarehoueActions(int WarehouseId, int Type, bool Flag)
        {
            bool status = false;
            var identity = User.Identity as ClaimsIdentity;
            int userid = 0;
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
            using (var db = new AuthContext())
            {
                var list = db.Warehouses.Where(x => x.WarehouseId == WarehouseId).FirstOrDefault();
                if (list != null)
                {
                    if (Type == 0)
                    {
                        list.IsDeliveryOptimizationEnabled = Flag;
                    }
                    else
                    {
                        list.IsLocationEnabled = Flag;
                    }
                    list.UpdatedDate = DateTime.Now;
                    list.UpdateBy = userid.ToString();
                    db.Entry(list).State = EntityState.Modified;
                    db.Commit();
                    status = true;
                }
            }
            return status;
        }

        #region "No Used Code"
        [HttpGet]
        [Route("InsertRemainingItem")]
        public bool InsertRemainingItem()
        {
            List<raminingInventory> raminingInventorys = new List<raminingInventory>();
            using (var context = new AuthContext())
            {
                if (context.Database.Connection.State != ConnectionState.Open)
                    context.Database.Connection.Open();


                var cmd = context.Database.Connection.CreateCommand();
                cmd.CommandText = "[dbo].[GetremainingInventoryCycleItem]";
                cmd.CommandType = System.Data.CommandType.StoredProcedure;


                var reader1 = cmd.ExecuteReader();
                raminingInventorys = ((IObjectContextAdapter)context)
                .ObjectContext
                .Translate<raminingInventory>(reader1).ToList();
            }

            DateTime AssignDate = DateTime.Now.Date;

            var warehouseIds = raminingInventorys.GroupBy(x => x.WarehouseId).Select(x => x.Key).ToList();
            MongoDbHelper<SupervisorInventoryCycleDay> mongoDbHelper = new MongoDbHelper<SupervisorInventoryCycleDay>();
            var inventoryPredicate = PredicateBuilder.New<SupervisorInventoryCycleDay>(x => warehouseIds.Contains(x.WarehouseId) && x.AssignDate == AssignDate);
            var mongosupervisors = (mongoDbHelper.Select(inventoryPredicate)).ToList();

            using (var context = new AuthContext())
            {
                if (mongosupervisors != null && mongosupervisors.Any())
                {
                    foreach (var supers in mongosupervisors.GroupBy(x => x.WarehouseId))
                    {
                        var items = raminingInventorys.Where(x => x.WarehouseId == supers.Key).ToList();
                        List<InventCycle> inventoryCycles = items.Select(x => new InventCycle
                        {
                            Comment = "",
                            CreatedDate = DateTime.Now,
                            CurrentInventory = 0,
                            DamagedQty = 0,
                            InventoryCount = 0,
                            InventoryCycleMasterId = x.InventoryCycleMasterId,
                            IsApproved = false,
                            IsDeleted = false,
                            ItemMultiMrpId = x.ItemMultiMRPId,
                            NonSellableQty = 0,
                            PastInventory = 0,
                            RtdCount = 0,
                            RtpCount = 0,
                            UpadtedBy = "",
                            WarehouseId = x.WarehouseId
                        }).ToList();
                        context.InventCycleDb.AddRange(inventoryCycles);
                        context.Commit();
                        var total = Convert.ToInt32(Math.Ceiling(items.Count() / Convert.ToDouble(supers.Count())));
                        int a = 0, j = 0;

                        foreach (var super in supers)
                        {
                            bool IsPV = context.Warehouses.Any(x => x.WarehouseId == super.WarehouseId && x.IsPV);
                            super.IsPV = IsPV;
                            var inventorycycleitems = inventoryCycles.Skip(a).Take(total);
                            super.InventoryCycleItems = (inventorycycleitems.Select(x => new SupervisorInventoryCycleItem
                            {
                                Id = x.Id,
                                NonSellableQty = x.NonSellableQty,
                                ABCClassification = items.Any(y => y.ItemMultiMRPId == x.ItemMultiMrpId && y.WarehouseId == x.WarehouseId) ? items.FirstOrDefault(y => y.ItemMultiMRPId == y.ItemMultiMRPId && y.WarehouseId == x.WarehouseId).classification : "",
                                Barcode = new List<string>(),
                                DamagedQty = 0,
                                InventoryCount = 0,
                                ItemBatch = new List<InventoryCycleItemBatch>(),
                                ItemMultiMRPId = x.ItemMultiMrpId,
                                ItemName = items.Any(y => y.ItemMultiMRPId == x.ItemMultiMrpId && y.WarehouseId == x.WarehouseId) ? items.FirstOrDefault(y => y.ItemMultiMRPId == x.ItemMultiMrpId && y.WarehouseId == x.WarehouseId).ItemName : "",
                                ItemNumber = items.Any(y => y.ItemMultiMRPId == x.ItemMultiMrpId && y.WarehouseId == x.WarehouseId) ? items.FirstOrDefault(y => y.ItemMultiMRPId == x.ItemMultiMrpId && y.WarehouseId == x.WarehouseId).ItemNumber : "",
                                MRP = items.Any(y => y.ItemMultiMRPId == x.ItemMultiMrpId && y.WarehouseId == x.WarehouseId) ? items.FirstOrDefault(y => y.ItemMultiMRPId == x.ItemMultiMrpId && y.WarehouseId == x.WarehouseId).MRP : 0
                            }).ToList());
                            mongoDbHelper.Replace(super.Id, super);
                            j++;
                            a = j * total;
                        }
                    }
                }
            }

            return true;
        }

        [HttpGet]
        [Route("UpdateitemName")]
        public bool UpdateitemName()
        {
            using (var context = new AuthContext())
            {
                string query = "select id,a.ItemMultiMrpId,c.ItemName,c.ItemNumber,c.MRP from InventCycles a with(nolock) Inner join CurrentStocks c with(nolock) on a.WarehouseId=c.WarehouseId and a.ItemMultiMrpId=c.ItemMultiMRPId and  cast(a.CreatedDate as date)=cast(getdate() as date) ";


                if (context.Database.Connection.State != ConnectionState.Open)
                    context.Database.Connection.Open();


                var cmd = context.Database.Connection.CreateCommand();
                cmd.CommandText = query;
                cmd.CommandType = System.Data.CommandType.Text;


                var reader1 = cmd.ExecuteReader();
                var raminingInventorys = ((IObjectContextAdapter)context)
                .ObjectContext
                .Translate<inventoryitem>(reader1).ToList();

                DateTime AssignDate = DateTime.Now.Date;

                MongoDbHelper<SupervisorInventoryCycleDay> mongoDbHelper = new MongoDbHelper<SupervisorInventoryCycleDay>();
                var inventoryPredicate = PredicateBuilder.New<SupervisorInventoryCycleDay>(x => x.AssignDate == AssignDate);
                var mongosupervisors = (mongoDbHelper.Select(inventoryPredicate)).ToList();

                foreach (var super in mongosupervisors)
                {
                    foreach (var item in super.InventoryCycleItems)
                    {
                        if (raminingInventorys.Any(x => x.id == item.Id))
                        {
                            var data = raminingInventorys.FirstOrDefault(x => x.id == item.Id);
                            item.ItemName = data.ItemName;
                            item.ItemNumber = data.ItemNumber;
                            item.MRP = data.MRP;
                        }
                    }

                    mongoDbHelper.Replace(super.Id, super);
                }

            }

            return true;

        }


        [HttpGet]
        [Route("GetBarcodeItem")]
        public async Task<List<Model.Item.PickItemForBarcode>> GetTodayScanItem(int warehouseId)
        {
            List<Model.Item.PickItemForBarcode> items = new List<Model.Item.PickItemForBarcode>();
            using (var context = new AuthContext())
            {
                items = (await context.PickItemForBarcodeDb.Where(x => x.WarehouseId == warehouseId && x.Status == 0).ToListAsync()).OrderBy(x => x.ItemName).ToList();
            }

            return items;
        }

        [Route("ItemScanImageUpload")]
        [HttpPost]
        [AllowAnonymous]
        public async Task<string> ItemScanImageUpload()
        {
            string LogoUrl = "";
            try
            {

                if (HttpContext.Current.Request.Files.AllKeys.Any())
                {
                    var httpPostedFile = HttpContext.Current.Request.Files["file"];

                    if (httpPostedFile != null)
                    {
                        if (!Directory.Exists(HttpContext.Current.Server.MapPath("~/ItemScan")))
                            Directory.CreateDirectory(HttpContext.Current.Server.MapPath("~/ItemScan"));

                        string extension = Path.GetExtension(httpPostedFile.FileName);

                        string fileName = httpPostedFile.FileName.Substring(0, httpPostedFile.FileName.LastIndexOf('.')) + DateTime.Now.ToString("ddMMyyyyHHmmss") + extension;

                        LogoUrl = Path.Combine(HttpContext.Current.Server.MapPath("~/ItemScan"), fileName);

                        httpPostedFile.SaveAs(LogoUrl);

                        AngularJSAuthentication.Common.Helpers.FileUploadHelper.Upload(fileName, "~/ItemScan", LogoUrl);

                        LogoUrl = "/ItemScan/" + fileName;
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error("Error in MovementStockImageUpload Method: " + ex.Message);
            }
            return LogoUrl;
        }

        [Route("UpdateScanBarCode")]
        [HttpPost]
        public async Task<bool> UpdateScanBarCode(scanItem scanItem)
        {
            bool result = false;
            using (var context = new AuthContext())
            {
                var item = (await context.PickItemForBarcodeDb.FirstOrDefaultAsync(x => x.Id == scanItem.Id && x.Status == 0));
                if (item != null)
                {
                    item.NewEAN = scanItem.barcode;
                    item.ImageUrl = scanItem.ImagePath;
                    item.ModifiedDate = DateTime.Now;
                    item.Status = 1;
                    context.Entry(item).State = EntityState.Modified;
                    result = (await context.CommitAsync()) > 0;
                }
            }
            return result;
        }

        [HttpGet]
        [Route("GetBarcodeItemforpage")]
        public async Task<List<Model.Item.PickItemForBarcode>> GetBarcodeItemforpage(int warehouseId)
        {
            List<Model.Item.PickItemForBarcode> items = new List<Model.Item.PickItemForBarcode>();
            using (var context = new AuthContext())
            {
                items = (await context.PickItemForBarcodeDb.Where(x => x.WarehouseId == warehouseId && x.Status == 1).ToListAsync());

                foreach (var item in items)
                {

                    if (!string.IsNullOrEmpty(item.ImageUrl))
                    {
                        item.ImageUrl = string.Format("{0}://{1}:{2}/{3}", new Uri(HttpContext.Current.Request.Url.AbsoluteUri).Scheme
                                                                , HttpContext.Current.Request.Url.DnsSafeHost
                                                                , (HttpContext.Current.Request.Url.Port != 80 && HttpContext.Current.Request.Url.Port != 443 ? ":" + HttpContext.Current.Request.Url.Port : "")
                                                                , item.ImageUrl);
                    }
                }
            }

            return items;
        }


        [HttpGet]
        [Route("BarcodeItemApproved")]
        public async Task<bool> BarcodeItemApproved(long id, bool status)
        {
            var identity = User.Identity as ClaimsIdentity;
            int userid = 0;
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
            bool result = false;
            using (var context = new AuthContext())
            {
                var item = (await context.PickItemForBarcodeDb.FirstOrDefaultAsync(x => x.Id == id));
                if (item != null)
                {
                    if (status)
                    {
                        var itembarcodes = (await context.ItemBarcodes.Where(x => x.Barcode == item.NewEAN && x.ItemNumber != item.ItemNumber).ToListAsync());
                        if (itembarcodes != null && itembarcodes.Any())
                        {
                            foreach (var barcode in itembarcodes)
                            {
                                barcode.IsActive = false;
                                barcode.IsDeleted = true;
                                barcode.ModifiedBy = userid;
                                barcode.ModifiedDate = DateTime.Now;
                                context.Entry(barcode).State = EntityState.Modified;
                            }
                        }
                        var existings = (await context.ItemBarcodes.Where(x => x.ItemNumber == item.ItemNumber && x.IsActive).ToListAsync());
                        if (existings.Any(x => x.Barcode == item.NewEAN))
                        {
                            var existing = existings.FirstOrDefault(x => x.Barcode == item.NewEAN);
                            existing.IsVerified = true;
                            existing.ModifiedBy = userid;
                            existing.ModifiedDate = DateTime.Now;
                            context.Entry(existing).State = EntityState.Modified;
                        }
                        else
                        {
                            context.ItemBarcodes.Add(new Model.Item.ItemBarcode
                            {
                                IsActive = true,
                                Barcode = item.NewEAN,
                                CreatedBy = userid,
                                CreatedDate = DateTime.Now,
                                IsDeleted = false,
                                IsParentBarcode = !existings.Any(),
                                IsVerified = true,
                                ItemNumber = item.ItemNumber
                            });
                        }

                    }
                    item.ModifiedDate = DateTime.Now;
                    item.ModifiedBy = userid;
                    item.Status = status ? 2 : 3;
                    context.Entry(item).State = EntityState.Modified;
                    result = (await context.CommitAsync()) > 0;
                }
            }
            return result;
        }

        #region PvReconcillation
        [HttpPost]
        [Route("GetPvReconcillationReport")]
        [AllowAnonymous]
        public async Task<APIResponse> PvReconcillationReport(PvRecocillationFilter pv)
        {
            try
            {
                using (AuthContext context = new AuthContext())
                {
                    var wareid = new SqlParameter("@warehouseId", pv.WarehouseId);
                    var date = new SqlParameter("@Startdate", pv.StartDate);
                    var status = new SqlParameter("@Status", pv.Status);
                    var isexport = new SqlParameter("@IsExport", pv.IsExport);
                    var skip = new SqlParameter("@Skip", pv.Skip);
                    var take = new SqlParameter("@Take", pv.Take);
                    var data = context.Database.SqlQuery<PvRecocillationDC>("PVReconcillationReport @warehouseId,@Startdate,@IsExport,@Skip,@Take,@Status", wareid, date, isexport, skip, take, status).ToList();
                    return new APIResponse { Status = true, Data = data };
                }
            }
            catch (Exception ex)
            {
                return new APIResponse { Status = false, Message = ex.Message };
            }
        }
        [HttpGet]
        [Route("GetPvReconcillationDateList")]
        public async Task<APIResponse> PvReconcillationDateList(int warehouseId)
        {
            try
            {
                using (AuthContext db = new AuthContext())
                {
                    var dateList = (from c in db.InventoryCycleHistoryDB
                                    join cc in db.Warehouses on c.WarehoueId equals cc.WarehouseId
                                    where c.WarehoueId == warehouseId && c.TransactionType == "PV" && c.IsStopCurrentStockTrans == true
                                    && cc.active == true && cc.Deleted == false && c.IsActive == true && c.IsDeleted == false
                                    group c.CreatedDate by c.CreatedDate into newdate
                                    orderby newdate.Key
                                    select new
                                    {
                                        Date = newdate.Key
                                    }).OrderByDescending(x => x.Date).ToList();
                    var date = dateList.Select(x => x.Date.ToString("yyyy-MM-dd HH:mm")).ToList();
                    var data = date.GroupBy(x => x).Select(x => x.Key).ToList();
                    var datetime = data.GroupBy(x => x).Select(x => new { Date = DateTime.Parse(x.FirstOrDefault()) }).ToList();

                    return new APIResponse { Status = true, Data = datetime };
                }
            }
            catch (Exception ex)
            {
                return new APIResponse { Status = false, Message = ex.Message };
            }
        }

        #endregion

        #endregion

        public class PvRecocillationFilter
        {
            public int WarehouseId { get; set; }
            public DateTime StartDate { get; set; }
            public string Status { get; set; }
            public bool IsExport { get; set; }
            public int Skip { get; set; }
            public int Take { get; set; }
        }
        public class PvRecocillationDC
        {
            public int StockId { get; set; }
            public string ItemNumber { get; set; }
            public string ItemName { get; set; }
            public int ItemMultiMRPId { get; set; }
            public int CurrentInventory { get; set; }
            public int InventoryCount { get; set; }
            public string isInventory { get; set; }
            public double MRP { get; set; }
            public string WarehouseName { get; set; }
            public string Status { get; set; }
            public string HQComment { get; set; }
            public string WarehouseComment { get; set; }
            public double? APP { get; set; }
            public long TotalRecords { get; set; }
        }

        public class RTPStocksDcBatch
        {
            public int Qty { get; set; }
            public long StockBatchMasterId { get; set; }
        }

        public class scanItem
        {
            public long Id { get; set; }
            public string barcode { get; set; }
            public string ImagePath { get; set; }
        }
        public class inventoryitem
        {
            public int id { get; set; }
            public int ItemMultiMrpId { get; set; }
            public string ItemName { get; set; }
            public string ItemNumber { get; set; }
            public double MRP { get; set; }
        }
        public class UploadInventoryCycleItem
        {
            public int WarehouseId { get; set; }
            public int ItemMultiMrpId { get; set; }
        }

        public class Inventoryapproval
        {
            public int InventoryCycleId { get; set; }
            public bool IsApproval { get; set; }
            public bool IsDeleted { get; set; }
            public int VerifierStatus { get; set; }
            public string Comment { get; set; }
            public bool IsHQ { get; set; }
        }

        public class InventCycleBatchHistoryList
        {
            public string BatchCode { get; set; }
            public int Qty { get; set; }
            public DateTime UpdatedDate { get; set; }
            public string UpdatedBy { get; set; }
            public int DiffInventory { get; set; }
        }
        public class InventCycleHistoryListDC
        {
            public string CreatedBy { get; set; }
            public DateTime CreatedDate { get; set; }
            public string VerifiedBy { get; set; }
            public DateTime? VerifiedTimestamp { get; set; }
            public string ApprovedBy { get; set; }
            public DateTime? ApprovedTimestamp { get; set; }
            public string Status { get; set; }
        }


        public class InventoryStartResDc
        {
            public string Message { get; set; }
            public bool Status { get; set; }
            public string OrderIds { get; set; }
            public bool IsInventoryCycleAutoRTPFound { get; set; }
            public List<BatchMismatchDTO> BatchMismatchData { get; set; }

        }

        public class BatchMismatchDTO
        {
            public long StockBatchMasterId { get; set; }
            public int StockId { get; set; }
            public int ItemMultiMRPId { get; set; }
            public int WarehouseId { get; set; }
            public string ItemNumber { get; set; }
            public int StockQty { get; set; }
            public int BatchQty { get; set; }
            public int CurrentToStockBatchDiff { get; set; }
            public string itemname { get; set; }

        }

        public class warehouesDTO
        {
            public int WarehouseId { get; set; }
            public string WarehouseName { get; set; }
            public bool IsStopCurrentStockTrans { get; set; }
            public bool IsDeliveryOptimizationEnabled { get; set; }
            public bool IsLocationEnabled { get; set; }
            public bool IsPV { get; set; }
            public bool IsPVProcess { get; set; }
            public string TransactionType { get; set; }
            public bool IsInventoryCycleAutoRTPConfirm { get; set; }
            public bool IsInventory { get; set; }

        }

        public class inventoryWarehouseDC
        {
            public int WarehouseId { get; set; }
            public string WarehouseName { get; set; }
            public bool IsStopCurrentStockTrans { get; set; }
            public bool IsClearanceEnabled { get; set; }
            public string Holidays { get; set; }
            public int saturdayflg { get; set; }
            public int Fridayflg { get; set; }
            public int thursdayflg { get; set; }
            public bool isShowBtn { get; set; }
            public string weekDay { get; set; }
            public bool IsInventory { get; set; }
        }

        public class InventoryCycleBatchDc
        {
            public long StockBatchId { get; set; }
            public long BatchMasterId { get; set; }
            public int ItemMultiMrpId { get; set; }
            public string BatchCode { get; set; }
            public string ItemNumber { get; set; }
            public DateTime? MFGDate { get; set; }
            public DateTime? ExpiryDate { get; set; }
            public int Qty { get; set; }
        }
        public class InventoryCycleHistoryDC
        {
            public string WarehoueName { get; set; }
            public bool IsStopCurrentStockTrans { get; set; }
            public DateTime CreatedDate { get; set; }
            public string CreatedBy { get; set; }
            public string TransactionType { get; set; }

        }
        public class WarehousePVHistoryDC
        {
            public bool IsPV { get; set; }
            public string WarehouseName { get; set; }
            public DateTime CreatedDate { get; set; }
            public string CreatedBy { get; set; }
        }
        public class InventoryCycleStatisticsDC
        {
            public int TotalLineItems { get; set; }
            public int CountedLineItem { get; set; }
            public int PendingLineItem { get; set; }
            public double? ApprovedExcessItems { get; set; }
            public double? ApprovedShortItems { get; set; }
            public double? ApprovedItemsDifference { get; set; }
        }
        public class GetInventCycledata
        {
            public int Id
            {
                get; set;
            }

            public int WarehouseId
            {
                get; set;
            }
            public string ItemName
            {
                get; set;
            }
            public string WarehouseName
            {
                get; set;
            }

            public int? InventoryCount
            {
                get; set;
            }
            public string Comment
            {
                get; set;
            }
            public int CurrentInventory
            {
                get; set;
            }
            public DateTime CreatedDate
            {
                get; set;

            }
            public double MRP
            {
                get; set;

            }
            public string ItemNumber { get; set; }
            public string Barcode { get; set; }
            public int ItemMultiMRPId
            {
                get; set;

            }
            public int? PastInventory
            {
                get; set;
            }

            public int ItemId { get; set; }

            public DateTime? UpdatedDate { get; set; }

            public string Updatedby { get; set; }

            public int RtdCount { get; set; }

            public int RtpCount { get; set; }

            public int DamagedQty { get; set; }

            public int NonSellableQty { get; set; }

            public bool IsApproved { get; set; }
            public bool IsDeleted { get; set; }

            public string ABCClassification { get; set; }
            public double APP { get; set; }
            public int? diff { get; set; }
            public int IsWarehouseApproved { get; set; }
            public int VerifierStatus { get; set; }
            public int HQVerifierStatus { get; set; }
            public int zeroDiff { get; set; }
            public string HQComment { get; set; }
            public bool IsPV { get; set; }
            public List<InventoryCycleItemBatchDc> InventoryCycleItemBatchDcs { get; set; }
        }


        public class GetInventCycleZeroDifferencedata
        {
            public int Id
            {
                get; set;
            }

            public int WarehouseId
            {
                get; set;
            }
            public string ItemName
            {
                get; set;
            }
            public string WarehouseName
            {
                get; set;
            }

            public int? InventoryCount
            {
                get; set;
            }
            public string Comment
            {
                get; set;
            }
            public int CurrentInventory
            {
                get; set;
            }
            public DateTime CreatedDate
            {
                get; set;

            }
            public double MRP
            {
                get; set;

            }
            public string ItemNumber { get; set; }
            public string Barcode { get; set; }
            public int ItemMultiMRPId
            {
                get; set;

            }
            public int? PastInventory
            {
                get; set;
            }

            public int ItemId { get; set; }

            public DateTime? UpdatedDate { get; set; }

            public string Updatedby { get; set; }

            public int RtdCount { get; set; }

            public int RtpCount { get; set; }

            public int DamagedQty { get; set; }

            public int NonSellableQty { get; set; }

            public bool IsApproved { get; set; }
            public bool IsDeleted { get; set; }

            public string ABCClassification { get; set; }
            public double APP { get; set; }
            public int? diff { get; set; }
            public int IsWarehouseApproved { get; set; }
            public int VerifierStatus { get; set; }
            public int HQVerifierStatus { get; set; }
            public int zeroDiff { get; set; }
            public string HQComment { get; set; }
            public bool IsPV { get; set; }
            //public List<InventoryCycleItemBatchDc> InventoryCycleItemBatchDcs { get; set; }
        }





        public class ExportInventCycledataItemWise
        {
            public int Id
            {
                get; set;
            }

            public int WarehouseId
            {
                get; set;
            }
            public string ItemName
            {
                get; set;
            }
            public string WarehouseName
            {
                get; set;
            }

            public int? InventoryCount
            {
                get; set;
            }
            public string Comment
            {
                get; set;
            }
            public int CurrentInventory
            {
                get; set;
            }
            public DateTime CreatedDate
            {
                get; set;

            }
            public double MRP
            {
                get; set;

            }
            public string ItemNumber { get; set; }
            public string Barcode { get; set; }
            public int ItemMultiMRPId
            {
                get; set;

            }
            public int? PastInventory
            {
                get; set;
            }

            public int ItemId { get; set; }

            public DateTime? UpdatedDate { get; set; }

            public string Updatedby { get; set; }

            public int RtdCount { get; set; }

            public int RtpCount { get; set; }

            public int DamagedQty { get; set; }

            public int NonSellableQty { get; set; }

            public bool IsApproved { get; set; }
            public bool IsDeleted { get; set; }

            public string ABCClassification { get; set; }
            public double? APP { get; set; }
            public int? diff { get; set; }
            public int IsWarehouseApproved { get; set; }
            public int VerifierStatus { get; set; }
            public int HQVerifierStatus { get; set; }
            public string HQComment { get; set; }
            public bool IsPV { get; set; }
            public string WarehoseApproved { get; set; }
            public string HQApproved { get; set; }
            public string Createdby { get; set; }
            public string Status { get; set; }
        }

        public class ExportInventCycledata
        {

            public string ItemName
            {
                get; set;
            }
            public string WarehouseName
            {
                get; set;
            }

            public int? InventoryCount
            {
                get; set;
            }
            public string Comment
            {
                get; set;
            }
            public int CurrentInventory
            {
                get; set;
            }
            public DateTime CreatedDate
            {
                get; set;

            }
            public double MRP
            {
                get; set;

            }
            public string ItemNumber { get; set; }
            public int ItemMultiMRPId
            {
                get; set;

            }
            public int? PastInventory
            {
                get; set;
            }
            public int Diff { get; set; }
            public DateTime? UpdatedDate { get; set; }

            public string Updatedby { get; set; }

            public int RtdCount { get; set; }

            public int RtpCount { get; set; }

            public int DamagedQty { get; set; }

            public int NonSellableQty { get; set; }

            public string Status { get; set; }

            public string BatchCode { get; set; }
            public DateTime? MFGDate { get; set; }
            public DateTime? ExpiryDate { get; set; }
            public int? batchInventoryCount { get; set; }
            public int batchDamagedQty { get; set; }
            public int batchNonSellableQty { get; set; }
            public string ABCClassification { get; set; }
            public int? BatchPastInventory { get; set; }
            public int? batchRtpCount { get; set; }
            public int batchDiff { get; set; }
            public double? APP { get; set; }
        }

        public class PaggingDTO
        {
            public List<GetInventCycledata> GetInventCycledatadto { get; set; }
            public int total_count { get; set; }

        }
        public class PaggingZeroDifferenceDTO
        {
            public List<GetInventCycleZeroDifferencedata> GetInventCycleZeroDifferencedata { get; set; }
            public int total_count { get; set; }

        }

        public class Supervisors
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }

        public class response
        {
            public bool Status { get; set; }
            public string msg { get; set; }
        }

        public class SupervisorInventoryCycleDayDc
        {
            public string WarehouseName { get; set; }
            public int PeopleId { get; set; }
            public string DisplayName { get; set; }
            public DateTime AssignDate { get; set; }
            public int TotalItem { get; set; }

        }

        public class SupervisorInventoryCycleDay
        {
            [MongoDB.Bson.Serialization.Attributes.BsonId]
            public MongoDB.Bson.ObjectId Id { get; set; }
            public int WarehouseId { get; set; }
            public string WarehouseName { get; set; }
            public int PeopleId { get; set; }
            public string DisplayName { get; set; }
            [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
            public DateTime AssignDate { get; set; }
            public bool IsSubmitted { get; set; }
            public bool? IsPV { get; set; }
            public List<SupervisorInventoryCycleItem> InventoryCycleItems { get; set; }
        }

        public class SupervisorInventoryCycleItem
        {
            public int Id { get; set; }
            public int ItemMultiMRPId { get; set; }
            public string ItemNumber { get; set; }
            public double MRP { get; set; }
            public string ItemName { get; set; }
            public string ABCClassification { get; set; }
            public List<string> Barcode { get; set; }
            public int InventoryCount { get; set; }
            public int DamagedQty { get; set; }
            public int NonSellableQty { get; set; }
            public string Comment { get; set; }
            public bool IsRejected { get; set; }
            public List<InventoryCycleItemBatch> ItemBatch { get; set; }
        }

        public class SupervisorInventoryCycleDayDC
        {
            public string Id { get; set; }
            public int WarehouseId { get; set; }
            public string WarehouseName { get; set; }
            public int PeopleId { get; set; }
            public string DisplayName { get; set; }
            public DateTime AssignDate { get; set; }
            public bool IsSubmitted { get; set; }
            public List<SupervisorInventoryCycleItem> InventoryCycleItems { get; set; }
            public SupervisorInventoryCycleItemDc SupervisorInventoryCycleItemDc { get; set; }
        }

        public class ScanInventoryCycleItemDc
        {
            public int Id { get; set; }
            public int ItemMultiMRPId { get; set; }
            public string ItemNumber { get; set; }
            public double MRP { get; set; }
        }
        public class SupervisorInventoryCycleItemDc
        {
            public int Id { get; set; }
            public int ItemMultiMRPId { get; set; }
            public string ItemNumber { get; set; }
            public double MRP { get; set; }
            public string ItemName { get; set; }
            public string ABCClassification { get; set; }
            public List<string> Barcode { get; set; }
            public int status { get; set; }
            public string Comment { get; set; }
            public List<InventoryCycleBatchDc> BatchDcs { get; set; }
            public List<InventoryCycleItemBatch> ItemBatch { get; set; }
            public List<ItemBatchDc> AllItemBatch { get; set; }
        }

        public class InventoryCycleItemBatch
        {
            public long StockBatchId { get; set; }
            public long BatchMasterId { get; set; }
            public string BatchCode { get; set; }
            public DateTime? MFGDate { get; set; }
            public DateTime? ExpiryDate { get; set; }
            public int InventoryCount { get; set; }
            public int DamagedQty { get; set; }
            public int NonSellableQty { get; set; }
            public string DamagedImageUrl { get; set; }
            public string NonSellableImageUrl { get; set; }

            public int AvailableQty { get; set; }
        }


        public class InventoryCycleItemBatchDc
        {
            public int Id { get; set; }
            public int WarehouseId { get; set; }
            public int PeopleId { get; set; }
            public int ItemMultiMrpId { get; set; }
            public long BatchMasterId { get; set; }
            public long StockBatchId { get; set; }
            public string BatchCode { get; set; }
            public DateTime? MFGDate { get; set; }
            public DateTime? ExpiryDate { get; set; }
            public int InventoryCount { get; set; }
            public int DamagedQty { get; set; }
            public int NonSellableQty { get; set; }
            public string DamagedImageUrl { get; set; }
            public string NonSellableImageUrl { get; set; }
            public int InventCycle_Id { get; set; }

            public int? PastInventory { get; set; }
            public int Rtp { get; set; }
            public string Comment { get; set; }
            public int DiffInventory { get; set; }
            public int Qty { get; set; }
            public bool IsStockUpdated { get; set; }
            public int InventCycleBatchId { get; set; }
        }

        public class InventoryCycleEditDc
        {
            public int Id { get; set; }
            public int ItemMultiMRPId { get; set; }
            public int InventoryCount { get; set; }
            public int rtpQty { get; set; }
            public int rtdQty { get; set; }
            public int CurrentInventory { get; set; }
        }

        public class RTPStocksDc
        {
            public int Qty { get; set; }
            public int ItemMultiMRPId { get; set; }
        }


        public class InventoryCycleItemBarcodeDc
        {
            public int WarehouseId { get; set; }
            public int PeopleId { get; set; }
            public int ItemMultiMrpId { get; set; }

        }



        public class UpdateInventoryBatchDc
        {
            public int Id { get; set; }
            public int InventoryCount { get; set; }
            public int DamagedQty { get; set; }
            public int NonSellableQty { get; set; }
        }

        public class raminingInventory
        {
            public int ItemMultiMRPId { get; set; }
            public int WarehouseId { get; set; }
            public string classification { get; set; }
            public string ItemName { get; set; }
            public string ItemNumber { get; set; }
            public double MRP { get; set; }
            public long InventoryCycleMasterId { get; set; }
        }


        public class InventCycleBatchExtraQty
        {
            public int InventCycleBatchId { get; set; }
            public long StockBatchMasterId { get; set; }
            public int Qty { get; set; }
            public int Sequence { get; set; }
        }

        public class BatchCurrentVsInvCountDc
        {
            public long StockBatchMasterId { get; set; }
            public int Qty { get; set; }
        }





    }
}