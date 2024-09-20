using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.ClusterHoliday
{
    public class ClusterHolidayDc
    {
        public long warehouseId { get; set; }
        public long clusterId { get; set; }
        public List<string> holiday { get; set; }
        //public int year { get; set; }
    }

    public class CustomerHolidayDc
    {
        public long warehouseId { get; set; }
        public long clusterId { get; set; }
        public string holiday { get; set; }
        public List<DateTime> HolidayDate { get; set; }
        public string skCode { get; set; }
       // public int year { get; set; }
    }
    public class GetCustomerDC
    {
            public int warehouseid { get; set; }
            public int clusterid { get; set; }
            public List<string> SkcodeList { get; set; }
            public int skip { get; set; }
            public int take { get; set; }
    }
    public class CustomerHolidayListDC
    {
        public long id { get; set; }
        public int warehouseid { get; set; }
        public int clusterId { get; set; }
        public string clusterName { get; set; }
        public string holiday { get; set; }
        public string skCode { get; set; }
        //public int year { get; set; }
    }
    public class CustomerHolidayListTotalDC
    {
        public int TotalRecords { get; set; }
        public List<CustomerHolidayListDC> CustomerHolidayList { get; set; }
    }

    public class getClusterHolidayListDC
    {
        public List<string> holiday { get; set; }
        public List<DateTime> HolidayDate { get; set; }
    }

    public class UpdateCustomerHolidayDC
    {
       // public long Id { get; set; }
        public int warehouseid { get; set; }
        public int clusterId { get; set; }
        public string skCode { get; set; }
        public List<string> holiday { get; set; }
       // public int year { get; set; }
    }

    public class customerHolidayUploadDC
    {
        public string skcode { get; set; }
        public string holiday { get; set; }
    }
}
