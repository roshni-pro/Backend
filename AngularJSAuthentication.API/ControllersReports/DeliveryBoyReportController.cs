using AngularJSAuthentication.Model;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Web.Http;

namespace AngularJSAuthentication.API.Controllers
{
    [RoutePrefix("api/DeliveryBoyReport")]
    public class DeliveryBoyReportController : ApiController
    {
        public static Logger logger = LogManager.GetCurrentClassLogger();

        [HttpPost]
        public dynamic Post(DBOYinfo DBI)
        {
            logger.Info("start OrderMaster: ");
            List<deliveryboydatalist> MainReport = new List<deliveryboydatalist>();
            DateTime start = DateTime.Parse("01-01-2017 00:00:00");
            DateTime end = DateTime.Today.AddDays(1);
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
                logger.Info("User ID : {0} , Company Id : {1}", compid, userid);

                if (DBI.datefrom == null)
                {
                    DBI.datefrom = DateTime.Parse("01-01-2017 00:00:00");
                    DBI.dateto = DateTime.Today.AddDays(1);
                }
                else
                {
                    start = DBI.datefrom.GetValueOrDefault();
                    end = DBI.dateto.GetValueOrDefault();
                }
                foreach (var i in DBI.ids)
                {
                    deliveryboydatalist oh = new deliveryboydatalist();
                    var olist = getDBoyOrdersHistory(i.mob, DBI.datefrom, DBI.dateto, i.id, compid);
                    oh = olist;
                    MainReport.Add(oh);
                }
                return MainReport;
            }
            catch (Exception ex)
            {
                logger.Error("Error in OrderMaster " + ex.Message);
                logger.Info("End  OrderMaster: ");
                return null;
            }
        }

        [Route("month")]
        [HttpGet]
        public dynamic GetMonth(DateTime? datefrom, DateTime? dateto, int type, string ids)
        {
            logger.Info("start OrderMaster: ");
            List<MainReports> MainReport = new List<MainReports>();
            DateTime start = DateTime.Parse("01-01-2017 00:00:00");
            DateTime end = DateTime.Today.AddDays(1);
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
                logger.Info("User ID : {0} , Company Id : {1}", compid, userid);

                if (datefrom == null)
                {
                    datefrom = DateTime.Parse("01-01-2017 00:00:00");
                    dateto = DateTime.Today.AddDays(1);
                }
                else
                {
                    start = datefrom.GetValueOrDefault();
                    end = dateto.GetValueOrDefault();
                }
                var array = ids.Split(',');
                foreach (var iidd in array)
                {
                    int id = Convert.ToInt32(iidd);

                }
                return MainReport;
            }
            catch (Exception ex)
            {
                logger.Error("Error in OrderMaster " + ex.Message);
                logger.Info("End  OrderMaster: ");
                return null;
            }
        }

        [Route("year")]
        [HttpGet]
        public dynamic GetYear(DateTime? datefrom, DateTime? dateto, int type, string ids)
        {
            logger.Info("start OrderMaster: ");
            List<MainReports> MainReport = new List<MainReports>();
            DateTime start = DateTime.Parse("01-01-2017 00:00:00");
            DateTime end = DateTime.Today.AddDays(1);
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
                logger.Info("User ID : {0} , Company Id : {1}", compid, userid);

                if (datefrom == null)
                {
                    datefrom = DateTime.Parse("01-01-2017 00:00:00");
                    dateto = DateTime.Today.AddDays(1);
                }
                else
                {
                    start = datefrom.GetValueOrDefault();
                    end = dateto.GetValueOrDefault();
                }
                var array = ids.Split(',');
                foreach (var iidd in array)
                {
                    int id = Convert.ToInt32(iidd);

                }
                return MainReport;
            }
            catch (Exception ex)
            {
                logger.Error("Error in OrderMaster " + ex.Message);
                logger.Info("End  OrderMaster: ");
                return null;
            }
        }

        public class DBOYinfo
        {
            public List<dbinf> ids { get; set; }
            public DateTime? datefrom { get; set; }
            public DateTime? dateto { get; set; }
        }
        public class dbinf
        {
            public int id { get; set; }
            public string mob { get; set; }
        }

        public class dboydata
        {
            public List<deliveryboydatalist> reports { get; set; }
        }

        public deliveryboydatalist getDBoyOrdersHistory(string mob, DateTime? start, DateTime? end, int dboyId, int compid)
        {
            using (var db = new AuthContext())
            {
                try
                {
                    deliveryboydatalist orderhistory = new deliveryboydatalist();
                    orderhistory.totalOrder = 0;
                    orderhistory.AllOrderAmount = 0;
                    orderhistory.DeliveredOrderAmount = 0;
                    orderhistory.Canceled = 0;
                    orderhistory.CanceledOderAmount = 0;
                    orderhistory.RedispatchedOrderAmount = 0;
                    orderhistory.Redispatched = 0;
                    orderhistory.Delivered = 0;
                    orderhistory.Pending = 0;
                    orderhistory.PendingOderAmount = 0;

                    //var Issulist1 = db.DeliveryIssuanceDb.Where(x => x.CreatedDate > start && x.CreatedDate <= end && x.PeopleID == dboyId && x.CompanyId == compid).ToList();
                    //if (Issulist != null)
                    //{
                    //    orderhistory.name = Issulist[0].DisplayName;
                    //    orderhistory.DBoyId = Issulist[0].PeopleID;
                    //}

                    var People = db.Peoples.Where(z => z.PeopleID == dboyId).FirstOrDefault();
                    orderhistory.name = People.DisplayName;
                    orderhistory.DBoyId = People.PeopleID;
                    var Issulists = db.OrderDispatchedMasters.Where(x => x.OrderedDate > start && x.OrderedDate <= end && x.DboyMobileNo == People.Mobile && x.CompanyId == compid).ToList();


                    //foreach (var o in Issulist)
                    //{
                    //    if (o.Status == "Accepted")
                    //    {
                    var orderhistory1 = new OrderHistory();
                    List<OrderDispatchedMaster> OrdersObj = new List<OrderDispatchedMaster>();

                    //string[] ids = o.OrderdispatchIds.Split(',');

                    foreach (var od in Issulists)
                    {
                        //var oid = Convert.ToInt32(od);

                        var orderdipatchmaster = db.OrderDispatchedMasters.Where(x => x.OrderId == od.OrderId && x.CompanyId == compid && x.WarehouseId == od.WarehouseId).SingleOrDefault();
                        if (orderdipatchmaster != null)
                        {
                            if ((orderdipatchmaster.Status == "Delivery Canceled" || orderdipatchmaster.Status == "Order Canceled" || orderdipatchmaster.Status == "Post Order Canceled"))
                            {
                                orderhistory.Canceled = orderhistory.Canceled + 1;
                                orderhistory.CanceledOderAmount = orderhistory.CanceledOderAmount + orderdipatchmaster.GrossAmount;
                            }
                            else if ((orderdipatchmaster.Status == "Delivered" || orderdipatchmaster.Status == "sattled" || orderdipatchmaster.Status == "Account settled" || orderdipatchmaster.Status == "Partial settled" || orderdipatchmaster.Status == "Partial receiving -Bounce"))
                            {
                                orderhistory.Delivered = orderhistory.Delivered + 1;
                                orderhistory.DeliveredOrderAmount = orderhistory.DeliveredOrderAmount + orderdipatchmaster.GrossAmount;
                            }
                            if ((orderdipatchmaster.Status == "Ready to Dispatch" || orderdipatchmaster.Status == "Delivery Redispatch") && (orderdipatchmaster.ReDispatchCount > 0))
                            {
                                orderhistory.Redispatched = orderhistory.Redispatched + 1;
                                orderhistory.RedispatchedOrderAmount = orderhistory.RedispatchedOrderAmount + orderdipatchmaster.GrossAmount;
                            }
                            if ((orderdipatchmaster.Status == "Ready to Dispatch"))
                            {
                                orderhistory.ReadyToDispatch = orderhistory.ReadyToDispatch + 1;
                                orderhistory.ReadyToDispatchOderAmount = orderhistory.ReadyToDispatchOderAmount + orderdipatchmaster.GrossAmount;
                            }
                            if ((orderdipatchmaster.Status == "Pending" || orderdipatchmaster.Status == "Shipped" || orderdipatchmaster.Status == "Issued"))
                            {
                                orderhistory.Pending = orderhistory.Pending + 1;
                                orderhistory.PendingOderAmount = orderhistory.PendingOderAmount + orderdipatchmaster.GrossAmount;
                            }
                            orderhistory.AllOrderAmount = orderhistory.AllOrderAmount + orderdipatchmaster.GrossAmount;
                            orderhistory.totalOrder = orderhistory.totalOrder + 1;
                        }
                    }
                    //    }
                    //}
                    return orderhistory;
                }
                catch (Exception ex)
                {
                    return null;
                }
            }
        }
        public class deliveryboydatalist
        {
            public double DeliveredOrderAmount { get; set; }
            public double AllOrderAmount { get; set; }
            public double RedispatchedOrderAmount { get; set; }
            public double CanceledOderAmount { get; set; }
            public double PendingOderAmount { get; set; }
            public double ReadyToDispatchOderAmount { get; set; }

            public string name { get; set; }

            public int Pending { get; set; }
            public int ReadyToDispatch { get; set; }
            public int Delivered { get; set; }
            public int DBoyId { get; set; }

            public int Redispatched { get; set; }
            public int Canceled { get; set; }
            public int totalOrder { get; set; }

        }



    }
    public class DBOYinfo
    {
        public List<dbinf> ids { get; set; }
        public DateTime? datefrom { get; set; }
        public DateTime? dateto { get; set; }
    }
    public class dbinf
    {
        public int id { get; set; }
        public string mob { get; set; }
    }

}
