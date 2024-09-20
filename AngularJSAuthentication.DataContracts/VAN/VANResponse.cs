using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.VAN
{
    public class VANResponse
    {
        public List<GenericCorporateAlertRequest> GenericCorporateAlertRequest { get; set; }
    }

    public class GenericCorporateAlertRequest
    {
        [JsonProperty(PropertyName = "Alert Sequence No")]
        public string AlertSequenceNo { get; set; }

        public string BenefDetails2 { get; set; }
        public string Accountnumber { get; set; }
        public string DebitCredit { get; set; }
        public double Amount { get; set; }
        [JsonProperty(PropertyName = "Remitter Name")]
        public string RemitterName { get; set; }
        [JsonProperty(PropertyName = "Remitter Account")]
        public string RemitterAccount { get; set; }
        [JsonProperty(PropertyName = "Remitter Bank")]
        public string RemitterBank { get; set; }
        [JsonProperty(PropertyName = "Remitter IFSC")]
        public string RemitterIFSC { get; set; }
        [JsonProperty(PropertyName = "Cheque No")]
        public string ChequeNo { get; set; }
        [JsonProperty(PropertyName = "User Reference Number")]
        public string UserReferenceNumber { get; set; }
        [JsonProperty(PropertyName = "Mnemonic Code")]
        public string MnemonicCode { get; set; }
        [JsonProperty(PropertyName = "Transaction Description")]
        public string TransactionDescription { get; set; }
        [JsonProperty(PropertyName = "Transaction Date")]
        public DateTime TransactionDate { get; set; }
    }

    public class VANReturnResponse
    {
        public ReturnResp GenericCorporateAlertResponse { get; set; }

    }

    public class ReturnResp
    {
        public string errorCode { get; set; }
        public string errorMessage { get; set; }
        public string domainReferenceNo { get; set; }
    }

    public class RTGSTransactionResponse
    {
        public double Amount { get; set; }
        public long ObjectId { get; set; }
        public string comment { get; set; }
        public DateTime CreatedDate { get; set; }
        public string RefNo { get; set; }
    }

    public class VANPaymentUploadDc
    {
        public string Clientcode{ get; set; }
        public string Reference_No { get; set; }
        public double Amount { get; set; }
        public int userid { get; set; }
    }
    public enum PaymentStatus {
        Pending=0,
        Success=1,
        Deviation=2
    }
}
