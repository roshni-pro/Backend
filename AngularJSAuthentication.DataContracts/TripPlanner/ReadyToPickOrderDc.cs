using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.TripPlanner
{
    public class ReadyToPickOrderDc
    {
        public int OrderId { get; set; }
        public int WarehouseId { get; set; }
        public DateTime ReadyToPickDate { get; set; }
    }

    public class PickOrderListDc
    {
        public int OrderId { get; set; }
        public double OrderAmount { get; set; }
        public DateTime OrderDate { get; set; }
        public string Status { get; set; }
        public DateTime Deliverydate { get; set; }
        public bool IsAlreadyHold { get; set; }
        public string Skcode { get; set; }
        public string CustomerName { get; set; }
        public string ShopName { get; set; }
        public string BillingAddress { get; set; }
        public string ShippingAddress { get; set; }
        public DateTime? PrioritizedDate { get; set; }
        public DateTime? DispatchDate { get; set; }
    }
    public class TotalPickOrderListDc
    {
        public int TotalCount { get; set; }
        public List<PickOrderListDc> PickOrderListDcs { get; set; }
    }

    public class InputPickOrder
    {
        public int WarehouseId { get; set; }
        public int skip { get; set; }
        public int take { get; set; }
        public string Keyword { get; set; }
        public DateTime? date { get; set; }
        public int userId { get; set; }
    }
    public class HoldOrderOutputDc
    {
        public bool IsSuccess { get; set; }
        public DateTime? DeliveryDate { get; set; }
        public DateTime? DispatchDate { get; set; }
    }
}
