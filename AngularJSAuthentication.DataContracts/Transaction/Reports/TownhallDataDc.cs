using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.Transaction.Reports
{
    public class TownhallData
    {
        public ObjectId Id { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }
        public List<ActiveAgentDc> ActiveAgents { get; set; }
        public List<ActiveKpp> ActiveKpps { get; set; }
        public List<ActiveCluster> ActiveClusters { get; set; }
        public List<PoFillrateDc> PoFillrates { get; set; }
        public List<OrderFillrateDc> OrderFillrates { get; set; }
        public List<ActiveVarifiedStoresDc> ActiveVarifiedStoresDcs { get; set; }
        public List<DPaddedusedDc> DPaddedusedDcs { get; set; }
        public List<CRMlevelDc> CRMlevelDcs { get; set; }
        public List<InventoryDayDc> InventoryDayDcs { get; set; }
        public List<ActiveSourcingDc> ActiveSourcingDcs { get; set; }
        public List<ActiveKKSourcingDc> ActiveKKSourcingDcs { get; set; }
        public List<MonthlySaleDataDc> MonthlySaleDataDc { get; set; }
        public List<TownHallPostCancellationDc> TownHallPostCancellationDcs { get; set; }
        public List<TATinHrsDc> TATinHrsDc { get; set; }


    }

    public class ActiveAgentDc
    {
        public int Months { get; set; }
        public int TotalActiveAgent { get; set; }
        public string MonthName { get; set; }
    }
    public class ActiveKpp
    {
        public int Months { get; set; }
        public int TotalActivekpp { get; set; }
        public string MonthName { get; set; }
    }
    public class ActiveCluster
    {
        public int Months { get; set; }
        public int TotalActiveCluster { get; set; }
        public string MonthName { get; set; }
    }
    public class PoFillrateDc
    {
        public int Months { get; set; }
        public double fillrates { get; set; }
        public string MonthName { get; set; }
    }
    public class OrderFillrateDc
    {
        public int Months { get; set; }
        public double fillrates { get; set; }
        public string MonthName { get; set; }
    }
    public class ActiveVarifiedStoresDc
    {
        public int Months { get; set; }
        public int ActiveStores { get; set; }
        public int VarifiedStores { get; set; }
        public string MonthName { get; set; }
    }
    public class DPaddedusedDc
    {
        public int Months { get; set; }
        public double PointsAdded { get; set; }
        public double PointsUsed { get; set; }
        public string MonthName { get; set; }
    }
    public class CRMlevelDc
    {
        public int Months { get; set; }
        public int L5Count { get; set; }
        public int L4Count { get; set; }
        public int L3Count { get; set; }
        public int L2Count { get; set; }
        public int L1Count { get; set; }
        public int L0Count { get; set; }
        public string MonthName { get; set; }
    }
    public class InventoryDayDc
    {
        public int Months { get; set; }

        public double Values { get; set; }
        public string MonthName { get; set; }
    }

    public class ActiveSourcingDc
    {
        public int Months { get; set; }
        public int ActiveItems { get; set; }
        public int ActiveBrand { get; set; }
        public int ActiveVendors { get; set; }
        public string MonthName { get; set; }
    }

    public class ActiveKKSourcingDc
    {
        public int Months { get; set; }
        public int ActiveItemsKK { get; set; }
        public string MonthName { get; set; }
    }
    public class MonthlySaleDataDc
    {
        public int TotalOrders { get; set; }
        public int OrderedBrands { get; set; }
        public int OrderRetailers { get; set; }
        public double FreqOfOrders { get; set; }
        public int Months { get; set; }
        public double AvgLineItem { get; set; }
        public double AvgOrderValue { get; set; }
        public double TotalSales { get; set; }
        public double KisanKiranaSales { get; set; }
        public int KisanKiranaRetailers { get; set; }
        public double OnlineSales { get; set; }
        public string MonthName { get; set; }
    }



    public class TownHallPostCancellationDc
    {
        public int Months { get; set; }
        public double PostCancellation { get; set; }
        public string AmountPercentage { get; set; }
        public string MonthName { get; set; }
    }

    public class TATinHrsDc
    {
        public int Months { get; set; }
        public double TATinHrs { get; set; }
        public string MonthName { get; set; }
    }


    public class TownHallCommentsSection
    {
        public ObjectId Id { get; set; }
        public int Year { get; set; }
        public int Month { get; set; }

        public TownHallKppComments TownHallKppComments { get; set; }
        public TownHallCustomerDelightComments TownHallCustomerDelightComments { get; set; }
        public TownHallPurchaseComments TownHallPurchaseComments { get; set; }
        public TownHallSourcingComments TownHallSourcingComments { get; set; }
        public TownHallOperationComments TownHallOperationComments { get; set; }
        public TownHallSalesComments TownHallSalesComments { get; set; }
    }

    public class TownHallKppComments
    {
       
        public string ActiveCluster { get; set; }
        public string ActiveKPP { get; set; }
        public string AgentKPP { get; set; }
        public string Code { get; set; }
    }
    public class TownHallCustomerDelightComments
    {
        public string ActiveVerifiedStores { get; set; }
        public string DPAdded { get; set; }
        public string CRMLevel { get; set; }
    }
    public class TownHallPurchaseComments
    {
        public string FillRateAnalysis { get; set; }
        public string InventoryDays { get; set; }
    }
    public class TownHallSourcingComments
    {
        public string ActiveItems { get; set; }
        public string ActiveBrandsAndVendors { get; set; }
        public string ActiveItemsKissanKirana { get; set; }
    }
    public class TownHallOperationComments
    {
        public string CancelOrder { get; set; }
        public string TurnAroundTime { get; set; }
    }
    public class TownHallSalesComments
    {
        public string TotalSales { get; set; }
        public string OnlineSales { get; set; }
        public string KisanKiranaSales { get; set; }
        public string OrderAndKisanKiranaRetailers { get; set; }
        public string TotalOrders { get; set; }
        public string TotalBrands { get; set; }


    }
}
