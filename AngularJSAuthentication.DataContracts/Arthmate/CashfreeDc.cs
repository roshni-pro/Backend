using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.Arthmate
{
    public class CashfreeDc
    {
    }



    #region CreateSubscriptionwithPlanInfo

    public class CreateSubscriptionwithPlanInfoDc
    {
        public string subscriptionId { get; set; } //always New UniqueId
        public string customerName { get; set; }
        public string customerEmail { get; set; }
        public string customerPhone { get; set; }
        public double authAmount { get; set; } //1
        public Date expiresOn { get; set; }
        public string returnUrl { get; set; }
        public Date firstChargeDate { get; set; }
        public PlanInfo planInfo { get; set; }
        public List<string> notificationChannels { get; set; }
        public bool tpvEnabled { get; set; }
        public PayerAccountDetails payerAccountDetails { get; set; }
        public Notes notes { get; set; }
        public int linkExpiry { get; set; }
        public bool refundAuthAmount { get; set; }
    }
    public class Notes
    {
        public string key1 { get; set; }
        public string key2 { get; set; }
        public string key3 { get; set; }
        public string key4 { get; set; }
    }
    public class PayerAccountDetails
    {
        public string accountNumber { get; set; }
        public string accountHolderName { get; set; }
        public string bankId { get; set; }
        public string accountType { get; set; }
        public string ifsc { get; set; }
    }
    public class PlanInfo
    {
        public string type { get; set; } //PERIODIC 
        public string planName { get; set; }
        public double maxAmount { get; set; }
        public int maxCycles { get; set; }
        public string intervalType { get; set; }
        public int intervals { get; set; }
        public double recurringAmount { get; set; }

    }

    public class CreateSubscriptionwithPlanInfoRes
    {
        public string status { get; set; }
        public string message { get; set; }
        public SubscriptionwithPlanInfoResData Data { get; set; }
        public string subCode { get; set; }
    }
    public class SubscriptionwithPlanInfoResData
    {
        public int subReferenceId { get; set; }
        public string authLink { get; set; }
    }
    #endregion



    #region  Get SubscriptionDetails

    public class SubscriptionDetailsRes
    {
        public string status { get; set; }
        public string message { get; set; }
        public Subscription subscription { get; set; }
    }

    public class Subscription
    {
        public string subscriptionId { get; set; }
        public int subReferenceId { get; set; }
        public string planId { get; set; }
        public string planName { get; set; }
        public string type { get; set; }
        public double maxAmount { get; set; }
        public string currency { get; set; }
        public string customerName { get; set; }
        public string customerEmail { get; set; }
        public string customerPhone { get; set; }
        public string mode { get; set; }
        public string cardNumber { get; set; }
        public string status { get; set; }
        public string firstChargeDate { get; set; }
        public string addedOn { get; set; }
        public string scheduledOn { get; set; }
        public int currentCycle { get; set; }
        public string authLink { get; set; }
        public string bankAccountNumber { get; set; }
        public string bankAccountHolder { get; set; }
        public string umrn { get; set; }
        public bool tpvEnabled { get; set; }
        public PayerAccountDetails payerAccountDetails { get; set; }
    }


    #endregion


    #region   UpdateRecurring
    public class UpdateRecurringAmount
    {
        public double amount { get; set; }
    }

    public class UpdateRecurringRes
    {
        public string status { get; set; }  // OK //ERROR
        public string message { get; set; }
        public string subCode { get; set; }
    }
    #endregion


    #region  CancelSubscription
    public class CancelSubscriptionRes
    {
        public string status { get; set; }
        public string message { get; set; }
        public string subCode { get; set; }
    }

    #endregion
}
