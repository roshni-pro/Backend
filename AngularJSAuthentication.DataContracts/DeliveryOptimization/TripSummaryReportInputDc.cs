using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.DeliveryOptimization
{
    public class TripSummaryReportInputDc
    {
        public int WarehouseId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string DateRangeOption { get; set; }
        public string LabelName { get; set; }
    }

    public class LMDDashboardPart1InputDc
    {
        public List<int> WarehouseId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string DateRangeOption { get; set; }
        public string LabelName { get; set; }
        public List<string> TransporterNames { get; set; }
    }

    public class LMDDashboardPart3DC
    {
        public double UtilizedPercentageByAmount { get; set; }
        public double UtilizedPercentageByTouchPoints { get; set; }
    }

    public class LMDDashboardPart4DC
    {
        public int VehicleCountAvailable { get; set; }
        public int VehicleCountRequired { get; set; }
    }
   
    public class LMDDashboardExportOrderDC
    {
        public long TripPlannerMasterId { get; set; }
        public long OrderId { get; set; }
        public string ClusterName { get; set; }
        public string ExecutiveName { get; set; }
        public string DboyName { get; set; }
        public string OrderStatus { get; set; }
        public string comments { get; set; }
        public DateTime OrderedDate { get; set; }
        public DateTime ETADate { get; set; }
    }
   

    public class LMDDashboardExport
    {
        public string AttendanceDate { get; set; }
        public int? TripCount { get; set; }
        public string HubName { get; set; }
        public string TransportName { get; set; }
        public string FleetType { get; set; }
        public string VehicleType { get; set; }
        public string VehicleNo { get; set; }
        public double? MonthlyContrackKM { get; set; }
        public int? ThisTripTravelledKM { get; set; }
        public int? ExpectedKM { get; set; }
        public double? MonthlyContractAmount { get; set; }
        public double? TodayUtilizedAmount { get; set; }
        public int? OrderCount { get; set; }
        public int? ThresholdTouchPoints { get; set; }
        public int? ActualTouchPoints { get; set; }
        public double? ThresholdValueOfLoad { get; set; }
        public double? VisitedValueOfLoad { get; set; }
        public int? DeliverdCount { get; set; }
        public int? DeliverdValue { get; set; }
        public int? CancelCount { get; set; }
        public int? CancelValue { get; set; }
        public int? RedispatchValue { get; set; }
        public int? RedispatchCount { get; set; }
        public double? UtilizedPercentageByAmount { get; set; }
        public double? UtilizedPercentageByTouchPoints { get; set; }
        public double? DboyCost { get; set; }

        public int? ReattemptValue { get; set; }
        public int? ReattemptCount { get; set; }
        public double? ExecutedAmount { get; set; }
        public double? ExecutedOrderCount { get; set; }
    }

    public class LMDDashboardExport1
    {
        public string AttendanceDate { get; set; }
        public string HubName { get; set; }
        public double? DeliveryCostPercentage { get; set; }
        public int? PerOrderDeliveryCost { get; set; }
        public long? TotalVehicleUsed { get; set; }
        public int? VehicleCost { get; set; }
        public double? TripUtilizationAmountPercentage { get; set; }
        public double? TripUtilizationKMPercentage { get; set; }
        public int? AdHocVehiclecount { get; set; }
        //public int? WarehouseId { get; set; }
        //public DateTime? ETADate { get; set; }
        public int? PlannedVehicle { get; set; }
        public int? AdHocVehicleCost { get; set; }
        public double? TouchPointValue { get; set; }
      
    }




}
