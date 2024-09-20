using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.Mongo
{
    public class BatchCodePendingTransaction
    {
        [BsonId]
        public ObjectId Id { get; set; }

        //public long Timestamp { get; set; }
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime TransactionDate { get; set; }

        public long ObjectId { get; set; }
        public long ObjectDetailId { get; set; }
        public string TransactionType { get; set; }
        //public string TransactionId { get; set; }

    }
}
