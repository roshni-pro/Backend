using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.BatchCode
{
    public class CurrentStockBatchMasterDC
    {
        public long? StockBatchMasterId { get; set; }
        public long? BatchMasterId { get; set; }

        public int Qty { get; set; }
        public string BatchCode { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public DateTime? MFGDate { get; set; }

    }

    public class BatchcodeWiseCurrentStockDC
    {
        public string ItemNumber { get; set; }
        public string WarehouseName { get; set; }
        public string BatchCode { get; set; }
        public int ItemMultiMRPId { get; set; }
        public string Category { get; set; }
        public string Subcategory { get; set; }
        public string ItemName { get; set; }
        public int BatchInventory { get; set; }
        public DateTime? MFGDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public double? RemainingShelfLife { get; set; }
        public double APP { get; set; }

    }
    public class StockBatchMasterDC
    {
        public long StockBatchMasterId { get; set; }
        public string BatchCode { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public DateTime? MFGDate { get; set; }
        public int Qty { get; set; }
    }

    public class BatchMasterDc
    {
        public long Id { get; set; }
        public DateTime MFGDate { get; set; }
        public DateTime ExpiryDate { get; set; }
        public string BatchCode { get; set; }
    }
    public class ODDetailsDc
    {
        public int ItemMultiMRPId { get; set; }
        public string itemNumber { get; set; }
        public int WarehouseId { get; set; }
        public int qty { get; set; }
        public string StockType { get; set; }  //C,F

    }
    public class APPDC
    {
        public double APP { get; set; }
    }
}
