using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.Masters
{
    public class AssignmentOrderListPayDC
    {
       
        public int OrderId { get; set; }
        public string CustomerName { get; set; }
        public string Skcode { get; set; }
        public string Status { get; set; }
        public double GrossAmount { get; set; }
        public string PaymentFrom { get; set; }
        public  double amount { get; set; }
        public DateTime OrderedDate { get; set; }
        public double? ChequeAmount { get; set; }
        public double? CashAmount { get; set; }
        public double? OnlineAmount { get; set; }
    }


  
}
