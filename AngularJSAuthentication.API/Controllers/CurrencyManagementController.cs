using AgileObjects.AgileMapper;
using AngularJSAuthentication.API.DataContract;
using AngularJSAuthentication.API.Helper;
using AngularJSAuthentication.API.Helpers;
using AngularJSAuthentication.API.Managers;
using AngularJSAuthentication.API.WebAPIHelper;
using AngularJSAuthentication.Common.Constants;
using AngularJSAuthentication.DataContracts.Masters;
using AngularJSAuthentication.DataContracts.Transaction.Mongo;
using AngularJSAuthentication.DataContracts.Transaction.Reports;
using AngularJSAuthentication.Model;
using AngularJSAuthentication.Model.CashManagement;
using Hangfire;
using LinqKit;
using Nito.AspNetBackgroundTasks;
using NLog;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Transactions;
using System.Web;
using System.Web.Http;
using ChequeCollection = AngularJSAuthentication.Model.CashManagement.ChequeCollection;
using System.Net;
using System.Net.Http;
using System.Data.Entity.Infrastructure;
using AngularJSAuthentication.Common.Enums;
using AngularJSAuthentication.DataContracts.External.SalesAppDc;
using Newtonsoft.Json;

namespace AngularJSAuthentication.API.Controllers
{
    [RoutePrefix("api/Currency")]
    // [Authorize]
    [AllowAnonymous]


    public class CurrencyManagementController : ApiController
    {
        private static TimeZoneInfo INDIAN_ZONE = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
        DateTime indianTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE);
        private static Logger logger = LogManager.GetCurrentClassLogger();
        // private static List<long> currencyCollectionIds;
        public static List<long> currencyCollectionIds = new List<long>();
        #region Warehouse Screen Method
        [Route("GetWarehouseCurrency")]
        [HttpGet]
        public WcurrencycollectionPaggingData GetWarehouseCash(int totalitem, int page, int warehouseid, int? dBoyPeopleId, string status)
        {
            WcurrencycollectionPaggingData wcurrencycollectionPaggingData = new WcurrencycollectionPaggingData();
            List<WarehousecurrencycollectionDc> WarehousecurrencycollectionDcs = new List<WarehousecurrencycollectionDc>();
            try
            {
                var identity = User.Identity as ClaimsIdentity;
                int compid = 0, userid = 0;

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))
                    compid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                    userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

                logger.Info("User ID : {0} , Company Id : {1}", compid, userid);

                using (AuthContext context = new AuthContext())
                {
                    var predicate = PredicateBuilder.True<CurrencyCollection>();
                    predicate = predicate.And(x => x.Warehouseid == warehouseid);
                    predicate = predicate.And(x => x.IsActive);
                    predicate = predicate.And(x => x.IsDeleted.HasValue && !x.IsDeleted.Value);
                    //if (Deliveryissueid.HasValue)
                    //predicate = predicate.And(x => x.Deliveryissueid == Deliveryissueid);
                    if (dBoyPeopleId > 0)
                        predicate = predicate.And(x => x.DBoyPeopleId == dBoyPeopleId);

                    //predicate = predicate.And(x => x.Orderid == orderid);                    
                    if (!string.IsNullOrEmpty(status))
                        predicate = predicate.And(x => x.Status == status);

                    //var query = context.CurrencyCollection.Where(predicate).Select( x => new WarehousecurrencycollectionDc

                    if (status == "Settlement")
                    {
                        //var query = (from x in context.CurrencyCollection
                        //             where x.Warehouseid == warehouseid && x.Status == status 
                        //             orderby x.CreatedDate descending
                        //             select new WarehousecurrencycollectionDc
                        var query = context.CurrencyCollection.Where(predicate).Select(x => new WarehousecurrencycollectionDc
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
                            TotalDueAmt = x.TotalDueAmt + x.Fine,
                            //TotalDueAmt = (x.TotalDueAmt>0)? x.TotalDueAmt + x.Fine: x.Fine,
                            Status = x.Status,
                            IsCashVerify = x.IsCashVerify,
                            IsChequeVerify = x.IsChequeVerify,
                            IsOnlinePaymentVerify = x.IsOnlinePaymentVerify,
                            DeclineNote = x.DeclineNote,
                            WareHouseSettleDate = x.WareHouseSettleDate,
                        }).OrderByDescending(x => x.CreatedDate).ToList();
                        wcurrencycollectionPaggingData.total_count = query.Count();
                        wcurrencycollectionPaggingData.WarehousecurrencycollectionDcs = query.OrderByDescending(x => x.CreatedDate).Skip((page - 1) * totalitem).Take(totalitem).ToList();
                    }

                    if (status != "Settlement")
                    {
                        var query = from c in context.OTPVerificationDb
                                    join x in context.CurrencyCollection.Where(predicate)
                                    on c.AssignmentID equals x.Deliveryissueid
                                    where c.ModifiedBy == userid
                                    select new WarehousecurrencycollectionDc
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
                                        TotalDueAmt = x.TotalDueAmt + x.Fine,
                                        //TotalDueAmt = (x.TotalDueAmt>0)? x.TotalDueAmt + x.Fine: x.Fine,
                                        Status = x.Status,
                                        IsCashVerify = x.IsCashVerify,
                                        IsChequeVerify = x.IsChequeVerify,
                                        IsOnlinePaymentVerify = x.IsOnlinePaymentVerify,
                                        DeclineNote = x.DeclineNote,
                                        WareHouseSettleDate = x.WareHouseSettleDate,

                                        //Comment = x.Comment,

                                    };
                        wcurrencycollectionPaggingData.total_count = query.Count();
                        wcurrencycollectionPaggingData.WarehousecurrencycollectionDcs = query.ToList();
                    }

                    if (wcurrencycollectionPaggingData.WarehousecurrencycollectionDcs != null && wcurrencycollectionPaggingData.WarehousecurrencycollectionDcs.Any())
                    {
                        var peoplesids = wcurrencycollectionPaggingData.WarehousecurrencycollectionDcs.Select(x => x.DBoyPeopleId).Distinct().ToList();
                        var peopeldata = context.Peoples.Where(x => peoplesids.Contains(x.PeopleID)).Select(x => new { x.PeopleID, x.DisplayName });
                        foreach (var item in wcurrencycollectionPaggingData.WarehousecurrencycollectionDcs)
                        {
                            item.DBoyPeopleName = peopeldata != null && peopeldata.Any(x => x.PeopleID == item.DBoyPeopleId) ? peopeldata.FirstOrDefault(x => x.PeopleID == item.DBoyPeopleId).DisplayName : "";

                        }
                    }
                }
                return wcurrencycollectionPaggingData;
            }

            catch (Exception ex)
            {
                logger.Error("Error in GetWarehouseCash Method: " + ex.Message);
                return null;
            }
        }

        public class DeliveryissueAgent
        {
            public int AgentId { get; set; }
            public int DeliveryIssuanceId { get; set; }
        }

        #region for search by Assignment Id and status
        /// <summary>
        /// for search by Assignment Id (Date:19-06-2019) created by anushka
        /// </summary>
        /// <param name="totalitem"></param>
        /// <param name="page"></param>
        /// <param name="warehouseid"></param>
        /// <param name="dBoyPeopleId"></param>
        /// <param name="Deliveryissueid"></param>
        /// <returns></returns>
        [Route("GetWarehouseCurrencybyDeliveryIssueId")]
        [HttpGet]
        public WcurrencycollectionPaggingData GetWarehouseCurrencybyDeliveryIssueId(int totalitem, int page, int? warehouseid, int? Deliveryissueid, string status, DateTime? StartDate, DateTime? EndDate)
        {
            WcurrencycollectionPaggingData wcurrencycollectionPaggingData = new WcurrencycollectionPaggingData();
            List<WarehousecurrencycollectionDc> WarehousecurrencycollectionDcs = new List<WarehousecurrencycollectionDc>();
            try
            {
                var identity = User.Identity as ClaimsIdentity;
                int compid = 0, userid = 0;

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))
                    compid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                    userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

                logger.Info("User ID : {0} , Company Id : {1}", compid, userid);


                using (AuthContext context = new AuthContext())
                {
                    var predicate = PredicateBuilder.True<CurrencyCollection>();
                    predicate = predicate.And(x => x.Warehouseid == warehouseid);
                    predicate = predicate.And(x => x.IsActive);
                    predicate = predicate.And(x => x.IsDeleted.HasValue && !x.IsDeleted.Value);
                    //if (Deliveryissueid.HasValue)
                    //predicate = predicate.And(x => x.Deliveryissueid == Deliveryissueid);
                    //if (dBoyPeopleId.HasValue)
                    //    predicate = predicate.And(x => x.DBoyPeopleId == dBoyPeopleId);
                    if (Deliveryissueid > 0)
                    {

                        predicate = predicate.And(x => x.Deliveryissueid == Deliveryissueid);
                    }
                    //predicate = predicate.And(x => x.Orderid == orderid);

                    if (StartDate.HasValue && EndDate.HasValue)
                    {
                        predicate = predicate.And(x => EntityFunctions.TruncateTime(x.CreatedDate) >= EntityFunctions.TruncateTime(StartDate.Value) && EntityFunctions.TruncateTime(x.CreatedDate) <= EntityFunctions.TruncateTime(EndDate.Value));
                    }
                    if (!string.IsNullOrEmpty(status))
                        predicate = predicate.And(x => x.Status == status);


                    var query = context.CurrencyCollection.Where(predicate).Select(x => new WarehousecurrencycollectionDc
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
                        TotalDueAmt = x.TotalDueAmt + x.Fine,
                        //TotalDueAmt = (x.TotalDueAmt>0)? x.TotalDueAmt + x.Fine: x.Fine,
                        Status = x.Status,
                        IsCashVerify = x.IsCashVerify,
                        IsChequeVerify = x.IsChequeVerify,
                        IsOnlinePaymentVerify = x.IsOnlinePaymentVerify,
                        DeclineNote = x.DeclineNote,
                        WareHouseSettleDate = x.WareHouseSettleDate,
                    }).ToList();
                    wcurrencycollectionPaggingData.total_count = query.Count();
                    wcurrencycollectionPaggingData.WarehousecurrencycollectionDcs = query.Skip((page - 1) * totalitem).Take(totalitem).ToList();

                    if (wcurrencycollectionPaggingData.WarehousecurrencycollectionDcs != null && wcurrencycollectionPaggingData.WarehousecurrencycollectionDcs.Any())
                    {
                        var peoplesids = wcurrencycollectionPaggingData.WarehousecurrencycollectionDcs.Select(x => x.DBoyPeopleId).Distinct().ToList();
                        var peopeldata = context.Peoples.Where(x => peoplesids.Contains(x.PeopleID)).Select(x => new { x.PeopleID, x.DisplayName });
                        foreach (var item in wcurrencycollectionPaggingData.WarehousecurrencycollectionDcs)
                        {
                            item.DBoyPeopleName = peopeldata != null && peopeldata.Any(x => x.PeopleID == item.DBoyPeopleId) ? peopeldata.FirstOrDefault(x => x.PeopleID == item.DBoyPeopleId).DisplayName : "";

                        }
                    }
                }
                return wcurrencycollectionPaggingData;
            }
            catch (Exception ex)
            {
                logger.Error("Error in GetWarehouseCash Method: " + ex.Message);
                return null;
            }
        }
        #endregion

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
                            from c in context.CurrencyDenomination.Where(x => x.IsActive)
                            join p in context.CashCollection.Where(o => o.CurrencyCollectionId == currencyCollectionId && o.IsActive
                            && ((o.IsDeleted.HasValue && !o.IsDeleted.Value) || !o.IsDeleted.HasValue))
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
                                CashCurrencyType = c == null ? "" : c.currencyType,
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

        [Route("VarifyDBoyCurrency")]
        [HttpGet]
        public bool VarifyDBoyCurrency(long currencyCollectionId, int currencyType, string Comment)
        {
            bool result = false;
            try
            {
                var identity = User.Identity as ClaimsIdentity;
                int compid = 0, userid = 0;

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))
                    compid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                    userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

                using (AuthContext context = new AuthContext())
                {
                    var currencyCollection = context.CurrencyCollection.FirstOrDefault(x => x.Id == currencyCollectionId && x.Status != "Settlement");
                    if (currencyCollection != null)
                    {
                        if (currencyType == 1)
                        {
                            currencyCollection.IsCashVerify = true;
                            currencyCollection.Comment = Comment;
                        }
                        if (currencyType == 2)
                        {
                            currencyCollection.IsChequeVerify = true;
                        }
                        if (currencyType == 3)
                        {
                            currencyCollection.IsOnlinePaymentVerify = true;
                        }
                        currencyCollection.ModifiedBy = userid;
                        currencyCollection.ModifiedDate = indianTime;
                        context.Entry(currencyCollection).State = EntityState.Modified;
                        result = context.Commit() > 0;
                    }
                    else
                    {
                        result = false;
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error("Error in VarifyDBoyCurrency Method: " + ex.Message);
                result = false;
            }

            return result;
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
                    ChequeCollectionDcs = context.ChequeCollection.Where(x => x.CurrencyCollectionId == currencyCollectionId && x.IsActive
                            && ((x.IsDeleted.HasValue && !x.IsDeleted.Value) || !x.IsDeleted.HasValue)).Select(x =>
                        new ChequeCollectionDc
                        {
                            ChequeAmt = x.ChequeAmt,
                            ChequeDate = x.ChequeDate,
                            ChequeNumber = x.ChequeNumber,
                            ChequeStatus = x.ChequeStatus,
                            CurrencyCollectionId = x.CurrencyCollectionId.Value,
                            DBoyPeopleId = x.DBoyPeopleId,
                            Id = x.Id,
                            IsChequeClear = x.IsChequeClear,
                            WarehousePeopleId = x.WarehousePeopleId,
                            ChequeimagePath = x.ChequeimagePath,
                            OrderId = x.Orderid
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

        [Route("ChequeReject")]
        [HttpGet]
        public bool ChequeReject(long chequeCollectionId)
        {
            bool result = false;
            try
            {

                var identity = User.Identity as ClaimsIdentity;
                int compid = 0, userid = 0;

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))
                    compid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                    userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
                using (AuthContext context = new AuthContext())
                {
                    var chequeCollection = context.ChequeCollection.FirstOrDefault(x => x.Id == chequeCollectionId);
                    if (chequeCollection != null)
                    {
                        var currencyCollection = context.CurrencyCollection.FirstOrDefault(x => x.Id == chequeCollection.CurrencyCollectionId);
                        currencyCollection.TotalCheckAmt = currencyCollection.TotalCheckAmt - chequeCollection.ChequeAmt;
                        currencyCollection.TotalDueAmt = currencyCollection.TotalDeliveryissueAmt - (currencyCollection.TotalCashAmt + currencyCollection.TotalCheckAmt + currencyCollection.TotalOnlineAmt);
                        currencyCollection.ModifiedBy = userid;
                        currencyCollection.ModifiedDate = indianTime;
                        //context.CurrencyCollection.Attach(currencyCollection);
                        context.Entry(currencyCollection).State = EntityState.Modified;

                        chequeCollection.ChequeStatus = Convert.ToInt32(ChequeStatusEnum.Reject);
                        chequeCollection.ModifiedBy = userid;
                        chequeCollection.ModifiedDate = indianTime;
                        //context.ChequeCollection.Attach(chequeCollection);
                        context.Entry(chequeCollection).State = EntityState.Modified;
                        result = context.Commit() > 0;
                    }
                }


            }
            catch (Exception ex)
            {
                logger.Error("Error in ChequeReject Method: " + ex.Message);
            }

            return result;
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
                    OnlineCollectionDcs = context.OnlineCollection.Where(x => x.CurrencyCollectionId == currencyCollectionId && x.IsActive
                            && ((x.IsDeleted.HasValue && !x.IsDeleted.Value) || !x.IsDeleted.HasValue)).Select(x =>
                        new OnlineCollectionDc
                        {
                            CurrencyCollectionId = x.CurrencyCollectionId,
                            Id = x.Id,
                            MPOSAmt = x.MPOSAmt,
                            MPOSReferenceNo = x.MPOSReferenceNo,
                            PaymentGetwayAmt = x.PaymentGetwayAmt,
                            PaymentReferenceNO = x.PaymentReferenceNO,
                            Orderid = x.Orderid,
                            PaymentFrom = x.PaymentFrom,

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

        [Route("TotalOnlineCollection")]
        [HttpGet]
        public List<TotalOnlineCollectionDc> TotalOnlineCollection(int warehouseid)
        {
            List<TotalOnlineCollectionDc> TotalOnlineCollectionDcs = new List<TotalOnlineCollectionDc>();
            try
            {
                using (AuthContext context = new AuthContext())
                {
                    DateTime startdate = DateTime.Now.Date;

                    var param1 = new SqlParameter("@Warehouseid", warehouseid);
                    var param2 = new SqlParameter("@StartDate", startdate);

                    TotalOnlineCollectionDcs = context.Database.SqlQuery<TotalOnlineCollectionDc>("exec dbo.TotalOnlineCollectionSp @Warehouseid, @StartDate ", param1, param2).ToList();
                    return TotalOnlineCollectionDcs;
                }


            }
            catch (Exception ex)
            {
                logger.Error("Error in GetOnlineCollection Method: " + ex.Message);
                return null;
            }
        }

        [Route("GetOnlineCollectionpaging")]
        [HttpGet]
        public OnlineCollectionDcPaggingData GetOnlineCollectionpaging(int totalitem, int page, int warehouseid, string searchfilter, DateTime? StartDate, DateTime? EndDate)
        {
            OnlineCollectionDcPaggingData onlineCollectionDcPaggingData = new OnlineCollectionDcPaggingData();
            List<OnlineCollectionDc> OnlineCollectionDcs = new List<OnlineCollectionDc>();
            try
            {
                var identity = User.Identity as ClaimsIdentity;
                int compid = 0, userid = 0;

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))
                    compid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                    userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);


                using (AuthContext context = new AuthContext())
                {
                    var predicate = PredicateBuilder.True<OnlineCollection>();
                    predicate = predicate.And(x => x.CurrencyCollection.Warehouseid == warehouseid || x.WarehouseId == warehouseid);
                    predicate = predicate.And(x => x.IsActive);
                    predicate = predicate.And(x => x.IsDeleted.HasValue && !x.IsDeleted.Value);


                    if (StartDate.HasValue && EndDate.HasValue)
                    {
                        predicate = predicate.And(x => EntityFunctions.TruncateTime(x.CreatedDate) >= EntityFunctions.TruncateTime(StartDate.Value) && EntityFunctions.TruncateTime(x.CreatedDate) <= EntityFunctions.TruncateTime(EndDate.Value));
                    }
                    if (!string.IsNullOrEmpty(searchfilter))
                    {
                        predicate = predicate.And(x => x.CurrencyCollection.Deliveryissueid.ToString().Contains(searchfilter)
                                                || x.MPOSReferenceNo.Contains(searchfilter)
                                                || x.Orderid.ToString().Contains(searchfilter)
                                                || x.PaymentReferenceNO.ToString().Contains(searchfilter)
                                                || x.MPOSReferenceNo.ToString().Contains(searchfilter)
                                                || x.MPOSAmt.ToString().Contains(searchfilter)
                                                || x.PaymentGetwayAmt.ToString().Contains(searchfilter)
                                                || x.PaymentGetwayAmt.ToString().Contains(searchfilter)
                                                || x.PaymentFrom.ToString().Contains(searchfilter)
                                               );
                    }

                    context.Database.Log = s => Debug.WriteLine(s);
                    var query = context.OnlineCollection.Where(predicate).Select(x =>
                        new OnlineCollectionDc
                        {
                            CurrencyCollectionId = x.CurrencyCollectionId,
                            Id = x.Id,
                            MPOSAmt = x.MPOSAmt,
                            MPOSReferenceNo = x.MPOSReferenceNo,
                            PaymentGetwayAmt = x.PaymentGetwayAmt,
                            PaymentReferenceNO = x.PaymentReferenceNO,
                            Orderid = x.Orderid,
                            CreatedDate = x.CreatedDate,
                            PaymentFrom = x.PaymentFrom,
                            //Deliveryissueid = x.CurrencyCollection.Deliveryissueid
                            Deliveryissueid = x.CurrencyCollection != null ? x.CurrencyCollection.Deliveryissueid : (int?)null,
                        }).ToList();
                    List<OnlinePaymentDc> OnlinePaymentDcs = new List<OnlinePaymentDc>();
                    foreach (var item in query.ToList())
                    {
                        if (!string.IsNullOrEmpty(item.MPOSReferenceNo))
                        {
                            OnlinePaymentDcs.Add(new OnlinePaymentDc
                            {
                                CurrencyCollectionId = item.CurrencyCollectionId,
                                Id = item.Id,
                                Amount = item.MPOSAmt,
                                ReferenceNo = item.MPOSReferenceNo,
                                Type = "MPOS",
                                Orderid = item.Orderid,
                                Deliveryissueid = item.Deliveryissueid,
                                CreatedDate = item.CreatedDate,
                                PaymentFrom = item.PaymentFrom,
                            });
                        }
                        if (!string.IsNullOrEmpty(item.PaymentReferenceNO))
                        {

                            OnlinePaymentDcs.Add(new OnlinePaymentDc
                            {
                                CurrencyCollectionId = item.CurrencyCollectionId,
                                Id = item.Id,
                                Amount = item.PaymentGetwayAmt,
                                ReferenceNo = item.PaymentReferenceNO,
                                Type = "Online",
                                Orderid = item.Orderid,
                                Deliveryissueid = item.Deliveryissueid,
                                CreatedDate = item.CreatedDate,
                                PaymentFrom = item.PaymentFrom,
                            });
                        }
                        else if (!string.IsNullOrEmpty(item.PaymentFrom) && item.PaymentFrom == "Gullak")
                        {

                            OnlinePaymentDcs.Add(new OnlinePaymentDc
                            {
                                CurrencyCollectionId = item.CurrencyCollectionId,
                                Id = item.Id,
                                Amount = item.PaymentGetwayAmt,
                                ReferenceNo = item.PaymentReferenceNO,
                                Type = "Gullak",
                                Orderid = item.Orderid,
                                Deliveryissueid = item.Deliveryissueid,
                                CreatedDate = item.CreatedDate,
                                PaymentFrom = item.PaymentFrom,
                            });
                        }
                    }
                    onlineCollectionDcPaggingData.total_count = OnlinePaymentDcs.Count();
                    onlineCollectionDcPaggingData.onlinePaymentDcs = OnlinePaymentDcs.OrderByDescending(x => x.Id).Skip((page - 1) * totalitem).Take(totalitem).ToList();
                    if (onlineCollectionDcPaggingData.onlinePaymentDcs != null && onlineCollectionDcPaggingData.onlinePaymentDcs.Any())
                    {
                        var orderIds = onlineCollectionDcPaggingData.onlinePaymentDcs.Select(x => x.Orderid).Distinct().ToList();
                        var orderskcode = context.DbOrderMaster.Where(x => orderIds.Contains(x.OrderId)).Select(x => new { x.Skcode, x.OrderId });
                        var orderVerify = context.PaymentResponseRetailerAppDb.Where(x => orderIds.Contains(x.OrderId) && x.status == "Success" && x.IsOnline/* (x.PaymentFrom == "mPos" || x.PaymentFrom == "hdfc" || x.PaymentFrom == "ePaylater" || x.PaymentFrom == "NEFT")*/).Select(x => new { x.IsSettled, x.OrderId }).ToList();
                        foreach (var item in onlineCollectionDcPaggingData.onlinePaymentDcs)
                        {
                            item.SkCode = orderskcode != null && orderskcode.Any(x => x.OrderId == item.Orderid) ? orderskcode.FirstOrDefault(x => x.OrderId == item.Orderid).Skcode : "";
                            item.IsSettled = orderVerify.Where(x => x.OrderId == item.Orderid).Select(x => x.IsSettled).FirstOrDefault();

                        }
                    }
                }
                return onlineCollectionDcPaggingData;
            }
            catch (Exception ex)
            {
                logger.Error("Error in GetOnlineCollectionpaging Method: " + ex.Message);
                return null;
            }
        }

        [Route("WarehouseAssignmentSettlement")]
        [HttpPost]
        public ResponseResult WarehouseAssignmentSettlement(CurrencyCollectionUpdateDc currencyCollectionUpdateDc)
        {
            ResponseResult responseResult = new ResponseResult { status = false, Message = "" };
            try
            {
                var identity = User.Identity as ClaimsIdentity;
                int compid = 0, userid = 0;

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))
                    compid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                    userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
                TransactionOptions option = new TransactionOptions();
                option.IsolationLevel = System.Transactions.IsolationLevel.RepeatableRead;
                option.Timeout = TimeSpan.FromSeconds(90);
                if (currencyCollectionIds != null && currencyCollectionIds.Any(x => x == currencyCollectionUpdateDc.currencyCollectionId))
                {
                    responseResult = new ResponseResult
                    {
                        status = false,
                        Message = "Assignment in process please wait."
                    };
                    return responseResult;
                }

                currencyCollectionIds.Add(currencyCollectionUpdateDc.currencyCollectionId);
                using (TransactionScope dbContextTransaction = new TransactionScope(TransactionScopeOption.Required, option))
                // using (var dbContextTransaction = new TransactionScope(TransactionScopeOption.RequiresNew))
                {
                    using (AuthContext context = new AuthContext())
                    {
                        string status = API.External.DeliveryAPP.CurrencyCollectionStatusEnum.Settlement.ToString();

                        var dbCurrencyCollection = context.CurrencyCollection.Where(x => x.Id == currencyCollectionUpdateDc.currencyCollectionId && x.Status != status).Include(x => x.CashCollections).Include(x => x.ChequeCollections).Include(x => x.OnlineCollections).FirstOrDefault();
                        if (dbCurrencyCollection != null)
                        {
                            if (currencyCollectionUpdateDc.status == API.External.DeliveryAPP.CurrencyCollectionStatusEnum.Settlement.ToString())
                            {

                                //string CancelOrderIds= context.Database.SqlQuery<string>("select STRING_AGG(OrderId,',') from OrderDispatchedMasters a where  DeliveryIssuanceIdOrderDeliveryMaster="+ dbCurrencyCollection.Deliveryissueid + " and Status='Delivery Canceled'").FirstOrDefault();
                                //if (!string.IsNullOrEmpty(CancelOrderIds))
                                //{
                                //    responseResult = new ResponseResult
                                //    {
                                //        status = false,
                                //        Message = "You can not settle the assignment due to order # " + CancelOrderIds + " not POC."
                                //    };

                                //    return responseResult;
                                //}

                                var dbHubCurrencyCollection = context.CurrencyHubStock.Where(x => x.Warehouseid == currencyCollectionUpdateDc.warehouseid && EntityFunctions.TruncateTime(x.BOD) == EntityFunctions.TruncateTime(DateTime.Now)).Include(x => x.HubCashCollections).FirstOrDefault();
                                if (dbHubCurrencyCollection != null)
                                {
                                    if (dbHubCurrencyCollection.EOD.HasValue)
                                    {
                                        currencyCollectionIds.RemoveAll(x => x == currencyCollectionUpdateDc.currencyCollectionId);
                                        responseResult = new ResponseResult
                                        {
                                            status = false,
                                            Message = "End of Date(EOD) is done on warehouse."
                                        };

                                        return responseResult;
                                    }
                                    else
                                    {
                                        decimal totalCash = 0, totalCheque = 0, totalOnline = 0;
                                        if (dbCurrencyCollection.CashCollections.Any())
                                        {
                                            var currencyDenomination = context.CurrencyDenomination.Where(x => x.IsActive).ToList();

                                            var cashCollections = dbCurrencyCollection.CashCollections.Where(x => x.IsActive && ((x.IsDeleted.HasValue && !x.IsDeleted.Value) || !x.IsDeleted.HasValue));
                                            totalCash = (from a in cashCollections
                                                         join b in currencyDenomination on a.CurrencyDenominationId equals b.Id
                                                         select new
                                                         {
                                                             totalcash = a.CurrencyCountByDBoy * b.Value
                                                         }).Sum(x => x.totalcash);

                                            #region Cash Collection

                                            if (dbCurrencyCollection.CashCollections != null && dbCurrencyCollection.CashCollections.Any())
                                            {
                                                foreach (var item in dbCurrencyCollection.CashCollections.Where(x => x.IsActive && ((x.IsDeleted.HasValue && !x.IsDeleted.Value) || !x.IsDeleted.HasValue)))
                                                {
                                                    item.CurrencyHubStockId = dbHubCurrencyCollection.Id;
                                                    item.ModifiedBy = userid;
                                                    item.ModifiedDate = indianTime;
                                                    //context.CashCollection.Attach(item);
                                                    context.Entry(item).State = EntityState.Modified;
                                                }
                                            }

                                            if (dbHubCurrencyCollection.HubCashCollections != null && dbHubCurrencyCollection.HubCashCollections.Any())
                                            {
                                                if (dbCurrencyCollection.CashCollections != null && dbCurrencyCollection.CashCollections.Any())
                                                {
                                                    //List<int> usedCurrencyDenominationId = new List<int>();
                                                    foreach (var Cashitem in dbHubCurrencyCollection.HubCashCollections)
                                                    {
                                                        if (dbCurrencyCollection.CashCollections.Any(x => x.CurrencyDenominationId == Cashitem.CurrencyDenominationId))
                                                        {
                                                            //usedCurrencyDenominationId.Add(Cashitem.CurrencyDenominationId);
                                                            Cashitem.CurrencyCount = Cashitem.CurrencyCount + dbCurrencyCollection.CashCollections.FirstOrDefault(x => x.CurrencyDenominationId == Cashitem.CurrencyDenominationId).CurrencyCountByDBoy;
                                                            Cashitem.ModifiedBy = userid;
                                                            Cashitem.ModifiedDate = indianTime;
                                                            Cashitem.IsActive = true;
                                                            Cashitem.IsDeleted = false;
                                                            //context.HubCashCollection.Attach(Cashitem);
                                                            context.Entry(Cashitem).State = EntityState.Modified;
                                                        }

                                                    }

                                                    if (dbHubCurrencyCollection.Deliveryissueids != null && dbHubCurrencyCollection.Deliveryissueids.Length > 0)
                                                    {
                                                        dbHubCurrencyCollection.Deliveryissueids += "," + dbCurrencyCollection.Deliveryissueid;
                                                    }
                                                    else
                                                    {
                                                        dbHubCurrencyCollection.Deliveryissueids += dbCurrencyCollection.Deliveryissueid;
                                                    }
                                                    //context.CurrencyHubStock.Attach(dbHubCurrencyCollection);
                                                    context.Entry(dbHubCurrencyCollection).State = EntityState.Modified;
                                                }
                                            }
                                            #endregion

                                        }

                                        if (dbCurrencyCollection.ChequeCollections != null && dbCurrencyCollection.ChequeCollections.Any())
                                        {
                                            totalCheque = dbCurrencyCollection.ChequeCollections.Where(x => x.ChequeStatus != Convert.ToInt32(ChequeStatusEnum.Return) && x.ChequeStatus != Convert.ToInt32(ChequeStatusEnum.Reject) && x.IsActive && ((x.IsDeleted.HasValue && !x.IsDeleted.Value) || !x.IsDeleted.HasValue)).Sum(x => x.ChequeAmt);
                                            foreach (var item in dbCurrencyCollection.ChequeCollections.Where(x => x.ChequeStatus != Convert.ToInt32(ChequeStatusEnum.Return) && x.IsActive && ((x.IsDeleted.HasValue && !x.IsDeleted.Value) || !x.IsDeleted.HasValue)))
                                            {
                                                item.CurrencyHubStockId = dbHubCurrencyCollection.Id;
                                                item.ModifiedBy = userid;
                                                item.ModifiedDate = indianTime;
                                                item.ChequeStatus = Convert.ToInt32(ChequeStatusEnum.Operation);
                                                //context.ChequeCollection.Attach(item);
                                                context.Entry(item).State = EntityState.Modified;
                                            }
                                        }
                                        if (dbCurrencyCollection.OnlineCollections != null && dbCurrencyCollection.OnlineCollections.Any())
                                        {
                                            totalOnline = dbCurrencyCollection.OnlineCollections.Where(x => x.IsActive && ((x.IsDeleted.HasValue && !x.IsDeleted.Value) || !x.IsDeleted.HasValue)).Sum(x => x.MPOSAmt + x.PaymentGetwayAmt);
                                            foreach (var item in dbCurrencyCollection.OnlineCollections.Where(x => x.IsActive && ((x.IsDeleted.HasValue && !x.IsDeleted.Value) || !x.IsDeleted.HasValue)))
                                            {
                                                item.CurrencyHubStockId = dbHubCurrencyCollection.Id;
                                                item.ModifiedBy = userid;
                                                item.ModifiedDate = indianTime;
                                                //context.OnlineCollection.Attach(item);
                                                context.Entry(item).State = EntityState.Modified;
                                            }
                                        }

                                        dbHubCurrencyCollection.TotalCashAmt = dbHubCurrencyCollection.TotalCashAmt + totalCash;
                                        dbHubCurrencyCollection.TotalCheckAmt = dbHubCurrencyCollection.TotalCheckAmt + totalCheque;
                                        dbHubCurrencyCollection.TotalOnlineAmt = dbHubCurrencyCollection.TotalOnlineAmt + totalOnline;
                                        dbHubCurrencyCollection.TotalDeliveryissueAmt = dbHubCurrencyCollection.TotalDeliveryissueAmt + dbCurrencyCollection.TotalDeliveryissueAmt;
                                        dbHubCurrencyCollection.TotalDueAmt = dbHubCurrencyCollection.TotalDueAmt + dbCurrencyCollection.TotalDueAmt;
                                        dbHubCurrencyCollection.ModifiedBy = userid;
                                        dbHubCurrencyCollection.ModifiedDate = indianTime;
                                        //context.CurrencyHubStock.Attach(dbHubCurrencyCollection);
                                        context.Entry(dbHubCurrencyCollection).State = EntityState.Modified;

                                        dbCurrencyCollection.Status = API.External.DeliveryAPP.CurrencyCollectionStatusEnum.Settlement.ToString();
                                        dbCurrencyCollection.ModifiedBy = userid;
                                        dbCurrencyCollection.ModifiedDate = indianTime;
                                        dbCurrencyCollection.WareHouseSettleDate = indianTime;
                                        context.Entry(dbCurrencyCollection).State = EntityState.Modified;

                                        if (context.Commit() > 0)
                                        {
                                            var assignment = context.DeliveryIssuanceDb.FirstOrDefault(x => x.DeliveryIssuanceId == dbCurrencyCollection.Deliveryissueid);
                                            var people = context.Peoples.FirstOrDefault(x => x.PeopleID == userid);
                                            if (assignment != null)
                                            {
                                                assignment.Status = "Freezed";
                                                OrderDeliveryMasterHistories AssginDeli = new OrderDeliveryMasterHistories();
                                                AssginDeli.DeliveryIssuanceId = assignment.DeliveryIssuanceId;
                                                AssginDeli.Cityid = assignment.Cityid;
                                                AssginDeli.city = assignment.city;
                                                AssginDeli.DisplayName = assignment.DisplayName;
                                                AssginDeli.Status = assignment.Status;
                                                AssginDeli.WarehouseId = assignment.WarehouseId;
                                                AssginDeli.PeopleID = assignment.PeopleID;
                                                AssginDeli.VehicleId = assignment.VehicleId;
                                                AssginDeli.VehicleNumber = assignment.VehicleNumber;
                                                AssginDeli.CreatedDate = indianTime;
                                                AssginDeli.UpdatedDate = indianTime;
                                                AssginDeli.userid = people.PeopleID;
                                                AssginDeli.Description = "Assignment No. : " + assignment.DeliveryIssuanceId + "Is Updates";
                                                if (people.DisplayName == null)
                                                {
                                                    AssginDeli.UpdatedBy = people.PeopleFirstName;
                                                }
                                                else
                                                {
                                                    AssginDeli.UpdatedBy = people.DisplayName;
                                                }
                                                context.OrderDeliveryMasterHistoriesDB.Add(AssginDeli);
                                                context.Entry(assignment).State = EntityState.Modified;

                                                #region AutoSettleEntry

                                                if (AppConstants.IsUsingAutoSettleOrder)
                                                {
                                                    var AssignIdparm = new SqlParameter
                                                    {
                                                        ParameterName = "Assign",
                                                        Value = assignment.DeliveryIssuanceId
                                                    };

                                                    var orderListByAssignIdDCs = context.Database.SqlQuery<OrderListByAssignIdDC>("spGetOrdersByAssignmentId @Assign ", AssignIdparm).ToList();
                                                    List<AutoSettleOrderDetail> AutoList = new List<AutoSettleOrderDetail>();
                                                    foreach (var list in orderListByAssignIdDCs)
                                                    {
                                                        AutoSettleOrderDetail autoSettleOrderDetail = new AutoSettleOrderDetail();
                                                        autoSettleOrderDetail.OrderId = list.OrderId;
                                                        autoSettleOrderDetail.IsProcess = false;
                                                        autoSettleOrderDetail.RetryCount = 0;
                                                        autoSettleOrderDetail.IsActive = true;
                                                        autoSettleOrderDetail.InsertedDate = DateTime.Now;
                                                        autoSettleOrderDetail.CustomerId = list.CustomerId;
                                                        AutoList.Add(autoSettleOrderDetail);
                                                    }
                                                    context.AutoSettleOrderDetailDB.AddRange(AutoList);
                                                }
                                                #endregion

                                                if (context.Commit() > 0)
                                                {

                                                }
                                            }

                                            #region Agent Ledger entry
                                            AgentLedgerHelper agentLedgerHelper = new AgentLedgerHelper();
                                            AngularJSAuthentication.API.External.DeliveryAPP.CurrencyCollectionDc currencyCollectionDb = new AngularJSAuthentication.API.External.DeliveryAPP.CurrencyCollectionDc
                                            {
                                                DBoyPeopleId = dbCurrencyCollection.DBoyPeopleId,
                                                Deliveryissueid = dbCurrencyCollection.Deliveryissueid,
                                                Id = dbCurrencyCollection.Id,
                                                TotalCashAmt = dbCurrencyCollection.TotalCashAmt,
                                                TotalCheckAmt = dbCurrencyCollection.TotalCheckAmt,
                                                Status = dbCurrencyCollection.Status,
                                                TotalOnlineAmt = dbCurrencyCollection.TotalOnlineAmt,
                                                Warehouseid = dbCurrencyCollection.Warehouseid,
                                                TotalDeliveryissueAmt = dbCurrencyCollection.TotalDeliveryissueAmt,
                                            };

                                            agentLedgerHelper.UpdateAgentCommission(assignment.DeliveryIssuanceId);
                                            BackgroundJob.Schedule(() => agentLedgerHelper.OnPaymentAccept(assignment, userid, currencyCollectionDb), TimeSpan.FromMilliseconds(150));

                                            BackgroundJob.Schedule(() => agentLedgerHelper.OnGetCommisiondata(assignment, userid, currencyCollectionDb), TimeSpan.FromMilliseconds(150));

                                            #endregion
                                            #region  Agent Entry for Assignment Freezed status  for order cancel and Redispatch order 

                                            BackgroundJob.Schedule(() => agentLedgerHelper.Onordercancelandredispatch(assignment, userid), TimeSpan.FromMilliseconds(150));

                                            #endregion

                                            responseResult = new ResponseResult
                                            {
                                                status = true,
                                                Message = "Assignment " + API.External.DeliveryAPP.CurrencyCollectionStatusEnum.Settlement.ToString() + " successfully."
                                            };
                                            dbContextTransaction.Complete();
                                            currencyCollectionIds.RemoveAll(x => x == currencyCollectionUpdateDc.currencyCollectionId);
                                            return responseResult;
                                        }
                                        else
                                        {
                                            currencyCollectionIds.RemoveAll(x => x == currencyCollectionUpdateDc.currencyCollectionId);
                                            responseResult = new ResponseResult
                                            {
                                                status = true,
                                                Message = "Some error occurred during " + API.External.DeliveryAPP.CurrencyCollectionStatusEnum.Settlement.ToString() + " assignment."
                                            };
                                            return responseResult;

                                        }
                                    }
                                }
                                else
                                {
                                    responseResult = new ResponseResult
                                    {
                                        status = false,
                                        Message = "Beigning of Date(BOD) not started on warehouse."
                                    };
                                    currencyCollectionIds.RemoveAll(x => x == currencyCollectionUpdateDc.currencyCollectionId);
                                    return responseResult;

                                }
                            }
                            else if (currencyCollectionUpdateDc.status == API.External.DeliveryAPP.CurrencyCollectionStatusEnum.Decline.ToString())
                            {
                                dbCurrencyCollection.Status = API.External.DeliveryAPP.CurrencyCollectionStatusEnum.Decline.ToString();
                                dbCurrencyCollection.DeclineNote = API.External.DeliveryAPP.CurrencyCollectionStatusEnum.Decline.ToString();
                                dbCurrencyCollection.ModifiedBy = userid;
                                dbCurrencyCollection.ModifiedDate = indianTime;
                                context.Entry(dbCurrencyCollection).State = EntityState.Modified;
                                if (context.Commit() > 0)
                                {
                                    responseResult = new ResponseResult
                                    {
                                        status = true,
                                        Message = "Assignment " + API.External.DeliveryAPP.CurrencyCollectionStatusEnum.Decline.ToString() + " successfully."
                                    };
                                    dbContextTransaction.Complete();
                                    currencyCollectionIds.RemoveAll(x => x == currencyCollectionUpdateDc.currencyCollectionId);
                                    return responseResult;
                                }
                                else
                                {
                                    currencyCollectionIds.RemoveAll(x => x == currencyCollectionUpdateDc.currencyCollectionId);
                                    responseResult = new ResponseResult
                                    {
                                        status = true,
                                        Message = "Some error occurred during " + API.External.DeliveryAPP.CurrencyCollectionStatusEnum.Decline.ToString() + " assignment."
                                    };
                                    return responseResult;
                                }
                            }
                        }
                        else
                        {
                            currencyCollectionIds.RemoveAll(x => x == currencyCollectionUpdateDc.currencyCollectionId);
                            responseResult = new ResponseResult
                            {
                                status = false,
                                Message = "Assignment Data not found."
                            };
                            return responseResult;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error("Error in WarehouseAssignmentSettlement Method: " + ex.Message);
                currencyCollectionIds.RemoveAll(x => x == currencyCollectionUpdateDc.currencyCollectionId);
                responseResult = new ResponseResult
                {
                    status = false,
                    Message = "Some error occurred during process assignment."
                };

            }
            return responseResult;
        }
        #endregion


        #region Warehouse Live deschboard screen method
        [Route("GetLoginPeopleWarehouseId")]
        [HttpGet]
        public int GetLoginPeopleWarehouseId()
        {
            int Warehouse_id = 0;
            try
            {
                var identity = User.Identity as ClaimsIdentity;
                int userid = 0;

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "Warehouseid"))
                    Warehouse_id = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "Warehouseid").Value);

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                    userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

                //Warehouse_id = 1;
            }
            catch (Exception ex)
            {
                logger.Error("Error in GetLoginPeopleWarehouseId Method: " + ex.Message);
                Warehouse_id = 0;
            }
            return Warehouse_id;
        }
        [Route("LiveWarehouseCashDashboard")]
        [HttpGet]
        public LiveWarehouseCashDc GetLiveWarehouseCashDashboard(int warehouseid)
        {
            LiveWarehouseCashDc liveWarehouseCashDc = new LiveWarehouseCashDc();
            if (warehouseid > 0)
            {
                try
                {
                    using (AuthContext context = new AuthContext())
                    {

                        var TodayHubCurrencyCollection = context.CurrencyHubStock.FirstOrDefault(x => x.Warehouseid == warehouseid
                                                                                       && EntityFunctions.TruncateTime(x.BOD) == EntityFunctions.TruncateTime(DateTime.Now));
                        List<hubCashCollectionDc> WarehouseOpeningCash = new List<hubCashCollectionDc>();
                        long CurrencyHubStockId = 0;
                        if (TodayHubCurrencyCollection != null)
                        {
                            long yesterdayhubcollectionid = TodayHubCurrencyCollection.Id;
                            CurrencyHubStockId = yesterdayhubcollectionid;
                            WarehouseOpeningCash = (
                               from c in context.CurrencyDenomination.Where(x => x.IsActive)
                               join p in context.HubCashCollection.Where(o => o.CurrencyHubStockId == yesterdayhubcollectionid && o.IsActive
                               && ((o.IsDeleted.HasValue && !o.IsDeleted.Value) || !o.IsDeleted.HasValue))
                               on c.Id equals p.CurrencyDenominationId into ps
                               from p in ps.DefaultIfEmpty()
                               select new hubCashCollectionDc
                               {
                                   CurrencyDenominationId = c == null ? 0 : c.Id,
                                   CurrencyCount = p == null ? 0 : (p.OpeningCurrencyCount),
                                   CurrencyDenominationTitle = c == null ? "" : c.Title,
                                   CurrencyDenominationValue = c == null ? 0 : c.Value,
                                   Id = p == null ? 0 : p.Id,
                                   CashCurrencyType = c == null ? "" : c.currencyType,
                               }).ToList();
                        }
                        else
                        {
                            WarehouseOpeningCash = (from c in context.CurrencyDenomination.Where(x => x.IsActive)
                                                    select new hubCashCollectionDc
                                                    {
                                                        CurrencyDenominationId = c == null ? 0 : c.Id,
                                                        CurrencyCount = 0,
                                                        CurrencyDenominationTitle = c == null ? "" : c.Title,
                                                        CurrencyDenominationValue = c == null ? 0 : c.Value,
                                                        CashCurrencyType = c == null ? "" : c.currencyType,
                                                    }).ToList();
                        }
                        liveWarehouseCashDc.WarehouseOpeningCash = new List<hubCashCollectionDc>();
                        liveWarehouseCashDc.WarehouseOpeningCash = WarehouseOpeningCash;


                        List<hubCashCollectionDc> WarehouseTodayCash = new List<hubCashCollectionDc>();
                        if (TodayHubCurrencyCollection != null)
                        {
                            var todayhubcollection = TodayHubCurrencyCollection;
                            WarehouseTodayCash = (
                               from c in context.CurrencyDenomination.Where(x => x.IsActive)
                               join p in context.HubCashCollection.Where(o => o.CurrencyHubStockId == todayhubcollection.Id && o.IsActive
                               && ((o.IsDeleted.HasValue && !o.IsDeleted.Value) || !o.IsDeleted.HasValue))
                               on c.Id equals p.CurrencyDenominationId into ps
                               from p in ps.DefaultIfEmpty()
                               select new hubCashCollectionDc
                               {
                                   CurrencyDenominationId = c == null ? 0 : c.Id,
                                   CurrencyCount = p == null ? 0 : p.CurrencyCount,
                                   CurrencyDenominationTitle = c == null ? "" : c.Title,
                                   CurrencyDenominationValue = c == null ? 0 : c.Value,
                                   BankDepositCurrencyCount = p == null ? 0 : p.BankSendCurrencyCount,
                                   ExchangeInCurrencyCount = p == null ? 0 : p.ExchangeCurrencyCount,
                                   Id = p == null ? 0 : p.Id,
                                   CashCurrencyType = c == null ? "" : c.currencyType,
                               }).ToList();
                            liveWarehouseCashDc.BOD = todayhubcollection.BOD;
                            liveWarehouseCashDc.CurrencyHubStockId = todayhubcollection.Id;
                            liveWarehouseCashDc.EOD = todayhubcollection.EOD;
                            liveWarehouseCashDc.IsBOD = true;
                            liveWarehouseCashDc.IsEOD = liveWarehouseCashDc.EOD.HasValue ? true : false;
                        }
                        else
                        {
                            WarehouseTodayCash = (from c in context.CurrencyDenomination.Where(x => x.IsActive)
                                                  select new hubCashCollectionDc
                                                  {
                                                      CurrencyDenominationId = c == null ? 0 : c.Id,
                                                      CurrencyCount = 0,
                                                      CurrencyDenominationTitle = c == null ? "" : c.Title,
                                                      CurrencyDenominationValue = c == null ? 0 : c.Value,
                                                      CashCurrencyType = c == null ? "" : c.currencyType,
                                                  }).ToList();
                            liveWarehouseCashDc.IsBOD = false;
                            liveWarehouseCashDc.IsEOD = false;
                        }
                        liveWarehouseCashDc.WarehouseTodayCash = new List<hubCashCollectionDc>();
                        liveWarehouseCashDc.WarehouseTodayCash = WarehouseTodayCash;
                        List<hubCashCollectionDc> WarehouseTodayClosingCash = new List<hubCashCollectionDc>();

                        WarehouseTodayClosingCash = WarehouseTodayCash.Union(WarehouseOpeningCash).GroupBy(x => new { x.CurrencyDenominationId, x.CurrencyDenominationTitle, x.CurrencyDenominationValue, x.CashCurrencyType })
                                                    .Select(x => new hubCashCollectionDc
                                                    {
                                                        CurrencyDenominationId = x.Key.CurrencyDenominationId,
                                                        CurrencyCount = x.Sum(y => y.CurrencyCount) - x.Sum(p => p.BankDepositCurrencyCount) + x.Sum(p => p.ExchangeInCurrencyCount),
                                                        CurrencyDenominationTitle = x.Key.CurrencyDenominationTitle,
                                                        CurrencyDenominationValue = x.Key.CurrencyDenominationValue,
                                                        CashCurrencyType = x.Key.CashCurrencyType,
                                                    }).ToList();

                        liveWarehouseCashDc.WarehouseClosingCash = WarehouseTodayClosingCash;

                        if (liveWarehouseCashDc.WarehouseTodayCash != null && liveWarehouseCashDc.WarehouseTodayCash.Count() > 0 && CurrencyHubStockId > 0)
                        {
                            var ExchangeHubCashCollections = context.ExchangeHubCashCollection.Where(o => o.CurrencyHubStockId == CurrencyHubStockId && o.IsActive
                                && ((o.IsDeleted.HasValue && !o.IsDeleted.Value) || !o.IsDeleted.HasValue));

                            liveWarehouseCashDc.Deliveryissueids = context.CurrencyHubStock.Where(x => x.Id == CurrencyHubStockId).Select(x => x.Deliveryissueids).FirstOrDefault();
                            foreach (var item in liveWarehouseCashDc.WarehouseTodayCash)
                            {
                                if (ExchangeHubCashCollections != null && ExchangeHubCashCollections.Any(x => x.CurrencyDenominationId == item.CurrencyDenominationId))
                                {
                                    item.ExchangeInCurrencyCount = ExchangeHubCashCollections.Any(x => x.CurrencyDenominationId == item.CurrencyDenominationId && x.CurrencyCount > 0) ? ExchangeHubCashCollections.Where(x => x.CurrencyDenominationId == item.CurrencyDenominationId && x.CurrencyCount > 0).Sum(x => x.CurrencyCount) : 0;
                                    item.ExchangeOutCurrencyCount = ExchangeHubCashCollections.Any(x => x.CurrencyDenominationId == item.CurrencyDenominationId && x.CurrencyCount < 0) ? ExchangeHubCashCollections.Where(x => x.CurrencyDenominationId == item.CurrencyDenominationId && x.CurrencyCount < 0).Sum(x => x.CurrencyCount) : 0;

                                }
                            }
                        }
                    }


                }
                catch (Exception ex)
                {
                    logger.Error("Error in GetLiveWarehouseCashDeshboard Method: " + ex.Message);
                    return null;
                }
            }
            return liveWarehouseCashDc;
        }


        [Route("LiveCasherWarehouseCashDashboard")]
        [HttpGet]
        public LiveCasherWarehouseCashDc LiveCasherWarehouseCashDashboard(int warehouseid, DateTime Inputdate)
        {
            var identity = User.Identity as ClaimsIdentity;
            int userid = 0;
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

            LiveCasherWarehouseCashDc liveWarehouseCashDc = new LiveCasherWarehouseCashDc();
            if (warehouseid > 0)
            {
                try
                {
                    using (AuthContext context = new AuthContext())
                    {
                        var date = DateTime.Now;
                        var TodayHubCurrencyCollection = context.CurrencyHubStock.FirstOrDefault(x => x.Warehouseid == warehouseid
                                                                                       && EntityFunctions.TruncateTime(x.BOD) == EntityFunctions.TruncateTime(date));
                        List<hubCashCollectionDc> WarehouseOpeningCash = new List<hubCashCollectionDc>();

                        List<hubCashCollectionDc> WarehouseTodayCash = new List<hubCashCollectionDc>();

                        if (TodayHubCurrencyCollection != null)
                        {
                            var todayhubcollection = TodayHubCurrencyCollection;
                            WarehouseTodayCash = (
                               from c in context.CurrencyDenomination.Where(x => x.IsActive)
                               join p in context.CashCollection.Where(o => o.CurrencyHubStockId == todayhubcollection.Id && o.IsActive
                               && ((o.IsDeleted.HasValue && !o.IsDeleted.Value) || !o.IsDeleted.HasValue))
                               on c.Id equals p.CurrencyDenominationId into ps
                               from p in ps.DefaultIfEmpty()
                               select new hubCashCollectionDc
                               {
                                   CurrencyDenominationId = c == null ? 0 : c.Id,
                                   CurrencyCount = p == null ? 0 : p.CurrencyCountByDBoy,
                                   CurrencyDenominationTitle = c == null ? "" : c.Title,
                                   CurrencyDenominationValue = c == null ? 0 : c.Value,
                                   BankDepositCurrencyCount = 0,
                                   ExchangeInCurrencyCount = 0,
                                   Id = p == null ? 0 : p.Id,
                                   CashCurrencyType = c == null ? "" : c.currencyType,
                                   WarehousePeopleId = p == null ? 0 : p.ModifiedBy,
                               }).ToList();
                            liveWarehouseCashDc.BOD = todayhubcollection.BOD;
                            liveWarehouseCashDc.CurrencyHubStockId = todayhubcollection.Id;
                            liveWarehouseCashDc.EOD = todayhubcollection.EOD;
                            liveWarehouseCashDc.IsBOD = true;
                            liveWarehouseCashDc.IsEOD = liveWarehouseCashDc.EOD.HasValue ? true : false;
                        }
                        else
                        {
                            WarehouseTodayCash = (from c in context.CurrencyDenomination.Where(x => x.IsActive)
                                                  select new hubCashCollectionDc
                                                  {
                                                      CurrencyDenominationId = c == null ? 0 : c.Id,
                                                      CurrencyCount = 0,
                                                      CurrencyDenominationTitle = c == null ? "" : c.Title,
                                                      CurrencyDenominationValue = c == null ? 0 : c.Value,
                                                      CashCurrencyType = c == null ? "" : c.currencyType,
                                                  }).ToList();
                            liveWarehouseCashDc.IsBOD = false;
                            liveWarehouseCashDc.IsEOD = false;
                        }

                        var PeopleIds = WarehouseTodayCash.Select(x => x.WarehousePeopleId).Distinct().ToList();
                        var PeopleList = context.Peoples.Where(x => PeopleIds.Contains(x.PeopleID) && x.Active == true && x.Deleted == false).Select(x => new PeopleListDc
                        {
                            PeopleID = x.PeopleID,
                            DisplayName = x.DisplayName
                        }).ToList();

                        #region ChequeInformation
                        liveWarehouseCashDc.WarehouseTodayCash = new List<hubCashCollectionDc>();
                        var chequeList = context.ChequeCollection.Where(x => x.CurrencyHubStockId == TodayHubCurrencyCollection.Id && EntityFunctions.TruncateTime(x.ModifiedDate) == EntityFunctions.TruncateTime(date) && x.IsActive == true && x.IsDeleted == false).Select(x => new CheuqueCollectionNewDc
                        {
                            ModifiedBy = x.ModifiedBy,
                            OrderId = x.Orderid,
                            ChequeAmt = x.ChequeAmt
                        }).ToList();
                        if (chequeList != null && chequeList.Any())
                        {
                            liveWarehouseCashDc.WarehouseTotalTodayChequeCount = chequeList.Where(x => x.ModifiedBy == userid).Count();
                            liveWarehouseCashDc.WarehouseTotalChequeAmount = chequeList.Where(x => x.ModifiedBy == userid).Sum(x => x.ChequeAmt);
                        }
                        #endregion


                        if (WarehouseTodayCash.Any(x => x.WarehousePeopleId == userid))
                        {
                            liveWarehouseCashDc.WarehouseTodayCash = WarehouseTodayCash.Where(x => x.WarehousePeopleId == userid).GroupBy(x => x.CurrencyDenominationId).Select(x => new hubCashCollectionDc
                            {
                                CurrencyDenominationId = x.Key,
                                CurrencyCount = x.Sum(y => y.CurrencyCount),
                                CurrencyDenominationTitle = x.Max(y => y.CurrencyDenominationTitle),
                                CurrencyDenominationValue = x.Max(y => y.CurrencyDenominationValue),
                                BankDepositCurrencyCount = 0,
                                ExchangeInCurrencyCount = 0,
                                Id = x.Max(y => y.Id),
                                CashCurrencyType = x.Max(y => y.CashCurrencyType),
                                WarehousePeopleId = userid,
                            }).ToList();

                            liveWarehouseCashDc.CasherPeopleName = PeopleList.Where(y => y.PeopleID == userid).Select(y => y.DisplayName).FirstOrDefault();
                        }
                        else
                        {
                            liveWarehouseCashDc.WarehouseTodayCash = (from c in context.CurrencyDenomination.Where(x => x.IsActive)
                                                                      select new hubCashCollectionDc
                                                                      {
                                                                          CurrencyDenominationId = c == null ? 0 : c.Id,
                                                                          CurrencyCount = 0,
                                                                          CurrencyDenominationTitle = c == null ? "" : c.Title,
                                                                          CurrencyDenominationValue = c == null ? 0 : c.Value,
                                                                          BankDepositCurrencyCount = 0,
                                                                          ExchangeInCurrencyCount = 0,
                                                                          Id = 0,
                                                                          CashCurrencyType = c == null ? "" : c.currencyType,
                                                                          WarehousePeopleId = userid
                                                                      }).ToList();

                            liveWarehouseCashDc.CasherPeopleName = "";
                        }


                        liveWarehouseCashDc.OtherCasherDataDcs = WarehouseTodayCash.Where(x => x.WarehousePeopleId != userid).GroupBy(x => x.WarehousePeopleId).Select(p =>
                             new OtherCasherDataDc
                             {
                                 OtherPeopleId = p.Key,
                                 OtheCasherPeopleName = PeopleList.Where(x => x.PeopleID == p.Key).Select(x => x.DisplayName).FirstOrDefault(),

                                 #region ChequeInformation
                                 OtherCasherTotalTodayChequeCount = chequeList.Where(x => x.ModifiedBy == p.Key).Count(),
                                 OtherCasherTotalChequeAmount = chequeList.Where(x => x.ModifiedBy == p.Key).Sum(x => x.ChequeAmt),
                                 #endregion

                                 //WarehouseTodayCash = p.Where(x => x.WarehousePeopleId != userid).GroupBy(x => x.CurrencyDenominationId).Select(x => new hubCashCollectionDc
                                 WarehouseTodayCash = p.GroupBy(x => x.CurrencyDenominationId).Select(x => new hubCashCollectionDc
                                 {
                                     CurrencyDenominationId = x.Key,
                                     CurrencyCount = x.Sum(y => y.CurrencyCount),
                                     CurrencyDenominationTitle = x.Max(y => y.CurrencyDenominationTitle),
                                     CurrencyDenominationValue = x.Max(y => y.CurrencyDenominationValue),
                                     BankDepositCurrencyCount = 0,
                                     ExchangeInCurrencyCount = 0,
                                     Id = x.Max(y => y.Id),
                                     CashCurrencyType = x.Max(y => y.CashCurrencyType),
                                     WarehousePeopleId = x.Max(y => y.WarehousePeopleId),
                                 }).ToList()

                             }).ToList();
                        if (liveWarehouseCashDc.OtherCasherDataDcs == null && !liveWarehouseCashDc.OtherCasherDataDcs.Any())
                        {
                            string query = "select top 1 a.PeopleId from CurrencyVerifications a with(nolock) inner join People p with(nolock) on a.PeopleId=p.PeopleID and cast(a.CreatedDate as date)=cast(getdate() as date) and a.PeopleId!=" + userid;
                            int? otherPeopleId = context.Database.SqlQuery<int>(query).FirstOrDefault();
                            if (otherPeopleId.HasValue)
                            {
                                liveWarehouseCashDc.WarehouseTodayCash = (from c in context.CurrencyDenomination.Where(x => x.IsActive)
                                                                          select new hubCashCollectionDc
                                                                          {
                                                                              CurrencyDenominationId = c == null ? 0 : c.Id,
                                                                              CurrencyCount = 0,
                                                                              CurrencyDenominationTitle = c == null ? "" : c.Title,
                                                                              CurrencyDenominationValue = c == null ? 0 : c.Value,
                                                                              BankDepositCurrencyCount = 0,
                                                                              ExchangeInCurrencyCount = 0,
                                                                              Id = 0,
                                                                              CashCurrencyType = c == null ? "" : c.currencyType,
                                                                              WarehousePeopleId = otherPeopleId
                                                                          }).ToList();
                            }
                        }
                        var loginUserSubmittedAmount = context.CMSCashierVerificationDB.Where(x => x.WarehouseId == warehouseid && x.RequestPeopleID == userid && EntityFunctions.TruncateTime(x.CreatedDate) == EntityFunctions.TruncateTime(date) && x.VerifiePeopleID != 0).Select(x => new { x.TotalAmtCash, x.TotalAmtcheque }).ToList();
                        liveWarehouseCashDc.TotalOpeingamount = Convert.ToDecimal(loginUserSubmittedAmount != null && loginUserSubmittedAmount.Any() ? loginUserSubmittedAmount.Sum(x => x.TotalAmtCash) : 0);
                        liveWarehouseCashDc.TotalSubmittedChequeamount = Convert.ToDecimal(loginUserSubmittedAmount != null && loginUserSubmittedAmount.Any() ? loginUserSubmittedAmount.Sum(x => x.TotalAmtcheque) : 0);
                    }

                }
                catch (Exception ex)
                {
                    logger.Error("Error in LiveCasherWarehouseCashDashboard Method: " + ex.Message);
                    return null;
                }
            }
            return liveWarehouseCashDc;
        }

        //for search by warehouse and date
        [Route("WarehouseCashHistory")]
        [HttpGet]
        public LiveWarehouseCashDc WarehouseCashHistory(int warehouseid, DateTime Filterdate)
        {
            LiveWarehouseCashDc liveWarehouseCashDc = new LiveWarehouseCashDc();
            if (warehouseid > 0)
            {
                try
                {
                    using (AuthContext context = new AuthContext())
                    {
                        long CurrencyHubStockId = 0;
                        var TodayHubCurrencyCollection = context.CurrencyHubStock.FirstOrDefault(x => x.Warehouseid == warehouseid
                                                                                       && EntityFunctions.TruncateTime(x.BOD) == EntityFunctions.TruncateTime(Filterdate));
                        List<hubCashCollectionDc> WarehouseOpeningCash = new List<hubCashCollectionDc>();
                        if (TodayHubCurrencyCollection != null)
                        {
                            long yesterdayhubcollectionid = TodayHubCurrencyCollection.Id;
                            CurrencyHubStockId = yesterdayhubcollectionid;
                            WarehouseOpeningCash = (
                               from c in context.CurrencyDenomination.Where(x => x.IsActive)
                               join p in context.HubCashCollection.Where(o => o.CurrencyHubStockId == yesterdayhubcollectionid && o.IsActive
                               && ((o.IsDeleted.HasValue && !o.IsDeleted.Value) || !o.IsDeleted.HasValue))
                               on c.Id equals p.CurrencyDenominationId into ps
                               from p in ps.DefaultIfEmpty()
                               select new hubCashCollectionDc
                               {
                                   CurrencyDenominationId = c == null ? 0 : c.Id,
                                   CurrencyCount = p == null ? 0 : (p.OpeningCurrencyCount),
                                   CurrencyDenominationTitle = c == null ? "" : c.Title,
                                   CurrencyDenominationValue = c == null ? 0 : c.Value,
                                   Id = p == null ? 0 : p.Id,
                                   CashCurrencyType = c == null ? "" : c.currencyType,
                               }).ToList();
                        }
                        else
                        {
                            WarehouseOpeningCash = (from c in context.CurrencyDenomination.Where(x => x.IsActive)
                                                    select new hubCashCollectionDc
                                                    {
                                                        CurrencyDenominationId = c == null ? 0 : c.Id,
                                                        CurrencyCount = 0,
                                                        CurrencyDenominationTitle = c == null ? "" : c.Title,
                                                        CurrencyDenominationValue = c == null ? 0 : c.Value,
                                                        CashCurrencyType = c == null ? "" : c.currencyType,
                                                    }).ToList();
                        }
                        liveWarehouseCashDc.WarehouseOpeningCash = new List<hubCashCollectionDc>();
                        liveWarehouseCashDc.WarehouseOpeningCash = WarehouseOpeningCash;


                        List<hubCashCollectionDc> WarehouseTodayCash = new List<hubCashCollectionDc>();
                        if (TodayHubCurrencyCollection != null)
                        {
                            var todayhubcollection = TodayHubCurrencyCollection;
                            WarehouseTodayCash = (
                               from c in context.CurrencyDenomination.Where(x => x.IsActive)
                               join p in context.HubCashCollection.Where(o => o.CurrencyHubStockId == todayhubcollection.Id && o.IsActive
                               && ((o.IsDeleted.HasValue && !o.IsDeleted.Value) || !o.IsDeleted.HasValue))
                               on c.Id equals p.CurrencyDenominationId into ps
                               from p in ps.DefaultIfEmpty()
                               select new hubCashCollectionDc
                               {
                                   CurrencyDenominationId = c == null ? 0 : c.Id,
                                   CurrencyCount = p == null ? 0 : p.CurrencyCount,
                                   CurrencyDenominationTitle = c == null ? "" : c.Title,
                                   CurrencyDenominationValue = c == null ? 0 : c.Value,
                                   BankDepositCurrencyCount = p == null ? 0 : p.BankSendCurrencyCount,
                                   ExchangeInCurrencyCount = p == null ? 0 : p.ExchangeCurrencyCount,
                                   Id = p == null ? 0 : p.Id,
                                   CashCurrencyType = c == null ? "" : c.currencyType,
                               }).ToList();
                            liveWarehouseCashDc.BOD = todayhubcollection.BOD;
                            liveWarehouseCashDc.CurrencyHubStockId = todayhubcollection.Id;
                            liveWarehouseCashDc.EOD = todayhubcollection.EOD;
                            liveWarehouseCashDc.IsBOD = true;
                            liveWarehouseCashDc.IsEOD = liveWarehouseCashDc.EOD.HasValue ? true : false;
                        }
                        else
                        {
                            WarehouseTodayCash = (from c in context.CurrencyDenomination.Where(x => x.IsActive)
                                                  select new hubCashCollectionDc
                                                  {
                                                      CurrencyDenominationId = c == null ? 0 : c.Id,
                                                      CurrencyCount = 0,
                                                      CurrencyDenominationTitle = c == null ? "" : c.Title,
                                                      CurrencyDenominationValue = c == null ? 0 : c.Value,
                                                      CashCurrencyType = c == null ? "" : c.currencyType,
                                                  }).ToList();
                            liveWarehouseCashDc.IsBOD = false;
                            liveWarehouseCashDc.IsEOD = false;
                        }
                        liveWarehouseCashDc.WarehouseTodayCash = new List<hubCashCollectionDc>();
                        liveWarehouseCashDc.WarehouseTodayCash = WarehouseTodayCash;
                        List<hubCashCollectionDc> WarehouseTodayClosingCash = new List<hubCashCollectionDc>();

                        WarehouseTodayClosingCash = WarehouseTodayCash.Union(WarehouseOpeningCash).GroupBy(x => new { x.CurrencyDenominationId, x.CurrencyDenominationTitle, x.CurrencyDenominationValue, x.CashCurrencyType })
                                                    .Select(x => new hubCashCollectionDc
                                                    {
                                                        CurrencyDenominationId = x.Key.CurrencyDenominationId,
                                                        CurrencyCount = x.Sum(y => y.CurrencyCount) - x.Sum(p => p.BankDepositCurrencyCount) + x.Sum(p => p.ExchangeInCurrencyCount),
                                                        CurrencyDenominationTitle = x.Key.CurrencyDenominationTitle,
                                                        CurrencyDenominationValue = x.Key.CurrencyDenominationValue,
                                                        CashCurrencyType = x.Key.CashCurrencyType,
                                                    }).ToList();

                        liveWarehouseCashDc.WarehouseClosingCash = WarehouseTodayClosingCash;

                        if (liveWarehouseCashDc.WarehouseTodayCash != null && liveWarehouseCashDc.WarehouseTodayCash.Count() > 0 && CurrencyHubStockId > 0)
                        {
                            var ExchangeHubCashCollections = context.ExchangeHubCashCollection.Where(o => o.CurrencyHubStockId == CurrencyHubStockId && o.IsActive
                                && ((o.IsDeleted.HasValue && !o.IsDeleted.Value) || !o.IsDeleted.HasValue));
                            foreach (var item in liveWarehouseCashDc.WarehouseTodayCash)
                            {
                                if (ExchangeHubCashCollections != null && ExchangeHubCashCollections.Any(x => x.CurrencyDenominationId == item.CurrencyDenominationId))
                                {
                                    item.ExchangeInCurrencyCount = ExchangeHubCashCollections.Any(x => x.CurrencyDenominationId == item.CurrencyDenominationId && x.CurrencyCount > 0) ? ExchangeHubCashCollections.Where(x => x.CurrencyDenominationId == item.CurrencyDenominationId && x.CurrencyCount > 0).Sum(x => x.CurrencyCount) : 0;
                                    item.ExchangeOutCurrencyCount = ExchangeHubCashCollections.Any(x => x.CurrencyDenominationId == item.CurrencyDenominationId && x.CurrencyCount < 0) ? ExchangeHubCashCollections.Where(x => x.CurrencyDenominationId == item.CurrencyDenominationId && x.CurrencyCount < 0).Sum(x => x.CurrencyCount) : 0;
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    logger.Error("Error in GetLiveWarehouseCashDeshboard Method: " + ex.Message);
                    return null;
                }
            }
            return liveWarehouseCashDc;
        }

        [Route("WarehouseDayStartStop")]
        [HttpGet]
        public ResponseResult WarehouseDayStartStop(int warehouseid, bool IsBOD)
        {
            ResponseResult responseResult = new ResponseResult { status = false };
            if (warehouseid > 0)
            {
                var identity = User.Identity as ClaimsIdentity;
                int compid = 0, userid = 0;

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))
                    compid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                    userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
                try
                {
                    using (AuthContext context = new AuthContext())
                    {
                        var TodayHubCurrencyCollection = context.CurrencyHubStock.FirstOrDefault(x => x.Warehouseid == warehouseid && EntityFunctions.TruncateTime(x.BOD) == EntityFunctions.TruncateTime(DateTime.Now));
                        if (TodayHubCurrencyCollection == null)
                        {
                            if (IsBOD)
                            {
                                var yesterdaydate = DateTime.Now.AddDays(-1);
                                var YesterdayHubCurrencyCollections = context.CurrencyHubStock.Where(x => x.Warehouseid == warehouseid
                                                                                     && (EntityFunctions.TruncateTime(x.BOD) == EntityFunctions.TruncateTime(yesterdaydate) || x.Status != "ClosingTransfer")).Include(x => x.HubCashCollections).Include(x => x.ChequeCollections).ToList();


                                CurrencyHubStock currencyHubStock = new CurrencyHubStock
                                {
                                    BOD = DateTime.Now,
                                    CreatedBy = userid,
                                    CreatedDate = indianTime,
                                    IsActive = true,
                                    IsDeleted = false,
                                    TotalCashAmt = 0,
                                    TotalCheckAmt = 0,
                                    TotalDeliveryissueAmt = 0,
                                    TotalOnlineAmt = 0,
                                    TotalDueAmt = 0,
                                    Status = "StartDay",
                                    Warehouseid = warehouseid
                                };


                                currencyHubStock.HubCashCollections = new List<HubCashCollection>();
                                if (YesterdayHubCurrencyCollections != null && YesterdayHubCurrencyCollections.Any())
                                {
                                    foreach (var YesterdayHubCurrencyCollection in YesterdayHubCurrencyCollections)
                                    {
                                        if (YesterdayHubCurrencyCollection != null && YesterdayHubCurrencyCollection.HubCashCollections != null && YesterdayHubCurrencyCollection.HubCashCollections.Any())
                                        {
                                            foreach (var item in YesterdayHubCurrencyCollection.HubCashCollections.Where(x => x.IsActive && ((x.IsDeleted.HasValue && !x.IsDeleted.Value) || !x.IsDeleted.HasValue)))
                                            {
                                                if (!currencyHubStock.HubCashCollections.Any(x => x.CurrencyDenominationId == item.CurrencyDenominationId))
                                                {
                                                    HubCashCollection hubCashCollection = new HubCashCollection();
                                                    hubCashCollection.OpeningCurrencyCount = item.OpeningCurrencyCount + item.CurrencyCount - item.BankSendCurrencyCount + (item.ExchangeCurrencyCount);
                                                    hubCashCollection.CurrencyCount = 0;
                                                    hubCashCollection.CurrencyDenominationId = item.CurrencyDenominationId;
                                                    hubCashCollection.BankSendCurrencyCount = 0;
                                                    hubCashCollection.CreatedBy = userid;
                                                    hubCashCollection.CreatedDate = indianTime;
                                                    hubCashCollection.IsActive = true;
                                                    hubCashCollection.IsDeleted = false;
                                                    context.HubCashCollection.Add(hubCashCollection);
                                                    currencyHubStock.HubCashCollections.Add(hubCashCollection);
                                                }
                                                else
                                                {
                                                    var addCashCollection = currencyHubStock.HubCashCollections.FirstOrDefault(x => x.CurrencyDenominationId == item.CurrencyDenominationId);
                                                    addCashCollection.OpeningCurrencyCount = addCashCollection.OpeningCurrencyCount + item.OpeningCurrencyCount + item.CurrencyCount - item.BankSendCurrencyCount + (item.ExchangeCurrencyCount);
                                                }
                                            }

                                            //var totalCash = (from a in currencyHubStock.HubCashCollections
                                            //                 join b in context.CurrencyDenomination.Where(x=>x.IsActive) on a.CurrencyDenominationId equals b.Id
                                            //                 select new
                                            //                 {
                                            //                     totalcash = a.OpeningCurrencyCount * b.Value
                                            //                 }).Sum(x => x.totalcash);
                                            //var totaldue = YesterdayHubCurrencyCollection.TotalDueAmt;
                                            //currencyHubStock.TotalCashAmt = currencyHubStock.TotalCashAmt + totalCash;
                                            //currencyHubStock.TotalDueAmt = currencyHubStock.TotalDueAmt + totaldue;
                                            YesterdayHubCurrencyCollection.Status = "ClosingTransfer";
                                        }

                                        if (YesterdayHubCurrencyCollection.ChequeCollections.Any(x => x.ChequeStatus == Convert.ToInt32(ChequeStatusEnum.Operation)))
                                        {
                                            currencyHubStock.ChequeCollections = new List<ChequeCollection>();
                                            foreach (var item in YesterdayHubCurrencyCollection.ChequeCollections)
                                            {
                                                currencyHubStock.ChequeCollections.Add(item);
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    var denominations = context.CurrencyDenomination.Where(x => x.IsActive).ToList();
                                    var hubCashCollections = denominations.Select(x =>
                                               new HubCashCollection
                                               {
                                                   OpeningCurrencyCount = 0,
                                                   CurrencyCount = 0,
                                                   CurrencyDenominationId = x.Id,
                                                   BankSendCurrencyCount = 0,
                                                   CreatedBy = userid,
                                                   CreatedDate = indianTime,
                                                   IsActive = true,
                                                   IsDeleted = false,
                                                   ExchangeCurrencyCount = 0,
                                               }).ToList();
                                    if (hubCashCollections != null)
                                    {
                                        context.HubCashCollection.AddRange(hubCashCollections);
                                        foreach (var item in hubCashCollections)
                                        {
                                            currencyHubStock.HubCashCollections.Add(item);
                                        }
                                    }

                                }
                                context.CurrencyHubStock.Add(currencyHubStock);


                                if (context.Commit() > 0)
                                {
                                    responseResult = new ResponseResult { status = true, Message = "Warehouse Beigning Of Day (BOD) successfully." };
                                }
                                else
                                {
                                    responseResult = new ResponseResult { status = false, Message = "Some error occurred during Beigning Of Day (BOD)." };
                                }
                            }
                            else
                            {
                                responseResult = new ResponseResult { status = false, Message = "You are not start a today day." };
                            }
                        }
                        else
                        {
                            if (IsBOD)
                            {
                                responseResult = new ResponseResult { status = false, Message = "You are already start a today day." };
                            }
                            else
                            {
                                TodayHubCurrencyCollection.EOD = DateTime.Now;
                                TodayHubCurrencyCollection.ModifiedBy = userid;
                                TodayHubCurrencyCollection.ModifiedDate = indianTime;
                                TodayHubCurrencyCollection.Status = "EndDay";
                                //context.CurrencyHubStock.Attach(TodayHubCurrencyCollection);
                                context.Entry(TodayHubCurrencyCollection).State = EntityState.Modified;
                                if (context.Commit() > 0)
                                {
                                    responseResult = new ResponseResult { status = true, Message = "Warehouse End Of Day (BOD) successfully." };
                                }
                                else
                                {
                                    responseResult = new ResponseResult { status = false, Message = "Some error occurred during End Of Day (BOD)." };
                                }

                            }

                        }


                    }
                }
                catch (Exception ex)
                {
                    string error = ex.InnerException != null ? ex.ToString() + Environment.NewLine + ex.InnerException.ToString() : ex.ToString();

                    logger.Error("Error in WarehouseDayStartStop Method: " + error);
                    responseResult = new ResponseResult { status = false, Message = "Some error occurred during Beigning Of Day (BOD)." };
                }
            }
            return responseResult;
        }

        [Route("CloseNotEODWarehouse")]
        [HttpGet]
        [AllowAnonymous]
        public bool CloseNotEODWarehouse()
        {
            bool result = false;
            try
            {
                using (AuthContext context = new AuthContext())
                {
                    var HubCurrencyCollection = context.CurrencyHubStock.Where(x => EntityFunctions.TruncateTime(x.BOD) <= EntityFunctions.TruncateTime(DateTime.Now) && !x.EOD.HasValue).ToList();
                    if (HubCurrencyCollection != null && HubCurrencyCollection.Any())
                    {
                        foreach (var item in HubCurrencyCollection)
                        {
                            item.EOD = DateTime.Now;
                            //item.ModifiedBy = userid;
                            item.ModifiedDate = indianTime;
                            item.Status = "EndDay";
                            //context.CurrencyHubStock.Attach(item);
                            context.Entry(item).State = EntityState.Modified;
                            result = context.Commit() > 0;
                        }
                    }
                    else
                        result = true;
                }
            }
            catch (Exception ex)
            {
                logger.Error("Error in CloseAllNotEODWarehouse Method: " + ex.Message);
            }

            return result;
        }


        [Route("WarehouseDayStart")]
        [HttpGet]
        [AllowAnonymous]
        public bool WarehouseDayStart()
        {
            bool result = false;
            // bool isWCGenerated = false;
            var identity = User.Identity as ClaimsIdentity;
            int compid = 0, userid = 0;

            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))
                compid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);

            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
            try
            {
                WorkingCapitalManager manager = new WorkingCapitalManager();
                result = manager.CashManagementEOD();

            }
            catch (Exception ex)
            {
                string error = ex.InnerException != null ? ex.ToString() + Environment.NewLine + ex.InnerException.ToString() : ex.ToString();

                logger.Error("Error in WarehouseDayStart Method: " + error);
                result = false;
            }

            return result;
        }
        #endregion

        #region Cheque History Mthod
        [Route("GetHubChequeCollection")]
        [HttpGet]
        public List<ChequeCollectionDc> GetHubChequeCollection(int warehouseid, long? currencyHubStockId)
        {
            List<ChequeCollectionDc> ChequeCollectionDcs = new List<ChequeCollectionDc>();
            try
            {
                using (AuthContext context = new AuthContext())
                {
                    var chequestatusid = Convert.ToInt32(ChequeStatusEnum.Operation);
                    var predicate = PredicateBuilder.True<ChequeCollection>();

                    predicate = predicate.And(x => x.CurrencyStock.Warehouseid == warehouseid && x.ChequeStatus == chequestatusid && x.IsActive && ((x.IsDeleted.HasValue && !x.IsDeleted.Value) || !x.IsDeleted.HasValue));
                    if (currencyHubStockId.HasValue)
                        predicate = predicate.And(x => x.CurrencyHubStockId == currencyHubStockId.Value);

                    ChequeCollectionDcs = context.ChequeCollection.Where(predicate).OrderByDescending(x => x.CreatedDate).Select(x =>
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
                               BankSubmitDate = x.BankSubmitDate,
                               ChequeimagePath = x.ChequeimagePath,
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

        [Route("GetHubChequeCollectionPaginator")]
        [HttpPost]
        public List<ChequeCollectionDc> GetHubChequeCollectionPaginator(Pager pager)
        {
            List<ChequeCollectionDc> ChequeCollectionDcs = new List<ChequeCollectionDc>();
            try
            {

                using (AuthContext context = new AuthContext())
                {
                    var chequestatusid = Convert.ToInt32(ChequeStatusEnum.Operation);
                    var predicate = PredicateBuilder.True<ChequeCollection>();

                    predicate = predicate.And(x => x.IsActive && ((x.IsDeleted.HasValue && !x.IsDeleted.Value) || !x.IsDeleted.HasValue));
                    if (pager.WarehouseID > 0)
                        predicate = predicate.And(x => x.CurrencyCollection.Warehouseid == pager.WarehouseID || x.WarehouseId == pager.WarehouseID);
                    if (pager.PeopleID > 0 || pager.PeopleID == null)
                        predicate = predicate.And(x => x.CurrencyCollection.DBoyPeopleId == pager.PeopleID);

                    if (pager.CurrencyHubStockId.HasValue)
                    {
                        predicate = predicate.Or(x => x.CurrencyHubStockId == pager.CurrencyHubStockId.Value || x.CurrencyHubStockId == null);
                    }

                    if (pager.ChequeStatus.HasValue)
                    {
                        predicate = predicate.And(x => x.ChequeStatus == pager.ChequeStatus.Value);
                    }

                    if (!string.IsNullOrEmpty(pager.searchfilter))
                    {
                        predicate = predicate.And(x => x.CurrencyCollection.Deliveryissueid.ToString().Contains(pager.searchfilter)
                                                || x.ChequeNumber.Contains(pager.searchfilter)
                                                || x.Orderid.ToString().Contains(pager.searchfilter)
                                                || x.ChequeBankName.ToString().Contains(pager.searchfilter)
                                                || x.DBoyPeopleId.ToString().Contains(pager.searchfilter)
                                                || x.ChequeAmt.ToString().Contains(pager.searchfilter)
                                                 || x.CurrencyCollection.Warehouseid.ToString().Contains(pager.searchfilter)
                                                );
                    }

                    if (pager.StartDate.HasValue && pager.EndDate.HasValue)
                    {
                        predicate = predicate.And(x => x.ChequeDate >= pager.StartDate.Value && x.ChequeDate <= pager.EndDate.Value);
                    }
                    context.Database.Log = s => Debug.WriteLine(s);
                    ChequeCollectionDcs = context.ChequeCollection.Where(predicate).OrderByDescending(x => x.CreatedDate).Skip(pager.SkipCount).Take(pager.RowCount).Select(x =>
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
                          BankSubmitDate = x.BankSubmitDate,
                          ChequeimagePath = x.ChequeimagePath,
                          OrderId = x.Orderid,
                          Deliveryissueid = x.CurrencyCollection != null ? x.CurrencyCollection.Deliveryissueid : (int?)null,
                          ChequeBankName = x.ChequeBankName,
                          CurrencySettlementid = x.CurrencySettlementSourceId,
                          CurrencyCollectionStatus = x.CurrencyCollection != null ? x.CurrencyCollection.Status : "",
                          ReturnComment = x.ReturnComment,
                          BounceImage = x.BounceImage,
                          WarehouseId = x.CurrencyCollection != null ? x.CurrencyCollection.Warehouseid : (int?)null,
                          //DepositBankName= x.CurrencySettlementSource !=null? x.CurrencySettlementSource.SettlementSource:""
                      }).ToList();


                    if (ChequeCollectionDcs != null && ChequeCollectionDcs.Any())
                    {
                        var warehousesids = ChequeCollectionDcs.Select(x => x.WarehouseId).ToList();
                        var warehouseData = context.Warehouses.Where(x => warehousesids.Contains(x.WarehouseId)).Select(x => new { x.WarehouseId, x.WarehouseName });
                        var peoplesids = ChequeCollectionDcs.Select(x => x.DBoyPeopleId).Distinct().ToList();
                        var peopeldata = context.Peoples.Where(x => peoplesids.Contains(x.PeopleID)).Select(x => new { x.PeopleID, x.DisplayName });
                        var orderIds = ChequeCollectionDcs.Select(x => x.OrderId).Distinct().ToList();
                        var orderskcode = context.DbOrderMaster.Where(x => orderIds.Contains(x.OrderId)).Select(x => new { x.Skcode, x.OrderId });
                        foreach (var item in ChequeCollectionDcs)
                        {

                            item.DBoyName = peopeldata != null && peopeldata.Any(x => x.PeopleID == item.DBoyPeopleId) ? peopeldata.FirstOrDefault(x => x.PeopleID == item.DBoyPeopleId).DisplayName : "";
                            item.WarehouseName = warehouseData != null && warehouseData.Any(x => x.WarehouseId == item.WarehouseId) ? warehouseData.FirstOrDefault(x => x.WarehouseId == item.WarehouseId).WarehouseName : "";
                            item.SKCode = orderskcode != null && orderskcode.Any(x => x.OrderId == item.OrderId) ? orderskcode.FirstOrDefault(x => x.OrderId == item.OrderId).Skcode : "";
                            if (item.CurrencySettlementid.HasValue)
                                item.DepositBankName = context.CurrencySettlementSource.FirstOrDefault(x => x.Id == item.CurrencySettlementid.Value)?.SettlementSource;
                        }
                    }

                }

                return ChequeCollectionDcs;
            }
            catch (Exception ex)
            {
                logger.Error("Error in GetChequeCollection Method: " + ex.Message);
                return null;
            }
        }

        [Route("GetHubChequeCollectionPaginatorCount")]
        [HttpPost]
        public int GetHubChequeCollectionPaginatorCount(Pager pager)
        {
            int count = 0;
            try
            {
                using (AuthContext context = new AuthContext())
                {

                    var chequestatusid = Convert.ToInt32(ChequeStatusEnum.Operation);
                    var predicate = PredicateBuilder.True<ChequeCollection>();

                    predicate = predicate.And(x => x.IsActive && ((x.IsDeleted.HasValue && !x.IsDeleted.Value) || !x.IsDeleted.HasValue));
                    if (pager.WarehouseID > 0)
                        predicate = predicate.And(x => x.CurrencyCollection.Warehouseid == pager.WarehouseID || x.WarehouseId == pager.WarehouseID);
                    if (pager.CurrencyHubStockId.HasValue)
                        predicate = predicate.And(x => x.CurrencyHubStockId == pager.CurrencyHubStockId.Value || x.CurrencyHubStockId == null);
                    if (pager.ChequeStatus.HasValue)
                    {
                        predicate = predicate.And(x => x.ChequeStatus == pager.ChequeStatus.Value);
                    }


                    if (!string.IsNullOrEmpty(pager.searchfilter))
                    {
                        predicate = predicate.And(x => x.CurrencyCollection.Deliveryissueid.ToString().Contains(pager.searchfilter)
                                                || x.ChequeNumber.Contains(pager.searchfilter)
                                                || x.Orderid.ToString().Contains(pager.searchfilter)
                                                || x.ChequeBankName.ToString().Contains(pager.searchfilter)
                                                || x.DBoyPeopleId.ToString().Contains(pager.searchfilter)
                                                );
                    }


                    if (pager.ChequeDate.HasValue)
                    {
                        predicate = predicate.And(x => x.ChequeDate == pager.ChequeDate.Value);
                    }
                    if (pager.StartDate.HasValue && pager.EndDate.HasValue)
                    {
                        predicate = predicate.And(x => x.ChequeDate >= pager.StartDate.Value && x.ChequeDate <= pager.EndDate.Value);
                    }
                    count = context.ChequeCollection.Where(predicate).OrderByDescending(x => x.CreatedDate).Count();

                }

                return count;
            }
            catch (Exception ex)
            {
                logger.Error("Error in GetChequeCollection Method: " + ex.Message);
                return count;
            }
        }


        [Route("GetHubRetrunChequeCollection")]
        [HttpPost]
        public ReturnChequeCollectionPaggingData GetHubRetrunChequeCollectionPaginator(Pager pager)
        {
            ReturnChequeCollectionPaggingData returnChequeCollectionPaggingData = new ReturnChequeCollectionPaggingData();
            List<ReturnChequeCollectionDc> returnChequeCollectionDcs = new List<ReturnChequeCollectionDc>();
            try
            {
                using (AuthContext context = new AuthContext())
                {
                    //var chequestatusid = Convert.ToInt32(ChequeReturnStatusEnum.Return);
                    var predicate = PredicateBuilder.True<ReturnChequeCollection>();
                    predicate = predicate.And(x => x.IsActive && x.IsDeleted != true);
                    if (pager.WarehouseID > 0)
                        predicate = predicate.And(x => x.ChequeCollection.CurrencyCollection.Warehouseid == pager.WarehouseID);

                    if (pager.ChequeStatus.HasValue)
                    {
                        predicate = predicate.And(x => x.Status == pager.ChequeStatus.Value);
                    }

                    if (!string.IsNullOrEmpty(pager.searchfilter))
                    {
                        predicate = predicate.And(x => x.ChequeCollection.CurrencyCollection.Deliveryissueid.ToString().Contains(pager.searchfilter)
                                                || x.ChequeCollection.ChequeNumber.Contains(pager.searchfilter)
                                                || x.ChequeCollection.Orderid.ToString().Contains(pager.searchfilter)
                                                || x.ChequeCollection.ChequeBankName.ToString().Contains(pager.searchfilter)
                                                || x.ChequeCollection.ChequeDate.ToString().Contains(pager.searchfilter)
                                                || x.CourierName.ToString().Contains(pager.searchfilter)
                                                );
                    }

                    if (pager.StartDate.HasValue && pager.EndDate.HasValue)
                    {
                        predicate = predicate.And(x => x.ChequeCollection.ChequeDate >= pager.StartDate.Value && x.ChequeCollection.ChequeDate <= pager.EndDate.Value);
                    }

                    context.Database.Log = s => Debug.WriteLine(s);
                    var returnchequequery = context.ReturnChequeCollection.Where(predicate).Select(x =>
                           new ReturnChequeCollectionDc
                           {

                               ChequeAmt = x.ChequeCollection.ChequeAmt,
                               ChequeDate = x.ChequeCollection.ChequeDate,
                               ChequeNumber = x.ChequeCollection.ChequeNumber,
                               Status = x.Status,
                               ChequeCollectionId = x.ChequeCollectionId,
                               Id = x.Id,
                               OrderId = x.ChequeCollection.Orderid,
                               Deliveryissueid = x.ChequeCollection.CurrencyCollection.Deliveryissueid,
                               ChequeBankName = x.ChequeCollection.ChequeBankName,
                               CourierDate = x.CourierDate,
                               CourierName = x.CourierName,
                               CreatedDate = x.CreatedDate,
                               HandOverAgentId = x.HandOverAgentId,
                               HandOverAgentName = x.HandOverAgentName,
                               HandOverDate = x.HandOverDate,
                               HQReceiveDate = x.HQReceiveDate,
                               HQReceiverName = x.HQReceiverName,
                               HubReceiveDate = x.HubReceiveDate,
                               HubReceiverName = x.HubReceiverName,
                               HubSenderName = x.HubSenderName,
                               ModifiedDate = x.ModifiedDate,
                               PodNo = x.PodNo,
                               Warehouseid = x.ChequeCollection.CurrencyCollection.Warehouseid,
                               WarehouseName = context.Warehouses.Where(y => y.WarehouseId == x.ChequeCollection.CurrencyCollection.Warehouseid).Select(j => j.WarehouseName).FirstOrDefault(),
                           });

                    int total = returnchequequery.Count();
                    returnChequeCollectionDcs = returnchequequery.OrderByDescending(x => x.CreatedDate).Skip(pager.SkipCount).Take(pager.RowCount).ToList();

                    if (returnChequeCollectionDcs != null && returnChequeCollectionDcs.Any())
                    {
                        var orderIds = returnChequeCollectionDcs.Select(x => x.OrderId).Distinct().ToList();
                        var orderskcode = context.DbOrderMaster.Where(x => orderIds.Contains(x.OrderId)).Select(x => new { x.Skcode, x.OrderId });
                        var WarehouseId = returnChequeCollectionDcs.Select(x => x.Warehouseid).Distinct().ToList();
                        foreach (var item in returnChequeCollectionDcs)
                        {
                            item.SKCode = orderskcode != null && orderskcode.Any(x => x.OrderId == item.OrderId) ? orderskcode.FirstOrDefault(x => x.OrderId == item.OrderId).Skcode : "";
                            item.IsHQReceive = item.HQReceiveDate.HasValue;
                            item.IsHQSentCourier = item.CourierDate.HasValue;
                            item.IsHubReceive = item.HubReceiveDate.HasValue;
                            item.IsHubHandOverAgent = item.HandOverDate.HasValue;
                        }
                    }
                    returnChequeCollectionPaggingData.total_count = total;
                    returnChequeCollectionPaggingData.ReturnChequeCollectionDcs = returnChequeCollectionDcs;
                }

                return returnChequeCollectionPaggingData;
            }
            catch (Exception ex)
            {
                logger.Error("Error in GetHubRetrunChequeCollection Method: " + ex.Message);
                return null;
            }
        }

        [Route("GetChequeStatus")]
        [HttpGet]
        public List<ChequeStatusResponse> GetChequeStatus()

        {
            List<ChequeStatusResponse> list = new List<ChequeStatusResponse>();
            ChequeStatusResponse response = new ChequeStatusResponse()
            {
                Name = ChequeStatusEnum.Operation.ToString(),
                Value = (int)ChequeStatusEnum.Operation
            };
            list.Add(response);


            response = new ChequeStatusResponse()
            {
                Name = ChequeStatusEnum.Bank.ToString(),
                Value = (int)ChequeStatusEnum.Bank
            };
            list.Add(response);

            response = new ChequeStatusResponse()
            {
                Name = ChequeStatusEnum.Clear.ToString(),
                Value = (int)ChequeStatusEnum.Clear
            };
            list.Add(response);

            response = new ChequeStatusResponse()
            {
                Name = ChequeStatusEnum.Return.ToString(),
                Value = (int)ChequeStatusEnum.Return
            };
            list.Add(response);

            response = new ChequeStatusResponse()
            {
                Name = ChequeStatusEnum.Reject.ToString(),
                Value = (int)ChequeStatusEnum.Reject
            };
            list.Add(response);
            response = new ChequeStatusResponse()
            {
                Name = ChequeStatusEnum.Recovered.ToString(),
                Value = (int)ChequeStatusEnum.Recovered
            };
            list.Add(response);
            return list;
        }

        [Route("GetAllChequeStatus")]
        [HttpGet]
        public List<ChequeStatusResponse> GetAllChequeStatus()
        {
            List<ChequeStatusResponse> list = new List<ChequeStatusResponse>();
            //ChequeStatusResponse response = new ChequeStatusResponse()
            //{
            //    Name = ChequeStatusEnum.Operation.ToString(),
            //    Value = (int)ChequeStatusEnum.Operation
            //};
            //list.Add(response);


            ChequeStatusResponse response = new ChequeStatusResponse()
            {
                Name = ChequeStatusEnum.Bank.ToString(),
                Value = (int)ChequeStatusEnum.Bank
            };
            list.Add(response);

            response = new ChequeStatusResponse()
            {
                Name = ChequeStatusEnum.Clear.ToString(),
                Value = (int)ChequeStatusEnum.Clear
            };
            list.Add(response);

            response = new ChequeStatusResponse()
            {
                Name = ChequeStatusEnum.Return.ToString(),
                Value = (int)ChequeStatusEnum.Return
            };
            list.Add(response);

            return list;
        }

        #endregion

        #region CurrencySettlement source screen method
        [Route("GetHubCashCollection")]
        [HttpGet]
        public List<hubCashCollectionDc> GetHubCashCollection(long currencyHubStockId)
        {
            List<hubCashCollectionDc> hubCashCollectionDc = new List<hubCashCollectionDc>();
            try
            {
                using (AuthContext context = new AuthContext())
                {
                    var predicate = PredicateBuilder.True<HubCashCollection>();
                    predicate = predicate.And(x => x.CurrencyHubStockId == currencyHubStockId && x.IsActive && ((x.IsDeleted.HasValue && !x.IsDeleted.Value) || !x.IsDeleted.HasValue));

                    hubCashCollectionDc = (from c in context.CurrencyDenomination.Where(x => x.IsActive)
                                           join p in context.HubCashCollection.Where(predicate)
                                           on c.Id equals p.CurrencyDenominationId into ps
                                           from p in ps.DefaultIfEmpty()
                                           select new hubCashCollectionDc
                                           {
                                               CurrencyCount = (p == null ? 0 : p.CurrencyCount) + (p == null ? 0 : p.OpeningCurrencyCount) - ((p == null ? 0 : p.BankSendCurrencyCount)) + (p == null ? 0 : p.ExchangeCurrencyCount),
                                               Id = (p == null ? 0 : p.Id),
                                               CurrencyDenominationId = c.Id,
                                               CurrencyDenominationTitle = c == null ? "" : c.Title,
                                               CurrencyDenominationValue = c == null ? 0 : c.Value
                                           }).ToList();
                }

                return hubCashCollectionDc;
            }
            catch (Exception ex)
            {
                logger.Error("Error in GetHubCashCollection Method: " + ex.Message);
                return null;
            }
        }

        [Route("GetCurrencySettlementSource")]
        [HttpGet]
        public List<CurrencySettlementSourceDc> GetCurrencySettlementSource(long currencyHubStockId)
        {
            List<CurrencySettlementSourceDc> currencySettlementSourceDcs = new List<CurrencySettlementSourceDc>();
            try
            {
                using (AuthContext context = new AuthContext())
                {
                    currencySettlementSourceDcs = context.CurrencySettlementSource.Where(x => x.currencyHubStockId == currencyHubStockId && EntityFunctions.TruncateTime(x.CreatedDate) == EntityFunctions.TruncateTime(DateTime.Now)
                                                                              && x.IsActive && ((x.IsDeleted.HasValue && !x.IsDeleted.Value) || !x.IsDeleted.HasValue)).Select(x =>
                        new CurrencySettlementSourceDc
                        {
                            CreatedBy = x.CreatedBy,
                            CreatedDate = x.CreatedDate,
                            HandOverPerson = x.HandOverPerson,
                            Id = x.Id,
                            IsActive = x.IsActive,
                            Note = x.Note,
                            SettlementDate = x.SettlementDate,
                            SettlementSource = x.SettlementSource,
                            Status = x.Status,
                            TotalCashAmt = x.TotalCashAmt,
                            TotalChequeAmt = x.TotalChequeAmt,
                            ProcessTransId = x.ProcessTransId,
                            SettlementProofImages = x.CurrencySettlementImages.Select(y => new CurrencySettlementImagesDc
                            {
                                CreatedBy = y.CreatedBy,
                                CreatedDate = y.CreatedDate,
                                CurrencySettlementSourceId = y.CurrencySettlementSourceId,
                                Id = y.Id,
                                IsActive = y.IsActive,
                                IsDeleted = y.IsDeleted,
                                SettlementImage = y.SettlementImage,
                                SettlementImageType = y.SettlementImageType
                            }).ToList()
                        }).ToList();
                    if (currencySettlementSourceDcs != null && currencySettlementSourceDcs.Any())
                    {
                        var peoplesids = currencySettlementSourceDcs.Select(x => x.HandOverPerson).Distinct().ToList();
                        var peopeldata = context.Peoples.Where(x => peoplesids.Contains(x.PeopleID)).Select(x => new { x.PeopleID, x.DisplayName });
                        foreach (var item in currencySettlementSourceDcs)
                        {
                            var allSettlementProofImages = item.SettlementProofImages;
                            if (allSettlementProofImages != null && allSettlementProofImages.Any())
                            {
                                item.SlipImages = allSettlementProofImages.Where(x => x.SettlementImageType == "Deposit").ToList();
                                if (item.SlipImages != null && item.SlipImages.Any())
                                {
                                    item.SlipImages.ForEach(x =>
                                    {
                                        x.SettlementImage = HttpContext.Current.Request.UrlReferrer.AbsoluteUri + x.SettlementImage;
                                        x.SettlementImagename = x.SettlementImage.Split('/').Any() ? x.SettlementImage.Split('/').Last() : x.SettlementImagename;
                                    });
                                }

                                item.SettlementProofImages = allSettlementProofImages.Where(x => x.SettlementImageType == "Withdraw").ToList();
                                if (item.SettlementProofImages != null && item.SettlementProofImages.Any())
                                {
                                    item.SettlementProofImages.ForEach(x =>
                                    {
                                        x.SettlementImage = HttpContext.Current.Request.UrlReferrer.AbsoluteUri + x.SettlementImage;
                                        x.SettlementImagename = x.SettlementImage.Split('/').Any() ? x.SettlementImage.Split('/').Last() : x.SettlementImagename;
                                    });

                                }
                            }

                            item.HandOverPersonName = peopeldata != null && peopeldata.Any(x => x.PeopleID == item.HandOverPerson) ? peopeldata.FirstOrDefault(x => x.PeopleID == item.HandOverPerson).DisplayName : "";
                        }
                    }
                }

                return currencySettlementSourceDcs;
            }
            catch (Exception ex)
            {
                logger.Error("Error in GetCurrencySettlementSource Method: " + ex.Message);
                return null;
            }
        }

        //Bank Deposit
        [Route("GetBankDepositDetail")]
        [HttpGet]
        public BankDepositDetailDc GetBankDepositDetail(long currencyHubStockId, long warehouseId)
        {
            BankDepositDetailDc BankDepositDetailDc = new BankDepositDetailDc { DepositType = 0 };
            List<CurrencySettlementBankDc> currencySettlementBankDcs = new List<CurrencySettlementBankDc>();
            try
            {
                using (AuthContext context = new AuthContext())
                {
                    currencySettlementBankDcs = context.CurrencySettlementBank.Where(x => x.DepositBank).Select(x =>
                          new CurrencySettlementBankDc
                          {
                              BankImage = x.BankImage,
                              BankName = x.BankName,
                              Id = x.Id
                          }).ToList();


                    var predicate = PredicateBuilder.True<HubCashCollection>();
                    predicate = predicate.And(x => x.CurrencyHubStockId == currencyHubStockId && x.IsActive && ((x.IsDeleted.HasValue && !x.IsDeleted.Value) || !x.IsDeleted.HasValue));

                    var hubCashCollectionDc = (from c in context.CurrencyDenomination.Where(x => x.IsActive)
                                               join p in context.HubCashCollection.Where(predicate)
                                               on c.Id equals p.CurrencyDenominationId into ps
                                               from p in ps.DefaultIfEmpty()
                                               select new hubCashCollectionDc
                                               {
                                                   CurrencyCount = (p == null ? 0 : p.CurrencyCount) + (p == null ? 0 : p.OpeningCurrencyCount) - ((p == null ? 0 : p.BankSendCurrencyCount)) + (p == null ? 0 : p.ExchangeCurrencyCount),
                                                   Id = (p == null ? 0 : p.Id),
                                                   CurrencyDenominationId = c.Id,
                                                   CurrencyDenominationTitle = c == null ? "" : c.Title,
                                                   CurrencyDenominationValue = c == null ? 0 : c.Value,
                                                   //BankDepositCurrencyCount = (p == null ? 0 : p.CurrencyCount) + (p == null ? 0 : p.OpeningCurrencyCount) - ((p == null ? 0 : p.BankSendCurrencyCount)) + (p == null ? 0 : p.ExchangeCurrencyCount),
                                                   BankDepositCurrencyCount = 0,
                                                   CashCurrencyType = c == null ? "" : c.currencyType,
                                               }).ToList();

                    var chequestatusid = Convert.ToInt32(ChequeStatusEnum.Operation);
                    var predicatecheque = PredicateBuilder.True<ChequeCollection>();

                    predicatecheque = predicatecheque.And(x => ((x.CurrencyHubStockId == currencyHubStockId && x.ChequeStatus == chequestatusid) || (x.ChequeStatus == chequestatusid && x.CurrencyCollection.Warehouseid == warehouseId) && x.IsActive && ((x.IsDeleted.HasValue && !x.IsDeleted.Value) || !x.IsDeleted.HasValue)));


                    var ChequeCollectionDcs = context.ChequeCollection.Where(predicatecheque).OrderByDescending(x => x.CreatedDate).Select(x =>
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
                               BankSubmitDate = x.BankSubmitDate,
                               ChequeimagePath = x.ChequeimagePath,
                               ChequeBankName = x.ChequeBankName

                           }).ToList();

                    BankDepositDetailDc.CurrencySettlementBankDcs = currencySettlementBankDcs;
                    BankDepositDetailDc.hubCashCollectionDcs = hubCashCollectionDc;
                    BankDepositDetailDc.ChequeCollectionDcs = ChequeCollectionDcs;
                }

                return BankDepositDetailDc;
            }
            catch (Exception ex)
            {
                logger.Error("Error in GetCurrencySettlementBanks Method: " + ex.Message);
                return null;
            }
        }
        [Route("UpdateBankDepositDetail")]
        [HttpPost]
        public ResponseResult UpdateBankDepositDetail(BankDepositDetailDc bankDepositDetailDc)
        {
            ResponseResult responseResult = new ResponseResult { status = false, Message = "" };
            try
            {
                var identity = User.Identity as ClaimsIdentity;
                int compid = 0, userid = 0;

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))
                    compid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                    userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

                if (bankDepositDetailDc != null && (bankDepositDetailDc.ChequeCollectionDcs.Any() || bankDepositDetailDc.hubCashCollectionDcs.Any()))
                {
                    TransactionOptions option = new TransactionOptions();
                    option.IsolationLevel = System.Transactions.IsolationLevel.RepeatableRead;
                    option.Timeout = TimeSpan.FromSeconds(90);
                    using (var dbContextTransaction = new TransactionScope(TransactionScopeOption.Required, option))
                    {
                        using (AuthContext context = new AuthContext())
                        {
                            //----S------Validate-----------
                            int warehouseid7 = 0;
                            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "Warehouseid"))
                                warehouseid7 = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "Warehouseid").Value);

                            //&& x.PageName == PageNumber 
                            var PageAccessList = context.CMSPageAccessDB.FirstOrDefault(x => x.WarehouseId == warehouseid7 && x.AccessEndTime == null && x.UserID == userid);

                            if (PageAccessList == null)
                            {
                                responseResult.Message = "process not started by cashier";
                                responseResult.status = false;
                                return responseResult;
                            }
                            else
                            {
                                //----E------Validate-----------


                                decimal totalChequeamt = 0, totalCashamt = 0;
                                CurrencySettlementSource currencySettlementSource = new CurrencySettlementSource();
                                currencySettlementSource.CreatedBy = userid;
                                currencySettlementSource.CreatedDate = indianTime;
                                currencySettlementSource.currencyHubStockId = bankDepositDetailDc.currencyHubStockId;
                                currencySettlementSource.DepositType = bankDepositDetailDc.DepositType;
                                currencySettlementSource.HandOverPerson = userid;
                                currencySettlementSource.IsActive = true;
                                currencySettlementSource.IsDeleted = false;
                                currencySettlementSource.SettlementDate = DateTime.Now;
                                currencySettlementSource.SettlementSource = bankDepositDetailDc.SettlementSource;
                                currencySettlementSource.Note = bankDepositDetailDc.Note;
                                currencySettlementSource.Warehouseid = bankDepositDetailDc.WarehouseId;
                                currencySettlementSource.ChequeCollections = new List<ChequeCollection>();
                                currencySettlementSource.CashSettlements = new List<CashSettlement>();
                                currencySettlementSource.Status = 0;
                                if (bankDepositDetailDc.DepositType == 0)
                                {
                                    if ((bankDepositDetailDc.ChequeCollectionDcs.Any(x => x.Ischecked) || bankDepositDetailDc.hubCashCollectionDcs.Any(x => x.BankDepositCurrencyCount > 0)))
                                    {

                                        if (bankDepositDetailDc.ChequeCollectionDcs.Any(x => x.Ischecked))
                                        {
                                            var chequeids = bankDepositDetailDc.ChequeCollectionDcs.Where(x => x.Ischecked).Select(x => x.Id).ToList();
                                            var ChequeCollection = context.ChequeCollection.Where(x => chequeids.Contains(x.Id));
                                            ChequeCollection.ToList().ForEach(x =>
                                            {
                                                x.ChequeStatus = Convert.ToInt32(ChequeStatusEnum.Bank);
                                                x.BankSubmitDate = indianTime;
                                                x.ModifiedBy = userid;
                                                x.ModifiedDate = indianTime;
                                                //context.ChequeCollection.Attach(x);
                                                context.Entry(x).State = EntityState.Modified;
                                                currencySettlementSource.ChequeCollections.Add(x);
                                                totalChequeamt += x.ChequeAmt;
                                            });
                                        }

                                        if (bankDepositDetailDc.hubCashCollectionDcs.Any(x => x.BankDepositCurrencyCount > 0))
                                        {


                                            List<CashSettlement> CashSettlements = bankDepositDetailDc.hubCashCollectionDcs.Where(x => x.BankDepositCurrencyCount > 0).Select(x => new CashSettlement
                                            {
                                                CreatedBy = userid,
                                                CreatedDate = indianTime,
                                                CurrencyCount = x.BankDepositCurrencyCount,
                                                CurrencyDenominationId = x.CurrencyDenominationId,
                                                IsActive = true,
                                                IsDeleted = false
                                            }).ToList();
                                            context.CashSettlement.AddRange(CashSettlements);

                                            var currencyDenominations = context.CurrencyDenomination.Where(x => x.IsActive).ToList();
                                            foreach (var item in CashSettlements)
                                            {
                                                var denomiation = currencyDenominations.FirstOrDefault(x => x.Id == item.CurrencyDenominationId);
                                                totalCashamt += denomiation.Value * item.CurrencyCount;
                                                currencySettlementSource.CashSettlements.Add(item);
                                            }

                                            ///Update Hub Cash                          
                                            var depositDenominationid = CashSettlements.Select(x => x.CurrencyDenominationId);
                                            var hubCashCollections = context.HubCashCollection.Where(x => x.CurrencyHubStockId == bankDepositDetailDc.currencyHubStockId && depositDenominationid.Contains(x.CurrencyDenominationId));
                                            if (hubCashCollections != null && hubCashCollections.Any())
                                            {
                                                foreach (var item in hubCashCollections)
                                                {
                                                    var bankCash = CashSettlements.FirstOrDefault(x => x.CurrencyDenominationId == item.CurrencyDenominationId);
                                                    item.BankSendCurrencyCount = item.BankSendCurrencyCount + bankCash.CurrencyCount;
                                                    //context.HubCashCollection.Attach(item);
                                                    context.Entry(item).State = EntityState.Modified;
                                                }
                                            }

                                        }
                                        currencySettlementSource.TotalChequeAmt = totalChequeamt;
                                        currencySettlementSource.TotalCashAmt = totalCashamt;
                                        context.CurrencySettlementSource.Add(currencySettlementSource);
                                    }
                                    else
                                    {
                                        responseResult.Message = "Please select at least one cheque and cash to deposit to bank.";
                                        responseResult.status = false;

                                    }
                                }
                                else if (bankDepositDetailDc.DepositType == 1)
                                {
                                    if (bankDepositDetailDc.hubCashCollectionDcs.Any(x => x.BankDepositCurrencyCount > 0))
                                    {

                                        List<CashSettlement> CashSettlements = bankDepositDetailDc.hubCashCollectionDcs.Where(x => x.BankDepositCurrencyCount > 0).Select(x => new CashSettlement
                                        {
                                            CreatedBy = userid,
                                            CreatedDate = indianTime,
                                            CurrencyCount = x.BankDepositCurrencyCount,
                                            CurrencyDenominationId = x.CurrencyDenominationId,
                                            IsActive = true,
                                            IsDeleted = false
                                        }).ToList();
                                        context.CashSettlement.AddRange(CashSettlements);
                                        var currencyDenominations = context.CurrencyDenomination.Where(x => x.IsActive).ToList();
                                        foreach (var item in CashSettlements)
                                        {
                                            var denomiation = currencyDenominations.FirstOrDefault(x => x.Id == item.CurrencyDenominationId);
                                            totalCashamt += denomiation.Value * item.CurrencyCount;
                                            currencySettlementSource.CashSettlements.Add(item);
                                        }

                                        ///Update Hub Cash
                                        var depositDenominationid = CashSettlements.Select(x => x.CurrencyDenominationId);
                                        var hubCashCollections = context.HubCashCollection.Where(x => x.CurrencyHubStockId == bankDepositDetailDc.currencyHubStockId && depositDenominationid.Contains(x.CurrencyDenominationId));
                                        if (hubCashCollections != null && hubCashCollections.Any())
                                        {
                                            foreach (var item in hubCashCollections)
                                            {
                                                var bankCash = CashSettlements.FirstOrDefault(x => x.CurrencyDenominationId == item.CurrencyDenominationId);
                                                item.BankSendCurrencyCount = item.BankSendCurrencyCount + bankCash.CurrencyCount;
                                                //context.HubCashCollection.Attach(item);
                                                context.Entry(item).State = EntityState.Modified;
                                            }
                                        }

                                        currencySettlementSource.TotalChequeAmt = totalChequeamt;
                                        currencySettlementSource.TotalCashAmt = totalCashamt;
                                        context.CurrencySettlementSource.Add(currencySettlementSource);
                                    }
                                    else
                                    {
                                        responseResult.Message = "Please select at least one cash denomination to deposit to bank.";
                                        responseResult.status = false;

                                    }
                                }
                                else if (bankDepositDetailDc.DepositType == 2)
                                {
                                    if (bankDepositDetailDc.ChequeCollectionDcs.Any(x => x.Ischecked))
                                    {
                                        var chequeids = bankDepositDetailDc.ChequeCollectionDcs.Where(x => x.Ischecked).Select(x => x.Id).ToList();
                                        var ChequeCollection = context.ChequeCollection.Where(x => chequeids.Contains(x.Id));
                                        ChequeCollection.ToList().ForEach(x =>
                                        {
                                            x.ChequeStatus = Convert.ToInt32(ChequeStatusEnum.Bank);
                                            x.ModifiedBy = userid;
                                            x.ModifiedDate = indianTime;
                                            x.BankSubmitDate = indianTime;
                                            //context.ChequeCollection.Attach(x);
                                            context.Entry(x).State = EntityState.Modified;
                                            currencySettlementSource.ChequeCollections.Add(x);
                                            totalChequeamt += x.ChequeAmt;
                                        });
                                        currencySettlementSource.TotalChequeAmt = totalChequeamt;
                                        currencySettlementSource.TotalCashAmt = totalCashamt;
                                        context.CurrencySettlementSource.Add(currencySettlementSource);
                                    }
                                    else
                                    {
                                        if (bankDepositDetailDc.DepositType == 2)
                                        {
                                            responseResult.Message = "Please select at least one cheque to deposit to bank.";
                                            responseResult.status = false;
                                        }
                                    }
                                }


                                if (context.Commit() > 0)
                                {
                                    // string fileName = "WithdrawSlip_" + currencySettlementSource.Id + "_" + DateTime.Now.ToString("yyyy-dd-M--HH-mm-ss") +".pdf";
                                    //string htmlData = GetWithdrawSlipHtml(currencySettlementSource, context);


                                    string saveFilepath = ChequeHistoryHelper.Generatepdf(currencySettlementSource, context);

                                    //if (!string.IsNullOrEmpty(htmlData))
                                    //{
                                    //    string saveFilepath = HTMLToPDFGenerator.Convert(@"~\BankWithdrawDepositFile\", fileName, htmlData);
                                    if (!string.IsNullOrEmpty(saveFilepath))
                                    {
                                        CurrencySettlementImages currencySettlementImages = new CurrencySettlementImages();
                                        currencySettlementImages.CurrencySettlementSourceId = currencySettlementSource.Id;
                                        currencySettlementImages.CreatedBy = userid;
                                        currencySettlementImages.CreatedDate = indianTime;
                                        currencySettlementImages.IsActive = true;
                                        currencySettlementImages.IsDeleted = false;
                                        currencySettlementImages.SettlementImage = saveFilepath;
                                        currencySettlementImages.SettlementImageType = "Withdraw";
                                        context.CurrencySettlementImages.Add(currencySettlementImages);
                                        if (context.Commit() == 0)
                                        {
                                            logger.Error("Error during Save Bank Withdraw image");
                                        }
                                    }


                                    //}
                                    responseResult.Message = "Bank Withdraw detail & slip generated successfully.";
                                    responseResult.status = true;
                                    responseResult.bankDepositDetailDc = bankDepositDetailDc;


                                }
                                else
                                {
                                    responseResult.Message = "Some error occurred during save Bank detail.";
                                    responseResult.status = false;
                                }

                                //----S------Validate-----------
                            }
                            //----E------Validate-----------
                        }

                        dbContextTransaction.Complete();
                    }
                }
                else
                {
                    responseResult.Message = "Please select at least one cheque to deposit to bank.";
                    responseResult.status = false;
                }

            }
            catch (Exception ex)
            {
                logger.Error("Error in GetExchangeHubCashCollection Method: " + ex.Message);
                responseResult.Message = "Some error occurred during save Bank detail.";
                responseResult.status = false;
            }
            return responseResult;
        }

        private string GetWithdrawSlipHtml(CurrencySettlementSource currencySettlementSource, AuthContext context)
        {
            string htmldata = "";
            if (currencySettlementSource != null)
            {
                string path = System.Web.Hosting.HostingEnvironment.MapPath(@"~\Templates\withdraw_slip1.html");
                if (File.Exists(path))
                {
                    htmldata = File.ReadAllText(path);
                    if (!string.IsNullOrEmpty(htmldata))
                    {
                        if (currencySettlementSource.HandOverPerson > 0)
                        {
                            var withdrawperson = context.Peoples.Where(x => x.PeopleID == currencySettlementSource.HandOverPerson).Select(x => x.DisplayName).FirstOrDefault();
                            htmldata = htmldata.Replace("{{withdrawperson}}", withdrawperson);
                        }
                        htmldata = htmldata.Replace("{{withdrawdate}}", currencySettlementSource.SettlementDate.ToString("dd/MM/yyyy hh:mm tt"));
                        htmldata = htmldata.Replace("{{bankname}}", currencySettlementSource.SettlementSource);
                        htmldata = htmldata.Replace("{{comment}}", currencySettlementSource.Note);
                    }
                    if (currencySettlementSource.DepositType == 0 || currencySettlementSource.DepositType == 1)
                    {

                        string noteshtml = "", coinshtml = "";
                        decimal totatAmt = 0;
                        if (currencySettlementSource.CashSettlements != null && currencySettlementSource.CashSettlements.Any())
                        {
                            var currencyDenominations = context.CurrencyDenomination.Where(x => x.IsActive).ToList();
                            foreach (var item in currencySettlementSource.CashSettlements)
                            {
                                var denomiation = currencyDenominations.FirstOrDefault(x => x.Id == item.CurrencyDenominationId);
                                if (denomiation.currencyType == "Notes")
                                {
                                    noteshtml += "<tr>";
                                    noteshtml += "<td>" + denomiation.Title + "</td>";
                                    noteshtml += "<td>" + item.CurrencyCount + "</td>";
                                    noteshtml += "<td> &#8377; " + denomiation.Value * item.CurrencyCount + "</td>";
                                    noteshtml += "</tr>";
                                }
                                else
                                {
                                    coinshtml += "<tr>";
                                    coinshtml += "<td>" + denomiation.Title + "</td>";
                                    coinshtml += "<td>" + item.CurrencyCount + "</td>";
                                    coinshtml += "<td> &#8377; " + denomiation.Value * item.CurrencyCount + "</td>";
                                    coinshtml += "</tr>";
                                }
                                totatAmt += denomiation.Value * item.CurrencyCount;
                            }
                        }
                        htmldata = htmldata.Replace("{{cashdisplay}}", "");
                        htmldata = htmldata.Replace("{{notes}}", noteshtml);
                        htmldata = htmldata.Replace("{{coins}}", coinshtml);
                        htmldata = htmldata.Replace("{{totalcashamt}}", totatAmt.ToString());
                    }
                    else
                    {
                        htmldata = htmldata.Replace("{{cashdisplay}}", "none");
                        htmldata = htmldata.Replace("{{notes}}", "");
                        htmldata = htmldata.Replace("{{coins}}", "");
                        htmldata = htmldata.Replace("{{totalcashamt}}", "");
                    }

                    if (currencySettlementSource.DepositType == 0 || currencySettlementSource.DepositType == 2)
                    {
                        htmldata = htmldata.Replace("{{cashdisplay}}", "");
                        string chequehtml = "";
                        decimal totatChequeAmt = 0;
                        if (currencySettlementSource.ChequeCollections != null && currencySettlementSource.ChequeCollections.Any())
                        {
                            foreach (var item in currencySettlementSource.ChequeCollections)
                            {
                                chequehtml += "<tr>";
                                chequehtml += "<td>" + item.ChequeNumber + "</td>";
                                chequehtml += "<td>" + item.ChequeDate.ToString("dd/MM/yyyy") + "</td>";
                                chequehtml += "<td> &#8377; " + item.ChequeAmt + "</td>";
                                chequehtml += "</tr>";
                                totatChequeAmt += item.ChequeAmt;
                            }
                        }
                        htmldata = htmldata.Replace("{{chequedisplay}}", "");
                        htmldata = htmldata.Replace("{{cheque}}", chequehtml);
                        htmldata = htmldata.Replace("{{totalchequeamt}}", totatChequeAmt.ToString());
                    }
                    else
                    {
                        htmldata = htmldata.Replace("{{chequedisplay}}", "none");
                        htmldata = htmldata.Replace("{{cheque}}", "");
                        htmldata = htmldata.Replace("{{totalchequeamt}}", "");
                    }
                }
            }

            return htmldata;
        }

        [Route("SaveCurrencySettlementImage")]
        [HttpPost]
        public ResponseResult SaveCurrencySettlementImage(CurrencySettlementImage currencySettlementImage)
        {
            ResponseResult responseResult = new ResponseResult { status = false, Message = "" };
            try
            {
                var identity = User.Identity as ClaimsIdentity;
                int userid = 0;

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                    userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);


                using (AuthContext context = new AuthContext())
                {
                    CurrencySettlementImages currencySettlementImages = new CurrencySettlementImages();
                    currencySettlementImages.CurrencySettlementSourceId = currencySettlementImage.settlementsourceid;
                    currencySettlementImages.CreatedBy = userid;
                    currencySettlementImages.CreatedDate = indianTime;
                    currencySettlementImages.IsActive = true;
                    currencySettlementImages.IsDeleted = false;
                    currencySettlementImages.SettlementImage = currencySettlementImage.settlementimage;
                    currencySettlementImages.Comment = currencySettlementImage.Comment;
                    currencySettlementImages.SettlementImageType = currencySettlementImage.settlementimagetype;
                    context.CurrencySettlementImages.Add(currencySettlementImages);

                    if (context.Commit() > 0)
                    {
                        responseResult.Message = "Save successfully.";
                        responseResult.status = true;
                    }
                    else
                    {
                        responseResult.Message = "some error occurred during save data.";
                        responseResult.status = false;
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error("Error on SaveCurrencySettlementImage method:" + ex.ToString());
                responseResult.Message = "some error occurred during save data.";
                responseResult.status = false;
            }
            return responseResult;

        }

        [AllowAnonymous]
        [Route("ChequeBounceDetail")]
        [HttpPost]
        public ResponseResult ChequeBounceDetail(ChequeBounceDc chequeBounceDc)
        {
            ResponseResult responseResult = new ResponseResult { status = false, Message = "" };
            var identity = User.Identity as ClaimsIdentity;
            int userid = 0;

            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

            if (chequeBounceDc != null)
            {
                using (AuthContext context = new AuthContext())
                {
                    if (chequeBounceDc.Id > 0)
                    {
                        var BounceImage = "select b.ChequeAmt,b.ChequeNumber,b.Orderid,o.Skcode,o.ShippingAddress Address,o.ShopName PartyName ,b.ReturnComment Reason"
                                          + "     from ChequeCollections b "
                                        + "  inner join OrderMasters o on b.Orderid=o.OrderId "
                                        + "where id=" + chequeBounceDc.Id;


                        var newdata = context.Database.SqlQuery<ChequeBounceDc>(BounceImage).ToList();

                        chequeBounceDc.Address = newdata.Select(x => x.Address).FirstOrDefault();
                        chequeBounceDc.PartyName = newdata.Select(x => x.PartyName).FirstOrDefault();
                        chequeBounceDc.SKCode = newdata.Select(x => x.SKCode).FirstOrDefault();
                        chequeBounceDc.Date = DateTime.Now.ToString("dd/MM/yyyy");



                        string saveFilepath = BounceChequeHelper.Generatepdfbounce(chequeBounceDc, context);
                        if (!string.IsNullOrEmpty(saveFilepath))
                        {
                            var cheque = context.ChequeCollection.FirstOrDefault(x => x.Id == chequeBounceDc.Id);
                            cheque.BounceImage = saveFilepath;
                            cheque.ModifiedBy = userid;
                            cheque.ModifiedDate = DateTime.Now;
                            context.Entry(cheque).State = EntityState.Modified;
                            if (context.Commit() == 0)
                            {
                                logger.Error("Error during Save Bank Withdraw image");
                            }
                        }


                        responseResult.Message = "Bank Withdraw detail & slip generated successfully.";
                        responseResult.status = true;
                    }
                    else
                    {
                        responseResult.Message = "Bank Withdraw detail & slip generated successfully.";
                        responseResult.status = false;
                    }
                }

            }
            else
            {
                responseResult.Message = "Please select at least one cheque to deposit to bank.";
                responseResult.status = false;
            }




            return responseResult;
        }

        [Route("GetCurrencySettlementHistory")]
        [HttpGet]
        public List<CurrencySettlementSourceDc> GetCurrencySettlementHistory(int? warehouseId, DateTime FilterDate)
        {
            List<CurrencySettlementSourceDc> currencySettlementSourceDcs = new List<CurrencySettlementSourceDc>();
            try
            {
                using (AuthContext context = new AuthContext())
                {
                    //
                    if (warehouseId.HasValue)
                    {

                        currencySettlementSourceDcs = context.CurrencySettlementSource.Where(x => x.Warehouseid == warehouseId && EntityFunctions.TruncateTime(x.CreatedDate) == EntityFunctions.TruncateTime(FilterDate)
                                                                                  && x.IsActive && ((x.IsDeleted.HasValue && !x.IsDeleted.Value) || !x.IsDeleted.HasValue)).Select(x =>
                                                                                  new CurrencySettlementSourceDc
                                                                                  {
                                                                                      //WarehouseName=WarehouseName.WarehouseName,
                                                                                      WarehouseId = x.Warehouseid,
                                                                                      CreatedBy = x.CreatedBy,
                                                                                      CreatedDate = x.CreatedDate,
                                                                                      HandOverPerson = x.HandOverPerson,
                                                                                      Id = x.Id,
                                                                                      IsActive = x.IsActive,
                                                                                      Note = x.Note,
                                                                                      SettlementDate = x.SettlementDate,
                                                                                      SettlementSource = x.SettlementSource,
                                                                                      Status = x.Status,
                                                                                      TotalCashAmt = x.TotalCashAmt,
                                                                                      TotalChequeAmt = x.TotalChequeAmt,
                                                                                      SettlementProofImages = x.CurrencySettlementImages.Select(y => new CurrencySettlementImagesDc
                                                                                      {
                                                                                          CreatedBy = y.CreatedBy,
                                                                                          CreatedDate = y.CreatedDate,
                                                                                          CurrencySettlementSourceId = y.CurrencySettlementSourceId,
                                                                                          Id = y.Id,
                                                                                          IsActive = y.IsActive,
                                                                                          IsDeleted = y.IsDeleted,
                                                                                          SettlementImage = y.SettlementImage,
                                                                                          SettlementImageType = y.SettlementImageType
                                                                                      }).ToList()
                                                                                  }).ToList();
                    }
                    else
                    {
                        currencySettlementSourceDcs = context.CurrencySettlementSource.Where(x => EntityFunctions.TruncateTime(x.CreatedDate) == EntityFunctions.TruncateTime(FilterDate)
                                                                                 && x.IsActive && ((x.IsDeleted.HasValue && !x.IsDeleted.Value) || !x.IsDeleted.HasValue)).Select(x =>
                                                                                 new CurrencySettlementSourceDc
                                                                                 {
                                                                                     // WarehouseName = WarehouseName.WarehouseName,
                                                                                     WarehouseId = x.Warehouseid,
                                                                                     CreatedBy = x.CreatedBy,
                                                                                     CreatedDate = x.CreatedDate,
                                                                                     HandOverPerson = x.HandOverPerson,
                                                                                     Id = x.Id,
                                                                                     IsActive = x.IsActive,
                                                                                     Note = x.Note,
                                                                                     SettlementDate = x.SettlementDate,
                                                                                     SettlementSource = x.SettlementSource,
                                                                                     Status = x.Status,
                                                                                     TotalCashAmt = x.TotalCashAmt,
                                                                                     TotalChequeAmt = x.TotalChequeAmt,
                                                                                     SettlementProofImages = x.CurrencySettlementImages.Select(y => new CurrencySettlementImagesDc
                                                                                     {
                                                                                         CreatedBy = y.CreatedBy,
                                                                                         CreatedDate = y.CreatedDate,
                                                                                         CurrencySettlementSourceId = y.CurrencySettlementSourceId,
                                                                                         Id = y.Id,
                                                                                         IsActive = y.IsActive,
                                                                                         IsDeleted = y.IsDeleted,
                                                                                         SettlementImage = y.SettlementImage,
                                                                                         SettlementImageType = y.SettlementImageType
                                                                                     }).ToList()
                                                                                 }).ToList();
                    }


                    if (currencySettlementSourceDcs != null && currencySettlementSourceDcs.Any())
                    {
                        var peoplesids = currencySettlementSourceDcs.Select(x => x.HandOverPerson).Distinct().ToList();
                        var peopeldata = context.Peoples.Where(x => peoplesids.Contains(x.PeopleID)).Select(x => new { x.PeopleID, x.DisplayName });
                        List<int> warehouseIds = currencySettlementSourceDcs.Select(x => x.WarehouseId).Distinct().ToList();
                        var WarehouseNames = context.Warehouses.Where(x => warehouseIds.Contains(x.WarehouseId)).Select(x => new { WarehouseId = x.WarehouseId, WarehouseName = x.WarehouseName + "-" + x.CityName }).ToList();
                        foreach (var item in currencySettlementSourceDcs)
                        {
                            var allSettlementProofImages = item.SettlementProofImages;
                            if (allSettlementProofImages != null && allSettlementProofImages.Any())
                            {
                                item.SlipImages = allSettlementProofImages.Where(x => x.SettlementImageType == "Deposit").ToList();
                                if (item.SlipImages != null && item.SlipImages.Any())
                                {
                                    item.SlipImages.ForEach(x =>
                                    {
                                        x.SettlementImage = HttpContext.Current.Request.UrlReferrer.AbsoluteUri + x.SettlementImage;
                                        x.SettlementImagename = x.SettlementImage.Split('/').Any() ? x.SettlementImage.Split('/').Last() : x.SettlementImagename;
                                    });
                                }

                                item.SettlementProofImages = allSettlementProofImages.Where(x => x.SettlementImageType == "Withdraw").ToList();
                                if (item.SettlementProofImages != null && item.SettlementProofImages.Any())
                                {
                                    item.SettlementProofImages.ForEach(x =>
                                    {
                                        x.SettlementImage = HttpContext.Current.Request.UrlReferrer.AbsoluteUri + x.SettlementImage;
                                        x.SettlementImagename = x.SettlementImage.Split('/').Any() ? x.SettlementImage.Split('/').Last() : x.SettlementImagename;
                                    });

                                }
                            }

                            item.HandOverPersonName = peopeldata != null && peopeldata.Any(x => x.PeopleID == item.HandOverPerson) ? peopeldata.FirstOrDefault(x => x.PeopleID == item.HandOverPerson).DisplayName : "";
                            item.WarehouseName = WarehouseNames != null && WarehouseNames.Any(x => x.WarehouseId == item.WarehouseId) ? WarehouseNames.FirstOrDefault(x => x.WarehouseId == item.WarehouseId).WarehouseName : "";
                        }
                    }
                }

                return currencySettlementSourceDcs;
            }
            catch (Exception ex)
            {
                logger.Error("Error in GetCurrencySettlementSource Method: " + ex.Message);
                return null;
            }
        }

        [Route("BankDepositVerify")]
        [HttpGet]
        public bool BankDepositVerify(long id, int status)
        {
            bool result = false;
            var identity = User.Identity as ClaimsIdentity;
            int compid = 0, userid = 0;

            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))
                compid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);

            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
            using (AuthContext context = new AuthContext())
            {
                var currencysttlement = context.CurrencySettlementSource.FirstOrDefault(x => x.Id == id);
                if (currencysttlement != null)
                {
                    currencysttlement.Status = status;
                    currencysttlement.ModifiedDate = indianTime;
                    currencysttlement.ModifiedBy = userid;
                    //context.CurrencySettlementSource.Attach(currencysttlement);
                    context.Entry(currencysttlement).State = EntityState.Modified;
                    result = context.Commit() > 0;
                }
            }
            return result;
        }
        #endregion

        #region warehouse cash exchange screen method
        [Route("GetExchangeHubCashCollection")]
        [HttpGet]
        public CashExchangeDetailDc GetExchangeHubCashCollection(long currencyHubStockId)
        {
            CashExchangeDetailDc cashExchangeDetailDc = new CashExchangeDetailDc();
            cashExchangeDetailDc.hubCashCollectionDc = new List<hubCashCollectionDc>();
            List<hubCashCollectionDc> hubCashCollectionDc = new List<hubCashCollectionDc>();
            try
            {
                using (AuthContext context = new AuthContext())
                {

                    var predicate = PredicateBuilder.True<HubCashCollection>();
                    predicate = predicate.And(x => x.CurrencyHubStockId == currencyHubStockId && x.IsActive && ((x.IsDeleted.HasValue && !x.IsDeleted.Value) || !x.IsDeleted.HasValue));

                    hubCashCollectionDc = (from c in context.CurrencyDenomination.Where(x => x.IsActive)
                                           join p in context.HubCashCollection.Where(predicate)
                                           on c.Id equals p.CurrencyDenominationId into ps
                                           from p in ps.DefaultIfEmpty()
                                           select new hubCashCollectionDc
                                           {
                                               currencyHubStockId = currencyHubStockId,
                                               CurrencyCount = (p == null ? 0 : p.CurrencyCount) + (p == null ? 0 : p.OpeningCurrencyCount) - ((p == null ? 0 : p.BankSendCurrencyCount)) + (p == null ? 0 : p.ExchangeCurrencyCount),
                                               Id = (p == null ? 0 : p.Id),
                                               CurrencyDenominationId = c.Id,
                                               CurrencyDenominationTitle = c == null ? "" : c.Title,
                                               CurrencyDenominationValue = c == null ? 0 : c.Value,
                                               CashCurrencyType = c == null ? "" : c.currencyType,
                                           }).ToList();
                }
                cashExchangeDetailDc.hubCashCollectionDc = hubCashCollectionDc;
            }
            catch (Exception ex)
            {
                logger.Error("Error in GetExchangeHubCashCollection Method: " + ex.Message);
            }

            return cashExchangeDetailDc;
        }

        [Route("UpdateExchangeHubCashCollection")]
        [HttpPost]
        public bool UpdateExchangeHubCashCollection(CashExchangeDetailDc cashExchangeDetailDc)
        {
            bool result = false;
            try
            {
                List<hubCashCollectionDc> hubCashCollectionDcs = cashExchangeDetailDc.hubCashCollectionDc;
                string comment = cashExchangeDetailDc.Comment;
                var identity = User.Identity as ClaimsIdentity;
                int compid = 0, userid = 0;

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))
                    compid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                    userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

                if (hubCashCollectionDcs != null && hubCashCollectionDcs.Any())
                {
                    var currencyHubStockId = hubCashCollectionDcs.FirstOrDefault().currencyHubStockId;
                    using (AuthContext context = new AuthContext())
                    {
                        var predicate = PredicateBuilder.True<HubCashCollection>();
                        predicate = predicate.And(x => x.CurrencyHubStockId == currencyHubStockId && x.IsActive && ((x.IsDeleted.HasValue && !x.IsDeleted.Value) || !x.IsDeleted.HasValue));
                        var HubCashCollections = context.HubCashCollection.Where(predicate);
                        foreach (var item in hubCashCollectionDcs)
                        {
                            if (HubCashCollections.Any(x => x.CurrencyDenominationId == item.CurrencyDenominationId))
                            {
                                if (item.ExchangeInCurrencyCount > 0 || item.ExchangeOutCurrencyCount > 0)
                                {
                                    var hubcash = HubCashCollections.FirstOrDefault(x => x.CurrencyDenominationId == item.CurrencyDenominationId);
                                    hubcash.ExchangeCurrencyCount = hubcash.ExchangeCurrencyCount + item.ExchangeInCurrencyCount - item.ExchangeOutCurrencyCount;
                                    hubcash.ModifiedBy = userid;
                                    hubcash.ModifiedDate = indianTime;
                                    //context.HubCashCollection.Attach(hubcash);
                                    context.Entry(hubcash).State = EntityState.Modified;
                                    List<ExchangeHubCashCollection> ExhubCashCollections = new List<ExchangeHubCashCollection>();
                                    if (item.ExchangeInCurrencyCount > 0)
                                    {
                                        ExhubCashCollections.Add(new ExchangeHubCashCollection
                                        {
                                            CurrencyCount = item.ExchangeInCurrencyCount,
                                            CurrencyHubStockId = currencyHubStockId,
                                            CurrencyDenominationId = item.CurrencyDenominationId,
                                            CreatedBy = userid,
                                            CreatedDate = indianTime,
                                            IsActive = true,
                                            IsDeleted = false,
                                            Comment = comment
                                        });
                                    }
                                    if (item.ExchangeOutCurrencyCount > 0)
                                    {
                                        ExhubCashCollections.Add(new ExchangeHubCashCollection
                                        {
                                            CurrencyCount = (-1) * item.ExchangeOutCurrencyCount,
                                            CurrencyHubStockId = currencyHubStockId,
                                            CurrencyDenominationId = item.CurrencyDenominationId,
                                            CreatedBy = userid,
                                            CreatedDate = indianTime,
                                            IsActive = true,
                                            IsDeleted = false,
                                            Comment = comment
                                        });
                                    }
                                    context.ExchangeHubCashCollection.AddRange(ExhubCashCollections);
                                }
                            }
                            else
                            {
                                if (item.ExchangeInCurrencyCount > 0)
                                {
                                    HubCashCollection hubCashCollection = new HubCashCollection();
                                    hubCashCollection.OpeningCurrencyCount = 0;
                                    hubCashCollection.CurrencyCount = 0;
                                    hubCashCollection.BankSendCurrencyCount = 0;
                                    hubCashCollection.ExchangeCurrencyCount = item.ExchangeInCurrencyCount;
                                    hubCashCollection.CurrencyDenominationId = item.CurrencyDenominationId;
                                    hubCashCollection.CreatedBy = userid;
                                    hubCashCollection.CreatedDate = indianTime;
                                    hubCashCollection.IsActive = true;
                                    hubCashCollection.IsDeleted = false;
                                    hubCashCollection.CurrencyHubStockId = currencyHubStockId;
                                    context.HubCashCollection.Add(hubCashCollection);

                                    List<ExchangeHubCashCollection> ExhubCashCollections = new List<ExchangeHubCashCollection>();
                                    if (item.ExchangeInCurrencyCount > 0)
                                    {
                                        ExhubCashCollections.Add(new ExchangeHubCashCollection
                                        {
                                            CurrencyCount = item.ExchangeInCurrencyCount,
                                            CurrencyHubStockId = currencyHubStockId,
                                            CurrencyDenominationId = item.CurrencyDenominationId,
                                            CreatedBy = userid,
                                            CreatedDate = indianTime,
                                            IsActive = true,
                                            IsDeleted = false,
                                            Comment = comment
                                        });
                                    }
                                    context.ExchangeHubCashCollection.AddRange(ExhubCashCollections);
                                }
                            }
                        }

                        result = context.Commit() > 0;
                    }
                }


            }
            catch (Exception ex)
            {
                logger.Error("Error in GetExchangeHubCashCollection Method: " + ex.Message);
                result = false;
            }
            return result;
        }


        [Route("GetExchangeCashHistory")]
        [HttpGet]
        public ExchangeCashDc GetExchangeCashHistory(int? warehouseid, DateTime Filterdate)
        {
            ExchangeCashDc exchangeCashDc = new ExchangeCashDc();
            exchangeCashDc.hubCashCollectionDcs = new List<hubCashCollectionDc>();
            exchangeCashDc.ExchangeCommentDcs = new List<ExchangeCommentDc>();
            if (warehouseid == 0)
            {
                warehouseid = null;
            }
            try
            {
                using (AuthContext context = new AuthContext())
                {

                    List<hubCashCollectionDc> hubCashCollectionDcs = new List<hubCashCollectionDc>();
                    List<ExchangeStockDc> CurrencyHubStocks = new List<ExchangeStockDc>();
                    List<ExchangeHubCashCollection> exchange = new List<ExchangeHubCashCollection>();
                    var currency = context.CurrencyDenomination.Where(x => x.IsActive).ToList();
                    if (warehouseid.HasValue)
                    {
                        var predicate = PredicateBuilder.True<ExchangeHubCashCollection>();
                        predicate = predicate.And(o => EntityFunctions.TruncateTime(o.CreatedDate) == EntityFunctions.TruncateTime(Filterdate)
                               && o.IsActive && ((o.IsDeleted.HasValue && !o.IsDeleted.Value) || !o.IsDeleted.HasValue));
                        if (warehouseid > 0)
                        {
                            predicate = predicate.And(x => x.CurrencyHubStock.Warehouseid == warehouseid);
                        }
                        exchange = context.ExchangeHubCashCollection.Where(predicate).ToList();

                        // var warehouse=context.Warehouses.Where(x=>x.WarehouseId==warehouseid).Select(x=>x.).ToList();
                        if (exchange.Any(x => x.CurrencyCount > 0))
                        {
                            foreach (var item in exchange.Where(x => x.CurrencyCount > 0).GroupBy(x => x.CreatedDate))
                            {
                                var hubCashInCollectionDcs = currency.Select(c => new hubCashCollectionDc
                                {
                                    CurrencyDenominationId = c.Id,
                                    CurrencyCount = item.Where(x => x.CurrencyDenominationId == c.Id && x.CurrencyCount > 0).Sum(p => p.CurrencyCount),
                                    CurrencyDenominationTitle = c.Title,
                                    CurrencyDenominationValue = c.Value,
                                    CashCurrencyType = c.currencyType,
                                    ExchangeComment = item != null && item.Any(x => x.CurrencyDenominationId == c.Id && x.CurrencyCount > 0) ? item.FirstOrDefault(x => x.CurrencyDenominationId == c.Id && x.CurrencyCount > 0).Comment : "",
                                    CreateDate = item != null && item.Any(x => x.CurrencyDenominationId == c.Id && x.CurrencyCount > 0) ? item.FirstOrDefault(x => x.CurrencyDenominationId == c.Id && x.CurrencyCount > 0).CreatedDate : (DateTime?)null,
                                    currencyHubStockId = item.FirstOrDefault().CurrencyHubStockId > 0 ? item.FirstOrDefault().CurrencyHubStockId : 0,
                                }).ToList();
                                hubCashCollectionDcs.AddRange(hubCashInCollectionDcs);
                            }
                        }
                        if (exchange.Any(x => x.CurrencyCount < 0))
                        {
                            foreach (var item in exchange.Where(x => x.CurrencyCount < 0).GroupBy(x => x.CreatedDate))
                            {
                                var hubCashOutCollectionDcs = currency.Select(c => new hubCashCollectionDc
                                {
                                    CurrencyDenominationId = c.Id,
                                    CurrencyCount = item.Where(x => x.CurrencyDenominationId == c.Id && x.CurrencyCount < 0).Sum(p => p.CurrencyCount),
                                    CurrencyDenominationTitle = c.Title,
                                    CurrencyDenominationValue = c.Value,
                                    CashCurrencyType = c.currencyType,
                                    ExchangeComment = item != null && item.Any(x => x.CurrencyDenominationId == c.Id && x.CurrencyCount < 0) ? item.FirstOrDefault(x => x.CurrencyDenominationId == c.Id && x.CurrencyCount < 0).Comment : "",
                                    CreateDate = item != null && item.Any(x => x.CurrencyDenominationId == c.Id && x.CurrencyCount < 0) ? item.FirstOrDefault(x => x.CurrencyDenominationId == c.Id && x.CurrencyCount < 0).CreatedDate : (DateTime?)null,
                                    currencyHubStockId = item.FirstOrDefault().CurrencyHubStockId > 0 ? item.FirstOrDefault().CurrencyHubStockId : 0,
                                }).ToList();
                                hubCashCollectionDcs.AddRange(hubCashOutCollectionDcs);
                            }
                        }

                        if (exchange.Count() == 0)
                        {
                            hubCashCollectionDcs = currency.Select(c => new hubCashCollectionDc
                            {
                                CurrencyDenominationId = c.Id,
                                CurrencyCount = 0,
                                CurrencyDenominationTitle = c.Title,
                                CurrencyDenominationValue = c.Value,
                                CashCurrencyType = c.currencyType,
                                ExchangeComment = "",
                                CreateDate = (DateTime?)null,
                            }).ToList();
                        }
                    }
                    else
                    {
                        exchange = context.ExchangeHubCashCollection.Where(o => /*o.CurrencyHubStock.Warehouseid == warehouseid &&*/
                              EntityFunctions.TruncateTime(o.CreatedDate) == EntityFunctions.TruncateTime(Filterdate)
                             && o.IsActive && ((o.IsDeleted.HasValue && !o.IsDeleted.Value) || !o.IsDeleted.HasValue)).ToList();
                        var CurrencyHubStockIds = exchange != null && exchange.Any() ? exchange.Select(x => x.CurrencyHubStockId).Distinct().ToList() : new List<long>();
                        CurrencyHubStocks = context.CurrencyHubStock.Where(x => CurrencyHubStockIds.Contains(x.Id)).Select(x => new ExchangeStockDc { Id = x.Id, WarehouseId = x.Warehouseid }).ToList();

                        if (exchange.Any(x => x.CurrencyCount > 0))
                        {

                            foreach (var item in exchange.Where(x => x.CurrencyCount > 0).GroupBy(x => x.CreatedDate))
                            {
                                var hubCashInCollectionDcs = currency.Select(c => new hubCashCollectionDc
                                {
                                    CurrencyDenominationId = c.Id,
                                    CurrencyCount = item.Where(x => x.CurrencyDenominationId == c.Id && x.CurrencyCount > 0).Sum(p => p.CurrencyCount),
                                    CurrencyDenominationTitle = c.Title,
                                    CurrencyDenominationValue = c.Value,
                                    CashCurrencyType = c.currencyType,
                                    ExchangeComment = item != null && item.Any(x => x.CurrencyDenominationId == c.Id && x.CurrencyCount > 0) ? item.FirstOrDefault(x => x.CurrencyDenominationId == c.Id && x.CurrencyCount > 0).Comment : "",
                                    CreateDate = item != null && item.Any(x => x.CurrencyDenominationId == c.Id && x.CurrencyCount > 0) ? item.FirstOrDefault(x => x.CurrencyDenominationId == c.Id && x.CurrencyCount > 0).CreatedDate : (DateTime?)null,
                                    currencyHubStockId = item.FirstOrDefault().CurrencyHubStockId > 0 ? item.FirstOrDefault().CurrencyHubStockId : 0,
                                }).ToList();
                                hubCashCollectionDcs.AddRange(hubCashInCollectionDcs);
                            }
                        }
                        if (exchange.Any(x => x.CurrencyCount < 0))
                        {
                            foreach (var item in exchange.Where(x => x.CurrencyCount < 0).GroupBy(x => x.CreatedDate))
                            {
                                var hubCashOutCollectionDcs = currency.Select(c => new hubCashCollectionDc
                                {
                                    CurrencyDenominationId = c.Id,
                                    CurrencyCount = item.Where(x => x.CurrencyDenominationId == c.Id && x.CurrencyCount < 0).Sum(p => p.CurrencyCount),
                                    CurrencyDenominationTitle = c.Title,
                                    CurrencyDenominationValue = c.Value,
                                    CashCurrencyType = c.currencyType,
                                    ExchangeComment = item != null && item.Any(x => x.CurrencyDenominationId == c.Id && x.CurrencyCount < 0) ? item.FirstOrDefault(x => x.CurrencyDenominationId == c.Id && x.CurrencyCount < 0).Comment : "",
                                    CreateDate = item != null && item.Any(x => x.CurrencyDenominationId == c.Id && x.CurrencyCount < 0) ? item.FirstOrDefault(x => x.CurrencyDenominationId == c.Id && x.CurrencyCount < 0).CreatedDate : (DateTime?)null,
                                    currencyHubStockId = item.FirstOrDefault().CurrencyHubStockId > 0 ? item.FirstOrDefault().CurrencyHubStockId : 0,
                                }).ToList();
                                hubCashCollectionDcs.AddRange(hubCashOutCollectionDcs);
                            }
                        }

                        if (exchange.Count() == 0)
                        {
                            hubCashCollectionDcs = currency.Select(c => new hubCashCollectionDc
                            {
                                CurrencyDenominationId = c.Id,
                                CurrencyCount = 0,
                                CurrencyDenominationTitle = c.Title,
                                CurrencyDenominationValue = c.Value,
                                CashCurrencyType = c.currencyType,
                                ExchangeComment = "",
                                CreateDate = (DateTime?)null,
                            }).ToList();
                        }
                    }
                    List<hubCashCollectionDc> BankCashCollectionDcs = new List<hubCashCollectionDc>();
                    if (hubCashCollectionDcs != null && hubCashCollectionDcs.Any())
                    {
                        var CurrencyHubStockIds = exchange != null && exchange.Any() ? exchange.Select(x => x.CurrencyHubStockId).Distinct().ToList() : new List<long>();
                        CurrencyHubStocks = context.CurrencyHubStock.Where(x => CurrencyHubStockIds.Contains(x.Id)).Select(x => new ExchangeStockDc { Id = x.Id, WarehouseId = x.Warehouseid }).ToList();

                        if (warehouseid.HasValue)
                        {
                            var bankCashCollection = context.CashSettlement.Where(o => o.CurrencySettlementSource.Warehouseid == warehouseid
                             && EntityFunctions.TruncateTime(o.CreatedDate) == EntityFunctions.TruncateTime(Filterdate)
                             && o.IsActive
                             && ((o.IsDeleted.HasValue && !o.IsDeleted.Value) || !o.IsDeleted.HasValue)).ToList();

                            BankCashCollectionDcs = currency.Select(c => new hubCashCollectionDc
                            {
                                CurrencyDenominationId = c.Id,
                                BankDepositCurrencyCount = bankCashCollection == null && bankCashCollection.Any() ? bankCashCollection.Sum(p => p.CurrencyCount) : 0,
                                CurrencyDenominationTitle = c.Title,
                                CurrencyDenominationValue = c.Value,
                                CashCurrencyType = c.currencyType,
                                CreateDate = bankCashCollection == null && bankCashCollection.Any() ? bankCashCollection.FirstOrDefault().CreatedDate : (DateTime?)null,
                            }).ToList();
                        }
                        else
                        {
                            var bankCashCollection = context.CashSettlement.Where(o => EntityFunctions.TruncateTime(o.CreatedDate) == EntityFunctions.TruncateTime(Filterdate)
                            && o.IsActive
                            && ((o.IsDeleted.HasValue && !o.IsDeleted.Value) || !o.IsDeleted.HasValue)).ToList();

                            BankCashCollectionDcs = currency.Select(c => new hubCashCollectionDc
                            {
                                CurrencyDenominationId = c.Id,
                                BankDepositCurrencyCount = bankCashCollection == null && bankCashCollection.Any() ? bankCashCollection.Sum(p => p.CurrencyCount) : 0,
                                CurrencyDenominationTitle = c.Title,
                                CurrencyDenominationValue = c.Value,
                                CashCurrencyType = c.currencyType,
                                CreateDate = bankCashCollection == null && bankCashCollection.Any() ? bankCashCollection.FirstOrDefault().CreatedDate : (DateTime?)null,
                            }).ToList();
                        }

                    }

                    if (hubCashCollectionDcs != null && hubCashCollectionDcs.Any())
                    {
                        exchangeCashDc.hubCashCollectionDcs = hubCashCollectionDcs.GroupBy(x => new { x.CurrencyDenominationId, x.CurrencyDenominationTitle, x.CurrencyDenominationValue, x.CashCurrencyType })
                           .Select(x => new hubCashCollectionDc
                           {
                               CurrencyDenominationId = x.Key.CurrencyDenominationId,
                               ExchangeInCurrencyCount = x.Any(y => y.CurrencyCount > 0) ? x.Where(y => y.CurrencyCount > 0).Sum(y => y.CurrencyCount) : 0,
                               ExchangeOutCurrencyCount = x.Any(y => y.CurrencyCount < 0) ? x.Where(y => y.CurrencyCount < 0).Sum(y => y.CurrencyCount) : 0,
                               BankDepositCurrencyCount = BankCashCollectionDcs.Any(y => y.CurrencyDenominationId == x.Key.CurrencyDenominationId) ? BankCashCollectionDcs.Where(y => y.CurrencyDenominationId == x.Key.CurrencyDenominationId).Sum(y => y.BankDepositCurrencyCount) : 0,
                               CurrencyDenominationTitle = x.Key.CurrencyDenominationTitle,
                               CurrencyDenominationValue = x.Key.CurrencyDenominationValue,
                               CashCurrencyType = x.Key.CashCurrencyType,
                               CreateDate = x.FirstOrDefault().CreateDate,
                               ExchangeComment = x.FirstOrDefault().ExchangeComment
                           }).ToList();

                        var exchangehubcash = hubCashCollectionDcs.GroupBy(x => new { x.CurrencyDenominationId, x.CurrencyDenominationTitle, x.CurrencyDenominationValue, x.CashCurrencyType, x.CreateDate, x.currencyHubStockId })
                           .Select(x => new hubCashCollectionDc
                           {
                               CurrencyDenominationId = x.Key.CurrencyDenominationId,
                               ExchangeInCurrencyCount = x.Any(y => y.CurrencyCount > 0) ? x.Where(y => y.CurrencyCount > 0).Sum(y => y.CurrencyCount) : 0,
                               ExchangeOutCurrencyCount = x.Any(y => y.CurrencyCount < 0) ? x.Where(y => y.CurrencyCount < 0).Sum(y => y.CurrencyCount) : 0,
                               CurrencyDenominationTitle = x.Key.CurrencyDenominationTitle,
                               CurrencyDenominationValue = x.Key.CurrencyDenominationValue,
                               CashCurrencyType = x.Key.CashCurrencyType,
                               CreateDate = x.Key.CreateDate,
                               currencyHubStockId = x.Key.currencyHubStockId,
                               ExchangeComment = x.FirstOrDefault().ExchangeComment,
                               Warehouseid = CurrencyHubStocks.Any(y => y.Id == x.Key.currencyHubStockId) ? CurrencyHubStocks.FirstOrDefault(y => y.Id == x.Key.currencyHubStockId).WarehouseId : 0
                           }).ToList();
                        List<int> wIds = exchangehubcash.Select(x => x.Warehouseid).Distinct().ToList();
                        var warehouses = context.Warehouses.Where(y => wIds.Contains(y.WarehouseId)).Select(j => new { j.WarehouseId, j.WarehouseName }).ToList();
                        exchangeCashDc.ExchangeCommentDcs = exchangehubcash.Where(x => x.CreateDate.HasValue).GroupBy(x => new { x.CreateDate, x.currencyHubStockId }).Select(x =>
                          new ExchangeCommentDc
                          {
                              //WarehouseName = warehouse.WarehouseName,
                              WarehouseName = x.FirstOrDefault().Warehouseid > 0 ? warehouses.FirstOrDefault(y => y.WarehouseId == x.FirstOrDefault().Warehouseid).WarehouseName : "",
                              comment = x.FirstOrDefault().ExchangeComment,
                              TotalInAmount = x.Sum(y => y.ExchangeInCurrencyCount * y.CurrencyDenominationValue),
                              TotalOutAmount = x.Sum(y => y.ExchangeOutCurrencyCount * y.CurrencyDenominationValue),
                              ExchangeDate = x.Key.CreateDate.Value

                          }).OrderBy(x => x.ExchangeDate).ToList();

                        //if (exchangeCashDc != null)
                        //{
                        //    var warehousesids = exchangeCashDc.ExchangeCommentDcs.Select(x => x.WarehouseId).ToList();
                        //    var warehouseData = context.Warehouses.Where(x => warehousesids.Contains(x.WarehouseId)).Select(x => new { x.WarehouseId, x.WarehouseName });

                        //    foreach (var item in exchangeCashDc.ExchangeCommentDcs)
                        //    {

                        //        item.WarehouseName = warehouseData != null && warehouseData.Any(x => x.WarehouseId == item.WarehouseId) ? warehouseData.FirstOrDefault(x => x.WarehouseId == item.WarehouseId).WarehouseName : "";

                        //    }
                        //}
                    }

                }
            }
            catch (Exception ex)
            {
                logger.Error("Error in GetExchangeCashHistory Method: " + ex.Message);
                return null;
            }

            return exchangeCashDc;
        }
        #endregion
        [Route("CurrencySettlementImageUpload")]
        [HttpPost]
        [AllowAnonymous]
        public string CurrencySettlementImageUpload()
        {
            string LogoUrl = "";
            try
            {
                var identity = User.Identity as ClaimsIdentity;
                int compid = 0, userid = 0;
                string warehouseid = "";

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "Warehouseid"))
                    warehouseid = identity.Claims.FirstOrDefault(x => x.Type == "Warehouseid").Value;

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                    userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

                if (HttpContext.Current.Request.Files.AllKeys.Any())
                {
                    var httpPostedFile = HttpContext.Current.Request.Files["file"];

                    if (httpPostedFile != null)
                    {
                        if (!Directory.Exists(HttpContext.Current.Server.MapPath("~/CurrencySettlementImage")))
                            Directory.CreateDirectory(HttpContext.Current.Server.MapPath("~/CurrencySettlementImage"));

                        string extension = Path.GetExtension(httpPostedFile.FileName);

                        string fileName = httpPostedFile.FileName.Substring(0, httpPostedFile.FileName.LastIndexOf('.')) + DateTime.Now.ToString("ddMMyyyyHHmmss") + extension;

                        LogoUrl = Path.Combine(HttpContext.Current.Server.MapPath("~/CurrencySettlementImage"), fileName);

                        httpPostedFile.SaveAs(LogoUrl);

                        AngularJSAuthentication.Common.Helpers.FileUploadHelper.Upload(httpPostedFile.FileName, "~/CurrencySettlementImage", LogoUrl);

                        LogoUrl = "/CurrencySettlementImage/" + fileName;
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error("Error in GetExchangeHubCashCollection Method: " + ex.Message);
            }
            return LogoUrl;
        }

        [Route("CurrencyUploadChequeImage")]
        [HttpPost]
        [AllowAnonymous]
        public string CurrencyUploadChequeImage()
        {
            string LogoUrl = "";
            try
            {
                var identity = User.Identity as ClaimsIdentity;
                int compid = 0, userid = 0;

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))
                    compid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                    userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

                if (HttpContext.Current.Request.Files.AllKeys.Any())
                {
                    var httpPostedFile = HttpContext.Current.Request.Files["file"];
                    if (httpPostedFile != null)
                    {
                        if (!Directory.Exists(HttpContext.Current.Server.MapPath("~/CurrencyChequeImage")))
                            Directory.CreateDirectory(HttpContext.Current.Server.MapPath("~/CurrencyChequeImage"));

                        string extension = Path.GetExtension(httpPostedFile.FileName);

                        string fileName = httpPostedFile.FileName.Substring(0, httpPostedFile.FileName.LastIndexOf('.')) + DateTime.Now.ToString("ddMMyyyyHHmmss") + extension;



                        LogoUrl = Path.Combine(HttpContext.Current.Server.MapPath("~/CurrencyChequeImage"), fileName);

                        httpPostedFile.SaveAs(LogoUrl);

                        AngularJSAuthentication.Common.Helpers.FileUploadHelper.Upload(httpPostedFile.FileName, "~/CurrencyChequeImage", LogoUrl);

                        LogoUrl = "/CurrencyChequeImage/" + fileName;
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error("Error in GetExchangeHubCashCollection Method: " + ex.Message);
            }
            return LogoUrl;
        }

        [Route("CurrencyUploadedChequeImage")]
        [HttpPost]
        [AllowAnonymous]
        public IHttpActionResult CurrencyUploadedChequeImage()
        {
            string LogoUrl = "";
            try
            {
                var identity = User.Identity as ClaimsIdentity;
                int compid = 0, userid = 0;

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))
                    compid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                    userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

                if (HttpContext.Current.Request.Files.AllKeys.Any())
                {
                    var httpPostedFile = HttpContext.Current.Request.Files["file"];
                    if (httpPostedFile != null)
                    {
                        if (!Directory.Exists(HttpContext.Current.Server.MapPath("~/CurrencyChequeImage")))
                            Directory.CreateDirectory(HttpContext.Current.Server.MapPath("~/CurrencyChequeImage"));

                        string extension = Path.GetExtension(httpPostedFile.FileName);

                        string fileName = httpPostedFile.FileName.Substring(0, httpPostedFile.FileName.LastIndexOf('.')) + DateTime.Now.ToString("ddMMyyyyHHmmss") + extension;



                        LogoUrl = Path.Combine(HttpContext.Current.Server.MapPath("~/CurrencyChequeImage"), fileName);

                        httpPostedFile.SaveAs(LogoUrl);

                        AngularJSAuthentication.Common.Helpers.FileUploadHelper.Upload(httpPostedFile.FileName, "~/CurrencyChequeImage", LogoUrl);

                        LogoUrl = "/CurrencyChequeImage/" + fileName;
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error("Error in GetExchangeHubCashCollection Method: " + ex.Message);
            }
            return Created(LogoUrl, LogoUrl);
        }


        [Route("UpdateChequeStatus")]
        [HttpGet]
        public bool UpdateChequeStatus(long ChequeCollectionId, int chequestatus, string ReturnComment, string cancelStatus)
        {
            bool result = false;
            try
            {
                var identity = User.Identity as ClaimsIdentity;
                int compid = 0, userid = 0;

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))
                    compid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                    userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
                using (AuthContext context = new AuthContext())
                {
                    var chequeCollection = context.ChequeCollection.FirstOrDefault(x => x.Id == ChequeCollectionId && x.ChequeStatus != 6);
                    if (chequeCollection != null)
                    {
                        int? previousChequeStatus = chequeCollection.ChequeStatus;
                        chequeCollection.ChequeStatus = chequestatus;
                        chequeCollection.ModifiedBy = userid;
                        chequeCollection.ModifiedDate = indianTime;
                        //if (ReturnComment == null || cancelStatus == null)
                        //{

                        //}

                        if (ReturnComment != "null" && ReturnComment != "undefined")
                        {
                            chequeCollection.ReturnComment = ReturnComment;
                        }
                        if (cancelStatus != null && cancelStatus != "Other" && cancelStatus != "undefined")
                        {
                            chequeCollection.ReturnComment = cancelStatus;
                        }

                        //context.ChequeCollection.Attach(chequeCollection);
                        context.Entry(chequeCollection).State = EntityState.Modified;
                        int amount = 300;
                        long currencyCollectionId = chequeCollection.CurrencyCollectionId.HasValue && chequeCollection.CurrencyCollectionId.Value > 0 ? chequeCollection.CurrencyCollectionId.Value : 0;
                        CurrencyCollection currencyCollection = currencyCollectionId > 0 ? context.CurrencyCollection.FirstOrDefault(x => x.Id == currencyCollectionId) : null;

                        if (chequeCollection.ChequeAmt >= 10000)
                        {
                            amount = 500;
                        }
                        if (chequestatus == 4)
                        {
                            decimal Chequeamount = context.ChequeCollection.Where(x => x.Id == ChequeCollectionId).Select(x => x.ChequeAmt).FirstOrDefault();
                            chequeCollection.Fine = chequeCollection.Fine + amount;
                            chequeCollection.Fine = chequeCollection.Fine < 0 ? 0 : chequeCollection.Fine;
                            context.Entry(chequeCollection).State = EntityState.Modified;
                            if (currencyCollection != null)
                            {
                                currencyCollection.Fine = currencyCollection.Fine + amount;
                                currencyCollection.Fine = currencyCollection.Fine < 0 ? 0 : currencyCollection.Fine;
                                if (Chequeamount > 0)
                                {
                                    currencyCollection.TotalDueAmt += Chequeamount;
                                }
                                //currencyCollection.TotalDueAmt += chequeCollection.ChequeAmt;
                                if (currencyCollection.TotalCheckAmt > 0)
                                {
                                    currencyCollection.TotalCheckAmt -= Chequeamount;
                                }
                                context.Entry(currencyCollection).State = EntityState.Modified;
                            }

                            var exists = context.ReturnChequeCollection.Any(x => x.ChequeCollectionId == chequeCollection.Id);
                            if (!exists)
                            {
                                ReturnChequeCollection ReturnChequeCollection = new ReturnChequeCollection
                                {
                                    ChequeCollectionId = chequeCollection.Id,
                                    Status = Convert.ToInt32(ChequeReturnStatusEnum.Return),
                                    CreatedBy = userid,
                                    CreatedDate = indianTime,
                                    IsActive = true,
                                    IsDeleted = false,
                                    Fine = chequeCollection.Fine
                                };
                                context.ReturnChequeCollection.Add(ReturnChequeCollection);
                            }
                            else
                            {
                                var returncheque = context.ReturnChequeCollection.FirstOrDefault(x => x.ChequeCollectionId == chequeCollection.Id && x.IsActive && ((x.IsDeleted.HasValue && !x.IsDeleted.Value) || !x.IsDeleted.HasValue));
                                returncheque.IsActive = false;
                                returncheque.IsDeleted = true;
                                returncheque.ModifiedBy = userid;
                                returncheque.ModifiedDate = indianTime;
                                returncheque.Fine = currencyCollection.Fine;
                                context.Entry(chequeCollection).State = EntityState.Modified;

                                ReturnChequeCollection ReturnChequeCollection = new ReturnChequeCollection
                                {
                                    ChequeCollectionId = chequeCollection.Id,
                                    Status = Convert.ToInt32(ChequeReturnStatusEnum.Return),
                                    CreatedBy = userid,
                                    CreatedDate = indianTime,
                                    IsActive = true,
                                    IsDeleted = false,
                                    Fine = chequeCollection.Fine
                                };
                                context.ReturnChequeCollection.Add(ReturnChequeCollection);
                            }

                            if (chequeCollection.Orderid > 0 && !string.IsNullOrEmpty(chequeCollection.ChequeNumber))
                            {
                                var paymentResponse = context.PaymentResponseRetailerAppDb.FirstOrDefault(x => x.OrderId == chequeCollection.Orderid && x.PaymentFrom == "Cheque" && x.GatewayTransId == chequeCollection.ChequeNumber && x.status == "Success");
                                if (paymentResponse != null)
                                {
                                    paymentResponse.UpdatedDate = DateTime.Now;
                                    paymentResponse.status = "Bounce";
                                    context.Entry(paymentResponse).State = EntityState.Modified;
                                }
                                var Deliverymaster = context.OrderDispatchedMasters.FirstOrDefault(x => x.OrderId == chequeCollection.Orderid && x.CheckNo == chequeCollection.ChequeNumber);
                                {
                                    if (Deliverymaster != null)
                                    {
                                        Deliverymaster.UpdatedDate = DateTime.Now;
                                        Deliverymaster.CheckAmount = 0;
                                        Deliverymaster.CheckNo = null;
                                        context.Entry(Deliverymaster).State = EntityState.Modified;
                                    }
                                }

                                string msg = "Cheque No. " + chequeCollection.ChequeNumber + " for Rs " + chequeCollection.ChequeAmt + "/- dated " + chequeCollection.ChequeDate.ToString("MM/dd/yyyy") + " issued to ShopKirana E Trading Pvt Ltd is return due to " + chequeCollection.ReturnComment + "";
                                var customermobileno = context.DbOrderMaster.Where(x => x.OrderId == chequeCollection.Orderid).Select(x => x.Customerphonenum).FirstOrDefault();
                                //if (!string.IsNullOrEmpty(customermobileno)) /// Neet to approve template 
                                //    GetSmsNotification(customermobileno, msg);
                            }
                            #region  Agent Entry for Cheque Return
                            //var assignment = context.DeliveryIssuanceDb.FirstOrDefault(a =>
                            //    !context.CurrencyCollection.Any(x => x.Deliveryissueid == a.DeliveryIssuanceId && x.Id == chequeCollection.CurrencyCollectionId
                            //    && x.IsActive && ((x.IsDeleted.HasValue && !x.IsDeleted.Value) || !x.IsDeleted.HasValue)));
                            var query = from di in context.DeliveryIssuanceDb
                                        join cc in context.CurrencyCollection
                                        on di.DeliveryIssuanceId equals cc.Deliveryissueid
                                        join cheqc in context.ChequeCollection
                                        on cc.Id equals cheqc.CurrencyCollectionId
                                        where cheqc.Id == ChequeCollectionId
                                        select di;
                            var assignment = query.FirstOrDefault();

                            BackgroundTaskManager.Run(() =>
                            {
                                AgentLedgerHelper agentLedgerHelper = new AgentLedgerHelper();
                                ChequeCollectionDc chequeDc = new ChequeCollectionDc
                                {
                                    AgentId = chequeCollection.AgentId,
                                    BankSubmitDate = chequeCollection.BankSubmitDate,
                                    ChequeAmt = chequeCollection.ChequeAmt,
                                    ChequeBankName = chequeCollection.ChequeBankName,
                                    ChequeDate = chequeCollection.ChequeDate,
                                    ChequeimagePath = chequeCollection.ChequeimagePath,
                                    ChequeNumber = chequeCollection.ChequeNumber,
                                    ChequeStatus = chequeCollection.ChequeStatus,
                                    CurrencyCollectionId = chequeCollection.CurrencyCollectionId,
                                    CurrencySettlementid = chequeCollection.CurrencyHubStockId,
                                    OrderId = chequeCollection.Orderid,
                                    ReturnComment = chequeCollection.ReturnComment,
                                };
                                BackgroundJob.Enqueue(() => agentLedgerHelper.OnChequeCancel(assignment, userid, amount, chequeDc));
                            });


                            //BackgroundTaskManager.Run(() =>
                            // {
                            //     AgentLedgerHelper agentLedgerHelper = new AgentLedgerHelper();
                            //     CustomerChequeBounceLedgerHelper customerChequeBounceLedgerHelper = new CustomerChequeBounceLedgerHelper();
                            //     if (assignment != null && assignment.AgentId > 0)
                            //     {
                            //         agentLedgerHelper.OnChequeCancel(assignment.AgentId, Convert.ToDouble(chequeCollection.ChequeAmt), chequeCollection.ChequeNumber, userid, assignment.DeliveryIssuanceId, chequeCollection.Orderid.ToString());
                            //         agentLedgerHelper.OnChequeFine(assignment.AgentId, amount, chequeCollection.ChequeNumber, userid, assignment.DeliveryIssuanceId, chequeCollection.Orderid.ToString());

                            //     }
                            //     if (chequeCollection.Orderid > 0 && !string.IsNullOrEmpty(chequeCollection.ChequeNumber))
                            //     {
                            //         List<AngularJSAuthentication.DataContracts.Transaction.Ledger.ChequeBounceVM> chequeBounceVM = new List<AngularJSAuthentication.DataContracts.Transaction.Ledger.ChequeBounceVM> {
                            //              new AngularJSAuthentication.DataContracts.Transaction.Ledger.ChequeBounceVM{
                            //               Amount=Convert.ToDouble(chequeCollection.ChequeAmt),
                            //               ChequeNumber=chequeCollection.ChequeNumber,
                            //               Date=chequeCollection.ChequeDate
                            //              }
                            //             };
                            //         customerChequeBounceLedgerHelper.OnBounce(chequeCollection.Orderid, chequeBounceVM, amount, userid);
                            //     }
                            // });
                            #endregion



                        }
                        else if (previousChequeStatus == 4 && chequestatus != 4)
                        {
                            chequeCollection.Fine = chequeCollection.Fine - amount;
                            chequeCollection.Fine = chequeCollection.Fine < 0 ? 0 : chequeCollection.Fine;
                            chequeCollection.ReturnComment = null;
                            context.Entry(chequeCollection).State = EntityState.Modified;


                            if (currencyCollection != null)
                            {
                                currencyCollection.Fine = currencyCollection.Fine - amount;
                                currencyCollection.Fine = currencyCollection.Fine < 0 ? 0 : currencyCollection.Fine;
                                currencyCollection.TotalDueAmt -= chequeCollection.ChequeAmt;
                                context.Entry(currencyCollection).State = EntityState.Modified;

                            }
                            var returncheque = context.ReturnChequeCollection.FirstOrDefault(x => x.ChequeCollectionId == chequeCollection.Id && x.IsActive && ((x.IsDeleted.HasValue && !x.IsDeleted.Value) || !x.IsDeleted.HasValue));
                            if (returncheque != null)
                            {
                                returncheque.IsActive = false;
                                returncheque.IsDeleted = true;
                                returncheque.ModifiedBy = userid;
                                returncheque.ModifiedDate = indianTime;
                                context.Entry(chequeCollection).State = EntityState.Modified;

                                #region  Agent Entry for Revert Cheque Return

                                var query = from di in context.DeliveryIssuanceDb
                                            join cc in context.CurrencyCollection
                                            on di.DeliveryIssuanceId equals cc.Deliveryissueid
                                            join cheqc in context.ChequeCollection
                                            on cc.Id equals cheqc.CurrencyCollectionId
                                            where cheqc.Id == ChequeCollectionId
                                            select di;
                                var assignment = query.FirstOrDefault();

                                if (chequeCollection.Orderid > 0 && !string.IsNullOrEmpty(chequeCollection.ChequeNumber))
                                {
                                    var paymentResponse = context.PaymentResponseRetailerAppDb.FirstOrDefault(x => x.OrderId == chequeCollection.Orderid && x.PaymentFrom == "Cheque" && x.GatewayTransId == chequeCollection.ChequeNumber && x.status == "Bounce");
                                    if (paymentResponse != null)
                                    {
                                        paymentResponse.UpdatedDate = DateTime.Now;
                                        paymentResponse.status = "Success";
                                        context.Entry(paymentResponse).State = EntityState.Modified;


                                        var cashpaymentResponse = context.PaymentResponseRetailerAppDb.FirstOrDefault(x => x.OrderId == chequeCollection.Orderid && x.PaymentFrom == "Cash" && x.status == "Success" && x.amount == paymentResponse.amount);
                                        if (cashpaymentResponse != null)
                                        {
                                            cashpaymentResponse.UpdatedDate = DateTime.Now;
                                            cashpaymentResponse.status = "Failed";
                                            context.Entry(cashpaymentResponse).State = EntityState.Modified;
                                        }
                                    }
                                }

                                BackgroundTaskManager.Run(() =>
                                {
                                    AgentLedgerHelper agentLedgerHelper = new AgentLedgerHelper();
                                    CustomerChequeBounceLedgerHelper customerChequeBounceLedgerHelper = new CustomerChequeBounceLedgerHelper();
                                    //var assignment = context.DeliveryIssuanceDb.FirstOrDefault(a =>
                                    //!context.CurrencyCollection.Any(x => x.Deliveryissueid == a.DeliveryIssuanceId && x.Id == chequeCollection.CurrencyCollectionId && x.IsActive && ((x.IsDeleted.HasValue && !x.IsDeleted.Value) || !x.IsDeleted.HasValue)));
                                    if (assignment != null && assignment.AgentId > 0)
                                    {
                                        agentLedgerHelper.OnRevertChequeCancel(assignment.AgentId, Convert.ToDouble(chequeCollection.ChequeAmt), chequeCollection.ChequeNumber, userid, assignment.DeliveryIssuanceId, chequeCollection.Orderid.ToString());
                                        agentLedgerHelper.OnRevertChequeFine(assignment.AgentId, 300, chequeCollection.ChequeNumber, userid, assignment.DeliveryIssuanceId, chequeCollection.Orderid.ToString());
                                    }
                                    if (chequeCollection.Orderid > 0 && !string.IsNullOrEmpty(chequeCollection.ChequeNumber))
                                    {
                                        List<AngularJSAuthentication.DataContracts.Transaction.Ledger.ChequeBounceVM> chequeBounceVM = new List<AngularJSAuthentication.DataContracts.Transaction.Ledger.ChequeBounceVM> {
                                          new AngularJSAuthentication.DataContracts.Transaction.Ledger.ChequeBounceVM{
                                           Amount=Convert.ToDouble(chequeCollection.ChequeAmt),
                                           ChequeNumber=chequeCollection.ChequeNumber,
                                           Date=chequeCollection.ChequeDate
                                          }
                                         };
                                        customerChequeBounceLedgerHelper.OnRevertBounce(chequeCollection.Orderid, chequeBounceVM, amount, userid);
                                    }
                                });
                                #endregion
                            }
                        }


                        result = context.Commit() > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error("Error in GetChequeCollection Method: " + ex.Message);
                result = false;
            }

            return result;

        }


        //public bool GetSmsNotification(string MobileNumber, string message)
        //{

        //string Sender = ConfigurationManager.AppSettings["NewriseOTPSenderId"].ToString();
        //string username = ConfigurationManager.AppSettings["NewriseOTPUsername"].ToString();
        //string passwrod = ConfigurationManager.AppSettings["NewriseOTPPasswrod"].ToString();
        //string authkey = Startup.smsauthKey;
        ////string path = "http://bulksms.newrise.in/api/sendhttp.php?authkey=" + authkey + "&mobiles=" + MobileNumber + "&message=" + sRandomOTP + " :" + OtpMessage + " &sender=" + Sender + "&route=" + route + "&country=" + CountryCode;

        ////string path ="http://bulksms.newrise.in/api/sendhttp.php?authkey=100498AhbWDYbtJT56af33e3&mobiles=9770838685&message= SK OTP is : " + sRandomOTP + " &sender=SHOPKR&route=4&country=91";
        //string path = "http://www.smsjust.com/blank/sms/user/urlsms.php?username=" + username + "&pass=" + passwrod + "&senderid=" + Sender + "&dest_mobileno=" + MobileNumber + "&message=" + message + " &response=Y";

        //var webRequest = (HttpWebRequest)WebRequest.Create(path);
        //webRequest.Method = "GET";
        //webRequest.ContentType = "application/json";
        //webRequest.UserAgent = "Mozilla/5.0 (Windows NT 5.1; rv:28.0) Gecko/20100101 Firefox/28.0";
        //webRequest.ContentLength = 0; // added per comment 
        //webRequest.Credentials = CredentialCache.DefaultCredentials;
        //webRequest.Accept = "*/*";
        //var webResponse = (HttpWebResponse)webRequest.GetResponse();
        //if (webResponse.StatusCode != HttpStatusCode.OK) Console.WriteLine("{0}", webResponse.Headers);

        //return Common.Helpers.SendSMSHelper.SendSMS(MobileNumber, message, ((Int32)Common.Enums.SMSRouteEnum.Transactional).ToString());

        // }

        #region HQ Currency Collection
        [Route("GetHubCurrencyCollection")]
        [HttpGet]
        public List<HubCurrencyCollectionDc> GetHubCurrencyCollection(DateTime dateFilter)
        {
            List<HubCurrencyCollectionDc> hubCurrencyCollectionDcs = new List<HubCurrencyCollectionDc>();
            try
            {
                using (AuthContext context = new AuthContext())
                {
                    hubCurrencyCollectionDcs = context.CurrencyHubStock.Where(x => EntityFunctions.TruncateTime(x.CreatedDate) == EntityFunctions.TruncateTime(dateFilter)).OrderByDescending(x => x.CreatedDate).Select(x =>
                            new HubCurrencyCollectionDc
                            {
                                BOD = x.BOD,
                                CreatedBy = x.CreatedBy,
                                CreatedDate = x.CreatedDate,
                                DueResion = x.DueResion,
                                EOD = x.EOD,
                                Id = x.Id,
                                IsActive = x.IsActive,
                                IsDeleted = x.IsDeleted,
                                ModifiedBy = x.ModifiedBy,
                                ModifiedDate = x.ModifiedDate,
                                Status = x.Status,
                                //TotalAssignmentCount = x.CashCollections.Any() ? x.CashCollections.GroupBy(y => y.CurrencyCollection.Deliveryissueid).Count() : x.ChequeCollections.Any() ? x.ChequeCollections.GroupBy(y => y.CurrencyCollection.Deliveryissueid).Count() : 0,
                                TotalBankCashAmt = x.CurrencySettlementSources.Any() ? x.CurrencySettlementSources.Sum(y => y.TotalCashAmt) : 0,
                                TotalBankChequeAmt = x.CurrencySettlementSources.Any() ? x.CurrencySettlementSources.Sum(y => y.TotalChequeAmt) : 0,
                                TotalBankDepositDate = x.CurrencySettlementSources.Any() ? x.CurrencySettlementSources.FirstOrDefault().SettlementDate : (DateTime?)null,
                                TotalCashAmt = x.TotalCashAmt,
                                TotalCheckAmt = x.TotalCheckAmt,
                                TotalDeliveryissueAmt = x.TotalDeliveryissueAmt,
                                TotalDueAmt = x.TotalDueAmt,
                                TotalOnlineAmt = x.TotalOnlineAmt,
                                Warehouseid = x.Warehouseid
                            }).ToList();

                    if (hubCurrencyCollectionDcs != null && hubCurrencyCollectionDcs.Any())
                    {
                        var warehouse = context.Warehouses.Select(x => new { x.WarehouseId, WarehouseName = x.WarehouseName + "" + x.CityName }).ToList();
                        if (warehouse != null)
                        {
                            foreach (var item in hubCurrencyCollectionDcs)
                            {

                                item.WarehouseName = warehouse.FirstOrDefault(x => x.WarehouseId == item.Warehouseid).WarehouseName;
                                item.TotalAssignmentCount = context.CurrencyCollection.Count(x => x.Warehouseid == item.Warehouseid && EntityFunctions.TruncateTime(x.CreatedDate) == EntityFunctions.TruncateTime(dateFilter));
                            }
                        }
                    }
                }

                return hubCurrencyCollectionDcs;
            }
            catch (Exception ex)
            {
                logger.Error("Error in GetHubCurrencyCollection Method: " + ex.Message);
                return null;
            }
        }

        [Route("LiveHQCashDashboard")]
        [HttpGet]
        public LiveWarehouseCashDc LiveHQCashDashboard(int? warehouseid)
        {
            LiveWarehouseCashDc liveWarehouseCashDc = new LiveWarehouseCashDc();

            try
            {
                using (AuthContext context = new AuthContext())
                {
                    List<CurrencyHubStock> TodayHubCurrencyCollection = new List<CurrencyHubStock>();
                    if (warehouseid.HasValue)
                        TodayHubCurrencyCollection = context.CurrencyHubStock.Where(x => x.Warehouseid == warehouseid.Value && EntityFunctions.TruncateTime(x.BOD) == EntityFunctions.TruncateTime(DateTime.Now)).ToList();
                    else
                        TodayHubCurrencyCollection = context.CurrencyHubStock.Where(x => EntityFunctions.TruncateTime(x.BOD) == EntityFunctions.TruncateTime(DateTime.Now)).ToList();

                    List<long> yesterdayhubcollectionids = null;
                    List<hubCashCollectionDc> WarehouseOpeningCash = new List<hubCashCollectionDc>();
                    if (TodayHubCurrencyCollection != null && TodayHubCurrencyCollection.Any())
                    {
                        yesterdayhubcollectionids = TodayHubCurrencyCollection.Select(x => x.Id).ToList();
                        WarehouseOpeningCash = (
                           from c in context.CurrencyDenomination.Where(x => x.IsActive)
                           join p in context.HubCashCollection.Where(o => yesterdayhubcollectionids.Contains(o.CurrencyHubStockId) && o.IsActive
                           && ((o.IsDeleted.HasValue && !o.IsDeleted.Value) || !o.IsDeleted.HasValue))
                           on c.Id equals p.CurrencyDenominationId into ps
                           from p in ps.DefaultIfEmpty()
                               //group ps by new { c.Id, c.Title, c.Value, c.currencyType }
                               //into newGroup
                           select new hubCashCollectionDc
                           {
                               CurrencyDenominationId = c.Id,
                               CurrencyCount = p == null ? 0 : p.OpeningCurrencyCount,
                               CurrencyDenominationTitle = c.Title,
                               CurrencyDenominationValue = c.Value,
                               CashCurrencyType = c.currencyType,
                           }).ToList();

                        WarehouseOpeningCash = WarehouseOpeningCash.GroupBy(x => new { x.CurrencyDenominationId, x.CurrencyDenominationTitle, x.CurrencyDenominationValue, x.CashCurrencyType }).
                                             Select(x => new hubCashCollectionDc
                                             {
                                                 CurrencyDenominationId = x.Key.CurrencyDenominationId,
                                                 CurrencyCount = x.Sum(y => y.CurrencyCount),
                                                 CurrencyDenominationTitle = x.Key.CurrencyDenominationTitle,
                                                 CurrencyDenominationValue = x.Key.CurrencyDenominationValue,
                                                 CashCurrencyType = x.Key.CashCurrencyType,
                                             }).ToList();
                    }
                    else
                    {
                        WarehouseOpeningCash = (from c in context.CurrencyDenomination.Where(x => x.IsActive)
                                                select new hubCashCollectionDc
                                                {
                                                    CurrencyDenominationId = c == null ? 0 : c.Id,
                                                    CurrencyCount = 0,
                                                    CurrencyDenominationTitle = c == null ? "" : c.Title,
                                                    CurrencyDenominationValue = c == null ? 0 : c.Value,
                                                    CashCurrencyType = c == null ? "" : c.currencyType,
                                                }).ToList();
                    }
                    liveWarehouseCashDc.WarehouseOpeningCash = new List<hubCashCollectionDc>();
                    liveWarehouseCashDc.WarehouseOpeningCash = WarehouseOpeningCash;


                    List<hubCashCollectionDc> WarehouseTodayCash = new List<hubCashCollectionDc>();
                    if (TodayHubCurrencyCollection != null && TodayHubCurrencyCollection.Any())
                    {
                        List<long> todaydayhubcollectionids = TodayHubCurrencyCollection.Select(x => x.Id).ToList();
                        WarehouseTodayCash = (
                           from c in context.CurrencyDenomination.Where(x => x.IsActive)
                           join p in context.HubCashCollection.Where(o => todaydayhubcollectionids.Contains(o.CurrencyHubStockId) && o.IsActive
                           && ((o.IsDeleted.HasValue && !o.IsDeleted.Value) || !o.IsDeleted.HasValue))
                           on c.Id equals p.CurrencyDenominationId into ps
                           from p in ps.DefaultIfEmpty()
                               //group ps by new { c.Id, c.Title, c.Value, c.currencyType }
                               //into newGroup
                           select new hubCashCollectionDc
                           {
                               CurrencyDenominationId = c.Id,
                               CurrencyCount = p == null ? 0 : p.CurrencyCount,
                               CurrencyDenominationTitle = c.Title,
                               CurrencyDenominationValue = c.Value,
                               CashCurrencyType = c.currencyType,
                               BankDepositCurrencyCount = p == null ? 0 : p.BankSendCurrencyCount,
                               ExchangeInCurrencyCount = p == null ? 0 : (p.ExchangeCurrencyCount > 0 ? p.ExchangeCurrencyCount : 0),
                               ExchangeOutCurrencyCount = p == null ? 0 : (p.ExchangeCurrencyCount < 0 ? p.ExchangeCurrencyCount : 0),
                           }).ToList();

                        WarehouseTodayCash = WarehouseTodayCash.GroupBy(x => new { x.CurrencyDenominationId, x.CurrencyDenominationTitle, x.CurrencyDenominationValue, x.CashCurrencyType }).
                                            Select(x => new hubCashCollectionDc
                                            {
                                                CurrencyDenominationId = x.Key.CurrencyDenominationId,
                                                CurrencyCount = x.Sum(y => y.CurrencyCount),
                                                CurrencyDenominationTitle = x.Key.CurrencyDenominationTitle,
                                                CurrencyDenominationValue = x.Key.CurrencyDenominationValue,
                                                CashCurrencyType = x.Key.CashCurrencyType,
                                                ExchangeInCurrencyCount = x.Sum(y => y.ExchangeInCurrencyCount),
                                                ExchangeOutCurrencyCount = x.Sum(y => y.ExchangeOutCurrencyCount),
                                                BankDepositCurrencyCount = x.Sum(y => y.BankDepositCurrencyCount),
                                            }).ToList();
                        //liveWarehouseCashDc.BOD = todayhubcollection.BOD;
                        //liveWarehouseCashDc.CurrencyHubStockId = todayhubcollection.Id;
                        //liveWarehouseCashDc.EOD = todayhubcollection.EOD;
                        //liveWarehouseCashDc.IsBOD = true;
                        //liveWarehouseCashDc.IsEOD = liveWarehouseCashDc.EOD.HasValue ? true : false;
                    }
                    else
                    {
                        WarehouseTodayCash = (from c in context.CurrencyDenomination.Where(x => x.IsActive)
                                              select new hubCashCollectionDc
                                              {
                                                  CurrencyDenominationId = c == null ? 0 : c.Id,
                                                  CurrencyCount = 0,
                                                  CurrencyDenominationTitle = c == null ? "" : c.Title,
                                                  CurrencyDenominationValue = c == null ? 0 : c.Value,
                                                  CashCurrencyType = c == null ? "" : c.currencyType,
                                              }).ToList();
                        liveWarehouseCashDc.IsBOD = false;
                        liveWarehouseCashDc.IsEOD = false;
                    }
                    liveWarehouseCashDc.WarehouseTodayCash = new List<hubCashCollectionDc>();
                    liveWarehouseCashDc.WarehouseTodayCash = WarehouseTodayCash;
                    List<hubCashCollectionDc> WarehouseTodayClosingCash = new List<hubCashCollectionDc>();

                    WarehouseTodayClosingCash = WarehouseTodayCash.Union(WarehouseOpeningCash).GroupBy(x => new { x.CurrencyDenominationId, x.CurrencyDenominationTitle, x.CurrencyDenominationValue, x.CashCurrencyType })
                                                .Select(x => new hubCashCollectionDc
                                                {
                                                    CurrencyDenominationId = x.Key.CurrencyDenominationId,
                                                    CurrencyCount = x.Sum(y => y.CurrencyCount) - x.Sum(p => p.BankDepositCurrencyCount) + x.Sum(p => p.ExchangeInCurrencyCount) + x.Sum(p => p.ExchangeOutCurrencyCount),
                                                    CurrencyDenominationTitle = x.Key.CurrencyDenominationTitle,
                                                    CurrencyDenominationValue = x.Key.CurrencyDenominationValue,
                                                    CashCurrencyType = x.Key.CashCurrencyType,
                                                }).ToList();

                    liveWarehouseCashDc.WarehouseClosingCash = WarehouseTodayClosingCash;

                    if (liveWarehouseCashDc.WarehouseTodayCash != null && liveWarehouseCashDc.WarehouseTodayCash.Count() > 0 && yesterdayhubcollectionids != null && yesterdayhubcollectionids.Any())
                    {
                        var ExchangeHubCashCollections = context.ExchangeHubCashCollection.Where(o => yesterdayhubcollectionids.Contains(o.CurrencyHubStockId) && o.IsActive
                            && ((o.IsDeleted.HasValue && !o.IsDeleted.Value) || !o.IsDeleted.HasValue));

                        foreach (var item in liveWarehouseCashDc.WarehouseTodayCash)
                        {
                            if (ExchangeHubCashCollections != null && ExchangeHubCashCollections.Any(x => x.CurrencyDenominationId == item.CurrencyDenominationId))
                            {
                                item.ExchangeInCurrencyCount = ExchangeHubCashCollections.Any(x => x.CurrencyDenominationId == item.CurrencyDenominationId && x.CurrencyCount > 0) ? ExchangeHubCashCollections.Where(x => x.CurrencyDenominationId == item.CurrencyDenominationId && x.CurrencyCount > 0).Sum(x => x.CurrencyCount) : 0;
                                item.ExchangeOutCurrencyCount = ExchangeHubCashCollections.Any(x => x.CurrencyDenominationId == item.CurrencyDenominationId && x.CurrencyCount < 0) ? ExchangeHubCashCollections.Where(x => x.CurrencyDenominationId == item.CurrencyDenominationId && x.CurrencyCount < 0).Sum(x => x.CurrencyCount) : 0;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error("Error in LiveHQCashDashboard Method: " + ex.Message);
                return null;
            }

            return liveWarehouseCashDc;
        }

        [Route("HQCashHistory")]
        [HttpGet]
        public LiveWarehouseCashDc HQCashHistory(int? warehouseid, DateTime Filterdate)
        {
            LiveWarehouseCashDc liveWarehouseCashDc = new LiveWarehouseCashDc();

            try
            {
                using (AuthContext context = new AuthContext())
                {
                    List<CurrencyHubStock> TodayHubCurrencyCollection = new List<CurrencyHubStock>();
                    if (warehouseid.HasValue)
                        TodayHubCurrencyCollection = context.CurrencyHubStock.Where(x => x.Warehouseid == warehouseid.Value && EntityFunctions.TruncateTime(x.BOD) == EntityFunctions.TruncateTime(Filterdate)).ToList();
                    else
                        TodayHubCurrencyCollection = context.CurrencyHubStock.Where(x => EntityFunctions.TruncateTime(x.BOD) == EntityFunctions.TruncateTime(Filterdate)).ToList();

                    List<long> yesterdayhubcollectionids = null;
                    List<hubCashCollectionDc> WarehouseOpeningCash = new List<hubCashCollectionDc>();
                    if (TodayHubCurrencyCollection != null && TodayHubCurrencyCollection.Any())
                    {
                        yesterdayhubcollectionids = TodayHubCurrencyCollection.Select(x => x.Id).ToList();
                        WarehouseOpeningCash = (
                           from c in context.CurrencyDenomination.Where(x => x.IsActive)
                           join p in context.HubCashCollection.Where(o => yesterdayhubcollectionids.Contains(o.CurrencyHubStockId) && o.IsActive
                           && ((o.IsDeleted.HasValue && !o.IsDeleted.Value) || !o.IsDeleted.HasValue))
                           on c.Id equals p.CurrencyDenominationId into ps
                           from p in ps.DefaultIfEmpty()
                               //group ps by new { c.Id, c.Title, c.Value, c.currencyType }
                               //into newGroup
                           select new hubCashCollectionDc
                           {
                               CurrencyDenominationId = c.Id,
                               CurrencyCount = p == null ? 0 : p.OpeningCurrencyCount,
                               CurrencyDenominationTitle = c.Title,
                               CurrencyDenominationValue = c.Value,
                               CashCurrencyType = c.currencyType,
                           }).ToList();

                        WarehouseOpeningCash = WarehouseOpeningCash.GroupBy(x => new { x.CurrencyDenominationId, x.CurrencyDenominationTitle, x.CurrencyDenominationValue, x.CashCurrencyType }).
                                             Select(x => new hubCashCollectionDc
                                             {
                                                 CurrencyDenominationId = x.Key.CurrencyDenominationId,
                                                 CurrencyCount = x.Sum(y => y.CurrencyCount),
                                                 CurrencyDenominationTitle = x.Key.CurrencyDenominationTitle,
                                                 CurrencyDenominationValue = x.Key.CurrencyDenominationValue,
                                                 CashCurrencyType = x.Key.CashCurrencyType,
                                             }).ToList();
                    }
                    else
                    {
                        WarehouseOpeningCash = (from c in context.CurrencyDenomination.Where(x => x.IsActive)
                                                select new hubCashCollectionDc
                                                {
                                                    CurrencyDenominationId = c == null ? 0 : c.Id,
                                                    CurrencyCount = 0,
                                                    CurrencyDenominationTitle = c == null ? "" : c.Title,
                                                    CurrencyDenominationValue = c == null ? 0 : c.Value,
                                                    CashCurrencyType = c == null ? "" : c.currencyType,
                                                }).ToList();
                    }
                    liveWarehouseCashDc.WarehouseOpeningCash = new List<hubCashCollectionDc>();
                    liveWarehouseCashDc.WarehouseOpeningCash = WarehouseOpeningCash;


                    List<hubCashCollectionDc> WarehouseTodayCash = new List<hubCashCollectionDc>();
                    if (TodayHubCurrencyCollection != null && TodayHubCurrencyCollection.Any())
                    {
                        List<long> todaydayhubcollectionids = TodayHubCurrencyCollection.Select(x => x.Id).ToList();
                        WarehouseTodayCash = (
                           from c in context.CurrencyDenomination.Where(x => x.IsActive)
                           join p in context.HubCashCollection.Where(o => todaydayhubcollectionids.Contains(o.CurrencyHubStockId) && o.IsActive
                           && ((o.IsDeleted.HasValue && !o.IsDeleted.Value) || !o.IsDeleted.HasValue))
                           on c.Id equals p.CurrencyDenominationId into ps
                           from p in ps.DefaultIfEmpty()
                               //group ps by new { c.Id, c.Title, c.Value, c.currencyType }
                               //into newGroup
                           select new hubCashCollectionDc
                           {
                               CurrencyDenominationId = c.Id,
                               CurrencyCount = p == null ? 0 : p.CurrencyCount,
                               CurrencyDenominationTitle = c.Title,
                               CurrencyDenominationValue = c.Value,
                               CashCurrencyType = c.currencyType,
                               BankDepositCurrencyCount = p == null ? 0 : p.BankSendCurrencyCount,
                               ExchangeInCurrencyCount = p == null ? 0 : (p.ExchangeCurrencyCount > 0 ? p.ExchangeCurrencyCount : 0),
                               ExchangeOutCurrencyCount = p == null ? 0 : (p.ExchangeCurrencyCount < 0 ? p.ExchangeCurrencyCount : 0),
                           }).ToList();

                        WarehouseTodayCash = WarehouseTodayCash.GroupBy(x => new { x.CurrencyDenominationId, x.CurrencyDenominationTitle, x.CurrencyDenominationValue, x.CashCurrencyType }).
                                            Select(x => new hubCashCollectionDc
                                            {
                                                CurrencyDenominationId = x.Key.CurrencyDenominationId,
                                                CurrencyCount = x.Sum(y => y.CurrencyCount),
                                                CurrencyDenominationTitle = x.Key.CurrencyDenominationTitle,
                                                CurrencyDenominationValue = x.Key.CurrencyDenominationValue,
                                                CashCurrencyType = x.Key.CashCurrencyType,
                                                ExchangeInCurrencyCount = x.Sum(y => y.ExchangeInCurrencyCount),
                                                ExchangeOutCurrencyCount = x.Sum(y => y.ExchangeOutCurrencyCount),
                                                BankDepositCurrencyCount = x.Sum(y => y.BankDepositCurrencyCount),
                                            }).ToList();
                        //liveWarehouseCashDc.BOD = todayhubcollection.BOD;
                        //liveWarehouseCashDc.CurrencyHubStockId = todayhubcollection.Id;
                        //liveWarehouseCashDc.EOD = todayhubcollection.EOD;
                        //liveWarehouseCashDc.IsBOD = true;
                        //liveWarehouseCashDc.IsEOD = liveWarehouseCashDc.EOD.HasValue ? true : false;
                    }
                    else
                    {
                        WarehouseTodayCash = (from c in context.CurrencyDenomination.Where(x => x.IsActive)
                                              select new hubCashCollectionDc
                                              {
                                                  CurrencyDenominationId = c == null ? 0 : c.Id,
                                                  CurrencyCount = 0,
                                                  CurrencyDenominationTitle = c == null ? "" : c.Title,
                                                  CurrencyDenominationValue = c == null ? 0 : c.Value,
                                                  CashCurrencyType = c == null ? "" : c.currencyType,
                                              }).ToList();
                        liveWarehouseCashDc.IsBOD = false;
                        liveWarehouseCashDc.IsEOD = false;
                    }
                    liveWarehouseCashDc.WarehouseTodayCash = new List<hubCashCollectionDc>();
                    liveWarehouseCashDc.WarehouseTodayCash = WarehouseTodayCash;

                    List<hubCashCollectionDc> WarehouseTodayClosingCash = new List<hubCashCollectionDc>();

                    WarehouseTodayClosingCash = WarehouseTodayCash.Union(WarehouseOpeningCash).GroupBy(x => new { x.CurrencyDenominationId, x.CurrencyDenominationTitle, x.CurrencyDenominationValue, x.CashCurrencyType })
                                                .Select(x => new hubCashCollectionDc
                                                {
                                                    CurrencyDenominationId = x.Key.CurrencyDenominationId,
                                                    CurrencyCount = x.Sum(y => y.CurrencyCount) - x.Sum(p => p.BankDepositCurrencyCount) + x.Sum(p => p.ExchangeInCurrencyCount) + x.Sum(p => p.ExchangeOutCurrencyCount),
                                                    CurrencyDenominationTitle = x.Key.CurrencyDenominationTitle,
                                                    CurrencyDenominationValue = x.Key.CurrencyDenominationValue,
                                                    CashCurrencyType = x.Key.CashCurrencyType,
                                                }).ToList();

                    liveWarehouseCashDc.WarehouseClosingCash = WarehouseTodayClosingCash;

                    if (liveWarehouseCashDc.WarehouseTodayCash != null && liveWarehouseCashDc.WarehouseTodayCash.Count() > 0 && yesterdayhubcollectionids != null && yesterdayhubcollectionids.Any())
                    {
                        var ExchangeHubCashCollections = context.ExchangeHubCashCollection.Where(o => yesterdayhubcollectionids.Contains(o.CurrencyHubStockId) && o.IsActive
                            && ((o.IsDeleted.HasValue && !o.IsDeleted.Value) || !o.IsDeleted.HasValue));
                        foreach (var item in liveWarehouseCashDc.WarehouseTodayCash)
                        {
                            if (ExchangeHubCashCollections != null && ExchangeHubCashCollections.Any(x => x.CurrencyDenominationId == item.CurrencyDenominationId))
                            {
                                item.ExchangeInCurrencyCount = ExchangeHubCashCollections.Any(x => x.CurrencyDenominationId == item.CurrencyDenominationId && x.CurrencyCount > 0) ? ExchangeHubCashCollections.Where(x => x.CurrencyDenominationId == item.CurrencyDenominationId && x.CurrencyCount > 0).Sum(x => x.CurrencyCount) : 0;
                                item.ExchangeOutCurrencyCount = ExchangeHubCashCollections.Any(x => x.CurrencyDenominationId == item.CurrencyDenominationId && x.CurrencyCount < 0) ? ExchangeHubCashCollections.Where(x => x.CurrencyDenominationId == item.CurrencyDenominationId && x.CurrencyCount < 0).Sum(x => x.CurrencyCount) : 0;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error("Error in HQCashHistory Method: " + ex.Message);
                return null;
            }

            return liveWarehouseCashDc;
        }

        [Route("ResetHubEOD")]
        [HttpGet]
        public bool ResetHubEOD(long currencyHubStockid)
        {
            bool result = false;
            try
            {
                var identity = User.Identity as ClaimsIdentity;
                int compid = 0, userid = 0;

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))
                    compid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                    userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
                using (AuthContext context = new AuthContext())
                {
                    var currencyHubStock = context.CurrencyHubStock.FirstOrDefault(x => x.Id == currencyHubStockid);
                    if (currencyHubStock != null)
                    {
                        currencyHubStock.EOD = null;
                        currencyHubStock.ModifiedBy = userid;
                        currencyHubStock.ModifiedDate = indianTime;
                        //context.CurrencyHubStock.Attach(currencyHubStock);
                        context.Entry(currencyHubStock).State = EntityState.Modified;
                        result = context.Commit() > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error("Error in GetHubCurrencyCollection Method: " + ex.Message);
                result = false;
            }

            return result;
        }
        #endregion

        #region Generate Random OTP
        [HttpGet]
        [Route("GenerateOTPForCurrency")]
        public bool GenerateOTPForCurrency()
        {
            bool result = false;
            try
            {
                var identity = User.Identity as ClaimsIdentity;
                int compid = 0, userid = 0;

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))
                    compid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                    userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
                using (AuthContext context = new AuthContext())
                {
                    var people = context.Peoples.FirstOrDefault(x => x.PeopleID == userid);
                    if (people != null)
                    {
                        var otp = Getotp(people.Mobile);
                        if (otp != null && !string.IsNullOrEmpty(otp.OtpNo))
                        {
                            var CurrencyVerification = context.CurrencyVerification.FirstOrDefault(x => x.PeopleId == userid && x.MobileNo == people.Mobile && x.WarehouseId == people.WarehouseId && EntityFunctions.TruncateTime(x.CreatedDate) == EntityFunctions.TruncateTime(DateTime.Now));
                            if (CurrencyVerification != null)
                            {
                                CurrencyVerification.WarehouseId = people.WarehouseId;
                                CurrencyVerification.PeopleId = userid;
                                CurrencyVerification.MobileNo = people.Mobile;
                                CurrencyVerification.OTP = otp.OtpNo;
                                CurrencyVerification.CreatedDate = DateTime.Now;
                                //context.CurrencyVerification.Attach(CurrencyVerification);
                                context.Entry(CurrencyVerification).State = EntityState.Modified;
                                result = context.Commit() > 0;

                                //already exist for same day 

                            }
                            else
                            {
                                CurrencyVerification varification = new CurrencyVerification();
                                varification.WarehouseId = people.WarehouseId;
                                varification.PeopleId = userid;
                                varification.MobileNo = people.Mobile;
                                varification.OTP = otp.OtpNo;
                                varification.CreatedDate = DateTime.Now;
                                varification.UpdatedDate = DateTime.Now;
                                context.CurrencyVerification.Add(varification);

                                context.Commit();
                            }

                        }

                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error("Error in GenerateOTPForCurrency Method: " + ex.Message);
                result = false;
            }
            return result;
        }

        [HttpGet]
        [Route("ValidateOTPForCurrency")]
        public bool ValidateOTPForCurrency(string otp)
        {
            bool result = false;
            try
            {
                var identity = User.Identity as ClaimsIdentity;
                int compid = 0, userid = 0;

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))
                    compid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                    userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
                using (AuthContext context = new AuthContext())
                {
                    var CurrencyVerification = context.CurrencyVerification.FirstOrDefault(x => x.PeopleId == userid && x.OTP == otp && EntityFunctions.TruncateTime(x.CreatedDate) == EntityFunctions.TruncateTime(DateTime.Now));
                    if (CurrencyVerification != null)
                    {
                        CurrencyVerification.UpdatedDate = DateTime.Now;
                        CurrencyVerification.PeopleId = userid;
                        //CurrencyVerification.MobileNo = Mobile;
                        //currencyVerification.OTP = otp.OTP;
                        CurrencyVerification.CreatedDate = DateTime.Now;
                        //context.CurrencyVerification.Attach(CurrencyVerification);
                        context.Entry(CurrencyVerification).State = EntityState.Modified;
                        result = true;
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error("Error in GenerateOTPForCurrency Method: " + ex.Message);
                result = false;
            }
            return result;
        }


        /// <summary>
        /// Created by 18/12/2018 
        /// Create rendom otp
        /// </summary>
        /// <param name="iOTPLength"></param>
        /// <param name="saAllowedCharacters"></param>
        /// <returns></returns>
        private string GenerateRandomOTP(int iOTPLength, string[] saAllowedCharacters)
        {
            string sOTP = String.Empty;
            string sTempChars = String.Empty;
            Random rand = new Random();

            for (int i = 0; i < iOTPLength; i++)
            {
                int p = rand.Next(0, saAllowedCharacters.Length);
                sTempChars = saAllowedCharacters[rand.Next(0, saAllowedCharacters.Length)];
                sOTP += sTempChars;
            }
            return sOTP;
        }
        #endregion

        #region Generate Customer OTP
        /// <summary>
        /// Created by 18/12/2018 
        /// OTP Genration code 
        /// </summary>
        /// <returns></returns>
        [Route("Genotp")]
        public OTP Getotp(string MobileNumber)
        {
            logger.Info("start Gen OTP: ");
            try
            {

                string[] saAllowedCharacters = { "1", "2", "3", "4", "5", "6", "7", "8", "9", "0" };
                string sRandomOTP = GenerateRandomOTP(4, saAllowedCharacters);
                // string OtpMessage = " : is Your Verification Code. :).ShopKirana";
                string OtpMessage = ""; //"{#var1#} : is Your Verification Code. {#var2#}.ShopKirana";
                var dltSMS = SMSTemplateHelper.getTemplateText((int)AppEnum.Others, "Currency_Verification_Code");
                OtpMessage = dltSMS == null ? "" : dltSMS.Template;
                if (!string.IsNullOrEmpty(OtpMessage))
                {
                    OtpMessage = OtpMessage.Replace("{#var1#}", sRandomOTP);
                    OtpMessage = OtpMessage.Replace("{#var2#}", ":)");
                }
                //string message = sRandomOTP + " :" + OtpMessage;
                //string CountryCode = "91";
                //string Sender = ConfigurationManager.AppSettings["NewriseOTPSenderId"].ToString();
                //string username = ConfigurationManager.AppSettings["NewriseOTPUsername"].ToString();
                //string passwrod = ConfigurationManager.AppSettings["NewriseOTPPasswrod"].ToString();
                //string authkey = Startup.smsauthKey;
                //int route = 4;
                ////string path = "http://bulksms.newrise.in/api/sendhttp.php?authkey=" + authkey + "&mobiles=" + MobileNumber + "&message=" + sRandomOTP + " :" + OtpMessage + " &sender=" + Sender + "&route=" + route + "&country=" + CountryCode;

                ////string path ="http://bulksms.newrise.in/api/sendhttp.php?authkey=100498AhbWDYbtJT56af33e3&mobiles=9770838685&message= SK OTP is : " + sRandomOTP + " &sender=SHOPKR&route=4&country=91";
                //string path = "http://www.smsjust.com/blank/sms/user/urlsms.php?username=" + username + "&pass=" + passwrod + "&senderid=" + Sender + "&dest_mobileno=" + MobileNumber + "&message=" + sRandomOTP + " :" + OtpMessage + " &response=Y";

                //var webRequest = (HttpWebRequest)WebRequest.Create(path);
                //webRequest.Method = "GET";
                //webRequest.ContentType = "application/json";
                //webRequest.UserAgent = "Mozilla/5.0 (Windows NT 5.1; rv:28.0) Gecko/20100101 Firefox/28.0";
                //webRequest.ContentLength = 0; // added per comment 
                //webRequest.Credentials = CredentialCache.DefaultCredentials;
                //webRequest.Accept = "*/*";
                //var webResponse = (HttpWebResponse)webRequest.GetResponse();
                //if (webResponse.StatusCode != HttpStatusCode.OK) Console.WriteLine("{0}", webResponse.Headers);
                if (dltSMS != null)
                    Common.Helpers.SendSMSHelper.SendSMS(MobileNumber, OtpMessage, ((Int32)Common.Enums.SMSRouteEnum.OTP).ToString(), dltSMS.DLTId);

                logger.Info("OTP Genrated: " + sRandomOTP);
                OTP a = new OTP()
                {
                    OtpNo = sRandomOTP
                };
                return a;
            }
            catch (Exception ex)
            {
                logger.Error("Error in OTP Genration.");
                return null;
            }
        }
        #endregion


        #region Only active agent Get API
        /// <summary>
        /// Created Date 19/07/2019
        /// Created By Anushka
        /// this api used to get all active agent 
        /// </summary>
        /// <returns>person</returns>
        [Route("Activeagent")]
        public IEnumerable<People> GetActive(int warehouseId)
        {
            try
            {
                using (AuthContext context = new AuthContext())
                {
                    string query = "select distinct p.* from People p inner join AspNetUsers u on p.Email=u.Email inner join AspNetUserRoles ur on u.Id=ur.UserId inner join AspNetRoles r on ur.RoleId=r.Id where p.WarehouseId=" + warehouseId + " and r.Name='Agent'and p.Deleted=0";
                    List<People> person = context.Database.SqlQuery<People>(query).ToList();
                    //List<People> person = context.Peoples.Where(x => x.WarehouseId == warehouseId && x.Active && !x.Deleted && x.Department == "Agent").ToList();
                    return person;
                }
            }
            catch (Exception ex)
            {
                logger.Error("Error in getting Peoples " + ex.Message);
                return null;
            }
        }
        #endregion

        // Added by Anoop 5/2/2021
        [Route("ActiveagentData")]
        [HttpPost]
        [AllowAnonymous]
        // public HttpResponseMessage GetPoTracker(PoDashBoardParam ObjPoDashBoardParam)
        public HttpResponseMessage ActiveagentData(int[] warehouseid)//List<warehouesDTO> warehouseid)
        {
            try
            {

                //string WId = "";

                string WId = String.Join(",", warehouseid.Select(p => p.ToString()).ToArray());

                List<People> person = new List<People>();
                using (AuthContext context = new AuthContext())
                {
                    foreach (var item in warehouseid)
                    {
                        //WId = String.Join(",", item.ToString(","));
                        //    WId = item.ToString(",");
                        //result = arr.ToString(",");
                        //var WarehouseIds = warehouseid.Split(',').Select(Int32.Parse).ToList();


                        //string query = "select distinct p.* from People p inner join AspNetUsers u on p.Email=u.Email inner join AspNetUserRoles ur on u.Id=ur.UserId inner join AspNetRoles r on ur.RoleId=r.Id where p.WarehouseId=" + WId + " and r.Name='Agent' and p.Deleted=0";
                        // person = context.Database.SqlQuery<People>(query).ToList();
                        ////List<People> person = context.Peoples.Where(x => x.WarehouseId == warehouseId && x.Active && !x.Deleted && x.Department == "Agent").ToList();

                    }


                    string query = "select distinct p.* from People p inner join AspNetUsers u on p.Email=u.Email inner join AspNetUserRoles ur on u.Id=ur.UserId inner join AspNetRoles r on ur.RoleId=r.Id where p.WarehouseId in (" + WId + ") and r.Name='Agent' and p.Deleted=0";
                    person = context.Database.SqlQuery<People>(query).ToList();

                    return Request.CreateResponse(HttpStatusCode.OK, person);

                }
                // return Request.CreateResponse(HttpStatusCode.OK, true);
            }
            catch (Exception ex)
            {

                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex.GetBaseException().Message.ToString());
            }

        }



        #region Add Cheque Return Details 
        /// <summary> 
        ///  Add Cheque Return Details for Warehouse
        /// </summary>    
        /// <param name="Cheque"></param>
        /// <returns></returns>
        [Route("UpdateReturnChequeDetails")]
        [AcceptVerbs("POST")]
        public bool UpdateReturnChequeDetails(ReturnChequeCollectionDc ReturnWarehouse)
        {
            bool result = false;
            logger.Info("start Add Returns: ");
            try
            {

                var identity = User.Identity as ClaimsIdentity;
                int compid = 0, userid = 0;

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))
                    compid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                    userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

                using (AuthContext db = new AuthContext())
                {
                    if (ReturnWarehouse != null && ReturnWarehouse.Id > 0)
                    {
                        People agentName = db.Peoples.Where(x => x.PeopleID == ReturnWarehouse.HandOverAgentId).FirstOrDefault();
                        var dm = db.ReturnChequeCollection.FirstOrDefault(x => x.Id == ReturnWarehouse.Id);
                        if (ReturnWarehouse.Type == "WarehouseReceive")
                        {

                            dm.HubReceiverName = ReturnWarehouse.HubReceiverName;
                            dm.HubReceiveDate = ReturnWarehouse.HubReceiveDate;
                            dm.Status = Convert.ToInt32(ChequeReturnStatusEnum.ReceivedAtHub);
                        }
                        if (ReturnWarehouse.Type == "HandOver")
                        {
                            dm.HubSenderName = ReturnWarehouse.HubSenderName;
                            dm.HandOverDate = ReturnWarehouse.HandOverDate;
                            dm.HandOverAgentId = ReturnWarehouse.HandOverAgentId;
                            dm.HandOverAgentName = agentName.DisplayName;
                            dm.Status = Convert.ToInt32(ChequeReturnStatusEnum.HandOvertoAgent);
                        }
                        if (ReturnWarehouse.Type == "HQReceive")
                        {
                            dm.HQReceiverName = ReturnWarehouse.HQReceiverName;
                            dm.HQReceiveDate = ReturnWarehouse.HQReceiveDate;
                            dm.Status = Convert.ToInt32(ChequeReturnStatusEnum.ReceivedAtHQ);
                        }

                        if (ReturnWarehouse.Type == "HQCourier")
                        {
                            dm.CourierName = ReturnWarehouse.CourierName;
                            dm.CourierDate = ReturnWarehouse.CourierDate;
                            dm.PodNo = ReturnWarehouse.PodNo;
                            dm.Status = Convert.ToInt32(ChequeReturnStatusEnum.Couriered);
                        }
                        dm.ModifiedDate = DateTime.Now;
                        dm.ModifiedBy = userid;

                        db.Entry(dm).State = EntityState.Modified;
                        result = db.Commit() > 0;
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                logger.Error("Error in WarehopuseReturnChequeAddDetails " + ex.Message);
                return false;
            }
        }
        #endregion

        //#region Deu Amount Assignment (18/09/2019)
        ///// <summary>
        ///// Created Date 18/09/2019
        ///// Created By Anushka
        ///// this API use for Agent Deu payment
        ///// </summary>
        ///// <returns>person</returns>
        //[Route("GetDeuAmount")]
        //[HttpGet]
        //public IEnumerable<dynamic> GetDeuAmount()
        //{
        //    try
        //    {
        //        var identity = User.Identity as ClaimsIdentity;
        //        int compid = 0, userid = 0, Warehouse_id = 0;

        //        if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))
        //            compid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);

        //        if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
        //            userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

        //        if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "Warehouseid"))
        //            Warehouse_id = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "Warehouseid").Value);

        //        logger.Info("User ID : {0} , Company Id : {1}", compid, userid);
        //        using (AuthContext context = new AuthContext())
        //        {

        //            List<AgentdueAmountDc> AgentPayment = new List<AgentdueAmountDc>();
        //            if (Warehouse_id > 0)
        //            {

        //                //var AgentPayment = from c in context.CurrencyCollection
        //                //                   join p in context.AgentSettelment
        //                //                   on c.Deliveryissueid !=  
        //                //                 where c.
        //                var agentsettle = context.AgentSettelment.Where(x => x.IsDeleted == false).Select(x => x.Deliveryissueid).ToList();
        //                if (agentsettle.Count != 0)
        //                {
        //                    var agentdata = context.CurrencyCollection.Where(x => x.IsDeleted == false && x.TotalDueAmt > 0 && x.Warehouseid == Warehouse_id).Select(y => new AgentdueAmountDc
        //                    {
        //                        CurrencycollectionId = y.Id,
        //                        Deliveryissueid = y.Deliveryissueid,
        //                        TotalDueAmt = y.TotalDueAmt,
        //                        WareHouseSettleDate = y.WareHouseSettleDate,
        //                        Warehouseid = y.Warehouseid,
        //                        DBoyPeopleId = y.DBoyPeopleId

        //                    }).ToList();

        //                    foreach (var agdataid in agentdata)
        //                    {

        //                        if (agentsettle.Contains(agdataid.Deliveryissueid))
        //                        {


        //                        }
        //                        else
        //                        {
        //                            AgentPayment.Add(agdataid);
        //                        }
        //                    }

        //                }
        //                else
        //                {
        //                    var agentdata = context.CurrencyCollection.Where(x => x.IsDeleted == false && x.TotalDueAmt > 0 && x.Warehouseid == Warehouse_id).Select(y => new AgentdueAmountDc
        //                    {
        //                        CurrencycollectionId = y.Id,
        //                        Deliveryissueid = y.Deliveryissueid,
        //                        TotalDueAmt = y.TotalDueAmt,
        //                        WareHouseSettleDate = y.WareHouseSettleDate,
        //                        Warehouseid = y.Warehouseid,
        //                        DBoyPeopleId = y.DBoyPeopleId

        //                    }).ToList();
        //                    if (agentdata != null)
        //                    {
        //                        AgentPayment.AddRange(agentdata);
        //                    }

        //                }
        //                foreach (var currencydata in AgentPayment)
        //                {
        //                    currencydata.DBoyPeopleName = context.Peoples.Where(x => x.PeopleID == currencydata.DBoyPeopleId).Select(x => x.DisplayName).FirstOrDefault();
        //                    currencydata.WarehouseName = context.Warehouses.Where(x => x.WarehouseId == currencydata.Warehouseid).Select(x => x.WarehouseName).FirstOrDefault();
        //                    currencydata.AgentName = context.DeliveryIssuanceDb.Where(x => x.AgentId == currencydata.Agentid).Select(x => x.DisplayName).FirstOrDefault();
        //                }

        //                return AgentPayment;

        //            }
        //            else
        //            {
        //                var agentsettle = context.AgentSettelment.Where(x => x.IsDeleted == false).Select(x => x.Deliveryissueid).ToList();
        //                foreach (var delissid in agentsettle)
        //                {

        //                    AgentPayment = context.CurrencyCollection.Where(x => x.IsDeleted == false && x.TotalDueAmt > 0).Select(y => new AgentdueAmountDc
        //                    {
        //                        CurrencycollectionId = y.Id,
        //                        Deliveryissueid = y.Deliveryissueid,
        //                        TotalDueAmt = y.TotalDueAmt,
        //                        WareHouseSettleDate = y.WareHouseSettleDate,
        //                        Warehouseid = y.Warehouseid,
        //                        DBoyPeopleId = y.DBoyPeopleId

        //                    }).ToList();

        //                }
        //                foreach (var currencydata in AgentPayment)
        //                {
        //                    currencydata.DBoyPeopleName = context.Peoples.Where(x => x.PeopleID == currencydata.DBoyPeopleId).Select(x => x.DisplayName).FirstOrDefault();
        //                    currencydata.WarehouseName = context.Warehouses.Where(x => x.WarehouseId == currencydata.Warehouseid).Select(x => x.WarehouseName).FirstOrDefault();
        //                    currencydata.AgentName = context.DeliveryIssuanceDb.Where(x => x.AgentId == currencydata.Agentid).Select(x => x.DisplayName).FirstOrDefault();
        //                }
        //                return AgentPayment;
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        logger.Error("Error in getting Peoples " + ex.Message);
        //        return null;
        //    }
        //}
        //#endregion

        #region Deu Amount Assignment(07-11-2019)
        /// <summary>
        /// Created Date 18/09/2019
        /// Created By Anushka
        /// this API use for Agent Deu payment
        /// </summary>
        /// <returns>person</returns>
        [Route("GetDeuAmountbyagentPaginatorCount")]
        [HttpGet]
        public GetDeuAmountPaginatorCountdata GetDeuAmountbyagentPaginatorCount(int totalitem, int page, int warehouseid, int PeopleID)
        {
            try
            {
                var identity = User.Identity as ClaimsIdentity;
                int compid = 0, userid = 0, Warehouse_id = 0;

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))
                    compid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                    userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "Warehouseid"))
                    Warehouse_id = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "Warehouseid").Value);

                logger.Info("User ID : {0} , Company Id : {1}", compid, userid);
                Warehouse_id = warehouseid;
                using (AuthContext context = new AuthContext())
                {
                    GetDeuAmountPaginatorCountdata GetDeuAmountPaginatorCountdata = new GetDeuAmountPaginatorCountdata();
                    List<AgentdueAmountDc> AgentPayment = new List<AgentdueAmountDc>();
                    if (Warehouse_id > 0)
                    {
                        var agentsettle = context.AgentSettelment.Where(x => x.IsDeleted == false).Select(x => x.Deliveryissueid).ToList();
                        var totalcount = context.CurrencyCollection.Where(x => x.IsDeleted == false && x.TotalDueAmt > 0 && x.Warehouseid == Warehouse_id).ToList().Count;

                        if (agentsettle.Count != 0)
                        {
                            var agentdata = (
                             from c in context.CurrencyCollection
                             join p in context.DeliveryIssuanceDb
                             on c.Deliveryissueid equals p.DeliveryIssuanceId
                             where p.AgentId == PeopleID && c.IsDeleted == false && c.TotalDueAmt > 0 && c.Warehouseid == Warehouse_id
                             select new AgentdueAmountDc
                             // var agentdata = context.CurrencyCollection.Where(x => x.IsDeleted == false && x.TotalDueAmt > 0 && x.Warehouseid == Warehouse_id).Select(y => new AgentdueAmountDc
                             {
                                 CurrencycollectionId = c.Id,
                                 Deliveryissueid = c.Deliveryissueid,
                                 TotalDueAmt = c.TotalDueAmt,
                                 WareHouseSettleDate = c.WareHouseSettleDate,
                                 Warehouseid = c.Warehouseid,
                                 DBoyPeopleId = c.DBoyPeopleId,
                                 Agentid = p.AgentId,
                                 Orderid = p.OrderIds
                             }).OrderByDescending(x => x.Deliveryissueid).Skip((page - 1) * totalitem).Take(totalitem).ToList();

                            foreach (var agdataid in agentdata)
                            {
                                if (agentsettle.Contains(agdataid.Deliveryissueid))
                                {
                                }
                                else
                                {
                                    AgentPayment.Add(agdataid);
                                }
                            }
                        }
                        else
                        {
                            var agentdata = (
                            from c in context.CurrencyCollection
                            join p in context.DeliveryIssuanceDb
                            on c.Deliveryissueid equals p.DeliveryIssuanceId
                            where p.AgentId == PeopleID && c.IsDeleted == false && c.TotalDueAmt > 0 && c.Warehouseid == Warehouse_id
                            select new AgentdueAmountDc
                            {
                                CurrencycollectionId = c.Id,
                                Deliveryissueid = c.Deliveryissueid,
                                TotalDueAmt = c.TotalDueAmt,
                                WareHouseSettleDate = c.WareHouseSettleDate,
                                Warehouseid = c.Warehouseid,
                                DBoyPeopleId = c.DBoyPeopleId,
                                Agentid = p.AgentId,
                                Orderid = p.OrderIds
                            }).OrderByDescending(x => x.Deliveryissueid).Skip((page - 1) * totalitem).Take(totalitem).ToList();
                            if (agentdata != null)
                            {
                                AgentPayment.AddRange(agentdata);
                            }

                        }

                        if (AgentPayment != null && AgentPayment.Any())
                        {
                            var dboyIds = AgentPayment.Select(x => x.DBoyPeopleId).Distinct().ToList();
                            var dboyNames = context.Peoples.Where(x => dboyIds.Contains(x.PeopleID)).Select(x => new { x.PeopleID, x.DisplayName }).ToList();
                            var warehouseIds = AgentPayment.Select(x => x.DBoyPeopleId).Distinct().ToList();
                            var warehouseNames = context.Warehouses.Where(x => warehouseIds.Contains(x.WarehouseId)).Select(x => new { WarehouseId = x.WarehouseId, WarehouseName = x.WarehouseName + "-" + x.CityName }).ToList();
                            var AgentIds = AgentPayment.Select(x => x.Agentid).Distinct().ToList();
                            var AgentNames = context.Peoples.Where(x => AgentIds.Contains(x.PeopleID)).Select(x => new { x.PeopleID, x.DisplayName }).ToList();
                            foreach (var currencydata in AgentPayment)
                            {
                                currencydata.DBoyPeopleName = dboyNames != null && dboyNames.Any(x => x.PeopleID == currencydata.DBoyPeopleId) ? dboyNames.FirstOrDefault(x => x.PeopleID == currencydata.DBoyPeopleId).DisplayName : "";
                                currencydata.WarehouseName = warehouseNames != null && warehouseNames.Any(x => x.WarehouseId == currencydata.Warehouseid) ? warehouseNames.FirstOrDefault(x => x.WarehouseId == currencydata.Warehouseid).WarehouseName : "";
                                if (currencydata.Agentid > 0)
                                {
                                    currencydata.AgentName = AgentNames != null && AgentNames.Any(x => x.PeopleID == currencydata.Agentid) ? AgentNames.FirstOrDefault(x => x.PeopleID == currencydata.Agentid).DisplayName : "";
                                }
                            }
                        }
                        GetDeuAmountPaginatorCountdata.AgentdueAmountDc = AgentPayment;
                        GetDeuAmountPaginatorCountdata.total_count = totalcount;

                        return GetDeuAmountPaginatorCountdata;

                    }
                    return null;

                }
            }
            catch (Exception ex)
            {
                logger.Error("Error in getting Peoples " + ex.Message);
                return null;
            }
        }
        #endregion

        #region Deu Amount Assignment(18-09-2019)
        /// <summary>
        /// Created Date 18/09/2019
        /// Created By Anushka
        /// this API use for Agent Deu payment
        /// </summary>
        /// <returns>person</returns>
        [Route("GetDeuAmountPaginatorCount")]
        [HttpGet]
        public GetDeuAmountPaginatorCountdata GetDeuAmountPaginatorCount(int totalitem, int page, int warehouseid)
        {
            try
            {
                var identity = User.Identity as ClaimsIdentity;
                int compid = 0, userid = 0, Warehouse_id = 0;

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))
                    compid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                    userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "Warehouseid"))
                    Warehouse_id = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "Warehouseid").Value);

                logger.Info("User ID : {0} , Company Id : {1}", compid, userid);
                Warehouse_id = warehouseid;
                using (AuthContext context = new AuthContext())
                {
                    GetDeuAmountPaginatorCountdata GetDeuAmountPaginatorCountdata = new GetDeuAmountPaginatorCountdata();
                    List<AgentdueAmountDc> AgentPayment = new List<AgentdueAmountDc>();
                    if (Warehouse_id > 0)
                    {

                        var agentsettle = context.AgentSettelment.Where(x => x.IsDeleted == false).Select(x => x.Deliveryissueid).ToList();
                        var totalcount = context.CurrencyCollection.Where(x => x.IsDeleted == false && x.TotalDueAmt > 0 && x.Warehouseid == Warehouse_id).ToList().Count;

                        if (agentsettle.Count != 0)
                        {
                            var agentdata = (
                            from c in context.CurrencyCollection
                            join p in context.DeliveryIssuanceDb
                            on c.Deliveryissueid equals p.DeliveryIssuanceId
                            where c.IsDeleted == false && c.TotalDueAmt > 0 && c.Warehouseid == Warehouse_id
                            select new AgentdueAmountDc
                            //var agentdata = context.CurrencyCollection.Where(x => x.IsDeleted == false && x.TotalDueAmt > 0 && x.Warehouseid == Warehouse_id).Select(y => new AgentdueAmountDc
                            {
                                CurrencycollectionId = c.Id,
                                Deliveryissueid = c.Deliveryissueid,
                                TotalDueAmt = c.TotalDeliveryissueAmt + c.Fine - (c.TotalCashAmt + c.TotalCheckAmt + c.TotalOnlineAmt),
                                WareHouseSettleDate = c.WareHouseSettleDate,
                                Warehouseid = c.Warehouseid,
                                DBoyPeopleId = c.DBoyPeopleId,
                                Agentid = p.AgentId,
                                Orderid = p.OrderIds

                            }).OrderByDescending(x => x.Deliveryissueid).Skip((page - 1) * totalitem).Take(totalitem).ToList();

                            foreach (var agdataid in agentdata)
                            {
                                if (agentsettle.Contains(agdataid.Deliveryissueid))
                                {
                                }
                                else
                                {
                                    AgentPayment.Add(agdataid);
                                }
                            }
                        }
                        else
                        {
                            var agentdata = (
                             from c in context.CurrencyCollection
                             join p in context.DeliveryIssuanceDb
                             on c.Deliveryissueid equals p.DeliveryIssuanceId
                             where c.IsDeleted == false && c.TotalDueAmt > 0 && c.Warehouseid == Warehouse_id
                             select new AgentdueAmountDc
                             {
                                 CurrencycollectionId = c.Id,
                                 Deliveryissueid = c.Deliveryissueid,
                                 TotalDueAmt = c.TotalDeliveryissueAmt + c.Fine - (c.TotalCashAmt + c.TotalCheckAmt + c.TotalOnlineAmt),
                                 WareHouseSettleDate = c.WareHouseSettleDate,
                                 Warehouseid = c.Warehouseid,
                                 DBoyPeopleId = c.DBoyPeopleId,
                                 Agentid = p.AgentId,
                                 Orderid = p.OrderIds
                             }).OrderByDescending(x => x.Deliveryissueid).Skip((page - 1) * totalitem).Take(totalitem).ToList();
                            if (agentdata != null)
                            {
                                AgentPayment.AddRange(agentdata);
                            }

                        }

                        if (AgentPayment != null && AgentPayment.Any())
                        {
                            var AgentIds = AgentPayment.Select(x => x.Agentid).Distinct().ToList();
                            var AgentNames = context.Peoples.Where(x => AgentIds.Contains(x.PeopleID)).Select(x => new { x.PeopleID, x.DisplayName }).ToList();
                            var WarehouseIds = AgentPayment.Select(x => x.Warehouseid).Distinct().ToList();
                            var WarehouseNames = context.Warehouses.Where(x => WarehouseIds.Contains(x.WarehouseId)).Select(x => new { x.WarehouseId, x.WarehouseName }).ToList();
                            foreach (var currencydata in AgentPayment)
                            {
                                currencydata.AgentName = context.Peoples.Where(x => x.PeopleID == currencydata.Agentid).Select(x => x.DisplayName).FirstOrDefault();
                                currencydata.WarehouseName = context.Warehouses.Where(x => x.WarehouseId == currencydata.Warehouseid).Select(x => x.WarehouseName).FirstOrDefault();
                                if (currencydata.Agentid > 0)
                                {
                                    currencydata.WarehouseName = WarehouseNames != null && WarehouseNames.Any(x => x.WarehouseId == currencydata.Warehouseid) ? WarehouseNames.FirstOrDefault(x => x.WarehouseId == currencydata.Warehouseid).WarehouseName : "";
                                    currencydata.AgentName = AgentNames != null && AgentNames.Any(x => x.PeopleID == currencydata.Agentid) ? AgentNames.FirstOrDefault(x => x.PeopleID == currencydata.Agentid).DisplayName : "";
                                }
                            }
                        }
                        GetDeuAmountPaginatorCountdata.AgentdueAmountDc = AgentPayment;
                        GetDeuAmountPaginatorCountdata.total_count = totalcount;
                        return GetDeuAmountPaginatorCountdata;

                    }
                    return null;

                }
            }
            catch (Exception ex)
            {
                logger.Error("Error in getting Peoples " + ex.Message);
                return null;
            }
        }
        #endregion

        #region Agent payment Settel (4-10-2019)
        /// <summary>
        /// Agent Payment Settel 
        /// </summary>
        /// <param name="SettelList"></param>
        /// <returns></returns>
        [Route("AgentPaymentSettel")]
        [HttpPost]
        public bool AgentPaymentSettel(List<AgentSettelDc> SettelList)
        {
            bool result = false;
            try
            {
                var identity = User.Identity as ClaimsIdentity;
                int compid = 0, userid = 0;

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))
                    compid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                    userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
                using (AuthContext db = new AuthContext())
                {
                    People People = db.Peoples.Where(x => x.PeopleID == userid).FirstOrDefault();

                    foreach (var Settel in SettelList)
                    {
                        AgentSettelment Agent = new AgentSettelment();

                        Agent.Deliveryissueid = Settel.Deliveryissueid;
                        Agent.DBoyPeopleId = Settel.DBoyPeopleId;
                        Agent.AgentId = Settel.Agentid;
                        Agent.TotalDueAmt = Settel.TotalDueAmt;
                        Agent.CreatedDate = DateTime.Now;
                        Agent.IsActive = true;
                        Agent.IsDeleted = false;
                        Agent.Warehouseid = Settel.Warehouseid;
                        Agent.WareHouseSettleDate = Settel.WareHouseSettleDate;
                        Agent.ModifiedDate = DateTime.Now;
                        Agent.CreatedBy = userid;

                        db.AgentSettelment.Add(Agent);
                        db.Commit();
                    }

                }
                return true;
            }

            catch (Exception ex)
            {
                logger.Error("Error in Settel: " + ex.Message);
            }
            return false;
        }
        #endregion

        #region payment settel for other Mod payment (24-09-2019)
        /// <summary>
        /// payment settel for Agent collection(24-09-2019) 
        /// </summary>
        /// <param name="agentothermodpayementdata"></param>
        /// <returns></returns>      
        [Route("AgentOtherModPayement")]
        [HttpPost]
        public bool AgentOtherModPayement(Agentothermodpayementdata agentothermodpayementdata)
        {
            bool result = false;
            decimal TotalCash = 0, TotalCheque = 0, TotalOnline = 0;
            try
            {
                var identity = User.Identity as ClaimsIdentity;
                int compid = 0, userid = 0, Warehouse_id = 0;

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))
                    compid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                    userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "Warehouseid"))
                    Warehouse_id = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "Warehouseid").Value);

                using (AuthContext db = new AuthContext())
                {
                    // CurrencyCollection currencydata=db.CurrencyCollection.Where(x=>x.Deliveryissueid== agentothermodpayementdata.Del)
                    //People People = db.Peoples.Where(x => x.PeopleID == userid).FirstOrDefault();

                    var CurrencyHubStock = db.CurrencyHubStock.FirstOrDefault(x => x.Warehouseid == Warehouse_id && EntityFunctions.TruncateTime(x.BOD) == EntityFunctions.TruncateTime(DateTime.Now));

                    var denomination = db.CurrencyDenomination.Where(x => x.IsActive).ToList();
                    List<AgentCollection> AgentCashCollections = new List<AgentCollection>();
                    foreach (var agdata in agentothermodpayementdata.agentCollectionDc)
                    {
                        AgentCollection Agent = new AgentCollection();
                        //Agent.CurrencycollectionId=agdata.CurrencycollectionId,
                        Agent.AgentSettelmentId = agdata.AgentSettelmentId;
                        Agent.CurrencyDenominationId = Convert.ToInt32(agdata.Id);
                        Agent.CurrencyHubstockId = CurrencyHubStock != null ? Convert.ToInt32(CurrencyHubStock.Id) : (Int32?)null;
                        Agent.CurrencyCount = agdata.CurrencyCount;
                        Agent.CreatedDate = indianTime;
                        Agent.IsActive = true;
                        Agent.IsDeleted = false;
                        Agent.ModifiedDate = DateTime.Now;
                        Agent.TotalAmount = agentothermodpayementdata.totalamount;
                        Agent.CreatedBy = userid;
                        AgentCashCollections.Add(Agent);
                        db.AgentCollection.Add(Agent);
                        var denom = denomination.FirstOrDefault(x => x.Id == Agent.CurrencyDenominationId);
                        if (denom != null)
                            TotalCash += denom.Value * Agent.CurrencyCount.Value;

                        if (CurrencyHubStock != null)
                            CurrencyHubStock.TotalCashAmt += TotalCash;
                    }

                    if (agentothermodpayementdata.agentChequeCollectionDc != null)
                    {

                        var BankName = db.CurrencySettlementBank.Where(x => x.Id == agentothermodpayementdata.agentChequeCollectionDc.BankId).Select(x => x.BankName).FirstOrDefault();
                        ChequeCollection Agentdata = new ChequeCollection();

                        Agentdata.WarehouseId = Warehouse_id;
                        Agentdata.AgentId = agentothermodpayementdata.agentdueAmountDc[0].DBoyPeopleId;
                        Agentdata.ChequeNumber = agentothermodpayementdata.agentChequeCollectionDc.ChequeNumber;
                        Agentdata.ChequeAmt = agentothermodpayementdata.agentChequeCollectionDc.CollectAmount;
                        Agentdata.ChequeBankName = BankName;
                        Agentdata.ChequeDate = indianTime;
                        Agentdata.CurrencyHubStockId = CurrencyHubStock != null ? CurrencyHubStock.Id : (long?)null;
                        Agentdata.CreatedBy = userid;
                        Agentdata.CurrencyCollectionId = null;
                        Agentdata.DBoyPeopleId = 0;
                        Agentdata.CreatedDate = DateTime.Now;
                        //Agentdata.Orderid = 0;
                        Agentdata.Orderid = agentothermodpayementdata.agentChequeCollectionDc.OrderId;
                        Agentdata.ChequeDate = DateTime.Now;
                        Agentdata.IsActive = true;
                        Agentdata.IsDeleted = false;
                        Agentdata.UsedChequeAmt = Agentdata.ChequeAmt;
                        Agentdata.ChequeStatus = 1;
                        Agentdata.ChequeimagePath = agentothermodpayementdata.ChequeimagePath;
                        db.ChequeCollection.Add(Agentdata);

                        TotalCheque = Agentdata.UsedChequeAmt;
                        if (CurrencyHubStock != null)
                            CurrencyHubStock.TotalCheckAmt += TotalCheque;
                    }
                    else if (agentothermodpayementdata.agentOnlineCollectionDc != null)
                    {

                        OnlineCollection agonline = new OnlineCollection();
                        agonline.WarehouseId = Warehouse_id;
                        agonline.AgentId = agentothermodpayementdata.agentdueAmountDc[0].DBoyPeopleId;
                        agonline.MPOSAmt = agentothermodpayementdata.agentOnlineCollectionDc.CollectAmount;
                        agonline.PaymentGetwayAmt = agentothermodpayementdata.agentOnlineCollectionDc.PaymentGetwayAmt;
                        agonline.CurrencyHubStockId = CurrencyHubStock != null ? CurrencyHubStock.Id : (long?)null;
                        agonline.IsActive = true;
                        agonline.IsDeleted = false;
                        agonline.CreatedDate = indianTime;
                        agonline.ModifiedDate = indianTime;
                        agonline.CreatedBy = userid;
                        agonline.CurrencyCollectionId = null;
                        agonline.Orderid = agentothermodpayementdata.agentOnlineCollectionDc.Orderid;
                        agonline.MPOSReferenceNo = agentothermodpayementdata.agentOnlineCollectionDc.MPOSReferenceNo;
                        agonline.PaymentReferenceNO = agentothermodpayementdata.agentOnlineCollectionDc.PaymentReferenceNO;
                        agonline.PaymentFrom = agentothermodpayementdata.agentOnlineCollectionDc.PaymentType;
                        db.OnlineCollection.Add(agonline);

                        TotalOnline = agonline.MPOSAmt + agonline.PaymentGetwayAmt;

                        if (CurrencyHubStock != null)
                            CurrencyHubStock.TotalOnlineAmt += TotalOnline;
                    }


                    string Asd = "";

                    foreach (var agsettledata in agentothermodpayementdata.agentdueAmountDc.OrderByDescending(x => x.TotalDueAmt))
                    {

                        decimal amountpay = 0;
                        decimal TotalDue = 0;
                        //TotalCash = 0, TotalCheque = 0, TotalOnline
                        CurrencyCollection currencydata = db.CurrencyCollection.Where(x => x.Id == agsettledata.CurrencycollectionId).FirstOrDefault();
                        TotalDue = currencydata.TotalDueAmt + currencydata.Fine;

                        if (TotalCash > 0 && TotalDue > 0)
                        {
                            amountpay = 0;
                            if (TotalCash >= TotalDue)
                            {
                                amountpay = TotalDue;
                                TotalDue = 0;
                            }
                            else
                            {
                                amountpay = TotalCash;
                                TotalDue -= TotalCash;
                            }
                            currencydata.TotalCashAmt += amountpay;
                            TotalCash -= amountpay;
                        }

                        if (TotalCheque > 0 && TotalDue > 0)
                        {
                            amountpay = 0;
                            if (TotalCheque >= TotalDue)
                            {
                                amountpay = TotalDue;
                                TotalDue = 0;
                            }
                            else
                            {
                                amountpay = TotalCheque;
                                TotalDue -= TotalCheque;
                            }
                            currencydata.TotalCheckAmt += amountpay;
                            TotalCheque -= amountpay;
                        }

                        if (TotalOnline > 0 && TotalDue > 0)
                        {
                            amountpay = 0;
                            if (TotalOnline >= TotalDue)
                            {
                                amountpay = TotalDue;
                                TotalDue = 0;
                            }
                            else
                            {
                                amountpay = TotalOnline;
                                TotalDue -= TotalCheque;
                            }
                            currencydata.TotalOnlineAmt += amountpay;
                            TotalOnline -= amountpay;
                        }


                        if (Asd == "")
                        {
                            Asd = Convert.ToString(agsettledata.Deliveryissueid);
                        }
                        else
                        {

                            Asd = Asd + "," + Convert.ToString(agsettledata.Deliveryissueid);

                        }
                        AgentSettelment agsettle = new AgentSettelment();
                        agsettle.Deliveryissueid = agsettledata.Deliveryissueid;
                        agsettle.DBoyPeopleId = agsettledata.DBoyPeopleId;
                        agsettle.AgentId = agsettledata.Agentid;
                        agsettle.TotalDueAmt = agsettledata.TotalDueAmt;
                        agsettle.CreatedDate = indianTime;
                        agsettle.IsActive = true;
                        agsettle.IsDeleted = false;
                        agsettle.Warehouseid = agsettledata.Warehouseid;
                        agsettle.WareHouseSettleDate = agsettledata.WareHouseSettleDate;
                        agsettle.ModifiedDate = indianTime;
                        agsettle.CreatedBy = userid;
                        agsettle.Deliveryissueids = Asd;
                        db.AgentSettelment.Add(agsettle);
                        currencydata.TotalDueAmt = 0;
                        currencydata.Fine = 0;
                        db.Entry(currencydata).State = EntityState.Modified;
                    }

                    if (CurrencyHubStock != null && AgentCashCollections != null && AgentCashCollections.Any())
                    {
                        db.Entry(CurrencyHubStock).State = EntityState.Modified;
                        var cashCollection = db.HubCashCollection.Where(x => x.CurrencyHubStockId == CurrencyHubStock.Id);
                        foreach (var item in cashCollection)
                        {
                            if (AgentCashCollections.Any(x => x.CurrencyDenominationId == item.CurrencyDenominationId))
                                item.CurrencyCount += AgentCashCollections.FirstOrDefault(x => x.CurrencyDenominationId == item.CurrencyDenominationId).CurrencyCount.Value;

                            db.Entry(item).State = EntityState.Modified;
                        }
                    }



                    db.Commit();
                }
                return true;
            }

            catch (Exception ex)
            {
                logger.Error("Error in Settel: " + ex.Message);
            }
            return false;
        }
        #endregion


        #region Cash Balance Details for Warehosue 
        /// <summary>
        /// Cash Balance Details
        /// </summary>
        /// <param name="warehouseid"></param>
        /// <returns></returns>
        [Route("CashBalanceDetails")]
        [HttpGet]
        public LiveWarehouseCashDc CashBalanceDetails(int? warehouseid)
        {
            LiveWarehouseCashDc liveWarehouseCashDc = new LiveWarehouseCashDc();
            if (warehouseid > 0)
            {
                var identity = User.Identity as ClaimsIdentity;
                int userid = 0; List<string> roleNames = new List<string>();

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "RoleNames") && identity.Claims.FirstOrDefault(x => x.Type == "RoleNames").Value != "")
                    roleNames = (identity.Claims.FirstOrDefault(x => x.Type == "RoleNames").Value).Split(',').ToList();

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                    userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

                string Role = "";
                if (roleNames.Any(x => x == "Hub Cashier"))
                    Role = "Hub Cashier";
                else if (roleNames.Any(x => x == "Inbound Lead"))
                    Role = "Inbound Lead";
                else if (roleNames.Any(x => x == "Outbound Lead"))
                    Role = "Outbound Lead";
                try
                {
                    using (AuthContext context = new AuthContext())
                    {
                        var yesterdaydate = DateTime.Now.AddDays(-1);

                        long CurrencyHubStockId = 0;
                        var TodayHubCurrencyCollection = context.CurrencyHubStock.FirstOrDefault(x => x.Warehouseid == warehouseid
                                                                                       && EntityFunctions.TruncateTime(x.BOD) == EntityFunctions.TruncateTime(yesterdaydate));
                        List<hubCashCollectionDc> WarehouseOpeningCash = new List<hubCashCollectionDc>();
                        if (TodayHubCurrencyCollection != null)
                        {
                            long yesterdayhubcollectionid = TodayHubCurrencyCollection.Id;
                            CurrencyHubStockId = yesterdayhubcollectionid;

                            WarehouseOpeningCash = (
                               from c in context.CurrencyDenomination.Where(x => x.IsActive)
                               join p in context.HubCashCollection.Where(o => o.CurrencyHubStockId == yesterdayhubcollectionid && o.IsActive
                               && ((o.IsDeleted.HasValue && !o.IsDeleted.Value) || !o.IsDeleted.HasValue))
                               on c.Id equals p.CurrencyDenominationId into ps
                               from p in ps.DefaultIfEmpty()
                               select new hubCashCollectionDc
                               {
                                   CurrencyDenominationId = c == null ? 0 : c.Id,
                                   CurrencyCount = p == null ? 0 : (p.OpeningCurrencyCount),
                                   CurrencyDenominationTitle = c == null ? "" : c.Title,
                                   CurrencyDenominationValue = c == null ? 0 : c.Value,
                                   Id = p == null ? 0 : p.Id,
                                   CashCurrencyType = c == null ? "" : c.currencyType,
                               }).ToList();
                        }
                        else
                        {
                            WarehouseOpeningCash = (from c in context.CurrencyDenomination.Where(x => x.IsActive)
                                                    select new hubCashCollectionDc
                                                    {
                                                        CurrencyDenominationId = c == null ? 0 : c.Id,
                                                        CurrencyCount = 0,
                                                        CurrencyDenominationTitle = c == null ? "" : c.Title,
                                                        CurrencyDenominationValue = c == null ? 0 : c.Value,
                                                        CashCurrencyType = c == null ? "" : c.currencyType,
                                                    }).ToList();
                        }
                        liveWarehouseCashDc.WarehouseOpeningCash = new List<hubCashCollectionDc>();
                        liveWarehouseCashDc.WarehouseOpeningCash = WarehouseOpeningCash;


                        List<hubCashCollectionDc> WarehouseTodayCash = new List<hubCashCollectionDc>();
                        if (TodayHubCurrencyCollection != null)
                        {
                            var todayhubcollection = TodayHubCurrencyCollection;
                            WarehouseTodayCash = (
                               from c in context.CurrencyDenomination.Where(x => x.IsActive)
                               join p in context.HubCashCollection.Where(o => o.CurrencyHubStockId == todayhubcollection.Id && o.IsActive
                               && ((o.IsDeleted.HasValue && !o.IsDeleted.Value) || !o.IsDeleted.HasValue))
                               on c.Id equals p.CurrencyDenominationId into ps
                               from p in ps.DefaultIfEmpty()
                               select new hubCashCollectionDc
                               {
                                   CurrencyDenominationId = c == null ? 0 : c.Id,
                                   CurrencyCount = p == null ? 0 : p.CurrencyCount,
                                   CurrencyDenominationTitle = c == null ? "" : c.Title,
                                   CurrencyDenominationValue = c == null ? 0 : c.Value,
                                   BankDepositCurrencyCount = p == null ? 0 : p.BankSendCurrencyCount,
                                   ExchangeInCurrencyCount = p == null ? 0 : p.ExchangeCurrencyCount,
                                   Id = p == null ? 0 : p.Id,
                                   CashCurrencyType = c == null ? "" : c.currencyType,
                               }).ToList();
                            liveWarehouseCashDc.BOD = todayhubcollection.BOD;
                            liveWarehouseCashDc.CurrencyHubStockId = todayhubcollection.Id;
                            liveWarehouseCashDc.EOD = todayhubcollection.EOD;
                            liveWarehouseCashDc.IsBOD = true;
                            liveWarehouseCashDc.IsEOD = liveWarehouseCashDc.EOD.HasValue ? true : false;
                            liveWarehouseCashDc.previousDate = yesterdaydate;
                        }
                        else
                        {
                            WarehouseTodayCash = (from c in context.CurrencyDenomination.Where(x => x.IsActive)
                                                  select new hubCashCollectionDc
                                                  {
                                                      CurrencyDenominationId = c == null ? 0 : c.Id,
                                                      CurrencyCount = 0,
                                                      CurrencyDenominationTitle = c == null ? "" : c.Title,
                                                      CurrencyDenominationValue = c == null ? 0 : c.Value,
                                                      CashCurrencyType = c == null ? "" : c.currencyType,
                                                  }).ToList();
                            liveWarehouseCashDc.IsBOD = false;
                            liveWarehouseCashDc.IsEOD = false;
                            liveWarehouseCashDc.previousDate = yesterdaydate;
                        }
                        liveWarehouseCashDc.WarehouseTodayCash = new List<hubCashCollectionDc>();
                        liveWarehouseCashDc.WarehouseTodayCash = WarehouseTodayCash;
                        List<hubCashCollectionDc> WarehouseTodayClosingCash = new List<hubCashCollectionDc>();

                        WarehouseTodayClosingCash = WarehouseTodayCash.Union(WarehouseOpeningCash).GroupBy(x => new { x.CurrencyDenominationId, x.CurrencyDenominationTitle, x.CurrencyDenominationValue, x.CashCurrencyType })
                                                    .Select(x => new hubCashCollectionDc
                                                    {
                                                        CurrencyDenominationId = x.Key.CurrencyDenominationId,
                                                        CurrencyCount = x.Sum(y => y.CurrencyCount) - x.Sum(p => p.BankDepositCurrencyCount) + x.Sum(p => p.ExchangeInCurrencyCount),
                                                        CurrencyDenominationTitle = x.Key.CurrencyDenominationTitle,
                                                        CurrencyDenominationValue = x.Key.CurrencyDenominationValue,
                                                        CashCurrencyType = x.Key.CashCurrencyType,
                                                    }).ToList();

                        liveWarehouseCashDc.WarehouseClosingCash = WarehouseTodayClosingCash;

                        if (liveWarehouseCashDc.WarehouseTodayCash != null && liveWarehouseCashDc.WarehouseTodayCash.Count() > 0 && CurrencyHubStockId > 0)
                        {
                            var ExchangeHubCashCollections = context.ExchangeHubCashCollection.Where(o => o.CurrencyHubStockId == CurrencyHubStockId && o.IsActive
                                && ((o.IsDeleted.HasValue && !o.IsDeleted.Value) || !o.IsDeleted.HasValue));
                            foreach (var item in liveWarehouseCashDc.WarehouseTodayCash)
                            {
                                if (ExchangeHubCashCollections != null && ExchangeHubCashCollections.Any(x => x.CurrencyDenominationId == item.CurrencyDenominationId))
                                {
                                    item.ExchangeInCurrencyCount = ExchangeHubCashCollections.Any(x => x.CurrencyDenominationId == item.CurrencyDenominationId && x.CurrencyCount > 0) ? ExchangeHubCashCollections.Where(x => x.CurrencyDenominationId == item.CurrencyDenominationId && x.CurrencyCount > 0).Sum(x => x.CurrencyCount) : 0;
                                    item.ExchangeOutCurrencyCount = ExchangeHubCashCollections.Any(x => x.CurrencyDenominationId == item.CurrencyDenominationId && x.CurrencyCount < 0) ? ExchangeHubCashCollections.Where(x => x.CurrencyDenominationId == item.CurrencyDenominationId && x.CurrencyCount < 0).Sum(x => x.CurrencyCount) : 0;
                                }
                            }
                        }

                        var nextdayCurrencyHubStock = context.CurrencyHubStock.FirstOrDefault(x => x.Warehouseid == warehouseid && EntityFunctions.TruncateTime(x.BOD) == EntityFunctions.TruncateTime(DateTime.Now));
                        long todayStockId = nextdayCurrencyHubStock != null ? nextdayCurrencyHubStock.Id : 0;
                        liveWarehouseCashDc.IsBOD = context.CashBalanceCollection.Any(o => o.CurrencyHubStockId == todayStockId && o.IsActive
                                    && ((o.IsDeleted.HasValue && !o.IsDeleted.Value) || !o.IsDeleted.HasValue) && o.SubmittedRole == Role);

                    }
                }
                catch (Exception ex)
                {
                    logger.Error("Error in GetLiveWarehouseCashDeshboard Method: " + ex.Message);
                    return null;
                }
            }
            return liveWarehouseCashDc;
        }

        /// <summary>
        /// Cash Balance History
        /// </summary>
        /// <param name="warehouseid"></param>
        /// <returns></returns>
        [Route("CashBalanceHistory")]
        [HttpGet]
        public LiveWarehouseCashDc CashBalanceHistory(int warehouseid, DateTime historyDate, string Role)

        {
            LiveWarehouseCashDc liveWarehouseCashDc = new LiveWarehouseCashDc();
            if (warehouseid > 0)
            {
                var identity = User.Identity as ClaimsIdentity;
                int userid = 0; List<string> roleNames = new List<string>();

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "RoleNames") && identity.Claims.FirstOrDefault(x => x.Type == "RoleNames").Value != "")
                    roleNames = (identity.Claims.FirstOrDefault(x => x.Type == "RoleNames").Value).Split(',').ToList();

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                    userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

                if (string.IsNullOrEmpty(Role))
                {
                    if (roleNames.Any(x => x == "Hub Cashier"))
                        Role = "Hub Cashier";
                    else if (roleNames.Any(x => x == "Inbound Lead"))
                        Role = "Inbound Lead";
                    else if (roleNames.Any(x => x == "Outbound Lead"))
                        Role = "Outbound Lead";
                }
                try
                {
                    using (AuthContext context = new AuthContext())
                    {

                        long CurrencyHubStockId = 0;
                        var TodayHubCurrencyCollection = context.CurrencyHubStock.FirstOrDefault(x => x.Warehouseid == warehouseid
                                                                                       && EntityFunctions.TruncateTime(x.BOD) == EntityFunctions.TruncateTime(historyDate));

                        List<hubCashCollectionDc> WarehouseTodayCash = new List<hubCashCollectionDc>();
                        if (TodayHubCurrencyCollection != null)
                        {
                            var todayhubcollection = TodayHubCurrencyCollection;
                            CurrencyHubStockId = todayhubcollection.Id;
                            WarehouseTodayCash = (
                               from c in context.CurrencyDenomination.Where(x => x.IsActive)
                               join p in context.HubCashCollection.Where(o => o.CurrencyHubStockId == todayhubcollection.Id && o.IsActive
                               && ((o.IsDeleted.HasValue && !o.IsDeleted.Value) || !o.IsDeleted.HasValue))
                               on c.Id equals p.CurrencyDenominationId into ps
                               from p in ps.DefaultIfEmpty()
                               select new hubCashCollectionDc
                               {
                                   OpenCurrencyCount = p == null ? 0 : p.OpeningCurrencyCount,
                                   CurrencyDenominationId = c == null ? 0 : c.Id,
                                   CurrencyCount = p == null ? 0 : p.CurrencyCount,
                                   CurrencyDenominationTitle = c == null ? "" : c.Title,
                                   CurrencyDenominationValue = c == null ? 0 : c.Value,
                                   BankDepositCurrencyCount = p == null ? 0 : p.BankSendCurrencyCount,
                                   ExchangeInCurrencyCount = p == null ? 0 : p.ExchangeCurrencyCount,
                                   Id = p == null ? 0 : p.Id,
                                   CashCurrencyType = c == null ? "" : c.currencyType,
                               }).ToList();
                        }
                        else
                        {
                            WarehouseTodayCash = (from c in context.CurrencyDenomination.Where(x => x.IsActive)
                                                  select new hubCashCollectionDc
                                                  {
                                                      OpenCurrencyCount = 0,
                                                      CurrencyDenominationId = c == null ? 0 : c.Id,
                                                      CurrencyCount = 0,
                                                      CurrencyDenominationTitle = c == null ? "" : c.Title,
                                                      CurrencyDenominationValue = c == null ? 0 : c.Value,
                                                      CashCurrencyType = c == null ? "" : c.currencyType,
                                                  }).ToList();
                        }
                        liveWarehouseCashDc.WarehouseTodayCash = new List<hubCashCollectionDc>();
                        List<hubCashCollectionDc> WarehouseTodayClosingCash = new List<hubCashCollectionDc>();

                        WarehouseTodayClosingCash = WarehouseTodayCash.GroupBy(x => new { x.CurrencyDenominationId, x.CurrencyDenominationTitle, x.CurrencyDenominationValue, x.CashCurrencyType })
                                                    .Select(x => new hubCashCollectionDc
                                                    {
                                                        CurrencyDenominationId = x.Key.CurrencyDenominationId,
                                                        CurrencyCount = x.Sum(y => y.OpenCurrencyCount.Value) + x.Sum(y => y.CurrencyCount) - x.Sum(p => p.BankDepositCurrencyCount) + x.Sum(p => p.ExchangeInCurrencyCount),
                                                        CurrencyDenominationTitle = x.Key.CurrencyDenominationTitle,
                                                        CurrencyDenominationValue = x.Key.CurrencyDenominationValue,
                                                        CashCurrencyType = x.Key.CashCurrencyType,
                                                    }).ToList();

                        liveWarehouseCashDc.WarehouseClosingCash = WarehouseTodayClosingCash;

                        var Nextdaydate = historyDate.AddDays(1);
                        var nextdayCurrencyHubStock = context.CurrencyHubStock.FirstOrDefault(x => x.Warehouseid == warehouseid
                                                                                      && EntityFunctions.TruncateTime(x.BOD) == EntityFunctions.TruncateTime(Nextdaydate));
                        long nextdayStockId = nextdayCurrencyHubStock != null ? nextdayCurrencyHubStock.Id : 0;

                        List<hubCashCollectionDc> WarehouseTodayCashBalance = new List<hubCashCollectionDc>();
                        if (nextdayStockId != 0)
                        {
                            WarehouseTodayCashBalance = (
                                  from c in context.CurrencyDenomination.Where(x => x.IsActive)
                                  join p in context.CashBalanceCollection.Where(o => o.CurrencyHubStockId == nextdayStockId && o.IsActive
                                  && ((o.IsDeleted.HasValue && !o.IsDeleted.Value) || !o.IsDeleted.HasValue) && o.SubmittedRole == Role)
                                  on c.Id equals p.CurrencyDenominationId into ps
                                  from p in ps.DefaultIfEmpty()
                                  select new hubCashCollectionDc
                                  {
                                      OpenCurrencyCount = 0,
                                      CurrencyDenominationId = c == null ? 0 : c.Id,
                                      CurrencyCount = p == null ? 0 : p.CurrencyCount.Value,
                                      CurrencyDenominationTitle = c == null ? "" : c.Title,
                                      CurrencyDenominationValue = c == null ? 0 : c.Value,
                                      Id = p == null ? 0 : p.Id,
                                      CashCurrencyType = c == null ? "" : c.currencyType,
                                      ExchangeComment = p == null ? "" : p.Reason,
                                  }).ToList();
                        }
                        else
                        {
                            WarehouseTodayCashBalance = (from c in context.CurrencyDenomination.Where(x => x.IsActive)
                                                         select new hubCashCollectionDc
                                                         {
                                                             OpenCurrencyCount = 0,
                                                             CurrencyDenominationId = c == null ? 0 : c.Id,
                                                             CurrencyCount = 0,
                                                             CurrencyDenominationTitle = c == null ? "" : c.Title,
                                                             CurrencyDenominationValue = c == null ? 0 : c.Value,
                                                             CashCurrencyType = c == null ? "" : c.currencyType,
                                                         }).ToList();
                        }
                        liveWarehouseCashDc.WarehouseTodayCash = WarehouseTodayCashBalance;

                        if (liveWarehouseCashDc.WarehouseClosingCash != null)
                        {
                            List<hubCashCollectionDc> WarehousediffCashBalance = new List<hubCashCollectionDc>();
                            foreach (var item in liveWarehouseCashDc.WarehouseClosingCash)
                            {
                                int todaycount = 0;
                                if (liveWarehouseCashDc.WarehouseTodayCash != null && liveWarehouseCashDc.WarehouseTodayCash.Any(x => x.CurrencyDenominationId == item.CurrencyDenominationId))
                                {
                                    todaycount = liveWarehouseCashDc.WarehouseTodayCash.FirstOrDefault(x => x.CurrencyDenominationId == item.CurrencyDenominationId).CurrencyCount;
                                }

                                WarehousediffCashBalance.Add(new hubCashCollectionDc
                                {
                                    CurrencyDenominationId = item.CurrencyDenominationId,
                                    //CurrencyCount = todaycount == 0 ? 0 : item.CurrencyCount - todaycount,
                                    CurrencyCount = todaycount == 0 ? item.CurrencyCount - todaycount : todaycount > 0 ? item.CurrencyCount - todaycount : 0,
                                    CurrencyDenominationTitle = item.CurrencyDenominationTitle,
                                    CurrencyDenominationValue = item.CurrencyDenominationValue,
                                    Id = item.Id,
                                    CashCurrencyType = item.CashCurrencyType,
                                });
                            }

                            liveWarehouseCashDc.WarehouseOpeningCash = WarehousediffCashBalance;
                            if (liveWarehouseCashDc.WarehouseTodayCash != null && liveWarehouseCashDc.WarehouseTodayCash.Any())
                            {
                                liveWarehouseCashDc.comment = liveWarehouseCashDc.WarehouseTodayCash.FirstOrDefault().ExchangeComment;
                            }

                        }
                    }
                }
                catch (Exception ex)
                {
                    logger.Error("Error in CashBalanceDetails Method: " + ex.Message);
                    return null;
                }
            }
            return liveWarehouseCashDc;
        }
        #endregion

        #region CashBalanceCollection (17-09-2019)
        /// <summary>
        /// payment settel for Agent collection(24-09-2019) 
        /// </summary>
        /// <param name="agentothermodpayementdata"></param>
        /// <returns></returns>      
        [Route("CashBalanceCollection")]
        [HttpPost]
        public bool CashBalanceCollection(CashBalanceCollectiondata CashBalanceCollectiondata)
        {
            bool result = false;
            try
            {
                var identity = User.Identity as ClaimsIdentity;
                int compid = 0, userid = 0, Warehouse_id = 0; List<string> roleNames = new List<string>();

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))
                    compid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                    userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "Warehouseid"))
                    Warehouse_id = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "Warehouseid").Value);



                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "RoleNames") && identity.Claims.FirstOrDefault(x => x.Type == "RoleNames").Value != "")
                    roleNames = (identity.Claims.FirstOrDefault(x => x.Type == "RoleNames").Value).Split(',').ToList();

                string Role = "";

                if (string.IsNullOrEmpty(Role))
                {
                    if (roleNames.Any(x => x == "Hub Cashier"))
                        Role = "Hub Cashier";
                    else if (roleNames.Any(x => x == "Inbound Lead"))
                        Role = "Inbound Lead";
                    else if (roleNames.Any(x => x == "Outbound Lead"))
                        Role = "Outbound Lead";
                }

                using (AuthContext db = new AuthContext())
                {
                    long CurrencyHubStockId = 0;
                    var TodayHubCurrencyCollection = db.CurrencyHubStock.FirstOrDefault(x => x.Warehouseid == Warehouse_id
                                                                                   && EntityFunctions.TruncateTime(x.BOD) == EntityFunctions.TruncateTime(DateTime.Now));
                    if (TodayHubCurrencyCollection != null)
                        CurrencyHubStockId = TodayHubCurrencyCollection.Id;

                    var todayCashBalance = db.CashBalanceCollection.Where(o => o.CurrencyHubStockId == CurrencyHubStockId && o.IsActive && ((o.IsDeleted.HasValue && !o.IsDeleted.Value) || !o.IsDeleted.HasValue));

                    foreach (var cashdata in CashBalanceCollectiondata.cashbalancecollectionDc)
                    {
                        if (!todayCashBalance.Any(x => x.CurrencyDenominationId == cashdata.Id))
                        {
                            CashBalanceCollection Cash = new CashBalanceCollection();
                            Cash.CurrencyDenominationId = Convert.ToInt32(cashdata.Id);
                            if (cashdata.CurrencyCount != null && cashdata.CurrencyCount > 0)
                            {
                                Cash.CurrencyCount = cashdata.CurrencyCount;
                            }
                            else
                            {
                                Cash.CurrencyCount = 0;

                            }
                            Cash.SubmittedRole = Role;
                            Cash.CreatedDate = indianTime;
                            Cash.IsActive = true;
                            Cash.IsDeleted = false;
                            Cash.ModifiedDate = DateTime.Now;
                            Cash.CreatedBy = userid;
                            Cash.WarehouseId = Warehouse_id;
                            Cash.CurrencyHubStockId = CurrencyHubStockId;
                            Cash.Reason = CashBalanceCollectiondata.Reason;
                            db.CashBalanceCollection.Add(Cash);
                        }
                        else
                        {
                            var cash = todayCashBalance.FirstOrDefault(x => x.CurrencyDenominationId == cashdata.Id);
                            cash.CurrencyCount = cashdata.CurrencyCount;
                            cash.SubmittedRole = Role;
                            cash.ModifiedBy = userid;
                            db.Entry(cash).State = EntityState.Modified;
                        }

                        db.Commit();
                    }
                }
                return true;
            }

            catch (Exception ex)
            {
                logger.Error("Error in Settel: " + ex.Message);
            }
            return false;
        }
        #endregion

        #region Return Cheque Paginator Count data (11-10-2019)
        /// <summary>
        /// Return ChequePaginatorCountdata
        /// </summary>
        /// <param name="totalitem"></param>
        /// <param name="page"></param>
        /// <param name="warehouseid"></param>
        /// <returns></returns>
        [Route("ReturnChequePaginatorCountdata")]
        [HttpGet]
        public ReturnChequePaginatorCount ReturnChequePaginatorCount(int totalitem, int page, int warehouseid)
        {
            try
            {
                var identity = User.Identity as ClaimsIdentity;
                int compid = 0, userid = 0, Warehouse_id = 0;

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))
                    compid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                    userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "Warehouseid"))
                    Warehouse_id = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "Warehouseid").Value);

                logger.Info("User ID : {0} , Company Id : {1}", compid, userid);

                ReturnChequePaginatorCount returnchequepaginatorcount = new ReturnChequePaginatorCount();
                Warehouse_id = warehouseid;
                using (AuthContext context = new AuthContext())
                {
                    List<ReturnChequeCollectionDc> Returncheque = new List<ReturnChequeCollectionDc>();
                    if (Warehouse_id > 0)
                    {
                        context.Database.Log = s => Debug.WriteLine(s);
                        var totalcount = context.ReturnChequeCollection.Where(x => x.IsDeleted == false && x.ChequeCollection.ChequeAmt > 0 && x.ChequeCollection.CurrencyCollection.Warehouseid == Warehouse_id && x.ReturnchequeId == null && x.PaymentCollectionType == null && x.IsCollect == false).ToList().Count;
                        var Return = context.ReturnChequeCollection.Where(x => x.IsDeleted == false && x.ChequeCollection.ChequeAmt > 0 && x.ChequeCollection.CurrencyCollection.Warehouseid == Warehouse_id && x.ReturnchequeId == null && x.PaymentCollectionType == null && x.IsCollect == false).Select(y => new ReturnChequeCollectionDc
                        {
                            Id = y.Id,
                            Deliveryissueid = y.ChequeCollection.CurrencyCollection.Deliveryissueid,
                            ChequeAmt = y.ChequeCollection.ChequeAmt,
                            HandOverAgentName = y.HandOverAgentName,
                            HandOverDate = y.HandOverDate,
                            ChequeBankName = y.ChequeCollection.ChequeBankName,
                            PodNo = y.PodNo,
                            ChequeNumber = y.ChequeCollection.ChequeNumber,
                            Status = y.Status,
                            CreatedDate = indianTime,
                            IsActive = true,
                            IsDeleted = false,
                            ChequeDate = y.ChequeCollection.ChequeDate,
                            OrderId = y.ChequeCollection.Orderid,
                            Fine = y.Fine,
                        }).OrderByDescending(x => x.Deliveryissueid).Skip((page - 1) * totalitem).Take(totalitem).ToList();

                        returnchequepaginatorcount.total_count = totalcount;
                        returnchequepaginatorcount.ReturnChequeChargeDc = Return;
                    }
                    return returnchequepaginatorcount;
                }
            }



            catch (Exception ex)
            {
                logger.Error("Error in getting Peoples " + ex.Message);
                return null;
            }
        }
        #endregion

        /// <summary>
        /// Add New cheque
        /// </summary>
        /// <param name="ChequeList"></param>
        /// <returns></returns>
        [Route("CashPayment")]
        [HttpPost]
        public bool CashPayment(CashdataDc cashData)
        {
            bool result = false;
            try
            {
                var identity = User.Identity as ClaimsIdentity;
                int compid = 0, userid = 0, Warehouse_id = 0;

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))
                    compid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                    userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "Warehouseid"))
                    Warehouse_id = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "Warehouseid").Value);


                using (AuthContext db = new AuthContext())
                {
                    People People = db.Peoples.Where(x => x.PeopleID == userid).FirstOrDefault();
                    var CurrencyHubStock = db.CurrencyHubStock.FirstOrDefault(x => x.Warehouseid == Warehouse_id && EntityFunctions.TruncateTime(x.BOD) == EntityFunctions.TruncateTime(DateTime.Now));
                    ReturnChequeCollection ReturnChequedata = db.ReturnChequeCollection.Where(x => x.Id == cashData.Id).FirstOrDefault();
                    var GetcashCollectionId = db.ChequeCollection.Where(x => x.Orderid == cashData.OrderId).Select(x => x.CurrencyCollectionId).FirstOrDefault();

                    var denomination = db.CurrencyDenomination.Where(x => x.IsActive).ToList();
                    List<CashCollection> CashCollections = new List<CashCollection>();
                    if (GetcashCollectionId > 0)
                    {
                        var denominationids = cashData.cashList.Where(x => x.CurrencyCount > 0).Select(x => x.Id).ToList();
                        var cashCollections = db.CashCollection.Where(x => x.CurrencyCollectionId == GetcashCollectionId && denominationids.Contains(x.CurrencyDenominationId));
                        foreach (var cash in cashCollections)
                        {
                            cash.CurrencyCountByDBoy = cashData.cashList.Any(x => x.Id == cash.CurrencyDenominationId) ? cash.CurrencyCountByDBoy + cashData.cashList.FirstOrDefault(x => x.Id == cash.CurrencyDenominationId).CurrencyCount : cash.CurrencyCountByDBoy;
                            cash.ModifiedBy = userid;
                            cash.ModifiedDate = indianTime;
                            db.Entry(cash).State = EntityState.Modified;
                        }

                        List<HubCashCollection> hubCashCollections = db.HubCashCollection.Where(x => x.IsDeleted == false && x.CurrencyHubStockId == CurrencyHubStock.Id && denominationids.Contains(x.CurrencyDenominationId)).ToList();
                        if (hubCashCollections != null && hubCashCollections.Any())
                        {
                            foreach (var hubCashCollection in hubCashCollections)
                            {
                                hubCashCollection.CurrencyCount = cashData.cashList.Any(x => x.Id == hubCashCollection.CurrencyDenominationId) ? hubCashCollection.CurrencyCount + cashData.cashList.FirstOrDefault(x => x.Id == hubCashCollection.CurrencyDenominationId).CurrencyCount : hubCashCollection.CurrencyCount;
                                hubCashCollection.ModifiedBy = userid;
                                db.Entry(hubCashCollection).State = EntityState.Modified;
                            }
                        }
                        //    CurrencyCollection currencydata = db.CurrencyCollection.Where(x => x.Id == GetcashCollectionId).FirstOrDefault();
                        //    currencydata.TotalCashAmt += cashData.Amount;
                        //    currencydata.TotalDueAmt = currencydata.TotalDeliveryissueAmt - (currencydata.TotalCashAmt + currencydata.TotalCheckAmt + currencydata.TotalOnlineAmt);
                        //    currencydata.ModifiedDate = DateTime.Now;
                        //    currencydata.ModifiedBy = userid;
                        //    db.Entry(currencydata).State = EntityState.Modified;

                        //}

                        //ChequeCollection chequedata = db.ChequeCollection.Where(x => x.Orderid == cashData.OrderId && x.ChequeStatus == 4).FirstOrDefault();
                        //chequedata.ChequeStatus = 6;
                        //chequedata.Fine = Convert.ToInt32(cashData.Amount - chequedata.ChequeAmt);
                        //chequedata.ModifiedBy = userid;
                        //db.Entry(chequedata).State = EntityState.Modified;

                        CurrencyCollection currencydata = db.CurrencyCollection.Where(x => x.Id == GetcashCollectionId).FirstOrDefault();
                        currencydata.TotalCashAmt += cashData.Amount;
                        currencydata.TotalDueAmt = currencydata.TotalDeliveryissueAmt - (currencydata.TotalCashAmt + currencydata.TotalCheckAmt + currencydata.TotalOnlineAmt);
                        currencydata.ModifiedDate = DateTime.Now;
                        currencydata.ModifiedBy = userid;
                        db.Entry(currencydata).State = EntityState.Modified;


                        ChequeCollection chequedata = db.ChequeCollection.Where(x => x.Orderid == cashData.OrderId && x.ChequeStatus == 4 && x.Id == ReturnChequedata.ChequeCollectionId && ReturnChequedata.IsCollect == false).FirstOrDefault();
                        if (chequedata != null)
                        {
                            chequedata.ChequeStatus = 6;
                            chequedata.ModifiedBy = userid;
                            chequedata.Fine = Convert.ToInt32(cashData.Amount - chequedata.ChequeAmt);
                            db.Entry(chequedata).State = EntityState.Modified;
                        }
                        var fine = db.ChequeCollection.Where(x => x.CurrencyCollectionId == GetcashCollectionId && x.ChequeStatus == 6).Select(x => x.Fine).ToList();
                        if (fine != null)
                        {
                            currencydata.Fine = fine.Sum() + chequedata.Fine;
                        }
                        else
                        {
                            currencydata.Fine = chequedata.Fine;
                        }

                        db.Entry(currencydata).State = EntityState.Modified;

                        ReturnChequedata.PaymentCollectionType = "Cash";
                        ReturnChequedata.IsCollect = true;
                        ReturnChequedata.ModifiedBy = userid;
                        ReturnChequedata.Fine = Convert.ToInt32(cashData.Amount - chequedata.ChequeAmt);
                        db.Entry(ReturnChequedata).State = EntityState.Modified;

                        db.PaymentResponseRetailerAppDb.Add(new GenricEcommers.Models.PaymentResponseRetailerApp
                        {
                            amount = Convert.ToDouble(cashData.Amount),
                            CreatedDate = DateTime.Now,
                            UpdatedDate = DateTime.Now,
                            status = "Success",
                            PaymentFrom = "Cash",
                            PaymentThrough = "Offline",
                            statusDesc = "New Cash Added on Behalf of Bounce Cheque",
                            OrderId = cashData.OrderId,

                        });
                        db.Commit();

                    }
                    return true;

                }
            }

            catch (Exception ex)
            {
                logger.Error("Error in Settel: " + ex.Message);
            }
            return false;
        }

        /// <summary>
        /// Add New cheque
        /// </summary>
        /// <param name="ChequeList"></param>
        /// <returns></returns>
        [Route("ChequePaymentAdd")]
        [HttpPost]
        public bool ChequePaymentAdd(ChequePaymentDC ChequeList)
        {
            bool result = false;
            try
            {
                var identity = User.Identity as ClaimsIdentity;
                int compid = 0, userid = 0;

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))
                    compid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                    userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
                using (AuthContext db = new AuthContext())
                {

                    People People = db.Peoples.Where(x => x.PeopleID == userid).FirstOrDefault();
                    var BankName = db.CurrencySettlementBank.Where(x => x.Id == ChequeList.BankId).Select(x => x.BankName).FirstOrDefault();
                    //ChequeCollection cheque = db.ChequeCollection.Where(y => y.CurrencyCollectionId).FirstOrDefault();
                    ReturnChequeCollection ReturnChequedata = db.ReturnChequeCollection.Where(x => x.Id == ChequeList.Id).FirstOrDefault();
                    var GetCollectionId = db.ChequeCollection.Where(x => x.Id == ReturnChequedata.ChequeCollectionId).Select(x => x.CurrencyCollectionId).FirstOrDefault();

                    ChequeCollection Payment = new ChequeCollection();

                    Payment.ChequeStatus = ChequeList.Status = 1;
                    Payment.ChequeAmt = ChequeList.ChequeAmt;
                    Payment.ChequeBankName = BankName;
                    Payment.ChequeNumber = ChequeList.ChequeNumber;
                    Payment.WarehouseId = ChequeList.Warehouseid;
                    Payment.Orderid = ChequeList.OrderId;
                    Payment.ChequeDate = ChequeList.ChequeDate;
                    Payment.CreatedDate = indianTime;
                    Payment.CreatedBy = userid;
                    Payment.CurrencyCollectionId = GetCollectionId;
                    Payment.IsActive = true;
                    Payment.IsDeleted = false;
                    Payment.ChequeimagePath = ChequeList.ChequeimagePath;
                    db.ChequeCollection.Add(Payment);



                    CurrencyCollection currencydata = db.CurrencyCollection.Where(x => x.Id == Payment.CurrencyCollectionId).FirstOrDefault();
                    currencydata.TotalCheckAmt += Payment.ChequeAmt;
                    currencydata.TotalDueAmt = currencydata.TotalDeliveryissueAmt - (currencydata.TotalCashAmt + currencydata.TotalCheckAmt + currencydata.TotalOnlineAmt);
                    currencydata.ModifiedBy = userid;
                    //currencydata.Fine = Payment.Fine;


                    ChequeCollection chequedata = db.ChequeCollection.Where(x => x.Orderid == Payment.Orderid && x.ChequeStatus == 4 && x.Id == ReturnChequedata.ChequeCollectionId && ReturnChequedata.IsCollect == false).FirstOrDefault();
                    if (chequedata != null)
                    {
                        chequedata.ChequeStatus = 6;
                        chequedata.ModifiedBy = userid;
                        chequedata.Fine = Convert.ToInt32(Payment.ChequeAmt - chequedata.ChequeAmt);
                        db.Entry(chequedata).State = EntityState.Modified;
                    }

                    ReturnChequedata.ReturnchequeId = Payment.Id;
                    ReturnChequedata.IsCollect = true;
                    ReturnChequedata.PaymentCollectionType = "Cheque";
                    ReturnChequedata.ModifiedBy = userid;
                    ReturnChequedata.Fine = Convert.ToInt32(ChequeList.ChequeAmt - chequedata.ChequeAmt);
                    db.Entry(ReturnChequedata).State = EntityState.Modified;

                    var fine = db.ChequeCollection.Where(x => x.CurrencyCollectionId == Payment.CurrencyCollectionId && x.ChequeStatus == 6).Select(x => x.Fine).ToList(); ;
                    if (fine != null)
                    {
                        currencydata.Fine = fine.Sum() + chequedata.Fine;
                    }
                    else
                    {
                        currencydata.Fine = chequedata.Fine;
                    }

                    db.Entry(currencydata).State = EntityState.Modified;

                    db.PaymentResponseRetailerAppDb.Add(new GenricEcommers.Models.PaymentResponseRetailerApp
                    {
                        amount = Convert.ToDouble(Payment.ChequeAmt),
                        CreatedDate = DateTime.Now,
                        UpdatedDate = DateTime.Now,
                        status = "Success",
                        PaymentFrom = "Cheque",
                        OrderId = ChequeList.OrderId,
                        statusDesc = "New Cheque Added on Behalf of Bounce",
                        GatewayTransId = Payment.ChequeNumber
                    });

                    db.Commit();

                }

                return true;
            }

            catch (Exception ex)
            {
                logger.Error("Error in Settel: " + ex.Message);
            }
            return false;
        }

        /// <summary>
        /// Add New cheque
        /// </summary>
        /// <param name="ChequeList"></param>
        /// <returns></returns>
        [Route("OnlinePayment")]
        [HttpPost]
        public bool OnlinePayment(OnlineCollectionDc onlinedata)
        {

            bool result = false;
            try
            {
                var identity = User.Identity as ClaimsIdentity;
                int compid = 0, userid = 0;

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))
                    compid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                    userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
                using (AuthContext db = new AuthContext())
                {

                    People People = db.Peoples.Where(x => x.PeopleID == userid).FirstOrDefault();
                    ReturnChequeCollection ReturnChequedata = db.ReturnChequeCollection.Where(x => x.Id == onlinedata.Id).FirstOrDefault();
                    var GetOnlineCollectionId = db.ChequeCollection.Where(x => x.Orderid == onlinedata.Orderid).Select(x => x.CurrencyCollectionId).FirstOrDefault();

                    //var GetOnlineCollectionId = db.OnlineCollection.Where(x => x.Id == onlinedata.Orderid).Select(x => x.CurrencyCollectionId).FirstOrDefault();
                    OnlineCollection Online = new OnlineCollection();

                    Online.PaymentGetwayAmt = onlinedata.PaymentGetwayAmt;
                    Online.MPOSAmt = onlinedata.MPOSAmt;
                    Online.PaymentReferenceNO = onlinedata.PaymentReferenceNO;
                    Online.MPOSReferenceNo = onlinedata.MPOSReferenceNo;
                    Online.Orderid = onlinedata.Orderid;
                    Online.CreatedDate = indianTime;
                    Online.CreatedBy = userid;
                    Online.CurrencyCollectionId = GetOnlineCollectionId;
                    Online.IsActive = true;
                    Online.IsDeleted = false;
                    Online.PaymentFrom = onlinedata.PaymentFrom;
                    db.OnlineCollection.Add(Online);



                    //ChequeCollection chequedata = db.ChequeCollection.Where(x => x.Orderid == Online.Orderid && x.ChequeStatus == 4).FirstOrDefault();
                    //chequedata.ChequeStatus = 6 ;
                    //chequedata.Fine = Convert.ToInt32(Online.MPOSAmt - chequedata.ChequeAmt);
                    //chequedata.ModifiedBy = userid;
                    //db.Entry(chequedata).State = EntityState.Modified;

                    CurrencyCollection currencydata = db.CurrencyCollection.Where(x => x.Id == Online.CurrencyCollectionId).FirstOrDefault();
                    if (Online.MPOSAmt > 0)
                    {
                        currencydata.TotalOnlineAmt += Online.MPOSAmt;
                    }
                    if (Online.PaymentGetwayAmt > 0)
                    {
                        currencydata.TotalOnlineAmt += Online.PaymentGetwayAmt;
                    }
                    currencydata.TotalDueAmt = currencydata.TotalDeliveryissueAmt - (currencydata.TotalCashAmt + currencydata.TotalCheckAmt + currencydata.TotalOnlineAmt);
                    currencydata.ModifiedBy = userid;

                    ChequeCollection chequedata = db.ChequeCollection.Where(x => x.Orderid == Online.Orderid && x.ChequeStatus == 4 && x.Id == ReturnChequedata.ChequeCollectionId && ReturnChequedata.IsCollect == false).FirstOrDefault();
                    if (chequedata != null)
                    {
                        chequedata.ChequeStatus = 6;
                        chequedata.ModifiedBy = userid;
                        if (Online.MPOSAmt > 0)
                        {
                            chequedata.Fine = Convert.ToInt32(Online.MPOSAmt - chequedata.ChequeAmt);
                        }
                        if (Online.PaymentGetwayAmt > 0)
                        {
                            chequedata.Fine = Convert.ToInt32(Online.PaymentGetwayAmt - chequedata.ChequeAmt);
                        }
                        db.Entry(chequedata).State = EntityState.Modified;
                    }

                    ReturnChequedata.PaymentCollectionType = "Online";
                    ReturnChequedata.IsCollect = true;
                    ReturnChequedata.ModifiedBy = userid;
                    if (Online.MPOSAmt > 0)
                    {
                        ReturnChequedata.Fine = Convert.ToInt32(onlinedata.MPOSAmt - chequedata.ChequeAmt);
                    }
                    if (Online.PaymentGetwayAmt > 0)
                    {
                        ReturnChequedata.Fine = Convert.ToInt32(onlinedata.PaymentGetwayAmt - chequedata.ChequeAmt);
                    }
                    db.Entry(ReturnChequedata).State = EntityState.Modified;

                    var fine = db.ChequeCollection.Where(x => x.CurrencyCollectionId == Online.CurrencyCollectionId && x.ChequeStatus == 6).Select(x => x.Fine).ToList();
                    if (fine != null)
                    {
                        currencydata.Fine = fine.Sum() + chequedata.Fine;
                    }
                    else
                    {
                        currencydata.Fine = chequedata.Fine;
                    }

                    db.Entry(currencydata).State = EntityState.Modified;

                    if (Online.MPOSAmt > 0)
                    {
                        db.PaymentResponseRetailerAppDb.Add(new GenricEcommers.Models.PaymentResponseRetailerApp
                        {
                            amount = Convert.ToDouble(Online.MPOSAmt),
                            CreatedDate = DateTime.Now,
                            UpdatedDate = DateTime.Now,
                            status = "Success",
                            PaymentFrom = "mPos",
                            PaymentThrough = "Online",
                            OrderId = onlinedata.Orderid,
                            statusDesc = "New Mpos Payment Added on Behalf of Bounce Cheque",
                            GatewayTransId = Online.MPOSReferenceNo,
                            IsOnline = true
                        });
                    }
                    else if (Online.PaymentGetwayAmt > 0)
                    {
                        db.PaymentResponseRetailerAppDb.Add(new GenricEcommers.Models.PaymentResponseRetailerApp
                        {
                            amount = Convert.ToDouble(Online.PaymentGetwayAmt),
                            CreatedDate = DateTime.Now,
                            UpdatedDate = DateTime.Now,
                            status = "Success",
                            PaymentFrom = "RTGS/NEFT",
                            PaymentThrough = "Online",
                            OrderId = onlinedata.Orderid,
                            statusDesc = "New Online Payment Added on Behalf of Bounce Cheque",
                            GatewayTransId = Online.PaymentReferenceNO,
                            IsOnline = true
                        });
                    }

                    db.Commit();


                    return true;
                }
            }
            catch (Exception ex)
            {
                logger.Error("Error in Settel: " + ex.Message);
            }
            return false;
        }
        #region UpdateWorkingCapital

        [Route("UpdateWC")]
        [HttpGet]
        [AllowAnonymous]
        public bool UpdateWC()
        {
            WorkingCapitalManager cmContoller = new WorkingCapitalManager();
            return cmContoller.UpdateWorkingCapital();
        }

        //        public bool UpdateWorkingCapital()
        //        {

        //            bool result = false;
        //            MongoDbHelper<MongoOrderMaster> mongoDbHelper = new MongoDbHelper<MongoOrderMaster>();
        //            MongoDbHelper<WorkingCapitalData> mongoDbHelperWorkingCapital = new MongoDbHelper<WorkingCapitalData>();
        //            var orderMasters = new List<MongoOrderMaster>();
        //            var salesorderMasters = new List<MongoOrderMaster>();
        //            var orderStatusAmount = new List<OrderStatusAmount>();
        //            var orderNotReconsileAmount = new List<OrderStatusAmount>();
        //            var salesorderStatusAmount = new List<OrderStatusAmount>();
        //            var hubInventory = new List<HubAvgInventory>();
        //            var CashInOperation = new List<WarehouseCashAmount>();
        //            var WarehouseChequeAmount = new List<WarehouseChequeAmount>();
        //            var WarehouseSupplierAmount = new List<WarehouseCashAmount>();
        //            var WarehouseSupplierAdvanceAmount = new List<WarehouseCashAmount>();
        //            var WarehouseIRPendingBuyerAmount = new List<WarehouseCashAmount>();
        //            var WarehouseAgentAmount = new List<WarehouseCashAmount>();
        //            var DamageInventory = new List<WarehouseCashAmount>();
        //            var PendingGRN = new List<WarehouseCashAmount>();
        //            var GoodsReceivedNotInvoiced = new List<WarehouseCashAmount>();
        //            var WarehouseOnlinAmount = new List<WarehouseCashAmount>();
        //            var onlinePaymentV2WithpaymentFrom = new List<OnlinePaymentV2WithpaymentFrom>();
        //            DateTime WCDate = DateTime.Now.AddDays(-1).Date;
        //            var startDate = new DateTime(WCDate.Year, WCDate.Month, WCDate.Day);
        //            var endDate = startDate.AddDays(1).AddMilliseconds(-1);
        //            var taskList = new List<Task>();
        //            var AssignmentList = new List<int>();
        //            bool isGenerated = mongoDbHelperWorkingCapital.Select(x => x.CreateDate == WCDate
        //                                , collectionName: "WorkingCapitalData")
        //                                .Any();

        //            //isGenerated = false;
        //            if (!isGenerated)
        //            {


        //                var task1 = Task.Factory.StartNew(() =>
        //                {
        //                    //orderMasters = mongoDbHelper.Select(x => !x.Deleted && (x.Status == "Shipped" || x.Status == "Issued" || x.Status == "Ready to Dispatch" || x.Status == "Delivery Redispatch"
        //                    //                                    || x.Status == "Delivery Canceled")
        //                    //                , collectionName: "OrderMaster")
        //                    //                .ToList();



        //                    using (var context = new AuthContext())
        //                    {
        //                        //AssignmentList = context.DeliveryIssuanceDb.Where(x => x.Status == "Payment Submitted" || x.Status == "Payment Accepted" || x.Status == "Submitted").Select(x => x.DeliveryIssuanceId).ToList();
        //                        //var ReconciledOrder = orderMasters.Where(x => x.DeliveryIssuanceIdOrderDeliveryMaster.HasValue && AssignmentList.Contains(x.DeliveryIssuanceIdOrderDeliveryMaster.Value) && x.Status== "Delivered").Select(x => new { x.WarehouseId, x.OrderId, Totalamount = (x.DispatchAmount ?? x.GrossAmount) }).ToList();
        //                        //var OrderIds = ReconciledOrder.Select(x => x.OrderId).ToList();
        //                        //orderNotReconsileAmount = ReconciledOrder.GroupBy(x => x.WarehouseId).Select(x => new OrderStatusAmount { WarehouseId = x.Key, TotalAmount = x.Sum(z => z.Totalamount) }).ToList();
        //                        context.Database.CommandTimeout = 600;
        //                        var query = "select a.WarehouseId,sum(a.GrossAmount) TotalAmount from OrderDispatchedMasters a with (nolock) inner join DeliveryIssuances b with (nolock) on a.DeliveryIssuanceIdOrderDeliveryMaster=b.DeliveryIssuanceId and b.Status in ('Payment Submitted','Payment Accepted','Submitted') and a.Status ='Delivered'  group by a.WarehouseId";
        //                        orderNotReconsileAmount = context.Database.SqlQuery<OrderStatusAmount>(query).ToList();
        //                        query = "select a.WarehouseId,a.Status,sum(a.GrossAmount) TotalAmount from OrderDispatchedMasters a with (nolock) where a.Status in ('Shipped','Issued','Ready to Dispatch','Delivery Redispatch','Delivery Canceled') group by a.WarehouseId,a.Status";
        //                        orderStatusAmount = context.Database.SqlQuery<OrderStatusAmount>(query).ToList();

        //                        //orderStatusAmount = orderMasters.GroupBy(x => new { x.WarehouseId, x.Status }).Select(x => new OrderStatusAmount
        //                        //{
        //                        //    WarehouseId = x.Key.WarehouseId,
        //                        //    Status = x.Key.Status,
        //                        //    TotalAmount = x.Sum(z => z.DispatchAmount ?? z.GrossAmount)
        //                        //}).ToList();
        //                    }
        //                });

        //                taskList.Add(task1);


        //                var task2 = Task.Factory.StartNew(() =>
        //                {
        //                    using (var context = new AuthContext())
        //                    {
        //                        context.Database.CommandTimeout = 600;
        //                        // var query = "select c.WarehouseId, sum(c.CurrentInventory * item.purchasePrice) as Inventory from CurrentStocks c with (nolock) inner join GMWarehouseProgresses gw with (nolock) on c.WarehouseId=gw.WarehouseID and gw.IsLaunched=1 cross apply(select max(i.netpurchaseprice) purchasePrice from  itemmasters i  with (nolock) where c.WarehouseId = i.WarehouseId and c.ItemNumber = i.Number and c.ItemMultiMRPId = i.ItemMultiMRPId and i.Deleted=0 group by i.WarehouseId, i.ItemMultiMRPId ) item group by c.WarehouseId";
        //                        var query = "select C.WarehouseId,sum(remqty * price) as Inventory from inqueue c with (nolock)  where cast(createddate as date) =  cast(DATEADD(day,-1,getdate()) as date) group by c.WarehouseId";
        //"
        //                        hubInventory = context.Database.SqlQuery<HubAvgInventory>(query).ToList();
        //                    }
        //                });

        //                taskList.Add(task2);

        //                var task3 = Task.Factory.StartNew(() =>
        //                {
        //                    using (var context = new AuthContext())
        //                    {
        //                        context.Database.CommandTimeout = 600;
        //                        var query = "select c.Warehouseid,Cast(sum(a.openingcurrencycount * b.Value) as decimal(18,2)) as Amount  from CurrencyHubStocks c with(nolock) inner join  HubCashCollections a  with(nolock)  on c.id=a.CurrencyHubStockId and cast(c.BOD as date)=cast(getdate() as date) inner join CurrencyDenominations b  with(nolock)  on a.CurrencyDenominationId=b.Id group by c.Warehouseid";
        //                        CashInOperation = context.Database.SqlQuery<WarehouseCashAmount>(query).ToList();
        //                    }
        //                });

        //                taskList.Add(task3);

        //                var task4 = Task.Factory.StartNew(() =>
        //                {
        //                    using (var context = new AuthContext())
        //                    {
        //                        context.Database.CommandTimeout = 600;
        //                        var query = "select a.ChequeStatus,Cast(Sum(a.ChequeAmt) as decimal(18,2)) as Amount,b.WarehouseId from ChequeCollections a  with(nolock) inner join  ordermasters b  with(nolock) on a.orderid=b.OrderId and b.Status<>'sattled'  where a.ChequeStatus in (1,2,4) and a.IsActive=1 and (a.IsDeleted is null or a.IsDeleted=0) group by b.WarehouseId,a.ChequeStatus";
        //                        WarehouseChequeAmount = context.Database.SqlQuery<WarehouseChequeAmount>(query).ToList();
        //                    }
        //                });

        //                taskList.Add(task4);
        //                var task5 = Task.Factory.StartNew(() =>
        //                {

        //                    //salesorderMasters = mongoDbHelper.Select(x => !x.Deleted && (x.Status != "Payment Pending" && x.Status != "Inactive" && x.Status != "Failed" && x.Status != "Dummy Order Cancelled" )
        //                    //                       && x.CreatedDate >= startDate && x.CreatedDate <= endDate, collectionName: "OrderMaster")
        //                    //                        .ToList();


        //                    //salesorderStatusAmount = salesorderMasters.GroupBy(x => new { x.WarehouseId }).Select(x => new OrderStatusAmount
        //                    //{
        //                    //    WarehouseId = x.Key.WarehouseId,
        //                    //    TotalAmount = x.Sum(z => z.GrossAmount + (z.BillDiscountAmount.HasValue ? z.BillDiscountAmount.Value : 0) + (z.WalletAmount.HasValue ? z.WalletAmount.Value : 0))
        //                    //}).ToList();


        //                    using (var context = new AuthContext())
        //                    {

        //                        context.Database.CommandTimeout = 600;
        //                        var query = "Select WarehouseId,OrderId,(grossamount + isnull(BillDiscountAmount,0)+ isnull(WalletAmount,0)) DispatchAmount from OrderMasters with (nolock) where status not In ('Payment Pending','Inactive','Failed','Dummy Order Cancelled') and CreatedDate >= '" + startDate.ToString("yyyy-MM-dd HH:mm:ss") + "' and CreatedDate <= '" + endDate.ToString("yyyy-MM-dd HH:mm:ss") + "' ";

        //                        List<OrderSummaryStatusWise> OrderSummaryStatusWise = context.Database.SqlQuery<OrderSummaryStatusWise>(query).ToList();
        //                        salesorderStatusAmount = OrderSummaryStatusWise != null && OrderSummaryStatusWise.Any() ? OrderSummaryStatusWise.GroupBy(x => x.WarehouseId).Select(x => new OrderStatusAmount { WarehouseId = x.Key, TotalAmount = x.Sum(y => y.DispatchAmount.Value) }).ToList() : new List<OrderStatusAmount>();


        //                        //var orderIdDt = new DataTable();
        //                        //orderIdDt.Columns.Add("IntValue");
        //                        //foreach (var item in OrderSummaryStatusWise)
        //                        //{
        //                        //    var dr = orderIdDt.NewRow();
        //                        //    dr["IntValue"] = item.OrderId;
        //                        //    orderIdDt.Rows.Add(dr);
        //                        //}
        //                        //var param = new SqlParameter("orderIds", orderIdDt);
        //                        //param.SqlDbType = SqlDbType.Structured;
        //                        //param.TypeName = "dbo.IntValues";
        //                        //WarehouseOnlinAmount = context.Database.SqlQuery<WarehouseCashAmount>("exec GetOnlineOrderPayment @orderIds", param).ToList();

        //                        //var param2 = new SqlParameter("orderIds", orderIdDt);
        //                        //param2.SqlDbType = SqlDbType.Structured;
        //                        //param2.TypeName = "dbo.IntValues";
        //                        onlinePaymentV2WithpaymentFrom = context.Database.SqlQuery<OnlinePaymentV2WithpaymentFrom>("exec OnlinePaymentV2WithpaymentFrom").ToList();

        //                        onlinePaymentV2WithpaymentFrom = onlinePaymentV2WithpaymentFrom.GroupBy(x => new { x.paymentFrom, x.WarehouseId }).Select(y => new OnlinePaymentV2WithpaymentFrom
        //                        {
        //                            paymentFrom = y.Key.paymentFrom,
        //                            Amount = y.Sum(z => z.Amount),
        //                            WarehouseId = y.Key.WarehouseId
        //                        }).ToList();


        //                    }

        //                });

        //                taskList.Add(task5);

        //                var task6 = Task.Factory.StartNew(() =>
        //                {
        //                    using (var context = new AuthContext())
        //                    {
        //                        context.Database.CommandTimeout = 600;
        //                        //var query = "select Cast(Sum(IsNull(LE.Credit, 0))as decimal(18,2))-Cast(Sum(IsNull(LE.Debit, 0))as decimal(18,2)) as Amount ,isnull(ir.WarehouseId,0) WarehouseId    from LadgerEntries LE INNER JOIN Ladgers L On L.ID = LE.LagerID   and l.ObjectType ='Supplier'  left join IRMasters ir on ir.id=le.ObjectID group by ir.WarehouseId";
        //                        var query = "Select Sum(Amount) as Amount, WarehouseId FROM ( select  Cast(Sum(IsNull(LE.Debit, 0))as decimal(18,2))-Cast(Sum(IsNull(LE.Credit, 0))as decimal(18,2)) as Amount ," +
        //                                      " case when isnull(ir.WarehouseId,0) <> 0 THEN ir.WarehouseId when isnull(POM.WarehouseId,0) <> 0 THEN POM.WarehouseId ELSE 78 END as WarehouseId" +
        //                                      " from LadgerEntries LE  with(nolock)	INNER JOIN Ladgers L  with(nolock)	 On L.ID = LE.LagerID   and l.ObjectType ='Supplier'  left join IRMasters ir  with(nolock) on ir.id=le.ObjectID and le.ObjectType = 'IR'" +
        //                                      " LEFT JOIN PurchaseOrderMasters POM  with(nolock) ON LE.ObjectID = POM.PurchaseOrderId and le.ObjectType = 'PR' group by ir.WarehouseId, POM.WarehouseId ) X group by WarehouseId";

        //                        WarehouseSupplierAmount = context.Database.SqlQuery<WarehouseCashAmount>(query).ToList();
        //                    }
        //                });

        //                taskList.Add(task6);

        //                var task7 = Task.Factory.StartNew(() =>
        //                {
        //                    using (var context = new AuthContext())
        //                    {
        //                        context.Database.CommandTimeout = 600;
        //                        var query = "select  Cast(Sum(IsNull(LE.Debit, 0))as decimal(18,2)) - Cast(Sum(IsNull(LE.Credit, 0))as decimal(18,2))  as Amount,a.WarehouseId    from LadgerEntries LE INNER JOIN Ladgers L On L.ID = LE.LagerID inner join DeliveryIssuances a on a.DeliveryIssuanceId=le.ObjectID where L.ObjectType ='Agent' group by a.WarehouseId";
        //                        WarehouseAgentAmount = context.Database.SqlQuery<WarehouseCashAmount>(query).ToList();
        //                    }
        //                });

        //                taskList.Add(task7);


        //                var task8 = Task.Factory.StartNew(() =>
        //                {
        //                    using (var context = new AuthContext())
        //                    {
        //                        context.Database.CommandTimeout = 600;
        //                        var query = " select a.WarehouseId, cast(sum(DamageInventory * isnull(item.purchaseprice,0))as decimal(18,2)) Amount from DamageStocks a with(nolock) outer apply  (select min(PurchasePrice) purchaseprice from ItemMasters b with(nolock) where a.Deleted=0 and a.ItemNumber=b.Number and a.WarehouseId=b.WarehouseId) item  group by a.WarehouseId";
        //                        DamageInventory = context.Database.SqlQuery<WarehouseCashAmount>(query).ToList();
        //                    }
        //                });

        //                taskList.Add(task8);

        //                var task9 = Task.Factory.StartNew(() =>
        //                {
        //                    using (var context = new AuthContext())
        //                    {
        //                        context.Database.CommandTimeout = 600;
        //                        var query = " select b.WarehouseId, cast(sum(a.Price * a.qty) as decimal(18,2)) Amount  from GoodsReceivedDetails a inner join PurchaseOrderDetails b on a.PurchaseOrderDetailId=b.PurchaseOrderDetailId where a.Status=1 group by b.WarehouseId ";
        //                        PendingGRN = context.Database.SqlQuery<WarehouseCashAmount>(query).ToList();
        //                    }
        //                });

        //                taskList.Add(task9);

        //                var task10 = Task.Factory.StartNew(() =>
        //                {
        //                    using (var context = new AuthContext())
        //                    {
        //                        context.Database.CommandTimeout = 600;
        //                        var query = "Exec GetWHGRIRDifference ";
        //                        GoodsReceivedNotInvoiced = context.Database.SqlQuery<WarehouseCashAmount>(query).ToList();
        //                    }
        //                });

        //                taskList.Add(task10);

        //                var task11 = Task.Factory.StartNew(() =>
        //                {
        //                    using (var context = new AuthContext())
        //                    {
        //                        context.Database.CommandTimeout = 600;

        //                        var query = " SELECT	Cast(SUM(AMOUNT) - Cast(ISNULL(sum(gr.gramount),0)as decimal(18,2)) as decimal(18,2)) as Amount  , WarehouseId FROM ( SELECT	SUM(AMOUNT) AMOUNT," +
        //                                    " PurchaseOrderId, WarehouseId FROM ( select  Sum(IsNull(LE.Debit, 0)) Amount, PRP.PurchaseOrderId, PRP.WarehouseId  " +
        //                                    " from LadgerEntries LE with(nolock)	INNER JOIN Ladgers L with(nolock) On L.ID = LE.LagerID   and l.ObjectType ='Supplier'" +
        //                                    " INNER JOIN IRMasters IRM with(nolock) ON LE.ObjectID = IRM.Id AND LE.ObjectType = 'IR' Inner JOIN PurchaseRequestPayments PRP  with(nolock) " +
        //                                    " ON IRM.PurchaseOrderId = PRP.PurchaseOrderId group by PRP.PurchaseOrderId , PRP.WarehouseId UNION select  Sum(IsNull(LE.Debit, 0)) Amount," +
        //                                    " PRP.PurchaseOrderId ,PRP.WarehouseId from LadgerEntries LE with(nolock)	INNER JOIN Ladgers L with(nolock) On L.ID = LE.LagerID   and l.ObjectType ='Supplier'" +
        //                                    " Inner JOIN PurchaseRequestPayments PRP  with(nolock) ON LE.ObjectID = PRP.Id and le.ObjectType = 'PR' group by PRP.PurchaseOrderId , PRP.WarehouseId" +
        //                                    " ) Y group by PurchaseOrderId , WarehouseId )X outer apply ( Select Sum(a.Qty*a.Price) gramount from PurchaseOrderDetails b  with(nolock)" +
        //                                    " Inner join GoodsReceivedDetails a  with(nolock) on a.PurchaseOrderDetailId=b.PurchaseOrderDetailId and  b.PurchaseOrderId=X.PurchaseOrderId " +
        //                                    " and a.IsActive=1 and a.IsDeleted=0 and a.Status=2 ) gr  GROUP BY	WarehouseId";
        //                        WarehouseSupplierAdvanceAmount = context.Database.SqlQuery<WarehouseCashAmount>(query).ToList();
        //                    }
        //                });

        //                taskList.Add(task11);

        //                var task12 = Task.Factory.StartNew(() =>
        //                {
        //                    using (var context = new AuthContext())
        //                    {
        //                        context.Database.CommandTimeout = 600;

        //                        var query = " Select im.WarehouseId, Cast(isnull( Sum((((ira.IRQuantity * ira.Price)-  " +
        //                                      " (case when ira.DiscountPercent is null or ira.DiscountPercent = 0 then ira.DiscountAmount else (ira.IRQuantity * ira.Price) * ira.DiscountPercent /100 end)) " +
        //                                      "  * ((ira.TotalTaxPercentage + isnull(ira.CessTaxPercentage,0)) / 100)) " +
        //                                      " + ((ira.IRQuantity * ira.Price)- (case when ira.DiscountPercent is null or ira.DiscountPercent = 0 then ira.DiscountAmount else (ira.IRQuantity * ira.Price) * ira.DiscountPercent /100 end))),0)  " +
        //                                      "  as decimal(18,2)) as Amount	 from InvoiceReceiptDetails ira  with(nolock) inner join IRMasters im with(nolock)  on im.Id = ira.IRMasterId  and im.IRStatus='Pending from Buyer side'  where  ira.IsActive=1 and (ira.IsDeleted=0 or ira.IsDeleted is null)  and ira.IRQuantity>0  group by im.WarehouseId ";

        //                        WarehouseIRPendingBuyerAmount = context.Database.SqlQuery<WarehouseCashAmount>(query).ToList();
        //                    }
        //                });

        //                taskList.Add(task12);

        //                Task.WaitAll(taskList.ToArray());

        //                List<int> warehouseIds = orderStatusAmount.Select(x => x.WarehouseId).Distinct().ToList();

        //                if (hubInventory != null && hubInventory.Any())
        //                    warehouseIds.AddRange(hubInventory.Select(x => x.WarehouseId).ToList());

        //                warehouseIds = warehouseIds.Select(x => x).Distinct().ToList();

        //                var warehouses = new List<WarehouseMinDc>();

        //                using (var context = new AuthContext())
        //                {
        //                    warehouses = context.Warehouses.Where(x => warehouseIds.Contains(x.WarehouseId)).Select(x => new WarehouseMinDc
        //                    {
        //                        WarehouseId = x.WarehouseId,
        //                        WarehouseName = x.WarehouseName
        //                    }).ToList();
        //                }

        //                List<WorkingCapitalData> WorkingCapitalData = new List<WorkingCapitalData>();


        //                foreach (var item in warehouses)
        //                {
        //                    var data = new WorkingCapitalData();

        //                    data.WarehouseName = item.WarehouseName;
        //                    data.WarehouseId = item.WarehouseId;
        //                    data.DeliveryCanceledAmount = orderStatusAmount.Any(x => x.Status == "Delivery Canceled" && x.WarehouseId == item.WarehouseId) ? orderStatusAmount.FirstOrDefault(x => x.Status == "Delivery Canceled" && x.WarehouseId == item.WarehouseId).TotalAmount : 0;
        //                    data.DeliveryRedispatchAmount = orderStatusAmount.Any(x => x.Status == "Delivery Redispatch" && x.WarehouseId == item.WarehouseId) ? orderStatusAmount.FirstOrDefault(x => x.Status == "Delivery Redispatch" && x.WarehouseId == item.WarehouseId).TotalAmount : 0;
        //                    data.IssuedAmount = orderStatusAmount.Any(x => x.Status == "Issued" && x.WarehouseId == item.WarehouseId) ? orderStatusAmount.FirstOrDefault(x => x.Status == "Issued" && x.WarehouseId == item.WarehouseId).TotalAmount : 0;
        //                    data.ReadyToDispatchAmount = orderStatusAmount.Any(x => x.Status == "Ready to Dispatch" && x.WarehouseId == item.WarehouseId) ? orderStatusAmount.FirstOrDefault(x => x.Status == "Ready to Dispatch" && x.WarehouseId == item.WarehouseId).TotalAmount : 0;
        //                    data.ShippedAmount = orderStatusAmount.Any(x => x.Status == "Shipped" && x.WarehouseId == item.WarehouseId) ? orderStatusAmount.FirstOrDefault(x => x.Status == "Shipped" && x.WarehouseId == item.WarehouseId).TotalAmount : 0;
        //                    data.InventoryAmount = hubInventory.Any(x => x.WarehouseId == item.WarehouseId) ? hubInventory.FirstOrDefault(x => x.WarehouseId == item.WarehouseId).Inventory : 0;
        //                    data.DeliveredButNotReconciled = orderNotReconsileAmount.Any(x => x.WarehouseId == item.WarehouseId) ? orderNotReconsileAmount.FirstOrDefault(x => x.WarehouseId == item.WarehouseId).TotalAmount : 0;
        //                    data.CashInOperation = Convert.ToDouble(CashInOperation.Any(x => x.WarehouseId == item.WarehouseId) ? CashInOperation.First(x => x.WarehouseId == item.WarehouseId).Amount : 0);
        //                    data.SupplierCredit = WarehouseSupplierAmount.Any(x => x.WarehouseId == item.WarehouseId) ? Convert.ToDouble(WarehouseSupplierAmount.FirstOrDefault(x => x.WarehouseId == item.WarehouseId).Amount) : 0;
        //                    data.AgentDues = WarehouseAgentAmount.Any(x => x.WarehouseId == item.WarehouseId) ? Convert.ToDouble(WarehouseAgentAmount.FirstOrDefault(x => x.WarehouseId == item.WarehouseId).Amount) : 0;
        //                    data.AvgSale = salesorderStatusAmount.Any(x => x.WarehouseId == item.WarehouseId) ? salesorderStatusAmount.FirstOrDefault(x => x.WarehouseId == item.WarehouseId).TotalAmount : 0;
        //                    data.ChequeInOperation = WarehouseChequeAmount.Any(x => x.WarehouseId == item.WarehouseId && x.ChequeStatus == 1) ? WarehouseChequeAmount.FirstOrDefault(x => x.WarehouseId == item.WarehouseId && x.ChequeStatus == 1).Amount : 0;
        //                    data.ChequeInBank = WarehouseChequeAmount.Any(x => x.WarehouseId == item.WarehouseId && x.ChequeStatus == 2) ? WarehouseChequeAmount.FirstOrDefault(x => x.WarehouseId == item.WarehouseId && x.ChequeStatus == 2).Amount : 0;
        //                    data.ChequeBounce = WarehouseChequeAmount.Any(x => x.WarehouseId == item.WarehouseId && x.ChequeStatus == 4) ? WarehouseChequeAmount.FirstOrDefault(x => x.WarehouseId == item.WarehouseId && x.ChequeStatus == 4).Amount : 0;
        //                    data.CreateDate = DateTime.Now.AddDays(-1).Date;
        //                    data.OnlinePrePaidAmount = onlinePaymentV2WithpaymentFrom.Any(x => x.WarehouseId == item.WarehouseId) ? Convert.ToDouble(onlinePaymentV2WithpaymentFrom.Where(x => x.WarehouseId == item.WarehouseId).Sum(x => x.Amount)) : 0;
        //                    data.OnlinePrePaidAmountePaylater = onlinePaymentV2WithpaymentFrom.Any(x => x.paymentFrom == "ePaylater" && x.WarehouseId == item.WarehouseId) ? Convert.ToDouble(onlinePaymentV2WithpaymentFrom.FirstOrDefault(x => x.paymentFrom == "ePaylater" && x.WarehouseId == item.WarehouseId).Amount) : 0;
        //                    data.OnlinePrePaidAmounthdfc = onlinePaymentV2WithpaymentFrom.Any(x => x.paymentFrom == "hdfc" && x.WarehouseId == item.WarehouseId) ? Convert.ToDouble(onlinePaymentV2WithpaymentFrom.FirstOrDefault(x => x.paymentFrom == "hdfc" && x.WarehouseId == item.WarehouseId).Amount) : 0;
        //                    data.OnlinePrePaidAmountmPos = onlinePaymentV2WithpaymentFrom.Any(x => x.paymentFrom == "mPos" && x.WarehouseId == item.WarehouseId) ? Convert.ToDouble(onlinePaymentV2WithpaymentFrom.FirstOrDefault(x => x.paymentFrom == "mPos" && x.WarehouseId == item.WarehouseId).Amount) : 0;
        //                    data.PendingGRNAmount = PendingGRN.Any(x => x.WarehouseId == item.WarehouseId) ? Convert.ToDouble(PendingGRN.FirstOrDefault(x => x.WarehouseId == item.WarehouseId).Amount) : 0;
        //                    data.DamageStockAmount = DamageInventory.Any(x => x.WarehouseId == item.WarehouseId) ? Convert.ToDouble(DamageInventory.FirstOrDefault(x => x.WarehouseId == item.WarehouseId).Amount) : 0;
        //                    data.GoodsReceivedNotInvoiced = GoodsReceivedNotInvoiced.Any(x => x.WarehouseId == item.WarehouseId) ? Convert.ToDouble(GoodsReceivedNotInvoiced.FirstOrDefault(x => x.WarehouseId == item.WarehouseId).Amount) : 0;
        //                    data.SupplierAdvances = WarehouseSupplierAdvanceAmount.Any(x => x.WarehouseId == item.WarehouseId) ? Convert.ToDouble(WarehouseSupplierAdvanceAmount.FirstOrDefault(x => x.WarehouseId == item.WarehouseId).Amount) : 0;
        //                    data.IRPendingBuyerSide = WarehouseIRPendingBuyerAmount.Any(x => x.WarehouseId == item.WarehouseId) ? Convert.ToDouble(WarehouseIRPendingBuyerAmount.FirstOrDefault(x => x.WarehouseId == item.WarehouseId).Amount) : 0;
        //                    WorkingCapitalData.Add(data);
        //                }

        //                if (WorkingCapitalData != null && WorkingCapitalData.Any())
        //                {
        //                    result = mongoDbHelperWorkingCapital.InsertMany(WorkingCapitalData);
        //                }
        //            }
        //            return result;
        //        }
        #endregion
        [Route("GetWarehouseBasedBalance")]
        [HttpGet]
        public LiveWarehouseCashDc GetWarehouseBasedBalance(int? warehouseid, DateTime Filterdate)
        {
            LiveWarehouseCashDc liveWarehouseCashDc = new LiveWarehouseCashDc();
            try
            {
                using (AuthContext context = new AuthContext())
                {
                    List<CurrencyHubStock> TodayHubCurrencyCollection = new List<CurrencyHubStock>();
                    if (warehouseid.HasValue)
                        TodayHubCurrencyCollection = context.CurrencyHubStock.Where(x => x.Warehouseid == warehouseid.Value && EntityFunctions.TruncateTime(x.BOD) == EntityFunctions.TruncateTime(Filterdate)).ToList();
                    else
                        TodayHubCurrencyCollection = context.CurrencyHubStock.Where(x => EntityFunctions.TruncateTime(x.BOD) == EntityFunctions.TruncateTime(Filterdate)).ToList();

                    List<long> yesterdayhubcollectionids = null;
                    List<hubCashCollectionDc> WarehouseOpeningCash = new List<hubCashCollectionDc>();
                    if (TodayHubCurrencyCollection != null && TodayHubCurrencyCollection.Any())
                    {
                        yesterdayhubcollectionids = TodayHubCurrencyCollection.Select(x => x.Id).ToList();
                        WarehouseOpeningCash = (
                           from c in context.CurrencyDenomination.Where(x => x.IsActive)
                           join p in context.HubCashCollection.Where(o => yesterdayhubcollectionids.Contains(o.CurrencyHubStockId) && o.IsActive
                           && ((o.IsDeleted.HasValue && !o.IsDeleted.Value) || !o.IsDeleted.HasValue))
                           on c.Id equals p.CurrencyDenominationId into ps
                           from p in ps.DefaultIfEmpty()
                               //group ps by new { c.Id, c.Title, c.Value, c.currencyType }
                               //into newGroup
                           select new hubCashCollectionDc
                           {
                               CurrencyDenominationId = c.Id,
                               CurrencyCount = p == null ? 0 : p.OpeningCurrencyCount,
                               CurrencyDenominationTitle = c.Title,
                               CurrencyDenominationValue = c.Value,
                               CashCurrencyType = c.currencyType,
                           }).ToList();

                        WarehouseOpeningCash = WarehouseOpeningCash.GroupBy(x => new { x.CurrencyDenominationId, x.CurrencyDenominationTitle, x.CurrencyDenominationValue, x.CashCurrencyType }).
                                             Select(x => new hubCashCollectionDc
                                             {
                                                 CurrencyDenominationId = x.Key.CurrencyDenominationId,
                                                 CurrencyCount = x.Sum(y => y.CurrencyCount),
                                                 CurrencyDenominationTitle = x.Key.CurrencyDenominationTitle,
                                                 CurrencyDenominationValue = x.Key.CurrencyDenominationValue,
                                                 CashCurrencyType = x.Key.CashCurrencyType,
                                             }).ToList();
                    }
                    else
                    {
                        WarehouseOpeningCash = (from c in context.CurrencyDenomination.Where(x => x.IsActive)
                                                select new hubCashCollectionDc
                                                {
                                                    CurrencyDenominationId = c == null ? 0 : c.Id,
                                                    CurrencyCount = 0,
                                                    CurrencyDenominationTitle = c == null ? "" : c.Title,
                                                    CurrencyDenominationValue = c == null ? 0 : c.Value,
                                                    CashCurrencyType = c == null ? "" : c.currencyType,
                                                }).ToList();
                    }
                    liveWarehouseCashDc.WarehouseOpeningCash = new List<hubCashCollectionDc>();
                    liveWarehouseCashDc.WarehouseOpeningCash = WarehouseOpeningCash;
                    int TotalOpeingamount = 0;
                    foreach (var data in WarehouseOpeningCash)
                    {
                        TotalOpeingamount += (data.CurrencyDenominationTotal);
                    }
                    liveWarehouseCashDc.TotalOpeingamount = TotalOpeingamount;
                    List<hubCashCollectionDc> WarehouseTodayCash = new List<hubCashCollectionDc>();
                    if (TodayHubCurrencyCollection != null && TodayHubCurrencyCollection.Any())
                    {
                        List<long> todaydayhubcollectionids = TodayHubCurrencyCollection.Select(x => x.Id).ToList();
                        WarehouseTodayCash = (
                           from c in context.CurrencyDenomination.Where(x => x.IsActive)
                           join p in context.HubCashCollection.Where(o => todaydayhubcollectionids.Contains(o.CurrencyHubStockId) && o.IsActive
                           && ((o.IsDeleted.HasValue && !o.IsDeleted.Value) || !o.IsDeleted.HasValue))
                           on c.Id equals p.CurrencyDenominationId into ps
                           from p in ps.DefaultIfEmpty()

                           select new hubCashCollectionDc
                           {
                               CurrencyDenominationId = c.Id,
                               CurrencyCount = p == null ? 0 : p.CurrencyCount,
                               CurrencyDenominationTitle = c.Title,
                               CurrencyDenominationValue = c.Value,
                               CashCurrencyType = c.currencyType,
                               BankDepositCurrencyCount = p == null ? 0 : p.BankSendCurrencyCount,
                               ExchangeInCurrencyCount = p == null ? 0 : (p.ExchangeCurrencyCount > 0 ? p.ExchangeCurrencyCount : 0),
                               ExchangeOutCurrencyCount = p == null ? 0 : (p.ExchangeCurrencyCount < 0 ? p.ExchangeCurrencyCount : 0),
                           }).ToList();

                        WarehouseTodayCash = WarehouseTodayCash.GroupBy(x => new { x.CurrencyDenominationId, x.CurrencyDenominationTitle, x.CurrencyDenominationValue, x.CashCurrencyType }).
                                            Select(x => new hubCashCollectionDc
                                            {
                                                CurrencyDenominationId = x.Key.CurrencyDenominationId,
                                                CurrencyCount = x.Sum(y => y.CurrencyCount),
                                                CurrencyDenominationTitle = x.Key.CurrencyDenominationTitle,
                                                CurrencyDenominationValue = x.Key.CurrencyDenominationValue,
                                                CashCurrencyType = x.Key.CashCurrencyType,
                                                ExchangeInCurrencyCount = x.Sum(y => y.ExchangeInCurrencyCount),
                                                ExchangeOutCurrencyCount = x.Sum(y => y.ExchangeOutCurrencyCount),
                                                BankDepositCurrencyCount = x.Sum(y => y.BankDepositCurrencyCount),
                                            }).ToList();

                    }
                    else
                    {
                        WarehouseTodayCash = (from c in context.CurrencyDenomination.Where(x => x.IsActive)
                                              select new hubCashCollectionDc
                                              {
                                                  CurrencyDenominationId = c == null ? 0 : c.Id,
                                                  CurrencyCount = 0,
                                                  CurrencyDenominationTitle = c == null ? "" : c.Title,
                                                  CurrencyDenominationValue = c == null ? 0 : c.Value,
                                                  CashCurrencyType = c == null ? "" : c.currencyType,
                                              }).ToList();
                        liveWarehouseCashDc.IsBOD = false;
                        liveWarehouseCashDc.IsEOD = false;
                    }
                    liveWarehouseCashDc.WarehouseTodayCash = new List<hubCashCollectionDc>();
                    liveWarehouseCashDc.WarehouseTodayCash = WarehouseTodayCash;

                    List<hubCashCollectionDc> WarehouseTodayClosingCash = new List<hubCashCollectionDc>();

                    WarehouseTodayClosingCash = WarehouseTodayCash.Union(WarehouseOpeningCash).GroupBy(x => new { x.CurrencyDenominationId, x.CurrencyDenominationTitle, x.CurrencyDenominationValue, x.CashCurrencyType })
                                                .Select(x => new hubCashCollectionDc
                                                {
                                                    CurrencyDenominationId = x.Key.CurrencyDenominationId,
                                                    CurrencyCount = x.Sum(y => y.CurrencyCount) - x.Sum(p => p.BankDepositCurrencyCount) + x.Sum(p => p.ExchangeInCurrencyCount) + x.Sum(p => p.ExchangeOutCurrencyCount),
                                                    CurrencyDenominationTitle = x.Key.CurrencyDenominationTitle,
                                                    CurrencyDenominationValue = x.Key.CurrencyDenominationValue,
                                                    CashCurrencyType = x.Key.CashCurrencyType,
                                                }).ToList();

                    liveWarehouseCashDc.WarehouseClosingCash = WarehouseTodayClosingCash;

                    if (liveWarehouseCashDc.WarehouseTodayCash != null && liveWarehouseCashDc.WarehouseTodayCash.Count() > 0 && yesterdayhubcollectionids != null && yesterdayhubcollectionids.Any())
                    {
                        var ExchangeHubCashCollections = context.ExchangeHubCashCollection.Where(o => yesterdayhubcollectionids.Contains(o.CurrencyHubStockId) && o.IsActive
                            && ((o.IsDeleted.HasValue && !o.IsDeleted.Value) || !o.IsDeleted.HasValue));
                        foreach (var item in liveWarehouseCashDc.WarehouseTodayCash)
                        {
                            if (ExchangeHubCashCollections != null && ExchangeHubCashCollections.Any(x => x.CurrencyDenominationId == item.CurrencyDenominationId))
                            {
                                item.ExchangeInCurrencyCount = ExchangeHubCashCollections.Any(x => x.CurrencyDenominationId == item.CurrencyDenominationId && x.CurrencyCount > 0) ? ExchangeHubCashCollections.Where(x => x.CurrencyDenominationId == item.CurrencyDenominationId && x.CurrencyCount > 0).Sum(x => x.CurrencyCount) : 0;
                                item.ExchangeOutCurrencyCount = ExchangeHubCashCollections.Any(x => x.CurrencyDenominationId == item.CurrencyDenominationId && x.CurrencyCount < 0) ? ExchangeHubCashCollections.Where(x => x.CurrencyDenominationId == item.CurrencyDenominationId && x.CurrencyCount < 0).Sum(x => x.CurrencyCount) : 0;
                            }
                        }
                    }

                    //int TotalClosingamount = 0;

                    //foreach(var data in  )
                }
            }
            catch (Exception ex)
            {

                return null;
            }

            return liveWarehouseCashDc;
        }

        //[AllowAnonymous]
        //[Route("GetHubAgentPaymentHistoryforChequePaginator")]
        //[HttpPost]
        //public List<ChequeCollectionDc> GetHubAgentPaymentHistoryforChequePaginator(Pager pager)
        //{
        //    List<ChequeCollectionDc> ChequeCollectionDcs = new List<ChequeCollectionDc>();
        //    try
        //    {

        //        using (AuthContext context = new AuthContext())
        //        {
        //            var chequestatusid = Convert.ToInt32(ChequeStatusEnum.Operation);
        //            var predicate = PredicateBuilder.True<ChequeCollection>();

        //            predicate = predicate.And(x => x.IsActive && ((x.IsDeleted.HasValue && !x.IsDeleted.Value) || !x.IsDeleted.HasValue));
        //            if (pager.WarehouseID > 0)
        //                predicate = predicate.And(x => x.CurrencyCollection.Warehouseid == pager.WarehouseID || x.WarehouseId == pager.WarehouseID);

        //            if (pager.CurrencyHubStockId.HasValue)
        //            {
        //                predicate = predicate.Or(x => x.CurrencyHubStockId == pager.CurrencyHubStockId.Value || x.CurrencyHubStockId == null);
        //            }

        //            if (pager.ChequeStatus.HasValue)
        //            {
        //                predicate = predicate.And(x => x.ChequeStatus == pager.ChequeStatus.Value);
        //            }

        //            if (!string.IsNullOrEmpty(pager.searchfilter))
        //            {
        //                predicate = predicate.And(x => x.CurrencyCollection.Deliveryissueid.ToString().Contains(pager.searchfilter)
        //                                        || x.ChequeNumber.Contains(pager.searchfilter)
        //                                        || x.Orderid.ToString().Contains(pager.searchfilter)
        //                                        || x.ChequeBankName.ToString().Contains(pager.searchfilter)
        //                                        || x.ChequeAmt.ToString().Contains(pager.searchfilter)
        //                                         || x.CurrencyCollection.Warehouseid.ToString().Contains(pager.searchfilter)
        //                                        );
        //            }

        //            if (pager.StartDate.HasValue && pager.EndDate.HasValue)
        //            {
        //                predicate = predicate.And(x => x.ChequeDate >= pager.StartDate.Value && x.ChequeDate <= pager.EndDate.Value);
        //            }
        //            context.Database.Log = s => Debug.WriteLine(s);
        //            ChequeCollectionDcs = context.ChequeCollection.Where(predicate).OrderByDescending(x => x.CreatedDate).Skip(pager.SkipCount).Take(pager.RowCount).Select(x =>
        //              new ChequeCollectionDc
        //              {
        //                  ChequeAmt = x.ChequeAmt,
        //                  ChequeDate = x.ChequeDate,
        //                  ChequeNumber = x.ChequeNumber,
        //                  ChequeStatus = x.ChequeStatus,
        //                  CurrencyCollectionId = x.CurrencyCollectionId,
        //                  Id = x.Id,
        //                  IsChequeClear = x.IsChequeClear,
        //                  WarehousePeopleId = x.WarehousePeopleId,
        //                  BankSubmitDate = x.BankSubmitDate,
        //                  ChequeimagePath = x.ChequeimagePath,
        //                  OrderId = x.Orderid,
        //                  Deliveryissueid = x.CurrencyCollection != null ? x.CurrencyCollection.Deliveryissueid : (int?)null,
        //                  ChequeBankName = x.ChequeBankName,
        //                  CurrencySettlementid = x.CurrencySettlementSourceId,
        //                  CurrencyCollectionStatus = x.CurrencyCollection != null ? x.CurrencyCollection.Status : "",
        //                  BounceImage = x.BounceImage,
        //                  WarehouseId = x.WarehouseId,
        //                  AgentId = x.AgentId
        //              }).ToList();


        //            if (ChequeCollectionDcs != null && ChequeCollectionDcs.Any())
        //            {
        //                var warehousesids = ChequeCollectionDcs.Select(x => x.WarehouseId).ToList();
        //                var warehouseData = context.Warehouses.Where(x => warehousesids.Contains(x.WarehouseId)).Select(x => new { x.WarehouseId, x.WarehouseName });
        //                var peoplesids = ChequeCollectionDcs.Select(x => x.DBoyPeopleId).Distinct().ToList();
        //                var peopeldata = context.Peoples.Where(x => peoplesids.Contains(x.PeopleID)).Select(x => new { x.PeopleID, x.DisplayName });
        //                var orderIds = ChequeCollectionDcs.Select(x => x.OrderId).Distinct().ToList();
        //                var orderskcode = context.DbOrderMaster.Where(x => orderIds.Contains(x.OrderId)).Select(x => new { x.Skcode, x.OrderId });
        //                var Agentids = ChequeCollectionDcs.Select(x => x.AgentId).ToList();
        //                var Agentdata = context.Peoples.Where(x => Agentids.Contains(x.PeopleID)).Select(x => new { x.PeopleID, x.DisplayName });

        //                foreach (var item in ChequeCollectionDcs)
        //                {
        //                    if (item.Deliveryissueid == null && item.DBoyName == "")
        //                    {
        //                        //item.AgentName = Agentdata != null && Agentdata.Any(x => x.PeopleID == item.AgentId) ? Agentdata.FirstOrDefault(x => x.PeopleID == item.AgentId).DisplayName : "";
        //                        item.DBoyName = peopeldata != null && peopeldata.Any(x => x.PeopleID == item.DBoyPeopleId) ? peopeldata.FirstOrDefault(x => x.PeopleID == item.DBoyPeopleId).DisplayName : "";
        //                        item.WarehouseName = warehouseData != null && warehouseData.Any(x => x.WarehouseId == item.WarehouseId) ? warehouseData.FirstOrDefault(x => x.WarehouseId == item.WarehouseId).WarehouseName : "";
        //                        item.SKCode = orderskcode != null && orderskcode.Any(x => x.OrderId == item.OrderId) ? orderskcode.FirstOrDefault(x => x.OrderId == item.OrderId).Skcode : "";
        //                        if (item.CurrencySettlementid.HasValue)
        //                            item.DepositBankName = context.CurrencySettlementSource.FirstOrDefault(x => x.Id == item.CurrencySettlementid.Value)?.SettlementSource;
        //                    }
        //                }
        //            }

        //        }

        //        return ChequeCollectionDcs;
        //    }
        //    catch (Exception ex)
        //    {
        //        logger.Error("Error in GetChequeCollection Method: " + ex.Message);
        //        return null;
        //    }
        //}


        [AllowAnonymous]
        [Route("GetHubAgentPaymentHistoryforChequePaginator")]
        [HttpGet]
        public List<ChequeCollectionDc> GetHubAgentPaymentHistoryforChequePaginator()
        {
            List<ChequeCollection> ChequeCollection = new List<ChequeCollection>();

            using (AuthContext context = new AuthContext())
            {
                List<ChequeCollectionDc> ChequeCollectionDcs = new List<ChequeCollectionDc>();
                ChequeCollectionDcs = context.ChequeCollection.Where(x => x.AgentId != null && x.WarehouseId != 0).OrderByDescending(x => x.CreatedDate).Select(x =>
                      new ChequeCollectionDc
                      {
                          ChequeAmt = x.ChequeAmt,
                          ChequeDate = x.ChequeDate,
                          ChequeNumber = x.ChequeNumber,
                          ChequeStatus = x.ChequeStatus,
                          CurrencyCollectionId = x.CurrencyCollectionId,
                          Id = x.Id,
                          IsChequeClear = x.IsChequeClear,
                          WarehousePeopleId = x.WarehousePeopleId,
                          BankSubmitDate = x.BankSubmitDate,
                          ChequeimagePath = x.ChequeimagePath,
                          OrderId = x.Orderid,
                          Deliveryissueid = x.CurrencyCollection != null ? x.CurrencyCollection.Deliveryissueid : (int?)null,
                          ChequeBankName = x.ChequeBankName,
                          CurrencySettlementid = x.CurrencySettlementSourceId,
                          CurrencyCollectionStatus = x.CurrencyCollection != null ? x.CurrencyCollection.Status : "",
                          BounceImage = x.BounceImage,
                          WarehouseId = x.WarehouseId,
                          AgentId = x.AgentId
                      }).ToList();

                var orderIds = ChequeCollectionDcs.Select(x => x.OrderId).Distinct().ToList();
                var orderskcode = context.DbOrderMaster.Where(x => orderIds.Contains(x.OrderId)).Select(x => new { x.Skcode, x.OrderId });
                var Agentids = ChequeCollectionDcs.Select(x => x.AgentId).ToList();
                var Agentdata = context.Peoples.Where(x => Agentids.Contains(x.PeopleID)).Select(x => new { x.PeopleID, x.DisplayName });
                foreach (var item in ChequeCollectionDcs)
                {
                    item.AgentName = Agentdata != null && Agentdata.Any(x => x.PeopleID == item.AgentId) ? Agentdata.FirstOrDefault(x => x.PeopleID == item.AgentId).DisplayName : "";
                    item.SKCode = orderskcode != null && orderskcode.Any(x => x.OrderId == item.OrderId) ? orderskcode.FirstOrDefault(x => x.OrderId == item.OrderId).Skcode : "";
                }
                return ChequeCollectionDcs;
            }
        }


        [Route("ChequeFineAppoved")]
        [HttpPost]
        public ChequeFineAppoved chequeFineAppoved(ReturnChequeCollectionDc data)
        {
            using (AuthContext context = new AuthContext())
            {
                var identity = User.Identity as ClaimsIdentity;
                int compid = 0, userid = 0, Warehouse_id = 0;

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))
                    compid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                    userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "Warehouseid"))
                    Warehouse_id = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "Warehouseid").Value);

                ChequeFineAppoved chequefine = new ChequeFineAppoved();
                var chequedata = context.ReturnChequeCollection.Where(x => x.Id == data.Id).Select(x => new { x.Fine, x.ChequeCollectionId }).FirstOrDefault();
                var chequeFineExist = context.ChequeFineAppoved.FirstOrDefault(x => x.ChequeCollectionId == chequedata.ChequeCollectionId);
                if (chequeFineExist == null)
                {
                    chequefine.ChequeCollectionId = chequedata.ChequeCollectionId;
                    chequefine.ChangeAmount = data.Fine;
                    chequefine.FineAmount = chequedata.Fine;
                    chequefine.CreatedBy = userid;
                    chequefine.CreatedDate = DateTime.Now;
                    chequefine.IsActive = true;
                    chequefine.IsDeleted = false;
                    chequefine.Note = data.Note;
                    chequefine.Status = Convert.ToInt32(ChequeAppovedStatusEnum.Pending);
                    context.ChequeFineAppoved.Add(chequefine);
                }
                else
                {

                    chequeFineExist.ChangeAmount = data.Fine;
                    chequeFineExist.FineAmount = chequedata.Fine;
                    chequeFineExist.ModifiedBy = userid;
                    chequeFineExist.ModifiedDate = DateTime.Now;
                    chequeFineExist.IsActive = true;
                    chequeFineExist.IsDeleted = false;
                    chequeFineExist.Note = data.Note;
                    chequeFineExist.Status = Convert.ToInt32(ChequeAppovedStatusEnum.Pending);
                    context.Entry(chequeFineExist).State = EntityState.Modified;
                }

                ReturnChequeCollection Rcheue = context.ReturnChequeCollection.Where(x => x.ChequeCollectionId == chequedata.ChequeCollectionId && x.IsActive == true && x.IsDeleted == false).FirstOrDefault();
                Rcheue.Status = Convert.ToInt32(ChequeReturnStatusEnum.AppovalSent);
                context.Entry(Rcheue).State = EntityState.Modified;

                context.Commit();
                return chequefine;

            }

        }


        [Route("GetChequeFineAppoved")]
        [HttpGet]
        public List<ChequeFineDc> GetChequeFineAppoved()
        {
            using (var db = new AuthContext())
            {
                try
                {
                    var identity = User.Identity as ClaimsIdentity;
                    int compid = 0, userid = 0, Warehouse_id = 0;

                    if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))
                        compid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);

                    if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                        userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

                    if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "Warehouseid"))
                        Warehouse_id = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "Warehouseid").Value);



                    var newdata = (from a in db.ChequeFineAppoved
                                   where (a.IsDeleted == false && a.IsActive == true)
                                   join b in db.ChequeCollection on a.ChequeCollectionId equals b.Id
                                   where a.Status != 2
                                   select new ChequeFineDc
                                   {
                                       WarehouseId = b.WarehouseId ?? 0,
                                       ChequeCollectionId = a.ChequeCollectionId,
                                       OrderId = b.Orderid,
                                       ChequeAmt = b.ChequeAmt,
                                       ChequeNumber = b.ChequeNumber,
                                       ChequeDate = b.ChequeDate,
                                       FineAmount = a.FineAmount,
                                       ChangeAmount = a.ChangeAmount,
                                       CreatedDate = a.CreatedDate,
                                       Status = a.Status,

                                       //               }).OrderByDescending(x => x.ChequeDate).ToList();
                                   }).OrderByDescending(x => x.ChequeDate).ToList();
                    return newdata;

                }
                catch (Exception ex)
                {
                    return null;
                }

            }
        }

        [Route("ChequeApproved")]
        [HttpPost]
        public ChequeFineDc ChequeApproved(ChequeFineDc data)
        {

            var identity = User.Identity as ClaimsIdentity;
            int userid = 0;
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);


            using (var db = new AuthContext())
            {

                ChequeFineAppoved chequeFineAppoved = db.ChequeFineAppoved.Where(x => x.ChequeCollectionId == data.ChequeCollectionId).FirstOrDefault();
                chequeFineAppoved.AppovedBy = userid;
                chequeFineAppoved.ModifiedDate = indianTime;
                chequeFineAppoved.ModifiedBy = userid;
                chequeFineAppoved.Status = Convert.ToInt32(ChequeAppovedStatusEnum.Appoved);
                db.Entry(chequeFineAppoved).State = EntityState.Modified;

                ReturnChequeCollection retrunchequecollection = db.ReturnChequeCollection.Where(x => x.ChequeCollectionId == data.ChequeCollectionId && x.IsDeleted == false && x.IsActive == true).FirstOrDefault();
                retrunchequecollection.Fine = Convert.ToInt32(data.ChangeAmount);
                retrunchequecollection.Status = Convert.ToInt32(ChequeReturnStatusEnum.AppovedSuccesfully);
                retrunchequecollection.ModifiedBy = userid;
                retrunchequecollection.ModifiedDate = indianTime;
                db.Entry(retrunchequecollection).State = EntityState.Modified;

                db.Commit();

                return data;
            }

        }
        [Route("ChequeFineReject")]
        [HttpPost]
        public ChequeFineDc ChequeFineReject(ChequeFineDc data)
        {
            var identity = User.Identity as ClaimsIdentity;
            int userid = 0;
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

            using (var db = new AuthContext())
            {

                ChequeFineAppoved chequeFineAppoved = db.ChequeFineAppoved.Where(x => x.ChequeCollectionId == data.ChequeCollectionId).FirstOrDefault();
                chequeFineAppoved.RejectedBy = userid;
                chequeFineAppoved.ModifiedDate = indianTime;
                chequeFineAppoved.ModifiedBy = userid;
                chequeFineAppoved.Note = data.Note;
                chequeFineAppoved.Status = Convert.ToInt32(ChequeAppovedStatusEnum.Rejected);
                db.Entry(chequeFineAppoved).State = EntityState.Modified;

                ReturnChequeCollection retrunchequecollection = db.ReturnChequeCollection.Where(x => x.ChequeCollectionId == data.ChequeCollectionId).FirstOrDefault();
                retrunchequecollection.Status = Convert.ToInt32(ChequeReturnStatusEnum.ChequeFineReject);
                retrunchequecollection.ModifiedBy = userid;
                retrunchequecollection.ModifiedDate = indianTime;
                db.Entry(retrunchequecollection).State = EntityState.Modified;

                db.Commit();

                return data;
            }

        }

        [Route("GetChequeFineRejected")]
        [HttpGet]
        public List<ChequeFineDc> GetChequeFineRejected()
        {
            using (var db = new AuthContext())
            {
                try
                {
                    var identity = User.Identity as ClaimsIdentity;
                    int compid = 0, userid = 0, Warehouse_id = 0;

                    if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))
                        compid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);

                    if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                        userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

                    if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "Warehouseid"))
                        Warehouse_id = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "Warehouseid").Value);



                    var Rejectdata = (from a in db.ChequeFineAppoved
                                      where (a.IsDeleted == false && a.IsActive == true)
                                      join b in db.ChequeCollection on a.ChequeCollectionId equals b.Id
                                      where a.Status == 2
                                      select new ChequeFineDc
                                      {
                                          WarehouseId = b.WarehouseId ?? 0,
                                          ChequeCollectionId = a.ChequeCollectionId,
                                          OrderId = b.Orderid,
                                          ChequeAmt = b.ChequeAmt,
                                          ChequeNumber = b.ChequeNumber,
                                          ChequeDate = b.ChequeDate,
                                          FineAmount = a.FineAmount,
                                          ChangeAmount = a.ChangeAmount,
                                          CreatedDate = a.CreatedDate,
                                          Status = a.Status,

                                      }).OrderByDescending(x => x.ChequeCollectionId).ToList();

                    return Rejectdata;

                }
                catch (Exception ex)
                {
                    return null;
                }

            }
        }

        [Route("CashBalanceVerified")]
        [HttpPost]
        public CashBalanceVerifiedDc CashBalanceVerified(CashBalanceVerifiedDc Cashverify)
        {

            var identity = User.Identity as ClaimsIdentity;
            int compid = 0, userid = 0, Warehouse_id = 0;

            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))
                compid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);

            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "Warehouseid"))
                Warehouse_id = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "Warehouseid").Value);

            using (var db = new AuthContext())
            {
                var yesterdaydate = DateTime.Now.AddDays(-1);
                var Verifydata = db.CashBalanceVerify.Where(x => x.WarehouseId == Cashverify.WarehouseId && x.CreatedDate == Cashverify.CreatedDate && x.IsVerify == true).FirstOrDefault();
                if (Verifydata == null)
                {
                    CashBalanceVerify verifed = new CashBalanceVerify();
                    verifed.WarehouseId = Cashverify.WarehouseId;
                    verifed.VerifyBy = userid;
                    verifed.VerifyDate = yesterdaydate;
                    verifed.IsVerify = true;
                    verifed.IsActive = true;
                    verifed.IsDeleted = false;
                    verifed.CreatedDate = DateTime.Now;
                    verifed.CreatedBy = userid;
                    verifed.SubmittedRole = Cashverify.SubmittedRole;
                    db.CashBalanceVerify.Add(verifed);
                    db.Commit();

                }
                else
                {
                    return Cashverify;
                }
            }
            return Cashverify;

        }


        [Route("GetCashBalanceVerified")]
        [HttpGet]
        public CashBalanceVerifiedDc GetCashBalanceVerified(int WarehouseId, DateTime Fillterdate, string Role)
        {
            using (var db = new AuthContext())
            {

                CashBalanceVerifiedDc verify = new CashBalanceVerifiedDc();
                DateTime startDate = Fillterdate.Date, endDate = Fillterdate.Date.AddDays(1);
                var Verifydata = db.CashBalanceVerify.Where(x => x.WarehouseId == WarehouseId && x.IsVerify == true && (x.VerifyDate < endDate && x.VerifyDate >= startDate) && x.SubmittedRole == Role).FirstOrDefault();
                verify = Mapper.Map(Verifydata).ToANew<CashBalanceVerifiedDc>();
                if (Verifydata != null)
                {

                    var people = db.Peoples.Where(x => x.PeopleID == Verifydata.VerifyBy).FirstOrDefault();
                    verify.VerifyName = people.DisplayName;
                }

                return verify;
            }


        }

        [Route("GetChequeDataExport")]
        [HttpGet]
        public async Task<List<ChequeCollectionDc>> GetChequeDataExport()
        {
            using (var db = new AuthContext())
            {
                List<ChequeCollectionDc> Chequedata = new List<ChequeCollectionDc>();
                var chequeCollection = (from ch in db.ChequeCollection
                                        join c in db.CurrencyCollection
                                        on ch.CurrencyCollectionId equals c.Id
                                        join w in db.Warehouses on c.Warehouseid equals w.WarehouseId
                                        join o in db.DbOrderMaster on ch.Orderid equals o.OrderId
                                        where (ch.IsDeleted == false && ch.IsActive == true)
                                        select new
                                        {
                                            c.Deliveryissueid,
                                            ch.Orderid,
                                            ch.ChequeBankName,
                                            ch.ChequeAmt,
                                            ch.ChequeDate,
                                            ch.ChequeStatus,
                                            ch.CreatedDate,
                                            w.WarehouseName,
                                            ch.ChequeNumber,
                                            ch.ReturnComment,
                                            ch.BankSubmitDate,
                                            o.Skcode,

                                        }).ToList();
                Chequedata = Mapper.Map(chequeCollection).ToANew<List<ChequeCollectionDc>>();
                if (chequeCollection.Count > 0)
                {
                    return Chequedata;
                }
                else
                {
                    return Chequedata;
                }
            }

        }


        [Route("getChequecollectiondata")]
        [HttpGet]
        public async Task<List<ChequeCollectionSearchDc>> getChequecollectiondata(int WarehouseID, int PeopleID, int CurrencyHubStockId, int ChequeStatus, string searchfilter, int PageNumber, int PageSize)
        {
            if (searchfilter == null)
            {
                searchfilter = "";
            }

            using (var context = new AuthContext())
            {
                try
                {
                    var WareHouseIdParam = new SqlParameter
                    {
                        ParameterName = "WarehouseID",
                        Value = WarehouseID
                    };

                    var PeopleIDParam = new SqlParameter
                    {
                        ParameterName = "PeopleID",
                        Value = PeopleID
                    };
                    var CurrencyHubStockIdParam = new SqlParameter
                    {
                        ParameterName = "CurrencyHubStockId",
                        Value = CurrencyHubStockId
                    };
                    var ChequeStatusParam = new SqlParameter
                    {
                        ParameterName = "ChequeStatus",
                        Value = ChequeStatus
                    };
                    var searchfilterParam = new SqlParameter
                    {
                        ParameterName = "searchfilter",
                        Value = searchfilter
                    };
                    var PageNumberParam = new SqlParameter
                    {
                        ParameterName = "PageNumber",
                        Value = PageNumber
                    };
                    var PageSizeNumberParam = new SqlParameter
                    {
                        ParameterName = "PageSize",
                        Value = PageSize
                    };
                    List<ChequeCollectionSearchDc> chequeCollectionDc = context.Database.SqlQuery<ChequeCollectionSearchDc>("GetChequeCollection @WareHouseID ,@PeopleID,@CurrencyHubStockId,@ChequeStatus,@searchfilter,@PageNumber,@PageSize ", WareHouseIdParam,
                    PeopleIDParam, CurrencyHubStockIdParam, ChequeStatusParam, searchfilterParam, PageNumberParam, PageSizeNumberParam).ToList();
                    return chequeCollectionDc;

                }
                catch (Exception ex)
                {
                    return null;
                }
            }

        }

        [Route("getChequecollectiondataCount")]
        [HttpGet]
        public int getChequecollectiondataCount(int WarehouseID, int PeopleID, int CurrencyHubStockId, int ChequeStatus, string searchfilter)
        {
            if (searchfilter == null)
            {
                searchfilter = "";
            }
            int Count = 0;
            using (var context = new AuthContext())
            {
                try
                {
                    var WareHouseIdParam = new SqlParameter
                    {
                        ParameterName = "WarehouseID",
                        Value = WarehouseID
                    };

                    var PeopleIDParam = new SqlParameter
                    {
                        ParameterName = "PeopleID",
                        Value = PeopleID
                    };
                    var CurrencyHubStockIdParam = new SqlParameter
                    {
                        ParameterName = "CurrencyHubStockId",
                        Value = CurrencyHubStockId
                    };
                    var ChequeStatusParam = new SqlParameter
                    {
                        ParameterName = "ChequeStatus",
                        Value = ChequeStatus
                    };
                    var searchfilterParam = new SqlParameter
                    {
                        ParameterName = "searchfilter",
                        Value = searchfilter
                    };
                    Count = context.Database.SqlQuery<ChequeCollectionSearchDc>("GetChequeCollectionCount @WareHouseID ,@PeopleID,@CurrencyHubStockId,@ChequeStatus,@searchfilter ", WareHouseIdParam,
                    PeopleIDParam, CurrencyHubStockIdParam, ChequeStatusParam, searchfilterParam).Count();
                    return Count;

                }
                catch (Exception ex)
                {
                    return 0;
                }
            }


        }


        [Route("GetChequeDataExportDateWise")]
        [HttpGet]
        public async Task<List<ChequeCollectionDc>> GetChequeDataExportDateWise(DateTime StartDate, DateTime EndDate)
        {
            using (var db = new AuthContext())
            {
                List<ChequeCollectionDc> Chequedata = new List<ChequeCollectionDc>();
                var chequeCollection = (from ch in db.ChequeCollection
                                        join c in db.CurrencyCollection
                                        on ch.CurrencyCollectionId equals c.Id
                                        join w in db.Warehouses on c.Warehouseid equals w.WarehouseId
                                        join o in db.DbOrderMaster on ch.Orderid equals o.OrderId
                                        where (ch.IsDeleted == false && ch.IsActive == true && ch.CreatedDate >= StartDate && ch.CreatedDate <= EndDate)
                                        select new
                                        {
                                            c.Deliveryissueid,
                                            ch.Orderid,
                                            ch.ChequeBankName,
                                            ch.ChequeAmt,
                                            ch.ChequeDate,
                                            ch.ChequeStatus,
                                            ch.CreatedDate,
                                            w.WarehouseName,
                                            ch.ChequeNumber,
                                            ch.ReturnComment,
                                            ch.BankSubmitDate,
                                            o.Skcode
                                        }).ToList();


                Chequedata = Mapper.Map(chequeCollection).ToANew<List<ChequeCollectionDc>>();
                if (chequeCollection.Count > 0)
                {
                    return Chequedata;
                }
                else
                {
                    return Chequedata;
                }
            }

        }

        [Route("GetSkCashCollectionByUserName")]
        [HttpGet]
        [AllowAnonymous]
        public CashCollectionResponse GetSkCashCollectionByUserName(string PeopleUserName)
        {
            CashCollectionResponse cashCollectionResponse = new CashCollectionResponse();

            if (!(HttpContext.Current.Request.Headers.GetValues("AirtelUserName")?.FirstOrDefault() == ConfigurationManager.AppSettings["AirtelUserName"].ToString() && HttpContext.Current.Request.Headers.GetValues("AirtelPassword")?.FirstOrDefault() == ConfigurationManager.AppSettings["AirtelPassword"].ToString()))
            {
                cashCollectionResponse.Message = "Authorization has been denied for this request.";
                cashCollectionResponse.status = false;
                cashCollectionResponse.CashCollection = new List<currencysettelDc>();
                return cashCollectionResponse;
            }

            using (AuthContext context = new AuthContext())
            {

                var people = context.Peoples.Where(x => x.UserName == PeopleUserName).FirstOrDefault();
                if (people != null)
                {
                    var currencySettlementSources = context.CurrencySettlementSource.Where(c => c.Warehouseid == people.WarehouseId && c.SettlementSource == "Airtel Payment BANK" && c.IsDeposited == false).ToList();
                    if (currencySettlementSources != null && currencySettlementSources.Any())
                    {
                        var CashCollections = currencySettlementSources.Select(c => new currencysettelDc
                        {
                            PeopleName = people.DisplayName,
                            PeopleUserName = people.UserName,
                            MobileNo = people.Mobile,
                            CashAmount = (int)c.TotalCashAmt,
                            TransactionDate = c.SettlementDate,
                            TransactionId = c.Id,
                        }).FirstOrDefault();
                        cashCollectionResponse.CashCollection = new List<currencysettelDc> { CashCollections };
                        cashCollectionResponse.status = true;
                        cashCollectionResponse.Message = "";
                        foreach (var currencysettel in currencySettlementSources)
                        {
                            currencysettel.IsProgress = true;
                            currencysettel.ModifiedBy = people.PeopleID;
                            currencysettel.ModifiedDate = indianTime;
                            context.Entry(currencysettel).State = EntityState.Modified;
                        }
                        context.Commit();
                    }
                    else
                    {
                        cashCollectionResponse.status = false;
                        cashCollectionResponse.Message = "None of the pending cash required available for this user";
                        cashCollectionResponse.CashCollection = new List<currencysettelDc>();
                    }

                }
                else
                {
                    cashCollectionResponse.status = false;
                    cashCollectionResponse.Message = "Invalid User. Please Enter valid username";
                    cashCollectionResponse.CashCollection = new List<currencysettelDc>();
                }
                return cashCollectionResponse;


            }



        }

        [Route("CashPaymentConfirmation")]
        [HttpPost]
        [AllowAnonymous]
        public ReturnCashCollectionResponse CashPaymentConfirmation(CashCollectionRequest cashCollectionRequest)
        {
            ReturnCashCollectionResponse returnCashCollectionResponse = new ReturnCashCollectionResponse();
            if (!(HttpContext.Current.Request.Headers.GetValues("AirtelUserName")?.FirstOrDefault() == ConfigurationManager.AppSettings["AirtelUserName"].ToString() && HttpContext.Current.Request.Headers.GetValues("AirtelPassword")?.FirstOrDefault() == ConfigurationManager.AppSettings["AirtelPassword"].ToString()))
            {
                returnCashCollectionResponse.Message = "Authorization has been denied for this request.";
                returnCashCollectionResponse.status = false;
                returnCashCollectionResponse.TransactionId = cashCollectionRequest.TransactionId;
                return returnCashCollectionResponse;
            }
            if (cashCollectionRequest != null && cashCollectionRequest.TransactionId > 0)
            {
                using (AuthContext context = new AuthContext())
                {
                    returnCashCollectionResponse.TransactionId = cashCollectionRequest.TransactionId;
                    var currencysettelAdd = context.CurrencySettlementSource.Where(x => x.Id == cashCollectionRequest.TransactionId && x.IsProgress).FirstOrDefault();
                    if (currencysettelAdd != null)
                    {
                        if (!currencysettelAdd.IsDeposited)
                        {
                            currencysettelAdd.IsDeposited = true;
                            currencysettelAdd.ProcessTransId = cashCollectionRequest.ProcessTransId;
                            //currencysettelAdd.ModifiedBy = userid;
                            currencysettelAdd.ModifiedDate = indianTime;
                            context.Entry(currencysettelAdd).State = EntityState.Modified;

                            returnCashCollectionResponse.status = context.Commit() > 0;
                            returnCashCollectionResponse.Message = "";
                            if (!returnCashCollectionResponse.status)
                            {
                                returnCashCollectionResponse.Message = "Error during update request. please try after some time.";
                            }
                        }
                        else
                        {
                            returnCashCollectionResponse.status = false;
                            returnCashCollectionResponse.Message = "Request transaction id has already processed";
                        }
                    }
                    else
                    {
                        returnCashCollectionResponse.status = false;
                        returnCashCollectionResponse.Message = "Request transaction id doesn't exist.";
                    }
                }
            }
            else
            {
                returnCashCollectionResponse.status = false;
                returnCashCollectionResponse.Message = "Invalid request. Please try with valid request";
            }

            return returnCashCollectionResponse;


        }

        #region CaseMemoRequest
        [Route("CaseMemoReq")]
        [HttpGet]
        [AllowAnonymous]
        public List<OrderListByAssignIdDC> GetCaseMemoRequest(int AssignId)
        {
            List<OrderListByAssignIdDC> orderListByAssignIdDCs = new List<OrderListByAssignIdDC>();

            using (var context = new AuthContext())
            {
                try
                {
                    var AssignIdparm = new SqlParameter
                    {
                        ParameterName = "Assign",
                        Value = AssignId
                    };

                    orderListByAssignIdDCs = context.Database.SqlQuery<OrderListByAssignIdDC>("spGetOrdersByAssignmentId @Assign ", AssignIdparm).ToList();
                    return orderListByAssignIdDCs;
                }
                catch (Exception ex)
                {
                    return null;
                }
            }
        }

        [Route("PostCaseMemoAmountRequest")]
        [HttpPost]
        [AllowAnonymous]
        public ResponseResult PostCaseMemoAmountRequest(OrderListByAssignIdDC param)
        {
            ResponseResult responseResult = new ResponseResult { status = false, Message = "" };
            try
            {
                Random random = new Random();
                var identity = User.Identity as ClaimsIdentity;
                int userid = 0;
                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                {
                    userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
                }
                using (var context = new AuthContext())
                {
                    var list = context.caseMemoDetails.Where(i => i.OrderId == param.OrderId && i.IsActive == true).FirstOrDefault();
                    if (list == null)
                    {
                        CaseMemoDetail caseMemoDetail = new CaseMemoDetail
                        {
                            OrderId = param.OrderId,
                            Amount = param.TotalAmount,
                            PeopleId = param.DeliveryBoyid,
                            IsApproved = false,
                            IsActive = true,
                            IsUsedBy = null,
                            // CaseMemoNo = random.Next(),
                            RequestedBy = userid,
                            RequestedDate = DateTime.Now,
                            ApprovedDate = Convert.ToDateTime("1900-01-01")

                        };
                        context.caseMemoDetails.Add(caseMemoDetail);
                        if (context.Commit() > 0)
                        {
                            responseResult.Message = "Request sent successfully.";
                            responseResult.status = true;
                        }
                        else
                        {
                            responseResult.Message = "some error occurred during save data.";
                            responseResult.status = false;
                        }
                    }
                    else
                    {
                        responseResult.Message = "Request of this order id is already submitted please connect with IT.";
                        responseResult.status = false;
                    }

                }
            }
            catch (Exception ex)
            {
                logger.Error("Error on PostCaseMemoAmountRequest method:" + ex.ToString());
                responseResult.Message = "some error occurred during save data.";
                responseResult.status = false;
            }
            return responseResult;
        }

        [Route("GetCaseMemoDetails")]
        [HttpGet]
        [AllowAnonymous]
        public List<OrderListByAssignIdDC> GetCaseMemoDetails()
        {


            using (var context = new AuthContext())
            {
                try
                {
                    var orderListByAssignIdDCs = context.Database.SqlQuery<OrderListByAssignIdDC>("spGetCaseMemoReqDetails").ToList();

                    return orderListByAssignIdDCs;
                }
                catch (Exception ex)
                {
                    return null;
                }
            }
        }
        [Route("PostCaseMemoDetails")]
        [HttpPost]
        [AllowAnonymous]
        public ResponseResult PostCaseMemoDetailsUpdate(CaseMemoDetail param)
        {
            ResponseResult responseResult = new ResponseResult { status = false, Message = "" };
            try
            {
                Random random = new Random();
                var identity = User.Identity as ClaimsIdentity;
                int userid = 0;
                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                {
                    userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
                }
                using (var context = new AuthContext())
                {
                    CaseMemoDetail caseMemoDetail = context.caseMemoDetails.First(i => i.OrderId == param.OrderId && i.IsApproved == false);
                    if (caseMemoDetail != null)
                    {
                        caseMemoDetail.IsApproved = true;
                        caseMemoDetail.ApprovedDate = DateTime.Now;
                        caseMemoDetail.ApprovedBy = userid;
                        caseMemoDetail.CaseMemoNo = random.Next();
                        // context.SaveChanges();
                        if (context.Commit() > 0)
                        {
                            responseResult.Message = "Request Approved successfully. Case Memo No - " + caseMemoDetail.CaseMemoNo;
                            responseResult.status = true;
                        }
                        else
                        {
                            responseResult.Message = "some error occurred during save data.";
                            responseResult.status = false;
                        }
                    }
                    else
                    {
                        responseResult.Message = "some error occurred during save data.";
                        responseResult.status = false;
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error("Error on PostCaseMemoAmountRequest method:" + ex.ToString());
                responseResult.Message = "some error occurred during save data.";
                responseResult.status = false;
            }
            return responseResult;
        }
        #endregion
        [Route("UpdateYesterdayClosing")]
        [HttpGet]
        public bool UpdateYesterdayClosing(int warehouseid, DateTime Filterdate)
        {
            bool status = false;
            try
            {
                if (warehouseid > 0 && Filterdate != null)
                {
                    using (AuthContext context = new AuthContext())
                    {
                        var identity = User.Identity as ClaimsIdentity;
                        int userid = 0;

                        if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                            userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
                        var datetime = Filterdate.ToString("yyyy-MM-dd");
                        var time = DateTime.Now.ToString(" HH:mm:ss");
                        var finaldate = datetime + time;
                        var Warehouseid = new SqlParameter("@warehouseid", warehouseid);
                        var Date = new SqlParameter("@currentdate", finaldate);
                        var UserId = new SqlParameter("@userId", userid);
                        var results = context.Database.ExecuteSqlCommand("UpdateCashOpening @currentdate,@warehouseid,@userId", Date, Warehouseid, UserId);
                        status = true;
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error("Error in UpdateYesterdayClosing Method: " + ex.Message);
                return false;
            }

            return status;
        }
        [HttpPost]
        [Route("GetCMSReportdata")]
        [AllowAnonymous]
        public async Task<List<CMSReportDatadc>> GetCMSReportapi(CMSDetailDC wids)
        {
            List<CMSReportDatadc> list = new List<CMSReportDatadc>();
            using (var authContext = new AuthContext())
            {
                if (authContext.Database.Connection.State != ConnectionState.Open)
                    authContext.Database.Connection.Open();

                DataTable IdDt = new DataTable();
                IdDt = new DataTable();
                IdDt.Columns.Add("IntValue");
                foreach (var item in wids.Warehouseids)
                {
                    var dr = IdDt.NewRow();
                    dr["IntValue"] = item;
                    IdDt.Rows.Add(dr);
                }

                var param1 = new SqlParameter("warehouseIds", IdDt);
                var param2 = new SqlParameter("@fromdt", wids.fromdate);
                var param3 = new SqlParameter("@todt", wids.todate);

                var cmd = authContext.Database.Connection.CreateCommand();
                cmd.CommandText = "[dbo].[SpGetCMSReportData]";
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.Parameters.Add(param1);
                cmd.Parameters.Add(param2);
                cmd.Parameters.Add(param3);
                // Run the sproc
                var reader = cmd.ExecuteReader();
                list = ((IObjectContextAdapter)authContext)
                .ObjectContext
                .Translate<CMSReportDatadc>(reader).ToList();
                reader.NextResult();


                return list;
            }
        }

        #region old api
        //[Route("CashPaymentPayLater")]
        //[HttpPost]
        //public APIResponse CashPaymentPayLater(CashdataDc cashData)
        //{
        //    try
        //    {
        //        var identity = User.Identity as ClaimsIdentity;
        //        int userid = 0;
        //        if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
        //            userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
        //        using (AuthContext db = new AuthContext())
        //        {
        //            var payLaterCollection = db.PayLaterCollectionDb.FirstOrDefault(x => x.OrderId == cashData.OrderId
        //                                                                  && x.IsActive == true
        //                                                                  && x.IsDeleted == false);
        //            if (payLaterCollection != null)
        //            {
        //                double totalAmount = 0;
        //                var totalAmountlist = db.PayLaterCollectionHistoryDb.Where(x => x.PayLaterCollectionId == payLaterCollection.Id
        //                                                                  && x.IsActive == true
        //                                                                   && x.IsDeleted == false && (x.PaymentStatus == 1 || x.PaymentStatus == 0)).ToList();
        //                var CashAmount = cashData.cashList.Where(x => x.CurrencyCount > 0).Sum(x => (x.CurrencyCount * x.Value));
        //                if (totalAmountlist != null && totalAmountlist.Any())
        //                {
        //                    List<PayLaterAmountUpdateDC> list = new List<PayLaterAmountUpdateDC>();
        //                    foreach (var item in totalAmountlist)
        //                    {
        //                        PayLaterAmountUpdateDC singlepayment = new PayLaterAmountUpdateDC();
        //                        singlepayment.Amount = item.Amount;
        //                        singlepayment.Comment = item.Comment;
        //                        list.Add(singlepayment);
        //                    }
        //                    //double paidamount = 0;
        //                    //double returnamount = 0;
        //                    //double gullakamount = 0;
        //                    //paidamount = totalAmountlist.Any(a => a.Comment != "Return Order" && a.Comment != "Gullak Refund") ? totalAmountlist.Where(a => a.Comment != "Return Order" && a.Comment != "Gullak Refund").Sum(y => y.Amount) : 0;
        //                    //returnamount = totalAmountlist.Any(a => a.Comment == "Return Order") ? totalAmountlist.Where(a => a.Comment == "Return Order").Sum(y => y.Amount) : 0;
        //                    //gullakamount = totalAmountlist.Any(a => a.Comment == "Gullak Refund") ? totalAmountlist.Where(a => a.Comment == "Gullak Refund").Sum(y => y.Amount) : 0;
        //                    //totalAmount = totalAmountlist.Sum(x => x.Amount);
        //                    double getamount = 0;
        //                    getamount = ReturnAmount(list);
        //                    //totalAmount = payLaterCollection.Amount - returnamount - paidamount + gullakamount;
        //                    totalAmount = getamount;
        //                }
        //                if (payLaterCollection.Amount < totalAmount + Convert.ToDouble(CashAmount))
        //                {
        //                    return new APIResponse { Status = false, Message = "Your Request Already Pending for this Order" };
        //                }

        //                //People People = db.Peoples.Where(x => x.PeopleID == userid).FirstOrDefault();
        //                //var CurrencyHubStock = db.CurrencyHubStock.FirstOrDefault(x => x.Warehouseid == cashData.WarehouseId && EntityFunctions.TruncateTime(x.BOD) == EntityFunctions.TruncateTime(DateTime.Now));
        //                //var denomination = db.CurrencyDenomination.Where(x => x.IsActive).ToList();
        //                //List<CashCollection> CashCollections = new List<CashCollection>();

        //                //var denominationids = cashData.cashList.Where(x => x.CurrencyCount > 0).Select(x => x.Id).ToList();
        //                //var cashCollections = db.CashCollection.Where(x => x.CurrencyHubStockId == CurrencyHubStock.Id && denominationids.Contains(x.CurrencyDenominationId));
        //                //foreach (var cash in cashCollections)
        //                //{
        //                //    cash.CurrencyCountByDBoy = cashData.cashList.Any(x => x.Id == cash.CurrencyDenominationId) ? cash.CurrencyCountByDBoy + cashData.cashList.FirstOrDefault(x => x.Id == cash.CurrencyDenominationId).CurrencyCount : cash.CurrencyCountByDBoy;
        //                //    cash.ModifiedBy = userid;
        //                //    cash.ModifiedDate = indianTime;
        //                //    db.Entry(cash).State = EntityState.Modified;
        //                //}

        //                //List<HubCashCollection> hubCashCollections = db.HubCashCollection.Where(x => x.IsDeleted == false && x.CurrencyHubStockId == CurrencyHubStock.Id && denominationids.Contains(x.CurrencyDenominationId)).ToList();
        //                //if (hubCashCollections != null && hubCashCollections.Any())
        //                //{
        //                //    foreach (var hubCashCollection in hubCashCollections)
        //                //    {
        //                //        hubCashCollection.CurrencyCount = cashData.cashList.Any(x => x.Id == hubCashCollection.CurrencyDenominationId) ? hubCashCollection.CurrencyCount + cashData.cashList.FirstOrDefault(x => x.Id == hubCashCollection.CurrencyDenominationId).CurrencyCount : hubCashCollection.CurrencyCount;
        //                //        hubCashCollection.ModifiedBy = userid;
        //                //        db.Entry(hubCashCollection).State = EntityState.Modified;
        //                //    }
        //                //}

        //                string request = JsonConvert.SerializeObject(cashData);
        //                if (payLaterCollection.Amount >= (totalAmount + CashAmount))
        //                {
        //                    db.PayLaterCollectionHistoryDb.Add(new PayLaterCollectionHistory
        //                    {
        //                        Amount = Convert.ToDouble(CashAmount),
        //                        CreatedDate = DateTime.Now,
        //                        PaymentMode = "Cash",
        //                        CurrencyHubStockId = 0,
        //                        IsActive = true,
        //                        IsDeleted = false,
        //                        CreatedBy = userid,
        //                        PayLaterCollectionId = payLaterCollection.Id,
        //                        PaymentStatus = 0,
        //                        Request = request
        //                        //statusDesc = "New Cash Added on Behalf of Bounce Cheque",
        //                    });

        //                }
        //                if (db.Commit() > 0)
        //                    return new APIResponse { Status = true, Message = "Cash Added Successfully" };
        //            }
        //            else
        //                return new APIResponse { Status = false, Message = "Data Not Found!" };
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        logger.Error("Error in Added payment: " + ex.Message);
        //        return new APIResponse { Status = false, Message = ex.Message };
        //    }
        //    return new APIResponse { Status = false, Message = "Something went Wrong!" };
        //}
        #endregion


        [Route("CashPaymentPayLater")]
        [HttpPost]
        public APIResponse CashPaymentPayLater(CashdataDc cashData)
        {
            try
            {
                var identity = User.Identity as ClaimsIdentity;
                int  userid = 0;
                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                    userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
                using (AuthContext db = new AuthContext())
                {
                    var payLaterCollection = db.PayLaterCollectionDb.FirstOrDefault(x => x.OrderId == cashData.OrderId
                                                                          && x.IsActive == true
                                                                          && x.IsDeleted == false);
                    if (payLaterCollection != null)
                    {
                        double totalAmount = 0;
                        int CustomerId = 0;
                        var totalAmountlist = db.PayLaterCollectionHistoryDb.Where(x => x.PayLaterCollectionId == payLaterCollection.Id
                                                                          && x.IsActive == true
                                                                           && x.IsDeleted == false && (x.PaymentStatus==1 || x.PaymentStatus == 0)).ToList();
                        CustomerId = db.DbOrderMaster.FirstOrDefault(x => x.OrderId == payLaterCollection.OrderId).CustomerId;
                        var CashAmount = cashData.cashList.Where(x => x.CurrencyCount > 0).Sum(x => (x.CurrencyCount * x.Value));
                        if (totalAmountlist != null && totalAmountlist.Any())
                        {
                            List<PayLaterAmountUpdateDC> list = new List<PayLaterAmountUpdateDC>();
                            foreach (var item in totalAmountlist)
                            {
                                PayLaterAmountUpdateDC singlepayment = new PayLaterAmountUpdateDC();
                                singlepayment.Amount = item.Amount;
                                singlepayment.Comment = item.Comment;
                                list.Add(singlepayment);
                            }
                            //double paidamount = 0;
                            //double returnamount = 0;
                            //double gullakamount = 0;
                            //paidamount = totalAmountlist.Any(a => a.Comment != "Return Order" && a.Comment != "Gullak Refund") ? totalAmountlist.Where(a => a.Comment != "Return Order" && a.Comment != "Gullak Refund").Sum(y => y.Amount) : 0;
                            //returnamount = totalAmountlist.Any(a => a.Comment == "Return Order") ? totalAmountlist.Where(a => a.Comment == "Return Order").Sum(y => y.Amount) : 0;
                            //gullakamount = totalAmountlist.Any(a => a.Comment == "Gullak Refund") ? totalAmountlist.Where(a => a.Comment == "Gullak Refund").Sum(y => y.Amount) : 0;
                            //totalAmount = totalAmountlist.Sum(x => x.Amount);
                            double getamount = 0;
                            getamount = ReturnAmount(list);
                            //totalAmount = payLaterCollection.Amount - returnamount - paidamount + gullakamount;
                            totalAmount = getamount;
                        }
                        if (payLaterCollection.Amount < totalAmount + Convert.ToDouble(CashAmount))
                        {
                            return new APIResponse { Status = false, Message = "Amount is greater then Order Amount" };
                        }
                        var CurrencyHubStock = db.CurrencyHubStock.FirstOrDefault(x => x.Warehouseid == cashData.WarehouseId && EntityFunctions.TruncateTime(x.BOD) == EntityFunctions.TruncateTime(DateTime.Now));
                        if (CurrencyHubStock != null)
                        {
                            var denomination = db.CurrencyDenomination.Where(x => x.IsActive).ToList();
                            List<CashCollection> CashCollections = new List<CashCollection>();

                            var denominationids = cashData.cashList.Where(x => x.CurrencyCount > 0).Select(x => x.Id).ToList();
                            List<HubCashCollection> hubCashCollections = db.HubCashCollection.Where(x => x.IsDeleted == false && x.CurrencyHubStockId == CurrencyHubStock.Id && denominationids.Contains(x.CurrencyDenominationId)).ToList();
                            if (hubCashCollections != null && hubCashCollections.Any())
                            {
                                foreach (var hubCashCollection in hubCashCollections)
                                {
                                    hubCashCollection.CurrencyCount = cashData.cashList.Any(x => x.Id == hubCashCollection.CurrencyDenominationId) ? hubCashCollection.CurrencyCount + cashData.cashList.FirstOrDefault(x => x.Id == hubCashCollection.CurrencyDenominationId).CurrencyCount : hubCashCollection.CurrencyCount;
                                    hubCashCollection.ModifiedBy = userid;
                                    db.Entry(hubCashCollection).State = EntityState.Modified;
                                }
                            }
                            PayLaterCollectionHistory payLater = new PayLaterCollectionHistory();
                            payLater.Amount = Convert.ToDouble(CashAmount);
                            payLater.CreatedDate = DateTime.Now;
                            payLater.PaymentMode = "Cash";
                            payLater.CurrencyHubStockId = CurrencyHubStock.Id;
                            payLater.IsActive = true;
                            payLater.IsDeleted = false;
                            payLater.CreatedBy = userid;
                            payLater.PayLaterCollectionId = payLaterCollection.Id;
                            payLater.PaymentStatus = 1;
                            db.PayLaterCollectionHistoryDb.Add(payLater);
                            
                            if ((totalAmount + Convert.ToDouble(CashAmount)) == payLaterCollection.Amount)
                            {
                                var payorderId = db.OnlineCollection.Where(x => x.Orderid == payLaterCollection.OrderId && x.PaymentFrom == "PayLater" && x.IsActive == true && x.IsDeleted == false).FirstOrDefault();
                                if (payorderId != null)
                                {
                                    payorderId.IsActive = false;
                                    payorderId.IsDeleted = true;
                                    payorderId.ModifiedBy = userid;
                                    payorderId.ModifiedDate = DateTime.Now;
                                    db.Entry(payorderId).State = EntityState.Modified;
                                }
                                payLaterCollection.Status = Convert.ToInt32(PayCollectionEnum.Paid);
                                payLaterCollection.ModifiedDate = DateTime.Now;
                                payLaterCollection.ModifiedBy = userid;
                                db.Entry(payLaterCollection).State = EntityState.Modified;
                                #region ordermaster settle 
                                CashCollectionNewController ctrl = new CashCollectionNewController();
                                bool res = ctrl.OrderSettle(db, payLaterCollection.OrderId);
                                #endregion
                            }
                            else
                            {
                                if (payLaterCollection.Status == 2 || payLaterCollection.Status == 3) { }
                                else
                                {
                                    payLaterCollection.Status = Convert.ToInt32(PayCollectionEnum.Partial);
                                }
                                payLaterCollection.ModifiedDate = DateTime.Now;
                                payLaterCollection.ModifiedBy = userid;
                                db.Entry(payLaterCollection).State = EntityState.Modified;
                            }
                            if (db.Commit() > 0)
                            {
                                string gatewayid = "SkPayLater" + payLater.Id;
                                #region hit Ladger
                                if (AppConstants.IsUsingLedgerHitOnOnlinePayment)
                                {
                                    if (db.OnlinePaymentDtlsForLedgerDB.FirstOrDefault(z => z.OrderId == payLaterCollection.OrderId && z.TransactionId == gatewayid) == null)
                                    {
                                        OnlinePaymentDtlsForLedger Opdl = new OnlinePaymentDtlsForLedger();
                                        Opdl.OrderId = payLaterCollection.OrderId;
                                        Opdl.IsPaymentSuccess = true;
                                        Opdl.IsLedgerAffected = "Yes";
                                        Opdl.PaymentDate = DateTime.Now;
                                        Opdl.TransactionId = gatewayid;
                                        Opdl.IsActive = true;
                                        Opdl.CustomerId = CustomerId;
                                        db.OnlinePaymentDtlsForLedgerDB.Add(Opdl);
                                    }
                                }
                                #endregion
                                db.Commit();
                                return new APIResponse { Status = true, Message = "Cash Added Successfully" };
                            }
                            else
                            {
                                return new APIResponse { Status = false, Message = "Something Went Wrong" };
                            }
                        }
                        else
                        {
                            return new APIResponse { Status = false, Message = "Something Went Wrong" };
                        }
                        //People People = db.Peoples.Where(x => x.PeopleID == userid).FirstOrDefault();
                        //var CurrencyHubStock = db.CurrencyHubStock.FirstOrDefault(x => x.Warehouseid == cashData.WarehouseId && EntityFunctions.TruncateTime(x.BOD) == EntityFunctions.TruncateTime(DateTime.Now));
                        //var denomination = db.CurrencyDenomination.Where(x => x.IsActive).ToList();
                        //List<CashCollection> CashCollections = new List<CashCollection>();

                        //var denominationids = cashData.cashList.Where(x => x.CurrencyCount > 0).Select(x => x.Id).ToList();
                        //var cashCollections = db.CashCollection.Where(x => x.CurrencyHubStockId == CurrencyHubStock.Id && denominationids.Contains(x.CurrencyDenominationId));
                        //foreach (var cash in cashCollections)
                        //{
                        //    cash.CurrencyCountByDBoy = cashData.cashList.Any(x => x.Id == cash.CurrencyDenominationId) ? cash.CurrencyCountByDBoy + cashData.cashList.FirstOrDefault(x => x.Id == cash.CurrencyDenominationId).CurrencyCount : cash.CurrencyCountByDBoy;
                        //    cash.ModifiedBy = userid;
                        //    cash.ModifiedDate = indianTime;
                        //    db.Entry(cash).State = EntityState.Modified;
                        //}

                        //List<HubCashCollection> hubCashCollections = db.HubCashCollection.Where(x => x.IsDeleted == false && x.CurrencyHubStockId == CurrencyHubStock.Id && denominationids.Contains(x.CurrencyDenominationId)).ToList();
                        //if (hubCashCollections != null && hubCashCollections.Any())
                        //{
                        //    foreach (var hubCashCollection in hubCashCollections)
                        //    {
                        //        hubCashCollection.CurrencyCount = cashData.cashList.Any(x => x.Id == hubCashCollection.CurrencyDenominationId) ? hubCashCollection.CurrencyCount + cashData.cashList.FirstOrDefault(x => x.Id == hubCashCollection.CurrencyDenominationId).CurrencyCount : hubCashCollection.CurrencyCount;
                        //        hubCashCollection.ModifiedBy = userid;
                        //        db.Entry(hubCashCollection).State = EntityState.Modified;
                        //    }
                        //}

                        //string request = JsonConvert.SerializeObject(cashData);
                        //if (payLaterCollection.Amount >= (totalAmount + CashAmount)  )
                        //{
                        //    db.PayLaterCollectionHistoryDb.Add(new PayLaterCollectionHistory
                        //    {
                        //        Amount = Convert.ToDouble(CashAmount),
                        //        CreatedDate = DateTime.Now,
                        //        PaymentMode = "Cash",
                        //        CurrencyHubStockId = 0,
                        //        IsActive = true,
                        //        IsDeleted = false,
                        //        CreatedBy = userid,
                        //        PayLaterCollectionId = payLaterCollection.Id,
                        //        PaymentStatus = 0,
                        //        Request= request
                        //        //statusDesc = "New Cash Added on Behalf of Bounce Cheque",
                        //    });

                        //}
                        
                            
                    }
                    else
                        return new APIResponse { Status = false, Message = "Data Not Found!" };
                }
            }
            catch (Exception ex)
            {
                logger.Error("Error in Added payment: " + ex.Message);
                return new APIResponse { Status = false, Message = ex.Message };
            }
            return new APIResponse { Status = false, Message = "Something went Wrong!" };
        }

        [Route("ChequePaymentAddPayLater")]
        [HttpPost]
        public APIResponse ChequePaymentAddPayLater(ChequePaymentDC ChequeList)
        {
            try
            {
                var identity = User.Identity as ClaimsIdentity;
                int userid = 0;
                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                    userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

                using (AuthContext db = new AuthContext())
                {
                    var payLaterCollection = db.PayLaterCollectionDb.FirstOrDefault(x => x.OrderId == ChequeList.OrderId
                                                                   && x.IsActive == true
                                                                   && x.IsDeleted == false);
                    if (ChequeList.BankId == 0)
                    {
                        return new APIResponse { Status = false, Message = "Please Select Bank " };
                    }
                    if (payLaterCollection != null)
                    {
                        var History = db.PayLaterCollectionHistoryDb.Where(x => x.PayLaterCollectionId == payLaterCollection.Id
                                                                        && x.IsActive == true
                                                                         && x.IsDeleted == false && (x.PaymentStatus == 0 || x.PaymentStatus == 1)).ToList();
                        double totalPaidAmount = 0;
                        if (History != null && History.Count > 0)
                        {
                            //totalPaidAmount = History.Sum(x => x.Amount);
                            //double paidamount = 0;
                            //double returnamount = 0;
                            //double gullakamount = 0;
                            //paidamount = History.Any(a => a.Comment != "Return Order" && a.Comment != "Gullak Refund") ? History.Where(a => a.Comment != "Return Order" && a.Comment != "Gullak Refund").Sum(y => y.Amount) : 0;
                            //returnamount = History.Any(a => a.Comment == "Return Order") ? History.Where(a => a.Comment == "Return Order").Sum(y => y.Amount) : 0;
                            //gullakamount = History.Any(a => a.Comment == "Gullak Refund") ? History.Where(a => a.Comment == "Gullak Refund").Sum(y => y.Amount) : 0;
                            //totalAmount = totalAmountlist.Sum(x => x.Amount);
                            //totalPaidAmount = payLaterCollection.Amount - returnamount - paidamount + gullakamount;
                            List<PayLaterAmountUpdateDC> list = new List<PayLaterAmountUpdateDC>();
                            foreach (var item in History)
                            {
                                PayLaterAmountUpdateDC singlepayment = new PayLaterAmountUpdateDC();
                                singlepayment.Amount = item.Amount;
                                singlepayment.Comment = item.Comment;
                                list.Add(singlepayment);
                            }
                            double getamount = 0;
                            getamount = ReturnAmount(list);
                            totalPaidAmount = getamount;

                        }
                        if (payLaterCollection.Amount < totalPaidAmount + Convert.ToDouble(ChequeList.ChequeAmt))
                        {
                            return new APIResponse { Status = false, Message = "Please Refresh Page" };
                        }
                        People People = db.Peoples.Where(x => x.PeopleID == userid).FirstOrDefault();
                        var CurrencyHubStock = db.CurrencyHubStock.FirstOrDefault(x => x.Warehouseid == ChequeList.Warehouseid && EntityFunctions.TruncateTime(x.BOD) == EntityFunctions.TruncateTime(DateTime.Now));
                        var BankName = db.CurrencySettlementBank.Where(x => x.Id == ChequeList.BankId).Select(x => x.BankName).FirstOrDefault();
                        //ChequeCollection cheque = db.ChequeCollection.Where(y => y.CurrencyCollectionId).FirstOrDefault();
                        //ReturnChequeCollection ReturnChequedata = db.ReturnChequeCollection.Where(x => x.Id == ChequeList.Id).FirstOrDefault();
                        //var GetCollectionId = db.ChequeCollection.Where(x => x.Id == ReturnChequedata.ChequeCollectionId).Select(x => x.CurrencyCollectionId).FirstOrDefault();
                        if (CurrencyHubStock != null)
                        {
                            ChequeCollection Payment = new ChequeCollection();
                            Payment.ChequeStatus = ChequeList.Status = 1;
                            Payment.ChequeAmt = ChequeList.ChequeAmt;
                            Payment.ChequeBankName = BankName;
                            Payment.ChequeNumber = ChequeList.ChequeNumber;
                            Payment.WarehouseId = ChequeList.Warehouseid;
                            Payment.Orderid = ChequeList.OrderId;
                            Payment.ChequeDate = ChequeList.ChequeDate;
                            Payment.CreatedDate = indianTime;
                            Payment.CreatedBy = userid;
                            Payment.CurrencyHubStockId = CurrencyHubStock.Id;
                            Payment.IsActive = true;
                            Payment.IsDeleted = false;
                            Payment.ChequeimagePath = ChequeList.ChequeimagePath;
                            Payment.CurrencySettlementSourceId = ChequeList.BankId;
                            string request = JsonConvert.SerializeObject(Payment);
                            if ((totalPaidAmount + Convert.ToDouble(ChequeList.ChequeAmt)) <= payLaterCollection.Amount)
                            {
                                //var payorderId = db.OnlineCollection.Where(x => x.Orderid == ChequeList.OrderId && x.PaymentFrom == "PayLater" && x.IsActive == true && x.IsDeleted == false).FirstOrDefault();
                                //if (payorderId != null)
                                //{
                                //    payorderId.IsActive = false;
                                //    payorderId.IsDeleted = true;
                                //    payorderId.ModifiedBy = userid;
                                //    payorderId.ModifiedDate = DateTime.Now;
                                //    db.Entry(payorderId).State = EntityState.Modified;
                                //}
                                db.PayLaterCollectionHistoryDb.Add(new PayLaterCollectionHistory
                                {
                                    Amount = Convert.ToDouble(ChequeList.ChequeAmt),
                                    RefNo = ChequeList.ChequeNumber,
                                    CreatedDate = DateTime.Now,
                                    PaymentMode = "Cheque",
                                    CurrencyHubStockId = 0,
                                    IsActive = true,
                                    IsDeleted = false,
                                    CreatedBy = userid,
                                    PayLaterCollectionId = payLaterCollection.Id,
                                    Request = request
                                    //statusDesc = "New Cash Added on Behalf of Bounce Cheque",
                                });
                            }
                        }
                    }
                    else
                        return new APIResponse { Status = false, Message = "Data Not Found" };

                    if (db.Commit() > 0)
                        return new APIResponse { Status = true, Message = "cheque Added Successfully" };
                    else
                        return new APIResponse { Status = false, Message = "Something Went Wrong!" };
                }
            }
            catch (Exception ex)
            {
                logger.Error("Error in Settel: " + ex.Message);
                return new APIResponse { Status = false, Message = ex.Message };
            }
        }
        [Route("OnlinePaymentPayLater")]
        [HttpPost]
        public APIResponse OnlinePaymentPayLater(OnlineCollectionDc onlinedata)
        {
            try
            {
                var identity = User.Identity as ClaimsIdentity;
                int compid = 0, userid = 0, Warehouse_id = 0;

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))
                    compid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                    userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "Warehouseid"))
                    Warehouse_id = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "Warehouseid").Value);
                using (AuthContext db = new AuthContext())
                {
                    var payLaterCollection = db.PayLaterCollectionDb.FirstOrDefault(x => x.OrderId == onlinedata.Orderid
                                                                     && x.IsActive == true
                                                                     && x.IsDeleted == false);
                    if (payLaterCollection != null)
                    {
                        var History = db.PayLaterCollectionHistoryDb.Where(x => x.PayLaterCollectionId == payLaterCollection.Id
                                                && x.IsActive == true
                                                 && x.IsDeleted == false && (x.PaymentStatus==0 || x.PaymentStatus ==1)).ToList();
                        double totalPaidAmount = 0;
                        if (History != null && History.Count > 0)
                        {
                            //totalPaidAmount = History.Sum(x => x.Amount);
                            //double paidamount = 0;
                            //double returnamount = 0;
                            //double gullakamount = 0;
                            //paidamount = History.Any(a => a.Comment != "Return Order" && a.Comment != "Gullak Refund") ? History.Where(a => a.Comment != "Return Order" && a.Comment != "Gullak Refund").Sum(y => y.Amount) : 0;
                            //returnamount = History.Any(a => a.Comment == "Return Order") ? History.Where(a => a.Comment == "Return Order").Sum(y => y.Amount) : 0;
                            //gullakamount = History.Any(a => a.Comment == "Gullak Refund") ? History.Where(a => a.Comment == "Gullak Refund").Sum(y => y.Amount) : 0;
                            //totalAmount = totalAmountlist.Sum(x => x.Amount);
                            List<PayLaterAmountUpdateDC> list = new List<PayLaterAmountUpdateDC>();
                            foreach (var item in History)
                            {
                                PayLaterAmountUpdateDC singlepayment = new PayLaterAmountUpdateDC();
                                singlepayment.Amount = item.Amount;
                                singlepayment.Comment = item.Comment;
                                list.Add(singlepayment);
                            }
                            double getamount = 0;
                            getamount = ReturnAmount(list);
                            //totalPaidAmount = payLaterCollection.Amount - returnamount - paidamount + gullakamount;
                            totalPaidAmount =  getamount;
                        }
                        var CurrencyHubStock = db.CurrencyHubStock.FirstOrDefault(x => x.Warehouseid == onlinedata.WarehouseId && EntityFunctions.TruncateTime(x.BOD) == EntityFunctions.TruncateTime(DateTime.Now));
                        if (CurrencyHubStock != null)
                        {
                            if (onlinedata.MPOSAmt > 0)
                            {
                                if (payLaterCollection.Amount < totalPaidAmount + Convert.ToDouble(onlinedata.MPOSAmt))
                                {
                                    return new APIResponse { Status = false, Message = "Please Refresh Page" };
                                }
                                OnlineCollection Online = new OnlineCollection();
                                Online.PaymentGetwayAmt = onlinedata.PaymentGetwayAmt;
                                Online.MPOSAmt = onlinedata.MPOSAmt;
                                Online.PaymentReferenceNO = onlinedata.PaymentReferenceNO;
                                Online.MPOSReferenceNo = onlinedata.MPOSReferenceNo;
                                Online.Orderid = onlinedata.Orderid;
                                Online.CreatedDate = indianTime;
                                Online.CreatedBy = userid;
                                Online.CurrencyHubStockId = CurrencyHubStock.Id;
                                Online.IsActive = true;
                                Online.IsDeleted = false;
                                Online.PaymentFrom = "mPos";
                                Online.WarehouseId = onlinedata.WarehouseId;
                                
                                //db.OnlineCollection.Add(Online);
                                string request = JsonConvert.SerializeObject(Online);
                                if ((totalPaidAmount + Convert.ToDouble(onlinedata.MPOSAmt)) <= payLaterCollection.Amount)
                                {
                                    //var payorderId = db.OnlineCollection.Where(x => x.Orderid == onlinedata.Orderid && x.PaymentFrom == "PayLater" && x.IsActive == true && x.IsDeleted == false).FirstOrDefault();
                                    //if (payorderId != null)
                                    //{
                                    //    payorderId.IsActive = false;
                                    //    payorderId.IsDeleted = true;
                                    //    payorderId.ModifiedBy = userid;
                                    //    payorderId.ModifiedDate = DateTime.Now;
                                    //    db.Entry(payorderId).State = EntityState.Modified;
                                    //}
                                    db.PayLaterCollectionHistoryDb.Add(new PayLaterCollectionHistory
                                    {
                                        Amount = Convert.ToDouble(onlinedata.MPOSAmt),
                                        RefNo = onlinedata.MPOSReferenceNo,
                                        CreatedDate = DateTime.Now,
                                        PaymentMode = "mPos",
                                        CurrencyHubStockId = CurrencyHubStock.Id,
                                        IsActive = true,
                                        IsDeleted = false,
                                        CreatedBy = userid,
                                        PayLaterCollectionId = payLaterCollection.Id,
                                        Request = request
                                        //statusDesc = "New Cash Added on Behalf of Bounce Cheque",
                                    });
                                    //payLaterCollection.Status = Convert.ToInt32(PayCollectionEnum.Paid);
                                    //payLaterCollection.ModifiedDate = DateTime.Now;
                                    //payLaterCollection.ModifiedBy = userid;
                                    //db.Entry(payLaterCollection).State = EntityState.Modified;
                                }
                            }
                            else if (onlinedata.PaymentGetwayAmt > 0)
                            {
                                OnlineCollection Online = new OnlineCollection();
                                Online.PaymentGetwayAmt = onlinedata.PaymentGetwayAmt;
                                Online.MPOSAmt = onlinedata.MPOSAmt;
                                Online.PaymentReferenceNO = onlinedata.PaymentReferenceNO;
                                Online.MPOSReferenceNo = onlinedata.MPOSReferenceNo;
                                Online.Orderid = onlinedata.Orderid;
                                Online.CreatedDate = indianTime;
                                Online.CreatedBy = userid;
                                Online.CurrencyHubStockId = CurrencyHubStock.Id;
                                Online.IsActive = true;
                                Online.IsDeleted = false;
                                Online.PaymentFrom = "RTGS/NEFT";
                                Online.WarehouseId = onlinedata.WarehouseId;
                                string request = JsonConvert.SerializeObject(Online);
                                if ((totalPaidAmount + Convert.ToDouble(onlinedata.PaymentGetwayAmt)) <= payLaterCollection.Amount)
                                {
                                    //var payorderId = db.OnlineCollection.Where(x => x.Orderid == onlinedata.Orderid && x.PaymentFrom == "PayLater" && x.IsActive == true && x.IsDeleted == false).FirstOrDefault();
                                    //if (payorderId != null)
                                    //{
                                    //    payorderId.IsActive = false;
                                    //    payorderId.IsDeleted = true;
                                    //    payorderId.ModifiedBy = userid;
                                    //    payorderId.ModifiedDate = DateTime.Now;
                                    //    db.Entry(payorderId).State = EntityState.Modified;
                                    //}

                                    db.PayLaterCollectionHistoryDb.Add(new PayLaterCollectionHistory
                                    {
                                        Amount = Convert.ToDouble(onlinedata.PaymentGetwayAmt),
                                        RefNo = onlinedata.PaymentReferenceNO,
                                        CreatedDate = DateTime.Now,
                                        PaymentMode = "RTGS/NEFT",
                                        CurrencyHubStockId = CurrencyHubStock.Id,
                                        IsActive = true,
                                        IsDeleted = false,
                                        CreatedBy = userid,
                                        PayLaterCollectionId = payLaterCollection.Id
                                        ,Request = request
                                        //statusDesc = "New Cash Added on Behalf of Bounce Cheque",
                                    });
                                    //payLaterCollection.Status = Convert.ToInt32(PayCollectionEnum.Paid);
                                    //payLaterCollection.ModifiedDate = DateTime.Now;
                                    //payLaterCollection.ModifiedBy = userid;
                                    //db.Entry(payLaterCollection).State = EntityState.Modified;
                                }
                                
                            }
                            else if (onlinedata.UPIAmt > 0)
                            {
                                OnlineCollection Online = new OnlineCollection();
                                Online.PaymentGetwayAmt = onlinedata.UPIAmt;
                                Online.MPOSAmt = onlinedata.MPOSAmt;
                                Online.PaymentReferenceNO = onlinedata.PaymentReferenceNO;
                                Online.MPOSReferenceNo = onlinedata.MPOSReferenceNo;
                                Online.Orderid = onlinedata.Orderid;
                                Online.CreatedDate = indianTime;
                                Online.CreatedBy = userid;
                                Online.CurrencyHubStockId = CurrencyHubStock.Id;
                                Online.IsActive = true;
                                Online.IsDeleted = false;
                                Online.PaymentFrom = "UPI";
                                Online.WarehouseId = onlinedata.WarehouseId;
                                //db.OnlineCollection.Add(Online);
                                string request = JsonConvert.SerializeObject(Online);
                                if ((totalPaidAmount + Convert.ToDouble(onlinedata.UPIAmt)) <= payLaterCollection.Amount)
                                {
                                    //var payorderId = db.OnlineCollection.Where(x => x.Orderid == onlinedata.Orderid && x.PaymentFrom == "PayLater" && x.IsActive == true && x.IsDeleted == false).FirstOrDefault();
                                    //if (payorderId != null)
                                    //{
                                    //    payorderId.IsActive = false;
                                    //    payorderId.IsDeleted = true;
                                    //    payorderId.ModifiedBy = userid;
                                    //    payorderId.ModifiedDate = DateTime.Now;
                                    //    db.Entry(payorderId).State = EntityState.Modified;
                                    //}
                                    db.PayLaterCollectionHistoryDb.Add(new PayLaterCollectionHistory
                                    {
                                        Amount = Convert.ToDouble(onlinedata.UPIAmt),
                                        RefNo = onlinedata.PaymentReferenceNO,
                                        CreatedDate = DateTime.Now,
                                        PaymentMode = "UPI",
                                        CurrencyHubStockId = CurrencyHubStock.Id,
                                        IsActive = true,
                                        IsDeleted = false,
                                        CreatedBy = userid,
                                        PayLaterCollectionId = payLaterCollection.Id,
                                        Request = request
                                        //statusDesc = "New Cash Added on Behalf of Bounce Cheque",
                                    });
                                    //payLaterCollection.Status = Convert.ToInt32(PayCollectionEnum.Paid);
                                    //payLaterCollection.ModifiedDate = DateTime.Now;
                                    //payLaterCollection.ModifiedBy = userid;
                                    //db.Entry(payLaterCollection).State = EntityState.Modified;
                                }
                                
                            }


                        }
                    }
                    else
                        return new APIResponse { Status = false, Message = "Data Not Found" };
                    if (db.Commit() > 0)
                        return new APIResponse { Status = true, Message = "Added Successfully" };
                    else
                        return new APIResponse { Status = false, Message = "Something Went Wrong!" };

                }
            }
            catch (Exception ex)
            {
                logger.Error("Error in Settel: " + ex.Message);
                return new APIResponse { Status = false, Message = ex.Message };
            }
        }

        [Route("AddCustomerLimit")]
        [HttpPost]
        public APIResponse AddCustomerLimit(AddCustomerLimitDC addCustomer)
        {
            try
            {
                var identity = User.Identity as ClaimsIdentity;
                int userid = 0;

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                    userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

                using (var Context = new AuthContext())
                {
                    var storeCreditLimit = Context.StoreCreditLimitDb.Where(x => x.StoreId == addCustomer.StoreId && x.CustomerId == addCustomer.CustomerId).FirstOrDefault();
                    if (storeCreditLimit == null)
                    {
                        Context.StoreCreditLimitDb.Add(new StoreCreditLimit
                        {
                            StoreId = addCustomer.StoreId,
                            CustomerId = addCustomer.CustomerId,
                            CreditLimit = addCustomer.CreditLimit,
                            DueDays = addCustomer.DueDays,
                            CreatedDate = DateTime.Now,
                            ModifiedDate = null,
                            IsActive = true,
                            IsDeleted = false,
                            CreatedBy = userid,
                            ModifiedBy = null
                        });
                        if (Context.Commit() > 0)
                            return new APIResponse { Status = true, Message = "Data Saved" };
                        else
                            return new APIResponse { Status = false, Message = "Something Went Wrong!" };
                    }
                    else
                        return new APIResponse { Status = false, Message = "Customer Already Exists" };
                }
            }
            catch (Exception ex)
            {
                logger.Error("Error in Settel: " + ex.Message);
                return new APIResponse { Status = false, Message = ex.Message };
            }
        }

        [Route("GetSkCodeCustomerList")]
        [HttpGet]
        [AllowAnonymous]
        public List<Customer> GetSkCodeCustomerList(string Skcode)
        {
            using (var db = new AuthContext())
            {
                List<Customer> res = new List<Customer>();
                if (Skcode != null)
                {
                    string query = "select * from Customers where Skcode like '%" + Skcode + "%' and Active=1 and Deleted=0";
                    res = db.Database.SqlQuery<Customer>(query).ToList();
                }
                return res;
            }
        }

        //[Route("PaymentApprove")]
        //[HttpGet]
        //public APIResponse PaymentApprove(long Id)
        //{
        //    try
        //    {
        //        var identity = User.Identity as ClaimsIdentity;
        //        int userid = 0;
        //        if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
        //            userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
        //        using (var db = new AuthContext())
        //        {
        //            var paylaterhistorydata = db.PayLaterCollectionHistoryDb.FirstOrDefault(x => x.Id == Id && x.IsActive==true && x.IsDeleted == false );
        //            if (paylaterhistorydata != null)
        //            {
        //                double totalpaidamount = 0;
        //                int CustomerId = 0;
        //                var paylatercollection = db.PayLaterCollectionDb.FirstOrDefault(x => x.Id == paylaterhistorydata.PayLaterCollectionId && x.IsActive == true && x.IsDeleted == false);
        //                var paylatercollectionhistory = db.PayLaterCollectionHistoryDb.Where(x => x.PayLaterCollectionId == paylaterhistorydata.PayLaterCollectionId && x.IsActive == true && x.IsDeleted == false && x.PaymentStatus == 1).ToList();
        //                var orderdispatchmaster = db.OrderDispatchedMasters.Where(x => x.OrderId == paylatercollection.OrderId).FirstOrDefault();
        //                CustomerId = db.DbOrderMaster.FirstOrDefault(x => x.OrderId == paylatercollection.OrderId).CustomerId;
        //                if (paylatercollectionhistory != null && paylatercollectionhistory.Any())
        //                {
        //                    //totalpaidamount = paylatercollectionhistory.Sum(x => x.Amount);
        //                    //double paidamount = 0;
        //                    //double returnamount = 0;
        //                    //double gullakamount = 0;
        //                    //paidamount = paylatercollectionhistory.Any(a => a.Comment != "Return Order" && a.Comment != "Gullak Refund") ? paylatercollectionhistory.Where(a => a.Comment != "Return Order" && a.Comment != "Gullak Refund").Sum(y => y.Amount) : 0;
        //                    //returnamount = paylatercollectionhistory.Any(a => a.Comment == "Return Order") ? paylatercollectionhistory.Where(a => a.Comment == "Return Order").Sum(y => y.Amount) : 0;
        //                    //gullakamount = paylatercollectionhistory.Any(a => a.Comment == "Gullak Refund") ? paylatercollectionhistory.Where(a => a.Comment == "Gullak Refund").Sum(y => y.Amount) : 0;
        //                    //totalAmount = totalAmountlist.Sum(x => x.Amount);
        //                    List<PayLaterAmountUpdateDC> list = new List<PayLaterAmountUpdateDC>();
        //                    foreach (var item in paylatercollectionhistory)
        //                    {
        //                        PayLaterAmountUpdateDC singlepayment = new PayLaterAmountUpdateDC();
        //                        singlepayment.Amount = item.Amount;
        //                        singlepayment.Comment = item.Comment;
        //                        list.Add(singlepayment);
        //                    }
        //                    double getamount = 0;
        //                    getamount = ReturnAmount(list);
        //                    //totalpaidamount = paylatercollection.Amount - returnamount - paidamount + gullakamount;
        //                    totalpaidamount =  getamount;
        //                }

        //                if((totalpaidamount+paylaterhistorydata.Amount) > paylatercollection.Amount)
        //                {
        //                    return new APIResponse { Status = false, Message = "Amount is greater then Total Amount" };
        //                }

        //                #region for cash
        //                if(paylaterhistorydata.PaymentMode== "Cash")
        //                {
        //                    CashdataDc cashData = JsonConvert.DeserializeObject<CashdataDc>(paylaterhistorydata.Request);
        //                    var CurrencyHubStock = db.CurrencyHubStock.FirstOrDefault(x => x.Warehouseid == cashData.WarehouseId && EntityFunctions.TruncateTime(x.BOD) == EntityFunctions.TruncateTime(DateTime.Now));
        //                    if(CurrencyHubStock != null)
        //                    {
        //                        var denomination = db.CurrencyDenomination.Where(x => x.IsActive).ToList();
        //                        List<CashCollection> CashCollections = new List<CashCollection>();

        //                        var denominationids = cashData.cashList.Where(x => x.CurrencyCount > 0).Select(x => x.Id).ToList();
        //                        //var cashCollections = db.CashCollection.Where(x => x.CurrencyHubStockId == CurrencyHubStock.Id && denominationids.Contains(x.CurrencyDenominationId));
        //                        //foreach (var cash in cashCollections)
        //                        //{
        //                        //    cash.CurrencyCountByDBoy = cashData.cashList.Any(x => x.Id == cash.CurrencyDenominationId) ? cash.CurrencyCountByDBoy + cashData.cashList.FirstOrDefault(x => x.Id == cash.CurrencyDenominationId).CurrencyCount : cash.CurrencyCountByDBoy;
        //                        //    cash.ModifiedBy = userid;
        //                        //    cash.ModifiedDate = indianTime;
        //                        //    db.Entry(cash).State = EntityState.Modified;
        //                        //}

        //                        List<HubCashCollection> hubCashCollections = db.HubCashCollection.Where(x => x.IsDeleted == false && x.CurrencyHubStockId == CurrencyHubStock.Id && denominationids.Contains(x.CurrencyDenominationId)).ToList();
        //                        if (hubCashCollections != null && hubCashCollections.Any())
        //                        {
        //                            foreach (var hubCashCollection in hubCashCollections)
        //                            {
        //                                hubCashCollection.CurrencyCount = cashData.cashList.Any(x => x.Id == hubCashCollection.CurrencyDenominationId) ? hubCashCollection.CurrencyCount + cashData.cashList.FirstOrDefault(x => x.Id == hubCashCollection.CurrencyDenominationId).CurrencyCount : hubCashCollection.CurrencyCount;
        //                                hubCashCollection.ModifiedBy = userid;
        //                                db.Entry(hubCashCollection).State = EntityState.Modified;
        //                            }
        //                        }

        //                        string gatewayid = "SkPayLater" + Id;
        //                        if ((totalpaidamount + paylaterhistorydata.Amount) == paylatercollection.Amount)
        //                        {
        //                            var payorderId = db.OnlineCollection.Where(x => x.Orderid == paylatercollection.OrderId && x.PaymentFrom == "PayLater" && x.IsActive == true && x.IsDeleted == false).FirstOrDefault();
        //                            if (payorderId != null)
        //                            {
        //                                payorderId.IsActive = false;
        //                                payorderId.IsDeleted = true;
        //                                payorderId.ModifiedBy = userid;
        //                                payorderId.ModifiedDate = DateTime.Now;
        //                                db.Entry(payorderId).State = EntityState.Modified;
        //                            }

        //                            paylaterhistorydata.PaymentStatus = 1;
        //                            paylaterhistorydata.ModifiedBy = userid;
        //                            paylaterhistorydata.ModifiedDate = DateTime.Now;
        //                            db.Entry(paylaterhistorydata).State = EntityState.Modified;

        //                            paylatercollection.Status = Convert.ToInt32(PayCollectionEnum.Paid);
        //                            paylatercollection.ModifiedDate = DateTime.Now;
        //                            paylatercollection.ModifiedBy = userid;
        //                            db.Entry(paylatercollection).State = EntityState.Modified;

        //                            #region ordermaster settle 
        //                            CashCollectionNewController ctrl = new CashCollectionNewController();
        //                            bool res = ctrl.OrderSettle(db, paylatercollection.OrderId);
        //                            #endregion
        //                        }
        //                        else
        //                        {
        //                            paylaterhistorydata.PaymentStatus = 1;
        //                            paylaterhistorydata.ModifiedBy = userid;
        //                            paylaterhistorydata.ModifiedDate = DateTime.Now;
        //                            db.Entry(paylaterhistorydata).State = EntityState.Modified;
        //                            if (paylatercollection.Status == 2 || paylatercollection.Status == 3) { }
        //                            else
        //                            {
        //                                paylatercollection.Status = Convert.ToInt32(PayCollectionEnum.Partial);
        //                            }
        //                            paylatercollection.ModifiedDate = DateTime.Now;
        //                            paylatercollection.ModifiedBy = userid;
        //                            db.Entry(paylatercollection).State = EntityState.Modified;
        //                        }
        //                        #region hit Ladger
        //                        if (AppConstants.IsUsingLedgerHitOnOnlinePayment)
        //                        {

        //                            if (db.OnlinePaymentDtlsForLedgerDB.FirstOrDefault(z => z.OrderId == paylatercollection.OrderId && z.TransactionId == gatewayid) == null)
        //                            {

        //                                OnlinePaymentDtlsForLedger Opdl = new OnlinePaymentDtlsForLedger();
        //                                Opdl.OrderId = paylatercollection.OrderId;
        //                                Opdl.IsPaymentSuccess = true;
        //                                Opdl.IsLedgerAffected = "Yes";
        //                                Opdl.PaymentDate = DateTime.Now;
        //                                Opdl.TransactionId = gatewayid;
        //                                Opdl.IsActive = true;
        //                                Opdl.CustomerId = CustomerId;
        //                                db.OnlinePaymentDtlsForLedgerDB.Add(Opdl);
        //                            }
        //                        }
        //                        #endregion
        //                    }
        //                    else
        //                    {
        //                        return new APIResponse { Status = false, Message = "Something Went Wrong" };
        //                    }
        //                }
        //                #endregion

        //                #region For Cheque
        //                else if(paylaterhistorydata.PaymentMode == "Cheque")
        //                {
        //                    CurrencyCollection currencyCollection = new CurrencyCollection();
        //                    int deliveryissueid = orderdispatchmaster != null && orderdispatchmaster.DeliveryIssuanceIdOrderDeliveryMaster >0 ? Convert.ToInt32(orderdispatchmaster.DeliveryIssuanceIdOrderDeliveryMaster) : 0;
        //                    if(deliveryissueid > 0)
        //                    {
        //                        currencyCollection = db.CurrencyCollection.Where(x => x.Deliveryissueid == deliveryissueid).FirstOrDefault();
        //                    }
        //                    if(currencyCollection != null)
        //                    {
        //                        ChequeCollection Payment = JsonConvert.DeserializeObject<ChequeCollection>(paylaterhistorydata.Request);
        //                        var CurrencyHubStock = db.CurrencyHubStock.FirstOrDefault(x => x.Warehouseid == Payment.WarehouseId && EntityFunctions.TruncateTime(x.BOD) == EntityFunctions.TruncateTime(DateTime.Now));
        //                        if (CurrencyHubStock != null)
        //                        {
        //                            ChequeCollection Payments = new ChequeCollection();
        //                            Payments.ChequeStatus = Payment.ChequeStatus;
        //                            Payments.ChequeAmt = Payment.ChequeAmt;
        //                            Payments.ChequeBankName = Payment.ChequeBankName;
        //                            Payments.ChequeNumber = Payment.ChequeNumber;
        //                            Payments.WarehouseId = Payment.WarehouseId;
        //                            Payments.Orderid = Payment.Orderid;
        //                            Payments.ChequeDate = Payment.ChequeDate;
        //                            Payments.CreatedDate = indianTime;
        //                            Payments.CreatedBy = userid;
        //                            Payments.CurrencyHubStockId = CurrencyHubStock.Id;
        //                            Payments.IsActive = true;
        //                            Payments.IsDeleted = false;
        //                            Payments.ChequeimagePath = Payment.ChequeimagePath;
        //                            Payments.CurrencyCollectionId = currencyCollection != null ? currencyCollection.Id : 0;
        //                            Payments.DBoyPeopleId = orderdispatchmaster != null && orderdispatchmaster.DBoyId > 0 ? orderdispatchmaster.DBoyId : 0;

        //                            db.ChequeCollection.Add(Payments);

        //                            string gatewayid = "SkPayLater" + Id;
        //                            if ((totalpaidamount + paylaterhistorydata.Amount) == paylatercollection.Amount)
        //                            {
        //                                var payorderId = db.OnlineCollection.Where(x => x.Orderid == paylatercollection.OrderId && x.PaymentFrom == "PayLater" && x.IsActive == true && x.IsDeleted == false).FirstOrDefault();
        //                                if (payorderId != null)
        //                                {
        //                                    payorderId.IsActive = false;
        //                                    payorderId.IsDeleted = true;
        //                                    payorderId.ModifiedBy = userid;
        //                                    payorderId.ModifiedDate = DateTime.Now;
        //                                    db.Entry(payorderId).State = EntityState.Modified;
        //                                }

        //                                paylaterhistorydata.PaymentStatus = 1;
        //                                paylaterhistorydata.ModifiedBy = userid;
        //                                paylaterhistorydata.ModifiedDate = DateTime.Now;
        //                                db.Entry(paylaterhistorydata).State = EntityState.Modified;

        //                                paylatercollection.Status = Convert.ToInt32(PayCollectionEnum.Paid);
        //                                paylatercollection.ModifiedDate = DateTime.Now;
        //                                paylatercollection.ModifiedBy = userid;
        //                                db.Entry(paylatercollection).State = EntityState.Modified;

        //                                #region ordermaster settle 
        //                                CashCollectionNewController ctrl = new CashCollectionNewController();
        //                                bool res = ctrl.OrderSettle(db, paylatercollection.OrderId);
        //                                #endregion
        //                            }
        //                            else
        //                            {
        //                                paylaterhistorydata.PaymentStatus = 1;
        //                                paylaterhistorydata.ModifiedBy = userid;
        //                                paylaterhistorydata.ModifiedDate = DateTime.Now;
        //                                db.Entry(paylaterhistorydata).State = EntityState.Modified;

        //                                if (paylatercollection.Status == 2 || paylatercollection.Status == 3) { }
        //                                else
        //                                {
        //                                    paylatercollection.Status = Convert.ToInt32(PayCollectionEnum.Partial);
        //                                }
        //                                paylatercollection.ModifiedDate = DateTime.Now;
        //                                paylatercollection.ModifiedBy = userid;
        //                                db.Entry(paylatercollection).State = EntityState.Modified;
        //                            }
        //                            #region hit Ladger
        //                            if (AppConstants.IsUsingLedgerHitOnOnlinePayment)
        //                            {

        //                                if (db.OnlinePaymentDtlsForLedgerDB.FirstOrDefault(z => z.OrderId == paylatercollection.OrderId && z.TransactionId == gatewayid) == null)
        //                                {

        //                                    OnlinePaymentDtlsForLedger Opdl = new OnlinePaymentDtlsForLedger();
        //                                    Opdl.OrderId = paylatercollection.OrderId;
        //                                    Opdl.IsPaymentSuccess = true;
        //                                    Opdl.IsLedgerAffected = "Yes";
        //                                    Opdl.PaymentDate = DateTime.Now;
        //                                    Opdl.TransactionId = gatewayid;
        //                                    Opdl.IsActive = true;
        //                                    Opdl.CustomerId = CustomerId;
        //                                    db.OnlinePaymentDtlsForLedgerDB.Add(Opdl);
        //                                }
        //                            }
        //                            #endregion
        //                        }
        //                        else
        //                        {
        //                            return new APIResponse { Status = false, Message = "Something Went Wrong" };
        //                        }
        //                    }
        //                    else
        //                    {
        //                        return new APIResponse { Status = false, Message = "First freeze the assignment and then it will be approved." };
        //                    }
                            

        //                }
        //                #endregion

        //                #region For mpos or RTGS/NEFT or UPI
        //                else if (paylaterhistorydata.PaymentMode == "mPos" || paylaterhistorydata.PaymentMode == "RTGS/NEFT" || paylaterhistorydata.PaymentMode == "UPI")
        //                {
        //                    OnlineCollection payments = JsonConvert.DeserializeObject<OnlineCollection>(paylaterhistorydata.Request);
        //                    var CurrencyHubStock = db.CurrencyHubStock.FirstOrDefault(x => x.Warehouseid == payments.WarehouseId && EntityFunctions.TruncateTime(x.BOD) == EntityFunctions.TruncateTime(DateTime.Now));
        //                    if(CurrencyHubStock != null)
        //                    {
        //                        OnlineCollection Online = new OnlineCollection();
        //                        Online.PaymentGetwayAmt = payments.PaymentGetwayAmt;
        //                        Online.MPOSAmt = payments.MPOSAmt;
        //                        Online.PaymentReferenceNO = payments.PaymentReferenceNO;
        //                        Online.MPOSReferenceNo = payments.MPOSReferenceNo;
        //                        Online.Orderid = payments.Orderid;
        //                        Online.CreatedDate = indianTime;
        //                        Online.CreatedBy = userid;
        //                        Online.CurrencyHubStockId = CurrencyHubStock.Id;
        //                        Online.IsActive = true;
        //                        Online.IsDeleted = false;
        //                        Online.PaymentFrom = payments.PaymentFrom;
        //                        db.OnlineCollection.Add(Online);

        //                        string gatewayid = "SkPayLater" + Id;
        //                        if ((totalpaidamount + paylaterhistorydata.Amount) == paylatercollection.Amount)
        //                        {
        //                            var payorderId = db.OnlineCollection.Where(x => x.Orderid == paylatercollection.OrderId && x.PaymentFrom == "PayLater" && x.IsActive == true && x.IsDeleted == false).FirstOrDefault();
        //                            if (payorderId != null)
        //                            {
        //                                payorderId.IsActive = false;
        //                                payorderId.IsDeleted = true;
        //                                payorderId.ModifiedBy = userid;
        //                                payorderId.ModifiedDate = DateTime.Now;
        //                                db.Entry(payorderId).State = EntityState.Modified;
        //                            }

        //                            paylaterhistorydata.PaymentStatus = 1;
        //                            paylaterhistorydata.ModifiedBy = userid;
        //                            paylaterhistorydata.ModifiedDate = DateTime.Now;
        //                            db.Entry(paylaterhistorydata).State = EntityState.Modified;

        //                            paylatercollection.Status = Convert.ToInt32(PayCollectionEnum.Paid);
        //                            paylatercollection.ModifiedDate = DateTime.Now;
        //                            paylatercollection.ModifiedBy = userid;
        //                            db.Entry(paylatercollection).State = EntityState.Modified;

        //                            #region ordermaster settle 
        //                            CashCollectionNewController ctrl = new CashCollectionNewController();
        //                            bool res = ctrl.OrderSettle(db, paylatercollection.OrderId);
        //                            #endregion
        //                        }
        //                        else
        //                        {
        //                            paylaterhistorydata.PaymentStatus = 1;
        //                            paylaterhistorydata.ModifiedBy = userid;
        //                            paylaterhistorydata.ModifiedDate = DateTime.Now;
        //                            db.Entry(paylaterhistorydata).State = EntityState.Modified;

        //                            if (paylatercollection.Status == 2 || paylatercollection.Status == 3) { }
        //                            else
        //                            {
        //                                paylatercollection.Status = Convert.ToInt32(PayCollectionEnum.Partial);
        //                            }
        //                            paylatercollection.ModifiedDate = DateTime.Now;
        //                            paylatercollection.ModifiedBy = userid;
        //                            db.Entry(paylatercollection).State = EntityState.Modified;
        //                        }
        //                        #region hit Ladger
        //                        if (AppConstants.IsUsingLedgerHitOnOnlinePayment)
        //                        {

        //                            if (db.OnlinePaymentDtlsForLedgerDB.FirstOrDefault(z => z.OrderId == paylatercollection.OrderId && z.TransactionId == gatewayid) == null)
        //                            {

        //                                OnlinePaymentDtlsForLedger Opdl = new OnlinePaymentDtlsForLedger();
        //                                Opdl.OrderId = paylatercollection.OrderId;
        //                                Opdl.IsPaymentSuccess = true;
        //                                Opdl.IsLedgerAffected = "Yes";
        //                                Opdl.PaymentDate = DateTime.Now;
        //                                Opdl.TransactionId = gatewayid;
        //                                Opdl.IsActive = true;
        //                                Opdl.CustomerId = CustomerId;
        //                                db.OnlinePaymentDtlsForLedgerDB.Add(Opdl);
        //                            }
        //                        }
        //                        #endregion
        //                    }
        //                    else
        //                    {
        //                        return new APIResponse { Status = false, Message = "Something Went Wrong" };
        //                    }
        //                }
        //                #endregion

                        

        //                if (db.Commit() > 0)
        //                {
        //                    return new APIResponse { Status = true, Message = "payment Approve Successfully" };
        //                }
        //                else
        //                {
        //                    return new APIResponse { Status = false, Message = "Something Went wrong" };
        //                }
        //            }
        //            else
        //            {
        //                return new APIResponse { Status = false, Message = "Data Not Found" };
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        logger.Error("Error in Approve payment: " + ex.Message);
        //        return new APIResponse { Status = false, Message = ex.Message };
        //    }
        //    return new APIResponse { Status = false, Message = "Something went Wrong!" };
        //}

        [Route("PaymentReject")]
        [HttpGet]
        public APIResponse PaymentReject(long Id)
        {
            try
            {
                var identity = User.Identity as ClaimsIdentity;
                int userid = 0;
                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                    userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
                using (var db = new AuthContext())
                {
                    var data = db.PayLaterCollectionHistoryDb.FirstOrDefault(x => x.Id == Id);
                    if(data != null)
                    {
                        double totalamount = 0;
                        var paylatercollection = db.PayLaterCollectionDb.FirstOrDefault(x => x.Id == data.PayLaterCollectionId);
                        var paylaterhistory = db.PayLaterCollectionHistoryDb.Where(x => x.PayLaterCollectionId == paylatercollection.Id && x.PaymentStatus == 1 && x.IsActive == true && x.IsDeleted == false).ToList();
                        if(paylaterhistory!= null && paylaterhistory.Any())
                        {
                            //totalamount = paylaterhistory.Sum(x => x.Amount);
                            //double paidamount = 0;
                            //double returnamount = 0;
                            //double gullakamount = 0;
                            //paidamount = paylaterhistory.Any(a => a.Comment != "Return Order" && a.Comment != "Gullak Refund") ? paylaterhistory.Where(a => a.Comment != "Return Order" && a.Comment != "Gullak Refund").Sum(y => y.Amount) : 0;
                            //returnamount = paylaterhistory.Any(a => a.Comment == "Return Order") ? paylaterhistory.Where(a => a.Comment == "Return Order").Sum(y => y.Amount) : 0;
                            //gullakamount = paylaterhistory.Any(a => a.Comment == "Gullak Refund") ? paylaterhistory.Where(a => a.Comment == "Gullak Refund").Sum(y => y.Amount) : 0;
                            //totalAmount = totalAmountlist.Sum(x => x.Amount);
                            List<PayLaterAmountUpdateDC> list = new List<PayLaterAmountUpdateDC>();
                            foreach (var item in paylaterhistory)
                            {
                                PayLaterAmountUpdateDC singlepayment = new PayLaterAmountUpdateDC();
                                singlepayment.Amount = item.Amount;
                                singlepayment.Comment = item.Comment;
                                list.Add(singlepayment);
                            }
                            double getamount = 0;
                            getamount = ReturnAmount(list);
                            //totalamount = paylatercollection.Amount - returnamount - paidamount + gullakamount;
                            totalamount =  getamount;
                        }
                        if(totalamount == 0)
                        {
                            if (paylatercollection.Status == 2 || paylatercollection.Status == 3) { }
                            else
                            {
                                paylatercollection.Status = Convert.ToInt32(PayCollectionEnum.Pending);
                            }
                            
                            paylatercollection.ModifiedBy = userid;
                            paylatercollection.ModifiedDate = DateTime.Now;
                            db.Entry(paylatercollection).State = EntityState.Modified;
                        }
                        data.PaymentStatus = 2;
                        data.ModifiedBy = userid;
                        data.ModifiedDate = DateTime.Now;
                        db.Entry(data).State = EntityState.Modified;
                        if(db.Commit() > 0)
                        {
                            return new APIResponse { Status = true, Message = "Payment Rejected Successfully" };
                        }
                        else
                        {
                            return new APIResponse { Status = false, Message = "Something Went Wrong" };
                        }
                    }
                    else
                    {
                        return new APIResponse { Status = false, Message = "Data Not Found" };
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error("Error in Approve payment: " + ex.Message);
                return new APIResponse { Status = false, Message = ex.Message };
            }
            return new APIResponse { Status = false, Message = "Something went Wrong!" };
        }

        public double ReturnAmount(List<PayLaterAmountUpdateDC> payLaterAmountUpdateDCs)
        {
            double returnamounts = 0;
            double paidamount = 0;
            double returnamount = 0;
            double gullakamount = 0;
            paidamount = payLaterAmountUpdateDCs.Any(a => a.Comment != "Return Order" && a.Comment != "Gullak Refund") ? payLaterAmountUpdateDCs.Where(a => a.Comment != "Return Order" && a.Comment != "Gullak Refund").Sum(y => y.Amount) : 0;
            returnamount = payLaterAmountUpdateDCs.Any(a => a.Comment == "Return Order") ? payLaterAmountUpdateDCs.Where(a => a.Comment == "Return Order").Sum(y => y.Amount) : 0;
            gullakamount = payLaterAmountUpdateDCs.Any(a => a.Comment == "Gullak Refund") ? payLaterAmountUpdateDCs.Where(a => a.Comment == "Gullak Refund").Sum(y => y.Amount) : 0;
            returnamounts = paidamount+returnamount-gullakamount;
            if (returnamounts < 0)
            {
                returnamounts = 0;
            }
            return returnamounts;
        }

        [Route("ManuallyUpdateOrder")]
        [HttpPost]
        public bool ManuallyUpdateOrder(int OrderId,string RefNo,double Amount,string mode,string comment)
        {
            using (var db = new AuthContext())
            {
                bool res = false;
                long customerid = 0;
                var paylatercollection = db.PayLaterCollectionDb.Where(x => x.OrderId == OrderId && x.Status != 4 && x.IsActive==true && x.IsDeleted == false).FirstOrDefault();
                if(paylatercollection != null)
                {
                    customerid = db.DbOrderMaster.FirstOrDefault(x => x.OrderId == OrderId).CustomerId;
                    PayLaterCollectionHistory his = new PayLaterCollectionHistory();
                    his.Amount = Amount;
                    his.RefNo = RefNo;
                    his.PayLaterCollectionId = paylatercollection.Id;
                    his.CreatedBy = 1;
                    his.CreatedDate = DateTime.Now;
                    his.IsActive = true;
                    his.IsDeleted = false;
                    his.Comment = comment;
                    his.PaymentStatus = 1;
                    his.PaymentMode = mode;
                    db.PayLaterCollectionHistoryDb.Add(his);
                    paylatercollection.Status = Convert.ToInt32(PayCollectionEnum.Paid);
                    paylatercollection.ModifiedDate = DateTime.Now;
                    paylatercollection.ModifiedBy = 1;
                    res = true;
                    if (db.Commit() > 0)
                    {
                        string gatewayid = "SkPayLater" + his.Id;
                        if (AppConstants.IsUsingLedgerHitOnOnlinePayment)
                        {
                            if (db.OnlinePaymentDtlsForLedgerDB.FirstOrDefault(z => z.OrderId == paylatercollection.OrderId && z.TransactionId == gatewayid) == null)
                            {

                                OnlinePaymentDtlsForLedger Opdl = new OnlinePaymentDtlsForLedger();
                                Opdl.OrderId = paylatercollection.OrderId;
                                Opdl.IsPaymentSuccess = true;
                                Opdl.IsLedgerAffected = "Yes";
                                Opdl.PaymentDate = DateTime.Now;
                                Opdl.TransactionId = gatewayid;
                                Opdl.IsActive = true;
                                Opdl.CustomerId = customerid;
                                db.OnlinePaymentDtlsForLedgerDB.Add(Opdl);
                                db.Commit();
                            }
                        }


                    }
                }
                return res;
            }
        }

        //[Route("ChequePaymentAddPayLater")]
        //[HttpPost]
        //public APIResponse ChequePaymentAddPayLater(ChequePaymentDC ChequeList)
        //{
        //    try
        //    {
        //        var identity = User.Identity as ClaimsIdentity;
        //        int userid = 0;
        //        if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
        //            userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

        //        using (AuthContext db = new AuthContext())
        //        {
        //            var payLaterCollection = db.PayLaterCollectionDb.FirstOrDefault(x => x.OrderId == ChequeList.OrderId
        //                                                           && x.IsActive == true
        //                                                           && x.IsDeleted == false);
        //            if (ChequeList.BankId == 0)
        //            {
        //                return new APIResponse { Status = false, Message = "Please Select Bank " };
        //            }
        //            if (payLaterCollection != null)
        //            {
        //                var History = db.PayLaterCollectionHistoryDb.Where(x => x.PayLaterCollectionId == payLaterCollection.Id
        //                                                                && x.IsActive == true
        //                                                                 && x.IsDeleted == false && (x.PaymentStatus == 0 || x.PaymentStatus == 1)).ToList();
        //                var orderdispatchmaster = db.OrderDispatchedMasters.Where(x => x.OrderId == payLaterCollection.OrderId).FirstOrDefault();
        //                var ordermaster = db.DbOrderMaster.Where(x => x.OrderId == payLaterCollection.OrderId).FirstOrDefault();
        //                double totalPaidAmount = 0;
        //                if (History != null && History.Count > 0)
        //                {
        //                    //totalPaidAmount = History.Sum(x => x.Amount);
        //                    //double paidamount = 0;
        //                    //double returnamount = 0;
        //                    //double gullakamount = 0;
        //                    //paidamount = History.Any(a => a.Comment != "Return Order" && a.Comment != "Gullak Refund") ? History.Where(a => a.Comment != "Return Order" && a.Comment != "Gullak Refund").Sum(y => y.Amount) : 0;
        //                    //returnamount = History.Any(a => a.Comment == "Return Order") ? History.Where(a => a.Comment == "Return Order").Sum(y => y.Amount) : 0;
        //                    //gullakamount = History.Any(a => a.Comment == "Gullak Refund") ? History.Where(a => a.Comment == "Gullak Refund").Sum(y => y.Amount) : 0;
        //                    //totalAmount = totalAmountlist.Sum(x => x.Amount);
        //                    //totalPaidAmount = payLaterCollection.Amount - returnamount - paidamount + gullakamount;
        //                    List<PayLaterAmountUpdateDC> list = new List<PayLaterAmountUpdateDC>();
        //                    foreach (var item in History)
        //                    {
        //                        PayLaterAmountUpdateDC singlepayment = new PayLaterAmountUpdateDC();
        //                        singlepayment.Amount = item.Amount;
        //                        singlepayment.Comment = item.Comment;
        //                        list.Add(singlepayment);
        //                    }
        //                    double getamount = 0;
        //                    getamount = ReturnAmount(list);
        //                    totalPaidAmount = getamount;

        //                }
        //                if (payLaterCollection.Amount < totalPaidAmount + Convert.ToDouble(ChequeList.ChequeAmt))
        //                {
        //                    return new APIResponse { Status = false, Message = "Please Refresh Page" };
        //                }
        //                People People = db.Peoples.Where(x => x.PeopleID == userid).FirstOrDefault();
        //                var CurrencyHubStock = db.CurrencyHubStock.FirstOrDefault(x => x.Warehouseid == ChequeList.Warehouseid && EntityFunctions.TruncateTime(x.BOD) == EntityFunctions.TruncateTime(DateTime.Now));
        //                var BankName = db.CurrencySettlementBank.Where(x => x.Id == ChequeList.BankId).Select(x => x.BankName).FirstOrDefault();
        //                //ChequeCollection cheque = db.ChequeCollection.Where(y => y.CurrencyCollectionId).FirstOrDefault();
        //                //ReturnChequeCollection ReturnChequedata = db.ReturnChequeCollection.Where(x => x.Id == ChequeList.Id).FirstOrDefault();
        //                //var GetCollectionId = db.ChequeCollection.Where(x => x.Id == ReturnChequedata.ChequeCollectionId).Select(x => x.CurrencyCollectionId).FirstOrDefault();
        //                if (CurrencyHubStock != null)
        //                {
        //                    CurrencyCollection currencyCollection = new CurrencyCollection();
        //                    int deliveryissueid = orderdispatchmaster != null && orderdispatchmaster.DeliveryIssuanceIdOrderDeliveryMaster > 0 ? Convert.ToInt32(orderdispatchmaster.DeliveryIssuanceIdOrderDeliveryMaster) : 0;
        //                    if (deliveryissueid > 0)
        //                    {
        //                        currencyCollection = db.CurrencyCollection.Where(x => x.Warehouseid == ordermaster.WarehouseId && x.Deliveryissueid == deliveryissueid).FirstOrDefault();
        //                        if(currencyCollection == null)
        //                        {
        //                            return new APIResponse { Status = false, Message = "Assignment Not Settled" };
        //                        }
        //                    }
        //                    ChequeCollection Payment = new ChequeCollection();
        //                    Payment.ChequeStatus = ChequeList.Status = 1;
        //                    Payment.ChequeAmt = ChequeList.ChequeAmt;
        //                    Payment.ChequeBankName = BankName;
        //                    Payment.ChequeNumber = ChequeList.ChequeNumber;
        //                    Payment.WarehouseId = ChequeList.Warehouseid;
        //                    Payment.Orderid = ChequeList.OrderId;
        //                    Payment.ChequeDate = ChequeList.ChequeDate;
        //                    Payment.CreatedDate = indianTime;
        //                    Payment.CreatedBy = userid;
        //                    Payment.CurrencyHubStockId = CurrencyHubStock.Id;
        //                    Payment.IsActive = true;
        //                    Payment.IsDeleted = false;
        //                    Payment.ChequeimagePath = ChequeList.ChequeimagePath;
        //                    //Payment.CurrencySettlementSourceId = ChequeList.BankId;
        //                    Payment.CurrencyCollectionId = currencyCollection != null ? currencyCollection.Id : 0;
        //                    Payment.DBoyPeopleId = orderdispatchmaster != null && orderdispatchmaster.DBoyId > 0 ? orderdispatchmaster.DBoyId : 0;
        //                    db.ChequeCollection.Add(Payment);
        //                    if (db.Commit() > 0)
        //                    {
        //                        string request = Convert.ToString(Payment.Id);
        //                        if ((totalPaidAmount + Convert.ToDouble(ChequeList.ChequeAmt)) <= payLaterCollection.Amount)
        //                        {
        //                            //var payorderId = db.OnlineCollection.Where(x => x.Orderid == ChequeList.OrderId && x.PaymentFrom == "PayLater" && x.IsActive == true && x.IsDeleted == false).FirstOrDefault();
        //                            //if (payorderId != null)
        //                            //{
        //                            //    payorderId.IsActive = false;
        //                            //    payorderId.IsDeleted = true;
        //                            //    payorderId.ModifiedBy = userid;
        //                            //    payorderId.ModifiedDate = DateTime.Now;
        //                            //    db.Entry(payorderId).State = EntityState.Modified;
        //                            //}
        //                            db.PayLaterCollectionHistoryDb.Add(new PayLaterCollectionHistory
        //                            {
        //                                Amount = Convert.ToDouble(ChequeList.ChequeAmt),
        //                                RefNo = ChequeList.ChequeNumber,
        //                                CreatedDate = DateTime.Now,
        //                                PaymentMode = "Cheque",
        //                                CurrencyHubStockId = CurrencyHubStock.Id,
        //                                IsActive = true,
        //                                IsDeleted = false,
        //                                CreatedBy = userid,
        //                                PayLaterCollectionId = payLaterCollection.Id,
        //                                Request = request
        //                                //statusDesc = "New Cash Added on Behalf of Bounce Cheque",
        //                            });
        //                        }
        //                    }

        //                }
        //            }
        //            else
        //                return new APIResponse { Status = false, Message = "Data Not Found" };

        //            if (db.Commit() > 0)
        //                return new APIResponse { Status = true, Message = "cheque Added Successfully" };
        //            else
        //                return new APIResponse { Status = false, Message = "Something Went Wrong!" };
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        logger.Error("Error in Settel: " + ex.Message);
        //        return new APIResponse { Status = false, Message = ex.Message };
        //    }
        //}

        [Route("PaymentApprove")]
        [HttpGet]
        public APIResponse PaymentApprove(long Id)
        {
            try
            {
                var identity = User.Identity as ClaimsIdentity;
                int userid = 0;
                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                    userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
                using (var db = new AuthContext())
                {
                    var paylaterhistorydata = db.PayLaterCollectionHistoryDb.FirstOrDefault(x => x.Id == Id && x.IsActive == true && x.IsDeleted == false);
                    if (paylaterhistorydata != null)
                    {
                        double totalpaidamount = 0;
                        int CustomerId = 0;
                        var paylatercollection = db.PayLaterCollectionDb.FirstOrDefault(x => x.Id == paylaterhistorydata.PayLaterCollectionId && x.IsActive == true && x.IsDeleted == false);
                        var paylatercollectionhistory = db.PayLaterCollectionHistoryDb.Where(x => x.PayLaterCollectionId == paylaterhistorydata.PayLaterCollectionId && x.IsActive == true && x.IsDeleted == false && x.PaymentStatus == 1).ToList();
                        //var orderdispatchmaster = db.OrderDispatchedMasters.Where(x => x.OrderId == paylatercollection.OrderId).FirstOrDefault();
                        CustomerId = db.DbOrderMaster.FirstOrDefault(x => x.OrderId == paylatercollection.OrderId).CustomerId;
                        if (paylatercollectionhistory != null && paylatercollectionhistory.Any())
                        {
                            //totalpaidamount = paylatercollectionhistory.Sum(x => x.Amount);
                            //double paidamount = 0;
                            //double returnamount = 0;
                            //double gullakamount = 0;
                            //paidamount = paylatercollectionhistory.Any(a => a.Comment != "Return Order" && a.Comment != "Gullak Refund") ? paylatercollectionhistory.Where(a => a.Comment != "Return Order" && a.Comment != "Gullak Refund").Sum(y => y.Amount) : 0;
                            //returnamount = paylatercollectionhistory.Any(a => a.Comment == "Return Order") ? paylatercollectionhistory.Where(a => a.Comment == "Return Order").Sum(y => y.Amount) : 0;
                            //gullakamount = paylatercollectionhistory.Any(a => a.Comment == "Gullak Refund") ? paylatercollectionhistory.Where(a => a.Comment == "Gullak Refund").Sum(y => y.Amount) : 0;
                            //totalAmount = totalAmountlist.Sum(x => x.Amount);
                            List<PayLaterAmountUpdateDC> list = new List<PayLaterAmountUpdateDC>();
                            foreach (var item in paylatercollectionhistory)
                            {
                                PayLaterAmountUpdateDC singlepayment = new PayLaterAmountUpdateDC();
                                singlepayment.Amount = item.Amount;
                                singlepayment.Comment = item.Comment;
                                list.Add(singlepayment);
                            }
                            double getamount = 0;
                            getamount = ReturnAmount(list);
                            //totalpaidamount = paylatercollection.Amount - returnamount - paidamount + gullakamount;
                            totalpaidamount = getamount;
                        }

                        if ((totalpaidamount + paylaterhistorydata.Amount) > paylatercollection.Amount)
                        {
                            return new APIResponse { Status = false, Message = "Amount is greater then Total Amount" };
                        }

                        #region for cash
                        if (paylaterhistorydata.PaymentMode == "Cash")
                        {
                            CashdataDc cashData = JsonConvert.DeserializeObject<CashdataDc>(paylaterhistorydata.Request);
                            var CurrencyHubStock = db.CurrencyHubStock.FirstOrDefault(x => x.Warehouseid == cashData.WarehouseId && EntityFunctions.TruncateTime(x.BOD) == EntityFunctions.TruncateTime(DateTime.Now));
                            if (CurrencyHubStock != null)
                            {
                                var denomination = db.CurrencyDenomination.Where(x => x.IsActive).ToList();
                                List<CashCollection> CashCollections = new List<CashCollection>();

                                var denominationids = cashData.cashList.Where(x => x.CurrencyCount > 0).Select(x => x.Id).ToList();
                                //var cashCollections = db.CashCollection.Where(x => x.CurrencyHubStockId == CurrencyHubStock.Id && denominationids.Contains(x.CurrencyDenominationId));
                                //foreach (var cash in cashCollections)
                                //{
                                //    cash.CurrencyCountByDBoy = cashData.cashList.Any(x => x.Id == cash.CurrencyDenominationId) ? cash.CurrencyCountByDBoy + cashData.cashList.FirstOrDefault(x => x.Id == cash.CurrencyDenominationId).CurrencyCount : cash.CurrencyCountByDBoy;
                                //    cash.ModifiedBy = userid;
                                //    cash.ModifiedDate = indianTime;
                                //    db.Entry(cash).State = EntityState.Modified;
                                //}

                                List<HubCashCollection> hubCashCollections = db.HubCashCollection.Where(x => x.IsDeleted == false && x.CurrencyHubStockId == CurrencyHubStock.Id && denominationids.Contains(x.CurrencyDenominationId)).ToList();
                                if (hubCashCollections != null && hubCashCollections.Any())
                                {
                                    foreach (var hubCashCollection in hubCashCollections)
                                    {
                                        hubCashCollection.CurrencyCount = cashData.cashList.Any(x => x.Id == hubCashCollection.CurrencyDenominationId) ? hubCashCollection.CurrencyCount + cashData.cashList.FirstOrDefault(x => x.Id == hubCashCollection.CurrencyDenominationId).CurrencyCount : hubCashCollection.CurrencyCount;
                                        hubCashCollection.ModifiedBy = userid;
                                        db.Entry(hubCashCollection).State = EntityState.Modified;
                                    }
                                }

                                string gatewayid = "SkPayLater" + Id;
                                if ((totalpaidamount + paylaterhistorydata.Amount) == paylatercollection.Amount)
                                {
                                    var payorderId = db.OnlineCollection.Where(x => x.Orderid == paylatercollection.OrderId && x.PaymentFrom == "PayLater" && x.IsActive == true && x.IsDeleted == false).FirstOrDefault();
                                    if (payorderId != null)
                                    {
                                        payorderId.IsActive = false;
                                        payorderId.IsDeleted = true;
                                        payorderId.ModifiedBy = userid;
                                        payorderId.ModifiedDate = DateTime.Now;
                                        db.Entry(payorderId).State = EntityState.Modified;
                                    }

                                    paylaterhistorydata.PaymentStatus = 1;
                                    paylaterhistorydata.ModifiedBy = userid;
                                    paylaterhistorydata.ModifiedDate = DateTime.Now;
                                    db.Entry(paylaterhistorydata).State = EntityState.Modified;

                                    paylatercollection.Status = Convert.ToInt32(PayCollectionEnum.Paid);
                                    paylatercollection.ModifiedDate = DateTime.Now;
                                    paylatercollection.ModifiedBy = userid;
                                    db.Entry(paylatercollection).State = EntityState.Modified;

                                    #region ordermaster settle 
                                    CashCollectionNewController ctrl = new CashCollectionNewController();
                                    bool res = ctrl.OrderSettle(db, paylatercollection.OrderId);
                                    #endregion
                                }
                                else
                                {
                                    paylaterhistorydata.PaymentStatus = 1;
                                    paylaterhistorydata.ModifiedBy = userid;
                                    paylaterhistorydata.ModifiedDate = DateTime.Now;
                                    db.Entry(paylaterhistorydata).State = EntityState.Modified;
                                    if (paylatercollection.Status == 2 || paylatercollection.Status == 3) { }
                                    else
                                    {
                                        paylatercollection.Status = Convert.ToInt32(PayCollectionEnum.Partial);
                                    }
                                    paylatercollection.ModifiedDate = DateTime.Now;
                                    paylatercollection.ModifiedBy = userid;
                                    db.Entry(paylatercollection).State = EntityState.Modified;
                                }
                                #region hit Ladger
                                if (AppConstants.IsUsingLedgerHitOnOnlinePayment)
                                {

                                    if (db.OnlinePaymentDtlsForLedgerDB.FirstOrDefault(z => z.OrderId == paylatercollection.OrderId && z.TransactionId == gatewayid) == null)
                                    {

                                        OnlinePaymentDtlsForLedger Opdl = new OnlinePaymentDtlsForLedger();
                                        Opdl.OrderId = paylatercollection.OrderId;
                                        Opdl.IsPaymentSuccess = true;
                                        Opdl.IsLedgerAffected = "Yes";
                                        Opdl.PaymentDate = DateTime.Now;
                                        Opdl.TransactionId = gatewayid;
                                        Opdl.IsActive = true;
                                        Opdl.CustomerId = CustomerId;
                                        db.OnlinePaymentDtlsForLedgerDB.Add(Opdl);
                                    }
                                }
                                #endregion
                            }
                            else
                            {
                                return new APIResponse { Status = false, Message = "Something Went Wrong" };
                            }
                        }
                        #endregion

                        //#region For Cheque
                        //else if (paylaterhistorydata.PaymentMode == "Cheque")
                        //{
                        //    CurrencyCollection currencyCollection = new CurrencyCollection();
                        //    int deliveryissueid = orderdispatchmaster != null && orderdispatchmaster.DeliveryIssuanceIdOrderDeliveryMaster >0 ? Convert.ToInt32(orderdispatchmaster.DeliveryIssuanceIdOrderDeliveryMaster) : 0;
                        //    if(deliveryissueid > 0)
                        //    {
                        //        currencyCollection = db.CurrencyCollection.Where(x => x.Deliveryissueid == deliveryissueid).FirstOrDefault();
                        //    }
                        //    if(currencyCollection != null)
                        //    {
                        //    ChequeCollection Payment = JsonConvert.DeserializeObject<ChequeCollection>(paylaterhistorydata.Request);
                        //    var CurrencyHubStock = db.CurrencyHubStock.FirstOrDefault(x => x.Warehouseid == Payment.WarehouseId && EntityFunctions.TruncateTime(x.BOD) == EntityFunctions.TruncateTime(DateTime.Now));
                        //    if (CurrencyHubStock != null)
                        //    {
                        //    ChequeCollection Payments = new ChequeCollection();
                        //    Payments.ChequeStatus = Payment.ChequeStatus;
                        //    Payments.ChequeAmt = Payment.ChequeAmt;
                        //    Payments.ChequeBankName = Payment.ChequeBankName;
                        //    Payments.ChequeNumber = Payment.ChequeNumber;
                        //    Payments.WarehouseId = Payment.WarehouseId;
                        //    Payments.Orderid = Payment.Orderid;
                        //    Payments.ChequeDate = Payment.ChequeDate;
                        //    Payments.CreatedDate = indianTime;
                        //    Payments.CreatedBy = userid;
                        //    Payments.CurrencyHubStockId = CurrencyHubStock.Id;
                        //    Payments.IsActive = true;
                        //    Payments.IsDeleted = false;
                        //    Payments.ChequeimagePath = Payment.ChequeimagePath;
                        //    Payments.CurrencyCollectionId = currencyCollection != null ? currencyCollection.Id : 0;
                        //    Payments.DBoyPeopleId = orderdispatchmaster != null && orderdispatchmaster.DBoyId > 0 ? orderdispatchmaster.DBoyId : 0;

                        //    db.ChequeCollection.Add(Payments);
                        //    //long chequeid = Convert.ToInt64(paylaterhistorydata.Request);
                        //    //if (chequeid > 0)
                        //    //{
                        //    //    var chequedata = db.ChequeCollection.Where(x => x.Id == chequeid).FirstOrDefault();
                        //    //    if (chequedata != null)
                        //    //    {
                        //    //        if (chequedata.ChequeStatus == 3)
                        //    //        {
                        //    //            string gatewayid = "SkPayLater" + Id;
                        //    //            if ((totalpaidamount + paylaterhistorydata.Amount) == paylatercollection.Amount)
                        //    //            {
                        //    //                var payorderId = db.OnlineCollection.Where(x => x.Orderid == paylatercollection.OrderId && x.PaymentFrom == "PayLater" && x.IsActive == true && x.IsDeleted == false).FirstOrDefault();
                        //    //                if (payorderId != null)
                        //    //                {
                        //    //                    payorderId.IsActive = false;
                        //    //                    payorderId.IsDeleted = true;
                        //    //                    payorderId.ModifiedBy = userid;
                        //    //                    payorderId.ModifiedDate = DateTime.Now;
                        //    //                    db.Entry(payorderId).State = EntityState.Modified;
                        //    //                }

                        //    //                paylaterhistorydata.PaymentStatus = 1;
                        //    //                paylaterhistorydata.ModifiedBy = userid;
                        //    //                paylaterhistorydata.ModifiedDate = DateTime.Now;
                        //    //                db.Entry(paylaterhistorydata).State = EntityState.Modified;

                        //    //                paylatercollection.Status = Convert.ToInt32(PayCollectionEnum.Paid);
                        //    //                paylatercollection.ModifiedDate = DateTime.Now;
                        //    //                paylatercollection.ModifiedBy = userid;
                        //    //                db.Entry(paylatercollection).State = EntityState.Modified;

                        //    //                #region ordermaster settle 
                        //    //                CashCollectionNewController ctrl = new CashCollectionNewController();
                        //    //                bool res = ctrl.OrderSettle(db, paylatercollection.OrderId);
                        //    //                #endregion
                        //    //            }
                        //    //            else
                        //    //            {
                        //    //                paylaterhistorydata.PaymentStatus = 1;
                        //    //                paylaterhistorydata.ModifiedBy = userid;
                        //    //                paylaterhistorydata.ModifiedDate = DateTime.Now;
                        //    //                db.Entry(paylaterhistorydata).State = EntityState.Modified;

                        //    //                if (paylatercollection.Status == 2 || paylatercollection.Status == 3) { }
                        //    //                else
                        //    //                {
                        //    //                    paylatercollection.Status = Convert.ToInt32(PayCollectionEnum.Partial);
                        //    //                }
                        //    //                paylatercollection.ModifiedDate = DateTime.Now;
                        //    //                paylatercollection.ModifiedBy = userid;
                        //    //                db.Entry(paylatercollection).State = EntityState.Modified;
                        //    //            }
                        //    //            #region hit Ladger
                        //    //            if (AppConstants.IsUsingLedgerHitOnOnlinePayment)
                        //    //            {

                        //    //                if (db.OnlinePaymentDtlsForLedgerDB.FirstOrDefault(z => z.OrderId == paylatercollection.OrderId && z.TransactionId == gatewayid) == null)
                        //    //                {

                        //    //                    OnlinePaymentDtlsForLedger Opdl = new OnlinePaymentDtlsForLedger();
                        //    //                    Opdl.OrderId = paylatercollection.OrderId;
                        //    //                    Opdl.IsPaymentSuccess = true;
                        //    //                    Opdl.IsLedgerAffected = "Yes";
                        //    //                    Opdl.PaymentDate = DateTime.Now;
                        //    //                    Opdl.TransactionId = gatewayid;
                        //    //                    Opdl.IsActive = true;
                        //    //                    Opdl.CustomerId = CustomerId;
                        //    //                    db.OnlinePaymentDtlsForLedgerDB.Add(Opdl);
                        //    //                }
                        //    //            }
                        //    //            #endregion
                        //    //        }
                        //    //        else
                        //    //        {
                        //    //            return new APIResponse { Status = false, Message = "Cheque Not Clear" };
                        //    //        }

                        //    //    }
                        //    //    else
                        //    //    {
                        //    //        return new APIResponse { Status = false, Message = "Data Not Found" };
                        //    //    }
                        //    //}
                        //    //else
                        //    //{
                        //    //    return new APIResponse { Status = false, Message = "Cheque Not Found" };
                        //    //}


                        //    //}
                        //    //else
                        //    //{
                        //    //    return new APIResponse { Status = false, Message = "Something Went Wrong" };
                        //    //}
                        //    //}
                        //    //else
                        //    //{
                        //    //    return new APIResponse { Status = false, Message = "First freeze the assignment and then it will be approved." };
                        //    //}


                        //}
                        //#endregion

                        #region For Cheque
                        else if (paylaterhistorydata.PaymentMode == "Cheque")
                        {
                            ChequeCollection Payment = JsonConvert.DeserializeObject<ChequeCollection>(paylaterhistorydata.Request);
                            var CurrencyHubStock = db.CurrencyHubStock.FirstOrDefault(x => x.Warehouseid == Payment.WarehouseId && EntityFunctions.TruncateTime(x.BOD) == EntityFunctions.TruncateTime(DateTime.Now));
                            if (CurrencyHubStock != null)
                            {
                                ChequeCollection Payments = new ChequeCollection();
                                Payments.ChequeStatus = Payment.ChequeStatus;
                                Payments.ChequeAmt = Payment.ChequeAmt;
                                Payments.ChequeBankName = Payment.ChequeBankName;
                                Payments.ChequeNumber = Payment.ChequeNumber;
                                Payments.WarehouseId = Payment.WarehouseId;
                                Payments.Orderid = Payment.Orderid;
                                Payments.ChequeDate = Payment.ChequeDate;
                                Payments.CreatedDate = indianTime;
                                Payments.CreatedBy = userid;
                                Payments.CurrencyHubStockId = CurrencyHubStock.Id;
                                Payments.IsActive = true;
                                Payments.IsDeleted = false;
                                Payments.ChequeimagePath = Payment.ChequeimagePath;
                                db.ChequeCollection.Add(Payments);

                                string gatewayid = "SkPayLater" + Id;
                                if ((totalpaidamount + paylaterhistorydata.Amount) == paylatercollection.Amount)
                                {
                                    var payorderId = db.OnlineCollection.Where(x => x.Orderid == paylatercollection.OrderId && x.PaymentFrom == "PayLater" && x.IsActive == true && x.IsDeleted == false).FirstOrDefault();
                                    if (payorderId != null)
                                    {
                                        payorderId.IsActive = false;
                                        payorderId.IsDeleted = true;
                                        payorderId.ModifiedBy = userid;
                                        payorderId.ModifiedDate = DateTime.Now;
                                        db.Entry(payorderId).State = EntityState.Modified;
                                    }

                                    paylaterhistorydata.PaymentStatus = 1;
                                    paylaterhistorydata.ModifiedBy = userid;
                                    paylaterhistorydata.ModifiedDate = DateTime.Now;
                                    db.Entry(paylaterhistorydata).State = EntityState.Modified;

                                    paylatercollection.Status = Convert.ToInt32(PayCollectionEnum.Paid);
                                    paylatercollection.ModifiedDate = DateTime.Now;
                                    paylatercollection.ModifiedBy = userid;
                                    db.Entry(paylatercollection).State = EntityState.Modified;

                                    #region ordermaster settle 
                                    CashCollectionNewController ctrl = new CashCollectionNewController();
                                    bool res = ctrl.OrderSettle(db, paylatercollection.OrderId);
                                    #endregion
                                }
                                else
                                {
                                    paylaterhistorydata.PaymentStatus = 1;
                                    paylaterhistorydata.ModifiedBy = userid;
                                    paylaterhistorydata.ModifiedDate = DateTime.Now;
                                    db.Entry(paylaterhistorydata).State = EntityState.Modified;

                                    if (paylatercollection.Status == 2 || paylatercollection.Status == 3) { }
                                    else
                                    {
                                        paylatercollection.Status = Convert.ToInt32(PayCollectionEnum.Partial);
                                    }
                                    paylatercollection.ModifiedDate = DateTime.Now;
                                    paylatercollection.ModifiedBy = userid;
                                    db.Entry(paylatercollection).State = EntityState.Modified;
                                }
                                #region hit Ladger
                                if (AppConstants.IsUsingLedgerHitOnOnlinePayment)
                                {

                                    if (db.OnlinePaymentDtlsForLedgerDB.FirstOrDefault(z => z.OrderId == paylatercollection.OrderId && z.TransactionId == gatewayid) == null)
                                    {

                                        OnlinePaymentDtlsForLedger Opdl = new OnlinePaymentDtlsForLedger();
                                        Opdl.OrderId = paylatercollection.OrderId;
                                        Opdl.IsPaymentSuccess = true;
                                        Opdl.IsLedgerAffected = "Yes";
                                        Opdl.PaymentDate = DateTime.Now;
                                        Opdl.TransactionId = gatewayid;
                                        Opdl.IsActive = true;
                                        Opdl.CustomerId = CustomerId;
                                        db.OnlinePaymentDtlsForLedgerDB.Add(Opdl);
                                    }
                                }
                                #endregion
                            }
                            else
                            {
                                return new APIResponse { Status = false, Message = "Something Went Wrong" };
                            }

                        }
                        #endregion

                        #region For mpos or RTGS/NEFT or UPI
                        else if (paylaterhistorydata.PaymentMode == "mPos" || paylaterhistorydata.PaymentMode == "RTGS/NEFT" || paylaterhistorydata.PaymentMode == "UPI")
                        {
                            OnlineCollection payments = JsonConvert.DeserializeObject<OnlineCollection>(paylaterhistorydata.Request);
                            var CurrencyHubStock = db.CurrencyHubStock.FirstOrDefault(x => x.Warehouseid == payments.WarehouseId && EntityFunctions.TruncateTime(x.BOD) == EntityFunctions.TruncateTime(DateTime.Now));
                            if (CurrencyHubStock != null)
                            {
                                OnlineCollection Online = new OnlineCollection();
                                Online.PaymentGetwayAmt = payments.PaymentGetwayAmt;
                                Online.MPOSAmt = payments.MPOSAmt;
                                Online.PaymentReferenceNO = payments.PaymentReferenceNO;
                                Online.MPOSReferenceNo = payments.MPOSReferenceNo;
                                Online.Orderid = payments.Orderid;
                                Online.CreatedDate = indianTime;
                                Online.CreatedBy = userid;
                                Online.CurrencyHubStockId = CurrencyHubStock.Id;
                                Online.IsActive = true;
                                Online.IsDeleted = false;
                                Online.PaymentFrom = payments.PaymentFrom;
                                db.OnlineCollection.Add(Online);

                                string gatewayid = "SkPayLater" + Id;
                                if ((totalpaidamount + paylaterhistorydata.Amount) == paylatercollection.Amount)
                                {
                                    var payorderId = db.OnlineCollection.Where(x => x.Orderid == paylatercollection.OrderId && x.PaymentFrom == "PayLater" && x.IsActive == true && x.IsDeleted == false).FirstOrDefault();
                                    if (payorderId != null)
                                    {
                                        payorderId.IsActive = false;
                                        payorderId.IsDeleted = true;
                                        payorderId.ModifiedBy = userid;
                                        payorderId.ModifiedDate = DateTime.Now;
                                        db.Entry(payorderId).State = EntityState.Modified;
                                    }

                                    paylaterhistorydata.PaymentStatus = 1;
                                    paylaterhistorydata.ModifiedBy = userid;
                                    paylaterhistorydata.ModifiedDate = DateTime.Now;
                                    db.Entry(paylaterhistorydata).State = EntityState.Modified;

                                    paylatercollection.Status = Convert.ToInt32(PayCollectionEnum.Paid);
                                    paylatercollection.ModifiedDate = DateTime.Now;
                                    paylatercollection.ModifiedBy = userid;
                                    db.Entry(paylatercollection).State = EntityState.Modified;

                                    #region ordermaster settle 
                                    CashCollectionNewController ctrl = new CashCollectionNewController();
                                    bool res = ctrl.OrderSettle(db, paylatercollection.OrderId);
                                    #endregion
                                }
                                else
                                {
                                    paylaterhistorydata.PaymentStatus = 1;
                                    paylaterhistorydata.ModifiedBy = userid;
                                    paylaterhistorydata.ModifiedDate = DateTime.Now;
                                    db.Entry(paylaterhistorydata).State = EntityState.Modified;

                                    if (paylatercollection.Status == 2 || paylatercollection.Status == 3) { }
                                    else
                                    {
                                        paylatercollection.Status = Convert.ToInt32(PayCollectionEnum.Partial);
                                    }
                                    paylatercollection.ModifiedDate = DateTime.Now;
                                    paylatercollection.ModifiedBy = userid;
                                    db.Entry(paylatercollection).State = EntityState.Modified;
                                }
                                #region hit Ladger
                                if (AppConstants.IsUsingLedgerHitOnOnlinePayment)
                                {

                                    if (db.OnlinePaymentDtlsForLedgerDB.FirstOrDefault(z => z.OrderId == paylatercollection.OrderId && z.TransactionId == gatewayid) == null)
                                    {

                                        OnlinePaymentDtlsForLedger Opdl = new OnlinePaymentDtlsForLedger();
                                        Opdl.OrderId = paylatercollection.OrderId;
                                        Opdl.IsPaymentSuccess = true;
                                        Opdl.IsLedgerAffected = "Yes";
                                        Opdl.PaymentDate = DateTime.Now;
                                        Opdl.TransactionId = gatewayid;
                                        Opdl.IsActive = true;
                                        Opdl.CustomerId = CustomerId;
                                        db.OnlinePaymentDtlsForLedgerDB.Add(Opdl);
                                    }
                                }
                                #endregion
                            }
                            else
                            {
                                return new APIResponse { Status = false, Message = "Something Went Wrong" };
                            }
                        }
                        #endregion



                        if (db.Commit() > 0)
                        {
                            return new APIResponse { Status = true, Message = "payment Approve Successfully" };
                        }
                        else
                        {
                            return new APIResponse { Status = false, Message = "Something Went wrong" };
                        }
                    }
                    else
                    {
                        return new APIResponse { Status = false, Message = "Data Not Found" };
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error("Error in Approve payment: " + ex.Message);
                return new APIResponse { Status = false, Message = ex.Message };
            }
            return new APIResponse { Status = false, Message = "Something went Wrong!" };
        }

    }
}
/*public class CMSReportDatadc
{
   public int Warehouseid { get; set; }
   public string WarehouseName { get; set; }
   public DateTime BOD { get; set; }
   public int opening { get; set; }
   public int ExchangeIn { get; set; }
   public int ExchangeOut { get; set; }
   public int Closing { get; set; }
   public decimal onlineCollectionAmount { get; set; }
}
public class warehoues
{
   public List<int> Warehouseids { get; set; }
   public DateTime fromdate { get; set; }
   public DateTime todate { get; set; }
}*/

public enum PaymentTypeEnum
{
    Cash = 1,
    Mpos = 2,
    Online = 3,
    Cheque = 4
}
public enum ChequeReturnStatusEnum
{
    Return = 1,
    ReceivedAtHQ = 2,
    Couriered = 3,
    ReceivedAtHub = 4,
    HandOvertoAgent = 5,
    AppovalSent = 6,
    AppovedSuccesfully = 7,
    ChequeFineReject = 8
}
public enum ChequeAppovedStatusEnum
{
    Pending = 0,
    Appoved = 1,
    Rejected = 2,
}
public enum ChequeStatusEnum
{
    InProgress = 0,
    Operation = 1,
    Bank = 2,
    Clear = 3,
    Return = 4,
    Reject = 5,
    Recovered = 6

}



public class ResponseResult
{
    public BankDepositDetailDc bankDepositDetailDc { get; set; }
    public bool status { get; set; }
    public string Message { get; set; }
}

public class ChequeStatusResponse
{
    public string Name { get; set; }
    public int Value { get; set; }
}
public class Pager
{
    public int WarehouseID { get; set; }
    public long? CurrencyHubStockId { get; set; }
    public int SkipCount { get; set; }
    public int RowCount { get; set; }
    public int? ChequeStatus { get; set; }
    public string searchfilter { get; set; }
    public DateTime? ChequeDate { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public int PeopleID { get; set; }


}

public class CurrencySettlementImage
{
    public long settlementsourceid { get; set; }
    public string settlementimagetype { get; set; }
    public string settlementimage { get; set; }
    public string Comment { get; set; }

}
public class ReturnChequeCollectionPaggingData
{
    public int total_count { get; set; }
    public List<ReturnChequeCollectionDc> ReturnChequeCollectionDcs { get; set; }
}

public class AgentdueAmountDc
{
    public long CurrencycollectionId { get; set; }
    public int Deliveryissueid { get; set; }
    public string Deliveryissueids { get; set; }

    public decimal TotalDueAmt { get; set; }
    public DateTime? WareHouseSettleDate { get; set; }
    public int Warehouseid { get; set; }
    public int DBoyPeopleId { get; set; }
    public string WarehouseName { get; set; }
    public string DBoyPeopleName { get; set; }
    public string AgentName { get; set; }
    public int Agentid { get; set; }
    public string ChequeimagePath { get; set; }
    public string Orderid { get; set; }


}

//public class Agentpayementdata
//{
//    public decimal totalamount { get; set; }
//    public List<AgentCollectionDc> agentCollectionDc { get; set; }
//    public List<AgentdueAmountDc> agentdueAmountDc { get; set; }
//}


public class Agentothermodpayementdata
{
    public decimal totalamount { get; set; }
    public List<AgentCollectionDc> agentCollectionDc { get; set; }
    public AgentChequeCollectionDc agentChequeCollectionDc { get; set; }
    public AgentOnlineCollectionDc agentOnlineCollectionDc { get; set; }
    public List<AgentdueAmountDc> agentdueAmountDc { get; set; }
    public string ChequeimagePath { get; set; }

}


public class CashBalanceCollectiondata
{
    public string Reason { get; set; }
    public List<CashBalanceCollectionDc> cashbalancecollectionDc { get; set; }


}
public class GetDeuAmountPaginatorCountdata
{
    public int total_count { get; set; }
    public List<AgentdueAmountDc> AgentdueAmountDc { get; set; }
}
public class GetChequeBouncechargePaginatorCountdata
{
    public int total_count { get; set; }
    public List<ReturnChequeChargeDc> ReturnChequeChargeDc { get; set; }
}


public class ReturnChequePaginatorCount
{
    public int total_count { get; set; }
    public List<ReturnChequeCollectionDc> ReturnChequeChargeDc { get; set; }
}

public class CustomerKisanDanDTO
{
    public int CustomerId { get; set; }
    public int OrderId { get; set; }
    public double KisanKiranaAmount { get; set; }

}

public class CashCollectionResponse
{
    public bool status { get; set; }
    public string Message { get; set; }
    public List<currencysettelDc> CashCollection { get; set; }
}

public class ReturnCashCollectionResponse
{
    public bool status { get; set; }
    public string Message { get; set; }
    public long TransactionId { get; set; }
}


public class currencysettelDc
{
    public string PeopleName { get; set; }
    public string PeopleUserName { get; set; }
    public string MobileNo { get; set; }
    public long TransactionId { get; set; }
    public DateTime TransactionDate { get; set; }
    public int CashAmount { get; set; }
}

public class CashCollectionRequest
{
    public string PeopleUserName { get; set; }
    public string MobileNo { get; set; }
    public long TransactionId { get; set; }
    public DateTime TransactionDate { get; set; }
    public int CashAmount { get; set; }
    public string ProcessTransId { get; set; }
}
public class warehouesDTO
{
    public int Wwarehouseid { get; set; }
}










