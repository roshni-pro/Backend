using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.APIParams
{
   public class CustomerLedgerMessagePager
    {
        public int SkipCount { get; set; }
        public int Take { get; set; }
        public int Id { get; set; }
        public string Name { get; set; }
       
    }
}
