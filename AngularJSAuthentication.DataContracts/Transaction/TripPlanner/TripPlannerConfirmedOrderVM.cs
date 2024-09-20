using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.Transaction.TripPlanner
{
    public class TripPlannerConfirmedOrderVM
    {
        public int OrderId { get; set; }
        public string CustomerName { get; set; }

        public string ClusterName { get; set; }

        public int ClusterId { get; set; }

        public string ShippingAddress { get; set; }
        public double Amount { get; set; }
        public long TimeInMins { get; set; }
        public long DistanceInMeter { get; set; }
        public long TripPlannerConfirmedOrderId { get; set; }
        public bool IsActive { get; set; }
        public long TripPlannerConfirmedDetailId { get; set; }
        public bool IsActiveOld { get; set; }
        public bool IsManuallyAdded { get; set; }
        public string Skcode { get; set; }
        public string Mobile { get; set; }
        public double Lat { get; set; }
        public double Lng { get; set; }
        public long CustomerId { get; set; }
        public double WeightInKg { get; set; }
        public string ShopName { get; set; }
        public string ReDispatchCount { get; set; }
        public bool IsRightLocation { get; set; }
        public bool IsAddableDueToCustomerLocation { get; set; }
    	public DateTime OrderDate { get; set; }
        public string Status { get; set; }
        public bool IsNewPickerOrder { get; set; }
        public long? OrderPickerMasterId { get; set; }
        public string CRMTags { get; set; }
        public string OrderType { get; set; }
        public DateTime ETADate { get; set; }
        public string CustomerType { get; set; }
        public DateTime? ExpectedRtdDate { get; set; }
        public DateTime? PrioritizedDate { get; set; }

    }
}
