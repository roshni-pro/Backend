using AngularJSAuthentication.Common.Helpers;
using AngularJSAuthentication.DataContracts.Transaction.Stocks;
using AngularJSAuthentication.Model.Base.Audit;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace AngularJSAuthentication.API.Helper.Stock
{
    public class StockContextHelper
    {
        public bool MakeEntries(List<Audit> auditEntityList, StockEnum stockEnum, AuthContext context)
        {
            StockConfigMasterHelper masterHelper = new StockConfigMasterHelper();
            StockConfigDc stockConfig = masterHelper.GetByStockEnum(stockEnum, context);

            List<Audit> masterAuditList = auditEntityList.Where(x => x.AuditEntity == stockConfig.Master.MasterEntityName).ToList();
            List<Audit> helperAuditList = auditEntityList.Where(x => x.AuditEntity != stockConfig.Master.MasterEntityName).ToList();

            if (masterAuditList != null && masterAuditList.Count > 0)
            {
                foreach (Audit masterAudit in masterAuditList)
                {
                    List<SqlParameter> paramList = GetParamList(stockConfig, masterAudit, helperAuditList);
                    string spNameWithParam = stockConfig.Master.SpName + GetParamName(stockConfig);
                    bool result = MakeEntry(paramList, spNameWithParam, context);
                    if (!result)
                    {
                        return false;
                    }
                }
            }
            return true;
        }


        private bool MakeEntry(List<SqlParameter> paramList, string spNameWithParam, AuthContext context)
        {
            //using (var scope = new TransactionScope())
            //{
            try
            {
                int result = context.Database.SqlQuery<int>(spNameWithParam, paramList.ToArray()).FirstOrDefault();
                if (result == 0)
                {
                    //scope.Dispose();
                    return false;
                }
            }
            catch (Exception ex)
            {
                TextFileLogHelper.TraceLog("BEFORE MakeEntry  2: " +
                    ex.InnerException != null ? ex.ToString() + Environment.NewLine + ex.InnerException.ToString() : ex.ToString()
                    );
                //scope.Dispose();
                TextFileLogHelper.TraceLog("BEFORE MakeEntry  2: " + spNameWithParam);
                return false;
            }

            //scope.Complete();
            //}

            return true;
        }

        private List<SqlParameter> GetParamList(StockConfigDc stockConfig, Audit masterAudit, List<Audit> helperAuditList)
        {
            List<SqlParameter> paramList = null;

            if (stockConfig != null && stockConfig.DetailList != null && stockConfig.DetailList.Any())
            {
                paramList = new List<SqlParameter>();
                foreach (var item in stockConfig.DetailList)
                {
                    Object value = null;
                    Audit audit = null;
                    if (item.EntityName == masterAudit.AuditEntity)
                    {
                        audit = masterAudit;
                    }
                    else
                    {
                        audit = helperAuditList.FirstOrDefault(x => x.AuditEntity == item.EntityName);
                    }
                    value = audit.AuditFields.FirstOrDefault(x => x.FieldName == item.PropertyName).NewValue;
                    var param = new SqlParameter("@" + item.ParamName, value);
                    paramList.Add(param);
                }
            }
            return paramList;
        }

        private string GetParamName(StockConfigDc stockConfig)
        {
            string paramName = "";
            if (stockConfig != null && stockConfig.DetailList != null && stockConfig.DetailList.Any())
            {
                foreach (var item in stockConfig.DetailList)
                {
                    if (string.IsNullOrEmpty(paramName))
                    {
                        paramName += " @" + item.ParamName;
                    }
                    else
                    {
                        paramName += " ," + "@" + item.ParamName;
                    }

                }
            }
            return paramName;
        }
    }
}