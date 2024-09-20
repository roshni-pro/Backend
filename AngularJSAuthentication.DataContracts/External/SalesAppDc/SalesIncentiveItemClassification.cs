using System.Collections.Generic;

namespace AngularJSAuthentication.DataContracts.External.SalesAppDc
{
    public class SalesIncentiveItemClassification
    {
        public int PeopleId { get; set; }
        public string ItemClassification { get; set; }
        public long ItemIncentiveClassificationId { get; set; }
        public double CommissionPercentage { get; set; }
        public double SaleValue { get; set; }
        public double Earning { get; set; }
    }


    public class SalesPersonKpiAndIncentive
    {
        public long KPIId { get; set; }
        public string KpiName { get; set; }
        public string DisplayName { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }
        public int StoreId { get; set; }
        public double Target { get; set; }
        public double AchievePercent { get; set; }
        public double IncentivePercent { get; set; }
        public string Type { get; set; }
        public string ExecutiveName { get; set; }
        public double IncentiveAmount { get; set; }
        public int ClusterId { get; set; }       
        public string StoreName { get; set; }       
    }
    public class SalesPersonKpiAndIncentiveAchivement
    {
        public long KPIId { get; set; }
        public string KpiName { get; set; }
        public string DisplayName { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }
        public int StoreId { get; set; }
        public double Target { get; set; }
        public double AchievePercent { get; set; }
        public double IncentivePercent { get; set; }
        public string Type { get; set; }
        public string ExecutiveName { get; set; }
        public double IncentiveAmount { get; set; }
        public int ClusterId { get; set; }
        public int ExecutiveId { get; set; }
    }

    public class SalesPersonKpiAndIncentiveInfo
    {        
        public string KpiName { get; set; }
        public string DisplayName { get; set; }
        public double Target { get; set; }
        public double AchievePercent { get; set; }
        public double IncentivePercent { get; set; }
        public string Type { get; set; }
        public string ExecutiveName { get; set; }
        public double IncentiveAmount { get; set; }
        public int ClusterId { get; set; }
        public string ClusterName { get; set; }
    }

    public class SalesKpiResponse
    {
        public int StoreId { get; set; }
        public string StoreName { get; set; }
        public List<SalesPersonKpiResponse> SalesPersonKpi { get; set; }
    }
    public class SalesPersonKpiResponse
    {
        public string KpiName { get; set; }
        public string DisplayName { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }
        public string Type { get; set; }
        public double Target { get; set; }
        public double Achievement { get; set; }
        public double AchievementPercent { get; set; }
        public double Earning { get; set; }
        
    }

    public class SalesPersonKpiAchivementResponse
    {
        public int ExecutiveId { get; set; }
        public string KpiName { get; set; }
        public string DisplayName { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }
        public string Type { get; set; }
        public double Target { get; set; }
        public double Achievement { get; set; }
        public double AchievementPercent { get; set; }
        public double Earning { get; set; }



    }

    public class SalesPersonKpiElasticData
    {
        public string skcode { get; set; }
        public long storeid { get; set; }
        public double dispatchamount { get; set; }
        public double linecount { get; set; }
    }

    public class SalesPersonKpiElasticSuccssStoreData
    {
        public string skcode { get; set; }
        public long storeid { get; set; }
        public int executiveid { get; set; }
        public double dispatchamount { get; set; }
        public double linecount { get; set; }
    }

    public class SalesPersonKpiForSuccessStore
    {
        public int Id { get; set; }
        public long KPIId { get; set; }
        public string KpiName { get; set; }
        public string DisplayName { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }
        public int StoreId { get; set; }
        public double Target { get; set; }
        public double AchievePercent { get; set; }
        public double IncentivePercent { get; set; }
        public string Type { get; set; }
        public string ExecutiveName { get; set; }
        public double IncentiveAmount { get; set; }
        public int ClusterId { get; set; }
        public int ExecutiveId { get; set; }
    }

}
