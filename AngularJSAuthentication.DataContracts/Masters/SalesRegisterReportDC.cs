using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.Masters
{
    public class SalesRegisterReportDC
    {
        public int OrderId { get; set; }
        public string ClusterName { get; set; }
        public string OrderType { get; set; }
        public string Skcode { get; set; }
        public string StoreName { get; set; }
        public string SalesPerson { get; set; }
        public string invoice_no { get; set; }

        public double invoiceAmount { get; set; }
        public DateTime? DispatchDate { get; set; }
        public DateTime? OrderDate { get; set; }
        public DateTime? DeliveredDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public string RetailerName { get; set; }
        public string ShopName { get; set; }
        public string Mobile { get; set; }
        public int ItemId { get; set; }
        public int ItemMultiMRPId { get; set; }
        public string itemname { get; set; }
        public string CategoryName { get; set; }
        public string BrandName { get; set; }
        public string WarehouseName { get; set; }
        public double UnitPrice { get; set; }
        public int Quantity { get; set; }
        public double TotalAmt { get; set; }
        public int DispatchedQuantity { get; set; }
        public double DispatchedTotalAmt { get; set; }
        public string Status { get; set; }
        public string DboyName { get; set; }
        public string HSNCode { get; set; }
        public string GSTno { get; set; }
        public int? AssignmentNo { get; set; }
        public Double AmtWithoutTaxDisc { get; set; }
        public Double AmtWithoutAfterTaxDisc { get; set; }
        public double TaxPercentage { get; set; }
        public Double TaxAmmount { get; set; }
        public double SGSTTaxPercentage { get; set; }
        public Double SGSTTaxAmmount { get; set; }
        public double CGSTTaxPercentage { get; set; }
        public Double CGSTTaxAmmount { get; set; }
        public double TotalCessPercentage { get; set; }
        public double CessTaxAmount { get; set; }
        public DateTime? PocCreditNoteDate { get; set; }
        public string PocCreditNoteNumber { get; set; }
        public double? WalletAmount { get; set; }
        public double? BillDiscountAmount { get; set; }
        public string IRNNo { get; set; }
        public string comment { get; set; }
        public string comments { get; set; }
        public string DeliveryCanceledComments { get; set; }
        public double TCSAmount { get; set; }
        public string EwayBillNumber { get; set; }
    }
}
