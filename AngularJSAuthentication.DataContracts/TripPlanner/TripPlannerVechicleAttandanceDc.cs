using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.TripPlanner
{
    public class TripPlannerVechicleAttandanceFinalDC
    {
        public List<TripPlannerVechicleAttandanceDc> plannerVechicleAttandance { get; set; }
        public int TotalCount { get; set; }
    }
    public class TripPlannerVechicleAttandanceDc
    {
        public long Id { get; set; }
        public int WarehouseId { get; set; }
        public DateTime AttendanceDate { get; set; }
        public double KMTravelled { get; set; }
        public double Fare { get; set; }
        public double TollFare { get; set; }
        public double TotalWeightCarry { get; set; }
        public double TotalWeightDelivered { get; set; }
        public long VehicleMasterId { get; set; }    
        public DateTime CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
    }
    public class InsertVechicleAttandanceDc
    {
        public int WarehouseId { get; set; }
        public bool IsTodayAttendance { get; set; }     
        public long VehicleMasterId { get; set; }
        public DateTime AttandanceDate { get; set; }
        public bool IsDateSendbyUser { get; set; }

    }

    public class TripPlannerWarehouseList
    {
        public int WarehouseId { get; set; }
        public string WarehouseName { get; set; }
    }
    public class VechicleAttandanceExportList
    {
        public int WarehouseId { get; set; }
        public List<long> VehicleMasterId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
    public class VehicleMasterList
    {
        public int Id { get; set; }
        public string RegistrationNo { get; set; }
    }
    public class VehicleAttandanceListDc
    {
        public long VehicleMasterId { get; set; }
        public string VehicleNo { get; set; }
        public string TransportName { get; set; }
        public string Type { get; set; }//FleetType
        public string VehicleModel { get; set; }//Model
        public double MonthlyLimitAmount { get; set; }//FixedCost
        public int OrderAmountDelivered { get; set; }
        public double MonthlyLimitKM { get; set; }//DistanceContract
        public double ThresholdValueOfLoad { get; set; }//VehicleCapacity
        public double UtiKMTillYesterday { get; set; }//KMTravelled
        public int NoOfOrders { get; set; }
        public int TouchPointVisited { get; set; }
        public int NoOfOrderDelivered { get; set; }
        public int TouchPointPlanned { get; set; }
        public int OrderAmount { get; set; }//utiAmount
        public double PeriodLimitAmount { get; set; }//FixedCost/@NoOfDays
        public double PeriodLimitKm { get; set; }//DistanceContract/@NoOfDays
        public int Today { get; set; }
        public int Tomorrow { get; set; }
        public double UtilAmountPercentage { get; set; }
        public double UtilKmPercentage { get; set; }
        public int VisitedOrderAmount { get; set; }
        public string StatusOfUtilization { get; set; }
        public double DboyCost { get; set; }
        public string ReplacementVehicleNo { get; set; }
    }

    public class VehicleAttandanceVm
    {
        public int WarehouseId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsAutoAdded { get; set; }


    }
    //public class VechicleAttandanceFinalDC
    //{
    //    public List<VehicleAttandanceListDc> vehicleAttandanceListDcs { get; set; }
    //    public int TotalCount { get; set; }
    //}
    public class VehicleAttandanceTodayTomorrow
    {
        public int WarehouseId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int Today { get; set; }
        public int Tomorrow { get; set; }
        public int Skip { get; set; }
        public int Take { get; set; }
    }
    public class AttandanceTodayTomorrowDc
    {
        public int WarehouseId { get; set; } 
        public int Today { get; set; }
        public int Tomorrow { get; set; }

    }
    public class AttandanceTodayTomorrowSpList
    {
        public long VehicleMasterId { get; set; }
        public int Today { get; set; }
        public int Tomorrow { get; set; }

    }
    public class VehicleAttandanceExportLists
    {
        public string AttendanceDate { get; set; }
        public string HubName { get; set; }
        public string VehicleNo { get; set; }
        public string TransportName { get; set; }
        public string Type { get; set; }//FleetType
        public string VehicleType { get; set; }//
        public double MonthlyContractAmount { get; set; }//FixedCost/MonthlyLimitAmount      
        public double UtilizedAmount { get; set; }//Fare/VisitedUtiAmount 
        public double MonthlycontractKM { get; set; }//DistanceContract/MonthlyLimitKM 
        public double UtilizedKM { get; set; }//KMTravelled/VisitedUtiKm 
        public int ThresholdTouchPoint { get; set; }//TouchPoint/PlannedTouchPoint 
        public int ActualTouchPoint { get; set; }//TouchpointVisited 
        public int OrderCount { get; set; }//NoOfOrders  
        public double ThresholdValueOfLoad { get; set; }//VehicleCapacity 
        public int VisitedValueOfLoad { get; set; }//OrderAmountDelivered 
        public string StatusOfUtilization { get; set; } 
        //public double Cost { get; set; }
        //public string Status { get; set; }
        //public string VehicleModel { get; set; }//Model
        public double DeliveredValueOfLoad { get; set; }
        public double DboyCost { get; set; }
        public string ReplacementVehicleNo { get; set; }
    }
    public class VehicleNoList
    {
        public int Id { get; set; }
        public string VehicleNo { get; set; }
    }
    public class TotalHistoryAttandanceList
    {
        public int TotalCount { get; set; }
        public List<HistoryAttandanceListDc> historyAttandanceListDcs { get; set; }
    }
    public class HistoryAttandanceListDc
    {
        public DateTime DateOfUtilization { get; set; }
        public string VehicleNo { get; set; }
        public string TransportName { get; set; }
        public string WarehouseName { get; set; }
        public string Type { get; set; }//FleetType
        public double VehicleCapacity { get; set; }
        public double Cost { get; set; }//Fare
        public double LoadedWeight { get; set; }//TotalWeightCarry
        public string StatusUtilization { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public string CreatedBy { get; set; }
        public string ModifiedBy { get; set; }

    }
    public class ActiveStatuswiseAttandanceDc
    {
        public long Id { get; set; }
        public DateTime AttendanceDate { get; set; }
        public bool IsActive { get; set; }
        public long VehicleMasterId { get; set; }
        public int WarehouseId { get; set; }

    }
    public class FutureAttandanceDataDc
    {
        public long VehicleMasterId { get; set; }
        public int WarehouseId { get; set; }
        public List<AttandanceListDc> attandanceListDcs  { get; set; }

    }
    public class AttandanceListDc
    {
        public int Id { get; set; }
        public DateTime AttendanceDate { get; set; }
        public bool IsActive { get; set; }
    }
    public class AttendanceTripDetailDc
    {
        public bool Status { get; set; }
        public string Message { get; set; }
        public List<long> TripConfirmedMasterIds { get; set; }
    }
    public class VechicleExportDC
    {
        public List<int> WarehouseId { get; set; }
        public List<long> VehicleMasterId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
    public class AllVehicleReportDC
    {
        public string VehicleNo { get; set; }
        public string VehicleType { get; set; }
        public DateTime CreatedDate { get; set; }
        public string HubName { get; set; }
        public string TransportName { get; set; }
        public int OrderCount { get; set; }
        public int OrderValue { get; set; }
        public int ShippedCount { get; set; }
        public int ShippedValue { get; set; }
        public int DeliverdValue { get; set; }
        public int DeliverdCount { get; set; }
        public int RedispatchValue { get; set; }
        public int RedispatchCount { get; set; }
        public int DCCount { get; set; }
        public int DCValue { get; set; }
        public int ReattemptCount { get; set; }
        public int ReattemptValue { get; set; }

    }
    public class VehicleTypeDc
    {
        public int Id { get; set; }
        public int ThresholdLoadInKg { get; set; }
    }
    public class VehicleTripExportLists
    {
        public string VehicleNo { get; set; }
        public string VehicleType { get; set; }
        public DateTime ClosingTime { get; set; }
        public DateTime ReportingTime { get; set; }
        public double StartKm { get; set; } 
        public double ClosingKm { get; set; }
        public double TotalKm { get; set; }
        public DateTime CreatedDate { get; set; }    
        public string DriverName { get; set; }
        public long TripNo { get; set; }
        public string HubName { get; set; }
        public int OrderCount { get; set; }
        public int OrderValue { get; set; }
        public int DeliverdValue { get; set; } 
        public int DeliverdCount { get; set; }
        public double DeliverdPercent { get; set; }
        public int RedispatchValue { get; set; }
        public int RedispatchCount { get; set; }
        public double RedispatchPercent { get; set; }
        public int DCCount { get; set; }
        public int DCValue { get; set; }
        public double DCPercent { get; set; }
        public string Dboy { get; set; }
        public double DboyCost { get; set; }
        public long ExpectedKm { get; set; }
        public int? ReattemptValue { get; set; }
        public int? ReattemptCount { get; set; }
        public string ReplacementVehicleNo { get; set; }
        public string TransportName { get; set; }
    }

    public class AdhocFleetDc
    {
        public long FleetMasterId { get; set; }
        public string TransportName { get; set; }
        public string FleetType { get; set; }
        public long FleetMasterDetailsId { get; set; }
        public string VehicleType { get; set; }
        public double VehicleWeight { get; set; }
    }
    public class AddNewAdhocVehicleDc
    {
        public string FleetType { get; set; }
        public string VehicleType { get; set; }
        public int FleetDetailId { get; set; }
        public string VehicleNo { get; set; }
        public double VehicleWeight { get; set; }
        public int WarehouseId { get; set; }
        public int CityId { get; set; }
    }

    public class AllVehicleNoListReportDC
    {
        public string VehicleNo { get; set; }
        public string VehicleType { get; set; }
        public DateTime CreatedDate { get; set; }
        public string HubName { get; set; }
        public string TransportName { get; set; }
        public int OrderCount { get; set; }
        public int OrderValue { get; set; }
        public int ShippedCount { get; set; }
        public int ShippedValue { get; set; }
        public int DeliverdValue { get; set; }
        public int DeliverdCount { get; set; }
        public int RedispatchValue { get; set; }
        public int RedispatchCount { get; set; }
        public int DCCount { get; set; }
        public int DCValue { get; set; }
        public int ReattemptCount { get; set; }
        public int ReattemptValue { get; set; }
        
    }
}
