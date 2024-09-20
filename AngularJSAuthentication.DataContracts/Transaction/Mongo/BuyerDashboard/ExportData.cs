namespace AngularJSAuthentication.DataContracts.Transaction.Mongo.BuyerDashboard
{
    public class ExportData
    {
        public string ItemNumber { get; set; }
        public int ItemMultiMrpId { get; set; }
        public string ItemName { get; set; }
        public int BrandId { get; set; }
        public string BrandName { get; set; }
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public int WarehouseId { get; set; }
        public string WarehouseName { get; set; }
        public int BuyerId { get; set; }
        public string BuyerName { get; set; }
        public double? MRP { get; set; }
        public double? TaxPercentage { get; set; }

        public double Value { get; set; }
    }
}
