using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.Masters.Store
{
    public class ClusterExecutive
    {
        public int ExecutiveId { get; set; }
        public string ExecutiveName { get; set; }
        public string Email { get; set; }
        public string EmployeeCode { get; set; }
        public string Mobile { get; set; }
        public string Role { get; set; }
        public int ClusterId { get; set; }
        public string ClusterNames { get; set; }
        public string WarehouseName { get; set; }
    }
}
