using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.Arthmate
{
    public class CompositeDisbursementDc
    {
        public string loan_id { get; set; }
        public string loan_app_id { get; set; }
        public string borrower_id { get; set; }
        public string partner_loan_id { get; set; }
        public string partner_borrower_id { get; set; }
        public string borrower_mobile { get; set; }
        public string ifsc_code { get; set; }
        public long account_no { get; set; }
        public string txn_date { get; set; }
        public double sanction_amount { get; set; }
        public double net_disbur_amt { get; set; }
    }

    public class Response
    {
        public bool success { get; set; }
        public string message { get; set; }
    }

    public class CompositeDisbursementResponse
    {
        public string loan_id { get; set; }
        public string partner_loan_id { get; set; }
        public Response response { get; set; }
    }

    public class LeadLoanDetailDc
    {
        public long LeadMasterId { get; set; }
        public string loan_id { get; set; }
        public string loan_app_id { get; set; }
        public string borrower_id { get; set; }
        public string partner_loan_id { get; set; }
        public string partner_borrower_id { get; set; }
        public string borrower_mobile { get; set; }
        public string ifsc_code { get; set; }
        public string account_no { get; set; }
        public DateTime txn_date { get; set; }
        public string sanction_amount { get; set; }
        public string net_disbur_amt { get; set; }
    }


    public class DisbursementData
    {
        public string loan_id { get; set; }
        public string partner_loan_id { get; set; }
        public string status_code { get; set; }
        public double net_disbur_amt { get; set; }
        public string utr_number { get; set; }
        public string utr_date_time { get; set; }
    }

    public class DisbursementDataDc
    {
        public bool success { get; set; }
        public DisbursementData data { get; set; }
        public string message { get; set; }
    }

}
