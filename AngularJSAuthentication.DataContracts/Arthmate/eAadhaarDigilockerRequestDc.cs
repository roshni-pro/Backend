using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.Arthmate
{
    public class eAadhaarDigilockerRequestDc
    {

        public string consent { get; set; }
        public string aadhaarNo { get; set; }

    }

    //------S----1----------------------
    public class eAadhaarDigilockerOTPDC
    {

    }
    //------E----1----------------------


    //------S----2----------------------
    public class eAadhaarDigilockerRequesTVerifyOTPDC
    {
        public string otp { get; set; }
        public string aadhaarNo { get; set; }
        public string requestId { get; set; }
        public string consent { get; set; }
    }

    public class eAadhaarDigilockerRequesTVerifyOTPDCXml
    {
        public string otp { get; set; }
        public string requestId { get; set; }
        public string consent { get; set; }
        public string aadhaarNo { get; set; }
    }
    //------E----2----------------------

    //------S----3----------------------
    public class eAadhaarDigilockerRequestDownloadDC
    {
        //        public string aadhaarNo { get; set; }
        public string requestId { get; set; }
        public string consent { get; set; }
    }
    //------E----3----------------------


    public class ApiRequestResponseDC
    {
        public string ApiUrl { get; set; }
        public string ApiSecret { get; set; }
        public string ApiKey { get; set; }
        public string Other { get; set; }
        public string Header { get; set; }
        public long ApiMasterId { get; set; }
    }

    public class AadharRes
    {
        public string id_number { get; set; }
        public string name { get; set; }
        public string dob { get; set; }
        public string gender { get; set; }
        public string address { get; set; }
        public string pincode { get; set; }
        public string base_64_face { get; set; }
    }
}
