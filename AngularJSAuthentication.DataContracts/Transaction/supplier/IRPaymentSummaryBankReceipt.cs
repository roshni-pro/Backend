using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.Transaction.supplier
{
    public class IRPaymentSummaryBankReceipt
    {
        public string AccountNumber { get; set; }
        public int? TotalAmount { get; set; }
        public string IFSC { get; set; }
        public string SupplierName { get; set; }
        public string SupplierCode { get; set; }
        public string PaymentDate { get; set; }
        public string WarehouseName { get; set; }
    }
}
