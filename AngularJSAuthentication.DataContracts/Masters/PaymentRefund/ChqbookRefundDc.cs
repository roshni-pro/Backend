using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.Masters.PaymentRefund
{

    public class ChqbookRefundDc
    {
        public string GatewayTransId { set; get; }
        public string OrderId { set; get; }
    }
    public class RefundSuccessData
    {
        public string message { get; set; }
        public List<object> results { get; set; }
    }

    public class ChqbookBookRefundResponse
    {
        public RefundSuccessData data { get; set; }
    }

    public class RefundError
    {
        public List<string> errors { get; set; }
    }
    public class ChqbookResDC
    {
        public bool status { get; set; }
        public string message { get; set; }
        public string RequestMsg { get; set; }
        public string ResponseMsg { get; set; }
    }
}
