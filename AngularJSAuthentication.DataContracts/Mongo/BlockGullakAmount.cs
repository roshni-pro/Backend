using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.Mongo
{
    public class BlockGullakAmount
    {
        [BsonId]
        public ObjectId Id { get; set; }
        public int CustomerId { get; set; }
        public string Guid { get; set; }
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime CreatedDate { get; set; }
        public bool IsActive { get; set; }
        public double Amount { get; set; }
    }
    public class BlockCashAmount
    {
        [BsonId]
        public ObjectId Id { get; set; }
        public int CustomerId { get; set; }
        public List<long> OrderId { get; set; }
        public string Guid { get; set; }
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime CreatedDate { get; set; }
        public bool IsActive { get; set; }
        public double Amount { get; set; }
    }
    public class BlockPayLaterAmount
    {
        [BsonId]
        public ObjectId Id { get; set; }
        public int CustomerId { get; set; }
        public long StoreId { get; set; }
        public double Amount { get; set; }
        public bool IsActive { get; set; }
        public string Guid { get; set; }
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime CreatedDate { get; set; }

    }
}
