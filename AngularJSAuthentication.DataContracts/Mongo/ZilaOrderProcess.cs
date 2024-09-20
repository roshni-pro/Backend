using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.Mongo
{
    public class ZilaOrderProcess
    {
        public ObjectId Id { get; set; }
        public int OrderId { get; set; }
        public string Status { get; set; }
        public bool IsProcess { get; set; }
        public string Error { get; set; }

        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime CreateDate { get; set; }
    }
}

