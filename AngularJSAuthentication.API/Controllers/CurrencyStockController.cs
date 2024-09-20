using AngularJSAuthentication.Model;
using GenricEcommers.Models;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Web.Http;

namespace AngularJSAuthentication.API.Controllers
{
    [RoutePrefix("api/CurrencyStock")]
    public class CurrencyStockController : ApiController
    {

        private static Logger logger = LogManager.GetCurrentClassLogger();

        //[Route("")]
        //[HttpGet]
        //public IEnumerable<CurrencyHistory> Get(int id)
        //{
        //    //return null;
        //    logger.Info("start StockCurrency: ");
        //    List<CurrencyHistory> ass = new List<CurrencyHistory>();
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

        //        logger.Info("User ID : {0} , Company Id : {1}", compid, userid);
        //        ass = context.TotalStockCurrencys(id).ToList();
        //        logger.Info("End  StockCurrency: ");
        //        return ass;
        //    }
        //    catch (Exception ex)
        //    {
        //        logger.Error("Error in StockCurrency " + ex.Message);
        //        logger.Info("End  StockCurrency: ");
        //        return null;
        //    }
        //}

        [Route("")]
        [HttpGet]
        public IEnumerable<CurrencyHistory> Get()
        {
            //return null;
            logger.Info("start StockCurrency: ");
            using (AuthContext context = new AuthContext())
            {
                List<CurrencyHistory> ass = new List<CurrencyHistory>();
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
                    ass = context.TotalStockCurrencys().ToList();
                    logger.Info("End  StockCurrency: ");
                    return ass;
                }
                catch (Exception ex)
                {
                    logger.Error("Error in StockCurrency " + ex.Message);
                    logger.Info("End  StockCurrency: ");
                    return null;
                }
            }
        }

        [Route("BankDisposible")]
        [HttpGet]
        public IEnumerable<BankDisposable> BankDisposible()
        {
            //return null;
            logger.Info("start StockCurrency: ");
            using (AuthContext db = new AuthContext())
            {
                List<BankDisposable> ass = new List<BankDisposable>();
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
                    ass = db.BankDisposableDB.ToList();
                    logger.Info("End  StockCurrency: ");
                    return ass;
                }
                catch (Exception ex)
                {
                    logger.Error("Error in StockCurrency " + ex.Message);
                    logger.Info("End  StockCurrency: ");
                    return null;
                }
            }
        }
        [Route("")]
        [HttpGet]
        public IEnumerable<CurrencyStock> Get11(string Stock_status)
        {
            logger.Info("start StockCurrency: ");
            using (AuthContext context = new AuthContext())
            {
                List<CurrencyStock> ass = new List<CurrencyStock>();
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
                    ass = context.AllStockCurrencys(Stock_status).ToList();
                    logger.Info("End  StockCurrency: ");
                    return ass;
                }
                catch (Exception ex)
                {
                    logger.Error("Error in StockCurrency " + ex.Message);
                    logger.Info("End  StockCurrency: ");
                    return null;
                }
            }
        }

        [Route("GetDelivaryBoyCurrencyData")]
        [HttpGet]
        public IEnumerable<CurrencyStock> GetDboyCurrency()
        {
            logger.Info("start StockCurrency: ");
            using (AuthContext context = new AuthContext())
            {
                List<CurrencyStock> ass = new List<CurrencyStock>();
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
                    ass = context.GetDboyCurrencyData().ToList();
                    logger.Info("End  StockCurrency: ");
                    return ass;
                }
                catch (Exception ex)
                {
                    logger.Error("Error in StockCurrency " + ex.Message);
                    logger.Info("End  StockCurrency: ");
                    return null;
                }
            }
        }

        [Route("DelivaryBoyTotal")]
        [HttpGet]
        public IEnumerable<CurrencyStock> DelivaryBoyTotal()
        {
            logger.Info("start StockCurrency: ");
            using (AuthContext context = new AuthContext())
            {
                List<CurrencyStock> ass = new List<CurrencyStock>();
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
                    ass = context.DelivaryBoyTotalData().ToList();
                    logger.Info("End  StockCurrency: ");
                    return ass;
                }
                catch (Exception ex)
                {
                    logger.Error("Error in StockCurrency " + ex.Message);
                    logger.Info("End  StockCurrency: ");
                    return null;
                }
            }
        }

        //[Route("historyget")]
        //[HttpGet]
        //public IEnumerable<CurrencyBankSettle> GetHistory(string Stock_status)
        //{
        //    logger.Info("start StockCurrency: ");
        //    List<CurrencyBankSettle> ass = new List<CurrencyBankSettle>();
        //    try
        //    {
        //        var identity = User.Identity as ClaimsIdentity;
        //        int compid = 0, userid = 0;
        //        // Access claims
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

        //        logger.Info("User ID : {0} , Company Id : {1}", compid, userid);
        //        ass = context.AllStockCurrencysHistory(Stock_status).ToList();
        //        logger.Info("End  StockCurrency: ");
        //        return ass;
        //    }
        //    catch (Exception ex)
        //    {
        //        logger.Error("Error in StockCurrency " + ex.Message);
        //        logger.Info("End  StockCurrency: ");
        //        return null;
        //    }
        //}

        //[Route("historygetin")]
        //[HttpGet]
        //public IEnumerable<CurrencyStock> GetHistoryin(string Stock_status)
        //{
        //    logger.Info("start StockCurrency: ");
        //    List<CurrencyStock> ass = new List<CurrencyStock>();
        //    try
        //    {
        //        var identity = User.Identity as ClaimsIdentity;
        //        int compid = 0, userid = 0;
        //        // Access claims
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

        //        logger.Info("User ID : {0} , Company Id : {1}", compid, userid);
        //        ass = context.AllStockCurrencysHistoryin(Stock_status).ToList();
        //        logger.Info("End  StockCurrency: ");
        //        return ass;
        //    }
        //    catch (Exception ex)
        //    {
        //        logger.Error("Error in StockCurrency " + ex.Message);
        //        logger.Info("End  StockCurrency: ");
        //        return null;
        //    }
        //}
        [Route("BanksettleCurrency")]
        [HttpPost]
        public HttpResponseMessage BankCurrencyStock(CurrencyBankSettle obj, int id)
        {
            using (AuthContext db = new AuthContext())
            {
                try
                {
                    var DBoyorders = db.BankStock(obj, id);
                    if (DBoyorders == null)
                    {
                        return Request.CreateResponse(HttpStatusCode.BadRequest, "Error Occured");
                    }
                    return Request.CreateResponse(HttpStatusCode.OK, DBoyorders);
                }
                catch (Exception ex)
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, ex.Message);
                }
            }
        }
        [Route("BankSettleAmount")]
        [HttpGet]
        public IEnumerable<CurrencyBankSettle> Get1()
        {
            logger.Info("start StockCurrency: ");
            using (AuthContext context = new AuthContext())
            {
                List<CurrencyBankSettle> ass = new List<CurrencyBankSettle>();
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
                    ass = context.AllBankStockCurrencys().ToList();
                    logger.Info("End  StockCurrency: ");
                    return ass;
                }
                catch (Exception ex)
                {
                    logger.Error("Error in StockCurrency " + ex.Message);
                    logger.Info("End  StockCurrency: ");
                    return null;
                }
            }
        }

        [Route("GetBankSettle")]
        [HttpGet]
        public IEnumerable<CurrencyBankSettle> GetBankSettle()
        {
            logger.Info("start StockCurrency: ");
            using (AuthContext context = new AuthContext())
            {
                List<CurrencyBankSettle> ass = new List<CurrencyBankSettle>();
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
                    ass = context.AllBankStockCurrencysByDate().ToList();
                    logger.Info("End  StockCurrency: ");
                    return ass;
                }
                catch (Exception ex)
                {
                    logger.Error("Error in StockCurrency " + ex.Message);
                    logger.Info("End  StockCurrency: ");
                    return null;
                }
            }
        }

        [Route("BankSettleAmountPut")]
        [HttpPut]
        public CurrencyBankSettle Put(CurrencyBankSettle obj)
        {
            using (AuthContext db = new AuthContext())
            {
                //return null;
                try
                {
                    return db.BankCurrencyPut(obj);
                }
                catch (Exception ex)
                {
                    logger.Error("Error in Put News " + ex.Message);
                    return null;
                }
            }
        }
        [Route("BankSettleAmountGet")]
        [HttpGet]
        public IEnumerable<CurrencyBankSettle> Getbank(int id)
        {
            //return null;
            logger.Info("start StockCurrency: ");
            using (AuthContext context = new AuthContext())
            {
                List<CurrencyBankSettle> ass = new List<CurrencyBankSettle>();
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
                    ass = context.Imagegetview(id).ToList();
                    logger.Info("End  StockCurrency: ");
                    return ass;
                }
                catch (Exception ex)
                {
                    logger.Error("Error in StockCurrency " + ex.Message);
                    logger.Info("End  StockCurrency: ");
                    return null;
                }
            }
        }
        [Route("Checkget")]
        [HttpGet]
        public IEnumerable<CheckCurrency> Getcheckdata(string status)
        {
            logger.Info("start StockCurrency: ");
            using (AuthContext context = new AuthContext())
            {
                List<CheckCurrency> ass = new List<CheckCurrency>();
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
                    ass = context.AllStockCurrencyscheck(status).ToList();
                    logger.Info("End  StockCurrency: ");
                    return ass;
                }
                catch (Exception ex)
                {
                    logger.Error("Error in StockCurrency " + ex.Message);
                    logger.Info("End  StockCurrency: ");
                    return null;
                }
            }
        }
        [Route("GetDelivaryBoyData")]
        [HttpGet]
        public IEnumerable<People> GetDelivaryBoy()
        {
            logger.Info("start StockCurrency: ");
            using (AuthContext context = new AuthContext())
            {
                List<People> ass = new List<People>();
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

                    logger.Info("User ID : {0} , Company Id : {1}", compid, userid);
                    ass = context.AllDBoyWid(compid, Warehouse_id).ToList();
                    logger.Info("End  StockCurrency: ");
                    return ass;
                }
                catch (Exception ex)
                {
                    logger.Error("Error in StockCurrency " + ex.Message);
                    logger.Info("End  StockCurrency: ");
                    return null;
                }
            }
        }

        [Route("CurrencyUp")]
        [HttpPost]
        public string CurrencyUp(CurrencyData cc)
        {
            using (AuthContext context = new AuthContext())
            {
                try
                {
                    string rr = context.InsertCurrencyData(cc);
                    return rr;

                }
                catch (Exception ee)
                {
                    return ee.Message;
                }
            }
        }

        [Route("GetCurrencyData")]
        [HttpGet]
        public CurrencyData GetCurrencyData()
        {
            logger.Info("start StockCurrency: ");
            using (AuthContext context = new AuthContext())
            {
                var ass = new CurrencyData();
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

                    logger.Info("User ID : {0} , Company Id : {1}", compid, userid);
                    var today = DateTime.Today.Date;
                    var yesterday = today.AddDays(-2);
                    var yedate = yesterday.Date;
                    using (var db = new AuthContext())
                    {
                        var data = (from t in db.CurrencyDataDB where t.Deleted == false && t.CreatedDate < today /*&& t.CreatedDate > yesterday*/ select t).SingleOrDefault();

                        return data;
                    }
                }
                catch (Exception ex)
                {
                    logger.Error("Error in StockCurrency " + ex.Message);
                    logger.Info("End  StockCurrency: ");
                    return null;
                }
            }
        }

        #region For Paggination
        [Route("historygetin")]
        [HttpGet]
        public PaggingData_ctin GetD(int list, int page, string status)
        {

            logger.Info("start OrderMaster: ");
            //  List<OrderMaster> ass = new List<OrderMaster>();
            using (AuthContext context = new AuthContext())
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
                    logger.Info("User ID : {0} , Company Id : {1}", compid, userid);
                    var ass = context.AllCurrencyHistoryIN(list, page, status);
                    logger.Info("End OrderMaster: ");
                    return ass;
                }
                catch (Exception ex)
                {
                    logger.Error("Error in OrderMaster " + ex.Message);
                    logger.Info("End  OrderMaster: ");
                    return null;
                }
            }
        }
        [Route("HistoryOut")]
        [HttpGet]
        public PaggingData_ctout GetE(int list, int page, string status)
        {

            logger.Info("start OrderMaster: ");
            using (AuthContext context = new AuthContext())
            {
                //  List<OrderMaster> ass = new List<OrderMaster>();
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
                    logger.Info("User ID : {0} , Company Id : {1}", compid, userid);
                    var ass = context.AllCurrencyHistoryOut(list, page, status);
                    logger.Info("End OrderMaster: ");
                    return ass;
                }
                catch (Exception ex)
                {
                    logger.Error("Error in OrderMaster " + ex.Message);
                    logger.Info("End  OrderMaster: ");
                    return null;
                }
            }
        }
        #endregion

        [Route("getcomparehistory")]
        [HttpGet]
        public CurrencyHistory GetCompareData()
        {
            logger.Info("start StockCurrency: ");
            using (AuthContext context = new AuthContext())
            {
                var ass = new CurrencyHistory();
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

                    using (var db = new AuthContext())
                    {
                        var data = db.CurrencyHistoryDB.Where(x => x.Deleted == false).FirstOrDefault();


                        return data;
                    }
                }
                catch (Exception ex)
                {
                    logger.Error("Error in StockCurrency " + ex.Message);
                    logger.Info("End  StockCurrency: ");
                    return null;
                }
            }

        }
    }
}




