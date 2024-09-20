using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.ServiceRequestParam
{
    public class TraceLog
    {
        public ObjectId Id { get; set; }
        public string CoRelationId { get; set; }
        public string UserName { get; set; }
        public string RequestInfo { get; set; }
        public string IP { get; set; }
        public string ForwardedIps { get; set; }
        public string xForwardedHttpHeader { get; set; }
        public string Referrer { get; set; }
        public string Method { get; set; }
        public string Headers { get; set; }
        public string LogType { get; set; }
        public string Message { get; set; }
        public string Browser { get; set; }
        public DateTime CreatedDate { get; set; }
    }


    public class ErrorLog
    {
        public ObjectId Id { get; set; }
        public string CoRelationId { get; set; }
        public string IP { get; set; }
        public string ForwardedIps { get; set; }
        public string xForwardedHttpHeader { get; set; }
        public string Message { get; set; }
        public DateTime CreatedDate { get; set; }

    }

    public class MongoLog
    {
        public int Count { get; set; }
        public List<TraceLog> TraceLogList { get; set; }
    }
}
