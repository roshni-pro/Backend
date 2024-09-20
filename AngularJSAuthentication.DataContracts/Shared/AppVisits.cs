using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.Shared
{
    public class AppVisits
    {
        public ObjectId Id { get; set; }
        public string UserName { get; set; }
        public int UserId { get; set; }
        public string AppType { get; set; }

        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime VisitedOn { get; set; }
    }
}
