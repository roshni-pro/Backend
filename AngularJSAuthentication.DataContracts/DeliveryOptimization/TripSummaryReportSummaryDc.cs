using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.DeliveryOptimization
{
    public class TripSummaryReportSummaryDc
    {
        public decimal PlannedKm { get; set; }
        public int TotalTrips { get; set; }
        public decimal DistancePerTrip { get; set; }
        public decimal ActualKm { get; set; }
        public int OrderPerDay { get; set; }
        public int TotalOrder { get; set; }
        public int DelayedTrips { get; set; }
        public int AvgServiceTimeInMin { get; set; }

        public int DeliveredCount { get; set; }
        public int DeliveryCancelCount { get; set; }
        public int DeliveryRedispatchCount { get; set; }
        public int ReAttemptCount { get; set; }
        public int OnTime { get; set; }
        public int Late { get; set; }
        public int VeryLate { get; set; }
        public double? DeliveredAmount { get; set; }
        public double? ExecutedAmount { get; set; }
    }
}
