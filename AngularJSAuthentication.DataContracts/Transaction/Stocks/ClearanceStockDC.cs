using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.Transaction.Stocks
{
    public class ClearanceStockDC
    {
        public int StockId { get; set; }
        public string ItemName { get; set; }
        public string ItemNumber { get; set; }
        public int ItemMultiMRPId { get; set; }
        public double MRP { get; set; }
        public int CurrentInventory { get; set; }
        public string Cateogry { get; set; }

        public string ABCClassification { get; set; }

        public int WarehouseId { get; set; }

    }
}
