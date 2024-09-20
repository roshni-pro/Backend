using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.Transaction.Reports
{
    public class AchievedWarehouseDataDc
    {
        public long? id { set; get; }
        public string HeadName { set; get; }
        public string PlanType { set; get; }
        public long ObjectId { set; get; }
        public string ObjectName { set; get; }
        public double? PlannedValue { set; get; }
        public double AchievedValue { set; get; }

        public double? AchievedPercent { set; get; }
        public int Month { set; get; }
        public int Year { set; get; }
    }
}
