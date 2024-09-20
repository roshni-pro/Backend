using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using AngularJSAuthentication.DataContracts.CustomerGroup;

namespace AngularJSAuthentication.API.Helper.CustomerGroup
{
    public class CustomerGroupOrderCount : CustomerGroupCommon, ICustomerGroup
    {
        public string GetQueryPart(CustomerGroupCriteria customerGroupCriteria)
        {
            if (customerGroupCriteria.OrderCount.HasValue && !string.IsNullOrEmpty(customerGroupCriteria.ConditionValue))
            {
                string condition = customerGroupCriteria.ConditionValue;
                string orderCount = customerGroupCriteria.OrderCount.Value.ToString();
                string warehouseQuery = GetWarehouseSubQuery(customerGroupCriteria);
                string dateFilterQuery = GetDateFilterSubQuery("o.CreatedDate", customerGroupCriteria);
                string query = (orderCount== "0"?" Not ": "") +  $@"exists
                                    (
                                        Select 1 from OrderMasters o with (nolock) where a.CustomerId=o.CustomerId
                                        {dateFilterQuery}
	                                    group by o.CustomerId
	                                    having count(o.orderid) {condition} {orderCount} {warehouseQuery}
                                    )";

                return query;
            }
            else
            {
                throw new Exception("Wrong amount criteria");
            }

        }
    }
}