using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.Transaction.Expense
{
    public class BookExpensePaymentDc
    {
        public long? Id { get; set; }
        public long BookExpenseId { get; set; }
        public DateTime PaymentDate { get; set; }
        public double Amount { get; set; }
        public string Remark { get; set; }
        public string Narration { get; set; }
    }
}
