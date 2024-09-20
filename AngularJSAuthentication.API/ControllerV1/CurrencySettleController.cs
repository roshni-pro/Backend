using AngularJSAuthentication.Model;
using GenricEcommers.Models;
using NLog;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Web.Http;

namespace AngularJSAuthentication.API.ControllerV1
{
    [RoutePrefix("api/CurrencySettle")]
    public class CurrencySettleController : ApiController
    {
        public static Logger logger = LogManager.GetCurrentClassLogger();
        private static TimeZoneInfo INDIAN_ZONE = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
        DateTime indianTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE);

        [Route("")]
        [HttpGet]
        public HttpResponseMessage getbyId(int PeopleID)
        {
            using (var context = new AuthContext())
            {
                try
                {
                    var DBoyCurrency = context.getdboysCurrency(PeopleID);
                    return Request.CreateResponse(HttpStatusCode.OK, DBoyCurrency);
                }
                catch (Exception ex)
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, ex.Message);
                }
            }
        }
        [Route("checkdata")]
        public List<OrderDispatchedMaster> Getcheckdata(int PeopleID)
        {
            logger.Info("start get all Sales Executive: ");
            using (var context = new AuthContext())
            {
                try
                {
                    var identity = User.Identity as ClaimsIdentity;
                    int compid = 0, userid = 0;

                    List<OrderDispatchedMaster> displist1 = new List<OrderDispatchedMaster>();
                    List<OrderDispatchedMaster> displist2 = new List<OrderDispatchedMaster>();
                    List<OrderDispatchedMaster> displist3 = new List<OrderDispatchedMaster>();



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
                    var db = context.Peoples.Where(p => p.PeopleID == PeopleID).FirstOrDefault();
                    if (db != null)
                    {
                        displist1 = context.OrderDispatchedMasters.Where(p => p.DboyMobileNo == db.Mobile && p.CheckNo != null && p.CheckAmount > 0 && p.DboycheckRecived == false).ToList();
                    }
                    foreach (var aa in displist1)
                    {

                        var matches = from ad in context.DeliveryIssuanceDb
                                      where ad.OrderIds.Contains(aa.OrderId.ToString())
                                      select ad;
                        var aas = context.DeliveryIssuanceDb.Where(p => p.OrderIds.Contains(aa.OrderId.ToString())).FirstOrDefault();
                        OrderDispatchedMaster c = new OrderDispatchedMaster();
                        if (aas != null)
                        {
                            c.DeliveryIssuanceId = aas.DeliveryIssuanceId;
                            c.CheckNo = aa.CheckNo;
                            c.CheckAmount = aa.CheckAmount;
                            c.CashAmount = aa.CashAmount;
                            c.OrderId = aa.OrderId;
                            c.DboyName = aa.DboyName;
                            c.CustomerName = aa.CustomerName;
                            c.UpdatedDate = aa.UpdatedDate;
                        }
                        displist2.Add(c);
                    }

                    return displist2;
                }
                catch (Exception ex)
                {

                    return null;
                }
            }
        }
        [Route("dueamountget")]
        public List<DBoyCurrency> Get(string status, int PeopleID)
        {
            logger.Info("start get all Sales Executive: ");
            using (var context = new AuthContext())
            {
                try
                {
                    var identity = User.Identity as ClaimsIdentity;
                    int compid = 0, userid = 0;
                    List<DBoyCurrency> displist = new List<DBoyCurrency>();
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
                    displist = context.DBoyCurrencyDB.Where(p => p.PeopleId == PeopleID && p.Dueamountstatus == "Partial Settle").ToList();

                    logger.Info("End  Sales Executive: ");
                    return displist;
                }
                catch (Exception ex)
                {
                    logger.Error("Error in getall Sales Executive " + ex.Message);
                    logger.Info("End getall Sales Executive: ");
                    return null;
                }
            }
        }
        [Route("")]
        [HttpGet]
        public HttpResponseMessage getAppOrders(string M, string mob) //get orders for delivery
        {
            using (var context = new AuthContext())
            {
                try
                {
                    if (M == "all")
                    {
                        var DBoyorders = context.getallOrderofboy(mob);

                        return Request.CreateResponse(HttpStatusCode.OK, DBoyorders);
                    }
                    else
                    {
                        var DBoyorders = context.getAcceptedOrders(mob);
                        return Request.CreateResponse(HttpStatusCode.OK, DBoyorders);
                    }


                }
                catch (Exception ex)
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, ex.Message);
                }
            }
        }
        [Route("")]
        [HttpPost]
        public HttpResponseMessage DBCurrency(DBoyCurrency obj) //Order change delivery boy
        {
            using (var context = new AuthContext())
            {
                try
                {
                    var DBoyorders = context.DboyCu(obj, obj.PeopleId);
                    if (DBoyorders == null)
                    {
                        return Request.CreateResponse(HttpStatusCode.BadRequest, "Error Occured");
                    }
                    var res = new
                    {
                        DBoyorders = DBoyorders,
                        Status = true,
                        Message = "Success."
                    };
                    return Request.CreateResponse(HttpStatusCode.OK, res);
                }
                catch (Exception ex)
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, ex.Message);
                }
            }
        }
        [Route("Stock")]
        [HttpPost]
        public dynamic DBCurrencyStock(CurrencyStock objlist) //Order change delivery boy
        {
            using (var context = new AuthContext())
            {
                try
                {
                    if (objlist != null)
                    {
                        var existDatas = context.CurrencyStockDB.Where(x => x.Deleted == false).FirstOrDefault();

                        if (existDatas == null)
                        {
                            objlist.UpdatedDate = indianTime;
                            objlist.CreatedDate = indianTime;
                            objlist.status = "Delivered Boy Currency Inserted InCST";
                            context.CurrencyStockDB.Add(objlist);
                            int id = context.Commit();
                        }
                    }


                }
                catch (Exception ex)
                {

                    return null;
                }
            }
            return objlist;

        }
        [Route("Stockhistory")]
        [HttpPost]
        public dynamic DBCurrencyStockhistory(CurrencyHistory objlist, int PeopleID) //Order change delivery boy
        {
            using (var context = new AuthContext())
            {
                try
                {
                    var deliveryBoy = context.Peoples.Where(x => x.PeopleID == PeopleID && x.Deleted == false).FirstOrDefault();

                    if (objlist != null)
                    {
                        var existDatas = context.CurrencyHistoryDB.Where(x => x.Deleted == false).FirstOrDefault();

                        if (existDatas == null)
                        {
                            objlist.UpdatedDate = indianTime;
                            objlist.CreatedDate = indianTime;
                            objlist.DboyName = deliveryBoy.DisplayName;
                            objlist.status = "Delivered Boy Currency Inserted InCST";
                            context.CurrencyHistoryDB.Add(objlist);
                            int id = context.Commit();
                        }
                        else
                        {
                            var existData = context.CurrencyHistoryDB.Where(x => x.CurrencyHistoryid == existDatas.CurrencyHistoryid && x.Deleted == false).FirstOrDefault();


                            existData.OneRupee += objlist.OneRupee;
                            existData.onerscount += objlist.onerscount;
                            existData.TwoRupee += objlist.TwoRupee;
                            existData.tworscount += objlist.tworscount;
                            existData.FiveRupee += objlist.FiveRupee;
                            existData.fiverscount += objlist.fiverscount;
                            existData.TenRupee += objlist.TenRupee;
                            existData.tenrscount += objlist.tenrscount;
                            existData.FiveNote += objlist.FiveNote;
                            existData.FiveNoteCount += objlist.FiveNoteCount;
                            existData.TenNote += objlist.TenNote;
                            existData.TenNoteCount += objlist.TenNoteCount;
                            existData.TwentyRupee += objlist.TwentyRupee;
                            existData.Twentyrscount += objlist.Twentyrscount;
                            existData.fiftyRupee += objlist.fiftyRupee;
                            existData.fiftyrscount += objlist.fiftyrscount;
                            existData.HunRupee += objlist.HunRupee;
                            existData.hunrscount += objlist.hunrscount;
                            existData.twoHunRupee += objlist.twoHunRupee;
                            existData.twohunrscount += objlist.twohunrscount;
                            existData.fiveHRupee += objlist.fiveHRupee;
                            existData.fivehrscount += objlist.fivehrscount;
                            existData.twoTHRupee += objlist.twoTHRupee;
                            existData.twoTHrscount += objlist.twoTHrscount;
                            existData.TotalAmount += objlist.TotalAmount;
                            existData.UpdatedDate = indianTime;
                            context.CurrencyHistoryDB.Attach(existData);
                            context.Entry(existData).State = EntityState.Modified;
                            context.Commit();
                        }
                        if (deliveryBoy != null)
                        {

                            foreach (var o in objlist.AssignAmountId)
                            {
                                DBoyCurrency db = context.DBoyCurrencyDB.Where(x => x.DBoyCId == o.DBoyCId).FirstOrDefault();
                                db.Status = "Delivered Boy Currency Settled";
                                db.UpdatedDate = indianTime;
                                if (objlist.Dueamount != 0)
                                {
                                    db.Dueamount = objlist.Dueamount;
                                    db.Dueamountstatus = "Partial Settle";

                                }

                                context.DBoyCurrencyDB.Attach(db);
                                context.Entry(db).State = EntityState.Modified;
                                context.Commit();
                            }
                        }
                        try
                        {
                            CurrencyStock curreStock = new CurrencyStock();
                            curreStock.OneRupee = objlist.OneRupee;
                            curreStock.onerscount = objlist.onerscount;
                            curreStock.TwoRupee = objlist.TwoRupee;
                            curreStock.tworscount = objlist.tworscount;
                            curreStock.FiveRupee = objlist.FiveRupee;
                            curreStock.fiverscount = objlist.fiverscount;
                            curreStock.TenRupee = objlist.TenRupee;
                            curreStock.tenrscount = objlist.tenrscount;
                            curreStock.FiveNote = objlist.FiveNote;
                            curreStock.FiveNoteCount = objlist.FiveNoteCount;
                            curreStock.TenNote = objlist.TenNote;
                            curreStock.TenNoteCount = objlist.TenNoteCount;
                            curreStock.TwentyRupee = objlist.TwentyRupee;
                            curreStock.Twentyrscount = objlist.Twentyrscount;
                            curreStock.fiftyRupee = objlist.fiftyRupee;
                            curreStock.fiftyrscount = objlist.fiftyrscount;
                            curreStock.HunRupee = objlist.HunRupee;
                            curreStock.hunrscount = objlist.hunrscount;
                            curreStock.twoHunRupee = objlist.twoHunRupee;
                            curreStock.twohunrscount = objlist.twohunrscount;
                            curreStock.fiveHRupee = objlist.fiveHRupee;
                            curreStock.fivehrscount = objlist.fivehrscount;
                            curreStock.twoTHRupee = objlist.twoTHRupee;
                            curreStock.twoTHrscount = objlist.twoTHrscount;
                            curreStock.TotalAmount = objlist.TotalAmount;
                            curreStock.DeliveryIssuanceId = objlist.DeliveryIssuanceId;
                            curreStock.UpdatedDate = indianTime;
                            curreStock.CreatedDate = indianTime;
                            curreStock.DboyName = deliveryBoy.DisplayName;
                            curreStock.DBoyCId = deliveryBoy.PeopleID;
                            curreStock.status = "Delivered Boy Currency Inserted InCST";
                            context.CurrencyStockDB.Add(curreStock);
                            int id = context.Commit();
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.Message);
                        }
                    }
                }
                catch (Exception ex)
                {

                    return null;
                }
            }
            return true;

        }
        [Route("Stockhistorydue")]
        [HttpPost]
        public dynamic DBCurrencyStockhistorydue(CurrencyStock objlist, int PeopleID) //Order change delivery boy
        {
            using (var context = new AuthContext())
            {
                try
                {
                    var deliveryBoy = context.Peoples.Where(x => x.PeopleID == PeopleID && x.Deleted == false).FirstOrDefault();
                    if (objlist != null)
                    {
                        var existDatas = context.CurrencyStockDB.Where(x => x.Deleted == false && x.DeliveryIssuanceId == objlist.DeliveryIssuanceId).FirstOrDefault();

                        if (existDatas == null)
                        {
                            objlist.UpdatedDate = indianTime;
                            objlist.CreatedDate = indianTime;
                            objlist.DboyName = deliveryBoy.DisplayName;
                            objlist.status = "Delivered Boy Currency Inserted InCST";
                            context.CurrencyStockDB.Add(objlist);
                            int id = context.Commit();
                        }
                        else
                        {
                            var existData = context.CurrencyStockDB.Where(x => x.DeliveryIssuanceId == existDatas.DeliveryIssuanceId && x.CurrencyStockid == existDatas.CurrencyStockid && x.Deleted == false).FirstOrDefault();

                            //CurrencyStock CST = new CurrencyStock();
                            existData.OneRupee += objlist.OneRupee;
                            existData.onerscount += objlist.onerscount;
                            existData.TwoRupee += objlist.TwoRupee;
                            existData.tworscount += objlist.tworscount;
                            existData.FiveRupee += objlist.FiveRupee;
                            existData.fiverscount += objlist.fiverscount;
                            existData.FiveNote += objlist.FiveNote;
                            existData.FiveNoteCount += objlist.FiveNoteCount;
                            existData.TenRupee += objlist.TenRupee;
                            existData.tenrscount += objlist.tenrscount;
                            existData.TenNote += objlist.TenNote;
                            existData.TenNoteCount += objlist.TenNoteCount;
                            existData.TwentyRupee += objlist.TwentyRupee;
                            existData.Twentyrscount += objlist.Twentyrscount;
                            existData.fiftyRupee += objlist.fiftyRupee;
                            existData.fiftyrscount += objlist.fiftyrscount;
                            existData.HunRupee += objlist.HunRupee;
                            existData.hunrscount += objlist.hunrscount;
                            existData.twoHunRupee += objlist.twoHunRupee;
                            existData.twohunrscount += objlist.twohunrscount;
                            existData.fiveHRupee += objlist.fiveHRupee;
                            existData.fivehrscount += objlist.fivehrscount;
                            existData.twoTHRupee += objlist.twoTHRupee;
                            existData.twoTHrscount += objlist.twoTHrscount;
                            existData.TotalAmount += objlist.TotalAmount;
                            existData.TotalAmount += objlist.Dueamount;
                            //existData.Dueamount += objlist.Dueamount;
                            existData.UpdatedDate = indianTime;
                            //context.CurrencyStockDB.Attach(existData);
                            context.Entry(existData).State = EntityState.Modified;
                            context.Commit();
                        }
                        if (deliveryBoy != null)
                        {
                            DBoyCurrency db = context.DBoyCurrencyDB.Where(x => x.DBoyCId == objlist.DBoyCId).FirstOrDefault();

                            db.Dueamountstatus = " Settled ";

                            context.DBoyCurrencyDB.Attach(db);
                            context.Entry(db).State = EntityState.Modified;
                            context.Commit();


                        }
                        try
                        {
                            CurrencyHistory currehistory = context.CurrencyHistoryDB.Where(x => x.CurrencyHistoryid == 1 && x.Deleted == false).FirstOrDefault();
                            currehistory.OneRupee += objlist.OneRupee;
                            currehistory.onerscount += objlist.onerscount;
                            currehistory.TwoRupee += objlist.TwoRupee;
                            currehistory.tworscount += objlist.tworscount;
                            currehistory.FiveRupee += objlist.FiveRupee;
                            currehistory.fiverscount += objlist.fiverscount;
                            currehistory.TenRupee += objlist.TenRupee;
                            currehistory.tenrscount += objlist.tenrscount;
                            currehistory.FiveNote += objlist.FiveNote;
                            currehistory.FiveNoteCount += objlist.FiveNoteCount;
                            currehistory.TenNote += objlist.TenNote;
                            currehistory.TenNoteCount += objlist.TenNoteCount;
                            currehistory.TwentyRupee += objlist.TwentyRupee;
                            currehistory.Twentyrscount += objlist.Twentyrscount;
                            currehistory.fiftyRupee += objlist.fiftyRupee;
                            currehistory.fiftyrscount += objlist.fiftyrscount;
                            currehistory.HunRupee += objlist.HunRupee;
                            currehistory.hunrscount += objlist.hunrscount;
                            currehistory.twoHunRupee += objlist.twoHunRupee;
                            currehistory.twohunrscount += objlist.twohunrscount;
                            currehistory.fiveHRupee += objlist.fiveHRupee;
                            currehistory.fivehrscount += objlist.fivehrscount;
                            currehistory.twoTHRupee += objlist.twoTHRupee;
                            currehistory.twoTHrscount += objlist.twoTHrscount;

                            currehistory.TotalAmount += objlist.TotalAmount;
                            currehistory.TotalAmount += objlist.Dueamount;

                            //currehistory.DeliveryIssuanceId = objlist.DeliveryIssuanceId;
                            currehistory.UpdatedDate = indianTime;
                            currehistory.CreatedDate = indianTime;
                            currehistory.DboyName = deliveryBoy.DisplayName;
                            currehistory.DBoyCId = deliveryBoy.PeopleID;
                            currehistory.status = "due amount add successfully";
                            context.CurrencyHistoryDB.Attach(currehistory);
                            context.Entry(currehistory).State = EntityState.Modified;
                            context.Commit();
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.Message);
                        }
                    }
                }
                catch (Exception ex)
                {

                    return null;
                }
            }
            return true;

        }
        [Route("Checkdeatil")]
        [HttpPost]
        public dynamic CheckdeatilStock(CheckCurrency objlist, int PeopleID) //Order change delivery boy
        {
            using (var context = new AuthContext())
            {
                try
                {

                    OrderDispatchedMaster od = context.OrderDispatchedMasters.Where(x => x.OrderId == objlist.OrderId).FirstOrDefault();

                    if (od != null)
                    {
                        od.DboycheckRecived = true;
                        //context.OrderDispatchedMasters.Attach(od);
                        context.Entry(od).State = EntityState.Modified;
                        context.Commit();
                    }
                    var deliveryBoy = context.Peoples.Where(x => x.PeopleID == PeopleID && x.Deleted == false).FirstOrDefault();

                    if (objlist != null)
                    {
                        objlist.UpdatedDate = indianTime;
                        objlist.CreatedDate = indianTime;
                        objlist.status = "Receive";
                        context.CheckCurrencyDB.Add(objlist);
                        int id = context.Commit();
                    }


                }
                catch (Exception ex)
                {

                    return null;
                }
            }
            return objlist;

        }
    }
    public class OrderDispatchedMasterOTM
    {
        public int OrderDispatchedMasterId { get; set; }
        public int DeliveryIssuanceId { get; set; }
        public int OrderId { get; set; }
        public int CompanyId { get; set; }
        public int? SalesPersonId { get; set; }
        public string SalesPerson { get; set; }
        public string SalesMobile { get; set; }
        public int CustomerId { get; set; }
        public string CustomerName { get; set; }
        public string ShopName { get; set; }
        public string Skcode { get; set; }
        public string Status { get; set; }
        public string invoice_no { get; set; }
        public string Trupay { get; set; }
        public int CustomerCategoryId { get; set; }
        public string CustomerCategoryName { get; set; }
        public string CustomerType { get; set; }
        public string Customerphonenum { get; set; }
        public string BillingAddress { get; set; }
        public string ShippingAddress { get; set; }
        public string comments { get; set; }
        public double deliveryCharge { get; set; }
        public double TotalAmount { get; set; }
        public double GrossAmount { get; set; }
        public double DiscountAmount { get; set; }
        public double TaxAmount { get; set; }
        public double SGSTTaxAmmount { get; set; }
        public double CGSTTaxAmmount { get; set; }
        public string CheckNo { get; set; }
        public double CheckAmount { get; set; }
        public string ElectronicPaymentNo { get; set; }
        public double ElectronicAmount { get; set; }
        public double CashAmount { get; set; }
        public double PaymentAmount { get; set; }
        public double RecivedAmount { get; set; }
        public string DboyName { get; set; }
        public string DboyMobileNo { get; set; }
        public int ReDispatchCount { get; set; }

        public int? CityId { get; set; }
        public int Warehouseid { get; set; }
        public string WarehouseName { get; set; }
        public bool active { get; set; }

        public bool DboycheckRecived { get; set; }
        public DateTime OrderedDate { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime Deliverydate { get; set; }
        public DateTime UpdatedDate { get; set; }
        public bool Deleted { get; set; }
        public int DivisionId { get; set; }
        public int ClusterId { get; set; }
        public string Signimg { get; set; }
        public string ClusterName { get; set; }
        [NotMapped]
        public DateTime OrderDate { get; set; }
        [NotMapped]
        public bool check { get; set; }
        [NotMapped]
        public double lat { get; set; }
        [NotMapped]
        public double lg { get; set; }

        public bool cash { get; set; }
        public bool electronic { get; set; }
        public bool cheq { get; set; }
        public double BounceCheqAmount { get; set; }
        public double? WalletAmount { get; set; }
        public double? RewardPoint { get; set; }
        public double ShortAmount { get; set; }
        public int? OrderTakenSalesPersonId { get; set; }
        public string OrderTakenSalesPerson { get; set; }
        public string Tin_No { get; set; }
    }
}
