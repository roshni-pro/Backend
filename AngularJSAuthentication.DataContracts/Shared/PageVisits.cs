using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace AngularJSAuthentication.DataContracts.Shared
{
    public class PageVisits
    {
        public ObjectId Id { get; set; }
        public string Route { get; set; }
        public string UserName { get; set; }

        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime VisitedOn { get; set; }
    }


    public class PageVisitsSqlBulk
    {
        public string Route { get; set; }
        public string UserName { get; set; }
        public DateTime VisitedOn { get; set; }
    }
}
