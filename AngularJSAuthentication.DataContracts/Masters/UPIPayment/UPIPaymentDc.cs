using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.Masters.UPIPayment
{
    public class UPIPaymentDc
    {

    }
    public class callBackResDc
    {
        [DisplayName("UPI Txn ID")]
        public long UPITxnID { get; set; }
        [DisplayName("Merchant Trnx Reference")]
        public string MerchantTrnxReference { get; set; }
        [DisplayName("Amount")]
        public double Amount { get; set; }
        [DisplayName("Transaction Auth Date")]
        public string TransactionAuthDate { get; set; }
        [DisplayName("Status")]
        public string Status { get; set; }
        [DisplayName("Status Description")]
        public string StatusDescription { get; set; }
        [DisplayName("Response Code")]
        public string ResponseCode { get; set; }
        [DisplayName("Approval Number")]
        public string ApprovalNumber { get; set; }
        [DisplayName("Payer Virtual Address")]
        public string PayerVirtualAddress { get; set; }
        [DisplayName("Customer Reference No")]
        public string CustomerReferenceNo { get; set; }
        [DisplayName("Reference ID")]
        public string ReferenceID { get; set; }
        [DisplayName("Additional Field 1")]
        public string AdditionalField1 { get; set; }
        [DisplayName("Additional Field 2")]
        public string AdditionalField2 { get; set; }
        [DisplayName("Additional Field 3")]
        public string AdditionalField3 { get; set; }
        [DisplayName("Additional Field 4")]
        public string AdditionalField4 { get; set; }
        [DisplayName("Additional Field 5")]
        public string AdditionalField5 { get; set; }
        [DisplayName("Additional Field 6")]
        public string AdditionalField6 { get; set; }
        [DisplayName("Additional Field 7")]
        public string AdditionalField7 { get; set; }
        [DisplayName("Additional Field 8")]
        public string AdditionalField8 { get; set; }
        [DisplayName("Additional Field 9")]
        public string AdditionalField9 { get; set; }
        [DisplayName("Additional Field 10")]
        public string AdditionalField10 { get; set; }
        public string pgMerchantId { get; set; }


    }

    public class GenerateOrderAmtQRCodeDc
    {
        public List<int> orderIds { get; set; }
        public int peopleId { get; set; }
        // public double amount { get; set; }
    }


    public class GenerateSalesAppOrderAmtQRCodeDc
    {
        public int OrderId { get; set; }
        public int peopleId { get; set; }
        public double amount { get; set; }
    }

    public class GenerateBackEndAmtQRCode
    {
        public int OrderId { get; set; }
        // public int peopleId { get; set; }
        public double amount { get; set; }
    }



    public class QRPaymentResponseDc
    {
        public string QRexpireDateTime { get; set; }
        public bool Status { get; set; }
        public string msg { get; set; }
        public string QRCodeurl { get; set; }
        public double Amount { get; set; }
        public List<int> OrderIds { get; set; }

    }

    public class SalesAppQRGenerateDc
    {
        public bool Status { get; set; }
        public string msg { get; set; }
        public string QRCodeurl { get; set; }
        public double Amount { get; set; }
        public int OrderId { get; set; }
        public string UPITxnID { get; set; }
    }


    public class BackEndQRGenerateDc
    {
        public bool Status { get; set; }
        public string msg { get; set; }
        public string QRCodeurl { get; set; }
        public double Amount { get; set; }
        public int OrderId { get; set; }
        public string UPITxnID { get; set; }
    }


    public class InitiateDUPayInetentReqDc
    {
        public string PaymentReqId { get; set; }
        public double amount { get; set; }
        public int UserId { get; set; }

    }

    public class UPIPaymentResponseDc
    {
        public bool Status { get; set; }
        public string msg { get; set; }
        public string IntentString { get; set; }

    }

    public class PayInetentReqResDc
    {
        [DisplayName("Order no")]
        public string MerchantTrnxReference { get; set; }
        public string Status { get; set; }

        [DisplayName("Transaction Description")]
        public string TransactionDescription { get; set; }
    }
    public class PostInetentReqDc
    {
        public string requestMsg { get; set; }
        public string pgMerchantId { get; set; }
    }

    public class UPITransactionDetailDc
    {
        public double TxnAmount { get; set; }
        public string UPITxnID { get; set; }
        public string TxnStatus { get; set; }
        public DateTime? TxnDate { get; set; }

    }

    public class UpiStatusResponse
    {
        public long Id { get; set; }
        public string UPITxnID { get; set; }
        public double amount { get; set; }
        public string Status { get; set; }
        public bool IsSuccess { get; set; }
        public string TxnNo { get; set; }
        public DateTime? CreatedDate { get; set; }
        public int OrderId { get; set; }
        public string PaymentMode { get; set; }



    }

    public class RazorPayResponse
    {
        public string id { get; set; }
        public string entity { get; set; }
        public decimal amount { get; set; }
        //public string currency { get; set; }
        public string status { get; set; }
        //public object order_id { get; set; }
        //public object invoice_id { get; set; }
        //public bool international { get; set; }
        //public string method { get; set; }
        //public int amount_refunded { get; set; }
        //public object refund_status { get; set; }
        //public bool captured { get; set; }
        //public string description { get; set; }
        //public object card_id { get; set; }
        //public object bank { get; set; }
        //public object wallet { get; set; }
        //public string vpa { get; set; }
        //public string email { get; set; }
        //public string contact { get; set; }
        //public string customer_id { get; set; }
        //public object[] notes { get; set; }
        //public int fee { get; set; }
        //public int tax { get; set; }
        //public object error_code { get; set; }
        //public object error_description { get; set; }
        //public object error_source { get; set; }
        //public object error_step { get; set; }
        //public object error_reason { get; set; }
        //public Acquirer_Data acquirer_data { get; set; }
        //public int created_at { get; set; }
        //public Upi upi { get; set; }
    }

    //public class Acquirer_Data
    //{
    //    public string rrn { get; set; }
    //}

    //public class Upi
    //{
    //    public string payer_account_type { get; set; }
    //    public string vpa { get; set; }
    //}



}
