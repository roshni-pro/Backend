using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AngularJSAuthentication.DataContracts.Shared;

namespace AngularJSAuthentication.Model
{

    public class ShoppingCartBODc
    {
        public List<IBODetail> itemDetails { get; set; }
        public string CustomerName { get; set; }
        public string ShippingAddress { get; set; }
        public double? BillDiscountAmount { get; set; }
        public double UsedWalletAmount { get; set; }
        public string MobileNo { get; set; }
        public double NetAmount { get; set; }
        public string RefNo { get; set; }
        public List<int> OfferIds { get; set; }
        public string CreditNoteNumber { get; set; }


    }
    public class IBODetail
    {
        public int ItemId { get; set; }
        public int qty { get; set; }
        public int WarehouseId { get; set; }
        public double UnitPrice { get; set; }
        public bool IsSuccess { get; set; }
        public string Message { get; set; }
        public List<BOBatchDetail> batchdetails { get; set; }
        

    }
    public class BOBatchDetail
    {
        public long StockBatchMasterId { get; set; }
        public int qty { get; set; }

    }

    public class BackendOrderResponseMsg
    {
        public bool Status { get; set; }
        public string Message { get; set; }
        public dynamic Data { get; set; }

    }



    public class UpdateBOPaymentDc
    {
        public List<BOPaymentDc> BOPayments { get; set; }
        public int OrderId { get; set; }
        public string Status { get; set; }  //PaymentSuccess Or Order Canceled
    }
    public class BOPaymentDc
    {
        public string PaymentMode { get; set; }  // Cash,UPI,RTGS, POS 
        public double Amount { get; set; }
        public string PaymentRefNo { get; set; }


    }
    public class BackendOrderPlaceOrderResponse
    {

        public BackendOrderPlacedOrderMasterDTM OrderMaster { get; set; }
        public CustomerShoppingCartDc cart { get; set; }
        public int WheelCount { get; set; }
        public double WheelAmountLimit { get; set; }
        public int DialEarnigPoint { get; set; }
        public decimal KisanDaanAmount { get; set; }
        public int EarnWalletPoint { get; set; }
        public string Message { get; set; }
        public bool NotServicing { get; set; }
        public bool IsSuccess { get; set; }
    }
    public class BackendOrderPlacedOrderMasterDTM
    {
        public int OrderId { get; set; }
        public double TotalAmount { get; set; }
        public int WheelCount { get; set; }
        public double WheelAmountLimit { get; set; }
        public List<int> WheelList { get; set; }
        public int DialEarnigPoint { get; set; }
        public string Skcode { get; set; }
        public int CustomerId { get; set; }
        public int WarehouseId { get; set; }
        public int? PlayedWheelCount { get; set; }
        public string RSAKey { get; set; }
        public string HDFCOrderId { get; set; }
        public string eplOrderId { get; set; }
        public DateTime ExpectedETADate { get; set; }
        public bool IsDefaultDeliveryChange { get; set; }

    }
    public class ApplyNewOfferDC
    {
        public int WarehouseId { get; set; }
        public int CustomerId { get; set; }
        public int OfferId { get; set; }
        public List<int> ExistingOfferId { get; set; }
        public List<IBODetail> iBODetails { get; set; }
    }
    public class ReturnCalulateOfferValueDC
    {
        public int OrderId { get; set; }
        public List<ReturnItemDetails> returnItemDetails { get; set; }
    }
    public class ReturnItemDetails
    {
        public int ItemId { get; set; }
        public int ItemMultiMRPId { get; set; }
        public int Returnqty { get; set; }
        public int OrderDetailsId { get; set; }
        public bool IsFreeItem { get; set; }

        public List<ItemWiseBatchCode> ItemWiseBatchCodeLists { get; set; }
    }

    
    public class CreditNoteDetailsDC
    {
        public string Message { get; set; }
        public bool Status { get; set; }
        public string CreditNoteNumber { get; set; }
        public double Amount { get; set; }
        public DateTime? CreditNoteValidTill { get; set; }
        public string SKCode { get; set; }
        public string MobileNo { get; set; }
        public DateTime? CreatedDate { get; set; }
   
    }

    public class ResponseCalulateOfferValueDC
    {
        public double CalculateBillDiscount { get; set; }
        public double ReturnAmount { get; set; }
        public DateTime? ValidTill { get; set; }
    }
    public class ReturnConsumerDetailDC
    {
        public int CustomerId { get; set; }
        public int OrderId { get; set; }
        //public long KKRRRequestId { get; set; }
        //public List<int> OrderDetailsId { get; set; }
        public List<ReturnItemDetails> returnItemDetails { get; set; }
    }

    public class ResponseItemWiseBatchCode
    {
        public List<ItemWiseBatchCode> ItemWiseBatchCode { get; set; }
        public string Message { get; set; }
        public bool Status { get; set; }



    }
    public class ItemWiseBatchCode
    {
        public string BatchCode { get; set; }
        public int OrderDetailsId { get; set; }
        public string itemNumber { get; set; }
        public long StockBatchMasterId { get; set; }
        public int MultiMrpId { get; set; }
        public int Quantity { get; set; }
        public int Returnqty { get; set; }
        public bool IsFreeItem { get; set; }



    }
    public class WarehouseQrDC
    {
        public int WarehouseId { get; set; }
        public string FcmId{ get; set; }
        //public long KKRRRequestId { get; set; }
        //public List<int> OrderDetailsId { get; set; }
        //public List<ReturnItemDetails> returnItemDetails { get; set; }
    }
}
