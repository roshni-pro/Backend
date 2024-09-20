using AgileObjects.AgileMapper;
using AngularJSAuthentication.API.Controllers;
using AngularJSAuthentication.API.Controllers.Base;
using AngularJSAuthentication.DataContracts.External;
using AngularJSAuthentication.DataContracts.Masters;
using AngularJSAuthentication.Model;
using AngularJSAuthentication.Model.CashManagement;
using AngularJSAuthentication.Model.VAN;
using GenricEcommers.Models;
using NLog;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace AngularJSAuthentication.API.External.DeliveryAPP
{
    [RoutePrefix("api/MobileDelivery")]
    public class MobileDeliveryController : BaseAuthController
    {
        private static TimeZoneInfo INDIAN_ZONE = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
        public static Logger logger = LogManager.GetCurrentClassLogger();
        /// <summary>
        /// get Assigment item List for Status Freezed
        /// </summary>
        /// <param name="peopleId"></param>
        /// <param name="warehouseid"></param>
        /// <returns></returns>
        [Route("FreezedAssignment/V1")]
        [HttpGet]
        public HttpResponseMessage FreezedAssignment(int peopleId, int warehouseid)
        {
            List<DeliveryIssuanceDetailDc> DeliveryIssuanceDetailDcs = new List<DeliveryIssuanceDetailDc>();
            List<DeliveryIssuanceDc> DeliveryIssuanceDcs = new List<DeliveryIssuanceDc>();
            try
            {
                using (AuthContext context = new AuthContext())
                {
                    DateTime currentdate = new DateTime(2019, 8, 1);
                    DeliveryIssuanceDetailDcs = (from a in context.DeliveryIssuanceDb
                                                 where (a.WarehouseId == warehouseid && a.Status == "Freezed" && a.PeopleID == peopleId && EntityFunctions.TruncateTime(a.CreatedDate) >= EntityFunctions.TruncateTime(currentdate)
                                                  && !context.CurrencyCollection.Any(x => x.Deliveryissueid == a.DeliveryIssuanceId && x.Warehouseid == warehouseid && x.IsActive && ((x.IsDeleted.HasValue && !x.IsDeleted.Value) || !x.IsDeleted.HasValue)))
                                                 join b in context.OrderDeliveryMasterDB.Where(x => x.Status == "Delivered") on a.DeliveryIssuanceId equals b.DeliveryIssuanceId
                                                 //join c in context.OrderDispatchedMasters.Where(x => x.Status == "Delivered"|| x.Status== "sattled" || x.Status == "Account settled") on a.DeliveryIssuanceId equals c.DeliveryIssuanceIdOrderDeliveryMaster
                                                 select new DeliveryIssuanceDetailDc
                                                 {
                                                     OrderId = b.OrderId,
                                                     CustomerId = b.CustomerId,
                                                     CustomerName = b.CustomerName,
                                                     ShopName = b.ShopName,
                                                     Skcode = b.Skcode,
                                                     Status = b.Status,
                                                     Trupay = b.Trupay,
                                                     Customerphonenum = b.Customerphonenum,
                                                     BillingAddress = b.BillingAddress,
                                                     ShippingAddress = b.ShippingAddress,
                                                     comments = b.comments,
                                                     TotalAmount = b.TotalAmount,
                                                     GrossAmount = b.GrossAmount,
                                                     CheckNo = b.CheckNo,
                                                     CheckAmount = b.CheckAmount,
                                                     ElectronicPaymentNo = b.ElectronicPaymentNo,
                                                     ElectronicAmount = b.ElectronicAmount,
                                                     CashAmount = b.CashAmount,
                                                     RecivedAmount = b.RecivedAmount,
                                                     DboyName = b.DboyName,
                                                     DboyMobileNo = b.DboyMobileNo,
                                                     WarehouseId = b.WarehouseId,
                                                     IsElectronicPayment = b.IsElectronicPayment,
                                                     ChequeImageUrl = b.ChequeImageUrl,
                                                     OrderedDate = b.OrderedDate,
                                                     DeliveryIssuanceId = b.DeliveryIssuanceId,
                                                     ElectronicPaymentType = b.ElectronicPaymentType,//paymenttype
                                                     Chequecomments = b.Chequecomments,//note
                                                     ChequeBankName = b.ChequeBankName,
                                                     ChequeDate = b.ChequeDate,
                                                     //DeliveryDate=c.Deliverydate   
                                                 }).OrderByDescending(x => x.DeliveryIssuanceId).ToList();

                    if (DeliveryIssuanceDetailDcs != null && DeliveryIssuanceDetailDcs.Count > 0)
                    {
                        string baseUrl = HttpContext.Current.Request.Url.GetLeftPart(UriPartial.Authority);
                        DeliveryIssuanceDetailDcs.ForEach(x =>
                        {
                            x.ChequeImageUrl = !string.IsNullOrEmpty(x.ChequeImageUrl) ? baseUrl + x.ChequeImageUrl : "";
                            x.PaymentDetails = context.PaymentResponseRetailerAppDb.Where(z => z.OrderId == x.OrderId && z.status == "Success")
                            .Select(z => new PaymentDto
                            {
                                Amount = z.amount,
                                PaymentFrom = z.PaymentFrom,
                                TransDate = z.UpdatedDate,
                                TransRefNo = z.GatewayTransId,
                                IsOnline = z.IsOnline
                            }).ToList();

                        });

                    }


                }
                if (DeliveryIssuanceDetailDcs != null && DeliveryIssuanceDetailDcs.Count > 0)
                {

                    DeliveryIssuanceDcs = DeliveryIssuanceDetailDcs.GroupBy(x => x.DeliveryIssuanceId,
                                                                                  (key, group) => new DeliveryIssuanceDc { DeliveryIssuanceId = key, DeliveryIssuanceDetailDcs = group.ToList() }).ToList();
                    var res = new
                    {
                        DeliveryIssuanceDcs = DeliveryIssuanceDcs,
                        status = true,
                        Message = "Success."
                    };
                    return Request.CreateResponse(HttpStatusCode.OK, res);
                }
                else
                {
                    var res = new
                    {
                        DeliveryIssuanceDcs = DeliveryIssuanceDcs,
                        status = false,
                        Message = "No Record found."
                    };

                    return Request.CreateResponse(HttpStatusCode.OK, res);

                }
            }
            catch (Exception ex)
            {
                logger.Error(ex.ToString());
                var res = new
                {
                    DeliveryIssuanceDcs = DeliveryIssuanceDcs,
                    status = false,
                    Message = "No Record found."
                };
                return Request.CreateResponse(HttpStatusCode.OK, res);
            }
        }



        #region get Assigment item List for Status Payment Submitted //anushka have update on this
        /// <summary>
        /// get Assigment item List for Status Payment Submitted //anushka have update on this
        /// </summary>
        /// <param name="peopleId"></param>
        /// <param name="warehouseid"></param>
        /// <returns></returns>
        [Route("PaymentSubmittedAssignment/V1")]
        [HttpGet]
        public HttpResponseMessage PaymentSubmittedAssignment(int peopleId, int warehouseid)
        {
            List<DeliveryIssuanceDetailDc> DeliveryIssuanceDetailDcs = new List<DeliveryIssuanceDetailDc>();
            List<DeliveryIssuanceDc> DeliveryIssuanceDcs = new List<DeliveryIssuanceDc>();
            try
            {
                using (AuthContext context = new AuthContext())
                {
                    DateTime currentdate = new DateTime(2019, 8, 1);
                    DeliveryIssuanceDetailDcs = (from a in context.DeliveryIssuanceDb
                                                 where (a.WarehouseId == warehouseid && a.Status == "Payment Submitted" && a.PeopleID == peopleId && EntityFunctions.TruncateTime(a.CreatedDate) >= EntityFunctions.TruncateTime(currentdate)
                                                  && !context.CurrencyCollection.Any(x => x.Deliveryissueid == a.DeliveryIssuanceId && x.Warehouseid == warehouseid && x.IsActive && ((x.IsDeleted.HasValue && !x.IsDeleted.Value) || !x.IsDeleted.HasValue)))
                                                 join b in context.OrderDeliveryMasterDB.Where(x => x.Status == "Delivered") on a.DeliveryIssuanceId equals b.DeliveryIssuanceId
                                                 //join c in context.OrderDispatchedMasters.Where(x => x.Status == "Delivered"|| x.Status== "sattled" || x.Status == "Account settled") on a.DeliveryIssuanceId equals c.DeliveryIssuanceIdOrderDeliveryMaster
                                                 select new DeliveryIssuanceDetailDc
                                                 {
                                                     OrderId = b.OrderId,
                                                     CustomerId = b.CustomerId,
                                                     CustomerName = b.CustomerName,
                                                     ShopName = b.ShopName,
                                                     Skcode = b.Skcode,
                                                     Status = b.Status,
                                                     Trupay = b.Trupay,
                                                     Customerphonenum = b.Customerphonenum,
                                                     BillingAddress = b.BillingAddress,
                                                     ShippingAddress = b.ShippingAddress,
                                                     comments = b.comments,
                                                     TotalAmount = b.TotalAmount,
                                                     GrossAmount = b.GrossAmount,
                                                     CheckNo = b.CheckNo,
                                                     CheckAmount = b.CheckAmount,
                                                     ElectronicPaymentNo = b.ElectronicPaymentNo,
                                                     ElectronicAmount = b.ElectronicAmount,
                                                     CashAmount = b.CashAmount,
                                                     RecivedAmount = b.RecivedAmount,
                                                     DboyName = b.DboyName,
                                                     DboyMobileNo = b.DboyMobileNo,
                                                     WarehouseId = b.WarehouseId,
                                                     IsElectronicPayment = b.IsElectronicPayment,
                                                     ChequeImageUrl = b.ChequeImageUrl,
                                                     OrderedDate = b.OrderedDate,
                                                     DeliveryIssuanceId = b.DeliveryIssuanceId,
                                                     ElectronicPaymentType = b.ElectronicPaymentType,//paymenttype
                                                     Chequecomments = b.Chequecomments,//note
                                                     ChequeBankName = b.ChequeBankName,
                                                     ChequeDate = b.ChequeDate,
                                                     //DeliveryDate = c.Deliverydate
                                                 }).OrderByDescending(x => x.DeliveryIssuanceId).ToList();

                    if (DeliveryIssuanceDetailDcs != null && DeliveryIssuanceDetailDcs.Count > 0)
                    {

                        var orderIds = DeliveryIssuanceDetailDcs.Select(y => y.OrderId).Distinct();
                        List<long> LongorderIds = orderIds.Select(x => (long)x).ToList();
                        var PaymentResponseRetailerApps = context.PaymentResponseRetailerAppDb.Where(x => orderIds.Contains(x.OrderId) && x.status == "Success").ToList();
                        var vANTransactiones = context.VANTransactiones.Where(x => LongorderIds.Contains(x.ObjectId) && x.ObjectType == "Order" && x.IsActive && x.IsDeleted == false).ToList();
                        var PaymentResponseRetailerAppList = new List<RetailerOrderPaymentDc>();
                        foreach (var item in PaymentResponseRetailerApps.GroupBy(x => new { x.OrderId, x.PaymentFrom, x.GatewayTransId, x.IsOnline, x.status }))
                        {
                            PaymentResponseRetailerAppList.Add(new RetailerOrderPaymentDc
                            {
                                PaymentResponseRetailerAppId = item.FirstOrDefault().id,
                                GatewayTransId = item.FirstOrDefault().GatewayTransId,
                                OrderId = item.FirstOrDefault().OrderId,
                                amount = item.Sum(x => x.amount),
                                status = item.FirstOrDefault().status,
                                PaymentFrom = item.FirstOrDefault().PaymentFrom,
                                ChequeImageUrl = item.FirstOrDefault().ChequeImageUrl,
                                ChequeBankName = item.FirstOrDefault().ChequeBankName,
                                IsOnline = item.FirstOrDefault().IsOnline,
                                TxnDate = item.OrderBy(c => c.CreatedDate).FirstOrDefault().CreatedDate
                            });
                        }
                        //string baseUrl = HttpContext.Current.Request.Url.GetLeftPart(UriPartial.Authority);
                        DeliveryIssuanceDetailDcs.ForEach(x =>
                        {
                            x.ChequeImageUrl = !string.IsNullOrEmpty(x.ChequeImageUrl) ? x.ChequeImageUrl : "";
                            x.PaymentDetails = PaymentResponseRetailerAppList.Where(z => z.OrderId == x.OrderId && z.status == "Success")
                            .Select(z => new PaymentDto
                            {
                                Amount = z.amount,
                                PaymentFrom = z.PaymentFrom,
                                TransDate = z.TxnDate,
                                TransRefNo = z.GatewayTransId,
                                ChequeImageUrl = z.ChequeImageUrl,
                                ChequeBankName = z.ChequeBankName,
                                IsOnline = z.IsOnline,
                                IsEditRTGS = PaymentResponseRetailerAppList.Any() && PaymentResponseRetailerAppList.Where(q => q.OrderId == x.OrderId && q.PaymentResponseRetailerAppId == z.PaymentResponseRetailerAppId && q.status == "Success" && q.PaymentFrom == "RTGS/NEFT").Count() > 0 && !vANTransactiones.Any(e => e.ObjectId == x.OrderId) ? true : false,
                                DeliveryIssuanceId = x.DeliveryIssuanceId,
                                PaymentResponseRetailerAppId = PaymentResponseRetailerAppList.Any() && PaymentResponseRetailerAppList.Where(q => q.OrderId == x.OrderId && q.PaymentResponseRetailerAppId == z.PaymentResponseRetailerAppId && q.status == "Success" && q.PaymentFrom == "RTGS/NEFT").Count() > 0 ? z.PaymentResponseRetailerAppId : 0
                            }).ToList();
                        });
                    }
                    //string Query = "select a.DeliveryIssuanceId from DeliveryIssuances a where 
                    //a.Status='Payment Submitted' and a.warehouseid=" + warehouseid + " and a.PeopleID=" + peopleId + " and 'Delivery Redispatch' = all" +
                    //"(select b.Status from OrderDeliveryMasters b where b.DeliveryIssuanceId = a.DeliveryIssuanceId) " +
                    //"and not exists(select c.Deliveryissueid from CurrencyCollections c where c.Deliveryissueid=a.DeliveryIssuanceId)";

                    string Query = "select a.DeliveryIssuanceId from DeliveryIssuances a where a.Status = 'Payment Submitted' and a.warehouseid = " + warehouseid + " and a.PeopleID =" + peopleId +
                   "and not exists(select b.Status from OrderDeliveryMasters b where b.DeliveryIssuanceId = a.DeliveryIssuanceId and a.warehouseid = b.WarehouseId  and b.Status not in ('Delivery Redispatch', 'Delivery Canceled'))" +
                   "and not exists (select c.Deliveryissueid from CurrencyCollections c where c.Deliveryissueid = a.DeliveryIssuanceId)";

                    var DeliveryIssuanceids = context.Database.SqlQuery<int>(Query).ToList();
                    if (DeliveryIssuanceids.Count > 0)
                    {
                        foreach (var DeliveryIssuanceId in DeliveryIssuanceids)
                        {
                            DeliveryIssuanceDetailDc ReDeliveryIssuanceDcs = new DeliveryIssuanceDetailDc();
                            ReDeliveryIssuanceDcs.DeliveryIssuanceId = DeliveryIssuanceId;
                            ReDeliveryIssuanceDcs.PaymentDetails = new List<PaymentDto> { new PaymentDto {
                             Amount=0,
                             PaymentFrom="Cash",
                             TransDate=DateTime.Now,
                             TransRefNo=""
                            } };
                            DeliveryIssuanceDetailDcs.Add(ReDeliveryIssuanceDcs);
                        }
                    }

                    if (DeliveryIssuanceDetailDcs != null && DeliveryIssuanceDetailDcs.Count > 0)
                    {

                        DeliveryIssuanceDcs = DeliveryIssuanceDetailDcs.GroupBy(x => x.DeliveryIssuanceId,
                        (key, group) => new DeliveryIssuanceDc { DeliveryIssuanceId = key, DeliveryIssuanceDetailDcs = group.ToList() }).ToList();
                        var res = new
                        {
                            DeliveryIssuanceDcs = DeliveryIssuanceDcs,
                            status = true,
                            Message = "Success."
                        };
                        return Request.CreateResponse(HttpStatusCode.OK, res);
                    }
                    else
                    {
                        var res = new
                        {
                            DeliveryIssuanceDcs = DeliveryIssuanceDcs,
                            status = false,
                            Message = "No Record found."
                        };

                        return Request.CreateResponse(HttpStatusCode.OK, res);

                    }

                }
            }
            catch (Exception ex)
            {
                logger.Error(ex.ToString());
                var res = new
                {
                    DeliveryIssuanceDcs = DeliveryIssuanceDcs,
                    status = false,
                    Message = "No Record found."
                };
                return Request.CreateResponse(HttpStatusCode.OK, res);
            }
        }
        #endregion

        [Route("CurrencyDenomination")]
        [HttpGet]
        public HttpResponseMessage GetCurrencyDenomination()
        {
            List<CurrencyDenominationDc> CurrencyDenominationDcs = new List<CurrencyDenominationDc>();
            try
            {
                using (AuthContext context = new AuthContext())
                {

                    CurrencyDenominationDcs = context.CurrencyDenomination.Where(x => x.IsActive).Select(b => new CurrencyDenominationDc
                    {
                        currencyImage = b.currencyImage,
                        currencyType = b.currencyType,
                        Id = b.Id,
                        Title = b.Title,
                        Value = b.Value
                    }).ToList();

                }
                if (CurrencyDenominationDcs != null && CurrencyDenominationDcs.Count > 0)
                {
                    var res = new
                    {
                        CurrencyDenomination = CurrencyDenominationDcs,
                        status = true,
                        Message = "Success."
                    };
                    return Request.CreateResponse(HttpStatusCode.OK, res);
                }
                else
                {
                    var res = new
                    {
                        CurrencyDenomination = CurrencyDenominationDcs,
                        status = false,
                        Message = "No Record found."
                    };

                    return Request.CreateResponse(HttpStatusCode.OK, res);

                }
            }
            catch (Exception ex)
            {
                logger.Error(ex.ToString());
                var res = new
                {
                    CurrencyDenomination = CurrencyDenominationDcs,
                    status = false,
                    Message = "No Record found."
                };
                return Request.CreateResponse(HttpStatusCode.OK, res);
            }
        }

        /// <summary>
        ///  DBoy Mobile Collection API For Cash Cheque Online
        /// </summary>
        /// <param name="currencyCollection"></param>
        /// <returns></returns>
        [Route("DBoyCashCollection")]
        [HttpPost]
        public HttpResponseMessage DBoyCashCollection(CurrencyCollectionDc currencyCollection)
        {
            int totalRecordaffected = 0;
            DateTime indianTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE);
            try
            {
                using (AuthContext context = new AuthContext())
                {
                    var dbCurrencyCollection = context.CurrencyCollection.Where(x => x.Deliveryissueid == currencyCollection.Deliveryissueid && x.Warehouseid == currencyCollection.Warehouseid).Include(x => x.CashCollections).Include(x => x.ChequeCollections).Include(x => x.OnlineCollections).FirstOrDefault();
                    if (dbCurrencyCollection != null)
                    {
                        if (dbCurrencyCollection.Status == "Settlement")
                        {
                            var res1 = new
                            {
                                status = false,
                                Message = "Assignment already settled."
                            };
                            return Request.CreateResponse(HttpStatusCode.OK, res1);
                        }

                        dbCurrencyCollection.DBoyPeopleId = currencyCollection.DBoyPeopleId;
                        dbCurrencyCollection.Deliveryissueid = currencyCollection.Deliveryissueid;
                        dbCurrencyCollection.IsActive = true;
                        dbCurrencyCollection.IsDeleted = false;
                        dbCurrencyCollection.TotalCashAmt = currencyCollection.TotalCashAmt;
                        dbCurrencyCollection.TotalCheckAmt = currencyCollection.TotalCheckAmt;
                        dbCurrencyCollection.TotalDeliveryissueAmt = currencyCollection.TotalDeliveryissueAmt;
                        dbCurrencyCollection.TotalDueAmt = currencyCollection.TotalDeliveryissueAmt - (currencyCollection.TotalCashAmt + currencyCollection.TotalCheckAmt + currencyCollection.TotalOnlineAmt);
                        dbCurrencyCollection.TotalOnlineAmt = currencyCollection.TotalOnlineAmt;
                        dbCurrencyCollection.Warehouseid = currencyCollection.Warehouseid;
                        dbCurrencyCollection.ModifiedDate = indianTime;
                        dbCurrencyCollection.ModifiedBy = currencyCollection.CreatedBy;
                        dbCurrencyCollection.Status = CurrencyCollectionStatusEnum.InProgress.ToString();
                        dbCurrencyCollection.IsCashVerify = false;
                        dbCurrencyCollection.IsChequeVerify = false;
                        dbCurrencyCollection.IsOnlinePaymentVerify = false;
                        #region Cash Collection
                        if (dbCurrencyCollection.CashCollections != null && dbCurrencyCollection.CashCollections.Any())
                        {
                            if (currencyCollection.CashCollections != null && currencyCollection.CashCollections.Any())
                            {
                                List<int> usedCurrencyDenominationId = new List<int>();
                                foreach (var Cashitem in dbCurrencyCollection.CashCollections)
                                {
                                    if (currencyCollection.CashCollections.Any(x => x.CurrencyDenominationId == Cashitem.CurrencyDenominationId))
                                    {
                                        usedCurrencyDenominationId.Add(Cashitem.CurrencyDenominationId);
                                        Cashitem.CurrencyCountByDBoy = currencyCollection.CashCollections.FirstOrDefault(x => x.CurrencyDenominationId == Cashitem.CurrencyDenominationId).CurrencyCountByDBoy;
                                        Cashitem.DBoyPeopleId = currencyCollection.DBoyPeopleId;
                                        Cashitem.ModifiedBy = currencyCollection.CreatedBy;
                                        Cashitem.ModifiedDate = indianTime;
                                        Cashitem.IsActive = true;
                                        Cashitem.IsDeleted = false;
                                        context.CashCollection.Attach(Cashitem);
                                        context.Entry(Cashitem).State = EntityState.Modified;
                                    }
                                    else
                                    {
                                        Cashitem.IsActive = true;
                                        Cashitem.IsDeleted = false;
                                        Cashitem.CurrencyCountByDBoy = 0;
                                        Cashitem.DBoyPeopleId = currencyCollection.DBoyPeopleId;
                                        Cashitem.ModifiedBy = currencyCollection.CreatedBy;
                                        Cashitem.ModifiedDate = indianTime;
                                        context.CashCollection.Attach(Cashitem);
                                        context.Entry(Cashitem).State = EntityState.Modified;
                                    }
                                }

                                if (usedCurrencyDenominationId.Any() && currencyCollection.CashCollections.Any(x => !usedCurrencyDenominationId.Contains(x.CurrencyDenominationId)))
                                {
                                    //List<CashCollection> CashCollections = new List<CashCollection>();
                                    foreach (var Cashitem in currencyCollection.CashCollections.Where(x => !usedCurrencyDenominationId.Contains(x.CurrencyDenominationId)))
                                    {
                                        //CashCollections.Add(new CashCollection
                                        //{
                                        //    CreatedBy = currencyCollection.CreatedBy,
                                        //    CreatedDate = indianTime,
                                        //    CurrencyCountByDBoy = Cashitem.CurrencyCountByDBoy,
                                        //    DBoyPeopleId = currencyCollection.DBoyPeopleId,
                                        //    CurrencyDenominationId = Cashitem.CurrencyDenominationId,
                                        //    CurrencyCollectionId = currencyCollection.Id,
                                        //    IsActive = true,
                                        //    IsDeleted = false,
                                        //    CurrencyCountByHQ = 0,
                                        //    CurrencyCountByWarehouse = 0
                                        //});
                                        dbCurrencyCollection.CashCollections.Add(new CashCollection
                                        {
                                            CreatedBy = currencyCollection.CreatedBy,
                                            CreatedDate = indianTime,
                                            CurrencyCountByDBoy = Cashitem.CurrencyCountByDBoy,
                                            DBoyPeopleId = currencyCollection.DBoyPeopleId,
                                            CurrencyDenominationId = Cashitem.CurrencyDenominationId,
                                            IsActive = true,
                                            IsDeleted = false,
                                            CurrencyCountByHQ = 0,
                                            CurrencyCountByWarehouse = 0
                                        });
                                    }
                                    // context.CashCollection.AddRange(CashCollections);

                                }
                            }
                            else
                            {
                                foreach (var Cashitem in dbCurrencyCollection.CashCollections)
                                {
                                    Cashitem.IsActive = true;
                                    Cashitem.IsDeleted = false;
                                    Cashitem.CurrencyCountByDBoy = 0;
                                    Cashitem.CurrencyCollectionId = currencyCollection.Id;
                                    Cashitem.DBoyPeopleId = currencyCollection.DBoyPeopleId;
                                    Cashitem.ModifiedBy = currencyCollection.CreatedBy;
                                    Cashitem.ModifiedDate = indianTime;
                                    context.CashCollection.Attach(Cashitem);
                                    context.Entry(Cashitem).State = EntityState.Modified;
                                }

                                dbCurrencyCollection.IsCashVerify = true;
                            }
                        }
                        else if (currencyCollection.CashCollections != null && currencyCollection.CashCollections.Any())
                        {
                            // List<CashCollection> CashCollections = new List<CashCollection>();
                            foreach (var Cashitem in currencyCollection.CashCollections)
                            {
                                //CashCollections.Add(new CashCollection
                                //{
                                //    CreatedBy = currencyCollection.CreatedBy,
                                //    CreatedDate = indianTime,
                                //    CurrencyCountByDBoy = Cashitem.CurrencyCountByDBoy,
                                //    DBoyPeopleId = currencyCollection.DBoyPeopleId,
                                //    CurrencyDenominationId = Cashitem.CurrencyDenominationId,
                                //    CurrencyCollectionId = currencyCollection.Id,
                                //    IsActive = true,
                                //    IsDeleted = false,
                                //    CurrencyCountByHQ = 0,
                                //    CurrencyCountByWarehouse = 0
                                //});
                                dbCurrencyCollection.CashCollections.Add(new CashCollection
                                {
                                    CreatedBy = currencyCollection.CreatedBy,
                                    CreatedDate = indianTime,
                                    CurrencyCountByDBoy = Cashitem.CurrencyCountByDBoy,
                                    DBoyPeopleId = currencyCollection.DBoyPeopleId,
                                    CurrencyDenominationId = Cashitem.CurrencyDenominationId,
                                    IsActive = true,
                                    IsDeleted = false,
                                    CurrencyCountByHQ = 0,
                                    CurrencyCountByWarehouse = 0
                                });
                            }
                            //context.CashCollection.AddRange(CashCollections);

                            dbCurrencyCollection.IsCashVerify = true;
                        }
                        #endregion


                        #region Cheque Collection
                        if (dbCurrencyCollection.ChequeCollections != null && dbCurrencyCollection.ChequeCollections.Any())
                        {
                            foreach (var Chequeitem in dbCurrencyCollection.ChequeCollections)
                            {
                                Chequeitem.IsActive = false;
                                Chequeitem.ModifiedBy = currencyCollection.CreatedBy;
                                Chequeitem.ModifiedDate = indianTime;
                                context.ChequeCollection.Attach(Chequeitem);
                                context.Entry(Chequeitem).State = EntityState.Modified;
                            }
                        }

                        if (currencyCollection.ChequeCollections != null && currencyCollection.ChequeCollections.Any())
                        {
                            //List<ChequeCollection> ChequeCollections = new List<ChequeCollection>();
                            foreach (var Chequeitem in currencyCollection.ChequeCollections)
                            {
                                //ChequeCollections.Add(new ChequeCollection
                                //{
                                //    CreatedBy = currencyCollection.CreatedBy,
                                //    CreatedDate = indianTime,
                                //    ChequeAmt = Chequeitem.ChequeAmt,
                                //    DBoyPeopleId = currencyCollection.DBoyPeopleId,
                                //    ChequeNumber = Chequeitem.ChequeNumber,
                                //    ChequeDate = Chequeitem.ChequeDate,
                                //    Orderid = Chequeitem.Orderid,
                                //    UsedChequeAmt = Chequeitem.UsedChequeAmt,
                                //    ChequeimagePath = Chequeitem.ChequeimagePath,
                                //    CurrencyCollectionId = currencyCollection.Id,
                                //    IsActive = true,
                                //    IsDeleted = false,
                                //    ChequeStatus = Convert.ToInt32(ChequeStatusEnum.Operation)
                                //});
                                dbCurrencyCollection.ChequeCollections.Add(new ChequeCollection
                                {
                                    CreatedBy = currencyCollection.CreatedBy,
                                    CreatedDate = indianTime,
                                    ChequeAmt = Chequeitem.ChequeAmt,
                                    DBoyPeopleId = currencyCollection.DBoyPeopleId,
                                    ChequeNumber = Chequeitem.ChequeNumber,
                                    ChequeDate = Chequeitem.ChequeDate,
                                    Orderid = Chequeitem.Orderid,
                                    UsedChequeAmt = Chequeitem.UsedChequeAmt,
                                    ChequeimagePath = Chequeitem.ChequeimagePath,
                                    ChequeBankName = Chequeitem.ChequeBankName,
                                    IsActive = true,
                                    IsDeleted = false,
                                    ChequeStatus = Convert.ToInt32(ChequeStatusEnum.InProgress)
                                });
                            }
                            // context.ChequeCollection.AddRange(ChequeCollections);
                        }
                        else
                        {
                            dbCurrencyCollection.IsChequeVerify = true;
                        }
                        #endregion


                        #region Online Collection
                        if (dbCurrencyCollection.OnlineCollections != null && dbCurrencyCollection.OnlineCollections.Any())
                        {
                            foreach (var Onlineitem in dbCurrencyCollection.OnlineCollections)
                            {
                                Onlineitem.IsActive = false;
                                Onlineitem.ModifiedBy = currencyCollection.CreatedBy;
                                Onlineitem.ModifiedDate = indianTime;
                                context.OnlineCollection.Attach(Onlineitem);
                                context.Entry(Onlineitem).State = EntityState.Modified;
                            }
                        }

                        if (currencyCollection.OnlineCollections != null && currencyCollection.OnlineCollections.Any())
                        {
                            //List<OnlineCollection> OnlineCollections = new List<OnlineCollection>();
                            foreach (var Onlineitem in currencyCollection.OnlineCollections)
                            {
                                //OnlineCollections.Add(new OnlineCollection
                                //{
                                //    CreatedBy = currencyCollection.CreatedBy,
                                //    CreatedDate = indianTime,
                                //    MPOSAmt = Onlineitem.MPOSAmt,
                                //    MPOSReferenceNo = Onlineitem.MPOSReferenceNo,
                                //    PaymentGetwayAmt = Onlineitem.PaymentGetwayAmt,
                                //    PaymentReferenceNO = Onlineitem.PaymentReferenceNO,
                                //    CurrencyCollectionId = currencyCollection.Id,
                                //    Orderid = Onlineitem.Orderid,
                                //    IsActive = true,
                                //    IsDeleted = false
                                //});
                                dbCurrencyCollection.OnlineCollections.Add(new OnlineCollection
                                {
                                    CreatedBy = currencyCollection.CreatedBy,
                                    CreatedDate = indianTime,
                                    MPOSAmt = Onlineitem.MPOSAmt,
                                    MPOSReferenceNo = Onlineitem.MPOSReferenceNo,
                                    PaymentGetwayAmt = Onlineitem.PaymentGetwayAmt,
                                    PaymentReferenceNO = Onlineitem.PaymentReferenceNO,
                                    PaymentFrom = Onlineitem.PaymentFrom,
                                    Orderid = Onlineitem.Orderid,
                                    IsActive = true,
                                    IsDeleted = false
                                });
                            }
                            //context.OnlineCollection.AddRange(OnlineCollections);
                        }
                        else
                        {
                            dbCurrencyCollection.IsOnlinePaymentVerify = true;
                        }
                        #endregion

                        context.CurrencyCollection.Attach(dbCurrencyCollection);
                        context.Entry(dbCurrencyCollection).State = EntityState.Modified;
                        totalRecordaffected = context.Commit();

                        if (totalRecordaffected > 0)
                        {
                            var payLaterOrderIds = dbCurrencyCollection.OnlineCollections.Where(x => x.PaymentFrom == "PayLater").Select(x => x.Orderid).ToList();
                            var payLaterCollectionList = context.PayLaterCollectionDb.Where(x => payLaterOrderIds.Contains(x.OrderId) && x.IsActive == true && x.IsDeleted == false).ToList();

                            payLaterCollectionList.ForEach(item =>
                            {
                                item.OnlineCollectionId = dbCurrencyCollection.OnlineCollections.Any(x => x.Orderid == item.OrderId) ? dbCurrencyCollection.OnlineCollections.FirstOrDefault(x => x.Orderid == item.OrderId).Id : 0;
                                context.Entry(item).State = EntityState.Modified;
                            });
                            context.Commit();
                        }

                    }
                    else
                    {
                        var dbNewCurrencyCollection = new CurrencyCollection();
                        dbNewCurrencyCollection.DBoyPeopleId = currencyCollection.DBoyPeopleId;
                        dbNewCurrencyCollection.Deliveryissueid = currencyCollection.Deliveryissueid;
                        dbNewCurrencyCollection.IsActive = true;
                        dbNewCurrencyCollection.IsDeleted = false;
                        dbNewCurrencyCollection.TotalCashAmt = currencyCollection.TotalCashAmt;
                        dbNewCurrencyCollection.TotalCheckAmt = currencyCollection.TotalCheckAmt;
                        dbNewCurrencyCollection.TotalDeliveryissueAmt = currencyCollection.TotalDeliveryissueAmt;
                        dbNewCurrencyCollection.TotalOnlineAmt = currencyCollection.TotalOnlineAmt;
                        dbNewCurrencyCollection.Warehouseid = currencyCollection.Warehouseid;
                        dbNewCurrencyCollection.CreatedDate = indianTime;
                        dbNewCurrencyCollection.CreatedBy = currencyCollection.CreatedBy;
                        dbNewCurrencyCollection.Status = CurrencyCollectionStatusEnum.InProgress.ToString();
                        dbNewCurrencyCollection.TotalDueAmt = currencyCollection.TotalDeliveryissueAmt - (currencyCollection.TotalCashAmt + currencyCollection.TotalCheckAmt + currencyCollection.TotalOnlineAmt);
                        dbNewCurrencyCollection.IsCashVerify = false;
                        dbNewCurrencyCollection.IsChequeVerify = false;
                        dbNewCurrencyCollection.IsOnlinePaymentVerify = false;
                        #region Add Cash collection
                        dbNewCurrencyCollection.CashCollections = new List<CashCollection>();
                        if (currencyCollection.CashCollections == null || !currencyCollection.CashCollections.Any())
                            dbNewCurrencyCollection.IsCashVerify = true;

                        foreach (var Cashitem in currencyCollection.CashCollections)
                        {
                            dbNewCurrencyCollection.CashCollections.Add(new CashCollection
                            {
                                CreatedBy = currencyCollection.CreatedBy,
                                CreatedDate = indianTime,
                                CurrencyCountByDBoy = Cashitem.CurrencyCountByDBoy,
                                DBoyPeopleId = currencyCollection.DBoyPeopleId,
                                CurrencyDenominationId = Cashitem.CurrencyDenominationId,
                                IsActive = true,
                                IsDeleted = false,
                                CurrencyCountByHQ = 0,
                                CurrencyCountByWarehouse = 0
                            });
                        }
                        #endregion

                        #region Add Cheque collection
                        dbNewCurrencyCollection.ChequeCollections = new List<ChequeCollection>();
                        if (currencyCollection.ChequeCollections == null || !currencyCollection.ChequeCollections.Any())
                            dbNewCurrencyCollection.IsChequeVerify = true;

                        foreach (var Chequeitem in currencyCollection.ChequeCollections)
                        {
                            dbNewCurrencyCollection.ChequeCollections.Add(new ChequeCollection
                            {
                                CreatedBy = currencyCollection.CreatedBy,
                                CreatedDate = indianTime,
                                ChequeAmt = Chequeitem.ChequeAmt,
                                DBoyPeopleId = currencyCollection.DBoyPeopleId,
                                ChequeNumber = Chequeitem.ChequeNumber,
                                ChequeDate = Chequeitem.ChequeDate,
                                Orderid = Chequeitem.Orderid,
                                UsedChequeAmt = Chequeitem.UsedChequeAmt,
                                ChequeimagePath = Chequeitem.ChequeimagePath,
                                ChequeBankName = Chequeitem.ChequeBankName,
                                IsActive = true,
                                IsDeleted = false,
                                ChequeStatus = Convert.ToInt32(ChequeStatusEnum.InProgress)
                            });
                        }
                        #endregion 

                        #region Add Online collection
                        dbNewCurrencyCollection.OnlineCollections = new List<OnlineCollection>();
                        if (currencyCollection.OnlineCollections == null || !currencyCollection.OnlineCollections.Any())
                            dbNewCurrencyCollection.IsOnlinePaymentVerify = true;
                        foreach (var Onlineitem in currencyCollection.OnlineCollections)
                        {
                            dbNewCurrencyCollection.OnlineCollections.Add(new OnlineCollection
                            {
                                CreatedBy = currencyCollection.CreatedBy,
                                CreatedDate = indianTime,
                                MPOSAmt = Onlineitem.MPOSAmt,
                                MPOSReferenceNo = Onlineitem.MPOSReferenceNo,
                                PaymentGetwayAmt = Onlineitem.PaymentGetwayAmt,
                                PaymentReferenceNO = Onlineitem.PaymentReferenceNO,
                                Orderid = Onlineitem.Orderid,
                                PaymentFrom = Onlineitem.PaymentFrom,
                                IsActive = true,
                                IsDeleted = false
                            });
                        }
                        #endregion

                        context.CurrencyCollection.Add(dbNewCurrencyCollection);
                        totalRecordaffected = context.Commit();
                        if (totalRecordaffected > 0)
                        {
                            var payLaterOrderIds = dbNewCurrencyCollection.OnlineCollections.Where(x => x.PaymentFrom == "PayLater").Select(x => x.Orderid).ToList();
                            var payLaterCollectionList = context.PayLaterCollectionDb.Where(x => payLaterOrderIds.Contains(x.OrderId) && x.IsActive == true && x.IsDeleted == false).ToList();

                            payLaterCollectionList.ForEach(item =>
                            {
                                item.OnlineCollectionId = dbNewCurrencyCollection.OnlineCollections.Any(x => x.Orderid == item.OrderId) ? dbNewCurrencyCollection.OnlineCollections.FirstOrDefault(x => x.Orderid == item.OrderId).Id : 0;
                                context.Entry(item).State = EntityState.Modified;
                            });
                            context.Commit();
                        }

                    }

                    if (totalRecordaffected > 0)
                    {
                        AngularJSAuthentication.BusinessLayer.Managers.Masters.CustomersManager manager = new AngularJSAuthentication.BusinessLayer.Managers.Masters.CustomersManager();
                        List<AngularJSAuthentication.DataContracts.Masters.CustomerWalletBillDiscountDc> customerWalletBillDiscountDcs = manager.GetCustomerPostOrderWallet(currencyCollection.Deliveryissueid);

                        if (customerWalletBillDiscountDcs != null && customerWalletBillDiscountDcs.Any())
                        {
                            foreach (var item in customerWalletBillDiscountDcs)
                            {
                                var wallet = context.WalletDb.Where(c => c.CustomerId == item.CustomerId).SingleOrDefault();

                                CustomerWalletHistory CWH = new CustomerWalletHistory();

                                if (item.BillDiscountTypeValue > 0)
                                {
                                    CWH.WarehouseId = currencyCollection.Warehouseid;
                                    CWH.CompanyId = 1;
                                    CWH.CustomerId = item.CustomerId;
                                    CWH.Through = "Due To Order " + item.OrderId + " Delivered Successfully.";
                                    CWH.NewAddedWAmount = item.BillDiscountTypeValue;
                                    CWH.TotalWalletAmount = wallet.TotalAmount + item.BillDiscountTypeValue;
                                    CWH.CreatedDate = indianTime;
                                    CWH.UpdatedDate = indianTime;
                                    CWH.OrderId = item.OrderId;
                                    context.CustomerWalletHistoryDb.Add(CWH);

                                    //update in wallet
                                    wallet.TotalAmount += item.BillDiscountTypeValue;
                                    wallet.TransactionDate = indianTime;
                                    context.Entry(wallet).State = EntityState.Modified;
                                }

                                var billDiscount = context.BillDiscountDb.FirstOrDefault(x => x.Id == item.Id);
                                if (billDiscount != null)
                                {
                                    billDiscount.IsAddNextOrderWallet = true;
                                }
                                context.Entry(billDiscount).State = EntityState.Modified;
                            }
                            context.Commit();

                        }


                    }
                }


                var res = new
                {
                    status = totalRecordaffected > 0,
                    Message = totalRecordaffected > 0 ? "Success." : "Fail."
                };
                return Request.CreateResponse(HttpStatusCode.OK, res);

            }
            catch (Exception ex)
            {
                logger.Error(ex.ToString());
                var res = new
                {
                    status = false,
                    Message = "Error during Save detail."
                };

                return Request.CreateResponse(HttpStatusCode.OK, res);
            }
        }

        /// <summary>
        /// Get Last Two Days Currency Collection data
        /// </summary>
        /// <param name="peopleId"></param>
        /// <param name="warehouseid"></param>
        /// <returns></returns>
        public HttpResponseMessage GetLastTwoDayCurrencyCollection(int peopleId, int warehouseid)
        {
            List<CurrencyCollectionSummaryDc> CurrencyCollectionSummaryDcs = new List<CurrencyCollectionSummaryDc>();
            DateTime TodayDate = DateTime.Now.Date;
            DateTime Previouse2Date = TodayDate.AddDays(-2);
            try
            {
                using (AuthContext context = new AuthContext())
                {
                    context.Database.Log = s => System.Diagnostics.Debug.WriteLine(s);
                    CurrencyCollectionSummaryDcs = context.CurrencyCollection.Where(x => x.DBoyPeopleId == peopleId && x.Warehouseid == warehouseid
                                                     && x.IsActive && ((x.IsDeleted.HasValue && !x.IsDeleted.Value) || !x.IsDeleted.HasValue)
                                                     && (EntityFunctions.TruncateTime(x.CreatedDate) >= EntityFunctions.TruncateTime(Previouse2Date)
                                                     && EntityFunctions.TruncateTime(x.CreatedDate) <= EntityFunctions.TruncateTime(TodayDate)))
                    .Select(x => new CurrencyCollectionSummaryDc
                    {
                        CurrencyCollectionId = x.Id,
                        Warehouseid = x.Warehouseid,
                        DBoyPeopleId = x.DBoyPeopleId,
                        Deliveryissueid = x.Deliveryissueid,
                        TotalCashAmt = x.TotalCashAmt,
                        TotalOnlineAmt = x.TotalOnlineAmt,
                        TotalCheckAmt = x.TotalCheckAmt,
                        TotalDeliveryissueAmt = x.TotalDeliveryissueAmt,
                        CreatedBy = x.CreatedBy,
                        DeclineNote = x.DeclineNote,
                        Status = x.Status
                    }).ToList();
                }

                var res = new
                {
                    CurrencyCollectionSummaryDcs = CurrencyCollectionSummaryDcs,
                    status = CurrencyCollectionSummaryDcs != null && CurrencyCollectionSummaryDcs.Count > 0 ? true : false,
                    Message = CurrencyCollectionSummaryDcs != null && CurrencyCollectionSummaryDcs.Count > 0 ? "Success" : "No Data Found"
                };
                return Request.CreateResponse(HttpStatusCode.OK, res);
            }
            catch (Exception ex)
            {
                logger.Error(ex.ToString());
                var res = new
                {
                    CurrencyCollectionSummaryDcs = CurrencyCollectionSummaryDcs,
                    status = false,
                    Message = "Fail"
                };

                return Request.CreateResponse(HttpStatusCode.OK, res);
            }
        }
        /// <summary>
        /// Get Currency Collection Detail by Id
        /// </summary>
        /// <param name="currencyCollectionId"></param>
        /// <returns></returns>
        public HttpResponseMessage GetCurrencyCollectionById(long currencyCollectionId)
        {
            CurrencyCollectionDc currencyCollectionDc = new CurrencyCollectionDc();
            CurrencyCollection dbCurrencyCollection = new CurrencyCollection();
            List<VANTransaction> vANTransactiones = new List<VANTransaction>();
            List<PaymentResponseRetailerApp> PaymentDetails = new List<PaymentResponseRetailerApp>();
            try
            {
                using (AuthContext context = new AuthContext())
                {
                    dbCurrencyCollection = context.CurrencyCollection.Where(x => x.Id == currencyCollectionId).Include(x => x.CashCollections).Include(x => x.ChequeCollections).Include(x => x.OnlineCollections).FirstOrDefault();

                    if (dbCurrencyCollection != null)
                    {
                        var CurrencyDenomination = context.CurrencyDenomination.Where(x => x.IsActive).ToList();

                        if (dbCurrencyCollection.CashCollections.Any())
                            dbCurrencyCollection.CashCollections = dbCurrencyCollection.CashCollections.Where(x => x.IsActive && !x.IsDeleted.Value).ToList();
                        if (dbCurrencyCollection.ChequeCollections.Any())
                            dbCurrencyCollection.ChequeCollections = dbCurrencyCollection.ChequeCollections.Where(x => x.IsActive && !x.IsDeleted.Value).ToList();
                        if (dbCurrencyCollection.OnlineCollections.Any())
                            dbCurrencyCollection.OnlineCollections = dbCurrencyCollection.OnlineCollections.Where(x => x.IsActive && !x.IsDeleted.Value).ToList();

                        currencyCollectionDc.CashCollections = new List<CashCollectionDc>();
                        currencyCollectionDc.ChequeCollections = new List<ChequeCollectionDc>();
                        currencyCollectionDc.OnlineCollections = new List<OnlineCollectionDc>();

                        var orderids = context.OrderDeliveryMasterDB.Where(x => x.DeliveryIssuanceId == dbCurrencyCollection.Deliveryissueid && x.Status == "Delivered").Select(x => x.OrderId).ToList();
                        double totatCollectionamt = 0, totalonlineamt = 0, totalchequeamt = 0, totalmposamt = 0;
                        if (orderids != null && orderids.Any())
                        {
                            List<long> LongorderIds = orderids.Select(x => (long)x).ToList();
                            vANTransactiones = context.VANTransactiones.Where(x => LongorderIds.Contains(x.ObjectId) && x.ObjectType == "Order" && x.IsActive && x.IsDeleted == false).ToList();
                            PaymentDetails = context.PaymentResponseRetailerAppDb.Where(z => orderids.Contains(z.OrderId) && z.status == "Success").ToList();
                            if (PaymentDetails != null && PaymentDetails.Any())
                            {
                                totatCollectionamt = PaymentDetails.Where(x => x.PaymentFrom == "Cash").Sum(x => x.amount);
                                totalchequeamt = PaymentDetails.Where(x => x.PaymentFrom == "Cheque").Sum(x => x.amount);
                                totalonlineamt = PaymentDetails.Where(x => x.IsOnline /*x.PaymentFrom != "Cheque" && x.PaymentFrom != "Cash"*/).Sum(x => x.amount);
                            }
                        }
                        currencyCollectionDc = new CurrencyCollectionDc
                        {
                            Id = dbCurrencyCollection.Id,
                            Warehouseid = dbCurrencyCollection.Warehouseid,
                            DBoyPeopleId = dbCurrencyCollection.DBoyPeopleId,
                            Deliveryissueid = dbCurrencyCollection.Deliveryissueid,
                            TotalCashAmt = dbCurrencyCollection.TotalCashAmt,
                            TotalOnlineAmt = dbCurrencyCollection.TotalOnlineAmt,
                            TotalCheckAmt = dbCurrencyCollection.TotalCheckAmt,
                            TotalDeliveryissueAmt = dbCurrencyCollection.TotalDeliveryissueAmt,
                            TotalCollectionCashAmt = Convert.ToDecimal(totatCollectionamt),
                            TotalCollectionCheckAmt = Convert.ToDecimal(totalchequeamt),
                            TotalCollectionOnlineAmt = Convert.ToDecimal(totalonlineamt),
                            CreatedBy = dbCurrencyCollection.CreatedBy,
                            Status = dbCurrencyCollection.Status,
                            CashCollections = (from c in CurrencyDenomination
                                               join p in dbCurrencyCollection.CashCollections
                                               on c.Id equals p.CurrencyDenominationId into ps
                                               from p in ps.DefaultIfEmpty()
                                               select new CashCollectionDc
                                               {
                                                   Id = c == null ? 0 : c.Id,
                                                   CurrencyCountByDBoy = p == null ? 0 : (p.CurrencyCountByDBoy),
                                                   CurrencyDenominationId = c == null ? 0 : c.Id,
                                                   Title = c == null ? "" : c.Title,
                                                   Value = c == null ? 0 : c.Value,
                                                   currencyType = c == null ? "" : c.currencyType,
                                               }).ToList(),
                            //dbCurrencyCollection.CashCollections.Select(x => new CashCollectionDc
                            //{
                            //    Id = x.Id,
                            //    CurrencyCountByDBoy = x.CurrencyCountByDBoy,
                            //    CurrencyDenominationId = x.CurrencyDenominationId
                            //}).ToList(),
                            ChequeCollections = dbCurrencyCollection.ChequeCollections.Select(y => new ChequeCollectionDc
                            {
                                ChequeAmt = y.ChequeAmt,
                                ChequeDate = y.ChequeDate,
                                ChequeNumber = y.ChequeNumber,
                                Orderid = y.Orderid,
                                Id = y.Id,
                                UsedChequeAmt = y.UsedChequeAmt,
                                ChequeStatus = y.ChequeStatus,
                                ChequeNote = y.ChequeNote,
                                ChequeimagePath = y.ChequeimagePath,
                                ChequeBankName = y.ChequeBankName
                            }).ToList(),
                            OnlineCollections = dbCurrencyCollection.OnlineCollections.Select(y => new OnlineCollectionDc
                            {
                                CurrencyCollectionId = y.CurrencyCollectionId,
                                Id = y.Id,
                                MPOSAmt = y.MPOSAmt,
                                MPOSReferenceNo = y.MPOSReferenceNo,
                                Orderid = y.Orderid,
                                PaymentGetwayAmt = y.PaymentGetwayAmt,
                                PaymentReferenceNO = y.PaymentReferenceNO,
                                PaymentFrom = y.PaymentFrom,
                                IsEditRTGS = y.PaymentFrom == "RTGS/NEFT" && !vANTransactiones.Any(e => e.ObjectId == y.Orderid) ? true : false,
                                DeliveryIssuanceId = dbCurrencyCollection.Deliveryissueid,
                                PaymentResponseRetailerAppId = PaymentDetails.Any() && PaymentDetails != null && PaymentDetails.Any(x => x.OrderId == y.Orderid && x.PaymentFrom == "RTGS/NEFT" && x.status == "Success" && x.IsOnline == true) ? PaymentDetails.Where(x => x.OrderId == y.Orderid && x.PaymentFrom == "RTGS/NEFT" && !string.IsNullOrEmpty(x.GatewayTransId.Trim()) && x.status == "Success" && x.IsOnline == true && x.GatewayTransId.Trim() == y.PaymentReferenceNO.Trim()).Select(x => x.id).FirstOrDefault() : 0
                            }).ToList()
                        };
                    }
                }

                var res = new
                {
                    CurrencyCollectionDc = currencyCollectionDc,
                    status = true,
                    Message = "Success"
                };
                return Request.CreateResponse(HttpStatusCode.OK, res);
            }
            catch (Exception ex)
            {
                logger.Error(ex.ToString());
                var res = new
                {
                    CurrencyCollectionDc = currencyCollectionDc,
                    status = false,
                    Message = "Fail"
                };

                return Request.CreateResponse(HttpStatusCode.OK, res);
            }
        }

        [Route("CurrencyUploadChequeImageForMobile")]
        [HttpPost]
        [AllowAnonymous]
        public string CurrencyUploadChequeImageForMobile()
        {
            string LogoUrl = "";
            try
            {
                if (HttpContext.Current.Request.Files.AllKeys.Any())
                {
                    var httpPostedFile = HttpContext.Current.Request.Files["file"];
                    if (httpPostedFile != null)
                    {
                        //if (!Directory.Exists(HttpContext.Current.Server.MapPath("~/CurrencyChequeImage")))
                        //    Directory.CreateDirectory(HttpContext.Current.Server.MapPath("~/CurrencyChequeImage"));

                        //LogoUrl = Path.Combine(HttpContext.Current.Server.MapPath("~/CurrencyChequeImage"), httpPostedFile.FileName);

                        //httpPostedFile.SaveAs(LogoUrl);

                        string extension = Path.GetExtension(httpPostedFile.FileName);

                        string fileName = httpPostedFile.FileName.Substring(0, httpPostedFile.FileName.LastIndexOf('.')) + DateTime.Now.ToString("ddMMyyyyHHmmss") + extension;

                        LogoUrl = Path.Combine(HttpContext.Current.Server.MapPath("~/CurrencyChequeImage"), fileName);

                        httpPostedFile.SaveAs(LogoUrl);

                        AngularJSAuthentication.Common.Helpers.FileUploadHelper.Upload(fileName, "~/CurrencyChequeImage", LogoUrl);

                        LogoUrl = "/CurrencyChequeImage/" + fileName;
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error("Error in CurrencyUploadChequeImageForMobile Method: " + ex.Message);
            }
            return LogoUrl;
        }

        [Route("GetBankName")]
        [HttpGet]
        public HttpResponseMessage GetBankName()
        {

            List<DataContract.CurrencySettlementBankDc> currencySettlementBankDcs = new List<DataContract.CurrencySettlementBankDc>();
            try
            {
                using (AuthContext context = new AuthContext())
                {

                    currencySettlementBankDcs = context.CurrencySettlementBank.Where(x => x.ChequeBank).Select(x =>
                          new DataContract.CurrencySettlementBankDc
                          {
                              BankImage = x.BankImage,
                              BankName = x.BankName,
                              Id = x.Id
                          }).ToList();
                }

                var res = new
                {
                    BankNameDc = currencySettlementBankDcs,
                    status = true,
                    Message = "Success"
                };
                return Request.CreateResponse(HttpStatusCode.OK, res);
            }
            catch (Exception ex)
            {
                logger.Error(ex.ToString());
                var res = new
                {
                    BankNameDc = currencySettlementBankDcs,
                    status = false,
                    Message = "Fail"
                };

                return Request.CreateResponse(HttpStatusCode.OK, res);
            }
        }

        /// <summary>
        ///  this Api Use for Get Delivery App Version
        /// </summary>
        /// <returns></returns>
        [Route("GetDeliveryAppVersion")]
        [HttpGet]
        public HttpResponseMessage GetDeliveryAppVersion()
        {
            string deliveryAppVersion = null;
            try
            {
                using (AuthContext context = new AuthContext())
                {

                    deliveryAppVersion = context.DeliveryAppVersionDB.Where(x => x.isCompulsory == true && x.Active == true).Select(x => x.App_version).FirstOrDefault();
                }

                var res = new
                {
                    deliveryAppVersion = deliveryAppVersion,
                    status = true,
                    Message = "Success"
                };
                return Request.CreateResponse(HttpStatusCode.OK, res);
            }
            catch (Exception ex)
            {
                logger.Error(ex.ToString());
                var res = new
                {
                    deliveryAppVersion = deliveryAppVersion,
                    status = false,
                    Message = "Fail"
                };

                return Request.CreateResponse(HttpStatusCode.OK, res);
            }




        }

        [Route("UpdateProfileImage")]
        [HttpPost]
        public async Task<bool> UpdateProfileImage(UpdateDboyProfileImageDc obj)
        {
            bool result = false;
            if (obj != null && obj.PeopleId > 0 && obj.ProfilePic != null)
            {
                using (var db = new AuthContext())
                {
                    var person = await db.Peoples.Where(u => u.PeopleID == obj.PeopleId).FirstOrDefaultAsync();
                    person.ProfilePic = obj.ProfilePic;
                    person.UpdatedDate = DateTime.Now;
                    db.Entry(person).State = EntityState.Modified;
                    result = db.Commit() > 0;
                }
            }
            return result;
        }

        #region
        // Delivery Boy Rating  (type 2)
        [Route("GetDboyRatingOrder")]
        [HttpGet]
        [AllowAnonymous]
        public RatingDboyDC GetDboyRatingOrder(int Id) //Id : OrderId
        {
            RatingDboyDC ratingDboyDC = new RatingDboyDC();
            using (var context = new AuthContext())
            {
                var param = new SqlParameter("OrderId", Id);
                var Order = context.Database.SqlQuery<DeliveryDboyRatingOrderDc>("exec operation.GetDboyRatingOrder @OrderId", param).FirstOrDefault();
                var ratingConfig = context.RatingMasters.Where(x => x.AppType == 2 && x.IsActive == true && x.IsDeleted == false).Include(x => x.RatingDetails).ToList();
                var result = Mapper.Map(ratingConfig).ToANew<List<UserRatingDc>>();
                if (Order != null && ratingConfig != null)
                {
                    ratingDboyDC.DeliveryDboyRatingOrder = Order;
                    result.ForEach(x =>
                    {
                        x.AppTypeName = "Delivery Rating";
                    });
                    ratingDboyDC.userRatingDc = result;
                }
            }
            return ratingDboyDC;
        }
        #endregion

        [Route("UpdateRefNumberForOrder")]
        [HttpGet]
        public bool UpdateRefNumberForOrder(int OrderId, string RefNo, int PaymentResponseRetailerAppId) //Id : OrderId
        {
            bool status = false;
            string Oldref = string.Empty;
            using (var context = new AuthContext())
            {
                var payment = context.PaymentResponseRetailerAppDb.FirstOrDefault(x => x.id == PaymentResponseRetailerAppId && x.OrderId == OrderId && x.PaymentFrom == "RTGS/NEFT" && x.status == "Success");
                if (payment != null)
                {
                    Oldref = payment.GatewayTransId;
                    payment.GatewayTransId = RefNo;
                    payment.UpdatedDate = DateTime.Now;
                    context.Entry(payment).State = EntityState.Modified;

                    var onlineCollection = context.OnlineCollection.Where(x => x.Orderid == OrderId && x.PaymentReferenceNO == Oldref.Trim() && x.PaymentFrom == "RTGS/NEFT" && x.IsActive == true && x.IsDeleted == false).FirstOrDefault();
                    if (onlineCollection != null)
                    {
                        onlineCollection.PaymentReferenceNO = RefNo;
                        onlineCollection.ModifiedDate = DateTime.Now;
                        context.Entry(onlineCollection).State = EntityState.Modified;
                    }
                    context.Commit();
                    status = true;
                }
            }
            return status;
        }

    }
    public class UpdateDboyProfileImageDc
    {
        public int PeopleId { get; set; }
        public string ProfilePic { get; set; }
    }
    public class DeliveryIssuanceDetailDc
    {
        public int OrderId { get; set; }
        public int CustomerId { get; set; }
        public string CustomerName { get; set; }
        public string ShopName { get; set; }
        public string Skcode { get; set; }
        public string Status { get; set; }
        public string Trupay { get; set; }
        public string Customerphonenum { get; set; }
        public string BillingAddress { get; set; }
        public string ShippingAddress { get; set; }
        public string comments { get; set; }
        public double TotalAmount { get; set; }
        public double GrossAmount { get; set; }
        public string CheckNo { get; set; }
        public double CheckAmount { get; set; }
        public string ElectronicPaymentNo { get; set; }
        public double ElectronicAmount { get; set; }
        public double CashAmount { get; set; }
        public double RecivedAmount { get; set; }
        public string DboyName { get; set; }
        public string DboyMobileNo { get; set; }
        public int WarehouseId { get; set; }
        public bool IsElectronicPayment { get; set; }
        public int? ElectronicPaymentType { get; set; }
        public string ChequeImageUrl { get; set; }
        public DateTime OrderedDate { get; set; }
        public int DeliveryIssuanceId { get; set; }
        //public DateTime DeliveryDate { get; set; }

        public string Chequecomments { get; set; }
        [StringLength(1000)]
        public string ChequeBankName { get; set; }
        public DateTime? ChequeDate { get; set; }
        public List<PaymentDto> PaymentDetails { get; set; }
    }
    public class DeliveryIssuanceRediDcs
    {
        public int DeliveryIssuanceId { get; set; }
    }


    public class PaymentDto
    {
        public string TransRefNo { get; set; }
        public double Amount { get; set; }
        public string PaymentFrom { get; set; }
        public DateTime TransDate { get; set; }
        public string ChequeImageUrl { get; set; }
        public string ChequeBankName { get; set; }
        public bool IsOnline { get; set; }
        public bool IsEditRTGS { get; set; }
        public int DeliveryIssuanceId { get; set; }
        public int PaymentResponseRetailerAppId { get; set; }
    }

    public class DeliveryIssuanceDc
    {
        public int DeliveryIssuanceId { get; set; }
        public List<DeliveryIssuanceDetailDc> DeliveryIssuanceDetailDcs { get; set; }
    }


    public class CurrencyCollectionDc
    {
        public long Id { get; set; }
        public int Warehouseid { get; set; }
        public int DBoyPeopleId { get; set; }
        public int Deliveryissueid { get; set; }
        public decimal TotalCashAmt { get; set; }
        public decimal TotalOnlineAmt { get; set; }
        public decimal TotalCheckAmt { get; set; }

        public decimal TotalCollectionCashAmt { get; set; }
        public decimal TotalCollectionOnlineAmt { get; set; }
        public decimal TotalCollectionCheckAmt { get; set; }
        public decimal TotalDeliveryissueAmt { get; set; }
        public int CreatedBy { get; set; }
        public string Status { get; set; }
        public List<CashCollectionDc> CashCollections { get; set; }
        public List<ChequeCollectionDc> ChequeCollections { get; set; }
        public List<OnlineCollectionDc> OnlineCollections { get; set; }

    }

    public class CashCollectionDc
    {
        public long Id { get; set; }
        public int CurrencyDenominationId { get; set; }
        public int CurrencyCountByDBoy { get; set; }
        public string Title { get; set; }
        public int Value { get; set; }
        public string currencyType { get; set; }
    }

    public class ChequeCollectionDc
    {
        public long Id { get; set; }
        [StringLength(255)]
        public string ChequeNumber { get; set; }
        public decimal ChequeAmt { get; set; }
        public int Orderid { get; set; }
        public DateTime ChequeDate { get; set; }
        public decimal UsedChequeAmt { get; set; }
        public int? ChequeStatus { get; set; }
        [StringLength(1000)]
        public string ChequeNote { get; set; }
        [StringLength(2000)]
        public string ChequeimagePath { get; set; }
        [StringLength(1000)]
        public string ChequeBankName { get; set; }
    }

    public class OnlineCollectionDc
    {
        public long Id { get; set; }
        public long? CurrencyCollectionId { get; set; }
        public decimal MPOSAmt { get; set; }
        public decimal PaymentGetwayAmt { get; set; }
        [StringLength(255)]
        public string MPOSReferenceNo { get; set; }
        [StringLength(255)]
        public string PaymentReferenceNO { get; set; }
        [StringLength(255)]
        public string PaymentFrom { get; set; }
        public int Orderid { get; set; }
        public bool IsEditRTGS { get; set; }
        public int DeliveryIssuanceId { get; set; }
        public int PaymentResponseRetailerAppId { get; set; }
    }

    public class CurrencyDenominationDc
    {
        public int Id { get; set; }
        public string currencyType { get; set; }
        public string Title { get; set; }
        public string currencyImage { get; set; }
        public int Value { get; set; }

    }

    public enum CurrencyCollectionStatusEnum
    {
        InProgress,
        Settlement,
        Decline
    }

    public class CurrencyCollectionSummaryDc
    {
        public long CurrencyCollectionId { get; set; }
        public int Warehouseid { get; set; }
        public int DBoyPeopleId { get; set; }
        public int Deliveryissueid { get; set; }
        public decimal TotalCashAmt { get; set; }
        public decimal TotalOnlineAmt { get; set; }
        public decimal TotalCheckAmt { get; set; }

        public decimal TotalCollectionAmt
        {
            get
            {
                return TotalCashAmt + TotalOnlineAmt + TotalCheckAmt;
            }
        }
        public decimal TotalDeliveryissueAmt { get; set; }
        public int CreatedBy { get; set; }

        public string Status { get; set; }
        public string DeclineNote { get; set; }
    }

}
