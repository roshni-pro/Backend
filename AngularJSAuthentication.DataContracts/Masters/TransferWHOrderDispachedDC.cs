using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.Masters
{
   public class TransferWHOrderDispachedDC
    {
        public int TransferOrderId { get; set; }
        public string vehicleType { get; set; }
        public string vehicleNumber { get; set; }
        public double fread { get; set; }
        public string EwaybillNumber { get; set; }
        public int CreatedById { get; set; }
        public List<TransferWHOrderDispachedDetailsDC> TransferWHOrderDetailss { get; set; }
    }
}
