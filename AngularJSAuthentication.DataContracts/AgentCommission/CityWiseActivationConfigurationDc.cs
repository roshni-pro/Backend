using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.AgentCommission
{
    public class CityWiseActivationConfigurationDc
    {

        public long Id { get; set; }
        public long CityId { get; set; }
        public decimal CommissionAmount { get; set; }
        public string CommissionType { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public bool IsActive { get; set; }
        public bool? IsDeleted { get; set; }
        public int CreatedBy { get; set; }
        public int? ModifiedBy { get; set; }
    }
}
