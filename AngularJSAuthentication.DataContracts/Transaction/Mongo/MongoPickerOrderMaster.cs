using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.Transaction.Mongo
{

    public class PickerChooseMaster 
    {
        [BsonId]
        public ObjectId Id { get; set; }
        public int CreatedBy { get; set; }
        public int UpdateBy { get; set; }
        public int WarehouseId { get; set; }
        public bool Finalize { get; set; }
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime CreatedDate { get; set; }
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime UpdatedDate { get; set; }
        public int CustomerType { get; set; }
        public virtual List<MongoPickerOrderMaster> mongoPickerOrderMaster { get; set; }
    }
    public class MongoPickerOrderMaster
    {
        [BsonId]
        public ObjectId Id { get; set; }
        public int OrderId { get; set; }
        public string Status { get; set; }
        public int WarehouseId { get; set; }
        public int ClusterId { get; set; }
        public string Skcode { get; set; }
        public string ShopName { get; set; }
        public string ShippingAddress { get; set; }
        public double GrossAmount { get; set; }
        public int PickerOrderStatus { get; set; } // 1 Pick,2 Pick Generate,3 reject
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime CreatedDate { get; set; }
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime UpdatedDate { get; set; }
        public int CustomerType { get; set; }
        public virtual List<MongoPickerOrderDetails> orderDetails { get; set; }
    }
    public class MongoPickerOrderDetails
    {
        public Guid GUID { get; set; }
        public int OrderDetailsId { get; set; }
        public int OrderId { get; set; }
        public int WarehouseId { get; set; }
        public int Qty { get; set; }
        public string itemNumber { get; set; }
        public string itemname { get; set; }
        public bool IsFreeItem { get; set; }
        public double price { get; set; }
        public double UnitPrice { get; set; }
        public int ItemMultiMrpId { get; set; }
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime CreatedDate { get; set; }
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime UpdatedDate { get; set; }
        public bool IsDispatchedFreeStock { get; set; }
    }
}