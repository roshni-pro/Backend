using AngularJSAuthentication.Model.Base.Audit;
using MongoDB.Bson;
using System;

namespace AngularJSAuthentication.DataContracts.Mongo
{

    public class LedgerQueueIndex
    {
        public string HashVal { get; set; }
        public string EntityName { get; set; }
    }

    public class LedgerQueueMongo : LedgerQueueIndex
    {
        public ObjectId Id { get; set; }
        public string ErrorMessage { get; set; }
        public string InnerException { get; set; }
        public string Source { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
