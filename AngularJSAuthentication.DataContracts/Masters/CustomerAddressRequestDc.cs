using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.Masters
{
    public class CustomerAddressRequestDc
    {
        public long CustomerAddressRequestId { get; set; }
        public int CustomerId { get; set; }
        public double StoredLat { get; set; }
        public double StoredLng { get; set; }
        public string ShippingAddress { get; set; }
        public string Skcode { get; set; }


        public string ImagePath { get; set; }
        public double? NewLat { get; set; }
        public double? NewLng { get; set; }
        public int? PeopleId { get; set; }
    }
}
