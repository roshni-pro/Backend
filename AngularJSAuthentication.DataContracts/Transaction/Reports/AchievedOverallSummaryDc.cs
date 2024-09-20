using System.Collections.Generic;

namespace AngularJSAuthentication.DataContracts.Transaction.Reports
{
    public class AchievedOverallSummaryDc
    {
        public long? id { set; get; }
        public string HeadName { set; get; }
        public string PlanType { set; get; }
        public double? PlannedValue { set; get; }
        public double AchievedValue { set; get; }
        public double? AchievedPercent { set; get; }
        public int Month { get; set; }
        public int Year { get; set; }
        public string ArrowDirection { get; set; }


    }

    public class ParentList
    {
        public int Month { get; set; }
        public int Year { get; set; }
        public dynamic Summary { get; set; }
        public List<Heads> Heads { get; set; }
    }

    public class Heads
    {
        public string HeadName { get; set; }
        public List<string> PlanTypes { get; set; }
    }


    public class FilterSummaryObj
    {
        public int type { get; set; }
        public string plantype { get; set; }
        public int storeid { get; set; }
        public int warehouseid { get; set; }

    }

    public class FieldDashboardMasterFilter
    {
        public string HeadName { get; set; }
        public string PlanType { get; set; }
        public int warehouseid { get; set; }

    }


    public class HeadsFromDB
    {
        public string HeadName { get; set; }
        public string PlanType{ get; set; }
    }

    public class FieldDashboardData
    {
        public List<FieldDashboardMainData> Graphs { get; set; }
        public List<FieldDashboardMainData> MainList { get; set; }
        public List<AchievedSummaryDc> ClusterData { get; set; }
        public List<AchievedSummaryDc> BrandData { get; set; }
        public List<AchievedSummaryDc> StoreData { get; set; }



    }

    public class FieldDashboardMainData
    {
        public string HeadName { get; set; }
        public string PlanType { get; set; }
        public double? PlannedValue { get; set; }
        public double? AchievedValue { get; set; }
        public double? AchievedPercent { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }
        public string ArrowDirection { get; set; }
    }

    public class AchievedSummaryDc
    {
        public string HeadName { set; get; }
        public string PlanType { set; get; }
        public long ObjectId { set; get; }
        public string ObjectName { set; get; }
        public double? PlannedValue { set; get; }
        public double AchievedValue { set; get; }
        public double? AchievedPercent { set; get; }
        public int Month { get; set; }
        public int Year { get; set; }
       
    }
}
