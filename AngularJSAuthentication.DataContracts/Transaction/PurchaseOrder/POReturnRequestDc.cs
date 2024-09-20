using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.Transaction.PurchaseOrder
{
    public class POReturnRequestDc
    {

        public int PurchaseOrderId { get; set; }
        public string Status { get; set; }  //Pending Cancelled Approved
        public long Id { get; set; }
        public long ItemId { get; set; }
        public string Guid { get; set; }
        public int ApprovedBy { get; set; }
        public int WarehouseId { get; set; }
        public string CancelType { get; set; } // PO GR IR 
    }

    public class POReturnRequestPageDc
    {
        public string POReturnRequestStatus { get; set; }
        public long POReturnRequestId { get; set; }
        public long ItemId { get; set; }
        public string RequestedBy { get; set; }
        public DateTime RequestedDate { get; set; }
        public string ApprovedBy { get; set; }
        public DateTime? ApprovedDate { get; set; }
        public string CancelType { get; set; }
        public double? Amount { get; set; }
        public string Status { get; set; }   //   PO/GR/IR   Status
        public int totalRecordCount { get; set; }
        public string  RefNumber { get; set; }


    }
}





