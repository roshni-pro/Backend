using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.Transaction.ClusterDashboard
{
    public class ClusterRefreshRequestDc
    {
        public int Id { get; set; }
        public DateTime RefreshDate { get; set; }
        public int CityId { get; set; }

        public string CityName { get; set; }
        public int RefreshCustomerCount { get; set; }
        public int Status { get; set; }
        public int RefereshBy { get; set; }

        public string strStatus
        {
            get
            {
                return Status == 0 ? "Inprogress" : (Status == 1 ? "Success" : "Error");
            }
        }
        public string RefereshByName { get; set; }

    }
}
