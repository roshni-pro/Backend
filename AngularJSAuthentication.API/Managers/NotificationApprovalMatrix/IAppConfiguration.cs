using AngularJSAuthentication.DataContracts.PeopleNotification;
using AngularJSAuthentication.Model.DeliveryOptimization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.API.Managers.NotificationApprovalMatrix
{
    public interface IAppConfiguration
    {
        bool SendNotification(string FcmId, string Msg, string FCMKey);
        Task<bool> SendNotificationForApproval(string FcmId, string Msg, int OrderId, string FCMKey, string OrderStatus, string SkCode, string ShopName, double GrossAmount);
        bool SendNotificationForApprovalDeliveyApp(string FcmId, string Msg, int OrderId, string FCMKey, string OrderStatus, int status, string notify_type);
        Task<bool> NotifyCustomer(string Title, string Message, string FcmId, int CustomerId);
        Task<bool> SendNotificationForApprovalSarthiTrip_DeliveryApp(string FcmId, string Msg, long TripPlannerConfirmedMasterId, string FCMKey, string notify_type, bool IsApproved);

    }
}
