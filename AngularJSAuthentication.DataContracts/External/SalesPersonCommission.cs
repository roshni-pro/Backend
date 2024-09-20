using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.External
{
    public class SalesPersonCommissionData
    {

        public long Id { get; set; }
        public string CategoryName { get; set; }
        public string EventCatName { get; set; }
        public string EventName { get; set; }
        public int WarehouseId { get; set; }
        public int ExecutiveId { get; set; }
        public string Name { get; set; }
        public double ReqBookedValue { get; set; }
        public int IncentiveType { get; set; }
        public double IncentiveValue { get; set; }
        public double BookedValue { get; set; }
        public double EarnValue { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string ShowColumnWithValueField { get; set; }

    }


    public class SalesPersonCommission
    {
        public string Name { get; set; }

        public int EarnValue
        {
            get
            {
                if (CategoryCommissions != null && CategoryCommissions.Any())
                    return CategoryCommissions.Sum(x => x.EarnValue);
                else
                    return 0;
            }
        }
        public List<CategoryCommission> CategoryCommissions { get; set; }

    }

    public class CategoryCommission
    {
        public string CategoryName { get; set; }

        public Dictionary<string, string> ShowColumnWithValueField { get; set; }
        public int EarnValue
        {
            get
            {
                if (EventCommissions != null && EventCommissions.Any())
                    return EventCommissions.Sum(x => x.EarnValue);
                else
                    return 0;
            }
        }
        public List<EventCommission> EventCommissions { get; set; }
    }


    public class EventCommission
    {
        public long Id { get; set; }
        public string EventCatName { get; set; }
        public string EventName { get; set; }
        public int ReqBookedValue { get; set; }
        public int IncentiveType { get; set; }
        public double IncentiveValue { get; set; }
        public int BookedValue { get; set; }
        public int EarnValue { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }

    public class ExportSalesPersonCommission
    {
        public string WarehouseName { get; set; }
        public string ExecutiveName { get; set; }
        public string CategoryName { get; set; }
        public string EventCatName { get; set; }
        public string EventName { get; set; }
        public double RequiredBookedValue { get; set; }
        public string IncentiveType { get; set; }
        public double IncentiveValue { get; set; }
        public double BookedValue { get; set; }
        public double EarnValue { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }


    public class ClusterSaleData
    {
        public string WarehouseName { get; set; }
        public string ClusterName { get; set; }
        public string StoreName { get; set; }
        public string Executive { get; set; }

        public double SkSale { get; set; }
        public double SafoyaSale { get; set; }
        public int CompanyCount { get; set; }
        public int RetailerCount { get; set; }
        public double AvgLineItem { get; set; }

    }

}
