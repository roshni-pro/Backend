using AngularJSAuthentication.API.Helper;
using AngularJSAuthentication.BusinessLayer.Managers.Reports;
using AngularJSAuthentication.Common.Helpers;
using AngularJSAuthentication.DataContracts.constants;
using AngularJSAuthentication.DataContracts.Shared;
using AngularJSAuthentication.DataContracts.Transaction.Ledger.ItemLedger;
using AngularJSAuthentication.DataContracts.Transaction.Stocks;
using AngularJSAuthentication.Model;
using NLog;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Transactions;
using System.Web.Http;

namespace AngularJSAuthentication.API.Controllers
{
    [RoutePrefix("api/InventoryEditController")]
    public class InventoryEditController : ApiController
    {
        private static TimeZoneInfo INDIAN_ZONE = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
        DateTime indianTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE);
        private static Logger logger = LogManager.GetCurrentClassLogger();

        #region Warehouse and SearchKey based get item List in current stock
        /// <summary>
        /// Created Date:28/01/2020
        /// Created by Vinayak
        /// </summary>
        /// <param name="WarehouseId"></param>
        /// <returns></returns>

        [Route("getItemlistCurrentStock")]
        [HttpGet]
        public async Task<List<CurrentStockitemDTO>> getItemlistCurrentStock(int WarehouseId, string key)
        {
            logger.Info("start current stock: ");
            return null;
            // List<CurrentStock> ass = new List<CurrentStock>();

            //using (AuthContext db = new AuthContext())
            //{
            //    string msg = null;
            //    if (WarehouseId > 0)
            //    {
            //        var data = db.DbCurrentStock.Where(x => x.WarehouseId == WarehouseId && x.Deleted == false && (key == null || (x.itemname.Contains(key)) || (x.ItemNumber.Contains(key)))).ToList();
            //        var NumberList = data.Select(x => x.ItemNumber).Distinct().ToList();

            //        var tempitemsList = db.itemMasters.Where(x => NumberList.Contains(x.Number) && x.WarehouseId == WarehouseId && x.Deleted == false && x.PurchasePrice > 0).ToList();

            //        var itemsList = tempitemsList.GroupBy(x => x.Number).Select(x => x.First()).ToList();

            //        if (itemsList != null)
            //        {
            //            var result = (from p in data
            //                          join im in itemsList on
            //                          p.ItemNumber equals im.Number
            //                          select new CurrentStockitemDTO
            //                          {
            //                              itemname = p.itemname,
            //                              ItemNumber = p.ItemNumber,
            //                              ItemMultiMRPId = p.ItemMultiMRPId,
            //                              CurrentInventory = p.CurrentInventory,
            //                              PurchasePrice = im.PurchasePrice,
            //                              MRP = p.MRP,
            //                              StockId = p.StockId,
            //                              WarehouseId = p.WarehouseId,
            //                              TotalAmount = (im.PurchasePrice * p.CurrentInventory),
            //                          }).ToList();
            //            List<ItemClassificationDC> ABCitemsList = result.Select(item => new ItemClassificationDC { ItemNumber = item.ItemNumber, WarehouseId = item.WarehouseId }).ToList();

            //            var manager = new ItemLedgerManager();
            //            var GetItem = await manager.GetItemClassificationsAsync(ABCitemsList);
            //            foreach (var item in result)
            //            {

            //                if (GetItem != null && GetItem.Any())
            //                {
            //                    if (GetItem.Any(x => x.ItemNumber == item.ItemNumber))
            //                    {
            //                        item.ABC_Classification = GetItem.FirstOrDefault(x => x.ItemNumber == item.ItemNumber).Category;
            //                    }
            //                    else { item.ABC_Classification = "D"; }
            //                }
            //                else { item.ABC_Classification = "D"; }

            //            }

            //            return result;
            //        }
            //        else
            //        {
            //            return null;
            //        }
            //    }
            //    else
            //    {
            //        return null;
            //    }
            //}

        }
        #endregion

        [Route("addStock")]
        [AcceptVerbs("POST")]
        public HttpResponseMessage add(List<InventoryEditStockDTO> item)
        {

            logger.Info("start add Inventory: ");
            using (AuthContext context = new AuthContext())
            {
                try
                {
                    var identity = User.Identity as ClaimsIdentity;
                    int userid = 0;

                    if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                        userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

                    if (item[0].WarehouseId > 0)
                    {
                        var people = context.Peoples.Where(x => x.PeopleID == userid).FirstOrDefault();
                        int Warehouse_id = item[0].WarehouseId;
                        List<int> Stockids = item.Select(x => x.StockId).ToList();
                        List<CurrentStock> stocks = context.DbCurrentStock.Where(c => Stockids.Contains(c.StockId) && c.Deleted == false && c.WarehouseId == Warehouse_id).ToList();
                        InventoryFormEdit inventory = new InventoryFormEdit();

                        if (stocks != null)
                        {
                            var status = true;
                            string multimrpId = string.Empty;
                            for (var i = 0; i < item.Count(); i++)
                            {
                                var Stock = stocks.FirstOrDefault(x => x.StockId == item[i].StockId);
                                if (item[i].ManualInventory < 0 && status)
                                {
                                    var ManualInventory = -1 * item[i].ManualInventory;
                                    if (ManualInventory > Stock.CurrentInventory)
                                    {
                                        multimrpId = Stock.ItemMultiMRPId.ToString();
                                        status = false;
                                    }
                                }

                            }
                            if (status)
                            {
                                inventory.WarehouseId = Warehouse_id;
                                inventory.CreatedDate = indianTime;
                                inventory.UpdateDate = indianTime;
                                inventory.CreatedBy = people.PeopleID;
                                inventory.Deleted = false;
                                context.InventoryFormEditDB.Add(inventory);
                                for (var i = 0; i < item.Count(); i++)
                                {
                                    InventoryFormEditDetail inventorydetails = new InventoryFormEditDetail();
                                    inventorydetails.InventoryEditId = inventory.InventoryEditId;
                                    inventorydetails.StockId = item[i].StockId;
                                    inventorydetails.ManualInventory = item[i].ManualInventory;
                                    inventorydetails.ManualReason = item[i].ManualReason;
                                    inventorydetails.CreatedDate = indianTime;
                                    inventorydetails.Deleted = false;
                                    context.InventoryFormEditDetailDB.Add(inventorydetails);
                                }
                                context.Commit();
                                var res = new
                                {
                                    Status = true,
                                    Message = "Send Inventory for Approval Successfully."
                                };
                                return Request.CreateResponse(HttpStatusCode.OK, res);
                            }
                            else
                            {
                                var res = new
                                {
                                    Status = false,
                                    Message = "item multiMRPId :" + multimrpId + " Inventory should not less then current Inventory"
                                };
                                return Request.CreateResponse(HttpStatusCode.OK, res);
                            }
                        }
                        else
                        {
                            var res = new
                            {
                                Status = false,
                                Message = "Error during fatching current stock "
                            };
                            return Request.CreateResponse(HttpStatusCode.OK, res);
                        }
                    }
                    else
                    {
                        var res = new
                        {
                            Status = false,
                            Message = "Warehouse not found "
                        };
                        return Request.CreateResponse(HttpStatusCode.OK, res);
                    }
                }
                catch (Exception ex)
                {
                    logger.Error("Error in Inventory Edit " + ex.Message);
                    logger.Info("End: ");
                    return null;
                }
            }
        }


        [Route("GetWarehousebased")]
        [HttpPost]
        public PaggingData GetWarehousebasedAsync(FilterOrderDataDTO filterOrderData)
        {
            logger.Info("start Inventory Stock: ");
            try
            {
                PaggingData paggingData = new PaggingData();
                List<InventoryEditStockDTO> ass = new List<InventoryEditStockDTO>();
                //dynamic forInventory = null;
                using (AuthContext context = new AuthContext())
                {
                    int skip = (filterOrderData.PageNo - 1) * filterOrderData.ItemPerPage;
                    int take = filterOrderData.ItemPerPage;


                    var identity = User.Identity as ClaimsIdentity;
                    int Warehouse_id = 0, userid = 0; ;

                    if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                        userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

                    if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "WarehouseId"))
                        Warehouse_id = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "WarehouseId").Value);
                    Warehouse_id = filterOrderData.WarehouseId;


                    if (filterOrderData.Start.HasValue && filterOrderData.End.HasValue)
                    {
                        var forInventory = from ie in context.InventoryFormEditDB.Where(X => X.WarehouseId == Warehouse_id && EntityFunctions.TruncateTime(X.CreatedDate) >= EntityFunctions.TruncateTime(filterOrderData.Start) && EntityFunctions.TruncateTime(X.CreatedDate) <= EntityFunctions.TruncateTime(filterOrderData.End))

                                           select new InventoryEditStockDTO
                                           {
                                               InventoryEditId = ie.InventoryEditId,
                                               //ItemName = p.itemname,
                                               //StockId = p.StockId,
                                               //ItemNumber = p.ItemNumber,
                                               //WarehouseName = wh.CityName + "--" + wh.WarehouseName,
                                               CreatedBy = context.Peoples.FirstOrDefault(x => x.PeopleID == ie.CreatedBy).DisplayName,
                                               CreatedDate = ie.CreatedDate,
                                               UpdatedDate = ie.UpdateDate,
                                               IsUpdate = ie.IsUpdated,
                                               IsRejected = ie.IsRejected,
                                               ApprovedBy = context.Peoples.FirstOrDefault(x => x.PeopleID == ie.ApprovedBy).DisplayName,
                                           };
                        var data = forInventory.OrderByDescending(x => x.CreatedDate).Skip(skip).Take(take).ToList();
                        int dataCount = forInventory.Count();
                        if (data != null && data.Any())
                        {
                            paggingData.total_count = dataCount;
                            var orderids = data.Select(x => x.InventoryEditId).ToList();
                        }
                        paggingData.ordermaster = data;
                    }
                    else
                    {
                        var forInventory = from ie in context.InventoryFormEditDB.Where(x => x.WarehouseId == Warehouse_id)

                                           select new InventoryEditStockDTO
                                           {

                                               InventoryEditId = ie.InventoryEditId,

                                               //WarehouseName = wh.CityName + "--" + wh.WarehouseName,
                                               CreatedBy = context.Peoples.FirstOrDefault(x => x.PeopleID == ie.CreatedBy).DisplayName,
                                               CreatedDate = ie.CreatedDate,
                                               UpdatedDate = ie.UpdateDate,
                                               IsUpdate = ie.IsUpdated,
                                               IsRejected = ie.IsRejected,
                                               ApprovedBy = context.Peoples.FirstOrDefault(x => x.PeopleID == ie.ApprovedBy).DisplayName
                                           };
                        var data = forInventory.OrderByDescending(x => x.CreatedDate).Skip(skip).Take(take).ToList();
                        int dataCount = forInventory.Count();
                        if (data != null && data.Any())
                        {
                            paggingData.total_count = dataCount;
                            var orderids = data.Select(x => x.InventoryEditId).ToList();
                        }
                        paggingData.ordermaster = data;
                    }
                    return paggingData;
                }

            }
            catch (Exception ex)
            {
                logger.Error("Error in Inventory Edit stock " + ex.Message);
                return null;
            }
        }

        [Route("GetinventoryDetailList")]
        [HttpGet]
        public async Task<List<InventoryEditStockDTO>> GetinventoryDetailList(int Id)
        {
            logger.Info("start Inventory Stock: ");
            try
            {
                InventoryEditStockDTO inventory = new InventoryEditStockDTO();
                using (AuthContext context = new AuthContext())
                {
                    var forInventory = (from ie in context.InventoryFormEditDetailDB.Where(x => x.InventoryEditId == Id).OrderByDescending(x => x.CreatedDate)
                                        join id in context.InventoryFormEditDB
                                        on ie.InventoryEditId equals id.InventoryEditId
                                        join p in context.DbCurrentStock
                                        on ie.StockId equals p.StockId
                                        select new InventoryEditStockDTO
                                        {
                                            StockId = ie.StockId,
                                            ItemName = p.itemname,
                                            ItemNumber = p.ItemNumber,
                                            TotalAmount = (p.MRP * p.CurrentInventory),
                                            CurrentInventory = p.CurrentInventory,
                                            Price = p.MRP,
                                            ManualInventory = ie.ManualInventory,
                                            InventoryEditId = ie.InventoryEditId,
                                            IsUpdate = id.IsUpdated,
                                            IsRejected = ie.IsRejected,
                                            ManualReason = ie.ManualReason

                                        }).Distinct().ToList();

                    forInventory.ForEach(q => q.TotalPrice = forInventory.Sum(c => c.TotalAmount));
                    List<ItemClassificationDC> ABCitemsList = forInventory.Select(item => new ItemClassificationDC { ItemNumber = item.ItemNumber, WarehouseId = item.WarehouseId }).ToList();

                    var manager = new ItemLedgerManager();
                    var GetItem = await manager.GetItemClassificationsAsync(ABCitemsList);
                    foreach (var item in forInventory)
                    {

                        if (GetItem != null && GetItem.Any())
                        {
                            if (GetItem.Any(x => x.ItemNumber == item.ItemNumber))
                            {
                                item.ABCClassification = GetItem.FirstOrDefault(x => x.ItemNumber == item.ItemNumber).Category;
                            }
                            else { item.ABCClassification = "D"; }
                        }
                        else { item.ABCClassification = "D"; }

                    }

                    return forInventory;
                }
            }
            catch (Exception ex)
            {
                logger.Error("Error in Inventory Edit stock " + ex.Message);
                return null;
            }
        }


        [Route("updateInventoryStatus")]
        [HttpPost]
        public HttpResponseMessage updateInventoryStatus(List<InventoryEditStockDTO> data)
        {
            logger.Info("start Inventory Edit updateInventoryStatus: ");
            var identity = User.Identity as ClaimsIdentity;
            int userid = 0;
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
            var msg = " ";
            if (userid > 0)
            {
                TransactionOptions option = new TransactionOptions();
                option.IsolationLevel = IsolationLevel.RepeatableRead;
                option.Timeout = TimeSpan.FromSeconds(90);
                using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, option))
                {
                    using (AuthContext context = new AuthContext())
                    {
                        var warehouseId = data.FirstOrDefault().WarehouseId;
                        var warehouse = context.Warehouses.FirstOrDefault(x => x.WarehouseId == warehouseId);

                        if (warehouse.IsStopCurrentStockTrans)
                            return Request.CreateResponse(HttpStatusCode.InternalServerError, "Inventory Transactions are currently disabled for this warehouse... Please try after some time");


                        var people = context.Peoples.Where(x => x.PeopleID == userid && x.Deleted == false && x.Active == true).FirstOrDefault();
                        var stockListIds = data.Select(x => x.StockId).ToList();
                        var stockList = context.DbCurrentStock.Where(c => stockListIds.Contains(c.StockId) && c.Deleted == false).ToList();
                        int InventoryEditId = data[0].InventoryEditId;
                        bool Result = false;
                        List<OutDc> Outitems = new List<OutDc>();
                        InventoryFormEdit ass = context.InventoryFormEditDB.Where(x => x.InventoryEditId == InventoryEditId && x.IsUpdated == false && x.Deleted == false).Include(x => x.InventoryFormEditDetails).FirstOrDefault();
                        if (ass != null && people != null)
                        {
                            ass.IsUpdated = true;
                            ass.UpdateDate = indianTime;
                            ass.ApprovedBy = people.PeopleID;
                            foreach (var item in ass.InventoryFormEditDetails)
                            {
                                var edititem = data.Where(x => x.StockId == item.StockId).FirstOrDefault();
                                if (item.IsRejected == false)
                                {
                                    var stock = stockList.Where(c => c.Deleted == false && c.StockId == edititem.StockId).FirstOrDefault();
                                    var IsValidateQty = (stock.CurrentInventory + (edititem.ManualInventory));//If Inventory is Positve 
                                    if (IsValidateQty >= 0)
                                    {
                                        item.CurrentInventory = stock.CurrentInventory;
                                        StockTransactionHelper helper = new StockTransactionHelper();
                                        PhysicalStockUpdateRequestDc StockTransfer = new PhysicalStockUpdateRequestDc();
                                        StockTransfer.ItemMultiMRPId = stock.ItemMultiMRPId;
                                        StockTransfer.WarehouseId = stock.WarehouseId;
                                        StockTransfer.Qty = edititem.ManualInventory;
                                        StockTransfer.Reason = "Stock Update from InventoryEdit form : " + edititem.ManualReason;
                                        StockTransfer.StockTransferType = StockTransferTypeName.ManualInventory;
                                        if (StockTransfer.Qty < 0)
                                        {
                                            StockTransfer.Qty = (-1 * StockTransfer.Qty);
                                            StockTransfer.SourceStockType = StockTypeTableNames.CurrentStocks;// "CurrentStocks";
                                            StockTransfer.DestinationStockType = StockTypeTableNames.VirtualStock;// "CurrentStocks";
                                        }
                                        else
                                        {
                                            StockTransfer.SourceStockType = StockTypeTableNames.VirtualStock;// "CurrentStocks";
                                            StockTransfer.DestinationStockType = StockTypeTableNames.CurrentStocks;// "CurrentStocks";
                                        }
                                        bool isupdated = helper.TransferBetweenVirtualStockAndPhysicalStocks(StockTransfer, userid, context, scope);
                                        if (isupdated)
                                        {
                                            Result = true;
                                            Outitems.Add(new OutDc
                                            {
                                                WarehouseId = StockTransfer.WarehouseId,
                                                ItemMultiMrpId = StockTransfer.ItemMultiMRPId,
                                                Qty = StockTransfer.Qty,
                                                Destination = StockTransfer.SourceStockType == "CurrentStocks" ? "Manual Out" : "Manual In"
                                            });

                                        }
                                        else
                                        {
                                            Result = false;
                                            scope.Dispose();
                                            return Request.CreateResponse(HttpStatusCode.OK, " Some thing went wrong ");
                                        }
                                    }
                                    else
                                    {
                                        Result = false;
                                        scope.Dispose();
                                        return Request.CreateResponse(HttpStatusCode.OK, "Inventory transaction is not validated for item : " + stock.itemname);
                                    }
                                }
                            }
                            context.Entry(ass).State = EntityState.Modified;
                        }
                        if (Result)
                        {
                            context.Commit();
                            msg = "Inventory approved successfully";

                            scope.Complete();

                            #region Insert in FIFO
                            if (ConfigurationManager.AppSettings["LiveFIFO"] == "1")
                            {
                                List<OutDc> items = Outitems.Where(x => x.Qty > 0 && x.Destination == "Manual Out").Select(x => new OutDc
                                {
                                    ItemMultiMrpId = x.ItemMultiMrpId,
                                    WarehouseId = x.WarehouseId,
                                    Destination = "Manual Out",
                                    CreatedDate = indianTime,
                                    ObjectId = 0,
                                    Qty = x.Qty,
                                    SellingPrice = 0,
                                }).ToList();

                                foreach (var it in items)
                                {
                                    RabbitMqHelper rabbitMqHelper = new RabbitMqHelper();
                                    rabbitMqHelper.Publish("ManualOut", it);
                                }

                                //GRDS
                                List<GrDC> itemsin = Outitems.Where(x => x.Qty > 0 && x.Destination == "Manual In").Select(x => new GrDC
                                {
                                    ItemMultiMrpId = x.ItemMultiMrpId,
                                    WarehouseId = x.WarehouseId,
                                    Source = "Manual In",
                                    CreatedDate = indianTime,
                                    Qty = x.Qty,
                                }).ToList();

                                foreach (var itin in itemsin)
                                {
                                    RabbitMqHelper rabbitMqHelper = new RabbitMqHelper();
                                    rabbitMqHelper.Publish("ManualIn", itin);
                                }
                            }

                            #endregion
                        }
                        return Request.CreateResponse(HttpStatusCode.OK, msg);

                    }
                }
                #region old code
                //logger.Info("start Inventory Edit stock: ");
                //using (AuthContext context = new AuthContext())
                //{

                //    try
                //    {
                //        string msg = null;
                //        var identity = User.Identity as ClaimsIdentity;
                //        int userid = 0;

                //        if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                //            userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
                //        var people = context.Peoples.Where(x => x.PeopleID == userid).FirstOrDefault();
                //        int InventoryEditId = data[0].InventoryEditId;

                //       int isApprovedInventory = context.InventoryFormEditDB.Where(x => x.InventoryEditId == InventoryEditId && x.IsUpdated == true && x.Deleted == false).Count();
                //        if (isApprovedInventory > 0)
                //        {
                //            msg = "Inventory is Already Approved,plz Verify.";
                //            return Request.CreateResponse(HttpStatusCode.OK, msg);
                //        }
                //        else
                //        { 
                //            var stockListIds = data.Select(x => x.StockId).ToList();
                //            var stockList = context.DbCurrentStock.Where(c => stockListIds.Contains(c.StockId) && c.Deleted == false).ToList();
                //            InventoryFormEdit ass = context.InventoryFormEditDB.Where(x => x.InventoryEditId == InventoryEditId && x.IsUpdated == false && x.Deleted == false).FirstOrDefault();
                //            if (ass != null)
                //            {
                //                ass.IsUpdated = true;
                //                ass.UpdateDate = indianTime;
                //                ass.ApprovedBy = people.PeopleID;
                //                context.Entry(ass).State = EntityState.Modified;

                //                foreach (var item in data)
                //                {
                //                    if (item.IsRejected == false)
                //                    {
                //                        var stock = stockList.Where(c => c.Deleted == false && c.StockId == item.StockId).FirstOrDefault();
                //                        var IsValidateQty = (stock.CurrentInventory + (item.ManualInventory));//If Inventory is Positve 
                //                        if (IsValidateQty >= 0)
                //                        {
                //                            CurrentStockHistory CurrentStockHistory = new CurrentStockHistory();
                //                            CurrentStockHistory.updationDate = indianTime;
                //                            CurrentStockHistory.StockId = stock.StockId;
                //                            CurrentStockHistory.ItemMultiMRPId = stock.ItemMultiMRPId;
                //                            CurrentStockHistory.itemname = stock.itemname;
                //                            CurrentStockHistory.ItemNumber = stock.ItemNumber;
                //                            CurrentStockHistory.CurrentInventory = stock.CurrentInventory + (item.ManualInventory);
                //                            CurrentStockHistory.TotalInventory = stock.CurrentInventory + (item.ManualInventory);
                //                            CurrentStockHistory.ManualInventoryIn = item.ManualInventory;
                //                            CurrentStockHistory.OdOrPoId = ass.InventoryEditId;
                //                            CurrentStockHistory.ManualReason = "Stock Update from InventoryEdit form : " + item.ManualReason;
                //                            CurrentStockHistory.UserName = people.DisplayName;
                //                            CurrentStockHistory.userid = people.PeopleID;
                //                            CurrentStockHistory.CreationDate = indianTime;
                //                            CurrentStockHistory.Warehouseid = stock.WarehouseId;
                //                            CurrentStockHistory.WarehouseName = stock.WarehouseName;
                //                            context.CurrentStockHistoryDb.Add(CurrentStockHistory);

                //                            stock.CurrentInventory += (item.ManualInventory);
                //                            stock.UpdatedDate = indianTime;
                //                            context.Entry(stock).State = EntityState.Modified;
                //                            context.Commit();
                //                        }
                //                    }
                //                }
                //            }
                //            msg = "Inventory updated.";
                //            return Request.CreateResponse(HttpStatusCode.OK, msg);
                //        }
                //    }
                //    catch (Exception ex)
                //    {
                //        logger.Error("Error in Inventory stock " + ex.Message);
                //        return Request.CreateResponse(HttpStatusCode.NotFound, false);
                //    }
                //}
                #endregion
            }
            else
            {
                return Request.CreateResponse(HttpStatusCode.OK, " Some thing went wrong ");
            }
        }


        [Route("RejectInventory")]
        [AcceptVerbs("Delete")]
        [HttpPost]
        public HttpResponseMessage Remove(int id, int InventoryEditId)
        {
            logger.Info("start delete Inventory: ");
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

                    logger.Info("User ID : {0} , Company Id : {1}", compid, userid);

                    var inventory = context.InventoryFormEditDB.Where(x => x.InventoryEditId == InventoryEditId && x.IsRejected == false && x.IsUpdated == false).Include(x => x.InventoryFormEditDetails).FirstOrDefault();
                    var data = context.InventoryFormEditDetailDB.Where(x => x.StockId == id && x.InventoryEditId == InventoryEditId && x.IsRejected == false && x.Deleted == false).FirstOrDefault();
                    if (inventory != null)
                    {
                        if (inventory.InventoryFormEditDetails.Count == 1)
                        {
                            inventory.IsRejected = true;
                            inventory.UpdateDate = indianTime;
                            context.Entry(inventory).State = EntityState.Modified;
                        }
                        data.IsRejected = true;
                        context.Entry(data).State = EntityState.Modified;
                        context.Commit();
                    }
                    return Request.CreateResponse(HttpStatusCode.OK, data);
                }
                catch (Exception ex)
                {
                    logger.Error("Error in delete Inventory" + ex.Message);
                    return null;
                }
            }
        }


        [Route("RejectAllInventory")]
        [AcceptVerbs("POST")]
        [HttpPost]
        public HttpResponseMessage RejectAllInventory(List<InventoryEditStockDTO> data)
        {

            logger.Info("start add Inventory: ");
            using (AuthContext context = new AuthContext())
            {
                try
                {
                    List<InventoryFormEditDetail> inventorydetails = new List<InventoryFormEditDetail>();
                    var identity = User.Identity as ClaimsIdentity;
                    int result = 0;
                    int userid = 0;

                    if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                        userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
                    foreach (var item in data)
                    {
                        if (item.InventoryEditId > 0)
                        {
                            var inventory = context.InventoryFormEditDB.Where(x => x.InventoryEditId == item.InventoryEditId && x.IsRejected == false && x.IsUpdated == false).FirstOrDefault();
                            if (inventory != null)
                            {
                                inventory.IsRejected = true;
                                inventory.UpdateDate = indianTime;
                                context.Entry(inventory).State = EntityState.Modified;
                                inventorydetails = context.InventoryFormEditDetailDB.Where(x => x.InventoryEditId == inventory.InventoryEditId && x.Deleted == false).ToList();
                                foreach (var items in inventorydetails)
                                {
                                    items.IsRejected = true;
                                    context.Entry(items).State = EntityState.Modified;
                                }
                            }

                        }
                        result++;
                    }
                    if (result == data.Count())
                    {
                        context.Commit();
                    }
                    logger.Info("All Inventory Rejected: ");
                    return Request.CreateResponse(HttpStatusCode.OK, inventorydetails);
                }
                catch (Exception ex)
                {
                    logger.Error("Error in Inventory Edit " + ex.Message);
                    logger.Info("End: ");
                    return Request.CreateResponse(HttpStatusCode.NotFound, false);
                }
            }
        }

    }

    #region DTO

    public class InventoryEditStockDTO
    {
        public string ItemName { get; set; }
        public string ItemNumber { get; set; }
        public int WarehouseId { get; set; }
        public string WarehouseName { get; set; }
        public int InventoryEditId { get; set; }
        public int StockId { get; set; }
        public int ManualInventory { get; set; }
        public int CurrentInventory { get; set; }
        public double Price { get; set; }
        public double TotalPrice { get; set; }
        public double TotalAmount { get; set; }
        public string ManualReason { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public string ApprovedBy { get; set; }
        public string CreatedBy { get; set; }
        public bool IsUpdate { get; set; }
        public bool IsRejected { get; set; }
        public bool Deleted { get; set; }
        public string ABCClassification { get; set; }
    }

    public class CurrentStockitemDTO
    {
        public int ItemMultiMRPId { get; set; }
        public string itemname { get; set; }
        public int StockId { get; set; }
        public double MRP { get; set; }
        public int WarehouseId { get; set; }
        public double PurchasePrice { get; set; }
        public string ItemNumber { get; set; }
        public double TotalAmount { get; set; }
        public int CurrentInventory { get; set; }
        public double TotalPrice { get; set; }
        public string ABC_Classification { get; set; }
    }

    public class FilterOrderDataDTO
    {
        public int ItemPerPage { get; set; }
        public int PageNo { get; set; }
        public int WarehouseId { get; set; }
        public int Cityid { get; set; }
        public DateTime? Start { get; set; }
        public DateTime? End { get; set; }


    }

    public class InventoryStockHistorydata
    {
        public int total_count { get; set; }
        public dynamic inventoryEditstock { get; set; }

    }
    #endregion
}
