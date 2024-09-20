using AngularJSAuthentication.API.Controllers.ScaleUp;
using AngularJSAuthentication.Common.Constants;
using AngularJSAuthentication.Common.Helpers;
using AngularJSAuthentication.DataContracts.ScaleUp;
using Newtonsoft.Json;
using NLog;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;
using static AngularJSAuthentication.DataContracts.ScaleUp.ScaleUpDc;

namespace AngularJSAuthentication.API.Managers
{
    public class ScaleUpManager
    {
        public static Logger logger = LogManager.GetCurrentClassLogger();

        public async Task<OrderInvoiceRes> OrderInvoice(string OrderNo, string InvoiceNo, double invoiceAmount, DateTime orderDate, string InvoicePdfURL, bool Status)
        {
            OrderInvoiceRes res = new OrderInvoiceRes();
            ScaleUpConfigDc scaleup = new ScaleUpConfigDc();
            using (var db = new AuthContext())
            {
                var ScaleUps = db.ScaleUpConfig.Where(x => x.IsActive == true && x.IsDeleted == false && x.ProductCode == "CreditLine").ToList();
                var ScaleUp = ScaleUps.FirstOrDefault(x => x.Name == "token");
                if (ScaleUp != null)
                {
                    scaleup = new ScaleUpConfigDc
                    {
                        ApiKey = ScaleUp.ApiKey,
                        ApiSecretKey = ScaleUp.ApiSecretKey,
                        ScaleUpUrl = ScaleUp.ScaleUpUrl,
                        ApiUrl = ScaleUp.ApiUrl
                    };
                    var ScaleUpToken = await GenerateTokenScaleUp(scaleup);

                    OrderInvoice invoice = new OrderInvoice
                    {
                        InvoiceDate = orderDate,
                        InvoiceNo = InvoiceNo,
                        InvoiceAmount = invoiceAmount,
                        OrderNo = OrderNo,
                        InvoicePdfURL = InvoicePdfURL,
                        StatusMsg = Status == true ? "Success" : "Failed"
                    };

                    var newJson = JsonConvert.SerializeObject(invoice);
                    if (ScaleUpToken != null)
                    {
                        using (var httpClient = new HttpClient())
                        {
                            using (var request = new HttpRequestMessage(new HttpMethod("POST"), scaleup.ScaleUpUrl + "/aggregator/LoanAccountAgg/UpdateInvoiceInformation"))
                            {
                                ServicePointManager.Expect100Continue = true;
                                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                                request.Headers.TryAddWithoutValidation("noencryption", "1");
                                request.Headers.TryAddWithoutValidation("Authorization", ScaleUpToken.token_type + " " + ScaleUpToken.access_token);

                                request.Content = new StringContent(newJson);
                                request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
                                var response = await httpClient.SendAsync(request);

                                if (response.StatusCode == HttpStatusCode.OK)
                                {
                                    string jsonString = (await response.Content.ReadAsStringAsync());
                                    var data = JsonConvert.DeserializeObject<OrderInvoiceRes>(jsonString);
                                    if (data != null)
                                    {
                                        res.Message = data.Result;
                                        res.status = data.status;

                                        if (!res.status)
                                        {
                                            EmailHelper.SendMail(AppConstants.MasterEmail, "ravikant.dhamne@shopkirana.com;sudeep.solanki@shopkirana.com;harry@shopkirana.com;anurag.shukla@shopkirana.com", "", ConfigurationManager.AppSettings["Environment"] + " --Scaleup Invoice Fail", newJson + " Error:" + res.Message, "");

                                        }
                                    }
                                }
                                else
                                {
                                    res.Message = "Bad Request";
                                    res.status = false;
                                }
                            }
                        }
                    }
                }
            }

            return res;
        }

        public async Task<OrderInvoiceRes> OrderCompleted(string TransactionId, bool Status)
        {
            TextFileLogHelper.TraceLog("scaleUp OrderCompleted call 2");

            OrderInvoiceRes res = new OrderInvoiceRes();
            ScaleUpConfigDc scaleup = new ScaleUpConfigDc();
            try
            {


                using (var db = new AuthContext())
                {
                    var ScaleUps = db.ScaleUpConfig.Where(x => x.IsActive == true && x.IsDeleted == false && x.ProductCode == "CreditLine").ToList();
                    var ScaleUp = ScaleUps.FirstOrDefault(x => x.Name == "token");
                    if (ScaleUp != null)
                    {
                        scaleup = new ScaleUpConfigDc
                        {
                            ApiKey = ScaleUp.ApiKey,
                            ApiSecretKey = ScaleUp.ApiSecretKey,
                            ScaleUpUrl = ScaleUp.ScaleUpUrl,
                            ApiUrl = ScaleUp.ApiUrl
                        };
                        var ScaleUpToken = await GenerateTokenScaleUp(scaleup);
                        if (ScaleUpToken != null)
                        {
                            using (var httpClient = new HttpClient())
                            {
                                // using (var request = new HttpRequestMessage(new HttpMethod("GET"), scaleup.ScaleUpUrl + "/services/loanaccount/v1/AnchorOrderCompleted?transactionNo=" + TransactionId +"&transStatus="+ Status ))
                                using (var request = new HttpRequestMessage(new HttpMethod("GET"), scaleup.ScaleUpUrl + "/aggregator/LoanAccountAgg/AnchorOrderCompleted?transactionNo=" + TransactionId + "&transStatus=" + Status))
                                {
                                    ServicePointManager.Expect100Continue = true;
                                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                                    request.Headers.TryAddWithoutValidation("noencryption", "1");
                                    request.Headers.TryAddWithoutValidation("Authorization", ScaleUpToken.token_type + " " + ScaleUpToken.access_token);

                                    //request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
                                    var response = await httpClient.SendAsync(request);
                                    if (response.StatusCode == HttpStatusCode.OK)
                                    {
                                        string jsonString = (await response.Content.ReadAsStringAsync());
                                        TextFileLogHelper.TraceLog("scaleUp OrderCompleted response " + jsonString);
                                        var data = JsonConvert.DeserializeObject<OrderInvoiceRes>(jsonString);
                                        if (data != null)
                                        {
                                            res.Message = data.Message;
                                            res.status = data.status;
                                        }
                                    }
                                    else
                                    {
                                        string jsonString = (await response.Content.ReadAsStringAsync());
                                        res.Message = "Bad Request";
                                        res.status = false;
                                        TextFileLogHelper.TraceLog("scaleUp OrderCompleted response " + jsonString);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error("Error during  Scalup payment complete : " + ex.ToString());
                res.Message = "Scalup service down.";
                res.status = false;
            }

            return res;
        }

        public async Task<ScaleUpTokenDc> GenerateTokenScaleUp(ScaleUpConfigDc scaleup)
        {
            ScaleUpTokenDc accessToken = new ScaleUpTokenDc();

            using (var httpClient = new HttpClient())
            {
                using (var request = new HttpRequestMessage(new HttpMethod("POST"), scaleup.ScaleUpUrl + "/" + scaleup.ApiUrl /*"/services/identity/v1/connect/token"*/))
                {
                    request.Headers.TryAddWithoutValidation("noencryption", "1");
                    var contentList = new List<string>();
                    contentList.Add($"grant_type={Uri.EscapeDataString("client_credentials")}");
                    contentList.Add($"client_Id={Uri.EscapeDataString(scaleup.ApiKey)}");
                    contentList.Add($"client_secret={Uri.EscapeDataString(scaleup.ApiSecretKey)}");
                    request.Content = new StringContent(string.Join("&", contentList));
                    request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/x-www-form-urlencoded");

                    ServicePointManager.Expect100Continue = true;
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                    var response = await httpClient.SendAsync(request);
                    if (System.Net.HttpStatusCode.OK == response.StatusCode)
                    {
                        string jsonString = (await response.Content.ReadAsStringAsync()).Replace("\\", "").Trim(new char[1] { '"' });
                        accessToken = JsonConvert.DeserializeObject<ScaleUpTokenDc>(jsonString);
                    }
                    else
                    {
                        accessToken = null;
                    }
                }
            }
            return accessToken;
        }


        #region scalepup transaction refund
        public async Task<RefundTransactionResponse> RefundTransactionJob(string orderNo, double refundAmount)
        {
            RefundTransactionResponse res = new RefundTransactionResponse();
            using (var db = new AuthContext())
            {
                var ScaleUps = db.ScaleUpConfig.Where(x => x.IsActive == true && x.IsDeleted == false).ToList();
                var ScaleUpconfig = ScaleUps.FirstOrDefault(x => x.Name == "token");

                if (ScaleUpconfig != null)
                {
                    var scaleupconfig = new ScaleUpConfigDc
                    {
                        ApiKey = ScaleUpconfig.ApiKey,
                        ApiSecretKey = ScaleUpconfig.ApiSecretKey,
                        ScaleUpUrl = ScaleUpconfig.ScaleUpUrl,
                        ApiUrl = ScaleUpconfig.ApiUrl
                    };
                    var ScaleUpToken = await GenerateTokenScaleUp(scaleupconfig);

                    if (ScaleUpToken != null && ScaleUpToken.access_token != null)
                    {
                        using (var httpClient = new HttpClient())
                        {
                            using (var request = new HttpRequestMessage(new HttpMethod("GET"), scaleupconfig.ScaleUpUrl + "/services/loanaccount/v1/RefundTransaction?orderNo=" + orderNo + "&refundAmount=" + refundAmount + ""))
                            {
                                ServicePointManager.Expect100Continue = true;
                                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                                request.Headers.TryAddWithoutValidation("noencryption", "1");
                                request.Headers.TryAddWithoutValidation("Authorization", ScaleUpToken.token_type + " " + ScaleUpToken.access_token);

                                var response = await httpClient.SendAsync(request);
                                if (response.StatusCode == HttpStatusCode.OK)
                                {
                                    string jsonString = (await response.Content.ReadAsStringAsync());
                                    var Response = JsonConvert.DeserializeObject<RefundTransactionResponse>(jsonString);
                                    if (Response != null)
                                    {
                                        res.status = Response.status;
                                        res.Message = Response.Message;
                                    }
                                }
                                else
                                {
                                    res.status = false;
                                    res.Message = "Bad Request";
                                }
                            }
                        }
                    }
                }
            }
            return res;
        }
        #endregion
    }
}