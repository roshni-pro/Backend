using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.Masters
{
   public class GstTCS
    {
        public string Skcode { get; set; }
        public string cusotomername { get; set; }
        public string RefNo { get; set; }
        public string WarehouseName { get; set; }
        public int OrderId { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime  DispatchedDate { get; set; }
        public double GrossAmount { get; set; }
        public double TCSAmount { get; set; }
        public string Status { get; set; }
       public DateTime OrderedDate { get; set; }
    }
}
