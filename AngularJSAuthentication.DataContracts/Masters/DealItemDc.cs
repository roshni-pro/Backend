using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.Masters
{
    public class DealItemDc
    {
        public long Id { get; set; }        
        public bool IsActive { get; set; } 
        public string ItemName { get; set; }
        public int ItemMultiMRPId { get; set; }
        public int MinOrderQty { get; set; }
        public string WarehouseName { get; set; }
        public int WarehouseId { get; set; }
        public double DealPrice { get; set; }
        public int TotalLimit { get; set; } //multipal of  MinOrderQty
        public int OrderLimit { get; set; } //multipal of  MinOrderQty not more than TotalLimit
        public int TotalConsume { get; set; }
        public int CustomerLimit { get; set; }
        public DateTime? StartDate { get; set; }
    }

    public class ResponseDealItem
    {
        public int TotalItem { get; set; }
        public List<DealItemDc> DealItemDcs { get; set; }
    }

    public class ResponseInsertDealItem
    {
        public bool result { get; set; }
        public string msg { get; set; }
    }

}
