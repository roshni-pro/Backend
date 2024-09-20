using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.TripPlanner
{
    public class TripPlannerApprovalRequestDc
    {
        public string RequestType { get; set; }
        public long RequestId { get; set; }
        public DateTime CreatedDate { get; set; }
        public string RequestStatus { get; set; }
        public bool IsRequestPending { get; set; }
        public string RequestProcessedBy { get; set; }
        public DateTime? RequestProcessedDate { get; set; }
        public double? StartKm { get; set; }
        public double? ClosingKm { get; set; }
        public string StartKmUrl { get; set; }
        public string ClosingKMUrl { get; set; }
        public long? TripPlannerMasterId { get; set; }
        public string VehicleNumber { get; set; }
        public string DboyName { get; set; }

    }

    public class TripPlannerApprovalRequestInputDc
    {
        public int PeopleId { get; set; }
        public bool IsPendingOnly { get; set; }
        public int Skip { get; set; }
        public int Take { get; set; }
    }
}
