using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.BatchCode
{
    public class BatchCodeSubject
    {
        public string TransactionType { get; set; }
        public long ObjectId { get; set; }
        public long ObjectDetailId { get; set; }
        public int Quantity { get; set; }
        public DateTime TransactionDate { get; set; }
        public int ItemMultiMrpId { get; set; }
        public int WarehouseId { get; set; }
        public string HashCode { get; set; }
        public string QueueName { get; set; }


    }

    public class BatchCodeSubjectMongo
    {
        public string TransactionType { get; set; }
        public long ObjectId { get; set; }
        public long ObjectDetailId { get; set; }
        public int Quantity { get; set; }
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime TransactionDate { get; set; }
        public int ItemMultiMrpId { get; set; }
        public int WarehouseId { get; set; }
        public string HashCode { get; set; }
        public ObjectId Id { get; set; }

    }

    public class BatchCodeSubjectMongoQueue
    {
        public string TransactionType { get; set; }
        public long ObjectId { get; set; }
        public long ObjectDetailId { get; set; }
        public int Quantity { get; set; }
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime TransactionDate { get; set; }
        public int ItemMultiMrpId { get; set; }
        public int WarehouseId { get; set; }
        public string HashCode { get; set; }
        public ObjectId Id { get; set; }
        public bool IsProcess { get; set; }

        public bool IsPublishErrorOccurs { get; set; }
        public bool IsSubscriberErrorOccurs { get; set; }
        public string PublishError { get; set; }
        public string SubscriberError { get; set; }

        public string QueueName { get; set; }
    }

    public class BatchCodeSubjectMongoError: BatchCodeSubjectMongo
    {
       

    }
    public class BatchCodeSubjectDc
    {
        public string StockType { get; set; }
        public int ItemMultiMrpId { get; set; }
        public int WarehouseId { get; set; }
        public long ObjectDetailId { get; set; }
        public long ObjectId { get; set; }
        public int Quantity { get; set; }
        
    }
}
