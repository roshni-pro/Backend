using System;

namespace AngularJSAuthentication.API.Models
{
    public class PagerDataUIViewModel
    {
        public int First { get; set; }
        public int Last { get; set; }
        public string ColumnName { get; set; }
        public bool IsAscending { get; set; }
        public string Contains { get; set; }
        public int WarehouseId { get; set; }

       
    }
}