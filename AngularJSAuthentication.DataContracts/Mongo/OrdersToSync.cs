using MongoDB.Bson;
using System;

namespace AngularJSAuthentication.DataContracts.Mongo
{
    public class OrdersToSync
    {
        public ObjectId Id { get; set; }
        public int OrderId { get; set; }
        public string NewStatus { get; set; }
        public DateTime CreateOrUpdateDate { get; set; }
        public bool IsProcessed { get; set; }
        public DateTime? ProcessedDate { get; set; }
    }
}
