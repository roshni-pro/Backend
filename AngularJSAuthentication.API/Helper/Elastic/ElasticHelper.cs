using AngularJSAuthentication.Common.Helpers;
using AngularJSAuthentication.DataContracts.ElasticSearch;
using AngularJSAuthentication.DataContracts.External.MobileExecutiveDC;
using Newtonsoft.Json;
using Nito.AsyncEx;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Web;

namespace AngularJSAuthentication.API.Helper.Elastic
{
    public class ElasticHelper
    {
        #region Elastic
        public List<OrderElasticDataDC> GetList(string query, int fetchSize = 1000)
        {
            List<OrderElasticDataDC> list = new List<OrderElasticDataDC>();
            QueryToPostElasticsearch q = new QueryToPostElasticsearch
            {
                fetch_size = fetchSize,
                query = query
            };
            ElasticQueryResult result = Query(q);
            TypeConverter<OrderElasticDataDC> typeConverter = new TypeConverter<OrderElasticDataDC>();
            List<OrderElasticDataDC> listPart = null;
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
                typeConverter = new TypeConverter<OrderElasticDataDC>();
                listPart = typeConverter.GetList(result.rows, baseResult.columns);
                if (listPart != null && listPart.Any())
                {
                    list.AddRange(listPart);
                }
            }
            return list;

        }

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
                    TextFileLogHelper.LogError(counter.ToString() + ex.ToString(), false);
                    //LogHelper.LogError($"Count is : {counter.ToString()} \t {ex.ToString()}");
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

                System.Net.Http.Formatting.MediaTypeFormatter mediaTypeFormatter = new System.Net.Http.Formatting.JsonMediaTypeFormatter();
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
        public static void AddHeaders(HttpClient client)
        {
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Add("Authorization", ConfigurationManager.AppSettings["elasticAuthorizationHeader"]);

        }

        public List<ExcelOrderElasticDataDC> GetOrderMasterList(string query, int fetchSize = 1000)
        {
            List<ExcelOrderElasticDataDC> list = new List<ExcelOrderElasticDataDC>();
            QueryToPostElasticsearch q = new QueryToPostElasticsearch
            {
                fetch_size = fetchSize,
                query = query
            };
            ElasticQueryResult result = Query(q);
            TypeConverter<ExcelOrderElasticDataDC> typeConverter = new TypeConverter<ExcelOrderElasticDataDC>();
            List<ExcelOrderElasticDataDC> listPart = null;
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
                typeConverter = new TypeConverter<ExcelOrderElasticDataDC>();
                listPart = typeConverter.GetList(result.rows, baseResult.columns);
                if (listPart != null && listPart.Any())
                {
                    list.AddRange(listPart);
                }
            }
            return list;

        }

        #endregion

    }
}