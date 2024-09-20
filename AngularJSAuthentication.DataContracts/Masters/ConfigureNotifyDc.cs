using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.Masters
{
    public class ConfigureNotifyDc
    {
        public long Id { get; set; }
        public int AppId { set; get; }   
        public string RoleId { set; get; }
        public string OnAction { set; get; } // Reattempt (Status) //
        public int AfterTimeinMins { set; get; } // Miniute
        public bool IsNotifcation { set; get; }
        public bool IsApproval { set; get; }//
        public DateTime CreatedDate { get; set; }
        public bool IsActive { get; set; }
        public bool? IsDeleted { get; set; }
    }
    public class SendConfigureNotifyDc
    {
        public string FcmId { get; set; }
        public int PeopleId { get; set; }

    }


}
