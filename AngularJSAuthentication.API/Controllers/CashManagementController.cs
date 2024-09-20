using AngularJSAuthentication.API.DataContract;
using AngularJSAuthentication.Model.CashManagement;
using LinqKit;
using Microsoft.Extensions.Logging;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace AngularJSAuthentication.API.Controllers
{
    public class CashManagementController : ApiController
    {
        private static TimeZoneInfo INDIAN_ZONE = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
        DateTime indianTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE);
        private static Logger logger = LogManager.GetCurrentClassLogger();

        [Route("GetWarehouseCash")]
        [HttpGet]
        public WcurrencycollectionPaggingData GetWarehouseCash(int totalitem, int page,int warehouseid, int? dBoyPeopleId, string status)
        {
            WcurrencycollectionPaggingData wcurrencycollectionPaggingData = new WcurrencycollectionPaggingData();
            List<WarehousecurrencycollectionDc> WarehousecurrencycollectionDcs = new List<WarehousecurrencycollectionDc>();
            
            try
            {

                var identity = User.Identity as ClaimsIdentity;
                int compid = 0, userid = 0;

                if(identity !=null && identity.Claims!=null && identity.Claims.Any(x=>x.Type== "compid"))
                    compid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                    userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

                
                logger.Info("User ID : {0} , Company Id : {1}", compid, userid);
                using (AuthContext context = new AuthContext())
                {
                    var predicate = PredicateBuilder.True<CurrencyCollection>();
                    predicate = predicate.And(x => x.Warehouseid == warehouseid);
                    predicate = predicate.And(x =>  x.IsActive);
                    predicate = predicate.And(x => x.IsDeleted.HasValue);
                    //if (deliveryissueid.HasValue)
                    //    predicate = predicate.And(x => x.Deliveryissueid == deliveryissueid);
                    if (dBoyPeopleId.HasValue)
                        predicate = predicate.And(x => x.DBoyPeopleId == dBoyPeopleId);

                    if(!string.IsNullOrEmpty(status))
                        predicate = predicate.And(x => x.Status == status);

                    WarehousecurrencycollectionDcs = context.CurrencyCollection.Where(predicate).Select(x => new WarehousecurrencycollectionDc
                    {
                        Id = x.Id,
                        Warehouseid = x.Warehouseid,
                        DBoyPeopleId = x.DBoyPeopleId,
                        Deliveryissueid = x.Deliveryissueid,
                        TotalCashAmt = x.TotalCashAmt,
                        TotalOnlineAmt = x.TotalOnlineAmt,
                        TotalCheckAmt = x.TotalCheckAmt,
                        TotalDeliveryissueAmt = x.TotalDeliveryissueAmt,
                        CreatedDate = x.CreatedDate,
                        TotalDueAmt = x.TotalDueAmt,
                        Status = x.Status                        
                    }).ToList();
                }
                wcurrencycollectionPaggingData.total_count = WarehousecurrencycollectionDcs.Count();
                wcurrencycollectionPaggingData.WarehousecurrencycollectionDcs = WarehousecurrencycollectionDcs.OrderByDescending(x => x.Id).Skip((page - 1) * totalitem).Take(totalitem).ToList();
                return wcurrencycollectionPaggingData;
            }
            catch (Exception ex)
            {
                logger.Error("Error in GetWarehouseCash Method: " + ex.Message);                
                return null;
            }
        }


        [Route("GetCashCollection")]
        [HttpGet]
        public List<CashCollectionDc> GetWarehouseCash(long currencyCollectionId)
        {  
            List<CashCollectionDc> CashCollectionDcs = new List<CashCollectionDc>();
            try
            {             
                using (AuthContext context = new AuthContext())
                {
                    var q =
                            from c in context.CurrencyDenomination
                            join p in context.CashCollection.Where(o => o.CurrencyCollectionId == currencyCollectionId) 
                            on c.Id equals p.CurrencyDenominationId into ps                            
                            from p in ps.DefaultIfEmpty()
                            select new CashCollectionDc
                            {
                                CurrencyCollectionId = currencyCollectionId,
                                CurrencyCountByDBoy = p == null ? 0 : p.CurrencyCountByDBoy,
                                CurrencyCountByWarehouse = p == null ? 0 : p.CurrencyCountByWarehouse,
                                CurrencyDenominationId = c.Id,
                                DBoyPeopleId = p == null ? 0 : p.DBoyPeopleId,
                                WarehousePeopleId = p == null ? 0 : p.WarehousePeopleId,
                                CurrencyDenominationTitle = c == null ? "" : c.Title,
                                CurrencyDenominationValue = c == null ? 0 : c.Value,
                                Id = p == null ? 0 : p.Id
                            };

                    CashCollectionDcs = q.ToList();

                }

                return CashCollectionDcs;
            }
            catch (Exception ex)
            {
                logger.Error("Error in GetWarehouseCash Method: " + ex.Message);
                return null;
            }
        }

        [Route("GetChequeCollection")]
        [HttpGet]
        public List<ChequeCollectionDc> GetChequeCollection(long currencyCollectionId)
        {
            List<ChequeCollectionDc> ChequeCollectionDcs = new List<ChequeCollectionDc>();
            try
            {
                using (AuthContext context = new AuthContext())
                {
                    ChequeCollectionDcs = context.ChequeCollection.Where(x => x.CurrencyCollectionId == currencyCollectionId).Select(x =>
                        new ChequeCollectionDc
                        {
                            ChequeAmt = x.ChequeAmt,
                            ChequeDate = x.ChequeDate,
                            ChequeNumber = x.ChequeNumber,
                            ChequeStatus = x.ChequeStatus,
                            CurrencyCollectionId = x.CurrencyCollectionId,
                            DBoyPeopleId = x.DBoyPeopleId,
                            Id = x.Id,
                            IsChequeClear = x.IsChequeClear,
                            WarehousePeopleId = x.WarehousePeopleId,
                        }).ToList();

                }

                return ChequeCollectionDcs;
            }
            catch (Exception ex)
            {
                logger.Error("Error in GetChequeCollection Method: " + ex.Message);
                return null;
            }
        }

        [Route("GetOnlineCollection")]
        [HttpGet]
        public List<OnlineCollectionDc> GetOnlineCollection(long currencyCollectionId)
        {
            List<OnlineCollectionDc> OnlineCollectionDcs = new List<OnlineCollectionDc>();
            try
            {
                using (AuthContext context = new AuthContext())
                {
                    OnlineCollectionDcs = context.OnlineCollection.Where(x => x.CurrencyCollectionId == currencyCollectionId).Select(x =>
                        new OnlineCollectionDc
                        {
                            CurrencyCollectionId=x.CurrencyCollectionId,
                            Id=x.Id,
                            MPOSAmt=x.MPOSAmt,
                            MPOSReferenceNo=x.MPOSReferenceNo,
                            PaymentGetwayAmt=x.PaymentGetwayAmt,
                            PaymentReferenceNO=x.PaymentReferenceNO
                        }).ToList();
                }

                return OnlineCollectionDcs;
            }
            catch (Exception ex)
            {
                logger.Error("Error in GetOnlineCollection Method: " + ex.Message);
                return null;
            }
        }

    }
}
