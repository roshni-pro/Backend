using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.RazorPay.POS
{
    public class NotificationResponse
    {
        public bool success { get; set; }
        public string messageCode { get; set; }
        public string message { get; set; }
        public string errorCode { get; set; }
        public string errorMessage { get; set; }
        public string realCode { get; set; }
        public string apiMessageTitle { get; set; }
        public string apiMessage { get; set; }
        public string apiMessageText { get; set; }
        public string apiWarning { get; set; }
        public string p2pRequestId { get; set; }
    }
    public class NotificationStatusResponse
    {
        public string status { get; set; }
        public string txnId { get; set; }
        public string P2PRequestId { get; set; }
        public string messageCode { get; set; }

    }
}
