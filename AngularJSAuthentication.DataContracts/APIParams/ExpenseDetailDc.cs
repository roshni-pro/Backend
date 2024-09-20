using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.APIParams
{
    public class ExpenseDetailDc
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public int ExpenseID { get; set; }
        public long? CreditLedgerID { get; set; }
        public string CreditLedgerName { get; set; }
        public int CreditLedgerTypeId { get; set; }
        public string CreditLedgerTypeName { get; set; }
        public bool IsFixedCreditLedgerId { get; set; }
        public Boolean IsMasterLedger { get; set; }
        public int CreatedBy { get; set; }
    }
}
