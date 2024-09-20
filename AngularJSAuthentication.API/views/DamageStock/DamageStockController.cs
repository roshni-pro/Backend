using AngularJSAuthentication.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Web.Http;
using System.Web.Http.Description;
using NLog;
using System.Data.Entity;
using AngularJSAuthentication.API.Helper;
using AngularJSAuthentication.DataContracts.Transaction.Stocks;
using AngularJSAuthentication.DataContracts.constants;
using System.Transactions;
using System.Data.SqlClient;
using AngularJSAuthentication.BusinessLayer.Managers.Reports;
using AngularJSAuthentication.DataContracts.Shared;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using AngularJSAuthentication.Model.Stocks;
using AngularJSAuthentication.DataContracts.Transaction.Ledger.ItemLedger;
using AngularJSAuthentication.Common.Helpers;
using System.Configuration;
using AngularJSAuthentication.API.Managers;
using System.Data.Entity.Infrastructure;
using LinqKit;
using AngularJSAuthentication.API.Helpers;
using System.Web;
using System.IO;
using AngularJSAuthentication.Model.Stocks.Batch;
using AngularJSAuthentication.Model.NonRevenueOrders;
using System.Data;

namespace AngularJSAuthentication.API.Controllers
{
    [RoutePrefix("api/damagestock")]

    public class DamageStockController : ApiController
    {

        private static TimeZoneInfo INDIAN_ZONE = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
        DateTime indianTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE);
        public static Logger logger = LogManager.GetCurrentClassLogger();
        public static List<int> DamageRequestIds = new List<int>();
        [Route("get")]
        [HttpGet]
        public PaggingDatastock get(int list, int page, int WarehouseId)
        {

            try
            {
                var identity = User.Identity as ClaimsIdentity;
                int compid = 0, userid = 0;
                int Warehouse_id = 0;

                using (AuthContext context = new AuthContext())
                {
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
                    logger.Info("User ID : {0} , Company Id : {1}", compid, userid);
                    PaggingDatastock data = new PaggingDatastock();
                    var total_count = context.DamageStockDB.Where(x => x.Deleted == false && x.CompanyId == compid && x.WarehouseId == WarehouseId).Count();
                    var damagest = context.DamageStockDB.Where(x => x.Deleted == false && x.CompanyId == compid && x.WarehouseId == WarehouseId).OrderByDescending(x => x.DamageStockId).Skip((page - 1) * list).Take(list).ToList();
                    data.damagest = damagest;
                    data.total_count = total_count;
                    return data;
                }
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        [Route("getList")]
        [HttpGet]
        public async System.Threading.Tasks.Task<PaggingDatastock> getListAsync(int WarehouseId)
        {
            try
            {
                var identity = User.Identity as ClaimsIdentity;
                int compid = 0, userid = 0;
                int Warehouse_id = 0;

                using (AuthContext context = new AuthContext())
                {

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
                    logger.Info("User ID : {0} , Company Id : {1}", compid, userid);
                    PaggingDatastock data = new PaggingDatastock();
                    var total_count = context.DamageStockDB.Where(x => x.Deleted == false && x.CompanyId == compid && x.WarehouseId == WarehouseId).Count();
                    var damagest = context.DamageStockDB.Where(x => x.Deleted == false && x.CompanyId == compid && x.WarehouseId == WarehouseId).OrderByDescending(x => x.DamageStockId).ToList();
                    var manager = new ItemLedgerManager();
                    List<ItemClassificationDC> objItemClassificationDClist = new List<ItemClassificationDC>();
                    foreach (var dmgstockdata in damagest)
                    {
                        ItemClassificationDC obj = new ItemClassificationDC();
                        obj.WarehouseId = dmgstockdata.WarehouseId;
                        obj.ItemNumber = dmgstockdata.ItemNumber;
                        objItemClassificationDClist.Add(obj);

                    }
                    List<ItemClassificationDC> _objItemClassificationDClist = await manager.GetItemClassificationsAsync(objItemClassificationDClist);
                    List<DamageStock> ObjDmgItemData = new List<DamageStock>();
                    foreach (var item in damagest)
                    {
                        if (_objItemClassificationDClist != null && _objItemClassificationDClist.Any())
                        {

                            if (_objItemClassificationDClist.Any(x => x.ItemNumber == item.ItemNumber))
                            {
                                item.ABCClassification = _objItemClassificationDClist.Where(x => x.ItemNumber == item.ItemNumber).Select(x => x.Category).FirstOrDefault();
                            }
                            else { item.ABCClassification = "D"; }
                        }
                        else
                        {
                            item.ABCClassification = "D";
                        }

                    }
                    data.damagest = damagest;
                    data.total_count = total_count;
                    return data;
                }
            }
            catch (Exception ex)
            {
                return null;
            }
        }


        [Route("postList")]
        [HttpPost]
        public async System.Threading.Tasks.Task<PaggingDatastock> PostListAsync(string WarehouseId)
        {
            try
            {
                var identity = User.Identity as ClaimsIdentity;
                int compid = 0, userid = 0;
                int Warehouse_id = 0;
                var WarehouseIds = WarehouseId.Split(',').Select(Int32.Parse).ToList();


                using (AuthContext context = new AuthContext())
                {

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
                    logger.Info("User ID : {0} , Company Id : {1}", compid, userid);
                    PaggingDatastock data = new PaggingDatastock();
                    //ass = context.DamageStockDB.Where(x => WarehouseIds.Contains(x.WarehouseId) && x.Deleted == false && x.ItemMultiMRPId > 0).ToList();
                    var total_count = context.DamageStockDB.Count(x => x.Deleted == false && x.CompanyId == compid && WarehouseIds.Contains(x.WarehouseId));
                    var damagest = context.DamageStockDB.Where(x => x.Deleted == false && x.CompanyId == compid && WarehouseIds.Contains(x.WarehouseId)).OrderByDescending(x => x.DamageStockId).ToList();
                    var manager = new ItemLedgerManager();
                    List<ItemClassificationDC> objItemClassificationDClist = new List<ItemClassificationDC>();

                    foreach (var dmgstockdata in damagest)
                    {
                        ItemClassificationDC obj = new ItemClassificationDC();
                        obj.WarehouseId = dmgstockdata.WarehouseId;
                        obj.ItemNumber = dmgstockdata.ItemNumber;
                        objItemClassificationDClist.Add(obj);

                    }
                    List<ItemClassificationDC> _objItemClassificationDClist = await manager.GetItemClassificationsAsync(objItemClassificationDClist);
                    List<DamageStock> ObjDmgItemData = new List<DamageStock>();
                    foreach (var item in damagest)
                    {
                        if (_objItemClassificationDClist != null && _objItemClassificationDClist.Any())
                        {

                            if (_objItemClassificationDClist.Any(x => x.ItemNumber == item.ItemNumber))
                            {
                                item.ABCClassification = _objItemClassificationDClist.Where(x => x.ItemNumber == item.ItemNumber).Select(x => x.Category).FirstOrDefault();
                            }
                            else { item.ABCClassification = "D"; }
                        }
                        else
                        {
                            item.ABCClassification = "D";
                        }

                    }
                    data.damagest = damagest;
                    data.total_count = total_count;
                    return data;
                }
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        //[Route("getall")]
        //[HttpGet]
        //public HttpResponseMessage Get(int WarehouseId)//get all Issuances which are active for the delivery boy
        //{

        //    try
        //    {
        //        using (AuthContext context = new AuthContext())
        //        {
        //            var identity = User.Identity as ClaimsIdentity;
        //            int compid = 0, userid = 0;

        //            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))
        //                compid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);

        //            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
        //                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

        //            logger.Info("User ID : {0} , Company Id : {1}", compid, userid);
        //            var DamageitemData = context.DamageStockDB.Where(x => x.Deleted == false && x.DamageInventory > 0 && x.ItemMultiMRPId > 0 && x.WarehouseId == WarehouseId).ToList();
        //            return Request.CreateResponse(HttpStatusCode.OK, DamageitemData);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        return Request.CreateResponse(HttpStatusCode.BadRequest, ex.Message);
        //    }
        //}

        [Route("getall")]
        [HttpGet]
        public async Task<HttpResponseMessage> GetAsync(string key, int WarehouseId, string OrderType)//get all Issuances which are active for the delivery boy
        {

            using (AuthContext context = new AuthContext())
            {
                var identity = User.Identity as ClaimsIdentity;
                int compid = 0, userid = 0;

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))
                    compid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                    userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

                logger.Info("User ID : {0} , Company Id : {1}", compid, userid);
                if (OrderType == "D")
                {
                    var DamageitemData = context.DamageStockDB.Where(x => x.ItemName.Contains(key) && x.Deleted == false && x.DamageInventory > 0 && x.ItemMultiMRPId > 0 && x.WarehouseId == WarehouseId).ToList();

                    var manager = new ItemLedgerManager();
                    List<ItemClassificationDC> objItemClassificationDClist = new List<ItemClassificationDC>();
                    foreach (var data in DamageitemData)
                    {
                        ItemClassificationDC obj = new ItemClassificationDC();
                        obj.WarehouseId = data.WarehouseId;
                        obj.ItemNumber = data.ItemNumber;
                        objItemClassificationDClist.Add(obj);

                    }
                    List<ItemClassificationDC> _objItemClassificationDClist = await manager.GetItemClassificationsAsync(objItemClassificationDClist);
                    List<DamageStock> ObjDmgItemData = new List<DamageStock>();
                    foreach (var item in DamageitemData)
                    {
                        var numberData = context.itemMasters.Where(x => x.itemname.Contains(key) && x.Deleted == false && x.ItemMultiMRPId == item.ItemMultiMRPId && x.WarehouseId == item.WarehouseId && x.UnitPrice > 0).OrderByDescending(x => x.CreatedDate).FirstOrDefault();
                        if (numberData != null)
                        {
                            item.UnitPrice = numberData.UnitPrice;
                            item.MRP = numberData.MRP;
                        }
                        if (objItemClassificationDClist != null && objItemClassificationDClist.Any())
                        {

                            if (_objItemClassificationDClist.Any(x => x.ItemNumber == item.ItemNumber))
                            {
                                item.ABCClassification = _objItemClassificationDClist.Where(x => x.ItemNumber == item.ItemNumber).Select(x => x.Category).FirstOrDefault();
                            }
                            else { item.ABCClassification = "D"; }
                        }
                        else
                        {
                            item.ABCClassification = "D";
                        }

                    }

                    return Request.CreateResponse(HttpStatusCode.OK, DamageitemData);
                }
                else if (OrderType == "N")
                {
                    var DamageitemData = context.NonSellableStockDB.Where(x => x.ItemName.Contains(key) && x.IsDeleted == false && x.Inventory > 0 && x.ItemMultiMRPId > 0 && x.WarehouseId == WarehouseId).ToList();
                    var manager = new ItemLedgerManager();
                    List<ItemClassificationDC> objItemClassificationDClist = new List<ItemClassificationDC>();
                    foreach (var data in DamageitemData)
                    {
                        var numberData = context.itemMasters.Where(x => x.itemname.Contains(key) && x.Deleted == false && x.ItemMultiMRPId == data.ItemMultiMRPId && x.WarehouseId == data.WarehouseId && x.UnitPrice > 0).OrderByDescending(x => x.CreatedDate).FirstOrDefault();
                        if (numberData != null)
                        {
                            data.UnitPrice = numberData.UnitPrice;
                            data.MRP = numberData.MRP;
                        }
                        ItemClassificationDC obj = new ItemClassificationDC();
                        obj.WarehouseId = data.WarehouseId;
                        if (numberData != null)
                        {
                            obj.ItemNumber = numberData.ItemNumber;
                        }
                        objItemClassificationDClist.Add(obj);

                    }
                    //List<ItemClassificationDC> _objItemClassificationDClist = await manager.GetItemClassificationsAsync(objItemClassificationDClist);
                    //List<DamageStock> ObjDmgItemData = new List<DamageStock>();
                    //foreach (var item in DamageitemData)
                    //{
                    //    var numberData = context.ItemMultiMRPDB.Where(x => x.itemname.Contains(key) && x.Deleted == false && x.ItemMultiMRPId > 0).OrderByDescending(x => x.CreatedDate).FirstOrDefault();
                    //    if (objItemClassificationDClist != null && objItemClassificationDClist.Any())
                    //    {

                    //        if (_objItemClassificationDClist.Any(x => x.ItemNumber == numberData.ItemNumber))
                    //        {
                    //            item. = _objItemClassificationDClist.Where(x => x.ItemNumber == numberData.ItemNumber).Select(x => x.Category).FirstOrDefault();
                    //        }
                    //        else { item.ABCClassification = "D"; }
                    //    }
                    //    else
                    //    {
                    //        item.ABCClassification = "D";
                    //    }

                    //}
                    return Request.CreateResponse(HttpStatusCode.OK, DamageitemData);
                }
                else
                {
                    var NonRevenueData = context.NonRevenueOrderStocks.Where(x => x.ItemName.Contains(key) && x.IsDeleted == false && x.NonRevenueInventory > 0 && x.ItemMultiMRPId > 0 && x.WarehouseId == WarehouseId).ToList();
                    return Request.CreateResponse(HttpStatusCode.OK, NonRevenueData);
                }
            }

        }



        [Route("search")]
        [HttpGet, HttpPost]
        public dynamic search(int ItemId)
        {

            try
            {
                var identity = User.Identity as ClaimsIdentity;
                int compid = 0, userid = 0;
                int Warehouse_id = 0;
                using (AuthContext context = new AuthContext())
                {

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
                    logger.Info("User ID : {0} , Company Id : {1}", compid, userid);
                    if (ItemId != 0 && ItemId > 0)
                    {
                        var data = context.itemMasters.Where(x => x.Deleted == false && x.active == true && x.ItemId == ItemId && x.CompanyId == compid && x.WarehouseId == Warehouse_id).SingleOrDefault();

                        return data;
                    }
                    else
                    {

                        return null;
                    }
                }
            }
            catch (Exception ex)
            {

                return false;
            }
        }


        [Route("damage")]
        [HttpPost]
        [AcceptVerbs("POST")]
        /*public DamageResult Post(DamageStockDc DamageStockobj)
        {
            var identity = User.Identity as ClaimsIdentity;
            int compid = 0, userid = 0;
            // Access claims
            DamageResult msgresult = new DamageResult();
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))
                compid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);

            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
            var result = " ";
            TransactionOptions option = new TransactionOptions();
            option.IsolationLevel = IsolationLevel.RepeatableRead;
            option.Timeout = TimeSpan.FromSeconds(90);
            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, option))
            {
                using (AuthContext context = new AuthContext())
                {
                    if (DamageStockobj != null && userid > 0)
                    {
                        var warehouse = context.Warehouses.FirstOrDefault(x => x.WarehouseId == DamageStockobj.WarehouseId);

                        if (warehouse.IsStopCurrentStockTrans)
                        {
                            msgresult.Status = false;
                            msgresult.Message = "Inventory Transactions are currently disabled for this warehouse... Please try after some time";
                            return msgresult;
                        }

                        People people = context.Peoples.Where(q => q.PeopleID == userid && q.Active == true).SingleOrDefault();
                        var CurrentStock = context.DbCurrentStock.Where(x => x.ItemNumber == DamageStockobj.ItemNumber && x.WarehouseId == DamageStockobj.WarehouseId && x.ItemMultiMRPId == DamageStockobj.ItemMultiMRPId).FirstOrDefault();
                        if (DamageStockobj.Stocktype == 1) // Damage Stock
                        {
                            if (people != null && CurrentStock.CurrentInventory >= DamageStockobj.DamageInventory)
                            {

                                DamageStock dst = context.DamageStockDB.Where(x => x.ItemNumber == DamageStockobj.ItemNumber && x.WarehouseId == DamageStockobj.WarehouseId && x.ItemMultiMRPId == DamageStockobj.ItemMultiMRPId).FirstOrDefault();
                                if (dst == null)
                                {
                                    double UnitPrice = 0;
                                    double PurchasePrice = 0;

                                    int ItemId = 0;
                                    // double DamageAmount = 0;

                                    var itemmaster = context.itemMasters.Where(x => x.Number == CurrentStock.ItemNumber && x.WarehouseId == CurrentStock.WarehouseId && x.PurchasePrice > 0).FirstOrDefault();
                                    if (itemmaster != null)
                                    {
                                        UnitPrice = Math.Round(itemmaster.UnitPrice, 2);
                                        PurchasePrice = Math.Round(itemmaster.PurchasePrice, 2);
                                        ItemId = itemmaster.ItemId;
                                    }

                                    DamageStock objst = new DamageStock();
                                    objst.WarehouseId = CurrentStock.WarehouseId;
                                    objst.WarehouseName = CurrentStock.WarehouseName;
                                    objst.ItemId = ItemId;
                                    objst.MRP = CurrentStock.MRP;
                                    objst.ItemMultiMRPId = CurrentStock.ItemMultiMRPId;
                                    objst.ItemNumber = CurrentStock.ItemNumber;
                                    objst.ItemName = CurrentStock.itemname;
                                    objst.DamageInventory = 0;// DamageStockobj.DamageInventory;
                                    objst.UnitPrice = UnitPrice;
                                    objst.PurchasePrice = PurchasePrice;//we set PurchasePrice 
                                    objst.ReasonToTransfer = DamageStockobj.ReasonToTransfer;
                                    objst.CreatedDate = indianTime;
                                    objst.CompanyId = compid;

                                    context.DamageStockDB.Add(objst);
                                    context.Commit();
                                }
                                else
                                {
                                }
                                double PurchasePrices = 0;
                                double DamageAmount = 0;
                                double DamageSumQtyAmount = 0;
                                double totalSumQty = 0;
                                var DamageStockDB = context.DamageStockDB.Where(x => x.ItemNumber == CurrentStock.ItemNumber && x.WarehouseId == CurrentStock.WarehouseId && x.PurchasePrice > 0 && x.ItemMultiMRPId == CurrentStock.ItemMultiMRPId).FirstOrDefault();
                                var DamageStockSumQtydata = DamageStockDB != null ? context.DamageStockHistoryDB.Where(x => x.DamageStockId == DamageStockDB.DamageStockId && x.OdOrPoId == 0 && x.CreatedDate.Month == indianTime.Month && x.CreatedDate.Year == indianTime.Year).ToList().Sum(x => x.InwordQty) : 0;
                                PurchasePrices = Math.Round(DamageStockDB != null ? DamageStockDB.PurchasePrice : 0, 2);
                                int DmgQty = DamageStockobj.DamageInventory;
                                DamageAmount = Math.Round(DmgQty * PurchasePrices, 2);
                                DamageSumQtyAmount = Math.Round(DamageStockSumQtydata * PurchasePrices, 2);
                                totalSumQty = Math.Round(DamageAmount + DamageSumQtyAmount, 2);

                                ApprovalConfiguration approval = context.ApprovalConfigurations.Where(x => x.EntityType == CurrentStock.itemname && x.WarehouseId == CurrentStock.WarehouseId && x.Action == 0 && x.IsDeleted == false).FirstOrDefault();
                                if (approval == null)
                                {
                                    if (totalSumQty < 25000) // 50000
                                    {
                                        List<PhysicalStockUpdateRequestDc> StockTransferList = new List<PhysicalStockUpdateRequestDc>();

                                        StockTransactionHelper helper = new StockTransactionHelper();
                                        PhysicalStockUpdateRequestDc StockTransfer = new PhysicalStockUpdateRequestDc();
                                        StockTransfer.ItemMultiMRPId = CurrentStock.ItemMultiMRPId;
                                        StockTransfer.WarehouseId = CurrentStock.WarehouseId;
                                        StockTransfer.Qty = DamageStockobj.DamageInventory;
                                        StockTransfer.SourceStockType = StockTypeTableNames.CurrentStocks;// "CurrentStocks";
                                        StockTransfer.DestinationStockType = StockTypeTableNames.DamagedStock;// "DamagedStocks";
                                        StockTransfer.StockTransferType = StockTransferTypeName.DamagedStocks;
                                        StockTransfer.Reason = "Stock transfer to Damagestock Due to: " + DamageStockobj.ReasonToTransfer;
                                        StockTransferList.Add(StockTransfer);
                                        bool isupdated = helper.TransferBetweenPhysicalStocks(StockTransfer, userid, context, scope);

                                        BatchMasterManager batchMasterManager = new BatchMasterManager();
                                        bool isSuccess = batchMasterManager.MoveDirectBatchItemInSameBatch("C", "D", DamageStockobj.DamageInventory, DamageStockobj.StockBatchMasterId, DamageStockobj.ItemMultiMRPId, DamageStockobj.WarehouseId, context, userid);

                                        if (isupdated && isSuccess)
                                        {
                                            scope.Complete();
                                            msgresult.Status = true;
                                            msgresult.Message = "Transaction Saved Successfully";


                                            #region Insert in FIFO
                                            if (ConfigurationManager.AppSettings["LiveFIFO"] == "1")
                                            {
                                                List<OutDc> items = StockTransferList.Where(x => x.Qty > 0).Select(x => new OutDc
                                                {
                                                    ItemMultiMrpId = x.ItemMultiMRPId,
                                                    WarehouseId = x.WarehouseId,
                                                    Destination = "Damage Out",
                                                    CreatedDate = indianTime,
                                                    ObjectId = 0,
                                                    Qty = x.Qty,
                                                    SellingPrice = 0,
                                                }).ToList();
                                                foreach (var it in items)
                                                {
                                                    RabbitMqHelper rabbitMqHelper = new RabbitMqHelper();
                                                    rabbitMqHelper.Publish("DamageOut", it);
                                                }
                                            }
                                            #endregion


                                        }
                                        else
                                        {
                                            scope.Dispose();
                                            msgresult.Status = false;
                                            if (!isupdated)
                                                msgresult.Message = "one of the stock not available";
                                            if (!isSuccess)
                                                msgresult.Message = "currently stock movement in process for same ItemMultiMRP and Warehouse. Please try after some time.";
                                        }


                                    }

                                    else
                                    {
                                        //var Damage = context.DamageRequests.Where(x => x.itemName == dmrqst.itemName).FirstOrDefault();
                                        // ApprovalConfiguration approval = context.ApprovalConfigurations.Where(x => x.EntityType == CurrentStock.itemname && x.WarehouseId == CurrentStock.WarehouseId && x.Action == 0 && x.IsDeleted == false).FirstOrDefault();
                                        if (approval == null)
                                        {
                                            DamageRequest dmrqst = new DamageRequest();
                                            dmrqst.itemName = CurrentStock.itemname;
                                            dmrqst.ItemMultiMRPId = CurrentStock.ItemMultiMRPId;
                                            dmrqst.WarehouseId = CurrentStock.WarehouseId;
                                            dmrqst.Qty = DmgQty;
                                            dmrqst.CreatedBy = userid;
                                            dmrqst.CreatedDate = DateTime.Now;
                                            dmrqst.IsDeleted = false;
                                            dmrqst.StockBatchMasterId = DamageStockobj.StockBatchMasterId;
                                            context.DamageRequests.Add(dmrqst);
                                            context.Commit();


                                            //var DamageRequest = context.DamageRequests.Where(x => x.itemName == dmrqst.itemName && x.Qty == dmrqst.Qty && x.IsDeleted == false).FirstOrDefault();

                                            string query = string.Format("select  p.DisplayName,p.PeopleID from People p where exists (select u.Id from AspNetUsers u inner join AspNetUserRoles ur on u.Id=ur.UserId and p.Email=u.Email inner join AspNetRoles r on ur.RoleId=r.Id and r.name ='{0}') and p.Active = 1 ",
                                                    "Supplier Payment Approver");

                                            DamageAprprover damgeP = context.Database.SqlQuery<DamageAprprover>(query).FirstOrDefault();
                                            People peopleName = context.Peoples.Where(q => q.PeopleID == damgeP.PeopleId).FirstOrDefault();

                                            var DamageItemList = context.ApprovalConfigurations.Where(x => x.EntityId == dmrqst.Id).FirstOrDefault();
                                            if (DamageItemList == null)
                                            {
                                                ApprovalConfiguration dmg = new ApprovalConfiguration();
                                                dmg.ApproverId = damgeP.PeopleId;
                                                dmg.EntityId = dmrqst.Id;
                                                dmg.EntityType = dmrqst.itemName;
                                                dmg.WarehouseId = dmrqst.WarehouseId;
                                                dmg.CreatedBy = userid;
                                                dmg.IsDeleted = false;
                                                dmg.CreatedDate = DateTime.Now;
                                                dmg.updatedDate = DateTime.Now;
                                                dmg.ApprovedDate = null;
                                                context.ApprovalConfigurations.Add(dmg);

                                            }
                                            //  context.Commit();

                                            if (context.Commit() > 0)
                                            {
                                                scope.Complete();
                                                msgresult.Status = true;
                                                msgresult.Message = "Transaction Saved Successfully ,Sent For Approval";
                                            }
                                            else
                                            {
                                                scope.Dispose();
                                                msgresult.Status = false;
                                                msgresult.Message = "one of the stock not available";
                                            }
                                        }
                                        else
                                        {
                                            //scope.Dispose();
                                            msgresult.Status = true;
                                            msgresult.Message = "Item Approval Pending";
                                        }
                                    }
                                }
                                else
                                {
                                    //scope.Dispose();
                                    msgresult.Status = true;
                                    msgresult.Message = "Item Approval Pending";
                                }
                            }
                        }
                        if (DamageStockobj.Stocktype == 2)
                        {
                            if (people != null && CurrentStock.CurrentInventory >= DamageStockobj.DamageInventory)
                            {
                                double PurchasePrices = 0;
                                double ClearanceAmount = 0;
                                double ClearanceSumQtyAmount = 0; //needed
                                double totalSumQty = 0;


                                var ClearanceStockDb = context.ClearanceStockNewDB.Where(x => x.WarehouseId == CurrentStock.WarehouseId && x.ItemMultiMRPId == CurrentStock.ItemMultiMRPId).FirstOrDefault(); // needed where clause
                                                                                                                                                                                                              // var ClearanceStockSumQtydata = ClearanceStockDb != null ? context.ClearanceStockHistoryDB.Where(x => x.ClearanceStockId == ClearanceStockDb.ClearanceStockId && x.OdOrPoId == 0 && x.CreatedDate.Month == indianTime.Month && x.CreatedDate.Year == indianTime.Year).ToList().Sum(x => x.InwordQty) : 0;

                                // PurchasePrices = Math.Round(ClearanceStockDb != null ? ClearanceStockDb.PurchasePrice : 0, 2);


                                int DmgQty = DamageStockobj.DamageInventory;


                                ClearanceAmount = Math.Round(DmgQty * PurchasePrices, 2); // purchase price needed

                                // ClearanceSumQtyAmount = Math.Round(ClearanceStockSumQtydata * PurchasePrices, 2);

                                totalSumQty = Math.Round(ClearanceAmount + ClearanceSumQtyAmount, 2);

                                ApprovalConfiguration approval = context.ApprovalConfigurations.Where(x => x.EntityType == CurrentStock.itemname && x.WarehouseId == CurrentStock.WarehouseId && x.Action == 0 && x.IsDeleted == false).FirstOrDefault();
                                if (approval == null)
                                {
                                    if (totalSumQty < 25000) // 50000
                                    {
                                    }
                                }
                            }
                        }
                    }
                    return msgresult;

                    #region old code

                    //try
                    //{
                    //    var identity = User.Identity as ClaimsIdentity;
                    //    int compid = 0, userid = 0;
                    //    // Access claims
                    //    if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))
                    //        compid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);

                    //    if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                    //        userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
                    //    using (AuthContext context = new AuthContext())
                    //    {
                    //        using (var dbContextTransaction = context.Database.BeginTransaction())
                    //        {
                    //            if (DamageStockobj != null)
                    //            {

                    //                People people = context.Peoples.Where(q => q.PeopleID == userid && q.Active == true).SingleOrDefault();
                    //                var CurrentStock = context.DbCurrentStock.Where(x => x.ItemNumber == DamageStockobj.ItemNumber && x.WarehouseId == DamageStockobj.WarehouseId && x.ItemMultiMRPId == DamageStockobj.ItemMultiMRPId).FirstOrDefault();
                    //                if (people != null && CurrentStock.CurrentInventory >= DamageStockobj.DamageInventory)
                    //                {
                    //                    var itemmaster = context.itemMasters.Where(x => x.Number == CurrentStock.ItemNumber && x.WarehouseId == CurrentStock.WarehouseId && x.PurchasePrice > 0).FirstOrDefault();
                    //                    if (itemmaster != null)
                    //                    {
                    //                        DamageStock dst = context.DamageStockDB.Where(x => x.ItemNumber == DamageStockobj.ItemNumber && x.WarehouseId == DamageStockobj.WarehouseId && x.ItemMultiMRPId == DamageStockobj.ItemMultiMRPId).FirstOrDefault();
                    //                        if (dst == null)
                    //                        {
                    //                            DamageStock objst = new DamageStock();
                    //                            objst.WarehouseId = CurrentStock.WarehouseId;
                    //                            objst.WarehouseName = CurrentStock.WarehouseName;
                    //                            objst.ItemId = itemmaster.ItemId;
                    //                            objst.MRP = CurrentStock.MRP;
                    //                            objst.ItemMultiMRPId = CurrentStock.ItemMultiMRPId;
                    //                            objst.ItemNumber = CurrentStock.ItemNumber;
                    //                            objst.ItemName = CurrentStock.itemname;
                    //                            objst.DamageInventory = DamageStockobj.DamageInventory;
                    //                            double UnitPrice = Math.Round(itemmaster.UnitPrice, 2);
                    //                            objst.UnitPrice = UnitPrice;
                    //                            double PurchasePrice = Math.Round(itemmaster.PurchasePrice, 2);
                    //                            objst.PurchasePrice = PurchasePrice;//we set PurchasePrice 
                    //                            objst.ReasonToTransfer = DamageStockobj.ReasonToTransfer;
                    //                            objst.CreatedDate = indianTime;
                    //                            objst.CompanyId = compid;
                    //                            context.DamageStockDB.Add(objst);
                    //                            context.Commit();
                    //                            CurrentStockHistory Oss = new CurrentStockHistory();
                    //                            Oss.StockId = CurrentStock.StockId;
                    //                            Oss.ItemNumber = CurrentStock.ItemNumber;
                    //                            Oss.itemname = CurrentStock.itemname;
                    //                            Oss.CurrentInventory = CurrentStock.CurrentInventory;
                    //                            Oss.DamageInventoryOut = Convert.ToInt32(DamageStockobj.DamageInventory);
                    //                            Oss.TotalInventory = Convert.ToInt32(CurrentStock.CurrentInventory - DamageStockobj.DamageInventory);
                    //                            Oss.WarehouseName = CurrentStock.WarehouseName;
                    //                            Oss.Warehouseid = CurrentStock.WarehouseId;
                    //                            Oss.CompanyId = CurrentStock.CompanyId;
                    //                            Oss.CreationDate = indianTime;
                    //                            Oss.ItemMultiMRPId = CurrentStock.ItemMultiMRPId;
                    //                            Oss.ManualReason = " Stock transfer to Damagestock Due to: " + DamageStockobj.ReasonToTransfer;
                    //                            Oss.userid = people.PeopleID;
                    //                            Oss.UserName = people.DisplayName;
                    //                            context.CurrentStockHistoryDb.Add(Oss);

                    //                            CurrentStock.CurrentInventory = CurrentStock.CurrentInventory - DamageStockobj.DamageInventory;
                    //                            CurrentStock.UpdatedDate = indianTime;
                    //                            context.Entry(CurrentStock).State = EntityState.Modified;

                    //                            var DSH = new DamageStockHistory();
                    //                            DSH.CompanyId = compid;
                    //                            DSH.CreatedDate = indianTime;
                    //                            DSH.DamageStockId = objst.DamageStockId;
                    //                            DSH.Deleted = false;
                    //                            DSH.InwordQty = DamageStockobj.DamageInventory;
                    //                            DSH.DamageInventory = objst.DamageInventory;
                    //                            DSH.itemBaseName = objst.itemBaseName;
                    //                            DSH.ItemId = objst.ItemId;
                    //                            DSH.ItemMultiMRPId = objst.ItemMultiMRPId;
                    //                            DSH.ItemName = objst.ItemName;
                    //                            DSH.ItemNumber = objst.ItemNumber;
                    //                            DSH.MRP = objst.MRP;
                    //                            DSH.OutwordQty = 0;
                    //                            DSH.ReasonToTransfer = "Stock Transferred from Current Stock:" + objst.ReasonToTransfer;
                    //                            DSH.UnitofQuantity = objst.UnitofQuantity;
                    //                            DSH.UOM = objst.UOM;
                    //                            DSH.UpdatedDate = indianTime;
                    //                            DSH.WarehouseId = objst.WarehouseId;
                    //                            DSH.WarehouseName = objst.WarehouseName;
                    //                            DSH.PurchasePrice = itemmaster.PurchasePrice;
                    //                            DSH.UnitPrice = itemmaster.UnitPrice;
                    //                            DSH.CreatedBy = people.PeopleID;
                    //                            DSH.UserName = people.DisplayName;
                    //                            context.DamageStockHistoryDB.Add(DSH);
                    //                        }
                    //                        else
                    //                        {
                    //                            dst.DamageInventory = dst.DamageInventory + DamageStockobj.DamageInventory;
                    //                            dst.UpdatedDate = indianTime;
                    //                            context.Entry(dst).State = EntityState.Modified;

                    //                            CurrentStockHistory Oss = new CurrentStockHistory();
                    //                            Oss.StockId = CurrentStock.StockId;
                    //                            Oss.ItemNumber = CurrentStock.ItemNumber;
                    //                            Oss.itemname = CurrentStock.itemname;
                    //                            Oss.CurrentInventory = CurrentStock.CurrentInventory;
                    //                            Oss.DamageInventoryOut = Convert.ToInt32(DamageStockobj.DamageInventory);
                    //                            Oss.TotalInventory = Convert.ToInt32(CurrentStock.CurrentInventory - DamageStockobj.DamageInventory);
                    //                            Oss.WarehouseName = CurrentStock.WarehouseName;
                    //                            Oss.Warehouseid = CurrentStock.WarehouseId;
                    //                            Oss.CompanyId = compid;
                    //                            Oss.ItemMultiMRPId = CurrentStock.ItemMultiMRPId;
                    //                            Oss.ManualReason = " Stock transfer to Damagestock Due to: " + DamageStockobj.ReasonToTransfer;
                    //                            Oss.CreationDate = indianTime;
                    //                            Oss.userid = people.PeopleID;
                    //                            Oss.UserName = people.DisplayName;
                    //                            context.CurrentStockHistoryDb.Add(Oss);

                    //                            CurrentStock.CurrentInventory = CurrentStock.CurrentInventory - DamageStockobj.DamageInventory;
                    //                            CurrentStock.UpdatedDate = indianTime;
                    //                            context.Entry(CurrentStock).State = EntityState.Modified;

                    //                            var DSH = new DamageStockHistory();
                    //                            DSH.CompanyId = compid;
                    //                            DSH.CreatedDate = indianTime;
                    //                            DSH.DamageInventory = dst.DamageInventory;
                    //                            DSH.DamageStockId = dst.DamageStockId;
                    //                            DSH.Deleted = false;
                    //                            DSH.InwordQty = DamageStockobj.DamageInventory;
                    //                            DSH.itemBaseName = dst.itemBaseName;
                    //                            DSH.ItemId = dst.ItemId;
                    //                            DSH.ItemMultiMRPId = dst.ItemMultiMRPId;
                    //                            DSH.ItemName = dst.ItemName;
                    //                            DSH.ItemNumber = dst.ItemNumber;
                    //                            DSH.MRP = dst.MRP;
                    //                            DSH.OutwordQty = 0;
                    //                            DSH.ReasonToTransfer = "Stock Transferred from Current Stock:" + DamageStockobj.ReasonToTransfer;
                    //                            DSH.UnitofQuantity = dst.UnitofQuantity;
                    //                            DSH.UOM = dst.UOM;
                    //                            DSH.UpdatedDate = indianTime;
                    //                            DSH.WarehouseId = dst.WarehouseId;
                    //                            DSH.WarehouseName = dst.WarehouseName;
                    //                            DSH.PurchasePrice = itemmaster.PurchasePrice;
                    //                            DSH.UnitPrice = itemmaster.UnitPrice;
                    //                            DSH.CreatedBy = people.PeopleID;
                    //                            DSH.UserName = people.DisplayName;
                    //                            context.DamageStockHistoryDB.Add(DSH);
                    //                        }
                    //                    }
                    //                    if (context.Commit() > 0) { dbContextTransaction.Commit(); } else { dbContextTransaction.Rollback(); }
                    //                }
                    //            }
                    //        }
                    //    }
                    //    return DamageStockobj;
                    //}
                    //catch (Exception ex)
                    //{
                    //    return null;
                    //}
                    #endregion

                }
            }
        }
*/
        ///////////////new code/////////////


        //[Route("filtre")]
        //[HttpPost]
        //public dynamic get(DBOYinfo1 DBI)
        //{

        //    try
        //    {
        //        var identity = User.Identity as ClaimsIdentity;
        //        int compid = 0, userid = 0;

        //        foreach (Claim claim in identity.Claims)
        //        {
        //            if (claim.Type == "compid")
        //            {
        //                compid = int.Parse(claim.Value);
        //            }
        //            if (claim.Type == "userid")
        //            {
        //                userid = int.Parse(claim.Value);
        //            }
        //        }
        //        List<CurrentStock> returnlist = new List<CurrentStock>();
        //        using (AuthContext context = new AuthContext())
        //        {
        //            var StockIds = DBI.ids.Select(x => x.id).ToList();
        //            var Warehouseid = DBI.Warehouseid;
        //            //var lst = context.AddDamageStock(i.id, DBI.Warehouseid, compid);
        //            if (Warehouseid > 0)
        //            {
        //                returnlist = context.DbCurrentStock.Where(x => x.Deleted == false && StockIds.Contains(x.StockId) && x.CurrentInventory > 0 && x.WarehouseId == Warehouseid).ToList();
        //            }
        //        }
        //        return returnlist;
        //    }
        //    catch (Exception ex)
        //    {

        //        return null;
        //    }
        //}



        [Route("filtre")]
        /*[HttpPost]*/
        public HttpResponseMessage get(DBOYinfo1 DBI)
        {

            return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "");
            //try
            //{
            //    var identity = User.Identity as ClaimsIdentity;
            //    int compid = 0, userid = 0;

            //    foreach (Claim claim in identity.Claims)
            //    {
            //        if (claim.Type == "compid")
            //        {
            //            compid = int.Parse(claim.Value);
            //        }
            //        if (claim.Type == "userid")
            //        {
            //            userid = int.Parse(claim.Value);
            //        }
            //    }



            //    string Stockid = String.Join(",", DBI.ids);
            //    List<DamageStockItemDc> objDamageStockItemDc = new List<DamageStockItemDc>();


            //    if (DBI.Warehouseid > 0)
            //    {


            //        using (AuthContext context = new AuthContext())
            //        {
            //            var WarehouseidParam = new SqlParameter
            //            {
            //                ParameterName = "Warehouseid",
            //                Value = DBI.Warehouseid
            //            };



            //            var StockIdParam = new SqlParameter
            //            {
            //                ParameterName = "Stockid",
            //                Value = Stockid

            //            };


            //            objDamageStockItemDc = context.Database.SqlQuery<DamageStockItemDc>("GetDamageStockItemNew @Stockid ,@Warehouseid ", StockIdParam, WarehouseidParam
            //               ).ToList();
            //        }
            //    }
            //    return Request.CreateResponse(HttpStatusCode.OK, objDamageStockItemDc);

            //}
            //catch (Exception ex)
            //{
            //    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex.Message.ToString());

            //}
        }



        [Route("getExportDSH")]
        [HttpPost]
        public HttpResponseMessage getExportDSH(DBOYinfo1 DBI)
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
                string Stockid = String.Join(",", DBI.ids);
                List<DamageStockItemDc> objDamageStockItemDc = new List<DamageStockItemDc>();

                if (DBI.Warehouseid > 0)
                {
                    using (AuthContext context = new AuthContext())
                    {
                        var WarehouseidParam = new SqlParameter
                        {
                            ParameterName = "Warehouseid",
                            Value = DBI.Warehouseid
                        };

                        var StockIdParam = new SqlParameter
                        {
                            ParameterName = "Stockid",
                            Value = Stockid

                        };

                        objDamageStockItemDc = context.Database.SqlQuery<DamageStockItemDc>("GetDamageStockItem @Stockid ,@Warehouseid ", StockIdParam, WarehouseidParam
                           ).ToList();
                    }
                }
                return Request.CreateResponse(HttpStatusCode.OK, objDamageStockItemDc);
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex.Message.ToString());
            }
        }



        public class DBOYinfo1
        {
            //public List<dbinf> ids { get; set; }
            public int Warehouseid { get; set; }

            public List<int> ids { get; set; }
        }
        public class dbinf
        {
            public int id { get; set; }


        }

        public class DBOYinfoParam
        {
            public List<int> id { get; set; }
            public int Warehouseid { get; set; }
        }

        [HttpGet]
        [Route("GetWarehouseCustomer/{warehouseId}/{key}")]
        public List<Customer> GetWarehouseCashCustomer(int warehouseId, string key)
        {
            using (var context = new AuthContext())
            {
                List<Customer> customer = null; ;
                if (key != null && key != "undefined")
                {
                    customer = context.Customers.Where(x => x.Skcode.ToLower().Contains(key.ToLower()) && x.Active == true && x.Deleted == false && x.Warehouseid == warehouseId).ToList();
                }

                return customer;
            }
        }


        [Route("Custall")]
        [HttpGet]
        public HttpResponseMessage Custall(int WarehouseId)//get all Issuances which are active for the delivery boy
        {

            var identity = User.Identity as ClaimsIdentity;
            int compid = 0, userid = 0;

            var CustList = new List<ClustCustomerDTO>();
            using (AuthContext context = new AuthContext())
            {

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))
                    compid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                    userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);


                string query = "select Name,CustomerId,Skcode,Warehouseid,ShopName,City from Customers with(nolock) where Deleted=0 and Active=1 and Warehouseid=" + WarehouseId;
                CustList = context.Database.SqlQuery<ClustCustomerDTO>(query).ToList();

                return Request.CreateResponse(HttpStatusCode.OK, CustList);
            }

        }



        public void InsertDatainDamageStockHistory(DamageStock obj, int inward, int outword, int userid, AuthContext con)
        {
            var DSH = new DamageStockHistory();
            DSH.CompanyId = obj.CompanyId;
            DSH.CreatedDate = indianTime;
            DSH.DamageInventory = obj.DamageInventory;
            DSH.DamageStockId = obj.DamageStockId;
            DSH.Deleted = obj.Deleted;
            DSH.InwordQty = inward;
            DSH.itemBaseName = obj.itemBaseName;
            DSH.ItemId = obj.ItemId;
            DSH.ItemMultiMRPId = obj.ItemMultiMRPId;
            DSH.ItemName = obj.ItemName;
            DSH.ItemNumber = obj.ItemNumber;
            DSH.MRP = obj.MRP;
            DSH.OutwordQty = outword;
            DSH.ReasonToTransfer = obj.ReasonToTransfer;
            DSH.UnitofQuantity = obj.UnitofQuantity;
            DSH.UnitPrice = obj.UnitPrice;
            DSH.UOM = obj.UOM;
            DSH.UpdatedDate = indianTime;
            DSH.WarehouseId = obj.WarehouseId;
            DSH.WarehouseName = obj.WarehouseName;
            DSH.CreatedBy = userid;
            con.DamageStockHistoryDB.Add(DSH);
            con.Commit();
        }


        #region
        [Route("GetDamageHistory")]
        public async System.Threading.Tasks.Task<PaggingData_DamageStockHistory> GetDAsync(int list, int page, string ItemNumber, int WarehouseId, int StockId)
        {

            logger.Info("start OrderMaster: ");
            using (AuthContext context = new AuthContext())
            {
                try
                {

                    var identity = User.Identity as ClaimsIdentity;
                    int compid = 0, userid = 0;
                    if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))
                        compid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);

                    if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                        userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

                    int CompanyId = compid;
                    if (CompanyId == 0)
                    {
                        throw new ArgumentNullException("item");
                    }
                    logger.Info("User ID : {0} , Company Id : {1}", compid, userid);
                    List<DamageStockHistory> newdata = new List<DamageStockHistory>();
                    var listOrders = context.DamageStockHistoryDB.Where(x => x.Deleted == false && x.ItemNumber == ItemNumber && x.WarehouseId == WarehouseId && x.DamageStockId == StockId).OrderByDescending(x => x.CreatedDate).Skip((page - 1) * list).Take(list).ToList();
                    for (var i = 0; i < listOrders.ToList().Count(); i++)
                    {
                        int Userid = listOrders[i].CreatedBy;
                        var DisplayName = context.Peoples.Where(x => x.PeopleID == Userid).Select(x => x.DisplayName).FirstOrDefault();
                        if (DisplayName != null)
                        {
                            listOrders[i].UserName = DisplayName;
                        }
                        else
                        {
                            listOrders[i].UserName = null;
                        }
                        int warehouseId = listOrders[i].WarehouseId;
                        var warehouesname = context.Warehouses.Where(x => x.WarehouseId == warehouseId).Select(x => x.WarehouseName).FirstOrDefault();
                        if (warehouesname != null)
                        {
                            listOrders[i].WarehouseName = warehouesname;
                        }
                        else
                        {
                            listOrders[i].WarehouseName = null;
                        }
                    }
                    newdata = listOrders;
                    PaggingData_DamageStockHistory obj = new PaggingData_DamageStockHistory();
                    obj.total_count = context.DamageStockHistoryDB.Where(x => x.Deleted == false && x.ItemNumber == ItemNumber && x.WarehouseId == WarehouseId && x.DamageStockId == StockId).Count();
                    var manager = new ItemLedgerManager();
                    List<ItemClassificationDC> objItemClassificationDClist = new List<ItemClassificationDC>();
                    foreach (var dmgstockdata in newdata)
                    {
                        ItemClassificationDC objItemClassificationDC = new ItemClassificationDC();
                        objItemClassificationDC.WarehouseId = dmgstockdata.WarehouseId;
                        objItemClassificationDC.ItemNumber = dmgstockdata.ItemNumber;
                        objItemClassificationDClist.Add(objItemClassificationDC);

                    }
                    List<ItemClassificationDC> _objItemClassificationDClist = await manager.GetItemClassificationsAsync(objItemClassificationDClist);
                    List<DamageStock> ObjDmgItemData = new List<DamageStock>();
                    foreach (var item in newdata)
                    {
                        if (_objItemClassificationDClist != null && _objItemClassificationDClist.Any())
                        {

                            if (_objItemClassificationDClist.Any(x => x.ItemNumber == item.ItemNumber))
                            {
                                item.ABCClassification = _objItemClassificationDClist.Where(x => x.ItemNumber == item.ItemNumber).Select(x => x.Category).FirstOrDefault();
                            }
                            else { item.ABCClassification = "D"; }
                        }
                        else
                        {
                            item.ABCClassification = "D";
                        }

                    }
                    obj.ordermaster = newdata;

                    return obj;
                    //logger.Info("End OrderMaster: ");
                    //return ass;
                }
                catch (Exception ex)
                {
                    logger.Error("Error in OrderMaster " + ex.Message);
                    logger.Info("End  OrderMaster: ");
                    return null;
                }
            }
        }
        #endregion




        [Route("GetDamageHistoryAll")]
        [HttpPost]
        public async System.Threading.Tasks.Task<PaggingData_DamageStockHistory> GetDamageHistoryAll(String WarehouseId)
        {

            logger.Info("start OrderMaster: ");
            using (AuthContext context = new AuthContext())
            {
                try
                {
                    // PaggingDatastock data = new PaggingDatastock();
                    PaggingData_DamageStockHistory obj = new PaggingData_DamageStockHistory();
                    var WarehouseIds = WarehouseId.Split(',').Select(Int32.Parse).ToList();
                    var identity = User.Identity as ClaimsIdentity;
                    int compid = 0, userid = 0;
                    if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))
                        compid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);

                    if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                        userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

                    int CompanyId = compid;
                    if (CompanyId == 0)
                    {
                        throw new ArgumentNullException("item");
                    }
                    logger.Info("User ID : {0} , Company Id : {1}", compid, userid);
                    List<DamageStockHistory> newdata = new List<DamageStockHistory>();
                    // Added Join By Anoop 4/3/2021
                    string sqlquery = "SELECT DSH.DamageInventory,DSH.InwordQty, DSH.OutwordQty, DSH.DamageStockId,DSH.Id,DSH.CompanyId,DSH.WarehouseId,DSH.ItemId,DSH.ItemNumber,DSH.ItemName, DSH.PurchasePrice,DSH.ReasonToTransfer,DSH.OdOrPoId, DSH.Deleted,DS.UnitPrice, DSH.CreatedDate, DSH.UpdatedDate, DSH.itemBaseName, DSH.ItemMultiMRPId, DSH.MRP, DSH.UnitofQuantity, DSH.UOM,DSH.CreatedBy,DSH.UpdatedDate as Date  from DamageStockHistories DSH left join DamageStocks DS on DSH.DamageStockId = DS.DamageStockId where DSH.Deleted = 0 and DSH.WarehouseId IN (" + WarehouseId + ")";
                    var listOrders = context.Database.SqlQuery<DamageStockHistoryAll>(sqlquery).OrderByDescending(x => x.CreatedDate).ToList();

                    // var listOrders = context.DamageStockHistoryDB.Where(x => WarehouseIds.Contains(x.WarehouseId) &&  x.Deleted == false).OrderByDescending(x => x.CreatedDate).ToList();

                    // var listOrders = context.

                    // List<DamageStock> ObjDmgItemData = new List<DamageStock>();
                    // var damagest = context.DamageStockDB.Where(x => x.Deleted == false && x.CompanyId == compid && WarehouseIds.Contains(x.WarehouseId)).OrderByDescending(x => x.DamageStockId).ToList();

                    for (var i = 0; i < listOrders.ToList().Count(); i++)
                    {
                        int Userid = listOrders[i].CreatedBy;
                        var DisplayName = context.Peoples.Where(x => x.PeopleID == Userid).Select(x => x.DisplayName).FirstOrDefault();
                        if (DisplayName != null)
                        {
                            listOrders[i].UserName = DisplayName;
                        }
                        else
                        {
                            listOrders[i].UserName = null;
                        }
                        int warehouseId = listOrders[i].WarehouseId;
                        var warehouesname = context.Warehouses.Where(x => x.WarehouseId == warehouseId).Select(x => x.WarehouseName).FirstOrDefault();
                        if (warehouesname != null)
                        {
                            listOrders[i].WarehouseName = warehouesname;
                        }
                        else
                        {
                            listOrders[i].WarehouseName = null;
                        }
                    }


                    // newdata = listOrders;
                    //PaggingData_DamageStockHistory obj = new PaggingData_DamageStockHistory();
                    // obj.total_count = context.DamageStockHistoryDB.Where(x => WarehouseIds.Contains(x.WarehouseId) && x.Deleted == false).Count();

                    //obj.ordermasterHistory = newdata;
                    obj.ordermasterHistory = listOrders;
                    //obj.dmhistory = damagest;
                    return obj;
                    //logger.Info("End OrderMaster: ");
                    //return ass;
                }
                catch (Exception ex)
                {
                    logger.Error("Error in OrderMaster " + ex.Message);
                    logger.Info("End  OrderMaster: ");
                    return null;
                }
            }
        }




        [Route("PostWarehousebasedDitem")]
        [HttpPost]
        //[AllowAnonymous]

        public async Task<List<DamageStock>> PostWarehousebasedDitem(string WarehouseId)
        {
            logger.Info("start current stock: ");
            string WId = "";
            using (AuthContext context = new AuthContext())
            {
                List<DamageStock> ass = new List<DamageStock>();
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
                    //Change
                    var WarehouseIds = WarehouseId.Split(',').Select(Int32.Parse).ToList();
                    //foreach (var item in WarehouseId)
                    //{
                    //    WId = String.Join(",", item.Wwarehouseid);
                    //    int int_WId = Int32.Parse(WId);
                    //    //int int_WId = item;
                    //Warehouse_id = WarehouseIds;
                    logger.Info("User ID : {0} , Company Id : {1}", compid, userid);
                    //ass = context.GetAllCurrentStockWid(CompanyId, Warehouse_id).ToList();
                    if (WarehouseIds.Count > 0)
                    {
                        ass = context.DamageStockDB.Where(x => WarehouseIds.Contains(x.WarehouseId) && x.Deleted == false && x.ItemMultiMRPId > 0).ToList();
                        //string query = "select * from DamageStocks with(nolock) where Deleted=0 and ItemMultiMRPId>0 and  WarehouseId in ('" + string.Join("','", WarehouseId) + "')";
                        //ass = context.Database.SqlQuery<DamageStock>(query).ToList();
                    }

                    //   }

                    //Warehouse_id = WarehouseId;
                    //logger.Info("User ID : {0} , Company Id : {1}", compid, userid);
                    ////ass = context.GetAllCurrentStockWid(CompanyId, Warehouse_id).ToList();
                    //if (WarehouseId > 0)
                    //{
                    //    string query = "select * from DamageStocks with(nolock) where Deleted=0 and ItemMultiMRPId>0 and  WarehouseId=" + Warehouse_id;
                    //    ass = context.Database.SqlQuery<DamageStock>(query).ToList();
                    //}

                    logger.Info("End  current stock: ");
                    // return Request.CreateResponse(HttpStatusCode.OK, ass); 
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


        [Authorize]
        [Route("GetPendingDamage")]
        [HttpPost]
        public PaggingData GetPendingDamage(FilterOrderDataDTOs filterOrderData)
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

                    string sqlquery = "Select CS.ItemNumber,DR.WarehouseId,DR.ItemMultiMRPId,DS.ReasonToTransfer,AC.Id,AC.EntityId,DR.Qty,DR.itemName,cs.WarehouseName,sum(DR.Qty * DS.PurchasePrice ) as Amount,AC.Action,DR.CreatedDate,StockBatchMasterId,BM.MFGDate,BM.ExpiryDate,BM.BatchCode"
                                     + "   from DamageRequests DR join ApprovalConfigurations AC on AC.EntityId = DR.Id join CurrentStocks CS"
                                     + "   on CS.ItemMultiMRPId = DR.ItemMultiMRPId and"
                                     + "    cs.WarehouseId = DR.WarehouseId join DamageStocks DS on Ds.ItemMultiMRPId = DR.ItemMultiMRPId and DS.WarehouseId = DR.WarehouseId"
                                     + "    join BatchMasters BM on BM.Id=DR.StockBatchMasterId"
                                     + "  where AC.Action = 0  and AC.IsDeleted = 0 and DR.IsDeleted = 0 ";
                    if (filterOrderData.WarehouesId > 0)
                    {
                        sqlquery += " and DR.WarehouseId = " + filterOrderData.WarehouesId + " ";
                    }
                    sqlquery += " group by CS.ItemNumber,DR.WarehouseId,DR.ItemMultiMRPId,DS.ReasonToTransfer,AC.Id,AC.EntityId,DR.Qty,DR.itemName,cs.WarehouseName,AC.Action,DR.CreatedDate,StockBatchMasterId,BM.MFGDate,BM.ExpiryDate,BM.BatchCode ";
                    List<PurchaseOrderMasterDTOs> PRSts = db.Database.SqlQuery<PurchaseOrderMasterDTOs>(sqlquery).OrderByDescending(a => a.Id).ToList();
                    var PRStsdata = PRSts.Skip(skip).Take(take).ToList();
                    int dataCount = PRSts.Count();
                    if (PRSts != null && PRSts.Any())
                    {
                        paggingData.total_count = dataCount;
                    }
                    paggingData.ordermaster = PRStsdata;
                    return paggingData;
                }
            }
            catch (Exception ex)
            {
                return null;
            }
        }


        [Authorize]
        [Route("GetApprovedDamage")]
        [HttpPost]
        public PaggingData GetApprovedDamage(FilterOrderDataDTOs filterOrderData)
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

                    string sqlquery = "Select CS.ItemNumber,DR.WarehouseId,DR.ItemMultiMRPId,DS.ReasonToTransfer,AC.Id,AC.EntityId,DR.Qty,DR.itemName,cs.WarehouseName,sum(DR.Qty * DS.PurchasePrice ) as Amount,AC.Action,DR.CreatedDate,StockBatchMasterId,BM.MFGDate,BM.ExpiryDate,BM.BatchCode"
                                     + "   from DamageRequests DR join ApprovalConfigurations AC on AC.EntityId = DR.Id join CurrentStocks CS"
                                     + "   on CS.ItemMultiMRPId = DR.ItemMultiMRPId and"
                                     + "    cs.WarehouseId = DR.WarehouseId join DamageStocks DS on Ds.ItemMultiMRPId = DR.ItemMultiMRPId and DS.WarehouseId = DR.WarehouseId"
                                     + "    join BatchMasters BM on BM.Id=DR.StockBatchMasterId"
                                     + "  where AC.Action = 1  and AC.IsDeleted = 0 and DR.IsDeleted = 0 and DR.WarehouseId = " + filterOrderData.WarehouesId + " group by CS.ItemNumber,DR.WarehouseId,DR.ItemMultiMRPId,DS.ReasonToTransfer,AC.Id,AC.EntityId,DR.Qty,DR.itemName,cs.WarehouseName,AC.Action,DR.CreatedDate,StockBatchMasterId,BM.MFGDate,BM.ExpiryDate,BM.BatchCode "; List<PurchaseOrderMasterDTOs> PRSts = db.Database.SqlQuery<PurchaseOrderMasterDTOs>(sqlquery).OrderByDescending(a => a.Id).ToList();
                    var PRStsdata = PRSts.Skip(skip).Take(take).ToList();
                    int dataCount = PRSts.Count();
                    if (PRSts != null && PRSts.Any())
                    {
                        paggingData.total_count = dataCount;
                    }
                    paggingData.ordermaster = PRStsdata;
                    return paggingData;
                }
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        [Authorize]
        [Route("GetRejectDamage")]
        [HttpPost]
        public PaggingData GetRejectDamage(FilterOrderDataDTOs filterOrderData)
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

                    string sqlquery = "Select AC.Comment,CS.ItemNumber,DR.WarehouseId,DR.ItemMultiMRPId,DS.ReasonToTransfer,AC.Id,AC.EntityId,DR.Qty,DR.itemName,cs.WarehouseName,sum(DR.Qty * DS.PurchasePrice ) as Amount,AC.Action,DR.CreatedDate,StockBatchMasterId,BM.MFGDate,BM.ExpiryDate,BM.BatchCode"
                                     + "   from DamageRequests DR join ApprovalConfigurations AC on AC.EntityId = DR.Id join CurrentStocks CS"
                                     + "   on CS.ItemMultiMRPId = DR.ItemMultiMRPId and"
                                     + "    cs.WarehouseId = DR.WarehouseId join DamageStocks DS on Ds.ItemMultiMRPId = DR.ItemMultiMRPId and DS.WarehouseId = DR.WarehouseId"
                                     + "    join BatchMasters BM on BM.Id=DR.StockBatchMasterId"
                                     + "  where AC.Action = 2  and AC.IsDeleted = 0 and DR.IsDeleted = 0 and DR.WarehouseId = " + filterOrderData.WarehouesId + " group by AC.Comment,CS.ItemNumber,DR.WarehouseId,DR.ItemMultiMRPId,DS.ReasonToTransfer,AC.Id,AC.EntityId,DR.Qty,DR.itemName,cs.WarehouseName,AC.Action,DR.CreatedDate,StockBatchMasterId,BM.MFGDate,BM.ExpiryDate,BM.BatchCode";
                    List<PurchaseOrderMasterDTOs> PRSts = db.Database.SqlQuery<PurchaseOrderMasterDTOs>(sqlquery).OrderByDescending(a => a.Id).ToList();
                    var PRStsdata = PRSts.Skip(skip).Take(take).ToList();
                    int dataCount = PRSts.Count();
                    if (PRSts != null && PRSts.Any())
                    {
                        paggingData.total_count = dataCount;
                    }
                    paggingData.ordermaster = PRStsdata;
                    return paggingData;
                }
            }
            catch (Exception ex)
            {
                return null;
            }
        }


        [Route("ApprovedItem")]
        [HttpPost]
        /* public async Task<DamageResult> ApprovedItem(DamageStockDc DamageStockobj)
         {
             DamageResult pOResult = new DamageResult();
             var identity = User.Identity as ClaimsIdentity;
             int userid = 0;
             if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                 userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
             using (var context = new AuthContext())
             {
                 var warehouse = context.Warehouses.FirstOrDefault(x => x.WarehouseId == DamageStockobj.WarehouseId);

                 if (warehouse.IsStopCurrentStockTrans)
                 {
                     pOResult = new DamageResult
                     {
                         Status = false,
                         Message = "Inventory Transactions are currently disabled for this warehouse... Please try after some time"
                     };
                     return pOResult;
                 }


                 var DamageApp = context.ApprovalConfigurations.Where(x => x.EntityId == DamageStockobj.EntityId && x.IsDeleted == false).FirstOrDefault();
                 if (DamageApp.ApproverId == userid)
                 {

                     var result = " ";
                     TransactionOptions option = new TransactionOptions();
                     option.IsolationLevel = IsolationLevel.RepeatableRead;
                     option.Timeout = TimeSpan.FromSeconds(90);
                     using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, option))
                     {

                         if (DamageStockobj != null && userid > 0)
                         {

                             People people = context.Peoples.Where(q => q.PeopleID == userid && q.Active == true).SingleOrDefault();
                             var CurrentStock = context.DbCurrentStock.Where(x => x.ItemNumber == DamageStockobj.ItemNumber && x.WarehouseId == DamageStockobj.WarehouseId && x.ItemMultiMRPId == DamageStockobj.ItemMultiMRPId).FirstOrDefault();
                             if (people != null && CurrentStock.CurrentInventory >= DamageStockobj.DamageInventory)
                             {

                                 DamageStock dst = context.DamageStockDB.Where(x => x.ItemNumber == DamageStockobj.ItemNumber && x.WarehouseId == DamageStockobj.WarehouseId && x.ItemMultiMRPId == DamageStockobj.ItemMultiMRPId).FirstOrDefault();
                                 if (dst == null)
                                 {
                                     double UnitPrice = 0;
                                     double PurchasePrice = 0;

                                     int ItemId = 0;
                                     // double DamageAmount = 0;

                                     var itemmaster = context.itemMasters.Where(x => x.Number == CurrentStock.ItemNumber && x.WarehouseId == CurrentStock.WarehouseId && x.PurchasePrice > 0).FirstOrDefault();
                                     if (itemmaster != null)
                                     {
                                         UnitPrice = Math.Round(itemmaster.UnitPrice, 2);
                                         PurchasePrice = Math.Round(itemmaster.PurchasePrice, 2);
                                         ItemId = itemmaster.ItemId;
                                     }

                                     DamageStock objst = new DamageStock();
                                     objst.WarehouseId = CurrentStock.WarehouseId;
                                     objst.WarehouseName = CurrentStock.WarehouseName;
                                     objst.ItemId = ItemId;
                                     objst.MRP = CurrentStock.MRP;
                                     objst.ItemMultiMRPId = CurrentStock.ItemMultiMRPId;
                                     objst.ItemNumber = CurrentStock.ItemNumber;
                                     objst.ItemName = CurrentStock.itemname;
                                     objst.DamageInventory = 0;// DamageStockobj.DamageInventory;
                                     objst.UnitPrice = UnitPrice;
                                     objst.PurchasePrice = PurchasePrice;//we set PurchasePrice 
                                     objst.ReasonToTransfer = DamageStockobj.ReasonToTransfer;
                                     objst.CreatedDate = indianTime;
                                     objst.CompanyId = 1;
                                     context.DamageStockDB.Add(objst);
                                     context.Commit();
                                     DamageApp.Action = 1;
                                     DamageApp.ApprovedDate = DateTime.Now;
                                     context.Entry(DamageApp).State = EntityState.Modified;
                                     context.Commit();
                                 }
                                 else
                                 {
                                 }
                                 double PurchasePrices = 0;
                                 double DamageAmount = 0;
                                 double DamageSumQtyAmount = 0;
                                 double totalSumQty = 0;
                                 var DamageStockDB = context.DamageStockDB.Where(x => x.ItemNumber == CurrentStock.ItemNumber && x.WarehouseId == CurrentStock.WarehouseId && x.PurchasePrice > 0 && x.ItemMultiMRPId == CurrentStock.ItemMultiMRPId).FirstOrDefault();
                                 var DamageStockSumQtydata = context.DamageStockHistoryDB.Where(x => x.DamageStockId == DamageStockDB.DamageStockId && x.OdOrPoId == 0 && x.CreatedDate.Month == indianTime.Month && x.CreatedDate.Year == indianTime.Year).ToList().Sum(x => x.InwordQty);
                                 PurchasePrices = Math.Round(DamageStockDB.PurchasePrice, 2);
                                 int DmgQty = DamageStockobj.DamageInventory;
                                 DamageAmount = Math.Round(DmgQty * PurchasePrices, 2);
                                 DamageSumQtyAmount = Math.Round(DamageStockSumQtydata * PurchasePrices, 2);
                                 totalSumQty = Math.Round(DamageAmount + DamageSumQtyAmount, 2);


                                 StockTransactionHelper helper = new StockTransactionHelper();
                                 PhysicalStockUpdateRequestDc StockTransfer = new PhysicalStockUpdateRequestDc();
                                 StockTransfer.ItemMultiMRPId = CurrentStock.ItemMultiMRPId;
                                 StockTransfer.WarehouseId = CurrentStock.WarehouseId;
                                 StockTransfer.Qty = DamageStockobj.DamageInventory;
                                 StockTransfer.SourceStockType = StockTypeTableNames.CurrentStocks;// "CurrentStocks";
                                 StockTransfer.DestinationStockType = StockTypeTableNames.DamagedStock;// "DamagedStocks";
                                 StockTransfer.StockTransferType = StockTransferTypeName.DamagedStocks;
                                 StockTransfer.Reason = "Stock transfer to Damagestock Due to: " + DamageStockobj.ReasonToTransfer;
                                 bool isupdated = helper.TransferBetweenPhysicalStocks(StockTransfer, userid, context, scope);

                                 BatchMasterManager batchMasterManager = new BatchMasterManager();
                                 bool isSuccess = batchMasterManager.MoveDirectBatchItemInSameBatch("C", "D", DamageStockobj.DamageInventory, DamageStockobj.StockBatchMasterId, DamageStockobj.ItemMultiMRPId, DamageStockobj.WarehouseId, context, userid);



                                 if (isupdated && isSuccess)
                                 {
                                     DamageApp.Action = 1;
                                     DamageApp.ApprovedDate = DateTime.Now;
                                     context.Entry(DamageApp).State = EntityState.Modified;
                                     context.Commit();
                                     scope.Complete();
                                     pOResult.Status = true;
                                     pOResult.Message = "Transaction Saved Successfully";
                                 }
                                 else
                                 {
                                     scope.Dispose();
                                     pOResult.Status = false;
                                     if (!isupdated)
                                         pOResult.Message = "one of the stock not available";
                                     if (!isSuccess)
                                         pOResult.Message = "currently stock movement in process for same ItemMultiMRP and Warehouse. Please try after some time.";
                                 }
                             }
                             else
                             {
                                 scope.Dispose();
                                 pOResult.Status = false;
                                 pOResult.Message = "Transfer Stock Is Insufficient in Current Stock Can't Approve";
                             }
                         }

                     }
                 }
                 else
                 {
                     pOResult.Status = false;
                     pOResult.Message = "You are not authorize to approve.";
                 }

             }
             return pOResult;
         }*/


        [Route("RejectDamageItem")]
        /*[HttpPost]*/
        public DamageResult rejectpr(RejectDamge reject)
        {
            var identity = User.Identity as ClaimsIdentity;
            int userid = 0;
            DamageResult pOResult = new DamageResult();
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

            using (AuthContext db = new AuthContext())
            {
                var pRPaymentAppoved = db.ApprovalConfigurations.Where(x => x.Id == reject.Id && x.IsDeleted == false).FirstOrDefault();
                var warehouse = db.Warehouses.FirstOrDefault(x => x.WarehouseId == pRPaymentAppoved.WarehouseId);

                if (warehouse.IsStopCurrentStockTrans)
                {
                    pOResult = new DamageResult
                    {
                        Status = false,
                        Message = "Inventory Transactions are currently disabled for this warehouse... Please try after some time"
                    };
                    return pOResult;
                }

                if (pRPaymentAppoved.ApproverId == userid)
                {
                    ApprovalConfiguration pr = db.ApprovalConfigurations.Where(x => x.Id == reject.Id && x.IsDeleted == false).FirstOrDefault();

                    if (pr != null)
                    {
                        pr.Action = 2;
                        pr.Comment = reject.Comment;
                        db.Entry(pr).State = EntityState.Modified;
                        if (db.Commit() > 0)
                        {
                            pOResult.Status = true;
                            pOResult.Message = "Rejected successfully.";
                        }
                    }
                }
                else
                {
                    pOResult.Status = false;
                    pOResult.Message = "You are not Authorize to Reject.";
                }
            }
            return pOResult;

        }


        #region New Damage Code


        [Route("StockMovementRequest")]
        [HttpPost]
        public DamageResult StockMovementRequest(MovementItemBatchDc movementItemBatchDc)
        {
            DamageResult msgresult = new DamageResult { Message = "", Status = false };
            TransactionOptions option = new TransactionOptions();
            option.IsolationLevel = System.Transactions.IsolationLevel.RepeatableRead;
            option.Timeout = TimeSpan.FromSeconds(90);
            string stock = "";
            if (movementItemBatchDc.StockType == 0)  //to stock type
            {
                stock = "DamageStock";
            }
            else if (movementItemBatchDc.StockType == 1)
            {
                stock = "NonSellable";
            }
            else if (movementItemBatchDc.StockType == 2)//NonRevenueStock
            {
                stock = "NonRevenueStock";
            }
            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, option))
            {
                using (AuthContext context = new AuthContext())
                {
                    var warehouse = context.Warehouses.FirstOrDefault(x => x.WarehouseId == movementItemBatchDc.WarehouseId);
                    if (!warehouse.IsStopCurrentStockTrans)
                    {
                        msgresult.Status = false;
                        msgresult.Message = "Please stop transactions of this warehouse before stock movement.";
                        return msgresult;
                    }
                    var shelfLife = context.ClearanceNonsShelfConfigurations.FirstOrDefault(cs => cs.ItemNumber == movementItemBatchDc.ItemNumber && cs.IsActive == true && cs.IsDeleted == false);
                    //var batchMasterdata = context.BatchMasters.FirstOrDefault(bm => bm.Id == movementItemBatchDc.BatchMasterId && bm.IsActive == true && bm.IsDeleted == false);
                    if (movementItemBatchDc.MFGDate != null && movementItemBatchDc.ExpiryDate != null && stock == "NonSellable")
                    {
                        var MFGDateParam = new SqlParameter("@MFGDate", movementItemBatchDc.MFGDate);
                        var ExpiryDateParam = new SqlParameter("@ExpiryDate", movementItemBatchDc.ExpiryDate);
                        var shelfLifePercentage = context.Database.SqlQuery<double>("GetShelfLifePercentage @MFGDate,@ExpiryDate", MFGDateParam, ExpiryDateParam).FirstOrDefault();
                        if (shelfLife != null && shelfLifePercentage >= shelfLife.NonSellShelfLifeFrom && shelfLifePercentage <= shelfLife.NonSellShelfLifeTo)
                        {
                        }
                        else
                        {
                            msgresult.Status = false;
                            msgresult.Message = "Stock Movement cannot be done because of shelflife is " + shelfLifePercentage;
                            return msgresult;
                        }
                    }

                    List<int> statuslst = new List<int> { 1, 3 };
                    //if (context.MovementDetailStockDB.Any(x => x.ItemMultiMrpId == movementItemBatchDc.ItemMultiMrpId && x.WarehouseId == movementItemBatchDc.WarehouseId && statuslst.Contains(x.Status) && x.IsActive))
                    //{
                    //    msgresult.Status = false;
                    //    msgresult.Message = "Already one request pending for approval.";
                    //    return msgresult;
                    //}
                    StockBatchMaster stockBatchMaster = new StockBatchMaster();
                    CurrentStock currentstock = new CurrentStock();
                    ClearanceStockNew clearanceStock = new ClearanceStockNew();

                    int availableqty = 0;
                    if (movementItemBatchDc.FromStockType == 7)  //current
                    {
                        currentstock = context.DbCurrentStock.FirstOrDefault(c => c.ItemMultiMRPId == movementItemBatchDc.ItemMultiMrpId && c.Deleted == false && c.WarehouseId == movementItemBatchDc.WarehouseId);
                        availableqty = currentstock.CurrentInventory;
                        stockBatchMaster = context.StockBatchMasters.FirstOrDefault(x => x.StockId == currentstock.StockId && x.BatchMasterId == movementItemBatchDc.BatchMasterId && x.StockType == "C");
                    }
                    else if (movementItemBatchDc.FromStockType == 8) //clearance
                    {
                        clearanceStock = context.ClearanceStockNewDB.FirstOrDefault(c => c.ItemMultiMRPId == movementItemBatchDc.ItemMultiMrpId && c.IsDeleted == false && c.WarehouseId == movementItemBatchDc.WarehouseId);
                        availableqty = clearanceStock.Inventory;

                        stockBatchMaster = context.StockBatchMasters.FirstOrDefault(x => x.StockId == clearanceStock.ClearanceStockId && x.BatchMasterId == movementItemBatchDc.BatchMasterId && x.StockType == "CL");
                    }

                    if (stockBatchMaster.Qty < movementItemBatchDc.InventoryCount && availableqty < movementItemBatchDc.InventoryCount && availableqty <= 0)
                    {
                        msgresult.Status = false;
                        msgresult.Message = "Requested qty not available.";
                        return msgresult;
                    }




                    var People = context.Peoples.FirstOrDefault(x => x.PeopleID == movementItemBatchDc.PeopleId);
                    var stockTxnTypes = context.StockTxnTypeMasters.Where(x => x.StockTxnType.Contains(stock));
                    var stockTxnTypeIn = stockTxnTypes.FirstOrDefault(x => x.StockTxnType == stock + "In");
                    var stockTxnTypeOut = stockTxnTypes.FirstOrDefault(x => x.StockTxnType == stock + "Out");
                    if ((movementItemBatchDc.FromStockType == 7 && stockBatchMaster != null) || (movementItemBatchDc.FromStockType == 8 && stockBatchMaster != null))
                    {
                        MovementDetailStock movementDetailStock = new MovementDetailStock
                        {
                            StockBatchMasterId = stockBatchMaster.Id,
                            Comment = movementItemBatchDc.Comment,
                            CreatedBy = People.PeopleID,
                            CreatedDate = DateTime.Now,
                            FromStockType = movementItemBatchDc.FromStockType == 7 ? "Current" : "Clearance",
                            Imageurl = movementItemBatchDc.ImageUrl,
                            InventoryCount = movementItemBatchDc.InventoryCount,
                            IsActive = true,
                            IsDeleted = false,
                            ItemMultiMrpId = movementItemBatchDc.ItemMultiMrpId,
                            Status = 1,
                            ToStockType = stock,  //to stock type
                            WarehouseId = movementItemBatchDc.WarehouseId,
                            PastInventory = stockBatchMaster.Qty
                        };
                        context.MovementDetailStockDB.Add(movementDetailStock);
                        context.Commit();

                        int requireQty = 0;
                        string Source = "", Destination = "";
                        if (stockBatchMaster.Qty < movementDetailStock.InventoryCount)
                        {
                            requireQty = movementDetailStock.InventoryCount - stockBatchMaster.Qty;
                            Source = StockTypeTableNames.VirtualStock;
                            Destination = movementItemBatchDc.FromStockType == 7 ? StockTypeTableNames.CurrentStocks : StockTypeTableNames.ClearanceStockNews;
                            stockBatchMaster.Qty += requireQty;
                            stockBatchMaster.ModifiedBy = movementItemBatchDc.PeopleId;
                            stockBatchMaster.ModifiedDate = DateTime.Now;
                            context.Entry(stockBatchMaster).State = EntityState.Modified;

                            StockBatchTransaction stockBatchTransaction1 = new StockBatchTransaction
                            {
                                StockTxnTypeId = stockTxnTypeIn.Id,
                                CreatedBy = movementItemBatchDc.PeopleId,
                                CreatedDate = indianTime,
                                IsActive = true,
                                IsDeleted = false,
                                ObjectId = movementDetailStock.Id,
                                ObjectDetailId = 0,
                                StockBatchMasterId = stockBatchMaster.Id,
                                TransactionDate = indianTime,
                                Qty = requireQty
                            };
                            context.StockBatchTransactions.Add(stockBatchTransaction1);
                        }
                        else
                        {
                            requireQty = 0;
                            Destination = StockTypeTableNames.VirtualStock;
                            Source = movementItemBatchDc.FromStockType == 7 ? StockTypeTableNames.CurrentStocks : StockTypeTableNames.ClearanceStockNews;
                        }

                        if (requireQty != 0)
                        {
                            StockTransactionHelper sthelper = new StockTransactionHelper();
                            List<ManualStockUpdateRequestDc> manualStockUpdateDcList = new List<ManualStockUpdateRequestDc>();

                            ManualStockUpdateRequestDc manualStockUpdateDc = new ManualStockUpdateRequestDc
                            {
                                ItemMultiMRPId = movementItemBatchDc.FromStockType == 7 ? currentstock.ItemMultiMRPId : clearanceStock.ItemMultiMRPId,
                                Reason = stock + " Movement Request Date:" + indianTime.ToString("dd/MM/yyyy") + " Id:" + movementDetailStock.Id,
                                StockTransferType = "ManualInventory",
                                Qty = requireQty,
                                WarehouseId = movementItemBatchDc.FromStockType == 7 ? currentstock.WarehouseId : clearanceStock.WarehouseId,
                                DestinationStockType = Destination,
                                SourceStockType = Source,
                                Status = "Pending"
                            };
                            manualStockUpdateDcList.Add(manualStockUpdateDc);

                            bool isSuccess = sthelper.ManualStockUpdate(manualStockUpdateDcList, People.PeopleID, context, scope);
                            if (!isSuccess)
                            {
                                msgresult.Message = stock + " Movement request not process please try after some time.";
                                return msgresult;
                            }
                            else
                            {
                                context.Commit();
                                if (movementItemBatchDc.FromStockType == 7)
                                {
                                    currentstock.CurrentInventory += requireQty;
                                    currentstock.CurrentInventory -= movementItemBatchDc.InventoryCount;
                                    currentstock.UpdateBy = People.DisplayName;
                                    currentstock.UpdatedDate = DateTime.Now;
                                    context.Entry(currentstock).State = EntityState.Modified;

                                    CurrentStockHistory Oss1 = new CurrentStockHistory();
                                    Oss1.StockId = currentstock.StockId;
                                    Oss1.ItemNumber = currentstock.ItemNumber;
                                    Oss1.itemname = currentstock.itemname;
                                    Oss1.ItemMultiMRPId = currentstock.ItemMultiMRPId;
                                    Oss1.ManualInventoryIn = (-1) * movementItemBatchDc.InventoryCount;
                                    Oss1.CurrentInventory = (currentstock.CurrentInventory);
                                    Oss1.TotalInventory = (currentstock.CurrentInventory);
                                    Oss1.ManualReason = stock + " Movement";
                                    Oss1.UOM = "Pc";
                                    Oss1.WarehouseName = currentstock.WarehouseName;
                                    Oss1.Warehouseid = currentstock.WarehouseId;
                                    Oss1.CompanyId = currentstock.CompanyId;
                                    Oss1.userid = People.PeopleID;
                                    Oss1.UserName = currentstock.UpdateBy;
                                    Oss1.CreationDate = currentstock.UpdatedDate;
                                    context.CurrentStockHistoryDb.Add(Oss1);
                                }
                                else if (movementItemBatchDc.FromStockType == 8)
                                {
                                    clearanceStock.Inventory += requireQty;
                                    clearanceStock.Inventory -= movementItemBatchDc.InventoryCount;//////////
                                    clearanceStock.ModifiedBy = movementItemBatchDc.PeopleId;
                                    clearanceStock.UpdatedDate = DateTime.Now;
                                    context.Entry(clearanceStock).State = EntityState.Modified;

                                    ClearanceStockNewHistory clearanceStockNewHistory = new ClearanceStockNewHistory();
                                    clearanceStockNewHistory.ClearanceStockId = clearanceStock.ClearanceStockId;
                                    clearanceStockNewHistory.ItemName = currentstock.itemname;
                                    clearanceStockNewHistory.ItemMultiMRPId = clearanceStock.ItemMultiMRPId;
                                    clearanceStockNewHistory.Inventory = (-1) * movementItemBatchDc.InventoryCount;////////
                                    clearanceStockNewHistory.NetInventory = (clearanceStock.Inventory);//////////////////
                                    clearanceStockNewHistory.Comment = stock + " Movement";
                                    clearanceStockNewHistory.WarehouseId = clearanceStock.WarehouseId;
                                    clearanceStockNewHistory.CreatedBy = People.PeopleID;
                                    clearanceStockNewHistory.CreatedDate = DateTime.Now;
                                    clearanceStockNewHistory.IsActive = true;
                                    clearanceStockNewHistory.IsDeleted = false;
                                    context.ClearanceStockNewHistoryDB.Add(clearanceStockNewHistory);
                                }

                            }

                        }
                        else
                        {
                            if (movementItemBatchDc.FromStockType == 7)
                            {
                                currentstock.CurrentInventory -= movementItemBatchDc.InventoryCount;
                                currentstock.UpdateBy = People.DisplayName;
                                currentstock.UpdatedDate = DateTime.Now;
                                context.Entry(currentstock).State = EntityState.Modified;

                                CurrentStockHistory Oss1 = new CurrentStockHistory();
                                Oss1.StockId = currentstock.StockId;
                                Oss1.ItemNumber = currentstock.ItemNumber;
                                Oss1.itemname = currentstock.itemname;
                                Oss1.ItemMultiMRPId = currentstock.ItemMultiMRPId;
                                Oss1.ManualInventoryIn = (-1) * movementItemBatchDc.InventoryCount;
                                Oss1.CurrentInventory = (currentstock.CurrentInventory);
                                Oss1.TotalInventory = (currentstock.CurrentInventory);
                                Oss1.ManualReason = stock + " Movement";
                                Oss1.UOM = "Pc";
                                Oss1.WarehouseName = currentstock.WarehouseName;
                                Oss1.Warehouseid = currentstock.WarehouseId;
                                Oss1.CompanyId = currentstock.CompanyId;
                                Oss1.userid = People.PeopleID;
                                Oss1.UserName = currentstock.UpdateBy;
                                Oss1.CreationDate = currentstock.UpdatedDate;
                                context.CurrentStockHistoryDb.Add(Oss1);
                            }
                            else if (movementItemBatchDc.FromStockType == 8)
                            {
                                clearanceStock.Inventory -= movementItemBatchDc.InventoryCount;//////////
                                clearanceStock.ModifiedBy = movementItemBatchDc.PeopleId;
                                clearanceStock.UpdatedDate = DateTime.Now;
                                context.Entry(clearanceStock).State = EntityState.Modified;

                                ClearanceStockNewHistory clearanceStockNewHistory = new ClearanceStockNewHistory();
                                clearanceStockNewHistory.ClearanceStockId = clearanceStock.ClearanceStockId;
                                clearanceStockNewHistory.ItemName = currentstock.itemname;
                                clearanceStockNewHistory.ItemMultiMRPId = clearanceStock.ItemMultiMRPId;
                                clearanceStockNewHistory.Inventory = (-1) * movementItemBatchDc.InventoryCount;////////
                                clearanceStockNewHistory.NetInventory = (clearanceStock.Inventory);//////////////////
                                clearanceStockNewHistory.Comment = stock + " Movement";
                                clearanceStockNewHistory.WarehouseId = clearanceStock.WarehouseId;
                                clearanceStockNewHistory.CreatedBy = People.PeopleID;
                                clearanceStockNewHistory.CreatedDate = DateTime.Now;
                                clearanceStockNewHistory.IsActive = true;
                                clearanceStockNewHistory.IsDeleted = false;
                                context.ClearanceStockNewHistoryDB.Add(clearanceStockNewHistory);
                            }
                        }



                        stockBatchMaster.Qty -= movementItemBatchDc.InventoryCount;
                        stockBatchMaster.ModifiedBy = movementItemBatchDc.PeopleId;
                        stockBatchMaster.ModifiedDate = DateTime.Now;
                        context.Entry(stockBatchMaster).State = EntityState.Modified;


                        StockBatchTransaction stockBatchTransaction = new StockBatchTransaction
                        {
                            StockTxnTypeId = stockTxnTypeOut.Id,
                            CreatedBy = movementItemBatchDc.PeopleId,
                            CreatedDate = indianTime,
                            IsActive = true,
                            IsDeleted = false,
                            ObjectId = movementDetailStock.Id,
                            ObjectDetailId = 0,
                            StockBatchMasterId = stockBatchMaster.Id,
                            TransactionDate = indianTime,
                            Qty = movementItemBatchDc.InventoryCount - requireQty
                        };
                        context.StockBatchTransactions.Add(stockBatchTransaction);


                        ReservedStock reservedStock = new ReservedStock
                        {
                            CreatedBy = People.PeopleID,
                            CreatedDate = DateTime.Now,
                            EntityId = Convert.ToInt32(movementDetailStock.Id),
                            EntityType = "MovementDetailStock",
                            InOutQty = movementItemBatchDc.InventoryCount,
                            IsActive = true,
                            IsDeleted = false,
                            ItemMultiMRPId = movementItemBatchDc.ItemMultiMrpId,
                            RefStockCode = movementItemBatchDc.FromStockType == 7 ? "C" : "CL",
                            WarehouseId = movementItemBatchDc.WarehouseId
                        };
                        context.ReservedStockDB.Add(reservedStock);
                        msgresult.Status = context.Commit() > 0;

                        if (msgresult.Status)
                        {
                            msgresult.Message = "Stock transfer request sent successfully.";
                            scope.Complete();
                        }
                        else
                        {
                            msgresult.Message = "Due to some issue this request not process, please try after some time.";
                            scope.Dispose();
                        }
                    }
                    else
                    {
                        msgresult.Status = false;
                        msgresult.Message = "Current inventory not found for selected batch code.";
                        return msgresult;
                    }

                }
            }

            return msgresult;
        }

        [Route("GetMovementStockDetail")]
        [HttpGet]
        public MovementResponse GetMovementStockDetail(int warehouseId, string barcode)
        {
            MovementResponse movementResponse = new MovementResponse { Msg = "", Status = false, BarcodeItemWithBatchDcs = new List<BarcodeItemWithBatchDc>() };
            List<BarcodeItemWithBatchDc> barcodeItemWithBatchDcs = new List<BarcodeItemWithBatchDc>();

            if (warehouseId == 0 || string.IsNullOrEmpty(barcode))
            {
                movementResponse.Status = false;
                movementResponse.Msg = "Barcode and warehouse required.";
                return movementResponse;
            }

            using (AuthContext context = new AuthContext())
            {
                var warehouse = context.Warehouses.FirstOrDefault(x => x.WarehouseId == warehouseId);

                if (!warehouse.IsStopCurrentStockTrans)
                {
                    movementResponse.Status = false;
                    movementResponse.Msg = "Please stop transactions of this warehouse before stock movement.";
                    return movementResponse;
                }
                List<BarcodeItemWithBatchDc> BarcodeItemWithBatchDcs = new List<BarcodeItemWithBatchDc>();
                //List<BarcodeItemWithBatchCurrentWiseDc> BarcodeItemWithBatchCurrentWiseDcs = new List<BarcodeItemWithBatchCurrentWiseDc>();
                List<BarcodeItemWithBatchClearanceWiseDc> BarcodeItemWithBatchClearanceWiseDcs = new List<BarcodeItemWithBatchClearanceWiseDc>();
                List<StockBatchDc> StockBatchDcs = new List<StockBatchDc>();
                List<StockBatchCLDc> StockBatchCLDcs = new List<StockBatchCLDc>();
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
                                         .Translate<BarcodeItemWithBatchDc>(reader).ToList();
                reader.NextResult();
                if (reader.HasRows)
                {
                    StockBatchDcs = ((IObjectContextAdapter)context)
                                    .ObjectContext
                                    .Translate<StockBatchDc>(reader).ToList();
                }
                if (context.Database.Connection.State != System.Data.ConnectionState.Open)
                    context.Database.Connection.Open();
                context.Database.CommandTimeout = 120;
                var cmdCL = context.Database.Connection.CreateCommand();
                cmdCL.CommandText = "[dbo].[GetItemWithBatchDetailClearanceWise]";
                cmdCL.CommandType = System.Data.CommandType.StoredProcedure;
                cmdCL.Parameters.Add(new SqlParameter("@barcode", barcode));
                cmdCL.Parameters.Add(new SqlParameter("@warehouseId", warehouseId));


                // Run the sproc
                var readerCL = cmdCL.ExecuteReader();
                BarcodeItemWithBatchClearanceWiseDcs = ((IObjectContextAdapter)context)
                                         .ObjectContext
                                         .Translate<BarcodeItemWithBatchClearanceWiseDc>(readerCL).ToList();
                readerCL.NextResult();
                if (readerCL.HasRows)
                {
                    StockBatchCLDcs = ((IObjectContextAdapter)context)
                                    .ObjectContext
                                    .Translate<StockBatchCLDc>(readerCL).ToList();
                }

                if (BarcodeItemWithBatchDcs != null && BarcodeItemWithBatchDcs.Any())
                {
                    foreach (var item in BarcodeItemWithBatchDcs)
                    {
                        if (StockBatchDcs != null && StockBatchDcs.Any(x => x.ItemMultiMrpId == item.ItemMultiMrpId))
                        {
                            item.StockBatchDcs = StockBatchDcs.Where(x => x.ItemMultiMrpId == item.ItemMultiMrpId).ToList();
                        }
                        if (StockBatchCLDcs != null && StockBatchCLDcs.Any(x => x.ItemMultiMrpId == item.ItemMultiMrpId))
                        {
                            item.StockBatchCLDcs = StockBatchCLDcs.Where(x => x.ItemMultiMrpId == item.ItemMultiMrpId).ToList();
                        }
                    }
                }
                if (BarcodeItemWithBatchClearanceWiseDcs != null && BarcodeItemWithBatchClearanceWiseDcs.Any())
                {
                    foreach (var item in BarcodeItemWithBatchClearanceWiseDcs)
                    {
                        if (StockBatchCLDcs != null && StockBatchCLDcs.Any(x => x.ItemMultiMrpId == item.ItemMultiMrpId))
                        {
                            item.StockBatchCLDcs = StockBatchCLDcs.Where(x => x.ItemMultiMrpId == item.ItemMultiMrpId).ToList();
                        }
                    }
                }
                movementResponse.BarcodeItemWithBatchDcs = BarcodeItemWithBatchDcs;
                //movementResponse.BarcodeItemWithBatchClearanceWiseDcs = BarcodeItemWithBatchClearanceWiseDcs;
                movementResponse.Status = true;
                movementResponse.Msg = "";
            }
            return movementResponse;
        }

        [Route("GetSubmittedMovementStock")]
        [HttpGet]
        public List<MobileMovementItem> GetSubmittedMovementStock(int peopleId, int skip, int take)
        {
            List<MobileMovementItem> mobileMovementItems = new List<MobileMovementItem>();
            skip = skip * take;
            using (AuthContext context = new AuthContext())
            {
                if (context.Database.Connection.State != System.Data.ConnectionState.Open)
                    context.Database.Connection.Open();

                var cmd = context.Database.Connection.CreateCommand();
                cmd.CommandText = "[dbo].[GetSubmittedMovementStock]";
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.Parameters.Add(new SqlParameter("@peopleId", peopleId));
                cmd.Parameters.Add(new SqlParameter("@skip", skip));
                cmd.Parameters.Add(new SqlParameter("@take", take));


                // Run the sproc
                var reader = cmd.ExecuteReader();
                mobileMovementItems = ((IObjectContextAdapter)context)
                                         .ObjectContext
                                         .Translate<MobileMovementItem>(reader).ToList();

            }
            return mobileMovementItems;

        }

        [Route("GetMovementStockForWarehouse")]
        [HttpPost]
        public ResponseMovementItems GetMovementStockForWarehouse(RequestMovementItem requestMovementItem)
        {
            ResponseMovementItems responseMovementItems = new ResponseMovementItems();
            var identity = User.Identity as ClaimsIdentity;
            int userid = 0, Warehouseid = 0;
            List<string> rolenames = null;
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "Warehouseid"))
                Warehouseid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "Warehouseid").Value);
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type.ToLower() == "rolenames"))
                rolenames = (identity.Claims.FirstOrDefault(x => x.Type.ToLower() == "rolenames").Value).Split(',').Select(x => x).ToList();


            MongoDbHelper<StockMovmentApproval> mongoDbHelper = new MongoDbHelper<StockMovmentApproval>();
            var stockMovmentPredicate = PredicateBuilder.New<StockMovmentApproval>(x => !string.IsNullOrEmpty(x.StockType));
            if (requestMovementItem.stockType != "All")
                stockMovmentPredicate = stockMovmentPredicate.And(x => x.StockType == requestMovementItem.stockType);
            var mongoStockMovmentApprovals = (mongoDbHelper.Select(stockMovmentPredicate)).ToList();
            if (rolenames != null && mongoStockMovmentApprovals != null && mongoStockMovmentApprovals.Any() && rolenames.Contains(mongoStockMovmentApprovals.FirstOrDefault().WarehouseApprovalRole))
            {
                using (AuthContext context = new AuthContext())
                {
                    if (string.IsNullOrEmpty(requestMovementItem.actiontype))
                        requestMovementItem.actiontype = "search";

                    if (context.Database.Connection.State != System.Data.ConnectionState.Open)
                        context.Database.Connection.Open();


                    var cmd = context.Database.Connection.CreateCommand();
                    cmd.CommandText = "[dbo].[GetMovementStockForWarehouse]";
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;


                    var MrpIdDt1 = new System.Data.DataTable();
                    MrpIdDt1.Columns.Add("IntValue");
                    foreach (var item in requestMovementItem.warehouseId)
                    {
                        var dr = MrpIdDt1.NewRow();
                        dr["IntValue"] = item;
                        MrpIdDt1.Rows.Add(dr);
                    }

                    var mrpparam1 = new SqlParameter("@warehouseId", MrpIdDt1);
                    mrpparam1.SqlDbType = System.Data.SqlDbType.Structured;
                    mrpparam1.TypeName = "dbo.IntValues";
                    //cmd.Parameters.Add(new SqlParameter("@warehouseId", requestMovementItem.warehouseId.FirstOrDefault()));
                    cmd.Parameters.Add(mrpparam1);
                    cmd.Parameters.Add(new SqlParameter("@stockType", requestMovementItem.stockType));
                    cmd.Parameters.Add(new SqlParameter("@fromStockType", requestMovementItem.fromStockType));
                    cmd.Parameters.Add(new SqlParameter("@status", requestMovementItem.status));
                    cmd.Parameters.Add(new SqlParameter("@startDate", requestMovementItem.startDate));
                    cmd.Parameters.Add(new SqlParameter("@endDate", requestMovementItem.endDate));
                    cmd.Parameters.Add(new SqlParameter("@skip", requestMovementItem.skip));
                    cmd.Parameters.Add(new SqlParameter("@take", requestMovementItem.take));
                    cmd.Parameters.Add(new SqlParameter("@actiontype", requestMovementItem.actiontype));


                    var reader = cmd.ExecuteReader();
                    var MovementItems = ((IObjectContextAdapter)context)
                                             .ObjectContext
                                             .Translate<MovementItems>(reader).ToList();
                    reader.NextResult();
                    if (reader.Read())
                    {
                        responseMovementItems.totalRecord = Convert.ToInt32(reader["totalRecord"]);
                    }

                    if (MovementItems.Any())
                    {
                        DateTime startDate = DateTime.Now.Date.AddDays(-10);
                        DateTime endDate = DateTime.Now;
                        var MrpIdDt = new System.Data.DataTable();
                        MrpIdDt.Columns.Add("IntValue");
                        foreach (var item in MovementItems)
                        {
                            var dr = MrpIdDt.NewRow();
                            dr["IntValue"] = item.ItemMultiMrpId;
                            MrpIdDt.Rows.Add(dr);
                        }
                        var warehouseIdDt1 = new System.Data.DataTable();
                        warehouseIdDt1.Columns.Add("IntValue");
                        if (requestMovementItem.warehouseId == null)
                        {
                            warehouseIdDt1 = null;
                        }
                        else
                        {
                            foreach (var wId in requestMovementItem.warehouseId)
                            {
                                var dr = warehouseIdDt1.NewRow();
                                dr["IntValue"] = wId;
                                warehouseIdDt1.Rows.Add(dr);
                            }
                        }

                        var warehouseParam1 = new SqlParameter("warehouseId", warehouseIdDt1);
                        warehouseParam1.SqlDbType = System.Data.SqlDbType.Structured;
                        warehouseParam1.TypeName = "dbo.IntValues";
                        var mrpparam = new SqlParameter("itemmultimrpIds", MrpIdDt);
                        mrpparam.SqlDbType = System.Data.SqlDbType.Structured;
                        mrpparam.TypeName = "dbo.IntValues";
                        cmd = context.Database.Connection.CreateCommand();
                        cmd.CommandText = "[FIFO].[GetItemAPPsByMrpIdAndWarehouse]";
                        cmd.CommandType = System.Data.CommandType.StoredProcedure;
                        cmd.Parameters.Add(warehouseParam1);
                        cmd.Parameters.Add(mrpparam);
                        cmd.Parameters.Add(new SqlParameter("@startDate", startDate));
                        cmd.Parameters.Add(new SqlParameter("@enddate", endDate));

                        // Run the sproc
                        var readerApp = cmd.ExecuteReader();
                        var LastMonthApps = ((IObjectContextAdapter)context)
                         .ObjectContext
                         .Translate<LastMonthApp>(readerApp).ToList();


                        foreach (var item in MovementItems)
                        {
                            if (LastMonthApps.Any(x => x.itemmultimrpid == item.ItemMultiMrpId))
                            {
                                item.APP = LastMonthApps.FirstOrDefault(x => x.itemmultimrpid == item.ItemMultiMrpId).APP;
                            }

                            if (!string.IsNullOrEmpty(item.Imageurl))
                            {
                                item.Imageurl = string.Format("{0}://{1}:{2}/{3}", new Uri(HttpContext.Current.Request.Url.AbsoluteUri).Scheme
                                                                        , HttpContext.Current.Request.Url.DnsSafeHost
                                                                        , (HttpContext.Current.Request.Url.Port != 80 && HttpContext.Current.Request.Url.Port != 443 ? ":" + HttpContext.Current.Request.Url.Port : "")
                                                                        , item.Imageurl);
                            }
                        }
                    }

                    responseMovementItems.MovementItems = MovementItems;
                }
            }
            return responseMovementItems;
        }

        [Route("WarehouseMovementReqestProcess")]
        [HttpGet]
        public DamageResult WarehouseMovementReqestProcess(int requestId, bool Status, string comment = null, int stockType = 7)
        {
            var identity = User.Identity as ClaimsIdentity;
            int userid = 0;
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
            DamageResult msgresult = new DamageResult { Message = "", Status = false };
            CurrentStock currentstock = new CurrentStock();
            ClearanceStockNew clearanceStockNew = new ClearanceStockNew();
            NonRevenueOrderStock nonRevenueOrderStock = new NonRevenueOrderStock();

            if (requestId > 0 && userid > 0)
            {
                TransactionOptions option = new TransactionOptions();
                option.IsolationLevel = System.Transactions.IsolationLevel.RepeatableRead;
                option.Timeout = TimeSpan.FromSeconds(90);
                using (TransactionScope scope = new TransactionScope(TransactionScopeOption.RequiresNew, option))
                {
                    using (AuthContext context = new AuthContext())
                    {
                        var movementDetailStock = context.MovementDetailStockDB.FirstOrDefault(x => x.Id == requestId);
                        if (movementDetailStock != null)
                        {

                            MongoDbHelper<StockMovmentApproval> mongoDbHelper = new MongoDbHelper<StockMovmentApproval>();
                            var stockMovmentPredicate = PredicateBuilder.New<StockMovmentApproval>(x => !string.IsNullOrEmpty(x.StockType) && x.StockType == movementDetailStock.ToStockType);
                            var mongoStockMovmentApprovals = (mongoDbHelper.Select(stockMovmentPredicate)).FirstOrDefault();

                            movementDetailStock.WarehouseComment = comment;
                            if (Status)
                            {
                                if (context.Database.Connection.State != System.Data.ConnectionState.Open)
                                    context.Database.Connection.Open();

                                var cmd = context.Database.Connection.CreateCommand();
                                cmd.CommandText = "[dbo].[GetStockMovementConsume]";
                                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                                cmd.Parameters.Add(new SqlParameter("@requestId", movementDetailStock.Id));

                                var totalConsume = Convert.ToDouble(cmd.ExecuteScalar());

                                movementDetailStock.Status = 3; //2 = Approved By WLP

                                string finalApprovalRole = "";
                                if (mongoStockMovmentApprovals != null && mongoStockMovmentApprovals.StockMovmentApprovalRanges.Any())
                                {
                                    finalApprovalRole = mongoStockMovmentApprovals.StockMovmentApprovalRanges.FirstOrDefault(x => x.FromAmount < totalConsume && x.ToAmount >= totalConsume)?.ApprovalRole;
                                    movementDetailStock.FinalApprovedRole = finalApprovalRole;
                                    movementDetailStock.ModifiedBy = userid;
                                    movementDetailStock.ModifiedDate = DateTime.Now;
                                }

                                //if (totalConsume < 50000)//HQ LP
                                //{
                                //    var warehouse = context.Warehouses.FirstOrDefault(x => x.WarehouseId == movementDetailStock.WarehouseId);
                                //    if (!warehouse.IsStopCurrentStockTrans)
                                //    {
                                //        msgresult.Status = false;
                                //        msgresult.Message = "Please stop transactions of this warehouse before stock movement.";
                                //        return msgresult;
                                //    }
                                //    movementDetailStock.FinalApprovedBy = userid;
                                //    movementDetailStock.ModifiedBy = userid;
                                //    movementDetailStock.ModifiedDate = DateTime.Now;
                                //    movementDetailStock.Status = 4; //4=Approved
                                //    MovmentStockFinalApproved(requestId, userid, context);
                                //}
                                //else if (totalConsume > 50000 && totalConsume < 100000)  // Zonal Purchase Lead
                                //{
                                //    movementDetailStock.FinalApprovedBy = 1;
                                //    movementDetailStock.ModifiedBy = userid;
                                //    movementDetailStock.ModifiedDate = DateTime.Now;
                                //}
                                //else if (totalConsume > 100000)  // Zonal Purchase Lead
                                //{
                                //    movementDetailStock.FinalApprovedBy = 1;
                                //    movementDetailStock.ModifiedBy = userid;
                                //    movementDetailStock.ModifiedDate = DateTime.Now;
                                //}

                            }
                            else
                            {
                                var warehouse = context.Warehouses.FirstOrDefault(x => x.WarehouseId == movementDetailStock.WarehouseId);
                                if (!warehouse.IsStopCurrentStockTrans)
                                {
                                    msgresult.Status = false;
                                    msgresult.Message = "Please stop transactions of this warehouse before stock movement.";
                                    return msgresult;
                                }

                                movementDetailStock.Status = 5; //5='Reject by WLP
                                movementDetailStock.ModifiedBy = userid;
                                movementDetailStock.ModifiedDate = DateTime.Now;
                                if (stockType == 7)
                                {
                                    currentstock = context.DbCurrentStock.FirstOrDefault(c => c.ItemMultiMRPId == movementDetailStock.ItemMultiMrpId && c.Deleted == false && c.WarehouseId == movementDetailStock.WarehouseId);
                                }
                                //else if (stockType == 9)
                                //{
                                //    nonRevenueOrderStock = context.NonRevenueOrderStocks.FirstOrDefault(c => c.ItemMultiMRPId == movementDetailStock.ItemMultiMrpId && c.IsDeleted == false && c.WarehouseId == movementDetailStock.WarehouseId);
                                //}
                                else
                                {
                                    clearanceStockNew = context.ClearanceStockNewDB.FirstOrDefault(c => c.ItemMultiMRPId == movementDetailStock.ItemMultiMrpId && c.IsDeleted == false && c.WarehouseId == movementDetailStock.WarehouseId);
                                }

                                var stockBatchMaster = context.StockBatchMasters.FirstOrDefault(x => x.Id == movementDetailStock.StockBatchMasterId);
                                var People = context.Peoples.FirstOrDefault(x => x.PeopleID == userid);

                                var stockTxnTypes = context.StockTxnTypeMasters.Where(x => x.StockTxnType.Contains(movementDetailStock.ToStockType));
                                var stockTxnTypeIn = stockTxnTypes.FirstOrDefault(x => x.StockTxnType == movementDetailStock.ToStockType + "In");
                                var stockTxnTypeOut = stockTxnTypes.FirstOrDefault(x => x.StockTxnType == movementDetailStock.ToStockType + "Out");

                                var stockTrans = context.StockBatchTransactions.Where(x => x.ObjectId == movementDetailStock.Id && x.IsActive).ToList();

                                stockBatchMaster.Qty += movementDetailStock.InventoryCount;
                                stockBatchMaster.ModifiedBy = userid;
                                stockBatchMaster.ModifiedDate = DateTime.Now;
                                context.Entry(stockBatchMaster).State = EntityState.Modified;
                                StockBatchTransaction stockBatchTransaction = null;
                                //int virtualQty = stockTrans != null && stockTrans.Any() ? stockTrans.Where(x => x.Qty > 0).Sum(x => x.Qty) : 0;
                                string stockreason = movementDetailStock.ToStockType + " Movement Request Date:" + movementDetailStock.CreatedDate.ToString("dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture) + " Id:" + movementDetailStock.Id;
                                int? virtualQty = null;
                                if (stockType == 7)
                                {
                                    var maualstocks = context.ManualStockUpdateRequestDB.Where(x => x.ItemMultiMRPId == currentstock.ItemMultiMRPId && x.WarehouseId == currentstock.WarehouseId && x.Reason == stockreason);

                                    if (maualstocks != null && maualstocks.Any())
                                        virtualQty = maualstocks.Sum(x => x.Qty);
                                }
                                //else if (stockType == 9) //nonrevenus stock
                                //{
                                //    var maualstocks = context.ManualStockUpdateRequestDB.Where(x => x.ItemMultiMRPId == nonRevenueOrderStock.ItemMultiMRPId && x.WarehouseId == nonRevenueOrderStock.WarehouseId && x.Reason == stockreason);

                                //    if (maualstocks != null && maualstocks.Any())
                                //        virtualQty = maualstocks.Sum(x => x.Qty);
                                //}
                                else
                                {
                                    var maualstocks = context.ManualStockUpdateRequestDB.Where(x => x.ItemMultiMRPId == clearanceStockNew.ItemMultiMRPId && x.WarehouseId == clearanceStockNew.WarehouseId && x.Reason == stockreason);

                                    if (maualstocks != null && maualstocks.Any())
                                        virtualQty = maualstocks.Sum(x => x.Qty);
                                }

                                foreach (var trans in stockTrans)
                                {
                                    stockBatchTransaction = new StockBatchTransaction
                                    {
                                        StockTxnTypeId = stockTxnTypes.FirstOrDefault(x => x.Id != trans.Id).Id,
                                        CreatedBy = userid,
                                        CreatedDate = indianTime,
                                        IsActive = true,
                                        IsDeleted = false,
                                        ObjectId = movementDetailStock.Id,
                                        ObjectDetailId = 0,
                                        StockBatchMasterId = stockBatchMaster.Id,
                                        TransactionDate = indianTime,
                                        Qty = trans.Qty
                                    };
                                    context.StockBatchTransactions.Add(stockBatchTransaction);
                                }


                                if (stockType != 9)
                                {
                                    ReservedStock reservedStock = new ReservedStock
                                    {
                                        CreatedBy = People.PeopleID,
                                        CreatedDate = DateTime.Now,
                                        EntityId = Convert.ToInt32(movementDetailStock.Id),
                                        EntityType = "MovementDetailStock",
                                        InOutQty = (-1) * movementDetailStock.InventoryCount,
                                        IsActive = true,
                                        IsDeleted = false,
                                        ItemMultiMRPId = movementDetailStock.ItemMultiMrpId,
                                        RefStockCode = stockType == 7 ? "C" : "CL",
                                        WarehouseId = movementDetailStock.WarehouseId
                                    };
                                    context.ReservedStockDB.Add(reservedStock);
                                }
                                //else if (stockType == 9)  //NonRevenueStock
                                //{
                                //    ReservedStock reservedStock = new ReservedStock
                                //    {
                                //        CreatedBy = People.PeopleID,
                                //        CreatedDate = DateTime.Now,
                                //        EntityId = Convert.ToInt32(movementDetailStock.Id),
                                //        EntityType = "MovementDetailStock",
                                //        InOutQty = (-1) * movementDetailStock.InventoryCount,
                                //        IsActive = true,
                                //        IsDeleted = false,
                                //        ItemMultiMRPId = movementDetailStock.ItemMultiMrpId,
                                //        RefStockCode = "NR",
                                //        WarehouseId = movementDetailStock.WarehouseId
                                //    };
                                //    context.ReservedStockDB.Add(reservedStock);
                                //}

                                if (stockType == 7)
                                {

                                    currentstock.CurrentInventory += movementDetailStock.InventoryCount;
                                    currentstock.UpdateBy = People.DisplayName;
                                    currentstock.UpdatedDate = DateTime.Now;
                                    context.Entry(currentstock).State = EntityState.Modified;
                                    context.Commit();

                                    CurrentStockHistory Oss1 = new CurrentStockHistory();
                                    Oss1.StockId = currentstock.StockId;
                                    Oss1.ItemNumber = currentstock.ItemNumber;
                                    Oss1.itemname = currentstock.itemname;
                                    Oss1.ItemMultiMRPId = currentstock.ItemMultiMRPId;
                                    Oss1.ManualInventoryIn = movementDetailStock.InventoryCount;
                                    Oss1.CurrentInventory = (currentstock.CurrentInventory);
                                    Oss1.TotalInventory = (currentstock.CurrentInventory);
                                    Oss1.ManualReason = movementDetailStock.ToStockType + " Movement Return";
                                    Oss1.UOM = "Pc";
                                    Oss1.WarehouseName = currentstock.WarehouseName;
                                    Oss1.Warehouseid = currentstock.WarehouseId;
                                    Oss1.CompanyId = currentstock.CompanyId;
                                    Oss1.userid = People.PeopleID;
                                    Oss1.UserName = currentstock.UpdateBy;
                                    Oss1.CreationDate = currentstock.UpdatedDate;
                                    context.CurrentStockHistoryDb.Add(Oss1);

                                }
                                //else if (stockType == 9)//NonRevenue Stock
                                //{

                                //    nonRevenueOrderStock.NonRevenueInventory += movementDetailStock.InventoryCount;
                                //    nonRevenueOrderStock.ModifiedBy = People.PeopleID;
                                //    nonRevenueOrderStock.ModifiedDate = DateTime.Now;
                                //    context.Entry(nonRevenueOrderStock).State = EntityState.Modified;
                                //    context.Commit();

                                //    NonRevenueOrderStockHistory Oss1 = new NonRevenueOrderStockHistory();
                                //    Oss1.Id = currentstock.StockId;
                                //    Oss1.ItemNumber = currentstock.ItemNumber;
                                //    Oss1.ItemName = currentstock.itemname;
                                //    Oss1.ItemMultiMRPId = currentstock.ItemMultiMRPId;
                                //    Oss1.InwordQty = movementDetailStock.InventoryCount;
                                //    Oss1.NonRevenueInventory = (currentstock.CurrentInventory);
                                //    Oss1.Comment = movementDetailStock.Comment;

                                //    //Oss1. = (currentstock.CurrentInventory);
                                //    Oss1.ReasonToTransfer = movementDetailStock.ToStockType + " Movement Return";
                                //    //Oss1. = "Pc";
                                //    //Oss1.WarehouseId = currentstock.WarehouseName;
                                //    Oss1.WarehouseId = currentstock.WarehouseId;
                                //    //Oss1.com = currentstock.CompanyId;
                                //    // Oss1.use = People.PeopleID;
                                //    //Oss1.user = currentstock.UpdateBy;
                                //    Oss1.CreatedDate = currentstock.UpdatedDate;
                                //    context.NonRevenueOrderStockHistories.Add(Oss1);

                                //}
                                else
                                {
                                    clearanceStockNew.Inventory += movementDetailStock.InventoryCount;
                                    clearanceStockNew.ModifiedBy = People.PeopleID;
                                    clearanceStockNew.UpdatedDate = DateTime.Now;
                                    context.Entry(clearanceStockNew).State = EntityState.Modified;
                                    context.Commit();

                                    ClearanceStockNewHistory Oss1 = new ClearanceStockNewHistory();
                                    Oss1.ClearanceStockId = clearanceStockNew.ClearanceStockId;
                                    Oss1.ItemMultiMRPId = clearanceStockNew.ItemMultiMRPId;
                                    Oss1.Inventory = movementDetailStock.InventoryCount;
                                    Oss1.NetInventory = (clearanceStockNew.Inventory);
                                    Oss1.Comment = movementDetailStock.ToStockType + " Movement Return";
                                    Oss1.WarehouseId = clearanceStockNew.WarehouseId;
                                    Oss1.CreatedBy = People.PeopleID;
                                    Oss1.CreatedDate = DateTime.Now;
                                    Oss1.IsActive = true;
                                    Oss1.IsDeleted = false;
                                    context.ClearanceStockNewHistoryDB.Add(Oss1);
                                }

                                if (virtualQty.HasValue && virtualQty.Value > 0)
                                {
                                    StockTransactionHelper sthelper = new StockTransactionHelper();
                                    List<ManualStockUpdateRequestDc> manualStockUpdateDcList = new List<ManualStockUpdateRequestDc>();
                                    ManualStockUpdateRequestDc manualStockUpdateDc = new ManualStockUpdateRequestDc
                                    {
                                        ItemMultiMRPId = stockType == 7 ? currentstock.ItemMultiMRPId : clearanceStockNew.ItemMultiMRPId,
                                        Reason = movementDetailStock.ToStockType + " Movement Request Date:" + indianTime.ToString("dd/MM/yyyy"),
                                        StockTransferType = "ManualInventory",
                                        Qty = virtualQty.Value,
                                        WarehouseId = stockType == 7 ? currentstock.WarehouseId : clearanceStockNew.WarehouseId,
                                        DestinationStockType = StockTypeTableNames.VirtualStock,
                                        SourceStockType = stockType == 7 ? StockTypeTableNames.CurrentStocks : StockTypeTableNames.ClearanceStockNews,
                                        Status = "Pending"
                                    };
                                    manualStockUpdateDcList.Add(manualStockUpdateDc);

                                    bool isSuccess = sthelper.ManualStockUpdate(manualStockUpdateDcList, People.PeopleID, context, scope);
                                    if (!isSuccess)
                                    {
                                        msgresult.Message = movementDetailStock.ToStockType + " Movement request not process please try after some time.";
                                        return msgresult;
                                    }
                                    else
                                    {
                                        if (stockBatchMaster != null)
                                        {
                                            if (stockBatchMaster.Qty >= virtualQty.Value)
                                                stockBatchMaster.Qty -= virtualQty.Value;
                                            else
                                                stockBatchMaster.Qty = 0;

                                            context.Entry(stockBatchMaster).State = EntityState.Modified;
                                        }
                                    }
                                }
                            }
                            context.Entry(movementDetailStock).State = EntityState.Modified;
                            msgresult.Status = context.Commit() > 0;

                            if (msgresult.Status)
                            {
                                msgresult.Message = "Stock transfer request " + (Status ? "Approved" : "Rejected") + " successfully.";
                                scope.Complete();
                            }
                            else
                            {
                                msgresult.Message = "Due to some issue please try after some time.";
                                scope.Dispose();
                            }
                        }
                        else
                        {
                            msgresult.Message = "Request Stock not found";
                            msgresult.Status = false;
                        }
                    }
                }
            }
            return msgresult;
        }

        [Route("HQMovementReqestProcess")]
        [HttpGet]
        public DamageResult HQMovementReqestProcess(int requestId, bool Status, string comment = null, int stockType = 7)
        {
            DamageResult msgresult = new DamageResult { Message = "", Status = false };
            CurrentStock currentstock = new CurrentStock();
            ClearanceStockNew clearanceStockNew = new ClearanceStockNew();
            NonRevenueOrderStock nonRevenueOrderStock = new NonRevenueOrderStock();
            if (DamageRequestIds.Any(x => x == requestId))
            {
                msgresult.Message = "Request already process please wait!!";
                return msgresult;
            }

            var identity = User.Identity as ClaimsIdentity;
            int userid = 0;
            List<string> rolenames = new List<string>();
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type.ToLower() == "rolenames"))
                rolenames = (identity.Claims.FirstOrDefault(x => x.Type.ToLower() == "rolenames").Value).Split(',').Select(x => x).ToList();


            TransactionOptions option = new TransactionOptions();
            option.IsolationLevel = System.Transactions.IsolationLevel.RepeatableRead;
            option.Timeout = TimeSpan.FromSeconds(90);
            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, option))
            {
                using (AuthContext context = new AuthContext())
                {
                    var movementDetailStock = context.MovementDetailStockDB.FirstOrDefault(x => x.Id == requestId && rolenames.Contains(x.FinalApprovedRole));
                    if (movementDetailStock != null)
                    {
                        movementDetailStock.HQComment = comment;
                        var warehouse = context.Warehouses.FirstOrDefault(x => x.WarehouseId == movementDetailStock.WarehouseId);
                        if (!warehouse.IsStopCurrentStockTrans)
                        {
                            DamageRequestIds.RemoveAll(x => x == requestId);
                            msgresult.Status = false;
                            msgresult.Message = "Please stop transactions of this warehouse before stock movement.";
                            return msgresult;
                        }
                        if (Status)
                        {
                            movementDetailStock.ModifiedBy = userid;
                            movementDetailStock.ModifiedDate = DateTime.Now;
                            movementDetailStock.Status = 4; //4=Approved
                            movementDetailStock.FinalApprovedBy = userid;
                            MovmentStockFinalApproved(requestId, userid, context);
                        }
                        else
                        {

                            movementDetailStock.Status = 6; //6='Reject by HQLP'
                            movementDetailStock.ModifiedBy = userid;
                            movementDetailStock.ModifiedDate = DateTime.Now;

                            if (stockType == 7)
                            {
                                currentstock = context.DbCurrentStock.FirstOrDefault(c => c.ItemMultiMRPId == movementDetailStock.ItemMultiMrpId && c.Deleted == false && c.WarehouseId == movementDetailStock.WarehouseId);
                            }
                            //else if (stockType == 9)
                            //{
                            //    nonRevenueOrderStock = context.NonRevenueOrderStocks.FirstOrDefault(c => c.ItemMultiMRPId == movementDetailStock.ItemMultiMrpId && c.IsDeleted == false && c.WarehouseId == movementDetailStock.WarehouseId);
                            //}
                            else
                            {
                                clearanceStockNew = context.ClearanceStockNewDB.FirstOrDefault(c => c.ItemMultiMRPId == movementDetailStock.ItemMultiMrpId && c.IsDeleted == false && c.WarehouseId == movementDetailStock.WarehouseId);
                            }

                            var stockBatchMaster = context.StockBatchMasters.FirstOrDefault(x => x.Id == movementDetailStock.StockBatchMasterId);
                            var People = context.Peoples.FirstOrDefault(x => x.PeopleID == userid);

                            var stockTxnTypes = context.StockTxnTypeMasters.Where(x => x.StockTxnType.Contains(movementDetailStock.ToStockType));
                            var stockTxnTypeIn = stockTxnTypes.FirstOrDefault(x => x.StockTxnType == movementDetailStock.ToStockType + "In");
                            var stockTxnTypeOut = stockTxnTypes.FirstOrDefault(x => x.StockTxnType == movementDetailStock.ToStockType + "Out");

                            var stockTrans = context.StockBatchTransactions.Where(x => x.ObjectId == movementDetailStock.Id && x.IsActive).ToList();

                            stockBatchMaster.Qty += movementDetailStock.InventoryCount;
                            stockBatchMaster.ModifiedBy = userid;
                            stockBatchMaster.ModifiedDate = DateTime.Now;
                            context.Entry(stockBatchMaster).State = EntityState.Modified;
                            StockBatchTransaction stockBatchTransaction = null;
                            //int virtualQty = stockTrans != null && stockTrans.Any() ? stockTrans.Where(x => x.Qty > 0).Sum(x => x.Qty) : 0;
                            string stockreason = movementDetailStock.ToStockType + " Movement Request Date:" + movementDetailStock.CreatedDate.ToString("dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture) + " Id:" + movementDetailStock.Id;
                            int? virtualQty = null;
                            if (stockType == 7)
                            {
                                var maualstocks = context.ManualStockUpdateRequestDB.Where(x => x.ItemMultiMRPId == currentstock.ItemMultiMRPId && x.WarehouseId == currentstock.WarehouseId && x.Reason == stockreason);

                                if (maualstocks != null && maualstocks.Any())
                                    virtualQty = maualstocks.Sum(x => x.Qty);
                            }
                            //else if (stockType == 9)//NoNRevenus Stock
                            //{
                            //    var maualstocks = context.ManualStockUpdateRequestDB.Where(x => x.ItemMultiMRPId == nonRevenueOrderStock.ItemMultiMRPId && x.WarehouseId == nonRevenueOrderStock.WarehouseId && x.Reason == stockreason);

                            //    if (maualstocks != null && maualstocks.Any())
                            //        virtualQty = maualstocks.Sum(x => x.Qty);
                            //}
                            else
                            {
                                var maualstocks = context.ManualStockUpdateRequestDB.Where(x => x.ItemMultiMRPId == clearanceStockNew.ItemMultiMRPId && x.WarehouseId == clearanceStockNew.WarehouseId && x.Reason == stockreason);

                                if (maualstocks != null && maualstocks.Any())
                                    virtualQty = maualstocks.Sum(x => x.Qty);
                            }

                            foreach (var trans in stockTrans)
                            {
                                stockBatchTransaction = new StockBatchTransaction
                                {
                                    StockTxnTypeId = stockTxnTypes.FirstOrDefault(x => x.Id != trans.Id).Id,
                                    CreatedBy = userid,
                                    CreatedDate = indianTime,
                                    IsActive = true,
                                    IsDeleted = false,
                                    ObjectId = movementDetailStock.Id,
                                    ObjectDetailId = 0,
                                    StockBatchMasterId = stockBatchMaster.Id,
                                    TransactionDate = indianTime,
                                    Qty = trans.Qty
                                };
                                context.StockBatchTransactions.Add(stockBatchTransaction);
                            }
                            if (stockType != 9)
                            {
                                ReservedStock reservedStock = new ReservedStock
                                {
                                    CreatedBy = People.PeopleID,
                                    CreatedDate = DateTime.Now,
                                    EntityId = Convert.ToInt32(movementDetailStock.Id),
                                    EntityType = "MovementDetailStock",
                                    InOutQty = (-1) * movementDetailStock.InventoryCount,
                                    IsActive = true,
                                    IsDeleted = false,
                                    ItemMultiMRPId = movementDetailStock.ItemMultiMrpId,
                                    RefStockCode = stockType == 7 ? "C" : "CL",
                                    WarehouseId = movementDetailStock.WarehouseId
                                };
                                context.ReservedStockDB.Add(reservedStock);
                            }
                            //else if (stockType == 9)
                            //{
                            //    ReservedStock reservedStock = new ReservedStock
                            //    {
                            //        CreatedBy = People.PeopleID,
                            //        CreatedDate = DateTime.Now,
                            //        EntityId = Convert.ToInt32(movementDetailStock.Id),
                            //        EntityType = "MovementDetailStock",
                            //        InOutQty = (-1) * movementDetailStock.InventoryCount,
                            //        IsActive = true,
                            //        IsDeleted = false,
                            //        ItemMultiMRPId = movementDetailStock.ItemMultiMrpId,
                            //        RefStockCode = "NR",
                            //        WarehouseId = movementDetailStock.WarehouseId
                            //    };
                            //    context.ReservedStockDB.Add(reservedStock);
                            //}

                            if (stockType == 7)
                            {
                                currentstock.CurrentInventory += movementDetailStock.InventoryCount;
                                currentstock.UpdateBy = People.DisplayName;
                                currentstock.UpdatedDate = DateTime.Now;
                                context.Entry(currentstock).State = EntityState.Modified;
                                context.Commit();

                                CurrentStockHistory Oss1 = new CurrentStockHistory();
                                Oss1.StockId = currentstock.StockId;
                                Oss1.ItemNumber = currentstock.ItemNumber;
                                Oss1.itemname = currentstock.itemname;
                                Oss1.ItemMultiMRPId = currentstock.ItemMultiMRPId;
                                Oss1.ManualInventoryIn = movementDetailStock.InventoryCount;
                                Oss1.CurrentInventory = (currentstock.CurrentInventory);
                                Oss1.TotalInventory = (currentstock.CurrentInventory);
                                Oss1.ManualReason = movementDetailStock.ToStockType + " Movement Return";
                                Oss1.UOM = "Pc";
                                Oss1.WarehouseName = currentstock.WarehouseName;
                                Oss1.Warehouseid = currentstock.WarehouseId;
                                Oss1.CompanyId = currentstock.CompanyId;
                                Oss1.userid = People.PeopleID;
                                Oss1.UserName = currentstock.UpdateBy;
                                Oss1.CreationDate = currentstock.UpdatedDate;
                                context.CurrentStockHistoryDb.Add(Oss1);
                            }
                            //else if (stockType == 9) //NonReveue Stock
                            //{
                            //    nonRevenueOrderStock.NonRevenueInventory += movementDetailStock.InventoryCount;
                            //    nonRevenueOrderStock.ModifiedBy = People.PeopleID;
                            //    nonRevenueOrderStock.ModifiedDate = DateTime.Now;
                            //    context.Entry(nonRevenueOrderStock).State = EntityState.Modified;
                            //    context.Commit();

                            //    NonRevenueOrderStockHistory Oss1 = new NonRevenueOrderStockHistory();
                            //    Oss1.Id = currentstock.StockId;
                            //    Oss1.ItemNumber = currentstock.ItemNumber;
                            //    Oss1.ItemName = currentstock.itemname;
                            //    Oss1.ItemMultiMRPId = currentstock.ItemMultiMRPId;
                            //    Oss1.InwordQty = movementDetailStock.InventoryCount;
                            //    Oss1.NonRevenueInventory = (currentstock.CurrentInventory);
                            //    //Oss1.TotalInventory = (currentstock.CurrentInventory);
                            //    Oss1.ReasonToTransfer = movementDetailStock.ToStockType + " Movement Return";
                            //    //Oss1.UOM = "Pc";
                            //    //Oss1.WarehouseName = currentstock.WarehouseName;
                            //    Oss1.WarehouseId = currentstock.WarehouseId;
                            //    //Oss1.CompanyId = currentstock.CompanyId;
                            //    //Oss1.userid = People.PeopleID;
                            //    //Oss1.UserName = currentstock.UpdateBy;
                            //    Oss1.CreatedDate = currentstock.UpdatedDate;
                            //    context.NonRevenueOrderStockHistories.Add(Oss1);
                            //}
                            else
                            {
                                clearanceStockNew.Inventory += movementDetailStock.InventoryCount;
                                clearanceStockNew.ModifiedBy = People.PeopleID;
                                clearanceStockNew.UpdatedDate = DateTime.Now;
                                context.Entry(clearanceStockNew).State = EntityState.Modified;
                                context.Commit();

                                ClearanceStockNewHistory Oss1 = new ClearanceStockNewHistory();
                                Oss1.ClearanceStockId = clearanceStockNew.ClearanceStockId;
                                Oss1.ItemMultiMRPId = clearanceStockNew.ItemMultiMRPId;
                                Oss1.Inventory = movementDetailStock.InventoryCount;
                                Oss1.NetInventory = (clearanceStockNew.Inventory);
                                Oss1.Comment = movementDetailStock.ToStockType + " Movement Return";
                                Oss1.WarehouseId = clearanceStockNew.WarehouseId;
                                Oss1.CreatedBy = People.PeopleID;
                                Oss1.CreatedDate = DateTime.Now;
                                Oss1.IsActive = true;
                                Oss1.IsDeleted = false;
                                context.ClearanceStockNewHistoryDB.Add(Oss1);
                            }



                            if (virtualQty.HasValue && virtualQty.Value > 0)
                            {
                                StockTransactionHelper sthelper = new StockTransactionHelper();
                                List<ManualStockUpdateRequestDc> manualStockUpdateDcList = new List<ManualStockUpdateRequestDc>();
                                ManualStockUpdateRequestDc manualStockUpdateDc = new ManualStockUpdateRequestDc
                                {
                                    ItemMultiMRPId = stockType == 7 ? currentstock.ItemMultiMRPId : clearanceStockNew.ItemMultiMRPId,
                                    Reason = movementDetailStock.ToStockType + " Movement Request Date:" + indianTime.ToString("dd/MM/yyyy"),
                                    StockTransferType = "ManualInventory",
                                    Qty = virtualQty.Value,
                                    WarehouseId = stockType == 7 ? currentstock.WarehouseId : clearanceStockNew.WarehouseId,
                                    DestinationStockType = StockTypeTableNames.VirtualStock,
                                    SourceStockType = stockType == 7 ? StockTypeTableNames.CurrentStocks : StockTypeTableNames.ClearanceStockNews,
                                    Status = "Pending"
                                };
                                manualStockUpdateDcList.Add(manualStockUpdateDc);

                                bool isSuccess = sthelper.ManualStockUpdate(manualStockUpdateDcList, People.PeopleID, context, scope);
                                if (!isSuccess)
                                {
                                    msgresult.Message = movementDetailStock.ToStockType + " Movement request not process please try after some time.";
                                    return msgresult;
                                }
                                else
                                {
                                    if (stockBatchMaster != null)
                                    {
                                        if (stockBatchMaster.Qty >= virtualQty.Value)
                                            stockBatchMaster.Qty -= virtualQty.Value;
                                        else
                                            stockBatchMaster.Qty = 0;

                                        context.Entry(stockBatchMaster).State = EntityState.Modified;
                                    }
                                }
                            }

                        }
                        context.Entry(movementDetailStock).State = EntityState.Modified;
                        msgresult.Status = context.Commit() > 0;

                        if (msgresult.Status)
                        {
                            msgresult.Message = "Stock transfer request " + (Status ? "Approved" : "Rejected") + " successfully.";
                            scope.Complete();
                            DamageRequestIds.RemoveAll(x => x == requestId);
                        }
                        else
                        {
                            msgresult.Message = "Due to some issue please try after some time.";
                            scope.Dispose();
                            DamageRequestIds.RemoveAll(x => x == requestId);
                        }
                    }
                    else
                    {
                        DamageRequestIds.RemoveAll(x => x == requestId);
                        msgresult.Message = "Request Stock not found";
                        msgresult.Status = false;
                    }
                }
            }

            return msgresult;
        }
        private void MovmentStockFinalApproved(int requestId, int userId, AuthContext context)
        {
            string stocktype = "";
            var movementDetailStock = context.MovementDetailStockDB.FirstOrDefault(x => x.Id == requestId);
            var currentstock = context.DbCurrentStock.FirstOrDefault(c => c.ItemMultiMRPId == movementDetailStock.ItemMultiMrpId && c.Deleted == false && c.WarehouseId == movementDetailStock.WarehouseId);
            var currentStockBatchMaster = context.StockBatchMasters.FirstOrDefault(x => x.Id == movementDetailStock.StockBatchMasterId);

            var stockTxnType = context.StockTxnTypeMasters.FirstOrDefault(x => x.StockTxnType == movementDetailStock.ToStockType + "In");

            if (movementDetailStock.ToStockType == "DamageStock")
            {
                stocktype = "D";
            }
            else if (movementDetailStock.ToStockType == "NonSellable")
            {
                stocktype = "N";
            }
            else if (movementDetailStock.ToStockType == "NonRevenueStock")
            {
                stocktype = "NR";
            }
            ReservedStock reservedStock = new ReservedStock
            {
                CreatedBy = userId,
                CreatedDate = DateTime.Now,
                EntityId = Convert.ToInt32(movementDetailStock.Id),
                EntityType = "MovementDetailStock",
                InOutQty = (-1) * movementDetailStock.InventoryCount,
                IsActive = true,
                IsDeleted = false,
                ItemMultiMRPId = movementDetailStock.ItemMultiMrpId,
                RefStockCode = "C",
                WarehouseId = movementDetailStock.WarehouseId
            };
            context.ReservedStockDB.Add(reservedStock);
            if (movementDetailStock.ToStockType == "DamageStock")
            {
                DamageStock dst = context.DamageStockDB.Where(x => x.WarehouseId == movementDetailStock.WarehouseId && x.ItemMultiMRPId == movementDetailStock.ItemMultiMrpId).FirstOrDefault();
                if (dst == null)
                {

                    double UnitPrice = 0;
                    double PurchasePrice = 0;

                    int ItemId = 0;

                    var itemmaster = context.itemMasters.Where(x => x.Number == currentstock.ItemNumber && x.WarehouseId == movementDetailStock.WarehouseId && x.PurchasePrice > 0).FirstOrDefault();
                    if (itemmaster != null)
                    {
                        UnitPrice = Math.Round(itemmaster.UnitPrice, 2);
                        PurchasePrice = Math.Round(itemmaster.PurchasePrice, 2);
                        ItemId = itemmaster.ItemId;
                    }

                    dst = new DamageStock();
                    dst.WarehouseId = movementDetailStock.WarehouseId;
                    dst.WarehouseName = currentstock.WarehouseName;
                    dst.ItemId = ItemId;
                    dst.MRP = currentstock.MRP;
                    dst.ItemMultiMRPId = currentstock.ItemMultiMRPId;
                    dst.ItemNumber = currentstock.ItemNumber;
                    dst.ItemName = currentstock.itemname;
                    dst.DamageInventory = movementDetailStock.InventoryCount;
                    dst.UnitPrice = UnitPrice;
                    dst.PurchasePrice = PurchasePrice;//we set PurchasePrice 
                    dst.ReasonToTransfer = movementDetailStock.Comment;
                    dst.CreatedDate = indianTime;
                    dst.CompanyId = currentstock.CompanyId;
                    context.DamageStockDB.Add(dst);
                    context.Commit();
                }
                else
                {
                    dst.DamageInventory += movementDetailStock.InventoryCount;
                    dst.ReasonToTransfer = movementDetailStock.Comment;
                    dst.UpdatedDate = indianTime;
                    context.Entry(dst).State = EntityState.Modified;
                }


                var DSH = new DamageStockHistory();
                DSH.CompanyId = dst.CompanyId;
                DSH.CreatedDate = indianTime;
                DSH.DamageInventory = dst.DamageInventory;
                DSH.DamageStockId = dst.DamageStockId;
                DSH.Deleted = false;
                DSH.InwordQty = movementDetailStock.InventoryCount;
                DSH.itemBaseName = dst.itemBaseName;
                DSH.ItemId = dst.ItemId;
                DSH.ItemMultiMRPId = dst.ItemMultiMRPId;
                DSH.ItemName = dst.ItemName;
                DSH.ItemNumber = dst.ItemNumber;
                DSH.MRP = dst.MRP;
                DSH.OutwordQty = 0;
                DSH.ReasonToTransfer = dst.ReasonToTransfer;
                DSH.UnitofQuantity = dst.UnitofQuantity;
                DSH.UnitPrice = dst.UnitPrice;
                DSH.UOM = dst.UOM;
                DSH.UpdatedDate = indianTime;
                DSH.WarehouseId = dst.WarehouseId;
                DSH.WarehouseName = dst.WarehouseName;
                DSH.CreatedBy = userId;
                context.DamageStockHistoryDB.Add(DSH);

                var stockBatchMaster = context.StockBatchMasters.FirstOrDefault(x => x.StockId == dst.DamageStockId && x.BatchMasterId == currentStockBatchMaster.BatchMasterId && x.StockType == stocktype);
                if (stockBatchMaster == null)
                {
                    stockBatchMaster = new StockBatchMaster
                    {
                        BatchMasterId = currentStockBatchMaster.BatchMasterId,
                        CreatedBy = userId,
                        CreatedDate = indianTime,
                        IsActive = true,
                        IsDeleted = false,
                        Qty = movementDetailStock.InventoryCount,
                        StockId = dst.DamageStockId,
                        StockType = stocktype
                    };
                    context.StockBatchMasters.Add(stockBatchMaster);
                }
                else
                {
                    stockBatchMaster.Qty += movementDetailStock.InventoryCount;
                    stockBatchMaster.ModifiedBy = userId;
                    stockBatchMaster.ModifiedDate = DateTime.Now;
                    context.Entry(stockBatchMaster).State = EntityState.Modified;
                }
                StockBatchTransaction stockBatchTransaction = new StockBatchTransaction
                {
                    StockTxnTypeId = stockTxnType.Id,
                    CreatedBy = userId,
                    CreatedDate = indianTime,
                    IsActive = true,
                    IsDeleted = false,
                    ObjectId = movementDetailStock.Id,
                    ObjectDetailId = 0,
                    StockBatchMasterId = stockBatchMaster.Id,
                    TransactionDate = indianTime,
                    Qty = movementDetailStock.InventoryCount
                };
                context.StockBatchTransactions.Add(stockBatchTransaction);
            }
            else if (movementDetailStock.ToStockType == "NonSellable")
            {
                NonSellableStock nonSellableStock = context.NonSellableStockDB.Where(x => x.WarehouseId == movementDetailStock.WarehouseId && x.ItemMultiMRPId == movementDetailStock.ItemMultiMrpId).FirstOrDefault();
                if (nonSellableStock == null)
                {
                    nonSellableStock = new NonSellableStock();
                    nonSellableStock.Comment = "";
                    nonSellableStock.CreatedBy = userId;
                    nonSellableStock.CreatedDate = indianTime;
                    nonSellableStock.Inventory = movementDetailStock.InventoryCount;
                    nonSellableStock.IsActive = true;
                    nonSellableStock.IsDeleted = false;
                    nonSellableStock.ItemMultiMRPId = currentstock.ItemMultiMRPId;
                    nonSellableStock.ItemName = currentstock.itemname;
                    nonSellableStock.WarehouseId = movementDetailStock.WarehouseId;
                    context.NonSellableStockDB.Add(nonSellableStock);
                    context.Commit();
                }
                else
                {
                    nonSellableStock.Inventory += movementDetailStock.InventoryCount;
                    nonSellableStock.Comment = movementDetailStock.Comment;
                    nonSellableStock.UpdatedDate = indianTime;
                    nonSellableStock.ModifiedBy = userId;
                    context.Entry(nonSellableStock).State = EntityState.Modified;
                }

                var existNonSellableStockData = context.NonSellableStockHistoryDB.Where(x => x.ItemMultiMRPId == nonSellableStock.ItemMultiMRPId && x.WarehouseId == nonSellableStock.WarehouseId).OrderByDescending(x => x.Id).FirstOrDefault();
                var DSH = new NonSellableStockHistory();

                DSH.CreatedDate = indianTime;
                DSH.Inventory = movementDetailStock.InventoryCount;
                DSH.NonSellableStockId = nonSellableStock.NonSellableStockId;
                DSH.IsDeleted = false;
                DSH.ItemMultiMRPId = nonSellableStock.ItemMultiMRPId;
                DSH.ItemName = nonSellableStock.ItemName;
                DSH.Comment = nonSellableStock.Comment;
                DSH.WarehouseId = nonSellableStock.WarehouseId;
                DSH.IsActive = true;
                DSH.CreatedBy = userId;
                DSH.NetInventory = existNonSellableStockData != null ? (existNonSellableStockData.NetInventory + movementDetailStock.InventoryCount) : movementDetailStock.InventoryCount;
                context.NonSellableStockHistoryDB.Add(DSH);

                var stockBatchMaster = context.StockBatchMasters.FirstOrDefault(x => x.StockId == nonSellableStock.NonSellableStockId && x.BatchMasterId == currentStockBatchMaster.BatchMasterId && x.StockType == stocktype);
                if (stockBatchMaster == null)
                {
                    stockBatchMaster = new StockBatchMaster
                    {
                        BatchMasterId = currentStockBatchMaster.BatchMasterId,
                        CreatedBy = userId,
                        CreatedDate = indianTime,
                        IsActive = true,
                        IsDeleted = false,
                        Qty = movementDetailStock.InventoryCount,
                        StockId = nonSellableStock.NonSellableStockId,
                        StockType = stocktype
                    };
                    context.StockBatchMasters.Add(stockBatchMaster);
                }
                else
                {
                    stockBatchMaster.Qty += movementDetailStock.InventoryCount;
                    stockBatchMaster.ModifiedBy = userId;
                    stockBatchMaster.ModifiedDate = DateTime.Now;
                    context.Entry(stockBatchMaster).State = EntityState.Modified;
                }

                StockBatchTransaction stockBatchTransaction = new StockBatchTransaction
                {
                    StockTxnTypeId = stockTxnType.Id,
                    CreatedBy = userId,
                    CreatedDate = indianTime,
                    IsActive = true,
                    IsDeleted = false,
                    ObjectId = movementDetailStock.Id,
                    ObjectDetailId = 0,
                    StockBatchMasterId = stockBatchMaster.Id,
                    TransactionDate = indianTime,
                    Qty = movementDetailStock.InventoryCount
                };
                context.StockBatchTransactions.Add(stockBatchTransaction);
            }
            else if (movementDetailStock.ToStockType == "NonRevenueStock")
            {
                NonRevenueOrderStock nonRevenueOrder = context.NonRevenueOrderStocks.Where(x => x.WarehouseId == movementDetailStock.WarehouseId && x.ItemMultiMRPId == movementDetailStock.ItemMultiMrpId).FirstOrDefault();

                var itemmaster = context.itemMasters.Where(x => x.Number == currentstock.ItemNumber && x.WarehouseId == movementDetailStock.WarehouseId && x.PurchasePrice > 0).FirstOrDefault();
                double UnitPrice = 0;
                double PurchasePrice = 0;

                int ItemId = 0;
                if (itemmaster != null)
                {
                    UnitPrice = Math.Round(itemmaster.UnitPrice, 2);
                    PurchasePrice = Math.Round(itemmaster.PurchasePrice, 2);
                    ItemId = itemmaster.ItemId;
                }

                if (nonRevenueOrder == null)
                {
                    nonRevenueOrder = new NonRevenueOrderStock();
                    // nonRevenueOrder.Comment = "";
                    nonRevenueOrder.CreatedBy = userId;
                    nonRevenueOrder.CreatedDate = indianTime;
                    nonRevenueOrder.NonRevenueInventory = movementDetailStock.InventoryCount;
                    nonRevenueOrder.IsActive = true;
                    nonRevenueOrder.IsDeleted = false;
                    nonRevenueOrder.ItemMultiMRPId = currentstock.ItemMultiMRPId;
                    nonRevenueOrder.ItemName = currentstock.itemname;
                    nonRevenueOrder.WarehouseId = movementDetailStock.WarehouseId;
                    nonRevenueOrder.MRP = currentstock.MRP;
                    nonRevenueOrder.UnitPrice = UnitPrice;
                    // nonRevenueOrder.PurchasePrice = PurchasePrice;
                    // nonRevenueOrder.ItemId = ItemId;
                    nonRevenueOrder.ItemNumber = currentstock.ItemNumber;
                    //nonRevenueOrder.itemBaseName = currentstock.itemBaseName;
                    // nonRevenueOrder.UnitofQuantity = itemmaster.UnitofQuantity;
                    // nonRevenueOrder.UOM = itemmaster.UOM;
                    // nonRevenueOrder.ReasonToTransfer = movementDetailStock.Comment;

                    context.NonRevenueOrderStocks.Add(nonRevenueOrder);
                    context.Commit();
                }
                else
                {
                    nonRevenueOrder.NonRevenueInventory += movementDetailStock.InventoryCount;
                    //nonRevenueOrder.Comment = movementDetailStock.Comment;
                    nonRevenueOrder.ModifiedDate = indianTime;
                    nonRevenueOrder.ModifiedBy = userId;
                    context.Entry(nonRevenueOrder).State = EntityState.Modified;
                }

                var NSH = new NonRevenueOrderStockHistory();

                NSH.CreatedDate = indianTime;
                NSH.NonRevenueInventory = nonRevenueOrder.NonRevenueInventory;
                NSH.NonRevenueStockId = nonRevenueOrder.Id;
                NSH.IsDeleted = false;
                NSH.ItemMultiMRPId = nonRevenueOrder.ItemMultiMRPId;
                NSH.ItemNumber = nonRevenueOrder.ItemNumber;
                //NSH.PurchasePrice = nonRevenueOrder.PurchasePrice;
                NSH.ReasonToTransfer = movementDetailStock.Comment;
                NSH.ItemName = nonRevenueOrder.ItemName;
                // NSH.Comment = nonRevenueOrder.Comment;
                NSH.WarehouseId = nonRevenueOrder.WarehouseId;
                NSH.IsActive = true;
                NSH.CreatedBy = userId;
                NSH.OutwordQty = 0;
                NSH.InwordQty = movementDetailStock.InventoryCount;
                context.NonRevenueOrderStockHistories.Add(NSH);

                var stockBatchMaster = context.StockBatchMasters.FirstOrDefault(x => x.StockId == nonRevenueOrder.Id && x.BatchMasterId == currentStockBatchMaster.BatchMasterId && x.StockType == stocktype);
                if (stockBatchMaster == null)
                {
                    stockBatchMaster = new StockBatchMaster
                    {
                        BatchMasterId = currentStockBatchMaster.BatchMasterId,
                        CreatedBy = userId,
                        CreatedDate = indianTime,
                        IsActive = true,
                        IsDeleted = false,
                        Qty = movementDetailStock.InventoryCount,
                        StockId = nonRevenueOrder.Id,
                        StockType = stocktype
                    };
                    context.StockBatchMasters.Add(stockBatchMaster);
                }
                else
                {
                    stockBatchMaster.Qty += movementDetailStock.InventoryCount;
                    stockBatchMaster.ModifiedBy = userId;
                    stockBatchMaster.ModifiedDate = DateTime.Now;
                    context.Entry(stockBatchMaster).State = EntityState.Modified;
                }

                StockBatchTransaction stockBatchTransaction = new StockBatchTransaction
                {
                    StockTxnTypeId = stockTxnType.Id,
                    CreatedBy = userId,
                    CreatedDate = indianTime,
                    IsActive = true,
                    IsDeleted = false,
                    ObjectId = movementDetailStock.Id,
                    ObjectDetailId = 0,
                    StockBatchMasterId = stockBatchMaster.Id,
                    TransactionDate = indianTime,
                    Qty = movementDetailStock.InventoryCount
                };
                context.StockBatchTransactions.Add(stockBatchTransaction);
            }
        }


        [Route("GetMovementStockForHQ")]
        [HttpPost]
        public ResponseMovementItems GetMovementStockForHQ(RequestMovementItem requestMovementItem)
        {
            List<LastMonthApp> lastMonthApps1 = new List<LastMonthApp>();
            List<ItemMasterPurchase> itemMasterPurchases = new List<ItemMasterPurchase>();
            ResponseMovementItems responseMovementItems = new ResponseMovementItems();
            var identity = User.Identity as ClaimsIdentity;
            int userid = 0, Warehouseid = 0;
            List<string> rolenames = null;
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "Warehouseid"))
                Warehouseid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "Warehouseid").Value);
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type.ToLower() == "rolenames"))
                rolenames = (identity.Claims.FirstOrDefault(x => x.Type.ToLower() == "rolenames").Value).Split(',').Select(x => x).ToList();

            MongoDbHelper<StockMovmentApproval> mongoDbHelper = new MongoDbHelper<StockMovmentApproval>();
            var stockMovmentPredicate = PredicateBuilder.New<StockMovmentApproval>(x => !string.IsNullOrEmpty(x.StockType));
            if (requestMovementItem.stockType != "All")
                stockMovmentPredicate = stockMovmentPredicate.And(x => x.StockType == requestMovementItem.stockType);
            var mongoStockMovmentApprovals = (mongoDbHelper.Select(stockMovmentPredicate)).ToList();


            var orderIdDt = new System.Data.DataTable();
            orderIdDt.Columns.Add("stringValue");
            if (mongoStockMovmentApprovals != null && mongoStockMovmentApprovals.Any())
            {
                var approvalRang = mongoStockMovmentApprovals.SelectMany(x => x.StockMovmentApprovalRanges).Select(x => x.ApprovalRole);
                foreach (var role in approvalRang)
                {
                    if (rolenames != null && rolenames.Contains(role))
                    {
                        var dr = orderIdDt.NewRow();
                        dr["stringValue"] = role;
                        orderIdDt.Rows.Add(dr);
                    }
                }

            }
            var param = new SqlParameter("userRoles", orderIdDt);
            param.SqlDbType = System.Data.SqlDbType.Structured;
            param.TypeName = "dbo.stringValues";

            var warehouseIdDt = new System.Data.DataTable();
            warehouseIdDt.Columns.Add("IntValue");
            if (requestMovementItem.warehouseId == null)
            {
                warehouseIdDt = null;
            }
            else
            {
                foreach (var wId in requestMovementItem.warehouseId)
                {
                    var dr = warehouseIdDt.NewRow();
                    dr["IntValue"] = wId;
                    warehouseIdDt.Rows.Add(dr);
                }
            }

            var Warehouseparam = new SqlParameter("warehouseId", warehouseIdDt);
            Warehouseparam.SqlDbType = System.Data.SqlDbType.Structured;
            Warehouseparam.TypeName = "dbo.IntValues";

            //WarehouseIds
            //var WarehouseIdDts = new System.Data.DataTable();
            //WarehouseIdDts.Columns.Add("IntValue");

            //if (requestMovementItem.warehouseId != null && requestMovementItem.warehouseId.Any())
            //{
            //    foreach (var item in requestMovementItem.warehouseId)
            //    {
            //        var dr = WarehouseIdDts.NewRow();
            //        dr["IntValue"] = item;
            //        WarehouseIdDts.Rows.Add(dr);
            //    }
            //}
            //var Warehouseparam = new SqlParameter("warehouseId", WarehouseIdDts);
            //Warehouseparam.SqlDbType = System.Data.SqlDbType.Structured;
            //Warehouseparam.TypeName = "dbo.IntValues";

            ////Status
            //var statusIdDt = new System.Data.DataTable();
            //statusIdDt.Columns.Add("stringValues");
            //if (requestMovementItem.status != null && requestMovementItem.status.Any())
            //{
            //    foreach (var item in requestMovementItem.status)
            //    {
            //        var dr = statusIdDt.NewRow();
            //        dr["stringValues"] = item;
            //        statusIdDt.Rows.Add(dr);
            //    }
            //}
            //var Statusparam = new SqlParameter("status", statusIdDt);
            //Statusparam.SqlDbType = System.Data.SqlDbType.Structured;
            //Statusparam.TypeName = "dbo.stringValues";

            using (AuthContext context = new AuthContext())
            {
                if (context.Database.Connection.State != System.Data.ConnectionState.Open)
                    context.Database.Connection.Open();

                if (!requestMovementItem.startDate.HasValue || !requestMovementItem.endDate.HasValue)
                {
                    requestMovementItem.startDate = new DateTime(2000, 01, 01);
                    requestMovementItem.endDate = new DateTime(2000, 01, 01);
                }

                if (string.IsNullOrEmpty(requestMovementItem.actiontype))
                    requestMovementItem.actiontype = "search";

                var cmd = context.Database.Connection.CreateCommand();
                cmd.CommandText = "[dbo].[GetMovementStockForHQ]";
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.Parameters.Add(Warehouseparam);
                cmd.Parameters.Add(new SqlParameter("@stockType", requestMovementItem.stockType));
                cmd.Parameters.Add(new SqlParameter("@fromStockType", requestMovementItem.fromStockType));
                cmd.Parameters.Add(new SqlParameter("@status", requestMovementItem.status));
                cmd.Parameters.Add(new SqlParameter("@startDate", requestMovementItem.startDate));
                cmd.Parameters.Add(new SqlParameter("@endDate", requestMovementItem.endDate));
                cmd.Parameters.Add(new SqlParameter("@skip", requestMovementItem.skip));
                cmd.Parameters.Add(new SqlParameter("@take", requestMovementItem.take));
                cmd.Parameters.Add(param);
                // cmd.Parameters.Add(Warehouseparam);
                // cmd.Parameters.Add(Statusparam);
                cmd.Parameters.Add(new SqlParameter("@actiontype", requestMovementItem.actiontype));

                var reader = cmd.ExecuteReader();
                var MovementItems = ((IObjectContextAdapter)context)
                                         .ObjectContext
                                         .Translate<MovementItems>(reader).ToList();
                reader.NextResult();
                if (reader.Read())
                {
                    responseMovementItems.totalRecord = Convert.ToInt32(reader["totalRecord"]);
                }
                if (MovementItems.Any())
                {
                    DateTime startDate = DateTime.Now.Date.AddDays(-10);
                    DateTime endDate = DateTime.Now;
                    var MrpIdDt = new System.Data.DataTable();
                    MrpIdDt.Columns.Add("IntValue");
                    foreach (var item in MovementItems)
                    {
                        var dr = MrpIdDt.NewRow();
                        dr["IntValue"] = item.ItemMultiMrpId;
                        MrpIdDt.Rows.Add(dr);
                    }

                    var mrpparam = new SqlParameter("itemmultimrpIds", MrpIdDt);
                    mrpparam.SqlDbType = System.Data.SqlDbType.Structured;
                    mrpparam.TypeName = "dbo.IntValues";

                    var warehouseIdDt1 = new System.Data.DataTable();
                    warehouseIdDt1.Columns.Add("IntValue");
                    if (requestMovementItem.warehouseId == null)
                    {
                        warehouseIdDt1 = null;
                    }
                    else
                    {
                        foreach (var wId in requestMovementItem.warehouseId)
                        {
                            var dr = warehouseIdDt1.NewRow();
                            dr["IntValue"] = wId;
                            warehouseIdDt1.Rows.Add(dr);
                        }
                    }

                    var warehouseParam1 = new SqlParameter("warehouseId", warehouseIdDt1);
                    warehouseParam1.SqlDbType = System.Data.SqlDbType.Structured;
                    warehouseParam1.TypeName = "dbo.IntValues";
                    // context.Database.CommandTimeout = 300;
                    cmd = context.Database.Connection.CreateCommand();
                    cmd.CommandTimeout = 1200;
                    cmd.CommandText = "[FIFO].[GetItemAPPsByMrpIdAndWarehouse]";
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.Parameters.Add(warehouseParam1);
                    cmd.Parameters.Add(mrpparam);
                    cmd.Parameters.Add(new SqlParameter("@startDate", startDate));
                    cmd.Parameters.Add(new SqlParameter("@enddate", endDate));

                    // Run the sproc
                    var readerApp = cmd.ExecuteReader();
                    lastMonthApps1 = ((IObjectContextAdapter)context)
                        .ObjectContext
                        .Translate<LastMonthApp>(readerApp).ToList();
                    //readerApp.NextResult();

                    //if (readerApp.HasRows)
                    //{
                    //    itemMasterPurchases = ((IObjectContextAdapter)context)
                    //                        .ObjectContext
                    //                        .Translate<ItemMasterPurchase>(readerApp).ToList();
                    //}
                    //readerApp.NextResult();
                    //if (readerApp.HasRows)
                    //{
                    //    lastMonthApps1 = ((IObjectContextAdapter)context)
                    //                        .ObjectContext
                    //                        .Translate<LastMonthApp>(readerApp).ToList();
                    //}                  
                    //var LastMonthApps = ((IObjectContextAdapter)context)
                    // .ObjectContext
                    // .Translate<LastPurchase>(readerApp).ToList();
                    //readerApp.NextResult();
                    //if (readerApp.HasRows)
                    //{
                    //    itemMasterPurchases = ((IObjectContextAdapter)context)
                    //                        .ObjectContext
                    //                        .Translate<ItemMasterPurchase>(readerApp).ToList();
                    //    readerApp.NextResult();
                    //    if (readerApp.HasRows)
                    //    {
                    //        lastMonthApps1 = ((IObjectContextAdapter)context)
                    //                            .ObjectContext
                    //                            .Translate<LastMonthApp>(readerApp).ToList();
                    //    }
                    //}


                    foreach (var item in MovementItems)
                    {
                        if (lastMonthApps1.Any(x => x.itemmultimrpid == item.ItemMultiMrpId && x.warehouseid == item.WarehouseId))
                        {
                            item.APP = lastMonthApps1.FirstOrDefault(x => x.itemmultimrpid == item.ItemMultiMrpId && x.warehouseid == item.WarehouseId).APP;
                        }

                        if (!string.IsNullOrEmpty(item.Imageurl))
                        {
                            item.Imageurl = string.Format("{0}://{1}:{2}/{3}", new Uri(HttpContext.Current.Request.Url.AbsoluteUri).Scheme
                                                                        , HttpContext.Current.Request.Url.DnsSafeHost
                                                                        , (HttpContext.Current.Request.Url.Port != 80 && HttpContext.Current.Request.Url.Port != 443 ? ":" + HttpContext.Current.Request.Url.Port : "")
                                                                        , item.Imageurl);
                        }
                    }
                }
                responseMovementItems.MovementItems = MovementItems;
            }

            return responseMovementItems;
        }

        [Route("InsertApprovalConfig")]
        [HttpGet]
        public bool InsertApprovalConfig()
        {
            MongoDbHelper<StockMovmentApproval> mongoDbHelper = new MongoDbHelper<StockMovmentApproval>();
            List<StockMovmentApprovalRange> stockMovmentApprovalRanges = new List<StockMovmentApprovalRange>();
            stockMovmentApprovalRanges.Add(new StockMovmentApprovalRange
            {
                ApprovalRole = "Banking Executives",
                FromAmount = 0,
                ToAmount = 25000,
            });
            stockMovmentApprovalRanges.Add(new StockMovmentApprovalRange
            {
                ApprovalRole = "Banking dept. lead",
                FromAmount = 25000,
                ToAmount = 100000,
            });
            stockMovmentApprovalRanges.Add(new StockMovmentApprovalRange
            {
                ApprovalRole = "Founder - GBO and GBU",
                FromAmount = 100000,
                ToAmount = 10000000,
            });

            StockMovmentApproval stockMovmentApproval = new StockMovmentApproval
            {
                StockType = "Damage",
                WarehouseApprovalRole = "Banking Associates",
                StockMovmentApprovalRanges = stockMovmentApprovalRanges
            };


            mongoDbHelper.Insert(stockMovmentApproval);

            StockMovmentApproval stockMovmentApproval1 = new StockMovmentApproval
            {
                StockType = "NonSellable",
                WarehouseApprovalRole = "Banking Associates",
                StockMovmentApprovalRanges = stockMovmentApprovalRanges
            };

            mongoDbHelper.Insert(stockMovmentApproval1);
            return true;

        }


        [Route("MovementStockImageUpload")]
        [HttpPost]
        [AllowAnonymous]
        public string MovementStockImageUpload()
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
                        if (!Directory.Exists(HttpContext.Current.Server.MapPath("~/MovementStock")))
                            Directory.CreateDirectory(HttpContext.Current.Server.MapPath("~/MovementStock"));

                        string extension = Path.GetExtension(httpPostedFile.FileName);

                        string fileName = httpPostedFile.FileName.Substring(0, httpPostedFile.FileName.LastIndexOf('.')) + DateTime.Now.ToString("ddMMyyyyHHmmss") + extension;

                        LogoUrl = Path.Combine(HttpContext.Current.Server.MapPath("~/MovementStock"), fileName);

                        httpPostedFile.SaveAs(LogoUrl);

                        AngularJSAuthentication.Common.Helpers.FileUploadHelper.Upload(fileName, "~/MovementStock", LogoUrl);

                        LogoUrl = "/MovementStock/" + fileName;
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error("Error in MovementStockImageUpload Method: " + ex.Message);
            }
            return LogoUrl;
        }
        #endregion

        #region Job For Auto Send Email or Reject of items Movememnt From Sarthi App on Last Day of the Month

        [Route("MonthlyPendingInventoryMovementItems")]
        [HttpGet]
        public bool AutoReject()
        {
            bool Result = false;
            DateTime today = DateTime.Now.Date;
            DateTime FirstDay = new DateTime(today.Year, today.Month, 1);
            DateTime LastDay = FirstDay.AddMonths(1).AddDays(-1);
            DateTime Last3rdDay = FirstDay.AddMonths(1).AddDays(-3);


            TransactionOptions option = new TransactionOptions();
            option.IsolationLevel = System.Transactions.IsolationLevel.RepeatableRead;
            option.Timeout = TimeSpan.FromSeconds(90);
            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, option))
            {
                using (AuthContext context = new AuthContext())
                {
                    List<MovementDetailStock> Data = new List<MovementDetailStock>();
                    Data = context.Database.SqlQuery<MovementDetailStock>("Exec MonthlyPendingInventoryMovementItems").ToList();

                    if (Data != null && today == Last3rdDay)
                    {
                        string ExcelSavePath = HttpContext.Current.Server.MapPath("~/ExcelGeneratePath/AutoRejectItems");
                        if (!Directory.Exists(ExcelSavePath))
                            Directory.CreateDirectory(ExcelSavePath);

                        DataTable dt = ClassToDataTable.CreateDataTable(Data);
                        string fileName = $"MonthlyPendingInventoryMovementItems_{DateTime.Now.ToString("yyyy-dd-MM")}.xlsx";
                        string filePath = Path.Combine(ExcelSavePath, fileName);

                        if (ExcelGenerator.DataTable_To_Excel(dt, "PendingInventoryMovementRequests", filePath))
                        {
                            string connectionString = ConfigurationManager.ConnectionStrings["AuthContext"].ConnectionString;
                            string To = "", From = "", Bcc = "";
                            DataTable emaildatatable = new DataTable();
                            using (var connection = new SqlConnection(connectionString))
                            {
                                using (var command = new SqlCommand("Select * from EmailRecipients where EmailType='AutoRejectItemMovementPendingRequest'", connection))
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
                            string subject = "";
                            string message = "";
                            if (!string.IsNullOrEmpty(To) && !string.IsNullOrEmpty(From))
                            {
                                bool Res = EmailHelper.SendMail(From, To, Bcc, subject, message, filePath);

                            }
                        }
                    }
                    if (today == LastDay)
                    {
                        foreach (var item in Data)
                        {
                            CurrentStock currentstock = new CurrentStock();
                            ClearanceStockNew clearanceStockNew = new ClearanceStockNew();

                            item.Status = 6; //6='Reject by HQLP'
                            item.ModifiedBy = 1;
                            item.ModifiedDate = DateTime.Now;
                            item.Comment = "Auto Rejected on Last day of the month.";

                            if (item.FromStockType == "Current")
                            {
                                currentstock = context.DbCurrentStock.FirstOrDefault(c => c.ItemMultiMRPId == item.ItemMultiMrpId && c.Deleted == false && c.WarehouseId == item.WarehouseId);
                            }
                            else
                            {
                                clearanceStockNew = context.ClearanceStockNewDB.FirstOrDefault(c => c.ItemMultiMRPId == item.ItemMultiMrpId && c.IsDeleted == false && c.WarehouseId == item.WarehouseId);
                            }

                            var stockBatchMaster = context.StockBatchMasters.FirstOrDefault(x => x.Id == item.StockBatchMasterId);
                            var stockTxnTypes = context.StockTxnTypeMasters.Where(x => x.StockTxnType.Contains(item.ToStockType));
                            var stockTxnTypeIn = stockTxnTypes.FirstOrDefault(x => x.StockTxnType == item.ToStockType + "In");
                            var stockTxnTypeOut = stockTxnTypes.FirstOrDefault(x => x.StockTxnType == item.ToStockType + "Out");
                            var stockTrans = context.StockBatchTransactions.Where(x => x.ObjectId == item.Id && x.IsActive).ToList();

                            stockBatchMaster.Qty += item.InventoryCount;
                            stockBatchMaster.ModifiedBy = 1;
                            stockBatchMaster.ModifiedDate = DateTime.Now;
                            context.Entry(stockBatchMaster).State = EntityState.Modified;

                            StockBatchTransaction stockBatchTransaction = null;
                            string stockreason = item.ToStockType + " Movement Request Date:" + item.CreatedDate.ToString("dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture) + " Id:" + item.Id;
                            int? virtualQty = null;
                            if (item.FromStockType == "Current")
                            {
                                var maualstocks = context.ManualStockUpdateRequestDB.Where(x => x.ItemMultiMRPId == currentstock.ItemMultiMRPId && x.WarehouseId == currentstock.WarehouseId && x.Reason == stockreason);

                                if (maualstocks != null && maualstocks.Any())
                                    virtualQty = maualstocks.Sum(x => x.Qty);
                            }
                            else
                            {
                                var maualstocks = context.ManualStockUpdateRequestDB.Where(x => x.ItemMultiMRPId == clearanceStockNew.ItemMultiMRPId && x.WarehouseId == clearanceStockNew.WarehouseId && x.Reason == stockreason);

                                if (maualstocks != null && maualstocks.Any())
                                    virtualQty = maualstocks.Sum(x => x.Qty);
                            }

                            foreach (var trans in stockTrans)
                            {
                                stockBatchTransaction = new StockBatchTransaction
                                {
                                    StockTxnTypeId = stockTxnTypes.FirstOrDefault(x => x.Id != trans.Id).Id,
                                    CreatedBy = 1,
                                    CreatedDate = indianTime,
                                    IsActive = true,
                                    IsDeleted = false,
                                    ObjectId = item.Id,
                                    ObjectDetailId = 0,
                                    StockBatchMasterId = stockBatchMaster.Id,
                                    TransactionDate = indianTime,
                                    Qty = trans.Qty
                                };
                                context.StockBatchTransactions.Add(stockBatchTransaction);
                            }

                            ReservedStock reservedStock = new ReservedStock
                            {
                                CreatedBy = 1,
                                CreatedDate = DateTime.Now,
                                EntityId = Convert.ToInt32(item.Id),
                                EntityType = "MovementDetailStock",
                                InOutQty = (-1) * item.InventoryCount,
                                IsActive = true,
                                IsDeleted = false,
                                ItemMultiMRPId = item.ItemMultiMrpId,
                                RefStockCode = item.FromStockType == "Current" ? "C" : "CL",
                                WarehouseId = item.WarehouseId
                            };
                            context.ReservedStockDB.Add(reservedStock);

                            if (item.FromStockType == "Current")
                            {
                                currentstock.CurrentInventory += item.InventoryCount;
                                currentstock.UpdateBy = "";
                                currentstock.UpdatedDate = DateTime.Now;
                                context.Entry(currentstock).State = EntityState.Modified;

                                CurrentStockHistory Oss1 = new CurrentStockHistory();
                                Oss1.StockId = currentstock.StockId;
                                Oss1.ItemNumber = currentstock.ItemNumber;
                                Oss1.itemname = currentstock.itemname;
                                Oss1.ItemMultiMRPId = currentstock.ItemMultiMRPId;
                                Oss1.ManualInventoryIn = item.InventoryCount;
                                Oss1.CurrentInventory = (currentstock.CurrentInventory);
                                Oss1.TotalInventory = (currentstock.CurrentInventory);
                                Oss1.ManualReason = item.ToStockType + " Movement Return";
                                Oss1.UOM = "Pc";
                                Oss1.WarehouseName = currentstock.WarehouseName;
                                Oss1.Warehouseid = currentstock.WarehouseId;
                                Oss1.CompanyId = currentstock.CompanyId;
                                Oss1.userid = 1;
                                Oss1.UserName = currentstock.UpdateBy;
                                Oss1.CreationDate = currentstock.UpdatedDate;
                                context.CurrentStockHistoryDb.Add(Oss1);
                                context.Commit();

                            }
                            else
                            {
                                clearanceStockNew.Inventory += item.InventoryCount;
                                clearanceStockNew.ModifiedBy = 1;
                                clearanceStockNew.UpdatedDate = DateTime.Now;
                                context.Entry(clearanceStockNew).State = EntityState.Modified;

                                ClearanceStockNewHistory Oss1 = new ClearanceStockNewHistory();
                                Oss1.ClearanceStockId = clearanceStockNew.ClearanceStockId;
                                Oss1.ItemMultiMRPId = clearanceStockNew.ItemMultiMRPId;
                                Oss1.Inventory = item.InventoryCount;
                                Oss1.NetInventory = (clearanceStockNew.Inventory);
                                Oss1.Comment = item.ToStockType + " Movement Return";
                                Oss1.WarehouseId = clearanceStockNew.WarehouseId;
                                Oss1.CreatedBy = 1;
                                Oss1.CreatedDate = DateTime.Now;
                                Oss1.IsActive = true;
                                Oss1.IsDeleted = false;
                                context.ClearanceStockNewHistoryDB.Add(Oss1);
                                context.Commit();

                            }

                            if (virtualQty.HasValue && virtualQty.Value > 0)
                            {
                                StockTransactionHelper sthelper = new StockTransactionHelper();
                                List<ManualStockUpdateRequestDc> manualStockUpdateDcList = new List<ManualStockUpdateRequestDc>();
                                ManualStockUpdateRequestDc manualStockUpdateDc = new ManualStockUpdateRequestDc
                                {
                                    ItemMultiMRPId = item.FromStockType == "Current" ? currentstock.ItemMultiMRPId : clearanceStockNew.ItemMultiMRPId,
                                    Reason = item.ToStockType + " Movement Request Date:" + indianTime.ToString("dd/MM/yyyy"),
                                    StockTransferType = "ManualInventory",
                                    Qty = virtualQty.Value,
                                    WarehouseId = item.FromStockType == "Current" ? currentstock.WarehouseId : clearanceStockNew.WarehouseId,
                                    DestinationStockType = StockTypeTableNames.VirtualStock,
                                    SourceStockType = item.FromStockType == "Current" ? StockTypeTableNames.CurrentStocks : StockTypeTableNames.ClearanceStockNews,
                                    Status = "Pending"
                                };
                                manualStockUpdateDcList.Add(manualStockUpdateDc);

                                bool isSuccess = sthelper.ManualStockUpdate(manualStockUpdateDcList, 1, context, scope);
                                if (!isSuccess)
                                {
                                    return Result;
                                }
                                else
                                {
                                    if (stockBatchMaster != null)
                                    {
                                        if (stockBatchMaster.Qty >= virtualQty.Value)
                                            stockBatchMaster.Qty -= virtualQty.Value;
                                        else
                                            stockBatchMaster.Qty = 0;

                                        context.Entry(stockBatchMaster).State = EntityState.Modified;
                                        Result = true;
                                    }
                                }
                            }
                            context.Entry(item).State = EntityState.Modified;
                            Result = true;
                        }

                        if (Result)
                        {
                            context.Commit();
                            scope.Complete();
                        }
                        else
                        {
                            scope.Dispose();
                        }
                    }

                }
            }
            return Result;
        }
        #endregion


    }



    #region MovementStock Dc


    public class StockMovmentApproval
    {
        [MongoDB.Bson.Serialization.Attributes.BsonId]
        public MongoDB.Bson.ObjectId Id { get; set; }
        public string StockType { get; set; }
        public string WarehouseApprovalRole { get; set; }
        public List<StockMovmentApprovalRange> StockMovmentApprovalRanges { get; set; }
    }

    public class StockMovmentApprovalRange
    {
        public int FromAmount { get; set; }
        public int ToAmount { get; set; }
        public string ApprovalRole { get; set; }
    }

    public class RequestMovementItem
    {
        public List<int> warehouseId { get; set; }
        public string stockType { get; set; }
        public string fromStockType { get; set; }
        public int status { get; set; }
        public DateTime? startDate { get; set; }
        public DateTime? endDate { get; set; }
        public int skip { get; set; }
        public int take { get; set; }

        public string actiontype { get; set; }
    }

    public class ResponseMovementItems
    {
        public int totalRecord { get; set; }
        public List<MovementItems> MovementItems { get; set; }
    }

    public class MovementItems
    {
        public long Id { get; set; }
        public string StockType { get; set; }
        public string FromStockType { get; set; }
        public int WarehouseId { get; set; }
        public string WarehouseName { get; set; }
        public string ItemName { get; set; }
        public string ItemNumber { get; set; }
        public double MRP { get; set; }
        public int ItemMultiMrpId { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public string BatchCode { get; set; }
        public DateTime? MFGDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public int RequestQty { get; set; }
        public string Status { get; set; }
        public string Comment { get; set; }
        public string Imageurl { get; set; }
        public string RequestBy { get; set; }
        public int AvailableQty { get; set; }
        public double APP { get; set; }
        public int PastInventory { get; set; }
        public string FinalApprovedBy { get; set; }
        public string StoreName { get; set; }
        public string CategoryName { get; set; }
        public string SubCategory { get; set; }
        public string SubsubCategory { get; set; }
    }

    public class MobileMovementItem
    {
        public string StockType { get; set; }
        public string ItemName { get; set; }
        public double MRP { get; set; }
        public int ItemMultiMRPId { get; set; }
        public DateTime Date { get; set; }
        public string BatchCode { get; set; }
        public DateTime? MFGDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public int InventoryCount { get; set; }
        public string Status { get; set; }
        public string FromStockType { get; set; }
        public string ToStockType { get; set; }
    }

    public class MovementResponse
    {
        public bool Status { get; set; }
        public string Msg { get; set; }
        public List<BarcodeItemWithBatchDc> BarcodeItemWithBatchDcs { get; set; }
        //public List<BarcodeItemWithBatchClearanceWiseDc> BarcodeItemWithBatchClearanceWiseDcs { get; set; }
    }
    public class PVScanResponse
    {
        //public bool Status { get; set; }
        //public string Msg { get; set; }
        public List<BarcodeItemWithBatchDcList> BarcodeItemWithBatchDcs { get; set; }
    }

    public class MovementItemBatchDc
    {
        public int StockType { get; set; }
        public int FromStockType { get; set; }
        public int WarehouseId { get; set; }
        public int PeopleId { get; set; }
        public int ItemMultiMrpId { get; set; }
        public string ItemNumber { get; set; }
        public double MRP { get; set; }
        public string ItemName { get; set; }
        public long BatchMasterId { get; set; }
        public string BatchCode { get; set; }
        public DateTime? MFGDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public int InventoryCount { get; set; }
        public string ImageUrl { get; set; }
        public string Comment { get; set; }
    }

    public class BarcodeItemWithBatchDc
    {
        public int ItemMultiMrpId { get; set; }
        public string ItemNumber { get; set; }
        public double MRP { get; set; }
        public string ItemName { get; set; }
        public string ABCClassification { get; set; }
        public List<StockBatchDc> StockBatchDcs { get; set; }
        public List<StockBatchCLDc> StockBatchCLDcs { get; set; }
    }
    public class BarcodeItemWithBatchClearanceWiseDc
    {
        public int ItemMultiMrpId { get; set; }
        public string ItemNumber { get; set; }
        public double MRP { get; set; }
        public string ItemName { get; set; }
        public string ABCClassification { get; set; }
        public List<StockBatchCLDc> StockBatchCLDcs { get; set; }
    }

    public class StockBatchCLDc
    {
        public long StockBatchId { get; set; }
        public int ItemMultiMrpId { get; set; }
        public long BatchMasterId { get; set; }
        public string BatchCode { get; set; }
        public string ItemNumber { get; set; }
        public DateTime? MFGDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public int Qty { get; set; }
    }

    public class StockBatchDc
    {
        public long StockBatchId { get; set; }
        public int ItemMultiMrpId { get; set; }
        public long BatchMasterId { get; set; }
        public string BatchCode { get; set; }
        public string ItemNumber { get; set; }
        public DateTime? MFGDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public int Qty { get; set; }
    }

    public class BarcodeItemWithBatchDcList
    {
        public int Id { get; set; }
        public int ItemMultiMrpId { get; set; }
        public string ItemNumber { get; set; }
        public double MRP { get; set; }
        public string ItemName { get; set; }
        public string ABCClassification { get; set; }
        public List<string> Barcode { get; set; }
        public int status { get; set; }
        public List<StockBatchDc> BatchDcs { get; set; }
        public List<ItemBatchDc> ItemBatch { get; set; }

        public List<ItemBatchDc> AllItemBatch { get; set; }
    }
    public class ItemBatchDc
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
        public int ItemMultiMRPId { get; set; }
        public int AvailableQty { get; set; }
    }
    #endregion

    public class DamageStockDc
    {
        public int EntityId { get; set; }
        public int DamageStockId { get; set; }
        public int WarehouseId { get; set; }
        public string WarehouseName { get; set; }
        public int StockId { get; set; }
        public string ItemNumber { get; set; }
        public string ItemName { get; set; }
        public string ReasonToTransfer { get; set; }
        public int DamageInventory { get; set; }
        public int ItemMultiMRPId { get; set; }

        public string ABCClassification { get; set; }
        public int Stocktype { get; set; }
        public int TotalTaxPercentage { get; set; }
        public double AveragePurchasePrice { get; set; }
        public long StockBatchMasterId { get; set; }
    }

    public class DamageStockItemDc
    {
        public int StockId { get; set; }
        public string ItemName { get; set; }
        public string ItemNumber { get; set; }
        public int ItemMultiMRPId { get; set; }
        public double MRP { get; set; }
        public int CurrentInventory { get; set; }
        public string Cateogry { get; set; }
        public string ABCClassification { get; set; }
        public int WarehouseId { get; set; }
        public int Qty { get; set; }
        public string BatchCode { get; set; }
        public long StockBatchMasterId { get; set; }
        public DateTime? MFGDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
    }
    public class DamageAprprover
    {
        public int PeopleId { get; set; }
        public string DisplayName { get; set; }
    }
    public class PurchaseOrderMasterDTOs
    {

        public int EntityId { get; set; }
        public int Id { get; set; }
        public int WarehouseId { get; set; }
        public int ItemMultiMRPId { get; set; }
        public string ReasonToTransfer { get; set; }
        public string ItemNumber { get; set; }
        public string itemName { get; set; }
        public string WarehouseName { get; set; }
        public Double Amount { get; set; }
        public int Qty { get; set; }
        public DateTime CreatedDate { get; set; }
        public int Action { get; set; }//(0=Pending, 1= Accept , 2 = Reject)
        public string Comment { get; set; }
        public string BatchCode { get; set; }
        public long StockBatchMasterId { get; set; }
        public DateTime MFGDate { get; set; }
        public DateTime ExpiryDate { get; set; }
    }
    public class DamageResult
    {
        public bool Status { get; set; }
        public string Message { get; set; }
    }
    public class RejectDamge
    {
        //  public bool check { get; set; }
        public string Comment { get; set; }
        public int Id { get; set; }
    }
    public class FilterOrderDataDTOs
    {
        public int ItemPerPage { get; set; }
        public int PageNo { get; set; }
        public int WarehouesId { get; set; }
    }
    public class warehouesDTO
    {
        public int Wwarehouseid { get; set; }
    }

    // Added by Anoop on 4/3/2021
    public class DamageStockHistoryAll
    {
        [Key]
        public int Id { get; set; }
        public int DamageStockId { get; set; }
        public int CompanyId { get; set; }
        public int WarehouseId { get; set; }
        public string WarehouseName { get; set; }
        public int ItemId { get; set; }
        public string ItemNumber { get; set; }
        public string ItemName { get; set; }
        public double? UnitPrice { get; set; }
        public double? PurchasePrice { get; set; }
        public string ReasonToTransfer { get; set; }
        public int DamageInventory { get; set; }
        public int InwordQty { get; set; }
        public int OutwordQty { get; set; }

        public int? OdOrPoId { get; set; }

        public bool Deleted { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public string itemBaseName { get; set; }
        public int ItemMultiMRPId { get; set; }
        public double MRP { get; set; }
        public string UnitofQuantity { get; set; }
        public string UOM { get; set; }//Unit of masurement like GM Kg 
        public int CreatedBy { get; set; }

        [NotMapped]
        public string ABCClassification { get; set; }

        [NotMapped]
        public string UserName { get; set; }
    }

    public class ClearanceAprprover
    {
        public int PeopleId { get; set; }
        public string DisplayName { get; set; }
    }

}
