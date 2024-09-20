using System.Collections.Generic;

namespace AngularJSAuthentication.DataContracts.External.Razorpay
{
    public class RazorPayWebHookResponse
    {
        public string entity { get; set; }
        public string account_id { get; set; }
        public string @event { get; set; }
        public string[] contains { get; set; }
        public int created_at { get; set; }
        public Payload payload { get; set; }
        //public UpiTransfer upi_transfer { get; set; }
    }

    public class Payload
    {
        public Payment payment { get; set; }
        public VirtualAccount virtual_account { get; set; }
    }
    public class Payment
    {
        public Entity entity { get; set; }
    }
    public class Entity
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
        public int amount_transferred { get; set; }
        public string refund_status { get; set; }
        public bool captured { get; set; }
        public string description { get; set; }
        public string card_id { get; set; }
        public string bank { get; set; }
        public string wallet { get; set; }
        public string vpa { get; set; }
        public string email { get; set; }
        public string contact { get; set; }
        public string customer_id { get; set; }
        public int fee { get; set; }
        public int tax { get; set; }
        public int error_code { get; set; }
        public string error_description { get; set; }
        public acquirer_data acquirer_data { get; set; }
        public int created_at { get; set; }
    }

    public class acquirer_data
    {
        public string rrn { get; set; }
    }

    public class VirtualAccount
    {
        public VirtualAccountEntity entity { get; set; }
    }

    public class VirtualAccountEntity
    {
        public string id { get; set; }
        public string name { get; set; }
        public string entity { get; set; }
        public string status { get; set; }
        public string description { get; set; }
        public int? amount_expected { get; set; }
        public string[] notes { get; set; }
        public int amount_paid { get; set; }
        public string customer_id { get; set; }
        public int close_by { get; set; }
        public int closed_at { get; set; }
        public int created_at { get; set; }
        public List<VirtualAccountReceivers> receivers { get; set; }

    }

    public class VirtualAccountReceivers
    {
        public string id { get; set; }
        public string entity { get; set; }
        public string username { get; set; }
        public string handle { get; set; }
        public string address { get; set; }

    }

    public class UpiTransfer
    {
        public UpiTransferEntity entity { get; set; }
    }

    public class UpiTransferEntity
    {
        public string id { get; set; }
        public string entity { get; set; }
        public int amount { get; set; }
        public string payer_vpa { get; set; }
        public string payer_bank { get; set; }
        public string payer_account { get; set; }
        public string payer_ifsc { get; set; }
        public string payment_id { get; set; }
        public string rrn { get; set; }
        public string virtual_account_id { get; set; }

    }

    #region Consumer Webhook
    public class ConsumerRazorPayWebHookResponse
    {
        public string entity { get; set; }
        public string account_id { get; set; }
        public string @event { get; set; }
        public string[] contains { get; set; }
        public ConsumerPayload payload { get; set; }
        public int created_at { get; set; }
    }

    public class ConsumerPayload
    {
        public ConsumerPayment payment { get; set; }
        public Qr_Code qr_code { get; set; }
    }

    public class ConsumerPayment
    {
        public ConsumerEntity entity { get; set; }
    }

    public class ConsumerEntity
    {
        public string id { get; set; }
        public string entity { get; set; }
        public int amount { get; set; }
        public string currency { get; set; }
        public string status { get; set; }
        public object order_id { get; set; }
        public object invoice_id { get; set; }
        public bool international { get; set; }
        public string method { get; set; }
        public int amount_refunded { get; set; }
        public object refund_status { get; set; }
        public bool captured { get; set; }
        public string description { get; set; }
        public object card_id { get; set; }
        public object bank { get; set; }
        public object wallet { get; set; }
        public string vpa { get; set; }
        public string email { get; set; }
        public string contact { get; set; }
        public string customer_id { get; set; }
        public ConsumerNotes notes { get; set; }
        public int fee { get; set; }
        public int tax { get; set; }
        public object error_code { get; set; }
        public object error_description { get; set; }
        public object error_source { get; set; }
        public object error_step { get; set; }
        public object error_reason { get; set; }
        public Acquirer_Data acquirer_data { get; set; }
        public int created_at { get; set; }
    }

    public class ConsumerNotes
    {
        public string Branch { get; set; }
    }

    public class Acquirer_Data
    {
        public string rrn { get; set; }
    }

    public class Qr_Code
    {
        public Entity1 entity { get; set; }
    }

    public class Entity1
    {
        public string id { get; set; }
        public string entity { get; set; }
        public int created_at { get; set; }
        public string name { get; set; }
        public string usage { get; set; }
        public string type { get; set; }
        public string image_url { get; set; }
        public object payment_amount { get; set; }
        public string status { get; set; }
        public string description { get; set; }
        public bool fixed_amount { get; set; }
        public int payments_amount_received { get; set; }
        public int payments_count_received { get; set; }
        public Notes1 notes { get; set; }
        public string customer_id { get; set; }
        public int close_by { get; set; }
        public object closed_at { get; set; }
        public object close_reason { get; set; }
    }

    public class Notes1
    {
        public string Branch { get; set; }
    }


    #endregion
}
