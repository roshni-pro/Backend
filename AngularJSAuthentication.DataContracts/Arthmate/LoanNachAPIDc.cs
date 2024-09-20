using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.Arthmate
{
  
    public class LoanNachAPIDc
    {
        public string umrn { get; set; } //Mandatory
        public string mandate_ref_no { get; set; }
        public string nach_amount { get; set; }
        public string nach_registration_status { get; set; }
        public string nach_status_desc { get; set; }
        public string nach_account_holder_name { get; set; }
        public string nach_account_num { get; set; }
        public string nach_ifsc { get; set; }
        public string nach_start { get; set; }
        public string nach_end { get; set; }
    }

    public class LoanNachResponseDC
    {
        public bool success { get; set; }
        public string message { get; set; }
    }
}
