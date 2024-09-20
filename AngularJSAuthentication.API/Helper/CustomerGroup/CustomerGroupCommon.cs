using AngularJSAuthentication.DataContracts.CustomerGroup;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AngularJSAuthentication.API.Helper.CustomerGroup
{
    public abstract class CustomerGroupCommon
    {
        public string GetWarehouseSubQuery(CustomerGroupCriteria customerGroupCriteria)
        {
            string warehouseQuery = "";
            if (customerGroupCriteria.WarehouseId.HasValue)
            {
                warehouseQuery = $" and a.Warehouseid = {customerGroupCriteria.WarehouseId.Value.ToString()}";
            }
            return warehouseQuery;
        }

        public string GetDateFilterSubQuery(string dateField, CustomerGroupCriteria customerGroupCriteria)
        {
            string dateFilterQuery = "";
            if (customerGroupCriteria.StartDate.HasValue && customerGroupCriteria.EndDate.HasValue)
            {
                var endDate = customerGroupCriteria.EndDate.Value.AddDays(1);
                dateFilterQuery = $" and {dateField} between '{customerGroupCriteria.StartDate.Value.ToString("yyyy/MM/dd")}' and '{endDate.ToString("yyyy/MM/dd")}'  ";//Dateadd(day,1,cast('2022/11/01' as date)) 
            }
            return dateFilterQuery;
        }
    }
}