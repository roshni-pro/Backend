using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.NonRevenueOrderDc
{
   
    public class ResponseMsg
    {
        public bool Status { get; set; }
        public string Message { get; set; }

    }
    public class NonRevenueSettelmentOrders
    {
        public List<int> WarehouseIds { get; set; }
        public List<string> CustomerType { get; set; }
        public List<string> Status { get; set; }
        public string FromDate { get; set; }
        public string ToDate { get; set; }
        public string Keyword { get; set; }
        public int Skip { get; set; }
        public int Take { get; set; }

    }
    public class ReturnNonRevenueSettelmentOrders
    {
        public int? OrderId { get; set; }
        public string WarehouseName { get; set; }
        public string Skcode { get; set; }
        public string Reason { get; set; }
        public string status { get; set; }
        public string customerDetails { get; set; }
        public DateTime? OrderDate { get; set; }
        public int TotalRecords { get; set; }


    }
   
}
