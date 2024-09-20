using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.ClearanceStockNonSaleable
{
    public class ClearanceNonSellableDc
    {
        public long Id { get; set; }    //ClearanceNonSaleablePrepareItems table  p id
        public string OrderType { get; set; }
        public int Warehouseid { get; set; }
        public double UnitPrice { get; set; }
        public int Quantity { get; set; }
        public int ItemMultiMRPId { get; set; }
        public long StockBatchMasterId { get; set; }
        //public long StoreId { get; set; }
        public int BuyerId { get; set; }


    }

    /*public class UpdateStoreWiseData
    {
        public long StoreId { get; set; }
        public string StockType { get; set; }
        public double ShelfLifeFrom { get; set; }
        public double ShelfLifeTo { get; set; }

    }*/

    public class GetClearanceStockMovementOrder
    {
        public long Id { get; set; }
        public string WarehouseName { get; set; }
        public string Status { get; set; }
        public DateTime CreatedDate { get; set; }
        public double Amount { get; set; }   
        public string OrderType { get; set;}



    }
    public class ClearanceStockMovementOrderList
    {
        public int TotalRecords { get; set; }
        public List<GetClearanceStockMovementOrder> GetClearanceStockMovementOrder { get; set; }
    }
    public class GetClearanceStockMovementOrderDC
    {
        public int Warehouseid { get; set; }
        public string OrderType { get; set; }
        public string Status { get; set; }
        public string keyword { get; set; }
        public int skip { get; set; }
        public int take { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public bool IsExport { get; set; }

    }

    public class GetClearanceOrderItemDC
    {
        public string ItemNumber { get; set; }
        public string ItemName { get; set; }
        public int ItemMultiMRPId { get; set; }
        public int TotalQuantity { get; set; }
        public double UnitPrice { get; set; }
       
        public List<GetClearanceOrderItemDetailDC> GetClearanceOrderItemDetailDCs { get; set; }

    }

    public class GetClearanceOrderItemDetailDC
    {
        public long Id { get; set; }
        public string BatchCode { get; set; }
        public int? RemainShelfLifeDays { get; set; }
        public double? CurrentShelfLife { get; set; }
        public int AvailableQuantity { get; set; }
        public int ItemMultiMRPId { get; set; }
        public long StockBatchMasterId { get; set; } 

    }


    public class GetItemListDC
    {
        public string ItemName { get; set; }
        public int ItemMultiMRPId { get; set; }
        public int WarehouseId { get; set; }
        public long StoreId { get; set; }
    }
    public class GetClearanceLiveItemStatusDC
    {
        public int WarehouseId { get; set; }
        public string WarehouseName { get; set; }
        public int ItemMultiMRPId { get; set; }
        public string itemname { get; set; }
        public int AvailableQty { get; set; }
        public int TotalQty { get; set; }
        public double UnitPrice { get; set; }
        public int IsActive { get; set; }
        public long Id { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ExpiryDate { get; set; }
        public DateTime MFGDate { get; set; }
        public string BatchCode { get; set; }
        public double ShelfLifeLeft { get; set; }

    }

  
    public class GetCleNonSaleableMovementOrderDC
    {
       
        public long ClearanceStockMovementOrderMasterId { get; set; }
        public double Amount { get; set; }
        public string StoreName { get; set; }
        public int ItemMultiMRPId { get; set; }
        public string ItemNumbr { get; set; }
        public string itemname { get; set; }
        public double UnitPrice { get; set; }
        public double MRP { get; set; }

        public List<GetCleNonSaleableMovementOrderDetailsDC> GetCleNonSaleableMovementOrderDetailsDCs { get; set; }



    }
    public class GetCleNonSaleableMovementOrderDetailsDC
    {
        public long Detailid { get; set; }
        public int ItemMultiMRPId { get; set; }
        public int AvailableQuantity { get; set; }
        public int Quantity { get; set; }
        public int OrderQuantity { get; set; }
        public string BatchCode { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public DateTime? MFGDate { get; set; }
        public int RemainShelfLifedays { get; set; }
        public double CurrentShelfLife { get; set; }

        

    }

    //-------------------//------------------------//---------------------------//
    public class CategoryListDc
    {
        public int CategoryID { get; set; }
        public string CategoryName { get; set; }
    }
    public class UpdateList
    {
        public int CategoryId { get; set; }
        public int SubCategoryId { get; set; }
        public int BrandId { get; set; }
        public string ItemNumber { get; set; }
       // public int Itemid { get; set; }
        public double ClearanceShelfLifeFrom { get; set; }
        public double ClearanceShelfLifeTo { get; set; }
        public double NonSellShelfLifeFrom { get; set; }
        public double NonSellShelfLifeTo { get; set; }
    }
  
    public class ApproveShelfLifeDc
    {
        public int Id { get; set; }
        public string Status { get; set; }  //Approved ,  Reject
        public string comment { get; set; }
    }
    public class ApprovePageSearchDc
    {
        public long Id { get; set; }
        public int CategoryId { get; set; }
        public int SubCategoryId { get; set; }
        public int BrandId { get; set; }
        public string ItemNumber { get; set; }
        public string CategoryName { get; set; }
        public string SubCategoryName { get; set; }
        public string BrandName { get; set; }
        public string Itemname { get; set; }
        //public int ItemId { get; set; }
        public string Status { get; set; }
        public string RejectComment { get; set; }
        public string RequestedBy { get; set; }
      
    }
    public class ApprovePageSearchDcList
    { 
        public List<ApprovePageSearchDc> ApprovePageSearchList { get; set; }
        public int TotalRecords { get; set; }
    }

     public class ClearanceNonConfigSearch
    {
        public long Id { get; set; }
        public string CategoryName { get; set; }
        public string BrandName { get; set; }
        public string ItemNumber { get; set; }
        public string Itemname { get; set; }
        //public int ItemId { get; set; }
        public double ClearanceShelfLifeFrom { get; set; }
        public double ClearanceShelfLifeTo { get; set; }
        public double NonSellShelfLifeFrom { get; set; }
        public double NonSellShelfLifeTo { get; set; }
    }
    public class ApprovePagePopupDc
    {
        public int CategoryId { get; set; }
        public int SubCategoryId { get; set; }
        public int BrandId { get; set; }
        public string ItemNumber { get; set; }
        //public int Itemid { get; set; }
        public string status { get; set; }

    }
    public class SearchitemOnConfigurationPage
    {
        public int? CategoryId { get; set; }
        public int? SubCategoryId { get; set; }
        public int? BrandId { get; set; }
        public List<string> ItemNumbers { get; set; }

    }
   
    public class ApprovePagePopupList
    {
        public double ReqClearanceShelfLifeFrom { get; set; }
        public double ReqClearanceShelfLifeTo { get; set; }
        public double ReqNonSellShelfLifeFrom { get; set; }
        public double ReqNonSellShelfLifeTo { get; set; }
        public string RejectComment { get; set; }
       
    }
    public class BuyerNameListDc
    {
        public int BuyerId { get; set; }
        public string BuyerName { get; set; }
    }
    public class WarehouseByUserDc
    {
        public int WarehouseId { get; set; }
        public string WarehouseName { get; set; }
        

    }
    public class StoreNameListDc
    {
        public long StoreId { get; set; }
        public string StoreName { get; set; }
        public double Discount { get; set; }
    }


    public class ClearanceOrderPickerList
    {
        public int TotalRecords { get; set; }
        public List<ClearanceOrderPickerListDC> ClearanceOrderPickerListDCs { get; set; }
    }

    public class ClearanceOrderPickerListDC
    {
        public long OrderNumber { get; set; }
        public string WareHouseName { get; set; }
        public string ItemNumber { get; set; }
        public int Quantity { get; set; }
        public double? ValueAmount { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? RajectedDateTime { get; set; }
        public string RajectedBy { get; set; }

        public string RajectionComment { get; set; }

    }



    public class SearchClearanceOrderPickerDC
    {
        public int WareHouseId { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string ItemName { get; set; }
        public string PickerStatus { get; set; }

        public int skip { get; set; }
        public int take { get; set; }
    }

    public class UpdateStatusDC
    {
        public long Id { get; set; }
        public string Status { get; set; }
        public string comment { get; set; }
        public int Quantity { get; set; }
       
    }


    public class getClPendingOrd
    {
        public string WarehouseName { get; set; }
        public long OrderId { get; set; }
        public DateTime OrderDate { get; set; }
        public long OrderValue { get; set; }
        
        public string BuyerName{ get; set; }
        public string Email { get; set; }


    }


}
