using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AngularJSAuthentication.API.ControllerV7.Store
{
    public class ClusterExecutiveBeat
    {
        public string Skcode { get; set; }
        public int? BeatNumber { get; set; }
        public string Day { get; set; }
        public int ExecutiveId { get; set; }
        public int SkipDays { get; set; }
        public int SkipWeeks { get; set; }
        public long StoreId { get; set; }
        public string EvenOrOddWeek { get; set; }
    }
    public class ValidatingAssignBeatDc
    {
        public List<ClusterExecutiveBeat> ClusterExecutiveBeat { get; set; }
        public List<int?> clusterIds { get; set; }
        public List<long> ChannelMasterIds { get; set; }

    }
}