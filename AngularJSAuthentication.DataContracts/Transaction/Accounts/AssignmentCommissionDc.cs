using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.Transaction.Accounts
{
    public class AssignmentCommissionDc
    {
        public long Id { get; set; }
        public int AssignmentID { get; set; }
        public int AgentID { get; set; }
        public double? CommissionAmount { get; set; }
        public double? TDSAmount { get; set; }
        public double? PaidAmount { get; set; }
        public DateTime FreezeDate { get; set; }
        public string Status { get; set; } // Settled , UnSettled , Partially Settled
    }
}
