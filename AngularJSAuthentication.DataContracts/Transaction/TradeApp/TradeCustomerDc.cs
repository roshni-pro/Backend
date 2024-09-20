using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.Transaction.TradeApp
{

    public class TradeCustomerDc
    {
        public long Id { get; set; }
        public long CustomerId { get; set; }
        public string CustomerName { get; set; }
        public string Mobile { get; set; }
        public decimal? Lat { get; set; }
        public decimal? Lng { get; set; }
        public double? BuyerRating { get; set; }
        public double? SellerRating { get; set; }
        public bool AllowTrade { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public long? CreatedBy { get; set; }
        public long? ModifiedBy { get; set; }
        public bool IsActive { get; set; }
        public bool IsDelete { get; set; }
        public string SkCode { get; set; }
        public string City { get; set; }
        public string Email { get; set; }
        public string GstNo { get; set; }
        public bool IsSeller { get; set; }
        //public bool IsBuyer { get; set; }
        public string Address { get; set; }
        public bool IsOrdersForSeller { get; set; }
        public bool TermsAgreed { get; set; }
        public string Password { get; set; }
        public string AccountNumber { get; set; }
        public string BankName { get; set; }
        public string ReferralSkCode { get; set; }
        public bool? IsReferral { get; set; }
        public string IFSC { get; set; }

    }
}
