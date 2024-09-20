using Nito.AsyncEx;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace AngularJSAuthentication.BatchManager.Helpers
{
    /// <summary>
    /// This class exposes RESTful CRUD functionality in a generic way, abstracting
    /// the implementation and useage details of HttpClient, HttpRequestMessage,
    /// HttpResponseMessage, ObjectContent, Formatters etc. 
    /// </summary>
    /// <typeparam name="T">This is the Type of Resource you want to work with, such as Customer, Order etc.</typeparam>
    /// <typeparam name="TResourceIdentifier">This is the type of the identifier that uniquely identifies a specific resource such as Id or Username etc.</typeparam>
    public class GenericHttpClient<T, TResourceIdentifier> : IDisposable where T : class
    {
        private bool _disposed;
        private HttpClient _httpClient;

        private readonly string _serviceBaseAddress;
        private readonly string _addressSuffix;
        private const string JsonMediaType = "application/json";
        private const string XMLMediaType = "application/xml";
        private const string FormMediaType = "application/x-www-form-urlencoded;charset=UTF-8";
        private static object _lock = new object();
        private List<KeyValuePair<string, IEnumerable<string>>> _extraDataAsHeader;

        public string FullAddress { get { return _serviceBaseAddress + _addressSuffix; } }

        /// <summary>
        /// The constructor requires two parameters that essentially initialize the underlying HttpClient.
        /// In a RESTful service, you might have URLs of the following nature (for a given resource - Member in this example):<para />
        /// 1. http://www.somedomain/api/members/<para />
        /// 2. http://www.somedomain/api/members/jdoe<para />
        /// Where the first URL will GET you all members, and allow you to POST new members.<para />
        /// While the second URL supports PUT and DELETE operations on a specifc member.
        /// </summary>
        /// <param name="serviceBaseAddress">As per the example, this would be "http://www.somedomain"</param>
        /// <param name="addressSuffix">As per the example, this would be "api/members/"</param>
        /// <param name="extraDataAsHeader"></param>
        public GenericHttpClient(string serviceBaseAddress, string addressSuffix, List<KeyValuePair<string, IEnumerable<string>>> extraDataAsHeader = null)
        {
            if (extraDataAsHeader == null || !extraDataAsHeader.Any())
            {
                List<KeyValuePair<string, IEnumerable<string>>> newExtraDataAsHeader = new List<KeyValuePair<string, IEnumerable<string>>>();

                extraDataAsHeader = newExtraDataAsHeader;

            }
            _serviceBaseAddress = serviceBaseAddress;
            _addressSuffix = addressSuffix;
            _extraDataAsHeader = extraDataAsHeader.ToList();
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
            CreateClient();



        }

        private HttpRequestMessage GetRequestMessage(HttpMethod method, string url)
        {

            var msg = new HttpRequestMessage(method, url);

            if (_extraDataAsHeader != null)
            {
                foreach (var keyValuePair in _extraDataAsHeader)
                {
                    msg.Headers.Add(keyValuePair.Key, keyValuePair.Value.FirstOrDefault());
                }
            }

            //if (HttpContext.Current != null && HttpContext.Current.Session != null && HttpContext.Current.Session["VisitorIP"] != null)
            //{
            //    msg.Headers.Add("VisitorIP", HttpContext.Current.Session["VisitorIP"] as string);
            //    msg.Headers.Add("Browser", HttpContext.Current.Request.Browser.Browser);
            //}

            //StringBuilder sb = HttpContext.Current != null ? HttpContext.Current.Request.UrlReferrer != null
            //    ? new StringBuilder(HttpContext.Current.Request.UrlReferrer.AbsoluteUri).Append(" --> ").Append(HttpContext.Current.Request.Url.AbsoluteUri)
            //    : new StringBuilder(HttpContext.Current.Request.Url.AbsoluteUri) : new StringBuilder("");
            //if (HttpContext.Current != null)
            //    msg.Headers.Referrer = new Uri(sb.ToString());

            return msg;
        }


        private void CreateClient()
        {
            _httpClient = HttpClientService.Instance;
            try
            {

                ServicePointManager.FindServicePoint(new Uri(_serviceBaseAddress + _addressSuffix)).ConnectionLeaseTimeout = (int)TimeSpan.FromSeconds(15).TotalMilliseconds;
            }
            catch (Exception ex)
            {
                //LogHelper.LogError(ex.ToString(), true);
                //throw;
            }
            //}
        }

        public async Task<IEnumerable<T>> GetManyAsync()
        {
            var msg = GetRequestMessage(HttpMethod.Get, _serviceBaseAddress + "/" + _addressSuffix);


            var responseMessage = await _httpClient.SendAsync(msg);
            responseMessage.EnsureSuccessStatusCode();
            return await responseMessage.Content.ReadAsAsync<IEnumerable<T>>();
        }

        public async Task<T> GetAsync(TResourceIdentifier identifier)
        {

            var msg = GetRequestMessage(HttpMethod.Get, _serviceBaseAddress + "/" + _addressSuffix + identifier);

            var responseMessage = await _httpClient.SendAsync(msg);
            responseMessage.EnsureSuccessStatusCode();
            return await responseMessage.Content.ReadAsAsync<T>();
        }

        public async Task<T> GetAsync()
        {
            var msg = GetRequestMessage(HttpMethod.Get, _serviceBaseAddress + "/" + _addressSuffix);

            var responseMessage = await _httpClient.SendAsync(msg);
            responseMessage.EnsureSuccessStatusCode();
            return await responseMessage.Content.ReadAsAsync<T>();


            //var responseMessage = await _httpClient.GetAsync(_addressSuffix);
            //responseMessage.EnsureSuccessStatusCode();
            //return await responseMessage.Content.ReadAsAsync<T>();
        }


        //public async Task<T> GetAsync()
        //{
        //     var msg = GetRequestMessage(HttpMethod.Put, _serviceBaseAddress + (!string.IsNullOrEmpty(_addressSuffix) ? "/" + _addressSuffix : ""));

        //    var responseMessage = await _httpClient.SendAsync(msg);
        //    responseMessage.EnsureSuccessStatusCode();
        //    return await responseMessage.Content.ReadAsAsync<T>();
        //}


        public async Task<bool> GetBulkAsync()
        {
            var msg = GetRequestMessage(HttpMethod.Put, _serviceBaseAddress + (!string.IsNullOrEmpty(_addressSuffix) ? "/" + _addressSuffix : ""));

            //var responseMessage = await _httpClient.SendAsync(msg);
            var responseMessage = await _httpClient.SendAsync(msg);
            responseMessage.EnsureSuccessStatusCode();
            return true;
        }

        public async Task<T> PostAsync(T model)
        {
            var msg = GetRequestMessage(HttpMethod.Post, _serviceBaseAddress + "/" + _addressSuffix);


            var objectContent = CreateJsonObjectContent(model);
            msg.Content = objectContent;

            var responseMessage = await _httpClient.SendAsync(msg);
            responseMessage.EnsureSuccessStatusCode();
            return await responseMessage.Content.ReadAsAsync<T>();
        }


        public async Task<T> PutAsync(T model)
        {
            var msg = GetRequestMessage(HttpMethod.Put, _serviceBaseAddress + "/" + _addressSuffix);


            var objectContent = CreateJsonObjectContent(model);
            msg.Content = objectContent;

            var responseMessage = await _httpClient.SendAsync(msg);
            responseMessage.EnsureSuccessStatusCode();
            return await responseMessage.Content.ReadAsAsync<T>();
        }

        public async Task<Y> PutAsync<Y>(T model)
        {
            var msg = GetRequestMessage(HttpMethod.Put, _serviceBaseAddress + (!string.IsNullOrEmpty(_addressSuffix) ? "/" + _addressSuffix : ""));


            var objectContent = CreateJsonObjectContent(model);
            msg.Content = objectContent;

            var responseMessage = await _httpClient.SendAsync(msg);
            responseMessage.EnsureSuccessStatusCode();
            return await responseMessage.Content.ReadAsAsync<Y>();
        }

        public Y PutAsyncWithHandleError<Y>(T model, out string error)
        {
            error = "";
            var msg = GetRequestMessage(HttpMethod.Put, _serviceBaseAddress + (!string.IsNullOrEmpty(_addressSuffix) ? "/" + _addressSuffix : ""));


            var objectContent = CreateJsonObjectContent(model);
            msg.Content = objectContent;

            var responseMessage = AsyncContext.Run(() => _httpClient.SendAsync(msg));
            if (responseMessage.StatusCode == HttpStatusCode.OK)
            {
                responseMessage.EnsureSuccessStatusCode();
                return AsyncContext.Run(() => responseMessage.Content.ReadAsAsync<Y>());
            }
            else
            {
                error = AsyncContext.Run(() => responseMessage.Content.ReadAsStringAsync());
                return default(Y);
            }
        }
        public Y GetAsyncirn<Y>()
        {
            var msg = GetRequestMessage(HttpMethod.Get, _serviceBaseAddress + "/" + _addressSuffix);

            var responseMessage = AsyncContext.Run(() => _httpClient.SendAsync(msg));
            responseMessage.EnsureSuccessStatusCode();
            return AsyncContext.Run(() => responseMessage.Content.ReadAsAsync<Y>());


            //var responseMessage = await _httpClient.GetAsync(_addressSuffix);
            //responseMessage.EnsureSuccessStatusCode();
            //return await responseMessage.Content.ReadAsAsync<T>();
        }

        public async Task<Y> PostAsync<Y>(T model)
        {
            var msg = GetRequestMessage(HttpMethod.Post, _serviceBaseAddress + (!string.IsNullOrEmpty(_addressSuffix) ? "/" + _addressSuffix : ""));


            var objectContent = CreateJsonObjectContent(model);
            msg.Content = objectContent;

            var responseMessage = await _httpClient.SendAsync(msg);
            responseMessage.EnsureSuccessStatusCode();
            return await responseMessage.Content.ReadAsAsync<Y>();
        }

        public async Task<T> PostMultipartAsync(string fileName, byte[] imageData)
        {
            var msg = GetRequestMessage(HttpMethod.Post, _serviceBaseAddress + "/" + _addressSuffix);

            using (var requestContent = new MultipartFormDataContent("Upload----" + DateTime.Now.ToString(CultureInfo.InvariantCulture)))
            {
                HttpContent imageContent = new StreamContent(new MemoryStream(imageData));
                imageContent.Headers.ContentType = MediaTypeHeaderValue.Parse("image/jpeg");
                requestContent.Add(imageContent, "image", fileName);
                msg.Content = requestContent;
                var responseMessage = await _httpClient.SendAsync(msg);
                responseMessage.EnsureSuccessStatusCode();
                return await responseMessage.Content.ReadAsAsync<T>();
            }
        }

        public async Task<string> GetStringAsync()
        {
            var msg = GetRequestMessage(HttpMethod.Get, _serviceBaseAddress + "/" + _addressSuffix);


            var responseMessage = await _httpClient.SendAsync(msg);
            responseMessage.EnsureSuccessStatusCode();
            return await responseMessage.Content.ReadAsStringAsync();
        }

        public async Task<string> GetStringAsyncPost(T model)
        {
            var msg = GetRequestMessage(HttpMethod.Post, _serviceBaseAddress + "/" + _addressSuffix);


            var objectContent = CreateJsonObjectContent(model);
            msg.Content = objectContent;

            var responseMessage = await _httpClient.SendAsync(msg);
            responseMessage.EnsureSuccessStatusCode();
            return await responseMessage.Content.ReadAsStringAsync();

        }

        public async Task<byte[]> GetBytesAsyncPost(T model)
        {
            var msg = GetRequestMessage(HttpMethod.Post, _serviceBaseAddress + "/" + _addressSuffix);


            var objectContent = CreateJsonObjectContent(model);
            msg.Content = objectContent;

            var responseMessage = await _httpClient.SendAsync(msg);
            responseMessage.EnsureSuccessStatusCode();
            return await responseMessage.Content.ReadAsByteArrayAsync();

        }



        private HttpContent CreateJsonObjectContent(T model)
        {
            MediaTypeFormatter mediaTypeFormatter = new JsonMediaTypeFormatter();
            MediaTypeHeaderValue mediaTypeHeaderValue = MediaTypeHeaderValue.Parse(JsonMediaType);

            HttpContent content = new ObjectContent<T>(model, mediaTypeFormatter, mediaTypeHeaderValue);
            return content;
        }

        public string PostXMLAsync(string XML)
        {
            string _result = "";
            HttpWebResponse response = null;
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(_serviceBaseAddress);
                request.Method = "POST";
                request.ContentType = "text/xml";
                var writer = new StreamWriter(request.GetRequestStream());
                writer.Write(XML);
                writer.Close();

                response = (HttpWebResponse)request.GetResponse();
                var streamResponse = response.GetResponseStream();
                var streamRead = new StreamReader(streamResponse);
                _result = streamRead.ReadToEnd().Trim();
                streamRead.Close();
                streamResponse.Close();
                response.Close();
                return _result;
            }
            catch (Exception ex)
            {

            }
            return _result;
        }
        public string PostJsonAsync(string Json)
        {
            string _result = "";
            HttpWebResponse response = null;
            try
            {
                string GoogleAppID = "AIzaSyCPgKrihQKKfxMtCO9YaCR8nsUzYJgXBQ4";
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(_serviceBaseAddress);
                request.Method = "POST";
                request.ContentType = "application/json";
                request.Headers.Add(string.Format("Authorization: key={0}", GoogleAppID));

                Byte[] byteArray = Encoding.UTF8.GetBytes(Json);
                request.ContentLength = byteArray.Length;
                var writer = new StreamWriter(request.GetRequestStream());

                writer.Write(Json);

                writer.Close();

                response = (HttpWebResponse)request.GetResponse();
                var streamResponse = response.GetResponseStream();
                var streamRead = new StreamReader(streamResponse);
                _result = streamRead.ReadToEnd().Trim();
                streamRead.Close();
                streamResponse.Close();
                response.Close();
                return _result;
            }
            catch (Exception ex)
            {

            }
            return _result;
        }

        public string PostWebRequest()
        {
            string _result = "";

            try
            {
                var objRequest = (HttpWebRequest)WebRequest.Create(_addressSuffix);
                var objResponse = objRequest.GetResponse();
                var streamResponse = objResponse.GetResponseStream();
                var streamRead = new StreamReader(streamResponse);
                _result = streamRead.ReadToEnd().Trim();
                streamRead.Close();
                streamResponse.Close();

                return _result;
            }
            catch (Exception ex)
            {

            }
            return _result;
        }

        private HttpContent CreateXMLObjectContent(T model)
        {
            MediaTypeFormatter mediaTypeFormatter = new XmlMediaTypeFormatter();
            MediaTypeHeaderValue mediaTypeHeaderValue = MediaTypeHeaderValue.Parse(XMLMediaType);

            HttpContent content = new ObjectContent<T>(model, mediaTypeFormatter, mediaTypeHeaderValue);
            return content;
        }

        #region Notification Related

        public async Task<T> PostYcmNotificationAsync(T model)
        {
            var objectContent = CreateFormObjectContent(model);
            var responseMessage = await _httpClient.PostAsync(_addressSuffix, objectContent);
            return await responseMessage.Content.ReadAsAsync<T>();

        }

        private HttpContent CreateFormObjectContent(T model)
        {
            MediaTypeFormatter mediaTypeFormatter = new JsonMediaTypeFormatter();
            MediaTypeHeaderValue mediaTypeHeaderValue = MediaTypeHeaderValue.Parse(FormMediaType);

            HttpContent content = new ObjectContent<T>(model, mediaTypeFormatter, mediaTypeHeaderValue);
            return content;
        }


        public async Task<string> PostXMLAsync(string data, string url, string action)
        {

            string _result = "";
            try
            {
                string result = string.Empty;

                var byteData = Encoding.ASCII.GetBytes(data);

                HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(url);
                if (!String.IsNullOrEmpty(action))
                {
                    webRequest.Headers.Add("SOAPAction", action);
                }
                webRequest.ContentType = "text/xml;charset=\"utf-8\"";
                webRequest.Accept = "text/xml";
                webRequest.Method = "POST";
                webRequest.ContentLength = data.Length;

                using (var stream = webRequest.GetRequestStream())
                {
                    stream.Write(byteData, 0, byteData.Length);
                }

                using (WebResponse webResponse = await webRequest.GetResponseAsync())
                {
                    using (StreamReader rd = new StreamReader(webResponse.GetResponseStream()))
                    {
                        _result = rd.ReadToEnd();
                    }
                }

                return _result;
            }
            catch (Exception ex)
            {
                throw;
            }

        }


        #endregion Notification Related

        #region IDisposable Members
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (!_disposed && disposing)
            {
                _disposed = true;
            }
        }
        #endregion IDisposable Members
    }

    public class HttpClientService : HttpClient
    {
        public static string ServiceBaseAddress { get; set; }

        private readonly static Lazy<HttpClientService> _instance = new Lazy<HttpClientService>(
        () => new HttpClientService());

        static HttpClientService() { }

        private HttpClientService() : base()
        {
            Timeout = TimeSpan.FromSeconds(15);
            DefaultRequestHeaders.Add("Connection", "close");
            //DefaultRequestHeaders.AcceptEncoding.Add(StringWithQualityHeaderValue.Parse("gzip"));
            //DefaultRequestHeaders.AcceptEncoding.Add(StringWithQualityHeaderValue.Parse("deflate"));
            //DefaultRequestHeaders.AcceptEncoding.Add(StringWithQualityHeaderValue.Parse("sdch"));
            //DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue(new ProductHeaderValue("Matlus_HttpClient", "1.0")));
            DefaultRequestHeaders.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json"));
        }

        public static HttpClientService Instance
        {
            get
            {
                return _instance.Value;
            }

        }

    }
}
