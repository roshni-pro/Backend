using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.Arthmate
{
    public class LoanDocumentDc
    {
        public string loan_app_id { get; set; }
        public string borrower_id { get; set; }
        public string partner_loan_app_id { get; set; }
        public string partner_borrower_id { get; set; }
        // public string file_url { get; set; }
        public string code { get; set; }
        public string base64pdfencodedfile { get; set; }
    }

    public class LoanDocumentBankDc
    {
        public string loan_app_id { get; set; }
        public string borrower_id { get; set; }
        public string partner_loan_app_id { get; set; }
        public string partner_borrower_id { get; set; }
        // public string file_url { get; set; }
        public string code { get; set; }
        public string base64pdfencodedfile { get; set; }
        public string doc_key { get; set; } //doc_key==pdfPassword \
        public string fileType { get; set; } //==bank_stmnts \

    }


    public class LoanDocumentResponseDc
    {
        public bool success { get; set; }
        public string message { get; set; }
        public UploadDocumentData uploadDocumentData { get; set; }
        public long RequestId { get; set; }
        public long ReponseId { get; set; }

    }
    public class UploadDocumentData
    {
        public int document_id { get; set; }
        public string message { get; set; }
    }
    public class LoanDocumentError
    {
        public bool success { get; set; }
        public string message { get; set; }
    }
}
