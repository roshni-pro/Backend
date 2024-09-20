using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.Arthmate
{
 
    public class CallBackData
    {
        public string status_code { get; set; }
        public string loan_id { get; set; }
        public string partner_loan_id { get; set; }
        public double net_disbur_amt { get; set; }
        public string utr_number { get; set; }
        public string utr_date_time { get; set; }
        public string txn_id { get; set; }


    }


    public class CompositeDisbursementWebhookDc
    {
        public string event_key { get; set; }
        [JsonProperty("data")]
        public CallBackData CallBackData { get; set; }
    }
}
