using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.Transaction.PurchaseOrder
{
    public class PORequestPaymentDc
    {
        public long BankLedgerId { get; set; }
        public String RefNo { get; set; }
        public string Remark { get; set; }
        public DateTime PaymentDate { get; set; }
        public List<POPaymentDetailsDc> IRPaymentDetailList { get; set; }
    }


    public class POPaymentDetailsDc
    {
        public int PurchaseOrderId { get; set; }
        public int TotalAmount { get; set; }

    }

    public class PORequestPaymentPage
    {
        public int TotalRecordCount { get; set; }
        public List<PORequestPaymentDisplayDc> RowList { get; set; }
    }

    public class PORequestPaymentDisplayDc
    {
        public int PurchaseOrderId { get; set; }
        public int TotalAmount { get; set; }
        public string SupplierName { get; set; }
        public DateTime? CreationDate { get; set; }
    }




    public class PORequestPager
    {
        public int Skip { get; set; }
        public int Take { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int? SupplierId { get; set; }
        public string Keyword { get; set; }

    }
}
