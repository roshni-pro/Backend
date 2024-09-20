using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.ElasticSearch
{

    public class ElasticSearchResult
    {
        public int took { get; set; }
        public bool timed_out { get; set; }
        public Shards _shards { get; set; }
        public Hit hits { get; set; }
        public bool isactive { get; set; }
        public Suggest suggest { get; set; }
    }

    public class Shards
    {
        public int total { get; set; }
        public int successful { get; set; }
        public int skipped { get; set; }
        public int failed { get; set; }
    }

    public class Total
    {
        public int value { get; set; }
        public string relation { get; set; }
    }
        

    public class Fields
    {
        public List<int> sellerid { get; set; }
    }

    public class Hit
    {
        public string _index { get; set; }
        public string _type { get; set; }
        public string _id { get; set; }
        public object _score { get; set; }
        public ElasticSearchItem _source { get; set; }
        public List<double> sort { get; set; }
        public Total total { get; set; }
        public object max_score { get; set; }
        public List<Hit> hits { get; set; }
        public Fields fields { get; set; }
        public InnerHits inner_hits { get; set; }
    }

    public class Mindistance
    {
        public Hit hits { get; set; }
    }

    public class InnerHits
    {
        public Mindistance mindistance { get; set; }
    }
    public class Option
    {
        public string text { get; set; }
        public double score { get; set; }
        public int freq { get; set; }
    }

    public class Namesuggester
    {
        public string text { get; set; }
        public int offset { get; set; }
        public int length { get; set; }
        public List<Option> options { get; set; }
    }

    public class Suggest
    {
        public List<Namesuggester> namesuggester { get; set; }
    }

}
