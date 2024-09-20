using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace AngularJSAuthentication.API.Helper.EwayBill
{
    public class TokenTimeEwaybillHelper
    {

        public bool TokenGenerateTime(DateTime oldTime)
        {
            bool isgenerate = true;

            List<TimeRange> timeRangeList = new List<TimeRange>();

            TimeRange timeRange = new TimeRange();

            timeRange.Id = 0;
            timeRange.FromTime = new TimeSpan(0, 0, 0);
            timeRange.ToTime = new TimeSpan(05, 59, 59);
            timeRangeList.Add(timeRange);

            timeRange = new TimeRange();

            timeRange.Id = 2;
            timeRange.FromTime = new TimeSpan(06, 00, 00);
            timeRange.ToTime = new TimeSpan(11, 59, 59);
            timeRangeList.Add(timeRange);

            timeRange = new TimeRange();

            timeRange.Id = 3;
            timeRange.FromTime = new TimeSpan(12, 00, 00);
            timeRange.ToTime = new TimeSpan(17, 59, 59);
            timeRangeList.Add(timeRange);

            timeRange = new TimeRange();

            timeRange.Id = 4;
            timeRange.FromTime = new TimeSpan(18, 00, 00);
            timeRange.ToTime = new TimeSpan(23, 59, 59);
            timeRangeList.Add(timeRange);

            DateTime currentTime = DateTime.Now;
          //  DateTime oldTime = Convert.ToDateTime("2023-05-19 17:00:00.000");//tokan Time
            
            //var timerangeData = timeRangeList.Where(x => x.FromTime.Hours > currentTime.Hour && x.ToTime.Hours <= currentTime.Hour).FirstOrDefault();

            var timerangeDataNew = timeRangeList.FirstOrDefault(x => currentTime.Hour >= x.FromTime.Hours && currentTime.Hour <= x.ToTime.Hours);

            if (oldTime.Hour >= timerangeDataNew.FromTime.Hours && oldTime.Hour <= timerangeDataNew.ToTime.Hours)
            {
                isgenerate = false;
            }
            else
            {
                isgenerate = true;
            }
            return isgenerate;
        }

        public class TimeRange
        {
            public int Id { get; set; }
            public TimeSpan FromTime { get; set; }
            public TimeSpan ToTime { get; set; }
        }


    }
}