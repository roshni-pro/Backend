using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.Masters
{
   public class SalesAppDashboardDc
    {
        public string ExectiveName { get; set; }
        public string WarehouseName { get; set; }
        public string ClusterName { get; set; }
        public int TotalCustomer { get; set; }
        public int TotalBeatCustomer { get; set; }
        public int PlannVisited { get; set; }
        public int Visited { get; set; }
        public string Conversion { get; set; }
        public int Order { get; set; }
        public double OrderAmount { get; set; }
        public int AvgLine { get; set; }

    }
}
