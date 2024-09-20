using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.CustomerReferralDc
{
    public class CustomerReferralDc
    {

    }
    public enum ReferralType
    {
        Customer=1,
        People=2,
        Consumer = 3
    }
    public enum OnDeliverd
    {
        Deliverd=1
    }
    public class GetCustReferralConfigDc 
    {
        public int OnOrder { get; set; }
        public string OrderCount { get; set; }
        public string orderStatus { get; set; }
        public int OnDeliverd { get; set; }
        public double ReferralWalletPoint { get; set; }
        public double CustomerWalletPoint { get; set; }
    }

    public class AppReferralConfig
    {
        public int SingupWallet { get; set; }
        public List<GetCustReferralConfigDc> CustReferralConfigDcs { get; set; }
    }

    public class GetPeopleReferralOrderListDc
    {
        public string PeopleName { get; set; }
        public string ShopName { get; set; }
        public string SkCode { get; set; }
        public string ReferralSkCode { get; set; }
        public double ReferralWalletPoint { get; set; }
        public double CustomerWalletPoint { get; set; }
        public int OnOrder { get; set; }
        public int OrderId { get; set; }
        public string Status { get; set; }
        public int IsUsed { get; set; }
    }

    public class UpdateSalesManProfileImageDc
    {
        public int PeopleId { get; set; }
        public string ProfilePic { get; set; }
    }

    public class SalesCustomerRegistor
    {
        public string MobileNumber { get; set; }
        public int PeopleId { get; set; }
        public string Otp { get; set; }
        public string ShopName { get; set; }
        public string Shopimage { get; set; }
        public double lat { get; set; }
        public double lg { get; set; }
    }
}
