using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.External
{
    public class ManageBidDc
    {
        public long SellerId { get; set; }
        public List<ManageBidItems> StockItems { get; set; }
    }

    public class ManageBidItems
    {
        public int Qty { get; set; }
        public string ItemNumber { get; set; }        
        public long CentraltemId { get; set; }
        public decimal MRP { get; set; }
        public decimal PurchasePrice { get; set; }
    }

   
}
