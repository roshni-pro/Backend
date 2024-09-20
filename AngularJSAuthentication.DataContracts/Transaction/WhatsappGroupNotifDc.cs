using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.Transaction
{
    public class WhatsappGroupNotifDc
    {
        public long Id { get; set; }
        public long TemplateId { get; set; }
        public string CityIds { get; set; }
        public string WarehouseIds { get; set; }
        public string GroupIds { get; set; }
        public string NotificationName { get; set; }
        public bool IsSend { get; set; }

    }

    public class WhatsappGroupNotifyList
    {
        public List<WhatsappGroupNotifDc> whatsappGroupNotifDcs { get; set; }
        public int TotalCount { get; set; }
    }
    public class MsgDc
    {
        public string Msg { get; set; }
    }
    public class WhatsAppGroupCustomer
    {
        public string Skcode { get; set; }
        public int CustomerId { get; set; }
    }
}
