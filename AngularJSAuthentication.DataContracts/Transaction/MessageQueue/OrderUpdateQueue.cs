using System;

namespace AngularJSAuthentication.DataContracts.Transaction.MessageQueue
{
    public class OrderUpdateQueue
    {
        public int OrderId { get; set; }
        public string Status { get; set; }
        public DateTime UpdateDate { get; set; }
        public string Error { get; set; }
    }
}
