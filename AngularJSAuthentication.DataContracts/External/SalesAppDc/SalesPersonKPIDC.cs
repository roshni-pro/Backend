using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.External.SalesAppDc
{
    public class SalesPersonKPIDC
    {
        public long Id { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }
        public int ExecutiveId { get; set; }
        public int ClusterId { get; set; }
        public double Target { get; set; }
        public double IncentiveAmount { get; set; }
        public int StoreId { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public bool IsActive { get; set; }
        public bool? IsDeleted { get; set; }
        public int CreatedBy { get; set; }
        public int? ModifiedBy { get; set; }

    }
    
}
