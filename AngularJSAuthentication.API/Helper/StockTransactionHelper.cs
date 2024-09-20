using AngularJSAuthentication.API.Helpers;
using AngularJSAuthentication.API.Managers.Stocks;
using AngularJSAuthentication.Caching;
using AngularJSAuthentication.Common.Helpers;
using AngularJSAuthentication.DataContracts.constants;
using AngularJSAuthentication.DataContracts.Shared;
using AngularJSAuthentication.DataContracts.Transaction;
using AngularJSAuthentication.DataContracts.Transaction.Stocks;
using AngularJSAuthentication.Model.Base.Audit;
using AngularJSAuthentication.Model.Stocks;
using AngularJSAuthentication.Model.Stocks.Configuration;
using Newtonsoft.Json;
using Nito.AspNetBackgroundTasks;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Transactions;
using System.Web;

namespace AngularJSAuthentication.API.Helper
{
    public class StockTransactionHelper
    {
        public bool ManualStockUpdate(List<ManualStockUpdateRequestDc> list, int userId, AuthContext context, TransactionScope scope, bool IsSettleAlso=false)
        {
            string msg = "";
            DateTime currentDate = DateTime.Now;
            if (list != null && list.Any())
            {
                List<ManualStockUpdateRequest> requestList = new List<ManualStockUpdateRequest>();
                foreach (ManualStockUpdateRequestDc request in list)
                {
                    requestList.Add(new ManualStockUpdateRequest
                    {
                        CreatedBy = userId,
                        CreatedDate = currentDate,
                        DestinationStockType = request.DestinationStockType,
                        IsActive = true,
                        IsDeleted = false,
                        Qty = request.Qty,
                        SourceStockType = request.SourceStockType,
                        Status = "Pending",
                        ItemMultiMRPId = request.ItemMultiMRPId,
                        WarehouseId = request.WarehouseId,
                        Reason = request.Reason,
                        StockTransferType = string.IsNullOrEmpty(request.StockTransferType) ? StockTransferTypeName.CurrentStocks : request.StockTransferType
                    });
                }

                string transactionID = Guid.NewGuid().ToString();
                foreach (ManualStockUpdateRequestDc request in list)
                {
                    string destinationStockChildTableName = StockTypeTableNames.GetStockTypeChildTableNames(request.DestinationStockType);
                    string sourceStockChildTableName = StockTypeTableNames.GetStockTypeChildTableNames(request.SourceStockType);
                    ManualStockUpdateRequest requestEntity = new ManualStockUpdateRequest
                    {
                        CreatedBy = userId,
                        CreatedDate = currentDate,
                        DestinationStockType = destinationStockChildTableName,
                        IsActive = true,
                        IsDeleted = false,
                        Qty = request.Qty,
                        SourceStockType = sourceStockChildTableName,
                        Status = "Pending",
                        ItemMultiMRPId = request.ItemMultiMRPId,
                        WarehouseId = request.WarehouseId,
                        Reason = request.Reason,
                        StockTransferType = string.IsNullOrEmpty(request.StockTransferType) ? StockTransferTypeName.CurrentStocks : request.StockTransferType
                    };
                    context.ManualStockUpdateRequestDB.Add(requestEntity);
                    context.Commit();


                    string tableName = "";
                    if (request.SourceStockType == StockTypeTableNames.VirtualStock)
                    {
                        tableName = request.DestinationStockType;
                        request.Qty = -request.Qty;
                    }
                    else
                    {
                        tableName = request.SourceStockType;
                    }

                    try
                    {
                        MultiStockHelper<ManualStockUpdateDc> FreeMultiStockHelpers = new MultiStockHelper<ManualStockUpdateDc>();

                        ManualStockUpdateDc manualStockUpdateDc = new ManualStockUpdateDc
                        {
                            ItemMultiMRPId  = request.ItemMultiMRPId,
                            ManualReason = requestEntity.Reason,
                            ManualStockUpdateRequestId = requestEntity.Id,
                            Qty = request.Qty,
                            StockTransferType = requestEntity.StockTransferType,
                            TableName = tableName,
                            UserId = userId,
                            WarehouseId = request.WarehouseId,
                            TransactionId = transactionID,
                            IsSettleAlso = IsSettleAlso
                        };

                        List<ManualStockUpdateDc> manualStockUpdateDcList = new List<ManualStockUpdateDc>();
                        manualStockUpdateDcList.Add(manualStockUpdateDc);

                        bool isSuccess = FreeMultiStockHelpers.MakeEntry(manualStockUpdateDcList, "Stock_UpdateSecondary", context, scope);

                        if (!isSuccess)
                        {
                            return false;
                        }
                        else
                        {
                            msg = "Transaction Saved Successfully";
                        }
                    }
                    catch (Exception e)
                    {
                        throw e;
                    }
                }


            }
            return true;
        }


        public bool TransferBetweenVirtualStockAndPhysicalStocks(PhysicalStockUpdateRequestDc request, int userId, AuthContext context, TransactionScope scope, bool IsSettleAlso=false)
        {
            bool result = false;
            if (request != null && request.Qty > 0)
            {
                DateTime currentDate = DateTime.Now;
                List<ManualStockUpdateRequestDc> requestList = new List<ManualStockUpdateRequestDc>();
                requestList.Add(new ManualStockUpdateRequestDc
                {
                    DestinationStockType = request.DestinationStockType,
                    Qty = request.Qty,
                    SourceStockType = request.SourceStockType,
                    Status = "Pending",
                    ItemMultiMRPId = request.ItemMultiMRPId,
                    WarehouseId = request.WarehouseId,
                    Reason = request.Reason,
                    StockTransferType = request.StockTransferType,
                    
                });


                result = ManualStockUpdate(requestList, userId, context, scope, IsSettleAlso);
            }
            return result;
        }



        public bool TransferBetweenPhysicalStocks(PhysicalStockUpdateRequestDc request, int userId, AuthContext context, TransactionScope scope)
        {
            bool result = false;
            if (request != null && request.Qty > 0)
            {
                DateTime currentDate = DateTime.Now;
                List<ManualStockUpdateRequestDc> requestList = new List<ManualStockUpdateRequestDc>();
                requestList.Add(new ManualStockUpdateRequestDc
                {
                    DestinationStockType = StockTypeTableNames.VirtualStock,
                    Qty = request.Qty,
                    SourceStockType = request.SourceStockType,
                    Status = "Pending",
                    ItemMultiMRPId = request.ItemMultiMRPId,
                    WarehouseId = request.WarehouseId,
                    Reason = request.Reason,
                    StockTransferType = request.StockTransferType
                });

                requestList.Add(new ManualStockUpdateRequestDc
                {
                    DestinationStockType = request.DestinationStockType,
                    Qty = request.Qty,
                    SourceStockType = StockTypeTableNames.VirtualStock,
                    Status = "Pending",
                    ItemMultiMRPId = request.ItemMultiMRPId,
                    WarehouseId = request.WarehouseId,
                    Reason = request.Reason,
                    StockTransferType = request.StockTransferType
                });

                result = ManualStockUpdate(requestList, userId, context, scope);
            }
            return result;
        }



        #region not using metods
        public void UpdateStockList(List<Audit> entityList, int userid)
        {
            if (entityList != null && entityList.Count > 0)
            {
                foreach (var item in entityList)
                {
                    UpdateStock(item, userid);
                }
            }
        }

        private List<StockTransactionMaster> UpdateStock(Audit entity, int userid)
        {
            List<StockTransactionMaster> returnMasterList = new List<StockTransactionMaster>();
            StockTransactionManager stockTransactionManager = new StockTransactionManager();
            List<StockTransactionMaster> stockTransactionMasterList = new List<StockTransactionMaster>();
            try
            {
                EntityReflactionHelper entityReflactionHelper = new EntityReflactionHelper();
                string entityName = entityReflactionHelper.GetClassType(entity);
                stockTransactionMasterList = stockTransactionManager.GetMasterByEntityName(entityName);

                if (stockTransactionMasterList != null && stockTransactionMasterList.Any())
                {
                    foreach (var master in stockTransactionMasterList)
                    {
                        bool isValid = IsValidConditionList(master.Id, entity);
                        if (isValid)
                        {
                            returnMasterList.Add(master);
                        }
                    }

                }

                if (returnMasterList != null && returnMasterList.Count > 0)
                {
                    RedisCacheProvider redisCacheProvider = new RedisCacheProvider();

                    var values = new NameValueEntry[]
                   {
                        new NameValueEntry("sensor_id", JsonConvert.SerializeObject(new StockTransactionMQ { Entity = entity, StockTransactionMasterIdList = returnMasterList.Select(x => x.Id).ToList() }))
                   };

                    var db = redisCacheProvider.RedisConnection.GetDatabase();
                    var messageId = db.StreamAdd("event_stream", values);

                    //ISubscriber sub = redisCacheProvider.RedisConnection.GetSubscriber();
                    //await sub.PublishAsync("testq", "Done", CommandFlags.HighPriority);


                    //RedisCacheProvider redisCacheProvider = new RedisCacheProvider();

                    //ISubscriber sub = redisCacheProvider.RedisConnection.GetSubscriber();
                    //sub.Publish("Stock.TestMQ", JsonConvert.SerializeObject(new StockTransactionMQ { Entity = entity, StockTransactionMasterIdList = returnMasterList.Select(x => x.Id).ToList() }));
                }
            }
            catch (Exception ex)
            {
                BackgroundTaskManager.Run(async () =>
                {
                    var result = await MongoLogger.ErrorLog(new AccountErrorLog
                    {
                        CoRelationId = "",
                        IP = "",
                        ForwardedIps = "",
                        Message = ex.Message,
                        InnerException = ex.InnerException.Message,
                        CreatedDate = DateTime.Now,
                        xForwardedHttpHeader = null
                    });
                });
            }
            return returnMasterList;
        }
        public bool MakeRTDEntry(StockTransactionMQ queue)
        {
            Audit entity = queue.Entity;
            int quantity = int.Parse(entity.AuditFields.FirstOrDefault(x => x.FieldName == "qty").NewValue);
            int itemMultiMRPId = int.Parse(entity.AuditFields.FirstOrDefault(x => x.FieldName == "ItemMultiMRPId").NewValue);
            int warehouseId = int.Parse(entity.AuditFields.FirstOrDefault(x => x.FieldName == "WarehouseId").NewValue);
            int orderId = int.Parse(entity.AuditFields.FirstOrDefault(x => x.FieldName == "OrderId").NewValue);
            int orderDispatchDetailId = int.Parse(entity.AuditFields.FirstOrDefault(x => x.FieldName == "OrderDispatchedDetailsId").NewValue);

            var parameters = new[] {
                new SqlParameter("@Qty", quantity),
                new SqlParameter("@ItemMultiMRPId", itemMultiMRPId),
                new SqlParameter("@WarehouseId", warehouseId),
                new SqlParameter("@OrderId", orderId),
                new SqlParameter("@OrderDispatchedDetailsId", orderDispatchDetailId)
            };

            using (var context = new AuthContext())
            {
                // CurrentSkockHistory, RTDStock, VirtualStock, CurrentStock, StockDetail

                //create a new column in CurrentSkockHistory are SettleQty, TransactionNo

                int result = context.Database.ExecuteSqlCommand(" @Qty, @ItemMultiMRPId, @WarehouseId, @OrderId, @OrderDispatchedDetailsId", parameters);
            }

            return true;
        }
        private bool IsValidConditionList(long stockTransactionMasterId, Audit entity)
        {
            StockTransactionManager stockTransactionManager = new StockTransactionManager();
            List<StockTransactionCondition> conditionList = stockTransactionManager.GetConditionListByMasterId(stockTransactionMasterId);
            if (conditionList != null && conditionList.Any())
            {
                bool isValidOperationResult = true;
                foreach (var condition in conditionList)
                {
                    isValidOperationResult = isValidOperationResult && IsValid(condition, entity);
                    if (!isValidOperationResult)
                    {
                        break;
                    }
                }
                return isValidOperationResult;
            }
            else
            {
                return false;
            }

        }
        private bool IsValid(StockTransactionCondition condition, Audit entity)
        {
            bool isValidResult = false;
            EntityReflactionHelper reflactionHelper = new EntityReflactionHelper();

            if (entity.AuditAction != EntityState.Added.ToString() && entity.AuditFields.FirstOrDefault(x => x.FieldName == condition.PropertyName).NewValue == entity.AuditFields.FirstOrDefault(x => x.FieldName == condition.PropertyName).OldValue)
            {
                return false;
            }

            TypeConverter typeConverter = TypeDescriptor.GetConverter(entity.AuditFields.FirstOrDefault(x => x.FieldName == condition.PropertyName).NewValue);
            string operandOne = reflactionHelper.GetValue(entity, condition.PropertyName).ToString();
            string operandTwo = condition.PropertyValue;
            isValidResult = isValidResult = (operandOne.ToString().ToLower() == operandTwo.ToString().ToLower());
            return isValidResult;
        }
        #endregion not using metods

    }
}