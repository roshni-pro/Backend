using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.Shared
{
   public class IRMasterViewDc
    {
        public double PurchaseAmount { get; set; }
        public double? TotalTaxAmount { get; set; }
        public double? CGSTAmount { get; set; }
        public double? IGSTAmount { get; set; }
        public double? SGSTAmount { get; set; }
        public double? Discount { get; set; }
        public double TotalAmountRemaining { get; set; }
        public double? OtherAmount { get; set; }
        public double? ExpenseAmount { get; set; }
        public double? RoundofAmount { get; set; }
        public DateTime? InvoiceDate { get; set; }
      
    }
}
