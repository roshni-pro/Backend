using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.Mongo
{
    public class ManualAutoNotification
    {
        public ObjectId Id { get; set; }
        public int ObjectId { get; set; }
        public string ObjectType { get; set; }
        public string FcmKey { get; set; }
        public string NotificationMsg { get; set; }
        public bool IsSent { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    public class DefaultNotificationMessage
    {
        public ObjectId Id { get; set; }
        public string NotificationMsg { get; set; }
        public string NotificationMsgType { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
