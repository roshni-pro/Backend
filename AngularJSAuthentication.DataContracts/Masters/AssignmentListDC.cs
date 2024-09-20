using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.Masters
{
   public class AssignmentListDC
    {
        public int DeliveryIssuanceId { get; set; }
        public string DisplayName { get; set; }
        public string Status { get; set; }
        public DateTime CreatedDate { get; set; }
        public double? TotalAssignmentAmount { get; set; }
        public double? ChequeAmount { get; set; }
        public double? CashAmount { get; set; }
        public double? OnlineAmount { get; set; }
    }
}
