using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.Transaction.Accounts
{
   public class AgentPaymentSettlementDc
    {
        public double Amount { get; set; }
        public DateTime SettleDate { get; set; }
        public string Status { get; set; }
        public int AgentCommissionPaymentId { get; set; }
        public int AssignmentCommissionId { get; set; }
        
    }

    public class AgentPaymentSettlementDisplayDc
    {
        public long Id { get; set; }
        public double Amount { get; set; }
        public DateTime SettleDate { get; set; }
        public int AgentCommissionPaymentId { get; set; }
        public int AssignmentCommissionId { get; set; }
        public string Status { get; set; }

        public string AgentName { get; set; }

        public long AssignmentId { get; set; }

    }

    public class AgentPaymentSettlementPager
    {
        public List<AgentPaymentSettlementDisplayDc> AgentSettlementList { get; set; }
        public int NetRecords { get; set; }

    }


}
