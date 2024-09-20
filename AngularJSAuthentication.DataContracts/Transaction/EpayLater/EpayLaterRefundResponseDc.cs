using System;
using System.Collections.Generic;

namespace AngularJSAuthentication.DataContracts.Transaction.EpayLater
{
    public class EpayLaterRefundResponseDc
    {
        public long id { get; set; }
        public double amount { get; set; }
        public DateTime date { get; set; }
        public string currencyCode { get; set; }
        public bool paylater { get; set; }
        public string status { get; set; }
        public string marketplaceOrderId { get; set; }

    }

    public class Address
    {
        public string fullAddress { get; set; }
        public string fullAddressWithLine1CityState { get; set; }
        public string line1 { get; set; }
        public string city { get; set; }
        public string state { get; set; }
        public string postcode { get; set; }
    }
    public class Merchant
    {
        public string marketplaceMerchantId { get; set; }
        public string name { get; set; }
        public string telephoneNumber { get; set; }
        public string emailAddress { get; set; }
        public Address address { get; set; }
    }
    public class Customer
    {
        public string firstName { get; set; }
        public string lastName { get; set; }
        public string emailAddress { get; set; }
        public string telephoneNumber { get; set; }
        public bool nachRepaymentEnabled { get; set; }
    }
    public class EpayLaterRefundAcceptResponseDc
    {
        public long Id { get; set; }
        public double Amount { get; set; }
        public string CurrencyCode { get; set; }
        public DateTime Date { get; set; }
        public bool Paylater { get; set; }
        public string Status { get; set; }
        public string MarketplaceOrderId { get; set; }
        public double ReturnAmount { get; set; }
        public string MarketplaceRefundId { get; set; }
    }


    public class EpayLtrErrors
    {
        public List<string> errors { get; set; }
    }
    public class EpayLtrReturnError
    {
        public string type { get; set; }
        public string reason { get; set; }
        public string code { get; set; }
        public string message { get; set; }
        public string eplErrorCode { get; set; }
    }




}
