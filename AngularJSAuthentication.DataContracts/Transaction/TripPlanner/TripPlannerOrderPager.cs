using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.Transaction.TripPlanner
{
    public class TripPlannerOrderPager
    {
        public string Keyword { get; set; }
        public int Skip { get; set; }
        public int Take { get; set; }
        public long WarehouseId { get; set; }
        public long TripPlannerConfirmedMasterId { get; set; }
    }

    public class TripPlannerOrderPageResult
    {
        public int RowCount{ get; set; }
        public List<TripPlannerConfirmedOrderVM> OrderList { get; set; }
    }
}
