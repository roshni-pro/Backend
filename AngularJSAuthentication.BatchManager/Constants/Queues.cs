using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.BatchManager.Constants
{
    public class Queues
    {
        public static string BatchCodeQueue { get { return "BatchCode"; } }
        public static string OrderInvoiceQueue { get { return "OrderInvoice"; } }
        public static string BackendOrderQueue { get { return "BackendOrder"; } }
        public static string ZilaOrderQueue { get { return "ZilaOrderQueue"; } }

        public static string itemMqName
        {
            get
            {
                return "ItemElasticItemId";
            }
        }

        public static string itemLimitMqName
        {
            get
            {
                return "ItemElasticItemMultiMrpId";
            }
        }

    }
}
