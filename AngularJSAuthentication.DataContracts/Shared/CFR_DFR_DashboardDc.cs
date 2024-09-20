using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.Shared
{
    public class DFRDashboardDataDc
    {
        public string BuyerName { get; set; }
        public string warehousename { get; set; }
        public string ItemName { get; set; }
        public int yesterdaydemand { get; set; }
        public int olddemand { get; set; }
        public int Demand { get; set; }
        public int CurrentStock { get; set; }
        public int NetDemand { get; set; }
        public int TotalGrQty { get; set; }
        public int TotalInternalTransfer { get; set; }
        public int DFRPercent { get; set; }
        public string status { get; set; }
        public DateTime DemandDate { get; set; }
    }

    public class DFRDashboardGraphDc
    {
        public string BuyerName { get; set; }
        public int DFRPercent { get; set; }
        public string DemandDay { get; set; }
    }

    public class DFRDashboardDc
    {
        public List<DFRDashboardDataDc> DFRDashboardDataDcs { get; set; }
        public List<DFRDashboardGraphDc> DFRDashboardGraphDcs { get; set; }
        public int TotalGreen { get; set; }
        public int TotalRed { get; set; }
    }

    public class DFRDashboardRequestDc
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public List<int> buyerIds { get; set; }
        public List<int> warehouseIds { get; set; }
    }

    public class CFRDashboardRequestDc
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public List<int> buyerIds { get; set; }
        public List<int> warehouseIds { get; set; }
        public List<int> categoriesIds { get; set; }
        public List<int> subcategoriesIds { get; set; }
        public List<int> brandIds { get; set; }
    }

    public class CFRDashboardDataDc
    {
        public string buyerName { get; set; }
        public string itemnumber { get; set; }
        public string warehouseName { get; set; }
        public string ItemName { get; set; }
        public string CategoryName { get; set; }
        public string SubCategoryName { get; set; }
        public string SubSubCategoryName { get; set; }
        public string itembasename { get; set; }
        public double MRP { get; set; }
        public double LimitValue { get; set; }
        public bool IsActive { get; set; }
        public double Active_per { get; set; }
        public string status { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    public class CFRDashboardGraphDc
    {
        public string BuyerName { get; set; }
        public int CFRPercent { get; set; }
        public string DemandDay { get; set; }
    }

    public class CFRDashboardDc
    {
        public List<CFRDashboardDataDc> CFRDashboardDataDcs { get; set; }
        public List<CFRDashboardGraphDc> CFRDashboardGraphDcs { get; set; }
        public int TotalGreen { get; set; }
        public int TotalRed { get; set; }
    }

}
