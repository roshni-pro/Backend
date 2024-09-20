using System.Collections.Generic;

namespace AngularJSAuthentication.API.Helper.IRN
{
    public class ResponseIRNDC
    {
        public object custom_fields { get; set; }
        public string document_status { get; set; }
        public object errors { get; set; }
        public GovtResponse govt_response { get; set; }
        public object group_id { get; set; }
        public string gstin { get; set; }
        public string owner_id { get; set; }
        public Transaction transaction { get; set; }
        public string transaction_id { get; set; }
        public object transaction_metadata { get; set; }       
        public bool deleted { get; set; }       
        public object error_response { get; set; }        
          
        public bool is_deleted { get; set; }      
        public string tag_identifier { get; set; }
      
       
    }
    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse); 
    public class GovtResponse
    {
        public string Success { get; set; }
        public long AckNo { get; set; }
        public string AckDt { get; set; }
        public string Irn { get; set; }
        public string SignedInvoice { get; set; }
        public string SignedQRCode { get; set; }
        public string Status { get; set; }
        public long EwbNo { get; set; }
        public string EwbDt { get; set; }
        public string EwbValidTill { get; set; }
        public List<ErrorDetail> ErrorDetails { get; set; }
    }

}