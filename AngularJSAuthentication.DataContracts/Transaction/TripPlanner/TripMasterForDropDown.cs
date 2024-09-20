using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.Transaction.TripPlanner
{
    public class TripMasterForDropDown
    {
        public long TripPlannerMasterId { get; set; }
        public int TripNumber { get; set; }
        public long? TripPlannerVehicleId { get; set; }
		public bool IsManual { get; set; }
        public string FreezedStatus { get; set; }
        public string DboyName { get; set; }
        public int TripTypeEnum { get; set; }
    }
}
