namespace AngularJSAuthentication.DataContracts.Masters
{
    public class CurrentStockMinDc
    {
        public int WarehouseId { get; set; }
        public string ItemNumber { get; set; }
        public int ItemMultiMRPId { get; set; }
        public int CurrentInventory { get; set; }
        public bool IsFreeItem { get; set; }
        public bool Deleted { get; set; }
    }

}
