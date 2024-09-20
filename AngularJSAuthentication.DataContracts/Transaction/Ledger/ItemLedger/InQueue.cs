using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.Transaction.Ledger.ItemLedger
{
    public class InQueue
    {
        public long Id { get; set; }
        public int ItemMultiMrpId { get; set; }
        public int WarehouseId { get; set; }
        public int ObjectId { get; set; }
        public string SupplierName { get; set; }
        public int Qty { get; set; }
        public double Price { get; set; }
        public DateTime CreatedDate { get; set; }
        public int RemQty { get; set; }
        public string Source { get; set; }
        public DateTime? InDate { get; set; }
        public string TransactionId { get; set; }
        public double CancelInPP { get; set; }
    }
}
