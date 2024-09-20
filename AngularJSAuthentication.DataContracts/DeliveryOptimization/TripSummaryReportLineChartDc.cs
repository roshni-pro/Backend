using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.DeliveryOptimization
{
    public class TripSummaryReportLineChartDc
    {
        public List<string> labels { get; set; }
        public List<TripSummaryReportLineCharDataSetDc> datasets { get; set; }
    }

    public class TripSummaryReportLineCharDataSetDc
    {
        public string label { get; set; }
        public List<decimal> data { get; set; }
    }

    public class TripSummaryReportLineCharRawDataDc
    {
        public int YearValue { get; set; }
        public string MonthValue { get; set; }
        public int ChartValue { get; set; }
        public decimal CostPerKm { get; set; }
        public decimal CostPerKg { get; set; }
        public decimal CostPerVehicle { get; set; }
        public decimal CostPerOrder { get; set; }
        public decimal CostPerTrip { get; set; }
    }
}
