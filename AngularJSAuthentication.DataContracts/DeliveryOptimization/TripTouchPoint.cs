using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.DeliveryOptimization
{
    public class TripTouchPointRearrange
    {
        public long Id { get; set; }
        public string Skcode { get; set; }
        public string ShopName { get; set; }
        public string OrderList { get; set; }
        public string ShippingAddress { get; set; }
        public int SequenceNo { get; set; }
    }
}
