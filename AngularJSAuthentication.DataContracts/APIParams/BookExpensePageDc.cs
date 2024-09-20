using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.APIParams
{
    public class BookExpensePageDc
    {
        public int Count { get; set; }
        public dynamic PageList { get; set; }
    }
}
