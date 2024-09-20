using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.Masters
{
    public class OldAssignmentPayDC
    {
       
        public int DeliveryIssuanceId { get; set; }
        public int AgentId { get; set; }
        public double? TotalAssignmentAmount { get; set; }
        public double? TotalAssignmentDeliverdAmount { get; set; }
        public string RefNo { get; set; }
        public DateTime PaymentDate { get; set; }
        public string Remark { get; set; }
        public string Status { get; set; }
        public double? ChequeAmount { get; set; }
        public double? CashAmount { get; set; }
        public double? OnlineAmount { get; set; }
        public List<AssignmentOrderListPayDC> AssignmentorderDetails { get; set; }

    }
}
