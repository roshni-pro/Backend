using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.Transaction.TripPlanner
{
    public class ShipmentManifestDc
    {
        public ShipmentManifest shipmentManifest { get; set; }
        public List<AssignmentlistDc> Assignmentlist { get; set; }
        public List<EWBDetailsDc> EWBDetails { get; set; }

    }
    public class ShipmentManifest
    {
        public int TripNo { get; set; }
        public string TripType { get; set; }
        public int NoOfAssignment { get; set; }
        public string vehicleNo { get; set; }
        public DateTime vehicleInTime { get; set; }
        public int NoOfTouchPoint { get; set; }
        public string DriverName { get; set; }
        public string DriverCellNo { get; set; }
        public string DrliveryBoyName { get; set; }
        public string DrliveryBoyNo { get; set; }
        public string Date { get; set; }
        public string WarehouseName { get; set; }
        public double TotalLoadInKg { get; set; }
        public string VehicleType { get; set; }
        public double VehicleKMReading { get; set; }
        public string AgentName { get; set; }
        public double TotalEstimatedRoundtripKM { get; set; }
        public DateTime VehicleOutTime { get; set; }
    }

    public class AssignmentlistDc
    {
        public int AssignmentId { get; set; }
        public double AssignmentValue { get; set; }
        public double AssignmentWeight { get; set; }
        public bool EWBApplicable { get; set; }
    }
    public class EWBDetailsDc
    {
        public int OrderId { get; set; }
        public DateTime OrderDate { get; set; }
        public double OrderValue { get; set; }
        public string EWBNo { get; set; }
        public DateTime EWBDate { get; set; }
    }
}
