using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.ElasticSearch
{
    public class QueryToPostElasticsearch
    {
        public string query { get; set; }
        public int fetch_size { get; set; }
    }

    public class ElasticsearchNextPage
    {
        public string cursor { get; set; }
    }

    public class ElasticQueryResult
    {
        public List<ElasticQueryColumn> columns { get; set; }

        public List<List<Object>> rows { get; set; }
        public string cursor { get; set; }
    }

    public class ElasticQueryColumn
    {
        public string name { get; set; }
        public string type { get; set; }
    }
}
