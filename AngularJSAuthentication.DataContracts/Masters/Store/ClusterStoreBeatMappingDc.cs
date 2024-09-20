using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.Masters.Store
{
    public class ClusterStoreBeatMappingDc
    {
        public long Id { get; set; }
    }
    public class WarehouseClusterDc
    {
        public int WarehouseId { get; set; }
        public int ClusterId { get; set; }
        public string ClusterName { get; set; }

    }
    public class SearchStoreClusterDc
    {
        public List<int> ClusterIds { get; set; }
        public long StoreId { get; set; }
    }
    public class StoreClusterExecutiveDc
    {
        public long StoreId { get; set; }
        public string StoreName { get; set; }
        public int WarehouseId { get; set; }
        public string WarehouseName { get; set; }
        public string ExecutiveName { get; set; }
        public int ExecutiveId { get; set; }
        public int  ClusterId { get; set; }
        public string ClusterName { get; set; }
        public int NoOfBeat { get; set; }
    }

}
