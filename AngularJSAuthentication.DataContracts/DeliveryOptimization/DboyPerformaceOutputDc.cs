using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.DeliveryOptimization
{
    public class DboyPerformaceOutputDc
    {
        public long TripPlannerMasterId { get; set; }
        public int PeopleID { get; set; }
        public String DboyName { get; set; }
        public string WarehouseName { get; set; }
        public int DeliveredOrderCount { get; set; }
        public int TotalOrderCount { get; set; }
        public int VehicleCapacityInKg { get; set; }
        public decimal DeliveredOrderAmount { get; set; }
        public decimal TotalOrderAmount { get; set; }
        public decimal DeliveredOrderWeightInKg { get; set; }
        public decimal TotalOrderWeightInKg { get; set; }
        public decimal ActualDistanceTraveledByGoogleInKm { get; set; }
        public decimal ActualDistanceTraveledByMiloMeterInKm { get; set; }
        public decimal ExpectedDistanceInKm { get; set; }
        public decimal TripServiceTimeInHrs { get; set; }
        public decimal ExpectedServiceTimeInHrs { get; set; }
        public int TotalTouchPoints { get; set; }
        public int ServedTouchPoints { get; set; }
        public int AvgServiceTimeInMins { get; set; }
        public DateTime? Date { get; set; }
        public decimal ActualTripTimeInHrs { get; set; }
        public List<DboyPerformaceOutputDc> TripList { get; set; }
    }

    public class DboyPerformaceOutputExportDc
    {
        
        public String Dboy { get; set; }
        public string Hub { get; set; }
        public long TripNumber { get; set; }
        public DateTime? Date { get; set; }
        public int TotalOrder { get; set; }
        public int DeliveredOrder { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal DeliveredAmount { get; set; }
        public int VehicleCapacityInKg { get; set; }
        public decimal TotalLoadInKg { get; set; }
        public decimal DeliveredLoadInKg { get; set; }
        public int PlannedTouchPoints { get; set; }
        public int ActualTouchPoints { get; set; }
        public decimal PlannedDistanceInKm { get; set; }
        public decimal ActualComputedDistanceInKm { get; set; }
        public decimal ActualEnteredDistanceInKm { get; set; }
        public decimal PlannedTripTimeInHrs { get; set; }
        public decimal ActualTripTimeInHrs { get; set; }
        public int AvgServiceTimeInMins { get; set; }
    }
}
