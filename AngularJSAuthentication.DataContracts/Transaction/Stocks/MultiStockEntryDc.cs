using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.Transaction.Stocks
{
    public class RTDStockEntryDc
    {
        public int WarehouseId { get; set; }
        public int ItemMultiMRPId { get; set; }
        public int Qty { get; set; }
        public int OrderDispatchedDetailsId { get; set; }
        public int UserId { get; set; }
        public int OrderId { get; set; }
        public bool IsFreeStock { get; set; }
        public bool IsDispatchFromPlannedStock { get; set; }
        public string RefStockCode { get; set; }


    }
    public class Stock_OnPickedDc
    {
        public int WarehouseId { get; set; }
        public int ItemMultiMRPId { get; set; }
        public int Qty { get; set; }
        public int OrderDispatchedDetailsId { get; set; }
        public int UserId { get; set; }
        public int OrderId { get; set; }
        public bool IsFreeStock { get; set; }
        public string RefStockCode { get; set; }
    }
    public class Picker_OnPickedDc
    {
        public int WarehouseId { get; set; }
        public int ItemMultiMRPId { get; set; }
        public int Qty { get; set; }
        public int OrderDispatchedDetailsId { get; set; }
        public int UserId { get; set; }
        public int OrderId { get; set; }
        public string OrderIds { get; set; }
        public bool IsFreeStock { get; set; }
        public string RefStockCode { get; set; }
    }
    public class POCStockEntryDc
    {
        public int WarehouseId { get; set; }
        public int ItemMultiMRPId { get; set; }
        public int Qty { get; set; }
        public int OrderDispatchedDetailsId { get; set; }
        public int UserId { get; set; }
        public int OrderId { get; set; }
        public string RefStockCode { get; set; }
    }
    public class OnDeliveredStockEntryDc
    {
        public int WarehouseId { get; set; }
        public int ItemMultiMRPId { get; set; }
        public int Qty { get; set; }
        public int OrderDispatchedDetailsId { get; set; }
        public int UserId { get; set; }
        public int OrderId { get; set; }
        public string RefStockCode { get; set; }
 
    }
    public class OnDeliveredOnAssignmentRejectDC
    {
        public int WarehouseId { get; set; }
        public int ItemMultiMRPId { get; set; }
        public int Qty { get; set; }
        public int OrderDispatchedDetailsId { get; set; }
        public int UserId { get; set; }
        public int OrderId { get; set; }
        public string RefStockCode { get; set; }
        public string SourceTableName { get; set; } //-- DeliveryCancelStocks, DeliveryRedispatchStocks
    }
    public class ONDeliveryCancelOnAssignmentRejectDc
    {
        public int WarehouseId { get; set; }
        public int ItemMultiMRPId { get; set; }
        public int Qty { get; set; }
        public int OrderDispatchedDetailsId { get; set; }
        public int UserId { get; set; }
        public int OrderId { get; set; }
        public string RefStockCode { get; set; }
    }


    public class OnGRNRequestCStockEntryDc
    {
        public int WarehouseId { get; set; }
        public int ItemMultiMRPId { get; set; }
        public int Qty { get; set; }
        public int DamageQty { get; set; }
        public int ExpiryQty { get; set; }
        public int ShortQty { get; set; }

        public long GoodReceivedDetailsId { get; set; }
        public int UserId { get; set; }
        public int PurchaseOrderID { get; set; }
        public bool IsFreeStock { get; set; }
    }
    public class OnGRNApproveCStockEntryDc
    {
        public int WarehouseId { get; set; }
        public int ItemMultiMRPId { get; set; }
        public int Qty { get; set; }
        public long GoodReceivedDetailsId { get; set; }
        public int UserId { get; set; }
        public int PurchaseOrderID { get; set; }
        public bool IsFreeStock { get; set; }
        public int DamageQty { get; set; }
        public int ExpiryQty { get; set; }
        public int ShortQty { get; set; }

        public string ManualReason { get; set; }

    }
    public class OnGRNPendingRejectDc
    {
        public int WarehouseId { get; set; }
        public int ItemMultiMRPId { get; set; }
        public int Qty { get; set; }
        public long GoodReceivedDetailsId { get; set; }
        public int UserId { get; set; }
        public int PurchaseOrderID { get; set; }
        public bool IsFreeStock { get; set; }
        public int DamageQty { get; set; }
        public int ExpiryQty { get; set; }
        public int ShortQty { get; set; }

    }
    public class OnWarehouseTransferDispatchDC
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
    public class OnWarehouseTransferDispatchedRejectDC
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
    public class OnWarehouseTransferDeliveredDc
    {
        public int SourceWarehouseId { get; set; }
        public int DestinationWarehouseId { get; set; }
        public int ItemMultiMRPId { get; set; }
        public int Qty { get; set; }
        public long TransferWHOrderDispatchedDetailsId { get; set; }
        public int UserId { get; set; }
        public int TransferOrderId { get; set; }
        public bool IsFreeStock { get; set; }
        public string Reason { get; set; }
    }
    public class OnMultiMRPTransferDc
    {
        public int WarehouseId { get; set; }
        public int SourceItemMultiMRPId { get; set; }
        public int DestinationItemMultiMRPId { get; set; }
        public int Qty { get; set; }
        public int UserId { get; set; }
        public string ManualReason { get; set; }

    }
    public class ManualStockUpdateDc
    {
        public string TableName { get; set; }
        public int Qty { get; set; }
        public int UserId { get; set; }
        public int ItemMultiMRPId { get; set; }
        public int WarehouseId { get; set; }
        public long ManualStockUpdateRequestId { get; set; }
        public string StockTransferType { get; set; }
        public string ManualReason { get; set; }
        public string TransactionId { get; set; }
        public bool IsSettleAlso { get; set; }
    }
    public class OnIssuedStockEntryDc
    {
        public int WarehouseId { get; set; }
        public int ItemMultiMRPId { get; set; }
        public int Qty { get; set; }
        public int OrderDispatchedDetailsId { get; set; }
        public int UserId { get; set; }
        public int OrderId { get; set; }
        public string RefStockCode { get; set; }
        public bool IsDeliveryRedispatch { get; set; }

    }
    public class OnShippedStockEntryDc
    {
        public int WarehouseId { get; set; }
        public int ItemMultiMRPId { get; set; }
        public int Qty { get; set; }
        public int OrderDispatchedDetailsId { get; set; }
        public int UserId { get; set; }
        public int OrderId { get; set; }
        public string RefStockCode { get; set; }
    }

    public class OnShippedRejectEntryDc
    {
        public int ItemMultiMRPId { get; set; }
        public int OrderDispatchedDetailsId { get; set; }
        public int OrderId { get; set; }
        public int Qty { get; set; }
        public int UserId { get; set; }
        public int WarehouseId { get; set; }
        public string RefStockCode { get; set; }
    }

    public class OnDeliveryCanceledStockEntryDc
    {
        public int WarehouseId { get; set; }
        public int ItemMultiMRPId { get; set; }
        public int Qty { get; set; }
        public int OrderDispatchedDetailsId { get; set; }
        public int UserId { get; set; }
        public int OrderId { get; set; }
        public bool IsDeliveryRedispatchCancel { get; set; }
        public string RefStockCode { get; set; }

    }
    public class OnDeliveryRedispatchedStockEntryDc
    {
        public int WarehouseId { get; set; }
        public int ItemMultiMRPId { get; set; }
        public int Qty { get; set; }
        public int OrderDispatchedDetailsId { get; set; }
        public int UserId { get; set; }
        public int OrderId { get; set; }
        public bool IsDeliveryCancel { get; set; }
        public string RefStockCode { get; set; }


    }

    public class OnRedispatchOnAssignmentRejectDc
    {
        public int WarehouseId { get; set; }
        public int ItemMultiMRPId { get; set; }
        public int Qty { get; set; }
        public int OrderDispatchedDetailsId { get; set; }
        public int UserId { get; set; }
        public int OrderId { get; set; }
        public string RefStockCode { get; set; }
    }

    public class OnPickedCancelDc
    {
        public int WarehouseId { get; set; }
        public int ItemMultiMRPId { get; set; }
        public int Qty { get; set; }
        public int OrderDispatchedDetailsId { get; set; }
        public int UserId { get; set; }
        public int OrderId { get; set; }
        public bool IsFreeStock { get; set; }
        public string RefStockCode { get; set; }


    }



    public class RTDOnPickedDc
    {
        public int WarehouseId { get; set; }
        public int ItemMultiMRPId { get; set; }
        public int Qty { get; set; }
        public int OrderDispatchedDetailsId { get; set; }
        public int UserId { get; set; }
        public int OrderId { get; set; }
        public bool IsFreeStock { get; set; }
        public bool IsDispatchFromPlannedStock { get; set; }
        public string RefStockCode { get; set; }


    }
}
