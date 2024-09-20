using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.Mongo
{
    public class UPIcallBackResMongo
    {
        [BsonId]
        public ObjectId Id { get; set; }
        public string EncryptRes { get; set; }
        public string TxnNo { get; set; }
        public string UPITxnId { get; set; }
        public string DecryptRes { get; set; }
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime CreatedDate { get; set; }
    }
}
