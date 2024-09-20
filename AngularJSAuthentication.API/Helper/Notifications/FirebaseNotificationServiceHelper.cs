using AngularJSAuthentication.API.Controllers;
using AngularJSAuthentication.Common.Helpers;
using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace AngularJSAuthentication.API.Helper.Notification
{
    public class FirebaseNotificationServiceHelper
    {
        private static FirebaseApp _firebaseApp;
        public FirebaseNotificationServiceHelper(string jsonKeyPath)
        {
            _firebaseApp = FirebaseApp.DefaultInstance;
            // Initialize Firebase
            _firebaseApp = FirebaseApp.Create(new AppOptions()
            {
                Credential = GoogleCredential.FromFile(jsonKeyPath)
                //.CreateScoped("https://www.googleapis.com/auth/firebase.messaging")
            });

        }
        public async Task<dynamic> SendNotificationAsync(string FcmId, AngularJSAuthentication.Model.Notification notification, string notify_type)
        {
            try
            {
                var message = new Message()
                {
                    Token = FcmId,
                    Data = new Dictionary<string, string>()
                    {
                        { "title",notification.title },
                        { "body", notification.Message},
                        { "icon", notification.Pic},
                        { "priority", notification.priority},
                        { "notify_type", notify_type}
                    },
                };
                // Send a message to the device corresponding to the provided registration token.
                TextFileLogHelper.TraceLog($"Notify1 FcmId: {FcmId}, Data: {notification.title}");
                var res = await FirebaseMessaging.DefaultInstance.SendAsync(message);
                _firebaseApp.Delete();
                return res;
            }
            catch (Exception ex)
            {
                TextFileLogHelper.LogError(ex.ToString());
                TextFileLogHelper.TraceLog($"Notify2 error: {ex.Message.ToString()}");
                return null;
            }
        }
        public async Task<dynamic> SendNotificationForApprovalAsync(string FcmId, FCMData fCMData)
        {
            try
            {
                var message = new Message()
                {
                    Token = FcmId,
                    Data = new Dictionary<string, string>()
                    {
                        { "title",fCMData.title },
                        { "body", fCMData.body},
                        { "icon", fCMData.icon},
                        { "typeId",Convert.ToString(fCMData.typeId)},
                        { "notificationCategory", fCMData.notificationCategory},
                        { "notificationType", fCMData.notificationType},
                        { "notificationId", Convert.ToString(fCMData.notificationId)},
                        { "notify_type", fCMData.notify_type},
                        { "OrderId", Convert.ToString(fCMData.OrderId)},
                        { "url", fCMData.url},
                        { "OrderStatus", fCMData.OrderStatus},
                        { "IsApproved",Convert.ToString(fCMData.IsApproved)},
                        { "image_url",Convert.ToString(fCMData.image_url)},
                    },
                };
                TextFileLogHelper.TraceLog($"Notify1 FcmId: {FcmId}, Data: {fCMData.title}");
                // Send a message to the device corresponding to the provided registration token.
                var res = await FirebaseMessaging.DefaultInstance.SendAsync(message);
                _firebaseApp.Delete();
                return res;
            }
            catch (Exception ex)
            {
                TextFileLogHelper.TraceLog($"Notify2 error: {ex.Message.ToString()}");
                return null;
            }
        }
    }
}