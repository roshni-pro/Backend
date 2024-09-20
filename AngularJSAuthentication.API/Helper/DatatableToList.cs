using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;

namespace AngularJSAuthentication.API.Helper
{
    public class DatatableToList
    {

        // Method to convert DataTable to List<T>
        public static List<T> ConvertDataTableToList<T>(DataTable dataTable) where T : new()
        {
            List<T> dataList = new List<T>();

            foreach (DataRow row in dataTable.Rows)
            {
                T item = new T();

                foreach (DataColumn column in dataTable.Columns)
                {
                    // Assuming property names in your class match the column names in the DataTable
                    var property = item.GetType().GetProperty(column.ColumnName);
                    if (property != null)
                    {
                     
                        object value = row[column];
                        property.SetValue(item, value is DBNull ? null : value);
                    }
                }

                dataList.Add(item);
            }

            return dataList;
        }
    }
}