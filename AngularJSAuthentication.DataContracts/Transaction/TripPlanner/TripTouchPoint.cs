using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.Transaction.TripPlanner
{
    public class TripTouchPoint
    {
        public double Lat { get; set; }
        public double Lng { get; set; }
        public int OrderCount { get; set; }
        public double TotalAmount { get; set; }
        public bool IsProcess { get; set; }
        public long TotalDistanceInMeter { get; set; }
        public double TotalWeight { get; set; }
        public long TripPlannerConfirmDtailId { get; set; }
        public long CustomerId{ get; set; }
        public string Skcode { get; set; }
        public string OrderStatus { get; set; }
        public bool IsSkip { get; set; }
        public int? SequenceNo { get; set; }
        public int? RealSequenceNo { get; set; }
        public double? UnloadLat { get; set; }
        public double? UnloadLng { get; set; }
    }

    public class TripInformation 
    {
        public string TruckStatus { get; set; }
        public int TruckStatusId { get; set; }
        public string Assignedto { get; set; }
        public string VehicleType { get; set; }
        public DateTime? StartTime { get; set; }
        public int NoOfSkCodeLeft { get; set; }
        public double EstimatedTimetoComplete { get; set; }
        public double Kmtravelled { get; set; }
        public DateTime? EndTime { get; set; }
        public double ReminingTime { get; set; }
        public double DistanceLeft { get; set; }
        public double BreakTimeInSec { get; set; }
        public DateTime? ReportingTime { get; set; }
        public double? StartKm { get; set; }
        public double? ClosingKm { get; set; }
        public DateTime? BreakStartTime { get; set; }
        public long ActualDistanceTraveled { get; set; }
        public int MiloMeterReading { get; set; }
        public long TotalDistanceInMeter { get; set; }
    }
    public class TripTouchPointInformation
    {
        public TripInformation tripInformation { get; set; }
        public List<TripTouchPoint> TripTouchPointList { get; set; }
    }
}
