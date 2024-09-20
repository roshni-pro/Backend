using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Collections.Generic;

namespace AngularJSAuthentication.DataContracts.Transaction.Mongo.CpMatrix
{
    public class CustomerProductMatrix
    {
        [BsonId]
        public ObjectId Id { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }
        public List<ProductList> ProductList { get; set; }

    }

    public class ProductList
    {
        public int ItemMultiMrpId { get; set; }
        public string ItemName { get; set; }
        public string ItemNumber { get; set; }
        public double OrderAmount { get; set; }
        public List<CustomerDetail> Customers { get; set; }
    }

    public class CustomerDetail
    {
        public int CustomerId { get; set; }
        public string SkCode { get; set; }
        public string ShopName { get; set; }
        public string CustomerName { get; set; }
        public int WarehouseId { get; set; }
        public string WarehouseName { get; set; }
        public double OrderAmount { get; set; }
    }
}
