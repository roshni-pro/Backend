using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.Transaction.PurchaseRequest
{
    public class PurchaseRequestSettlementFilterDc
    {
        public int SupplierId { get; set; }
        public int WarehouseId { get; set; }
        public int Skip { get; set; }
        public int Take { get; set; }
    }
}
