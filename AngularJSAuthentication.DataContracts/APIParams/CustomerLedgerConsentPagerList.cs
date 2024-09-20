using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.APIParams
{
   public class CustomerLedgerConsentPagerList
    {
        public int Skip { get; set; }
        public int Take { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string Name { get; set; }
        public bool IsConsent { get; set; }

    }
}
