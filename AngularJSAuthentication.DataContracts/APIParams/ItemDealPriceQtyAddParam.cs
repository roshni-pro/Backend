using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.APIParams
{
    public class ItemDealPriceQtyAddParam
    {
        public int WareHouseId { get; set; }
        public int SupplierId { get; set; }
        public string ItemNumber { get; set; }
        public int ItemMultiMrpID { get; set; }
        public int DealQty { get; set; }
        public double DealPrice { get; set; }
        public int CreatedById { get; set; }
        public int? UpdatedByID { get; set; }
    }
}
