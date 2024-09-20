using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.Masters.Store
{
    public class ClusterStoreExecutiveDc
    {
        public long Id { get; set; }
        public int ClusterId { get; set; }
        public string ClusterName { get; set; }
        public long StoreId { get; set; }
        public string StoreName { get; set; }
        public long ChannelMasterId { get; set; }
        public string ChannelName { get; set; }
        public int ExecutiveId { get; set; }
        public string ExecutiveName { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public bool IsActive { get; set; }
        public string CreatedBy { get; set; }
        public string ModifiedBy { get; set; }
        public string Empcode { get; set; }

    }    
    public class ClusterIdDC
    {
       public List<int> ClusterIds { get; set; }
    }
}
