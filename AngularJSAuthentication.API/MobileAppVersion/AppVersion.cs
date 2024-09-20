using AngularJSAuthentication.API.Helpers;
using AngularJSAuthentication.Model;
using LinqKit;
using NLog;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Http;

namespace AngularJSAuthentication.API.Controllers
{
    [RoutePrefix("api/appVersion")]
    public class AppVersionController : ApiController
    {
       
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private static TimeZoneInfo INDIAN_ZONE = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
        DateTime indianTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE);
        [HttpGet]
        [Route("")]
        [AcceptVerbs("GET")]
        public HttpResponseMessage Get()
        {
            try
            {
                using (AuthContext db = new AuthContext())
                {
                    var item = db.appVersionDb.Where(x => x.isCompulsory).ToList();
                    return Request.CreateResponse(HttpStatusCode.OK, item);
                }
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex.Message);
            }
        }

        [HttpPost]
        [Route("")]
        [AcceptVerbs("POST")]
        public HttpResponseMessage add(appVersion item)
        {
            logger.Info("start add Feedback: ");
            try
            {
                using (AuthContext db = new AuthContext())
                {
                    item.createdDate = indianTime;
                    db.appVersionDb.Add(item);
                    db.Commit();
                    logger.Info("End add feedBack: ");
                    return Request.CreateResponse(HttpStatusCode.OK, "requesting brand added suscessfully");
                }
            }
            catch (Exception ex)
            {
                logger.Error("Error in add feedBack " + ex.Message);
                logger.Info("End  addCity: ");
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Error:" + ex.Message);
            }
        }
        //#region sales app data get
        //[HttpGet]
        //[Route("salesdata")]
        //[AcceptVerbs("GET")]
        //public HttpResponseMessage salesdataGet()
        //{
        //    try
        //    {
        //        var item = db.SalesappVersionDB.AsEnumerable();
        //        return Request.CreateResponse(HttpStatusCode.OK, item);
        //    }
        //    catch (Exception ex)
        //    {
        //        return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex.Message);
        //    }
        //}
        //#endregion



        //[HttpGet]
        //[Route("salesdata")]
        //[AllowAnonymous]
        //public SalesappVersion salesdataGet()
        //{
        //    using (AuthContext context = new AuthContext())
        //    {
        //        var item = context.SalesappVersionDB.Where(x => x.isCompulsory==true).FirstOrDefault();
        //        return item;

        //    }
            

        //}
       
        [Route("salesdata")]
        [HttpGet]
        public HttpResponseMessage salesdata()
        {
           List<SalesappVersion>salesapp = new List<SalesappVersion>();

            using (AuthContext context = new AuthContext())
            {

                salesapp = context.SalesappVersionDB.Where(x => x.isCompulsory == true).ToList();
            }

            
            return Request.CreateResponse(HttpStatusCode.OK, salesapp);

        }

        [Route("customerTrackingAppVersionData")]
        [HttpGet]
        public HttpResponseMessage customerTrackingAppVersionData()
        {
            List<CustomerTrackingAppVersion> customerTrackingApp = new List<CustomerTrackingAppVersion>();

            using (AuthContext context = new AuthContext())
            {

                customerTrackingApp = context.CustomerTrackingAppVersionDB.Where(x => x.isCompulsory == true).ToList();

            }


            return Request.CreateResponse(HttpStatusCode.OK, customerTrackingApp);

        }


        #region DeliveryAppVersion 
        [HttpGet]
        [Route("DeliveryApp")]
        [AcceptVerbs("GET")]
        public HttpResponseMessage DeliveryAppdataGet()
        {
            try
            {
                using (AuthContext db = new AuthContext())
                {
                    var item = db.DeliveryAppVersionDB.Where(x => x.Active == true).ToList();

                    return Request.CreateResponse(HttpStatusCode.OK, item);
                }
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex.Message);
            }
        }
        #endregion
        #region NewDeliveryAppVersion 
        [HttpGet]
        [Route("NewDeliveryApp")]
        [AcceptVerbs("GET")]
        public HttpResponseMessage NewDeliveryAppdataGet()
        {
            try
            {
                using (AuthContext db = new AuthContext())
                {
                    var item = db.NewDeliveryAppVersionDB.Where(x => x.Active == true).ToList();

                    return Request.CreateResponse(HttpStatusCode.OK, item);
                }
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex.Message);
            }
        }
        #endregion


        #region SupplierAppVersion
        [HttpGet]
        [Route("SupplierApp")]
        [AcceptVerbs("GET")]
        public HttpResponseMessage SupplierAppdataGet()
        {
            try
            {
                using (AuthContext db = new AuthContext())
                {
                    var item = db.SupplierAppVersionDB.Where(x => x.Active == true).FirstOrDefault();
                    return Request.CreateResponse(HttpStatusCode.OK, item);
                }
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex.Message);
            }
        }
        #endregion


        #region
        /// <summary>
        /// Wudu App version API by Anushka(16-01-2020)
        /// </summary>
        /// <returns></returns>
        [Route("WuduAppVersion")]
        [HttpGet]
        public HttpResponseMessage WuduAppVersion()
        {
            try
            { 

                 using (AuthContext db = new AuthContext())
            {
                var wudu = db.WuduAppVersionDB.Where(x => x.Active == true).AsEnumerable();
                return Request.CreateResponse(HttpStatusCode.OK, wudu);
            }
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex.Message);
            }
        }
        #endregion






        #region SupplierAppVersion
        [HttpGet]
        [Route("TradeApp")]
        [AcceptVerbs("GET")]
        public HttpResponseMessage TradeAppdataGet()
        {
            try
            {
                using (AuthContext db = new AuthContext())
                {
                    var item = db.TradeAppVersionDB.Where(x => x.Active == true).AsEnumerable();
                    return Request.CreateResponse(HttpStatusCode.OK, item);
                }
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex.Message);
            }
        }
        #endregion

        #region
        /// <summary>
        /// Distributor App version API by Vinayak(06-03-2020)
        /// </summary>
        /// <returns></returns>
        [Route("DistributorAppVersion")]
        [HttpGet]
        public HttpResponseMessage DistributorAppVersion()
        {
            MongoDbHelper<DistributorAppVersion> mongoDbHelper = new MongoDbHelper<DistributorAppVersion>();
            var DistPredicate = PredicateBuilder.New<DistributorAppVersion>(x => x.isCompulsory && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value));
            DistributorAppVersion lastdata = mongoDbHelper.Select(DistPredicate).FirstOrDefault();
            return Request.CreateResponse(HttpStatusCode.OK, lastdata);
        }
        #endregion

        [HttpPost]
        [Route("fcm")]
        [AcceptVerbs("POST")]
        public HttpResponseMessage post(Customer cust)
        {
            logger.Info("start add Feedback: ");
            try
            {
                using (AuthContext db = new AuthContext())
                {
                    Customer customer = db.Customers.Where(x => x.CustomerId == cust.CustomerId).SingleOrDefault();
                    if (customer != null)
                    {
                        customer.fcmId = cust.fcmId;
                        //db.Customers.Attach(customer);
                        db.Entry(customer).State = EntityState.Modified;
                        db.Commit();
                        logger.Info("End add feedBack: ");
                        appVersion app = db.appVersionDb.OrderByDescending(e => e.id).FirstOrDefault();
                        return Request.CreateResponse(HttpStatusCode.OK, app);
                    }
                    else
                        return Request.CreateResponse(HttpStatusCode.OK, "request not add  ");
                }
            }
            catch (Exception ex)
            {
                logger.Error("Error in add feedBack " + ex.Message);
                logger.Info("End  addCity: ");
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Error:" + ex.Message);
            }
        }
    }











    public class appVersion
    {
        [Key]
        public int id { get; set; }
        //public double App_version { get; set; }
        public string App_version { get; set; }
        public bool isCompulsory { get; set; }
        public DateTime createdDate { get; set; }
    }

    public class ConsumerappVersion
    {
        [Key]
        public int id { get; set; }        
        public string App_version { get; set; }
        public bool IsActive { get; set; }
        public DateTime createdDate { get; set; }       
        public string SplashUrl { get; set; }
    }
}