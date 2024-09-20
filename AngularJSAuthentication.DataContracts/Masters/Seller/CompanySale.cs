using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.Masters.Seller
{
    public class CompanySale
    {
        public int TotalSale { get; set; }
        public double? TotalSalePercant { get; set; }
        public int TotalDispatchAmt { get; set; }
        public double? TotalDispatchAmtPercant { get; set; }
        public int TotalPOCAmt { get; set; }
        public double? TotalPOCAmtPercant { get; set; }
        public int AverageOrderValue { get; set; }
        public double? AverageOrderValuePercant { get; set; }
        public int BilledCustomer { get; set; }
        public double? BilledCustomerPercant { get; set; }
        public double AvgLineItem { get; set; }
        public double? AvgLineItemPercant { get; set; }
        public int SkuSold { get; set; }
        public double? SkuSoldPercant { get; set; }
        public int CustomerReach { get; set; }
        public double? CustomerReachPercant { get; set; }
        public int LiveCFRSKU { get; set; }
        public double? LiveCFRSKUPercant { get; set; }
    }

    public class SellerDashboardData
    {
        public CompanySale CurrentCompanySale { get; set; }
        public CompanySale PreviouseCompanySale { get; set; }
        public CompanyInventory CompanyInventory { get; set; }
    }

    public class SellerRequest
    {
        public int CompanyId { get; set; }
        public List<int> BrandIds { get; set; }
        public int CityId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }

    public class SellerDashboardRequest : SellerRequest
    {
        public string DateRangeType { get; set; }
    }

    public class CompanyCatalogRequest
    {
        public int CompanyId { get; set; }
        public int CityId { get; set; }
    }

    public class CompanySaleRequest
    {
        public int CompanyId { get; set; }
        public int CityId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string DateRangeType { get; set; }
    }

    public class CompanySaleItemRequest
    {
        public int BrandId { get; set; }
        public int CityId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string DateRangeType { get; set; }
    }

    public class CompanyCatalogBrandRequest : CompanyCatalogRequest
    {
        public string Type { get; set; }

    }

    public class CompanyCatalogItemRequest : CompanyCatalogRequest
    {
        public int BrandId { get; set; }
        public string Type { get; set; }
        public string CategoryName { get; set; }

    }


    public class SellerDashboardGraphRequest : SellerRequest
    {
        public string Type { get; set; }
    }


    public class CompanyInventory
    {
        public int CurrentStockQuantity { get; set; }
        public double CurrentStockValue { get; set; }
        public int AvgAging { get; set; }
        public int DamageStockValue { get; set; }
        public int NearExpiryStockValue { get; set; }
        public int StockOutQuantity { get; set; }
        public double PurchaseFillRate { get; set; }
        public int LowStockQuantity { get; set; }
    }

    public class CompanyGraphData
    {
        public DateTime Xaxis { get; set; }
        public double Yaxis { get; set; }
    }

    public class CompanyCatalog
    {
        public int TotalItems { get; set; }
        public int ActiveItems { get; set; }
        public int StockOut { get; set; }
        public int LowStock { get; set; }
    }

    public class CompanyCatalogBrand
    {
        public string CategoryName { get; set; }
        public int BrandId { get; set; }
        public string BrandName { get; set; }
        public int ItemCount { get; set; }

    }
    public class CompanyCatalogItem
    {
        public string ItemName { get; set; }
        public string LogoUrl { get; set; }
        public double MRP { get; set; }
        public double POPrice { get; set; }
        public int MOQ { get; set; }
        public int Stock { get; set; }

    }

    public class CompanySaleData
    {
        public int BrandId { get; set; }
        public string itemname { get; set; }
        public string BrandName { get; set; }
        public int Amount { get; set; }
        public int Qty { get; set; }

    }

    public class GraphAmountData
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Amount { get; set; }
    }

    public class GraphQtyData
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Qty { get; set; }
    }

    public class CompanySaleGraph
    {
        public List<GraphAmountData> BrandAmount { get; set; }
        public List<GraphQtyData> BrandQty { get; set; }

        public List<GraphAmountData> ItemAmount { get; set; }
        public List<GraphQtyData> ItemQty { get; set; }
    }

    public class CompanySalesItemGraph
    {
        public List<GraphAmountData> ItemAmount { get; set; }
        public List<GraphQtyData> ItemQty { get; set; }
    }

    public class CompanyPageSaleData
    {
        public int TotalSale { get; set; }
        public double? TotalSalePercant { get; set; }
        public int TotalDispatchAmt { get; set; }
        public double? TotalDispatchAmtPercant { get; set; }
        public int TotalPOCAmt { get; set; }
        public double? TotalPOCAmtPercant { get; set; }
        public int AverageOrderValue { get; set; }
        public double? AverageOrderValuePercant { get; set; }
        public double AvgLineItem { get; set; }
        public double? AvgLineItemPercant { get; set; }

    }

    public class CompanyInventoryRequest
    {
        public int CompanyId { get; set; }
        public int CityId { get; set; }
    }

    public class CompanyInventoryGraphRequest
    {
        public int CompanyId { get; set; }
        public int CityId { get; set; }
        public int type { get; set; }
    }

    public class CompanyInventoryAgingData
    {
        public string itemname { get; set; }
        public string BrandName { get; set; }
        public int Qty { get; set; }

    }


    public class CompanyInventoryGraph
    {
        public List<GraphAmountData> BrandInventoryAmount { get; set; }
        public List<GraphQtyData> BrandInventoryQty { get; set; }

        public List<GraphAmountData> ItemInventoryAmount { get; set; }
        public List<GraphQtyData> ItemInventoryQty { get; set; }

        public List<GraphQtyData> BrandAvgAging { get; set; }
        public List<GraphQtyData> ItemAvgAging { get; set; }
    }

    public class CompanyOrder
    {
        public string Status { get; set; }
        public int OrderCount { get; set; }

    }


    public class CompanyOrderListElastic
    {
        public int orderid { get; set; }
        public DateTime createddate { get; set; }

        public string cityname { get; set; }
        public Double orderamount { get; set; }
        public double dispatchamount { get; set; }
        public string status { get; set; }

    }

    public class CompanyOrderListExtendedElastic
    {
        public int orderid { get; set; }
        public DateTime createddate { get; set; }
        public DateTime updateddate { get; set; }
        public DateTime? delivereddate { get; set; }

        public string cityname { get; set; }
        public Double orderamount { get; set; }
        public double dispatchamount { get; set; }
        public string status { get; set; }

    }

    public class CompanyOrderList
    {
        public int OrderId { get; set; }
        public DateTime Createddate { get; set; }
        public string City { get; set; }
        public int BillAmount { get; set; }

        public string OrderStatus { get; set; }

        public string CycleTime
        {
            get
            {

                var dateDiff = (DateTime.Now - Createddate);

                return (dateDiff.TotalDays > 0 ? Convert.ToInt32(dateDiff.TotalDays).ToString() + " Days " : "")
                       + (dateDiff.TotalHours > 0 ? Convert.ToInt32(dateDiff.TotalHours).ToString() + " hrs " : "");


            }
        }

    }

    public class ExportCompanyOrderList
    {
        public int OrderId { get; set; }
        public string Skcode { get; set; }
        public string ShopName { get; set; }
        public string OrderBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string City { get; set; }
        public string ItemName { get; set; }
        public int OrderedQuantity { get; set; }
        public double OrderedAmount { get; set; }
        public int DispatchQuantity { get; set; }
        public double DispatchedAmount { get; set; }
        public string OrderStatus { get; set; }
        public string CycleTime
        {
            get
            {

                var dateDiff = (DateTime.Now - CreatedDate);

                return (dateDiff.TotalDays > 0 ? Convert.ToInt32(dateDiff.TotalDays).ToString() + " Days " : "")
                       + (dateDiff.TotalHours > 0 ? Convert.ToInt32(dateDiff.TotalHours).ToString() + " hrs " : "");


            }
        }
        public string PaymentType { get; set; }
        public string Remarks { get; set; }
        public DateTime Deliverydate { get; set; }

    }

    public class CompanyOrderDetail
    {
        public string ItemName { get; set; }
        public double MRP { get; set; }
        public double SellingPrice { get; set; }
        public int Qty { get; set; }

        public double BillAmount
        {
            get
            {
                return Qty * SellingPrice;

            }
        }

    }

    public class CompanyOrderDetailElastic
    {
        public string itemname { get; set; }
        public double mrp { get; set; }
        public double sellingprice { get; set; }
        public int ordqty { get; set; }
        public int? dispatchqty { get; set; }

    }

    public class HeatMapData
    {
        public double Lat { get; set; }
        public double Lg { get; set; }
        public int TotalSale { get; set; }
        public int TotalQty { get; set; }
    }


    public class HeatMapRequest
    {
        public int Id { get; set; }
        public int CityId { get; set; }
        public DateTime startDate { get; set; }
        public DateTime endDate { get; set; }

    }

        public class AllCompanySale
        {
            public int CompanyId { get; set; }
            public string CompanyName { get; set; }
            public int TotalSale { get; set; }
            public int TotalDispatchAmt { get; set; }
            public int AverageOrderValue { get; set; }
            public int BilledCustomer { get; set; }
            public int OrderCount { get; set; }
       
        }
        public class AllCompanyValue
        {
            public double val { get; set; }
            public int compid { get; set; }
        }
        public class AllCompanyordcountamounts
        {
            public int compid { get; set; }
            public int ordcount { get; set; }
            public double ordamount { get; set; }
            public int linecount { get; set; }
            public int billedcust { get; set; }
            public int skusold { get; set; }

        }
        //public class ALLCompanySellerDashboardData
        //{
        //    public AllCompanySale CurrentCompanySale { get; set; }
        //    //public AllCompanySale PreviouseCompanySale { get; set; }
        //}
        public class AllCompanyPayload
        {
            public int CityId { get; set; }
            public DateTime StartDate { get; set; }
            public DateTime EndDate { get; set; }
            public string DateRangeType { get; set; }
        }
}


