using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.Transaction
{
    public class DeliveryMappingstatusCount
    {
        public int NumberofOrder { get; set; }
        public int NotCompletedOrder { get; set; }
        public int DeliveredOrder { get; set; }
        public int RedispatchOrder { get; set; }
        public int CancelledOrder { get; set; }
    }
    public class ClusterWiseOrderCount
    {
        public int ClusterId { get; set; }
        public string ClusterName { get; set; }
        public int OrderCount { get; set; }

    }

    public class ResponseDc
    {
        public List<OrderDC> listOrder { get; set; }
        public int TotalCount { get; set; }
    }
    public class OrderDC
    {
        public int OrderId { get; set; }
        public double GrossAmount { get; set; }
        public string invoice_no { get; set; }
        public string Skcode { get; set; }
        public string Customerphonenum { get; set; }
        public string ShopName { get; set; }
        public string BillingAddress { get; set; }
        public DateTime Deliverydate { get; set; }
        public int ReDispatchCount { get; set; }

    }
    public enum AssignmentTypeEnum
    {
        Planned = 1,
        Redispatch = 2,
        Manual = 3,
        Return = 4,
    }

    public class TripDetailsDC
    {          
        public int OrderCount { get; set; }
        public int CustomerCount { get; set; }
        public double TotalWeight { get; set; }
        public int TripCount { get; set; }
    }
    public class OrderstatusCountListDC
    {
        public int OrderId { get; set; }
        public string Status { get; set; }

    }
    public class VehicleLiveDetailslistDc
    {
        public int CurrentStatus { get; set; }

    }
    public class VehicleLiveDetailsDc
    {
        public SingleVehicle OnDuty { get; set; }
        public SingleVehicle NotStarted { get; set; }
        public SingleVehicle Intransit { get; set; }
        public SingleVehicle OnBreak { get; set; }
        public SingleVehicle Delivering { get; set; }
        public SingleVehicle TripEnd { get; set; }
    }

    public class SingleVehicle
    {
        public int Id { get; set; }
        public int Count { get; set; }
    }

    public class TripListDc
    {
        public long? TripId { get; set; }
        public int TripNumber { get; set; }

        public TripDetailDc TripDetails { get; set; }
    }
    public class TripDetailDc
    {
        public int TripNumber { get; set; }
        public string VehicleName { get; set; }
        //public string AgentName { get; set; }
        //public string AgentMobileNumber { get; set; }
        public string DriverName { get; set; }
        public string DriverMobileNumber { get; set; }
        public string DeliveryBoyName { get; set; }
        public string DeliveryBoyNumber { get; set; }
    }
}
