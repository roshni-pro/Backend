using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.Arthmate
{
    public class AdharDigiLocker
    {
        public class eAadhaarDigilockerResponseDc
        {
            public string requestId { get; set; }
            public res_msg result { get; set; }
            public int? statusCode { get; set; }
            public ErrorResponse error { get; set; }
            public string personId { get; set; }
        }
        public class ErrorResponse
        {
            public Error error { get; set; }
        }
        public class res_msg
        {
            public string message { get; set; }
        }
        public class Error
        {
            public string requestId { get; set; }
            public string status { get; set; }
            public string message { get; set; }
            public string error { get; set; }
        }

        public class eAadhaarDigilockerRequestDc
        {

            public string consent { get; set; }
            public string aadhaarNo { get; set; }

        }

        public class eAdhaarDigilockerVerifyOTPResponseDcXml
        {
            public string requestId { get; set; }
            public res_message result { get; set; }
            public int? statusCode { get; set; }
            public ErrorResponse error { get; set; }
        }

        public class res_message
        {
            public DataFromAdhaar dataFromAadhaar { get; set; }
            public string message { get; set; }
            public string shareCode { get; set; }
        }
        public class DataFromAdhaar
        {
            public string generatedDateTime { get; set; }
            public string maskedAadhaarNumber { get; set; }
            public string name { get; set; }
            public string dob { get; set; }
            public string gender { get; set; }
            public string mobileHash { get; set; }
            public string emailHash { get; set; }
            public string fatherName { get; set; }
            public Addressess address { get; set; }
            public string image { get; set; }
            public string maskedVID { get; set; }
            public string file { get; set; }

        }
        public class Addressess
        {
            public SplitAddressess splitAddress { get; set; }
            public string combinedAddress { get; set; }
        }
        public class SplitAddressess
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
    }
}
