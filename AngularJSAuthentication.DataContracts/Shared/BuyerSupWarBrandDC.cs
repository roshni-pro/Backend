using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.Shared
{
   public class BuyerSupWarBrandDC
    {
        public List<BuyerSupplierDC> buyerSupplierDCs { get; set; }
        public List<BuyerWarehouseDC> buyerWarehouseDCs { get; set; }
        public List<NonBuyerWarehouseDC> nonBuyerWarehouseDCs { get; set; }
    }
    public class BuyerSupplierDC
    {
        public int SupplierId { get; set; }
        public string SUPPLIERCODES { get; set; }
        public string Name { get; set; }
    }
    public class BuyerWarehouseDC
    {
        public int WarehouseId { get; set; }
        public string WarehouseName { get; set; }
        
    }
    public class NonBuyerWarehouseDC
    {
        public int WarehouseId { get; set; }
        public string WarehouseName { get; set; }
    }
}
