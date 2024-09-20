using AngularJSAuthentication.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http.Description;
using System.Security.Claims;
using System.Web.Http;
using NLog;
using System.Collections.Concurrent;
using System.Data.Entity;
using System.Configuration;
using System.Net.Mail;
using LinqKit;
using AngularJSAuthentication.API.Models;
using System.Transactions;
using AngularJSAuthentication.DataContracts.Transaction.Stocks;
using AngularJSAuthentication.API.Helper;
using AngularJSAuthentication.DataContracts.constants;
using AngularJSAuthentication.Model.Stocks;
using System.Data.SqlClient;
using AngularJSAuthentication.DataContracts.Shared;
using AngularJSAuthentication.BusinessLayer.Managers.Reports;
using AgileObjects.AgileMapper;
using AngularJSAuthentication.Common.Helpers;
using AngularJSAuthentication.DataContracts.Transaction.Ledger.ItemLedger;
using AngularJSAuthentication.DataLayer.Repositories.Transactions.BatchCode;
using AngularJSAuthentication.DataContracts.BatchCode;
using AngularJSAuthentication.BusinessLayer.Managers.Transactions.BatchCode;
using AngularJSAuthentication.API.Managers;
using AngularJSAuthentication.DataContracts.ROC;
using System.Web.Hosting;
using System.IO;
using System.Data;

namespace AngularJSAuthentication.API.Controllers
{
    [RoutePrefix("api/CurrentStock")]
    public class CurrentStockController : ApiController
    {

        private static TimeZoneInfo INDIAN_ZONE = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
        DateTime indianTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE);
        private static Logger logger = LogManager.GetCurrentClassLogger();


        #region Warehouse based get data
        [Route("GetWarehousebased")]
        public async Task<List<CurrentStockDC>> GetWarehousebased(int WarehouseId)
        {
            using (AuthContext context = new AuthContext())
            {
                List<CurrentStockDC> result = new List<CurrentStockDC>();
                if (WarehouseId > 0)
                {
                    result =await GetAllCurrentStock(WarehouseId, context);
                }
                logger.Info("End  current stock: ");
                return result;
            }
        }
        #endregion

        public async Task<List<CurrentStockDC>> GetAllCurrentStock(int WarehouseId, AuthContext context)
        {
            string Query = "Exec GetAllCurrentStock " + WarehouseId;
            context.Database.CommandTimeout = 300;
            var _result = await context.Database.SqlQuery<CurrentStockDC>(Query).ToListAsync();
            return _result;
        }

        //[Authorize]
        [Route("")]
        public CurrentStock Get(int id)
        {
            logger.Info("start current stock: ");
            using (AuthContext context = new AuthContext())
            {
                CurrentStock ass = new CurrentStock();
                try
                {
                    var identity = User.Identity as ClaimsIdentity;
                    int compid = 0, userid = 0;

                    foreach (Claim claim in identity.Claims)
                    {
                        if (claim.Type == "compid")
                        {
                            compid = int.Parse(claim.Value);
                        }
                        if (claim.Type == "userid")
                        {
                            userid = int.Parse(claim.Value);
                        }
                    }
                    int CompanyId = compid;
                    logger.Info("User ID : {0} , Company Id : {1}", compid, userid);

                    //ass = context.GetCurrentStock(id, CompanyId);
                    logger.Info("End  current stock: ");
                    return ass;
                }
                catch (Exception ex)
                {
                    logger.Error("Error in current stock" + ex.Message);
                    logger.Info("End  current stock: ");
                    return null;
                }
            }
        }

        [ResponseType(typeof(CurrentStock))]
        [Route("")]
        [AcceptVerbs("Delete")]
        public void Remove(int id)
        {
            logger.Info("start deleteCityy: ");
            using (AuthContext context = new AuthContext())
            {
                try
                {

                    var identity = User.Identity as ClaimsIdentity;
                    int compid = 0, userid = 0;
                    foreach (Claim claim in identity.Claims)
                    {
                        if (claim.Type == "compid")
                        {
                            compid = int.Parse(claim.Value);
                        }
                        if (claim.Type == "userid")
                        {
                            userid = int.Parse(claim.Value);
                        }
                    }
                    int CompanyId = compid;
                    if (CompanyId == 0)
                    {
                        throw new ArgumentNullException("item");
                    }
                    logger.Info("User ID : {0} , Company Id : {1}", compid, userid);
                    context.DeleteCurrentStock(id, CompanyId);
                    logger.Info("End  delete City: ");
                }
                catch (Exception ex)
                {

                    logger.Error("Error in deleteCity" + ex.Message);


                }
            }
        }

        [Route("")]
        // [HttpGet]
        public PaggingData_st GetD(int list, int page, string ItemNumber, int WarehouseId, int StockId)
        {

            logger.Info("start OrderMaster: ");
            using (AuthContext context = new AuthContext())
            {
                //  List<OrderMaster> ass = new List<OrderMaster>();
                try
                {
                    var identity = User.Identity as ClaimsIdentity;
                    int compid = 0, userid = 0;
                    foreach (Claim claim in identity.Claims)
                    {
                        if (claim.Type == "compid")
                        {
                            compid = int.Parse(claim.Value);
                        }
                        if (claim.Type == "userid")
                        {
                            userid = int.Parse(claim.Value);
                        }
                    }
                    int CompanyId = compid;
                    if (CompanyId == 0)
                    {
                        throw new ArgumentNullException("item");
                    }
                    logger.Info("User ID : {0} , Company Id : {1}", compid, userid);
                    var ass = context.AllItemHistory(list, page, ItemNumber, WarehouseId, StockId);
                    logger.Info("End OrderMaster: ");
                    return ass;
                }
                catch (Exception ex)
                {
                    logger.Error("Error in OrderMaster " + ex.Message);
                    logger.Info("End  OrderMaster: ");
                    return null;
                }
            }
        }


        [Route("Safetystockpagging")]
        // [HttpGet]
        public PaggingData_st Getsafetystockdata(int list, int page, int WarehouseId)
        {

            logger.Info("start OrderMaster: ");
            using (AuthContext context = new AuthContext())
            {
                //  List<OrderMaster> ass = new List<OrderMaster>();
                try
                {
                    var identity = User.Identity as ClaimsIdentity;
                    int compid = 0, userid = 0;
                    foreach (Claim claim in identity.Claims)
                    {
                        if (claim.Type == "compid")
                        {
                            compid = int.Parse(claim.Value);
                        }
                        if (claim.Type == "userid")
                        {
                            userid = int.Parse(claim.Value);
                        }
                    }
                    int CompanyId = compid;
                    if (CompanyId == 0)
                    {
                        throw new ArgumentNullException("item");
                    }
                    logger.Info("User ID : {0} , Company Id : {1}", compid, userid);
                    // var ass = context.AllItemHistory(list, page, WarehouseId);
                    List<CurrentStockHistory> newdata = new List<CurrentStockHistory>();
                    var listOrders = context.CurrentStockHistoryDb.Where(x => x.Deleted == false && x.Warehouseid == WarehouseId).OrderByDescending(x => x.CreationDate).Skip((page - 1) * list).Take(list).ToList();
                    newdata = listOrders;
                    PaggingData_st obj = new PaggingData_st();
                    obj.total_count = context.CurrentStockHistoryDb.Where(x => x.Deleted == false && x.Warehouseid == WarehouseId).Count();
                    obj.ordermaster = newdata;
                    logger.Info("End OrderMaster: ");
                    return obj;
                }
                catch (Exception ex)
                {
                    logger.Error("Error in OrderMaster " + ex.Message);
                    logger.Info("End  OrderMaster: ");
                    return null;
                }
            }
        }

        [Route("GetEmptyStockItem")]
        [HttpGet]
        public IEnumerable<CurrentStock> GetEmptyStockItem()
        {
            logger.Info("start current stock: ");
            using (AuthContext context = new AuthContext())
            {
                List<CurrentStock> ass = new List<CurrentStock>();
                try
                {
                    var identity = User.Identity as ClaimsIdentity;
                    int compid = 0, userid = 0;
                    int Warehouse_id = 0;


                    foreach (Claim claim in identity.Claims)
                    {
                        if (claim.Type == "compid")
                        {
                            compid = int.Parse(claim.Value);
                        }
                        if (claim.Type == "userid")
                        {
                            userid = int.Parse(claim.Value);
                        }
                        if (claim.Type == "Warehouseid")
                        {
                            Warehouse_id = int.Parse(claim.Value);
                        }

                    }
                    int CompanyId = compid;
                    if (Warehouse_id > 0)
                    {
                        logger.Info("User ID : {0} , Company Id : {1}", compid, userid);
                        ass = context.GetAllEmptyStockItem(CompanyId, Warehouse_id).ToList();
                        logger.Info("End  current stock: ");
                        return ass;
                    }

                    else
                    {
                        logger.Info("User ID : {0} , Company Id : {1}", compid, userid);
                        ass = context.GetAllEmptyStock().ToList();
                        logger.Info("End  current stock: ");
                        return ass;
                    }

                }
                catch (Exception ex)
                {
                    logger.Error("Error in current stock " + ex.Message);
                    logger.Info("End  current stock: ");
                    return null;
                }
            }
        }
        [Route("GetEmptyStockItemForWeb")]
        [HttpGet]
        public IEnumerable<CurrentStock> GetEmptyStockItemForWeb()
        {
            logger.Info("start current stock: ");
            using (AuthContext context = new AuthContext())
            {
                List<CurrentStock> ass = new List<CurrentStock>();
                try
                {
                    var identity = User.Identity as ClaimsIdentity;
                    int compid = 0, userid = 0;
                    int Warehouse_id = 0;


                    foreach (Claim claim in identity.Claims)
                    {
                        if (claim.Type == "compid")
                        {
                            compid = int.Parse(claim.Value);
                        }
                        if (claim.Type == "userid")
                        {
                            userid = int.Parse(claim.Value);
                        }
                        if (claim.Type == "Warehouseid")
                        {
                            Warehouse_id = int.Parse(claim.Value);
                        }

                    }
                    int CompanyId = compid;
                    if (Warehouse_id > 0)
                    {
                        logger.Info("User ID : {0} , Company Id : {1}", compid, userid);
                        ass = context.GetAllEmptyStockItemForWeb(CompanyId, Warehouse_id).ToList();
                        logger.Info("End  current stock: ");
                        return ass;
                    }

                    else
                    {
                        //logger.Info("User ID : {0} , Company Id : {1}", compid, userid);
                        //ass = context.GetAllEmptyStockItemForWeb().ToList();
                        //logger.Info("End  current stock: ");
                        return ass;
                    }

                }
                catch (Exception ex)
                {
                    logger.Error("Error in current stock " + ex.Message);
                    logger.Info("End  current stock: ");
                    return null;
                }
            }
        }
        // removed by Harry : 21 May 2019 FindItemHighDP
        [Route("GetEmptyStockItem")]
        [HttpGet]
        public IEnumerable<CurrentStock> GetEmptyStockItem(int CompanyId, int Warehouse_id)
        {
            logger.Info("start current stock: ");
            using (AuthContext context = new AuthContext())
            {
                List<CurrentStock> ass = new List<CurrentStock>();
                try
                {
                    if (Warehouse_id > 0)
                    {
                        ass = context.GetAllEmptyStockItem(CompanyId, Warehouse_id).ToList();
                        logger.Info("End  current stock: ");
                        // return ass;
                        return null;
                    }

                    else
                    {
                        ass = context.GetAllEmptyStock().ToList();
                        logger.Info("End  current stock: ");
                        //return ass;
                        return null;
                    }

                }
                catch (Exception ex)
                {
                    logger.Error("Error in current stock " + ex.Message);
                    logger.Info("End  current stock: ");
                    return null;
                }
            }
        }

        [Route("Export")]
        [HttpGet]
        public HttpResponseMessage GetExport(int StockId, int WarehouseId)
        {
            using (AuthContext db = new AuthContext())
            {
                List<CurrentStockHistory> Data = new List<CurrentStockHistory>();
                Data = db.CurrentStockHistoryDb.Where(x => x.StockId == StockId && x.Warehouseid == WarehouseId).OrderByDescending(x => x.CreationDate).ThenByDescending(x => x.id).ToList();

                if (Data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, "No record");
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.OK, Data);
                }
            }

        }
        [Route("AdjGetWarehousebased")]
        [HttpGet]
        public async Task<List<CurrentStockDC>> AdjGetWarehousebased(int WarehouseId)
        {
            List<CurrentStockDC> result = new List<CurrentStockDC>();

            using (AuthContext context = new AuthContext())
            {

                if (WarehouseId > 0)
                {
                    result =await GetAllCurrentStock(WarehouseId, context);
                }

            }
            return result;

        }
        /// <summary>
        /// Get item movement report
        /// Date: 11/06/2019
        /// </summary>
        /// <param name="WarehouseId"></param>
        /// <returns></returns>
        [Route("Getinvreport")]
        [HttpGet]
        public async Task<List<InvReportDto>> GetReport(int WarehouseId, bool Isactiva)
        {
            DateTime MydateLastSaven = indianTime.AddDays(-7);
            DateTime MydateLastFifteen = indianTime.AddDays(-15);
            DateTime MydateLastThirty = indianTime.AddDays(-30);

            List<InvReportDto> IRD = new List<InvReportDto>();
            List<ItemMaster> Data = new List<ItemMaster>();
            List<ItemClassificationDC> adddata = new List<ItemClassificationDC>();
            using (AuthContext Authdb = new AuthContext())
            {
                #region commented copde for get data using foreach
                var Stock = Authdb.DbCurrentStock.Where(x => x.Deleted == false && x.WarehouseId == WarehouseId && x.CurrentInventory > 0).ToList();
                List<CurrentStockHistory> CSHList = Authdb.CurrentStockHistoryDb.Where(a => a.Warehouseid == WarehouseId && a.CreationDate >= MydateLastThirty).ToList();

                foreach (var im in Stock)
                {
                    int? CS1;
                    int? CS2;
                    int? CS3;
                    if (im != null)
                    {
                        List<CurrentStockHistory> CSH = CSHList.Where(a => a.StockId == im.StockId && a.CreationDate >= MydateLastThirty).OrderByDescending(a => a.CreationDate).ToList();
                        if (CSH.Count() > 0)
                        {
                            int? CSHLastSaven = CSH.Where(a => a.CreationDate >= MydateLastSaven).Select(a => a.TotalInventory).FirstOrDefault();
                            int? CSHLastFifteen = CSH.Where(a => a.CreationDate >= MydateLastFifteen && a.CreationDate < MydateLastSaven).Select(a => a.TotalInventory).FirstOrDefault();
                            int? CSHLastThirty = CSH.Where(a => a.CreationDate >= MydateLastThirty && a.CreationDate < MydateLastFifteen).Select(a => a.TotalInventory).FirstOrDefault();
                            int? ACS = CSH.OrderByDescending(a => a.CreationDate).Select(a => a.TotalInventory).FirstOrDefault();

                            CS1 = CSHLastSaven != null ? CSHLastSaven : ACS;
                            CS2 = CSHLastFifteen != null ? CSHLastFifteen : ACS;
                            CS3 = CSHLastThirty != null ? CSHLastThirty : ACS;

                            List<ItemClassificationDC> ABCitemsList = Stock.Select(item => new ItemClassificationDC { ItemNumber = item.ItemNumber, WarehouseId = item.WarehouseId }).ToList();

                            var manager = new ItemLedgerManager();
                            var GetItem = await manager.GetItemClassificationsAsync(ABCitemsList);
                            InvReportDto inv = new InvReportDto()
                            {
                                ItemMultiMRPId = im.ItemMultiMRPId,
                                ItemNumber = im.ItemNumber,
                                StockId = im.StockId,
                                MRP = im.MRP,
                                itemname = im.itemname,
                                ClosingStock = im.CurrentInventory,
                                Lastsavendays = CS1,
                                Lastfifteendays = CS2,
                                LastThirtydays = CS3,
                                WarehouseName = im.WarehouseName,
                                Category = GetItem.Where(x => x.ItemNumber == im.ItemNumber).Select(x => x.Category).FirstOrDefault() != null ? GetItem.Where(x => x.ItemNumber == im.ItemNumber).Select(x => x.Category).FirstOrDefault() : "D"
                            };
                            IRD.Add(inv);
                        }
                    }
                }
                IRD.OrderByDescending(a => a.ClosingStock);
                return IRD;
                #endregion
            }
        }
        #region transfer current stock 
        /// <summary>
        /// Created Date:19/07/2019
        /// Created By Raj
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        [Authorize]
        [Route("StockTransfer")]
        [HttpPut]
        public HttpResponseMessage Put(TransferStockDTO item)
        {
            var identity = User.Identity as ClaimsIdentity;
            int compid = 0, userid = 0;
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))
                compid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);

            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

            if (userid > 0)
            {
                TransactionOptions option = new TransactionOptions();
                option.IsolationLevel = System.Transactions.IsolationLevel.RepeatableRead; //(System.Transactions.IsolationLevel)System.Data.IsolationLevel.RepeatableRead;
                option.Timeout = TimeSpan.FromSeconds(90);
                using (var dbContextTransaction = new TransactionScope(TransactionScopeOption.Required, option))
                {

                    using (AuthContext db = new AuthContext())
                    {
                        var warehouse = db.Warehouses.FirstOrDefault(x => x.WarehouseId == item.WarehouseId);

                        if (warehouse.IsStopCurrentStockTrans)
                            return Request.CreateResponse(HttpStatusCode.InternalServerError, "Inventory Transactions are currently disabled for this warehouse... Please try after some time");


                        #region via virtual stock
                        MultiStockHelper<OnMultiMRPTransferDc> MultiStockHelpers = new MultiStockHelper<OnMultiMRPTransferDc>();
                        List<OnMultiMRPTransferDc> StockList = new List<OnMultiMRPTransferDc>();
                        StockList.Add(new OnMultiMRPTransferDc
                        {
                            WarehouseId = item.WarehouseId,
                            SourceItemMultiMRPId = item.ItemMultiMRPId,
                            DestinationItemMultiMRPId = item.ItemMultiMRPIdTrans,
                            Qty = item.CurrentInventory,
                            UserId = userid,
                            ManualReason = "Stock transfer= " + item.ManualReason,
                        });

                        //MrpStockTransfer AddMrpStockTransfer = new MrpStockTransfer();
                        //AddMrpStockTransfer.WarehouseId = item.WarehouseId;
                        //AddMrpStockTransfer.FromMultiMRPId = item.ItemMultiMRPId;
                        //AddMrpStockTransfer.ToMultiMRPId = item.ItemMultiMRPIdTrans;
                        //AddMrpStockTransfer.Qty = item.CurrentInventory;
                        //AddMrpStockTransfer.CreatedDate = indianTime;
                        //db.MrpStockTransfers.Add(AddMrpStockTransfer);

                        if (StockList.Any())
                        {

                            bool res = MultiStockHelpers.MakeEntry(StockList, "Stock_OnMultiMRPTransfer", db, dbContextTransaction);
                            if (!res)
                            {
                                item = null;
                            }
                            else
                            {
                                db.Commit();
                                dbContextTransaction.Complete();

                                #region Insert in FIFO
                                if (ConfigurationManager.AppSettings["LiveFIFO"] == "1")
                                {
                                    List<OutDc> items = StockList.Where(x => x.Qty > 0).Select(x => new OutDc
                                    {
                                        ItemMultiMrpId = x.SourceItemMultiMRPId,
                                        WarehouseId = x.WarehouseId,
                                        Destination = "Stock Transfer Out",
                                        CreatedDate = indianTime,
                                        ObjectId = 0,
                                        Qty = x.Qty,
                                        SellingPrice = 0,
                                        InMrpId = x.DestinationItemMultiMRPId

                                    }).ToList();

                                    foreach (var it in items)
                                    {
                                        RabbitMqHelper rabbitMqHelper = new RabbitMqHelper();
                                        rabbitMqHelper.Publish("MrpOutTransfer", it);
                                    }
                                }
                                #endregion
                            }
                        }
                        #endregion

                    }
                }
            }
            return Request.CreateResponse(HttpStatusCode.OK, item);

        }


        [Authorize]
        [Route("StockTransferNew")]
        [HttpPut]
        public HttpResponseMessage StockTransferNew(List<TransferStockDTONew> transferStockList)
        {
            var identity = User.Identity as ClaimsIdentity;
            int compid = 0, userid = 0;
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))
                compid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);

            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

            if (userid > 0)
            {
                TransactionOptions option = new TransactionOptions();
                option.IsolationLevel = System.Transactions.IsolationLevel.RepeatableRead; //(System.Transactions.IsolationLevel)System.Data.IsolationLevel.RepeatableRead;
                option.Timeout = TimeSpan.FromSeconds(90);
                using (var dbContextTransaction = new TransactionScope(TransactionScopeOption.Required, option))
                {

                    using (AuthContext db = new AuthContext())
                    {
                        if (transferStockList != null && transferStockList.Any())
                        {
                            int wid = transferStockList.First().WarehouseId;
                            var warehouse = db.Warehouses.FirstOrDefault(x => x.WarehouseId == wid);
                            if (warehouse.IsStopCurrentStockTrans)
                                return Request.CreateResponse(HttpStatusCode.InternalServerError, "Inventory Transactions are currently disabled for this warehouse... Please try after some time");

                            foreach (var itemNew in transferStockList)
                            {
                                #region via virtual stock
                                MultiStockHelper<OnMultiMRPTransferDc> MultiStockHelpers = new MultiStockHelper<OnMultiMRPTransferDc>();
                                List<OnMultiMRPTransferDc> StockList = new List<OnMultiMRPTransferDc>();
                                StockList.Add(new OnMultiMRPTransferDc
                                {
                                    WarehouseId = itemNew.WarehouseId,
                                    SourceItemMultiMRPId = itemNew.ItemMultiMRPId,
                                    DestinationItemMultiMRPId = itemNew.ItemMultiMRPIdTrans,
                                    Qty = itemNew.Qty,
                                    UserId = userid,
                                    ManualReason = "Stock transfer= " + itemNew.ManualReason,
                                });

                                //MrpStockTransfer AddMrpStockTransfer = new MrpStockTransfer();
                                //AddMrpStockTransfer.WarehouseId = item.WarehouseId;
                                //AddMrpStockTransfer.FromMultiMRPId = item.ItemMultiMRPId;
                                //AddMrpStockTransfer.ToMultiMRPId = item.ItemMultiMRPIdTrans;
                                //AddMrpStockTransfer.Qty = item.CurrentInventory;
                                //AddMrpStockTransfer.CreatedDate = indianTime;
                                //db.MrpStockTransfers.Add(AddMrpStockTransfer);

                                if (StockList.Any())
                                {

                                    bool res = MultiStockHelpers.MakeEntry(StockList, "Stock_OnMultiMRPTransfer", db, dbContextTransaction);
                                    if (!res)
                                    {
                                        return Request.CreateResponse(HttpStatusCode.InternalServerError, "Error: Inventory issue");

                                    }
                                    else
                                    {



                                        //#region Insert in FIFO
                                        //if (ConfigurationManager.AppSettings["LiveFIFO"] == "1")
                                        //{
                                        //    List<OutDc> items = StockList.Where(x => x.Qty > 0).Select(x => new OutDc
                                        //    {
                                        //        ItemMultiMrpId = x.SourceItemMultiMRPId,
                                        //        WarehouseId = x.WarehouseId,
                                        //        Destination = "Stock Transfer Out",
                                        //        CreatedDate = indianTime,
                                        //        ObjectId = 0,
                                        //        Qty = x.Qty,
                                        //        SellingPrice = 0,
                                        //        InMrpId = x.DestinationItemMultiMRPId

                                        //    }).ToList();

                                        //    foreach (var it in items)
                                        //    {
                                        //        RabbitMqHelper rabbitMqHelper = new RabbitMqHelper();
                                        //        rabbitMqHelper.Publish("MrpOutTransfer", it);
                                        //    }
                                        //}
                                        //#endregion
                                    }
                                }
                                #endregion

                            }

                            BatchMasterManager batchMasterManager = new BatchMasterManager();
                            bool isSuccess = batchMasterManager.MoveStock(transferStockList, db, userid);
                            if (isSuccess)
                            {
                                db.Commit();
                                dbContextTransaction.Complete();
                            }
                            else
                            {
                                dbContextTransaction.Dispose();
                                return Request.CreateResponse(HttpStatusCode.InternalServerError, "Error: Batch code transfer issue");
                            }

                        }
                    }
                }
            }
            return Request.CreateResponse(HttpStatusCode.OK);

        }

        #endregion

        #region  Get active and deactive item value
        /// <summary>
        /// Get active and deactive item value
        /// </summary>
        /// <param name="wid"></param>
        /// <returns></returns>
        //[Route("GetItemValu")]
        //[HttpGet]
        //public ItemValue GetItemValu(int wid)
        //{
        //    try
        //    {
        //        TimeZoneInfo INDIAN_ZONE = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
        //        DateTime indianTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE);
        //        var identity = User.Identity as ClaimsIdentity;
        //        int compid = 0, userid = 0;
        //        int Warehouse_id = 0;

        //        if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))
        //            compid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);

        //        if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
        //            userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

        //        if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "Warehouseid"))
        //            Warehouse_id = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "Warehouseid").Value);
        //        int CompanyId = compid;

        //        using (AuthContext db = new AuthContext())
        //        {
        //            var totalInv = 0.0;
        //            var totalInvdact = 0.0;
        //            var totDay = 0;


        //            List<ItemMaster> SomOfDactItemValue = db.itemMasters.Where(a => a.WarehouseId == wid && a.active == false && a.CompanyId == compid).ToList();
        //            List<CurrentStock> currentstock = db.DbCurrentStock.Where(q => q.WarehouseId == wid && q.CurrentInventory > 0).ToList();
        //            foreach (var z in SomOfDactItemValue)
        //            {
        //                try
        //                {
        //                    var CS = currentstock.Where(w => w.ItemNumber == z.Number && w.ItemMultiMRPId == z.ItemMultiMRPId).FirstOrDefault();
        //                    totalInvdact += (CS.CurrentInventory * z.NetPurchasePrice);
        //                }
        //                catch (Exception ex)
        //                {
        //                    logger.Info(ex.Message);
        //                }
        //            }

        //            if (avgInv.Count != 0)
        //            {
        //                if (indianTime.Hour >= 20)
        //                {
        //                    totDay = Convert.ToInt32((indianTime.Date - avgInv[0].date.Date).TotalDays) + 1;
        //                }
        //                else
        //                {
        //                    totDay = Convert.ToInt32((indianTime.Date - avgInv[0].date.Date).TotalDays);
        //                }
        //                foreach (var ac in avgInv)
        //                {
        //                    totalInv += ac.totals;
        //                }
        //            }

        //            ItemValue IV = new ItemValue()
        //            {
        //                ActiveItemValue = (totalInv / totDay),
        //                DeactiveItemValue = totalInvdact
        //            };

        //            logger.Info("End  current stock: ");
        //            return IV;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        logger.Error("Error in current stock " + ex.Message);
        //        logger.Info("End  current stock: ");
        //        return null;
        //    }
        //}


        #endregion
        #region Warehouse based get data in current stock
        /// <summary>
        /// Created Date:19/07/2019
        /// Created by Raj
        /// </summary>
        /// <param name="WarehouseId"></param>
        /// <returns></returns>

        [Route("WidCurrentStock")]
        [HttpGet]
        public IEnumerable<CurrentStock> GetWarehousedata(string ItemNumber, int ItemMultiMRPId, int WarehouseId)
        {
            logger.Info("start current stock: ");

            List<CurrentStock> ass = new List<CurrentStock>();

            try
            {
                using (AuthContext db = new AuthContext())
                {

                    List<CurrentStock> stockdata = db.DbCurrentStock.Where(x => x.WarehouseId == WarehouseId && x.Deleted == false && x.ItemNumber == ItemNumber && x.ItemMultiMRPId != ItemMultiMRPId).ToList();
                    logger.Info("End  current stock: ");
                    return stockdata;
                }

            }
            catch (Exception ex)
            {
                logger.Error("Error in current stock " + ex.Message);
                logger.Info("End  current stock: ");
                return null;
            }
        }

        #endregion
        // Trigger event when manual inventory update 
        #region SendMailManualInventory
        //SendMailManualInventory
        public static void SendMailManualInventory(int CurrentInventory, int ManualUpdateInventory, string UserName, CurrentStock cst)
        {
            try
            {
                string masteremail = ConfigurationManager.AppSettings["MasterEmail"];
                string masterpassword = ConfigurationManager.AppSettings["MasterPassword"];
                string body = "<div style='background: #FAFAFA; color: #333333; padding-left: 30px;font-family: arial,sans-serif; font-size: 14px;'>";
                //body += "<img style='padding-top: 10px;' src='http://shopkirana.com/wp-content/uploads/2015/07/ShopKirana-Logo11.png'><br/>";
                body += "<h3 style='background-color: rgb(241, 89, 34);'>Alert! " + cst.itemname + " Manual Inventory Updated due to: " + cst.ManualReason + "</h3> ";
                body += "Hello,";
                body += "<p>Reason:" + cst.ManualReason + " </p>";
                body += "<p>Warehouse Name:" + cst.WarehouseName + "(" + cst.WarehouseId + ")</p>";
                body += "<p>City Name:" + cst.CityName + "(" + cst.CityId + ") </p>";
                body += "<p><strong>";
                body += CurrentInventory + "</strong>" + " : Inventory Before Updated  and Now Inventory Updated by this QTY : " + ManualUpdateInventory + "</p> <h1> So Current Inventory Is :" + (CurrentInventory + ManualUpdateInventory) + "</h1>";
                body += "<p>Item Number:" + cst.ItemNumber + " </p>";
                body += "<p>Updated By user Name : <strong>" + UserName + "</strong> Date <strong>" + cst.UpdatedDate + "</strong></p>";
                body += "Thanks,";
                body += "<br />";
                body += "<b>IT Team</b>";
                body += "</div>";
                var Subj = "Alert! " + cst.itemname + "  Manual Inventory Updated due to  " + cst.ManualReason;
                var msg = new MailMessage("donotreply_backend@shopkirana.com", "donotreply_backend@shopkirana.com", Subj, body);
                // msg.To.Add("deepak@shopkirana.com");
                msg.To.Add("bhavik.parikh@shopkirana.com");
                msg.IsBodyHtml = true;
                var smtpClient = new SmtpClient("smtp.gmail.com", 587);
                smtpClient.UseDefaultCredentials = true;
                smtpClient.Credentials = new NetworkCredential(masteremail, masterpassword);
                smtpClient.EnableSsl = true;
                smtpClient.Send(msg);

            }
            catch (Exception ss) { }


        }
        #endregion

        ///// <summary>
        ///// Get Temporary current stock
        ///// Created by Yogendra
        ///// Created on 17/07/2019
        ///// </summary>
        ///// <param name="WarehouseId"></param>
        ///// <returns></returns>
        //[Route("TempGetWarehousebased")]
        //[HttpGet]
        //public IEnumerable<TemporaryCurrentStock> TempGetWarehousebased(int WarehouseId)
        //{
        //    logger.Info("start current stock: ");
        //    using (AuthContext db = new AuthContext())
        //    {
        //        List<TemporaryCurrentStock> ass = new List<TemporaryCurrentStock>();
        //        try
        //        {
        //            var identity = User.Identity as ClaimsIdentity;
        //            int compid = 0, userid = 0;
        //            int Warehouse_id = 0;


        //            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))
        //                compid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);

        //            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
        //                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

        //            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "Warehouseid"))
        //                Warehouse_id = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "Warehouseid").Value);

        //            int CompanyId = compid;
        //            Warehouse_id = WarehouseId;
        //            logger.Info("User ID : {0} , Company Id : {1}", compid, userid);

        //            var predicate = PredicateBuilder.True<TemporaryCurrentStock>();
        //            predicate = predicate.And(x => x.CompanyId == compid);
        //            predicate = predicate.And(x => x.WarehouseId == Warehouse_id);
        //            ass = db.TemporaryCurrentStockDB.Where(predicate).ToList();
        //            logger.Info("End  current stock: ");
        //            return ass;
        //        }
        //        catch (Exception ex)
        //        {
        //            logger.Error("Error in current stock " + ex.Message);
        //            logger.Info("End  current stock: ");
        //            return null;
        //        }
        //    }
        //}
        ///// <summary>
        ///// Get temporary item current stock history
        ///// created by Yogendra
        ///// created on 17/07/2019
        ///// </summary>
        ///// <param name="list"></param>
        ///// <param name="page"></param>
        ///// <param name="ItemNumber"></param>
        ///// <param name="WarehouseId"></param>
        ///// <param name="StockId"></param>
        ///// <returns></returns>
        //[Route("TempStockHistory")]
        //[HttpGet]
        //public TempcurrentHistorycollectionPaggingData TempStockHistory(int list, int page, string ItemNumber, int WarehouseId, int StockId)
        //{
        //    logger.Info("start TempStockHistory: ");
        //    using (AuthContext db = new AuthContext())
        //    {
        //        try
        //        {
        //            TempcurrentHistorycollectionPaggingData tempcurrentHistorycollectionPaggingData = new TempcurrentHistorycollectionPaggingData();
        //            var identity = User.Identity as ClaimsIdentity;
        //            int compid = 0, userid = 0, Warehouse_id = 0;

        //            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))
        //                compid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);

        //            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
        //                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

        //            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "Warehouseid"))
        //                Warehouse_id = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "Warehouseid").Value);
        //            int CompanyId = compid;

        //            if (CompanyId == 0)
        //            {
        //                throw new ArgumentNullException("item");
        //            }

        //            logger.Info("User ID : {0} , Company Id : {1}", compid, userid);

        //            var predicate = PredicateBuilder.True<TemporaryCurrentStockHistory>();
        //            predicate = predicate.And(x => x.CompanyId == compid);
        //            predicate = predicate.And(x => x.ItemNumber == ItemNumber);
        //            predicate = predicate.And(x => x.Warehouseid == WarehouseId);
        //            predicate = predicate.And(x => x.StockId == StockId);
        //            predicate = predicate.And(x => x.Deleted == false);

        //            var listOrders = db.TemporaryCurrentStockHistoryDB.Where(predicate).ToList();
        //            tempcurrentHistorycollectionPaggingData.total_count = listOrders.Count();
        //            tempcurrentHistorycollectionPaggingData.TemporaryCurrentStockHistorydata = listOrders.OrderByDescending(x => x.CreationDate).Skip((page - 1) * list).Take(list).ToList();

        //            return tempcurrentHistorycollectionPaggingData;
        //        }
        //        catch (Exception ex)
        //        {
        //            logger.Error("Error in TempStockHistory " + ex.Message);
        //            logger.Info("End  TempStockHistory: ");
        //            return null;
        //        }
        //    }
        //}


        #region OldCOde
        //[Route("GetWarehousebasedV7")]
        //[HttpPost]
        //public IHttpActionResult GetWarehousebasedV7(int WarehouseId, PagerDataUIViewModel pager)
        //{
        //    CurrentStockPagerListDTO t = new CurrentStockPagerListDTO();
        //    using (AuthContext context = new AuthContext())
        //    {
        //        pager.WarehouseId = WarehouseId;

        //        if (!string.IsNullOrEmpty(pager.Contains) && pager.Contains.Length > 4)
        //        {
        //            var propertyInfo = typeof(CurrentStock).GetProperty(pager.ColumnName);
        //            List<CurrentStock> data;
        //            if (pager.IsAscending == true)
        //            {
        //                data = context.DbCurrentStock.Where(x => x.WarehouseId == WarehouseId && (pager.Contains == null || (x.itemname.Contains(pager.Contains)) || (x.ItemNumber.Contains(pager.Contains)))).AsEnumerable().OrderBy(x => propertyInfo.GetValue(x, null)).ToList();//ass.Count();
        //            }
        //            else
        //            {
        //                data = context.DbCurrentStock.Where(x => x.WarehouseId == WarehouseId && (pager.Contains == null || (x.itemname.Contains(pager.Contains)) || (x.ItemNumber.Contains(pager.Contains)))).AsEnumerable().OrderByDescending(x => propertyInfo.GetValue(x, null)).ToList();//ass.Count();
        //            }
        //            t.TotalRecords = data.Count();
        //            t.CurrentStockPagerList = data.Skip(pager.First).Take(pager.Last - pager.First).ToList();
        //        }
        //        else
        //        {
        //            var propertyInfo = typeof(CurrentStock).GetProperty(pager.ColumnName);
        //            List<CurrentStock> data;
        //            if (pager.IsAscending == true)
        //            {
        //                data = context.DbCurrentStock.Where(x => x.WarehouseId == WarehouseId && (pager.Contains == null || (x.itemname.Contains(pager.Contains)) || (x.ItemNumber.Contains(pager.Contains)))).AsEnumerable().OrderBy(x => propertyInfo.GetValue(x, null)).ToList();//ass.Count();
        //            }
        //            else
        //            {
        //                data = context.DbCurrentStock.Where(x => x.WarehouseId == WarehouseId && (pager.Contains == null || (x.itemname.Contains(pager.Contains)) || (x.ItemNumber.Contains(pager.Contains)))).AsEnumerable().OrderByDescending(x => propertyInfo.GetValue(x, null)).ToList();//ass.Count();
        //            }
        //            t.TotalRecords = data.Count();
        //            t.CurrentStockPagerList = data.Skip(pager.First).Take(pager.Last - pager.First).ToList();
        //        }
        //        return Ok(t);

        //    }
        //}
        #endregion
        #region get safetystockhistory
        /// <summary>
        /// Get Safety Stock History
        /// By pooja k
        /// </summary>
        /// <param name="poapprovalid"></param>
        /// <returns></returns>
        [Route("safetystockhistory")]
        [HttpGet]
        public dynamic safetystockhistory(int StockId)
        {
            using (var odd = new AuthContext())
            {
                try
                {
                    var data = odd.SafetyStockHistoryDB.Where(x => x.StockId == StockId).ToList();
                    return data;
                }
                catch (Exception ex)
                {
                    return false;
                }
            }
        }
        #endregion

        #region transfer current stock to freestock
        /// <summary>
        /// Created Date:07/10/2019
        /// Created By Raj
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        [Authorize]
        [Route("TransferToFreeStock")]
        [HttpPut]
        public HttpResponseMessage TransferToFreeStock(TransferToFreestockDTO item)
        {
            People People = null;
            var result = "";
            var identity = User.Identity as ClaimsIdentity;
            int compid = 0, userid = 0;
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))
                compid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);

            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
            if (userid > 0)
            {
                TransactionOptions option = new TransactionOptions();
                option.IsolationLevel = System.Transactions.IsolationLevel.RepeatableRead; //(System.Transactions.IsolationLevel)System.Data.IsolationLevel.RepeatableRead;
                option.Timeout = TimeSpan.FromSeconds(90);
                using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, option))
                {
                    using (AuthContext db = new AuthContext())
                    {
                        var warehouse = db.Warehouses.FirstOrDefault(x => x.WarehouseId == item.WarehouseId);

                        if (warehouse.IsStopCurrentStockTrans)
                            return Request.CreateResponse(HttpStatusCode.OK, "Inventory Transactions are currently disabled for this warehouse... Please try after some time");


                        People = db.Peoples.Where(x => x.PeopleID == userid && x.Active == true).SingleOrDefault();
                        if (People != null)
                        {
                            CurrentStock CurrentStock = db.DbCurrentStock.Where(c => c.ItemNumber == item.ItemNumber && c.Deleted == false && c.WarehouseId == item.WarehouseId && c.ItemMultiMRPId == item.ItemMultiMRPId).FirstOrDefault();
                            if (CurrentStock != null && CurrentStock.CurrentInventory >= item.Transferinventory)
                            {
                                FreeStock FreeStock = db.FreeStockDB.Where(x => x.WarehouseId == CurrentStock.WarehouseId && x.ItemMultiMRPId == CurrentStock.ItemMultiMRPId).FirstOrDefault();
                                if (FreeStock == null)
                                {
                                    ItemMultiMRP ItemMultiMRP = db.ItemMultiMRPDB.Where(a => a.ItemMultiMRPId == CurrentStock.ItemMultiMRPId).SingleOrDefault();
                                    if (ItemMultiMRP != null)
                                    {
                                        FreeStock FreeStockobj = new FreeStock();
                                        FreeStockobj.ItemNumber = ItemMultiMRP.ItemNumber;
                                        FreeStockobj.itemname = CurrentStock.itemname;
                                        FreeStockobj.ItemMultiMRPId = ItemMultiMRP.ItemMultiMRPId;
                                        FreeStockobj.MRP = ItemMultiMRP.MRP;
                                        FreeStockobj.WarehouseId = Convert.ToInt32(CurrentStock.WarehouseId);
                                        FreeStockobj.CurrentInventory = 0;
                                        FreeStockobj.CreatedBy = People.DisplayName;
                                        FreeStockobj.CreationDate = indianTime;
                                        FreeStockobj.Deleted = false;
                                        FreeStockobj.UpdatedDate = indianTime;
                                        db.FreeStockDB.Add(FreeStockobj);
                                        db.Commit();
                                    }
                                }

                                List<PhysicalStockUpdateRequestDc> StockTransferToFreeList = new List<PhysicalStockUpdateRequestDc>();

                                StockTransactionHelper helper = new StockTransactionHelper();
                                PhysicalStockUpdateRequestDc StockTransferToFree = new PhysicalStockUpdateRequestDc();
                                StockTransferToFree.ItemMultiMRPId = CurrentStock.ItemMultiMRPId;
                                StockTransferToFree.WarehouseId = CurrentStock.WarehouseId;
                                StockTransferToFree.Qty = item.Transferinventory;
                                StockTransferToFree.SourceStockType = StockTypeTableNames.CurrentStocks;
                                StockTransferToFree.DestinationStockType = StockTypeTableNames.FreebieStock;
                                StockTransferToFree.StockTransferType = StockTransferTypeName.ManualInventory;
                                StockTransferToFree.Reason = "Stock transfer to Freestock = " + item.ManualReason;

                                StockTransferToFreeList.AddRange(StockTransferToFreeList);
                                bool isupdated = helper.TransferBetweenPhysicalStocks(StockTransferToFree, userid, db, scope);
                                BatchMasterManager batchMasterManager = new BatchMasterManager();
                                bool isSuccess = batchMasterManager.MoveDirectBatchItemInSameBatch("C", "F", item.Transferinventory, item.StockBatchMasterId, item.ItemMultiMRPId, item.WarehouseId, db, userid);
                                if (isupdated && isSuccess)
                                {
                                    scope.Complete();
                                    result = "Transaction Saved Successfully";
                                    #region Insert in FIFO

                                    if (ConfigurationManager.AppSettings["LiveFIFO"] == "1")
                                    {
                                        List<OutDc> items = StockTransferToFreeList.Where(x => x.Qty > 0).Select(x => new OutDc
                                        {
                                            ItemMultiMrpId = x.ItemMultiMRPId,
                                            WarehouseId = x.WarehouseId,
                                            Destination = "FreeOutFromCS",
                                            CreatedDate = indianTime,
                                            ObjectId = 0,
                                            Qty = x.Qty,
                                            SellingPrice = 0,
                                            InMrpId = x.ItemMultiMRPId
                                        }).ToList();
                                        foreach (var it in items)
                                        {
                                            RabbitMqHelper rabbitMqHelper = new RabbitMqHelper();
                                            rabbitMqHelper.Publish("ToFreeStock", it);
                                        }
                                    }
                                    #endregion
                                }
                                else
                                {
                                    scope.Dispose();
                                    if (!isupdated)
                                        result = "one of the stock not available";
                                    if (!isSuccess)
                                        result = "currently stock movement in process for same ItemMultiMRP and Warehouse. Please try after some time.";
                                }
                                result = item.Transferinventory + " Qty Transfered to FreeStock Successfully";
                            }
                            else
                            {
                                result = "CurretStock Qty not availble to transfer in FreeStock of Qty: " + item.Transferinventory;
                            }
                        }
                    }
                }
            }
            return Request.CreateResponse(HttpStatusCode.OK, result);

            #region old code
            //using (AuthContext db = new AuthContext())
            //{
            //    var User = db.Peoples.Where(x => x.PeopleID == userid && x.Active == true).SingleOrDefault();
            //    if (User != null)
            //    {
            //        CurrentStock CurrentStock = db.DbCurrentStock.Where(c => c.ItemNumber == item.ItemNumber && c.Deleted == false && c.WarehouseId == item.WarehouseId && c.ItemMultiMRPId == item.ItemMultiMRPId).FirstOrDefault();

            //        if (CurrentStock != null && CurrentStock.CurrentInventory >= item.Transferinventory)
            //        {
            //            Warehouse Warehouse = db.Warehouses.Where(z => z.WarehouseId == item.WarehouseId).FirstOrDefault();
            //            CurrentStockHistory CurrentStockHistory = new CurrentStockHistory();
            //            CurrentStockHistory.updationDate = indianTime;
            //            CurrentStockHistory.CreationDate = indianTime;
            //            CurrentStockHistory.StockId = CurrentStock.StockId;
            //            CurrentStockHistory.ItemMultiMRPId = CurrentStock.ItemMultiMRPId;
            //            CurrentStockHistory.itemname = CurrentStock.itemname;
            //            CurrentStockHistory.ItemNumber = CurrentStock.ItemNumber;
            //            CurrentStockHistory.CurrentInventory = CurrentStock.CurrentInventory - item.Transferinventory;
            //            CurrentStockHistory.UserName = User.DisplayName;
            //            CurrentStockHistory.userid = userid;
            //            CurrentStockHistory.CompanyId = compid;
            //            CurrentStockHistory.TotalInventory = CurrentStock.CurrentInventory - item.Transferinventory;
            //            //CurrentStockHistory.InventoryOut = item.Transferinventory;
            //            CurrentStockHistory.ManualInventoryIn = item.Transferinventory * (-1);
            //            CurrentStockHistory.ManualReason = " Stock transfer to FreeStock Due to: " + item.ManualReason;
            //            CurrentStockHistory.Warehouseid = Warehouse.WarehouseId;
            //            CurrentStockHistory.WarehouseName = Warehouse.WarehouseName;
            //            db.CurrentStockHistoryDb.Add(CurrentStockHistory);

            //            FreeStock FreeStock = db.FreeStockDB.Where(x => x.ItemNumber == CurrentStockHistory.ItemNumber && x.WarehouseId == CurrentStockHistory.Warehouseid && x.ItemMultiMRPId == CurrentStockHistory.ItemMultiMRPId).FirstOrDefault();
            //            if (FreeStock != null)
            //            {

            //                FreeStock.CurrentInventory = FreeStock.CurrentInventory + item.Transferinventory;
            //                if (FreeStock.CurrentInventory < 0)
            //                {
            //                    FreeStock.CurrentInventory = 0;
            //                }
            //                db.Entry(FreeStock).State = EntityState.Modified;

            //                FreeStockHistory FreeStockHistory = new FreeStockHistory();
            //                FreeStockHistory.ManualReason = "Stock In from CurrentStock Due to:" + item.ManualReason;
            //                FreeStockHistory.FreeStockId = FreeStock.FreeStockId;
            //                FreeStockHistory.ItemMultiMRPId = FreeStock.ItemMultiMRPId;
            //                FreeStockHistory.ItemNumber = FreeStock.ItemNumber;
            //                FreeStockHistory.itemname = FreeStock.itemname;
            //                FreeStockHistory.CurrentInventory = FreeStock.CurrentInventory;
            //                //FreeStockHistory.InventoryIn = item.Transferinventory;
            //                FreeStockHistory.ManualInventoryIn = item.Transferinventory;
            //                FreeStockHistory.TotalInventory = Convert.ToInt32(FreeStock.CurrentInventory);
            //                FreeStockHistory.WarehouseId = FreeStock.WarehouseId;
            //                FreeStockHistory.CreationDate = indianTime;
            //                FreeStockHistory.userid = userid;
            //                db.FreeStockHistoryDB.Add(FreeStockHistory);
            //                db.Commit();
            //            }
            //            else
            //            {
            //                ItemMultiMRP ItemMultiMRP = db.ItemMultiMRPDB.Where(a => a.ItemMultiMRPId == CurrentStockHistory.ItemMultiMRPId).SingleOrDefault();
            //                if (ItemMultiMRP != null)
            //                {
            //                    FreeStock FreeStockobj = new FreeStock();
            //                    FreeStockobj.ItemNumber = ItemMultiMRP.ItemNumber;
            //                    FreeStockobj.itemname = CurrentStockHistory.itemname;
            //                    FreeStockobj.ItemMultiMRPId = ItemMultiMRP.ItemMultiMRPId;
            //                    FreeStockobj.MRP = ItemMultiMRP.MRP;
            //                    FreeStockobj.WarehouseId = Convert.ToInt32(CurrentStock.WarehouseId);
            //                    FreeStockobj.CurrentInventory = item.Transferinventory;
            //                    FreeStockobj.CreatedBy = User.DisplayName;
            //                    FreeStockobj.CreationDate = indianTime;
            //                    FreeStockobj.Deleted = false;
            //                    FreeStockobj.UpdatedDate = indianTime;
            //                    db.FreeStockDB.Add(FreeStockobj);
            //                    db.Commit();

            //                    FreeStockHistory FreeStockHistory = new FreeStockHistory();
            //                    FreeStockHistory.ManualReason = "Stock In from CurrentStock Due to:" + item.ManualReason;
            //                    FreeStockHistory.FreeStockId = FreeStockobj.FreeStockId;
            //                    FreeStockHistory.ItemMultiMRPId = FreeStockobj.ItemMultiMRPId;
            //                    FreeStockHistory.ItemNumber = FreeStockobj.ItemNumber;
            //                    FreeStockHistory.itemname = FreeStockobj.itemname;
            //                    FreeStockHistory.CurrentInventory = FreeStockobj.CurrentInventory;
            //                    // FreeStockHistory.InventoryIn = FreeStockobj.CurrentInventory;
            //                    FreeStockHistory.ManualInventoryIn = FreeStockobj.CurrentInventory;
            //                    FreeStockHistory.TotalInventory = Convert.ToInt32(FreeStockobj.CurrentInventory);
            //                    FreeStockHistory.WarehouseId = FreeStockobj.WarehouseId;
            //                    FreeStockHistory.CreationDate = indianTime;
            //                    FreeStockHistory.userid = userid;
            //                    FreeStockHistory.CreationDate = indianTime;
            //                    db.FreeStockHistoryDB.Add(FreeStockHistory);

            //                }

            //            }

            //            int CurrentInventory = CurrentStock.CurrentInventory;//current inventory
            //            int ManualUpdateInventory = CurrentStock.CurrentInventory - item.Transferinventory;// New Added Inventory

            //            CurrentStock.ManualReason = "Stock transfer to Freestock= " + item.ManualReason;
            //            CurrentStock.UpdatedDate = indianTime;
            //            CurrentStock.CurrentInventory -= item.Transferinventory;
            //            db.Entry(CurrentStock).State = EntityState.Modified;
            //            if (db.Commit() > 0)
            //            {
            //                result = item.Transferinventory + " Qty Transfered to FreeStock Successfully";
            //                try
            //                {
            //                    CurrentStock.CityName = Warehouse.CityName;
            //                    CurrentStock.CityId = Warehouse.Cityid;
            //                    SendMailManualInventory(CurrentInventory, ManualUpdateInventory, User.DisplayName, CurrentStock);
            //                }
            //                catch (Exception ex)
            //                {
            //                    logger.Error("Error in send mail stock transfer " + ex.Message);
            //                    logger.Info("End  send mail stock transfer: ");
            //                    return null;
            //                }
            //                return Request.CreateResponse(HttpStatusCode.OK, result);
            //            };
            //        }
            //        else
            //        {
            //            result = "CurretStock Qty not availble to transfer in FreeStock of Qty: " + item.Transferinventory;
            //            return Request.CreateResponse(HttpStatusCode.OK, result);
            //        }
            //    }
            //}
            //result = "Something went wrong";
            //return Request.CreateResponse(HttpStatusCode.OK, result);
            #endregion
        }


        [Authorize]
        [Route("TransferToFreeStockV1")]
        [HttpPut]
        public HttpResponseMessage TransferToFreeStockV1(List<TransferToFreeStockDC> list)
        {
            People People = null;
            var result = "";

            var identity = User.Identity as ClaimsIdentity;
            int compid = 0, userid = 0;
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))
                compid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);

            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
            if (userid > 0)
            {
                TransactionOptions option = new TransactionOptions();
                option.IsolationLevel = System.Transactions.IsolationLevel.RepeatableRead; //(System.Transactions.IsolationLevel)System.Data.IsolationLevel.RepeatableRead;
                option.Timeout = TimeSpan.FromSeconds(90);
                using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, option))
                {
                    using (AuthContext db = new AuthContext())
                    {
                        int wid = list.First().WarehouseId;
                        var warehouse = db.Warehouses.FirstOrDefault(x => x.WarehouseId == wid);

                        if (warehouse.IsStopCurrentStockTrans)
                            return Request.CreateResponse(HttpStatusCode.OK, "Inventory Transactions are currently disabled for this warehouse... Please try after some time");


                        People = db.Peoples.Where(x => x.PeopleID == userid && x.Active == true).SingleOrDefault();
                        if (People != null)
                        {
                            foreach (var itemNew in list)
                            {

                                var query = from s in db.StockBatchMasters
                                            join c in db.DbCurrentStock
                                            on s.StockId equals c.StockId
                                            where (s.StockType == "C" && s.Id == itemNew.StockBatchMasterId)
                                            select c;
                                CurrentStock CurrentStock = query.FirstOrDefault();


                                if (CurrentStock != null && CurrentStock.CurrentInventory >= itemNew.Qty)
                                {
                                    FreeStock FreeStock = db.FreeStockDB.Where(x => x.WarehouseId == CurrentStock.WarehouseId && x.ItemMultiMRPId == CurrentStock.ItemMultiMRPId).FirstOrDefault();
                                    if (FreeStock == null)
                                    {
                                        ItemMultiMRP ItemMultiMRP = db.ItemMultiMRPDB.Where(a => a.ItemMultiMRPId == CurrentStock.ItemMultiMRPId).SingleOrDefault();
                                        if (ItemMultiMRP != null)
                                        {
                                            FreeStock FreeStockobj = new FreeStock();
                                            FreeStockobj.ItemNumber = ItemMultiMRP.ItemNumber;
                                            FreeStockobj.itemname = CurrentStock.itemname;
                                            FreeStockobj.ItemMultiMRPId = ItemMultiMRP.ItemMultiMRPId;
                                            FreeStockobj.MRP = ItemMultiMRP.MRP;
                                            FreeStockobj.WarehouseId = Convert.ToInt32(CurrentStock.WarehouseId);
                                            FreeStockobj.CurrentInventory = 0;
                                            FreeStockobj.CreatedBy = People.DisplayName;
                                            FreeStockobj.CreationDate = indianTime;
                                            FreeStockobj.Deleted = false;
                                            FreeStockobj.UpdatedDate = indianTime;
                                            db.FreeStockDB.Add(FreeStockobj);
                                            db.Commit();
                                        }
                                    }

                                    List<PhysicalStockUpdateRequestDc> StockTransferToFreeList = new List<PhysicalStockUpdateRequestDc>();

                                    StockTransactionHelper helper = new StockTransactionHelper();
                                    PhysicalStockUpdateRequestDc StockTransferToFree = new PhysicalStockUpdateRequestDc();
                                    StockTransferToFree.ItemMultiMRPId = CurrentStock.ItemMultiMRPId;
                                    StockTransferToFree.WarehouseId = CurrentStock.WarehouseId;
                                    StockTransferToFree.Qty = itemNew.Qty;
                                    StockTransferToFree.SourceStockType = StockTypeTableNames.CurrentStocks;
                                    StockTransferToFree.DestinationStockType = StockTypeTableNames.FreebieStock;
                                    StockTransferToFree.StockTransferType = StockTransferTypeName.ManualInventory;
                                    StockTransferToFree.Reason = "Stock transfer to Freestock = " + itemNew.ManualReason;

                                    StockTransferToFreeList.AddRange(StockTransferToFreeList);

                                    bool isupdated = helper.TransferBetweenPhysicalStocks(StockTransferToFree, userid, db, scope);
                                    BatchMasterManager batchMasterManager = new BatchMasterManager();
                                    bool isSuccess = batchMasterManager.MoveDirectBatchItemInSameBatch("C", "F", itemNew.Qty, itemNew.StockBatchMasterId, itemNew.ItemMultiMRPId, itemNew.WarehouseId, db, userid);
                                    if (isupdated && isSuccess)
                                    {
                                        db.Commit();
                                        //#region Insert in FIFO

                                        //if (ConfigurationManager.AppSettings["LiveFIFO"] == "1")
                                        //{
                                        //    List<OutDc> items = StockTransferToFreeList.Where(x => x.Qty > 0).Select(x => new OutDc
                                        //    {
                                        //        ItemMultiMrpId = x.ItemMultiMRPId,
                                        //        WarehouseId = x.WarehouseId,
                                        //        Destination = "FreeOutFromCS",
                                        //        CreatedDate = indianTime,
                                        //        ObjectId = 0,
                                        //        Qty = x.Qty,
                                        //        SellingPrice = 0,
                                        //        InMrpId = x.ItemMultiMRPId
                                        //    }).ToList();
                                        //    foreach (var it in items)
                                        //    {
                                        //        RabbitMqHelper rabbitMqHelper = new RabbitMqHelper();
                                        //        rabbitMqHelper.Publish("ToFreeStock", it);
                                        //    }
                                        //}
                                        //#endregion
                                    }
                                    else
                                    {
                                        scope.Dispose();
                                        if (!isupdated)
                                            result = "one of the stock not available";
                                        if (!isSuccess)
                                            result = "currently stock movement in process for same ItemMultiMRP and Warehouse. Please try after some time.";

                                        Request.CreateResponse(HttpStatusCode.OK, result);
                                    }
                                    result = itemNew.Qty + " Qty Transfered to FreeStock Successfully";
                                }
                                else
                                {
                                    scope.Dispose();
                                    result = "CurretStock Qty not availble to transfer in FreeStock of Qty: " + itemNew.Qty;
                                    Request.CreateResponse(HttpStatusCode.OK, result);
                                }
                            }
                            //db.Commit();
                            try
                            {

                                scope.Complete();
                                result = "Transaction Saved Successfully";

                            }
                            catch (Exception ex)
                            {
                                //Console.WriteLine(ex);
                                result = "Something went wrong!";
                            }
                        }
                    }
                }
            }
            return Request.CreateResponse(HttpStatusCode.OK, result);

            #region old code
            //using (AuthContext db = new AuthContext())
            //{
            //    var User = db.Peoples.Where(x => x.PeopleID == userid && x.Active == true).SingleOrDefault();
            //    if (User != null)
            //    {
            //        CurrentStock CurrentStock = db.DbCurrentStock.Where(c => c.ItemNumber == item.ItemNumber && c.Deleted == false && c.WarehouseId == item.WarehouseId && c.ItemMultiMRPId == item.ItemMultiMRPId).FirstOrDefault();

            //        if (CurrentStock != null && CurrentStock.CurrentInventory >= item.Transferinventory)
            //        {
            //            Warehouse Warehouse = db.Warehouses.Where(z => z.WarehouseId == item.WarehouseId).FirstOrDefault();
            //            CurrentStockHistory CurrentStockHistory = new CurrentStockHistory();
            //            CurrentStockHistory.updationDate = indianTime;
            //            CurrentStockHistory.CreationDate = indianTime;
            //            CurrentStockHistory.StockId = CurrentStock.StockId;
            //            CurrentStockHistory.ItemMultiMRPId = CurrentStock.ItemMultiMRPId;
            //            CurrentStockHistory.itemname = CurrentStock.itemname;
            //            CurrentStockHistory.ItemNumber = CurrentStock.ItemNumber;
            //            CurrentStockHistory.CurrentInventory = CurrentStock.CurrentInventory - item.Transferinventory;
            //            CurrentStockHistory.UserName = User.DisplayName;
            //            CurrentStockHistory.userid = userid;
            //            CurrentStockHistory.CompanyId = compid;
            //            CurrentStockHistory.TotalInventory = CurrentStock.CurrentInventory - item.Transferinventory;
            //            //CurrentStockHistory.InventoryOut = item.Transferinventory;
            //            CurrentStockHistory.ManualInventoryIn = item.Transferinventory * (-1);
            //            CurrentStockHistory.ManualReason = " Stock transfer to FreeStock Due to: " + item.ManualReason;
            //            CurrentStockHistory.Warehouseid = Warehouse.WarehouseId;
            //            CurrentStockHistory.WarehouseName = Warehouse.WarehouseName;
            //            db.CurrentStockHistoryDb.Add(CurrentStockHistory);

            //            FreeStock FreeStock = db.FreeStockDB.Where(x => x.ItemNumber == CurrentStockHistory.ItemNumber && x.WarehouseId == CurrentStockHistory.Warehouseid && x.ItemMultiMRPId == CurrentStockHistory.ItemMultiMRPId).FirstOrDefault();
            //            if (FreeStock != null)
            //            {

            //                FreeStock.CurrentInventory = FreeStock.CurrentInventory + item.Transferinventory;
            //                if (FreeStock.CurrentInventory < 0)
            //                {
            //                    FreeStock.CurrentInventory = 0;
            //                }
            //                db.Entry(FreeStock).State = EntityState.Modified;

            //                FreeStockHistory FreeStockHistory = new FreeStockHistory();
            //                FreeStockHistory.ManualReason = "Stock In from CurrentStock Due to:" + item.ManualReason;
            //                FreeStockHistory.FreeStockId = FreeStock.FreeStockId;
            //                FreeStockHistory.ItemMultiMRPId = FreeStock.ItemMultiMRPId;
            //                FreeStockHistory.ItemNumber = FreeStock.ItemNumber;
            //                FreeStockHistory.itemname = FreeStock.itemname;
            //                FreeStockHistory.CurrentInventory = FreeStock.CurrentInventory;
            //                //FreeStockHistory.InventoryIn = item.Transferinventory;
            //                FreeStockHistory.ManualInventoryIn = item.Transferinventory;
            //                FreeStockHistory.TotalInventory = Convert.ToInt32(FreeStock.CurrentInventory);
            //                FreeStockHistory.WarehouseId = FreeStock.WarehouseId;
            //                FreeStockHistory.CreationDate = indianTime;
            //                FreeStockHistory.userid = userid;
            //                db.FreeStockHistoryDB.Add(FreeStockHistory);
            //                db.Commit();
            //            }
            //            else
            //            {
            //                ItemMultiMRP ItemMultiMRP = db.ItemMultiMRPDB.Where(a => a.ItemMultiMRPId == CurrentStockHistory.ItemMultiMRPId).SingleOrDefault();
            //                if (ItemMultiMRP != null)
            //                {
            //                    FreeStock FreeStockobj = new FreeStock();
            //                    FreeStockobj.ItemNumber = ItemMultiMRP.ItemNumber;
            //                    FreeStockobj.itemname = CurrentStockHistory.itemname;
            //                    FreeStockobj.ItemMultiMRPId = ItemMultiMRP.ItemMultiMRPId;
            //                    FreeStockobj.MRP = ItemMultiMRP.MRP;
            //                    FreeStockobj.WarehouseId = Convert.ToInt32(CurrentStock.WarehouseId);
            //                    FreeStockobj.CurrentInventory = item.Transferinventory;
            //                    FreeStockobj.CreatedBy = User.DisplayName;
            //                    FreeStockobj.CreationDate = indianTime;
            //                    FreeStockobj.Deleted = false;
            //                    FreeStockobj.UpdatedDate = indianTime;
            //                    db.FreeStockDB.Add(FreeStockobj);
            //                    db.Commit();

            //                    FreeStockHistory FreeStockHistory = new FreeStockHistory();
            //                    FreeStockHistory.ManualReason = "Stock In from CurrentStock Due to:" + item.ManualReason;
            //                    FreeStockHistory.FreeStockId = FreeStockobj.FreeStockId;
            //                    FreeStockHistory.ItemMultiMRPId = FreeStockobj.ItemMultiMRPId;
            //                    FreeStockHistory.ItemNumber = FreeStockobj.ItemNumber;
            //                    FreeStockHistory.itemname = FreeStockobj.itemname;
            //                    FreeStockHistory.CurrentInventory = FreeStockobj.CurrentInventory;
            //                    // FreeStockHistory.InventoryIn = FreeStockobj.CurrentInventory;
            //                    FreeStockHistory.ManualInventoryIn = FreeStockobj.CurrentInventory;
            //                    FreeStockHistory.TotalInventory = Convert.ToInt32(FreeStockobj.CurrentInventory);
            //                    FreeStockHistory.WarehouseId = FreeStockobj.WarehouseId;
            //                    FreeStockHistory.CreationDate = indianTime;
            //                    FreeStockHistory.userid = userid;
            //                    FreeStockHistory.CreationDate = indianTime;
            //                    db.FreeStockHistoryDB.Add(FreeStockHistory);

            //                }

            //            }

            //            int CurrentInventory = CurrentStock.CurrentInventory;//current inventory
            //            int ManualUpdateInventory = CurrentStock.CurrentInventory - item.Transferinventory;// New Added Inventory

            //            CurrentStock.ManualReason = "Stock transfer to Freestock= " + item.ManualReason;
            //            CurrentStock.UpdatedDate = indianTime;
            //            CurrentStock.CurrentInventory -= item.Transferinventory;
            //            db.Entry(CurrentStock).State = EntityState.Modified;
            //            if (db.Commit() > 0)
            //            {
            //                result = item.Transferinventory + " Qty Transfered to FreeStock Successfully";
            //                try
            //                {
            //                    CurrentStock.CityName = Warehouse.CityName;
            //                    CurrentStock.CityId = Warehouse.Cityid;
            //                    SendMailManualInventory(CurrentInventory, ManualUpdateInventory, User.DisplayName, CurrentStock);
            //                }
            //                catch (Exception ex)
            //                {
            //                    logger.Error("Error in send mail stock transfer " + ex.Message);
            //                    logger.Info("End  send mail stock transfer: ");
            //                    return null;
            //                }
            //                return Request.CreateResponse(HttpStatusCode.OK, result);
            //            };
            //        }
            //        else
            //        {
            //            result = "CurretStock Qty not availble to transfer in FreeStock of Qty: " + item.Transferinventory;
            //            return Request.CreateResponse(HttpStatusCode.OK, result);
            //        }
            //    }
            //}
            //result = "Something went wrong";
            //return Request.CreateResponse(HttpStatusCode.OK, result);
            #endregion
        }


        #endregion
        #region  enter the safety stock 
        /// <summary>
        /// Created Date 12/10/2019
        /// Created by Pooja K
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        [Authorize]
        [Route("SafetyStock")]
        [HttpPut]
        public async Task<string> Safetystockput(CurrentStock item)
        {

            string status = "";
            using (AuthContext context = new AuthContext())
            {
                var identity = User.Identity as ClaimsIdentity;
                int compid = 0, userid = 0;

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))
                    compid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                    userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

                var UserName = context.Peoples.Where(x => x.PeopleID == userid).Select(a => a.DisplayName).SingleOrDefault();
                var stock = context.DbCurrentStock.Where(x => x.StockId == item.StockId && x.Deleted == false).SingleOrDefault();
                var Oldstockqty = stock.SafetystockfQuantity;

                List<ItemClassificationDC> itemsList = new List<ItemClassificationDC>();

                ItemClassificationDC obj = new ItemClassificationDC();
                obj.ItemNumber = stock.ItemNumber;
                obj.WarehouseId = stock.WarehouseId;
                itemsList.Add(obj);

                var manager = new ItemLedgerManager();
                var ABCClassification = await manager.GetItemClassificationsAsync(itemsList);

                if (ABCClassification.Count == 0 || ABCClassification.Any(x => x.Category == "C"))
                {
                    string cat = "";
                    if (ABCClassification.Count == 0)
                    {
                        cat = "D";
                    }
                    else
                    {
                        cat = "C";
                    }
                    // return "You can't update safety stock, due to item is in "+ cat + " Category";
                }

                if (stock != null)
                {
                    stock.UpdatedDate = indianTime;
                    stock.SafetystockfQuantity = item.SafetystockfQuantity;
                    context.Entry(stock).State = EntityState.Modified;

                    SafetyStockHistory safeStockHistory = new SafetyStockHistory();
                    safeStockHistory.StockId = stock.StockId;
                    safeStockHistory.NewSafetyStock = stock.SafetystockfQuantity;
                    safeStockHistory.OldSafetyStock = Oldstockqty;
                    safeStockHistory.ItemMultiMRPId = stock.ItemMultiMRPId;
                    safeStockHistory.ItemNumber = stock.ItemNumber;
                    safeStockHistory.UpdateBy = UserName;
                    safeStockHistory.CreatedBy = UserName;
                    safeStockHistory.updationDate = DateTime.Now;
                    safeStockHistory.WarehouseId = stock.WarehouseId;
                    safeStockHistory.CreationDate = DateTime.Now;
                    context.SafetyStockHistoryDB.Add(safeStockHistory);
                    if (context.Commit() > 0)
                    {
                        status = "Record Updated Successfully ";
                    }
                    return status;
                }
                return "Something went wrong";
            }
        }

        #endregion

        [Route("ManualEntriesTotalExport")]
        [HttpGet]
        public HttpResponseMessage ManualEntriesTotalExport(int WarehouseId)//get all 
        {
            using (AuthContext context = new AuthContext())
            {
                int MonthADD = 0; int MonthSub = 0; int TodayAdd = 0; int Todaysub = 0;
                var MonthADDList = new List<CurrentStockManualExportDC>();
                var MonthSubList = new List<CurrentStockManualExportDC>();
                var TodayAddList = new List<CurrentStockManualExportDC>();
                var TodaysubList = new List<CurrentStockManualExportDC>();

                //string Query = "select stockId,ItemMultiMRPId, ItemName, ManualInventoryIn, ManualReason, UserName, CreationDate, WarehouseName from CurrentStockHistories"
                //+ " where ManualInventoryIn is not null and Month(CreationDate) = MONTH(getdate()) and Year(CreationDate) = Year(getdate()) and Warehouseid =" + WarehouseId;
                string Query = "Exec GetMonthlyManualEditStock " + WarehouseId;
                var _result = context.Database.SqlQuery<CurrentStockManualExportDC>(Query).ToList();

                if (_result != null)
                {
                    MonthADDList = _result.Where(x => x.ManualInventoryIn > 0).ToList();
                    MonthSubList = _result.Where(x => x.ManualInventoryIn < 0).ToList();
                    MonthADD = _result.Where(x => x.ManualInventoryIn > 0).Select(x => x.ManualInventoryIn).Sum();
                    MonthSub = _result.Where(x => x.ManualInventoryIn < 0).Select(x => x.ManualInventoryIn).Sum();

                    var todayresult = _result.Where(x => x.CreationDate.Day == indianTime.Day).ToList();

                    if (todayresult != null)
                    {
                        TodayAddList = _result.Where(x => x.ManualInventoryIn < 0).ToList();
                        TodaysubList = _result.Where(x => x.ManualInventoryIn > 0).ToList();
                        Todaysub = todayresult.Where(x => x.ManualInventoryIn < 0).Select(x => x.ManualInventoryIn).Sum();
                        TodayAdd = todayresult.Where(x => x.ManualInventoryIn > 0).Select(x => x.ManualInventoryIn).Sum();
                    }

                    var responce = new
                    {
                        MonthADDList = MonthADDList,
                        MonthSubList = MonthSubList,
                        TodaysubList = TodaysubList,
                        TodayAddList = TodayAddList,
                        MonthADD = MonthADD,
                        MonthSub = MonthSub,
                        Todaysub = Todaysub,
                        TodayAdd = TodayAdd
                    };

                    return Request.CreateResponse(HttpStatusCode.OK, responce);
                }
                else
                {
                    var responce = new
                    {
                        MonthADDList = MonthADDList,
                        MonthSubList = MonthSubList,
                        TodaysubList = TodaysubList,
                        TodayAddList = TodayAddList,
                        MonthADD = MonthADD,
                        MonthSub = MonthSub,
                        Todaysub = Todaysub,
                        TodayAdd = TodayAdd
                    };
                    return Request.CreateResponse(HttpStatusCode.OK, responce);
                }

            }

        }


        [Route("ManualEntriesExport")]
        [HttpGet]
        public dynamic ManualEntriesExport(int WarehouseId)//get all 
        {
            using (AuthContext context = new AuthContext())
            {
                // int MonthADD = 0; int MonthSub = 0; int TodayAdd = 0; int Todaysub = 0;
                var MonthADDList = new List<CurrentStockManualExportDC>();
                var MonthSubList = new List<CurrentStockManualExportDC>();
                var TodayAddList = new List<CurrentStockManualExportDC>();
                var TodaysubList = new List<CurrentStockManualExportDC>();

                //string Query = "select stockId,ItemMultiMRPId, ItemName, ManualInventoryIn, ManualReason, UserName, CreationDate, WarehouseName from CurrentStockHistories"
                //+ " where ManualInventoryIn is not null and Month(CreationDate) = MONTH(getdate()) and Year(CreationDate) = Year(getdate()) and Warehouseid =" + WarehouseId;
                string Query = "Exec GetMonthlyManualEditStock " + WarehouseId;
                var _result = context.Database.SqlQuery<CurrentStockManualExportDC>(Query).ToList();

                return _result;
            }

        }


        [Route("getExportData")]
        [HttpGet]
        public async Task<List<CurrentStockDC>> getExportData(int WarehouseId)
        {
            List<CurrentStockDC> result = new List<CurrentStockDC>();

            if (WarehouseId > 0)
            {
                using (var context = new AuthContext())
                {

                    result = await GetAllCurrentStock(WarehouseId, context);
                }
            }
            return result;
        }


        #region Warehouse GetWarehouseStockItem
        [Route("GetWarehouseStockItem")]
        [HttpGet]
        public IEnumerable<CurrentStockDC> GetWarehouseStockItem(int WarehouseId)
        {
            logger.Info("start current stock: ");
            using (AuthContext context = new AuthContext())
            {
                List<CurrentStockDC> ass = new List<CurrentStockDC>();
                try
                {
                    var identity = User.Identity as ClaimsIdentity;
                    int compid = 0, userid = 0;
                    int Warehouse_id = 0;


                    if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))
                        compid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);

                    if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                        userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

                    int CompanyId = compid;
                    Warehouse_id = WarehouseId;
                    logger.Info("User ID : {0} , Company Id : {1}", compid, userid);
                    if (WarehouseId > 0)
                    {
                        string query = "select * from CurrentStocks with(nolock) where Deleted=0 and CurrentInventory>0 and  WarehouseId=" + Warehouse_id;
                        ass = context.Database.SqlQuery<CurrentStockDC>(query).ToList();
                    }
                    logger.Info("End  current stock: ");
                    return ass;
                }
                catch (Exception ex)
                {
                    logger.Error("Error in current stock " + ex.Message);
                    logger.Info("End  current stock: ");
                    return null;
                }
            }
        }
        #endregion

        #region Get Virtual stock data
        /// <summary>
        /// Created Date 21/04/2020
        /// Created by Raj
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("GetVirtuals")]

        public VirtualstockDataDC GetVirtualStock(VirtualStockPaginator vs)
        {
            using (var context = new AuthContext())
            {

                VirtualstockDataDC virtualstockDataDC = new VirtualstockDataDC();
                if (vs.EndDate.HasValue)
                {
                    vs.EndDate = vs.EndDate.Value.AddDays(1).AddSeconds(-1);
                }
                //var query = from v in context.VirtualStockDB
                //            join item in context.DbCurrentStock
                //            on v.ItemMultiMRPId equals item.ItemMultiMRPId
                //            where v.WarehouseId == vs.WarehouseId && item.WarehouseId == vs.WarehouseId && item.Deleted == false
                //            && (!vs.StartDate.HasValue || v.CreatedDate >= vs.StartDate)
                //            && (!vs.EndDate.HasValue || v.CreatedDate <= vs.EndDate)
                //            //group new { v, item } by new
                //            //{
                //            //    Id = item.MRP
                //            //}
                //            //into g
                //            select new VirtualStockDetailsDC
                //            {
                //                ItemName = item.itemname,        //g.FirstOrDefault().item.itemname,//  +" "+ g.FirstOrDefault().item.MRP + "  MRP  " + g.FirstOrDefault().item.UnitofQuantity + "  "+ g.FirstOrDefault().item.UOM,
                //                StockType = v.StockType,     //g.FirstOrDefault().v.StockType,
                //                WarehouseName = item.WarehouseName, //g.FirstOrDefault().item.WarehouseName,
                //                InOutQty = v.InOutQty,      //g.FirstOrDefault().v.InOutQty,
                //                CreatedDate = v.CreatedDate,    //g.FirstOrDefault().v.CreatedDate
                //                Reason = v.Reason
                //            };

                var p1 = new SqlParameter("@warehouseid", vs.WarehouseId);
                var p2 = new SqlParameter("@startDate", vs.StartDate);

                var p3 = new SqlParameter("@endDate", vs.EndDate);

                var p4 = new SqlParameter("@skip", vs.Skip);
                var p5 = new SqlParameter("@take", vs.Take);
                var p6 = new SqlParameter("@checkk", vs.checkk);

                List<VirtualStockDetailsDC> virtualStockDetails = context.Database.SqlQuery<VirtualStockDetailsDC>("exec sp_GetVirtualStock @warehouseid, @startDate,@endDate,@skip,@take,@checkk",
                   p1, p2, p3, p4, p5, p6).ToList();

                virtualstockDataDC.virtualStockDetailsDCs = virtualStockDetails;
                virtualstockDataDC.TotalRecords = virtualStockDetails.FirstOrDefault().TotalRecords;
                return (virtualstockDataDC);


                //context.Database.SqlQuery("exec sp_GetVirtualStock @warehouseid, @startDate,@endDate,@skip,@take,@checkk",).
                //virtualstockDataDC.virtualStockDetailsDCs = query.OrderByDescending(x => x.CreatedDate).Skip(vs.Skip).Take(vs.Take).ToList();
                //  virtualstockDataDC.TotalRecords = query.Count();

                // return virtualStockDetails;
            }


        }
        #endregion


        #region Get Virtual stock data  Export
        /// <summary>
        /// Created Date 12/05/2020
        /// Created by Raj
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("GetVirtualsExport")]

        public List<VirtualStockDetailsDC> GetVirtualsExport(VirtualStockPaginator vs)
        {
            using (var context = new AuthContext())
            {

                List<VirtualStockDetailsDC> virtualstockDetailsDC = new List<VirtualStockDetailsDC>();
                if (vs.EndDate.HasValue)
                {
                    vs.EndDate = vs.EndDate.Value.AddDays(1).AddSeconds(-1);
                }
                var query = from v in context.VirtualStockDB
                            join item in context.DbCurrentStock
                            on v.ItemMultiMRPId equals item.ItemMultiMRPId
                            where v.WarehouseId == vs.WarehouseId && item.WarehouseId == vs.WarehouseId && item.Deleted == false
                            && (!vs.StartDate.HasValue || v.CreatedDate >= vs.StartDate)
                            && (!vs.EndDate.HasValue || v.CreatedDate <= vs.EndDate)
                            //group new { v, item } by new
                            //{
                            //    Id = item.MRP
                            //}
                            //into g
                            select new VirtualStockDetailsDC
                            {
                                ItemName = item.itemname,        //g.FirstOrDefault().item.itemname,//  +" "+ g.FirstOrDefault().item.MRP + "  MRP  " + g.FirstOrDefault().item.UnitofQuantity + "  "+ g.FirstOrDefault().item.UOM,
                                StockType = v.StockType,     //g.FirstOrDefault().v.StockType,
                                WarehouseName = item.WarehouseName, //g.FirstOrDefault().item.WarehouseName,
                                InOutQty = v.InOutQty,      //g.FirstOrDefault().v.InOutQty,
                                CreatedDate = v.CreatedDate,     //g.FirstOrDefault().v.CreatedDate
                                Reason = v.Reason
                            };
                virtualstockDetailsDC = query.OrderByDescending(x => x.CreatedDate).ToList();

                return virtualstockDetailsDC;
            }


        }
        #endregion


        [Route("GetWarehousebasedV7")]
        [HttpPost]
        public IHttpActionResult GetWarehousebasedV7(int WarehouseId, PagerDataUIViewModel pager)
        {

            using (AuthContext context = new AuthContext())
            {
                var WarehouseidParam = new SqlParameter
                {
                    ParameterName = "WarehouseId",
                    Value = WarehouseId
                };


                var FirstParam = new SqlParameter
                {
                    ParameterName = "First",
                    Value = pager.First
                };

                var LastParam = new SqlParameter
                {
                    ParameterName = "Last",
                    Value = pager.Last
                };

                var ItemNumberParam = new SqlParameter
                {
                    ParameterName = "ItemNumber",
                    Value = pager.Contains == null ? DBNull.Value : (object)pager.Contains

                };

                var ItemNameParam = new SqlParameter
                {
                    ParameterName = "ItemName",
                    Value = pager.Contains == null ? DBNull.Value : (object)pager.Contains

                };





                List<CurrentStockDC> objCurrentStockDc = context.Database.SqlQuery<CurrentStockDC>("GetCurrentStock_With_Pagination @WarehouseId ,@First,@Last,@ItemNumber,@ItemName ", WarehouseidParam,
                    FirstParam, LastParam, ItemNumberParam, ItemNameParam).ToList();

                int Totalcount = context.DbCurrentStock.Where(x => x.WarehouseId == WarehouseId).Count();
                CurrentStockResponse objCurrentStockResponse = new CurrentStockResponse()
                {
                    currentStockList = objCurrentStockDc,
                    totalRecords = Totalcount
                };


                if (objCurrentStockResponse != null && objCurrentStockResponse.currentStockList.Count > 0)
                {
                    TripPlannerHelper tripPlannerHelper = new TripPlannerHelper();
                    List<ItemWarehouseDc> itemWarehouseDcs = new List<ItemWarehouseDc>();
                    var itemWarehouse = objCurrentStockResponse.currentStockList.Select(x => new ItemWarehouseDc { WarehouseId = x.WarehouseId, ItemMultiMRPId = x.ItemMultiMRPId }).ToList();
                    var list = tripPlannerHelper.RocTagValueGet(itemWarehouse);
                    if (list != null)
                    {
                        foreach (var da in objCurrentStockResponse.currentStockList)
                        {
                            da.Tag = list.Result.Where(x => x.ItemMultiMRPId == da.ItemMultiMRPId && x.WarehouseId == da.WarehouseId).Select(x => x.Tag).FirstOrDefault();
                        }
                    }
                }


                return Ok(objCurrentStockResponse);




            }
        }

        [Route("GetItemClassificationsAsync")]
        [HttpPost]
        public async Task<List<ItemClassificationDC>> GetItemClassificationsAsync(List<DataContracts.Shared.ItemClassificationDC> itemsList)
        {
            var manager = new ItemLedgerManager();
            return await manager.GetItemClassificationsAsync(itemsList);
        }

        [Route("GetItemClassificationsForExportAsync")]
        [HttpPost]
        public async Task<List<ItemClassificationForOffer>> GetItemClassificationsForExportAsync(List<ItemClassificationForOffer> itemsList)
        {

            using (AuthContext authcontext = new AuthContext())
            {
                List<ItemClassificationDC> ItemClassificationList = new List<ItemClassificationDC>();

                foreach (var item in itemsList)
                {
                    var itemforitemnumber = authcontext.itemMasters.FirstOrDefault(x => x.ItemMultiMRPId == item.ItemMultiMRPId);
                    if (itemforitemnumber != null && itemforitemnumber.ItemNumber != null)
                    {
                        item.ItemNumber = itemforitemnumber.ItemNumber;
                    }
                    else
                    {
                        itemsList = itemsList.Where(x => x.ItemMultiMRPId != item.ItemMultiMRPId).ToList();
                    }
                }

                ItemClassificationList = Mapper.Map(itemsList).ToANew<List<ItemClassificationDC>>();
                var manager = new ItemLedgerManager();


                var ItemsList = await manager.GetItemClassificationsAsync(ItemClassificationList);
                var ItemsMainList = Mapper.Map(ItemsList).ToANew<List<ItemClassificationForOffer>>();
                List<ItemClassificationForOffer> ItemsListForOffer = new List<ItemClassificationForOffer>();
                foreach (var classificationitm in ItemsMainList)
                {
                    var itemforitemnumber = itemsList.FirstOrDefault(x => x.ItemNumber == classificationitm.ItemNumber);
                    ItemClassificationForOffer offerItem = new ItemClassificationForOffer();
                    if (itemforitemnumber != null && itemforitemnumber.ItemNumber != null)
                    {
                        offerItem.ItemId = itemforitemnumber.ItemId;
                        offerItem.WarehouseId = classificationitm.WarehouseId;
                        offerItem.Category = classificationitm.Category;
                        offerItem.ItemNumber = classificationitm.ItemNumber;
                        offerItem.ItemMultiMRPId = itemforitemnumber.ItemMultiMRPId;
                        ItemsListForOffer.Add(offerItem);
                    }
                    //else
                    //{
                    //    itemsList = itemsList.Where(x => x.ItemNumber != item.ItemNumber).ToList();
                    //}
                }
                return ItemsListForOffer;
            }
        }

        //Get: GetCurrentStockBatchMastersData
        [Route("GetCurrentStockBatchMastersData")]
        [HttpGet]
        [AllowAnonymous]
        public async Task<List<CurrentStockBatchMasterDC>> GetCurrentStockBatchMastersData(int stockId, string stockType)
        {
            StockBatchTransactionManager manager = new StockBatchTransactionManager();
            var currentStockBatchMasterList = await manager.GetCurrentStockBatchMastersData(stockId, stockType);
            return currentStockBatchMasterList;
        }
        [Route("BatchcodeWiseCurrentStock")]
        [HttpGet]
        [AllowAnonymous]
        public async Task<List<BatchcodeWiseCurrentStockDC>> BatchcodeWiseCurrentStock(int warehouseid)
        {
            StockBatchTransactionManager manager = new StockBatchTransactionManager();
            var currentStockBatchWiseMasterList = await manager.BatchcodeWiseCurrentStock(warehouseid);
            return currentStockBatchWiseMasterList;
        }

        [Route("AllWarehouseBatchcodeWiseCurrentStockReport")]
        [HttpGet]
        public bool AllWarehouseBatchcodeWiseCurrentStockReport()
        {
            using (var db = new AuthContext())
            {
                List<BatchcodeWiseCurrentStockDC> ReportList = new List<BatchcodeWiseCurrentStockDC>();

                ReportList = db.Database.SqlQuery<BatchcodeWiseCurrentStockDC>("Exec AllWarehouseBatchcodeWiseCurrentStockReport").ToList();

                string ExcelSavePath = HostingEnvironment.MapPath("~/ExcelGeneratePath/");
                if (!Directory.Exists(ExcelSavePath))
                    Directory.CreateDirectory(ExcelSavePath);

                var dataTables = new List<DataTable>();
                DataTable dt = ClassToDataTable.CreateDataTable(ReportList);
                dt.TableName = "AllWarehouseBatchcodeWiseCurrentStockReport";
                dataTables.Add(dt);

                string filePath = ExcelSavePath + "AllWarehouseBatchcodeWiseCurrentStockReport" + DateTime.Now.ToString("yyyy-dd-M--HH-mm-ss") + ".xlsx";
                if (ExcelGenerator.DataTable_To_Excel(dataTables, filePath))
                {
                    string connectionString = ConfigurationManager.ConnectionStrings["AuthContext"].ConnectionString;
                    string To = "", From = "", Bcc = "";
                    DataTable emaildatatable = new DataTable();
                    using (var connection = new SqlConnection(connectionString))
                    {
                        using (var command = new SqlCommand("Select * from EmailRecipients where EmailType='AllWarehouseBatchcodeWiseCurrentStockReport'", connection))
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
                    string subject = DateTime.Now.ToString("dd MMM yyyy") + " All Warehouse BatchcodeWise CurrentStock Report";
                    string message = "Please find attach All Warehouse BatchcodeWise CurrentStock Report";
                    if (!string.IsNullOrEmpty(To) && !string.IsNullOrEmpty(From))
                    {
                        EmailHelper.SendMail(From, To, Bcc, subject, message, filePath);
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                return true;
            }
        }




        //Get: GetStockBatchMastersData
        [Route("GetStockBatchMastersData")]
        [HttpGet]
        [AllowAnonymous]
        public async Task<List<StockBatchMasterDC>> GetStockBatchMastersData(int ItemMultiMRPId, int WarehouseId, string stockType)
        {
            StockBatchTransactionManager manager = new StockBatchTransactionManager();
            var currentStockBatchMasterList = await manager.GetStockBatchMastersData(ItemMultiMRPId, WarehouseId, stockType);
            return currentStockBatchMasterList;
        }

        [Route("GetStockBatchMastersDataNew")]
        [HttpGet]
        [AllowAnonymous]
        public async Task<List<CurrentStockBatchMasterDC>> GetStockBatchMastersDataNew(int ItemMultiMRPId, int WarehouseId, string stockType)
        {
            StockBatchTransactionManager manager = new StockBatchTransactionManager();
            var currentStockBatchMasterList = await manager.GetBatchcodeQty(ItemMultiMRPId, WarehouseId, stockType);
            return currentStockBatchMasterList;
        }

        [Route("GetAPPIteMultiMrpWhWise")]
        [HttpGet]
        [AllowAnonymous]
        public async Task<APPDC> GetAPPIteMultiMrpWhWise(int ItemMultiMRPId, int WarehouseId, string stockType)
        {
            APPDC res = new APPDC();
            if (ItemMultiMRPId > 0 && WarehouseId > 0)
            {
                using (var myContext = new AuthContext())
                {
                    var ItemMultiMRPIdParam = new SqlParameter("@ItemMultiMRPId", ItemMultiMRPId);
                    var WarehouseIdParam = new SqlParameter("@WarehouseId", WarehouseId);
                    //var stockTypeParam = new SqlParameter("@stockType", stockType);

                    res = await myContext.Database.SqlQuery<APPDC>("BatchCode.GetAPPByMultiMRPId @ItemMultiMRPId,@WarehouseId", ItemMultiMRPIdParam, WarehouseIdParam).FirstOrDefaultAsync();
                }
            }
            return res;
        }

        [Route("GetBatchMasterList")]
        [HttpGet]
        [AllowAnonymous]

        public async Task<List<BatchMasterDc>> GetBatchMasterList(int itemMultiMRPId, int warehouseId, string keyword)
        {
            StockBatchTransactionManager manager = new StockBatchTransactionManager();
            var currentStockBatchMasterList = await manager.GetBatchMasterList(itemMultiMRPId, warehouseId, keyword);
            return currentStockBatchMasterList;
        }

    }


    public class PlannedStDc
    {
        public int WarehouseId { get; set; }
        public int ItemMultiMRPId { get; set; }
        public int InOutQty { get; set; }
    }

    public class CurrentStockssDTO
    {
        public string ItemName { get; set; }
        public string ItemNumber { get; set; }
        public int WarehouseId { get; set; }
        public string WarehouseName { get; set; }
        public string CategoryName { get; set; }
        public int ItemMultiMRPId { get; set; }
        public double MRP { get; set; }
        public int CurrentInventory { get; set; }
        public string ShowTypes { get; set; }
        public int StockId { get; set; }
        public string SubsubcategoryName { get; set; }
        public string UnitofQuantity { get; set; }
        public string UOM { get; set; }//Unit of masurement like GM Kg 
        public DateTime CreationDate { get; set; }
        public DateTime UpdatedDate { get; set; }
        public int PlannedStock { get; set; }


    }

    public class CurrentStockManualExportDC
    {
        public int stockId { get; set; }
        public int ItemMultiMRPId { get; set; }
        public string UserName { get; set; }
        public string WarehouseName { get; set; }
        public string ItemName { get; set; }
        public int ManualInventoryIn { get; set; }
        public string ManualReason { get; set; }
        public DateTime CreationDate { get; set; }
    }

    public class TransferToFreestockDTO
    {
        public string ItemNumber { get; set; }
        public int ItemMultiMRPId { get; set; }
        public int WarehouseId { get; set; }
        public int Transferinventory { get; set; }
        public string ManualReason { get; set; }
        public long StockBatchMasterId { get; set; }
    }






    public class dataSelects
    {
        public int totalOrder { get; set; }
        public double totalSale { get; set; }
        public int pendingOrder { get; set; }
        public double PendingSale { get; set; }

    }

    public class InvReDTO
    {
        public int ItemMultiMRPId { get; set; }
        public string Number { get; set; }
        public bool active { get; set; }
    }
    public class InvReportDto
    {
        public int ItemMultiMRPId { get; set; }
        public int StockId { get; set; }
        public string ItemNumber { get; set; }
        public string itemname { get; set; }
        public int ClosingStock { get; set; }
        public double? MRP { get; set; }
        public int? Lastsavendays { get; set; }
        public int? Lastfifteendays { get; set; }
        public int? LastThirtydays { get; set; }
        public bool IsActivate { get; set; }

        public string WarehouseName { get; set; }
        public string Category { get; set; }


    }






    public class TransferStockDTO
    {
        public string ItemNumber { get; set; }
        public string ItemNumberTrans { get; set; }
        public int ItemMultiMRPId { get; set; }
        public int ItemMultiMRPIdTrans { get; set; }
        public int WarehouseId { get; set; }
        public int CurrentInventory { get; set; }
        public string ManualReason { get; set; }
    }

    public class TransferStockDTONew
    {
        public long StockBatchMasterId { get; set; }
        public long BatchMasterId { get; set; }
        public string ItemNumber { get; set; }
        public string ItemNumberTrans { get; set; }
        public int ItemMultiMRPId { get; set; }
        public int ItemMultiMRPIdTrans { get; set; }
        public int WarehouseId { get; set; }
        public int Qty { get; set; }
        public string ManualReason { get; set; }
    }


    public class TempcurrentHistorycollectionPaggingData
    {
        public int total_count { get; set; }
        public List<TemporaryCurrentStockHistory> TemporaryCurrentStockHistorydata { get; set; }
    }

    public class ItemValue
    {
        public double ActiveItemValue { get; set; }
        public double DeactiveItemValue { get; set; }
    }

    public class CurrentStockPagerListDTO
    {
        public List<CurrentStock> CurrentStockPagerList { get; set; }
        public int TotalRecords { get; set; }

    }

    public class CurrentStockResponse
    {
        public List<CurrentStockDC> currentStockList { get; set; }
        public int totalRecords { get; set; }
    }

    public class CurrentStockDC
    {
        public int PlannedStock { get; set; }
        public int StockId { get; set; }
        public int CompanyId { get; set; }
        public int WarehouseId { get; set; }
        public string WarehouseName { get; set; }
        //public string SellingSku { get; set; }
        // public int ItemId { get; set; }
        public string ItemNumber { get; set; }
        public int CurrentInventory { get; set; }
        public DateTime CreationDate { get; set; }
        public DateTime UpdatedDate { get; set; }
        public string CreatedBy { get; set; }
        public string UpdateBy { get; set; }
        public bool Deleted { get; set; }
        public bool IsEmptyStock { get; set; }
        public string itemname { get; set; }
        public string itemBaseName { get; set; }
        public int ItemMultiMRPId { get; set; }
        public double MRP { get; set; }
        public string UnitofQuantity { get; set; }
        public string UOM { get; set; }//Unit of masurement like GM Kg 
        public int SafetystockfQuantity { get; set; }
        public int? CityId { get; set; }
        public string CityName { get; set; }
        public string ManualReason { get; set; }
        public int userid { get; set; }
        public string ShowTypes { get; set; }
        public string ABCClassification { get; set; }
        public double APP { get; set; }
        public double APPtotalamount { get; set; }
        public long StockBatchMasterId { get; set; }
        public int Tag { get; set; } // New Add for ROC
    }
    public class VirtualStockPaginator
    {
        public int WarehouseId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int Take { get; set; }
        public int Skip { get; set; }
        public bool checkk { get; set; }

    }

    public class VirtualStockDetailsDC
    {
        public string ItemName { get; set; }
        public string StockType { get; set; }
        public string WarehouseName { get; set; }
        public int InOutQty { get; set; }
        public DateTime CreatedDate { get; set; }
        public string Reason { get; set; }
        public int TotalRecords { get; set; }

    }

    public class VirtualstockDataDC
    {

        public int TotalRecords { get; set; }
        public List<VirtualStockDetailsDC> virtualStockDetailsDCs { get; set; }
    }



    public class MultiMRPDetailsDC
    {
        public string ItemName { get; set; }
        public string ItemBaseName { get; set; }
        public string WarehouseName { get; set; }
        public int ItemMultiMRPId { get; set; }
        public int WarehouseId { get; set; }
        public string ItemNumber { get; set; }
        public double MRP { get; set; }
    }
}



