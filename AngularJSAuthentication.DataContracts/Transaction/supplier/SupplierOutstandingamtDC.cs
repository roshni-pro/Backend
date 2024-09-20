using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.Transaction.supplier
{
   public class SupplierOutstandingamtDC
    {
        public long ID { get; set; } 
        public int SupplierId { get; set; }
        public double? Credit { get; set; }
        public double? Debit { get; set; }
        public double? OutStandingAmt { get; set; }
        public DateTime CreatedDate { get; set; }
        public string Name { get; set; }
        public string SUPPLIERCODES { get; set; }


    }
}
