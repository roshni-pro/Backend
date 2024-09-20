using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.Transaction.TripPlanner
{
    /// <summary>
    /// SP output: TripPlanner_VehicleGet
    /// </summary>
    public class TripPlannerVehicle
    {
        public long Id { get; set; }
        public long VehicleCapacity { get; set; }
        public long TimeInMins { get; set; }
        public long DistanceContract { get; set; }
        public string RegistrationNo { get; set; }
        public string VehicleType { get; set; }
        //public long AgentId { get; set; }


    }

    public class VehicleDboyDriverDDs
    {
        
        public List<VehicleDropDownList> Vehicles { get; set; }
        public List<DboyDropDownList> Dboys { get; set; }
        public List<DriverDropDownList> Drivers { get; set; }
    }

    public class VehicleDropDownList
    {
        public long VehicleId { get; set; }
        public string VehicleName { get; set; }
        public bool? IsReplacementVehicleNo { get; set; }
        public string ReplacementVehicleNo { get; set; }
        //public int TripTypeEnum { get; set; }

    }

    public class DboyDropDownList
    {
        public long DboyId { get; set; }
        public string DboyName { get; set; }
    }
    public class DriverDropDownList
    {
        public long DriverId { get; set; }
        public string DriverName { get; set; }

    }
}
