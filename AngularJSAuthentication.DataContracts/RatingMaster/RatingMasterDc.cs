using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.RatingMaster
{
   public class RatingMasterDc
    {
        public class RatingMasterDC
        {
            public long Id { set; get; }
            public int AppType { set; get; } // Sales App , Retailer App,  Delivery App
            public string AppTypeName { set; get; }
            public int Rating { set; get; } // 1,2,3,4,5 
            public List<RatingDetailDC> RatingDetails { set; get; }
            public DateTime CreatedDate { get; set; }
            public DateTime? ModifiedDate { get; set; }
            public bool IsActive { get; set; }
            public bool? IsTripId { set; get; }
            public bool? IsDeleted { get; set; }
            public int CreatedBy { get; set; }
            public int? ModifiedBy { get; set; }
        }
        public class RatingDetailDC
        {
            public long Id { set; get; }
            public string Detail { set; get; } //
            public long RatingMasterId { get; set; }
            public DateTime CreatedDate { get; set; }
            public DateTime? ModifiedDate { get; set; }
            public bool IsActive { get; set; }
            public bool? IsDeleted { get; set; }
            public int CreatedBy { get; set; }
            public int? ModifiedBy { get; set; }
        }
        public enum AppTypes 
        { 
            SalesApp = 1,
            RetailerApp = 2,
            DeliveryApp = 3           
        }
        public class ResRatingMasterDC
        {
            public bool Result { get; set; }
            public string msg { get; set; }
        }
    }
}