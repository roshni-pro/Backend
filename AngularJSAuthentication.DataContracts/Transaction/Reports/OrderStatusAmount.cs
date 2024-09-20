namespace AngularJSAuthentication.DataContracts.Transaction.Reports
{
    public class OrderStatusAmount
    {
        public int WarehouseId { get; set; }
        public string Status { get; set; }
        public double TotalAmount { get; set; }
        public string WarehouseName { get; set; }
    }

    public class OrderSummaryStatusWise
    {
        public string Status { get; set; }
        public string WarehouseName { get; set; }
        public int WarehouseId { get; set; }
        public int OrderId { get; set; }
        public double? DispatchAmount { get; set; }
    }
}
