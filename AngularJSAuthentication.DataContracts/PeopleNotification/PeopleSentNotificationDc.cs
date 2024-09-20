using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.PeopleNotification
{
    public class PeopleSentNotificationDc
    {
        public long Id { get; set; }
        public int ToPeopleId { set; get; }
        public int OrderId { set; get; }
        public string Message { set; get; }
        public int NotificationType { set; get; }  // 1: Notifcation , 2 :Notifcation for IsApproval
        public bool IsApproved { set; get; }
        public bool IsRejected { set; get; }
        public DateTime CreatedDate { get; set; }
        public DateTime TimeLeft { get; set; }
        public string Skcode { get; set; }
        public string ShopName { get; set; }
        public string ShopAddress { get; set; }
        public string Status { get; set; }
        public string Customerphonenum { get; set; }
        public double TotalAmount { get; set; }
        public int DboyId { get; set; }
        public string DboyName { get; set; }
        public string DboyMobileNo { get; set; }
        public string Shopimage { get; set; }
        public List<Reason> reasons { get; set; }
        public int TotalCount { set; get; }
        public long TripPlannerConfirmedDetailId { get; set; }
        public string OTP { get; set; }
        public string SalesPerson { get; set; }
        public string Mobile { get; set; }
        public string CustomerName { get; set; }
        public int TotalLimit { set; get; }
        public int UsedOTP { set; get; }
        public double FineCharge { set; get; }
        public int Requests { set; get; }
        public string VideoUrl { get; set; }
        public bool? IsVideoSeen { get; set; }
    }
    public class Reason
    {
        public string reason { get; set; }
        public long PeopleSentNotificationId { get; set; }
    }
}
