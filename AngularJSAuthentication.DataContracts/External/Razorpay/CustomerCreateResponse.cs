using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.External.Razorpay
{
    public class CustomerCreateResponse
    {
        public string id { get; set; }
        public string name { get; set; }
        public string contact { get; set; }
        public string email { get; set; }
        public string fail_existing { get; set; }
        public string gstin { get; set; }
        public CreateCustNotes notes { get; set; }
        public int created_at { get; set; }
        public ErrorResponse error { get; set; }
    }
}
