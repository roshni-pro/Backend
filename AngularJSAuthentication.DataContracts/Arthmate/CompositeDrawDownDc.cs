using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.Arthmate
{
   public class CompositeDrawDownDc
    {
        public string loan_app_id { get; set; }
        public string loan_id { get; set; }
        public string borrower_id { get; set; }
        public string partner_loan_app_id { get; set; }
        public string partner_loan_id { get; set; }
        public string partner_borrower_id { get; set; }
        public string borrower_mobile { get; set; }
        public string drawadown_request_date { get; set; }
        public int drawdown_amount { get; set; }
        public int no_of_emi { get; set; }
        public int net_drawdown_amount { get; set; }
        public int usage_fees { get; set; }
        public int usage_fees_including_gst { get; set; }
    }

    public class CompositeData
    {
        public string reference_id { get; set; }
        public DateTime created_date { get; set; }
        public string remarks { get; set; }
    }

    public class CompositeResponse
    {
        public string txn_id { get; set; }
        public int acknowledge { get; set; }
        public CompositeData data { get; set; }
    }

    public class CompositeDrawDownResponseDc
    {
        public string loan_id { get; set; }
        public string partner_loan_id { get; set; }
        public string due_date { get; set; }
        public string int_value { get; set; }
        public int principal_amount { get; set; }
        public int usage_id { get; set; }
        public CompositeResponse response { get; set; }
        public DrawDownErrorRes ErrorRes { get; set; }
    }
    public class DrawDownErrorRes
    {
        public bool success { get; set; }
        public string message { get; set; }
    }
}
