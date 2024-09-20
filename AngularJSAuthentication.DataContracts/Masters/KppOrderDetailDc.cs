using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AngularJSAuthentication.Model
{

    public class KppOrderShoppingCartDc
    {
        public int CustomerId { get; set; }
        public int WarehouseId { get; set; }
        public double TotalAmount { get; set; }
        public double? DeliveryCharge { get; set; }
        public string paymentThrough { get; set; }
        public string TrupayTransactionId { get; set; }
        public string paymentMode { get; set; }
        public DateTime CreatedDate { get; set; }
        public List<KppOrderShoppingCartItemsDc> itemDetails { get; set; }
        public int PeopleId { get; set; }
        public int DboyId { get; set; }

        public string Customerphonenum { get; set; }
        public string CustomerName { get; set; }
        public string ShippingAddress { get; set; }
        public string InvoiceNo { get; set; }
        public double? BillDiscountAmount { get; set; } // Bill Discount Amount 

    }

    public class KppOrderShoppingCartItemsDc
    {
        public string ItemNumber { get; set; }
        public string ItemName { get; set; }
        public double MRP { get; set; }
        public int Qty { get; set; }
        public double UnitPrice { get; set; }

        public int? ItemId { get; set; }
        public int? CompanyId { get; set; }
        public int? WarehouseId { get; set; }

    }


    public class OfferFreeItemDc
    {
        public int Id { get; set; }
        public int offerId { get; set; }
        public string ItemNumber { get; set; }
        public string OfferType { get; set; }
        public int OfferMinimumQty { get; set; }
        public int OfferWalletPoint { get; set; }
        public int FreeItemId { get; set; }
        public double OfferQtyAvaiable { get; set; }
        public double OfferQtyConsumed { get; set; }
        public string OfferFreeItemName { get; set; }
        public int OfferFreeItemQuantity { get; set; }
        public string OfferFreeItemImage { get; set; }
        public double QtyAvaiable { get; set; }
        public int ItemId { get; set; }
        public bool IsActive { get; set; }
        public string OfferOn { get; set; }
        public int ItemMultiMRPId { get; set; }
    }
}
