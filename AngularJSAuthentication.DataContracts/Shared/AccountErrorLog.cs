using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.Shared
{
    public class AccountErrorLog
    {
        public ObjectId Id { get; set; }
        public string CoRelationId { get; set; }
        public string IP { get; set; }
        public string ForwardedIps { get; set; }
        public string xForwardedHttpHeader { get; set; }
        public string Message { get; set; }
        public string InnerException { get; set; }
        public DateTime CreatedDate { get; set; }

    }
}
