using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.Mongo
{
    public class DeliveryCapacityOpti
    {
        public ObjectId Id { get; set; }
        public int Warehouseid { get; set; }
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime DeliveryDate { get; set; }
        public int OrderedCount { get; set; }
        public int DeliveredCount { get; set; }
        public double DeliveredPercent { get; set; }
        public int ThresholdCount { get; set; }
        public int cumCountPending { get; set; }
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime ETADelayDate { get; set; }
        public int OrderCapacity { get; set; }
        public int UpdateEta { get; set; }
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime CreatedDate { get; set; }
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime ModifiedDate { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public int CreatedBy { get; set; }
        public int ModifiedBy { get; set; }
        

    }
    public class ResDeliveryCapacity
    {
        public bool Result { get; set; }
        public string msg { get; set; }
    }

    public class ResponseExportData
    {
        public long WarehouseId { get; set; }
        public string WarehouseName { get; set; }
        public DateTime DeliveryDate { get; set; }
        public int OrderedCount { get; set; }
        public double DeliveredPercent { get; set; }
        public int ThresholdCount { get; set; }
        public int cumCountPending { get; set; }
        public DateTime ETADelayDate { get; set; }
    }
}
