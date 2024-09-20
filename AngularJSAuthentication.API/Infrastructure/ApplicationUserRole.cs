using Microsoft.AspNet.Identity.EntityFramework;
using System;

namespace AspNetIdentity.WebApi.Infrastructure
{

    public class ApplicationUserRole : IdentityUserRole
    {
        public bool IsPrimary { get; set; }
        public DateTime? validFrom { get; set; }
        public DateTime? validTill { get; set; }
        public int? ReqAccId { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public string ModifiedBy { get; set; }
        public bool isActive { get; set; }

    }

}