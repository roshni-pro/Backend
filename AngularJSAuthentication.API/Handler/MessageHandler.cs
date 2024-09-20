using AngularJSAuthentication.API.Helpers;
using AngularJSAuthentication.Common.Helpers;
using AngularJSAuthentication.DataContracts.ServiceRequestParam;
using Newtonsoft.Json;
using Nito.AspNetBackgroundTasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.ExceptionHandling;

namespace AngularJSAuthentication.API.Handler
{
    public abstract class MessageHandler : DelegatingHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            string IP = "unknown";
            string forwardedIps = "";
            string xForwardedHttpHeader = "";
            string browser = "unknown";
            var loggedInUser = HttpContext.Current != null && HttpContext.Current.User != null && HttpContext.Current.User.Identity != null ? HttpContext.Current.User.Identity.Name : "";
            try
            {
                IEnumerable<string> userNameheader = new List<string>();
                IEnumerable<string> browserValues = new List<string>();
                request.Headers.TryGetValues("username", out userNameheader);
                request.Headers.TryGetValues("Browser", out browserValues);

                IP = IPHelper.GetVisitorIPAddress(request, out forwardedIps, out xForwardedHttpHeader); /*headerValues != null && headerValues.Count() > 0 ? headerValues.FirstOrDefault() : ;*/
                browser = browserValues != null && browserValues.Count() > 0 ? browserValues.FirstOrDefault() : string.Empty;

                if ((string.IsNullOrEmpty(loggedInUser) || loggedInUser == "RetailerApp" || loggedInUser == "SalesApp" || loggedInUser == "DeliveryApp")
                     && userNameheader != null && userNameheader.Any())
                {
                    loggedInUser = userNameheader?.FirstOrDefault();
                }
            }
            catch (Exception ex) { }


            string otherHeaders = String.Empty;
            foreach (var key in HttpContext.Current.Request.Headers.AllKeys)
                otherHeaders += key + "=" + HttpContext.Current.Request.Headers[key] + Environment.NewLine;



            var corrId = string.Format("{0}{1}", DateTime.Now.Ticks, Thread.CurrentThread.ManagedThreadId);

            HttpContext.Current.Request.Headers.Add("CorelationId", corrId);


            //IEnumerable<string> bearerToken = new List<string>();
            //request.Headers.TryGetValues("Authorization", out bearerToken);

            //var bearerTokenObj = bearerToken != null && bearerToken.Any() ? Encoding.UTF8.GetString(TextEncodings.Base64Url.Decode(bearerToken.FirstOrDefault().Replace("Bearer ", ""))) : "";

            var referrer = request.Headers.Referrer == null ? "unknown" : request.Headers.Referrer.AbsoluteUri;
            var requestInfo = string.Format("{0}", request.RequestUri);


            string path = string.Format("{0}://{1}{2}/{3}", new Uri((HttpContext.Current.Request.UrlReferrer != null ? HttpContext.Current.Request.UrlReferrer.AbsoluteUri : HttpContext.Current.Request.Url.AbsoluteUri)).Scheme
                                                             , HttpContext.Current.Request.Url.DnsSafeHost
                                                             , (HttpContext.Current.Request.Url.Port != 80 && HttpContext.Current.Request.Url.Port != 443 ? ":" + HttpContext.Current.Request.Url.Port : "")
                                                             , "UploadedImages/");

            if (HttpContext.Current.Request.Url.AbsoluteUri.ToLower() == path.ToLower())
            {
                return new HttpResponseMessage(HttpStatusCode.NotFound);
            }

            if (HttpContext.Current.Request.Url.AbsoluteUri.ToLower().Contains(@"/external/") ||
                HttpContext.Current.Request.Url.AbsoluteUri.ToLower().Contains("token") ||
                HttpContext.Current.Request.Url.AbsoluteUri.ToLower().Contains(@"/error")
                || request.Headers.Contains("IsError") || HttpContext.Current.Request.Url.AbsoluteUri.ToLower().Contains("getvanresponse")
                || HttpContext.Current.Request.Url.AbsoluteUri.ToLower().Contains("icicicallbackres")
                || HttpContext.Current.Request.Url.AbsoluteUri.ToLower().Contains(@"api/htmltopdf")
                || (request.Content.Headers.ContentType != null && request.Content.Headers.ContentType.MediaType.Contains("multipart/form-data")))
            {

                return await base.SendAsync(request, cancellationToken);
            }
            else
            {

                var requestMessage = await request.Content.ReadAsByteArrayAsync();
                await IncommingMessageAsync(browser, IP, corrId, requestInfo, requestMessage, otherHeaders, referrer, request.Method.Method, loggedInUser, forwardedIps, xForwardedHttpHeader);

                var response = await base.SendAsync(request, cancellationToken); // change request body

                byte[] responseMessage;

                if (response.IsSuccessStatusCode && response.Content != null)
                    responseMessage = await response.Content.ReadAsByteArrayAsync();
                else
                    responseMessage = Encoding.UTF8.GetBytes(response.ReasonPhrase);

                await OutgoingMessageAsync(browser, IP, corrId, requestInfo, responseMessage, otherHeaders, referrer, request.Method.Method, loggedInUser, forwardedIps, xForwardedHttpHeader);

                if (HttpContext.Current.Request.Url.AbsoluteUri.ToLower().Contains(@"/external/") || 
                    HttpContext.Current.Request.Url.AbsoluteUri.ToLower().Contains("token")
               || HttpContext.Current.Request.Url.AbsoluteUri.ToLower().Contains(@"/error")
               || !HttpContext.Current.Request.Url.AbsoluteUri.ToLower().Contains(@"/api/")
               || HttpContext.Current.Request.Url.AbsoluteUri.ToLower().Contains(@"/api/appversion")
               || request.Headers.Contains("IsError")
              )
                    return response;

                return GenerateResponse(request, response, corrId);
            }
        }

        private HttpResponseMessage GenerateResponse(HttpRequestMessage request, HttpResponseMessage response, string corelationId)
        {
            HttpStatusCode statusCode = response.StatusCode;
            string doNotEncryptData = "";

            if (request.Headers.Contains("NoEncryption"))
            {
                doNotEncryptData = HttpContext.Current.Request.Headers.GetValues("NoEncryption").FirstOrDefault();
            }

            if (HttpContext.Current.Request.Url.AbsoluteUri.ToLower().Contains("getskcashcollectionbyusername") || HttpContext.Current.Request.Url.AbsoluteUri.ToLower().Contains("cashpaymentconfirmation"))
            {
                doNotEncryptData = "true";
            }


            object responseContent;
            bool isValidContent = response.TryGetContentValue(out responseContent);
            string errorMessage = response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.NoContent ? "" : responseContent.ToString();

            if (isValidContent)
            {
                if (responseContent is HttpError httpError)
                {

                    errorMessage = JsonConvert.SerializeObject(responseContent);
                    responseContent = null;
                }
            }

            AES256 aes = new AES256();
            string redisAesKey = DateTime.Now.ToString("yyyyMMdd") + "1201"; // "Sh0pK!r@n@#@!@#$";

            ResponseMetaData responseMetadata = new ResponseMetaData();
            responseMetadata.Status = (statusCode == HttpStatusCode.OK) || statusCode == HttpStatusCode.NoContent ? "OK" : "ERROR";
            responseMetadata.Data = statusCode == HttpStatusCode.OK ? string.IsNullOrEmpty(doNotEncryptData) ? aes.Encrypt(JsonConvert.SerializeObject(responseContent), redisAesKey) : responseContent : null;

            DateTime dt = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second, DateTime.Now.Millisecond);
            responseMetadata.Timestamp = dt;
            responseMetadata.ErrorMessage = errorMessage;
            var result = request.CreateResponse(statusCode == HttpStatusCode.OK || statusCode == HttpStatusCode.NoContent ? HttpStatusCode.OK : statusCode, responseMetadata);
            return result;
        }

        protected abstract Task IncommingMessageAsync(string browser, string IP, string correlationId, string requestInfo, byte[] message, string otherHeaders, string referrer, string method, string userName, string forwardedIps, string xForwardedHttpHeader);
        protected abstract Task OutgoingMessageAsync(string browser, string IP, string correlationId, string requestInfo, byte[] message, string otherHeaders, string referrer, string Method, string userName, string forwardedIps, string xForwardedHttpHeader);
    }



    public class MessageLoggingHandler : MessageHandler
    {
        protected override async Task IncommingMessageAsync(string browser, string IP, string correlationId, string requestInfo, byte[] message, string otherHeaders, string referrer, string Method, string userName, string forwardedIps, string xForwardedHttpHeader)
        {
            var now = DateTime.Now;
            var strMessage = Encoding.UTF8.GetString(message);
            BackgroundTaskManager.Run(async () =>
            {
                var res = await LogHelper.TraceLog(new TraceLog
                {
                    Browser = browser,
                    CoRelationId = correlationId,
                    Headers = otherHeaders.Replace(Environment.NewLine, "~~"),
                    ForwardedIps = forwardedIps,
                    IP = IP,
                    LogType = "Request",
                    Message = strMessage,
                    RequestInfo = requestInfo,
                    CreatedDate = now,
                    Method = Method,
                    Referrer = referrer,
                    UserName = userName,
                    xForwardedHttpHeader = xForwardedHttpHeader
                });
            });

        }


        protected override async Task OutgoingMessageAsync(string browser, string IP, string correlationId, string requestInfo, byte[] message, string otherHeaders, string referrer, string Method, string userName, string forwardedIps, string xForwardedHttpHeader)
        {
            var now = DateTime.Now;
            var strMessage = Encoding.UTF8.GetString(message);
            BackgroundTaskManager.Run(async () =>
            {
                var result = await LogHelper.TraceLog(new TraceLog
                {
                    Browser = browser,
                    CoRelationId = correlationId,
                    Headers = otherHeaders.Replace(Environment.NewLine, "~~"),
                    ForwardedIps = forwardedIps,
                    IP = IP,
                    LogType = "Response",
                    Message = strMessage,
                    RequestInfo = requestInfo,
                    CreatedDate = now,
                    Method = Method,
                    Referrer = referrer,
                    UserName = userName,
                    xForwardedHttpHeader = xForwardedHttpHeader
                });
            });

        }
    }


    public class TraceExceptionLogger : ExceptionLogger
    {
        public override void Log(ExceptionLoggerContext context)
        {
            var now = DateTime.Now;
            string forwardedIps = "";
            string xForwardedHttpHeader = "";
            string IP = IPHelper.GetVisitorIPAddress(null, out forwardedIps, out xForwardedHttpHeader);

            string corId = "";
            if (HttpContext.Current != null && HttpContext.Current.Request != null)
            {

                try
                {
                    corId = HttpContext.Current.Request.Headers.GetValues("CorelationId")?.FirstOrDefault();
                }
                catch { }
            }

            string error = context.ExceptionContext.Exception.InnerException != null ? context.ExceptionContext.Exception.ToString() + Environment.NewLine + context.ExceptionContext.Exception.InnerException.ToString() : context.ExceptionContext.Exception.ToString();
            BackgroundTaskManager.Run(async () =>
            {
                var result = await LogHelper.ErrorLog(new ErrorLog
                {
                    CoRelationId = corId,
                    IP = IP,
                    ForwardedIps = forwardedIps,
                    Message = error,
                    CreatedDate = now,
                    xForwardedHttpHeader = xForwardedHttpHeader
                });
            });
        }
    }


    public class CustomExceptionHandler : IExceptionHandler
    {
        public virtual Task HandleAsync(ExceptionHandlerContext context,
                                        CancellationToken cancellationToken)
        {
            return HandleAsyncCore(context, cancellationToken);
        }

        public virtual Task HandleAsyncCore(ExceptionHandlerContext context,
                                           CancellationToken cancellationToken)
        {
            HandleCore(context);
            return Task.FromResult(0);
        }

        public virtual void HandleCore(ExceptionHandlerContext context)
        {
            ResponseMetaData responseMetadata = new ResponseMetaData();
            responseMetadata.Status = "ERROR";
            responseMetadata.Data = null;
            DateTime dt = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second, DateTime.Now.Millisecond);
            responseMetadata.Timestamp = dt;
            responseMetadata.ErrorMessage = context.ExceptionContext.Exception.Message;
            context.Result = new TextPlainErrorResult
            {
                Request = context.ExceptionContext.Request,
                Content = responseMetadata
            };

            context.Request.Headers.Add("IsError", "Yes");
            MediaTypeFormatter mediaTypeFormatter = new JsonMediaTypeFormatter();
            context.Request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json"); //"application/json"
        }


        public virtual bool ShouldHandle(ExceptionHandlerContext context)
        {
            return true;
        }
    }


    public class TextPlainErrorResult : IHttpActionResult
    {
        public HttpRequestMessage Request { get; set; }
        public ResponseMetaData Content { get; set; }

        public Task<HttpResponseMessage> ExecuteAsync(CancellationToken cancellationToken)
        {
            HttpResponseMessage response =
                             new HttpResponseMessage(HttpStatusCode.OK);
            response.Content = new StringContent(JsonConvert.SerializeObject(Content));
            response.RequestMessage = Request;
            response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            return Task.FromResult(response);
        }
    }
}