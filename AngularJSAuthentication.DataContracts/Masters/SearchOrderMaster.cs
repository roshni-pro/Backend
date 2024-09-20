using System;

namespace AngularJSAuthentication.DataContracts.Masters
{
    public class SearchOrderMaster
    {
        public int TotalCount { get; set; }
        public int OrderId { get; set; }
        public string OrderColor { get; set; }
        public int OrderType { get; set; }
        public string Status { get; set; }
        public string invoice_no { get; set; }
        public string OfferCode { get; set; }
        public string SalesPerson { get; set; }
        public int WarehouseId { get; set; }
        public string WarehouseName { get; set; }
        public int ClusterId { get; set; }
        public string ClusterName { get; set; }
        public DateTime CreatedDate { get; set; }
        public string Trupay { get; set; }
        public string CustomerId { get; set; }
        public string SkCode { get; set; }
        public string ShopName { get; set; }
        public string CustomerName { get; set; }
        public string Customerphonenum { get; set; }
        public double OrderAmount { get; set; }
        public double DispatchAmount { get; set; }
        public double TCSAmount { get; set; }
        public bool? IsPrimeCustomer { get; set; }
        public int ReDispatchCount { get; set; }
        public DateTime? PrioritizedDate { get; set; }
        #region NotFromDB
        public DateTime Deliverydate { get; set; }
        public double RemainingTime
        {
            get
            {
                if ((Deliverydate.Day - CreatedDate.Day) > 2)
                {
                    if (DateTime.Now >= Deliverydate.AddDays(-1))
                    {
                        return (DateTime.Now - Deliverydate.AddDays(-1)).TotalSeconds;
                    }
                    else
                    {
                        return 0;
                    };
                }
                else
                {
                    if (DateTime.Now >= Deliverydate.AddDays(-2))
                    {
                        return (DateTime.Now - Deliverydate.AddDays(-2)).TotalSeconds;
                    }
                    else
                    {
                        return 0;
                    };
                };
            }
        }

        public string OrderTypestr
        {
            get
            {
                if (OrderType == 0 || OrderType == 1)
                    return "G"; //General
                else if (OrderType == 2)
                    return "B"; //Bundle
                else if (OrderType == 3)
                    return "R"; //Return
                else if (OrderType == 4)
                    return "D"; //Distributor
                else if (OrderType == 5)
                    return "Z"; //Zaruri
                else if (OrderType == 6)
                    return "DO"; //Damage Order
                else if (OrderType == 7)
                    return "F"; //Franshise
                else if (OrderType == 8)
                    return "CL"; //Clearance
                else if (OrderType == 9)
                    return "N"; //Clearance
                else if (OrderType == 10)
                    return "NR"; //NONREVENUE ORder
                else if (OrderType == 11)
                    return "C"; //Consumer ORder
                else
                    return "G";

            }
        }

        public string Distance { get; set; }

        #endregion


        public int OrderTakenSalesPersonId { get; set; }
        public string OrderTakenSalesPerson { get; set; }

        public double custlat { get; set; }
        public double custlg { get; set; }
        public double whlat { get; set; }
        public double whlg { get; set; }
        public double? WalletAmount { get; set; }
        public double? BillDiscountAmount { get; set; }

        public string ReasonCancle { get; set; }
        public int? OrderDispatchedMasterId { get; set; }

        public string OrderStatusOtp { get; set; }
        public string OrderOtpStatus { get; set; }

        public double reditemavaiableValue { get; set; }

        public string reditemavaiableValuestr
        {
            get
            {
                string result = "";
                if ((OrderColor == "rgb(255, 153, 153)" || OrderColor == "yellow") && reditemavaiableValue > 0)
                {
                    result = reditemavaiableValue + "<br/>" + Math.Round((reditemavaiableValue / (OrderAmount + (WalletAmount ?? 0) + (BillDiscountAmount ?? 0))) * 100, 2) + "%";
                }
                return result;
            }
        }

        public double deliveryDistance { get; set; }
        public string CustomerType { get; set; }
        public int? ParentOrderId { get; set; }
        public bool IsFirstOrder { get; set; }  // Added On 07 Fab 2022
        public double VANAmount { get; set; }
        public bool IsOnlineRefundEnabled { get; set; }
        public bool IsOtpShow { get; set; }

        public DateTime? NextRedispatchDate { get; set; }
        public string CRMTags { get; set; }
        public bool? IsDigitalOrder { get; set; }
        public DateTime? DispatchDate { get; set; }

        public bool? IsVideoSeen { get; set; }
        public string VideoUrl { get; set; }
        public string UserType{ get; set; }
        public long? DeliveryOtpId { get; set; }

    }

    public class SearchOrderExport
    {
        public int OrderId { get; set; }
        public string OrderType { get; set; }
        public string invoice_no { get; set; }
        public string OfferCode { get; set; }
        public string SalesPerson { get; set; }
        public string WarehouseName { get; set; }
        public string ClusterName { get; set; }
        public DateTime CreatedDate { get; set; }
        public string PaymentMode { get; set; }
        public int CustomerId { get; set; }
        public string SkCode { get; set; }
        public string ShopName { get; set; }
        public string CustomerName { get; set; }
        public string Customerphonenum { get; set; }
        public string OrderedTotalAmt { get; set; }
        public int itemmultimrpid { get; set; }

        public double? DispatchAmount { get; set; }
        public string Status { get; set; }
        public bool IsPrimeCustomer { get; set; }
        public string OrderTakenSalesPerson { get; set; }
        public double? WalletAmount { get; set; }
        public double? BillDiscountAmount { get; set; }
        public int ItemId { get; set; }
        public string ItemName { get; set; }
        public string ABC_Classification { get; set; }
        public string CategoryName { get; set; }
        public string SubcategoryName { get; set; }
        public string BrandName { get; set; }
        public string HSNCode { get; set; }
        public string sellingSKU { get; set; }
        public double? TotalAmt { get; set; }
        public double? UnitPrice { get; set; }
        public double? MinOrderQtyPrice { get; set; }
        public int qty { get; set; }
        public double? DiscountAmount { get; set; }
        public double? DiscountPercentage { get; set; }
        public double? TaxAmmount { get; set; }
        public double? TaxPercentage { get; set; }
        public double? SGSTTaxAmmount { get; set; }
        public double? SGSTTaxPercentage { get; set; }
        public double? CGSTTaxPercentage { get; set; }
        public double? IGSTTaxAmount { get; set; }
        public double? IGSTTaxPercent { get; set; }
        public double? deliveryCharge { get; set; }
        public string GSTN_No { get; set; }
        public string comments { get; set; }
        public int? DeliveryIssuanceIdOrderDeliveryMaster { get; set; }
        public string StoreName { get; set; }
        public string CreditNoteNumber { get; set; }
        public DateTime? CreditNoteDate { get; set; }
        public string OrderColor { get; set; }

        public string CustomerType { get; set; }
        public bool IsFirstOrder { get; set; }  // Added On 07 Fab 2022

    }
}
