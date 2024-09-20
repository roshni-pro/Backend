using NLog;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Text;

namespace AngularJSAuthentication.API.Helpers
{
    public class BulkInsertHelper
    {
        private static string connStr => ConfigurationManager.ConnectionStrings["AuthContext"].ConnectionString;
        public static Logger logger = LogManager.GetCurrentClassLogger();

        public bool BulkInsert(DataTable dataTable, string tableName)
        {
            bool isSuccess = true;
            try
            {
                using (var sqlCon = new SqlConnection(connStr))
                {
                    using (SqlBulkCopy sqlbc = new SqlBulkCopy(sqlCon))
                    {
                        sqlbc.DestinationTableName = tableName;
                        foreach (DataColumn item in dataTable.Columns)
                        {
                            sqlbc.ColumnMappings.Add(item.ColumnName, item.ColumnName);
                        }

                        if (sqlCon.State != ConnectionState.Open)
                            sqlCon.Open();
                        sqlbc.WriteToServer(dataTable);
                        sqlCon.Close();
                    }

                }
            }
            catch (System.Exception ex)
            {
                logger.Error(new StringBuilder("Error while Saving ").Append(tableName).Append("  ").Append(ex.ToString()).ToString());
                isSuccess = false;
            }
            return isSuccess;
        }
    }
}