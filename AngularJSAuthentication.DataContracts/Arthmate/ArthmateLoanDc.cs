using AngularJSAuthentication.Model.Arthmate;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using Newtonsoft.Json;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.Arthmate
{

    #region  error response dc
    public class GetUrlTokenDc
    {
        public string url { get; set; }
        public string token { get; set; }
        public string CompanyCode { get; set; }
        public long id { get; set; }
        public string ApiSecretKey { get; set; }
        public string TokenKey { get; set; }
    }
    public class ArthMateLeadPostDc
    {
        public string partner_loan_app_id { get; set; }
        public string partner_borrower_id { get; set; }
        public string bus_name { get; set; }
        public string doi { get; set; }
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
        public string first_name { get; set; }
        public string middle_name { get; set; } //new added
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
    }

    public class LeadResponseErrorDc
    {
        public bool success { get; set; }
        public string message { get; set; }
        public string errorCode { get; set; }
        public ArthmateLoanDc data { get; set; }
        public List<LeadDatum> LeadDatum { get; set; }
    }
    public class LeadResponseDc
    {
        public bool success { get; set; }
        public string message { get; set; }
        public string errorCode { get; set; }
        public ArthmateLoanDc data { get; set; }
        public List<LeadDatum> LeadDatum { get; set; }
    }
    public class ArthmateLoanDc
    {
        public List<ExactErrorRow> exactErrorRows { get; set; }
        public List<ErrorRow> errorRows { get; set; }
        public List<PreparedbiTmpl> preparedbiTmpl { get; set; }

    }
    public class MissingColumn
    {
        public string dept { get; set; }
        public string field { get; set; }
        public string title { get; set; }
        public string type { get; set; }
        public string validationmsg { get; set; }
        public string isOptional { get; set; }
        public string @checked { get; set; }
        public string isCommon { get; set; }
    }
    public class LeadDatum
    {
        public int? address_same { get; set; }
        public List<string> borrowers_id { get; set; }
        public List<string> guarantors_id { get; set; }
        public int? _id { get; set; }
        public int? product_id { get; set; }
        public int? company_id { get; set; }
        public int? loan_schema_id { get; set; }
        public string loan_app_id { get; set; }
        public string borrower_id { get; set; }
        public string partner_loan_app_id { get; set; }
        public string partner_borrower_id { get; set; }
        public string first_name { get; set; }
        public string last_name { get; set; }
        public string type_of_addr { get; set; }
        public string resi_addr_ln1 { get; set; }
        public string resi_addr_ln2 { get; set; }
        public string city { get; set; }
        public string state { get; set; }
        public int? pincode { get; set; }
        public string per_addr_ln1 { get; set; }
        public string per_addr_ln2 { get; set; }
        public string per_city { get; set; }
        public string per_state { get; set; }
        public int? per_pincode { get; set; }
        public string appl_phone { get; set; }
        public string appl_pan { get; set; }
        public string email_id { get; set; }
        public string aadhar_card_num { get; set; }
        public string dob { get; set; }
        public string gender { get; set; }
        public string addr_id_num { get; set; }
        public string age { get; set; }
        public string lead_status { get; set; }
        public string residence_status { get; set; }
        public string cust_id { get; set; }
        public string loan_status { get; set; }
        public string status { get; set; }
        public int? is_deleted { get; set; }
        public string bureau_pull_consent { get; set; }
        public string aadhaar_fname { get; set; }
        public string aadhaar_lname { get; set; }
        public string aadhaar_dob { get; set; }
        public string aadhaar_pincode { get; set; }
        public string parsed_aadhaar_number { get; set; }
        public string pan_fname { get; set; }
        public string pan_lname { get; set; }
        public string pan_dob { get; set; }
        public string pan_father_fname { get; set; }
        public string pan_father_lname { get; set; }
        public string parsed_pan_number { get; set; }
        public string father_fname { get; set; }
        public string father_lname { get; set; }
        public object urc_parsing_data { get; set; }
        public object urc_parsing_status { get; set; }
        public string bus_add_corr_line1 { get; set; }
        public string bus_add_corr_line2 { get; set; }
        public string bus_add_corr_city { get; set; }
        public string bus_add_corr_state { get; set; }
        public string bus_add_corr_pincode { get; set; }
        public string bus_add_per_line1 { get; set; }
        public string bus_add_per_line2 { get; set; }
        public string bus_add_per_city { get; set; }
        public string bus_pan { get; set; }
        public string bus_add_per_state { get; set; }
        public string bus_add_per_pincode { get; set; }
        public string bus_name { get; set; }
        public DateTime doi { get; set; }
        public string bus_entity_type { get; set; }
        public List<string> coborrower { get; set; }
        public List<string> guarantor { get; set; }
        public DateTime created_at { get; set; }
        public List<string> additional_docs { get; set; }
        public DateTime updated_at { get; set; }
        public int? __v { get; set; }
        public string scr_match_count { get; set; }
        public string scr_match_result { get; set; }
    }
    public class ErrorRow
    {
        public string partner_loan_app_id { get; set; }
        public string partner_borrower_id { get; set; }
        public string bus_name { get; set; }
        public string doi { get; set; }
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
        public string first_name { get; set; }
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
    }
    public class ExactErrorRow
    {
        public string doi { get; set; }
        public string bus_pan { get; set; }
        public string pincode { get; set; }
        public string per_pincode { get; set; }
        public string appl_phone { get; set; }
        public string appl_pan { get; set; }
        public string email_id { get; set; }
        public string aadhar_card_num { get; set; }
        public string dob { get; set; }
        public string age { get; set; }
    }
    public class PreparedbiTmpl
    {
        public string partner_loan_app_id { get; set; }
        public string partner_borrower_id { get; set; }
        public string loan_app_id { get; set; }
        public string borrower_id { get; set; }
    }
    #endregion


    #region //by vishal

    public class PanVerificationRequestJson
    {
        public string pan { get; set; }
        public string loan_app_id { get; set; }
        public string consent { get; set; }
        public string consent_timestamp { get; set; }
        //public leadid
    }
    public class PanVerificationRequestV3
    {
        public string pan { get; set; }
        public string name { get; set; }
        public string father_name { get; set; }
        public string dob { get; set; }
        public string loan_app_id { get; set; }
        public string consent { get; set; }
        public string consent_timestamp { get; set; }
    }
    public class PanNsdlResponse
    {
        public string kyc_id { get; set; }
        public PanApiResponse data { get; set; }
        public bool success { get; set; }
        public string message { get; set; }
        public string KYCResponse { get; set; }
    }
    public class PanApiResponse
    {
        public string pan_number { get; set; }
        public string first_name { get; set; }
        public string middle_name { get; set; }
        public string last_name { get; set; }
        public string pan_holder_title { get; set; }
        public string pan_last_updated { get; set; }
        public string name_on_card { get; set; }
        public string seeding_status { get; set; }
        public string pan_status { get; set; }
        public string status { get; set; }
        public string msg { get; set; }
    }
    /// <summary>
    /// /new Pna 
    /// </summary>
    /// 

    #region PAN validation API
    //new pan api Response:=> PAN validation API
    public class PanResponse
    {
        public Results result { get; set; }
        public string request_id { get; set; }

        [JsonProperty("status-code")]
        public string statuscode { get; set; }
    }

    public class Results
    {
        public string name { get; set; }
    }
    public class PanValidationRspnsNew
    {
        public string kyc_id { get; set; }
        public PanResponse data { get; set; }
        public bool success { get; set; }
        public string request_id { get; set; }
        public string status { get; set; }
        public string message { get; set; }
    }

    //new on 20-03-2024 For Pan Validation V3

    public class PanResponsev3
    {
        public string pan_number { get; set; }
        public string seeding_status { get; set; }
        public string name_match { get; set; }
        public string dob_match { get; set; }
        public string status { get; set; }
        public string msg { get; set; }
    }

    public class PanValidationRspnsV3
    {
        public string kyc_id { get; set; }
        public PanResponsev3 data { get; set; }
        public Message messages { get; set; }
        public bool success { get; set; }
        public string message { get; set; }
        public string KYCResponse { get; set; }
    }
    public class Message
    {
        public string type { get; set; }
        public string validationmsg { get; set; }
        public string @checked { get; set; }
        public string field { get; set; }
    }




    #endregion

    public class JsonXmlRequest
    {
        public string partner_loan_app_id { get; set; }
        public string partner_borrower_id { get; set; }
        public string loan_app_id { get; set; }
        public string borrower_id { get; set; }
        public string code { get; set; }
        public string base64pdfencodedfile { get; set; }
    }
    public class ValidationDocResponse
    {
        public bool success { get; set; }
        public string message { get; set; }
    }



    public class AddSelfieDc
    {
        public long LeadmasterId { get; set; }
        public string Selfie { get; set; }
        public int SequenceNo { get; set; }
    }

    //MsME ADD DC
    public class AddMSMEDc
    {
        public long LeadmasterId { get; set; }
        public string Selfie { get; set; }
        public int SequenceNo { get; set; }
    }



    public class LeadPanNewDc
    {
        public long LeadmasterId { get; set; }
        public string PanNo { get; set; } 
        public string Name { get; set; } //ok required for new change in api 
        public string dob { get; set; } //new 22-03-2024 required for new change in api  (dd/mm/yyyy)
        public string PanImage { get; set; } 
        public string Selfie { get; set; }
        public int SequenceNo { get; set; }

    }


    public class LeadPanNewKarzaDc
    {
        public long LeadmasterId { get; set; }
        public string PanNo { get; set; }
        public string name { get; set; }
        public string dob { get; set; }
        public int SequenceNo { get; set; }
    }

    public class PanNewResponse
    {
        public bool status { get; set; }
        public string Message { get; set; }
    }

    public class LoanApiRequestDc
    {
        public string partner_loan_app_id { get; set; }
        public string partner_borrower_id { get; set; }
        public string loan_app_id { get; set; }
        public string borrower_id { get; set; }
        public string partner_loan_id { get; set; }
        public string a_score_request_id { get; set; }
        public string co_lender_assignment_id { get; set; }
        public string marital_status { get; set; }
        public string residence_vintage { get; set; }
        public string loan_app_date { get; set; }
        public string loan_amount_requested { get; set; }
        public string sanction_amount { get; set; }
        public string processing_fees_perc { get; set; }
        public string processing_fees_amt { get; set; }
        public string gst_on_pf_perc { get; set; }
        public string gst_on_pf_amt { get; set; }
        public string conv_fees { get; set; }
        public string insurance_amount { get; set; }
        public string net_disbur_amt { get; set; }
        public string int_type { get; set; }
        public string loan_int_rate { get; set; }
        public string loan_int_amt { get; set; }
        public string broken_period_int_amt { get; set; }
        public string repayment_type { get; set; }
        public string tenure_type { get; set; }
        public string tenure { get; set; }
        public string first_inst_date { get; set; }
        public string emi_amount { get; set; }
        public string emi_count { get; set; }
        public string final_approve_date { get; set; }
        public string final_remarks { get; set; }
        public string borro_bank_name { get; set; }
        public string borro_bank_acc_num { get; set; }
        public string borro_bank_ifsc { get; set; }
        public string borro_bank_account_holder_name { get; set; }
        public string borro_bank_account_type { get; set; }
        public string bene_bank_name { get; set; }
        public string bene_bank_acc_num { get; set; }
        public string bene_bank_ifsc { get; set; }
        public string bene_bank_account_holder_name { get; set; }
        public string bene_bank_account_type { get; set; }
        public string itr_ack_no { get; set; }
        public string business_name { get; set; }
        public string business_address_ownership { get; set; }
        public string program_type { get; set; }
        public string business_entity_type { get; set; }
        public string business_pan { get; set; }
        public string gst_number { get; set; }
        public string udyam_reg_no { get; set; }
        public string other_business_reg_no { get; set; }
        public string business_vintage_overall { get; set; }
        public string business_establishment_proof_type { get; set; }
        public string co_app_or_guar_name { get; set; }
        public string co_app_or_guar_dob { get; set; }
        public string co_app_or_guar_gender { get; set; }
        public string co_app_or_guar_address { get; set; }
        public string co_app_or_guar_mobile_no { get; set; }
        public string co_app_or_guar_pan { get; set; }
        public string relation_with_applicant { get; set; }
        public string co_app_or_guar_bureau_type { get; set; }
        public string co_app_or_guar_bureau_score { get; set; }
        public string co_app_or_guar_ntc { get; set; }
        public string insurance_company { get; set; }
        public string purpose_of_loan { get; set; }
        public string emi_obligation { get; set; }

    }
    public class LoanApiResponse
    {
        public bool success { get; set; }
        public string message { get; set; }
        public string errorCode { get; set; }
        //public LoanApiData data { get; set; }
        public ArthmateLoanDc data { get; set; }
    }
    public class GetLoanApiResponse
    {
        public bool success { get; set; }
        public string message { get; set; }
        public List<Datum> data { get; set; }
    }

    public class LoanApiData
    {
        public string loan_app_id { get; set; }
        public string loan_id { get; set; }
        public string borrower_id { get; set; }
        public string partner_loan_app_id { get; set; }
        public string partner_loan_id { get; set; }
        public string partner_borrower_id { get; set; }
        public int company_id { get; set; }
        public int product_id { get; set; }
        public string loan_app_date { get; set; }
        public string religion { get; set; }
        public string sanction_amount { get; set; }
        public string gst_on_pf_amt { get; set; }
        public string gst_on_pf_perc { get; set; }
        public string net_disbur_amt { get; set; }
        public string status { get; set; }
        public int stage { get; set; }
        public string exclude_interest_till_grace_period { get; set; }
        public int overdue_days { get; set; }
        public string customer_risk_segment { get; set; }
        public string vintage_current_employer { get; set; }
        public string customer_type { get; set; }
        public string borro_bank_account_type { get; set; }
        public string borro_bank_account_holder_name { get; set; }
        public string loan_int_rate { get; set; }
        public string processing_fees_amt { get; set; }
        public string processing_fees_perc { get; set; }
        public string tenure { get; set; }
        public string tenure_type { get; set; }
        public string int_type { get; set; }
        public string borro_bank_ifsc { get; set; }
        public string borro_bank_acc_num { get; set; }
        public string borro_bank_name { get; set; }
        public string umrn { get; set; }
        public string father_or_spouse_name { get; set; }
        public string first_name { get; set; }
        public string last_name { get; set; }
        public int current_overdue_value { get; set; }
        public string bureau_score { get; set; }
        public string bureau_type { get; set; }
        public string monthly_income { get; set; }
        public string bounces_in_one_month { get; set; }
        public string bounces_in_six_month { get; set; }
        public string number_of_deposit_txn { get; set; }
        public string number_of_withdrawal_txn { get; set; }
        public string regular_salary_credit { get; set; }
        public string loan_amount_requested { get; set; }
        public string bene_bank_name { get; set; }
        public string bene_bank_acc_num { get; set; }
        public string bene_bank_ifsc { get; set; }
        public string bene_bank_account_holder_name { get; set; }
        public DateTime created_at { get; set; }
        public DateTime updated_at { get; set; }
        public int _id { get; set; }
        public int __v { get; set; }
    }




    public class RePaymentApiReq
    {
        public string loan_id { get; set; }
        public string partner_loan_id { get; set; }
        public int txn_amount { get; set; }
        public string txn_reference { get; set; }
        public string txn_reference_datetime { get; set; }
        public string utr_number { get; set; }
        public string utr_date_time_stamp { get; set; }
        public string payment_mode { get; set; }
        public string label { get; set; }
        public string created_by { get; set; }
        public int amount_net_of_tds { get; set; }
        public int tds_amount { get; set; }
    }


    public class Data
    {
        public List<ExactErrorRow> exactErrorRows { get; set; }
        public List<ErrorRow> errorRows { get; set; }
        public List<RepaymentMissingColumn> missingColumns { get; set; }

        public bool flag { get; set; }
        public string message { get; set; }
        public DateTime timestamp { get; set; }
        public Body body { get; set; }
    }

    public class RepaymentMissingColumn
    {
        public string field { get; set; }
        public string type { get; set; }
        public string @checked { get; set; }
        public string validationmsg { get; set; }
    }

    public class RepaymentAllResponse
    {
        public string message { get; set; }
        public string errorCode { get; set; }
        public bool success { get; set; }
        public Data data { get; set; }

    }
    public class Body
    {
        public string flag { get; set; }
        public string details { get; set; }
    }






    //get lead Api
    public class GetLeadResponse
    {
        public int _id { get; set; }
        public int product_id { get; set; }
        public int company_id { get; set; }
        public int loan_schema_id { get; set; }
        public string loan_app_id { get; set; }
        public string borrower_id { get; set; }
        public string partner_loan_app_id { get; set; }
        public string partner_borrower_id { get; set; }
        public string first_name { get; set; }
        public string last_name { get; set; }
        public string resi_addr_ln1 { get; set; }
        public string city { get; set; }
        public string state { get; set; }
        public int pincode { get; set; }
        public string appl_phone { get; set; }
        public string appl_pan { get; set; }
        public string aadhar_card_num { get; set; }
        public string dob { get; set; }
        public string addr_id_num { get; set; }
        public DateTime created_at { get; set; }
        public int __v { get; set; }
        public string loan_id { get; set; }
        public bool success { get; set; }
        public string message { get; set; }
    }


    //gst verification request / response DC



    public class GstVerifyRequest
    {
        public string loan_app_id { get; set; }
        public string gstin { get; set; }
        public string consent { get; set; }
        public string consent_timestamp { get; set; }

    }



    public class GstVerifyResponse
    {
        public string requestID { get; set; }
        public string status { get; set; }
        public Message message { get; set; }
        public string kyc_id { get; set; }
        public bool success { get; set; }
        public gstdata data { get; set; }
        // public string message { get; set; }
    }
    //public class Message
    //{
    //    public string loan_id { get; set; }
    //}

    public class Contacted
    {
        public object email { get; set; }
        public object mobNum { get; set; }
        public object name { get; set; }
    }

    public class gstdata
    {
        public string requestId { get; set; }
        public Result result { get; set; }
        public int statusCode { get; set; }
    }

    public class Pradr
    {
        public string addr { get; set; }
        public string ntr { get; set; }
        public string adr { get; set; }
        public string em { get; set; }
        public string lastUpdatedDate { get; set; }
        public string mb { get; set; }
    }

    public class Result
    {
        public string stjCd { get; set; }
        public string dty { get; set; }
        public string lgnm { get; set; }
        public string stj { get; set; }
        public List<object> adadr { get; set; }
        public string cxdt { get; set; }
        public List<string> nba { get; set; }
        public string gstin { get; set; }
        public string lstupdt { get; set; }
        public string ctb { get; set; }
        public string rgdt { get; set; }
        public Pradr pradr { get; set; }
        public string ctjCd { get; set; }
        public string tradeNam { get; set; }
        public string sts { get; set; }
        public string ctj { get; set; }
        public List<object> mbr { get; set; }
        public string canFlag { get; set; }
        public string cmpRt { get; set; }
        public Contacted contacted { get; set; }
        public string ppr { get; set; }
    }

    public class GetLeadCurrentActivityDc
    {
        public long ActivityMasterId { get; set; }
        public string ApiName { get; set; }
    }

    public class GetNoActivityDocDc
    {
        public long LeadMasterId { get; set; }
        public string DocumentName { get; set; }
        public bool IsVerified { get; set; }
        public bool IsRejected { get; set; }
    }


    //Risk services Co-Lender Selector API 
    public class CoLenderRequest
    {
        public string first_name { get; set; }
        public string last_name { get; set; }
        public string dob { get; set; }
        public string appl_pan { get; set; }
        public string gender { get; set; }
        public string appl_phone { get; set; }
        public string address { get; set; }
        public string city { get; set; }
        public string state { get; set; }
        public string pincode { get; set; }
        public string enquiry_purpose { get; set; }
        public string bureau_type { get; set; }
        public int tenure { get; set; }
        public string request_id_a_score { get; set; }
        public string request_id_b_score { get; set; }
        public string ceplr_cust_id { get; set; }
        public string interest_rate { get; set; }
        public string product_type_code { get; set; }
        public double sanction_amount { get; set; }
        public int dscr { get; set; }
        public double monthly_income { get; set; }
        public string loan_app_id { get; set; }
        public string consent { get; set; }
        public string consent_timestamp { get; set; }
    }

    public class CoLenderResponseDc
    {
        public string request_id { get; set; }
        public object loan_amount { get; set; }
        public object pricing { get; set; }
        public string co_lender_shortcode { get; set; }
        public string loan_app_id { get; set; }
        public int co_lender_assignment_id { get; set; }
        public string co_lender_full_name { get; set; }
        public string status { get; set; }
        public string message { get; set; }
        public string program_type { get; set; }

    }
    //Request - A Score API
    public class AScoreAPIRequest
    {
        public string first_name { get; set; }
        public string last_name { get; set; }
        public string dob { get; set; }
        public string pan { get; set; }
        public string gender { get; set; }
        public string mobile_number { get; set; }
        public string address { get; set; }
        public string city { get; set; }
        public string pin_code { get; set; }
        public string state_code { get; set; }
        public string enquiry_stage { get; set; }
        public string enquiry_purpose { get; set; }
        public string enquiry_amount { get; set; }
        public string en_acc_account_number_1 { get; set; }
        public string bureau_type { get; set; }
        public int tenure { get; set; } // 36 default
        public string loan_app_id { get; set; }
        public string consent { get; set; }
        public string product_type { get; set; }
        public string consent_timestamp { get; set; }
    }
    public class AScoreAPIResponse
    {
        public string request_id { get; set; }
        public string message { get; set; }
        public int status_code { get; set; }
        public string data { get; set; }
    }
    public class AScoreWebhookResponse
    {
        public string request_id { get; set; }
        public string status { get; set; }
    }
    //adhar api
    public class GetAdharDc
    {
        public string DocumentNumber { get; set; }
        public string FrontFileUrl { get; set; }
        public string BackFileUrl { get; set; }
        public long LeadMasterId { get; set; }
        public int SequenceNo { get; set; }

    }
    public class UpdateAadhaarVerificationRequestDC
    {
        public string otp { get; set; }
        public string requestId { get; set; }
        public string aadhaarNo { get; set; }
        public long LeadMasterId { get; set; }
        public int SequenceNo { get; set; }
    }
    //public class UpdateAdhaarInfoDc
    //{
    //    public long LeadMasterId { get; set; }
    //    public string AadharNo { get; set; }
    //    public string FrontUrl { get; set; }
    //    public string BackUrl { get; set; }
    //    public long DocumentMasterId { get; set; }
    //    public bool Status { get; set; }
    //    public string ErrorMsg { get; set; }
    //    public string OtherInfo { get; set; }
    //    public string Name { get; set; }
    //}
    public class GetLoanApiResponseDc
    {
        public int _id { get; set; }
        public string loan_app_id { get; set; }
        public string loan_id { get; set; }
        public string borrower_id { get; set; }
        public string partner_loan_app_id { get; set; }
        public string partner_loan_id { get; set; }
        public string partner_borrower_id { get; set; }
        public int company_id { get; set; }
        public int product_id { get; set; }
        public string loan_app_date { get; set; }
        public string sanction_amount { get; set; }
        public string net_disbur_amt { get; set; }
        public string status { get; set; }
        public int stage { get; set; }
        public string exclude_interest_till_grace_period { get; set; }
        public string first_name { get; set; }
        public string last_name { get; set; }
        public string bene_bank_name { get; set; }
        public string bene_bank_acc_num { get; set; }
        public string bene_bank_ifsc { get; set; }
        public string bene_bank_account_holder_name { get; set; }
        public DateTime created_at { get; set; }
        public DateTime updated_at { get; set; }
        public int __v { get; set; }
        public DateTime final_approve_date { get; set; }
    }



    //ceplr Pdf Reports

    public class CeplrPdfResponse
    {
        public string customer_id { get; set; }
        public int request_id { get; set; }
        public string token { get; set; }
    }

    public class PdfResDcCeplr
    {
        public int code { get; set; }
        public CeplrPdfResponse data { get; set; }
        public string message { get; set; }
    }
    public class CeplrMessage
    {
        public string status { get; set; }
        public string code { get; set; }
        public string msg { get; set; }
    }

    public class PdfCeplrErrorDc
    {
        public int code { get; set; }
        //public CeplrMessage message { get; set; }
        public string message { get; set; }
    }



    public class CeplrBasicReportResponse
    {
        public int code { get; set; }
        public List<Datum> data { get; set; }
    }

    public class Analytics
    {
        public int transaction_count { get; set; }
        public string start_date { get; set; }
        public string end_date { get; set; }
        public string avg_daily_closing_balance { get; set; }
        public string avg_monthly_debits { get; set; }
        public string avg_monthly_credits { get; set; }
        public CreditOverallSummary credit_overall_summary { get; set; }
        public OutwardBouncesSummary outward_bounces_summary { get; set; }
        public SalarySummary salary_summary { get; set; }
        public BalanceSummary balance_summary { get; set; }
        public CashWithdrawalsSummary cash_withdrawals_summary { get; set; }
        public CashDepositSummary cash_deposit_summary { get; set; }
        public DebitsSummary debits_summary { get; set; }
        public RegularDebitsSummary regular_debits_summary { get; set; }
        public DebitToCreditRatio debit_to_credit_ratio { get; set; }
        public AvgMonthlySurplus avg_monthly_surplus { get; set; }
        public CountOfNegativeIncedent count_of_negative_incedent { get; set; }
    }

    public class AvgMonthlySurplus
    {
        public string total_surplus { get; set; }
        public List<MonthlyAnalysis> monthly_analysis { get; set; }
    }

    public class BalanceSummary
    {
        public string average_balance { get; set; }
        public string last_balance { get; set; }
        public string max_balance { get; set; }
        public string min_balance { get; set; }
        public string opening_balance { get; set; }
        public string average_balance_at_1st { get; set; }
        public string average_balance_at_5th { get; set; }
        public string average_balance_at_15th { get; set; }
        public string average_balance_at_25th { get; set; }
        public string average_balance_at_30th { get; set; }
        public List<MonthlyAnalysis> monthly_analysis { get; set; }
    }

    public class CashDepositSummary
    {
        public int deposit_count { get; set; }
        public string total_deposit { get; set; }
        public List<MonthlyAnalysis> monthly_analysis { get; set; }
    }

    public class CashWithdrawalsSummary
    {
        public int cash_withdrawals_count { get; set; }
        public string total_cash_withdrawal { get; set; }
        public List<MonthlyAnalysis> monthly_analysis { get; set; }
    }

    public class CountOfNegativeIncedent
    {
        public int total_negative_incedent_count { get; set; }
        public List<MonthlyAnalysis> monthly_analysis { get; set; }
    }

    public class CreditOverallSummary
    {
        public int cash_credits_count { get; set; }
        public string credit_median { get; set; }
        public string total_credits { get; set; }
        public List<MonthlyAnalysis> monthly_analysis { get; set; }
    }

    public class Datum
    {
        public string name { get; set; }
        public string mobile { get; set; }
        public string email { get; set; }
        public object dob { get; set; }
        public object address { get; set; }
        public object pan { get; set; }
        public string masked_account_number { get; set; }
        public object account_type { get; set; }
        public object holding_type { get; set; }
        public string current_balance { get; set; }
        public Analytics analytics { get; set; }
    }

    public class DebitsSummary
    {
        public int total_debits_count { get; set; }
        public string total_debits { get; set; }
        public List<MonthlyAnalysis> monthly_analysis { get; set; }
    }

    public class DebitToCreditRatio
    {
        public string total_debit_to_credit_ratio { get; set; }
        public List<MonthlyAnalysis> monthly_analysis { get; set; }
    }

    public class MonthlyAnalysis
    {
        public int cash_credit_counts { get; set; }
        public object total_credit { get; set; }
        public object monthly_credit_median { get; set; }
        public int total_negative_incedent_count { get; set; }
        public int outward_bounces_count { get; set; }
        public int outward_bounce_amount { get; set; }
        public object month_average { get; set; }
        public object balance_on_1st { get; set; }
        public object balance_on_5th { get; set; }
        public object balance_on_15th { get; set; }
        public object balance_on_25th { get; set; }
        public object balance_on_30th { get; set; }
        public int cash_withdrawals_count { get; set; }
        public object total_cash_withdrawal { get; set; }
        public int deposit_count { get; set; }
        public string total_deposit { get; set; }
        public int total_debits_count { get; set; }
        public object total_debits { get; set; }
        public int regular_debits_count { get; set; }
        public object total_regular_debits { get; set; }
        public string total_debit_to_credit_ratio { get; set; }
        public string total_surplus { get; set; }
    }

    public class OutwardBouncesSummary
    {
        public int outward_bounces_count { get; set; }
        public int outward_bounce_amount { get; set; }
        public List<MonthlyAnalysis> monthly_analysis { get; set; }
    }

    public class RegularDebitsSummary
    {
        public int regular_debits_count { get; set; }
        public string total_regular_debits { get; set; }
        public List<MonthlyAnalysis> monthly_analysis { get; set; }
    }



    public class SalarySummary
    {
        public int salary_flag { get; set; }
        public List<object> salary_dates { get; set; }
        public string stable_monthly_inflow { get; set; }
        public string total_salary { get; set; }
    }


    //Retry Api Dc
    public class RetryApiDc
    {
        public long LeadMasterId { get; set; }
        //public string CustomerUid { get; set; }
        public string start_date { get; set; }
        public string end_date { get; set; }
        public string ApiName { get; set; }
    }


    public class LeadDocumentDc
    {
        public long id { get; set; }
        public string partner_borrower_id { get; set; }
        public string partner_loan_app_id { get; set; }
        public string loan_app_id { get; set; }
        public string borrower_id { get; set; }
        public string DocumentName { get; set; }
        public string FrontFileUrl { get; set; }
        public string OtherInfo { get; set; }
        public string DocumentTypeCode { get; set; }
        public long LeadmasterID { get; set; }
    }


    //PostLoanShowOnPageDc
    public class GenerateLoanDc
    {
        public string a_score_request_id { get; set; }
        public int co_lender_assignment_id { get; set; }
        public string partner_loan_app_id { get; set; }
        public string partner_borrower_id { get; set; }
        public string loan_app_id { get; set; }
        public string borrower_id { get; set; }
        public string partner_loan_id { get; set; }
        public string marital_status { get; set; }
        public string residence_vintage { get; set; }
        public string loan_app_date { get; set; }
        public string loan_amount_requested { get; set; }
        public string sanction_amount { get; set; }
        public string processing_fees_perc { get; set; }
        public string processing_fees_amt { get; set; }
        public string gst_on_pf_perc { get; set; }
        public string gst_on_pf_amt { get; set; }
        public string conv_fees { get; set; }
        public string insurance_amount { get; set; }
        public string net_disbur_amt { get; set; }
        public string int_type { get; set; }
        public string loan_int_rate { get; set; }
        public string loan_int_amt { get; set; }
        public string broken_period_int_amt { get; set; }
        public string repayment_type { get; set; }
        public string tenure_type { get; set; }
        public string tenure { get; set; }
        public string first_inst_date { get; set; }
        public string emi_amount { get; set; }
        public string emi_count { get; set; }
        public string final_approve_date { get; set; }
        public string final_remarks { get; set; }
        public string borro_bank_name { get; set; }
        public string borro_bank_acc_num { get; set; }
        public string borro_bank_ifsc { get; set; }
        public string borro_bank_account_holder_name { get; set; }
        public string borro_bank_account_type { get; set; }
        public string bene_bank_name { get; set; }
        public string bene_bank_acc_num { get; set; }
        public string bene_bank_ifsc { get; set; }
        public string bene_bank_account_holder_name { get; set; }
        public string bene_bank_account_type { get; set; }
        public string avg_banking_turnover_6_months { get; set; }
        public string abb { get; set; }
        public string monthly_income { get; set; }
        public string itr_ack_no { get; set; }
        public string bureau_type { get; set; }
        public string bureau_score { get; set; }
        public string customer_type_ntc { get; set; }
        public string foir { get; set; }
        public string bureau_fetch_date { get; set; }
        public string bounces_in_three_month { get; set; }
        public string business_name { get; set; }
        public string business_address { get; set; }
        public string business_city { get; set; }
        public string business_state { get; set; }
        public string business_pin_code { get; set; }
        public string business_address_ownership { get; set; }
        public string program_type { get; set; }
        public string business_entity_type { get; set; }
        public string business_pan { get; set; }
        public string gst_number { get; set; }
        public string udyam_reg_no { get; set; }
        public string other_business_reg_no { get; set; }
        public string business_vintage_overall { get; set; }
        public string txn_avg { get; set; }
        public string txn_1 { get; set; }
        public string txn_2 { get; set; }
        public string txn_3 { get; set; }
        public string txn_4 { get; set; }
        public string txn_5 { get; set; }
        public string txn_6 { get; set; }
        public string business_establishment_proof_type { get; set; }
        public string co_app_or_guar_name { get; set; }
        public string co_app_or_guar_dob { get; set; }
        public string co_app_or_guar_gender { get; set; }
        public string co_app_or_guar_address { get; set; }
        public string co_app_or_guar_mobile_no { get; set; }
        public string co_app_or_guar_pan { get; set; }
        public string relation_with_applicant { get; set; }
        public string co_app_or_guar_bureau_type { get; set; }
        public string co_app_or_guar_bureau_score { get; set; }
        public string co_app_or_guar_ntc { get; set; }
        public string insurance_company { get; set; }
        public string purpose_of_loan { get; set; }
        public string emi_obligation { get; set; }

    }

    public class GetLoanApiResponseDC
    {
        public bool success { get; set; }
        public string message { get; set; }
        public List<ResponseLoanData> data { get; set; }
    }
    public class ResponseLoanData
    {
        public int? co_lender_assignment_id { get; set; }
        public string loan_app_id { get; set; }
        public string loan_id { get; set; }
        public string borrower_id { get; set; }
        public string partner_loan_app_id { get; set; }
        public string partner_loan_id { get; set; }
        public string partner_borrower_id { get; set; }
        public int? company_id { get; set; }
        public int? co_lender_id { get; set; }
        public string co_lend_flag { get; set; }
        public int? product_id { get; set; }
        public string itr_ack_no { get; set; }
        public string loan_app_date { get; set; }
        public int? penal_interest { get; set; }
        public int? bounce_charges { get; set; }
        public string marital_status { get; set; }
        public string sanction_amount { get; set; }
        public string gst_on_pf_amt { get; set; }
        public string gst_on_pf_perc { get; set; }
        public string repayment_type { get; set; }
        public DateTime? first_inst_date { get; set; }
        public string net_disbur_amt { get; set; }
        public DateTime? final_approve_date { get; set; }
        public string final_remarks { get; set; }
        public string foir { get; set; }
        public string status { get; set; }
        public int? stage { get; set; }
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
        public int? emi_count { get; set; }
        public string broken_interest { get; set; }
        public int? dpd_in_last_12_months { get; set; }
        public int? dpd_in_last_3_months_credit_card { get; set; }
        public int? dpd_in_last_3_months_unsecured { get; set; }
        public double? broken_period_int_amt { get; set; }
        public int? dpd_in_last_24_months { get; set; }
        public int? enquiries_bureau_30_days { get; set; }
        public int? cnt_active_unsecured_loans { get; set; }
        public int? total_overdues_in_cc { get; set; }
        public int? insurance_amount { get; set; }
        public int? bureau_outstanding_loan_amt { get; set; }
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
        public int? enquiries_in_last_3_months { get; set; }
        public double? gst_on_conv_fees { get; set; }   //05/03/2024
        public double? cgst_on_conv_fees { get; set; }//05/03/2024
        public double? sgst_on_conv_fees { get; set; }//05/03/2024
        public double? igst_on_conv_fees { get; set; }//05/03/2024
        public string gst_on_application_fees { get; set; }
        public string cgst_on_application_fees { get; set; }
        public string sgst_on_application_fees { get; set; }
        public string igst_on_application_fees { get; set; }
        public string interest_type { get; set; }
        public double? conv_fees_excluding_gst { get; set; }//05/03/2024
        public string application_fees_excluding_gst { get; set; }
        public string emi_obligation { get; set; }
        public string a_score_request_id { get; set; }
        public int? a_score { get; set; }
        public int? b_score { get; set; }
        public int? offered_amount { get; set; }
        public double? offered_int_rate { get; set; }
        public double? monthly_average_balance { get; set; }
        public double? monthly_imputed_income { get; set; }
        public string party_type { get; set; }
        public string co_app_or_guar_dob { get; set; }
        public string co_app_or_guar_gender { get; set; }
        public string co_app_or_guar_ntc { get; set; }
        public string residence_vintage { get; set; }
        public string business_entity_type { get; set; }
        public string udyam_reg_no { get; set; }
        public string program_type { get; set; }
        public int? borrower_premium { get; set; }
        public int? coborrower_premium { get; set; }
        public int? written_off_settled { get; set; }
        public string upi_handle { get; set; }
        public string upi_reference { get; set; }
        public int? fc_offer_days { get; set; }
        public string foreclosure_charge { get; set; }
        public DateTime? created_at { get; set; }
        public DateTime? updated_at { get; set; }
        public int? _id { get; set; }
        public int? __v { get; set; }
    }
    #endregion



    #region Loan Harry



    //post
    public class Postrepayment_scheduleDc
    {
        public string loan_id { get; set; }
        public long company_id { get; set; }
        public string product_id { get; set; }
    }


    //response
    public class repay_scheduleDc
    {
        public bool error { get; set; }
        public bool success { get; set; }
        public repay_scheduleData data { get; set; }
        public string message { get; set; }


    }
    public class repay_scheduleData
    {
        public List<repay_scheduleDetails> rows { get; set; }
        public int count { get; set; }
    }


    public class repay_scheduleDetails
    {
        public int _id { get; set; }
        public int repay_schedule_id { get; set; }
        public int company_id { get; set; }
        public int product_id { get; set; }
        public string loan_id { get; set; }
        public int emi_no { get; set; }
        public DateTime? due_date { get; set; }
        public double emi_amount { get; set; }
        public double prin { get; set; }
        public double int_amount { get; set; }
        public int __v { get; set; }
        public double principal_bal { get; set; }
        public double principal_outstanding { get; set; }
        public DateTime created_at { get; set; }
        public DateTime updated_at { get; set; }


    }


    //////// Loan Status API     
    //req
    public class LoanStatusChangeAPIReq
    {
        public string loan_app_id { get; set; }
        public string loan_id { get; set; }
        public string borrower_id { get; set; }
        public string partner_loan_app_id { get; set; }
        public string partner_loan_id { get; set; }
        public string partner_borrower_id { get; set; }
        public string status { get; set; }
    }

    //reponse
    public class LoanStatusChangeAPIRes
    {
        public string message { get; set; }
        public UpdateRespData updateResp { get; set; }
    }

    public class UpdateRespData
    {
        public int _id { get; set; }
        public string loan_app_id { get; set; }
        public string loan_id { get; set; }
        public string borrower_id { get; set; }
        public string partner_loan_app_id { get; set; }
        public string partner_loan_id { get; set; }
        public string partner_borrower_id { get; set; }
        public int company_id { get; set; }
        public int product_id { get; set; }
        public string loan_app_date { get; set; }
        public string sanction_amount { get; set; }
        public string gst_on_pf_amt { get; set; }
        public string gst_on_pf_perc { get; set; }
        public string repayment_type { get; set; }
        public string net_disbur_amt { get; set; }
        public string status { get; set; }
        public int stage { get; set; }
        public string exclude_interest_till_grace_period { get; set; }
        public string partner_customer_catagory { get; set; }
        public string loan_int_rate { get; set; }
        public string processing_fees_amt { get; set; }
        public string processing_fees_perc { get; set; }
        public string tenure { get; set; }
        public string tenure_type { get; set; }
        public string int_type { get; set; }
        public string first_name { get; set; }
        public string last_name { get; set; }
        public int disc_factor_merchant_risk_cat { get; set; }
        public int disc_factor_bureau_score { get; set; }
        public int current_overdue_value { get; set; }
        public string dpd_in_last_6_months { get; set; }
        public string bureau_score { get; set; }
        public string bureau_type { get; set; }
        public string loan_amount_requested { get; set; }
        public string bene_bank_name { get; set; }
        public string bene_bank_acc_num { get; set; }
        public string bene_bank_ifsc { get; set; }
        public string bene_bank_account_holder_name { get; set; }
        public string bene_bank_account_type { get; set; }
        public DateTime created_at { get; set; }
        public DateTime updated_at { get; set; }
        public int __v { get; set; }
        public DateTime final_approve_date { get; set; }
    }


    public class LeadLoanDataDc
    {
        public string UrlSlaDocument { get; set; }
        public string UrlSlaUploadSignedDocument { get; set; }
        public string UrlSlaUploadDocument_id { get; set; }
        public double? loan_amount { get; set; }
        public double SanctionAmount { get; set; }
        public string LoanStatus { get; set; }
        public double? pricing { get; set; }
        public int? tenure { get; set; }
        public bool? IsUpload { get; set; }
        public string loan_int_amt { get; set; }
        public string loan_int_rate { get; set; }
        public string emi_amount { get; set; }
        public int? emi_count { get; set; }
        public string UMRN { get; set; }
        public int? Loan_insurance_amount { get; set; }


    }





    #endregion




}
