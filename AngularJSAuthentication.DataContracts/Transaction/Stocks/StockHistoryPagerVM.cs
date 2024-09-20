using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.Transaction.Stocks
{
    public class StockHistoryPageFilterDc
    {
        public int? Skip { get; set; }
        public int? Take { get; set; }
        public string StockType { get; set; } 
        public int? ItemMultiMRPId { get; set; }
        public int? WarehouseId { get; set; }
        public string RefStockType { get; set; }
        public int? UserId { get; set; }
    }

    public class StockHistoryPageContentDc
    {
        public string CreatedDateString { get; set; }
        public string EntityType { get; set; }
        public int? EntityId { get; set; }
        public int? InOutQty { get; set; }
        public string WarehouseName { get; set; }
        public string ItemName { get; set; }
        public string UOM { get; set; }
        public string ItemNumber { get; set; }
        public string Reason { get; set; }
        public string Email { get; set; }
        public int ItemMultiMRPId { get; set; }
        public string RefStockCode { get; set; }
    }

    public class StockHistoryListDc
    {
        public int TotalRecords { get; set; }
        public List<StockHistoryPageContentDc> PageList { get; set; }
    }

}
