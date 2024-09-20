using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataLayer.Repositories.Transactions.TripPlanner
{
    public class TripReportInputDc
    {
        public int WarehouseId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public long? TripPlannerConfirmMasterId { get; set; }
    }
}
