using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.InvoiceReceipt
{
    public class InvoiceReceiptImageDC
    {
        public int PurchaseOrderId { get; set; }
        public int GrSerialNumber { get; set; }
        public string InvoiceImage { get; set; } //InvoiceImage image
        public string InvoiceNumber { get; set; } //InvoiceNumber
        public DateTime? InvoiceDate { get; set; }

        public string GSTN { get; set; } //Po Gstn number 

    }


}
