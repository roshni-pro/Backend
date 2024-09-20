using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.Masters.PaymentRefund
{
    public class UpdatePaymentRefundDc
    {
       
    }

    public class PostPaymentRefundDc
    {
        public long PaymentRefundRequestId { set; get; }
        public string Source { set; get; }
        public int CustomerId { get; set; }
        public int OrderId { get; set; }
        public double Amount { get; set; }
        public string GatewayTransId { get; set; }
        public string GatewayOrderId { get; set; }
        public string returnType { get; set; } // full, partial
        public int OrderType { get; set; }
        public int WarehouseId { get; set; }

    }

}
