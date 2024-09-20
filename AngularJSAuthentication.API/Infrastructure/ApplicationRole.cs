using Microsoft.AspNet.Identity.EntityFramework;
using System;

namespace AspNetIdentity.WebApi.Infrastructure
{
    public class ApplicationRole : IdentityRole
    {
        public ApplicationRole() : base() { }

        //public ApplicationRole(string name, bool istemp) : base(name)
        //{
        //    this.IsTemp = istemp;
        //}
        public virtual bool IsTemp { get; set; }
        public virtual DateTime? CreatedDate { get; set; }
        public virtual string CreatedBy { get; set; }
        public virtual DateTime? ModifiedDate { get; set; }
        public virtual string ModifiedBy { get; set; }

    }


}