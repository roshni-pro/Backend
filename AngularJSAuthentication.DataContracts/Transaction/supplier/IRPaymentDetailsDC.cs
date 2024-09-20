using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.Transaction.supplier
{
    public class IRPaymentDetailsDC
    {
        public int Id { get; set; }
        public int SupplierId { get; set; }
        public int BankId { get; set; }
        public string BankName { get; set; }
        public string RefNo { get; set; }
        public double TotalAmount { get; set; }
        public double? TotalReaminingAmount { get; set; }
        public string Remark { get; set; }

        public string IRList { get; set; }
        public DateTime CreatedDate { get; set; }

        public string SupplierName { get; set; }
        public string SupplierCodes { get; set; }
        public DateTime? PaymentDate { get; set; }

        public bool? IsIROutstandingPending { get; set; }
        public string PaymentStatus { get; set; } //Approved, Rejected, Pending
        public int IRPaymentSummaryId { get; set; }

        public bool? IsLedger { get; set; }

        public bool? IsApproved {get; set;}

        public string WarehouseName { get; set; }
        public double TDSAmount { get; set; }
        public string Bank_Ifsc { get; set; }
        public string Bank_AC_No { get; set; }
        public int PurchaseOrderId { get; set; }


    }
}
