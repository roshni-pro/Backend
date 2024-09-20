using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.Transaction.TripPlanner
{
    public class TripPlannerConfirmedMasterVM
    {
        public long Id { get; set; }
        public long? TripPlannerMasterId { get; set; }
        public DateTime TripDate { get; set; }
        public int TripNumber { get; set; }
        public string VehicleNumber { get; set; }
        public long WarehouseId { get; set; }
        public long? DboyId { get; set; }
        public double TotalWeight { get; set; }
        public long TotalDistanceInMeter { get; set; }
        public long TotalTimeInMins { get; set; }
        public double TotalAmount { get; set; }
        public int OrderCount { get; set; }
        public int CustomerCount { get; set; }
        public double WarehouseLat { get; set; }
        public double WarehouseLng { get; set; }
        public long VehicleCapacityInKg { get; set; }
        public string VehicleType { get; set; }
        public long? VehicleId { get; set; }
        public long? AgentId { get; set; }
        public long? DriverId { get; set; }
        public bool IsFreezed { get; set; }
        public DateTime? ReportingTime { get; set; }
        public double? StartKm { get; set; }
        public long TripPlannerConfirmMasterId { get; set; }
        public int? LateReportingTimeInMins { get; set; }
        public double? PenaltyCharge { get; set; }
        public bool IsVisibleToDboy { get; set; }
        public bool IsPickerGenerated { get; set; }
        public bool IsPickerRequired { get; set; }
        public long? OrderPickerMasterId { get; set; }
        public List<int> DeliveryIssuanceIdList { get; set; }
        public bool IsNotLastMileTrip { get; set; }
        public int TripTypeEnum { get; set; }

        public List<long> PickerIdList { get; set; }
        public string FleetType { get; set; }
        public bool? IsReplacementVehicleNo { get; set; }
        public string ReplacementVehicleNo { get; set; }
        public double VehicleFare { get; set; }

    }
}
