using AngularJSAuthentication.API.Helper.Notification;
using AngularJSAuthentication.Common.Helpers;
using AngularJSAuthentication.Model;
using GenricEcommers.Models;
using Nito.AspNetBackgroundTasks;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;

namespace AngularJSAuthentication.API.Controllers.Reporting
{
    [RoutePrefix("api/CustomerWalletPoint")]
    public class CustomerWalletPointController : ApiController
    {
        private static TimeZoneInfo INDIAN_ZONE = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
        private static DateTime indianTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE);

        [Route("UpdateCustomerAutodeductWalletPoint")]
        [HttpGet]
        [AllowAnonymous]
        public bool UpdateCustomerAutodeductWalletPoint()
        {
            string RDays = ConfigurationManager.AppSettings["ConfigureWalletExpireDays"];
            string ConsumerDays = ConfigurationManager.AppSettings["ConsumerConfigureWalletExpireDays"];

            int DaysRemove = Convert.ToInt32(RDays);
            int ConsumerDaysRemove = Convert.ToInt32(ConsumerDays);
            if (DaysRemove > 0)
            {
                bool IsConfWalletRemoved = ConfigureWalletRemove(DaysRemove);
                bool IsConsumerConfWalletRemoved = ConsumerConfigureWalletRemove(ConsumerDaysRemove);
            }

            using (AuthContext db = new AuthContext())
            {
                #region If any retailer is not active for 1-month auto deduct points
                DateTime InMonth = indianTime.AddMonths(-1);

                #region Get Data From S
                //get from Customer
                if (db.Database.Connection.State != ConnectionState.Open)
                    db.Database.Connection.Open();

                db.Database.CommandTimeout = 6000;
                var cmd = db.Database.Connection.CreateCommand();
                cmd.CommandText = "[dbo].GetCustomerListWithFCM";
                cmd.CommandType = System.Data.CommandType.StoredProcedure;

                // Run the sproc
                var reader = cmd.ExecuteReader();
                List<CustomerIdsList> custlist = ((IObjectContextAdapter)db)
                .ObjectContext
                .Translate<CustomerIdsList>(reader).ToList();
                //var custlist = db.Database.SqlQuery<CustomerIdsList>("exec [GetCustomerListWithFCM]").ToList();

                var CustIds = custlist.Select(x => x.CustomerId).ToList();

                //Get from Order
                var CustomerIds = new DataTable();
                CustomerIds.Columns.Add("IntValue");
                foreach (var item in CustIds)
                {
                    var dr = CustomerIds.NewRow();
                    dr["IntValue"] = item;
                    CustomerIds.Rows.Add(dr);
                }

                var param = new SqlParameter("param", CustomerIds);
                param.SqlDbType = SqlDbType.Structured;
                param.TypeName = "dbo.IntValues";
                var OrderList = db.Database.SqlQuery<OrderMonthList>("exec [GetCustomerLastThirtyDayOrder] @param", param).ToList();
                #endregion

                ConcurrentBag<CustomerWalletHistory> AddWalletHistory = new ConcurrentBag<CustomerWalletHistory>();
                ConcurrentBag<DeviceNotification> addDeviceNotification = new ConcurrentBag<DeviceNotification>();
                ConcurrentBag<Wallet> UpdateWallet = new ConcurrentBag<Wallet>();

                var CustomerIdMonthNotIds = OrderList.Where(x => x.CreatedDate < InMonth).Select(x => x.CustomerId).Distinct().ToList();
                //get wallet of customer who has not place order in last 30  days
                var walletList = db.WalletDb.Where(c => CustomerIdMonthNotIds.Contains(c.CustomerId) && c.TotalAmount > 0).ToList();


                //Excel
                string TartgetfolderPath = "";
                TartgetfolderPath = System.Web.HttpContext.Current.Server.MapPath(@"~\CustAutowalletDeduct");
                if (!Directory.Exists(TartgetfolderPath))
                    Directory.CreateDirectory(TartgetfolderPath);

                string thFisleName = DateTime.Now.Year.ToString() + DateTime.Now.Month.ToString() + DateTime.Now.Day.ToString() + DateTime.Now.Hour.ToString() + DateTime.Now.Minute.ToString(); ;
                string thFileName = thFisleName + "_CustAutowalletDeduct.csv";
                string fullPhysicalPath = TartgetfolderPath + "\\" + thFileName;
                using (StreamWriter sw = File.CreateText(fullPhysicalPath))
                {
                    for (int i = 0; i < walletList.Count; i++)
                    {
                        sw.WriteLine(walletList[i].CustomerId);
                    }
                }

                ParallelLoopResult parellelResult = Parallel.ForEach(custlist, (cust) =>
                //foreach (var cust in custlist)
                {
                    var IsSmsSent = false;
                    bool ODMInMonth = OrderList.Any(x => x.CreatedDate > InMonth && x.CustomerId == cust.CustomerId);
                    if (!ODMInMonth)
                    {
                        var walt = walletList.Where(c => c.CustomerId == cust.CustomerId).FirstOrDefault();
                        if (walt != null && walt.TotalAmount > 0)
                        {
                            TextFileLogHelper.TraceLog("Customer(AutodeductWalletPoint) , " + walt.CustomerId + " , old , " + walt.TotalAmount.ToString());

                            CustomerWalletHistory od = new CustomerWalletHistory();
                            //update wallet history to zero  
                            od.CustomerId = walt.CustomerId;
                            od.WarehouseId = walt.WarehouseId;
                            od.CompanyId = walt.CompanyId;

                            od.PeopleName = "Auto system deduct";
                            od.Through = "due to no order in last 30Days";
                            od.NewOutWAmount = walt.TotalAmount;

                            od.TotalWalletAmount = 0;
                            od.comment = walt.TotalAmount + " point Expired";
                            od.UpdatedDate = indianTime;
                            od.TransactionDate = indianTime;
                            od.CreatedDate = indianTime;
                            AddWalletHistory.Add(od);

                            //update wallet to zero
                            walt.TotalAmount = 0;
                            walt.UpdatedDate = indianTime;
                            walt.TransactionDate = indianTime;
                            UpdateWallet.Add(walt);
                        }
                    }
                    #endregion
                    else
                    {
                        #region If any retailer is not active for 29days send  notification
                        DateTime InTwentyNineDay = indianTime.AddDays(-29);
                        var ODMInTwentyNineDay = OrderList.Any(x => x.CreatedDate > InTwentyNineDay && x.CustomerId == cust.CustomerId);
                        if (!ODMInTwentyNineDay)
                        {
                            var title = "कृपया आर्डर करे !!";
                            var Message = "आपने " + 29 + " दिनों से आर्डर नहीं किया है , " + 1 + " दिन में आपके ड्रीम पॉइंट की वैद्यता समाप्त हो जाएगी ।";

                            DeviceNotification obj = new DeviceNotification();
                            obj.CustomerId = cust.CustomerId;

                            obj.title = title;
                            obj.Message = Message;
                            obj.NotificationTime = indianTime;
                            obj.Deleted = false;
                            addDeviceNotification.Add(obj);

                            if (cust.fcmId != null)
                            {
                                BackgroundTaskManager.Run(() => sendRemindersToCustomer(cust, 29, 1));
                            }
                            IsSmsSent = true;


                        }
                        #endregion
                        #region If any retailer is not active for 20days send  notification
                        DateTime InTwentyDay = indianTime.AddDays(-20);
                        var ODMInTwentyDay = OrderList.Any(x => x.CreatedDate > InTwentyDay && x.CustomerId == cust.CustomerId);
                        if (!ODMInTwentyDay && !IsSmsSent)
                        {
                            var title = "कृपया आर्डर करे !!";
                            var Message = "आपने " + 20 + " दिनों से आर्डर नहीं किया है , " + 10 + " दिन में आपके ड्रीम पॉइंट की वैद्यता समाप्त हो जाएगी ।";

                            DeviceNotification obj = new DeviceNotification();
                            obj.CustomerId = cust.CustomerId;

                            obj.title = title;
                            obj.Message = Message;
                            obj.NotificationTime = indianTime;
                            obj.Deleted = false;
                            addDeviceNotification.Add(obj);
                            if (cust.fcmId != null)
                            {
                                BackgroundTaskManager.Run(() => sendRemindersToCustomer(cust, 20, 10));
                            }
                            IsSmsSent = true;
                        }
                        #endregion
                        #region If any retailer is not active for 10days send notification
                        DateTime InTenDay = indianTime.AddDays(-10);
                        bool ODMInTenDay = OrderList.Any(x => x.CreatedDate > InTenDay && x.CustomerId == cust.CustomerId);
                        if (!ODMInTenDay && !IsSmsSent)
                        {
                            var title = "कृपया आर्डर करे !!";
                            var Message = "आपने " + 10 + " दिनों से आर्डर नहीं किया है , " + 20 + " दिन में आपके ड्रीम पॉइंट की वैद्यता समाप्त हो जाएगी ।";

                            DeviceNotification obj = new DeviceNotification();
                            obj.CustomerId = cust.CustomerId;

                            obj.title = title;
                            obj.Message = Message;
                            obj.NotificationTime = indianTime;
                            obj.Deleted = false;
                            addDeviceNotification.Add(obj);
                            if (cust.fcmId != null)
                            {
                                BackgroundTaskManager.Run(() => sendRemindersToCustomer(cust, 10, 20));
                            }
                        }

                        #endregion

                    }
                });

                if (parellelResult.IsCompleted && AddWalletHistory.Any())
                {
                    if (AddWalletHistory != null && AddWalletHistory.Count > 0)
                    {
                        db.CustomerWalletHistoryDb.AddRange(AddWalletHistory);
                    }
                    if (addDeviceNotification != null && addDeviceNotification.Count > 0)
                    {
                        db.DeviceNotificationDb.AddRange(addDeviceNotification);
                    }
                    if (UpdateWallet != null && UpdateWallet.Count > 0)
                    {
                        foreach (var item in UpdateWallet)
                        {
                            db.Entry(item).State = EntityState.Modified;
                        }

                    }
                    if (db.Commit() > 0)
                    {
                        return true;
                    }
                }
            }

            return false;

        }


        #region send Reminders To Customer to use there wallet points
        private static async Task<bool> sendRemindersToCustomer(CustomerIdsList Customer, int Day, int endDay)
        {
            bool Result = false;
            try
            {
                Notification notification = new Notification();
                notification.title = "कृपया आर्डर करे !!";
                if (endDay == 1)
                {
                    notification.Message = "आपने " + Day + " दिनों से आर्डर नहीं किया है , " + endDay + " दिन में आपके ड्रीम पॉइंट की वैद्यता समाप्त हो जाएगी ।";
                }
                else
                {
                    notification.Message = "आपने " + Day + " दिनों से आर्डर नहीं किया है , " + endDay + " दिनों में आपके ड्रीम पॉइंट की वैद्यता समाप्त हो जाएगी ।";
                }
                notification.Pic = "http://shopkirana.com/wp-content/uploads/2015/07/ShopKirana-Logo11.png";
                string Key = ConfigurationManager.AppSettings["FcmApiKey"];
                // string id = ConfigurationManager.AppSettings["FcmApiId"];
                //WebRequest tRequest = WebRequest.Create("https://fcm.googleapis.com/fcm/send") as HttpWebRequest;
                //tRequest.Method = "post";

                //var objNotification = new
                //{
                //    data = new
                //    {
                //        title = notification.title,
                //        body = notification.Message,
                //        icon = notification.Pic
                //    },
                //    to = Customer.fcmId
                //};

                if (Customer.fcmId != null)
                {
                    //string jsonNotificationFormat = Newtonsoft.Json.JsonConvert.SerializeObject(objNotification);
                    //Byte[] byteArray = Encoding.UTF8.GetBytes(jsonNotificationFormat);
                    //tRequest.Headers.Add(string.Format("Authorization: key={0}", Key));
                    //// tRequest.Headers.Add(string.Format("Sender: id={0}", id11));
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
                    //                    return true;
                    //                }
                    //                else if (response.failure == 1)
                    //                {
                    //                    return false;
                    //                }
                    //            }
                    //        }
                    //    }
                    //}

                    var data = new FCMData
                    {
                        title = notification.title,
                        body = notification.Message,
                        icon = notification.Pic
                    };
                    var firebaseService = new FirebaseNotificationServiceHelper(Key);
                    var result = await firebaseService.SendNotificationForApprovalAsync(Customer.fcmId, data);
                    if (result != null)
                    {
                        Result = true;
                    }
                    else
                    {
                        Result = false;
                    }
                }
            }
            catch (Exception es)
            {
                return false;
            }
            return Result;
        }
        #endregion



        #region send Reminders To Customer to use there wallet points
        private static bool ConfigureWalletRemove(int RemoveBeforDays)
        {
            using (AuthContext db = new AuthContext())
            {

                #region Get Data 
                db.Database.CommandTimeout = 1200;

                string Query = "Exec ConfiguredExpireCustomersWallet " + RemoveBeforDays;
                var OrderList = db.Database.SqlQuery<OrderMonthList>(Query).ToList();

                if (OrderList != null && OrderList.Any())
                {
                    #endregion
                    ConcurrentBag<CustomerWalletHistory> AddWalletHistory = new ConcurrentBag<CustomerWalletHistory>();
                    ConcurrentBag<Wallet> UpdateWallet = new ConcurrentBag<Wallet>();
                    var CustomerIdNotIds = OrderList.Select(x => x.CustomerId).Distinct().ToList();
                    var walletList = db.WalletDb.Where(c => CustomerIdNotIds.Contains(c.CustomerId) && c.TotalAmount > 0).ToList();

                    ParallelLoopResult parellelResult = Parallel.ForEach(CustomerIdNotIds, (cust) =>
                    {
                        var walt = walletList.Where(c => c.CustomerId == cust).FirstOrDefault();
                        if (walt != null && walt.TotalAmount > 0)
                        {
                            CustomerWalletHistory od = new CustomerWalletHistory();
                            //update wallet history to zero  
                            od.CustomerId = walt.CustomerId;
                            od.WarehouseId = walt.WarehouseId;
                            od.CompanyId = walt.CompanyId;
                            od.PeopleName = "Auto system deduct";
                            od.Through = "due to no order in last " + RemoveBeforDays + "Days";
                            od.comment = walt.TotalAmount + " point Expired";
                            od.TotalWalletAmount = 0;
                            od.NewOutWAmount = walt.TotalAmount;
                            od.UpdatedDate = indianTime;
                            od.TransactionDate = indianTime;
                            od.CreatedDate = indianTime;
                            AddWalletHistory.Add(od);

                            //update wallet to zero
                            walt.TotalAmount = 0;
                            walt.UpdatedDate = indianTime;
                            walt.TransactionDate = indianTime;
                            UpdateWallet.Add(walt);
                        }
                    });

                    if (parellelResult.IsCompleted && AddWalletHistory.Any())
                    {

                        if (AddWalletHistory != null && AddWalletHistory.Count > 0)
                        {
                            db.CustomerWalletHistoryDb.AddRange(AddWalletHistory);
                        }
                        if (UpdateWallet != null && UpdateWallet.Count > 0)
                        {
                            foreach (var item in UpdateWallet)
                            {
                                db.Entry(item).State = EntityState.Modified;
                            }

                        }
                        if (db.Commit() > 0)
                        {
                            return true;
                        }
                    }
                }
            }

            return true;
        }

        private static bool ConsumerConfigureWalletRemove(int RemoveBeforDays)
        {
            using (AuthContext db = new AuthContext())
            {

                #region Get Data 
                db.Database.CommandTimeout = 1200;

                string Query = "Exec ConsumerConfiguredExpireCustomersWallet " + RemoveBeforDays;
                var OrderList = db.Database.SqlQuery<OrderMonthList>(Query).ToList();

                if (OrderList != null && OrderList.Any())
                {
                    #endregion
                    ConcurrentBag<CustomerWalletHistory> AddWalletHistory = new ConcurrentBag<CustomerWalletHistory>();
                    ConcurrentBag<Wallet> UpdateWallet = new ConcurrentBag<Wallet>();
                    var CustomerIdNotIds = OrderList.Select(x => x.CustomerId).Distinct().ToList();
                    var walletList = db.WalletDb.Where(c => CustomerIdNotIds.Contains(c.CustomerId) && c.TotalAmount > 0).ToList();

                    ParallelLoopResult parellelResult = Parallel.ForEach(CustomerIdNotIds, (cust) =>
                    {
                        var walt = walletList.Where(c => c.CustomerId == cust).FirstOrDefault();
                        if (walt != null && walt.TotalAmount > 0)
                        {
                            CustomerWalletHistory od = new CustomerWalletHistory();
                            //update wallet history to zero  
                            od.CustomerId = walt.CustomerId;
                            od.WarehouseId = walt.WarehouseId;
                            od.CompanyId = walt.CompanyId;
                            od.PeopleName = "Auto system deduct";
                            od.Through = "due to no order in last " + RemoveBeforDays + "Days";
                            od.comment = walt.TotalAmount + " point Expired";
                            od.TotalWalletAmount = 0;
                            od.NewOutWAmount = walt.TotalAmount;
                            od.UpdatedDate = indianTime;
                            od.TransactionDate = indianTime;
                            od.CreatedDate = indianTime;
                            AddWalletHistory.Add(od);

                            //update wallet to zero
                            walt.TotalAmount = 0;
                            walt.UpdatedDate = indianTime;
                            walt.TransactionDate = indianTime;
                            UpdateWallet.Add(walt);
                        }
                    });

                    if (parellelResult.IsCompleted && AddWalletHistory.Any())
                    {

                        if (AddWalletHistory != null && AddWalletHistory.Count > 0)
                        {
                            db.CustomerWalletHistoryDb.AddRange(AddWalletHistory);
                        }
                        if (UpdateWallet != null && UpdateWallet.Count > 0)
                        {
                            foreach (var item in UpdateWallet)
                            {
                                db.Entry(item).State = EntityState.Modified;
                            }

                        }
                        if (db.Commit() > 0)
                        {
                            return true;
                        }
                    }
                }
            }

            return true;
        }
        #endregion





        [Route("updateMaxWalletPointsSet")]
        [HttpPost]
        public bool UpdateMaxWalletPointsSet(MaxWalletpointused MaxWalletpointused)
        {
            using (AuthContext db = new AuthContext())
            {
                var updatewalletpoint = db.CompanyDetailsDB.Where(x => x.IsActive == true && x.IsDeleted == false).FirstOrDefault();
                if (updatewalletpoint != null)
                {
                    updatewalletpoint.MaxWalletPointUsed = MaxWalletpointused.Maxwalletpointused;
                    db.Entry(updatewalletpoint).State = EntityState.Modified;
                    db.Commit();
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        [Route("getCompanyDetails")]
        [HttpGet]
        public dynamic getCompanyDetails()
        {
            using (AuthContext db = new AuthContext())
            {
                var updatewalletpoint = db.CompanyDetailsDB.Where(x => x.IsActive == true && x.IsDeleted == false).FirstOrDefault();
                return updatewalletpoint;
            }
        }
        public class MaxWalletpointused
        {
            public int Maxwalletpointused { get; set; }
        }
        public class FCMResponse
        {
            public long multicast_id { get; set; }
            public int success { get; set; }
            public int failure { get; set; }
            public int canonical_ids { get; set; }
            public List<FCMResult> results { get; set; }
        }
        public class FCMResult
        {
            public string message_id { get; set; }
        }

        public class CustomerIdsList
        {
            public int CustomerId { get; set; }
            public string fcmId { get; set; }
            public string MobileNo { get; set; }

        }

        public class OrderMonthList
        {
            public int CustomerId { get; set; }
            public DateTime CreatedDate { get; set; }
        }
    }

}
