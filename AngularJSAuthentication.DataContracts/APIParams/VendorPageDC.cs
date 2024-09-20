using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.APIParams
{
    public class VendorPageDC
    {
        public int Count { get; set; }
        public List<VendorDC> PageList { get; set; }
    }
}
