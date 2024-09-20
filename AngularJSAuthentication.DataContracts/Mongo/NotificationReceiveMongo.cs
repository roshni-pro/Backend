using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.Mongo
{
    public class NotificationReceiveMongo
    {
            public ObjectId Id { get; set; }
            public int CustomerId { get; set; }
            public int NotificationId { get; set; }
            public bool IsUpdated { get; set; }
    }
}
