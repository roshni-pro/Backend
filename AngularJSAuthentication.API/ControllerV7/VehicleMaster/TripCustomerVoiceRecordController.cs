using AngularJSAuthentication.API.Helper.VehicleMasterHelper;
using NLog;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Http;

namespace AngularJSAuthentication.API.ControllerV7.VehicleMaster
{
    [RoutePrefix("api/TripCustVoiceRecord")]
    public class TripCustomerVoiceRecordController : ApiController
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private static TimeZoneInfo INDIAN_ZONE = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
        DateTime indianTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE);

        // Insert: TripCustomerVoiceRecord
        [AllowAnonymous]
        [Route("InsertTripCustomerVoiceRecord")]
        [HttpPost]
        public bool InsertTripCustomerVoiceRecord(TripVoiceRecordDC tripVoiceRecordDCs)
        {
            TripVoiceRecordHelper voiceRecordHelper = new TripVoiceRecordHelper();
            bool res = false;
            int userid = 0;
            userid = GetUserId();
            using (var context = new AuthContext())
            {
               // bool result=true;
                bool result = voiceRecordHelper.AddnewTripVoicerecord(tripVoiceRecordDCs, userid);
                if (result)
                {
                    res = true;
                    return res;

                }
                else
                {
                    res = false;
                    return res;

                }
            }
        }

        //Get: TripCustomerVoiceRecordList
        [Route("GetTripCustVoiceRecordListByTripId")]
        [HttpGet]
        [AllowAnonymous]
        public async Task<List<TripVoiceRecordDC>> GetTripCustVoiceRecordListByTripId(int TripId)
        {
            using (var myContext = new AuthContext())
            {
                var tripIdParam = new SqlParameter("@TripId", TripId);

                var result = myContext.Database.SqlQuery<TripVoiceRecordDC>("GetTripVoiceRecordDetail @TripId", tripIdParam).ToList();
                return result;
            }
        }
        //Get: TripCustomerVoiceRecordListByTripIdAndCustomerId
        [Route("GetTripCustVoiceRecordListByTripIdCustId")]
        [HttpGet]
        [AllowAnonymous]
        public async Task<TripVoiceRecordDC> GetTripCustVoiceRecordListByTripIdCustId(int TripId, int CustomerId)
        {
            TripVoiceRecordDC res = new TripVoiceRecordDC();
            if (TripId > 0 && CustomerId > 0)
            {
                using (var myContext = new AuthContext())
                {
                    var tripIdParam = new SqlParameter("@TripId", TripId);
                    var CustIdParam = new SqlParameter("@CustomerId", CustomerId);

                    res = myContext.Database.SqlQuery<TripVoiceRecordDC>("GetTripIdCustomerViceVoiceRecordDetail @TripId,@CustomerId", tripIdParam, CustIdParam).FirstOrDefault();                   
                }
            }
            return res;
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
    public class TripVoiceRecordDC
    {
        public long Id { get; set; }
        public long TripId { get; set; }
        public int CustomerId { get; set; }
        public string RecordingUrl { get; set; }
        public string Comment { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        //public DateTime CreatedDate { get; set; }
        //public int CreatedBy { get; set; }
    }
}