using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.APIParams
{
    public class BookExpenseDc
    {
        public long ExpenseId { get; set; }
        public long? Id { get; set; }
        public double? TotalAmount { get; set; }
        public long DebitLedgerId { get; set; }
        public long DebitLedgerTypeId { get; set; }
        public double? DebitLedgerAmount { get; set; }
        public int? DepartmentId { get; set; }
        public int? WorkingLocationId { get; set; }
        public int? WorkingCompanyId { get; set; }
        public int? CheckerId { get; set; }
        public Boolean IsChecked { get; set; }
        public Boolean IsLedgerGenerated { get; set; }
        public DateTime ExpenseDate { get; set; }
        public Boolean IsTDSApplied { get; set; }
        public Boolean IsGSTApplied { get; set; }
        public long? TDSLedgerId { get; set; }
        public long? GSTLedgerId { get; set; }
        public double? TDSAmount { get; set; }
        public double? GSTAmount { get; set; }
        public string Status { get; set; }
        public List<BookExpenseDetailDc> BookExpenseDetailList { get; set; }
    }
}
