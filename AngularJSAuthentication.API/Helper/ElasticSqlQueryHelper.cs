using AngularJSAuthentication.Common.Helpers;
using AngularJSAuthentication.DataContracts.ElasticSearch;
using Newtonsoft.Json;
using Nito.AsyncEx;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace AngularJSAuthentication.API.Helper
{
    public class ElasticSqlQueryHelper<T>
    {
   

   
        public ElasticQueryResult Query(QueryToPostElasticsearch q, ElasticsearchNextPage nextPage = null)
        {
            int counter = 0;
            while (counter < 11)
            {
                counter++;
                try
                {
                    return QueryInner(q, nextPage);
                }
                catch (Exception ex)
                {

                    if (counter < 11)
                    {
                        Thread.Sleep(10 * 1000);
                        return QueryInner(q, nextPage);
                    }
                    else
                    {
                        throw ex;
                    }
                }
            }
            return null;
        }
        public ElasticQueryResult QueryInner(QueryToPostElasticsearch q, ElasticsearchNextPage nextPage = null)
        {
            using (var _client = new HttpClient())
            {
                AddHeaders(_client);

                string responsemsg = "";
                var BaseUrl = ConfigurationManager.AppSettings["ElasticSearchBaseUrl"].ToString() + "_sql?format=json";

                var msg = new HttpRequestMessage(System.Net.Http.HttpMethod.Post, BaseUrl);

                MediaTypeFormatter mediaTypeFormatter = new JsonMediaTypeFormatter();
                MediaTypeHeaderValue mediaTypeHeaderValue = MediaTypeHeaderValue.Parse("application/json");




                HttpContent content = null;
                if (q != null)
                {
                    content = new ObjectContent<object>(q, mediaTypeFormatter, mediaTypeHeaderValue);
                }
                else
                {
                    content = new ObjectContent<object>(nextPage, mediaTypeFormatter, mediaTypeHeaderValue);
                }
                msg.Content = content;

                var responseMessage = AsyncContext.Run(() => _client.SendAsync(msg));
                if (responseMessage.StatusCode == HttpStatusCode.OK)
                {
                    responseMessage.EnsureSuccessStatusCode();
                    responsemsg = AsyncContext.Run(() => responseMessage.Content.ReadAsStringAsync());

                }

                ElasticQueryResult elasticQueryResult = null;
                if (!string.IsNullOrEmpty(responsemsg))
                {
                    elasticQueryResult = JsonConvert.DeserializeObject<ElasticQueryResult>(responsemsg);
                }
                return elasticQueryResult;
            }

        }


        public List<T> GetList(string query, int fetchSize = 50000)
        {
            List<T> list = new List<T>();
            QueryToPostElasticsearch q = new QueryToPostElasticsearch
            {
                fetch_size = fetchSize,
                query = query
            };
            ElasticQueryResult result = Query(q);
            TypeConverter<T> typeConverter = new TypeConverter<T>();
            List<T> listPart = null;
            if (result != null)
            {
                listPart = typeConverter.GetList(result.rows, result.columns);
            }
            if (listPart != null && listPart.Any())
            {
                list.AddRange(listPart);
            }
            var baseResult = result;
            while (result != null && !string.IsNullOrEmpty(result.cursor))
            {
                result = Query(null, nextPage: new ElasticsearchNextPage { cursor = result.cursor });
                typeConverter = new TypeConverter<T>();
                listPart = typeConverter.GetList(result.rows, baseResult.columns);
                if (listPart != null && listPart.Any())
                {
                    list.AddRange(listPart);
                }
            }
            return list;

        }

        public static void AddHeaders(HttpClient client)
        {
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Add("Authorization", ConfigurationManager.AppSettings["elasticAuthorizationHeader"]);

        }


        public async Task<List<T>> GetListAsync(string query)
        {

            List<T> list = new List<T>();
            try
            {
                QueryToPostElasticsearch q = new QueryToPostElasticsearch
                {
                    fetch_size = 5000,
                    query = query
                };
                ElasticQueryResult result = await QueryAsync(q); // await QueryAsyncHttpClient(q);
                TypeConverter<T> typeConverter = new TypeConverter<T>();
                List<T> listPart = null;
                if (result != null)
                {
                    listPart = typeConverter.GetList(result.rows, result.columns);
                }
                if (listPart != null && listPart.Any())
                {
                    list.AddRange(listPart);
                }
                var baseResult = result;
                while (result != null && !string.IsNullOrEmpty(result.cursor))
                {
                    result = await QueryAsync(null, nextPage: new ElasticsearchNextPage { cursor = result.cursor });
                    typeConverter = new TypeConverter<T>();
                    listPart = typeConverter.GetList(result.rows, baseResult.columns);
                    if (listPart != null && listPart.Any())
                    {
                        list.AddRange(listPart);
                    }
                }

            }
            catch (Exception ex)
            {
                //LogHelper.LogError(ex.ToString());
                throw ex;
            }
            return list;
        }

        public async Task<ElasticQueryResult> QueryAsync(QueryToPostElasticsearch q, ElasticsearchNextPage nextPage = null)
        {
            int counter = 0;
            while (counter < 11)
            {
                counter++;
                try
                {
                    return await QueryInnerAsync(q, nextPage);
                }
                catch (Exception ex)
                {

                    //LogHelper.LogError($"Count is : {counter.ToString()} \t {ex.ToString()}");
                    if (counter < 11)
                    {
                        Thread.Sleep(10 * 1000);
                        return await QueryInnerAsync(q, nextPage);
                    }
                    else
                    {
                        throw ex;
                    }
                }
            }
            return null;
        }

        public async Task<ElasticQueryResult> QueryInnerAsync(QueryToPostElasticsearch q, ElasticsearchNextPage nextPage = null)
        {
            List<KeyValuePair<string, IEnumerable<string>>> extraDataAsHeader = new List<KeyValuePair<string, IEnumerable<string>>>();

            extraDataAsHeader.Add(new KeyValuePair<string, IEnumerable<string>>("Authorization", new List<string> { ConfigurationManager.AppSettings["elasticAuthorizationHeader"] }));

            ElasticQueryResult elasticQueryResult = new ElasticQueryResult();
            if (q != null)
            {
                using (var httpclient = new GenericRestHttpClient<QueryToPostElasticsearch, string>(ConfigurationManager.AppSettings["ElasticSearchBaseUrl"].ToString() + "_sql?format=json", "", extraDataAsHeader))
                {

                    elasticQueryResult = await httpclient.PostAsync<ElasticQueryResult>(q);

                }
            }
            else
            {
                using (var httpclient = new GenericRestHttpClient<ElasticsearchNextPage, string>(ConfigurationManager.AppSettings["ElasticSearchBaseUrl"].ToString() + "_sql?format=json", "", extraDataAsHeader))
                {

                    elasticQueryResult = await httpclient.PostAsync<ElasticQueryResult>(nextPage);

                }
            }

            return elasticQueryResult;

        }
    }
}