using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.UdharCreditLending
{
    public class UdharCreditLendingDc
    {
        public string UniqueCode { get; set; } //skcode
    }
    public class PostUdharCLAppDc
    {
        public int CustomerId { get; set; } //CustomerId
        public long OrderId { get; set; } //OrderId
        public long AccountId { get; set; } //AccountId of AccountCredit
        public double Amount { get; set; } //Orderamount
        public string PaymentMode { get; set; }


    }
    public class PostUdharCreditLimitDc
    {
        public long OrderId { get; set; } //OrderId
        public long AccountId { get; set; } //AccountId in AccountCredit
        public double Amount { get; set; } //Orderamount
        public string UniqueCode { get; set; } //skcode
        public string PaymentMode { get; set; }


    }


    public class UdharCreditLimitResponseMasterDc
    {
        public string Status { get; set; }
        public string ErrorMessage { get; set; }
        public UdharCreditLimitResponseDc Data { get; set; }
        public DateTime Timestamp { get; set; }
    }

    public class UdharCreditLimitResponseDc
    {
        public string UniqueCode { get; set; } //skcode
        public long AccountId { get; set; } //AccountId in AccountCredit
        public double Amount { get; set; } //Amount in AccountCredit
        public bool ShowHideLimit { get; set; } //ShowHideLimit in Account
    }

    public class PostUdharCreditLimitResponseDc
    {
        public long AccountId { get; set; } //AccountId 
        public double Amount { get; set; } // order amount  
        public long OrderId { get; set; } // order OrderId  
        public string TransactionRefNo { get; set; } // TransactionNo 
        public bool Status { get; set; } //Transaction status
    }

    public class RefundPaymentRequestDc
    {
        public int CustomerId { get; set; }
        public long OrderId { get; set; }
        public double Amount { get; set; }
        public string TrasanctionId { get; set; }
        public string returnType { get; set; } // full, partial
    }

}



