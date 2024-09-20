using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.APIParams
{
    public class BookExpensePager
    {
        public int SkipCount { get; set; } 
        public int Take { get; set; }
        public string Filter { get; set; }
        public int? DepartmentId { get; set; }
        public long? WorkingCompanyId { get; set; }
        public int? WorkingLocatiponID { get; set; }
    }
}
