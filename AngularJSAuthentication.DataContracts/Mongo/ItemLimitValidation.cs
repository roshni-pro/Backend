using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.Mongo
{
    public class ItemLimitValidation
    {
        [BsonId]
        public ObjectId Id { get; set; }
        public string Guid { get; set; }
        public int ItemMultiMRPId { get; set; }
        public int WarehouseId { get; set; }
        public int ItemId { get; set; }
        public int Qty { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
