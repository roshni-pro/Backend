using System;

namespace AngularJSAuthentication.DataContracts.Transaction.Ledger.ItemLedger
{
    public class OutQueue
    {
        public long Id { get; set; }
        public int ItemMultiMrpId { get; set; }
        public int WarehouseId { get; set; }
        public int ObjectId { get; set; }
        public int Qty { get; set; }
        public double? SellingPrice { get; set; }
        public string Destination { get; set; }
        public DateTime CreatedDate { get; set; }
        public double? PurchasePrice { get; set; }
        public int? InWarehouseId { get; set; }
        public string InTransactionId { get; set; }
        public string InTransType { get; set; }
    }
}
