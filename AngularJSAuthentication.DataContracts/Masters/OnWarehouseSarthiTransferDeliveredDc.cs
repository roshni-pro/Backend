﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.Masters
{
    public class OnWarehouseSarthiTransferDeliveredDc
    {
        public int SourceWarehouseId { get; set; }
        public int DestinationWarehouseId { get; set; }
        public int ItemMultiMRPId { get; set; }
        public int Qty { get; set; }
        public int? ReceiveQty { get; set; }
        public int? DamageQty { get; set; }
        public int? ExpiryQty { get; set; }
        public long TransferWHOrderDispatchedDetailsId { get; set; }
        public int UserId { get; set; }
        public int TransferOrderId { get; set; }
        public bool IsFreeStock { get; set; }
        public string Reason { get; set; }
    }
}
