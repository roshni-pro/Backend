using AngularJSAuthentication.API.Controllers;
using AngularJSAuthentication.API.Helper.Notification;
using AngularJSAuthentication.API.Helpers;
using AngularJSAuthentication.BusinessLayer.FinBox;
using AngularJSAuthentication.DataContracts.Mongo;
using AngularJSAuthentication.Model;
using Newtonsoft.Json;
using NLog;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.API.App_Code.FinBox
{
    public class FinBoxRepository : IDisposable
    {
        string Url = ConfigurationManager.AppSettings["FinBoxUrl"];
        string ServerKey = ConfigurationManager.AppSettings["FinBoxServerApiKey"];
        string isCreditLineServerKey = ConfigurationManager.AppSettings["isCreditLineServerKey"];
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private bool disposed = false;
        private AuthContext Context;
        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    Context.Dispose();
                }
            }
            this.disposed = true;
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        public FinBoxRepository(AuthContext Context)
        {
            this.Context = Context;
        }

        public ResponseDC CreateUser(CreateUserDC createUserDC)
        {
            using (AuthContext context = new AuthContext())
            {

                ResponseDC objResponseDC = new ResponseDC();
                HttpClient client = new HttpClient();

                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(
                    new MediaTypeWithQualityHeaderValue("application/json"));

                if (createUserDC.isCreditLine)
                {
                    client.DefaultRequestHeaders.Add("x-api-key", isCreditLineServerKey);
                }
                else
                {
                    client.DefaultRequestHeaders.Add("x-api-key", ServerKey);
                }


                var res = client.PostAsJsonAsync(Url + "/v1/user/create", createUserDC).Result;
                bool FinBoxRequestResponse = AddFinBoxRequestResponse(Url + "/v1/user/create", Convert.ToInt32(createUserDC.customerID), res.Content.ReadAsStringAsync().Result, JsonConvert.SerializeObject(createUserDC), "");

                string jsonString = res.Content.ReadAsStringAsync().Result.Replace("\\", "").Trim(new char[1] { '"' });

                objResponseDC = JsonConvert.DeserializeObject<ResponseDC>(jsonString);
                bool Result = true;
                if (res.IsSuccessStatusCode && FinBoxRequestResponse)
                {
                    int CustomerId = Convert.ToInt32(createUserDC.customerID);
                    var data = context.Customers.Where(x => x.CustomerId == CustomerId).FirstOrDefault();
                    if (data != null)
                    {
                        if (createUserDC.isCreditLine)
                            data.IsFinBoxCreditLine = true;
                        else
                            data.IsFinBox = true;

                        context.Entry(data).State = EntityState.Modified;
                        Result = true;
                    }
                    if (Result)
                    {
                        context.Commit();
                        Eligiblity(CustomerId, createUserDC.isCreditLine);
                    }
                }



                return objResponseDC;
            }
        }

        public EligibleResponseDC Eligiblity(int CustomerId, bool isCreditLine)
        {

            HttpClient client = new HttpClient();

            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
            if (isCreditLine)
                client.DefaultRequestHeaders.Add("x-api-key", isCreditLineServerKey);
            else
                client.DefaultRequestHeaders.Add("x-api-key", ServerKey);


            string RequestUrl = Url + "/v1/user/eligibility?customerID=" + CustomerId;

            var res = client.GetAsync(RequestUrl).Result;
            bool FinBoxRequestResponse = AddFinBoxRequestResponse(RequestUrl, CustomerId, res.Content.ReadAsStringAsync().Result, "", "");
            string jsonString = res.Content.ReadAsStringAsync().Result.Replace("\\", "").Trim(new char[1] { '"' });
            EligibleResponseDC eligibleResponseDC = new EligibleResponseDC();
            if (res.IsSuccessStatusCode && FinBoxRequestResponse)
            {
                eligibleResponseDC = JsonConvert.DeserializeObject<EligibleResponseDC>(jsonString);

            }
            return eligibleResponseDC;

        }

        public TokenDC GenerateToken(int CustomerId, bool isCreditLine)
        {
            HttpClient client = new HttpClient();

            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));

            if (isCreditLine)
            {
                client.DefaultRequestHeaders.Add("x-api-key", isCreditLineServerKey);
            }
            else
            {
                client.DefaultRequestHeaders.Add("x-api-key", ServerKey);
            }


            CreateUserDC createUserDC = new CreateUserDC()
            {
                customerID = Convert.ToString(CustomerId)
            };
            var res = client.PostAsJsonAsync(Url + "/v1/user/token", createUserDC).Result;
            bool FinBoxRequestResponse = AddFinBoxRequestResponse(Url + "/v1/user/token", CustomerId, res.Content.ReadAsStringAsync().Result, JsonConvert.SerializeObject(createUserDC), "");
            string jsonString = res.Content.ReadAsStringAsync().Result.Replace("\\", "").Trim(new char[1] { '"' });
            TokenDC ObjTokenDc = new TokenDC();
            if (res.IsSuccessStatusCode)
            {
                ObjTokenDc = JsonConvert.DeserializeObject<TokenDC>(jsonString);


            }
            return ObjTokenDc;
        }


        public bool IsFinBox(int CustomerId, bool isCreditLine)
        {
            using (AuthContext context = new AuthContext())
            {
                var data = context.Customers.Where(x => x.CustomerId == CustomerId && x.Deleted == false).FirstOrDefault();
                if (data != null)
                {
                    if (isCreditLine)
                        return data.IsFinBoxCreditLine;
                    else
                        return data.IsFinBox;
                }
                else
                {
                    return false;
                }
            }
        }
        //use same method for finbox and credit line
        public bool GenerateWebhook(WebhookDC webhookDC)
        {
            bool Result = false;
            Webhook webhook = new Webhook()
            {
                CustomerID = Convert.ToInt32(webhookDC.customerID),
                EventType = webhookDC.eventType,
                EntityType = webhookDC.entityType,
                EventDescription = webhookDC.eventDescription,
                LoanApplicationID = webhookDC.loanApplicationID,
                CreatedBy = Convert.ToInt32(webhookDC.customerID),
                LoggedAt = Convert.ToDateTime(webhookDC.loggedAt),
                IsCreditLine = webhookDC.IsCreditLine,
                IsActive = true,
                IsDelete = false,
                CreatedDate = DateTime.Now
            };
            Context.Webhook.Add(webhook);

            if (webhook != null)
            {
                int CustomerId = Convert.ToInt32(webhookDC.customerID);
                var data = Context.Customers.Where(x => x.CustomerId == CustomerId).FirstOrDefault();
                if (data != null)
                {
                    data.UpdatedDate = DateTime.Now;
                    var apptype = 0;
                    if (webhookDC.IsCreditLine)
                        data.CurrentCreditLineActivity = webhookDC.eventType;
                    else
                        data.CurrentFinBoxActivity = webhookDC.eventType;

                    Context.Entry(data).State = EntityState.Modified;
                    Context.Commit();
                    var FinBoxConfigs = Context.FinBoxConfigs.Where(x => x.Type == 1 && x.AppType == apptype).ToList();
                    if (FinBoxConfigs.Any(x => x.Activity == webhookDC.eventType))
                    {
                        var finbox = FinBoxConfigs.FirstOrDefault(x => x.Activity == webhookDC.eventType);
                        if (!string.IsNullOrEmpty(finbox.Text))
                        {
                            SendNotification(finbox.NotificationTitle, finbox.Text, webhookDC.IsCreditLine, data);
                        }
                    }
                    Result = true;
                }
            }
            return Result;
        }

        private bool AddFinBoxRequestResponse(string RequestUrl, int CustomerId, string ResponseJson, string RequestJson, string ErrorMessage)
        {
            bool Result = false;

            FinBoxRequestResponse ObjFinBoxRequestResponse = new FinBoxRequestResponse();
            ObjFinBoxRequestResponse.RequestUrl = RequestUrl;
            ObjFinBoxRequestResponse.RequestJson = RequestJson;
            ObjFinBoxRequestResponse.ResponseJson = ResponseJson;
            ObjFinBoxRequestResponse.CustomerId = CustomerId;
            ObjFinBoxRequestResponse.CreatedDate = DateTime.Now;
            ObjFinBoxRequestResponse.ErrorMessage = ErrorMessage;
            Context.FinBoxRequestResponse.Add(ObjFinBoxRequestResponse);
            if (ObjFinBoxRequestResponse != null)
            {
                Context.Commit();

                Result = true;


            }
            return Result;
        }

        private bool SendNotification(string Title, string Message, bool isCreditLine, Customer Objcustomer)
        {
            bool Result = false;
            string Key = ConfigurationManager.AppSettings["FcmApiKey"];

            if (Objcustomer != null)
            {
                Message = Message.Replace("[[CustomerName]]", !string.IsNullOrEmpty(Objcustomer.ShopName) ? Objcustomer.ShopName : Objcustomer.Name);

                //dynamic notification = new
                //{
                //    to = Objcustomer.fcmId,
                //    CustId = Objcustomer.CustomerId,
                //    data = new
                //    {
                //        title = Title,
                //        body = Message,
                //        icon = "",
                //        typeId = 0,
                //        notificationCategory = "home",
                //        notificationType = "Actionable",
                //        notificationId = 1,
                //        notify_type = isCreditLine ? "CreditLine" : "FinBox",
                //        url = "",
                //    }
                //};
                var data = new FCMData
                {
                    title = Title,
                    body = Message,
                    icon = "",
                    typeId = 0,
                    notificationCategory = "home",
                    notificationType = "Actionable",
                    notificationId = 1,
                    notify_type = isCreditLine ? "CreditLine" : "FinBox",
                    url = "",
                };
                var AutoNotification = new ManualAutoNotification
                {
                    CreatedDate = DateTime.Now,
                    FcmKey = ConfigurationManager.AppSettings["FcmApiKey"],
                    IsActive = true,
                    IsSent = false,
                    NotificationMsg = Newtonsoft.Json.JsonConvert.SerializeObject(data),
                    ObjectId = Objcustomer.CustomerId,
                    ObjectType = isCreditLine ? "CreditLine" : "FinBox"
                };
                try
                {

                    var firebaseService = new FirebaseNotificationServiceHelper(Key);
                    //var fcmid = "fZGIeP5dTKGd-2JBZttcDj:APA91bGINtqXqKuCcEHd4qXZMN-VtX5KC2g98KkytmGdpc28_-duDu8Ry1P6Kk_Xb9RgRG0iDWrp8DkwE1EPCOPG0OLz2uRjo-HBg-Ysg-mDaMErlYLjJGZE7ScXKIkjmWZ0xNO6UyxN";
                    var result = firebaseService.SendNotificationForApprovalAsync(Objcustomer.fcmId, data);
                    if (result != null)
                    {
                        AutoNotification.IsSent = true;
                        Result = true;
                    }
                    else
                    {
                        AutoNotification.IsSent = false;
                    }
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
                    //                    Result = true;
                    //                }
                    //                else if (response.failure == 1)
                    //                {
                    //                    AutoNotification.IsSent = false;
                    //                }
                    //            }
                    //        }
                    //    }
                    //}
                }
                catch (Exception ex)
                {
                    logger.Error("Error during sent finbox notification : " + ex.ToString());
                    AutoNotification.IsSent = false;
                }
                MongoDbHelper<ManualAutoNotification> AutoNotificationmongoDbHelper = new MongoDbHelper<ManualAutoNotification>();
                AutoNotificationmongoDbHelper.Insert(AutoNotification);

            }
            return Result;
        }

        public bool IsEligibilityAlreadycalculated(int CustomerId, bool isCreditLine)
        {
            bool Result = false;
            using (AuthContext context = new AuthContext())
            {

                var data = context.Customers.Where(x => x.CustomerId == CustomerId && x.Deleted == false).FirstOrDefault();
                if (data != null)
                {
                    if (isCreditLine)
                    {
                        if (string.IsNullOrEmpty(data.CurrentFinBoxActivity) || data.CurrentFinBoxActivity.ToLower() == "user_created")
                        {
                            Result = false;

                        }
                        else
                        {
                            Result = true;
                        }
                    }
                    else
                    {
                        if (string.IsNullOrEmpty(data.CurrentCreditLineActivity) || data.CurrentCreditLineActivity.ToLower() == "user_created")
                        {
                            Result = false;

                        }
                        else
                        {
                            Result = true;
                        }
                    }
                }
                else
                {
                    Result = false;

                }
            }
            return Result;
        }

        public async Task<CreditLimitData> GetCustomerCreditLimit(int CustomerId, bool isCreditLine)
        {
            CreditLimitResponseDC objResponseDC = new CreditLimitResponseDC();
            using (AuthContext context = new AuthContext())
            {


                HttpClient client = new HttpClient();

                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(
                    new MediaTypeWithQualityHeaderValue("application/json"));

                if (isCreditLine)
                {
                    client.DefaultRequestHeaders.Add("x-api-key", isCreditLineServerKey);
                }
                else
                {
                    client.DefaultRequestHeaders.Add("x-api-key", ServerKey);
                }

                var res = await client.GetAsync(Url + "/v1/creditline/details?customerID=" + CustomerId);
                string jsonString = (await res.Content.ReadAsStringAsync()).Replace("\\", "").Trim(new char[1] { '"' });
                bool FinBoxRequestResponse = AddFinBoxRequestResponse(Url + "/v1/creditline/details", CustomerId, jsonString, "CustomerId=" + CustomerId, "");

                objResponseDC = JsonConvert.DeserializeObject<CreditLimitResponseDC>(jsonString);

                if (res.IsSuccessStatusCode && FinBoxRequestResponse)
                {
                    if (objResponseDC.status)
                    {
                        if (objResponseDC.data.status.ToUpper() == "ACTIVE" && objResponseDC.data.validity.Date > DateTime.Now.Date)
                            objResponseDC.data.availableLimit = objResponseDC.data.availableLimit;
                        else
                            objResponseDC.data.availableLimit = 0;
                    }
                }
                return objResponseDC.data;
            }
        }
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
}