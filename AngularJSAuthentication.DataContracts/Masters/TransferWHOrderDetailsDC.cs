using System;
using System.Collections.Generic;

namespace AngularJSAuthentication.DataContracts.Masters
{
    public class TransferWHOrderDetailsDC
    {
        public int TransferOrderDetailId { get; set; }
        public int TransferOrderId { get; set; }
        public int CompanyId { get; set; }
        public int? WarehouseId { get; set; }
        public string WarehouseName { get; set; }
        public string Status { get; set; }
        public int? RequestToWarehouseId { get; set; }
        public string RequestToWarehouseName { get; set; }
        public int ItemId { get; set; }
        public string ItemNumber { get; set; }
        public string ItemHsn { get; set; }
        public double? NPP { get; set; }
        public int TotalQuantity { get; set; }
        public int? DispatchQuantity { get; set; }
        public DateTime CreationDate { get; set; }
        public string itemname { get; set; }
        public string itemBaseName { get; set; }
        public int ItemMultiMRPId { get; set; }
        public double? TotalTaxPercentage { get; set; }
        public double MRP { get; set; }
        public string UnitofQuantity { get; set; }
        public string UOM { get; set; }//Unit of masurement like GM Kg        
        public double PriceofItem { get; set; }
        public double TotalPrice { get; set; }
    }

    public class TransferWHOrderDispachedDetailsGetDC
    {
        public int TransferOrderDispatchedDetailId { get; set; }
        public int TransferOrderDetailId { get; set; }
        public int TransferOrderId { get; set; }
        public int ItemId { get; set; }
        public string ItemNumber { get; set; }
        public string ItemHsn { get; set; }
        public double? TotalTaxPercentage { get; set; }
        public double? NPP { get; set; }
        public int TotalQuantity { get; set; }
        public int? DispatchQuantity { get; set; }
        public int? ReceiveQuantity { get; set; }
        public int? DamageQuantity { get; set; }
        public int? ExpiryQuantity { get; set; }
        public string itemname { get; set; }
        public string itemBaseName { get; set; }
        public int ItemMultiMRPId { get; set; }
        public double MRP { get; set; }
        public string UnitofQuantity { get; set; }
        public string UOM { get; set; }
        public double PriceofItem { get; set; }
        public double TotalPrice { get; set; }

    }
    public class TransferWHOrderDispachedDetailsDC
    {
        public int TransferOrderDetailId { get; set; }
        public int DispatchedQuantity { get; set; }
    }

    public class RevokePostDC
    {
        public int TransferOrderId { get; set; }
        public int CreatedBy { get; set; }
        public string RejectRession { get; set; }
    }
    public class RejectePostDC
    {
        public int TransferOrderId { get; set; }
        public int CreatedById { get; set; }
        public string RejectRession { get; set; }
    }
    public class ApprovedPostDC
    {
        public int TransferOrderId { get; set; }
        public int CreatedById { get; set; }
    }
    public class RejecteByReceiverDC
    {
        public int TransferOrderId { get; set; }
        public int CreatedById { get; set; }
        public string RejectRession { get; set; }
    }
    public class ApprovedByReceiverDC
    {
        public int TransferOrderId { get; set; }
        public int CreatedById { get; set; }
    }
    public class ApprovedRejectedByReceiverOrderDC
    {
        public int TransferOrderId { get; set; }
        public int CreatedById { get; set; }
    }
    public class TransferOrderReceiveDC
    {
        public int TransferOrderId { get; set; }
        public int CreatedById { get; set; }
        public List<TransferOrderReceiveDetailDC> transferOrderReceiveDetailDCs { get; set; }
    }
    public class TransferOrderReceiveDetailDC
    {
        public int TransferOrderDetailId { get; set; }
        public int DispachedQuantity { get; set; }
        public int ReceiveQuantity { get; set; }
        public int DamageQuantity { get; set; }
        public int ExpiryQuantity { get; set; }
    }

}
