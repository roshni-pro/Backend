using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.Masters
{
   public  class TDSBILLtoBill
    {
       public string Remark { get; set; }
       public string RefNo { get; set; }
      
       public int PurchaseOrderId { get; set; }
       public string IRID { get; set; }
        public string SupplierName { get; set; }
        public string SUPPLIERCODES { get; set; }
        public string GSTN { get; set; }
        public DateTime PaymentDate { get; set; }
        public double paymentAmount { get; set; }
        public double TDSAmount { get; set; }
        public string WarehouseName { get; set; }
        public decimal Rate_of_TDS { get; set; }
        public DateTime entrydate { get; set; }
    }
}
