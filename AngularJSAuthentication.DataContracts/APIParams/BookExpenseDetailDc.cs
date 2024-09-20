using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.APIParams
{
    public class BookExpenseDetailDc
    {
        public long  ExpenseDetailId { get; set; }
        public string ExpenseDetailName { get; set; }
        public long? Id { get; set; }
        public long BookExpenseId { get; set; }

        public long CreditLedgerId { get; set; }
        public string CreditLedgerName { get; set; }
        public long CreditLedgerTypeId { get; set; }
        public double? CreditLedgerAmount { get; set; }
    }
}
