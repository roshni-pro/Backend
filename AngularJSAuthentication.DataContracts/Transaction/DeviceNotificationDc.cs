using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.Transaction
{
    public class DeviceNotificationDc
    {
        public int? Id { get; set; }
        public int? CompanyId { get; set; }
        public int? WarehouseId { get; set; }
        public int? CustomerId { get; set; }
        public string DeviceId { get; set; }
        public string title { get; set; }
        public string Message { get; set; }
        public string ImageUrl { get; set; }
        public DateTime NotificationTime { get; set; }
        public bool Deleted { get; set; }
        public string NotificationCategory { get; set; }
        public string NotificationType { get; set; }
        public string notify_type { get; set; }
        public int ObjectId { get; set; }
        public int NotificationId { get; set; }
        //public int Views { get; set; }
        public int IsView { get; set; }
        public int TotalCount { get; set; }
    }
}
