using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.Transaction.AutoNotification
{
    public class FcmNotificationMaster
    {
        public string title { get; set; }
        public string body { get; set; }
        public string icon { get; set; }
        public string notify_type { get; set; }
        public int ObjectId { get; set; }

    }

    public class FcmNotification
    {
        public FcmNotificationMaster data { get; set; }
        public string to { get; set; }

    }
}
