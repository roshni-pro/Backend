using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.Transaction.Ledger
{
    public class IRPayementRevertDc
    {
        public int? IRPaymentDetailsId { get; set; }
        public double?  CreditAmount { get; set; }
    }
}
