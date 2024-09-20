using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.Masters.Seller
{

    public class GetCatelogueItemWithCFRDc
    {
        public int TotalItem { get; set; }
        public int Activeitem { get; set; }
        public int TotalCFRItem { get; set; }
        public int CFRItem { get; set; }
    }
    public class CatelogueItemExportDc
    {
        public string Number { get; set; }
        public int IsActive { get; set; }
        public string ItemBaseName { get; set; }
        public string WarehouseName { get; set; }
        public string CityName { get; set; }
    }
    public class GetSellerSalesDc
    {
        public int monthactiveRetailer { get; set; }
        public double monthtotalsale { get; set; }
        public int monthtotalOrder { get; set; }
        public int monthtotalLineitem { get; set; }
        public double monthAvgOrderValue { get; set; }
        public decimal monthAvgLineItem { get; set; }

        public int TodayactiveRetailer { get; set; }
        public double todaytotalsale { get; set; }
        public int todaytotalOrder { get; set; }
        public double todayAvgOrderValue { get; set; }
        public int todaytotalLineitem { get; set; }
        public decimal todayAvgLineItem { get; set; }

        public int YesterdayactiveRetailer { get; set; }
        public double Yesterdaytotalsale { get; set; }
        public int YesterdaytotalOrder { get; set; }
        public double YesterdayAvgOrderValue { get; set; }
        public int YesterdaytotalLineitem { get; set; }
        public decimal YesterdayAvgLineItem { get; set; }

    }
    public class DashboardPoStatusCountDc
    {
        public int Pcount { get; set; }
        public string Status { get; set; }

        public double PoAmount { get; set; }
    }
    public class DashboardOrderStatusDataDc
    {
        public int Ordercount { get; set; }
        public string Status { get; set; }
        public double  Sales { get; set; }
    }
    public class FillRateDc
    {
        public decimal fillrate { get; set; }
        public decimal Signfillrate { get; set; }
    }
    public class DashboardOrderAvgTATDc
    {
        public int? OrderAvgTAT { get; set; }
    }
    public class POAvgTATDc
    {
        public int? POAvgTAT { get; set; }
    }


    public class DashboardCurrentVsNetCurrentDc
    {
        public double CurrentStockAmount { get; set; }
        public double CurrentNetStockAmount { get; set; }
        public double DamageStockAmount { get; set; }
        public double ExpireStockAmount { get; set; }
        public double YesterdayStock { get; set; }

    }
    public class SellerReqDc
    {
        public int PeopleId { get; set; }
        public int SubCatId { get; set; }

    }

    public class SearchReqDc
    {
        public List<int> CityIds { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
    }

    public class DashboardOrderStatusDataCount
    {
        public int PendingOrdercount { get; set; }
        public double PendingOrderAmount { get; set; }
        public int ReadytoDispatchOrdercount { get; set; }
        public double ReadytoDispatchOrderAmount { get; set; }
        public int IssuedOrdercount { get; set; }
        public double IssuedOrderAmount { get; set; }
        public int ShippedOrdercount { get; set; }
        public double ShippedOrderAmount { get; set; }
        public int DeliveredOrdercount { get; set; }
        public double DeliveredOrderAmount { get; set; }
        public int DeliveryRedispatchOrdercount { get; set; }
        public double DeliveryRedispatchOrderAmount { get; set; }
        public int DeliveryCanceledOrdercount { get; set; }
        public double DeliveryCanceledOrderAmount { get; set; }
        public int PreCanceledOrdercount { get; set; }
        public double PreCanceledOrderAmount { get; set; }


    }

    public class DashboardPoStatusCount
    {
   
        public int CancelPOCount { get; set; } 
        public int PendingPOCount { get; set; }
        public int PartialPOCount { get; set; }
        public int ClosedPOCount { get; set; }
        public double CancelPOAmount { get; set; }
        public double PartialPOAmount { get; set; }
        public double PendingPOAmount { get; set; }
        public double ClosedPOAmount { get; set; }


    }

    public class POGRIRDC
    {
        public int POCount { get; set; }
        public int IRCount { get; set; }
        public int GRCount { get; set; }
        public decimal POAmount { get; set; }
        public decimal IRAmount { get; set; }
        public decimal GRAmount { get; set; }


    }
    public class ParetoIndex
    {
        public int passmonth { get; set; }
        public int passyear { get; set; }
        public int TotalCustomerCount { get; set; }
        public decimal TotalSaleValue { get; set; }
        public decimal SeventyPercentSales { get; set; }
        public int SeventyPercentCustomer { get; set; }
    }



    public class OrderDetailExportDc
    {
        public string WarehouseName { get; set; }
        public string ItemName { get; set; }
        public int OrderId { get; set; }
        public int OrderQty { get; set; }
        public double MRP { get; set; }
        public double TotalAmount { get; set; }
    }

    public class SellerCFRDc
    {
        public string ItemNumber { get; set; }
        public string CategoryName { get; set; }
        public string BrandName { get; set; }
        public string itemBaseName { get; set; }
        public double MRP { get; set; }
        public string WarehouseName { get; set; }
        public string Category { get; set; }
        public double LimitValue { get; set; }
        public int active { get; set; }
        public string activeItem { get; set; }

    }


    public class PostCityidsDc
    {
        public List<int> cityids { get; set; }
        

    }




    public class SalesExportDc
    {
        public int OrderId { get; set; }
        public int OrderDetailsId { get; set; }
        public string Itemname { get; set; }
        public string ItemNumber { get; set; }
        public double UnitPrice { get; set; }
        public int OrderQty { get; set; }
        public string WarehouseName { get; set; }
        public string CityName { get; set; }
        public string Skcode { get; set; }
        public DateTime CreatedDate { get; set; }

    }
    public class OrderFillRateExportDc
    {
        public int OrderId { get; set; }
        public int OrderDetailsId { get; set; }
        public string Itemname { get; set; }
        public string ItemNumber { get; set; }
        public double UnitPrice { get; set; }
        public int OrderQty { get; set; }
        public int DispatchedQty { get; set; }
        public string WarehouseName { get; set; }
        public string CityName { get; set; }
        public string Skcode { get; set; }
        public DateTime CreatedDate { get; set; }

    }

    public class POFillRateExportDc
    {
        public int PurchaseOrderId { get; set; }
        public int PurchaseOrderDetailId { get; set; }
        public string ItemName { get; set; }
        public string ItemNumber { get; set; }
        public int TotalQty { get; set; }
        public int GRNTotalQty { get; set; }
        public string WarehouseName { get; set; }
        public string CityName { get; set; }
        public string SupplierName { get; set; }
        public DateTime CreatedDate { get; set; }

    }
}


