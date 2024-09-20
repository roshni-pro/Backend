using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.Transaction.Customer
{
    public class CustomerBillingAddressDc
    {
        public int CustomerId { set; get; }
        public string BillingAddress1 { set; get; }
        public string BillingAddress { set; get; }
        public string BillingState { set; get; }
        public string BillingZipCode { set; get; }
        public string BillingCity { set; get; }

    }
}
