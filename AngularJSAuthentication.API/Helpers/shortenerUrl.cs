using Newtonsoft.Json;
using Nito.AsyncEx;
using System;
using System.Net.Http;
using System.Text;
using NLog;
using System.Net;
using System.Configuration;
using Kutt.NET;
using Kutt.NET.Links;
using AngularJSAuthentication.Common.Helpers;

namespace AngularJSAuthentication.API.Helpers
{
    public static class ShortenerUrl
    {
        public static Logger logger = LogManager.GetCurrentClassLogger();
        //public static string ShortenUrl(string longUrl)
        //{
        //    string api = @"https://hideuri.com/api/v1/shorten";
        //    try
        //    {
        //        using (var client = new HttpClient())
        //        {
        //            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;

        //            urljsondata data = new urljsondata { url = longUrl };
        //            var newJson = JsonConvert.SerializeObject(data);
        //            using (var content = new StringContent(newJson, Encoding.UTF8, "application/json"))
        //            {
        //                var response = AsyncContext.Run(() => client.PostAsync(api, content));
        //                if (!response.IsSuccessStatusCode)
        //                    return string.Empty;

        //                var responsestr = AsyncContext.Run(() => response.Content.ReadAsStringAsync());

        //                dynamic jsonResponse = JsonConvert.DeserializeObject<dynamic>(responsestr);
        //                return jsonResponse["result_url"];
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        logger.Error(ex.ToString());
        //    }

        //    return string.Empty;
        //}


        public static string ShortenUrl(string longUrl)
        {
            try
            {
                urljsondata result = new urljsondata();
                using (GenericRestHttpClient<string, string> memberClient = new GenericRestHttpClient<string,string>(ConfigurationManager.AppSettings["ShortenURL"], "", null))
                {
                    result = AsyncContext.Run(() => memberClient.PostAsync<urljsondata>(longUrl));
                }
                //// Initializes a Kutt instance with default server
                //KuttApi kutt = new KuttApi(ConfigurationManager.AppSettings["TinyURLKey"].ToString());

                //// Creates a shortened URL
                //Link link = AsyncContext.Run(() => kutt.CreateLinkAsync(longUrl));

                return result.url;
            }
            catch (Exception ex)
            {
                logger.Error(ex.ToString());
            }

            return string.Empty;
        }
    }

    public class urljsondata
    {
        public string url { get; set; }
    }
}