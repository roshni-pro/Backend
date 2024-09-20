using AngularJSAuthentication.API.Helper;
using AngularJSAuthentication.Model;
using NLog;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Transactions;
using System.Web.Http;
using System.Web.Http.Description;

namespace AngularJSAuthentication.API.Controllers
{
    [RoutePrefix("api/OrderDispatchedMasterFinal")]
    public class OrderDispatchedMasterFinalController : ApiController
    {

        public static Logger logger = LogManager.GetCurrentClassLogger();
        public static readonly object LockObject = new object();


        [Route("")]
        public FinalOrderDispatchedMaster Get(string id)
        {
            logger.Info("start PurchaseOrderMaster: ");
            using (var db = new AuthContext())
            {
                try
                {
                    var identity = User.Identity as ClaimsIdentity;
                    int compid = 0, userid = 0;
                    // Access claims
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


                    FinalOrderDispatchedMaster wh = new FinalOrderDispatchedMaster();

                    if (id != null)
                    {
                        int Id = Convert.ToInt32(id);

                        wh = db.FinalOrderDispatchedMasterDb.Where(x => x.OrderId == Id && x.CompanyId == compid).SingleOrDefault();
                    }
                    return wh;
                }
                catch (Exception ex)
                {
                    logger.Error("Error in returnorderby id " + ex.Message);
                    logger.Info("End  returnorderby id: ");
                    return null;
                }

            }
        }

        [ResponseType(typeof(FinalOrderDispatchedMaster))]
        [Route("")]
        [AcceptVerbs("POST")]
        public FinalOrderDispatchedMaster add(FinalOrderDispatchedMaster item)
        {
            logger.Info("start FinalOrderDispatchedMaster: ");
            lock (LockObject)
            {
                using (var context = new AuthContext())
                {
                    using (var dbContextTransaction = context.Database.BeginTransaction())
                    {
                        try
                        {
                            var identity = User.Identity as ClaimsIdentity;
                            int compid = 0, userid = 0;
                            string Username = null;
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
                                if (claim.Type == "username")
                                {
                                    Username = (claim.Value);
                                }
                            }
                            item.CompanyId = compid;
                            item.userid = userid;
                            if (item == null)
                            {
                                throw new ArgumentNullException("item");
                            }
                            logger.Info("User ID : {0} , Company Id : {1}", compid, userid);
                            // for ordermaster master status change
                            int OrderId = item.OrderId;
                            var UserName = context.Peoples.Where(x => x.PeopleID == userid).SingleOrDefault();
                            if (OrderId > 0 && UserName != null)
                            {
                                OrderDispatchedMaster ox = context.OrderDispatchedMasters.Where(x => x.OrderId == OrderId && x.Deleted == false).Include("orderDetails").FirstOrDefault();
                                if (ox.Status != "sattled" && ox.Status == "Delivered")
                                {

                                    // order dispatched master status change
                                    OrderMaster om = context.DbOrderMaster.Where(x => x.OrderId == OrderId && x.Deleted == false).Include("orderDetails").FirstOrDefault();
                                    om.Status = "sattled";
                                    om.UpdatedDate = DateTime.Now;
                                    context.Entry(om).State = EntityState.Modified;

                                    ox.Status = "sattled";
                                    ox.DiscountAmount = item.DiscountAmount;
                                    ox.RecivedAmount = item.RecivedAmount;
                                    ox.Reason = item.Reason;
                                    ox.UpdatedDate = DateTime.Now;
                                    //OrderDispatchedMasters.Attach(ox);
                                    context.Entry(ox).State = EntityState.Modified;

                                    //new code for non revenue orders
                                    #region For Non Revenue Orders
                                    if (om.OrderType == 10)
                                    {
                                        DeliveryIssuance delivryIssue = context.DeliveryIssuanceDb.Where(x => x.OrderIds.Contains(OrderId.ToString())).FirstOrDefault();
                                        delivryIssue.Status = "Freezed";
                                        context.Entry(delivryIssue).State = EntityState.Modified;

                                        DamageOrderMaster dm = context.DamageOrderMasterDB.Where(x => x.invoice_no == item.invoice_no && x.Status=="Delivered" && x.active==true && x.OrderTypes== 2 ).FirstOrDefault();
                                        dm.Status = "sattled";
                                        dm.UpdatedDate = DateTime.Now;
                                        context.Entry(dm).State = EntityState.Modified; 
                                    }
                                    #endregion

                                    #region Order Master History
                                    OrderMasterHistories h1 = new OrderMasterHistories();
                                    h1.orderid = item.OrderId;
                                    h1.Status = om.Status;
                                    h1.Reasoncancel = "Order Settle";
                                    h1.Warehousename = item.WarehouseName;
                                    h1.userid = UserName.PeopleID;
                                    h1.username = UserName.DisplayName;
                                    h1.CreatedDate = DateTime.Now;
                                    context.OrderMasterHistoriesDB.Add(h1);
                                    #endregion
                                    if (item != null)
                                    {
                                        item.UpdatedDate = DateTime.Now;
                                        item.Status = "sattled";
                                        context.Entry(item).State = EntityState.Added;
                                        if (context.Commit() > 0)
                                        {
                                            dbContextTransaction.Commit();
                                            CustomerLedgerHelperAuto helper = new CustomerLedgerHelperAuto();
                                            helper.OnSettle(item.OrderId, item.CustomerId, UserName.PeopleID);
                                        }
                                        else { dbContextTransaction.Rollback(); }
                                        return item;
                                    }
                                }
                            }
                            //context.AddFinalOrderDispatchedMaster(item);
                            logger.Info("End  UnitMaster: ");
                            return item;
                        }
                        catch (Exception ex)
                        {
                            dbContextTransaction.Rollback();
                            logger.Error("Error in Final OrderDispatchedMaster " + ex.Message);
                            logger.Info("End  Final OrderDispatchedMaster: ");
                            return null;
                        }
                    }
                }
            }
        }


        //[ResponseType(typeof(OrderDispatchedMaster))]
        //[Route("")]
        //[AcceptVerbs("PUT")]
        //public OrderDispatchedMaster put(int id,string DboyNo)
        //{
        //    var db = new AuthContext();
        //    logger.Info("start OrderDispatchedMaster: ");
        //    OrderDispatchedMaster obj = db.OrderDispatchedMasters.Where(x => x.OrderDispatchedMasterId == id).FirstOrDefault();
        //    People DeliveryOBJ = db.Peoples.Where(x => x.Mobile == DboyNo).FirstOrDefault();

        //    obj.DboyName = DeliveryOBJ.DisplayName;
        //    obj.DboyMobileNo = DeliveryOBJ.Mobile;


        //    try
        //    {
        //        var identity = User.Identity as ClaimsIdentity;
        //        int compid = 0, userid = 0;

        //        foreach (Claim claim in identity.Claims)
        //        {
        //            if (claim.Type == "compid")
        //            {
        //                compid = int.Parse(claim.Value);
        //            }
        //            if (claim.Type == "userid")
        //            {
        //                userid = int.Parse(claim.Value);
        //            }
        //        }
        //       // item.CompanyId = compid;
        //        if (obj == null)
        //        {
        //            throw new ArgumentNullException("item");
        //        }
        //        logger.Info("User ID : {0} , Company Id : {1}", compid, userid);
        //        context.UpdateOrderDispatchedMaster(obj);
        //        logger.Info("End  UnitMaster: ");
        //        return obj;
        //    }
        //    catch (Exception ex)
        //    {
        //        logger.Error("Error in OrderDispatchedMaster " + ex.Message);
        //        logger.Info("End  OrderDispatchedMaster: ");
        //        return null;
        //    }
        //}

        // For Multi Select 
        // For Multi Select 
        [Route("Multisettle")]
        [HttpPost, HttpPut]
        public HttpResponseMessage PostMultiSettle(FinalOrderDispatchedMaster obj)//add issuance
        {
            lock (LockObject)
            {
                //using (var dbContextTransaction = new TransactionScope())
                //{
                using (var context = new AuthContext())
                {

                    try
                    {

                        var identity = User.Identity as ClaimsIdentity;
                        int compid = 0, userid = 0;
                        // Access claims
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
                        obj.AssignedOrders[0].userid = userid;
                        obj.AssignedOrders[0].CompanyId = compid;
                        if (obj != null)
                        {
                            var UserName = context.Peoples.Where(x => x.PeopleID == userid).SingleOrDefault();
                            if (UserName != null)
                            {
                                List<int> orderIds = obj.AssignedOrders.Select(x => x.OrderId).ToList();
                                List<OrderDispatchedMaster> OrderDispatchedMasters = context.OrderDispatchedMasters.Where(x => orderIds.Contains(x.OrderId) && x.Deleted == false).ToList();
                                List<OrderMaster> OrderMasters = context.DbOrderMaster.Where(x => orderIds.Contains(x.OrderId) && x.Deleted == false).ToList();
                                List<FinalOrderDispatchedMaster> AssignedOrders = new List<FinalOrderDispatchedMaster>();
                                foreach (var item in obj.AssignedOrders)
                                {
                                    int OrderId = item.OrderId;
                                    var ox = OrderDispatchedMasters.FirstOrDefault(x => x.OrderId == OrderId);
                                    if (ox.Status != "sattled" && ox.Status == "Delivered")
                                    {
                                        // order dispatched master status change
                                        var om = OrderMasters.FirstOrDefault(x => x.OrderId == OrderId);
                                        om.Status = "sattled";
                                        om.UpdatedDate = DateTime.Now;
                                        context.Entry(om).State = EntityState.Modified;

                                        ox.Status = "sattled";
                                        ox.DiscountAmount = item.DiscountAmount;
                                        ox.RecivedAmount = item.RecivedAmount;
                                        ox.Reason = item.Reason;
                                        ox.UpdatedDate = DateTime.Now;
                                        //OrderDispatchedMasters.Attach(ox);
                                        context.Entry(ox).State = EntityState.Modified;
                                        #region Order Master History
                                        OrderMasterHistories h1 = new OrderMasterHistories();
                                        h1.orderid = item.OrderId;
                                        h1.Status = om.Status;
                                        h1.Reasoncancel = "Order Settle";
                                        h1.Warehousename = item.WarehouseName;
                                        h1.userid = UserName.PeopleID;
                                        h1.username = UserName.DisplayName;
                                        h1.CreatedDate = DateTime.Now;
                                        context.OrderMasterHistoriesDB.Add(h1);
                                        #endregion


                                        //new code for non revenue orders
                                        #region For Non Revenue Orders
                                        if (om.OrderType == 10)
                                        {
                                            DeliveryIssuance delivryIssue = context.DeliveryIssuanceDb.Where(x => x.OrderIds.Contains(OrderId.ToString())).FirstOrDefault();
                                            delivryIssue.Status = "Freezed";
                                            context.Entry(delivryIssue).State = EntityState.Modified;

                                            DamageOrderMaster dm = context.DamageOrderMasterDB.Where(x => x.invoice_no == item.invoice_no && x.Status == "Delivered" && x.active == true && x.OrderTypes == 2).FirstOrDefault();
                                            dm.Status = "sattled";
                                            dm.UpdatedDate = DateTime.Now;
                                            context.Entry(dm).State = EntityState.Modified;
                                        }
                                        #endregion

                                        item.UpdatedDate = DateTime.Now;
                                        item.Status = "sattled";
                                        context.Entry(item).State = EntityState.Added;
                                        AssignedOrders.Add(item);
                                    }
                                }

                                if (AssignedOrders.Any())
                                {
                                    if (context.Commit() > 0)
                                    {
                                        foreach (var item in AssignedOrders)
                                        {
                                            CustomerLedgerHelperAuto helper = new CustomerLedgerHelperAuto();
                                            helper.OnSettle(item.OrderId, item.CustomerId, UserName.PeopleID);
                                        }
                                    }
                                }
                            }
                            ///dbContextTransaction.Complete();
                        }
                        //db.DBMultiSettle(obj.AssignedOrders);

                        return Request.CreateResponse(HttpStatusCode.OK, true);
                    }
                    catch (Exception ex)
                    {
                        //dbContextTransaction.Dispose();
                        string error = ex.InnerException != null ? ex.ToString() + Environment.NewLine + ex.InnerException.ToString() : ex.ToString();
                        logger.Error("Error in Multisettle: " + error);
                        return Request.CreateResponse(HttpStatusCode.BadRequest, ex.Message);
                    }
                }
                //}

            }
        }
    }
}