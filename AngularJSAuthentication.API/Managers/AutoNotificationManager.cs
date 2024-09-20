using AngularJSAuthentication.API.Helpers;
using AngularJSAuthentication.DataContracts.Mongo;
using AngularJSAuthentication.Model.CustomerShoppingCart;
using LinqKit;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using NLog;
using static AngularJSAuthentication.API.Controllers.NotificationController;
using System.Data;
using System.Data.SqlClient;
using System.Data.Entity.Infrastructure;
using AngularJSAuthentication.DataContracts.ElasticSearch;
using AngularJSAuthentication.API.Helper;
using AngularJSAuthentication.API.Controllers;
using AngularJSAuthentication.API.Helper.Notification;

namespace AngularJSAuthentication.API.Managers
{
    public class AutoNotificationManager
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        public bool ShoppingCartNotification()
        {
            bool result = false;
            using (var context = new AuthContext())
            {
                MongoDbHelper<CustomerShoppingCart> mongoDbHelper = new MongoDbHelper<CustomerShoppingCart>();
                //var StartDate = DateTime.Now.AddDays(-1).Date;
                //var EndDate = new DateTime( DateTime.Now.AddDays(-1).Date.Year, DateTime.Now.AddDays(-1).Date.Month, DateTime.Now.AddDays(-1).Date.Day,23,59,59);
                //var cartPredicate = PredicateBuilder.New<CustomerShoppingCart>(x => !x.GeneratedOrderId.HasValue && x.ModifiedDate.Value < EndDate && x.ModifiedDate.Value > StartDate && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value));
                var cartPredicate = PredicateBuilder.New<CustomerShoppingCart>(x => !x.GeneratedOrderId.HasValue && x.Mobile == "9977044088" && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value));
                var customerShoppingCarts = mongoDbHelper.Select(cartPredicate).ToList();
                if (customerShoppingCarts != null && customerShoppingCarts.Any())
                {
                    var customerids = customerShoppingCarts.Select(x => x.CustomerId).Distinct().ToList();
                    var customers = context.Customers.Where(x => customerids.Contains(x.CustomerId) && !x.Deleted && x.Active && !string.IsNullOrEmpty(x.fcmId))
                          .Select(x => new { x.fcmId, x.CustomerId, x.Skcode, x.ShopName, x.Name }).ToList();

                    MongoDbHelper<ManualAutoNotification> AutoNotificationmongoDbHelper = new MongoDbHelper<ManualAutoNotification>();
                    MongoDbHelper<DefaultNotificationMessage> DefalutmongoDbHelper = new MongoDbHelper<DefaultNotificationMessage>();
                    string message = DefalutmongoDbHelper.Select(x => x.NotificationMsgType == "ShopingCart").FirstOrDefault()?.NotificationMsg;//   "Hi [CustomerName]. You Have Left Something in Your Cart, complete your Purchase with ShopKirana on Sigle Click.";
                    if (!string.IsNullOrEmpty(message))
                    {
                        //var objNotificationList = customers.Select(x => new
                        //{
                        //    to = x.fcmId,
                        //    CustId = x.CustomerId,
                        //    data = new
                        //    {
                        //        title = "ChectOut With Shopkirana",
                        //        body = !string.IsNullOrEmpty(x.Name) ? message.Replace("[CustomerName]", x.Name) : message.Replace("[CustomerName]", x.ShopName),
                        //        icon = "",
                        //        typeId = "",
                        //        notificationCategory = "",
                        //        notificationType = "Actionable",
                        //        notificationId = 1,
                        //        notify_type = "ShoppingCart",
                        //        url = "",
                        //    }
                        //}).ToList();
                        List<ManualAutoNotification> AutoNotifications = new List<ManualAutoNotification>();

                        var Key = ConfigurationManager.AppSettings["FcmApiKey"];
                        var firebaseService = new FirebaseNotificationServiceHelper(Key);
                        ParallelLoopResult parellelResult = Parallel.ForEach(customers, async (x) =>
                        {

                            var data = new FCMData
                            {
                                title = "ChectOut With Shopkirana",
                                body = !string.IsNullOrEmpty(x.Name) ? message.Replace("[CustomerName]", x.Name) : message.Replace("[CustomerName]", x.ShopName),
                                icon = "",
                                notificationCategory = "",
                                notificationType = "Actionable",
                                notificationId = 1,
                                notify_type = "ShoppingCart",
                                url = "",
                            };
                            var AutoNotification = new ManualAutoNotification
                            {
                                CreatedDate = DateTime.Now,
                                FcmKey = Key,
                                IsActive = true,
                                IsSent = false,
                                NotificationMsg = Newtonsoft.Json.JsonConvert.SerializeObject(data),
                                ObjectId = x.CustomerId,
                                ObjectType = "Customer"
                            };
                            try
                            {
                                //WebRequest tRequest = WebRequest.Create("https://fcm.googleapis.com/fcm/send") as HttpWebRequest;
                                //tRequest.Method = "post";
                                //string jsonNotificationFormat = Newtonsoft.Json.JsonConvert.SerializeObject(item);
                                //Byte[] byteArray = Encoding.UTF8.GetBytes(jsonNotificationFormat);
                                //tRequest.Headers.Add(string.Format("Authorization: key={0}", AutoNotification.FcmKey));

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
                                //                    AutoNotification.IsSent = true;
                                //                }
                                //                else if (response.failure == 1)
                                //                {
                                //                    AutoNotification.IsSent = false;
                                //                }
                                //            }
                                //        }
                                //    }
                                //}

                                var results = await firebaseService.SendNotificationForApprovalAsync(x.fcmId, data);
                                if (results != null)
                                {
                                    AutoNotification.IsSent = true;
                                }
                                else
                                {
                                    AutoNotification.IsSent = false;
                                }
                            }
                            catch (Exception asd)
                            {
                                logger.Error("Error during sent ShoppingCart auto notification : " + asd.ToString());
                            }

                            AutoNotifications.Add(AutoNotification);
                        });

                        if (parellelResult.IsCompleted)
                        {
                            if (AutoNotifications.Any())
                                AutoNotificationmongoDbHelper.InsertMany(AutoNotifications);
                            result = true;
                        }
                    }
                }
                else
                    result = true;

            }
            return result;
        }

        public bool CustomerTargetNotification()
        {
            using (var context = new AuthContext())
            {
                var date = DateTime.Now;
                if (date.Day < 6)
                {
                    date = DateTime.Now.AddMonths(-1);
                }
                var lastDayOfMonth = DateTime.DaysInMonth(date.Year, date.Month);

                var MonthlyCustomerTarget = new MongoDbHelper<CustomersTargets.MonthlyCustomerTarget>();
                string DocumentName = "MonthlyTargetData_" + date.Month.ToString() + date.Year.ToString();
                var customerTargets = MonthlyCustomerTarget.Select(x => !x.IsClaimed && x.Target > 0, null, null, null, false, "", DocumentName).ToList();
                customerTargets = customerTargets.Where(x => ((x.CurrentVolume / x.Target) * 100) > 70).ToList();
                if (customerTargets != null && customerTargets.Any())
                {
                    var customerstarget = customerTargets.Select(x => x.Skcode).Distinct().ToList();
                    var skcodeList = new DataTable();
                    skcodeList.Columns.Add("stringValues");
                    if (customerstarget != null && customerstarget.Any())
                    {
                        foreach (var item in customerstarget)
                        {
                            var dr = skcodeList.NewRow();
                            dr["stringValues"] = item;
                            skcodeList.Rows.Add(dr);
                        }
                    }
                    var Skcodeparam = new SqlParameter("Skcode", skcodeList);
                    Skcodeparam.SqlDbType = SqlDbType.Structured;
                    Skcodeparam.TypeName = "dbo.stringValues";

                    if (context.Database.Connection.State != ConnectionState.Open)
                        context.Database.Connection.Open();

                    context.Database.CommandTimeout = 400;
                    var cmd = context.Database.Connection.CreateCommand();
                    cmd.CommandText = "[dbo].GetCustomerDataBySkcode";
                    cmd.Parameters.Add(Skcodeparam);
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;

                    // Run the sproc
                    var reader = cmd.ExecuteReader();
                    List<GetCustomerTargetDC> Allcustomers = ((IObjectContextAdapter)context)
                    .ObjectContext
                    .Translate<GetCustomerTargetDC>(reader).ToList();

                    var customers = Allcustomers.Where(x => !x.Deleted && x.Active && !string.IsNullOrEmpty(x.fcmId)).ToList();
                    var skCodes70 = customerTargets.Where(x => ((x.CurrentVolume / x.Target) * 100) < 100).Select(x => x.Skcode).Distinct().ToList();
                    var customers70 = customers.Where(x => skCodes70.Contains(x.Skcode)).ToList();
                    MongoDbHelper<ManualAutoNotification> AutoNotificationmongoDbHelper = new MongoDbHelper<ManualAutoNotification>();
                    MongoDbHelper<DefaultNotificationMessage> DefalutmongoDbHelper = new MongoDbHelper<DefaultNotificationMessage>();
                    string message = DefalutmongoDbHelper.Select(x => x.NotificationMsgType == "Claim70").FirstOrDefault()?.NotificationMsg;
                    string message100 = DefalutmongoDbHelper.Select(x => x.NotificationMsgType == "Claim100").FirstOrDefault()?.NotificationMsg;

                    if (!string.IsNullOrEmpty(message) && !string.IsNullOrEmpty(message100))
                    {
                        List<ManualAutoNotification> AutoNotifications = new List<ManualAutoNotification>();

                        var Key = ConfigurationManager.AppSettings["FcmApiKey"];
                        var firebaseService = new FirebaseNotificationServiceHelper(Key);
                        ParallelLoopResult parellelResult = Parallel.ForEach(customers,async (x) =>
                        {
                            //var notification = new
                            //{
                            //    to = x.fcmId,
                            //    CustId = x.CustomerId,
                            //    data = new
                            //    {
                            //        title = "Monthly Target",
                            //        icon = "",
                            //        typeId = "",
                            //        notificationCategory = "",
                            //        notificationType = "Actionable",
                            //        notificationId = 1,
                            //        notify_type = "Claim",
                            //        url = "",
                            //        not = ((customerTargets.FirstOrDefault(y => y.Skcode == x.Skcode).CurrentVolume / customerTargets.FirstOrDefault(y => y.Skcode == x.Skcode).Target) * 100) >= 100
                            //        ? message100.Replace("[CustomerName]", (!string.IsNullOrEmpty(x.Name) ? x.Name : x.ShopName))
                            //        : message.Replace("[CustomerName]", (!string.IsNullOrEmpty(x.Name) ? x.Name : x.ShopName))
                            //            .Replace("[Parcent]", ((customerTargets.FirstOrDefault(y => y.Skcode == x.Skcode).CurrentVolume / customerTargets.FirstOrDefault(y => y.Skcode == x.Skcode).Target) * 100).ToString())
                            //    }
                            //};
                            var data = new FCMData
                            {
                                title = "Monthly Target",
                                icon = "",
                                notificationCategory = "",
                                notificationType = "Actionable",
                                notificationId = 1,
                                notify_type = "Claim",
                                url = "",
                                //not = ((customerTargets.FirstOrDefault(y => y.Skcode == x.Skcode).CurrentVolume / customerTargets.FirstOrDefault(y => y.Skcode == x.Skcode).Target) * 100) >= 100
                                //        ? message100.Replace("[CustomerName]", (!string.IsNullOrEmpty(x.Name) ? x.Name : x.ShopName))
                                //        : message.Replace("[CustomerName]", (!string.IsNullOrEmpty(x.Name) ? x.Name : x.ShopName))
                                //            .Replace("[Parcent]", ((customerTargets.FirstOrDefault(y => y.Skcode == x.Skcode).CurrentVolume / customerTargets.FirstOrDefault(y => y.Skcode == x.Skcode).Target) * 100).ToString())
                                ////    
                            };



                            var AutoNotification = new ManualAutoNotification
                            {
                                CreatedDate = DateTime.Now,
                                FcmKey = Key,
                                IsActive = true,
                                IsSent = false,
                                NotificationMsg = Newtonsoft.Json.JsonConvert.SerializeObject(data),
                                ObjectId = x.CustomerId,
                                ObjectType = "Customer"
                            };
                            try
                            {

                                //WebRequest tRequest = WebRequest.Create("https://fcm.googleapis.com/fcm/send") as HttpWebRequest;
                                //tRequest.Method = "post";
                                //string jsonNotificationFormat = Newtonsoft.Json.JsonConvert.SerializeObject(notification);
                                //Byte[] byteArray = Encoding.UTF8.GetBytes(jsonNotificationFormat);
                                //tRequest.Headers.Add(string.Format("Authorization: key={0}", AutoNotification.FcmKey));
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
                                //                    AutoNotification.IsSent = true;
                                //                }
                                //                else if (response.failure == 1)
                                //                {
                                //                    AutoNotification.IsSent = false;
                                //                }
                                //            }
                                //        }
                                //    }
                                //}
                                var results = await firebaseService.SendNotificationForApprovalAsync(x.fcmId, data);
                                if (results != null)
                                {
                                    AutoNotification.IsSent = true;
                                }
                                else
                                {
                                    AutoNotification.IsSent = false;
                                }
                            }
                            catch (Exception asd)
                            {
                                logger.Error("Error during sent claim auto notification : " + asd.ToString());
                            }
                            AutoNotifications.Add(AutoNotification);
                        });
                        if (parellelResult.IsCompleted)
                        {
                            if (AutoNotifications.Any())
                                AutoNotificationmongoDbHelper.InsertMany(AutoNotifications);

                        }
                    }

                }
                return true;
            }



        }

        public bool TopMarginItemNotification()
        {
            bool result = true;
            using (var context = new AuthContext())
            {
                var warehouseids = context.Warehouses.Where(x => x.active && !x.Deleted && (x.IsKPP == false || x.IsKppShowAsWH == true)).Select(x => x.WarehouseId).ToList();
                if (warehouseids != null && warehouseids.Any())
                {
                    foreach (var warehouseid in warehouseids)
                    {
                        List<DataContracts.External.ItemDataDC> ItemDataDCs = new List<DataContracts.External.ItemDataDC>();
                        if (context.Database.Connection.State != ConnectionState.Open)
                            context.Database.Connection.Open();


                        var cmd = context.Database.Connection.CreateCommand();
                        cmd.CommandTimeout = 600;
                        cmd.CommandText = "[dbo].[GetRendomTopMarginItem]";
                        cmd.Parameters.Add(new SqlParameter("@warehouseId", warehouseid));
                        cmd.CommandType = System.Data.CommandType.StoredProcedure;

                        // Run the sproc
                        var reader = cmd.ExecuteReader();
                        var ItemData = ((IObjectContextAdapter)context)
                        .ObjectContext
                        .Translate<DataContracts.External.ItemDataDC>(reader).ToList();

                        var customers = context.Customers.Where(x => x.Warehouseid == warehouseid && !x.Deleted && x.Active && !string.IsNullOrEmpty(x.fcmId)).GroupBy(x => x.fcmId)
                                  .Select(x => new { fcmId = x.Key, CustomerId = x.FirstOrDefault().CustomerId, Skcode = x.FirstOrDefault().Skcode, ShopName = x.FirstOrDefault().ShopName, Name = x.FirstOrDefault().Name }).ToList();

                        if (customers != null && customers.Any() && ItemData != null && ItemData.Any())
                        {
                            string message = "[CustomerName] जी,\n हम लाए है खास आपके लिए आज के स्पेशल प्रोडक्ट्स \n";
                            int i = 1;
                            foreach (var item in ItemData)
                            {
                                string struom = "";
                                if (item.UOM == "Gm")
                                    struom = "gram";
                                else if (item.UOM == "Kg")
                                    struom = "kilogram";
                                else if (item.UOM == "Combo")
                                    struom = "combo";
                                else if (item.UOM == "Ltr")
                                    struom = "liter";
                                else if (item.UOM == "Ml")
                                    struom = "mili liter";
                                else if (item.UOM == "Pc")
                                    struom = "pieces";
                                else if (item.UOM == "Size")
                                    struom = "size";

                                //var price = item.UnitPrice.ToString().Split('.');

                                message += i.ToString() + ". " + item.itemBaseName + " " + item.MRP + " MRP " + (!string.IsNullOrEmpty(struom) ? (item.UnitofQuantity + " " + struom) : "") + " @ " + item.UnitPrice.ToString("#.##") + "/-";

                                //if (price.Length == 2 && Convert.ToInt32(price[1]) > 0)
                                //    message += "." +  price[1] ;

                                message += "\n";
                                i++;
                            }
                            message += " तो जल्दी कीजिये यह रेटस केवल सिमित अवधि के लिए ही उपलब्ध है";
                            ConcurrentBag<ManualAutoNotification> AutoNotifications = new ConcurrentBag<ManualAutoNotification>();
                            //List<ManualAutoNotification> AutoNotifications = new List<ManualAutoNotification>();
                            MongoDbHelper<ManualAutoNotification> AutoNotificationmongoDbHelper = new MongoDbHelper<ManualAutoNotification>();

                            var Key = ConfigurationManager.AppSettings["FcmApiKey"];
                            var firebaseService = new FirebaseNotificationServiceHelper(Key);

                            ParallelLoopResult parellelResult = Parallel.ForEach(customers, async (item) =>
                            {
                                //dynamic notification = new
                                //{
                                //    to = item.fcmId,
                                //    CustId = item.CustomerId,
                                //    data = new
                                //    {
                                //        title = "आज के टॉप स्पेशल प्रोडक्ट्स",
                                //        body = message.Replace("[CustomerName]", string.IsNullOrEmpty(item.Name) ? item.ShopName : item.Name),
                                //        icon = "",
                                //        typeId = 0,
                                //        notificationCategory = "home",
                                //        notificationType = "Actionable",
                                //        notificationId = 1,
                                //        notify_type = "home",
                                //        url = "",
                                //    }
                                //};
                                var data = new FCMData
                                {
                                    title = "आज के टॉप स्पेशल प्रोडक्ट्स",
                                    body = message.Replace("[CustomerName]", string.IsNullOrEmpty(item.Name) ? item.ShopName : item.Name),
                                    icon = "",
                                    typeId = 0,
                                    notificationCategory = "home",
                                    notificationType = "Actionable",
                                    notificationId = 1,
                                    notify_type = "home",
                                    url = "",
                                };

                                var AutoNotification = new ManualAutoNotification
                                {
                                    CreatedDate = DateTime.Now,
                                    FcmKey = Key,
                                    IsActive = true,
                                    IsSent = false,
                                    NotificationMsg = Newtonsoft.Json.JsonConvert.SerializeObject(data),
                                    ObjectId = item.CustomerId,
                                    ObjectType = "Customer"
                                };
                                try
                                {

                                    //WebRequest tRequest = WebRequest.Create("https://fcm.googleapis.com/fcm/send") as HttpWebRequest;
                                    //tRequest.Method = "post";
                                    //string jsonNotificationFormat = Newtonsoft.Json.JsonConvert.SerializeObject(notification);
                                    //Byte[] byteArray = Encoding.UTF8.GetBytes(jsonNotificationFormat);
                                    //tRequest.Headers.Add(string.Format("Authorization: key={0}", AutoNotification.FcmKey));
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
                                    //                    AutoNotification.IsSent = true;
                                    //                }
                                    //                else if (response.failure == 1)
                                    //                {
                                    //                    AutoNotification.IsSent = false;
                                    //                }
                                    //            }
                                    //        }
                                    //    }
                                    //}
                                    var results = await firebaseService.SendNotificationForApprovalAsync(item.fcmId, data);
                                    if (results != null)
                                    {
                                        AutoNotification.IsSent = true;
                                    }
                                    else
                                    {
                                        AutoNotification.IsSent = false;
                                    }
                                }
                                catch (Exception asd)
                                {
                                    logger.Error("Error during sent claim auto notification : " + asd.ToString());
                                    AutoNotification.IsSent = false;
                                }
                                AutoNotifications.Add(AutoNotification);
                            });
                            if (parellelResult.IsCompleted && AutoNotifications != null && AutoNotifications.Any())
                            {
                                var autoNot = AutoNotifications.ToList();
                                AutoNotificationmongoDbHelper.InsertMany(autoNot);

                            }
                        }
                    }

                }
            }

            return result;
        }

        public bool GamePointExpireNotification()
        {
            MongoDbHelper<Controllers.CustomerPlayedGame> mongoDbHelper = new MongoDbHelper<Controllers.CustomerPlayedGame>();
            DateTime startDate = DateTime.Now.Date;
            DateTime endDate = startDate.AddDays(1).AddMinutes(-1);
            var CustomerPlayedGames = mongoDbHelper.Select(x => !x.IsExpired && x.Point > 0).ToList();
            if (CustomerPlayedGames != null && CustomerPlayedGames.Any())
            {
                var customerIds = CustomerPlayedGames.Where(x => (x.CreatedDate - startDate).Hours >= 6 && (x.CreatedDate - startDate).Hours < 8).GroupBy(x => x.CustomerId).Select(x => x.Key).ToList();
                if (customerIds != null && customerIds.Any())
                {
                    using (var context = new AuthContext())
                    {
                        context.Database.CommandTimeout = 2000;
                        var customers = context.Customers.Where(x => customerIds.Contains(x.CustomerId)).Select(x => new { x.CustomerId, x.fcmId, x.Mobile, x.ShopName, x.Name }).ToList();
                        MongoDbHelper<ManualAutoNotification> AutoNotificationmongoDbHelper = new MongoDbHelper<ManualAutoNotification>();
                        MongoDbHelper<DefaultNotificationMessage> DefalutmongoDbHelper = new MongoDbHelper<DefaultNotificationMessage>();
                        string message = DefalutmongoDbHelper.Select(x => x.NotificationMsgType == "GamePointExpired").FirstOrDefault()?.NotificationMsg;//   "Hi [CustomerName]. You Have Left Something in Your Cart, complete your Purchase with ShopKirana on Sigle Click.";
                        if (!string.IsNullOrEmpty(message))
                        {
                            #region send Notification
                            //var objNotificationList = customers.Where(x => !string.IsNullOrEmpty(x.fcmId)).Select(x => new
                            //{
                            //    to = x.fcmId,
                            //    CustId = x.CustomerId,
                            //    data = new
                            //    {
                            //        title = "Hurry Up Your Game Point Expired",
                            //        body = message.Replace("[CustomerName]", (string.IsNullOrEmpty(x.ShopName) ? x.Name : x.ShopName)),
                            //        icon = "",
                            //        typeId = "",
                            //        notificationCategory = "",
                            //        notificationType = "Actionable",
                            //        notificationId = 1,
                            //        notify_type = "ShoppingCart",
                            //        url = "",
                            //    }
                            //}).ToList();
                            var Key = ConfigurationManager.AppSettings["FcmApiKey"];
                            
                            var firebaseService = new FirebaseNotificationServiceHelper(Key);

                            List<ManualAutoNotification> AutoNotifications = new List<ManualAutoNotification>();
                            ParallelLoopResult parellelResult = Parallel.ForEach(customers, async (x) =>
                            {
                                var data = new FCMData
                                {
                                    title = "Hurry Up Your Game Point Expired",
                                    body = message.Replace("[CustomerName]", (string.IsNullOrEmpty(x.ShopName) ? x.Name : x.ShopName)),
                                    icon = "",
                                    notificationCategory = "",
                                    notificationType = "Actionable",
                                    notificationId = 1,
                                    notify_type = "ShoppingCart",
                                    url = "",
                                };
                                var AutoNotification = new ManualAutoNotification
                                {
                                    CreatedDate = DateTime.Now,
                                    FcmKey = Key,
                                    IsActive = true,
                                    IsSent = false,
                                    NotificationMsg = Newtonsoft.Json.JsonConvert.SerializeObject(data),
                                    ObjectId = x.CustomerId,
                                    ObjectType = "Customer"
                                };
                                try
                                {
                                    //WebRequest tRequest = WebRequest.Create("https://fcm.googleapis.com/fcm/send") as HttpWebRequest;
                                    //tRequest.Method = "post";
                                    //string jsonNotificationFormat = Newtonsoft.Json.JsonConvert.SerializeObject(item);
                                    //Byte[] byteArray = Encoding.UTF8.GetBytes(jsonNotificationFormat);
                                    //tRequest.Headers.Add(string.Format("Authorization: key={0}", AutoNotification.FcmKey));
                                    //if (byteArray != null && byteArray.Length > 0)
                                    //{
                                    //    tRequest.ContentLength = byteArray.Length;
                                    //    tRequest.ContentType = "application/json";
                                    //    using (Stream dataStream = tRequest.GetRequestStream())
                                    //    {
                                    //        dataStream.Write(byteArray, 0, byteArray.Length);
                                    //        using (WebResponse tResponse = tRequest.GetResponse())
                                    //        {
                                    //            using (Stream dataStreamResponse = tResponse.GetResponseStream())
                                    //            {
                                    //                using (StreamReader tReader = new StreamReader(dataStreamResponse))
                                    //                {
                                    //                    String responseFromFirebaseServer = tReader.ReadToEnd();
                                    //                    if (!string.IsNullOrEmpty(responseFromFirebaseServer))
                                    //                    {
                                    //                        FCMResponse response = Newtonsoft.Json.JsonConvert.DeserializeObject<FCMResponse>(responseFromFirebaseServer);
                                    //                        if (response.success == 1)
                                    //                        {
                                    //                            AutoNotification.IsSent = true;
                                    //                        }
                                    //                        else if (response.failure == 1)
                                    //                        {
                                    //                            AutoNotification.IsSent = false;
                                    //                        }
                                    //                    }
                                    //                    else
                                    //                        AutoNotification.IsSent = false;
                                    //                }
                                    //            }
                                    //        }
                                    //    }
                                    //}
                                    //else
                                    //    AutoNotification.IsSent = false;

                                    var result = await firebaseService.SendNotificationForApprovalAsync(x.fcmId, data);
                                    if (result != null)
                                    {
                                        AutoNotification.IsSent = true;
                                    }
                                    else
                                    {
                                        AutoNotification.IsSent = false;
                                    }
                                }
                                catch (Exception asd)
                                {
                                    logger.Error("Error during sent ShoppingCart auto notification : " + asd.ToString());
                                }

                                AutoNotifications.Add(AutoNotification);
                            });

                            if (parellelResult.IsCompleted)
                            {
                                if (AutoNotifications.Any())
                                    AutoNotificationmongoDbHelper.InsertMany(AutoNotifications);

                            }

                            #endregion
                            //var smsCustomers = customers.Where(x => !string.IsNullOrEmpty(x.Mobile)).Select(x => new { x.Mobile, x.ShopName }).ToList();
                            //ParallelLoopResult parellelResult1 = Parallel.ForEach(smsCustomers, (item) =>
                            //{
                            //    string smsmessage = message;
                            //    smsmessage = smsmessage.Replace("[CustomerName]", item.ShopName);
                            //    Common.Helpers.SendSMSHelper.SendSMS(item.Mobile, smsmessage, ((Int32)Common.Enums.SMSRouteEnum.Transactional).ToString());

                            //});
                        }
                    }
                }
            }

            return true;
        }


        public bool SentNotifyItemNotification()
        {
            MongoDbHelper<CustomerItemNotifyMe> mongoDbHelper = new MongoDbHelper<CustomerItemNotifyMe>();
            var customerItemNotifyMe = mongoDbHelper.Select(x => !x.IsSentNotify && x.Customers.Any(y => !y.IsNotify)).ToList();
            MongoDbHelper<ElasticSearchQuery> mongoDbHelperElastic = new MongoDbHelper<ElasticSearchQuery>();
            MongoDbHelper<ManualAutoNotification> AutoNotificationmongoDbHelper = new MongoDbHelper<ManualAutoNotification>();

            if (customerItemNotifyMe != null && customerItemNotifyMe.Any())
            {
                var itemnumbers = customerItemNotifyMe.Select(x => x.ItemNumber).Distinct().ToList();
                var warehouseIds = customerItemNotifyMe.Select(x => x.WarehouseId).Distinct().ToList();

                Suggest suggest = null;

                /*
                var searchPredicate = PredicateBuilder.New<ElasticSearchQuery>(x => x.ObjectType == "ItemMaster" && x.QueryType == "ItemNotify");
                var searchQuery = mongoDbHelperElastic.Select(searchPredicate, null, null, null, collectionName: "ElasticSearchQuery").FirstOrDefault();
                var query = searchQuery.Query.Replace(@"\r", "").Replace(@"\n", "")
                   .Replace("{#warehouseids#}", string.Join(",", warehouseIds))
                   .Replace("{#itemnumbers#}", string.Join("+", itemnumbers))
                   .Replace("{#itemactive#}", @"{""term"": {""active"":true}},")
                   .Replace("{#from#}", "0")
                   .Replace("{#size#}", "5000");
                List<ElasticSearchItem> elasticSearchItems = ElasticSearchHelper.GetItemByElasticSearchQuery(query, out suggest);
                */
                List<ElasticSearchItem> elasticSearchItems = null;
                if (elasticSearchItems != null && elasticSearchItems.Any())
                {
                    itemnumbers = elasticSearchItems.Select(x => x.itemnumber).ToList();
                    warehouseIds = elasticSearchItems.Select(x => x.warehouseid).ToList();
                    var customers = customerItemNotifyMe.Where(x => itemnumbers.Contains(x.ItemNumber) && warehouseIds.Contains(x.WarehouseId))
                         .SelectMany(x => x.Customers.Select(z => new { z.CustomerId, z.fcmId, x.ItemNumber }))
                         .GroupBy(x => new { x.CustomerId, x.fcmId })
                         .Select(x => new { x.Key.CustomerId, x.Key.fcmId, Items = x.Select(z => new { z.ItemNumber, ItemName = elasticSearchItems.FirstOrDefault(y => y.itemnumber == z.ItemNumber).itemname }) }).ToList();

                    //var objNotificationList = customers.Where(x => !string.IsNullOrEmpty(x.fcmId)).Select(x => new
                    //{
                    //    to = x.fcmId,
                    //    CustId = x.CustomerId,
                    //    data = new
                    //    {
                    //        title = "आइटम उपलब्ध",
                    //        body = "Hurry Up शॉपकिराना एप्लीकेशन पर आपका आइटम उपलब्ध है \n" + string.Join("\n", x.Items.Select(y => y.ItemName).ToList()),
                    //        icon = "",
                    //        typeId = "",
                    //        notificationCategory = "home",
                    //        notificationType = "Actionable",
                    //        notificationId = 1,
                    //        notify_type = "home",
                    //        url = "",
                    //    }
                    //}).ToList();
                    
                    var Key = ConfigurationManager.AppSettings["FcmApiKey"];
                    var FcmIds = customers.Select(x => new { x.fcmId, x.CustomerId ,x.Items}).ToList();
                    var firebaseService = new FirebaseNotificationServiceHelper(Key);
                    List<ManualAutoNotification> AutoNotifications = new List<ManualAutoNotification>();
                    ParallelLoopResult parellelResult = Parallel.ForEach(FcmIds, async (item) =>
                    {
                        var data = new FCMData
                        {
                            title = "आइटम उपलब्ध",
                            body = "Hurry Up शॉपकिराना एप्लीकेशन पर आपका आइटम उपलब्ध है \n" + string.Join("\n", item.Items.Select(y => y.ItemName).ToList()),
                            icon = "",
                            notificationCategory = "home",
                            notificationType = "Actionable",
                            notificationId = 1,
                            notify_type = "home",
                            url = "",
                        };
                        var AutoNotification = new ManualAutoNotification
                        {
                            CreatedDate = DateTime.Now,
                            FcmKey = Key,
                            IsActive = true,
                            IsSent = false,
                            NotificationMsg = Newtonsoft.Json.JsonConvert.SerializeObject(data),
                            ObjectId = item.CustomerId,
                            ObjectType = "Customer"
                        };
                        try
                        {
                            //WebRequest tRequest = WebRequest.Create("https://fcm.googleapis.com/fcm/send") as HttpWebRequest;
                            //tRequest.Method = "post";
                            //string jsonNotificationFormat = Newtonsoft.Json.JsonConvert.SerializeObject(item);
                            //Byte[] byteArray = Encoding.UTF8.GetBytes(jsonNotificationFormat);
                            //tRequest.Headers.Add(string.Format("Authorization: key={0}", AutoNotification.FcmKey));

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
                            //                    AutoNotification.IsSent = true;
                            //                }
                            //                else if (response.failure == 1)
                            //                {
                            //                    AutoNotification.IsSent = false;
                            //                }
                            //            }
                            //        }
                            //    }
                            //}


                            var result = await firebaseService.SendNotificationForApprovalAsync(item.fcmId, data);
                            if (result != null)
                            {
                                AutoNotification.IsSent = true;
                            }
                            else
                            {
                                AutoNotification.IsSent = false;
                            }
                        }
                        catch (Exception asd)
                        {
                            logger.Error("Error during sent Notify auto notification : " + asd.ToString());
                        }

                        AutoNotifications.Add(AutoNotification);
                    });

                    if (parellelResult.IsCompleted)
                    {
                        if (AutoNotifications.Any())
                            AutoNotificationmongoDbHelper.InsertMany(AutoNotifications);

                        foreach (var item in customerItemNotifyMe)
                        {
                            if (elasticSearchItems.Any(x => x.itemnumber == item.ItemNumber && x.warehouseid == item.WarehouseId))
                            {
                                item.IsSentNotify = true;
                                item.Customers.ForEach(x => x.IsNotify = true);
                                mongoDbHelper.Replace(item.Id, item);
                            }
                        }

                    }

                }
            }
            return true;
        }
    }


}