using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.ROC
{
    public class RocDC
    {
        public int ItemMultiMrpId { get; set; }
        public string ItemName { get; set; }
        public string ItemCode { get; set; }
        public string Brand { get; set; }
        public string Category { get; set; }
        public string WarehouseName { get; set; }
        public string BuyerName { get; set; }
        public double MRP { get; set; }
        public double ItemTaxPercent { get; set; }
        public int BrandId { get; set; }
        public int BuyerId { get; set; }
        public int CategoryId { get; set; }
        public int WarehouseId { get; set; }
        public double FrontMargin { get; set; }
        public double Discounts { get; set; }
        public double DamageMovement { get; set; }//
        public double DamageSale { get; set; }
        public double Opening { get; set; }
        public double Closing { get; set; }
        public double AverageInventoryAmount { get; set; }
        public double InvoiceDiscount { get; set; }
        public double ROCPercent { get; set; }
        public double ROCValue { get; set; }
        public DateTime ForMonthData { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
    }
    public class RocMsgDc
    {
        public bool Status { get; set; }
        public string Message { get; set; }
    }

    public class ItemTaggingInsertDc
    {
        public int ItemMultiMRPId { get; set; }
        public int WarehouseId { get; set; }
        public long ROCBucketId { get; set; }
        public DateTime ForMonthData { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
    }

    public class ItemWarehouseDc
    {
        public int ItemMultiMRPId { get; set; }
        public int WarehouseId { get; set; }
    }
    public class ItemWarehouseData
    {
        public int ItemMultiMRPId { get; set; }
        public int WarehouseId { get; set; }
        public int Tag { get; set; }
    }

    public class RocReportDataDc
    {
        public int ItemMultiMrpId { get; set; }
        public string ItemName { get; set; }
        public string ItemCode { get; set; }
        public string Brand { get; set; }
        public string Category { get; set; }
        public string WarehouseName { get; set; }
        public string BuyerName { get; set; }
        public double MRP { get; set; }
        public double ItemTaxPercent { get; set; }
        public int BrandId { get; set; }
        public int BuyerId { get; set; }
        public int CategoryId { get; set; }
        public int WarehouseId { get; set; }
        public double FrontMargin { get; set; }
        public double Discounts { get; set; }
        public double DamageMovement { get; set; }
        public double DamageSale { get; set; }
        public double Opening { get; set; }
        public double Closing { get; set; }
        public double AverageInventoryAmount { get; set; }
        public double InvoiceDiscount { get; set; }
        public double ROCPercent { get; set; }
        public double ROCValue { get; set; }
        public double NetRevenue { get; set; }
    }
}
