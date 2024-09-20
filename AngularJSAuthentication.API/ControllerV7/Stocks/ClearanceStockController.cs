using AngularJSAuthentication.API.Controllers;
using AngularJSAuthentication.API.Helper;
using AngularJSAuthentication.Common.Helpers;
using AngularJSAuthentication.DataContracts.constants;
using AngularJSAuthentication.DataContracts.Transaction.Ledger.ItemLedger;
using AngularJSAuthentication.DataContracts.Transaction.Stocks;
using AngularJSAuthentication.Model;
using AngularJSAuthentication.Model.Stocks;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Transactions;
using System.Web.Http;

namespace AngularJSAuthentication.API.ControllerV7.Stocks
{
    [RoutePrefix("api/ClearanceStock")]
    public class ClearanceStockController : ApiController
    {
        private DateTime indianTime;

        [Authorize]
        [Route("GetPendingClearance")]
        [HttpPost]
        public PaggingData GetPendingClearance(FilterOrderDataDTOs filterOrderData)
        {
            try
            {
                PaggingData paggingData = new PaggingData();
                using (var db = new AuthContext())
                {
                    int skip = (filterOrderData.PageNo - 1) * filterOrderData.ItemPerPage;
                    int take = filterOrderData.ItemPerPage;

                    var identity = User.Identity as ClaimsIdentity;
                    int userid = 0;

                    if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                        userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

                    //string sqlquery = "Select CS.ItemNumber,DR.WarehouseId,DR.ItemMultiMRPId,DS.ReasonToTransfer,AC.Id,AC.EntityId,DR.Qty,DR.itemName,cs.WarehouseName,sum(DR.Qty * DS.PurchasePrice ) as Amount,AC.Action,DR.CreatedDate"
                    //                 + "   from DamageRequests DR join ApprovalConfigurations AC on AC.EntityId = DR.Id join CurrentStocks CS"
                    //                 + "   on CS.ItemMultiMRPId = DR.ItemMultiMRPId and"
                    //                 + "    cs.WarehouseId = DR.WarehouseId join DamageStocks DS on Ds.ItemMultiMRPId = DR.ItemMultiMRPId and DS.WarehouseId = DR.WarehouseId"
                    //                 + "  where AC.Action = 0  and AC.IsDeleted = 0 and DR.IsDeleted = 0 ";
                    //if (filterOrderData.WarehouesId > 0)
                    //{
                    //    sqlquery += " and DR.WarehouseId = " + filterOrderData.WarehouesId + " ";
                    //}
                    //sqlquery += " group by CS.ItemNumber,DR.WarehouseId,DR.ItemMultiMRPId,DS.ReasonToTransfer,AC.Id,AC.EntityId,DR.Qty,DR.itemName,cs.WarehouseName,AC.Action,DR.CreatedDate ";
                    //List<PurchaseOrderMasterDTOs> PRSts = db.Database.SqlQuery<PurchaseOrderMasterDTOs>(sqlquery).OrderByDescending(a => a.Id).ToList();
                    //var PRStsdata = PRSts.Skip(skip).Take(take).ToList();
                    //int dataCount = PRSts.Count();
                    //if (PRSts != null && PRSts.Any())
                    //{
                    //    paggingData.total_count = dataCount;
                    //}
                    //paggingData.ordermaster = PRStsdata;
                    return paggingData;
                }
            }
            catch (Exception ex)
            {
                return null;
            }
        }


        [Authorize]
        [Route("GetApprovedClearance")]
        [HttpPost]
        public PaggingData GetApprovedClearance(FilterOrderDataDTOs filterOrderData)
        {
            try
            {
                PaggingData paggingData = new PaggingData();
                using (var db = new AuthContext())
                {
                    int skip = (filterOrderData.PageNo - 1) * filterOrderData.ItemPerPage;
                    int take = filterOrderData.ItemPerPage;

                    var identity = User.Identity as ClaimsIdentity;
                    int userid = 0;

                    if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                        userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

                    //string sqlquery = "Select CS.ItemNumber,DR.WarehouseId,DR.ItemMultiMRPId,DS.ReasonToTransfer,AC.Id,AC.EntityId,DR.Qty,DR.itemName,cs.WarehouseName,sum(DR.Qty * DS.PurchasePrice ) as Amount,AC.Action,DR.CreatedDate"
                    //                 + "   from DamageRequests DR join ApprovalConfigurations AC on AC.EntityId = DR.Id join CurrentStocks CS"
                    //                 + "   on CS.ItemMultiMRPId = DR.ItemMultiMRPId and"
                    //                 + "    cs.WarehouseId = DR.WarehouseId join DamageStocks DS on Ds.ItemMultiMRPId = DR.ItemMultiMRPId and DS.WarehouseId = DR.WarehouseId"
                    //                 + "  where AC.Action = 1  and AC.IsDeleted = 0 and DR.IsDeleted = 0 and DR.WarehouseId = " + filterOrderData.WarehouesId + " group by CS.ItemNumber,DR.WarehouseId,DR.ItemMultiMRPId,DS.ReasonToTransfer,AC.Id,AC.EntityId,DR.Qty,DR.itemName,cs.WarehouseName,AC.Action,DR.CreatedDate"; List<PurchaseOrderMasterDTOs> PRSts = db.Database.SqlQuery<PurchaseOrderMasterDTOs>(sqlquery).OrderByDescending(a => a.Id).ToList();
                    //var PRStsdata = PRSts.Skip(skip).Take(take).ToList();
                    //int dataCount = PRSts.Count();
                    //if (PRSts != null && PRSts.Any())
                    //{
                    //    paggingData.total_count = dataCount;
                    //}
                    //paggingData.ordermaster = PRStsdata;
                    return paggingData;
                }
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        [Authorize]
        [Route("GetRejectClearance")]
        [HttpPost]
        public PaggingData GetRejectClearance(FilterOrderDataDTOs filterOrderData)
        {
            try
            {
                PaggingData paggingData = new PaggingData();
                using (var db = new AuthContext())
                {
                    int skip = (filterOrderData.PageNo - 1) * filterOrderData.ItemPerPage;
                    int take = filterOrderData.ItemPerPage;

                    var identity = User.Identity as ClaimsIdentity;
                    int userid = 0;

                    if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                        userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

                    //string sqlquery = "Select AC.Comment,CS.ItemNumber,DR.WarehouseId,DR.ItemMultiMRPId,DS.ReasonToTransfer,AC.Id,AC.EntityId,DR.Qty,DR.itemName,cs.WarehouseName,sum(DR.Qty * DS.PurchasePrice ) as Amount,AC.Action,DR.CreatedDate"
                    //                 + "   from DamageRequests DR join ApprovalConfigurations AC on AC.EntityId = DR.Id join CurrentStocks CS"
                    //                 + "   on CS.ItemMultiMRPId = DR.ItemMultiMRPId and"
                    //                 + "    cs.WarehouseId = DR.WarehouseId join DamageStocks DS on Ds.ItemMultiMRPId = DR.ItemMultiMRPId and DS.WarehouseId = DR.WarehouseId"
                    //                 + "  where AC.Action = 2  and AC.IsDeleted = 0 and DR.IsDeleted = 0 and DR.WarehouseId = " + filterOrderData.WarehouesId + " group by AC.Comment,CS.ItemNumber,DR.WarehouseId,DR.ItemMultiMRPId,DS.ReasonToTransfer,AC.Id,AC.EntityId,DR.Qty,DR.itemName,cs.WarehouseName,AC.Action,DR.CreatedDate";
                    //List<PurchaseOrderMasterDTOs> PRSts = db.Database.SqlQuery<PurchaseOrderMasterDTOs>(sqlquery).OrderByDescending(a => a.Id).ToList();
                    //var PRStsdata = PRSts.Skip(skip).Take(take).ToList();
                    //int dataCount = PRSts.Count();
                    //if (PRSts != null && PRSts.Any())
                    //{
                    //    paggingData.total_count = dataCount;
                    //}
                    //paggingData.ordermaster = PRStsdata;
                    return paggingData;
                }
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        [Route("StockHistory")]
        [HttpGet]
        public PaggingStockHistoryData GetStockHistory(int skip, int take, long stockId, string stockType)
        {
            PaggingStockHistoryData paggingStockHistoryData = new PaggingStockHistoryData();

            using (var authcontext = new AuthContext())
            {
                if (authcontext.Database.Connection.State != ConnectionState.Open)
                    authcontext.Database.Connection.Open();
                var cmd = authcontext.Database.Connection.CreateCommand();
                var keyParam = new SqlParameter
                {
                    ParameterName = "StockId",
                    Value = stockId
                };
                cmd.CommandText = "[dbo].[GetStockHistory]";

                cmd.Parameters.Add(new SqlParameter("@StockType", stockType));
                cmd.Parameters.Add(keyParam);
                cmd.Parameters.Add(new SqlParameter("@skip", skip));
                cmd.Parameters.Add(new SqlParameter("@take", take));
                cmd.CommandType = CommandType.StoredProcedure;
                var reader = cmd.ExecuteReader();
                paggingStockHistoryData.StockHistoryData = ((IObjectContextAdapter)authcontext).ObjectContext.Translate<StockHistoryDC>(reader).ToList();

                reader.NextResult();

                if (reader.Read())
                {
                    paggingStockHistoryData.TotalRecords = Convert.ToInt32(reader["TotalRecords"]);
                }
                return paggingStockHistoryData;

            }

        }

        [Route("ExportStockHistory")]
        [HttpGet]
        public PaggingStockHistoryData ExportStockHistory(long stockId, string stockType)
        {
            PaggingStockHistoryData paggingStockHistoryData = new PaggingStockHistoryData();

            using (var authcontext = new AuthContext())
            {
                if (authcontext.Database.Connection.State != ConnectionState.Open)
                    authcontext.Database.Connection.Open();
                var cmd = authcontext.Database.Connection.CreateCommand();
                var keyParam = new SqlParameter
                {
                    ParameterName = "StockId",
                    Value = stockId
                };
                cmd.CommandText = "[dbo].[ExportStockHistory]";

                cmd.Parameters.Add(new SqlParameter("@StockType", stockType));
                cmd.Parameters.Add(keyParam);
                cmd.CommandType = CommandType.StoredProcedure;
                var reader = cmd.ExecuteReader();
                paggingStockHistoryData.StockHistoryData = ((IObjectContextAdapter)authcontext).ObjectContext.Translate<StockHistoryDC>(reader).ToList();
                reader.NextResult();
                return paggingStockHistoryData;

            }

        }


    }
    //public class DBOYinfo1
    //{
    //    //public List<dbinf> ids { get; set; }
    //    public int Warehouseid { get; set; }

    //    public List<int> ids { get; set; }
    //}

    //public class ClearanceStockItemDc
    //{
    //    public int StockId { get; set; }
    //    public string ItemName { get; set; }
    //    public string ItemNumber { get; set; }
    //    public int ItemMultiMRPId { get; set; }
    //    public double MRP { get; set; }
    //    public int CurrentInventory { get; set; }
    //    public string Cateogry { get; set; }

    //    public string ABCClassification { get; set; }

    //    public int WarehouseId { get; set; }

    //}

    //public class ClearanceStockDc
    //{
    //    public int EntityId { get; set; }
    //    public int ClearanceStockId { get; set; }
    //    public int WarehouseId { get; set; }
    //    public string WarehouseName { get; set; }
    //    public int StockId { get; set; }
    //    public string ItemNumber { get; set; }
    //    public string ItemName { get; set; }
    //    public string ReasonToTransferClearance { get; set; }
    //    public int ClearanceInventory { get; set; }
    //    public int ItemMultiMRPId { get; set; }

    //    public string ABCClassification { get; set; }
    //    public int Stocktype { get; set; }
    //}

    //public class ClearanceResult
    //{
    //    public bool Status { get; set; }
    //    public string Message { get; set; }
    //}
    //public class ClearanceAprprover
    //{
    //    public int PeopleId { get; set; }
    //    public string DisplayName { get; set; }
    //}
}