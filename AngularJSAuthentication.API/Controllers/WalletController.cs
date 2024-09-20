using AgileObjects.AgileMapper;
using AngularJSAuthentication.API.Controllers.Base;
using AngularJSAuthentication.API.Helpers;
using AngularJSAuthentication.Model;
using GenricEcommers.Models;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using NLog;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Http;

namespace AngularJSAuthentication.API.Controllers
{
    [RoutePrefix("api/wallet")]
    public class WalletController : BaseAuthController
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private static TimeZoneInfo INDIAN_ZONE = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
        DateTime indianTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE);
        //[Authorize]
        [Route("")]
        public HttpResponseMessage Get()
        {
            logger.Info("start WalletList: ");
            using (AuthContext context = new AuthContext())
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
                    var pointList = (from i in context.WalletDb
                                     where i.Deleted == false
                                     join j in context.Customers on i.CustomerId equals j.CustomerId
                                     join k in context.Warehouses on j.Warehouseid equals k.WarehouseId into ts
                                     from k in ts.DefaultIfEmpty()
                                     select new
                                     {
                                         Id = i.Id,
                                         CustomerId = i.CustomerId,
                                         TotalAmount = i.TotalAmount,
                                         CreatedDate = i.CreatedDate,
                                         TransactionDate = i.TransactionDate,
                                         UpdatedDate = i.UpdatedDate,
                                         Skcode = j.Skcode,
                                         ShopName = j.ShopName,
                                         WarehouseName = k.WarehouseName,
                                         WarehouseId = j.Warehouseid,
                                         City = k.CityName
                                     }).ToList();
                    logger.Info("End  wallet: ");
                    return Request.CreateResponse(HttpStatusCode.OK, pointList);


                }
                catch (Exception ex)
                {
                    logger.Error("Error in WalletList " + ex.Message);
                    logger.Info("End  WalletList: ");
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex.Message); ;
                }
            }
        }
        [Route("Search")]
        [HttpGet]
        public HttpResponseMessage GetData(DateTime start, DateTime end, int WarehouseId)
        {
            logger.Info("start WalletList: ");
            using (AuthContext context = new AuthContext())
            {
                try
                {
                    var pointList = (from i in context.WalletDb
                                     where i.Deleted == false && i.CreatedDate >= start && i.CreatedDate <= end
                                     join j in context.Customers on i.CustomerId equals j.CustomerId
                                     where j.Warehouseid == WarehouseId
                                     join k in context.Warehouses on j.Warehouseid equals k.WarehouseId into ts
                                     from k in ts.DefaultIfEmpty()
                                     select new
                                     {
                                         Id = i.Id,
                                         CustomerId = i.CustomerId,
                                         TotalAmount = i.TotalAmount,
                                         CreatedDate = i.CreatedDate,
                                         TransactionDate = i.TransactionDate,
                                         UpdatedDate = i.UpdatedDate,
                                         Skcode = j.Skcode,
                                         ShopName = j.ShopName,
                                         WarehouseName = k.WarehouseName,
                                         WarehouseId = j.Warehouseid,
                                         City = k.CityName
                                     }).ToList();
                    logger.Info("End  wallet: ");
                    return Request.CreateResponse(HttpStatusCode.OK, pointList);
                }
                catch (Exception ex)
                {
                    logger.Error("Error in WalletList " + ex.Message);
                    logger.Info("End  WalletList: ");
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex.Message); ;
                }
            }
        }
        [Route("")]
        [AcceptVerbs("Get")]
        public HttpResponseMessage Get(int CustomerId)
        {
            logger.Info("start single  GetcusomerWallets: ");
            using (AuthContext context = new AuthContext())
            {
                WalletReward Item = new WalletReward();
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
                    logger.Info("in Wallets");

                    //if (Warehouse_id==0) {
                    //    Item.wallet = context.GetWalletbyCustomeridWid(CustomerId, Warehouse_id);
                    //   Item.reward = context.GetRewardbyCustomeridWid(CustomerId, Warehouse_id);
                    //    Item.conversion = context.CashConversionDb.Where(x => x.Warehouseid == Warehouse_id).FirstOrDefault();
                    //  //  Item.rewardConversion = context.RPConversionDb.FirstOrDefault();
                    //    return Request.CreateResponse(HttpStatusCode.OK, Item);

                    //}
                    //else {
                    //  Item.wallet = context.GetWalletbyCustomerid(CustomerId,compid);
                    // Item.reward = context.GetRewardbyCustomerid(CustomerId, compid);
                    // Item.conversion = context.CashConversionDb.FirstOrDefault();
                    //Item.rewardConversion = context.RPConversionDb.FirstOrDefault();


                    Item.wallet = context.GetWalletbyCustomerid(CustomerId);
                    Item.reward = context.GetRewardbyCustomerid(CustomerId);

                    Item.conversion = context.CashConversionDb.FirstOrDefault(x=>x.IsConsumer == false);
                    //Item.rewardConversion = context.RPConversionDb.FirstOrDefault();
                    return Request.CreateResponse(HttpStatusCode.OK, Item);
                    //  }

                }
                catch (Exception ex)
                {
                    logger.Error("Error in Get single GetcusomerWallets " + ex.Message);
                    logger.Info("End  single GetcusomerWallets: ");
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Got Error"); ;
                }
            }
        }


        [Route("cash")]
        [AcceptVerbs("Get")]
        public HttpResponseMessage GetCashConversion()
        {
            using (AuthContext context = new AuthContext())
            {
                CashConversion pointList = new CashConversion();
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

                    //if (Warehouse_id > 0) {
                    //    pointList = context.CashConversionDb.Where(x => x.Warehouseid == Warehouse_id).FirstOrDefault();
                    //    return Request.CreateResponse(HttpStatusCode.OK, pointList);
                    //}
                    //else {
                    pointList = context.CashConversionDb.FirstOrDefault(x=>x.IsConsumer == false);
                    return Request.CreateResponse(HttpStatusCode.OK, pointList);
                    // }

                }
                catch (Exception ex)
                {
                    logger.Error("Error in conversion " + ex.Message);
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Got Error");
                }
            }
        }
        [Route("customer")]
        public HttpResponseMessage GetCustomer(string skcode)
        {
            using (AuthContext context = new AuthContext())
            {
                logger.Info("start custmer wallet: ");
                Customer cust = new Customer();
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
                        cust = context.Customers.Where(x => x.Skcode.Equals(skcode) && x.Deleted == false).FirstOrDefault();

                        if (cust != null)
                        {
                            var Custs = context.Customers.Where(a => a.CustomerId == cust.CustomerId && a.Warehouseid == Warehouse_id).FirstOrDefault();

                            if (Custs != null)
                            {
                                logger.Info("End  custmer: ");
                                return Request.CreateResponse(HttpStatusCode.OK, cust);
                            }
                            else
                            {

                                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "No Customer found");
                            }
                        }
                        else
                        {
                            return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "No Customer found");
                        }
                    }


                    else
                    {

                        cust = context.Customers.Where(x => x.Skcode.ToLower().Equals(skcode.ToLower()) && x.Deleted == false).FirstOrDefault();

                        if (cust != null)
                        {
                            var Custs = context.Customers.Where(a => a.CustomerId == cust.CustomerId && a.CompanyId == compid).FirstOrDefault();

                            if (Custs != null)
                            {
                                logger.Info("End  custmer: ");
                                return Request.CreateResponse(HttpStatusCode.OK, cust);
                            }
                            else
                            {

                                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "No Customer found");
                            }
                        }
                        else
                        {
                            return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "No Customer found");
                        }

                    }
                }
                catch (Exception ex)
                {
                    logger.Error("Error in cusomer " + ex.Message);
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Got Error");
                }
            }
        }



        [Route("addmw")]
        [AcceptVerbs("Post")]
        public HttpResponseMessage postmw(ManualWallet mw)
        {
            logger.Info("start single  ManualWallets: ");
            using (AuthContext context = new AuthContext())
            {
                ManualWallet mm = new ManualWallet();
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
                    mm = context.AddManualWallet(mw);
                    return Request.CreateResponse(HttpStatusCode.OK, mm);
                }
                catch (Exception ex)
                {
                    logger.Error("Error in single  ManualWallets " + ex.Message);
                    logger.Info("End  single  ManualWallets: ");
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Got Error"); ;
                }
            }
        }
        [Route("")]
        [AcceptVerbs("Post")]
        public HttpResponseMessage post(Wallet wallet)
        {
            logger.Info("start single  GetcusomerWallets: ");
            using (AuthContext context = new AuthContext())
            {
                Wallet Item = new Wallet();
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
                    wallet.CompanyId = compid;
                    wallet.WarehouseId = Warehouse_id;


                    if (Warehouse_id > 0)
                    {
                        logger.Info("in Wallets");
                        if (wallet.CustomerId > 0)
                            Item = context.postWalletbyCustomeridWid(wallet, userid);
                        else
                            Item = null;
                        return Request.CreateResponse(HttpStatusCode.OK, Item);
                    }
                    else
                    {
                        //wallet.CompanyId = compid;
                        //wallet.Warehouseid = Warehouse_id;
                        logger.Info("in Wallets");
                        if (wallet.CustomerId > 0)
                            Item = context.postWalletbyCustomerid(wallet, userid);
                        else
                            Item = null;
                        return Request.CreateResponse(HttpStatusCode.OK, Item);
                    }
                }
                catch (Exception ex)
                {
                    logger.Error("Error in Get single GetcusomerWallets " + ex.Message);
                    logger.Info("End  single GetcusomerWallets: ");
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Got Error"); ;
                }
            }
        }
        [Route("cash")]
        [AcceptVerbs("Post")]
        public HttpResponseMessage postCashConversion(CashConversion point)
        {
            using (AuthContext context = new AuthContext())
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
                    if (point.Id > 0) { }
                    else
                        point.Id = 0;
                    var rpoint = context.CashConversionDb.Where(c => c.Id == point.Id).SingleOrDefault();
                    rpoint.PeopleId = userid;
                    if (rpoint != null)
                    {
                        rpoint.point = point.point;
                        rpoint.rupee = point.rupee;

                        context.CashConversionDb.Attach(rpoint);
                        context.Entry(rpoint).State = EntityState.Modified;
                        context.Commit();
                        bool Addrecord = CashConversionHistory(rpoint);//History
                    }
                    else
                    {
                        context.CashConversionDb.Add(point);
                        context.Commit();
                        rpoint = point;
                        rpoint.PeopleId = userid;
                        bool Addrecord = CashConversionHistory(rpoint);//History

                    }
                    return Request.CreateResponse(HttpStatusCode.OK, rpoint);
                }
                catch (Exception ex)
                {
                    logger.Error("Error" + ex.Message);
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Got Error");
                }
            }
        }

        [Route("")]
        // [HttpGet]
        public PaggingData_wt GetD(int list, int page, int CustomerId)
        {
            logger.Info("start OrderMaster: ");
            using (AuthContext context = new AuthContext())
            {
                try
                {
                    var identity = User.Identity as ClaimsIdentity;
                    int compid = 1, userid = 0;
                    int Warehouse_id = 1;
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
                    int CompanyId = compid;
                    int Warehouseid = Warehouse_id;
                    if (CompanyId == 0)
                    {
                        throw new ArgumentNullException("item");
                    }
                    logger.Info("User ID : {0} , Company Id : {1}", compid, userid);

                    if (Warehouse_id > 0)
                    {
                        var ass = context.AllWalletHistory(list, page, CustomerId, CompanyId, Warehouseid);
                        return ass;
                    }
                    else
                    {
                        var ass = context.AllWalletHistoryComp(list, page, CustomerId, CompanyId, Warehouseid);
                        return ass;
                    }
                }
                catch (Exception ex)
                {
                    logger.Error("Error in OrderMaster " + ex.Message);
                    logger.Info("End  OrderMaster: ");
                    return null;
                }
            }
        }

        [Route("GetManualWallet")]
        [HttpGet]

        public HttpResponseMessage GetManualWallet()
        {

            logger.Info("start Get Manual Wallet: ");
            using (AuthContext context = new AuthContext())
            {
                try
                {
                    var manualquery = "select mw.Name,c.CityName,mw.CreatedDate from ManualWallets mw inner join Cities c on mw.CityId =c.Cityid where mw.Active =1";

                    var manualwall = context.Database.SqlQuery<GetManualHistory>(manualquery).ToList();

                    return Request.CreateResponse(HttpStatusCode.OK, manualwall);
                }
                catch (Exception ex)
                {

                    logger.Error("Error in Getmanualwallet " + ex.Message);
                    logger.Info("End  Getmanualwallet: ");
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex.Message); ;
                }
            }


        }

        [Route("GetManualName")]
        [HttpGet]

        public HttpResponseMessage GetManualName()
        {

            logger.Info("start Get Manual Wallet: ");
            using (AuthContext context = new AuthContext())
            {
                try
                {
                    var manualquery = context.ManualWallets.Where(x => x.Active == true).Select(x => new { x.Id, x.Name }).ToList();

                    return Request.CreateResponse(HttpStatusCode.OK, manualquery);
                }
                catch (Exception ex)
                {

                    logger.Error("Error in Getmanualwallet " + ex.Message);
                    logger.Info("End  Getmanualwallet: ");
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex.Message); ;
                }
            }


        }

        [Route("Export")]
        [HttpGet]
        public HttpResponseMessage ExportGetData(int CustomerId)
        {
            logger.Info("start cutomer Wallet History List: ");
            using (AuthContext context = new AuthContext())
            {
                try
                {
                    var listOrders = (from i in context.CustomerWalletHistoryDb
                                      where i.CustomerId == CustomerId
                                      join k in context.Customers on i.CustomerId equals k.CustomerId
                                      select new CustomerWalletHistoryDTOM
                                      {
                                          Id = i.Id,
                                          CustomerId = i.CustomerId,
                                          Name = k.Name,
                                          Skcode = k.Skcode,
                                          City = k.City,
                                          ShopName = k.ShopName,
                                          Mobile = k.Mobile,
                                          Emailid = k.Emailid,
                                          BillingAddress = k.BillingAddress,
                                          NewAddedWAmount = i.NewAddedWAmount,
                                          NewOutWAmount = i.NewOutWAmount,
                                          CreatedDate = i.CreatedDate,
                                          CompanyId = i.CompanyId,
                                          WarehouseId = i.WarehouseId,
                                          TotalWalletAmount = i.TotalWalletAmount,
                                          rewardPoint = i.rewardPoint,
                                          EarningPoint = i.EarningPoint,
                                          UsedPoint = i.UsedPoint,
                                          TotalEarningPoint = i.TotalEarningPoint,
                                          TotalrewardPoint = i.TotalrewardPoint,
                                          TotalUsedPoint = i.TotalUsedPoint,
                                          MilestonePoint = i.MilestonePoint,
                                          TotalMilestonePoint = i.TotalMilestonePoint,
                                          Through = i.Through,
                                          PeopleName = i.PeopleName,
                                          OrderId = i.OrderId
                                      }).OrderByDescending(x => x.Id).ToList();

                    return Request.CreateResponse(HttpStatusCode.OK, listOrders);
                }
                catch (Exception ex)
                {
                    logger.Error("Error in WalletList " + ex.Message);
                    logger.Info("End  WalletList: ");
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex.Message);
                }
            }
        }

        #region History record CashConversionHistory
        public bool CashConversionHistory(CashConversion rpoint)
        {
            using (AuthContext context = new AuthContext())
            {
                try
                {
                    CashConversionHistory cch = new CashConversionHistory();
                    //op by user
                    try
                    {
                        People People = context.Peoples.Where(c => c.PeopleID == rpoint.PeopleId).SingleOrDefault();
                        cch.PeopleId = People.PeopleID;
                        cch.PeopleName = People.DisplayName;
                    }
                    catch (Exception ss) { }
                    if (rpoint.point != 0)
                    {
                        cch.point = rpoint.point;
                        cch.rupee = rpoint.rupee;
                    }
                    cch.CreatedDate = indianTime;
                    context.CashConversionHistoryDB.Add(cch);
                    context.Commit();
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

        [Route("BulkWalletPoint")]
        [HttpGet]
        public Wallet postWalletbyCustomerid(Wallet wallet, int PeopleId)
        {
            //var CustomersDss = (from i in context.Customers
            //                    where i.Deleted == false && i.Cityid == 7
            //                    join j in context.CustWarehouseDB on i.CustomerId equals j.CustomerId
            //                    select new
            //                    {
            //                        CustomerId = i.CustomerId,
            //                        Skcode = i.Skcode,
            //                        ShopName = i.ShopName,
            //                        WarehouseName = j.WarehouseName,
            //                        WarehouseId = j.WarehouseId,
            //                    }).ToList();

            //foreach (var custt in CustomersDss)
            //{
            //    try
            //    {
            //        // Wallet wallet = new Wallet();
            //        Wallet walletST = new Wallet();
            //        walletST.CreditAmount = 1000;
            //        walletST.Through = "1st Order Offer In Feb";
            //        walletST.CustomerId = custt.CustomerId;
            //        var walt = context.WalletDb.Where(c => c.CustomerId == walletST.CustomerId).SingleOrDefault();

            //        if (walt != null)
            //        {
            //            CustomerWalletHistory od = new CustomerWalletHistory();
            //            od.CustomerId = walletST.CustomerId;
            //            //op by user
            //            try
            //            {
            //                People People = context.Peoples.Where(c => c.PeopleID == PeopleId).SingleOrDefault();
            //                od.PeopleId = People.PeopleID;
            //                od.PeopleName = People.DisplayName;
            //            }
            //            catch (Exception ss)
            //            {

            //            }
            //            //op by Cust
            //            try
            //            {
            //                CustWarehouse cust = context.CustWarehouseDB.Where(c => c.CustomerId == walletST.CustomerId).SingleOrDefault();
            //                od.WarehouseId = cust.WarehouseId ?? 0;
            //                od.CompanyId = cust.CompanyId ?? 0;
            //            }
            //            catch (Exception cs)
            //            {
            //            }
            //            od.Through = walletST.Through;
            //            if (walletST.CreditAmount != 0)
            //            {
            //                od.NewAddedWAmount = walletST.CreditAmount;
            //                od.TotalWalletAmount = walt.TotalAmount + walletST.CreditAmount;
            //            }
            //            od.UpdatedDate = indianTime;
            //            od.TransactionDate = indianTime;
            //            od.CreatedDate = indianTime;
            //            context.CustomerWalletHistoryDb.Add(od);
            //            context.SaveChanges();

            //            walt.CustomerId = walletST.CustomerId;
            //            if (walletST.CreditAmount != 0)
            //            {
            //                walt.TotalAmount += walletST.CreditAmount;
            //                walt.UpdatedDate = indianTime;
            //                //Wallet Trigger :wallet amount added more than Rs.5000
            //                if (walletST.CreditAmount > 5000)
            //                {
            //                    var cust = context.Customers.Where(c => c.CustomerId == walletST.CustomerId).SingleOrDefault();
            //                  //  SendMailCreditWalletNotification(walletST.CreditAmount, od.PeopleName, cust.Name, cust.Skcode, indianTime);
            //                }
            //                //ForWalletNotification(walletST.CustomerId, walletST.CreditAmount);
            //            }
            //            if (walletST.DebitAmount > 0)
            //            {
            //                walt.TotalAmount -= walletST.DebitAmount;
            //                walt.TransactionDate = indianTime;
            //            }
            //            context.WalletDb.Attach(walt);
            //            context.Entry(walt).State = EntityState.Modified;
            //            context.SaveChanges();
            //            //return walt;
            //        }
            //        else
            //        {

            //            //code for wallet create on custmer sign up
            //            try
            //            {
            //                // var walletss = GetWalletbyCustomerid(wallet.CustomerId);
            //                var reward = context.GetRewardbyCustomerid(walletST.CustomerId);
            //            }
            //            catch (Exception ex)
            //            {

            //            }



            //            if (walletST.CreditAmount > 0)
            //            {
            //                walletST.TotalAmount = walletST.CreditAmount;
            //                walletST.UpdatedDate = indianTime;
            //                //ForWalletNotification(walletST.CustomerId, walletST.CreditAmount);
            //            }
            //            walletST.CreatedDate = indianTime;
            //            walletST.UpdatedDate = indianTime;
            //            walletST.Deleted = false;
            //            context.WalletDb.Add(walletST);
            //            context.SaveChanges();
            //            //return wallet;

            //        }
            //    }
            //    catch (Exception ex)
            //    {
            //        logger.Error(ex.Message);
            //        return null;
            //    }
            //}
            return wallet;
        }

        [Route("AddWalletPointToCustomer")]
        [HttpPost]
        public HttpResponseMessage AddWalletPointToCustomer(CustomerAddScoreWalletDc wallet)
        {
            string Msg = "";
            using (AuthContext context = new AuthContext())
            {

                var Walletcustomer = context.WalletDb.FirstOrDefault(x => x.CustomerId == wallet.CustomerId);
                CustomerWalletHistory od = new CustomerWalletHistory();
                MongoDbHelper<CustomerPlayedGame> mongoDbHelper = new MongoDbHelper<CustomerPlayedGame>();
                DateTime startDate = DateTime.Now.Date;
                DateTime endDate = DateTime.Now.Date.AddDays(1).AddMinutes(-1);
                CustomerPlayedGame CustomerPlayedGame = new CustomerPlayedGame
                {
                    CustomerId = wallet.CustomerId,
                    CreatedDate = DateTime.Now,
                    CreatedBy = wallet.CustomerId,
                    GameName = wallet.GameName,
                    GamePayType = wallet.GamePointOn,
                    IsActive = true,
                    IsDeleted = false,
                    Point = 0,
                    IsExpired = false
                };
                if (!mongoDbHelper.Select(x => x.CustomerId == wallet.CustomerId && x.GameName == wallet.GameName && x.GamePayType == wallet.GamePointOn && x.CreatedDate >= startDate && x.CreatedDate <= endDate).Any())
                {
                    if (Walletcustomer != null)
                    {
                        var totalScore = 0;
                        if (wallet.GamePointOn == "Play")
                            totalScore = 50;
                        else
                        {
                            totalScore = Convert.ToInt32(wallet.Score / 10);
                            if (totalScore > 100)
                                totalScore = 100;
                        }

                        var previousWaletPoint = Walletcustomer.TotalAmount;
                        Walletcustomer.TotalAmount += totalScore;
                        Walletcustomer.UpdatedDate = indianTime;

                        od.CustomerId = Walletcustomer.CustomerId;
                        od.WarehouseId = Walletcustomer.WarehouseId;
                        od.CompanyId = Walletcustomer.CompanyId;
                        od.Through = "gameplay";
                        od.comment = totalScore + " point added into " + previousWaletPoint + " .Total Points : " + Walletcustomer.TotalAmount;
                        od.TotalWalletAmount = Walletcustomer.TotalAmount;
                        od.NewAddedWAmount = totalScore;
                        od.UpdatedDate = indianTime;
                        od.TransactionDate = indianTime;
                        od.CreatedDate = indianTime;
                        context.CustomerWalletHistoryDb.Add(od);
                        context.Entry(Walletcustomer).State = EntityState.Modified;
                        if (context.Commit() > 0)
                        {
                            CustomerPlayedGame.Point = totalScore;
                            Msg = totalScore + " points credited successfully to your wallet ";

                        }
                        else { Msg = "Some thing went wrong"; }

                    }
                }
                else
                {
                    Msg = "Better luck next time!";
                }
                mongoDbHelper.Insert(CustomerPlayedGame);
                return Request.CreateResponse(HttpStatusCode.OK, Msg);

            }
        }

        [Route("GetRetailAppGame")]
        [HttpGet]
        public List<GameConfiguration> GetRetailAppGame(int customerId, int warehouseId)
        {
            MongoDbHelper<GameConfiguration> mongoDbHelper = new MongoDbHelper<GameConfiguration>();
            List<GameConfiguration> GameConfigurations = mongoDbHelper.Select(x => x.IsActive && !x.IsDeleted.Value).ToList();
            return GameConfigurations;
        }

        [Route("GetRetailAppGameBanner")]
        [HttpGet]
        public List<GameBanner> GetRetailAppGameBanner(int customerId, int warehouseId)
        {
            MongoDbHelper<GameBanner> mongoDbHelper = new MongoDbHelper<GameBanner>();
            List<GameBanner> GameBanners = mongoDbHelper.Select(x => x.IsActive && !x.IsDeleted.Value).ToList();
            return GameBanners;
        }


        [Route("InsertGameConfiguration")]
        [HttpGet]
        [AllowAnonymous]
        public bool InsertGameConfiguration()
        {
            bool result = true;
            MongoDbHelper<GameConfiguration> mongoDbHelper = new MongoDbHelper<GameConfiguration>();
            List<GameConfiguration> GameConfigurations = new List<GameConfiguration>();

            var DefaultNotificationMessage2 = new GameConfiguration
            {
                CreatedDate = DateTime.Now,
                CreatedBy = 1,
                GameName = "Helix",
                IsActive = true,
                IsDeleted = false,
                GameUrl = "https://er15.xyz:4436/images/Game/helix",
                GameLogo = "https://er15.xyz:4436/Game/GameLogo/helix.ico",
                WalletPointOnGameOver = true,
                WalletPointOnPlay = true
            };
            GameConfigurations.Add(DefaultNotificationMessage2);
            var DefaultNotificationMessage1 = new GameConfiguration
            {
                CreatedDate = DateTime.Now,
                CreatedBy = 1,
                GameName = "2048",
                IsActive = true,
                IsDeleted = false,
                GameUrl = "https://er15.xyz:4436/images/Game/twozero",
                GameLogo = "https://er15.xyz:4436/Game/GameLogo/2048.ico",
                WalletPointOnGameOver = true,
                WalletPointOnPlay = true
            };
            GameConfigurations.Add(DefaultNotificationMessage1);
            //var DefaultNotificationMessage3 = new GameConfiguration
            //{
            //    CreatedDate = DateTime.Now,
            //    CreatedBy = 1,
            //    GameName = "Fruit Ninja",
            //    IsActive = true,
            //    IsDeleted = false,
            //    GameUrl = "https://uat.shopkirana.in/images/Fruit%20Ninja",
            //    GameLogo = "https://uat.shopkirana.in/images/Fruit%20Ninja/images/logo.png",
            //    WalletPointOnGameOver = true,
            //    WalletPointOnPlay = true
            //};
            //GameConfigurations.Add(DefaultNotificationMessage3);
            //var DefaultNotificationMessage4 = new GameConfiguration
            //{
            //    CreatedDate = DateTime.Now,
            //    CreatedBy = 1,
            //    GameName = "Pacman",
            //    IsActive = true,
            //    IsDeleted = false,
            //    GameUrl = "https://uat.shopkirana.in/images/pacman",
            //    GameLogo = "https://uat.shopkirana.in/images/pacman/img/Icon-512x512.png",
            //    WalletPointOnGameOver = true,
            //    WalletPointOnPlay = true
            //};
            //GameConfigurations.Add(DefaultNotificationMessage4);
            //var DefaultNotificationMessage5 = new GameConfiguration
            //{
            //    CreatedDate = DateTime.Now,
            //    CreatedBy = 1,
            //    GameName = "HexGL",
            //    IsActive = true,
            //    IsDeleted = false,
            //    GameUrl = "https://uat.shopkirana.in/images/HexGL",
            //    GameLogo = "https://uat.shopkirana.in/images/HexGL/icon_256.png",
            //    WalletPointOnGameOver = true,
            //    WalletPointOnPlay = true
            //};
            //GameConfigurations.Add(DefaultNotificationMessage5);
            //var DefaultNotificationMessage6 = new GameConfiguration
            //{
            //    CreatedDate = DateTime.Now,
            //    CreatedBy = 1,
            //    GameName = "Solitare",
            //    IsActive = true,
            //    IsDeleted = false,
            //    GameUrl = "",
            //    GameLogo = "https://uat.shopkirana.in/images/HexGL/icon_256.png",
            //    WalletPointOnGameOver = true,
            //    WalletPointOnPlay = true
            //};
            //GameConfigurations.Add(DefaultNotificationMessage5);
            mongoDbHelper.InsertMany(GameConfigurations);
            return result;
        }
        [Route("InsertGameBanner")]
        [HttpGet]
        [AllowAnonymous]
        public bool InsertGameBanner()
        {
            bool result = true;
            MongoDbHelper<GameBanner> mongoDbHelper = new MongoDbHelper<GameBanner>();
            List<GameBanner> GameConfigurations = new List<GameBanner>();

            var DefaultNotificationMessage2 = new GameBanner
            {
                CreatedDate = DateTime.Now,
                CreatedBy = 1,
                BannerImageUrl = "https://er15.xyz:4436/images/KisandanImages/happy-kisan.png",
                BannerType = "Brand",
                IsDeleted = false,
                ObjectId = "1",
                IsActive = true
            };
            GameConfigurations.Add(DefaultNotificationMessage2);
            var DefaultNotificationMessage1 = new GameBanner
            {
                CreatedDate = DateTime.Now,
                CreatedBy = 1,
                BannerImageUrl = "https://er15.xyz:4436/images/KisandanImages/kisan-01.png",
                BannerType = "Brand",
                IsDeleted = false,
                ObjectId = "1",
                IsActive = true
            };
            GameConfigurations.Add(DefaultNotificationMessage1);
            //var DefaultNotificationMessage3 = new GameBanner
            //{
            //    CreatedDate = DateTime.Now,
            //    CreatedBy = 1,
            //    BannerImageUrl = "https://uat.shopkirana.in/images/ComboImage/aa20012020120457.jpg",
            //    BannerType = "SubCategory",
            //    IsDeleted = false,
            //    ObjectId = "1",
            //    IsActive = true
            //};
            //GameConfigurations.Add(DefaultNotificationMessage3);
            //var DefaultNotificationMessage4 = new GameBanner
            //{
            //    CreatedDate = DateTime.Now,
            //    CreatedBy = 1,
            //    BannerImageUrl = "https://uat.shopkirana.in/images/ComboImage/aa20012020120104.jpg",
            //    BannerType = "Item",
            //    IsDeleted = false,
            //    ObjectId = "23",
            //    IsActive = true
            //};
            //GameConfigurations.Add(DefaultNotificationMessage4);
            //var DefaultNotificationMessage5 = new GameBanner
            //{
            //    CreatedDate = DateTime.Now,
            //    CreatedBy = 1,
            //    BannerImageUrl = "https://uat.shopkirana.in/images/ComboImage/aa20012020120104.jpg",
            //    BannerType = "Offer",
            //    IsDeleted = false,
            //    ObjectId = "0",
            //    IsActive = true
            //};
            //GameConfigurations.Add(DefaultNotificationMessage5);
            mongoDbHelper.InsertMany(GameConfigurations);
            return result;
        }

        [Route("MemberShipBanifit")]
        [HttpGet]
        [AllowAnonymous]
        public bool MemberShipBanifit()
        {
            bool result = true;
            MongoDbHelper<MemberShipBanifit> mongoDbHelper = new MongoDbHelper<MemberShipBanifit>();
            List<MemberShipBanifit> memberShipBanifit = new List<MemberShipBanifit>();

            var DefaultNotificationMessage2 = new MemberShipBanifit
            {
                Logo = "https://er15.xyz:4436/images/Game/GameLogo/After Membeship.jpeg",
                PrimeHtmL = "<p><span style='color: rgb(235, 107, 86);'><strong><span style='font-size: 19px;'>Congratulation for join Fayda membership</span></strong></span> <br/>  <br/> Welcome to Shopkirana (“Fayda Membership”).Thank you for subscribing our Fayda membership, Enjoy lots of benefits for Fayda membership.</ p >  < p style = 'text-align: justify;' >< span style = 'color: rgb(235, 107, 86);' >< strong >< span style = 'font-size: 19px;' > One membership, many benefits </ span ></ strong ></ span ></ p >  < p style = 'text-align: justify;' >< span style = 'color: rgb(235, 107, 86);' >< strong >< span style = 'font-size: 19px;' >< img src = 'https://er15.xyz:4436/images/Game/GameLogo/2nd banner.jpeg' ></ span ></ strong ></ span >< span style = 'color: rgb(0, 0, 0);' >< strong >< span style = 'font-size: 16px;' ></ span ></ strong ></ span ></ p >  < p style = 'text-align: left;' >< strong > Shop with unlimited FREE Delivery at your door step on over 10, 000 eligible items from India&#39;s biggest brands.</strong></p>  <p style='text-align: left;'><strong><img src='https://er15.xyz:4436/images/Game/GameLogo/1st banner.jpeg'></strong></p><span style='color: rgb(235, 107, 86);'><strong><span style='font-size: 19px;'>              <p style='text-align: left;'><br></p>          </span></strong><strong><span style='font-size: 19px;'>              <p><br></p>          </span></strong></span><br>",
                PrimeHindiHtmL = "<p><span style='color: rgb(235, 107, 86);'><strong><span style='font-size: 19px;'>फ़ायदा मेम्बरशिप्स से जुड़ने के लिए बधाई हो</span></strong></span> <br/>  <br/> शॉपकीना में आपका स्वागत है(“फ़ायदा मेम्बरशिप्स”) फ़ायदा मेम्बरशिप्स लेने के लिए धन्यवाद, फ़ायद मेम्बरशिप से होने वाले बहुत सारे  लाभों का आनंद लें</ p >  < p style = 'text-align: justify;' >< span style = 'color: rgb(235, 107, 86);' >< strong >< span style = 'font-size: 19px;' > एक  सदस्तया, कई लाभ </ span ></ strong ></ span ></ p >  < p style = 'text-align: justify;' >< span style = 'color: rgb(235, 107, 86);' >< strong >< span style = 'font-size: 19px;' >< img src = 'https://er15.xyz:4436/images/Game/GameLogo/2nd banner.jpeg' ></ span ></ strong ></ span >< span style = 'color: rgb(0, 0, 0);' >< strong >< span style = 'font-size: 16px;' ></ span ></ strong ></ span ></ p >  < p style = 'text-align: left;' >< strong > खरीदारी करे शॉपकिरणा ऐप्प से 10000 से  भी  अधिक योग्य आइटम पर  पाए असीमित फ्री डिलीवरी.</strong></p>  <p style='text-align: left;'><strong><img src='https://er15.xyz:4436/images/Game/GameLogo/1st banner.jpeg'></strong></p><span style='color: rgb(235, 107, 86);'><strong><span style='font-size: 19px;'>              <p style='text-align: left;'><br></p>          </span></strong><strong><span style='font-size: 19px;'>              <p><br></p>          </span></strong></span><br>",
            };


            mongoDbHelper.Insert(DefaultNotificationMessage2);
            return result;
        }

        [HttpGet]
        [Route("PostDirectWalletPoint")]
        public int AddDirectWalletPoint(string mobilenumber, int walletpoint, int orderid)
        {
            using (AuthContext Context = new AuthContext())
            {
                var param = new SqlParameter
                {
                    ParameterName = "Mobile",
                    Value = mobilenumber
                };
                var param1 = new SqlParameter
                {
                    ParameterName = "WalletPoint",
                    Value = walletpoint
                };

                var param2 = new SqlParameter
                {
                    ParameterName = "OrderId",
                    Value = orderid
                };

                int result = Context.Database.ExecuteSqlCommand("exec [InsertDirectWalletPoint] @Mobile,@WalletPoint,@OrderId", param, param1, param2);
                return result;

            }
        }

        [Route("walletListWithPaging")]
        [AllowAnonymous]
        [HttpGet]
        public ResWallet walletListWithPaging(DateTime? start, DateTime? end, int? WarehouseId, int skip, int take)
        {
            ResWallet res = new ResWallet();
            int Skiplist = (skip - 1) * take;
           
            using (AuthContext context = new AuthContext())
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
                    int count = 0;
                    List<walletDc> List = new List<walletDc>();
                    string whereclouse = " where i.Deleted = 0 ";

                    if (WarehouseId > 0)
                    {
                        whereclouse = whereclouse + " and j.Warehouseid = " + WarehouseId ;
                    }
                    if (start != null && end != null)
                    {
                        //whereclouse = whereclouse + " and i.CreatedDate >=" + start +" and i.CreatedDate <=" + end ;
                    whereclouse += " and (i.CreatedDate >= " + "'" + start.Value.ToString("MM-dd-yyyy HH:mm:ss") + "'" + " And  i.CreatedDate <=" + "'" + end.Value.ToString("MM-dd-yyyy HH:mm:ss") + "')";
                }
                    string sqlquery = "";
                    if(skip==0 && take==0) sqlquery = " select i.Id,i.CustomerId,ISNULL(i.TotalAmount,0) TotalAmount,i.CreatedDate, ISNULL(i.TransactionDate,'') TransactionDate,i.UpdatedDate,j.Skcode,j.ShopName,k.WarehouseName,j.Warehouseid,k.CityName as City from Wallets i join Customers j on i.CustomerId = j.CustomerId join Warehouses k on j.Warehouseid = k.WarehouseId " + whereclouse + " Order by i.Id desc"; 
                    else sqlquery = " select i.Id,i.CustomerId,ISNULL(i.TotalAmount,0) TotalAmount,i.CreatedDate, ISNULL(i.TransactionDate,'') TransactionDate,i.UpdatedDate,j.Skcode,j.ShopName,k.WarehouseName,j.Warehouseid,k.CityName as City from Wallets i join Customers j on i.CustomerId = j.CustomerId join Warehouses k on j.Warehouseid = k.WarehouseId " + whereclouse + " Order by i.Id desc offset " + Skiplist + " rows fetch next " + take + " rows only"; 
                    string sqlcount = " select count(*) from Wallets i join Customers j on i.CustomerId = j.CustomerId join Warehouses k on j.Warehouseid = k.WarehouseId " + whereclouse;

                    int totalcount = context.Database.SqlQuery<int>(sqlcount).FirstOrDefault();
                    List<walletDc> pointList = context.Database.SqlQuery<walletDc>(sqlquery).ToList();
                    List = Mapper.Map(pointList).ToANew<List<walletDc>>();
                    res.walletdata = List;
                    res.TotalCount = totalcount;

                    return res;
            }
        }

        [Route("WalletListByKeyward")]
        [HttpGet]
        public ResWallet WalletListByKeyward(string key)
        {
            ResWallet res = new ResWallet();
            logger.Info("start WalletList: ");
            using (AuthContext context = new AuthContext())
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
                var pointList = (from i in context.WalletDb
                                 where i.Deleted == false
                                 join j in context.Customers on i.CustomerId equals j.CustomerId
                                 join k in context.Warehouses on j.Warehouseid equals k.WarehouseId into ts
                                 from k in ts.DefaultIfEmpty()
                                 where (k.CityName.Contains(key) || j.Skcode == key || j.ShopName.Contains(key))
                                 select new
                                 {
                                     Id = i.Id,
                                     CustomerId = i.CustomerId,
                                     TotalAmount = i.TotalAmount,
                                     CreatedDate = i.CreatedDate,
                                     TransactionDate = i.TransactionDate,
                                     UpdatedDate = i.UpdatedDate,
                                     Skcode = j.Skcode,
                                     ShopName = j.ShopName,
                                     WarehouseName = k.WarehouseName,
                                     WarehouseId = j.Warehouseid,
                                     City = k.CityName
                                 }).ToList();

                logger.Info("End  wallet: ");
                res.walletdata = pointList;
                res.TotalCount = 0;
                return res;
            }
        }
    }
    public class ResWallet
    {
        public dynamic walletdata { get; set; }
        public int TotalCount { get; set; }
    }
    public class MemberShipBanifit
    {
        [BsonId]
        public ObjectId Id { get; set; }
        public string PrimeHtmL { get; set; }
        public string PrimeHindiHtmL { get; set; }
        public string Logo { get; set; }
    }

    public class CustomerAddScoreWalletDc
    {
        public string GameName { get; set; }
        public string GamePointOn { get; set; }
        public int CustomerId { get; set; }
        public int Score { get; set; }
    }

    public class WalletReward
    {
        public Wallet wallet { get; set; }
        public RewardPoint reward { get; set; }
        public CashConversion conversion { get; set; }

    }

    public class CustomerWalletHistoryDTOM
    {
        public int CustomerId { get; set; }
        public int? OrderId { get; set; }
        public int Id { get; set; }
        public int CompanyId { get; set; }
        public int Warehouseid { get; set; }
        public double? TotalWalletAmount { get; set; }
        public double? NewAddedWAmount { get; set; }
        public double? NewOutWAmount { get; set; }
        public double? rewardPoint { get; set; }
        public double? EarningPoint { get; set; }
        public double? UsedPoint { get; set; }
        public double? MilestonePoint { get; set; }
        public double? TotalrewardPoint { get; set; }
        public double? TotalEarningPoint { get; set; }
        public double? TotalUsedPoint { get; set; }
        public double? TotalMilestonePoint { get; set; }
        public string ShopName { get; set; }
        public string Through { get; set; }

        public DateTime? TransactionDate { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public string Skcode { get; set; }
        public string CompanyName { get; set; }
        public string Day { get; set; }
        public int? BeatNumber { get; set; }
        public int? ExecutiveId { get; set; }
        public string ExecutiveName { get; set; }
        public string Mobile { get; set; }
        public string Name { get; set; }
        public int SubsubCategoryid { get; set; } //brand Id
        public string SubsubcategoryName { get; set; } //brand name
        public bool IsMine { get; set; }
        public string SUPPLIERCODES { get; set; }
        public string BillingAddress { get; set; }
        public string ShippingAddress { get; set; }
        public string Emailid { get; set; }
        public bool IsAssigned { get; set; }
        public int? WarehouseId { get; set; }
        public string WarehouseName { get; set; }

        public string City { get; set; }

        public string PeopleName { get; set; }
    }


    public class GetManualHistory
    {
        public string Name { get; set; }
        public string CityName { get; set; }
        public DateTime createddate { get; set; }




    }

    public class GameConfiguration
    {
        [BsonId]
        public ObjectId Id { get; set; }
        public string GameName { get; set; }
        public string GameUrl { get; set; }
        public string GameLogo { get; set; }
        public bool WalletPointOnPlay { get; set; }
        public bool WalletPointOnGameOver { get; set; }
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime CreatedDate { get; set; }
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime? ModifiedDate { get; set; }
        public bool IsActive { get; set; }
        public bool? IsDeleted { get; set; }
        public int CreatedBy { get; set; }
        public int? ModifiedBy { get; set; }
    }

    public class GameBanner
    {
        [BsonId]
        public ObjectId Id { get; set; }
        public string BannerImageUrl { get; set; }
        public string BannerType { get; set; }
        public string ObjectId { get; set; }
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime CreatedDate { get; set; }
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime? ModifiedDate { get; set; }
        public bool IsActive { get; set; }
        public bool? IsDeleted { get; set; }
        public int CreatedBy { get; set; }
        public int? ModifiedBy { get; set; }
    }


    public class CustomerPlayedGame
    {
        [BsonId]
        public ObjectId Id { get; set; }
        public string GameName { get; set; }
        public string GamePayType { get; set; }
        public int CustomerId { get; set; }
        public int Point { get; set; }
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime CreatedDate { get; set; }
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime? ModifiedDate { get; set; }
        public bool IsActive { get; set; }
        public bool? IsDeleted { get; set; }
        public int CreatedBy { get; set; }
        public int? ModifiedBy { get; set; }
        public bool IsExpired { get; set; }
    }

    public class WarehouseVideo
    {
        [BsonId]
        public ObjectId Id { get; set; }
        public int WarehouseId { get; set; }
        public string Videolink { get; set; }
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime StartDate { get; set; }
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime? EndDate { get; set; }
        public bool IsActive { get; set; }
        public bool? IsDeleted { get; set; }
        public int CreatedBy { get; set; }
        public int? ModifiedBy { get; set; }
        public bool IsExpired { get; set; }

        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime CreatedDate { get; set; }
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime? ModifiedDate { get; set; }
    }

    public class walletDc
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public double TotalAmount { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime TransactionDate { get; set; }
        public DateTime UpdatedDate { get; set; }
        public string Skcode { get; set; }
        public string ShopName { get; set; }
        public string WarehouseName { get; set; }
        public int Warehouseid { get; set; }
        public string City { get; set; }
    }
}
