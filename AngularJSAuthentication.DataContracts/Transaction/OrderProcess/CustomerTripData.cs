using System.Collections.Generic;

namespace AngularJSAuthentication.DataContracts.Transaction.OrderProcess
{
    public class CustomerTripDataFromDb
    {
        public long TripId { get; set; }
        public int CustomerId { get; set; }
        public long TripPlannerVehicleId { get; set; }
        public int DBoyId { get; set; }
        public string DBoyName { get; set; }
        public string DboyMobile { get; set; }
        public string DboyProfilePic { get; set; }
        public string DeliveryBoyRating { get; set; }
        public int OrderId { get; set; }
        public string ShippingAddress { get; set; }
        public double Lat { get; set; }
        public double Lng { get; set; }
        public double OrderAmount { get; set; }
        public double PayableAmount { get; set; }
        public bool IsPaid { get; set; }
        public string OTP { get; set; }

    }

    public class CustomerTrip
    {
        public string CollectionName { get; set; }
        public long TripId { get; set; }
        public long TripPlannerVehicleId { get; set; }

        public int CustomerId { get; set; }
        public int DBoyId { get; set; }
        public string DBoyName { get; set; }
        public string DboyMobile { get; set; }
        public string DboyProfilePic { get; set; }
        public string DeliveryBoyRating { get; set; }
        public string ShippingAddress { get; set; }
        public double Lat { get; set; }
        public double Lng { get; set; }
        public List<TripOrders> Orders { get; set; }
    }

    public class TripOrders
    {
        public int OrderId { get; set; }
        public double OrderAmount { get; set; }
        public double PayableAmount { get; set; }
        public bool IsPaid { get; set; }
        public string OTP { get; set; }
    }
}
