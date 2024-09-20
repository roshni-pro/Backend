using AngularJSAuthentication.Model.Stocks.Batch;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.BatchCode
{
    public class StockBatchMasterDc
    {
        public long StockBatchMasterId { get; set; }
        public int Quantity { get; set; }
        public DateTime? ExpiryDate { get; set; }
    }


    public class StockBatchGroup
    {
        public StockBatchTransaction Transaction { get; set; }
        public StockBatchMasterDc StockBatchMaster { get; set; }

    }
    public class TransferOrderItemBatchMasterDc
    {
        public int Qty { get; set; }
        public long StockBatchMasterId { get; set; }
        public int ItemMultiMRPId { get; set; }
        public int WarehouseId { get; set; }
        public long ObjectId { get; set; }
        public long ObjectIdDetailId { get; set; }
    }

    public class ManulorderdetailDcs
    {
        public int OrderId { get; set; }
        public int WarehouseId { get; set; }
        public int ItemMultiMRPId { get; set; }
        public string StockType { get; set; }
        public int OrderDetailsId { get; set; }
        public int Quantity { get; set; }
    }
}
