using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.UserRating
{
    public class UserRatingBackendDC
    {
        public int Id { get; set; }
        public int UserId { set; get; }  //(Sales  , Delivery ) : PeopleId   &  Retailer   : CustomerId
        public string UserName { get; set; }
        public int AppType { set; get; }  // 1 : Sales  , 2: Retailer , 3: Delivery 
        public string AppTypeName { get; set; }
        public int Rating { set; get; }
        public int? OrderId { set; get; }
        public string ShopVisited { set; get; }  // Use only for Sales  , Delivery 
        public List<UserRatingDetailDC> UserRatingDetails { set; get; }
        public DateTime CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public bool IsActive { get; set; }
        public bool? IsDeleted { get; set; }
        public int CreatedBy { get; set; }
        public int? ModifiedBy { get; set; }
    }
    public class UserRatingDetailDC
    {
        public long Id { set; get; }
        public string Detail { set; get; }
        public long UserRatingId { get; set; }
    }
    public class UserRatingResDC
    {
        public List<UserRatingBackendDC> UserRating { get; set; }
        public int TotalCount { get; set; }
    }
}
