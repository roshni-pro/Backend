using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Data.Entity;
using AngularJSAuthentication.Model.CashManagement;
using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Logging;
using NLog;

namespace AngularJSAuthentication.API.External.DeliveryAPP
{
    [RoutePrefix("api/MobileDelivery")]
    public class MobileDeliveryController : ApiController
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

                    DeliveryIssuanceDetailDcs = (from a in context.DeliveryIssuanceDb
                                                 where (a.WarehouseId == warehouseid && a.Status == "Freezed" && a.PeopleID == peopleId
                                                  && !context.CurrencyCollection.Any(x=>x.Deliveryissueid==a.DeliveryIssuanceId && x.Warehouseid== warehouseid))
                                                 join b in context.OrderDeliveryMasterDB on a.DeliveryIssuanceId equals b.DeliveryIssuanceId
                                                 
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
                                                     DeliveryIssuanceId = b.DeliveryIssuanceId
                                                 }).ToList();

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


        [Route("CurrencyDenomination")]
        [HttpGet]
        public HttpResponseMessage GetCurrencyDenomination()
        {
            List<CurrencyDenominationDc> CurrencyDenominationDcs = new List<CurrencyDenominationDc>();
            try
            {
                using (AuthContext context = new AuthContext())
                {

                    CurrencyDenominationDcs = context.CurrencyDenomination.Select(b => new CurrencyDenominationDc
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
                        dbCurrencyCollection.DBoyPeopleId = currencyCollection.DBoyPeopleId;
                        dbCurrencyCollection.Deliveryissueid = currencyCollection.Deliveryissueid;
                        dbCurrencyCollection.IsActive = true;
                        dbCurrencyCollection.IsDeleted = false;
                        dbCurrencyCollection.TotalCashAmt = currencyCollection.TotalCashAmt;
                        dbCurrencyCollection.TotalCheckAmt = currencyCollection.TotalCheckAmt;
                        dbCurrencyCollection.TotalDeliveryissueAmt = currencyCollection.TotalDeliveryissueAmt;
                        dbCurrencyCollection.TotalOnlineAmt = currencyCollection.TotalOnlineAmt;
                        dbCurrencyCollection.Warehouseid = currencyCollection.Warehouseid;
                        dbCurrencyCollection.ModifiedDate = indianTime;
                        dbCurrencyCollection.ModifiedBy = currencyCollection.CreatedBy;

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
                                    List<CashCollection> CashCollections = new List<CashCollection>();
                                    foreach (var Cashitem in currencyCollection.CashCollections.Where(x => !usedCurrencyDenominationId.Contains(x.CurrencyDenominationId)))
                                    {
                                        CashCollections.Add(new CashCollection
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
                                    context.CashCollection.AddRange(CashCollections);

                                }
                            }
                            else
                            {
                                foreach (var Cashitem in dbCurrencyCollection.CashCollections)
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
                        }
                        else if (currencyCollection.CashCollections != null && currencyCollection.CashCollections.Any())
                        {
                            List<CashCollection> CashCollections = new List<CashCollection>();
                            foreach (var Cashitem in currencyCollection.CashCollections)
                            {
                                CashCollections.Add(new CashCollection
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
                            context.CashCollection.AddRange(CashCollections);
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
                        else if (currencyCollection.ChequeCollections != null && currencyCollection.ChequeCollections.Any())
                        {
                            List<ChequeCollection> ChequeCollections = new List<ChequeCollection>();
                            foreach (var Chequeitem in currencyCollection.ChequeCollections)
                            {
                                ChequeCollections.Add(new ChequeCollection
                                {
                                    CreatedBy = currencyCollection.CreatedBy,
                                    CreatedDate = indianTime,
                                    ChequeAmt = Chequeitem.ChequeAmt,
                                    DBoyPeopleId = currencyCollection.DBoyPeopleId,
                                    ChequeNumber = Chequeitem.ChequeNumber,
                                    ChequeDate= Chequeitem.ChequeDate,
                                    Orderid = Chequeitem.Orderid,
                                    UsedChequeAmt = Chequeitem.UsedChequeAmt,
                                    IsActive = true,
                                    IsDeleted = false
                                });
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
                                    IsActive = true,
                                    IsDeleted = false
                                });
                            }
                            context.ChequeCollection.AddRange(ChequeCollections);
                        }
                        #endregion


                        #region Cheque Collection
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
                        else if (currencyCollection.OnlineCollections != null && currencyCollection.OnlineCollections.Any())
                        {
                            List<OnlineCollection> OnlineCollections = new List<OnlineCollection>();
                            foreach (var Onlineitem in currencyCollection.OnlineCollections)
                            {
                                OnlineCollections.Add(new OnlineCollection
                                {
                                    CreatedBy = currencyCollection.CreatedBy,
                                    CreatedDate = indianTime,
                                    MPOSAmt = Onlineitem.MPOSAmt,
                                    MPOSReferenceNo = Onlineitem.MPOSReferenceNo,
                                    PaymentGetwayAmt = Onlineitem.PaymentGetwayAmt,
                                    PaymentReferenceNO = Onlineitem.PaymentReferenceNO,
                                    Orderid = Onlineitem.Orderid,                                    
                                    IsActive = true,
                                    IsDeleted = false
                                });
                                dbCurrencyCollection.OnlineCollections.Add(new OnlineCollection
                                {
                                    CreatedBy = currencyCollection.CreatedBy,
                                    CreatedDate = indianTime,
                                    MPOSAmt = Onlineitem.MPOSAmt,
                                    MPOSReferenceNo = Onlineitem.MPOSReferenceNo,
                                    PaymentGetwayAmt = Onlineitem.PaymentGetwayAmt,
                                    PaymentReferenceNO = Onlineitem.PaymentReferenceNO,
                                    Orderid = Onlineitem.Orderid,
                                    IsActive = true,
                                    IsDeleted = false
                                });
                            }
                            context.OnlineCollection.AddRange(OnlineCollections);
                        }
                        #endregion

                        context.CurrencyCollection.Attach(dbCurrencyCollection);
                        context.Entry(dbCurrencyCollection).State = EntityState.Modified;
                        totalRecordaffected = context.SaveChanges();
                    }
                    else
                    {
                        var dbNewCurrencyCollection = new CurrencyCollection();
                        dbNewCurrencyCollection.DBoyPeopleId = currencyCollection.CreatedBy;
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
                        #region Add Cash collection
                        dbNewCurrencyCollection.CashCollections = new List<CashCollection>();
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
                                IsActive = true,
                                IsDeleted = false
                            });
                        }
                        #endregion 

                        #region Add Online collection
                        dbNewCurrencyCollection.OnlineCollections = new List<OnlineCollection>();
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
                                IsActive = true,
                                IsDeleted = false
                            });
                        }
                        #endregion

                        context.CurrencyCollection.Add(dbNewCurrencyCollection);
                        totalRecordaffected = context.SaveChanges();

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
        public string ChequeImageUrl { get; set; }
        public DateTime OrderedDate { get; set; }
        public int DeliveryIssuanceId { get; set; }
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
        public decimal TotalDeliveryissueAmt { get; set; }
        public int CreatedBy { get; set; }

        public List<CashCollectionDc> CashCollections { get; set; }
        public List<ChequeCollectionDc> ChequeCollections { get; set; }
        public List<OnlineCollectionDc> OnlineCollections { get; set; }

    }

    public class CashCollectionDc
    {
        public long Id { get; set; }        
        public int CurrencyDenominationId { get; set; }
        public int CurrencyCountByDBoy { get; set; }      

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
    }

    public class OnlineCollectionDc
    {
        public long Id { get; set; }
        public long CurrencyCollectionId { get; set; }
        public decimal MPOSAmt { get; set; }
        public decimal PaymentGetwayAmt { get; set; }
        [StringLength(255)]
        public string MPOSReferenceNo { get; set; }
        [StringLength(255)]
        public string PaymentReferenceNO { get; set; }
        public int Orderid { get; set; }
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
        Settled
    }

}
