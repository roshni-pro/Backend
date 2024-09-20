using MongoDB.Bson;
using System;

namespace AngularJSAuthentication.DataContracts.Mongo
{
    public class MakerChecker
    {
        public ObjectId Id { get; set; }
        public string EntityName { get; set; }
        public string EntityId { get; set; }
        public string Operation { get; set; } // I,U,D
        public string OldJson { get; set; }
        public string NewJson { get; set; }
        public string Status { get; set; } // Pending,Approved,Rejected
        public string CheckerComment { get; set; }
        public string MakerBy { get; set; }
        public string CheckerBy { get; set; }
        public DateTime MakerDate { get; set; }
        public DateTime? CheckerDate { get; set; }



    }
}
