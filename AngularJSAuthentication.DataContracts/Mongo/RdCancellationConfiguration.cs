using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;


namespace AngularJSAuthentication.DataContracts.Mongo
{
   public class RdCancellationConfiguration
    {
        [BsonId]
        public ObjectId Id { get; set; }
        public string DepartmentName { get; set; }
        public string  Role { get; set; }
        public bool isActive { get; set; } 
    } 
}
