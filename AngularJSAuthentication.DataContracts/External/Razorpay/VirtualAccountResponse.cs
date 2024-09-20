using System.Collections.Generic;

namespace AngularJSAuthentication.DataContracts.External.Razorpay
{
    public class VirtualAccountResponse
    {
        public string id { get; set; }
        public string name { get; set; }
        public string entity { get; set; }
        public string status { get; set; }
        public string description { get; set; }
        public int amount_expected { get; set; }
        public int amount_paid { get; set; }
        public string customer_id { get; set; }
        public int? close_by { get; set; }
        public int? closed_at { get; set; }
        public long created_at { get; set; }
        public Notes notes { get; set; }
        public List<VirtualAccountResponseReceivers> receivers { get; set; }

        public ErrorMetaData error { get; set; }

    }
    public class ErrorResponse
    {
        public string code { get; set; }
        public string description { get; set; }
        public string source { get; set; }
        public string field { get; set; }
        public string step { get; set; }
        public string reason { get; set; }
        public ErrorMetaData metadata { get; set; }

    }

    public class ErrorMetaData
    {
        public string payment_id { get; set; }
        public string order_id { get; set; }

    }

    public class VirtualAccountResponseReceivers
    {
        public string id { get; set; }
        public string entity { get; set; }
        public string reference { get; set; }
        public string short_url { get; set; }
        public int created_at { get; set; }

    }


    public class VirtualAccountPaymentResponse
    {
        public string entity { get; set; }
        public int count { get; set; }
        public List<VirtualAccountPaymentCollection> items { get; set; }
    }

    public class VirtualAccountPaymentCollection
    {
        public string id { get; set; }
        public string entity { get; set; }
        public int amount { get; set; }
        public string currency { get; set; }
        public string status { get; set; }
        public string order_id { get; set; }
        public string invoice_id { get; set; }
        public bool international { get; set; }
        public string method { get; set; }
        public int amount_refunded { get; set; }
        public string refund_status { get; set; }
        public bool captured { get; set; }
        public string description { get; set; }
        public string card_id { get; set; }
        public string bank { get; set; }
        public string wallet { get; set; }
        public string vpa { get; set; }
        public string email { get; set; }
        public string contact { get; set; }
        public int fee { get; set; }
        public int tax { get; set; }
        public string error_code { get; set; }
        public string error_description { get; set; }
        public int created_at { get; set; }
    }

}
