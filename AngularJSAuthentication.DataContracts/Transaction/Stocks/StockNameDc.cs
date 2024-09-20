using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.Transaction.Stocks
{
    public class StockNameDc
    {
        public int Qty { get; set; }
        public string StockType { get; set; }
        public bool IsDraggable { get; set; }
        public bool IsDroppable { get; set; }
        public bool CanBeNegetive { get; set; }
        public string DisplayName { get; set; }
    }

    public class StockListDc
    {
        public List<StockNameDc> AllSecondaryStockList { get; set; }
        public StockNameDc VirtualStock { get; set; }
    }

    public class ManualStockUpdateRequestDc
    {
        public long Id { get; set; }
        public int ItemMultiMRPId { get; set; }
        public int WarehouseId { get; set; }
        public string SourceStockType { get; set; }
        public string DestinationStockType { get; set; }
        public int Qty { get; set; }
        public string Status { get; set; }
        public string UserName { get; set; }
        public string ItemName { get; set; }
        public string Reason { get; set; }
        public string StockTransferType { get; set; }
        public DateTime CreatedDate { get; set; }

        public string WarehouseName { get; set; }
        public bool IsPairFound { get; set; }

        public long? sStockBatchMasterID { get; set; }
        public long? sBatchMasterID { get; set; }
        public long? dStockBatchMasterID { get; set; }
        public long? dBatchMasterID { get; set; }
    }
    public class StockTransactionListDC
    {
        //public string Keyword { get; set; }
        //public int Skip { get; set; }
        //public int Take { get; set; }
        //public int WarehouseId { get; set; }
        //public int? ItemMultiMRPId { get; set; }

        [MaxLength(50)]
        public string StockType { get; set; }
        public long StockId { get; set; }
        public int ItemMultiMRPId { get; set; }
        public int WarehouseId { get; set; }
        public int InOutQty { get; set; }
        public DateTime CreatedDate { get; set; }
        public bool IsActive { get; set; }
        public bool? IsDeleted { get; set; }
        public int CreatedBy { get; set; }
        public int ItemId { get; set; }
        public string itemname { get; set; }
    }

    public class ManualStockUpdatePagerDc
    {
        public List<ManualStockUpdateRequestDc> ManualStockRequestsList { get; set; }
        public int TotalRecords { get; set; }
        
    }


    public class PhysicalStockUpdateRequestDc
    {
        public int ItemMultiMRPId { get; set; }
        public int WarehouseId { get; set; }
        public string SourceStockType { get; set; }
        public string DestinationStockType { get; set; }
        public int Qty { get; set; }
        public string Reason { get; set; }
        public string StockTransferType { get; set; }
    }



    public class FetchDynamicStockDc
    {

        public int ItemMultiMRPId { get; set; }
        public double MRP { get; set; }
        public int WarehouseId { get; set; }
        public string WarehouseName { get; set; }
        public string itemname { get; set; }
        public string ItemNumber { get; set; }
        public int AvailableStock { get; set; }

    }
}
