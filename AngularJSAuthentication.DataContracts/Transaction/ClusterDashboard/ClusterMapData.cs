using System.Collections.Generic;

namespace AngularJSAuthentication.DataContracts.Transaction.ClusterDashboard
{
    public class ClusterMapData
    {
        public int CityId { get; set; }
        public string CityName { get; set; }
        public int WarehouseId { get; set; }
        public string WarehouseName { get; set; }
        public int ClusterId { get; set; }
        public string ClusterName { get; set; }
        public decimal lat { get; set; }
        public decimal lng { get; set; }
        public int TotalOrders { get; set; }
        public int OrderedBrands { get; set; }
        public int OrderRetailers { get; set; }
        public int FreqOfOrders { get; set; }
        public double AvgOrderValue { get; set; }
        public double TotalSales { get; set; }
        public double KisanKiranaSales { get; set; }
        public double OnlineSales { get; set; }

        public string Skcode{ get; set; }
        public string ShopName { get; set; }
        public string Mobile { get; set; }
    }

    public class FilteredClusterMapData
    {
        public string FilteredWith { get; set; }
        public int CityId { get; set; }
        public string CityName { get; set; }
        public int WarehouseId { get; set; }
        public string WarehouseName { get; set; }
        public int ClusterId { get; set; }
        public string ClusterName { get; set; }
        public decimal lat { get; set; }
        public decimal lng { get; set; }
        public string FilteredId { get; set; }
        public int TotalOrders { get; set; }
        public int OrderedBrands { get; set; }
        public int OrderRetailers { get; set; }
        public int FreqOfOrders { get; set; }
        public double AvgOrderValue { get; set; }
        public double TotalSales { get; set; }
        public double KisanKiranaSales { get; set; }
        public double OnlineSales { get; set; }
        public string Skcode { get; set; }

    }

    public class FilteredMapData
    {
        public string FilteredWith { get; set; }
        public List<FilteredList> FilteredList { get; set; }
    }

    public class FilteredList
    {
        public string FilterId { get; set; }
        public List<ClusterMapData> ClusterData { get; set; }
    }

}
