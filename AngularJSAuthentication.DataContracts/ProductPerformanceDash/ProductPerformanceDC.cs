using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.ProductPerformanceDash
{
    public class ZSMLeadDetailDc
    {
       
        public int value { get; set; }
        public string label { get; set; }
        public int RegionManagerId { get; set; }
        public string RegionName { get; set; }
    }
    public class ZSMPerformanceListDc
    {
        public long Id { get; set; }
        public string ItemNumber { get; set; }
        //public string Quadrant { get; set; }
        public int ItemMultiMRPid { get; set; }
        public string ItemName { get; set; }
        public double ASP { get; set; }
        public double SixMonthDispatch { get; set; }
        public int Median { get; set; }
        public int SystemForeCastValue { get; set; }
        public int CommitedForeCastValue { get; set; }
        public int totalcount { get; set; }
        public string Comment { get; set; }
        public string Status { get; set; }
    }
    public class ZSMPerformanceList
    {
        public List<ZSMPerformanceListDc> ZsmPerformanceList { get; set; }
        public int TotalRecord { get; set; }
    }

    public class QuadrantDetailPostDc
    {
        public long Id { get; set; }
        public double NewASP { get; set; }
        public double MinValue { get; set; }
        public double MaxValue { get; set; }
        public int CommitedForeCastValue { get; set; }
        public string BuyerRemark { get; set; }
        public string SalesRemark { get; set; }
        public double CaseSize { get; set; }
    }
    public class QuadrantPerformancesDc
    {
        public int warehouseId { get; set; }
        public int StoreId { get; set; }
        public List<int> BuyerIds { get; set; }
        public DateTime MonthDate { get; set; }
    }

    public class WarehouseQuadrantCustomerTypeDC
    {
        public List<int> WarehouseIDs { get; set; }
        public string Quadrant { get; set; }
        public string CustomerType { get; set; }
        public float? Margin { get; set; }
        public int skip { get; set; }
        public int take { get; set; }
    }
    public class SearchWarehouseQuadrantCustomerTypeDC
    {
        public long Id { get; set; }
        public int WarehouseId { get; set; }
        public float MinMarginPercent { get; set; }
        public string WarehouseName { get; set; }
        public string Quadrant { get; set; }
        public string StoreName { get; set; }
        public string customerType { get; set; }
        public int TotalRecords { get; set; }
    }
    public class WarehouseQuadrantMarginExport
    {
        public int WarehouseId { get; set; }
        public string WarehouseName { get; set; }
        public float MinMarginPercent { get; set; }

        public string Quadrant { get; set; }
        public string StoreName { get; set; }
        public string customerType { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
    }
    public class QuadrantPerformanceItemListDC
    {
        public long Id { get; set; }
        public int Warehouseid { get; set; }
        public int Storeid { get; set; }
        public int ItemMultiMRPid { get; set; }
        public string ItemNumber { get; set; }
        public string ItemName { get; set; }
        public double CaseSize { get; set; }
        public int Median { get; set; }
        public int MedianInCase { get; set; }

        public int CommitedForeCastValue { get; set; }
        public int CommitedForeCastInCase { get; set; }

        public int SystemForeCastValue { get; set; }
        public int SystemForeCastInCase { get; set; }
        public Double ASP { get; set; }
        public Double NewASP { get; set; }
        public Double MinValue { get; set; }
        public Double MinValueInCase { get; set; }

        public Double MaxValue { get; set; }
        public Double MaxValueInCase { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Status { get; set; }
        public double CurrentDispatch { get; set; }
        public double SixMonthDispatch { get; set; }
        public double AchievmentPercentage { get; set; }
        public double DeviationPercentage { get; set; }
        public bool IsBuyerEdit { get; set; }
        public bool IsHubLeadEdit { get; set; }
        public string MultiBuyerComents { get; set; }

        public string MultiSalesComents { get; set; }

        public string BuyerRemark { get; set; }

        public string SalesRemark { get; set; }
    }

    public class QuadrantPerformancesItemDc
    {
        public int warehouseId { get; set; }
        public int StoreId { get; set; }
        public List<int> BrandIds { get; set; }
        public DateTime MonthDate { get; set; }
        public string Status { get; set; }
    }

    public class ExportItemRequestDc
    {
        public int warehouseId { get; set; }
        public int StoreId { get; set; }
        public List<int> BrandIds { get; set; }
        public DateTime MonthDate { get; set; }
        public string Status { get; set; }
    }

    public class WarehoouseQuadrantBuyerList
    {
        public int BuyerId { get; set; }
        public string BuyerName { get; set; }
    }
    public class Getproductsalesappdashboard
    {
        public string Quadrant { get; set; }
        public long QuadrantId { set; get; }
        public double targetValue { get; set; }
        public double Salesforecast { get; set; }
        public double Mtdsales { get; set; }
        public double Acheivement { get; set; }
        public double totalDispatchValue { get; set; }
        public double totalPlannedValue { get; set; }
        public double totalmtdsales { get; set; }
        public double totalsalesforecast { get; set; }
        public double TotalAcheivement { get; set; }
        public double TotalDeviation { get; set; }
        public double Deviation { get; set; }
    }
    public class Getproductsalespayload
    {
        public int WarehouseId { get; set; }
        public int peopleId { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }
        public string Key { get; set; }
        public bool IsCase { get; set; }

    }

    public class WarehouseQuadrantStoreList
    {
        public long StoreId { get; set; }
        public string StoreName { get; set; }
    }
    public class BrandList
    {
        public int SubsubCategoryid { get; set; }
        public string SubsubcategoryName { get; set; }
    }
    public class Getproductsalesitempayload
    {
        public int warehouseId { get; set; }
        public int peopleId { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }
        public string Key { get; set; }
        public bool IsCase { get; set; }
        public List<int> BrandIds { get; set; }

    }

    public class GetproductPerformanceDashboard
    {
        public List<Getproductsalesappitemdashboard> Items { get; set; }
        public double totalmtdsales { get; set; }
        public double totalsalesforecast { get; set; }
        public double TotalAcheivement { get; set; }
        public double totaldeviation { get; set; }
        public int TotalCount { get; set; }

    }

    public class Getproductsalesappitemdashboard
    {

        public string ItemNumber { get; set; }
        public string itemName { get; set; }
        public double Salesforecast { get; set; }
        public double Mtdsales { get; set; }
        public double Acheivement { get; set; }
        public double ASP { get; set; }
        public double deviation { get; set; }


    }
    public class SBForcastConfigDC
    {
        public long Id { get; set; }
        public int FromDay { get; set; }
        public int ToDay { get; set; }
        public bool IsAnytime { get; set; }
        public bool IsSalesForecast { get; set; }
    }
    public class QuadrantDetailDC
    {
        public long Id { get; set; }
        public int Warehouseid { get; set; }
        public int MrpId { get; set; }
        public string ItemNumber { get; set; }
        public int ItemMultiMRPid { get; set; }
        public int CommitedForeCastValue { get; set; }
        public string Status { get; set; }
        public string Comment { get; set; }
        public int SystemForeCastValue { get; set; }
        public double ASP { get; set; }
        public int SystemSuggestedQty { get; set; }
        public double MinValue { get; set; }
        public double MaxValue { get; set; }
        public int CurrentDispatch { get; set; }
        public int OverAllMedian { get; set; }
        public string itemName { get; set; }
        public int PurchaseMOQ { get; set; }
    }
    public class GetWarehouseQuadrantItemspayload
    {
        public List<int> Brandids { get; set; }
        public int warehouseid { get; set; }
        public long QuadrantId { get; set; }
        public DateTime monthdate { get; set; }
        public int StoreId { get; set; }
    }
    public class GetQuadrantPerformanceCurentDispatchListDC
    {
        public long QuadrantId { get; set; }
        public int ItemMultiMRPId { get; set; }
        public string Quadrant { get; set; }
    }

    public class ElasticGetQuadrantPerformanceCurentDispatch
    {
        public string itemnumber { get; set; }
        public int ordqty { get; set; }
    }

    public class WarehouseQuadrantCurentDispatch
    {
        public double Value { get; set; }
        public string Quadrant { get; set; }
    }


    public class WarehouseQuadrantSearchList
    {
        public long QuadrantId { get; set; }
        public string Quadrant { get; set; }
        public double QuadrantPer { get; set; }
        public double TotalQuadrantValue { get; set; }
        public int calValue { get; set; }
        public double CuurentDispatchValue { get; set; }
        public double AchievmentPercentage { get; set; }
        public double DeviationPercentage { get; set; }
        public double totalDispatchValue { get; set; }
        public double totalPlannedValue { get; set; }

    }

    public class GetQuadrantItemASP
    {
        public int itemmultimrpid { get; set; }
        public int WarehouseId { get; set; }
        public double ASP { get; set; }
    }
    public class GetQuadrantitemHistory
    {
        public string BuyerRemark { get; set; }
        public string SalesRemark { get; set; }
        public DateTime CreatedDate { get; set; }
        public string CreatedBy { get; set; }
        public int Forast { get; set; }
        public double ASP { get; set; }
        public double CaseSize { get; set; }
    }
    public class GetQuadrantItemforSearchListDc
    {

        public string ItemNumber { get; set; }
        public string itemname { get; set; }
        public int CommitedForeCastValue { get; set; }
        public double PurchaseMOQ { get; set; }
        public double NewASP { get; set; }
        public double MTD { get; set; }
        public string MTDStatus { get; set; }
        public int SalesForecast { get;set; }
        public double MOQ { get; set; }

    }

    public class GetQuadrantItemsalesASP
    {
        public int itemmultimrpid { get; set; }
        public int WarehouseId { get; set; }
        public double ASP { get; set; }
        public double MOQ { get; set; }
    }
    public class GetSalesQuadrant
    {
        public long Id { get; set; }
        public string Quadrant { get; set; }
    }
    public class SampleFileDC
    {
        public string ItemNumber { get; set; }
        public int ItemMultiMRPId { get; set; }
        public double MinQty { get; set; }
        public double MaxQty { get; set; }
        public string Warehouse { get; set; }
        public string Store { get; set; }
    }

    public class ExpoertAllQuadrantPerformanceItemListDC
    {
        public long ID { get; set; }
        public int WarehouseId { get; set; }
        public string WarehouseName { get; set; }
        public string ItemNumber { get; set; }
        //public int ItemMultiMRPId { get; set; }
        public string ItemName { get; set; }
        public string Status { get; set; }
        public double MOQ { get; set; }
        //public double CaseSize { get; set; }
        public double MRP { get; set; }
        public double ASP { get; set; }
        public double NewASP { get; set; }
        public string BuyerRemark { get; set; }
        public int Median { get; set; }
        public int SystemForecast { get; set; }
        public double MinValue { get; set; }
        public double MaxValue { get; set; }
        public int SalesForecast { get; set; }
        public string SalesRemark { get; set; }
        public double MTD { get; set; }
        public double AchievmentPercentage { get; set; }
        public double DeviationPercentage { get; set; }
        public string MultiBuyerComents { get; set; }
        public string MultiSalesComents { get; set; }
        public string CFRStatus { get; set; }
        public string MTDStatus { get; set; }


    }

    public class warehouselist
    {
        public int value { get; set; }
        public string label { get; set; }
    }

    public class QuadrantDetailHistoryDC
    {
        public long QuadrantDetailId { get; set; }
        public string BuyerRemark { get; set; }
        public string SalesRemark { get; set; }
        public int Forcast { get; set; }
        public double ASP { get; set; }
        public double CaseSize { get; set; }
    }

  
}
