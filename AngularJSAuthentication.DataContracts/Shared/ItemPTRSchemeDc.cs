using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.Shared
{
    public class ItemPTRSchemeDc
    {
        public string ItemNumber { get; set; }
        public double? PTR { get; set; } 
        public double? BaseScheme { get; set; } 
    }
}
