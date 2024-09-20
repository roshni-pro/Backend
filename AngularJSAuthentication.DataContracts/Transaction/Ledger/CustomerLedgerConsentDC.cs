using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.Transaction.Ledger
{
    public class CustomerLedgerConsentDC
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public List<CustomerLedgerConsentDetailsDC> customerLedgerConsentDetails { get; set; }
        public int  Createdby{ get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }


    public class CustomerLedgerConsentDCNew
    {
        public string Skcode { get; set; }
        public int CustomerId { get; set; }
        public string Mobile { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}
