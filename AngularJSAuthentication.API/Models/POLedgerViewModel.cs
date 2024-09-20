using System;

namespace AngularJSAuthentication.API.Models
{
    public class POLedgerViewModel
    {
        public int PurchaseOrderId { get; set; }
        public int SupplierId { get; set; }
        public double? GR1_Amount { get; set; }
        public DateTime? GR1_Date { get; set; }
        public int? GR1PersonID { get; set; }
        public double? discount1 { get; set; }

        public double? GR2_Amount { get; set; }
        public DateTime? GR2_Date { get; set; }
        public int? GR2PersonID { get; set; }
        public double? discount2 { get; set; }

        public double? GR3_Amount { get; set; }
        public DateTime? GR3_Date { get; set; }
        public int? GR3PersonID { get; set; }
        public double? discount3 { get; set; }

        public double? GR4_Amount { get; set; }
        public DateTime? GR4_Date { get; set; }
        public int? GR4PersonID { get; set; }
        public double? discount4 { get; set; }

        public double? GR5_Amount { get; set; }
        public DateTime? GR5_Date { get; set; }
        public int? GR5PersonID { get; set; }
        public double? discount5 { get; set; }
    }


    public class IRLedgerViewModel
    {
        public int? SupplierId { get; set; }
        public int? PurchaseOrderId { get; set; }
        public string InVoiceNumber { get; set; }
        public double? CreditInVoiceAmount { get; set; }
        public DateTime? CreatedDate { get; set; }

    }
}