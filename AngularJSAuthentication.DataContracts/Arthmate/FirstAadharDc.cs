using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.Arthmate
{
    public class FirstAadharDc
    {

        #region Step 1 Aadhar post and response
        public class FirstAadharXMLPost
        {
            public long LeadId { get; set; }
            public string aadhaar_no { get; set; }
            public string loan_app_id { get; set; }
            public string consent { get; set; }
            public string consent_timestamp { get; set; }
        }
        public class FirstAadharXML_Post
        {
            public string aadhaar_no { get; set; }
            public string loan_app_id { get; set; }
            public string consent { get; set; }
            public string consent_timestamp { get; set; }
        }
        public class Data
        {
            public string requestId { get; set; }
            public Result result { get; set; }
            public int statusCode { get; set; }
        }
        public class Result
        {
            public string message { get; set; }
        }
        public class FirstAadharXMLResponseDc
        {
            public string kyc_id { get; set; }
            public Data data { get; set; }
            public string message { get; set; }
            public string request_id { get; set; }
            public string requestId { get; set; }
            public string status { get; set; }
        }


        public class AadharOtpDataRes
        {
            public string requestId { get; set; }
            public AadharOtpResult result { get; set; }
            public int statusCode { get; set; }
        }

        public class AadharOtpResult
        {
            public string message { get; set; }
        }

        public class AadharOtpGenerateRes
        {
            public string kyc_id { get; set; }
            public AadharOtpDataRes data { get; set; }
            public bool success { get; set; } //
            public string message { get; set; }
        }




        #endregion
    }
}
