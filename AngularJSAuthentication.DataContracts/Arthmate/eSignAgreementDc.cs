using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.Arthmate
{
    public class AgreementDetailDc
    {
        public long LeadDocumentid { get; set; }
        public string name { get; set; }
        public string email { get; set; }
        public string workflowId { get; set; }
        public int yob { get; set; }
        public string gender { get; set; }
        public string mobileNo { get; set; }
        public string document { get; set; }
        public string FrontFileUrl { get; set; }
        public string Agreement { get; set; }
        public long LeadMasteId { get; set; }
        public bool IsVerified { get; set; }
    }
    public class LeadeSignAgreementDc
    {
        public long LeadMasterId { get; set; }
        public string PathUrl { get; set; }
        public string documentId { get; set; }
    }

}
