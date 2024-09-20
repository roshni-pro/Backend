using AngularJSAuthentication.DataContracts.APIParams;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.Transaction.Expense
{
    public class BookExpenseViewDc
    {
        public string ExpenseName { get; set; }
        public long? Id { get; set; }
        public double? TotalAmount { get; set; }
        public string DebitLedgerName { get; set; }
        public double? DebitLedgerAmount { get; set; }
        public string DepartmentName { get; set; }
        public string WorkingLocationName { get; set; }
        public string WorkingCompanyName { get; set; }
        public string CheckerName { get; set; }
        public Boolean IsChecked { get; set; }
        public Boolean IsLedgerGenerated { get; set; }
        public DateTime ExpenseDate { get; set; }
        public Boolean IsTDSApplied { get; set; }
        public Boolean IsGSTApplied { get; set; }
        public string TDSLedgerName { get; set; }
        public string GSTLedgerName { get; set; }
        public double? TDSAmount { get; set; }
        public double? GSTAmount { get; set; }
        public string Status { get; set; }

        public List<BookExpenseDetailDc> BookExpenseDetailList { get; set; }


    }
}
