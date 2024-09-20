using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.Transaction.AutoNotification
{
    public class AutoNotificationConditionVM
    {
        public long AutoNotificationId { get; set; }
        public string FieldName { get; set; }
        public string DbObjectFieldName { get; set; }
        public string FieldType { get; set; }
        public string OperatorSign { get; set; }
        public string Value1 { get; set; }
        public string Value2 { get; set; }
        public string SqlQuery { get; set; }
        public AutoNotificationVM AutoNotification { get; set; }
    }
}
