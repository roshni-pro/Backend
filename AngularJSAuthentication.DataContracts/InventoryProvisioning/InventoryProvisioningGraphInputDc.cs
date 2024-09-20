using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.InventoryProvisioning
{
    public class InventoryProvisioningGraphInputDc
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public List<int> WarehouseIdList { get; set; }
        public List<int> BrandIdList { get; set; }

    }

    public class InventoryProvisioningGraphDataDc
    {
        public string WarehouseName { get; set; }
        public DateTime CalculationDate { get; set; }
        public double ProvisioningAmount { get; set; }

    }


    public class InventoryProvisioningGraphAllData
    {
        public List<string> labels { get; set; }
        public List<InventoryProvisioningGraphAllDataSet> datasets { get; set; }
    }

    public class InventoryProvisioningGraphAllDataSet
    {
        public string label { get; set; }
        public List<double> data { get; set; }
    }

}
