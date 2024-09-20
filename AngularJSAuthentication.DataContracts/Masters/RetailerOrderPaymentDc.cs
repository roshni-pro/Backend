using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.Masters
{
    public class RetailerOrderPaymentDc
    {
        public int OrderId { get; set; }
        public string GatewayTransId { get; set; } //
        public double amount { get; set; }
        public string status { get; set; }
        public string PaymentFrom { get; set; }
        public string ChequeImageUrl { get; set; }
        public string ChequeBankName { get; set; }
        public bool IsOnline { get; set; }
        public bool IsRTGSflag { get; set; }
        public DateTime TxnDate { get; set; }
        public int PaymentResponseRetailerAppId { get; set; }

    }
}
