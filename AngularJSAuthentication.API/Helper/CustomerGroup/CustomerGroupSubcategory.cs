using AngularJSAuthentication.DataContracts.CustomerGroup;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AngularJSAuthentication.API.Helper.CustomerGroup
{
    public class CustomerGroupSubcategory : CustomerGroupCommon, ICustomerGroup
    {
        public string GetQueryPart(CustomerGroupCriteria customerGroupCriteria)
        {
            if (!string.IsNullOrEmpty(customerGroupCriteria.SelectedSubCategoryName)
                && customerGroupCriteria.OrderValue.HasValue
                && !string.IsNullOrEmpty(customerGroupCriteria.ConditionValue))
            {
                string condition = customerGroupCriteria.ConditionValue;
                string subcategoryName = customerGroupCriteria.SelectedSubCategoryName;
                string orderValue = customerGroupCriteria.OrderValue.Value.ToString();
                string warehouseQuery = GetWarehouseSubQuery(customerGroupCriteria);
                string dateFilterQuery = GetDateFilterSubQuery("o.CreatedDate", customerGroupCriteria);

                string query = $@"Exists
                                (
                                  Select 1 from OrderDetails o with (nolock) 
                                  where a.CustomerId=o.CustomerId
                                  {dateFilterQuery}
                                  and o.SubcategoryName='{subcategoryName}' 
                                  group by o.CustomerId
                                  having sum(o.qty * o.unitprice) {condition} {orderValue} {warehouseQuery}
                                )";

                return query;
            }
            else
            {
                throw new Exception("Wrong categry criteria");
            }

        }
    }
}