using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.Transaction
{
    public class RTGSOrderDetailsDC
    {
        public string RefNo { get; set; }
        public int OrderId { get; set; }
        public string Skcode { get; set; }
        public double amount { get; set; }
        public string WarehouseName { get; set; }
        public string Deliveryboy { get; set; }
        public string CustMobNo { get; set; }
        public string CustName { get; set; }
        public int ApproveBy { get; set; }
        public int type { get; set; }
        public int CustomerId { get; set; }
        public int Skip { get; set; }
        public int Take { get; set; }
        public string PaymentFrom { get; set; }
        public int TotalRecords { get; set; }
        public List<int> WarehouseIds { get; set; }
        public string ApproverName { get; set; }
        public string PaymentDate { get; set; }
        public int PaymentResponseRetailerAppID { get; set; }

    }
}
