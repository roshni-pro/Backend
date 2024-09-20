using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.InventoryProvisioning
{
    public class InventoryProvisioningInputDc
    {
        public DateTime? CalculationDate { get; set; }
        public int WarehouseId { get; set; }
        public List<int> BrandIdList { get; set; }
        public string Keyword { get; set; }
    }
}
