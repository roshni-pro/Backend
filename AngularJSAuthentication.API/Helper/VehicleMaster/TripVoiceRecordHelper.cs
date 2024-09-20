using AngularJSAuthentication.API.ControllerV7.VehicleMaster;
using AngularJSAuthentication.Model.TripPlanner;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AngularJSAuthentication.API.Helper.VehicleMasterHelper
{
    public class TripVoiceRecordHelper
    {
        public static Logger logger = LogManager.GetCurrentClassLogger();
        public bool AddnewTripVoicerecord(TripVoiceRecordDC tripVoiceRecordDCs, int userid)
        {
            bool result = false;
            if (tripVoiceRecordDCs != null)
            {
                using (var context = new AuthContext())
                {
                    TripCustomerVoiceRecord tripRecord = new TripCustomerVoiceRecord()
                    {
                        TripId = tripVoiceRecordDCs.TripId,
                        RecordingUrl = tripVoiceRecordDCs.RecordingUrl,
                        CustomerId = tripVoiceRecordDCs.CustomerId,
                        Comment = tripVoiceRecordDCs.Comment,
                        CreatedBy = userid,
                        CreatedDate = DateTime.Now,
                        IsActive = true,
                        IsDeleted = false
                    };
                    context.TripCustomerVoiceRecords.Add(tripRecord);
                    if (context.Commit() > 0)
                    {
                        result = true;
                    }
                }
            }
            return result;
        }

    }
}