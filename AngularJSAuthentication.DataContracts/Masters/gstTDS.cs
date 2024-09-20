using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.Masters
{
    public class gstTDS
    {
     public string Remark { get; set; }
        public string RefNo { get; set; }
        public string SupplierCode { get; set; }
        public int PurchaseOrderId { get; set; }
        public string SupplierName { get; set; }
        public string GSTN { get; set; }
        public DateTime PaymentDate { get; set; }
        public int paymentAmount { get; set; }
        public double TDSAmount { get; set; }
        public string WarehouseName { get; set; }
        public double Rate_of_TDS { get; set; }
        public DateTime entrydate { get; set; }
         }
}
