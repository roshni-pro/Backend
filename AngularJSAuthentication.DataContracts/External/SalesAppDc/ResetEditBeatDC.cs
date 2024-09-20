using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.External.SalesAppDc
{
    public class ResetEditBeatDC
    {
        public int ExecutiveId { get; set; }
        public int StoreId { get; set; }
        public List<int> ClusterIds { get; set; }
    }
}
