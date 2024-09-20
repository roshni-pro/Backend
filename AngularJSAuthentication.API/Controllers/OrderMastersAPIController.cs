using AngularJSAuthentication.API.App_Code.FinBox;
using AngularJSAuthentication.API.Controllers.Base;
using AngularJSAuthentication.API.Helper;
using AngularJSAuthentication.API.Helper.Notification;
using AngularJSAuthentication.API.Helper.Razorpay;
using AngularJSAuthentication.API.Helpers;
using AngularJSAuthentication.API.Managers;
using AngularJSAuthentication.Common.Constants;
using AngularJSAuthentication.Common.Helpers;
using AngularJSAuthentication.DataContracts.Masters;
using AngularJSAuthentication.DataContracts.Mongo;
using AngularJSAuthentication.DataContracts.RazorPay;
using AngularJSAuthentication.Model;
using AngularJSAuthentication.Model.BillDiscount;
using AngularJSAuthentication.Model.ComboItem;
using AngularJSAuthentication.Model.CustomerShoppingCart;
using BarcodeLib;
using GenricEcommers.Models;
using LinqKit;
using MongoDB.Bson;
using Newtonsoft.Json;
using Nito.AspNetBackgroundTasks;
using Nito.AsyncEx;
using NLog;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Mail;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.Description;
using static AngularJSAuthentication.DataContracts.External.GamificationDc;

namespace AngularJSAuthentication.API.Controllers
{
    [RoutePrefix("api/OrderMastersAPI")]
    public class OrderMastersAPIController : BaseAuthController
    {

        public static Logger logger = LogManager.GetCurrentClassLogger();
        private static TimeZoneInfo INDIAN_ZONE = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
        DateTime indianTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE);
        public double xPointValue = AppConstants.xPoint;
        public static List<int> doncustomerIds = new List<int>();
        public static List<int> PayNowncustomerIds = new List<int>();
        public static List<int> retailercustomerIds = new List<int>();
        public static List<string> paymentInprocess = new List<string>();


        [ResponseType(typeof(CustomerRegistration))]
        [Route("")]
        [AcceptVerbs("PUT")]
        public CustomerRegistration Put(CustomerRegistration cust)
        {
            using (var context = new AuthContext())
            {
                try
                {
                    //var context = new AuthContext(new AuthContext());
                    return context.PutCustomerRegistration(cust);
                }
                catch (Exception ex)
                {
                    return null;
                }
            }
        }

        #region for post order of dream item from app (RedeemItem)

        [Route("dreamitem")]
        [HttpPost]
        [AllowAnonymous]
        public HttpResponseMessage Post(DreamOrder sc)
        {
            using (var context = new AuthContext())
            {
                try
                {
                    Customer cust = new Customer();
                    cust = context.Customers.Where(x => (x.CustomerId == sc.CustomerId || x.Skcode == sc.Skcode) && x.CustomerId == sc.CustomerId).SingleOrDefault();
                    if (cust != null)
                    {
                        Wallet wlt = context.WalletDb.Where(c => c.CustomerId == cust.CustomerId).SingleOrDefault();
                        if (wlt.TotalAmount >= sc.WalletPoint)
                        {
                            sc.WarehouseId = cust.Warehouseid ?? 0;
                            sc.WarehouseName = cust.WarehouseName;
                            sc.CityId = cust.Cityid;
                            sc.ShopName = cust.ShopName;
                            sc.Status = "Pending";
                            sc.CustomerMobile = cust.Mobile;
                            sc.ShippingAddress = cust.ShippingAddress;
                            //sc.SalesPersonId = cust.ExecutiveId;
                            sc.CompanyId = cust.CompanyId;

                            sc.CreatedDate = indianTime;
                            sc.UpdatedDate = indianTime;
                            sc.Deliverydate = indianTime.AddDays(2);
                            sc.Deleted = false;
                            sc.ReDispatchCount = 0;
                            foreach (var a in sc.DreamItemDetails)
                            {
                                if (a.ItemId > 0)
                                {
                                    RewardItems it = context.RewardItemsDb.Where(x => x.rItemId == a.ItemId).SingleOrDefault();
                                    if (cust != null)
                                    {
                                        a.ShopName = cust.ShopName;
                                        a.Skcode = cust.Skcode;
                                        a.ItemName = it.rItem;
                                        a.Discription = it.Description;
                                        a.Status = "Pending";
                                        a.CreatedDate = indianTime;
                                        a.UpdatedDate = indianTime;
                                        a.Deleted = false;
                                        SendNotificationForReedem(cust.Skcode, it.rItem, indianTime);
                                    }
                                }
                            }
                            context.DreamOrderDb.Add(sc);


                            if (wlt != null)
                            {
                                //if (sc.WalletPoint > 0)
                                if (wlt != null && wlt.TotalAmount > 0 && wlt.TotalAmount >= sc.WalletPoint)
                                {
                                    CustomerWalletHistory CWH = new CustomerWalletHistory();

                                    CWH.WarehouseId = cust.Warehouseid ?? 0;
                                    CWH.CompanyId = cust.CompanyId ?? 0;
                                    CWH.CustomerId = wlt.CustomerId;
                                    CWH.NewOutWAmount = sc.WalletPoint;
                                    CWH.Through = "From Redeem Order";
                                    CWH.OrderId = sc.Order_Id;
                                    CWH.TotalWalletAmount = wlt.TotalAmount - sc.WalletPoint;
                                    CWH.CreatedDate = indianTime;
                                    CWH.UpdatedDate = indianTime;
                                    context.CustomerWalletHistoryDb.Add(CWH);

                                    wlt.TotalAmount -= sc.WalletPoint;
                                    wlt.TransactionDate = indianTime;

                                    context.WalletDb.Attach(wlt);
                                    context.Entry(wlt).State = EntityState.Modified;

                                    var rpoint = context.RewardPointDb.Where(c => c.CustomerId == cust.CustomerId).SingleOrDefault();
                                    if (rpoint != null)
                                    {
                                        rpoint.UsedPoint += sc.WalletPoint;
                                        rpoint.UpdatedDate = indianTime;
                                        context.RewardPointDb.Attach(rpoint);
                                        context.Entry(rpoint).State = EntityState.Modified;

                                    }
                                    context.Commit();
                                }

                            }
                        }

                    }

                    return Request.CreateResponse(HttpStatusCode.OK, sc);
                }

                catch (Exception ex)
                {
                    logger.Error(ex.Message);
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex.Message);
                }
            }
        }



        public static void SendNotificationForReedem(string skcode, string ItemName, DateTime delDate)
        {
            try
            {
                string masteremail = ConfigurationManager.AppSettings["MasterEmail"];
                string masterpassword = ConfigurationManager.AppSettings["MasterPassword"];
                //string body = "<div style='background: #FAFAFA; color: #333333; padding-left: 30px;font-family: arial,sans-serif; font-size: 14px;'>";
                ////body += "<img style='padding-top: 10px;' src='http://shopkirana.com/wp-content/uploads/2015/07/ShopKirana-Logo11.png'><br/>";
                //body += "<h3 style='background-color: rgb(241, 89, 34);Redeem Gift,</h3> ";
                //body += "Hello,";
                //body += "<p>SKCode:" + skcode + " </p>";
                //body += "<p>has ordered:" + ItemName + "(Gift)</p>";
                //body += "</div>";
                var Subj = "Alert! " + skcode + "   posted for redeem ";
                var msg = new MailMessage("donotreply_backend@shopkirana.com", "donotreply_backend@shopkirana.com", Subj, "<b>Redeem Gift<b> <br/>" +
                    "'" + skcode + "' Has Ordered '" + ItemName + "' On Date '" + delDate + "' Delivery Deadline Is '" + delDate.AddDays(7) + "' ");
                msg.To.Add("customerdelight.nitesh@shopkirana.com");
                msg.To.Add("info@shopkirana.com");
                msg.IsBodyHtml = true;
                var smtpClient = new SmtpClient("smtp.gmail.com", 587);
                smtpClient.UseDefaultCredentials = true;
                smtpClient.Credentials = new NetworkCredential(masteremail, masterpassword);
                smtpClient.EnableSsl = true;
                smtpClient.Send(msg);

            }
            catch (Exception ss) { }


        }

        #endregion

        //by neha comment Box 19/08/19
        [Route("CommentBox")]
        [AcceptVerbs("Put")]
        public HttpResponseMessage AddComment(DreamOrder sc)
        {
            using (var context = new AuthContext())
            {
                try
                {
                    var od = context.DreamOrderDb.Where(x => x.Order_Id == sc.Order_Id).Include("DreamItemDetails").SingleOrDefault();


                    od.comments = sc.comments;
                    od.comments2 = sc.comments2;



                    od.UpdatedDate = indianTime;

                    context.DreamOrderDb.Attach(od);
                    context.Entry(od).State = EntityState.Modified;
                    context.Commit();


                    return Request.CreateResponse(HttpStatusCode.OK, od);
                }

                catch (Exception ex)
                {
                    logger.Error(ex.Message);
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex.Message);
                }
            }
        }


        #region  dispatched process of order of dream item
        [Route("dreamitem")]
        [AcceptVerbs("PUT")]
        public HttpResponseMessage Put(DreamOrder sc)
        {
            using (var context = new AuthContext())
            {
                try
                {
                    var od = context.DreamOrderDb.Where(x => x.Order_Id == sc.Order_Id).Include("DreamItemDetails").SingleOrDefault();
                    if (od != null)
                    {
                        od.Status = sc.Status;
                        if (sc.Status == "Dispatched")
                        {
                            od.DboyName = sc.DboyName;
                            od.DboyMobileNo = sc.DboyMobileNo;
                        }
                        od.UpdatedDate = indianTime;

                        //context.DreamOrderDb.Attach(od);
                        context.Entry(od).State = EntityState.Modified;
                        context.Commit();
                    }

                    return Request.CreateResponse(HttpStatusCode.OK, od);
                }

                catch (Exception ex)
                {
                    logger.Error(ex.Message);
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex.Message);
                }
            }
        }

        #endregion

        #region cancel process of order of dream item
        [Route("cancel")]
        [AcceptVerbs("PUT")]
        public HttpResponseMessage Put(string cancel, int id)
        {
            using (var context = new AuthContext())
            {
                try
                {
                    var od = context.DreamOrderDb.Where(x => x.Order_Id == id).Include("DreamItemDetails").SingleOrDefault();
                    if (od != null)
                    {
                        od.Status = "Canceled";
                        od.UpdatedDate = indianTime;

                        //context.DreamOrderDb.Attach(od);
                        context.Entry(od).State = EntityState.Modified;
                        context.Commit();
                        Wallet wlt = context.WalletDb.Where(c => c.CustomerId == od.CustomerId).SingleOrDefault();
                        if (wlt != null)
                        {
                            if (od.WalletPoint > 0)
                            {


                                try
                                {
                                    CustomerWalletHistory CWH = new CustomerWalletHistory();
                                    Warehouse w = context.Warehouses.Where(wr => wr.WarehouseId == od.WarehouseId).FirstOrDefault();
                                    CWH.WarehouseId = od.WarehouseId;
                                    CWH.CompanyId = w.CompanyId;
                                    CWH.CustomerId = wlt.CustomerId;
                                    CWH.NewAddedWAmount = od.WalletPoint;
                                    CWH.OrderId = od.Order_Id;
                                    CWH.TotalWalletAmount = wlt.TotalAmount + od.WalletPoint;
                                    CWH.CreatedDate = indianTime;
                                    CWH.UpdatedDate = indianTime;
                                    context.CustomerWalletHistoryDb.Add(CWH);
                                    int idd = context.Commit();
                                }
                                catch (Exception ex)
                                {
                                }
                                wlt.TotalAmount += od.WalletPoint;
                                wlt.TransactionDate = indianTime;

                                context.WalletDb.Attach(wlt);
                                context.Entry(wlt).State = EntityState.Modified;
                                context.Commit();
                                var rpoint = context.RewardPointDb.Where(c => c.CustomerId == od.CustomerId).SingleOrDefault();
                                if (rpoint != null)
                                {
                                    rpoint.UsedPoint -= od.WalletPoint;
                                    rpoint.UpdatedDate = indianTime;
                                    context.RewardPointDb.Attach(rpoint);
                                    context.Entry(rpoint).State = EntityState.Modified;
                                    context.Commit();
                                }
                            }
                        }
                    }

                    return Request.CreateResponse(HttpStatusCode.OK, od);
                }

                catch (Exception ex)
                {
                    logger.Error(ex.Message);
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex.Message);
                }
            }
        }

        #endregion

        #region get dream order on page
        [Route("dreamitem")]
        [AcceptVerbs("GET")]
        public HttpResponseMessage get(int list, int page)
        {
            using (var context = new AuthContext())
            {
                try
                {
                    var identity = User.Identity as ClaimsIdentity;
                    int compid = 0, userid = 0;
                    int Warehouse_id = 0;

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
                            Warehouse_id = int.Parse(claim.Value);
                        }
                    }


                    if (Warehouse_id > 0)
                    {

                        var sc = context.DreamOrderDb.Where(x => x.Deleted == false && x.WarehouseId == Warehouse_id).Include("DreamItemDetails").OrderByDescending(x => x.Order_Id).Skip((page - 1) * list).Take(list).ToList();
                        PaggingData obj = new PaggingData();
                        obj.total_count = context.DreamOrderDb.Count();
                        obj.ordermaster = sc;
                        return Request.CreateResponse(HttpStatusCode.OK, obj);


                    }
                    else
                    {

                        var sc = context.DreamOrderDb.Where(x => x.Deleted == false).Include("DreamItemDetails").OrderByDescending(x => x.Order_Id).Skip((page - 1) * list).Take(list).ToList();
                        PaggingData obj = new PaggingData();
                        obj.total_count = context.DreamOrderDb.Count();
                        obj.ordermaster = sc;
                        return Request.CreateResponse(HttpStatusCode.OK, obj);
                    }

                }
                catch (Exception ex)
                {
                    logger.Error(ex.Message);
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex.Message);
                }

            }
        }
        #endregion
        #region get dream order on Warehouse Based
        [Route("Warehousebased")]
        [AcceptVerbs("GET")]
        public HttpResponseMessage Warehousebasedget(int list, int page, int WarehouseId)
        {
            using (var context = new AuthContext())
            {
                try
                {
                    var identity = User.Identity as ClaimsIdentity;
                    int compid = 0, userid = 0;
                    int Warehouse_id = 0;

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
                            Warehouse_id = int.Parse(claim.Value);
                        }
                    }

                    Warehouse_id = WarehouseId;
                    if (Warehouse_id > 0)
                    {

                        var sc = context.DreamOrderDb.Where(x => x.Deleted == false && x.WarehouseId == Warehouse_id).Include("DreamItemDetails").OrderByDescending(x => x.Order_Id).Skip((page - 1) * list).Take(list).ToList();
                        PaggingData obj = new PaggingData();
                        obj.total_count = context.DreamOrderDb.Count();
                        obj.ordermaster = sc;
                        return Request.CreateResponse(HttpStatusCode.OK, obj);


                    }
                    else
                    {

                        var sc = context.DreamOrderDb.Where(x => x.Deleted == false).Include("DreamItemDetails").OrderByDescending(x => x.Order_Id).Skip((page - 1) * list).Take(list).ToList();
                        PaggingData obj = new PaggingData();
                        obj.total_count = context.DreamOrderDb.Count();
                        obj.ordermaster = sc;
                        return Request.CreateResponse(HttpStatusCode.OK, obj);
                    }


                }
                catch (Exception ex)
                {
                    logger.Error(ex.Message);
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex.Message);
                }
            }
        }
        #endregion
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sc"></param>
        /// <returns></returns>
        /// 
        [Route("getsearchdata")]
        [HttpGet]
        public IEnumerable<DreamOrder> getsearchdata(string Status, int hubid)
        {

            logger.Info("start DreamOrder: ");

            List<DreamOrder> ass = new List<DreamOrder>();
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
                    logger.Info("User ID : {0} , Company Id : {1}", compid, userid);


                    var list = context.DreamOrderDb.Where(p => p.Deleted == false && p.Status == Status && p.WarehouseId == hubid).Include("DreamItemDetails").OrderByDescending(x => x.Order_Id).ToList();




                    logger.Info("End DreamOrder: ");
                    return list;
                }
                catch (Exception ex)
                {
                    logger.Error("Error in DreamOrder " + ex.Message);
                    logger.Info("End DreamOrder: ");
                    return null;
                }
            }
        }

        #region History record from App order (10 order ov less than 700 rs order issue)
        public bool AddPuchOrderMaster(ShoppingCart sc)
        {
            using (var context = new AuthContext())
            {
                try
                {

                    OrderPunchMaster CWH = new OrderPunchMaster();
                    CWH.Customerphonenum = sc.Customerphonenum;
                    CWH.SalesPersonId = sc.SalesPersonId;
                    CWH.CustomerName = sc.CustomerName;
                    CWH.Trupay = sc.Trupay;
                    CWH.ShopName = sc.ShopName;
                    CWH.ShippingAddress = sc.ShippingAddress;
                    CWH.Skcode = sc.Skcode;
                    CWH.deliveryCharge = sc.deliveryCharge;
                    CWH.WalletAmount = sc.WalletAmount;
                    CWH.walletPointUsed = sc.walletPointUsed;
                    CWH.DialEarnigPoint = sc.DialEarnigPoint;
                    CWH.Savingamount = sc.Savingamount;
                    CWH.TotalAmount = sc.TotalAmount;
                    CWH.OnlineServiceTax = sc.OnlineServiceTax;
                    CWH.TotalAmount = sc.TotalAmount;
                    CWH.CreatedDate = indianTime;
                    context.OrderPunchMasterDB.Add(CWH);
                    int idd = context.Commit();
                    if (idd > 0)
                    {
                        foreach (var a in sc.itemDetails)
                        {
                            OrderPunchDetails opd = new OrderPunchDetails();
                            opd.OrderPunchId = CWH.Id;
                            opd.ItemId = a.ItemId;
                            opd.qty = a.qty;
                            opd.WarehouseId = a.WarehouseId;
                            opd.CompanyId = a.CompanyId;
                            opd.CreatedDate = indianTime;
                            context.OrderPunchDetailsDB.Add(opd);
                            int iddD = context.Commit();
                        }

                    }

                    return true;
                }
                catch (Exception ex)
                {
                    logger.Error(ex.Message);
                    return false;

                }

            }

        }
        #endregion

        #region get dream order on page
        [Route("dreamitem")]
        [AcceptVerbs("GET")]
        public HttpResponseMessage getdreamitem(int list, int page)
        {
            using (var context = new AuthContext())
            {
                try

                {
                    var sc = context.DreamOrderDb.Where(x => x.Deleted == false).OrderByDescending(x => x.Order_Id).Skip((page - 1) * list).Take(list).ToList();
                    PaggingData obj = new PaggingData();
                    obj.total_count = context.DreamOrderDb.Count();
                    obj.ordermaster = sc;
                    return Request.CreateResponse(HttpStatusCode.OK, obj);
                }
                catch (Exception ex)
                {
                    logger.Error(ex.Message);
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex.Message);
                }
            }
        }
        #endregion

        #region Post Inactive Customer Order V3 by Sachin & Harry 27/05/2019
        public InActiveCustomerOrderMaster AddInactiveCustomerOrderMasterV3(ShoppingCart sc)
        {
            using (var context = new AuthContext())
            {
                try
                {
                    InActiveCustomerOrderMaster objOrderMaster = new InActiveCustomerOrderMaster();
                    double offerWalletPoint = 0;
                    Customer cust = context.Customers.Where(c => c.Mobile.Equals(sc.Customerphonenum) || c.Skcode == sc.Skcode).Select(c => c).SingleOrDefault();
                    if (sc.itemDetails[0].CompanyId > 0)
                    {
                        List<CompanyItemDetail> companyItems = new List<CompanyItemDetail>();
                        foreach (var cItems in sc.itemDetails)
                        {
                            if (companyItems.Count == 0)
                            {
                                CompanyItemDetail comItems = new CompanyItemDetail();
                                comItems.item_Detail = sc.itemDetails.Where(c => c.CompanyId == cItems.CompanyId).ToList();
                                companyItems.Add(comItems);
                            }
                            else
                            {
                                var add = false;
                                foreach (var cIt in companyItems)
                                {
                                    var cpp = cIt.item_Detail.Where(c => c.CompanyId == cItems.CompanyId).FirstOrDefault();
                                    if (cpp != null)
                                    {
                                        add = true;
                                        break;
                                    }
                                }
                                if (add == false)
                                {
                                    CompanyItemDetail comItems = new CompanyItemDetail();
                                    comItems.item_Detail = sc.itemDetails.Where(c => c.CompanyId == cItems.CompanyId).ToList();
                                    companyItems.Add(comItems);
                                }
                            }
                        }
                        List<int> offerItemId = new List<int>();
                        List<int> FlashDealOrderId = new List<int>();
                        foreach (var itmList in companyItems)
                        {
                            double finaltotal = 0;
                            double finalTaxAmount = 0;
                            double finalSGSTTaxAmount = 0;
                            double finalCGSTTaxAmount = 0;
                            double finalGrossAmount = 0;
                            double finalTotalTaxAmount = 0;
                            if (cust != null)
                            {
                                cust.ShippingAddress = sc.ShippingAddress;
                                //context.Customers.Attach(cust);
                                context.Entry(cust).State = EntityState.Modified;
                                context.Commit();
                            }
                            List<IDetail> cart = new List<IDetail>();
                            cart = itmList.item_Detail.Where(a => a.qty > 0).Select(a => a).ToList<IDetail>();
                            Int32 iddd = (Int32)cart[0].WarehouseId;
                            var warehouse = context.Warehouses.Where(w => w.WarehouseId == iddd).Select(c => c).SingleOrDefault();
                            Customer custPeople = context.Customers.Where(c => c.CustomerId == cust.CustomerId && c.Warehouseid == warehouse.WarehouseId).Select(c => c).SingleOrDefault();
                            try
                            {
                                objOrderMaster.CompanyId = warehouse.CompanyId;
                                objOrderMaster.WarehouseId = warehouse.WarehouseId;
                                objOrderMaster.WarehouseName = warehouse.WarehouseName;
                                objOrderMaster.Status = "Dummy Order";
                                objOrderMaster.CustomerName = cust.Name;
                                objOrderMaster.ShopName = cust.ShopName;
                                objOrderMaster.LandMark = cust.LandMark;
                                objOrderMaster.Skcode = cust.Skcode;
                                objOrderMaster.Tin_No = cust.RefNo;
                                objOrderMaster.CustomerId = cust.CustomerId;
                                objOrderMaster.CityId = cust.Cityid;
                                objOrderMaster.Customerphonenum = (sc.Customerphonenum);
                                // MRP-Actual Price
                                objOrderMaster.Savingamount = System.Math.Round(sc.Savingamount, 2);
                                objOrderMaster.ClusterId = Convert.ToInt32(cust.ClusterId);
                                objOrderMaster.OnlineServiceTax = sc.OnlineServiceTax;
                                var clstr = context.Clusters.Where(x => x.ClusterId == cust.ClusterId).SingleOrDefault();
                                if (clstr != null)
                                {
                                    objOrderMaster.ClusterName = clstr.ClusterName;
                                }
                                //People p = context.Peoples.Where(x => x.PeopleID == custPeople.ExecutiveId && x.Deleted == false && x.Active == true).SingleOrDefault();
                                //if (p != null)
                                //{
                                //    try
                                //    {
                                //        objOrderMaster.OrderTakenSalesPersonId = p.PeopleID;
                                //        objOrderMaster.OrderTakenSalesPerson = p.PeopleFirstName + " " + p.PeopleLastName;
                                //        objOrderMaster.SalesMobile = p.Mobile;
                                //        objOrderMaster.SalesPersonId = p.PeopleID;
                                //        objOrderMaster.SalesPerson = p.PeopleFirstName + " " + p.PeopleLastName;
                                //    }
                                //    catch (Exception ex)
                                //    {
                                //        logger.Error(ex.Message + " Sales person id not exist We using 0/Self.");
                                //    }
                                //}
                                //else
                                //{
                                //    try
                                //    {
                                //        objOrderMaster.OrderTakenSalesPersonId = 0;
                                //        objOrderMaster.OrderTakenSalesPerson = "Self";
                                //    }
                                //    catch (Exception ex)
                                //    {
                                //        logger.Error(ex.Message + " Sales person id not exist We using 0/Self.");
                                //    }
                                //}
                                if (sc.SalesPersonId == 0)
                                {
                                    objOrderMaster.OrderTakenSalesPersonId = 0;
                                    objOrderMaster.OrderTakenSalesPerson = "Self";
                                }
                                objOrderMaster.BillingAddress = sc.ShippingAddress;
                                objOrderMaster.ShippingAddress = sc.ShippingAddress;
                                objOrderMaster.active = true;
                                objOrderMaster.CreatedDate = indianTime;
                                if (indianTime.Hour > 16)
                                {
                                    objOrderMaster.Deliverydate = indianTime.AddDays(2);
                                }
                                else
                                {
                                    objOrderMaster.Deliverydate = indianTime.AddDays(1);
                                }
                                objOrderMaster.UpdatedDate = indianTime;
                                objOrderMaster.Deleted = false;
                                List<InActiveCustomerOrderDetails> collection = new List<InActiveCustomerOrderDetails>();

                                objOrderMaster.orderDetails = collection;
                                var warehouseId = cart.FirstOrDefault().WarehouseId;
                                var appHome = context.AppHomeDynamicDb.Where(x => x.Wid == warehouseId && x.delete == false && x.active == true && x.detail.Any(y => y.IsFlashDeal == true && y.active == true && y.StartOfferDate <= indianTime
                                                     && y.EndOfferDate >= indianTime)).Include("detail");
                                List<AppHomeItem> appHomeItems = appHome.SelectMany(x => x.detail).ToList();
                                List<int> appHomeItemids = appHomeItems.Where(x => x.active == true).Select(y => y.ItemId).ToList();
                                var cartItemIds = cart.Where(x => x.OfferCategory == 2 && appHomeItemids.Contains(x.ItemId)).Select(x => x.ItemId).ToList();
                                foreach (var i in cart.Select(x => x))
                                {
                                    try
                                    {
                                        if (i.qty != 0 && i.qty > 0)
                                        {
                                            ItemMaster items = context.itemMasters.Where(x => x.ItemId == i.ItemId && x.active == true && x.WarehouseId == i.WarehouseId).Select(x => x).FirstOrDefault();
                                            InActiveCustomerOrderDetails od = new InActiveCustomerOrderDetails();
                                            od.CustomerId = cust.CustomerId;
                                            od.CustomerName = cust.Name;
                                            od.CityId = cust.Cityid;
                                            od.Mobile = cust.Mobile;
                                            od.OrderDate = indianTime;
                                            od.Status = "Dummy Order";
                                            od.CompanyId = warehouse.CompanyId;
                                            od.WarehouseId = warehouse.WarehouseId;
                                            od.WarehouseName = warehouse.WarehouseName;
                                            od.NetPurchasePrice = items.NetPurchasePrice + ((items.NetPurchasePrice * items.TotalTaxPercentage) / 100);
                                            od.ItemId = items.ItemId;
                                            od.Itempic = items.LogoUrl;
                                            od.itemname = items.itemname;
                                            od.SupplierName = items.SupplierName;
                                            od.SellingUnitName = items.SellingUnitName;
                                            od.CategoryName = items.CategoryName;
                                            od.SubsubcategoryName = items.SubsubcategoryName;
                                            od.SubcategoryName = items.SubcategoryName;
                                            od.SellingSku = items.SellingSku;
                                            od.City = items.CityName;
                                            od.itemcode = items.itemcode;
                                            od.HSNCode = items.HSNCode;
                                            od.itemNumber = items.Number;
                                            od.Barcode = items.itemcode;
                                            if (i.OfferCategory == 2)
                                            {
                                                AppHomeItem appHomeItem = context.AppHomeItemDb.Where(x => x.ItemId == i.ItemId && x.IsFlashDeal == true && x.active == true && x.StartOfferDate <= indianTime
                                                            && x.EndOfferDate >= indianTime).FirstOrDefault();
                                                if (appHomeItem != null)
                                                {
                                                    var itemprice = context.itemMasters.Where(x => x.ItemId == i.ItemId && x.Deleted == false).FirstOrDefault();
                                                    od.UnitPrice = Convert.ToDouble(itemprice.FlashDealSpecialPrice);
                                                }
                                                else
                                                {
                                                    od.UnitPrice = items.UnitPrice;
                                                }

                                            }
                                            else
                                            {
                                                od.UnitPrice = items.UnitPrice;
                                            }

                                            if (i.OfferCategory == 2)
                                            {
                                                AppHomeItem appHomeItem = appHomeItems.Where(x => x.ItemId == i.ItemId && x.IsFlashDeal == true && x.active == true && x.StartOfferDate <= indianTime
                                                          && x.EndOfferDate >= indianTime).FirstOrDefault();
                                                //AppHomeItem appHomeItem = AppHomeItemDb.Where(x => x.ItemId == i.ItemId && x.IsFlashDeal == true && x.active == true && x.StartOfferDate <= indianTime
                                                //           && x.EndOfferDate >= indianTime).FirstOrDefault();
                                                if (appHomeItem != null)
                                                {
                                                    try
                                                    {
                                                        items.OfferQtyAvaiable = items.OfferQtyAvaiable - i.qty;
                                                        items.OfferQtyConsumed = items.OfferQtyConsumed + i.qty;
                                                        //context.itemMasters.Attach(items);
                                                        context.Entry(items).State = EntityState.Modified;
                                                        context.Commit();
                                                        appHomeItem.FlashDealQtyAvaiable = appHomeItem.FlashDealQtyAvaiable - i.qty;
                                                        context.AppHomeItemDb.Attach(appHomeItem);
                                                        context.Entry(appHomeItem).State = EntityState.Modified;
                                                        context.Commit();
                                                    }
                                                    catch (Exception ee)
                                                    {

                                                    }
                                                }



                                                //Insert in flashdealitemconsumed for functionilty that an customer take only one time flash deal.
                                                FlashDealItemConsumed flashDealItemConsumed = new FlashDealItemConsumed();
                                                flashDealItemConsumed.FlashDealId = Convert.ToInt32(appHomeItem.id);
                                                flashDealItemConsumed.ItemId = i.ItemId;
                                                flashDealItemConsumed.WarehouseId = i.WarehouseId;
                                                flashDealItemConsumed.CompanyId = i.CompanyId;
                                                flashDealItemConsumed.CustomerId = cust.CustomerId;
                                                flashDealItemConsumed.CreatedDate = indianTime;
                                                flashDealItemConsumed.UpdatedDate = indianTime;
                                                context.FlashDealItemConsumedDB.Add(flashDealItemConsumed);
                                                context.Commit();
                                                FlashDealOrderId.Add(flashDealItemConsumed.FlashDealItemConsumedId);

                                            }
                                            //od.UnitPrice = items.UnitPrice;
                                            od.price = items.price;
                                            od.MinOrderQty = items.MinOrderQty;
                                            od.MinOrderQtyPrice = (od.MinOrderQty * items.UnitPrice);
                                            od.qty = Convert.ToInt32(i.qty);
                                            od.SizePerUnit = items.SizePerUnit;
                                            od.TaxPercentage = items.TotalTaxPercentage;
                                            if (od.TaxPercentage >= 0)
                                            {
                                                od.SGSTTaxPercentage = od.TaxPercentage / 2;
                                                od.CGSTTaxPercentage = od.TaxPercentage / 2;
                                            }
                                            od.Noqty = od.qty; // for total qty (no of items)    
                                            od.TotalAmt = System.Math.Round(od.UnitPrice * od.qty, 2);
                                            od.AmtWithoutTaxDisc = ((100 * od.UnitPrice * od.qty) / (1 + od.TaxPercentage / 100)) / 100;
                                            od.AmtWithoutAfterTaxDisc = (100 * od.AmtWithoutTaxDisc) / (100 + items.PramotionalDiscount);
                                            od.TaxAmmount = (od.AmtWithoutAfterTaxDisc * od.TaxPercentage) / 100;
                                            if (od.TaxAmmount >= 0)
                                            {
                                                od.SGSTTaxAmmount = od.TaxAmmount / 2;
                                                od.CGSTTaxAmmount = od.TaxAmmount / 2;
                                            }
                                            od.TotalAmountAfterTaxDisc = od.AmtWithoutAfterTaxDisc + od.TaxAmmount;
                                            od.DiscountPercentage = items.PramotionalDiscount;
                                            od.DiscountAmmount = (od.NetAmmount * items.PramotionalDiscount) / 100;
                                            double DiscountAmmount = od.DiscountAmmount;
                                            double NetAmtAfterDis = (od.NetAmmount - DiscountAmmount);
                                            od.NetAmtAfterDis = (od.NetAmmount - DiscountAmmount);
                                            double TaxAmmount = od.TaxAmmount;
                                            od.Purchaseprice = items.PurchasePrice;
                                            od.CreatedDate = indianTime;
                                            od.UpdatedDate = indianTime;
                                            od.Deleted = false;
                                            objOrderMaster.orderDetails.Add(od);
                                            finaltotal = finaltotal + od.TotalAmt;
                                            finalTaxAmount = finalTaxAmount + od.TaxAmmount;
                                            finalSGSTTaxAmount = finalSGSTTaxAmount + od.SGSTTaxAmmount;
                                            finalCGSTTaxAmount = finalCGSTTaxAmount + od.CGSTTaxAmmount;
                                            finalGrossAmount = finalGrossAmount + od.TotalAmountAfterTaxDisc;
                                            finalTotalTaxAmount = finalTotalTaxAmount + od.TotalAmountAfterTaxDisc;

                                            if (i.IsOffer == true && i.FreeItemId > 0 && i.FreeItemqty > 0)
                                            {
                                                //When there is a free item then we add this item in order detail
                                                //Calculate its unit price as 0.
                                                ItemMaster Freeitem = context.itemMasters.Where(x => x.ItemId == i.FreeItemId && x.Deleted == false && x.active == true && x.WarehouseId == i.WarehouseId).Select(x => x).FirstOrDefault();
                                                InActiveCustomerOrderDetails od1 = new InActiveCustomerOrderDetails();
                                                od1.CustomerId = cust.CustomerId;
                                                od1.CustomerName = cust.Name;
                                                od1.CityId = cust.Cityid;
                                                od1.Mobile = cust.Mobile;
                                                od1.OrderDate = indianTime;
                                                od1.Status = "Pending";
                                                od1.CompanyId = warehouse.CompanyId;
                                                od1.WarehouseId = warehouse.WarehouseId;
                                                od1.WarehouseName = warehouse.WarehouseName;
                                                od1.NetPurchasePrice = Freeitem.NetPurchasePrice + ((Freeitem.NetPurchasePrice * Freeitem.TotalTaxPercentage) / 100);
                                                od1.ItemId = Freeitem.ItemId;
                                                od1.ItemMultiMRPId = Freeitem.ItemMultiMRPId;
                                                od1.Itempic = Freeitem.LogoUrl;
                                                od1.itemname = Freeitem.itemname;
                                                od1.SupplierName = Freeitem.SupplierName;
                                                od1.SellingUnitName = Freeitem.SellingUnitName;
                                                od1.CategoryName = Freeitem.CategoryName;
                                                od1.SubsubcategoryName = Freeitem.SubsubcategoryName;
                                                od1.SubcategoryName = Freeitem.SubcategoryName;
                                                od1.SellingSku = Freeitem.SellingSku;
                                                od1.City = Freeitem.CityName;
                                                od1.itemcode = Freeitem.itemcode;
                                                od1.HSNCode = Freeitem.HSNCode;
                                                od1.itemNumber = Freeitem.Number;
                                                od1.Barcode = Freeitem.itemcode;
                                                od1.UnitPrice = 0;
                                                od1.price = Freeitem.price;
                                                od1.MinOrderQty = 0;
                                                od1.MinOrderQtyPrice = 0;
                                                od1.qty = Convert.ToInt32(i.FreeItemqty);
                                                od1.SizePerUnit = 0;
                                                od1.TaxPercentage = 0;
                                                od1.SGSTTaxPercentage = 0;
                                                od1.CGSTTaxPercentage = 0;
                                                od1.Noqty = od1.qty; // for total qty (no of items)    
                                                od1.TotalAmt = 0;
                                                od1.AmtWithoutTaxDisc = 0;
                                                od1.AmtWithoutAfterTaxDisc = 0;
                                                od1.AmtWithoutTaxDisc = 0;
                                                od1.AmtWithoutAfterTaxDisc = 0;
                                                od1.TaxAmmount = 0;
                                                if (od1.TaxAmmount >= 0)
                                                {
                                                    od1.SGSTTaxAmmount = 0;
                                                    od1.CGSTTaxAmmount = 0;
                                                }
                                                od1.DiscountPercentage = 0;
                                                od1.DiscountAmmount = 0;
                                                DiscountAmmount = 0;
                                                NetAmtAfterDis = 0;
                                                od1.NetAmtAfterDis = 0;
                                                TaxAmmount = 0;
                                                od1.Purchaseprice = 0;
                                                od1.CreatedDate = indianTime;
                                                od1.UpdatedDate = indianTime;
                                                od1.Deleted = false;
                                                od1.marginPoint = 0;
                                                objOrderMaster.orderDetails.Add(od1);
                                                var offer = context.OfferDb.Where(x => x.OfferId == i.OfferId).SingleOrDefault();
                                                //If there is an offer then  we insert its value on offeritem table.
                                                OfferItem ff = new OfferItem();
                                                try
                                                {
                                                    ff.CompanyId = i.CompanyId;
                                                    ff.WarehouseId = i.WarehouseId;
                                                    ff.itemId = offer.itemId;
                                                    ff.itemname = offer.itemname;
                                                    ff.MinOrderQuantity = offer.MinOrderQuantity;
                                                    ff.NoOffreeQuantity = i.FreeItemqty;
                                                    ff.FreeItemId = offer.FreeItemId;
                                                    ff.FreeItemName = offer.FreeItemName;
                                                    ff.FreeItemMRP = offer.FreeItemMRP;
                                                    ff.IsDeleted = false;
                                                    ff.CreatedDate = DateTime.Now;
                                                    ff.UpdateDate = DateTime.Now;
                                                    ff.CustomerId = cust.CustomerId;
                                                    ff.OrderId = objOrderMaster.OrderId;
                                                    ff.OfferType = "ItemMaster";
                                                    ff.ReferOfferId = i.OfferId;
                                                    context.OfferItemDb.Add(ff);
                                                    context.Commit();
                                                    offerItemId.Add(ff.OfferId);
                                                }
                                                catch (Exception ee)
                                                {

                                                }

                                                if (i.IsOffer == true && i.OfferWalletPoint > 0)
                                                {
                                                    //If offer is on wallet point then update is wallet point.
                                                    offerWalletPoint = offerWalletPoint + i.OfferWalletPoint;
                                                    var offerdata = context.OfferDb.Where(x => x.OfferId == i.OfferId).SingleOrDefault();
                                                    OfferItem offerItem = new OfferItem();
                                                    try
                                                    {
                                                        offerItem.CompanyId = i.CompanyId;
                                                        offerItem.WarehouseId = i.WarehouseId;
                                                        offerItem.itemId = offerdata.itemId;
                                                        offerItem.itemname = offerdata.itemname;
                                                        offerItem.MinOrderQuantity = offerdata.MinOrderQuantity;
                                                        offerItem.NoOffreeQuantity = i.FreeItemqty;
                                                        offerItem.FreeItemId = offerdata.FreeItemId;
                                                        offerItem.FreeItemName = offerdata.FreeItemName;
                                                        offerItem.FreeItemMRP = offerdata.FreeItemMRP;
                                                        offerItem.IsDeleted = false;
                                                        offerItem.CreatedDate = DateTime.Now;
                                                        offerItem.UpdateDate = DateTime.Now;
                                                        offerItem.CustomerId = cust.CustomerId;
                                                        offerItem.OrderId = objOrderMaster.OrderId;
                                                        offerItem.WallentPoint = Convert.ToInt32(i.OfferWalletPoint);
                                                        offerItem.OfferId = i.OfferId;
                                                        offerItem.OfferType = "WalletPoint";
                                                        context.OfferItemDb.Add(ff);
                                                        context.Commit();
                                                        offerItemId.Add(offerItem.OfferId);
                                                    }
                                                    catch (Exception ee)
                                                    {

                                                    }
                                                }

                                            }
                                        }
                                    }
                                    catch (Exception es)
                                    {
                                        logger.Error(es.Message + " if Item not activated or not exist ");
                                    }
                                }
                                objOrderMaster.deliveryCharge = sc.deliveryCharge;
                                try
                                {
                                    objOrderMaster.TotalAmount = System.Math.Round(finaltotal, 2) + sc.deliveryCharge;
                                    objOrderMaster.TaxAmount = System.Math.Round(finalTaxAmount, 2);
                                    objOrderMaster.SGSTTaxAmmount = System.Math.Round(finalSGSTTaxAmount, 2);
                                    objOrderMaster.CGSTTaxAmmount = System.Math.Round(finalCGSTTaxAmount, 2);
                                    objOrderMaster.GrossAmount = System.Math.Round((Convert.ToInt32(finalGrossAmount) + sc.deliveryCharge), 0);
                                    objOrderMaster.DiscountAmount = System.Math.Round((finalTotalTaxAmount - finaltotal), 2);
                                    try
                                    {
                                        if (sc.Trupay != null && sc.Trupay != "false")
                                        {
                                            objOrderMaster.Trupay = sc.Trupay;
                                            objOrderMaster.paymentThrough = sc.paymentThrough;
                                            TrupayTransaction tpt = new TrupayTransaction();
                                            tpt.TrupayTransactionStatus = objOrderMaster.Trupay;
                                            tpt.TrupayTransactionId = sc.TrupayTransactionId;
                                            tpt.paymentMode = sc.paymentMode;
                                            tpt.paymentThrough = sc.paymentThrough;
                                            tpt.CustomerName = cust.Name;
                                            tpt.Status = "From Inactive Order";
                                            tpt.Skcode = cust.Skcode;
                                            tpt.WarehouseName = warehouse.WarehouseName; ;
                                            tpt.OnlineAmount = objOrderMaster.TotalAmount;
                                            tpt.OnlineServiceTax = objOrderMaster.OnlineServiceTax;
                                            tpt.CreatedDate = indianTime;
                                            context.TrupayTransactionDB.Add(tpt);
                                            context.Commit();
                                        }
                                    }
                                    catch (Exception ess)
                                    {
                                        logger.Error(ess.Message + "From Order  ");
                                    }
                                }
                                catch (Exception ex)
                                {
                                    logger.Error("Error in Get single GetcusomerWallets " + ex.Message);
                                }
                            }
                            catch (Exception ex)
                            {
                                logger.Error(ex.Message);
                            }

                        }

                        double rewAmount = 0;
                        objOrderMaster.GrossAmount = System.Math.Round((objOrderMaster.GrossAmount - rewAmount), 0);
                        objOrderMaster.TotalAmount = objOrderMaster.TotalAmount - rewAmount;
                        context.InActiveCustomerOrderMasterDB.Add(objOrderMaster);
                        int id = context.Commit();
                        foreach (var data in offerItemId)
                        {
                            try
                            {
                                var offerd = context.OfferItemDb.Where(x => x.OfferId == data).SingleOrDefault();
                                offerd.OrderId = objOrderMaster.OrderId;
                                context.OfferItemDb.Attach(offerd);
                                context.Entry(offerd).State = EntityState.Modified;
                                context.Commit();
                            }
                            catch (Exception ee)
                            {

                            }

                        }
                        //Update OrderId in FlashDealItemConsumedDB
                        foreach (var FlashDealOrderIdData in FlashDealOrderId)
                        {
                            try
                            {
                                var offerd = context.FlashDealItemConsumedDB.Where(x => x.FlashDealItemConsumedId == FlashDealOrderIdData).SingleOrDefault();
                                offerd.OrderId = objOrderMaster.OrderId;
                                context.FlashDealItemConsumedDB.Attach(offerd);
                                context.Entry(offerd).State = EntityState.Modified;
                                context.Commit();
                            }
                            catch (Exception ee)
                            {

                            }
                        }
                        //#region call to whatsapp Bot 
                        //try
                        //{

                        //    cust.ClusterId = 27;//send for template
                        //    cust.BillingAddress = objOrderMaster.CreatedDate.ToString("dd-MM-YYYY");//send for status
                        //    cust.CustomerId = objOrderMaster.OrderId;//send for Order Id
                        //    context.Customersms(cust);
                        //}
                        //catch (Exception ex) { }
                        //#endregion            
                    }
                    return objOrderMaster;
                }
                catch (Exception ex)
                {
                    logger.Error(ex.Message);
                    return null;

                }

            }
        }
        #endregion

        #region Old Post Inactive Customer Order V1
        public InActiveCustomerOrderMaster AddInactiveCustomerOrderMaster(ShoppingCart sc)
        {
            using (var context = new AuthContext())
            {
                try
                {
                    InActiveCustomerOrderMaster objOrderMaster = new InActiveCustomerOrderMaster();

                    //Customer cust = context.Customers.Where(c => c.Mobile.Equals(sc.Customerphonenum) || c.Skcode == sc.Skcode).Select(c => c).SingleOrDefault();
                    //if (sc.itemDetails[0].CompanyId > 0)
                    //{
                    //    List<CompanyItemDetail> companyItems = new List<CompanyItemDetail>();
                    //    foreach (var cItems in sc.itemDetails)
                    //    {
                    //        if (companyItems.Count == 0)
                    //        {
                    //            CompanyItemDetail comItems = new CompanyItemDetail();
                    //            comItems.item_Detail = sc.itemDetails.Where(c => c.CompanyId == cItems.CompanyId).ToList();
                    //            companyItems.Add(comItems);
                    //        }
                    //        else
                    //        {
                    //            var add = false;
                    //            foreach (var cIt in companyItems)
                    //            {
                    //                var cpp = cIt.item_Detail.Where(c => c.CompanyId == cItems.CompanyId).FirstOrDefault();
                    //                if (cpp != null)
                    //                {
                    //                    add = true;
                    //                    break;
                    //                }
                    //            }
                    //            if (add == false)
                    //            {
                    //                CompanyItemDetail comItems = new CompanyItemDetail();
                    //                comItems.item_Detail = sc.itemDetails.Where(c => c.CompanyId == cItems.CompanyId).ToList();
                    //                companyItems.Add(comItems);
                    //            }
                    //        }
                    //    }
                    //    foreach (var itmList in companyItems)
                    //    {
                    //        double finaltotal = 0;
                    //        double finalTaxAmount = 0;
                    //        double finalSGSTTaxAmount = 0;
                    //        double finalCGSTTaxAmount = 0;
                    //        double finalGrossAmount = 0;
                    //        double finalTotalTaxAmount = 0;
                    //        if (cust != null)
                    //        {
                    //            cust.ShippingAddress = sc.ShippingAddress;
                    //            //context.Customers.Attach(cust);
                    //            context.Entry(cust).State = EntityState.Modified;
                    //            context.Commit();
                    //        }
                    //        List<IDetail> cart = new List<IDetail>();
                    //        cart = itmList.item_Detail.Where(a => a.qty > 0).Select(a => a).ToList<IDetail>();
                    //        Int32 iddd = (Int32)cart[0].WarehouseId;
                    //        var warehouse = context.Warehouses.Where(w => w.WarehouseId == iddd).Select(c => c).SingleOrDefault();
                    //        Customer custPeople = context.Customers.Where(c => c.CustomerId == cust.CustomerId && c.Warehouseid == warehouse.WarehouseId).Select(c => c).SingleOrDefault();
                    //        try
                    //        {
                    //            objOrderMaster.CompanyId = warehouse.CompanyId;
                    //            objOrderMaster.WarehouseId = warehouse.WarehouseId;
                    //            objOrderMaster.WarehouseName = warehouse.WarehouseName;
                    //            objOrderMaster.Status = "Dummy Order";
                    //            objOrderMaster.CustomerName = cust.Name;
                    //            objOrderMaster.ShopName = cust.ShopName;
                    //            objOrderMaster.LandMark = cust.LandMark;
                    //            objOrderMaster.Skcode = cust.Skcode;
                    //            objOrderMaster.Tin_No = cust.RefNo;
                    //            objOrderMaster.CustomerId = cust.CustomerId;
                    //            objOrderMaster.CityId = cust.Cityid;
                    //            objOrderMaster.Customerphonenum = (sc.Customerphonenum);
                    //            // MRP-Actual Price
                    //            objOrderMaster.Savingamount = System.Math.Round(sc.Savingamount, 2);
                    //            objOrderMaster.ClusterId = Convert.ToInt32(cust.ClusterId);
                    //            objOrderMaster.OnlineServiceTax = sc.OnlineServiceTax;
                    //            var clstr = context.Clusters.Where(x => x.ClusterId == cust.ClusterId).SingleOrDefault();
                    //            if (clstr != null)
                    //            {
                    //                objOrderMaster.ClusterName = clstr.ClusterName;
                    //            }
                    //            People p = context.Peoples.Where(x => x.PeopleID == custPeople.ExecutiveId && x.Deleted == false && x.Active == true).SingleOrDefault();
                    //            if (p != null)
                    //            {
                    //                try
                    //                {
                    //                    objOrderMaster.OrderTakenSalesPersonId = p.PeopleID;
                    //                    objOrderMaster.OrderTakenSalesPerson = p.PeopleFirstName + " " + p.PeopleLastName;
                    //                    objOrderMaster.SalesMobile = p.Mobile;
                    //                    objOrderMaster.SalesPersonId = p.PeopleID;
                    //                    objOrderMaster.SalesPerson = p.PeopleFirstName + " " + p.PeopleLastName;
                    //                }
                    //                catch (Exception ex)
                    //                {
                    //                    logger.Error(ex.Message + " Sales person id not exist We using 0/Self.");
                    //                }
                    //            }
                    //            else
                    //            {
                    //                try
                    //                {
                    //                    objOrderMaster.OrderTakenSalesPersonId = 0;
                    //                    objOrderMaster.OrderTakenSalesPerson = "Self";
                    //                }
                    //                catch (Exception ex)
                    //                {
                    //                    logger.Error(ex.Message + " Sales person id not exist We using 0/Self.");
                    //                }
                    //            }
                    //            if (sc.SalesPersonId == 0)
                    //            {
                    //                objOrderMaster.OrderTakenSalesPersonId = 0;
                    //                objOrderMaster.OrderTakenSalesPerson = "Self";
                    //            }
                    //            objOrderMaster.BillingAddress = sc.ShippingAddress;
                    //            objOrderMaster.ShippingAddress = sc.ShippingAddress;
                    //            objOrderMaster.active = true;
                    //            objOrderMaster.CreatedDate = indianTime;
                    //            if (indianTime.Hour > 16)
                    //            {
                    //                objOrderMaster.Deliverydate = indianTime.AddDays(2);
                    //            }
                    //            else
                    //            {
                    //                objOrderMaster.Deliverydate = indianTime.AddDays(1);
                    //            }
                    //            objOrderMaster.UpdatedDate = indianTime;
                    //            objOrderMaster.Deleted = false;
                    //            List<InActiveCustomerOrderDetails> collection = new List<InActiveCustomerOrderDetails>();
                    //            objOrderMaster.orderDetails = collection;
                    //            foreach (var i in cart.Select(x => x))
                    //            {
                    //                try
                    //                {
                    //                    if (i.qty != 0 && i.qty > 0)
                    //                    {
                    //                        ItemMaster items = context.itemMasters.Where(x => x.ItemId == i.ItemId && x.active == true && x.WarehouseId == i.WarehouseId).Select(x => x).FirstOrDefault();
                    //                        InActiveCustomerOrderDetails od = new InActiveCustomerOrderDetails();
                    //                        od.CustomerId = cust.CustomerId;
                    //                        od.CustomerName = cust.Name;
                    //                        od.CityId = cust.Cityid;
                    //                        od.Mobile = cust.Mobile;
                    //                        od.OrderDate = indianTime;
                    //                        od.Status = "Dummy Order";
                    //                        od.CompanyId = warehouse.CompanyId;
                    //                        od.WarehouseId = warehouse.WarehouseId;
                    //                        od.WarehouseName = warehouse.WarehouseName;
                    //                        od.NetPurchasePrice = items.NetPurchasePrice + ((items.NetPurchasePrice * items.TotalTaxPercentage) / 100);
                    //                        od.ItemId = items.ItemId;
                    //                        od.Itempic = items.LogoUrl;
                    //                        od.itemname = items.itemname;
                    //                        od.SupplierName = items.SupplierName;
                    //                        od.SellingUnitName = items.SellingUnitName;
                    //                        od.CategoryName = items.CategoryName;
                    //                        od.SubsubcategoryName = items.SubsubcategoryName;
                    //                        od.SubcategoryName = items.SubcategoryName;
                    //                        od.SellingSku = items.SellingSku;
                    //                        od.City = items.CityName;
                    //                        od.itemcode = items.itemcode;
                    //                        od.HSNCode = items.HSNCode;
                    //                        od.itemNumber = items.Number;
                    //                        od.Barcode = items.itemcode;
                    //                        od.UnitPrice = items.UnitPrice;
                    //                        od.price = items.price;
                    //                        od.MinOrderQty = items.MinOrderQty;
                    //                        od.MinOrderQtyPrice = (od.MinOrderQty * items.UnitPrice);
                    //                        od.qty = Convert.ToInt32(i.qty);
                    //                        od.SizePerUnit = items.SizePerUnit;
                    //                        od.TaxPercentage = items.TotalTaxPercentage;
                    //                        if (od.TaxPercentage >= 0)
                    //                        {
                    //                            od.SGSTTaxPercentage = od.TaxPercentage / 2;
                    //                            od.CGSTTaxPercentage = od.TaxPercentage / 2;
                    //                        }
                    //                        od.Noqty = od.qty; // for total qty (no of items)    
                    //                        od.TotalAmt = System.Math.Round(od.UnitPrice * od.qty, 2);
                    //                        od.AmtWithoutTaxDisc = ((100 * od.UnitPrice * od.qty) / (1 + od.TaxPercentage / 100)) / 100;
                    //                        od.AmtWithoutAfterTaxDisc = (100 * od.AmtWithoutTaxDisc) / (100 + items.PramotionalDiscount);
                    //                        od.TaxAmmount = (od.AmtWithoutAfterTaxDisc * od.TaxPercentage) / 100;
                    //                        if (od.TaxAmmount >= 0)
                    //                        {
                    //                            od.SGSTTaxAmmount = od.TaxAmmount / 2;
                    //                            od.CGSTTaxAmmount = od.TaxAmmount / 2;
                    //                        }
                    //                        od.TotalAmountAfterTaxDisc = od.AmtWithoutAfterTaxDisc + od.TaxAmmount;
                    //                        od.DiscountPercentage = items.PramotionalDiscount;
                    //                        od.DiscountAmmount = (od.NetAmmount * items.PramotionalDiscount) / 100;
                    //                        double DiscountAmmount = od.DiscountAmmount;
                    //                        double NetAmtAfterDis = (od.NetAmmount - DiscountAmmount);
                    //                        od.NetAmtAfterDis = (od.NetAmmount - DiscountAmmount);
                    //                        double TaxAmmount = od.TaxAmmount;
                    //                        od.Purchaseprice = items.price;
                    //                        od.CreatedDate = indianTime;
                    //                        od.UpdatedDate = indianTime;
                    //                        od.Deleted = false;
                    //                        objOrderMaster.orderDetails.Add(od);
                    //                        finaltotal = finaltotal + od.TotalAmt;
                    //                        finalTaxAmount = finalTaxAmount + od.TaxAmmount;
                    //                        finalSGSTTaxAmount = finalSGSTTaxAmount + od.SGSTTaxAmmount;
                    //                        finalCGSTTaxAmount = finalCGSTTaxAmount + od.CGSTTaxAmmount;
                    //                        finalGrossAmount = finalGrossAmount + od.TotalAmountAfterTaxDisc;
                    //                        finalTotalTaxAmount = finalTotalTaxAmount + od.TotalAmountAfterTaxDisc;
                    //                    }
                    //                }
                    //                catch (Exception es)
                    //                {
                    //                    logger.Error(es.Message + " if Item not activated or not exist ");
                    //                }
                    //            }
                    //            objOrderMaster.deliveryCharge = sc.deliveryCharge;
                    //            try
                    //            {
                    //                objOrderMaster.TotalAmount = System.Math.Round(finaltotal, 2) + sc.deliveryCharge;
                    //                objOrderMaster.TaxAmount = System.Math.Round(finalTaxAmount, 2);
                    //                objOrderMaster.SGSTTaxAmmount = System.Math.Round(finalSGSTTaxAmount, 2);
                    //                objOrderMaster.CGSTTaxAmmount = System.Math.Round(finalCGSTTaxAmount, 2);
                    //                objOrderMaster.GrossAmount = System.Math.Round((Convert.ToInt32(finalGrossAmount) + sc.deliveryCharge), 0);
                    //                objOrderMaster.DiscountAmount = System.Math.Round((finalTotalTaxAmount - finaltotal), 2);
                    //                try
                    //                {
                    //                    if (sc.Trupay != null)
                    //                    {
                    //                        objOrderMaster.Trupay = sc.Trupay;
                    //                        TrupayTransaction tpt = new TrupayTransaction();
                    //                        tpt.TrupayTransactionStatus = objOrderMaster.Trupay;
                    //                        tpt.TrupayTransactionId = sc.TrupayTransactionId;
                    //                        tpt.paymentMode = sc.paymentMode;
                    //                        tpt.CustomerName = cust.Name;
                    //                        tpt.Status = "From Inactive Order";
                    //                        tpt.Skcode = cust.Skcode;
                    //                        tpt.WarehouseName = warehouse.WarehouseName; ;
                    //                        tpt.OnlineAmount = objOrderMaster.TotalAmount;
                    //                        tpt.OnlineServiceTax = objOrderMaster.OnlineServiceTax;
                    //                        tpt.CreatedDate = indianTime;
                    //                        context.TrupayTransactionDB.Add(tpt);
                    //                        context.Commit();
                    //                    }
                    //                }
                    //                catch (Exception ess)
                    //                {
                    //                    logger.Error(ess.Message + "From Order  ");
                    //                }
                    //            }
                    //            catch (Exception ex)
                    //            {
                    //                logger.Error("Error in Get single GetcusomerWallets " + ex.Message);
                    //            }
                    //        }
                    //        catch (Exception ex)
                    //        {
                    //            logger.Error(ex.Message);
                    //        }

                    //    }

                    //    double rewAmount = 0;
                    //    objOrderMaster.GrossAmount = System.Math.Round((objOrderMaster.GrossAmount - rewAmount), 0);
                    //    objOrderMaster.TotalAmount = objOrderMaster.TotalAmount - rewAmount;
                    //    context.InActiveCustomerOrderMasterDB.Add(objOrderMaster);
                    //    int id = context.Commit();

                    //}
                    return objOrderMaster;
                }
                catch (Exception ex)
                {
                    logger.Error(ex.Message);
                    return null;

                }
            }
        }
        #endregion



        #region update Correct taxes on order
        [Route("UpdateOrderTax/Tax")]
        [HttpPost]
        public HttpResponseMessage updateoldlive(List<int> OrderIds)
        {

            using (AuthContext db = new AuthContext())
            {
                var Odcount = 0;
                var ODdcount = 0;
                //int[] OrderIds = new int[] { 205927 };
                if (OrderIds != null && OrderIds.Any())
                {
                    List<OrderMaster> orderMDList = db.DbOrderMaster.Where(X => OrderIds.Contains(X.OrderId)).Include("orderDetails").ToList();
                    List<OrderDispatchedMaster> OrderDispatchedMasterList = db.OrderDispatchedMasters.Where(X => OrderIds.Contains(X.OrderId)).Include("orderDetails").ToList();

                    List<OrderMaster> UpdateorderMDList = new List<OrderMaster>();
                    List<OrderDispatchedMaster> UpdateDispatchedMasterList = new List<OrderDispatchedMaster>();

                    for (var i = 0; i < OrderIds.Count; i++)
                    {
                        Odcount++;
                        int OrderId = OrderIds[i];
                        OrderMaster orderMD = orderMDList.Where(c => c.OrderId == OrderId).FirstOrDefault();
                        double finalTaxAmount = 0;
                        double finalSGSTTaxAmount = 0;
                        double finalCGSTTaxAmount = 0;
                        double finalTotalTaxAmount = 0;
                        double finalCessTaxAmount = 0; //cess 
                        foreach (var odd in orderMD.orderDetails)
                        {
                            var items = db.itemMasters.Where(c => c.ItemId == odd.ItemId && c.WarehouseId == odd.WarehouseId).FirstOrDefault();
                            if (items == null)
                            {
                                break;
                            }
                            odd.TaxPercentage = items.TotalTaxPercentage;
                            if (odd.TaxPercentage >= 0)
                            {
                                odd.SGSTTaxPercentage = odd.TaxPercentage / 2;
                                odd.CGSTTaxPercentage = odd.TaxPercentage / 2;
                            }
                            //if there is cess for that item

                            if (items.TotalCessPercentage > 0)
                            {
                                odd.TotalCessPercentage = items.TotalCessPercentage;
                                double tempPercentagge = items.TotalCessPercentage + items.TotalTaxPercentage;
                                odd.AmtWithoutTaxDisc = ((100 * odd.UnitPrice * odd.qty) / (1 + tempPercentagge / 100)) / 100;
                                odd.AmtWithoutAfterTaxDisc = (100 * odd.AmtWithoutTaxDisc) / (100 + items.PramotionalDiscount);
                                odd.CessTaxAmount = (odd.AmtWithoutAfterTaxDisc * odd.TotalCessPercentage) / 100;
                            }

                            double tempPercentagge2 = items.TotalCessPercentage + items.TotalTaxPercentage;
                            odd.AmtWithoutTaxDisc = ((100 * odd.UnitPrice * odd.qty) / (1 + tempPercentagge2 / 100)) / 100;
                            odd.AmtWithoutAfterTaxDisc = (100 * odd.AmtWithoutTaxDisc) / (100 + items.PramotionalDiscount);
                            odd.TaxAmmount = (odd.AmtWithoutAfterTaxDisc * odd.TaxPercentage) / 100;
                            if (odd.TaxAmmount >= 0)
                            {
                                odd.SGSTTaxAmmount = odd.TaxAmmount / 2;
                                odd.CGSTTaxAmmount = odd.TaxAmmount / 2;
                            }
                            //for cess
                            if (odd.CessTaxAmount > 0)
                            {
                                double tempPercentagge3 = items.TotalCessPercentage + items.TotalTaxPercentage;
                                odd.AmtWithoutTaxDisc = ((100 * odd.UnitPrice * odd.qty) / (1 + tempPercentagge3 / 100)) / 100;
                                odd.AmtWithoutAfterTaxDisc = (100 * odd.AmtWithoutTaxDisc) / (100 + items.PramotionalDiscount);
                                odd.TotalAmountAfterTaxDisc = odd.AmtWithoutAfterTaxDisc + odd.CessTaxAmount + odd.TaxAmmount;
                            }
                            else
                            {
                                odd.TotalAmountAfterTaxDisc = odd.AmtWithoutAfterTaxDisc + odd.TaxAmmount;
                            }
                            double TaxAmmount = odd.TaxAmmount;

                            ////after 
                            if (odd.CessTaxAmount > 0)
                            {
                                finalCessTaxAmount = finalCessTaxAmount + odd.CessTaxAmount;
                                finalTaxAmount = finalTaxAmount + odd.TaxAmmount + odd.CessTaxAmount;
                            }
                            else
                            {
                                finalTaxAmount = finalTaxAmount + odd.TaxAmmount;
                            }

                            odd.UpdatedDate = indianTime;

                            finalSGSTTaxAmount = finalSGSTTaxAmount + odd.SGSTTaxAmmount;
                            finalCGSTTaxAmount = finalCGSTTaxAmount + odd.CGSTTaxAmmount;
                            finalTotalTaxAmount = finalTotalTaxAmount + odd.TotalAmountAfterTaxDisc;
                        }
                        orderMD.TaxAmount = System.Math.Round(finalTaxAmount, 2);
                        orderMD.SGSTTaxAmmount = System.Math.Round(finalSGSTTaxAmount, 2);
                        orderMD.CGSTTaxAmmount = System.Math.Round(finalCGSTTaxAmount, 2);
                        orderMD.UpdatedDate = indianTime;
                        UpdateorderMDList.Add(orderMD);
                    }

                    // for OrderDispatched
                    for (var ii = 0; ii < OrderIds.Count; ii++)
                    {
                        int OrderId = OrderIds[ii];
                        var orderlistD = OrderDispatchedMasterList.Where(X => X.OrderId == OrderId).FirstOrDefault();
                        if (orderlistD != null)
                        {
                            ODdcount++;
                            double finalTaxAmount = 0;
                            double finalSGSTTaxAmount = 0;
                            double finalCGSTTaxAmount = 0;
                            double finalTotalTaxAmount = 0;
                            double finalCessTaxAmount = 0; //cess 

                            foreach (var odd in orderlistD.orderDetails)
                            {
                                var items = db.itemMasters.Where(c => c.ItemId == odd.ItemId && c.WarehouseId == odd.WarehouseId).FirstOrDefault();
                                if (items == null)
                                {
                                    break;
                                }

                                odd.TaxPercentage = items.TotalTaxPercentage;
                                if (odd.TaxPercentage >= 0)
                                {
                                    odd.SGSTTaxPercentage = odd.TaxPercentage / 2;
                                    odd.CGSTTaxPercentage = odd.TaxPercentage / 2;
                                }
                                //if there is cess for that item

                                if (items.TotalCessPercentage > 0)
                                {
                                    odd.TotalCessPercentage = items.TotalCessPercentage;
                                    double tempPercentagge = items.TotalCessPercentage + items.TotalTaxPercentage;
                                    odd.AmtWithoutTaxDisc = ((100 * odd.UnitPrice * odd.qty) / (1 + tempPercentagge / 100)) / 100;
                                    odd.AmtWithoutAfterTaxDisc = (100 * odd.AmtWithoutTaxDisc) / (100 + items.PramotionalDiscount);
                                    odd.CessTaxAmount = (odd.AmtWithoutAfterTaxDisc * odd.TotalCessPercentage) / 100;
                                }

                                double tempPercentagge2 = items.TotalCessPercentage + items.TotalTaxPercentage;
                                odd.AmtWithoutTaxDisc = ((100 * odd.UnitPrice * odd.qty) / (1 + tempPercentagge2 / 100)) / 100;
                                odd.AmtWithoutAfterTaxDisc = (100 * odd.AmtWithoutTaxDisc) / (100 + items.PramotionalDiscount);
                                odd.TaxAmmount = (odd.AmtWithoutAfterTaxDisc * odd.TaxPercentage) / 100;
                                if (odd.TaxAmmount >= 0)
                                {
                                    odd.SGSTTaxAmmount = odd.TaxAmmount / 2;
                                    odd.CGSTTaxAmmount = odd.TaxAmmount / 2;
                                }
                                //for cess
                                if (odd.CessTaxAmount > 0)
                                {
                                    double tempPercentagge3 = items.TotalCessPercentage + items.TotalTaxPercentage;
                                    odd.AmtWithoutTaxDisc = ((100 * odd.UnitPrice * odd.qty) / (1 + tempPercentagge3 / 100)) / 100;
                                    odd.AmtWithoutAfterTaxDisc = (100 * odd.AmtWithoutTaxDisc) / (100 + items.PramotionalDiscount);
                                    odd.TotalAmountAfterTaxDisc = odd.AmtWithoutAfterTaxDisc + odd.CessTaxAmount + odd.TaxAmmount;
                                }
                                else
                                {
                                    odd.TotalAmountAfterTaxDisc = odd.AmtWithoutAfterTaxDisc + odd.TaxAmmount;
                                }
                                double TaxAmmount = odd.TaxAmmount;
                                ////after 
                                if (odd.CessTaxAmount > 0)
                                {
                                    finalCessTaxAmount = finalCessTaxAmount + odd.CessTaxAmount;
                                    finalTaxAmount = finalTaxAmount + odd.TaxAmmount + odd.CessTaxAmount;
                                }
                                else
                                {
                                    finalTaxAmount = finalTaxAmount + odd.TaxAmmount;
                                }

                                odd.UpdatedDate = indianTime;
                                finalSGSTTaxAmount = finalSGSTTaxAmount + odd.SGSTTaxAmmount;
                                finalCGSTTaxAmount = finalCGSTTaxAmount + odd.CGSTTaxAmmount;
                                finalTotalTaxAmount = finalTotalTaxAmount + odd.TotalAmountAfterTaxDisc;
                            }
                            orderlistD.TaxAmount = System.Math.Round(finalTaxAmount, 2);
                            orderlistD.SGSTTaxAmmount = System.Math.Round(finalSGSTTaxAmount, 2);
                            orderlistD.CGSTTaxAmmount = System.Math.Round(finalCGSTTaxAmount, 2);
                            orderlistD.UpdatedDate = indianTime;
                            UpdateDispatchedMasterList.Add(orderlistD);

                        }
                    }
                    foreach (var order in UpdateorderMDList)
                    {
                        db.Entry(order).State = EntityState.Modified;
                    }

                    foreach (var item in UpdateDispatchedMasterList)
                    {
                        db.Entry(item).State = EntityState.Modified;
                    }
                }
                if (db.Commit() > 0)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, true);
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.OK, false);
                };
            }
        }
        #endregion



        #region
        [Route("EpayLaterInfo")]
        [AllowAnonymous]
        [AcceptVerbs("Get")]
        public HttpResponseMessage save()
        {
            using (var context = new AuthContext())
            {

                var Epaycust = context.Customers.Where(x => x.Active == true).ToList();
                // Epaycust.ForEach(x => x.CustomerId);

                foreach (Customer c in Epaycust)
                {
                    string EPAYURI = ConfigurationManager.AppSettings["EPayUrl"];
                    string AccessToken = ConfigurationManager.AppSettings["EPayAccessToken"];
                    if (!string.IsNullOrEmpty(EPAYURI))
                    {
                        //int cid = 20232; /*"Authorization", "Bearer " + AccessToken*/
                        //string AccessToken = "secret_7a4d4613-6442-4b4d-aa1a-e0dd79424a43";
                        //string userAuthenticationURI = "https://api-sandbox.epaylater.in:443/user/marketplaceCustomer/" + c.CustomerId + "/creditLimit";
                        string userAuthenticationURI = EPAYURI + c.Skcode + "/creditLimit";

                        if (!string.IsNullOrEmpty(userAuthenticationURI))
                        {
                            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(userAuthenticationURI);
                            request.Method = "GET";
                            request.ContentType = "application/json";
                            request.Headers.Add("Authorization", "Bearer " + AccessToken);

                            using (HttpWebResponse resp = (HttpWebResponse)request.GetResponse())
                            {
                                StreamReader reader = new StreamReader(resp.GetResponseStream());
                                var dato = reader.ReadToEnd();
                                epayltrDTO result = JsonConvert.DeserializeObject<epayltrDTO>(dato);

                                using (AuthContext db = new AuthContext())
                                {
                                    Customer w = new Customer();
                                    var cust = db.Customers.Where(q => q.CustomerId == result.marketplaceCustomerId).SingleOrDefault();

                                    cust.UdharLimitRemaining = Convert.ToInt32(result.availableCreditLimit);
                                    //db.Customers.Attach(cust);
                                    db.Entry(cust).State = EntityState.Modified;
                                    db.Commit();
                                }
                            }
                        }
                    }
                }
                return null;
            }
        }


        #endregion


        #region App Order punch Offer Billdiscount(Percantage/WalletPoint) intigrated By Harry (13/06/2019)
        /// <summary>
        /// Orderpunch Offer Billdiscount(Percantage/WalletPoint) intigrated By Harry (13/06/2019)
        /// </summary>
        /// <param name="sc"></param>
        /// <returns></returns>

        #endregion


        #region Change Order Flow

        /// <summary>
        ///  New Order Place API By Atish -- 4/July/2019
        /// </summary>
        /// <param name="sc"></param>
        /// <returns></returns>
        [Route("V6")]
        [AllowAnonymous]
        [HttpPost]
        public async Task<HttpResponseMessage> PostOrderV6(ShoppingCart sc)
        {
            using (var context = new AuthContext())
            {
                var placeOrderResponse = new Model.PlaceOrder.PlaceOrderResponse();

                #region OverDueAmount Customer/Sales
                MongoDbHelper<UdharOverDueDayValidation> UdharOverDueDay = new MongoDbHelper<UdharOverDueDayValidation>();
                var DueAmt = UdharOverDueDay.GetAll();
                if (DueAmt != null && DueAmt.Any() && DueAmt.Max(x => x.MaxOverDueDay) > 0)
                {
                    var minDay = DueAmt.Min(x => x.MinOverDueDay);
                    var maxDay = DueAmt.Max(x => x.MaxOverDueDay);
                    var Customer = context.Customers.FirstOrDefault(x => x.CustomerId == sc.CustomerId);
                    if (Customer.UdharDueDays > 0)
                    {
                        CheckDueAmtDc UDData = new CheckDueAmtDc();

                        var param1 = new SqlParameter("@CustomerId", sc.CustomerId);
                        UDData = context.Database.SqlQuery<CheckDueAmtDc>("Exec CheckDueAmt @CustomerId ", param1).FirstOrDefault();
                        if (UDData != null && UDData.Amount >= 1)
                        {
                            if (sc.SalesPersonId > 0 && sc.APPType == "SalesApp" && Customer.UdharDueDays > minDay)
                            {
                                placeOrderResponse.IsSuccess = false;
                                placeOrderResponse.Message = "Please request the customer to clear the Direct Udhaar overdue amount of Rs. " + UDData.Amount + " to continue placing a new order.";
                                placeOrderResponse.cart = null;
                                return Request.CreateResponse(HttpStatusCode.OK, placeOrderResponse);
                            }
                            if (sc.CustomerId > 0 && sc.APPType == "Retailer" && Customer.UdharDueDays > maxDay)
                            {
                                placeOrderResponse.IsSuccess = false;
                                placeOrderResponse.Message = "Kindly clear your Direct Udhaar overdue amount of Rs." + UDData.Amount + " before placing the order.";
                                placeOrderResponse.cart = null;
                                return Request.CreateResponse(HttpStatusCode.OK, placeOrderResponse);
                            }

                            if (sc.CustomerId > 0 && sc.APPType == "Retailer" && Customer.UdharDueDays > minDay && Customer.UdharDueDays < maxDay)
                            {
                                if (sc.paymentThrough.Split(',').ToList().Any(x => x == "cash" || x == "DirectUdhar" || x == "rtgs/neft"))
                                {
                                    placeOrderResponse.IsSuccess = false;
                                    placeOrderResponse.Message = "Kindly clear your Direct Udhaar overdue amount of Rs." + UDData.Amount + " to enable Udhaar and COD mode of payment.";
                                    placeOrderResponse.cart = null;
                                    return Request.CreateResponse(HttpStatusCode.OK, placeOrderResponse);
                                }

                            }

                        }
                    }

                }
                #endregion

                #region InActive SalesPerson Request
                if (sc.SalesPersonId.Value > 0 && context.Peoples.Any(x => x.PeopleID == sc.SalesPersonId.Value && x.Active == false))
                {
                    placeOrderResponse.IsSuccess = false;
                    placeOrderResponse.Message = "Your account is Inactive, you can't place order.";
                    placeOrderResponse.cart = null;
                    return Request.CreateResponse(HttpStatusCode.OK, placeOrderResponse);
                }
                #endregion

                #region Handle Single Customer Request
                if (retailercustomerIds.Any(x => x == sc.CustomerId.Value))
                {
                    placeOrderResponse.IsSuccess = false;
                    placeOrderResponse.Message = "आप का आर्डर प्रोसेस मे है प्लीज प्रतिक्षा करे।";
                    placeOrderResponse.cart = null;
                    return Request.CreateResponse(HttpStatusCode.OK, placeOrderResponse);
                }
                else
                {
                    retailercustomerIds.Add(sc.CustomerId.Value);
                }


                BlockGullakAmount blockGullakAmount = null;
                MongoDbHelper<BlockGullakAmount> mongoDbHelper = new MongoDbHelper<BlockGullakAmount>();
                if (sc.paymentThrough == "Gullak" && sc.GulkAmount > 0)
                {
                    blockGullakAmount = new BlockGullakAmount
                    {
                        CreatedDate = DateTime.Now,
                        CustomerId = sc.CustomerId.Value,
                        Guid = Guid.NewGuid().ToString(),
                        Amount = sc.GulkAmount,
                        IsActive = true
                    };
                    await mongoDbHelper.InsertAsync(blockGullakAmount);
                    sc.BlockGullakAmountGuid = blockGullakAmount.Guid;
                }
                #endregion


                #region PaylaterLimitHandling
                BlockPayLaterAmount blockPayLaterAmount = null;
                var paymentThroughs = sc.paymentThrough.Split(',').ToList();
                long PayLaterStoreId = 0;
                if (sc.APPType == "SalesApp")
                {
                    if (sc.PaylaterAmount > 0 && paymentThroughs.Any(x => x.ToLower() == "paylater"))
                    {
                        List<long> storeids = new List<long>();
                        RetailerAppManager retailerAppManager = new RetailerAppManager();
                        List<AngularJSAuthentication.DataContracts.Masters.Store.StoreCategorySubCategoryBrand> StoreCategorySubCategoryBrands = retailerAppManager.GetStoreWithDetail();
                        var itemIds = sc.itemDetails.Select(x => x.ItemId).Distinct().ToList();
                        List<AngularJSAuthentication.Model.ItemMaster> itemMastersListt = new List<AngularJSAuthentication.Model.ItemMaster>();
                        if (itemIds != null && itemIds.Any())
                        {
                            itemMastersListt = context.itemMasters.Where(x => itemIds.Contains(x.ItemId)).Distinct().ToList();
                        }

                        foreach (var i in sc.itemDetails.Where(x => x.qty > 0).ToList())
                        {
                            var item = itemMastersListt.Where(x => x.ItemId == i.ItemId && x.WarehouseId == i.WarehouseId).FirstOrDefault();
                            if (StoreCategorySubCategoryBrands != null && StoreCategorySubCategoryBrands.Any(x => x.Categoryid == item.Categoryid && x.SubCategoryId == item.SubCategoryId && x.BrandId == item.SubsubCategoryid))
                            {
                                var store = StoreCategorySubCategoryBrands.First(x => x.Categoryid == item.Categoryid && x.SubCategoryId == item.SubCategoryId && x.BrandId == item.SubsubCategoryid);
                                if (store.StoreId > 0)
                                {
                                    storeids.Add(store.StoreId);
                                }
                            }
                        }
                        if (storeids.Any() && storeids.Distinct().Count() == 1)
                        {
                            PayLaterStoreId = storeids.FirstOrDefault();
                            MongoDbHelper<BlockPayLaterAmount> mongoDbHelperPaylater = new MongoDbHelper<BlockPayLaterAmount>();
                            blockPayLaterAmount = new BlockPayLaterAmount
                            {
                                CreatedDate = DateTime.Now,
                                CustomerId = sc.CustomerId.Value,
                                Guid = Guid.NewGuid().ToString(),
                                Amount = Math.Round(sc.PaylaterAmount, 0, MidpointRounding.AwayFromZero),
                                IsActive = true,
                                StoreId = PayLaterStoreId
                            };
                            await mongoDbHelperPaylater.InsertAsync(blockPayLaterAmount);
                            sc.BlockPayLaterAmountGuid = blockPayLaterAmount.Guid;

                        }

                    }
                }
                #endregion

                #region Get Customer COD Limit
                CustomerCODLimitDc codLimitDc = await GetCustomerCODLimit(sc.CustomerId.Value);
                if (codLimitDc != null && codLimitDc.CODLimit > 0 && codLimitDc.IsActive == true && codLimitDc.IsCustomCODLimit == true)
                {
                    if ((codLimitDc.AvailableCODLimit - sc.CODAmount) < 0)
                    {
                        placeOrderResponse.IsSuccess = false;
                        placeOrderResponse.Message = "COD Limit Exceed";
                        placeOrderResponse.cart = null;
                        retailercustomerIds.Remove(sc.CustomerId.Value);
                        return Request.CreateResponse(HttpStatusCode.OK, placeOrderResponse);
                    }
                }
                #endregion

                try
                {
                    placeOrderResponse = await context.PushOrderMasterV6(sc);//Post order

                    // insert order in mongo gamification work
                    if (placeOrderResponse.IsSuccess)
                    {
                        var CustomerId = placeOrderResponse.OrderMaster.CustomerId;
                        var OrderId = placeOrderResponse.OrderMaster.OrderId;
                        var date = DateTime.Now;
                        InsertCustomerBucket(CustomerId, OrderId, date);
                    }


                }
                catch (Exception ex)
                {
                    throw ex;
                }
                finally
                {
                    if (retailercustomerIds.Any(x => x == sc.CustomerId.Value))
                    {
                        retailercustomerIds.Remove(sc.CustomerId.Value);
                    }
                    if (sc.paymentThrough == "Gullak" && sc.GulkAmount > 0)
                    {
                        var cartPredicate = PredicateBuilder.New<BlockGullakAmount>(x => x.CustomerId == sc.CustomerId && x.Guid == blockGullakAmount.Guid);
                        var blockGullakAmountdb = mongoDbHelper.Select(cartPredicate).FirstOrDefault();
                        blockGullakAmountdb.IsActive = false;
                        var result = await mongoDbHelper.ReplaceAsync(blockGullakAmountdb.Id, blockGullakAmountdb);
                    }
                    if (sc.PaylaterAmount > 0 && paymentThroughs.Any(x => x.ToLower() == "paylater"))
                    {
                        MongoDbHelper<BlockPayLaterAmount> mongoPayLaterDbHelper = new MongoDbHelper<BlockPayLaterAmount>();
                        var cartPredicate = PredicateBuilder.New<BlockPayLaterAmount>(x => x.CustomerId == sc.CustomerId && x.Guid == blockPayLaterAmount.Guid && x.StoreId == PayLaterStoreId);
                        var blockPayLaterAmountdb = mongoPayLaterDbHelper.Select(cartPredicate).FirstOrDefault();
                        blockPayLaterAmountdb.IsActive = false;
                        var result = await mongoPayLaterDbHelper.ReplaceAsync(blockPayLaterAmountdb.Id, blockPayLaterAmountdb);
                    }
                }
                return Request.CreateResponse(HttpStatusCode.OK, placeOrderResponse);
            }
        }

        [Route("InsertWarehouseWalletHumbredPer")]
        [AllowAnonymous]
        [HttpGet]
        public async Task<bool> InsertWarehouseWalletHumbredPer(int WId)
        {
            MongoDbHelper<AngularJSAuthentication.Model.CustomerShoppingCart.WalletHundredPercentUse> mongoDbHelper_W = new MongoDbHelper<AngularJSAuthentication.Model.CustomerShoppingCart.WalletHundredPercentUse>(); //!x.GeneratedOrderId.HasValue
            mongoDbHelper_W.Insert(new WalletHundredPercentUse { WarehouseId = WId }, "WalletHundredPercentUse");
            return true;
        }

        [Route("RemoveCustomerForV6")]
        [AllowAnonymous]
        public bool RemoveCustomerForV6(int customerId, bool clearGullakBlockAmt)
        {
            if (retailercustomerIds.Any(x => x == customerId))
            {
                retailercustomerIds.Remove(customerId);
            }
            if (clearGullakBlockAmt)
            {
                MongoDbHelper<BlockGullakAmount> mongoDbHelper = new MongoDbHelper<BlockGullakAmount>();
                var cartPredicate = PredicateBuilder.New<BlockGullakAmount>(x => x.CustomerId == customerId && x.IsActive);
                var blockGullakAmountdbs = mongoDbHelper.Select(cartPredicate).ToList();
                foreach (var blockGullakAmountdb in blockGullakAmountdbs)
                {
                    blockGullakAmountdb.IsActive = false;
                    var result = mongoDbHelper.Replace(blockGullakAmountdb.Id, blockGullakAmountdb);
                }

            }
            return true;
        }

        [Route("PlaceComboOrder")]
        [AllowAnonymous]
        [HttpPost]
        public async Task<Model.PlaceOrder.PlaceOrderResponse> PlaceComboOrder(ComboOrderDc comboOrderDc)
        {
            OrderMaster objOrderMaster = new OrderMaster();
            var placeOrderResponse = new Model.PlaceOrder.PlaceOrderResponse();
            if (comboOrderDc != null)
            {
                if (comboOrderDc.CustomerId == 0)
                {
                    placeOrderResponse.IsSuccess = false;
                    placeOrderResponse.Message = "Customer not found!";
                    return placeOrderResponse;
                }

                if (comboOrderDc.ComboOrderItemDcs == null || !comboOrderDc.ComboOrderItemDcs.Any())
                {
                    placeOrderResponse.IsSuccess = false;
                    placeOrderResponse.Message = "Please select atleast one combo";
                    return placeOrderResponse;
                }
                using (var context = new AuthContext())
                {
                    objOrderMaster.orderDetails = new List<OrderDetails>();
                    var cust = context.Customers.FirstOrDefault(c => c.CustomerId == comboOrderDc.CustomerId);
                    var warehouse = context.Warehouses.Where(w => w.WarehouseId == cust.Warehouseid).Select(c => c).SingleOrDefault();
                    var combolist = new MongoDbHelper<Combo>();
                    var comboids = comboOrderDc.ComboOrderItemDcs.Select(x => new ObjectId(x.Id)).ToList();
                    var combos = combolist.Select(x => comboids.Contains(x.Id)).ToList();


                    #region Prepair order 
                    objOrderMaster.CompanyId = warehouse.CompanyId;
                    objOrderMaster.WarehouseId = warehouse.WarehouseId;
                    objOrderMaster.WarehouseName = warehouse.WarehouseName;
                    objOrderMaster.CustomerCategoryId = 2;
                    objOrderMaster.Status = "Payment Pending";
                    objOrderMaster.CustomerName = cust.Name;
                    objOrderMaster.ShopName = cust.ShopName;
                    objOrderMaster.LandMark = cust.LandMark;
                    objOrderMaster.Skcode = cust.Skcode;
                    objOrderMaster.Tin_No = cust.RefNo;
                    objOrderMaster.CustomerType = cust.CustomerType;
                    objOrderMaster.CustomerId = cust.CustomerId;
                    objOrderMaster.CityId = cust.Cityid;
                    objOrderMaster.Customerphonenum = (cust.Mobile);
                    // MRP-Actual Price
                    // objOrderMaster.Savingamount = System.Math.Round(sc.Savingamount, 2);
                    objOrderMaster.ClusterId = Convert.ToInt32(cust.ClusterId);
                    // objOrderMaster.OnlineServiceTax = sc.OnlineServiceTax;
                    var clstr = context.Clusters.Where(x => x.ClusterId == cust.ClusterId).SingleOrDefault();
                    if (clstr != null)
                    {
                        objOrderMaster.ClusterName = clstr.ClusterName;
                    }
                    //People p = new People();
                    //p = context.Peoples.Where(x => x.PeopleID == cust.ExecutiveId && x.Deleted == false && x.Active == true).SingleOrDefault();

                    //if (p != null)
                    //{
                    //    objOrderMaster.SalesMobile = p.Mobile;
                    //    objOrderMaster.SalesPersonId = p.PeopleID;
                    //    objOrderMaster.SalesPerson = p.PeopleFirstName + " " + p.PeopleLastName;
                    //    objOrderMaster.OrderTakenSalesPersonId = 0;
                    //    objOrderMaster.OrderTakenSalesPerson = "Self";
                    //}

                    //else
                    //{
                    #region old code 
                    //objOrderMaster.OrderTakenSalesPersonId = 0;
                    //objOrderMaster.OrderTakenSalesPerson = "Self";
                    #endregion
                    //}

                    AngularJSAuthentication.API.Managers.CRM.CRMManager mg = new AngularJSAuthentication.API.Managers.CRM.CRMManager();
                    objOrderMaster.IsDigitalOrder = await mg.IsDigitalCustomer(cust.Skcode);

                    #region new code for ordertakenbyid
                    People p = new People();
                    if (comboOrderDc.ExecutiveId > 0)
                    {
                        p = context.Peoples.Where(x => x.PeopleID == comboOrderDc.ExecutiveId).FirstOrDefault();
                        if (p != null)
                        {
                            objOrderMaster.OrderTakenSalesPersonId = p.PeopleID;
                            objOrderMaster.OrderTakenSalesPerson = p.PeopleFirstName + " " + p.PeopleLastName;
                        }
                    }
                    else
                    {
                        objOrderMaster.OrderTakenSalesPersonId = 0;
                        objOrderMaster.OrderTakenSalesPerson = "Self";
                    }
                    #endregion

                    //code for get all channels
                    var CustomerChannels = context.CustomerChannelMappings.Where(x => x.CustomerId == comboOrderDc.CustomerId && x.IsActive == true).ToList();

                    objOrderMaster.BillingAddress = cust.ShippingAddress;
                    objOrderMaster.ShippingAddress = cust.ShippingAddress;
                    objOrderMaster.active = true;
                    objOrderMaster.CreatedDate = indianTime;
                    objOrderMaster.Lat = cust.lat;
                    objOrderMaster.Lng = cust.lg;
                    objOrderMaster.IsPrimeCustomer = cust.IsPrimeCustomer;
                    objOrderMaster.userid = 1;

                    //code comment 
                    //if (indianTime.Hour > 16)
                    //{
                    //    objOrderMaster.Deliverydate = indianTime.AddDays(2);
                    //}
                    //else
                    //{
                    //    objOrderMaster.Deliverydate = indianTime.AddDays(1);
                    //}
                    //
                    objOrderMaster.orderProcess = true;
                    objOrderMaster.accountProcess = true;
                    objOrderMaster.chequeProcess = false;
                    objOrderMaster.Savingamount = 0;
                    objOrderMaster.OnlineServiceTax = 0;
                    objOrderMaster.TCSAmount = 0;
                    objOrderMaster.IsFirstOrder = false;
                    objOrderMaster.Deliverydate = DateTime.Now;
                    objOrderMaster.ReDispatchCount = 0;
                    objOrderMaster.DivisionId = 0;
                    objOrderMaster.ShortAmount = 0;
                    objOrderMaster.UpdatedDate = indianTime;
                    objOrderMaster.Deleted = false;
                    RetailerAppManager retailerAppManager = new RetailerAppManager();
                    List<DataContracts.Masters.Store.StoreCategorySubCategoryBrand> StoreCategorySubCategoryBrands = retailerAppManager.GetStoreWithDetail();
                    List<DataContracts.Masters.Store.ClusterStoreExecutiveDc> clusterStoreExecutiveDcs = retailerAppManager.GetStoreClusterExecutiveDetail();

                    #endregion

                    foreach (var combo in combos)
                    {
                        var purchasecombo = comboOrderDc.ComboOrderItemDcs.FirstOrDefault(x => x.Id == combo.Id.ToString());
                        var itemIds = combo.ComboItemList.Select(x => x.ItemId).ToList();
                        var itemMastersList = context.itemMasters.Where(x => itemIds.Contains(x.ItemId) && x.WarehouseId == cust.Warehouseid).ToList();
                        foreach (var items in itemMastersList)
                        {
                            var comboItem = combo.ComboItemList.FirstOrDefault(x => x.ItemId == items.ItemId);
                            double UnitPrice = comboItem.UnitPrice - ((comboItem.UnitPrice * Convert.ToDouble(comboItem.Parcentage)) / 100);
                            OrderDetails od = new OrderDetails();
                            if (StoreCategorySubCategoryBrands != null && StoreCategorySubCategoryBrands.Any(x => x.Categoryid == items.Categoryid && x.SubCategoryId == items.SubCategoryId && x.BrandId == items.SubsubCategoryid))
                            {
                                var store = StoreCategorySubCategoryBrands.First(x => x.Categoryid == items.Categoryid && x.SubCategoryId == items.SubCategoryId && x.BrandId == items.SubsubCategoryid);
                                od.StoreId = store.StoreId;
                                if (CustomerChannels != null && CustomerChannels.Any(x => x.StoreId == od.StoreId))
                                    od.ChannelMasterId = CustomerChannels.FirstOrDefault(x => x.StoreId == store.StoreId).ChannelMasterId;

                                if (store.StoreId > 0 && clusterStoreExecutiveDcs != null && clusterStoreExecutiveDcs.Any(a => a.StoreId == store.StoreId && a.ClusterId == cust.ClusterId) && CustomerChannels != null)
                                {
                                    if (CustomerChannels.Any(x => x.StoreId == od.StoreId))
                                    {
                                        var clusterStoreExecutiveDc = clusterStoreExecutiveDcs.FirstOrDefault(a => a.StoreId == store.StoreId && a.ClusterId == cust.ClusterId && a.ChannelMasterId == od.ChannelMasterId);
                                        if (clusterStoreExecutiveDc != null)
                                        {
                                            od.ExecutiveId = clusterStoreExecutiveDc.ExecutiveId;
                                            od.ExecutiveName = clusterStoreExecutiveDc.ExecutiveName;
                                        }

                                    }

                                }
                            }
                            else
                            {
                                od.StoreId = 0;
                                od.ExecutiveId = 0;
                                od.ExecutiveName = "";
                            }
                            od.CustomerId = cust.CustomerId;
                            od.CustomerName = cust.Name;
                            od.CityId = cust.Cityid;
                            od.Mobile = cust.Mobile;
                            od.OrderDate = indianTime;
                            od.Status = cust.Active ? "Pending" : "Inactive";
                            od.CompanyId = warehouse.CompanyId;
                            od.WarehouseId = warehouse.WarehouseId;
                            od.WarehouseName = warehouse.WarehouseName;
                            od.NetPurchasePrice = items.NetPurchasePrice + ((items.NetPurchasePrice * items.TotalTaxPercentage) / 100);
                            od.ItemId = items.ItemId;
                            od.ItemMultiMRPId = items.ItemMultiMRPId;
                            od.Itempic = items.LogoUrl;
                            od.itemname = items.itemname;
                            od.SupplierName = items.SupplierName;
                            od.SellingUnitName = items.SellingUnitName;
                            od.CategoryName = items.CategoryName;
                            od.SubsubcategoryName = items.SubsubcategoryName;
                            od.SubcategoryName = items.SubcategoryName;
                            od.SellingSku = items.SellingSku;
                            od.City = items.CityName;
                            od.itemcode = items.itemcode;
                            od.HSNCode = items.HSNCode;
                            od.itemNumber = items.Number;
                            od.Barcode = items.itemcode;
                            od.ActualUnitPrice = items.UnitPrice;
                            od.UnitPrice = UnitPrice;
                            od.price = items.price;
                            od.MinOrderQty = items.MinOrderQty;
                            od.MinOrderQtyPrice = (od.MinOrderQty * UnitPrice);
                            od.qty = Convert.ToInt32(comboItem.Qty) * purchasecombo.qty;
                            od.SizePerUnit = items.SizePerUnit;
                            od.TaxPercentage = items.TotalTaxPercentage;
                            if (od.TaxPercentage >= 0)
                            {
                                od.SGSTTaxPercentage = od.TaxPercentage / 2;
                                od.CGSTTaxPercentage = od.TaxPercentage / 2;
                            }
                            od.Noqty = od.qty; // for total qty (no of items)    
                            od.TotalAmt = System.Math.Round(od.UnitPrice * od.qty, 2);

                            if (items.TotalCessPercentage > 0)
                            {
                                od.TotalCessPercentage = items.TotalCessPercentage;
                                double tempPercentagge = items.TotalCessPercentage + items.TotalTaxPercentage;

                                od.AmtWithoutTaxDisc = ((100 * od.UnitPrice * od.qty) / (1 + tempPercentagge / 100)) / 100;


                                od.AmtWithoutAfterTaxDisc = (100 * od.AmtWithoutTaxDisc) / (100 + items.PramotionalDiscount);
                                od.CessTaxAmount = (od.AmtWithoutAfterTaxDisc * od.TotalCessPercentage) / 100;
                            }


                            double tempPercentagge2 = items.TotalCessPercentage + items.TotalTaxPercentage;

                            od.AmtWithoutTaxDisc = ((100 * od.UnitPrice * od.qty) / (1 + tempPercentagge2 / 100)) / 100;
                            od.AmtWithoutAfterTaxDisc = (100 * od.AmtWithoutTaxDisc) / (100 + items.PramotionalDiscount);
                            od.TaxAmmount = (od.AmtWithoutAfterTaxDisc * od.TaxPercentage) / 100;
                            if (od.TaxAmmount >= 0)
                            {
                                od.SGSTTaxAmmount = od.TaxAmmount / 2;
                                od.CGSTTaxAmmount = od.TaxAmmount / 2;
                            }
                            //for cess
                            if (od.CessTaxAmount > 0)
                            {
                                double tempPercentagge3 = items.TotalCessPercentage + items.TotalTaxPercentage;
                                //double temp = od.TaxPercentage + od.TotalCessPercentage;
                                od.AmtWithoutTaxDisc = ((100 * od.UnitPrice * od.qty) / (1 + tempPercentagge3 / 100)) / 100;
                                od.AmtWithoutAfterTaxDisc = (100 * od.AmtWithoutTaxDisc) / (100 + items.PramotionalDiscount);
                                od.TotalAmountAfterTaxDisc = od.AmtWithoutAfterTaxDisc + od.CessTaxAmount + od.TaxAmmount;
                            }
                            else
                            {
                                od.TotalAmountAfterTaxDisc = od.AmtWithoutAfterTaxDisc + od.TaxAmmount;
                            }
                            od.DiscountPercentage = 0;// items.PramotionalDiscount;
                            od.DiscountAmmount = 0;// (od.NetAmmount * items.PramotionalDiscount) / 100;
                            double DiscountAmmount = od.DiscountAmmount;
                            double NetAmtAfterDis = (od.NetAmmount - DiscountAmmount);
                            od.NetAmtAfterDis = (od.NetAmmount - DiscountAmmount);
                            double TaxAmmount = od.TaxAmmount;
                            od.Purchaseprice = items.PurchasePrice;
                            od.CreatedDate = indianTime;
                            od.UpdatedDate = indianTime;
                            od.Deleted = false;

                            //////////////////////////////////////////////////////////////////////////////////////////////

                            /// Dream Point Logic && Margin Point relogic from 22April2019
                            int? MP, PP;
                            double xPoint = 0;
                            xPoint = xPointValue * 10; //Customer (0.2 * 10=1)

                            if (items.promoPerItems.Equals(null) && items.promoPerItems == null)
                            {
                                PP = 0;
                            }
                            else
                            {
                                PP = items.promoPerItems;
                            }
                            if (items.marginPoint.Equals(null) && items.promoPerItems == null)
                            {
                                MP = 0;
                            }
                            else
                            {
                                double WithTaxNetPurchasePrice = Math.Round(items.NetPurchasePrice * (1 + (items.TotalTaxPercentage / 100)), 3);//With tax
                                MP = Convert.ToInt32((items.UnitPrice - WithTaxNetPurchasePrice) * xPoint); // (UnitPrice-NPP withtax) * By xpoint 
                            }
                            if (PP > 0 && MP > 0)
                            {
                                int? PP_MP = PP + MP;
                                items.dreamPoint = PP_MP;
                            }
                            else if (MP > 0)
                            {
                                items.dreamPoint = MP;
                            }
                            else if (PP > 0)
                            {
                                items.dreamPoint = PP;
                            }
                            else
                            {
                                items.dreamPoint = 0;
                            }

                            od.marginPoint = items.dreamPoint * od.qty;//dp point multiplyby order qty

                            objOrderMaster.orderDetails.Add(od);
                        }

                        combo.TotalOrder += 1;
                        combo.TotalQty += purchasecombo.qty;
                        if (combo.TotalQty > combo.SellQty)
                        {
                            placeOrderResponse.IsSuccess = false;
                            placeOrderResponse.Message = combo.ComboName + " Is Outof stock.";
                            return placeOrderResponse;
                        }
                    }


                    var rewardpoint = (double)objOrderMaster.orderDetails.Sum(x => x.marginPoint);


                    objOrderMaster.TotalAmount = System.Math.Round(objOrderMaster.orderDetails.Sum(x => x.TotalAmt));
                    objOrderMaster.TaxAmount = System.Math.Round(objOrderMaster.orderDetails.Sum(x => x.TaxAmmount + x.CessTaxAmount), 2);
                    objOrderMaster.SGSTTaxAmmount = System.Math.Round(objOrderMaster.orderDetails.Sum(x => x.SGSTTaxAmmount), 2);
                    objOrderMaster.CGSTTaxAmmount = System.Math.Round(objOrderMaster.orderDetails.Sum(x => x.CGSTTaxAmmount), 2);
                    objOrderMaster.GrossAmount = System.Math.Round(objOrderMaster.TotalAmount, 0);
                    objOrderMaster.DiscountAmount = 0;//System.Math.Round(objOrderMaster.orderDetails.Sum(x => x.TotalAmountAfterTaxDisc), 2);

                    //add cluster to ordermaster
                    objOrderMaster.ClusterId = cust.ClusterId ?? 0;
                    objOrderMaster.ClusterName = cust.ClusterName;
                    objOrderMaster.paymentMode = "COD";
                    objOrderMaster.Status = "Pending";
                    objOrderMaster.Status = cust.Active && clstr != null && clstr.Active ? objOrderMaster.Status : "Inactive";
                    objOrderMaster.OrderType = 2;
                    var deliveryCharges = context.DeliveryChargeDb.Where(x => x.WarehouseId == warehouse.WarehouseId && x.isDeleted == false).ToList();
                    double DeliveryAmount = 0;
                    var storeIds = objOrderMaster.orderDetails.Where(x => x.qty > 0).Select(x => x.StoreId).Distinct().ToList();
                    if (deliveryCharges != null && deliveryCharges.Any(x => x.max_Amount >= objOrderMaster.TotalAmount && x.min_Amount <= objOrderMaster.TotalAmount))
                    {
                        if (storeIds.All(x => x == storeIds.Max(y => y))
                            && deliveryCharges.Any(x => (x.storeId.HasValue && storeIds.Contains(x.storeId.Value)) && x.max_Amount >= objOrderMaster.TotalAmount && x.min_Amount <= objOrderMaster.TotalAmount)
                            )
                            DeliveryAmount = Convert.ToDouble(deliveryCharges.Where(x => (x.storeId.HasValue && storeIds.Contains(x.storeId.Value)) && x.max_Amount >= objOrderMaster.TotalAmount && x.min_Amount <= objOrderMaster.TotalAmount).Max(x => x.del_Charge));
                        else
                            DeliveryAmount = Convert.ToDouble(deliveryCharges.Where(x => (!x.storeId.HasValue || x.storeId.Value == 0) && x.max_Amount >= objOrderMaster.TotalAmount && x.min_Amount <= objOrderMaster.TotalAmount).Max(x => x.del_Charge));

                    }

                    objOrderMaster.deliveryCharge = DeliveryAmount;

                    objOrderMaster.TotalAmount += DeliveryAmount;
                    objOrderMaster.GrossAmount += DeliveryAmount;

                    //#region etadatenewcode
                    //RetailerAppManager retailerApp = new RetailerAppManager();
                    //if (objOrderMaster.IsDigitalOrder.Value)
                    //{
                    //    objOrderMaster.Deliverydate = DateTime.Now.AddDays(1);
                    //}
                    //else
                    //{
                    //    var orderid = 0;
                    //    using (var AuthContext = new AuthContext())
                    //    {
                    //        orderid = AuthContext.DbOrderMaster.Where(x => x.CustomerId == cust.CustomerId).OrderByDescending(x => x.CreatedDate).Select(x => x.OrderId).FirstOrDefault();
                    //    }
                    //    List<AngularJSAuthentication.DataContracts.External.NextETADate> NextETADate = null;
                    //    NextETADate = retailerApp.GetNextETADate(objOrderMaster.WarehouseId, orderid);
                    //    if (NextETADate != null)
                    //    {
                    //        objOrderMaster.Deliverydate = NextETADate.Min(x => x.NextDeliveryDate);
                    //    }
                    //}
                    //#endregion

                    context.DbOrderMaster.Add(objOrderMaster);
                    context.Commit();

                    if (objOrderMaster.OrderId != 0)
                    {
                        #region etadatenewcode
                        RetailerAppManager retailerApp = new RetailerAppManager();
                        if (objOrderMaster.IsDigitalOrder.Value)
                        {
                            objOrderMaster.Deliverydate = DateTime.Now.AddDays(1);
                        }
                        else
                        {
                            var orderid = objOrderMaster.OrderId;
                            List<AngularJSAuthentication.DataContracts.External.NextETADate> NextETADate = null;
                            NextETADate = retailerApp.GetNextETADate(objOrderMaster.WarehouseId, orderid);
                            if (NextETADate != null)
                            {
                                objOrderMaster.Deliverydate = NextETADate.Min(x => x.NextDeliveryDate);
                            }
                        }
                        #endregion

                        placeOrderResponse.IsSuccess = true;
                        context.PaymentResponseRetailerAppDb.Add(new PaymentResponseRetailerApp
                        {
                            amount = Math.Round(objOrderMaster.TotalAmount, 0),
                            CreatedDate = indianTime,
                            currencyCode = "INR",
                            OrderId = objOrderMaster.OrderId,
                            PaymentFrom = "Cash",
                            status = "Success",
                            statusDesc = "Order Place",
                            UpdatedDate = indianTime,
                            IsRefund = false
                        });

                        string Borderid = Convert.ToString(objOrderMaster.OrderId);
                        string BorderCodeId = Borderid.PadLeft(11, '0');
                        temOrderQBcode code = GetBarcode(BorderCodeId);
                        objOrderMaster.InvoiceBarcodeImage = code.BarcodeImage;

                        context.Entry(objOrderMaster).State = EntityState.Modified;
                        context.Commit();
                    }

                    if (placeOrderResponse.IsSuccess && objOrderMaster.OrderId > 0)
                    {
                        foreach (var combo in combos)
                        {
                            combolist.ReplaceWithoutFind(combo.Id, combo);
                        }

                        AngularJSAuthentication.Model.PlaceOrder.PlacedOrderMasterDTM order = new AngularJSAuthentication.Model.PlaceOrder.PlacedOrderMasterDTM();
                        order.OrderId = objOrderMaster.OrderId;
                        order.CustomerId = objOrderMaster.CustomerId;
                        order.Skcode = objOrderMaster.Skcode;
                        order.WarehouseId = objOrderMaster.WarehouseId;
                        order.TotalAmount = objOrderMaster.TotalAmount;
                        var totalamt = objOrderMaster.TotalAmount + objOrderMaster.BillDiscountAmount ?? 0 + objOrderMaster.WalletAmount ?? 0;
                        order.WheelCount = Convert.ToInt32(Math.Floor(totalamt / 4000));
                        order.WheelAmountLimit = 4000;
                        order.DialEarnigPoint = 0;
                        decimal KisanDaanAmount = 0;
                        if (objOrderMaster.orderDetails != null && objOrderMaster.orderDetails.Any(x => !string.IsNullOrEmpty(x.SubcategoryName) && x.SubcategoryName.ToLower() == "kisan kirana"))
                        {
                            var KKAmount = Convert.ToDecimal(objOrderMaster.orderDetails.Where(x => !x.IsFreeItem && !string.IsNullOrEmpty(x.SubcategoryName) && x.SubcategoryName.ToLower() == "kisan kirana").Sum(x => x.qty * x.UnitPrice));
                            if (KKAmount > 0)
                            {
                                var KisanDaan = context.kisanDanMaster.FirstOrDefault(x => (x.OrderFromAmount <= KKAmount && x.OrderToAmount >= KKAmount) && x.IsActive);
                                if (KisanDaan != null)
                                {
                                    KisanDaanAmount = KKAmount * KisanDaan.KisanDanPrecentage / 100;
                                }
                            }
                        }


                        placeOrderResponse.OrderMaster = order;
                        placeOrderResponse.EarnWalletPoint = 0;
                        placeOrderResponse.KisanDaanAmount = KisanDaanAmount;
                        placeOrderResponse.NotServicing = clstr == null || !clstr.Active;
                        if (placeOrderResponse.NotServicing)
                            placeOrderResponse.Message = "We are currently not servicing in your area. Our team will contact you soon.";

                    }

                }
            }
            else
            {
                placeOrderResponse.IsSuccess = false;
                placeOrderResponse.Message = "Some incorrect data found.";
                return placeOrderResponse;
            }
            return placeOrderResponse;
        }


        [Route("PlaceDistributorOrder")]
        [AllowAnonymous]
        [HttpPost]
        public Model.PlaceOrder.PlaceOrderResponse PlaceDistributorOrder(ShoppingCart sc)
        {
            var placeOrderResponse = new Model.PlaceOrder.PlaceOrderResponse();

            if (doncustomerIds.Any(x => x == sc.CustomerId.Value))
            {
                placeOrderResponse.IsSuccess = false;
                placeOrderResponse.Message = "आप का आर्डर प्रोसेस मे है प्लीज प्रतिक्षा करे।";
                placeOrderResponse.cart = null;
                return placeOrderResponse;
            }
            else
            {
                doncustomerIds.Add(sc.CustomerId.Value);
            }
            using (var context = new AuthContext())
            {
                Customer cust = new Customer();
                var rsaKey = string.Empty;
                var hdfcOrderId = string.Empty;
                var eplOrderId = string.Empty;

                OrderMaster objOrderMaster = new OrderMaster();

                List<BillDiscount> BillDiscounts = new List<BillDiscount>();
                cust = context.Customers.FirstOrDefault(c => !c.Deleted && c.Mobile.Equals(sc.Customerphonenum));

                MongoDbHelper<UdharOverDueDayValidation> UdharOverDueDay = new MongoDbHelper<UdharOverDueDayValidation>();
                var DueAmt = UdharOverDueDay.GetAll();
                if (DueAmt != null && DueAmt.Any() && DueAmt.Max(x => x.MaxOverDueDay) > 0)
                {
                    var minDay = DueAmt.Min(x => x.MinOverDueDay);
                    var maxDay = DueAmt.Max(x => x.MaxOverDueDay);

                    if (cust.UdharDueDays > 0)
                    {
                        CheckDueAmtDc UDData = new CheckDueAmtDc();

                        var param1 = new SqlParameter("@CustomerId", sc.CustomerId);
                        UDData = context.Database.SqlQuery<CheckDueAmtDc>("Exec CheckDueAmt @CustomerId ", param1).FirstOrDefault();
                        if (UDData != null && UDData.Amount >= 1)
                        {
                            if (sc.CustomerId > 0 && sc.APPType == "DON App" && cust.UdharDueDays > maxDay)
                            {
                                placeOrderResponse.IsSuccess = false;
                                placeOrderResponse.Message = "Kindly clear your Direct Udhaar overdue amount of Rs." + UDData.Amount + " before placing the order.";
                                placeOrderResponse.cart = null;
                                return placeOrderResponse;
                            }

                            if (sc.CustomerId > 0 && sc.APPType == "DON App" && cust.UdharDueDays > minDay && cust.UdharDueDays < maxDay)
                            {
                                if (sc.paymentThrough.Split(',').ToList().Any(x => x == "cash" || x == "DirectUdhar"))
                                {
                                    placeOrderResponse.IsSuccess = false;
                                    placeOrderResponse.Message = "Kindly clear your Direct Udhaar overdue amount of Rs." + UDData.Amount + " to enable Udhaar  mode of payment.";
                                    placeOrderResponse.cart = null;
                                    return placeOrderResponse;
                                }

                            }
                        }
                    }
                }

                if (cust == null)
                {
                    if (doncustomerIds.Any(x => x == cust.CustomerId))
                    {
                        doncustomerIds.Remove(cust.CustomerId);
                    }
                    placeOrderResponse.IsSuccess = false;
                    placeOrderResponse.Message = "Customer not found.";
                    placeOrderResponse.cart = null;
                    return placeOrderResponse;
                }
                //stop gullak from V6 12 March 2022
                //if (sc.paymentThrough != null && sc.paymentThrough == "Gullak" && sc.GulkAmount > 0)
                //{
                //    placeOrderResponse.IsSuccess = false;
                //    placeOrderResponse.Message = "गुलक से आर्डर स्वीकार नहीं कर रहे हैं। असुविधा के लिए खेद है।";
                //    placeOrderResponse.cart = null;
                //    return placeOrderResponse;
                //}
                if (!cust.IsKPP && !context.DistributorVerificationDB.Any(x => x.CustomerID == cust.CustomerId && x.IsActive))
                {
                    if (doncustomerIds.Any(x => x == cust.CustomerId))
                    {
                        doncustomerIds.Remove(cust.CustomerId);
                    }
                    placeOrderResponse.IsSuccess = false;
                    placeOrderResponse.Message = "आप इस ऐप पर ऑर्डर देने के लिए अधिकृत नहीं हैं|";
                    placeOrderResponse.cart = null;
                    return placeOrderResponse;
                }

                var customerGullak = context.GullakDB.FirstOrDefault(x => x.CustomerId == cust.CustomerId && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value));
                string OrderStatus = "Pending";
                if (sc.paymentThrough == "Gullak" && sc.GulkAmount > 0)
                {
                    if (customerGullak == null || customerGullak.TotalAmount < sc.GulkAmount)
                    {
                        if (doncustomerIds.Any(x => x == cust.CustomerId))
                        {
                            doncustomerIds.Remove(cust.CustomerId);
                        }
                        placeOrderResponse.IsSuccess = false;
                        placeOrderResponse.Message = "Insufficient fund in your gullak please add money to your gullak.";
                        placeOrderResponse.cart = null;
                        return placeOrderResponse;
                    }
                }
                //var currentappVersion = context.appVersionDb.FirstOrDefault(x => x.isCompulsory);
                //if (sc.SalesPersonId.HasValue && sc.SalesPersonId.Value == 0 && string.IsNullOrEmpty(sc.APPVersion) && currentappVersion != null && currentappVersion.App_version != sc.APPVersion)
                //{
                //    placeOrderResponse.IsSuccess = false;
                //    placeOrderResponse.Message = "Please update you App. before placing order.";
                //    placeOrderResponse.cart = null;
                //    return placeOrderResponse;
                //}

                //var cluster = cust.ClusterId.HasValue ? context.Clusters.FirstOrDefault(x => x.ClusterId == cust.ClusterId.Value) : null;

                List<int> offerItemId = new List<int>();

                var warehouse = context.Warehouses.Where(w => w.WarehouseId == cust.Warehouseid).Select(c => c).SingleOrDefault();

                var isWareHouseLive = context.GMWarehouseProgressDB.FirstOrDefault(x => x.WarehouseID == warehouse.WarehouseId)?.IsLaunched;

                if (isWareHouseLive.HasValue && !isWareHouseLive.Value)
                {
                    if (doncustomerIds.Any(x => x == cust.CustomerId))
                    {
                        doncustomerIds.Remove(cust.CustomerId);
                    }
                    placeOrderResponse.IsSuccess = false;
                    placeOrderResponse.Message = "We are coming soon to your location.";
                    placeOrderResponse.cart = null;
                    return placeOrderResponse;
                }


                var cartItemIds = new List<int>();
                var itemIds = sc.itemDetails.Select(x => x.ItemId).ToList();
                var freeItemIds = sc.itemDetails.Where(x => x.FreeItemId > 0 && x.FreeItemqty > 0).Select(x => x.FreeItemId).ToList();
                var itemMastersList = context.itemMasters.Where(x => itemIds.Contains(x.ItemId) && x.WarehouseId == cust.Warehouseid).ToList();
                var itemNumbers = itemMastersList.GroupBy(x => new { x.Number, x.ItemMultiMRPId }).ToList();
                var FreeitemsList = context.itemMasters.Where(x => freeItemIds.Contains(x.ItemId) && x.WarehouseId == cust.Warehouseid).Select(x => x).ToList();

                List<ItemLimitMaster> itemLimits = new List<ItemLimitMaster>();

                itemNumbers.ForEach(x =>
                {
                    var itemLimit = context.ItemLimitMasterDB.FirstOrDefault(z => z.ItemNumber == x.Key.Number && z.ItemMultiMRPId == x.Key.ItemMultiMRPId && z.WarehouseId == cust.Warehouseid && z.IsItemLimit);
                    if (itemLimit != null)
                        itemLimits.Add(itemLimit);
                });


                var customerBrandIds = context.CustomerBrandAcessDB.Where(x => x.CustomerId == cust.CustomerId && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value)).Select(x => x.BrandId).ToList();
                if (customerBrandIds != null && customerBrandIds.Any())
                {
                    if (itemMastersList.Any(x => !customerBrandIds.Contains(x.SubsubCategoryid)))
                    {
                        if (doncustomerIds.Any(x => x == cust.CustomerId))
                        {
                            doncustomerIds.Remove(cust.CustomerId);
                        }
                        string brands = string.Join(",", itemMastersList.Where(x => !customerBrandIds.Contains(x.SubsubCategoryid)).Select(x => x.SubsubcategoryName).ToList());
                        placeOrderResponse.IsSuccess = false;
                        placeOrderResponse.Message = "Sorry, You have not access of following cart item brands " + brands;
                        placeOrderResponse.cart = null;
                        return placeOrderResponse;
                    }
                }

                var freeItemoffers = context.OfferDb.Where(x => x.OfferAppType == "Distributor App" && x.WarehouseId == cust.Warehouseid && x.IsActive && !x.IsDeleted && x.start <= DateTime.Now && x.end >= DateTime.Now && x.WarehouseId == cust.Warehouseid && x.OfferFreeItems.Any()).Include(x => x.OfferFreeItems)
                       .SelectMany(x =>
                   x.OfferFreeItems.Select(y => new OfferFreeItemDc
                   {
                       IsActive = x.IsActive,
                       FreeItemId = y.FreeItemId,
                       Id = y.Id,
                       ItemNumber = y.ItemNumber,
                       OfferFreeItemImage = y.OfferFreeItemImage,
                       OfferFreeItemName = y.OfferFreeItemName,
                       OfferFreeItemQuantity = y.OfferFreeItemQuantity,
                       offerId = y.offerId,
                       OfferMinimumQty = y.OfferMinimumQty,
                       OfferQtyAvaiable = y.OfferQtyAvaiable,
                       OfferQtyConsumed = y.OfferQtyConsumed,
                       OfferType = y.OfferType,
                       OfferWalletPoint = y.OfferWalletPoint,
                       ItemId = y.Offer.itemId,
                       QtyAvaiable = y.Offer.QtyAvaiable,
                       OfferOn = y.OfferOn,
                       ItemMultiMRPId = y.ItemMultiMRPId,
                   })).ToList();

                placeOrderResponse = context.ValidateDistributorShoppingCart(sc, warehouse, cust, null, cartItemIds, itemMastersList, FreeitemsList, itemLimits, freeItemoffers, out objOrderMaster, out BillDiscounts);

                if (placeOrderResponse.IsSuccess)
                {
                    double offerWalletPoint = 0;
                    List<OfferItem> offerItemsList = new List<OfferItem>();
                    var OfferUpdate = new List<Offer>();

                    foreach (var i in placeOrderResponse.cart.itemDetails)
                    {
                        ItemLimitFreebiesDc ItemLimitFreebiesconsume = new ItemLimitFreebiesDc();

                        var items = itemMastersList.Where(x => x.ItemId == i.ItemId && x.WarehouseId == i.WarehouseId).FirstOrDefault();

                        #region Add if validate
                        #region old code
                        //var freeItemOffer = freeItemoffers.FirstOrDefault(x => x.ItemNumber == items.ItemNumber);
                        #endregion

                        #region code changes for sellingsku
                        OfferFreeItemDc freeItemOffer = null;
                        if (freeItemoffers.Any(x => x.OfferOn == "Item" && x.ItemNumber == items.Number && x.ItemMultiMRPId == items.ItemMultiMRPId) || freeItemoffers.Any(x => x.OfferOn == "SellingSku" && x.ItemNumber == items.SellingSku && x.ItemMultiMRPId == items.ItemMultiMRPId))
                        {
                            if (freeItemoffers.Any(x => x.OfferOn == "Item" && x.ItemNumber == items.Number && x.ItemMultiMRPId == items.ItemMultiMRPId))
                            {
                                freeItemOffer = freeItemoffers.FirstOrDefault(x => x.OfferOn == "Item" && x.ItemNumber == items.Number && x.ItemMultiMRPId == items.ItemMultiMRPId);
                            }
                            else if (freeItemoffers.Any(x => x.OfferOn == "SellingSku" && x.ItemNumber == items.SellingSku && x.ItemMultiMRPId == items.ItemMultiMRPId))
                            {
                                freeItemOffer = freeItemoffers.FirstOrDefault(x => x.OfferOn == "SellingSku" && x.ItemNumber == items.SellingSku && x.ItemMultiMRPId == items.ItemMultiMRPId);
                            }
                        }
                        #endregion

                        if (freeItemOffer != null && i.IsOffer && i.FreeItemId > 0 && i.FreeItemqty > 0 && freeItemOffer.FreeItemId > 0 && freeItemOffer.QtyAvaiable > 0)
                        {
                            #region Add if validated
                            var offer = context.OfferDb.Where(x => x.OfferId == freeItemOffer.offerId).Include(x => x.OfferFreeItems).SingleOrDefault();
                            //to consume qty of freebiese if stock hit from currentstock in offer
                            if (offer != null && !offer.IsDispatchedFreeStock)
                            {
                                ItemLimitFreebiesconsume.ItemMultiMrpId = FreeitemsList.FirstOrDefault(f => f.ItemId == i.FreeItemId).ItemMultiMRPId;
                                ItemLimitFreebiesconsume.Qty = i.FreeItemqty;
                            }


                            //freesqtylimit
                            if (offer != null && i.FreeItemqty <= offer.FreeItemLimit)
                            {
                                offer.QtyAvaiable = offer.QtyAvaiable - i.FreeItemqty;
                                offer.QtyConsumed = offer.QtyConsumed + i.FreeItemqty;
                                if (offer.QtyAvaiable <= 0)
                                {
                                    offer.IsActive = false;
                                }

                                if (offer.OfferFreeItems != null && offer.OfferFreeItems.Any(x => x.FreeItemId == i.FreeItemId))
                                {
                                    offer.OfferFreeItems.FirstOrDefault(x => x.FreeItemId == i.FreeItemId).OfferQtyAvaiable = offer.OfferFreeItems.FirstOrDefault(x => x.FreeItemId == i.FreeItemId).OfferQtyAvaiable - i.FreeItemqty;
                                    offer.OfferFreeItems.FirstOrDefault(x => x.FreeItemId == i.FreeItemId).OfferQtyConsumed = offer.OfferFreeItems.FirstOrDefault(x => x.FreeItemId == i.FreeItemId).OfferQtyConsumed + i.FreeItemqty;
                                }

                                OfferUpdate.Add(offer);


                                OfferItem ff = new OfferItem();
                                ff.CompanyId = i.CompanyId;
                                ff.WarehouseId = i.WarehouseId;
                                ff.itemId = i.ItemId;
                                ff.itemname = offer.itemname;
                                ff.MinOrderQuantity = offer.MinOrderQuantity;
                                ff.NoOffreeQuantity = i.FreeItemqty;
                                ff.FreeItemId = offer.FreeItemId;
                                ff.FreeItemName = offer.FreeItemName;
                                ff.FreeItemMRP = offer.FreeItemMRP;
                                ff.IsDeleted = false;
                                ff.CreatedDate = indianTime;
                                ff.UpdateDate = indianTime;
                                ff.CustomerId = cust.CustomerId;
                                //ff.OrderId = placeOrderResponse.OrderMaster.OrderId;
                                ff.OfferType = "ItemMaster";
                                ff.ReferOfferId = i.OfferId;
                                //offerItemId.Add(ff.OfferId);
                                offerItemsList.Add(ff);
                            }
                            #endregion
                        }


                        if (i.IsOffer == true && i.OfferWalletPoint > 0)
                        {
                            //If offer is on wallet point then update is wallet point.
                            offerWalletPoint = offerWalletPoint + i.OfferWalletPoint;
                            var offerdata = context.OfferDb.Where(x => x.OfferId == i.OfferId).SingleOrDefault();
                            OfferItem offerItem = new OfferItem();

                            offerItem.CompanyId = i.CompanyId;
                            offerItem.WarehouseId = i.WarehouseId;
                            offerItem.itemId = i.ItemId;
                            offerItem.itemname = offerdata.itemname;
                            offerItem.MinOrderQuantity = offerdata.MinOrderQuantity;
                            offerItem.NoOffreeQuantity = i.FreeItemqty;
                            offerItem.FreeItemId = offerdata.FreeItemId;
                            offerItem.FreeItemName = offerdata.FreeItemName;
                            offerItem.FreeItemMRP = offerdata.FreeItemMRP;
                            offerItem.IsDeleted = false;
                            offerItem.CreatedDate = indianTime;
                            offerItem.UpdateDate = indianTime;
                            offerItem.CustomerId = cust.CustomerId;
                            //offerItem.OrderId = objOrderMaster.OrderId;
                            offerItem.WallentPoint = Convert.ToInt32(i.OfferWalletPoint);
                            offerItem.OfferId = i.OfferId;
                            offerItem.OfferType = "WalletPoint";
                            offerItemsList.Add(offerItem);
                            //offerItemId.Add(offerItem.OfferId);

                        }
                        #endregion

                        #region Add if Validated
                        ItemLimitMaster ItemLimitMaster = context.ItemLimitMasterDB.Where(x => x.ItemNumber == items.Number && x.WarehouseId == items.WarehouseId && x.ItemMultiMRPId == items.ItemMultiMRPId).FirstOrDefault();
                        if (ItemLimitMaster != null && ItemLimitMaster.IsItemLimit == true)
                        {
                            #region to consume qty of freebiese if stock hit from currentstock in offer
                            if (ItemLimitFreebiesconsume != null)
                            {
                                if (ItemLimitFreebiesconsume.ItemMultiMrpId == ItemLimitMaster.ItemMultiMRPId)
                                {
                                    i.qty += ItemLimitFreebiesconsume.Qty;
                                }
                            }
                            #endregion
                            if (i.qty < ItemLimitMaster.ItemlimitQty || i.qty == 0)
                            {
                                ItemLimitMaster.ItemlimitQty = ItemLimitMaster.ItemlimitQty - i.qty;
                                ItemLimitMaster.ItemLimitSaleQty = ItemLimitMaster.ItemLimitSaleQty + i.qty;
                                context.Entry(ItemLimitMaster).State = EntityState.Modified;
                            }
                            else
                            {
                                ItemLimitMaster.ItemlimitQty = ItemLimitMaster.ItemlimitQty - i.qty;
                                ItemLimitMaster.ItemLimitSaleQty = ItemLimitMaster.ItemLimitSaleQty + i.qty;
                                ItemLimitMaster.IsItemLimit = false;//08/07/2019
                                context.Entry(ItemLimitMaster).State = EntityState.Modified;

                                if (ItemLimitMaster.ItemlimitQty <= 0 || items.MinOrderQty > ItemLimitMaster.ItemlimitQty)
                                {
                                    //deactive
                                    List<ItemMaster> itemsDeactive = context.itemMasters.Where(x => x.Number == ItemLimitMaster.ItemNumber && x.WarehouseId == ItemLimitMaster.WarehouseId && x.ItemMultiMRPId == ItemLimitMaster.ItemMultiMRPId).ToList();
                                    foreach (var Ditem in itemsDeactive)
                                    {
                                        Ditem.active = false;
                                        Ditem.UpdatedDate = indianTime;
                                        Ditem.UpdateBy = "Auto Dective";
                                        context.Entry(Ditem).State = EntityState.Modified;
                                    }

                                }
                            }
                        }
                        ItemLimitMaster freeItemLimitMaster = context.ItemLimitMasterDB.Where(x => x.WarehouseId == items.WarehouseId && x.ItemMultiMRPId == ItemLimitFreebiesconsume.ItemMultiMrpId && x.ItemMultiMRPId != items.ItemMultiMRPId).FirstOrDefault();
                        if (freeItemLimitMaster != null && freeItemLimitMaster.IsItemLimit == true && ItemLimitFreebiesconsume != null)
                        {
                            freeItemLimitMaster.ItemlimitQty = freeItemLimitMaster.ItemlimitQty - ItemLimitFreebiesconsume.Qty;
                            freeItemLimitMaster.ItemLimitSaleQty = freeItemLimitMaster.ItemLimitSaleQty + ItemLimitFreebiesconsume.Qty;
                            context.Entry(freeItemLimitMaster).State = EntityState.Modified;
                        }
                        #endregion


                    }



                    #region Rewards, Offers, FlashDeals, Wallet etc....


                    objOrderMaster.deliveryCharge = sc.deliveryCharge;

                    objOrderMaster.TotalAmount = System.Math.Round(objOrderMaster.orderDetails.Sum(x => x.TotalAmt) + objOrderMaster.deliveryCharge.Value, 2);
                    objOrderMaster.TaxAmount = System.Math.Round(objOrderMaster.orderDetails.Sum(x => x.TaxAmmount + x.CessTaxAmount), 2);
                    objOrderMaster.SGSTTaxAmmount = System.Math.Round(objOrderMaster.orderDetails.Sum(x => x.SGSTTaxAmmount), 2);
                    objOrderMaster.CGSTTaxAmmount = System.Math.Round(objOrderMaster.orderDetails.Sum(x => x.CGSTTaxAmmount), 2);
                    objOrderMaster.GrossAmount = System.Math.Round(objOrderMaster.TotalAmount, 0);
                    objOrderMaster.DiscountAmount = 0;//System.Math.Round(objOrderMaster.orderDetails.Sum(x => x.TotalAmountAfterTaxDisc), 2);                   
                                                      //add cluster to ordermaster
                    objOrderMaster.ClusterId = cust.ClusterId ?? 0;
                    objOrderMaster.ClusterName = cust.ClusterName;
                    objOrderMaster.OrderType = 4;
                    var walletUsedPoint1 = sc.walletPointUsed;
                    var walletAmount1 = sc.WalletAmount;
                    CashConversion cash = context.CashConversionDb.FirstOrDefault(x => x.IsConsumer == false);

                    double rewPoint = 0;
                    double rewAmount = 0;
                    objOrderMaster.paymentMode = sc.paymentThrough;
                    objOrderMaster.Status = OrderStatus;

                    var disCustomer = context.DistributorVerificationDB.FirstOrDefault(x => x.CustomerID == cust.CustomerId);
                    objOrderMaster.Status = cust.Active ? objOrderMaster.Status : "Inactive"; ;
                    //&& disCustomer != null && disCustomer.IsActive && (!disCustomer.IsDeleted.HasValue || !disCustomer.IsDeleted.Value) && disCustomer.IsVerified ? objOrderMaster.Status : "Inactive";//a

                    if (objOrderMaster.orderDetails != null && objOrderMaster.orderDetails.Any())
                    {
                        objOrderMaster.orderDetails.ToList().ForEach(x => x.Status = objOrderMaster.Status);
                    }
                    // call function

                    //removerd by Harry ( on 21)
                    //objOrderMaster = RewardAndWalletPointForPlacedOrder(placeOrderResponse.cart, offerWalletPoint, objOrderMaster, rewardpoint, cust, walletUsedPoint1, rewPoint, rewAmount, cash);

                    context.DbOrderMaster.Add(objOrderMaster);
                    context.Commit();

                    objOrderMaster = context.RewardAndWalletPointForDistributorPlacedOrder(placeOrderResponse.cart, offerWalletPoint, objOrderMaster, 0, cust, walletUsedPoint1, rewPoint, rewAmount, cash);

                    if (OfferUpdate != null && OfferUpdate.Any())
                    {
                        foreach (var Offers in OfferUpdate)
                        {
                            context.Entry(Offers).State = EntityState.Modified;
                        }
                    }
                    if (cust != null)
                    {
                        cust.ShippingAddress = sc.ShippingAddress;
                        cust.ordercount = cust.ordercount + 1;
                        cust.MonthlyTurnOver = cust.MonthlyTurnOver + objOrderMaster.GrossAmount;
                        context.Entry(cust).State = EntityState.Modified;
                    }
                    //for first order

                    #endregion


                    #region Bill Discount
                    if (!string.IsNullOrEmpty(sc.BillDiscountOfferId))
                    {
                        #region BillDiscount Free Item

                        List<int> billdiscountofferids = BillDiscounts.Select(x => x.OfferId).ToList();
                        List<Offer> Offers = context.OfferDb.Where(x => billdiscountofferids.Contains(x.OfferId)).Include(x => x.BillDiscountFreeItems).ToList();

                        if (Offers != null)
                        {
                            List<int> offerids = new List<int>();
                            foreach (var item in Offers.Where(x => x.BillDiscountOfferOn == "FreeItem").SelectMany(x => x.BillDiscountFreeItems).ToList())
                            {
                                item.RemainingOfferStockQty += item.Qty;
                                context.Entry(item).State = EntityState.Modified;
                                if (item.RemainingOfferStockQty >= item.OfferStockQty)
                                {
                                    offerids.Add(item.offerId);
                                }
                            }

                            if (Offers.Any(x => x.FreeItemLimit.HasValue && x.FreeItemLimit.Value > 0 && (x.OfferOn == "ScratchBillDiscount" || x.OfferOn == "BillDiscount")))
                            {
                                var limitofferids = Offers.Where(x => x.FreeItemLimit.HasValue && x.FreeItemLimit.Value > 0 && (x.OfferOn == "ScratchBillDiscount" || x.OfferOn == "BillDiscount")).Select(x => x.OfferId);
                                var offerTakingCount = context.BillDiscountDb.Where(x => limitofferids.Contains(x.OfferId)).GroupBy(x => x.OfferId).Select(x => new { offerid = x.Key, totalCount = x.Count() }).ToList();
                                if (offerTakingCount != null && offerTakingCount.Any())
                                {
                                    foreach (var item in Offers.Where(x => limitofferids.Contains(x.OfferId)))
                                    {
                                        var offertaking = offerTakingCount.FirstOrDefault(x => x.offerid == item.OfferId);
                                        if (offertaking != null && item.FreeItemLimit <= offertaking.totalCount + 1)
                                        {
                                            offerids.Add(item.OfferId);
                                        }
                                        else if (item.FreeItemLimit == 1)
                                        {
                                            offerids.Add(item.OfferId);
                                        }
                                    }
                                }
                            }

                            if (offerids.Any())
                            {
                                foreach (var item in Offers.Where(x => offerids.Contains(x.OfferId)))
                                {
                                    item.UpdateDate = indianTime;
                                    item.IsActive = false;
                                    context.Entry(item).State = EntityState.Modified;
                                }
                            }
                        }


                        #endregion

                        foreach (var offer in BillDiscounts)
                        {
                            offer.OrderId = objOrderMaster.OrderId;
                            //var scritchcartoffer = context.BillDiscountDb.FirstOrDefault(x => x.OfferId == offer.OfferId && x.CustomerId == cust.CustomerId && x.OrderId == 0 && x.BillDiscountType == "ScratchBillDiscount");
                            var scritchcartoffer = !Offers.Any(x => x.OfferId == offer.OfferId && x.BillDiscountOfferOn == "DynamicAmount") ?
                            context.BillDiscountDb.FirstOrDefault(x => x.OfferId == offer.OfferId && x.CustomerId == cust.CustomerId && x.OrderId == 0)
                            : context.BillDiscountDb.FirstOrDefault(x => x.OfferId == offer.OfferId && x.CustomerId == cust.CustomerId && x.BillDiscountAmount == offer.BillDiscountAmount && x.OrderId == 0);


                            if (scritchcartoffer == null)
                                context.BillDiscountDb.Add(offer);
                            else
                            {
                                scritchcartoffer.OrderId = objOrderMaster.OrderId;
                                scritchcartoffer.BillDiscountAmount = offer.BillDiscountAmount;
                                scritchcartoffer.ModifiedBy = cust.CustomerId;
                                scritchcartoffer.ModifiedDate = indianTime;
                                context.Entry(scritchcartoffer).State = EntityState.Modified;
                            }
                        }
                        sc.BillDiscountAmount = BillDiscounts.Sum(x => x.BillDiscountAmount);
                    }

                    #region TCS Calculate
                    GetCustomersTotalPurchaseInMongo helper = new GetCustomersTotalPurchaseInMongo();
                    var tcsConfig = helper.GetCustomersTotalPurchaseForTCS(cust.CustomerId, cust.PanNo, context);

                    if (tcsConfig != null && !cust.IsTCSExemption)
                    {
                        var percent = !cust.IsPanVerified ? tcsConfig.NotGSTTCSPercent : tcsConfig.GSTTCSPercent;
                        if (tcsConfig.IsAlreadyTcsUsed == true)
                        {
                            objOrderMaster.TCSAmount = (objOrderMaster.TotalAmount - (sc.BillDiscountAmount.HasValue ? sc.BillDiscountAmount.Value : 0)) * percent / 100;
                        }
                        else if ((tcsConfig.TotalPurchase + tcsConfig.PendingOrderAmount + objOrderMaster.TotalAmount) > tcsConfig.TCSAmountLimit)
                        {
                            if (tcsConfig.TotalPurchase > tcsConfig.TCSAmountLimit)
                            {
                                objOrderMaster.TCSAmount = (objOrderMaster.TotalAmount - (sc.BillDiscountAmount.HasValue ? sc.BillDiscountAmount.Value : 0)) * percent / 100;
                            }
                            else if (tcsConfig.TotalPurchase + tcsConfig.PendingOrderAmount > tcsConfig.TCSAmountLimit)
                            {
                                objOrderMaster.TCSAmount = (objOrderMaster.TotalAmount - (sc.BillDiscountAmount.HasValue ? sc.BillDiscountAmount.Value : 0)) * percent / 100;
                            }
                            else
                            {
                                var TCSCalculatedAMT = (tcsConfig.TotalPurchase + tcsConfig.PendingOrderAmount + objOrderMaster.TotalAmount) - tcsConfig.TCSAmountLimit;
                                if (TCSCalculatedAMT > 0)
                                {
                                    objOrderMaster.TCSAmount = (objOrderMaster.TotalAmount - (sc.BillDiscountAmount.HasValue ? sc.BillDiscountAmount.Value : 0)) * percent / 100;
                                }
                            }
                        }
                    }
                    #endregion

                    objOrderMaster.TotalAmount = objOrderMaster.TotalAmount - (sc.BillDiscountAmount.HasValue ? sc.BillDiscountAmount.Value : 0) + objOrderMaster.TCSAmount;
                    objOrderMaster.BillDiscountAmount = sc.BillDiscountAmount.HasValue ? sc.BillDiscountAmount.Value : 0;
                    objOrderMaster.GrossAmount = System.Math.Round(objOrderMaster.TotalAmount, 0, MidpointRounding.AwayFromZero);
                    #endregion


                    bool sendNotification = false;

                    if (objOrderMaster.OrderId != 0)
                    {
                        MongoDbHelper<AngularJSAuthentication.Model.CustomerShoppingCart.CustomerShoppingCart> mongoDbHelper = new MongoDbHelper<AngularJSAuthentication.Model.CustomerShoppingCart.CustomerShoppingCart>();
                        var cartPredicate = LinqKit.PredicateBuilder.New<AngularJSAuthentication.Model.CustomerShoppingCart.CustomerShoppingCart>(x => x.CustomerId == objOrderMaster.CustomerId && x.WarehouseId == objOrderMaster.WarehouseId && !x.GeneratedOrderId.HasValue && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value));
                        var customerShoppingCart = mongoDbHelper.Select(cartPredicate, null, null, null, collectionName: "CustomerShoppingCart").FirstOrDefault();
                        if (customerShoppingCart != null)
                        {
                            customerShoppingCart.GeneratedOrderId = objOrderMaster.OrderId;
                            bool status = mongoDbHelper.Replace(customerShoppingCart.Id, customerShoppingCart, "CustomerShoppingCart");
                        }

                        if (offerItemsList != null && offerItemsList.Any())
                        {
                            foreach (var data in offerItemsList)
                            {
                                data.OrderId = objOrderMaster.OrderId;
                                int OrderDetailsId = 0;
                                if (data.OfferType != "BillDiscount_FreeItem")
                                {
                                    OrderDetailsId = objOrderMaster.orderDetails.Any(x => x.FreeWithParentItemId > 0 && x.FreeWithParentItemId == data.itemId) ?
                                       objOrderMaster.orderDetails.FirstOrDefault(x => x.FreeWithParentItemId > 0 && x.FreeWithParentItemId == data.itemId).OrderDetailsId : 0;
                                }
                                else
                                    OrderDetailsId = objOrderMaster.orderDetails.FirstOrDefault(x => x.ItemId == data.FreeItemId && x.UnitPrice == 0.0001).OrderDetailsId;
                                data.OrderDetailsId = OrderDetailsId;
                            }
                            context.OfferItemDb.AddRange(offerItemsList);
                        }


                        sendNotification = true;
                        OrderStatus = cust.Active ? OrderStatus : "Inactive";
                        if (!string.IsNullOrEmpty(sc.paymentThrough) && (sc.paymentThrough.ToLower().Contains("hdfc") || sc.paymentThrough.ToLower().Contains("truepay") || sc.paymentThrough.ToLower().Contains("epaylater") || sc.paymentThrough.ToLower().Contains("chqbook") || sc.paymentThrough.ToLower().Contains("directudhar")))//by Ashwin
                        {
                            OrderStatus = "Payment Pending";
                            objOrderMaster.paymentThrough = sc.paymentThrough;
                            objOrderMaster.paymentMode = "Online";
                        }
                        else if (sc.paymentThrough == "Gullak" && sc.GulkAmount > 0)
                        {
                            context.PaymentResponseRetailerAppDb.Add(new PaymentResponseRetailerApp
                            {
                                amount = Math.Round(sc.GulkAmount, 0),
                                CreatedDate = indianTime,
                                currencyCode = "INR",
                                OrderId = objOrderMaster.OrderId,
                                PaymentFrom = "Gullak",
                                status = "Success",
                                statusDesc = "Order Place",
                                UpdatedDate = indianTime,
                                IsRefund = false,
                                IsOnline = true,
                                GatewayTransId = "G" + DateTime.Now.ToString("ddMMyyyyHHmmss"),
                                GatewayOrderId = customerGullak.Id.ToString()
                            });

                            context.GullakTransactionDB.Add(new Model.Gullak.GullakTransaction
                            {
                                CreatedDate = indianTime,
                                CreatedBy = customerGullak.CustomerId,
                                Comment = "Order Placed : " + objOrderMaster.OrderId.ToString(),
                                Amount = (-1) * Math.Round(sc.GulkAmount, 0),
                                GullakId = customerGullak.Id,
                                CustomerId = customerGullak.CustomerId,
                                IsActive = true,
                                IsDeleted = false,
                                ObjectId = objOrderMaster.OrderId.ToString(),
                                ObjectType = "Order"
                            });

                            customerGullak.TotalAmount -= Math.Round(sc.GulkAmount);
                            customerGullak.ModifiedBy = customerGullak.CustomerId;
                            customerGullak.ModifiedDate = indianTime;

                            objOrderMaster.paymentThrough = sc.paymentThrough;
                            objOrderMaster.paymentMode = "Online";
                            context.Entry(customerGullak).State = EntityState.Modified;
                        }
                        else if (sc.paymentThrough == "Cash")
                        {
                            if ((Math.Round(objOrderMaster.TotalAmount, 0) - sc.GulkAmount) > 0)
                            {
                                OrderStatus = cust.Active ? "InTransit" : OrderStatus;
                                context.PaymentResponseRetailerAppDb.Add(new PaymentResponseRetailerApp
                                {
                                    amount = (Math.Round(objOrderMaster.TotalAmount, 0) - sc.GulkAmount),
                                    CreatedDate = indianTime,
                                    currencyCode = "INR",
                                    OrderId = objOrderMaster.OrderId,
                                    PaymentFrom = "Cash",
                                    status = "Success",
                                    statusDesc = "Order Place",
                                    UpdatedDate = indianTime,
                                    IsRefund = false
                                });
                            }
                            objOrderMaster.paymentMode = "COD";
                            objOrderMaster.paymentThrough = sc.paymentThrough;
                        }


                        string Borderid = Convert.ToString(objOrderMaster.OrderId);
                        string BorderCodeId = Borderid.PadLeft(11, '0');
                        temOrderQBcode code = GetBarcode(BorderCodeId);
                        objOrderMaster.InvoiceBarcodeImage = code.BarcodeImage;
                        objOrderMaster.Status = OrderStatus;
                        objOrderMaster.orderDetails.ToList().ForEach(x => x.status = objOrderMaster.Status);

                    }
                    //if (context.IsCustFirstOrder(objOrderMaster.CustomerId))
                    //{
                    objOrderMaster.IsFirstOrder = false;
                    //};

                    context.Entry(objOrderMaster).State = EntityState.Modified;

                    context.Commit();

                    if (sendNotification)
                    {
                        try
                        {
                            #region for first order
                            if (cust.ordercount == 1)//if this is customer first order
                            {
                                context.FirstCustomerOrder(cust, objOrderMaster);
                            }
                            #endregion

                            if (cust.ordercount > 1)
                            {
                                context.ForNotification(cust.CustomerId, objOrderMaster.GrossAmount);
                            }
                        }
                        catch (Exception ex)
                        {
                            logger.Error(ex.Message);
                        }
                    }
                }


                if (placeOrderResponse.IsSuccess && objOrderMaster.OrderId > 0)
                {
                    AngularJSAuthentication.Model.PlaceOrder.PlacedOrderMasterDTM order = new AngularJSAuthentication.Model.PlaceOrder.PlacedOrderMasterDTM();
                    order.OrderId = objOrderMaster.OrderId;
                    order.CustomerId = objOrderMaster.CustomerId;
                    order.Skcode = objOrderMaster.Skcode;
                    order.WarehouseId = objOrderMaster.WarehouseId;
                    order.TotalAmount = objOrderMaster.TotalAmount;
                    var totalamt = objOrderMaster.TotalAmount + objOrderMaster.BillDiscountAmount ?? 0 + objOrderMaster.WalletAmount ?? 0;
                    order.WheelCount = Convert.ToInt32(Math.Floor(totalamt / 4000));
                    order.WheelAmountLimit = 4000;
                    order.DialEarnigPoint = 0;
                    decimal KisanDaanAmount = 0;
                    if (objOrderMaster.orderDetails != null && objOrderMaster.orderDetails.Any(x => !string.IsNullOrEmpty(x.SubcategoryName) && x.SubcategoryName.ToLower() == "kisan kirana"))
                    {
                        var KKAmount = Convert.ToDecimal(objOrderMaster.orderDetails.Where(x => !x.IsFreeItem && !string.IsNullOrEmpty(x.SubcategoryName) && x.SubcategoryName.ToLower() == "kisan kirana").Sum(x => x.qty * x.UnitPrice));
                        if (KKAmount > 0)
                        {
                            var KisanDaan = context.kisanDanMaster.FirstOrDefault(x => (x.OrderFromAmount <= KKAmount && x.OrderToAmount >= KKAmount) && x.IsActive);
                            if (KisanDaan != null)
                            {
                                KisanDaanAmount = KKAmount * KisanDaan.KisanDanPrecentage / 100;
                            }
                        }
                    }


                    placeOrderResponse.OrderMaster = order;
                    placeOrderResponse.EarnWalletPoint = BillDiscounts != null && BillDiscounts.Any(x => x.IsUsedNextOrder) ? Convert.ToInt32(BillDiscounts.Where(x => x.IsUsedNextOrder).Sum(x => x.BillDiscountTypeValue)) : 0;
                    placeOrderResponse.KisanDaanAmount = KisanDaanAmount;
                    placeOrderResponse.NotServicing = true;
                    //    cluster == null || !cluster.Active;
                    //if (placeOrderResponse.NotServicing)
                    //    placeOrderResponse.Message = "We are currently not servicing in your area. Our team will contact you soon.";

                }
            }
            if (doncustomerIds.Any(x => x == sc.CustomerId.Value))
            {
                doncustomerIds.Remove(sc.CustomerId.Value);
            }
            return placeOrderResponse;
        }

        public temOrderQBcode GetBarcode(string OrderId)
        {
            temOrderQBcode obj = new temOrderQBcode();
            try
            {

                string barcode = OrderId;

                //Barcode image into your system
                var barcodeLib = new BarcodeLib.Barcode(barcode);
                barcodeLib.Height = 120;
                barcodeLib.Width = 245;
                barcodeLib.LabelPosition = BarcodeLib.LabelPositions.BOTTOMCENTER;//
                barcodeLib.ImageFormat = System.Drawing.Imaging.ImageFormat.Png;//
                System.Drawing.Font font = new System.Drawing.Font("verdana", 12f);//
                barcodeLib.LabelFont = font;
                barcodeLib.IncludeLabel = true;
                barcodeLib.Alignment = BarcodeLib.AlignmentPositions.CENTER;
                barcodeLib.LabelPosition = BarcodeLib.LabelPositions.BOTTOMCENTER;//
                Image imeg = barcodeLib.Encode(TYPE.CODE128, barcode);//bytestream
                obj.BarcodeImage = (byte[])(new ImageConverter()).ConvertTo(imeg, typeof(byte[]));

                return obj;
            }

            catch (Exception err)
            {
                return obj;
            }
        }

        [Route("RemoveCustomerForPayNow")]
        [AllowAnonymous]
        public bool RemoveCustomerForPayNow(int customerId)
        {
            if (PayNowncustomerIds.Any(x => x == customerId))
            {
                PayNowncustomerIds.Remove(customerId);
            }

            return true;
        }

        [Route("InsertOnlineTransactionV2")]
        [AllowAnonymous]
        [HttpPost]
        public async Task<HttpResponseMessage> InsertOnlineTransactionV2(PaymentReq paymentReq)
        {
            string decData = string.Empty;
            PaymentResponseRetailerApp PcData = null;
            RazorpayPaymentResponse razorpayRes = null;
            //var authContextt = new AuthContext();
            try
            {
                AES256 aes = new AES256();
                //string redisAesKey = "Sh0pK!r@n@#@!@#$";
                string redisAesKey = DateTime.Now.ToString("yyyyMMdd") + "1201";

                decData = aes.Decrypt(paymentReq.EncString, redisAesKey);
                PcData = JsonConvert.DeserializeObject<PaymentResponseRetailerApp>(decData);
            }
            catch (Exception ex)
            {
                logger.Error("Error while decrypding data: " + ex.ToString());
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Invalid Data");
            }
            if (PcData != null && PcData.IsPayLater == false)
            {
                PcData.CreatedDate = indianTime;
                PcData.UpdatedDate = indianTime;
                if (PcData.PaymentFrom.ToLower() == "gullak")
                {
                    PcData.GatewayTransId = "G" + DateTime.Now.ToString("ddMMyyyyHHmmss");
                }
                using (var authContext = new AuthContext())
                {
                    if (PcData.PaymentFrom.ToLower() != "cash" && (!string.IsNullOrEmpty(PcData.GatewayTransId) && PcData.GatewayTransId != "0"))
                    {
                        if (PcData.GatewayRequest.Contains(ConfigurationManager.AppSettings["CcAvenueCreditAccessCode"]))
                        {
                            PcData.PaymentFrom = "HDFC-Credit";
                        }
                        if (PcData.PaymentFrom.ToLower() == "razorpay")
                        {
                            razorpayRes = JsonConvert.DeserializeObject<RazorpayPaymentResponse>(PcData.GatewayResponse);
                            if (razorpayRes != null)
                            {
                                RazorPayTransactionHelper razorPayTransactionHelper = new RazorPayTransactionHelper();
                                RazorPayTransactionDC razorPayTransactionDC = new RazorPayTransactionDC()
                                {
                                    OrderId = PcData.OrderId,
                                    Amount = Math.Round(PcData.amount, 0),
                                    response = razorpayRes,
                                };
                                bool res = await razorPayTransactionHelper.PostRazorPayTransactionAsync(razorPayTransactionDC, 0, authContext);
                                PcData.GatewayOrderId = razorpayRes.razorpay_order_id;
                                if (!res)
                                    return Request.CreateResponse(HttpStatusCode.OK, res);
                            }
                        }
                        PcData.IsOnline = true;

                        var existingPaymentResponse = authContext.PaymentResponseRetailerAppDb.Any(x => !string.IsNullOrEmpty(x.GatewayTransId) && x.GatewayTransId == PcData.GatewayTransId);
                        if (existingPaymentResponse)
                            return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Request has been tampered");

                        //var existingPaymentResponse = authContext.PaymentResponseRetailerAppDb.Any(x => !string.IsNullOrEmpty(x.GatewayTransId) && !string.IsNullOrEmpty(x.PaymentFrom) && x.GatewayTransId == PcData.GatewayTransId && x.OrderId==PcData.OrderId && x.PaymentFrom ==PcData.PaymentFrom);
                        //if (existingPaymentResponse)
                        //    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Request has been tampered");
                    }

                    PcData.amount = Math.Round(PcData.amount, 0);
                    authContext.PaymentResponseRetailerAppDb.Add(PcData);

                    #region Handle Insert Payment Request
                    if (PcData.IsOnline)
                    {
                        if (paymentInprocess.Any(x => x == PcData.GatewayTransId))
                        {
                            return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Request has been tampered");
                        }
                        else
                        {
                            paymentInprocess.Add(PcData.GatewayTransId);
                        }
                    }
                    #endregion

                    var order = await authContext.DbOrderMaster.Where(x => x.OrderId == PcData.OrderId).FirstOrDefaultAsync();
                    var cust = await authContext.Customers.FirstOrDefaultAsync(x => x.CustomerId == order.CustomerId);

                    //if (PcData.PaymentFrom.ToLower() != "cash" && PcData.status == "Success" && (!string.IsNullOrEmpty(PcData.GatewayTransId) && PcData.GatewayTransId != "0") && (order.Status.ToLower() == "pending" || order.Status.ToLower() == "shipped"))
                    if (PcData.PaymentFrom.ToLower() != "cash" && PcData.status == "Success" && (!string.IsNullOrEmpty(PcData.GatewayTransId) && PcData.GatewayTransId != "0"))
                    {
                        var cashPayment = authContext.PaymentResponseRetailerAppDb.Where(x => x.PaymentFrom == "Cash" && x.status == "Success" && x.OrderId == PcData.OrderId).ToList();
                        if (cashPayment.Any())
                        {
                            cashPayment.ForEach(x =>
                            {
                                x.status = "Falied";
                                authContext.Entry(x).State = EntityState.Modified;
                            });
                        }
                    }
                    if (PcData.PaymentFrom.ToLower() == "gullak")
                    {

                        if (PayNowncustomerIds.Any(x => x == order.CustomerId))
                        {
                            if (paymentInprocess.Any(x => x == PcData.GatewayTransId))
                            {
                                paymentInprocess.Remove(PcData.GatewayTransId);
                            }
                            return Request.CreateResponse(HttpStatusCode.OK, false);
                        }
                        else
                        {
                            PayNowncustomerIds.Add(order.CustomerId);
                        }
                        var customerGullak = authContext.GullakDB.FirstOrDefault(x => x.CustomerId == order.CustomerId && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value));

                        if (customerGullak == null || customerGullak.TotalAmount < Math.Round(PcData.amount, 0))
                        {
                            PcData.status = "Falied";
                            if (PayNowncustomerIds.Any(x => x == order.CustomerId))
                            {
                                PayNowncustomerIds.Remove(order.CustomerId);
                            }
                        }
                        else
                        {
                            var paidAmt = authContext.PaymentResponseRetailerAppDb.Where(x => x.status == "Success" && x.IsOnline && x.OrderId == PcData.OrderId).Select(x => x.amount).ToList().Sum(x => x);
                            if (order.GrossAmount == paidAmt)
                            {
                                if (PayNowncustomerIds.Any(x => x == order.CustomerId))
                                {
                                    PayNowncustomerIds.Remove(order.CustomerId);
                                }
                                if (paymentInprocess.Any(x => x == PcData.GatewayTransId))
                                {
                                    paymentInprocess.Remove(PcData.GatewayTransId);
                                }
                                return Request.CreateResponse(HttpStatusCode.OK, false);
                            }
                            authContext.GullakTransactionDB.Add(new Model.Gullak.GullakTransaction
                            {
                                CreatedDate = indianTime,
                                CreatedBy = customerGullak.CustomerId,
                                Comment = "Pay Now : " + order.OrderId.ToString(),
                                Amount = (-1) * Math.Round(PcData.amount, 0),
                                GullakId = customerGullak.Id,
                                CustomerId = customerGullak.CustomerId,
                                IsActive = true,
                                IsDeleted = false,
                                ObjectId = order.OrderId.ToString(),
                                ObjectType = "Order"
                            });

                            customerGullak.TotalAmount -= Math.Round(PcData.amount);
                            customerGullak.ModifiedBy = customerGullak.CustomerId;
                            customerGullak.ModifiedDate = indianTime;

                            authContext.Entry(customerGullak).State = EntityState.Modified;
                        }
                    }

                    if (PcData.PaymentFrom.ToLower() == "epaylater" && PcData.status == "Success")
                    {
                        var a = 0;
                        var b = 0;
                        a = Convert.ToInt32(cust.UdharLimitRemaining);
                        b = Convert.ToInt32(PcData.amount);
                        cust.UdharLimitRemaining = a - b;
                        authContext.Entry(cust).State = EntityState.Modified;
                    }

                    if (PcData.PaymentFrom == "DirectUdhar" && PcData.status == "Failed")
                    {

                        var param1 = new SqlParameter("@OrderId", PcData.OrderId);
                        var result = authContext.Database.SqlQuery<bool>("EXEC sp_OrderCancelForDirectUdhar @OrderId ", param1).FirstOrDefault();


                    }

                    if (PcData.PaymentFrom.ToLower() == "truepay")
                    {
                        TrupayTransaction tpt = new TrupayTransaction();
                        tpt.TrupayTransactionStatus = PcData.status;
                        tpt.OrderId = PcData.OrderId;
                        tpt.TrupayTransactionId = PcData.GatewayTransId;
                        tpt.paymentThrough = PcData.PaymentThrough;
                        tpt.CustomerName = order.CustomerName;
                        tpt.Status = "From Order";
                        tpt.Skcode = order.Skcode;
                        tpt.WarehouseName = order.WarehouseName;
                        tpt.OnlineAmount = PcData.amount;
                        //tpt.OnlineServiceTax = re.OnlineServiceTax;
                        tpt.CreatedDate = indianTime;
                        tpt.ResponseMessage = PcData.GatewayResponse;
                        authContext.TrupayTransactionDB.Add(tpt);
                    }

                    #region InsertCheckforLedgerTable
                    if (AppConstants.IsUsingLedgerHitOnOnlinePayment)
                    {
                        //if ((PcData.PaymentFrom.ToLower() == "hdfc" ||
                        //    PcData.PaymentFrom.ToLower() == "epaylater" ||
                        //    PcData.PaymentFrom.ToLower() == "chqbook" ||
                        //    PcData.PaymentFrom.ToLower() == "credit hdfc" ||
                        //    PcData.PaymentFrom.ToLower() == "razorpay qr")
                        //    && PcData.status == "Success")
                        if (PcData.PaymentFrom.ToLower() != "cash" && PcData.status == "Success")
                        {
                            if (authContext.OnlinePaymentDtlsForLedgerDB.FirstOrDefault(z => z.OrderId == PcData.OrderId && z.TransactionId == PcData.GatewayTransId) == null)
                            {

                                OnlinePaymentDtlsForLedger Opdl = new OnlinePaymentDtlsForLedger();
                                Opdl.OrderId = PcData.OrderId;
                                Opdl.IsPaymentSuccess = true;
                                Opdl.IsLedgerAffected = "Yes";
                                Opdl.PaymentDate = DateTime.Now;
                                Opdl.TransactionId = PcData.GatewayTransId;
                                Opdl.IsActive = true;
                                Opdl.CustomerId = cust.CustomerId;
                                authContext.OnlinePaymentDtlsForLedgerDB.Add(Opdl);
                            }
                        }
                        //}
                        #endregion
                        //authContext.Commit();
                    }
                    //else
                    //{
                    //    return Request.CreateResponse(HttpStatusCode.OK, "Something Went Wrong...Try Again");
                    //}

                    await authContext.CommitAsync();


                    if (PcData.PaymentFrom == "ScaleUp" && PcData.status == "Success")
                    {
                        try
                        {
                            BusinessLayer.Managers.OrderMasterManager manager = new BusinessLayer.Managers.OrderMasterManager();
                            BackgroundTaskManager.Run(() => manager.GetOrderInvoiceHtml(PcData.OrderId));
                        }
                        catch (Exception ex)
                        {
                        }
                    }


                    if (PayNowncustomerIds.Any(x => x == order.CustomerId))
                    {
                        PayNowncustomerIds.Remove(order.CustomerId);
                    }
                }


                if (paymentInprocess.Any(x => x == PcData.GatewayTransId))
                {
                    paymentInprocess.Remove(PcData.GatewayTransId);
                }

                return Request.CreateResponse(HttpStatusCode.OK, true);
            }
            else if (PcData != null && PcData.IsPayLater == true)
            {
                using (var authContext = new AuthContext())
                {
                    var paylaterdata = authContext.PayLaterCollectionDb.FirstOrDefault(x => x.OrderId == PcData.OrderId && x.IsActive == true && x.IsDeleted == false);
                    if (paylaterdata != null)
                    {
                        double totalPaidAmount = 0;
                        var paylaterhistory = authContext.PayLaterCollectionHistoryDb.Where(x => x.PayLaterCollectionId == paylaterdata.Id && x.IsActive == true && x.IsDeleted == false && x.PaymentStatus == 1).ToList();
                        if (paylaterhistory != null && paylaterhistory.Any())
                        {
                            //double paidamount = 0;
                            //double returnamount = 0;
                            //double gullakamount = 0;
                            //paidamount = paylaterhistory.Any(a => a.Comment != "Return Order" && a.Comment != "Gullak Refund") ? paylaterhistory.Where(a => a.Comment != "Return Order" && a.Comment != "Gullak Refund").Sum(y => y.Amount) : 0;
                            //returnamount = paylaterhistory.Any(a => a.Comment == "Return Order") ? paylaterhistory.Where(a => a.Comment == "Return Order").Sum(y => y.Amount) : 0;
                            //gullakamount = paylaterhistory.Any(a => a.Comment == "Gullak Refund") ? paylaterhistory.Where(a => a.Comment == "Gullak Refund").Sum(y => y.Amount) : 0;
                            //totalPaidAmount = paylaterhistory.Sum(x => x.Amount);
                            List<PayLaterAmountUpdateDC> list = new List<PayLaterAmountUpdateDC>();
                            foreach (var item in paylaterhistory)
                            {
                                PayLaterAmountUpdateDC singlepayment = new PayLaterAmountUpdateDC();
                                singlepayment.Amount = item.Amount;
                                singlepayment.Comment = item.Comment;
                                list.Add(singlepayment);
                            }
                            double getamount = 0;
                            CurrencyManagementController currencyManagementController = new CurrencyManagementController();
                            getamount = currencyManagementController.ReturnAmount(list);
                            //totalPaidAmount = paylaterdata.Amount - returnamount - paidamount + gullakamount;
                            totalPaidAmount = getamount;
                        }

                        if (paylaterdata.Amount > totalPaidAmount + PcData.amount)
                        {
                            return Request.CreateResponse(HttpStatusCode.OK, "Amount is greater then Total Amount");
                        }
                    }
                    Model.CashManagement.PayLaterRequestResponseMsg msg = new Model.CashManagement.PayLaterRequestResponseMsg();
                    msg.Amount = PcData.amount;
                    msg.Type = "Request";
                    msg.orderId = PcData.OrderId;
                    msg.CreatedDate = indianTime;
                    msg.IsActive = true;
                    msg.IsDeleted = false;
                    msg.RequestResponse = decData;
                    authContext.PayLaterRequestResponseMsgs.Add(msg);
                    await authContext.CommitAsync();
                }
                return Request.CreateResponse(HttpStatusCode.OK, true);
            }
            else
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Invalid Data");
        }

        /// <summary>
        /// Update Order Status to Pending if Payment is succeed and Falied if status is failed
        /// </summary>
        /// <param name="paymentReq"></param>
        /// <returns></returns>
        [Route("UpdateOrderForOnlinePaymentV2")]
        [HttpPost]
        [AllowAnonymous]
        public async Task<HttpResponseMessage> UpdateOrderForOnlinePaymentV2(PaymentReq paymentReq)
        {
            string oldstatus = "";
            OnlinePaymentResponse response = null;
            string decData = string.Empty;
            try
            {
                AES256 aes = new AES256();
                //string redisAesKey = "Sh0pK!r@n@#@!@#$";
                string redisAesKey = DateTime.Now.ToString("yyyyMMdd") + "1201";

                decData = aes.Decrypt(paymentReq.EncString, redisAesKey);
                response = JsonConvert.DeserializeObject<OnlinePaymentResponse>(decData);
            }
            catch (Exception ex)
            {
                logger.Error("Error while decrypding data: " + ex.ToString());
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Invalid Data");
            }

            bool IsStopOrderToPending = false;
            if (response != null && response.IsPayLater == false)
            {
                //Update status of Order to pending and insert a record in 
                using (var authContext = new AuthContext())
                {
                    var order = await authContext.DbOrderMaster.Where(x => x.OrderId == response.OrderId).FirstOrDefaultAsync();
                    oldstatus = order.Status;
                    var cust = await authContext.Customers.FirstOrDefaultAsync(x => x.CustomerId == order.CustomerId);
                    if (!string.IsNullOrEmpty(response.OrderPaymentMode))
                        order.paymentMode = response.OrderPaymentMode;
                    order.UpdatedDate = indianTime;

                    if (!response.IsSuccess)
                    {
                        //// add wallet points, minus reward points
                        //var rpoint = authContext.RewardPointDb.Where(c => c.CustomerId == order.CustomerId).SingleOrDefault();
                        //rpoint.EarningPoint -= order.RewardPoint;
                        //rpoint.UpdatedDate = indianTime;
                        //authContext.Entry(rpoint).State = EntityState.Modified;

                        //var wallet = authContext.WalletDb.Where(c => c.CustomerId == order.CustomerId).SingleOrDefault();

                        //CustomerWalletHistory CWH = new CustomerWalletHistory();
                        //if (order.walletPointUsed.HasValue && order.walletPointUsed.Value > 0)
                        //{
                        //    CWH.WarehouseId = order.WarehouseId;
                        //    CWH.CompanyId = order.CompanyId;
                        //    CWH.CustomerId = wallet.CustomerId;
                        //    CWH.Through = "Due To Failed Order";
                        //    CWH.NewAddedWAmount = order.walletPointUsed;
                        //    CWH.TotalWalletAmount = wallet.TotalAmount + order.walletPointUsed;
                        //    CWH.TotalEarningPoint = rpoint.EarningPoint * (-1);
                        //    CWH.CreatedDate = indianTime;
                        //    CWH.UpdatedDate = indianTime;
                        //    CWH.OrderId = order.OrderId;
                        //    authContext.CustomerWalletHistoryDb.Add(CWH);

                        //    //update in wallet
                        //    wallet.TotalAmount += order.walletPointUsed;
                        //    wallet.TransactionDate = indianTime;
                        //    authContext.Entry(wallet).State = EntityState.Modified;
                        //}
                    }
                    else
                    {
                        // move push notification here 
                        if (response.OrderPaymentMode.ToLower() == "cod")
                        {
                            var oldcash = authContext.PaymentResponseRetailerAppDb.Where(x => x.OrderId == response.OrderId && x.PaymentFrom == "Cash").ToList();
                            if (oldcash != null && oldcash.Any())
                            {
                                oldcash.ForEach(x =>
                                {
                                    x.status = "Failed";
                                    x.UpdatedDate = indianTime;
                                    x.statusDesc = "Due to Cash Amount Changed while Placing Order";
                                    x.IsRefund = false;
                                    authContext.Entry(x).State = EntityState.Modified;
                                });
                            }
                            #region Get Customer COD Limit
                            CustomerCODLimitDc codLimitDc = await GetCustomerCODLimit(cust.CustomerId);
                            if (codLimitDc != null && codLimitDc.CODLimit > 0 && codLimitDc.IsCustomCODLimit == true)
                            {
                                if ((codLimitDc.AvailableCODLimit - response.AmountPaid) < 0)
                                {
                                    IsStopOrderToPending = true;
                                }
                            }

                            #endregion

                            if (!IsStopOrderToPending)
                            {
                                var PaymentResponseRetailerAppDb = new PaymentResponseRetailerApp
                                {
                                    amount = Math.Round(response.AmountPaid, 0),
                                    CreatedDate = indianTime,
                                    currencyCode = "INR",
                                    OrderId = response.OrderId,
                                    PaymentFrom = "Cash",
                                    status = "Success",
                                    statusDesc = "Due to Cash Amount Changed while Placing Order",
                                    UpdatedDate = indianTime,
                                    IsRefund = false
                                };
                                authContext.PaymentResponseRetailerAppDb.Add(PaymentResponseRetailerAppDb);
                            }
                            order.UpdatedDate = indianTime;
                            order.CreatedDate = indianTime;
                        }

                        //if (cust.ordercount > 1)
                        //{
                        //    try
                        //    {
                        //        authContext.ForNotification(cust.CustomerId, order.GrossAmount);
                        //    }
                        //    catch (Exception ex) { logger.Error(ex.ToString()); }

                        //}
                    }


                    if (order.Status == "Failed" || order.Status == "Payment Pending" || order.Status == "InTransit")
                    {
                        order.Status = (response.IsSuccess && IsStopOrderToPending == false) ? "Pending" : "Failed";
                        //  order.Status = response.IsSuccess ? "Pending" : (order.Status != "InTransit" ? "Failed" : "InTransit");
                        //IsCustFirstOrder
                        if (order.Status == "Pending")
                        {
                            if (authContext.IsCustFirstOrder(order.CustomerId))
                            {
                                order.IsFirstOrder = true;
                            };
                        }
                    }
                    //if (order.Status == "InTransit")
                    //{
                    //    bool IsTransitstatuschanged = authContext.PaymentResponseRetailerAppDb.Any(x => x.OrderId == response.OrderId && x.PaymentFrom == "Cash" && x.amount > 0 && x.status == "Success");
                    //    if (!IsTransitstatuschanged) { order.Status = response.IsSuccess ? "Pending" : "Failed"; }
                    //}

                    #region Clearence (reverse the stock to live stock) 
                    if (order.OrderType == 8 && order.Status == "Failed" && oldstatus != "Failed" && oldstatus != "Order Canceled")
                    {
                        order.Deleted = true;
                        order.active = false;
                        order.UpdatedDate = DateTime.Now;
                        var param = new SqlParameter("@OrderId", order.OrderId);
                        var Isupdate = await authContext.Database.ExecuteSqlCommandAsync("exec [Clearance].[UpdateClPaymentFailed] @OrderId", param);
                    }
                    #endregion




                    if (!string.IsNullOrEmpty(response.PaymentThrough) && response.PaymentThrough.ToLower() == "scaleup")
                    {
                        TextFileLogHelper.TraceLog("scaleUp OrderCompleted initiate with order no:" + response.OrderId.ToString());
                        ScaleUpManager scaleUpManager = new ScaleUpManager();
                        var resp = await scaleUpManager.OrderCompleted(response.TransactionId, response.IsSuccess);
                        //var resp= await scaleUpManager.OrderInvoice(response.TransactionId, response.OrderId, order.CreatedDate, "https://uat.shopkirana.in/BO/139.pdf", response.IsSuccess);
                        var orderRTD = authContext.OrderDispatchedMasters.Any(x => x.OrderId == order.OrderId);
                        if (!resp.status && order.Status != "Pending" && !orderRTD)
                        {
                            order.Status = "Failed";
                        }
                        TextFileLogHelper.TraceLog("scaleUp OrderCompleted End with order no:" + response.OrderId.ToString() + "and status:" + resp.status.ToString());
                    }

                    authContext.Entry(order).State = EntityState.Modified;
                    await authContext.CommitAsync();
                }

                return Request.CreateResponse(HttpStatusCode.OK, true);
            }
            else if (response != null && response.IsPayLater == true)
            {
                if (response.IsSuccess)
                {
                    using (var authContext = new AuthContext())
                    {
                        var paylaterdata = authContext.PayLaterCollectionDb.FirstOrDefault(x => x.OrderId == response.OrderId && x.IsActive == true && x.IsDeleted == false);
                        if (paylaterdata != null)
                        {
                            var identity = User.Identity as ClaimsIdentity;
                            int userid = 0;
                            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

                            double totalPaidAmount = 0;
                            var paylaterhistory = authContext.PayLaterCollectionHistoryDb.Where(x => x.PayLaterCollectionId == paylaterdata.Id && x.IsActive == true && x.IsDeleted == false && x.PaymentStatus == 1).ToList();
                            if (paylaterhistory != null && paylaterhistory.Any())
                            {
                                //double paidamount = 0;
                                //double returnamount = 0;
                                //double gullakamount = 0;
                                //paidamount = paylaterhistory.Any(a => a.Comment != "Return Order" && a.Comment != "Gullak Refund") ? paylaterhistory.Where(a => a.Comment != "Return Order" && a.Comment != "Gullak Refund").Sum(y => y.Amount) : 0;
                                //returnamount = paylaterhistory.Any(a => a.Comment == "Return Order") ? paylaterhistory.Where(a => a.Comment == "Return Order").Sum(y => y.Amount) : 0;
                                //gullakamount = paylaterhistory.Any(a => a.Comment == "Gullak Refund") ? paylaterhistory.Where(a => a.Comment == "Gullak Refund").Sum(y => y.Amount) : 0;
                                //totalPaidAmount = paylaterhistory.Sum(x => x.Amount);
                                List<PayLaterAmountUpdateDC> list = new List<PayLaterAmountUpdateDC>();
                                foreach (var item in paylaterhistory)
                                {
                                    PayLaterAmountUpdateDC singlepayment = new PayLaterAmountUpdateDC();
                                    singlepayment.Amount = item.Amount;
                                    singlepayment.Comment = item.Comment;
                                    list.Add(singlepayment);
                                }
                                double getamount = 0;
                                CurrencyManagementController currencyManagementController = new CurrencyManagementController();
                                getamount = currencyManagementController.ReturnAmount(list);
                                //totalPaidAmount = paylaterdata.Amount - returnamount - paidamount + gullakamount;
                                totalPaidAmount = getamount;
                            }

                            double amount = 0;
                            string refno = string.Empty;
                            string paymentmode = string.Empty;
                            var requestdata = authContext.PayLaterRequestResponseMsgs.Where(x => x.orderId == response.OrderId && x.Type == "Request").OrderByDescending(y => y.CreatedDate).FirstOrDefault();
                            if (requestdata != null)
                            {
                                amount = requestdata.Amount;
                                var jsonobject = Newtonsoft.Json.Linq.JObject.Parse(requestdata.RequestResponse);
                                refno = (string)jsonobject["GatewayTransId"] != null ? (string)jsonobject["GatewayTransId"] : String.Empty;
                                paymentmode = (string)jsonobject["PaymentFrom"];
                            }
                            if (totalPaidAmount + amount > paylaterdata.Amount)
                            {
                                return Request.CreateResponse(HttpStatusCode.BadRequest, "Amount is greater then Total Amount");
                            }
                            Model.CashManagement.PayLaterRequestResponseMsg msg = new Model.CashManagement.PayLaterRequestResponseMsg();
                            msg.orderId = response.OrderId;
                            msg.Amount = amount;
                            msg.Type = "Response";
                            msg.CreatedDate = indianTime;
                            msg.CreatedBy = userid;
                            msg.IsActive = true;
                            msg.IsDeleted = false;
                            msg.RequestResponse = decData;
                            authContext.PayLaterRequestResponseMsgs.Add(msg);

                            Model.CashManagement.PayLaterCollectionHistory history = new Model.CashManagement.PayLaterCollectionHistory();
                            if ((totalPaidAmount + amount) == paylaterdata.Amount)
                            {
                                var payorderId = authContext.OnlineCollection.Where(x => x.Orderid == response.OrderId && x.PaymentFrom == "PayLater" && x.IsActive == true && x.IsDeleted == false).FirstOrDefault();
                                if (payorderId != null)
                                {
                                    payorderId.IsActive = false;
                                    payorderId.IsDeleted = true;
                                    payorderId.ModifiedBy = userid;
                                    payorderId.ModifiedDate = DateTime.Now;
                                    authContext.Entry(payorderId).State = EntityState.Modified;
                                }

                                history.Amount = amount;
                                history.CreatedDate = indianTime;
                                history.PaymentMode = paymentmode;
                                history.CurrencyHubStockId = 0;
                                history.IsActive = true;
                                history.IsDeleted = false;
                                history.Comment = "Retailer Pay Now";
                                history.RefNo = refno;
                                history.PayLaterCollectionId = paylaterdata.Id;
                                history.CreatedBy = userid;
                                history.PaymentStatus = 1;

                                authContext.PayLaterCollectionHistoryDb.Add(history);
                                paylaterdata.Status = Convert.ToInt32(AngularJSAuthentication.API.DataContract.PayCollectionEnum.Paid);
                                paylaterdata.ModifiedDate = DateTime.Now;
                                paylaterdata.ModifiedBy = 1;
                                authContext.Entry(paylaterdata).State = EntityState.Modified;

                                #region ordermaster settle 
                                CashCollectionNewController ctrl = new CashCollectionNewController();
                                bool res = ctrl.OrderSettle(authContext, response.OrderId);
                                #endregion
                            }
                            else
                            {
                                history.Amount = amount;
                                history.CreatedDate = indianTime;
                                history.PaymentMode = paymentmode;
                                history.CurrencyHubStockId = 0;
                                history.IsActive = true;
                                history.IsDeleted = false;
                                history.Comment = "Retailer Pay Now";
                                history.RefNo = refno;
                                history.PayLaterCollectionId = paylaterdata.Id;
                                history.CreatedBy = userid;
                                history.PaymentStatus = 1;

                                authContext.PayLaterCollectionHistoryDb.Add(history);
                                if (paylaterdata.Status == 2 || paylaterdata.Status == 3) { }
                                else
                                {
                                    paylaterdata.Status = Convert.ToInt32(AngularJSAuthentication.API.DataContract.PayCollectionEnum.Partial);
                                }
                                paylaterdata.ModifiedDate = DateTime.Now;
                                paylaterdata.ModifiedBy = 1;
                                authContext.Entry(paylaterdata).State = EntityState.Modified;
                            }

                            await authContext.CommitAsync();
                            int customerid = authContext.DbOrderMaster.FirstOrDefault(x => x.OrderId == response.OrderId).CustomerId;
                            if (AppConstants.IsUsingLedgerHitOnOnlinePayment)
                            {
                                string gatewayid = "SkPayLater" + history.Id;
                                if (authContext.OnlinePaymentDtlsForLedgerDB.FirstOrDefault(z => z.OrderId == response.OrderId && z.TransactionId == gatewayid) == null)
                                {

                                    OnlinePaymentDtlsForLedger Opdl = new OnlinePaymentDtlsForLedger();
                                    Opdl.OrderId = response.OrderId;
                                    Opdl.IsPaymentSuccess = true;
                                    Opdl.IsLedgerAffected = "Yes";
                                    Opdl.PaymentDate = DateTime.Now;
                                    Opdl.TransactionId = gatewayid;
                                    Opdl.IsActive = true;
                                    Opdl.CustomerId = customerid;
                                    authContext.OnlinePaymentDtlsForLedgerDB.Add(Opdl);
                                }
                            }
                            await authContext.CommitAsync();
                        }
                    }

                }
                return Request.CreateResponse(HttpStatusCode.OK, true);

            }
            else
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Invalid Data");
        }


        #endregion


        #region EpayLater Customer Form

        [Route("AddEpaycust")]
        [AcceptVerbs("POST")]
        [AllowAnonymous]
        public HttpResponseMessage AddepayCust(EpayLaterForm customer)
        {
            using (AuthContext db = new AuthContext())
            {

                customerEpayDetail res;
                logger.Info("start addCustomerForm: ");
                using (var context = new AuthContext())
                {
                    try
                    {

                        EpayLaterForm customerdata = db.EpayLaterFormDB.Where(x => x.CustomerId == customer.CustomerId || x.Mobile.Trim().Equals(customer.Mobile.Trim())).FirstOrDefault();

                        EpayLaterForm c = new EpayLaterForm();
                        if (customerdata == null)
                        {
                            logger.Info("End  addCustomerForm: ");
                            c.CustomerId = customer.CustomerId;
                            c.ShopName = customer.ShopName;
                            c.FirmType = customer.FirmType;
                            c.ProprietorFirstName = customer.ProprietorFirstName;
                            c.ProprietorLastName = customer.ProprietorLastName;
                            c.Gender = customer.Gender;
                            c.Mobile = customer.Mobile;
                            c.Email = customer.Email;
                            c.DOB = customer.DOB;
                            c.PAN_No = customer.PAN_No;
                            c.Country = customer.Country;
                            c.State = customer.State;
                            c.City = customer.City;
                            c.PostalCode = customer.PostalCode;
                            c.CreatedDate = indianTime;
                            c.UpdatedDate = indianTime;
                            c.FSSAIImagePath = customer.FSSAIImagePath;
                            c.LicenseImagePath = customer.LicenseImagePath;
                            c.GSTImagePath = customer.GSTImagePath;
                            c.GovtRegNumberImagePath = customer.GovtRegNumberImagePath;
                            c.lat = customer.lat;
                            c.lg = customer.lg;
                            c.WhatsAppNumber = customer.WhatsAppNumber;
                            context.EpayLaterFormDB.Add(c);
                            context.Commit();

                            res = new customerEpayDetail()
                            {
                                customers = c,
                                Status = true,
                                Message = "Registration successfully."
                            };
                            return Request.CreateResponse(HttpStatusCode.OK, res);
                        }
                        else
                        {

                            customerdata.CustomerId = customer.CustomerId;
                            customerdata.ShopName = customer.ShopName;
                            customerdata.FirmType = customer.FirmType;
                            customerdata.ProprietorFirstName = customer.ProprietorFirstName;
                            customerdata.ProprietorLastName = customer.ProprietorLastName;
                            customerdata.Gender = customer.Gender;
                            customerdata.Mobile = customer.Mobile;
                            customerdata.Email = customer.Email;
                            customerdata.DOB = customer.DOB;
                            customerdata.PAN_No = customer.PAN_No;
                            customerdata.Country = customer.Country;
                            customerdata.State = customer.State;
                            customerdata.City = customer.City;
                            customerdata.PostalCode = customer.PostalCode;
                            customerdata.UpdatedDate = indianTime;
                            customerdata.FSSAIImagePath = customer.FSSAIImagePath;
                            customerdata.LicenseImagePath = customer.LicenseImagePath;
                            customerdata.GSTImagePath = customer.GSTImagePath;
                            customerdata.GovtRegNumberImagePath = customer.GovtRegNumberImagePath;
                            customerdata.lat = customer.lat;
                            customerdata.lg = customer.lg;
                            customerdata.WhatsAppNumber = customer.WhatsAppNumber;
                            context.Entry(customerdata).State = EntityState.Modified;
                            context.Commit();

                            res = new customerEpayDetail()
                            {
                                customers = customerdata,
                                Status = true,
                                Message = "Customer already exist."
                            };
                            return Request.CreateResponse(HttpStatusCode.OK, res);
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.Error("Error in addCustomerForm " + ex.Message);
                        res = new customerEpayDetail()
                        {
                            customers = null,
                            Status = false,
                            Message = "something went wrong."
                        };
                        return Request.CreateResponse(HttpStatusCode.BadRequest, res);
                    }
                }
            }
        }



        [Route("GetepayCust")]
        [HttpGet]
        [AllowAnonymous]
        public HttpResponseMessage GetepayCust(int customerId)
        {
            using (AuthContext db = new AuthContext())
            {

                customerEpayDetail res;
                logger.Info("start addCustomerForm: ");
                try
                {

                    EpayLaterForm customerdata = db.EpayLaterFormDB.Where(x => x.CustomerId == customerId).FirstOrDefault();

                    res = new customerEpayDetail()
                    {
                        customers = customerdata,
                        Status = true
                    };
                    return Request.CreateResponse(HttpStatusCode.OK, res);

                }
                catch (Exception ex)
                {
                    logger.Error("Error in addCustomerForm " + ex.Message);
                    res = new customerEpayDetail()
                    {
                        customers = null,
                        Status = false,
                        Message = "something went wrong."
                    };
                    return Request.CreateResponse(HttpStatusCode.BadRequest, res);
                }
            }
        }
        #endregion

        #region EpayLater Partner Form
        [Route("AddEpayPartner")]
        [AcceptVerbs("POST")]
        public HttpResponseMessage AddPartner(EpayLaterPartner customer)
        {
            using (AuthContext db = new AuthContext())
            {
                customerEpaypartnerDetail res;
                logger.Info("start AddPartner: ");
                using (var context = new AuthContext())
                    try
                    {
                        EpayLaterForm customerdata = db.EpayLaterFormDB.Where(x => x.CustomerId == customer.CustomerId || x.Mobile.Trim().Equals(customer.Mobile.Trim())).FirstOrDefault();


                        EpayLaterPartner c = new EpayLaterPartner();
                        //Customer customers = context.Customers.Where(s => s.Mobile.Trim().Equals(customer.Mobile.Trim())).FirstOrDefault();
                        if (customerdata == null)
                        {
                            logger.Info("End  AddPartner: ");
                            c.CustomerId = customer.CustomerId;
                            c.PartnerFristName = customer.PartnerFristName;
                            c.PartnerLastName = customer.PartnerLastName;
                            c.Gender = customer.Gender;
                            c.Mobile = customer.Mobile;
                            c.Email = customer.Email;
                            c.DOB = customer.DOB;
                            c.PartnerPan_No = customer.PartnerPan_No;
                            c.Country = customer.Country;
                            c.State = customer.State;
                            c.City = customer.City;
                            c.PostalCode = customer.PostalCode;
                            c.CreatedDate = indianTime;
                            c.UpdatedDate = indianTime;
                            c.WhatsAppNumber = customer.WhatsAppNumber;
                            context.EpayLaterPartnerDB.Add(c);
                            context.Commit();

                            res = new customerEpaypartnerDetail()
                            {
                                customers = c,
                                Status = true,
                                Message = "Registration successfully."
                            };
                            return Request.CreateResponse(HttpStatusCode.OK, res);
                        }
                        else
                        {
                            res = new customerEpaypartnerDetail()
                            {
                                customers = customer,
                                Status = false,
                                Message = "Customer already exist."
                            };
                            return Request.CreateResponse(HttpStatusCode.OK, res);
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.Error("Error in AddPartner " + ex.Message);
                        res = new customerEpaypartnerDetail()
                        {
                            customers = null,
                            Status = false,
                            Message = "something went wrong."
                        };
                        return Request.CreateResponse(HttpStatusCode.BadRequest, res);
                    }
            }
        }
        #endregion

        #region Get Payment Status
        [Route("Getpaymentstatus")]
        [HttpGet]
        // [AllowAnonymous]
        public List<PaymentResponseRetailerAppDc> getpayment(int OrderId)
        {
            using (var db = new AuthContext())
            {
                var Query = "exec GetOrderpayment " + OrderId;
                var result = db.Database.SqlQuery<PaymentResponseRetailerAppDc>(Query).ToList();
                return result;

            }
        }
        [Route("GetChangeOnlieOrderMop")]
        [HttpGet]
        // [AllowAnonymous]
        public bool changeonlineordermop(int OrderId, int CustomerId, string FromMOP, string ToMOP, string tranId)
        {
            bool result = false;
            using (var db = new AuthContext())
            {

                var OId = new SqlParameter("@OrderId", OrderId);
                var CId = new SqlParameter("@CustomerCode", CustomerId);
                var FromMo = new SqlParameter("@FromMOP", FromMOP);
                var ToMO = new SqlParameter("@ToMOP", ToMOP);
                var TId = new SqlParameter("@tranId", tranId);

                result = db.Database.SqlQuery<bool>("EXEC TestChangeOnlineOrderMOP @OrderId,@CustomerCode,@FromMOP,@ToMOP,@tranId", OId, CId, FromMo, ToMO, TId).FirstOrDefault();

            }
            return result;
        }


        [Route("GetRetailerOrderPayment")]
        [HttpGet]
        public List<RetailerOrderPaymentDc> GetRetailerOrderPayment(int OrderId)
        {
            using (var db = new AuthContext())
            {
                var Query = "exec GetRetailerOrderPayment " + OrderId;
                var result = db.Database.SqlQuery<RetailerOrderPaymentDc>(Query).ToList();
                return result;
            }
        }

        #endregion

        /// <summary>
        /// Update Missing Barcode
        /// </summary>
        /// <param name="orderid"></param>
        /// <returns></returns>
        [Route("UpdateOrder/Barcode")]
        [HttpGet]
        public HttpResponseMessage Barcodeupdate(int orderid)
        {
            try
            {
                if (orderid != 0)
                {


                    using (var context = new AuthContext())
                    {
                        var od = context.DbOrderMaster.Where(x => x.OrderId == orderid).FirstOrDefault();
                        if (od != null)
                        {

                            try
                            {
                                string Borderid = Convert.ToString(orderid);
                                string BorderCodeId = Borderid.PadLeft(11, '0');
                                temOrderQBcode code = context.GetBarcode(BorderCodeId);
                                od.InvoiceBarcodeImage = code.BarcodeImage;
                                if (string.IsNullOrEmpty(od.invoice_no))
                                {
                                    od.invoice_no = "Od_" + Convert.ToString(orderid);
                                }


                            }
                            catch (Exception es) { }
                            context.Entry(od).State = EntityState.Modified;

                        }

                        var odd = context.OrderDispatchedMasters.Where(x => x.OrderId == orderid).FirstOrDefault();
                        if (odd != null)
                        {

                            try
                            {
                                string Borderid = Convert.ToString(orderid);
                                string BorderCodeId = Borderid.PadLeft(11, '0');
                                temOrderQBcode code = context.GetBarcode(BorderCodeId);
                                odd.InvoiceBarcodeImage = code.BarcodeImage;
                                if (string.IsNullOrEmpty(od.invoice_no))
                                {
                                    odd.invoice_no = "Od_" + Convert.ToString(orderid);
                                }

                            }
                            catch (Exception es) { }
                            context.Entry(odd).State = EntityState.Modified;

                        }
                        context.Commit();
                    }
                }

                return Request.CreateResponse(HttpStatusCode.OK, true);
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex.Message);
            }
        }





        #region  Post Wheel DialValue of Played Wheel After Place Order App
        /// <summary>
        /// Post DialValue of Played Wheel After Place Order 
        /// Date; 09/08/2019
        /// </summary>
        /// <param name="sc"></param>
        /// <returns></returns>
        [Route("V3/PostOrderDialValue")]
        [HttpPost]
        [Authorize]
        public HttpResponseMessage PostDialValueWheelPalyed(PlacedOrderMasterDTM sc)
        {
            using (var context = new AuthContext())
            {
                WheelMaster obj;
                logger.Info("start PostDialValueWheelPalyed V3: ");
                OrderMaster Od = context.DbOrderMaster.Where(x => x.CustomerId == sc.CustomerId && x.Deleted == false && x.OrderId == sc.OrderId).FirstOrDefault();
                if (Od != null)
                {
                    string message = !string.IsNullOrEmpty(sc.lang) && sc.lang.Trim() == "hi" ? "डायल प्वाइंट सफलतापूर्वक जोड़ा गया!" : "Dial Point Added Successfully!";
                    ////Wheel Update
                    DialValuePoint Dl = context.DialValuePointDB.FirstOrDefault(x => x.OrderId == Od.OrderId);
                    if (Dl != null)
                    {
                        if (sc.DialEarnigPoint > 0)
                        {
                            Dl.PlayedWheelCount = Dl.PlayedWheelCount.HasValue ? Dl.PlayedWheelCount.Value + 1 : 1;
                            Dl.point = Dl.point + sc.DialEarnigPoint;
                            Dl.IsPlayWeel = true;
                            context.Entry(Dl).State = EntityState.Modified;

                            Od.RewardPoint = Od.RewardPoint + sc.DialEarnigPoint;
                            Od.UpdatedDate = indianTime;
                            context.Entry(Od).State = EntityState.Modified;
                        }
                        else
                        {
                            Dl.PlayedWheelCount = 0;
                            Dl.IsPlayWeel = true;
                            context.Entry(Dl).State = EntityState.Modified;
                        }
                        if (context.Commit() > 0)
                        {
                            obj = new WheelMaster()
                            {
                                WheelDetails = new WheelDetails
                                {
                                    OrderId = Dl.OrderId,
                                    WheelPlayedCount = Dl.PlayedWheelCount,
                                    WheelPlayedEarnigPoint = Dl.point
                                },
                                Status = true,
                                Message = message
                            };
                            return Request.CreateResponse(HttpStatusCode.OK, obj);
                        }
                        else if (sc.DialEarnigPoint == 0)
                        {
                            obj = new WheelMaster()
                            {
                                WheelDetails = null,
                                Status = true,
                                Message = message
                            };
                            return Request.CreateResponse(HttpStatusCode.OK, obj);
                        }
                    }
                    else
                    {
                        DialValuePoint DialValue = new DialValuePoint();
                        if (sc.DialEarnigPoint == 0)
                        {
                            DialValue.PlayedWheelCount = 0;// for skip wheel 
                        }
                        else
                        { DialValue.PlayedWheelCount = 1; }

                        DialValue.point = sc.DialEarnigPoint;
                        DialValue.OrderId = Od.OrderId;
                        DialValue.Skcode = Od.Skcode;
                        DialValue.CustomerId = Od.CustomerId;
                        DialValue.ShopName = Od.ShopName;
                        DialValue.OrderAmount = Od.GrossAmount;
                        DialValue.CreatedDate = indianTime;
                        DialValue.IsPlayWeel = true;
                        context.DialValuePointDB.Add(DialValue);

                        Od.RewardPoint = Od.RewardPoint + sc.DialEarnigPoint;
                        Od.UpdatedDate = indianTime;
                        context.Entry(Od).State = EntityState.Modified;

                        if (context.Commit() > 0)
                        {
                            obj = new WheelMaster()
                            {
                                WheelDetails = new WheelDetails
                                {
                                    OrderId = DialValue.OrderId,
                                    WheelPlayedCount = DialValue.PlayedWheelCount,
                                    WheelPlayedEarnigPoint = DialValue.point
                                },
                                Status = true,
                                Message = message
                            };
                            return Request.CreateResponse(HttpStatusCode.OK, obj);
                        }
                        else
                        {
                            obj = new WheelMaster()
                            {
                                WheelDetails = null,
                                Status = false,
                                Message = "Some thing went wrong"
                            };
                            return Request.CreateResponse(HttpStatusCode.OK, obj);
                        }
                    }
                }
                obj = new WheelMaster()
                {
                    WheelDetails = null,
                    Status = false,
                    Message = "This Customer order Doesn't Found."
                };
                return Request.CreateResponse(HttpStatusCode.OK, obj);
            }
        }
        #endregion


        //#region Update ETA of Order  
        //[Route("UpdateETA")]
        //[HttpPost]
        //public async Task<HttpResponseMessage> UpdateETAofOrder(UpdateETAOfOrderDc UpdateETA)
        //{
        //    int customerId = 0;
        //    var loggedInUser = HttpContext.Current != null && HttpContext.Current.User != null && HttpContext.Current.User.Identity != null ? HttpContext.Current.User.Identity.Name : "System";

        //    if ((string.IsNullOrEmpty(loggedInUser) || loggedInUser == "RetailerApp")
        //       && HttpContext.Current != null && HttpContext.Current.Request != null && HttpContext.Current.Request.Headers.AllKeys.Any(x => x == "username"))
        //    {
        //        loggedInUser = Convert.ToString(HttpContext.Current.Request.Headers.GetValues("username").FirstOrDefault());

        //        customerId = loggedInUser.Split('_').Length > 1 ? Convert.ToInt32(loggedInUser.Split('_')[1]) : 0;
        //    }

        //    bool result = false;
        //    using (var context = new AuthContext())
        //    {
        //        var order = await context.DbOrderMaster.FirstOrDefaultAsync(x => x.OrderId == UpdateETA.OrderId);

        //        if (order != null && UpdateETA.ETADate != null && order.Status == "Pending")
        //        {
        //            order.Deliverydate = UpdateETA.ETADate;
        //            order.UpdatedDate = indianTime;
        //            context.Entry(order).State = EntityState.Modified;
        //            await context.CommitAsync();
        //            result = true;

        //            if (customerId == 0)
        //            {
        //                var customers = context.Customers.FirstOrDefault(x => x.fcmId != null && x.CustomerId == order.CustomerId);
        //                if (customers != null)
        //                {
        //                    string Message = "ETA for your Order Id " + order.OrderId + " has been updated to " + order.Deliverydate;
        //                    string title = "Delivery ETA Updated";
        //                    ForNotificationpostordercancellation(Message, title, customers.fcmId);

        //                }

        //            }

        //        }
        //    }
        //    return Request.CreateResponse(HttpStatusCode.OK, result);
        //}



        #region Update ETA of Order  
        [Route("UpdateETA")]
        [HttpPost]
        public async Task<HttpResponseMessage> UpdateETAofOrder(UpdateETAOfOrderDc UpdateETA)
        {
            int customerId = 0;
            var loggedInUser = HttpContext.Current != null && HttpContext.Current.User != null && HttpContext.Current.User.Identity != null ? HttpContext.Current.User.Identity.Name : "System";

            if ((string.IsNullOrEmpty(loggedInUser) || loggedInUser == "RetailerApp")
               && HttpContext.Current != null && HttpContext.Current.Request != null && HttpContext.Current.Request.Headers.AllKeys.Any(x => x == "username"))
            {
                loggedInUser = Convert.ToString(HttpContext.Current.Request.Headers.GetValues("username").FirstOrDefault());

                customerId = loggedInUser.Split('_').Length > 1 ? Convert.ToInt32(loggedInUser.Split('_')[1]) : 0;
            }

            bool result = false;
            using (var context = new AuthContext())
            {
                var order = await context.DbOrderMaster.FirstOrDefaultAsync(x => x.OrderId == UpdateETA.OrderId);
                if (order != null)
                {
                    var warehouse = await context.Warehouses.FirstOrDefaultAsync(x => x.WarehouseId == order.WarehouseId);
                    if (warehouse.IsAutoRTPRunning)
                    {
                        return Request.CreateResponse(HttpStatusCode.OK, false);
                    }
                }
                if (order != null && UpdateETA.ETADate != null && order.Status == "Pending")
                {
                    order.Deliverydate = UpdateETA.ETADate;
                    order.UpdatedDate = indianTime;
                    context.Entry(order).State = EntityState.Modified;
                    await context.CommitAsync();
                    result = true;

                    if (customerId == 0)
                    {
                        var customers = context.Customers.FirstOrDefault(x => x.fcmId != null && x.CustomerId == order.CustomerId);
                        if (customers != null)
                        {
                            string Message = "ETA for your Order Id " + order.OrderId + " has been updated to " + order.Deliverydate;
                            string title = "Delivery ETA Updated";
                            ForNotificationpostordercancellation(Message, title, customers.fcmId);

                        }

                    }

                }
            }
            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

        [Route("UpdateETANew")]//change as per Yogesh sir on 04-10-2023
        [HttpPost]
        [AllowAnonymous]
        public async Task<EtaReturnDc> UpdateETANewofOrder(UpdateETAOfOrderNewDc UpdateETA)
        {
            int customerId = 0;
            EtaReturnDc etaReturnDc = new EtaReturnDc();
            var loggedInUser = HttpContext.Current != null && HttpContext.Current.User != null && HttpContext.Current.User.Identity != null ? HttpContext.Current.User.Identity.Name : "System";

            if ((string.IsNullOrEmpty(loggedInUser) || loggedInUser == "RetailerApp")
               && HttpContext.Current != null && HttpContext.Current.Request != null && HttpContext.Current.Request.Headers.AllKeys.Any(x => x == "username"))
            {
                loggedInUser = Convert.ToString(HttpContext.Current.Request.Headers.GetValues("username").FirstOrDefault());

                customerId = loggedInUser.Split('_').Length > 1 ? Convert.ToInt32(loggedInUser.Split('_')[1]) : 0;
            }

            bool result = false;
            using (var context = new AuthContext())
            {
                var order = await context.DbOrderMaster.FirstOrDefaultAsync(x => x.OrderId == UpdateETA.OrderId);
                if (order != null)
                {
                    var warehouse = await context.Warehouses.FirstOrDefaultAsync(x => x.WarehouseId == order.WarehouseId);
                    if (warehouse.IsAutoRTPRunning)
                    {
                        etaReturnDc.status = false;
                        etaReturnDc.Message = !string.IsNullOrEmpty(UpdateETA.lan) && UpdateETA.lan.Trim() == "hi" ? "आप आर्डर की डिलीवरी डेट चेंज नहीं कर सकते क्यों की आप का आर्डर प्रोसेस हो चूका है |" : "Unable to Change ETA as Currently RTP is In Progress.Please try after sometime";

                        return etaReturnDc;//Request.CreateResponse(HttpStatusCode.OK, false);
                    }
                }
                if (order != null && UpdateETA.ETADate != null && (order.Status == "Pending" || order.Status == "Inactive"))
                {
                    order.Deliverydate = UpdateETA.ETADate;
                    order.ExpectedRtdDate = UpdateETA.ETADate.Date.AddDays(-1);
                    order.UpdatedDate = indianTime;
                    context.Entry(order).State = EntityState.Modified;
                    await context.CommitAsync();
                    //result = true;
                    etaReturnDc.status = true;
                    etaReturnDc.Message = "ETA Changed Successfully";

                    etaReturnDc.Message = !string.IsNullOrEmpty(UpdateETA.lan) && UpdateETA.lan.Trim() == "hi" ? "डिलीवरी की तारीख सफलतापूर्वक बदली गयी |" : "ETA Changed Successfully.";

                    if (customerId == 0)
                    {
                        var customers = context.Customers.FirstOrDefault(x => x.fcmId != null && x.CustomerId == order.CustomerId);
                        if (customers != null)
                        {
                            string Message = "ETA for your Order Id " + order.OrderId + " has been updated to " + order.Deliverydate;
                            string title = "Delivery ETA Updated";
                            ForNotificationpostordercancellation(Message, title, customers.fcmId);
                        }

                    }

                }
                else
                {
                    etaReturnDc.Message = "Only Pending Orders Eligible for Change ETA.";
                    etaReturnDc.status = false;
                }
            }
            return etaReturnDc;
            //return Request.CreateResponse(HttpStatusCode.OK, result);
        }




        private async Task<bool> ForNotificationpostordercancellation(string Message, string title, string FCMId)
        {
            bool Result = false;
            string Key = ConfigurationManager.AppSettings["FcmApiKey"];
            //string id11 = ConfigurationManager.AppSettings["FcmApiId"];
            //WebRequest tRequest = WebRequest.Create("https://fcm.googleapis.com/fcm/send") as HttpWebRequest;
            //tRequest.Method = "post";
            //var objNotification = new
            //{
            //    to = FCMId,
            //    notification = new
            //    {
            //        title = title,
            //        body = Message,
            //    }
            //};
            //string jsonNotificationFormat = Newtonsoft.Json.JsonConvert.SerializeObject(objNotification);
            //Byte[] byteArray = Encoding.UTF8.GetBytes(jsonNotificationFormat);
            //tRequest.Headers.Add(string.Format("Authorization: key={0}", Key));
            //tRequest.Headers.Add(string.Format("Sender: id={0}", id11));
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
            //            }
            //        }
            //    }
            //}
            var data = new FCMData
            {

                title = title,
                body = Message,
            };
            var firebaseService = new FirebaseNotificationServiceHelper(Key);
            var result = await firebaseService.SendNotificationForApprovalAsync(FCMId, data);
            if (result != null)
            {
                Result = true;
            }
            else
            {
                Result = false;
            }
            return Result;
        }
        #endregion


        #region  Get Customer Cod Limit
        [Route("GetCustomerCODLimit/{CustomerId}")]
        [HttpGet]
        public async Task<CustomerCODLimitDc> GetCustomerCODLimit(int CustomerId)
        {
            string ConsumerApptype = new OrderPlaceHelper().GetCustomerAppType();
            var result = new CustomerCODLimitDc();
            if (CustomerId > 0)
            {
                using (AuthContext db = new AuthContext())
                {
                    result = await db.Database.SqlQuery<CustomerCODLimitDc>("exec GetCustomerCODLimit {0}", CustomerId).FirstOrDefaultAsync();
                }
                if (!string.IsNullOrEmpty(ConsumerApptype) && ConsumerApptype.ToLower() == "consumer")
                    result.IsCustomCODLimit = false;
            }
            return result;
        }
        #endregion


        [Route("RemovePaymentInprocess/{GatewayTransId}")]
        [HttpGet]
        [AllowAnonymous]
        public bool RemovePaymentInprocess(string GatewayTransId)
        {
            bool result = false;
            if (!string.IsNullOrEmpty(GatewayTransId) && paymentInprocess.Any(x => x == GatewayTransId))
            {
                paymentInprocess.RemoveAll(x => x == GatewayTransId);
                result = true;
            }
            return result;
        }

        public bool InsertCustomerBucket(int CustomerId, int OrderId, DateTime OrderDate)
        {
            int IncreaseDay = 0;
            bool flag = false;
            bool IsFirstOrder = false;

            using (var context = new AuthContext())
            {

                MongoDbHelper<GameConditionMastersMongo> mongohelperIncreaseDay = new MongoDbHelper<GameConditionMastersMongo>();
                IncreaseDay = mongohelperIncreaseDay.Select(x => x.Name == "IncreaseDay" && x.IsActive == true && x.IsDeleted == false).Select(x => x.Value).FirstOrDefault();

                MongoDbHelper<GameCustomerBucketHdrOrderDetail> mongoDbHelperCustomerBucketHdrOrderDetail = new MongoDbHelper<GameCustomerBucketHdrOrderDetail>();

                MongoDbHelper<GameCustomerBucketHdr> mongoDbHelperCustomerBucketHdr = new MongoDbHelper<GameCustomerBucketHdr>();
                var dataCustomerBucketHdr = mongoDbHelperCustomerBucketHdr.Select(x => x.CustomerId == CustomerId && x.IsActive == true && x.IsDeleted == false).FirstOrDefault();

                if (dataCustomerBucketHdr == null)
                {
                    return flag;
                }

                long GameBucketRewardId = 0;
                GameBucketRewardId = mongoDbHelperCustomerBucketHdrOrderDetail.Select(x => x.CustomerId == CustomerId && x.BucketNo == dataCustomerBucketHdr.BucketNo && x.GameBucketNo == dataCustomerBucketHdr.GameBucketNo && x.GameBucketRewardId > 0).Select(y => y.GameBucketRewardId).FirstOrDefault();

                bool IsOrderInsert = true;
                var res = context.GameBucketRewards.FirstOrDefault(x => x.BucketNo == dataCustomerBucketHdr.BucketNo && x.IsActive == true && x.IsDeleted == false);
                if (res != null && GameBucketRewardId > 0 && res.Id != GameBucketRewardId)
                { res = context.GameBucketRewards.FirstOrDefault(x => x.Id == GameBucketRewardId); }

                if (res != null && !res.IsFix) // For Dynamic = false
                {
                    //if (!(EntityFunctions.TruncateTime(res.StartDate.Value) <= OrderDate
                    //   && EntityFunctions.TruncateTime(res.EndDate.Value) >= OrderDate.Date))
                    if (!(res.StartDate.Value.Date <= OrderDate.Date && res.EndDate.Value.Date >= OrderDate.Date))
                        IsOrderInsert = false;
                }


                int BucketNo = dataCustomerBucketHdr.BucketNo;
                DateTime BacktStartDate = (DateTime)dataCustomerBucketHdr.BucketStartDate;
                DateTime BacktEndDate = (DateTime)dataCustomerBucketHdr.BucketEndDate;
                int NextBucktNo = dataCustomerBucketHdr.NextBucketNo;
                int GameBucketNo = dataCustomerBucketHdr.GameBucketNo;

                External.Gamification.GamificationController gamificationController = new External.Gamification.GamificationController();

                if (!(dataCustomerBucketHdr.LastOrderDate.Value.Date >= dataCustomerBucketHdr.BucketStartDate.Date
                    && dataCustomerBucketHdr.LastOrderDate.Value.Date <= dataCustomerBucketHdr.BucketEndDate.Date)
                    || dataCustomerBucketHdr.BucketEndDate.Date < OrderDate.Date)
                {
                    GameBucketNo = GameBucketNo + 1;
                    dataCustomerBucketHdr.NextBucketNo = BucketNo + 1;
                    dataCustomerBucketHdr.GameBucketNo = GameBucketNo;
                    dataCustomerBucketHdr.BucketStartDate = OrderDate.Date;
                    dataCustomerBucketHdr.BucketEndDate = OrderDate.Date.AddDays(IncreaseDay - 1);
                    BacktStartDate = dataCustomerBucketHdr.BucketStartDate;
                    BacktEndDate = dataCustomerBucketHdr.BucketEndDate;
                    IsFirstOrder = true;
                }

                if (!dataCustomerBucketHdr.IsStreakCreated)
                {
                    dataCustomerBucketHdr.IsStreakCreated = true;
                    StreakCustomerDc obj = new StreakCustomerDc();
                    obj.CustomerId = CustomerId;
                    obj.NoOfPeriod = 0;//Configuration: GameConditionMasters Where Name='StreakDuration'
                    obj.StartDate = BacktStartDate;
                    obj.EndDate = BacktEndDate;
                    obj.BucketNo = BucketNo;
                    AsyncContext.Run(() => gamificationController.CreateStreakCustomerTransactionPeriod(obj));
                }

                dataCustomerBucketHdr.LastOrderDate = OrderDate;
                flag = mongoDbHelperCustomerBucketHdr.ReplaceWithoutFind(dataCustomerBucketHdr.Id, dataCustomerBucketHdr);

                if (IsOrderInsert)
                {
                    GameCustomerBucketHdrOrderDetail obj = new GameCustomerBucketHdrOrderDetail();
                    obj.CustomerId = CustomerId;
                    obj.OrderId = OrderId;
                    obj.GameBucketRewardId = res != null && IsFirstOrder ? res.Id : 0;
                    obj.BucketNo = BucketNo;
                    obj.GameBucketNo = GameBucketNo;
                    obj.BucketStartDate = BacktStartDate;
                    obj.BucketEndDate = BacktEndDate;
                    obj.RewardCredit = 0;
                    obj.RewardStatus = "InProcess";
                    obj.IsCompleted = false;
                    obj.CreatedBy = 1;
                    obj.CreatedDate = DateTime.Now;
                    obj.IsActive = true;
                    obj.IsDeleted = false;
                    flag = mongoDbHelperCustomerBucketHdrOrderDetail.Insert(obj);


                    #region CustomerLedger_InsertOrderId
                    List<CreateCustomerLedgerDc> InsertNewCustomerLedgerList = new List<CreateCustomerLedgerDc>();
                    CreateCustomerLedgerDc objNewLedgerEntry = new CreateCustomerLedgerDc();
                    if (res != null) // For Dynamic = false
                    {
                        objNewLedgerEntry.CustomerId = CustomerId;
                        objNewLedgerEntry.GameBucketNo = GameBucketNo;
                        objNewLedgerEntry.BucketNo = BucketNo;
                        objNewLedgerEntry.ForRewardStrack = 1;//"Reward=1  / Strack=2
                        objNewLedgerEntry.StreakIdFrom = 0;
                        objNewLedgerEntry.StreakIdTo = 0;
                        objNewLedgerEntry.GameBucketRewardId = res.Id;
                        objNewLedgerEntry.GameStreakLevelConfigMasterId = 0;
                        objNewLedgerEntry.GameStreakLevelConfigDetailId = 0;
                        objNewLedgerEntry.RewardValue = res.value;
                        objNewLedgerEntry.IsUpComingReward = true;
                        objNewLedgerEntry.IsCompleted = false;
                        objNewLedgerEntry.IsCanceled = false;
                        objNewLedgerEntry.BucketStartDate = BacktStartDate;
                        objNewLedgerEntry.BucketEndDate = BacktEndDate;

                        List<int> OrderIdList = new List<int>();
                        OrderIdList.Add(OrderId);
                        objNewLedgerEntry.OrderIdList = OrderIdList;

                        InsertNewCustomerLedgerList.Add(objNewLedgerEntry);

                        AsyncContext.Run(() => gamificationController.CustomerLedger_Create(InsertNewCustomerLedgerList));
                    }
                    #endregion
                }

                //validate configuration available
                AsyncContext.Run(() => gamificationController.InsertStreakOrder(CustomerId, OrderId, OrderDate));

            }
            return flag;
        }






        public class UpdateETAOfOrderDc
        {
            public int OrderId { set; get; }
            public DateTime ETADate { set; get; }
            //public string lan { get; set; }//new on 27-09-2023
        }
        public class UpdateETAOfOrderNewDc
        {
            public int OrderId { set; get; }
            public DateTime ETADate { set; get; }
            public string lan { get; set; }//new on 27-09-2023
        }

        public class customerEpayDetail
        {
            public EpayLaterForm customers { get; set; }
            public bool Status { get; set; }
            public string Message { get; set; }
        }

        public class customerEpaypartnerDetail
        {
            public EpayLaterPartner customers { get; set; }
            public bool Status { get; set; }
            public string Message { get; set; }
        }

        public class OnlinePaymentResponse
        {
            public string TransactionId { get; set; }
            public bool IsSuccess { get; set; }
            public string ResponseMessage { get; set; }
            public string PaymentMode { get; set; }
            public int OrderId { get; set; }
            public double AmountPaid { get; set; }
            public string PaymentThrough { get; set; }// Trupay , HDFC
            public string OrderPaymentMode { get; set; } //COD,Online
            public string RequestMessage { get; set; }
            public bool IsPayLater { get; set; }
        }

        public class PaymentResponseRetailerAppDTO
        {
            public PaymentResponseRetailerApp chdata { get; set; }
            public bool Status { get; set; }
            public string Message { get; set; }
        }
        public class PlacedOrderMaster
        {
            public PlacedOrderMasterDTM order { get; set; }
            public ShoppingCart Cart { get; set; }
            public bool Status { get; set; }
            public string Message { get; set; }
        }
        public class PlacedOrderMasterDTM
        {
            public int OrderId { get; set; }
            public double TotalAmount { get; set; }
            public int WheelCount { get; set; }
            public double WheelAmountLimit { get; set; }
            public int DialEarnigPoint { get; set; }
            public string Skcode { get; set; }
            public int CustomerId { get; set; }
            public int WarehouseId { get; set; }
            public int? PlayedWheelCount { get; set; }
            public string lang { get; set; }
        }

        public class epayltrDTO
        {
            public int marketplaceId { get; set; }
            public string emailAddress { get; set; }
            public double availableCreditLimit { get; set; }
            public string outstandingDue { get; set; }
            public string overdueAmount { get; set; }
            public DateTime createdDate { get; set; }
            public string canExceed { get; set; }
            public int marketplaceCustomerId { get; set; }
        }

        public class PaymentReq
        {
            public string EncString { get; set; }
        }



        /// <summary>
        /// / WheelMaster  V3
        /// </summary>
        public class WheelMaster
        {
            public WheelDetails WheelDetails { get; set; }
            public bool Status { get; set; }
            public string Message { get; set; }
        }
        public class WheelDetails
        {
            public int? OrderId { get; set; }
            public int? WheelPlayedCount { get; set; }
            public double? WheelPlayedEarnigPoint { get; set; }

        }

        public class PaymentResponseRetailerAppDc
        {

            public int OrderId { get; set; }
            public string GatewayOrderId { get; set; }
            public string GatewayTransId { get; set; }
            public double amount { get; set; }
            public string status { get; set; }
            public string PaymentFrom { get; set; }
            public string GatewayRequest { get; set; }
            public string PaymentThrough { get; set; }
            public DateTime CreatedDate { get; set; }
            public int ChequeStatus { get; set; }
            public string ChequeStatusText
            {
                get
                {
                    ChequeStatusEnum chequeStatusEnum = (ChequeStatusEnum)ChequeStatus;
                    if (chequeStatusEnum != null)
                        return chequeStatusEnum.ToString();
                    else
                        return "";
                }
            }

            public string statusDesc { get; set; }

            public bool IsRefund { get; set; }
            public int Fine { get; set; }
            public bool IsOnline { get; set; }
            public string DueDaysDesc { get; set; }
            public int id { get; set; }

        }

        public class CustomerCODLimitDc
        {
            public int CustomerId { get; set; }
            public double CODLimit { get; set; }
            public double AvailableCODLimit { get; set; }
            public bool IsActive { get; set; }
            public bool IsCustomCODLimit { get; set; }
        }
        //new on 22/09/2023
        public class EtaReturnDc
        {
            public bool status { get; set; }
            public string Message { get; set; }
        }

    }
}