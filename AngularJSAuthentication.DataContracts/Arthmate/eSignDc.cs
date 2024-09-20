using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.Arthmate
{
    public class eSignDc
    {
    }

    public class eSignSessionResponse
    {
        public string requestId { get; set; }
        public Result result { get; set; }
        public int statusCode { get; set; }
        public string errorString { get; set; }
    }
    public class eSignSessionRequest
    {
        public string name { get; set; }
        public string email { get; set; }
        public string workflowId { get; set; }
        public string yob { get; set; }
        public string gender { get; set; }
        public string mobileNo { get; set; }
        public List<AdditionalSigner> additionalSigners { get; set; }
        public string document { get; set; }
    }
    public class AdditionalSigner
    {
        public string name { get; set; }
        public string email { get; set; }
        public string yob { get; set; }
        public string gender { get; set; }
        public string mobileNo { get; set; }
    }
    public class eSignAgreementDc
    {
        public long LeadMasterId { get; set; }
        public string name { get; set; }
        public string email { get; set; }
        public string workflowId { get; set; }
        public string yob { get; set; }
        public string gender { get; set; }
        public string mobileNo { get; set; }
        public string document { get; set; }
        public string PathUrl { get; set; }
        public string Agreement { get; set; }
        public bool IsVerified { get; set; }
    }

    public class eSignResult
    {
        public string documentId { get; set; }
        public List<SigningDetail> signingDetails { get; set; }
    }

    public class eSignDocResponseDc
    {
        public string requestId { get; set; }
        public eSignResult result { get; set; }
        public int statusCode { get; set; }
    }

    public class SigningDetail
    {
        public string name { get; set; }
        public string signUrl { get; set; }
        public DateTime expiryDate { get; set; }
    }


}
