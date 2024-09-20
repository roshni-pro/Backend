using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.Transaction.Ledger
{
    public class CustomerOTPVerification
    {
        public int CustomerId { get; set; }
        public int OTP { get; set; }
        public string GUID { get; set; }
        public DateTime ToDate { get; set; }
        public DateTime FromDate { get; set; }
    }
}
