using AngularJSAuthentication.Model;
using AngularJSAuthentication.Model.PlaceOrder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.Masters
{
    public class GenerateClearanceOrderDc
    {
        public ClearanceShoppingCart cart { get; set; }
        public string Message { get; set; }
        public bool IsSuccess { get; set; }
        public double GrossAmount { get; set; }
        public DateTime CreatedDate { get; set; }
        public int OrderId { get; set; }
        public DateTime? ETADate { set; get; }

    }
    public class ClearanceShoppingCart
    {
        public int CustomerId { get; set; }
        public List<ClearanceIDetailDc> itemDetails { get; set; }
        public double TotalAmount { get; set; }
        public double GullakAmount { get; set; }
        public DateTime? CreatedDate { get; set; }
        public List<long> BillDiscountOfferId { get; set; } // Offer Id of Bill Discount (change to string from int)
        public double Lat { get; set; }
        public double Lng { get; set; }
        public string BlockGullakAmountGuid { get; set; }
        public int? PeopleId { get; set; }
        public string MOP { get; set; } //COD 

    }
    public class ClearanceIDetailDc
    {
        public long Id { get; set; }
        public int qty { get; set; }
        public double UnitPrice { get; set; }
        public double NewUnitPrice { get; set; }
        public int NewRemainingStockQty { get; set; }

        public bool IsSuccess { get; set; }
        public string Message { get; set; }
    }

    public class ApplyOfferResponse
    {
        public List<ClearanceIDetailDc> cart { get; set; }
        public double BillDiscount { get; set; }
        public List<ClearanceDiscount> BillDiscountOffers { get; set; }
        public bool IsSuccess { get; set; }
        public string Message { get; set; }
    }

    public class ClearanceDiscount
    {
        public int OfferId { get; set; }
        public double DiscountAmount { get; set; }
        public int NewBillingWalletPoint { get; set; }
    }

    public class ClFailedRes
    {
        public bool status { get; set; }
        public string UPITxnId { get; set; }
        public string Message { get; set; }
    }


}
