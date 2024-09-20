using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.Transaction.PurchaseOrder
{
    public class POReturnRequestPager
    {
        public string CancelType { get; set; }
        public int Take { get; set; }
        public int Skip { get; set; }
      
    }
}
