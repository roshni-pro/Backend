using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.External
{
    public class UserRatingDc
    {
        public int UserId { set; get; }  //(Sales  , Delivery ) : PeopleId   &  Retailer  2 : CustomerId
        public int AppType { set; get; } // 1 : Sales Rating   , 2: Delivery Rating , 3:  Retailer Rating
        public string AppTypeName { set; get; }
        public int Rating { set; get; }
        public int? OrderId { set; get; }
        public string ShopVisited { set; get; }  // Use only for Sales  , Delivery 
        public List<UserRatingDetailDc> RatingDetails { set; get; }
        public string DisplayName { set; get; }
        public string ProfilePic { set; get; }
        public DateTime OrderedDate { set; get; }
        public bool IsRemoveFront { set; get; }
        public bool IsTrip { set; get; }

    }
    public class UserRatingDetailDc
    {
        public string Detail { set; get; }
    }

   
    #region Rating

    public class DboyRatingOrderDc
    {
        public int UserId { set; get; }
        public int OrderId { set; get; }
        public string DisplayName { set; get; }
        public string ProfilePic { set; get; }
        public DateTime OrderedDate { set; get; }
    }
    public class DeliveryDboyRatingOrderDc
    {
        public int OrderId { set; get; }
        public int CustomerId { set; get; }
        public string Shopimage { set; get; }
        public string ShopName { set; get; }
        public string ShippingAddress { set; get; }
    }
    public class TripDeliveryDboyRatingOrderDc
    {
        public long OrderId { set; get; }
        public int CustomerId { set; get; }
        public string Shopimage { set; get; }
        public string ShopName { set; get; }
        public string ShippingAddress { set; get; }
    }
    public class RatingDboyDC
    {
        public DeliveryDboyRatingOrderDc DeliveryDboyRatingOrder { get; set; }
        public List<UserRatingDc> userRatingDc { get; set; }
    }
    public class TripRatingDboyDC
    {
        public TripDeliveryDboyRatingOrderDc DeliveryDboyRatingOrder { get; set; }
        public List<UserRatingDc> userRatingDc { get; set; }
    }
    public class SalesManRatingOrderDc
    {
        public int UserId { set; get; }
        public int OrderId { set; get; }
        public string DisplayName { set; get; }
        public string ProfilePic { set; get; }
        public DateTime OrderedDate { set; get; }
    }

    #endregion

    public class WarehouseIdDC
    {
        public List<int> WarehouseIds { get; set; }
    }
    public class DboyDC
    {
        public int DboyId { get; set; }
        public string DboyName { get; set; }
    }
    public class DboyRatingFilter
    {
        public int Take { get; set; }
        public int Skip { get; set; }
        public int AppType { get; set; }
        public List<int> DboyIds { get; set; }
        public DateTime? Today { get; set; }
        public DateTime? Start { get; set; }
        public DateTime? End { get; set; }
        public string key { get; set; }
        //public int OrderId { get; set; }
        //public string Skcode { get; set; }
        //public string Mobile { get; set; }
    }
    public class DboyRatingDC
    {
        public long Id { set; get; }
        public int OrderID { set; get; }
        public DateTime OrderDate { set; get; }
        public string DeliveryBoy { set; get; }
        public string CityName { get; set; }
        public string Warehouse { set; get; } 
        public DateTime RatingDate { get; set; }
        public int Rating { set; get; }
        public string Review { get; set; }
        public string Skcode { set; get; }  
        public string CustomerName { set; get; }
        public string MobileNo { set; get; }        
        public List<UserRatingDetailDcs> UserRatingDetailDcs { get; set; }

    }
    //public class ExportDboyRatingDC
    //{
    //    public int OrderID { set; get; }
    //    public DateTime OrderDate { set; get; }
    //    public string DeliveryBoy { set; get; }
    //    public string CityName { get; set; }
    //    public string Warehouse { set; get; }
    //    public DateTime RatingDate { get; set; }
    //    public int Rating { set; get; }
    //    public string Review { get; set; }
    //    public string Skcode { set; get; }
    //    public string CustomerName { set; get; }
    //    public string MobileNo { set; get; }       
    //}    
    public class SalesPersonDC
    {
        public int SalesId { get; set; }
        public string SalesPersonName { get; set; }
    }
    public class SalesRatingFilter
    {
        public int Take { get; set; }
        public int Skip { get; set; }
        public int AppType { get; set; }
        public List<int> SalesIds { get; set; }
        public DateTime? Today { get; set; }
        public DateTime? Start { get; set; }
        public DateTime? End { get; set; }
        public string key { get; set; }
        //public int OrderId { get; set; }
        //public string Skcode { get; set; }
        //public string Mobile { get; set; }
    }
    public class SalesRatingDC
    {
        public long Id { set; get; }
        public int OrderID { set; get; }
        public DateTime OrderDate { set; get; }
        public string SalesPerson { set; get; }
        public string CityName { get; set; }
        public string Warehouse { set; get; }
        public string Cluster { set; get; }
        public DateTime RatingDate { get; set; }
        public int Rating { set; get; }
        public string Review { get; set; }
        public string Frequency { set; get; }
        public string Skcode { set; get; }
        public string CustomerName { set; get; }
        public string MobileNo { set; get; }      
        public List<UserRatingDetailDcs> UserRatingDetailDcs { get; set; }

    }
    //public class ExportSalesRatingDC
    //{
    //    public int OrderID { set; get; }
    //    public DateTime OrderDate { set; get; }
    //    public string SalesPerson { set; get; }
    //    public string CityName { get; set; }
    //    public string Warehouse { set; get; }
    //    public string Cluster { set; get; }
    //    public DateTime RatingDate { get; set; }
    //    public int Rating { set; get; }
    //    public string Review { get; set; }
    //    public string Frequency { set; get; }
    //    public string Skcode { set; get; }
    //    public string CustomerName { set; get; }
    //    public string MobileNo { set; get; }
        
    //}
    public class UserRatingDetailDcs
    {
        public long Id { set; get; }
        public string Review { set; get; }
        public long UserRatingId { get; set; }
    }
    public class CustomerRatingFilter
    {
        public int Take { get; set; }
        public int Skip { get; set; }
        public List<int> WarehouseIds { get; set; }
        public int AppType { get; set; }
        public DateTime? Today { get; set; }
        public DateTime? Start { get; set; }
        public DateTime? End { get; set; }
        public string key { get; set; }
        //public int OrderId { get; set; }
        //public string Skcode { get; set; }
        //public string Mobile { get; set; }
    }
    public class CustomerRatingDC
    {
        public long Id { set; get; }
        public int OrderID { set; get; }
        public DateTime OrderDate { set; get; }
        public string Skcode { set; get; }
        public string CustomerName { set; get; }
        public string CityName { get; set; }
        public string Warehouse { set; get; }
        public string MobileNo { set; get; }
        public DateTime RatingDate { get; set; }
        public int Rating { set; get; }
        public string Review { get; set; }
        public string DeliveryBoy { set; get; }        
        public List<UserRatingDetailDcs> UserRatingDetailDcs { get; set; }
    }

    //public class ExportCustomerRatingDC
    //{
    //    public int OrderID { set; get; }
    //    public DateTime OrderDate { set; get; }
    //    public string Skcode { set; get; }
    //    public string CustomerName { set; get; }
    //    public string CityName { get; set; }
    //    public string Warehouse { set; get; }
    //    public string MobileNo { set; get; }
    //    public DateTime RatingDate { get; set; }
    //    public int Rating { set; get; }
    //    public string Review { get; set; }
    //    public string DeliveryBoy { set; get; }
    //}
    public class PaggingRatingrData
    {
        public int total_count { get; set; }
        public dynamic RatingDC { get; set; }

    }
}
