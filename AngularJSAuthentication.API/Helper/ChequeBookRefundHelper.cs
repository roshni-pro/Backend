using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Newtonsoft.Json;
using RestSharp;

namespace AngularJSAuthentication.API.Helper
{
    public class ChequeBookRefundHelper
    {
        public async Task<string> chequebookcancle(int GatewayTransId, int OrderId)
        {
            try
            {
                //using (var httpClient = new HttpClient())
                {
                    string BaseURL = "https://uat.chqbook.com";
                    string url = BaseURL + "/api/cl/pg/tx/cancel-approved-tx";
                    var client = new RestClient(url);
                    client.Timeout = -1;
                    var request = new RestRequest(Method.POST);
                    request.AddHeader("API-KEY", "f1f5rfe2-c6a6-79ea-66d0-07162bc115783");
                    request.AddHeader("Content-Type", "application/x-www-form-urlencoded");
                    request.AddHeader("Cookie", "lvl_session=WV0DZrnO7UpWIhzxG5mbn4S1TCCGVFTOJXSb5J6d");
                    request.AddParameter("accountProvider", "SHOP_KIRANA");
                    request.AddParameter("transactionId", GatewayTransId);
                    request.AddParameter("partnerTxRef", Convert.ToString(OrderId));
                    IRestResponse response = client.Execute(request);
                    Console.WriteLine(response.Content);

                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        return null;
                    }
                    else
                    {
                        return null;
                    }
                }
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
    }
}


public class cheqbookDc
{
    public int transactionId { get; set; }
    public string partnerTxRef { get; set; }
}