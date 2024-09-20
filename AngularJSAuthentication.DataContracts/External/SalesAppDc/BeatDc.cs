using AngularJSAuthentication.DataContracts.Mongo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.External.SalesAppDc
{
    //    public class TopSKUsItemDc
    //    {
    //        public int ItemId { get; set; }
    //        public string itemName { get; set; }
    //        public double UnitPrice { get; set; }
    //        public double Margin { get; set; }
    //        public int StoreId { get; set; }
    //        public int WarehouseId { get; set; }
    //        public string WarehouseName { get; set; }
    //    }
    public class APIResponse
    {
        public string Message { get; set; }
        public bool Status { get; set; }
        public object Data { get; set; }
    }
    public class uploadtargetDTOdc
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsMonth { get; set; }
        public int warehouseid { get; set; }
        public int storeid { get; set; }
    }
    public class ElascticTargetMTD
    {
        public double dispatchamt { get; set; }
        public int executiveid { get; set; }
        public int custCount { get; set; }
        public int storeid { get; set; }
        public int clusterid { get; set; }
    }
    public class ElasticTargetMAC
    {
        public int custCount { get; set; }
        public int storeid { get; set; }
        public int executiveid { get; set; }
        public int clusterid { get; set; }
    }
    public class DownloadTargetDC
    {
        public int ExecutiveId { get; set; }
        public string SalesPerson { get; set; }
        public string Cluster { get; set; }
        public string Store { get; set; }
        public double Target { get; set; }
        public double IncentiveAmount { get; set; }
        public double? Achievement { get; set; }
        public string SalesKpi { get; set; }
        public int WarehouseId { get; set; }
        public long StoreId { get; set; }
        public int ClusterId { get; set; }

    }
    public class DownloadTargetDataDC
    {
        public string SalesPerson { get; set; }
        public string Cluster { get; set; }
        public string Store { get; set; }
        public double Target { get; set; }
        public double IncentiveAmount { get; set; }
        public double? Achievement { get; set; }
        public string SalesKpi { get; set; }
    }
    public class MainDashboardDc
    {
        public TodaySaleDc TodaySaleDcs { get; set; }
        public MTDSaleDc MTDSaleDcs { get; set; }
        public YesterdaySaleDC YesterdaySaleDCs { get; set; }
    }

    public class BeatCustomers
    {
        public int Customertype { get; set; }
        public List<int> customers { get; set; }
    }
    public class TodaySaleDc
    {
        public double TodaySales { get; set; }
        public double TodayTargetSales { get; set; }
        public int TodayTotalVisitBeatCustomer { get; set; }
        public int TodayTotalBeatCustomer { get; set; }
    }
    public class MTDSaleDc
    {
        public double MTDSales { get; set; }
        public double MTDTargetSales { get; set; }
        public int MTDTotalVisitBeatCustomer { get; set; }
        public int MTDTotalBeatCustomer { get; set; }
    }
    public class YesterdaySaleDC
    {
        public double YesterdaySales { get; set; }
        public double YesterdayTargetSales { get; set; }
        public int YesterdayTotalVisitBeatCustomer { get; set; }
        public int YesterdayTotalBeatCustomer { get; set; }
    }
    public class BeatSalesDc
    {
        public double TodaySales { get; set; }
        public double MTDSales { get; set; }
        public double YesterdaySales { get; set; }
        public int TotalProductiveCustomer { get; set; }
    }
    public class DashboardDc
    {
        public TodayDashboardDc TodayDashboardDcs { get; set; }
        public MTDDashboardDc MTDDashboardDcs { get; set; }
        public YesterdayDashboardDc YesterdayDashboardDcs { get; set; }
    }
    public class TargetSalesdc
    {
        public double MTDTargetSales { get; set; }
        public double TodayTargetSales { get; set; }
        public double YesterdayTargetSales { get; set; }
    }
    public class TargetDashboardDc
    {
        public BeatSalesDc sales { get; set; }
        public TargetSalesdc TargetSales { get; set; }
    }
    public class BeatCustomerDTOdc
    {
        public int GroupId { get; set; }
        public int SubGroupId { get; set; }
        public int PeopleId { get; set; }
        public int OrderDays { get; set; }
        //public List<int> VisitDays { get; set; }
        public string Day { get; set; }
        public string KeyValue { get; set; }
        public int warehouseId { get; set; }
        public int skip { get; set; }
        public int take { get; set; }
        public bool BeatEdit { get; set; }
        public bool IsPagination { get; set; }
        public double lat { get; set; }
        public double lg { get; set; }
        public string APIType { get; set; }
    }
    public class GroupMetricsDTOdc
    {
        public int CustomerId { get; set; }
        public int PeopleId { get; set; }
        public int flag { get; set; }
        public int warehouseid { get; set; }
    }
    public class TodayDashboardDc
    {
        public double TodayBeatSales { get; set; }
        public int TodayBeatOrder { get; set; }
        public double TodayExtraSales { get; set; }
        public double TodayTotalSales { get; set; }
        public int TodayProductiveCall { get; set; }
        public int TodayVisitPlanned { get; set; }
        public int TodayTotalCall { get; set; }
        public double TodayOutletCoverage { get; set; }
        public double TodayStrikeRate { get; set; }
        public int TodayExtraVisit { get; set; }
        public int TodayExtraCall { get; set; }
        public int TodayExtraOrder { get; set; }
        public int TodaySupperOrder { get; set; }
        public int TodayPerfactOrder { get; set; }
        public int PhoneOrder { get; set; }
        public int Eco { get; set; }
        public double AvgOrderValue { get; set; }
        public double AvgLineItem { get; set; }
    }
    public class MTDDashboardDc
    {
        public double MTDBeatSales { get; set; }
        public int MTDBeatOrder { get; set; }
        public double MTDExtraSales { get; set; }
        public double MTDTotalSales { get; set; }
        public int MTDProductiveCall { get; set; }
        public int MTDVisitPlanned { get; set; }
        public int MTDTotalCall { get; set; }
        public double MTDOutletCoverage { get; set; }
        public double MTDStrikeRate { get; set; }
        public int MTDExtraVisit { get; set; }
        public int MTDExtraCall { get; set; }
        public int MTDExtraOrder { get; set; }
        public double MTDAvgOrderValue { get; set; }
        public double MTDAvgLineItem { get; set; }
        public double MTDEco { get; set; }
        public int MTDSupperOrder { get; set; }
        public int MTDPerfactOrder { get; set; }
    }
    public class YesterdayDashboardDc
    {
        public double YesterdayBeatSales { get; set; }
        public int YesterdayBeatOrder { get; set; }
        public double YesterdayExtraSales { get; set; }
        public double YesterdayTotalSales { get; set; }
        public int YesterdayProductiveCall { get; set; }
        public int YesterdayVisitPlanned { get; set; }
        public int YesterdayTotalCall { get; set; }
        public double YesterdayOutletCoverage { get; set; }
        public double YesterdayStrikeRate { get; set; }
        public int YesterdayExtraVisit { get; set; }
        public int YesterdayExtraCall { get; set; }
        public int YesterdayExtraOrder { get; set; }
        public int YesterdaySupperOrder { get; set; }
        public int YesterdayPerfactOrder { get; set; }
    }
    //public class testdc
    //{
    //    public double BeatSales { get; set; }
    //    public int BeatOrder { get; set; }
    //    public double ExtraSales { get; set; }
    //    public double TotalSales { get; set; }
    //    public int ProductiveCall { get; set; }
    //    //public int VisitPlanned { get; set; }
    //   // public int TotalCall { get; set; }
    //   // public double OutletCoverage { get; set; }
    //    //public double StrikeRate { get; set; }
    //    //public int ExtraVisit { get; set; }
    //    public int ExtraCall { get; set; }
    //    public int ExtraOrder { get; set; }
    //    //public double AvgOrderValue { get; set; }
    //    //public double AvgLineItem { get; set; }
    //    //public int Eco { get; set; }
    //    //public int PhoneOrder { get; set; }
    //}
    public class DashboardDetailDc
    {
        public double BeatSales { get; set; }
        public int BeatOrder { get; set; }
        public string StoreName { get; set; }
        public string ClusterName { get; set; }
        public string ChannelName { get; set; }
        public double ExtraSales { get; set; }
        public double TotalSales { get; set; }
        public int ProductiveCall { get; set; }
        public int VisitPlanned { get; set; }
        public int TotalCall { get; set; }
        public double OutletCoverage { get; set; }
        public double StrikeRate { get; set; }
        public int ExtraVisit { get; set; }
        public int ExtraCall { get; set; }
        public int ExtraOrder { get; set; }
        public double AvgOrderValue { get; set; }
        public double AvgLineItem { get; set; }
        public int Eco { get; set; }
        public int SuccessPhoneOrder { get; set; } // order placed by phone order
        public int PerfactOrder { get; set; }
        public int SupperOrder { get; set; }
        public string WarehouseName { get; set; }
    }


    public class TodayDSRDc
    {
        public string WarehouseName { get; set; }
        public int PeopleId { get; set; }
        public string storeName { get; set; }
        public string ClusterName { get; set; }
        public string PeopleName { get; set; }
        public int VisitPlanned { get; set; }
        public double BeatSales { get; set; }
        public double ExtraSales { get; set; }
        public double TotalSales { get; set; }
        public int ProductiveCall { get; set; }
        public int ExtraCall { get; set; }
        public int BeatOrder { get; set; }
        public int ExtraOrder { get; set; }
        public int PerfactOrder { get; set; }
        public int SupperOrder { get; set; }
        public int ExtraVisit { get; set; }
        public int TotalCall { get; set; }
        public DateTime? BeatIn { get; set; }
        public DateTime? BeatOut { get; set; }

    }
    public class StoreDC
    {
        public int StoreId { get; set; }
        public string StoreName { get; set; }
        public List<GroupDc> GroupDcs { get; set; }
    }
    public class GroupDc
    {
        public long GroupId { get; set; }
        public string GroupName { get; set; }
    }
    public class SalesGroupDc
    {
        public int CustomerId { get; set; }
        public long id { get; set; }
        public string MainGroup { get; set; }
        public string GroupName { get; set; }
        public int StoreId { get; set; }
        public int CreatedBy { get; set; }
        public bool isEdit { get; set; }
    }
    public class MyBeatDC
    {
        public object ObjectId { get; set; }
        public int CustomerId { get; set; }
        public string Skcode { get; set; }
        public double lat { get; set; }
        public double lg { get; set; }
        public string ShopName { get; set; }
        public string ShippingAddress { get; set; }
        public string Mobile { get; set; }
        public DateTime? LastOrderDate { get; set; }
        public int LastOrderDaysAgo { get; set; }
        public bool IsCompany { get; set; }
        public List<SalesGroupDc> SalesGroupDcs { get; set; }
        public string CustomerVerify { get; set; }
        public bool IsVisited { get; set; }
        public double MTDSales { get; set; }
        public int OrderCount { get; set; }
        public int PendingOrderCount { get; set; }
        public string Day { get; set; }
        public string CustomerName { get; set; }
        public int? WarehouseId { get; set; }
        public bool Active { get; set; }
        public bool IsKPP { get; set; }
        public bool IsBeatEdit { get; set; }
        public double Distance { get; set; }
        public bool IsReschedule { get; set; }
        public List<InsertCustomerRemarksDC> CustomerRemarksDCs { get; set; }
        public DateTime? CheckIn { get; set; }
        public bool IsCustomerUpdate { get; set; }
        public bool IsVerifyRequestPending { get; set; }
        public bool IsCustomerProfileEdit { get; set; }
        public int LastVisitDaysAgo { get; set; }
        public bool IsBeat { get; set; }
        public string CRMTag { get; set; }
        public string CustomerType { get; set; }
    }
    public class MyBeatCustomerDc
    {
        public List<MyBeatDC> MyBeatDCs { get; set; }
        public List<MyBeatDC> NoVisitMyBeatDCs { get; set; }
        public int VisitCount { get; set; }
        public int NonVisitCount { get; set; }
        public int totalRecord { get; set; }
        public bool IsBeatEdit { get; set; }
        public int Reschedule { get; set; }
    }
    public class BeatCustomerOrderDc
    {
        public int CustomerId { get; set; }
        public DateTime? LastOrderDate { get; set; }
        public int LastOrderDaysAgo { get; set; }
        public string CustomerVerify { get; set; }
        public double lat { get; set; }
        public double lg { get; set; }
        public string ShippingAddress { get; set; }
        public double Distance { get; set; }
        public DateTime? CheckIn { get; set; }
        public bool IsCustomerUpdate { get; set; }
        public bool IsVerifyRequestPending { get; set; }
        public bool IsCustomerProfileEdit { get; set; }
        public string CustomerType { get; set; }
    }
    public class FocusBrandDC
    {
        public string SubsubcategoryName { get; set; }
        public string HindiName { get; set; }
        public double Amount { get; set; }
        public int CustomerTarget { get; set; }
        public int AchieveCustomerCount { get; set; }
    }
    public class ProductPerformanceDC
    {
        public string SubcategoryName { get; set; }
        public string HindiName { get; set; }
        public int Amount { get; set; }
        public int CustomerTarget { get; set; }
        public int TotalOrder { get; set; }
        public string itemname { get; set; }
    }
    public class ProductPerformanceFilter
    {
        public int clusterId { get; set; }
        public int peopleid { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime Todate { get; set; }
        public int Flag { get; set; }
        public int ObjectId { get; set; }
        public int Skip { get; set; }
        public int Take { get; set; }
        public int WarehouseId { get; set; }
    }
    public class SKUItemListDC
    {
        public int ItemId { get; set; }
        public int WarehouseId { get; set; }
        //public string itemName { get; set; }
        public double UnitPrice { get; set; }
        public double Margin { get; set; }
        public long StoreId { get; set; }
        public string StoreName { get; set; }
        public string itemname { get; set; }
        public double MRP { get; set; }
        public string Number { get; set; }
        public bool IsActive { get; set; }
    }
    public class PromotionalSKUItemListDC
    {
        public int ItemId { get; set; }
        public long StoreId { get; set; }
        public string StoreName { get; set; }
        public int WarehouseId { get; set; }
        public double SellingPrice { get; set; }
        public double Margin { get; set; }
        public string ItemName { get; set; }
        public double MRP { get; set; }
        public string ItemNumber { get; set; }
        public string ActiveStatus { get; set; }
    }
    public class InsertSKUItemDC
    {
        public int ItemId { get; set; }
        public string itemName { get; set; }
        public double UnitPrice { get; set; }
        public double Margin { get; set; }
    }
    public class GetTopHighMarginSkuItemListDC
    {
        public int ItemId { get; set; }
        public string itemName { get; set; }
        public double UnitPrice { get; set; }
        public double Margin { get; set; }
    }
    public class TopSKUsItemDc
    {
        public int Id { get; set; }
        public int ItemId { get; set; }
        public double UnitPrice { get; set; }
        public double Margin { get; set; }
        public string itemName { get; set; }
        public int StoreId { get; set; }
        public int WarehouseId { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public bool IsActive { get; set; }
        public bool? IsDeleted { get; set; }
        public int CreatedBy { get; set; }
        public int? ModifiedBy { get; set; }
        public string WarehouseName { get; set; }
        public int SequenceNo { get; set; }
    }
    public class TopPromotionalSKUsItemDc
    {
        public int Id { get; set; }
        public int ItemId { get; set; }
        public double UnitPrice { get; set; }
        public double Margin { get; set; }
        public string itemName { get; set; }
        public int WarehouseId { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public bool IsActive { get; set; }
        public bool? IsDeleted { get; set; }
        public int CreatedBy { get; set; }
        public int? ModifiedBy { get; set; }
        public string WarehouseName { get; set; }
        public int SequenceNo { get; set; }
    }
    public class TopSKUsItem_Dc
    {
        public long Id { get; set; }
        public int itemId { get; set; }
        public double UnitPrice { get; set; }
        public double Margin { get; set; }
        public string itemName { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public bool IsActive { get; set; }
        public bool? IsDeleted { get; set; }
        public int CreatedBy { get; set; }
        public int? ModifiedBy { get; set; }
    }
    public class SalesGroupDC
    {
        public string GroupName { get; set; }
        public int PeopleId { get; set; }
        public int storeid { get; set; } //0 company,-1 salesperson, -2 All, 0 < store
        public bool IsActive { get; set; }
        public bool? IsDeleted { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public int ModifiedBy { get; set; }
        public DateTime ModifiedDate { get; set; }
        public int IsNewGroup { get; set; } // 0-new group, 1 exist group
        public int customerid { get; set; }
        public int GroupId { get; set; }
    }
    public class SalesMyGroupDC
    {
        public long id { get; set; }
        public string GroupName { get; set; }
        public bool isEdit { get; set; }
    }
    public class GroupCustRemoveDc
    {
        public int PeopleId { get; set; }
        public int GroupId { get; set; }
        public int CustomerID { get; set; }
    }
    public class SalesGroupCustomerDC
    {
        public int GroupId { get; set; }
        public int PeopleId { get; set; }
        public int CustomerID { get; set; }
        public string Name { get; set; }
        public bool IsActive { get; set; }
        public bool? IsDeleted { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public int ModifiedBy { get; set; }
        public DateTime ModifiedDate { get; set; }
    }
    public class RescheduleBeatDC
    {
        public int CustomerID { get; set; }
        public int ExecutiveId { get; set; }
        public DateTime StartDate { get; set; }
        public string Day { get; set; }
        public int StoreID { get; set; }
        public bool? IsReschedule { get; set; }
        public bool IsActive { get; set; }
        public bool? IsDeleted { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public int ModifiedBy { get; set; }
        public DateTime ModifiedDate { get; set; }
    }
    public class PerformanceDc
    {
        public int ObjectId { get; set; }
        public string Name { get; set; }
        public int Customers { get; set; }
        public int TotalOrder { get; set; }
        public double Amount { get; set; }
    }
    public class ClusterDc
    {
        public int ClusterId { get; set; }
        public string ClusterName { get; set; }
    }

    public class targetUploadDc
    {
        public int StoreId { get; set; }
        public int WarehouseId { get; set; }
        public int ClusterId { get; set; }
        public int SalesPersonId { get; set; }
        public int NewCustomer { get; set; }
        public int TotalVerifiedRetailers { get; set; }
        public int ActiveRetailers { get; set; }
        public double Sales { get; set; }
        public double Dispatch { get; set; }
        public double Cancellation { get; set; }
        public double QoQRetention { get; set; }
        public double CustomerPercentage { get; set; }
        public double ProductPercentage { get; set; }
    }
    public class TargetResponseDc
    {
        public bool Status { get; set; }
        public string msg { get; set; }
    }
    public class BeatEditDc
    {
        public int CustomerID { get; set; }
        public int ExecutiveId { get; set; }
        public DateTime StartDate { get; set; }
        public string Day { get; set; }
        public string Type { get; set; }
        public string SubType { get; set; }
        public int StoreID { get; set; }
        public bool? IsBeatEdit { get; set; }
        public bool IsActive { get; set; }
        public bool? IsDeleted { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public int ModifiedBy { get; set; }
        public DateTime ModifiedDate { get; set; }
    }
    public class DayWiseShopCountDc
    {
        public string Day { get; set; }
        public int ShopCount { get; set; }
    }


    public class SalesPersonTargetDc
    {
        public string Name { get; set; }
        public int ClusterId { get; set; }
        public double NumPer { get; set; }
        public int PeopleID { get; set; }
    }
    public class CheckInStatuDc
    {
        public int customerId { get; set; }
        public string SKCode { get; set; }
        public string ShopName { get; set; }
        public string CustomerType { get; set; }
    }

    public class PerformanceDashboardDc
    {
        public int NewCustomer { get; set; }
        public int VerifiedRetailer { get; set; }
        public int ActiveRetailers { get; set; }
        public double SalesValue { get; set; }
        public double MTDDispatchValue { get; set; }
        public double Cancellation { get; set; }
        public double QoQ { get; set; }
        public double CustomerPercentage { get; set; }
        public double ProductPercentage { get; set; }
    }
    public class PerformanceTargetDc
    {
        public string Name { get; set; }
        public double NumPer { get; set; }
        public int PeopleID { get; set; }
    }

    public class PerformanceTargetdata

    {
        public PerformanceTargetDc performanceTargetDcs { get; set; }
        public List<PerformanceDashboardDc> pfdata { get; set; }

    }
    public class TargetDataDc
    {
        public string HeadName { get; set; }
        public long HeadId { get; set; }
        public List<TargetList> TargetListData { get; set; }
    }

    public class TargetList
    {
        public int ExecutiveId { get; set; }
        public int ClusterId { get; set; }
        public long StoreId { get; set; }
        public int WarehouseId { get; set; }
        public string ClusterName { get; set; }
        public string StoreName { get; set; }
        public string ExecutiveName { get; set; }
        public double NumPer { get; set; }

    }
    public class ListData
    {
        public int ExecutiveId { get; set; }
        public int ClusterId { get; set; }
        public int StoreId { get; set; }
        public int WarehouseId { get; set; }
        public string ClusterName { get; set; }
        public string StoreName { get; set; }
        public string ExecutiveName { get; set; }
        public string HeadName { get; set; }
        public int HeadId { get; set; }
        public double NumPer { get; set; }
    }

    public class SalesKpiDataDC
    {
        //Salesperson	Cluster	WarehouseName	Store	Target	IncentiveAmount	SalesKpi

        public int Month { get; set; }
        public int Year { get; set; }
        public string Salesperson { get; set; }
        public string Cluster { get; set; }
        public string WarehouseName { get; set; }
        public string Store { get; set; }
        public string Channel { get; set; }
        public string SalesKpi { get; set; }
        public double Target { get; set; }
        public double IncentiveAmount { get; set; }
    }

    public class CRMLevelBeatCustomerDc
    {
        public int WarehouseId { get; set; }
        public string WarehouseName { get; set; }
        public int Cityid { get; set; }
        public string SkCode { get; set; }
        public int CustomerId { get; set; }
        public int BrandCount { get; set; }
        public int OrderCount { get; set; }
        public double Volume { get; set; }
        public double KKvolume { get; set; }
        public int Selfordercount { get; set; }
        public int Salespersonordercount { get; set; }
        public bool IsActive { get; set; }
        public string Level { get; set; }
    }
    public class BeatCustomersCRMDc
    {
        public int Month { get; set; }
        public int Year { get; set; }
        public int OrderCount { get; set; }
        public int CustomerId { get; set; }
        public int BrandCount { get; set; }
        public int SelfOrderCount { get; set; }
        public double Volume { get; set; }
        public double KKvolume { get; set; }
        public int Level { get; set; }
        public DateTime CreatedDate { get; set; }
        public bool IsDeleted { get; set; }
        public int CreatedBy { get; set; }
        public int Salespersonordercount { get; set; }
        public string Skcode { get; set; }
    }

    public class WarehouseListDc
    {
        public int Wid { get; set; }
        public string WarehouseName { get; set; }
    }
    public class StoreDc
    {
        public int Storeid { get; set; }
        public string StoreName { get; set; }
    }
    public class PeopleDc
    {
        public int Peopleid { get; set; }
        public string PeopleName { get; set; }
    }

    public class ExecutiveAllBeatDc
    {
        public int Customerid { get; set; }
        public string day { get; set; }
    }
    public class SalesAllBeatDc
    {
        public int CustomerId { get; set; }
        public string day { get; set; }
        public DateTime CreateDate { get; set; }
    }
    public class BeatCustomerDc
    {
        public List<ExecutiveBeatCustomerDc> TodayBeatCustomerIds { get; set; }
        public List<ExecutiveBeatCustomerDc> MonthBeatCustomerIds { get; set; }
        public List<ExecutiveBeatCustomerDc> YesterdayBeatCustomerIds { get; set; }
    }
    public class ExecutiveBeatCustomerDc
    {
        public int Customerid { get; set; }
    }
    public class MonthBeatCustomerDc
    {
        public int Customerid { get; set; }
    }
    public class YesterdayBeatCustomerDc
    {
        public int Customerid { get; set; }
    }

    public class SalesDashboardExportData : SalesDashboardExportDataDc
    {
        public int SalesPersonId { get; set; }
        public int StoreId { get; set; }
    }
    public class SalesDashboardExportDataDc
    {
        public string EmpCode { get; set; }
        public string SalesPerson { get; set; }
        public string StoreName { get; set; }
        public string ClusterName { get; set; }
        public string ChannelName { get; set; }
        public int VisitPlanned { get; set; }
        public int TotalCall { get; set; }
        public int ProductiveCall { get; set; }
        public int BeatOrder { get; set; }
        public double BeatSales { get; set; }
        public int ExtraVisit { get; set; }
        public int ExtraCall { get; set; }
        public int ExtraOrder { get; set; }
        public double ExtraSales { get; set; }
        public double TotalSales { get; set; }
        public double? TodayTarget { get; set; }
        public double OutletCoverage { get; set; }
        public double StrikeRate { get; set; }
        public DateTime? CheckIn { get; set; }
        public DateTime? CheckOut { get; set; }
        //mtd fields
        //public int MtdVisitPlanned { get; set; }
        //public int MtdTotalCall { get; set; }
        //public int MtdProductiveCall { get; set; }
        //public int MtdBeatOrder { get; set; }
        //public double MtdBeatSales { get; set; }
        //public int MtdExtraVisit { get; set; }
        //public int MtdExtraCall { get; set; }
        //public int MtdExtraOrder { get; set; }
        //public double MtdExtraSales { get; set; }
        //public double MtdTotalSales { get; set; }
        //public int MtdMonthlyTarget { get; set; }
        //public int MtdRemainingMonthlyTarget { get; set; }
        //public double MtdOutletCoverage { get; set; }
        //public double MtdStrikeRate { get; set; }
        //public double MtdAverageOrderValue { get; set; }
        //public double MtdAverageLineItem { get; set; }
        //public double MtdECO { get; set; }


    }
    public class PerformanceDashboardMongoDc
    {
        public string SalesPerson { get; set; }
        public string StoreName { get; set; }
        public string ClusterName { get; set; }
        public string WarehouseName { get; set; }
        public int ActiveRetailers { get; set; }
        public int AchieveActiveRetailers { get; set; }
        public int VerifiedRetailer { get; set; }
        public int AchieveVerifiedRetailer { get; set; }
        public int NewCustomer { get; set; }
        public int AchieveNewCustomer { get; set; }
        public double SalesValue { get; set; }
        public double AchieveSalesValue { get; set; }
        public double MTDDispatchValue { get; set; }
        public double AchieveMTDDispatchValue { get; set; }
        public double Cancellation { get; set; }
        public double AchieveCancellation { get; set; }
        public double QoQ { get; set; }
        public double AchieveQoQ { get; set; }
        public double CustomerPercentage { get; set; }
        public double AchieveCustomerPercentage { get; set; }
        public double ProductPercentage { get; set; }
        public double AchieveProductPercentage { get; set; }
    }
    public class TodayDashboardDataInternalDc
    {
        public string ExecutiveName { get; set; }
        public string StoreName { get; set; }
        public string ClusterName { get; set; }
        public double TodayBeatSales { get; set; }
        public int TodayBeatOrder { get; set; }
        public double TodayExtraSales { get; set; }
        public double TodayTotalSales { get; set; }
        public int TodayProductiveCall { get; set; }
        public int TodayVisitPlanned { get; set; }
        public int TodayTotalCall { get; set; }
        public double TodayOutletCoverage { get; set; }
        public double TodayStrikeRate { get; set; }
        public int TodayExtraVisit { get; set; }
        public int TodayExtraCall { get; set; }
        public int TodayExtraOrder { get; set; }
        public int TodaySupperOrder { get; set; }
        public int TodayPerfactOrder { get; set; }
        public int PhoneOrder { get; set; }
        public int Eco { get; set; }
        public double AvgOrderValue { get; set; }
        public double AvgLineItem { get; set; }
    }
    public class InsertCatalogConfigDc
    {
        public long? CityId { get; set; }
        public float Frequency { get; set; }
        public float CustomerReach { get; set; }
        public float Amount { get; set; }
        public bool IsRepeat { get; set; }
        public List<ListingConfigurationDC> listingConfigurationDCs { get; set; }
    }
    public class ListingConfigurationDC
    {
        public long? Id { get; set; }
        public int Sequence { get; set; }
        public string ConfigName { get; set; }
        public bool? Status { get; set; }
        public int ItemCount { get; set; }
        public string Sort { get; set; }
        public bool? Unbilled { get; set; }
        public bool? IsScoreCheck { get; set; }
        public float? ScoreFrom { get; set; }
        public float? ScoreTo { get; set; }
        public bool? NewLaunch { get; set; }
        public bool? PromotionalItems { get; set; }
    }

    public class InsertCustomerRemarksDC
    {
        public long? Id { get; set; }
        public long CustomerId { get; set; }
        public long ExecutiveId { get; set; }
        public string ExecutiveName { get; set; }
        public string Remark { get; set; }
        public DateTime CreatedDate { get; set; }
        public bool IsDelete { get; set; }
    }

    public class AddCheckOutReason
    {
        public int CustomerId { get; set; }
        public int ExecutiveId { get; set; }
        public string Reason { get; set; }
        public string Comment { get; set; }
        public double CurrentLat { get; set; }
        public double CurrentLng { get; set; }
        public string CurrentAddress { get; set; }
        public double Distance { get; set; }
        public DateTime StartDateTime { get; set; }
        public DateTime EndDateTime { get; set; }
        public string ChackOutComment { get; set; }
        public string ShopCloseImage { get; set; }
        public bool IsEnd { get; set; }
        public bool IsBeat { get; set; }

        public bool IsCustInterested { get; set; }
        public bool OrderTaken { get; set; }
        public long? OrderId { get; set; }
        public double? OrderAmount { get; set; }
        public string TechProductInquiry { get; set; }
        public string SKUInquiry { get; set; }
        public string RequiredItemInquiry { get; set; }
        public bool OfferExplain { get; set; }
        public bool MyTargetExplain { get; set; }
        public bool GameSectionExplain { get; set; }
        public bool IsPhysicalVisit { get; set; }
        public int FormType { get; set; }
        public bool IsCall { get; set; }
        public TimeSpan? Duration { get; set; }
        public TimeSpan? ActualCallDuration { get; set; }
    }


    public class GetProductCatalogDC
    {
        public long Id { get; set; }
        public string SectionName { get; set; }
        public string SectionHindiName { get; set; }
        public bool IsPromotional { get; set; }
        public string Type { get; set; }
        public long WarehouseId { get; set; }
        public int Sequence { get; set; }
        public string URL { get; set; }
        public string CustomList { get; set; }
    }
    public class UpdateCheckOutDC
    {
        public long Id { get; set; }
        public string Comment { get; set; }
        public int Sequence { get; set; }
    }
    public class GetCityListDc
    {
        public long Cityid { get; set; }
        public string CityName { get; set; }
        public string CreatedDate { get; set; }
        public bool IsDisable { get; set; }
        public bool IsShow { get; set; }
    }
    public class InsertProductCatalogDataDC
    {
        public long Id { get; set; }
        public string SectionName { get; set; }
        public string SectionHindiName { get; set; }
        public bool IsPromotional { get; set; }
        public string Type { get; set; }
        public long WarehouseId { get; set; }
        public long StoreId { get; set; }
        public int Sequence { get; set; }
        public string URL { get; set; }
        public string CustomList { get; set; }
        public List<itemNumber> ItemNumber { get; set; }
    }
    public class itemNumber
    {
        public int StoreId { get; set; }
        public int Sequence { get; set; }
        public string ItemNumber { get; set; }
    }
    public class CustomerLastOrderDayDc
    {
        public int CustomerId { get; set; }
        public int LastOrderDaysAgo { get; set; }
        public DateTime LastOrderDate { get; set; }
    }


    public class salesDashboardTodayDC //aartimukati
    {
        public int peopleId { get; set; }
        public int SalesPersonId { get; set; }
        public string CityName { get; set; }
        public string WarehouseName { get; set; }
        public string StoreName { get; set; }
        public string ClusterName { get; set; }
        public string SalesPerson { get; set; }
        public int VisitPlanned { get; set; }
        public double BeatSales { get; set; }
        public double ExtraSales { get; set; }
        public double TotalSales { get; set; }
        public int ProductiveCall { get; set; }
        public int ExtraCall { get; set; }
        public int BeatOrder { get; set; }
        public int ExtraOrder { get; set; }
        public int TodayPerfactOrder { get; set; }
        public int TodaySupperOrder { get; set; }
        public int TotalCall { get; set; }
        public int ExtraVisit { get; set; }
        public long TotalRecord { get; set; }
        public DateTime? CheckIn { get; set; }
        public DateTime? CheckOut { get; set; }
        public double TodayStrikeRate { get; set; }
        public double TodayOutletCoverage { get; set; }
        public double Eco { get; set; }
        public int TodayProductiveCall { get; set; }
        public int TodayTotalCall { get; set; }
        public int TodayVisitPlanned { get; set; }
        public int PhoneOrder { get; set; }
        public double AvgLineItem { get; set; }
        public double AvgOrderValue { get; set; }
        public string ChannelName { get; set; }
    }

    public class BeatDSRReportResponseDc   //aartimukati
    {
        public List<salesDashboardTodayDC> salesTodayDC { get; set; }
        public List<SalesDashboardTodayMTDDataa> SalesDashboardTodayMTDData { get; set; }
        public int Totalcount { get; set; }
    }
    public class SalesDashboardTodayMTDDataa : SalesDashboardTodayMTDData   //aartimukati
    {
        public string StartDate { get; set; }
        public string EndDate { get; set; }
    }

    public class SalesPerformanceDashbordFilter
    {
        public List<int> CityIds { get; set; }
        public List<int> WarehouseIds { get; set; }
        public List<int> StoreIds { get; set; }
        public DateTime FirstDate { get; set; }
        public DateTime LastDate { get; set; }
        public string KeyWord { get; set; }
        public int Skip { get; set; }
        public int Take { get; set; }
        public bool IsExecutiveData { get; set; }
        public bool WarehouseData { get; set; }
    }
    public class SalesPerformanceDashbordListDC
    {
        public int ExecutiveId { get; set; }
        public string ExecutiveName { get; set; }
        public string ClusterName { get; set; }
        public string StoreName { get; set; }
        public int CityId { get; set; }
        public string CityName { get; set; }
        public int WarehouseId { get; set; }
        public string WarehouseName { get; set; }
        public double Sales { get; set; }
        public double BeatSales { get; set; }
        public string SalesColour { get; set; }
        public double Dispatch { get; set; }
        public double BeatVisitPercent { get; set; }
        public int BeatTC { get; set; }
        public int TC { get; set; }
        public string TCColour { get; set; }
        public int BeatPC { get; set; }
        public int ProductiveCall { get; set; }
        public string PCColour { get; set; }
        public int ActiveRetailer { get; set; }
        public double AvgLineItem { get; set; }
        public int PerfactOrder { get; set; }
        public int Range { get; set; }
        public double ActualTimeSpend { get; set; }
    }
    public class SalesPerformanceDashbordExecDC
    {
        public List<SalesPerformanceDashbordWarehouseDC> WarehouseDataList { get; set; }
        public List<SalesPerformanceDashbordListDC> salesPerformanceDashbordListDCs { get; set; }
        public int TotalRecords { get; set; }
    }
    public class SalesPerformanceDashbordWarehouseDC
    {
        public string CityName { get; set; }
        public string WarehouseName { get; set; }
        public double Sales { get; set; }
        public string SalesColour { get; set; }
        public double Dispatch { get; set; }
        public double BeatVisitPercent { get; set; }
        public int TC { get; set; }
        public string TCColour { get; set; }
        public int ProductiveCall { get; set; }
        public string PCColour { get; set; }
        public int ActiveRetailer { get; set; }
        public double AvgLineItem { get; set; }
        public int PerfactOrder { get; set; }
        public double Range { get; set; }
        public double ActualTimeSpend { get; set; }
    }

}
