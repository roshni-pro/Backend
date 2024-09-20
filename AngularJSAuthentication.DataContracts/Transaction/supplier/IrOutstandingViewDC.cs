using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.Transaction.supplier
{
   public class IrOutstandingViewDC
    {
            public int Id { get; set; }
            public int? PurchaseOrderId { get; set; }
            public string InvoiceNumber { get; set; }
            public DateTime? InvoiceDate { get; set; }
            public double? TotalAmount { get; set; }
            public string IRStatus { get; set; }
            public int? DueDays { get; set; }
            public int? DifInHours { get; set; }
            public int? DifInHoursForApproval { get; set; }
            public int? DifInHoursForGRN { get; set; }
            public string SupplierCode { get; set; }
            public string SupplierName { get; set; }
            public int? SupplierId { get; set; }
            public DateTime? IRApprovedDate { get; set; }
           public DateTime? GRNDate { get; set; }
    }

        public class IrOutstandingViewListDC
        {
            public int Count { get; set; }
            public List<IrOutstandingViewDC> IrOutstandingList { get; set; }
        }

    }

