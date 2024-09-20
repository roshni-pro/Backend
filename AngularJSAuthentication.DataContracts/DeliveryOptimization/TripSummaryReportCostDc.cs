using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.DeliveryOptimization
{
    public class TripSummaryReportCostDc
    {
        public decimal CostPerKm { get; set; }
        public decimal CostPerKg { get; set; }
        public decimal CostPerVehicle { get; set; }
        public decimal CostPerOrder { get; set; }
        public decimal CostPerTrip { get; set; }
        public double DboyCost { get; set; }
    }
    public class TransporterList
    {
        public string Name { get; set; }
    }

    public class LMDChartDc
    {
        public decimal CostPerKm { get; set; }
        public decimal DeliverAmountPerVehicleCost { get; set; }
        public decimal VehiclesCost { get; set; }
        public decimal CostPerDeliverOrder { get; set; }
        public decimal CostPerTrip { get; set; }
        public decimal DboyCost { get; set; }
    }
}
