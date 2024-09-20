using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.Transaction.AutoNotification
{
    public class AutoNotificationVM
    {
        public long Id { get; set; }
        public int? CityId { get; set; }
        public int? WarehouseId { get; set; }
        public int? ClusterId { get; set; }
        public string ANType { get; set; }//Promotional/Event
        public string ANEventType { get; set; }//Schedule/Transaction
        public long? ANScheduleMasterId { get; set; }
        public long? ANFrequencyMasterId { get; set; }
        public string DbObjectName { get; set; }
        public string EntityName { get; set; }

        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        public int? RecurEvery { get; set; }

        public bool IsPublish { get; set; }
        public int? PublishBy { get; set; }
        public DateTime? PublishDate { get; set; }
        public string TextMessage { get; set; }
        public string FCMNotification { get; set; }
        public string AutoDialAudioFile { get; set; }
        public string AutoDialUrl { get; set; }

        public bool IsReminder { get; set; }
        public int ReminderCount { get; set; }
        public int ReminderDays { get; set; }
        public string AutoDialAudioText { get; set; }
        public string SqlQuery { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public long SendToDetailFiedId { get; set; }

        public string EntityAction { get; set; }
        public ANScheduleMasterVM ANScheduleMaster { get; set; }
        public ANFrequencyMasterVM ANFrequencyMaster { get; set; }

        public ICollection<AutoNotificationConditionVM> AutoNotificationConditions { get; set; }


        public bool? IsSupplierNotification { get; set; }
        public bool? IsCustomerNotification { get; set; }
        public bool? IsPeopleNotification { get; set; }


        public string AutoNotificationTitle { get; set; }
        public string ClassName { get; set; }

    }
}
