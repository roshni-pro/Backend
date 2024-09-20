using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.Transaction.Ledger
{
    public class ChequeBounceVM
    {
        public string ChequeNumber { get; set; }
        public Double? Amount { get; set; }
        public DateTime Date { get; set; }
    }
}
