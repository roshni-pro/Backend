using AngularJSAuthentication.API.Managers;
using AngularJSAuthentication.Common.Constants;
using AngularJSAuthentication.Common.Helpers;
using AngularJSAuthentication.DataContracts.ScaleUp;
using GenricEcommers.Models;
using Newtonsoft.Json;
using Nito.AsyncEx;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Http;
using static AngularJSAuthentication.DataContracts.ScaleUp.ScaleUpDc;

namespace AngularJSAuthentication.API.Controllers.ScaleUp
{
    [RoutePrefix("api/ScaleUpIntegration")]
    public class ScaleUpIntegrationController : ApiController
    {
        public async Task<InitiateLeadDetail> GetCustomerInvoiceOfLastSixMonth(CustomerInfoDc customer)
        {
            List<CustomerInvoiceData> list = new List<CustomerInvoiceData>();
            List<BuyingHistories> data = new List<BuyingHistories>();
            InitiateLeadDetail result = new InitiateLeadDetail();

            if (customer != null)
            {
                DateTime todaydate = DateTime.Today;
                DateTime EndDate = DateTime.Today;
                DateTime startDate = EndDate.AddMonths(-6);

                string eDate = EndDate.ToString("yyyy-MM-dd");
                string sDate = startDate.ToString("yyyy-MM-dd");
                var platformIdxName = "skorderdata_" + AppConstants.Environment;
                //string query = $" select custid, count(distinct orderid) TotalMonthInvoice,sum(grossamount) InvoiceAmount ,min(createddate) Startdate from {platformIdxName} where custid={customer.customerid} and createddate >='{sDate}' and createddate <='{eDate}' group by custid,month(createddate) ";

                string query = $" select custid,orderid, (grossamount) InvoiceAmount ,min(createddate) Startdate from {platformIdxName} where custid={customer.customerid} and createddate >='{sDate}' and createddate <='{eDate}' and status not in ('Delivery Canceled','Order Canceled','Post Order Canceled')  group by custid,month(createddate),orderid,grossamount ";


                ElasticSqlHelper<CustomerInvoiceData> elasticSqlHelperData = new ElasticSqlHelper<CustomerInvoiceData>();
                list = AsyncContext.Run(async () => (await elasticSqlHelperData.GetListAsync(query)).ToList());

                int Vintday = Convert.ToInt32((todaydate.Date - customer.CreateDate.Date).TotalDays);

                result.VintageDays = Vintday < 0 ? 0 : Vintday;
                result.CustomerReferenceNo = customer.Skcode;
                result.MobileNumber = customer.MobileNo;
                result.BuyingHistories = new List<BuyingHistories>();
                if (list.Count > 0 && list.Any())
                {
                    data = list
                      .GroupBy(x => new { x.Startdate.Month, x.Startdate.Year })
                      .Select(x => new BuyingHistories
                      {
                          MonthFirstBuyingDate = new DateTime(x.Key.Year, x.Key.Month, 1),
                          TotalMonthInvoice = x.Count(),
                          MonthTotalAmount = Convert.ToInt32(x.Sum(y => y.InvoiceAmount))
                      }).ToList();

                    result.BuyingHistories.AddRange(data);
                }
                else
                {
                    BuyingHistories obj = new BuyingHistories();
                    obj.MonthFirstBuyingDate = DateTime.Now;
                    obj.TotalMonthInvoice = 0;
                    obj.MonthTotalAmount = 0;
                    data.Add(obj);
                    result.BuyingHistories.AddRange(data);
                }
            }
            return result;
        }

        [HttpGet]
        [Route("LeadInitiate")]
        public async Task<ScaleUpResponse> LeadInitiate(int customerId, string ProductType = "CreditLine")
        {
            TextFileLogHelper.TraceLog("Lead Mobile in- ");

            LeadRequestPost obj = new LeadRequestPost();
            long companyId = 0;
            long ProductId = 0;
            long? LeadId = null;
            ScaleUpResponse res = new ScaleUpResponse();
            InitiateLeadDetail data = new InitiateLeadDetail();
            ScaleUpConfigDc scaleup = new ScaleUpConfigDc();
            using (var db = new AuthContext())
            {
                var customer = db.Customers.FirstOrDefault(x => x.CustomerId == customerId && !x.Deleted);
                var ScaleUps = db.ScaleUpConfig.Where(x => x.IsActive == true && x.IsDeleted == false && x.ProductType == ProductType).ToList();
                var ScaleUp = ScaleUps.FirstOrDefault(x => x.Name == "token");
                if (ScaleUp != null)
                {
                    obj.companyCode = ScaleUp.CompanyCode;
                    obj.ProductCode = ScaleUp.ProductCode;
                    var scaleupCust = db.ScaleUpCustomers.FirstOrDefault(x => x.CustomerId == customer.CustomerId && x.ProductCode == obj.ProductCode);
                    if (customer != null && ScaleUp != null && scaleupCust == null)
                    {
                        obj.Mobile = customer.Mobile;
                        obj.CustomerReferenceNo = customer.Skcode;
                        scaleup = new ScaleUpConfigDc
                        {
                            ApiKey = ScaleUp.ApiKey,
                            ApiSecretKey = ScaleUp.ApiSecretKey,
                            ScaleUpUrl = ScaleUp.ScaleUpUrl,
                            ApiUrl = ScaleUp.ApiUrl
                        };
                        TextFileLogHelper.TraceLog("Lead Mobile - " + obj.Mobile);
                        var ScaleUpToken = await GenerateTokenScaleUp(scaleup);
                        if (ScaleUpToken != null)
                        {
                            TextFileLogHelper.TraceLog("Lead Token - " + ScaleUpToken.access_token);
                            using (var httpClient = new HttpClient())
                            {
                                using (var request = new HttpRequestMessage(new HttpMethod("GET"), scaleup.ScaleUpUrl + "/services/lead/v1/LeadExistsForCustomer?companyCode=" + obj.companyCode + "&productCode=" + obj.ProductCode + "&Mobile=" + obj.Mobile + "&customerReferenceNo=" + obj.CustomerReferenceNo + ""))
                                {
                                    ServicePointManager.Expect100Continue = true;
                                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                                    request.Headers.TryAddWithoutValidation("noencryption", "1");
                                    request.Headers.TryAddWithoutValidation("Authorization", ScaleUpToken.token_type + " " + ScaleUpToken.access_token);

                                    var response = await httpClient.SendAsync(request);
                                    if (response.StatusCode == HttpStatusCode.OK)
                                    {
                                        string jsonString = (await response.Content.ReadAsStringAsync());
                                        LeadId = JsonConvert.DeserializeObject<long>(jsonString);
                                        TextFileLogHelper.TraceLog("Check Lead Exists - " + LeadId);

                                        if (LeadId != null && LeadId == 0)
                                        {

                                            CustomerInfoDc cust = new CustomerInfoDc
                                            {
                                                customerid = customer.CustomerId,
                                                MobileNo = customer.Mobile,
                                                Skcode = customer.Skcode,
                                                CreateDate = customer.CreatedDate
                                            };

                                            data = await GetCustomerInvoiceOfLastSixMonth(cust);

                                            InitiateLeadDetail lead = new InitiateLeadDetail
                                            {
                                                AnchorCompanyCode = obj.companyCode,
                                                ProductCode = obj.ProductCode,
                                                MobileNumber = customer.Mobile,
                                                CustomerReferenceNo = customer.Skcode,
                                                VintageDays = data.VintageDays,
                                                Email = string.IsNullOrEmpty(customer.Emailid) ? "" : customer.Emailid,
                                                City = customer.City,
                                                State = customer.State,
                                                BuyingHistories = data.BuyingHistories.ToList()
                                            };

                                            var newJson = JsonConvert.SerializeObject(lead);

                                            using (var LeadInitiate = new HttpRequestMessage(new HttpMethod("POST"), scaleup.ScaleUpUrl + "/aggregator/LeadAgg/LeadInitiate"))
                                            {
                                                LeadInitiate.Headers.TryAddWithoutValidation("Accept", "*/*");
                                                LeadInitiate.Headers.TryAddWithoutValidation("noencryption", "1");
                                                LeadInitiate.Content = new StringContent(newJson);
                                                LeadInitiate.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
                                                LeadInitiate.Headers.TryAddWithoutValidation("Authorization", ScaleUpToken.token_type + " " + ScaleUpToken.access_token);

                                                var Leadresponse = await httpClient.SendAsync(LeadInitiate);
                                                if (Leadresponse.StatusCode == HttpStatusCode.OK)
                                                {
                                                    string LeadjsonString = (await Leadresponse.Content.ReadAsStringAsync());
                                                    var result = JsonConvert.DeserializeObject<ScalupLeadInitiateResponse>(LeadjsonString);
                                                    TextFileLogHelper.TraceLog("Lead Initiate - " + LeadjsonString);
                                                    if (result.Status)
                                                    {
                                                        LeadId = result.Response.LeadId;
                                                        companyId = result.Response.CompanyId;
                                                        ProductId = result.Response.ProductId;
                                                        res.message = "Success";
                                                        res.status = true;
                                                    }
                                                    else
                                                    {
                                                        res.message = "Failed";
                                                        res.status = false;
                                                    }
                                                }
                                                else
                                                {
                                                    string LeadjsonString = (await response.Content.ReadAsStringAsync());
                                                    TextFileLogHelper.TraceLog("Lead Initiate - " + LeadjsonString);
                                                    res.message = "Failed";
                                                    res.status = false;
                                                }
                                            }
                                        }
                                        else
                                        {
                                            res.status = true;
                                        }
                                    }
                                    else
                                    {
                                        res.message = "Failed";
                                        res.status = false;
                                    }
                                }
                            }
                        }
                        else
                        {
                            res.message = "Authentication Failed";
                            res.status = false;
                        }

                        if (res.status)
                        {
                            scaleupCust = new Model.ScaleUp.ScaleUpCustomer
                            {
                                CustomerId = customer.CustomerId,
                                ProductCode = obj.ProductCode,
                                AnchorCompanyCode = obj.companyCode,
                                CreatedDate = DateTime.Now,
                                LeadId = LeadId.Value,
                                IsActive = true,
                                IsDeleted = false,
                                CreatedBy = 1
                            };
                            db.ScaleUpCustomers.Add(scaleupCust);
                            if (db.SaveChanges() > 0)
                            {
                                var ScaleUpWebView = ScaleUps.FirstOrDefault(x => x.Name == "WebView");
                                res.status = true;
                                res.MobileNo = customer.Mobile;
                                res.Product = scaleupCust.ProductCode;
                                res.Company = scaleupCust.AnchorCompanyCode;
                                res.BaiseUrl = ScaleUp.ScaleUpUrl;
                                res.response = ScaleUpWebView.ScaleUpUrl.Replace("[mobileno]", customer.Mobile).Replace("[companyCode]", scaleupCust.AnchorCompanyCode).Replace("[productCode]", scaleupCust.ProductCode);
                            }
                            else
                            {
                                res.message = "Failed";
                                res.status = false;
                            }
                        }
                    }
                    else
                    {
                        var ScaleUpWebView = ScaleUps.FirstOrDefault(x => x.Name == "WebView");
                        res.status = true;
                        res.MobileNo = customer.Mobile;
                        res.Product = scaleupCust.ProductCode;
                        res.Company = scaleupCust.AnchorCompanyCode;
                        res.BaiseUrl = ScaleUp.ScaleUpUrl;
                        res.response = ScaleUpWebView.ScaleUpUrl.Replace("[mobileno]", customer.Mobile).Replace("[companyCode]", scaleupCust.AnchorCompanyCode.ToString()).Replace("[productCode]", scaleupCust.ProductCode.ToString());
                    }
                }
                else
                {
                    res.status = false;
                    res.message = "ScaleUp Configuration not found.";
                }
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

        [HttpGet]
        [Route("GetScaleUpLimit")]
        public async Task<LoanCreditLimit> GetScaleUpLimit(int customerId)
        {
            LoanCreditLimit loanCreditLimit = new LoanCreditLimit();
            using (var db = new AuthContext())
            {
                var ScaleUps = db.ScaleUpConfig.Where(x => x.IsActive == true && x.IsDeleted == false && x.ProductCode == "CreditLine").ToList();
                var ScaleUpconfig = ScaleUps.FirstOrDefault(x => x.Name == "token");
                var customer = db.ScaleUpCustomers.Where(x => x.CustomerId == customerId && x.IsActive && x.ProductCode == "CreditLine").FirstOrDefault();
                if (ScaleUpconfig != null && customer != null && customer.AccountId.HasValue)
                {
                    var scaleupconfig = new ScaleUpConfigDc
                    {
                        ApiKey = ScaleUpconfig.ApiKey,
                        ApiSecretKey = ScaleUpconfig.ApiSecretKey,
                        ScaleUpUrl = ScaleUpconfig.ScaleUpUrl,
                        ApiUrl = ScaleUpconfig.ApiUrl
                    };
                    var ScaleUpToken = await GenerateTokenScaleUp(scaleupconfig);
                    if (ScaleUpToken != null)
                    {
                        using (var httpClient = new HttpClient())
                        {
                            using (var request = new HttpRequestMessage(new HttpMethod("GET"), ScaleUpconfig.ScaleUpUrl + "/aggregator/LoanAccountAgg/GetAvailableCreditLimitByLeadId?LoanAccountId=" + customer.AccountId))
                            {
                                ServicePointManager.Expect100Continue = true;
                                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                                request.Headers.TryAddWithoutValidation("noencryption", "1");
                                request.Headers.TryAddWithoutValidation("Authorization", ScaleUpToken.token_type + " " + ScaleUpToken.access_token);

                                var response = await httpClient.SendAsync(request);
                                if (response.StatusCode == HttpStatusCode.OK)
                                {
                                    string jsonString = (await response.Content.ReadAsStringAsync());
                                    var limitResponse = JsonConvert.DeserializeObject<ScalupLimitResponse>(jsonString);
                                    if (limitResponse != null)
                                    {
                                        loanCreditLimit = limitResponse.response;
                                        loanCreditLimit.Message = limitResponse.message;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return loanCreditLimit;
        }

        [HttpGet]
        [Route("ScaleUpPaymentInitiate")]
        public async Task<OrderResponse> ScaleUpPaymentInitiate(int customerId, int OrderId, double TransactionAmount)
        {
            OrderResponse orderResponse = new OrderResponse();
            string webViewUrl = "";
            using (var db = new AuthContext())
            {
                ScaleUpConfigDc scaleupDc = new ScaleUpConfigDc();
                var scaleup = db.ScaleUpConfig.Where(x => x.Name == "token" && x.IsActive == true && x.IsDeleted == false && x.ProductCode == "CreditLine").FirstOrDefault();
                var customer = db.ScaleUpCustomers.Where(x => x.CustomerId == customerId && x.IsActive && x.ProductCode == "CreditLine").FirstOrDefault();
                if (scaleup != null && customer != null)
                {
                    var OrderAmount = db.DbOrderMaster.FirstOrDefault(x => x.OrderId == OrderId).GrossAmount;
                    PaymentRequestdc paymentRequestdc = new PaymentRequestdc
                    {
                        AnchorCompanyCode = customer.AnchorCompanyCode,
                        LoanAccountId = customer.AccountId.Value,
                        OrderNo = OrderId.ToString(),
                        TransactionAmount = Math.Round(TransactionAmount, 2),
                        OrderAmount = OrderAmount
                    };
                    scaleupDc = new ScaleUpConfigDc
                    {
                        ApiKey = scaleup.ApiKey,
                        ApiSecretKey = scaleup.ApiSecretKey,
                        ScaleUpUrl = scaleup.ScaleUpUrl,
                        ApiUrl = scaleup.ApiUrl
                    };
                    var ScaleUpToken = await GenerateTokenScaleUp(scaleupDc);
                    var newJson = JsonConvert.SerializeObject(paymentRequestdc);
                    using (var httpClient = new HttpClient())
                    {
                        using (var request = new HttpRequestMessage(new HttpMethod("POST"), scaleup.ScaleUpUrl + "/aggregator/LoanAccountAgg/OrderInitiate"))
                        {
                            ServicePointManager.Expect100Continue = true;
                            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                            request.Headers.TryAddWithoutValidation("noencryption", "1");
                            request.Headers.TryAddWithoutValidation("Authorization", ScaleUpToken.token_type + " " + ScaleUpToken.access_token);

                            // request.Headers.TryAddWithoutValidation("Authorization", ScaleUpToken.token_type + " " + ScaleUpToken.access_token);
                            request.Content = new StringContent(newJson);
                            request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
                            var response = await httpClient.SendAsync(request);
                            if (response.StatusCode == HttpStatusCode.OK)
                            {
                                string jsonString = (await response.Content.ReadAsStringAsync());
                                var orderInitiateResponse = JsonConvert.DeserializeObject<ScalupOrderInitiateResponse>(jsonString);
                                if (orderInitiateResponse != null)
                                {
                                    if (orderInitiateResponse.status)
                                    {
                                        webViewUrl = orderInitiateResponse.response;
                                        orderResponse.WebViewUrl = webViewUrl;
                                        orderResponse.TransactionId = orderInitiateResponse.message;
                                        orderResponse.BaiseUrl = scaleup.ScaleUpUrl;
                                        orderResponse.status = true;
                                    }
                                    else
                                    {
                                        orderResponse.status = false;
                                        orderResponse.message = orderInitiateResponse.message;
                                    }
                                }
                                else
                                {
                                    orderResponse.status = false;
                                    orderResponse.message = "Scalup services down please try after some time.";
                                }
                            }
                            else
                            {
                                orderResponse.status = false;
                                orderResponse.message = "Scalup services down please try after some time.";
                            }
                        }
                    }
                }
            }
            return orderResponse;
        }

        [HttpPost]
        [Route("UpdateCustomerAccount")]
        public async Task<bool> UpdateCustomerAccount(AccountDisbursementNotify accountDisbursementNotify)
        {
            bool result = false;
            using (var db = new AuthContext())
            {
                var customer = db.Customers.Where(x => x.Skcode == accountDisbursementNotify.CustomerUniqueCode).FirstOrDefault();
                if (customer != null)
                {
                    var scaleupCust = db.ScaleUpCustomers.FirstOrDefault(x => x.CustomerId == customer.CustomerId && x.LeadId == accountDisbursementNotify.LeadId);
                    if (scaleupCust != null)
                    {
                        scaleupCust.AccountId = accountDisbursementNotify.AccountId;
                        db.Entry(scaleupCust).State = System.Data.Entity.EntityState.Modified;
                        result = db.SaveChanges() > 0;
                    }
                }
            }
            return result;
        }


        [HttpPost]
        [Route("NotifyAnchorOrderCanceled")]
        [AllowAnonymous]
        public async Task<bool> NotifyAnchorOrderCanceled(List<NotifyAnchorOrderCanceled> notifyAnchorOrderCanceled)
        {
            bool result = false;
            if (notifyAnchorOrderCanceled != null && notifyAnchorOrderCanceled.Any())
            {
                List<string> strOrderIds = notifyAnchorOrderCanceled.Select(x => x.OrderNo).Distinct().ToList();
                List<int> OrderIds = strOrderIds.Select(int.Parse).ToList();
                var TransactionNos = notifyAnchorOrderCanceled.Select(x => x.TransactionNo).ToList();
                using (var db = new AuthContext())
                {
                    var paymentlist = await db.PaymentResponseRetailerAppDb.Where(x => OrderIds.Contains(x.OrderId) && x.status == "Success" && (x.PaymentFrom == "ScaleUp" || x.PaymentFrom == "Cash")).ToListAsync();
                    if (paymentlist != null && paymentlist.Any())
                    {
                        var Onlinepaymentlist = paymentlist.Where(x => OrderIds.Contains(x.OrderId) && x.status == "Success" && x.PaymentFrom == "ScaleUp" && TransactionNos.Contains(x.GatewayTransId)).ToList();

                        if (Onlinepaymentlist.Any() && Onlinepaymentlist != null)
                        {
                            OrderIds.ForEach(OrderId =>
                            {
                                if (Onlinepaymentlist.Any(y => y.OrderId == OrderId))
                                {
                                    var orderData = notifyAnchorOrderCanceled.Where(y => y.OrderNo == Convert.ToString(OrderId)).ToList();
                                    var OldorderPaymentlist = Onlinepaymentlist.Where(y => y.OrderId == OrderId).ToList();
                                    var Oldcashpayment = paymentlist.FirstOrDefault(a => a.OrderId == OrderId && a.PaymentFrom == "Cash");
                                    if (OldorderPaymentlist.Any() && OldorderPaymentlist != null && orderData.Any())
                                    {
                                        OldorderPaymentlist.ForEach(i =>
                                        {
                                            i.status = "Failed";
                                            i.statusCode = orderData.FirstOrDefault().Comment;
                                            db.Entry(i).State = EntityState.Modified;
                                        });
                                        if (Oldcashpayment == null)
                                        {
                                            PaymentResponseRetailerApp NewPaymant = new PaymentResponseRetailerApp
                                            {
                                                amount = orderData.Sum(z => z.amount),
                                                CreatedDate = DateTime.Now,
                                                currencyCode = "INR",
                                                OrderId = OrderId,
                                                PaymentFrom = "Cash",
                                                status = "Success",
                                                //statusDesc = orderData.FirstOrDefault().Comment,
                                                UpdatedDate = DateTime.Now,
                                                IsRefund = false
                                            };
                                            db.PaymentResponseRetailerAppDb.Add(NewPaymant);
                                        }
                                        else
                                        {
                                            Oldcashpayment.amount += orderData.Sum(z => z.amount);
                                            Oldcashpayment.UpdatedDate = DateTime.Now;
                                            db.Entry(Oldcashpayment).State = EntityState.Modified;
                                        }
                                    }
                                }
                            });
                            result = db.Commit() > 0;
                        }
                    }
                }
            }
            return result;
        }

        [HttpGet]
        [Route("UpdateBuyingHistory")]
        public async Task<ScaleUpResponse> UpdateBuyingHistory(string MobileNo, string ProductType = "CreditLine")
        {
            TextFileLogHelper.TraceLog("Lead Mobile in- ");

            LeadRequestPost obj = new LeadRequestPost();
            long companyId = 0;
            long ProductId = 0;
            long? LeadId = null;
            ScaleUpResponse res = new ScaleUpResponse();
            InitiateLeadDetail data = new InitiateLeadDetail();
            ScaleUpConfigDc scaleup = new ScaleUpConfigDc();
            using (var db = new AuthContext())
            {
                var customer = db.Customers.FirstOrDefault(x => x.Mobile == MobileNo && !x.Deleted);
                var ScaleUps = db.ScaleUpConfig.Where(x => x.IsActive == true && x.IsDeleted == false && x.ProductType == ProductType).ToList();
                var ScaleUp = ScaleUps.FirstOrDefault(x => x.Name == "token");
                if (ScaleUp != null)
                {
                    obj.companyCode = ScaleUp.CompanyCode;
                    obj.ProductCode = ScaleUp.ProductCode;
                    var scaleupCust = db.ScaleUpCustomers.FirstOrDefault(x => x.CustomerId == customer.CustomerId && x.ProductCode == obj.ProductCode);
                    if (customer != null && ScaleUp != null && scaleupCust != null)
                    {
                        obj.Mobile = customer.Mobile;
                        obj.CustomerReferenceNo = customer.Skcode;
                        scaleup = new ScaleUpConfigDc
                        {
                            ApiKey = ScaleUp.ApiKey,
                            ApiSecretKey = ScaleUp.ApiSecretKey,
                            ScaleUpUrl = ScaleUp.ScaleUpUrl,
                            ApiUrl = ScaleUp.ApiUrl
                        };
                        TextFileLogHelper.TraceLog("Lead Mobile - " + obj.Mobile);
                        var ScaleUpToken = await GenerateTokenScaleUp(scaleup);
                        if (ScaleUpToken != null)
                        {
                            TextFileLogHelper.TraceLog("Lead Token - " + ScaleUpToken.access_token);
                            using (var httpClient = new HttpClient())
                            {
                                using (var request = new HttpRequestMessage(new HttpMethod("GET"), scaleup.ScaleUpUrl + "/services/lead/v1/LeadExistsForCustomer?companyCode=" + obj.companyCode + "&productCode=" + obj.ProductCode + "&Mobile=" + obj.Mobile + "&customerReferenceNo=" + obj.CustomerReferenceNo + ""))
                                {
                                    ServicePointManager.Expect100Continue = true;
                                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                                    request.Headers.TryAddWithoutValidation("noencryption", "1");
                                    request.Headers.TryAddWithoutValidation("Authorization", ScaleUpToken.token_type + " " + ScaleUpToken.access_token);

                                    var response = await httpClient.SendAsync(request);
                                    if (response.StatusCode == HttpStatusCode.OK)
                                    {
                                        string jsonString = (await response.Content.ReadAsStringAsync());
                                        LeadId = JsonConvert.DeserializeObject<long>(jsonString);
                                        TextFileLogHelper.TraceLog("Check Lead Exists - " + LeadId);

                                        if (LeadId != null && LeadId != 0)
                                        {

                                            CustomerInfoDc cust = new CustomerInfoDc
                                            {
                                                customerid = customer.CustomerId,
                                                MobileNo = customer.Mobile,
                                                Skcode = customer.Skcode,
                                                CreateDate = customer.CreatedDate
                                            };

                                            data = await GetCustomerInvoiceOfLastSixMonth(cust);

                                            InitiateLeadDetail lead = new InitiateLeadDetail
                                            {
                                                AnchorCompanyCode = obj.companyCode,
                                                ProductCode = obj.ProductCode,
                                                MobileNumber = customer.Mobile,
                                                CustomerReferenceNo = customer.Skcode,
                                                VintageDays = data.VintageDays,
                                                Email = string.IsNullOrEmpty(customer.Emailid) ? "" : customer.Emailid,
                                                City = customer.City,
                                                State = customer.State,
                                                BuyingHistories = data.BuyingHistories.ToList()
                                            };

                                            var newJson = JsonConvert.SerializeObject(lead);

                                            using (var LeadInitiate = new HttpRequestMessage(new HttpMethod("POST"), scaleup.ScaleUpUrl + "/aggregator/LeadAgg/UpdateBuyingHistory"))
                                            {
                                                LeadInitiate.Headers.TryAddWithoutValidation("Accept", "*/*");
                                                LeadInitiate.Headers.TryAddWithoutValidation("noencryption", "1");
                                                LeadInitiate.Content = new StringContent(newJson);
                                                LeadInitiate.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
                                                LeadInitiate.Headers.TryAddWithoutValidation("Authorization", ScaleUpToken.token_type + " " + ScaleUpToken.access_token);

                                                var Leadresponse = await httpClient.SendAsync(LeadInitiate);
                                                if (Leadresponse.StatusCode == HttpStatusCode.OK)
                                                {
                                                    string LeadjsonString = (await Leadresponse.Content.ReadAsStringAsync());
                                                    var result = JsonConvert.DeserializeObject<ScalupPurchaseInvoiceResponse>(LeadjsonString);
                                                    TextFileLogHelper.TraceLog("Lead Initiate - " + LeadjsonString);
                                                    if (result.Status)
                                                    {
                                                        //LeadId = result.Response.LeadId;
                                                        //companyId = result.Response.CompanyId;
                                                        //ProductId = result.Response.ProductId;
                                                        res.message = "Success";
                                                        res.status = true;
                                                    }
                                                    else
                                                    {
                                                        res.message = "Failed";
                                                        res.status = false;
                                                    }
                                                }
                                                else
                                                {
                                                    string LeadjsonString = (await response.Content.ReadAsStringAsync());
                                                    TextFileLogHelper.TraceLog("Lead Initiate - " + LeadjsonString);
                                                    res.message = "Failed";
                                                    res.status = false;
                                                }
                                            }
                                        }
                                        else
                                        {
                                            res.status = true;
                                        }
                                    }
                                    else
                                    {
                                        res.message = "Failed";
                                        res.status = false;
                                    }
                                }
                            }
                        }
                        else
                        {
                            res.message = "Authentication Failed";
                            res.status = false;
                        }
                    }
                }
                else
                {
                    res.status = false;
                    res.message = "ScaleUp Configuration not found.";
                }
            }
            return res;
        }

        //[HttpGet]
        //[Route("OrderInvoice")]
        //public async Task<OrderInvoiceRes> OrderInvoice(string TransactionId, int OrderId, bool Status)
        //{
        //    ScaleUpManager scaleUpManager = new ScaleUpManager();
        //    OrderInvoiceRes res = new OrderInvoiceRes();
        //    res = await scaleUpManager.OrderInvoice(TransactionId, OrderId, Status);
        //    return res;
        //}
    }

    public class OrderInvoice
    {
        public string InvoiceNo { get; set; }
        public double InvoiceAmount { get; set; }
        public DateTime InvoiceDate { get; set; }
        public string InvoicePdfURL { get; set; }
        public string OrderNo { get; set; }
        public string StatusMsg { get; set; }
    }
    public class OrderInvoiceRes
    {
        public bool status { get; set; }
        public string OrderNo { get; set; }
        public string Message { get; set; }
        public string Result { get; set; }
    }
    public class AccountDisbursementNotify
    {
        public bool Status { get; set; }
        public string Message { get; set; }
        public long AccountId { get; set; }
        public long LeadId { get; set; }
        public string CustomerUniqueCode { get; set; }
    }

    public class PaymentRequestdc
    {
        public double TransactionAmount { get; set; }
        public string AnchorCompanyCode { get; set; }
        public string OrderNo { get; set; }
        public long LoanAccountId { get; set; }
        public double OrderAmount { get; set; }
    }

    public class ScalupLimitResponse
    {
        public bool status { get; set; }
        public string message { get; set; }
        public LoanCreditLimit response { get; set; }
    }


    public class ScalupOrderInitiateResponse
    {
        public bool status { get; set; }
        public string message { get; set; }
        public string TransactionId { get; set; }
        public string response { get; set; }
    }

    public class OrderResponse
    {
        public bool status { get; set; }
        public string message { get; set; }
        public string TransactionId { get; set; }
        public string WebViewUrl { get; set; }
        public string BaiseUrl { get; set; }
    }

    public class ScalupLeadInitiateResponse
    {
        public bool Status { get; set; }
        public string Message { get; set; }
        public InitiateLeadReply Response { get; set; }
    }

    public class InitiateLeadReply
    {
        public long LeadId { get; set; }
        public long CompanyId { get; set; }
        public long ProductId { get; set; }
    }



    public class LoanCreditLimit
    {
        public double CreditLimit { get; set; }
        public bool IsBlock { get; set; }
        public bool IsBlockHideLimit { get; set; }
        public string Message { get; set; }
    }
    public class NotifyAnchorOrderCanceled
    {
        public string OrderNo { get; set; }
        public string TransactionNo { get; set; }
        public double amount { get; set; }
        public string Comment { get; set; }

    }
    public class ScalupPurchaseInvoiceResponse
    {
        public bool Status { get; set; }
        public string Message { get; set; }
        public object Response { get; set; }
    }
}
