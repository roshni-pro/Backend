using AngularJSAuthentication.DataContracts.constants;
using AngularJSAuthentication.DataContracts.Transaction.Stocks;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace AngularJSAuthentication.API.Helper.Stock
{
    public class StockHistoryHelper
    {
        public StockHistoryListDc GetStockList(StockHistoryPageFilterDc filter)
        {
            string spName = "";
            StockHistoryListDc stockHistoryListDc = null;
            string tableName = filter.StockType;
            if (filter == null)
            {
                return stockHistoryListDc;
            }

            if (
                    filter.StockType == StockTypeTableNames.CurrentStocks
                    || filter.StockType == StockTypeTableNames.FreebieStock
                    || filter.StockType == StockTypeTableNames.DamagedStock
                    || filter.StockType == StockTypeTableNames.InTransitStock
                    || filter.StockType == StockTypeTableNames.VirtualStock
                    || filter.StockType == StockTypeTableNames.NonSellableStock
                    || filter.StockType == StockTypeTableNames.ClearanceStockNews
                    || filter.StockType == StockTypeTableNames.InventoryReserveStocks
                    || filter.StockType == StockTypeTableNames.ClearancePlannedStocks
                    || filter.StockType == StockTypeTableNames.NonRevenueStocks

               )
            {
                spName = "Stock_Fetch_GetHistory_Diferrent";
            }

            else
            {
                spName = "Stock_Fetch_GetHistory_Similar";
            }

            using (var context = new AuthContext())
            {
                context.Database.CommandTimeout = 180;
                stockHistoryListDc = new StockHistoryListDc();
                string spNameList;
                List<SqlParameter> paramList = GetSqlParamList(filter, 0, tableName, spName, out spNameList);
                stockHistoryListDc.PageList = context.Database.SqlQuery<StockHistoryPageContentDc>(spNameList, paramList.ToArray()).ToList();

                string spNameCount;
                paramList = GetSqlParamList(filter, 1, tableName, spName, out spNameCount);
                StockHistoryListDc count = context.Database.SqlQuery<StockHistoryListDc>(spNameCount, paramList.ToArray()).FirstOrDefault();
                stockHistoryListDc.TotalRecords = count.TotalRecords;
                return stockHistoryListDc;
            }

        }



        public StockHistoryListDc GetCurrentStockList(StockHistoryPageFilterDc filter)
        {
            return null;
        }

        private List<SqlParameter> GetSqlParamList(StockHistoryPageFilterDc filter, int isGetCount, string tableName, string spName, out string spNameWithParam)
        {
            List<SqlParameter> paramList = new List<SqlParameter>();
            spNameWithParam = "EXEC " + spName + " @TableName";
            paramList.Add(new SqlParameter("@TableName", tableName));
            paramList.Add(new SqlParameter("@IsGetCount", isGetCount));
            spNameWithParam += ", @IsGetCount";
            if (!filter.Skip.HasValue)
            {
                filter.Skip = 0;
            }

            paramList.Add(new SqlParameter("@Skip", filter.Skip.Value));
            spNameWithParam += ", @Skip";
            if (!filter.Take.HasValue)
            {
                filter.Take = 10;
            }
            paramList.Add(new SqlParameter("@Take", filter.Take.Value));
            spNameWithParam += ", @Take";

            if (!filter.ItemMultiMRPId.HasValue)
            {
                filter.ItemMultiMRPId = 0;
            }
            paramList.Add(new SqlParameter("@ItemMultiMRPId", filter.ItemMultiMRPId.Value));
            spNameWithParam += ", @ItemMultiMRPId";

            if (!filter.WarehouseId.HasValue)
            {
                filter.WarehouseId = 0;
            }
            paramList.Add(new SqlParameter("@WarehouseId", filter.WarehouseId.Value));
            spNameWithParam += ", @WarehouseId";

            if (!filter.UserId.HasValue)
            {
                filter.UserId = 0;
            }
            paramList.Add(new SqlParameter("@UserId", filter.UserId.Value));
            spNameWithParam += ", @UserId";

            if (!string.IsNullOrEmpty(filter.RefStockType))
            {
                paramList.Add(new SqlParameter("@RefStockCode", filter.RefStockType));
                spNameWithParam += ", @RefStockCode";
            }

            return paramList;
        }
    }
}