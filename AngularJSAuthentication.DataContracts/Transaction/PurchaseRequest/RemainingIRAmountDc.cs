using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.Transaction.PurchaseRequest
{
    public class RemainingIRAmountDc
    {
        public double TotalAmountRemaining { get; set; }
        public int SettleAmount { get; set; }
        public string InvoiceNumber { get; set; }
        public DateTime? InvoiceDate { get; set; }
        public int PurchaseOrderId { get; set; }
        public int IRMasterId { get; set; }
    }
}
