using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.Transaction.Accounts
{
    public class AgentCommissionPaymentDc
    {
        public long Id { get; set; }
        public long AgentId { get; set; }
        public long AgentLedgerId { get; set; }
        public long BankLedgerId { get; set; }
        public double Amount { get; set; }
        public DateTime PaymentDate { get; set; }
        public string RefNumber { get; set; }
        public string Narration { get; set; }
        public string Status { get; set; }   //Cancelled, Paid
        public string SettledStatus { get; set; }

        public int SettledAmount { get; set; }
        
    }
}
