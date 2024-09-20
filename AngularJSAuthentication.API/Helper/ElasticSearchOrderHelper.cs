using AngularJSAuthentication.DataContracts.ElasticLanguageSearch;
using AngularJSAuthentication.DataContracts.ElasticSearch;
using AngularJSAuthentication.DataContracts.External;
using Elasticsearch.Net;
using Nest;
using Newtonsoft.Json;
using Nito.AsyncEx;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Threading;
using System.Web;

namespace AngularJSAuthentication.API.Helper
{
    public class ElasticSearchOrderHelper
    {       
        public static List<OrderSearchDataWithId> GetAllOrderSearchQuery(string query,string idnx,int skip,int take)
        {
            List<OrderSearchDataWithId> elasticSearchItem = new List<OrderSearchDataWithId> ();
            using (var _client = new HttpClient())
            {
                _client.DefaultRequestHeaders.Accept.Clear();
                _client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                _client.DefaultRequestHeaders.Add("Authorization", ConfigurationManager.AppSettings["elasticAuthorizationHeader"]);
                
                var BaseUrl = ConfigurationManager.AppSettings["ElasticSearchBaseUrl"] + idnx + "/_search";

                var msg = new HttpRequestMessage(System.Net.Http.HttpMethod.Post, BaseUrl);

                MediaTypeFormatter mediaTypeFormatter = new JsonMediaTypeFormatter();
                MediaTypeHeaderValue mediaTypeHeaderValue = MediaTypeHeaderValue.Parse("application/json");

                HttpContent content = new ObjectContent<object>(JsonConvert.DeserializeObject<object>(query.Replace("#skip",(skip*take).ToString()).Replace("#take",take.ToString())), mediaTypeFormatter, mediaTypeHeaderValue);
                msg.Content = content;

                var responseMessage = AsyncContext.Run(() => _client.SendAsync(msg));
                if (responseMessage.StatusCode == HttpStatusCode.OK)
                {
                    responseMessage.EnsureSuccessStatusCode();
                    var responsemsg = AsyncContext.Run(() => responseMessage.Content.ReadAsStringAsync());

                    var obj = JsonConvert.DeserializeObject<ElasticOrderSearchResult>(responsemsg);

                    if (obj != null && obj.hits != null && obj.hits.hits != null && obj.hits.hits.Any())
                    {                        
                        elasticSearchItem = obj.hits.hits.Select(z => new OrderSearchDataWithId
                        {
                            orderid = z._source.orderid,
                            status = z._source.status,
                            _id = z._id
                        }).ToList();
                        skip = (skip + 1) ;
                        elasticSearchItem.AddRange(GetAllOrderSearchQuery(query, idnx, skip, take));
                    }                    
                }
            }
            return elasticSearchItem;
        }

    }

    public class ElasticOrderSearchResult
    {
        public int took { get; set; }
        public bool timed_out { get; set; }
        public Shards _shards { get; set; }
        public Hits hits { get; set; }
        public bool isactive { get; set; }
    }

    public class Shards
    {
        public int total { get; set; }
        public int successful { get; set; }
        public int skipped { get; set; }
        public int failed { get; set; }
    }

    public class Hits
    {
        public Total total { get; set; }
        public double? max_score { get; set; }
        public List<Hit> hits { get; set; }
    }
    public class Hit
    {
        public string _index { get; set; }
        public string _type { get; set; }
        public string _id { get; set; }
        public object _score { get; set; }
        public orderData _source { get; set; }       
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

    public class orderData
    {
        public int orderid { get; set; }
        public string status { get; set; }
    }

    public class OrderSearchDataWithId
    {
        public string _id { get; set; }
        public int orderid { get; set; }
        public string status { get; set; }
    }
}
