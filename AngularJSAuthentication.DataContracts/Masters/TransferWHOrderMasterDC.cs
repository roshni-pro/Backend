using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.Masters
{
    public class TransferWHOrderMasterDC
    {
        public int TransferOrderId { get; set; }
        public int CompanyId { get; set; }
        public int? WarehouseId { get; set; }
        public string WarehouseName { get; set; }
        public string Status { get; set; }
        public int? RequestToWarehouseId { get; set; }
        public string RequestToWarehouseName { get; set; }
        public DateTime CreationDate { get; set; }
        // public DateTime UpdatedDate { get; set; }
        public string CreatedBy { get; set; }
        // public string UpdateBy { get; set; }
        // public bool IsDeleted { get; set; }
        // public bool IsActivate { get; set; }
        public string RequestToWarehouseCityName { get; set; }
        public int userid { get; set; }
    }
    public class TransferWHOrderMasterDispechedInprogresDC : TransferWHOrderMasterDC
    {
        public string DispechedInProgressBy { get; set; }
        public DateTime? DispechedInProgressDate { get; set; }
    }
    public class TransferWHOrderMasterRejectedFromRecDC : TransferWHOrderMasterDC
    {
        public string RejectedtfromRecBy { get; set; }
        public DateTime? RejectedtfromRecDate { get; set; }
    }
    public class TransferWHOrderMasterDispatchedApprovedDC : TransferWHOrderMasterDC
    {
        public string DispatchedApprovedBy { get; set; }
        public DateTime? DispatchedApprovedDate { get; set; }
    }
    public class TransferWHOrderMasterDeleiveryInprogresDC : TransferWHOrderMasterDC
    {
        public string DeleiveryInProgressBy { get; set; }
        public DateTime? DeleiveryInProgressDate { get; set; }
    }

    public class TransferOrderHistoryDC {
        public int Id { get; set; }
        public int TransferOrderId { get; set; }
        public string Status { get; set; }
        public string WarehouseName { get; set; }
        public string RequestToWarehouseName { get; set; }
        public DateTime CreatedDate { get; set; }
    }
    public class BatchITransferDetailDc
    {
        public int TransferOrderDetailId { get; set; }
        public long StockBatchMasterId { get; set; }//Sudhir 04-10-2022
        public int Qty { get; set; }//Sudhir 04-10-2022
    }
}
