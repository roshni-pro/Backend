using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.DeliveryOptimization
{
    public class TripReportOverviewDc
    {
        public long TripPlannedConfirmMastedId { get; set; }
        public long TripNumber { get; set; }
        public DateTime TripDate { get; set; }
        public int TouchPoints { get; set; }
        public int TotalOrders { get; set; }
        public decimal PlannedDistance { get; set; }
        public decimal PlannedTripTimeInHrs { get; set; }
        public int DeliveredOrders { get; set; }
        public decimal ActualDistance { get; set; }
        public decimal ActualTripTimeInHrs { get; set; }
        public string DBoy { get; set; }
        public string VehicleNumber { get; set; }
    }


    public class TripReportOverviewExportDc
    {
        public long TripNumber { get; set; }
        public DateTime TripDate { get; set; }
        public int TouchPoints { get; set; }
        public int TotalOrders { get; set; }
        public decimal PlannedDistance { get; set; }
        public decimal PlannedTripTimeInHrs { get; set; }
        public int DeliveredOrders { get; set; }
        public decimal ActualDistance { get; set; }
        public decimal ActualTripTimeInHrs { get; set; }
        public string  DBoy { get; set; }
        public string VehicleNumber { get; set; }

    }


    public class TripReportOrderOverviewDc
    {
        public long TripNumber { get; set; }
        public DateTime TripDate { get; set; }
        public long OrderId { get; set; }
        public string Skcode { get; set; }
        public int PlannedStopNumber { get; set; }
        public int PlannedServiceTime { get; set; }
        public int ActualStopNumber { get; set; }
        public DateTime DeliveryTime { get; set; }
        public int ActualServiceTime { get; set; }
        public string OrderStatus { get; set; }
    }

   
}
