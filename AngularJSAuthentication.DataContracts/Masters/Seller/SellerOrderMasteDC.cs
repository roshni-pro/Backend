using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.Masters.Seller
{
    public class SellerOrderMasteSPDC
    {
        public int OrderId { get; set; }
        public double GrossAmount { get; set; }
        public double TaxAmount { get; set; }
        public DateTime CreatedDate { get; set; }
        public string Status { get; set; }
        public DateTime? DeliveredDate { get; set; }
        public DateTime UpdatedDate { get; set; }
        public double? deliveryCharge { get; set; }        
        public int OrderDetailsId { get; set; }
        public string itemname { get; set; }
        public double price { get; set; }
        public double TCSAmount { get; set; }
        public int MinOrderQty { get; set; }
        public double UnitPrice { get; set; }
        public int qty { get; set; }
        public double TotalAmt { get; set; }
        public double TaxAmmount { get; set; }

    }
    public class SellerOrderMasteDC
    {
        public int OrderId { get; set; }
        public double TCSAmount { get; set; }

        public double GrossAmount { get; set; }
        public double TaxAmount { get; set; }
        public DateTime CreatedDate { get; set; }
        public string Status { get; set; }
        public DateTime? DeliveredDate { get; set; }
        public DateTime UpdatedDate { get; set; }
        public double? deliveryCharge { get; set; }
        public List<SellerOrderDetailsDC> sellerOrderDetailsDCs { get; set; }
    }

    public class SellerOrderDetailsDC
    {
        public int OrderDetailsId { get; set; }
        public string itemname { get; set; }
        public double price { get; set; }
        public int MinOrderQty { get; set; }
        public double UnitPrice { get; set; }
        public int qty { get; set; }
        public double TotalAmt { get; set; }
        public double TaxAmmount { get; set; }

    }
}
