using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.External.SalesAppDc
{
    public class SalesItemForNotificationDC
    {
        public int ItemMultiMRPId { get; set; }
        public string ItemNumber { get; set; }
        public string ItemType { get; set; }
        public int WarehouseId { get; set; }

    }
    public class NewLaunchesItemNotificationDC
    {
        public string ItemName { get; set; }
        public double MRP { get; set; }
        public double UnitPrice { get; set; }
        public string WarehouseName { get; set; }
        public string LogoUrl { get; set; }
    }
    public class GetNotificationByPeopleDc
    {
        public string Title { get; set; }
        public string Message { get; set; }
        public DateTime CreatedDate { get; set; }
        public string LogoUrl { get; set; }
    }
}
