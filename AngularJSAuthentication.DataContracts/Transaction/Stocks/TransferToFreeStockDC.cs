using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.Transaction.Stocks
{
    public class TransferToFreeStockDC
    {
        public long StockBatchMasterId { get; set; }
        public string BatchCode { get; set; }
        public int Qty { get; set; }
        public string ManualReason { get; set; }
        public int WarehouseId { get; set; }
        public int ItemMultiMRPId { get; set; }
    }
}
