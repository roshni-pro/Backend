using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;

namespace AngularJSAuthentication.API.Models
{
    public class CustomerProductSearch
    {
        [BsonId]
        public ObjectId Id { get; set; }
        public int customerId { get; set; }
        public string keyword { get; set; }
        public bool IsDeleted { get; set; }
        public List<int> Items { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    public class ExecutiveProductSearch
    {
        [BsonId]
        public ObjectId Id { get; set; }
        public int PeopleId { get; set; }
        public string keyword { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}