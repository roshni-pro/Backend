using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.Transaction.CustomerTarget
{
    public class CustomerTargetDispatchDataViewModel
    {
        public int? WarehouseId { get; set; }
        public string WarehouseName { get; set; }
        public int? Cityid { get; set; }
        public string Skcode { get; set; }

        public bool?  IsActive { get; set; }

        public double? totalVolume { get; set; }
        public double? Volume { get; set; }

    }
}
