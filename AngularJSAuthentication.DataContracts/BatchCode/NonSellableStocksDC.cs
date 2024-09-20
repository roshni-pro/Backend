using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.BatchCode
{
    public class NonSellableStocksAndClearanceStockDC
    {
        public int WarehouseId { get; set; }
        public string WarehouseName { get; set; }

        public string ItemName { get; set; }
        public string ItemNumber { get; set; }

        public long ItemMultiMRPId { get; set; }

        public double Inventory { get; set; }

        public int Id { get; set; }
        public string UnitofQuantity { get; set; }
        public double MRP { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public string PeopleFirstName { get; set; }
        public string CreatedBy { get; set; }
        public string Comment { get; set; }
        public string ABCClassification { get; set; }        
        public double APP { get; set; }
        public string StoreName { get; set; }
        public string BatchCode { get; set; }
        public int StockQty { get; set; }
        public DateTime? MFGDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public int? RemainShelfLifedays { get; set; }
        public double? RemainingShelfLife { get; set; }
        public string CategoryName { get; set; }
        public string SubcategoryName { get; set; }
        public string SubsubcategoryName { get; set; }
    }
    public class ClearanceStockDC
    {
        public int WarehouseId { get; set; }
        public string WarehouseName { get; set; }

        public string ItemName { get; set; }
        public string ItemNumber { get; set; }

        public long ItemMultiMRPId { get; set; }

        public double Inventory { get; set; }

        public int Id { get; set; }
        public string UnitofQuantity { get; set; }
        public double MRP { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public string PeopleFirstName { get; set; }
        public string CreatedBy { get; set; }
        public string Comment { get; set; }
        public string ABCClassification { get; set; }
        public double APP { get; set; }
        public string StoreName { get; set; }
        public string BatchCode { get; set; }
        public DateTime? MFGDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
    }
    public class GetNonSellableAndClearanceStockListReqDC
    {

        public int Skip { get; set; }
        public int Take { get; set; }
        public List<int> WarehouseIds { get; set; }
        public List<int> SubsubCategoryids { get; set; }
        public string StockType { get; set; }
        public int? ItemMultiMRPId { get; set; }
        public string ItemName { get; set; }
        

    }

    public class GetTotalNonSellableAndClearanceStockListReqDC
    {
     public List<NonSellableStocksAndClearanceStockDC> allRecords { get; set; }

     public int totalRecords { get; set; }
    }

    public class ExportNonSellableAndClearanceStockList
    {
        public List<int> WarehouseIds { get; set; }
        public List<int> SubsubCategoryids { get; set; }
        public string StockType { get; set; }
      
    }
    public class GetNonSellStkClearanceBrandDC
    {
        public string StockType { get; set; }
        public  List<int> warehouseids { get; set; }

    }

    public class NonSellStkClearanceBrandList
    {
        public int BrandNumber { get; set; }
        public string BrandName { get; set; }
    }

}