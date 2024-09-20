using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.Masters
{
    public class CustomerAddressOperationRequestVM
    {
        public int CustomerId { get; set; }
        public long RequestId { get; set; }
        public int WarehouseId { get; set; }
        public string Skcode { get; set; }
        public double? NewLat { get; set; }
        public double? NewLng { get; set; }
        public double? CustomerLat { get; set; }
        public double? CustomerLng { get; set; }
        public DateTime CreatedDate { get; set; }
        public string CreatedBy { get; set; }
        public string BillingAddress { get; set; }
        public Boolean IsApproved { get; set; }
        public Boolean IsRejected { get; set; }

    }
}
