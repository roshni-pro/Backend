using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using AngularJSAuthentication.DataContracts.CustomerGroup;

namespace AngularJSAuthentication.API.Helper.CustomerGroup
{
    public class CustomerGroupOrderAmount : CustomerGroupCommon, ICustomerGroup
    {
        public string GetQueryPart(CustomerGroupCriteria customerGroupCriteria)
        {
            if(customerGroupCriteria.OrderAmount.HasValue && !string.IsNullOrEmpty(customerGroupCriteria.ConditionValue))
            {
                string condition = customerGroupCriteria.ConditionValue;
                string orderAmount = customerGroupCriteria.OrderAmount.Value.ToString();
                string warehouseQuery = GetWarehouseSubQuery(customerGroupCriteria);
                string dateFilterQuery = GetDateFilterSubQuery("o.CreatedDate", customerGroupCriteria);

                string query = (orderAmount == "0" ? " Not " : "") + $@"exists
                                    (
                                        Select 1 from OrderMasters o with (nolock) where a.CustomerId=o.CustomerId
                                        {dateFilterQuery}
	                                    group by o.CustomerId
	                                    having sum(o.GrossAmount) {condition} {orderAmount} {warehouseQuery}
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