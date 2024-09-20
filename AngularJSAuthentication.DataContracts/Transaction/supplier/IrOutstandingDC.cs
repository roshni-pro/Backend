using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.Transaction.supplier
{
    public class IrOutstandingDC
    {
        public int Id { get; set; }
        public int? PurchaseOrderId { get; set; }
        public string InvoiceNumber { get; set; }
        public string InvoiceDate { get; set; }  
        public double? TotalAmount { get; set; }
        public string IRStatus { get; set; }
        public int? DueDays { get; set; }
        public int? DifInHours { get; set; }
        public int? DifInHoursForApproval { get; set; }
        public int? DifInHoursForGRN { get; set; }
        public string SupplierCode { get; set; }
        public string SupplierName { get; set; }
        public int? SupplierId { get; set; }
        public int WarehouseId{ get; set; }
        public DateTime? IRApprovedDate { get; set; }
        public DateTime? GRNDate { get; set; }
        public double PaidAmount { get; set; }
        public double SettledAmount { get; set; }
        public double TDSPercentage { get; set; }
        public double PaymentTillDate { get; set; }
        public double TDSAmount { get; set; }
        public DateTime? IrCreationDate { get; set; }
        public string WarehouseName { get; set; }
        public string BuyerName { get; set; }
    }

    public class IrOutstandingListDC
    {
        public int Count { get; set; }
        public List<IrOutstandingDC> IrOutstandingList { get; set; }
    }

    public class IrOutstandingPayment
    {
        public int BankId { get; set; }
        public string BankName { get; set; }
        public string RefNo { get; set; }
        public int TotalAmount { get; set; }
        public double? TotalReaminingAmount { get; set; }
        public string Remark { get; set; }
        public DateTime? PaymentDate { get; set; }
        public int WarehouseId { get; set; }
        public List<IrOutstandingDC> IrOutstandingList { get; set; }
        
    }

}
