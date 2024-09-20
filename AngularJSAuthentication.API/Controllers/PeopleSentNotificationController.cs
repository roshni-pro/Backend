using NLog;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Security.Claims;
using AngularJSAuthentication.API.Helper;
using AgileObjects.AgileMapper;
using System.Configuration;

namespace AngularJSAuthentication.API.Controllers
{
    [RoutePrefix("api/PeopleSentNotification")]
    public class PeopleSentNotificationController : ApiController
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private static TimeZoneInfo INDIAN_ZONE = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
        DateTime indianTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE);

        //Get: GetWarehouseWisePeopleList
        [Route("GetWarehouseWisePeoples")]
        [HttpGet]
        [AllowAnonymous]
        public async Task<List<WarehouseWisePeopleDC>> GetWarehouseWisePeoples(int wid)
        {
            using (var myContext = new AuthContext())
            {
                var wIdParam = new SqlParameter("@WarehouseId", wid);
                var result = myContext.Database.SqlQuery<WarehouseWisePeopleDC>("GetWarehouseWiseRoleForPeopleSentNotification @WarehouseId", wIdParam).ToList();
                return result;
            }
        }

        //GetallNotificationsBySerachOrderId
        [Route("GetallNotificationsByOrderId")]
        [HttpPost]
        public PaggingDataNotification GetallNotificationsByOrderId(NotificationDC NotificationDC)
        {
            int ApproveTimeLeft = Convert.ToInt32(ConfigurationManager.AppSettings["ApproveNotifyTimeLeftInMinute"]);
            using (var context = new AuthContext())
            {
                PaggingDataNotification data = new PaggingDataNotification();
                List<PeopleSentNotificationDC> peopleSentNotificationList = new List<PeopleSentNotificationDC>();
                context.Database.CommandTimeout = 600;
                var peopleIdParam = new SqlParameter("@PeopleId", NotificationDC.PeopleId);
                var orderIdParam = new SqlParameter("@OrderId", NotificationDC.OrderId);
                var skipParam = new SqlParameter("@Skip", NotificationDC.skip);
                var takeParam = new SqlParameter("@Take", NotificationDC.take);
                var PeopleSentNotificationDc = context.Database.SqlQuery<PeopleSentNotificationDC>("Operation.GetAllPeopleNotification @PeopleId,@OrderId,@Skip,@Take", peopleIdParam, orderIdParam, skipParam, takeParam).ToList();
                //var peopleids = PeopleSentNotificationDc.Select(x => x.ApprovedBy).ToList();
                //var people = context.Peoples.Where(x => peopleids.Contains(x.PeopleID)).Select(x => new { x.PeopleID, x.DisplayName, x.PeopleFirstName }).ToList();
                if (PeopleSentNotificationDc != null && PeopleSentNotificationDc.Any())
                {
                    PeopleSentNotificationDc = Mapper.Map(PeopleSentNotificationDc).ToANew<List<PeopleSentNotificationDC>>();
                    PeopleSentNotificationDc.ForEach(x =>
                    {
                        x.TimeLeft = x.CreatedDate.AddMinutes(ApproveTimeLeft); // from Create date
                        if(x.IsApproved == true)
                        {
                            var name = context.Peoples.Where(y => y.PeopleID == x.ApprovedBy).Select(y => new { y.PeopleID, y.DisplayName }).FirstOrDefault();
                            if (name != null)
                            {
                                x.ApprovalBy = name.DisplayName;
                            }
                        }
                        if (x.IsRejected == true)
                        {
                            var rejectByname = context.Peoples.Where(y => y.PeopleID == x.RejectedBy).Select(y => new { y.PeopleID, y.DisplayName }).FirstOrDefault();
                            //var name = people.Where(y => y.PeopleID == x.ApprovedBy).FirstOrDefault();
                            if (rejectByname != null)
                            {
                                x.RejectedByName = rejectByname.DisplayName;
                            }
                        }
                    });
                }
                data.notificationmaster = PeopleSentNotificationDc;
                data.total_count = PeopleSentNotificationDc != null && PeopleSentNotificationDc.Any() ? PeopleSentNotificationDc.FirstOrDefault().TotalCount : 0;
                return data;
            }
        }

        //Get: PeopleSentNotificationApproved
        [Route("PeopleSentNotificationApproved")]
        [HttpGet]
        [AllowAnonymous]
        public async Task<bool> PeopleSentNotificationApproved(int Id, int PeopleId, bool IsNotificationApproved)
        {
            ConfigureNotifyHelper configureNotifyHelper = new ConfigureNotifyHelper();
            using (var myContext = new AuthContext())
            {
                bool result = await configureNotifyHelper.IsNotificationApproved(Id, PeopleId, IsNotificationApproved,myContext);
                return result;
            }
        }

        private int GetUserId()
        {
            var identity = User.Identity as ClaimsIdentity;
            int userid = 0;
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
            return userid;
        }
    }

    public class NotificationDC
    {
        public int skip { get; set; }
        public int take { get; set; }
        public int PeopleId { get; set; }
        public int OrderId { get; set; }
    }
    public class WarehouseWisePeopleDC
    {
        public int PeopleId { set; get; }
        public string DisplayName { set; get; }
        public string RoleName { set; get; }
    }
    public class PeopleSentNotificationDC
    {
        public long Id { get; set; }
        public int OrderId { set; get; }
        public int AppId { set; get; } //DeliveryApp = 2, SalesApp = 3, SarthiApp = 4, RetailerApp = 1
        public int ToPeopleId { set; get; }
        public string FcmId { set; get; }
        public string Message { set; get; }
        public int NotificationType { set; get; }  // 1: Notifcation , 2 :Notifcation for IsApproval
        public bool IsApproved { set; get; }
        public DateTime CreatedDate { get; set; }
        public DateTime? ApprovedDate { get; set; }
        public int CreatedBy { get; set; }
        public int? ApprovedBy { get; set; }
        public int TotalCount { set; get; }
        public string ApprovalBy { get; set; }
        public DateTime TimeLeft { get; set; }
        public string OrderStatus { get; set; }
        public string Status { get; set; }
        public long TripPlannerConfirmedDetailId { get; set; }
        public bool IsRejected { set; get; }
        public int? RejectedBy { get; set; }
        public string RejectedByName { get; set; }
    }
    public class PaggingDataNotification
    {
        public int total_count { get; set; }
        public dynamic notificationmaster { get; set; }
    }
}
