namespace AngularJSAuthentication.DataContracts.Transaction.Mongo.CpMatrix
{
    public class CpMatrixGroupedItems
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public int ItemMultiMrpId { get; set; }
        public string ItemName { get; set; }
        public string ItemNumber { get; set; }
        public double TotalPrice { get; set; }
    }
}
