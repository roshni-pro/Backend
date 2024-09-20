using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.Transaction.Mongo.BuyerDashboard
{
    public class ItemsClosing
    {
        public string multimrpwarehouse { get; set; }
        public int ItemMultiMrpId { get; set; }
        public int WarehouseId { get; set; }
        public int ClosingQty { get; set; }
        public double ClosingAmount { get; set; }
    }
}
