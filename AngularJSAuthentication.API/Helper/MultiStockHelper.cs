using AngularJSAuthentication.Common.Helpers;
using AngularJSAuthentication.DataContracts.Transaction.Stocks;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Transactions;

namespace AngularJSAuthentication.API.Helper
{
    public class MultiStockHelper<T>
    {
        public bool MakeEntry(List<T> entryList, string spName, AuthContext context, TransactionScope scope)
        {
            //using (var scope = new TransactionScope())
            //{
            try
            {

                foreach (T entry in entryList)
                {
                    List<SqlParameter> paramList = GetParamList(entry);
                    string query = "EXEC " + spName + GetParamName(entry);
                    int result = context.Database.SqlQuery<int>(query, paramList.ToArray()).FirstOrDefault();
                    if (result == 0)
                    {
                        TextFileLogHelper.LogError("Stock Hit error : " + Environment.NewLine + JsonConvert.SerializeObject(entry));
                        //scope.Dispose();
                        return false;
                    }
                }
                //context.Commit();

            }
            catch (Exception ex)
            {
                TextFileLogHelper.TraceLog("BEFORE MakeEntry  2: " +
                    ex.InnerException != null ? ex.ToString() + Environment.NewLine + ex.InnerException.ToString() : ex.ToString()
                    );
                //scope.Dispose();
                TextFileLogHelper.TraceLog("BEFORE MakeEntry  2: " + entryList.First().ToString());
                return false;
            }

            //scope.Complete();
            //}

            return true;
        }

        public async Task<bool> MakeEntryAsync(List<T> entryList, string spName, AuthContext context, TransactionScope scope)
        {
            //using (var scope = new TransactionScope())
            //{
            try
            {

                foreach (T entry in entryList)
                {
                    List<SqlParameter> paramList = GetParamList(entry);
                    string query = "EXEC " + spName + GetParamName(entry);
                    int result = await context.Database.SqlQuery<int>(query, paramList.ToArray()).FirstOrDefaultAsync();

                    if (result == 0)
                    {
                        TextFileLogHelper.LogError("Stock Hit error : " + Environment.NewLine + JsonConvert.SerializeObject(entry));
                        //scope.Dispose();
                        return false;
                    }
                }
                //context.Commit();

            }
            catch (Exception ex)
            {
                TextFileLogHelper.TraceLog("BEFORE MakeEntry  2: " +
                    ex.InnerException != null ? ex.ToString() + Environment.NewLine + ex.InnerException.ToString() : ex.ToString()
                    );
                //scope.Dispose();
                TextFileLogHelper.TraceLog("BEFORE MakeEntry  2: " + entryList.First().ToString());
                return false;
            }

            //scope.Complete();
            //}

            return true;
        }

        public bool MakeEntryDependableTransaction(List<T> entryList, string spName, object transaction)
        {

            //Create a DependentTransaction from the object passed to the WorkerThread
            DependentTransaction dTx = (DependentTransaction)transaction;

            using (var scope = new TransactionScope(dTx))
            {
                try
                {
                    using (var context = new AuthContext())
                    {
                        foreach (T entry in entryList)
                        {
                            List<SqlParameter> paramList = GetParamList(entry);
                            string query = spName + GetParamName(entry);
                            int result = context.Database.SqlQuery<int>(query, paramList.ToArray()).FirstOrDefault();
                            if (result == 0)
                            {
                                scope.Dispose();
                                return false;
                            }
                        }
                        //context.Commit();
                    }
                }
                catch (Exception ex)
                {
                    TextFileLogHelper.TraceLog("BEFORE MakeEntry  2: " + ex.ToString());
                    scope.Dispose();
                    return false;
                }

                scope.Complete();
            }

            return true;
        }


        private List<SqlParameter> GetParamList(T entry)
        {
            List<SqlParameter> paramList = null;
            if (entry != null)
            {
                paramList = new List<SqlParameter>();
                List<PropertyInfo> propertyInfoList = entry.GetType().GetProperties().ToList();
                foreach (var item in propertyInfoList)
                {
                    var param = new SqlParameter("@" + item.Name, item.GetValue(entry, null));
                    paramList.Add(param);
                }
            }
            return paramList;
        }

        private string GetParamName(T entry)
        {
            string paramName = "";
            if (entry != null)
            {
                List<PropertyInfo> propertyInfoList = entry.GetType().GetProperties().ToList();
                foreach (var item in propertyInfoList)
                {
                    if (string.IsNullOrEmpty(paramName))
                    {
                        paramName += " @" + item.Name;
                    }
                    else
                    {
                        paramName += " ," + "@" + item.Name;
                    }

                }
            }
            return paramName;
        }

        public bool MakeBulkEntryOnPicker(List<RTDOnPickedDc> list, AuthContext context)
        {
            if (list != null && list.Any())
            {
                DataTable dt = new DataTable();
                dt.Columns.Add("WarehouseId");
                dt.Columns.Add("ItemMultiMRPId");
                dt.Columns.Add("Qty");
                dt.Columns.Add("OrderDispatchedDetailsId");
                dt.Columns.Add("UserId");
                dt.Columns.Add("OrderId");
                dt.Columns.Add("IsFreeStock");
                dt.Columns.Add("IsDispatchFromPlannedStock");
                dt.Columns.Add("RefStockCode");

                foreach (var item in list)
                {
                    var dr = dt.NewRow();
                    dr["WarehouseId"] = item.WarehouseId;
                    dr["ItemMultiMRPId"] = item.ItemMultiMRPId;
                    dr["Qty"] = item.Qty;
                    dr["OrderDispatchedDetailsId"] = item.OrderDispatchedDetailsId;
                    dr["UserId"] = item.UserId;
                    dr["OrderId"] = item.OrderId;
                    dr["IsFreeStock"] = item.IsFreeStock;
                    dr["IsDispatchFromPlannedStock"] = item.IsDispatchFromPlannedStock;
                    dr["RefStockCode"] = item.RefStockCode;
                    dt.Rows.Add(dr);
                }

                var stockTableParam = new SqlParameter("StockTable", dt);
                stockTableParam.SqlDbType = SqlDbType.Structured;
                stockTableParam.TypeName = "dbo.VirtualStockType";

                var isDoneParam = new SqlParameter("IsDone", SqlDbType.Bit);
                isDoneParam.Direction = ParameterDirection.Output;
                bool isSuccess = false;

                context.Database.CommandTimeout = 240;
                context.Database.ExecuteSqlCommand("Stock_OnRTD_Bulk_Update @StockTable, @IsDone OUTPUT", stockTableParam, isDoneParam);
                isSuccess = Convert.ToBoolean(isDoneParam.Value);

                return isSuccess;
            }
            else
            {
                return true;
            }
        }

        public bool MakeBulkEntryOnPickerIssued(List<OnIssuedStockEntryDc> list, AuthContext context)
        {
            if (list != null && list.Any())
            {
                DataTable dt = new DataTable();
                dt.Columns.Add("WarehouseId");
                dt.Columns.Add("ItemMultiMRPId");
                dt.Columns.Add("Qty");
                dt.Columns.Add("OrderDispatchedDetailsId");
                dt.Columns.Add("UserId");
                dt.Columns.Add("OrderId");
                dt.Columns.Add("RefStockCode");
                dt.Columns.Add("IsDeliveryRedispatch");


                foreach (var item in list)
                {
                    var dr = dt.NewRow();
                    dr["WarehouseId"] = item.WarehouseId;
                    dr["ItemMultiMRPId"] = item.ItemMultiMRPId;
                    dr["Qty"] = item.Qty;
                    dr["OrderDispatchedDetailsId"] = item.OrderDispatchedDetailsId;
                    dr["UserId"] = item.UserId;
                    dr["OrderId"] = item.OrderId;
                    dr["RefStockCode"] = item.RefStockCode;
                    dr["IsDeliveryRedispatch"] = item.IsDeliveryRedispatch;
                    dt.Rows.Add(dr);
                }

                var stockTableParam = new SqlParameter("StockTable", dt);
                stockTableParam.SqlDbType = SqlDbType.Structured;
                stockTableParam.TypeName = "dbo.VirtualIssuedStockType";

                var isDoneParam = new SqlParameter("IsDone", SqlDbType.Bit);
                isDoneParam.Direction = ParameterDirection.Output;
                bool isSuccess = false;

                context.Database.CommandTimeout = 240;
                context.Database.ExecuteSqlCommand("Stock_OnIssued_Bulk_Update @StockTable, @IsDone OUTPUT", stockTableParam, isDoneParam);
                isSuccess = Convert.ToBoolean(isDoneParam.Value);


                return isSuccess;
            }
            else
            {
                return true;
            }
        }

    }
}