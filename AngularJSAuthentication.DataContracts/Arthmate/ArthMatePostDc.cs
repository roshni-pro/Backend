using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.Arthmate
{
    public class ArthMatePostDc
    {
        public class CommonResponseDc
        {
            public string Msg { get; set; }
            public bool Status { get; set; }
            public object Data { get; set; }
            public bool IsNotEditable { get; set; }
            public List<LeadDocUrlDc> leadDocUrlDcs { get; set; }
            public ArthMateOfferDc ArthMateOffer { get; set; }
            public string NameOnCard { get; set; }

        }

        public class LeadSequenceData
        {
            public int sequenceNo { get; set; }
            public int IsApproved { get; set; }
            public bool IsComplete { get; set; }
            public bool IsAadharOtp { get; set; }
        }
        public class LeadSequenceNoDc
        {
            public long Id { get; set; }
            public int SequenceNo { get; set; }
        }

        public class AddPersonalDetailDc
        {
            public long LeadMasterId { get; set; }
            public string first_name { get; set; }
            public string last_name { get; set; }
            public string father_fname { get; set; }
            public string father_lname { get; set; }
            public string dob { get; set; }
            public string gender { get; set; }
            public string type_of_addr { get; set; }
            public string resi_addr_ln1 { get; set; }
            public string resi_addr_ln2 { get; set; }
            public string city { get; set; }
            public string state { get; set; }
            public string pincode { get; set; }
            public string per_addr_ln1 { get; set; }
            public string per_addr_ln2 { get; set; }
            public string per_city { get; set; }
            public string per_state { get; set; }
            public string per_pincode { get; set; }
            public string alt_phone { get; set; }
            public string email_id { get; set; }
            public int SequenceNo { get; set; }
        }
        public class AddBusinessDetail
        {
            public long LeadMasterId { get; set; }
            public string bus_name { get; set; }
            public string doi { get; set; }
            public string bus_gstno { get; set; }
            public string bus_entity_type { get; set; }
            public string bus_pan { get; set; }
            public string bus_add_corr_line1 { get; set; }
            public string bus_add_corr_line2 { get; set; }
            public string bus_add_corr_city { get; set; }
            public string bus_add_corr_state { get; set; }
            public string bus_add_corr_pincode { get; set; }
            public string bus_add_per_line1 { get; set; }
            public string bus_add_per_line2 { get; set; }
            public string bus_add_per_city { get; set; }
            public string bus_add_per_state { get; set; }
            public string bus_add_per_pincode { get; set; }
            public double Bus_MonthlySalary { get; set; }
            public int SequenceNo { get; set; }
        }


        public class AddBankDetail
        {
            public long LeadMasterId { get; set; }
            public string fip_id { get; set; }  //for ceplr
            public string borro_bank_name { get; set; }
            public string borro_bank_ifsc { get; set; }
            public string borro_bank_acc_num { get; set; }
            public string GSTStatement { get; set; } //url
            //public string BankStatement { get; set; } //url
            public List<string> BankStatement { get; set; } //url
            public string PdfPassword { get; set; } //url
            public double EnquiryAmount { get; set; }
            public int SequenceNo { get; set; }
            public string AccType { get; set; }
            public string AccountHolderName { get; set; }

        }

        public class AddBenBankDetail
        {
            public long LeadmasterId { get; set; }
            public string ben_bank_name { get; set; }
            public string ben_bank_ifsc { get; set; }
            public string ben_bank_acc_num { get; set; }
            public string ben_accountHolderName { get; set; } //url
            public string ben_Typeofaccount { get; set; } //url
        }



        //public string ben_Accountholdername { get; set; }
        //public string ben_Typeofaccount { get; set; }



        public class LeadPostdc
        {
            public long Id { get; set; }
            public string SkCode { get; set; }
            public string MobileNo { get; set; }
            public string partner_loan_app_id { get; set; }
            public string partner_borrower_id { get; set; }
            public string bus_name { get; set; }
            public string doi { get; set; }
            public string bus_entity_type { get; set; }
            public string bus_pan { get; set; }
            public string bus_add_corr_line1 { get; set; }
            //IncomeSlab
            public string IncomeSlab { get; set; }
            public string bus_add_corr_line2 { get; set; }
            public string bus_add_corr_city { get; set; }
            public string bus_add_corr_state { get; set; }
            public string bus_add_corr_pincode { get; set; }
            public string bus_add_per_line1 { get; set; }
            public string bus_add_per_line2 { get; set; }
            public string bus_add_per_city { get; set; }
            public string bus_add_per_state { get; set; }
            public string bus_add_per_pincode { get; set; }
            public string first_name { get; set; }
            public string middle_name { get; set; }
            public string last_name { get; set; }
            public string father_fname { get; set; }
            public string father_lname { get; set; }
            public string type_of_addr { get; set; }
            public string resi_addr_ln1 { get; set; }
            public string resi_addr_ln2 { get; set; }
            public string city { get; set; }
            public string state { get; set; }
            public string pincode { get; set; }
            public string per_addr_ln1 { get; set; }
            public string per_addr_ln2 { get; set; }
            public string per_city { get; set; }
            public string per_state { get; set; }
            public string per_pincode { get; set; }
            public string appl_phone { get; set; }
            public string appl_pan { get; set; }
            public string email_id { get; set; }
            public string aadhar_card_num { get; set; }
            public string dob { get; set; }
            public string gender { get; set; }
            public string age { get; set; }
            public string residence_status { get; set; }
            public string bureau_pull_consent { get; set; }
            public bool IsLeadGenerate { get; set; }
            public string CompletionStage { get; set; }

            //new added
            public string borro_bank_name { get; set; }
            public string borro_bank_ifsc { get; set; }
            public string borro_bank_acc_num { get; set; }
            public string GSTStatement { get; set; }
            public string BankStatement { get; set; }


            public string marital_status { get; set; }
            public int SequenceNo { get; set; }
            public string qualification { get; set; }
        }
        public class LoanDocumentPostDc
        {
            public long LeadMasterId { get; set; }
            public string loan_app_id { get; set; }
            public string borrower_id { get; set; }
            public string partner_loan_app_id { get; set; }
            public string partner_borrower_id { get; set; }
            public string file_url { get; set; }
            public string code { get; set; }
            public string base64pdfencodedfile { get; set; }
            public string FrontUrl { get; set; }
            public string PdfPassword { get; set; }
        }


        public class UploadSlaDocDc
        {
            public string borrower_id { get; set; }
            public string partner_borrower_id { get; set; }
            public string partner_loan_app_id { get; set; }
            public string loan_app_id { get; set; }
            public string FrontFileUrl { get; set; }
            public string BackFileUrl { get; set; }
            public string DocumentTypeCode { get; set; }
            public string DocumentName { get; set; }
            public long Id { get; set; }
            public long DocumentMasterId { get; set; }
            public string PdfPassword { get; set; }

        }
        //new added on 1-11-2023 for document 
        public class PostArthmateDocumentDc
        {
            public long LeadMasterId { get; set; }
            //public string file_url { get; set; }
            //public string base64pdfencodedfile { get; set; }
            // public string DucumentName { get; set; }

        }
        public class SequenceDc
        {
            public string ScreenName { get; set; }
            public int SequenceNo { get; set; }
            public bool IsEditable { get; set; }
            public bool IsComplete { get; set; }
            public bool IsAadharOtp { get; set; }

        }

        public class PanDocumentDc
        {
            public string NameOnCard { get; set; }
            public string DateOfBirth { get; set; }
            public string IssuedDate { get; set; }
            public string FatherName { get; set; }
            public string OtherInfo { get; set; }
        }
        public class AScorePostDc
        {
            public long LeadMasterId { get; set; }
            public string request_id { get; set; }
        }
        public class GetAScorePostDc
        {
            public string request_id { get; set; }
            public string product { get; set; }
        }
        public class RepaymentScheduleDc
        {
            public long LeadMasterId { get; set; }
            public string loan_id { get; set; }
            public int company_id { get; set; }
            public string product_id { get; set; }
        }


        #region //for page view Dc

        public class LeadPagedata
        {
            public long Id { get; set; }
            public string SkCode { get; set; }
            public string MobileNo { get; set; }
            public string loan_app_id { get; set; }
            public string borrower_id { get; set; }
            public DateTime CreatedDate { get; set; }
            public string CustomerName { get; set; }
            public string appl_pan { get; set; }
            public string addr_id_num { get; set; }
            public string email_id { get; set; }
            public string partner_loan_app_id { get; set; }
            public string partner_borrower_id { get; set; }
            public string ScreenName { get; set; }
            public int SequenceNo { get; set; }
            public string Msg { get; set; }
            public int ButtonActive { get; set; } //0- Lead Generate,1- AScore, 2- Ceplar, 3- Offer,
            public int TotalCount { get; set; }
            public decimal loan_amount { get; set; }
            public decimal pricing { get; set; }
            public string loan_id { get; set; }


        }
        //new on 10-04-2024 Foir Export
        public class LeadPagedataExport
        {
            public long LeadID { get; set; }
            public string SkCode { get; set; }
            public string MobileNo { get; set; }
            public string loan_app_id { get; set; }
            public string borrower_id { get; set; }
            public DateTime LoginDate { get; set; }
            public string CustomerName { get; set; }
            public string appl_pan { get; set; }
            public string addr_id_num { get; set; }
            public string email_id { get; set; }
            public string partner_loan_app_id { get; set; }
            public string partner_borrower_id { get; set; }
            public string Stage { get; set; }
            public int SequenceNo { get; set; }
            public string Msg { get; set; }
            public int ButtonActive { get; set; } //0- Lead Generate,1- AScore, 2- Ceplar, 3- Offer,
            public int TotalCount { get; set; }
            public decimal loan_amount { get; set; }
            public decimal pricing { get; set; }
            public string loan_id { get; set; }
            public string business_establishment_proof_type { get; set; }
            public string Location { get; set; }
            public double? SanctionAmount { get; set; }
            public double? DisbursedAmount { get; set; }
            public DateTime? DisbursedDate { get; set; }


        }
        public class LeadPageDocuments
        {
            public string DocumentNumber { get; set; }
            public string FrontFileUrl { get; set; }
            public string BackFileUrl { get; set; }
            public string DocumentName { get; set; }
            public string DocumentTypeCode { get; set; }
            public long LeadMasterId { get; set; }
        }

        public class GetLeadDetailsDc
        {
            //public string SkCode { get; set; }
            //public string MobileNo { get; set; }
            //public string loan_app_id { get; set; }
            //public int CreatedDate { get; set; }
            //public string CustomerName { get; set; }

            public List<CustomerPersonalSDetailsDc> customerPersonalSDetailsDc { get; set; }
            public List<BankDetailsDc> bankDetailsDc { get; set; }
            public List<BusinessDetailDc> businessDetailDc { get; set; }
            public List<PanDetailsDc> panDetailsDc { get; set; }
            public List<SelfieDetailsDc> selfieDetailsDcs { get; set; }
            public List<AadharDetailDc> AadharDetailDcs { get; set; }
            public List<EagreementDc> eagreementDcs { get; set; }
            public List<AadharDetailDc> MsMeDataDcs { get; set; }//msmse
            public List<LeadActivityProgressesHistoriesDc> LeadActivityProgressesHistDcs { get; set; }
            public List<BankStatementDc> BankStatementDcs { get; set; }
        }
        public class CustomerPersonalSDetailsDc
        {
            public string DocumentName { get; set; }
            public string first_name { get; set; }
            public string last_name { get; set; }
            public string middle_name { get; set; }
            public string father_fname { get; set; }
            public string father_lname { get; set; }
            public string dob { get; set; }
            public string marital_status { get; set; }
            public string SkCode { get; set; }
            public int? age { get; set; }
            public string gender { get; set; }
            public string MobileNo { get; set; }
            public string email_id { get; set; }
            public string type_of_addr { get; set; }
            public string resi_addr_ln1 { get; set; }
            public string resi_addr_ln2 { get; set; }
            public string city { get; set; }
            public string state { get; set; }
            public string pincode { get; set; }
            public string per_addr_ln1 { get; set; }
            public string per_addr_ln2 { get; set; }
            public string per_city { get; set; }
            public string per_state { get; set; }
            public string residence_status { get; set; }
            public string qualification { get; set; }
            public int? SequenceNo { get; set; }
            public bool IsComplete { get; set; }
            public int IsApprove { get; set; }//new added on 16-11-2023
            public string Comment { get; set; }


            //a.loan_app_id,a.borrower_id,a.CreatedDate as LeadGenerateDate

            public string Lead_loan_app_id { get; set; }
            public string Lead_borrower_id { get; set; }
            public DateTime LeadGenerateDate { get; set; }

            public string CountryName { get; set; }

        }
        public class BankDetailsDc
        {
            public string borro_bank_name { get; set; }
            public string borro_bank_ifsc { get; set; }
            public string borro_bank_acc_num { get; set; }
            //new 
            public string borrower_id { get; set; }
            public string partner_borrower_id { get; set; }
            public double? annual_income_borro { get; set; }
            public double? cibil_score_borro { get; set; }
            public string FrontFileUrl { get; set; }
            public string DocumentName { get; set; }
            public bool IsComplete { get; set; }
            public int? SequenceNo { get; set; }
            public int IsApprove { get; set; }//new added on 16-11-2023
            public string Comment { get; set; }
            public double? EnquiryAmount { get; set; }
            public string PdfPassword { get; set; }
            public string AccountHolderName { get; set; }
            public string AccountType { get; set; }



            //new on 12_01_2024
            public string ben_Accountholdername { get; set; }
            public string ben_AccountNumber { get; set; }
            public string ben_Bankname { get; set; }
            public string ben_IFSCCode { get; set; }
            public string ben_Typeofaccount { get; set; }
        }
        public class BusinessDetailDc
        {
            public string DocumentName { get; set; }
            public string bus_name { get; set; }
            public string business_establishment_proof_type { get; set; }
            public string bus_add_corr_line1 { get; set; }
            public string IncomeSlab { get; set; }
            public string bus_entity_type { get; set; }
            public bool IsComplete { get; set; }
            public int? SequenceNo { get; set; }
            public int IsApprove { get; set; }//new added on 16-11-2023
            public string Comment { get; set; }

            // ADDED//
            // public string bus_name { get; set; }  
            public string bus_gstno { get; set; }
            public string doi { get; set; }
            public string bus_pan { get; set; }
            public string add_corr_line1 { get; set; }
            public string bus_add_corr_line2 { get; set; }

            public string bus_add_corr_city { get; set; }
            public string bus_add_corr_state { get; set; }
            public string bus_add_corr_pincode { get; set; }
            public string CountryName { get; set; }

            public double Bus_MonthlySalary { get; set; }
            //  public string a.bus_entity_type { get; set; }

            //   public string bus_entity_type { get; set; }






            //,a.bus_add_corr_line1--address1
            //,a.bus_add_corr_line2--address2
            //,a.bus_add_corr_city--city
            //,a.bus_add_corr_state --state
            //,a.bus_add_corr_pincode--pin
            //,a.CountryName -- Country





        }
        //Adhar Dc

        public class PanDetailsDc
        {
            public string DocumentName { get; set; }
            public string appl_pan { get; set; }
            public string FrontFileUrl { get; set; }
            public string BackFileUrl { get; set; }
            public string DocumentNumber { get; set; }
            public string Selfie { get; set; }
            public string OtherInfo { get; set; }
            public string NameOnCard { get; set; }
            public string DateOfBirth { get; set; }
            public bool IsComplete { get; set; }
            public int? SequenceNo { get; set; }
            public int IsApprove { get; set; }//new added on 16-11-2023
            public string Comment { get; set; }
        }
        public class SelfieDetailsDc
        {
            public string DocumentName { get; set; }
            public string appl_pan { get; set; }
            public string FrontFileUrl { get; set; }
            public string BackFileUrl { get; set; }
            public string DocumentNumber { get; set; }
            public string Selfie { get; set; }
            public string OtherInfo { get; set; }
            public string NameOnCard { get; set; }
            public string DateOfBirth { get; set; }
            public bool IsComplete { get; set; }
            public int? SequenceNo { get; set; }
            public int IsApprove { get; set; }//new added on 16-11-2023
            public string Comment { get; set; }
        }

        public class AadharDetailDc
        {
            public string DocumentName { get; set; }
            public string aadhar_card_num { get; set; }
            public string FrontFileUrl { get; set; }
            public string BackFileUrl { get; set; }
            public string DocumentNumber { get; set; }
            public string Selfie { get; set; }
            public string OtherInfo { get; set; }
            public bool IsComplete { get; set; }
            public int? SequenceNo { get; set; }
            public int IsApprove { get; set; }//new added on 16-11-2023
            public string Comment { get; set; }
            public string Msme_Bus_Name { get; set; }
            public string Msme_Bus_Type { get; set; }
            public int? Msme_VintageDays { get; set; }
        }

        public class EagreementDc
        {
            public string DocumentName { get; set; }
            public string FrontFileUrl { get; set; }
            public string BackFileUrl { get; set; }
            public string DocumentNumber { get; set; }
            public string Selfie { get; set; }
            public string OtherInfo { get; set; }
            public string NameOnCard { get; set; }
            public int? SequenceNo { get; set; }
            public int IsApprove { get; set; }//new added on 16-11-2023
            public string Comment { get; set; }
            public bool IsComplete { get; set; }
        }
        #endregion

        public class MSMERegVerificationDc
        {
            public long LeadMasterId { get; set; }
            public string MSMERegNum { get; set; }
            public string BusinessName { get; set; }
            public string BusinessType { get; set; }
            public int Vintage { get; set; }
            public string MSMECertificate { get; set; }
            public int? SequenceNo { get; set; }
            public bool IsComplete { get; set; }
        }

        //list withn lead dta
        public class LeadDocUrlDc
        {
            public string DocumentName { get; set; }
            public string FrontFileUrl { get; set; }
            public string BackFileUrl { get; set; }
            public string DocumentNumber { get; set; }
            public string Selfie { get; set; }
            public string OtherInfo { get; set; }
        }

        public class ScreenApproveRejectDc
        {
            public string DocumentName { get; set; }
            public long LeadmasterId { get; set; }
            public bool isApprove { get; set; }
            public int SequenceNo { get; set; }
            public string Comment { get; set; }
        }

        public class CeplrPdfReportDc
        {
            //public long LeadMasterId { get; set; }
            public string email { get; set; }
            public string file { get; set; }
            public string ifsc_code { get; set; }
            public string fip_id { get; set; }
            public string mobile { get; set; }
            public string name { get; set; }
            public string file_password { get; set; } //if pass. protected file is uploaded
            public string configuration_uuid { get; set; }
            public string callback_url { get; set; }
        }
        public class CeplrPdf_SpData
        {
            public long Id { get; set; }
            public string DocumentName { get; set; }
            public string FrontFileUrl { get; set; }
            public string BackFileUrl { get; set; }
            public string DocumentNumber { get; set; }
            public string email_id { get; set; }
            public string borro_bank_ifsc { get; set; }
            public string borro_bank_name { get; set; }
            public string MobileNo { get; set; }
            public string first_name { get; set; }
            public string last_name { get; set; }
            public string PdfPassword { get; set; }
        }
        public class CeplrBasicReportDc
        {
            // public long LeadMasterId { get; set; }
            //public string CustomerUid { get; set; }
            public string start_date { get; set; }
            public string end_date { get; set; }

        }
        public class CeplrBasicReportPayload
        {
            public long LeadmasterId { get; set; }
            //public string CustomerUid { get; set; }
            public string start_date { get; set; }
            public string end_date { get; set; }

        }

        //for LeadBackgroundRuns 
        public class LeadBackgroundRunDc
        {
            public long LeadMasterId { get; set; }
            public long ArthmateActivityMastersId { get; set; }
            public string ArthmateActivityMastersApiName { get; set; }
            public string ReqJson { get; set; }
            public string ResJson { get; set; }
            public string JsonType { get; set; }
            public string Status { get; set; }
            public string ScreenName { get; set; }
            public DateTime CreatedDate { get; set; }

        }

        public class AadharKYCValidateDc
        {
            public long LeadMasterId { get; set; }
            public int Tenure { get; set; }
        }


        //list withn lead dta
        public class ArthMateOfferDc
        {
            public double loan_amt { get; set; }
            public double interest_rt { get; set; }
            public int loan_tnr { get; set; }
            public string loan_tnr_type { get; set; } //Month, Year
            public double Orignal_loan_amt { get; set; }

        }
        public class ArthmateStateCodeDc
        {
            public int StateCode { get; set; }
            public string State { get; set; }
        }

        #region Penny Drop Dc


        public class PennyResponseDc
        {
            public Data data { get; set; }
            public bool success { get; set; }
            public string message { get; set; }
        }
        public class PennyDropReqJson
        {
            public string ifsc { get; set; }
            public string account_number { get; set; }
            public string loan_app_id { get; set; }
        }
        public class Data
        {
            public Result result { get; set; }
            public string request_id { get; set; }
            public string statuscode { get; set; }
        }

        public class Result
        {
            public string accountNumber { get; set; }
            public string ifsc { get; set; }
            public string accountName { get; set; }
            public string bankResponse { get; set; }
            public bool bankTxnStatus { get; set; }
        }


        #region  GetLoan Details Dc


        public class GetLoanDetailsDc
        {
            public LoanDetails loanDetails { get; set; }
            public string message { get; set; }


        }
        public class LoanDetails
        {
            public int _id { get; set; }
            public int co_lender_assignment_id { get; set; }
            public string loan_app_id { get; set; }
            public string loan_id { get; set; }
            public string borrower_id { get; set; }
            public string partner_loan_app_id { get; set; }
            public string partner_loan_id { get; set; }
            public string partner_borrower_id { get; set; }
            public int company_id { get; set; }
            public int co_lender_id { get; set; }
            public string co_lend_flag { get; set; }
            public int product_id { get; set; }
            public string itr_ack_no { get; set; }
            public string loan_app_date { get; set; }
            public int penal_interest { get; set; }
            public int bounce_charges { get; set; }
            public string marital_status { get; set; }
            public string sanction_amount { get; set; }
            public string gst_on_pf_amt { get; set; }
            public string gst_on_pf_perc { get; set; }
            public string repayment_type { get; set; }
            public DateTime first_inst_date { get; set; }
            public string net_disbur_amt { get; set; }
            public DateTime final_approve_date { get; set; }
            public string final_remarks { get; set; }
            public string foir { get; set; }
            public string status { get; set; }
            public int stage { get; set; }
            public string upfront_interest { get; set; }
            public string exclude_interest_till_grace_period { get; set; }
            public string borro_bank_account_type { get; set; }
            public string borro_bank_account_holder_name { get; set; }
            public string business_vintage_overall { get; set; }
            public string loan_int_amt { get; set; }
            public string loan_int_rate { get; set; }
            public string conv_fees { get; set; }
            public string processing_fees_amt { get; set; }
            public string processing_fees_perc { get; set; }
            public string tenure { get; set; }
            public string tenure_type { get; set; }
            public string int_type { get; set; }
            public string borro_bank_ifsc { get; set; }
            public string borro_bank_acc_num { get; set; }
            public string borro_bank_name { get; set; }
            public string first_name { get; set; }
            public string last_name { get; set; }
            public string ninety_plus_dpd_in_last_24_months { get; set; }
            public int current_overdue_value { get; set; }
            public string dpd_in_last_9_months { get; set; }
            public string dpd_in_last_3_months { get; set; }
            public string dpd_in_last_6_months { get; set; }
            public string bureau_score { get; set; }
            public string loan_amount_requested { get; set; }
            public string insurance_company { get; set; }
            public string credit_card_settlement_amount { get; set; }
            public string emi_amount { get; set; }
            public string emi_allowed { get; set; }
            public string bene_bank_name { get; set; }
            public string bene_bank_acc_num { get; set; }
            public string bene_bank_ifsc { get; set; }
            public string bene_bank_account_holder_name { get; set; }
            public string bene_bank_account_type { get; set; }
            public string igst_amount { get; set; }
            public string cgst_amount { get; set; }
            public string sgst_amount { get; set; }
            public int emi_count { get; set; }
            public string broken_interest { get; set; }
            public int dpd_in_last_12_months { get; set; }
            public int dpd_in_last_3_months_credit_card { get; set; }
            public int dpd_in_last_3_months_unsecured { get; set; }
            public string broken_period_int_amt { get; set; }
            public int dpd_in_last_24_months { get; set; }
            public int enquiries_bureau_30_days { get; set; }
            public int cnt_active_unsecured_loans { get; set; }
            public int total_overdues_in_cc { get; set; }
            public double insurance_amount { get; set; }
            public int bureau_outstanding_loan_amt { get; set; }
            public string subvention_fees_amount { get; set; }
            public string gst_on_subvention_fees { get; set; }
            public string cgst_on_subvention_fees { get; set; }
            public string sgst_on_subvention_fees { get; set; }
            public string igst_on_subvention_fees { get; set; }
            public string purpose_of_loan { get; set; }
            public string business_name { get; set; }
            public string co_app_or_guar_name { get; set; }
            public string co_app_or_guar_address { get; set; }
            public string co_app_or_guar_mobile_no { get; set; }
            public string co_app_or_guar_pan { get; set; }
            public string co_app_or_guar_bureau_score { get; set; }
            public string business_address_ownership { get; set; }
            public string business_pan { get; set; }
            public string other_business_reg_no { get; set; }
            public int enquiries_in_last_3_months { get; set; }
            public double gst_on_conv_fees { get; set; }
            public double cgst_on_conv_fees { get; set; }
            public double sgst_on_conv_fees { get; set; }
            public double igst_on_conv_fees { get; set; }
            public string gst_on_application_fees { get; set; }
            public string cgst_on_application_fees { get; set; }
            public string sgst_on_application_fees { get; set; }
            public string igst_on_application_fees { get; set; }
            public string interest_type { get; set; }
            public double conv_fees_excluding_gst { get; set; }
            public string application_fees_excluding_gst { get; set; }
            public string emi_obligation { get; set; }
            public string a_score_request_id { get; set; }
            public int a_score { get; set; }
            public int b_score { get; set; }
            public int offered_amount { get; set; }
            public double offered_int_rate { get; set; }
            public double monthly_average_balance { get; set; }
            public double monthly_imputed_income { get; set; }
            public string party_type { get; set; }
            public string co_app_or_guar_dob { get; set; }
            public string co_app_or_guar_gender { get; set; }
            public string co_app_or_guar_ntc { get; set; }
            public string residence_vintage { get; set; }
            public string business_entity_type { get; set; }
            public string udyam_reg_no { get; set; }
            public string program_type { get; set; }
            public int written_off_settled { get; set; }
            public string upi_handle { get; set; }
            public string upi_reference { get; set; }
            public int fc_offer_days { get; set; }
            public string foreclosure_charge { get; set; }
            public DateTime created_at { get; set; }
            public DateTime updated_at { get; set; }
            public int __v { get; set; }
            public string user_id { get; set; }
            public string aadhar_card_hash { get; set; }
            public string aadhar_verified { get; set; }
            public int prev_stage { get; set; }
            public string prev_status { get; set; }
            public List<Validation> validations { get; set; }
        }



        public class Validation
        {
            public string code { get; set; }
            public bool status { get; set; }
            public string remarks { get; set; }
        }



        #endregion


        #endregion

        public class LeadActivityProgressesHistoriesDc
        {
            public long LeadMasterId { get; set; }
            public long? ActivityMasterId { get; set; }
            public string ActivityName { get; set; }
            public string Comments { get; set; }
            public int? CreatedBy { get; set; }
            public DateTime? CreatedDate { get; set; }
            public int? ModifiedBy { get; set; }
            public DateTime? ModifiedDate { get; set; }
        }
        public class LeadActivityProgressStatusDc
        {
            public string ActivityName { get; set; }
            public bool IsVerified { get; set; }
        }

        public class ActivityResponseDc
        {
            public string ApiName { get; set; }
            public string RequestResponseMsg { get; set; }
        }

        public class DisbursedDetailDc
        {
            //loan_id,  sanction_amount, InsurancePremium, MonthlyEMI, borro_bank_acc_num
            public string loan_id { get; set; }
            public string sanction_amount { get; set; }
            public int InsurancePremium { get; set; }
            public string MonthlyEMI { get; set; }
            public string borro_bank_acc_num { get; set; }
            public int company_id { get; set; }
            public string product_id { get; set; }
        }

        public class RePaymentScheduleDataDc
        {
            public string sanction_amount { get; set; }
            public int InsurancePremium { get; set; }
            public string MonthlyEMI { get; set; }
            public string borro_bank_acc_num { get; set; }
            public List<repay_scheduleDetails> rows { get; set; }
        }




        public class ApiParamForTesting
        {
            public long Id { get; set; }
            public string DocumentName { get; set; }
            public string FrontFileUrl { get; set; }
            public string BackFileUrl { get; set; }
            public string DocumentNumber { get; set; }
            public string email_id { get; set; }
            public string borro_bank_ifsc { get; set; }
            public string borro_bank_name { get; set; }
            public string MobileNo { get; set; }
            public string first_name { get; set; }
            public string last_name { get; set; }
            public string PdfPassword { get; set; }
            //new added on 19/12/2023
            public string aa_fip_id { get; set; }
            public string pdf_fip_id { get; set; }
            public string fip_name { get; set; }
            public string ApiSecretKey { get; set; }
            public string TokenKey { get; set; }
            public string URL { get; set; }
            //RequestAscorApi
            public string dob { get; set; }
            public string appl_pan { get; set; }
            public string gender { get; set; }
            public string bus_add_corr_line1 { get; set; }
            public string bus_add_corr_city { get; set; }
            public string bus_add_corr_pincode { get; set; }
            public string EnquiryAmount { get; set; }
            public string loan_app_id { get; set; }
            public string tenure { get; set; }
            public string state_code { get; set; }
            public string enquiry_purpose { get; set; }
            public string enquiry_stage { get; set; }
            public string en_acc_account_number_1 { get; set; }
            public string bureau_type { get; set; }
            public string consent { get; set; }
            public string consent_timestamp { get; set; }



            //colender
            public string appl_phone { get; set; }
            public string per_addr_ln1 { get; set; }
            public string city { get; set; }
            public string state { get; set; }
            public string pincode { get; set; }
            public string Ascore_request_id { get; set; }
            public string cepler_customer_id { get; set; }
            public string configuration_uuid { get; set; }
            public string callback_url { get; set; }
            public string interest_rate { get; set; }
            public string product_type_code { get; set; }
            public string sanction_amount { get; set; }
            public string dscr { get; set; }
            public string monthly_income { get; set; }
            //public string consent { get; set; }
            //public string consent_timestamp { get; set; }


        }



    }
    public class AddBankStatementDc
    {
        public long LeadMasterId { get; set; }
        public List<string> BankStatement { get; set; } //url
        public string Remark { get; set; }
    }
    public class AddStampDataDc
    {

        public long LeadMasterId { get; set; }
        public int StampNumber { get; set; }
        public double StampAmount { get; set; }
        public string StampUrl { get; set; } //imageurl
        public string UsedFor { get; set; }
        public string PartnerName { get; set; }
        public string Purpose { get; set; }

    }

    public class BankStatementDc
    {
        public long LeadBankStatementId { get; set; }
        public int Sequence { get; set; }
        public string StatementFile { get; set; } //url
        public string Remark { get; set; }
        public bool IsSuccess { get; set; }
    }

    public class GetStampDataDC
    {
        public long Id { get; set; }
        public string UsedFor { get; set; }
        public string PartnerName { get; set; }
        public double StampAmount { get; set; }
        public DateTime? DateofUtilisation { get; set; }
        public int StampPaperNo { get; set; }
        public long LeadmasterId { get; set; }
        public bool IsStampUsed { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public string CreatedBy { get; set; }
        public string ModifiedBy { get; set; }
        public string StampUrl { get; set; }
        public string LeadName { get; set; }
        public string MobileNo { get; set; }
        public string SkCode { get; set; }
    }

    public class GetStampAutoFilledDC
    {
        public string UsedFor { get; set; }
        public int StampAmount { get; set; }
        public string PartnerName { get; set; }
        public string Purpose { get; set; }
    }

    public class RepaymentV2Dc
    {
        public string loan_id { get; set; }
        public string partner_loan_id { get; set; }
        public double txn_amount { get; set; }
        public string txn_reference { get; set; }
        public string txn_reference_datetime { get; set; }
        public string utr_number { get; set; }
        public string utr_date_time_stamp { get; set; }
        public string payment_mode { get; set; }
        public string label { get; set; }
        public string created_by { get; set; }
        public double amount_net_of_tds { get; set; }
        public double tds_amount { get; set; }
    }
    public class RepaymentV2ResponseDc
    {
        public bool success { get; set; }
        public string message { get; set; }
    }



    }
