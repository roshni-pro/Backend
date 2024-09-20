using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.Shared
{
    public class FilterOrderDTO
    {
        public int ItemPerPage { get; set; }
        public int PageNo { get; set; }
        //public int WarehouseId { get; set; }
        //public int Cityid { get; set; }
        public List<int> WarehouseIds { get; set; }
        public List<int?> Cityids { get; set; }
        public DateTime? start { get; set; }
        public DateTime? end { get; set; }
        public int OrderId { get; set; }
        public string Skcode { get; set; }
        public string ShopName { get; set; }
        public string Mobile { get; set; }
        public string status { get; set; }
        public int? TimeLeft { get; set; }
        public string LevelName { get; set; }
        public string invoice_no { get; set; }
        public List<string> PaymentFrom { get; set; }
        public long GenerationId { get; set; }
        public int OrderType { get; set; }
        public string SortDirection { get; set; }
        public string CustomerType { get; set; }
    }
}
