using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.Transaction.RequestAccess
{
    public class RequestAccessDataViewModel
    {
        public string RoleId { get; set; }
        public DateTime ValidFrom { get; set; }
        public DateTime ValidTo { get; set; }
        public string CreatedBy { get; set; }
        public string Usertid { get; set; }
        public int? PageId { get; set; }


    }
    public class GetUniqueIdUser
    {
        public string Unid { get; set; }

    }
}
