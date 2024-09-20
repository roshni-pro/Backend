using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.Mongo
{
    public class ArthMateWebhookResponse
    {
        public ObjectId Id { get; set; }
        public string WebhookName { get; set; }
        public string RequestId { get; set; }
        public string Response { get; set; }
        public bool IsActive { get; set; }

        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime CreatedDate { get; set; }
    }
}
