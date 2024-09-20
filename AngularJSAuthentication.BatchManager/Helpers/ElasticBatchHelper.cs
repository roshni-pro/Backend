using AngularJSAuthentication.DataContracts.BatchCode;
using Elasticsearch.Net;
using Nest;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.BatchManager.Helpers
{
    public class ElasticBatchHelper
    {
        public ElasticClient CreateElaticConfig()
        {
            var node = new Uri(ConfigurationManager.AppSettings["ElasticSearchBaseUrl"]);
            var pool = new SingleNodeConnectionPool(node);
            String userName = ConfigurationManager.AppSettings["elasticUserName"];
            String password = ConfigurationManager.AppSettings["elasticPassword"];
            string indexName = ConfigurationManager.AppSettings["ElasticSearchBatchcodeIndexName"];

            var settings = new ConnectionSettings(pool)
              .DefaultMappingFor<BatchCodeElastic>(m => m
                  .IndexName(indexName)

              ).BasicAuthentication(userName, password);

            var client = new ElasticClient(settings);
            var index = IndexName.From<BatchCodeElastic>();
            return client;
        }

        public static void AddHeaders(HttpClient client)
        {
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Add("Authorization", ConfigurationManager.AppSettings["elasticAuthorizationHeader"]);

        }


        public void CreateIndex()
        {
            string indexName = ConfigurationManager.AppSettings["ElasticSearchBatchcodeIndexName"];

            var client = CreateElaticConfig();


            #region Delete All documents

            if (!client.Indices.Exists(indexName).Exists)
            {
                var delete = client.Indices.Delete(indexName);

                var indexDescriptor = new CreateIndexDescriptor(indexName)
                            .Map<BatchCodeElastic>(m => m.DateDetection(true).AutoMap());

                var response = client.Indices.Create(indexName,
                                    index => index.Map<BatchCodeElastic>(
                                        x => x.AutoMap()
                                    ));
            }
            #endregion
        }

        public void DeleteIndex()
        {
            string indexName = ConfigurationManager.AppSettings["ElasticSearchBatchcodeIndexName"];

            var client = CreateElaticConfig();


            #region Delete All documents

            var delete = client.Indices.Delete(indexName);

            var indexDescriptor = new CreateIndexDescriptor(indexName)
                        .Map<BatchCodeElastic>(m => m.DateDetection(true).AutoMap());

            var response = client.Indices.Create(indexName,
                                index => index.Map<BatchCodeElastic>(
                                    x => x.AutoMap()
                                ));
            #endregion
        }



        public bool Delete(string id)
        {
            ElasticClient client = CreateElaticConfig();
            var response = client.DeleteByQuery<BatchCodeElastic>(u => u
              .Query(q => q
                  .Term(f => f.idnew, id)
                  )

                  .Conflicts(Conflicts.Proceed)
                  .Refresh(true)
                   );

            return response.IsValid;

        }


        public string Insert(BatchCodeElastic batchCodeElastic)
        {
            batchCodeElastic.createddate = DateTime.Now;
            batchCodeElastic.idnew = GetHashValue(batchCodeElastic);

            ElasticClient client = CreateElaticConfig();
            var response = client.IndexDocument<BatchCodeElastic>(batchCodeElastic);

            if (response.IsValid)
            {
                client.Indices.Refresh(ConfigurationManager.AppSettings["ElasticSearchBatchcodeIndexName"]);
                return batchCodeElastic.idnew;
            }
            else
            {
                return null;
            }

        }


        private string GetHashValue(BatchCodeElastic subject)
        {
            string source = JsonConvert.SerializeObject(subject);
            using (MD5 md5Hash = MD5.Create())
            {
                string hash = HashHelper.GetMd5Hash(md5Hash, source);
                return hash;
            }
        }


        public bool IsAnyPendingDocExists(int itemMultiMRPId, int warehouseId)
        {
            ElasticClient client = CreateElaticConfig();
            //var response = client.DeleteByQuery<BatchCodeElastic>(u => u
            //  .Query(q => q
            //      .Term(f => f.id, id)
            //      )

            //      .Conflicts(Conflicts.Proceed)
            //      .Refresh(true)
            //       );

            var response = client.Search<BatchCodeElastic>(s => s
                            .From(0)
                            .Size(1)
                            .Query(q =>
                                q.Term(c => c.itemmultimrpid, itemMultiMRPId)
                                && q.Term(c => c.warehouseid, warehouseId)
                                ));

            return response.Total > 0 ? true : false;
        }

        public bool IsAnyPendingDocExists(List<int> itemMultiMRPIdList, int warehouseId)
        {
            ElasticClient client = CreateElaticConfig();
            //var response = client.DeleteByQuery<BatchCodeElastic>(u => u
            //  .Query(q => q
            //      .Term(f => f.id, id)
            //      )

            //      .Conflicts(Conflicts.Proceed)
            //      .Refresh(true)
            //       );

            var response = client.Search<BatchCodeElastic>(s => s
                            .From(0)
                            .Size(1)
                            .Query(q =>
                                q.Terms(c => c
                                    .Field(p => p.itemmultimrpid)
                                    .Terms<int>(itemMultiMRPIdList)
                                )
                                && q.Term(c => c.warehouseid, warehouseId)
                                ));

            return response.Total > 0 ? true : false;
        }

    }
}
