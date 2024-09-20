using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.Masters
{
    public class CustomerExport
    {
        public int RetailerId { get; set; }
        public string RetailersCode { get; set; }
        public string CRMTags { get; set; }
        //  public string RetailerName { get; set; }
        public string ShopName { get; set; }
        //public string Mobile { get; set; }
        public string Address { get; set; }        
        public string Area { get; set; }
        public string city { get; set; }
        public string Warehouse { get; set; }
        public int? ExecutiveId { get; set; }
        public string ExecutiveName { get; set; }
        public string Emailid { get; set; }

        public int? ClusterId { get; set; }
        public string ClusterName { get; set; }

        public double latitute { get; set; }
        public double longitute { get; set; }
        public string Day { get; set; }

        public int? BeatNumber { get; set; }

        public string Description { get; set; }

        public string CustomerVerify { get; set; }

        public bool Deleted { get; set; }
        public bool Active { get; set; }
        public bool IsKPP { get; set; }
        public string AgentCode { get; set; }
        public DateTime CreationDate { get; set; }
        public DateTime UpdatedDate { get; set; }
        public string LastModifiedBy { get; set; }
        public string CustomerAppTypes { get; set; }
        public string CurrentAPKversion { get; set; }

        public string VerifiedBy { get; set; }

        public DateTime? VerifiedDate { get; set; }
        public bool IsFranchise { get; set; }
        public string FranchiseApprovedby { get; set; }
        public DateTime? FranchiseApprovedDate { get; set; }
       // public string Channelype { get; set; }

    }
}
