using System.Collections.Generic;

namespace AngularJSAuthentication.DataContracts.Transaction
{
    public class CpMatrixProductDetailDc
    {
        public int ItemMultiMrpId { get; set; }
        public string ItemName { get; set; }
        public string ItemNumber { get; set; }
        public double OrderAmount { get; set; }
        public List<CustomerDetailDc> Customers { get; set; }
    }

    public class CustomerDetailDc
    {
        public int CustomerId { get; set; }
        public string SkCode { get; set; }
        public string ShopName { get; set; }
        public string CustomerName { get; set; }
        public int WarehouseId { get; set; }
        public string WarehouseName { get; set; }
        public double OrderAmount { get; set; }
        public double Delta { get; set; }
    }


    public class CpMatrixGroupedItemsDc
    {
        public int CustomerId { get; set; }
        public int ItemMultiMrpId { get; set; }
        public double Delta { get; set; }
    }
}
