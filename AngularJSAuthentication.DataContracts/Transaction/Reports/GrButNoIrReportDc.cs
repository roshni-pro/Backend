using System;

namespace AngularJSAuthentication.DataContracts.Transaction.Reports
{
    public class GrButNoIrReportDc
    {
        public int PurchaseOrderId { get; set; }
        public string WarehouseName { get; set; }
        public string Status { get; set; }
        public string BuyerName { get; set; }
        public string SupplierName { get; set; }
        public int ItemMultiMRPId { get; set; }
        public string ItemName { get; set; }
        public int gr1Qty { get; set; }
        public int gr2Qty { get; set; }
        public int gr3Qty { get; set; }
        public int gr4Qty { get; set; }
        public int gr5Qty { get; set; }

        public int ir1Qty { get; set; }
        public int ir2Qty { get; set; }
        public int ir3Qty { get; set; }
        public int ir4Qty { get; set; }
        public int ir5Qty { get; set; }
        public int GrTotalQty { get; set; }
        public int IRTotalQty { get; set; }
        public int GrIrDiff { get; set; }
        public double GRAmount { get; set; }
        public double IrAmount { get; set; }

        public string GRcomment {get;set;}
        public double DiffGrIr { get; set; }
        public int PaymentTerms { get; set; }
        public DateTime? PODate { get; set; }
        public DateTime? GRDate { get; set; }
        public DateTime? InvoiceDate { get; set; }
    }
}
