using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.Shared
{
    public class ItemDealPriceQtyDC
    {
        public int ID { get; set; }
        public int WareHouseId { get; set; }
        public int SupplierId { get; set; }
        public string ItemNumber { get; set; }
        public int ItemMultiMrpID { get; set; }
        public int DealQty { get; set; }
        public double DealPrice { get; set; }
    }
}
