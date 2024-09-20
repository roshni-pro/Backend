using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Script.Serialization;
using AgileObjects.AgileMapper;
using AngularJSAuthentication.API.Helpers;
using AngularJSAuthentication.Model;
using MongoDB.Bson;
using Newtonsoft.Json;
using Nito.AsyncEx;
using RestSharp;
using AngularJSAuthentication.DataContracts.Mongo;
using AngularJSAuthentication.DataContracts.UdharCreditLending;
using AngularJSAuthentication.Common.Helpers;

namespace AngularJSAuthentication.API.ControllerV7
{
    [RoutePrefix("api/Udhar")]
    public class SkCreditLendingController : ApiController
    {
        [AllowAnonymous]
        [HttpGet]
        [Route("GenerateLead")]
        public async Task<CreditLendingResponseDc> GenerateLead(int CustomerId)
        {
            AngularJSAuthentication.Common.Helpers.TextFileLogHelper.TraceLog("GenerateLead start 0 : " + CustomerId);

            CreditLendingResponseDc Result = new CreditLendingResponseDc();
            UdharRepDc res = new UdharRepDc();
            MongoDbHelper<UdharRequestResponse> mongoDbHelper = new MongoDbHelper<UdharRequestResponse>();
            var customer = new CustomerDc();
            using (var context = new AuthContext())
            {
                var param = new SqlParameter("@Id", CustomerId);
                customer = context.Database.SqlQuery<CustomerDc>("Exec SpGetCustomerInfoById @Id", param).FirstOrDefault();
            }
            if (customer != null && customer.IsCreditLendingEnable && customer.CreditLendingApiKey != null && customer.CreditLendingApiKeySecret != null)
            {

                AngularJSAuthentication.Common.Helpers.TextFileLogHelper.TraceLog("GenerateLead start 010 : " + CustomerId);
                AngularJSAuthentication.Common.Helpers.TextFileLogHelper.TraceLog("GenerateLead start CustomerDC : " + customer);

                var CreditLendingToken = await GenerateTokenCreditLending(customer);
                AngularJSAuthentication.Common.Helpers.TextFileLogHelper.TraceLog("GenerateLead start after GenerateTokenCreditLending response : " + CreditLendingToken??"null token");

                if (CreditLendingToken != null)
                {
                    AngularJSAuthentication.Common.Helpers.TextFileLogHelper.TraceLog("GenerateLead start after GenerateTokenCreditLending response : " + CreditLendingToken);
                    AddLeadDc lead = new AddLeadDc();
                    if (customer.ShippingAddress == null && customer.BillingAddress != null)
                    {
                        lead.Address1 = customer.BillingAddress;
                    }
                    else if (customer.ShippingAddress == null && customer.BillingAddress == null)
                    {
                        Result.Msg = " please update your address first then apply";
                        Result.Result = false;
                        return Result;
                    }
                    else
                    {
                        lead.Address1 = customer.ShippingAddress;
                    }
                    lead.Address2 = customer.BillingAddress;
                    lead.MobileNo = customer.Mobile;
                    lead.CityName = customer.City;
                    lead.StateCode = customer.StateCode;
                    lead.Zip = Convert.ToInt32(customer.ZipCode);
                    lead.UniqueCode = customer.Skcode;
                    lead.FirmName = customer.ShopName;
                    lead.EmailId = customer.EmailId;
                    lead.Name = customer.Name;
                    lead.AvgMonthlyBuying = customer.AvgMonthlyBuying;
                    lead.VintageDays = customer.VintageDays;
                    lead.GSTNo = customer.GSTNo;
                    lead.CustomerType = customer.CustomerType;
                    lead.StoreLocationId = customer.StoreLocationId;
                    lead.StoreLocationName = customer.StoreLocationName;
                    lead.Limit = customer.Limit;
                    var newJson = JsonConvert.SerializeObject(lead);
                    AngularJSAuthentication.Common.Helpers.TextFileLogHelper.TraceLog("GenerateLead start 1 : " + newJson);
                    using (var httpClient = new HttpClient())
                    {
                        using (var request = new HttpRequestMessage(new HttpMethod("POST"), customer.CreditLendingBaseUrl + "/api/Lead/addlead"))
                        {
                            request.Headers.TryAddWithoutValidation("Accept", "*/*");
                            request.Headers.TryAddWithoutValidation("noencryption", "1");
                            request.Headers.TryAddWithoutValidation("Authorization", CreditLendingToken.token_type + " " + CreditLendingToken.access_token);
                            request.Content = new StringContent(newJson);
                            request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
                            //httpClient.DefaultRequestHeaders.ConnectionClose = true;

                            AngularJSAuthentication.Common.Helpers.TextFileLogHelper.TraceLog("GenerateLead start 2 : " + request);
                            var response = await httpClient.SendAsync(request);
                            AngularJSAuthentication.Common.Helpers.TextFileLogHelper.TraceLog("GenerateLead start 3 : " + response);

                            if (response.StatusCode == HttpStatusCode.OK)
                            {
                                AngularJSAuthentication.Common.Helpers.TextFileLogHelper.TraceLog("GenerateLead start Response true 4 : " + response);

                                string jsonString = (await response.Content.ReadAsStringAsync());
                                var ExtrResult = JsonConvert.DeserializeObject<CreditLendingResponseMasterDc>(jsonString);
                                Result = ExtrResult.Data;

                                if (Result != null && Result.Result && Result.Msg == null)
                                {
                                    res.IsSuccess = Result.Result;
                                    res.Message = Convert.ToString(Result.Data); //URL  only
                                }
                                else
                                {
                                    res.IsSuccess = false;
                                    res.Message = Result.Msg;
                                }
                                UdharRequestResponse UdharRequestResponsedc = new UdharRequestResponse
                                {
                                    CustomerId = CustomerId,
                                    Type = request.Method.ToString(),
                                    Url = customer.CreditLendingBaseUrl + "/api/Lead/addlead",
                                    RequestResponseMsg = res.Message,
                                    Header = request.Headers.ToString(),
                                    CreatedDate = DateTime.Now,
                                };
                                mongoDbHelper.Insert(UdharRequestResponsedc);
                            }
                            else
                            {
                                res.IsSuccess = false;
                                res.Message = "Failed";
                            }
                        }
                    }
                }
                else
                {
                    res.IsSuccess = false;
                    res.Message = "Authentication Failed";
                }
            }
            else
            {
                res.IsSuccess = false;
                res.Message = "You are not Eligible";
            }
            Result.Msg = res.Message;
            Result.Result = res.IsSuccess;
            AngularJSAuthentication.Common.Helpers.TextFileLogHelper.TraceLog("main result : " + Result);

            return Result;
        }

        #region get UdharCreditLimit and  post Order payment
        [AllowAnonymous]
        [HttpGet]
        [Route("GetUdharCreditLimit")]
        public async Task<UdharRepDc> GetUdharCreditLimit(int CustomerId)
        {
            UdharRepDc result = new UdharRepDc();
            MongoDbHelper<UdharRequestResponse> mongoDbHelper = new MongoDbHelper<UdharRequestResponse>();
            var customer = new CustomerDc();
            using (var context = new AuthContext())
            {
                var param = new SqlParameter("@Id", CustomerId);
                customer = context.Database.SqlQuery<CustomerDc>("Exec GetCustomerInfoById @Id", param).FirstOrDefault();
                if (customer == null)
                    TextFileLogHelper.TraceLog($"Customer null 123: Before" + customer);
            }

            TextFileLogHelper.TraceLog($"Customer data  Before");
            TextFileLogHelper.TraceLog($"Customer data {customer.CreditLendingApiKeySecret}: Before");

            if (customer != null && customer != null && customer.IsCreditLendingEnable && customer.CreditLendingApiKey != null && customer.CreditLendingApiKeySecret != null)
            {
                TextFileLogHelper.TraceLog($"Customer 010");
                var CreditLendingToken = await GenerateTokenCreditLending(customer);
                TextFileLogHelper.TraceLog($"Customer 020 New Response" + CreditLendingToken);
                if (CreditLendingToken != null)
                {
                    TextFileLogHelper.TraceLog($"CreditLendingToken {CreditLendingToken}: ");
                    UdharCreditLendingDc User = new UdharCreditLendingDc();
                    User.UniqueCode = customer.Skcode;
                    var newJson = JsonConvert.SerializeObject(User);
                    TextFileLogHelper.TraceLog($"newJson {newJson}: Before request");
                    using (var httpClient = new HttpClient())
                    {
                        TextFileLogHelper.TraceLog("Before API Hit 01");
                        using (var request = new HttpRequestMessage(new HttpMethod("POST"), customer.CreditLendingBaseUrl + "/api/Account/GetAccountCreditLimit"))
                        {
                            request.Headers.TryAddWithoutValidation("Accept", "*/*");
                            request.Headers.TryAddWithoutValidation("noencryption", "1");
                            request.Headers.TryAddWithoutValidation("Authorization", CreditLendingToken.token_type + " " + CreditLendingToken.access_token);
                            request.Content = new StringContent(newJson);
                            request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
                            //System.Net.ServicePointManager.ServerCertificateValidationCallback = (senderX, certificate, chain, sslPolicyErrors) => { return true; };
                            TextFileLogHelper.TraceLog("Before API Hit 02");
                            var response = await httpClient.SendAsync(request);
                            TextFileLogHelper.TraceLog("Before API Hit 03");
                            if (response.StatusCode == HttpStatusCode.OK)
                            {
                                TextFileLogHelper.TraceLog("Before API Hit 04");
                                string jsonString = (await response.Content.ReadAsStringAsync());

                                var Extractres = JsonConvert.DeserializeObject<UdharCreditLimitResponseMasterDc>(jsonString);
                                var res = Extractres.Data;
                                if (res != null)
                                {
                                    if (Extractres.Data.ShowHideLimit == true)
                                    {
                                        result.IsSuccess = false;
                                        result.Message = "Your Account has been permanently closed";
                                        result.DynamicData = res;
                                    }
                                    else
                                    {
                                        result.DynamicData = res;
                                        result.IsSuccess = true;
                                    }

                                }
                                else
                                {
                                    result.IsSuccess = false;
                                    result.Message = "Something went wrong, please try after sometime";
                                }
                                UdharRequestResponse UdharRequestResponsedc = new UdharRequestResponse
                                {
                                    CustomerId = CustomerId,
                                    Type = request.Method.ToString(),
                                    Url = customer.CreditLendingBaseUrl + "/api/Account/GetAccountCredit",
                                    RequestResponseMsg = result.Message,
                                    Header = request.Headers.ToString(),
                                    CreatedDate = DateTime.Now,
                                };
                                mongoDbHelper.Insert(UdharRequestResponsedc);
                                TextFileLogHelper.TraceLog($"UdharRequestResponsedc {UdharRequestResponsedc}: ");
                            }
                            else
                            {

                                TextFileLogHelper.TraceLog($" HttpStatusCode.OK:not ok ");
                                result.IsSuccess = false;
                                result.Message = "Something went wrong, please try after sometime";
                            }
                        }
                    }
                }
                else
                {
                    TextFileLogHelper.TraceLog($"Authentication Failed: ");
                    result.IsSuccess = false;
                    result.Message = "Authentication Failed";
                }
            }
            else
            {
                TextFileLogHelper.TraceLog($"You are not Eligible : ");
                result.IsSuccess = false;
                result.Message = "You are not Eligible";
            }
            return result;
        }
        [AllowAnonymous]
        [HttpPost]
        [Route("GeneratePayment")]
        public async Task<UdharRepDc> GeneratePaymentRequest(PostUdharCLAppDc PostUdharCreditLimit)
        {
            UdharRepDc result = new UdharRepDc();
            MongoDbHelper<UdharRequestResponse> mongoDbHelper = new MongoDbHelper<UdharRequestResponse>();
            var customer = new CustomerDc();
            using (var context = new AuthContext())
            {
                var param = new SqlParameter("@Id", PostUdharCreditLimit.CustomerId);
                customer = context.Database.SqlQuery<CustomerDc>("Exec GetCustomerInfoById @Id", param).FirstOrDefault();
            }
            if (customer != null && customer != null && customer.IsCreditLendingEnable && customer.CreditLendingApiKey != null && customer.CreditLendingApiKeySecret != null)
            {
                var CreditLendingToken = await GenerateTokenCreditLending(customer);
                if (CreditLendingToken != null)
                {
                    PostUdharCreditLimitDc UseUdharCreditLimit = new PostUdharCreditLimitDc();
                    UseUdharCreditLimit.AccountId = PostUdharCreditLimit.AccountId;
                    UseUdharCreditLimit.OrderId = PostUdharCreditLimit.OrderId;
                    UseUdharCreditLimit.UniqueCode = customer.Skcode;
                    UseUdharCreditLimit.Amount = PostUdharCreditLimit.Amount;
                    UseUdharCreditLimit.PaymentMode = PostUdharCreditLimit.PaymentMode;
                    var newJson = JsonConvert.SerializeObject(UseUdharCreditLimit);
                    using (var httpClient = new HttpClient())
                    {
                        using (var request = new HttpRequestMessage(new HttpMethod("POST"), customer.CreditLendingBaseUrl + "/api/PaymentRequest/Generate"))
                        {
                            request.Headers.TryAddWithoutValidation("Accept", "*/*");
                            request.Headers.TryAddWithoutValidation("noencryption", "1");
                            request.Headers.TryAddWithoutValidation("Authorization", CreditLendingToken.token_type + " " + CreditLendingToken.access_token);
                            request.Content = new StringContent(newJson);
                            request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
                            //System.Net.ServicePointManager.ServerCertificateValidationCallback = (senderX, certificate, chain, sslPolicyErrors) => { return true; };

                            var response = await httpClient.SendAsync(request);
                            if (response.StatusCode == HttpStatusCode.OK)
                            {
                                string jsonString = (await response.Content.ReadAsStringAsync());
                                var Extractres = JsonConvert.DeserializeObject<CreditLendingResponseMasterDc>(jsonString);
                                var res = Extractres.Data;
                                if (res != null && res.Result && res.Msg == null)
                                {
                                    result.Message = Convert.ToString(res.Data); ;
                                    result.IsSuccess = true;
                                }
                                else
                                {
                                    result.IsSuccess = res.Result;
                                    result.Message = res.Msg;
                                }
                                UdharRequestResponse UdharRequestResponsedc = new UdharRequestResponse
                                {
                                    CustomerId = PostUdharCreditLimit.CustomerId,
                                    Type = request.Method.ToString(),
                                    Url = customer.CreditLendingBaseUrl + "/api/PaymentRequest/Generate",
                                    RequestResponseMsg = result.Message,
                                    Header = request.Headers.ToString(),
                                    CreatedDate = DateTime.Now,
                                };
                                mongoDbHelper.Insert(UdharRequestResponsedc);
                            }
                            else
                            {
                                result.IsSuccess = false;
                                result.Message = "Something went wrong, please try after sometime";
                            }
                        }
                    }
                }
                else
                {
                    result.IsSuccess = false;
                    result.Message = "current request is unauthorized for payment";
                }
            }
            else
            {
                result.IsSuccess = false;
                result.Message = "You are not Eligible";
            }
            return result;
        }

        #endregion



        [AllowAnonymous]
        [HttpGet]
        [Route("UdharOverDueDayRestrication")]
        public async Task<ReturnDataDC> UdharOverDueDayRestrication(int CustomerId, string AppType, string lang)
        {

            CheckDueAmtDc Data = new CheckDueAmtDc();
            ReturnDataDC res = new ReturnDataDC();

            MongoDbHelper<UdharOverDueDayValidation> UdharOverDueDay = new MongoDbHelper<UdharOverDueDayValidation>();
            var DueAmt = UdharOverDueDay.GetAll();
            var minDay = 0;
            var maxDay = 0;
            var SalesMinDay = 0;
            using (var context = new AuthContext())
            {
                if (CustomerId > 0 && DueAmt != null && DueAmt.Any(x => x.MinOverDueDay > 0) && DueAmt.Any(x => x.MaxOverDueDay > 0))
                {
                    minDay = DueAmt.Min(x => x.MinOverDueDay);
                    maxDay = DueAmt.Max(x => x.MaxOverDueDay);
                    SalesMinDay = DueAmt.Select(x => x.SalesMinDay).FirstOrDefault();

                    var param1 = new SqlParameter("@CustomerId", CustomerId);
                    Data = context.Database.SqlQuery<CheckDueAmtDc>("Exec CheckDueAmt @CustomerId ", param1).FirstOrDefault();

                    if (Data != null && Data.Amount >= 1 && lang == "en")
                    {
                        if (AppType == "SalesApp")
                        {

                            if (Data.OverDueDays <= minDay && Data.OverDueDays > (minDay - SalesMinDay))
                            {
                                res.Msg = "Please request the customer to clear the Direct Udhaar overdue amount of Rs. " + Data.Amount + "";
                                res.IsOrder = true;
                            }
                            else if (Data.OverDueDays > minDay)
                            {
                                res.Msg = "Please request the customer to clear the Direct Udhaar overdue amount of Rs. " + Data.Amount + " to continue placing a new order.";
                                res.IsOrder = false;
                            }
                            else
                            {
                                res.Msg = "";
                                res.IsOrder = true;
                            }
                        }
                        else //RETAILER APP
                        {

                            if (Data.OverDueDays > minDay && Data.OverDueDays < maxDay)
                            {
                                res.Msg = "Kindly clear your Direct Udhaar overdue amount of Rs." + Data.Amount + " to enable Direct Udhaar and COD mode of payment.";
                                res.IsOrder = true;
                            }
                            else
                            {
                                if (Data.OverDueDays > maxDay)
                                {
                                    res.Msg = "Kindly clear your Direct Udhaar overdue amount of Rs." + Data.Amount + " before placing the order.";
                                    res.IsOrder = false;
                                }
                                else
                                {
                                    res.Msg = "";
                                    res.IsOrder = true;
                                }
                            }
                        }

                    }
                    else if (Data != null && Data.Amount >= 1 && lang == "hi")
                    {
                        if (AppType == "SalesApp")
                        {

                            if (Data.OverDueDays <= minDay && Data.OverDueDays > (minDay - SalesMinDay))
                            {
                                res.Msg = "कृपया ग्राहक से " + Data.Amount + " रुपये की डायरेक्ट उधार राशि का भुगतान करने का अनुरोध करें।.";
                                res.IsOrder = true;
                            }
                            else if (Data.OverDueDays > minDay)
                            {
                                res.Msg = "कृपया ग्राहक से " + Data.Amount + " रुपये की डायरेक्ट उधार राशि का भुगतान करने का अनुरोध करें। नया ऑर्डर जारी रखने के लिए.";
                                res.IsOrder = false;
                            }
                            else
                            {
                                res.Msg = "";
                                res.IsOrder = true;
                            }
                        }
                        else //RETAILER APP
                        {

                            if (Data.OverDueDays > minDay && Data.OverDueDays < maxDay)
                            {

                                res.Msg = "उधार और सीओडी(COD) मोड को जारी रखने के लिए कृपया अपनी डायरेक्ट  उधार राशि रु. " + Data.Amount + " का भुगतान करें। ";
                                res.IsOrder = true;
                            }
                            else
                            {
                                if (Data.OverDueDays > maxDay)
                                {
                                    res.Msg = "ऑर्डर देने से पहले कृपया अपनी डायरेक्ट  उधार राशि रु. " + Data.Amount + " का भुगतान करें।";
                                    res.IsOrder = false;
                                }
                                else
                                {
                                    res.Msg = "";
                                    res.IsOrder = true;
                                }
                            }
                        }

                    }
                    else
                    {
                        res.Msg = "";
                        res.IsOrder = true;
                    }
                }
                else
                {
                    res.Msg = "Something Went Wrong";
                    res.IsOrder = false;
                }
                return res;
            }

        }
        public async Task<CreditLendingTokenDc> GenerateTokenCreditLending(CustomerDc companyDetails)
        {
            CreditLendingTokenDc accessToken = new CreditLendingTokenDc();
            AngularJSAuthentication.Common.Helpers.TextFileLogHelper.TraceLog("GenerateLead start 020 : " + companyDetails);
            using (var httpClient = new HttpClient())
            {
                AngularJSAuthentication.Common.Helpers.TextFileLogHelper.TraceLog("GenerateLead start 030 : " + companyDetails.CreditLendingBaseUrl);
                AngularJSAuthentication.Common.Helpers.TextFileLogHelper.TraceLog("GenerateLead start 030 : " + companyDetails.CreditLendingApiKey);
                using (var request = new HttpRequestMessage(new HttpMethod("POST"), companyDetails.CreditLendingBaseUrl + "/token"))
                {
                    try
                    {
                        AngularJSAuthentication.Common.Helpers.TextFileLogHelper.TraceLog("GenerateLead start 030 : " + companyDetails.CreditLendingBaseUrl);

                        request.Headers.TryAddWithoutValidation("noencryption", "1");
                        AngularJSAuthentication.Common.Helpers.TextFileLogHelper.TraceLog("GenerateLead start 040 : ");

                        var contentList = new List<string>();
                        AngularJSAuthentication.Common.Helpers.TextFileLogHelper.TraceLog("GenerateLead start 050 : ");

                        contentList.Add($"grant_type={Uri.EscapeDataString("client_credentials")}");
                        AngularJSAuthentication.Common.Helpers.TextFileLogHelper.TraceLog("GenerateLead start 060 : ");

                        contentList.Add($"client_Id={Uri.EscapeDataString(companyDetails.CreditLendingApiKey)}");
                        AngularJSAuthentication.Common.Helpers.TextFileLogHelper.TraceLog("GenerateLead start 070 : " + companyDetails.CreditLendingApiKey);

                        contentList.Add($"client_secret={Uri.EscapeDataString(companyDetails.CreditLendingApiKeySecret)}");

                        AngularJSAuthentication.Common.Helpers.TextFileLogHelper.TraceLog("GenerateLead start 080 : " + 0);

                        request.Content = new StringContent(string.Join("&", contentList));
                        request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/x-www-form-urlencoded");
                        AngularJSAuthentication.Common.Helpers.TextFileLogHelper.TraceLog("GenerateLead start 090 Request : " + request);

                        var response = await httpClient.SendAsync(request);
                        AngularJSAuthentication.Common.Helpers.TextFileLogHelper.TraceLog("GenerateLead start 090 Response : " + response);

                        if (System.Net.HttpStatusCode.OK == response.StatusCode)
                        {
                            AngularJSAuthentication.Common.Helpers.TextFileLogHelper.TraceLog("GenerateLead start 0910 : " + 0);

                            // accessToken = JsonConvert.DeserializeObject<CreditLendingTokenDc>(response.Content);
                            string jsonString = (await response.Content.ReadAsStringAsync()).Replace("\\", "").Trim(new char[1] { '"' });
                            accessToken = JsonConvert.DeserializeObject<CreditLendingTokenDc>(jsonString);
                        }
                        else
                        {
                            AngularJSAuthentication.Common.Helpers.TextFileLogHelper.TraceLog("GenerateLead start 9010 : " + 0);

                            accessToken = null;
                        }
                    }
                    catch (Exception ex)
                    {
                        AngularJSAuthentication.Common.Helpers.TextFileLogHelper.TraceLog("GenerateLead Token Not genereated Error: " + ex.Message);
                        throw ex;
                    }
                }
            }
            return accessToken;
        }


    }
}

public class UdharRepDc
{
    public object DynamicData { set; get; }
    public bool IsSuccess { set; get; }
    public string Message { set; get; }
    public string RequestMsg { set; get; }
}

public class CreditLendingResponseDc
{
    public object Data { get; set; }
    public string Msg { get; set; }
    public bool Result { get; set; }
    public object DynamicData { get; set; }
}
public class AddLeadDc
{
    public string MobileNo { get; set; }
    public string UniqueCode { get; set; } //skcode
    public string Address1 { get; set; }
    public string Address2 { get; set; }
    public string EmailId { get; set; }
    public int? Zip { get; set; }
    public string StateCode { get; set; }
    public string CityName { get; set; }
    public string Name { get; set; }
    public string FirmName { get; set; }
    public int VintageDays { get; set; }
    public double AvgMonthlyBuying { get; set; }
    public string GSTNo { get; set; }
    public string CustomerType { get; set; }
    public int StoreLocationId { get; set; }
    public string StoreLocationName { get; set; }
    public double Limit { get; set; } //// Limit set from Chqbook, EpayLater from sk
}
public class CustomerDc
{
    public string Mobile { get; set; }
    public string EmailId { get; set; }
    public string Skcode { get; set; } //skcode
    public string ShippingAddress { get; set; }
    public string BillingAddress { get; set; }
    public string ZipCode { get; set; }
    public string StateCode { get; set; }
    public string City { get; set; }
    public string Name { get; set; }
    public string ShopName { get; set; }
    public bool IsCreditLendingEnable { get; set; }
    public string CreditLendingApiKey { get; set; }
    public string CreditLendingApiKeySecret { get; set; }
    public string CreditLendingBaseUrl { get; set; }
    public int VintageDays { get; set; }
    public double AvgMonthlyBuying { get; set; }
    public string GSTNo { get; set; }
    public string CustomerType { get; set; }
    public int StoreLocationId { get; set; }
    public string StoreLocationName { get; set; }
    public double Limit { get; set; } //// Limit set from Chqbook, EpayLater from sk
}
public class CreditLendingTokenDc
{
    public string access_token { get; set; }
    public string token_type { get; set; }
    public int expires_in { get; set; }

    [JsonProperty(".issued")]
    public string Issued { get; set; }

    [JsonProperty(".expires")]
    public string Expires { get; set; }
}

//public class CompanyDetailsDC
//{
//    public bool IsCreditLendingEnable { get; set; }
//    public string CreditLendingApiKey { get; set; }
//    public string CreditLendingApiKeySecret { get; set; }
//    public string CreditLendingBaseUrl { get; set; }
//}
public class CreditLendingResponseMasterDc
{
    public string Status { get; set; }
    public string ErrorMessage { get; set; }
    public CreditLendingResponseDc Data { get; set; }
    public DateTime Timestamp { get; set; }
}
public class CheckDueAmtDc
{
    public double Amount { get; set; }
    public int OverDueDays { get; set; }
}
public class ReturnDataDC
{
    public string Msg { get; set; }
    public Boolean IsOrder { get; set; }
    public double UdharAmount { get; set; }

}