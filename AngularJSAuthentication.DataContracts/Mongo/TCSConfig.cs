using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.Mongo
{
    public class TCSConfig
    {
        [BsonId]
        public ObjectId Id { get; set; }
        public double NotGSTTCSPercent { get; set; }
        public double GSTTCSPercent { get; set; }
        public double TCSAmountLimit { get; set; }
        public string FinancialYear { get; set; }
    }

    public class TCSCustomer
    {
        [BsonId]
        public ObjectId Id { get; set; }
        public int CustomerId { get; set; }
        public double TotalPurchase { get; set; }
        public string FinancialYear { get; set; }
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime LastUpdatedDate { get; set; }
       
    }
}
