using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.Arthmate
{
    #region Ceplr Api
    public class RequestConsentDc
    {
        public string customer_name { get; set; }
        public string customer_contact { get; set; }
        public string customer_email { get; set; }
        public string configuration_uuid { get; set; }
        public string redirect_url { get; set; }
        public string callback_url { get; set; }
        public string fip_id { get; set; }
    }
  
    public class RequestConsentResponse
    {
        public string url { get; set; }
        public string link_uuid { get; set; }
    }

    public class RequestConsentResponseDc
    {
        public int code { get; set; }
        public RequestConsentResponse data { get; set; }
    }
    public class CeplrFIStatusResponseDc
    {
        public int code { get; set; }
        public string message { get; set; }
        public string status { get; set; }
    }

    public class CeplrBank
    {
        public string aa_fip_id { get; set; }
        public int? pdf_fip_id { get; set; }
        public string fip_name { get; set; }
        public string enable { get; set; }
        public string fip_logo_uri { get; set; }
    }

    public class CeplrBankListdc
    {
        public int code { get; set; }
        public List<CeplrBank> data { get; set; }
    }
    public class CeplrPdfReportsResponse
    {
        public string customer_id { get; set; }
        public int request_id { get; set; }
    }

    public class CeplrPdfReportsResponseDc
    {
        public int code { get; set; }
        public CeplrPdfReportsResponse data { get; set; }
    }

    #endregion
}
