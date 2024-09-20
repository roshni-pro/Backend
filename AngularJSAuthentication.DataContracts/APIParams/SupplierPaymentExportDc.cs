using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.APIParams
{
    public class SupplierPaymentExportDc
    {
        public string Date { get; set; }
        public string Particulars { get; set; }
        public string UTRNumber { get; set; }
        public int Amount { get; set; }
        public string Name { get; set; }
        public string ShopName { get; set; }
        public string SupplierCode { get; set; }
        public string WarehouseName { get; set; }
        public string CityName { get; set; }
        public long PurchaseOrderId { get; set; }


    }
}
