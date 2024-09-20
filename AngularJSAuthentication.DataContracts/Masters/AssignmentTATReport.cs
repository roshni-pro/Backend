using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.Masters
{
    public class AssignmentTATReport
    {
        public int? DeliveryIssuanceId { get; set; }
        public int? PeopleID { get; set; }
        public DateTime? SavedAsDraft { get; set; }
        public DateTime? Assigned { get; set; }
        public DateTime? Accepted { get; set; }
        public DateTime? Submitted { get; set; }
        public DateTime? PaymentAccepted { get; set; }
        public DateTime? PaymentSubmitted { get; set; }
        public DateTime? Freezed { get; set; }
        public decimal? SaveAsDraftToAssigned { get; set; }
        public decimal? AssignedToAccepted { get; set; } 
        public decimal? AcceptedToSubmitted { get; set; }
        public decimal? SubmittedToPaymentAccepted { get; set; }
        public decimal? PaymentAcceptedToPaymentSubmitted { get; set; }
        public decimal? PaymentSubmittedToFreezed { get; set; }
        public string DeliveryBoyName { get; set; }
        public string PaymentMode { get; set; }
    }
}
