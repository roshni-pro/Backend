using AngularJSAuthentication.DataContracts.CustomerGroup;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using static AngularJSAuthentication.API.Controllers.SalesGroup.SalesGroupController;

namespace AngularJSAuthentication.API.Helper.CustomerGroup
{
    public class CustomerGroupManager
    {
        public List<CustomerDetailDc> GetCustomerDetail(List<CustomerGroupCriteria> criteriaList)
        {
            List<CustomerDetailDc> list = null;
            string query = @"Select a.CustomerId,a.Skcode,a.ShopName,W.WarehouseName,a.City from Customers a with (nolock) INNER JOIN Warehouses W with (nolock) ON W.WarehouseId = a.Warehouseid ";
            if (criteriaList != null && criteriaList.Any())
            {
                int count = 0;
                foreach (var criteria in criteriaList)
                {
                    string subQuery = "";
                    ICustomerGroup customerGroup = null;
                    switch (criteria.TypeCode)
                    {
                        case (int)CustomerGroupTypeEnum.OrderAmount:
                            customerGroup = new CustomerGroupOrderAmount();
                            break;
                        case (int)CustomerGroupTypeEnum.OrderCount:
                            customerGroup = new CustomerGroupOrderCount();
                            break;
                        case (int)CustomerGroupTypeEnum.Category:
                            customerGroup = new CustomerGroupCategory();
                            break;
                        case (int)CustomerGroupTypeEnum.SubCategory:
                            customerGroup = new CustomerGroupSubcategory();
                            break;
                        case (int)CustomerGroupTypeEnum.Brand:
                            customerGroup = new CustomerGroupBrand();
                            break;
                    }
                    subQuery = customerGroup.GetQueryPart(criteria);
                    if(count++ == 0)
                    {
                        query = query + " WHERE " + subQuery;
                    }
                    else
                    {
                        query = query + " AND " + subQuery;
                    }
                }
            
                using(var context = new AuthContext())
                {
                    list  = context.Database.SqlQuery<CustomerDetailDc>(query).ToList();
                }
            }
            return list;
        }
    }
}