using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.InventoryProvisioning
{
    public class ItemFrontMarginClosingDc
    {
        public int WarehouseId { get; set; }
        public int ItemMultiMRPId { get; set; }
        public double FrontMargin { get; set; }
        public DateTime CalculationDate { get; set; }

        public DateTime CreatedDate { get; set; }
        public bool IsActive { get; set; }
        public bool? IsDeleted { get; set; }
        public int CreatedBy { get; set; }
    }
}
