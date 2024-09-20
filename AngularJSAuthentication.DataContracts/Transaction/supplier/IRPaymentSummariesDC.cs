using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.Transaction.supplier
{
   public class IRPaymentSummariesDC
    {
        public int Id { get; set; }
        public double? TotalAmount { get; set; }
        public DateTime? PaymentDate { get; set; }
        public bool IsActive { get; set; }
        public bool? IsIROutstandingPending { get; set; }

    }
    public class IrPaymentSummaryPaginator
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int SkipCount { get; set; }
        public int Take { get; set; }
    }
    public class IrPaymentSummaryListDC
    {
        public int Count { get; set; }
        public List<IRPaymentSummariesDC> IRPaymentSummaryList { get; set; }
    }
}
