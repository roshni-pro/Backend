using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.Arthmate
{

    public class ValidAuthenticationPanResDc
    {
        [JsonProperty("result")]
        public result person { get; set; }
        public string request_id { get; set; }
        [JsonProperty("status-code")]
        public int StatusCode { get; set; }
        public string error { get; set; }
    }
    public class ValidAuthenticationPanPost
    {
        public string consent { get; set; }
        public string pan { get; set; }
    }

    //New Pan Authentication Json 22-03-2024
    //start kk
    public class KarzaAuthenticationPanV2
    {
        public string pan { get; set; }
        public string name { get; set; }
        public string dob { get; set; }
        public string consent { get; set; }
        public ClientData clientData { get; set; }
    }
    public class ClientData
    {
        public string caseId { get; set; }
    }

    //ResponseKarzaNewV2
   

    public class ResponseKarzaNewV2
    {
        public Resultss result { get; set; }
        public string request_id { get; set; }

        [JsonProperty("status-code")]
        public string statuscode { get; set; }
        public ClientData clientData { get; set; }
    }
    public class Resultss
    {
        public string status { get; set; }
        public object duplicate { get; set; }
        public bool nameMatch { get; set; }
        public bool dobMatch { get; set; }
    }

    //end kk

    public class result
    {
        public string name { get; set; }
    }
    public class OcrPostDc
    {
        public string url { get; set; }
        public string maskAadhaar { get; set; }
        public string hideAadhaar { get; set; }
        public bool conf { get; set; }
        public string docType { set; get; }
    }
    public class PanOcrOtherInfoDc
    {
        public int? age { get; set; }
        public string date_of_birth { get; set; }
        public string date_of_issue { get; set; }
        public string fathers_name { get; set; }
        public string id_number { get; set; }
        public bool id_scanned { get; set; }
        public bool minor { get; set; }
        public string name_on_card { get; set; }
        public string pan_type { get; set; }
    }
    public class PanOcrResDc
    {
        public string requestId { get; set; }
        public List<Result2> result { get; set; }
        public int statusCode { get; set; }
        public string error { get; set; }
        public PanOcrOtherInfoDc OtherInfo { get; set; }
    }
    public class Result2
    {
        public Details details { get; set; }
        public string type { get; set; }
    }
    public class Details
    {
        public Date date { get; set; }
        public DateOfIssue dateOfIssue { get; set; }
        public Father father { get; set; }
        public Name name { get; set; }
        public PanNo panNo { get; set; }
    }
    public class Date
    {
        public string value { get; set; }
    }

    public class DateOfIssue
    {
        public string value { get; set; }
    }

    public class Father
    {
        public string value { get; set; }
    }

    public class Name
    {
        public string value { get; set; }
    }

    public class PanNo
    {
        public string value { get; set; }
    }

    public class LeadActivityHistoriesDc
    {
        public long LeadMasteId { get; set; }
        public string Activity { get; set; }
        public long? LeadLoanDocumentId { get; set; }
        public bool? ApprovalStatus { get; set; }
        public int Sequence { get; set; }
        public string Message { get; set; }
    }
   
}
