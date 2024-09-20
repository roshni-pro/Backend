using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.TripPlanner
{
    public class VehicleHistoryVM
    {
        public long TripPlannerVehicleId { get; set; }
        public double Lat { get; set; }
        public double Lng { get; set; }
        public long CurrentServingOrderId { get; set; }
        public int RecordStatus { get; set; }
        public DateTime? RecordTime { get; set; }
        public long TripPlannerConfirmedDetailId { get; set; }
    }
}
