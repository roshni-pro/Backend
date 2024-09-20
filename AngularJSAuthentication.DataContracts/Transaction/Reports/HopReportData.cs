using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.Transaction.Reports
{
    public class HopReportData
    {
        public string HeadName { get; set; }
        //public long GroupHubPlanId { get; set; }
        //public long Id { get; set; }
        //public long StoreId { get; set; }
        public string PlanType { get; set; }
        public string StoreName { get; set; }
        //public long WarehouseId { get; set; }
        public string WarehouseName { get; set; }
        public double PlannedValue { get; set; }
        public double AchievedValue { get; set; }
        public string AchievedPercent { get; set; }

    }

    public class Procedures
    {
        public int RoundNo { get; set; }
        public string ProcName { get; set; }
    }
}
