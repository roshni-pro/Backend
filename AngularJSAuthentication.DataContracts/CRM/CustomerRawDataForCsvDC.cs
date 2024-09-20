using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.CRM
{
    public class CustomerRawDataForCsvDC
    {
        public string Ccode { get; set; }
        public DateTime TDate { get; set; }
        public int TId { get; set; }
        public double? Amount { get; set; }
        public int? Quantity { get; set; }
        public string CompanyName { get; set; }
        public string CategoryName { get; set; }
        public string BrandName { get; set; }
        //public string itemcode { get; set; }
        //public int? itemmultimrpid { get; set; }
        public string CityName { get; set; }
        public string WarehouseName { get; set; }
        public string ClusterName { get; set; }
    }

    public class crmCustomerResultDC
    {
        public string Skcode { get; set; }
        public List<long> Platforms { get; set; }
    }

    public class CRMDetailGetDc
    {
        public string CRMName { get; set; }
        public long CRMID { get; set; }
        public string CRMTags { get; set; }
    }

    public class CRMCustomerWithTag
    {
        public int CustomerId { get; set; }
        public string CRMTags { get; set; }
        public string Skcode { get; set; }
    }
}
