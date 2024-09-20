using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.Masters
{
    public class LiveBrandDataForMetaBaseDC
    {
        public string Brand { get; set; }
        public int MTDCFR { get; set; }
        public int MTDDFR { get; set; }
        public int MTDSPI { get; set; }
        public int MTDVisit { get; set; }
        public int MTDActiveUser { get; set; }
        public int TodayCFR { get; set; }
        public double TodayDFR { get; set; }
        public double TodaySPI { get; set; }
        public int TodayVisit { get; set; }
        public int TodayActiveUser { get; set; }
        public int YesterdayCFR { get; set; }
        public double YesterdayDFR { get; set; }
        public double YesterdaySPI { get; set; }
        public int YesterdayVisit { get; set; }
        public int YesterdayActiveUser { get; set; }
    }
    public class LiveBrandDataForMetaRes
    {
        public List<LiveBrandDataForMetaBaseDC> LiveBrandDataForMetaBaseDC { get; set; }
        public int TotalCount { get; set; }
    }
    public class LiveBrandDataForMetaBaseFilterDC
    {
        public int Skip { get; set; }
        public int Take { get; set; }
        public List<int> BrandIds { get; set; }
    }
}
