using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.Transaction.PurchaseRequest
{
    public class PurchaseRequestSettlementPageDc
    {
        public int SupplierId { get; set; }
        public string SupplierName { get; set; }
        public string SupplierCode { get; set; }
        public int PurchaseOrderId { get; set; }
        public double RemainingAmount { get; set; }
        public DateTime CreationDate { get; set; }
        public DateTime PaymentDate { get; set; }
        public int PaidAmount { get; set; }
        public long PurchaseRequestPaymentId { get; set; }
    }

    public class PurchaseRequestPaymentSettlementDc
    {
        public int SupplierId { get; set; }
        public int PurchaseOrderId { get; set; }
        public long PurchaseRequestPaymentId { get; set; }
        public int AfterSettleRemainingAmount { get; set; }
        public List<ChildIrMasterDc> PaymentList { get; set; }
    }

    public class ChildIrMasterDc
    {
        public int IRMasterId { get; set; }
        public int PayingAmount { get; set; }
    }
}
