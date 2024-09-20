using AngularJSAuthentication.API.Controllers;
using AngularJSAuthentication.BatchManager.Helpers;
using AngularJSAuthentication.Common.Constants;
using AngularJSAuthentication.Common.Helpers;
using AngularJSAuthentication.DataContracts.BatchCode;
using AngularJSAuthentication.DataContracts.Transaction.Stocks;
using AngularJSAuthentication.Model.NonRevenueOrders;
using AngularJSAuthentication.Model.Stocks;
using AngularJSAuthentication.Model.Stocks.Batch;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace AngularJSAuthentication.API.Managers
{
    public class BatchMasterManager
    {
        public bool MoveDirectBatchItemInSameBatch(string fromStockType, string toStockType, int qty, long fromStockBatchMasterId, int itemMultiMRPId, int warehouseId, AuthContext context, int userId)
        {
            ElasticBatchHelper elasticBatchHelper = new ElasticBatchHelper();
            bool isAnyTransactionRunning = elasticBatchHelper.IsAnyPendingDocExists(itemMultiMRPId, warehouseId);
            if (isAnyTransactionRunning)
            {
                return false;
            }

            StockBatchMaster fromBatch = context.StockBatchMasters.Where(x => x.Id == fromStockBatchMasterId).FirstOrDefault();
            var toStockBatchMasterId = GetOrCreate(itemMultiMRPId, warehouseId, toStockType, fromBatch.BatchMasterId, context, userId);
            StockBatchMaster toBatch = context.StockBatchMasters.Where(x => x.Id == toStockBatchMasterId).FirstOrDefault();

            fromBatch.Qty = fromBatch.Qty - qty;
            toBatch.Qty = toBatch.Qty + qty;

            context.Entry(fromBatch).State = EntityState.Modified;
            context.Entry(toBatch).State = EntityState.Modified;

            CreateStockBatchTransactionEntry(context, fromBatch.Id, StocTxnType.ManualOut, qty, userId, fromBatch.StockId);
            CreateStockBatchTransactionEntry(context, toBatch.Id, StocTxnType.ManualIn, qty, userId, toBatch.StockId);

            return true;
        }


        public long GetOrCreate(int itemMultiMRPId, int warehouseId, string stockType, long batchMasterId, AuthContext authContext, int userId)
        {
            var itemMultiMrpIdParam = new SqlParameter("@ItemMultiMRPId", itemMultiMRPId);
            var warehouseIdParam = new SqlParameter("@WarehouseId", warehouseId);
            var stockTypeParam = new SqlParameter("@StockType", stockType);
            long? stockId = authContext.Database.SqlQuery<long?>("exec [BatchCode].[GetStockId] @ItemMultiMRPId, @WarehouseId, @StockType", itemMultiMrpIdParam, warehouseIdParam, stockTypeParam).FirstOrDefault();

            if (stockId.HasValue && stockId.Value > 0)
            {
                stockId = stockId.Value;
                long id = GetOrCreateStockMaster(itemMultiMRPId, warehouseId, stockType, batchMasterId, stockId.Value, authContext, userId);
                return id;
            }
            else
            {
                var itemMultiMrpIdParamData = new SqlParameter("@ItemMultiMRPId", itemMultiMRPId);
                var warehouseIdParamData = new SqlParameter("@WarehouseId", warehouseId);
                var itemData = authContext.Database.SqlQuery<MultiMRPDetailsDC>("exec [GetMultiMRPIdDetail] @ItemMultiMRPId, @WarehouseId", itemMultiMrpIdParamData, warehouseIdParamData).FirstOrDefault();
                switch (stockType)
                {
                    case "C":
                        break;
                    case "D":
                        //var itemData = authContext.itemMasters.Where(x => x.ItemMultiMRPId == itemMultiMRPId && x.WarehouseId == warehouseId && x.active == true && x.Deleted == false).FirstOrDefault();
                        var damageEntity = new Model.DamageStock
                        {
                            CompanyId = 1,
                            CreatedDate = DateTime.Now,
                            DamageInventory = 0,
                            //DamageStockId=0,
                            Deleted = false,
                            ItemId = 0,
                            ItemMultiMRPId = itemMultiMRPId,
                            itemBaseName = itemData.ItemBaseName,
                            ItemName = itemData.ItemName,
                            ItemNumber = itemData.ItemNumber,
                            WarehouseId = warehouseId,
                            WarehouseName = itemData.WarehouseName,
                            MRP = itemData.MRP,
                            PurchasePrice = 0,
                            UnitPrice = 0,
                            ReasonToTransfer = "Transfer to Damage Stock"
                        };
                        authContext.DamageStockDB.Add(damageEntity);
                        authContext.Commit();
                        stockId = damageEntity.DamageStockId;
                        break;
                    case "F":
                        //var freeitemData = authContext.itemMasters.Where(x => x.ItemMultiMRPId == itemMultiMRPId && x.WarehouseId == warehouseId && x.active == true && x.Deleted == false).FirstOrDefault();
                        var freeEntity = new Model.FreeStock
                        {
                            ItemMultiMRPId = itemMultiMRPId,
                            Deleted = false,
                            MRP = itemData.MRP,
                            CurrentInventory = 0,
                            CreatedBy = "",
                            CreationDate = DateTime.Now,
                            UpdatedDate = DateTime.Now,
                            itemname = itemData.ItemName,
                            ItemNumber = itemData.ItemNumber,
                            WarehouseId = warehouseId,
                        };
                        authContext.FreeStockDB.Add(freeEntity);
                        authContext.Commit();
                        stockId = freeEntity.FreeStockId;
                        break;
                    case "N":
                        var nonSellableEntity = new Model.NonSellableStock
                        {
                            Inventory = 0,
                            CreatedBy = 0,
                            CreatedDate = DateTime.Now,
                            IsActive = true,
                            IsDeleted = false,
                            ItemMultiMRPId = itemMultiMRPId,
                            ItemName = itemData.ItemName,
                            WarehouseId = warehouseId,

                        };
                        authContext.NonSellableStockDB.Add(nonSellableEntity);
                        authContext.Commit();
                        stockId = nonSellableEntity.NonSellableStockId;

                        break;
                    case "CL":
                        var clearanceStockEntity = new ClearanceStockNew
                        {
                            CreatedBy = 0,
                            CreatedDate = DateTime.Now,
                            Inventory = 0,
                            IsActive = true,
                            IsDeleted = false,
                            ItemMultiMRPId = itemMultiMRPId,
                            WarehouseId = warehouseId,
                            ItemName = itemData.ItemName
                        };
                        authContext.ClearanceStockNewDB.Add(clearanceStockEntity);
                        authContext.Commit();
                        stockId = clearanceStockEntity.ClearanceStockId;


                        break;
                    case "IR":
                        var InventoryReserveStockEntity = new InventoryReserveStock
                        {
                            CreatedBy = 0,
                            CreatedDate = DateTime.Now,
                            Inventory = 0,
                            IsActive = true,
                            IsDeleted = false,
                            ItemMultiMRPId = itemMultiMRPId,
                            WarehouseId = warehouseId,
                            ItemName = itemData.ItemName
                        };
                        authContext.InventoryReserveStocks.Add(InventoryReserveStockEntity);
                        authContext.Commit();
                        stockId = InventoryReserveStockEntity.Id;

                        break;
                    case "NR":
                        //var itemData = authContext.itemMasters.Where(x => x.ItemMultiMRPId == itemMultiMRPId && x.WarehouseId == warehouseId && x.active == true && x.Deleted == false).FirstOrDefault();
                        var NonRevenueStockEntity = new NonRevenueOrderStock
                        {
                            CreatedBy = 0,
                            CreatedDate = DateTime.Now,
                            NonRevenueInventory = 0,
                            IsActive = true,
                            IsDeleted = false,
                            //ItemId = 0,
                            ItemMultiMRPId = itemMultiMRPId,
                           // itemBaseName = itemData.ItemBaseName,
                            ItemName = itemData.ItemName,
                            ItemNumber = itemData.ItemNumber,
                            WarehouseId = warehouseId,
                            MRP = itemData.MRP,
                          //  PurchasePrice = 0,
                            UnitPrice = 0,
                           // ReasonToTransfer = "Transfer to NonRevenue Stock"
                        };
                        authContext.NonRevenueOrderStocks.Add(NonRevenueStockEntity);
                        authContext.Commit();
                        stockId = NonRevenueStockEntity.Id;

                        break;
                }
                authContext.Commit();
                long id = GetOrCreateStockMaster(itemMultiMRPId, warehouseId, stockType, batchMasterId, stockId.Value, authContext, userId);
                return id;
            }
        }

        public long GetOrCreateStockMaster(int temMultiMRPId, int warehouseId, string stockType, long batchMasterId, long stockId, AuthContext authContext, int userId)
        {
            var query = "BatchCode.GetOrCreateStockMaster @ItemMultiMRPId, @WarehouseId, @StockType, @BatchMasterId,  @stockId";
            var itemMultiMRPIdParam = new SqlParameter
            {
                ParameterName = "ItemMultiMRPId",
                Value = temMultiMRPId

            };
            var warehouseIdParams = new SqlParameter
            {
                ParameterName = "WarehouseId",
                Value = warehouseId

            };
            var stockTypeParams = new SqlParameter
            {
                ParameterName = "StockType",
                Value = stockType

            };
            var batchMasterIdIdParams = new SqlParameter
            {
                ParameterName = "BatchMasterId",
                Value = batchMasterId

            };
            var stockIdParams = new SqlParameter
            {
                ParameterName = "StockId",
                Value = stockId

            };
            var userIdParams = new SqlParameter
            {
                ParameterName = "UserId",
                Value = userId

            };

            long id = authContext.Database.SqlQuery<long>(query, itemMultiMRPIdParam, warehouseIdParams, stockTypeParams, batchMasterIdIdParams, stockIdParams, userIdParams).FirstOrDefault();
            authContext.Commit();
            return id;
        }
        //public long GetOrCreate(int itemMultiMRPId, int warehouseId, string stockType, long batchMasterId, AuthContext authContext)
        //{

        //    var query = "BatchCode.GetOrCreateStockBatchMaster @ItemMultiMRPId, @WarehouseId, @StockType, @BatchMasterId";
        //    var itemMultiMRPIdParam = new SqlParameter
        //    {
        //        ParameterName = "ItemMultiMRPId",
        //        Value = itemMultiMRPId

        //    };
        //    var warehouseIdParam = new SqlParameter
        //    {
        //        ParameterName = "WarehouseId",
        //        Value = warehouseId

        //    };
        //    var stockTypeParam = new SqlParameter
        //    {
        //        ParameterName = "StockType",
        //        Value = stockType

        //    };
        //    var batchMasterIdParam = new SqlParameter
        //    {
        //        ParameterName = "BatchMasterId",
        //        Value = batchMasterId

        //    };

        //    long id = authContext.Database.SqlQuery<long>(query, itemMultiMRPIdParam, warehouseIdParam, stockTypeParam, batchMasterIdParam).FirstOrDefault();
        //    return id;

        //}

        public bool AddQty(List<TransferOrderItemBatchMasterDc> transferOrderItemBatchMasterDcs, AuthContext context, int userid, long StockTxnTypeId)
        {
            ElasticBatchHelper elasticBatchHelper = new ElasticBatchHelper();
            bool isAnyTransactionRunning = false;
            bool status = false;
            foreach (var item in transferOrderItemBatchMasterDcs)
            {
                isAnyTransactionRunning = elasticBatchHelper.IsAnyPendingDocExists(item.ItemMultiMRPId, item.WarehouseId);
                if (isAnyTransactionRunning)
                {
                    break;
                }
            }
            if (!isAnyTransactionRunning)
            {
                var batchstockMasterIds = transferOrderItemBatchMasterDcs.Select(x => x.StockBatchMasterId).Distinct().ToList();

                var stockBatchMasters = context.StockBatchMasters.Where(x => batchstockMasterIds.Contains(x.Id)).ToList();

                if (stockBatchMasters != null && stockBatchMasters.Any())
                {
                    List<StockBatchTransaction> AddStockBatchTransactionList = new List<StockBatchTransaction>();

                    transferOrderItemBatchMasterDcs.ForEach(x =>
                    {
                        
                        var findData = stockBatchMasters.FirstOrDefault(z => z.Id == x.StockBatchMasterId);
                        if (findData != null)
                        {
                            findData.Qty += x.Qty;
                            findData.ModifiedBy = userid;
                            findData.ModifiedDate = DateTime.Now;
                            context.Entry(findData).State = EntityState.Modified;

                            AddStockBatchTransactionList.Add(new StockBatchTransaction
                            {
                                StockBatchMasterId = x.StockBatchMasterId,
                                Qty = (x.Qty < 0) ? ((-1) * x.Qty) : x.Qty,
                                CreatedDate = DateTime.Now,
                                CreatedBy = userid,
                                TransactionDate = DateTime.Now,
                                ObjectId = x.ObjectId,
                                ObjectDetailId = x.ObjectIdDetailId,
                                StockTxnTypeId = StockTxnTypeId,
                                IsActive = true,
                                IsDeleted = false
                            });

                        }
                    });
                    if (AddStockBatchTransactionList != null && AddStockBatchTransactionList.Any())
                    {
                        context.StockBatchTransactions.AddRange(AddStockBatchTransactionList);
                    }
                    status = true;
                }
            }
            return status;
        }

        public bool MoveStock(List<ManualStockUpdateRequestDc> stockList, AuthContext context, int peopleId)
        {
            bool result = true;
            if (stockList != null && stockList.Any())
            {
                StockHelper stockHelper = new StockHelper();
                foreach (var item in stockList)
                {
                    if (stockHelper.IsPhyshicalStock(item.SourceStockType))
                    {
                        if (!item.sStockBatchMasterID.HasValue)
                        {
                            result = false;
                            return result;
                        }
                        else
                        {
                            var fromStock = context.StockBatchMasters.First(x => x.Id == item.sStockBatchMasterID.Value);
                            fromStock.Qty -= Math.Abs(item.Qty);
                            fromStock.ModifiedBy = peopleId;
                            fromStock.ModifiedDate = DateTime.Now;

                            context.Entry(fromStock).State = EntityState.Modified;

                            CreateStockBatchTransactionEntry(context, item.sStockBatchMasterID.Value, StocTxnType.ManualOut, Math.Abs(item.Qty), peopleId, 0);
                        }
                    }
                    else if (stockHelper.IsPhyshicalStock(item.DestinationStockType))
                    {
                        if (!item.dBatchMasterID.HasValue)
                        {
                            result = false;
                            return result;
                        }
                        else
                        {
                            string stockType = stockHelper.GetStockShortName(item.DestinationStockType);
                            //var stockBatchMasterId = GetOrCreate(item.ItemMultiMRPId, item.WarehouseId, stockType, item.dBatchMasterID.Value, context);
                            var stockBatchMasterId = GetOrCreate(item.ItemMultiMRPId, item.WarehouseId, stockType, item.dBatchMasterID.Value, context, peopleId);
                            var toStock = context.StockBatchMasters.First(x => x.Id == stockBatchMasterId);
                            toStock.Qty += Math.Abs(item.Qty);
                            toStock.ModifiedBy = peopleId;
                            toStock.ModifiedDate = DateTime.Now;

                            context.Entry(toStock).State = EntityState.Modified;

                            CreateStockBatchTransactionEntry(context, stockBatchMasterId, StocTxnType.ManualIn, Math.Abs(item.Qty), peopleId, 0);
                        }
                    }
                    //else
                    //{
                    //    return false;
                    //}
                }
            }
            return result;
        }

        public bool MoveStock(List<TransferStockDTONew> transferStockList, AuthContext context, int peopleId, string toBatchStockType = "C")
        {
            bool result = true;
            if (transferStockList != null && transferStockList.Any())
            {
                foreach (var item in transferStockList)
                {
                    var fromBatch = context.StockBatchMasters.First(x => x.Id == item.StockBatchMasterId);
                    fromBatch.Qty -= item.Qty;
                    fromBatch.ModifiedBy = peopleId;
                    fromBatch.ModifiedDate = DateTime.Now;

                    context.Entry(fromBatch).State = EntityState.Modified;
                    CreateStockBatchTransactionEntry(context, fromBatch.Id, StocTxnType.ManualOut, item.Qty, peopleId, fromBatch.StockId);

                    var toBatchId = GetOrCreate(item.ItemMultiMRPIdTrans, item.WarehouseId, toBatchStockType, fromBatch.BatchMasterId, context, peopleId);
                    var toBatch = context.StockBatchMasters.First(x => x.Id == toBatchId);
                    toBatch.Qty += item.Qty;
                    toBatch.ModifiedBy = peopleId;
                    toBatch.ModifiedDate = DateTime.Now;

                    context.Entry(toBatch).State = EntityState.Modified;
                    CreateStockBatchTransactionEntry(context, toBatch.Id, StocTxnType.ManualIn, item.Qty, peopleId, toBatch.StockId);
                }
            }
            return result;
        }


        public long GetStockTxnTypeId(StocTxnType stocTxnType, AuthContext context)
        {
            string txnType = stocTxnType.Value;
            long? id = context.StockTxnTypeMasters.FirstOrDefault(x => x.StockTxnType == txnType)?.Id;
            if (id.HasValue)
            {
                return id.Value;
            }
            else
            {
                return 0;
            }
        }

        public void CreateStockBatchTransactionEntry(AuthContext context, long stockBatchMasterId, StocTxnType stocTxnType, int qty, int userId, long stockId)
        {
            long stockTxnTypeId = GetStockTxnTypeId(stocTxnType, context);

            var transaction = new StockBatchTransaction
            {
                CreatedBy = userId,
                CreatedDate = DateTime.Now,
                IsActive = true,
                IsDeleted = false,
                ModifiedBy = null,
                ModifiedDate = null,
                ObjectId = stockId,
                ObjectDetailId = 0,
                Qty = qty,
                StockBatchMasterId = stockBatchMasterId,
                StockTxnTypeId = stockTxnTypeId,
                TransactionDate = DateTime.Now
            };
            context.StockBatchTransactions.Add(transaction);

        }


        public bool UpdateStockInSameBatch(long StockBatchMasterId, AuthContext context, int peopleId, int qty)
        {
            bool result = true;
            var stockBatch = context.StockBatchMasters.First(x => x.Id == StockBatchMasterId);

            if (StockBatchMasterId > 0 && peopleId > 0)
            {

                stockBatch.Qty -= qty;
                stockBatch.ModifiedBy = peopleId;
                stockBatch.ModifiedDate = DateTime.Now;
                context.Entry(stockBatch).State = EntityState.Modified;

                if (qty > 0)
                {
                    CreateStockBatchTransactionEntry(context, StockBatchMasterId, StocTxnType.ManualOut, qty, peopleId, stockBatch.StockId);

                }
                else
                {
                    CreateStockBatchTransactionEntry(context, StockBatchMasterId, StocTxnType.ManualIn, qty*(-1), peopleId, stockBatch.StockId);
                }
            }
            return result;
        }



    }
}