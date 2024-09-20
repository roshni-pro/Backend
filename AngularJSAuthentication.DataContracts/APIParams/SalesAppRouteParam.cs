using AngularJSAuthentication.DataContracts.Transaction.Reports;
using System;
using System.Collections.Generic;

namespace AngularJSAuthentication.DataContracts.APIParams
{
    public class SalesAppRouteParam
    {      
        public int PeopleId { get; set; }
        public int CustomerId { get; set; }
        public double CurrentLat { get; set; }
        public double CurrentLng { get; set; }
        public string CurrentAddress { get; set; }
        public double Distance { get; set; }
        public DateTime StartDateTime { get; set; }
        public DateTime EndDateTime { get; set; }
        public string Comment { get; set; }
        public string ShopCloseImage { get; set; }
        public bool IsEnd { get; set; }
        public bool IsBeat { get; set; }
    }

    public class DayStartParams
    {
        public int PeopleId { get; set; }
        public double lat { get; set; }

        public double lng { get; set; }

        public string DayStartAddress { get; set; }

    }


    public class SalesDeshboardData
    {
        public MyBeat MyBeat { get; set; }
        public bool ShowTarget { get; set; }
        public BeatSale SalesMetricsDaily { get; set; }
        public BeatSaleWeekly SaleMetricsWeekly { get; set; }
        public BeatSaleMonthly SaleMetricsMonthly { get; set; }
        public BeatSale CustomerAcquisitionMonthly { get; set; }

        public BeatTarget BeatTarget { get; set; }
        public SalesTarget SalesTarget { get; set; }
        public BeatTarget SalesWeeklyTarget { get; set; }
        public SalesMonthlyTarget SalesMonthlyTarget { get; set; }
        public CancellationReportResDc CancellationReports { get; set; }

    }

    public class MyBeat
    {
        public string AreaName { get; set; }
        public int TodayVisit { get; set; }
        public int Visited { get; set; }
        public string VisitedColor { get; set; }
        public int Conversion { get; set; }
        public string ConversionColor { get; set; }
        public int BeatOrder { get; set; }
        public string BeatOrderColor { get; set; }
        public int BeatAmount { get; set; }
        public string BeatAmountColor { get; set; }
        public decimal AvgLineItem { get; set; }
        public string AvgLineItemColor { get; set; }
        public decimal TotalDistance { get; set; }
        public decimal TotalTime { get; set; }
        public decimal AmountPotential { get; set; }
        public decimal OrderPotential { get; set; }
        public List<BeatCustomer> BeatCustomers { get; set; }

    }

    public class SalesTarget
    {
        public int CustomerCount { get; set; }
        public int OrderCount { get; set; }
        public int OrderAmount { get; set; }
        public decimal AvgLineItem { get; set; }
    }

    public class SalesMonthlyTarget: SalesTarget
    {
        public double ProductPareto { get; set; }
        public double CustomerPareto { get; set; }
    }

    public class BeatTarget : SalesTarget
    {
        public int Visited { get; set; }
        public int Conversion { get; set; }       
    }
    public class BeatCustomer
    {
        public int CustomerId { get; set; }
        public string ShopName { get; set; }
        public string Mobile { get; set; }
        public string Name { get; set; }
        public string Skcode { get; set; }
        public int BeatNumber { get; set; }
        public string BillingAddress { get; set; }
        public string ShippingAddress { get; set; }
        public int? WarehouseId { get; set; }
        public string WarehouseName { get; set; }
        public string AreaName { get; set; }
        public bool Active { get; set; }
        public double lat { get; set; }
        public double lg { get; set; }
        public bool IsVisited { get; set; }
        public bool IsKPP {get; set;}
        public int TotalOrder { get; set; }
        public int TotalOrderAmount { get; set; }
        public decimal AvgLineItem { get; set; }
        public double WtAvgAmount { get; set; }
        public double WtAvgOrder { get; set; }
        public string Comment { get; set; }
        public string Status { get; set; }
        public int MaxOrderCount { get; set; }
        public decimal TotalLineItem { get; set; }
        public DateTime? TravalStart { get; set; }
        public long TotalTravalInSec { get; set; }
        public string CustomerType { get; set; }

    }

    public class BeatSale
    {
        public int CustomerCount { get; set; }
        public string CustomerCountColor { get; set; }
        public int TotalOrders { get; set; }
        public string TotalOrdersColor { get; set; }
        public int TotalAmount { get; set; }
        public string TotalAmountColor { get; set; }
        public decimal AvgLineItem { get; set; }
        public string AvgLineItemColor { get; set; }
    }

    public class BeatSaleWeekly: BeatSale
    {
        public int PlannedVisit { get; set; }
        public int Visited { get; set; }
        public string VisitedColor { get; set; }
        public int Conversion { get; set; }
        public string ConversionColor { get; set; }
        public int NotVisited { get; set; }
        public string NotVisitedColor { get; set; }
        public List<BeatCustomer> BeatCustomers { get; set; }
    }
    public class BeatSaleMonthly : BeatSale
    {
        public double ProductPareto { get; set; }
        public double CustomerPareto { get; set; }
       // public List<BeatCustomer> BeatCustomers { get; set; }
    }

    public class BeatCustomerOrder
    {
        public int CustomerId { get; set; }
        public int OrderId { get; set; }
        public double GrossAmount { get; set; }
        public int lineItem { get; set; }
        public DateTime CreatedDate { get; set; }
        public int weightAvgOrder { get; set; }
        public double weightAvgAmount { get; set; }
    }

    public class SalesCustomerOrders
    {
        public List<string> Subcategories { get; set; }
        public List<string> Brands { get; set; }
        public List<CusterOrderHistory> CusterOrderHistories { get; set; }
    }
    public class CusterOrderHistory
    {
        public string SubCategoryName { get; set; }
        public string BrandName { get; set; }
        public string ItemName { get; set; }
        public int OrderAmount { get; set; }
    }

    public class BestSellingSubCategory
    {
        public string SubCategoryName { get; set; }

        public string HindiName { get; set; }
        public int customerCount { get; set; }
        public int OrderCount { get; set; }
        public int Amount { get; set; }
        public double diff { get; set; }
    }

    public class ClusterPareto
    {
        public double CustomerPareto { get; set; }
        public double ItemPareto { get; set; }
    }

    public class BeatWithMeanLatLg
    {
        public double Lat { get; set; }
        public double lg { get; set; }
        public string Day { get; set; }
    }

    public class SalesDashboardReportReqDc
    {
        public int WarehouseId { get; set; }
        public List<int> PeopleIds { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }


    public class SalesDashboardReportDc
    {
        public string ExectiveName { get; set; }
        public int? ExectiveId { get; set; }
        public string WarehouseName { get; set; }
        public string ClusterName { get; set; }
        public int? TotalCustomer { get; set; }
        public int? TotalBeat { get; set; }
        public int? CustomerPlann { get; set; }
        public int? Visited { get; set; }
        public int? Conversion { get; set; }
        public double? OrderAmount { get; set; }
        public decimal? AvgLine { get; set; }
        public int? Ordercount { get; set; }

        public List<CustomerVisitDc>  VisitDetails  { get; set; }
}
    public class CustomerVisitDc
    {
        public string SKcode { get; set; }
        public string ShopName { get; set; }
        public string Address { get; set; }
        public DateTime? Date { get; set; }

    }

    public class PeopleBeatCustomerOrder
    {
        public int CustomerId { get; set; }
        public int PeopleId { get; set; }
        public int OrderId { get; set; }
        public double GrossAmount { get; set; }
        public int lineItem { get; set; }
        public DateTime CreatedDate { get; set; }
        public int weightAvgOrder { get; set; }
        public double weightAvgAmount { get; set; }
    }

    public class PeopleBeatCustomers
    {
        public string ExectiveName { get; set; }
        public int PeopleId { get; set; }
        public string WarehouseName { get; set; }
        public string ClusterName { get; set; }
        public int TotalCustomer { get; set; }
        public int TotalBeat { get; set; }
    
    }

    public class StoreSalesAppDashboardReqDc
    {
        public int SubCateId { get; set; }
        public int CityId { get; set; }
        public List<int> PeopleIds { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
    public class StoreSalesDashboardReportDc
    {
        public string ExectiveName { get; set; }
        public int? ExectiveId { get; set; }
        public string WarehouseName { get; set; }
        public string ClusterName { get; set; }
        public int? TotalCustomer { get; set; }
        public int? TotalBeat { get; set; }
        public int? CustomerPlann { get; set; }
        public int? Visited { get; set; }
        public int? Conversion { get; set; }
        public double? OrderAmount { get; set; }
        public decimal? AvgLine { get; set; }
        public int? Ordercount { get; set; }

        public List<CustomerVisitDc> VisitDetails { get; set; }
    }
}
