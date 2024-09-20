using System;

namespace AngularJSAuthentication.DataContracts.Transaction.OrderProcess
{
    public class OrdersForAutoPick
    {
        public long Id { get; set; }
        public int CustomerId { get; set; }
        public int OrderId { get; set; }
        public int ItemMultiMrpId { get; set; }
        public int WarehouseId { get; set; }
        public int OrderDetailsId { get; set; }
        public int Qty { get; set; }
        public double UnitPrice { get; set; }
        public bool IsDispatchedFreeStock { get; set; }
        public bool IsFreeItem { get; set; }
        public bool IsDelayDelivery { get; set; }
        public int CurrentInventory { get; set; }
        public int FreeInventory { get; set; }
        public int RequiredQty { get; set; }
        public double OrderAmount { get; set; }
        public double ItemAmount { get; set; }
        public string ItemColor { get; set; }
        public string OrderColor { get; set; }
        public int DaysSinceOrdered { get; set; }
        public int CustomerSubType { get; set; } // 1-Small, 2-Med, 3-Big
        public bool IsSelfOrder { get; set; }
        public int IsOnline { get; set; }
        public int MOQ { get; set; }
        public int FulfilledQty { get; set; }
        public bool IsCut { get; set; }
        public bool IsFulfilled
        {
            get;
            set;
        }
        public bool IsLessInventory { get; set; }

        public bool IsProcessed { set; get; }
        public string ErrorMsg { set; get; }
        public DateTime CreatedDate { get; set; }
        public int IsDigitalOrder { get; set; }
        public DateTime? PrioritizedDate { get; set; }
    }

    public class ItemsForAutoPick
    {

        public int ItemMultiMrpId { get; set; }
        public int WarehouseId { get; set; }
        public int CurrentInventory { get; set; }
        public int OriginalInventory { get; set; }
        public int FreeInventory { get; set; }
        public int OriginalFreeInventory { get; set; }

        public int RequiredQty { get; set; }
        public int FulfilledQty { get; set; }
        public int CustomerCount { get; set; }
        public int OrderCount { get; set; }
    }
}
