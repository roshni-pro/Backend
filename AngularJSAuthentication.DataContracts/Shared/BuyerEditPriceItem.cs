using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.Shared
{
   public class BuyerEditPriceItem
    {
        public int ItemId { get; set; }
        public int WarehouseId { get; set; }
        public int ItemMultiMRPId { get; set; }
        public string itemname { get; set; }
        public double price { get; set; }
        public double Discount { get; set; }
        public double UnitPrice { get; set; }
        public string Number { get; set; }
        public double PurchasePrice { get; set; }
        public double Margin { get; set; }
        public double WithTaxNetPurchasePrice { get; set; }
        public double TotalTaxPercentage { get; set; }
        public double POPurchasePrice { get; set; }
        public int SupplierId { get; set; }
        public int DepoId { get; set; }
        public int DealQty { get; set; }
        public double DealPrice { get; set; }
    }
}
