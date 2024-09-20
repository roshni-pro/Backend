using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.APIParams
{
    public class CustomerLedgerConsentMsgSendDC
    {
        public string Name { get; set; }
        public string Skcode { get; set; }
        public string MobileNo { get; set; }
        public DateTime? MessageSendDate { get; set; }
        public bool IsMessageSend { get; set; }
        public string Consent { get; set; }
        public int  ConsentStatus { get; set; }
        public DateTime LedgerStartDate { get; set; }
        public DateTime LedgerEndDate { get; set; }
    }
}
