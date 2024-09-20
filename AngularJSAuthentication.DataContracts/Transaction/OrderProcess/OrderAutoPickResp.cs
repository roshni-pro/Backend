using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.Transaction.OrderProcess
{
    public class OrderAutoPickResp
    {   
        public int OrderId { set; get; }
        public int OrderDetailId { set; get; }
        public bool IsProcess { set; get; }
        public string ErrorMsg { set; get; }
    }
}
