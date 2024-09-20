
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AngularJSAuthentication.DataContracts.Transaction.AutoNotification
{
    public class AutoNotificationListVM
    {
        public long Id { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool IsPublish { get; set; }
        public int? PublishBy { get; set; }
        public DateTime? PublishDate { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public int CreatedBy { get; set; }
        public int? ModifiedBy { get; set; }

        public string EntityName { get; set; }
        public string AutoNotificationTitle { get; set; }


    }
}
