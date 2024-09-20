using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.APIParams
{
    public  class ExpenseDc
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public int DebitLedgerId { get; set; }
        public string DebitLedgerName { get; set; }
        public int DebitLedgerTypeId { get; set; }
        public string DebitLedgerTypeName { get; set; }
        public int? CheckerId { get; set; }
        public string  CheckerName { get; set; }
        public int CreatedBy { get; set; }
        public Boolean IsTDSApplied { get; set; }
        public Boolean IsGSTApplied { get; set; }
        public List<ExpenseDetailDc> ExpenseDetailList { get; set; }
    }
}
