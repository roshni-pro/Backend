using AngularJSAuthentication.Common.Constants;
using AngularJSAuthentication.Common.Helpers;
using AngularJSAuthentication.Model;
using AngularJSAuthentication.Model.SalesApp;
using GenricEcommers.Models;
using Newtonsoft.Json;
using Nito.AspNetBackgroundTasks;
using NLog;
using SqlBulkTools;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using static AngularJSAuthentication.API.Controllers.NotificationController;
using AngularJSAuthentication.API.Helper.Notification;
using System.Data;
using AngularJSAuthentication.Model.Login;
using System.Data.Entity.Infrastructure;

namespace AngularJSAuthentication.API.Controllers
{
    [RoutePrefix("api/NotificationUpdated")]
    public class NotificationUpdatedController : ApiController
    {

        private static Logger logger = LogManager.GetCurrentClassLogger();
        private static TimeZoneInfo INDIAN_ZONE = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
        DateTime indianTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE);
        public double xPointValue = AppConstants.xPoint;
        [Route("get")]
        [HttpGet]
        public PaggingDatas notifyy(int list, int page)
        {
            using (var context = new AuthContext())
            {
                try
                {

                    var identity = User.Identity as ClaimsIdentity;
                    int compid = 0, userid = 0;

                    foreach (Claim claim in identity.Claims)
                    {
                        if (claim.Type == "compid")
                        {
                            compid = int.Parse(claim.Value);
                        }
                        if (claim.Type == "userid")
                        {
                            userid = int.Parse(claim.Value);
                        }
                    }

                    int CompanyId = compid;

                    if (CompanyId == 0)
                    {

                        throw new ArgumentNullException("item");
                    }
                    PaggingDatas data = new PaggingDatas();
                    var total_count = context.NotificationUpdatedDb.Where(x => x.Message != null && x.CompanyId == CompanyId).Count();
                    var notificationmaster = context.NotificationUpdatedDb.Where(x => x.Message != null && x.CompanyId == CompanyId).Include(x => x.NotificationSchedulers).OrderByDescending(x => x.NotificationTime).Skip((page - 1) * list).Take(list).ToList();

                    if (notificationmaster.Any())
                    {
                        var TotalStores = context.StoreDB.Where(x => x.IsActive == true && x.IsDeleted == false).Count();
                        List<int> warehouseids = notificationmaster.Select(x => x.WarehouseID).ToList();
                        var warehouses = context.Warehouses.Where(x => warehouseids.Contains(x.WarehouseId)).Select(x => new { x.WarehouseName, x.WarehouseId }).ToList();
                        notificationmaster.ForEach(x =>
                            {
                                x.WarehouseName = warehouses.Any(y => y.WarehouseId == x.WarehouseID) ? warehouses.FirstOrDefault(y => y.WarehouseId == x.WarehouseID).WarehouseName : "";
                            });

                        foreach (var res in notificationmaster)
                        {
                            if (res.GroupAssociation == "SalesApp")
                            {
                                var storeLists = (from x1 in context.NotificationUpdatedDb
                                                  join y in context.StoreNotifications
                                                  on x1.Id equals y.NotificationId
                                                  where x1.Id == res.Id
                                                  select new
                                                  {
                                                      x1.Id,
                                                      y.StoreId
                                                  });
                                res.StoreIds = storeLists.Select(y => (int)y.StoreId).Distinct().ToList();
                                res.StoreNames = res.StoreIds.Count() < TotalStores ? string.Join(", ", context.StoreDB.Where(x => res.StoreIds.Contains((int)x.Id)).Select(y => y.Name).ToList()) : "ALL";
                            }
                        }
                    }
                    data.notificationmaster = notificationmaster;
                    data.total_count = total_count;
                    return data;
                }
                catch (Exception ex)
                {
                    return null;
                }
            }
        }

        [Route("allfcmcust")]
        [HttpGet]
        public HttpResponseMessage Get(int id)
        {
            List<Customer> ass = new List<Customer>();
            using (var context = new AuthContext())
            {
                try
                {
                    var identity = User.Identity as ClaimsIdentity;
                    int compid = 0, userid = 0;

                    foreach (Claim claim in identity.Claims)
                    {
                        if (claim.Type == "compid")
                        {
                            compid = int.Parse(claim.Value);
                        }
                        if (claim.Type == "userid")
                        {
                            userid = int.Parse(claim.Value);
                        }
                    }

                    var list = (from i in context.DbOrderDetails
                                where i.Deleted == false
                                join b in context.itemMasters on i.ItemId equals b.ItemId
                                join c in context.Categorys on b.Categoryid equals c.Categoryid
                                join j in context.Customers on i.CustomerId equals j.CustomerId
                                select new notModelUpdated
                                {
                                    categoryId = c.Categoryid,
                                    CustomerId = i.CustomerId,
                                    orderTotal = i.TotalAmt,

                                    fcmId = j.fcmId
                                }).ToList();
                    var list1 = list.Where(x => x.categoryId == id).ToList();
                    List<notModelUpdated> uniqecustomer = new List<notModelUpdated>();
                    foreach (var a in list1)
                    {
                        notModelUpdated customer = uniqecustomer.Where(c => c.CustomerId == a.CustomerId).SingleOrDefault();
                        if (customer == null)
                        {
                            a.orderCount = 1;
                            uniqecustomer.Add(a);
                        }
                        else
                        {
                            customer.orderCount++;
                            customer.orderTotal += a.orderTotal;
                        }

                    }
                    var cust = uniqecustomer.OrderBy(o => o.orderCount).Take(2);
                    List<Customer> custlist = new List<Customer>();
                    foreach (var b in cust)
                    {
                        Customer cu = context.Customers.Where(c => c.CustomerId == b.CustomerId).SingleOrDefault();
                        custlist.Add(cu);
                    }

                    logger.Info("End  Return: ");
                    return Request.CreateResponse(HttpStatusCode.OK, custlist);
                }
                catch (Exception ex)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex.Message);
                }
            }
        }


        [Route("GroupData")]
        [HttpGet]
        public HttpResponseMessage GroupData(string GroupAssosiation, long GroupID, int WarehouseID)
        {
            using (var context = new AuthContext())
            {
                try
                {
                    var identity = User.Identity as ClaimsIdentity;
                    int compid = 0, userid = 0;

                    foreach (Claim claim in identity.Claims)
                    {
                        if (claim.Type == "compid")
                        {
                            compid = int.Parse(claim.Value);
                        }
                        if (claim.Type == "userid")
                        {
                            userid = int.Parse(claim.Value);
                        }
                    }
                    //============Old Code===========//
                    //switch (GroupAssosiation)
                    //{
                    //    case "Retailer":
                    //        var retailer = context.Customers.Where(c => c.fcmId != null && c.GroupName == GroupName).ToList();
                    //        logger.Info("End  Return: ");
                    //        return Request.CreateResponse(HttpStatusCode.OK, retailer);

                    //    case "People":
                    //        var people = context.Peoples.Where(c => c.FcmId != null && c.GroupName == GroupName).ToList();
                    //        logger.Info("End  Return: ");
                    //        return Request.CreateResponse(HttpStatusCode.OK, people);

                    //    //case "Supplier":
                    //    //    var supplier = context.Suppliers.Where(c => c.FcmId != null && c.GroupName == GroupName).ToList();
                    //    //    logger.Info("End  Return: ");
                    //    //    return Request.CreateResponse(HttpStatusCode.OK, supplier);
                    //    default:
                    //        return Request.CreateResponse(HttpStatusCode.OK);
                    //}

                    //=============New Code============//
                    switch (GroupAssosiation)
                    {
                        case "Retailer":
                            var retailer = context.SalesGroupDb.Where(c => c.Id == GroupID).ToList();
                            logger.Info("End  Return: ");
                            return Request.CreateResponse(HttpStatusCode.OK, retailer);

                        //case "People":
                        //var people = context.Peoples.Where(c => c.FcmId != null && c.GroupID == GroupID).ToList();
                        //logger.Info("End  Return: ");
                        //return Request.CreateResponse(HttpStatusCode.OK, people);

                        //case "Supplier":
                        //    var supplier = context.Suppliers.Where(c => c.FcmId != null && c.GroupName == GroupName).ToList();
                        //    logger.Info("End  Return: ");
                        //    return Request.CreateResponse(HttpStatusCode.OK, supplier);
                        default:
                            return Request.CreateResponse(HttpStatusCode.OK);
                    }

                }
                catch (Exception ex)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex.Message);
                }
            }
        }


        //for Notification View
        [Route("NotificationView")]
        [HttpPost]
        public HttpResponseMessage NotificationView(int NotificationID, string FcmID)
        {
            using (var context = new AuthContext())
            {
                try
                {
                    var identity = User.Identity as ClaimsIdentity;
                    int compid = 0, userid = 0;

                    foreach (Claim claim in identity.Claims)
                    {
                        if (claim.Type == "compid")
                        {
                            compid = int.Parse(claim.Value);
                        }
                        if (claim.Type == "userid")
                        {
                            userid = int.Parse(claim.Value);
                        }
                    }
                    return Request.CreateResponse(HttpStatusCode.OK);
                }
                catch (Exception ex)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex.Message);
                }
            }
        }


        //for Update Notification
        [Route("UpdateNotification")]
        [HttpPost]
        public NotificationUpdated UpdateNotification(NotificationUpdated notification)
        {
            using (AuthContext context = new AuthContext())
            {
                try
                {
                    var identity = User.Identity as ClaimsIdentity;
                    int compid = 0, userid = 0;

                    foreach (Claim claim in identity.Claims)
                    {
                        if (claim.Type == "compid")
                        {
                            compid = int.Parse(claim.Value);
                        }
                        if (claim.Type == "userid")
                        {
                            userid = int.Parse(claim.Value);
                        }
                    }
                    /*if (notification.GroupAssociation == "SalesApp")
                    {
                        var res = context.NotificationUpdatedDb.Where(x => x.Id == notification.Id && !x.Sent).FirstOrDefault();
                        if (res != null)
                        {
                            context.Commit();
                        }
                    }
                    else
                    {*/
                    context.UpdateNotification(notification);
                    //}
                    return notification;
                }
                catch (Exception ex)
                {
                    NotificationUpdated objdt = new NotificationUpdated();
                    objdt.ErrorMessage = ex.Message;
                    return objdt;
                }
            }
        }


        [Route("DisableNotification")]
        [HttpGet]
        public async Task<bool> DisableNotification(int notificationId)
        {
            bool result = false;
            using (AuthContext context = new AuthContext())
            {
                var identity = User.Identity as ClaimsIdentity;
                int userid = 0;
                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                    userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

                var notification = await context.NotificationUpdatedDb.Where(x => x.Id == notificationId).Include(x => x.NotificationSchedulers).FirstOrDefaultAsync();
                if (notification != null)
                {
                    notification.Sent = true;
                    notification.UpdateTime = DateTime.Now;
                    if (notification.IsMultiSchedule && notification.NotificationSchedulers != null && notification.NotificationSchedulers.Any())
                    {
                        notification.NotificationSchedulers.ForEach(x => x.Sent = true);
                    }
                    context.Entry(notification).State = EntityState.Modified;
                    result = context.Commit() > 0;
                }
            }
            return result;
        }
        //for Notification Addition
        [Route("AddNotification")]
        [HttpPost]
        public async Task<NotificationUpdated> AddNotification(NotificationUpdated notification)
        {
            try
            {
                var identity = User.Identity as ClaimsIdentity;
                int compid = 0, userid = 0;
                string UserName = "";

                foreach (Claim claim in identity.Claims)
                {
                    if (claim.Type == "compid")
                    {
                        compid = int.Parse(claim.Value);
                    }
                    if (claim.Type == "userid")
                    {
                        userid = int.Parse(claim.Value);
                    }
                    if (claim.Type == "DisplayName")
                    {
                        UserName = claim.Value;
                    }
                }
                FCMResponse response1 = new FCMResponse();
                if (notification.GroupAssociation == "SalesApp")
                {
                    foreach (var ctid in notification.CityIdList)
                    {
                        foreach (var wrid in notification.WarehouseIdList)
                        {
                            notification.Id = 0;
                            if (notification.NotificationSchedulers.Count > 0) notification.NotificationSchedulers.ForEach(x => { x.Id = 0; });
                            using (var context = new AuthContext())
                            {
                                var res = context.Warehouses.Where(x => x.Cityid == ctid && x.WarehouseId == wrid && x.active == true && x.Deleted == false).FirstOrDefault();
                                if (res != null)
                                {
                                    notification.CityId = ctid;
                                    notification.WarehouseID = wrid;
                                    notification.CityName = res.CityName;
                                    notification.WarehouseName = res.WarehouseName;
                                    notification.CompanyId = compid;
                                    notification.CreatedBy = userid;
                                    notification.CreatedByName = UserName;
                                    notification.CreatedTime = indianTime;
                                    notification.NotificationTime = indianTime;
                                    context.NotificationUpdatedDb.Add(notification);
                                    context.Commit();
                                }
                                foreach (var strid in notification.StoreIds)
                                {
                                    StoreNotification storeNotification = new StoreNotification();
                                    storeNotification.NotificationId = notification.Id;
                                    storeNotification.StoreId = strid;
                                    storeNotification.CityId = ctid;
                                    storeNotification.Sent = false;
                                    storeNotification.CreatedBy = userid;
                                    storeNotification.CreatedDate = DateTime.Now;
                                    storeNotification.IsActive = true;
                                    storeNotification.IsDeleted = false;
                                    context.StoreNotifications.Add(storeNotification);
                                }
                                context.Commit();
                            }
                        }
                    }
                }
                else
                {
                    using (var context = new AuthContext())
                    {
                        notification.CompanyId = compid;
                        notification.CreatedBy = userid;
                        notification.CreatedByName = UserName;
                        context.AddNotification(notification);
                        notification.NotificationSchedulers = null;
                    }
                }
                return notification;

            }
            catch (Exception ex)
            {
                NotificationUpdated objdt = new NotificationUpdated();
                objdt.ErrorMessage = ex.Message;
                return objdt;
            }
        }

        public async Task<string> sendHolidayNotification(int customerId, string CustomerFcmId, string days)
        {
            logger.Info("Add message: ");
            using (var context = new AuthContext())
            {
                string result = "";
                string Key1 = ConfigurationManager.AppSettings["FcmApiKey"];

                try
                {
                    //WebRequest tRequest = WebRequest.Create("https://fcm.googleapis.com/fcm/send") as HttpWebRequest;
                    //tRequest.Method = "post";
                    //string jsonNotificationFormat = Newtonsoft.Json.JsonConvert.SerializeObject(objNotification);
                    //Byte[] byteArray = Encoding.UTF8.GetBytes(jsonNotificationFormat);
                    //tRequest.Headers.Add(string.Format("Authorization: key={0}", Key1));
                    ////tRequest.Headers.Add(string.Format("Sender: id={0}", id11));
                    //tRequest.ContentLength = byteArray.Length;
                    //tRequest.ContentType = "application/json";
                    //using (Stream dataStream = tRequest.GetRequestStream())
                    //{
                    //    dataStream.Write(byteArray, 0, byteArray.Length);
                    //    using (WebResponse tResponse = tRequest.GetResponse())
                    //    {
                    //        using (Stream dataStreamResponse = tResponse.GetResponseStream())
                    //        {
                    //            using (StreamReader tReader = new StreamReader(dataStreamResponse))
                    //            {
                    //                String responseFromFirebaseServer = tReader.ReadToEnd();
                    //                FCMResponse response = Newtonsoft.Json.JsonConvert.DeserializeObject<FCMResponse>(responseFromFirebaseServer);
                    //                if (response.success == 1 && response.results != null && response.results.Any() && !string.IsNullOrEmpty(response.results.FirstOrDefault().message_id))
                    //                {
                    //                    result = "Notification sent successfully";
                    //                }
                    //                else
                    //                {
                    //                    result = "Notification Not sent";
                    //                }
                    //            }
                    //        }
                    //    }
                    //}

                    var data = new FCMData
                    {
                        title = "हॉलिडे चेंज",
                        body = "आप का हॉलिडे चेंज हो चूका है जो अब है " + days,
                        icon = "",
                        typeId = 0,
                        notificationCategory = "",
                        notificationType = "",
                        notificationId = 0,
                        notify_type = "Holiday",
                        url = "",
                        IsEnabledDismissNotification = false
                    };
                    var firebaseService = new FirebaseNotificationServiceHelper(Key1);
                    var Result = await firebaseService.SendNotificationForApprovalAsync(CustomerFcmId, data);
                    if (Result != null)
                    {
                        result = "Sent Successfully.";
                    }
                    else
                    {
                        result = "Not Sent.";
                    }

                }
                catch (Exception asd)
                {
                    logger.Error(new StringBuilder("Error while sending Holiday notifcation for type: ").Append(asd.Message).Append(asd.ToString()).ToString());
                    return result = "Notification Failed";
                }
                return result;
            }
        }

        [ResponseType(typeof(Notification))]
        [Route("")]
        [AcceptVerbs("POST")]
        public async Task<string> SendNotification(int NotificationId)
        {
            var result = "";

            using (var context = new AuthContext())
            {
                try
                {

                    var notification = context.NotificationUpdatedDb.Where(x => x.Id == NotificationId).FirstOrDefault();

                    var isSend = true;
                    if ((notification.From.HasValue && notification.TO.HasValue && indianTime > notification.TO.Value) || notification.IsCRMNotification)
                    {
                        isSend = false;
                        result = notification.IsCRMNotification == true ? "CRM Notifcation can't send manually" : "notification time expired";
                        return result;

                    }

                    if (isSend)
                    {
                        var typeCode = 0;
                        var notificationTitle = "";
                        if (notification.NotificationType == "Actionable")
                        {
                            switch (notification.NotificationCategory)
                            {
                                case "Item":
                                    notificationTitle = notification.title + " | " + notification.ItemName;
                                    typeCode = notification.ItemCode;
                                    break;
                                case "Brand":
                                    notificationTitle = notification.title + " | " + notification.BrandName;
                                    typeCode = notification.BrandCode;
                                    break;
                                case "Flash Deal":
                                    notificationTitle = notification.title;
                                    typeCode = notification.ItemCode;
                                    break;
                                case "Offer":
                                    notificationTitle = notification.title;
                                    typeCode = notification.ItemCode;
                                    break;
                                case "Category":
                                    notificationTitle = notification.title + " | " + notification.ItemName;
                                    typeCode = notification.ItemCode;
                                    break;
                                case "SubCategory":
                                    notificationTitle = notification.title + " | " + notification.ItemName;
                                    typeCode = notification.ItemCode;
                                    break;
                                default:
                                    notificationTitle = notification.title;
                                    notification.NotificationDisplayType = notification.NotificationCategory;
                                    break;
                            }
                        }
                        else
                        {
                            notificationTitle = notification.title;
                        }


                        switch (notification.GroupAssociation)
                        {
                            case "SalesApp":
                                var sent = false;
                                //notification.StoreIds
                                notification.StoreIds = context.StoreNotifications.Where(x => x.NotificationId == notification.Id).Select(y => (int)y.StoreId).ToList();
                                ConcurrentBag<int> totalSentCount = new ConcurrentBag<int>();
                                ConcurrentBag<int> totalNotSentCount = new ConcurrentBag<int>();
                                List<FCMIds> AllFcmIds = new List<FCMIds>();
                                if (notification.StoreIds != null && notification.StoreIds.Any())
                                {
                                    foreach (var strid in notification.StoreIds)
                                    {
                                        var FCMIds = (from x in context.ClusterStoreExecutives
                                                      join y in context.Peoples on x.ExecutiveId equals y.PeopleID
                                                      where x.StoreId == strid && y.WarehouseId == notification.WarehouseID && x.IsActive == true && x.IsDeleted == false
                                                            && y.Active == true && y.Deleted == false
                                                      //&& y.FcmId == "eRLqNgl7R4-o6wU5OmeDxP:APA91bFyjfPzyLSoE1Cfk0XUk1mw1O-H_gXntj5sYl18t1r1XZF20hqFGQmFZ4Pl9O91cO9YjiC9LxC4cSUGdWyQGF3EecXyzFz_3hSRAJot3cmxPAFdPq9BFzqzSIPsjBXz1uLcxVE7"
                                                      //x.StoreId == strid &&
                                                      select new FCMIds
                                                      {
                                                          FcmId = y.FcmId,
                                                          PeopleID = y.PeopleID,
                                                          UpdatedDate = y.UpdatedDate
                                                      }).ToList();

                                        AllFcmIds.AddRange(FCMIds);
                                    }
                                }


                                if (AllFcmIds.Any())
                                {
                                    AllFcmIds = AllFcmIds.Where(x => x.FcmId != null).Distinct().ToList();

                                    var DistincFcmIds = AllFcmIds.Select(y => new { FcmId = y.FcmId }).Distinct().ToList();
                                    string Key1 = ConfigurationManager.AppSettings["SalesFcmApiKeyNew"];
                                    var firebaseService1 = new FirebaseNotificationServiceHelper(Key1);
                                    foreach (var fcmobj in DistincFcmIds)
                                    {

                                        //var fcmdcs = new FcmidDCss();
                                        //fcmdcs.body = notification.Message;
                                        //fcmdcs.title = notification.title;
                                        //fcmdcs.notify_type = "general_notification";
                                        //fcmdcs.image_url = notification.Pic;
                                        //fcmdcs.IsEnabledDismissNotification = notification.IsEnabledDismissNotification;
                                        //var fcmdc = new FcmidDC();
                                        //fcmdc.to = fcmobj.FcmId;
                                        //fcmdc.data = fcmdcs;

                                        var data1 = new FCMData
                                        {
                                            title = notification.title,
                                            body = notification.Message,
                                            icon = "",
                                            notificationCategory = "",
                                            notificationType = "",
                                            notify_type = "general_notification",
                                            url = "",
                                            image_url = notification.Pic,
                                            IsEnabledDismissNotification = notification.IsEnabledDismissNotification
                                        };
                                        try
                                        {
                                            //var fcmid = "fZGIeP5dTKGd-2JBZttcDj:APA91bGINtqXqKuCcEHd4qXZMN-VtX5KC2g98KkytmGdpc28_-duDu8Ry1P6Kk_Xb9RgRG0iDWrp8DkwE1EPCOPG0OLz2uRjo-HBg-Ysg-mDaMErlYLjJGZE7ScXKIkjmWZ0xNO6UyxN";
                                            var results = firebaseService1.SendNotificationForApprovalAsync(fcmobj.FcmId, data1);
                                            if (results != null)
                                            {
                                                totalSentCount.Add(1);
                                                //fcmdc.MessageId = results.ToString();
                                                result = "Notification sent successfully";
                                                sent = true;
                                            }
                                            else
                                            {
                                                totalNotSentCount.Add(1);
                                                result = "Notification Not sent";
                                            }
                                            //WebRequest tRequest = WebRequest.Create("https://fcm.googleapis.com/fcm/send") as HttpWebRequest;
                                            //tRequest.Method = "POST";
                                            //string jsonNotificationFormat = Newtonsoft.Json.JsonConvert.SerializeObject(fcmdc);
                                            //Byte[] byteArray = Encoding.UTF8.GetBytes(jsonNotificationFormat);
                                            //tRequest.Headers.Add(string.Format("Authorization: key={0}", Key1));
                                            //tRequest.ContentLength = byteArray.Length;
                                            //tRequest.ContentType = "application/json";
                                            //using (Stream dataStream = tRequest.GetRequestStream())
                                            //{
                                            //    dataStream.Write(byteArray, 0, byteArray.Length);
                                            //    using (WebResponse tResponse = tRequest.GetResponse())
                                            //    {
                                            //        using (Stream dataStreamResponse = tResponse.GetResponseStream())
                                            //        {
                                            //            using (StreamReader tReader = new StreamReader(dataStreamResponse))
                                            //            {
                                            //                String responseFromFirebaseServer = tReader.ReadToEnd();
                                            //                AngularJSAuthentication.API.Controllers.NotificationController.FCMResponse response = Newtonsoft.Json.JsonConvert.DeserializeObject<AngularJSAuthentication.API.Controllers.NotificationController.FCMResponse>(responseFromFirebaseServer);
                                            //                if (response.success == 1)
                                            //                {
                                            //                    totalSentCount.Add(1);
                                            //                    fcmdc.MessageId = response.results.FirstOrDefault().message_id;
                                            //                    result = "Notification sent successfully";
                                            //                    sent = true;
                                            //                }
                                            //                else if (response.failure == 1)
                                            //                {
                                            //                    totalNotSentCount.Add(1);
                                            //                    result = "Notification Not sent";
                                            //                }
                                            //            }
                                            //        }
                                            //    }
                                            //}
                                        }
                                        catch (Exception asd)
                                        {
                                            logger.Error(new StringBuilder("Error while sending FCM for type: ").Append(asd.ToString()).ToString());
                                        }
                                        if (totalSentCount.Any() || totalNotSentCount.Any())
                                        {
                                            var peopleId = AllFcmIds.Where(x => x.FcmId == fcmobj.FcmId).OrderByDescending(x => x.UpdatedDate).Select(p => p.PeopleID).FirstOrDefault();
                                            ExecutiveDeviceNotification execDeviceNotification = new ExecutiveDeviceNotification
                                            {

                                                CompanyId = 1,
                                                WarehouseId = notification.WarehouseID,
                                                ExecutiveId = peopleId,//(int)fcmobj.PeopleID,
                                                DeviceId = fcmobj.FcmId,
                                                title = notification.title,
                                                Message = notification.Message,
                                                MessageId = "",
                                                ImageUrl = notification.Pic,
                                                NotificationTime = indianTime,
                                                Deleted = false,
                                                NotificationCategory = notification.NotificationCategory,
                                                NotificationType = notification.NotificationType,
                                                NotificationId = notification.Id,
                                                IsReceived = 0,
                                                NotificationDisplayType = notification.NotificationDisplayType
                                            };
                                            context.ExecutiveDeviceNotifications.Add(execDeviceNotification);
                                        }
                                    }
                                }
                                //}
                                if (totalSentCount.Any() || totalNotSentCount.Any())
                                {
                                    notification.TotalSent = totalSentCount.Count();
                                    notification.Sent = true;
                                    notification.TotalNotSent = totalNotSentCount.Count();
                                    context.Entry(notification).State = EntityState.Modified;
                                    if (notification.IsMultiSchedule)
                                    {
                                        var notifyScedule = context.NotificationSchedulers.Where(x => x.NotificationUpdatedId == notification.Id).ToList();
                                        foreach (var data1 in notifyScedule)
                                        {
                                            data1.TotalSent = totalSentCount.Count();
                                            data1.TotalNotSent = totalNotSentCount.Count();
                                            data1.Sent = true;
                                            context.Entry(data1).State = EntityState.Modified;
                                        }
                                    }
                                    result = "Notification sent successfully";
                                    var res = context.StoreNotifications.Where(x => x.NotificationId == notification.Id && !x.Sent).ToList();
                                    foreach (var data2 in res)
                                    {
                                        data2.Sent = true;
                                        context.Entry(data2).State = EntityState.Modified;
                                    }
                                    context.Commit();
                                }

                                return result;

                            case "Retailer":
                                List<NotificationCustomerDTO> customers = new List<NotificationCustomerDTO>();
                                string Sqlstring = "";
                                if (notification.GroupID > 0)
                                {
                                    if (notification.WarehouseID > 0)
                                    {
                                        Sqlstring = "select b.CustomerId,b.fcmId from  SalesGroupCustomers a with(nolock) inner join Customers b with(nolock) on a.CustomerID=b.CustomerId and b.fcmId is not null and b.fcmId!=''  and a.GroupID=" + notification.GroupID + " and b.Warehouseid=" + notification.WarehouseID;
                                        customers = context.Database.SqlQuery<NotificationCustomerDTO>(Sqlstring).ToList();
                                    }
                                    else
                                    {
                                        Sqlstring = "select b.CustomerId,b.fcmId from SalesGroupCustomers a with(nolock) inner join Customers b with(nolock) on a.CustomerID=b.CustomerId and  b.fcmId is not null and b.fcmId!='' and b.Active=1 and b.Deleted=0  and b.cityid=" + notification.CityId + " and a.GroupID=" + notification.GroupID;
                                        customers = context.Database.SqlQuery<NotificationCustomerDTO>(Sqlstring).ToList();
                                    }
                                }
                                else if (notification.GroupID == -1)
                                {
                                    if (notification.WarehouseID > 0)
                                        Sqlstring = "select b.CustomerId,b.fcmId from Customers b with(nolock) where b.IsKPP=0 and b.fcmId is not null and b.fcmId!='' and not exists(select customerid from DistributorVerifications a with(nolock)  where a.CustomerID=b.CustomerId) and b.Warehouseid=" + notification.WarehouseID;
                                    else
                                        Sqlstring = "select b.CustomerId,b.fcmId from Customers b with(nolock) where b.IsKPP=0 and b.fcmId is not null and b.fcmId!='' and not exists(select customerid from DistributorVerifications a with(nolock)  where a.CustomerID=b.CustomerId) and b.cityId=" + notification.CityId;
                                    customers = context.Database.SqlQuery<NotificationCustomerDTO>(Sqlstring).ToList();
                                }
                                else if (notification.GroupID == -2)
                                {
                                    if (notification.WarehouseID > 0)
                                        Sqlstring = "select b.CustomerId,b.fcmId from Customers b with(nolock) where b.IsKPP=1 and b.fcmId is not null and b.fcmId!='' and b.Active=1 and b.Deleted=0 and b.Warehouseid=" + notification.WarehouseID;
                                    else
                                        Sqlstring = "select b.CustomerId,b.fcmId from Customers b with(nolock) where b.IsKPP=1 and b.fcmId is not null and b.fcmId!='' and b.Active=1 and b.Deleted=0 and b.cityId=" + notification.CityId;
                                    customers = context.Database.SqlQuery<NotificationCustomerDTO>(Sqlstring).ToList();
                                }
                                else if (notification.GroupID == -3)
                                {
                                    if (notification.WarehouseID > 0)
                                        Sqlstring = "select b.CustomerId,b.fcmId from Customers b with(nolock) where  b.fcmId is not null and b.fcmId!='' and b.Active=1 and b.Deleted=0  and exists(select customerid from DistributorVerifications a with(nolock)  where a.CustomerID=b.CustomerId) and b.Warehouseid=" + notification.WarehouseID;
                                    else
                                        Sqlstring = "select b.CustomerId,b.fcmId from Customers b with(nolock) where  b.fcmId is not null and b.fcmId!='' and b.Active=1 and b.Deleted=0  and exists(select customerid from DistributorVerifications a with(nolock)  where a.CustomerID=b.CustomerId) and b.cityid=" + notification.CityId;
                                    customers = context.Database.SqlQuery<NotificationCustomerDTO>(Sqlstring).ToList();
                                }
                                else if (notification.GroupID == -10)
                                {
                                    //and exists(select customerid from DistributorVerifications a with(nolock)  where a.CustomerID=b.CustomerId)
                                    Sqlstring = "select b.CustomerId,b.fcmId from CRMNotifiationCustomers a with(nolock) inner join Customers b with(nolock) on a.CustomerId=b.CustomerId where  b.fcmId is not null and b.fcmId!='' and b.Active=1 and b.Deleted=0  and a.NotificationId=" + notification.Id;
                                    customers = context.Database.SqlQuery<NotificationCustomerDTO>(Sqlstring).ToList();
                                }
                                else if (notification.GroupID <= -4)
                                {
                                    int Level = -1;
                                    switch (notification.GroupID)
                                    {
                                        case -4:
                                            Level = 0;
                                            break;
                                        case -5:
                                            Level = 1;
                                            break;
                                        case -6:
                                            Level = 2;
                                            break;
                                        case -7:
                                            Level = 3;
                                            break;
                                        case -8:
                                            Level = 4;
                                            break;
                                        case -9:
                                            Level = 5;
                                            break;
                                    }
                                    var fromdate = DateTime.Now;
                                    fromdate = DateTime.Now.AddMonths(-1);
                                    Sqlstring = "Select distinct a.CustomerId,b.fcmId from CRMCustomerLevels a with(nolock)  inner join Customers b  with(nolock)  on a.CustomerId=b.CustomerId and IsDeleted=0 and b.Warehouseid=" + notification.WarehouseID + " and a.Month=" + fromdate.Month + " and a.Year=" + fromdate.Year + " And a.Level=" + Level;
                                    customers = context.Database.SqlQuery<NotificationCustomerDTO>(Sqlstring).ToList();
                                }
                                else
                                {
                                    Sqlstring = "select b.CustomerId,b.fcmId from Customers b with(nolock) where  b.fcmId is not null and b.fcmId!='' and b.Active=1 and b.Deleted=0  and b.cityid=" + notification.CityId;
                                    customers = context.Database.SqlQuery<NotificationCustomerDTO>(Sqlstring).ToList();
                                }

                                //var customerIdsList = notification.GroupID > 0 ? context.GroupMappings.Where(x => x.WarehouseID == notification.WarehouseID && x.GroupID == notification.GroupID).Select(x => new { CustomerID = x.CustomerID }).ToList()
                                //                      : context.Customers.Where(x => x.Warehouseid == notification.WarehouseID && x.Active && !x.Deleted && x.fcmId != null).Select(x => new { CustomerID = x.CustomerId }).ToList();

                                //foreach (var o in customerIdsList)
                                //{
                                //    var dis = context.DistributorVerificationDB.Where(x => x.CustomerID == o.CustomerID).FirstOrDefault();
                                //    if (dis == null)
                                //    {
                                //        var customerData = context.Customers.Where(x => x.CustomerId == o.CustomerID && x.fcmId != null).FirstOrDefault();
                                //        if (customerData != null)
                                //            customers.Add(customerData);
                                //    }

                                //}


                                var objNotificationList = customers.GroupBy(x => x.fcmId).Select(x => new FCMRequest
                                {
                                    to = x.Key,
                                    CustId = x.FirstOrDefault().CustomerId,
                                    MessageId = "",
                                    data = new FCMData
                                    {
                                        title = notificationTitle,
                                        body = notification.Message,
                                        icon = notification.Pic,
                                        typeId = typeCode,
                                        notificationCategory = notification.NotificationCategory,
                                        notificationType = notification.NotificationType,
                                        notificationId = notification.Id,
                                        notify_type = notification.NotificationDisplayType,
                                        url = notification.NotificationMediaType == "Video" ? notification.Pic : "",
                                        IsEnabledDismissNotification = notification.IsEnabledDismissNotification
                                    }
                                }).ToList();

                                ConcurrentBag<int> totalSent = new ConcurrentBag<int>();
                                ConcurrentBag<int> totalNotSent = new ConcurrentBag<int>();
                                string Key = ConfigurationManager.AppSettings["FcmApiKey"];
                                var firebaseService = new FirebaseNotificationServiceHelper(Key);
                                var Data = new FCMData
                                {
                                    title = notificationTitle,
                                    body = notification.Message,
                                    icon = notification.Pic,
                                    typeId = typeCode,
                                    notificationCategory = notification.NotificationCategory,
                                    notificationType = notification.NotificationType,
                                    notificationId = notification.Id,
                                    notify_type = notification.NotificationDisplayType,
                                    url = notification.NotificationMediaType == "Video" ? notification.Pic : "",
                                    IsEnabledDismissNotification = notification.IsEnabledDismissNotification
                                };
                                var FcmIds = customers.Select(x => x.fcmId).Distinct().ToList();

                                ParallelLoopResult parellelResult = Parallel.ForEach(FcmIds, async (x) =>
                                {
                                    try
                                    {
                                        //WebRequest tRequest = WebRequest.Create("https://fcm.googleapis.com/fcm/send") as HttpWebRequest;
                                        //tRequest.Method = "post";
                                        //string jsonNotificationFormat = Newtonsoft.Json.JsonConvert.SerializeObject(x);
                                        //Byte[] byteArray = Encoding.UTF8.GetBytes(jsonNotificationFormat);
                                        //tRequest.Headers.Add(string.Format("Authorization: key={0}", Key));
                                        ////tRequest.Headers.Add(string.Format("Sender: id={0}", id11));
                                        //tRequest.ContentLength = byteArray.Length;
                                        //tRequest.ContentType = "application/json";
                                        //using (Stream dataStream = tRequest.GetRequestStream())
                                        //{
                                        //    dataStream.Write(byteArray, 0, byteArray.Length);
                                        //    using (WebResponse tResponse = tRequest.GetResponse())
                                        //    {
                                        //        using (Stream dataStreamResponse = tResponse.GetResponseStream())
                                        //        {
                                        //            using (StreamReader tReader = new StreamReader(dataStreamResponse))
                                        //            {
                                        //                String responseFromFirebaseServer = tReader.ReadToEnd();
                                        //                FCMResponse response = Newtonsoft.Json.JsonConvert.DeserializeObject<FCMResponse>(responseFromFirebaseServer);
                                        //                if (response.success == 1 && response.results != null && response.results.Any() && !string.IsNullOrEmpty(response.results.FirstOrDefault().message_id))
                                        //                {
                                        //                    totalSent.Add(1);
                                        //                    x.MessageId = response.results.FirstOrDefault().message_id;
                                        //                }
                                        //                else
                                        //                {
                                        //                    totalNotSent.Add(1);
                                        //                }
                                        //            }
                                        //        }
                                        //    }
                                        //}

                                        var Result = await firebaseService.SendNotificationForApprovalAsync(x, Data);
                                        if (Result != null)
                                        {
                                            result = "Sent Successfully.";
                                            totalSent.Add(1);
                                        }
                                        else
                                        {
                                            result = "Not Sent.";
                                            totalNotSent.Add(1);
                                        }

                                    }
                                    catch (Exception asd)
                                    {
                                        logger.Error(new StringBuilder("Error while sending FCM for type: ").Append(notification.NotificationType).Append(asd.ToString()).ToString());
                                    }
                                });



                                if (parellelResult.IsCompleted )
                                {
                                    notification.TotalSent = totalSent.Count();
                                    notification.Sent = true;
                                    notification.TotalNotSent = totalNotSent.Count();
                                    context.Entry(notification).State = EntityState.Modified;

                                    var deviceNotifications = objNotificationList.Select(x => new DeviceNotification
                                    {
                                        CompanyId = 1,
                                        CustomerId = x.CustId,
                                        Deleted = false,
                                        DeviceId = x.to,
                                        ImageUrl = notification.Pic,
                                        Message = notification.Message,
                                        NotificationTime = indianTime,
                                        title = notification.title,
                                        WarehouseId = notification.WarehouseID,
                                        NotificationCategory = notification.NotificationCategory,
                                        NotificationType = notification.NotificationType,
                                        ObjectId = typeCode,
                                        NotificationId = notification.Id,
                                        IsView = notification.TotalViews,
                                        IsReceived = 0,
                                        MessageId = x.MessageId,
                                        NotificationDisplayType = notification.NotificationDisplayType

                                    }).ToList();

                                    context.DeviceNotificationDb.AddRange(deviceNotifications);
                                    context.Commit();
                                }

                                result = "Notification sent successfully";
                                return result;
                            case "DistributorAPP":
                                List<Customer> customersD = new List<Customer>();

                                var customerIdsLists = notification.GroupID > 0 ? context.GroupMappings.Where(x => x.WarehouseID == notification.WarehouseID && x.GroupID == notification.GroupID).Select(x => new { CustomerID = x.CustomerID }).ToList()
                                                      : context.Customers.Where(x => x.Warehouseid == notification.WarehouseID && x.Active && !x.Deleted && x.fcmId != null).Select(x => new { CustomerID = x.CustomerId }).ToList();

                                foreach (var o in customerIdsLists)
                                {
                                    var dist = context.DistributorVerificationDB.Where(x => x.CustomerID == o.CustomerID).FirstOrDefault();
                                    if (dist != null)
                                    {
                                        var customerData = context.Customers.Where(x => x.CustomerId == o.CustomerID && x.fcmId != null).FirstOrDefault();
                                        if (customerData != null)
                                            customersD.Add(customerData);
                                    }

                                }

                                notification.TotalSent = customersD.Count();
                                notification.Sent = true;
                                context.UpdateNotification(notification);

                                string KeyD = ConfigurationManager.AppSettings["DistributorFcmApiKey"];


                                var objNotificationListD = customersD.Select(x => new FCMRequest

                                {
                                    to = x.fcmId,
                                    CustId = x.CustomerId,
                                    MessageId = "",
                                    data = new FCMData
                                    {
                                        title = notificationTitle,
                                        body = notification.Message,
                                        icon = notification.Pic,
                                        typeId = typeCode,
                                        notificationCategory = notification.NotificationCategory,
                                        notificationType = notification.NotificationType,
                                        notificationId = notification.Id,
                                        IsEnabledDismissNotification = notification.IsEnabledDismissNotification,
                                    }
                                }).ToList();



                                objNotificationListD.ForEach(x =>
                                {
                                    try
                                    {
                                        WebRequest tRequest = WebRequest.Create("https://fcm.googleapis.com/fcm/send") as HttpWebRequest;
                                        tRequest.Method = "post";
                                        string jsonNotificationFormat = Newtonsoft.Json.JsonConvert.SerializeObject(x);
                                        Byte[] byteArray = Encoding.UTF8.GetBytes(jsonNotificationFormat);
                                        tRequest.Headers.Add(string.Format("Authorization: key={0}", KeyD));
                                        //tRequest.Headers.Add(string.Format("Sender: id={0}", id11));
                                        tRequest.ContentLength = byteArray.Length;
                                        tRequest.ContentType = "application/json";
                                        using (Stream dataStream = tRequest.GetRequestStream())
                                        {
                                            dataStream.Write(byteArray, 0, byteArray.Length);
                                            using (WebResponse tResponse = tRequest.GetResponse())
                                            {
                                                using (Stream dataStreamResponse = tResponse.GetResponseStream())
                                                {
                                                    using (StreamReader tReader = new StreamReader(dataStreamResponse))
                                                    {
                                                        String responseFromFirebaseServer = tReader.ReadToEnd();
                                                        FCMResponse response = Newtonsoft.Json.JsonConvert.DeserializeObject<FCMResponse>(responseFromFirebaseServer);
                                                        if (response.success == 1 && response.results != null && response.results.Any() && !string.IsNullOrEmpty(response.results.FirstOrDefault().message_id))
                                                        {
                                                            Console.Write(response);
                                                            x.MessageId = response.results.FirstOrDefault().message_id;
                                                        }
                                                        else if (response.failure == 1)
                                                        {
                                                            Console.Write(response);
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    catch (Exception asd)
                                    {
                                        logger.Error(new StringBuilder("Error while sending FCM for type: ").Append(notification.NotificationType).Append(asd.ToString()).ToString());
                                    }
                                });

                                var deviceNotificationsD = objNotificationListD.Select(x => new DeviceNotification
                                {
                                    CompanyId = 1,
                                    CustomerId = x.CustId,
                                    Deleted = false,
                                    DeviceId = x.to,
                                    ImageUrl = notification.Pic,
                                    Message = notification.Message,
                                    NotificationTime = indianTime,
                                    title = notification.title,
                                    WarehouseId = notification.WarehouseID,
                                    NotificationCategory = notification.NotificationCategory,
                                    NotificationType = notification.NotificationType,
                                    ObjectId = typeCode,
                                    NotificationId = notification.NotificationId,
                                    IsView = notification.TotalViews,
                                    IsReceived = 0,
                                    MessageId = x.MessageId,
                                    NotificationDisplayType = notification.NotificationDisplayType

                                }).ToList();

                                context.DeviceNotificationDb.AddRange(deviceNotificationsD);
                                context.Commit();



                                result = "Notification sent successfully";
                                return result;
                            case "People":

                                List<People> poeples = new List<People>();

                                var poepleIdsList = context.PeopleGroupMappings.Where(x => x.GroupID == notification.GroupID).ToList();
                                foreach (var o in poepleIdsList)
                                {
                                    var peopleData = context.Peoples.Where(x => x.PeopleID == o.PeopleID && x.FcmId != null).FirstOrDefault();
                                    if (peopleData != null)
                                        poeples.Add(peopleData);
                                }

                                notification.TotalSent = poeples.Count();
                                notification.Sent = true;
                                context.UpdateNotification(notification);

                                Key = ConfigurationManager.AppSettings["FcmApiKey"];

                                //var objNotificationListPeople = poeples.Select(x => new
                                //{
                                //    to = x.FcmId,

                                //    data = new
                                //    {
                                //        title = notificationTitle,
                                //        body = notification.Message,
                                //        icon = notification.Pic,
                                //        typeId = typeCode,
                                //        notificationCategory = notification.NotificationCategory,
                                //        notificationType = notification.NotificationType,
                                //        IsEnabledDismissNotification = notification.IsEnabledDismissNotification
                                //    }
                                //}).ToList();

                                var firebaseService2 = new FirebaseNotificationServiceHelper(Key);
                                var Data2 = new FCMData
                                {
                                    title = notificationTitle,
                                    body = notification.Message,
                                    icon = notification.Pic,
                                    typeId = typeCode,
                                    notificationCategory = notification.NotificationCategory,
                                    notificationType = notification.NotificationType,
                                    IsEnabledDismissNotification = notification.IsEnabledDismissNotification
                                };
                                var FcmIds1 = poeples.Select(x => x.FcmId).Distinct().ToList();

                                BackgroundTaskManager.Run(() =>
                                {
                                    FcmIds1.ForEach(x =>
                                    {
                                        try
                                        {
                                            //WebRequest tRequest = WebRequest.Create("https://fcm.googleapis.com/fcm/send") as HttpWebRequest;
                                            //tRequest.Method = "post";
                                            //string jsonNotificationFormat = Newtonsoft.Json.JsonConvert.SerializeObject(x);
                                            //Byte[] byteArray = Encoding.UTF8.GetBytes(jsonNotificationFormat);
                                            //tRequest.Headers.Add(string.Format("Authorization: key={0}", Key));
                                            ////tRequest.Headers.Add(string.Format("Sender: id={0}", id11));
                                            //tRequest.ContentLength = byteArray.Length;
                                            //tRequest.ContentType = "application/json";
                                            //using (Stream dataStream = tRequest.GetRequestStream())
                                            //{
                                            //    dataStream.Write(byteArray, 0, byteArray.Length);
                                            //    using (WebResponse tResponse = tRequest.GetResponse())
                                            //    {
                                            //        using (Stream dataStreamResponse = tResponse.GetResponseStream())
                                            //        {
                                            //            using (StreamReader tReader = new StreamReader(dataStreamResponse))
                                            //            {
                                            //                String responseFromFirebaseServer = tReader.ReadToEnd();
                                            //                FCMResponse response = Newtonsoft.Json.JsonConvert.DeserializeObject<FCMResponse>(responseFromFirebaseServer);
                                            //                if (response.success == 1)
                                            //                {
                                            //                    Console.Write(response);
                                            //                }
                                            //                else if (response.failure == 1)
                                            //                {
                                            //                    Console.Write(response);
                                            //                }
                                            //            }
                                            //        }
                                            //    }
                                            //}
                                            var Result = firebaseService2.SendNotificationForApprovalAsync(x, Data2);
                                            if (Result != null)
                                            {
                                                Console.Write(Result);
                                            }
                                            else
                                            {
                                                Console.Write(Result);
                                            }

                                        }
                                        catch (Exception asd)
                                        {
                                            logger.Error(new StringBuilder("Error while sending FCM for type: ").Append(notification.NotificationType).Append(asd.ToString()).ToString());
                                        }
                                    });

                                });



                                result = "Notification sent successfully";
                                return result;

                            case "Supplier":
                                result = "Notification sent successfully";
                                return result;
                            case "Consumer":
                                List<NotificationCustomerDTO> consumercustomers = new List<NotificationCustomerDTO>();
                                string Sqlstrings = "";
                                if (notification.GroupID > 0)
                                {
                                    if (notification.WarehouseID > 0)
                                    {
                                        Sqlstrings = "select b.CustomerId,b.fcmId from  SalesGroupCustomers a with(nolock) inner join Customers b with(nolock) on a.CustomerID=b.CustomerId and b.fcmId is not null and b.fcmId!=''  and a.GroupID=" + notification.GroupID + " and exists(select top 1 b.CustomerId from ConsumerAddresses c with(nolock)  where b.CustomerId=c.CustomerId and c.IsActive=1 and c.WarehouseId =" + notification.WarehouseID +")" ; //b.Warehouseid=" + notification.WarehouseID;
                                        consumercustomers = context.Database.SqlQuery<NotificationCustomerDTO>(Sqlstrings).ToList();
                                    }
                                    else
                                    {
                                        Sqlstrings = "select b.CustomerId,b.fcmId from SalesGroupCustomers a with(nolock) inner join Customers b with(nolock) on a.CustomerID=b.CustomerId and  b.fcmId is not null and b.fcmId!='' and b.Active=1 and b.Deleted=0   and a.GroupID=" + notification.GroupID + " and exists(select top 1 b.CustomerId from ConsumerAddresses c with(nolock)  where b.CustomerId=c.CustomerId and c.IsActive=1 and c.Cityid =" + notification.CityId + ")";  //and b.cityid=" + notification.CityId + "
                                        consumercustomers = context.Database.SqlQuery<NotificationCustomerDTO>(Sqlstrings).ToList();
                                    }
                                }
                                else if (notification.GroupID == -1)
                                {
                                    if (notification.WarehouseID > 0)
                                        Sqlstrings = "select b.CustomerId,b.fcmId from Customers b with(nolock) where b.IsKPP=0 and b.fcmId is not null and b.fcmId!='' and not exists(select customerid from DistributorVerifications a with(nolock)  where a.CustomerID=b.CustomerId)  and exists(select top 1 b.CustomerId from ConsumerAddresses c with(nolock)  where b.CustomerId=c.CustomerId and c.IsActive=1 and c.WarehouseId =" + notification.WarehouseID + ")";// and b.Warehouseid=" + notification.WarehouseID;
                                    else
                                        Sqlstrings = "select b.CustomerId,b.fcmId from Customers b with(nolock) where b.IsKPP=0 and b.fcmId is not null and b.fcmId!='' and not exists(select customerid from DistributorVerifications a with(nolock)  where a.CustomerID=b.CustomerId) and exists(select top 1 b.CustomerId from ConsumerAddresses c with(nolock)  where b.CustomerId=c.CustomerId and c.IsActive=1 and c.Cityid =" + notification.CityId + ")"; //and b.cityId=" + notification.CityId;
                                    consumercustomers = context.Database.SqlQuery<NotificationCustomerDTO>(Sqlstrings).ToList();
                                }
                                else if (notification.GroupID == -2)
                                {
                                    if (notification.WarehouseID > 0)
                                        Sqlstrings = "select b.CustomerId,b.fcmId from Customers b with(nolock) where b.IsKPP=1 and b.fcmId is not null and b.fcmId!='' and b.Active=1 and b.Deleted=0 and exists(select top 1 b.CustomerId from ConsumerAddresses c with(nolock)  where b.CustomerId=c.CustomerId and c.IsActive=1 and c.WarehouseId =" + notification.WarehouseID + ")"; //and b.Warehouseid=" + notification.WarehouseID;
                                    else
                                        Sqlstrings = "select b.CustomerId,b.fcmId from Customers b with(nolock) where b.IsKPP=1 and b.fcmId is not null and b.fcmId!='' and b.Active=1 and b.Deleted=0 and exists(select top 1 b.CustomerId from ConsumerAddresses c with(nolock)  where b.CustomerId=c.CustomerId and c.IsActive=1 and c.Cityid =" + notification.CityId + ")"; //and b.cityId=" + notification.CityId;
                                    consumercustomers = context.Database.SqlQuery<NotificationCustomerDTO>(Sqlstrings).ToList();
                                }
                                else if (notification.GroupID == -3)
                                {
                                    if (notification.WarehouseID > 0)
                                        Sqlstrings = "select b.CustomerId,b.fcmId from Customers b with(nolock) where  b.fcmId is not null and b.fcmId!='' and b.Active=1 and b.Deleted=0  and exists(select customerid from DistributorVerifications a with(nolock)  where a.CustomerID=b.CustomerId) and  exists(select top 1 b.CustomerId from ConsumerAddresses c with(nolock)  where b.CustomerId=c.CustomerId and c.IsActive=1 and c.WarehouseId =" + notification.WarehouseID + ")"; //b.Warehouseid=" + notification.WarehouseID;
                                    else
                                        Sqlstrings = "select b.CustomerId,b.fcmId from Customers b with(nolock) where  b.fcmId is not null and b.fcmId!='' and b.Active=1 and b.Deleted=0  and exists(select customerid from DistributorVerifications a with(nolock)  where a.CustomerID=b.CustomerId) and exists(select top 1 b.CustomerId from ConsumerAddresses c with(nolock)  where b.CustomerId=c.CustomerId and c.IsActive=1 and c.Cityid =" + notification.CityId + ")"; //and b.cityid=" + notification.CityId;
                                    consumercustomers = context.Database.SqlQuery<NotificationCustomerDTO>(Sqlstrings).ToList();
                                }
                                else if (notification.GroupID == -10)
                                {
                                    //and exists(select customerid from DistributorVerifications a with(nolock)  where a.CustomerID=b.CustomerId)
                                    Sqlstrings = "select b.CustomerId,b.fcmId from CRMNotifiationCustomers a with(nolock) inner join Customers b with(nolock) on a.CustomerId=b.CustomerId where  b.fcmId is not null and b.fcmId!='' and b.Active=1 and b.Deleted=0  and a.NotificationId=" + notification.Id;
                                    consumercustomers = context.Database.SqlQuery<NotificationCustomerDTO>(Sqlstrings).ToList();
                                }
                                else if (notification.GroupID <= -4)
                                {
                                    int Level = -1;
                                    switch (notification.GroupID)
                                    {
                                        case -4:
                                            Level = 0;
                                            break;
                                        case -5:
                                            Level = 1;
                                            break;
                                        case -6:
                                            Level = 2;
                                            break;
                                        case -7:
                                            Level = 3;
                                            break;
                                        case -8:
                                            Level = 4;
                                            break;
                                        case -9:
                                            Level = 5;
                                            break;
                                    }
                                    var fromdate = DateTime.Now;
                                    fromdate = DateTime.Now.AddMonths(-1);
                                    Sqlstrings = "Select distinct a.CustomerId,b.fcmId from CRMCustomerLevels a with(nolock)  inner join Customers b  with(nolock)  on a.CustomerId=b.CustomerId and IsDeleted=0 and b.Warehouseid=" + notification.WarehouseID + " and a.Month=" + fromdate.Month + " and a.Year=" + fromdate.Year + " And a.Level=" + Level;
                                    consumercustomers = context.Database.SqlQuery<NotificationCustomerDTO>(Sqlstrings).ToList();
                                }
                                else
                                {
                                    Sqlstrings = "select b.CustomerId,b.fcmId from Customers b with(nolock) where  b.fcmId is not null and b.fcmId!='' and b.Active=1 and b.Deleted=0  and exists(select top 1 b.CustomerId from ConsumerAddresses c with(nolock)  where b.CustomerId=c.CustomerId and c.IsActive=1 and c.Cityid =" + notification.CityId + ")";                                    //and b.cityid=" + notification.CityId;
                                    consumercustomers = context.Database.SqlQuery<NotificationCustomerDTO>(Sqlstrings).ToList();
                                }

                                //var customerIdsList = notification.GroupID > 0 ? context.GroupMappings.Where(x => x.WarehouseID == notification.WarehouseID && x.GroupID == notification.GroupID).Select(x => new { CustomerID = x.CustomerID }).ToList()
                                //                      : context.Customers.Where(x => x.Warehouseid == notification.WarehouseID && x.Active && !x.Deleted && x.fcmId != null).Select(x => new { CustomerID = x.CustomerId }).ToList();

                                //foreach (var o in customerIdsList)
                                //{
                                //    var dis = context.DistributorVerificationDB.Where(x => x.CustomerID == o.CustomerID).FirstOrDefault();
                                //    if (dis == null)
                                //    {
                                //        var customerData = context.Customers.Where(x => x.CustomerId == o.CustomerID && x.fcmId != null).FirstOrDefault();
                                //        if (customerData != null)
                                //            customers.Add(customerData);
                                //    }

                                //}


                                string Keys = ConfigurationManager.AppSettings["ConsumerFcmKey"];


                                var objNotificationLists = consumercustomers.GroupBy(x => x.fcmId).Select(x => new FCMRequest
                                {
                                    to = x.Key,
                                    CustId = x.FirstOrDefault().CustomerId,
                                    MessageId = "",
                                    data = new FCMData
                                    {
                                        title = notificationTitle,
                                        body = notification.Message,
                                        icon = notification.Pic,
                                        typeId = typeCode,
                                        notificationCategory = notification.NotificationCategory,
                                        notificationType = notification.NotificationType,
                                        notificationId = notification.Id,
                                        notify_type = notification.NotificationDisplayType,
                                        url = notification.NotificationMediaType == "Video" ? notification.Pic : "",
                                        IsEnabledDismissNotification = notification.IsEnabledDismissNotification
                                    }
                                }).ToList();
                                var data = new FCMData
                                {
                                    title = notificationTitle,
                                    body = notification.Message,
                                    icon = notification.Pic,
                                    typeId = typeCode,
                                    notificationCategory = notification.NotificationCategory,
                                    notificationType = notification.NotificationType,
                                    notificationId = notification.Id,
                                    notify_type = notification.NotificationDisplayType,
                                    url = notification.NotificationMediaType == "Video" ? notification.Pic : "",
                                    IsEnabledDismissNotification = notification.IsEnabledDismissNotification
                                };


                                ConcurrentBag<int> totalSents = new ConcurrentBag<int>();
                                ConcurrentBag<int> totalNotSents = new ConcurrentBag<int>();
                                var firebaseService3 = new FirebaseNotificationServiceHelper(Keys);
                                ParallelLoopResult parellelResults = Parallel.ForEach(consumercustomers.GroupBy(x => x.fcmId), (x) =>
                                {
                                    try
                                    {
                                        //var fcmid = "fZGIeP5dTKGd-2JBZttcDj:APA91bGINtqXqKuCcEHd4qXZMN-VtX5KC2g98KkytmGdpc28_-duDu8Ry1P6Kk_Xb9RgRG0iDWrp8DkwE1EPCOPG0OLz2uRjo-HBg-Ysg-mDaMErlYLjJGZE7ScXKIkjmWZ0xNO6UyxN";
                                        var results = firebaseService3.SendNotificationForApprovalAsync(x.Key, data);
                                        if (results != null)
                                        {
                                            totalSents.Add(1);
                                        }
                                        else
                                        {
                                            totalSents.Add(1);
                                        }
                                        //WebRequest tRequest = WebRequest.Create("https://fcm.googleapis.com/fcm/send") as HttpWebRequest;
                                        //tRequest.Method = "post";
                                        //string jsonNotificationFormat = Newtonsoft.Json.JsonConvert.SerializeObject(x);
                                        //Byte[] byteArray = Encoding.UTF8.GetBytes(jsonNotificationFormat);
                                        //tRequest.Headers.Add(string.Format("Authorization: key={0}", Keys));
                                        ////tRequest.Headers.Add(string.Format("Sender: id={0}", id11));
                                        //tRequest.ContentLength = byteArray.Length;
                                        //tRequest.ContentType = "application/json";
                                        //using (Stream dataStream = tRequest.GetRequestStream())
                                        //{
                                        //    dataStream.Write(byteArray, 0, byteArray.Length);
                                        //    using (WebResponse tResponse = tRequest.GetResponse())
                                        //    {
                                        //        using (Stream dataStreamResponse = tResponse.GetResponseStream())
                                        //        {
                                        //            using (StreamReader tReader = new StreamReader(dataStreamResponse))
                                        //            {
                                        //                String responseFromFirebaseServer = tReader.ReadToEnd();
                                        //                FCMResponse response = Newtonsoft.Json.JsonConvert.DeserializeObject<FCMResponse>(responseFromFirebaseServer);
                                        //                if (response.success == 1 && response.results != null && response.results.Any() && !string.IsNullOrEmpty(response.results.FirstOrDefault().message_id))
                                        //                {
                                        //                    totalSents.Add(1);
                                        //                    x.MessageId = response.results.FirstOrDefault().message_id;
                                        //                }
                                        //                else
                                        //                {
                                        //                    totalNotSents.Add(1);
                                        //                }
                                        //            }
                                        //        }
                                        //    }
                                        //}
                                    }
                                    catch (Exception asd)
                                    {
                                        logger.Error(new StringBuilder("Error while sending FCM for type: ").Append(notification.NotificationType).Append(asd.ToString()).ToString());
                                    }
                                });



                                if (parellelResults.IsCompleted && (totalSents.Any() || totalNotSents.Any()))
                                {
                                    notification.TotalSent = totalSents.Count();
                                    notification.Sent = true;
                                    notification.TotalNotSent = totalNotSents.Count();
                                    context.Entry(notification).State = EntityState.Modified;

                                    var deviceNotifications = objNotificationLists.Select(x => new DeviceNotification
                                    {
                                        CompanyId = 1,
                                        CustomerId = x.CustId,
                                        Deleted = false,
                                        DeviceId = x.to,
                                        ImageUrl = notification.Pic,
                                        Message = notification.Message,
                                        NotificationTime = indianTime,
                                        title = notification.title,
                                        WarehouseId = notification.WarehouseID,
                                        NotificationCategory = notification.NotificationCategory,
                                        NotificationType = notification.NotificationType,
                                        ObjectId = typeCode,
                                        NotificationId = notification.Id,
                                        IsView = notification.TotalViews,
                                        IsReceived = 0,
                                        MessageId = x.MessageId,
                                        NotificationDisplayType = notification.NotificationDisplayType

                                    }).ToList();

                                    context.DeviceNotificationDb.AddRange(deviceNotifications);
                                    context.Commit();
                                }

                                result = "Notification sent successfully";
                                return result;

                        }
                    }

                    return result;

                }
                catch (Exception ex)
                {
                    logger.Error("Error in Add message " + ex.Message);

                    return null;
                }
            }
        }

        //[HttpGet]
        [Route("GetNotificationItems")]
        [AcceptVerbs("GET")]
        public async Task<List<factoryItemdata>> GetNotificationItems(string notificationType, int typeId, int wareHouseId)
        {
            List<ItemMaster> items = new List<ItemMaster>();
            using (var myContext = new AuthContext())
            {
                {
                    switch (notificationType)
                    {
                        case "Item":
                            items = myContext.itemMasters.Where(x => x.ItemId == typeId && x.active && !x.Deleted && x.WarehouseId == wareHouseId).ToList();
                            break;

                        case "Offer":
                            items = myContext.itemMasters.Where(x => x.ItemId == typeId && x.active && !x.Deleted && x.WarehouseId == wareHouseId).ToList();
                            break;

                        case "Flash Deal":
                            items = myContext.itemMasters.Where(x => x.ItemId == typeId && x.active && !x.Deleted && x.WarehouseId == wareHouseId).ToList();
                            break;

                        case "Brand":
                            items = myContext.itemMasters.Where(x => x.SubsubCategoryid == typeId && x.active && !x.Deleted && x.WarehouseId == wareHouseId).ToList();
                            break;
                    }
                }

                var retList = items.Select(a => new factoryItemdata
                {
                    WarehouseId = a.WarehouseId,
                    CompanyId = a.CompanyId,
                    Categoryid = a.Categoryid,
                    Discount = a.Discount,
                    ItemId = a.ItemId,
                    ItemNumber = a.Number,
                    itemname = a.itemname,
                    LogoUrl = a.LogoUrl,
                    MinOrderQty = a.MinOrderQty,
                    price = a.price,
                    SubCategoryId = a.SubCategoryId,
                    SubsubCategoryid = a.SubsubCategoryid,
                    TotalTaxPercentage = a.TotalTaxPercentage,
                    SellingUnitName = a.SellingUnitName,
                    SellingSku = a.SellingSku,
                    UnitPrice = a.UnitPrice,
                    HindiName = a.HindiName,
                    VATTax = a.VATTax,
                    active = a.active,
                    marginPoint = a.marginPoint,
                    NetPurchasePrice = a.NetPurchasePrice,
                    promoPerItems = a.promoPerItems,
                    IsOffer = a.IsOffer,
                    Deleted = a.Deleted,
                    OfferCategory = a.OfferCategory,
                    OfferStartTime = a.OfferStartTime,
                    OfferEndTime = a.OfferEndTime,
                    OfferQtyAvaiable = a.OfferQtyAvaiable,
                    OfferQtyConsumed = a.OfferQtyConsumed,
                    OfferId = a.OfferId,
                    OfferType = a.OfferType,
                    OfferWalletPoint = a.OfferWalletPoint,
                    OfferFreeItemId = a.OfferFreeItemId,
                    OfferPercentage = a.OfferPercentage,
                    OfferFreeItemName = a.OfferFreeItemName,
                    OfferFreeItemImage = a.OfferFreeItemImage,
                    OfferFreeItemQuantity = a.OfferFreeItemQuantity,
                    OfferMinimumQty = a.OfferMinimumQty,
                    FlashDealSpecialPrice = a.FlashDealSpecialPrice,
                    FlashDealMaxQtyPersonCanTake = a.OfferMaxQtyPersonCanTake
                }).OrderByDescending(x => x.ItemNumber).ToList();

                foreach (var it in retList)
                {
                    using (var context = new AuthContext())
                        try
                        {  /// Dream Point Logic && Margin Point
                            int? MP, PP;
                            double xPoint = xPointValue * 10;
                            //Customer (0.2 * 10=1)
                            if (it.promoPerItems.Equals(null) && it.promoPerItems == null)
                            {
                                PP = 0;
                            }
                            else
                            {
                                PP = it.promoPerItems;
                            }
                            if (it.marginPoint.Equals(null) && it.promoPerItems == null)
                            {
                                MP = 0;
                            }
                            else
                            {
                                double WithTaxNetPurchasePrice = Math.Round(it.NetPurchasePrice * (1 + (it.TotalTaxPercentage / 100)), 3);//With tax
                                MP = Convert.ToInt32((it.UnitPrice - WithTaxNetPurchasePrice) * xPoint); // (UnitPrice-NPP withtax) * By xpoint 
                            }
                            if (PP > 0 && MP > 0)
                            {
                                int? PP_MP = PP + MP;
                                it.dreamPoint = PP_MP;
                            }
                            else if (MP > 0)
                            {
                                it.dreamPoint = MP;
                            }
                            else if (PP > 0)
                            {
                                it.dreamPoint = PP;
                            }
                            else
                            {
                                it.dreamPoint = 0;
                            }
                            // Margin % On app site logic ((MRP-UnitPrice)*100)/UnitPrice
                            if (it.price > it.UnitPrice)
                            {
                                it.marginPoint = ((it.price - it.UnitPrice) * 100) / it.UnitPrice; //MP;  we replce marginpoint value by margin for app here 
                            }
                            else
                            {
                                it.marginPoint = 0;
                            }
                        }
                        catch { }
                }

                return retList;
            }

        }


        [Route("SendAutoNotification")]
        [HttpGet]
        [AllowAnonymous]
        public bool SendAutoNotification()
        {
            logger.Info("Add message: ");
            using (var context = new AuthContext())
            {
                try
                {

                    var notifications = context.NotificationUpdatedDb.Where(x => x.Sent == false && x.TO.HasValue && x.From.HasValue && (x.TO.Value >= indianTime && x.From <= indianTime) && !x.IsMultiSchedule && !x.IsCRMNotification).ToList();


                    var scheduleNotification = context.NotificationUpdatedDb.Where(x => x.Sent == false && (x.IsMultiSchedule || x.IsCRMNotification) && x.NotificationSchedulers.Any(y => (y.StartDate <= indianTime && y.EndDate >= indianTime) && !y.Sent)).Include(x => x.NotificationSchedulers).ToList();

                    if (notifications == null) notifications = new List<NotificationUpdated>();
                    if (scheduleNotification != null && scheduleNotification.Any()) notifications.AddRange(scheduleNotification);
                    foreach (var notification in notifications)
                    {
                        var isSend = true;
                        if (isSend)
                        {
                            var typeCode = 0;
                            var notificationTitle = "";
                            if (notification.NotificationType == "Actionable")
                            {
                                switch (notification.NotificationCategory)
                                {
                                    case "Item":
                                        notificationTitle = notification.title + " | " + notification.ItemName;
                                        typeCode = notification.ItemCode;
                                        break;
                                    case "Brand":
                                        notificationTitle = notification.title + " | " + notification.BrandName;
                                        typeCode = notification.BrandCode;
                                        break;
                                    case "Flash Deal":
                                        notificationTitle = notification.title;
                                        typeCode = notification.ItemCode;
                                        break;
                                    case "Offer":
                                        notificationTitle = notification.title;
                                        typeCode = notification.ItemCode;
                                        break;
                                    case "Category":
                                        notificationTitle = notification.title + " | " + notification.ItemName;
                                        typeCode = notification.ItemCode;
                                        break;
                                    case "SubCategory":
                                        notificationTitle = notification.title + " | " + notification.ItemName;
                                        typeCode = notification.ItemCode;
                                        break;
                                    default:
                                        notificationTitle = notification.title;
                                        notification.NotificationDisplayType = notification.NotificationCategory;
                                        break;
                                }
                            }
                            else
                            {
                                notificationTitle = notification.title;
                            }


                            switch (notification.GroupAssociation)
                            {
                                case "SalesApp":
                                    var sent = false;
                                    ConcurrentBag<int> totalSentCount = new ConcurrentBag<int>();
                                    ConcurrentBag<int> totalNotSentCount = new ConcurrentBag<int>();
                                    notification.StoreIds = context.StoreNotifications.Where(x => x.NotificationId == notification.Id && !x.Sent).Select(x => (int)x.StoreId).ToList();
                                    foreach (var strid in notification.StoreIds)
                                    {
                                        var FCMIds = (from x in context.ClusterStoreExecutives
                                                      join y in context.Peoples
                                                      on x.ExecutiveId equals y.PeopleID
                                                      where x.StoreId == strid && y.WarehouseId == notification.WarehouseID && x.IsActive == true && x.IsDeleted == false
                                                            && y.Active == true && y.Deleted == false
                                                            && y.FcmId == "eRLqNgl7R4-o6wU5OmeDxP:APA91bFyjfPzyLSoE1Cfk0XUk1mw1O-H_gXntj5sYl18t1r1XZF20hqFGQmFZ4Pl9O91cO9YjiC9LxC4cSUGdWyQGF3EecXyzFz_3hSRAJot3cmxPAFdPq9BFzqzSIPsjBXz1uLcxVE7"
                                                      select new
                                                      {
                                                          y.FcmId,
                                                          y.PeopleID,
                                                          y.UpdatedDate
                                                      }).ToList();

                                        FCMIds = FCMIds.Where(x => x.FcmId != null).Distinct().ToList();
                                        if (FCMIds.Any())
                                        {
                                            var DistincFcmIds = FCMIds.Select(y => new { FcmId = y.FcmId }).Distinct().ToList();
                                            string Key1 = ConfigurationManager.AppSettings["SalesFcmApiKeyNew"];
                                            var firebaseService1 = new FirebaseNotificationServiceHelper(Key1);
                                            foreach (var fcmobj in DistincFcmIds)
                                            {

                                                //var fcmdcs = new FcmidDCss();
                                                //fcmdcs.body = notification.Message;
                                                //fcmdcs.title = notification.title;
                                                //fcmdcs.notify_type = "general_notification";
                                                //fcmdcs.image_url = notification.Pic;
                                                //fcmdcs.IsEnabledDismissNotification = notification.IsEnabledDismissNotification;
                                                //var fcmdc = new FcmidDC();
                                                //fcmdc.to = fcmobj.FcmId;
                                                //fcmdc.data = fcmdcs;
                                                var data = new FCMData
                                                {
                                                    title = notification.title,
                                                    body = notification.Message,
                                                    icon = "",
                                                    notificationCategory = "",
                                                    notificationType = "",
                                                    notify_type = "general_notification",
                                                    url = "",
                                                    image_url = notification.Pic,
                                                    IsEnabledDismissNotification = notification.IsEnabledDismissNotification
                                                };
                                                try
                                                {

                                                    //var fcmid = "fZGIeP5dTKGd-2JBZttcDj:APA91bGINtqXqKuCcEHd4qXZMN-VtX5KC2g98KkytmGdpc28_-duDu8Ry1P6Kk_Xb9RgRG0iDWrp8DkwE1EPCOPG0OLz2uRjo-HBg-Ysg-mDaMErlYLjJGZE7ScXKIkjmWZ0xNO6UyxN";
                                                    var results = firebaseService1.SendNotificationForApprovalAsync(fcmobj.FcmId, data);
                                                    if (results != null)
                                                    {
                                                        totalSentCount.Add(1);
                                                        //fcmdc.MessageId = results.ToString();
                                                        //result = "Notification sent successfully";
                                                        sent = true;
                                                    }
                                                    else
                                                    {
                                                        totalNotSentCount.Add(1);
                                                        //result = "Notification Not sent";
                                                    }
                                                    //WebRequest tRequest = WebRequest.Create("https://fcm.googleapis.com/fcm/send") as HttpWebRequest;
                                                    //tRequest.Method = "POST";
                                                    //string jsonNotificationFormat = Newtonsoft.Json.JsonConvert.SerializeObject(fcmdc);
                                                    //Byte[] byteArray = Encoding.UTF8.GetBytes(jsonNotificationFormat);
                                                    //tRequest.Headers.Add(string.Format("Authorization: key={0}", Key1));
                                                    //tRequest.ContentLength = byteArray.Length;
                                                    //tRequest.ContentType = "application/json";
                                                    //using (Stream dataStream = tRequest.GetRequestStream())
                                                    //{
                                                    //    dataStream.Write(byteArray, 0, byteArray.Length);
                                                    //    using (WebResponse tResponse = tRequest.GetResponse())
                                                    //    {
                                                    //        using (Stream dataStreamResponse = tResponse.GetResponseStream())
                                                    //        {
                                                    //            using (StreamReader tReader = new StreamReader(dataStreamResponse))
                                                    //            {
                                                    //                String responseFromFirebaseServer = tReader.ReadToEnd();
                                                    //                AngularJSAuthentication.API.Controllers.NotificationController.FCMResponse response = Newtonsoft.Json.JsonConvert.DeserializeObject<AngularJSAuthentication.API.Controllers.NotificationController.FCMResponse>(responseFromFirebaseServer);
                                                    //                if (response.success == 1)
                                                    //                {
                                                    //                    totalSentCount.Add(1);
                                                    //                    fcmdc.MessageId = response.results.FirstOrDefault().message_id;
                                                    //                    sent = true;
                                                    //                }
                                                    //                else if (response.failure == 1)
                                                    //                {
                                                    //                    totalNotSentCount.Add(1);
                                                    //                }
                                                    //            }
                                                    //        }
                                                    //    }
                                                    //}
                                                }
                                                catch (Exception asd)
                                                {
                                                    logger.Error(new StringBuilder("Error while sending FCM for type: ").Append(asd.ToString()).ToString());
                                                }
                                                if (totalSentCount.Any() || totalNotSentCount.Any())
                                                {
                                                    var peopleId = FCMIds.Where(x => x.FcmId == fcmobj.FcmId).OrderByDescending(x => x.UpdatedDate).Select(p => p.PeopleID).FirstOrDefault();
                                                    ExecutiveDeviceNotification execDeviceNotification = new ExecutiveDeviceNotification
                                                    {
                                                        CompanyId = 1,
                                                        WarehouseId = notification.WarehouseID,
                                                        ExecutiveId = peopleId,//(int)fcmobj.PeopleID,
                                                        DeviceId = fcmobj.FcmId,
                                                        title = notification.title,
                                                        Message = notification.Message,
                                                        MessageId = "",
                                                        ImageUrl = notification.Pic,
                                                        NotificationTime = indianTime,
                                                        Deleted = false,
                                                        NotificationCategory = notification.NotificationCategory,
                                                        NotificationType = notification.NotificationType,
                                                        NotificationId = notification.Id,
                                                        IsReceived = 0,
                                                        NotificationDisplayType = notification.NotificationDisplayType
                                                    };
                                                    context.ExecutiveDeviceNotifications.Add(execDeviceNotification);
                                                }
                                            }
                                        }
                                    }
                                    if (totalSentCount.Any() || totalNotSentCount.Any())
                                    {
                                        notification.TotalNotSent = totalNotSentCount.Count();
                                        notification.TotalSent = totalSentCount.Count();
                                        notification.Sent = true;
                                        context.Entry(notification).State = EntityState.Modified;
                                        if (notification.IsMultiSchedule)
                                        {
                                            var notifyScedule = context.NotificationSchedulers.Where(x => x.NotificationUpdatedId == notification.Id).ToList();
                                            foreach (var data in notifyScedule)
                                            {
                                                data.Sent = true;
                                                data.TotalSent = totalSentCount.Count();
                                                data.TotalNotSent = totalNotSentCount.Count();
                                                context.Entry(data).State = EntityState.Modified;
                                            }
                                        }
                                        var res = context.StoreNotifications.Where(x => x.NotificationId == notification.Id && !x.Sent).ToList();
                                        foreach (var data in res)
                                        {
                                            data.Sent = true;
                                            context.Entry(data).State = EntityState.Modified;
                                        }
                                        context.Commit();
                                    }
                                    break;


                                case "Retailer":
                                    //  List<Customer> customers = new List<Customer>();
                                    List<NotificationCustomerDTO> customers = new List<NotificationCustomerDTO>();

                                    string Sqlstring = "";
                                    if (notification.IsCRMNotification)
                                    {
                                        Sqlstring = "select b.CustomerId,b.fcmId,n.Id as SchedulerId from CRMNotifiationCustomers a with(nolock) inner join NotificationSchedulers n with(nolock)  on n.id=a.SchedulerId inner join Customers b with(nolock) on a.CustomerID=b.CustomerId and b.fcmId is not null and b.fcmId!='' and n.StartDate <= GETDATE() and n.EndDate >= GETDATE()  and a.[IsSent]=0 and n.[Sent]=0 and a.NotificationId=" + notification.Id;
                                        customers = context.Database.SqlQuery<NotificationCustomerDTO>(Sqlstring).ToList();
                                    }
                                    else if (notification.GroupID > 0)
                                    {
                                        Sqlstring = "select b.CustomerId,b.fcmId from SalesGroupCustomers a with(nolock) inner join Customers b with(nolock) on a.CustomerID=b.CustomerId and b.fcmId is not null and b.fcmId!=''  and a.GroupID=" + notification.GroupID + " and b.Warehouseid=" + notification.WarehouseID;
                                        customers = context.Database.SqlQuery<NotificationCustomerDTO>(Sqlstring).ToList();
                                    }
                                    else if (notification.GroupID == -1)
                                    {
                                        Sqlstring = "select b.CustomerId,b.fcmId from Customers b with(nolock) where b.IsKPP=0 and b.fcmId is not null and b.fcmId!='' and not exists(select customerid from DistributorVerifications a with(nolock)  where a.CustomerID=b.CustomerId) and b.Warehouseid=" + notification.WarehouseID;
                                        customers = context.Database.SqlQuery<NotificationCustomerDTO>(Sqlstring).ToList();
                                    }
                                    else if (notification.GroupID == -2)
                                    {
                                        Sqlstring = "select b.CustomerId,b.fcmId from Customers b with(nolock) where b.IsKPP=1 and b.fcmId is not null and b.fcmId!='' and b.Active=1 and b.Deleted=0 and b.Warehouseid=" + notification.WarehouseID;
                                        customers = context.Database.SqlQuery<NotificationCustomerDTO>(Sqlstring).ToList();
                                    }
                                    else if (notification.GroupID == -3)
                                    {
                                        Sqlstring = "select b.CustomerId,b.fcmId from Customers b with(nolock) where  b.fcmId is not null and b.fcmId!='' and b.Active=1 and b.Deleted=0  and exists(select customerid from DistributorVerifications a with(nolock)  where a.CustomerID=b.CustomerId) and b.Warehouseid=" + notification.WarehouseID;
                                        customers = context.Database.SqlQuery<NotificationCustomerDTO>(Sqlstring).ToList();
                                    }
                                    else if (notification.GroupID == -10)
                                    {
                                        //and exists(select customerid from DistributorVerifications a with(nolock)  where a.CustomerID=b.CustomerId)
                                        Sqlstring = "select b.CustomerId,b.fcmId from CRMNotifiationCustomers a with(nolock) inner join Customers b with(nolock) on a.CustomerId=b.CustomerId where  b.fcmId is not null and b.fcmId!='' and b.Active=1 and b.Deleted=0  and a.NotificationId=" + notification.Id;
                                        customers = context.Database.SqlQuery<NotificationCustomerDTO>(Sqlstring).ToList();
                                    }
                                    else if (notification.GroupID <= -4)
                                    {
                                        int Level = -1;
                                        switch (notification.GroupID)
                                        {
                                            case -4:
                                                Level = 0;
                                                break;
                                            case -5:
                                                Level = 1;
                                                break;
                                            case -6:
                                                Level = 2;
                                                break;
                                            case -7:
                                                Level = 3;
                                                break;
                                            case -8:
                                                Level = 4;
                                                break;
                                            case -9:
                                                Level = 5;
                                                break;
                                        }
                                        var fromdate = DateTime.Now;
                                        fromdate = DateTime.Now.AddMonths(-1);
                                        Sqlstring = "Select distinct a.CustomerId from CRMCustomerLevels a with(nolock)  inner join Customers b  with(nolock)  on a.CustomerId=b.CustomerId and IsDeleted=0 and b.Warehouseid=" + notification.WarehouseID + " and a.Month=" + fromdate.Month + " and a.Year=" + fromdate.Year + " And a.Level=" + Level;
                                        customers = context.Database.SqlQuery<NotificationCustomerDTO>(Sqlstring).ToList();
                                    }
                                    else
                                    {
                                        Sqlstring = "select b.CustomerId,b.fcmId from Customers b with(nolock) where  b.fcmId is not null and b.fcmId!='' and b.Active=1 and b.Deleted=0  and b.Warehouseid=" + notification.WarehouseID;
                                        customers = context.Database.SqlQuery<NotificationCustomerDTO>(Sqlstring).ToList();
                                    }

                                    string Key = ConfigurationManager.AppSettings["FcmApiKey"];


                                    var objNotificationList = customers.GroupBy(x => x.fcmId).Select(x => new FCMRequest
                                    {
                                        to = x.Key,
                                        CustId = x.FirstOrDefault().CustomerId,
                                        MessageId = "",
                                        data = new FCMData
                                        {
                                            title = notificationTitle,
                                            body = notification.Message,
                                            icon = notification.Pic,
                                            typeId = typeCode,
                                            notificationCategory = notification.NotificationCategory,
                                            notificationType = notification.NotificationType,
                                            notificationId = notification.Id,
                                            notify_type = notification.NotificationDisplayType,
                                            url = notification.NotificationMediaType == "Video" ? notification.Pic : "",
                                            IsEnabledDismissNotification = notification.IsEnabledDismissNotification
                                        }
                                    }).ToList();

                                    var firebaseService3 = new FirebaseNotificationServiceHelper(Key);
                                    var Data3 = new FCMData
                                    {
                                        title = notificationTitle,
                                        body = notification.Message,
                                        icon = notification.Pic,
                                        typeId = typeCode,
                                        notificationCategory = notification.NotificationCategory,
                                        notificationType = notification.NotificationType,
                                        notificationId = notification.Id,
                                        notify_type = notification.NotificationDisplayType,
                                        url = notification.NotificationMediaType == "Video" ? notification.Pic : "",
                                        IsEnabledDismissNotification = notification.IsEnabledDismissNotification
                                    };
                                    var FcmIds3 = customers.Select(x => x.fcmId).Distinct().ToList();

                                    ConcurrentBag<int> totalSent = new ConcurrentBag<int>();
                                    ConcurrentBag<int> totalNotSent = new ConcurrentBag<int>();
                                    var notificationSchedulers = notification.NotificationSchedulers != null ? notification.NotificationSchedulers.FirstOrDefault(y => (y.StartDate <= indianTime && y.EndDate >= indianTime) && !y.Sent) : null;
                                    if (notificationSchedulers != null || !notification.IsMultiSchedule)
                                    {
                                        ParallelLoopResult parellelResult = Parallel.ForEach(FcmIds3, async (x) =>
                                         {
                                             try
                                             {
                                                //WebRequest tRequest = WebRequest.Create("https://fcm.googleapis.com/fcm/send") as HttpWebRequest;
                                                //tRequest.Method = "post";
                                                //string jsonNotificationFormat = Newtonsoft.Json.JsonConvert.SerializeObject(x);
                                                //Byte[] byteArray = Encoding.UTF8.GetBytes(jsonNotificationFormat);
                                                //tRequest.Headers.Add(string.Format("Authorization: key={0}", Key));
                                                ////tRequest.Headers.Add(string.Format("Sender: id={0}", id11));
                                                //tRequest.ContentLength = byteArray.Length;
                                                //tRequest.ContentType = "application/json";
                                                //using (Stream dataStream = tRequest.GetRequestStream())
                                                //{
                                                //    dataStream.Write(byteArray, 0, byteArray.Length);
                                                //    using (WebResponse tResponse = tRequest.GetResponse())
                                                //    {
                                                //        using (Stream dataStreamResponse = tResponse.GetResponseStream())
                                                //        {
                                                //            using (StreamReader tReader = new StreamReader(dataStreamResponse))
                                                //            {
                                                //                String responseFromFirebaseServer = tReader.ReadToEnd();
                                                //                FCMResponse response = Newtonsoft.Json.JsonConvert.DeserializeObject<FCMResponse>(responseFromFirebaseServer);
                                                //                if (response.success == 1 && response.results != null && response.results.Any() && !string.IsNullOrEmpty(response.results.FirstOrDefault().message_id))
                                                //                {
                                                //                    totalSent.Add(1);
                                                //                    x.MessageId = response.results.FirstOrDefault().message_id;
                                                //                }
                                                //                else
                                                //                {
                                                //                    totalNotSent.Add(1);
                                                //                    x.MessageId = "";
                                                //                }
                                                //            }
                                                //        }
                                                //    }
                                                //}

                                                var Result = firebaseService3.SendNotificationForApprovalAsync(x, Data3);
                                                 if (Result != null)
                                                 {
                                                     totalSent.Add(1);
                                                 }
                                                 else
                                                 {
                                                     totalNotSent.Add(1);
                                                 }
                                             }
                                             catch (Exception asd)
                                             {
                                                 logger.Error(new StringBuilder("Error while sending FCM for type: ").Append(notification.NotificationType).Append(asd.ToString()).ToString());
                                             }
                                         });

                                        if (parellelResult.IsCompleted && (totalSent.Any() || totalNotSent.Any()))
                                        {
                                            var sentnoti = true;
                                            if (notification.IsMultiSchedule)
                                            {
                                                notificationSchedulers.Sent = true;
                                                notificationSchedulers.TotalSent = totalSent.Count();
                                                notificationSchedulers.TotalNotSent = totalNotSent.Count();
                                                sentnoti = notification.NotificationSchedulers.All(x => x.Sent);
                                                //context.Entry(notificationSchedulers).State = EntityState.Modified;
                                            }
                                            notification.TotalSent += totalSent.Count();
                                            notification.Sent = sentnoti;
                                            notification.TotalNotSent += totalNotSent.Count();
                                            context.Entry(notification).State = EntityState.Modified;

                                            if (notification.IsCRMNotification)
                                            {
                                                var IdDt = new System.Data.DataTable();
                                                IdDt = new System.Data.DataTable();
                                                IdDt.Columns.Add("IntValue");
                                                foreach (var item in customers.Select(v => v.SchedulerId).ToList())
                                                {
                                                    var dr = IdDt.NewRow();
                                                    dr["IntValue"] = item;
                                                    IdDt.Rows.Add(dr);
                                                }
                                                var param = new SqlParameter("SchedulerIds", IdDt);
                                                param.SqlDbType = System.Data.SqlDbType.Structured;
                                                param.TypeName = "dbo.IntValues";
                                                context.Database.CommandTimeout = 300;

                                                int SchedulerIdCount = context.Database.ExecuteSqlCommand("Exec UpdateCRMNotificationScheduler @SchedulerIds", param);
                                            }
                                            context.Commit();
                                            var deviceNotifications = objNotificationList.Select(x => new DeviceNotification
                                            {
                                                CompanyId = 1,
                                                CustomerId = x.CustId,
                                                Deleted = false,
                                                DeviceId = x.to,
                                                ImageUrl = notification.Pic,
                                                Message = notification.Message,
                                                NotificationTime = indianTime,
                                                title = notification.title,
                                                WarehouseId = notification.WarehouseID,
                                                NotificationCategory = notification.NotificationCategory,
                                                NotificationType = notification.NotificationType,
                                                ObjectId = typeCode,
                                                NotificationId = notification.Id,
                                                MessageId = x.MessageId,
                                                IsView = notification.TotalViews,
                                                IsReceived = 0,
                                                NotificationDisplayType = notification.NotificationDisplayType

                                            }).ToList();

                                            var deviceNotificationOpt = new BulkOperations();
                                            deviceNotificationOpt.Setup<DeviceNotification>(x => x.ForCollection(deviceNotifications))
                                                .WithTable("DeviceNotifications")
                                                .WithBulkCopyBatchSize(4000)
                                                .WithBulkCopyCommandTimeout(720) // Default is 600 seconds
                                                .WithSqlCommandTimeout(720) // Default is 600 seconds
                                                .AddAllColumns()
                                                .BulkInsert();
                                            deviceNotificationOpt.CommitTransaction("AuthContext");

                                            //context.DeviceNotificationDb.AddRange(deviceNotifications);
                                            //context.Commit();
                                        }
                                    }
                                    break;
                                case "People":

                                    List<People> poeples = new List<People>();

                                    var poepleIdsList = context.PeopleGroupMappings.Where(x => x.GroupID == notification.GroupID).ToList();
                                    foreach (var o in poepleIdsList)
                                    {
                                        var peopleData = context.Peoples.Where(x => x.PeopleID == o.PeopleID && x.FcmId != null).FirstOrDefault();
                                        if (peopleData != null)
                                            poeples.Add(peopleData);
                                    }

                                    notification.TotalSent = poeples.Count();
                                    notification.Sent = true;
                                    context.UpdateNotification(notification);

                                    Key = ConfigurationManager.AppSettings["FcmApiKey"];


                                    //var objNotificationListPeople = poeples.Select(x => new
                                    //{
                                    //    to = x.FcmId,

                                    //    data = new
                                    //    {
                                    //        title = notificationTitle,
                                    //        body = notification.Message,
                                    //        icon = notification.Pic,
                                    //        typeId = typeCode,
                                    //        notificationCategory = notification.NotificationCategory,
                                    //        notificationType = notification.NotificationType,
                                    //        IsEnabledDismissNotification = notification.IsEnabledDismissNotification
                                    //    }
                                    //}).ToList();
                                    var firebaseService4 = new FirebaseNotificationServiceHelper(Key);
                                    var Data4 = new FCMData
                                    {
                                        title = notificationTitle,
                                        body = notification.Message,
                                        icon = notification.Pic,
                                        typeId = typeCode,
                                        notificationCategory = notification.NotificationCategory,
                                        notificationType = notification.NotificationType,
                                        IsEnabledDismissNotification = notification.IsEnabledDismissNotification
                                    };
                                    var FcmIds4 = poeples.Select(x => x.FcmId).Distinct().ToList();

                                    BackgroundTaskManager.Run(() =>
                                    {
                                        FcmIds4.ForEach(x =>
                                        {
                                            try
                                            {
                                                //WebRequest tRequest = WebRequest.Create("https://fcm.googleapis.com/fcm/send") as HttpWebRequest;
                                                //tRequest.Method = "post";
                                                //string jsonNotificationFormat = Newtonsoft.Json.JsonConvert.SerializeObject(x);
                                                //Byte[] byteArray = Encoding.UTF8.GetBytes(jsonNotificationFormat);
                                                //tRequest.Headers.Add(string.Format("Authorization: key={0}", Key));
                                                ////tRequest.Headers.Add(string.Format("Sender: id={0}", id11));
                                                //tRequest.ContentLength = byteArray.Length;
                                                //tRequest.ContentType = "application/json";
                                                //using (Stream dataStream = tRequest.GetRequestStream())
                                                //{
                                                //    dataStream.Write(byteArray, 0, byteArray.Length);
                                                //    using (WebResponse tResponse = tRequest.GetResponse())
                                                //    {
                                                //        using (Stream dataStreamResponse = tResponse.GetResponseStream())
                                                //        {
                                                //            using (StreamReader tReader = new StreamReader(dataStreamResponse))
                                                //            {
                                                //                String responseFromFirebaseServer = tReader.ReadToEnd();
                                                //                FCMResponse response = Newtonsoft.Json.JsonConvert.DeserializeObject<FCMResponse>(responseFromFirebaseServer);
                                                //                if (response.success == 1 && response.results != null && response.results.Any() && !string.IsNullOrEmpty(response.results.FirstOrDefault().message_id))
                                                //                {
                                                //                    Console.Write(response);

                                                //                }
                                                //                else if (response.failure == 1)
                                                //                {
                                                //                    Console.Write(response);
                                                //                }
                                                //            }
                                                //        }
                                                //    }
                                                //}
                                                var Result = firebaseService4.SendNotificationForApprovalAsync(x, Data4);
                                                if (Result != null)
                                                {
                                                    Console.Write(Result);
                                                }
                                                else
                                                {
                                                    Console.Write(Result);
                                                }
                                            }
                                            catch (Exception asd)
                                            {
                                                logger.Error(new StringBuilder("Error while sending FCM for type: ").Append(notification.NotificationType).Append(asd.ToString()).ToString());
                                            }
                                        });

                                    });
                                    break;
                                case "Supplier":
                                    var msg = "Implement Later";
                                    break;
                                case "DistributorAPP":

                                    customers = new List<NotificationCustomerDTO>();
                                    if (notification.GroupID > 0)
                                    {
                                        Sqlstring = "select b.CustomerId,b.fcmId from GroupMappings a with(nolock) inner join Customers b with(nolock) on a.CustomerID=b.CustomerId and b.fcmId is not null and b.fcmId!=''  and a.GroupID=" + notification.GroupID + " and b.Warehouseid=" + notification.WarehouseID;
                                        customers = context.Database.SqlQuery<NotificationCustomerDTO>(Sqlstring).ToList();
                                    }
                                    else if (notification.GroupID == -1)
                                    {
                                        Sqlstring = "select b.CustomerId,b.fcmId from Customers b with(nolock) where b.IsKPP=0 and b.fcmId is not null and b.fcmId!='' and exists(select customerid from DistributorVerifications a with(nolock)  where a.CustomerID=b.CustomerId) and b.Warehouseid=" + notification.WarehouseID;
                                        customers = context.Database.SqlQuery<NotificationCustomerDTO>(Sqlstring).ToList();
                                    }

                                    if (customers != null && customers.Any())
                                    {
                                        var DobjNotificationList = customers.Select(x => new FCMRequest
                                        {
                                            to = x.fcmId,
                                            CustId = x.CustomerId,
                                            MessageId = "",
                                            data = new FCMData
                                            {
                                                title = notificationTitle,
                                                body = notification.Message,
                                                icon = notification.Pic,
                                                typeId = typeCode,
                                                notificationCategory = notification.NotificationCategory,
                                                notificationType = notification.NotificationType,
                                                notificationId = notification.Id,
                                                IsEnabledDismissNotification = notification.IsEnabledDismissNotification
                                            }
                                        }).ToList();

                                        string DKey = ConfigurationManager.AppSettings["DistributorFcmApiKey"];
                                        ConcurrentBag<int> dtotalSent = new ConcurrentBag<int>();
                                        ConcurrentBag<int> dtotalNotSent = new ConcurrentBag<int>();
                                        var dnotificationSchedulers = notification.NotificationSchedulers.FirstOrDefault(y => (y.StartDate <= indianTime && y.EndDate >= indianTime) && !y.Sent);
                                        if (dnotificationSchedulers != null)
                                        {
                                            ParallelLoopResult parellelResult = Parallel.ForEach(DobjNotificationList, (x) =>
                                            {
                                                try
                                                {
                                                    WebRequest tRequest = WebRequest.Create("https://fcm.googleapis.com/fcm/send") as HttpWebRequest;
                                                    tRequest.Method = "post";
                                                    string jsonNotificationFormat = Newtonsoft.Json.JsonConvert.SerializeObject(x);
                                                    Byte[] byteArray = Encoding.UTF8.GetBytes(jsonNotificationFormat);
                                                    tRequest.Headers.Add(string.Format("Authorization: key={0}", DKey));
                                                    //tRequest.Headers.Add(string.Format("Sender: id={0}", id11));
                                                    tRequest.ContentLength = byteArray.Length;
                                                    tRequest.ContentType = "application/json";
                                                    using (Stream dataStream = tRequest.GetRequestStream())
                                                    {
                                                        dataStream.Write(byteArray, 0, byteArray.Length);
                                                        using (WebResponse tResponse = tRequest.GetResponse())
                                                        {
                                                            using (Stream dataStreamResponse = tResponse.GetResponseStream())
                                                            {
                                                                using (StreamReader tReader = new StreamReader(dataStreamResponse))
                                                                {
                                                                    String responseFromFirebaseServer = tReader.ReadToEnd();
                                                                    FCMResponse response = Newtonsoft.Json.JsonConvert.DeserializeObject<FCMResponse>(responseFromFirebaseServer);
                                                                    if (response.success == 1 && response.results != null && response.results.Any() && !string.IsNullOrEmpty(response.results.FirstOrDefault().message_id))
                                                                    {
                                                                        dtotalSent.Add(1);
                                                                        x.MessageId = response.results.FirstOrDefault().message_id;
                                                                    }
                                                                    else if (response.failure == 1)
                                                                    {
                                                                        dtotalNotSent.Add(1);
                                                                        x.MessageId = "";
                                                                    }
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                                catch (Exception asd)
                                                {
                                                    logger.Error(new StringBuilder("Error while sending FCM for type: ").Append(notification.NotificationType).Append(asd.ToString()).ToString());
                                                }
                                            });

                                            if (parellelResult.IsCompleted && (dtotalSent.Any() || dtotalNotSent.Any()))
                                            {
                                                var sentnoti = true;
                                                if (notification.IsMultiSchedule)
                                                {
                                                    dnotificationSchedulers.Sent = true;
                                                    dnotificationSchedulers.TotalSent = dtotalSent.Count();
                                                    sentnoti = notification.NotificationSchedulers.All(x => x.Sent);
                                                }
                                                notification.TotalSent += dtotalSent.Count();
                                                notification.Sent = sentnoti;
                                                context.Entry(notification).State = EntityState.Modified;
                                                context.Commit();
                                                var deviceNotifications = DobjNotificationList.Select(x => new DeviceNotification
                                                {
                                                    CompanyId = 1,
                                                    CustomerId = x.CustId,
                                                    Deleted = false,
                                                    DeviceId = x.to,
                                                    ImageUrl = notification.Pic,
                                                    Message = notification.Message,
                                                    NotificationTime = indianTime,
                                                    title = notification.title,
                                                    WarehouseId = notification.WarehouseID,
                                                    NotificationCategory = notification.NotificationCategory,
                                                    NotificationType = notification.NotificationType,
                                                    ObjectId = typeCode,
                                                    NotificationId = notification.Id,
                                                    IsView = notification.TotalViews,
                                                    IsReceived = 0,
                                                    MessageId = x.MessageId,
                                                    NotificationDisplayType = notification.NotificationDisplayType

                                                }).ToList();


                                                var deviceNotificationOpt = new BulkOperations();
                                                deviceNotificationOpt.Setup<DeviceNotification>(x => x.ForCollection(deviceNotifications))
                                                    .WithTable("DeviceNotifications")
                                                    .WithBulkCopyBatchSize(4000)
                                                    .WithBulkCopyCommandTimeout(720) // Default is 600 seconds
                                                    .WithSqlCommandTimeout(720) // Default is 600 seconds
                                                    .AddAllColumns()
                                                    .BulkInsert();
                                                deviceNotificationOpt.CommitTransaction("AuthContext");
                                            }
                                        }
                                    }
                                    break;
                                case "Consumer":
                                    List<NotificationCustomerDTO> consumercustomers = new List<NotificationCustomerDTO>();

                                    string Sqlstrings = "";
                                    if (notification.IsCRMNotification)
                                    {
                                        Sqlstrings = "select b.CustomerId,b.fcmId,n.Id as SchedulerId from CRMNotifiationCustomers a with(nolock) inner join NotificationSchedulers n with(nolock)  on n.id=a.SchedulerId inner join Customers b with(nolock) on a.CustomerID=b.CustomerId and b.fcmId is not null and b.fcmId!='' and n.StartDate <= GETDATE() and n.EndDate >= GETDATE()  and a.[IsSent]=0 and n.[Sent]=0 and a.NotificationId=" + notification.Id;
                                        consumercustomers = context.Database.SqlQuery<NotificationCustomerDTO>(Sqlstrings).ToList();
                                    }
                                    else if (notification.GroupID > 0)
                                    {
                                        Sqlstrings = "select b.CustomerId,b.fcmId from SalesGroupCustomers a with(nolock) inner join Customers b with(nolock) on a.CustomerID=b.CustomerId and b.fcmId is not null and b.fcmId!=''  and a.GroupID=" + notification.GroupID + " and exists(select top 1 b.CustomerId from ConsumerAddresses c with(nolock)  where b.CustomerId=c.CustomerId and c.IsActive=1 and c.WarehouseId =" + notification.WarehouseID + ")";  //and b.Warehouseid=" + notification.WarehouseID;
                                        consumercustomers = context.Database.SqlQuery<NotificationCustomerDTO>(Sqlstrings).ToList();
                                    }
                                    else if (notification.GroupID == -1)
                                    {
                                        Sqlstrings = "select b.CustomerId,b.fcmId from Customers b with(nolock) where b.IsKPP=0 and b.fcmId is not null and b.fcmId!='' and not exists(select customerid from DistributorVerifications a with(nolock)  where a.CustomerID=b.CustomerId) and and exists(select top 1 b.CustomerId from ConsumerAddresses c with(nolock)  where b.CustomerId=c.CustomerId and c.IsActive=1 and c.WarehouseId =" + notification.WarehouseID + ")"; //b.Warehouseid=" + notification.WarehouseID;
                                        consumercustomers = context.Database.SqlQuery<NotificationCustomerDTO>(Sqlstrings).ToList();
                                    }
                                    else if (notification.GroupID == -2)
                                    {
                                        Sqlstrings = "select b.CustomerId,b.fcmId from Customers b with(nolock) where b.IsKPP=1 and b.fcmId is not null and b.fcmId!='' and b.Active=1 and b.Deleted=0 and and exists(select top 1 b.CustomerId from ConsumerAddresses c with(nolock)  where b.CustomerId=c.CustomerId and c.IsActive=1 and c.WarehouseId =" + notification.WarehouseID + ")"; // b.Warehouseid=" + notification.WarehouseID;
                                        consumercustomers = context.Database.SqlQuery<NotificationCustomerDTO>(Sqlstrings).ToList();
                                    }
                                    else if (notification.GroupID == -3)
                                    {
                                        Sqlstrings = "select b.CustomerId,b.fcmId from Customers b with(nolock) where  b.fcmId is not null and b.fcmId!='' and b.Active=1 and b.Deleted=0  and exists(select customerid from DistributorVerifications a with(nolock)  where a.CustomerID=b.CustomerId) and exists(select top 1 b.CustomerId from ConsumerAddresses c with(nolock)  where b.CustomerId=c.CustomerId and c.IsActive=1 and c.WarehouseId =" + notification.WarehouseID + ")";//and b.Warehouseid=" + notification.WarehouseID;
                                        consumercustomers = context.Database.SqlQuery<NotificationCustomerDTO>(Sqlstrings).ToList();
                                    }
                                    else if (notification.GroupID == -10)
                                    {
                                        //and exists(select customerid from DistributorVerifications a with(nolock)  where a.CustomerID=b.CustomerId)
                                        Sqlstrings = "select b.CustomerId,b.fcmId from CRMNotifiationCustomers a with(nolock) inner join Customers b with(nolock) on a.CustomerId=b.CustomerId where  b.fcmId is not null and b.fcmId!='' and b.Active=1 and b.Deleted=0  and a.NotificationId=" + notification.Id;
                                        consumercustomers = context.Database.SqlQuery<NotificationCustomerDTO>(Sqlstrings).ToList();
                                    }
                                    else if (notification.GroupID <= -4)
                                    {
                                        int Level = -1;
                                        switch (notification.GroupID)
                                        {
                                            case -4:
                                                Level = 0;
                                                break;
                                            case -5:
                                                Level = 1;
                                                break;
                                            case -6:
                                                Level = 2;
                                                break;
                                            case -7:
                                                Level = 3;
                                                break;
                                            case -8:
                                                Level = 4;
                                                break;
                                            case -9:
                                                Level = 5;
                                                break;
                                        }
                                        var fromdate = DateTime.Now;
                                        fromdate = DateTime.Now.AddMonths(-1);
                                        Sqlstrings = "Select distinct a.CustomerId from CRMCustomerLevels a with(nolock)  inner join Customers b  with(nolock)  on a.CustomerId=b.CustomerId and IsDeleted=0 and b.Warehouseid=" + notification.WarehouseID + " and a.Month=" + fromdate.Month + " and a.Year=" + fromdate.Year + " And a.Level=" + Level;
                                        consumercustomers = context.Database.SqlQuery<NotificationCustomerDTO>(Sqlstrings).ToList();
                                    }
                                    else
                                    {
                                        Sqlstrings = "select b.CustomerId,b.fcmId from Customers b with(nolock) where  b.fcmId is not null and b.fcmId!='' and b.Active=1 and b.Deleted=0  and exists(select top 1 b.CustomerId from ConsumerAddresses c with(nolock)  where b.CustomerId=c.CustomerId and c.IsActive=1 and c.WarehouseId =" + notification.WarehouseID + ")";                                        // and b.Warehouseid=" + notification.WarehouseID;
                                        consumercustomers = context.Database.SqlQuery<NotificationCustomerDTO>(Sqlstrings).ToList();
                                    }

                                    string Keys = ConfigurationManager.AppSettings["ConsumerFcmKey"];

                                    var objNotificationLists = consumercustomers.GroupBy(x => x.fcmId).Select(x => new FCMRequest
                                    {
                                        to = x.Key,
                                        CustId = x.FirstOrDefault().CustomerId,
                                        MessageId = "",
                                        data = new FCMData
                                        {
                                            title = notificationTitle,
                                            body = notification.Message,
                                            icon = notification.Pic,
                                            typeId = typeCode,
                                            notificationCategory = notification.NotificationCategory,
                                            notificationType = notification.NotificationType,
                                            notificationId = notification.Id,
                                            notify_type = notification.NotificationDisplayType,
                                            url = notification.NotificationMediaType == "Video" ? notification.Pic : "",
                                            IsEnabledDismissNotification = notification.IsEnabledDismissNotification
                                        }
                                    }).ToList();
                                    var data1 = new FCMData
                                    {
                                        title = notificationTitle,
                                        body = notification.Message,
                                        icon = notification.Pic,
                                        typeId = typeCode,
                                        notificationCategory = notification.NotificationCategory,
                                        notificationType = notification.NotificationType,
                                        notificationId = notification.Id,
                                        notify_type = notification.NotificationDisplayType,
                                        url = notification.NotificationMediaType == "Video" ? notification.Pic : "",
                                        IsEnabledDismissNotification = notification.IsEnabledDismissNotification
                                    };
                                    ConcurrentBag<int> totalSents = new ConcurrentBag<int>();
                                    ConcurrentBag<int> totalNotSents = new ConcurrentBag<int>();
                                    var consumernotificationSchedulers = notification.NotificationSchedulers != null ? notification.NotificationSchedulers.FirstOrDefault(y => (y.StartDate <= indianTime && y.EndDate >= indianTime) && !y.Sent) : null;
                                    var firebaseService = new FirebaseNotificationServiceHelper(Keys);

                                    List<CustomerLoginDevice> CustomerLoginDeviceList = new List<CustomerLoginDevice>();
                                    if (consumercustomers != null && consumercustomers.Any())
                                    {
                                        if (context.Database.Connection.State != ConnectionState.Open)
                                            context.Database.Connection.Open();

                                        DataTable IdDt = new DataTable();
                                        IdDt = new DataTable();
                                        IdDt.Columns.Add("IntValue");
                                        foreach (var item in consumercustomers.GroupBy(x => x.CustomerId))
                                        {
                                            var dr = IdDt.NewRow();
                                            dr["IntValue"] = item;
                                            IdDt.Rows.Add(dr);
                                        }

                                        var param1 = new SqlParameter("@CustomerIds", IdDt);
                                        
                                        var cmd = context.Database.Connection.CreateCommand();
                                        cmd.CommandText = "[dbo].[ConsumerCustomersLoginFcmids]";
                                        cmd.CommandType = System.Data.CommandType.StoredProcedure;
                                        cmd.Parameters.Add(param1);
                                        
                                        var reader = cmd.ExecuteReader();
                                        CustomerLoginDeviceList = ((IObjectContextAdapter)context)
                                        .ObjectContext
                                        .Translate<CustomerLoginDevice>(reader).ToList();
                                        reader.NextResult();
                                    }


                                    if (consumernotificationSchedulers != null || !notification.IsMultiSchedule)
                                    {
                                        ParallelLoopResult parellelResult = Parallel.ForEach(CustomerLoginDeviceList.GroupBy(x => x.FCMID), (x) =>
                                        {
                                            try
                                            {
                                                //var fcmid = "fZGIeP5dTKGd-2JBZttcDj:APA91bGINtqXqKuCcEHd4qXZMN-VtX5KC2g98KkytmGdpc28_-duDu8Ry1P6Kk_Xb9RgRG0iDWrp8DkwE1EPCOPG0OLz2uRjo-HBg-Ysg-mDaMErlYLjJGZE7ScXKIkjmWZ0xNO6UyxN";
                                                var results = firebaseService.SendNotificationForApprovalAsync(x.Key, data1);
                                                if (results != null)
                                                {
                                                    totalSents.Add(1);
                                                }
                                                else
                                                {
                                                    totalSents.Add(1);
                                                }
                                                //WebRequest tRequest = WebRequest.Create("https://fcm.googleapis.com/fcm/send") as HttpWebRequest;
                                                //tRequest.Method = "post";
                                                //string jsonNotificationFormat = Newtonsoft.Json.JsonConvert.SerializeObject(x);
                                                //Byte[] byteArray = Encoding.UTF8.GetBytes(jsonNotificationFormat);
                                                //tRequest.Headers.Add(string.Format("Authorization: key={0}", Keys));
                                                ////tRequest.Headers.Add(string.Format("Sender: id={0}", id11));
                                                //tRequest.ContentLength = byteArray.Length;
                                                //tRequest.ContentType = "application/json";
                                                //using (Stream dataStream = tRequest.GetRequestStream())
                                                //{
                                                //    dataStream.Write(byteArray, 0, byteArray.Length);
                                                //    using (WebResponse tResponse = tRequest.GetResponse())
                                                //    {
                                                //        using (Stream dataStreamResponse = tResponse.GetResponseStream())
                                                //        {
                                                //            using (StreamReader tReader = new StreamReader(dataStreamResponse))
                                                //            {
                                                //                String responseFromFirebaseServer = tReader.ReadToEnd();
                                                //                FCMResponse response = Newtonsoft.Json.JsonConvert.DeserializeObject<FCMResponse>(responseFromFirebaseServer);
                                                //                if (response.success == 1 && response.results != null && response.results.Any() && !string.IsNullOrEmpty(response.results.FirstOrDefault().message_id))
                                                //                {
                                                //                    totalSents.Add(1);
                                                //                    x.MessageId = response.results.FirstOrDefault().message_id;
                                                //                }
                                                //                else
                                                //                {
                                                //                    totalNotSents.Add(1);
                                                //                    x.MessageId = "";
                                                //                }
                                                //            }
                                                //        }
                                                //    }
                                                //}
                                            }
                                            catch (Exception asd)
                                            {
                                                logger.Error(new StringBuilder("Error while sending FCM for type: ").Append(notification.NotificationType).Append(asd.ToString()).ToString());
                                            }
                                        });

                                        if (parellelResult.IsCompleted && (totalSents.Any() || totalNotSents.Any()))
                                        {
                                            var sentnoti = true;
                                            if (notification.IsMultiSchedule)
                                            {
                                                consumernotificationSchedulers.Sent = true;
                                                consumernotificationSchedulers.TotalSent = totalSents.Count();
                                                consumernotificationSchedulers.TotalNotSent = totalNotSents.Count();
                                                sentnoti = notification.NotificationSchedulers.All(x => x.Sent);
                                                //context.Entry(notificationSchedulers).State = EntityState.Modified;
                                            }
                                            notification.TotalSent += totalSents.Count();
                                            notification.Sent = sentnoti;
                                            notification.TotalNotSent += totalNotSents.Count();
                                            context.Entry(notification).State = EntityState.Modified;

                                            if (notification.IsCRMNotification)
                                            {
                                                var IdDt = new System.Data.DataTable();
                                                IdDt = new System.Data.DataTable();
                                                IdDt.Columns.Add("IntValue");
                                                foreach (var item in consumercustomers.Select(v => v.SchedulerId).ToList())
                                                {
                                                    var dr = IdDt.NewRow();
                                                    dr["IntValue"] = item;
                                                    IdDt.Rows.Add(dr);
                                                }
                                                var param = new SqlParameter("SchedulerIds", IdDt);
                                                param.SqlDbType = System.Data.SqlDbType.Structured;
                                                param.TypeName = "dbo.IntValues";
                                                context.Database.CommandTimeout = 300;

                                                int SchedulerIdCount = context.Database.ExecuteSqlCommand("Exec UpdateCRMNotificationScheduler @SchedulerIds", param);
                                            }
                                            context.Commit();
                                            var deviceNotifications = objNotificationLists.Select(x => new DeviceNotification
                                            {
                                                CompanyId = 1,
                                                CustomerId = x.CustId,
                                                Deleted = false,
                                                DeviceId = x.to,
                                                ImageUrl = notification.Pic,
                                                Message = notification.Message,
                                                NotificationTime = indianTime,
                                                title = notification.title,
                                                WarehouseId = notification.WarehouseID,
                                                NotificationCategory = notification.NotificationCategory,
                                                NotificationType = notification.NotificationType,
                                                ObjectId = typeCode,
                                                NotificationId = notification.Id,
                                                MessageId = x.MessageId,
                                                IsView = notification.TotalViews,
                                                IsReceived = 0,
                                                NotificationDisplayType = notification.NotificationDisplayType

                                            }).ToList();

                                            var deviceNotificationOpt = new BulkOperations();
                                            deviceNotificationOpt.Setup<DeviceNotification>(x => x.ForCollection(deviceNotifications))
                                                .WithTable("DeviceNotifications")
                                                .WithBulkCopyBatchSize(4000)
                                                .WithBulkCopyCommandTimeout(720) // Default is 600 seconds
                                                .WithSqlCommandTimeout(720) // Default is 600 seconds
                                                .AddAllColumns()
                                                .BulkInsert();
                                            deviceNotificationOpt.CommitTransaction("AuthContext");

                                            //context.DeviceNotificationDb.AddRange(deviceNotifications);
                                            //context.Commit();
                                        }
                                    }
                                    break;
                            }
                        }
                    }
                    return true;

                }
                catch (Exception ex)
                {
                    logger.Error("Error in Add message " + ex.Message);
                    return false;
                }
            }
        }

        [HttpGet]
        [Route("NotificationActiveCategory")]
        public List<NotificationSectionItem> NotificationActiveCategory()
        {
            using (var context = new AuthContext())
            {
                List<NotificationSectionItem> notificationSectionItems = new List<NotificationSectionItem>();
                notificationSectionItems = context.itemMasters.Where(x => x.Deleted == false && x.active == true).Select(x => new NotificationSectionItem { Id = x.Categoryid, Name = x.CategoryName }).Distinct().ToList();
                return notificationSectionItems;

            }
        }

        [HttpGet]
        [Route("NotificationActiveSubCategory")]
        public List<NotificationSectionItem> NotificationActiveSubCategory()
        {
            using (var context = new AuthContext())
            {
                List<NotificationSectionItem> notificationSectionItems = new List<NotificationSectionItem>();
                notificationSectionItems = context.itemMasters.Where(x => x.Deleted == false && x.active == true).Select(x => new NotificationSectionItem { Id = x.SubCategoryId, Name = x.SubcategoryName }).Distinct().ToList();
                return notificationSectionItems;
            }
        }


        [HttpGet]
        [Route("NotificationActiveBrand")]
        public List<NotificationSectionItem> NotificationActiveBrand()
        {
            using (var context = new AuthContext())
            {
                List<NotificationSectionItem> notificationSectionItems = new List<NotificationSectionItem>();
                notificationSectionItems = context.itemMasters.Where(x => x.Deleted == false && x.active == true).Select(x => new NotificationSectionItem { Id = x.SubsubCategoryid, Name = x.SubsubcategoryName }).Distinct().ToList();
                return notificationSectionItems;
            }
        }

        [HttpPost]
        [Route("ExportToExcel")]
        public List<ExportNotificaton> ExportToExcel(ExportFilter export)
        {
            using (var context = new AuthContext())
            {
                List<ExportNotificaton> notificationSectionItems = new List<ExportNotificaton>();
                var startParam = new SqlParameter
                {
                    ParameterName = "startDate"
                };
                var EndParam = new SqlParameter
                {
                    ParameterName = "@endDate"
                };
                var warehouseParam = new SqlParameter
                {
                    ParameterName = "@warehouseId",
                    Value = export.warehouseId
                };

                if (export.start.HasValue && export.end.HasValue)
                {

                    EndParam.Value = export.end.Value;
                    startParam.Value = export.start.Value;
                    //notificationSectionItems = context.NotificationUpdatedDb.Where(x => x.WarehouseID == export.warehouseId && x.CreatedTime >= export.start && x.CreatedTime <= export.end && x.Deleted == false).ToList();
                }
                else
                {
                    EndParam.Value = "";
                    startParam.Value = "";
                    //notificationSectionItems = context.NotificationUpdatedDb.Where(x => x.WarehouseID == export.warehouseId && x.Deleted == false).ToList();
                }
                notificationSectionItems = context.Database.SqlQuery<ExportNotificaton>("exec GetNotificationForExport @warehouseId,@startDate,@endDate", warehouseParam, startParam, EndParam).ToList();

                return notificationSectionItems;
            }
        }

        [Route("GetSellerCity")]
        [HttpGet]
        public async Task<dynamic> GetSellerCity()
        {
            string url = "https://directb2bapi.shopkirana.in//";

            using (var httpClient = new HttpClient())
            {
                using (var request = new HttpRequestMessage(new HttpMethod("Get"), url + "api/admin/panel/B2bConfiguration/GetSellerCity"))
                {
                    request.Headers.TryAddWithoutValidation("Accept", "*/*");
                    request.Headers.TryAddWithoutValidation("noencryption", "1");

                    var response = await httpClient.SendAsync(request);
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        string jsonString = (await response.Content.ReadAsStringAsync());
                        TextFileLogHelper.TraceLog("jsonString  :" + jsonString);
                        var resp = JsonConvert.DeserializeObject<dynamic>(jsonString);


                        if (resp != null)
                        {
                            TextFileLogHelper.TraceLog("Response  after post response");
                            var res = resp.Data.ResultList;
                            return res;
                        }
                        else
                        {
                            TextFileLogHelper.TraceLog("response :" + resp);

                        }
                    }
                    else
                    {
                        TextFileLogHelper.TraceLog("eMandate eMandateResponse response  :" + response);
                    }
                }
            }
            return true;
        }


    }


    public class ExportFilter
    {
        public int warehouseId { get; set; }
        public DateTime? start { get; set; }
        public DateTime? end { get; set; }
    }

    public class FCMResponseUpdated
    {
        public long multicast_id { get; set; }
        public int success { get; set; }
        public int failure { get; set; }
        public int canonical_ids { get; set; }
        public List<FCMResultUpdated> results { get; set; }
    }
    public class FCMResultUpdated
    {
        public string message_id { get; set; }
    }
    //public class customers
    //{
    //    public string fcmId { get; set; }
    //    public int CustomerId { get; set; }
    //    public int orderCount { get; set; }
    //    public double orderTotal { get; set; }
    //}

    public class notModelUpdated
    {
        public int ItemId { get; set; }
        public int CustomerId { get; set; }
        public int categoryId { get; set; }
        public string fcmId { get; set; }
        public int orderCount { get; set; }
        public double orderTotal { get; set; }
    }

    public class NotificationSectionItem
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }


    public class FCMRequest
    {
        public string to { get; set; }
        public int CustId { get; set; }
        public string MessageId { get; set; }
        public FCMData data { get; set; }
    }
    public class FCMData
    {
        public string title { get; set; }
        public string body { get; set; }
        public string icon { get; set; }
        public int typeId { get; set; }
        public int OrderId { get; set; }
        public int AssignmentID { get; set; }
        public string notificationCategory { get; set; }
        public string notificationType { get; set; }
        public int notificationId { get; set; }
        public string notify_type { get; set; }
        public string url { get; set; }
        public string OrderStatus { get; set; }
        public int OrderApprovestatus { get; set; }
        public bool IsApproved { get; set; }
        public string UPITxnID { get; set; }
        public bool? IsEnabledDismissNotification { get; set; }
        public string image_url { get; set; }
        public string priority { get; set; }



    }

    public class SellerCityListDC
    {
        public int value { get; set; }
        public string label { get; set; }
    }
    public class FcmidDC
    {
        public string to { get; set; }
        public string MessageId { get; set; }
        public FcmidDCss data { get; set; }

    }
    public class FcmidDCss
    {
        public string body { get; set; }
        public string title { get; set; }
        public string notify_type { get; set; }
        public string image_url { get; set; }
        public bool? IsEnabledDismissNotification { get; set; }
    }


}




