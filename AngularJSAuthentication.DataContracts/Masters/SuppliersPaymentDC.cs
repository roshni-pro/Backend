using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.Masters
{
    public class SupplierPaymentDC
    {
        [Key]
        public int SupplierId { get; set; }
        public int? WarehouseId { get; set; }
        public int Amount { get; set; }
        public int? Cityid { get; set; }
        public int? POId { get; set; }
    
    }
}
