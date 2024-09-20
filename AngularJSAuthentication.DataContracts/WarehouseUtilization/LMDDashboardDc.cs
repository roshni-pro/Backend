using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.WarehouseUtilization
{
    public class LMDDashboardDc
    {
        public string VehicleNo { get; set; }
        public string VehicleType { get; set; }
        public DateTime CreatedDate { get; set; } //AttandanceDate
        public string HubName { get; set; }
        public string TransportName { get; set; }
        public int NumberOfTrips { get; set; }
        public string TripNos { get; set; }
        public int ThresholdOrderCount { get; set; }
        public int OrderCount { get; set; }
        public decimal UtilizationPercentageOnOrderCount { get; set; }
        public double ThresholdOrderAmount { get; set; }
        public double OrderValue { get; set; }
        public decimal UtilizationPercentageOnOrderValue { get; set; }
        public int InProcessOrderCount { get; set; }
        public double InProcessOrderAmount { get; set; }
        public int DeliverdCount { get; set; }
        public decimal ThresholdVSDeliveryPercentageOnOrderCount { get; set; }
        public decimal CarriedVSDeliveryPercentageOnOrderCount { get; set; }
        public double DeliverdValue { get; set; }
        public int RedispatchCount { get; set; }
        public double RedispatchValue { get; set; }
        public int DCCount { get; set; }
        public double DCValue { get; set; }
        public int ReattemptCount { get; set; }
        public double ReattemptValue { get; set; }
        public string WorkingDuration { get; set; }
        public string Assignments { get; set; }
        public double TotalKmExcludingRunningTrip { get; set; }
        public decimal ThresholdVSDeliveryPercentOnOrderValue { get; set; }
        public decimal CarriedVSDeliveryPercentageOnOrderValue { get; set; }
    }
    public class LMDInputDc
    {
        public List<int> warehouseId { get; set; }
        public DateTime startdate { get; set; }
        public DateTime enddate { get; set; }
    }
    public class LMDDashboardChartDc
    {
        public string HubName { get; set; }
        public int NumberOfTrips { get; set; }
        public int ThresholdOrderCount { get; set; }
        public int OrderCount { get; set; }
        public decimal UtilizationPercentageOnOrderCount { get; set; }
        public double ThresholdOrderAmount { get; set; }
        public double OrderValue { get; set; }
        public decimal UtilizationPercentageOnOrderValue { get; set; }
        public decimal OverallUtilizationPercentage { get; set; }
        public int InProcessOrderCount { get; set; }
        public double InProcessOrderAmount { get; set; }
        public int DeliverdCount { get; set; }
        public decimal ThresholdVSDeliveryPercentageOnOrderCount { get; set; }
        public decimal CarriedVSDeliveryPercentageOnOrderCount { get; set; }
        public double DeliverdValue { get; set; }
        public decimal ThresholdVSDeliveryPercentOnOrderValue { get; set; }
        public decimal CarriedVSDeliveryPercentageOnOrderValue { get; set; }
        public int RedispatchCount { get; set; }
        public double RedispatchValue { get; set; }
        public int DCCount { get; set; }
        public double DCValue { get; set; }
        public int ReattemptCount { get; set; }
        public double ReattemptValue { get; set; }
        public double TotalKmExcludingRunningTrip { get; set; }
    }

    public class FTLDataDc
    {
        public string HubName { get; set; }
        public int TotalVehicle { get; set; }
        public int FTLVehiclesByCount { get; set; }
        public int FTLVehiclesByAmount { get; set; }
        public int FTLVehiclesOverAll { get; set; }       
        public decimal FTLVehiclesByCountPerCentage { get; set; }
        public decimal FTLVehiclesByAmountPerCentage { get; set; }
        public decimal FTLVehiclesOverAllPerCentage { get; set; }
    }

    public class FTLFullDataDc
    {
        public string HubName { get; set; }
        public string VehicleNumber { get; set; }
        public DateTime Date { get; set; }
        public int ThresholdOrderCount { get; set; }
        public double ThresholdOrderAmount { get; set; }
        public int OrderCount { get; set; }
        public double OrderValue { get; set; }
        public int DeliverdCount { get; set; }
        public double DeliverdValue { get; set; }
        public int RedispatchCount { get; set; }
        public double RedispatchValue { get; set; }
        public int DCCount { get; set; }
        public double DCValue { get; set; }
        public int ReattemptCount { get; set; }
        public double ReattemptValue { get; set; }

        public int FTLCount { get; set; }
        public double FTLValue { get; set; }
        public bool IsFTL { get; set; }
        public decimal FTLCountPerCentage { get; set; }
        public decimal FTLAmountPerCentage { get; set; }
    }



    public class DesciplineDataDc
    {
        public string HubName { get; set; }
        public long VehichleMasterId { get; set; }
        public string VehicleNo { get; set; }
        public string AttendanceDate { get; set; }
        public bool IsDisciplined { get; set; }
        public decimal DisciplinePercentage { get; set; }
        public int ThresholdOrders { get; set; }
        public int T10 { get; set; }
        public int T11 { get; set; }
        public int T12 { get; set; }
        public int T13 { get; set; }
        public int T14 { get; set; }
        public int T15 { get; set; }
        public int T16 { get; set; }
        public int T17 { get; set; }
        public int T18 { get; set; }
        public int T19 { get; set; }

    }

    public class DesciplineChartDataDc
    {
        public string HubName { get; set; }
        public int TotalVehicleDays { get; set; }
        public int DesciplineVehicleDays { get; set; }
    }
}
