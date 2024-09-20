using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.Transaction.BackendOrder
{
    public class BackendItem
    {
        public int ItemId { get; set; }
        public int WarehouseId { get; set; }
        public int ItemMultiMRPId { get; set; }
        public int MinOrderQty { get; set; }
        public double UnitPrice { get; set; }
        public double WholeSalePrice { get; set; }
        public double TradePrice { get; set; }
        public double Consumerprice { get; set; }
        public double MRP { get; set; }
        public string ItemName { get; set; }
        public string ItemNumber { get; set; }
        public List<BackendItemBatchs> BackendItemBatchs { get; set; }
        public List<MoqItem> MoqItems { get; set; }
        public int CurrentInventory { get; set; }
        public List<AngularJSAuthentication.DataContracts.External.ItemDataDC> itemDataDCs { get; set; }

    }
    public class BackendItemBatchs
    {
        public long StockBatchMasterId { get; set; }
        public string BatchCode { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public DateTime? MFGDate { get; set; }
        public int Batchqty { get; set; }
        public int itemmultimrpid { get; set; }

    }
    public class MoqItem
    {
        public string Number { get; set; }
        public int MinOrderQty { get; set; }
        public double UnitPrice { get; set; }
        public int CurrentInventory { get; set; }
        public double WholeSalePrice { get; set; }
        public double TradePrice { get; set; }
    }

    public class SearchItemforStore
    {
        public int ItemMultiMRPId { get; set; }
        public string Itemname { get; set; }
    }
    public class GetStoreConfigDC
    {
        public long Id { get; set; }
        public string WarehouseName { get; set; }
        public int ItemMultiMRPId { get; set; }
        public string ItemName { get; set; }
        public string Typename { get; set; }
        public double Percentage { get; set; }
        public int TotalCount { get; set; }
    }
    public class GetStoreConfigPayload
    {
        public int WarehouseId { get; set; }
        public List<int> ItemmultimrpId { get; set; }

        public int Skip { get; set; }
        public int Take { get; set; }
    }
    public class AddStoreConfigDC
    {
        public int WarehouseId { get; set; }
        public List<int> ItemmultimrpId { get; set; }
        public int Type { get; set; }
        public double Percentage { get; set; }
    }
    public class StorePriceDC
    {
        //public string Wname { get; set; }
        //public int Wid { get; set; }
        public int Mrpid { get; set; }
        public string Pricetype { get; set; }
        public int Type { get; set; }
        public double Percentage { get; set; }

    }
    public class DownloadStorePriceDC
    {
        public int ItemMultiMrpId { get; set; }
        public string PriceType { get; set; }
        public double Percentage { get; set; }
    }
    public class CustomerShoppingCart
    {
        
        public double DeliveryCharges { get; set; }
        public double CartTotalAmt { get; set; }
        public double GrossTotalAmt { get; set; }
        public double TotalTaxAmount { get; set; }
        public double TotalDiscountAmt { get; set; }
        public double WalletAmount { get; set; }
        public double TCSPercent { get; set; }
        public int NewBillingWalletPoint { get; set; }
        public int DeamPoint { get; set; }
        public int TotalQty { get; set; }
        public int WheelCount { get; set; }
        public double TotalSavingAmt { get; set; }
        public int? GeneratedOrderId { get; set; }
        public int CustomerId { get; set; }
        public string SkCode { get; set; }
        public string Mobile { get; set; }
        public string ShopName { get; set; }
        public string City { get; set; }
        public int PeopleId { get; set; }
        public int WarehouseId { get; set; }

        public DateTime CreatedDate { get; set; }

        public DateTime? ModifiedDate { get; set; }
        public bool IsActive { get; set; }
        public bool? IsDeleted { get; set; }
        public int CreatedBy { get; set; }
        public int? ModifiedBy { get; set; }
        public List<ShoppingCartItem> ShoppingCartItems { get; set; }
        public List<ShoppingCartDiscount> ShoppingCartDiscounts { get; set; }
        public double PreTotalDispatched { get; set; }
        public double TCSLimit { get; set; }
    }
    
    public class ShoppingCartItem
    {
  
        public int ItemId { get; set; }
        public string ItemName { get; set; }
        public string ItemNumber { get; set; }
        public int ItemMultiMRPId { get; set; }
        public int qty { get; set; }
        public int FreeItemqty { get; set; }
        public double TotalFreeWalletPoint { get; set; }
        public double UnitPrice { get; set; }
        public bool IsFreeItem { get; set; }
        public bool? IsPrimeItem { get; set; }
        public bool? IsDealItem { get; set; }
        public double TaxPercentage { get; set; }
        public double TaxAmount { get; set; }

        public DateTime CreatedDate { get; set; }

        public DateTime? ModifiedDate { get; set; }
        public bool IsActive { get; set; }
        public bool? IsDeleted { get; set; }
        public int CreatedBy { get; set; }
        public int? ModifiedBy { get; set; }
        
    }

    public class ShoppingCartDiscount
    {
      
        public int? OfferId { get; set; }         
        public double DiscountAmount { get; set; }
        public int NewBillingWalletPoint { get; set; }

        public DateTime CreatedDate { get; set; }

        public DateTime? ModifiedDate { get; set; }
        public bool IsActive { get; set; }
        public bool? IsDeleted { get; set; }
        public int CreatedBy { get; set; }
        public int? ModifiedBy { get; set; }
       
    }
    public class CreateCustomer
    {
        public int WarehouseId { get; set; }
        public string MobileNo { get; set; }
        public string ShippingAddress { get; set; }
        public string CustomerName { get; set; }
        public string RefNo { get; set; }

    }
    public class FreeBatchCodeList
    {
        public int ItemMultiMRPId { get; set; }
        public int Qty { get; set; }
        public long StockBatchMasterId { get; set; }
        public int WarehouseId { get; set; }
        public int OrderId { get; set; }
        public int OrderDetailsId { get; set; }
        public DateTime? ExpiryDate { get; set; }
    }
    public class WalletPointDC
    {
        public double OfferWalletConfig { get; set; }
        public double OfferWalletValue { get; set; }
        public double RetailerWalletPoint { get; set; }
        public double ConsumerWalletPoint { get; set; }

    }

    public class orderItemDiscountMappingDc
    {
        public int OrderId { get; set; }
        public int OrderDetailId { get; set; }
        public int OrderQty { get; set; }
        public int DispatchQty { get; set; }
        public double UnitPrice { get; set; }
        public double BillDiscountAmount { get; set; }
        public double WalletAmount { get; set; }

    }
    public class ExportStoreConfigDC
    {
        public string WarehouseName { get; set; }
        public int ItemMultiMRPId { get; set; }
        public string ItemName { get; set; }
        public string Typename { get; set; }
        public double Percentage { get; set; }
    }
}