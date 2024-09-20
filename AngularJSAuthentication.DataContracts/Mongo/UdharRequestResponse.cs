using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace AngularJSAuthentication.DataContracts.Mongo
{
   public class UdharRequestResponse
    {
        [BsonId]
        public ObjectId Id { get; set; }
        public int CustomerId { get; set; }
        public string Type { get; set; }
        public string Url { get; set; }
        public string RequestResponseMsg { get; set; }
        public string Header { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    public class UdharOverDueDayValidation
    {
        [BsonId]
        public ObjectId Id { get; set; }        
        public int MinOverDueDay { get; set; }
        public int MaxOverDueDay { get; set; }
        public int SalesMinDay { get; set; }
    }

    public class SKSocial
    {
        [BsonId]
        public ObjectId Id { get; set; }
        public int CityId { get; set; }
    }


}
