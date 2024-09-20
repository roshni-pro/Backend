using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.Transaction.Accounts
{
   public class AssignmentCommissionPager
    {
        public List<AssignmentCommissionDc> AssignmentCommissionList { get; set; }
        public int NetRecords { get; set; }
    }
}
