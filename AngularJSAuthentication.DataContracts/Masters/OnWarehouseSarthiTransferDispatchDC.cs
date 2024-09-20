using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.Masters
{
   public class OnWarehouseSarthiTransferDispatchDC
    {
        public int SourceWarehouseId { get; set; }
        public int ItemMultiMRPId { get; set; }
        public int Qty { get; set; }
        public long TransferWHOrderDispatchedDetailsId { get; set; }
        public int UserId { get; set; }
        public int TransferOrderId { get; set; }
        public bool IsFreeStock { get; set; }
        public string Reason { get; set; }
    }
    public class OnWarehouseSarthiTransferDispatchedRejectDC
    {
        public int SourceWarehouseId { get; set; }
        public int ItemMultiMRPId { get; set; }
        public int Qty { get; set; }
        public long TransferWHOrderDispatchedDetailsId { get; set; }
        public int UserId { get; set; }
        public int TransferOrderId { get; set; }
        public bool IsFreeStock { get; set; }
        public string Reason { get; set; }
    }
}
