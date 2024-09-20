using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.Transaction
{
    public class OrderForORToolVM
    {
        public long CustomerId { get; set; }
        public string OrderIdList { get; set; }
        public double OrderTotalAmount { get; set; }
        public double TotalWeight { get; set; }
        public double WLat { get; set; }
        public double WLng { get; set; }
        public double CLat { get; set; }
        public double CLng { get; set; }
        public int WarehouseId { get; set; }
        public long VehicleId { get; set; }
        public string VehicleNo { get; set; }
        public string VehicleType { get; set; }
        public string VehicleCapacity { get; set; }
        public long OrderUnloadTimeInMins { get; set; }
        public bool? IsDummyNode { get; set; }

        public long? ArialDistanceFromWarehouse { get; set; }
    }
}
