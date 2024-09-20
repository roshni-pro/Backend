using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.Mongo
{
    public class CustomerOtp
    {
        public ObjectId Id { get; set; }
        public string Mobile { get; set; }
        public string OTP { get; set; }

        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime GenertedTime { get; set; }
        
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime ExpiryTime { get; set; }
    }
}
