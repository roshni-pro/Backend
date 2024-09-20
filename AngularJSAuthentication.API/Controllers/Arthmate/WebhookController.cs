using AngularJSAuthentication.API.Helper.ArthMateHelper;
using AngularJSAuthentication.Common.Helpers;
using AngularJSAuthentication.DataContracts.Arthmate;
using AngularJSAuthentication.DataContracts.Mongo;
using AngularJSAuthentication.Model.Arthmate;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Script.Serialization;
using static AngularJSAuthentication.DataContracts.Arthmate.ArthMatePostDc;

namespace AngularJSAuthentication.API.Controllers.Arthmate
{
    [RoutePrefix("Webhook")]
    public class WebhookController : ApiController
    {
        //public static string Hashkey = "79DAB1734BA82A3B17B7DE763EA5C32A";
        public static string Hashkey = "9V0ERpHNrFGesdcz+I69qUDREKt2svfzhKGicQFAXIc=";//https://www.devglan.com/online-tools/text-encryption-decryption  ==>> skbusinessloan~callback!

        public static List<string> AscoreTxnInProcess = new List<string>();

        private string CeplrPdfReportCallbackUrl = ConfigurationManager.AppSettings["CeplrPdfReportCallbackUrl"];
        [HttpPost]
        [Route("AScore/callback")]
        [AllowAnonymous]
        public async Task<bool> AScoreWebhookResponse(HttpRequestMessage request)
        {

            ////if (!(HttpContext.Current.Request.Headers.GetValues("AirtelUserName")?.FirstOrDefault() == ConfigurationManager.AppSettings["Hashkey"].ToString() ))
            //if (!(HttpContext.Current.Request.Headers.GetValues("AirtelUserName")?.FirstOrDefault() == Hashkey))
            //{
            //    cashCollectionResponse.Message = "Authorization has been denied for this request.";
            //    cashCollectionResponse.status = false;
            //    cashCollectionResponse.CashCollection = new List<currencysettelDc>();
            //    return cashCollectionResponse;
            //}

            TextFileLogHelper.TraceLog("AScore Start");
            var content = request.Content;
            string jsonContent = await content.ReadAsStringAsync();

            TextFileLogHelper.TraceLog("Ascore webhook 1 =" + jsonContent.ToString());

            TextFileLogHelper.TraceLog("Data Deserialize Start =" + jsonContent.ToString());

            var resdata = JsonConvert.DeserializeObject<AScoreWebhookDc>(jsonContent);
            TextFileLogHelper.TraceLog("Data Deserialize End");

            #region prevent duplicate request
            if (resdata.request_id != null)
            {
                ArthMateWebhookResponse WebhookResponse = new ArthMateWebhookResponse();
                WebhookResponse.RequestId = resdata.request_id;
                WebhookResponse.WebhookName = "AScore";
                WebhookResponse.Response = jsonContent;
                WebhookResponse.CreatedDate = DateTime.Now;
                WebhookResponse.IsActive = true;
                InsertCallBackResponseInMongo(WebhookResponse);
            }
            #endregion
            if (resdata.status == "success")
            {

                #region to stop duplicate req
                if (resdata.request_id != null)
                {
                    if (AscoreTxnInProcess != null && AscoreTxnInProcess.Any(x => x == resdata.request_id))
                    {
                        TextFileLogHelper.TraceLog("TxnInProcess Already in Queue: " + resdata.request_id);
                        return false;
                    }
                    else
                    {
                        AscoreTxnInProcess.Add(resdata.request_id);
                    }
                }
                #endregion
                using (var db = new AuthContext())
                {


                    var lead = db.AScore.Where(x => x.request_id == resdata.request_id && x.IsActive == true && x.IsDeleted == false).FirstOrDefault();
                    var configdata = db.LoanConfiguration.FirstOrDefault();
                    if (lead != null)
                    {
                        ArthMateController arthMateController = new ArthMateController();
                        arthMateController.LeadActivityProgressesHistory(lead.LeadMasterId, 0, 0, "GetMonthlySalary", "Ascore webhook Start (Request: " + jsonContent.ToString() + ")");


                        if (!db.CoLenderResponse.Any(x => x.AScoreRequest_id == lead.request_id && x.IsActive == true && x.pricing > 0 && x.loan_amount > 0))
                        {
                            TextFileLogHelper.TraceLog("AScoreWebhookResponse: Ascore webhook lead ");

                            var leadMaster = db.LeadMasters.Where(x => x.Id == lead.LeadMasterId && x.IsActive == true && x.IsDeleted == false).FirstOrDefault();

                            var LeadId = new SqlParameter("@leadmasterid", lead.LeadMasterId);
                            CeplrPdf_SpData leaddata = db.Database.SqlQuery<CeplrPdf_SpData>("EXEC [Arthmate].[CeplrPdfReportData] @leadmasterid", LeadId).FirstOrDefault();
                            AngularJSAuthentication.API.Helper.ArthMateHelper.ArthMateHelper ArthMateHelper = new AngularJSAuthentication.API.Helper.ArthMateHelper.ArthMateHelper();

                            var documentId = db.ArthMateDocumentMasters.FirstOrDefault(x => x.DocumentName == "pan_card").Id;

                            //PanVerificationRequestJson Panvalidateobj = new PanVerificationRequestJson();
                            //Panvalidateobj.pan = leadMaster.appl_pan;
                            //Panvalidateobj.loan_app_id = leadMaster.loan_app_id;
                            //Panvalidateobj.consent = "Y";
                            //Panvalidateobj.consent_timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                            PanVerificationRequestV3 Panvalidateobj = new PanVerificationRequestV3();
                            Panvalidateobj.pan = leadMaster.appl_pan;
                            Panvalidateobj.name = (string.IsNullOrEmpty(leadMaster.first_name) ? "" : leadMaster.first_name) + " " + (string.IsNullOrEmpty(leadMaster.middle_name) ? "" : leadMaster.middle_name) + " " + (string.IsNullOrEmpty(leadMaster.last_name) ? "" : leadMaster.last_name);
                            Panvalidateobj.father_name = (string.IsNullOrEmpty(leadMaster.father_fname) ? "" : leadMaster.father_fname) + " " + (string.IsNullOrEmpty(leadMaster.father_mname) ? "" : leadMaster.father_mname) + " " + (string.IsNullOrEmpty(leadMaster.father_lname) ? "" : leadMaster.father_lname);
                            Panvalidateobj.dob = leadMaster.dob;
                            Panvalidateobj.loan_app_id = leadMaster.loan_app_id;
                            Panvalidateobj.consent = "Y";
                            Panvalidateobj.consent_timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                            var Panvalidate = await ArthMateHelper.PanVerificationV3Async(Panvalidateobj, leadMaster.Id);

                            //string resjson = Newtonsoft.Json.JsonConvert.SerializeObject(Panvalidate);


                            KYCValidationResponse kyc = new KYCValidationResponse();
                            kyc.LeadMasterId = leadMaster.Id;
                            kyc.DocumentMasterId = Convert.ToInt32(documentId);
                            kyc.kyc_id = Panvalidate.kyc_id;
                            kyc.Status = Panvalidate.success == true ? "success" : "failed";
                            kyc.ResponseJson = Panvalidate.KYCResponse;
                            kyc.Message = Panvalidate.success == true ? "success" : "failed";
                            kyc.CreatedDate = DateTime.Now;
                            kyc.ModifiedDate = null;
                            kyc.IsDeleted = false;
                            kyc.IsActive = true;
                            db.KYCValidationResponse.Add(kyc);
                            db.Commit();
                            TextFileLogHelper.TraceLog("AScoreWebhookResponse: Ascore Panvalidate");
                            //ArthMateController arthMateController = new ArthMateController();
                            await arthMateController.PanAndAadharXmlUpload(leadMaster.Id, db);

                            CeplrPdfReportDc pdfReportDc = new CeplrPdfReportDc();
                            if (leaddata != null)
                            {
                                pdfReportDc.file = leaddata.FrontFileUrl;
                                pdfReportDc.callback_url = CeplrPdfReportCallbackUrl;
                                pdfReportDc.file_password = leaddata.PdfPassword;
                                pdfReportDc.email = leaddata.email_id;
                                pdfReportDc.ifsc_code = leaddata.borro_bank_ifsc;
                                var Bankdata = db.CeplrBankList.Where(x => x.fip_name == leaddata.borro_bank_name).FirstOrDefault();
                                if (Bankdata != null)
                                {
                                    pdfReportDc.fip_id = string.IsNullOrEmpty(Bankdata.pdf_fip_id.ToString()) ? Bankdata.pdf_fip_id.ToString() : null;
                                }
                                else
                                {
                                    pdfReportDc.fip_id = null;
                                }
                                pdfReportDc.mobile = leaddata.MobileNo;
                                pdfReportDc.name = leaddata.first_name;
                                string LogoUrl = "";
                                TextFileLogHelper.TraceLog("AScoreWebhookResponse: Ascore webhook 01 fip_id =");

                                PdfResDcCeplr ress = await ArthMateHelper.CeplrPdfReports(pdfReportDc, lead.LeadMasterId);

                                TextFileLogHelper.TraceLog("AScoreWebhookResponse: Ascore webhook 3 =" + ress.code.ToString());
                                if (ress.code == 202)
                                {
                                    TextFileLogHelper.TraceLog("AScoreWebhookResponse: Ascore webhook 202 =");
                                    TextFileLogHelper.TraceLog("AScoreWebhookResponse: Code =success" + pdfReportDc.file);
                                    CeplrPdfReports pdfinsert = new CeplrPdfReports();
                                    pdfinsert.LeadMasterId = lead.LeadMasterId;
                                    pdfinsert.request_id = ress.data.request_id;
                                    pdfinsert.customer_id = ress.data.customer_id;
                                    pdfinsert.FileName = pdfReportDc.file;
                                    pdfinsert.file_password = pdfReportDc.file_password;
                                    pdfinsert.callback_url = pdfReportDc.callback_url;
                                    pdfinsert.IsActive = true;
                                    pdfinsert.IsDeleted = false;
                                    pdfinsert.CreatedBy = 0;
                                    pdfinsert.CreatedDate = DateTime.Now;
                                    pdfinsert.ModifiedBy = 0;
                                    pdfinsert.ModifiedDate = null;
                                    db.CeplrPdfReports.Add(pdfinsert);
                                    db.Commit();
                                    arthMateController.LeadActivityProgressesHistory(lead.LeadMasterId, 0, 0, "Cepler Generated", "4. Ceplr Generated By Webhook Successfully");

                                    var AScoreRequest_id = lead.request_id;

                                    var ceplr_cust_id = ress.data.customer_id;
                                    ////TextFileLogHelper.TraceLog(" AScoreWebhookResponse: MonthlySalary Start:Plase 1 ");
                                    //double MonthlySalary = await ArthMateHelper.GetMonthlySalary(lead.LeadMasterId, db);
                                    double MonthlySalary = leadMaster.Bus_MonthlySalary;
                                    ////TextFileLogHelper.TraceLog(" AScoreWebhookResponse: MonthlySalary End:Plase 1 ");

                                    arthMateController.LeadActivityProgressesHistory(lead.LeadMasterId, 0, 0, "GetMonthlySalary", "Get monthly salary (" + MonthlySalary + ") by ceplr basic report");

                                    CoLenderRequest colenderObj = new CoLenderRequest();

                                    colenderObj.first_name = leadMaster.first_name;
                                    colenderObj.last_name = leadMaster.last_name;
                                    colenderObj.dob = leadMaster.dob;
                                    colenderObj.appl_pan = leadMaster.appl_pan;
                                    colenderObj.gender = leadMaster.gender;
                                    colenderObj.appl_phone = leadMaster.appl_phone;
                                    colenderObj.address = leadMaster.per_addr_ln1;
                                    colenderObj.city = leadMaster.city;
                                    colenderObj.state = leadMaster.state;
                                    colenderObj.pincode = leadMaster.pincode;
                                    colenderObj.enquiry_purpose = "61";
                                    colenderObj.bureau_type = "cibil";
                                    colenderObj.tenure = leadMaster.tenure;
                                    colenderObj.request_id_a_score = AScoreRequest_id;
                                    colenderObj.request_id_b_score = "";
                                    colenderObj.ceplr_cust_id = ceplr_cust_id;
                                    colenderObj.interest_rate = ""; // Note- get from loan master
                                    colenderObj.product_type_code = "UBLN";
                                    colenderObj.sanction_amount = leadMaster.EnquiryAmount;
                                    colenderObj.dscr = 0;  // Note- need to discuss
                                    colenderObj.monthly_income = MonthlySalary; // Note- get from ceplr
                                    colenderObj.loan_app_id = leadMaster.loan_app_id;
                                    colenderObj.consent = "Y";
                                    colenderObj.consent_timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");


                                    var res = await ArthMateHelper.CoLenderApi(colenderObj, 1, leadMaster.Id);
                                    var colender = db.CoLenderResponse.Where(x => x.LeadMasterId == leadMaster.Id && x.IsActive == true && x.IsDeleted == false).FirstOrDefault();

                                    if (colender == null)
                                    {
                                        CoLenderResponse obj = new CoLenderResponse();
                                        obj.LeadMasterId = leadMaster.Id;
                                        obj.request_id = res.request_id;
                                        obj.loan_amount = Convert.ToDouble(res.loan_amount);
                                        obj.SanctionAmount = Convert.ToDouble(res.loan_amount);
                                        obj.pricing = configdata.InterestRate;//Convert.ToDouble(res.pricing);
                                        obj.co_lender_shortcode = res.co_lender_shortcode;
                                        obj.loan_app_id = res.loan_app_id;
                                        obj.co_lender_assignment_id = res.co_lender_assignment_id;
                                        obj.co_lender_full_name = res.co_lender_full_name;
                                        obj.status = res.status;
                                        obj.CreatedDate = DateTime.Now;
                                        obj.ModifiedDate = null;
                                        obj.IsActive = true;
                                        obj.IsDeleted = false;
                                        obj.CreatedBy = 1;
                                        obj.ModifiedBy = null;
                                        obj.AScoreRequest_id = AScoreRequest_id;
                                        obj.ceplr_cust_id = ceplr_cust_id;
                                        //obj.ProgramType = string.IsNullOrEmpty(res.program_type) ? "" : res.program_type;
                                        //05-03-2024
                                        obj.ProgramType = (!string.IsNullOrEmpty(res.program_type) && res.program_type == "Transaction_POS") ? "Transactions" : res.program_type;
                                        db.CoLenderResponse.Add(obj);
                                    }
                                    else
                                    {
                                        colender.IsActive = false;
                                        colender.IsDeleted = true;
                                        colender.ModifiedDate = DateTime.Now;
                                        db.Entry(colender).State = EntityState.Modified;
                                        db.Commit();

                                        CoLenderResponse obj = new CoLenderResponse();
                                        obj.LeadMasterId = leadMaster.Id;
                                        obj.request_id = res.request_id;
                                        obj.loan_amount = Convert.ToDouble(res.loan_amount);
                                        obj.SanctionAmount = Convert.ToDouble(res.loan_amount);
                                        obj.pricing = configdata.InterestRate; //Convert.ToDouble(res.pricing);
                                        obj.co_lender_shortcode = res.co_lender_shortcode;
                                        obj.loan_app_id = res.loan_app_id;
                                        obj.co_lender_assignment_id = res.co_lender_assignment_id;
                                        obj.co_lender_full_name = res.co_lender_full_name;
                                        obj.status = res.status;
                                        obj.CreatedDate = DateTime.Now;
                                        obj.ModifiedDate = null;
                                        obj.IsActive = true;
                                        obj.IsDeleted = false;
                                        obj.CreatedBy = 1;
                                        obj.ModifiedBy = null;
                                        obj.AScoreRequest_id = AScoreRequest_id;
                                        obj.ceplr_cust_id = ceplr_cust_id;
                                        //05-03-2024
                                        obj.ProgramType = (!string.IsNullOrEmpty(res.program_type) && res.program_type == "Transaction_POS") ? "Transactions" : res.program_type;
                                        db.CoLenderResponse.Add(obj);
                                    }
                                    if (res != null && Convert.ToDouble(res.loan_amount) > 0 && Convert.ToDouble(configdata.InterestRate) > 0)
                                    {
                                        var ActivitySequence = db.ArthMateActivitySequence.FirstOrDefault(x => x.ScreenName.Trim() == "InProgress" && x.IsActive == true);

                                        leadMaster.SequenceNo = Convert.ToInt32(ActivitySequence.SequenceNo);
                                        leadMaster.ModifiedDate = DateTime.Now;
                                        db.Entry(leadMaster).State = System.Data.Entity.EntityState.Modified;

                                        arthMateController.LeadActivityProgressesHistory(leadMaster.Id, 0, 0, "Colender(Offer) Generated", "5. Colender Generated By Webhook Successfully");
                                    }
                                    if (db.Commit() > 0)
                                    {
                                        await UpdateLeadCurrentActivity(leadMaster.Id, db, leadMaster.SequenceNo);

                                    }
                                    TextFileLogHelper.TraceLog("AScoreWebhookResponse: A Score Commit Succesfully ");
                                    AscoreTxnInProcess.RemoveAll(x => x == resdata.request_id);
                                }
                                else
                                {
                                    AscoreTxnInProcess.RemoveAll(x => x == resdata.request_id);

                                }
                            }
                        }
                    }
                }
            }
            else
            {
                TextFileLogHelper.TraceLog("AScoreWebhookResponse: A Score Status Error ");
                AscoreTxnInProcess.RemoveAll(x => x == resdata.request_id);

                return false;
            }
            TextFileLogHelper.TraceLog("AScoreWebhookResponse: A Score Status Error ");
            AscoreTxnInProcess.RemoveAll(x => x == resdata.request_id);
            return true;
        }


        [HttpGet]
        [Route("RemoveAscoreId/{AscoreId}")]
        [AllowAnonymous]
        private bool RemoveAscoreId(string AscoreId)
        {
            AscoreTxnInProcess.RemoveAll(x => x == AscoreId);
            return true;
        }
        private async Task<bool> UpdateLeadCurrentActivity(long LeadMasterId, AuthContext db, int ArthmateActivitySequenceNo)
        {
            if (db == null)
            {
                db = new AuthContext();
            }
            if (LeadMasterId > 0)
            {
                //var LeadId = new SqlParameter("@LeadMasterId", LeadMasterId);
                //var ActivityId = new SqlParameter("@ArthmateActivitySequenceNo", ArthmateActivitySequenceNo);
                db.Database.ExecuteSqlCommand(string.Format("EXEC [Arthmate].[UpdateLeadCurrentActivity] {0},{1}", LeadMasterId, ArthmateActivitySequenceNo));

            }

            return true;
        }



        [HttpPost]
        [Route("TestAScoreWebhookResponse")]
        [AllowAnonymous]
        public async Task<bool> TestAScoreWebhookResponse(long leadmasterid)
        {
            try
            {
                using (var db = new AuthContext())
                {
                    var AScore = db.AScore.Where(x => x.LeadMasterId == leadmasterid && x.IsDeleted == false && x.IsActive).FirstOrDefault();
                    var configdata = db.LoanConfiguration.FirstOrDefault();
                    if (leadmasterid > 0)
                    {
                        var leadmaster = db.LeadMasters.Where(x => x.Id == leadmasterid && x.IsActive == true && x.IsDeleted == false).FirstOrDefault();
                        var LeadId = new SqlParameter("@leadmasterid", leadmasterid);
                        CeplrPdf_SpData leaddata = db.Database.SqlQuery<CeplrPdf_SpData>("EXEC [Arthmate].[CeplrPdfReportData] @leadmasterid", LeadId).FirstOrDefault();
                        AngularJSAuthentication.API.Helper.ArthMateHelper.ArthMateHelper ArthMateHelper = new AngularJSAuthentication.API.Helper.ArthMateHelper.ArthMateHelper();

                        var documentId = db.ArthMateDocumentMasters.FirstOrDefault(x => x.DocumentName == "pan_card").Id;
                        //PanVerificationRequestJson Panvalidateobj = new PanVerificationRequestJson();
                        //Panvalidateobj.pan = leadmaster.appl_pan;
                        //Panvalidateobj.loan_app_id = leadmaster.loan_app_id;
                        //Panvalidateobj.consent = "Y";
                        //Panvalidateobj.consent_timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                        //var Panvalidate = await ArthMateHelper.PanVerificationAsync(Panvalidateobj, leadmaster.Id);


                        PanVerificationRequestV3 Panvalidateobj = new PanVerificationRequestV3();
                        Panvalidateobj.pan = leadmaster.appl_pan;
                        Panvalidateobj.name = (string.IsNullOrEmpty(leadmaster.first_name) ? "" : leadmaster.first_name) + " " + (string.IsNullOrEmpty(leadmaster.middle_name) ? "" : leadmaster.middle_name) + " " + (string.IsNullOrEmpty(leadmaster.last_name) ? "" : leadmaster.last_name);
                        Panvalidateobj.father_name = (string.IsNullOrEmpty(leadmaster.father_fname) ? "" : leadmaster.father_fname) + " " + (string.IsNullOrEmpty(leadmaster.father_mname) ? "" : leadmaster.father_mname) + " " + (string.IsNullOrEmpty(leadmaster.father_lname) ? "" : leadmaster.father_lname);
                        Panvalidateobj.dob = leadmaster.dob;
                        Panvalidateobj.loan_app_id = leadmaster.loan_app_id;
                        Panvalidateobj.consent = "Y";
                        Panvalidateobj.consent_timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                        var Panvalidate = await ArthMateHelper.PanVerificationV3Async(Panvalidateobj, leadmaster.Id);



                        //string resjson = Newtonsoft.Json.JsonConvert.SerializeObject(Panvalidate);

                        KYCValidationResponse kyc = new KYCValidationResponse();
                        kyc.LeadMasterId = leadmasterid;
                        kyc.DocumentMasterId = Convert.ToInt32(documentId);
                        kyc.kyc_id = Panvalidate.kyc_id;
                        kyc.Status = Panvalidate.success == true ? "success" : "failed";
                        kyc.ResponseJson = Panvalidate.KYCResponse;
                        kyc.Message = "";// Panvalidate.data.msg; //Panvalidate.success == true ? "success" : "failed";
                        kyc.CreatedDate = DateTime.Now;
                        kyc.ModifiedDate = null;
                        kyc.IsDeleted = false;
                        kyc.IsActive = true;
                        db.KYCValidationResponse.Add(kyc);
                        db.Commit();
                        ArthMateController arthMateController = new ArthMateController();
                        await arthMateController.PanAndAadharXmlUpload(leadmasterid, db);

                        CeplrPdfReportDc pdfReportDc = new CeplrPdfReportDc();
                        if (leaddata != null)
                        {
                            pdfReportDc.file = leaddata.FrontFileUrl;
                            pdfReportDc.callback_url = CeplrPdfReportCallbackUrl;
                            pdfReportDc.file_password = leaddata.PdfPassword;
                            pdfReportDc.email = leaddata.email_id;
                            pdfReportDc.ifsc_code = leaddata.borro_bank_ifsc;

                            pdfReportDc.mobile = leaddata.MobileNo;
                            pdfReportDc.name = leaddata.first_name;

                            var Bankdata = db.CeplrBankList.Where(x => x.fip_name == leaddata.borro_bank_name).FirstOrDefault();
                            //pdfReportDc.fip_id = Bankdata.aa_fip_id;
                            pdfReportDc.fip_id = string.IsNullOrEmpty(Bankdata.pdf_fip_id.ToString()) ? Bankdata.pdf_fip_id.ToString() : null;
                            string LogoUrl = "";
                            PdfResDcCeplr ress = await ArthMateHelper.CeplrPdfReports(pdfReportDc, leadmasterid);
                            if (ress.code == 202)
                            {
                                TextFileLogHelper.TraceLog("Code =success" + pdfReportDc.file);
                                CeplrPdfReports pdfinsert = new CeplrPdfReports();
                                pdfinsert.LeadMasterId = leadmasterid;
                                pdfinsert.request_id = ress.data.request_id;
                                pdfinsert.customer_id = ress.data.customer_id;
                                pdfinsert.FileName = pdfReportDc.file;
                                pdfinsert.file_password = pdfReportDc.file_password;
                                pdfinsert.callback_url = pdfReportDc.callback_url;
                                pdfinsert.IsActive = true;
                                pdfinsert.IsDeleted = false;
                                pdfinsert.CreatedBy = 0;
                                pdfinsert.CreatedDate = DateTime.Now;
                                pdfinsert.ModifiedBy = 0;
                                pdfinsert.ModifiedDate = null;
                                db.CeplrPdfReports.Add(pdfinsert);
                                db.Commit();
                                arthMateController.LeadActivityProgressesHistory(leadmasterid, 0, 0, "Cepler Generated", "4. Ceplr Generated By Webhook Successfully");

                                //double MonthlySalary = await ArthMateHelper.GetMonthlySalary(leadmasterid, db);
                                double MonthlySalary = leadmaster.Bus_MonthlySalary;

                                CoLenderRequest colenderObj = new CoLenderRequest();

                                colenderObj.first_name = leadmaster.first_name;
                                colenderObj.last_name = leadmaster.last_name;
                                colenderObj.dob = leadmaster.dob;
                                colenderObj.appl_pan = leadmaster.appl_pan;
                                colenderObj.gender = leadmaster.gender;
                                colenderObj.appl_phone = leadmaster.appl_phone;
                                colenderObj.address = leadmaster.per_addr_ln1;
                                colenderObj.city = leadmaster.city;
                                colenderObj.state = leadmaster.state;
                                colenderObj.pincode = leadmaster.pincode;
                                colenderObj.enquiry_purpose = "61";
                                colenderObj.bureau_type = "cibil";
                                colenderObj.tenure = leadmaster.tenure;
                                colenderObj.request_id_a_score = AScore.request_id;
                                colenderObj.request_id_b_score = "";
                                colenderObj.ceplr_cust_id = ress.data.customer_id;
                                colenderObj.interest_rate = ""; // Note- get from loan master
                                colenderObj.product_type_code = "UBLN";
                                colenderObj.sanction_amount = (int)leadmaster.EnquiryAmount;
                                colenderObj.dscr = 0;  // Note- need to discuss
                                colenderObj.monthly_income = MonthlySalary; // Note- get from ceplr
                                colenderObj.loan_app_id = leadmaster.loan_app_id;
                                colenderObj.consent = "Y";
                                colenderObj.consent_timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                                AngularJSAuthentication.API.Helper.ArthMateHelper.ArthMateHelper arthMateHelper = new AngularJSAuthentication.API.Helper.ArthMateHelper.ArthMateHelper();
                                var res = await arthMateHelper.CoLenderApi(colenderObj, 1, leadmaster.Id);

                                CoLenderResponse obj = new CoLenderResponse();
                                obj.LeadMasterId = leadmasterid;
                                obj.request_id = res.request_id;
                                obj.loan_amount = Convert.ToDouble(res.loan_amount);
                                obj.SanctionAmount = Convert.ToDouble(res.loan_amount);
                                obj.pricing = configdata.InterestRate;//Convert.ToDouble(res.pricing);
                                obj.co_lender_shortcode = res.co_lender_shortcode;
                                obj.loan_app_id = res.loan_app_id;
                                obj.co_lender_assignment_id = res.co_lender_assignment_id;
                                obj.co_lender_full_name = res.co_lender_full_name;
                                obj.status = res.status;
                                obj.CreatedDate = DateTime.Now;
                                obj.ModifiedDate = null;
                                obj.IsActive = true;
                                obj.IsDeleted = false;
                                obj.CreatedBy = 1;
                                obj.ModifiedBy = null;
                                obj.ceplr_cust_id = ress.data.customer_id;
                                obj.AScoreRequest_id = AScore.request_id;
                                //obj.ProgramType = string.IsNullOrEmpty(res.program_type) ? "" : res.program_type;
                                //05-03-2024
                                obj.ProgramType = (!string.IsNullOrEmpty(res.program_type) && res.program_type == "Transaction_POS") ? "Transactions" : res.program_type;
                                db.CoLenderResponse.Add(obj);

                                if (res != null && Convert.ToDouble(res.loan_amount) > 0 && Convert.ToDouble(configdata.InterestRate) > 0)
                                {
                                    var ActivitySequence = db.ArthMateActivitySequence.FirstOrDefault(x => x.ScreenName.Trim() == "InProgress" && x.IsActive == true);

                                    leadmaster.SequenceNo = Convert.ToInt32(ActivitySequence.SequenceNo);
                                    leadmaster.ModifiedDate = DateTime.Now;
                                    db.Entry(leadmaster).State = System.Data.Entity.EntityState.Modified;
                                    arthMateController.LeadActivityProgressesHistory(leadmasterid, 0, 0, "Colender Generated (TestAScoreWebhookResponse) ", "5. Colender Generated By Webhook Successfully");
                                }
                                if (db.Commit() > 0)
                                {
                                    await UpdateLeadCurrentActivity(leadmaster.Id, db, leadmaster.SequenceNo);

                                }
                            }

                        }
                    }
                }
            }
            catch (Exception ex)
            {
                TextFileLogHelper.TraceLog("TestAScoreWebhookResponse:fail" + ex.Message);
                return false;
            }

            return true;
        }
        [Route("ceplr/callback")]
        [AllowAnonymous]
        public async Task<string> CeplrCallbackApi(HttpRequestMessage request)
        {
            string callBackres = "";

            var content = request.Content;
            string jsonContent = await content.ReadAsStringAsync();
            var resdata = JsonConvert.DeserializeObject<CeplrCallbackResponse>(jsonContent);
            using (AuthContext context = new AuthContext())
            {
                if (resdata != null)
                {
                    var ceplerData = context.CeplrPdfReports.FirstOrDefault(x => x.customer_id == resdata.data.customer_uuid);

                    ArthmateReqResp addRequest = new ArthmateReqResp()
                    {
                        ActivityMasterId = Convert.ToInt64(0),
                        LeadMasterId = ceplerData.LeadMasterId,
                        RequestResponseMsg = jsonContent,
                        Type = "Response",
                        Url = "ceplr/callback",
                        CreatedBy = 0,
                        CreatedDate = DateTime.Now
                    };
                    ArthMateHelper arthMateHelper = new ArthMateHelper();
                    var reqsave = arthMateHelper.InsertRequestResponse(addRequest);
                    if (resdata.code == 200)//"code": 200
                    {
                        callBackres = resdata.data.msg;
                    }
                }
            }
            return callBackres;
        }
        [HttpPost]
        [Route("eSign/callback")]
        [AllowAnonymous]
        public async Task<bool> eSignWebhookResponse()//HttpRequestMessage request
        {
            //TextFileLogHelper.TraceLog("eSign Start");
            //var content = request.Content;
            //string jsonContent = await content.ReadAsStringAsync();

            var jsonString = String.Empty;
            //string ContentString = await request.Content.ReadAsStringAsync();
            HttpContext.Current.Request.InputStream.Position = 0;
            using (var inputStream = new StreamReader(HttpContext.Current.Request.InputStream))
            {
                jsonString = inputStream.ReadToEnd();
            }
            TextFileLogHelper.TraceLog("eSign Response jsonString :" + jsonString);

            JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
            object serJsonDetails = javaScriptSerializer.Deserialize(jsonString, typeof(object));

            eSignResponseNew Response = JsonConvert.DeserializeObject<eSignResponseNew>(jsonString);

            TextFileLogHelper.TraceLog("eSign Response serJsonDetails :" + serJsonDetails);
            //eSignResponseDc Response = JsonConvert.DeserializeObject<eSignResponseDc>(str1);

            //TextFileLogHelper.TraceLog("eSign Response start :" + Response);

            if (Response.webhookStatus && Response.webhookStatusCode == 101)
            {
                ArthmateReqResp addRequest = new ArthmateReqResp()
                {
                    ActivityMasterId = Convert.ToInt64(0),
                    LeadMasterId = 0,
                    RequestResponseMsg = jsonString,
                    Type = "Request",
                    Url = "ceplr / callback",
                    CreatedBy = 0,
                    CreatedDate = DateTime.Now
                };
                ArthMateHelper arthMateHelper = new ArthMateHelper();
                var reqsave = arthMateHelper.InsertRequestResponse(addRequest);
            }
            return true;
        }

        [HttpPost]
        [Route("CompositeDisbursement/callback")]
        [AllowAnonymous]
        public async Task<bool> CompositeDisbursement(HttpRequestMessage request)
        {
            TextFileLogHelper.TraceLog("CompositeDisbursement Start");
            var content = request.Content;
            string jsonContent = await content.ReadAsStringAsync();
            TextFileLogHelper.TraceLog("CompositeDisbursement webhook 1 =" + jsonContent.ToString());

            TextFileLogHelper.TraceLog("Data Deserialize Start =" + jsonContent.ToString());

            var resdata = JsonConvert.DeserializeObject<CompositeDisbursementWebhookDc>(jsonContent);

            TextFileLogHelper.TraceLog("Data Deserialize End");

            #region prevent duplicate request

            if (resdata.event_key != null)
            {
                ArthMateWebhookResponse WebhookResponse = new ArthMateWebhookResponse();

                WebhookResponse.RequestId = resdata.CallBackData.loan_id;
                WebhookResponse.WebhookName = "CompositeDisbursement";
                WebhookResponse.Response = jsonContent;
                WebhookResponse.CreatedDate = DateTime.Now;
                WebhookResponse.IsActive = true;
                InsertCallBackResponseInMongo(WebhookResponse);
            }

            #endregion

            using (var db = new AuthContext())
            {
                var leadloan = db.LeadLoan.Where(x => x.loan_id == resdata.CallBackData.loan_id && x.IsActive == true && x.IsDeleted == false).FirstOrDefault();
                if (leadloan != null)
                {
                    CompositeDisbursementWebhookResponse obj = new CompositeDisbursementWebhookResponse();
                    obj.Response = resdata.ToString();
                    obj.status_code = resdata.CallBackData.status_code;
                    obj.loan_id = resdata.CallBackData.loan_id;
                    obj.partner_loan_id = resdata.CallBackData.partner_loan_id;
                    obj.net_disbur_amt = resdata.CallBackData.net_disbur_amt;
                    obj.utr_number = resdata.CallBackData.utr_number;
                    obj.utr_date_time = resdata.CallBackData.utr_date_time;
                    obj.txn_id = resdata.CallBackData.txn_id;
                    obj.LeadMasterId = (long)leadloan.LeadMasterId;
                    obj.IsActive = true;
                    obj.IsDeleted = false;
                    obj.CreatedDate = DateTime.Now;
                    db.CompositeDisbursementWebhookResponse.Add(obj);

                    if (!db.ArthmateDisbursements.Any(x => x.loan_id == obj.loan_id))
                    {
                        db.ArthmateDisbursements.Add(new ArthmateDisbursement
                        {
                            loan_id = obj.loan_id,
                            partner_loan_id = obj.partner_loan_id,
                            net_disbur_amt = obj.net_disbur_amt,
                            utr_date_time = obj.utr_date_time,
                            utr_number = obj.utr_number,
                            status_code = obj.status_code,
                            CreatedDate = DateTime.Now

                        });

                    }

                    var leadMaster = db.LeadMasters.FirstOrDefault(x => x.Id == leadloan.LeadMasterId && x.IsActive == true && x.IsDeleted == false);
                    var ActivitySequence = db.ArthMateActivitySequence.FirstOrDefault(x => x.ScreenName.Trim() == "Congratulations" && x.IsActive == true);

                    leadMaster.SequenceNo = Convert.ToInt32(ActivitySequence.SequenceNo);
                    leadMaster.ModifiedDate = DateTime.Now;
                    db.Entry(leadMaster).State = System.Data.Entity.EntityState.Modified;

                    await UpdateLeadCurrentActivity(leadMaster.Id, db, leadMaster.SequenceNo);

                    db.Commit();
                    TextFileLogHelper.TraceLog("Data saved");
                    ArthMateController arthMateController = new ArthMateController();
                    arthMateController.LeadActivityProgressesHistory((long)leadloan.LeadMasterId, 0, 0, "CompositeDisbursement Generated", "Disbursement Generated By Webhook Successfully");
                }

                TextFileLogHelper.TraceLog("Data saved");
            }

            return true;
        }

        public bool InsertCallBackResponseInMongo(ArthMateWebhookResponse obj)
        {
            MongoHelper<ArthMateWebhookResponse> mongoHelper = new MongoHelper<ArthMateWebhookResponse>();
            mongoHelper.Insert(obj);
            return true;
        }
    }
}

public class AScoreWebhookDc
{
    public string request_id { get; set; }
    public string status { get; set; }
}


public class Data
{
    public string customer_uuid { get; set; }
    public string link_uuid { get; set; }
    public string msg { get; set; }
    public int request_id { get; set; }
}

public class CeplrCallbackResponse
{
    public DateTime createdAt { get; set; }
    public string id { get; set; }
    public Data data { get; set; }
    public int code { get; set; }
}

public class Request
{
    public string expiryDate { get; set; }
    public bool expired { get; set; }
    public string phone { get; set; }
    public bool rejected { get; set; }
    public string name { get; set; }
    public bool active { get; set; }
    public bool signed { get; set; }
    public string signType { get; set; }
    public string signUrl { get; set; }
    public string email { get; set; }
}

public class eSignResponseNew
{
    public Request request { get; set; }
    public List<string> files { get; set; }
    public string mac { get; set; }
    public Verification verification { get; set; }
    public Signer signer { get; set; }
    public bool webhookStatus { get; set; }
    public int webhookStatusCode { get; set; }
    public string documentId { get; set; }
}

public class Signer
{
    public string pincode { get; set; }
    public string serialNumber { get; set; }
    public string gender { get; set; }
    public string name { get; set; }
    public string state { get; set; }
    public string title { get; set; }
    public string yob { get; set; }
    public string photoHash { get; set; }
}

public class Verification
{
}