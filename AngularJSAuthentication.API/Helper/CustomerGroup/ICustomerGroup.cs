using AngularJSAuthentication.DataContracts.CustomerGroup;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.API.Helper.CustomerGroup
{
    public interface ICustomerGroup
    {
        string GetQueryPart(CustomerGroupCriteria customerGroupCriteria);
    }
}
