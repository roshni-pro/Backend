using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.Masters.Batch
{
    public class BatchDc
    {
        public long BatchMasterId { get; set; }
        public string BatchCode { get; set; }
        public string ItemNumber { get; set; }
        public DateTime? MFGDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
    }
}
