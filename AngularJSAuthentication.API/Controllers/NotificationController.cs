using AngularJSAuthentication.API.Helper.Notification;
using AngularJSAuthentication.API.Helpers;
using AngularJSAuthentication.Common.Constants;
using AngularJSAuthentication.DataContracts.Transaction;
using AngularJSAuthentication.Model;
using GenricEcommers.Models;
using NLog;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Common;
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

namespace AngularJSAuthentication.API.Controllers
{
    [RoutePrefix("api/Notification")]
    public class NotificationController : ApiController
    {

        private static Logger logger = LogManager.GetCurrentClassLogger();
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
                    var total_count = context.NotificationDb.Where(x => x.Message != null && x.CompanyId == CompanyId).Count();
                    var notificationmaster = context.NotificationDb.Where(x => x.Message != null && x.CompanyId == CompanyId).OrderByDescending(x => x.NotificationTime).Skip((page - 1) * list).Take(list).ToList();
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

        [Route("getall")]
        [HttpGet]
        public PaggingDatas notifyDelivered(int list, int page, int customerid)
        {
            using (var context = new AuthContext())
            {
                PaggingDatas data = new PaggingDatas();
                context.Database.CommandTimeout = 600;
                var query = "GetCustomerNotification " + customerid.ToString() + "," + ((page - 1) * list).ToString() + ",10";
                var DeviceNotificationDcs = context.Database.SqlQuery<DeviceNotificationDc>(query).ToList();

                //var notificationmaster = context.DeviceNotificationDb.Where(x => x.CustomerId == customerid).ToList();//.OrderByDescending(x => x.NotificationTime).Skip((page - 1) * list).Take(10).ToList();
                data.notificationmaster = DeviceNotificationDcs;
                data.total_count = DeviceNotificationDcs != null && DeviceNotificationDcs.Any() ? DeviceNotificationDcs.FirstOrDefault().TotalCount : 0;
                return data;


            }
        }
        #region get notification all sales person
        /// <summary>
        /// notification all sales person
        /// create date 05/03/2019
        /// </summary>
        /// <param name="salespersonid"></param>
        /// <returns></returns>
        [Route("getNotifysales")]
        [HttpGet]
        public HttpResponseMessage get(int salespersonid)
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
                    PaggingDatas data = new PaggingDatas();
                    var salesperson = context.Peoples.Where(x => x.PeopleID == salespersonid && x.Active == true).SingleOrDefault();

                    List<SalespersonNotification> spn = context.SalespersonNotificationDB.Where(x => x.WarehouseId == salesperson.WarehouseId && x.Deleted == false).ToList();

                    return Request.CreateResponse(HttpStatusCode.OK, spn);

                }
                catch (Exception ex)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex.Message);
                }
            }
        }
        #endregion
        [Route("all")]
        [HttpGet]
        public HttpResponseMessage Get()
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
                    ass = context.Customers.Where(c => c.Deleted == false && c.fcmId != null).ToList();
                    logger.Info("End  Return: ");
                    return Request.CreateResponse(HttpStatusCode.OK, ass);
                }
                catch (Exception ex)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex.Message);
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
                                select new notModel
                                {
                                    categoryId = c.Categoryid,
                                    CustomerId = i.CustomerId,
                                    orderTotal = i.TotalAmt,

                                    fcmId = j.fcmId
                                }).ToList();
                    var list1 = list.Where(x => x.categoryId == id).ToList();
                    List<notModel> uniqecustomer = new List<notModel>();
                    foreach (var a in list1)
                    {
                        notModel customer = uniqecustomer.Where(c => c.CustomerId == a.CustomerId).SingleOrDefault();
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
        //warehouse

        [Route("allware")]
        [HttpGet]
        public HttpResponseMessage Get(int Warehouseid, string idd)
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

                    ass = context.Customers.Where(c => c.fcmId != null && c.Warehouseid == Warehouseid).ToList();
                    logger.Info("End  Return: ");
                    return Request.CreateResponse(HttpStatusCode.OK, ass);
                }
                catch (Exception ex)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex.Message);
                }
            }
        }


        // city

        [Route("allcity")]
        [HttpGet]
        public HttpResponseMessage City(int Cityid)
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
                    ass = context.Customers.Where(c => c.fcmId != null && c.Cityid == Cityid).ToList();
                    logger.Info("End  Return: ");
                    return Request.CreateResponse(HttpStatusCode.OK, ass);
                }
                catch (Exception ex)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex.Message);
                }
            }
        }

        //cluster

        [Route("allcluster")]
        [HttpGet]
        public HttpResponseMessage Cluster(int ClusterId)
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
                    ass = context.Customers.Where(c => c.fcmId != null && c.ClusterId == ClusterId).ToList();
                    logger.Info("End  Return: ");
                    return Request.CreateResponse(HttpStatusCode.OK, ass);
                }
                catch (Exception ex)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex.Message);
                }
            }
        }

        [ResponseType(typeof(Notification))]
        [Route("")]
        [AcceptVerbs("POST")]
        public async Task<Notification> add(Notification notification)
        {
            logger.Info("Add message: ");
            using (var context = new AuthContext())
            {
                try
                {

                    var identity = User.Identity as ClaimsIdentity;
                    int compid = 0, userid = 0;
                    int WarehouseId = 0;

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
                        if (claim.Type == "Warehouseid")
                        {
                            WarehouseId = int.Parse(claim.Value);
                        }
                    }
                    //if (notification == null)
                    //{
                    //    throw new ArgumentNullException("message");
                    //}
                    //notification.Warehous = WarehouseId;
                    notification.CompanyId = compid;
                    List<Customer> customers = new List<Customer>();
                    if (notification.ids.Count() > 0)
                    {
                        foreach (var i in notification.ids)
                        {
                            // var olist = context.Customers.Where(x => x.fcmId != null && x.CustomerId == i.id  && x.CreatedDate >= notification.From && x.CreatedDate <= notification.TO).SingleOrDefault();
                            var olist = context.Customers.Where(x => x.fcmId != null && x.CustomerId == i.id).SingleOrDefault();
                            customers.Add(olist);
                        }
                    }
                    else
                    {
                        customers = context.Customers.Where(x => x.fcmId != null).ToList();
                    }
                    context.AddNotification(notification);
                    var data = new FCMData
                    {
                        title = notification.title,
                        body = notification.Message,
                        icon = notification.Pic

                    };
                    string Key = ConfigurationManager.AppSettings["FcmApiKey"];
                    var firebaseService = new FirebaseNotificationServiceHelper(Key);
                    foreach (var item in customers)
                    {
                        if (item.fcmId != null)
                        {
                            //Registration Id created by Android App i.e. DeviceId.  
                            string regId;
                            regId = item.Name;
                            //API Key created in Google project  

                            //var Key = "AIzaSyAjVSDVnY779ag7izGXGYUVmipRH3OQy5o";
                            //var id = "81570511363";
                            /// AIzaSyCDT5CYkPMCPaGHzuafk3_Gr0 - OehUS_Rs new from gole ji 22Feb ke bad ki 2019
                            /// 
                            //string Key = ConfigurationManager.AppSettings["FcmApiKey"];
                            //string id = ConfigurationManager.AppSettings["FcmApiId"];

                            //var varMessage = notification.Message;
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
                            //    to = item.fcmId
                            //};
                            //string jsonNotificationFormat = Newtonsoft.Json.JsonConvert.SerializeObject(objNotification);
                            //Byte[] byteArray = Encoding.UTF8.GetBytes(jsonNotificationFormat);
                            //tRequest.Headers.Add(string.Format("Authorization: key={0}", Key));
                            //tRequest.Headers.Add(string.Format("Sender: id={0}", id));
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
                            //                    try
                            //                    {
                            //                        DeviceNotification obj = new DeviceNotification();
                            //                        obj.CustomerId = item.CustomerId;
                            //                        obj.DeviceId = item.fcmId;
                            //                        obj.title = notification.title;
                            //                        obj.Message = notification.Message;
                            //                        obj.ImageUrl = notification.Pic;
                            //                        obj.NotificationTime = DateTime.Now;
                            //                        context.DeviceNotificationDb.Add(obj);
                            //                        int Id = context.Commit();
                            //                        // return true;
                            //                    }
                            //                    catch (Exception ex)
                            //                    {
                            //                        logger.Error("Error in Add message " + ex.Message);

                            //                        return null;
                            //                    }
                            //                }
                            //                else if (response.failure == 1)
                            //                {

                            //                }

                            //            }
                            //        }
                            //    }
                            //}
                            
                            
                            var Result = await firebaseService.SendNotificationForApprovalAsync(item.fcmId, data);
                            if (Result != null)
                            {
                                
                            }
                        }
                    }

                    logger.Info("End  Add message: ");
                    return notification;
                }
                catch (Exception ex)
                {
                    logger.Error("Error in Add message " + ex.Message);

                    return null;
                }
            }
        }

        /// <summary>
        /// NotificationClick//by Ajay 30-07-2019
        /// </summary>
        /// <param name="CustomerId"></param>
        /// <returns></returns>
        [Route("NotificationClick")]
        [AcceptVerbs("put")]
        [HttpPut]
        public async Task<HttpResponseMessage> NotificationClick(int NotificationId, int customerid)
        {
            // NotificationUpdated objnot = new NotificationUpdated();
            using (var db = new AuthContext())
            {
                {
                    customerDetails res;
                    try
                    {
                        var n = await db.NotificationUpdatedDb.Where(c => c.Id == NotificationId).FirstOrDefaultAsync();
                        int i = Convert.ToInt32(n.TotalViews);

                        if (n != null)
                        {

                            n.UpdateTime = DateTime.Now;
                            n.TotalViews = i + 1;
                            db.Entry(n).State = EntityState.Modified;
                            db.Commit();

                            var deviceNotification = db.DeviceNotificationDb.FirstOrDefault(x => x.NotificationId == NotificationId && x.CustomerId == customerid);
                            if (deviceNotification != null)
                            {
                                deviceNotification.IsView = 1;
                                db.Entry(deviceNotification).State = EntityState.Modified;
                                db.Commit();
                            }

                            res = new customerDetails()
                            {
                                customers = n,
                                Status = true,
                                Message = "Success."
                            };
                            return Request.CreateResponse(HttpStatusCode.OK, res);
                        }
                        else
                        {
                            res = new customerDetails()
                            {
                                customers = null,
                                Status = false,
                                Message = "Customer not exist."
                            };
                            return Request.CreateResponse(HttpStatusCode.OK, res);
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.Error("Error in addCustomer " + ex.Message);
                        res = new customerDetails()
                        {
                            customers = null,
                            Status = false,
                            Message = "something went wrong."
                        };
                        return Request.CreateResponse(HttpStatusCode.OK, res);
                    }
                }
            }
        }

        [Route("UpdateNotificationReceived")]
        [HttpGet]
        [AllowAnonymous]
        public async Task<bool> UpdateNotificationReceived()
        {
            MongoDbHelper<DataContracts.Mongo.NotificationReceiveMongo> helper = new MongoDbHelper<DataContracts.Mongo.NotificationReceiveMongo>();
            var notifyReceive = helper.Select(x => x.IsUpdated == false).ToList();
            if (notifyReceive != null && notifyReceive.Any())
            {
                using (var context = new AuthContext())
                {
                    context.Database.CommandTimeout = 6000;
                    if (context.Database.Connection.State != ConnectionState.Open)
                        context.Database.Connection.Open();

                    var IdDt = new DataTable();
                    SqlParameter param = null;
                    DbCommand cmd = null;

                    IdDt = new DataTable();
                    IdDt.Columns.Add("CustomerId");
                    IdDt.Columns.Add("WarehouseId");                   
                    foreach (var item in notifyReceive)
                    {
                        var dr = IdDt.NewRow();
                        dr["CustomerId"] = item.CustomerId;
                        dr["WarehouseId"] = item.NotificationId;                        
                        IdDt.Rows.Add(dr);
                    }

                    param = new SqlParameter("Notification", IdDt);
                    param.SqlDbType = SqlDbType.Structured;
                    param.TypeName = "dbo.CustomerIdAndWarehouseId";
                    cmd = context.Database.Connection.CreateCommand();
                    cmd.CommandText = "[dbo].[BulkUpdateNotificationReceived]";
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.Parameters.Add(param);
                    if (cmd.ExecuteNonQuery() > 0)
                    {
                        foreach (var item in notifyReceive)
                        {
                            helper.Delete(item.Id);
                        }
                    }
                }
            }
            return true;
        }

        [Route("NotificationReceived")]
        [HttpGet]
        public async Task<HttpResponseMessage> NotificationReceived(int NotificationId, int customerid)
        {
            MongoDbHelper<DataContracts.Mongo.NotificationReceiveMongo> helper = new MongoDbHelper<DataContracts.Mongo.NotificationReceiveMongo>();
            DataContracts.Mongo.NotificationReceiveMongo obj = new DataContracts.Mongo.NotificationReceiveMongo
            {
                CustomerId = customerid,
                IsUpdated = false,
                NotificationId = NotificationId
            };

            helper.Insert(obj);
            using (var db = new AuthContext())
            {
                var n = await db.NotificationUpdatedDb.Where(c => c.Id == NotificationId).FirstOrDefaultAsync();
                customerDetails res = new customerDetails()
                {
                    customers = n,
                    Status = true,
                    Message = "Success."
                };
                return Request.CreateResponse(HttpStatusCode.OK, res);
            }
            // NotificationUpdated objnot = new NotificationUpdated();
            /*    using (var db = new AuthContext())
                {
                    var n = await db.NotificationUpdatedDb.Where(c => c.Id == NotificationId).FirstOrDefaultAsync();
                    customerDetails res;
                    if (n != null)
                    {
                        int i = await db.Database.ExecuteSqlCommandAsync("Exec UpdateNotificationReceived " + NotificationId + "," + customerid);                   
                        if (i > 0)
                        {
                            res = new customerDetails()
                            {
                                customers = n,
                                Status = true,
                                Message = "Success."
                            };
                            return Request.CreateResponse(HttpStatusCode.OK, res);
                        }
                        else
                        {
                            res = new customerDetails()
                            {
                                customers = null,
                                Status = false,
                                Message = "Notification not exist."
                            };
                            return Request.CreateResponse(HttpStatusCode.OK, res);
                        }
                    }
                    else
                    {

                        res = new customerDetails()
                        {
                            customers = null,
                            Status = false,
                            Message = "Notification not exist."
                        };
                        return Request.CreateResponse(HttpStatusCode.OK, res);
                    }

                }
                */
        }


        [Route("GetNotificationItems")]
        [AcceptVerbs("GET")]
        public async Task<List<DistributorDc>> GetNotificationItems(string notificationType, int typeId, int wareHouseId)
        {
            List<ItemMaster> items = new List<ItemMaster>();
            using (var myContext = new AuthContext())
            {
                {
                    switch (notificationType)
                    {
                        case "Item":
                            items = myContext.itemMasters.Where(x => x.ItemId == typeId && x.active && !x.Deleted && x.WarehouseId == wareHouseId && (x.ItemAppType == 0 || x.ItemAppType == 2) && x.DistributorShow == true).ToList();
                            break;

                        case "Offer":
                            items = myContext.itemMasters.Where(x => x.ItemId == typeId && x.active && !x.Deleted && x.WarehouseId == wareHouseId && (x.ItemAppType == 0 || x.ItemAppType == 2) && x.DistributorShow == true).ToList();
                            break;

                        case "Flash Deal":
                            items = myContext.itemMasters.Where(x => x.ItemId == typeId && x.active && !x.Deleted && x.WarehouseId == wareHouseId && (x.ItemAppType == 0 || x.ItemAppType == 2) && x.DistributorShow == true).ToList();
                            break;

                        case "Brand":
                            items = myContext.itemMasters.Where(x => x.SubsubCategoryid == typeId && x.active && !x.Deleted && x.WarehouseId == wareHouseId && (x.ItemAppType == 0 || x.ItemAppType == 2) && x.DistributorShow == true).ToList();
                            break;
                    }
                }

                var retList = items.Select(a => new DistributorDc
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
                    UnitPrice = a.DistributionPrice,
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
                    FlashDealMaxQtyPersonCanTake = a.OfferMaxQtyPersonCanTake,
                    ItemAppType = a.ItemAppType,
                    DistributorShow = a.DistributorShow,
                    DistributionPrice = a.DistributionPrice
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


        public class customerDetails
        {
            public NotificationUpdated customers { get; set; }
            public bool Status { get; set; }
            public string Message { get; set; }
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
        //public class customers
        //{
        //    public string fcmId { get; set; }
        //    public int CustomerId { get; set; }
        //    public int orderCount { get; set; }
        //    public double orderTotal { get; set; }
        //}

        public class notModel
        {
            public int ItemId { get; set; }
            public int CustomerId { get; set; }
            public int categoryId { get; set; }
            public string fcmId { get; set; }
            public int orderCount { get; set; }
            public double orderTotal { get; set; }
        }

        public class DistributorDc
        {
            public bool active { get; set; }
            public int ItemId { get; set; }
            public int ItemlimitQty { get; set; }
            public bool IsItemLimit { get; set; }
            public string ItemNumber { get; set; }
            public int CompanyId { get; set; }
            public int WarehouseId { get; set; }
            public int DepoId { get; set; }
            public string DepoName { get; set; }
            public int BaseCategoryId { get; set; }
            public int Categoryid { get; set; }
            public int SubCategoryId { get; set; }
            public int SubsubCategoryid { get; set; }
            public string itemname { get; set; }
            public string HindiName { get; set; }
            public double price { get; set; }
            public string SellingUnitName { get; set; }
            public string SellingSku { get; set; }
            public double? UnitPrice { get; set; }
            public double VATTax { get; set; }
            public string LogoUrl { get; set; }
            public int MinOrderQty { get; set; }
            public double Discount { get; set; }
            public double TotalTaxPercentage { get; set; }
            public double? marginPoint { get; set; }
            public int? promoPerItems { get; set; }
            public int? dreamPoint { get; set; }
            public bool IsOffer { get; set; }//
                                             //by sachin (Date 14-05-2019)
                                             //public bool Isoffer { get; set; }
            public bool IsSensitive { get; set; }//sudhir
            public bool IsSensitiveMRP { get; set; }//sudhir
            public string UnitofQuantity { get; set; }//sudhir
            public string UOM { get; set; }//sudhir
            public string itemBaseName { get; set; }
            public bool Deleted { get; set; }
            public double NetPurchasePrice { get; set; }

            public bool IsFlashDealUsed { get; set; }
            public int ItemMultiMRPId { get; set; }
            public int BillLimitQty { get; set; }
            public int? OfferCategory
            {
                get; set;
            }
            public DateTime? OfferStartTime
            {
                get; set;
            }
            public DateTime? OfferEndTime
            {
                get; set;
            }
            public double? OfferQtyAvaiable
            {
                get; set;
            }

            public double? OfferQtyConsumed
            {
                get; set;
            }

            public int? OfferId
            {
                get; set;
            }

            public string OfferType
            {
                get; set;
            }

            public double? OfferWalletPoint
            {
                get; set;
            }
            public int? OfferFreeItemId
            {
                get; set;
            }
            public int? FreeItemId
            {
                get; set;
            }


            public double? OfferPercentage
            {
                get; set;
            }

            public string OfferFreeItemName
            {
                get; set;
            }

            public string OfferFreeItemImage
            {
                get; set;
            }
            public int? OfferFreeItemQuantity
            {
                get; set;
            }
            public int? OfferMinimumQty
            {
                get; set;
            }
            public double? FlashDealSpecialPrice
            {
                get; set;
            }
            public int? FlashDealMaxQtyPersonCanTake
            {
                get; set;
            }

            public double? DistributionPrice { get; set; }
            public bool DistributorShow { get; set; }
            public int ItemAppType { get; set; }
        }
    }

}



