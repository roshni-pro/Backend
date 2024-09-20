using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.Transaction.Ledger
{
    public class CustomerLedgerConsentDetailsDC
    {
        public long Id { get; set; }
        public int MasterId { get; set; }
        public int CustomerId { get; set; }
        public string Consent { get; set; }
    }
}
