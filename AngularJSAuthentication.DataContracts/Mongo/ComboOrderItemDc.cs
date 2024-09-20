using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.Mongo
{
    public class ComboOrderDc
    {
        public int CustomerId { get; set; }
        public int ExecutiveId { get; set; }
        public List<ComboOrderItemDc> ComboOrderItemDcs { get; set; }
    }

    public class ComboOrderItemDc
    {
        public string Id { get; set; }
        public int qty { get; set; }
    }
}
