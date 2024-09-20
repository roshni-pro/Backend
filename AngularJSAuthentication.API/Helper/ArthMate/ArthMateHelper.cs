using AgileObjects.AgileMapper;
using AngularJSAuthentication.API.Controllers.Arthmate;
using AngularJSAuthentication.Common.Constants;
using AngularJSAuthentication.Common.Helpers;
using AngularJSAuthentication.DataContracts.Arthmate;
using AngularJSAuthentication.Model.Arthmate;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using static AngularJSAuthentication.DataContracts.Arthmate.AdharDigiLocker;
using static AngularJSAuthentication.DataContracts.Arthmate.ArthMatePostDc;
using static AngularJSAuthentication.DataContracts.Arthmate.FirstAadharDc;
using static AngularJSAuthentication.DataContracts.Arthmate.SecondAadharDc;
using eAadhaarDigilockerRequestDc = AngularJSAuthentication.DataContracts.Arthmate.AdharDigiLocker.eAadhaarDigilockerRequestDc;

namespace AngularJSAuthentication.API.Helper.ArthMateHelper
{
    public class ArthMateHelper
    {
        public static TimeZoneInfo INDIAN_ZONE = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
        DateTime indianTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE);

        public ArthmateApiConfig GetApiConfig(string providername)
        {
            ArthmateApiConfig ArthmateApiData = new ArthmateApiConfig();
            using (var authContext = new AuthContext())
            {
                ArthmateApiData = authContext.ArthmateApiConfig.Where(x => x.ProviderName == providername && x.IsActive == true && x.IsDeleted == false).FirstOrDefault();
            }
            return ArthmateApiData;

        }
        public ArthMateActivityMaster GetActivityMaster(string TaskName)
        {
            ArthMateActivityMaster ArthmateActivityData = new ArthMateActivityMaster();
            using (var authContext = new AuthContext())
            {
                ArthmateActivityData = authContext.ArthMateActivityMasters.Where(x => x.ApiName == TaskName && x.IsActive == true && x.IsDeleted == false).FirstOrDefault();
            }
            return ArthmateActivityData;

        }
        public GetUrlTokenDc GetUrlTokenForApi(string providername, string TaskName)
        {
            GetUrlTokenDc data = new GetUrlTokenDc();

            RepaymentAllResponse resdata = new RepaymentAllResponse();
            var configdata = GetApiConfig(providername);
            var Activitydata = GetActivityMaster(TaskName);
            if (configdata != null && Activitydata != null)
            {
                data.url = configdata.URL + "/" + Activitydata.URL;
                data.token = Activitydata.TokenKey;
                data.CompanyCode = configdata.CompanyCode == null ? "" : configdata.CompanyCode.ToString();
                data.id = Activitydata.Id;
                data.ApiSecretKey = Activitydata.ApiSecretKey;
                data.TokenKey = configdata.TokenKey;
            }
            return data;
        }
        public async Task<LeadResponseDc> LeadApi(LeadPostdc lead)
        {
            string url = "";
            string token = "";
            string CompanyCode = "";
            LeadResponseDc responseDc = new LeadResponseDc();
            TextFileLogHelper.TraceLog("PostLead : Before try");
            try
            {
                TextFileLogHelper.TraceLog("PostLead : After try");
                TextFileLogHelper.TraceLog("PostLead : GetApiConfig");
                var configdata = GetApiConfig("ArthMate");
                TextFileLogHelper.TraceLog("PostLead : GetActivityMaster");
                var Activitydata = GetActivityMaster("Lead API");

                if (configdata != null && Activitydata != null)
                {
                    TextFileLogHelper.TraceLog("PostLead : GetUrlandToken");
                    url = configdata.URL + "/" + Activitydata.URL;
                    token = configdata.TokenKey;
                    CompanyCode = configdata.CompanyCode != null ? configdata.CompanyCode.ToString() : null;
                }
                TextFileLogHelper.TraceLog("PostLead : phase 2");
                LeadMaster leadObj = new LeadMaster();


                List<ArthMateLeadPostDc> leadlist = new List<ArthMateLeadPostDc>();
                TextFileLogHelper.TraceLog("PostLead : phase 3");
                ArthMateLeadPostDc obj = Mapper.Map(lead).ToANew<ArthMateLeadPostDc>();
                leadlist.Add(obj);
                TextFileLogHelper.TraceLog("PostLead : phase 4");
                ServicePointManager.Expect100Continue = true;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                string jsonstringReq = JsonConvert.SerializeObject(leadlist);

                try
                {
                    var client = new RestClient(url);
                    client.Timeout = -1;
                    var request = new RestRequest(Method.POST);
                    request.AddHeader("NoEncryption", "1");
                    request.AddHeader("Content-Type", "application/json");
                    request.AddHeader("Authorization", token);
                    request.AddParameter("application/json", jsonstringReq, ParameterType.RequestBody);

                    ArthmateReqResp addRequest = new ArthmateReqResp()
                    {
                        partner_borrower_id = lead.partner_borrower_id,
                        ActivityMasterId = Activitydata.Id,
                        LeadMasterId = lead.Id,
                        RequestResponseMsg = jsonstringReq,
                        Type = "Request",
                        Url = url,
                        CreatedBy = 0,
                        CreatedDate = DateTime.Now
                    };
                    TextFileLogHelper.TraceLog("PostLead : phase 5");
                    var req = InsertRequestResponse(addRequest);

                    IRestResponse response = client.Execute(request);
                    TextFileLogHelper.TraceLog("PostLead : phase 6 - " + response);
                    string jsonString = "";
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        jsonString = (response.Content);
                        ArthmateReqResp AddResponse = new ArthmateReqResp()
                        {
                            partner_borrower_id = lead.partner_borrower_id,
                            ActivityMasterId = Activitydata.Id,
                            RequestResponseMsg = jsonString,
                            LeadMasterId = lead.Id,
                            Type = "Response",
                            Url = url,
                            CreatedBy = 0,
                            CreatedDate = DateTime.Now
                        };
                        var res = InsertRequestResponse(AddResponse);
                        responseDc = JsonConvert.DeserializeObject<LeadResponseDc>(jsonString);
                    }
                    else
                    {
                        jsonString = (response.Content);
                        ArthmateReqResp AddResponse = new ArthmateReqResp()
                        {
                            partner_borrower_id = lead.partner_borrower_id,
                            ActivityMasterId = Activitydata.Id,
                            RequestResponseMsg = jsonString,
                            LeadMasterId = lead.Id,
                            Type = "Response",
                            Url = url,
                            CreatedBy = 0,
                            CreatedDate = DateTime.Now
                        };
                        var res = InsertRequestResponse(AddResponse);
                        responseDc = JsonConvert.DeserializeObject<LeadResponseDc>(jsonString);
                    }

                }
                catch (Exception ex)
                {
                    ArthmateReqResp addRequest = new ArthmateReqResp()
                    {


                        partner_borrower_id = lead.partner_borrower_id,
                        ActivityMasterId = Activitydata.Id,
                        RequestResponseMsg = ex.ToString(),
                        LeadMasterId = lead.Id,
                        Type = "Response",
                        Url = url,
                        CreatedBy = 0,
                        CreatedDate = DateTime.Now
                    };
                    TextFileLogHelper.TraceLog("PostLead : phase 7 - " + ex.ToString());
                    var resw = InsertRequestResponse(addRequest);
                }
            }
            catch (Exception ex)
            {
                TextFileLogHelper.TraceLog("PostLead : Exception" + ex.Message);
                throw ex;
            }
            return responseDc;
        }
        public async Task<LoanDocumentResponseDc> LoanDocumentApi(LoanDocumentPostDc document, long leadmasterid, bool IsBankStatement)
        {
            string url = "";
            string token = "";
            string CompanyCode = "";
            string RequestjsonString = "";
            LoanDocumentResponseDc responseDc = new LoanDocumentResponseDc();
            var configdata = GetApiConfig("ArthMate");
            var Activitydata = GetActivityMaster("Loan Document API");
            if (configdata != null && Activitydata != null)
            {
                url = configdata.URL + "/" + Activitydata.URL;
                token = configdata.TokenKey;
                CompanyCode = configdata.CompanyCode == null ? "" : configdata.CompanyCode.ToString();
            }
            if (IsBankStatement)
            {
                LoanDocumentBankDc obj = new LoanDocumentBankDc()
                {
                    base64pdfencodedfile = document.base64pdfencodedfile,
                    borrower_id = document.borrower_id,
                    code = document.code,
                    doc_key = document.PdfPassword,
                    loan_app_id = document.loan_app_id,
                    partner_borrower_id = document.partner_borrower_id,
                    partner_loan_app_id = document.partner_loan_app_id,
                    fileType = "bank_stmnts"

                };
                RequestjsonString = JsonConvert.SerializeObject(obj);
            }
            else
            {
                LoanDocumentDc obj = Mapper.Map(document).ToANew<LoanDocumentDc>();

                RequestjsonString = JsonConvert.SerializeObject(obj);
            }
            TextFileLogHelper.TraceLog("LoanDocumentApi helper 1" + document.FrontUrl);


            try
            {
                var client = new RestClient(url);
                client.Timeout = -1;
                var request = new RestRequest(Method.POST);
                request.AddHeader("NoEncryption", "1");
                request.AddHeader("Content-Type", "application/json");
                request.AddHeader("Authorization", token);
                request.AddParameter("application/json", RequestjsonString, ParameterType.RequestBody);
                ServicePointManager.Expect100Continue = true;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                ArthmateReqResp addRequest = new ArthmateReqResp()
                {
                    ActivityMasterId = Activitydata.Id,
                    partner_borrower_id = document.borrower_id,
                    RequestResponseMsg = RequestjsonString,
                    LeadMasterId = leadmasterid,
                    Type = "Request",
                    Url = url,
                    CreatedBy = 0,
                    CreatedDate = DateTime.Now
                };
                // var reqsave = InsertRequestResponse(addRequest);

                var reqsave = InsertRequestResponseID(addRequest);

                IRestResponse response = await client.ExecuteAsync(request);

                TextFileLogHelper.TraceLog("LoanDocumentApi 2 =" + response.ToString());

                string jsonString = "";
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    jsonString = (response.Content);
                    ArthmateReqResp AddResponse = new ArthmateReqResp()
                    {
                        ActivityMasterId = Activitydata.Id,
                        partner_borrower_id = document.borrower_id,
                        RequestResponseMsg = jsonString,
                        LeadMasterId = leadmasterid,
                        Type = "Response",
                        Url = url,
                        CreatedBy = 0,
                        CreatedDate = DateTime.Now
                    };
                    var res = InsertRequestResponseID(AddResponse);
                    responseDc = JsonConvert.DeserializeObject<LoanDocumentResponseDc>(jsonString);
                }
                else
                {
                    jsonString = (response.Content);
                    ArthmateReqResp AddResponse = new ArthmateReqResp()
                    {
                        ActivityMasterId = Activitydata.Id,
                        partner_borrower_id = document.borrower_id,
                        RequestResponseMsg = jsonString,
                        LeadMasterId = leadmasterid,
                        Type = "Response",
                        Url = url,
                        CreatedBy = 0,
                        CreatedDate = DateTime.Now
                    };
                    var res = InsertRequestResponseID(AddResponse);
                    var data = JsonConvert.DeserializeObject<LoanDocumentError>(jsonString);
                    responseDc.message = data.message;
                    responseDc.success = data.success;
                    responseDc.RequestId = reqsave;
                    responseDc.ReponseId = res;

                }

            }
            catch (Exception ex)
            {
                ArthmateReqResp addRequest = new ArthmateReqResp()
                {
                    ActivityMasterId = Activitydata.Id,
                    partner_borrower_id = document.borrower_id,
                    RequestResponseMsg = ex.ToString(),
                    LeadMasterId = leadmasterid,
                    Type = "Response",
                    Url = url,
                    CreatedBy = 0,
                    CreatedDate = DateTime.Now
                };
                TextFileLogHelper.TraceLog("LoanDocumentApi catch blocks =" + ex.ToString());
                var resw = InsertRequestResponse(addRequest);
            }

            return responseDc;
        }
        public bool InsertRequestResponse(ArthmateReqResp obj)
        {
            bool res = false;
            using (AuthContext db = new AuthContext())
            {
                if (obj != null)
                {
                    db.ArthmateReqResp.Add(obj);
                    if (db.Commit() > 0)
                    {
                        res = true;
                    }
                }
            }
            return res;
        }

        public long InsertRequestResponseID(ArthmateReqResp obj)
        {
            long RequestResponse = 0;
            using (AuthContext db = new AuthContext())
            {
                if (obj != null)
                {
                    db.ArthmateReqResp.Add(obj);
                    if (db.Commit() > 0)
                    {
                        RequestResponse = obj.Id;
                    }
                }
            }
            return RequestResponse;
        }


        public bool InsertLeadBackgroundRun(long LeadMasterID, string Apiname, string ReqJson, string ResJson/*LeadBackgroundRun obj*/)
        {
            bool res = false;
            AuthContext auth = new AuthContext();
            var activitydata = auth.ArthMateActivityMasters.Where(x => x.ApiName == Apiname).FirstOrDefault();
            var backgrnddata = auth.LeadBackgroundRuns.Where(x => x.LeadMasterId == LeadMasterID && x.ArthmateActivityName == Apiname).FirstOrDefault();

            //auth.LeadBackgroundRuns.Add(obj);

            if (backgrnddata == null)
            {
                LeadBackgroundRun leadBackgroundRun = new LeadBackgroundRun();
                leadBackgroundRun.ArthmateActivityMastersId = activitydata.Id;
                leadBackgroundRun.ArthmateActivityName = Apiname;
                leadBackgroundRun.CreatedDate = DateTime.Now;
                leadBackgroundRun.CreatedBy = 0;
                leadBackgroundRun.LeadMasterId = LeadMasterID;
                leadBackgroundRun.Status = "";
                leadBackgroundRun.ReqJson = ReqJson;
                leadBackgroundRun.ResJson = ResJson;

                auth.LeadBackgroundRuns.Add(leadBackgroundRun);
                auth.Commit();
                res = true;
            }
            else
            {
                backgrnddata.ReqJson = ReqJson; ;
                backgrnddata.ResJson = ResJson;
                backgrnddata.ModifiedDate = DateTime.Now;
                backgrnddata.ModifiedBy = 0;
                auth.Entry(backgrnddata).State = EntityState.Modified;
                auth.Commit();
                res = true;
            }

            return res;
        }


        #region Ceplr Api //by roshni
        public async Task<RequestConsentResponseDc> CeplrRequestConsentApi(RequestConsentDc CeplrObj)
        {
            TextFileLogHelper.TraceLog("start CeplrRequestConsentApi 1");

            GetUrlTokenDc data = new GetUrlTokenDc();
            data = GetUrlTokenForApi("ArthMate", "GST VERIFY");

            RequestConsentDc Ceplr = new RequestConsentDc();
            RequestConsentResponseDc responseDc = new RequestConsentResponseDc();
            if (CeplrObj != null)
            {
                Ceplr = new RequestConsentDc
                {
                    customer_name = CeplrObj.customer_name,
                    customer_contact = CeplrObj.customer_contact,
                    customer_email = CeplrObj.customer_email,
                    configuration_uuid = CeplrObj.configuration_uuid,
                    redirect_url = CeplrObj.redirect_url,
                    callback_url = CeplrObj.callback_url,
                    fip_id = CeplrObj.fip_id
                };
            }
            using (var httpClient = new HttpClient())
            {
                try
                {
                    using (var request = new HttpRequestMessage(new HttpMethod("POST"), data.url))
                    {
                        request.Headers.TryAddWithoutValidation("cache-control", "no-cache");
                        request.Headers.TryAddWithoutValidation("Content-Type", "application/json");
                        request.Headers.TryAddWithoutValidation("Authorization", data.token);
                        request.Headers.TryAddWithoutValidation("company_code", data.CompanyCode);

                        string jsonstringReq = JsonConvert.SerializeObject(Ceplr);
                        request.Content = new StringContent(jsonstringReq);

                        TextFileLogHelper.TraceLog("CeplrRequestConsentApi request 2 =" + jsonstringReq);

                        ArthmateReqResp addRequest = new ArthmateReqResp()
                        {
                            ActivityMasterId = data.id,
                            LeadMasterId = 0,//AadharObj.LeadId,
                            RequestResponseMsg = jsonstringReq,
                            Type = "Request",
                            Url = data.url,
                            CreatedBy = 0,
                            CreatedDate = DateTime.Now
                        };
                        var req = InsertRequestResponse(addRequest);

                        string jsonString = string.Empty;
                        var response = await httpClient.SendAsync(request);

                        TextFileLogHelper.TraceLog("CeplrRequestConsentApi request 3 =" + response.ToString());

                        if (response.StatusCode == HttpStatusCode.OK)
                        {
                            jsonString = (await response.Content.ReadAsStringAsync());
                            responseDc = JsonConvert.DeserializeObject<RequestConsentResponseDc>(jsonString);
                        }
                        ArthmateReqResp AddResponse = new ArthmateReqResp()
                        {
                            ActivityMasterId = data.id,
                            LeadMasterId = 0,
                            RequestResponseMsg = jsonString,
                            Type = "Response",
                            Url = data.url,
                            CreatedBy = 0,
                            CreatedDate = DateTime.Now
                        };
                        var res = InsertRequestResponse(AddResponse);
                    };
                }
                catch (Exception ex)
                {
                    ArthmateReqResp addRequest = new ArthmateReqResp()
                    {
                        ActivityMasterId = data.id,
                        LeadMasterId = 0,
                        RequestResponseMsg = ex.ToString(),
                        Type = "Response",
                        Url = data.url,
                        CreatedBy = 0,
                        CreatedDate = DateTime.Now
                    };
                    TextFileLogHelper.TraceLog("CeplrRequestConsentApi catch block 4 =" + ex.ToString());
                }
            }

            return responseDc;
        }
        public async Task<CeplrFIStatusResponseDc> CeplrFIStatusApi(string link_uuid)
        {
            string url = "";
            string token = "";
            string CompanyCode = "";
            var configdata = GetApiConfig("Cepler");
            var Activitydata = GetActivityMaster("FI status");
            if (configdata != null && Activitydata != null)
            {
                url = configdata.URL + "/" + Activitydata.URL;
                token = configdata.TokenKey;
                CompanyCode = Convert.ToString(configdata.CompanyCode);
            }
            CeplrFIStatusResponseDc responseDc = new CeplrFIStatusResponseDc();
            using (var httpClient = new HttpClient())
            {
                try
                {
                    using (var request = new HttpRequestMessage(new HttpMethod("GET"), url + '/' + link_uuid))
                    {
                        request.Headers.TryAddWithoutValidation("cache-control", "no-cache");
                        request.Headers.TryAddWithoutValidation("Content-Type", "application/json");
                        request.Headers.TryAddWithoutValidation("Authorization", token);
                        request.Headers.TryAddWithoutValidation("company_code", CompanyCode);

                        //string jsonstringReq = JsonConvert.SerializeObject(link_uuid);
                        //request.Content = new StringContent(jsonstringReq);

                        ArthmateReqResp addRequest = new ArthmateReqResp()
                        {
                            ActivityMasterId = Activitydata.Id,
                            LeadMasterId = 0,//AadharObj.LeadId,
                            RequestResponseMsg = link_uuid,
                            Type = "Request",
                            Url = url,
                            CreatedBy = 0,
                            CreatedDate = DateTime.Now
                        };
                        var req = InsertRequestResponse(addRequest);

                        string jsonString = string.Empty;
                        var response = await httpClient.SendAsync(request);
                        if (response.StatusCode == HttpStatusCode.OK)
                        {
                            jsonString = (await response.Content.ReadAsStringAsync());
                            responseDc = JsonConvert.DeserializeObject<CeplrFIStatusResponseDc>(jsonString);
                        }
                        ArthmateReqResp AddResponse = new ArthmateReqResp()
                        {
                            ActivityMasterId = Activitydata.Id,
                            LeadMasterId = 0,
                            RequestResponseMsg = jsonString,
                            Type = "Response",
                            Url = url,
                            CreatedBy = 0,
                            CreatedDate = DateTime.Now
                        };
                        var res = InsertRequestResponse(AddResponse);
                    };
                }
                catch (Exception ex)
                {
                    ArthmateReqResp addRequest = new ArthmateReqResp()
                    {
                        ActivityMasterId = Activitydata.Id,
                        LeadMasterId = 0,
                        RequestResponseMsg = ex.ToString(),
                        Type = "Response",
                        Url = url,
                        CreatedBy = 0,
                        CreatedDate = DateTime.Now
                    };
                }
            }

            return responseDc;
        }
        public async Task<CeplrBankListdc> CeplrBankList()
        {
            string url = "";
            string token = "";
            string CompanyCode = "";
            string apikey = "";
            var configdata = GetApiConfig("Cepler");
            var Activitydata = GetActivityMaster("CeplrBankList");
            if (configdata != null && Activitydata != null)
            {
                url = configdata.URL + Activitydata.URL;
                token = configdata.TokenKey;
                CompanyCode = Convert.ToString(configdata.CompanyCode);
                apikey = Activitydata.ApiSecretKey;
            }
            CeplrBankListdc responseDc = new CeplrBankListdc();
            try
            {
                //using (var request = new HttpRequestMessage(new HttpMethod("GET"), url))
                //{
                //    ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;

                //    request.Headers.TryAddWithoutValidation("cache-control", "no-cache");
                //    request.Headers.TryAddWithoutValidation("Content-Type", "application/json");
                //    ////request.Headers.TryAddWithoutValidation("Authorization", token);
                //    ////request.Headers.TryAddWithoutValidation("company_code", CompanyCode);
                //    request.Headers.TryAddWithoutValidation("x-api-key", apikey);
                //    var response = await httpClient.SendAsync(request);

                //    string jsonstringReq = JsonConvert.SerializeObject(request);
                //    //request.Content = new StringContent(jsonstringReq);

                var client = new RestClient(url);
                client.Timeout = -1;
                var request = new RestRequest(Method.GET);
                request.AddHeader("NoEncryption", "1");
                request.AddHeader("Content-Type", "application/json");
                request.AddHeader("x-api-key", apikey);
                ServicePointManager.Expect100Continue = true;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                ArthmateReqResp addRequest = new ArthmateReqResp()
                {
                    ActivityMasterId = Activitydata.Id,
                    partner_borrower_id = "",
                    RequestResponseMsg = "",
                    LeadMasterId = 0,
                    Type = "Request",
                    Url = url,
                    CreatedBy = 0,
                    CreatedDate = DateTime.Now
                };
                var reqsave = InsertRequestResponse(addRequest);
                IRestResponse response = await client.ExecuteAsync(request);
                string jsonString = "";
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    jsonString = (response.Content);
                    responseDc = JsonConvert.DeserializeObject<CeplrBankListdc>(jsonString);
                }
                else
                {
                    jsonString = (response.Content);
                    responseDc = JsonConvert.DeserializeObject<CeplrBankListdc>(jsonString);

                }
                ArthmateReqResp AddResponse = new ArthmateReqResp()
                {
                    ActivityMasterId = Activitydata.Id,
                    partner_borrower_id = "",
                    RequestResponseMsg = jsonString,
                    LeadMasterId = 0,
                    Type = "Response",
                    Url = url,
                    CreatedBy = 0,
                    CreatedDate = DateTime.Now
                };
                var res = InsertRequestResponse(AddResponse);
            }
            catch (Exception ex)
            {
                ArthmateReqResp addRequest = new ArthmateReqResp()
                {
                    ActivityMasterId = Activitydata.Id,
                    partner_borrower_id = "",
                    RequestResponseMsg = ex.ToString(),
                    LeadMasterId = 0,
                    Type = "Response",
                    Url = url,
                    CreatedBy = 0,
                    CreatedDate = DateTime.Now
                };
                var resw = InsertRequestResponse(addRequest);
            }


            return responseDc;

        }


        #endregion

        #region //by vishal
        //public async Task<PanNsdlResponse> PanVerificationAsync(PanVerificationRequestJson data, long leadId) //PanNsdlResponse
        //{
        //    PanNsdlResponse responsedc = new PanNsdlResponse();

        //    ServicePointManager.Expect100Continue = true;
        //    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
        //    string url = "";
        //    string token = "";
        //    string CompanyCode = "";
        //    var configdata = GetApiConfig("ArthMate");
        //    var Activitydata = GetActivityMaster("PAN validation API"); //NSDL PAN validation V2 API
        //    if (configdata != null && Activitydata != null)
        //    {
        //        url = configdata.URL + "/" + Activitydata.URL;
        //        token = Activitydata.TokenKey;
        //        CompanyCode = configdata.CompanyCode == null ? "" : configdata.CompanyCode.ToString(); ;
        //    }
        //    var RequestjsonString = JsonConvert.SerializeObject(data);
        //    try
        //    {
        //        var client = new RestClient(url);
        //        client.Timeout = -1;
        //        var request = new RestRequest(Method.POST);
        //        request.AddHeader("NoEncryption", "1");
        //        request.AddHeader("Content-Type", "application/json");
        //        request.AddHeader("Authorization", token);
        //        request.AddParameter("application/json", RequestjsonString, ParameterType.RequestBody);

        //        ArthmateReqResp addRequest = new ArthmateReqResp()
        //        {
        //            ActivityMasterId = Activitydata.Id,
        //            LeadMasterId = leadId,
        //            RequestResponseMsg = RequestjsonString,
        //            Type = "Request",
        //            Url = url,
        //            CreatedBy = 0,
        //            CreatedDate = DateTime.Now
        //        };
        //        var req = InsertRequestResponse(addRequest);
        //        IRestResponse response = client.Execute(request);
        //        string jsonString = "";
        //        if (response.StatusCode == HttpStatusCode.OK)
        //        {
        //            jsonString = (response.Content);
        //            ArthmateReqResp AddResponse = new ArthmateReqResp()
        //            {
        //                ActivityMasterId = Activitydata.Id,
        //                LeadMasterId = leadId,
        //                RequestResponseMsg = jsonString,
        //                Type = "Response",
        //                Url = url,
        //                CreatedBy = 0,
        //                CreatedDate = DateTime.Now
        //            };
        //            var res = InsertRequestResponse(AddResponse);
        //            responsedc = JsonConvert.DeserializeObject<PanNsdlResponse>(jsonString);  //PanValidationRspnsNew //PanNsdlResponse
        //            responsedc.KYCResponse = jsonString;
        //        }
        //        else
        //        {
        //            jsonString = (response.Content);
        //            ArthmateReqResp AddResponse = new ArthmateReqResp()
        //            {
        //                ActivityMasterId = Activitydata.Id,
        //                LeadMasterId = leadId,
        //                RequestResponseMsg = jsonString,
        //                Type = "Response",
        //                Url = url,
        //                CreatedBy = 0,
        //                CreatedDate = DateTime.Now
        //            };
        //            var res = InsertRequestResponse(AddResponse);
        //            responsedc = JsonConvert.DeserializeObject<PanNsdlResponse>(jsonString);
        //            responsedc.KYCResponse = jsonString;
        //        }

        //    }
        //    catch (Exception ex)
        //    {
        //        ArthmateReqResp addRequest = new ArthmateReqResp()
        //        {
        //            ActivityMasterId = Activitydata.Id,
        //            LeadMasterId = leadId,
        //            RequestResponseMsg = ex.ToString(),
        //            Type = "Response",
        //            Url = url,
        //            CreatedBy = 0,
        //            CreatedDate = DateTime.Now
        //        };
        //        var resw = InsertRequestResponse(addRequest);
        //    }
        //    return responsedc;
        //}

        ////PanVerificationV3Async On 21_03_2024
        public async Task<PanValidationRspnsV3> PanVerificationV3Async(PanVerificationRequestV3 data, long leadId) //PanVerificationV3Async On 21_03_2024
        {
            PanValidationRspnsV3 responsedc = new PanValidationRspnsV3();

            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            string url = "";
            string token = "";
            string CompanyCode = "";
            var configdata = GetApiConfig("ArthMate");
            var Activitydata = GetActivityMaster("NSDL PAN validation V3 API"); //NSDL PAN validation V3 API
            if (configdata != null && Activitydata != null)
            {
                url = configdata.URL + "/" + Activitydata.URL;
                token = Activitydata.TokenKey;
                CompanyCode = configdata.CompanyCode == null ? "" : configdata.CompanyCode.ToString(); ;
            }
            var RequestjsonString = JsonConvert.SerializeObject(data);
            try
            {
                var client = new RestClient(url);
                client.Timeout = -1;
                var request = new RestRequest(Method.POST);
                request.AddHeader("NoEncryption", "1");
                request.AddHeader("Content-Type", "application/json");
                request.AddHeader("Authorization", token);
                request.AddParameter("application/json", RequestjsonString, ParameterType.RequestBody);

                ArthmateReqResp addRequest = new ArthmateReqResp()
                {
                    ActivityMasterId = Activitydata.Id,
                    LeadMasterId = leadId,
                    RequestResponseMsg = RequestjsonString,
                    Type = "Request",
                    Url = url,
                    CreatedBy = 0,
                    CreatedDate = DateTime.Now
                };
                var req = InsertRequestResponse(addRequest);
                IRestResponse response = client.Execute(request);
                string jsonString = "";
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    jsonString = (response.Content);
                    ArthmateReqResp AddResponse = new ArthmateReqResp()
                    {
                        ActivityMasterId = Activitydata.Id,
                        LeadMasterId = leadId,
                        RequestResponseMsg = jsonString,
                        Type = "Response",
                        Url = url,
                        CreatedBy = 0,
                        CreatedDate = DateTime.Now
                    };
                    var res = InsertRequestResponse(AddResponse);
                    responsedc = JsonConvert.DeserializeObject<PanValidationRspnsV3>(jsonString);  //PanValidationRspnsNew //PanNsdlResponse
                    responsedc.KYCResponse = jsonString;
                }
                else
                {
                    jsonString = (response.Content);
                    ArthmateReqResp AddResponse = new ArthmateReqResp()
                    {
                        ActivityMasterId = Activitydata.Id,
                        LeadMasterId = leadId,
                        RequestResponseMsg = jsonString,
                        Type = "Response",
                        Url = url,
                        CreatedBy = 0,
                        CreatedDate = DateTime.Now
                    };
                    var res = InsertRequestResponse(AddResponse);
                    responsedc = JsonConvert.DeserializeObject<PanValidationRspnsV3>(jsonString);
                    responsedc.KYCResponse = jsonString;
                }

            }
            catch (Exception ex)
            {
                ArthmateReqResp addRequest = new ArthmateReqResp()
                {
                    ActivityMasterId = Activitydata.Id,
                    LeadMasterId = leadId,
                    RequestResponseMsg = ex.ToString(),
                    Type = "Response",
                    Url = url,
                    CreatedBy = 0,
                    CreatedDate = DateTime.Now
                };
                var resw = InsertRequestResponse(addRequest);
            }
            return responsedc;
        }

        //ArthmateDocValidation xml
        public async Task<ValidationDocResponse> ArthmateDocValidationJsonXml(JsonXmlRequest jsonXmlRequest, long Leadmasterid)
        {
            TextFileLogHelper.TraceLog("ArthmateDocValidationJsonXml 1 =" + Leadmasterid.ToString());

            string url = "";
            string token = "";
            string CompanyCode = "";
            ValidationDocResponse resdata = new ValidationDocResponse();
            var configdata = GetApiConfig("ArthMate");
            var Activitydata = GetActivityMaster("Validation(JSON/XML)");
            if (configdata != null && Activitydata != null)
            {
                url = configdata.URL + Activitydata.URL;
                token = configdata.TokenKey;
                CompanyCode = configdata.CompanyCode == null ? "" : configdata.CompanyCode.ToString();
            }

            var RequestjsonString = JsonConvert.SerializeObject(jsonXmlRequest);
            TextFileLogHelper.TraceLog("ArthmateDocValidationJsonXml Request json String 2 =" + jsonXmlRequest);

            try
            {
                var client = new RestClient(url);
                client.Timeout = -1;
                var request = new RestRequest(Method.POST);
                request.AddHeader("NoEncryption", "1");
                request.AddHeader("Content-Type", "application/json");
                request.AddHeader("Authorization", token);
                request.AddParameter("application/json", RequestjsonString, ParameterType.RequestBody);
                ServicePointManager.Expect100Continue = true;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                ArthmateReqResp addRequest = new ArthmateReqResp()
                {
                    ActivityMasterId = Activitydata.Id,
                    LeadMasterId = Leadmasterid,
                    RequestResponseMsg = RequestjsonString,
                    Type = "Request",
                    Url = url,
                    CreatedBy = 0,
                    CreatedDate = DateTime.Now
                };
                var reqsave = InsertRequestResponse(addRequest);
                IRestResponse response = await client.ExecuteAsync(request);

                TextFileLogHelper.TraceLog("ArthmateDocValidationJsonXml response 3 =" + response);

                string jsonString = "";
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    jsonString = (response.Content);
                    resdata = JsonConvert.DeserializeObject<ValidationDocResponse>(jsonString);
                }
                else
                {
                    jsonString = (response.Content);
                    resdata = JsonConvert.DeserializeObject<ValidationDocResponse>(jsonString);
                }
                ArthmateReqResp AddResponse = new ArthmateReqResp()
                {
                    ActivityMasterId = Activitydata.Id,
                    LeadMasterId = Leadmasterid,
                    RequestResponseMsg = jsonString,
                    Type = "Response",
                    Url = url,
                    CreatedBy = 0,
                    CreatedDate = DateTime.Now
                };
                var res = InsertRequestResponse(AddResponse);
                //var data = InsertLeadBackgroundRun(Leadmasterid, "Repayment API (V2)", RequestjsonString, jsonString);
            }
            catch (Exception ex)
            {
                ArthmateReqResp addRequest = new ArthmateReqResp()
                {
                    ActivityMasterId = Activitydata.Id,
                    LeadMasterId = Leadmasterid,
                    RequestResponseMsg = ex.ToString(),
                    Type = "Response",
                    Url = url,
                    CreatedBy = 0,
                    CreatedDate = DateTime.Now
                };
                var resw = InsertRequestResponse(addRequest);
                TextFileLogHelper.TraceLog("ArthmateDocValidationJsonXml catch block =" + ex.ToString());
            }

            return resdata;
        }




        //loan Api by vishal

        public async Task<GetLeadResponse> GetLeadApi(string loan_app_id, int userid, long Leadid)
        {
            string url = "";
            string token = "";
            string CompanyCode = "";
            GetLeadResponse resdata = new GetLeadResponse();
            var configdata = GetApiConfig("ArthMate");
            var Activitydata = GetActivityMaster("Lead API  GET");
            if (configdata != null && Activitydata != null)
            {
                url = configdata.URL + "/" + Activitydata.URL + "/" + loan_app_id;
                token = configdata.TokenKey;
                CompanyCode = configdata.CompanyCode == null ? "" : configdata.CompanyCode.ToString();
            }
            //var RequestjsonString = JsonConvert.SerializeObject(json);
            try
            {
                var client = new RestClient(url);
                client.Timeout = -1;
                var request = new RestRequest(Method.GET);
                request.AddHeader("NoEncryption", "1");
                request.AddHeader("Content-Type", "application/json");
                request.AddHeader("Authorization", token);
                //request.AddParameter("application/json", loan_app_id, ParameterType.UrlSegment);
                ServicePointManager.Expect100Continue = true;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                ArthmateReqResp addRequest = new ArthmateReqResp()
                {
                    ActivityMasterId = Activitydata.Id,
                    LeadMasterId = Leadid,
                    RequestResponseMsg = loan_app_id,
                    Type = "Request",
                    Url = url,
                    CreatedBy = 0,
                    CreatedDate = DateTime.Now
                };
                var reqsave = InsertRequestResponse(addRequest);
                IRestResponse response = await client.ExecuteAsync(request);
                string jsonString = "";
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    jsonString = (response.Content);
                    resdata = JsonConvert.DeserializeObject<GetLeadResponse>(jsonString);
                }
                else
                {
                    jsonString = (response.Content);
                    resdata = JsonConvert.DeserializeObject<GetLeadResponse>(jsonString);
                }
                ArthmateReqResp AddResponse = new ArthmateReqResp()
                {
                    ActivityMasterId = Activitydata.Id,
                    LeadMasterId = Leadid,
                    RequestResponseMsg = jsonString,
                    Type = "Response",
                    Url = url,
                    CreatedBy = 0,
                    CreatedDate = DateTime.Now
                };
                var res = InsertRequestResponse(AddResponse);
            }
            catch (Exception ex)
            {
                ArthmateReqResp addRequest = new ArthmateReqResp()
                {
                    ActivityMasterId = Activitydata.Id,
                    LeadMasterId = Leadid,
                    RequestResponseMsg = ex.ToString(),
                    Type = "Response",
                    Url = url,
                    CreatedBy = 0,
                    CreatedDate = DateTime.Now
                };
                var resw = InsertRequestResponse(addRequest);
            }

            return resdata;
        }

        //gst verify
        public async Task<GstVerifyResponse> GstVerification(GstVerifyRequest gstVerifyRequest, long Leadid)
        {

            GstVerifyResponse resdata = new GstVerifyResponse();

            GetUrlTokenDc data = new GetUrlTokenDc();
            data = GetUrlTokenForApi("ArthMate", "GST VERIFY");

            var RequestjsonString = JsonConvert.SerializeObject(gstVerifyRequest);
            try
            {
                var client = new RestClient(data.url);
                client.Timeout = -1;
                var request = new RestRequest(Method.POST);
                request.AddHeader("NoEncryption", "1");
                request.AddHeader("Content-Type", "application/json");
                request.AddHeader("Authorization", data.token);
                request.AddParameter("application/json", RequestjsonString, ParameterType.RequestBody);
                ServicePointManager.Expect100Continue = true;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                ArthmateReqResp addRequest = new ArthmateReqResp()
                {
                    ActivityMasterId = Convert.ToInt64(data.id),
                    LeadMasterId = Leadid,
                    RequestResponseMsg = RequestjsonString,
                    Type = "Request",
                    Url = data.url,
                    CreatedBy = 0,
                    CreatedDate = DateTime.Now
                };
                var reqsave = InsertRequestResponse(addRequest);
                IRestResponse response = await client.ExecuteAsync(request);

                TextFileLogHelper.TraceLog("GstVerification 1 =" + response.ToString());

                string jsonString = "";
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    jsonString = (response.Content);
                    resdata = JsonConvert.DeserializeObject<GstVerifyResponse>(jsonString);
                }
                else
                {
                    jsonString = (response.Content);
                    resdata = JsonConvert.DeserializeObject<GstVerifyResponse>(jsonString);
                }
                ArthmateReqResp AddResponse = new ArthmateReqResp()
                {
                    ActivityMasterId = Convert.ToInt64(data.id),
                    LeadMasterId = Leadid,
                    RequestResponseMsg = jsonString,
                    Type = "Response",
                    Url = data.url,
                    CreatedBy = 0,
                    CreatedDate = DateTime.Now
                };
                var res = InsertRequestResponse(AddResponse);
            }
            catch (Exception ex)
            {
                ArthmateReqResp addRequest = new ArthmateReqResp()
                {
                    ActivityMasterId = Convert.ToInt64(data.id),
                    LeadMasterId = Leadid,
                    RequestResponseMsg = ex.ToString(),
                    Type = "Response",
                    Url = data.url,
                    CreatedBy = 0,
                    CreatedDate = DateTime.Now
                };
                var resw = InsertRequestResponse(addRequest);
                TextFileLogHelper.TraceLog("GstVerification catch block 2 =" + ex.ToString());
            }

            return resdata;
        }
        public async Task<CoLenderResponseDc> CoLenderApi(CoLenderRequest coLenderReq, int userid, long Leadid)
        {
            CoLenderResponseDc resdata = new CoLenderResponseDc();

            GetUrlTokenDc data = new GetUrlTokenDc();
            data = GetUrlTokenForApi("ArthMate", "Co-Lender");

            var RequestjsonString = JsonConvert.SerializeObject(coLenderReq);

            TextFileLogHelper.TraceLog("CoLenderApi request 1 =" + RequestjsonString);
            try
            {
                var client = new RestClient(data.url);
                client.Timeout = -1;
                var request = new RestRequest(Method.POST);
                request.AddHeader("NoEncryption", "1");
                request.AddHeader("Content-Type", "application/json");
                request.AddHeader("Authorization", data.token);
                request.AddParameter("application/json", RequestjsonString, ParameterType.RequestBody);
                ServicePointManager.Expect100Continue = true;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                ArthmateReqResp addRequest = new ArthmateReqResp()
                {
                    ActivityMasterId = Convert.ToInt64(data.id),
                    LeadMasterId = Leadid,
                    RequestResponseMsg = RequestjsonString,
                    Type = "Request",
                    Url = data.url,
                    CreatedBy = 0,
                    CreatedDate = DateTime.Now
                };
                var reqsave = InsertRequestResponse(addRequest);
                IRestResponse response = await client.ExecuteAsync(request);

                TextFileLogHelper.TraceLog("CoLenderApi response 2 =" + response.ToString());

                string jsonString = "";
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    jsonString = (response.Content);
                    ArthmateReqResp AddResponse = new ArthmateReqResp()
                    {
                        ActivityMasterId = Convert.ToInt64(data.id),
                        LeadMasterId = Leadid,
                        RequestResponseMsg = jsonString,
                        Type = "Response",
                        Url = data.url,
                        CreatedBy = 0,
                        CreatedDate = DateTime.Now
                    };
                    var res = InsertRequestResponse(AddResponse);
                    resdata = JsonConvert.DeserializeObject<CoLenderResponseDc>(jsonString);
                }
                else
                {
                    jsonString = (response.Content);
                    ArthmateReqResp AddResponse = new ArthmateReqResp()
                    {
                        ActivityMasterId = Convert.ToInt64(data.id),
                        LeadMasterId = Leadid,
                        RequestResponseMsg = jsonString,
                        Type = "Response",
                        Url = data.url,
                        CreatedBy = 0,
                        CreatedDate = DateTime.Now
                    };
                    var res = InsertRequestResponse(AddResponse);
                    resdata = JsonConvert.DeserializeObject<CoLenderResponseDc>(jsonString);
                }

            }
            catch (Exception ex)
            {
                ArthmateReqResp addRequest = new ArthmateReqResp()
                {
                    ActivityMasterId = Convert.ToInt64(data.id),
                    LeadMasterId = Leadid,
                    RequestResponseMsg = ex.ToString(),
                    Type = "Response",
                    Url = data.url,
                    CreatedBy = 0,
                    CreatedDate = DateTime.Now
                };
                var resw = InsertRequestResponse(addRequest);
                TextFileLogHelper.TraceLog("CoLenderApi catch block 2 =" + ex.ToString());
            }

            return resdata;
        }
        public async Task<AScoreAPIResponse> AscoreApi(AScoreAPIRequest scorereq, int userid, long Leadid)
        {
            AScoreAPIResponse resdata = new AScoreAPIResponse();

            GetUrlTokenDc data = new GetUrlTokenDc();
            data = GetUrlTokenForApi("ArthMate", "Request-A ScoreAPI");

            var RequestjsonString = JsonConvert.SerializeObject(scorereq);
            try
            {
                var client = new RestClient(data.url);
                client.Timeout = -1;
                var request = new RestRequest(Method.POST);
                request.AddHeader("NoEncryption", "1");
                request.AddHeader("Content-Type", "application/json");
                request.AddHeader("Authorization", data.token);
                request.AddParameter("application/json", RequestjsonString, ParameterType.RequestBody);
                ServicePointManager.Expect100Continue = true;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                ArthmateReqResp addRequest = new ArthmateReqResp()
                {
                    ActivityMasterId = Convert.ToInt64(data.id),
                    LeadMasterId = Leadid,
                    RequestResponseMsg = RequestjsonString,
                    Type = "Request",
                    Url = data.url,
                    CreatedBy = 0,
                    CreatedDate = DateTime.Now
                };
                var reqsave = InsertRequestResponse(addRequest);
                IRestResponse response = client.Post<RestResponse>(request);
                string jsonString = "";
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    jsonString = (response.Content);
                    ArthmateReqResp AddResponse = new ArthmateReqResp()
                    {
                        ActivityMasterId = Convert.ToInt64(data.id),
                        LeadMasterId = Leadid,
                        RequestResponseMsg = jsonString,
                        Type = "Response",
                        Url = data.url,
                        CreatedBy = 0,
                        CreatedDate = DateTime.Now
                    };
                    var res = InsertRequestResponse(AddResponse);

                    resdata = JsonConvert.DeserializeObject<AScoreAPIResponse>(jsonString);
                }
                else
                {
                    jsonString = (response.Content);
                    ArthmateReqResp AddResponse = new ArthmateReqResp()
                    {
                        ActivityMasterId = Convert.ToInt64(data.id),
                        LeadMasterId = Leadid,
                        RequestResponseMsg = jsonString,
                        Type = "Response",
                        Url = data.url,
                        CreatedBy = 0,
                        CreatedDate = DateTime.Now
                    };
                    var res = InsertRequestResponse(AddResponse);

                    resdata = JsonConvert.DeserializeObject<AScoreAPIResponse>(jsonString);
                }

                //var insertres = InsertLeadBackgroundRun(Leadid, "Request-A ScoreAPI", RequestjsonString, jsonString);
            }
            catch (Exception ex)
            {
                ArthmateReqResp addRequest = new ArthmateReqResp()
                {
                    ActivityMasterId = Convert.ToInt64(data.id),
                    LeadMasterId = Leadid,
                    RequestResponseMsg = ex.ToString(),
                    Type = "Response",
                    Url = data.url,
                    CreatedBy = 0,
                    CreatedDate = DateTime.Now
                };
                var resw = InsertRequestResponse(addRequest);
            }

            return resdata;
        }

        public async Task<AScoreResponseDc> GetAscoreApi(AScorePostDc scorereq)
        {
            AScoreResponseDc resdata = new AScoreResponseDc();

            GetAScorePostDc po = new GetAScorePostDc();
            po.request_id = scorereq.request_id;
            po.product = "UBLN";


            GetUrlTokenDc data = new GetUrlTokenDc();
            data = GetUrlTokenForApi("ArthMate", "GetAscoreApi");

            var RequestjsonString = JsonConvert.SerializeObject(po);
            try
            {
                var client = new RestClient(data.url);
                client.Timeout = -1;
                var request = new RestRequest(Method.POST);
                request.AddHeader("NoEncryption", "1");
                request.AddHeader("Content-Type", "application/json");
                request.AddHeader("Authorization", data.token);
                request.AddParameter("application/json", RequestjsonString, ParameterType.RequestBody);
                ServicePointManager.Expect100Continue = true;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                ArthmateReqResp addRequest = new ArthmateReqResp()
                {
                    ActivityMasterId = Convert.ToInt64(data.id),
                    LeadMasterId = scorereq.LeadMasterId,
                    RequestResponseMsg = RequestjsonString,
                    Type = "Request",
                    Url = data.url,
                    CreatedBy = 0,
                    CreatedDate = DateTime.Now
                };
                var reqsave = InsertRequestResponse(addRequest);
                IRestResponse response = await client.ExecuteAsync(request);
                string jsonString = "";
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    jsonString = (response.Content);
                    resdata = JsonConvert.DeserializeObject<AScoreResponseDc>(jsonString);
                }
                else
                {
                    jsonString = (response.Content);
                    resdata = JsonConvert.DeserializeObject<AScoreResponseDc>(jsonString);
                }
                ArthmateReqResp AddResponse = new ArthmateReqResp()
                {
                    ActivityMasterId = Convert.ToInt64(data.id),
                    LeadMasterId = scorereq.LeadMasterId,
                    RequestResponseMsg = jsonString,
                    Type = "Response",
                    Url = data.url,
                    CreatedBy = 0,
                    CreatedDate = DateTime.Now
                };
                var res = InsertRequestResponse(AddResponse);
                // var insertres = InsertLeadBackgroundRun(scorereq.LeadMasterId, "GetAscoreApi", RequestjsonString, jsonString);
            }
            catch (Exception ex)
            {
                ArthmateReqResp addRequest = new ArthmateReqResp()
                {
                    ActivityMasterId = Convert.ToInt64(data.id),
                    LeadMasterId = scorereq.LeadMasterId,
                    RequestResponseMsg = ex.ToString(),
                    Type = "Response",
                    Url = data.url,
                    CreatedBy = 0,
                    CreatedDate = DateTime.Now
                };
                var resw = InsertRequestResponse(addRequest);
            }

            return resdata;
        }
        #endregion

        //get loan api
        public async Task<GetLoanDetailsDc> GetLoanById(string loanappid, long Leadid, long userid)
        {
            TextFileLogHelper.TraceLog("GetLoanById start 1");

            GetLoanDetailsDc getloanres = new GetLoanDetailsDc();
            GetUrlTokenDc data = new GetUrlTokenDc();
            data = GetUrlTokenForApi("ArthMate", "GetLoan");

            try
            {
                //GET   //https://uat-apiorigin.arthmate.com/api/loan/{loan_id}
                var client = new RestClient(data.url + "/" + loanappid);
                client.Timeout = -1;
                var request = new RestRequest(Method.GET);
                request.AddHeader("NoEncryption", "1");
                request.AddHeader("Content-Type", "application/json");
                request.AddHeader("Authorization", data.token);
                ServicePointManager.Expect100Continue = true;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                ArthmateReqResp addRequest = new ArthmateReqResp()
                {
                    ActivityMasterId = Convert.ToInt64(data.id),
                    LeadMasterId = Leadid,
                    RequestResponseMsg = loanappid,
                    Type = "Request",
                    Url = data.url,
                    CreatedBy = 0,
                    CreatedDate = DateTime.Now
                };
                var reqsave = InsertRequestResponse(addRequest);
                IRestResponse response = await client.ExecuteAsync(request);

                TextFileLogHelper.TraceLog("GetLoanById response 2 =" + response.ToString());

                string jsonString = "";
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    jsonString = (response.Content);
                    ArthmateReqResp AddResponse = new ArthmateReqResp()
                    {
                        ActivityMasterId = Convert.ToInt64(data.id),
                        LeadMasterId = Leadid,
                        RequestResponseMsg = jsonString,
                        Type = "Response",
                        Url = data.url,
                        CreatedBy = 0,
                        CreatedDate = DateTime.Now
                    };
                    var res = InsertRequestResponse(AddResponse);
                    getloanres = JsonConvert.DeserializeObject<GetLoanDetailsDc>(jsonString);
                }
                else
                {
                    jsonString = (response.Content);
                    ArthmateReqResp AddResponse = new ArthmateReqResp()
                    {
                        ActivityMasterId = Convert.ToInt64(data.id),
                        LeadMasterId = Leadid,
                        RequestResponseMsg = jsonString,
                        Type = "Response",
                        Url = data.url,
                        CreatedBy = 0,
                        CreatedDate = DateTime.Now
                    };
                    var res = InsertRequestResponse(AddResponse);
                    getloanres = JsonConvert.DeserializeObject<GetLoanDetailsDc>(jsonString);
                }

            }
            catch (Exception ex)
            {
                ArthmateReqResp addRequest = new ArthmateReqResp()
                {
                    ActivityMasterId = Convert.ToInt64(data.id),
                    LeadMasterId = Leadid,
                    RequestResponseMsg = ex.ToString(),
                    Type = "Response",
                    Url = data.url,
                    CreatedBy = 0,
                    CreatedDate = DateTime.Now
                };
                var resw = InsertRequestResponse(addRequest);
                TextFileLogHelper.TraceLog("GetLoanById catch block 3 =" + ex.ToString());
            }

            return getloanres;
        }

        #region Aadhar api
        // step first
        public async Task<AadharOtpGenerateRes> GenerateOtpToAcceptOffer(FirstAadharXMLPost AadharObj)
        {
            string url = "";
            string token = "";
            string CompanyCode = "";
            var configdata = GetApiConfig("ArthMate");
            var Activitydata = GetActivityMaster("AadharFirst");
            if (configdata != null && Activitydata != null)
            {
                url = configdata.URL + "/" + Activitydata.URL;
                token = Activitydata.TokenKey;
                CompanyCode = Convert.ToString(configdata.CompanyCode);
            }
            AadharOtpGenerateRes responseDc = new AadharOtpGenerateRes();

            var Aadhar = new FirstAadharXML_Post
            {
                aadhaar_no = AadharObj.aadhaar_no,
                loan_app_id = AadharObj.loan_app_id,
                consent = "Y",
                consent_timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
            };

            try
            {
                var RequestjsonString = JsonConvert.SerializeObject(Aadhar);
                var client = new RestClient(url);
                client.Timeout = -1;
                var request = new RestRequest(Method.POST);
                request.AddHeader("NoEncryption", "1");
                request.AddHeader("Content-Type", "application/json");
                request.AddHeader("Authorization", token);
                request.AddParameter("application/json", RequestjsonString, ParameterType.RequestBody);
                ServicePointManager.Expect100Continue = true;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                ArthmateReqResp addRequest = new ArthmateReqResp()
                {
                    ActivityMasterId = Activitydata.Id,
                    LeadMasterId = AadharObj.LeadId,
                    RequestResponseMsg = RequestjsonString,
                    Type = "Request",
                    Url = url,
                    CreatedBy = (int)AadharObj.LeadId,
                    CreatedDate = DateTime.Now
                };
                var reqsave = InsertRequestResponse(addRequest);
                IRestResponse response = await client.ExecuteAsync(request);
                string jsonString = "";
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    jsonString = (response.Content);
                    responseDc = JsonConvert.DeserializeObject<AadharOtpGenerateRes>(jsonString);
                }
                else
                {
                    jsonString = (response.Content);
                    responseDc = JsonConvert.DeserializeObject<AadharOtpGenerateRes>(jsonString);
                }
                ArthmateReqResp AddResponse = new ArthmateReqResp()
                {
                    ActivityMasterId = Activitydata.Id,
                    LeadMasterId = AadharObj.LeadId,
                    RequestResponseMsg = jsonString,
                    Type = "Response",
                    Url = url,
                    CreatedBy = (int)AadharObj.LeadId,
                    CreatedDate = DateTime.Now
                };
                var res = InsertRequestResponse(AddResponse);
            }
            catch (Exception ex)
            {
                ArthmateReqResp addRequest = new ArthmateReqResp()
                {
                    ActivityMasterId = Activitydata.Id,
                    LeadMasterId = AadharObj.LeadId,
                    RequestResponseMsg = ex.Message.ToString(),
                    Type = "Response",
                    Url = url,
                    CreatedBy = (int)AadharObj.LeadId,
                    CreatedDate = DateTime.Now
                };
            }

            return responseDc;
        }
        public async Task<SecondAadharXMLResponseDc> StepTwoAadharApi(SecondAadharXMLPost AadharObj)
        {
            string url = "";
            string token = "";
            string CompanyCode = "";
            var configdata = GetApiConfig("ArthMate");
            var Activitydata = GetActivityMaster("AadharTwo");
            if (configdata != null && Activitydata != null)
            {
                url = configdata.URL + "/" + Activitydata.URL;
                token = Activitydata.TokenKey;
                CompanyCode = Convert.ToString(configdata.CompanyCode);
            }
            SecondAadharXMLResponseDc responseDc = new SecondAadharXMLResponseDc();

            SecondAadharXMLPostDc Aadhar = new SecondAadharXMLPostDc
            {
                aadhaar_no = AadharObj.aadhaar_no,
                loan_app_id = AadharObj.loan_app_id,
                request_id = AadharObj.request_id,
                otp = AadharObj.otp,
                consent = "Y",
                consent_timestamp = indianTime.ToString("yyyy-MM-dd HH:mm:ss")
            };

            try
            {
                {
                    var RequestjsonString = JsonConvert.SerializeObject(Aadhar);
                    var client = new RestClient(url);
                    client.Timeout = -1;
                    var request = new RestRequest(Method.POST);
                    request.AddHeader("NoEncryption", "1");
                    request.AddHeader("Content-Type", "application/json");
                    request.AddHeader("Authorization", token);
                    request.AddParameter("application/json", RequestjsonString, ParameterType.RequestBody);
                    ServicePointManager.Expect100Continue = true;
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                    ArthmateReqResp addRequest = new ArthmateReqResp()
                    {
                        ActivityMasterId = Activitydata.Id,
                        LeadMasterId = AadharObj.LeadId,
                        RequestResponseMsg = RequestjsonString,
                        Type = "Request",
                        Url = url,
                        CreatedBy = (int)AadharObj.LeadId,
                        CreatedDate = DateTime.Now
                    };
                    var reqsave = InsertRequestResponse(addRequest);
                    string jsonString = string.Empty;
                    IRestResponse response = await client.ExecuteAsync(request);
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        jsonString = (response.Content);
                        responseDc = JsonConvert.DeserializeObject<SecondAadharXMLResponseDc>(jsonString);
                        responseDc.KYCResponse = jsonString;
                    }
                    else
                    {
                        jsonString = (response.Content);
                        responseDc = JsonConvert.DeserializeObject<SecondAadharXMLResponseDc>(jsonString);
                        responseDc.KYCResponse = jsonString;
                    }
                    ArthmateReqResp AddResponse = new ArthmateReqResp()
                    {
                        ActivityMasterId = Activitydata.Id,
                        LeadMasterId = AadharObj.LeadId,
                        RequestResponseMsg = jsonString,
                        Type = "Response",
                        Url = url,
                        CreatedBy = (int)AadharObj.LeadId,
                        CreatedDate = DateTime.Now
                    };
                    var res = InsertRequestResponse(AddResponse);
                };
            }
            catch (Exception ex)
            {
                ArthmateReqResp addRequest = new ArthmateReqResp()
                {
                    ActivityMasterId = Activitydata.Id,
                    LeadMasterId = AadharObj.LeadId,
                    RequestResponseMsg = ex.ToString(),
                    Type = "Response",
                    Url = url,
                    CreatedBy = (int)AadharObj.LeadId,
                    CreatedDate = DateTime.Now
                };
            }
            return responseDc;
        }
        #endregion

        #region Karza Pan verification
        public async Task<ValidAuthenticationPanResDc> ValidAuthenticationPan(string PanNumber, long LeadMasterId)
        {
            ValidAuthenticationPanResDc result = new ValidAuthenticationPanResDc();
            var apiConfigdata = GetUrlTokenForApi("Karza", "KarzaValidAuthenticationPan");

            if (apiConfigdata != null)
            {
                ArthmateReqResp addresponse = null;
                try
                {
                    using (var httpClient = new HttpClient())
                    {
                        using (var request = new HttpRequestMessage(new HttpMethod("POST"), apiConfigdata.url))
                        {
                            request.Headers.TryAddWithoutValidation("x-karza-key", apiConfigdata.ApiSecretKey);
                            request.Headers.TryAddWithoutValidation("cache-control", "no-cache");

                            ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;

                            ValidAuthenticationPanPost reqdc = new ValidAuthenticationPanPost();
                            reqdc.pan = PanNumber;
                            reqdc.consent = "Y";
                            string jsonstringReq = JsonConvert.SerializeObject(reqdc);
                            request.Content = new StringContent(jsonstringReq);
                            request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");

                            ArthmateReqResp addRequest = new ArthmateReqResp()
                            {
                                ActivityMasterId = apiConfigdata.id,
                                LeadMasterId = LeadMasterId,
                                RequestResponseMsg = jsonstringReq,
                                Type = "Request",
                                Url = apiConfigdata.url,
                                CreatedBy = 0,
                                CreatedDate = DateTime.Now
                            };
                            InsertRequestResponse(addRequest);
                            var response = await httpClient.SendAsync(request);
                            string jsonString = string.Empty;
                            if (response.StatusCode == HttpStatusCode.OK)
                            {
                                jsonString = (await response.Content.ReadAsStringAsync()).Replace("\\", "").Trim(new char[1] { '"' });
                                result = JsonConvert.DeserializeObject<ValidAuthenticationPanResDc>(jsonString);
                                if (result.StatusCode != 101)
                                    result.error = await CheckAuthenticationError(result.StatusCode);
                            }
                            else if (response.StatusCode == HttpStatusCode.BadRequest)
                            {
                                result.error = "Your request is invalid.";
                                jsonString = result.error;
                            }
                            else if (response.StatusCode == HttpStatusCode.Unauthorized)
                            {
                                result.error = "please try again,after sometime.";
                                jsonString = result.error;
                            }
                            else if (response.StatusCode == HttpStatusCode.Forbidden)
                            {
                                result.error = "The API requested is hidden for administrators only.";
                                jsonString = result.error;
                            }
                            else if (response.StatusCode == HttpStatusCode.NotFound)
                            {
                                result.error = "The specified API could not be found.";
                                jsonString = result.error;
                            }
                            else if (response.StatusCode == HttpStatusCode.InternalServerError)
                            {
                                result.error = "We had a problem with our server.Try again later.";
                                jsonString = result.error;
                            }
                            else if (response.StatusCode == HttpStatusCode.BadGateway || response.StatusCode == HttpStatusCode.GatewayTimeout || response.StatusCode == HttpStatusCode.RequestTimeout)
                            {
                                result.error = "please try again";
                                jsonString = response.StatusCode.ToString();
                            }
                            else if (response.StatusCode == HttpStatusCode.ServiceUnavailable)
                            {
                                result.error = "please try again,after sometime";
                                jsonString = result.error;
                            }
                            else
                            {
                                jsonString = (await response.Content.ReadAsStringAsync());
                                result.error = jsonString;
                            }
                            addresponse = new ArthmateReqResp()
                            {
                                ActivityMasterId = apiConfigdata.id,
                                LeadMasterId = LeadMasterId,
                                RequestResponseMsg = jsonString,
                                Type = "Response",
                                Url = apiConfigdata.url,
                                CreatedBy = 0,
                                CreatedDate = DateTime.Now
                            };
                        }
                    }
                }
                catch (Exception ex)
                {
                    addresponse = new ArthmateReqResp()
                    {
                        ActivityMasterId = apiConfigdata.id,
                        LeadMasterId = LeadMasterId,
                        RequestResponseMsg = ex.ToString(),
                        Type = "Response",
                        Url = apiConfigdata.url,
                        CreatedBy = 0,
                        CreatedDate = DateTime.Now
                    };
                    result.error = ex.Message.ToString();
                }
                InsertRequestResponse(addresponse);
            }
            return result;
        }



        //new Pan Karza Api 22-03-2024




        public async Task<PanOcrResDc> PanVerificationWithOCRAsync(string imgurl, string request_id, long LeadMasterId)
        {

            PanOcrResDc responseDC = new PanOcrResDc();
            var apiConfigdata = GetUrlTokenForApi("Karza", "KarzaKycOcr");
            if (apiConfigdata != null)
            {
                ArthmateReqResp addresponse = null;
                try
                {
                    using (var httpClient = new HttpClient())
                    {
                        using (var request = new HttpRequestMessage(new HttpMethod("POST"), apiConfigdata.url))
                        {
                            request.Headers.TryAddWithoutValidation("x-karza-key", apiConfigdata.ApiSecretKey);
                            request.Headers.TryAddWithoutValidation("cache-control", "no-cache");

                            OcrPostDc OcrPost = new OcrPostDc();
                            OcrPost.url = string.Format("{0}://{1}{2}/{3}", new Uri((HttpContext.Current.Request.UrlReferrer != null ? HttpContext.Current.Request.UrlReferrer.AbsoluteUri : HttpContext.Current.Request.Url.AbsoluteUri)).Scheme
                                                                 , HttpContext.Current.Request.Url.DnsSafeHost
                                                                 , (HttpContext.Current.Request.Url.Port != 80 && HttpContext.Current.Request.Url.Port != 443 ? ":" + HttpContext.Current.Request.Url.Port : "")
                                                                 , imgurl);
                            OcrPost.docType = "file";
                            OcrPost.conf = true;
                            string jsonstringReq = JsonConvert.SerializeObject(OcrPost);
                            request.Content = new StringContent(jsonstringReq);
                            request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");

                            ArthmateReqResp addRequest = new ArthmateReqResp()
                            {
                                ActivityMasterId = apiConfigdata.id,
                                LeadMasterId = LeadMasterId,
                                RequestResponseMsg = jsonstringReq,
                                Type = "Request",
                                Url = apiConfigdata.url,
                                CreatedBy = 0,
                                CreatedDate = DateTime.Now
                            };
                            var req = InsertRequestResponse(addRequest);

                            string jsonString = string.Empty;
                            var response = await httpClient.SendAsync(request);
                            if (response.StatusCode == HttpStatusCode.OK)
                            {
                                jsonString = (await response.Content.ReadAsStringAsync());
                                responseDC = JsonConvert.DeserializeObject<PanOcrResDc>(jsonString);
                                if (responseDC.statusCode != 101)
                                    responseDC.error = await CheckOcrError(responseDC.statusCode);
                                if (responseDC.statusCode == 101)
                                {
                                    DateTime bdate = DateTime.Now;
                                    try
                                    {
                                        if (responseDC.result.FirstOrDefault(c => c.type == "Pan") != null)
                                        {
                                            bdate = Convert.ToDateTime(DateTime.ParseExact(responseDC.result.FirstOrDefault(c => c.type == "Pan").details.date.value, "dd/MM/yyyy", CultureInfo.InvariantCulture));
                                        }
                                    }
                                    catch (Exception es)
                                    {
                                        //TextFileLogHelper.LogError("Karza ocr date format : error " + responseDC.result.FirstOrDefault(c => c.type == "Pan").details.date.value);
                                        bdate = Convert.ToDateTime(DateTime.ParseExact(responseDC.result.FirstOrDefault(c => c.type == "Pan").details.date.value, "dd-MM-yyyy", CultureInfo.InvariantCulture));
                                    }
                                    if (responseDC.result.FirstOrDefault(c => c.type == "Pan") != null)
                                    {
                                        var other = new PanOcrOtherInfoDc
                                        {
                                            age = Convert.ToInt32(DateTime.Now.Year - bdate.Year),
                                            date_of_birth = responseDC.result.FirstOrDefault(c => c.type == "Pan").details.date.value,
                                            fathers_name = responseDC.result.FirstOrDefault(c => c.type == "Pan").details.father.value,
                                            name_on_card = responseDC.result.FirstOrDefault(c => c.type == "Pan").details.name.value,
                                            id_number = responseDC.result.FirstOrDefault(c => c.type == "Pan").details.panNo.value,
                                        };
                                        responseDC.OtherInfo = other;
                                    }
                                    else
                                    {
                                        responseDC.error = "Document Not Valid";
                                        jsonString = response.StatusCode.ToString();
                                    }
                                }
                            }

                            else if (response.StatusCode == HttpStatusCode.BadGateway || response.StatusCode == HttpStatusCode.GatewayTimeout || response.StatusCode == HttpStatusCode.RequestTimeout)
                            {
                                responseDC.error = "please try again";
                                jsonString = response.StatusCode.ToString();
                            }
                            else if (response.StatusCode == HttpStatusCode.ServiceUnavailable)
                            {

                                responseDC.error = "please try again,after sometime";
                                jsonString = responseDC.error;
                            }

                            else if (response.StatusCode == HttpStatusCode.BadRequest)
                            {
                                responseDC.error = "your request is invalid.";
                                jsonString = responseDC.error;
                            }
                            else if (response.StatusCode == HttpStatusCode.Unauthorized)
                            {
                                responseDC.error = "please try again,after sometime.";
                                jsonString = responseDC.error;
                            }
                            else if (response.StatusCode == HttpStatusCode.Forbidden || response.StatusCode == HttpStatusCode.NotFound)
                            {
                                responseDC.error = "something went wrong.";
                                jsonString = responseDC.error;
                            }
                            else if (response.StatusCode == HttpStatusCode.InternalServerError)
                            {
                                responseDC.error = "we had a problem with our server. try again later.";
                                jsonString = responseDC.error;
                            }
                            else if (response.StatusCode == HttpStatusCode.PaymentRequired)
                            {
                                responseDC.error = "Payment Required.";
                                jsonString = responseDC.error;
                            }

                            addresponse = new ArthmateReqResp()
                            {
                                ActivityMasterId = apiConfigdata.id,
                                LeadMasterId = LeadMasterId,
                                RequestResponseMsg = jsonString,
                                Type = "Response",
                                Url = apiConfigdata.url,
                                CreatedBy = 0,
                                CreatedDate = DateTime.Now
                            };
                        }
                    }
                }
                catch (Exception ex)
                {
                    addresponse = new ArthmateReqResp()
                    {
                        ActivityMasterId = apiConfigdata.id,
                        LeadMasterId = LeadMasterId,
                        RequestResponseMsg = ex.Message.ToString(),
                        Type = "Response",
                        Url = apiConfigdata.url,
                        CreatedBy = 0,
                        CreatedDate = DateTime.Now
                    };
                    responseDC.error = ex.Message.ToString();
                    //TextFileLogHelper.TraceLog("Error in PanVerificationWithOCRAsync ...." + ex.Message.ToString());
                }
                InsertRequestResponse(addresponse);
            }
            return responseDC;
        }
        public async Task<string> CheckOcrError(int statuscode)
        {
            string errormsg = null;
            if (statuscode == 102)
                errormsg = "No KYC Document identified";
            else if (statuscode == 103)
                errormsg = "Image Format Not Supported OR Size Exceeds 6MB";
            else if (statuscode == 104 || statuscode == 105 || statuscode == 106 || statuscode == 107 || statuscode == 108)
                errormsg = "N/A";
            return errormsg;
        }
        public async Task<string> CheckAuthenticationError(int statuscode)
        {
            string errormsg = null;
            if (statuscode == 102)
                errormsg = "Invalid ID number or combination of inputs";
            else if (statuscode == 103)
                errormsg = "No records found for the given ID or combination of inputs";
            else if (statuscode == 104)
                errormsg = "Max retries exceeded";
            else if (statuscode == 105)
                errormsg = "Missing Consent";
            else if (statuscode == 106)
                errormsg = "Multiple Records Exist";
            else if (statuscode == 107)
                errormsg = "Not Supported";
            else if (statuscode == 108)
                errormsg = "Internal Resource Unavailable";
            else if (statuscode == 108)
                errormsg = "Too many records Found";
            return errormsg;
        }
        #endregion

        public async Task<RepaymentScheduleResponseDc> RepaymentSchedule(RepaymentScheduleDc repayment)
        {
            TextFileLogHelper.TraceLog("RepaymentSchedule start 1");
            RepaymentScheduleResponseDc resdata = new RepaymentScheduleResponseDc();

            GetUrlTokenDc data = new GetUrlTokenDc();
            data = GetUrlTokenForApi("ArthMate", "RepaymentSchedule");

            try
            {
                var client = new RestClient(data.url + "/" + repayment.loan_id);
                client.Timeout = -1;
                var request = new RestRequest(Method.GET);
                request.AddHeader("NoEncryption", "1");
                request.AddHeader("Content-Type", "application/json");
                request.AddHeader("Authorization", data.token);
                ServicePointManager.Expect100Continue = true;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                ArthmateReqResp addRequest = new ArthmateReqResp()
                {
                    ActivityMasterId = Convert.ToInt64(data.id),
                    LeadMasterId = repayment.LeadMasterId,
                    RequestResponseMsg = repayment.loan_id,
                    Type = "Request",
                    Url = data.url,
                    CreatedBy = 0,
                    CreatedDate = DateTime.Now
                };
                var reqsave = InsertRequestResponse(addRequest);
                IRestResponse response = await client.ExecuteAsync(request);

                TextFileLogHelper.TraceLog("RepaymentSchedule response " + response.ToString());

                string jsonString = "";
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    jsonString = (response.Content);
                    RepaymentScheduleResponseDc dd = JsonConvert.DeserializeObject<RepaymentScheduleResponseDc>(jsonString);
                }
                else
                {
                    jsonString = (response.Content);
                    RepaymentScheduleResponseDc dt = JsonConvert.DeserializeObject<RepaymentScheduleResponseDc>(jsonString);
                }
                ArthmateReqResp AddResponse = new ArthmateReqResp()
                {
                    ActivityMasterId = Convert.ToInt64(data.id),
                    LeadMasterId = repayment.LeadMasterId,
                    RequestResponseMsg = jsonString,
                    Type = "Response",
                    Url = data.url,
                    CreatedBy = 0,
                    CreatedDate = DateTime.Now
                };
                var res = InsertRequestResponse(AddResponse);
            }
            catch (Exception ex)
            {
                ArthmateReqResp addRequest = new ArthmateReqResp()
                {
                    ActivityMasterId = Convert.ToInt64(data.id),
                    LeadMasterId = repayment.LeadMasterId,
                    RequestResponseMsg = ex.ToString(),
                    Type = "Response",
                    Url = data.url,
                    CreatedBy = 0,
                    CreatedDate = DateTime.Now
                };
                var resw = InsertRequestResponse(addRequest);
                TextFileLogHelper.TraceLog("RepaymentSchedule catch block " + ex.ToString());
            }

            return resdata;
        }

        public bool AddLeadActivityHistories(LeadActivityHistoriesDc leadActivityHistoriesDc, int userid, AuthContext authContext)
        {
            LeadActivityHistory leadActivityHistory = new LeadActivityHistory
            {
                ApprovalStatus = leadActivityHistoriesDc.ApprovalStatus,
                LeadLoanDocumentId = leadActivityHistoriesDc.LeadLoanDocumentId,
                LeadMasteId = leadActivityHistoriesDc.LeadMasteId,
                Message = leadActivityHistoriesDc.Message,
                CreatedBy = userid,
                CreatedDate = DateTime.Now,
                IsActive = true,
                IsDeleted = false,
                Sequence = leadActivityHistoriesDc.Sequence,
                Activity = leadActivityHistoriesDc.Activity
            };
            authContext.LeadActivityHistories.Add(leadActivityHistory);
            return true;
        }
        //Pdf Reports Api
        //Ceplr Basic Report
        public async Task<CeplrBasicReportResponse> CeplrBasicReport(CeplrBasicReportDc basicdata, string customer_id, long LeadmasterId)
        {
            string url = "";
            string ApiSecretKey = "";
            //CommonResponseDc resdata = new CommonResponseDc();
            CeplrBasicReportResponse resdata = new CeplrBasicReportResponse();

            //CeplrBasicReportDc BasicReportDc = new CeplrBasicReportDc();
            //BasicReportDc.start_date = basicdata.start_date;
            //BasicReportDc.end_date = basicdata.end_date;

            var configdata = GetApiConfig("Cepler");
            var Activitydata = GetActivityMaster("Basic Reports");
            if (configdata != null && Activitydata != null)
            {
                url = configdata.URL + "customers/" + customer_id + "/report";
                ApiSecretKey = Activitydata.ApiSecretKey;
            }
            var RequestjsonString = JsonConvert.SerializeObject(basicdata);
            try
            {
                var client = new RestClient(url);
                client.Timeout = -1;
                var request = new RestRequest(Method.POST);
                request.AddHeader("NoEncryption", "1");
                request.AddHeader("Content-Type", "application/json");
                request.AddParameter("application/json", RequestjsonString, ParameterType.RequestBody);
                request.AddHeader("x-api-key", ApiSecretKey);

                ServicePointManager.Expect100Continue = true;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                ArthmateReqResp addRequest = new ArthmateReqResp()
                {
                    ActivityMasterId = Activitydata.Id,
                    LeadMasterId = LeadmasterId,
                    RequestResponseMsg = RequestjsonString,
                    Type = "Request",
                    Url = url,
                    CreatedBy = 0,
                    CreatedDate = DateTime.Now
                };
                var reqsave = InsertRequestResponse(addRequest);
                IRestResponse response = await client.ExecuteAsync(request);
                string jsonString = "";
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    jsonString = (response.Content);
                    ArthmateReqResp AddResponse = new ArthmateReqResp()
                    {
                        ActivityMasterId = Activitydata.Id,
                        LeadMasterId = LeadmasterId,
                        RequestResponseMsg = jsonString,
                        Type = "Response",
                        Url = url,
                        CreatedBy = 0,
                        CreatedDate = DateTime.Now
                    };
                    var res = InsertRequestResponse(AddResponse);

                    resdata = JsonConvert.DeserializeObject<CeplrBasicReportResponse>(jsonString);

                }
                else
                {
                    jsonString = (response.Content);
                    ArthmateReqResp AddResponse = new ArthmateReqResp()
                    {
                        ActivityMasterId = Activitydata.Id,
                        LeadMasterId = LeadmasterId,
                        RequestResponseMsg = jsonString,
                        Type = "Response",
                        Url = url,
                        CreatedBy = 0,
                        CreatedDate = DateTime.Now
                    };
                    var res = InsertRequestResponse(AddResponse);

                    resdata = JsonConvert.DeserializeObject<CeplrBasicReportResponse>(jsonString);
                }

                //var data = InsertLeadBackgroundRun(LeadmasterId, "Basic Reports", RequestjsonString, jsonString);

            }
            catch (Exception ex)
            {
                ArthmateReqResp addRequest = new ArthmateReqResp()
                {
                    ActivityMasterId = Activitydata.Id,
                    LeadMasterId = LeadmasterId,
                    RequestResponseMsg = ex.Message.ToString(),
                    Type = "Response",
                    Url = url,
                    CreatedBy = 0,
                    CreatedDate = DateTime.Now
                };
                var resw = InsertRequestResponse(addRequest);


            }

            return resdata;
        }
        // Get MonthlySalary by ceplr basic report
        public async Task<double> GetMonthlySalary(long LeadMasterId, AuthContext db)
        {
            try
            {
                //TextFileLogHelper.TraceLog("GetMonthlySalary- Inside=Start:Phase 2 ");

                double MonthlySalary = 0;
                var ceplr_cust = db.CeplrPdfReports.Where(x => x.LeadMasterId == LeadMasterId && x.IsActive == true && x.IsDeleted == false).OrderByDescending(x => x.Id).FirstOrDefault();
                //TextFileLogHelper.TraceLog("GetMonthlySalary- Inside=Start:Phase 3 ");
                CeplrBasicReportDc CplrBasic = new CeplrBasicReportDc();
                CplrBasic.start_date = ceplr_cust.CreatedDate.AddMonths(-6).AddDays(-10).ToString("yyyy-MM-dd");
                CplrBasic.end_date = DateTime.Now.ToString("yyyy-MM-dd");
                //TextFileLogHelper.TraceLog("GetMonthlySalary- Inside=Start:Phase 4 ");
                ArthMateHelper ArthMateHelper = new ArthMateHelper();
                var BasicReport = await ArthMateHelper.CeplrBasicReport(CplrBasic, ceplr_cust.customer_id, LeadMasterId);
                if (BasicReport != null)
                {
                    TextFileLogHelper.TraceLog("GetMonthlySalary- Inside=Start:Phase 5");
                    if (BasicReport.data != null)
                    {
                        //TextFileLogHelper.TraceLog("GetMonthlySalary- Inside=Start:Phase 6 ");
                        MonthlySalary = Convert.ToDouble(BasicReport.data[0].analytics.salary_summary.total_salary);
                        //TextFileLogHelper.TraceLog("GetMonthlySalary- Inside=Start:Phase 7");
                    }
                }
                return MonthlySalary;
            }
            catch (Exception ex)
            {
                TextFileLogHelper.TraceLog("GetMonthlySalary- Inside=Error " + ex.Message);
                return 0;
            }
        }


        public Dictionary<string, string> CKYCImageCodeDescription()
        {
            Dictionary<string, string> dic = new Dictionary<string, string>();
            dic.Add("02", "Photograph");
            dic.Add("03", "Pan");
            dic.Add("04", "Proof of Possession of Aadhaar");
            dic.Add("05", "Passport");
            dic.Add("06", "Driving License");
            dic.Add("07", "Voters Identity Card");
            dic.Add("08", "Signature");
            dic.Add("09", "NREGA Job Card");
            dic.Add("10", "Simplified Measures Account"); //	Simplified Measures Account - Identity card with applicant’s photograph issued by Central/ State Government Departments, Statutory/ Regulatory Authorities, Public Sector Undertakings, Scheduled Commercial Banks, and Public Financial Institutions.
            return dic;
        }

        public Dictionary<string, string> CrifGenderCodes()
        {
            var gendercode = new Dictionary<string, string>();
            gendercode.Add("G01", "MALE");
            gendercode.Add("G02", "FEMALE");
            gendercode.Add("G03", "TRANSGENDER");
            return gendercode;

        }

        public Dictionary<int, string> CrifEnquiryStage()
        {
            var EnquiryStage = new Dictionary<int, string>();
            EnquiryStage.Add(1, "PRE-SCREEN");
            EnquiryStage.Add(2, "PRE-DISB");
            EnquiryStage.Add(3, "UW-REVIEW");
            EnquiryStage.Add(4, "COLLECTION");
            EnquiryStage.Add(5, "RENEWAL");
            return EnquiryStage;

        }
        public Dictionary<int, string> CrifEnquiryPurpose()
        {
            var EnquiryPurpose = new Dictionary<int, string>();
            EnquiryPurpose.Add(1, "ACCT-ORIG");
            EnquiryPurpose.Add(2, "ACCT-MAINT");
            EnquiryPurpose.Add(3, "OTHER");
            return EnquiryPurpose;

        }


        #region Karza Aadhar verification
        public async Task<eAadhaarDigilockerResponseDc> eAdharDigilockerOTPXml(GetAdharDc UpdateAdhaarInfo)
        {
            TextFileLogHelper.TraceLog("eAdharDigilockerOTPXml start 1");

            eAadhaarDigilockerResponseDc responseDC = new eAadhaarDigilockerResponseDc();
            var apiConfigdata = GetUrlTokenForApi("Karza", "eAdharVerify");

            if (apiConfigdata != null)
            {
                try
                {
                    using (var httpClient = new HttpClient())
                    {
                        eAadhaarDigilockerRequestDc eAadhaarRequestDC = new eAadhaarDigilockerRequestDc();
                        eAadhaarRequestDC.consent = "Y";
                        eAadhaarRequestDC.aadhaarNo = UpdateAdhaarInfo.DocumentNumber;

                        using (var request = new HttpRequestMessage(new HttpMethod("POST"), apiConfigdata.url))
                        {

                            request.Headers.TryAddWithoutValidation("Content-Type", "application/json");
                            request.Headers.TryAddWithoutValidation("x-karza-key", apiConfigdata.ApiSecretKey);

                            ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;

                            string jsonstringReq = JsonConvert.SerializeObject(eAadhaarRequestDC);
                            request.Content = new StringContent(jsonstringReq);

                            ArthmateReqResp addRequest = new ArthmateReqResp()
                            {
                                ActivityMasterId = apiConfigdata.id,
                                LeadMasterId = UpdateAdhaarInfo.LeadMasterId,
                                RequestResponseMsg = jsonstringReq,
                                Type = "Request",
                                Url = apiConfigdata.url,
                                CreatedBy = 0,
                                CreatedDate = DateTime.Now
                            };
                            InsertRequestResponse(addRequest);

                            string jsonString = string.Empty;
                            var response = await httpClient.SendAsync(request);
                            TextFileLogHelper.TraceLog("eAdharDigilockerOTPXml response 2" + response.ToString());
                            if (response.StatusCode == HttpStatusCode.OK)
                            {
                                jsonString = (await response.Content.ReadAsStringAsync());
                                responseDC = JsonConvert.DeserializeObject<eAadhaarDigilockerResponseDc>(jsonString);
                            }
                            else if (response.StatusCode == HttpStatusCode.BadGateway || response.StatusCode == HttpStatusCode.GatewayTimeout || response.StatusCode == HttpStatusCode.RequestTimeout)
                            {
                                responseDC.error = new AdharDigiLocker.ErrorResponse
                                {
                                    error = new AdharDigiLocker.Error { message = "please try again" }
                                };
                                jsonString = response.StatusCode.ToString();
                            }
                            else if (response.StatusCode == HttpStatusCode.ServiceUnavailable)
                            {
                                responseDC.error = new AdharDigiLocker.ErrorResponse
                                {
                                    error = new AdharDigiLocker.Error { message = "please try again,after sometime" }
                                };
                                jsonString = response.StatusCode.ToString();
                            }
                            else if (response.StatusCode == HttpStatusCode.BadRequest)
                            {
                                responseDC.error = new AdharDigiLocker.ErrorResponse
                                {
                                    error = new AdharDigiLocker.Error { message = "Your request is invalid." }
                                };
                                jsonString = response.StatusCode.ToString();
                            }
                            else if (response.StatusCode == HttpStatusCode.Unauthorized)
                            {
                                responseDC.error = new AdharDigiLocker.ErrorResponse
                                {
                                    error = new AdharDigiLocker.Error { message = "please try again,after sometime." }
                                };
                                jsonString = response.StatusCode.ToString();
                            }
                            else if (response.StatusCode == HttpStatusCode.Forbidden)
                            {
                                responseDC.error = new AdharDigiLocker.ErrorResponse
                                {
                                    error = new AdharDigiLocker.Error { message = "The API requested is hidden for administrators only." }
                                };
                                jsonString = response.StatusCode.ToString();
                            }
                            else if (response.StatusCode == HttpStatusCode.NotFound)
                            {
                                responseDC.error = new AdharDigiLocker.ErrorResponse
                                {
                                    error = new AdharDigiLocker.Error { message = "The specified API could not be found." }
                                };
                                jsonString = response.StatusCode.ToString();
                            }
                            else if (response.StatusCode == HttpStatusCode.InternalServerError)
                            {
                                responseDC.error = new AdharDigiLocker.ErrorResponse
                                {
                                    error = new AdharDigiLocker.Error { message = "We had a problem with our server. Try again later." }
                                };
                                jsonString = response.StatusCode.ToString();
                            }
                            else
                            {
                                jsonString = (await response.Content.ReadAsStringAsync()).Replace("\\", "").Trim(new char[1] { '"' });
                                responseDC.error = new AdharDigiLocker.ErrorResponse
                                {
                                    error = new AdharDigiLocker.Error { message = "We had a problem with our server. Try again later." }
                                };
                                jsonString = response.StatusCode.ToString();
                            }
                            ArthmateReqResp addresponse = new ArthmateReqResp()
                            {
                                ActivityMasterId = apiConfigdata.id,
                                LeadMasterId = UpdateAdhaarInfo.LeadMasterId,
                                RequestResponseMsg = jsonString,
                                Type = "Response",
                                Url = apiConfigdata.url,
                                CreatedBy = 0,
                                CreatedDate = DateTime.Now
                            };
                            InsertRequestResponse(addresponse);
                        }
                    }
                }
                catch (Exception ex)
                {
                    ArthmateReqResp addRequest = new ArthmateReqResp()
                    {
                        ActivityMasterId = apiConfigdata.id,
                        LeadMasterId = UpdateAdhaarInfo.LeadMasterId,
                        RequestResponseMsg = ex.Message.ToString(),
                        Type = "Response",
                        Url = apiConfigdata.url,
                        CreatedBy = 0,
                        CreatedDate = DateTime.Now
                    };
                    InsertRequestResponse(addRequest);
                    TextFileLogHelper.TraceLog("eAdharDigilockerOTPXml catch block =" + ex.ToString());
                }
            }
            return responseDC;
        }
        public async Task<eAdhaarDigilockerVerifyOTPResponseDcXml> eAadharDigilockerVerifyOTPXml(UpdateAadhaarVerificationRequestDC verifyOtp)
        {
            TextFileLogHelper.TraceLog("eAadharDigilockerVerifyOTPXml start 1");
            eAdhaarDigilockerVerifyOTPResponseDcXml responseDC = new eAdhaarDigilockerVerifyOTPResponseDcXml();

            var apiConfigdata = GetUrlTokenForApi("Karza", "eAdharOtpVerify");
            if (apiConfigdata != null)
            {
                try
                {
                    using (var httpClient = new HttpClient())
                    {
                        //-----------------------------------------------------------------
                        //string person_Id = verifyOtp.personId;

                        eAadhaarDigilockerRequesTVerifyOTPDCXml eAadhaarRequestDC = new eAadhaarDigilockerRequesTVerifyOTPDCXml();

                        //eAadhaarRequestDC.otp = verifyOtp.otp;
                        //eAadhaarRequestDC.requestId = verifyOtp.requestId;
                        ////string EncryptAadhar = EncryptDecrypt.EncodeStringToBase64(verifyOtp.UpdateAdhaarInfoDc.AadharNo);
                        ////commented by anjali
                        //eAadhaarRequestDC.AadharNo = verifyOtp.aadhaarNo;
                        //eAadhaarRequestDC.consent = "Y";

                        eAadhaarRequestDC.otp = verifyOtp.otp;
                        eAadhaarRequestDC.requestId = verifyOtp.requestId;
                        //string EncryptAadhar = EncryptDecrypt.EncodeStringToBase64(verifyOtp.UpdateAdhaarInfoDc.AadharNo);
                        //commented by anjali
                        eAadhaarRequestDC.aadhaarNo = verifyOtp.aadhaarNo;
                        eAadhaarRequestDC.consent = "Y";

                        using (var request = new HttpRequestMessage(new HttpMethod("POST"), apiConfigdata.url))
                        {
                            request.Headers.TryAddWithoutValidation("Content-Type", "application/json");
                            request.Headers.TryAddWithoutValidation("x-karza-key", apiConfigdata.ApiSecretKey);
                            //request.Headers.TryAddWithoutValidation("x-karza-key", "931h46xzlv1GT41ktdhy");
                            ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;

                            string jsonstringReq = JsonConvert.SerializeObject(eAadhaarRequestDC);

                            TextFileLogHelper.TraceLog("eAadharDigilockerVerifyOTPXml request 2 =" + jsonstringReq);

                            request.Content = new StringContent(jsonstringReq);

                            ArthmateReqResp addRequest = new ArthmateReqResp()
                            {
                                ActivityMasterId = apiConfigdata.id,
                                LeadMasterId = verifyOtp.LeadMasterId,
                                RequestResponseMsg = jsonstringReq,
                                Type = "Request",
                                Url = apiConfigdata.url,
                                CreatedBy = 0,
                                CreatedDate = DateTime.Now
                            };
                            var req = InsertRequestResponse(addRequest);

                            string jsonString = string.Empty;
                            var response = await httpClient.SendAsync(request);

                            TextFileLogHelper.TraceLog("eAadharDigilockerVerifyOTPXml request 2 =" + response.ToString());

                            if (response.StatusCode == HttpStatusCode.OK)
                            {
                                jsonString = (await response.Content.ReadAsStringAsync());
                                responseDC = JsonConvert.DeserializeObject<eAdhaarDigilockerVerifyOTPResponseDcXml>(jsonString);
                            }
                            else if (response.StatusCode == HttpStatusCode.BadGateway || response.StatusCode == HttpStatusCode.GatewayTimeout || response.StatusCode == HttpStatusCode.RequestTimeout)
                            {
                                responseDC.error = new AdharDigiLocker.ErrorResponse
                                {
                                    error = new AdharDigiLocker.Error { message = "please try again" }
                                };
                                jsonString = response.StatusCode.ToString();
                            }
                            else if (response.StatusCode == HttpStatusCode.ServiceUnavailable)
                            {
                                responseDC.error = new AdharDigiLocker.ErrorResponse
                                {
                                    error = new AdharDigiLocker.Error { message = "please try again,after sometime" }
                                };
                                jsonString = response.StatusCode.ToString();
                            }
                            else if (response.StatusCode == HttpStatusCode.BadRequest)
                            {
                                responseDC.error = new AdharDigiLocker.ErrorResponse
                                {
                                    error = new AdharDigiLocker.Error { message = "Your request is invalid." }
                                };
                                jsonString = response.StatusCode.ToString();
                            }
                            else if (response.StatusCode == HttpStatusCode.Unauthorized)
                            {
                                responseDC.error = new AdharDigiLocker.ErrorResponse
                                {
                                    error = new AdharDigiLocker.Error { message = "please try again,after sometime." }
                                };
                                jsonString = response.StatusCode.ToString();
                            }
                            else if (response.StatusCode == HttpStatusCode.Forbidden)
                            {
                                responseDC.error = new AdharDigiLocker.ErrorResponse
                                {
                                    error = new AdharDigiLocker.Error { message = "The API requested is hidden for administrators only." }
                                };
                                jsonString = response.StatusCode.ToString();
                            }
                            else if (response.StatusCode == HttpStatusCode.NotFound)
                            {
                                responseDC.error = new AdharDigiLocker.ErrorResponse
                                {
                                    error = new AdharDigiLocker.Error { message = "The specified API could not be found." }
                                };
                                jsonString = response.StatusCode.ToString();
                            }
                            else if (response.StatusCode == HttpStatusCode.InternalServerError)
                            {
                                responseDC.error = new AdharDigiLocker.ErrorResponse
                                {
                                    error = new AdharDigiLocker.Error { message = "We had a problem with our server. Try again later." }
                                };
                                jsonString = response.StatusCode.ToString();
                            }
                            else
                            {
                                jsonString = (await response.Content.ReadAsStringAsync()).Replace("\\", "").Trim(new char[1] { '"' });
                                responseDC.error = JsonConvert.DeserializeObject<AdharDigiLocker.ErrorResponse>(jsonString);
                            }
                            ArthmateReqResp addresponse = new ArthmateReqResp()
                            {
                                ActivityMasterId = apiConfigdata.id,
                                LeadMasterId = verifyOtp.LeadMasterId,
                                RequestResponseMsg = jsonString,
                                Type = "Response",
                                Url = apiConfigdata.url,
                                CreatedBy = 0,
                                CreatedDate = DateTime.Now
                            };
                            InsertRequestResponse(addresponse);
                        }
                    }
                }
                catch (Exception ex)
                {
                    ArthmateReqResp addRequest = new ArthmateReqResp()
                    {
                        ActivityMasterId = apiConfigdata.id,
                        LeadMasterId = verifyOtp.LeadMasterId,
                        RequestResponseMsg = ex.Message.ToString(),
                        Type = "Response",
                        Url = apiConfigdata.url,
                        CreatedBy = 0,
                        CreatedDate = DateTime.Now
                    };
                    InsertRequestResponse(addRequest);

                    TextFileLogHelper.TraceLog("eAadharDigilockerVerifyOTPXml catch block 3 =" + ex.ToString());
                }

            }
            return responseDC;
        }

        #endregion



        #region Loan

        public async Task<GetLoanApiResponseDC> LoanApi(LoanApiRequestDc req, int userid, long LeadId)
        {
            string url = "";
            string token = "";
            string CompanyCode = "";
            var configdata = GetApiConfig("ArthMate");
            var Activitydata = GetActivityMaster("Loan API");
            if (configdata != null && Activitydata != null)
            {
                url = configdata.URL + "/" + Activitydata.URL;
                token = configdata.TokenKey;
                CompanyCode = configdata.CompanyCode == null ? "" : configdata.CompanyCode.ToString();
            }
            //GetLoanApiResponse responseDc = new GetLoanApiResponse();
            GetLoanApiResponseDC responseDc = new GetLoanApiResponseDC();

            //List<LoanApiResponse> responsedataDc = new List<LoanApiResponse>();

            List<LoanApiRequestDc> LoanReq = new List<LoanApiRequestDc>();
            LoanReq.Add(req);
            var RequestjsonString = JsonConvert.SerializeObject(LoanReq);
            try
            {
                var client = new RestClient(url);
                client.Timeout = -1;
                var request = new RestRequest(Method.POST);
                request.AddHeader("NoEncryption", "1");
                request.AddHeader("Content-Type", "application/json");
                request.AddHeader("Authorization", token);
                request.AddParameter("application/json", RequestjsonString, ParameterType.RequestBody);
                ServicePointManager.Expect100Continue = true;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                ArthmateReqResp addRequest = new ArthmateReqResp()
                {
                    ActivityMasterId = Activitydata.Id,
                    LeadMasterId = LeadId,
                    RequestResponseMsg = RequestjsonString,
                    Type = "Request",
                    Url = url,
                    CreatedBy = 0,
                    CreatedDate = DateTime.Now
                };
                var reqsave = InsertRequestResponseID(addRequest);
                IRestResponse response = await client.ExecuteAsync(request);
                string jsonString = "";
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    jsonString = (response.Content);
                    //responseDc = JsonConvert.DeserializeObject<GetLoanApiResponseDC>(jsonString);
                }
                else
                {
                    jsonString = (response.Content);
                    //responseDc = JsonConvert.DeserializeObject<GetLoanApiResponseDC>(jsonString);
                }
                ArthmateReqResp AddResponse = new ArthmateReqResp()
                {
                    ActivityMasterId = Activitydata.Id,
                    LeadMasterId = LeadId,
                    RequestResponseMsg = jsonString,
                    Type = "Response",
                    Url = url,
                    CreatedBy = 0,
                    CreatedDate = DateTime.Now
                };
                var res = InsertRequestResponseID(AddResponse);
                responseDc = JsonConvert.DeserializeObject<GetLoanApiResponseDC>(jsonString);

                responseDc.success = response.StatusCode == HttpStatusCode.OK ? true : false;
                responseDc.message = response.ErrorMessage;

            }
            catch (Exception ex)
            {
                // LoanApiSaveResponseTesting(LeadId)

                ArthmateReqResp addRequest = new ArthmateReqResp()
                {
                    ActivityMasterId = Activitydata.Id,
                    LeadMasterId = 0,
                    RequestResponseMsg = ex.ToString(),
                    Type = "Response",
                    Url = url,
                    CreatedBy = 0,
                    CreatedDate = DateTime.Now
                };
                var resw = InsertRequestResponse(addRequest);
            }

            return responseDc;
        }

        public async Task<repay_scheduleDc> repayment_schedule(Postrepayment_scheduleDc req, int userid, long Leadid, bool InsertDataInTable)
        {
            string url = "";
            string token = "";
            string CompanyCode = "";
            string sTaskName = "RepaymentSchedule";

            repay_scheduleDc resdata = new repay_scheduleDc();
            var configdata = GetApiConfig("ArthMate");
            var Activitydata = GetActivityMaster(sTaskName);
            if (configdata != null && Activitydata != null)
            {
                url = configdata.URL + "/" + Activitydata.URL + "/" + req.loan_id;//"https://apiorigin.arthmate.com/api/repayment_schedule/"+ req.loan_id;//
                token = configdata.TokenKey;
                CompanyCode = configdata.CompanyCode == null ? "" : configdata.CompanyCode.ToString();
            }

            var RequestjsonString = JsonConvert.SerializeObject(req);
            try
            {
                var client = new RestClient(url);
                client.Timeout = -1;
                var request = new RestRequest(Method.POST);
                request.AddHeader("NoEncryption", "1");
                request.AddHeader("Content-Type", "application/json");
                request.AddHeader("Authorization", token);
                request.AddParameter("application/json", RequestjsonString, ParameterType.RequestBody);
                ServicePointManager.Expect100Continue = true;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                ArthmateReqResp addRequest = new ArthmateReqResp()
                {
                    ActivityMasterId = Activitydata.Id,
                    LeadMasterId = Leadid,
                    RequestResponseMsg = RequestjsonString,
                    Type = "Request",
                    Url = url,
                    CreatedBy = 0,
                    CreatedDate = DateTime.Now
                };
                var reqsave = InsertRequestResponse(addRequest);
                IRestResponse response = await client.ExecuteAsync(request);
                string jsonString = "";
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    jsonString = (response.Content);
                    ArthmateReqResp AddResponse = new ArthmateReqResp()
                    {
                        ActivityMasterId = Activitydata.Id,
                        LeadMasterId = Leadid,
                        RequestResponseMsg = jsonString,
                        Type = "Response",
                        Url = url,
                        CreatedBy = 0,
                        CreatedDate = DateTime.Now
                    };

                    var res = InsertRequestResponse(AddResponse);
                    resdata = JsonConvert.DeserializeObject<repay_scheduleDc>(jsonString);
                    if (jsonString == null || string.IsNullOrEmpty(jsonString))
                    {
                        ArthMateController arthmateController = new ArthMateController();
                        arthmateController.LeadActivityProgressesHistory(Leadid, 0, 0, "", "Error in Sanction And Lba (SLA) Letter");
                    }
                }
                else
                {
                    jsonString = (response.Content);
                    ArthmateReqResp AddResponse = new ArthmateReqResp()
                    {
                        ActivityMasterId = Activitydata.Id,
                        LeadMasterId = Leadid,
                        RequestResponseMsg = jsonString,
                        Type = "Response",
                        Url = url,
                        CreatedBy = 0,
                        CreatedDate = DateTime.Now
                    };

                    var res = InsertRequestResponse(AddResponse);
                    resdata = JsonConvert.DeserializeObject<repay_scheduleDc>(jsonString);

                }

                //var data = InsertLeadBackgroundRun(Leadid, sTaskName, RequestjsonString, jsonString);
                if (InsertDataInTable == true)
                {
                    AuthContext context = new AuthContext();
                    if (resdata != null && resdata.success && resdata.data.rows.Count > 0)
                    {
                        //new code start 1
                        var repaymentData = context.RepaymentSchedule.Where(x => x.LeadMasterId == Leadid && x.IsActive == true && x.IsDeleted == false).ToList();
                        if (repaymentData.Count > 0)
                        {
                            repaymentData.ForEach(x =>
                            {
                                x.IsActive = false;
                                x.IsDeleted = true;
                                context.Entry(x).State = EntityState.Modified;
                            });
                            //context.Commit();
                        }


                        List<RepaymentSchedule> list = new List<RepaymentSchedule>();
                        foreach (var item in resdata.data.rows)
                        {
                            RepaymentSchedule ReDatas = new RepaymentSchedule();
                            ReDatas._id = item._id;
                            ReDatas.count = resdata.data.count;
                            ReDatas.success = resdata.success;
                            ReDatas.repay_schedule_id = item.repay_schedule_id;
                            ReDatas.company_id = item.company_id;
                            ReDatas.product_id = item.product_id;
                            ReDatas.loan_id = item.loan_id;
                            ReDatas.emi_no = item.emi_no;
                            ReDatas.due_date = item.due_date;
                            ReDatas.emi_amount = Convert.ToInt32(item.emi_amount);
                            ReDatas.prin = Convert.ToInt32(item.prin);
                            ReDatas.int_amount = Convert.ToInt32(item.int_amount);
                            ReDatas.__v = item.__v;
                            ReDatas.principal_bal = item.principal_bal;
                            ReDatas.principal_outstanding = item.principal_outstanding;
                            ReDatas.created_at = item.created_at;
                            ReDatas.updated_at = item.updated_at;
                            ReDatas.IsActive = true;
                            ReDatas.IsDeleted = false;
                            ReDatas.CreatedBy = 117;
                            ReDatas.CreatedDate = DateTime.Now;
                            ReDatas.ModifiedBy = null;
                            ReDatas.ModifiedDate = null;
                            ReDatas.LeadMasterId = Leadid;

                            list.Add(ReDatas);
                        }
                        context.RepaymentSchedule.AddRange(list);
                        context.Commit();
                    }
                }
            }
            catch (Exception ex)
            {
                ArthmateReqResp addRequest = new ArthmateReqResp()
                {
                    ActivityMasterId = Activitydata.Id,
                    LeadMasterId = Leadid,
                    RequestResponseMsg = ex.ToString(),
                    Type = "Response",
                    Url = url,
                    CreatedBy = 0,
                    CreatedDate = DateTime.Now
                };
                var resw = InsertRequestResponse(addRequest);
            }

            return resdata;
        }

        public async Task<LoanStatusChangeAPIRes> borrowerinfostatusupdate(LoanStatusChangeAPIReq json, int userid, long Leadid)
        {
            string url = "";
            string token = "";
            string CompanyCode = "";
            LoanStatusChangeAPIRes resdata = new LoanStatusChangeAPIRes();
            var configdata = GetApiConfig("ArthMate");
            var Activitydata = GetActivityMaster("Loan Status API");
            if (configdata != null && Activitydata != null)
            {
                url = configdata.URL + "/" + Activitydata.URL + "/" + json.loan_app_id;
                token = configdata.TokenKey;
                CompanyCode = configdata.CompanyCode == null ? "" : configdata.CompanyCode.ToString();
            }
            var RequestjsonString = JsonConvert.SerializeObject(json);
            try
            {
                var client = new RestClient(url);
                client.Timeout = -1;
                var request = new RestRequest(Method.PUT);
                request.AddHeader("NoEncryption", "1");
                request.AddHeader("Content-Type", "application/json");
                request.AddHeader("Authorization", token);
                request.AddParameter("application/json", RequestjsonString, ParameterType.RequestBody);
                ServicePointManager.Expect100Continue = true;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                ArthmateReqResp addRequest = new ArthmateReqResp()
                {
                    ActivityMasterId = Activitydata.Id,
                    LeadMasterId = Leadid,
                    RequestResponseMsg = RequestjsonString,
                    Type = "Request",
                    Url = url,
                    CreatedBy = 0,
                    CreatedDate = DateTime.Now
                };
                var reqsave = InsertRequestResponse(addRequest);
                IRestResponse response = await client.ExecuteAsync(request);
                string jsonString = "";
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    jsonString = (response.Content);
                    resdata = JsonConvert.DeserializeObject<LoanStatusChangeAPIRes>(jsonString);
                    resdata.message = "Status Changed";
                }
                else
                {
                    jsonString = (response.Content);
                    resdata = JsonConvert.DeserializeObject<LoanStatusChangeAPIRes>(jsonString);
                }
                ArthmateReqResp AddResponse = new ArthmateReqResp()
                {
                    ActivityMasterId = Activitydata.Id,
                    LeadMasterId = Leadid,
                    RequestResponseMsg = jsonString,
                    Type = "Response",
                    Url = url,
                    CreatedBy = 0,
                    CreatedDate = DateTime.Now
                };
                var res = InsertRequestResponse(AddResponse);
            }
            catch (Exception ex)
            {
                ArthmateReqResp addRequest = new ArthmateReqResp()
                {
                    ActivityMasterId = Activitydata.Id,
                    LeadMasterId = Leadid,
                    RequestResponseMsg = ex.ToString(),
                    Type = "Response",
                    Url = url,
                    CreatedBy = 0,
                    CreatedDate = DateTime.Now
                };
                var resw = InsertRequestResponse(addRequest);
            }

            return resdata;
        }



        #endregion

        public async Task<eSignDocResponseDc> eSignSessionAsync(eSignAgreementDc obj)
        {
            TextFileLogHelper.TraceLog("eSignSessionAsyncMain : => Phase 5");
            eSignDocResponseDc responseDC = new eSignDocResponseDc();
            var apiConfigdata = GetUrlTokenForApi("Karza", "eSignSession");
            TextFileLogHelper.TraceLog("esign session third party: => start");

            if (apiConfigdata != null)
            {
                ArthmateReqResp addresponse = null;
                try
                {
                    using (var httpClient = new HttpClient())
                    {
                        eSignSessionRequest eSignRequestDc = new eSignSessionRequest();
                        eSignRequestDc.name = obj.name;
                        eSignRequestDc.email = obj.email;
                        eSignRequestDc.workflowId = apiConfigdata.ApiSecretKey;//"nJcSP7j";
                        eSignRequestDc.yob = obj.yob;
                        eSignRequestDc.gender = obj.gender;
                        eSignRequestDc.mobileNo = obj.mobileNo;
                        eSignRequestDc.document = obj.document;
                        List<AdditionalSigner> list = new List<AdditionalSigner>();
                        AdditionalSigner d = new AdditionalSigner();
                        d.name = obj.name;
                        d.email = obj.email;
                        d.yob = obj.yob;
                        d.gender = obj.gender;
                        d.mobileNo = obj.mobileNo;

                        list.Add(d);
                        eSignRequestDc.additionalSigners = list;

                        //apiConfigdata.url = "https://testapi.karza.in/v3/esign-session";

                        using (var request = new HttpRequestMessage(new HttpMethod("POST"), apiConfigdata.url))
                        {
                            request.Headers.TryAddWithoutValidation("cache-control", "no-cache");
                            request.Headers.TryAddWithoutValidation("Content-Type", "application/json");
                            request.Headers.TryAddWithoutValidation("x-karza-key", apiConfigdata.TokenKey);
                            //request.Headers.TryAddWithoutValidation("x-karza-key", "931h46xzlv1GT41ktdhy"); // test key 931h46xzlv1GT41ktdhy

                            string jsonstringReq = JsonConvert.SerializeObject(eSignRequestDc);
                            request.Content = new StringContent(jsonstringReq);

                            ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;

                            addresponse = new ArthmateReqResp()
                            {
                                ActivityMasterId = apiConfigdata.id,
                                LeadMasterId = obj.LeadMasterId,
                                RequestResponseMsg = jsonstringReq,
                                Type = "Request",
                                Url = apiConfigdata.url,
                                CreatedBy = 0,
                                CreatedDate = DateTime.Now
                            };
                            InsertRequestResponse(addresponse);

                            string jsonString = string.Empty;
                            var response = await httpClient.SendAsync(request);
                            if (response.StatusCode == HttpStatusCode.OK)
                            {
                                jsonString = (await response.Content.ReadAsStringAsync());

                                addresponse = new ArthmateReqResp()
                                {
                                    ActivityMasterId = apiConfigdata.id,
                                    LeadMasterId = obj.LeadMasterId,
                                    RequestResponseMsg = jsonString,
                                    Type = "Response",
                                    Url = apiConfigdata.url,
                                    CreatedBy = 0,
                                    CreatedDate = DateTime.Now
                                };
                                InsertRequestResponse(addresponse);

                                responseDC = JsonConvert.DeserializeObject<eSignDocResponseDc>(jsonString);
                            }
                            else
                            {
                                jsonString = response.StatusCode.ToString();

                                addresponse = new ArthmateReqResp()
                                {
                                    ActivityMasterId = apiConfigdata.id,
                                    LeadMasterId = obj.LeadMasterId,
                                    RequestResponseMsg = jsonString,
                                    Type = "Response",
                                    Url = apiConfigdata.url,
                                    CreatedBy = 0,
                                    CreatedDate = DateTime.Now
                                };
                                InsertRequestResponse(addresponse);
                            }

                        }
                    }
                }
                catch (Exception ex)
                {

                    addresponse = new ArthmateReqResp()
                    {
                        ActivityMasterId = apiConfigdata.id,
                        LeadMasterId = obj.LeadMasterId,
                        RequestResponseMsg = ex.ToString(),
                        Type = "Response",
                        Url = apiConfigdata.url,
                        CreatedBy = 0,
                        CreatedDate = DateTime.Now
                    };
                    // responseDC.errorString = ex.Message.ToString();

                    TextFileLogHelper.TraceLog("Error in eSignSessionAsync ...." + ex.Message.ToString());


                    InsertRequestResponse(addresponse);
                }

            }
            return responseDC;

        }
        public async Task<eSignDocumentResponse> eSignDocumentsAsync(eSignDocumentRequest obj)
        {
            eSignDocumentResponse responseDC = new eSignDocumentResponse();
            var apiConfigdata = GetUrlTokenForApi("Karza", "eSignDocument");

            if (apiConfigdata != null)
            {
                ArthmateReqResp addresponse = null;
                try
                {
                    using (var httpClient = new HttpClient())
                    {
                        //eSignDocumentRequest eSignDocRequestDc = new eSignDocumentRequest();
                        //eSignDocRequestDc.documentId = obj.documentId;
                        //eSignDocRequestDc.verificationDetailsRequired = "Y";

                        eSignDocumentReqJson eSignDocumentReqJson = new eSignDocumentReqJson()
                        {
                            documentId = obj.documentId,
                            verificationDetailsRequired = "Y"
                        };

                        //apiConfigdata.url = "https://testapi.karza.in/v3/esign-document";

                        using (var request = new HttpRequestMessage(new HttpMethod("POST"), apiConfigdata.url))
                        {
                            request.Headers.TryAddWithoutValidation("cache-control", "no-cache");
                            request.Headers.TryAddWithoutValidation("Content-Type", "application/json");
                            request.Headers.TryAddWithoutValidation("x-karza-key", apiConfigdata.ApiSecretKey);
                            // request.Headers.TryAddWithoutValidation("x-karza-key", "931h46xzlv1GT41ktdhy"); // test key

                            string jsonstringReq = JsonConvert.SerializeObject(eSignDocumentReqJson);
                            request.Content = new StringContent(jsonstringReq);

                            ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;

                            ArthmateReqResp addRequest = new ArthmateReqResp()
                            {
                                ActivityMasterId = apiConfigdata.id,
                                LeadMasterId = obj.LeadMasterId,
                                RequestResponseMsg = jsonstringReq,
                                Type = "Request",
                                Url = apiConfigdata.url,
                                CreatedBy = 0,
                                CreatedDate = DateTime.Now
                            };
                            InsertRequestResponse(addRequest);

                            string jsonString = string.Empty;
                            var response = await httpClient.SendAsync(request);
                            if (response.StatusCode == HttpStatusCode.OK)
                            {
                                jsonString = (await response.Content.ReadAsStringAsync());
                                responseDC = JsonConvert.DeserializeObject<eSignDocumentResponse>(jsonString);
                            }
                            else
                            {
                                jsonString = (await response.Content.ReadAsStringAsync());
                                responseDC = JsonConvert.DeserializeObject<eSignDocumentResponse>(jsonString);
                            }
                            addresponse = new ArthmateReqResp()
                            {
                                ActivityMasterId = apiConfigdata.id,
                                LeadMasterId = obj.LeadMasterId,
                                RequestResponseMsg = jsonString,
                                Type = "Response",
                                Url = apiConfigdata.url,
                                CreatedBy = 0,
                                CreatedDate = DateTime.Now
                            };
                            InsertRequestResponse(addresponse);
                        }
                    }
                }
                catch (Exception ex)
                {
                    addresponse = new ArthmateReqResp()
                    {
                        ActivityMasterId = apiConfigdata.id,
                        LeadMasterId = obj.LeadMasterId,
                        RequestResponseMsg = ex.ToString(),
                        Type = "Response",
                        Url = apiConfigdata.url,
                        CreatedBy = 0,
                        CreatedDate = DateTime.Now
                    };
                    InsertRequestResponse(addresponse);
                }
            }
            return responseDC;

        }




        #region Cashfree(Mandatory for periodic mandate)

        // api  https://sandbox.cashfree.com/api/v2/subscriptions/nonSeamless/subscription

        //1. Create Subscription with Plan Info
        //    https://docs.cashfree.com/reference/create-subscription-with-planinfo
        //2. Get Subscription Details
        //   https://docs.cashfree.com/reference/fetch-subscription-details
        //3. Update Recurring Amount
        //   https://docs.cashfree.com/reference/update-recurring-amount
        //4. Cancel Subscription
        //   https://docs.cashfree.com/reference/update-recurring-amount

        public async Task<CreateSubscriptionwithPlanInfoRes> CreateSubscriptionwithPlanInfo(CreateSubscriptionwithPlanInfoDc req, int userid, long Leadid)
        {
            string url = "";
            CreateSubscriptionwithPlanInfoRes resdata = new CreateSubscriptionwithPlanInfoRes();
            var configdata = GetApiConfig("Cashfree");
            var Activitydata = GetActivityMaster("CreateSubscriptionwithPlanInfo");
            if (configdata != null && Activitydata != null)
            {
                url = configdata.URL + "/" + Activitydata.URL;
            }
            var RequestjsonString = JsonConvert.SerializeObject(req);
            try
            {
                var client = new RestClient(url);
                client.Timeout = -1;
                var request = new RestRequest(Method.POST);
                request.AddHeader("NoEncryption", "1");
                request.AddHeader("Content-Type", "application/json");
                request.AddHeader("X-Client-Id", Activitydata.TokenKey);
                request.AddHeader("X-Client-Secret", Activitydata.ApiSecretKey);
                request.AddParameter("application/json", RequestjsonString, ParameterType.RequestBody);

                ServicePointManager.Expect100Continue = true;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                ArthmateReqResp addRequest = new ArthmateReqResp()
                {
                    ActivityMasterId = Activitydata.Id,
                    LeadMasterId = Leadid,
                    RequestResponseMsg = RequestjsonString,
                    Type = "Request",
                    Url = url,
                    CreatedBy = 0,
                    CreatedDate = DateTime.Now
                };
                var reqsave = InsertRequestResponse(addRequest);
                IRestResponse response = await client.ExecuteAsync(request);
                string jsonString = "";
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    jsonString = (response.Content);
                    resdata = JsonConvert.DeserializeObject<CreateSubscriptionwithPlanInfoRes>(jsonString);
                }
                else
                {
                    jsonString = (response.Content);
                    resdata = JsonConvert.DeserializeObject<CreateSubscriptionwithPlanInfoRes>(jsonString);
                }
                ArthmateReqResp AddResponse = new ArthmateReqResp()
                {
                    ActivityMasterId = Activitydata.Id,
                    LeadMasterId = Leadid,
                    RequestResponseMsg = jsonString,
                    Type = "Response",
                    Url = url,
                    CreatedBy = 0,
                    CreatedDate = DateTime.Now
                };
                var res = InsertRequestResponse(AddResponse);
            }
            catch (Exception ex)
            {
                ArthmateReqResp addRequest = new ArthmateReqResp()
                {
                    ActivityMasterId = Activitydata.Id,
                    LeadMasterId = Leadid,
                    RequestResponseMsg = ex.ToString(),
                    Type = "Response",
                    Url = url,
                    CreatedBy = 0,
                    CreatedDate = DateTime.Now
                };
                var resw = InsertRequestResponse(addRequest);
            }
            return resdata;
        }
        public async Task<SubscriptionDetailsRes> GetSubscriptionDetails(string subReferenceId, int userid, long Leadid)
        {
            string url = "";
            SubscriptionDetailsRes resdata = new SubscriptionDetailsRes();
            var configdata = GetApiConfig("Cashfree");
            var Activitydata = GetActivityMaster("GetSubscriptionDetails");
            if (configdata != null && Activitydata != null)
            {
                url = configdata.URL + "/" + Activitydata.URL;
                url = url.Replace("{subReferenceId}", subReferenceId);

            }
            try
            {
                var client = new RestClient(url);
                client.Timeout = -1;
                var request = new RestRequest(Method.POST);
                request.AddHeader("NoEncryption", "1");
                request.AddHeader("Content-Type", "application/json");
                request.AddHeader("X-Client-Id", Activitydata.TokenKey);
                request.AddHeader("X-Client-Secret", Activitydata.ApiSecretKey);
                ServicePointManager.Expect100Continue = true;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                ArthmateReqResp addRequest = new ArthmateReqResp()
                {
                    ActivityMasterId = Activitydata.Id,
                    LeadMasterId = Leadid,
                    RequestResponseMsg = subReferenceId,
                    Type = "Request",
                    Url = url,
                    CreatedBy = 0,
                    CreatedDate = DateTime.Now
                };
                var reqsave = InsertRequestResponse(addRequest);
                IRestResponse response = await client.ExecuteAsync(request);
                string jsonString = "";
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    jsonString = (response.Content);
                    resdata = JsonConvert.DeserializeObject<SubscriptionDetailsRes>(jsonString);
                }
                else
                {
                    jsonString = (response.Content);
                    resdata = JsonConvert.DeserializeObject<SubscriptionDetailsRes>(jsonString);
                }
                ArthmateReqResp AddResponse = new ArthmateReqResp()
                {
                    ActivityMasterId = Activitydata.Id,
                    LeadMasterId = Leadid,
                    RequestResponseMsg = jsonString,
                    Type = "Response",
                    Url = url,
                    CreatedBy = 0,
                    CreatedDate = DateTime.Now
                };
                var res = InsertRequestResponse(AddResponse);
            }
            catch (Exception ex)
            {
                ArthmateReqResp addRequest = new ArthmateReqResp()
                {
                    ActivityMasterId = Activitydata.Id,
                    LeadMasterId = Leadid,
                    RequestResponseMsg = ex.ToString(),
                    Type = "Response",
                    Url = url,
                    CreatedBy = 0,
                    CreatedDate = DateTime.Now
                };
                var resw = InsertRequestResponse(addRequest);
            }
            return resdata;
        }
        public async Task<UpdateRecurringRes> UpdateRecurringAmount(string subReferenceId, double amount, int userid, long Leadid)
        {

            var UpdateRecurringAmount = new UpdateRecurringAmount();
            UpdateRecurringAmount.amount = amount;
            var RequestjsonString = JsonConvert.SerializeObject(UpdateRecurringAmount);

            string url = "";
            UpdateRecurringRes resdata = new UpdateRecurringRes();
            var configdata = GetApiConfig("Cashfree");
            var Activitydata = GetActivityMaster("UpdateRecurringAmount");
            if (configdata != null && Activitydata != null)
            {
                url = configdata.URL + "/" + Activitydata.URL;
                url = url.Replace("{subRefId}", subReferenceId);

            }
            try
            {
                var client = new RestClient(url);
                client.Timeout = -1;
                var request = new RestRequest(Method.POST);
                request.AddHeader("NoEncryption", "1");
                request.AddHeader("Content-Type", "application/json");
                request.AddHeader("X-Client-Id", Activitydata.TokenKey);
                request.AddHeader("X-Client-Secret", Activitydata.ApiSecretKey);
                request.AddParameter("application/json", RequestjsonString, ParameterType.RequestBody);

                ServicePointManager.Expect100Continue = true;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                ArthmateReqResp addRequest = new ArthmateReqResp()
                {
                    ActivityMasterId = Activitydata.Id,
                    LeadMasterId = Leadid,
                    RequestResponseMsg = RequestjsonString,
                    Type = "Request",
                    Url = url,
                    CreatedBy = 0,
                    CreatedDate = DateTime.Now
                };
                var reqsave = InsertRequestResponse(addRequest);
                IRestResponse response = await client.ExecuteAsync(request);
                string jsonString = "";
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    jsonString = (response.Content);
                    resdata = JsonConvert.DeserializeObject<UpdateRecurringRes>(jsonString);
                }
                else
                {
                    jsonString = (response.Content);
                    resdata = JsonConvert.DeserializeObject<UpdateRecurringRes>(jsonString);
                }
                ArthmateReqResp AddResponse = new ArthmateReqResp()
                {
                    ActivityMasterId = Activitydata.Id,
                    LeadMasterId = Leadid,
                    RequestResponseMsg = jsonString,
                    Type = "Response",
                    Url = url,
                    CreatedBy = 0,
                    CreatedDate = DateTime.Now
                };
                var res = InsertRequestResponse(AddResponse);
            }
            catch (Exception ex)
            {
                ArthmateReqResp addRequest = new ArthmateReqResp()
                {
                    ActivityMasterId = Activitydata.Id,
                    LeadMasterId = Leadid,
                    RequestResponseMsg = ex.ToString(),
                    Type = "Response",
                    Url = url,
                    CreatedBy = 0,
                    CreatedDate = DateTime.Now
                };
                var resw = InsertRequestResponse(addRequest);
            }
            return resdata;
        }
        public async Task<CancelSubscriptionRes> CancelSubscription(string subReferenceId, int userid, long Leadid)
        {
            string url = "";
            CancelSubscriptionRes resdata = new CancelSubscriptionRes();

            var configdata = GetApiConfig("Cashfree");
            var Activitydata = GetActivityMaster("CancelSubscription");
            if (configdata != null && Activitydata != null)
            {
                url = configdata.URL + "/" + Activitydata.URL;
                url = url.Replace("{subReferenceId}", subReferenceId);
            }
            try
            {
                var client = new RestClient(url);
                client.Timeout = -1;
                var request = new RestRequest(Method.POST);
                request.AddHeader("NoEncryption", "1");
                request.AddHeader("Content-Type", "application/json");
                request.AddHeader("X-Client-Id", Activitydata.TokenKey);
                request.AddHeader("X-Client-Secret", Activitydata.ApiSecretKey);
                ServicePointManager.Expect100Continue = true;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                ArthmateReqResp addRequest = new ArthmateReqResp()
                {
                    ActivityMasterId = Activitydata.Id,
                    LeadMasterId = Leadid,
                    RequestResponseMsg = subReferenceId,
                    Type = "Request",
                    Url = url,
                    CreatedBy = 0,
                    CreatedDate = DateTime.Now
                };
                var reqsave = InsertRequestResponse(addRequest);
                IRestResponse response = await client.ExecuteAsync(request);
                string jsonString = "";
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    jsonString = (response.Content);
                    resdata = JsonConvert.DeserializeObject<CancelSubscriptionRes>(jsonString);
                }
                else
                {
                    jsonString = (response.Content);
                    resdata = JsonConvert.DeserializeObject<CancelSubscriptionRes>(jsonString);
                }
                ArthmateReqResp AddResponse = new ArthmateReqResp()
                {
                    ActivityMasterId = Activitydata.Id,
                    LeadMasterId = Leadid,
                    RequestResponseMsg = jsonString,
                    Type = "Response",
                    Url = url,
                    CreatedBy = 0,
                    CreatedDate = DateTime.Now
                };
                var res = InsertRequestResponse(AddResponse);
            }
            catch (Exception ex)
            {
                ArthmateReqResp addRequest = new ArthmateReqResp()
                {
                    ActivityMasterId = Activitydata.Id,
                    LeadMasterId = Leadid,
                    RequestResponseMsg = ex.ToString(),
                    Type = "Response",
                    Url = url,
                    CreatedBy = 0,
                    CreatedDate = DateTime.Now
                };
                var resw = InsertRequestResponse(addRequest);
            }
            return resdata;

        }
        #endregion



        #region Penny Drop Api 

        public async Task<PennyResponseDc> PennyDropVerification(PennyDropReqJson pennyDropReqJson, long leadid)
        {
            TextFileLogHelper.TraceLog("PennyDropVerification start 1");
            PennyResponseDc responseDc = new PennyResponseDc();

            string url = "";
            string token = "";
            var configdata = GetApiConfig("Arthmate");
            var Activitydata = GetActivityMaster("PennyDrop");
            if (configdata != null && Activitydata != null)
            {
                url = configdata.URL + "/" + Activitydata.URL;
                token = Activitydata.TokenKey;
            }
            var RequestjsonString = JsonConvert.SerializeObject(pennyDropReqJson);

            TextFileLogHelper.TraceLog("PennyDropVerification request 2 =" + RequestjsonString);
            try
            {
                var client = new RestClient(url);
                client.Timeout = -1;
                var request = new RestRequest(Method.POST);
                request.AddHeader("NoEncryption", "1");
                request.AddHeader("Content-Type", "application/json");
                request.AddHeader("Authorization", token);
                request.AddParameter("application/json", RequestjsonString, ParameterType.RequestBody);

                ServicePointManager.Expect100Continue = true;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                ArthmateReqResp addRequest = new ArthmateReqResp()
                {
                    ActivityMasterId = Activitydata.Id,
                    LeadMasterId = leadid,
                    RequestResponseMsg = RequestjsonString,
                    Type = "Request",
                    Url = url,
                    CreatedBy = 0,
                    CreatedDate = DateTime.Now
                };
                var reqsave = InsertRequestResponse(addRequest);
                IRestResponse response = await client.ExecuteAsync(request);

                TextFileLogHelper.TraceLog("PennyDropVerification response 3 =" + response.ToString());

                string jsonString = "";
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    jsonString = (response.Content);
                    responseDc = JsonConvert.DeserializeObject<PennyResponseDc>(jsonString);
                }
                else
                {
                    jsonString = (response.Content);
                    responseDc = JsonConvert.DeserializeObject<PennyResponseDc>(jsonString);
                }
                ArthmateReqResp AddResponse = new ArthmateReqResp()
                {
                    ActivityMasterId = Activitydata.Id,
                    LeadMasterId = leadid,
                    RequestResponseMsg = jsonString,
                    Type = "Response",
                    Url = url,
                    CreatedBy = 0,
                    CreatedDate = DateTime.Now
                };
                var res = InsertRequestResponse(AddResponse);


            }
            catch (Exception ex)
            {
                ArthmateReqResp addRequest = new ArthmateReqResp()
                {
                    ActivityMasterId = Activitydata.Id,
                    LeadMasterId = leadid,
                    RequestResponseMsg = ex.ToString(),
                    Type = "Response",
                    Url = url,
                    CreatedBy = 0,
                    CreatedDate = DateTime.Now
                };
                var resw = InsertRequestResponse(addRequest);
                TextFileLogHelper.TraceLog("PennyDropVerification catch block 4 =" + ex.ToString());
            }
            return responseDc;

        }


        #endregion


        #region  LoanNachPatchAPI(URMN)
        public async Task<LoanNachResponseDC> LoanNachPatchAPI(LoanNachAPIDc LoanNachAPI, long leadid, string loanid)
        {
            LoanNachResponseDC responseDc = new LoanNachResponseDC();

            string url = "";
            string token = "";
            var configdata = GetApiConfig("Arthmate");
            var Activitydata = GetActivityMaster("LoanNachPatch");
            if (configdata != null && Activitydata != null)
            {
                url = configdata.URL + "/" + Activitydata.URL + "/" + loanid;
                token = Activitydata.TokenKey;
            }

            var RequestjsonString = JsonConvert.SerializeObject(LoanNachAPI);
            try
            {
                var client = new RestClient(url);
                client.Timeout = -1;
                var request = new RestRequest(Method.PATCH);
                request.AddHeader("NoEncryption", "1");
                request.AddHeader("Content-Type", "application/json");
                request.AddHeader("Authorization", token);
                request.AddParameter("application/json", RequestjsonString, ParameterType.RequestBody);

                ServicePointManager.Expect100Continue = true;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                ArthmateReqResp addRequest = new ArthmateReqResp()
                {
                    ActivityMasterId = Activitydata.Id,
                    LeadMasterId = leadid,
                    RequestResponseMsg = RequestjsonString,
                    Type = "Request",
                    Url = url,
                    CreatedBy = 0,
                    CreatedDate = DateTime.Now
                };
                var reqsave = InsertRequestResponse(addRequest);
                IRestResponse response = await client.ExecuteAsync(request);
                string jsonString = "";
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    jsonString = (response.Content);
                    responseDc = JsonConvert.DeserializeObject<LoanNachResponseDC>(jsonString);
                }
                else
                {
                    jsonString = (response.Content);
                    responseDc = JsonConvert.DeserializeObject<LoanNachResponseDC>(jsonString);
                }
                ArthmateReqResp AddResponse = new ArthmateReqResp()
                {
                    ActivityMasterId = Activitydata.Id,
                    LeadMasterId = leadid,
                    RequestResponseMsg = jsonString,
                    Type = "Response",
                    Url = url,
                    CreatedBy = 0,
                    CreatedDate = DateTime.Now
                };
                var res = InsertRequestResponse(AddResponse);
            }
            catch (Exception ex)
            {
                ArthmateReqResp addRequest = new ArthmateReqResp()
                {
                    ActivityMasterId = Activitydata.Id,
                    LeadMasterId = leadid,
                    RequestResponseMsg = ex.ToString(),
                    Type = "Response",
                    Url = url,
                    CreatedBy = 0,
                    CreatedDate = DateTime.Now
                };
                var resw = InsertRequestResponse(addRequest);

            }
            return responseDc;
        }

        #endregion

        #region Composite Disbursement
        public async Task<CompositeDisbursementResponse> CompositeDisbursementAPI(CompositeDisbursementDc compdisObj, long leadid)
        {
            CompositeDisbursementResponse responseDc = new CompositeDisbursementResponse();

            string url = "";
            string token = "";
            var configdata = GetApiConfig("Arthmate");
            var Activitydata = GetActivityMaster("CompositeDisbursement");
            if (configdata != null && Activitydata != null)
            {
                url = configdata.URL + "/" + Activitydata.URL;
                token = Activitydata.TokenKey;
            }

            var RequestjsonString = JsonConvert.SerializeObject(compdisObj);
            try
            {
                var client = new RestClient(url);
                client.Timeout = -1;
                var request = new RestRequest(Method.POST);
                request.AddHeader("NoEncryption", "1");
                request.AddHeader("Content-Type", "application/json");
                request.AddHeader("Authorization", token);
                request.AddParameter("application/json", RequestjsonString, ParameterType.RequestBody);

                ServicePointManager.Expect100Continue = true;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                ArthmateReqResp addRequest = new ArthmateReqResp()
                {
                    ActivityMasterId = Activitydata.Id,
                    LeadMasterId = leadid,
                    RequestResponseMsg = RequestjsonString,
                    Type = "Request",
                    Url = url,
                    CreatedBy = 0,
                    CreatedDate = DateTime.Now
                };
                var reqsave = InsertRequestResponse(addRequest);
                IRestResponse response = await client.ExecuteAsync(request);
                string jsonString = "";
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    jsonString = (response.Content);
                    responseDc = JsonConvert.DeserializeObject<CompositeDisbursementResponse>(jsonString);
                }
                else
                {
                    jsonString = (response.Content);
                    responseDc = JsonConvert.DeserializeObject<CompositeDisbursementResponse>(jsonString);
                }
                ArthmateReqResp AddResponse = new ArthmateReqResp()
                {
                    ActivityMasterId = Activitydata.Id,
                    LeadMasterId = leadid,
                    RequestResponseMsg = jsonString,
                    Type = "Response",
                    Url = url,
                    CreatedBy = 0,
                    CreatedDate = DateTime.Now
                };
                var res = InsertRequestResponse(AddResponse);
            }
            catch (Exception ex)
            {
                ArthmateReqResp addRequest = new ArthmateReqResp()
                {
                    ActivityMasterId = Activitydata.Id,
                    LeadMasterId = leadid,
                    RequestResponseMsg = ex.ToString(),
                    Type = "Response",
                    Url = url,
                    CreatedBy = 0,
                    CreatedDate = DateTime.Now
                };
                var resw = InsertRequestResponse(addRequest);

            }
            return responseDc;
        }

        //public async Task<CompositeDrawDownResponseDc> CompositeDrawDownAPI(CompositeDrawDownDc compdisObj, long leadid)
        //{
        //    CompositeDrawDownResponseDc responseDc = new CompositeDrawDownResponseDc();

        //    string url = "";
        //    string token = "";
        //    var configdata = GetApiConfig("Arthmate");
        //    var Activitydata = GetActivityMaster("CompositeDrawdown");
        //    if (configdata != null && Activitydata != null)
        //    {
        //        url = configdata.URL + "/" + Activitydata.URL;
        //        token = Activitydata.TokenKey;
        //    }

        //    var RequestjsonString = JsonConvert.SerializeObject(compdisObj);
        //    try
        //    {
        //        var client = new RestClient(url);
        //        client.Timeout = -1;
        //        var request = new RestRequest(Method.POST);
        //        request.AddHeader("NoEncryption", "1");
        //        request.AddHeader("Content-Type", "application/json");
        //        request.AddHeader("Authorization", token);
        //        request.AddParameter("application/json", RequestjsonString, ParameterType.RequestBody);

        //        ServicePointManager.Expect100Continue = true;
        //        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
        //        ArthmateReqResp addRequest = new ArthmateReqResp()
        //        {
        //            ActivityMasterId = Activitydata.Id,
        //            LeadMasterId = leadid,
        //            RequestResponseMsg = RequestjsonString,
        //            Type = "Request",
        //            Url = url,
        //            CreatedBy = 0,
        //            CreatedDate = DateTime.Now
        //        };
        //        var reqsave = InsertRequestResponse(addRequest);
        //        IRestResponse response = await client.ExecuteAsync(request);
        //        string jsonString = "";
        //        if (response.StatusCode == HttpStatusCode.OK)
        //        {
        //            jsonString = (response.Content);
        //            responseDc = JsonConvert.DeserializeObject<CompositeDrawDownResponseDc>(jsonString);
        //        }
        //        else
        //        {
        //            jsonString = (response.Content);
        //            responseDc.ErrorRes = JsonConvert.DeserializeObject<DrawDownErrorRes>(jsonString);
        //        }
        //        ArthmateReqResp AddResponse = new ArthmateReqResp()
        //        {
        //            ActivityMasterId = Activitydata.Id,
        //            LeadMasterId = leadid,
        //            RequestResponseMsg = jsonString,
        //            Type = "Response",
        //            Url = url,
        //            CreatedBy = 0,
        //            CreatedDate = DateTime.Now
        //        };
        //        var res = InsertRequestResponse(AddResponse);
        //    }
        //    catch (Exception ex)
        //    {
        //        ArthmateReqResp addRequest = new ArthmateReqResp()
        //        {
        //            ActivityMasterId = Activitydata.Id,
        //            LeadMasterId = leadid,
        //            RequestResponseMsg = ex.ToString(),
        //            Type = "Response",
        //            Url = url,
        //            CreatedBy = 0,
        //            CreatedDate = DateTime.Now
        //        };
        //        var resw = InsertRequestResponse(addRequest);

        //    }
        //    return responseDc;
        //}
        public async Task<CommonResponseDc> CompositeDisbursementWebhook(CompositeDisbursementWebhookDc compdisObj, long leadid)
        {

            CommonResponseDc responseDc = new CommonResponseDc();

            string url = "";
            string token = "";
            var configdata = GetApiConfig("Arthmate");
            var Activitydata = GetActivityMaster("CallBackDisbursement");
            if (configdata != null && Activitydata != null)
            {
                url = configdata.URL + "/" + Activitydata.URL;
                token = Activitydata.TokenKey;
            }
            TextFileLogHelper.TraceLog("CompositeDisbursement Webhook start 1");

            var RequestjsonString = JsonConvert.SerializeObject(compdisObj);

            TextFileLogHelper.TraceLog("CompositeDisbursement Webhook request =" + RequestjsonString);

            try
            {
                var client = new RestClient(url);
                client.Timeout = -1;
                var request = new RestRequest(Method.POST);
                request.AddHeader("NoEncryption", "1");
                request.AddHeader("Content-Type", "application/json");
                request.AddHeader("Authorization", token);
                request.AddParameter("application/json", RequestjsonString, ParameterType.RequestBody);

                ServicePointManager.Expect100Continue = true;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                ArthmateReqResp addRequest = new ArthmateReqResp()
                {
                    ActivityMasterId = Activitydata.Id,
                    LeadMasterId = leadid,
                    RequestResponseMsg = RequestjsonString,
                    Type = "Request",
                    Url = url,
                    CreatedBy = 0,
                    CreatedDate = DateTime.Now
                };
                var reqsave = InsertRequestResponse(addRequest);
                IRestResponse response = await client.ExecuteAsync(request);

                TextFileLogHelper.TraceLog("CompositeDisbursement Webhook response =" + response.ToString());

                string jsonString = "";
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    jsonString = (response.Content);
                    //responseDc = JsonConvert.DeserializeObject<CompositeDrawDownResponseDc>(jsonString);
                }
                else
                {
                    jsonString = (response.Content);
                    //responseDc.ErrorRes = JsonConvert.DeserializeObject<DrawDownErrorRes>(jsonString);
                }
                ArthmateReqResp AddResponse = new ArthmateReqResp()
                {
                    ActivityMasterId = Activitydata.Id,
                    LeadMasterId = leadid,
                    RequestResponseMsg = jsonString,
                    Type = "Response",
                    Url = url,
                    CreatedBy = 0,
                    CreatedDate = DateTime.Now
                };
                var res = InsertRequestResponse(AddResponse);
            }
            catch (Exception ex)
            {
                ArthmateReqResp addRequest = new ArthmateReqResp()
                {
                    ActivityMasterId = Activitydata.Id,
                    LeadMasterId = leadid,
                    RequestResponseMsg = ex.ToString(),
                    Type = "Response",
                    Url = url,
                    CreatedBy = 0,
                    CreatedDate = DateTime.Now
                };
                var resw = InsertRequestResponse(addRequest);
                TextFileLogHelper.TraceLog("CompositeDisbursement Webhook catch block =" + ex.ToString());
            }
            return responseDc;
        }

        public async Task<DisbursementDataDc> GetDisbursementAPI(string loan_id, long leadid)
        {

            string url = "";
            DisbursementDataDc resdata = new DisbursementDataDc();
            var configdata = GetApiConfig("Arthmate");
            var Activitydata = GetActivityMaster("GetDisbursement");
            if (configdata != null && Activitydata != null)
            {
                url = configdata.URL + "" + Activitydata.URL;
                url = url.Replace("{loan_id}", loan_id);

            }
            try
            {
                var client = new RestClient(url);
                client.Timeout = -1;
                var request = new RestRequest(Method.GET);
                request.AddHeader("NoEncryption", "1");
                request.AddHeader("Content-Type", "application/json");
                request.AddHeader("Authorization", Activitydata.TokenKey);


                ServicePointManager.Expect100Continue = true;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                ArthmateReqResp addRequest = new ArthmateReqResp()
                {
                    ActivityMasterId = Activitydata.Id,
                    LeadMasterId = leadid,
                    RequestResponseMsg = loan_id,
                    Type = "Request",
                    Url = url,
                    CreatedBy = 0,
                    CreatedDate = DateTime.Now
                };
                var reqsave = InsertRequestResponse(addRequest);
                IRestResponse response = await client.ExecuteAsync(request);
                string jsonString = "";
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    jsonString = (response.Content);
                    resdata = JsonConvert.DeserializeObject<DisbursementDataDc>(jsonString);
                }
                else
                {
                    jsonString = (response.Content);
                    resdata = JsonConvert.DeserializeObject<DisbursementDataDc>(jsonString);
                }
                ArthmateReqResp AddResponse = new ArthmateReqResp()
                {
                    ActivityMasterId = Activitydata.Id,
                    LeadMasterId = leadid,
                    RequestResponseMsg = jsonString,
                    Type = "Response",
                    Url = url,
                    CreatedBy = 0,
                    CreatedDate = DateTime.Now
                };
                var res = InsertRequestResponse(AddResponse);
            }
            catch (Exception ex)
            {
                ArthmateReqResp addRequest = new ArthmateReqResp()
                {
                    ActivityMasterId = Activitydata.Id,
                    LeadMasterId = leadid,
                    RequestResponseMsg = ex.ToString(),
                    Type = "Response",
                    Url = url,
                    CreatedBy = 0,
                    CreatedDate = DateTime.Now
                };
                var resw = InsertRequestResponse(addRequest);
            }
            return resdata;
        }
        #endregion






        #region cplr CeplrPdfReports

        public async Task<PdfResDcCeplr> CeplrPdfReports(CeplrPdfReportDc pdfReportDc, long Leadid)
        {
            TextFileLogHelper.TraceLog("pdfReportDc.file FILEURL 1 =" + pdfReportDc.file.ToString());
            string url = "";
            string ApiSecretKey = "";

            PdfResDcCeplr pdfResDcCeplr = new PdfResDcCeplr();
            List<CeplrPostDc> postlist = new List<CeplrPostDc>();

            var configdata = GetApiConfig("Cepler");
            var Activitydata = GetActivityMaster("Pdf Reports");
            if (configdata != null && Activitydata != null)
            {
                url = configdata.URL + Activitydata.URL;
                pdfReportDc.configuration_uuid = Activitydata.ApiSecretKey;
                ApiSecretKey = Activitydata.TokenKey;
            }

            try
            {
                int BankFileCount = 0;
                using (var db = new AuthContext())
                {
                    string baseBath = HttpRuntime.AppDomainAppPath;

                    var MultipleBankFle = db.LeadBankStatement.Where(x => x.LeadMasterId == Leadid && x.IsActive == true && x.IsDeleted == false).OrderByDescending(x => x.Sequence).ToList();
                    if (MultipleBankFle.Count > 0)
                    {
                        BankFileCount = MultipleBankFle.Count();
                    }
                    if (BankFileCount > 0)
                    {
                        foreach (var file in MultipleBankFle)
                        {
                            var fileurl = string.Concat(HttpRuntime.AppDomainAppPath, file.StatementFile.Replace("/", "\\"));
                            pdfReportDc.file = fileurl;
                            postlist.Add(new CeplrPostDc
                            {
                                callback_url = pdfReportDc.callback_url,
                                configuration_uuid = pdfReportDc.configuration_uuid,
                                email = pdfReportDc.email,
                                file_password = pdfReportDc.file_password,
                                name = pdfReportDc.name,
                                filepath = pdfReportDc.file,
                                ifsc_code = pdfReportDc.ifsc_code,
                                fip_id = pdfReportDc.fip_id,
                                mobile = pdfReportDc.mobile,
                                Id = file.Id
                            });

                        }
                    }
                    //}

                    var RequestjsonString = JsonConvert.SerializeObject(postlist);

                    ArthmateReqResp addRequest = new ArthmateReqResp()
                    {
                        ActivityMasterId = Activitydata.Id,
                        LeadMasterId = Leadid,
                        RequestResponseMsg = RequestjsonString,
                        Type = "Request",
                        Url = url,
                        CreatedBy = 0,
                        CreatedDate = DateTime.Now
                    };
                    var reqsave = InsertRequestResponse(addRequest);

                    int count = 1;
                    string token = "";
                    int request_id = 0;
                    foreach (var item in postlist.OrderByDescending(x => x.Id))//3
                    {
                        if (postlist.Count > 1 && count == BankFileCount)
                        {
                            item.last_file = true;
                        }
                        item.allow_multiple = BankFileCount == 1 ? false : true;
                        if (request_id > 0 && item.allow_multiple)
                        {
                            //var ceplerTokens = db.CeplrPdfReports.Where(x => x.LeadMasterId == Leadid && x.IsActive == true && x.IsDeleted == false).FirstOrDefault();
                            //token = ceplerTokens.ApiToken;
                            //request_id = ceplerTokens.ApiRequest_id;

                            item.token = token;
                            item.request_id = request_id;
                        }

                        string jsonString = "";
                        string response = await PostCplr(item, ApiSecretKey, url);
                        if (response != null)
                        {


                            ArthmateReqResp AddResponse = new ArthmateReqResp()
                            {
                                ActivityMasterId = Activitydata.Id,
                                LeadMasterId = Leadid,
                                RequestResponseMsg = response,
                                Type = "Response",
                                Url = url,
                                CreatedBy = 0,
                                CreatedDate = DateTime.Now
                            };
                            var res = InsertRequestResponse(AddResponse);

                            pdfResDcCeplr = JsonConvert.DeserializeObject<PdfResDcCeplr>(response);

                            if (postlist.Count > 0)
                            {
                                //token = pdfResDcCeplr.data.token;
                                //request_id = pdfResDcCeplr.data.request_id;
                                if (pdfResDcCeplr.data != null)
                                {
                                    if (count == 1)
                                    {
                                        var ceplerdata = db.CeplrPdfReports.Where(x => x.LeadMasterId == Leadid && x.IsActive == true && x.IsDeleted == false).FirstOrDefault();
                                        ceplerdata.ApiRequest_id = pdfResDcCeplr.data.request_id;
                                        ceplerdata.ApiToken = pdfResDcCeplr.data.token;
                                        db.Entry(ceplerdata).State = EntityState.Modified;
                                        db.Commit();
                                        request_id = pdfResDcCeplr.data.request_id;
                                        token = pdfResDcCeplr.data.token;
                                    }
                                    //var ceplerdata = db.CeplrPdfReports.Where(x => x.LeadMasterId == Leadid && x.IsActive == true && x.IsDeleted == false).FirstOrDefault();
                                    //ceplerdata.ApiRequest_id = pdfResDcCeplr.data.request_id;
                                    //ceplerdata.ApiToken = pdfResDcCeplr.data.token;
                                    //db.Entry(ceplerdata).State = EntityState.Modified;
                                    //db.Commit();
                                    //var ceplerTokens = db.CeplrPdfReports.Where(x => x.LeadMasterId == Leadid && x.IsActive == true && x.IsDeleted == false).FirstOrDefault();
                                    //token = ceplerTokens.ApiToken;
                                    //request_id = ceplerTokens.ApiRequest_id;

                                }
                            }

                        }

                        count++;
                    }

                }
            }
            catch (Exception ex)
            {
                TextFileLogHelper.TraceLog("(Exception in Cepler Pdf Report ex=" + ex.Message);
                ArthmateReqResp addRequest = new ArthmateReqResp()
                {
                    ActivityMasterId = Activitydata.Id,
                    LeadMasterId = Leadid,
                    RequestResponseMsg = ex.Message.ToString(),
                    Type = "Response",
                    Url = url,
                    CreatedBy = 0,
                    CreatedDate = DateTime.Now
                };
                var resw = InsertRequestResponse(addRequest);
            }

            return pdfResDcCeplr;
        }


        public async Task<string> PostCplr(CeplrPostDc CeplrPost, string apikey, string postapi)
        {

            string pdfFilePath = CeplrPost.filepath; // @"C:\SKMK\trunk\AngularJSAuthentication.API\ArthmateDocument\50100228515419_1703164821009.pdf";
            // Read file data
            FileStream fs = new FileStream(pdfFilePath, FileMode.Open, FileAccess.Read);
            byte[] data = new byte[fs.Length];
            fs.Read(data, 0, data.Length);
            fs.Close();
            string filename = Path.GetFileName(pdfFilePath);

            // Generate post objects
            Dictionary<string, object> postParameters = new Dictionary<string, object>();
            //postParameters.Add("filename", filename);
            //postParameters.Add("fileformat", "pdf");
            postParameters.Add("file", new FormUpload.FileParameter(data, filename, "application/pdf"));
            postParameters.Add("email", CeplrPost.email);
            postParameters.Add("ifsc_code", CeplrPost.ifsc_code);
            postParameters.Add("fip_id", (string.IsNullOrEmpty(CeplrPost.fip_id) ? "" : CeplrPost.fip_id));
            postParameters.Add("callback_url", CeplrPost.callback_url);
            postParameters.Add("mobile", CeplrPost.mobile);
            postParameters.Add("name", CeplrPost.name);
            postParameters.Add("file_password", (string.IsNullOrEmpty(CeplrPost.file_password) ? "" : CeplrPost.file_password));
            postParameters.Add("configuration_uuid", CeplrPost.configuration_uuid);
            if (CeplrPost.allow_multiple == true)
            {
                postParameters.Add("allow_multiple", (CeplrPost.allow_multiple ? "true" : "false"));
            }
            if (CeplrPost.allow_multiple == true && CeplrPost.request_id > 0)
            {
                postParameters.Add("token", CeplrPost.token);
                postParameters.Add("request_id", CeplrPost.request_id);
            }
            if (CeplrPost.last_file == true)
            {
                postParameters.Add("last_file", (CeplrPost.last_file ? "true" : "false"));
            }
            // Create request and receive response
            string postURL = postapi;
            string userAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/73.0.3683.103 Safari/537.36";

            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            HttpWebResponse webResponse = FormUpload.MultipartFormDataPost(postURL, userAgent, postParameters, apikey);

            // Process response
            StreamReader responseReader = new StreamReader(webResponse.GetResponseStream());
            string fullResponse = responseReader.ReadToEnd();
            webResponse.Close();
            return fullResponse;
        }
        public static class FormUpload
        {
            private static readonly Encoding encoding = Encoding.UTF8;
            public static HttpWebResponse MultipartFormDataPost(string postUrl, string userAgent, Dictionary<string, object> postParameters, string apikey)
            {
                string formDataBoundary = String.Format("----------{0:N}", Guid.NewGuid());
                string contentType = "multipart/form-data; boundary=" + formDataBoundary;

                byte[] formData = GetMultipartFormData(postParameters, formDataBoundary);

                return PostForm(postUrl, userAgent, contentType, formData, apikey);
            }
            private static HttpWebResponse PostForm(string postUrl, string userAgent, string contentType, byte[] formData, string apikey)
            {
                //ServicePointManager.Expect100Continue = true;
                //ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                HttpWebRequest request = WebRequest.Create(postUrl) as HttpWebRequest;

                if (request == null)
                {
                    throw new NullReferenceException("request is not a http request");
                }

                request.Headers.Add("x-api-key", apikey);
                // Set up the request properties.
                request.Method = "POST";
                request.ContentType = contentType;
                request.UserAgent = userAgent;
                request.CookieContainer = new CookieContainer();
                request.ContentLength = formData.Length;

                // You could add authentication here as well if needed:
                // request.PreAuthenticate = true;
                // request.AuthenticationLevel = System.Net.Security.AuthenticationLevel.MutualAuthRequested;
                // request.Headers.Add("Authorization", "Basic " + Convert.ToBase64String(System.Text.Encoding.Default.GetBytes("username" + ":" + "password")));

                // Send the form data to the request.
                using (Stream requestStream = request.GetRequestStream())
                {
                    requestStream.Write(formData, 0, formData.Length);
                    requestStream.Close();
                }

                return request.GetResponse() as HttpWebResponse;
            }
            private static byte[] GetMultipartFormData(Dictionary<string, object> postParameters, string boundary)
            {
                Stream formDataStream = new System.IO.MemoryStream();
                bool needsCLRF = false;

                foreach (var param in postParameters)
                {
                    // Thanks to feedback from commenters, add a CRLF to allow multiple parameters to be added.
                    // Skip it on the first parameter, add it to subsequent parameters.
                    if (needsCLRF)
                        formDataStream.Write(encoding.GetBytes("\r\n"), 0, encoding.GetByteCount("\r\n"));

                    needsCLRF = true;

                    if (param.Value is FileParameter)
                    {
                        FileParameter fileToUpload = (FileParameter)param.Value;

                        // Add just the first part of this param, since we will write the file data directly to the Stream
                        string header = string.Format("--{0}\r\nContent-Disposition: form-data; name=\"{1}\"; filename=\"{2}\"\r\nContent-Type: {3}\r\n\r\n",
                            boundary,
                            param.Key,
                            fileToUpload.FileName ?? param.Key,
                            fileToUpload.ContentType ?? "application/octet-stream");

                        formDataStream.Write(encoding.GetBytes(header), 0, encoding.GetByteCount(header));

                        // Write the file data directly to the Stream, rather than serializing it to a string.
                        formDataStream.Write(fileToUpload.File, 0, fileToUpload.File.Length);
                    }
                    else
                    {
                        string postData = string.Format("--{0}\r\nContent-Disposition: form-data; name=\"{1}\"\r\n\r\n{2}",
                            boundary,
                            param.Key,
                            param.Value);
                        formDataStream.Write(encoding.GetBytes(postData), 0, encoding.GetByteCount(postData));
                    }
                }

                // Add the end of the request.  Start with a newline
                string footer = "\r\n--" + boundary + "--\r\n";
                formDataStream.Write(encoding.GetBytes(footer), 0, encoding.GetByteCount(footer));

                // Dump the Stream into a byte[]
                formDataStream.Position = 0;
                byte[] formData = new byte[formDataStream.Length];
                formDataStream.Read(formData, 0, formData.Length);
                formDataStream.Close();

                return formData;
            }
            public class FileParameter
            {
                public byte[] File { get; set; }
                public string FileName { get; set; }
                public string ContentType { get; set; }
                public FileParameter(byte[] file) : this(file, null) { }
                public FileParameter(byte[] file, string filename) : this(file, filename, null) { }
                public FileParameter(byte[] file, string filename, string contenttype)
                {
                    File = file;
                    FileName = filename;
                    ContentType = contenttype;
                }
            }
        }
        #endregion


        public async Task<PdfResDcCeplr> CeplrPdfReports_MultipleStatement(CeplrPdfReportDc pdfReportDc, long Leadid)
        {
            long request_id = 0;
            string Responsetoken = "";
            PdfResDcCeplr pdfResDcCeplr = new PdfResDcCeplr();
            using (var db = new AuthContext())
            {
                TextFileLogHelper.TraceLog("pdfReportDc.file FILEURL 1 =" + pdfReportDc.file.ToString());
                string url = "";
                string token = "";
                string CompanyCode = "";
                string ApiSecretKey = "";

                var configdata = GetApiConfig("Cepler");
                var Activitydata = GetActivityMaster("Pdf Reports");
                if (configdata != null && Activitydata != null)
                {
                    url = configdata.URL + Activitydata.URL; //prod=>https://api.ceplr.com/fiu/pdf
                    token = configdata.TokenKey;
                    CompanyCode = configdata.CompanyCode == null ? "" : configdata.CompanyCode.ToString();
                    pdfReportDc.configuration_uuid = Activitydata.ApiSecretKey;
                    ApiSecretKey = Activitydata.TokenKey; // prod=>SAq86Up8yI4f47vJYGYoW7ljhJbRQs8t4j1br7Dn 
                }

                string baseBath = HttpRuntime.AppDomainAppPath;


                Int32 BankFileCount = 0;
                var MultipleBankFle = db.LeadBankStatement.Where(x => x.LeadMasterId == Leadid && x.IsActive == true && x.IsDeleted == false).ToList();
                if (MultipleBankFle.Count > 0)
                { BankFileCount = MultipleBankFle.Count(); }

                if (BankFileCount > 0)
                {
                    foreach (var file in MultipleBankFle)
                    {
                        #region Main Code of Ceplr Report

                        try
                        {

                            TextFileLogHelper.TraceLog("pdfReportDc.file file path=" + string.Concat(HttpRuntime.AppDomainAppPath, pdfReportDc.file.Replace("/", "\\")));

                            var fileurl = string.Concat(HttpRuntime.AppDomainAppPath, pdfReportDc.file.Replace("/", "\\"));
                            pdfReportDc.file = fileurl;

                            TextFileLogHelper.TraceLog("pdfReportDc.file local path=" + pdfReportDc.file);

                            var client = new RestClient(url);
                            client.Timeout = -1;
                            var request = new RestRequest(Method.POST);
                            //// note - 0 is primary file and >0 is chiled file

                            if (file.Sequence == 0)
                            {
                                request.AddFile("file", pdfReportDc.file, "multipart/form-data");
                                request.AddParameter("allow_multiple", BankFileCount == 0 ? false : true);
                            }
                            else if (file.Sequence > 0)
                            {
                                pdfReportDc.file = string.Concat(HttpRuntime.AppDomainAppPath, file.StatementFile.Replace("/", "\\"));
                                request.AddFile("file", pdfReportDc.file, "multipart/form-data");

                                request.AddParameter("allow_multiple", true);
                                request.AddParameter("request_id", request_id);
                                request.AddParameter("token", Responsetoken);
                                request.AddParameter("last_file", true);
                            }
                            request.AddHeader("x-api-key", ApiSecretKey);
                            request.AlwaysMultipartFormData = true;

                            //Path.GetFileNameWithoutExtension(fullFileName), filepath)
                            request.AddHeader("Content-Type", "multipart/form-data");
                            request.AddParameter("email", pdfReportDc.email);
                            request.AddParameter("ifsc_code", pdfReportDc.ifsc_code);

                            request.AddParameter("fip_id", string.IsNullOrEmpty(pdfReportDc.ifsc_code) ? pdfReportDc.fip_id : null);
                            request.AddParameter("callback_url", pdfReportDc.callback_url); //https://internal.er15.xyz/Webhook/ceplr/callback
                            request.AddParameter("mobile", pdfReportDc.mobile);
                            request.AddParameter("name", pdfReportDc.name);
                            request.AddParameter("file_password", (string.IsNullOrEmpty(pdfReportDc.ifsc_code)) ? "" : pdfReportDc.file_password);
                            request.AddParameter("configuration_uuid", pdfReportDc.configuration_uuid); //"Configuration-1717-a723" prod=>Configuration-4302-9654

                            var RequestjsonString = JsonConvert.SerializeObject(pdfReportDc);

                            ServicePointManager.Expect100Continue = true;
                            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                            ArthmateReqResp addRequest = new ArthmateReqResp()
                            {
                                ActivityMasterId = Activitydata.Id,
                                LeadMasterId = Leadid,
                                RequestResponseMsg = RequestjsonString,
                                Type = "Request",
                                Url = url,
                                CreatedBy = 0,
                                CreatedDate = DateTime.Now
                            };
                            var reqsave = InsertRequestResponse(addRequest);



                            IRestResponse response = await client.ExecuteAsync(request);
                            string jsonString = "";
                            if (response.StatusCode == HttpStatusCode.Accepted)
                            {
                                jsonString = (response.Content);
                                pdfResDcCeplr = JsonConvert.DeserializeObject<PdfResDcCeplr>(jsonString);

                                if (Responsetoken == "" && pdfResDcCeplr.data.token != null)
                                {
                                    request_id = pdfResDcCeplr.data.request_id;
                                    Responsetoken = pdfResDcCeplr.data.token;
                                }

                                if (pdfResDcCeplr != null && pdfResDcCeplr.code == 202)
                                {
                                    TextFileLogHelper.TraceLog("Cepler Pdf Reports Api after post response");
                                    TextFileLogHelper.TraceLog("customer_id :" + pdfResDcCeplr.data.customer_id + pdfResDcCeplr.data);
                                    TextFileLogHelper.TraceLog("request_id :" + pdfResDcCeplr.data.request_id);
                                    TextFileLogHelper.TraceLog("request_id :" + pdfReportDc.file);
                                }
                                else
                                {
                                    TextFileLogHelper.TraceLog("Cepler Pdf Reports after Failed response :" + pdfResDcCeplr);
                                }
                            }
                            else
                            {
                                jsonString = (response.Content);
                                var data = JsonConvert.DeserializeObject<PdfCeplrErrorDc>(jsonString);
                                pdfResDcCeplr.message = data.message;
                                pdfResDcCeplr.code = data.code;
                            }
                            ArthmateReqResp AddResponse = new ArthmateReqResp()
                            {
                                ActivityMasterId = Activitydata.Id,
                                LeadMasterId = Leadid,
                                RequestResponseMsg = jsonString,
                                Type = "Response",
                                Url = url,
                                CreatedBy = 0,
                                CreatedDate = DateTime.Now
                            };
                            var res = InsertRequestResponse(AddResponse);

                            //AuthContext auth, long LeadMasterID, string Apiname, string ReqJson, string ResJson
                            // var data = InsertLeadBackgroundRun(Leadid, "Pdf Reports", RequestjsonString, jsonString);
                        }
                        catch (Exception ex)
                        {
                            TextFileLogHelper.TraceLog("(Exception in Cepler Pdf Report ex=" + ex.Message);
                            ArthmateReqResp addRequest = new ArthmateReqResp()
                            {
                                ActivityMasterId = Activitydata.Id,
                                LeadMasterId = Leadid,
                                RequestResponseMsg = ex.Message.ToString(),
                                Type = "Response",
                                Url = url,
                                CreatedBy = 0,
                                CreatedDate = DateTime.Now
                            };
                            var resw = InsertRequestResponse(addRequest);
                        }
                        #endregion


                    }
                }
            }
            return pdfResDcCeplr;
        }


        public async Task<RepaymentV2ResponseDc> RepaymentV2(List<RepaymentV2Dc> data, int userid)
        {

            string url = "";
            string token = "";
            string CompanyCode = "";
            RepaymentV2ResponseDc resdata = new RepaymentV2ResponseDc();
            var configdata = GetApiConfig("ArthMate");
            var Activitydata = GetActivityMaster("RepaymentV2");

            try
            {
                if (configdata != null && Activitydata != null)
                {
                    url = configdata.URL + "/" + Activitydata.URL;
                    token = configdata.TokenKey;
                    CompanyCode = configdata.CompanyCode == null ? "" : configdata.CompanyCode.ToString();
                }
                var RequestjsonString = JsonConvert.SerializeObject(data);

                var client = new RestClient(url);
                client.Timeout = -1;
                var request = new RestRequest(Method.POST);
                request.AddHeader("NoEncryption", "1");
                request.AddHeader("Content-Type", "application/json");
                request.AddHeader("Authorization", token);
                request.AddParameter("application/json", RequestjsonString, ParameterType.RequestBody);

                ServicePointManager.Expect100Continue = true;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                ArthmateReqResp addRequest = new ArthmateReqResp()
                {
                    ActivityMasterId = Activitydata.Id,
                    LeadMasterId = 0,
                    RequestResponseMsg = RequestjsonString,
                    Type = "Request",
                    Url = url,
                    CreatedBy = userid,
                    CreatedDate = DateTime.Now
                };
                var reqsave = InsertRequestResponse(addRequest);

                var response = client.Execute(request);
                string jsonString = "";
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    jsonString = (response.Content);
                    resdata = JsonConvert.DeserializeObject<RepaymentV2ResponseDc>(jsonString);
                }
                else
                {
                    jsonString = (response.Content);
                    resdata = JsonConvert.DeserializeObject<RepaymentV2ResponseDc>(jsonString);
                }
                ArthmateReqResp AddResponse = new ArthmateReqResp()
                {
                    ActivityMasterId = Activitydata.Id,
                    LeadMasterId = 0,
                    RequestResponseMsg = "",
                    Type = "Response",
                    Url = url,
                    CreatedBy = userid,
                    CreatedDate = DateTime.Now
                };
                var res = InsertRequestResponse(AddResponse);


            }
            catch (Exception ex)
            {
                ArthmateReqResp addRequest = new ArthmateReqResp()
                {
                    ActivityMasterId = Activitydata.Id,
                    LeadMasterId = 0,
                    RequestResponseMsg = ex.ToString(),
                    Type = "Response",
                    Url = url,
                    CreatedBy = userid,
                    CreatedDate = DateTime.Now
                };
                var resw = InsertRequestResponse(addRequest);
            }

            return resdata;
        }

    }
}


