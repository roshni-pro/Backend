using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.Mongo
{
    public class ElasticSearchQuery
    {
        public ObjectId Id { get; set; }
        public string ObjectType { get; set; }
        public string QueryType { get; set; }
        public string Query { get; set; }
    }
}
