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
    public class ElasticSearchHelper
    {
        public bool AddElasticProduct(List<ElasticSearchItem> elasticSearchItems)
        {
            bool Result = false;
            var node = new Uri(ConfigurationManager.AppSettings["ElasticSearchBaseUrl"]);
            var pool = new SingleNodeConnectionPool(node);
            String userName = ConfigurationManager.AppSettings["elasticUserName"];
            String password = ConfigurationManager.AppSettings["elasticPassword"];
            var settings = new ConnectionSettings(pool)
                .DefaultMappingFor<ElasticSearchItem>(m => m
                    .IndexName(ConfigurationManager.AppSettings["ElasticSearchIndexName"])

                ).BasicAuthentication(userName, password);

            var client = new ElasticClient(settings);
            foreach (var elasticSearchItem in elasticSearchItems)
            {
                Result = DeleteByProductId(elasticSearchItem.id);
            }

            var response = client.IndexMany<ElasticSearchItem>(elasticSearchItems);
            if (response.IsValid)
            {
                Result = true;
            }

            return Result;
        }

        public bool DropElasticIndex(string IndexName)
        {
            bool Result = false;
            var node = new Uri(ConfigurationManager.AppSettings["ElasticSearchBaseUrl"]);
            var pool = new SingleNodeConnectionPool(node);
            String userName = ConfigurationManager.AppSettings["elasticUserName"];
            String password = ConfigurationManager.AppSettings["elasticPassword"];
            var settings = new ConnectionSettings(pool)
                .DefaultMappingFor<ElasticSearchItem>(m => m
                    .IndexName(IndexName)

                ).BasicAuthentication(userName, password);

            var client = new ElasticClient(settings);
            var response = client.Indices.Delete(IndexName);
            if (response.IsValid)
            {
                Result = true;
            }

            return Result;
        }

        public bool DeleteAllRecoredElasticIndex(string IndexName)
        {
            bool Result = false;
            var node = new Uri(ConfigurationManager.AppSettings["ElasticSearchBaseUrl"]);
            var pool = new SingleNodeConnectionPool(node);
            String userName = ConfigurationManager.AppSettings["elasticUserName"];
            String password = ConfigurationManager.AppSettings["elasticPassword"];
            var settings = new ConnectionSettings(pool)
                .DefaultMappingFor<ElasticSearchItem>(m => m
                    .IndexName(IndexName)

                ).BasicAuthentication(userName, password);

            var client = new ElasticClient(settings);
            DeleteByQueryResponse response = null;
            if (client.Indices.Exists(IndexName).Exists)
            {
                 response = client.DeleteByQuery<ElasticSearchItem>(s => s.Query(q => q.MatchAll()).Index(IndexName));
            }
            // var response = client.Indices.Delete(IndexName);
            if (response!=null && response.IsValid)
            {
                Result = true;
            }

            return Result;
        }

        public bool DeleteByProductId(long itemId)
        {
            var node = new Uri(ConfigurationManager.AppSettings["ElasticSearchBaseUrl"]);
            var pool = new SingleNodeConnectionPool(node);
            String userName = ConfigurationManager.AppSettings["elasticUserName"];
            String password = ConfigurationManager.AppSettings["elasticPassword"];
            var settings = new ConnectionSettings(pool)
                .DefaultMappingFor<ElasticSearchItem>(m => m
                    .IndexName(ConfigurationManager.AppSettings["ElasticSearchIndexName"])

                ).BasicAuthentication(userName, password);

            var client = new ElasticClient(settings);
            var index = IndexName.From<ElasticSearchItem>();
            var response = client.DeleteByQuery<ElasticSearchItem>(u => u
              .Query(q => q
                  .Term(f => f.id, itemId)
                  )

                  .Conflicts(Conflicts.Proceed)
                  .Refresh(true)
                   );

            return response.IsValid;

        }

        public static List<ElasticSearchItem> GetItemByElasticSearchQuery(string query, out DataContracts.ElasticSearch.Suggest suggest)
        {
            List<ElasticSearchItem> elasticSearchItem = new List<ElasticSearchItem>();
            using (var _client = new HttpClient())
            {
                _client.DefaultRequestHeaders.Accept.Clear();
                _client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                _client.DefaultRequestHeaders.Add("Authorization", ConfigurationManager.AppSettings["elasticAuthorizationHeader"]);
                suggest = new DataContracts.ElasticSearch.Suggest();
                var BaseUrl = ConfigurationManager.AppSettings["ElasticSearchBaseUrl"] + ConfigurationManager.AppSettings["ElasticSearchIndexName"] + "/_search";

                var msg = new HttpRequestMessage(System.Net.Http.HttpMethod.Post, BaseUrl);

                MediaTypeFormatter mediaTypeFormatter = new JsonMediaTypeFormatter();
                MediaTypeHeaderValue mediaTypeHeaderValue = MediaTypeHeaderValue.Parse("application/json");

                HttpContent content = new ObjectContent<object>(JsonConvert.DeserializeObject<object>(query), mediaTypeFormatter, mediaTypeHeaderValue);
                msg.Content = content;

                var responseMessage = AsyncContext.Run(() => _client.SendAsync(msg));
                if (responseMessage.StatusCode == HttpStatusCode.OK)
                {
                    responseMessage.EnsureSuccessStatusCode();
                    var responsemsg = AsyncContext.Run(() => responseMessage.Content.ReadAsStringAsync());

                    var obj = JsonConvert.DeserializeObject<ElasticSearchResult>(responsemsg);

                    if (obj != null && obj.hits != null && obj.hits.hits != null && obj.hits.hits.Any())
                    {
                        obj.hits.hits.ForEach(x => x._source.Score = Convert.ToDouble(x._score));
                        elasticSearchItem = obj.hits.hits.Select(z => z._source).ToList();
                    }
                    else if (obj.suggest != null)
                    {
                        suggest = obj.suggest;
                    }
                }

            }

            return elasticSearchItem;
        }

        public bool BulkInsert(List<ElasticSearchItem> elasticSearchItems)
        {
            var node = new Uri(ConfigurationManager.AppSettings["ElasticSearchBaseUrl"]);
            var pool = new SingleNodeConnectionPool(node);
            String userName = ConfigurationManager.AppSettings["elasticUserName"];
            String password = ConfigurationManager.AppSettings["elasticPassword"];
            var settings = new ConnectionSettings(pool)
                 .DefaultMappingFor<ElasticSearchItem>(m => m
                     .IndexName(ConfigurationManager.AppSettings["ElasticSearchIndexName"])

                 ).BasicAuthentication(userName, password);
            var client = new ElasticClient(settings);

            var idxName = ConfigurationManager.AppSettings["ElasticSearchIndexName"];

            var indexDescriptor = new CreateIndexDescriptor(idxName)
               .Map<ElasticSearchItem>(m => m.DateDetection(true).AutoMap());


            if (!client.Indices.Exists(idxName).Exists)
            {
                var createresponse = client.Indices.Create(idxName,
                                    d => d.Map<ElasticSearchItem>(
                                        w => w.AutoMap()
                                    ));
            }


            var bulkAllObservable = client.BulkAll(elasticSearchItems, b => b
                        .Index(idxName)
                        // how long to wait between retries
                        .BackOffTime("10s")
                        // how many retries are attempted if a failure occurs
                        .BackOffRetries(2)
                        // refresh the index once the bulk operation completes
                        .RefreshOnCompleted(false)
                        // how many concurrent bulk requests to make
                        .MaxDegreeOfParallelism(Environment.ProcessorCount)
                        // number of items per bulk request
                        .Size(5000)
                        .ContinueAfterDroppedDocuments(true)
                        .DroppedDocumentCallback((item, doc) =>
                        {
                            // LogHelper.LogError($"Could not index doc.{Environment.NewLine}{item}{Environment.NewLine}{System.Text.Json.JsonSerializer.Serialize(doc)}");
                        })
                    );
            //.Wait(TimeSpan.FromSeconds(5), null);

            var handle = new ManualResetEvent(false);
            //var p = OpenSecondConsole();
            //WriteMessageToSecondConsole(p,$"Start Bulk Index {rawDatas.Count}");

            var observer = new BulkAllObserver(
            onNext: r =>
            {
                var page = r.Page;
                //LogHelper.LogTrace($"Index: {idxName},  {page} is done for {rawDatas.Count} rows");
            },
            onError: async e =>
            {
                var errorMsg = $"MatchingIndexEngine - error building index: {idxName} | Message: {e.Message} | Inner Exception: {e.InnerException} | Data: {e.Data}";
                handle.Set();

            },
            onCompleted: async () =>
            {
                //LogHelper.LogTrace($"Index: {idxName}, Indexed {rawDatas.Count} documents");
                handle.Set();
            }
            );
            bulkAllObservable.Subscribe(observer);


            handle.WaitOne();
            handle.Dispose();


            return true;
        }
    }
    public class ElasticLanguageHelper
    {
        public bool AddElasticLanguageData(string ElasticLngIndex, List<ElasticLanguageData> elasticLanguageDatas)
        {
            bool Result = false;
            //string ElasticLngIndex = "ElasticLanguageData";
            var client = CreateElaticConfig(ElasticLngIndex);

            var response = client.IndexMany<ElasticLanguageData>(elasticLanguageDatas);
            if (response.IsValid)
            {
                Result = true;
            }

            return Result;
        }

        public void CreateLanguageIndex(string indexname)
        {

            var client = CreateElaticConfig(indexname);


            #region Delete All documents

            if (!client.Indices.Exists(indexname).Exists)
            {
                var delete = client.Indices.Delete(indexname);

                var indexDescriptor = new CreateIndexDescriptor(indexname)
                            .Map<ElasticLanguageData>(m => m.DateDetection(true).AutoMap());

                var response = client.Indices.Create(indexname,
                                    index => index.Map<ElasticLanguageData>(
                                        x => x.AutoMap()
                                    ));
            }
            #endregion






        }

        public ElasticClient CreateElaticConfig(string indexName)
        {
            var node = new Uri(ConfigurationManager.AppSettings["ElasticSearchBaseUrl"]);
            var pool = new SingleNodeConnectionPool(node);
            String userName = ConfigurationManager.AppSettings["elasticUserName"];
            String password = ConfigurationManager.AppSettings["elasticPassword"];

            if (!string.IsNullOrEmpty(indexName))
            {
                var settings = new ConnectionSettings(pool)
               .DefaultMappingFor<ElasticLanguageData>(m => m
                   .IndexName(indexName)

               ).BasicAuthentication(userName, password);

                var client = new ElasticClient(settings);
                var index = IndexName.From<ElasticLanguageData>();
                return client;
            }
            else
            {
                return null;
            }


        }

        public List<ElasticLanguageData> GetAllElasticLanguageQuery(string query, string indexName)
        {
            List<ElasticLanguageData> elasticSearchLanguage = new List<ElasticLanguageData>();
            using (var _client = new HttpClient())
            {
                _client.DefaultRequestHeaders.Accept.Clear();
                _client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                _client.DefaultRequestHeaders.Add("Authorization", ConfigurationManager.AppSettings["elasticAuthorizationHeader"]);
                //suggest = new Suggest();
                var BaseUrl = ConfigurationManager.AppSettings["ElasticSearchBaseUrl"] + indexName + "/_search";

                var msg = new HttpRequestMessage(System.Net.Http.HttpMethod.Post, BaseUrl);

                MediaTypeFormatter mediaTypeFormatter = new JsonMediaTypeFormatter();
                MediaTypeHeaderValue mediaTypeHeaderValue = MediaTypeHeaderValue.Parse("application/json");

                HttpContent content = new ObjectContent<object>(JsonConvert.DeserializeObject<object>(query), mediaTypeFormatter, mediaTypeHeaderValue);
                msg.Content = content;

                var responseMessage = AsyncContext.Run(() => _client.SendAsync(msg));
                if (responseMessage.StatusCode == HttpStatusCode.OK)
                {
                    responseMessage.EnsureSuccessStatusCode();
                    var responsemsg = AsyncContext.Run(() => responseMessage.Content.ReadAsStringAsync());

                    var obj = JsonConvert.DeserializeObject<ElasticLanguageResult>(responsemsg);

                    if (obj != null && obj.hits != null && obj.hits.hits != null && obj.hits.hits.Any())
                    {
                        //obj.hits.hits.ForEach(x => x._source.Score = Convert.ToDouble(x._score));
                        elasticSearchLanguage = obj.hits.hits.Select(z => z._source).ToList();
                    }
                    //else if (obj.suggest != null)
                    //{
                    //    suggest = obj.suggest;
                    //}
                }

            }

            return elasticSearchLanguage;
        }
    }

    //public abstract class ElasticHelper<T>
    //{
    //    public ElasticClient CreateElaticConfig(string indexName = "")
    //    {
    //        var node = new Uri(ConfigurationManager.AppSettings["ElasticSearchBaseUrl"]);
    //        var pool = new SingleNodeConnectionPool(node);
    //        String userName = ConfigurationManager.AppSettings["elasticUserName"];
    //        String password = ConfigurationManager.AppSettings["elasticPassword"];

    //        if (!string.IsNullOrEmpty(indexName))
    //        {
    //            var settings = new ConnectionSettings(pool)
    //           .DefaultMappingFor<T>(m => m
    //               .IndexName(indexName)

    //           ).BasicAuthentication(userName, password);

    //            var client = new ElasticClient(settings);
    //            var index = IndexName.From<T>();
    //            return client;
    //        }
    //        else
    //        {
    //            return null;
    //        }


    //    }

    //    public static void AddHeaders(HttpClient client)
    //    {
    //        client.DefaultRequestHeaders.Accept.Clear();
    //        client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
    //        client.DefaultRequestHeaders.Add("Authorization", ConfigurationManager.AppSettings["elasticAuthorizationHeader"]);

    //    }
    //}
}
