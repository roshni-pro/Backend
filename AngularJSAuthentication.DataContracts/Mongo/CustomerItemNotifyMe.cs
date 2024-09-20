using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.Mongo
{
    public class CustomerItemNotifyMe
    {
        public ObjectId Id { get; set; }      
        public int WarehouseId { get; set; }
        public string ItemNumber { get; set; }
        public int ItemRequireCount { get; set; }
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime LastUpdated { get; set; }
        public bool IsSentNotify { get; set; }
        public List<CustomerNotifyMe> Customers { get; set; }
    }

    public class CustomerNotifyMe
    {
        public int CustomerId { get; set; }
        public string fcmId { get; set; }
        public bool IsNotify { get; set; }
        public string CustomerName { get; set; }
        public string Skcode { get; set; }
        public string Mobile { get; set; }

        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime CreatedDate { get; set; }
    }

    public class CustomerItemNotifyMeDc
    {
        public string WarehoueName { get; set; }
        public int WarehouseId { get; set; }
        public string ItemName { get; set; }
        public string BrandName { get; set; }
        public string ItemNumber { get; set; }
        public int ItemRequireCount { get; set; }     
        public DateTime LastUpdated { get; set; }
        public bool IsSentNotify { get; set; }
        public List<CustomerNotifyMeDc> Customers { get; set; }
    }
    public class CustomerItemNotifyMePaging
    {
        public List<CustomerItemNotifyMeDc> CustomerItemNotifyMeDcs { get; set; }
        public int total_count { get; set; }

    }
    public class CustomerNotifyMeDc
    {
        public int CustomerId { get; set; }
        public string fcmId { get; set; }
        public bool IsNotify { get; set; }
        public string CustomerName { get; set; }
        public string Skcode { get; set; }
        public string Mobile { get; set; }
        public DateTime CreatedDate { get; set; }
    }

}
