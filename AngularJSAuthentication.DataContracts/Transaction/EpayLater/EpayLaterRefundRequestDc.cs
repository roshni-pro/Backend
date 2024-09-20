using System;

namespace AngularJSAuthentication.DataContracts.Transaction.EpayLater
{
    public class EpayLaterRefundRequestDc
    {
        public string marketplaceOrderId { get; set; }
        public double returnAmount { get; set; }
        public DateTime returnAcceptedDate { get; set; }
        public DateTime returnShipmentReceivedDate { get; set; }
        public DateTime refundDate { get; set; }
        public string returnType { get; set; }

    }
}
