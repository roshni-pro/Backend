using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.RazorPay
{
    public class RazorpayPaymentResponse
    {
        public string razorpay_order_id { get; set; }
        public string razorpay_payment_id { get; set; }
        public string razorpay_signature { get; set; }
    }

    public class RazorPayTransactionDC
    {
        public int OrderId { get; set; }
        public double Amount { get; set; }
        public RazorpayPaymentResponse response { get; set; }        

    }
    public class PaymentRefundDc
    {
        public int amount { get; set; }
        public int reverse_all { get; set; }
    }
    public class RazorpayTransferResponse
    {
        public string id { get; set; }
        public string entity { get; set; }
        public string source { get; set; }
        public string recipient { get; set; }
        public int amount { get; set; }
        public string currency { get; set; }
        public int amount_reversed { get; set; }
        public List<RazorpayTransferNote> notes { get; set; }
        public bool on_hold { get; set; }
        public int? on_hold_until { get; set; }
        public string recipient_settlement_id { get; set; }
        public int? created_at { get; set; }
        public int? processed_at { get; set; }
        public string[] linked_account_notes { get; set; }
    }
    public class RazorpayTransferNote
    {
        public string name { get; set; }
        public string roll_no { get; set; }

    }  
    public class Error
    {
        public string code { get; set; }
        public string description { get; set; }
        public string source { get; set; }
        public string step { get; set; }
        public string reason { get; set; }
        public Metadata metadata { get; set; }
    }

    public class Metadata
    {
    }

    public class ErrorRazorpayResponse
    {
        public Error error { get; set; }
    }


}
