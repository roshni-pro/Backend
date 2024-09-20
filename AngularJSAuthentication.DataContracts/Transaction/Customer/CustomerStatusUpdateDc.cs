using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.Transaction.Customer
{
    public class CustomerStatusUpdateDc
    {
        public int CustomerId { set; get; }
        public string CustomerVerify { set; get; }
        public string CustomerType { set; get; }
        public bool IsActive { set; get; }
        public int CustomerDocumentStatus { set; get; }
        public int ShippingAddressStatus { set; get; }
        public string ShopName { set; get; }
        public string TypeOfBuissness { set; get; }
        //public long ChannelMasterId { get; set; }
    }
}
