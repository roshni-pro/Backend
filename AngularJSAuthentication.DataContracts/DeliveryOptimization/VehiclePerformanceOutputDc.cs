using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.DeliveryOptimization
{
    public class VehiclePerformanceOutputDc
    {
        public string CityName { get; set; }
        public string Hub { get; set; }
        public string VehicleNo { get; set; }
        public long? TripNumber { get; set; }
        public DateTime? TripDate { get; set; }
        public int VehicleCapacity { get; set; }
        public decimal TotalLoad { get; set; }
        public decimal PlannedDistanceInKM { get; set; }
        public decimal ActualEnteredDistanceInKM { get; set; }
        public decimal ActualCalculatedDistanceInKM { get; set; }
        public decimal PlannedTripTimeInHrs { get; set; }
        public decimal ActualTripTimeInHrs { get; set; }

        public List<VehiclePerformanceOutputDc> InnerList { get; set; }
    }


    public class VehiclePerformanceExportDc
    {
        public string CityName { get; set; }
        public string Hub { get; set; }
        public string VehicleNo { get; set; }
        public long? TripNumber { get; set; }
        public DateTime? TripDate { get; set; }
        public int VehicleCapacity { get; set; }
        public decimal TotalLoad { get; set; }
        public decimal PlannedDistanceInKM { get; set; }
        public decimal ActualEnteredDistanceInKM { get; set; }
        public decimal ActualCalculatedDistanceInKM { get; set; }
        public decimal PlannedTripTimeInHrs { get; set; }
        public decimal ActualTripTimeInHrs { get; set; }

    }
}
