using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.Arthmate
{
   public class SecondAadharDc
    {
        #region Step 2 Aadhar post and response
        public class SecondAadharXMLPost
        {
            public long LeadId { get; set; }
            public string request_id { get; set; }
            public string aadhaar_no { get; set; }
            public int otp { get; set; }
            public string loan_app_id { get; set; }
            public string consent { get; set; }
            public string consent_timestamp { get; set; }
        }
        public class SecondAadharXMLPostDc
        {
            public string request_id { get; set; }
            public string aadhaar_no { get; set; }
            public int otp { get; set; }
            public string loan_app_id { get; set; }
            public string consent { get; set; }
            public string consent_timestamp { get; set; }
        }
        public class SecondAadharXMLDc
        {
            public long LeadMasterId { get; set; }
            public string request_id { get; set; }
            public int otp { get; set; }
            public int SequenceNo { get; set; }
            public double loan_amt { get; set; }
            public bool insurance_applied { get; set; }
        }
        public class Address
        {
            public SplitAddress splitAddress { get; set; }
            public string combinedAddress { get; set; }
        }

        public class Data
        {
            public string requestId { get; set; }
            public Result result { get; set; }
            public int statusCode { get; set; }
        }

        public class DataFromAadhaar
        {
            public string generatedDateTime { get; set; }
            public string maskedAadhaarNumber { get; set; }
            public string name { get; set; }
            public string dob { get; set; }
            public string gender { get; set; }
            public string mobileHash { get; set; }
            public string emailHash { get; set; }
            public string relativeName { get; set; }
            public Address address { get; set; }
            public string image { get; set; }
            public string maskedVID { get; set; }
            public string file { get; set; }
        }
        public class Result
        {
            public DataFromAadhaar dataFromAadhaar { get; set; }
            public string message { get; set; }
            public string shareCode { get; set; }
        }
        public class SecondAadharXMLResponseDc
        {
            public string kyc_id { get; set; }
            public Data data { get; set; }
            public bool success { get; set; }
            public string message { get; set; }
            public string KYCResponse { get; set; }
        }
        public class SplitAddress
        {
            public string houseNumber { get; set; }
            public string street { get; set; }
            public string landmark { get; set; }
            public string subdistrict { get; set; }
            public string district { get; set; }
            public string vtcName { get; set; }
            public string location { get; set; }
            public string postOffice { get; set; }
            public string state { get; set; }
            public string country { get; set; }
            public string pincode { get; set; }
        }
        #endregion
    }
}
