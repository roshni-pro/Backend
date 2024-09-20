using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.Masters
{
    public class AddUpdateConsumerItemDc
    {
        public int ItemId { get; set; }
        public int WarehouseId { get; set; }
        public int SupplierId { get; set; }
        public int DepoId { get; set; }
        public int BuyerId { get; set; }
        public int ItemMultiMrpId { get; set; }
        public bool IsActive { get; set; }
        public int LimitQty { get; set; }
        public bool LimitIsActive { get; set; }


    }
}
