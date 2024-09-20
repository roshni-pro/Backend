using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.Transaction.TripPlanner
{
    public class TripOrderDetail
    {
        public long TripPlannerConfirmedOrderId { get; set; }
        public int OrderId { get; set; }
        public double OrderAmount { get; set; }
        public double WeightInKg { get; set; }
        public DateTime OrderDate { get; set; }
        public long UnloadTimeInMins { get; set; }
        public string OrderStatus { get; set; }
        public bool IsManuallyAdded { get; set; }
    }
}
