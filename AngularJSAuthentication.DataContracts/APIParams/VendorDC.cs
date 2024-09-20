using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.APIParams
{
   public  class VendorDC
    {
     
        public long Id { get; set; }
        public int StateId { get; set; }
        public string AddressOne { get; set; }
        public string AddressTwo { get; set; }
        public string VendorType { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public int? DepartmentId { get; set; }
        public int? WorkingLocationId { get; set; }
        public int? WorkingCompanyId { get; set; }
        public Boolean? IsTDSApplied { get; set; }
        public long? ExpenseTDSMasterID { get; set; }
        public int CreatedBy { get; set; }
        public string DepartmentName { get; set; }
        public string WorkingCompanyName { get; set; }
        public string WorkingLocationName { get; set; }
        public string StateName { get; set; }
        public string ExpenseTDSMasterName { get; set; }

    }
}
