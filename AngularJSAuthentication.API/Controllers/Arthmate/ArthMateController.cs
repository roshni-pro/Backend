using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Http;
using AngularJSAuthentication.API.Helper.ArthMateHelper;
using AngularJSAuthentication.DataContracts.Arthmate;
using AngularJSAuthentication.DataContracts.External.SalesAppDc;
using AngularJSAuthentication.Model.Arthmate;
using System.Web;
using System.IO;
using System.Data.SqlClient;
using static AngularJSAuthentication.DataContracts.Arthmate.ArthMatePostDc;
using AgileObjects.AgileMapper;
using AngularJSAuthentication.Common.Constants;
using System.Configuration;
using System.Transactions;
using System.Data;
using System.Data.Entity.Infrastructure;
using IsolationLevel = System.Data.IsolationLevel;
using System.Globalization;
using Newtonsoft.Json;
using static AngularJSAuthentication.DataContracts.Arthmate.AdharDigiLocker;
using AngularJSAuthentication.Common.ArthmateEnums;
using static AngularJSAuthentication.DataContracts.Arthmate.FirstAadharDc;
using static AngularJSAuthentication.DataContracts.Arthmate.SecondAadharDc;
using iTextSharp.text.pdf;
using AngularJSAuthentication.Common.Helpers;
using OpenHtmlToPdf;
using System.Drawing;
using AngularJSAuthentication.Common.Helpers.ReportMaker;
using AngularJSAuthentication.API.Helper;
using iTextSharp.text;
using System.Text;
using Rectangle = iTextSharp.text.Rectangle;
using Microsoft.VisualBasic;
using RestSharp;
using NPOI.XSSF.UserModel;
using System.Runtime.Serialization.Formatters.Binary;
using NPOI.SS.UserModel;
using System.Web.Script.Serialization;

namespace AngularJSAuthentication.API.Controllers.Arthmate
{
    [RoutePrefix("api/ArthMate")]
    public class ArthMateController : ApiController
    {
        public static TimeZoneInfo INDIAN_ZONE = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
        DateTime indianTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE);
        private string CeplrPdfReportCallbackUrl = ConfigurationManager.AppSettings["CeplrPdfReportCallbackUrl"];
        private bool GenerateEsign = Convert.ToBoolean(ConfigurationManager.AppSettings["GenerateEsign"]);

        string sLoan_sanction_letter = "loan_sanction_letter";
        string sSigned_loan_sanction_letter = "agreement";//"signed_loan_sanction_letter";
        string LoanStatus = "kyc_data_approved";
        #region  Api For Mobile App View

        [AllowAnonymous]
        [HttpGet]
        [Route("GetSequence")]
        public async Task<CommonResponseDc> GetSequence(long LeadMasterId)
        {
            CommonResponseDc results = new CommonResponseDc();
            using (var db = new AuthContext())
            {
                var Sequence = db.Database.SqlQuery<SequenceDc>("exec  [Arthmate].[GetSequence]").ToList();
                bool IsEditable = false;
                Sequence.ForEach(x => x.IsEditable = true);
                if (LeadMasterId > 0)
                {
                    var leadid = new SqlParameter("@LeadMasterId", LeadMasterId);
                    var seqno = db.Database.SqlQuery<LeadSequenceData>("EXEC [Arthmate].[LeadNextSequenceno] @LeadMasterId", leadid).FirstOrDefault();
                    if (seqno.sequenceNo <= 6 || seqno.IsApproved == 2)
                    {
                        IsEditable = true;// can edit 
                        if (seqno.sequenceNo == 3 && seqno.IsApproved == 2 && seqno.IsComplete == true || (seqno.IsComplete == false || seqno.IsApproved == 0))//reject case
                        {
                            IsEditable = true;
                            seqno.IsAadharOtp = true;
                        }
                        else if (seqno.sequenceNo == 3 && seqno.IsApproved == 1 && seqno.IsComplete == true)
                        {
                            IsEditable = false;
                        }
                        else if (seqno.sequenceNo == 4 && seqno.IsApproved == 1 && seqno.IsComplete == true)
                        {
                            IsEditable = false;
                        }
                    }
                    if (seqno.sequenceNo <= 6)
                        Sequence.Where(x => x.SequenceNo <= 6).ToList().ForEach(x => x.IsEditable = true);
                    else
                        Sequence.FirstOrDefault(x => x.SequenceNo == seqno.sequenceNo).IsEditable = IsEditable;
                }
                results.Data = Sequence;
                results.Status = true;
            }
            return results;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="SkCode"></param>
        /// <param name="MobileNo"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpGet]
        [Route("GetLeadMasterBySkcode")]
        public async Task<CommonResponseDc> GetLeadMasterBySkcode(string SkCode, string MobileNo)
        {
            // Table: LeadMaster.SequenceNo == ArthMateActivitySequences.ID
            // Table: LeadActivityMasterProgresses.ActivityMasterId == ArthMateActivitySequences.ID

            CommonResponseDc results = new CommonResponseDc();
            if (String.IsNullOrEmpty(SkCode) && String.IsNullOrEmpty(MobileNo))
            {
                results.Status = false;
                return results;
            }
            using (var authContext = new AuthContext())
            {
                var lead = authContext.LeadMasters.Where(x => x.SkCode.Trim().ToLower() == SkCode.Trim().ToLower()).FirstOrDefault();
                if (lead != null)
                {
                    if (lead.fip_id != null)
                    {
                        lead.fip_name = authContext.CeplrBankList.Where(x => x.aa_fip_id == lead.fip_id).Select(x => x.fip_name).FirstOrDefault();
                    }
                    //if (lead.SequenceNo == 8)
                    if (lead.SequenceNo == Convert.ToInt32((authContext.ArthMateActivitySequence.FirstOrDefault(x => x.ScreenName.Trim() == "InProgress" && x.IsActive == true).SequenceNo)))
                    {
                        var offer = authContext.CoLenderResponse.Where(x => x.LeadMasterId == lead.Id && x.IsActive == true && x.IsDeleted == false && x.status == "success").OrderByDescending(x => x.Id).FirstOrDefault();
                        if (offer != null)
                        {
                            var data = new ArthMateOfferDc
                            {
                                loan_amt = offer.SanctionAmount == 0 ? offer.loan_amount : offer.SanctionAmount,
                                interest_rt = offer.pricing,
                                loan_tnr = lead.tenure > 0 ? lead.tenure : 36,
                                loan_tnr_type = "Month",
                                Orignal_loan_amt = offer.loan_amount,
                            };
                            results.ArthMateOffer = data;
                        }

                    }
                    else
                    {
                        var param = new SqlParameter("@leadmasterid", lead.Id);
                        var data = authContext.Database.SqlQuery<LeadDocUrlDc>("EXEC [Arthmate].[GetReturnLeadData] @leadmasterid", param).ToList();
                        results.leadDocUrlDcs = data;

                    }

                    var leadid = new SqlParameter("@LeadMasterId", lead.Id);
                    var seqno = authContext.Database.SqlQuery<LeadSequenceData>("EXEC [Arthmate].[LeadNextSequenceno] @LeadMasterId", leadid).FirstOrDefault();

                    results.IsNotEditable = false;
                    seqno.sequenceNo = seqno.sequenceNo - 1;
                    if (lead.SequenceNo == 7 && seqno.IsApproved == 0)
                    {
                        results.IsNotEditable = true;
                    }
                    else if (seqno.IsApproved == 2)
                    {
                        results.IsNotEditable = false;
                    }

                    lead.SequenceNo = seqno.sequenceNo < 0 ? 0 : seqno.sequenceNo;
                    results.Data = lead;
                    results.Status = true;

                    return results;
                }
            }
            TransactionOptions option = new TransactionOptions();
            option.IsolationLevel = System.Transactions.IsolationLevel.RepeatableRead;
            option.Timeout = TimeSpan.FromSeconds(60);
            using (var authContext = new AuthContext())
            {
                string incre_Borro_id = "";
                string PartnerLoanAppId = SkCode + "-" + MobileNo;
                incre_Borro_id = SkCode + "00001";
                LeadMaster lead = new LeadMaster();
                lead.partner_loan_app_id = PartnerLoanAppId;
                lead.partner_borrower_id = incre_Borro_id;
                lead.SkCode = SkCode;
                lead.MobileNo = MobileNo;
                lead.appl_phone = MobileNo;
                lead.CreatedDate = DateTime.Now;
                lead.SequenceNo = 0;
                lead.IsActive = true;
                lead.IsDeleted = false;
                lead.tenure = 36;
                lead.purpose_of_loan = "61";
                authContext.LeadMasters.Add(lead);
                authContext.Commit();
                authContext.Database.ExecuteSqlCommand(string.Format("EXEC [Arthmate].[InsertLeadActivityProgresses] {0},{1}", lead.Id, 1));
                results.Data = lead;
                results.Status = true;
            }
            return results;

        } ///

        [AllowAnonymous]
        [HttpGet]
        [Route("GetLeadMasterById/{LeadMasterId}")]
        public async Task<CommonResponseDc> GetLeadMasterById(long LeadMasterId)
        {
            CommonResponseDc results = new CommonResponseDc();
            using (var authContext = new AuthContext())
            {
                var lead = authContext.LeadMasters.Where(x => x.Id == LeadMasterId && x.IsActive == true && x.IsDeleted == false).FirstOrDefault();
                if (lead != null)
                {
                    results.Data = lead;
                    results.Status = true;
                }
            }
            return results;

        }


        [AllowAnonymous]
        [HttpGet]
        [Route("DocumentNoExists/{LeadMasterId}/{DocumentNo}/{Seq}")]
        public async Task<CommonResponseDc> DocumentNoExists(long LeadMasterId, string DocumentNo, int Seq = 0)
        {
            CommonResponseDc results = new CommonResponseDc();
            using (var db = new AuthContext())
            {
                var loanid = new SqlParameter("@LeadMasterId", LeadMasterId);
                var documentno = new SqlParameter("@DocumentNo", DocumentNo);
                //results.Status = db.Database.SqlQuery<bool>("exec [Arthmate].[DocumentNoExists] @LeadMasterId, @DocumentNo", loanid, documentno).FirstOrDefault();
                //if (results.Status)
                //{
                //    results.Msg = "Document No Already exists.";
                //}

                if (db.Database.Connection.State != ConnectionState.Open)
                    db.Database.Connection.Open();
                var cmd = db.Database.Connection.CreateCommand();
                cmd.CommandText = "[Arthmate].[DocumentNoExists]";
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.Parameters.Add(loanid);
                cmd.Parameters.Add(documentno);

                var reader = cmd.ExecuteReader();

                bool statusa = ((IObjectContextAdapter)db).ObjectContext.Translate<bool>(reader).FirstOrDefault();
                reader.NextResult();

                string NameOnCard = ((IObjectContextAdapter)db).ObjectContext.Translate<string>(reader).FirstOrDefault();

                if (statusa)
                {
                    results.Msg = "Document No Already exists.";
                    results.NameOnCard = NameOnCard;
                    results.Status = statusa;
                }
                else
                {
                    //results.Msg = "Document No Already exists.";\
                    if (Seq == 1)
                    {
                        ArthMateHelper ArthMateHelper = new ArthMateHelper();
                        var ValidAuthenticationPanres = await ArthMateHelper.ValidAuthenticationPan(DocumentNo, LeadMasterId);
                        if (ValidAuthenticationPanres.StatusCode != 101 || ValidAuthenticationPanres.error != null) //101 mean succesfully 
                        {
                            results.Msg = ValidAuthenticationPanres.error;
                            //results.NameOnCard = "";
                            results.Status = true;
                        }
                        else
                        {
                            results.NameOnCard = ValidAuthenticationPanres.person.name;
                        }
                    }

                }

            }
            return results;
        }


        #region 1st Pan Authentication
        [Route("KarzaPanVerify")]
        [HttpPost]
        public async Task<CommonResponseDc> KarzaPanVerify(LeadPanNewDc UpdatePanInfo)
        {
            CommonResponseDc res = new CommonResponseDc();
            using (AuthContext db = new AuthContext())
            {
                var lead = db.LeadMasters.Where(x => x.Id == UpdatePanInfo.LeadmasterId && x.IsActive == true && x.IsDeleted == false).FirstOrDefault();
                var leadid = new SqlParameter("@LeadMasterId", lead.Id);
                var LeadActivity = db.Database.SqlQuery<LeadSequenceData>("EXEC [Arthmate].[LeadNextSequenceno] @LeadMasterId", leadid).FirstOrDefault();

                if ((lead.SequenceNo == 0 || lead.SequenceNo == 1) || UpdatePanInfo.SequenceNo == 1)
                {
                    var document = db.ArthMateDocumentMasters.Select(x => new { x.Id, x.DocumentName, x.DocumentTypeCode }).ToList();
                    ArthMateHelper ArthMateHelper = new ArthMateHelper();
                    var ValidAuthenticationPanres = await ArthMateHelper.ValidAuthenticationPan(UpdatePanInfo.PanNo, UpdatePanInfo.LeadmasterId);
                    if (ValidAuthenticationPanres.StatusCode != 101 || ValidAuthenticationPanres.error != null) //101 mean succesfully 
                    {
                        res.Status = false;
                        res.Msg = ValidAuthenticationPanres.error;
                        return res;
                    }
                    var ocrverifyres = await ArthMateHelper.PanVerificationWithOCRAsync(UpdatePanInfo.PanImage, ValidAuthenticationPanres.request_id, lead.Id);

                    if (ocrverifyres != null && ocrverifyres.error == null && ValidAuthenticationPanres.StatusCode == 101)
                    {
                        if (UpdatePanInfo.PanNo != ocrverifyres.result.FirstOrDefault(c => c.type == "Pan").details.panNo.value && ocrverifyres.statusCode == 101)
                        {
                            res.Status = false;
                            res.Msg = "Pan no. not match with uploaded pan image";
                        }
                        else
                        {
                            PanDocumentDc obj = new PanDocumentDc();
                            obj.NameOnCard = ValidAuthenticationPanres.person.name;// ocrverifyres.result.FirstOrDefault(c => c.type == "Pan").details.name.value;
                            if (ocrverifyres.result.FirstOrDefault(c => c.type == "Pan").details.date.value != null)
                            {
                                obj.DateOfBirth = ocrverifyres.result.FirstOrDefault(c => c.type == "Pan").details.date.value;
                            }
                            obj.IssuedDate = Convert.ToString(ocrverifyres.result.FirstOrDefault(c => c.type == "Pan").details.dateOfIssue.value);
                            obj.FatherName = ocrverifyres.result.FirstOrDefault(c => c.type == "Pan").details.father.value;
                            obj.OtherInfo = Newtonsoft.Json.JsonConvert.SerializeObject(ocrverifyres.OtherInfo);

                            // PAN Image
                            LeadLoanDocument doc = new LeadLoanDocument();

                            var documentId = db.ArthMateDocumentMasters.FirstOrDefault(x => x.DocumentName == "pan_card" && x.IsActive == true && x.IsDeleted == false).Id;
                            var leadDoc = db.LeadDocument.Where(x => x.LeadMasterId == UpdatePanInfo.LeadmasterId && x.DocumentMasterId == documentId && x.IsActive == true && x.IsDeleted == false).FirstOrDefault();

                            if (leadDoc == null)
                            {
                                doc.LeadMasterId = lead.Id;
                                doc.DocumentMasterId = document.FirstOrDefault(x => x.DocumentName == "pan_card").Id;
                                doc.DocumentNumber = UpdatePanInfo.PanNo;
                                doc.FrontFileUrl = UpdatePanInfo.PanImage;
                                doc.NameOnCard = obj.NameOnCard;
                                //doc.DateOfBirth = DateTime.ParseExact(obj.DateOfBirth.InnerText,"yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture); ;
                                doc.DateOfBirth = obj.DateOfBirth;
                                doc.IssuedDate = obj.IssuedDate;
                                doc.OtherInfo = obj.OtherInfo;
                                doc.CreatedDate = DateTime.Now;
                                doc.CreatedBy = 0;
                                doc.ModifiedDate = null;
                                doc.ModifiedBy = null;
                                doc.IsVerified = true;
                                doc.IsActive = true;
                                doc.IsDeleted = false;
                                db.LeadDocument.Add(doc);
                            }
                            else
                            {
                                leadDoc.DocumentNumber = UpdatePanInfo.PanNo;
                                leadDoc.FrontFileUrl = UpdatePanInfo.PanImage;

                                leadDoc.NameOnCard = obj.NameOnCard;
                                leadDoc.DateOfBirth = obj.DateOfBirth;
                                leadDoc.IssuedDate = obj.IssuedDate;
                                leadDoc.OtherInfo = obj.OtherInfo;

                                leadDoc.ModifiedDate = DateTime.Now;
                                leadDoc.ModifiedBy = (int)UpdatePanInfo.LeadmasterId;

                                db.Entry(leadDoc).State = EntityState.Modified;
                            }

                            string[] name = obj.NameOnCard.ToString().Trim().Split(new char[] { ' ' }, 3);
                            if (name[0] != null)
                            {
                                lead.first_name = name[0];
                            }
                            if (name.Length > 1 && name[1] != null)
                            {
                                if (name.Length == 2)
                                {
                                    lead.last_name = name[1];
                                }
                                else
                                {
                                    lead.middle_name = name[1];
                                }
                            }
                            if (name.Length > 2 && name[2] != null)
                            {
                                lead.last_name = name[2];
                            }

                            lead.last_name = string.IsNullOrEmpty(lead.last_name) ? "." : lead.last_name;  //01-03-2024
                            lead.middle_name = string.IsNullOrEmpty(lead.middle_name) ? "" : lead.middle_name;  //01-03-2024

                            string[] fatherName = obj.FatherName.ToString().Trim().Split(new char[] { ' ' }, 3);
                            if (fatherName[0] != null)
                            {
                                lead.father_fname = fatherName[0];
                            }
                            if (fatherName.Length > 1 && fatherName[1] != null)
                            {
                                if (fatherName.Length == 2)
                                {
                                    lead.father_lname = fatherName[1];
                                }
                                else
                                {
                                    lead.father_mname = fatherName[1];
                                }
                            }
                            if (fatherName.Length > 2 && fatherName[2] != null)
                            {
                                lead.father_lname = fatherName[2];
                            }

                            lead.appl_pan = UpdatePanInfo.PanNo;
                            //lead.dob = obj.DateOfBirth;
                            lead.dob = DateFormatReturn(obj.DateOfBirth);
                            lead.age = Math.Abs(Convert.ToDouble(DateTime.ParseExact(DateFormatReturn(obj.DateOfBirth), "yyyy-MM-dd", null).Year - DateTime.Now.Year));
                            lead.ModifiedDate = DateTime.Now;

                            if (LeadActivity.IsApproved == 0)//&& LeadActivity.IsComplete == true)
                            {
                                lead.SequenceNo = UpdatePanInfo.SequenceNo;
                            }
                            db.Entry(lead).State = EntityState.Modified;

                            //await UpdateLeadCurrentActivity(lead.Id, db, lead.SequenceNo);
                            await UpdateLeadCurrentActivity(lead.Id, db, UpdatePanInfo.SequenceNo);
                            if (db.Commit() > 0)
                            {
                                res.Status = true;
                                res.Msg = "PAN Varified Successfully";

                                LeadActivityProgressesHistory(UpdatePanInfo.LeadmasterId, UpdatePanInfo.SequenceNo, 0, "", "PAN Varified Successfully");
                            }
                        }
                    }
                    else
                    {
                        res.Status = false;
                        res.Msg = ocrverifyres.error;
                        LeadActivityProgressesHistory(UpdatePanInfo.LeadmasterId, UpdatePanInfo.SequenceNo, 0, "", "PAN Not Varified");
                    }
                }
                else
                {
                    res.Status = true;
                    res.Msg = "PAN Varified Successfully";
                }
                return res;
            }
        }
        #endregion



        [Route("KarzaPanVerifyV2testing")]
        [HttpPost]
        public async Task<CommonResponseDc> KarzaPanVerificationNew(LeadPanNewDc UpdatePanInfo)
        {
            CommonResponseDc res = new CommonResponseDc();
            using (AuthContext db = new AuthContext())
            {
                var lead = db.LeadMasters.Where(x => x.Id == UpdatePanInfo.LeadmasterId && x.IsActive == true && x.IsDeleted == false).FirstOrDefault();
                var leadid = new SqlParameter("@LeadMasterId", lead.Id);
                var LeadActivity = db.Database.SqlQuery<LeadSequenceData>("EXEC [Arthmate].[LeadNextSequenceno] @LeadMasterId", leadid).FirstOrDefault();

                if ((lead.SequenceNo == 0 || lead.SequenceNo == 1) || UpdatePanInfo.SequenceNo == 1)
                {
                    var document = db.ArthMateDocumentMasters.Select(x => new { x.Id, x.DocumentName, x.DocumentTypeCode }).ToList();
                    ArthMateHelper ArthMateHelper = new ArthMateHelper();
                    //var ValidAuthenticationPanres = await ArthMateHelper.ValidAuthenticationPan(UpdatePanInfo.PanNo, UpdatePanInfo.LeadmasterId);
                    LeadPanNewKarzaDc leadPanNewKarzaDc = new LeadPanNewKarzaDc();
                    leadPanNewKarzaDc.dob = UpdatePanInfo.dob;
                    leadPanNewKarzaDc.name = UpdatePanInfo.Name;
                    leadPanNewKarzaDc.PanNo = UpdatePanInfo.PanNo;
                    var ValidAuthenticationPanres = await KarzaPanVerifyNew(leadPanNewKarzaDc);

                    if (Convert.ToInt32(ValidAuthenticationPanres.statuscode) != 101 || ValidAuthenticationPanres.result == null) //101 mean succesfully 
                    {
                        res.Status = false;
                        res.Msg = ValidAuthenticationPanres.result.status == null || ValidAuthenticationPanres.result.status == "" ? "Something Mismatch try Again" : ValidAuthenticationPanres.result.status;
                        return res;
                    }
                    var ocrverifyres = await ArthMateHelper.PanVerificationWithOCRAsync(UpdatePanInfo.PanImage, ValidAuthenticationPanres.request_id, lead.Id);

                    if (ocrverifyres != null && ocrverifyres.error == null && Convert.ToInt32(ValidAuthenticationPanres.statuscode) == 101)
                    {
                        if (UpdatePanInfo.PanNo != ocrverifyres.result.FirstOrDefault(c => c.type == "Pan").details.panNo.value && ocrverifyres.statusCode == 101)
                        {
                            res.Status = false;
                            res.Msg = "Pan no. not match with uploaded pan image";
                        }
                        else
                        {
                            PanDocumentDc obj = new PanDocumentDc();
                            //obj.NameOnCard = ValidAuthenticationPanres.person.name;
                            obj.NameOnCard = ocrverifyres.result.FirstOrDefault(c => c.type == "Pan").details.name.value;
                            if (ocrverifyres.result.FirstOrDefault(c => c.type == "Pan").details.date.value != null)
                            {
                                obj.DateOfBirth = ocrverifyres.result.FirstOrDefault(c => c.type == "Pan").details.date.value;
                            }
                            obj.IssuedDate = Convert.ToString(ocrverifyres.result.FirstOrDefault(c => c.type == "Pan").details.dateOfIssue.value);
                            obj.FatherName = ocrverifyres.result.FirstOrDefault(c => c.type == "Pan").details.father.value;
                            obj.OtherInfo = Newtonsoft.Json.JsonConvert.SerializeObject(ocrverifyres.OtherInfo);

                            // PAN Image
                            LeadLoanDocument doc = new LeadLoanDocument();

                            var documentId = db.ArthMateDocumentMasters.FirstOrDefault(x => x.DocumentName == "pan_card" && x.IsActive == true && x.IsDeleted == false).Id;
                            var leadDoc = db.LeadDocument.Where(x => x.LeadMasterId == UpdatePanInfo.LeadmasterId && x.DocumentMasterId == documentId && x.IsActive == true && x.IsDeleted == false).FirstOrDefault();

                            if (leadDoc == null)
                            {
                                doc.LeadMasterId = lead.Id;
                                doc.DocumentMasterId = document.FirstOrDefault(x => x.DocumentName == "pan_card").Id;
                                doc.DocumentNumber = UpdatePanInfo.PanNo;
                                doc.FrontFileUrl = UpdatePanInfo.PanImage;
                                doc.NameOnCard = obj.NameOnCard;
                                //doc.DateOfBirth = DateTime.ParseExact(obj.DateOfBirth.InnerText,"yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture); ;
                                doc.DateOfBirth = obj.DateOfBirth;
                                doc.IssuedDate = obj.IssuedDate;
                                doc.OtherInfo = obj.OtherInfo;
                                doc.CreatedDate = DateTime.Now;
                                doc.CreatedBy = 0;
                                doc.ModifiedDate = null;
                                doc.ModifiedBy = null;
                                doc.IsVerified = true;
                                doc.IsActive = true;
                                doc.IsDeleted = false;
                                db.LeadDocument.Add(doc);
                            }
                            else
                            {
                                leadDoc.DocumentNumber = UpdatePanInfo.PanNo;
                                leadDoc.FrontFileUrl = UpdatePanInfo.PanImage;

                                leadDoc.NameOnCard = obj.NameOnCard;
                                leadDoc.DateOfBirth = obj.DateOfBirth;
                                leadDoc.IssuedDate = obj.IssuedDate;
                                leadDoc.OtherInfo = obj.OtherInfo;

                                leadDoc.ModifiedDate = DateTime.Now;
                                leadDoc.ModifiedBy = (int)UpdatePanInfo.LeadmasterId;

                                db.Entry(leadDoc).State = EntityState.Modified;
                            }

                            string[] name = obj.NameOnCard.ToString().Trim().Split(new char[] { ' ' }, 3);
                            if (name[0] != null)
                            {
                                lead.first_name = name[0];
                            }
                            if (name.Length > 1 && name[1] != null)
                            {
                                if (name.Length == 2)
                                {
                                    lead.last_name = name[1];
                                }
                                else
                                {
                                    lead.middle_name = name[1];
                                }
                            }
                            if (name.Length > 2 && name[2] != null)
                            {
                                lead.last_name = name[2];
                            }

                            lead.last_name = string.IsNullOrEmpty(lead.last_name) ? "." : lead.last_name;  //01-03-2024
                            lead.middle_name = string.IsNullOrEmpty(lead.middle_name) ? "" : lead.middle_name;  //01-03-2024

                            string[] fatherName = obj.FatherName.ToString().Trim().Split(new char[] { ' ' }, 3);
                            if (fatherName[0] != null)
                            {
                                lead.father_fname = fatherName[0];
                            }
                            if (fatherName.Length > 1 && fatherName[1] != null)
                            {
                                if (fatherName.Length == 2)
                                {
                                    lead.father_lname = fatherName[1];
                                }
                                else
                                {
                                    lead.father_mname = fatherName[1];
                                }
                            }
                            if (fatherName.Length > 2 && fatherName[2] != null)
                            {
                                lead.father_lname = fatherName[2];
                            }

                            lead.appl_pan = UpdatePanInfo.PanNo;
                            //lead.dob = obj.DateOfBirth;
                            lead.dob = DateFormatReturn(obj.DateOfBirth);
                            lead.age = Math.Abs(Convert.ToDouble(DateTime.ParseExact(DateFormatReturn(obj.DateOfBirth), "yyyy-MM-dd", null).Year - DateTime.Now.Year));
                            lead.ModifiedDate = DateTime.Now;

                            if (LeadActivity.IsApproved == 0)
                            {
                                lead.SequenceNo = UpdatePanInfo.SequenceNo;
                            }
                            db.Entry(lead).State = EntityState.Modified;


                            await UpdateLeadCurrentActivity(lead.Id, db, UpdatePanInfo.SequenceNo);
                            if (db.Commit() > 0)
                            {
                                res.Status = true;
                                res.Msg = "PAN Varified Successfully";

                                LeadActivityProgressesHistory(UpdatePanInfo.LeadmasterId, UpdatePanInfo.SequenceNo, 0, "", "PAN Varified Successfully");
                            }
                        }
                    }
                    else
                    {
                        res.Status = false;
                        res.Msg = ocrverifyres.error;
                        LeadActivityProgressesHistory(UpdatePanInfo.LeadmasterId, UpdatePanInfo.SequenceNo, 0, "", "PAN Not Varified");
                    }
                }
                else
                {
                    res.Status = true;
                    res.Msg = "PAN Varified Successfully";
                }
                return res;
            }
        }


        [Route("KarzaPanVerifyNew")]  //https://testapi.karza.in/v2/pan-authentication
        [HttpPost]
        [AllowAnonymous]
        public async Task<ResponseKarzaNewV2> KarzaPanVerifyNew(LeadPanNewKarzaDc UpdatePanInfos)
        {
            //CommonResponseDc res = new CommonResponseDc();
            ResponseKarzaNewV2 responseKarza = new ResponseKarzaNewV2();
            using (AuthContext db = new AuthContext())
            {
                KarzaAuthenticationPanV2 requestJson = new KarzaAuthenticationPanV2();
                if (UpdatePanInfos != null)
                {
                    requestJson.clientData = null;
                    requestJson.pan = UpdatePanInfos.PanNo;
                    requestJson.name = UpdatePanInfos.name;
                    requestJson.dob = UpdatePanInfos.dob;
                    requestJson.consent = "Y";

                }
                string jsonData = JsonConvert.SerializeObject(requestJson);

                ArthMateHelper arthMateHelper = new ArthMateHelper();
                var apiConfigdata = arthMateHelper.GetUrlTokenForApi("Karza", "PanAuthenticationV2");
                if (apiConfigdata != null)
                {
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
                    var client = new RestClient(apiConfigdata.url);
                    var request = new RestRequest(Method.POST);
                    request.AddHeader("x-karza-key", apiConfigdata.ApiSecretKey);
                    request.AddHeader("Content-Type", "application/json");

                    request.AddParameter("application/json", jsonData, ParameterType.RequestBody);
                    IRestResponse response = await client.ExecuteAsync(request);

                    string jsonString = "";
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        jsonString = (response.Content);
                        ArthmateReqResp AddResponse = new ArthmateReqResp()
                        {
                            ActivityMasterId = Convert.ToInt64(apiConfigdata.id),
                            LeadMasterId = UpdatePanInfos.LeadmasterId,
                            RequestResponseMsg = jsonString,
                            Type = "Response",
                            Url = apiConfigdata.url,
                            CreatedBy = 0,
                            CreatedDate = DateTime.Now
                        };
                        var ress = arthMateHelper.InsertRequestResponse(AddResponse);
                        responseKarza = JsonConvert.DeserializeObject<ResponseKarzaNewV2>(jsonString);
                        // res.Data = responseKarza;
                    }
                    else
                    {
                        jsonString = (response.Content);
                        ArthmateReqResp AddResponse = new ArthmateReqResp()
                        {
                            ActivityMasterId = Convert.ToInt64(apiConfigdata.id),
                            LeadMasterId = UpdatePanInfos.LeadmasterId,
                            RequestResponseMsg = jsonString,
                            Type = "Response",
                            Url = apiConfigdata.url,
                            CreatedBy = 0,
                            CreatedDate = DateTime.Now
                        };
                        var res2 = arthMateHelper.InsertRequestResponse(AddResponse);
                        responseKarza = JsonConvert.DeserializeObject<ResponseKarzaNewV2>(jsonString);
                        // res.Data = responseKarza;
                    }
                }
                else
                {
                    responseKarza.result = null;
                    responseKarza.statuscode = "Please Check Your Configuration(ApiConfig)";
                }
                //var result = arthMateHelper.ValidAuthenticationKarzaNew(UpdatePanInfos);


            }
            return responseKarza;
        }




        [Route("Addselfie")]
        [HttpPost]
        public async Task<CommonResponseDc> Addselfie(AddSelfieDc AddSelfie)
        {
            CommonResponseDc res = new CommonResponseDc();
            using (AuthContext db = new AuthContext())
            {
                var lead = db.LeadMasters.Where(x => x.Id == AddSelfie.LeadmasterId && x.IsActive == true && x.IsDeleted == false).FirstOrDefault();
                if (lead != null)
                {
                    var document = db.ArthMateDocumentMasters.Where(x => x.DocumentName == "selfie" && x.IsActive == true && x.IsDeleted == false).FirstOrDefault();
                    //var leadloandata = db.LeadDocument.Where(x => x.DocumentMasterId == document.Id && x.LeadMasterId == lead.Id).ToList();
                    //foreach (var tt in leadloandata)
                    //{
                    //    tt.IsActive = false;
                    //    tt.IsDeleted = true;
                    //}
                    //db.Entry(leadloandata).State = EntityState.Modified;
                    //db.Commit();
                    //if (document != null)
                    //{
                    LeadLoanDocument doc = new LeadLoanDocument();

                    var documentId = document.Id;// db.ArthMateDocumentMasters.FirstOrDefault(x => x.DocumentName == "aadhar_card" && x.IsActive == true && x.IsDeleted == false).Id;
                    var leadDoc = db.LeadDocument.Where(x => x.LeadMasterId == AddSelfie.LeadmasterId && x.DocumentMasterId == documentId && x.IsActive == true && x.IsDeleted == false).FirstOrDefault();
                    if (leadDoc == null)
                    {
                        doc.LeadMasterId = lead.Id;
                        doc.DocumentMasterId = document.Id;
                        doc.FrontFileUrl = AddSelfie.Selfie;
                        doc.CreatedDate = DateTime.Now;
                        doc.CreatedBy = 0;
                        doc.ModifiedDate = null;
                        doc.ModifiedBy = null;
                        doc.IsVerified = true;
                        doc.IsActive = true;
                        doc.IsDeleted = false;
                        db.LeadDocument.Add(doc);
                    }
                    else
                    {
                        leadDoc.FrontFileUrl = AddSelfie.Selfie;
                        leadDoc.ModifiedDate = DateTime.Now;
                        leadDoc.ModifiedBy = (int)AddSelfie.LeadmasterId;
                        leadDoc.OtherInfo = "selfie";
                        db.Entry(leadDoc).State = EntityState.Modified;
                    }

                    var leadid = new SqlParameter("@LeadMasterId", AddSelfie.LeadmasterId);
                    var seqno = db.Database.SqlQuery<LeadSequenceData>("EXEC [Arthmate].[LeadNextSequenceno] @LeadMasterId", leadid).FirstOrDefault();
                    if (seqno.IsApproved == 0)//&& seqno.IsComplete == true)
                    {
                        lead.SequenceNo = AddSelfie.SequenceNo;
                        lead.ModifiedDate = DateTime.Now;
                        db.Entry(lead).State = EntityState.Modified;

                    }
                    //await UpdateLeadCurrentActivity(lead.Id, db, lead.SequenceNo);
                    await UpdateLeadCurrentActivity(lead.Id, db, AddSelfie.SequenceNo);
                    if (db.Commit() > 0)
                    {
                        res.Status = true;
                        res.Msg = "selfie uploaded Successfully";
                        LeadActivityProgressesHistory(AddSelfie.LeadmasterId, AddSelfie.SequenceNo, 0, "", "selfie uploaded Successfully");
                    }
                }
            }

            return res;
        }



        // 2nd Aadhaar Authentication
        [AllowAnonymous]
        [HttpPost]
        [Route("AddAadhaar")]
        public async Task<CommonResponseDc> AddAadhaar(GetAdharDc getAdharDc)
        {
            CommonResponseDc res = new CommonResponseDc();

            using (AuthContext db = new AuthContext())
            {
                if (getAdharDc != null)
                {
                    var lead = db.LeadMasters.Where(x => x.Id == getAdharDc.LeadMasterId && x.IsActive == true && x.IsDeleted == false).FirstOrDefault();
                    var documentId = db.ArthMateDocumentMasters.FirstOrDefault(x => x.DocumentName == "aadhar_card" && x.IsActive == true && x.IsDeleted == false).Id;
                    if (lead != null)
                    {
                        var leadDoc = db.LeadDocument.Where(x => x.LeadMasterId == getAdharDc.LeadMasterId && x.DocumentMasterId == documentId && x.IsActive == true && x.IsDeleted == false).FirstOrDefault();
                        if (leadDoc == null)
                        {
                            LeadLoanDocument leadLoanDocument = new LeadLoanDocument();
                            leadLoanDocument.DocumentMasterId = documentId;
                            leadLoanDocument.BackFileUrl = getAdharDc.BackFileUrl;
                            leadLoanDocument.FrontFileUrl = getAdharDc.FrontFileUrl;
                            leadLoanDocument.CreatedBy = (int)getAdharDc.LeadMasterId;
                            leadLoanDocument.CreatedDate = DateTime.Now;
                            leadLoanDocument.DocumentNumber = getAdharDc.DocumentNumber;
                            leadLoanDocument.IsActive = true;
                            leadLoanDocument.IsDeleted = false;
                            leadLoanDocument.IsVerified = false;
                            leadLoanDocument.LeadMasterId = lead.Id;
                            db.LeadDocument.Add(leadLoanDocument);
                        }
                        else
                        {
                            leadDoc.DocumentMasterId = documentId;
                            leadDoc.BackFileUrl = getAdharDc.BackFileUrl;
                            leadDoc.FrontFileUrl = getAdharDc.FrontFileUrl;
                            leadDoc.ModifiedDate = DateTime.Now;
                            leadDoc.ModifiedBy = (int)getAdharDc.LeadMasterId;
                            leadDoc.DocumentNumber = getAdharDc.DocumentNumber;
                            db.Entry(leadDoc).State = EntityState.Modified;
                        }
                        lead.SequenceNo = getAdharDc.SequenceNo;
                        lead.ModifiedDate = DateTime.Now;
                        lead.aadhar_card_num = getAdharDc.DocumentNumber;
                        db.Entry(lead).State = EntityState.Modified;
                        db.Commit();

                        ArthMateHelper ArthMateHelper = new ArthMateHelper();
                        var responseDC = await ArthMateHelper.eAdharDigilockerOTPXml(getAdharDc);
                        if (responseDC.result != null)
                        {
                            res.Msg = responseDC.result.message;
                            res.Data = responseDC.requestId;
                            res.Status = true;
                        }
                        else
                        {
                            res.Msg = responseDC.error.error.message;
                            res.Data = responseDC.requestId;
                            res.Status = false;
                        }

                    }
                }
            }
            return res;
        }
        [HttpPost]
        [Route("KarzaAadharOTPValidate")]
        public async Task<CommonResponseDc> KarzaAadharOTPValidate(UpdateAadhaarVerificationRequestDC AadharObj)
        {
            using (var db = new AuthContext())
            {
                CommonResponseDc res = new CommonResponseDc();
                if (AadharObj.otp != null && AadharObj.requestId != null)
                {
                    var lead = db.LeadMasters.Where(x => x.Id == AadharObj.LeadMasterId && x.IsActive == true && x.IsDeleted == false).FirstOrDefault();
                    if (lead != null)
                    {
                        ArthMateHelper ArthMateHelper = new ArthMateHelper();
                        var responseDC = await ArthMateHelper.eAadharDigilockerVerifyOTPXml(AadharObj);

                        if (responseDC.result.message != null && responseDC.statusCode.ToString() == "101" && responseDC.result.message == "Aadhaar XML file downloaded successfully")
                        {
                            DataFromAdhaar data = new DataFromAdhaar();
                            data = responseDC.result.dataFromAadhaar;

                            string otherInfo = Newtonsoft.Json.JsonConvert.SerializeObject(data);

                            var document = db.ArthMateDocumentMasters.Select(x => new { x.Id, x.DocumentName, x.DocumentTypeCode }).ToList();

                            var documentId = document.FirstOrDefault(x => x.DocumentName == "aadhar_card").Id;
                            var leaddocument = db.LeadDocument.Where(x => x.LeadMasterId == AadharObj.LeadMasterId && x.DocumentMasterId == documentId && x.IsActive == true && x.IsDeleted == false).FirstOrDefault();
                            if (leaddocument != null)
                            {
                                string gender = "";
                                if (data.gender.ToUpper() == "M")
                                    gender = "Male";
                                else if (data.gender.ToUpper() == "F")
                                    gender = "Female";
                                else
                                    gender = data.gender;

                                leaddocument.LeadMasterId = AadharObj.LeadMasterId;
                                leaddocument.DocumentMasterId = documentId;
                                leaddocument.DateOfBirth = data.dob;
                                leaddocument.IssuedDate = null;
                                leaddocument.OtherInfo = otherInfo;
                                leaddocument.ModifiedDate = DateTime.Now;
                                leaddocument.ModifiedDate = null;
                                leaddocument.ModifiedBy = null;
                                leaddocument.IsVerified = false;
                                db.Entry(leaddocument).State = EntityState.Modified;

                                lead.per_addr_ln1 = (data.address.splitAddress.houseNumber + " " + data.address.splitAddress.street + " " + data.address.splitAddress.landmark + " " + data.address.splitAddress.subdistrict).Trim(); //data.address.combinedAddress;
                                lead.per_addr_ln2 = (data.address.splitAddress.vtcName + " " + data.address.splitAddress.location + " " + data.address.splitAddress.postOffice).Trim();
                                lead.per_pincode = data.address.splitAddress.pincode;
                                lead.per_state = data.address.splitAddress.state;
                                lead.per_city = data.address.splitAddress.district;
                                lead.CountryName = data.address.splitAddress.country;
                                lead.gender = gender;

                                //string[] name = data.name.ToString().Trim().Split(new char[] { ' ' }, 3);
                                //if (name[0] != null)
                                //{
                                //    lead.first_name = name[0];
                                //}
                                //if (name.Length > 1 && name[1] != null)
                                //{
                                //    if (name.Length == 2)
                                //    {
                                //        lead.last_name = name[1];
                                //    }
                                //    else
                                //    {
                                //        lead.middle_name = name[1];
                                //    }
                                //}
                                //if (name.Length > 2 && name[2] != null)
                                //{
                                //    lead.last_name = name[2];
                                //}

                                lead.ModifiedDate = DateTime.Now;
                                var leadid = new SqlParameter("@LeadMasterId", lead.Id);
                                var seqno = db.Database.SqlQuery<LeadSequenceData>("EXEC [Arthmate].[LeadNextSequenceno] @LeadMasterId", leadid).FirstOrDefault();
                                if (seqno.IsApproved == 0)//&& seqno.IsComplete == true)
                                {
                                    lead.SequenceNo = AadharObj.SequenceNo;
                                }

                                db.Entry(lead).State = EntityState.Modified;
                                //await UpdateLeadCurrentActivity(lead.Id, db, lead.SequenceNo);
                                await UpdateLeadCurrentActivity(lead.Id, db, AadharObj.SequenceNo);

                                db.Commit();

                                res.Msg = responseDC.result.message;
                                res.Status = true;

                                LeadActivityProgressesHistory(AadharObj.LeadMasterId, AadharObj.SequenceNo, 0, "", "Aadhar Verified Successfully");
                            }
                        }
                    }
                    else
                    {
                        res.Msg = "Aadhar Is Not Verified";
                        res.Status = false;

                        LeadActivityProgressesHistory(AadharObj.LeadMasterId, AadharObj.SequenceNo, 0, "", "Aadhar is not  Verified");
                    }

                }
                return res;
            }
        }

        //[AllowAnonymous]
        //[HttpPost]
        //[Route("UpdateLeadMaster")]
        //public async Task<CommonResponseDc> UpdateLeadMaster(LeadPostdc lead)  //customer post , Business data , Bankdetail
        //{
        //    CommonResponseDc res = new CommonResponseDc();
        //    LeadResponseDc LeadResponse = new LeadResponseDc();
        //    using (var db = new AuthContext())
        //    {
        //        ArthMateHelper arthmatehelper = new ArthMateHelper();

        //        var leaddata = db.LeadMasters.Where(x => x.Id == lead.Id).FirstOrDefault();
        //        var document = db.ArthMateDocumentMasters.Select(x => new { x.Id, x.DocumentName, x.DocumentTypeCode }).ToList();
        //        if (lead.CompletionStage != null && lead.CompletionStage == "PanCard")
        //        {
        //            leaddata.appl_pan = lead.appl_pan;

        //            db.Entry(leaddata).State = EntityState.Modified;
        //        }
        //        else if (lead.CompletionStage != null && lead.CompletionStage == "AadharCard")
        //        {
        //            leaddata.aadhar_card_num = lead.aadhar_card_num;
        //            db.Entry(leaddata).State = EntityState.Modified;
        //        }
        //        else if (lead.CompletionStage != null && lead.CompletionStage == "PersonalDetail")
        //        {
        //            leaddata.first_name = lead.first_name;
        //            leaddata.last_name = lead.last_name;
        //            leaddata.father_fname = lead.father_fname;
        //            leaddata.father_lname = lead.father_lname;
        //            leaddata.dob = lead.dob;
        //            leaddata.marital_status = lead.marital_status;
        //            leaddata.SkCode = lead.SkCode;
        //            leaddata.age = Convert.ToInt32(lead.age);
        //            leaddata.gender = lead.gender;
        //            leaddata.MobileNo = lead.MobileNo;
        //            leaddata.email_id = lead.email_id;
        //            leaddata.type_of_addr = lead.type_of_addr;
        //            leaddata.resi_addr_ln1 = lead.resi_addr_ln1;
        //            leaddata.resi_addr_ln2 = lead.resi_addr_ln2;
        //            leaddata.pincode = lead.pincode;
        //            leaddata.city = lead.city;
        //            leaddata.state = lead.state;
        //            leaddata.per_addr_ln1 = lead.per_addr_ln1;
        //            leaddata.per_addr_ln2 = lead.per_addr_ln2;
        //            leaddata.per_pincode = lead.per_pincode;
        //            leaddata.per_city = lead.per_city;
        //            leaddata.per_state = lead.per_state;
        //            leaddata.residence_status = lead.residence_status;
        //            leaddata.qualification = lead.qualification;
        //            db.Entry(leaddata).State = EntityState.Modified;
        //        }
        //        else if (lead.CompletionStage != null && lead.CompletionStage == "BusinessDetail")
        //        {
        //            leaddata.business_name = lead.bus_name;
        //            leaddata.co_app_address = lead.bus_add_corr_line1;
        //            leaddata.pincode = lead.pincode;
        //            leaddata.city = lead.city;
        //            leaddata.state = lead.state;
        //            db.Entry(leaddata).State = EntityState.Modified;
        //        }
        //        else if (lead.CompletionStage != null && lead.CompletionStage == "BankDetail")
        //        {
        //            leaddata.borro_bank_name = lead.borro_bank_name;
        //            leaddata.borro_bank_ifsc = lead.borro_bank_ifsc;
        //            leaddata.borro_bank_acc_num = lead.borro_bank_acc_num;

        //            // bank statement
        //            LeadLoanDocument bankdoc = new LeadLoanDocument();
        //            bankdoc.LeadMasterId = lead.Id;
        //            bankdoc.DocumentMasterId = document.FirstOrDefault(x => x.DocumentName == "borro_bank_stmt").Id;
        //            bankdoc.DocumentTypeCode = document.FirstOrDefault(x => x.DocumentName == "borro_bank_stmt").DocumentTypeCode;
        //            bankdoc.DocumentNumber = lead.borro_bank_acc_num;
        //            bankdoc.FrontFileUrl = lead.BankStatement;
        //            bankdoc.CreatedDate = DateTime.Now;
        //            bankdoc.CreatedBy = 0;
        //            bankdoc.ModifiedDate = null;
        //            bankdoc.ModifiedBy = null;
        //            bankdoc.IsVerified = true;
        //            bankdoc.IsActive = true;
        //            bankdoc.IsDeleted = false;
        //            db.LeadDocument.Add(bankdoc);

        //            // GST statement
        //            LeadLoanDocument Gstdoc = new LeadLoanDocument();
        //            Gstdoc.LeadMasterId = lead.Id;
        //            Gstdoc.DocumentMasterId = document.FirstOrDefault(x => x.DocumentName == "Gst").Id;
        //            Gstdoc.DocumentTypeCode = document.FirstOrDefault(x => x.DocumentName == "Gst").DocumentTypeCode;
        //            Gstdoc.DocumentNumber = lead.borro_bank_acc_num;
        //            Gstdoc.FrontFileUrl = lead.BankStatement;
        //            Gstdoc.CreatedDate = DateTime.Now;
        //            Gstdoc.CreatedBy = 0;
        //            Gstdoc.ModifiedDate = null;
        //            Gstdoc.ModifiedBy = null;
        //            Gstdoc.IsVerified = true;
        //            Gstdoc.IsActive = true;
        //            Gstdoc.IsDeleted = false;
        //            db.LeadDocument.Add(Gstdoc);

        //            db.Entry(leaddata).State = EntityState.Modified;
        //            lead.IsLeadGenerate = true;
        //        }

        //        leaddata.SequenceNo = lead.SequenceNo;
        //        leaddata.ModifiedBy = 0;
        //        leaddata.ModifiedDate = DateTime.Now;
        //        db.Entry(leaddata).State = EntityState.Modified;

        //        if (db.Commit() > 0)
        //        {
        //            res.Status = true;
        //            res.Msg = "Data Saved Successfully";
        //        }
        //    }
        //    return res;
        //}


        [AllowAnonymous]
        [HttpPost]
        [Route("AddPersonalDetail")]
        public async Task<CommonResponseDc> AddPersonalDetail(AddPersonalDetailDc lead)  //customer post , Business data , Bankdetail
        {
            CommonResponseDc res = new CommonResponseDc();
            LeadResponseDc LeadResponse = new LeadResponseDc();
            using (var db = new AuthContext())
            {
                ArthMateHelper arthmatehelper = new ArthMateHelper();
                var leaddata = db.LeadMasters.Where(x => x.Id == lead.LeadMasterId && x.IsActive == true && x.IsDeleted == false).FirstOrDefault();
                if (leaddata != null)
                {
                    leaddata.first_name = lead.first_name;
                    leaddata.last_name = lead.last_name;
                    leaddata.father_fname = lead.father_fname;
                    leaddata.father_lname = lead.father_lname;
                    leaddata.dob = lead.dob;
                    leaddata.gender = lead.gender;
                    leaddata.alt_phone = lead.alt_phone;
                    leaddata.email_id = lead.email_id;
                    leaddata.type_of_addr = "Permanent";
                    leaddata.resi_addr_ln1 = lead.resi_addr_ln1;
                    leaddata.resi_addr_ln2 = lead.resi_addr_ln2;
                    leaddata.pincode = lead.pincode;
                    leaddata.city = lead.city;
                    leaddata.state = lead.state;
                    leaddata.per_addr_ln1 = lead.per_addr_ln1;
                    leaddata.per_addr_ln2 = lead.per_addr_ln2;
                    leaddata.per_pincode = lead.per_pincode;
                    leaddata.per_city = lead.per_city;
                    leaddata.per_state = lead.per_state;
                    leaddata.residence_status = "Owned";
                    leaddata.ModifiedBy = 0;
                    leaddata.ModifiedDate = DateTime.Now;
                    var leadid = new SqlParameter("@LeadMasterId", leaddata.Id);
                    var seqno = db.Database.SqlQuery<LeadSequenceData>("EXEC [Arthmate].[LeadNextSequenceno] @LeadMasterId", leadid).FirstOrDefault();
                    if (seqno.IsApproved == 0)// && seqno.IsComplete == true)
                    {
                        leaddata.SequenceNo = lead.SequenceNo;
                    }

                    db.Entry(leaddata).State = EntityState.Modified;

                    //await UpdateLeadCurrentActivity(leaddata.Id, db, leaddata.SequenceNo);
                    await UpdateLeadCurrentActivity(leaddata.Id, db, lead.SequenceNo);
                    if (db.Commit() > 0)
                    {
                        res.Status = true;
                        res.Msg = "Data Saved Successfully";
                        LeadActivityProgressesHistory(lead.LeadMasterId, lead.SequenceNo, 0, "", "Personal Details Data Saved Successfully");
                    }
                }
                return res;
            }
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("AddBusinessDetail")]
        public async Task<CommonResponseDc> AddBusinessDetail(AddBusinessDetail lead)  //customer post , Business data , Bankdetail
        {
            CommonResponseDc res = new CommonResponseDc();
            res.Status = false;
            res.Msg = "";

            LeadResponseDc LeadResponse = new LeadResponseDc();
            if (lead.bus_add_corr_pincode.Length != 6)
            {
                res.Msg = "Pincode should be 6 digit!";
                return res;
            }
            using (var db = new AuthContext())
            {
                var leaddata = db.LeadMasters.Where(x => x.Id == lead.LeadMasterId && x.IsActive == true && x.IsDeleted == false).FirstOrDefault();
                if (leaddata != null)
                {
                    leaddata.business_name = lead.bus_name;
                    leaddata.bus_name = lead.bus_name;
                    leaddata.bus_gstno = lead.bus_gstno;
                    leaddata.doi = lead.doi;
                    leaddata.bus_pan = lead.bus_pan;
                    leaddata.bus_add_corr_line1 = lead.bus_add_corr_line1;
                    leaddata.bus_add_corr_line2 = lead.bus_add_corr_line2;
                    leaddata.bus_add_corr_pincode = lead.bus_add_corr_pincode;
                    leaddata.bus_add_corr_city = lead.bus_add_corr_city;
                    leaddata.bus_add_corr_state = lead.bus_add_corr_state;
                    leaddata.bus_add_per_line1 = lead.bus_add_per_line1;
                    leaddata.bus_add_per_line2 = lead.bus_add_per_line2;

                    leaddata.bus_add_per_pincode = lead.bus_add_per_pincode;

                    leaddata.bus_add_per_city = lead.bus_add_per_city;
                    leaddata.bus_add_per_state = lead.bus_add_per_state;
                    leaddata.bus_entity_type = lead.bus_entity_type;
                    leaddata.business_establishment_proof_type = "Udhyog Adhaar";
                    leaddata.Bus_MonthlySalary = lead.Bus_MonthlySalary;
                    leaddata.ModifiedBy = 0;
                    leaddata.ModifiedDate = DateTime.Now;
                    var leadid = new SqlParameter("@LeadMasterId", leaddata.Id);
                    var seqno = db.Database.SqlQuery<LeadSequenceData>("EXEC [Arthmate].[LeadNextSequenceno] @LeadMasterId", leadid).FirstOrDefault();
                    if (seqno.IsApproved == 0)// && seqno.IsComplete == true)
                    {
                        leaddata.SequenceNo = lead.SequenceNo;
                    }

                    db.Entry(leaddata).State = EntityState.Modified;

                    //await UpdateLeadCurrentActivity(leaddata.Id, db, leaddata.SequenceNo);
                    await UpdateLeadCurrentActivity(leaddata.Id, db, lead.SequenceNo);

                    if (db.Commit() > 0)
                    {
                        res.Status = true;
                        res.Msg = "Data Saved Successfully";

                        LeadActivityProgressesHistory(lead.LeadMasterId, lead.SequenceNo, 0, "", "Business Detail uploaded Successfully");
                    }
                }
                return res;
            }


        }
        [AllowAnonymous]
        [HttpPost]
        [Route("AddBankDetail")]
        public async Task<CommonResponseDc> AddBankDetail(AddBankDetail lead)  //customer post , Business data , Bankdetail
        {
            CommonResponseDc res = new CommonResponseDc();
            LeadResponseDc LeadResponse = new LeadResponseDc();
            using (var db = new AuthContext())
            {
                var leaddata = db.LeadMasters.Where(x => x.Id == lead.LeadMasterId && x.IsActive == true && x.IsDeleted == false).FirstOrDefault();
                if (lead != null)
                {
                    if (string.IsNullOrEmpty(lead.borro_bank_name) || string.IsNullOrEmpty(lead.borro_bank_ifsc)
                        || string.IsNullOrEmpty(lead.borro_bank_acc_num)
                        || string.IsNullOrEmpty(lead.AccType)
                        || string.IsNullOrEmpty(lead.AccountHolderName))
                    {
                        res.Status = false;
                        res.Msg = "Required fields all missing.";
                        return res;
                    }
                    leaddata.borro_bank_name = lead.borro_bank_name;
                    leaddata.borro_bank_ifsc = lead.borro_bank_ifsc;
                    leaddata.borro_bank_acc_num = lead.borro_bank_acc_num;
                    leaddata.fip_id = lead.fip_id;
                    leaddata.EnquiryAmount = lead.EnquiryAmount;
                    leaddata.AccountType = lead.AccType;
                    leaddata.AccountHolderName = lead.AccountHolderName;
                    if (lead.BankStatement.Count > 0)
                    {
                        var document = db.ArthMateDocumentMasters.Where(x => x.DocumentName.Trim().ToLower() == "bank_stmnts").FirstOrDefault();
                        var leadDoc = db.LeadDocument.Where(x => x.LeadMasterId == lead.LeadMasterId && x.DocumentMasterId == document.Id && x.IsActive == true && x.IsDeleted == false).FirstOrDefault();

                        if (leadDoc != null)
                        {
                            leadDoc.IsActive = false;
                            leadDoc.IsDeleted = true;
                            leadDoc.ModifiedDate = DateTime.Now;
                            db.Entry(leadDoc).State = EntityState.Modified;
                            var LeadBankStatement = db.LeadBankStatement.Where(x => x.LeadMasterId == lead.LeadMasterId && x.DocumentMasterId == document.Id && x.IsActive == true && x.IsDeleted == false).ToList();

                            if (LeadBankStatement != null && LeadBankStatement.Count > 0)
                            {
                                foreach (var item in LeadBankStatement)
                                {
                                    item.IsActive = false;
                                    item.IsDeleted = true;
                                    item.ModifiedDate = DateTime.Now;
                                    db.Entry(item).State = EntityState.Modified;
                                }
                            }
                        }

                        List<LeadBankStatement> AddBankStatement = new List<LeadBankStatement>();

                        int PrimaryFile = 0;
                        foreach (var file in lead.BankStatement)
                        {
                            if (PrimaryFile == 0)
                            {
                                LeadLoanDocument bankdoc = new LeadLoanDocument();
                                bankdoc.LeadMasterId = leaddata.Id;
                                bankdoc.DocumentMasterId = document.Id;
                                bankdoc.DocumentNumber = lead.borro_bank_acc_num;
                                bankdoc.FrontFileUrl = file;
                                bankdoc.CreatedDate = DateTime.Now;
                                bankdoc.PdfPassword = lead.PdfPassword;

                                bankdoc.CreatedBy = 0;
                                bankdoc.ModifiedDate = null;
                                bankdoc.ModifiedBy = null;
                                bankdoc.IsVerified = true;
                                bankdoc.IsActive = true;
                                bankdoc.IsDeleted = false;
                                db.LeadDocument.Add(bankdoc);
                            }

                            LeadBankStatement BankStatement = new LeadBankStatement();
                            BankStatement.LeadMasterId = leaddata.Id;
                            BankStatement.DocumentMasterId = (int)document.Id;
                            BankStatement.StatementFile = file;
                            BankStatement.Sequence = PrimaryFile;
                            BankStatement.Remark = "";
                            BankStatement.IsSuccess = false;
                            BankStatement.CreatedDate = DateTime.Now;
                            BankStatement.CreatedBy = 0;
                            BankStatement.ModifiedDate = null;
                            BankStatement.ModifiedBy = null;
                            BankStatement.IsActive = true;
                            BankStatement.IsDeleted = false;
                            AddBankStatement.Add(BankStatement);

                            PrimaryFile++;
                        }
                        if (AddBankStatement != null && AddBankStatement.Any())
                        {
                            db.LeadBankStatement.AddRange(AddBankStatement);
                        }
                    }
                    else
                    {    // GST statement
                        var document = db.ArthMateDocumentMasters.Where(x => x.DocumentName.Trim().ToLower() == "gst").FirstOrDefault();
                        var leadDoc = db.LeadDocument.Where(x => x.LeadMasterId == lead.LeadMasterId && x.DocumentMasterId == document.Id && x.IsActive == true && x.IsDeleted == false).FirstOrDefault();
                        if (leadDoc == null)
                        {
                            LeadLoanDocument Gstdoc = new LeadLoanDocument();
                            Gstdoc.LeadMasterId = leaddata.Id;
                            Gstdoc.DocumentMasterId = document.Id;
                            Gstdoc.DocumentNumber = leaddata.bus_gstno;
                            Gstdoc.FrontFileUrl = lead.GSTStatement;
                            Gstdoc.CreatedDate = DateTime.Now;
                            Gstdoc.CreatedBy = 0;
                            Gstdoc.ModifiedDate = null;
                            Gstdoc.ModifiedBy = null;
                            Gstdoc.IsVerified = true;
                            Gstdoc.IsActive = true;
                            Gstdoc.IsDeleted = false;
                            db.LeadDocument.Add(Gstdoc);
                        }
                        else
                        {
                            leadDoc.DocumentNumber = leaddata.bus_gstno;
                            leadDoc.FrontFileUrl = lead.GSTStatement;
                            leadDoc.NameOnCard = null;
                            leadDoc.DateOfBirth = null;
                            leadDoc.IssuedDate = null;
                            leadDoc.OtherInfo = "gst";
                            leadDoc.ModifiedDate = DateTime.Now;
                            leadDoc.ModifiedBy = (int)lead.LeadMasterId;
                            db.Entry(leadDoc).State = EntityState.Modified;
                        }

                    }

                    leaddata.ModifiedBy = 0;
                    leaddata.ModifiedDate = DateTime.Now;
                    var leadid = new SqlParameter("@LeadMasterId", leaddata.Id);
                    var seqno = db.Database.SqlQuery<LeadSequenceData>("EXEC [Arthmate].[LeadNextSequenceno] @LeadMasterId", leadid).FirstOrDefault();
                    if (seqno.IsApproved == 0)// && seqno.IsComplete == true)
                    {
                        leaddata.SequenceNo = lead.SequenceNo;
                    }

                    db.Entry(leaddata).State = EntityState.Modified;
                    //await UpdateLeadCurrentActivity(leaddata.Id, db, leaddata.SequenceNo);
                    await UpdateLeadCurrentActivity(leaddata.Id, db, lead.SequenceNo);

                    if (db.Commit() > 0)
                    {
                        res.Status = true;
                        res.Msg = "Data Saved Successfully";
                        LeadActivityProgressesHistory(lead.LeadMasterId, lead.SequenceNo, 0, "", "Bank Detail uploaded Successfully");
                    }
                }
                return res;
            }

        }




        [AllowAnonymous]
        [HttpPost]
        [Route("GstVerify")] //Arthmate Api For GstVerification
        public async Task<CommonResponseDc> GstVerifyApi(long leadmasterid, string GstNo)
        {
            CommonResponseDc res = new CommonResponseDc();

            using (AuthContext db = new AuthContext())
            {
                var lead = db.LeadMasters.Where(x => x.Id == leadmasterid && x.IsActive == true && x.IsDeleted == false).FirstOrDefault();
                if (lead != null)
                {
                    GstVerifyRequest gstVerifyRequest = new GstVerifyRequest()
                    {
                        gstin = GstNo,
                        consent = "Y",
                        consent_timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                        loan_app_id = lead.loan_app_id
                    };

                    ArthMateHelper ArthMateHelper = new ArthMateHelper();
                    var gst = await ArthMateHelper.GstVerification(gstVerifyRequest, leadmasterid);
                    res.Data = gst;
                }
            }

            return res;
        }

        #region Lead Generate
        [HttpGet]
        [Route("PostLead")]
        [AllowAnonymous]
        public async Task<CommonResponseDc> PostLead(long LeadMasterId)
        {
            CommonResponseDc res = new CommonResponseDc();
            using (var db = new AuthContext())
            {
                try
                {

                    TextFileLogHelper.TraceLog("PostLead : start");
                    var lead = db.LeadMasters.Where(x => x.Id == LeadMasterId).FirstOrDefault();

                    if (lead != null)
                    {
                        string address__ln1 = "";
                        string address__ln2 = "";
                        if (lead.per_addr_ln1.Length >= 10)
                        {
                            address__ln1 = lead.per_addr_ln1;
                            address__ln2 = lead.per_addr_ln2;
                        }
                        else
                        {
                            address__ln1 = (string.IsNullOrEmpty(lead.per_addr_ln1) ? "" : lead.per_addr_ln1) + (string.IsNullOrEmpty(lead.per_addr_ln2) ? "" : lead.per_addr_ln2) + (string.IsNullOrEmpty(lead.per_city) ? "" : lead.per_city) + (string.IsNullOrEmpty(lead.per_state) ? "" : lead.per_state);
                            address__ln2 = ".";
                        }

                        LeadPostdc leadObj = new LeadPostdc
                        {
                            partner_loan_app_id = lead.partner_loan_app_id,
                            partner_borrower_id = lead.partner_borrower_id,
                            //01-03-2024
                            first_name = lead.first_name + (String.IsNullOrEmpty(lead.middle_name) ? "" : " " + lead.middle_name), //first_name = lead.first_name,
                            middle_name = "",//string.IsNullOrEmpty(lead.middle_name) ? "" : lead.middle_name,
                            last_name = lead.last_name,
                            type_of_addr = "Current",
                            resi_addr_ln1 = lead.resi_addr_ln1.Length >= 40 ? lead.resi_addr_ln1.Substring(0, 39) : lead.resi_addr_ln1,
                            resi_addr_ln2 = lead.resi_addr_ln2,
                            city = lead.city,
                            state = lead.state,
                            pincode = lead.pincode,
                            //per_addr_ln1 = lead.per_addr_ln1,
                            //per_addr_ln2 = lead.per_addr_ln2,
                            per_addr_ln1 = address__ln1,
                            per_addr_ln2 = address__ln2,
                            per_city = lead.per_city,
                            per_state = lead.per_state,
                            per_pincode = lead.per_pincode,
                            appl_phone = lead.appl_phone,
                            appl_pan = lead.appl_pan,
                            email_id = lead.email_id,
                            aadhar_card_num = lead.aadhar_card_num,
                            dob = lead.dob,
                            age = lead.age.ToString(),
                            gender = lead.gender,
                            borro_bank_name = lead.borro_bank_name,
                            borro_bank_acc_num = lead.borro_bank_acc_num,
                            borro_bank_ifsc = lead.borro_bank_ifsc,
                            qualification = lead.qualification,
                            marital_status = lead.marital_status,
                            bus_pan = string.IsNullOrEmpty(lead.bus_pan) ? "" : lead.bus_pan,
                            bus_add_corr_line1 = lead.bus_add_corr_line1,
                            bus_add_corr_line2 = lead.bus_add_corr_line2,
                            bus_add_corr_city = lead.bus_add_corr_city,
                            bus_add_corr_state = lead.bus_add_corr_state,
                            bus_add_corr_pincode = lead.bus_add_corr_pincode,
                            bus_add_per_line1 = string.IsNullOrEmpty(lead.bus_add_per_line1) ? lead.bus_add_corr_line1 : lead.bus_add_per_line1,
                            bus_add_per_line2 = string.IsNullOrEmpty(lead.bus_add_per_line2) ? lead.bus_add_corr_line2 : lead.bus_add_per_line2,
                            bus_add_per_city = string.IsNullOrEmpty(lead.bus_add_per_city) ? lead.bus_add_corr_city : lead.bus_add_per_city,
                            bus_add_per_state = string.IsNullOrEmpty(lead.bus_add_per_state) ? lead.bus_add_corr_state : lead.bus_add_per_state,
                            bus_add_per_pincode = string.IsNullOrEmpty(lead.bus_add_per_pincode) ? lead.bus_add_corr_pincode : lead.bus_add_per_pincode,
                            residence_status = lead.residence_status,
                            bureau_pull_consent = "Yes",
                            father_fname = lead.father_fname,
                            father_lname = lead.father_lname,
                            bus_name = lead.bus_name,
                            doi = lead.doi,
                            bus_entity_type = lead.bus_entity_type,
                            Id = lead.Id,

                        };

                        TextFileLogHelper.TraceLog("PostLead : phase 1");
                        ArthMateHelper ArthMateHelper = new ArthMateHelper();

                        TextFileLogHelper.TraceLog("PostLead : ArthMateHelper");
                        var data = await ArthMateHelper.LeadApi(leadObj);

                        if (data != null)
                        {
                            if (data.success)
                            {
                                lead.loan_app_id = data.data.preparedbiTmpl.FirstOrDefault().loan_app_id;
                                lead.borrower_id = data.data.preparedbiTmpl.FirstOrDefault().borrower_id;
                                lead.AppliedDate = DateTime.Now;

                                db.Entry(lead).State = EntityState.Modified;
                                db.Commit();

                                LeadActivityProgressesHistory(LeadMasterId, 0, 0, "Generate Lead", "1. Lead  Generated (loan_app_id:" + lead.loan_app_id + ", borrower_id:" + lead.borrower_id + ") Successfully");

                                var dataaaa = await LoanDocumentApi(lead.Id);
                                LeadActivityProgressesHistory(LeadMasterId, 0, 0, "All Document Post", "2. Document Uploaded Successfully");

                                var result = await ArthmateApiFlow(LeadMasterId);


                                //var pen = await PennyDrop(LeadMasterId);

                                //var leadid = new SqlParameter("@LeadMasterId", lead.Id);
                                //var Created = new SqlParameter("@CreateBy", 1717);
                                //var ProgressData = db.Database.SqlQuery<int>("EXEC [Arthmate].[InsertLeadActivityProgresses] @LeadMasterId,@CreateBy", leadid, Created).FirstOrDefault();
                                res.Msg = data.message;
                                res.Status = data.success;
                            }
                            else
                            {
                                res.Msg = data.message;
                                res.Status = data.success;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    //throw ex;
                    TextFileLogHelper.TraceLog("PostLead : Catch in Post Lead Controller" + ex.Message);
                    string eerroorrmsg = ex.Message;
                    res.Msg = eerroorrmsg;
                    return res;

                }
                return res;
            }
        }


        [HttpGet]
        [Route("TestPostLead")]
        [AllowAnonymous]
        public async Task<string> TestPostLead(string jsonString)
        {
            LeadResponseDc leadResponseDc = new LeadResponseDc();
            leadResponseDc = JsonConvert.DeserializeObject<LeadResponseDc>(jsonString);

            return "";
        }


        #endregion  Api for mobile app end
        [AllowAnonymous]
        [HttpGet]
        [Route("RequestAScoreAPI")]
        public async Task<AScoreAPIResponse> RequestAScoreAPIAsync(long LeadMasterId)
        {
            // var docSendStatus = await LoanDocumentApi(LeadMasterId);
            return await RequestAScoreAPI(LeadMasterId);
        }

        private async Task<AScoreAPIResponse> RequestAScoreAPI(long LeadMasterId)
        {
            AScoreAPIResponse res = new AScoreAPIResponse();
            var identity = User.Identity as ClaimsIdentity;
            int userid = 0;
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
            using (AuthContext db = new AuthContext())
            {
                if (LeadMasterId != 0)
                {
                    var lead = db.LeadMasters.Where(x => x.Id == LeadMasterId && x.IsActive == true && x.IsDeleted == false).FirstOrDefault();
                    string dd = lead.bus_add_corr_state.ToLower();
                    var statecode = db.Database.SqlQuery<string>($"select case when len(StateCode)=1 then RTRIM(LTRIM('0'+cast(StateCode as char))) else RTRIM(LTRIM(cast(StateCode as char))) end as statecode from arthmatestatecodes where lower(state) = '{lead.bus_add_corr_state.ToLower()}'").FirstOrDefault();

                    if (lead != null)
                    {
                        string address = (string.IsNullOrEmpty(lead.per_addr_ln1) ? "" : lead.per_addr_ln1) + (string.IsNullOrEmpty(lead.per_addr_ln2) ? "" : lead.per_addr_ln2) + (string.IsNullOrEmpty(lead.per_city) ? "" : lead.per_city) + (string.IsNullOrEmpty(lead.per_state) ? "" : lead.per_state);

                        ArthMateHelper ArthMateHelper = new ArthMateHelper();
                        #region For Production payload
                        AScoreAPIRequest scorereq = new AScoreAPIRequest();
                        scorereq.first_name = lead.first_name;
                        scorereq.last_name = lead.last_name;
                        scorereq.dob = lead.dob;
                        scorereq.pan = lead.appl_pan;
                        scorereq.gender = lead.gender;
                        scorereq.mobile_number = lead.MobileNo;
                        //scorereq.address = lead.per_addr_ln1.Length > 40 ? lead.per_addr_ln1.Substring(0, 39) : lead.per_addr_ln1;
                        scorereq.address = address.Length > 40 ? address.Substring(0, 39) : address;
                        scorereq.city = lead.bus_add_corr_city;
                        scorereq.state_code = $"{statecode}";
                        scorereq.pin_code = lead.bus_add_corr_pincode;
                        scorereq.enquiry_purpose = "61";
                        scorereq.enquiry_stage = "PRE-SCREEN";
                        scorereq.enquiry_amount = Convert.ToString(lead.EnquiryAmount > 0 ? lead.EnquiryAmount : 100000);
                        scorereq.en_acc_account_number_1 = lead.loan_app_id;
                        scorereq.bureau_type = "cibil";
                        scorereq.tenure = lead.tenure;
                        scorereq.loan_app_id = lead.loan_app_id;
                        scorereq.consent = "Y";
                        scorereq.product_type = "UBLN";
                        scorereq.consent_timestamp = indianTime.ToString("yyyy-MM-dd HH:mm:ss");
                        #endregion

                        //#region for testing payload


                        res = await ArthMateHelper.AscoreApi(scorereq, userid, LeadMasterId);
                        if (res.request_id != null)
                        {
                            AScore obj = new AScore();
                            obj.LeadMasterId = LeadMasterId;
                            obj.request_id = res.request_id;
                            obj.CreatedDate = DateTime.Now;
                            obj.IsActive = true;
                            obj.IsDeleted = false;
                            obj.CreatedBy = userid;
                            db.AScore.Add(obj);
                            db.Commit();

                            LeadActivityProgressesHistory(LeadMasterId, 0, 0, "A-Score Generated", "3. A-Score Generated Successfully");
                        }
                    }
                    else
                    {
                        res.message = "Lead Not Found";
                    }
                }
            }
            return res;
        }

        //CeplrPdfReports
        [HttpGet]
        [Route("CeplrPdfReports")]
        public async Task<CommonResponseDc> CeplrPdfReports(long LeadMasterId)
        {
            CommonResponseDc res = new CommonResponseDc();
            ArthMateHelper ArthMateHelper = new ArthMateHelper();
            CeplrPdfReportDc pdfReportDc = new CeplrPdfReportDc();

            string LogoUrl = "";
            //string PDFfile_password = "";
            using (AuthContext db = new AuthContext())
            {
                var LeadId = new SqlParameter("@leadmasterid", LeadMasterId);
                CeplrPdf_SpData leaddata = db.Database.SqlQuery<CeplrPdf_SpData>("EXEC [Arthmate].[CeplrPdfReportData] @leadmasterid", LeadId).FirstOrDefault();

                if (leaddata != null)
                {
                    pdfReportDc.file = leaddata.FrontFileUrl;
                    pdfReportDc.callback_url = CeplrPdfReportCallbackUrl;
                    pdfReportDc.file_password = leaddata.PdfPassword;
                    pdfReportDc.email = leaddata.email_id;
                    pdfReportDc.ifsc_code = leaddata.borro_bank_ifsc;
                    pdfReportDc.mobile = leaddata.MobileNo;
                    pdfReportDc.name = leaddata.first_name + " " + leaddata.last_name;

                    var Bankdata = db.CeplrBankList.Where(x => x.fip_name == leaddata.borro_bank_name).FirstOrDefault();
                    if (Bankdata != null)
                    {
                        pdfReportDc.fip_id = string.IsNullOrEmpty(Bankdata.pdf_fip_id.ToString()) ? Bankdata.pdf_fip_id.ToString() : null;
                    }
                    PdfResDcCeplr ress = await ArthMateHelper.CeplrPdfReports(pdfReportDc, LeadMasterId);

                    if (ress.code == 202)
                    {
                        TextFileLogHelper.TraceLog("Code =success" + pdfReportDc.file);
                        CeplrPdfReports pdfinsert = new CeplrPdfReports();
                        pdfinsert.LeadMasterId = LeadMasterId;
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
                        res.Msg = "Pdf Report Fetched Successfully";
                        res.Status = true;

                        LeadActivityProgressesHistory(LeadMasterId, 0, 0, "CeplrPdfRepor Generated", "CeplrPdfReport Generated Successfully");
                    }
                    else
                    {
                        res.Msg = ress.message;
                        res.Status = false;

                        LeadActivityProgressesHistory(LeadMasterId, 0, 0, "CeplrPdfRepor Not Generated", "CeplrPdfReport Not Generated");
                    }

                }
            }
            return res;
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("CoLenderSelectorAPI")]
        // Note- running back groud
        public async Task<CoLenderResponseDc> CoLenderSelectorAPI(long LeadMasterId)
        {

            return await CoLenderSelector(LeadMasterId);
        }

        public async Task<CoLenderResponseDc> CoLenderSelector(long LeadMasterId)
        {
            CoLenderResponseDc res = new CoLenderResponseDc();
            var identity = User.Identity as ClaimsIdentity;
            int userid = 0;
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
            using (AuthContext db = new AuthContext())
            {
                ArthMateHelper arthMateHelper = new ArthMateHelper();

                var lead = db.LeadMasters.Where(x => x.Id == LeadMasterId && x.IsActive == true && x.IsDeleted == false).FirstOrDefault();
                var loanconfig = db.LoanConfiguration.FirstOrDefault();
                if (lead != null)
                {
                    var AScoreRequestid = db.AScore.Where(x => x.LeadMasterId == LeadMasterId && x.IsActive == true && x.IsDeleted == false).OrderByDescending(x => x.Id).FirstOrDefault();

                    var ceplr_cust = db.CeplrPdfReports.Where(x => x.LeadMasterId == LeadMasterId && x.IsActive == true && x.IsDeleted == false).OrderByDescending(x => x.Id).FirstOrDefault();

                    double MonthlySalary = await arthMateHelper.GetMonthlySalary(LeadMasterId, db);

                    CoLenderRequest colenderObj = new CoLenderRequest();

                    colenderObj.first_name = lead.first_name;
                    colenderObj.last_name = lead.last_name;
                    colenderObj.dob = lead.dob;
                    colenderObj.appl_pan = lead.appl_pan;
                    colenderObj.gender = lead.gender;
                    colenderObj.appl_phone = lead.appl_phone;
                    colenderObj.address = lead.per_addr_ln1;
                    colenderObj.city = lead.city;
                    colenderObj.state = lead.state;
                    colenderObj.pincode = lead.pincode;
                    colenderObj.enquiry_purpose = "61";
                    colenderObj.bureau_type = "cibil";
                    colenderObj.tenure = lead.tenure;
                    colenderObj.request_id_a_score = AScoreRequestid.request_id;
                    colenderObj.request_id_b_score = "";
                    colenderObj.ceplr_cust_id = ceplr_cust.customer_id;
                    colenderObj.interest_rate = ""; // Note- get from loan master
                    colenderObj.product_type_code = "UBLN";
                    colenderObj.sanction_amount = lead.EnquiryAmount;
                    colenderObj.dscr = 0;  // Note- need to discuss
                    colenderObj.monthly_income = MonthlySalary; // Note- get from ceplr
                    colenderObj.loan_app_id = lead.loan_app_id;
                    colenderObj.consent = "Y";
                    colenderObj.consent_timestamp = indianTime.ToString("yyyy-MM-dd HH:mm:ss");


                    res = await arthMateHelper.CoLenderApi(colenderObj, userid, lead.Id);

                    var colender = db.CoLenderResponse.Where(x => x.LeadMasterId == LeadMasterId && x.IsActive == true && x.IsDeleted == false).FirstOrDefault();
                    if (colender == null)
                    {
                        CoLenderResponse obj = new CoLenderResponse();
                        obj.LeadMasterId = lead.Id;
                        obj.request_id = res.request_id;
                        obj.loan_amount = Convert.ToDouble(res.loan_amount);
                        obj.pricing = loanconfig.InterestRate;//Convert.ToDouble(res.pricing);
                        obj.co_lender_shortcode = res.co_lender_shortcode;
                        obj.loan_app_id = res.loan_app_id;
                        obj.co_lender_assignment_id = res.co_lender_assignment_id;
                        obj.co_lender_full_name = res.co_lender_full_name;
                        obj.status = res.status;
                        obj.AScoreRequest_id = AScoreRequestid.request_id;
                        obj.ceplr_cust_id = ceplr_cust.customer_id;
                        //obj.SanctionAmount = Convert.ToDouble(res.loan_amount);
                        obj.SanctionAmount = lead.EnquiryAmount;
                        obj.CreatedDate = DateTime.Now;
                        obj.ModifiedDate = null;
                        obj.IsActive = true;
                        obj.IsDeleted = false;
                        obj.CreatedBy = userid;
                        obj.ModifiedBy = null;
                        //obj.ProgramType = string.IsNullOrEmpty(res.program_type) ? "" : res.program_type;
                        obj.ProgramType = (!string.IsNullOrEmpty(res.program_type) && res.program_type == "Transaction_POS") ? "Transactions" : res.program_type;
                        db.CoLenderResponse.Add(obj);

                        if ((Convert.ToDouble(res.loan_amount) > 0 && (Convert.ToDouble(24) > 0)))
                        {

                            var ActivitySequence = db.ArthMateActivitySequence.FirstOrDefault(x => x.ScreenName.Trim() == "InProgress" && x.IsActive == true);
                            await UpdateLeadCurrentActivity(LeadMasterId, db, ActivitySequence.SequenceNo);

                            lead.SequenceNo = Convert.ToInt32(ActivitySequence.SequenceNo);

                            LeadActivityProgressesHistory(LeadMasterId, 0, 0, "Colender/Offer Generated ", "Colender Generated Successfully");
                        }
                        else
                        {
                            LeadActivityProgressesHistory(LeadMasterId, 0, 0, "Colender/Offer NOT Generated ", "Colender NOT Generated Due to LoanAmount/pricing is come zero ");
                        }
                        db.Entry(lead).State = EntityState.Modified;
                        db.Commit();


                    }
                    else
                    {
                        colender.IsActive = false;
                        colender.IsDeleted = true;
                        colender.ModifiedDate = DateTime.Now;
                        db.Entry(colender).State = EntityState.Modified;
                        db.Commit();
                        CoLenderResponse obj = new CoLenderResponse();
                        obj.LeadMasterId = lead.Id;
                        obj.request_id = res.request_id;
                        obj.loan_amount = Convert.ToDouble(res.loan_amount);
                        obj.pricing = loanconfig.InterestRate;//Convert.ToDouble(res.pricing);
                        obj.co_lender_shortcode = res.co_lender_shortcode;
                        obj.loan_app_id = res.loan_app_id;
                        obj.co_lender_assignment_id = res.co_lender_assignment_id;
                        obj.co_lender_full_name = res.co_lender_full_name;
                        obj.status = res.status;
                        obj.AScoreRequest_id = AScoreRequestid.request_id;
                        obj.ceplr_cust_id = ceplr_cust.customer_id;
                        //obj.SanctionAmount = Convert.ToDouble(res.loan_amount);
                        obj.SanctionAmount = lead.EnquiryAmount;
                        obj.CreatedDate = DateTime.Now;
                        obj.ModifiedDate = null;
                        obj.IsActive = true;
                        obj.IsDeleted = false;
                        obj.CreatedBy = userid;
                        obj.ModifiedBy = null;
                        //obj.ProgramType = res.program_type;
                        obj.ProgramType = (!string.IsNullOrEmpty(res.program_type) && res.program_type == "Transaction_POS") ? "Transactions" : res.program_type;

                        db.CoLenderResponse.Add(obj);

                        if ((Convert.ToDouble(res.loan_amount) > 0 && (Convert.ToDouble(24) > 0)))
                        {
                            var ActivitySequence = db.ArthMateActivitySequence.FirstOrDefault(x => x.ScreenName.Trim() == "InProgress" && x.IsActive == true);
                            await UpdateLeadCurrentActivity(LeadMasterId, db, ActivitySequence.SequenceNo);
                            LeadActivityProgressesHistory(LeadMasterId, 0, 0, "Colender Generated (Offer Generated)- ", "Colender Generated Successfully");
                            lead.SequenceNo = Convert.ToInt32(ActivitySequence.SequenceNo);
                        }
                        else
                        {
                            LeadActivityProgressesHistory(LeadMasterId, 0, 0, "Colender/Offer NOT Generated ", "Colender NOT Generated Due to LoanAmount/pricing is come zero ");
                        }
                        db.Entry(lead).State = EntityState.Modified;
                        db.Commit();

                    }

                }
                else
                {
                    res.message = "Lead Not Found";

                }

            }
            return res;
        }

        #endregion
        [AllowAnonymous]
        [HttpGet]
        [Route("GetActivityResponse")]
        public async Task<CommonResponseDc> GetActivityResponse(long LeadMasterId)
        {
            CommonResponseDc result = new CommonResponseDc();
            var identity = User.Identity as ClaimsIdentity;
            int userid = 0;
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

            if (LeadMasterId > 0)
            {
                using (var db = new AuthContext())
                {
                    var leadid = new SqlParameter("@LeadMasterId", LeadMasterId);
                    var response = db.Database.SqlQuery<ActivityResponseDc>("Exec GetActivityResponse @LeadMasterId", leadid).FirstOrDefault();
                    if (response != null)
                    {
                        result.Data = response;
                        result.Status = true;
                    }
                    else
                    {
                        result.Data = null;
                        result.Status = false;
                    }
                }
            }

            return result;
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("GetAscore")]
        public async Task<CommonResponseDc> GetAscoreAPI(long LeadMasterId)
        {
            CommonResponseDc result = new CommonResponseDc();
            var identity = User.Identity as ClaimsIdentity;
            int userid = 0;
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

            if (LeadMasterId > 0)
            {
                using (var db = new AuthContext())
                {
                    var request_id = db.AScore.FirstOrDefault(x => x.LeadMasterId == LeadMasterId && x.IsActive == true && x.IsDeleted == false).request_id;
                    if (request_id != null)
                    {
                        ArthMateHelper ArthMateHelper = new ArthMateHelper();

                        AScorePostDc ReqstJson = new AScorePostDc();
                        ReqstJson.LeadMasterId = LeadMasterId;
                        ReqstJson.request_id = request_id;

                        var res = await ArthMateHelper.GetAscoreApi(ReqstJson);
                        if (res.success && res.data != null && res.data.score != null)
                        {
                            AScore obj = new AScore();
                            obj.LeadMasterId = ReqstJson.LeadMasterId;
                            obj.product = "UBLN";
                            obj.request_id = request_id;
                            obj.CINSRate = res.data.score.CINSRate;
                            obj.DPD24C2 = res.data.score.DPD24C2;
                            obj.EQ3C1 = res.data.score.EQ3C1;
                            obj.HL12 = res.data.score.HL12;
                            obj.HL24C3 = res.data.score.HL24C3;
                            obj.HL36CC = res.data.score.HL36CC;
                            obj.LLPL = res.data.score.LLPL;
                            obj.NCVL_AScore = res.data.score.NCVL_AScore;
                            obj.PSMA24 = res.data.score.PSMA24;
                            obj.RSKPremium = res.data.score.RSKPremium;
                            obj.VYC3 = res.data.score.VYC3;
                            obj.CreatedDate = DateTime.Now;
                            obj.CreatedBy = userid;
                            obj.IsActive = true;
                            obj.IsDeleted = false;
                            db.AScore.Add(obj);
                            db.Commit();
                        }
                        result.Status = res.success;
                        result.Msg = res.message;
                    }
                }
            }

            return result;
        }

        //[AllowAnonymous]
        //[HttpGet]
        //[Route("ArthmatePanVefify")] //discontinue from 1-4-2024
        //public async Task<CommonResponseDc> PanVefify(long LeadMasterId)
        //{
        //    CommonResponseDc res = new CommonResponseDc();

        //    if (LeadMasterId > 0)
        //    {
        //        using (var db = new AuthContext())
        //        {
        //            var data = db.LeadMasters.Where(x => x.Id == LeadMasterId && x.IsActive == true && x.IsDeleted == false).FirstOrDefault();

        //            if (data != null)
        //            {
        //                PanVerificationRequestJson obj = new PanVerificationRequestJson();
        //                obj.pan = data.appl_pan;
        //                obj.loan_app_id = data.loan_app_id;
        //                obj.consent = "Y";
        //                obj.consent_timestamp = indianTime.ToString("yyyy-MM-dd HH:mm:ss");

        //                ArthMateHelper ArthMateHelper = new ArthMateHelper();
        //                var resDC = await ArthMateHelper.PanVerificationAsync(obj, data.Id);

        //                if (resDC != null && resDC.data != null)
        //                {
        //                    var documentId = db.ArthMateDocumentMasters.FirstOrDefault(x => x.DocumentName == "pan_card").Id;
        //                    //string resjson = Newtonsoft.Json.JsonConvert.SerializeObject(resDC);
        //                    KYCValidationResponse kyc = new KYCValidationResponse();
        //                    kyc.LeadMasterId = LeadMasterId;
        //                    kyc.DocumentMasterId = Convert.ToInt32(documentId);
        //                    kyc.kyc_id = resDC.kyc_id;
        //                    kyc.Status = resDC.success == true ? "success" : "failed";
        //                    kyc.ResponseJson = resDC.KYCResponse;
        //                    kyc.Message = resDC.data.msg;
        //                    kyc.CreatedDate = DateTime.Now;
        //                    kyc.ModifiedDate = null;
        //                    kyc.IsActive = true;
        //                    kyc.IsDeleted = false;
        //                    db.KYCValidationResponse.Add(kyc);
        //                    db.Commit();

        //                    res.Msg = resDC.data.msg;
        //                    res.Status = true;
        //                }
        //                else
        //                {
        //                    res.Msg = resDC != null && resDC.message != null ? resDC.message : "somthing went wrong!";
        //                }
        //            }
        //            else
        //            {
        //                res.Msg = "Data Not Found in Lead!";
        //            }
        //        }
        //    }
        //    return res;
        //}

        #region NSDL pan Verification V3 New On 20-03-2024

        [AllowAnonymous]
        [HttpGet]
        [Route("ArthmatePanVerificationV3")] //pan Verification V3 20-03-2024
        public async Task<CommonResponseDc> ArthmatePanVerificationV3(long LeadMasterId)
        {
            CommonResponseDc res = new CommonResponseDc();

            if (LeadMasterId > 0)
            {
                using (var db = new AuthContext())
                {
                    var data = db.LeadMasters.Where(x => x.Id == LeadMasterId && x.IsActive == true && x.IsDeleted == false).FirstOrDefault();
                    if (data != null)
                    {
                        PanVerificationRequestV3 obj = new PanVerificationRequestV3();
                        obj.pan = data.appl_pan;
                        obj.name = (string.IsNullOrEmpty(data.first_name) ? "" : data.first_name) + " " + (string.IsNullOrEmpty(data.middle_name) ? "" : data.middle_name) + " " + (string.IsNullOrEmpty(data.last_name) ? "" : data.last_name);
                        obj.father_name = (string.IsNullOrEmpty(data.father_fname) ? "" : data.father_fname) + " " + (string.IsNullOrEmpty(data.father_mname) ? "" : data.father_mname) + " " + (string.IsNullOrEmpty(data.father_lname) ? "" : data.father_lname);
                        obj.dob = data.dob;
                        obj.loan_app_id = data.loan_app_id;
                        obj.consent = "Y";
                        obj.consent_timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                        //PanVerificationRequestV3 obj = new PanVerificationRequestV3();
                        //obj.pan = "AAAPW9785A";
                        //obj.name = "VINITA BHANUSHALI";
                        //obj.father_name = " ";
                        //obj.dob = "1928-02-09";
                        //obj.loan_app_id = data.loan_app_id;
                        //obj.consent = "Y";
                        //obj.consent_timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");


                        ArthMateHelper ArthMateHelper = new ArthMateHelper();
                        var resDC = await ArthMateHelper.PanVerificationV3Async(obj, data.Id);

                        if (resDC != null && resDC.data != null)
                        {
                            var documentId = db.ArthMateDocumentMasters.FirstOrDefault(x => x.DocumentName == "pan_card").Id;
                            //string resjson = Newtonsoft.Json.JsonConvert.SerializeObject(resDC);
                            KYCValidationResponse kyc = new KYCValidationResponse();
                            kyc.LeadMasterId = LeadMasterId;
                            kyc.DocumentMasterId = Convert.ToInt32(documentId);
                            kyc.kyc_id = resDC.kyc_id;
                            kyc.Status = resDC.success == true ? "success" : "failed";
                            kyc.ResponseJson = resDC.KYCResponse;
                            kyc.Message = resDC.data.msg;
                            kyc.CreatedDate = DateTime.Now;
                            kyc.ModifiedDate = null;
                            kyc.IsActive = true;
                            kyc.IsDeleted = false;
                            db.KYCValidationResponse.Add(kyc);
                            db.Commit();

                            res.Data = resDC.data;
                            res.Status = true;
                        }
                        else
                        {
                            res.Msg = resDC != null && resDC.message != null ? resDC.message : "somthing went wrong!";
                        }
                    }
                    else
                    {
                        res.Msg = "Data Not Found in Lead!";
                    }
                }
            }
            return res;
        }

        #endregion

        [AllowAnonymous]
        [HttpPost]
        [Route("GenerateOtpToAcceptOffer")]
        public async Task<CommonResponseDc> GenerateOtpToAcceptOffer(long LeadMasterId) // OfferAcceptGenerateOtp with Aadhaar to aacept aadhar
        {

            CommonResponseDc res = new CommonResponseDc();

            if (LeadMasterId > 0)
            {
                using (var db = new AuthContext())
                {
                    var data = db.LeadMasters.Where(x => x.Id == LeadMasterId).FirstOrDefault();

                    if (data != null)
                    {
                        FirstAadharXMLPost obj = new FirstAadharXMLPost();
                        obj.aadhaar_no = data.aadhar_card_num;
                        obj.loan_app_id = data.loan_app_id;
                        obj.LeadId = LeadMasterId;

                        ArthMateHelper ArthMateHelper = new ArthMateHelper();
                        var resDC = await ArthMateHelper.GenerateOtpToAcceptOffer(obj);
                        if (resDC != null && resDC.data != null && resDC.data.requestId != null)
                        {
                            res.Data = resDC.data.requestId;
                            res.Msg = resDC.data.result.message;
                            res.Status = resDC.success;

                            LeadActivityProgressesHistory(LeadMasterId, 0, 0, "GenerateOtpToAcceptOffer Generated", "Generate Otp ToAcceptOffer Generated Successfully");
                        }
                        else
                        {
                            res.Msg = resDC != null && resDC.message != null ? resDC.message : "somthing went wrong";
                            res.Status = false;
                            LeadActivityProgressesHistory(LeadMasterId, 0, 0, "GenerateOtpToAcceptOffer Not Generated", "Generate Otp ToAcceptOffer Not Generated");
                        }
                    }
                    else
                    {
                        res.Msg = "Data Not Found in Lead!";
                        res.Status = false;
                    }
                }
            }
            return res;
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("AcceptOfferWithXMLAadharOTP")]
        public async Task<CommonResponseDc> AcceptOfferWithXMLAadharOTP(SecondAadharXMLDc AadharObj)
        {
            CommonResponseDc res = new CommonResponseDc();
            if (AadharObj != null)
            {
                using (var db = new AuthContext())
                {
                    var data = db.LeadMasters.Where(x => x.Id == AadharObj.LeadMasterId && x.IsActive == true && x.IsDeleted == false).FirstOrDefault();
                    var configdata = db.LoanConfiguration.FirstOrDefault();
                    if (data != null)
                    {
                        SecondAadharXMLPost obj = new SecondAadharXMLPost();
                        obj.aadhaar_no = data.aadhar_card_num;
                        obj.loan_app_id = data.loan_app_id;
                        obj.request_id = AadharObj.request_id;
                        obj.otp = AadharObj.otp;
                        obj.LeadId = AadharObj.LeadMasterId;

                        ArthMateHelper ArthMateHelper = new ArthMateHelper();
                        var resDC = await ArthMateHelper.StepTwoAadharApi(obj);

                        if (resDC != null && resDC.data != null && resDC.data.result.message == "Aadhaar XML file downloaded successfully")
                        {
                            var documentId = db.ArthMateDocumentMasters.FirstOrDefault(x => x.DocumentName == "aadhar_card").Id;
                            //string resjson = Newtonsoft.Json.JsonConvert.SerializeObject(resDC);

                            KYCValidationResponse kyc = new KYCValidationResponse();
                            kyc.LeadMasterId = AadharObj.LeadMasterId;
                            kyc.DocumentMasterId = Convert.ToInt32(documentId);
                            kyc.kyc_id = resDC.kyc_id;
                            kyc.Status = resDC.success == true ? "success" : "failed";
                            kyc.ResponseJson = resDC.KYCResponse;
                            kyc.Message = resDC.data.result.message;
                            kyc.CreatedDate = DateTime.Now;
                            kyc.ModifiedDate = null;
                            kyc.IsDeleted = false;
                            kyc.IsActive = true;
                            db.KYCValidationResponse.Add(kyc);

                            db.Commit();
                            data.SequenceNo = AadharObj.SequenceNo;
                            data.ModifiedDate = DateTime.Now;
                            db.Entry(data).State = EntityState.Modified;

                            try
                            {
                                await PanAndAadharXmlUpload(AadharObj.LeadMasterId, db);
                                LeadActivityProgressesHistory(data.Id, data.SequenceNo, 0, "", " Pan And Aadhar Xml Upload successfully");

                                var colender = db.CoLenderResponse.Where(x => x.LeadMasterId == AadharObj.LeadMasterId && x.IsActive == true && x.IsDeleted == false && x.status == "success").OrderByDescending(x => x.Id).FirstOrDefault();
                                colender.SanctionAmount = AadharObj.loan_amt;

                                // Generate Laon
                                CommonResponseDc LoanresData = await CallLoanApi(AadharObj.LeadMasterId, AadharObj.insurance_applied, colender);
                                if (LoanresData.Status == true)
                                {
                                    await UpdateLeadCurrentActivity(data.Id, db, data.SequenceNo);
                                    db.Entry(colender).State = EntityState.Modified;
                                    db.Commit();
                                    string priceing = Convert.ToString(configdata.InterestRate);// 24 colender.pricing.ToString();
                                    string loan_amount = colender.loan_amount.ToString();
                                    LeadActivityProgressesHistory(AadharObj.LeadMasterId, data.SequenceNo, 0, "", "AcceptOfferWithXMLAadharOTP (OFFER): Loan Generated / Offer Accepted with Aadhar successfully!! " + "Pricing Offer:" + priceing + "Loan Amount Offer: " + loan_amount);
                                    res.Msg = "Offer Accepted with Aadhar successfully";
                                    res.Status = true;
                                    return res;
                                }
                                else
                                {
                                    res.Msg = LoanresData.Msg;
                                    res.Status = false;
                                    LeadActivityProgressesHistory(data.Id, data.SequenceNo, 0, "", "Accept Offer With Aadhatr Otp: Loan Not Generated (Offer Accepted with Aadhar)");
                                    return res;
                                }
                            }
                            catch (Exception ex)
                            {
                                TextFileLogHelper.TraceLog("Document Post Api after offer accept :" + ex.ToString());
                                LeadActivityProgressesHistory(data.Id, data.SequenceNo, 0, "", " Accept Offer With Aadhatr Otp: Offer(Aadhar Otp) Failed with Exception");
                                res.Msg = ex.Message;
                                res.Status = false;
                                return res;
                            }

                        }
                        else
                        {
                            res.Msg = resDC.data.result.message != null ? resDC.data.result.message : "Incorrect OTP";
                            res.Status = resDC.success;
                        }
                    }
                    else
                    {
                        res.Msg = "Data Not Found in Lead!";
                    }
                }
            }
            return res;
        }


        //validation XML/Doc APi
        [AllowAnonymous]
        [HttpPost]
        [Route("DocJsonXmlValidation")]
        public async Task<CommonResponseDc> DocJsonXmlValidationAPi(long Leadmasterid)
        {
            using (var db = new AuthContext())
            {
                return await PanAndAadharXmlUpload(Leadmasterid, db);
                //return await DocJsonXmlValidation(Leadmasterid);
            }
        }

        public async Task<CommonResponseDc> DocJsonXmlValidation(long Leadmasterid)
        {
            CommonResponseDc result = new CommonResponseDc();
            ValidationDocResponse res = new ValidationDocResponse();
            byte[] mybytearray = null;
            if (Leadmasterid > 0)
            {
                using (var db = new AuthContext())
                {

                    var LeadId = new SqlParameter("@leadMasterId", Leadmasterid);

                    var otherinfojson = db.Database.SqlQuery<LeadDocumentDc>("EXEC LoanDocumentByLeadId @leadMasterId", LeadId).ToList();

                    foreach (var item in otherinfojson)
                    {
                        ArthMateHelper ArthMateHelper = new ArthMateHelper();
                        mybytearray = item.OtherInfo.ToBytes();
                        var Jsonbase64string = Convert.ToBase64String(mybytearray);
                        string docname = "";

                        JsonXmlRequest jsonXmlRequest = new JsonXmlRequest()
                        {
                            base64pdfencodedfile = Jsonbase64string,
                            borrower_id = item.borrower_id,
                            code = item.DocumentName == "pan_card" ? "116" : "114",
                            loan_app_id = item.loan_app_id,
                            partner_borrower_id = item.partner_borrower_id,
                            partner_loan_app_id = item.partner_loan_app_id
                        };

                        res = await ArthMateHelper.ArthmateDocValidationJsonXml(jsonXmlRequest, Leadmasterid);
                        docname = item.DocumentName;
                        if (res.success)
                        {
                            result.Msg = "Aadhar And Pan  Document Validatedation Success";
                            result.Status = true;

                            LeadActivityProgressesHistory(Leadmasterid, 0, 0, "", "Xml Doc (" + docname + ") Verification Success");
                        }
                        else
                        {
                            result.Msg = "Lead Document Validatedation Failed";
                            result.Status = false;
                            LeadActivityProgressesHistory(Leadmasterid, 0, 0, "", "Xml Doc (" + docname + ") Verification Failed");
                        }
                    }
                }
            }

            return result;
        }



        [AllowAnonymous]
        [HttpPost]
        [Route("LoanApi")]
        public async Task<CommonResponseDc> LoanApi(long LeadMasterId, bool insurance_applied)
        {
            CoLenderResponse ColenderData = null;
            using (AuthContext db = new AuthContext())
            {
                ColenderData = db.CoLenderResponse.Where(x => x.LeadMasterId == LeadMasterId && x.IsActive && x.IsDeleted == false).FirstOrDefault();
            }
            return await CallLoanApi(LeadMasterId, insurance_applied, ColenderData);
        }

        public async Task<CommonResponseDc> CallLoanApi(long LeadMasterId, bool insurance_applied, CoLenderResponse ColenderData)
        {
            CommonResponseDc res = new CommonResponseDc();
            GetLoanApiResponseDC loanres = new GetLoanApiResponseDC();
            LoanApiRequestDc loanApiRequest = new LoanApiRequestDc();
            var identity = User.Identity as ClaimsIdentity;
            int userid = 0;
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
            using (var db = new AuthContext())
            {
                DateTime currentdate = DateTime.Now;
                var Leaddata = db.LeadMasters.Where(x => x.Id == LeadMasterId).FirstOrDefault();

                if (Leaddata == null)
                {
                    res.Msg = "Lead Data Not Found!";
                    return res;
                }
                var Ascoredata = db.AScore.Where(x => x.LeadMasterId == LeadMasterId && x.IsActive == true && x.IsDeleted == false).FirstOrDefault();
                if (Ascoredata == null)
                {
                    res.Msg = "Ascore data Not Found!";
                    return res;
                }

                //var ColenderData = db.CoLenderResponse.Where(x => x.LeadMasterId == LeadMasterId && x.loan_amount > 0 && x.pricing>0 && x.SanctionAmount>0 && x.IsActive == true && x.IsDeleted == false).FirstOrDefault();
                if (ColenderData == null)
                {
                    res.Msg = "Colender data Not Found!";
                    return res;
                }
                var LoanConfigData = db.LoanConfiguration.FirstOrDefault();

                string a_score_request_id = "";
                var co_lender_assignment_id = "";
                double loan_int_rate = 0;
                double sanction_amount = 0;
                DateTime FirstEmiDate;
                DateTime final_approve_date;


                a_score_request_id = Ascoredata.request_id;
                co_lender_assignment_id = Convert.ToString(ColenderData.co_lender_assignment_id);

                sanction_amount = ColenderData.SanctionAmount; //ColenderData.loan_amount;
                final_approve_date = ColenderData.CreatedDate;
                loan_int_rate = LoanConfigData.InterestRate;

                loanApiRequest.partner_loan_app_id = Leaddata.partner_loan_app_id; //": "SK151503-9691885247",
                loanApiRequest.partner_borrower_id = Leaddata.partner_borrower_id; //": "SK15150300001",
                loanApiRequest.loan_app_id = Leaddata.loan_app_id; //": "SKUBL-6127174608110",
                loanApiRequest.borrower_id = Leaddata.borrower_id;// ": "BEPPD49b2G",
                loanApiRequest.partner_loan_id = Leaddata.partner_loan_app_id;// ": "SK151503 - 9691885247",
                loanApiRequest.a_score_request_id = a_score_request_id;// "SHO0161-BUREAU-SCORECARD-V2-1699282989243",
                loanApiRequest.co_lender_assignment_id = co_lender_assignment_id;// ": 1404,
                loanApiRequest.loan_app_date = currentdate.ToString("yyyy-MM-dd");
                loanApiRequest.loan_amount_requested = (Leaddata.EnquiryAmount < sanction_amount ? sanction_amount : Leaddata.EnquiryAmount).ToString();// "300000";
                loanApiRequest.sanction_amount = sanction_amount.ToString();

                double ProcessingFeePer = LoanConfigData.PF;
                double CalProcessingFeeAmt = 0;
                CalProcessingFeeAmt = Convert.ToDouble(sanction_amount) * (ProcessingFeePer / 100);

                //platform fee
                #region  platformfeeamount
                double PlatformFee = LoanConfigData.PlatformFee.Value;
                double PlatformFeeAmt = 0;

                if (PlatformFee > 0)
                {
                    PlatformFeeAmt = (PlatformFee * sanction_amount) / 100;
                    TextFileLogHelper.LogError("PlatformFeeAmt " + PlatformFeeAmt);
                }

                #endregion
                loanApiRequest.processing_fees_perc = ProcessingFeePer.ToString();// ": "2",
                loanApiRequest.processing_fees_amt = CalProcessingFeeAmt.ToString();// ": "6000",

                double GstONPfPer = LoanConfigData.GST;
                double CalGstONPfAmt = Math.Round(CalProcessingFeeAmt * (GstONPfPer / 100), 2);
                double Broken_Period_Interest = 0;

                //FirstEmiDate = new DateTime(currentdate.AddMonths(1).Year, currentdate.AddMonths(1).Month, 5);
                if (currentdate.Day >= 1 && currentdate.Day <= 20)
                { FirstEmiDate = new DateTime(currentdate.AddMonths(1).Year, currentdate.AddMonths(1).Month, 5); }
                else
                { FirstEmiDate = new DateTime(currentdate.AddMonths(2).Year, currentdate.AddMonths(2).Month, 5); }

                int BrokenPeriod = (FirstEmiDate.Date - final_approve_date.Date).Days - 30;
                Broken_Period_Interest = Math.Round(BrokenPeriod * ((sanction_amount * (loan_int_rate / 100)) / 365), 2);
                Broken_Period_Interest = BrokenPeriod < 0 ? 0 : Broken_Period_Interest;
                loanApiRequest.broken_period_int_amt = Broken_Period_Interest.ToString();

                //--------------------------------
                double Calc_insurance_amount = 0;
                int NumberOfApplicants = 1;

                decimal Monthly_EMI_Amt = 0;
                int Loan_Tenure = 0;
                Loan_Tenure = Leaddata.tenure;

                if (insurance_applied == true)
                {
                    var LaonInsuranceData = db.LoanInsuranceConfiguration.Where(x => x.IsActive == true && x.IsDeleted == false && x.MonthDuration >= Loan_Tenure).FirstOrDefault();
                    if (LaonInsuranceData == null)
                    {
                        res.Msg = "Laon Insurance data Not Found!";
                        return res;
                    }

                    Calc_insurance_amount = Math.Round(((LaonInsuranceData.RateOfInterestInPer / 100) * (sanction_amount * NumberOfApplicants)) * 1.18, 2);
                }
                //--------------------------------

                loanApiRequest.gst_on_pf_perc = GstONPfPer.ToString();// "18",
                loanApiRequest.gst_on_pf_amt = CalGstONPfAmt.ToString();// "1080",

                double convenienceFees = 0;
                if (ColenderData.loan_amount <= 5000000 && ColenderData.loan_amount >= 2000000 && (Leaddata.bus_entity_type == "Partnership" || Leaddata.bus_entity_type == "PrivateLtd" || Leaddata.bus_entity_type == "LLP" || Leaddata.bus_entity_type == "HUF" || Leaddata.bus_entity_type == "OPC"))
                {
                    convenienceFees = 5000;
                }
                else if (ColenderData.loan_amount < 500000 && (Leaddata.bus_entity_type == "Proprietorship" || Leaddata.bus_entity_type == "SelfEmployed"))
                {
                    convenienceFees = 500;
                }
                else if (ColenderData.loan_amount >= 500000 && ColenderData.loan_amount <= 1000000 && (Leaddata.bus_entity_type == "Proprietorship" || Leaddata.bus_entity_type == "SelfEmployed"))
                {
                    convenienceFees = 900;
                }
                else if (ColenderData.loan_amount > 100000 && ColenderData.loan_amount < 2000000 && (Leaddata.bus_entity_type == "Proprietorship" || Leaddata.bus_entity_type == "SelfEmployed"))
                {
                    convenienceFees = 2000;
                }
                else if (ColenderData.loan_amount > 2000000 && (Leaddata.bus_entity_type == "Proprietorship" || Leaddata.bus_entity_type == "SelfEmployed"))
                {
                    convenienceFees = 3000;
                }

                loanApiRequest.conv_fees = Convert.ToString(convenienceFees + PlatformFeeAmt);//"0";

                loanApiRequest.insurance_amount = Calc_insurance_amount.ToString();

                //Net disbursement amount:- sanction amount- PF amount-gst on pf- insurance charges- Broken period interest 

                //loanApiRequest.net_disbur_amt = (Convert.ToDouble(sanction_amount) - (CalProcessingFeeAmt + CalGstONPfAmt + Calc_insurance_amount + Broken_Period_Interest)).ToString(); // "286920";
                loanApiRequest.net_disbur_amt = (Convert.ToDouble(sanction_amount) - (CalProcessingFeeAmt + CalGstONPfAmt + Calc_insurance_amount + /*Broken_Period_Interest +*/ Convert.ToDouble(convenienceFees) + PlatformFeeAmt)).ToString();
                loanApiRequest.int_type = "Reducing";
                loanApiRequest.loan_int_rate = loan_int_rate.ToString();

                Monthly_EMI_Amt = Math.Round(pmt(loan_int_rate, Loan_Tenure, sanction_amount), 2);

                loanApiRequest.loan_int_amt = ((Convert.ToDouble(Monthly_EMI_Amt) * Loan_Tenure) - sanction_amount).ToString();// "76400";
                loanApiRequest.emi_amount = Monthly_EMI_Amt.ToString(); // "8166";
                loanApiRequest.tenure = Loan_Tenure.ToString();
                loanApiRequest.emi_count = Loan_Tenure.ToString(); // "36"; 
                                                                   //loanApiRequest.broken_period_int_amt = "0";
                loanApiRequest.repayment_type = "Monthly";
                loanApiRequest.tenure_type = "Month";
                loanApiRequest.first_inst_date = FirstEmiDate.ToString("yyyy-MM-dd");// "2023-12-05"; //currentdate.ToString("yyyyy-MM-dd")
                loanApiRequest.final_approve_date = final_approve_date.ToString("yyyy-MM-dd"); //currentdate.ToString("yyyyy-MM-dd")
                loanApiRequest.final_remarks = "Done";
                loanApiRequest.borro_bank_name = Leaddata.borro_bank_name;// "HDFC Bank";
                loanApiRequest.borro_bank_acc_num = Leaddata.borro_bank_acc_num; //"12401050071454",
                loanApiRequest.borro_bank_ifsc = Leaddata.borro_bank_ifsc;// "Hdfc0001250",
                loanApiRequest.borro_bank_account_holder_name = Leaddata.first_name;// "NEERU DHAMNE",
                loanApiRequest.borro_bank_account_type = "Current";
                loanApiRequest.business_name = Leaddata.business_name; //"Milk ",
                loanApiRequest.business_address_ownership = "Owned";
                loanApiRequest.program_type = string.IsNullOrEmpty(ColenderData.ProgramType) ? "Banking" : ColenderData.ProgramType;//"Banking";
                loanApiRequest.business_entity_type = Leaddata.bus_entity_type == null ? "" : Leaddata.bus_entity_type;
                loanApiRequest.business_pan = Leaddata.bus_pan;// ": "DUMPY1312H",
                loanApiRequest.gst_number = Leaddata.bus_gstno == null ? "" : Leaddata.bus_gstno;  //"fasflkasjflas",
                loanApiRequest.udyam_reg_no = Leaddata.udyam_reg_no;// "UDYAM-MH-18-0130557";
                loanApiRequest.purpose_of_loan = Leaddata.purpose_of_loan;// "61";

                //-----------S----- Additional----------------------------
                loanApiRequest.bene_bank_name = Leaddata.borro_bank_name;//  "HDFC Bank",
                loanApiRequest.bene_bank_acc_num = Leaddata.borro_bank_acc_num; //"12401050071454",
                loanApiRequest.bene_bank_ifsc = Leaddata.borro_bank_ifsc;// "Hdfc0001250",
                loanApiRequest.bene_bank_account_holder_name = Leaddata.first_name;// "NEERU DHAMNE",
                loanApiRequest.bene_bank_account_type = loanApiRequest.borro_bank_account_type;// "Current";
                loanApiRequest.insurance_company = insurance_applied == true ? "GoDigit" : "NA";
                //if (insurance_applied == true)

                loanApiRequest.business_vintage_overall = Leaddata.Msme_VintageDays.ToString();
                loanApiRequest.marital_status = Leaddata.marital_status == null ? "Married" : Leaddata.marital_status; //": "Married",
                loanApiRequest.business_establishment_proof_type = Leaddata.business_establishment_proof_type;
                loanApiRequest.relation_with_applicant = Leaddata.relation_with_applicant == null ? "" : Leaddata.relation_with_applicant;// "",
                loanApiRequest.co_app_or_guar_bureau_type = "";
                loanApiRequest.co_app_or_guar_gender = Leaddata.gender;// "Male";
                loanApiRequest.co_app_or_guar_ntc = "No";
                //-----------E----- Additional----------------------------

                if (Leaddata != null)
                {
                    ArthMateHelper ArthMateHelper = new ArthMateHelper();
                    loanres = await ArthMateHelper.LoanApi(loanApiRequest, userid, LeadMasterId);

                    res.Status = loanres.success;
                    res.Msg = loanres.success == true ? "success" : loanres.message;

                    if (loanres != null)
                    {
                        if (loanres.success == true)
                        { LeadActivityProgressesHistory(LeadMasterId, 0, 0, "Loan Api", "Loan Generated Successfully"); }

                        if (loanres.success == true && loanres.data != null)
                        {

                            res.Status = true;
                            LeadLoan LeadLoanData = new LeadLoan();

                            LeadLoanData.LeadMasterId = LeadMasterId;
                            LeadLoanData.ReponseId = 1;
                            LeadLoanData.RequestId = 1;

                            LeadLoanData.IsSuccess = loanres.success;
                            LeadLoanData.Message = loanres.message;
                            LeadLoanData.CreatedDate = currentdate;
                            LeadLoanData.ModifiedDate = currentdate;
                            LeadLoanData.IsActive = true;
                            LeadLoanData.IsDeleted = false;
                            LeadLoanData.CreatedBy = 1717;
                            LeadLoanData.ModifiedBy = 0;
                            LeadLoanData.PlatformFeeAmt = PlatformFeeAmt;

                            LeadLoanData.loan_app_id = loanres.data.FirstOrDefault().loan_app_id;
                            LeadLoanData.loan_id = loanres.data.FirstOrDefault().loan_id;
                            LeadLoanData.borrower_id = loanres.data.FirstOrDefault().borrower_id;
                            LeadLoanData.partner_loan_app_id = loanres.data.FirstOrDefault().partner_loan_app_id;
                            LeadLoanData.partner_loan_id = loanres.data.FirstOrDefault().partner_loan_id;
                            LeadLoanData.partner_borrower_id = loanres.data.FirstOrDefault().partner_borrower_id;
                            LeadLoanData.company_id = loanres.data.FirstOrDefault().company_id;
                            LeadLoanData.product_id = loanres.data.FirstOrDefault().product_id.ToString();
                            LeadLoanData.loan_app_date = loanres.data.FirstOrDefault().loan_app_date;
                            LeadLoanData.sanction_amount = loanres.data.FirstOrDefault().sanction_amount;
                            LeadLoanData.gst_on_pf_amt = loanres.data.FirstOrDefault().gst_on_pf_amt;
                            LeadLoanData.gst_on_pf_perc = loanres.data.FirstOrDefault().gst_on_pf_perc;
                            LeadLoanData.net_disbur_amt = loanres.data.FirstOrDefault().net_disbur_amt;
                            LeadLoanData.status = loanres.data.FirstOrDefault().status;
                            LeadLoanData.stage = loanres.data.FirstOrDefault().stage;
                            LeadLoanData.exclude_interest_till_grace_period = loanres.data.FirstOrDefault().exclude_interest_till_grace_period;
                            LeadLoanData.borro_bank_account_type = loanres.data.FirstOrDefault().borro_bank_account_type;
                            LeadLoanData.borro_bank_account_holder_name = loanres.data.FirstOrDefault().borro_bank_account_holder_name;
                            LeadLoanData.loan_int_rate = Convert.ToString(LoanConfigData.InterestRate);//"24";//loanres.data.FirstOrDefault().loan_int_rate;
                            LeadLoanData.processing_fees_amt = loanres.data.FirstOrDefault().processing_fees_amt;
                            LeadLoanData.processing_fees_perc = loanres.data.FirstOrDefault().processing_fees_perc;
                            LeadLoanData.tenure = loanres.data.FirstOrDefault().tenure;
                            LeadLoanData.tenure_type = loanres.data.FirstOrDefault().tenure_type;
                            LeadLoanData.int_type = loanres.data.FirstOrDefault().int_type;
                            LeadLoanData.borro_bank_ifsc = loanres.data.FirstOrDefault().borro_bank_ifsc;
                            LeadLoanData.borro_bank_acc_num = loanres.data.FirstOrDefault().borro_bank_acc_num;
                            LeadLoanData.borro_bank_name = loanres.data.FirstOrDefault().borro_bank_name;
                            LeadLoanData.first_name = loanres.data.FirstOrDefault().first_name;
                            LeadLoanData.last_name = loanres.data.FirstOrDefault().last_name;
                            LeadLoanData.current_overdue_value = loanres.data.FirstOrDefault().current_overdue_value;
                            LeadLoanData.bureau_score = loanres.data.FirstOrDefault().bureau_score;
                            //LeadLoanData.monthly_income = loanres.data.FirstOrDefault().monthly_income;
                            LeadLoanData.loan_amount_requested = loanres.data.FirstOrDefault().loan_amount_requested;
                            LeadLoanData.bene_bank_name = loanres.data.FirstOrDefault().bene_bank_name;
                            LeadLoanData.bene_bank_acc_num = loanres.data.FirstOrDefault().bene_bank_acc_num;
                            LeadLoanData.bene_bank_ifsc = loanres.data.FirstOrDefault().bene_bank_ifsc;
                            LeadLoanData.bene_bank_account_holder_name = loanres.data.FirstOrDefault().bene_bank_account_holder_name;
                            LeadLoanData.created_at = loanres.data.FirstOrDefault().created_at;
                            LeadLoanData.updated_at = loanres.data.FirstOrDefault().updated_at;
                            //LeadLoanData.id_loan = loanres.data.FirstOrDefault().id_loan;
                            LeadLoanData.v = loanres.data.FirstOrDefault().__v;
                            LeadLoanData.co_lender_assignment_id = loanres.data.FirstOrDefault().co_lender_assignment_id;
                            LeadLoanData.co_lender_id = loanres.data.FirstOrDefault().co_lender_id;
                            LeadLoanData.co_lend_flag = loanres.data.FirstOrDefault().co_lend_flag;
                            LeadLoanData.itr_ack_no = loanres.data.FirstOrDefault().itr_ack_no;
                            LeadLoanData.penal_interest = loanres.data.FirstOrDefault().penal_interest;
                            LeadLoanData.bounce_charges = loanres.data.FirstOrDefault().bounce_charges;
                            LeadLoanData.repayment_type = loanres.data.FirstOrDefault().repayment_type;
                            LeadLoanData.first_inst_date = loanres.data.FirstOrDefault().first_inst_date;
                            LeadLoanData.final_approve_date = loanres.data.FirstOrDefault().final_approve_date;
                            LeadLoanData.final_remarks = loanres.data.FirstOrDefault().final_remarks;
                            LeadLoanData.foir = loanres.data.FirstOrDefault().foir;
                            LeadLoanData.upfront_interest = loanres.data.FirstOrDefault().upfront_interest;
                            //LeadLoanData.customer_type_ntc = loanres.data.FirstOrDefault().customer_type_ntc;
                            LeadLoanData.business_vintage_overall = loanres.data.FirstOrDefault().business_vintage_overall;
                            //LeadLoanData.gst_number = loanres.data.FirstOrDefault().gst_number;
                            //LeadLoanData.abb = loanres.data.FirstOrDefault().abb;
                            LeadLoanData.loan_int_amt = loanres.data.FirstOrDefault().loan_int_amt;
                            LeadLoanData.conv_fees = loanres.data.FirstOrDefault().conv_fees;
                            LeadLoanData.ninety_plus_dpd_in_last_24_months = loanres.data.FirstOrDefault().ninety_plus_dpd_in_last_24_months;
                            LeadLoanData.dpd_in_last_9_months = loanres.data.FirstOrDefault().dpd_in_last_9_months;
                            LeadLoanData.dpd_in_last_3_months = loanres.data.FirstOrDefault().dpd_in_last_3_months;
                            LeadLoanData.dpd_in_last_6_months = loanres.data.FirstOrDefault().dpd_in_last_6_months;
                            //LeadLoanData.bounces_in_three_month = loanres.data.FirstOrDefault().bounces_in_three_month;
                            LeadLoanData.insurance_company = loanres.data.FirstOrDefault().insurance_company;
                            LeadLoanData.credit_card_settlement_amount = loanres.data.FirstOrDefault().credit_card_settlement_amount;
                            LeadLoanData.emi_amount = loanres.data.FirstOrDefault().emi_amount;
                            LeadLoanData.emi_allowed = loanres.data.FirstOrDefault().emi_allowed;
                            LeadLoanData.igst_amount = loanres.data.FirstOrDefault().igst_amount;
                            LeadLoanData.cgst_amount = loanres.data.FirstOrDefault().cgst_amount;
                            LeadLoanData.sgst_amount = loanres.data.FirstOrDefault().sgst_amount;
                            LeadLoanData.emi_count = loanres.data.FirstOrDefault().emi_count;
                            LeadLoanData.broken_interest = loanres.data.FirstOrDefault().broken_interest;
                            LeadLoanData.dpd_in_last_12_months = loanres.data.FirstOrDefault().dpd_in_last_12_months;
                            LeadLoanData.dpd_in_last_3_months_credit_card = loanres.data.FirstOrDefault().dpd_in_last_3_months_credit_card;
                            LeadLoanData.dpd_in_last_3_months_unsecured = loanres.data.FirstOrDefault().dpd_in_last_3_months_unsecured;
                            LeadLoanData.broken_period_int_amt = loanres.data.FirstOrDefault().broken_period_int_amt;
                            LeadLoanData.dpd_in_last_24_months = loanres.data.FirstOrDefault().dpd_in_last_24_months;
                            LeadLoanData.avg_banking_turnover_6_months = 0;// loanres.data.FirstOrDefault().avg_banking_turnover_6_months;
                            LeadLoanData.enquiries_bureau_30_days = loanres.data.FirstOrDefault().enquiries_bureau_30_days;
                            LeadLoanData.cnt_active_unsecured_loans = loanres.data.FirstOrDefault().cnt_active_unsecured_loans;
                            LeadLoanData.total_overdues_in_cc = loanres.data.FirstOrDefault().total_overdues_in_cc;
                            LeadLoanData.insurance_amount = loanres.data.FirstOrDefault().insurance_amount;
                            LeadLoanData.bureau_outstanding_loan_amt = loanres.data.FirstOrDefault().bureau_outstanding_loan_amt;
                            LeadLoanData.purpose_of_loan = loanres.data.FirstOrDefault().purpose_of_loan;
                            LeadLoanData.business_name = loanres.data.FirstOrDefault().business_name;
                            LeadLoanData.co_app_or_guar_name = loanres.data.FirstOrDefault().co_app_or_guar_name;
                            LeadLoanData.co_app_or_guar_address = loanres.data.FirstOrDefault().co_app_or_guar_address;
                            LeadLoanData.co_app_or_guar_mobile_no = loanres.data.FirstOrDefault().co_app_or_guar_mobile_no;
                            LeadLoanData.co_app_or_guar_pan = loanres.data.FirstOrDefault().co_app_or_guar_pan;
                            //LeadLoanData.business_address = loanres.data.FirstOrDefault().business_address;
                            //LeadLoanData.business_state = loanres.data.FirstOrDefault().business_state;
                            //LeadLoanData.business_city = loanres.data.FirstOrDefault().business_city;
                            //LeadLoanData.business_pin_code = loanres.data.FirstOrDefault().business_pin_code;
                            LeadLoanData.business_address_ownership = loanres.data.FirstOrDefault().business_address_ownership;
                            LeadLoanData.business_pan = loanres.data.FirstOrDefault().business_pan;
                            LeadLoanData.bureau_fetch_date = currentdate;// loanres.data.FirstOrDefault().bureau_fetch_date;
                            LeadLoanData.enquiries_in_last_3_months = loanres.data.FirstOrDefault().enquiries_in_last_3_months;
                            LeadLoanData.gst_on_conv_fees = loanres.data.FirstOrDefault().gst_on_conv_fees;
                            LeadLoanData.cgst_on_conv_fees = loanres.data.FirstOrDefault().cgst_on_conv_fees;
                            LeadLoanData.sgst_on_conv_fees = loanres.data.FirstOrDefault().sgst_on_conv_fees;
                            LeadLoanData.igst_on_conv_fees = loanres.data.FirstOrDefault().igst_on_conv_fees;
                            LeadLoanData.interest_type = loanres.data.FirstOrDefault().interest_type;
                            LeadLoanData.conv_fees_excluding_gst = loanres.data.FirstOrDefault().conv_fees_excluding_gst;
                            LeadLoanData.a_score_request_id = loanres.data.FirstOrDefault().a_score_request_id;
                            LeadLoanData.a_score = loanres.data.FirstOrDefault().a_score;
                            LeadLoanData.b_score = loanres.data.FirstOrDefault().b_score;
                            LeadLoanData.offered_amount = loanres.data.FirstOrDefault().offered_amount;
                            LeadLoanData.offered_int_rate = loanres.data.FirstOrDefault().offered_int_rate.Value;
                            LeadLoanData.monthly_average_balance = loanres.data.FirstOrDefault().monthly_average_balance.Value;
                            LeadLoanData.monthly_imputed_income = loanres.data.FirstOrDefault().monthly_imputed_income.Value;
                            LeadLoanData.party_type = loanres.data.FirstOrDefault().party_type;
                            LeadLoanData.co_app_or_guar_dob = currentdate; //loanres.data.FirstOrDefault().co_app_or_guar_dob;
                            LeadLoanData.co_app_or_guar_gender = loanres.data.FirstOrDefault().co_app_or_guar_gender;
                            LeadLoanData.co_app_or_guar_ntc = loanres.data.FirstOrDefault().co_app_or_guar_ntc;
                            LeadLoanData.udyam_reg_no = loanres.data.FirstOrDefault().udyam_reg_no;
                            LeadLoanData.program_type = loanres.data.FirstOrDefault().program_type;
                            LeadLoanData.written_off_settled = loanres.data.FirstOrDefault().written_off_settled;
                            LeadLoanData.upi_handle = loanres.data.FirstOrDefault().upi_handle;
                            LeadLoanData.upi_reference = loanres.data.FirstOrDefault().upi_reference;
                            LeadLoanData.fc_offer_days = loanres.data.FirstOrDefault().fc_offer_days;
                            LeadLoanData.foreclosure_charge = loanres.data.FirstOrDefault().foreclosure_charge;
                            LeadLoanData.eligible_loan_amount = 0;// loanres.data.FirstOrDefault().eligible_loan_amount;
                            LeadLoanData.UrlSlaDocument = "";
                            LeadLoanData.UrlSlaUploadSignedDocument = "";
                            LeadLoanData.IsUpload = false;
                            LeadLoanData.UrlSlaUploadDocument_id = "";
                            db.LeadLoan.Add(LeadLoanData);
                            db.Commit();

                        }
                    }
                }
                else
                {
                    loanres.message = "Data Not Found in Lead!";
                }

            }
            return res;
        }
        [AllowAnonymous]
        [HttpGet]
        [Route("GetLoan")] //Leadloan Table all data 
        public List<LeadLoanDataDc> GetLoan(long LeadMasterId)
        {
            List<LeadLoanDataDc> leadLoanDataDc = new List<LeadLoanDataDc>();
            using (var db = new AuthContext())
            {
                var Leadid = new SqlParameter("@leadmasterid", LeadMasterId);
                leadLoanDataDc = db.Database.SqlQuery<LeadLoanDataDc>("EXEC [Arthmate].[GetLoanData] @leadmasterid", Leadid).ToList();

                if (leadLoanDataDc != null)
                {
                    return leadLoanDataDc;
                }
                else
                {
                    return null;
                }
            }
        }


        [AllowAnonymous]
        [HttpGet]
        [Route("UploadSlaDocument")]//for retailer app nikhil sir..not used
        public async Task<string> UploadSlaDocument(long Leadmasterid)
        {

            string htmldata = "";
            string replacetext = "";
            string path = "";
            string newfilepath = "";
            string thFileName = "";

            repay_scheduleDc Repyment_ScduleData = new repay_scheduleDc();
            using (AuthContext context = new AuthContext())
            {
                var data = context.LeadMasters.Where(x => x.Id == Leadmasterid).FirstOrDefault();
                var pdfdata = context.LoanConfiguration.FirstOrDefault();
                var laondata = context.LeadLoan.Where(x => x.LeadMasterId.Value == Leadmasterid).FirstOrDefault();
                //Repayment Schedule data 
                ArthMateHelper arthMateHelper = new ArthMateHelper();
                if (laondata != null)
                {
                    Postrepayment_scheduleDc reqdata = new Postrepayment_scheduleDc()
                    {
                        company_id = laondata.company_id.Value,
                        loan_id = laondata.loan_id,
                        product_id = laondata.product_id
                    };
                    Repyment_ScduleData = await arthMateHelper.repayment_schedule(reqdata, 0, Leadmasterid, false);
                }
                path = System.Web.Hosting.HostingEnvironment.MapPath(@"~\Templates\Arthmate\LBABusinessLoan.html");

                if (File.Exists(path))
                {
                    htmldata = File.ReadAllText(path);
                    if (!string.IsNullOrEmpty(htmldata))
                    {
                        replacetext = $"{data.first_name + data.last_name} ";
                        htmldata = htmldata.Replace("{{NameOfBorrower}}", replacetext);

                        replacetext = $"{data.Id} ";
                        htmldata = htmldata.Replace("{{LeadId}}", replacetext);

                        replacetext = $"{data.loan_app_id} ";
                        htmldata = htmldata.Replace("{{loan_app_id}}", replacetext);//loan reference

                        replacetext = $"{data.co_app_name} ";
                        htmldata = htmldata.Replace("{{co_app_name}}", replacetext);

                        replacetext = $"{data.appl_pan} "; //PAN of Borrower
                        htmldata = htmldata.Replace("{{appl_pan}}", replacetext);

                        replacetext = $"{data.co_app_pan} ";//PAN of Co-Borrower
                        htmldata = htmldata.Replace("{{co_app_pan}}", replacetext);

                        replacetext = $"{pdfdata.InterestRate} "; //Rate of Interest
                        htmldata = htmldata.Replace("{{InterestRate}}", replacetext);

                        replacetext = $" {pdfdata.PF} ";
                        htmldata = htmldata.Replace("{{ProcessingFees}}", replacetext);

                        replacetext = $" {data.first_name + data.last_name} ";
                        htmldata = htmldata.Replace("{{AccountHolderName}}", replacetext);

                        replacetext = $" {data.borro_bank_name} ";
                        htmldata = htmldata.Replace("{{bankName}}", replacetext);

                        replacetext = $" {data.borro_bank_acc_num} ";
                        htmldata = htmldata.Replace("{{accountNumber}}", replacetext);

                        replacetext = $" {data.borro_bank_ifsc} ";
                        htmldata = htmldata.Replace("{{IFSCCode}}", replacetext);

                        //sanction letter
                        replacetext = $"{DateTime.Now} ";
                        htmldata = htmldata.Replace("{{DATE}}", replacetext);

                        replacetext = $"{data.first_name} ";
                        htmldata = htmldata.Replace("{{BorrowerName}}", replacetext);

                        replacetext = $"{data.per_addr_ln1} ";
                        htmldata = htmldata.Replace("{{address1}}", replacetext);

                        replacetext = $"{data.per_addr_ln2} ";
                        htmldata = htmldata.Replace("{{address2}}", replacetext);

                        replacetext = $"{data.city} ";
                        htmldata = htmldata.Replace("{{city}}", replacetext);

                        replacetext = $"{data.state} ";
                        htmldata = htmldata.Replace("{{state}}", replacetext);

                        replacetext = $"{data.MobileNo} ";
                        htmldata = htmldata.Replace("{{MobileNo}}", replacetext);

                        replacetext = $"{data.email_id} ";
                        htmldata = htmldata.Replace("{{email_id}}", replacetext);

                        replacetext = $" {data.CreatedDate} ";
                        htmldata = htmldata.Replace("{{ApplicationDate}}", replacetext);

                        replacetext = $" {data.first_name + data.last_name} ";
                        htmldata = htmldata.Replace("{{BorrowerNamess}}", replacetext);

                        replacetext = $" {data.co_app_name} ";
                        htmldata = htmldata.Replace("{{co-borrowerName}}", replacetext);
                        replacetext = $" {data.bus_add_corr_line1} ";
                        htmldata = htmldata.Replace("{{bus_add_corr_line1}}", replacetext);


                        replacetext = $" {pdfdata.InterestRate} ";
                        htmldata = htmldata.Replace("{{InterestRate}}", replacetext);

                        replacetext = $" {pdfdata.PF} ";
                        htmldata = htmldata.Replace("{{ProcessingFees}}", replacetext);

                        replacetext = $" {data.purpose_of_loan} ";
                        htmldata = htmldata.Replace("{{purpose_of_loan}}", replacetext);

                        replacetext = $" {laondata.sanction_amount} ";
                        htmldata = htmldata.Replace("{{SanctionAmount}}", replacetext);

                        replacetext = $" {laondata.loan_amount_requested} ";
                        htmldata = htmldata.Replace("{{loan_amount_requested}}", replacetext);

                        replacetext = $" {laondata.loan_int_amt} ";
                        htmldata = htmldata.Replace("{{loan_int_amt}}", replacetext);

                        replacetext = $" {laondata.insurance_amount} ";
                        htmldata = htmldata.Replace("{{insurance_amount}}", replacetext);

                        replacetext = $" {laondata.tenure} ";
                        htmldata = htmldata.Replace("{{tenure}}", replacetext);

                        replacetext = $" {laondata.tenure} ";
                        htmldata = htmldata.Replace("{{tenure}}", replacetext);

                        //repayment schedule data
                        if (Repyment_ScduleData.data != null)
                        {
                            foreach (var item in Repyment_ScduleData.data.rows)
                            {
                                replacetext = $" {item.emi_no} ";
                                htmldata = htmldata.Replace("{{emi_no}}", replacetext);

                                replacetext = $" {0} ";
                                htmldata = htmldata.Replace("{{principal_outstanding}}", replacetext);

                                replacetext = $" {item.prin} ";
                                htmldata = htmldata.Replace("{{prin}}", replacetext);

                                //replacetext = $" {item.interest} ";   //INTETREST 
                                //htmldata = htmldata.Replace("{{ProcessingFees}}", replacetext);

                                replacetext = $" {item.emi_amount} ";
                                htmldata = htmldata.Replace("{{emi_amount}}", replacetext);

                            }
                        }
                    }
                }
                if (!string.IsNullOrEmpty(htmldata))
                {

                    path = System.Web.Hosting.HostingEnvironment.MapPath(@"~\Templates\Arthmate\LBABusinessLoan.html");

                    string TartgetfolderPath = HttpContext.Current.Server.MapPath(@"~\ArthmateDocument\SlaDocument");
                    if (!Directory.Exists(TartgetfolderPath))
                        Directory.CreateDirectory(TartgetfolderPath);

                    ///ArthmateDocument/SlaDocument
                    thFileName = "SanctionAndLbaBusinessLoan" + DateTime.Now.ToString("MMddyyyyhhmmss") + ".pdf";
                    newfilepath = "/ArthmateDocument/SlaDocument/" + thFileName;
                    string OutPutFile = Path.Combine(HttpContext.Current.Server.MapPath("~/ArthmateDocument/SlaDocument"), thFileName);

                    byte[] pdf = null;

                    pdf = Pdf
                          .From(htmldata)
                          .OfSize(OpenHtmlToPdf.PaperSize.A4)
                          .WithTitle("Invoice")
                          .WithoutOutline()
                          .WithMargins(PaperMargins.All(0.0.Millimeters()))
                          .Portrait()
                          .Comressed()
                          .Content();
                    FileStream file = File.Create(OutPutFile);
                    file.Write(pdf, 0, pdf.Length);
                    file.Close();
                }


                var LeadLoanDocuments = context.LeadDocument.Where(x => x.LeadMasterId == Leadmasterid && x.OtherInfo == "Agreement" && x.FrontFileUrl != "" && x.BackFileUrl == null && x.IsActive == true && x.IsDeleted == false).FirstOrDefault();

                if (LeadLoanDocuments != null)
                {
                    LeadLoanDocuments.IsActive = false;
                    LeadLoanDocuments.IsDeleted = true;
                    context.Entry(LeadLoanDocuments).State = EntityState.Modified;
                    //context.Commit();
                }

                long DocumentMasterId = context.ArthMateDocumentMasters.Where(x => x.DocumentName == "agreement").FirstOrDefault().Id;
                LeadLoanDocument leadLoanDocument = new LeadLoanDocument()
                {
                    FrontFileUrl = newfilepath,
                    BackFileUrl = null,
                    CreatedBy = 0,
                    CreatedDate = DateTime.Now,
                    DocumentNumber = "",
                    DocumentMasterId = DocumentMasterId,
                    IsActive = true,
                    LeadMasterId = data.Id,
                    OtherInfo = "Agreement",
                    IsVerified = true,
                    IsDeleted = false
                };
                context.LeadDocument.Add(leadLoanDocument);
                context.Commit();


                var Loandata = context.LeadLoan.Where(x => x.LeadMasterId == Leadmasterid).FirstOrDefault();
                if (Loandata != null)
                {
                    Loandata.UrlSlaDocument = newfilepath;
                    context.Entry(Loandata).State = EntityState.Modified;
                    context.Commit();
                }
            }
            return newfilepath;
        }



        private decimal pmt(double yearlyinterestrate, int totalnumberofmonths, double loanamount)
        {
            if (yearlyinterestrate > 0)
            {
                var rate = (double)yearlyinterestrate / 100 / 12;
                var denominator = Math.Pow((1 + rate), totalnumberofmonths) - 1;
                return new decimal((rate + (rate / denominator)) * loanamount);
            }
            return totalnumberofmonths > 0 ? new decimal(loanamount / totalnumberofmonths) : 0;
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("pmtTest")]//for retailer app nikhil sir..not used
        public async Task<decimal> pmtTest(double yearlyinterestrate, int totalnumberofmonths, double loanamount)
        {
            return pmt(yearlyinterestrate, totalnumberofmonths, loanamount);
        }


        [AllowAnonymous]
        [HttpPost]
        [Route("LoanApiSaveResponseTesting")]
        public async Task<bool> LoanApiSaveResponse(ResponseLoanData loanres, long LeadMasterId)
        {
            bool res = false;
            DateTime currentdate = DateTime.Now;
            try
            {
                using (var db = new AuthContext())
                {
                    long RowID = 4476;
                    var ArthmateReqResp = db.ArthmateReqResp.Where(x => x.LeadMasterId == LeadMasterId && x.Type == "Response" && x.ActivityMasterId == 35).OrderByDescending(x => x.Id).FirstOrDefault();

                    string jsonString = ArthmateReqResp.RequestResponseMsg;
                    GetLoanApiResponseDC responseDc123 = new GetLoanApiResponseDC();
                    responseDc123 = JsonConvert.DeserializeObject<GetLoanApiResponseDC>(jsonString);

                    //long LeadMasterId = 147;

                    var LeadLoanDta = db.LeadLoan.Where(x => x.LeadMasterId == LeadMasterId).FirstOrDefault();
                    if (LeadLoanDta == null)
                    {
                        LeadLoan LeadLoanData = new LeadLoan();

                        LeadLoanData.LeadMasterId = LeadMasterId;
                        LeadLoanData.ReponseId = 1;
                        LeadLoanData.RequestId = 1;
                        LeadLoanData.IsSuccess = true;
                        LeadLoanData.Message = "";

                        // LeadLoanData.Message = loanres.message;
                        LeadLoanData.CreatedDate = currentdate;
                        LeadLoanData.ModifiedDate = currentdate;
                        LeadLoanData.IsActive = true;
                        LeadLoanData.IsDeleted = false;
                        LeadLoanData.CreatedBy = 1717;
                        LeadLoanData.ModifiedBy = 0;

                        LeadLoanData.loan_app_id = loanres.loan_app_id;
                        LeadLoanData.loan_id = loanres.loan_id;
                        LeadLoanData.borrower_id = loanres.borrower_id;
                        LeadLoanData.partner_loan_app_id = loanres.partner_loan_app_id;
                        LeadLoanData.partner_loan_id = loanres.partner_loan_id;
                        LeadLoanData.partner_borrower_id = loanres.partner_borrower_id;
                        LeadLoanData.company_id = loanres.company_id;
                        LeadLoanData.product_id = loanres.product_id.ToString();
                        LeadLoanData.loan_app_date = loanres.loan_app_date;
                        LeadLoanData.sanction_amount = loanres.sanction_amount;
                        LeadLoanData.gst_on_pf_amt = loanres.gst_on_pf_amt;
                        LeadLoanData.gst_on_pf_perc = loanres.gst_on_pf_perc;
                        LeadLoanData.net_disbur_amt = loanres.net_disbur_amt;
                        LeadLoanData.status = loanres.status;
                        LeadLoanData.stage = loanres.stage;
                        LeadLoanData.exclude_interest_till_grace_period = loanres.exclude_interest_till_grace_period;
                        LeadLoanData.borro_bank_account_type = loanres.borro_bank_account_type;
                        LeadLoanData.borro_bank_account_holder_name = loanres.borro_bank_account_holder_name;
                        LeadLoanData.loan_int_rate = loanres.loan_int_rate;
                        LeadLoanData.processing_fees_amt = loanres.processing_fees_amt;
                        LeadLoanData.processing_fees_perc = loanres.processing_fees_perc;
                        LeadLoanData.tenure = loanres.tenure;
                        LeadLoanData.tenure_type = loanres.tenure_type;
                        LeadLoanData.int_type = loanres.int_type;
                        LeadLoanData.borro_bank_ifsc = loanres.borro_bank_ifsc;
                        LeadLoanData.borro_bank_acc_num = loanres.borro_bank_acc_num;
                        LeadLoanData.borro_bank_name = loanres.borro_bank_name;
                        LeadLoanData.first_name = loanres.first_name;
                        LeadLoanData.last_name = loanres.last_name;
                        LeadLoanData.current_overdue_value = loanres.current_overdue_value;
                        LeadLoanData.bureau_score = loanres.bureau_score;
                        //LeadLoanData.monthly_income = loanres.monthly_income;
                        LeadLoanData.loan_amount_requested = loanres.loan_amount_requested;
                        LeadLoanData.bene_bank_name = loanres.bene_bank_name;
                        LeadLoanData.bene_bank_acc_num = loanres.bene_bank_acc_num;
                        LeadLoanData.bene_bank_ifsc = loanres.bene_bank_ifsc;
                        LeadLoanData.bene_bank_account_holder_name = loanres.bene_bank_account_holder_name;
                        LeadLoanData.created_at = loanres.created_at;
                        LeadLoanData.updated_at = loanres.updated_at;
                        //LeadLoanData.id_loan = loanres.id_loan;
                        LeadLoanData.v = loanres.__v;
                        LeadLoanData.co_lender_assignment_id = loanres.co_lender_assignment_id;
                        LeadLoanData.co_lender_id = loanres.co_lender_id;
                        LeadLoanData.co_lend_flag = loanres.co_lend_flag;
                        LeadLoanData.itr_ack_no = loanres.itr_ack_no;
                        LeadLoanData.penal_interest = loanres.penal_interest;
                        LeadLoanData.bounce_charges = loanres.bounce_charges;
                        LeadLoanData.repayment_type = loanres.repayment_type;
                        LeadLoanData.first_inst_date = loanres.first_inst_date;
                        LeadLoanData.final_approve_date = loanres.final_approve_date;
                        LeadLoanData.final_remarks = loanres.final_remarks;
                        LeadLoanData.foir = loanres.foir;
                        LeadLoanData.upfront_interest = loanres.upfront_interest;
                        //LeadLoanData.customer_type_ntc = loanres.customer_type_ntc;
                        LeadLoanData.business_vintage_overall = loanres.business_vintage_overall;
                        //LeadLoanData.gst_number = loanres.gst_number;
                        //LeadLoanData.abb = loanres.abb;
                        LeadLoanData.loan_int_amt = loanres.loan_int_amt;
                        LeadLoanData.conv_fees = loanres.conv_fees;
                        LeadLoanData.ninety_plus_dpd_in_last_24_months = loanres.ninety_plus_dpd_in_last_24_months;
                        LeadLoanData.dpd_in_last_9_months = loanres.dpd_in_last_9_months;
                        LeadLoanData.dpd_in_last_3_months = loanres.dpd_in_last_3_months;
                        LeadLoanData.dpd_in_last_6_months = loanres.dpd_in_last_6_months;
                        //LeadLoanData.bounces_in_three_month = loanres.bounces_in_three_month;
                        LeadLoanData.insurance_company = loanres.insurance_company;
                        LeadLoanData.credit_card_settlement_amount = loanres.credit_card_settlement_amount;
                        LeadLoanData.emi_amount = loanres.emi_amount;
                        LeadLoanData.emi_allowed = loanres.emi_allowed;
                        LeadLoanData.igst_amount = loanres.igst_amount;
                        LeadLoanData.cgst_amount = loanres.cgst_amount;
                        LeadLoanData.sgst_amount = loanres.sgst_amount;
                        LeadLoanData.emi_count = loanres.emi_count;
                        LeadLoanData.broken_interest = loanres.broken_interest;
                        LeadLoanData.dpd_in_last_12_months = loanres.dpd_in_last_12_months;
                        LeadLoanData.dpd_in_last_3_months_credit_card = loanres.dpd_in_last_3_months_credit_card;
                        LeadLoanData.dpd_in_last_3_months_unsecured = loanres.dpd_in_last_3_months_unsecured;
                        LeadLoanData.broken_period_int_amt = loanres.broken_period_int_amt;
                        LeadLoanData.dpd_in_last_24_months = loanres.dpd_in_last_24_months;
                        LeadLoanData.avg_banking_turnover_6_months = 0;// loanres.avg_banking_turnover_6_months;
                        LeadLoanData.enquiries_bureau_30_days = loanres.enquiries_bureau_30_days;
                        LeadLoanData.cnt_active_unsecured_loans = loanres.cnt_active_unsecured_loans;
                        LeadLoanData.total_overdues_in_cc = loanres.total_overdues_in_cc;
                        LeadLoanData.insurance_amount = loanres.insurance_amount;
                        LeadLoanData.bureau_outstanding_loan_amt = loanres.bureau_outstanding_loan_amt;
                        LeadLoanData.purpose_of_loan = loanres.purpose_of_loan;
                        LeadLoanData.business_name = loanres.business_name;
                        LeadLoanData.co_app_or_guar_name = loanres.co_app_or_guar_name;
                        LeadLoanData.co_app_or_guar_address = loanres.co_app_or_guar_address;
                        LeadLoanData.co_app_or_guar_mobile_no = loanres.co_app_or_guar_mobile_no;
                        LeadLoanData.co_app_or_guar_pan = loanres.co_app_or_guar_pan;
                        //LeadLoanData.business_address = loanres.business_address;
                        //LeadLoanData.business_state = loanres.business_state;
                        //LeadLoanData.business_city = loanres.business_city;
                        //LeadLoanData.business_pin_code = loanres.business_pin_code;
                        LeadLoanData.business_address_ownership = loanres.business_address_ownership;
                        LeadLoanData.business_pan = loanres.business_pan;
                        LeadLoanData.bureau_fetch_date = currentdate;// loanres.bureau_fetch_date;
                        LeadLoanData.enquiries_in_last_3_months = loanres.enquiries_in_last_3_months;
                        LeadLoanData.gst_on_conv_fees = loanres.gst_on_conv_fees;
                        LeadLoanData.cgst_on_conv_fees = loanres.cgst_on_conv_fees;
                        LeadLoanData.sgst_on_conv_fees = loanres.sgst_on_conv_fees;
                        LeadLoanData.igst_on_conv_fees = loanres.igst_on_conv_fees;
                        LeadLoanData.interest_type = loanres.interest_type;
                        LeadLoanData.conv_fees_excluding_gst = loanres.conv_fees_excluding_gst;
                        LeadLoanData.a_score_request_id = loanres.a_score_request_id;
                        LeadLoanData.a_score = loanres.a_score;
                        LeadLoanData.b_score = loanres.b_score;
                        LeadLoanData.offered_amount = loanres.offered_amount;
                        LeadLoanData.offered_int_rate = loanres.offered_int_rate.Value;
                        LeadLoanData.monthly_average_balance = loanres.monthly_average_balance.Value;
                        LeadLoanData.monthly_imputed_income = loanres.monthly_imputed_income.Value;
                        LeadLoanData.party_type = loanres.party_type;
                        LeadLoanData.co_app_or_guar_dob = currentdate; //loanres.co_app_or_guar_dob;
                        LeadLoanData.co_app_or_guar_gender = loanres.co_app_or_guar_gender;
                        LeadLoanData.co_app_or_guar_ntc = loanres.co_app_or_guar_ntc;
                        LeadLoanData.udyam_reg_no = loanres.udyam_reg_no;
                        LeadLoanData.program_type = loanres.program_type;
                        LeadLoanData.written_off_settled = loanres.written_off_settled;
                        LeadLoanData.upi_handle = loanres.upi_handle;
                        LeadLoanData.upi_reference = loanres.upi_reference;
                        LeadLoanData.fc_offer_days = loanres.fc_offer_days;
                        LeadLoanData.foreclosure_charge = loanres.foreclosure_charge;
                        LeadLoanData.eligible_loan_amount = 0;// loanres.eligible_loan_amount;

                        LeadLoanData.UrlSlaDocument = "";
                        LeadLoanData.UrlSlaUploadSignedDocument = "";
                        LeadLoanData.IsUpload = false;
                        LeadLoanData.UrlSlaUploadDocument_id = "";
                        db.LeadLoan.Add(LeadLoanData);
                        db.Commit();

                    }
                }
                res = true;

            }
            catch (Exception ex)
            {
                res = false;
            }

            return true;
        }




        //from 16-10-2023 start Not Used
        [AllowAnonymous]
        [HttpPost]
        [Route("RepaymentAPIV2")]
        public async Task<RepaymentAllResponse> RepaymentAPIV2(List<RePaymentApiReq> ReqstJson)
        {
            RepaymentAllResponse res = new RepaymentAllResponse();
            var identity = User.Identity as ClaimsIdentity;
            int userid = 0;

            if (ReqstJson != null)
            {
                using (var db = new AuthContext())
                {
                    string loanappid = ReqstJson.FirstOrDefault().loan_id;
                    var data = db.LeadMasters.Where(x => x.loan_app_id == loanappid).FirstOrDefault();
                    if (data != null)
                    {
                        long llid = data.Id;
                        ArthMateHelper ArthMateHelper = new ArthMateHelper();
                        //  res = await ArthMateHelper.RepaymentAPI(ReqstJson, userid, llid);
                        // loanres = await ArthMateHelper.LoanApi(ReqstJson, userid, data.Id);
                    }
                    else
                    {
                        res.message = "Data Not Found in Lead!";
                    }
                }
            }

            return res;
        }


        [AllowAnonymous]
        [HttpPost]
        [Route("LoanDocumentApi")]
        public async Task<CommonResponseDc> LoanDocument_Api(long LeadMasterID)
        {

            return await LoanDocumentApi(LeadMasterID);
        }

        public async Task<CommonResponseDc> LoanDocumentApi(long LeadMasterID)
        {
            CommonResponseDc commonResponseDc = new CommonResponseDc();
            LoanDocumentResponseDc res = new LoanDocumentResponseDc();
            LoanDocumentPostDc ReqstJson = new LoanDocumentPostDc();
            bool IsBankStatement = false;
            if (ReqstJson != null)
            {
                using (var db = new AuthContext())
                {
                    var requestjsondata = (from k in db.LeadMasters
                                           join ld in db.LeadDocument on k.Id equals ld.LeadMasterId
                                           join adm in db.ArthMateDocumentMasters on ld.DocumentMasterId equals adm.Id
                                           where k.Id == LeadMasterID && ld.IsVerified == true
                                           select new
                                           {
                                               k.borrower_id,
                                               k.partner_borrower_id,
                                               k.partner_loan_app_id,
                                               k.loan_app_id,
                                               ld.FrontFileUrl,
                                               ld.BackFileUrl,
                                               adm.DocumentTypeCode,
                                               adm.DocumentName,
                                               k.Id,
                                               ld.DocumentMasterId,
                                               ld.PdfPassword
                                           }
                                           ).ToList();

                    foreach (var item in requestjsondata)
                    {
                        if (requestjsondata != null)
                        {

                            int iLoop = 1;
                            if (!string.IsNullOrEmpty(item.FrontFileUrl) && !string.IsNullOrEmpty(item.BackFileUrl))
                            { iLoop = 2; }

                            for (int i = 0; i < iLoop; i++)
                            {
                                //DocBase64String = ConvertToBase64String(item.DocumentTypeCode == "006" && item.FrontFileUrl != null && item.BackFileUrl != null ? item.BackFileUrl : item.BackFileUrl);

                                string DocBase64String = "";
                                string sCode = "";
                                //string DocBase64String = ConvertToBase64String(item.FrontFileUrl);
                                if (i == 0)
                                {
                                    DocBase64String = ConvertToBase64String(item.FrontFileUrl);
                                    sCode = item.DocumentTypeCode;
                                    IsBankStatement = item.DocumentTypeCode == "099" ? true : false;
                                }
                                else if (iLoop > 1 && i == 1)
                                {
                                    DocBase64String = ConvertToBase64String(item.BackFileUrl);
                                    sCode = "115";
                                }

                                LoanDocumentPostDc PostJson = new LoanDocumentPostDc
                                {
                                    base64pdfencodedfile = DocBase64String,
                                    borrower_id = item.borrower_id,
                                    loan_app_id = item.loan_app_id,
                                    partner_loan_app_id = item.partner_loan_app_id,
                                    code = sCode,
                                    partner_borrower_id = item.partner_borrower_id,
                                    PdfPassword = item.PdfPassword
                                };
                                ArthMateHelper ArthMateHelper = new ArthMateHelper();
                                res = await ArthMateHelper.LoanDocumentApi(PostJson, item.Id, IsBankStatement);
                                if (res.uploadDocumentData != null)
                                {
                                    if (res.uploadDocumentData.document_id > 0)
                                    {
                                        commonResponseDc.Status = true;
                                        commonResponseDc.Msg = "Loan document uploaded successfully";

                                        string query = "Update LeadLoanDocuments set RequestId =" + res.RequestId + ",ResponseId=" + res.ReponseId + ", ModifiedBy =1, ModifiedDate='" + DateTime.Now.ToString("yyyy/MM/dd") + "' Where LeadMasterId=" + item.Id + " and DocumentMasterId=" + item.DocumentMasterId + " and IsActive=1 and IsDeleted=0 ";
                                        int update = db.Database.ExecuteSqlCommand(query);
                                    }

                                }
                            }

                            LeadActivityProgressesHistory(LeadMasterID, 0, 0, "Document Api", "All Loan Documents (" + item.DocumentName + ") Uploaded Successfully");
                        }
                        else
                        {
                            res.message = "Data Not Found in Lead!";
                        }
                    }
                }
            }

            return commonResponseDc;
        }


        [AllowAnonymous]
        [HttpPost]
        [Route("ArthmatePostDoc")]
        public async Task<CommonResponseDc> ArthmatePostDoc(long LeadMasterID, string DocName)
        {
            CommonResponseDc commonResponseDc = new CommonResponseDc();
            LoanDocumentResponseDc res = new LoanDocumentResponseDc();
            LoanDocumentPostDc ReqstJson = new LoanDocumentPostDc();
            string uploadMsg = "";
            if (ReqstJson != null)
            {
                using (var db = new AuthContext())
                {
                    var DocumentName = new SqlParameter("@leadmasterid", LeadMasterID);
                    var LeadIid = new SqlParameter("@Document", DocName);
                    var requestjsondata = db.Database.SqlQuery<UploadSlaDocDc>("EXEC [Arthmate].[UploadSlaDocument] @Document,@leadmasterid", DocumentName, LeadIid).ToList();


                    foreach (var item in requestjsondata)
                    {
                        if (requestjsondata != null)
                        {

                            int iLoop = 1;
                            if (!string.IsNullOrEmpty(item.FrontFileUrl) && !string.IsNullOrEmpty(item.BackFileUrl))
                            { iLoop = 2; }

                            for (int i = 0; i < iLoop; i++)
                            {
                                //DocBase64String = ConvertToBase64String(item.DocumentTypeCode == "006" && item.FrontFileUrl != null && item.BackFileUrl != null ? item.BackFileUrl : item.BackFileUrl);

                                string DocBase64String = "";
                                string sCode = "";
                                bool IsBankStatement = false;
                                //string DocBase64String = ConvertToBase64String(item.FrontFileUrl);
                                if (i == 0)
                                {
                                    DocBase64String = ConvertToBase64String(item.FrontFileUrl);
                                    sCode = item.DocumentTypeCode;
                                    IsBankStatement = item.DocumentTypeCode == "099" ? true : false;

                                }
                                else if (iLoop > 1 && i == 1)
                                {
                                    DocBase64String = ConvertToBase64String(item.BackFileUrl);
                                    sCode = "132";
                                }

                                LoanDocumentPostDc PostJson = new LoanDocumentPostDc
                                {
                                    base64pdfencodedfile = DocBase64String,
                                    borrower_id = item.borrower_id,
                                    loan_app_id = item.loan_app_id,
                                    partner_loan_app_id = item.partner_loan_app_id,
                                    code = sCode,
                                    partner_borrower_id = item.partner_borrower_id,
                                    PdfPassword = item.PdfPassword
                                };
                                ArthMateHelper ArthMateHelper = new ArthMateHelper();
                                res = await ArthMateHelper.LoanDocumentApi(PostJson, item.Id, IsBankStatement);

                                if (res.uploadDocumentData != null)
                                {
                                    if (res.uploadDocumentData.document_id > 0)
                                    {
                                        commonResponseDc.Status = true;
                                        commonResponseDc.Msg = "Loan document uploaded successfully";
                                        uploadMsg = commonResponseDc.Msg;
                                        string query = "Update LeadLoanDocuments set RequestId =" + res.RequestId + ",ResponseId=" + res.ReponseId + ", ModifiedBy =1, ModifiedDate='" + DateTime.Now.ToString("yyyy/MM/dd") + "' Where LeadMasterId=" + item.Id + " and DocumentMasterId=" + item.DocumentMasterId + " and IsActive=1 and IsDeleted=0 ";
                                        int update = db.Database.ExecuteSqlCommand(query);

                                        LeadActivityProgressesHistory(LeadMasterID, 0, 0, "Document Upload", "Document (" + DocName + ") Upload Successfully!!");
                                    }

                                }
                            }
                        }
                        else
                        {
                            res.message = "Data Not Found in Lead!";
                        }
                    }
                    //regenerate 
                    //Task<CommonResponseDc> OfferRegenerate(int LeadId, int tenure, double sactionAmount)


                    //if (uploadMsg == "Loan document uploaded successfully")
                    //{
                    //    var leadloanTenure = db.LeadLoan.Where(x => x.LeadMasterId == LeadMasterID).FirstOrDefault();
                    //    CommonResponseDc returndata = await OfferRegenerate(Convert.ToInt32(LeadMasterID), Convert.ToInt32(leadloanTenure.tenure),Convert.ToInt32(leadloanTenure.sanction_amount));
                    //}

                }
            }

            return commonResponseDc;
        }

        public string ConvertToBase64String(string FileUrl)
        {
            string DocBase64String = "";
            TextFileLogHelper.LogError("FileUrl in ConvertToBase64String method " + FileUrl);

            //string baseBath = HttpRuntime.AppDomainAppPath;
            TextFileLogHelper.TraceLog("LOAN DOCUMENT API file path=" + string.Concat(HttpRuntime.AppDomainAppPath, FileUrl.Replace("/", "\\")));
            var fileurl = string.Concat(HttpRuntime.AppDomainAppPath, FileUrl.Replace("/", "\\"));
            FileUrl = fileurl;

            string extension = Path.GetExtension(FileUrl);
            string ext = extension.Trim().ToLower();
            if (ext == ".jpeg" || ext == ".png" || ext == ".jpg")
            {
                TextFileLogHelper.LogError("extension checked of file " + ext);
                string folderPath = HttpContext.Current.Server.MapPath("~/ArthmateDocument/OtherDoc");
                string filename = "ArthmateDoc_" + DateTime.Now.ToString("ddMMyyyyHHmmss") + ".pdf";
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }
                string path = Path.Combine(folderPath, filename);

                TextFileLogHelper.LogError("path where empty pdf will generate=> " + path);

                iTextSharp.text.Rectangle pageSize = null;

                using (var srcImage = new Bitmap(FileUrl))
                {
                    pageSize = new iTextSharp.text.Rectangle(0, 0, srcImage.Width, srcImage.Height);
                }
                using (var stream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    var document = new iTextSharp.text.Document(pageSize, 0, 0, 0, 0);
                    PdfWriter.GetInstance(document, stream);

                    document.Open();
                    TextFileLogHelper.LogError("uat path where read write operation will occur=> " + FileUrl);
                    using (var imageStream = new FileStream(FileUrl, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    {
                        var image = iTextSharp.text.Image.GetInstance(imageStream);
                        document.Add(image);
                    }
                    document.Close();
                    byte[] bytes = File.ReadAllBytes(stream.Name);
                    DocBase64String = Convert.ToBase64String(bytes);
                }
            }
            else
            {
                byte[] bytes = File.ReadAllBytes(FileUrl);
                DocBase64String = Convert.ToBase64String(bytes);
            }
            return DocBase64String;
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("UploadKYC")]
        // PAN,Aadhar,Selfie
        public IHttpActionResult UploadKYCImage()
        {
            string LogoUrl = "";
            try
            {
                if (HttpContext.Current.Request.Files.AllKeys.Any())
                {
                    var httpPostedFile = HttpContext.Current.Request.Files["file"];
                    if (httpPostedFile != null)
                    {
                        if (!Directory.Exists(System.Web.HttpContext.Current.Server.MapPath("~/ArthmateDocument/KYC")))
                            Directory.CreateDirectory(HttpContext.Current.Server.MapPath("~/ArthmateDocument/KYC"));

                        string extension = Path.GetExtension(httpPostedFile.FileName);
                        string fileName = httpPostedFile.FileName.Substring(0, httpPostedFile.FileName.LastIndexOf('.')) + DateTime.Now.ToString("ddMMyyyyHHmmss") + extension;
                        LogoUrl = Path.Combine(HttpContext.Current.Server.MapPath("~/ArthmateDocument/KYC"), fileName);
                        httpPostedFile.SaveAs(LogoUrl);
                        AngularJSAuthentication.Common.Helpers.FileUploadHelper.Upload(fileName, "~/ArthmateDocument/KYC", LogoUrl);

                        LogoUrl = string.Format("{0}://{1}{2}/{3}", new Uri((HttpContext.Current.Request.UrlReferrer != null ? HttpContext.Current.Request.UrlReferrer.AbsoluteUri : HttpContext.Current.Request.Url.AbsoluteUri)).Scheme
                                                                 , HttpContext.Current.Request.Url.DnsSafeHost
                                                                 , (HttpContext.Current.Request.Url.Port != 80 && HttpContext.Current.Request.Url.Port != 443 ? ":" + HttpContext.Current.Request.Url.Port : "")
                                                                 , "/ArthmateDocument/KYC/" + fileName);
                        LogoUrl = "/ArthmateDocument/KYC/" + fileName;
                    }

                }
                return Created<string>(LogoUrl, LogoUrl);
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("UploadStatement")]
        // GST,Bank statement
        public IHttpActionResult UploadStatementImage()
        {
            string LogoUrl = "";
            try
            {
                if (HttpContext.Current.Request.Files.AllKeys.Any())
                {
                    var httpPostedFile = HttpContext.Current.Request.Files["file"];
                    if (httpPostedFile != null)
                    {
                        if (!Directory.Exists(System.Web.HttpContext.Current.Server.MapPath("~/ArthmateDocument/Statement")))
                            Directory.CreateDirectory(HttpContext.Current.Server.MapPath("~/ArthmateDocument/Statement"));

                        string extension = Path.GetExtension(httpPostedFile.FileName);
                        string fileName = httpPostedFile.FileName.Substring(0, httpPostedFile.FileName.LastIndexOf('.')) + DateTime.Now.ToString("ddMMyyyyHHmmss") + extension;
                        LogoUrl = Path.Combine(HttpContext.Current.Server.MapPath("~/ArthmateDocument/Statement"), fileName);

                        httpPostedFile.SaveAs(LogoUrl);
                        AngularJSAuthentication.Common.Helpers.FileUploadHelper.Upload(fileName, "~/ArthmateDocument/Statement", LogoUrl);

                        LogoUrl = string.Format("{0}://{1}{2}/{3}", new Uri((HttpContext.Current.Request.UrlReferrer != null ? HttpContext.Current.Request.UrlReferrer.AbsoluteUri : HttpContext.Current.Request.Url.AbsoluteUri)).Scheme
                                                                 , HttpContext.Current.Request.Url.DnsSafeHost
                                                                 , (HttpContext.Current.Request.Url.Port != 80 && HttpContext.Current.Request.Url.Port != 443 ? ":" + HttpContext.Current.Request.Url.Port : "")
                                                                 , "/ArthmateDocument/Statement/" + fileName);
                        LogoUrl = "/ArthmateDocument/Statement/" + fileName;

                    }

                }
                return Created<string>(LogoUrl, LogoUrl);
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("UploadOtherDoc")]
        public IHttpActionResult UploadOtherDoc()
        {
            string LogoUrl = "";
            try
            {
                if (HttpContext.Current.Request.Files.AllKeys.Any())
                {
                    var httpPostedFile = HttpContext.Current.Request.Files["file"];
                    if (httpPostedFile != null)
                    {
                        if (!Directory.Exists(System.Web.HttpContext.Current.Server.MapPath("~/ArthmateDocument/OtherDoc")))
                            Directory.CreateDirectory(HttpContext.Current.Server.MapPath("~/ArthmateDocument/OtherDoc"));

                        string extension = Path.GetExtension(httpPostedFile.FileName);
                        string fileName = httpPostedFile.FileName.Substring(0, httpPostedFile.FileName.LastIndexOf('.')) + DateTime.Now.ToString("ddMMyyyyHHmmss") + extension;
                        LogoUrl = Path.Combine(HttpContext.Current.Server.MapPath("~/ArthmateDocument/OtherDoc"), fileName);
                        httpPostedFile.SaveAs(LogoUrl);
                        AngularJSAuthentication.Common.Helpers.FileUploadHelper.Upload(fileName, "~/ArthmateDocument/OtherDoc", LogoUrl);

                        LogoUrl = string.Format("{0}://{1}{2}/{3}", new Uri((HttpContext.Current.Request.UrlReferrer != null ? HttpContext.Current.Request.UrlReferrer.AbsoluteUri : HttpContext.Current.Request.Url.AbsoluteUri)).Scheme
                                                                 , HttpContext.Current.Request.Url.DnsSafeHost
                                                                 , (HttpContext.Current.Request.Url.Port != 80 && HttpContext.Current.Request.Url.Port != 443 ? ":" + HttpContext.Current.Request.Url.Port : "")
                                                                 , "/ArthmateDocument/OtherDoc/" + fileName);

                        LogoUrl = "/ArthmateDocument/OtherDoc/" + fileName;
                    }

                }
                return Created<string>(LogoUrl, LogoUrl);
            }
            catch (Exception ex)
            {
                return null;
            }
        }


        //Get Lead Api
        [AllowAnonymous]
        [HttpGet]
        [Route("GetArthMateLeadDetail")]
        public async Task<GetLeadResponse> GetLeadApi(string loan_app_id)
        {
            GetLeadResponse res = new GetLeadResponse();
            var identity = User.Identity as ClaimsIdentity;
            int userid = 0;
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
            using (AuthContext db = new AuthContext())
            {
                if (loan_app_id != "" && !string.IsNullOrEmpty(loan_app_id))
                {
                    var data = db.LeadMasters.Where(x => x.loan_app_id == loan_app_id && x.IsActive == true && x.IsDeleted == false).FirstOrDefault();
                    if (data != null)
                    {
                        long leadid = data.Id;
                        ArthMateHelper ArthMateHelper = new ArthMateHelper();
                        res = await ArthMateHelper.GetLeadApi(loan_app_id, userid, leadid);
                    }
                    else
                    {
                        res.message = "Lead Not Found";
                    }
                }
            }
            return res;
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("GetLeadCurrentActivity")]
        public async Task<GetLeadCurrentActivityDc> GetLeadCurrentActivity(long LeadMasterId)
        {
            GetLeadCurrentActivityDc res = new GetLeadCurrentActivityDc();
            var identity = User.Identity as ClaimsIdentity;
            int userid = 0;
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
            using (AuthContext db = new AuthContext())
            {
                if (LeadMasterId > 0)
                {
                    //var loanid = new SqlParameter("@partner_loan_app_id", partnerLoanAppid);
                    var loanid = new SqlParameter("@LeadMasterId", LeadMasterId);

                    res = db.Database.SqlQuery<GetLeadCurrentActivityDc>("EXEC [Arthmate].[LeadCurrentActivity] @LeadMasterId", loanid).FirstOrDefault();
                }
            }
            return res;
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



        [AllowAnonymous]
        [HttpGet]
        [Route("GetLoanByLoanId")]
        public async Task<CommonResponseDc> GetLoanByLoanId(long Leadmasterid)
        {
            CommonResponseDc res = new CommonResponseDc();
            using (AuthContext db = new AuthContext())
            {
                var data = db.LeadLoan.Where(x => x.LeadMasterId == Leadmasterid && x.IsActive == true && x.IsDeleted == false).FirstOrDefault();

                if (data != null)
                {
                    ArthMateHelper ArthMateHelper = new ArthMateHelper();
                    var getloandata = await ArthMateHelper.GetLoanById(data.loan_id, Leadmasterid, 0);
                    res.Data = getloandata;
                    res.Msg = getloandata.message;

                    if (data.status != getloandata.loanDetails.status)
                    {
                        //if(data.status != LoanStatus)
                        data.status = getloandata.loanDetails.status;
                        db.Entry(data).State = EntityState.Modified;
                        db.Commit();
                    }
                }
                else
                {
                    // res.message = "Lead Not Found";
                }

            }
            return res;
        }
        // 

        [AllowAnonymous]
        [HttpGet]
        [Route("ArthmateChangesLoanAmount")] //arthmate changes loan amount ..to update at our side
        public async Task<CommonResponseDc> ArthmateChangesLoanAmount(long Leadmasterid)
        {
            CommonResponseDc res = new CommonResponseDc();
            using (AuthContext db = new AuthContext())
            {
                var LeadLoan = db.LeadLoan.Where(x => x.LeadMasterId == Leadmasterid && x.IsActive == true && x.IsDeleted == false).FirstOrDefault();

                if (LeadLoan != null)
                {
                    ArthMateHelper ArthMateHelper = new ArthMateHelper();
                    var loandatabyId = await ArthMateHelper.GetLoanById(LeadLoan.loan_id, Leadmasterid, 0);
                    //res.Data = getloandata;
                    //res.Msg = getloandata.message;

                    //if (data.status != getloandata.loanDetails.status)
                    //{
                    //    //if(data.status != LoanStatus)
                    //    data.status = getloandata.loanDetails.status;
                    //    db.Entry(data).State = EntityState.Modified;
                    //    db.Commit();
                    //}





                }
                else
                {
                    // res.message = "Lead Not Found";
                }

            }
            return res;
        }



        //public string GetLoanByLoanIdCallApi(long Leadmasterid)
        //{
        //    var statusData=await GetLoanByLoanId(Leadmasterid);
        //    return statusData.Status;
        //}

        [HttpGet]
        [Route("RepaymentSchedule")]
        public async Task<RepaymentScheduleResponseDc> RepaymentSchedule(RepaymentScheduleDc repayment)
        {
            ArthMateHelper ArthMateHelper = new ArthMateHelper();
            var res = await ArthMateHelper.RepaymentSchedule(repayment);
            return res;
        }

        #region Page View data API's
        [AllowAnonymous]
        [HttpGet]
        [Route("LeadPageData")]
        public async Task<List<LeadPagedata>> LeadPageData(string keyword, DateTime? stardate, DateTime? enddate, int skip, int take)
        {
            List<LeadPagedata> res = new List<LeadPagedata>();
            using (var myContext = new AuthContext())
            {
                if (keyword == null)
                    keyword = "";
                var key = new SqlParameter("@keyword", keyword == null ? DBNull.Value : (object)keyword);
                var Sdate = new SqlParameter("@stardate", stardate == null ? DBNull.Value : (object)stardate);
                var Edate = new SqlParameter("@enddate", enddate == null ? DBNull.Value : (object)enddate);
                var Skip = new SqlParameter("@skip", skip);
                var Take = new SqlParameter("@take", take);

                res = await myContext.Database.SqlQuery<LeadPagedata>("EXEC Arthmate.GetleadPageData @keyword,@stardate,@enddate,@skip,@take", key, Sdate, Edate, Skip, Take).ToListAsync();
            }
            return res;

        }
        //new data for Export
        [AllowAnonymous]
        [HttpGet]
        [Route("LeadPageDataExport")]
        public async Task<List<LeadPagedataExport>> LeadPageDataExport(string keyword, DateTime? stardate, DateTime? enddate)
        {
            List<LeadPagedataExport> res = new List<LeadPagedataExport>();
            using (var myContext = new AuthContext())
            {
                if (keyword == null)
                    keyword = "";
                var key = new SqlParameter("@keyword", keyword == null ? DBNull.Value : (object)keyword);
                var Sdate = new SqlParameter("@stardate", stardate == null ? DBNull.Value : (object)stardate);
                var Edate = new SqlParameter("@enddate", enddate == null ? DBNull.Value : (object)enddate);
                //var Skip = new SqlParameter("@skip", skip);
                //var Take = new SqlParameter("@take", take);

                res = await myContext.Database.SqlQuery<LeadPagedataExport>("EXEC Arthmate.GetleadPageDataExport @keyword,@stardate,@enddate", key, Sdate, Edate).ToListAsync();
            }
            return res;

        }

        [AllowAnonymous]
        [HttpGet]
        [Route("GetCustomerPageData")]
        public async Task<LeadPagedata> GetCustomerPageData(int LeadId)
        {
            LeadPagedata res = new LeadPagedata();
            using (var myContext = new AuthContext())
            {
                var leadId = new SqlParameter("@LeadId", LeadId);

                res = await myContext.Database.SqlQuery<LeadPagedata>("EXEC Arthmate.GetCustomerPageData @LeadId", leadId).FirstOrDefaultAsync();
                if (res.SequenceNo > 6 && res.SequenceNo != 14)
                {

                    switch (res.SequenceNo)
                    {
                        case 7:
                            res.Msg = "Data ready for lead generate.";
                            res.ButtonActive = 0;
                            if (!string.IsNullOrEmpty(res.loan_app_id))
                            {
                                if (!myContext.AScore.Any(x => x.LeadMasterId == LeadId && x.IsActive == true && x.IsDeleted == false))
                                {
                                    res.Msg = "Data ready for A-Score.";
                                    res.ButtonActive = 1;
                                }
                                else if (!myContext.CeplrPdfReports.Any(x => x.LeadMasterId == LeadId && x.IsActive == true && x.IsDeleted == false))
                                {
                                    res.Msg = "Data ready for Cepler & offer generate.";
                                    res.ButtonActive = 2;
                                }
                                else
                                {
                                    res.Msg = "Data ready for offer generate.";
                                    res.ButtonActive = 3;
                                }
                            }
                            break;
                        case 8:
                            res.Msg = "Data ready for offer generate.";
                            res.ButtonActive = 3;
                            break;
                        case 9:
                            res.Msg = "Offer generated and Data ready for Loan.";
                            res.ButtonActive = 4;
                            break;
                        default:
                            res.Msg = string.Empty;
                            res.ButtonActive = -1;
                            break;
                    }
                }
                else
                {
                    res.Msg = string.Empty;
                    res.ButtonActive = -1;
                }

            }
            return res;

        }

        [AllowAnonymous]
        [HttpGet]
        [Route("GetNoActivityDocList")]
        public async Task<List<GetNoActivityDocDc>> GetNoActivityDocList(long LeadMasterId)
        {
            List<GetNoActivityDocDc> res = new List<GetNoActivityDocDc>();
            var identity = User.Identity as ClaimsIdentity;
            int userid = 0;
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
            using (AuthContext db = new AuthContext())
            {
                if (LeadMasterId > 0)
                {
                    var leadMasterId = new SqlParameter("@LeadMasterId", LeadMasterId);

                    res = db.Database.SqlQuery<GetNoActivityDocDc>("EXEC [Arthmate].[GetNoActivityDocList] @LeadMasterId", leadMasterId).ToList();
                }
            }
            return res;
        }


        [AllowAnonymous]
        [HttpGet]
        [Route("LeadDocumentsByLeadId")]
        public async Task<List<LeadPageDocuments>> PageLeadDocuments(long leadmasterid)
        {
            List<LeadPageDocuments> res = new List<LeadPageDocuments>();
            using (var myContext = new AuthContext())
            {
                var leadid = new SqlParameter("@leadmasterid", leadmasterid);
                res = await myContext.Database.SqlQuery<LeadPageDocuments>("EXEC ArthmateLeadDocuments @leadmasterid", leadid).ToListAsync();
            }
            return res;

        }


        [AllowAnonymous]
        [HttpGet]
        [Route("GetLeadDetailsData")]
        public async Task<GetLeadDetailsDc> GetLeadDetailsData(long leadid)
        {
            List<GetLeadDetailsDc> data = new List<GetLeadDetailsDc>();

            GetLeadDetailsDc getLeadDetailsDc = new GetLeadDetailsDc();

            List<CustomerPersonalSDetailsDc> customerPersonalSDetailsDc = new List<CustomerPersonalSDetailsDc>();
            List<BankDetailsDc> BankDetailsDc = new List<BankDetailsDc>();
            List<BusinessDetailDc> businessdata = new List<BusinessDetailDc>();
            List<PanDetailsDc> pandata = new List<PanDetailsDc>();
            List<SelfieDetailsDc> selfieData = new List<SelfieDetailsDc>();
            List<AadharDetailDc> Aadhardata = new List<AadharDetailDc>();
            List<EagreementDc> Eagreementdata = new List<EagreementDc>();
            List<AadharDetailDc> Msmedata = new List<AadharDetailDc>();
            List<LeadActivityProgressesHistoriesDc> LeadActivityProgressesHist = new List<LeadActivityProgressesHistoriesDc>();
            List<BankStatementDc> BankStatementlist = new List<BankStatementDc>();


            using (var context = new AuthContext())
            {
                if (context.Database.Connection.State != ConnectionState.Open)
                    context.Database.Connection.Open();

                var LeadId = new SqlParameter("@leadmasterid", leadid);
                var cmd = context.Database.Connection.CreateCommand();
                cmd.CommandText = "[Arthmate].[getLeadDetails]";
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.Parameters.Add(LeadId);
                var reader = cmd.ExecuteReader();

                customerPersonalSDetailsDc = ((IObjectContextAdapter)context).ObjectContext.Translate<CustomerPersonalSDetailsDc>(reader).ToList();
                if (customerPersonalSDetailsDc != null)
                    customerPersonalSDetailsDc.ForEach(x => x.DocumentName = "PersonalDetail");
                reader.NextResult();

                BankDetailsDc = ((IObjectContextAdapter)context).ObjectContext.Translate<BankDetailsDc>(reader).ToList();

                reader.NextResult();

                businessdata = ((IObjectContextAdapter)context).ObjectContext.Translate<BusinessDetailDc>(reader).ToList();

                //AAdhar data 
                reader.NextResult();
                Aadhardata = ((IObjectContextAdapter)context).ObjectContext.Translate<AadharDetailDc>(reader).ToList();


                //PAN DATA
                reader.NextResult();
                pandata = ((IObjectContextAdapter)context).ObjectContext.Translate<PanDetailsDc>(reader).ToList();

                //Selfie DATA
                reader.NextResult();
                selfieData = ((IObjectContextAdapter)context).ObjectContext.Translate<SelfieDetailsDc>(reader).ToList();

                //Eagrement  data 
                reader.NextResult();
                Eagreementdata = ((IObjectContextAdapter)context).ObjectContext.Translate<EagreementDc>(reader).ToList();

                //MSME Data 
                reader.NextResult();
                Msmedata = ((IObjectContextAdapter)context).ObjectContext.Translate<AadharDetailDc>(reader).ToList();

                reader.NextResult();
                LeadActivityProgressesHist = ((IObjectContextAdapter)context).ObjectContext.Translate<LeadActivityProgressesHistoriesDc>(reader).ToList();

                reader.NextResult();
                BankStatementlist = ((IObjectContextAdapter)context).ObjectContext.Translate<BankStatementDc>(reader).ToList();

                getLeadDetailsDc.customerPersonalSDetailsDc = customerPersonalSDetailsDc;
                getLeadDetailsDc.bankDetailsDc = BankDetailsDc;
                getLeadDetailsDc.businessDetailDc = businessdata;
                getLeadDetailsDc.panDetailsDc = pandata;
                getLeadDetailsDc.selfieDetailsDcs = selfieData;
                getLeadDetailsDc.AadharDetailDcs = Aadhardata;
                getLeadDetailsDc.eagreementDcs = Eagreementdata;
                getLeadDetailsDc.MsMeDataDcs = Msmedata;
                getLeadDetailsDc.BankStatementDcs = BankStatementlist;

                getLeadDetailsDc.LeadActivityProgressesHistDcs = LeadActivityProgressesHist;
            }
            return getLeadDetailsDc;

        }

        #endregion

        [AllowAnonymous]
        [HttpPost]
        [Route("MSMERegVerification")]
        public async Task<CommonResponseDc> MSMERegVerification(MSMERegVerificationDc msmeReg)
        {
            CommonResponseDc res = new CommonResponseDc();

            using (AuthContext db = new AuthContext())
            {
                if (msmeReg != null)
                {
                    var lead = db.LeadMasters.Where(x => x.Id == msmeReg.LeadMasterId && x.IsActive == true && x.IsDeleted == false).FirstOrDefault();

                    if (lead != null)
                    {
                        var documentId = db.ArthMateDocumentMasters.FirstOrDefault(x => x.DocumentName == "udyam_reg_cert").Id;
                        var leadDoc = db.LeadDocument.Where(x => x.LeadMasterId == msmeReg.LeadMasterId && x.DocumentMasterId == documentId && x.IsActive == true && x.IsDeleted == false).FirstOrDefault();
                        if (leadDoc == null)
                        {
                            LeadLoanDocument leadLoanDocument = new LeadLoanDocument();
                            leadLoanDocument.DocumentMasterId = documentId;
                            leadLoanDocument.FrontFileUrl = msmeReg.MSMECertificate;
                            leadLoanDocument.CreatedBy = (int)lead.Id;
                            leadLoanDocument.CreatedDate = DateTime.Now;
                            leadLoanDocument.DocumentNumber = msmeReg.MSMERegNum;
                            leadLoanDocument.IsActive = true;
                            leadLoanDocument.IsDeleted = false;
                            leadLoanDocument.IsVerified = false;
                            leadLoanDocument.LeadMasterId = lead.Id;
                            db.LeadDocument.Add(leadLoanDocument);
                        }
                        else
                        {
                            leadDoc.DocumentMasterId = documentId;
                            leadDoc.FrontFileUrl = msmeReg.MSMECertificate;
                            leadDoc.ModifiedDate = DateTime.Now;
                            leadDoc.ModifiedBy = (int)msmeReg.LeadMasterId;
                            leadDoc.DocumentNumber = msmeReg.MSMERegNum;
                            db.Entry(leadDoc).State = EntityState.Modified;
                        }
                        lead.udyam_reg_no = msmeReg.MSMERegNum;
                        lead.Msme_Bus_Name = msmeReg.BusinessName;
                        lead.Msme_Bus_Type = msmeReg.BusinessType;
                        lead.Msme_VintageDays = msmeReg.Vintage;

                        lead.ModifiedDate = DateTime.Now;

                        var leadid = new SqlParameter("@LeadMasterId", lead.Id);
                        var seqno = db.Database.SqlQuery<LeadSequenceData>("EXEC [Arthmate].[LeadNextSequenceno] @LeadMasterId", leadid).FirstOrDefault();
                        if (seqno.IsApproved == 0)//&& seqno.IsComplete == true)
                        {
                            lead.SequenceNo = msmeReg.SequenceNo.Value;
                        }


                        //await UpdateLeadCurrentActivity(lead.Id, db, lead.SequenceNo);
                        await UpdateLeadCurrentActivity(lead.Id, db, msmeReg.SequenceNo.Value);
                        db.Entry(lead).State = EntityState.Modified;
                        db.Commit();
                        res.Msg = "MSME Upload Successfully";
                        res.Status = true;

                        LeadActivityProgressesHistory(lead.Id, msmeReg.SequenceNo.Value, 0, "", "MSME Upload Successfully");
                    }
                }
            }
            return res;
        }

        [HttpGet]
        [Route("CeplrBankList")]
        public async Task<CommonResponseDc> CeplrBankList()
        {
            CommonResponseDc res = new CommonResponseDc();
            using (var context = new AuthContext())
            {
                List<CeplrBankList> banklist = new List<CeplrBankList>();
                ArthMateHelper ArthMateHelper = new ArthMateHelper();
                var result = await ArthMateHelper.CeplrBankList();
                if (result != null && result.data.Count > 0)
                {
                    string query = "Update CeplrBankLists set IsActive=0, IsDeleted=1,ModifiedBy=1, ModifiedDate='" + DateTime.Now.ToString("yyyy/MM/dd") + "' Where IsActive=1 and IsDeleted=0 ";
                    int i = context.Database.ExecuteSqlCommand(query);

                    foreach (var item in result.data)
                    {
                        CeplrBankList obj = new CeplrBankList();
                        obj.aa_fip_id = item.aa_fip_id;
                        obj.fip_name = item.fip_name;
                        obj.pdf_fip_id = item.pdf_fip_id;
                        obj.enable = item.enable;
                        obj.fip_logo_uri = item.fip_logo_uri;
                        obj.IsActive = true;
                        obj.IsDeleted = false;
                        obj.CreatedDate = DateTime.Now;
                        obj.ModifiedDate = null;
                        obj.CreatedBy = 0;
                        obj.ModifiedBy = null;
                        banklist.Add(obj);
                    }
                    context.CeplrBankList.AddRange(banklist);
                    context.Commit();
                    res.Status = true;
                }
                return res;
            }
        }

        [HttpGet]
        [Route("GetCeplrBankList")]
        public async Task<CommonResponseDc> GetCeplrBankList()
        {
            CommonResponseDc res = new CommonResponseDc();
            using (var db = new AuthContext())
            {
                var banklist = db.CeplrBankList.ToList();
                if (banklist != null)
                {
                    res.Data = banklist;
                    res.Status = true;
                }
                else
                {
                    res.Data = null;
                    res.Status = false;
                }
                return res;
            }
        }


        //Ceplr Basic Report
        [HttpPost]
        [Route("CeplrBasicReport")]
        public async Task<CommonResponseDc> CeplrBasicReport(long LeadmasterId)
        {
            CommonResponseDc res = new CommonResponseDc();
            //CeplrBasicReportDc BasicReportDc = new CeplrBasicReportDc();
            using (AuthContext db = new AuthContext())
            {
                var ceplrData = db.CeplrPdfReports.Where(x => x.LeadMasterId == LeadmasterId).FirstOrDefault();
                if (ceplrData != null)
                {
                    CeplrBasicReportDc CplrBasic = new CeplrBasicReportDc();
                    CplrBasic.start_date = ceplrData.CreatedDate.AddMonths(-6).AddDays(-10).ToString("yyyy-MM-dd");
                    CplrBasic.end_date = DateTime.Now.ToString("yyyy-MM-dd");

                    ArthMateHelper ArthMateHelper = new ArthMateHelper();
                    var BasicReport = await ArthMateHelper.CeplrBasicReport(CplrBasic, ceplrData.customer_id, LeadmasterId);
                    if (BasicReport != null)
                    {
                        res.Data = BasicReport;
                        res.Msg = "Reports Fetched Successfully.";
                        res.Status = true;
                    }
                    else
                    {
                        res.Data = BasicReport;
                        res.Msg = "Something went Wrong!";
                        res.Status = false;
                    }
                    res.Data = res.Data;
                }
            }
            return res;
        }

        //ducument verification
        [HttpPost]
        [Route("VerifyUploadedDocument")]
        public async Task<CommonResponseDc> VerifyUploadedDocument(ScreenApproveRejectDc obj)
        {
            CommonResponseDc res = new CommonResponseDc();

            var identity = User.Identity as ClaimsIdentity;
            int userid = 0;
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

            using (var db = new AuthContext())
            {
                long leaddicumentid = (from k in db.LeadDocument
                                       join adm in db.ArthMateDocumentMasters on k.DocumentMasterId equals adm.Id
                                       where adm.DocumentName == obj.DocumentName && k.LeadMasterId == obj.LeadmasterId && k.IsActive == true && k.IsDeleted == false
                                       select k.Id).FirstOrDefault();
                if (leaddicumentid == 0 && (obj.DocumentName == "pan_card" || obj.DocumentName == "aadhar_card" || obj.DocumentName == "udyam_reg_cert" || obj.DocumentName == "bank_stmnts" || obj.DocumentName == "selfie"))
                {
                    res.Msg = obj.DocumentName + " Upload Document First!!";
                    res.Status = false;

                }
                else
                {
                    var lead = db.LeadDocument.Where(x => x.Id == leaddicumentid).FirstOrDefault();
                    var leadMaster = db.LeadMasters.Where(x => x.Id == obj.LeadmasterId).FirstOrDefault();
                    //long SequenceNo = db.Database.SqlQuery<long>("[Arthmate].[getCurrentSequence] @DocumentName", DocumentName).FirstOrDefault();

                    //var docname = new SqlParameter("@DocumentName", DocumentName);
                    //int SequenceNo = db.Database.SqlQuery<int>("[Arthmate].[getCurrentSequence] @DocumentName", docname).FirstOrDefault();
                    res.Msg = "Not Verified!";
                    res.Status = false;
                    if (lead != null)
                    {
                        if (obj.isApprove == true)
                        {
                            lead.IsVerified = true;
                            lead.IsRejected = false;
                            lead.IsPostedError = false;
                            lead.PostedErrorMsg = obj.DocumentName + " " + "Is Verified";
                            lead.ModifiedDate = DateTime.Now;
                        }
                        else
                        {
                            lead.IsVerified = false;
                            lead.IsRejected = true;
                            lead.IsPostedError = true;
                            lead.PostedErrorMsg = obj.DocumentName + " " + "Is Rejected";
                            lead.IsActive = false;
                            lead.IsDeleted = true;
                            lead.ModifiedDate = DateTime.Now;


                            // leadMaster.SequenceNo = SequenceNo;
                            //var LeadId = new SqlParameter("@LeadMasterId", LeadmasterId);
                            //var ActivityId = new SqlParameter("@ArthmateActivitySequenceNo", SequenceNo);
                            //var res2 = db.Database.SqlQuery<string>("[Arthmate].[UpdateLeadCurrentActivity]  @LeadMasterId,@ArthmateActivitySequenceNo", LeadId, ActivityId).FirstOrDefault();
                        }
                        db.Entry(lead).State = EntityState.Modified;

                        if (obj.DocumentName == "bank_stmnts")
                        {
                            var statements = db.LeadBankStatement.Where(x => x.IsActive == true && x.IsDeleted == false && x.LeadMasterId == obj.LeadmasterId).ToList();
                            if (statements != null && statements.Count > 0)
                            {
                                foreach (var item in statements)
                                {
                                    if (obj.isApprove == true)
                                    {
                                        item.IsActive = true;
                                        item.IsDeleted = false;
                                    }
                                    else
                                    {
                                        item.IsActive = false;
                                        item.IsDeleted = true;
                                    }
                                    item.ModifiedBy = userid;
                                    item.ModifiedDate = DateTime.Now;
                                    db.Entry(item).State = EntityState.Modified;
                                }
                            }
                        }

                        //db.Entry(leadMaster).State = EntityState.Modified;
                        if (db.Commit() > 0)
                        {
                            res.Msg = obj.DocumentName + " Is " + (obj.isApprove ? "Approved Successfully" : "Rejected Successfully");
                            res.Status = true;

                            LeadActivityProgressesHistory(obj.LeadmasterId, 0, 0, "Verify Lead Document", "Lead Document: " + obj.DocumentName + " Is " + (obj.isApprove ? "Approved Successfully" : "Rejected Successfully"));
                        }
                    }

                    var sequenceData = db.ArthMateActivitySequence.FirstOrDefault(x => x.SequenceNo == obj.SequenceNo);
                    if (sequenceData != null)
                    {
                        var leadActivity = db.LeadActivityMasterProgress.FirstOrDefault(x => x.LeadMasterId == obj.LeadmasterId && x.ActivityMasterId == sequenceData.Id && x.IsActive);

                        if (leadActivity != null)
                        {
                            leadActivity.IsApprove = obj.isApprove ? 1 : 2;
                            leadActivity.Comment = obj.Comment;
                            leadActivity.ModifiedDate = DateTime.Now; ;
                            //leadActivity.IsComplete = false;
                            if (!obj.isApprove)
                            {
                                var l_app_id = "";
                                var p_bar_id = "";
                                var id = 1;
                                List<int> reprocessSequence = new List<int> { 1, 3, 4, 5 };
                                if (reprocessSequence.Any(x => x == sequenceData.SequenceNo))
                                {
                                    l_app_id = "";
                                    p_bar_id = "";
                                    id = 1;
                                    if (!string.IsNullOrEmpty(leadMaster.loan_app_id))
                                    {
                                        if (leadMaster.partner_loan_app_id.Contains("@"))
                                        {
                                            var appidarry = leadMaster.partner_loan_app_id.Split('@');
                                            l_app_id = appidarry[0];
                                            id = Convert.ToInt32(appidarry[1]);
                                            l_app_id = l_app_id + "@" + (id + 1).ToString();
                                            p_bar_id = leadMaster.SkCode + "0000" + (id + 1).ToString();
                                        }
                                        else
                                        {
                                            l_app_id = l_app_id + "@" + (id).ToString();
                                            p_bar_id = leadMaster.SkCode + "0000" + (id + 1).ToString();
                                        }
                                        leadMaster.partner_loan_app_id = l_app_id;
                                        leadMaster.partner_borrower_id = p_bar_id;
                                        leadMaster.loan_app_id = "";
                                        leadMaster.borrower_id = "";
                                        var ascore = db.AScore.FirstOrDefault(x => x.LeadMasterId == leadMaster.Id && x.IsActive);
                                        if (ascore != null)
                                        {
                                            ascore.IsActive = false;
                                            ascore.IsDeleted = true;
                                            ascore.ModifiedDate = DateTime.Now;
                                            db.Entry(ascore).State = EntityState.Modified;
                                        }
                                        var Ceplr = db.CeplrPdfReports.FirstOrDefault(x => x.LeadMasterId == leadMaster.Id && x.IsActive);
                                        if (Ceplr != null)
                                        {
                                            Ceplr.IsActive = false;
                                            Ceplr.IsDeleted = true;
                                            Ceplr.ModifiedDate = DateTime.Now;
                                            db.Entry(Ceplr).State = EntityState.Modified;
                                        }
                                        db.Entry(leadMaster).State = EntityState.Modified;
                                    }
                                }
                                else if (sequenceData.SequenceNo == 6 || sequenceData.SequenceNo == 7)
                                {
                                    var ascore = db.AScore.FirstOrDefault(x => x.LeadMasterId == leadMaster.Id && x.IsActive);
                                    if (ascore != null)
                                    {
                                        ascore.IsActive = false;
                                        ascore.IsDeleted = true;
                                        ascore.ModifiedDate = DateTime.Now;
                                        db.Entry(ascore).State = EntityState.Modified;
                                    }
                                    var Ceplr = db.CeplrPdfReports.FirstOrDefault(x => x.LeadMasterId == leadMaster.Id && x.IsActive);
                                    if (Ceplr != null)
                                    {
                                        Ceplr.IsActive = false;
                                        Ceplr.IsDeleted = true;
                                        Ceplr.ModifiedDate = DateTime.Now;
                                        db.Entry(Ceplr).State = EntityState.Modified;
                                    }
                                    db.Entry(leadMaster).State = EntityState.Modified;

                                    var CoLenderResponse = db.CoLenderResponse.FirstOrDefault(x => x.LeadMasterId == leadMaster.Id && x.IsActive);
                                    if (CoLenderResponse != null)
                                    {
                                        CoLenderResponse.IsActive = false;
                                        CoLenderResponse.IsDeleted = true;
                                        CoLenderResponse.ModifiedDate = DateTime.Now;
                                        db.Entry(CoLenderResponse).State = EntityState.Modified;
                                    }
                                }
                            }
                            db.Entry(leadActivity).State = EntityState.Modified;
                            if (db.Commit() > 0)
                            {
                                res.Msg = obj.DocumentName + " Is " + (obj.isApprove ? "Approved Successfully" : "Rejected Successfully");
                                res.Status = true;

                                LeadActivityProgressesHistory(obj.LeadmasterId, 0, 0, "Verify AScore/CeplrPdfReports Document", " AScore/CeplrPdfReports Document:" + obj.DocumentName + " Is " + (obj.isApprove ? "Approved Successfully" : "Rejected Successfully"));
                            }
                        }
                    }
                }
            }
            return res;

        }

        [HttpGet]
        [Route("OfferRegenerate")]
        public async Task<CommonResponseDc> OfferRegenerate(int LeadId, int tenure, double sactionAmount)
        {
            CommonResponseDc res = new CommonResponseDc();
            res.Msg = "Not Verified";
            res.Status = false;
            using (AuthContext db = new AuthContext())
            {
                int iSequenceNo = 7;
                var Leadmaster = db.LeadMasters.Where(x => x.Id == LeadId).FirstOrDefault();

                var leadActivities = (from c in db.LeadActivityMasterProgress.Where(x => x.LeadMasterId == LeadId && x.IsActive)
                                      join x in db.ArthMateActivitySequence
                                      on c.ActivityMasterId equals x.Id
                                      where x.SequenceNo > 8
                                      select c).ToList();

                if (leadActivities != null)
                {
                    foreach (var leadActivity in leadActivities)
                    {
                        leadActivity.IsApprove = 2;
                        leadActivity.Comment = "Due to tenure change";
                        leadActivity.ModifiedDate = DateTime.Now;
                        db.Entry(leadActivity).State = EntityState.Modified;
                    }
                    //var ascore = db.AScore.FirstOrDefault(x => x.LeadMasterId == LeadId && x.IsActive);
                    //if (ascore != null)
                    //{
                    //    ascore.IsActive = false;
                    //    ascore.IsDeleted = true;
                    //    ascore.ModifiedDate = DateTime.Now;
                    //    db.Entry(ascore).State = EntityState.Modified;
                    //}
                    //var Ceplr = db.CeplrPdfReports.FirstOrDefault(x => x.LeadMasterId == LeadId && x.IsActive);
                    //if (Ceplr != null)
                    //{
                    //    Ceplr.IsActive = false;
                    //    Ceplr.IsDeleted = true;
                    //    Ceplr.ModifiedDate = DateTime.Now;
                    //    db.Entry(Ceplr).State = EntityState.Modified;
                    //}
                    var CoLenderResponse = db.CoLenderResponse.FirstOrDefault(x => x.LeadMasterId == LeadId && x.IsActive);
                    if (CoLenderResponse != null)
                    {
                        CoLenderResponse.IsActive = false;
                        CoLenderResponse.IsDeleted = true;
                        CoLenderResponse.ModifiedDate = DateTime.Now;
                        db.Entry(CoLenderResponse).State = EntityState.Modified;
                    }

                    Leadmaster.tenure = tenure;
                    Leadmaster.ModifiedDate = DateTime.Now;

                    Leadmaster.EnquiryAmount = sactionAmount;

                    //await UpdateLeadCurrentActivity(Leadmaster.Id, db, 7);
                    //await UpdateLeadCurrentActivity(Leadmaster.Id, db, iSequenceNo);
                    var ActivitySequence = db.ArthMateActivitySequence.FirstOrDefault(x => x.ScreenName.Trim() == "InProgress" && x.IsActive == true);
                    var LeadActivityMasterProgresses = db.LeadActivityMasterProgress.FirstOrDefault(x => x.ActivityMasterId == ActivitySequence.Id && x.LeadMasterId == LeadId && x.IsActive == true);
                    if (LeadActivityMasterProgresses != null)
                    {
                        LeadActivityMasterProgresses.IsComplete = false;
                        db.Entry(LeadActivityMasterProgresses).State = EntityState.Modified;
                    }
                    db.Entry(Leadmaster).State = EntityState.Modified;
                }
                if (db.Commit() > 0)
                {
                    //await RequestAScoreAPI(LeadId);
                    await CoLenderSelector(LeadId);

                    res.Msg = "Your request under process.";
                    res.Status = true;

                    LeadActivityProgressesHistory(LeadId, iSequenceNo, 0, "", "Offer Regenerate (sactionAmount:" + sactionAmount + ") Successfully");
                }
            }

            return res;
        }

        //Get Verification Status
        [HttpGet]
        [Route("GetVerifiedDocumentStatus")]
        public async Task<CommonResponseDc> GetVerifiedDocumentStatus(int SequenceNo, long LeadmasterId)
        {
            CommonResponseDc res = new CommonResponseDc();
            using (var db = new AuthContext())
            {
                //long leaddicumentid = (from k in db.LeadDocument
                //                       join adm in db.ArthMateDocumentMasters on k.DocumentMasterId equals adm.Id
                //                       where adm.DocumentName == DocumentName && k.LeadMasterId == LeadmasterId
                //                       select k.Id).FirstOrDefault();

                //var lead = db.LeadDocument.Where(x => x.Id == leaddicumentid).FirstOrDefault();
                var ActivityMasterId = db.ArthMateActivitySequence.Where(x => x.SequenceNo == SequenceNo).Select(x => x.Id).FirstOrDefault();
                var lead = db.LeadActivityMasterProgress.Where(x => x.LeadMasterId == LeadmasterId && x.ActivityMasterId == ActivityMasterId).FirstOrDefault();
                if (lead != null && lead.Comment != null)
                {
                    if (lead.IsApprove == 2)
                    {
                        res.Msg = "Rejected Successfully";
                        res.Data = lead;
                        res.Status = true;
                    }
                    else
                    {
                        res.Msg = "Not Verified";
                        res.Data = lead;
                        res.Status = false;
                    }
                }
                if (lead != null && (lead.Comment == null || lead.Comment == ""))
                {
                    if (lead.IsApprove == 1)
                    {
                        res.Msg = "Verified Successfully";
                        res.Data = lead;
                        res.Status = true;
                    }
                    else
                    {
                        res.Msg = "Not Verified";
                        res.Data = lead;
                        res.Status = false;
                    }
                }

            }
            return res;

        }

        //UpdateDocument
        [AllowAnonymous]
        [HttpGet]
        [Route("UpdateDocument")]
        public async Task<CommonResponseDc> UpdateDocument(string DocumentName, long LeadmasterId, string FrontFileUrl, string BackFileUrl)
        {
            CommonResponseDc res = new CommonResponseDc();

            using (AuthContext db = new AuthContext())
            {
                var lead = db.LeadMasters.Where(x => x.Id == LeadmasterId).FirstOrDefault();
                var documentId = db.ArthMateDocumentMasters.FirstOrDefault(x => x.DocumentName == DocumentName.Trim()).Id;
                if (lead != null)
                {
                    var leadDoc = db.LeadDocument.Where(x => x.LeadMasterId == LeadmasterId && x.DocumentMasterId == documentId).FirstOrDefault();
                    if (leadDoc != null)
                    {
                        leadDoc.DocumentMasterId = documentId;
                        leadDoc.FrontFileUrl = FrontFileUrl;
                        leadDoc.BackFileUrl = BackFileUrl;
                        leadDoc.ModifiedDate = DateTime.Now;
                        leadDoc.ModifiedBy = (int)LeadmasterId;
                        db.Entry(leadDoc).State = EntityState.Modified;
                    }
                    lead.ModifiedDate = DateTime.Now;
                    db.Entry(lead).State = EntityState.Modified;
                    //db.Commit();
                    if (db.Commit() > 0)
                    {
                        res.Msg = "Updated Successfully";
                        res.Data = null;
                        res.Status = true;
                    }
                    else
                    {
                        res.Msg = "Something went wrong!!";
                        res.Data = null;
                        res.Status = false;
                    }
                }
            }
            return res;
        }

        //GetLeadBackgroundRuns

        [AllowAnonymous]
        [HttpGet]
        [Route("GetLeadBackgroundRuns")]
        public async Task<CommonResponseDc> GetLeadBackgroundRuns(long LeadMasterId)
        {
            CommonResponseDc results = new CommonResponseDc();
            List<LeadBackgroundRunDc> LeadPageData = new List<LeadBackgroundRunDc>();
            using (var authContext = new AuthContext())
            {
                //var lead = authContext.LeadMasters.Where(x => x.Id == LeadMasterId).FirstOrDefault();

                var leadmasid = new SqlParameter("@LeadMasterId", LeadMasterId);
                results.Data = authContext.Database.SqlQuery<LeadBackgroundRunDc>("EXEC [Arthmate].[GetBackgroundRunApiActivity] @LeadMasterId", leadmasid).ToList();
                if (results.Data != null)
                {
                    results.Msg = "Data Not Found";
                    results.Status = false;

                }
                else
                {
                    results.Msg = "Background data";
                    results.Status = true;
                }

            }
            return results;

        }


        [AllowAnonymous]
        [HttpGet]
        [Route("InsertLeadBackgroundRuns")]
        public List<LeadBackgroundRunDc> InsertLeadBackgroundRuns(LeadBackgroundRunDc insertdata)
        {
            CommonResponseDc results = new CommonResponseDc();
            List<LeadBackgroundRunDc> LeadPageData = new List<LeadBackgroundRunDc>();
            using (var authContext = new AuthContext())
            {
                var datanew = authContext.LeadBackgroundRuns.Where(x => x.LeadMasterId == insertdata.LeadMasterId && x.ArthmateActivityMastersId == insertdata.ArthmateActivityMastersId && x.Status == "Error").FirstOrDefault();
                if (datanew == null)
                {
                    LeadBackgroundRun LeadBackgroundInsert = new LeadBackgroundRun();
                    LeadBackgroundInsert.LeadMasterId = insertdata.LeadMasterId;
                    LeadBackgroundInsert.ArthmateActivityMastersId = insertdata.ArthmateActivityMastersId;
                    LeadBackgroundInsert.ArthmateActivityName = insertdata.ArthmateActivityMastersApiName;
                    LeadBackgroundInsert.ReqJson = insertdata.ReqJson;
                    LeadBackgroundInsert.ResJson = insertdata.ResJson;
                    LeadBackgroundInsert.Status = insertdata.Status;
                    LeadBackgroundInsert.IsActive = true;
                    LeadBackgroundInsert.IsDeleted = false;
                    LeadBackgroundInsert.CreatedDate = DateTime.Now;
                    LeadBackgroundInsert.CreatedBy = 0;
                    authContext.LeadBackgroundRuns.Add(LeadBackgroundInsert);
                    authContext.Commit();
                }
                else
                {
                    datanew.ReqJson = insertdata.ReqJson;
                    datanew.ResJson = insertdata.ResJson;
                    datanew.ModifiedDate = DateTime.Now;
                    datanew.ModifiedBy = 0;
                    authContext.Entry(datanew).State = EntityState.Modified;
                    authContext.Commit();
                }

            }
            return LeadPageData;

        }

        [AllowAnonymous]
        [HttpGet]
        [Route("CommonLeadBackgroundRuns")]
        public bool CommonInsertLeadBackgroundRuns(string Apiname, long LeadMasterID)
        {
            bool status = false;
            //List<LeadBackgroundRunDc> LeadPageData = new List<LeadBackgroundRunDc>();
            using (var authContext = new AuthContext())
            {
                #region  commented
                //var datanew = authContext.LeadBackgroundRuns.Where(x => x.LeadMasterId == insertdata.LeadMasterId && x.ArthmateActivityMastersId == insertdata.ArthmateActivityMastersId && x.Status == "Error").FirstOrDefault();
                //if (datanew == null)
                //{
                //    LeadBackgroundRun LeadBackgroundInsert = new LeadBackgroundRun();
                //    LeadBackgroundInsert.LeadMasterId = insertdata.LeadMasterId;
                //    LeadBackgroundInsert.ArthmateActivityMastersId = insertdata.ArthmateActivityMastersId;
                //    LeadBackgroundInsert.ArthmateActivityName = insertdata.ArthmateActivityMastersApiName;
                //    LeadBackgroundInsert.ReqJson = insertdata.ReqJson;
                //    LeadBackgroundInsert.ResJson = insertdata.ResJson;
                //    LeadBackgroundInsert.Status = insertdata.Status;
                //    LeadBackgroundInsert.IsActive = true;
                //    LeadBackgroundInsert.IsDeleted = false;
                //    LeadBackgroundInsert.CreatedDate = DateTime.Now;
                //    LeadBackgroundInsert.CreatedBy = 0;
                //    authContext.LeadBackgroundRuns.Add(LeadBackgroundInsert);
                //    authContext.Commit();
                //}
                //else
                //{
                //    datanew.ReqJson = insertdata.ReqJson;
                //    datanew.ResJson = insertdata.ResJson;
                //    datanew.ModifiedDate = DateTime.Now;
                //    datanew.ModifiedBy = 0;
                //    authContext.Entry(datanew).State = EntityState.Modified;
                //    authContext.Commit();
                //}
                #endregion
                if (LeadMasterID > 0)
                {
                    string ReqJson = "";
                    string ResJson = "";
                    ArthMateHelper arthMateHelper = new ArthMateHelper();
                    status = arthMateHelper.InsertLeadBackgroundRun(LeadMasterID, Apiname, ReqJson, ResJson);
                }
            }
            return status;
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("PostRetryApi")]
        public async Task<CommonResponseDc> PostRetryApi(RetryApiDc retryApiDc/*string ApiName, long LeadMasterId*/)
        {
            CommonResponseDc res = new CommonResponseDc();
            AuthContext context = new AuthContext();
            if (retryApiDc.ApiName == "CeplrPdfReports")
            {
                string bankpass = context.LeadDocument.Where(x => x.LeadMasterId == retryApiDc.LeadMasterId).Select(x => x.PdfPassword.Trim()).ToString();
                var pdfres = CeplrPdfReports(retryApiDc.LeadMasterId);
                res.Data = pdfres.Result.Data;
                res.Msg = pdfres.Result.Msg;
                res.Status = pdfres.Result.Status;
            }
            else if (retryApiDc.ApiName == "Basic Reports")
            {
                CeplrBasicReportPayload CplrBasic = new CeplrBasicReportPayload();
                CplrBasic.start_date = retryApiDc.start_date;
                CplrBasic.end_date = retryApiDc.start_date;
                CplrBasic.LeadmasterId = retryApiDc.LeadMasterId;

                var ceplrres = await CeplrBasicReport(retryApiDc.LeadMasterId);
                res.Data = ceplrres.Data;
                res.Msg = ceplrres.Msg;
                res.Status = ceplrres.Status;
            }
            else if (retryApiDc.ApiName == "Request-A ScoreAPI")
            {
                var resdata = await RequestAScoreAPI(retryApiDc.LeadMasterId);
                res.Data = resdata.data;
                res.Msg = resdata.message;
                res.Status = resdata.status_code == 200 ? true : false;
            }
            else if (retryApiDc.ApiName == "LeadApi")
            {
                var resdata = await PostLead(retryApiDc.LeadMasterId);
                res.Data = resdata.Data;
                res.Msg = resdata.Msg;
                res.Status = resdata.Status;
            }
            return res;
        }


        [AllowAnonymous]
        [HttpPost]
        [Route("ArthmateApiFlow")]
        public async Task<bool> ArthmateApiFlowAPI(long LeadMasterId)
        {
            return await ArthmateApiFlow(LeadMasterId);

        }

        private async Task<bool> ArthmateApiFlow(long LeadMasterId)
        {
            using (var db = new AuthContext())
            {
                var request_id = db.AScore.FirstOrDefault(x => x.LeadMasterId == LeadMasterId && x.IsActive == true && x.IsDeleted == false)?.request_id;
                if (request_id == null)
                {
                    var res1 = await RequestAScoreAPI(LeadMasterId);
                }
                return true;
            }

        }
        #region commented code
        //[AllowAnonymous]
        //[HttpGet]
        //[Route("AgeeTermsAndConditionsHtml")]
        //public string AgreeTermsAndCondition(long Leadmasterid)
        //{
        //    string thFileName = "";
        //    string htmldata = "";
        //    string replacetext = "";
        //    string path = "";
        //    string newfilepath = "";
        //    //repay_scheduleDc Repyment_ScduleData = new repay_scheduleDc();
        //    using (AuthContext context = new AuthContext())
        //    {
        //        var data = context.LeadMasters.Where(x => x.Id == Leadmasterid).FirstOrDefault();
        //        var loanid = context.LeadLoan.Where(x => x.LeadMasterId == Leadmasterid).FirstOrDefault();
        //        //var configurationdata = context.LoanConfiguration.FirstOrDefault();
        //        var pdfdata = context.LoanConfiguration.FirstOrDefault();
        //        //if (loanid != null)
        //        //{
        //        //    //Repayment Schedule data 
        //        //    ArthMateHelper arthMateHelper = new ArthMateHelper();

        //        //    Postrepayment_scheduleDc reqdata = new Postrepayment_scheduleDc()
        //        //    {
        //        //        company_id = 0,
        //        //        loan_id = loanid.loan_id,
        //        //        product_id = ""
        //        //    };
        //        //     Repyment_ScduleData = (arthMateHelper.repayment_schedule(reqdata, 0, Leadmasterid)).Result;

        //        //}
        //        path = System.Web.Hosting.HostingEnvironment.MapPath(@"~\Templates\Arthmate\AgreeTermsAndCondition.html");

        //        if (File.Exists(path))
        //        {
        //            htmldata = File.ReadAllText(path);
        //            if (!string.IsNullOrEmpty(htmldata))
        //            {
        //                replacetext = $" {data.CreatedDate} ";
        //                htmldata = htmldata.Replace("{{ApplicationDate}}", replacetext);

        //                replacetext = $"{data.first_name + data.last_name} ";
        //                htmldata = htmldata.Replace("{{NameOfBorrower}}", replacetext);

        //                replacetext = $"{data.bus_gstno} ";
        //                htmldata = htmldata.Replace("{{GstNumber}}", replacetext);

        //                replacetext = $" {pdfdata.PF} ";
        //                htmldata = htmldata.Replace("{{ProcessingFees}}", replacetext);

        //                replacetext = $" {pdfdata.BounceCharge} ";
        //                htmldata = htmldata.Replace("{{BounceCharge}}", replacetext);

        //                replacetext = $"{data.father_fname + data.father_lname} ";
        //                htmldata = htmldata.Replace("{{FatherName}}", replacetext);

        //                replacetext = $"{data.per_addr_ln1} ";
        //                htmldata = htmldata.Replace("{{address1}}", replacetext);

        //                replacetext = $"{data.per_addr_ln2} ";
        //                htmldata = htmldata.Replace("{{address2}}", replacetext);

        //                replacetext = $"{data.Id} ";
        //                htmldata = htmldata.Replace("{{LeadId}}", replacetext);

        //            }
        //        }

        //        if (!string.IsNullOrEmpty(htmldata))
        //        {

        //            string TartgetfolderPath = HttpContext.Current.Server.MapPath(@"~\ArthmateDocument\Agreement\TermsCondition");
        //            if (!Directory.Exists(TartgetfolderPath))
        //                Directory.CreateDirectory(TartgetfolderPath);

        //            thFileName = "TermsAndConditions" + DateTime.Now.ToString("MMddyyyyhhmmss") + ".pdf";
        //            newfilepath = "/ArthmateDocument/Agreement/TermsCondition" + thFileName;
        //            string OutPutFile = Path.Combine(HttpContext.Current.Server.MapPath("~/ArthmateDocument/Agreement/TermsCondition"), thFileName);

        //            byte[] pdf = null;

        //            pdf = Pdf
        //                  .From(htmldata)
        //                  .OfSize(OpenHtmlToPdf.PaperSize.A4)
        //                  .WithTitle("TermsAndCondition")
        //                  .WithoutOutline()
        //                  .WithMargins(PaperMargins.All(0.0.Millimeters()))
        //                  .Portrait()
        //                  .Comressed()
        //                  .Content();
        //            FileStream file = File.Create(OutPutFile);
        //            file.Write(pdf, 0, pdf.Length);
        //            file.Close();
        //        }

        //    }
        //    return newfilepath;
        //}

        #endregion

        [AllowAnonymous]
        [HttpGet]
        [Route("SanctionAndLbaHtml")]
        public async Task<string> LBABusinessLoan(long Leadmasterid, bool DisplayOrSubmit, int SequenceNo)
        {
            string htmldata = "";
            string replacetext = "";
            string path = "";
            string thFileName = "";
            string newfilepath = "";
            List<RepaymentSchedule> repaymentSchedule = new List<RepaymentSchedule>();
            repay_scheduleDc Repyment_ScduleData = new repay_scheduleDc();
            using (AuthContext context = new AuthContext())
            {
                var data = context.LeadMasters.Where(x => x.Id == Leadmasterid && x.IsActive == true && x.IsDeleted == false).FirstOrDefault();
                if (data != null)
                {
                    var LoanConfigData = context.LoanConfiguration.FirstOrDefault();
                    var laondata = context.LeadLoan.Where(x => x.LeadMasterId.Value == Leadmasterid && x.IsActive == true && x.IsDeleted == false).FirstOrDefault();
                    var colender = context.CoLenderResponse.Where(x => x.LeadMasterId == Leadmasterid && x.IsActive == true && x.IsDeleted == false && x.loan_amount > 0).OrderByDescending(x => x.CreatedDate).FirstOrDefault();

                    string StampUrl = "";
                    var arthmateSlaLbaStamp = context.ArthmateSlaLbaStampDetail.FirstOrDefault(x => x.LeadmasterId == Leadmasterid && x.IsActive && x.IsDeleted == false);
                    if (arthmateSlaLbaStamp != null)
                    {
                        if (arthmateSlaLbaStamp.IsStampUsed == false)
                        {
                            string url = string.Concat(HttpRuntime.AppDomainAppPath, arthmateSlaLbaStamp.StampUrl.Replace("/", "\\"));
                            //string url = "D://BackendUAT//AngularJSAuthentication.API///ArthmateDocument//SlaDocument//StampPaper//ARTHMATE.jpg";
                            string imageSrc = "data:image/" + Path.GetExtension(url).Replace(".", "") + ";base64," + Convert.ToBase64String(File.ReadAllBytes(url));
                            // Set the desired width and height
                            int width = 900;
                            int height = 1100;
                            // Modify the StampUrl string to include the width and height attributes
                            StampUrl = $"<img src=\"{imageSrc}\" width=\"{width}\" height=\"{height}\" />";
                        }
                        else
                        {
                            //Error
                            //return;
                            return "The Stamp has been used.Please Contact Administrator to reuse this stamp.";
                        }
                    }
                    else
                    {
                        var StampUrlData = context.ArthmateSlaLbaStampDetail.FirstOrDefault(x => x.LeadmasterId == 0 && x.IsStampUsed == false && x.IsActive && x.IsDeleted == false);
                        if (StampUrlData != null)
                        {
                            string url = string.Concat(HttpRuntime.AppDomainAppPath, arthmateSlaLbaStamp.StampUrl.Replace("/", "\\"));
                            string imageSrc = "data:image/" + Path.GetExtension(url).Replace(".", "") + ";base64," + Convert.ToBase64String(File.ReadAllBytes(url));
                            // Set the desired width and height
                            int width = 800;
                            int height = 900;
                            // Modify the StampUrl string to include the width and height attributes
                            StampUrl = $"<img src=\"{imageSrc}\" width=\"{width}\" height=\"{height}\" />";
                            StampUrlData.LeadmasterId = Leadmasterid;
                            StampUrlData.DateofUtilisation = DateTime.Now;
                            context.Entry(StampUrlData).State = EntityState.Modified;
                            context.Commit();
                        }
                    }


                    //Repayment Schedule data 
                    //if (!DisplayOrSubmit)
                    //{
                    ArthMateHelper arthMateHelper = new ArthMateHelper();
                    if (laondata != null)
                    {
                        Postrepayment_scheduleDc reqdata = new Postrepayment_scheduleDc()
                        {
                            company_id = laondata.company_id.Value, //9093101,//
                            loan_id = laondata.loan_id, //"AMLABKADG100000084449",//
                            product_id = laondata.product_id //"9093425",// 
                        };
                        //check on condition for Disbursment done or not
                        Repyment_ScduleData = await arthMateHelper.repayment_schedule(reqdata, 0, Leadmasterid, false);

                        //repaymentSchedule.AddRange(Repyment_ScduleData.data.rows);
                    }
                    //}

                    //var Repyment_Data = context.RepaymentSchedule.Where(x => x.LeadMasterId == Leadmasterid && x.IsActive == true && x.IsDeleted == false).ToList();
                    //repaymentSchedule.AddRange(Repyment_Data);
                    path = System.Web.Hosting.HostingEnvironment.MapPath(@"~\Templates\Arthmate\LBABusinessLoan.html");

                    if (File.Exists(path) && laondata != null)
                    {
                        htmldata = File.ReadAllText(path);
                        if (!string.IsNullOrEmpty(htmldata))
                        {
                            replacetext = $"{laondata.loan_id} ";
                            htmldata = htmldata.Replace("{{loan_id}}", replacetext);

                            replacetext = $"{data.first_name} ";
                            htmldata = htmldata.Replace("{{FirstNameOfBorrower}}", replacetext);

                            replacetext = $"{data.middle_name}&nbsp;";
                            htmldata = htmldata.Replace("{{MiddleNameOfBorrower}}", replacetext);

                            replacetext = $"{ data.last_name} ";
                            htmldata = htmldata.Replace("{{LastNameOfBorrower}}", replacetext);

                            replacetext = $"{data.Id} ";
                            htmldata = htmldata.Replace("{{LeadId}}", replacetext);

                            replacetext = $"{data.co_app_name} ";
                            htmldata = htmldata.Replace("{{co_app_name}}", replacetext);

                            replacetext = $"{data.appl_pan} ";
                            htmldata = htmldata.Replace("{{appl_pan}}", replacetext);

                            replacetext = $"{data.bus_pan} ";
                            htmldata = htmldata.Replace("{{co_app_pan}}", replacetext);

                            replacetext = $"{LoanConfigData.InterestRate} ";
                            htmldata = htmldata.Replace("{{InterestRate}}", replacetext);

                            //replacetext = $" {LoanConfigData.PF} ";
                            //htmldata = htmldata.Replace("{{ProcessingFees}}", replacetext);

                            replacetext = $" {data.AccountHolderName} ";
                            htmldata = htmldata.Replace("{{AccountHolderName}}", replacetext);

                            replacetext = $" {data.borro_bank_name} ";
                            htmldata = htmldata.Replace("{{bankName}}", replacetext);

                            replacetext = $" {data.borro_bank_acc_num} ";
                            htmldata = htmldata.Replace("{{accountNumber}}", replacetext);

                            replacetext = $" {data.borro_bank_ifsc} ";
                            htmldata = htmldata.Replace("{{IFSCCode}}", replacetext);

                            #region   Benefiarcy Bank Details
                            replacetext = $" {data.ben_Accountholdername} ";
                            htmldata = htmldata.Replace("{{ben_Accountholdername}}", replacetext);

                            replacetext = $" {data.ben_AccountNumber} ";
                            htmldata = htmldata.Replace("{{ben_AccountNumber}}", replacetext);

                            replacetext = $" {data.ben_IFSCCode} ";
                            htmldata = htmldata.Replace("{{ben_IFSCCode}}", replacetext);

                            replacetext = $" {data.ben_Typeofaccount} ";
                            htmldata = htmldata.Replace("{{ben_Typeofaccount}}", replacetext);

                            replacetext = $" {data.ben_Bankname} ";
                            htmldata = htmldata.Replace("{{ben_Bankname}}", replacetext);


                            #endregion


                            //sanction letter

                            // DateTime dt = DateTime.ParseExact(data.AppliedDate, "MM/dd/yyyy hh:mm:ss tt", CultureInfo.InvariantCulture);

                            //string s = dt.ToString("dd/M/yyyy", CultureInfo.InvariantCulture);
                            replacetext = $"{data.AppliedDate.Value.ToString("dd/MM/yyyy")} ";
                            htmldata = htmldata.Replace("{{DATE}}", replacetext);

                            replacetext = $"{data.business_name} ";   //{data.first_name} 
                            htmldata = htmldata.Replace("{{BorrowerName}}", replacetext);

                            replacetext = $"{data.per_addr_ln1} ";
                            htmldata = htmldata.Replace("{{address1}}", replacetext);

                            replacetext = $"{data.per_addr_ln2} ";
                            htmldata = htmldata.Replace("{{address2}}", replacetext);

                            replacetext = $"{data.city} ";
                            htmldata = htmldata.Replace("{{city}}", replacetext);

                            replacetext = $"{data.state} ";
                            htmldata = htmldata.Replace("{{state}}", replacetext);

                            replacetext = $"{data.MobileNo} ";
                            htmldata = htmldata.Replace("{{MobileNo}}", replacetext);

                            replacetext = $"{data.email_id} ";
                            htmldata = htmldata.Replace("{{email_id}}", replacetext);

                            replacetext = $" {data.CreatedDate} ";
                            htmldata = htmldata.Replace("{{ApplicationDate}}", replacetext);

                            replacetext = $" {data.first_name + data.last_name} "; //{data.first_name + data.last_name} 
                            htmldata = htmldata.Replace("{{BorrowerNamess}}", replacetext);

                            replacetext = $" {data.co_app_name} ";
                            htmldata = htmldata.Replace("{{co-borrowerName}}", replacetext);

                            replacetext = $" {data.bus_add_corr_line1} ";
                            htmldata = htmldata.Replace("{{bus_add_corr_line1}}", replacetext);

                            replacetext = $" {data.bus_add_corr_line2} ";
                            htmldata = htmldata.Replace("{{bus_add_corr_line2}}", replacetext);

                            replacetext = $" {data.bus_add_corr_city} ";
                            htmldata = htmldata.Replace("{{bus_add_corr_city}}", replacetext);

                            replacetext = $" {data.bus_add_corr_state} ";
                            htmldata = htmldata.Replace("{{bus_add_corr_state}}", replacetext);

                            replacetext = $" {LoanConfigData.InterestRate} ";
                            htmldata = htmldata.Replace("{{InterestRate}}", replacetext);

                            replacetext = $" {LoanConfigData.PlatformFee} ";
                            htmldata = htmldata.Replace("{{PlatformFee}}", replacetext);

                            replacetext = $" {LoanConfigData.PF} ";
                            htmldata = htmldata.Replace("{{ProcessingFees}}", replacetext);

                            replacetext = $" {data.purpose_of_loan} ";
                            htmldata = htmldata.Replace("{{purpose_of_loan}}", replacetext);

                            replacetext = $" {data.AccountType} ";
                            htmldata = htmldata.Replace("{{Acctype}}", replacetext);

                            replacetext = $" {data.business_name} ";
                            htmldata = htmldata.Replace("{{business_name}}", replacetext);


                            replacetext = $" {data.AccountHolderName} ";
                            htmldata = htmldata.Replace("{{AccountHolderName}}", replacetext);

                            double convenienceFees = 0;
                            if (colender.loan_amount <= 5000000 && colender.loan_amount >= 2000000 && (data.bus_entity_type == "Partnership" || data.bus_entity_type == "PrivateLtd" || data.bus_entity_type == "LLP" || data.bus_entity_type == "HUF" || data.bus_entity_type == "OPC"))
                            {
                                convenienceFees = 5000;
                            }
                            else if (colender.loan_amount < 500000 && (data.bus_entity_type == "Proprietorship" || data.bus_entity_type == "SelfEmployed"))
                            {
                                convenienceFees = 500;
                            }
                            else if (colender.loan_amount >= 500000 && colender.loan_amount <= 1000000 && (data.bus_entity_type == "Proprietorship" || data.bus_entity_type == "SelfEmployed"))
                            {
                                convenienceFees = 900;
                            }
                            else if (colender.loan_amount > 100000 && colender.loan_amount < 2000000 && (data.bus_entity_type == "Proprietorship" || data.bus_entity_type == "SelfEmployed"))
                            {
                                convenienceFees = 2000;
                            }
                            else if (colender.loan_amount > 2000000 && (data.bus_entity_type == "Proprietorship" || data.bus_entity_type == "SelfEmployed"))
                            {
                                convenienceFees = 3000;
                            }

                            //platform fees
                            double platformFeesAmt = 0;
                            if (LoanConfigData.PlatformFee > 0)
                            {
                                platformFeesAmt = (LoanConfigData.PlatformFee * colender.SanctionAmount??0) / 100;

                                TextFileLogHelper.LogError("PlatformFeeAmt lba " + platformFeesAmt);

                                convenienceFees = convenienceFees + platformFeesAmt;
                            }
                            replacetext = $" {convenienceFees} ";
                            htmldata = htmldata.Replace("{{convenienceFees}}", replacetext); //covonviences fees

                            double ProcessingFeesAmt = 0;
                            ProcessingFeesAmt = Math.Round(colender.SanctionAmount * (LoanConfigData.PF / 100), 2);
                            ProcessingFeesAmt = ProcessingFeesAmt * 1.18;
                            replacetext = $" {ProcessingFeesAmt} ";
                            htmldata = htmldata.Replace("{{ProcessingFeeAmount}}", replacetext);




                            //01-03-2024
                            var Broken_Period_interest = laondata.broken_period_int_amt;
                            replacetext = $" {Broken_Period_interest} ";
                            htmldata = htmldata.Replace("{{Broken_Period_interest}}", replacetext);

                            //string ImageUrl = AppConstants.ImageUrl;
                            //string img = ImageUrl + "Template/Arthmate";
                            if (!string.IsNullOrEmpty(StampUrl))
                            {
                                //string filepath = System.Web.HttpContext.Current.Server.MapPath("~/");

                                //TextFileLogHelper.TraceLog("stamp path filepath - " + filepath);

                                //string ImageUrl = AppConstants.ImageUrl;
                                //TextFileLogHelper.TraceLog("stamp path ImageUrl - " + ImageUrl);

                                ////string filepath = System.Web.HttpContext.Current.Server.MapPath("~/ArthmateDocument/StampPaper"); Replace("/", "\\"));
                                //// var pdfPath = string.Concat(HttpRuntime.AppDomainAppPath, data.FrontFileUrl.Replace("/", "\\"));
                                //string OutPutFile1 = string.Concat(ImageUrl, StampUrl);
                                //TextFileLogHelper.TraceLog("stamp path OutPutFile1 - " + OutPutFile1);
                                replacetext = $" {StampUrl.Trim()} ";
                                //TextFileLogHelper.TraceLog("stamp path replacetext - " + replacetext);
                                htmldata = htmldata.Replace("{{StampUrl}}", replacetext);
                            }
                            double OutstandingAmount = 0;
                            double PenalAmount = 0;
                            double PenalAmountGST = 0;
                            double TotalPenalAmount = 0;
                            double processing_fees_perc = LoanConfigData.PF;
                            double prepayment_charges_amt = 0;
                            double processingfeesamount = 0;

                            if (laondata != null && colender != null)
                            {
                                double borroamt = Convert.ToDouble(colender.SanctionAmount) + (Convert.ToDouble(laondata.loan_int_amt)
                                   + Convert.ToDouble(laondata.insurance_amount)) + Convert.ToDouble(convenienceFees); //+ Convert.ToDouble(laondata.broken_period_int_amt); 
                                // + Convert.ToDouble(laondata.processing_fees_amt) 

                                replacetext = $"{ borroamt + ProcessingFeesAmt } ";//+ platformFeesAmt
                                htmldata = htmldata.Replace("{{TotalBorroAmt}}", replacetext);//total amt paid by the borrower
                                if (Repyment_ScduleData != null && Repyment_ScduleData.data.count > 0)
                                {
                                    OutstandingAmount = Repyment_ScduleData.data.rows.FirstOrDefault().principal_outstanding == 0 ? 0 : Repyment_ScduleData.data.rows.FirstOrDefault().principal_outstanding;
                                }
                                else { OutstandingAmount = 0; }
                                //OutstandingAmount = Repyment_Data.FirstOrDefault().principal_outstanding == 0 ? 0 : Repyment_Data.FirstOrDefault().principal_outstanding;
                                PenalAmount = OutstandingAmount * (LoanConfigData.PenalPercent / 100);
                                PenalAmountGST = PenalAmount * (LoanConfigData.GST / 100);

                                TotalPenalAmount = PenalAmount + PenalAmountGST;

                                prepayment_charges_amt = (OutstandingAmount) * (LoanConfigData.PF / 100);


                                replacetext = $" {TotalPenalAmount} ";
                                htmldata = htmldata.Replace("{{TotalPenalAmount}}", replacetext);

                                replacetext = $" {prepayment_charges_amt} ";
                                htmldata = htmldata.Replace("{{prepayment_charges_amt}}", replacetext);


                                //processing fees amount
                                processingfeesamount = Convert.ToDouble(colender.SanctionAmount) * (processing_fees_perc / 100);

                                replacetext = $" {processingfeesamount} ";
                                htmldata = htmldata.Replace("{{processingfeesamount}}", replacetext);

                                replacetext = $" {colender.SanctionAmount} ";
                                htmldata = htmldata.Replace("{{SanctionAmount}}", replacetext);


                                replacetext = $" {laondata.first_inst_date.Value.ToString("dd/MM/yyyy")} "; //dd-mm-yyyy
                                htmldata = htmldata.Replace("{{first_inst_date}}", replacetext);

                                replacetext = $" {laondata.loan_amount_requested} ";
                                htmldata = htmldata.Replace("{{loan_amount_requested}}", replacetext);

                                replacetext = $" {laondata.loan_int_amt} ";
                                htmldata = htmldata.Replace("{{loan_int_amt}}", replacetext);

                                replacetext = $" {laondata.insurance_amount} ";
                                htmldata = htmldata.Replace("{{insurance_amount}}", replacetext);

                                replacetext = $" {laondata.tenure} ";
                                htmldata = htmldata.Replace("{{tenure}}", replacetext);

                                replacetext = $" {laondata.net_disbur_amt} ";
                                htmldata = htmldata.Replace("{{net_disbur_amt}}", replacetext);

                                replacetext = $" {laondata.loan_int_rate} ";
                                htmldata = htmldata.Replace("{{loan_int_rate}}", replacetext);

                                replacetext = $" {laondata.penal_interest} ";
                                htmldata = htmldata.Replace("{{penal_interest}}", replacetext);

                                replacetext = $" {laondata.emi_amount} ";
                                htmldata = htmldata.Replace("{{emi_amount}}", replacetext);


                                DateTime dt = DateTime.ParseExact(laondata.loan_app_date, "yyyy-MM-dd", CultureInfo.InvariantCulture);
                                replacetext = $" {dt.Date.ToString("dd-MM-yyyy")} ";
                                htmldata = htmldata.Replace("{{loan_app_date}}", replacetext);


                                int Tenuare = 0;
                                double MonthlyEmiAMount = 0;
                                double SanctionAmount = 0;
                                //double PfAmountGST = 0;
                                double PfAmount = 0;
                                int InsuranceAmount = 0;
                                double DocumentCharges = 0;

                                Tenuare = Convert.ToInt32(laondata.tenure);
                                MonthlyEmiAMount = Convert.ToDouble(laondata.emi_amount);
                                SanctionAmount = colender.SanctionAmount;
                                PfAmount = processingfeesamount;
                                //PfAmountGST = PfAmount * (0.18);
                                InsuranceAmount = Convert.ToInt32(laondata.insurance_amount);
                                DocumentCharges = 0;

                                double APR = Financial.Rate(Tenuare, (-MonthlyEmiAMount), SanctionAmount - PfAmount - InsuranceAmount - DocumentCharges) * 12 * 100;

                                replacetext = $" {Math.Round(APR, 2)} ";
                                htmldata = htmldata.Replace("{{APR}}", replacetext);
                            }
                            //repayment schedule data
                            if (Repyment_ScduleData != null && Repyment_ScduleData.data.count > 0)
                            {

                                List<DataTable> dt = new List<DataTable>();
                                DataTable Repyment_Scdule = new DataTable();
                                Repyment_Scdule.TableName = "Repyment_Scdule";
                                dt.Add(Repyment_Scdule);
                                Repyment_Scdule.Columns.Add("Instalment No");
                                Repyment_Scdule.Columns.Add("Outstanding Principal Amount(in Rupees)");
                                Repyment_Scdule.Columns.Add("Instalment(in Rupees)");//emi_amount
                                Repyment_Scdule.Columns.Add("Interest(in Rupees)");//int_amount
                                Repyment_Scdule.Columns.Add("Principal(in Rupees)");
                                //var repaymentNewData = context.RepaymentSchedule.Where(x => x.LeadMasterId == Leadmasterid && x.IsActive == true && x.IsDeleted == false).ToList();
                                foreach (var item in Repyment_ScduleData.data.rows)
                                {
                                    var dr = Repyment_Scdule.NewRow();
                                    dr["Instalment No"] = item.emi_no;
                                    dr["Outstanding Principal Amount(in Rupees)"] = item.principal_outstanding;
                                    dr["Instalment(in Rupees)"] = item.emi_amount;
                                    dr["Interest(in Rupees)"] = item.int_amount;
                                    dr["Principal(in Rupees)"] = item.prin;
                                    Repyment_Scdule.Rows.Add(dr);
                                    //replacetext = $" {item.emi_no} ";
                                    //htmldata = htmldata.Replace("{{emi_no}}", replacetext);

                                    //replacetext = $" {0} ";
                                    //htmldata = htmldata.Replace("{{principal_outstanding}}", replacetext);

                                    //replacetext = $" {item.prin} ";
                                    //htmldata = htmldata.Replace("{{prin}}", replacetext);

                                    ////replacetext = $" {item.interest} ";   //INTETREST 
                                    ////htmldata = htmldata.Replace("{{ProcessingFees}}", replacetext);

                                    //replacetext = $" {item.emi_amount} ";
                                    //htmldata = htmldata.Replace("{{emi_amount}}", replacetext);

                                }
                                var htmltable = ConvertDataTableToHTML(Repyment_Scdule);

                                replacetext = $" {htmltable} ";
                                htmldata = htmldata.Replace("{{DYTable}}", replacetext);

                            }
                        }
                        if (!string.IsNullOrEmpty(htmldata))
                        {
                            path = System.Web.Hosting.HostingEnvironment.MapPath(@"~\Templates\Arthmate\LBABusinessLoan.html");
                        }

                        TextFileLogHelper.TraceLog("stamp path htmldata - " + htmldata);

                        if (DisplayOrSubmit)//submit
                        {
                            string TartgetfolderPath = HttpContext.Current.Server.MapPath(@"~\ArthmateDocument\SlaDocument");
                            if (!Directory.Exists(TartgetfolderPath))
                                Directory.CreateDirectory(TartgetfolderPath);

                            ///ArthmateDocument/SlaDocument
                            thFileName = "SanctionAndLbaBusinessLoan" + Leadmasterid + "_" + DateTime.Now.ToString("MMddyyyyhhmmss") + ".pdf";
                            newfilepath = "/ArthmateDocument/SlaDocument/" + thFileName;
                            string OutPutFile = Path.Combine(HttpContext.Current.Server.MapPath("~/ArthmateDocument/SlaDocument"), thFileName);

                            byte[] pdf = null;
                            pdf = Pdf
                                  .From(htmldata)
                                  .OfSize(OpenHtmlToPdf.PaperSize.A4)
                                  //.WithHeader("<div style='text-align: center; font-size: 12px;'>Header</div>")
                                  //.WithFooter("<div style='text-align: center; font-size: 12px;'>Page {page} of {pages}</div>")
                                  .WithTitle("Agreement")
                                  .WithoutOutline()
                                  .WithMargins(PaperMargins.All(0.0.Millimeters()))
                                  .WithObjectSetting("footer.fontSize", "8")
                                  .WithObjectSetting("footer.fontName", "times")
                                  .WithObjectSetting("footer.center", "[page] of [topage]")
                                  .WithGlobalSetting("margin.bottom", "3cm")
                                  .WithGlobalSetting("margin.top", "2cm")
                                  .Portrait()
                                  .Comressed()
                                  .Content();
                            FileStream file = File.Create(OutPutFile);
                            file.Write(pdf, 0, pdf.Length);
                            file.Close();
                            AngularJSAuthentication.Common.Helpers.FileUploadHelper.Upload(thFileName, "~/ArthmateDocument/SlaDocument", OutPutFile);

                            if (laondata != null)
                            {
                                laondata.UrlSlaDocument = newfilepath;
                                context.Entry(laondata).State = EntityState.Modified;
                            }
                            data.SequenceNo = SequenceNo;
                            data.ModifiedDate = DateTime.Now;
                            context.Entry(data).State = EntityState.Modified;
                            context.Commit();

                            await UpdateLeadCurrentActivity(Leadmasterid, context, SequenceNo);

                            LeadActivityProgressesHistory(Leadmasterid, SequenceNo, 0, "", "Sanction And Lba (SLA) Letter Accepted By Customer Successfully");


                            #region esign session start
                            try
                            {
                                if (GenerateEsign)
                                {
                                    TextFileLogHelper.TraceLog("esign session : => start");
                                    var res = await eSignSessionAsync(Leadmasterid);
                                    //if (res.Status == true)
                                    //{
                                    //    string docId = res.NameOnCard;

                                    //    var eSignDocStatus = await ESignDocumentasync(Leadmasterid, docId);
                                    //    if (eSignDocStatus.result != null)
                                    //    {
                                    //        //UploadSignedSla(Leadmasterid);
                                    //        //using (AuthContext context = new AuthContext())
                                    //        //{
                                    //        //var LeadLoandata = context.LeadLoan.Where(x => x.LeadMasterId == Leadmasterid && x.IsActive == true && x.IsDeleted == false).FirstOrDefault();


                                    //        var sanctionLeadLoanDocuments = context.LeadDocument.Where(x => x.LeadMasterId == Leadmasterid && x.OtherInfo == sLoan_sanction_letter && x.IsActive == true && x.IsDeleted == false).FirstOrDefault();

                                    //        if (sanctionLeadLoanDocuments != null)
                                    //        {
                                    //            sanctionLeadLoanDocuments.IsActive = false;
                                    //            sanctionLeadLoanDocuments.IsDeleted = true;
                                    //            context.Entry(sanctionLeadLoanDocuments).State = EntityState.Modified;
                                    //            //context.Commit();
                                    //        }

                                    //        long sanctionDocumentMasterId = context.ArthMateDocumentMasters.Where(x => x.DocumentName == sLoan_sanction_letter).FirstOrDefault().Id;
                                    //        LeadLoanDocument sanctionleadLoanDocument = new LeadLoanDocument()
                                    //        {
                                    //            FrontFileUrl = eSignDocStatus.FileUrl,
                                    //            BackFileUrl = null,
                                    //            CreatedBy = 0,
                                    //            CreatedDate = DateTime.Now,
                                    //            DocumentNumber = "",
                                    //            DocumentMasterId = sanctionDocumentMasterId,
                                    //            IsActive = true,
                                    //            LeadMasterId = Leadmasterid,
                                    //            OtherInfo = sLoan_sanction_letter,
                                    //            IsVerified = true,
                                    //            IsDeleted = false
                                    //        };
                                    //        context.LeadDocument.Add(sanctionleadLoanDocument);

                                    //        //------- s ----------------------------------------------------
                                    //        var LeadLoanDocuments = context.LeadDocument.Where(x => x.LeadMasterId == Leadmasterid && x.OtherInfo == sSigned_loan_sanction_letter && x.IsActive == true && x.IsDeleted == false).FirstOrDefault();

                                    //        if (LeadLoanDocuments != null)
                                    //        {
                                    //            LeadLoanDocuments.IsActive = false;
                                    //            LeadLoanDocuments.IsDeleted = true;
                                    //            context.Entry(LeadLoanDocuments).State = EntityState.Modified;
                                    //            //context.Commit();
                                    //        }

                                    //        long DocumentMasterId = context.ArthMateDocumentMasters.Where(x => x.DocumentName == sSigned_loan_sanction_letter).FirstOrDefault().Id;
                                    //        LeadLoanDocument leadLoanDocument = new LeadLoanDocument()
                                    //        {
                                    //            FrontFileUrl = eSignDocStatus.FileUrl,
                                    //            CreatedBy = 0,
                                    //            CreatedDate = DateTime.Now,
                                    //            DocumentNumber = "",
                                    //            DocumentMasterId = DocumentMasterId,
                                    //            IsActive = true,
                                    //            LeadMasterId = Leadmasterid,
                                    //            OtherInfo = sSigned_loan_sanction_letter,
                                    //            IsVerified = true,
                                    //            IsDeleted = false
                                    //        };
                                    //        context.LeadDocument.Add(leadLoanDocument);

                                    //        //--------e---------------------------------------


                                    //        //if (LeadLoandata != null)
                                    //        //{
                                    //        //    //LeadLoandata.UrlSlaDocument = LogoUrl;//customer sla Document Accept copy pdf
                                    //        //    LeadLoandata.UrlSlaUploadSignedDocument = eSignDocStatus.FileUrl;//customer sla Document signed copy pdf
                                    //        //    context.Entry(LeadLoandata).State = EntityState.Modified;
                                    //        //}
                                    //        context.Commit();

                                    //        //post Doc To Arthmate 
                                    //        //var DocStatus = ArthmatePostDoc(Leadmasterid, sLoan_sanction_letter);
                                    //        //var datwa = ArthmatePostDoc(Leadmasterid, sSigned_loan_sanction_letter);

                                    //        LeadActivityProgressesHistory(Leadmasterid, 0, 0, "ESignDocumentasync works", "ESignDocument Done");

                                    //    }
                                    //}
                                }
                            }
                            catch (Exception ex)
                            {

                                TextFileLogHelper.TraceLog("esign Error : => start" + ex.Message);
                                //return ex.Message;
                            }
                            #endregion

                            // return "Success";
                        }

                    }
                }
                return htmldata;
            }
        }

        public static string ConvertDataTableToHTML(DataTable dt)
        {
            string html = "<table>";
            //add header row
            html += "<tr>";
            for (int i = 0; i < dt.Columns.Count; i++)
                html += "<td>" + dt.Columns[i].ColumnName + "</td>";
            html += "</tr>";
            //add rows
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                html += "<tr>";
                for (int j = 0; j < dt.Columns.Count; j++)
                    html += "<td>" + dt.Rows[i][j].ToString() + "</td>";
                html += "</tr>";
            }
            html += "</table>";
            return html;
        }


        [AllowAnonymous]
        [HttpGet]
        [Route("GetArthmateStateCode")]
        public async Task<CommonResponseDc> GetArthmateStateCode()
        {
            CommonResponseDc results = new CommonResponseDc();
            List<LeadBackgroundRunDc> LeadPageData = new List<LeadBackgroundRunDc>();
            using (var authContext = new AuthContext())
            {
                results.Data = authContext.Database.SqlQuery<ArthMatePostDc.ArthmateStateCodeDc>($"select State, StateCode from arthmatestatecodes").ToList();

                if (results.Data != null)
                {
                    results.Status = true;
                }
                else
                {
                    results.Msg = "No Data Found";
                    results.Status = true;
                }
            }
            return results;
        }



        [AllowAnonymous]
        [HttpGet]
        [Route("GetLoanJsonData")]
        public List<GenerateLoanDc> GetLoanStatusByLoanId(long LeadMasterId)
        {
            GenerateLoanDc results = new GenerateLoanDc();
            List<GenerateLoanDc> LoanPageData = new List<GenerateLoanDc>();
            using (var authContext = new AuthContext())
            {
                var Leaddata = authContext.LeadMasters.Where(x => x.Id == LeadMasterId).FirstOrDefault();
                var Ascoredata = authContext.AScore.Where(x => x.LeadMasterId == LeadMasterId && x.IsActive == true && x.IsDeleted == false).FirstOrDefault();
                var ColenderData = authContext.CoLenderResponse.Where(x => x.LeadMasterId == LeadMasterId && x.IsActive == true && x.IsDeleted == false).FirstOrDefault();
                var LoanConfigData = authContext.LoanConfiguration.FirstOrDefault();

                GenerateLoanDc loandc = new GenerateLoanDc()
                {
                    a_score_request_id = Ascoredata.request_id,
                    co_lender_assignment_id = ColenderData.co_lender_assignment_id,
                    partner_loan_app_id = Leaddata.partner_loan_app_id,
                    partner_borrower_id = Leaddata.partner_borrower_id,
                    loan_app_id = Leaddata.loan_app_id,
                    borrower_id = Leaddata.borrower_id,
                    partner_loan_id = Leaddata.partner_loan_app_id,
                    marital_status = Leaddata.marital_status,
                    //residence_vintage = Leaddata.residence_vintage,
                    loan_app_date = Leaddata.CreatedDate.ToString("yyyy-MM-dd"),
                    loan_amount_requested = Leaddata.EnquiryAmount.ToString(),
                    //sanction_amount = Leaddata.sanction_amount,
                    //processing_fees_perc = LoanConfigData.processing_fees_perc,
                    //processing_fees_amt = Leaddata.processing_fees_amt,
                    //gst_on_pf_perc = Leaddata.gst_on_pf_perc,
                    //gst_on_pf_amt = Leaddata.gst_on_pf_amt,
                    //conv_fees = Leaddata.conv_fees,

                    //working

                    // insurance_amount = Leaddata.insurance_amount,
                    //net_disbur_amt = Leaddata.,
                    int_type = "Reducing",
                    loan_int_rate = "",//Leaddata.loan_int_rate,
                                       //loan_int_amt = Leaddata.loan_int_amt,
                    broken_period_int_amt = "0",//Leaddata.broken_period_int_amt,
                    repayment_type = "Monthly",
                    tenure_type = "Month",// Leaddata.tenure_type,
                    tenure = Leaddata.tenure.ToString(),//Leaddata.tenure,
                    first_inst_date = "",//Leaddata.first_inst_date,
                                         //emi_amount = Leaddata.emi_amount,
                                         //emi_count = Leaddata.emi_count,
                                         //final_approve_date = Leaddata.final_approve_date,
                    final_remarks = "Done",
                    borro_bank_name = Leaddata.borro_bank_name,
                    borro_bank_acc_num = Leaddata.borro_bank_acc_num,
                    borro_bank_ifsc = Leaddata.borro_bank_ifsc,
                    borro_bank_account_holder_name = Leaddata.first_name + Leaddata.last_name,
                    borro_bank_account_type = "Current",
                    //bene_bank_name = Leaddata.bene_bank_name,
                    //bene_bank_acc_num = Leaddata.bene_bank_acc_num,
                    //bene_bank_ifsc = Leaddata.bene_bank_ifsc,
                    //bene_bank_account_holder_name = Leaddata.bene_bank_account_holder_name,
                    //bene_bank_account_type = Leaddata.bene_bank_account_type,
                    //avg_banking_turnover_6_months = Leaddata.avg_banking_turnover_6_months,
                    //abb = Leaddata.abb,
                    //monthly_income = Leaddata.monthly_income,
                    itr_ack_no = Leaddata.itr_ack_no,
                    //bureau_type = Leaddata.bureau_type,
                    //bureau_score = Leaddata.bureau_score,
                    //customer_type_ntc = Leaddata.customer_type_ntc,
                    //foir = Leaddata.foir,
                    //bureau_fetch_date = Leaddata.bureau_fetch_date,
                    //bounces_in_three_month = Leaddata.bounces_in_three_month,

                    business_name = Leaddata.business_name,
                    business_address = Leaddata.bus_add_corr_line1,
                    //working
                    business_city = Leaddata.bus_add_corr_city,
                    business_state = Leaddata.bus_add_corr_state,
                    business_pin_code = Leaddata.bus_add_corr_pincode,
                    business_address_ownership = "Owned",
                    program_type = "Banking",
                    business_entity_type = "",
                    business_pan = Leaddata.bus_pan,
                    gst_number = Leaddata.bus_gstno,
                    //udyam_reg_no = Leaddata.udyam_reg_no,
                    other_business_reg_no = "",//Leaddata.other_business_reg_no,
                                               //business_vintage_overall = Leaddata.business_vintage_overall,
                    txn_avg = "",
                    txn_1 = "",
                    txn_2 = "",
                    txn_3 = "",
                    txn_4 = "",
                    txn_5 = "",
                    txn_6 = "",
                    business_establishment_proof_type = Leaddata.business_establishment_proof_type,
                    co_app_or_guar_name = Leaddata.co_app_name,
                    //co_app_or_guar_dob = Leaddata.dob,
                    //co_app_or_guar_gender = Leaddata.co_app_or_guar_gender,
                    //co_app_or_guar_address = Leaddata.co_app_or_guar_address,
                    co_app_or_guar_mobile_no = Leaddata.co_app_mobile_no,
                    co_app_or_guar_pan = Leaddata.co_app_pan,
                    relation_with_applicant = Leaddata.relation_with_applicant,
                    //co_app_or_guar_bureau_type = Leaddata.co_app_bureau_score,
                    co_app_or_guar_bureau_score = Leaddata.co_app_bureau_score.ToString(),
                    //co_app_or_guar_ntc = Leaddata.co_app_or_guar_ntc,
                    insurance_company = "SK",
                    purpose_of_loan = Leaddata.purpose_of_loan,
                    emi_obligation = ""

                };
                LoanPageData.Add(loandc);
            }
            return LoanPageData;
        }


        #region Loan 

        [HttpGet]
        [Route("GetLoanStatus/{LeadMasterId}")]
        public async Task<CommonResponseDc> GetLoanDetail(long LeadMasterId)
        {
            CommonResponseDc result = new CommonResponseDc();
            using (var authContext = new AuthContext())
            {
                var Loan = await authContext.LeadLoan.Where(x => x.LeadMasterId == LeadMasterId).FirstOrDefaultAsync();
                if (Loan != null)
                {
                    result.Data = Loan;
                    result.Status = true;
                }
                else
                {
                    result.Status = false;
                }

            }
            return result;
        }

        [HttpGet]
        [Route("CheckLoanDetailArthmate/{LeadMasterId}")]
        public async Task<CommonResponseDc> CheckLoanDetailArthmate(long LeadMasterId) // https://uat-apiorigin.arthmate.com/api/loan/{loan_id}
        {
            CommonResponseDc result = new CommonResponseDc();

            using (var authContext = new AuthContext())
            {
                var Loan = await authContext.LeadLoan.Where(x => x.LeadMasterId == LeadMasterId).FirstOrDefaultAsync();
                ArthMateHelper ArthMateHelper = new ArthMateHelper();


            }
            return result;
        }

        [HttpGet]
        [Route("ChangeLoanStatus/{LeadMasterId}")]
        public async Task<CommonResponseDc> ChangeLoanStatus(long LeadMasterId, string Status)  //https://uat-apiorigin.arthmate.com/api/borrowerinfostatusupdate/{loan_id}
        {
            //string Status=;The applicable statuses are open, kyc_data_approved, credit_approved, disbursal_approved

            string ReturnStatus = "";
            int userid = 0;
            ArthMateHelper ArthMateHelper = new ArthMateHelper();
            CommonResponseDc result = new CommonResponseDc();
            using (var authContext = new AuthContext())
            {
                var Loan = await authContext.LeadLoan.Where(x => x.LeadMasterId == LeadMasterId && x.IsActive == true && x.IsDeleted == false).FirstOrDefaultAsync();
                GetLoanDetailsDc LaonStatusApi = await ArthMateHelper.GetLoanById(Loan.loan_id, LeadMasterId, 0);

                string LaonStatus = LaonStatusApi.loanDetails.status;
                if (LaonStatus == "validation_error")
                {
                    ReturnStatus = LaonStatusApi.loanDetails.status;
                    result.Msg = "Status is " + ReturnStatus + " and " + LaonStatusApi.loanDetails.validations.FirstOrDefault().remarks;
                }
                else
                {

                    if (LaonStatus == "open")
                    {
                        var LoanStatusChangeAPI = new LoanStatusChangeAPIReq
                        {
                            loan_id = Loan.loan_id,
                            partner_borrower_id = Loan.partner_borrower_id,
                            partner_loan_id = Loan.partner_loan_id,
                            status = Status,
                            borrower_id = Loan.borrower_id,
                            loan_app_id = Loan.loan_app_id,
                            partner_loan_app_id = Loan.partner_loan_app_id
                        };
                        var LoanStatusChangeAPIresponse = await ArthMateHelper.borrowerinfostatusupdate(LoanStatusChangeAPI, userid, LeadMasterId);

                        result.Data = LoanStatusChangeAPIresponse;
                        result.Msg = LoanStatusChangeAPIresponse.message;
                        result.Status = true;
                        if (LoanStatusChangeAPIresponse.message == "Status Changed")
                        {
                            Loan.status = Status;
                            authContext.Entry(Loan).State = EntityState.Modified;
                            authContext.Commit();

                            LeadActivityProgressesHistory(LeadMasterId, 0, 0, "", "Loan Status Updated Successfully");
                        }
                    }
                    else
                    {
                        ReturnStatus = LaonStatusApi.loanDetails.status;
                        result.Msg = "Status is " + ReturnStatus + " and " + LaonStatusApi.loanDetails.validations.FirstOrDefault().remarks;
                    }
                }
            }

            return result;
        }

        [HttpGet]
        [Route("LoanRepaymentScheduleDetails/{LeadMasterId}")]
        public async Task<CommonResponseDc> LoanRepaymentScheduleDetails(long LeadMasterId) ////https://uat-apiorigin.arthmate.com/api/repayment_schedule/{loan_id}
        {
            var identity = User.Identity as ClaimsIdentity;
            int userid = 0;
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
            ArthMateHelper ArthMateHelper = new ArthMateHelper();

            CommonResponseDc result = new CommonResponseDc();
            using (var authContext = new AuthContext())
            {
                var Loan = await authContext.LeadLoan.Where(x => x.LeadMasterId == LeadMasterId).FirstOrDefaultAsync();
                if (Loan != null)
                {
                    //to check for disbursment ...before and after (different api for after disbursment)
                    var Postrepayment_schedule = new Postrepayment_scheduleDc
                    {
                        loan_id = Loan.loan_id,
                        product_id = Loan.product_id,
                        company_id = Loan.company_id.Value,
                    };
                    var RepaymentAllResponse = await ArthMateHelper.repayment_schedule(Postrepayment_schedule, userid, LeadMasterId, false);
                    result.Data = RepaymentAllResponse;
                    result.Msg = RepaymentAllResponse.message;

                    result.Status = true;

                }
            }
            return result;
        }

        #endregion

        #region eSign work
        [HttpGet]
        [Route("eSignSessionAsync")]
        public async Task<CommonResponseDc> eSignSessionAsyncApi(long LeadMasterId)
        {
            CommonResponseDc res = new CommonResponseDc();
            res = await eSignSessionAsync(LeadMasterId);
            return res;
        }
        public async Task<CommonResponseDc> eSignSessionAsync(long LeadMasterId)
        {
            //string fileurl = "C:/Users/SK/Pictures/SanctionAndLbaBusinessLoan70_12112023044424.pdf";

            TextFileLogHelper.TraceLog("esign session : => Phase 1");
            CommonResponseDc res = new CommonResponseDc();
            try
            {
                res = await eSignSessionAsyncMain(LeadMasterId);
                if (res.Status == true)
                {
                    string docId = res.NameOnCard;

                    var eSignDocStatus = await ESignDocumentasync(LeadMasterId, docId);
                    if (eSignDocStatus.result != null)
                    {
                        using (AuthContext context = new AuthContext())
                        {
                            var sanctionLeadLoanDocuments = context.LeadDocument.Where(x => x.LeadMasterId == LeadMasterId && x.OtherInfo == sLoan_sanction_letter && x.IsActive == true && x.IsDeleted == false).FirstOrDefault();

                            if (sanctionLeadLoanDocuments != null)
                            {
                                sanctionLeadLoanDocuments.IsActive = false;
                                sanctionLeadLoanDocuments.IsDeleted = true;
                                context.Entry(sanctionLeadLoanDocuments).State = EntityState.Modified;
                                //context.Commit();
                            }

                            long sanctionDocumentMasterId = context.ArthMateDocumentMasters.Where(x => x.DocumentName == sLoan_sanction_letter).FirstOrDefault().Id;
                            LeadLoanDocument sanctionleadLoanDocument = new LeadLoanDocument()
                            {
                                FrontFileUrl = eSignDocStatus.FileUrl,
                                BackFileUrl = null,
                                CreatedBy = 0,
                                CreatedDate = DateTime.Now,
                                DocumentNumber = "",
                                DocumentMasterId = sanctionDocumentMasterId,
                                IsActive = true,
                                LeadMasterId = LeadMasterId,
                                OtherInfo = sLoan_sanction_letter,
                                IsVerified = true,
                                IsDeleted = false
                            };
                            context.LeadDocument.Add(sanctionleadLoanDocument);

                            //------- s ----------------------------------------------------
                            var LeadLoanDocuments = context.LeadDocument.Where(x => x.LeadMasterId == LeadMasterId && x.OtherInfo == sSigned_loan_sanction_letter && x.IsActive == true && x.IsDeleted == false).FirstOrDefault();

                            if (LeadLoanDocuments != null)
                            {
                                LeadLoanDocuments.IsActive = false;
                                LeadLoanDocuments.IsDeleted = true;
                                context.Entry(LeadLoanDocuments).State = EntityState.Modified;
                                //context.Commit();
                            }

                            long DocumentMasterId = context.ArthMateDocumentMasters.Where(x => x.DocumentName == sSigned_loan_sanction_letter).FirstOrDefault().Id;
                            LeadLoanDocument leadLoanDocument = new LeadLoanDocument()
                            {
                                FrontFileUrl = eSignDocStatus.FileUrl,
                                CreatedBy = 0,
                                CreatedDate = DateTime.Now,
                                DocumentNumber = "",
                                DocumentMasterId = DocumentMasterId,
                                IsActive = true,
                                LeadMasterId = LeadMasterId,
                                OtherInfo = sSigned_loan_sanction_letter,
                                IsVerified = true,
                                IsDeleted = false
                            };
                            context.LeadDocument.Add(leadLoanDocument);

                            //--------e---------------------------------------


                            //if (LeadLoandata != null)
                            //{
                            //    //LeadLoandata.UrlSlaDocument = LogoUrl;//customer sla Document Accept copy pdf
                            //    LeadLoandata.UrlSlaUploadSignedDocument = eSignDocStatus.FileUrl;//customer sla Document signed copy pdf
                            //    context.Entry(LeadLoandata).State = EntityState.Modified;
                            //}
                            context.Commit();

                            //post Doc To Arthmate 
                            //var DocStatus = ArthmatePostDoc(Leadmasterid, sLoan_sanction_letter);
                            //var datwa = ArthmatePostDoc(Leadmasterid, sSigned_loan_sanction_letter);

                            LeadActivityProgressesHistory(LeadMasterId, 0, 0, "ESignDocumentasync works", "ESignDocument Done");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return res;
        }


        public async Task<CommonResponseDc> eSignSessionAsyncMain(long LeadMasterId)
        {
            //string fileurl = "C:/Users/SK/Pictures/SanctionAndLbaBusinessLoan70_12112023044424.pdf";

            try
            {
                TextFileLogHelper.TraceLog("eSignSessionAsyncMain : => Phase 2");

                CommonResponseDc res = new CommonResponseDc();
                using (var db = new AuthContext())
                {
                    var LeadId = new SqlParameter("@leadMasterId", LeadMasterId);
                    var data = db.Database.SqlQuery<AgreementDetailDc>("EXEC GetAgreementByLeadId @leadMasterId", LeadId).FirstOrDefault();

                    TextFileLogHelper.TraceLog("eSignSessionAsyncMain : => Phase 3");

                    if (data != null && data.FrontFileUrl != null)
                    {
                        var pdfPath = string.Concat(HttpRuntime.AppDomainAppPath, data.FrontFileUrl.Replace("/", "\\"));
                        //pdfPath = fileurl;
                        byte[] pdfBytes = File.ReadAllBytes(pdfPath);
                        string pdfBase64 = Convert.ToBase64String(pdfBytes);
                        ArthMateHelper helper = new ArthMateHelper();

                        eSignAgreementDc esign = new eSignAgreementDc();
                        esign.document = pdfBase64;
                        esign.name = data.name;
                        esign.email = data.email;
                        esign.gender = "";
                        esign.yob = "";
                        esign.mobileNo = data.mobileNo;
                        esign.LeadMasterId = LeadMasterId;
                        TextFileLogHelper.TraceLog("eSignSessionAsyncMain : => Phase 4");
                        var response = await helper.eSignSessionAsync(esign);
                        if (response.statusCode == 101)
                        {
                            var esigndata = db.eSignDetail.Where(x => x.documentId == response.result.documentId).FirstOrDefault();
                            if (esigndata != null)
                            {
                                esigndata.IsActive = false;
                                esigndata.IsDeleted = true;
                                db.Entry(esigndata).State = EntityState.Modified;
                                //db.Commit();
                            }

                            eSignDetail doc = new eSignDetail();
                            doc.LeadDocumentId = data.LeadDocumentid;
                            doc.LeadMasterId = LeadMasterId;
                            doc.documentId = response.result.documentId;
                            doc.signingURL = response.result.signingDetails[0].signUrl;
                            doc.eSigned = false;
                            doc.requestId = response.requestId;
                            doc.IsActive = true;
                            doc.IsDeleted = false;
                            doc.CreatedBy = (int)LeadMasterId;
                            doc.CreatedDate = DateTime.Now;
                            doc.ModifiedDate = null;
                            doc.ModifiedBy = null;
                            db.eSignDetail.Add(doc);
                            db.Commit();

                            res.Data = response.result.signingDetails[0].signUrl;
                            res.NameOnCard = response.result.documentId;
                            res.Status = true;
                            res.Msg = "Success";
                        }
                        else
                        {
                            res.Msg = "Failed";
                            res.Status = false;
                        }
                    }
                    else
                    {
                        res.Msg = "failed";
                        res.Status = false;
                    }

                    return res;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion

        [HttpGet]
        [Route("CreateSubscription")]
        public async Task CreateSubscription(long Leadid)
        {
            ArthMateHelper arthMateHelper = new ArthMateHelper();
            using (var Context = new AuthContext())
            {
                var leadmasters = Context.LeadMasters.Where(x => x.Id == Leadid && x.IsActive == true && x.IsDeleted == false).FirstOrDefault();
                var leadloans = Context.LeadLoan.Where(x => x.Id == Leadid && x.IsActive == true && x.IsDeleted == false).FirstOrDefault();
                PlanInfo planInfo = new PlanInfo
                {
                    type = "",
                    planName = "",
                    maxAmount = 0,
                    maxCycles = 0,
                    intervals = 0,
                    intervalType = "",
                    recurringAmount = 0
                };
                PayerAccountDetails payerAccountDetails = new PayerAccountDetails
                {
                    accountHolderName = "",
                    accountNumber = "",
                    accountType = "",
                    bankId = "",
                    ifsc = ""
                };
                Notes notes = new Notes
                {
                    key1 = "",
                    key2 = "",
                    key3 = "",
                    key4 = ""
                };
                List<string> notifyChannelList = new List<string>
                {

                };

                CreateSubscriptionwithPlanInfoDc subscriptionwithPlanInfoDc = new CreateSubscriptionwithPlanInfoDc
                {
                    subscriptionId = "subscription_" + DateTime.Now.DayOfWeek.ToString() + "_" + DateTime.Now,
                    customerName = leadmasters.first_name + " " + leadmasters.last_name,
                    customerEmail = leadmasters.email_id,
                    customerPhone = leadmasters.MobileNo,
                    authAmount = leadloans.emi_amount != null && Convert.ToDouble(leadloans.emi_amount) > 0 ? Convert.ToDouble(leadloans.emi_amount) : 1,
                    expiresOn = new Date { value = "" },
                    returnUrl = "",
                    firstChargeDate = new Date { value = "" },
                    planInfo = planInfo,
                    notificationChannels = notifyChannelList,
                    tpvEnabled = false,
                    linkExpiry = 0,
                    refundAuthAmount = false,
                    payerAccountDetails = payerAccountDetails,
                    notes = notes
                };

                var createSubscriptionwithPlanInfo = arthMateHelper.CreateSubscriptionwithPlanInfo(subscriptionwithPlanInfoDc, 0, Leadid);



            }
        }


        #region For Sla Agreement
        [AllowAnonymous]
        [HttpPost]
        [Route("UploadSignedSla")] //Roshni
        public IHttpActionResult UploadSignedSla(long leadmasterid)
        {
            string LogoUrl = "";
            string msg = "";
            bool status = false;

            try
            {
                if (HttpContext.Current.Request.Files.AllKeys.Any())
                {
                    var httpPostedFile = HttpContext.Current.Request.Files["file"];
                    if (httpPostedFile != null)
                    {
                        if (!Directory.Exists(System.Web.HttpContext.Current.Server.MapPath("~/ArthmateDocument/SlaDocument/SlaSignedDocument")))
                            Directory.CreateDirectory(HttpContext.Current.Server.MapPath("~/ArthmateDocument/SlaDocument/SlaSignedDocument"));

                        string extension = Path.GetExtension(httpPostedFile.FileName);
                        string fileName = httpPostedFile.FileName.Substring(0, httpPostedFile.FileName.LastIndexOf('.')) + DateTime.Now.ToString("ddMMyyyyHHmmss") + extension;
                        LogoUrl = Path.Combine(HttpContext.Current.Server.MapPath("~/ArthmateDocument/SlaDocument/SlaSignedDocument"), fileName);
                        httpPostedFile.SaveAs(LogoUrl);
                        AngularJSAuthentication.Common.Helpers.FileUploadHelper.Upload(fileName, "~/ArthmateDocument/SlaDocument/SlaSignedDocument", LogoUrl);

                        LogoUrl = string.Format("{0}://{1}{2}/{3}", new Uri((HttpContext.Current.Request.UrlReferrer != null ? HttpContext.Current.Request.UrlReferrer.AbsoluteUri : HttpContext.Current.Request.Url.AbsoluteUri)).Scheme
                                                                 , HttpContext.Current.Request.Url.DnsSafeHost
                                                                 , (HttpContext.Current.Request.Url.Port != 80 && HttpContext.Current.Request.Url.Port != 443 ? ":" + HttpContext.Current.Request.Url.Port : "")
                                                                 , "/ArthmateDocument/SlaDocument/SlaSignedDocument/" + fileName);
                        LogoUrl = "/ArthmateDocument/SlaDocument/SlaSignedDocument/" + fileName;

                        using (AuthContext context = new AuthContext())
                        {
                            var LeadLoandata = context.LeadLoan.Where(x => x.LeadMasterId == leadmasterid && x.IsActive == true && x.IsDeleted == false).FirstOrDefault();
                            if (LeadLoandata != null)
                            {
                                if (LeadLoandata.status != LoanStatus)//"kyc_data_approved"
                                {
                                    return Created<string>("", "Documnet Not Uploaded due to Loan stats should be 'kyc_data_approved'");
                                }
                            }

                            //long arthmastedoc = context.ArthMateDocumentMasters.Where(x => x.DocumentName == "agreement").FirstOrDefault().Id;
                            //var Loandata = context.LeadDocument.Where(x => x.LeadMasterId == leadmasterid && x.DocumentMasterId == arthmastedoc).FirstOrDefault();
                            //if (Loandata != null)
                            //{
                            //    Loandata.BackFileUrl = LogoUrl;  //customer sla Document signed copy pdf
                            //    context.Entry(Loandata).State = EntityState.Modified;

                            var sanctionLeadLoanDocuments = context.LeadDocument.Where(x => x.LeadMasterId == leadmasterid && x.OtherInfo == sLoan_sanction_letter && x.IsActive == true && x.IsDeleted == false).FirstOrDefault();

                            if (sanctionLeadLoanDocuments != null)
                            {
                                //LeadLoanDocuments.ModifiedBy = 11;
                                //LeadLoanDocuments.ModifiedDate = DateTime.Now;
                                //LeadLoanDocuments.FrontFileUrl = newfilepath;
                                sanctionLeadLoanDocuments.IsActive = false;
                                sanctionLeadLoanDocuments.IsDeleted = true;
                                context.Entry(sanctionLeadLoanDocuments).State = EntityState.Modified;
                                //context.Commit();
                            }

                            long sanctionDocumentMasterId = context.ArthMateDocumentMasters.Where(x => x.DocumentName == sLoan_sanction_letter).FirstOrDefault().Id;
                            LeadLoanDocument sanctionleadLoanDocument = new LeadLoanDocument()
                            {
                                FrontFileUrl = LogoUrl,
                                BackFileUrl = null,
                                CreatedBy = 0,
                                CreatedDate = DateTime.Now,
                                DocumentNumber = "",
                                DocumentMasterId = sanctionDocumentMasterId,
                                IsActive = true,
                                LeadMasterId = leadmasterid,
                                OtherInfo = sLoan_sanction_letter,
                                IsVerified = true,
                                IsDeleted = false
                            };
                            context.LeadDocument.Add(sanctionleadLoanDocument);

                            //------- s ----------------------------------------------------
                            var LeadLoanDocuments = context.LeadDocument.Where(x => x.LeadMasterId == leadmasterid && x.OtherInfo == sSigned_loan_sanction_letter && x.IsActive == true && x.IsDeleted == false).FirstOrDefault();

                            if (LeadLoanDocuments != null)
                            {
                                LeadLoanDocuments.IsActive = false;
                                LeadLoanDocuments.IsDeleted = true;
                                context.Entry(LeadLoanDocuments).State = EntityState.Modified;
                                //context.Commit();
                            }

                            long DocumentMasterId = context.ArthMateDocumentMasters.Where(x => x.DocumentName == sSigned_loan_sanction_letter).FirstOrDefault().Id;
                            LeadLoanDocument leadLoanDocument = new LeadLoanDocument()
                            {
                                FrontFileUrl = LogoUrl,
                                CreatedBy = 0,
                                CreatedDate = DateTime.Now,
                                DocumentNumber = "",
                                DocumentMasterId = DocumentMasterId,
                                IsActive = true,
                                LeadMasterId = leadmasterid,
                                OtherInfo = sSigned_loan_sanction_letter,
                                IsVerified = true,
                                IsDeleted = false
                            };
                            context.LeadDocument.Add(leadLoanDocument);

                            //--------e---------------------------------------

                            var arthmateSlaLbaStamp = context.ArthmateSlaLbaStampDetail.FirstOrDefault(x => x.LeadmasterId == leadmasterid && x.IsActive && x.IsDeleted == false);
                            if (arthmateSlaLbaStamp != null)
                            {
                                arthmateSlaLbaStamp.IsStampUsed = true;
                                context.Entry(arthmateSlaLbaStamp).State = EntityState.Modified;
                            }
                            if (LeadLoandata != null)
                            {
                                //LeadLoandata.UrlSlaDocument = LogoUrl;//customer sla Document Accept copy pdf
                                LeadLoandata.UrlSlaUploadSignedDocument = LogoUrl;//customer sla Document signed copy pdf
                                context.Entry(LeadLoandata).State = EntityState.Modified;
                            }
                            context.Commit();

                            //post Doc To Arthmate 
                            var DocStatus = ArthmatePostDoc(leadmasterid, sLoan_sanction_letter);
                            var data = ArthmatePostDoc(leadmasterid, sSigned_loan_sanction_letter);

                            msg = "Saved Successfully";
                            status = true;

                            LeadActivityProgressesHistory(leadmasterid, 0, 0, "SLA Letter Uploaded ", "Sanction And Lba (SLA) Letter Uploaded Successfully");

                        }

                    }
                }

                return Created<string>(LogoUrl, msg);
            }
            catch (Exception ex)
            {
                return Created<string>("", ex.Message.ToString());
            }
        }


        #endregion


        #region   Penny Drop API to verify bank account holder name basis IFSC and account number. 
        [AllowAnonymous]
        [HttpPost]
        [Route("PennyDrop")] //Arthmate Penny drop API
        public async Task<CommonResponseDc> PennyDrop(long Leadmasterid)
        {
            CommonResponseDc res = new CommonResponseDc();
            if (Leadmasterid > 0)
            {
                using (var db = new AuthContext())
                {
                    var data = db.LeadMasters.Where(x => x.Id == Leadmasterid).FirstOrDefault();
                    if (data != null)
                    {
                        PennyDropReqJson pennyDropReqJson = new PennyDropReqJson()
                        {
                            account_number = data.borro_bank_acc_num,
                            ifsc = data.borro_bank_ifsc,
                            loan_app_id = data.loan_app_id
                        };
                        ArthMateHelper ArthMateHelper = new ArthMateHelper();
                        var resdata = await ArthMateHelper.PennyDropVerification(pennyDropReqJson, data.Id);

                        res.Msg = resdata.success == true ? "success" : resdata.message;
                        res.Status = resdata.success;

                        if (resdata.success == false)
                        {
                            ScreenApproveRejectDc obj = new ScreenApproveRejectDc()
                            {
                                Comment = "Auto Reject:" + resdata.message,
                                DocumentName = "bank_stmnts",
                                isApprove = false,
                                LeadmasterId = Leadmasterid,
                                SequenceNo = 6

                            };
                            var rject = VerifyUploadedDocument(obj);
                            { LeadActivityProgressesHistory(Leadmasterid, 0, 0, "Penny Drop Api", "Penny Drop (Bank Information) not verified"); }
                        }
                        else if (resdata.success == true)
                        { LeadActivityProgressesHistory(Leadmasterid, 0, 0, "Penny Drop Api", "Penny Drop (Bank Information) verified Successfully"); }

                    }
                    else
                    {
                        res.Msg = "Data Not Found in Lead!";
                    }
                }
            }

            return res;
        }
        #endregion

        [AllowAnonymous]
        [HttpPost]
        [Route("LoanNach")]
        public async Task<CommonResponseDc> LoanNach(string UMRN, long Leadmasterid)
        {

            string Remarks = "UMRN Not Verified";
            var identity = User.Identity as ClaimsIdentity;
            int userid = 0;
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

            CommonResponseDc res = new CommonResponseDc();
            if (Leadmasterid > 0)
            {
                using (var db = new AuthContext())
                {
                    var data = db.LeadLoan.Where(x => x.LeadMasterId == Leadmasterid && x.IsActive == true && x.IsDeleted == false).FirstOrDefault();
                    if (data != null)
                    {

                        LoanNachAPIDc obj = new LoanNachAPIDc()
                        {
                            umrn = UMRN,
                        };
                        ArthMateHelper ArthMateHelper = new ArthMateHelper();
                        var resdata = await ArthMateHelper.LoanNachPatchAPI(obj, data.LeadMasterId.Value, data.loan_id);
                        if (resdata.success)
                        {
                            data.UMRN = UMRN;
                            data.ModifiedDate = DateTime.Now;
                            data.ModifiedBy = userid;
                            db.Entry(data).State = EntityState.Modified;
                            db.Commit();

                            Remarks = "UMRN verified Successfully";
                        }

                        res.Msg = resdata.message;
                        res.Status = resdata.success;

                        LeadActivityProgressesHistory(Leadmasterid, 0, 0, "UMRN Api", Remarks);
                    }
                    else
                    {
                        res.Msg = "Data Not Found in Loan!";
                    }
                }
            }

            return res;
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("CompositeDisbursement")] //not used
        public async Task<CommonResponseDc> CompositeDisbursementAPI(long Leadmasterid)
        {
            CommonResponseDc res = new CommonResponseDc();
            if (Leadmasterid > 0)
            {
                using (var db = new AuthContext())
                {

                    var Leadid = new SqlParameter("@LeadmasterId", Leadmasterid);
                    var loan = db.Database.SqlQuery<LeadLoanDetailDc>("EXEC GetLeadLoanDetailByLeadId @LeadmasterId", Leadid).FirstOrDefault();

                    if (loan != null)
                    {
                        CompositeDisbursementDc obj = new CompositeDisbursementDc()
                        {
                            loan_id = loan.loan_id,
                            loan_app_id = loan.loan_app_id,
                            borrower_id = loan.borrower_id,
                            partner_loan_id = loan.partner_loan_id,
                            partner_borrower_id = loan.partner_borrower_id,
                            borrower_mobile = loan.borrower_mobile,
                            ifsc_code = loan.ifsc_code,
                            account_no = Convert.ToInt64(loan.account_no),
                            txn_date = indianTime.ToString("yyyy-MM-dd"),
                            sanction_amount = Convert.ToDouble(loan.sanction_amount),
                            net_disbur_amt = Convert.ToDouble(loan.net_disbur_amt)
                        };
                        ArthMateHelper ArthMateHelper = new ArthMateHelper();
                        var resdata = await ArthMateHelper.CompositeDisbursementAPI(obj, Leadmasterid);

                    }
                    else
                    {
                        res.Msg = "Data Not Found in Loan!";
                    }
                }
            }

            return res;
        }

        //[AllowAnonymous]
        //[HttpGet]
        //[Route("CompositeDrawDown")]
        //public async Task<CommonResponseDc> CompositeDrawDownAPI(long Leadmasterid)
        //{
        //    CommonResponseDc res = new CommonResponseDc();
        //    if (Leadmasterid > 0)
        //    {
        //        using (var db = new AuthContext())
        //        {

        //            var Leadid = new SqlParameter("@LeadmasterId", Leadmasterid);
        //            var loan = db.Database.SqlQuery<LeadLoanDetailDc>("EXEC GetLeadLoanDetailByLeadId @LeadmasterId", Leadid).FirstOrDefault();

        //            if (loan != null)
        //            {
        //                CompositeDrawDownDc obj = new CompositeDrawDownDc()
        //                {
        //                    loan_id = loan.loan_id,
        //                    loan_app_id = loan.loan_app_id,
        //                    borrower_id = loan.borrower_id,
        //                    partner_loan_id = loan.partner_loan_id,
        //                    partner_borrower_id = loan.partner_borrower_id,
        //                    borrower_mobile = loan.borrower_mobile,
        //                    partner_loan_app_id = loan.loan_app_id,
        //                    drawadown_request_date = indianTime.ToString("yyyy-MM-dd"),
        //                    //drawdown_amount = null,
        //                    //no_of_emi = null,
        //                    //net_drawdown_amount = null,
        //                    //usage_fees = null,
        //                    //usage_fees_including_gst = null,
        //                };
        //                ArthMateHelper ArthMateHelper = new ArthMateHelper();
        //                var resdata = await ArthMateHelper.CompositeDrawDownAPI(obj, Leadmasterid);

        //            }
        //            else
        //            {
        //                res.Msg = "Data Not Found in Loan!";
        //            }
        //        }
        //    }

        //    return res;
        //}


        [AllowAnonymous]
        [HttpGet]
        [Route("CompositeDisbursementWebhook")]//not used
        public async Task<CommonResponseDc> CompositeDisbursementWebhook(long Leadmasterid)
        {
            CommonResponseDc res = new CommonResponseDc();
            if (Leadmasterid > 0)
            {
                using (var db = new AuthContext())
                {

                    var Leadid = new SqlParameter("@LeadmasterId", Leadmasterid);
                    //var loan = db.Database.SqlQuery<LeadLoanDetailDc>("EXEC GetLeadLoanDetailByLeadId @LeadmasterId", Leadid).FirstOrDefault();
                    var compDisbursResponse = db.CompositeDisbursementWebhookResponse.Where(x => x.LeadMasterId == Leadmasterid && x.IsActive == true && x.IsDeleted == false).FirstOrDefault();
                    if (compDisbursResponse != null)
                    {
                        CompositeDisbursementWebhookDc obj = new CompositeDisbursementWebhookDc();
                        obj.event_key = "disbursement";
                        obj.CallBackData = new CallBackData
                        {
                            loan_id = compDisbursResponse.loan_id,
                            status_code = AppConstants.Environment == "prod" ? "3" : "30",
                            partner_loan_id = compDisbursResponse.partner_loan_id,
                            net_disbur_amt = compDisbursResponse.net_disbur_amt,
                            utr_number = compDisbursResponse.utr_number,
                            utr_date_time = compDisbursResponse.utr_date_time,
                            txn_id = compDisbursResponse.txn_id,
                        };

                        ArthMateHelper ArthMateHelper = new ArthMateHelper();
                        var resdata = await ArthMateHelper.CompositeDisbursementWebhook(obj, Leadmasterid);

                    }
                    else
                    {
                        res.Msg = "Data Not Found in Loan!";
                    }
                }
            }

            return res;
        }


        [AllowAnonymous]
        [HttpGet]
        [Route("DeleteLeadSkCode")]
        public string DeleteLeadSkCode(string SkCode)
        {
            string result = null;
            using (AuthContext db = new AuthContext())
            {
                if (SkCode != null)
                {
                    var SkCodeId = new SqlParameter("@SkCode", SkCode);

                    result = db.Database.SqlQuery<string>("EXEC [Arthmate].[DeleteSkCode] @SkCode", SkCodeId).FirstOrDefault();
                }
                else
                {
                    result = "Skcode Not Exists.";
                }

            }
            return result;
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("GetDisbursement")]
        public async Task<CommonResponseDc> GetDisbursementAPI(long Leadmasterid)
        {
            var result = new CommonResponseDc();
            if (Leadmasterid > 0)
            {
                using (var db = new AuthContext())
                {
                    var Loan = db.LeadLoan.Where(x => x.LeadMasterId == Leadmasterid && x.IsActive == true && x.IsDeleted == false).FirstOrDefault();
                    if (Loan != null)
                    {
                        var ArthmateDisbursement = db.ArthmateDisbursements.Where(x => x.loan_id == Loan.loan_id).FirstOrDefault();
                        if (ArthmateDisbursement != null)
                        {
                            result.Data = ArthmateDisbursement;
                            result.Status = true;
                        }
                        else
                        {
                            ArthMateHelper ArthMateHelper = new ArthMateHelper();
                            var resdata = await ArthMateHelper.GetDisbursementAPI(Loan.loan_id, Leadmasterid);
                            if (resdata != null && resdata.success)
                            {
                                db.ArthmateDisbursements.Add(new ArthmateDisbursement
                                {
                                    loan_id = resdata.data.loan_id,
                                    partner_loan_id = resdata.data.partner_loan_id,
                                    net_disbur_amt = resdata.data.net_disbur_amt,
                                    utr_date_time = resdata.data.utr_date_time,
                                    utr_number = resdata.data.utr_number,
                                    status_code = resdata.data.status_code,
                                    CreatedDate = DateTime.Now

                                });
                                db.Commit();
                                result.Data = Mapper.Map(resdata.data).ToANew<ArthmateDisbursement>();
                                result.Status = true;

                                //LeadActivityProgressesHistory(Leadmasterid, 0, 0, "GetDisbursement Api", "");
                            }
                            else
                            {
                                result.Msg = resdata.message;
                            }
                        }
                    }
                    else
                    {
                        result.Msg = "Loan Not Generated";
                        result.Status = false;
                    }
                }
            }
            return result;
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("LeadActivityProgressesHistory")]
        public string LeadActivityProgressesHistoryAPI(long Leadmasterid, long sequenceNo, long? ActivityMasterId, string ActivityName, string Comments)
        {

            return LeadActivityProgressesHistory(Leadmasterid, sequenceNo, ActivityMasterId, ActivityName, Comments);

        }

        public string LeadActivityProgressesHistory(long Leadmasterid, long sequenceNo, long? ActivityMasterId, string ActivityName, string Comments)
        {
            //LeadActivityProgressesHistory
            string save = "";
            long ArthMateActivitySequenceId = 0;
            string ScreenName = "";

            var identity = User.Identity as ClaimsIdentity;
            int userid = 0;
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

            using (AuthContext db = new AuthContext())
            {
                if (sequenceNo > 0 && ActivityMasterId == 0)
                {
                    var arthSequense = db.ArthMateActivitySequence.Where(x => x.SequenceNo == sequenceNo && x.IsActive == true && x.IsDeleted == false).FirstOrDefault();
                    if (arthSequense != null)
                    {
                        ArthMateActivitySequenceId = arthSequense.Id;
                        ScreenName = arthSequense.ScreenName;
                    }
                }

                // LeadActivityProgressesHistory leadActivityProgressesHistories = new LeadActivityProgressesHistory();
                LeadActivityProgressesHistory leadActivityHistory = new LeadActivityProgressesHistory()
                {
                    LeadMasterId = Leadmasterid,
                    ActivityMasterId = ActivityMasterId == 0 ? ArthMateActivitySequenceId : ActivityMasterId,
                    ActivityName = ActivityName == "" ? ScreenName : ActivityName,
                    Comments = Comments,
                    CreatedBy = userid,//123
                    CreatedDate = DateTime.Now,
                    ModifiedBy = null,
                    ModifiedDate = null,
                    IsActive = true,
                    IsDeleted = false

                };
                db.leadActivityProgressesHistories.Add(leadActivityHistory);
                db.Commit();
                save = "Record Save Successfully";

            }
            return save;

        }


        [AllowAnonymous]
        [HttpGet]
        [Route("GetRateOfInterest")]
        public LoanInsuranceConfiguration GetRateOfInterest(int tenure)
        {
            LoanInsuranceConfiguration loanInsuranceConfiguration = new LoanInsuranceConfiguration();
            using (AuthContext context = new AuthContext())
            {
                loanInsuranceConfiguration = context.LoanInsuranceConfiguration.Where(x => x.IsActive == true && x.IsDeleted == false && x.MonthDuration >= tenure).FirstOrDefault();

            }
            if (loanInsuranceConfiguration != null)
            {
                return loanInsuranceConfiguration;

            }
            else
                return null;

        }

        [AllowAnonymous]
        [HttpGet]
        [Route("UpdateProcessBack")]
        public string UpdateProcessBack(long LeadmasterID, string ScreenName)
        {
            string result = "";
            using (AuthContext context = new AuthContext())
            {
                var screenmae = new SqlParameter("@ScreenName", ScreenName);
                var leadid = new SqlParameter("@LeadMasterId", LeadmasterID);
                result = context.Database.SqlQuery<string>("EXEC [Arthmate].[UpdateProcessBack] @ScreenName,@LeadMasterId", screenmae, leadid).FirstOrDefault();
            }
            return result;
        }
        [AllowAnonymous]
        [HttpGet]
        [Route("LeadActivityProgressStatus")]
        public async Task<CommonResponseDc> LeadActivityProgressStatus(long LeadMasterId)
        {
            CommonResponseDc res = new CommonResponseDc();
            using (var db = new AuthContext())
            {
                var leadid = new SqlParameter("@LeadMasterId", LeadMasterId);
                var result = await db.Database.SqlQuery<LeadActivityProgressStatusDc>("EXEC LeadActivityProgressStatus @LeadMasterId", leadid).ToListAsync();
                if (result != null && result.Any())
                {
                    res.Data = result;
                    res.Status = true;
                }
                else
                {
                    res.Status = false;
                    res.Msg = "No Data Found";
                }
            }
            return res;
        }



        [HttpGet]
        [Route("UpdateCeplrBankListJob")]
        public async Task<CommonResponseDc> UpdateCeplrBankListJob()
        {
            CommonResponseDc commonResponseDc = new CommonResponseDc();
            commonResponseDc = await CeplrBankList();
            return commonResponseDc;
        }

        [HttpGet]
        [Route("GetDisbursedLoanDetail")]
        [AllowAnonymous]
        public async Task<RePaymentScheduleDataDc> GetDisbursedLoanDetail(long Leadmasterid)
        {
            RePaymentScheduleDataDc response = new RePaymentScheduleDataDc();
            using (var db = new AuthContext())
            {
                var leadid = new SqlParameter("@LeadMasterId", Leadmasterid);
                var result = db.Database.SqlQuery<DisbursedDetailDc>("Exec GetDisbursedLoanDetail @LeadMasterId", leadid).FirstOrDefault();
                if (result != null)
                {
                    Postrepayment_scheduleDc reqdata = new Postrepayment_scheduleDc()
                    {
                        company_id = result.company_id,
                        loan_id = result.loan_id,
                        product_id = result.product_id
                    };
                    ArthMateHelper ArthMateHelper = new ArthMateHelper();
                    var res = await ArthMateHelper.repayment_schedule(reqdata, 0, Leadmasterid, false);
                    if (res != null && res.success && res.data.rows.Count > 0)
                    {
                        List<repay_scheduleDetails> list = new List<repay_scheduleDetails>();
                        list.AddRange(res.data.rows);
                        response.rows = list;
                        response.MonthlyEMI = result.MonthlyEMI;
                        response.InsurancePremium = result.InsurancePremium;
                        response.sanction_amount = result.sanction_amount;
                        response.borro_bank_acc_num = result.borro_bank_acc_num;
                    }
                }

            }
            return response;
        }

        [HttpGet]
        [Route("InsertRepaymentScheduleData")]
        [AllowAnonymous]
        public async Task<RepaymentSchedule> InsertRepaymentScheduleData(long Leadmasterid)
        {
            RepaymentSchedule ReData = new RepaymentSchedule();
            RePaymentScheduleDataDc response = new RePaymentScheduleDataDc();
            repay_scheduleDc Repyment_ScduleData = new repay_scheduleDc();

            var identity = User.Identity as ClaimsIdentity;
            int userid = 0;
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

            using (var db = new AuthContext())
            {
                var laondata = db.LeadLoan.Where(x => x.LeadMasterId.Value == Leadmasterid && x.IsActive == true && x.IsDeleted == false).FirstOrDefault();
                var repaymentData = db.RepaymentSchedule.Where(x => x.LeadMasterId == Leadmasterid && x.IsActive == true && x.IsDeleted == false).ToList();
                if (repaymentData.Count > 0)
                {
                    repaymentData.ForEach(x =>
                    {
                        x.IsActive = false;
                        x.IsDeleted = true;
                        db.Entry(x).State = EntityState.Modified;
                    });
                    db.Commit();
                }
                ArthMateHelper arthMateHelper = new ArthMateHelper();
                if (laondata != null)
                {
                    Postrepayment_scheduleDc reqdata = new Postrepayment_scheduleDc()
                    {
                        company_id = laondata.company_id.Value,
                        loan_id = laondata.loan_id,
                        product_id = laondata.product_id
                    };
                    Repyment_ScduleData = await arthMateHelper.repayment_schedule(reqdata, 0, Leadmasterid, false);
                }

                if (Repyment_ScduleData != null && Repyment_ScduleData.success && Repyment_ScduleData.data.rows.Count > 0)
                {

                    //list.AddRange((IEnumerable<RepaymentSchedule>)Repyment_ScduleData.data.rows);
                    //db.RepaymentSchedule.AddRange(Repyment_ScduleData.data.rows);
                    //db.Commit();
                    //list.AddRange(Repyment_ScduleData.data.rows);
                    List<RepaymentSchedule> list = new List<RepaymentSchedule>();
                    foreach (var item in Repyment_ScduleData.data.rows)
                    {
                        RepaymentSchedule ReDatas = new RepaymentSchedule();
                        ReDatas._id = item._id;
                        ReDatas.count = Repyment_ScduleData.data.count;
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
                        ReDatas.CreatedBy = userid;// 117
                        ReDatas.CreatedDate = DateTime.Now;
                        ReDatas.ModifiedBy = null;
                        ReDatas.ModifiedDate = null;
                        ReDatas.LeadMasterId = Leadmasterid;

                        list.Add(ReDatas);
                    }
                    db.RepaymentSchedule.AddRange(list);
                    db.Commit();

                }

            }
            return ReData;
        }


        [AllowAnonymous]
        [HttpGet]
        [Route("GoDigitInsuranceHtml")]
        public string htmlconvertor()
        {
            string path = System.Web.Hosting.HostingEnvironment.MapPath(@"~\Templates\Arthmate\DigitGroupTotalProd.html");
            string htmldata = File.ReadAllText(path);
            string TartgetfolderPath = HttpContext.Current.Server.MapPath(@"~\ArthmateDocument\DigitGroup");
            if (!Directory.Exists(TartgetfolderPath))
                Directory.CreateDirectory(TartgetfolderPath);

            // ArthmateDocument / SlaDocument
            string thFileName = "SanctionAndLbaBusinessLoan" + DateTime.Now.ToString("MMddyyyyhhmmss") + ".pdf";
            string newfilepath = "/ArthmateDocument/DigitGroup/" + thFileName;
            string OutPutFile = Path.Combine(HttpContext.Current.Server.MapPath("~/ArthmateDocument/DigitGroup"), thFileName);

            byte[] pdf = null;

            pdf = Pdf
                 .From(htmldata)
                 .OfSize(OpenHtmlToPdf.PaperSize.A4)
                 .WithTitle("Agreement")
                 .WithoutOutline()
                 .WithMargins(PaperMargins.All(0.0.Millimeters()))
                 .Portrait()
                 //.Comressed()
                 .Content();

            // pdf.AddHeader("<div style='text-align: center;'>Your Header Content</div>");
            // Add footer
            //pdf.AddFooter("<div style='text-align: center;'>Your Footer Content</div>");
            FileStream file = File.Create(OutPutFile);
            file.Write(pdf, 0, pdf.Length);
            file.Close();
            //byte[] myBytes = (byte[])pdf;
            return htmldata;
        }




        [AllowAnonymous]
        [HttpGet]
        [Route("DateFormatReturn")]
        public string DateFormatReturn(String sdate)
        {
            string dd = "15-07-1999"; //or 31/12/2015
            DateTime startDate;
            string[] formats = { "dd/MM/yyyy", "dd/M/yyyy", "d/M/yyyy", "d/MM/yyyy",
                                "dd/MM/yy", "dd/M/yy", "d/M/yy", "d/MM/yy", "MM/dd/yyyy",

            "dd-MM-yyyy", "dd-M-yyyy", "d-M-yyyy", "d-MM-yyyy",
                                "dd-MM-yy", "dd-M-yy", "d-M-yy", "d-MM-yy", "MM-dd-yyyy"
                                , "yyyy-MM-dd", "yyyy/MM/dd"
            };

            DateTime.TryParseExact(sdate, formats,
            System.Globalization.CultureInfo.InvariantCulture,
            System.Globalization.DateTimeStyles.None, out startDate);

            //            Console.WriteLine();

            return startDate.ToString("yyyy-MM-dd");
        }


        //[HttpGet]
        //[Route("PanAndAadharXmlUpload")]
        public async Task<CommonResponseDc> PanAndAadharXmlUpload(long LeadMasterID, AuthContext db)
        {
            CommonResponseDc commonResponseDc = new CommonResponseDc();
            LoanDocumentResponseDc res = new LoanDocumentResponseDc();
            LoanDocumentPostDc ReqstJson = new LoanDocumentPostDc();

            if (ReqstJson != null)
            {
                {
                    var LeadId = new SqlParameter("@leadMasterId", LeadMasterID);
                    var requestjsondata = db.Database.SqlQuery<LeadDocumentDc>("EXEC LoanDocumentByLeadId @leadMasterId", LeadId).ToList();

                    foreach (var item in requestjsondata)
                    {
                        if (requestjsondata != null)
                        {
                            string DocBase64String = "";
                            string sCode = "";
                            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(item.OtherInfo);
                            DocBase64String = Convert.ToBase64String(plainTextBytes);
                            sCode = item.DocumentTypeCode;
                            //if (item.DocumentName == "pan_card")
                            //{

                            //    string TartgetfolderPath = HttpContext.Current.Server.MapPath(@"~\ArthmateDocument\SlaDocument");
                            //    if (!Directory.Exists(TartgetfolderPath))
                            //        Directory.CreateDirectory(TartgetfolderPath);

                            //    DocBase64String = item.OtherInfo;
                            //    string pdfFilePath = HttpContext.Current.Server.MapPath(@"~\ArthmateDocument\SlaDocument") + "\\" + "First PDF document_" + item.DocumentName + "_" + LeadMasterID + "_.pdf";
                            //    System.IO.FileStream fs = new FileStream(pdfFilePath, FileMode.Create);
                            //    Document document = new Document(PageSize.A4, 25, 25, 30, 30);
                            //    PdfWriter writer = PdfWriter.GetInstance(document, fs);
                            //    //document.AddAuthor("Micke Blomquist");
                            //    //document.AddCreator("Sample application using iTextSharp");
                            //    //document.AddKeywords("PDF tutorial education");
                            //    //document.AddSubject("Document subject - Describing the steps creating a PDF document");
                            //    //document.AddTitle("The document title - PDF creation using iTextSharp");
                            //    document.Open();
                            //    document.Add(new Paragraph(DocBase64String));
                            //    document.Close();
                            //    writer.Close();
                            //    fs.Close();
                            //    byte[] bytes = File.ReadAllBytes(pdfFilePath);
                            //    DocBase64String = Convert.ToBase64String(bytes);
                            //}
                            LoanDocumentPostDc PostJson = new LoanDocumentPostDc
                            {
                                base64pdfencodedfile = DocBase64String,
                                borrower_id = item.borrower_id,
                                loan_app_id = item.loan_app_id,
                                partner_loan_app_id = item.partner_loan_app_id,
                                code = sCode,
                                partner_borrower_id = item.partner_borrower_id,

                            };
                            ArthMateHelper ArthMateHelper = new ArthMateHelper();
                            res = await ArthMateHelper.LoanDocumentApi(PostJson, item.LeadmasterID, false);
                            if (res.uploadDocumentData != null)
                            {
                                if (res.uploadDocumentData.document_id > 0)
                                {
                                    commonResponseDc.Status = true;
                                    commonResponseDc.Msg = "Loan document uploaded successfully";

                                    string query = "Update KYCValidationResponses set Remark ='xml uploaded', ModifiedBy =1," +
                                        " ModifiedDate='" + DateTime.Now.ToString("yyyy/MM/dd") + "' Where isnull(Remark,'')='' and " +
                                        " LeadMasterId=" + item.LeadmasterID + " and id=" + item.id + " and IsActive=1 and IsDeleted=0 ";
                                    int update = db.Database.ExecuteSqlCommand(query);
                                }
                            }
                            LeadActivityProgressesHistory(LeadMasterID, 0, 0, "Document Api", "Pan & Aadhar Xml Documents (" + item.DocumentName + ") Uploaded Successfully");
                        }
                        else
                        {
                            res.message = "Data Not Found in Lead!";
                        }
                    }
                }
            }

            return commonResponseDc;
        }


        //negative pincode areas


        [AllowAnonymous]
        [HttpGet]
        [Route("NegativePincodeAreas")]
        public List<ArthmateNegativePincodeAreaMaster> NegativePincodeAreas(int pincode, string address)
        {
            List<ArthmateNegativePincodeAreaMaster> arthmatePinData = new List<ArthmateNegativePincodeAreaMaster>();

            using (AuthContext context = new AuthContext())
            {
                //arthmatePinData = context.ArthmateNegativePincodeAreaMaster.Where(x => x.Pin == pincode).ToList();
                var pinCod = new SqlParameter("@pincode", pincode);
                var Addres = new SqlParameter("@address", address);
                arthmatePinData = context.Database.SqlQuery<ArthmateNegativePincodeAreaMaster>("EXEC [Arthmate].[NegativePincodeAreaMasters] @pincode, @address", pinCod, Addres).ToList();
                foreach (var item in arthmatePinData)
                {
                    var ff = CalculateSimilarity(item.Area.ToLower().Trim(), address.ToLower().Trim());
                    var NameMatchPer = Math.Round(ff * 100);
                    if (NameMatchPer >= 60 && NameMatchPer <= 99)
                    {
                        //minor mismatch";
                        //verifyOtp.UpdateAdhaarInfoDc.ErrorMsg = "Name Mismatch";
                        //arthmatePinData.ForEach(x => x.Message = "Name Mismatch"+"Perceantage Is:"+ NameMatchPer);
                        //arthmatePinData.ForEach(x => x.Percentage = NameMatchPer);
                        item.Message = "Address Mismatch";
                        item.Percentage = NameMatchPer;
                    }
                    else if (NameMatchPer < 60)
                    {
                        string msg = "Address info is diffrent from NegativePinCode Areas ";
                        item.Message = msg;
                        item.Percentage = NameMatchPer;
                    }
                }
                return arthmatePinData;
            }
        }

        public static double CalculateSimilarity(string source, string target)
        {
            if ((source == null) || (target == null)) return 0.0;
            if ((source.Length == 0) || (target.Length == 0)) return 0.0;
            if (source == target) return 1.0;

            int stepsToSame = LevenshteinDistance(source, target);
            return (1.0 - ((double)stepsToSame / (double)Math.Max(source.Length, target.Length)));
        }
        public static int LevenshteinDistance(string source, string target)
        {
            // degenerate cases
            if (source == target) return 0;
            if (source.Length == 0) return target.Length;
            if (target.Length == 0) return source.Length;

            // create two work vectors of integer distances
            int[] v0 = new int[target.Length + 1];
            int[] v1 = new int[target.Length + 1];

            // initialize v0 (the previous row of distances)
            // this row is A[0][i]: edit distance for an empty s
            // the distance is just the number of characters to delete from t
            for (int i = 0; i < v0.Length; i++)
                v0[i] = i;

            for (int i = 0; i < source.Length; i++)
            {
                // calculate v1 (current row distances) from the previous row v0

                // first element of v1 is A[i+1][0]
                //   edit distance is delete (i+1) chars from s to match empty t
                v1[0] = i + 1;

                // use formula to fill in the rest of the row
                for (int j = 0; j < target.Length; j++)
                {
                    var cost = (source[i] == target[j]) ? 0 : 1;
                    v1[j + 1] = Math.Min(v1[j] + 1, Math.Min(v0[j + 1] + 1, v0[j] + cost));
                }

                // copy v1 (current row) to v0 (previous row) for next iteration
                for (int j = 0; j < v0.Length; j++)
                    v0[j] = v1[j];
            }

            return v1[target.Length];
        }


        public NegativeAdressDc CalculateMatchingPercentage(string inputAddress, string compareAddress)
        {
            string results = "";

            NegativeAdressDc negativeAdressDc = new NegativeAdressDc();

            // Tokenize the input addresses into words
            //string[] inputWords = inputAddress.Split(' ', ',', '(', ')');
            //string[] compareWords = compareAddress.Split(' ', ',', '(', ')');
            string[] inputWords = inputAddress.Split(' ', ',');
            string[] compareWords = compareAddress.Split();


            // Calculate the total number of words and matching words
            int totalWords = Math.Max(inputWords.Length, compareWords.Length);
            int matchingWords = 0;

            // Count the matching words
            foreach (string compareWord in compareWords)
            {
                foreach (string inputWord in inputWords)
                {
                    if (compareWord.Trim().Equals(inputWord.Trim(), StringComparison.OrdinalIgnoreCase))
                    {
                        matchingWords++;
                        break;
                    }
                }
            }

            string Compare2str = "";
            string Input2str = "";
            Int32 TotalWord = 2;
            string LastLevelMatchWord = "";

            if (matchingWords != compareWords.Length)
            {
                results = "Not Match total words (" + matchingWords + "/" + compareWords.Length + ").";
                TotalWord = compareWords.Length;
            }
            else
            {
                matchingWords = 1;
                for (int i = 1; i <= compareWords.Length; i++)
                {
                    if (i <= TotalWord)
                    {
                        Compare2str = (Compare2str + " " + compareWords[i - 1]).ToString().Trim();
                        if (i == TotalWord)
                        {
                            int index2 = inputAddress.IndexOf(Compare2str);
                            if (index2 < 0) //index2 <= 0
                            {
                                if (i == TotalWord)
                                {
                                    if (LastLevelMatchWord.Trim() != string.Empty)
                                    { results = "Not Match Word (" + Compare2str + ")!! \n Last-Level-Match-Word:" + LastLevelMatchWord; break; }
                                    else
                                    { results = "Not Match Word (" + Compare2str + ")!!"; break; }
                                }
                            }
                            else
                            {
                                matchingWords++;
                                LastLevelMatchWord = Compare2str + "(Total Word Match: " + TotalWord + ")";

                                if (i == compareWords.Length)
                                { results = "Match Word (" + Compare2str + ")!! "; break; }

                                TotalWord++;
                            }

                        }
                    }
                }
            }
            // Console.WriteLine(result);
            negativeAdressDc.Message = results;
            negativeAdressDc.MatchLevelWord = LastLevelMatchWord;
            negativeAdressDc.TotalCompareWord = TotalWord;
            negativeAdressDc.TotalMatchWord = matchingWords;

            // Calculate the percentage of matching words
            //double matchingPercentage = (double)matchingWords / totalWords * 100;
            //if (compareWords.Length <= matchingWords)
            //{
            //    matchingPercentage = 100;
            //}

            //else
            //{
            //    matchingPercentage = (double)matchingWords / totalWords * 100;

            //}
            //Console.WriteLine(matchingPercentage);
            return negativeAdressDc;
        }



        [AllowAnonymous]
        [HttpGet]
        [Route("CalculateMatchingPercentage")]
        public NegativeAdressDc CalculateMatchingAddresss(int pincode, string inputAddress, string compareAddress)
        {
            NegativeAdressDc negativeAdressDc = new NegativeAdressDc();

            //inputAddress = "1207 nagar, main road yadav nagar ( BANGANGA ) Indore, Madhya Pradesh, 452015";
            //compareAddress = "road yadav nagar (BANGANGA)";

            negativeAdressDc = CalculateMatchingPercentage(inputAddress, compareAddress);

            return negativeAdressDc;
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("GetMonthlySalaryApiTest")]
        public async Task<double> GetMonthlySalaryApi(long LeadMasterId)
        {
            double MonthlySalary = 0;
            using (AuthContext db = new AuthContext())
            {
                ArthMateHelper arthMateHelper = new ArthMateHelper();
                MonthlySalary = await arthMateHelper.GetMonthlySalary(LeadMasterId, db);
            }
            return MonthlySalary;
        }




        [AllowAnonymous]
        [HttpGet]
        [Route("GetApiParamsAndKeys")] //prod
        public async Task<dynamic> GetApiParamsAndKeys(long LeadMasterId, string Apiname)
        {
            //CeplrPdf_SpData ceplrPdf_SpData = new CeplrPdf_SpData();
            ApiParamForTesting apiParamForTesting = new ApiParamForTesting();
            using (AuthContext db = new AuthContext())
            {
                //if (Apiname == "CeplerPdfReport")
                {
                    var LeadmasterId = new SqlParameter("@leadmasterid", LeadMasterId);
                    var TaskNm = new SqlParameter("@TaskNm", Apiname);
                    apiParamForTesting = db.Database.SqlQuery<ApiParamForTesting>("EXEC [Arthmate].[CeplrPdfReportDataForApi]  @leadmasterid ,@TaskNm", LeadmasterId, TaskNm).FirstOrDefault();
                }
            }
            return apiParamForTesting;
        }



        [AllowAnonymous]
        [HttpGet]
        [Route("ESignDocumentasync")]
        public async Task<eSignDocumentResponse> ESignDocumentasync(long LeadMasterId, string documentId)
        {

            using (AuthContext context = new AuthContext())
            {

                eSignDocumentResponse eSignDocumentResponse = new eSignDocumentResponse();
                eSignDocumentRequest obj = new eSignDocumentRequest();
                ArthMateHelper arthMateHelper = new ArthMateHelper();

                if (documentId == "")
                {
                    var esigndetail = context.eSignDetail.Where(x => x.LeadMasterId == LeadMasterId).FirstOrDefault();
                    if (esigndetail != null)
                    { documentId = esigndetail.documentId; }
                }

                if (documentId == "")
                {
                    eSignDocumentResponse.requestId = "Error : Data not found";
                    eSignDocumentResponse.statusCode = 0;
                }
                else
                {
                    obj.documentId = documentId;//"I0ER60m";
                    obj.verificationDetailsRequired = "Y";
                    obj.LeadMasterId = LeadMasterId;
                    eSignDocumentResponse = await arthMateHelper.eSignDocumentsAsync(obj);
                    if (eSignDocumentResponse != null && eSignDocumentResponse.result != null)
                    {

                        string TartgetfolderPath = HttpContext.Current.Server.MapPath(@"~\ArthmateDocument\EsignDocument");
                        if (!Directory.Exists(TartgetfolderPath))
                            Directory.CreateDirectory(TartgetfolderPath);

                        byte[] bytes = Convert.FromBase64String(eSignDocumentResponse.result.file);
                        string FileName = System.Web.Hosting.HostingEnvironment.MapPath(@"~/ArthmateDocument/EsignDocument/" + "Esign_" + LeadMasterId + DateTime.Now.ToString("dd-MM-yyyy-hh-mm") + ".pdf");
                        System.IO.File.WriteAllBytes(FileName, bytes);
                        string FileUrl = "/ArthmateDocument/EsignDocument/" + "Esign_" + LeadMasterId + DateTime.Now.ToString("dd-MM-yyyy-hh-mm") + ".pdf";
                        eSignDocumentResponse.FileUrl = FileUrl;
                        var leadloan = context.LeadLoan.Where(x => x.LeadMasterId == LeadMasterId).FirstOrDefault();
                        if (leadloan != null)
                        {
                            leadloan.UrlSlaUploadSignedDocument = FileUrl;
                            context.Entry(leadloan).State = EntityState.Modified;
                            context.Commit();
                        }
                    }
                }

                return eSignDocumentResponse;
            }
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("DeleteBankStatementById")] //prod
        public async Task<CommonResponseDc> DeleteBankStatementById(long BankStatementId)
        {
            CommonResponseDc res = new CommonResponseDc();

            var identity = User.Identity as ClaimsIdentity;
            int userid = 0;
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

            using (var db = new AuthContext())
            {
                var statements = db.LeadBankStatement.Where(x => x.IsActive == true && x.IsDeleted == false && x.Id == BankStatementId).FirstOrDefault();
                if (statements != null)
                {

                    statements.IsActive = false;
                    statements.IsDeleted = true;
                    statements.ModifiedBy = userid;
                    statements.ModifiedDate = DateTime.Now;
                    db.Entry(statements).State = EntityState.Modified;
                    db.Commit();

                    res.Status = true;
                    res.Msg = "File Deleted Successfully";
                }
            }
            return res;
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("AddBankStatement")]
        public async Task<CommonResponseDc> AddBankStatement(AddBankStatementDc lead)
        {

            CommonResponseDc res = new CommonResponseDc();
            LeadResponseDc LeadResponse = new LeadResponseDc();

            var identity = User.Identity as ClaimsIdentity;
            int userid = 0;
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

            using (var db = new AuthContext())
            {
                var leaddata = await db.LeadMasters.Where(x => x.Id == lead.LeadMasterId && x.IsActive == true && x.IsDeleted == false).FirstOrDefaultAsync();
                if (lead != null)
                {
                    if (lead.BankStatement.Count > 0)
                    {
                        var document = db.ArthMateDocumentMasters.Where(x => x.DocumentName.Trim().ToLower() == "bank_stmnts").FirstOrDefault();
                        {
                            var LeadBankStatement = db.LeadBankStatement.Where(x => x.LeadMasterId == lead.LeadMasterId && x.DocumentMasterId == document.Id && x.IsActive == true && x.IsDeleted == false).ToList();

                            if (LeadBankStatement.Count > 0)
                            {
                                foreach (var item in LeadBankStatement)
                                {
                                    item.IsActive = false;
                                    item.IsDeleted = true;
                                    item.ModifiedDate = DateTime.Now;
                                    item.ModifiedBy = userid;
                                    db.Entry(item).State = EntityState.Modified;
                                }
                            }
                        }

                        int PrimaryFile = 0;
                        foreach (var file in lead.BankStatement)
                        {
                            LeadBankStatement BankStatement = new LeadBankStatement();
                            BankStatement.LeadMasterId = lead.LeadMasterId;
                            BankStatement.DocumentMasterId = (int)document.Id;
                            BankStatement.StatementFile = file;
                            BankStatement.Sequence = PrimaryFile;
                            BankStatement.Remark = lead.Remark;
                            BankStatement.IsSuccess = false;
                            BankStatement.CreatedDate = DateTime.Now;
                            BankStatement.CreatedBy = userid;
                            BankStatement.ModifiedDate = null;
                            BankStatement.ModifiedBy = null;
                            BankStatement.IsActive = true;
                            BankStatement.IsDeleted = false;
                            db.LeadBankStatement.Add(BankStatement);

                            PrimaryFile++;
                        }
                    }
                    if (db.Commit() > 0)
                    {
                        res.Status = true;
                        res.Msg = "Data Saved Successfully";
                    }
                }

            }
            return res;
        }


        //new on 12_01_2024
        [AllowAnonymous]
        [HttpPost]
        [Route("AddBenBankDetail")]
        public async Task<string> AddBenBankDetail(AddBenBankDetail BenBank)
        {

            string result = "Something went wrong.";
            using (var db = new AuthContext())
            {
                var leaddata = db.LeadMasters.Where(x => x.Id == BenBank.LeadmasterId && x.IsActive == true && x.IsDeleted == false).FirstOrDefault();
                if (leaddata != null && BenBank != null
                    && !string.IsNullOrEmpty(BenBank.ben_accountHolderName)
                    && !string.IsNullOrEmpty(BenBank.ben_bank_acc_num)
                    && !string.IsNullOrEmpty(BenBank.ben_Typeofaccount)
                    && !string.IsNullOrEmpty(BenBank.ben_bank_name)
                    && !string.IsNullOrEmpty(BenBank.ben_bank_ifsc)
                    )
                {
                    leaddata.ben_AccountNumber = BenBank.ben_bank_acc_num;
                    leaddata.ben_Accountholdername = BenBank.ben_accountHolderName;
                    leaddata.ben_Typeofaccount = BenBank.ben_Typeofaccount;
                    leaddata.ben_Bankname = BenBank.ben_bank_name;
                    leaddata.ben_IFSCCode = BenBank.ben_bank_ifsc;
                    db.Entry(leaddata).State = EntityState.Modified;
                    if (db.Commit() > 0)
                    {
                        result = "Saved Succesfully";
                    }
                }
                else
                {
                    result = "Failed, Some Bank field are null. ";
                }
                return result;
            }

        }


        [HttpPost]
        [Route("addloandetails")]
        public async Task<string> addloandetails(GetLoanApiResponseDC loanres)
        {
            using (AuthContext db = new AuthContext())
            {
                //GetLoanApiResponseDC loanres = new GetLoanApiResponseDC();
                //loanres = JsonConvert.DeserializeObject<GetLoanApiResponseDC>(jsonStrings);

                //x.partner_loan_app_id == loanres.data.FirstOrDefault().partner_loan_app_id && x.partner_borrower_id == loanres.data.FirstOrDefault().partner_borrower_id
                string partner_loan_app_id = loanres.data.FirstOrDefault().partner_loan_app_id;
                string partner_borrower_id = loanres.data.FirstOrDefault().partner_borrower_id;

                var leadid = db.LeadMasters.Where(x => x.partner_loan_app_id == partner_loan_app_id && x.partner_borrower_id == partner_borrower_id).FirstOrDefault();
                var Configdata = db.LoanConfiguration.FirstOrDefault();
                if (leadid == null)
                {
                    return "Data Not Found.";
                }
                else
                {
                    LeadLoan LeadLoanData = new LeadLoan();

                    LeadLoanData.LeadMasterId = leadid.Id;
                    LeadLoanData.ReponseId = 1;
                    LeadLoanData.RequestId = 1;

                    LeadLoanData.IsSuccess = loanres.success;
                    LeadLoanData.Message = loanres.message;
                    LeadLoanData.CreatedDate = DateTime.Now;
                    LeadLoanData.ModifiedDate = DateTime.Now;
                    LeadLoanData.IsActive = true;
                    LeadLoanData.IsDeleted = false;
                    LeadLoanData.CreatedBy = 1717;
                    LeadLoanData.ModifiedBy = 0;

                    LeadLoanData.loan_app_id = loanres.data.FirstOrDefault().loan_app_id;
                    LeadLoanData.loan_id = loanres.data.FirstOrDefault().loan_id;
                    LeadLoanData.borrower_id = loanres.data.FirstOrDefault().borrower_id;
                    LeadLoanData.partner_loan_app_id = loanres.data.FirstOrDefault().partner_loan_app_id;
                    LeadLoanData.partner_loan_id = loanres.data.FirstOrDefault().partner_loan_id;
                    LeadLoanData.partner_borrower_id = loanres.data.FirstOrDefault().partner_borrower_id;
                    LeadLoanData.company_id = loanres.data.FirstOrDefault().company_id;
                    LeadLoanData.product_id = loanres.data.FirstOrDefault().product_id.ToString();
                    LeadLoanData.loan_app_date = loanres.data.FirstOrDefault().loan_app_date;
                    LeadLoanData.sanction_amount = loanres.data.FirstOrDefault().sanction_amount;
                    LeadLoanData.gst_on_pf_amt = loanres.data.FirstOrDefault().gst_on_pf_amt;
                    LeadLoanData.gst_on_pf_perc = loanres.data.FirstOrDefault().gst_on_pf_perc;
                    LeadLoanData.net_disbur_amt = loanres.data.FirstOrDefault().net_disbur_amt;
                    LeadLoanData.status = loanres.data.FirstOrDefault().status;
                    LeadLoanData.stage = loanres.data.FirstOrDefault().stage;
                    LeadLoanData.exclude_interest_till_grace_period = loanres.data.FirstOrDefault().exclude_interest_till_grace_period;
                    LeadLoanData.borro_bank_account_type = loanres.data.FirstOrDefault().borro_bank_account_type;
                    LeadLoanData.borro_bank_account_holder_name = loanres.data.FirstOrDefault().borro_bank_account_holder_name;
                    LeadLoanData.loan_int_rate = Convert.ToString(Configdata.InterestRate);//loanres.data.FirstOrDefault().loan_int_rate;
                    LeadLoanData.processing_fees_amt = loanres.data.FirstOrDefault().processing_fees_amt;
                    LeadLoanData.processing_fees_perc = loanres.data.FirstOrDefault().processing_fees_perc;
                    LeadLoanData.tenure = loanres.data.FirstOrDefault().tenure;
                    LeadLoanData.tenure_type = loanres.data.FirstOrDefault().tenure_type;
                    LeadLoanData.int_type = loanres.data.FirstOrDefault().int_type;
                    LeadLoanData.borro_bank_ifsc = loanres.data.FirstOrDefault().borro_bank_ifsc;
                    LeadLoanData.borro_bank_acc_num = loanres.data.FirstOrDefault().borro_bank_acc_num;
                    LeadLoanData.borro_bank_name = loanres.data.FirstOrDefault().borro_bank_name;
                    LeadLoanData.first_name = loanres.data.FirstOrDefault().first_name;
                    LeadLoanData.last_name = loanres.data.FirstOrDefault().last_name;
                    LeadLoanData.current_overdue_value = loanres.data.FirstOrDefault().current_overdue_value;
                    LeadLoanData.bureau_score = loanres.data.FirstOrDefault().bureau_score;
                    //LeadLoanData.monthly_income = loanres.data.FirstOrDefault().monthly_income;
                    LeadLoanData.loan_amount_requested = loanres.data.FirstOrDefault().loan_amount_requested;
                    LeadLoanData.bene_bank_name = loanres.data.FirstOrDefault().bene_bank_name;
                    LeadLoanData.bene_bank_acc_num = loanres.data.FirstOrDefault().bene_bank_acc_num;
                    LeadLoanData.bene_bank_ifsc = loanres.data.FirstOrDefault().bene_bank_ifsc;
                    LeadLoanData.bene_bank_account_holder_name = loanres.data.FirstOrDefault().bene_bank_account_holder_name;
                    LeadLoanData.created_at = loanres.data.FirstOrDefault().created_at;
                    LeadLoanData.updated_at = loanres.data.FirstOrDefault().updated_at;
                    //LeadLoanData.id_loan = loanres.data.FirstOrDefault().id_loan;
                    LeadLoanData.v = loanres.data.FirstOrDefault().__v;
                    LeadLoanData.co_lender_assignment_id = loanres.data.FirstOrDefault().co_lender_assignment_id;
                    LeadLoanData.co_lender_id = loanres.data.FirstOrDefault().co_lender_id;
                    LeadLoanData.co_lend_flag = loanres.data.FirstOrDefault().co_lend_flag;
                    LeadLoanData.itr_ack_no = loanres.data.FirstOrDefault().itr_ack_no;
                    LeadLoanData.penal_interest = loanres.data.FirstOrDefault().penal_interest;
                    LeadLoanData.bounce_charges = loanres.data.FirstOrDefault().bounce_charges;
                    LeadLoanData.repayment_type = loanres.data.FirstOrDefault().repayment_type;
                    LeadLoanData.first_inst_date = loanres.data.FirstOrDefault().first_inst_date;
                    LeadLoanData.final_approve_date = loanres.data.FirstOrDefault().final_approve_date;
                    LeadLoanData.final_remarks = loanres.data.FirstOrDefault().final_remarks;
                    LeadLoanData.foir = loanres.data.FirstOrDefault().foir;
                    LeadLoanData.upfront_interest = loanres.data.FirstOrDefault().upfront_interest;
                    //LeadLoanData.customer_type_ntc = loanres.data.FirstOrDefault().customer_type_ntc;
                    LeadLoanData.business_vintage_overall = loanres.data.FirstOrDefault().business_vintage_overall;
                    //LeadLoanData.gst_number = loanres.data.FirstOrDefault().gst_number;
                    //LeadLoanData.abb = loanres.data.FirstOrDefault().abb;
                    LeadLoanData.loan_int_amt = loanres.data.FirstOrDefault().loan_int_amt;
                    LeadLoanData.conv_fees = loanres.data.FirstOrDefault().conv_fees;
                    LeadLoanData.ninety_plus_dpd_in_last_24_months = loanres.data.FirstOrDefault().ninety_plus_dpd_in_last_24_months;
                    LeadLoanData.dpd_in_last_9_months = loanres.data.FirstOrDefault().dpd_in_last_9_months;
                    LeadLoanData.dpd_in_last_3_months = loanres.data.FirstOrDefault().dpd_in_last_3_months;
                    LeadLoanData.dpd_in_last_6_months = loanres.data.FirstOrDefault().dpd_in_last_6_months;
                    //LeadLoanData.bounces_in_three_month = loanres.data.FirstOrDefault().bounces_in_three_month;
                    LeadLoanData.insurance_company = loanres.data.FirstOrDefault().insurance_company;
                    LeadLoanData.credit_card_settlement_amount = loanres.data.FirstOrDefault().credit_card_settlement_amount;
                    LeadLoanData.emi_amount = loanres.data.FirstOrDefault().emi_amount;
                    LeadLoanData.emi_allowed = loanres.data.FirstOrDefault().emi_allowed;
                    LeadLoanData.igst_amount = loanres.data.FirstOrDefault().igst_amount;
                    LeadLoanData.cgst_amount = loanres.data.FirstOrDefault().cgst_amount;
                    LeadLoanData.sgst_amount = loanres.data.FirstOrDefault().sgst_amount;
                    LeadLoanData.emi_count = loanres.data.FirstOrDefault().emi_count;
                    LeadLoanData.broken_interest = loanres.data.FirstOrDefault().broken_interest;
                    LeadLoanData.dpd_in_last_12_months = loanres.data.FirstOrDefault().dpd_in_last_12_months;
                    LeadLoanData.dpd_in_last_3_months_credit_card = loanres.data.FirstOrDefault().dpd_in_last_3_months_credit_card;
                    LeadLoanData.dpd_in_last_3_months_unsecured = loanres.data.FirstOrDefault().dpd_in_last_3_months_unsecured;
                    LeadLoanData.broken_period_int_amt = loanres.data.FirstOrDefault().broken_period_int_amt;
                    LeadLoanData.dpd_in_last_24_months = loanres.data.FirstOrDefault().dpd_in_last_24_months;
                    LeadLoanData.avg_banking_turnover_6_months = 0;// loanres.data.FirstOrDefault().avg_banking_turnover_6_months;
                    LeadLoanData.enquiries_bureau_30_days = loanres.data.FirstOrDefault().enquiries_bureau_30_days;
                    LeadLoanData.cnt_active_unsecured_loans = loanres.data.FirstOrDefault().cnt_active_unsecured_loans;
                    LeadLoanData.total_overdues_in_cc = loanres.data.FirstOrDefault().total_overdues_in_cc;
                    LeadLoanData.insurance_amount = loanres.data.FirstOrDefault().insurance_amount;
                    LeadLoanData.bureau_outstanding_loan_amt = loanres.data.FirstOrDefault().bureau_outstanding_loan_amt;
                    LeadLoanData.purpose_of_loan = loanres.data.FirstOrDefault().purpose_of_loan;
                    LeadLoanData.business_name = loanres.data.FirstOrDefault().business_name;
                    LeadLoanData.co_app_or_guar_name = loanres.data.FirstOrDefault().co_app_or_guar_name;
                    LeadLoanData.co_app_or_guar_address = loanres.data.FirstOrDefault().co_app_or_guar_address;
                    LeadLoanData.co_app_or_guar_mobile_no = loanres.data.FirstOrDefault().co_app_or_guar_mobile_no;
                    LeadLoanData.co_app_or_guar_pan = loanres.data.FirstOrDefault().co_app_or_guar_pan;
                    //LeadLoanData.business_address = loanres.data.FirstOrDefault().business_address;
                    //LeadLoanData.business_state = loanres.data.FirstOrDefault().business_state;
                    //LeadLoanData.business_city = loanres.data.FirstOrDefault().business_city;
                    //LeadLoanData.business_pin_code = loanres.data.FirstOrDefault().business_pin_code;
                    LeadLoanData.business_address_ownership = loanres.data.FirstOrDefault().business_address_ownership;
                    LeadLoanData.business_pan = loanres.data.FirstOrDefault().business_pan;
                    LeadLoanData.bureau_fetch_date = DateTime.Now;// loanres.data.FirstOrDefault().bureau_fetch_date;
                    LeadLoanData.enquiries_in_last_3_months = loanres.data.FirstOrDefault().enquiries_in_last_3_months;
                    LeadLoanData.gst_on_conv_fees = loanres.data.FirstOrDefault().gst_on_conv_fees;
                    LeadLoanData.cgst_on_conv_fees = loanres.data.FirstOrDefault().cgst_on_conv_fees;
                    LeadLoanData.sgst_on_conv_fees = loanres.data.FirstOrDefault().sgst_on_conv_fees;
                    LeadLoanData.igst_on_conv_fees = loanres.data.FirstOrDefault().igst_on_conv_fees;
                    LeadLoanData.interest_type = loanres.data.FirstOrDefault().interest_type;
                    LeadLoanData.conv_fees_excluding_gst = loanres.data.FirstOrDefault().conv_fees_excluding_gst;
                    LeadLoanData.a_score_request_id = loanres.data.FirstOrDefault().a_score_request_id;
                    LeadLoanData.a_score = loanres.data.FirstOrDefault().a_score;
                    LeadLoanData.b_score = loanres.data.FirstOrDefault().b_score;
                    LeadLoanData.offered_amount = loanres.data.FirstOrDefault().offered_amount;
                    LeadLoanData.offered_int_rate = loanres.data.FirstOrDefault().offered_int_rate.Value;
                    LeadLoanData.monthly_average_balance = loanres.data.FirstOrDefault().monthly_average_balance.Value;
                    LeadLoanData.monthly_imputed_income = loanres.data.FirstOrDefault().monthly_imputed_income.Value;
                    LeadLoanData.party_type = loanres.data.FirstOrDefault().party_type;
                    LeadLoanData.co_app_or_guar_dob = DateTime.Now; //loanres.data.FirstOrDefault().co_app_or_guar_dob;
                    LeadLoanData.co_app_or_guar_gender = loanres.data.FirstOrDefault().co_app_or_guar_gender;
                    LeadLoanData.co_app_or_guar_ntc = loanres.data.FirstOrDefault().co_app_or_guar_ntc;
                    LeadLoanData.udyam_reg_no = loanres.data.FirstOrDefault().udyam_reg_no;
                    LeadLoanData.program_type = loanres.data.FirstOrDefault().program_type;
                    LeadLoanData.written_off_settled = loanres.data.FirstOrDefault().written_off_settled;
                    LeadLoanData.upi_handle = loanres.data.FirstOrDefault().upi_handle;
                    LeadLoanData.upi_reference = loanres.data.FirstOrDefault().upi_reference;
                    LeadLoanData.fc_offer_days = loanres.data.FirstOrDefault().fc_offer_days;
                    LeadLoanData.foreclosure_charge = loanres.data.FirstOrDefault().foreclosure_charge;
                    LeadLoanData.eligible_loan_amount = 0;// loanres.data.FirstOrDefault().eligible_loan_amount;
                    LeadLoanData.UrlSlaDocument = "";
                    LeadLoanData.UrlSlaUploadSignedDocument = "";
                    LeadLoanData.IsUpload = false;
                    LeadLoanData.UrlSlaUploadDocument_id = "";

                    db.LeadLoan.Add(LeadLoanData);
                    db.Commit();
                    return "Fields Added Successfully.";
                }
            }
        }




        #region //new for Sla-Lba Stamp Attachment
        [AllowAnonymous]
        [HttpPost]
        [Route("AddStampPaperData")]
        public IHttpActionResult AddStampPaperData()
        {
            string sPathNm = "~/ArthmateDocument/SlaDocument/StampPaper";
            string sPathNmStampPaper = "/ArthmateDocument/SlaDocument/StampPaper/";
            CommonResponseDc res = new CommonResponseDc();
            res.Status = false;
            res.Msg = "";
            var identity = User.Identity as ClaimsIdentity;
            int userid = 0;
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

            string LogoUrl = "";
            try
            {
                if (HttpContext.Current.Request.Files.AllKeys.Any())
                {
                    var httpPostedFile = HttpContext.Current.Request.Files["file"];

                    double fileSizeInMB = 0;
                    if (httpPostedFile.ContentLength > 0)
                    {
                        long fileSizeInBytes = httpPostedFile.ContentLength;
                        double fileSizeInKB = fileSizeInBytes / 1024.0;
                        fileSizeInMB = fileSizeInKB / 1024.0;

                        if (fileSizeInMB > 7)
                        {
                            res.Status = false;
                            res.Msg = "Stamp Image should be less than 7MB.";
                            return Created(res.Msg, res);
                        }

                        using (AuthContext db = new AuthContext())
                        {
                            if (httpPostedFile != null)
                            {
                                string extension = Path.GetExtension(httpPostedFile.FileName);
                                string fileName = httpPostedFile.FileName.Substring(0, httpPostedFile.FileName.LastIndexOf('.')) + extension;
                                var StampAmount = HttpContext.Current.Request.Form["StampAmount"];
                                var StampNumber = HttpContext.Current.Request.Form["StampNumber"];
                                var UsedFor = HttpContext.Current.Request.Form["UsedFor"];
                                var PartnerName = HttpContext.Current.Request.Form["PartnerName"];
                                var Purpose = HttpContext.Current.Request.Form["Purpose"];

                                var LeadMasterId = HttpContext.Current.Request.Form["LeadMasterId"];
                                var ValidateResult = StampVerify(sPathNmStampPaper, fileName, Convert.ToInt32(StampNumber));
                                if (ValidateResult.Status == true)
                                {
                                    return Created(ValidateResult.Msg, ValidateResult);
                                }

                                if (!Directory.Exists(System.Web.HttpContext.Current.Server.MapPath(sPathNm)))
                                    Directory.CreateDirectory(HttpContext.Current.Server.MapPath(sPathNm));

                                LogoUrl = Path.Combine(HttpContext.Current.Server.MapPath(sPathNm), fileName);
                                httpPostedFile.SaveAs(LogoUrl);
                                AngularJSAuthentication.Common.Helpers.FileUploadHelper.Upload(fileName, "~/ArthmateDocument/StampPaper", LogoUrl);

                                LogoUrl = string.Format("{0}://{1}{2}/{3}", new Uri((HttpContext.Current.Request.UrlReferrer != null ? HttpContext.Current.Request.UrlReferrer.AbsoluteUri : HttpContext.Current.Request.Url.AbsoluteUri)).Scheme
                                                                         , HttpContext.Current.Request.Url.DnsSafeHost
                                                                         , (HttpContext.Current.Request.Url.Port != 80 && HttpContext.Current.Request.Url.Port != 443 ? ":" + HttpContext.Current.Request.Url.Port : "")
                                                                         , sPathNmStampPaper + fileName);

                                LogoUrl = sPathNmStampPaper + fileName;
                                ArthmateSlaLbaStampDetail slaLbaStampDetail = new ArthmateSlaLbaStampDetail()
                                {
                                    LeadmasterId = 0,
                                    IsActive = true,
                                    CreatedDate = DateTime.Now,
                                    CreatedBy = userid,//Convert.ToInt32(leadmasterid),
                                    StampUrl = LogoUrl,
                                    IsStampUsed = false,
                                    StampAmount = Convert.ToInt32(StampAmount),
                                    UsedFor = UsedFor,
                                    DateofUtilisation = null,
                                    IsDeleted = false,
                                    LoanId = "",
                                    Purpose = Purpose,
                                    StampPaperNo = Convert.ToInt32(StampNumber),
                                    PartnerName = PartnerName,

                                    //LeadmasterId = 0,
                                    //    IsActive = true,
                                    //    CreatedDate = DateTime.Now,
                                    //    CreatedBy = userid,//Convert.ToInt32(leadmasterid),
                                    //    StampUrl = LogoUrl,
                                    //    IsStampUsed = false,
                                    //    StampAmount = lead.StampAmount,
                                    //    UsedFor = lead.UsedFor,
                                    //    DateofUtilisation = DateTime.Now,
                                    //    IsDeleted = false,
                                    //    LoanId = "",
                                    //    Purpose = lead.Purpose,
                                    //    StampPaperNo = lead.StampNumber,
                                    //    PartnerName = lead.PartnerName,
                                };
                                db.ArthmateSlaLbaStampDetail.Add(slaLbaStampDetail);

                                if (db.Commit() > 0)
                                {
                                    res.Status = true;
                                    res.Msg = "Save Sucessfully";
                                }

                                return Created(res.Msg, res);
                            }
                        }

                    }
                }
                return Created(res.Msg, res);

            }
            catch (Exception ex)
            {
                res.Status = false;
                res.Msg = ex.Message;
                return Created(res.Msg, res);
            }
        }
        //[AllowAnonymous]
        //[HttpPost]
        //[Route("AddStampPaperData")]
        //public async Task<CommonResponseDc> AddStampData(AddStampDataDc lead)
        //{

        //    CommonResponseDc res = new CommonResponseDc();

        //    var identity = User.Identity as ClaimsIdentity;
        //    int userid = 0;
        //    if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
        //        userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

        //    using (var db = new AuthContext())
        //    {
        //        // var leaddata = await db.LeadMasters.Where(x => x.Id == lead.LeadMasterId && x.IsActive == true && x.IsDeleted == false).FirstOrDefaultAsync();
        //        if (lead.Count > 0)
        //        {
        //            foreach (var stampData in lead)
        //            {
        //                ArthmateSlaLbaStampDetail stampDetails = new ArthmateSlaLbaStampDetail();

        //                stampDetails.LeadmasterId = stampData.LeadMasterId;
        //                stampDetails.StampPaperNo = stampData.StampNumber;
        //                stampDetails.StampUrl = stampData.StampUrl;
        //                stampDetails.UsedFor = stampData.UsedFor;//"Arthmate India Financing Limited";
        //                stampDetails.PartnerName = stampData.PartnerName;//"ShopKirana";
        //                stampDetails.StampAmount = stampData.StampAmount;
        //                stampDetails.IsStampUsed = false;
        //                stampDetails.CreatedDate = DateTime.Now;
        //                stampDetails.CreatedBy = userid;
        //                stampDetails.ModifiedDate = null;
        //                stampDetails.ModifiedBy = null;
        //                stampDetails.IsActive = true;
        //                stampDetails.IsDeleted = false;
        //                db.ArthmateSlaLbaStampDetail.Add(stampDetails);
        //            }
        //        }
        //        if (db.Commit() > 0)
        //        {
        //            res.Status = true;
        //            res.Msg = "Data Saved Successfully";
        //        }
        //    }
        //    return res;
        //}





        //StampVerify=>
        //              stampUrl,StampNumber

        [AllowAnonymous]
        [HttpPost]
        [Route("StampPaperModification")]
        public IHttpActionResult StampPaperModification()
        {
            //long Id, string stampFileName, int? StampPaperNo, string ActivityAction
            CommonResponseDc res = new CommonResponseDc();
            res.Status = false;
            res.Msg = "";

            try
            {
                var httpPostedFile = HttpContext.Current.Request.Files["file"];

                double fileSizeInMB = 0;
                if (httpPostedFile.ContentLength > 0)
                {
                    long fileSizeInBytes = httpPostedFile.ContentLength;
                    double fileSizeInKB = fileSizeInBytes / 1024.0;
                    fileSizeInMB = fileSizeInKB / 1024.0;

                    if (fileSizeInMB > 7)
                    {
                        res.Status = false;
                        res.Msg = "Stamp Image should be less than 7MB.";
                        return Created(res.Msg, res);
                    }
                    var Id = HttpContext.Current.Request.Form["Id"];
                    long searchId = Convert.ToInt32(Id);
                    var stampFileName = "";
                    stampFileName = HttpContext.Current.Request.Form["stampFileName"];
                    stampFileName = !string.IsNullOrEmpty(stampFileName) ? stampFileName : httpPostedFile.FileName;
                    var StampPaperNo = HttpContext.Current.Request.Form["StampPaperNo"];
                    var ActivityAction = HttpContext.Current.Request.Form["ActivityAction"];

                    string sPathNm = "~/ArthmateDocument/SlaDocument/StampPaper";
                    string sPathNmStampPaper = "/ArthmateDocument/SlaDocument/StampPaper/";
                    string extension = "";
                    string fileName = "";
                    string fileNameOld = "";

                    var identity = User.Identity as ClaimsIdentity;
                    int userid = 0;
                    if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                        userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);



                    string LogoUrl = "";
                    try
                    {
                        using (AuthContext db = new AuthContext())
                        {
                            var stampdata = db.ArthmateSlaLbaStampDetail.FirstOrDefault(x => x.Id == searchId && x.IsActive == true && x.IsDeleted == false);
                            if (stampdata == null)
                            {
                                res.Msg = "Record Not exist";
                            }
                            else
                            {
                                if (ActivityAction == "D")
                                {
                                    stampdata.IsActive = false;
                                    stampdata.IsDeleted = true;
                                    stampdata.ModifiedDate = DateTime.Now;
                                    db.Entry(stampdata).State = EntityState.Modified;
                                }
                                else
                                {
                                    if (Convert.ToInt32(StampPaperNo) != 0)
                                    {
                                        stampdata.StampPaperNo = (Convert.ToInt32(StampPaperNo));
                                        db.Entry(stampdata).State = EntityState.Modified;
                                    }

                                    if (!string.IsNullOrEmpty(stampFileName))
                                    {
                                        if (HttpContext.Current.Request.Files.AllKeys.Any())
                                        {
                                            httpPostedFile = HttpContext.Current.Request.Files["file"];

                                            if (httpPostedFile != null)
                                            {
                                                fileNameOld = stampdata.StampUrl;

                                                extension = Path.GetExtension(httpPostedFile.FileName);
                                                fileName = httpPostedFile.FileName.Substring(0, httpPostedFile.FileName.LastIndexOf('.')) + extension;


                                                var ValidateResult = StampVerify(sPathNmStampPaper, fileName, (StampPaperNo != null) ? (Convert.ToInt32(StampPaperNo)) : 0);
                                                if (ValidateResult.Status == true)
                                                {
                                                    //return ValidateResult;
                                                    return Created(ValidateResult.Msg, ValidateResult);

                                                }

                                                //Delete From Physical Path
                                                if (!string.IsNullOrEmpty(fileNameOld) && System.IO.File.Exists(HttpContext.Current.Server.MapPath(fileNameOld)))
                                                {
                                                    System.IO.File.Delete(HttpContext.Current.Server.MapPath(fileNameOld));
                                                }

                                                if (!Directory.Exists(System.Web.HttpContext.Current.Server.MapPath(sPathNm)))
                                                    Directory.CreateDirectory(HttpContext.Current.Server.MapPath(sPathNm));

                                                LogoUrl = Path.Combine(HttpContext.Current.Server.MapPath(sPathNm), fileName);
                                                httpPostedFile.SaveAs(LogoUrl);
                                                AngularJSAuthentication.Common.Helpers.FileUploadHelper.Upload(fileName, "~/ArthmateDocument/StampPaper", LogoUrl);

                                                LogoUrl = string.Format("{0}://{1}{2}/{3}", new Uri((HttpContext.Current.Request.UrlReferrer != null ? HttpContext.Current.Request.UrlReferrer.AbsoluteUri : HttpContext.Current.Request.Url.AbsoluteUri)).Scheme
                                                                                         , HttpContext.Current.Request.Url.DnsSafeHost
                                                                                         , (HttpContext.Current.Request.Url.Port != 80 && HttpContext.Current.Request.Url.Port != 443 ? ":" + HttpContext.Current.Request.Url.Port : "")
                                                                                         , sPathNmStampPaper + fileName);

                                                LogoUrl = sPathNmStampPaper + fileName;

                                                stampdata.StampUrl = LogoUrl;
                                                db.Entry(stampdata).State = EntityState.Modified;
                                            }
                                        }
                                    }
                                }

                                if (db.Commit() > 0)
                                {
                                    res.Status = true;
                                    res.Msg = "Sucessfully";
                                }
                            }
                        }

                        //return res;
                        //return Created(res.Msg, res);

                    }
                    catch (Exception ex)
                    {
                        res.Status = false;
                        res.Msg = ex.Message;
                        //return res;
                        return Created(res.Msg, res);
                    }
                }

            }
            catch (Exception ex)
            {
                res.Status = false;
                res.Msg = ex.Message;
                return Created(res.Msg, res);
            }
            return Created(res.Msg, res);
        }

        public CommonResponseDc StampVerify(string sPathNmStampPaper, string stampFileName, int? StampNumber)
        {
            CommonResponseDc res = new CommonResponseDc();
            if (string.IsNullOrEmpty(stampFileName) || StampNumber == 0)
            {
                res.Msg = "";
                if (string.IsNullOrEmpty(stampFileName))
                {
                    res.Status = true;
                    res.Msg = "Please Enter StampUrl, ";
                }
                if (StampNumber == 0)
                {
                    res.Status = true;
                    res.Msg = res.Msg + "Please Enter StampNumber";
                }
                //res.Status = true;
                //res.Msg = "Please Enter Url Or StampNumber";
                return res;
            }
            using (AuthContext context = new AuthContext())
            {
                //var param = new SqlParameter("@stampfilename", stampFileName);
                //var param1 = new SqlParameter("@StampNumber", StampNumber);
                //var dataS = context.Database.SqlQuery<string>("EXEC [Arthmate].[StampVerify] @stampfilename,@StampNumber", param, param1).FirstOrDefault();
                //if(dataS!=null)
                //{

                //}
                string result = "";
                bool exist = false;
                var stampVerifyData = context.ArthmateSlaLbaStampDetail.Where(x => x.StampUrl.Replace(sPathNmStampPaper, "") == stampFileName || x.StampPaperNo == StampNumber && x.IsActive == true && x.IsDeleted == false).ToList();
                if (stampVerifyData != null && stampVerifyData.Count() > 0)
                {
                    if (stampVerifyData.Any(x => x.StampPaperNo == StampNumber))
                    {
                        exist = true;
                        result = "StampPaperNo is Matched ,";
                        res.Msg = result;//+ "StampPaperNo is Matched ,";
                        res.Status = exist;
                        return res;
                    }
                    else
                    {
                        exist = false;
                        result = "StampPaperNo is not Matched ,";
                        res.Msg = result;
                    }

                    if (stampVerifyData.Any(x => x.StampUrl.Replace(sPathNmStampPaper, "").Trim() == stampFileName.Trim()))
                    {
                        exist = true;
                        res.Msg = result + "StampUrl is Matched ,";
                        res.Status = exist;
                        return res;
                    }
                    else
                    {
                        exist = false;
                        res.Msg = result + "StampUrl is not Matched ,";
                    }
                    if (exist)
                    {
                        res.Status = exist;
                        return res;
                    }
                    else
                    {
                        res.Status = exist;
                        return res;
                    }
                }
                else
                {
                    res.Status = false;
                    res.Msg = " Url and StampNumber Doesn't Exist";
                    return res;
                }
            }
            return res;

            //return "";
        }
        #endregion

        [AllowAnonymous]
        [HttpGet]
        [Route("SlaLbaStampUpload")]
        public string SlaLbaStampUpload(long LeadID)
        {
            string returnPath = "";
            using (AuthContext context = new AuthContext())
            {
                var filepath = context.LeadLoan.Where(x => x.LeadMasterId == LeadID && x.IsActive == true && x.IsDeleted == false).FirstOrDefault();
                var photoFilePath = context.ArthmateSlaLbaStampDetail.Where(x => x.LeadmasterId == LeadID && x.IsActive == true && x.IsDeleted == false).FirstOrDefault();
                try
                {
                    TextFileLogHelper.TraceLog("SlaLbaStampUpload : start:>" + filepath.UrlSlaDocument);
                    TextFileLogHelper.TraceLog("SlaLbaStampUpload : start1:>" + photoFilePath.StampUrl);

                    string pdfFilePath = string.Concat(HttpRuntime.AppDomainAppPath, filepath.UrlSlaDocument.Replace("/", "\\"));
                    string photoPath = string.Concat(HttpRuntime.AppDomainAppPath, photoFilePath.StampUrl.Replace("/", "\\"));
                    //var pdfFilePath = "C:/Users/SK/Downloads//SanctionAndLbaBusinessLoan155_02122024034642.pdf";
                    //var photoFilePath = "C:/Users/SK/Downloads/Vishal_Pan.jpg";
                    var pageNumber = 8;

                    //var pdfManipulator = new PdfManipulator();
                    var dd = AddPhotoToPdf(pdfFilePath, photoPath, pageNumber);
                    if (dd == "Image Added Successfully")
                    {
                        //var stampDetail= context.SlaLbaStampDetail.Where(x => x.LeadmasterId == LeadID && x.IsActive==true  && x.IsDeleted==false).FirstOrDefault();
                        photoFilePath.IsStampUsed = true;
                        context.Entry(photoFilePath).State = EntityState.Modified;
                        context.Commit();

                    }
                    returnPath = filepath.UrlSlaDocument;
                }
                catch (Exception ex)
                {
                    TextFileLogHelper.TraceLog("SlaLbaStampUpload : Exception nEw  :>" + ex.Message);
                }
            }
            return returnPath;
        }
        public string AddPhotoToPdf(string pdfFilePath, string photoFilePath, int pageNumber)
        {
            try
            {
                // Load the existing PDF document
                TextFileLogHelper.TraceLog("SlaLbaStampUpload : AddPhotoToPdf start:>" + pdfFilePath);
                TextFileLogHelper.TraceLog("SlaLbaStampUpload : AddPhotoToPdf start1:>" + photoFilePath);
                using (var existingFileStream = new FileStream(pdfFilePath, FileMode.Open))
                using (var newMemoryStream = new MemoryStream())
                {
                    var reader = new PdfReader(existingFileStream);
                    var stamper = new PdfStamper(reader, newMemoryStream);

                    // Get the dimensions of the photo
                    var photoImage = iTextSharp.text.Image.GetInstance(photoFilePath);

                    //float photoWidth = photoImage.ScaledWidth;
                    //float photoHeight = photoImage.ScaledHeight;

                    // Get the page size of the PDF
                    //var pageSize = reader.GetPageSize(pageNumber);

                    // Calculate positioning for the photo
                    //float xPosition = (pageSize.Width - photoWidth) / 2;
                    //float yPosition = (pageSize.Height - photoHeight) / 2;

                    // Add the photo to the PDF
                    var contentByte = stamper.GetOverContent(pageNumber);
                    photoImage.ScaleToFit(700, 650);
                    photoImage.SetAbsolutePosition(70, 70);
                    //photoImage.SetAbsolutePosition(xPosition, yPosition);
                    contentByte.AddImage(photoImage);

                    // Close the stamper and reader
                    stamper.Close();
                    reader.Close();

                    // Write the modified PDF to disk
                    File.WriteAllBytes(pdfFilePath, newMemoryStream.ToArray());
                    return "Image Added Successfully";

                }
            }
            catch (Exception ex)
            {
                TextFileLogHelper.TraceLog("SlaLbaStampUpload : Exception>" + ex.Message);
                return ex.Message;
            }

        }


        public class jsonString
        {

            public string MyProperty { get; set; }
        }

        //[HttpGet]
        //[Route("testPan")]
        //public async Task<String> testPan()
        //{
        //    using (var db = new AuthContext())
        //    {
        //        //var ValidAuthenticationPanres = db.ArthmateReqResp.Where(x => x.Id == 138 && x.IsActive == true && x.IsDeleted == false).FirstOrDefault();
        //        string PanNumber = "BBJPL6445B"; long LeadMasterId= 14;
        //        ArthMateHelper ArthMateHelper = new ArthMateHelper();
        //        var ValidAuthenticationPanres = await ArthMateHelper.ValidAuthenticationPan(PanNumber, LeadMasterId);
        //        PanDocumentDc obj = new PanDocumentDc();
        //        //obj.NameOnCard = ValidAuthenticationPanres.RequestResponseMsg.person.name;
        //        obj.NameOnCard = ValidAuthenticationPanres.person.name;
        //        string[] name = obj.NameOnCard.ToString().Trim().Split(new char[] { ' ' }, 3);

        //        if (name[0] != null)
        //        {
        //            var first_name = name[0];
        //        }
        //        if (name.Length > 1 && name[1] != null)
        //        {
        //            if (name.Length == 2)
        //            {
        //                var last_name = name[1];
        //            }
        //            else
        //            {
        //                var middle_name = name[1];
        //            }
        //        }
        //        if (name.Length > 2 && name[2] != null)
        //        {
        //            var last_name = name[2];
        //        }
        //        try
        //        {
        //            //lead.last_name = string.IsNullOrEmpty(lead.last_name) ? "." : lead.last_name;
        //            var qlast_name = string.IsNullOrEmpty(name[1]) ? "." : "J";

        //        }
        //        catch (Exception ex)
        //        {

        //            throw ex;
        //        }

        //    }
        //    return "";
        //}
        [HttpGet]
        [Route("testcolenderres")]
        [AllowAnonymous]
        public async Task<string> testcolenderres(string json)
        {
            var ress = JsonConvert.DeserializeObject<CoLenderResponseDc>(json);

            var result = (!string.IsNullOrEmpty(ress.program_type) && ress.program_type == "Transaction_POS") ? "Transactions" : ress.program_type;


            return result;
        }
        [HttpGet]
        [Route("getArthmateStampData")]
        [AllowAnonymous]
        public List<GetStampDataDC> GetVantransationListExport(bool isStampUsed)
        {
            using (
                var myContext = new AuthContext())
            {
                var StampUsed = new SqlParameter("@isStampUsed", isStampUsed);
                //string query = "EXEC getArthmateSlaLbaStampDetailsData"+StampUsed;
                var arthmateData = myContext.Database.SqlQuery<GetStampDataDC>("EXEC getArthmateSlaLbaStampDetailsData @isStampUsed", StampUsed).ToList();
                //var arthmateData = myContext.Database.SqlQuery<GetStampDataDC>(query).ToList();
                if (arthmateData != null)
                {
                    return arthmateData;
                }
                return arthmateData;
            }
        }

        [HttpGet]
        [Route("getArthmateSlaLbaStampAutoFilled")]
        [AllowAnonymous]
        public GetStampAutoFilledDC getArthmateSlaLbaStampAutoFilled()
        {
            using (var myContext = new AuthContext())
            {

                //string query = "EXEC getArthmateSlaLbaStampDetailsData"+StampUsed;
                var arthmateData = myContext.Database.SqlQuery<GetStampAutoFilledDC>("EXEC getArthmateSlaLbaStampDetails").FirstOrDefault();
                //var arthmateData = myContext.Database.SqlQuery<GetStampDataDC>(query).ToList();
                if (arthmateData != null)
                {
                    return arthmateData;
                }
                return arthmateData;
            }
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("GetArthmateRepaymentUpload")]
        public async Task<CommonResponseDc> GetArthmateRepaymentUpload()
        {
            CommonResponseDc res = new CommonResponseDc();

            using (var db = new AuthContext())
            {
                var arthmateRepaymentUpdates = await db.ArthmateRepaymentUpdate.Where(x => x.IsActive == true && x.IsDeleted == false).OrderByDescending(y => y.Id).ToListAsync();
                if (arthmateRepaymentUpdates != null)
                {
                    foreach (var arthmate in arthmateRepaymentUpdates)
                    {
                        string fileName = arthmate.Filename;
                        arthmate.Filename = Path.GetFileName(fileName);
                    }
                    res.Data = arthmateRepaymentUpdates;
                    res.Status = true;
                    res.Msg = "Success";
                }
            }
            return res;
        }

        //[AllowAnonymous]
        //[HttpPost]
        //[Route("AddArthmateRepaymentUpload")]
        //public async Task<IHttpActionResult> AddArthmateRepaymentUpload()
        //{
        //    CommonResponseDc res = new CommonResponseDc();
        //    ArthmateRepaymentUpdate arthmateRepaymentUpdate = new ArthmateRepaymentUpdate();

        //    res.Status = false;
        //    res.Msg = "";
        //    var identity = User.Identity as ClaimsIdentity;
        //    int userid = 0;
        //    if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
        //        userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

        //    var httpPostedFile = HttpContext.Current.Request.Files["file"];

        //    if (httpPostedFile != null)
        //    {
        //        string ext = Path.GetExtension(httpPostedFile.FileName);
        //        if (ext == ".xlsx")
        //        {
        //            using (var db = new AuthContext())
        //            {
        //                arthmateRepaymentUpdate.Filename = httpPostedFile.FileName;
        //                arthmateRepaymentUpdate.CreatedBy = userid;
        //                arthmateRepaymentUpdate.DateOfupload = DateTime.Now;
        //                arthmateRepaymentUpdate.CreatedDate = DateTime.Now;
        //                arthmateRepaymentUpdate.ModifiedDate = null;
        //                arthmateRepaymentUpdate.IsActive = true;
        //                arthmateRepaymentUpdate.IsDeleted = false;
        //                arthmateRepaymentUpdate.ArthmateStatus = true;
        //                arthmateRepaymentUpdate.ArthmateStatusMsg = "Ok";

        //                db.ArthmateRepaymentUpdate.Add(arthmateRepaymentUpdate);
        //                if (db.Commit() > 0)
        //                {
        //                    res.Status = true;
        //                    res.Msg = "Data Saved Successfully";
        //                    return Created(res.Msg, res);
        //                }
        //            }

        //        }
        //    }
        //    return Created(res.Msg, res);
        //}

        [HttpPost]
        [Route("AddArthmateRepaymentUpload")]
        public async Task<IHttpActionResult> AddArthmateRepaymentUpload()
        {
            RepaymentV2ResponseDc repaymentresponse = new RepaymentV2ResponseDc();
            CommonResponseDc res = new CommonResponseDc();
            ArthmateRepaymentUpdate arthmateRepaymentUpdate = new ArthmateRepaymentUpdate();

            var msg = "";
            var identity = User.Identity as ClaimsIdentity;
            int userid = 0;
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

            ArthMateHelper ArthMateHelper = new ArthMateHelper();

            var uploadDate = HttpContext.Current.Request.Files;

            if (HttpContext.Current.Request.Files.Count > 0)
            {
                var httpPostedFile = HttpContext.Current.Request.Files["file"];

                var filename = httpPostedFile.FileName;

                string extension = Path.GetExtension(filename);

                if (httpPostedFile != null)
                {
                    if (extension == ".xlsx" || extension == ".xls")
                    {
                        string path = HttpContext.Current.Server.MapPath("~/UploadedFiles/Payment");
                        string a1, b;
                        if (!Directory.Exists(path))
                        {
                            Directory.CreateDirectory(path);
                        }
                        a1 = DateTime.Now.ToString("ddMMyyyyHHmmss") + "_" + httpPostedFile.FileName;
                        b = Path.Combine(HttpContext.Current.Server.MapPath("~/UploadedFiles/Payment"), a1);

                        httpPostedFile.SaveAs(b);

                        byte[] buffer = new byte[httpPostedFile.ContentLength];

                        using (BinaryReader br = new BinaryReader(File.OpenRead(b)))
                        {
                            br.Read(buffer, 0, buffer.Length);
                        }

                        XSSFWorkbook hssfwb;
                        List<RepaymentV2Dc> uploaditemlist = new List<RepaymentV2Dc>();
                        using (MemoryStream memStream = new MemoryStream())
                        {
                            BinaryFormatter binForm = new BinaryFormatter();
                            memStream.Write(buffer, 0, buffer.Length);
                            memStream.Seek(0, SeekOrigin.Begin);
                            hssfwb = new XSSFWorkbook(memStream);
                            string sSheetName = hssfwb.GetSheetName(0);
                            ISheet sheet = hssfwb.GetSheet(sSheetName);
                            IRow rowData;
                            ICell cellData = null;
                            for (int iRowIdx = 0; iRowIdx <= sheet.LastRowNum; iRowIdx++)  //  iRowIdx = 0; HeaderRow
                            {
                                if (iRowIdx == 0)
                                {
                                    rowData = sheet.GetRow(iRowIdx);
                                    if (rowData != null)
                                    {
                                        string field = string.Empty;
                                        field = rowData.GetCell(0).ToString();
                                        if (field != "loan_id") //ReferenceNo
                                        {
                                            JavaScriptSerializer objJSSerializer = new JavaScriptSerializer();
                                            msg = objJSSerializer.Serialize("Header Name  " + field + " does not exist..try again");
                                            res.Msg = msg;
                                            res.Status = false;
                                            return Created(res.Msg, res);
                                        }
                                        field = string.Empty;
                                        field = rowData.GetCell(1).ToString();
                                        if (field != "partner_loan_id") //ReferenceNo
                                        {
                                            JavaScriptSerializer objJSSerializer = new JavaScriptSerializer();
                                            msg = objJSSerializer.Serialize("Header Name  " + field + " does not exist..try again");
                                            res.Msg = msg;
                                            res.Status = false;
                                            return Created(res.Msg, res);
                                        }
                                        field = string.Empty;
                                        field = rowData.GetCell(2).ToString();
                                        if (field != "txn_amount") //ReferenceNo
                                        {
                                            JavaScriptSerializer objJSSerializer = new JavaScriptSerializer();
                                            msg = objJSSerializer.Serialize("Header Name  " + field + " does not exist..try again");
                                            res.Msg = msg;
                                            res.Status = false;
                                            return Created(res.Msg, res);
                                        }
                                        field = string.Empty;
                                        field = rowData.GetCell(3).ToString();
                                        if (field != "txn_reference") //ReferenceNo
                                        {
                                            JavaScriptSerializer objJSSerializer = new JavaScriptSerializer();
                                            msg = objJSSerializer.Serialize("Header Name  " + field + " does not exist..try again");
                                            res.Msg = msg;
                                            res.Status = false;
                                            return Created(res.Msg, res);
                                        }
                                        field = string.Empty;
                                        field = rowData.GetCell(4).ToString();
                                        if (field != "txn_reference_datetime") //ReferenceNo
                                        {
                                            JavaScriptSerializer objJSSerializer = new JavaScriptSerializer();
                                            msg = objJSSerializer.Serialize("Header Name  " + field + " does not exist..try again");
                                            res.Msg = msg;
                                            res.Status = false;
                                            return Created(res.Msg, res);
                                        }
                                        field = string.Empty;
                                        field = rowData.GetCell(5).ToString();
                                        if (field != "utr_number") //ReferenceNo
                                        {
                                            JavaScriptSerializer objJSSerializer = new JavaScriptSerializer();
                                            msg = objJSSerializer.Serialize("Header Name  " + field + " does not exist..try again");
                                            res.Msg = msg;
                                            res.Status = false;
                                            return Created(res.Msg, res);
                                        }
                                        field = string.Empty;
                                        field = rowData.GetCell(6).ToString();
                                        if (field != "utr_date_time_stamp") //ReferenceNo
                                        {
                                            JavaScriptSerializer objJSSerializer = new JavaScriptSerializer();
                                            msg = objJSSerializer.Serialize("Header Name  " + field + " does not exist..try again");
                                            res.Msg = msg;
                                            res.Status = false;
                                            return Created(res.Msg, res);
                                        }
                                        field = string.Empty;
                                        field = rowData.GetCell(7).ToString();
                                        if (field != "payment_mode") //ReferenceNo
                                        {
                                            JavaScriptSerializer objJSSerializer = new JavaScriptSerializer();
                                            msg = objJSSerializer.Serialize("Header Name  " + field + " does not exist..try again");
                                            res.Msg = msg;
                                            res.Status = false;
                                            return Created(res.Msg, res);
                                        }
                                        field = string.Empty;
                                        field = rowData.GetCell(8).ToString();
                                        if (field != "label") //ReferenceNo
                                        {
                                            JavaScriptSerializer objJSSerializer = new JavaScriptSerializer();
                                            msg = objJSSerializer.Serialize("Header Name  " + field + " does not exist..try again");
                                            res.Msg = msg;
                                            res.Status = false;
                                            return Created(res.Msg, res);
                                        }
                                        field = string.Empty;
                                        field = rowData.GetCell(9).ToString();
                                        if (field != "created_by") //ReferenceNo
                                        {
                                            JavaScriptSerializer objJSSerializer = new JavaScriptSerializer();
                                            msg = objJSSerializer.Serialize("Header Name  " + field + " does not exist..try again");
                                            res.Msg = msg;
                                            res.Status = false;
                                            return Created(res.Msg, res);
                                        }
                                        field = string.Empty;
                                        field = rowData.GetCell(10).ToString();
                                        if (field != "amount_net_of_tds") //ReferenceNo
                                        {
                                            JavaScriptSerializer objJSSerializer = new JavaScriptSerializer();
                                            msg = objJSSerializer.Serialize("Header Name  " + field + " does not exist..try again");
                                            res.Msg = msg;
                                            res.Status = false;
                                            return Created(res.Msg, res);
                                        }
                                        field = string.Empty;
                                        field = rowData.GetCell(11).ToString();
                                        if (field != "tds_amount") //ReferenceNo
                                        {
                                            JavaScriptSerializer objJSSerializer = new JavaScriptSerializer();
                                            msg = objJSSerializer.Serialize("Header Name  " + field + " does not exist..try again");
                                            res.Msg = msg;
                                            res.Status = false;
                                            return Created(res.Msg, res);
                                        }
                                    }
                                }
                                else
                                {
                                    RepaymentV2Dc additem = new RepaymentV2Dc();
                                    rowData = sheet.GetRow(iRowIdx);
                                    if (rowData != null)
                                    {
                                        if (rowData.GetCell(0) != null)
                                        {
                                            if (rowData.GetCell(0).ToString().Trim() != "")
                                            {
                                                cellData = rowData.GetCell(0);
                                                rowData = sheet.GetRow(iRowIdx);
                                                if (rowData != null && cellData != null)
                                                {
                                                    string col1 = null;
                                                    cellData = rowData.GetCell(0);
                                                    col1 = cellData == null ? "" : cellData.ToString();
                                                    additem.loan_id = col1.Trim();

                                                    string col2 = null;
                                                    cellData = rowData.GetCell(1);
                                                    col2 = cellData == null ? "" : cellData.ToString();
                                                    additem.partner_loan_id = col2.Trim();

                                                    string col3 = null;
                                                    cellData = rowData.GetCell(2);
                                                    col3 = cellData == null ? "" : cellData.ToString();
                                                    if (col3 != "")
                                                    {
                                                        additem.txn_amount = Convert.ToDouble(col3);
                                                    }

                                                    string col4 = null;
                                                    cellData = rowData.GetCell(3);
                                                    col4 = cellData == null ? "" : cellData.ToString();
                                                    additem.txn_reference = col4.Trim();

                                                    string col5 = null;
                                                    cellData = rowData.GetCell(4);
                                                    col5 = cellData == null ? "" : cellData.ToString();

                                                    if (col5 != "")
                                                    {
                                                        additem.txn_reference_datetime = Convert.ToDateTime(col5).ToString("yyyy-MM-dd hh:mm:ss", CultureInfo.CreateSpecificCulture("en-US"));
                                                    }

                                                    string col6 = null;
                                                    cellData = rowData.GetCell(5);
                                                    col6 = cellData == null ? "" : cellData.ToString();
                                                    additem.utr_number = col6.Trim();

                                                    string col7 = null;
                                                    cellData = rowData.GetCell(6);
                                                    col7 = cellData == null ? "" : cellData.ToString();

                                                    if (col7 != "")
                                                    {
                                                        additem.utr_date_time_stamp = Convert.ToDateTime(col7).ToString("yyyy-MM-dd hh:mm:ss", CultureInfo.CreateSpecificCulture("en-US"));
                                                    }

                                                    string col8 = null;
                                                    cellData = rowData.GetCell(7);
                                                    col8 = cellData == null ? "" : cellData.ToString();
                                                    additem.payment_mode = col8.Trim();

                                                    string col9 = null;
                                                    cellData = rowData.GetCell(8);
                                                    col9 = cellData == null ? "" : cellData.ToString();
                                                    additem.label = col9.Trim();

                                                    string col10 = null;
                                                    cellData = rowData.GetCell(9);
                                                    col10 = cellData == null ? "" : cellData.ToString();
                                                    additem.created_by = col10.Trim();

                                                    string col11 = null;
                                                    cellData = rowData.GetCell(10);
                                                    col11 = cellData == null ? "" : cellData.ToString();
                                                    if (col7 != "")
                                                    {
                                                        additem.amount_net_of_tds = Convert.ToDouble(col11);
                                                    }
                                                    string col12 = null;
                                                    cellData = rowData.GetCell(11);
                                                    col12 = cellData == null ? "" : cellData.ToString();
                                                    if (col2 != "")
                                                    {
                                                        additem.tds_amount = Convert.ToDouble(col12);
                                                    }

                                                    uploaditemlist.Add(additem);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        if (uploaditemlist != null && uploaditemlist.Any())
                        {
                            RepaymentV2Dc data = new RepaymentV2Dc();
                            repaymentresponse = await ArthMateHelper.RepaymentV2(uploaditemlist, userid);
                            if (repaymentresponse != null)
                            {
                                using (var db = new AuthContext())
                                {
                                    arthmateRepaymentUpdate.Filename = httpPostedFile.FileName;
                                    arthmateRepaymentUpdate.CreatedBy = userid;
                                    arthmateRepaymentUpdate.DateOfupload = DateTime.Now;
                                    arthmateRepaymentUpdate.CreatedDate = DateTime.Now;
                                    arthmateRepaymentUpdate.ModifiedDate = null;
                                    arthmateRepaymentUpdate.IsActive = true;
                                    arthmateRepaymentUpdate.IsDeleted = false;
                                    arthmateRepaymentUpdate.ArthmateStatus = repaymentresponse.success;
                                    arthmateRepaymentUpdate.ArthmateStatusMsg = repaymentresponse.message;

                                    db.ArthmateRepaymentUpdate.Add(arthmateRepaymentUpdate);
                                    if (db.Commit() > 0)
                                    {
                                        res.Status = true;
                                        res.Msg = "Data Saved Successfully";
                                        return Created(res.Msg, res);
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        res.Msg = "Please Upload Valid File";
                        res.Status = false;
                    }
                }
            }

            return Created(res.Msg, res);
        }
    }




    public class NegativeAdressDc
    {
        public string Message { get; set; }
        public string MatchLevelWord { get; set; }
        public int TotalCompareWord { get; set; }
        public int TotalMatchWord { get; set; }
    }
}

