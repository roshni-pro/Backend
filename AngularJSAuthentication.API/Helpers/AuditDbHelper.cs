using AngularJSAuthentication.Model.Base.Audit;
using System.Collections.Generic;

namespace AngularJSAuthentication.API.Helpers
{
    public class AuditDbHelper
    {
        internal bool InsertAuditLogs(List<Audit> auditList)
        {
            //DataTable dt = new DataTable();
            //dt.Columns.Add("PkValue");
            //dt.Columns.Add("PkFieldName");
            //dt.Columns.Add("AuditDate");
            //dt.Columns.Add("UserName");
            //dt.Columns.Add("AuditAction");
            //dt.Columns.Add("AuditEntity");
            //dt.Columns.Add("TableName");
            //dt.Columns.Add("GUID");

            //foreach (var item in auditList)
            //{
            //    DataRow row = dt.NewRow();
            //    row["PkValue"] = item.PkValue;
            //    row["PkFieldName"] = item.PkFieldName;
            //    row["AuditDate"] = item.AuditDate;
            //    row["UserName"] = item.UserName;
            //    row["AuditAction"] = item.AuditAction;
            //    row["AuditEntity"] = item.AuditEntity;
            //    row["TableName"] = item.TableName;
            //    row["GUID"] = item.GUID;

            //    dt.Rows.Add(row);
            //}

            //BulkInsertHelper bulkInsertHelper = new BulkInsertHelper();
            //var isSuccess = bulkInsertHelper.BulkInsert(dt, "Audits");

            //if (isSuccess)
            //{
            //    var auditFieldsList = auditList.SelectMany(x => x.AuditFields).ToList();

            //    DataTable fieldsDt = new DataTable();
            //    fieldsDt.Columns.Add("FieldName");
            //    fieldsDt.Columns.Add("OldValue");
            //    fieldsDt.Columns.Add("NewValue");
            //    fieldsDt.Columns.Add("AuditGuid");

            //    foreach (var item in auditFieldsList)
            //    {
            //        DataRow row = fieldsDt.NewRow();
            //        row["FieldName"] = item.FieldName;
            //        row["OldValue"] = item.OldValue;
            //        row["NewValue"] = item.NewValue;
            //        row["AuditGuid"] = item.AuditGuid;
            //        fieldsDt.Rows.Add(row);
            //    }

            //    bulkInsertHelper.BulkInsert(fieldsDt, "AuditFields");
            //}

            MongoDbHelper<Audit> mongoDbHelper = new MongoDbHelper<Audit>();
            foreach (var item in auditList)
            {
                mongoDbHelper.Insert(item, string.Format("{0}_Audit", item.AuditEntity));
            }

            return true;
        }

    }
}