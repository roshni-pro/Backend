using System;

namespace AngularJSAuthentication.DataContracts.APIParams
{
    public class HubPnLPostParams
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string WarehouseId { get; set; }
        public int? CategoryId { get; set; }
        public int? SubCategoryId { get; set; }
        public int? SubSubCategoryId { get; set; }
        public string ItemNumber { get; set; }
        public string ReportLevel { get; set; }
    }
}
