using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace AngularJSAuthentication.DataContracts.Transaction.Mongo
{
    public class MongoOrderMaster
    {
        [BsonId]
        public ObjectId Id { get; set; }
        public Guid GUID { get; set; }
        public int OrderId { get; set; }
        public int CompanyId { get; set; }
        public int? SalesPersonId { get; set; }
        public string SalesPerson { get; set; }
        public string SalesMobile { get; set; }
        public int CustomerId { get; set; }
        public string CustomerName { get; set; }
        public string Skcode { get; set; }
        public string ShopName { get; set; }
        public string Status { get; set; }
        public string invoice_no { get; set; }
        public string Trupay { get; set; }

        public string paymentThrough { get; set; }
        public string TrupayTransactionId { get; set; }
        public string paymentMode { get; set; }
        public int CustomerCategoryId { get; set; }
        public string CustomerCategoryName { get; set; }
        public string CustomerType { get; set; }
        public string LandMark { get; set; }
        public string Customerphonenum { get; set; }
        public string BillingAddress { get; set; }
        public string ShippingAddress { get; set; }
        public double TotalAmount { get; set; }
        public double GrossAmount { get; set; }
        public double DiscountAmount { get; set; }
        public double TaxAmount { get; set; }

        public double TCSAmount { get; set; }
        public double SGSTTaxAmmount { get; set; }
        public double CGSTTaxAmmount { get; set; }
        public int? CityId { get; set; }
        public int WarehouseId { get; set; }
        public string WarehouseName { get; set; }
        public bool active { get; set; }
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime CreatedDate { get; set; }
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime Deliverydate { get; set; }
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime UpdatedDate { get; set; }
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime? ReadytoDispatchedDate { get; set; }
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime? DeliveredDate { get; set; }
        public bool Deleted { get; set; }
        public int ReDispatchCount { get; set; }
        public int DivisionId { get; set; }
        public string ReasonCancle { get; set; }
        public int ClusterId { get; set; }
        public string ClusterName { get; set; }
        public double? deliveryCharge { get; set; }
        public double? WalletAmount { get; set; }
        public double? walletPointUsed { get; set; }
        public double? UsedPoint { get; set; }
        public double? RewardPoint { get; set; }
        public double ShortAmount { get; set; }
        public string comments { get; set; }
        public int? OrderTakenSalesPersonId { get; set; }
        public string OrderTakenSalesPerson { get; set; }
        public string Tin_No { get; set; }
        public string ShortReason { get; set; }
        public virtual List<OrderDetails> orderDetails { get; set; }
        public bool orderProcess { get; set; }
        public bool accountProcess { get; set; }
        public bool chequeProcess { get; set; }
        public bool epaymentProcess { get; set; }
        public double Savingamount { get; set; }
        public double OnlineServiceTax { get; set; }

        public byte[] InvoiceBarcodeImage { get; set; }

        public int userid { get; set; }

        public string Description { get; set; }

        public bool IsLessCurrentStock { get; set; }

        public double? BillDiscountAmount { get; set; } // Bill Discount Amount Harry 14/06/2019

        public string offertype { get; set; }

        public double RemainingTime
        {
            get
            {
                return (DateTime.Now - CreatedDate).TotalSeconds;
            }
        }

        public bool IsIgstInvoice { get; set; }

        public bool InactiveStatus => Status == "Inactive";


        public List<OrderMasterHistories> OrderMasterHistories { get; set; }
        public int? DeliveryIssuanceIdOrderDeliveryMaster { get; set; }
        public int? OrderDispatchedMasterId { get; set; }
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime OrderDate { get; set; }
        
        public double OrderAmount { get; set; }

        public double? DispatchAmount { get; set; }

        public double? DeliveredAmount { get; set; }
        public string OfferCode { get; set; }
        public string EwayBillNumber { get; set; }
        public string CreditNoteNumber { get; set; }
        public DateTime? CreditNoteDate { get; set; }
        public int OrderType { get; set; }
        public double? BillDiscountWallet { get; set; }
        public bool? IsPrimeCustomer { get; set; }

        public double? Lat { get; set; }
        public double? Lng { get; set; }
        public int? ParentOrderId { get; set; }
        public long OrderPickerMasterId { get; set; }
        public long TripPlannerMasterId { get; set; }
        public bool IsReAttempt { get; set; }
        public int ReAttemptCount { get; set; }
        public DateTime? NextRedispatchDate { get; set; }
        public bool IsFirstOrder { get; set; }
        public bool? IsDigitalOrder { get; set; }
    }

    public class OrderDetails
    {
        public Guid GUID { get; set; }
        public int OrderDetailsId { get; set; }
        public int OrderId { get; set; }
        public int CustomerId { get; set; }
        public string CustomerName { get; set; }
        public String City { get; set; }
        public string Mobile { get; set; }
        public DateTime OrderDate { get; set; }
        public int CompanyId { get; set; }
        public int? CityId { get; set; }
        public int WarehouseId { get; set; }
        public string WarehouseName { get; set; }
        public string CategoryName { get; set; }
        public string SubcategoryName { get; set; }
        public string SubsubcategoryName { get; set; }
        public string SellingSku { get; set; }
        public int ItemId { get; set; }
        public int? FreeWithParentItemId { get; set; }//Child 
        public string Itempic { get; set; }
        public string itemname { get; set; }
        public string SellingUnitName { get; set; }
        public string itemcode { get; set; }
        public string itemNumber { get; set; }
        public string HSNCode { get; set; }
        public string Barcode { get; set; }

        public double price { get; set; }
        public double UnitPrice { get; set; }
        public double Purchaseprice { get; set; }
        public double PTR { get; set; }
        public int MinOrderQty { get; set; }
        public double MinOrderQtyPrice { get; set; }
        public int qty { get; set; }

        public int Noqty { get; set; }
        public double AmtWithoutTaxDisc { get; set; }
        public double AmtWithoutAfterTaxDisc { get; set; }
        public double TotalAmountAfterTaxDisc { get; set; }

        public double NetAmmount { get; set; }
        public double DiscountPercentage { get; set; }
        public double DiscountAmmount { get; set; }
        public double NetAmtAfterDis { get; set; }
        public double TaxPercentage { get; set; }
        public double TaxAmmount { get; set; }
        public double SGSTTaxPercentage { get; set; }
        public double SGSTTaxAmmount { get; set; }
        public double CGSTTaxPercentage { get; set; }
        public double CGSTTaxAmmount { get; set; }
        //for cess
        public double TotalCessPercentage { get; set; }
        public double CessTaxAmount { get; set; }


        public double TotalAmt { get; set; }

        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
        public bool Deleted { get; set; }
        public string Status { get; set; }
        public double SizePerUnit { get; set; }
        public int? marginPoint { get; set; }
        public int? promoPoint { get; set; }
        public double NetPurchasePrice { get; set; }
        public bool IsFreeItem { get; set; }
        public bool IsDispatchedFreeStock { get; set; }

        public string ABCClassification { get; set; }
        public double CurrentStock { get; set; }

        public int day { get; set; }

        public int month { get; set; }

        public int year { get; set; }

        public string status { get; set; }
        public string SupplierName { get; set; }
        //multimrp
        public int ItemMultiMRPId { get; set; }
        public double IGSTTaxAmount { get; set; }
        public double IGSTTaxPercent { get; set; }

        public double? OrderedTotalAmt { get; set; }
        [NotMapped]
        public bool ISItemLimit { get; set; }
        [NotMapped]
        public int ItemLimitQty { get; set; }

        [NotMapped]
        public double SavingAmount { get; set; }
        [NotMapped]
        public string Category { get; set; }
        public double ActualUnitPrice { get; set; }
        public long StoreId { get; set; }
        public string StoreName { get; set; }
        public int ExecutiveId { get; set; }
        public string ExecutiveName { get; set; }
        public double? Lat { get; set; }
        public double? Lng { get; set; }
    }

    public class OrderMasterHistories
    {
        public Guid GUID { get; set; }
        public int id { get; set; }
        public int orderid { get; set; }
        public string Status { get; set; }
        public int? DeliveryIssuanceId { get; set; }
        public string Description { get; set; }
        public string Reasoncancel { get; set; }
        public string Warehousename { get; set; }
        public int userid { get; set; }
        public string username { get; set; }
        public DateTime CreatedDate { get; set; }
        public bool IsReAttempt { get; set; }

    }
}
