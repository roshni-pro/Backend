using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AngularJSAuthentication.API.Helper.IRN
{
    public class ErrorResponseIRNDC
    {
        public object custom_fields { get; set; }
        public bool deleted { get; set; }
        public string document_status { get; set; }
        public object error_response { get; set; }
        public object errors { get; set; }
        public GovtResponse govt_response { get; set; }
        public object group_id { get; set; }
        public string gstin { get; set; }
        public bool is_deleted { get; set; }
        public object owner_id { get; set; }
        public string tag_identifier { get; set; }
        public Transaction transaction { get; set; }
        public string transaction_id { get; set; }
        public object transaction_metadata { get; set; }
    }
    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse); 
    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse); 
    public class ErrorDetail
    {
        public string error_code { get; set; }
        public string error_message { get; set; }
        public string error_id { get; set; }
        public string error_source { get; set; }
    }

   

     

     
       
     






}