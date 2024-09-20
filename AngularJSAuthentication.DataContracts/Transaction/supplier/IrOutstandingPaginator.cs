using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.Transaction.supplier
{
    public class IrOutstandingPaginator
    {
        public int? WarehouseId { get; set; }
        public string Search { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int SkipCount { get; set; }
        public int Take { get; set; }
        public string IRStatus { get; set; }
        public bool IsGetFutureOutstandingAlso { get; set; }
    }


    public class IrOutstandingViewPaginator
    {
        public int? WarehouseId { get; set; }
        public string Search { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int SkipCount { get; set; }
        public int Take { get; set; }
        public string Status { get; set; }
        public int  BuyerId { get; set; }
    }
}
