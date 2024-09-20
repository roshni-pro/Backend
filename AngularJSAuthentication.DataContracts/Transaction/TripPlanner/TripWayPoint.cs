using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.Transaction.TripPlanner
{
    public class TripWayPoint
    {
        public long TripPlannerConfirmedDetailId { get; set; }
        public double Lat { get; set; }
        public double Lng { get; set; }
        public long CustomerId { get; set; }
        public long AllOrderUnloadTimeInMins { get; set; }

        public long? ArialDistance { get; set; }


    }
}
