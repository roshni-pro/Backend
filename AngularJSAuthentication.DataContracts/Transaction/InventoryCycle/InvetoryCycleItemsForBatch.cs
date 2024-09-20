namespace AngularJSAuthentication.DataContracts.Transaction.InventoryCycle
{
    public class InvetoryCycleItemsForBatch
    {       
        public int ItemMultiMRPId { get; set; }
        public int WarehouseId { get; set; }
        public string cat { get; set; }

    }
    public class InventCycleMasterForWarehouse
    {
        public int WarehouseId { get; set; }
        public string Classification { get; set; }
        public long InventoryCycleMasterId { get; set; }

    }
}
