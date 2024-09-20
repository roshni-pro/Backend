using AgileObjects.AgileMapper;
using AngularJSAuthentication.API.Controllers.Base;
using AngularJSAuthentication.API.Helpers;
using AngularJSAuthentication.API.Managers;
using AngularJSAuthentication.BusinessLayer.Managers.Reports;
using AngularJSAuthentication.Common.Constants;
using AngularJSAuthentication.Common.Helpers;
using AngularJSAuthentication.DataContracts.Shared;
using AngularJSAuthentication.Model;
using AngularJSAuthentication.Model.BillDiscount;
using AngularJSAuthentication.Model.Gullak;
using AngularJSAuthentication.Model.Seller;
using GenricEcommers.Models;
using Hangfire;
using LinqKit;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using NLog;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using SqlBulkTools;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
//using System.Data;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Transactions;
using System.Web;
using System.Web.Http;
using System.Web.Http.Description;
using System.Web.Script.Serialization;
using AngularJSAuthentication.DataContracts.BillDiscount;
using System.Reflection;
using System.Web.Hosting;
using Newtonsoft.Json;
using AngularJSAuthentication.Model.CRM;
using AngularJSAuthentication.BusinessLayer.Managers.Masters;
using AngularJSAuthentication.Model.Store;

namespace AngularJSAuthentication.API.Controllers
{
    [RoutePrefix("api/offer")]

    public class OfferController : BaseAuthController
    {
        private static TimeZoneInfo INDIAN_ZONE = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
        DateTime indianTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE);

        private static Logger logger = LogManager.GetCurrentClassLogger();
        public double xPointValue = AppConstants.xPoint;
        //For Getting list of Offer
        [Authorize]
        [Route("")]
        [HttpGet]
        public List<OfferList> Get()
        {
            logger.Info("start Offer: ");
            List<OfferList> OfferList = new List<OfferList>();
            using (var context = new AuthContext())
            {
                try
                {
                    var identity = User.Identity as ClaimsIdentity;
                    int compid = 0, userid = 0;
                    int Warehouse_id = 0;
                    string email = "";
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
                        if (claim.Type == "email")
                        {
                            email = claim.Value;
                        }
                    }
                    logger.Info("End Get Company: ");
                    if (Warehouse_id > 0)
                    {

                        OfferList = context.GetAllOfferWid(compid, Warehouse_id).ToList();
                        logger.Info("End  Offer: ");
                        return OfferList;
                    }
                    else
                    {
                        OfferList = context.GetAllOffer(compid).ToList();
                        logger.Info("End  Offer: ");
                        return OfferList;
                    }

                }
                catch (Exception ex)
                {
                    logger.Error("Error in Offer " + ex.Message);
                    logger.Info("End  Offer: ");
                    return null;
                }
            }
        }

        [Authorize]
        [Route("GetOffer")]
        [HttpPost]

        public OfferPaggingData GetOffer(getofferobject getofferobject)
        {
            logger.Info("start Offer: ");
            OfferPaggingData OfferPaggingData = new OfferPaggingData();
            List<OfferList> OfferList = new List<OfferList>();
            using (var context = new AuthContext())
            {
                try
                {

                    var predicate = PredicateBuilder.True<OfferList>();

                    if (!string.IsNullOrEmpty(getofferobject.keyword))
                    {
                        predicate = predicate.And(x => x.itemname.ToLower().Contains(getofferobject.keyword) || x.OfferName.ToLower().Contains(getofferobject.keyword) || x.OfferCode.ToLower().Contains(getofferobject.keyword));

                    }

                    if (getofferobject.warehouseid != null)
                    {
                        predicate = predicate.And(x => x.WarehouseId == getofferobject.warehouseid);
                    }
                    if (getofferobject.ShowType != -1)
                    {
                        bool status = Convert.ToBoolean(getofferobject.ShowType);
                        predicate = predicate.And(x => x.IsActive == status);
                    }

                    predicate = predicate.And(x => !x.IsDeleted);
                    predicate = predicate.And(x => x.OfferOn == "Item");

                    if (getofferobject.DateFrom.HasValue && getofferobject.DateTo.HasValue)
                    {
                        predicate = predicate.And(x => EntityFunctions.TruncateTime(x.CreatedDate) >= EntityFunctions.TruncateTime(getofferobject.DateFrom) && EntityFunctions.TruncateTime(x.CreatedDate) <= EntityFunctions.TruncateTime(getofferobject.DateTo));
                    }

                    try
                    {
                        var query = from ofr in context.OfferDb
                                    join wr in context.Warehouses
                                    on ofr.WarehouseId equals wr.WarehouseId
                                    select new OfferList
                                    {
                                        OfferId = ofr.OfferId,
                                        WarehouseId = ofr.WarehouseId,
                                        WarehouseName = wr.WarehouseName + " " + wr.CityName,
                                        OfferOn = ofr.OfferOn,
                                        OfferName = ofr.OfferName,
                                        OfferCategory = ofr.OfferCategory,
                                        FreeOfferType = ofr.FreeOfferType,
                                        Description = ofr.Description,
                                        itemname = ofr.itemname,
                                        ItemId = ofr.itemId,
                                        MinOrderQuantity = ofr.MinOrderQuantity,
                                        FreeItemName = ofr.FreeItemName,
                                        NoOffreeQuantity = ofr.NoOffreeQuantity,
                                        start = ofr.start,
                                        end = ofr.end,
                                        FreeWalletPoint = ofr.FreeWalletPoint,
                                        DiscountPercentage = ofr.DiscountPercentage,
                                        FreeItemId = ofr.FreeItemId,
                                        IsActive = ofr.IsActive,
                                        QtyAvaiable = ofr.QtyAvaiable,
                                        QtyConsumed = ofr.QtyConsumed,
                                        MaxQtyPersonCanTake = ofr.MaxQtyPersonCanTake,
                                        OfferWithOtherOffer = ofr.OfferWithOtherOffer,
                                        OfferVolume = ofr.OfferVolume,
                                        FreeItemMRP = ofr.FreeItemMRP,
                                        IsDeleted = ofr.IsDeleted,
                                        CreatedDate = ofr.CreatedDate,
                                        UpdateDate = ofr.UpdateDate,
                                        OfferCode = ofr.OfferCode,
                                        BillDiscountOfferOn = ofr.BillDiscountOfferOn,
                                        BillDiscountWallet = ofr.BillDiscountWallet,
                                        BillAmount = ofr.BillAmount,
                                        FreeItemLimit = ofr.FreeItemLimit,
                                        OfferAppType = ofr.OfferAppType,
                                        //Add by Anoop 28/01/2021
                                        ApplyType = ofr.ApplyType,
                                        IsFreebiesLevel = ofr.IsFreebiesLevel
                                    };
                        //List<OfferList> offers = query.Where(predicate).OrderByDescending(x => x.CreatedDate).ToList();

                        OfferPaggingData.total_count = query.Where(predicate).Count();

                        OfferPaggingData.OfferListDTO = query.Where(predicate).OrderByDescending(x => x.CreatedDate).Skip((getofferobject.page - 1) * getofferobject.totalitem).Take(getofferobject.totalitem).ToList();

                        return OfferPaggingData;
                    }
                    catch (Exception ee)
                    {
                        return null;
                    }

                }
                catch (Exception ex)
                {
                    logger.Error("Error in Offer " + ex.Message);
                    logger.Info("End  Offer: ");
                    return null;
                }
            }
        }

        //For Getting list of Offer by warehouse //tejas
        [Authorize]
        [Route("getOfferOnWarehouse")]
        [HttpGet]
        public List<OfferList> getOfferOnWarehouse(int WarehouseId)
        {
            logger.Info("start Offer: ");
            List<OfferList> OfferList = new List<OfferList>();
            using (var context = new AuthContext())
            {
                try
                {

                    logger.Info("End Get warehouseID: ");
                    if (WarehouseId > 0)
                    {

                        OfferList = context.GetAllOfferWArehouseID(WarehouseId).ToList();
                        logger.Info("End  Offer: ");
                        return OfferList;
                    }
                    else
                    {
                        return null;
                    }

                }
                catch (Exception ex)
                {
                    logger.Error("Error in Offer " + ex.Message);
                    logger.Info("End  Offer: ");
                    return null;
                }
            }
        }


        //For Getting list of Offer by warehouse //tejas
        [Authorize]
        [Route("billByWarehosue")]
        [HttpGet]
        public List<OfferList> billByWarehosue(int WarehouseId)
        {

            logger.Info("start Offer: ");
            List<OfferList> OfferList = new List<OfferList>();
            using (var context = new AuthContext())
            {
                try
                {
                    logger.Info("End Get Company: ");
                    if (WarehouseId > 0)
                    {

                        OfferList = context.getAllOfferWarehouseIDBill(WarehouseId).ToList();
                        logger.Info("End  Offer: ");
                        return OfferList;
                    }
                    else
                    {

                        return null;
                    }

                }
                catch (Exception ex)
                {
                    logger.Error("Error in Offer " + ex.Message);
                    logger.Info("End  Offer: ");
                    return null;
                }
            }
        }


        //For Getting list of Offer by warehouse //tejas
        [Authorize]
        [Route("offerbyWIDandDate")]
        [HttpGet]
        public List<OfferList> offerbyWIDandDate(int WarehouseId, DateTime? fromV1, DateTime? to)
        {

            logger.Info("start Offer: ");
            List<OfferList> OfferList = new List<OfferList>();
            using (var context = new AuthContext())
            {
                logger.Info("End Get Company: ");
                if (WarehouseId > 0)
                {
                    try
                    {

                        var predicate = PredicateBuilder.True<Offer>();
                        var warepredicate = PredicateBuilder.True<Warehouse>();
                        predicate = predicate.And(x => x.IsActive);
                        predicate = predicate.And(x => !x.IsDeleted);
                        predicate = predicate.And(o => (o.OfferOn == "BillDiscount" || o.OfferOn == "ScratchBillDiscount"));

                        warepredicate = warepredicate.And(x => !x.Deleted);

                        if (WarehouseId > 0)
                        {
                            predicate = predicate.And(x => x.WarehouseId == WarehouseId);
                            warepredicate = warepredicate.And(x => x.WarehouseId == WarehouseId);
                        }



                        if (fromV1.HasValue && to.HasValue)
                            predicate = predicate.And(x => EntityFunctions.TruncateTime(x.CreatedDate) >= EntityFunctions.TruncateTime(fromV1) && EntityFunctions.TruncateTime(x.CreatedDate) <= EntityFunctions.TruncateTime(to));

                        var warehouse = context.Warehouses.Where(warepredicate).Select(x => new
                        {
                            WarehouseId = x.WarehouseId,
                            WarehouseName = x.WarehouseName + " " + x.CityName
                        }).ToList();

                        List<OfferList> offers = context.OfferDb.Where(predicate).Select(o => new OfferList
                        {
                            OfferId = o.OfferId,
                            WarehouseId = o.WarehouseId,
                            // WarehouseName = w.WarehouseName + " " + w.CityName,
                            OfferOn = o.OfferOn,
                            OfferName = o.OfferName,
                            OfferCategory = o.OfferCategory,
                            FreeOfferType = o.FreeOfferType,
                            Description = o.Description,
                            itemname = o.itemname,
                            ItemId = o.itemId,
                            MinOrderQuantity = o.MinOrderQuantity,
                            FreeItemName = o.FreeItemName,
                            NoOffreeQuantity = o.NoOffreeQuantity,
                            start = o.start,
                            end = o.end,
                            FreeWalletPoint = o.FreeWalletPoint,
                            DiscountPercentage = o.DiscountPercentage,
                            FreeItemId = o.FreeItemId,
                            IsActive = o.IsActive,
                            QtyAvaiable = o.QtyAvaiable,
                            QtyConsumed = o.QtyConsumed,
                            MaxQtyPersonCanTake = o.MaxQtyPersonCanTake,
                            OfferWithOtherOffer = o.OfferWithOtherOffer,
                            OfferVolume = o.OfferVolume,
                            FreeItemMRP = o.FreeItemMRP,
                            IsDeleted = o.IsDeleted,
                            CreatedDate = o.CreatedDate,
                            UpdateDate = o.UpdateDate,
                            OfferCode = o.OfferCode,
                            BillDiscountOfferOn = o.BillDiscountOfferOn,
                            BillDiscountWallet = o.BillDiscountWallet,
                            BillAmount = o.BillAmount,
                            FreeItemLimit = o.FreeItemLimit,
                            ApplyType = o.ApplyType
                        }).ToList().OrderByDescending(x => x.CreatedDate).ToList();

                        offers.ForEach(x => x.WarehouseName = x.WarehouseId > 0 && warehouse.Any(y => y.WarehouseId == x.WarehouseId) ?
                        warehouse.FirstOrDefault(y => y.WarehouseId == x.WarehouseId).WarehouseName : "");

                        //List<OfferList> offers = (from o in context.OfferDb
                        //                          join w in context.Warehouses on o.WarehouseId equals w.WarehouseId
                        //                          where o.IsDeleted == false && o.WarehouseId == WarehouseId && o.CreatedDate > fromV1 && o.CreatedDate < to && !(o.OfferOn == "BillDiscount" || o.OfferOn == "ScratchBillDiscount")
                        //                          orderby o.CreatedDate descending
                        //                          select new OfferList
                        //                          {
                        //OfferId = o.OfferId,
                        //                              WarehouseId = o.WarehouseId,
                        //                              WarehouseName = w.WarehouseName + " " + w.CityName,
                        //                              OfferName = o.OfferName,
                        //                              OfferOn = o.OfferOn,
                        //                              OfferCategory = o.OfferCategory,
                        //                              FreeOfferType = o.FreeOfferType,
                        //                              Description = o.Description,
                        //                              ItemId = o.itemId,
                        //                              itemname = o.itemname,
                        //                              MinOrderQuantity = o.MinOrderQuantity,
                        //                              FreeItemName = o.FreeItemName,
                        //                              NoOffreeQuantity = o.NoOffreeQuantity,
                        //                              start = o.start,
                        //                              end = o.end,
                        //                              FreeWalletPoint = o.FreeWalletPoint,
                        //                              DiscountPercentage = o.DiscountPercentage,
                        //                              FreeItemId = o.FreeItemId,
                        //                              IsActive = o.IsActive,
                        //                              QtyAvaiable = o.QtyAvaiable,
                        //                              QtyConsumed = o.QtyConsumed,
                        //                              MaxQtyPersonCanTake = o.MaxQtyPersonCanTake,
                        //                              OfferWithOtherOffer = o.OfferWithOtherOffer,
                        //                              OfferVolume = o.OfferVolume,
                        //                              FreeItemMRP = o.FreeItemMRP,
                        //                              IsDeleted = o.IsDeleted,
                        //                              CreatedDate = o.CreatedDate,
                        //                              UpdateDate = o.UpdateDate,
                        //                              OfferCode = o.OfferCode,
                        //                              BillDiscountOfferOn = o.BillDiscountOfferOn,
                        //                              BillDiscountWallet = o.BillDiscountWallet,
                        //                              BillAmount = o.BillAmount,                                                     
                        //                              MaxBillAmount = o.MaxBillAmount,
                        //                              MaxDiscount = o.MaxDiscount,
                        //                              IsMultiTimeUse = o.IsMultiTimeUse,
                        //                              IsUseOtherOffer = o.IsUseOtherOffer,
                        //                              GroupId = o.GroupId,
                        //                              FreeItemLimit = o.FreeItemLimit
                        //                          }).ToList();
                        return offers;
                    }
                    catch (Exception ee)
                    {
                        logger.Error("Error in Offer " + ee.Message);
                        logger.Info("End  Offer: ");
                        return null;
                    }
                }
                else
                {
                    return null;
                }
            }
        }


        //For Getting list of Offer by warehouse //tejas
        [Authorize]
        [Route("billbyWIDandDate")]
        [HttpGet]
        public List<OfferList> billbyWIDandDate(int? WarehouseId, DateTime? fromV2, DateTime? to, string DiscountType)
        {
            logger.Info("start Offer: ");
            List<OfferList> OfferList = new List<OfferList>();
            using (var context = new AuthContext())
            {
                logger.Info("End Get Company: ");

                try
                {
                    var predicate = PredicateBuilder.True<Offer>();
                    var warepredicate = PredicateBuilder.True<Warehouse>();
                    predicate = predicate.And(x => x.IsActive);
                    predicate = predicate.And(x => !x.IsDeleted);
                    predicate = predicate.And(o => (o.OfferOn == "BillDiscount" || o.OfferOn == "ScratchBillDiscount"));

                    warepredicate = warepredicate.And(x => !x.Deleted);

                    if (WarehouseId.HasValue && WarehouseId.Value > 0)
                    {
                        predicate = predicate.And(x => x.WarehouseId == WarehouseId);
                        warepredicate = warepredicate.And(x => x.WarehouseId == WarehouseId);
                    }

                    if (!string.IsNullOrEmpty(DiscountType))
                    {

                        if (DiscountType == "ScratchBillDiscount")
                        {
                            predicate = predicate.And(o => o.OfferOn == DiscountType);
                        }
                        else
                        {
                            predicate = predicate.And(x => x.BillDiscountType == DiscountType);
                        }

                    }

                    if (fromV2.HasValue && to.HasValue)
                        predicate = predicate.And(x => EntityFunctions.TruncateTime(x.CreatedDate) >= EntityFunctions.TruncateTime(fromV2) && EntityFunctions.TruncateTime(x.CreatedDate) <= EntityFunctions.TruncateTime(to));

                    var warehouse = context.Warehouses.Where(warepredicate).Select(x => new
                    {
                        WarehouseId = x.WarehouseId,
                        WarehouseName = x.WarehouseName + " " + x.CityName
                    }).ToList();

                    List<OfferList> offers = context.OfferDb.Where(predicate).Select(o => new OfferList
                    {
                        OfferId = o.OfferId,
                        BillDiscountType = o.BillDiscountType,
                        WarehouseId = o.WarehouseId,
                        //WarehouseName = w.WarehouseName + " " + w.CityName,
                        OfferName = o.OfferName,
                        OfferOn = o.OfferOn,
                        OfferCategory = o.OfferCategory,
                        FreeOfferType = o.FreeOfferType,
                        Description = o.Description,
                        ItemId = o.itemId,
                        itemname = o.itemname,
                        MinOrderQuantity = o.MinOrderQuantity,
                        FreeItemName = o.FreeItemName,
                        NoOffreeQuantity = o.NoOffreeQuantity,
                        start = o.start,
                        end = o.end,
                        FreeWalletPoint = o.FreeWalletPoint,
                        DiscountPercentage = o.DiscountPercentage,
                        FreeItemId = o.FreeItemId,
                        IsActive = o.IsActive,
                        QtyAvaiable = o.QtyAvaiable,
                        QtyConsumed = o.QtyConsumed,
                        MaxQtyPersonCanTake = o.MaxQtyPersonCanTake,
                        OfferWithOtherOffer = o.OfferWithOtherOffer,
                        OfferVolume = o.OfferVolume,
                        FreeItemMRP = o.FreeItemMRP,
                        IsDeleted = o.IsDeleted,
                        CreatedDate = o.CreatedDate,
                        UpdateDate = o.UpdateDate,
                        OfferCode = o.OfferCode,
                        BillDiscountOfferOn = o.BillDiscountOfferOn,
                        BillDiscountWallet = o.BillDiscountWallet,
                        BillAmount = o.BillAmount,
                        MaxBillAmount = o.MaxBillAmount,
                        MaxDiscount = o.MaxDiscount,
                        IsMultiTimeUse = o.IsMultiTimeUse,
                        IsUseOtherOffer = o.IsUseOtherOffer,
                        GroupId = o.GroupId,
                        FreeItemLimit = o.FreeItemLimit,
                        ApplyType = o.ApplyType
                    }).ToList().OrderByDescending(x => x.CreatedDate).ToList();

                    offers.ForEach(x => x.WarehouseName = x.WarehouseId > 0 && warehouse.Any(y => y.WarehouseId == x.WarehouseId) ? warehouse.FirstOrDefault(y => y.WarehouseId == x.WarehouseId).WarehouseName : "");
                    return offers;
                }
                catch (Exception ee)
                {
                    logger.Error("Error in Offer " + ee.Message);
                    logger.Info("End  Offer: ");
                    return null;
                }

            }

        }
        [Authorize]
        [Route("Bill")]
        [HttpGet]
        public List<OfferList> GetBill()
        {

            logger.Info("start Offer: ");
            List<OfferList> OfferList = new List<OfferList>();
            using (var context = new AuthContext())
            {
                try
                {
                    var identity = User.Identity as ClaimsIdentity;
                    int compid = 0, userid = 0;
                    int Warehouse_id = 0;
                    string email = "";
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
                        if (claim.Type == "email")
                        {
                            email = claim.Value;
                        }
                    }
                    logger.Info("End Get Company: ");
                    if (Warehouse_id > 0)
                    {

                        OfferList = context.GetAllOfferWidBill(compid, Warehouse_id).ToList();
                        logger.Info("End  Offer: ");
                        return OfferList;
                    }
                    else
                    {
                        OfferList = context.GetAllOfferBIll(compid).ToList();
                        logger.Info("End  Offer: ");
                        return OfferList;
                    }

                }
                catch (Exception ex)
                {
                    logger.Error("Error in Offer " + ex.Message);
                    logger.Info("End  Offer: ");
                    return null;
                }
            }
        }

        [Authorize]
        [Route("GetBillDiscount")]
        [HttpPost]

        public BillPaggingData GetBillDiscount(GetBillObject GetBillObject)
        {
            logger.Info("start Offer: ");
            BillPaggingData BillPaggingData = new BillPaggingData();
            List<OfferList> OfferList = new List<OfferList>();
            using (var context = new AuthContext())
            {
                try
                {
                    var predicate = PredicateBuilder.True<OfferList>();

                    if (!string.IsNullOrEmpty(GetBillObject.keyword))
                    {
                        predicate = predicate.And(x => x.itemname.ToLower().Contains(GetBillObject.keyword) || x.OfferName.ToLower().Contains(GetBillObject.keyword) || x.OfferCode.ToLower().Contains(GetBillObject.keyword));
                    }

                    if (GetBillObject.warehouseid != null)
                    {
                        predicate = predicate.And(x => x.WarehouseId == GetBillObject.warehouseid);
                    }

                    if (GetBillObject.ShowType != -1)
                    {
                        bool status = Convert.ToBoolean(GetBillObject.ShowType);
                        predicate = predicate.And(x => x.IsActive == status);
                    }

                    if (!string.IsNullOrEmpty(GetBillObject.Types))
                    {

                        if (GetBillObject.Types == "ScratchBillDiscount")
                        {
                            predicate = predicate.And(x => x.OfferOn == GetBillObject.Types);
                        }
                        else
                        {
                            predicate = predicate.And(x => x.BillDiscountType == GetBillObject.Types);
                        }
                    }
                    predicate = predicate.And(x => !x.IsDeleted);
                    predicate = predicate.And(x => x.OfferOn == "BillDiscount" || x.OfferOn == "ScratchBillDiscount");

                    if (GetBillObject.DateFrom.HasValue && GetBillObject.DateTo.HasValue)
                    {
                        predicate = predicate.And(x => EntityFunctions.TruncateTime(x.CreatedDate) >= EntityFunctions.TruncateTime(GetBillObject.DateFrom) && EntityFunctions.TruncateTime(x.CreatedDate) <= EntityFunctions.TruncateTime(GetBillObject.DateTo));
                    }
                    try
                    {
                        var query = from ofr in context.OfferDb
                                    join wr in context.Warehouses
                                    on ofr.WarehouseId equals wr.WarehouseId
                                    select new OfferList
                                    {
                                        OfferId = ofr.OfferId,
                                        BillDiscountType = ofr.BillDiscountType,
                                        WarehouseId = ofr.WarehouseId,
                                        WarehouseName = wr.WarehouseName + " " + wr.CityName,
                                        OfferName = ofr.OfferName,
                                        OfferOn = ofr.OfferOn,
                                        OfferCategory = ofr.OfferCategory,
                                        FreeOfferType = ofr.FreeOfferType,
                                        Description = ofr.Description,
                                        ItemId = ofr.itemId,
                                        itemname = ofr.itemname,
                                        MinOrderQuantity = ofr.MinOrderQuantity,
                                        FreeItemName = ofr.FreeItemName,
                                        NoOffreeQuantity = ofr.NoOffreeQuantity,
                                        start = ofr.start,
                                        end = ofr.end,
                                        FreeWalletPoint = ofr.FreeWalletPoint,
                                        DiscountPercentage = ofr.DiscountPercentage,
                                        FreeItemId = ofr.FreeItemId,
                                        IsActive = ofr.IsActive,
                                        QtyAvaiable = ofr.QtyAvaiable,
                                        QtyConsumed = ofr.QtyConsumed,
                                        MaxQtyPersonCanTake = ofr.MaxQtyPersonCanTake,
                                        OfferWithOtherOffer = ofr.OfferWithOtherOffer,
                                        OfferVolume = ofr.OfferVolume,
                                        FreeItemMRP = ofr.FreeItemMRP,
                                        IsDeleted = ofr.IsDeleted,
                                        CreatedDate = ofr.CreatedDate,
                                        UpdateDate = ofr.UpdateDate,
                                        OfferCode = ofr.OfferCode,
                                        BillDiscountOfferOn = ofr.BillDiscountOfferOn,
                                        BillDiscountWallet = ofr.BillDiscountWallet,
                                        BillAmount = ofr.BillAmount,
                                        MaxBillAmount = ofr.MaxBillAmount,
                                        MaxDiscount = ofr.MaxDiscount,
                                        IsMultiTimeUse = ofr.IsMultiTimeUse,
                                        IsUseOtherOffer = ofr.IsUseOtherOffer,
                                        GroupId = ofr.GroupId,
                                        FreeItemLimit = ofr.FreeItemLimit,
                                        OfferAppType = ofr.OfferAppType,
                                        ApplyOn = ofr.ApplyOn,
                                        WalletType = ofr.WalletType,
                                        StoreId = ofr.StoreId,
                                        // Add by Anoop 28/01/2021
                                        ApplyType = ofr.ApplyType,
                                        LineItem = ofr.LineItem,
                                        ExcludeGroupId = ofr.ExcludeGroupId,
                                        CombinedGroupId = ofr.CombinedGroupId
                                    };
                        //List<OfferList> offers = query.Where(predicate).OrderByDescending(x => x.CreatedDate).ToList();

                        BillPaggingData.total_count = query.Where(predicate).Count();

                        BillPaggingData.BillListDTO = query.Where(predicate).OrderByDescending(x => x.CreatedDate).Skip((GetBillObject.page - 1) * GetBillObject.totalitem).Take(GetBillObject.totalitem).ToList();

                        
                        if (BillPaggingData.BillListDTO != null)
                        {
                            List<ExclusiveOfferGroup> CombinedGroupList = new List<ExclusiveOfferGroup>();
                            //List<MinSalesGroup> GroupList = new List<MinSalesGroup>();
                            List<AngularJSAuthentication.Model.SalesApp.SalesGroup> GroupList = new List<AngularJSAuthentication.Model.SalesApp.SalesGroup>();
                            if (BillPaggingData.BillListDTO.Any(x=>x.CombinedGroupId > 0))
                            {
                               CombinedGroupList = GetAllExclusivegroup();
                            }
                            if(BillPaggingData.BillListDTO.Any(x=>x.ExcludeGroupId > 0))
                            {
                                List<long> ids = BillPaggingData.BillListDTO.Where(x => x.ExcludeGroupId > 0).Select(y => Convert.ToInt64(y.ExcludeGroupId)).Distinct().ToList();
                                GroupList = context.SalesGroupDb.Where(x => ids.Contains(x.Id)).ToList();
                                //GroupList = GetNewCustomerGroupListForOffer(null);
                            }
                            var storeids = BillPaggingData.BillListDTO.Select(x => x.StoreId).Distinct().ToList();
                            var Stores = context.StoreDB.Where(x => storeids.Contains(x.Id)).Select(x => new { x.Id, x.Name }).ToList();

                            foreach (var item in BillPaggingData.BillListDTO)
                            {
                                item.StoreName = Stores != null && Stores.Any(x => x.Id == item.StoreId) ? Stores.FirstOrDefault(x => x.Id == item.StoreId).Name : "Company";
                                if (item.CombinedGroupId > 0)
                                {
                                    var combinedgroupdata = CombinedGroupList.FirstOrDefault(x => x.Id == item.CombinedGroupId);
                                    if(combinedgroupdata != null && combinedgroupdata.Name != null)
                                    {
                                        item.CombinedGroupName = combinedgroupdata.Name;
                                    }
                                    //item.CombinedGroupName = CombinedGroupList.FirstOrDefault(x=>x.Id==item.CombinedGroupId).Name;  
                                }
                                if (item.ExcludeGroupId > 0)
                                {
                                    var excludegroupdata = GroupList.FirstOrDefault(x => x.Id == item.ExcludeGroupId);
                                    if(excludegroupdata != null && excludegroupdata.GroupName != null)
                                    {
                                        item.ExcludeGroupName = excludegroupdata.GroupName;
                                    }
                                    //item.ExcludeGroupName = GroupList.FirstOrDefault(x => x.Id == item.ExcludeGroupId).GroupName;
                                }
                            }

                        }

                        return BillPaggingData;
                    }
                    catch (Exception ee)
                    {
                        return null;
                    }

                }
                catch (Exception ex)
                {
                    logger.Error("Error in Offer " + ex.Message);
                    logger.Info("End  Offer: ");
                    return null;
                }
            }
        }

        [Route("")]
        public Offer Get(int id)
        {
            logger.Info("start single User: ");
            Offer offer = new Offer();
            using (var context = new AuthContext())
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
                    offer = context.GetOfferbyId(id, compid);
                    logger.Info("End Get coupon by id: " + offer.OfferId);
                    return offer;
                }
                catch (Exception ex)
                {
                    logger.Error("Error in Get coupon by id " + ex.Message);
                    logger.Info("End  single coupon: ");
                    return null;
                }
            }
        }

        [Route("GetActiveItem")]
        [HttpGet]
        public IEnumerable<ItemMaster> GetActiveItem(int WarehouseId)
        {
            logger.Info("start single User: ");
            List<ItemMaster> item = new List<ItemMaster>();
            using (var context = new AuthContext())
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
                    item = context.GetActiveItemForOffer(compid, WarehouseId).ToList();
                    logger.Info("End Get coupon by id: " + item);
                    return item;
                }
                catch (Exception ex)
                {
                    logger.Error("Error in Get coupon by id " + ex.Message);
                    logger.Info("End  single coupon: ");
                    return null;
                }
            }
        }

        //By Sachin 21.05.2019
        //For showing offer on slider of apphome
        [Route("GetActiveOffer")]
        [HttpGet]
        public List<Offer> GetActiveOffer(int WarehouseId)
        {
            logger.Info("start single User: ");
            List<Offer> offers = new List<Offer>();
            using (var context = new AuthContext())
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
                    offers = context.GetOfferForSliderAppHome(WarehouseId);
                    logger.Info("Get offer " + offers);
                    return offers;
                }
                catch (Exception ex)
                {
                    logger.Error("Error in Get coupon by id " + ex.Message);
                    logger.Info("End  single coupon: ");
                    return null;
                }
            }
        }

        //For creating Offer.
        //Created By Sachin Jaiswal
        [ResponseType(typeof(Offer))]
        [Route("")]
        [AcceptVerbs("POST")]
        public OfferResponseDC add(Offer offer)
        {
            OfferResponseDC offerResponseDC = new OfferResponseDC { status = true, msg = "", Offer = null, ShowValidationSkipmsg = false };
            using (AuthContext context = new AuthContext())
            {
                List<Offer> offers = new List<Offer>();
                var identity = User.Identity as ClaimsIdentity;
                int compid = 0, userid = 0;

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))
                    compid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                    userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

                if (offer == null)
                {
                    throw new ArgumentNullException("offer");
                }
                offer.CompanyId = compid;
                offer.userid = userid;

                try
                {
                    bool Isexists = false;
                    DateTime date1 = offer.start;
                    DateTime date2 = offer.end;
                    RewardItems rewardItems = new RewardItems();
                    ItemMaster itemMaster = new ItemMaster();
                    ItemMaster freeitem = new ItemMaster();
                    bool data = false;
                    List<int> OfferIds = null;
                    List<int> customeridlist = null;
                    List<long> channelidslist = null;
                    if (!string.IsNullOrEmpty(offer.WarehouseIds))
                    {
                        List<int> warehouseids = offer.WarehouseIds.Split(',').Select(x => Convert.ToInt32(x)).ToList();
                        if(offer.ChannelIds != null && offer.ChannelIds != "")
                        {
                            channelidslist = offer.ChannelIds.Split(',').Select(x => Convert.ToInt64(x)).ToList();
                        }
                        List<Warehousedto> warehouses = new List<Warehousedto>();

                        if (offer.StoreId > 0)
                        {
                            if(offer.ChannelIds != null && offer.ChannelIds != "")
                            {
                                if (channelidslist.Any() && channelidslist.Count() > 0)
                                {
                                    string query = "select distinct CustomerId from CustomerChannelMappings where StoreId=" + offer.StoreId + "and ChannelMasterId in (" + offer.ChannelIds + ")and IsActive=1 and IsDeleted=0";
                                    customeridlist = context.Database.SqlQuery<int>(query).ToList();
                                    if (customeridlist.Count() > 0 && customeridlist.Any()) { }
                                    else
                                    {
                                        offerResponseDC.status = false;
                                        offerResponseDC.msg = "Please Assign customer for channel wise customer";
                                        return offerResponseDC;
                                    }
                                }
                            }
                            
                        }

                        if (offer.IsCRMOffer == true)
                        {
                            warehouses = context.Warehouses.Where(y => y.Deleted == false && y.active == true && (y.IsKPP == false || y.IsKppShowAsWH == true)).Select(y => new Warehousedto { WarehouseId = y.WarehouseId, CityId = y.Cityid, WarehouseName = y.WarehouseName }).ToList();
                        }
                        else
                        {
                            warehouses = context.Warehouses.Where(x => warehouseids.Contains(x.WarehouseId)).Select(y => new Warehousedto { WarehouseId = y.WarehouseId, WarehouseName = y.WarehouseName }).ToList();
                        }

                        //if (offer.OfferOn == "Item") //BillDiscount
                        if (offer.itemId > 0 && (offer.OfferOn == "Item" || offer.OfferOn == "BillDiscount"))
                        {
                            itemMaster = context.itemMasters.Where(x => x.ItemId == offer.itemId).SingleOrDefault();
                        }
                        foreach (var warehouse in warehouses)
                        {
                            var warehouseitem = new List<ItemMaster>();
                            if (offer.OfferOn == "Item")
                            {
                                if (!offer.IsFreebiesLevel)
                                {
                                    warehouseitem = context.itemMasters.Where(x => x.Number == itemMaster.Number && x.WarehouseId == warehouse.WarehouseId).ToList();
                                }
                                else
                                {
                                    warehouseitem = context.itemMasters.Where(x => x.SellingSku == itemMaster.SellingSku && x.WarehouseId == warehouse.WarehouseId).Take(1).ToList();
                                }
                                var itemids = warehouseitem.Select(x => x.ItemId).Distinct().ToList();
                                if (offer.OfferAppType != "Distributor App")
                                    data = context.OfferDb.Any(x => itemids.Contains(x.itemId) && x.IsDeleted == false && x.start <= date1 && x.end >= date2 && x.WarehouseId == warehouse.WarehouseId && x.OfferOn == "Item" && x.IsActive == true && x.OfferAppType != "Distributor App");
                                else
                                    data = context.OfferDb.Any(x => itemids.Contains(x.itemId) && x.IsDeleted == false && x.start <= date1 && x.end >= date2 && x.WarehouseId == warehouse.WarehouseId && x.OfferOn == "Item" && x.IsActive == true && x.OfferAppType == "Distributor App");

                                if (data)
                                {
                                    offerResponseDC.status = false;
                                    offerResponseDC.msg += (string.IsNullOrEmpty(offerResponseDC.msg) ? "" : "\n") + "This Item Offer already exist for " + warehouse.WarehouseName + ". Please first inactive previous offer.";
                                    offerResponseDC.Offer = offer;
                                }
                            }
                            if ((offer.OfferOn == "BillDiscount" || offer.OfferOn == "ScratchBillDiscount") && !offer.SkipValidation)
                            {
                                var predicate = PredicateBuilder.True<Offer>();
                                predicate = predicate.And(x => x.IsDeleted == false && x.IsActive == true && x.BillAmount == offer.BillAmount && x.MaxDiscount == offer.MaxDiscount && x.LineItem == offer.LineItem && x.WarehouseId == warehouse.WarehouseId && x.OfferOn == "BillDiscount" && x.BillDiscountType == offer.BillDiscountType && x.start <= date1 && x.end >= date2);
                                if (offer.BillDiscountType == "category")
                                {
                                    var Categoryids = offer.BillDiscountOfferSections.Select(x => x.ObjId);
                                    predicate = predicate.And(x => x.BillDiscountOfferSections.Any(y => Categoryids.Contains(y.ObjId)));
                                }
                                else if (offer.BillDiscountType == "store")
                                {
                                    var Categoryids = offer.BillDiscountOfferSections.Select(x => x.ObjId);
                                    predicate = predicate.And(x => x.BillDiscountOfferSections.Any(y => Categoryids.Contains(y.ObjId)));
                                }
                                else if (offer.BillDiscountType == "subcategory")
                                {
                                    var SubCategoryMappingids = offer.BillDiscountOfferSections.Select(x => x.ObjId);
                                    predicate = predicate.And(x => x.BillDiscountOfferSections.Any(y => SubCategoryMappingids.Contains(y.ObjId)));
                                }
                                else if (offer.BillDiscountType == "brand")
                                {
                                    var BrandMappingids = offer.BillDiscountOfferSections.Select(x => x.ObjId);
                                    predicate = predicate.And(x => x.BillDiscountOfferSections.Any(y => BrandMappingids.Contains(y.ObjId)));
                                }

                                if (offer.BillDiscountOfferOn == "Percentage")
                                {
                                    predicate = predicate.And(x => x.BillDiscountOfferOn == "Percentage");
                                }
                                else if (offer.BillDiscountOfferOn == "FreeItem")
                                {
                                    if (offer.BillDiscountFreeItems == null || !offer.BillDiscountFreeItems.Any())
                                    {
                                        offerResponseDC.status = false;
                                        offerResponseDC.msg += (string.IsNullOrEmpty(offerResponseDC.msg) ? "" : "\n") + "Please add atleast on free item.";
                                        offerResponseDC.Offer = offer;
                                    }
                                }
                                else
                                {
                                    predicate = predicate.And(x => x.BillDiscountOfferOn == "WalletPoint");
                                }
                                OfferIds = context.OfferDb.Where(predicate).Select(x => x.OfferId).ToList();
                                data = OfferIds.Any();
                                if (data)
                                {
                                    offerResponseDC.status = false;
                                    offerResponseDC.ShowValidationSkipmsg = true;
                                    offerResponseDC.msg += (string.IsNullOrEmpty(offerResponseDC.msg) ? "" : "\n") + "This " + offer.BillDiscountType + " Offer already exist for " + warehouse.WarehouseName + ". Please first inactive previous offer.";
                                    offerResponseDC.Offer = offer;
                                }
                            }
                            if ((offer.OfferOn == "ScratchBillDiscount" || offer.OfferOn == "BillDiscount") && OfferIds != null && !offer.SkipValidation)
                            {
                                if (offer.BillDiscountOfferOn != "FreeItem")
                                {
                                    if (offer.CustomerId > 0)
                                    {
                                        offer.ApplyType = "Customer";
                                        bool exists = OfferIds.Any() ? context.BillDiscountDb.Any(x => x.CustomerId == offer.CustomerId && x.BillDiscountType == offer.OfferOn && OfferIds.Contains(x.OfferId)) : false;
                                        if (exists)
                                        {
                                            Isexists = true;
                                        }
                                    }
                                    else if (offer.GroupId > 0)
                                    {
                                        offer.ApplyType = "Group";
                                        var groupcustomer = context.GroupMappings.Where(x => x.GroupID == offer.GroupId && x.WarehouseID == warehouse.WarehouseId).Select(x => x.CustomerID);
                                        bool exists = false;
                                        if (groupcustomer != null && groupcustomer.Any())
                                        {
                                            exists = OfferIds.Any() ? context.BillDiscountDb.Any(x => groupcustomer.Contains(x.CustomerId) && x.BillDiscountType == offer.OfferOn && OfferIds.Contains(x.OfferId)) : false;
                                        }
                                        if (exists)
                                        {
                                            Isexists = true;
                                        }

                                    }
                                    else if (offer.CustomerId == -1)
                                    {
                                        offer.ApplyType = "Warehouse";
                                    }
                                    else if (offer.CustomerId == -2)
                                    {
                                        offer.ApplyType = "KPPCustomer";
                                        var groupcustomer = context.Customers.Where(x => x.Warehouseid.Value == warehouse.WarehouseId && x.IsKPP).Select(x => x.CustomerId).ToList();
                                        bool exists = false;
                                        if (groupcustomer != null && groupcustomer.Any())
                                        {
                                            exists = OfferIds.Any() ? context.BillDiscountDb.Any(x => groupcustomer.Contains(x.CustomerId) && x.BillDiscountType == offer.OfferOn && OfferIds.Contains(x.OfferId)) : false;
                                        }
                                        if (exists)
                                        {
                                            Isexists = true;
                                        }

                                    }
                                    else if (offer.CustomerId < -2)
                                    {
                                        offer.ApplyType = "Level";
                                        Isexists = true;
                                    }
                                }
                                if (Isexists)
                                {
                                    offerResponseDC.status = false;
                                    offerResponseDC.ShowValidationSkipmsg = true;
                                    // offerResponseDC.msg += "This " + offer.BillDiscountType + "-" + offer.ApplyType + " Offer already exist for " + warehouse.WarehouseName + ". Please first inactive previous offer.";
                                    offerResponseDC.Offer = offer;
                                }
                            }

                            if ((offer.OfferOn == "ScratchBillDiscount" || offer.OfferOn == "BillDiscount") && !offer.SkipValidation)
                            {
                                if (offer.BillDiscountOfferOn != "FreeItem")
                                {
                                    if (offer.CustomerId > 0)
                                    {
                                        offer.ApplyType = "Customer";
                                    }
                                    else if (offer.GroupId > 0)
                                    {
                                        offer.ApplyType = "Group";
                                    }
                                    else if (offer.CustomerId == -1)
                                    {
                                        offer.ApplyType = "Warehouse";
                                    }
                                    else if (offer.CustomerId == -2)
                                    {
                                        offer.ApplyType = "KPPCustomer";
                                    }
                                    else if (offer.CustomerId < -2)
                                    {
                                        offer.ApplyType = "Level";
                                    }
                                }
                            }
                        }


                        if (offerResponseDC.status)
                        {
                            List<KeyValuePair<int, int>> items = new List<KeyValuePair<int, int>>();
                            if (offer.OfferItemsBillDiscounts != null && offer.OfferItemsBillDiscounts.Any())
                            {
                                var billDiscountItemids = offer.OfferItemsBillDiscounts.Select(x => x.itemId).ToList();
                                var itemMultiMrpIds = context.itemMasters.Where(x => billDiscountItemids.Contains(x.ItemId)).Select(x => x.ItemMultiMRPId).ToList();
                                var Dbitems = context.itemMasters.Where(x => itemMultiMrpIds.Contains(x.ItemMultiMRPId) && warehouseids.Contains(x.WarehouseId)).Select(x => new { x.ItemId, x.WarehouseId }).ToList();
                                foreach (var item in Dbitems)
                                {
                                    items.Add(new KeyValuePair<int, int>(item.WarehouseId, item.ItemId));
                                }
                            }
                            if (offer.OfferOn == "Item")
                            {
                                if (offer.FreeOfferType == "ItemMaster")
                                {
                                    freeitem = context.itemMasters.Where(x => x.ItemId == offer.FreeItemId).SingleOrDefault();
                                }
                            }
                            int i = 0;
                            foreach (var warehouse in warehouses)
                            {
                                Offer Newoffer = new Offer();

                                Newoffer.IsCRMOffer = offer.IsCRMOffer;

                                Newoffer.CompanyId = compid;
                                Newoffer.StoreId = offer.StoreId;
                                Newoffer.userid = userid;
                                Newoffer.WarehouseId = warehouse.WarehouseId;
                                // offer.WarehouseId = warehouse.WarehouseId;                               
                                if ((offer.FreeOfferType != "DreamItem" || offer.FreeOfferType != "ItemMaster") && (offer.OfferOn == "BillDiscount" || offer.OfferOn == "ScratchBillDiscount") && (offer.OfferAppType == "Retailer App" || offer.OfferAppType == "Sales App" || offer.OfferAppType == "Both"))
                                {

                                }
                                else
                                {
                                    if (offer.FreeOfferType == "ItemMaster")
                                    {

                                        Newoffer.FreeItemName = freeitem.itemname;
                                        Newoffer.FreeItemMRP = freeitem.price;
                                        Newoffer.QtyAvaiable = Convert.ToDouble(offer.FreeItemLimit);
                                        Newoffer.IsDispatchedFreeStock = offer.IsDispatchedFreeStock;
                                        Newoffer.ApplyType = "Warehouse";
                                    }
                                    // Offer ApplyType As UserType 
                                    if (itemMaster != null)
                                    {
                                        Newoffer.itemname = itemMaster.itemname;
                                    }
                                    else
                                    {
                                        Newoffer.itemname = offer.itemname;
                                    }

                                }
                                if (offer.CustomerId > 0)
                                {
                                    Newoffer.ApplyType = "Customer";
                                }//Customer Base
                                else if (offer.GroupId > 0)
                                {

                                    Newoffer.ApplyType = "Group";
                                }  // Group base
                                else if (offer.CustomerId == -1)
                                {

                                    Newoffer.ApplyType = "Warehouse";
                                } // All Warehouse
                                else if (offer.CustomerId == -2)
                                {
                                    Newoffer.ApplyType = "KPPCustomer";
                                } // KPP 
                                else if (offer.CustomerId == -9)
                                {
                                    Newoffer.ApplyType = "PrimeCustomer";
                                }//Prime Customer
                                else if (offer.CustomerId < -2)
                                {
                                    Newoffer.ApplyType = "Level";
                                }//level Customer

                                if (Newoffer.IsCRMOffer)
                                {
                                    Newoffer.ApplyType = "Customer";
                                }
                                if(offer.ChannelIds != null && offer.ChannelIds != "")
                                {
                                    if (offer.StoreId > 0 && channelidslist.Count() > 0 && channelidslist != null)
                                    {
                                        Newoffer.ApplyType = "Customer";
                                        Newoffer.ChannelIds = offer.ChannelIds;
                                    }
                                }
                                

                                Newoffer.start = date1;
                                Newoffer.end = date2;
                                Newoffer.itemId = offer.itemId;
                                Newoffer.IsDeleted = false;
                                Newoffer.IsActive = offer.IsActive;
                                Newoffer.OfferLogoUrl = offer.OfferLogoUrl;
                                Newoffer.CreatedDate = indianTime;
                                Newoffer.UpdateDate = indianTime;
                                Newoffer.OfferCode = offer.OfferCode;
                                Newoffer.Description = offer.Description;
                                Newoffer.DiscountPercentage = offer.DiscountPercentage;
                                Newoffer.OfferName = offer.OfferName;
                                Newoffer.OfferWithOtherOffer = offer.OfferWithOtherOffer;
                                Newoffer.BillDiscountOfferOn = offer.BillDiscountOfferOn;
                                Newoffer.BillDiscountWallet = offer.BillDiscountWallet;
                                Newoffer.IsMultiTimeUse = offer.IsMultiTimeUse;
                                Newoffer.IsUseOtherOffer = offer.IsUseOtherOffer;
                                Newoffer.OfferOn = offer.OfferOn;
                                Newoffer.FreeOfferType = offer.FreeOfferType;
                                Newoffer.FreeItemLimit = offer.FreeItemLimit; // add Item limit
                                Newoffer.OfferUseCount = offer.OfferUseCount;
                                Newoffer.OffersaleQty = 0;
                                Newoffer.Category = offer.Category;
                                Newoffer.subCategory = offer.subCategory;
                                Newoffer.subSubCategory = offer.subSubCategory;
                                Newoffer.OfferAppType = offer.OfferAppType;
                                Newoffer.ApplyOn = offer.ApplyOn;
                                Newoffer.WalletType = offer.WalletType;
                                Newoffer.BillAmount = offer.BillAmount;
                                Newoffer.BillDiscountFreeItems = offer.BillDiscountFreeItems;
                                Newoffer.BillDiscountType = offer.BillDiscountType;
                                Newoffer.CityId = offer.CityId > 0 ? offer.CityId : warehouse.CityId ?? 0;
                                Newoffer.Description = offer.Description;
                                Newoffer.DistributorDiscountAmount = offer.DistributorDiscountAmount;
                                Newoffer.DistributorDiscountPercentage = offer.DistributorDiscountPercentage;
                                Newoffer.DistributorOfferType = offer.DistributorOfferType;
                                Newoffer.FreeItemId = offer.FreeItemId;
                                Newoffer.FreeItemMRP = freeitem.MRP;
                                Newoffer.FreeItemName = freeitem.itemname;
                                Newoffer.FreeOfferType = offer.FreeOfferType;
                                Newoffer.FreeWalletPoint = offer.FreeWalletPoint;
                                Newoffer.GroupId = offer.GroupId;
                                Newoffer.IsDispatchedFreeStock = offer.IsDispatchedFreeStock;
                                Newoffer.IsOfferOnCart = offer.IsOfferOnCart;
                                Newoffer.ItemNumber = offer.ItemNumber;
                                Newoffer.LineItem = offer.LineItem;
                                Newoffer.MaxBillAmount = offer.MaxBillAmount;
                                Newoffer.MaxDiscount = offer.MaxDiscount;
                                Newoffer.MaxQtyPersonCanTake = offer.MaxQtyPersonCanTake;
                                Newoffer.MinOrderQuantity = offer.MinOrderQuantity;
                                Newoffer.NoOffreeQuantity = offer.NoOffreeQuantity;
                                Newoffer.OfferAppType = offer.OfferAppType;
                                Newoffer.OfferCategory = offer.OfferCategory;
                                Newoffer.OfferCode = offer.OfferCode;
                                Newoffer.OfferLogoUrl = offer.OfferLogoUrl;
                                Newoffer.IsAutoApply = offer.IsAutoApply;
                                Newoffer.ImagePath = offer.ImagePath;
                                Newoffer.IsPriorityOffer = offer.IsPriorityOffer;
                                //Newoffer.OfferScratchWeights = offer.OfferScratchWeights;

                                Newoffer.ExcludeGroupId = offer.ExcludeGroupId ?? 0;
                                Newoffer.CombinedGroupId = offer.CombinedGroupId ?? 0;
                                Newoffer.IsBillDiscountFreebiesItem = offer.IsBillDiscountFreebiesItem == true ? true : false;
                                Newoffer.IsBillDiscountFreebiesValue = offer.IsBillDiscountFreebiesValue == true ? true : false;

                                List<OfferScratchWeight> list = new List<OfferScratchWeight>();
                                if (offer.OfferScratchWeights != null && offer.OfferScratchWeights.Any())
                                {
                                    foreach (var item in offer.OfferScratchWeights)
                                    {
                                        OfferScratchWeight obj = new OfferScratchWeight();
                                        obj.WalletPoint = item.WalletPoint;
                                        obj.Weight = item.Weight;
                                        obj.Id = item.Id;
                                        obj.Offer = item.Offer;
                                        obj.offerId = item.offerId;
                                        list.Add(obj);
                                    }
                                }
                                Newoffer.OfferScratchWeights = list;

                                Newoffer.OfferVolume = offer.OfferVolume;
                                Newoffer.QtyAvaiable = Convert.ToDouble(offer.FreeItemLimit);
                                Newoffer.QtyConsumed = offer.QtyConsumed;
                                //Newoffer.ApplyType = offer.ApplyType;
                                if (Newoffer.BillDiscountOfferOn != "DynamicWalletPoint")
                                {
                                    Newoffer.OfferScratchWeights = new List<OfferScratchWeight>();
                                }
                                if (offer.OfferBillDiscountRequiredItems != null && offer.OfferBillDiscountRequiredItems.Any())
                                {

                                    List<OfferBillDiscountRequiredItem> offerBillDiscountRequiredItems = new List<OfferBillDiscountRequiredItem>();

                                    //Newoffer.OfferId = i;
                                    foreach (var item in offer.OfferBillDiscountRequiredItems)
                                    {
                                        OfferBillDiscountRequiredItem reqitem = new OfferBillDiscountRequiredItem
                                        {
                                            Id = item.Id,
                                            ObjectId = item.ObjectId,
                                            ObjectText = item.ObjectText.Length > 1000 ? item.ObjectText.Substring(0, 900) + ".." : item.ObjectText,
                                            ObjectType = item.ObjectType,
                                            ObjectValue = item.ObjectValue,
                                            offerId = item.offerId,
                                            ValueType = item.ValueType
                                        };
                                        //item.offerId = Newoffer.OfferId;                                       
                                        offerBillDiscountRequiredItems.Add(reqitem);
                                    }
                                    Newoffer.OfferBillDiscountRequiredItems = offerBillDiscountRequiredItems;
                                    i--;
                                }
                                else { Newoffer.OfferBillDiscountRequiredItems = new List<OfferBillDiscountRequiredItem>(); }

                                if (offer.OfferOn == "BillDiscount" && offer.OfferLineItemValues != null && offer.OfferLineItemValues.Any())
                                {
                                    List<OfferLineItemValue> offerLineItemValues = new List<OfferLineItemValue>();
                                    foreach (var item in offer.OfferLineItemValues)
                                    {
                                        OfferLineItemValue reqitem = new OfferLineItemValue
                                        {
                                            Id = item.Id,
                                            offerId = item.offerId,
                                            itemValue = item.itemValue
                                        };
                                        offerLineItemValues.Add(reqitem);
                                    }
                                    Newoffer.OfferLineItemValues = offerLineItemValues;
                                }
                                else { Newoffer.OfferLineItemValues = new List<OfferLineItemValue>(); }

                                if (offer.BillDiscountOfferOn == "DynamicAmount")
                                {
                                    Newoffer.ApplyType = "Customer";
                                }
                                offers.Add(Newoffer);

                            }

                            context.OfferDb.AddRange(offers);
                            if (context.Commit() > 0)
                            {
                                var offerbilldiscountitems = new List<OfferItemsBillDiscount>();
                                List<BillDiscountOfferSection> billDiscountOfferSections = new List<BillDiscountOfferSection>();
                                string[] saAllowedCharacters = { "1", "2", "3", "4", "5", "6", "7", "8", "9", "0" };
                                string Ocode = GenerateRandomCRM(6, saAllowedCharacters);

                                if (offer.ScratchCardCustomers != null && offer.ScratchCardCustomers.Any() && offer.BillDiscountOfferOn == "DynamicAmount")
                                {
                                    var skcodes = offer.ScratchCardCustomers.Select(x => x.SkCode).Distinct().ToList();
                                    var customers = context.Customers.Where(x => skcodes.Contains(x.Skcode)).Select(x => new { x.Skcode, x.CustomerId, x.Warehouseid }).ToList();
                                    offer.ScratchCardCustomers.ForEach(x =>
                                    {
                                        if (customers.Any(y => y.Skcode.ToLower() == x.SkCode.ToLower()))
                                        {
                                            var cust = customers.FirstOrDefault(y => y.Skcode.ToLower() == x.SkCode.ToLower());
                                            x.CustomerId = cust.CustomerId;
                                            x.WarehouseId = cust.Warehouseid.Value;
                                        }
                                    });
                                }
                                foreach (var offerdb in offers)
                                {
                                    string code = "";
                                    if (offerdb.OfferOn == "ScratchBillDiscount")
                                    {
                                        code = "SC_";
                                    }
                                    else if (offerdb.OfferOn == "BillDiscount")
                                    {
                                        code = "BD_";
                                    }
                                    else if (offerdb.OfferOn == "Item")
                                    {
                                        code = "ID_";
                                    }
                                    else if (offerdb.BillDiscountType == "ClearanceStock")
                                    {
                                        code = "CL_";
                                    }
                                    if (string.IsNullOrEmpty(offer.OfferCode) && offer.IsCRMOffer == false)
                                    {
                                        string offerCode = code + offerdb.OfferId;
                                        offerdb.OfferCode = offerCode;
                                    }

                                    if (string.IsNullOrEmpty(offer.OfferCode) && offer.IsCRMOffer)
                                    {
                                        offerdb.OfferCode = "CRM_" + Ocode;
                                    }

                                    if (items != null && items.Any(x => x.Key == offerdb.WarehouseId))
                                    {
                                        foreach (var item in items.Where(x => x.Key == offerdb.WarehouseId))
                                        {
                                            offerbilldiscountitems.Add(new OfferItemsBillDiscount
                                            {
                                                OfferId = offerdb.OfferId,
                                                itemId = item.Value,
                                                IsInclude = offer.BillDiscountType == "items"
                                            });
                                        }
                                    }

                                    if (offer.BillDiscountOfferSections != null && offer.BillDiscountOfferSections.Any())
                                    {
                                        foreach (var item in offer.BillDiscountOfferSections)
                                        {
                                            billDiscountOfferSections.Add(new BillDiscountOfferSection
                                            {
                                                IsInclude = item.IsInclude,
                                                ObjId = item.ObjId,
                                                OfferId = offerdb.OfferId
                                            });
                                        }

                                    }
                                    offerdb.IncentiveClassification = offer.IncentiveClassification;// by Sudhir 14-06-2023

                                    if (offerdb.OfferOn == "ScratchBillDiscount" || offerdb.OfferOn == "BillDiscount")
                                    {
                                        if (offerdb.OfferId > 0 && (offer.CustomerId > 0 || offer.BillDiscountOfferOn == "DynamicAmount"))
                                        {
                                            if (offerdb.BillDiscountOfferOn != "DynamicAmount")
                                            {
                                                double billAmount = 0;
                                                if (offerdb.BillDiscountOfferOn == "DynamicWalletPoint")
                                                {
                                                    WeightedRandomBag<int> itemDrops = new WeightedRandomBag<int>();
                                                    foreach (var item in offerdb.OfferScratchWeights)
                                                    {
                                                        itemDrops.AddEntry(item.WalletPoint, item.Weight);
                                                    }
                                                    billAmount = itemDrops.GetRandom();
                                                }

                                                Customer customer = context.Customers.Where(x => x.CustomerId == offer.CustomerId).FirstOrDefault();
                                                BillDiscount BillDiscount = new BillDiscount();
                                                BillDiscount.CustomerId = customer.CustomerId;
                                                BillDiscount.OrderId = 0;
                                                BillDiscount.OfferId = offerdb.OfferId;
                                                BillDiscount.BillDiscountType = offerdb.OfferOn;
                                                if (offerdb.OfferOn == "ScratchBillDiscount")
                                                {
                                                    BillDiscount.BillDiscountTypeValue = billAmount;//// scratch amount
                                                }
                                                BillDiscount.BillDiscountAmount = 0;
                                                BillDiscount.IsMultiTimeUse = offerdb.IsMultiTimeUse;
                                                BillDiscount.IsUseOtherOffer = offerdb.IsUseOtherOffer;
                                                BillDiscount.CreatedDate = indianTime;
                                                BillDiscount.ModifiedDate = indianTime;
                                                BillDiscount.IsActive = offerdb.IsActive;
                                                BillDiscount.IsDeleted = false;
                                                BillDiscount.CreatedBy = offerdb.userid;
                                                BillDiscount.ModifiedBy = offerdb.userid;
                                                BillDiscount.IsScratchBDCode = false;//scratched or not
                                                BillDiscount.Category = offerdb.Category;
                                                BillDiscount.Subcategory = offerdb.subCategory;
                                                BillDiscount.subSubcategory = offerdb.subSubCategory;

                                                context.BillDiscountDb.Add(BillDiscount);
                                            }
                                            else if (offer.ScratchCardCustomers != null && offer.ScratchCardCustomers.Any(x => x.WarehouseId == offerdb.WarehouseId) && offerdb.BillDiscountOfferOn == "DynamicAmount")
                                            {
                                                var customerids = offer.ScratchCardCustomers.Where(x => x.WarehouseId == offerdb.WarehouseId).Select(x => x.CustomerId).ToList();
                                                List<BillDiscount> customerdetails = context.BillDiscountDb.Where(x => customerids.Contains(x.CustomerId) && x.OfferId == offerdb.OfferId).ToList();
                                                List<BillDiscount> newbilldiscount = new List<BillDiscount>();
                                                foreach (var custdata in offer.ScratchCardCustomers)
                                                {
                                                    var customerdetail = customerdetails.FirstOrDefault(x => x.CustomerId == custdata.CustomerId);
                                                    if (customerdetail == null)
                                                    {
                                                        BillDiscount BillDiscount = new BillDiscount();
                                                        BillDiscount.CustomerId = custdata.CustomerId;
                                                        BillDiscount.OrderId = 0;
                                                        BillDiscount.OfferId = offerdb.OfferId;
                                                        BillDiscount.BillDiscountType = offerdb.OfferOn;
                                                        BillDiscount.BillDiscountTypeValue = custdata.Amount;
                                                        BillDiscount.BillDiscountAmount = custdata.Amount;
                                                        BillDiscount.IsMultiTimeUse = offerdb.IsMultiTimeUse;
                                                        BillDiscount.IsUseOtherOffer = offerdb.IsUseOtherOffer;
                                                        BillDiscount.CreatedDate = indianTime;
                                                        BillDiscount.ModifiedDate = indianTime;
                                                        BillDiscount.IsActive = offerdb.IsActive;
                                                        BillDiscount.IsDeleted = false;
                                                        BillDiscount.CreatedBy = offerdb.userid;
                                                        BillDiscount.ModifiedBy = offerdb.userid;
                                                        BillDiscount.IsScratchBDCode = false;//scratched or not
                                                        BillDiscount.MinOrderAmount = custdata.MinOrderAmount;
                                                        BillDiscount.MaxOrderAmount = custdata.MaxOrderAmount;
                                                        newbilldiscount.Add(BillDiscount);
                                                    }
                                                }

                                                if (newbilldiscount != null && newbilldiscount.Any())
                                                {
                                                    var BillDiscountsCustomers = new BulkOperations();
                                                    BillDiscountsCustomers.Setup<BillDiscount>(x => x.ForCollection(newbilldiscount))
                                                        .WithTable("BillDiscounts")
                                                        .WithBulkCopyBatchSize(4000)
                                                        .WithBulkCopyCommandTimeout(720) // Default is 600 seconds
                                                        .WithSqlCommandTimeout(720) // Default is 600 seconds
                                                        .AddAllColumns()
                                                        .BulkInsert();
                                                    BillDiscountsCustomers.CommitTransaction("AuthContext");
                                                }
                                            }

                                        }
                                        else if (offerdb.OfferId > 0 && offerdb.GroupId > 0)
                                        {
                                            // List<GroupMapping> groupmapp = context.GroupMappings.Where(x => x.GroupID == offerdb.GroupId && x.WarehouseID == offerdb.WarehouseId).ToList();
                                            string query = "select distinct a.CustomerID from SalesGroupCustomers a with(nolock) inner join Customers c with(nolock) on a.CustomerID=c.CustomerId and a.IsActive=1 and isnull(a.IsDeleted,0)=0 and a.GroupId=" + offerdb.GroupId + " and c.Warehouseid=" + offerdb.WarehouseId;
                                            List<int> groupmapp = context.Database.SqlQuery<int>(query).ToList();
                                            if (groupmapp != null && groupmapp.Count > 0)
                                            {
                                                double billAmount = 0;
                                                WeightedRandomBag<int> itemDrops = new WeightedRandomBag<int>();
                                                if (offerdb.BillDiscountOfferOn == "DynamicWalletPoint")
                                                {
                                                    foreach (var item in offerdb.OfferScratchWeights)
                                                    {
                                                        itemDrops.AddEntry(item.WalletPoint, item.Weight);
                                                    }
                                                }
                                                var customerids = groupmapp.ToList();
                                                List<BillDiscount> customerdetails = context.BillDiscountDb.Where(x => customerids.Contains(x.CustomerId) && x.OfferId == offerdb.OfferId).ToList();
                                                List<BillDiscount> newbilldiscount = new List<BillDiscount>();
                                                foreach (var custdata in groupmapp)
                                                {
                                                    billAmount = 0;
                                                    if (offerdb.BillDiscountOfferOn == "DynamicWalletPoint")
                                                    {
                                                        billAmount = itemDrops.GetRandom();
                                                    }
                                                    var customerdetail = customerdetails.FirstOrDefault(x => x.CustomerId == custdata);
                                                    if (customerdetail == null)
                                                    {
                                                        BillDiscount BillDiscount = new BillDiscount();
                                                        BillDiscount.CustomerId = custdata;
                                                        BillDiscount.OrderId = 0;
                                                        BillDiscount.OfferId = offerdb.OfferId;
                                                        BillDiscount.BillDiscountType = offerdb.OfferOn;
                                                        if (offerdb.OfferOn == "ScratchBillDiscount")
                                                        {
                                                            BillDiscount.BillDiscountTypeValue = billAmount;//// scratch amount
                                                        }
                                                        BillDiscount.BillDiscountAmount = 0;
                                                        BillDiscount.IsMultiTimeUse = offerdb.IsMultiTimeUse;
                                                        BillDiscount.IsUseOtherOffer = offerdb.IsUseOtherOffer;
                                                        BillDiscount.CreatedDate = indianTime;
                                                        BillDiscount.ModifiedDate = indianTime;
                                                        BillDiscount.IsActive = offerdb.IsActive;
                                                        BillDiscount.IsDeleted = false;
                                                        BillDiscount.CreatedBy = offerdb.userid;
                                                        BillDiscount.ModifiedBy = offerdb.userid;
                                                        BillDiscount.IsScratchBDCode = false;//scratched or not
                                                        newbilldiscount.Add(BillDiscount);
                                                    }
                                                }

                                                if (newbilldiscount != null && newbilldiscount.Any())
                                                {
                                                    var BillDiscountsCustomers = new BulkOperations();
                                                    BillDiscountsCustomers.Setup<BillDiscount>(x => x.ForCollection(newbilldiscount))
                                                        .WithTable("BillDiscounts")
                                                        .WithBulkCopyBatchSize(4000)
                                                        .WithBulkCopyCommandTimeout(720) // Default is 600 seconds
                                                        .WithSqlCommandTimeout(720) // Default is 600 seconds
                                                        .AddAllColumns()
                                                        .BulkInsert();
                                                    BillDiscountsCustomers.CommitTransaction("AuthContext");
                                                }

                                                // context.BillDiscountDb.AddRange(newbilldiscount);
                                            }
                                        }
                                        else if (offerdb.OfferId > 0 && (offer.CustomerId == -1 || offer.CustomerId == -2))
                                        {
                                            if (offerdb.StoreId > 0)
                                            {
                                                if(offer.ChannelIds != null && offer.ChannelIds != "")
                                                {
                                                    if (customeridlist.Any() && customeridlist.Count() > 0)
                                                    {
                                                        double billAmount = 0;
                                                        if (offerdb.BillDiscountOfferOn == "DynamicWalletPoint")
                                                        {
                                                            WeightedRandomBag<int> itemDrops = new WeightedRandomBag<int>();
                                                            foreach (var item in offerdb.OfferScratchWeights)
                                                            {
                                                                itemDrops.AddEntry(item.WalletPoint, item.Weight);
                                                            }
                                                            billAmount = itemDrops.GetRandom();
                                                        }

                                                        List<Customer> customers = context.Customers.Where(x => customeridlist.Contains(x.CustomerId)).ToList();
                                                        foreach (var customer in customers)
                                                        {
                                                            BillDiscount BillDiscount = new BillDiscount();
                                                            BillDiscount.CustomerId = customer.CustomerId;
                                                            BillDiscount.OrderId = 0;
                                                            BillDiscount.OfferId = offerdb.OfferId;
                                                            BillDiscount.BillDiscountType = offerdb.OfferOn;
                                                            if (offerdb.OfferOn == "ScratchBillDiscount")
                                                            {
                                                                BillDiscount.BillDiscountTypeValue = billAmount;//// scratch amount
                                                            }
                                                            BillDiscount.BillDiscountAmount = 0;
                                                            BillDiscount.IsMultiTimeUse = offerdb.IsMultiTimeUse;
                                                            BillDiscount.IsUseOtherOffer = offerdb.IsUseOtherOffer;
                                                            BillDiscount.CreatedDate = indianTime;
                                                            BillDiscount.ModifiedDate = indianTime;
                                                            BillDiscount.IsActive = offerdb.IsActive;
                                                            BillDiscount.IsDeleted = false;
                                                            BillDiscount.CreatedBy = offerdb.userid;
                                                            BillDiscount.ModifiedBy = offerdb.userid;
                                                            BillDiscount.IsScratchBDCode = false;//scratched or not
                                                            BillDiscount.Category = offerdb.Category;
                                                            BillDiscount.Subcategory = offerdb.subCategory;
                                                            BillDiscount.subSubcategory = offerdb.subSubCategory;

                                                            context.BillDiscountDb.Add(BillDiscount);
                                                        }

                                                    }
                                                }
                                               
                                            }
                                        }
                                        else if (offerdb.OfferId > 0 && offer.CustomerId == -9)
                                        {
                                            //string query = "Select a.CustomerId from Primecustomers a inner join Customers b on a.CustomerId=b.CustomerId and a.IsActive=1 and b.Active=1 and a.IsDeleted=0 and b.Warehouseid=" + offerdb.WarehouseId ;
                                            //List<int> customerids = context.Database.SqlQuery<int>(query).ToList();
                                            //double billAmount = 0;
                                            //WeightedRandomBag<int> itemDrops = new WeightedRandomBag<int>();
                                            //if (offerdb.BillDiscountOfferOn == "DynamicWalletPoint")
                                            //{
                                            //    foreach (var item in offerdb.OfferScratchWeights)
                                            //    {
                                            //        itemDrops.AddEntry(item.WalletPoint, item.Weight);
                                            //    }
                                            //}
                                            //foreach (var item in customerids)
                                            //{
                                            //    billAmount = 0;
                                            //    if (offerdb.BillDiscountOfferOn == "DynamicWalletPoint")
                                            //    {
                                            //        billAmount = itemDrops.GetRandom();
                                            //    }
                                            //    BillDiscount BillDiscount = new BillDiscount();
                                            //    BillDiscount.CustomerId = item;
                                            //    BillDiscount.OrderId = 0;
                                            //    BillDiscount.OfferId = offerdb.OfferId;
                                            //    BillDiscount.BillDiscountType = offerdb.OfferOn;
                                            //    BillDiscount.BillDiscountTypeValue = billAmount;
                                            //    BillDiscount.BillDiscountAmount = 0;
                                            //    BillDiscount.IsMultiTimeUse = offerdb.IsMultiTimeUse;
                                            //    BillDiscount.IsUseOtherOffer = offerdb.IsUseOtherOffer;
                                            //    BillDiscount.CreatedDate = indianTime;
                                            //    BillDiscount.ModifiedDate = indianTime;
                                            //    BillDiscount.IsActive = offerdb.IsActive;
                                            //    BillDiscount.IsDeleted = false;
                                            //    BillDiscount.CreatedBy = offerdb.userid;
                                            //    BillDiscount.ModifiedBy = offerdb.userid;
                                            //    BillDiscount.IsScratchBDCode = false;//scratched or not
                                            //    BillDiscount.Category = offerdb.Category;
                                            //    BillDiscount.Subcategory = offerdb.subCategory;
                                            //    BillDiscount.subSubcategory = offerdb.subSubCategory;
                                            //    context.BillDiscountDb.Add(BillDiscount);
                                            //}
                                        }
                                        else if (offer.CustomerId < -2)
                                        {
                                            int Level = -1;
                                            switch (offerdb.CustomerId)
                                            {
                                                case -3:
                                                    Level = 0;
                                                    break;
                                                case -4:
                                                    Level = 1;
                                                    break;
                                                case -5:
                                                    Level = 2;
                                                    break;
                                                case -6:
                                                    Level = 3;
                                                    break;
                                                case -7:
                                                    Level = 4;
                                                    break;
                                                case -8:
                                                    Level = 5;
                                                    break;
                                            }

                                            var fromdate = DateTime.Now;

                                            fromdate = DateTime.Now.AddMonths(-1);
                                            string query = "Select distinct a.CustomerId from CRMCustomerLevels a with(nolock)  inner join Customers b  with(nolock)  on a.CustomerId=b.CustomerId and IsDeleted=0 and b.Warehouseid=" + offerdb.WarehouseId + " and a.Month=" + fromdate.Month + " and a.Year=" + fromdate.Year + " And a.Level=" + Level;
                                            List<int> customerids = context.Database.SqlQuery<int>(query).ToList();


                                            double billAmount = 0;
                                            WeightedRandomBag<int> itemDrops = new WeightedRandomBag<int>();
                                            if (offerdb.BillDiscountOfferOn == "DynamicWalletPoint")
                                            {
                                                foreach (var item in offerdb.OfferScratchWeights)
                                                {
                                                    itemDrops.AddEntry(item.WalletPoint, item.Weight);
                                                }
                                            }
                                            List<BillDiscount> BillDiscounts = new List<BillDiscount>();
                                            foreach (var item in customerids)
                                            {
                                                billAmount = 0;
                                                if (offerdb.BillDiscountOfferOn == "DynamicWalletPoint")
                                                {
                                                    billAmount = itemDrops.GetRandom();
                                                }
                                                BillDiscount BillDiscount = new BillDiscount();
                                                BillDiscount.CustomerId = item;
                                                BillDiscount.OrderId = 0;
                                                BillDiscount.OfferId = offerdb.OfferId;
                                                BillDiscount.BillDiscountType = offerdb.OfferOn;
                                                BillDiscount.BillDiscountTypeValue = billAmount;
                                                BillDiscount.BillDiscountAmount = 0;
                                                BillDiscount.IsMultiTimeUse = offerdb.IsMultiTimeUse;
                                                BillDiscount.IsUseOtherOffer = offerdb.IsUseOtherOffer;
                                                BillDiscount.CreatedDate = indianTime;
                                                BillDiscount.ModifiedDate = indianTime;
                                                BillDiscount.IsActive = offerdb.IsActive;
                                                BillDiscount.IsDeleted = false;
                                                BillDiscount.CreatedBy = offerdb.userid;
                                                BillDiscount.ModifiedBy = offerdb.userid;
                                                BillDiscount.IsScratchBDCode = false;//scratched or not
                                                BillDiscount.Category = offerdb.Category;
                                                BillDiscount.Subcategory = offerdb.subCategory;
                                                BillDiscount.subSubcategory = offerdb.subSubCategory;
                                                BillDiscounts.Add(BillDiscount);
                                            }

                                            if (BillDiscounts != null && BillDiscounts.Any())
                                            {
                                                var BillDiscountsCustomers = new BulkOperations();
                                                BillDiscountsCustomers.Setup<BillDiscount>(x => x.ForCollection(BillDiscounts))
                                                    .WithTable("BillDiscounts")
                                                    .WithBulkCopyBatchSize(4000)
                                                    .WithBulkCopyCommandTimeout(720) // Default is 600 seconds
                                                    .WithSqlCommandTimeout(720) // Default is 600 seconds
                                                    .AddAllColumns()
                                                    .BulkInsert();
                                                BillDiscountsCustomers.CommitTransaction("AuthContext");
                                            }

                                        }

                                    }

                                    List<ItemMaster> itemnumber = new List<ItemMaster>();
                                    if (offerdb.OfferOn == "Item" && offerdb.IsActive && offerdb.start <= date1 && offerdb.end >= date2)
                                    {

                                        if (offerdb.OfferAppType != "Distributor App")
                                        {
                                            ItemMaster warehousefreeitem = new ItemMaster();
                                            if (offer.FreeOfferType == "ItemMaster")
                                            {
                                                warehousefreeitem = context.itemMasters.Where(x => x.SellingSku == freeitem.SellingSku && x.ItemMultiMRPId == freeitem.ItemMultiMRPId && x.WarehouseId == offerdb.WarehouseId).FirstOrDefault();
                                                offerdb.FreeItemId = warehousefreeitem.ItemId;
                                                itemMaster = context.itemMasters.Where(x => x.SellingSku == itemMaster.SellingSku && x.ItemMultiMRPId == itemMaster.ItemMultiMRPId && x.WarehouseId == offerdb.WarehouseId).FirstOrDefault();
                                                offerdb.itemId = itemMaster.ItemId;
                                                offerdb.FreeItemMRP = warehousefreeitem.MRP;
                                                offerdb.FreeItemName = warehousefreeitem.itemname;
                                            }
                                            if (offer.IsFreebiesLevel)//by sudhir 27-06-2023
                                            {
                                                offerdb.IsFreebiesLevel = offer.IsFreebiesLevel;
                                                itemnumber = context.itemMasters.Where(x => x.SellingSku == itemMaster.SellingSku && x.ItemMultiMRPId == itemMaster.ItemMultiMRPId && x.WarehouseId == offerdb.WarehouseId && x.Deleted == false).ToList();
                                            }
                                            else
                                            {
                                                itemnumber = context.itemMasters.Where(x => x.Number == itemMaster.Number && x.ItemMultiMRPId == itemMaster.ItemMultiMRPId && x.WarehouseId == offerdb.WarehouseId && x.Deleted == false).ToList();
                                            }
                                            if (itemnumber.Count != 0)
                                            {
                                                foreach (var editItemMaster in itemnumber)
                                                {
                                                    editItemMaster.IsOffer = true;
                                                    if (offerdb.OfferCategory == "Offer")
                                                    {
                                                        editItemMaster.OfferCategory = 1;
                                                    }
                                                    else if (offerdb.OfferCategory == "FlashDeal")
                                                    {
                                                        editItemMaster.OfferCategory = 2;
                                                    }
                                                    editItemMaster.OfferStartTime = date1;
                                                    editItemMaster.OfferEndTime = date2;
                                                    editItemMaster.OfferQtyAvaiable = offerdb.QtyAvaiable;
                                                    editItemMaster.OfferQtyConsumed = offerdb.QtyConsumed;
                                                    editItemMaster.OfferId = offerdb.OfferId;
                                                    editItemMaster.OfferWalletPoint = offerdb.FreeWalletPoint;
                                                    editItemMaster.OfferType = offerdb.FreeOfferType;
                                                    editItemMaster.OfferFreeItemId = warehousefreeitem.ItemId;
                                                    editItemMaster.OfferPercentage = offerdb.DiscountPercentage;
                                                    editItemMaster.OfferMinimumQty = offerdb.MinOrderQuantity;
                                                    editItemMaster.OfferFreeItemName = warehousefreeitem.itemname;
                                                    editItemMaster.OfferFreeItemQuantity = offerdb.NoOffreeQuantity;
                                                    if (offerdb.FreeItemId > 0 && offerdb.FreeOfferType == "ItemMaster")
                                                    {
                                                        ItemMasterCentral imageitem = context.ItemMasterCentralDB.Where(x => x.SellingSku == freeitem.SellingSku).FirstOrDefault();
                                                        editItemMaster.OfferFreeItemImage = imageitem.LogoUrl;
                                                    }
                                                    context.Entry(editItemMaster).State = EntityState.Modified;
                                                }
                                            }
                                        }
                                        else
                                        {
                                            ItemMaster warehousefreeitem = new ItemMaster();
                                            if (offer.FreeOfferType == "ItemMaster")
                                            {
                                                warehousefreeitem = context.itemMasters.Where(x => x.SellingSku == freeitem.SellingSku && x.ItemMultiMRPId == freeitem.ItemMultiMRPId && x.WarehouseId == offerdb.WarehouseId).FirstOrDefault();
                                                var imageitem = context.ItemMasterCentralDB.Where(x => x.SellingSku == freeitem.SellingSku).Select(x => x.LogoUrl).FirstOrDefault();
                                                warehousefreeitem.LogoUrl = imageitem;
                                                offerdb.FreeItemId = warehousefreeitem.ItemId;
                                                itemMaster = context.itemMasters.Where(x => x.SellingSku == itemMaster.SellingSku && x.ItemMultiMRPId == itemMaster.ItemMultiMRPId && x.WarehouseId == offerdb.WarehouseId).FirstOrDefault();
                                                offerdb.itemId = itemMaster.ItemId;
                                            }
                                            OfferFreeItem offerFreeItem = new OfferFreeItem
                                            {
                                                FreeItemId = warehousefreeitem.ItemId,
                                                ItemNumber = offer.IsFreebiesLevel == true ? itemMaster.SellingSku : itemMaster.Number,
                                                OfferFreeItemImage = warehousefreeitem.LogoUrl,
                                                OfferFreeItemName = warehousefreeitem.itemname,
                                                OfferFreeItemQuantity = offerdb.NoOffreeQuantity,
                                                OfferMinimumQty = offerdb.MinOrderQuantity,
                                                OfferQtyAvaiable = Convert.ToDouble(offer.FreeItemLimit),
                                                OfferQtyConsumed = offerdb.QtyConsumed,
                                                OfferType = offerdb.FreeOfferType,
                                                OfferWalletPoint = offerdb.FreeWalletPoint,
                                                ItemMultiMRPId = itemMaster.ItemMultiMRPId,
                                                OfferOn = offer.IsFreebiesLevel == true ? "SellingSku" : "Item",
                                            };

                                            if (offerdb.OfferFreeItems == null)
                                                offerdb.OfferFreeItems = new List<OfferFreeItem>();

                                            offerdb.OfferFreeItems.Add(offerFreeItem);



                                        }

                                    }

                                    context.Entry(offerdb).State = EntityState.Modified;
                                }

                                if (offerbilldiscountitems.Any())
                                    context.OfferItemsBillDiscountDB.AddRange(offerbilldiscountitems);
                                if (billDiscountOfferSections.Any())
                                    context.BillDiscountOfferSectionDB.AddRange(billDiscountOfferSections);

                                if (context.Commit() > 0)
                                {
                                    offerResponseDC.Offer = null;
                                    offerResponseDC.status = true;
                                    offerResponseDC.msg = "Offer Added Successfully.";
                                }
                                else
                                {
                                    foreach (var offerdb in offers)
                                    {
                                        offerdb.IsActive = false;
                                        offerdb.IsDeleted = true;
                                        context.Entry(offerdb).State = EntityState.Modified;
                                    }
                                    offerResponseDC.Offer = null;
                                    offerResponseDC.status = false;
                                    offerResponseDC.msg = "Offer not added.";
                                }

                            }
                            else
                            {
                                offerResponseDC.Offer = null;
                                offerResponseDC.status = false;
                                offerResponseDC.msg = "Offer not added.";
                            }
                        }

                    }
                    else
                    {
                        offerResponseDC.Offer = offer;
                        offerResponseDC.status = false;
                        offerResponseDC.msg = "Please select atleast one warehouse to add offer";
                    }


                }
                catch (Exception ex)
                {
                    logger.Error("Error during add Offers:" + ex.ToString());
                    offerResponseDC.msg = "Some error occurred during save data.";
                    offerResponseDC.status = false;
                }

                //var result = context.AddOffer(offer, UserName);
#if !DEBUG
                if (offerResponseDC != null && offerResponseDC.status)
                {


                    foreach (var Offer in offers)
                    {
                        if (Offer.IsActive && Offer.end > DateTime.Now)
                        {

                            var jobId = BackgroundJob.Schedule(
                                      () => ActiveDeativeofferByJob(Offer.OfferId),
                            TimeSpan.FromMinutes((Offer.end - DateTime.Now).TotalMinutes));
                            MongoDbHelper<ScheduleJobHistory> mongoDbHelper = new MongoDbHelper<ScheduleJobHistory>();
                            ScheduleJobHistory scheduleJobHistory = new ScheduleJobHistory
                            {
                                CreatedDate = DateTime.Now,
                                isActive = true,
                                jobid = jobId,
                                ObjectId = Offer.OfferId.ToString(),
                                ObjectType = "Offer"
                            };
                            mongoDbHelper.Insert(scheduleJobHistory);
                        }

                    }
                }
#endif

            }
            return offerResponseDC;
        }

        //[ResponseType(typeof(Coupon))]
        //For update Offer.
        //Created By Sachin Jaiswal
        [Route("")]
        [AcceptVerbs("PUT")]
        public Offer Put(Offer Offer)
        {
            using (var context = new AuthContext())
            {
                try
                {
                    var identity = User.Identity as ClaimsIdentity;
                    int compid = 0, userid = 0;
                    string UserName = string.Empty;
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
                            UserName = (claim.Value);
                        }
                    }

                    Offer.CompanyId = compid;
                    var result = context.PutOffer(Offer, UserName);
                    OfferJobScheduleReset(Offer);
                    return result;
                }
                catch (Exception ex)
                {
                    logger.Error("Error in Put Coupon " + ex.Message);
                    return null;
                }
            }
        }
        //For active and deactive Offer
        [Route("ActiveDeativeoffer")]
        [AcceptVerbs("PUT")]
        public InactiveOfferResponse ActiveDeativeoffer(Offer Offer)
        {
            InactiveOfferResponse inactiveOfferResponse = new InactiveOfferResponse();
            using (var context = new AuthContext())
            {
                try
                {
                    var identity = User.Identity as ClaimsIdentity;
                    int compid = 0, userid = 0; string UserName = string.Empty;

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
                            UserName = (claim.Value);
                        }
                    }
                    List<int> itemIds = new List<int>();
                    Offer.CompanyId = compid;
                    List<ItemMaster> itemnumber = new List<ItemMaster>();
                    ItemMaster itemMaster = new ItemMaster();
                    if (Offer.OfferOn == "Item" && Offer.IsActive && !Offer.InactivePreviousOffer)
                    {
                        var itemmaster = context.itemMasters.FirstOrDefault(x => x.ItemId == Offer.itemId);
                        if (itemmaster != null)
                        {
                            if (Offer.IsFreebiesLevel)//by sudhir 27-06-2023
                            {
                                itemIds = context.itemMasters.Where(x => x.SellingSku == itemmaster.SellingSku && x.WarehouseId == itemmaster.WarehouseId).Select(x => x.ItemId).ToList();
                                //itemnumber = context.itemMasters.Where(x => x.SellingSku == itemMaster.SellingSku && x.ItemMultiMRPId == itemMaster.ItemMultiMRPId && x.WarehouseId == offerdb.WarehouseId && x.Deleted == false).ToList();
                            }
                            else
                            {
                                itemIds = context.itemMasters.Where(x => x.ItemMultiMRPId == itemmaster.ItemMultiMRPId && x.WarehouseId == itemmaster.WarehouseId).Select(x => x.ItemId).ToList();
                                //itemnumber = context.itemMasters.Where(x => x.Number == itemMaster.Number && x.ItemMultiMRPId == itemMaster.ItemMultiMRPId && x.WarehouseId == offerdb.WarehouseId && x.Deleted == false).ToList();
                            }
                            var offercodes = context.OfferDb.Where(x => itemIds.Contains(x.itemId) && x.OfferOn == "Item" && x.OfferId != Offer.OfferId && x.OfferAppType == Offer.OfferAppType && x.WarehouseId == itemmaster.WarehouseId && x.IsActive).Select(s => s.OfferCode).ToList();
                            if (offercodes.Any())
                            {
                                inactiveOfferResponse.offer = Offer;
                                inactiveOfferResponse.status = false;
                                inactiveOfferResponse.msg = "Below Offer are active " + string.Join(",", offercodes) + ". Are you sure you want to inactive then active.";
                                return inactiveOfferResponse;
                            }

                        }
                    }
                    else if (Offer.InactivePreviousOffer)
                    {
                        var itemmaster = context.itemMasters.FirstOrDefault(x => x.ItemId == Offer.itemId);
                        // itemMaster = context.itemMasters.Where(x => x.ItemId == Offer.itemId).SingleOrDefault();
                        if (itemmaster != null)
                        {
                            if (Offer.IsFreebiesLevel)//by sudhir 27-06-2023
                            {
                                itemIds = context.itemMasters.Where(x => x.SellingSku == itemmaster.SellingSku && x.WarehouseId == itemmaster.WarehouseId).Select(x => x.ItemId).ToList();
                                //itemnumber = context.itemMasters.Where(x => x.SellingSku == itemMaster.SellingSku && x.ItemMultiMRPId == itemMaster.ItemMultiMRPId && x.WarehouseId == offerdb.WarehouseId && x.Deleted == false).ToList();
                            }
                            else
                            {
                                itemIds = context.itemMasters.Where(x => x.ItemMultiMRPId == itemmaster.ItemMultiMRPId && x.WarehouseId == itemmaster.WarehouseId).Select(x => x.ItemId).ToList();
                                //itemnumber = context.itemMasters.Where(x => x.Number == itemMaster.Number && x.ItemMultiMRPId == itemMaster.ItemMultiMRPId && x.WarehouseId == offerdb.WarehouseId && x.Deleted == false).ToList();
                            }
                            //itemIds = context.itemMasters.Where(x => x.SellingSku == itemmaster.SellingSku && x.ItemMultiMRPId == itemmaster.ItemMultiMRPId && x.WarehouseId == itemmaster.WarehouseId).Select(x => x.ItemId).ToList();
                            var activeoffers = context.OfferDb.Where(x => itemIds.Contains(x.itemId) && x.OfferId != Offer.OfferId && x.WarehouseId == Offer.WarehouseId && x.OfferOn == "Item" && x.IsActive).ToList();
                            if (activeoffers.Any())
                            {
                                foreach (var item in activeoffers)
                                {
                                    item.IsActive = false;
                                    var updatedOffernew = context.ActiveDeativeoffer(item, UserName);
                                    OfferJobScheduleReset(updatedOffernew);
                                }
                            }

                        }
                    }

                    var updatedOffer = context.ActiveDeativeoffer(Offer, UserName);
                    inactiveOfferResponse.offer = updatedOffer;
                    inactiveOfferResponse.status = true;
                    OfferJobScheduleReset(Offer);

                    return inactiveOfferResponse;
                }
                catch (Exception ex)
                {
                    logger.Error("Error in Put Coupon " + ex.Message);
                    return null;
                }
            }
        }

        [Route("ActiveDeativeofferByJob")]
        [AcceptVerbs("PUT")]
        public Offer ActiveDeativeofferByJob(int OfferId)
        {
            using (var context = new AuthContext())
            {
                var updatedOffer = context.ActiveDeativeofferByJob(OfferId, "System");

                OfferJobScheduleReset(updatedOffer);

                return updatedOffer;

            }
        }

        private bool OfferJobScheduleReset(Offer Offer)
        {
            MongoDbHelper<ScheduleJobHistory> mongoDbHelper = new MongoDbHelper<ScheduleJobHistory>();
            var predicate = PredicateBuilder.New<ScheduleJobHistory>(x => x.isActive && x.ObjectType == "Offer" && x.ObjectId == Offer.OfferId.ToString());
            var scheduleJobHistory = mongoDbHelper.Select(predicate).FirstOrDefault();

            if (Offer.IsActive)
            {
                if (scheduleJobHistory != null)
                {
                    BackgroundJob.Delete(scheduleJobHistory.jobid);
                    scheduleJobHistory.isActive = false;
                    scheduleJobHistory.ModifiedDate = DateTime.Now;
                    mongoDbHelper.ReplaceWithoutFind(scheduleJobHistory.Id, scheduleJobHistory);
                }

                Offer.IsActive = false;
                var jobId = BackgroundJob.Schedule(
                              () => ActiveDeativeoffer(Offer),
                    TimeSpan.FromMinutes((Offer.end - DateTime.Now).TotalMinutes));
                scheduleJobHistory = new ScheduleJobHistory
                {
                    CreatedDate = DateTime.Now,
                    isActive = true,
                    jobid = jobId,
                    ObjectId = Offer.OfferId.ToString(),
                    ObjectType = "Offer"
                };
                mongoDbHelper.Insert(scheduleJobHistory);

            }
            else if (scheduleJobHistory != null)
            {
                BackgroundJob.Delete(scheduleJobHistory.jobid);
                scheduleJobHistory.isActive = false;
                scheduleJobHistory.ModifiedDate = DateTime.Now;
                mongoDbHelper.ReplaceWithoutFind(scheduleJobHistory.Id, scheduleJobHistory);
            }

            return true;
        }

        //[ResponseType(typeof(Coupon))]
        //For Removing Offer
        [Route("")]
        [AcceptVerbs("Delete")]
        public void Remove(int id)
        {
            logger.Info("DELETE Remove: ");
            using (var context = new AuthContext())
            {
                try
                {
                    var identity = User.Identity as ClaimsIdentity;
                    int compid = 0, userid = 0; string UserName = string.Empty;

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
                            UserName = (claim.Value);
                        }
                    }
                    context.DeleteOffer(id, compid, UserName);
                }
                catch (Exception ex)
                {
                    logger.Error("Error in Remove offer " + ex.Message);

                }
            }
        }
        [Route("GetOfferItem")]
        [HttpGet]
        public IEnumerable<OfferItem> GetOfferItem(int oderid)
        {
            List<OfferItem> OfferItem = new List<OfferItem>();
            using (var context = new AuthContext())
            {
                try
                {
                    OfferItem = context.GetOfferItemByOrderId(oderid).ToList();
                    return OfferItem;
                }
                catch (Exception ex)
                {
                    logger.Error("Error in Get coupon by id " + ex.Message);
                    logger.Info("End  single coupon: ");
                    return null;
                }
            }
        }

        [Route("GetOfferBill")]
        [HttpGet]
        public InvoiceOrderOffer GetOfferBillDiscount(int oderid)
        {
            var invoiceOrderOffer = new InvoiceOrderOffer();
            List<InvoiceOrderOffer> invoiceOrderOffers = new List<InvoiceOrderOffer>();
            using (var context = new AuthContext())
            {

                var query = " select a.OrderId,b.OfferCode,b.ApplyOn,a.BillDiscountTypeValue,a.BillDiscountAmount from  BillDiscounts a  inner join Offers b on a.OfferId = b.OfferId  where a.orderid =  " + oderid + " and b.ApplyOn = 'PostOffer' Union all select orderid,'Flash Deal','',0,0 from FlashDealItemConsumeds a where a.orderid = " + oderid + " group by orderid";
                invoiceOrderOffers = context.Database.SqlQuery<InvoiceOrderOffer>(query).ToList();
                if (invoiceOrderOffers != null && invoiceOrderOffers.Any())
                {
                    var offerCodes = invoiceOrderOffers.Select(x => x.OfferCode).ToList();
                    invoiceOrderOffer.OfferCode = string.Join(",", offerCodes);
                    double totalBillDicount = 0;
                    foreach (var item in invoiceOrderOffers)
                    {
                        if (item.BillDiscountAmount > 0)
                            totalBillDicount += item.BillDiscountAmount;
                        else
                            totalBillDicount += item.BillDiscountTypeValue;
                    }
                    invoiceOrderOffer.BillDiscountAmount = totalBillDicount > 0 ? totalBillDicount / 10 : 0;
                }

                return invoiceOrderOffer;

            }
        }
        [Route("GetOfferByItem")]
        [HttpGet]
        public dynamic GetOfferForMobile(int itemid, int Companyid)
        {
            logger.Info("start single User: ");
            var Offer = new Offer();
            using (var context = new AuthContext())
            {
                try
                {

                    Offer = context.GetOfferByItem(itemid, Companyid);
                    if (Offer != null)
                    {
                        return Offer;
                    }
                    else
                    {
                        var obj = new
                        {
                        };

                        return obj;
                    }
                    //logger.Info("End Get coupon by id: " + offer.OfferId);

                }
                catch (Exception ex)
                {
                    logger.Error("Error in Get coupon by id " + ex.Message);
                    logger.Info("End  single coupon: ");
                    return null;
                }
            }
        }

        [Route("GetAllOfferItem")]
        [HttpGet]
        public List<ItemMaster> GetAllOfferItem()
        {
            logger.Info("GetAllOfferItem: ");
            List<ItemMaster> item = new List<ItemMaster>();
            using (var ac = new AuthContext())
            {
                {
                    item = (from i in ac.itemMasters
                            join o in ac.OfferDb on i.ItemId equals o.itemId
                            where o.start <= DateTime.Today && o.end >= DateTime.Today && o.IsActive && o.IsDeleted == false && i.active == true && i.Deleted == false
                            && i.IsOffer == true
                            select i).ToList();
                    List<ItemMaster> finallist = new List<ItemMaster>();
                    string itemnumber = string.Empty;
                    List<string> itemnolist = new List<string>();
                    foreach (var dd in item)
                    {
                        itemnumber = dd.Number;
                        var item1 = ac.itemMasters.Where(i => i.Number == itemnumber);
                        finallist.AddRange(item1);
                    }
                    return finallist;
                }
            }
        }

        [Route("GetAllOfferHistory")]
        [HttpGet]
        public List<OfferHistory> GetAllOfferHistory(int OfferId)
        {
            using (var context = new AuthContext())
            {
                try
                {
                    List<OfferHistory> offerHistories = context.OfferHistoryDB.Where(x => x.OfferId == OfferId).ToList();
                    return offerHistories;
                }
                catch (Exception ee)
                {
                    return null;
                }

            }
        }

        // removed by Harry : 21 May 2019 
        [Route("GetAllOffers")]
        [HttpGet]
        public List<offeritem> GetAllOffers(int warehouseid)
        {
            logger.Info("GetAllOffers: ");
            List<ItemMaster> item = new List<ItemMaster>();
            using (var ac = new AuthContext())
            {
                item = (from i in ac.itemMasters
                        join o in ac.OfferDb on i.ItemId equals o.itemId
                        where o.WarehouseId == warehouseid && o.start <= DateTime.Today && o.end >= DateTime.Today && o.IsActive && o.IsDeleted == false && i.active == true && i.Deleted == false
                        && i.IsOffer == true
                        select i).ToList();
                List<offeritem> finallist = new List<offeritem>();
                string itemnumber = string.Empty;
                List<string> itemnolist = new List<string>();
                foreach (var dd in item)
                {
                    itemnumber = dd.Number;
                    var item1 = ac.itemMasters.Where(i => i.Number == itemnumber);

                    var data = (from i in ac.itemMasters
                                join o in ac.OfferDb on i.ItemId equals o.itemId
                                where i.Number == itemnumber & o.IsActive == true
                                select new offeritem
                                {
                                    ItemId = i.ItemId,
                                    Cityid = i.Cityid,
                                    CityName = i.CityName,
                                    Categoryid = i.Categoryid,
                                    SubCategoryId = i.SubCategoryId,
                                    SubsubCategoryid = i.SubsubCategoryid,
                                    SubSubCode = i.SubSubCode,
                                    WarehouseId = i.WarehouseId,
                                    SupplierId = i.SupplierId,
                                    SUPPLIERCODES = i.SUPPLIERCODES,
                                    CompanyId = i.CompanyId,
                                    CategoryName = i.CategoryName,
                                    BaseCategoryName = i.BaseCategoryName,
                                    SubcategoryName = i.SubcategoryName,
                                    SubsubcategoryName = i.SubsubcategoryName,
                                    SupplierName = i.SupplierName,
                                    itemname = i.itemname,
                                    SellingUnitName = i.SellingUnitName,
                                    price = i.price,
                                    LogoUrl = i.LogoUrl,
                                    CatLogoUrl = i.CatLogoUrl,
                                    VATTax = i.VATTax,
                                    UnitPrice = i.UnitPrice,
                                    Number = i.Number,
                                    PurchaseSku = i.PurchaseSku,
                                    SellingSku = i.SellingSku,
                                    PurchasePrice = i.PurchasePrice,
                                    HindiName = i.HindiName,
                                    marginPoint = i.marginPoint,
                                    NetPurchasePrice = i.NetPurchasePrice,
                                    promoPoint = i.promoPoint,
                                    Discount = i.Discount,
                                    MinOrderQty = i.MinOrderQty,
                                    TotalTaxPercentage = i.TotalTaxPercentage,
                                    IsOffer = i.IsOffer,
                                    OfferName = o.OfferName,
                                    OfferLogoUrl = o.OfferLogoUrl,
                                    FreeItemName = o.FreeItemName,
                                    FreeItemMRP = o.FreeItemMRP,
                                    FreeWalletPoint = o.FreeWalletPoint
                                });

                    finallist.AddRange(data);
                }
                //return finallist;
                return null;
            }
        }


        [Route("GetOfferForMobile")]
        [HttpGet]
        public HttpResponseMessage GetOfferForMobile(DateTime CurrentDate, int WarehouseId)
        {
            using (var context = new AuthContext())
            {
                try
                {
                    List<Offer> offerData = context.OfferDb.Where(x => x.WarehouseId == WarehouseId && x.start <= CurrentDate && x.end >= CurrentDate && x.IsActive == true && x.IsDeleted == false).ToList();
                    if (offerData.Count() != 0)
                    {
                        var res = new
                        {
                            offerData = offerData,
                            Status = true,
                            Message = "Success."
                        };
                        return Request.CreateResponse(HttpStatusCode.OK, res);
                    }
                    else
                    {
                        var res = new
                        {
                            offerData = "No offer",
                            Status = false,
                            Message = "Fail"
                        };
                        return Request.CreateResponse(HttpStatusCode.OK, res);
                    }
                }
                catch (Exception ee)
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, ee.Message);
                }
            }
        }

        [Route("GetOfferCustomer")]
        [HttpGet]
        public List<CustomerOffer> GetOfferCustomer()
        {
            using (var context = new AuthContext())
            {
                try
                {
                    List<CustomerOffer> freeItems = (from freeitem in context.OfferItemDb
                                                     join warehouse in context.Warehouses on
                                                     freeitem.WarehouseId equals
                                                     warehouse.WarehouseId
                                                     join c in context.Customers on freeitem.CustomerId equals c.CustomerId
                                                     join offer in context.OfferDb on freeitem.ReferOfferId equals offer.OfferId
                                                     select new CustomerOffer
                                                     {
                                                         WarehouseName = warehouse.WarehouseName,
                                                         ItemName = freeitem.itemname,
                                                         FreeItemName = freeitem.FreeItemName,
                                                         WalletPoint = freeitem.WallentPoint,
                                                         Customer = c.ShopName,
                                                         OfferType = freeitem.OfferType,
                                                         CreatedDate = freeitem.CreatedDate,
                                                         OfferName = offer.OfferName
                                                     }).ToList();
                    return freeItems;

                }
                catch (Exception ee)
                {
                    return null;
                }
            }
        }

        [Route("GetOfferCustomerByOffer")]
        [HttpGet]
        public List<CustomerOffer> GetOfferCustomerByOffer(int OfferId)
        {
            using (var context = new AuthContext())
            {
                try
                {
                    List<CustomerOffer> freeItems = (from freeitem in context.OfferItemDb
                                                     join warehouse in context.Warehouses on
                                                     freeitem.WarehouseId equals
                                                     warehouse.WarehouseId
                                                     join c in context.Customers on freeitem.CustomerId equals c.CustomerId
                                                     join offer in context.OfferDb on freeitem.ReferOfferId equals offer.OfferId
                                                     where freeitem.ReferOfferId == OfferId
                                                     select new CustomerOffer
                                                     {
                                                         WarehouseName = warehouse.WarehouseName,
                                                         ItemName = freeitem.itemname,
                                                         FreeItemName = freeitem.FreeItemName,
                                                         WalletPoint = freeitem.WallentPoint,
                                                         Customer = c.ShopName,
                                                         OfferType = freeitem.OfferType,
                                                         CreatedDate = freeitem.CreatedDate,
                                                         OfferName = offer.OfferName
                                                     }).ToList();
                    return freeItems;

                }
                catch (Exception ee)
                {
                    return null;
                }
            }
        }

        //For Mobile API Showing item in that there are current offer.
        [Route("GetOfferItemForMobile")]
        [HttpGet]
        public HttpResponseMessage GetOfferItemForMobile(int WarehouseId)
        {
            using (var context = new AuthContext())
            {
                try
                {
                    WROFFERTEM item = new WROFFERTEM();
                    DateTime CurrentDate = DateTime.Now;
                    List<factoryItemdata> itemMasters = new List<factoryItemdata>();
                    itemMasters = (from a in context.itemMasters
                                   where (a.WarehouseId == WarehouseId && a.OfferStartTime <= CurrentDate
                                   && a.OfferEndTime >= CurrentDate && a.OfferCategory == 1 && a.active == true && a.Deleted == false)
                                   //join b in context.ItemMasterCentralDB on a.SellingSku equals b.SellingSku
                                   join c in context.OfferDb on a.OfferId equals c.OfferId
                                   where (c.IsActive == true && c.IsDeleted == false && c.OfferAppType == "Retailer App" || c.OfferAppType == "Both")
                                   let limit = context.ItemLimitMasterDB.Where(p2 => a.ItemMultiMRPId == p2.ItemMultiMRPId && a.WarehouseId == p2.WarehouseId).FirstOrDefault()
                                   select new factoryItemdata
                                   {
                                       WarehouseId = a.WarehouseId,
                                       CompanyId = a.CompanyId,
                                       IsItemLimit = limit != null ? limit.IsItemLimit : false,
                                       ItemlimitQty = limit != null && limit.IsItemLimit ? limit.ItemlimitQty : 0,
                                       Categoryid = a.Categoryid,
                                       Discount = a.Discount,
                                       ItemId = a.ItemId,
                                       ItemNumber = a.Number,
                                       //itemname = a.HindiName != null ? a.HindiName : a.itemname,
                                       itemname = a.itemname,
                                       LogoUrl = a.LogoUrl,
                                       MinOrderQty = a.MinOrderQty,
                                       price = a.price,
                                       SubCategoryId = a.SubCategoryId,
                                       SubsubCategoryid = a.SubsubCategoryid,
                                       TotalTaxPercentage = a.TotalTaxPercentage,
                                       SellingUnitName = a.SellingUnitName,
                                       SellingSku = a.SellingSku,
                                       UnitPrice = a.UnitPrice,
                                       HindiName = a.HindiName != null ? a.HindiName : a.itemname,
                                       VATTax = a.VATTax,
                                       active = a.active,
                                       marginPoint = a.marginPoint,
                                       NetPurchasePrice = a.NetPurchasePrice,
                                       promoPerItems = a.promoPerItems,
                                       IsOffer = a.IsOffer,
                                       Deleted = a.Deleted,
                                       OfferCategory = a.OfferCategory,
                                       OfferStartTime = a.OfferStartTime,
                                       OfferEndTime = a.OfferEndTime,
                                       OfferQtyAvaiable = a.OfferQtyAvaiable,
                                       OfferQtyConsumed = a.OfferQtyConsumed,
                                       OfferId = a.OfferId,
                                       OfferType = a.OfferType,
                                       OfferWalletPoint = a.OfferWalletPoint,
                                       OfferFreeItemId = a.OfferFreeItemId,
                                       OfferPercentage = a.OfferPercentage,
                                       OfferFreeItemName = a.OfferFreeItemName,
                                       OfferFreeItemImage = a.OfferFreeItemImage,
                                       OfferFreeItemQuantity = a.OfferFreeItemQuantity,
                                       OfferMinimumQty = a.OfferMinimumQty,
                                       FlashDealSpecialPrice = a.FlashDealSpecialPrice,
                                       ItemMultiMRPId = a.ItemMultiMRPId,
                                       BillLimitQty = a.BillLimitQty
                                   }).OrderByDescending(x => x.ItemNumber).ToList();

                    foreach (var it in itemMasters)
                    {
                        if (item.ItemMasters == null)
                        {
                            item.ItemMasters = new List<factoryItemdata>();
                        }
                        try
                        {/// Dream Point Logic && Margin Point
                            int? MP, PP;
                            double xPoint = xPointValue * 10;
                            //salesman 0.2=(0.02 * 10=0.2)
                            if (it.promoPerItems.Equals(null) && it.promoPerItems == null)
                            {
                                PP = 0;
                            }
                            else
                            {
                                PP = it.promoPerItems;
                            }
                            if (it.marginPoint.Equals(null) && it.promoPerItems == null)
                            {
                                MP = 0;
                            }
                            else
                            {
                                double WithTaxNetPurchasePrice = Math.Round(it.NetPurchasePrice * (1 + (it.TotalTaxPercentage / 100)), 3);//With tax
                                MP = Convert.ToInt32((it.UnitPrice - WithTaxNetPurchasePrice) * xPoint); // (UnitPrice-NPP withtax) * By xpoint 
                            }
                            if (PP > 0 && MP > 0)
                            {
                                int? PP_MP = PP + MP;
                                it.dreamPoint = PP_MP;
                            }
                            else if (MP > 0)
                            {
                                it.dreamPoint = MP;
                            }
                            else if (PP > 0)
                            {
                                it.dreamPoint = PP;
                            }
                            else
                            {
                                it.dreamPoint = 0;
                            }
                            // Margin % On app site logic ((MRP-UnitPrice)*100)/UnitPrice
                            if (it.price > it.UnitPrice)
                            {
                                it.marginPoint = ((it.price - it.UnitPrice) * 100) / it.UnitPrice;//MP;  we replce marginpoint value by margin for app here 
                            }
                            else
                            {
                                it.marginPoint = 0;
                            }
                        }
                        catch { }
                        item.ItemMasters.Add(it);
                    }
                    if (itemMasters.Count() != 0)
                    {
                        var res = new
                        {
                            offerData = itemMasters,
                            Status = true,
                            Message = "Success."
                        };
                        return Request.CreateResponse(HttpStatusCode.OK, res);
                    }
                    else
                    {
                        var res = new
                        {
                            offerData = itemMasters,
                            Status = false,
                            Message = "Fail"
                        };
                        return Request.CreateResponse(HttpStatusCode.OK, res);
                    }
                }
                catch (Exception ee)
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, ee.Message);
                }
            }
        }

        ////For Mobile API Showing item in that there are current offer.
        //[Route("GetOfferItemForAgent")]
        //[HttpGet]
        //public HttpResponseMessage GetOfferItemForAgent(int PeopleId, int WarehouseId)
        //{
        //    using (var context = new AuthContext())
        //    {
        //        try
        //        {
        //            WROFFERTEM item = new WROFFERTEM();
        //            //CurrentDate = DateTime.Now;
        //            List<factoryItemdata> itemMasters = new List<factoryItemdata>();
        //            itemMasters = (from a in context.itemMasters
        //                           where (a.WarehouseId == WarehouseId && a.OfferStartTime <= indianTime
        //                           && a.OfferEndTime >= indianTime && a.OfferCategory == 1 && a.active == true && a.Deleted == false && (a.ItemAppType == 0 || a.ItemAppType == 1))
        //                           join b in context.ItemMasterCentralDB on a.SellingSku equals b.SellingSku
        //                           join c in context.OfferDb on a.OfferId equals c.OfferId
        //                           where (c.IsActive == true && c.IsDeleted == false && c.OfferAppType == "Sales App" || c.OfferAppType == "Both")
        //                           let limit = context.ItemLimitMasterDB.Where(p2 => a.ItemMultiMRPId == p2.ItemMultiMRPId && a.Number == p2.ItemNumber && a.WarehouseId == p2.WarehouseId).FirstOrDefault()
        //                           select new factoryItemdata
        //                           {
        //                               WarehouseId = a.WarehouseId,
        //                               IsItemLimit = limit != null ? limit.IsItemLimit : false,
        //                               ItemlimitQty = limit != null && limit.IsItemLimit ? limit.ItemlimitQty : 0,
        //                               CompanyId = a.CompanyId,
        //                               Categoryid = b.Categoryid,
        //                               Discount = b.Discount,
        //                               ItemId = a.ItemId,
        //                               ItemNumber = b.Number,
        //                               //itemname = a.HindiName != null ? a.HindiName : a.itemname,
        //                               itemname = a.itemname,
        //                               LogoUrl = b.LogoUrl,
        //                               MinOrderQty = b.MinOrderQty,
        //                               price = a.price,
        //                               SubCategoryId = b.SubCategoryId,
        //                               SubsubCategoryid = b.SubsubCategoryid,
        //                               TotalTaxPercentage = b.TotalTaxPercentage,
        //                               SellingUnitName = b.SellingUnitName,
        //                               SellingSku = b.SellingSku,
        //                               UnitPrice = a.UnitPrice,
        //                               HindiName = a.HindiName != null ? a.HindiName : a.itemname,
        //                               VATTax = b.VATTax,
        //                               active = a.active,
        //                               marginPoint = a.marginPoint,
        //                               NetPurchasePrice = a.NetPurchasePrice,
        //                               promoPerItems = a.promoPerItems,
        //                               IsOffer = a.IsOffer,
        //                               Deleted = a.Deleted,
        //                               OfferCategory = a.OfferCategory,
        //                               OfferStartTime = a.OfferStartTime,
        //                               OfferEndTime = a.OfferEndTime,
        //                               OfferQtyAvaiable = a.OfferQtyAvaiable,
        //                               OfferQtyConsumed = a.OfferQtyConsumed,
        //                               OfferId = a.OfferId,
        //                               OfferType = a.OfferType,
        //                               OfferWalletPoint = a.OfferWalletPoint,
        //                               OfferFreeItemId = a.OfferFreeItemId,
        //                               OfferPercentage = a.OfferPercentage,
        //                               OfferFreeItemName = a.OfferFreeItemName,
        //                               OfferFreeItemImage = a.OfferFreeItemImage,
        //                               OfferFreeItemQuantity = a.OfferFreeItemQuantity,
        //                               OfferMinimumQty = a.OfferMinimumQty,
        //                               FlashDealSpecialPrice = a.FlashDealSpecialPrice,
        //                               BillLimitQty = a.BillLimitQty
        //                           }).OrderByDescending(x => x.ItemNumber).ToList();

        //            foreach (var it in itemMasters)
        //            {
        //                if (item.ItemMasters == null)
        //                {
        //                    item.ItemMasters = new List<factoryItemdata>();
        //                }
        //                try
        //                {/// Dream Point Logic && Margin Point
        //                    if (!it.IsOffer)
        //                    {
        //                        /// Dream Point Logic && Margin Point
        //                        int? MP, PP;
        //                        double xPoint = xPointValue * 10;
        //                        //salesman 0.2=(0.02 * 10=0.2)
        //                        if (it.promoPerItems.Equals(null) && it.promoPerItems == null)
        //                        {
        //                            PP = 0;
        //                        }
        //                        else
        //                        {
        //                            PP = it.promoPerItems;
        //                        }
        //                        if (it.marginPoint.Equals(null) && it.promoPerItems == null)
        //                        {
        //                            MP = 0;
        //                        }
        //                        else
        //                        {
        //                            double WithTaxNetPurchasePrice = Math.Round(it.NetPurchasePrice * (1 + (it.TotalTaxPercentage / 100)), 3);//With tax
        //                            MP = Convert.ToInt32((it.UnitPrice - WithTaxNetPurchasePrice) * xPoint); // (UnitPrice-NPP withtax) * By xpoint 
        //                        }
        //                        if (PP > 0 && MP > 0)
        //                        {
        //                            int? PP_MP = PP + MP;
        //                            it.dreamPoint = PP_MP;
        //                        }
        //                        else if (MP > 0)
        //                        {
        //                            it.dreamPoint = MP;
        //                        }
        //                        else if (PP > 0)
        //                        {
        //                            it.dreamPoint = PP;
        //                        }
        //                        else
        //                        {
        //                            it.dreamPoint = 0;
        //                        }

        //                    }
        //                    else
        //                    {
        //                        it.dreamPoint = 0;
        //                    }
        //                    // Margin % On app site logic ((MRP-UnitPrice)*100)/UnitPrice
        //                    if (it.price > it.UnitPrice)
        //                    {
        //                        it.marginPoint = ((it.price - it.UnitPrice) * 100) / it.UnitPrice;//MP;  we replce marginpoint value by margin for app here 
        //                    }
        //                    else
        //                    {
        //                        it.marginPoint = 0;
        //                    }
        //                }
        //                catch { }
        //                item.ItemMasters.Add(it);
        //            }
        //            if (itemMasters.Count() != 0)
        //            {
        //                var res = new
        //                {
        //                    offerData = itemMasters,
        //                    Status = true,
        //                    Message = "Success."
        //                };
        //                return Request.CreateResponse(HttpStatusCode.OK, res);
        //            }
        //            else
        //            {
        //                var res = new
        //                {
        //                    offerData = itemMasters,
        //                    Status = false,
        //                    Message = "Fail"
        //                };
        //                return Request.CreateResponse(HttpStatusCode.OK, res);
        //            }
        //        }
        //        catch (Exception ee)
        //        {
        //            return Request.CreateResponse(HttpStatusCode.BadRequest, ee.Message);
        //        }
        //    }
        //}

        [Route("GetPercentageOfferItemForMobile")]
        [HttpGet]
        public HttpResponseMessage GetPercentageOfferItemForMobile(DateTime CurrentDate, int WarehouseId)
        {
            using (var context = new AuthContext())
            {
                try
                {
                    List<Offer> offerData = context.OfferDb.Where(x => x.WarehouseId == WarehouseId && x.start <= CurrentDate && x.end >= CurrentDate && x.IsActive == true && x.IsDeleted == false && x.FreeOfferType == "Percentage" && x.OfferOn == "BillDiscount").ToList();
                    if (offerData.Count() != 0)
                    {
                        var res = new
                        {
                            offerData = offerData,
                            Status = true,

                            Message = "Success."
                        };
                        return Request.CreateResponse(HttpStatusCode.OK, res);
                    }
                    else
                    {
                        var res = new
                        {
                            offerData = offerData,
                            Status = false,
                            Message = "Fail"
                        };
                        return Request.CreateResponse(HttpStatusCode.OK, res);
                    }
                }
                catch (Exception ee)
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, ee.Message);
                }
            }
        }

        /// <summary>
        /// Get Customer Group Lists By Warehouse Id  Id 17/06/2019
        /// </summary>
        /// <param name="WarehouseId"></param>
        /// <returns></returns>
        [Authorize]
        [Route("CustomerGroupList")]
        [HttpGet]
        public HttpResponseMessage GetCustomerGroupList(int WarehouseId)
        {
            using (var context = new AuthContext())
            {

                try
                {
                    List<GroupSMS> GroupMapp = new List<GroupSMS>();

                    {
                        GroupMapp = context.GroupsSms.Where(x => x.Deleted == false).ToList();




                        if (GroupMapp.Count > 0)
                        {
                            return Request.CreateResponse(HttpStatusCode.OK, GroupMapp);
                        }
                    }
                    return Request.CreateResponse(HttpStatusCode.OK, GroupMapp);
                }
                catch (Exception ee)
                {

                    return Request.CreateResponse(HttpStatusCode.BadRequest, "null" + ee.Message);
                }
            }
        }
        [AllowAnonymous]
        [Route("GetItemName")]
        [HttpGet]
        public List<ItemDC> GetItemName(int warehouseId)
        {
            List<ItemDC> itemdc = new List<ItemDC>();
            using (var context = new AuthContext())
            {
                itemdc = context.Database.SqlQuery<ItemDC>("exec GetItemName " + warehouseId).ToList();
                return itemdc;

            }
        }
        [AllowAnonymous]
        [Route("NewCustomerGroupList")]
        [HttpGet]
        public HttpResponseMessage GetNewCustomerGroupList(int? StoreId, string AppType)
        {
            using (var context = new AuthContext())
            {

                if (context.Database.Connection.State != System.Data.ConnectionState.Open)
                    context.Database.Connection.Open();

                if (!StoreId.HasValue)
                    StoreId = -2;
                var param1 = new SqlParameter("@storeId", StoreId.Value);

                var cmd = context.Database.Connection.CreateCommand();
                cmd.CommandText = "[dbo].[getGroupsForOffer]";
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.Parameters.Add(param1);

                // Run the sproc
                var reader = cmd.ExecuteReader();
                var minSalesGroups = ((IObjectContextAdapter)context)
                 .ObjectContext
                 .Translate<MinSalesGroup>(reader).ToList();

                //List<Model.SalesApp.SalesGroup> GroupMapping = new List<Model.SalesApp.SalesGroup>();
                //{
                //if (StoreId != null)
                //{
                //    if (AppType == "Both" || AppType == "SalesApp")
                //    {
                //        GroupMapping = context.SalesGroupDb.Where(x => x.SalesGroupCustomers.Any() &&  x.StoreId == StoreId && x.Type == "Yes" && x.StoreId != -1 && x.ValidityDate.HasValue && x.ValidityDate.Value >= DateTime.Now && x.IsActive == true && x.IsDeleted == false).Include(x=>x.SalesGroupCustomers).ToList();

                //    }
                //    else
                //    {
                //        GroupMapping = context.SalesGroupDb.Where(x => x.SalesGroupCustomers.Any() && x.StoreId == StoreId && x.StoreId != -1 && x.ValidityDate.HasValue && x.ValidityDate.Value >= DateTime.Now && x.IsActive == true && x.IsDeleted == false).ToList();
                //    }
                //}
                //else
                //{
                //    if (AppType == "Both" || AppType == "SalesApp")
                //    {
                //        GroupMapping = context.SalesGroupDb.Where(x => x.IsActive == true && x.Type == "Yes" && x.StoreId != -1 && x.ValidityDate.HasValue && x.ValidityDate.Value >= DateTime.Now && x.IsActive == true && x.IsDeleted == false).ToList();
                //    }
                //    else
                //    {
                //        GroupMapping = context.SalesGroupDb.Where(x => x.IsActive == true && x.StoreId != -1 && x.ValidityDate.HasValue && x.ValidityDate.Value >= DateTime.Now && x.IsActive == true && x.IsDeleted == false).ToList();
                //    }
                //}
                //    if (GroupMapping.Count > 0)
                //    {
                //        return Request.CreateResponse(HttpStatusCode.OK, GroupMapping);
                //    }
                //}
                return Request.CreateResponse(HttpStatusCode.OK, minSalesGroups);

            }
        }
        /// <summary>
        /// Get  CustomerList By Warehouse Id 17/06/2019
        /// </summary>
        /// <param name="WarehouseId"></param>
        /// <returns></returns>
        [Authorize]
        [Route("CustomerList")]
        [HttpGet]
        public HttpResponseMessage GetCustomerList(int WarehouseId)
        {
            using (var context = new AuthContext())
            {

                try
                {
                    List<Customer> Customer = new List<Customer>();

                    {
                        Customer = context.Customers.Where(x => x.Deleted == false && x.Warehouseid == WarehouseId && x.Active == true && x.Deleted == false).ToList();
                        if (Customer.Count > 0)
                        {
                            return Request.CreateResponse(HttpStatusCode.OK, Customer);
                        }
                    }
                    return Request.CreateResponse(HttpStatusCode.OK, Customer);
                }
                catch (Exception ee)
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "null" + ee.Message);
                }
            }
        }



        [Authorize]
        [Route("GetCustomerBySkCode")]
        [HttpGet]
        public HttpResponseMessage GetCustomerBySkCode(string skcode)
        {
            using (var context = new AuthContext())
            {
                List<Customer> Customer = new List<Customer>();
                {
                    Customer = context.Customers.Where(x => x.Skcode.Contains(skcode) && x.Active == true && x.Deleted == false).ToList();
                    if (Customer.Count > 0)
                    {
                        return Request.CreateResponse(HttpStatusCode.OK, Customer);
                    }
                }
                return Request.CreateResponse(HttpStatusCode.OK, Customer);

            }
        }

        /// <summary>
        /// Get Group  By Warehouse Id & GroupId  Id 17/06/2019
        /// </summary>
        /// <param name="WarehouseId"></param>
        /// <param name="GroupId"></param>
        /// <returns></returns>
        [Authorize]
        [Route("GetGroupOnEdit")]
        [HttpGet]
        public HttpResponseMessage GetGroup(int WarehouseId, int GroupId)
        {
            using (var context = new AuthContext())
            {
                try
                {
                    GroupSMS GroupSMS = new GroupSMS();

                    {
                        GroupSMS = context.GroupsSms.Where(x => x.Deleted == false && x.GroupID == GroupId).FirstOrDefault();
                        if (GroupSMS != null)
                        {
                            return Request.CreateResponse(HttpStatusCode.OK, GroupSMS);
                        }
                    }
                    return Request.CreateResponse(HttpStatusCode.OK, GroupSMS);
                }
                catch (Exception ee)
                {

                    return Request.CreateResponse(HttpStatusCode.BadRequest, "null" + ee.Message);
                }
            }
        }

        /// <summary>
        /// Get  Customer  By Warehouse Id & Offer Id 17/06/2019
        /// </summary>
        /// <param name="WarehouseId"></param>
        /// /// <param name="WarehouseId"></param>
        /// <returns></returns>
        [Authorize]
        [Route("GetCustomerOnEdit")]
        [HttpGet]
        public HttpResponseMessage GetCustomerList(int WarehouseId, int OfferId)
        {
            using (var context = new AuthContext())
            {
                try
                {
                    Customer Customer = new Customer();

                    {
                        BillDiscount Billdiscount = context.BillDiscountDb.Where(x => x.OfferId == OfferId).FirstOrDefault();
                        if (Billdiscount != null)
                        {
                            Customer = context.Customers.Where(x => x.CustomerId == Billdiscount.CustomerId && x.Warehouseid == WarehouseId && x.Active == true && x.Deleted == false).FirstOrDefault();
                            if (Customer != null)
                            {
                                return Request.CreateResponse(HttpStatusCode.OK, Customer);
                            }
                        }

                    }
                    return Request.CreateResponse(HttpStatusCode.OK, Customer);
                }
                catch (Exception ee)
                {

                    return Request.CreateResponse(HttpStatusCode.BadRequest, "null" + ee.Message);
                }
            }
        }
        #region for serach to get  item List
        /// <summary>
        /// Created Date:27/06/2019
        /// Created By Raj
        /// </summary>
        /// <param name="key"></param>
        /// <param name="Warehouseid"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("SearchinitemOfferadd")]
        public IEnumerable<ItemMaster> searchsPOadd(string key, int Warehouseid)
        {
            List<ItemMaster> ass = new List<ItemMaster>();
            List<ItemMaster> result = new List<ItemMaster>();
            using (var context = new AuthContext())
            {
                try
                {
                    if (Warehouseid > 0)
                    {
                        //ass = context.itemMasters.Where(t => t.itemname.Contains(key) && t.WarehouseId == Warehouseid && t.Deleted == false).ToList();
                        ass = context.itemMasters.Where(t => (t.itemname.Contains(key) || t.SellingSku.Contains(key)) && t.WarehouseId == Warehouseid && t.Deleted == false).ToList();

                        foreach (var item in ass.GroupBy(x => new { x.SellingSku, x.ItemMultiMRPId }))
                        {
                            result.Add(item.ToList().FirstOrDefault());
                        }
                        return result;
                    }

                    else
                    {
                        return null;
                    }
                }
                catch (Exception ex)
                {
                    logger.Error("Error in Item Master " + ex.Message);
                    return null;
                }
            }
        }

        //Ritika----------------------------------------------
        [HttpGet]
        [Route("SearchBySkuAndItemName")]
        public IEnumerable<ItemMaster> searchData(string key, int Warehouseid)
        {
            List<ItemMaster> ass = new List<ItemMaster>();
            List<ItemMaster> result = new List<ItemMaster>();
            using (var context = new AuthContext())
            {
                try
                {
                    if (Warehouseid > 0)
                    {
                        ass = context.itemMasters.Where(t => (t.itemname.Contains(key) || t.SellingSku.Contains(key)) && t.WarehouseId == Warehouseid && t.Deleted == false).ToList();

                        foreach (var item in ass.GroupBy(x => new { x.SellingSku, x.ItemMultiMRPId }))
                        {
                            result.Add(item.ToList().FirstOrDefault());
                        }
                        return result;
                    }

                    else
                    {
                        return null;
                    }
                }
                catch (Exception ex)
                {
                    logger.Error("Error in Item Master " + ex.Message);
                    return null;
                }
            }
        }
        //end---------------------------------------------
        [HttpGet]
        [Route("SearchRDSitem")]
        public IEnumerable<ItemMaster> SearchRDSitem(string key, int Warehouseid)
        {
            List<ItemMaster> ass = new List<ItemMaster>();
            List<ItemMaster> result = new List<ItemMaster>();
            using (var context = new AuthContext())
            {
                try
                {
                    if (Warehouseid > 0)
                    {
                        ass = context.itemMasters.Where(t => t.itemname.Contains(key) && t.WarehouseId == Warehouseid && t.Deleted == false && t.DistributorShow).ToList();

                        foreach (var item in ass.GroupBy(x => x.SellingSku))
                        {
                            result.Add(item.ToList().FirstOrDefault());
                        }
                        return result;
                    }

                    else
                    {
                        return null;
                    }
                }
                catch (Exception ex)
                {
                    logger.Error("Error in Item Master " + ex.Message);
                    return null;
                }
            }
        }

        [HttpGet]
        [Route("GetItemStock")]
        public BillDiscountFreeItem GetItemWithStock(int itemId, int multiMRPId, int Warehouseid, bool IsFreeStock)
        {
            BillDiscountFreeItem billDiscountFreeItem = new BillDiscountFreeItem { ItemId = itemId };
            int stock = 0;
            using (var context = new AuthContext())
            {
                if (!IsFreeStock)
                {
                    stock = context.DbCurrentStock.FirstOrDefault(x => x.ItemMultiMRPId == multiMRPId && x.WarehouseId == Warehouseid).CurrentInventory;
                }
                else
                {
                    stock = context.FreeStockDB.FirstOrDefault(x => x.ItemMultiMRPId == multiMRPId && x.WarehouseId == Warehouseid)?.CurrentInventory ?? 0;
                }
            }
            billDiscountFreeItem.StockQty = stock;
            billDiscountFreeItem.OfferStockQty = stock;
            billDiscountFreeItem.StockType = IsFreeStock ? 2 : 1;
            billDiscountFreeItem.Qty = 1;
            return billDiscountFreeItem;
        }
        #endregion
        #region for serach to get  item List
        /// <summary>
        /// Created Date:27/06/2019
        /// Created By Raj
        /// </summary>
        /// <param name="key"></param>
        /// <param name="Warehouseid"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("offercustomerlist")]
        public List<CustomerOfferVM> OfferCustomerList(int OfferId)
        {
            logger.Info("start Item Master: ");
            using (var context = new AuthContext())
            {
                try
                {
                    var offercustomerlist = from c in context.Customers.Where(c => c.Active == true)
                                            join p in context.BillDiscountDb.Where(o => o.OfferId == OfferId && o.IsActive && (o.IsDeleted.HasValue && !o.IsDeleted.Value))
                                            on c.CustomerId equals p.CustomerId
                                            join k in context.Warehouses on c.Warehouseid equals k.WarehouseId
                                            join a in context.OfferDb.Where(x => x.OfferId == OfferId)
                                            on p.OfferId equals a.OfferId
                                            select new CustomerOfferVM()
                                            {
                                                CustomerId = c.CustomerId,
                                                Skcode = c.Skcode,
                                                OfferCode = a.OfferCode,
                                                CustomerName = c.Name,
                                                IsScratchBDCode = p.IsScratchBDCode,
                                                BillDiscountType = p.BillDiscountType,
                                                CreatedDate = p.CreatedDate,
                                                OrderID = p.OrderId,
                                                Amount = p.BillDiscountAmount,
                                                OrderAmount = context.DbOrderMaster.Where(x => x.OrderId == p.OrderId).Sum(x => x.GrossAmount),  //i.GrossAmount,
                                                HubName = k.WarehouseName + " " + k.CityName
                                            };

                    List<CustomerOfferVM> customerData = offercustomerlist.ToList();
                    return customerData;
                }
                catch (Exception ex)
                {
                    logger.Error("Error in Item Master " + ex.Message);
                    logger.Info("End  Item Master: ");
                    return null;
                }
            }
        }


        [HttpGet]
        [Route("SendNotification")]
        public bool SendNotification(int OfferId)
        {
            using (var context = new AuthContext())
            {
                var fcmIdlist = from o in context.OfferDb.Where(x => x.OfferId == OfferId && x.IsActive && !x.IsDeleted && x.end > indianTime)
                                join c in context.BillDiscountDb.Where(c => c.OrderId == 0) on o.OfferId equals c.OfferId
                                join cust in context.Customers on c.CustomerId equals cust.CustomerId
                                select new { cust.fcmId, o.OfferOn };

                if (fcmIdlist != null && fcmIdlist.Any())
                    context.ScratchCardNotificationList(fcmIdlist.Select(x => x.fcmId).ToList(), fcmIdlist.FirstOrDefault().OfferOn, " Scratch and Apply code on Order.");

                return true;

            }
        }

        [HttpGet]
        [Route("GetcatSubCatName")]
        public dynamic GetNames(int offerid)
        {
            using (var context = new AuthContext())
            {
                try
                {
                    var GetcatName = (from off in context.OfferDb
                                      join
                                      categ in context.Categorys on off.Category equals categ.Categoryid
                                      join subcat in context.SubCategorys on off.subCategory equals subcat.SubCategoryId
                                      join subsubcat in context.SubsubCategorys on off.subSubCategory equals subsubcat.SubsubCategoryid
                                      where off.OfferId == offerid
                                      select new GetNamesOfOfferidCat
                                      {
                                          categorname = categ.CategoryName,
                                          subcatname = subcat.SubcategoryName,
                                          subsubcatname = subsubcat.SubsubcategoryName

                                      }).FirstOrDefault();

                    var query = "select isnull(c.CategoryName, '') as categorname , isnull(sc.SubcategoryName, '') as subcatname,isnull(ssc.SubcategoryName,'') as subsubcatname " +
                                 " from offers o left join Categories c on o.Category = c.Categoryid left join SubCategories sc on o.subCategory = sc.SubCategoryId" +
                                 " left join SubsubCategories ssc on o.subSubCategory = ssc.SubsubCategoryid  where o.OfferId = '" + offerid + "'";

                    var GetcatNames = context.Database.SqlQuery<GetNamesOfOfferidCat>(query).FirstOrDefault();
                    return GetcatNames;

                }
                catch (Exception ex)
                {
                    return null;
                    ex.Message.ToString();
                }

            }



        }

        [HttpGet]
        [Route("GetStores")]
        public dynamic GetStores()
        {
            using (var context = new AuthContext())
            {
                var stores = context.StoreDB.Where(x => x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value)).Select(x => new { StoreId = x.Id, x.Name }).ToList();
                return stores;
            }
        }
        [HttpGet]
        [AllowAnonymous]
        [Route("GetStoreBrandCompany")]
        public StoreBrandCompany GetStoreBrandCompany(long storeId)
        {
            StoreBrandCompany storeBrandCompany = new StoreBrandCompany();
            using (var context = new AuthContext())
            {
                List<BrandCompany> brandCompany = new List<BrandCompany>();

                var brandQuery = "select distinct  e.CategoryName +'>>' + d.SubcategoryName +'>>'+ a.[SubsubcategoryName] BrandName,  b.BrandCategoryMappingId , e.CategoryName +'>>' + d.SubcategoryName SubcategoryName, b.SubCategoryMappingId"
                                     + " from SubsubCategories a inner join BrandCategoryMappings b on a.SubsubCategoryid = b.SubsubCategoryId"
                                     + " Inner Join StoreBrands s on b.BrandCategoryMappingId=s.BrandCategoryMappingId and s.IsActive=1 and s.IsDeleted=0"
                                     + " Inner Join Stores st on st.id=s.StoreId "
                                     + " inner join SubcategoryCategoryMappings c on c.SubCategoryMappingId = b.SubCategoryMappingId"
                                     + " inner join SubCategories d on d.SubCategoryId = c.SubCategoryId"
                                     + " inner join Categories e on e.Categoryid = c.Categoryid"
                                     + " where a.IsActive = 1 and b.IsActive = 1 and a.Deleted = 0 and b.Deleted = 0 and d.IsActive = 1 and d.Deleted = 0 and c.IsActive = 1 " +
                                     " and c.Deleted = 0 and e.IsActive = 1 and e.Deleted = 0 and (s.StoreId=" + storeId + " or st.IsUniversal=1)";
                brandCompany = context.Database.SqlQuery<BrandCompany>(brandQuery).ToList();

                storeBrandCompany.Brands = brandCompany.Select(x => new OfferDetail { Id = x.BrandCategoryMappingId, Name = x.BrandName }).Distinct().ToList();
                storeBrandCompany.Companys = brandCompany.Select(x => new { Id = x.SubCategoryMappingId, Name = x.SubcategoryName }).Distinct().Select(x => new OfferDetail { Id = x.Id, Name = x.Name }).ToList();
            }
            return storeBrandCompany;
        }

        [HttpGet]
        [Route("GetCompanyBrand")]
        public StoreBrandCompany GetCompanyBrand()
        {
            StoreBrandCompany storeBrandCompany = new StoreBrandCompany();
            using (var context = new AuthContext())
            {
                List<BrandCompany> brandCompany = new List<BrandCompany>();

                var brandQuery = "select distinct  e.CategoryName +'>>' + d.SubcategoryName +'>>'+ a.[SubsubcategoryName] BrandName,a.[SubsubcategoryName],  b.BrandCategoryMappingId , e.CategoryName +'>>' + d.SubcategoryName SubcategoryName, b.SubCategoryMappingId"
                                     + " from SubsubCategories a inner join BrandCategoryMappings b on a.SubsubCategoryid = b.SubsubCategoryId"
                                     + " inner join SubcategoryCategoryMappings c on c.SubCategoryMappingId = b.SubCategoryMappingId"
                                     + " inner join SubCategories d on d.SubCategoryId = c.SubCategoryId"
                                     + " inner join Categories e on e.Categoryid = c.Categoryid"
                                     + " where a.IsActive = 1 and b.IsActive = 1 and a.Deleted = 0 and b.Deleted = 0 and d.IsActive = 1 and d.Deleted = 0 and c.IsActive = 1 " +
                                     " and c.Deleted = 0 and e.IsActive = 1 and e.Deleted = 0 ";
                brandCompany = context.Database.SqlQuery<BrandCompany>(brandQuery).ToList();

                storeBrandCompany.Brands = brandCompany.Select(x => new OfferDetail { Id = x.BrandCategoryMappingId, Name = x.BrandName, ItemName = x.SubsubcategoryName }).Distinct().ToList();
                storeBrandCompany.Companys = brandCompany.Select(x => new { Id = x.SubCategoryMappingId, Name = x.SubcategoryName }).Distinct().Select(x => new OfferDetail { Id = x.Id, Name = x.Name }).ToList();
            }
            return storeBrandCompany;
        }


        [HttpGet]
        [Route("GetWarehouseCategrory")]
        public List<OfferDetail> GetCategory()
        {
            List<OfferDetail> OfferDetail = new List<OfferDetail>();
            using (var context = new AuthContext())
            {

                OfferDetail = context.Categorys.Where(x => x.IsActive && !x.Deleted).OrderBy(x => x.CategoryName).Select(x => new OfferDetail
                {
                    Id = x.Categoryid,
                    Name = x.CategoryName
                }).ToList();
            }

            return OfferDetail;
        }

        [HttpGet]
        [Route("GetWarehouseSubCategrory")]
        public List<OfferDetail> GetSubCategory()
        {
            List<OfferDetail> OfferDetail = new List<OfferDetail>();
            using (var context = new AuthContext())
            {
                var subCategoryQuery = "select distinct b.SubCategoryMappingId Id, d.CategoryName + '>>'+ a.[SubcategoryName]  Name from SubCategories a inner join SubcategoryCategoryMappings b on a.SubCategoryid=b.subCategoryid inner join Categories d on b.Categoryid=d.Categoryid and a.IsActive=1 and b.IsActive =1 and a.Deleted=0 and b.Deleted=0 and d.IsActive=1 and d.Deleted=0";
                OfferDetail = context.Database.SqlQuery<OfferDetail>(subCategoryQuery).ToList().OrderBy(x => x.Name).ToList();
                //OfferDetail = context.SubCategorys.Where(x => x.IsActive && !x.Deleted).Select(x => new OfferDetail
                //{
                //    Id = x.SubCategoryId,
                //    Name = x.CategoryName + ">>" + x.SubcategoryName
                //}).ToList().OrderBy(x => x.Name).ToList();
            }

            return OfferDetail;
        }

        [HttpGet]
        [Route("GetWarehouseBrand")]
        public List<OfferDetail> GetWarehouseBrand()
        {
            List<OfferDetail> OfferDetail = new List<OfferDetail>();
            using (var context = new AuthContext())
            {

                var brandQuery = "select distinct  e.CategoryName +'>>' + d.SubcategoryName +'>>'+ a.[SubsubcategoryName] Name,  b.BrandCategoryMappingId  Id"
                                     + " from SubsubCategories a inner"
                                     + " join BrandCategoryMappings b on a.SubsubCategoryid = b.SubsubCategoryId"
                                     + " inner join SubcategoryCategoryMappings c on c.SubCategoryMappingId = b.SubCategoryMappingId"
                                     + " inner join SubCategories d on d.SubCategoryId = c.SubCategoryId"
                                     + " inner join Categories e on e.Categoryid = c.Categoryid"
                                     + " where a.IsActive = 1 and b.IsActive = 1 and a.Deleted = 0 and b.Deleted = 0 and d.IsActive = 1 and d.Deleted = 0 and c.IsActive = 1 " +
                                     " and c.Deleted = 0 and e.IsActive = 1 and e.Deleted = 0";
                OfferDetail = context.Database.SqlQuery<OfferDetail>(brandQuery).ToList();
                //OfferDetail = context.SubsubCategorys.Where(x => x.IsActive && !x.Deleted).Select(x => new OfferDetail
                //{
                //    Id = x.SubsubCategoryid,
                //    Name = x.CategoryName + ">>" + x.SubcategoryName + ">>" + x.SubsubcategoryName
                //}).ToList().OrderBy(x => x.Name).ToList();
            }

            return OfferDetail;
        }

        [Route("GetItemsByType")]
        [HttpPost]
        public List<OfferDetail> GetItemsByType(itemdata data)
        {
            List<OfferDetail> OfferDetails = new List<OfferDetail>();
            using (var context = new AuthContext())
            {
                if (context.Database.Connection.State != System.Data.ConnectionState.Open)
                    context.Database.Connection.Open();

                var predicate = PredicateBuilder.True<ItemMaster>();

                predicate = predicate.And(x => x.WarehouseId == data.warehouseid && x.Deleted == false && x.IsDisContinued == false);
                if (data.type == "Category" && data.ids != null && data.ids.Any())
                {
                    predicate = predicate.And(x => data.ids.Contains(x.Categoryid));
                }

                if (data.type == "SubCategory" && data.ids != null && data.ids.Any())
                {
                    var subCategoryIds = context.SubcategoryCategoryMappingDb.Where(x => data.ids.Contains(x.SubCategoryMappingId)).Select(x => x.Categoryid + " " + x.SubCategoryId).ToList();

                    predicate = predicate.And(x => subCategoryIds.Contains(x.Categoryid + " " + x.SubCategoryId));
                }

                //if (data.type == "SubsubCategory" && data.ids != null && data.ids.Any())
                //{
                //    var subSubCategoryIds = context.BrandCategoryMappingDb.Where(x => data.ids.Contains(x.BrandCategoryMappingId)).Select(x => x.SubsubCategoryId).ToList();
                //    predicate = predicate.And(x => subSubCategoryIds.Contains(x.SubsubCategoryid));
                //}

                if (data.type == "SubsubCategory" && data.ids != null && data.ids.Any())
                {
                    System.Data.DataTable IdDt = new System.Data.DataTable();
                    IdDt.Columns.Add("IntValue");
                    foreach (var item in data.ids)
                    {
                        var dr = IdDt.NewRow();
                        dr["IntValue"] = item;
                        IdDt.Rows.Add(dr);
                    }

                    var param = new SqlParameter("id", IdDt);
                    param.SqlDbType = System.Data.SqlDbType.Structured;
                    param.TypeName = "dbo.IntValues";
                    var cmd = context.Database.Connection.CreateCommand();
                    cmd.Connection.State.ToString();

                    //if (cmd.Connection.State == cmd.Connection.Close())

                    //{
                    //mbd 27/06/20222
                    //  cmd.Connection.Open();
                    //}


                    cmd.CommandText = "[dbo].[GetOfferBrandCatMappingData]";
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.Parameters.Add(param);

                    var reader1 = cmd.ExecuteReader();
                    List<offerBrandCatMap> offerBrandCatMaps = ((IObjectContextAdapter)context)
                    .ObjectContext
                    .Translate<offerBrandCatMap>(reader1).ToList();

                    var lst = offerBrandCatMaps.Select(y => y.SubsubcategoryId + " " + y.SubcategoryId + " " + y.CategoryId).ToList();
                    //var subSubCategoryIds = context.BrandCategoryMappingDb.Where(x => data.ids.Contains(x.BrandCategoryMappingId)).Select(x => x.SubsubCategoryId).ToList();
                    predicate = predicate.And(x => lst.Contains(x.SubsubCategoryid + " " + x.SubCategoryId + " " + x.Categoryid));
                }

                OfferDetails = context.itemMasters.Where(predicate).Select(x => new OfferDetail
                {
                    Id = x.ItemId,
                    Name = x.CategoryName + "-" + x.SubcategoryName + "-" + x.SubsubcategoryName + "-" + x.itemname + "(" + x.Number + ")"
                }).ToList().OrderBy(x => x.Name).ToList();

                return OfferDetails;
            }

        }

        [Route("GetItemSearch")]
        [HttpGet]
        public List<OfferDetail> GetItemSearch(int WarehouseId, int? categoryId, int? subCategoryid, int? subSubCategoryId, float? margin, string itemClassification)//get all Issuances which are active for the delivery boy
        {
            List<OfferDetail> OfferDetails = new List<OfferDetail>();
            using (var db = new AuthContext())
            {

                var predicate = PredicateBuilder.True<ItemMaster>();

                predicate = predicate.And(x => x.WarehouseId == WarehouseId && x.Deleted == false && x.active == true);
                if (string.IsNullOrEmpty(itemClassification))
                {
                    if (categoryId.HasValue && categoryId.Value > 0)
                    {
                        predicate = predicate.And(x => x.Categoryid == categoryId);
                    }

                    if (subCategoryid.HasValue && subCategoryid.Value > 0)
                    {
                        predicate = predicate.And(x => x.SubCategoryId == subCategoryid);
                    }

                    if (subSubCategoryId.HasValue && subSubCategoryId.Value > 0)
                    {
                        predicate = predicate.And(x => x.SubsubCategoryid == subSubCategoryId);
                    }
                    if (margin.HasValue && margin.Value > 0)
                    {
                        predicate = predicate.And(x => x.Margin >= margin.Value);
                    }
                }
                else
                {
                    string Sql = " select a.Itemnumber from ItemsClassification a where a.warehouseid = " + WarehouseId + " and CHARINDEX('" + itemClassification + "', Category) > 0";
                    List<string> itemnumbers = db.Database.SqlQuery<string>(Sql).ToList();
                    if (itemnumbers != null && itemnumbers.Any())
                    {
                        predicate = predicate.And(x => itemnumbers.Contains(x.Number));
                    }
                    else
                    {
                        predicate = predicate.And(x => x.ItemId == -1);
                    }
                }

                if (WarehouseId > 0)
                {
                    OfferDetails = db.itemMasters.Where(predicate).Select(x => new OfferDetail
                    {
                        Id = x.ItemId,
                        //Name = x.CategoryName + "-" + x.SubcategoryName + "-" + x.SubsubcategoryName + "-" + x.itemname + "(" + x.Number + ")"
                        Name = x.CategoryName + "-" + x.SubcategoryName + "-" + x.SubsubcategoryName + "-" + x.itemname + "(" + x.SellingSku + ")"
                    }).ToList().OrderBy(x => x.Name).ToList();
                }
            }

            return OfferDetails;
        }

        [Route("GetItemSearchByKeyWord")]
        [HttpPost]
        [AllowAnonymous]
        public List<OfferDetail> GetItemSearchByKeyWord(OfferItemSearchRequest offerItemSearchRequest)
        {
            List<OfferDetail> OfferDetails = new List<OfferDetail>();
            using (var db = new AuthContext())
            {
                var predicate = PredicateBuilder.True<ItemMaster>();

                var Warehouse = db.Warehouses.FirstOrDefault(x=>x.WarehouseId == offerItemSearchRequest.WarehouseId);
                if(Warehouse != null && Warehouse.IsStore == true && Warehouse.StoreType == 1)
                {
                    predicate = predicate.And(x => x.WarehouseId == offerItemSearchRequest.WarehouseId && x.MinOrderQty==1 && x.itemname.Contains(offerItemSearchRequest.keyword) && x.Deleted == false && !x.IsDisContinued);
                }
                else
                {
                    predicate = predicate.And(x => x.WarehouseId == offerItemSearchRequest.WarehouseId && x.itemname.Contains(offerItemSearchRequest.keyword) && x.Deleted == false && !x.IsDisContinued);

                }



                if (string.IsNullOrEmpty(offerItemSearchRequest.itemClassification))
                {
                    if (offerItemSearchRequest.subCategoryMappingids != null && offerItemSearchRequest.subCategoryMappingids.Any())
                    {
                        var subCategoryIds = db.SubcategoryCategoryMappingDb.Where(x => offerItemSearchRequest.subCategoryMappingids.Contains(x.SubCategoryMappingId) && !x.Deleted && x.IsActive).Select(x => x.SubCategoryId).Distinct().ToList();

                        predicate = predicate.And(x => subCategoryIds.Contains(x.SubCategoryId));
                    }

                    if (offerItemSearchRequest.brandCategoryMappingIds != null && offerItemSearchRequest.brandCategoryMappingIds.Any())
                    {
                        var subSubCategoryIds = db.BrandCategoryMappingDb.Where(x => offerItemSearchRequest.brandCategoryMappingIds.Contains(x.BrandCategoryMappingId) && !x.Deleted && x.IsActive).Select(x => x.SubsubCategoryId).Distinct().ToList();
                        predicate = predicate.And(x => subSubCategoryIds.Contains(x.SubsubCategoryid));
                    }

                    if (offerItemSearchRequest.margin.HasValue && offerItemSearchRequest.margin.Value > 0)
                    {
                        predicate = predicate.And(x => x.Margin >= offerItemSearchRequest.margin.Value);
                    }
                }
                else
                {
                    string Sql = " select a.Itemnumber from ItemsClassification a where a.warehouseid = " + offerItemSearchRequest.WarehouseId + " and CHARINDEX('" + offerItemSearchRequest.itemClassification + "', Category) > 0";
                    List<string> itemnumbers = db.Database.SqlQuery<string>(Sql).ToList();
                    if (itemnumbers != null && itemnumbers.Any())
                    {
                        predicate = predicate.And(x => itemnumbers.Contains(x.Number));
                    }
                }

                OfferDetails = db.itemMasters.Where(predicate).Select(x => new OfferDetail
                {
                    Itemid = x.ItemId,
                    Id = x.ItemMultiMRPId,
                    MinOrderQty = x.MinOrderQty,
                    UnitPrice = x.UnitPrice == 0 ? x.price : x.UnitPrice,
                    Name = x.CategoryName + "-" + x.SubcategoryName + "-" + x.SubsubcategoryName + "-" + x.itemname + "(NO#-" + x.Number + " MOQ-" + x.MinOrderQty + ") - " + x.active
                }).ToList().OrderBy(x => x.Name).ToList();

            }

            return OfferDetails;
        }


        [Route("GetOfferItemSearchByKeyWord")]
        [HttpPost]
        [AllowAnonymous]
        public List<OfferDetail> GetOfferItemSearchByKeyWord(OfferItemSearchRequest offerItemSearchRequest)
        {
            List<OfferDetail> OfferDetails = new List<OfferDetail>();
            using (var db = new AuthContext())
            {

                var predicate = PredicateBuilder.True<ItemMaster>();

                predicate = predicate.And(x => x.WarehouseId == offerItemSearchRequest.WarehouseId && !string.IsNullOrEmpty(x.itemname) && x.itemname.ToLower().Contains(offerItemSearchRequest.keyword.ToLower()) && x.Deleted == false && !x.IsDisContinued);

                if (string.IsNullOrEmpty(offerItemSearchRequest.itemClassification))
                {
                    if (offerItemSearchRequest.subCategoryMappingids != null && offerItemSearchRequest.subCategoryMappingids.Any())
                    {
                        var subCategoryIds = db.SubcategoryCategoryMappingDb.Where(x => offerItemSearchRequest.subCategoryMappingids.Contains(x.SubCategoryMappingId) && !x.Deleted && x.IsActive).Select(x => x.SubCategoryId).Distinct().ToList();

                        predicate = predicate.And(x => subCategoryIds.Contains(x.SubCategoryId));
                    }
                    //mbd 8 june 2022
                    if (offerItemSearchRequest.brandCategoryMappingIds == null && offerItemSearchRequest.brandCategoryMappingIds.Any())
                    {
                        var subSubCategoryIds = db.BrandCategoryMappingDb.Where(x => offerItemSearchRequest.brandCategoryMappingIds.Contains(x.BrandCategoryMappingId) && !x.Deleted && x.IsActive).Select(x => x.SubsubCategoryId).Distinct().ToList();
                        predicate = predicate.And(x => subSubCategoryIds.Contains(x.SubsubCategoryid));
                    }

                    if (offerItemSearchRequest.margin.HasValue && offerItemSearchRequest.margin.Value > 0)
                    {
                        predicate = predicate.And(x => x.Margin >= offerItemSearchRequest.margin.Value);
                    }
                }
                else
                {
                    string Sql = " select a.Itemnumber from ItemsClassification a where a.warehouseid = " + offerItemSearchRequest.WarehouseId + " and CHARINDEX('" + offerItemSearchRequest.itemClassification + "', Category) > 0";
                    List<string> itemnumbers = db.Database.SqlQuery<string>(Sql).ToList();
                    if (itemnumbers != null && itemnumbers.Any())
                    {
                        predicate = predicate.And(x => itemnumbers.Contains(x.Number));
                    }
                }

                OfferDetails = db.itemMasters.Where(predicate).Select(x => new OfferDetail
                {
                    Itemid = x.ItemId,
                    Id = x.ItemMultiMRPId,
                    MinOrderQty = x.MinOrderQty,
                    UnitPrice = x.UnitPrice == 0 ? x.price : x.UnitPrice,
                    ItemName = x.itemname,
                    Name = x.CategoryName + "-" + x.SubcategoryName + "-" + x.SubsubcategoryName + "-" + x.itemname + "(NO#-" + x.Number + " MOQ-" + x.MinOrderQty + ") - " + x.active
                }).ToList();

                OfferDetails = OfferDetails.GroupBy(x => x.Id).Select(x => new OfferDetail
                {
                    Id = x.Key,
                    Name = x.FirstOrDefault().Name,
                    ItemName = x.FirstOrDefault().ItemName
                }).OrderBy(x => x.ItemName).ToList();

            }

            return OfferDetails;
        }
        #endregion

        [Route("GetOfferExportData")]
        [HttpGet]
        public List<OfferExportData> GetOfferExportData(DateTime start, DateTime end)//get all Issuances which are active for the delivery boy
        {
            var startfrom = start;
            var Toend = end;


            List<OfferExportData> OfferExportDatas = new List<OfferExportData>();
            using (var db = new AuthContext())
            {
                //var sqlquery = @"select orderoffer.Skcode,orderoffer.WarehouseName,orderoffer.CityName,a.OfferCode,a.OfferOn,a.OfferName,orderoffer.OrderId,orderoffer.TotalAmount OrderAmount,orderoffer.BillDiscountAmount,
                //                     a.Description,orderoffer.CreatedDate,orderoffer.Status,a.OfferAppType ,isnull(s.Name,'Company') StoreName,orderoffer.DispatchedDate,freeitem.BillDiscountFreeItem from offers a  with(nolock)   Outer apply (select o.Skcode,o.WarehouseName,w.CityName,b.OrderId,o.TotalAmount,b.BillDiscountAmount,o.CreatedDate,o.Status,ODM.CreatedDate as DispatchedDate from BillDiscounts b  with(nolock)  
                //                     inner join OrderMasters o with(nolock) on b.OrderId=o.OrderId and  a.OfferId=b.OfferId  inner join Warehouses w  with(nolock) on w.WarehouseId=o.WarehouseId
                //                     left join OrderDispatchedMasters ODM on o.OrderId=ODM.OrderId ) orderoffer  
                //                     Left join Stores s  with(nolock)  on a.StoreId=s.Id
                //                     outer apply (select STRING_AGG(bd.ItemName,',') as BillDiscountFreeItem  from BillDiscountFreeItems bd where a.OfferId=bd.offerId)freeitem
                //                     where a.CreatedDate >= CAST('{D1}' AS DATETIME) and a.CreatedDate <= CAST('{D2}' AS DATETIME)";
                //sqlquery = sqlquery.Replace("{D1}", startfrom.ToString("MM/dd/yyyy HH:mm:ss"));
                //sqlquery = sqlquery.Replace("{D2}", Toend.ToString("MM/dd/yyyy HH:mm:ss"));
                //sqlquery = sqlquery.Replace("{WID}", Warehouseid.ToString());
                //sqlquery = sqlquery.Replace("{BDT}", DiscountType.ToString());

                //"select orderoffer.Skcode,orderoffer.WarehouseName,orderoffer.CityName,a.OfferCode,a.OfferOn,a.OfferName,orderoffer.OrderId,orderoffer.TotalAmount OrderAmount,orderoffer.BillDiscountAmount,a.Description,orderoffer.CreatedDate from offers a  " +
                //                  " Outer apply (select o.Skcode,o.WarehouseName,w.CityName,b.OrderId,o.TotalAmount,o.BillDiscountAmount,o.CreatedDate from BillDiscounts b  inner join OrderMasters o on b.OrderId=o.OrderId " +
                //                  "and  a.OfferId=b.OfferId  inner join Warehouses w on w.WarehouseId=o.WarehouseId) orderoffer  where a.CreatedDate >= CAST(CONVERT(VARCHAR,'" + startfrom + "',101)AS DATETIME)  and a.CreatedDate <= CAST(CONVERT(VARCHAR,'" + Toend + "' ,101 )AS DATETIME) and a.Warehouseid=" + Warehouseid + "and BillDiscountType=" + DiscountType;
                //OfferExportDatas = db.Database.SqlQuery<OfferExportData>(sqlquery).ToList();

                var FromDate = new SqlParameter("@FromDate", start);
                var ToDate = new SqlParameter("@ToDate", end);
                OfferExportDatas = db.Database.SqlQuery<OfferExportData>("EXEC GetOfferExportData @FromDate,@ToDate", FromDate, ToDate).ToList();

            }
            return OfferExportDatas;
        }

        [Route("GetOfferExport")]
        [HttpGet]
        public List<OfferExportDataDC> GetOfferExport(int? WarehouseId, DateTime? start, DateTime? end)
        {
            var startfrom = start;
            var Toend = end;

            string whereclause = "";
            if (WarehouseId > 0)
                whereclause += " where a.WarehouseId = " + WarehouseId;

            if (start.HasValue && end.HasValue)
                whereclause += " and a.IsActive=1 and a.IsDeleted=0 and a.CreatedDate >= '" + startfrom.Value.ToString("MM/dd/yyyy HH:mm:ss") + "' and a.CreatedDate <= '" + Toend.Value.ToString("MM/dd/yyyy HH:mm:ss") + "'";

            List<OfferExportDataDC> OfferExportData = new List<OfferExportDataDC>();
            using (var db = new AuthContext())
            {
                // Added a.ApplyType by ANOOP 28/1/2021
                var sqlquery = @"select orderoffer.Skcode,w.WarehouseName,w.CityName,a.OfferCode,a.OfferOn,a.OfferName,orderoffer.OrderId,
                                a.FreeItemName,orderoffer.TotalAmount OrderAmount,orderoffer.UnitPrice as FreeItemPurchasePrice,orderoffer.qty as FreeItemQtyTaken,orderoffer.TotalAmt as FreebieValue,
                                a.Description,orderoffer.CreatedDate,orderoffer.Status,a.OfferAppType,Isnull(st.Name,'Company') StoreName,a.ApplyType,freeitem.BillDiscountFreeItem  from offers a  with(nolock) inner join Warehouses w on w.WarehouseId=a.WarehouseId Outer apply
                                (select o.Skcode,b.OrderId,o.TotalAmount,o.CreatedDate,o.Status,od.UnitPrice,od.qty,od.TotalAmt from BillDiscounts b with(nolock)  
                                inner join OrderMasters o with(nolock) on b.OrderId=o.OrderId and a.OfferId=b.OfferId 
                                inner join OrderDetails od with(nolock)  on b.OrderId=od.OrderId Left Join Stores st on od.storeid=st.Id where od.IsFreeItem=1)  
                                orderoffer  Left Join Stores st on a.storeid=st.Id 
                                outer apply (select STRING_AGG(bd.ItemName,',') as BillDiscountFreeItem  from BillDiscountFreeItems bd where a.OfferId=bd.offerId)freeitem " + whereclause;

                // sqlquery = sqlquery.Replace("{D1}", startfrom.Value.ToString("MM/dd/yyyy HH:mm:ss"));
                // sqlquery = sqlquery.Replace("{D2}", Toend.Value.ToString("MM/dd/yyyy HH:mm:ss"));

                OfferExportData = db.Database.SqlQuery<OfferExportDataDC>(sqlquery).ToList();

            }
            return OfferExportData;
        }

        [Route("AutoOfferDeactivate")]
        [HttpGet]
        [AllowAnonymous]
        public bool AutoOfferDeactivate()
        {
            using (var db = new AuthContext())
            {
                var offers = db.OfferDb.Where(x => x.IsActive && !x.IsDeleted && x.end <= indianTime).ToList();
                if (offers != null && offers.Any())
                {
                    List<int> itemids = offers.Select(y => y.itemId).Distinct().ToList();
                    if (itemids != null && itemids.Any())
                    {

                        List<ItemMaster> iteminfos = db.itemMasters.Where(x => itemids.Contains(x.ItemId)).ToList();
                        foreach (var iteminfo in iteminfos)
                        {
                            var offer = offers.FirstOrDefault(x => x.OfferId == iteminfo.OfferId);
                            if (offer != null && offer.OfferOn == "Item" && offer.OfferAppType != "Distributor App")
                            {
                                iteminfo.IsOffer = false;
                                iteminfo.OfferCategory = 0;
                                iteminfo.UpdatedDate = indianTime;
                                db.Entry(iteminfo).State = EntityState.Modified;
                            }
                        }
                    }

                    foreach (var offer in offers)
                    {
                        offer.IsActive = false;
                        offer.UpdateDate = indianTime;
                        db.Entry(offer).State = EntityState.Modified;
                    }
                    db.Commit();
                }
            }

            return true;
        }

        //[HttpPost]
        //[Route("GetDistributerOffer")]
        ////[AcceptVerbs("GET")]
        //[AllowAnonymous]
        //public List<DistributerListDc> GetDistributerOffer(offerdistributerDC offerdistributerDC)
        //{
        //    List<DistributerListDc> OfferList = new List<DistributerListDc>();
        //    using (var db = new AuthContext())
        //    {

        //        //string startdate = offerdistributerDC.start.Value.ToString("dd/MM/yyyy");
        //        //string enddate = offerdistributerDC.end.Value.ToString("dd/MM/yyyy");
        //        if (offerdistributerDC.start != null && offerdistributerDC.end != null)
        //        {
        //            offerdistributerDC.start = offerdistributerDC.start.Value.Date;
        //            offerdistributerDC.end = offerdistributerDC.end.Value.Date;
        //        }
        //        if (offerdistributerDC.offerOn == "ItemPost")
        //        {

        //            //OfferList = db.OfferDb.Where(x => x.WarehouseId == warehouseid && x.OfferAppType == "Distributor App" && x.DistributorOfferType == true && x.start >= start && x.end <= end).OrderByDescending(x => x.CreatedDate).ToList();
        //            OfferList = (from od in db.OfferDb
        //                         join d in db.DistributorOffer on
        //                         od.OfferId equals d.offerId
        //                         join wh in db.Warehouses on
        //                         od.WarehouseId equals wh.WarehouseId
        //                         where 
        //                         (!offerdistributerDC.start.HasValue || od.CreatedDate >= offerdistributerDC.start) && (!offerdistributerDC.end.HasValue || od.CreatedDate <= offerdistributerDC.end) && (od.DistributorOfferType == true) && (od.WarehouseId != 0 || od.WarehouseId == offerdistributerDC.warehouseid) && od.OfferAppType == "Distributor App" && od.OfferOn == "ItemPost" && od.IsActive == true && od.IsDeleted == false
        //                         select new DistributerListDc
        //                         {
        //                             OfferName = od.OfferName,
        //                             itemname = od.itemname,
        //                             WarehouseId = od.WarehouseId,
        //                             WarehouseName = wh.WarehouseName,
        //                             OfferOn = od.OfferOn,
        //                             FreeOfferType = od.FreeOfferType,
        //                             BillDiscountOfferOn = od.BillDiscountOfferOn,
        //                             BillAmount = od.BillAmount,
        //                             MaxBillAmount = od.MaxBillAmount,
        //                             OffersaleQty = d.OffersaleQty,
        //                             start = od.start,
        //                             end = od.end,
        //                             CreatedDate = od.CreatedDate,
        //                             DiscountPercentage = od.DiscountPercentage,
        //                             FreeWalletPoint = od.FreeWalletPoint,
        //                             BillDiscountWallet = d.OffersaleWeight,
        //                             UOM = d.UOM,
        //                             DistributorDiscountAmount = od.DistributorDiscountAmount,
        //                             DistributorDiscountPercentage = od.DistributorDiscountPercentage,
        //                             IsActive = od.IsActive,
        //                             OfferCode = od.OfferCode
        //                         }).ToList();
        //            OfferList = OfferList.OrderByDescending(x => x.CreatedDate).ToList();


        //        }
        //        else if (offerdistributerDC.offerOn == "Slab")
        //        {

        //            //OfferList = db.OfferDb.Where(x => x.WarehouseId == warehouseid && x.OfferAppType == "Distributor App" && x.DistributorOfferType == true && x.start >= start && x.end <= end).OrderByDescending(x => x.CreatedDate).ToList();
        //            OfferList = db.OfferDb.Where(x => x.OfferAppType == "Distributor App" && x.DistributorOfferType == true && x.IsActive == true && (!offerdistributerDC.start.HasValue || x.CreatedDate >= offerdistributerDC.start) && (!offerdistributerDC.end.HasValue || x.CreatedDate <= offerdistributerDC.end) && (x.WarehouseId != 0 || x.WarehouseId == offerdistributerDC.warehouseid) && x.OfferOn == "Slab")
        //                        .Select(x => new DistributerListDc
        //                        {
        //                            OfferName = x.OfferName,
        //                            itemname = x.itemname,
        //                            WarehouseId = x.WarehouseId,
        //                            //WarehouseName = x.WarehouseName,
        //                            OfferOn = x.OfferOn,
        //                            FreeOfferType = x.FreeOfferType,
        //                            BillDiscountOfferOn = x.BillDiscountOfferOn,
        //                            BillAmount = x.BillAmount,
        //                            MaxBillAmount = x.MaxBillAmount,
        //                            OffersaleQty = x.OffersaleQty,
        //                            start = x.start,
        //                            end = x.end,
        //                            CreatedDate = x.CreatedDate,
        //                            DiscountPercentage = x.DiscountPercentage,
        //                            FreeWalletPoint = x.FreeWalletPoint,
        //                            DistributorDiscountAmount = x.DistributorDiscountAmount,
        //                            DistributorDiscountPercentage = x.DistributorDiscountPercentage,
        //                            IsActive = x.IsActive,
        //                            OfferCode = x.OfferCode
        //                        }).ToList();
        //            OfferList = OfferList.OrderByDescending(x => x.CreatedDate).ToList();


        //        }


        //        return OfferList;
        //    }
        //}

        [AllowAnonymous]
        [Route("getOfferItem")]
        [HttpGet]
        public dynamic Getlist(string name, int warehouseid)
        {
            using (AuthContext db = new AuthContext())
            {
                if (warehouseid > 0)
                {
                    var whdata = db.itemMasters.Where(x => x.WarehouseId == warehouseid && x.Deleted == false && x.DistributorShow && x.active == true && x.DistributorShow == true && !(x.UOM == "PC" || x.UOM == "Size" || x.UOM == "Combo") && x.itemname.ToLower().Contains(name.ToLower())).Select(x => new { x.UOM, x.UnitPrice, x.Number, x.MinOrderQty, x.ItemId, itemnameWithMOQ = x.itemname + "  ( MOQ=" + x.MinOrderQty + ")", itemname = x.itemname, x.LogoUrl }).Take(50).ToList();
                    //var whdata = db.itemMasters.Where(x => x.WarehouseId == warehouseid && x.Deleted == false && x.active == true && x.itemname.ToLower().Contains(name.ToLower())).Select(x => new { x.UnitPrice, x.MinOrderQty, x.ItemId, itemname = x.itemname, x.LogoUrl }).Take(50).ToList();
                    return whdata;
                }
                else
                {
                    return null;
                }

            }
        }


        [Route("AddDistributorOffer")]
        [HttpPost]
        public bool adddistributor(DistributerOfferDc offer)
        {
            try
            {
                using (AuthContext context = new AuthContext())
                {

                    var identity = User.Identity as ClaimsIdentity;
                    int compid = 0, userid = 0;

                    if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))
                        compid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);

                    if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                        userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

                    if (offer == null)
                    {
                        throw new ArgumentNullException("offer");
                    }
                    bool Isexists = true;
                    bool result = false;
                    //offer.start = DateTime.Now;
                    //offer.end = DateTime.NowShowPickerListA7
                    DateTime date1 = offer.start;
                    DateTime date2 = offer.end;
                    Offer data = new Offer();
                    if (offer.OfferOn == "ItemPost")
                    {
                        data = context.OfferDb.Where(x => x.itemId == offer.itemId && x.IsDeleted == false && x.start <= date1 && x.end >= date2 && x.WarehouseId == offer.WarehouseId && x.OfferOn == "ItemPost" && x.IsActive == true).FirstOrDefault();
                    }
                    if (offer.OfferOn == "ItemMarkDown")
                    {
                        data = context.OfferDb.Where(x => x.IsDeleted == false && x.start <= date1 && x.end >= date2 && x.WarehouseId == offer.WarehouseId && x.OfferOn == "ItemMarkDown" && x.IsActive == true && x.ApplyType == offer.ApplyType && x.BillDiscountType == offer.BillDiscountType).FirstOrDefault();
                    }
                    if (offer.OfferOn == "Slab")
                    {
                        var predicate = PredicateBuilder.True<Offer>();
                        predicate = predicate.And(x => x.IsDeleted == false && x.IsActive == true && x.BillAmount == offer.BillAmount && x.MaxDiscount == offer.MaxDiscount && x.LineItem == offer.LineItem && x.WarehouseId == offer.WarehouseId && x.OfferOn == "Slab" && x.BillDiscountType == offer.BillDiscountType && x.start <= date1 && x.end >= date2);
                        if (offer.BillDiscountOfferOn == "Percentage")
                        {
                            predicate = predicate.And(x => x.BillDiscountOfferOn == "Percentage");
                        }
                        else
                        {
                            predicate = predicate.And(x => (x.BillDiscountOfferOn == "WalletPoint" || x.BillDiscountOfferOn == "Amount"));

                        }

                        data = context.OfferDb.Where(predicate).FirstOrDefault();
                    }

                    if (data == null && Isexists)
                    {
                        var itemmasterdata = context.itemMasters.Where(x => x.ItemId == offer.itemId && x.Deleted == false && x.active == true).FirstOrDefault();

                        List<Offer> offerdata = new List<Offer>();
                        Offer multioffer = new Offer();
                        multioffer.itemname = offer.itemname;

                        multioffer.WarehouseId = offer.WarehouseId;
                        multioffer.start = date1;
                        multioffer.end = date2;
                        //offer.itemId = offer.itemId;
                        multioffer.itemId = offer.itemId;
                        multioffer.IsDeleted = false;
                        multioffer.IsActive = offer.IsActive;
                        multioffer.CreatedDate = indianTime;
                        multioffer.UpdateDate = indianTime;
                        multioffer.OfferCode = offer.OfferCode;
                        multioffer.CityId = offer.CityId;
                        multioffer.ApplyType = offer.ApplyType;
                        //multioffer.DiscountPercentage = offer.DiscountPercentage;
                        multioffer.DistributorDiscountPercentage = Convert.ToDecimal(offer.DiscountPercentage);
                        multioffer.OfferName = offer.OfferName;
                        multioffer.Description = offer.Description;
                        multioffer.OfferWithOtherOffer = offer.OfferWithOtherOffer;
                        multioffer.BillDiscountOfferOn = offer.BillDiscountOfferOn;
                        multioffer.BillDiscountWallet = offer.BillDiscountWallet;
                        if (!string.IsNullOrEmpty(offer.BillDiscountType))
                            multioffer.BillDiscountType = offer.BillDiscountType.ToLower();
                        multioffer.OfferOn = offer.OfferOn;
                        multioffer.OfferAppType = "Distributor App";
                        multioffer.ApplyOn = offer.OfferOn == "ItemMarkDown" ? "PreOffer" : "PostOffer";
                        multioffer.OfferCategory = "Distributor Offer";
                        multioffer.WalletType = offer.WalletType;
                        multioffer.DistributorOfferType = true;
                        multioffer.BillAmount = offer.BillAmount;
                        multioffer.MaxBillAmount = offer.MaxBillAmount;
                        multioffer.IsUseOtherOffer = true;
                        //multioffer.MaxDiscount = offer.MaxDiscount;
                        multioffer.DistributorDiscountAmount = Convert.ToDecimal(offer.MaxDiscount);
                        multioffer.WalletType = offer.WalletType;
                        multioffer.FreeOfferType = offer.FreeOfferType;
                        // multioffer.DistributorDiscountAmount = offer.FreeWalletPoint;
                        multioffer.FreeWalletPoint = offer.FreeWalletPoint;
                        context.OfferDb.Add(multioffer);
                        result = context.Commit() > 0;
                        if (result)
                        {
                            string code = "";

                            if (offer.OfferOn == "Slab")
                            {
                                code = "SP_";
                                //data = context.OfferDb.Where(x => x.IsDeleted == false && x.WarehouseId == offer.WarehouseId && x.OfferOn == "Slab" && x.IsActive == true).FirstOrDefault();
                            }
                            else if (offer.OfferOn == "ItemPost")
                            {
                                code = "IP_";
                                // data = context.OfferDb.Where(x => x.itemId == offer.itemId && x.IsDeleted == false && x.WarehouseId == offer.WarehouseId && x.OfferOn == "ItemPost" && x.IsActive == true).FirstOrDefault();
                            }
                            else
                            {
                                code = "IMD_";
                            }

                            if (string.IsNullOrEmpty(offer.OfferCode))
                            {
                                string offerCode = code + multioffer.OfferId;
                                multioffer.OfferCode = offerCode;
                                //context.Entry(data).State = EntityState.Modified;
                                context.Commit();
                            }
                        }
                        if (offer.OfferOn == "ItemPost")
                        {
                            data = context.OfferDb.Where(x => x.itemId == offer.itemId && x.IsDeleted == false && x.WarehouseId == offer.WarehouseId && x.OfferOn == "ItemPost" && x.IsActive == true && x.OfferId == multioffer.OfferId).FirstOrDefault();

                            DistributorOffer Disoffers = new DistributorOffer();

                            Disoffers.offerId = data.OfferId;
                            Disoffers.OffersaleQty = offer.OffersaleQty;
                            Disoffers.OffersaleAmount = Convert.ToInt32(offer.MaxBillAmount);
                            Disoffers.OffersaleWeight = Convert.ToInt32(offer.BillDiscountWallet);
                            Disoffers.UOM = offer.UOM;
                            Disoffers.IsActive = true;
                            Disoffers.IsDeleted = false;
                            Disoffers.CreatedDate = DateTime.Now;
                            Disoffers.CreatedBy = userid;
                            context.DistributorOffer.Add(Disoffers);
                            context.Commit();
                        }

                        if (offer.OfferOn == "ItemMarkDown")
                        {
                            List<BillDiscountOfferSection> billDiscountOfferSections = new List<BillDiscountOfferSection>();
                            if (offer.ObjectIds != null && offer.ObjectIds.Any())
                            {
                                foreach (var item in offer.ObjectIds)
                                {
                                    billDiscountOfferSections.Add(new BillDiscountOfferSection
                                    {
                                        IsInclude = true,
                                        ObjId = item,
                                        OfferId = multioffer.OfferId
                                    });
                                }
                            }

                            if (billDiscountOfferSections.Any())
                                context.BillDiscountOfferSectionDB.AddRange(billDiscountOfferSections);

                            context.Commit();
                        }
                        return true;

                    }


                    else
                    {
                        return false;
                    }





                }
            }
            catch (Exception ex)
            {
                return false;
            }


        }


        [Route("PutDisributerOffer")]
        [HttpPut]
        [AllowAnonymous]
        public bool PutDisributerOffer(DistributerUpdateDC offer)
        {
            using (AuthContext context = new AuthContext())
            {

                var identity = User.Identity as ClaimsIdentity;
                int compid = 0, userid = 0;

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))
                    compid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                    userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
                Offer multiofferUpdate = context.OfferDb.Where(x => x.OfferCode == offer.OfferCode).FirstOrDefault();
                if (multiofferUpdate != null && multiofferUpdate.IsActive == true)
                {
                    multiofferUpdate.IsActive = false;
                    context.Entry(multiofferUpdate).State = EntityState.Modified;
                }
                else
                {
                    multiofferUpdate.IsActive = true;
                    context.Entry(multiofferUpdate).State = EntityState.Modified;
                }

                int id = context.Commit();
                if (id > 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        [Route("GetDestributorTarget")]
        [HttpGet]
        [AllowAnonymous]
        public OfferTargetDc GetDestributorTarget(int WarehouseId, int CustomerId)
        {
            using (AuthContext context = new AuthContext())
            {

                List<Offer> Offerdata = null;
                List<Offer> Offerslab = null;
                List<Offer> OfferItemPost = null;
                OfferTargetDc Target = new OfferTargetDc();
                Offerdata = context.OfferDb.Where(x => x.WarehouseId == WarehouseId && x.start <= DateTime.Now && x.end >= DateTime.Now && x.IsActive == true && x.IsDeleted == false && (x.OfferOn == "Slab" || x.OfferOn == "ItemPost")).ToList();

                if (Offerdata != null && Offerdata.Any())
                {
                    var startDate = Offerdata.Min(x => x.start);
                    var EndDate = Offerdata.Max(x => x.end);
                    List<OrderMaster> orders = context.DbOrderMaster.Where(x => startDate <= x.CreatedDate && EndDate >= x.CreatedDate && x.OrderType == 4 && x.CustomerId == CustomerId).Include("orderDetails").ToList();

                    Offerslab = Offerdata.Where(x => x.OfferOn == "Slab" && x.IsActive == true).ToList();
                    OfferItemPost = Offerdata.Where(x => x.OfferOn == "ItemPost" && x.IsActive == true).ToList();
                    if (Offerslab != null)
                    {
                        Target.SlapOffers = new List<SlapOffer>();
                        foreach (var offer in Offerslab)
                        {
                            var orderGrossAmt = orders.Where(x => offer.start <= x.CreatedDate && offer.end >= x.CreatedDate).Sum(x => x.GrossAmount);
                            //Target.SlapOffers = new List<SlapOffer>();
                            SlapOffer slapOffer = new SlapOffer();
                            slapOffer.TotalOrderValue = orderGrossAmt;
                            slapOffer.Discount = Convert.ToDouble(offer.DistributorDiscountAmount);
                            slapOffer.DiscountPercentage = Convert.ToDouble(offer.DistributorDiscountPercentage);
                            slapOffer.OfferOnPrice = offer.BillAmount;
                            slapOffer.WalletPercentage = Convert.ToDouble(offer.DistributorDiscountPercentage);
                            slapOffer.MaxDiscount = Convert.ToDouble(offer.DistributorDiscountAmount);
                            slapOffer.WalletAmount = offer.FreeWalletPoint;
                            slapOffer.BillDiscountOfferOn = offer.BillDiscountOfferOn;
                            slapOffer.StartDate = offer.start;
                            slapOffer.EndDate = offer.end;
                            Target.SlapOffers.Add(slapOffer);

                        }
                    }
                    if (OfferItemPost != null)
                    {
                        var orderDetails = orders.SelectMany(x => x.orderDetails).ToList();
                        Target.PostItemOffers = new List<PostItemOffer>();
                        foreach (var offeritem in OfferItemPost)
                        {
                            var data = context.itemMasters.Where(x => x.ItemId == offeritem.itemId).FirstOrDefault();
                            var itemids = context.itemMasters.Where(x => x.WarehouseId == data.WarehouseId && x.Number == data.Number).Select(x => x.ItemId).ToList();
                            var Orderitemdata = orderDetails.Where(x => itemids.Contains(x.ItemId)).ToList();
                            DistributorOffer distributorOffer = context.DistributorOffer.Where(x => x.offerId == offeritem.OfferId).FirstOrDefault();
                            if (distributorOffer != null)
                            {

                                PostItemOffer ItemOffer = new PostItemOffer();
                                if (Orderitemdata != null)
                                {
                                    if (offeritem.FreeOfferType == "Amount")
                                    {
                                        ItemOffer.TotalOrderValue = Orderitemdata.Sum(x => x.TotalAmt);
                                        ItemOffer.TotalOrderQty = 0;
                                        ItemOffer.TotalOrderWeight = 0;
                                    }
                                    else if (offeritem.FreeOfferType == "Weight")
                                    {
                                        decimal unitofQty = 0;
                                        decimal.TryParse(data.UnitofQuantity, out unitofQty);

                                        ItemOffer.TotalOrderValue = 0;
                                        ItemOffer.TotalOrderQty = 0;
                                        ItemOffer.TotalOrderWeight = Convert.ToInt32(Orderitemdata.Sum(x => x.qty) * unitofQty);
                                    }
                                    else if (offeritem.FreeOfferType == "Quantity")
                                    {
                                        ItemOffer.TotalOrderValue = 0;
                                        ItemOffer.TotalOrderQty = Orderitemdata.Sum(x => x.qty);
                                        ItemOffer.TotalOrderWeight = 0;
                                    }
                                }
                                ItemOffer.itemname = data.itemBaseName;
                                ItemOffer.OfferType = offeritem.FreeOfferType;
                                ItemOffer.OffersaleAmount = distributorOffer.OffersaleAmount;
                                ItemOffer.OffersaleQty = distributorOffer.OffersaleQty;
                                ItemOffer.OffersaleWeight = distributorOffer.OffersaleWeight;
                                ItemOffer.UOM = distributorOffer.UOM;
                                ItemOffer.MaxDiscount = Convert.ToDouble(offeritem.DistributorDiscountAmount);
                                ItemOffer.StartDate = offeritem.start;
                                ItemOffer.EndDate = offeritem.end;
                                Target.PostItemOffers.Add(ItemOffer);

                            }
                        }


                    }
                }
                return Target;
            }
        }

        [Route("GetCustomerEstimationOffer")]
        [HttpGet]
        [AllowAnonymous]
        public async Task<bool> GetCustomerEstimationOffer()
        {
            using (AuthContext context = new AuthContext())
            {
                List<Offer> Offerdata = null;
                List<Offer> Offerslab = null;
                List<Offer> OfferItemPost = null;
                List<CustomerEstimationOffer> custOffers = new List<CustomerEstimationOffer>();
                List<OrderMaster> orders = null;

                Offerdata = context.OfferDb.Where(x => x.IsActive == true && x.IsDeleted == false && (x.OfferOn == "Slab" || x.OfferOn == "ItemPost")).ToList();

                if (Offerdata != null && Offerdata.Any())
                {
                    var startDate = Offerdata.Min(x => x.start);
                    var EndDate = Offerdata.Max(x => x.end);
                    orders = context.DbOrderMaster.Where(x => startDate <= x.CreatedDate && EndDate >= x.CreatedDate && x.OrderType == 4 && x.Status.ToUpper() != "PENDING").Include("orderDetails").ToList();
                    Offerslab = Offerdata.Where(x => x.OfferOn == "Slab").ToList();
                    OfferItemPost = Offerdata.Where(x => x.OfferOn == "ItemPost").ToList();

                    if (Offerslab != null)
                    {
                        foreach (var offer in Offerslab)
                        {
                            var Offerorders = orders.Where(x => offer.start <= x.CreatedDate && offer.end >= x.CreatedDate && x.WarehouseId == offer.WarehouseId).ToList();
                            foreach (var customerOrder in Offerorders.GroupBy(x => x.CustomerId))
                            {
                                if (offer.BillAmount <= customerOrder.Sum(x => x.GrossAmount))
                                {
                                    custOffers.Add(new CustomerEstimationOffer
                                    {
                                        CustomerId = customerOrder.Key,
                                        OfferId = offer.OfferId,
                                        OrderIds = string.Join(",", customerOrder.Select(x => x.OrderId).Distinct().ToList()),
                                        Status = 0,
                                        CalculateDiscountvalue = 0,
                                        ChangeCalculateDiscountValue = 0,
                                        CreatedBy = 1,
                                        IsActive = true,
                                        IsDeleted = false,
                                        CreatedDate = DateTime.Now,
                                        OfferDiscount = offer.BillDiscountOfferOn == "Percentage" ? Convert.ToDecimal(offer.DiscountPercentage) : Convert.ToDecimal(offer.FreeWalletPoint),
                                    });
                                }
                            }
                        }

                    }
                    if (OfferItemPost != null)
                    {
                        var orderDetails = orders.SelectMany(x => x.orderDetails).ToList();
                        foreach (var offeritem in OfferItemPost)
                        {

                            var data = context.itemMasters.Where(x => x.ItemId == offeritem.itemId).FirstOrDefault();
                            //var data = context.itemMasters.Where(x => x.ItemId == 306395).FirstOrDefault();


                            var itemids = context.itemMasters.Where(x => x.WarehouseId == data.WarehouseId && x.Number == data.Number).Select(x => x.ItemId).ToList();
                            var Orderitemdata = orderDetails.Where(x => itemids.Contains(x.ItemId) && x.WarehouseId == offeritem.WarehouseId).ToList();
                            DistributorOffer distributorOffer = context.DistributorOffer.Where(x => x.offerId == offeritem.OfferId).FirstOrDefault();
                            if (distributorOffer != null)
                            {
                                PostItemOffer ItemOffer = new PostItemOffer();
                                if (Orderitemdata != null)
                                {
                                    foreach (var customerOrder in Orderitemdata.GroupBy(x => x.CustomerId))
                                    {
                                        if (offeritem.FreeOfferType == "Amount")
                                        {
                                            if (distributorOffer.OffersaleAmount <= customerOrder.Sum(x => x.TotalAmt))
                                            {
                                                custOffers.Add(new CustomerEstimationOffer
                                                {
                                                    CustomerId = customerOrder.Key,
                                                    OfferId = offeritem.OfferId,
                                                    OrderIds = string.Join(",", customerOrder.Select(x => x.OrderId).Distinct().ToList()),
                                                    Status = 0,
                                                    CalculateDiscountvalue = 0,
                                                    ChangeCalculateDiscountValue = 0,
                                                    CreatedBy = 1,
                                                    IsActive = true,
                                                    IsDeleted = false,
                                                    CreatedDate = DateTime.Now,
                                                    OfferDiscount = Convert.ToDecimal(offeritem.MaxDiscount),
                                                });
                                            }
                                        }
                                        else if (offeritem.FreeOfferType == "Weight")
                                        {
                                            decimal unitofQty = 0;

                                            decimal.TryParse(data.UnitofQuantity, out unitofQty);

                                            if (distributorOffer.OffersaleWeight <= Convert.ToInt32(Orderitemdata.Sum(x => x.qty) * unitofQty))
                                            {
                                                custOffers.Add(new CustomerEstimationOffer
                                                {
                                                    CustomerId = customerOrder.Key,
                                                    OfferId = offeritem.OfferId,
                                                    OrderIds = string.Join(",", customerOrder.Select(x => x.OrderId).Distinct().ToList()),
                                                    Status = 0,
                                                    CalculateDiscountvalue = 0,
                                                    ChangeCalculateDiscountValue = 0,
                                                    CreatedBy = 1,
                                                    IsActive = true,
                                                    IsDeleted = false,
                                                    CreatedDate = DateTime.Now,
                                                    OfferDiscount = Convert.ToDecimal(offeritem.MaxDiscount),
                                                });
                                            }
                                        }
                                        else if (offeritem.FreeOfferType == "Quantity")
                                        {
                                            if (distributorOffer.OffersaleQty <= Orderitemdata.Sum(x => x.qty))
                                            {
                                                custOffers.Add(new CustomerEstimationOffer
                                                {
                                                    CustomerId = customerOrder.Key,
                                                    OfferId = offeritem.OfferId,
                                                    OrderIds = string.Join(",", customerOrder.Select(x => x.OrderId).Distinct().ToList()),
                                                    Status = 0,
                                                    CalculateDiscountvalue = 0,
                                                    ChangeCalculateDiscountValue = 0,
                                                    CreatedBy = 1,
                                                    IsActive = true,
                                                    IsDeleted = false,
                                                    CreatedDate = DateTime.Now,
                                                    OfferDiscount = Convert.ToDecimal(offeritem.MaxDiscount),
                                                });
                                            }
                                        }
                                    }
                                }


                            }
                        }
                    }

                    if (custOffers != null && custOffers.Any())
                    {
                        foreach (var item in custOffers)
                        {
                            var customerEstimationOffer = context.CustomerEstimationOffer.FirstOrDefault(x => x.CustomerId == item.CustomerId && x.OfferId == item.OfferId);
                            if (customerEstimationOffer != null)
                            {
                                customerEstimationOffer.OrderIds = item.OrderIds;
                                context.Entry(customerEstimationOffer).State = EntityState.Modified;
                            }
                            else
                            {
                                context.CustomerEstimationOffer.Add(item);
                            }
                        }
                        context.Commit();
                    }

                }

            }
            return true;
        }

        [Route("GetCustomerEstimationOfferDetails")]
        [HttpGet]
        [AllowAnonymous]
        public HttpResponseMessage GetCustomerEstimationOfferDetails(int? Warehouseid, string Skcode, string OfferCode, string Startdate, string EndDate, int Skip, int Take)
        {
            try

            {
                List<CustomerEstimationOfferDetailsDc> ObjGetCustomerOfferEstDetails = GetCustomerOfferEstDetails(Warehouseid, Skcode, OfferCode, Startdate, EndDate, Skip, Take);

                return Request.CreateResponse(HttpStatusCode.OK, ObjGetCustomerOfferEstDetails);

            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.OK, ex.GetBaseException().Message.ToString());

            }
        }

        [Route("GetCheckerList")]
        [HttpGet]
        [AllowAnonymous]
        public HttpResponseMessage GetCheckerList()
        {
            try

            {


                using (AuthContext context = new AuthContext())
                {
                    var CheckerData = (from custesto in context.CustomerEstimationOffer
                                       join offer in context.OfferDb
                                       on custesto.OfferId equals offer.OfferId
                                       join cust in context.Customers
                                       on custesto.CustomerId equals cust.CustomerId
                                       where custesto.Status == 1 && custesto.IsActive == true && custesto.IsDeleted == false
                                       select new CustomerEstimationOfferDetailsDc
                                       {
                                           Id = custesto.Id,
                                           OfferId = offer.OfferId,
                                           OfferCode = offer.OfferCode,
                                           CustomerName = cust.Name,
                                           Skcode = cust.Skcode,
                                           MobileNo = cust.Mobile,
                                           OrderIds = custesto.OrderIds,
                                           OrderValue = 0,
                                           DispatchValue = 0,
                                           Discount = offer.MaxDiscount,
                                           CalculateDiscountvalue = custesto.CalculateDiscountvalue,
                                           OfferOn = offer.OfferOn,
                                           Status = custesto.Status,
                                           CustomerId = cust.CustomerId
                                       }).OrderByDescending(x => x.OfferId).ToList();
                    return Request.CreateResponse(HttpStatusCode.OK, CheckerData);
                }
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex.GetBaseException().Message.ToString());
            }
        }
        private List<CustomerEstimationOfferDetailsDc> GetCustomerOfferEstDetails(int? Warehouseid, string Skcode, string OfferCode, string Startdate, string EndDate, int Skip, int Take)

        {
            try
            {

                using (AuthContext context = new AuthContext())
                {
                    var WarehouseidParam = new SqlParameter
                    {
                        ParameterName = "Warhouseid",
                        Value = Warehouseid == null ? DBNull.Value : (object)Warehouseid
                    };



                    var SkcodeParam = new SqlParameter
                    {
                        ParameterName = "Skcode",
                        Value = Skcode == null ? DBNull.Value : (object)Skcode
                    };


                    var OffercodeParam = new SqlParameter
                    {
                        ParameterName = "Offercode",
                        Value = OfferCode == null ? DBNull.Value : (object)OfferCode
                    };
                    var StartdateParam = new SqlParameter
                    {
                        ParameterName = "Startdate",
                        Value = Startdate == null ? DBNull.Value : (object)Startdate
                    };

                    var EndDateParam = new SqlParameter
                    {
                        ParameterName = "EndDate",
                        Value = EndDate == null ? DBNull.Value : (object)EndDate
                    };
                    var SkipParam = new SqlParameter
                    {
                        ParameterName = "Skip",
                        Value = Skip
                    };
                    var TakeParam = new SqlParameter
                    {
                        ParameterName = "Take",
                        Value = Take
                    };
                    List<CustomerEstimationOfferDetailsDc> objCustomerEstimationOfferDetailsDc = context.Database.SqlQuery<CustomerEstimationOfferDetailsDc>("GetCustomerEstimationOfferDetails @Warhouseid ,@Skcode,@Offercode,@Startdate,@EndDate,@Skip,@Take ", WarehouseidParam,
                        SkcodeParam, OffercodeParam, StartdateParam, EndDateParam, SkipParam, TakeParam).OrderByDescending(x => x.OfferId).ToList();
                    return objCustomerEstimationOfferDetailsDc;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private DispatchDc OrderDispatchValue(string OrderId, string OfferOn, int OfferId)
        {
            double DispatchValue = 0;
            double Totalamount = 0;
            double TotalDispatchedAmount = 0;
            using (AuthContext context = new AuthContext())
            {
                Offer Objoffer = context.OfferDb.Where(x => x.OfferId == OfferId).FirstOrDefault();

                List<int> OrderIds = OrderId.Split(',').Select(x => Convert.ToInt32(x)).ToList();
                string BillDisocuntOffer = Objoffer.BillDiscountOfferOn;
                if (OfferOn.ToUpper().Equals("SLAB"))
                {
                    var data = context.OrderDispatchedMasters.Where(x => OrderIds.Contains(x.OrderId)).ToList();
                    TotalDispatchedAmount = data.Sum(x => x.GrossAmount);
                    if (data != null)
                    {
                        Totalamount = data.Sum(x => x.GrossAmount);

                        if (BillDisocuntOffer.ToUpper().Equals("PERCENTAGE"))
                        {
                            decimal DisocuntPercentage = Objoffer.DistributorDiscountPercentage;
                            if (Totalamount < Objoffer.MaxBillAmount)
                            {
                                DispatchValue = Totalamount * Convert.ToDouble(DisocuntPercentage / 100);
                            }
                            else
                            {
                                DispatchValue = Totalamount * Convert.ToDouble(DisocuntPercentage / 100);
                                //DispatchValue = Objoffer.MaxBillAmount * Convert.ToDouble(DisocuntPercentage / 100);

                            }
                        }
                        else if (BillDisocuntOffer.ToUpper().Equals("AMOUNT") || BillDisocuntOffer.ToUpper().Equals("WALLETPOINT"))
                        {
                            if (Totalamount < Objoffer.BillAmount)
                                DispatchValue = (Totalamount * (Convert.ToDouble(Objoffer.FreeWalletPoint) / Objoffer.BillAmount));
                            //DispatchValue = (Totalamount * (Convert.ToDouble(Objoffer.FreeWalletPoint) / Objoffer.BillAmount));

                            else
                                //DispatchValue = Convert.ToDouble(Objoffer.DistributorDiscountAmount);
                                DispatchValue = Convert.ToDouble(Objoffer.FreeWalletPoint);
                        }
                    }
                }
                else if (OfferOn.ToUpper().Equals("ITEMPOST"))
                {
                    double DispatchedAmount = 0;
                    var data = context.itemMasters.Where(x => x.ItemId == Objoffer.itemId).FirstOrDefault();
                    var itemids = context.itemMasters.Where(x => x.WarehouseId == data.WarehouseId && x.Number == data.Number).Select(x => x.ItemId).ToList();
                    List<OrderDispatchedDetails> ObjOrderDispatchedDetails = context.OrderDispatchedDetailss.Where(x => OrderIds.Contains(x.OrderId) && itemids.Contains(x.ItemId)).ToList();
                    DistributorOffer distributorOffer = context.DistributorOffer.Where(x => x.offerId == Objoffer.OfferId).FirstOrDefault();
                    DispatchedAmount = ObjOrderDispatchedDetails.Sum(x => x.TotalAmt);
                    if (Objoffer.FreeOfferType.ToUpper().Equals("AMOUNT"))
                    {

                        if (DispatchedAmount < distributorOffer.OffersaleAmount)
                            DispatchValue = (Totalamount * (Convert.ToDouble(Objoffer.MaxDiscount) / Objoffer.BillAmount));
                        else
                            DispatchValue = Convert.ToDouble(Objoffer.DistributorDiscountAmount);
                        //DispatchValue = Convert.ToDouble(Objoffer.MaxDiscount);

                    }
                    else if (Objoffer.FreeOfferType.ToUpper().Equals("WEIGHT"))
                    {
                        decimal unitofQty = 0;
                        decimal.TryParse(data.UnitofQuantity, out unitofQty);

                        if (Convert.ToInt32(ObjOrderDispatchedDetails.Sum(x => x.qty) * unitofQty) < distributorOffer.OffersaleWeight.Value)
                            DispatchValue = (Convert.ToInt32(ObjOrderDispatchedDetails.Sum(x => x.qty) * unitofQty)) * (Convert.ToDouble(Objoffer.MaxDiscount) / distributorOffer.OffersaleWeight.Value);
                        else
                            DispatchValue = Convert.ToDouble(Objoffer.DistributorDiscountAmount);
                        //DispatchValue = Convert.ToDouble(Objoffer.MaxDiscount);

                    }
                    else if (Objoffer.FreeOfferType.ToUpper().Equals("QUANTITY"))
                    {
                        if (Convert.ToInt32(ObjOrderDispatchedDetails.Sum(x => x.qty)) < distributorOffer.OffersaleQty.Value)
                            DispatchValue = (Convert.ToInt32(ObjOrderDispatchedDetails.Sum(x => x.qty))) * (Convert.ToDouble(Objoffer.MaxDiscount) / distributorOffer.OffersaleQty.Value);
                        else
                            DispatchValue = Convert.ToDouble(Objoffer.DistributorDiscountAmount);
                        //DispatchValue = Convert.ToDouble(Objoffer.MaxDiscount);
                    }
                    TotalDispatchedAmount = DispatchedAmount;
                }

            }
            DispatchDc ObjDispatchDc = new DispatchDc
            {
                DispatchAmount = TotalDispatchedAmount,
                DipstachDicount = DispatchValue
            };
            return ObjDispatchDc;

        }
        private List<CustomerEstimationOfferDetailsDc> GetCustomerEstimationOfferDetailsDc()
        {
            using (AuthContext context = new AuthContext())
            {
                List<CustomerEstimationOfferDetailsDc> ObjCustomerEstimationOfferDetailsDc = (from custesto in context.CustomerEstimationOffer
                                                                                              join offer in context.OfferDb
                                                                                              on custesto.OfferId equals offer.OfferId
                                                                                              join cust in context.Customers
                                                                                              on custesto.CustomerId equals cust.CustomerId
                                                                                              where custesto.IsActive == true && custesto.IsDeleted == false
                                                                                              select new CustomerEstimationOfferDetailsDc
                                                                                              {
                                                                                                  OfferCode = offer.OfferCode,
                                                                                                  CustomerName = cust.Name,
                                                                                                  Skcode = cust.Skcode,
                                                                                                  MobileNo = cust.Mobile,
                                                                                                  OrderIds = custesto.OrderIds,
                                                                                                  OrderValue = 0,
                                                                                                  DispatchValue = 0,
                                                                                                  Discount = offer.MaxDiscount,
                                                                                                  CalculateDiscountvalue = custesto.CalculateDiscountvalue,
                                                                                                  OfferOn = offer.OfferOn

                                                                                              }).ToList();
                return ObjCustomerEstimationOfferDetailsDc;
            }
        }

        [Route("Redeem")]
        [HttpPost]
        public HttpResponseMessage Redeem(CustomerEstimationOffer ObjCustomerEstimationOffer)
        {
            try

            {
                bool Result = false;
                using (AuthContext context = new AuthContext())
                {
                    var data = context.CustomerEstimationOffer.Where(x => x.OfferId == ObjCustomerEstimationOffer.OfferId && x.CustomerId == ObjCustomerEstimationOffer.CustomerId && (x.Status == 0 || x.Status == 3) && x.Id == ObjCustomerEstimationOffer.Id).FirstOrDefault();


                    if (data != null)
                    {
                        DispatchDc ObjDispatchDc = new DispatchDc();
                        ObjDispatchDc = OrderDispatchValue(data.OrderIds, ObjCustomerEstimationOffer.OfferOn, data.OfferId);
                        data.ModifiedDate = DateTime.Now;
                        data.ModifiedBy = GetUserId();
                        data.Status = 1;
                        data.CalculateDiscountvalue = Convert.ToDecimal(ObjDispatchDc.DipstachDicount);
                        data.ChangeCalculateDiscountValue = ObjCustomerEstimationOffer.ChangeCalculateDiscountValue;

                        context.Entry(data).State = EntityState.Modified;

                        context.SaveChanges();
                        Result = true;
                    }
                    return Request.CreateResponse(HttpStatusCode.OK, Result);
                }


            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex.GetBaseException().Message.ToString());
            }
        }

        [Route("Gullak")]
        [HttpPost]
        public HttpResponseMessage Gullak(UpdateGullakDc ObjGullakDc)
        {
            try
            {
                bool Result = false;
                TransactionOptions option = new TransactionOptions();
                option.IsolationLevel = IsolationLevel.RepeatableRead;
                option.Timeout = TimeSpan.FromSeconds(90);
                using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, option))
                //using (TransactionScope Scope = new TransactionScope())
                {
                    using (AuthContext context = new AuthContext())
                    {
                        //foreach (var data in ObjGullakDc)
                        //{
                        Result = UpdateStatusChecker(ObjGullakDc, context);
                        if (Result)
                        {
                            context.SaveChanges();
                            Result = true;
                            scope.Complete();
                        }

                        else
                        {
                            throw new Exception("Something went wrong please try again later!!");
                        }
                    }
                }

                return Request.CreateResponse(HttpStatusCode.OK, Result);
            }

            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex.GetBaseException().Message.ToString());
            }
        }
        //public HttpResponseMessage Gullak(List<UpdateGullakDc> ObjGullakDc)
        //{
        //    try

        //    {
        //        bool Result = false;
        //        using (TransactionScope Scope = new TransactionScope())
        //        {
        //            using (AuthContext context = new AuthContext())
        //            {

        //                foreach (var data in ObjGullakDc)
        //                {

        //                    Result = UpdateStatusChecker(data, context);

        //                    if (!Result)
        //                    {
        //                        throw new Exception("Something went wrong please try again later!!");

        //                    }

        //                }

        //                context.SaveChanges();
        //                Result = true;
        //                Scope.Complete();

        //            }

        //            return Request.CreateResponse(HttpStatusCode.OK, Result);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex.GetBaseException().Message.ToString());
        //    }
        //}

        private bool UpdateStatusChecker(UpdateGullakDc ObjGullakDc, AuthContext context)
        {
            bool Result = false;
            try
            {
                int status = 0;
                var data = context.CustomerEstimationOffer.Where(x => x.OfferId == ObjGullakDc.OfferId && x.Id == ObjGullakDc.Id && (x.Status == 1)).FirstOrDefault();
                if (data != null)
                {

                    if (ObjGullakDc.Type.ToUpper().Equals("REJECT"))
                    {
                        //ObjCustomerEstimationOffer.Status = 0;
                        status = 3;
                    }
                    else if (ObjGullakDc.Type.ToUpper().Equals("ADD"))
                    {
                        //ObjCustomerEstimationOffer.Status = 2;
                        status = 2;
                        #region Add Gullak Amount

                        var CustomerGullak = context.GullakDB.Where(x => x.CustomerId == data.CustomerId && x.IsActive == true).FirstOrDefault();
                        var offer = context.OfferDb.FirstOrDefault(x => x.OfferId == data.OfferId);
                        GullakTransaction gullakTransaction = new GullakTransaction();
                        gullakTransaction.CreatedBy = GetUserId();
                        gullakTransaction.CreatedDate = DateTime.Now;
                        gullakTransaction.Amount = Convert.ToDouble(data.ChangeCalculateDiscountValue);
                        gullakTransaction.CustomerId = data.CustomerId;
                        gullakTransaction.GullakId = CustomerGullak.Id;
                        gullakTransaction.IsActive = true;
                        gullakTransaction.IsDeleted = false;
                        gullakTransaction.Comment = "Offer Redeem ";
                        gullakTransaction.ObjectType = "Offer";
                        gullakTransaction.ObjectId = offer.OfferCode;
                        if (CustomerGullak != null)
                        {
                            if (!context.GullakTransactionDB.Any(x => x.ObjectId == offer.OfferCode && x.ObjectType == "Offer"))
                            {
                                context.GullakTransactionDB.Add(gullakTransaction);
                                //add to main amount 
                                CustomerGullak.TotalAmount = CustomerGullak.TotalAmount + Convert.ToDouble(data.ChangeCalculateDiscountValue);
                                CustomerGullak.ModifiedDate = DateTime.Now;
                                CustomerGullak.ModifiedBy = GetUserId();
                                context.Entry(CustomerGullak).State = EntityState.Modified;
                            }
                        }
                        else
                        {
                            CustomerGullak = new Gullak
                            {
                                CustomerId = data.CustomerId,
                                CreatedBy = GetUserId(),
                                CreatedDate = DateTime.Now,
                                IsActive = true,
                                IsDeleted = false,
                                TotalAmount = Convert.ToDouble(data.ChangeCalculateDiscountValue),
                                GullakTransactions = new List<GullakTransaction> {
                               gullakTransaction
                            }
                            };
                            context.GullakDB.Add(CustomerGullak);
                        }

                        #endregion
                    }

                    data.CheakerId = GetUserId();
                    data.Status = status;
                    //data.Comment = ObjGullakDc.Comment;
                    data.CheakerDate = indianTime; //DateTime.Now;
                                                   //context.CustomerEstimationOffer.Attach(ObjCustomerEstimationOffer);
                    context.Entry(data).State = EntityState.Modified;
                    Result = true;
                }


                return Result;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [Route("GetDispatchValue")]
        [HttpGet]
        public HttpResponseMessage GetDispatchValue(string OfferOn, int OfferId, string OrderId)
        {
            try
            {
                DispatchDc objDispatchDc = new DispatchDc();
                objDispatchDc = OrderDispatchValue(OrderId, OfferOn, OfferId);

                return Request.CreateResponse(HttpStatusCode.OK, objDispatchDc);
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.OK, ex.GetBaseException().Message.ToString());
            }
        }

        [HttpGet]
        [Route("GetDistributorOfferHistory")]
        public HttpResponseMessage GetDistributorOfferHistory(int CustomerId, int Skip, int Take)
        {
            try
            {
                using (AuthContext context = new AuthContext())
                {
                    var CustomerIdParam = new SqlParameter
                    {
                        ParameterName = "CustomerId",
                        Value = CustomerId
                    };
                    var SkipParam = new SqlParameter
                    {
                        ParameterName = "Skip",
                        Value = Skip
                    };
                    var TakeParam = new SqlParameter
                    {
                        ParameterName = "Take",
                        Value = Take
                    };

                    List<DistributorOfferHistoryDc> objGetDistributorOfferHistoryDc = context.Database.SqlQuery<DistributorOfferHistoryDc>("GetDistributorOfferHistory @CustomerId,@Skip,@Take ", CustomerIdParam, SkipParam, TakeParam).ToList();
                    return Request.CreateResponse(HttpStatusCode.OK, objGetDistributorOfferHistoryDc);
                }
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex.GetBaseException().Message.ToString());

            }
        }


        [HttpGet]
        [Route("GetDistributerOverviw")]
        //[AcceptVerbs("GET")]
        [AllowAnonymous]
        public List<DistributerOverViewDc> GetDistributerOverviw(int offerId, string BillDiscountType)
        {
            List<DistributerOverViewDc> OfferOverList = new List<DistributerOverViewDc>();



            OfferOverList = GetDistrbutorOverViewList(offerId, BillDiscountType, "GetOfferItemDetail " + " " + "@offerId" + ", " + "@BillDiscountType");




            return OfferOverList;

        }
        private List<DistributerOverViewDc> GetDistrbutorOverViewList(int offerId, string BillDiscountType, string ProcedureName)
        {
            try
            {

                using (AuthContext context = new AuthContext())
                {
                    var offerIdParam = new SqlParameter
                    {
                        ParameterName = "OfferId",
                        Value = offerId
                    };


                    var BillDiscountTypeParam = new SqlParameter
                    {
                        ParameterName = "BillDiscountType",
                        Value = BillDiscountType
                    };



                    List<DistributerOverViewDc> objDistributerOverViewDc = context.Database.SqlQuery<DistributerOverViewDc>(ProcedureName, offerIdParam, BillDiscountTypeParam
                        ).ToList();
                    return objDistributerOverViewDc;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }








        [HttpGet]
        [Route("GetDistributerOffer")]
        //[AcceptVerbs("GET")]
        [AllowAnonymous]
        public List<DistributerListDc> GetDistributerOffer(int Warehouseid, string StartDate, string EndDate, string offerOn)
        {
            List<DistributerListDc> OfferList = new List<DistributerListDc>();
            using (var db = new AuthContext())
            {
                if (offerOn.ToUpper().Equals("ITEMPOST"))
                {

                    OfferList = GetDistrbutorOfferList(Warehouseid, StartDate, EndDate, offerOn, "GetItemPostDistributorOffer " + " " + "@WareHouseId" + ", " + "@StartDate" + ", " + "@EndDate");
                }
                else if (offerOn.ToUpper().Equals("SLAB"))
                {
                    OfferList = GetDistrbutorOfferList(Warehouseid, StartDate, EndDate, offerOn, "GetSlabDistributorOffer " + " " + "@WareHouseId" + ", " + "@StartDate" + ", " + "@EndDate");

                }
                else if (offerOn.ToUpper().Equals("ITEMMARKDOWN"))
                {
                    OfferList = GetDistrbutorOfferList(Warehouseid, StartDate, EndDate, offerOn, "GetItemMarkDownDistributorOffer " + " " + "@WareHouseId" + ", " + "@StartDate" + ", " + "@EndDate");

                }


                return OfferList;
            }
        }

        private List<DistributerListDc> GetDistrbutorOfferList(int Warehouseid, string StartDate, string EndDate, string offerOn, string ProcedureName)
        {
            try
            {

                using (AuthContext context = new AuthContext())
                {
                    var WarehouseidParam = new SqlParameter
                    {
                        ParameterName = "WareHouseId",
                        Value = Warehouseid
                    };
                    var StartdateParam = new SqlParameter
                    {
                        ParameterName = "StartDate",
                        Value = StartDate == null ? DBNull.Value : (object)StartDate
                    };

                    var EndDateParam = new SqlParameter
                    {
                        ParameterName = "EndDate",
                        Value = EndDate == null ? DBNull.Value : (object)EndDate
                    };

                    List<DistributerListDc> objDistributerListDc = context.Database.SqlQuery<DistributerListDc>(ProcedureName, WarehouseidParam,
                        StartdateParam, EndDateParam).OrderByDescending(x => x.OfferCode).ToList();
                    return objDistributerListDc;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        private int GetUserId()
        {
            var identity = User.Identity as ClaimsIdentity;
            int userid = 0;
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
            return userid;
        }

        private int GetCompanyId()
        {
            var identity = User.Identity as ClaimsIdentity;
            int CompId = 0;
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))
                CompId = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);
            return CompId;
        }

        [Authorize]
        [HttpGet]
        [Route("GetMakerCheckerList")]
        public HttpResponseMessage GetMakerCheckerList()
        {
            try
            {
                using (AuthContext context = new AuthContext())
                {


                    List<OfferCheckerDc> objCheckerDc = context.Database.SqlQuery<OfferCheckerDc>("GetOfferCheckerDetails").ToList();
                    return Request.CreateResponse(HttpStatusCode.OK, objCheckerDc);

                }
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex.GetBaseException().Message.ToString());
            }
        }

        [Authorize]
        [HttpGet]
        [Route("GetChekerHistoryByChecker")]
        public HttpResponseMessage GetChekerHistory(int Skip, int Take)
        {
            try
            {
                int Userid = GetUserId();
                List<OfferCheckerDc> objCheckerDc = GetCheckerHistory(Userid, Skip, Take).OrderByDescending(x => x.Id).ToList();
                return Request.CreateResponse(HttpStatusCode.OK, objCheckerDc);
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex.GetBaseException().Message.ToString());
            }

        }

        private List<OfferCheckerDc> GetCheckerHistory(int Userid, int Skip, int Take)
        {
            using (AuthContext context = new AuthContext())
            {
                var CheckerIdParam = new SqlParameter
                {
                    ParameterName = "CheckerId",
                    Value = Userid
                };
                var SkipParam = new SqlParameter
                {
                    ParameterName = "Skip",
                    Value = Skip
                };
                var TakeParam = new SqlParameter
                {
                    ParameterName = "Take",
                    Value = Take
                };
                List<OfferCheckerDc> objCheckerDc = context.Database.SqlQuery<OfferCheckerDc>("GetCheckerDataHistory @CheckerId,@Skip,@Take", CheckerIdParam, SkipParam, TakeParam).OrderByDescending(x => x.Id).ToList();
                return objCheckerDc;

            }
        }

        [Authorize]
        [HttpGet]
        [Route("GetChekerHistoryAll")]
        public HttpResponseMessage GetChekerHistoryAll(int Skip, int Take)
        {
            try
            {
                List<OfferCheckerDc> objCheckerDc = GetCheckerHistory(0, Skip, Take).ToList();
                return Request.CreateResponse(HttpStatusCode.OK, objCheckerDc);
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex.GetBaseException().Message.ToString());
            }

        }

        [Route("GetItemClassificationsAsync")]
        [HttpPost]
        public async Task<List<ItemClassificationForOffer>> GetItemClassificationsAsync(List<ItemClassificationForOffer> itemsList)
        {
            using (AuthContext authcontext = new AuthContext())
            {
                List<ItemClassificationDC> ItemClassificationList = new List<ItemClassificationDC>();

                foreach (var item in itemsList)
                {
                    var itemforitemnumber = authcontext.itemMasters.FirstOrDefault(x => x.ItemId == item.ItemId);
                    if (itemforitemnumber != null && itemforitemnumber.ItemNumber != null)
                    {
                        item.ItemNumber = authcontext.itemMasters.FirstOrDefault(x => x.ItemId == item.ItemId).ItemNumber;
                    }
                    else
                    {
                        itemsList = itemsList.Where(x => x.ItemId != item.ItemId).ToList();
                    }
                }

                ItemClassificationList = Mapper.Map(itemsList).ToANew<List<ItemClassificationDC>>();
                var manager = new ItemLedgerManager();

                var ItemsList = await manager.GetItemClassificationsAsync(ItemClassificationList);
                List<ItemClassificationForOffer> ItemsListForOffer = new List<ItemClassificationForOffer>();
                foreach (var classificationitm in ItemsList)
                {
                    var itemforitemnumber = itemsList.FirstOrDefault(x => x.ItemNumber == classificationitm.ItemNumber);
                    ItemClassificationForOffer offerItem = new ItemClassificationForOffer();
                    if (itemforitemnumber != null && itemforitemnumber.ItemNumber != null)
                    {
                        offerItem.ItemId = itemforitemnumber.ItemId;
                        offerItem.WarehouseId = classificationitm.WarehouseId;
                        offerItem.Category = classificationitm.Category;
                        offerItem.ItemNumber = classificationitm.ItemNumber;
                        ItemsListForOffer.Add(offerItem);
                    }
                    //else
                    //{
                    //    itemsList = itemsList.Where(x => x.ItemNumber != item.ItemNumber).ToList();
                    //}
                }
                return ItemsListForOffer;
            }
        }
        /// <summary>
        /// Freebies Upload Function
        /// </summary>
        /// <returns></returns>
        string strJSON = null;
        string col0, col1, col2, col3, col4, col5, col6, col7, col8, col9, col10, col11, col12, col13, col14, col15, col16, col17, col18;
        [HttpPost]
        [AllowAnonymous]
        [Route("freebiesuploder")]
        public IHttpActionResult UploadFile()
        {
            if (HttpContext.Current.Request.Files.AllKeys.Any())
            {
                var formData1 = HttpContext.Current.Request.Form["compid"];
                logger.Info("start Transfer Order Upload Exel File: ");

                // Access claims
                var identity = User.Identity as ClaimsIdentity;
                int userid = 0;
                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                    userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

                // Get the uploaded image from the Files collection
                System.Web.HttpPostedFile httpPostedFile = HttpContext.Current.Request.Files["file"];

                if (httpPostedFile != null)
                {
                    var FileUrl = Path.Combine(HttpContext.Current.Server.MapPath("~/UploadedFiles"), httpPostedFile.FileName);
                    // Validate the uploaded image(optional)
                    byte[] buffer = new byte[httpPostedFile.ContentLength];

                    using (BinaryReader br = new BinaryReader(httpPostedFile.InputStream))
                    {
                        br.Read(buffer, 0, buffer.Length);
                    }
                    XSSFWorkbook hssfwb;
                    //   XSSFWorkbook workbook1;
                    using (MemoryStream memStream = new MemoryStream())
                    {
                        BinaryFormatter binForm = new BinaryFormatter();
                        memStream.Write(buffer, 0, buffer.Length);
                        memStream.Seek(0, SeekOrigin.Begin);
                        hssfwb = new XSSFWorkbook(memStream);
                    }
                    httpPostedFile.SaveAs(FileUrl);
                    return ReadFreebiesUploadFile(hssfwb, userid);
                }
            }

            return Created("Error", "Error");
        }

        public IHttpActionResult ReadFreebiesUploadFile(XSSFWorkbook hssfwb, int userid)
        {
            string sSheetName = hssfwb.GetSheetName(0);
            ISheet sheet = hssfwb.GetSheet(sSheetName);

            IRow rowData;
            ICell cellData = null;
            List<FreebiesDTO> flashdeallist = new List<FreebiesDTO>();
            int? warehouseID = 0;
            int? OfferName = null;
            int? ItemNumber = null;
            int? ItemName = null;
            int? FreeItemnumber = null;
            int? FreeItemName = null;
            int? NoOfFreeitemQty = null;
            int? MinimumOrderQty = null;
            int? FreeitemMRP = null;
            int? Discription = null;
            int? StartDate = null;
            int? EndDate = null;
            int? QtyAvaiable = 0;
            int? FreeItemLimit = 0;
            int? AppType = null;
            int? IsActive = 0;
            int? Stock = 0;
            int? MRP = 0;
            int? SellingSKU = 0;
            //double? MRP = 0;

            for (int iRowIdx = 0; iRowIdx <= sheet.LastRowNum; iRowIdx++)  //  iRowIdx = 0; HeaderRow
            {
                if (iRowIdx == 0)
                {
                    rowData = sheet.GetRow(iRowIdx);

                    if (rowData != null)
                    {
                        warehouseID = rowData.Cells.Any(x => x.ToString().Trim() == "warehouse ID") ? rowData.Cells.FirstOrDefault(x => x.ToString().Trim() == "warehouse ID").ColumnIndex : (int?)null;
                        if (!warehouseID.HasValue)
                        {
                            JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("SellingSku does not exist..try again");
                            return Created(strJSON, strJSON);
                        }
                        OfferName = rowData.Cells.Any(x => x.ToString().Trim() == "Offer Name") ? rowData.Cells.FirstOrDefault(x => x.ToString().Trim() == "Offer Name").ColumnIndex : (int?)null;
                        if (!OfferName.HasValue)
                        {
                            JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("FlashDealQtyAvaiable does not exist..try again");
                            return Created(strJSON, strJSON); ;
                        }
                        ItemNumber = rowData.Cells.Any(x => x.ToString().Trim() == "Item Number") ? rowData.Cells.FirstOrDefault(x => x.ToString().Trim() == "Item Number").ColumnIndex : (int?)null;
                        if (!ItemNumber.HasValue)
                        {
                            JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("FlashDealQtyAvaiable does not exist..try again");
                            return Created(strJSON, strJSON); ;
                        }
                        SellingSKU = rowData.Cells.Any(x => x.ToString().Trim() == "Selling SKU") ? rowData.Cells.FirstOrDefault(x => x.ToString().Trim() == "Selling SKU").ColumnIndex : (int?)null;
                        if (!SellingSKU.HasValue)
                        {
                            JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("FlashDealQtyAvaiable does not exist..try again");
                            return Created(strJSON, strJSON); ;
                        }
                        ItemName = rowData.Cells.Any(x => x.ToString().Trim() == "Item Name") ? rowData.Cells.FirstOrDefault(x => x.ToString().Trim() == "Item Name").ColumnIndex : (int?)null;
                        if (!ItemName.HasValue)
                        {
                            JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("FlashDealQtyAvaiable does not exist..try again");
                            return Created(strJSON, strJSON); ;
                        }
                        MinimumOrderQty = rowData.Cells.Any(x => x.ToString().Trim() == "Minimum Order Qty") ? rowData.Cells.FirstOrDefault(x => x.ToString().Trim() == "Minimum Order Qty").ColumnIndex : (int?)null;
                        if (!MinimumOrderQty.HasValue)
                        {
                            JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("FlashDealQtyAvaiable does not exist..try again");
                            return Created(strJSON, strJSON); ;
                        }
                        FreeItemnumber = rowData.Cells.Any(x => x.ToString().Trim() == "FreeItemnumber") ? rowData.Cells.FirstOrDefault(x => x.ToString().Trim() == "FreeItemnumber").ColumnIndex : (int?)null;
                        if (!FreeItemnumber.HasValue)
                        {
                            JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("FlashDealQtyAvaiable does not exist..try again");
                            return Created(strJSON, strJSON); ;
                        }
                        FreeItemName = rowData.Cells.Any(x => x.ToString().Trim() == "Free Item Name") ? rowData.Cells.FirstOrDefault(x => x.ToString().Trim() == "Free Item Name").ColumnIndex : (int?)null;
                        if (!FreeItemName.HasValue)
                        {
                            JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("FlashDealQtyAvaiable does not exist..try again");
                            return Created(strJSON, strJSON); ;
                        }
                        NoOfFreeitemQty = rowData.Cells.Any(x => x.ToString().Trim() == "No. Of Free item Qty") ? rowData.Cells.FirstOrDefault(x => x.ToString().Trim() == "No. Of Free item Qty").ColumnIndex : (int?)null;
                        if (!NoOfFreeitemQty.HasValue)
                        {
                            JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("No. Of Free item Qty does not exist..try again");
                            return Created(strJSON, strJSON); ;
                        }
                        FreeitemMRP = rowData.Cells.Any(x => x.ToString().Trim() == "Freeitem MRP") ? rowData.Cells.FirstOrDefault(x => x.ToString().Trim() == "Freeitem MRP").ColumnIndex : (int?)null;
                        if (!FreeitemMRP.HasValue)
                        {
                            JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("Freeitem MRP does not exist..try again");
                            return Created(strJSON, strJSON); ;
                        }
                        Discription = rowData.Cells.Any(x => x.ToString().Trim() == "Discription") ? rowData.Cells.FirstOrDefault(x => x.ToString().Trim() == "Discription").ColumnIndex : (int?)null;
                        if (!Discription.HasValue)
                        {
                            JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("Discription does not exist..try again");
                            return Created(strJSON, strJSON); ;
                        }
                        //FreeItemName = rowData.Cells.Any(x => x.ToString().Trim() == "Free Item Name") ? rowData.Cells.FirstOrDefault(x => x.ToString().Trim() == "Free Item Name").ColumnIndex : (int?)null;
                        //if (!FreeItemName.HasValue)
                        //{
                        //    JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("FlashDealQtyAvaiable does not exist..try again");
                        //    return Created(strJSON, strJSON); ;
                        //}

                        StartDate = rowData.Cells.Any(x => x.ToString().Trim() == "Start Date") ? rowData.Cells.FirstOrDefault(x => x.ToString().Trim() == "Start Date").ColumnIndex : (int?)null;
                        if (!StartDate.HasValue)
                        {
                            JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("Start Date does not exist..try again");
                            return Created(strJSON, strJSON); ;
                        }

                        EndDate = rowData.Cells.Any(x => x.ToString().Trim() == "End Date") ? rowData.Cells.FirstOrDefault(x => x.ToString().Trim() == "End Date").ColumnIndex : (int?)null;
                        if (!EndDate.HasValue)
                        {
                            JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("End Date does not exist..try again");
                            return Created(strJSON, strJSON); ;
                        }
                        //MRP = rowData.Cells.Any(x => x.ToString().Trim() == "MRP") ? rowData.Cells.FirstOrDefault(x => x.ToString().Trim() == "MRP").ColumnIndex : (double?)null;
                        //if (!MRP.HasValue)
                        //{
                        //    JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("MRP does not exist..try again");
                        //    return Created(strJSON, strJSON);
                        //}

                        QtyAvaiable = rowData.Cells.Any(x => x.ToString().Trim() == "Qty Available") ? rowData.Cells.FirstOrDefault(x => x.ToString().Trim() == "Qty Available").ColumnIndex : (int?)null;
                        if (!QtyAvaiable.HasValue)
                        {
                            JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("Qty Available does not exist..try again");
                            return Created(strJSON, strJSON); ;
                        }
                        FreeItemLimit = rowData.Cells.Any(x => x.ToString().Trim() == "Free Item Limit") ? rowData.Cells.FirstOrDefault(x => x.ToString().Trim() == "Free Item Limit").ColumnIndex : (int?)null;
                        if (!FreeItemLimit.HasValue)
                        {
                            JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("Free Item Limit does not exist..try again");
                            return Created(strJSON, strJSON); ;
                        }
                        AppType = rowData.Cells.Any(x => x.ToString().Trim() == "App Type") ? rowData.Cells.FirstOrDefault(x => x.ToString().Trim() == "App Type").ColumnIndex : (int?)null;
                        if (!AppType.HasValue)
                        {
                            JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("App Type does not exist..try again");
                            return Created(strJSON, strJSON);
                        }
                        IsActive = rowData.Cells.Any(x => x.ToString().Trim() == "Is Active") ? rowData.Cells.FirstOrDefault(x => x.ToString().Trim() == "Is Active").ColumnIndex : (int?)null;
                        if (!IsActive.HasValue)
                        {
                            JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("Is Active does not exist..try again");
                            return Created(strJSON, strJSON);
                        }
                        Stock = rowData.Cells.Any(x => x.ToString().Trim() == "Stock") ? rowData.Cells.FirstOrDefault(x => x.ToString().Trim() == "Stock").ColumnIndex : (int?)null;
                        if (!Stock.HasValue)
                        {
                            JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("Stock does not exist..try again");
                            return Created(strJSON, strJSON);
                        }
                        MRP = rowData.Cells.Any(x => x.ToString().Trim() == "MRP") ? rowData.Cells.FirstOrDefault(x => x.ToString().Trim() == "MRP").ColumnIndex : (int?)null;
                        if (!MRP.HasValue)
                        {
                            JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("MRP NO does not exist..try again");
                            return Created(strJSON, strJSON);
                        }
                    }
                }
                else
                {
                    rowData = sheet.GetRow(iRowIdx);
                    DateTime datetoday = DateTime.Today;
                    cellData = rowData.GetCell(0);
                    rowData = sheet.GetRow(iRowIdx);
                    if (rowData != null)
                    {
                        FreebiesDTO exceluplaod = new FreebiesDTO();

                        cellData = rowData.GetCell(0);
                        col0 = cellData == null ? "" : cellData.ToString();
                        exceluplaod.warehouesId = Convert.ToInt32(col0);

                        cellData = rowData.GetCell(1);
                        col1 = cellData == null ? "" : cellData.ToString();
                        exceluplaod.OfferName = Convert.ToString(col1); ;


                        cellData = rowData.GetCell(2);
                        col2 = cellData == null ? "" : cellData.ToString();
                        exceluplaod.Itemnumber = Convert.ToString(col2);

                        cellData = rowData.GetCell(3);
                        col3 = cellData == null ? "" : cellData.ToString();
                        exceluplaod.SellingSKU = Convert.ToString(col3);


                        cellData = rowData.GetCell(4);
                        col4 = cellData == null ? "" : cellData.ToString();
                        exceluplaod.ItemName = Convert.ToString(col4);

                        cellData = rowData.GetCell(5);
                        col5 = cellData == null ? "" : cellData.ToString();
                        exceluplaod.MinimumOrderQty = Convert.ToInt32(col5);

                        cellData = rowData.GetCell(6);
                        col6 = cellData == null ? "" : cellData.ToString();
                        exceluplaod.FreeItemnumber = Convert.ToString(col6);

                        cellData = rowData.GetCell(7);
                        col7 = cellData == null ? "" : cellData.ToString();
                        exceluplaod.FreeItemName = Convert.ToString(col7);

                        cellData = rowData.GetCell(8);
                        col8 = cellData == null ? "" : cellData.ToString();
                        exceluplaod.NoOfFreeitemQty = Convert.ToInt32(col8);

                        cellData = rowData.GetCell(9);
                        col9 = cellData == null ? "" : cellData.ToString();
                        exceluplaod.FreeitemMRP = Convert.ToInt32(col9);

                        cellData = rowData.GetCell(10);
                        col10 = cellData == null ? "" : cellData.ToString();
                        exceluplaod.Discription = Convert.ToString(col10);

                        cellData = rowData.GetCell(11);
                        col11 = cellData == null ? "" : cellData.ToString();

                        DateTime? dtst = DateTimeHelper.ConvertToDateTime(col11);
                        if (dtst.HasValue)
                            exceluplaod.StartDate = dtst;//Convert.ToDateTime(col3);//Convert.ToDateTime(col1);

                        if (exceluplaod.StartDate < datetoday)
                        {
                            continue;
                        }

                        cellData = rowData.GetCell(12);
                        col12 = cellData == null ? "" : cellData.ToString();

                        DateTime? dtendt = DateTimeHelper.ConvertToDateTime(col12);
                        if (dtendt.HasValue)
                            exceluplaod.EndDate = dtendt;//Convert.ToDateTime(col3);//Convert.ToDateTime(col1);
                                                         //exceluplaod.OfferEndTime = DateTime.Parse(col2);
                        if (exceluplaod.EndDate < datetoday)
                        {
                            continue;

                        }

                        cellData = rowData.GetCell(13);
                        col13 = cellData == null ? "" : cellData.ToString();
                        exceluplaod.QtyAvaiable = Convert.ToInt32(col13);

                        cellData = rowData.GetCell(14);
                        col14 = cellData == null ? "" : cellData.ToString();
                        exceluplaod.FreeItemLimit = Convert.ToInt32(col14);

                        cellData = rowData.GetCell(15);
                        col15 = cellData == null ? "" : cellData.ToString();
                        exceluplaod.AppType = Convert.ToString(col15);

                        cellData = rowData.GetCell(16);
                        col16 = cellData == null ? "" : cellData.ToString();
                        exceluplaod.IsActive = Convert.ToInt32(col16);

                        cellData = rowData.GetCell(17);
                        col17 = cellData == null ? "" : cellData.ToString();
                        exceluplaod.Stock = Convert.ToString(col17);

                        cellData = rowData.GetCell(18);
                        col18 = cellData == null ? "" : cellData.ToString();
                        exceluplaod.MRP = Convert.ToInt32(col18);

                        if (col2 != "" && col3 != "")
                        {
                            return Created("Error", "Only one field is allowed ItemNumber Or SellingSKU");
                        }

                        if (col2 == "" && col3 == "")
                        {
                            return Created("Error", "Please fill atleast one field-ItemNumber Or SellingSKU");
                        }

                        flashdeallist.Add(exceluplaod);
                    }
                }
            }
            var result = AddFreebiesUpload(flashdeallist, userid);

            if (result != null)
                return result;
            else
                return result;
        }

        public IHttpActionResult AddFreebiesUpload(List<FreebiesDTO> Collection, int userId)
        {
            bool result = true;
            List<FreebiesDTO> flashdealList = new List<FreebiesDTO>();
            List<ErrorListDTO> listerror = new List<ErrorListDTO>();
            using (var context = new AuthContext())
            {
                List<string> str = new List<string>();
                List<GenricEcommers.Models.Offer> offers = new List<GenricEcommers.Models.Offer>();
                List<ItemMaster> itemList = new List<ItemMaster>();
                List<string> SellingSKU = new List<string>();
                if (Collection.Count > 0)
                {
                    ItemMaster itemMaster = new ItemMaster();
                    ItemMaster freeitem = new ItemMaster();
                    bool ItemCheck = false;
                    string WareHouseName = "";

                    var WarehoueIds = Collection.Select(x => x.warehouesId).Distinct();

                    //var SellingSKU= Collection.Select(x => x.SellingSKU);

                    var WareHouseNames = context.Warehouses.Where(x => WarehoueIds.Contains(x.WarehouseId)).Select(y => new { y.WarehouseId, y.WarehouseName }).ToList();
                    List<ItemMaster> ItemMasterLst = new List<ItemMaster>();
                    foreach (var Wid in WarehoueIds)
                    {
                        SellingSKU = Collection.Where(sku => !string.IsNullOrWhiteSpace(sku.SellingSKU) && sku.warehouesId == Wid).Select(x => x.SellingSKU).ToList();
                        List<FreebiesDTO> WarehoueWiseList = Collection.Where(x => x.warehouesId == Wid).ToList();

                        var itemNumbers = WarehoueWiseList.Select(x => x.Itemnumber + "-" + x.MRP).ToList();
                        // var itemNumbers = WarehoueWiseList.Select(x => x.Itemnumber).Where(no => !string.IsNullOrWhiteSpace(no)).ToList();

                        var freeitemNumbers = WarehoueWiseList.Select(x => x.FreeItemnumber + "-" + x.FreeitemMRP).ToList();
                        List<ItemMaster> itemMasters = new List<ItemMaster>();
                        if (SellingSKU.Count > 0)
                        {
                            itemMasters = context.itemMasters.Where(x => x.WarehouseId == Wid && SellingSKU.Contains(x.SellingSku) && !x.Deleted).ToList();
                        }
                        else
                        {
                            itemMasters = context.itemMasters.Where(x => itemNumbers.Contains(x.Number + "-" + x.price) && x.WarehouseId == Wid && !x.Deleted).ToList();
                        }
                        //var freeitems = context.itemMasters.Where(x => freeitemNumbers.Contains(x.Number + "-" + x.price) && x.WarehouseId == Wid && x.active == true).ToList();
                        var freeitems = context.itemMasters.Where(x => freeitemNumbers.Contains(x.Number + "-" + x.price) && x.WarehouseId == Wid && !x.Deleted).ToList();


                        foreach (var item in WarehoueWiseList)
                        {
                            bool isactive = false;
                            if (item.SellingSKU != "")
                            {
                                itemMasters = context.itemMasters.Where(x => x.WarehouseId == item.warehouesId && item.SellingSKU == x.SellingSku && !x.Deleted).ToList();
                            }
                            else
                            {
                                itemMasters = context.itemMasters.Where(x => itemNumbers.Contains(x.Number + "-" + x.price) && x.WarehouseId == item.warehouesId && !x.Deleted).ToList();
                            }
                            DateTime date1 = item.StartDate.Value;
                            DateTime date2 = item.EndDate.Value;
                            var ItemIds = new List<int>(); // Initialize as an empty list
                            WareHouseName = WareHouseNames.Where(x => x.WarehouseId == item.warehouesId).Select(y => y.WarehouseName).FirstOrDefault();

                            if (item.SellingSKU != "")
                            {
                                itemMaster = itemMasters.Where(x => x.WarehouseId == item.warehouesId && x.price == item.MRP && item.SellingSKU.Contains(x.SellingSku)).FirstOrDefault();
                                ItemIds = itemMasters.Where(x => x.WarehouseId == item.warehouesId && x.price == item.MRP && item.SellingSKU.Contains(x.SellingSku)).Select(x => x.ItemId).ToList();
                            }
                            else
                            {
                                itemMaster = itemMasters.Where(x => x.Number == item.Itemnumber && x.WarehouseId == item.warehouesId && x.price == item.MRP).FirstOrDefault();
                                ItemIds = itemMasters.Where(x => x.Number == item.Itemnumber && x.WarehouseId == item.warehouesId && x.price == item.MRP).Select(x => x.ItemId).ToList();
                            }
                            if (itemMaster != null)
                            {
                                isactive = Convert.ToBoolean(item.IsActive);
                            }
                            else
                            {
                                return Created("Error", "Error with Main Item");
                            }

                            //if (item.FreeItemnumber == item.Itemnumber)
                            //    freeitem = itemMaster;
                            if (item.FreeItemnumber == item.Itemnumber)
                                freeitem = freeitems.Where(x => x.Number == item.FreeItemnumber && x.price == item.FreeitemMRP && x.WarehouseId == item.warehouesId).FirstOrDefault();
                            else
                                freeitem = freeitems.Where(x => x.Number == item.FreeItemnumber && x.price == item.FreeitemMRP && x.WarehouseId == item.warehouesId).FirstOrDefault();

                            ItemMasterLst.Add(itemMaster);
                            if (itemMaster != null && freeitem != null)
                            {
                                ItemCheck = context.OfferDb.Any(q => ItemIds.Contains(q.itemId) && q.IsActive == true && q.ApplyType == item.AppType);
                            }
                            if (itemMaster != null && freeitem != null && !ItemCheck)
                            {
                                GenricEcommers.Models.Offer Newoffer = new GenricEcommers.Models.Offer();
                                Newoffer.CompanyId = 1;
                                Newoffer.StoreId = 0;
                                Newoffer.IsFreebiesLevel = !string.IsNullOrEmpty(item.SellingSKU) ? true : false;
                                Newoffer.userid = userId;
                                Newoffer.WarehouseId = item.warehouesId;
                                Newoffer.ApplyType = "Warehouse";
                                Newoffer.itemname = itemMaster.itemname;
                                Newoffer.FreeItemName = freeitem.itemname;
                                Newoffer.FreeItemMRP = freeitem.price;
                                Newoffer.start = date1;
                                Newoffer.end = date2;
                                Newoffer.itemId = itemMaster.ItemId;
                                //offer.IsDeleted = false;
                                Newoffer.IsDeleted = false;
                                Newoffer.IsActive = isactive;
                                Newoffer.OfferLogoUrl = "";
                                Newoffer.CreatedDate = indianTime;
                                Newoffer.UpdateDate = indianTime;
                                Newoffer.OfferCode = "";
                                Newoffer.CityId = itemMaster.Cityid;
                                Newoffer.Description = item.Discription;
                                Newoffer.DiscountPercentage = 0;
                                Newoffer.OfferName = item.OfferName;
                                Newoffer.OfferWithOtherOffer = false;
                                Newoffer.BillDiscountOfferOn = "FreeItem";
                                Newoffer.IsMultiTimeUse = false;
                                Newoffer.IsUseOtherOffer = false;
                                Newoffer.OfferOn = "Item";
                                Newoffer.FreeOfferType = "ItemMaster";
                                Newoffer.FreeItemLimit = item.FreeItemLimit; // add Item limit                                           
                                Newoffer.OffersaleQty = 0;
                                Newoffer.OfferAppType = item.AppType;
                                Newoffer.BillAmount = 0;
                                Newoffer.BillDiscountType = "items";
                                Newoffer.FreeItemId = freeitem.ItemId;
                                Newoffer.FreeItemMRP = freeitem.MRP;
                                Newoffer.FreeItemName = freeitem.itemname;
                                Newoffer.FreeWalletPoint = 0;
                                Newoffer.IsDispatchedFreeStock = item.Stock == "Free Stock" ? true : false;
                                Newoffer.IsOfferOnCart = false;
                                Newoffer.ItemNumber = item.Itemnumber;
                                Newoffer.LineItem = 0;
                                Newoffer.MaxBillAmount = 0;
                                Newoffer.MaxDiscount = 0;
                                Newoffer.MaxQtyPersonCanTake = 0;
                                Newoffer.MinOrderQuantity = item.MinimumOrderQty;
                                Newoffer.NoOffreeQuantity = item.NoOfFreeitemQty;
                                Newoffer.OfferCategory = "Offer";
                                Newoffer.QtyAvaiable = Convert.ToDouble(item.QtyAvaiable);
                                Newoffer.QtyConsumed = 0;

                                offers.Add(Newoffer);
                            }
                            else
                            {
                                ErrorListDTO listerrors = new ErrorListDTO();
                                listerrors.ItemName = item.ItemName;
                                listerrors.Itemnumber = item.Itemnumber;
                                listerrors.MRP = item.MRP;
                                listerrors.WareHouseName = WareHouseName;
                                listerror.Add(listerrors);
                            }
                        }
                        //context.OfferDb.AddRange(offers);
                    }
                    if (offers.Count == Collection.Count)
                    {
                        context.OfferDb.AddRange(offers);

                        if (context.Commit() > 0)
                        {
                            foreach (var offerdb in offers)
                            {
                                string code = "";
                                code = "ID_";

                                string offerCode = code + offerdb.OfferId;
                                offerdb.OfferCode = offerCode;

                                List<ItemMaster> itemnumber = new List<ItemMaster>();
                                if (offerdb.OfferOn == "Item")
                                {
                                    if (offerdb.OfferAppType != "Distributor App")
                                    {
                                        var OfferItem = ItemMasterLst.FirstOrDefault(x => x.ItemId == offerdb.itemId);
                                        if (offerdb.IsFreebiesLevel)
                                        {
                                            itemnumber = context.itemMasters.Where(x => x.SellingSku == OfferItem.SellingSku && x.ItemMultiMRPId == OfferItem.ItemMultiMRPId && x.WarehouseId == offerdb.WarehouseId && x.Deleted == false).ToList();
                                        }
                                        else
                                        {
                                            itemnumber = context.itemMasters.Where(x => x.Number == OfferItem.Number && x.ItemMultiMRPId == OfferItem.ItemMultiMRPId && x.WarehouseId == offerdb.WarehouseId && x.Deleted == false).ToList();
                                        }
                                        if (itemnumber.Any())
                                        {
                                            string ItemNumberColl = null;
                                            foreach (var collectionData in Collection)
                                            {
                                                ItemNumberColl = collectionData.FreeItemnumber;
                                            }
                                            foreach (var editItemMaster in itemnumber)
                                            {
                                                editItemMaster.IsOffer = true;
                                                editItemMaster.OfferCategory = 1;
                                                editItemMaster.OfferStartTime = offerdb.start;
                                                editItemMaster.OfferEndTime = offerdb.end;
                                                editItemMaster.OfferQtyAvaiable = offerdb.QtyAvaiable;
                                                editItemMaster.OfferQtyConsumed = offerdb.QtyConsumed;
                                                editItemMaster.OfferId = offerdb.OfferId;
                                                editItemMaster.OfferWalletPoint = offerdb.FreeWalletPoint;
                                                editItemMaster.OfferType = offerdb.FreeOfferType;
                                                editItemMaster.OfferFreeItemId = offerdb.FreeItemId;
                                                editItemMaster.OfferPercentage = offerdb.DiscountPercentage;
                                                editItemMaster.OfferMinimumQty = offerdb.MinOrderQuantity;
                                                editItemMaster.OfferFreeItemName = offerdb.FreeItemName;
                                                editItemMaster.OfferFreeItemQuantity = offerdb.NoOffreeQuantity;
                                                editItemMaster.ModifiedBy = userId;
                                                editItemMaster.UpdatedDate = indianTime;
                                                if (offerdb.FreeItemId > 0 && offerdb.FreeOfferType == "ItemMaster")
                                                {
                                                    ItemMaster imagedata = context.itemMasters.Where(x => x.ItemId == offerdb.FreeItemId && x.WarehouseId == offerdb.WarehouseId).FirstOrDefault();
                                                    if(imagedata != null)
                                                    {
                                                        editItemMaster.OfferFreeItemImage = imagedata.LogoUrl;
                                                    }
                                                    //ItemMasterCentral imageitem = context.ItemMasterCentralDB.Where(x => ItemNumberColl.Contains(x.Number)).FirstOrDefault();
                                                    //editItemMaster.OfferFreeItemImage = imagedata.LogoUrl;

                                                }
                                                context.Entry(editItemMaster).State = EntityState.Modified;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        var itemmaster = context.itemMasters.Where(x => x.ItemId == offerdb.itemId).FirstOrDefault();
                                        ItemMaster warehousefreeitem = new ItemMaster();
                                        var imageitem = "";
                                        if (offerdb.FreeOfferType == "ItemMaster")
                                        {
                                            var itemmasterdata = context.itemMasters.Where(x => x.ItemId == offerdb.FreeItemId && x.WarehouseId == offerdb.WarehouseId).FirstOrDefault();
                                            if(itemmasterdata != null)
                                            {
                                                imageitem = itemmasterdata.LogoUrl;
                                            }
                                            //imageitem = context.ItemMasterCentralDB.Where(x => x.SellingSku == freeitem.SellingSku).Select(x => x.LogoUrl).FirstOrDefault();
                                        }
                                        GenricEcommers.Models.OfferFreeItem offerFreeItem = new GenricEcommers.Models.OfferFreeItem
                                        {
                                            FreeItemId = offerdb.FreeItemId,
                                            ItemNumber = offerdb.IsFreebiesLevel == true? itemmaster.SellingSku :offerdb.ItemNumber,
                                            OfferFreeItemImage = imageitem,
                                            OfferFreeItemName = offerdb.FreeItemName,
                                            OfferFreeItemQuantity = offerdb.NoOffreeQuantity,
                                            OfferMinimumQty = offerdb.MinOrderQuantity,
                                            OfferQtyAvaiable = Convert.ToDouble(offerdb.FreeItemLimit),
                                            OfferQtyConsumed = offerdb.QtyConsumed,
                                            OfferType = offerdb.FreeOfferType,
                                            OfferWalletPoint = offerdb.FreeWalletPoint,
                                            ItemMultiMRPId = itemmaster.ItemMultiMRPId,
                                            OfferOn = offerdb.IsFreebiesLevel == true ? "SellingSku" : "Item",
                                        };

                                        if (offerdb.OfferFreeItems == null)
                                            offerdb.OfferFreeItems = new List<GenricEcommers.Models.OfferFreeItem>();

                                        offerdb.OfferFreeItems.Add(offerFreeItem);
                                    }
                                }

                                context.Entry(offerdb).State = EntityState.Modified;
                            }
                            if (context.Commit() > 0)
                            {
                                result = true;
                                return Created("Success", "Success");
                            }
                            else
                            {
                                foreach (var offerdb in offers)
                                {
                                    offerdb.IsActive = false;
                                    offerdb.IsDeleted = true;
                                    context.Entry(offerdb).State = EntityState.Modified;
                                }
                                context.Commit();
                            }
                        }
                    }
                    else
                    {
                        //    return Ok(listerror);
                        return Created("Error", "Error");
                    }

#if !DEBUG
                    if (result)
                    {
                        foreach (var Offer in offers)
                        {
                            if (Offer.IsActive && Offer.end > DateTime.Now)
                            {

                                var jobId = BackgroundJob.Schedule(
                                          () => ActiveDeativeofferByJob(Offer.OfferId),
                                TimeSpan.FromMinutes((Offer.end - DateTime.Now).TotalMinutes));
                                MongoDbHelper<ScheduleJobHistory> mongoDbHelper = new MongoDbHelper<ScheduleJobHistory>();
                                ScheduleJobHistory scheduleJobHistory = new ScheduleJobHistory
                                {
                                    CreatedDate = DateTime.Now,
                                    isActive = true,
                                    jobid = jobId,
                                    ObjectId = Offer.OfferId.ToString(),
                                    ObjectType = "Offer"
                                };
                                mongoDbHelper.Insert(scheduleJobHistory);
                            }

                        }
                    }
#endif

                }
                return Created("Error", "Error");
            }

        }

        [HttpPost]
        [Route("OfferUploadFile")]
        [AllowAnonymous]
        public IHttpActionResult UploadOfferBillDiscountFile()
        {
            string res = string.Empty;
            if (HttpContext.Current.Request.Files.AllKeys.Any())
            {

                var formData1 = HttpContext.Current.Request.Form["compid"];
                // Access claims
                var identity = User.Identity as ClaimsIdentity;
                int userid = 0;

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                    userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
                // Get the uploaded image from the Files collection
                var httpPostedFile = HttpContext.Current.Request.Files["file"];

                if (httpPostedFile != null)
                {
                    byte[] buffer = new byte[httpPostedFile.ContentLength];

                    using (BinaryReader br = new BinaryReader(httpPostedFile.InputStream))
                    {
                        br.Read(buffer, 0, buffer.Length);
                    }
                    XSSFWorkbook hssfwb;
                    //   XSSFWorkbook workbook1;
                    using (MemoryStream memStream = new MemoryStream())
                    {
                        BinaryFormatter binForm = new BinaryFormatter();
                        memStream.Write(buffer, 0, buffer.Length);
                        memStream.Seek(0, SeekOrigin.Begin);
                        hssfwb = new XSSFWorkbook(memStream);
                    }

                    res = ReadOfferUploadedFile(hssfwb, userid);
                }
            }
            return Created(res, res);
        }
        public string ReadOfferUploadedFile(XSSFWorkbook hssfwb, int userid)
        {
            var identity = User.Identity as ClaimsIdentity;
            int compid = 0;

            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))
                compid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);

            using (var context = new AuthContext())
            {
                int a = 0;
                string ErrorMsg = string.Empty;

                var Store = context.StoreDB.Where(x => x.IsActive == true && x.IsDeleted == false).Select(x => new { StoreId = x.Id, StoreName = x.Name }).ToList();

                BillDiscountResponseExcel BillDiscount = new BillDiscountResponseExcel();
                IncludeItemResponseExcel IncludeItem = new IncludeItemResponseExcel();
                ExcludeItemResponseExcel ExcludeItem = new ExcludeItemResponseExcel();
                MandatoryResponseExcel Mandatory = new MandatoryResponseExcel();
                FreeitemResponseExcel FreeItem = new FreeitemResponseExcel();
                BillDiscountAllFilesDC BillDiscountAllFiles = new BillDiscountAllFilesDC();
                BillDiscountAllFiles.BillDiscountResponseExcel = new List<BillDiscountResponseExcel>();
                BillDiscountAllFiles.IncludeItemResponseExcel = new List<IncludeItemResponseExcel>();
                BillDiscountAllFiles.ExcludeItemResponseExcel = new List<ExcludeItemResponseExcel>();
                BillDiscountAllFiles.MandatoryResponseExcel = new List<MandatoryResponseExcel>();
                BillDiscountAllFiles.FreeitemResponseExcel = new List<FreeitemResponseExcel>();
                #region Column
                List<OfferColumn> BillDiscountColumn = new List<OfferColumn> {
                   new OfferColumn { ColumnName="Warehouse Name",DataType=typeof(string),IsRequired=true },
                   new OfferColumn { ColumnName="Select App Type",DataType=typeof(string),IsRequired=true,RequiredValues=new List<string>{ "Both", "Retailer App", "Sales App", "Distributor App" } },
                   new OfferColumn { ColumnName="Offer Name",DataType=typeof(string),IsRequired=true },
                   new OfferColumn { ColumnName="Offer On",DataType=typeof(string),IsRequired=true,RequiredValues=new List<string>{ "BillDiscount","ScratchBillDiscount","Item"}},
                   new OfferColumn { ColumnName="Auto Apply",DataType=typeof(int),RequiredValues=new List<string>{ "0","1" } },
                   new OfferColumn { ColumnName="Offer By",DataType=typeof(string),IsRequired=true,RequiredValues=new List<string>{ "Store", "Company" }  },
                   new OfferColumn { ColumnName="Select Store",DataType=typeof(string) }, 
                   //,RequiredValues=new List<string>{ "Store1","Store2","Store3","Kisan Kirana","Safoya" }
                   new OfferColumn { ColumnName="BillDiscountType",DataType=typeof(string)},
                   new OfferColumn { ColumnName="User Type",DataType=typeof(string),IsRequired=true ,RequiredValues=new List<string>{ "All Customer","Group","Level","KPP Customer" }},
                   new OfferColumn { ColumnName="Group/Level Name",DataType=typeof(string) },
                   new OfferColumn { ColumnName="Exclude User Group",DataType=typeof(string) },
                   new OfferColumn { ColumnName="Multi Time Use",DataType=typeof(int)},
                   new OfferColumn { ColumnName="Offer Use Count",DataType=typeof(int) },
                   new OfferColumn { ColumnName="Use other Offer",DataType=typeof(int)},
                   new OfferColumn { ColumnName="Combined Offer Group",DataType=typeof(int)},
                   new OfferColumn { ColumnName="Offer Limit",DataType=typeof(int) },
                   new OfferColumn { ColumnName="Start Date time",DataType=typeof(DateTime),IsRequired=true },
                   new OfferColumn { ColumnName="End Date Time",DataType=typeof(DateTime),IsRequired=true },
                   new OfferColumn { ColumnName="Discount On",DataType=typeof(string),IsRequired=true,RequiredValues=new List<string>{ "Percentage","Wallet Point","Free item" } },
                   new OfferColumn { ColumnName="Percentage",DataType=typeof(decimal) },
                   new OfferColumn { ColumnName="Wallet Type",DataType=typeof(string),RequiredValues=new List<string>{ "Wallet Percentage","Amount" }  },
                   new OfferColumn { ColumnName="Apply On",DataType=typeof(string),IsRequired=true,RequiredValues=new List<string>{ "Pre Offer","Post Offer" }},
                   new OfferColumn { ColumnName="Wallet Point",DataType=typeof(int)},
                   new OfferColumn { ColumnName="Bill Amount Min Limit",DataType=typeof(int),IsRequired=true},
                   new OfferColumn { ColumnName="Bill Amount Max Limit",DataType=typeof(int)},
                   new OfferColumn { ColumnName="Maximum Discount",DataType=typeof(int)},
                   new OfferColumn { ColumnName="Number of Line Item",DataType=typeof(int)},
                   //new OfferColumn { ColumnName="Add Line item Value",DataType=typeof(int)},
                   new OfferColumn { ColumnName="Description",DataType=typeof(string)},
                };

                List<OfferColumn> IncludeItemColumn = new List<OfferColumn> {
                   new OfferColumn { ColumnName="Warehouse Name",DataType=typeof(string),IsRequired=true },
                   new OfferColumn { ColumnName="Offer Name",DataType=typeof(string),IsRequired=true },
                   new OfferColumn { ColumnName="Category",DataType=typeof(string) },
                   new OfferColumn { ColumnName="Sub Category",DataType=typeof(string) },
                   new OfferColumn { ColumnName="Brand",DataType=typeof(string) },
                   new OfferColumn { ColumnName="ItemMultiMRPId",DataType=typeof(int) },
                };

                List<OfferColumn> ExcludeItemColumn = new List<OfferColumn> {
                   new OfferColumn { ColumnName="Warehouse Name",DataType=typeof(string),IsRequired=true },
                   new OfferColumn { ColumnName="Offer Name",DataType=typeof(string),IsRequired=true },
                   new OfferColumn { ColumnName="Category",DataType=typeof(string) },
                   new OfferColumn { ColumnName="Sub Category",DataType=typeof(string) },
                   new OfferColumn { ColumnName="Brand",DataType=typeof(string) },
                   new OfferColumn { ColumnName="ItemMultiMRPId",DataType=typeof(int) },
                };

                List<OfferColumn> MandatoryColumn = new List<OfferColumn> {
                   new OfferColumn { ColumnName="Warehouse Name",DataType=typeof(string),IsRequired=true },
                   new OfferColumn { ColumnName="Offer Name",DataType=typeof(string),IsRequired=true },
                   new OfferColumn { ColumnName="ItemRequiredOn",DataType=typeof(string),IsRequired = true,RequiredValues=new List<string>{ "Item","Brand" }  },
                   new OfferColumn { ColumnName="Category",DataType=typeof(string) },
                   new OfferColumn { ColumnName="Sub Category",DataType=typeof(string) },
                   new OfferColumn { ColumnName="Brand",DataType=typeof(string) },
                   new OfferColumn { ColumnName="ItemMultiMRPId",DataType=typeof(int) },
                    new OfferColumn { ColumnName="ValueType",DataType=typeof(string),IsRequired=true,RequiredValues = new List<string>{ "Qty","Value"} },
                    new OfferColumn { ColumnName="Qty/Value",DataType=typeof(int) ,IsRequired = true},
                };

                List<OfferColumn> FreeitemColumn = new List<OfferColumn> {
                   new OfferColumn { ColumnName="Warehouse Name",DataType=typeof(string),IsRequired=true },
                   new OfferColumn { ColumnName="Offer Name",DataType=typeof(string),IsRequired=true },
                   new OfferColumn { ColumnName="Category",DataType=typeof(string) },
                   new OfferColumn { ColumnName="Sub Category",DataType=typeof(string) },
                   new OfferColumn { ColumnName="Brand",DataType=typeof(string) },
                   new OfferColumn { ColumnName="ItemMultiMRPId",DataType=typeof(int) },
                    new OfferColumn { ColumnName="Stock Hit",DataType=typeof(string),RequiredValues=new List<string>{ "Free Stock","Current Stock" } },  //RequiredValues=new List<string>{ "Free Stock","Current Stock" }},
                   new OfferColumn { ColumnName="Freeitem Qty",DataType=typeof(int) },
                   new OfferColumn { ColumnName="SellingSku",DataType=typeof(string),IsRequired = true },

                };

                #endregion


                try
                {
                    if (hssfwb.NumberOfSheets == 5)
                    {

                        List<OfferColumn> cols = new List<OfferColumn>();
                        ISheet sheet = null;
                        string sSheetName = "";
                        for (int i = 0; i < 5; i++)
                        {
                            sSheetName = hssfwb.GetSheetName(i);
                            sheet = hssfwb.GetSheet(sSheetName);

                            if (sSheetName == "Bill Discount")
                                cols = BillDiscountColumn;
                            else if (sSheetName == "Include Item")
                                cols = IncludeItemColumn;
                            else if (sSheetName == "Exclude Item")
                                cols = ExcludeItemColumn;
                            else if (sSheetName == "Mandatory")
                                cols = MandatoryColumn;
                            else if (sSheetName == "Free item")
                                cols = FreeitemColumn;

                            ErrorMsg += ValidateSheet(sheet, cols);
                        }
                        //Fill Data
                        if ((!string.IsNullOrEmpty(ErrorMsg) || ErrorMsg == ""))
                        {
                            for (int i = 0; i < 5; i++)
                            {
                                sSheetName = hssfwb.GetSheetName(i);
                                if (sSheetName == "Bill Discount")
                                {
                                    BillDiscount = ReadOfferBillDiscountFile(hssfwb, userid, i, BillDiscountColumn);
                                }
                                else if (sSheetName == "Include Item")
                                {
                                    IncludeItem = ReadOfferIncludeItemFile(hssfwb, userid, i);
                                }
                                else if (sSheetName == "Exclude Item")
                                {
                                    ExcludeItem = ReadOfferExcludeItemFile(hssfwb, userid, i);
                                }
                                else if (sSheetName == "Mandatory")
                                {
                                    Mandatory = ReadOfferMandatoryItemFile(hssfwb, userid, i);
                                }
                                else if (sSheetName == "Free item")
                                {
                                    FreeItem = ReadOfferFreeitemFile(hssfwb, userid, i);
                                }
                            }
                        }
                        else
                        {
                            return ErrorMsg;
                        }
                    }
                    else
                    {
                        ErrorMsg += " Sheets are missing";

                    }
                    if (BillDiscount != null && BillDiscount.billDiscountDC.Any())
                    {
                        string emsg = string.Empty;
                        BillDiscount.billDiscountDC.ForEach(data =>
                        {
                            if (string.IsNullOrEmpty(data.OfferName))
                            {
                                emsg += ", " + "Offer Name can not be null";

                            }
                        });

                        int offertotalcount, offerdistinctcount = 0;
                        offertotalcount = BillDiscount.billDiscountDC.Where(x => !string.IsNullOrEmpty(x.OfferName)).Select(y => y.OfferName.Trim().ToLower()).Count();
                        offerdistinctcount = BillDiscount.billDiscountDC.Where(x => !string.IsNullOrEmpty(x.OfferName)).Select(y => y.OfferName.Trim().ToLower()).Distinct().Count();

                        if (offertotalcount > 0 && offerdistinctcount > 0)
                        {
                            if (offertotalcount != offerdistinctcount)
                            {
                                emsg += ", " + "Offer Name can not be same";
                            }
                        }

                        if (emsg != "")
                        {
                            return emsg;
                        }


                        List<string> includeWarehouseName = new List<string>();
                        List<string> includeOfferName = new List<string>();
                        List<string> excludeSheetWarehouse = new List<string>();
                        List<string> excludeSheetofferName = new List<string>();
                        List<string> FreeitemWarehouse = new List<string>();
                        List<string> FreeitemOffer = new List<string>();
                        List<string> mandatoryWarehouse = new List<string>();
                        List<string> mandatoryOffer = new List<string>();
                        var BillWarehouseName = BillDiscount.billDiscountDC.Select(Y => Y.WarehouseName).ToList();
                        var BillOfferName = BillDiscount.billDiscountDC.Select(y => y.OfferName).ToList();
                        if (IncludeItem.IncludeItemDC != null)
                        {
                            includeWarehouseName = IncludeItem.IncludeItemDC.Select(X => X.WarehouseName).ToList();
                            includeOfferName = IncludeItem.IncludeItemDC.Select(X => X.Offer_Name).ToList();
                        }
                        if (ExcludeItem.ExcludeItemDC != null)
                        {
                            excludeSheetWarehouse = ExcludeItem.ExcludeItemDC.Select(X => X.WarehouseName).ToList();
                            excludeSheetofferName = ExcludeItem.ExcludeItemDC.Select(X => X.Offer_Name).ToList();
                        }
                        if (Mandatory.MandatoryDC != null)
                        {
                            mandatoryWarehouse = Mandatory.MandatoryDC.Select(X => X.WarehouseName).ToList();
                            mandatoryOffer = Mandatory.MandatoryDC.Select(X => X.Offer_Name).ToList();
                        }


                        if (FreeItem.FreeitemDC != null)
                        {
                            FreeitemWarehouse = FreeItem.FreeitemDC.Select(X => X.WarehouseName).ToList();
                            FreeitemOffer = FreeItem.FreeitemDC.Select(X => X.Offer_Name).ToList();
                        }





                        List<string> storename = BillDiscount.billDiscountDC.Select(x => x.SelectStore).ToList();

                        includeWarehouseName.ForEach(data =>
                        {
                            if (!BillWarehouseName.Any(x => data.Equals(x)))
                            {
                                ErrorMsg = ErrorMsg + data + " Warehouse Name  is not matching in Bill Discount and Include Item Sheet ";
                            }
                        });

                        includeOfferName.ForEach(data =>
                        {
                            if (!BillOfferName.Any(x => data.Equals(x)))
                            {
                                ErrorMsg = ErrorMsg + ", " + data + " Offer Name is not matching in Bill Discount and Include Item Sheet";
                            }
                        });


                        excludeSheetWarehouse.ForEach(data =>
                        {
                            if (!BillWarehouseName.Any(x => data.Equals(x)))
                            {
                                ErrorMsg = ErrorMsg + ", " + data + " Warehouse Name  is not matching in Bill Discount and Exclude Item Sheet ";
                            }
                        });

                        excludeSheetofferName.ForEach(data =>
                        {
                            if (!BillOfferName.Any(x => data.Equals(x)))
                            {
                                ErrorMsg = ErrorMsg + ", " + data + " Offer Name is not matching in Bill Discount and Exclude Item Sheet";
                            }
                        });


                        mandatoryWarehouse.ForEach(data =>
                        {
                            if (!BillWarehouseName.Any(x => data.Equals(x)))
                            {
                                ErrorMsg = ErrorMsg + ", " + data + " Warehouse Name  is not matching in Bill Discount and Mandatory Sheet";
                            }
                        });

                        mandatoryOffer.ForEach(data =>
                        {
                            if (!BillOfferName.Any(x => data.Equals(x)))
                            {
                                ErrorMsg = ErrorMsg + ", " + data + " Offer Name is not matching in Bill Discount and Mandatory Sheet";
                            }
                        });


                        FreeitemWarehouse.ForEach(data =>
                        {
                            if (!BillWarehouseName.Any(x => data.Equals(x)))
                            {
                                ErrorMsg = ErrorMsg + ", " + data + " Warehouse Name  is not matching in Bill Discount and Free Sheet";
                            }
                        });

                        FreeitemOffer.ForEach(data =>
                        {
                            if (!BillOfferName.Any(x => data.Equals(x)))
                            {
                                ErrorMsg = ErrorMsg + ", " + data + " Offer Name is not matching in Bill Discount and Free Sheet";
                            }
                        });

                        BillDiscount.billDiscountDC.ForEach(data =>
                        {
                            if (!string.IsNullOrEmpty(data.SelectStore))
                            {
                                var storeindb = Store.FirstOrDefault(x => x.StoreName.Trim().ToLower() == data.SelectStore.Trim().ToLower());
                                if (storeindb == null)
                                {
                                    ErrorMsg = ErrorMsg + ", " + data.SelectStore + " is not present in Database";
                                }

                            }
                        });

                        List<int> MultiMRPIds = new List<int>();
                        //List<int> MutliMRPIds = new List<int>();
                        if (IncludeItem.Status)
                        {
                            var multimrp = IncludeItem.IncludeItemDC.Where(x => x.ItemMultiMRPId != null && x.ItemMultiMRPId > 0).Select(x => x.ItemMultiMRPId).ToList();
                            MultiMRPIds.AddRange(multimrp);
                        }
                        if (ExcludeItem.Status)
                        {
                            var multimrp = ExcludeItem.ExcludeItemDC.Where(x => x.ItemMultiMRPId > 0).Select(x => x.ItemMultiMRPId).ToList();
                            MultiMRPIds.AddRange(multimrp);
                        }
                        if (FreeItem.Status)
                        {
                            var multimrp = FreeItem.FreeitemDC.Where(x => x.ItemMultiMRPId > 0).Select(x => x.ItemMultiMRPId).ToList();
                            MultiMRPIds.AddRange(multimrp);
                        }
                        if (Mandatory.Status)
                        {
                            var multimrp = Mandatory.MandatoryDC.Where(x => x.ItemMultiMRPId > 0).Select(x => x.ItemMultiMRPId).ToList();
                            MultiMRPIds.AddRange(multimrp);
                        }

                        var Warehousenamelist = BillDiscount.billDiscountDC.Select(x => x.WarehouseName).Distinct().ToList();
                        var WarehouseData = context.Warehouses.Where(x => Warehousenamelist.Contains(x.WarehouseName) && x.active == true && x.Deleted == false).ToList();
                        var WarehouseList = WarehouseData.Select(y => y.WarehouseId).Distinct().ToList();
                        var itemmasters = context.itemMasters.Where(x => MultiMRPIds.Contains(x.ItemMultiMRPId) && WarehouseList.Contains(x.WarehouseId) && x.active && !x.Deleted).ToList();

                        foreach (var i in MultiMRPIds)
                        {
                            var idata = itemmasters.FirstOrDefault(x => x.ItemMultiMRPId == i);
                            if (idata == null)
                            {
                                ErrorMsg += ", " + " Itemmultimrp " + i + " is wrong";
                            }
                        }

                        List<string> categorylist = new List<string>();
                        //List<int> MutliMRPIds = new List<int>();
                        if (IncludeItem.Status)
                        {
                            var multimrp = IncludeItem.IncludeItemDC.Where(x => x.Category != null && !string.IsNullOrEmpty(x.Category)).Select(x => x.Category).ToList();
                            categorylist.AddRange(multimrp);
                        }
                        if (ExcludeItem.Status)
                        {
                            var multimrp = ExcludeItem.ExcludeItemDC.Where(x => x.Category != null && !string.IsNullOrEmpty(x.Category)).Select(x => x.Category).ToList();
                            categorylist.AddRange(multimrp);
                        }
                        if (FreeItem.Status)
                        {
                            var multimrp = FreeItem.FreeitemDC.Where(x => x.Category != null && !string.IsNullOrEmpty(x.Category)).Select(x => x.Category).ToList();
                            categorylist.AddRange(multimrp);
                        }
                        if (Mandatory.Status)
                        {
                            var multimrp = Mandatory.MandatoryDC.Where(x => x.Category != null && !string.IsNullOrEmpty(x.Category)).Select(x => x.Category).ToList();
                            categorylist.AddRange(multimrp);
                        }
                        if (categorylist.Count() > 0)
                        {
                            categorylist = categorylist.Select(x => x.Trim().ToLower()).Distinct().ToList();
                            var categoryDatalist = context.Categorys.Where(x => categorylist.Contains(x.CategoryName.Trim().ToLower()) && x.IsActive && !x.Deleted).Select(x => new { x.Categoryid, x.CategoryName }).ToList();
                            if (categorylist.Count() > 0)
                            {
                                foreach (var ct in categorylist)
                                {
                                    var ispresent = categoryDatalist.FirstOrDefault(x => x.CategoryName.Trim().ToLower() == ct);
                                    if (ispresent == null) { ErrorMsg += ", " + " Category " + ct + " is not present in our System"; }
                                }
                            }
                        }

                        List<string> subcategorylist = new List<string>();
                        //List<int> MutliMRPIds = new List<int>();
                        if (IncludeItem.Status)
                        {
                            var multimrp = IncludeItem.IncludeItemDC.Where(x => x.Sub_Category != null && !string.IsNullOrEmpty(x.Sub_Category)).Select(x => x.Sub_Category).ToList();
                            subcategorylist.AddRange(multimrp);
                        }
                        if (ExcludeItem.Status)
                        {
                            var multimrp = ExcludeItem.ExcludeItemDC.Where(x => x.Sub_Category != null && !string.IsNullOrEmpty(x.Sub_Category)).Select(x => x.Sub_Category).ToList();
                            subcategorylist.AddRange(multimrp);
                        }
                        if (FreeItem.Status)
                        {
                            var multimrp = FreeItem.FreeitemDC.Where(x => x.Sub_Category != null && !string.IsNullOrEmpty(x.Sub_Category)).Select(x => x.Sub_Category).ToList();
                            subcategorylist.AddRange(multimrp);
                        }
                        if (Mandatory.Status)
                        {
                            var multimrp = Mandatory.MandatoryDC.Where(x => x.Sub_Category != null && !string.IsNullOrEmpty(x.Sub_Category)).Select(x => x.Sub_Category).ToList();
                            subcategorylist.AddRange(multimrp);
                        }
                        if (subcategorylist.Count() > 0)
                        {
                            subcategorylist = subcategorylist.Select(x => x.Trim().ToLower()).Distinct().ToList();
                            var subcategoryDatalist = context.SubCategorys.Where(x => subcategorylist.Contains(x.SubcategoryName.Trim().ToLower()) && x.IsActive && !x.Deleted).Select(x => new { x.SubCategoryId, x.SubcategoryName }).ToList();
                            if (subcategorylist.Count() > 0)
                            {
                                foreach (var ct in subcategorylist)
                                {
                                    var ispresent = subcategoryDatalist.FirstOrDefault(x => x.SubcategoryName.Trim().ToLower() == ct);
                                    if (ispresent == null) { ErrorMsg += ", " + " SubCategory " + ct + " is not present in our System"; }
                                }
                            }
                        }

                        List<string> brandcategorylist = new List<string>();
                        //List<int> MutliMRPIds = new List<int>();
                        if (IncludeItem.Status)
                        {
                            var multimrp = IncludeItem.IncludeItemDC.Where(x => x.BrandName != null && !string.IsNullOrEmpty(x.BrandName)).Select(x => x.BrandName).ToList();
                            brandcategorylist.AddRange(multimrp);
                        }
                        if (ExcludeItem.Status)
                        {
                            var multimrp = ExcludeItem.ExcludeItemDC.Where(x => x.BrandName != null && !string.IsNullOrEmpty(x.BrandName)).Select(x => x.BrandName).ToList();
                            brandcategorylist.AddRange(multimrp);
                        }
                        if (FreeItem.Status)
                        {
                            var multimrp = FreeItem.FreeitemDC.Where(x => x.BrandName != null && !string.IsNullOrEmpty(x.BrandName)).Select(x => x.BrandName).ToList();
                            brandcategorylist.AddRange(multimrp);
                        }
                        if (Mandatory.Status)
                        {
                            var multimrp = Mandatory.MandatoryDC.Where(x => x.BrandName != null && !string.IsNullOrEmpty(x.BrandName)).Select(x => x.BrandName).ToList();
                            brandcategorylist.AddRange(multimrp);
                        }
                        if (brandcategorylist.Count() > 0)
                        {
                            brandcategorylist = brandcategorylist.Select(x => x.Trim().ToLower()).Distinct().ToList();
                            var brandlist = context.SubsubCategorys.Where(x => brandcategorylist.Contains(x.SubsubcategoryName.Trim().ToLower()) && x.IsActive && !x.Deleted).Select(x => new { x.SubsubCategoryid, x.SubsubcategoryName }).ToList();
                            if (brandcategorylist.Count() > 0)
                            {
                                if (brandcategorylist.Count() == brandlist.Count() && brandlist.Count() > 0) { }
                                else
                                {
                                    foreach (var ct in brandcategorylist)
                                    {
                                        var ispresent = brandlist.FirstOrDefault(x => x.SubsubcategoryName.Trim().ToLower() == ct);
                                        if (ispresent == null) { ErrorMsg += ", " + " Brand " + ct + " is not present in our System"; }
                                    }
                                }
                            }
                        }

                        string query = "EXEC GetBrandCategoryMappings ";
                        var BrandCategoryMappings = context.Database.SqlQuery<GetBrandCategoryMappingsDC>(query).ToList();

                        var GroupList = GetNewCustomerGroupListForOffer(null);
                        var CombinedGroupList = GetAllExclusivegroup();
                        //List<string> combinedgroupnamelist = new List<string>();
                        //if(CombinedGroupList.Count() >0 && CombinedGroupList.Any())
                        //{
                        //    combinedgroupnamelist = CombinedGroupList.Where(x => !string.IsNullOrEmpty(x.Name)).Select(y => y.Name.Trim().ToLower()).ToList();
                        //}

                        BillDiscount.billDiscountDC.ForEach(item =>
                        {
                            string sdates = string.Empty;
                            string edates = string.Empty;
                            if (string.IsNullOrEmpty(item.SelectAppType))
                            {
                                ErrorMsg += ", " + "Select app type can not be null";
                            }
                            if (string.IsNullOrEmpty(item.OfferName))
                            {
                                ErrorMsg += ", " + "Offer Name can not be null";
                            }
                            if (string.IsNullOrEmpty(item.OfferOn))
                            {
                                ErrorMsg += ", " + "OfferOn can not be null";
                            }
                            if (string.IsNullOrEmpty(item.OfferBy))
                            {
                                ErrorMsg += ", " + "OfferBy can not be null";
                            }
                            if (string.IsNullOrEmpty(item.Usertype))
                            {
                                ErrorMsg += ", " + "Usertype can not be null";
                            }
                            if (item.StartDateTime == null)
                            {
                                ErrorMsg += ", " + "StartDateTime can not be null";
                            }
                            if (item.EndDateTime == null)
                            {
                                ErrorMsg += ", " + "EndDateTime can not be null";
                            }
                            if (string.IsNullOrEmpty(item.DiscountOn))
                            {
                                ErrorMsg += ", " + "DiscountOn can not be null";
                            }
                            if (string.IsNullOrEmpty(item.ApplyOn))
                            {
                                ErrorMsg += ", " + "ApplyOn can not be null";
                            }
                            if (!string.IsNullOrEmpty(item.ApplyOn))
                            {
                                if (item.ApplyOn.Trim().ToLower() == "pre offer")
                                {
                                    item.ApplyOn = "PreOffer";
                                }
                                else if (item.ApplyOn.Trim().ToLower() == "post offer")
                                {
                                    item.ApplyOn = "PostOffer";
                                }
                                else
                                {
                                    ErrorMsg += " ," + "Please Enter Correct Pre/Post Name";
                                }
                            }
                            if (!string.IsNullOrEmpty(item.Usertype))
                            {
                                if (item.Usertype.Trim().ToLower() == "group")
                                {
                                    if (string.IsNullOrEmpty(item.GroupLevelName))
                                    {
                                        ErrorMsg += ", " + "GroupName is compulsory for Group";
                                    }
                                }
                                if (item.Usertype.Trim().ToLower() == "level")
                                {
                                    if (string.IsNullOrEmpty(item.GroupLevelName))
                                    {
                                        ErrorMsg += ", " + "LevelName is compulsory for Level";
                                    }
                                }
                            }
                            if (item.BillAmountMinLimit <= 0)
                            {
                                ErrorMsg += ", " + "BillAmountMinLimit can not be 0";
                            }
                            if (string.IsNullOrEmpty(item.BillDiscountType))
                            {
                                item.BillDiscountType = "BillDiscount";
                            }
                            if (item.OfferBy.ToLower() == "company")
                            {
                                if (item.BillDiscountType.Trim().ToLower() == "item")
                                {
                                    item.BillDiscountType = "items";
                                    if (IncludeItem.IncludeItemDC != null)
                                    {
                                        var list = IncludeItem.IncludeItemDC.FirstOrDefault(x => x.WarehouseName.Trim() == item.WarehouseName.Trim() && x.Offer_Name.Trim() == item.OfferName.Trim());
                                        if (list == null)
                                        {
                                            ErrorMsg += ", " + "Warehouse Name or Offer Name Does not match ";
                                        }
                                        else
                                        {
                                            var ispresent = IncludeItem.IncludeItemDC.Any(x => x.WarehouseName.Trim() == item.WarehouseName.Trim() && x.Offer_Name.Trim() == item.OfferName.Trim() && x.ItemMultiMRPId <= 0);
                                            if (ispresent) { ErrorMsg += ", " + "ItemmultiMrpId is compulsory "; }
                                        }
                                    }
                                    else
                                    {
                                        ErrorMsg += ", " + "Please Enter at Least One item in Include Item File ";
                                    }
                                }
                            }
                            else
                            {
                                if (!string.IsNullOrEmpty(item.SelectStore))
                                {
                                    var res = Store.FirstOrDefault(x => x.StoreName.Trim().ToLower() == item.SelectStore.Trim().ToLower());
                                    if (res == null)
                                    {
                                        ErrorMsg += ", " + "Please Enter Correct Store Name";
                                    }
                                    else
                                    {
                                        var storeBrand = context.StoreBrandDB.Where(x => x.StoreId == res.StoreId && x.IsActive).ToList();
                                        if (item.BillDiscountType.Trim().ToLower() == "subcategory" || item.BillDiscountType.Trim().ToLower() == "brand")
                                        {
                                            //item.BillDiscountType = item.BillDiscountType.Trim().ToLower();
                                            if (IncludeItem.IncludeItemDC != null)
                                            {
                                                var list = IncludeItem.IncludeItemDC.Where(x => x.WarehouseName.Trim() == item.WarehouseName.Trim() && x.Offer_Name.Trim() == item.OfferName.Trim()).ToList();
                                                if (list.Count() == 0)
                                                {
                                                    ErrorMsg += ", " + "Warehouse Name or Offer Name Does not match ";
                                                }
                                                else
                                                {
                                                    if (item.BillDiscountType.Trim().ToLower() == "subcategory")
                                                    {
                                                        item.BillDiscountType = "subcategory";
                                                        bool sublist = list.All(x => x.Category != null && x.Sub_Category != null);
                                                        var subcatlist = list.Where(x => x.Category != null && !string.IsNullOrEmpty(x.Category) && x.Sub_Category != null && !string.IsNullOrEmpty(x.Sub_Category)).ToList();
                                                        if (sublist)
                                                        {
                                                            if (subcatlist.Count() > 0 && subcatlist.Any())
                                                            {
                                                                foreach (var sc in subcatlist)
                                                                {
                                                                    if (sc.BrandName == null)
                                                                        sc.BrandName = "";
                                                                    if (BrandCategoryMappings.Any(x => x.CategoryName.Trim().ToLower() == sc.Category.Trim().ToLower() && (x.SubcategoryName.Trim().ToLower() == sc.Sub_Category.Trim().ToLower() || x.SubsubcategoryName.Trim().ToLower() == sc.BrandName.Trim().ToLower())))
                                                                    {
                                                                        var brandMapping = BrandCategoryMappings.FirstOrDefault(x => x.CategoryName.Trim().ToLower() == sc.Category.Trim().ToLower() && x.SubcategoryName.Trim().ToLower() == sc.Sub_Category.Trim().ToLower() && x.SubsubcategoryName.Trim().ToLower() == sc.BrandName.Trim().ToLower());
                                                                        if (string.IsNullOrEmpty(sc.BrandName))
                                                                            brandMapping = BrandCategoryMappings.FirstOrDefault(x => x.CategoryName.Trim().ToLower() == sc.Category.Trim().ToLower() && x.SubcategoryName.Trim().ToLower() == sc.Sub_Category.Trim().ToLower());

                                                                        if (brandMapping != null)
                                                                        {
                                                                            sc.BrandCategoryMappingId = string.IsNullOrEmpty(sc.BrandName) ? 0 : brandMapping.BrandCategoryMappingId;

                                                                            sc.SubCategoryMappingId = brandMapping.SubCategoryMappingId;
                                                                        }
                                                                        else
                                                                        {
                                                                            ErrorMsg += ", " + (string.IsNullOrEmpty(ErrorMsg) ? "" : "\n") + sc.Category + ", " + sc.Sub_Category + ", " + sc.BrandName + " Mapping not correct in include Sheet.";
                                                                        }
                                                                        if (storeBrand != null && !storeBrand.Any(x => x.BrandCategoryMappingId == brandMapping.BrandCategoryMappingId))
                                                                        {
                                                                            ErrorMsg += ", " + (string.IsNullOrEmpty(ErrorMsg) ? "" : "\n") + sc.Category + ", " + sc.Sub_Category + ", " + sc.BrandName + " not in selected store in include Sheet.";
                                                                        }
                                                                    }
                                                                    else
                                                                    {
                                                                        ErrorMsg += ", " + (string.IsNullOrEmpty(ErrorMsg) ? "" : "\n") + sc.Category + ", " + sc.Sub_Category + ", " + sc.BrandName + " Mapping not correct in include Sheet.";
                                                                    }
                                                                }
                                                            }
                                                            else
                                                            {
                                                                ErrorMsg += ", " + "Please Enter Category and subcategory";
                                                            }
                                                        }
                                                        else
                                                        {
                                                            ErrorMsg += ", " + "Please Enter Category and subcategory";
                                                        }
                                                    }
                                                    else
                                                    {
                                                        item.BillDiscountType = "brand";
                                                        bool isbrandlist = list.All(x => x.Category != null && x.Sub_Category != null && x.BrandName != null);
                                                        var brandlist = list.Where(x => x.Category != null && !string.IsNullOrEmpty(x.Category) && x.Sub_Category != null && !string.IsNullOrEmpty(x.Sub_Category) && x.BrandName != null && !string.IsNullOrEmpty(x.BrandName)).ToList();
                                                        if (isbrandlist)
                                                        {
                                                            if (brandlist.Count() > 0 && brandlist.Any())
                                                            {
                                                                foreach (var sc in brandlist)
                                                                {
                                                                    if (BrandCategoryMappings.Any(x => x.CategoryName.Trim().ToLower() == sc.Category.Trim().ToLower() && (x.SubcategoryName.Trim().ToLower() == sc.Sub_Category.Trim().ToLower() || x.SubsubcategoryName.Trim().ToLower() == sc.BrandName.Trim().ToLower())))
                                                                    {
                                                                        var brandMapping = BrandCategoryMappings.FirstOrDefault(x => x.CategoryName.Trim().ToLower() == sc.Category.Trim().ToLower() && x.SubcategoryName.Trim().ToLower() == sc.Sub_Category.Trim().ToLower() && x.SubsubcategoryName.Trim().ToLower() == sc.BrandName.Trim().ToLower());
                                                                        if (string.IsNullOrEmpty(sc.BrandName))
                                                                            brandMapping = BrandCategoryMappings.FirstOrDefault(x => x.CategoryName.Trim().ToLower() == sc.Category.Trim().ToLower() && x.SubcategoryName.Trim().ToLower() == sc.Sub_Category.Trim().ToLower());

                                                                        if (brandMapping != null)
                                                                        {
                                                                            sc.BrandCategoryMappingId = string.IsNullOrEmpty(sc.BrandName) ? 0 : brandMapping.BrandCategoryMappingId;

                                                                            sc.SubCategoryMappingId = brandMapping.SubCategoryMappingId;
                                                                        }
                                                                        else
                                                                        {
                                                                            ErrorMsg += ", " + (string.IsNullOrEmpty(ErrorMsg) ? "" : "\n") + sc.Category + ", " + sc.Sub_Category + ", " + sc.BrandName + " Mapping not correct in include Sheet.";
                                                                        }
                                                                        if (storeBrand != null && !storeBrand.Any(x => x.BrandCategoryMappingId == brandMapping.BrandCategoryMappingId))
                                                                        {
                                                                            ErrorMsg += ", " + (string.IsNullOrEmpty(ErrorMsg) ? "" : "\n") + sc.Category + ", " + sc.Sub_Category + ", " + sc.BrandName + " not in selected store in include Sheet.";
                                                                        }
                                                                    }
                                                                    else
                                                                    {
                                                                        ErrorMsg += ", " + (string.IsNullOrEmpty(ErrorMsg) ? "" : "\n") + sc.Category + ", " + sc.Sub_Category + ", " + sc.BrandName + " Mapping not correct in include Sheet.";
                                                                    }
                                                                }
                                                            }
                                                            else
                                                            {
                                                                ErrorMsg += ", " + "Please Enter Category and subcategory and Brand";
                                                            }
                                                        }
                                                        else
                                                        {
                                                            ErrorMsg += ", " + "Please Enter Category and subcategory and Brand";
                                                        }


                                                    }
                                                }
                                            }
                                            else
                                            {
                                                ErrorMsg += ", " + "Please Enter at Least One item in Include Item File ";
                                            }
                                        }
                                        else
                                        {
                                            ErrorMsg += ", " + "Please Enter BillDiscountType";
                                        }
                                    }
                                }
                                else
                                {
                                    ErrorMsg += ", " + " Please Enter Correct Store Name Because you are applying Offer on Store ";
                                }

                            }


                            if (item.DiscountOn.Trim().ToLower() == "percentage")
                            {
                                item.DiscountOn = "Percentage";
                                if (item.Percentage <= 0)
                                {
                                    ErrorMsg += ", " + " Please Enter Correct Percentage ";
                                }
                            }
                            else if (item.DiscountOn.Trim().ToLower() == "wallet point")
                            {
                                item.DiscountOn = "Wallet point";
                                if (!string.IsNullOrEmpty(item.WalletType))
                                {
                                    if (item.WalletType.Trim().ToLower() == "wallet percentage")
                                    {
                                        item.WalletType = "WalletPercentage";
                                    }
                                    else if (item.WalletType.Trim().ToLower() == "amount")
                                    {
                                        item.WalletType = "Amount";
                                    }
                                    else
                                    {
                                        ErrorMsg += ", " + " Please Enter Correct wallet type ";
                                    }
                                }
                                if (string.IsNullOrEmpty(item.WalletType))
                                {
                                    ErrorMsg += ", " + " Please Enter Wallet Type";
                                }
                                if (item.WalletPoint <= 0)
                                {
                                    ErrorMsg += ", " + " Please Enter Wallet Point";
                                }
                            }
                            else
                            {
                                item.DiscountOn = "FreeItem";
                                if (FreeItem.FreeitemDC != null)
                                {
                                    var list = FreeItem.FreeitemDC.Where(x => x.WarehouseName == item.WarehouseName && x.Offer_Name == item.OfferName).ToList();
                                    if (list.Count() == 0)
                                    {
                                        ErrorMsg += ", " + "Warehouse Name or Offer Name Does not match";
                                    }
                                    else
                                    {
                                        var ispresent = FreeItem.FreeitemDC.Any(x => x.ItemMultiMRPId <= 0 || x.Freeitem_Qty <= 0 || string.IsNullOrEmpty(x.Stock_Hit));
                                        if (ispresent)
                                            ErrorMsg += ", " + "ItemultiMrpId or FreeitemQty or StockHit should not be empty";

                                        foreach (var it in list)
                                        {
                                            var ispresentdatabase = itemmasters.FirstOrDefault(x => x.ItemMultiMRPId == it.ItemMultiMRPId && x.SellingSku == it.SellingSku);
                                            if (ispresentdatabase == null)
                                                ErrorMsg += ", " + "ItemultiMrpId and SellingSku is not Match in Free Item Sheet";
                                        }
                                    }
                                }
                                else
                                {
                                    ErrorMsg += ", " + " Please Enter at Least One Row for Free Item.";
                                }
                            }

                            var warehouseids = WarehouseData.FirstOrDefault(x => x.WarehouseName.Trim().ToLower() == item.WarehouseName.Trim().ToLower());
                            if (warehouseids == null)
                            {
                                ErrorMsg += ", " + " You are Entered wrong Warehouse Name";
                            }

                            sdates = item.StartDateTime.ToString();
                            edates = item.EndDateTime.ToString();
                            if (sdates != null && edates != null)
                            {
                                if (sdates.Equals(edates))
                                {
                                    ErrorMsg += ", " + "StartDateTime and EndDateTime can not be same";
                                }
                                if (item.StartDateTime >= item.EndDateTime)
                                {
                                    ErrorMsg += ", " + "StartDateTime not be greater then Endtime";
                                }
                            }
                            else
                            {
                                ErrorMsg += ", " + "StartDateTime or EndDateTime can not be empty";
                            }

                            if (!string.IsNullOrEmpty(item.ExcludeUserGroup))
                            {
                                var isexcludepresent = GroupList.FirstOrDefault(x => x.GroupName.Trim().ToLower() == item.ExcludeUserGroup.Trim().ToLower());
                                if (isexcludepresent == null)
                                {
                                    ErrorMsg += ", " + "ExcludeUserGroup " + item.ExcludeUserGroup + " is not Present in our System";
                                }
                            }
                            if (!string.IsNullOrEmpty(item.CombinedOfferGroup))
                            {

                                var iscombinedgrouppresent = CombinedGroupList.Any(x => x.Name == item.CombinedOfferGroup);
                                if (!iscombinedgrouppresent)
                                {
                                    ErrorMsg += ", " + "CombinedOfferGroup " + item.CombinedOfferGroup + " is not Present in our System";
                                }
                            }

                            if (ExcludeItem.ExcludeItemDC != null)
                            {
                                var list = ExcludeItem.ExcludeItemDC.Where(x => x.WarehouseName == item.WarehouseName && x.Offer_Name == item.OfferName).ToList();
                                if (list.Count() > 0 && list.Any())
                                {
                                    if (item.OfferBy.ToLower() == "company" && item.BillDiscountType.ToLower() == "billdiscount")
                                    {
                                        var isnottpresent = list.Any(x => string.IsNullOrEmpty(x.Category));
                                        if (isnottpresent) { ErrorMsg += ", " + " Please Enter category name in Exclude Sheet"; }
                                    }
                                    if (item.OfferBy.Trim().ToLower() == "store" && item.BillDiscountType.Trim().ToLower() == "subcategory" && item.BillDiscountType.Trim().ToLower() == "brand")
                                    {
                                        var isnottpresents = list.Any(x => x.ItemMultiMRPId <= 0);
                                        if (isnottpresents) { ErrorMsg += ", " + " Please Enter ItemMultiMrpId in Exclude Sheet"; }
                                    }
                                }
                            }
                            if (Mandatory.MandatoryDC != null)
                            {
                                var list = Mandatory.MandatoryDC.Where(x => x.WarehouseName == item.WarehouseName && x.Offer_Name == item.OfferName).ToList();
                                if (list.Count() > 0 && list.Any())
                                {
                                    foreach (var items in list)
                                    {
                                        if (string.IsNullOrEmpty(items.ValueType))
                                            ErrorMsg = " " + ", Please Enter ValueType in Mandatory Sheet";
                                        if (items.Qty <= 0)
                                            ErrorMsg = " " + ", Please Enter Quantity in Mandatory Sheet";
                                        if (items.ItemRequiredOn.Trim().ToLower() == "item")
                                        {
                                            if (items.ItemMultiMRPId <= 0)
                                                ErrorMsg = " " + ", Please Enter ItemMultimrpId in Mandatory Sheet";
                                        }
                                        else
                                        {
                                            //if(string.IsNullOrEmpty(items.BrandName))
                                            //    ErrorMsg = " " + ", Please Enter Brand in Mandatory Sheet";
                                            if (items.ItemMultiMRPId == 0)
                                            {
                                                if (BrandCategoryMappings.Any(x => x.CategoryName.Trim().ToLower() == items.Category.Trim().ToLower() && (x.SubcategoryName.Trim().ToLower() == items.Sub_Category.Trim().ToLower() && x.SubsubcategoryName.Trim().ToLower() == items.BrandName.Trim().ToLower())))
                                                {
                                                    var brandMapping = BrandCategoryMappings.FirstOrDefault(x => x.CategoryName.Trim().ToLower() == items.Category.Trim().ToLower() && x.SubcategoryName.Trim().ToLower() == items.Sub_Category.Trim().ToLower() && x.SubsubcategoryName.Trim().ToLower() == items.BrandName.Trim().ToLower());

                                                    items.BrandId = brandMapping.BrandCategoryMappingId;

                                                }
                                                else
                                                    ErrorMsg += (string.IsNullOrEmpty(ErrorMsg) ? "" : "\n") + items.Category + ", " + items.Sub_Category + ", " + items.BrandName + " Mapping not correct in Mandatory Sheet.";
                                            }
                                        }
                                    }
                                }
                            }

                            if (item.OfferUseCount > 0)
                            { }
                            else
                            {
                                item.OfferUseCount = null;
                            }

                        });


                        //var SubcategoryData = context.SubCategorys.Where(x => subcategoryNames.Contains(x.SubcategoryName) && x.IsActive && !x.Deleted).Select(x => new { x.SubCategoryId, x.SubcategoryName }).ToList();
                        //var BrandData = context.SubsubCategorys.Where(x => BrandNames.Contains(x.SubsubcategoryName) && x.IsActive && !x.Deleted).Select(x => new { x.SubsubCategoryid, x.SubsubcategoryName }).ToList();

                        if (ErrorMsg != "")
                        {
                            BillDiscountAllFiles.msg = ErrorMsg;
                            BillDiscountAllFiles.Status = false;
                            BillDiscountAllFiles.BillDiscountResponseExcel = null;
                            BillDiscountAllFiles.IncludeItemResponseExcel = null;
                            BillDiscountAllFiles.ExcludeItemResponseExcel = null;
                            BillDiscountAllFiles.MandatoryResponseExcel = null;
                            BillDiscountAllFiles.FreeitemResponseExcel = null;

                        }
                        else
                        {
                            BillDiscountAllFiles.BillDiscountResponseExcel.Add(BillDiscount);
                            BillDiscountAllFiles.IncludeItemResponseExcel.Add(IncludeItem);
                            BillDiscountAllFiles.ExcludeItemResponseExcel.Add(ExcludeItem);
                            BillDiscountAllFiles.MandatoryResponseExcel.Add(Mandatory);
                            BillDiscountAllFiles.FreeitemResponseExcel.Add(FreeItem);




                            //string query = "EXEC GetBrandCategoryMappings ";
                            //var BrandCategoryMappings = context.Database.SqlQuery<GetBrandCategoryMappingsDC>(query).ToList();




                            List<FreeItemStockDc> freeItemStockDcs = new List<FreeItemStockDc>();
                            if (FreeItem.Status && FreeItem.FreeitemDC != null)
                            {
                                ParallelLoopResult parallelLoop = Parallel.ForEach(FreeItem.FreeitemDC.GroupBy(x => x.Stock_Hit), (item) =>
                                {
                                    FreeItemStockDc freeItemStock = new FreeItemStockDc();
                                    var FreestockMrpids = item.Select(x => x.ItemMultiMRPId).Distinct().ToList();
                                    var WarehouseNames = item.Select(x => x.WarehouseName).Distinct().ToList();
                                    if (item.FirstOrDefault().Stock_Hit.ToLower() == "current stock")
                                    {
                                        freeItemStockDcs.AddRange(context.DbCurrentStock.Where(x => FreestockMrpids.Contains(x.ItemMultiMRPId) && WarehouseNames.Contains(x.WarehouseName)).Select(x => new FreeItemStockDc { Stock = x.CurrentInventory, WarehouseId = x.WarehouseId, ItemMultiMRPId = x.ItemMultiMRPId, StockType = "Current Stock" }).ToList());
                                    }
                                    else if (item.FirstOrDefault().Stock_Hit.ToLower() == "free stock")
                                    {
                                        var Warehouseids = WarehouseData.Where(x => WarehouseNames.Contains(x.WarehouseName)).Select(x => x.WarehouseId).ToList();
                                        freeItemStockDcs.AddRange(context.FreeStockDB.Where(x => FreestockMrpids.Contains(x.ItemMultiMRPId) && Warehouseids.Contains(x.WarehouseId)).Select(x => new FreeItemStockDc { Stock = x.CurrentInventory, WarehouseId = x.WarehouseId, ItemMultiMRPId = x.ItemMultiMRPId, StockType = "Free Stock" }).ToList());
                                    }

                                });
                            }

                            //var MultiMRPData = context.ItemMultiMRPDB.Where(x => multiMrpIds.Contains(x.ItemMultiMRPId)).ToList();


                            var categoryNames = IncludeItem.Status ? IncludeItem.IncludeItemDC.Where(x => x.Category != null).Select(x => x.Category).ToList() : new List<string>();
                            var subcategoryNames = IncludeItem.Status ? IncludeItem.IncludeItemDC.Where(x => x.Sub_Category != null).Select(x => x.Sub_Category).ToList() : new List<string>();
                            var BrandNames = IncludeItem.Status ? IncludeItem.IncludeItemDC.Where(x => x.BrandName != null).Select(x => x.BrandName).ToList() : new List<string>();

                            categoryNames.AddRange(ExcludeItem.Status ? ExcludeItem.ExcludeItemDC.Where(x => x.Category != null).Select(x => x.Category).ToList() : new List<string>());
                            subcategoryNames.AddRange(ExcludeItem.Status ? ExcludeItem.ExcludeItemDC.Where(x => x.Sub_Category != null).Select(x => x.Sub_Category).ToList() : new List<string>());
                            BrandNames.AddRange(ExcludeItem.Status ? ExcludeItem.ExcludeItemDC.Where(x => x.BrandName != null).Select(x => x.BrandName).ToList() : new List<string>());


                            var categoryData = context.Categorys.Where(x => categoryNames.Contains(x.CategoryName) && x.IsActive && !x.Deleted).Select(x => new { x.Categoryid, x.CategoryName }).ToList();
                            var SubcategoryData = context.SubCategorys.Where(x => subcategoryNames.Contains(x.SubcategoryName) && x.IsActive && !x.Deleted).Select(x => new { x.SubCategoryId, x.SubcategoryName }).ToList();
                            var BrandData = context.SubsubCategorys.Where(x => BrandNames.Contains(x.SubsubcategoryName) && x.IsActive && !x.Deleted).Select(x => new { x.SubsubCategoryid, x.SubsubcategoryName }).ToList();

                            var CategoryIds = categoryData.Select(x => x.Categoryid).Distinct().ToList();
                            var SubCategoryIds = SubcategoryData.Select(x => x.SubCategoryId).Distinct().ToList();
                            var BrandIds = BrandData.Select(x => x.SubsubCategoryid).Distinct().ToList();

                            var CateSubCateMapping = context.SubcategoryCategoryMappingDb.Where(x => CategoryIds.Contains(x.Categoryid) && SubCategoryIds.Contains(x.SubCategoryId) && x.IsActive && !x.Deleted).ToList();
                            var SubcategoryCategoryMappingIds = CateSubCateMapping.Select(x => x.SubCategoryMappingId).ToList();
                            var CateBrandMapping = context.BrandCategoryMappingDb.Where(x => SubcategoryCategoryMappingIds.Contains(x.SubCategoryMappingId) && BrandIds.Contains(x.SubsubCategoryId) && x.IsActive && !x.Deleted).ToList();



                            List<Offer> offers = new List<Offer>();
                            foreach (var item in BillDiscount.billDiscountDC)
                            {
                                var warehousesID = WarehouseData.Where(x => x.WarehouseName == item.WarehouseName).Select(y => new Warehousedto { WarehouseId = y.WarehouseId, WarehouseName = y.WarehouseName }).FirstOrDefault();
                                List<int> OfferIds = new List<int>();
                                List<string> subcatenames = new List<string>();
                                List<string> catenames = new List<string>();
                                List<OfferItemsBillDiscount> offerItemsBillDiscounts = new List<OfferItemsBillDiscount>();
                                List<BillDiscountOfferSection> billDiscountOfferSections = new List<BillDiscountOfferSection>();

                                List<AngularJSAuthentication.Model.Store.StoreBrand> storeBrands = new List<AngularJSAuthentication.Model.Store.StoreBrand>();

                                //context.StoreBrandDB.Where(x => x.StoreId == res.StoreId && x.IsActive).ToList();

                                if (!string.IsNullOrEmpty(item.SelectStore))
                                {
                                    var ress = Store.FirstOrDefault(x => x.StoreName.Trim().ToLower() == item.SelectStore.Trim().ToLower());
                                    if (ress != null)
                                    {
                                        storeBrands = context.StoreBrandDB.Where(x => x.StoreId == ress.StoreId && x.IsActive).ToList();
                                        //var storeBrand = context.StoreBrandDB.Where(x => x.StoreId == res.StoreId && x.IsActive).ToList();
                                    }
                                }

                                var wh = WarehouseData.FirstOrDefault(x => x.WarehouseName.ToLower() == item.WarehouseName.ToLower());

                                if (IncludeItem.Status && IncludeItem.IncludeItemDC != null)
                                {
                                    var IncludeData = IncludeItem.IncludeItemDC.Where(x => x.Offer_Name == item.OfferName && x.WarehouseName == item.WarehouseName).ToList();
                                    if (IncludeData.Count > 0 && IncludeData.Any())
                                    {
                                        foreach (var include in IncludeData)
                                        {
                                            subcatenames.Add(include.Sub_Category);
                                            catenames.Add(include.Category);
                                            if (include.ItemMultiMRPId > 0 && item.OfferBy.ToLower() == "company")
                                            {
                                                var itemdata = itemmasters.Where(x => x.ItemMultiMRPId == include.ItemMultiMRPId).ToList();
                                                foreach (var it in itemdata)
                                                {
                                                    OfferItemsBillDiscount offerItemsBill = new OfferItemsBillDiscount
                                                    {
                                                        itemId = it.ItemId,
                                                        IsInclude = true
                                                    };
                                                    offerItemsBillDiscounts.Add(offerItemsBill);
                                                }
                                            }
                                            else if (item.OfferBy.ToLower() == "store")
                                            {
                                                //if (include.ItemMultiMRPId > 0)
                                                //{
                                                //    OfferItemsBillDiscount billDiscountOffer = new OfferItemsBillDiscount();
                                                //    billDiscountOffer.IsInclude = true;
                                                //    var IncludeItems = itemmasters.Where(x => x.ItemMultiMRPId == include.ItemMultiMRPId).Select(x => new { x.ItemId, x.ItemMultiMRPId, x.SubsubCategoryid, x.SubCategoryId, x.Categoryid, x.WarehouseId, x.itemname, x.MRP }).ToList();
                                                //    if (storeBrands != null && storeBrands.Any() && IncludeItems != null && IncludeItems.Any())
                                                //    {
                                                //        var storecatBrands = BrandCategoryMappings.Where(x => storeBrands.Select(y => y.BrandCategoryMappingId).Contains(x.BrandCategoryMappingId)).ToList();
                                                //        IncludeItems = IncludeItems.Where(y => storecatBrands.Select(x => x.Categoryid + "-" + x.SubCategoryId + "-" + x.SubsubcategoryId).Contains(y.Categoryid + "-" + y.SubCategoryId + "-" + y.SubsubCategoryid)).ToList();
                                                //    }

                                                //    if (IncludeItems != null && include.ItemMultiMRPId > 0 && IncludeItems.Any(x => x.ItemMultiMRPId == include.ItemMultiMRPId && x.WarehouseId == wh.WarehouseId))
                                                //    {
                                                //        var dbItems = IncludeItems.Where(x => x.ItemMultiMRPId == include.ItemMultiMRPId && x.WarehouseId == wh.WarehouseId).ToList();
                                                //        if (dbItems == null && !dbItems.Any())
                                                //        {
                                                //            ErrorMsg += (string.IsNullOrEmpty(ErrorMsg) ? "" : "\n") + "MultiMRPId" + include.ItemMultiMRPId + " not live on " + wh.WarehouseName + " in Include Item Sheet.";
                                                //        }
                                                //        else
                                                //        {

                                                //            billDiscountOffer.itemId = dbItems.FirstOrDefault().ItemId;
                                                //        }
                                                //    }
                                                //    else if (IncludeItems != null && include.ItemMultiMRPId > 0 && !IncludeItems.Any(x => x.ItemMultiMRPId == include.ItemMultiMRPId && x.WarehouseId == wh.WarehouseId))
                                                //    {
                                                //        ErrorMsg += (string.IsNullOrEmpty(ErrorMsg) ? "" : "\n") + "MultiMRPId" + include.ItemMultiMRPId + " not live on " + wh.WarehouseName + " in Include Item Sheet.";
                                                //        return ErrorMsg;
                                                //    }
                                                //    else
                                                //        billDiscountOffer.itemId = include.BrandCategoryMappingId > 0 ? include.BrandCategoryMappingId : include.SubCategoryMappingId;


                                                //    offerItemsBillDiscounts.Add(billDiscountOffer);
                                                //    //var itemdata = itemmasters.Where(x => x.ItemMultiMRPId == include.ItemMultiMRPId).ToList();
                                                //    //var itemdata =  itemmasters.Where(x => warehousesID.WarehouseId == x.WarehouseId && include.ItemMultiMRPId== x.ItemMultiMRPId ).Select(x => new { x.ItemId, x.ItemMultiMRPId, x.SubsubCategoryid, x.SubCategoryId, x.Categoryid, x.WarehouseId, x.itemname, x.MRP }).ToList() ;
                                                //    //if (itemdata != null && storeBrands.Any() && itemdata != null && itemdata.Any())
                                                //    //{
                                                //    //    var storecatBrands = BrandCategoryMappings.Where(x => storeBrands.Select(y => y.BrandCategoryMappingId).Contains(x.BrandCategoryMappingId)).ToList();
                                                //    //    itemdata = itemdata.Where(y => storecatBrands.Select(x => x.Categoryid + "-" + x.SubCategoryId + "-" + x.SubsubcategoryId).Contains(y.Categoryid + "-" + y.SubCategoryId + "-" + y.SubsubCategoryid)).ToList();
                                                //    //    foreach (var it in itemdata)
                                                //    //    {
                                                //    //        BillDiscountOfferSection offerItemsBill = new BillDiscountOfferSection
                                                //    //        {
                                                //    //            ObjId = it.ItemId,
                                                //    //            IsInclude = true
                                                //    //        };
                                                //    //        billDiscountOfferSections.Add(offerItemsBill);
                                                //    //    }
                                                //    //}
                                                //    //foreach (var it in itemdata)
                                                //    //{
                                                //    //    BillDiscountOfferSection offerItemsBill = new BillDiscountOfferSection
                                                //    //    {
                                                //    //        ObjId = it.ItemId,
                                                //    //        IsInclude = true
                                                //    //    };
                                                //    //    billDiscountOfferSections.Add(offerItemsBill);
                                                //    //}
                                                //}
                                                //else
                                                //{
                                                if (string.IsNullOrEmpty(include.BrandName) && !string.IsNullOrEmpty(include.Sub_Category))
                                                {
                                                    var SubCatId = SubcategoryData.FirstOrDefault(x => x.SubcategoryName == include.Sub_Category).SubCategoryId;
                                                    var subcateMappingId = CateSubCateMapping.FirstOrDefault(x => x.SubCategoryId == SubCatId).SubCategoryMappingId;
                                                    BillDiscountOfferSection billDiscountOffer = new BillDiscountOfferSection
                                                    {
                                                        ObjId = include.BrandCategoryMappingId > 0 ? include.BrandCategoryMappingId : include.SubCategoryMappingId,
                                                        IsInclude = true
                                                    };
                                                    billDiscountOfferSections.Add(billDiscountOffer);
                                                }
                                                else if (!string.IsNullOrEmpty(include.BrandName))
                                                {
                                                    var SubSubCateId = BrandData.FirstOrDefault(x => x.SubsubcategoryName == include.BrandName).SubsubCategoryid;
                                                    var brandMappingId = CateBrandMapping.FirstOrDefault(x => x.SubsubCategoryId == SubSubCateId).SubCategoryMappingId;
                                                    BillDiscountOfferSection billDiscountOffer = new BillDiscountOfferSection
                                                    {
                                                        ObjId = include.BrandCategoryMappingId > 0 ? include.BrandCategoryMappingId : include.SubCategoryMappingId,
                                                        IsInclude = true
                                                    };
                                                    billDiscountOfferSections.Add(billDiscountOffer);
                                                }
                                                //}

                                            }
                                        }
                                    }
                                }
                                if (ExcludeItem.Status && ExcludeItem.ExcludeItemDC != null)
                                {
                                    var ExcludeData = ExcludeItem.ExcludeItemDC.Where(x => x.Offer_Name == item.OfferName && x.WarehouseName == item.WarehouseName).ToList();
                                    if (ExcludeData.Count > 0 && ExcludeData.Any())
                                    {
                                        foreach (var exclude in ExcludeData)
                                        {
                                            subcatenames.Add(exclude.Sub_Category);
                                            catenames.Add(exclude.Category);
                                            if (item.OfferBy.Trim().ToLower() == "company" && item.BillDiscountType.ToLower() == "billdiscount")
                                            {
                                                var Catid = categoryData.FirstOrDefault(x => x.CategoryName == exclude.Category).Categoryid;
                                                BillDiscountOfferSection billDiscountOffer = new BillDiscountOfferSection
                                                {
                                                    ObjId = Catid,
                                                    IsInclude = false
                                                };
                                                billDiscountOfferSections.Add(billDiscountOffer);
                                            }
                                            else if (item.OfferBy.Trim().ToLower() == "store" && exclude.ItemMultiMRPId > 0)
                                            {
                                                var itemdata = itemmasters.Where(x => x.ItemMultiMRPId == exclude.ItemMultiMRPId).ToList();
                                                foreach (var it in itemdata)
                                                {
                                                    OfferItemsBillDiscount offerItemsBill = new OfferItemsBillDiscount
                                                    {
                                                        itemId = it.ItemId,
                                                        IsInclude = false
                                                    };
                                                    offerItemsBillDiscounts.Add(offerItemsBill);
                                                }
                                            }

                                            //if (item.BillDiscountType.ToLower() == "billdiscount" && !string.IsNullOrEmpty(exclude.Category))
                                            //{
                                            //    var Catid = categoryData.FirstOrDefault(x => x.CategoryName == exclude.Category).Categoryid;
                                            //    BillDiscountOfferSection billDiscountOffer = new BillDiscountOfferSection
                                            //    {
                                            //        ObjId = Catid,
                                            //        IsInclude = false
                                            //    };
                                            //    billDiscountOfferSections.Add(billDiscountOffer);
                                            //}
                                            //else if (exclude.ItemMultiMRPId > 0 && item.OfferBy.ToLower() == "company")
                                            //{
                                            //    var itemdata = itemmasters.Where(x => x.ItemMultiMRPId == exclude.ItemMultiMRPId).ToList();
                                            //    foreach (var it in itemdata)
                                            //    {
                                            //        OfferItemsBillDiscount offerItemsBill = new OfferItemsBillDiscount
                                            //        {
                                            //            itemId = it.ItemId,
                                            //            IsInclude = false
                                            //        };
                                            //        offerItemsBillDiscounts.Add(offerItemsBill);
                                            //    }
                                            //}
                                            //else if (item.OfferBy.ToLower() == "store")
                                            //{
                                            //    if (string.IsNullOrEmpty(exclude.BrandName) && !string.IsNullOrEmpty(exclude.Sub_Category) && item.BillDiscountType.ToLower() == "subcategory")
                                            //    {
                                            //        var SubCatId = SubcategoryData.FirstOrDefault(x => x.SubcategoryName == exclude.Sub_Category).SubCategoryId;
                                            //        var subcateMappingId = CateSubCateMapping.FirstOrDefault(x => x.SubCategoryId == SubCatId).SubCategoryMappingId;
                                            //        BillDiscountOfferSection billDiscountOffer = new BillDiscountOfferSection
                                            //        {
                                            //            ObjId = subcateMappingId,
                                            //            IsInclude = false
                                            //        };
                                            //        billDiscountOfferSections.Add(billDiscountOffer);
                                            //    }
                                            //    else if (!string.IsNullOrEmpty(exclude.BrandName) && item.BillDiscountType.ToLower() == "brand")
                                            //    {
                                            //        var SubSubCateId = BrandData.FirstOrDefault(x => x.SubsubcategoryName == exclude.BrandName).SubsubCategoryid;
                                            //        var brandMappingId = CateBrandMapping.FirstOrDefault(x => x.SubsubCategoryId == SubSubCateId).SubCategoryMappingId;
                                            //        BillDiscountOfferSection billDiscountOffer = new BillDiscountOfferSection
                                            //        {
                                            //            ObjId = brandMappingId,
                                            //            IsInclude = false
                                            //        };
                                            //        billDiscountOfferSections.Add(billDiscountOffer);
                                            //    }
                                            //}
                                        }
                                    }
                                }
                                List<BillDiscountFreeItem> billDiscountFreeItemList = new List<BillDiscountFreeItem>();
                                if (FreeItem.FreeitemDC != null && FreeItem.FreeitemDC.Count > 0 && item.DiscountOn.ToLower() == "freeitem")
                                {
                                    foreach (var x in FreeItem.FreeitemDC)
                                    {
                                        if (x.Offer_Name == item.OfferName && x.WarehouseName == item.WarehouseName)
                                        {
                                            var itemmaster = itemmasters.Where(y => y.ItemMultiMRPId == x.ItemMultiMRPId && y.WarehouseName == x.WarehouseName && y.SellingSku == x.SellingSku).ToList();
                                            foreach (var it in itemmaster)
                                            {
                                                int stockqty = 0;
                                                var stockdata = freeItemStockDcs.FirstOrDefault(y => y.ItemMultiMRPId == x.ItemMultiMRPId && y.WarehouseId == it.WarehouseId && y.StockType.ToLower() == x.Stock_Hit.ToLower());
                                                if (stockdata != null)
                                                {
                                                    stockqty = stockdata.Stock;
                                                }
                                                var billDiscountFreeItem = new BillDiscountFreeItem
                                                {
                                                    ItemId = it.ItemId,
                                                    ItemMultiMrpId = x.ItemMultiMRPId,
                                                    MRP = it.MRP,
                                                    ItemName = it.itemname,
                                                    StockQty = stockqty,
                                                    OfferStockQty = stockqty,
                                                    Qty = x.Freeitem_Qty,
                                                    StockType = x.Stock_Hit.ToLower() == "free stock" ? 2 : 1,
                                                    RemainingOfferStockQty = 0
                                                };
                                                billDiscountFreeItemList.Add(billDiscountFreeItem);
                                            }
                                        }
                                    };
                                }

                                List<OfferBillDiscountRequiredItem> offerBillDiscountRequiredItems = new List<OfferBillDiscountRequiredItem>();
                                if (Mandatory.MandatoryDC != null && Mandatory.MandatoryDC.Any())
                                {
                                    var MandatoryItemdata = Mandatory.MandatoryDC.Where(x => x.Offer_Name == item.OfferName && x.WarehouseName == item.WarehouseName).ToList();
                                    ParallelLoopResult parellelResult = Parallel.ForEach(MandatoryItemdata.GroupBy(x => x.ItemRequiredOn), (it) =>
                                    {

                                        var ValueType = it.Select(x => x.ValueType).Distinct().ToList();
                                        foreach (var valuetype in ValueType)
                                        {
                                            var qty = it.Where(x => x.ValueType == valuetype).Select(x => x.Qty).Distinct().ToList();
                                            foreach (var qt in qty)
                                            {
                                                List<int> MultiMrp = new List<int>();
                                                List<string> ItemNames = new List<string>();
                                                if (it.FirstOrDefault().ItemRequiredOn.ToLower() == "item")
                                                {
                                                    MultiMrp = it.Where(x => x.Qty == qt).Select(x => x.ItemMultiMRPId).Distinct().ToList();
                                                    ItemNames = itemmasters.Where(x => MultiMrp.Contains(x.ItemMultiMRPId) && x.WarehouseName == item.WarehouseName).Select(x => x.itemname).ToList();
                                                }
                                                OfferBillDiscountRequiredItem reqitem = new OfferBillDiscountRequiredItem
                                                {
                                                    ObjectId = it.FirstOrDefault().ItemRequiredOn.ToLower() == "item" ? string.Join(",", MultiMrp) : it.FirstOrDefault().BrandId.ToString(),
                                                    ObjectText = it.FirstOrDefault().ItemRequiredOn.ToLower() == "item" ? string.Join(",", ItemNames) : it.FirstOrDefault().BrandName,
                                                    ObjectType = it.FirstOrDefault().ItemRequiredOn.ToLower() == "item" ? "Item" : "brand",
                                                    ObjectValue = qt,
                                                    offerId = 0,
                                                    ValueType = valuetype.ToTitleCase(),
                                                    OfferName = item.OfferName
                                                };
                                                offerBillDiscountRequiredItems.Add(reqitem);
                                            }
                                        }


                                    });
                                }




                                var subCategoryids = SubcategoryData.Where(x => subcatenames.Contains(x.SubcategoryName)).Select(x => x.SubCategoryId).ToList();
                                var Categoryids = categoryData.Where(x => catenames.Contains(x.CategoryName)).Select(x => x.Categoryid).ToList();
                                var SubcateMappingids = CateSubCateMapping.Where(x => subCategoryids.Contains(x.SubCategoryId) && Categoryids.Contains(x.Categoryid)).Select(x => x.SubCategoryMappingId).ToList();

                                var warehouseitem = new List<ItemMaster>();
                                //if ((item.OfferOn == "BillDiscount")) // && !item.SkipValidation
                                {
                                    var predicate = PredicateBuilder.True<Offer>();
                                    predicate = predicate.And(x => x.IsDeleted == false && x.IsActive == true && x.BillAmount == item.BillAmountMinLimit && x.MaxDiscount == item.BillAmountMaxLimit && x.WarehouseId == warehousesID.WarehouseId && x.OfferOn == "BillDiscount" && x.BillDiscountType == item.BillDiscountType && x.start <= item.StartDateTime && x.end >= item.EndDateTime);

                                    if (item.BillDiscountType.ToLower() == "category")
                                    {
                                        predicate = predicate.And(x => x.BillDiscountOfferSections.Any(y => CategoryIds.Contains(y.ObjId)));
                                    }
                                    else if (item.BillDiscountType.ToLower() == "store")
                                    {
                                        //var Categoryids = offer.BillDiscountOfferSections.Select(x => x.ObjId);
                                        predicate = predicate.And(x => x.BillDiscountOfferSections.Any(y => CategoryIds.Contains(y.ObjId)));
                                    }
                                    else if (item.BillDiscountType.ToLower() == "subcategory")
                                    {
                                        //var SubCategoryMappingids = offer.BillDiscountOfferSections.Select(x => x.ObjId);
                                        predicate = predicate.And(x => x.BillDiscountOfferSections.Any(y => SubcateMappingids.Contains(y.ObjId)));
                                    }
                                    else if (item.BillDiscountType.ToLower() == "brand")
                                    {
                                        var BrandMappingids = CateBrandMapping.Where(x => SubcateMappingids.Contains(x.BrandCategoryMappingId)).Select(x => x.BrandCategoryMappingId).ToList();
                                        //var BrandMappingids = offer.BillDiscountOfferSections.Select(x => x.ObjId);
                                        predicate = predicate.And(x => x.BillDiscountOfferSections.Any(y => BrandMappingids.Contains(y.ObjId)));
                                    }

                                    if (item.DiscountOn.ToLower() == "percentage")
                                    {
                                        predicate = predicate.And(x => x.BillDiscountOfferOn == "Percentage");
                                    }
                                    else if (item.DiscountOn.ToLower() == "freeitem")
                                    {
                                        if (!FreeItem.Status && !FreeItem.FreeitemDC.Any(x => x.WarehouseName == item.WarehouseName && x.Offer_Name == item.OfferName))
                                        {
                                            //offerResponseDC.status = false;
                                            //offerResponseDC.msg += (string.IsNullOrEmpty(offerResponseDC.msg) ? "" : "\n") + "Please add atleast on free item.";
                                            //offerResponseDC.Offer = offer;
                                        }
                                    }
                                    else
                                    {
                                        predicate = predicate.And(x => x.BillDiscountOfferOn == "WalletPoint");
                                    }
                                    OfferIds = context.OfferDb.Where(predicate).Select(x => x.OfferId).ToList();

                                    if (OfferIds.Any())
                                    {
                                        //offerResponseDC.status = false;
                                        //offerResponseDC.ShowValidationSkipmsg = true;
                                        //offerResponseDC.msg += (string.IsNullOrEmpty(offerResponseDC.msg) ? "" : "\n") + "This " + offer.BillDiscountType + " Offer already exist for " + warehouse.WarehouseName + ". Please first inactive previous offer.";
                                        //offerResponseDC.Offer = offer;
                                    }
                                }
                                if ((item.OfferOn == "BillDiscount") && OfferIds != null)  // && !offer.SkipValidation
                                {
                                    if (item.DiscountOn != "FreeItem")
                                    {
                                        //if (item.CustomerId > 0)
                                        //{
                                        //    offer.ApplyType = "Customer";
                                        //    bool exists = OfferIds.Any() ? context.BillDiscountDb.Any(x => x.CustomerId == offer.CustomerId && x.BillDiscountType == offer.OfferOn && OfferIds.Contains(x.OfferId)) : false;
                                        //    if (exists)
                                        //    {
                                        //        Isexists = true;
                                        //    }
                                        //}
                                        //    if (offer.GroupId > 0)
                                        //    {
                                        //        offer.ApplyType = "Group";
                                        //        var groupcustomer = context.GroupMappings.Where(x => x.GroupID == offer.GroupId && x.WarehouseID == warehouse.WarehouseId).Select(x => x.CustomerID);
                                        //        bool exists = false;
                                        //        if (groupcustomer != null && groupcustomer.Any())
                                        //        {
                                        //            exists = OfferIds.Any() ? context.BillDiscountDb.Any(x => groupcustomer.Contains(x.CustomerId) && x.BillDiscountType == offer.OfferOn && OfferIds.Contains(x.OfferId)) : false;
                                        //        }
                                        //        if (exists)
                                        //        {
                                        //            Isexists = true;
                                        //        }

                                        //    }
                                        //    else if (offer.CustomerId == -1)
                                        //    {
                                        //        offer.ApplyType = "Warehouse";
                                        //    }
                                        //    else if (offer.CustomerId == -2)
                                        //    {
                                        //        offer.ApplyType = "KPPCustomer";
                                        //        var groupcustomer = context.Customers.Where(x => x.Warehouseid.Value == warehouse.WarehouseId && x.IsKPP).Select(x => x.CustomerId).ToList();
                                        //        bool exists = false;
                                        //        if (groupcustomer != null && groupcustomer.Any())
                                        //        {
                                        //            exists = OfferIds.Any() ? context.BillDiscountDb.Any(x => groupcustomer.Contains(x.CustomerId) && x.BillDiscountType == offer.OfferOn && OfferIds.Contains(x.OfferId)) : false;
                                        //        }
                                        //        if (exists)
                                        //        {
                                        //            Isexists = true;
                                        //        }

                                        //    }
                                        //    else if (offer.CustomerId < -2)
                                        //    {
                                        //        offer.ApplyType = "Level";
                                        //        Isexists = true;
                                        //    }
                                        //}
                                        //if (Isexists)
                                        //{
                                        //    offerResponseDC.status = false;
                                        //    offerResponseDC.ShowValidationSkipmsg = true;
                                        //    // offerResponseDC.msg += "This " + offer.BillDiscountType + "-" + offer.ApplyType + " Offer already exist for " + warehouse.WarehouseName + ". Please first inactive previous offer.";
                                        //    offerResponseDC.Offer = offer;
                                    }
                                }


                                string ApplyType = "";
                                if (item.Usertype.ToLower() == "customer")
                                {
                                    ApplyType = "Customer";
                                }
                                else if (item.Usertype.ToLower() == "group")
                                {
                                    ApplyType = "Group";
                                }
                                else if (item.Usertype.ToLower() == "all customer")
                                {
                                    ApplyType = "Warehouse";
                                }
                                else if (item.Usertype.ToLower() == "kpp customer")
                                {
                                    ApplyType = "KPPCustomer";
                                }
                                else if (item.Usertype.ToLower() == "customer level")
                                {
                                    ApplyType = "Level";
                                }
                                else
                                {
                                    ErrorMsg = "Please Enter Correct Customer Type";
                                    return ErrorMsg;
                                }

                                if (item.SelectAppType.ToLower() == "both")
                                {
                                    item.SelectAppType = "Both";
                                }
                                else if (item.SelectAppType.ToLower() == "retailer app")
                                {
                                    item.SelectAppType = "Retailer App";
                                }
                                else if (item.SelectAppType.ToLower() == "sales app")
                                {
                                    item.SelectAppType = "Sales App";
                                }
                                else if (item.SelectAppType.ToLower() == "distributor app")
                                {
                                    item.SelectAppType = "Distributor App";
                                }
                                else
                                {
                                    ErrorMsg = "Please Enter Correct App Type";
                                    return ErrorMsg;
                                }



                                if (item.OfferBy.ToTitleCase() != "Company")
                                {
                                    if (string.IsNullOrEmpty(item.SelectStore))
                                    {
                                        ErrorMsg += " Select store is mandatory for Store Selection";
                                        return ErrorMsg;
                                    }
                                    else
                                    {
                                        var data = Store.FirstOrDefault(x => x.StoreName.ToLower() == item.SelectStore.ToLower());
                                        if (data == null)
                                        {
                                            ErrorMsg += " " + item.SelectStore + " is not Present in Our Database";
                                            return ErrorMsg;
                                        }
                                    }
                                }

                                var useotheroffer = item.UseOtherOffer;
                                var multiuseoffer = item.MultiTimeUse;
                                var useotheroffercount = item.OfferUseCount;
                                DateTime startdarte = item.StartDateTime;
                                DateTime enddarte = item.EndDateTime;


                                Offer Newoffer = new Offer
                                {
                                    IsCRMOffer = false,
                                    CompanyId = compid,
                                    userid = userid,
                                    StoreId = item.OfferBy.ToTitleCase() == "Company" ? 0 : Store.FirstOrDefault(x => x.StoreName.Trim().ToLower() == item.SelectStore.Trim().ToLower()).StoreId > 0 ? Store.FirstOrDefault(x => x.StoreName.Trim().ToLower() == item.SelectStore.Trim().ToLower()).StoreId : 0,
                                    WarehouseId = WarehouseData.FirstOrDefault(x => x.WarehouseName == item.WarehouseName).WarehouseId,
                                    CityId = WarehouseData.FirstOrDefault(x => x.WarehouseName == item.WarehouseName).Cityid,
                                    start = item.StartDateTime,
                                    end = item.EndDateTime,
                                    itemId = 0,
                                    itemname = "",
                                    IsDeleted = false,
                                    IsActive = true,
                                    OfferLogoUrl = "",
                                    ApplyType = ApplyType,
                                    CreatedDate = indianTime,
                                    UpdateDate = indianTime,
                                    //OfferCode = item.OfferCode.ToString(),
                                    Description = item.Description,
                                    DiscountPercentage = item.DiscountOn.ToLower() == "percentage" ? item.Percentage : 0,
                                    OfferName = item.OfferName,
                                    OfferWithOtherOffer = true,
                                    BillDiscountOfferOn = item.DiscountOn,//.ToTitleCase(),
                                    BillDiscountWallet = item.WalletPoint,
                                    IsMultiTimeUse = item.MultiTimeUse,
                                    IsUseOtherOffer = item.UseOtherOffer,
                                    OfferOn = item.OfferOn,
                                    FreeOfferType = "",
                                    FreeItemLimit = null,
                                    OfferUseCount = item.OfferUseCount,
                                    ApplyOn = item.ApplyOn,
                                    OfferAppType = item.SelectAppType,
                                    OffersaleQty = 0,
                                    Category = 0,
                                    subCategory = 0,
                                    subSubCategory = 0,
                                    WalletType = item.WalletType,
                                    BillAmount = item.BillAmountMinLimit,
                                    BillDiscountFreeItems = billDiscountFreeItemList,
                                    BillDiscountType = item.BillDiscountType,
                                    DistributorDiscountAmount = 0,
                                    DistributorDiscountPercentage = 0,
                                    DistributorOfferType = false,
                                    FreeItemId = 0,
                                    FreeItemMRP = 0,
                                    FreeItemName = "",
                                    FreeWalletPoint = 0, //item.WalletPoint,
                                    GroupId = item.Usertype.ToLower() == "group" ? (int)GroupList.FirstOrDefault(x => x.GroupName == item.GroupLevelName)?.Id : 0,
                                    IsDispatchedFreeStock = FreeItem.FreeitemDC != null ? FreeItem.FreeitemDC.Any(x => x.Offer_Name == item.OfferName && x.WarehouseName == item.WarehouseName && x.Stock_Hit == "FreeStock") ? true : false : false,
                                    IsOfferOnCart = false,
                                    ItemNumber = "",
                                    LineItem = item.NumberOfLineItem,
                                    MaxBillAmount = item.BillAmountMaxLimit,
                                    MaxDiscount = item.MaximumDiscount,
                                    MaxQtyPersonCanTake = 0,
                                    MinOrderQuantity = 0,
                                    NoOffreeQuantity = 0,
                                    OfferCategory = "",
                                    IsAutoApply = item.AutoApply,
                                    ImagePath = "",
                                    IsPriorityOffer = true,
                                    ExcludeGroupId = !string.IsNullOrEmpty(item.ExcludeUserGroup) ? (int)GroupList.FirstOrDefault(x => x.GroupName == item.ExcludeUserGroup)?.Id : 0,
                                    CombinedGroupId = !string.IsNullOrEmpty(item.CombinedOfferGroup) ? CombinedGroupList.Where(x => x.Name == item.CombinedOfferGroup).Select(x => x.Id).FirstOrDefault() : 0,
                                    OfferVolume = "",
                                    QtyAvaiable = 0,
                                    QtyConsumed = 0,
                                    Level = item.Usertype.ToLower() == "customer level" ? item.GroupLevelName : "",
                                    OfferItemsBillDiscounts = offerItemsBillDiscounts,
                                    BillDiscountOfferSections = billDiscountOfferSections,
                                    OfferBillDiscountRequiredItems = offerBillDiscountRequiredItems.Where(x => x.OfferName == item.OfferName).ToList(),
                                    OfferLineItemValues = new List<OfferLineItemValue>(),
                                };

                                offers.Add(Newoffer);
                            }


                            context.OfferDb.AddRange(offers);
                            if (context.Commit() > 0)
                            {
                                var offerbilldiscountitems = new List<OfferItemsBillDiscount>();
                                List<BillDiscountOfferSection> billDiscountOfferSections = new List<BillDiscountOfferSection>();
                                string[] saAllowedCharacters = { "1", "2", "3", "4", "5", "6", "7", "8", "9", "0" };
                                string Ocode = GenerateRandomCRM(6, saAllowedCharacters);


                                foreach (var offerdb in offers)
                                {

                                    if (offerdb.ScratchCardCustomers != null && offerdb.ScratchCardCustomers.Any() && offerdb.BillDiscountOfferOn == "DynamicAmount")
                                    {
                                        var skcodes = offerdb.ScratchCardCustomers.Select(x => x.SkCode).Distinct().ToList();
                                        var customers = context.Customers.Where(x => skcodes.Contains(x.Skcode)).Select(x => new { x.Skcode, x.CustomerId, x.Warehouseid }).ToList();
                                        offerdb.ScratchCardCustomers.ForEach(x =>
                                        {
                                            if (customers.Any(y => y.Skcode == x.SkCode))
                                            {
                                                var cust = customers.FirstOrDefault(y => y.Skcode == x.SkCode);
                                                x.CustomerId = cust.CustomerId;
                                                x.WarehouseId = cust.Warehouseid.Value;
                                            }
                                        });
                                    }
                                    List<KeyValuePair<int, int>> items = new List<KeyValuePair<int, int>>();
                                    if (offerdb.OfferItemsBillDiscounts != null && offerdb.OfferItemsBillDiscounts.Any())
                                    {
                                        var billDiscountItemids = offerdb.OfferItemsBillDiscounts.Select(x => x.itemId).ToList();
                                        var itemMultiMrpIds = context.itemMasters.Where(x => billDiscountItemids.Contains(x.ItemId)).Select(x => x.ItemMultiMRPId).ToList();
                                        var Dbitems = context.itemMasters.Where(x => itemMultiMrpIds.Contains(x.ItemMultiMRPId) && offerdb.WarehouseId == x.WarehouseId).Select(x => new { x.ItemId, x.WarehouseId }).ToList();
                                        foreach (var item in Dbitems)
                                        {
                                            items.Add(new KeyValuePair<int, int>(item.WarehouseId, item.ItemId));
                                        }
                                    }

                                    string code = "";
                                    if (offerdb.OfferOn == "BillDiscount")
                                    {
                                        code = "BD_";
                                    }
                                    else if (offerdb.BillDiscountType == "ClearanceStock")
                                    {
                                        code = "CL_";
                                    }
                                    if (string.IsNullOrEmpty(offerdb.OfferCode) && offerdb.IsCRMOffer == false)
                                    {
                                        string offerCode = code + offerdb.OfferId;
                                        offerdb.OfferCode = offerCode;
                                    }

                                    //if (items != null && items.Any(x => x.Key == offerdb.WarehouseId))
                                    //{
                                    //    foreach (var item in items.Where(x => x.Key == offerdb.WarehouseId))
                                    //    {
                                    //        offerbilldiscountitems.Add(new OfferItemsBillDiscount
                                    //        {
                                    //            OfferId = offerdb.OfferId,
                                    //            itemId = item.Value,
                                    //            IsInclude = offerdb.BillDiscountType == "items"
                                    //        });
                                    //    }
                                    //}

                                    //if (offerdb.BillDiscountOfferSections != null && offerdb.BillDiscountOfferSections.Any())
                                    //{
                                    //    foreach (var item in offerdb.BillDiscountOfferSections)
                                    //    {
                                    //        billDiscountOfferSections.Add(new BillDiscountOfferSection
                                    //        {
                                    //            IsInclude = item.IsInclude,
                                    //            ObjId = item.ObjId,
                                    //            OfferId = offerdb.OfferId
                                    //        });
                                    //    }

                                    //}

                                    offerdb.IncentiveClassification = offerdb.IncentiveClassification;// by Sudhir 14-06-2023


                                    if (offerdb.OfferOn == "BillDiscount")
                                    {
                                        if (offerdb.OfferId > 0 && offerdb.GroupId > 0)
                                        {
                                            // List<GroupMapping> groupmapp = context.GroupMappings.Where(x => x.GroupID == offerdb.GroupId && x.WarehouseID == offerdb.WarehouseId).ToList();
                                            string query2 = "select distinct a.CustomerID from SalesGroupCustomers a with(nolock) inner join Customers c with(nolock) on a.CustomerID=c.CustomerId and a.IsActive=1 and isnull(a.IsDeleted,0)=0 and a.GroupId=" + offerdb.GroupId + " and c.Warehouseid=" + offerdb.WarehouseId;
                                            List<int> groupmapp = context.Database.SqlQuery<int>(query2).ToList();
                                            if (groupmapp != null && groupmapp.Count > 0)
                                            {
                                                double billAmount = 0;
                                                WeightedRandomBag<int> itemDrops = new WeightedRandomBag<int>();

                                                var customerids = groupmapp.ToList();
                                                List<BillDiscount> customerdetails = context.BillDiscountDb.Where(x => customerids.Contains(x.CustomerId) && x.OfferId == offerdb.OfferId).ToList();
                                                List<BillDiscount> newbilldiscount = new List<BillDiscount>();
                                                foreach (var custdata in groupmapp)
                                                {
                                                    billAmount = 0;
                                                    if (offerdb.OfferOn == "DynamicWalletPoint")
                                                    {
                                                        billAmount = itemDrops.GetRandom();
                                                    }
                                                    var customerdetail = customerdetails.FirstOrDefault(x => x.CustomerId == custdata);
                                                    if (customerdetail == null)
                                                    {
                                                        BillDiscount billDiscount = new BillDiscount();
                                                        billDiscount.CustomerId = custdata;
                                                        billDiscount.OrderId = 0;
                                                        billDiscount.OfferId = offerdb.OfferId;
                                                        billDiscount.BillDiscountType = offerdb.OfferOn;
                                                        billDiscount.BillDiscountAmount = 0;
                                                        billDiscount.IsMultiTimeUse = offerdb.IsMultiTimeUse;
                                                        billDiscount.IsUseOtherOffer = offerdb.IsUseOtherOffer;
                                                        billDiscount.CreatedDate = indianTime;
                                                        billDiscount.ModifiedDate = indianTime;
                                                        billDiscount.IsActive = offerdb.IsActive;
                                                        billDiscount.IsDeleted = false;
                                                        billDiscount.CreatedBy = offerdb.userid;
                                                        billDiscount.ModifiedBy = offerdb.userid;
                                                        billDiscount.IsScratchBDCode = false;//scratched or not
                                                        newbilldiscount.Add(billDiscount);
                                                    }
                                                }

                                                if (newbilldiscount != null && newbilldiscount.Any())
                                                {
                                                    var BillDiscountsCustomers = new BulkOperations();
                                                    BillDiscountsCustomers.Setup<BillDiscount>(x => x.ForCollection(newbilldiscount))
                                                        .WithTable("BillDiscounts")
                                                        .WithBulkCopyBatchSize(4000)
                                                        .WithBulkCopyCommandTimeout(720) // Default is 600 seconds
                                                        .WithSqlCommandTimeout(720) // Default is 600 seconds
                                                        .AddAllColumns()
                                                        .BulkInsert();
                                                    BillDiscountsCustomers.CommitTransaction("AuthContext");
                                                }

                                                // context.BillDiscountDb.AddRange(newbilldiscount);
                                            }
                                        }
                                        else if (offerdb.OfferId > 0 && offerdb.CustomerId == -9)
                                        {
                                            string query3 = "Select a.CustomerId from Primecustomers a inner join Customers b on a.CustomerId=b.CustomerId and a.IsActive=1 and b.Active=1 and a.IsDeleted=0 and b.Warehouseid=" + offerdb.WarehouseId;
                                            List<int> customerids = context.Database.SqlQuery<int>(query3).ToList();
                                            double billAmount = 0;
                                            WeightedRandomBag<int> itemDrops = new WeightedRandomBag<int>();
                                            if (offerdb.BillDiscountOfferOn == "DynamicWalletPoint")
                                            {
                                                foreach (var item in offerdb.OfferScratchWeights)
                                                {
                                                    itemDrops.AddEntry(item.WalletPoint, item.Weight);
                                                }
                                            }
                                            foreach (var item in customerids)
                                            {
                                                billAmount = 0;
                                                if (offerdb.BillDiscountOfferOn == "DynamicWalletPoint")
                                                {
                                                    billAmount = itemDrops.GetRandom();
                                                }
                                                BillDiscount billDiscount = new BillDiscount();
                                                billDiscount.CustomerId = item;
                                                billDiscount.OrderId = 0;
                                                billDiscount.OfferId = offerdb.OfferId;
                                                billDiscount.BillDiscountType = offerdb.OfferOn;
                                                billDiscount.BillDiscountTypeValue = billAmount;
                                                billDiscount.BillDiscountAmount = 0;
                                                billDiscount.IsMultiTimeUse = offerdb.IsMultiTimeUse;
                                                billDiscount.IsUseOtherOffer = offerdb.IsUseOtherOffer;
                                                billDiscount.CreatedDate = indianTime;
                                                billDiscount.ModifiedDate = indianTime;
                                                billDiscount.IsActive = offerdb.IsActive;
                                                billDiscount.IsDeleted = false;
                                                billDiscount.CreatedBy = offerdb.userid;
                                                billDiscount.ModifiedBy = offerdb.userid;
                                                billDiscount.IsScratchBDCode = false;//scratched or not
                                                billDiscount.Category = offerdb.Category;
                                                billDiscount.Subcategory = offerdb.subCategory;
                                                billDiscount.subSubcategory = offerdb.subSubCategory;
                                                context.BillDiscountDb.Add(billDiscount);
                                            }
                                        }
                                        else if (offerdb.ApplyType == "Level")
                                        {

                                            int Level = Convert.ToInt32(offerdb.Level);
                                            //switch (offerdb.Level)
                                            //{
                                            //    case "0":
                                            //        Level = 0;
                                            //        break;
                                            //    case "1":
                                            //        Level = 1;
                                            //        break;
                                            //    case "2":
                                            //        Level = 2;
                                            //        break;
                                            //    case "4":
                                            //        Level = 3;
                                            //        break;
                                            //    case 4:
                                            //        Level = 4;
                                            //        break;
                                            //    case 5:
                                            //        Level = 5;
                                            //        break;
                                            //}

                                            var fromdate = DateTime.Now;

                                            fromdate = DateTime.Now.AddMonths(-1);
                                            string query1 = "Select distinct a.CustomerId from CRMCustomerLevels a with(nolock)  inner join Customers b  with(nolock)  on a.CustomerId=b.CustomerId and IsDeleted=0 and b.Warehouseid=" + offerdb.WarehouseId + " and a.Month=" + fromdate.Month + " and a.Year=" + fromdate.Year + " And a.Level=" + Level;
                                            List<int> customerids = context.Database.SqlQuery<int>(query1).ToList();


                                            double billAmount = 0;
                                            WeightedRandomBag<int> itemDrops = new WeightedRandomBag<int>();
                                            if (offerdb.BillDiscountOfferOn == "DynamicWalletPoint")
                                            {
                                                foreach (var item in offerdb.OfferScratchWeights)
                                                {
                                                    itemDrops.AddEntry(item.WalletPoint, item.Weight);
                                                }
                                            }
                                            List<BillDiscount> BillDiscounts = new List<BillDiscount>();
                                            foreach (var item in customerids)
                                            {
                                                billAmount = 0;
                                                if (offerdb.BillDiscountOfferOn == "DynamicWalletPoint")
                                                {
                                                    billAmount = itemDrops.GetRandom();
                                                }
                                                BillDiscount billDiscount = new BillDiscount();
                                                billDiscount.CustomerId = item;
                                                billDiscount.OrderId = 0;
                                                billDiscount.OfferId = offerdb.OfferId;
                                                billDiscount.BillDiscountType = offerdb.OfferOn;
                                                billDiscount.BillDiscountTypeValue = billAmount;
                                                billDiscount.BillDiscountAmount = 0;
                                                billDiscount.IsMultiTimeUse = offerdb.IsMultiTimeUse;
                                                billDiscount.IsUseOtherOffer = offerdb.IsUseOtherOffer;
                                                billDiscount.CreatedDate = indianTime;
                                                billDiscount.ModifiedDate = indianTime;
                                                billDiscount.IsActive = offerdb.IsActive;
                                                billDiscount.IsDeleted = false;
                                                billDiscount.CreatedBy = offerdb.userid;
                                                billDiscount.ModifiedBy = offerdb.userid;
                                                billDiscount.IsScratchBDCode = false;//scratched or not
                                                billDiscount.Category = offerdb.Category;
                                                billDiscount.Subcategory = offerdb.subCategory;
                                                billDiscount.subSubcategory = offerdb.subSubCategory;
                                                BillDiscounts.Add(billDiscount);
                                            }

                                            if (BillDiscounts != null && BillDiscounts.Any())
                                            {
                                                var BillDiscountsCustomers = new BulkOperations();
                                                BillDiscountsCustomers.Setup<BillDiscount>(x => x.ForCollection(BillDiscounts))
                                                    .WithTable("BillDiscounts")
                                                    .WithBulkCopyBatchSize(4000)
                                                    .WithBulkCopyCommandTimeout(720) // Default is 600 seconds
                                                    .WithSqlCommandTimeout(720) // Default is 600 seconds
                                                    .AddAllColumns()
                                                    .BulkInsert();
                                                BillDiscountsCustomers.CommitTransaction("AuthContext");
                                            }

                                        }

                                    }
                                    #region itemMaster
                                    List<ItemMaster> itemnumber = new List<ItemMaster>();

                                    context.Entry(offerdb).State = EntityState.Modified;
                                    #endregion
                                }
                                if (offerbilldiscountitems.Any())
                                    context.OfferItemsBillDiscountDB.AddRange(offerbilldiscountitems);
                                if (billDiscountOfferSections.Any())
                                    context.BillDiscountOfferSectionDB.AddRange(billDiscountOfferSections);
                                if (context.Commit() > 0)
                                    return "Offer Created Successfully";
                                else
                                {
                                    foreach (var offerdb in offers)
                                    {
                                        offerdb.IsActive = false;
                                        offerdb.IsDeleted = true;
                                        context.Entry(offerdb).State = EntityState.Modified;
                                    }
                                    context.Commit();
                                    return "Some Error Occured";
                                }
                            }
                        }
                    }

                }
                catch (Exception ex)
                {
                    logger.Error("Error offer Upload File " + "\n\n" + ex.Message + "\n\n" + ex.InnerException + "\n\n" + ex.StackTrace);

                    ErrorMsg += "Error please try after some time.";
                }
                return ErrorMsg;

            }

        }

        public string ValidateSheet(ISheet sheet, List<OfferColumn> Columns)
        {
            string errorMsg = string.Empty;
            IRow rowData;
            ICell cellData = null;
            for (int iRowIdx = 0; iRowIdx <= sheet.LastRowNum; iRowIdx++)
            {
                rowData = sheet.GetRow(iRowIdx);
                if (iRowIdx == 0)
                {
                    if (rowData != null)
                    {
                        foreach (var col in Columns.Select(x => x.ColumnName))
                        {
                            if (!rowData.Cells.Any(x => x.ToString().Trim() == col))
                            {
                                errorMsg += (string.IsNullOrEmpty(errorMsg) ? "" : "\n") + col + " Does not exist in " + sheet.SheetName + " Sheet";
                            }
                        }
                    }
                }
                else
                {
                    for (int j = 0; j < Columns.Count(); j++)
                    {
                        var col = Columns[j];
                        cellData = rowData.GetCell(j);
                        // cellData = rowData.GetCell(rowData.Cells.FirstOrDefault(x => x.ToString().Trim() == col.ColumnName).ColumnIndex);
                        if (col.IsRequired && cellData == null)
                        {
                            errorMsg += (string.IsNullOrEmpty(errorMsg) ? "" : "\n") + col.ColumnName + " Value Required for " + sheet.SheetName + " Sheet";
                        }
                        else
                        {
                            try
                            {
                                if (cellData != null && (col.IsRequired || (col.RequiredValues != null && col.RequiredValues.Any())))
                                {
                                    var colValue = Convert.ChangeType(cellData.ToString(), col.DataType);
                                    if (col.DataType == typeof(string))
                                    {
                                        if (colValue != "")
                                        {
                                            if (col.RequiredValues != null && !col.RequiredValues.Any(x => x.Trim().ToLower() == colValue.ToString().Trim().ToLower()))
                                            {
                                                errorMsg += (string.IsNullOrEmpty(errorMsg) ? "" : "\n") + col.ColumnName + " Value may be " + string.Join(",", col.RequiredValues) + " for " + sheet.SheetName + " Sheet";
                                            }
                                        }
                                    }
                                    else
                                    {
                                        if (col.RequiredValues != null && !col.RequiredValues.Any(x => x.Trim().ToLower() == colValue.ToString().Trim().ToLower()))
                                        {
                                            errorMsg += (string.IsNullOrEmpty(errorMsg) ? "" : "\n") + col.ColumnName + " Value may be " + string.Join(",", col.RequiredValues) + " for " + sheet.SheetName + " Sheet";
                                        }
                                    }

                                }
                            }
                            catch
                            {
                                errorMsg += (string.IsNullOrEmpty(errorMsg) ? "" : "\n") + col.ColumnName + " Value type mismatch for " + sheet.SheetName + " Sheet";
                            }
                        }
                    }
                    //    else if (col.IsRequired)
                    //{
                    //    errorMsg += (string.IsNullOrEmpty(errorMsg) ? "" : "\n") + col.ColumnName + " Value Required for " + sheet.SheetName + " Sheet";
                    //}
                }
            }
            return errorMsg;
        }
        public BillDiscountResponseExcel ReadOfferBillDiscountFile(XSSFWorkbook hssfwb, int userid, int i, List<OfferColumn> Columns)
        {
            using (var context = new AuthContext())
            {
                string Msg = "";
                string sSheetName = hssfwb.GetSheetName(i);
                ISheet sheet = hssfwb.GetSheet(sSheetName);
                IRow rowData;
                ICell cellData = null;
                BillDiscountResponseExcel BillDiscountResponseExcel = new BillDiscountResponseExcel();
                List<billDiscountDC> billDiscountDCs = new List<billDiscountDC>();


                List<billDiscountDC> trnfrorders = new List<billDiscountDC>();
                for (int iRowIdx = 1; iRowIdx <= sheet.LastRowNum; iRowIdx++)
                {
                    rowData = sheet.GetRow(iRowIdx);
                    if (rowData != null)
                    {
                        billDiscountDC trnfrorder = new billDiscountDC();
                        try
                        {
                            for (int j = 0; j < Columns.Count(); j++)
                            {
                                var col = Columns[j];
                                //}
                                //foreach (var col in Columns)
                                //{
                                //if (rowData.Cells.Any(x => x.ToString().Trim() == col.ColumnName))
                                {
                                    cellData = rowData.GetCell(j);

                                    if (col.ColumnName == "Warehouse Name")
                                        trnfrorder.WarehouseName = cellData == null ? "" : cellData.ToString();
                                    else if (col.ColumnName == "Select App Type")
                                        trnfrorder.SelectAppType = cellData == null ? "" : cellData.ToString();
                                    else if (col.ColumnName == "Offer Name")
                                        trnfrorder.OfferName = cellData == null ? "" : cellData.ToString();
                                    else if (col.ColumnName == "Offer On")
                                        trnfrorder.OfferOn = cellData == null ? "" : cellData.ToString();
                                    else if (col.ColumnName == "Auto Apply")
                                    {
                                        col3 = string.Empty;
                                        var cellDataPer = cellData == null ? "" : cellData.ToString().Trim();
                                        col3 = cellDataPer == "" && cellDataPer.Trim() != "-" ? null : cellDataPer.ToString();

                                        if (!string.IsNullOrEmpty(col3) && col3 == "1")
                                            col3 = "TRUE";
                                        else
                                            col3 = "FALSE";
                                        trnfrorder.AutoApply = cellData == null ? false : Convert.ToBoolean(col3);
                                    }
                                    else if (col.ColumnName == "Offer By")
                                        trnfrorder.OfferBy = cellData == null ? "" : cellData.ToString();
                                    else if (col.ColumnName == "Select Store")
                                        trnfrorder.SelectStore = cellData == null ? "" : cellData.ToString();
                                    else if (col.ColumnName == "BillDiscountType")
                                        trnfrorder.BillDiscountType = cellData == null ? "" : cellData.ToString();
                                    else if (col.ColumnName == "User Type")
                                        trnfrorder.Usertype = cellData == null ? "" : cellData.ToString();
                                    else if (col.ColumnName == "Group/Level Name")
                                        trnfrorder.GroupLevelName = cellData == null ? "" : cellData.ToString();
                                    else if (col.ColumnName == "Exclude User Group")
                                        trnfrorder.ExcludeUserGroup = cellData == null ? "" : cellData.ToString();
                                    else if (col.ColumnName == "Multi Time Use")
                                    {
                                        col4 = string.Empty;
                                        var cellDataPer = cellData == null ? "" : cellData.ToString().Trim();
                                        col4 = cellDataPer == "" && cellDataPer.Trim() != "-" ? null : cellDataPer.ToString();

                                        if (!string.IsNullOrEmpty(col4) && col4 == "1")
                                            col4 = "TRUE";
                                        else
                                            col4 = "FALSE";
                                        trnfrorder.MultiTimeUse = cellData == null ? false : Convert.ToBoolean(col4);
                                    }
                                    else if (col.ColumnName == "Offer Use Count")
                                    {
                                        trnfrorder.OfferUseCount = string.IsNullOrEmpty(cellData.ToString()) ? 0 : Convert.ToInt32(cellData.ToString());
                                        //if (cellData == null)
                                        //{
                                        //    trnfrorder.OfferUseCount = null;
                                        //}
                                        //else
                                        //{
                                        //    trnfrorder.OfferUseCount = Convert.ToInt32(cellData.ToString());
                                        //}
                                    }

                                    else if (col.ColumnName == "Use other Offer")
                                    {
                                        {
                                            col5 = string.Empty;
                                            var cellDataPer = cellData == null ? "" : cellData.ToString().Trim();
                                            col5 = cellDataPer == "" && cellDataPer.Trim() != "-" ? null : cellDataPer.ToString();

                                            if (!string.IsNullOrEmpty(col5) && col5 == "1")
                                                col5 = "TRUE";
                                            else
                                                col5 = "FALSE";
                                            trnfrorder.UseOtherOffer = cellData == null ? false : Convert.ToBoolean(col5);
                                        }

                                    }
                                    else if (col.ColumnName == "Combined Offer Group")
                                        trnfrorder.CombinedOfferGroup = cellData == null ? "" : cellData.ToString();
                                    else if (col.ColumnName == "Offer Limit")
                                        trnfrorder.OfferLimit = string.IsNullOrEmpty(cellData.ToString()) ? 0 : Convert.ToInt32(cellData.ToString());
                                    else if (col.ColumnName == "Start Date time")
                                    {
                                        string sdates = string.Empty;
                                        sdates = cellData == null ? "" : cellData.ToString();
                                        if (string.IsNullOrEmpty(sdates))
                                        {
                                            Msg = "Start Datetime is not Empty";
                                            BillDiscountResponseExcel.Status = false;
                                            BillDiscountResponseExcel.msg = Msg;
                                            return BillDiscountResponseExcel;
                                        }
                                        else
                                        {
                                            if (cellData.CellType.ToString() == "Numeric")
                                            {
                                                trnfrorder.StartDateTime = Convert.ToDateTime(cellData.DateCellValue.ToString());
                                            }
                                            else
                                            {
                                                trnfrorder.StartDateTime = Convert.ToDateTime(cellData.ToString());
                                            }

                                        }
                                    }
                                    else if (col.ColumnName == "End Date Time")
                                    {
                                        string edates = string.Empty;
                                        edates = cellData == null ? "" : cellData.ToString();
                                        if (string.IsNullOrEmpty(edates))
                                        {
                                            Msg = "Start Datetime is not Empty";
                                            BillDiscountResponseExcel.Status = false;
                                            BillDiscountResponseExcel.msg = Msg;
                                            return BillDiscountResponseExcel;
                                        }
                                        else
                                        {
                                            if (cellData.CellType.ToString() == "Numeric")
                                            {
                                                trnfrorder.EndDateTime = Convert.ToDateTime(cellData.DateCellValue.ToString());
                                                //trnfrorder.EndDateTime.ToString("dd/MM/yyyy HH:mm:ss");
                                            }
                                            else
                                            {
                                                trnfrorder.EndDateTime = Convert.ToDateTime(cellData.ToString());
                                                //trnfrorder.EndDateTime.ToString("dd/MM/yyyy HH:mm:ss");
                                            }

                                        }

                                    }

                                    else if (col.ColumnName == "Discount On")
                                        trnfrorder.DiscountOn = cellData == null ? "" : Convert.ToString(cellData);
                                    else if (col.ColumnName == "Percentage")
                                    {
                                        col1 = string.Empty;
                                        var cellDataPer = cellData == null ? "" : cellData.ToString().Trim();
                                        col1 = cellDataPer == "" && cellDataPer.Trim() != "-" ? null : cellDataPer.ToString();
                                        //trnfrorder.Percentage = cellData == null ? 0 : Convert.ToDouble(cellDataPer.ToString());
                                        trnfrorder.Percentage = string.IsNullOrEmpty(cellData.ToString()) ? 0 : Convert.ToDouble(cellDataPer.ToString());
                                    }
                                    else if (col.ColumnName == "Wallet Type")
                                        trnfrorder.WalletType = cellData == null ? "" : Convert.ToString(cellData);
                                    else if (col.ColumnName == "Apply On")
                                        trnfrorder.ApplyOn = cellData == null ? "" : Convert.ToString(cellData);
                                    else if (col.ColumnName == "Wallet Point")
                                        trnfrorder.WalletPoint = string.IsNullOrEmpty(cellData.ToString()) ? 0 : Convert.ToDouble(cellData.ToString());
                                    else if (col.ColumnName == "Bill Amount Min Limit")
                                    {
                                        col2 = string.Empty;
                                        var cellDataBillAmountMinLimit = cellData == null ? "" : cellData.ToString().Trim();
                                        col2 = cellDataBillAmountMinLimit == "" && cellDataBillAmountMinLimit.Trim() != "-" ? null : cellDataBillAmountMinLimit.ToString();
                                        trnfrorder.BillAmountMinLimit = cellData == null ? 0 : Convert.ToDouble(cellDataBillAmountMinLimit);
                                    }
                                    else if (col.ColumnName == "Bill Amount Max Limit")
                                    {

                                        trnfrorder.BillAmountMaxLimit = string.IsNullOrEmpty(cellData.ToString()) ? 0 : Convert.ToDouble(cellData.ToString());
                                    }

                                    else if (col.ColumnName == "Maximum Discount")
                                        trnfrorder.MaximumDiscount = string.IsNullOrEmpty(cellData.ToString()) ? 0 : Convert.ToDouble(cellData.ToString());
                                    else if (col.ColumnName == "Number of Line Item")
                                    {
                                        //col3 = string.Empty;
                                        //var cellDataNumberofLineItem = cellData == null ? "" : cellData.ToString().Trim();
                                        //col3 = cellDataNumberofLineItem == "" && cellDataNumberofLineItem.Trim() != "-" ? null : cellDataNumberofLineItem.ToString();
                                        //trnfrorder.NumberOfLineItem = cellData == null ? 0 : Convert.ToInt32(cellDataNumberofLineItem);
                                        trnfrorder.NumberOfLineItem = string.IsNullOrEmpty(cellData.ToString()) ? 0 : Convert.ToInt32(cellData.ToString());
                                    }
                                    //else if (col.ColumnName == "Add Line item Value")
                                    //    trnfrorder.AddLimeItemValue = cellData == null ? 0 : Convert.ToInt32(cellData.ToString());
                                    else if (col.ColumnName == "Description")
                                        trnfrorder.Description = cellData == null ? "" : Convert.ToString(cellData);
                                }
                            }


                            trnfrorder.userid = userid;
                            trnfrorders.Add(trnfrorder);
                        }
                        catch (Exception ex)
                        {
                            Msg = "Excel File: " + ex.Message;
                            BillDiscountResponseExcel.Status = false;
                            BillDiscountResponseExcel.msg = Msg;
                            return BillDiscountResponseExcel;
                        }
                    }

                }

                BillDiscountResponseExcel.billDiscountDC = trnfrorders;
                BillDiscountResponseExcel.Status = true;
                BillDiscountResponseExcel.msg = "ok";

                return BillDiscountResponseExcel;
            }
        }
        public IncludeItemResponseExcel ReadOfferIncludeItemFile(XSSFWorkbook hssfwb, int userid, int i)
        {
            using (var context = new AuthContext())
            {
                string Msg = string.Empty;
                string sSheetName = hssfwb.GetSheetName(i);
                ISheet sheet = hssfwb.GetSheet(sSheetName);
                IRow rowData;
                ICell cellData = null;
                IncludeItemResponseExcel IncludeItemResponseExcel = new IncludeItemResponseExcel();
                List<offeruploadIncludeItemDC> IncludeItemUploader = new List<offeruploadIncludeItemDC>();

                List<string> headerlst = new List<string>();

                List<offeruploadIncludeItemDC> trnfrorders = new List<offeruploadIncludeItemDC>();
                for (int iRowIdx = 1; iRowIdx <= sheet.LastRowNum; iRowIdx++)  //  iRowIdx = 0; HeaderRow
                {

                    rowData = sheet.GetRow(iRowIdx);
                    if (rowData != null)
                    {
                        // ForecastInventoryDaysUploadedFileDc trnfrorder = new ForecastInventoryDaysUploadedFileDc();
                        offeruploadIncludeItemDC trnfrorder = new offeruploadIncludeItemDC();
                        string WarehouseName = string.Empty, OfferName = string.Empty, Category = string.Empty, SubCategory = string.Empty, BrandName = string.Empty, Qty = string.Empty;
                        int ItemMultiMRPId = 0;
                        try
                        {
                            foreach (var cellDatas in rowData.Cells)
                            {
                                if (cellDatas.ColumnIndex <= 5)
                                {
                                    if (cellDatas.ColumnIndex == 0)
                                        WarehouseName = cellDatas.ToString();
                                    else if (cellDatas.ColumnIndex == 1)
                                        OfferName = cellDatas.ToString();
                                    else if (cellDatas.ColumnIndex == 2)
                                        Category = cellDatas.ToString();
                                    else if (cellDatas.ColumnIndex == 3)
                                        SubCategory = cellDatas.ToString();
                                    else if (cellDatas.ColumnIndex == 4)
                                        BrandName = cellDatas.ToString();
                                    else if (cellDatas.ColumnIndex == 5)
                                    {
                                        if (!string.IsNullOrEmpty(cellDatas.ToString()))
                                        {
                                            try
                                            {
                                                int id = 0;
                                                id = Convert.ToInt32(cellDatas.ToString());
                                                if (id > 0)
                                                {
                                                    ItemMultiMRPId = id;
                                                }
                                                else
                                                {
                                                    ItemMultiMRPId = 0;
                                                }
                                            }
                                            catch (Exception ex)
                                            {
                                            }
                                        }
                                        else
                                        {
                                            ItemMultiMRPId = 0;
                                        }
                                    }

                                }
                            }


                            trnfrorder.WarehouseName = WarehouseName;
                            trnfrorder.Offer_Name = OfferName;
                            trnfrorder.Category = Category;
                            trnfrorder.Sub_Category = SubCategory;
                            trnfrorder.BrandName = BrandName;
                            trnfrorder.ItemMultiMRPId = ItemMultiMRPId; //ItemMultiMRPId;
                            trnfrorder.SubCategoryMappingId = 0;
                            trnfrorder.BrandCategoryMappingId = 0;
                            trnfrorders.Add(trnfrorder);
                            IncludeItemUploader.Add(trnfrorder);
                        }
                        catch (Exception ex)
                        {
                            Msg = "Excel File: " + ex.Message;
                            IncludeItemResponseExcel.Status = false;
                            IncludeItemResponseExcel.msg = Msg;
                            return IncludeItemResponseExcel;
                            //msgitemname = ex.Message;
                            //logger.Error("Error VAN Paymant Upload File " + "\n\n" + ex.Message + "\n\n" + ex.InnerException + "\n\n" + ex.StackTrace);
                        }
                    }
                }


                if (trnfrorders != null && trnfrorders.Any())
                {
                    var validexcelinclude = trnfrorders.GroupBy(x => x.WarehouseName).Select(x => new offeruploadIncludeItemDC
                    {

                        WarehouseName = x.Key,
                        Offer_Name = x.Key
                    }).ToList();

                    foreach (var y in validexcelinclude)
                    {
                        if (y.WarehouseName == "" || y.Offer_Name == "")
                        {
                            Msg = y.WarehouseName + " Warehouse Name/Offer Name  is not being inserted in Include item file ";
                        }
                    }


                    IncludeItemResponseExcel.IncludeItemDC = trnfrorders;
                    IncludeItemResponseExcel.Status = true;
                    IncludeItemResponseExcel.msg = Msg;
                }

                return IncludeItemResponseExcel;

            }
        }
        public ExcludeItemResponseExcel ReadOfferExcludeItemFile(XSSFWorkbook hssfwb, int userid, int i)
        {
            using (var context = new AuthContext())
            {
                string Msg = string.Empty;
                string sSheetName = hssfwb.GetSheetName(i);
                ISheet sheet = hssfwb.GetSheet(sSheetName);
                IRow rowData;
                ICell cellData = null;
                ExcludeItemResponseExcel ExcludeItemResponseExcel = new ExcludeItemResponseExcel();

                List<offeruploadExcludeItemDC> ExcludeItemUploader = new List<offeruploadExcludeItemDC>();
                int? IdCellIndex1 = null;
                int? IdCellIndex2 = null;
                int? IdCellIndex3 = null;
                int? IdCellIndex4 = null;
                int? IdCellIndex5 = null;
                int? IdCellIndex6 = null;
                List<string> headerlst = new List<string>();

                List<offeruploadExcludeItemDC> trnfrorders = new List<offeruploadExcludeItemDC>();
                for (int iRowIdx = 0; iRowIdx <= sheet.LastRowNum; iRowIdx++)  //  iRowIdx = 0; HeaderRow
                {
                    if (iRowIdx == 0)
                    {
                        rowData = sheet.GetRow(iRowIdx);
                        if (rowData != null)
                        {
                            foreach (var item in rowData.Cells)
                            {
                                headerlst.Add(item.ToString());
                            }
                            IdCellIndex1 = rowData.Cells.Any(x => x.ToString().Trim() == "Warehouse Name") ? rowData.Cells.FirstOrDefault(x => x.ToString().Trim() == "Warehouse Name").ColumnIndex : (int?)null;
                            if (!IdCellIndex1.HasValue)
                            {
                                JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("Warehouse does not exist..try again");

                                ExcludeItemResponseExcel.msg = strJSON;
                                ExcludeItemResponseExcel.Status = false;
                                return ExcludeItemResponseExcel;
                            }

                            IdCellIndex2 = rowData.Cells.Any(x => x.ToString().Trim() == "Offer Name") ? rowData.Cells.FirstOrDefault(x => x.ToString().Trim() == "Offer Name").ColumnIndex : (int?)null;
                            if (!IdCellIndex2.HasValue)
                            {
                                JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("Offer does not exist..try again");
                                ExcludeItemResponseExcel.msg = strJSON;
                                ExcludeItemResponseExcel.Status = false;
                                return ExcludeItemResponseExcel;
                                //return Created(strJSON, strJSON); ;
                            }

                            IdCellIndex3 = rowData.Cells.Any(x => x.ToString().Trim() == "Category") ? rowData.Cells.FirstOrDefault(x => x.ToString().Trim() == "Category").ColumnIndex : (int?)null;
                            if (!IdCellIndex3.HasValue)
                            {
                                JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("Category does not exist..try again");
                                ExcludeItemResponseExcel.msg = strJSON;
                                ExcludeItemResponseExcel.Status = false;
                                return ExcludeItemResponseExcel;
                            }

                            IdCellIndex4 = rowData.Cells.Any(x => x.ToString().Trim() == "Sub Category") ? rowData.Cells.FirstOrDefault(x => x.ToString().Trim() == "Sub Category").ColumnIndex : (int?)null;
                            if (!IdCellIndex4.HasValue)
                            {
                                JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("Sub Category does not exist..try again");
                                ExcludeItemResponseExcel.msg = strJSON;
                                ExcludeItemResponseExcel.Status = false;
                                return ExcludeItemResponseExcel;
                            }

                            IdCellIndex5 = rowData.Cells.Any(x => x.ToString().Trim() == "Brand") ? rowData.Cells.FirstOrDefault(x => x.ToString().Trim() == "Brand").ColumnIndex : (int?)null;
                            if (!IdCellIndex5.HasValue)
                            {
                                JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("Brand does not exist..try again");
                                ExcludeItemResponseExcel.msg = strJSON;
                                ExcludeItemResponseExcel.Status = false;
                                return ExcludeItemResponseExcel;
                            }
                            IdCellIndex6 = rowData.Cells.Any(x => x.ToString().Trim() == "ItemMultiMRPId") ? rowData.Cells.FirstOrDefault(x => x.ToString().Trim() == "ItemMultiMRPId").ColumnIndex : (int?)null;
                            if (!IdCellIndex6.HasValue)
                            {
                                JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("ItemName does not exist..try again");
                                ExcludeItemResponseExcel.msg = strJSON;
                                ExcludeItemResponseExcel.Status = false;
                                return ExcludeItemResponseExcel;
                            }



                        }
                    }
                    else
                    {
                        rowData = sheet.GetRow(iRowIdx);
                        if (rowData != null)
                        {

                            offeruploadExcludeItemDC trnfrorder = new offeruploadExcludeItemDC();
                            string WarehouseName = string.Empty, OfferName = string.Empty, Category = string.Empty, SubCategory = string.Empty, BrandName = string.Empty, Qty = string.Empty;
                            int ItemMultiMRPId = 0;
                            try
                            {
                                trnfrorder = new offeruploadExcludeItemDC();
                                foreach (var cellDatas in rowData.Cells)
                                {
                                    if (cellDatas.ColumnIndex <= 5)
                                    {
                                        if (cellDatas.ColumnIndex == 0)
                                            WarehouseName = cellDatas.ToString();
                                        else if (cellDatas.ColumnIndex == 1)
                                            OfferName = cellDatas.ToString();

                                        else if (cellDatas.ColumnIndex == 2)
                                            Category = cellDatas.ToString();

                                        else if (cellDatas.ColumnIndex == 3)
                                            SubCategory = cellDatas.ToString();
                                        else if (cellDatas.ColumnIndex == 4)
                                            BrandName = cellDatas.ToString();
                                        else if (cellDatas.ColumnIndex == 5)
                                        {
                                            if (!string.IsNullOrEmpty(cellDatas.ToString()))
                                            {
                                                try
                                                {
                                                    int.TryParse(cellDatas.ToString(), out ItemMultiMRPId);
                                                }
                                                catch (Exception ex)
                                                {
                                                }
                                            }
                                        }

                                    }
                                }
                                {
                                    //trnfrorder. = dept;
                                    trnfrorder.WarehouseName = WarehouseName;
                                    trnfrorder.Offer_Name = OfferName;
                                    trnfrorder.Category = Category;
                                    trnfrorder.Sub_Category = SubCategory;
                                    trnfrorder.BrandName = BrandName;
                                    trnfrorder.ItemMultiMRPId = ItemMultiMRPId; //ItemMultiMRPId;

                                    trnfrorders.Add(trnfrorder);
                                }

                                ExcludeItemUploader.Add(trnfrorder);
                            }
                            catch (Exception ex)
                            {
                                Msg = "Excel File: " + ex.Message;
                                ExcludeItemResponseExcel.Status = false;
                                ExcludeItemResponseExcel.msg = Msg;
                                return ExcludeItemResponseExcel;
                                //msgitemname = ex.Message;
                                //logger.Error("Error VAN Paymant Upload File " + "\n\n" + ex.Message + "\n\n" + ex.InnerException + "\n\n" + ex.StackTrace);
                            }
                        }
                    }
                }
                if (trnfrorders != null && trnfrorders.Any())
                {
                    var validexcelinclude = trnfrorders.GroupBy(x => x.WarehouseName).Select(x => new offeruploadExcludeItemDC
                    {

                        WarehouseName = x.Key,
                        Offer_Name = x.Key
                    }).ToList();

                    foreach (var y in validexcelinclude)
                    {
                        if (y.WarehouseName == "" || y.Offer_Name == "")
                        {
                            Msg = y.WarehouseName + " Warehouse Name/Offer Name  is not being inserted in Exclude item file ";
                        }
                    }

                    if (Msg != "")
                    {
                        //BillDiscountResponseExcel.billDiscountDC = trnfrorders;
                        ExcludeItemResponseExcel.ExcludeItemDC = null;
                        ExcludeItemResponseExcel.Status = false;
                        ExcludeItemResponseExcel.msg = Msg;
                    }
                    else
                    {
                        ExcludeItemResponseExcel.ExcludeItemDC = trnfrorders;
                        ExcludeItemResponseExcel.Status = true;
                        ExcludeItemResponseExcel.msg = "OK";
                    }

                }

                return ExcludeItemResponseExcel;

            }
        }
        public MandatoryResponseExcel ReadOfferMandatoryItemFile(XSSFWorkbook hssfwb, int userid, int i)
        {
            using (var context = new AuthContext())
            {
                string Msg = string.Empty;
                string sSheetName = hssfwb.GetSheetName(i);
                ISheet sheet = hssfwb.GetSheet(sSheetName);
                IRow rowData;
                ICell cellData = null;

                MandatoryResponseExcel MandatoryResponseExcel = new MandatoryResponseExcel();
                List<offeruploadMandatoryDC> MandatoryUploader = new List<offeruploadMandatoryDC>();
                int? IdCellIndex1 = null;
                int? IdCellIndex2 = null;
                int? IdCellIndex3 = null;
                int? IdCellIndex4 = null;
                int? IdCellIndex5 = null;
                int? IdCellIndex6 = null;
                int? IdCellIndex7 = null;
                int? IdCellIndex8 = null;
                int? IdCellIndex9 = null;
                List<string> headerlst = new List<string>();
                //  List<ForecastInventoryDay> Addlist = new List<ForecastInventoryDay>();
                List<offeruploadMandatoryDC> trnfrorders = new List<offeruploadMandatoryDC>();
                for (int iRowIdx = 0; iRowIdx <= sheet.LastRowNum; iRowIdx++)  //  iRowIdx = 0; HeaderRow
                {
                    if (iRowIdx == 0)
                    {
                        rowData = sheet.GetRow(iRowIdx);
                        if (rowData != null)
                        {
                            foreach (var item in rowData.Cells)
                            {
                                headerlst.Add(item.ToString());
                            }
                            IdCellIndex1 = rowData.Cells.Any(x => x.ToString().Trim() == "Warehouse Name") ? rowData.Cells.FirstOrDefault(x => x.ToString().Trim() == "Warehouse Name").ColumnIndex : (int?)null;
                            if (!IdCellIndex1.HasValue)
                            {
                                JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("Warehouse does not exist..try again");
                                MandatoryResponseExcel.msg = strJSON;
                                MandatoryResponseExcel.Status = false;
                                return MandatoryResponseExcel;
                            }

                            IdCellIndex2 = rowData.Cells.Any(x => x.ToString().Trim() == "Offer Name") ? rowData.Cells.FirstOrDefault(x => x.ToString().Trim() == "Offer Name").ColumnIndex : (int?)null;
                            if (!IdCellIndex2.HasValue)
                            {
                                JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("Offer does not exist..try again");
                                MandatoryResponseExcel.msg = strJSON;
                                MandatoryResponseExcel.Status = false;
                                return MandatoryResponseExcel;
                            }

                            IdCellIndex3 = rowData.Cells.Any(x => x.ToString().Trim() == "ItemRequiredOn") ? rowData.Cells.FirstOrDefault(x => x.ToString().Trim() == "ItemRequiredOn").ColumnIndex : (int?)null;
                            if (!IdCellIndex3.HasValue)
                            {
                                JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("ObjectType does not exist..try again");
                                MandatoryResponseExcel.msg = strJSON;
                                MandatoryResponseExcel.Status = false;
                                return MandatoryResponseExcel;
                            }

                            IdCellIndex4 = rowData.Cells.Any(x => x.ToString().Trim() == "Category") ? rowData.Cells.FirstOrDefault(x => x.ToString().Trim() == "Category").ColumnIndex : (int?)null;
                            if (!IdCellIndex4.HasValue)
                            {
                                JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("Category does not exist..try again");
                                MandatoryResponseExcel.msg = strJSON;
                                MandatoryResponseExcel.Status = false;
                                return MandatoryResponseExcel;
                            }

                            IdCellIndex5 = rowData.Cells.Any(x => x.ToString().Trim() == "Sub Category") ? rowData.Cells.FirstOrDefault(x => x.ToString().Trim() == "Sub Category").ColumnIndex : (int?)null;
                            if (!IdCellIndex5.HasValue)
                            {
                                JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("Sub Category does not exist..try again");
                                MandatoryResponseExcel.msg = strJSON;
                                MandatoryResponseExcel.Status = false;
                                return MandatoryResponseExcel;
                            }

                            IdCellIndex6 = rowData.Cells.Any(x => x.ToString().Trim() == "Brand") ? rowData.Cells.FirstOrDefault(x => x.ToString().Trim() == "Brand").ColumnIndex : (int?)null;
                            if (!IdCellIndex6.HasValue)
                            {
                                JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("Brand does not exist..try again");
                                MandatoryResponseExcel.msg = strJSON;
                                MandatoryResponseExcel.Status = false;
                                return MandatoryResponseExcel;
                            }
                            IdCellIndex7 = rowData.Cells.Any(x => x.ToString().Trim() == "ItemMultiMRPId") ? rowData.Cells.FirstOrDefault(x => x.ToString().Trim() == "ItemMultiMRPId").ColumnIndex : (int?)null;
                            if (!IdCellIndex7.HasValue)
                            {
                                JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("ItemName does not exist..try again");
                                MandatoryResponseExcel.msg = strJSON;
                                MandatoryResponseExcel.Status = false;
                                return MandatoryResponseExcel;
                            }
                            IdCellIndex8 = rowData.Cells.Any(x => x.ToString().Trim() == "Qty/Value") ? rowData.Cells.FirstOrDefault(x => x.ToString().Trim() == "Qty/Value").ColumnIndex : (int?)null;
                            if (!IdCellIndex8.HasValue)
                            {
                                JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("Qty does not exist..try again");
                                MandatoryResponseExcel.msg = strJSON;
                                MandatoryResponseExcel.Status = false;
                                return MandatoryResponseExcel;
                            }
                            IdCellIndex9 = rowData.Cells.Any(x => x.ToString().Trim() == "ValueType") ? rowData.Cells.FirstOrDefault(x => x.ToString().Trim() == "ValueType").ColumnIndex : (int?)null;
                            if (!IdCellIndex9.HasValue)
                            {
                                JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("ValueType does not exist..try again");
                                MandatoryResponseExcel.msg = strJSON;
                                MandatoryResponseExcel.Status = false;
                                return MandatoryResponseExcel;
                            }

                            //SubCateName
                        }
                    }
                    else
                    {
                        rowData = sheet.GetRow(iRowIdx);
                        if (rowData != null)
                        {
                            // ForecastInventoryDaysUploadedFileDc trnfrorder = new ForecastInventoryDaysUploadedFileDc();
                            offeruploadMandatoryDC trnfrorder = new offeruploadMandatoryDC();
                            string WarehouseName = string.Empty, OfferName = string.Empty, Category = string.Empty, SubCategory = string.Empty, BrandName = string.Empty, ItemRequiredOn = string.Empty, ValueType = string.Empty;
                            int Qty = 0;
                            int ItemMultiMRPId = 0;
                            try
                            {
                                foreach (var cellDatas in rowData.Cells)
                                {
                                    trnfrorder = new offeruploadMandatoryDC();
                                    if (cellDatas.ColumnIndex < 9)
                                    {
                                        if (cellDatas.ColumnIndex == 0)
                                            WarehouseName = cellDatas.ToString();
                                        else if (cellDatas.ColumnIndex == 1)
                                            OfferName = cellDatas.ToString();
                                        else if (cellDatas.ColumnIndex == 2)
                                            ItemRequiredOn = cellDatas.ToString();
                                        else if (cellDatas.ColumnIndex == 3)
                                            Category = cellDatas.ToString();
                                        else if (cellDatas.ColumnIndex == 4)
                                            SubCategory = cellDatas.ToString();
                                        else if (cellDatas.ColumnIndex == 5)
                                            BrandName = cellDatas.ToString();
                                        else if (cellDatas.ColumnIndex == 6)
                                        {
                                            if (!string.IsNullOrEmpty(cellDatas.ToString()))
                                            {
                                                try
                                                {
                                                    var result = int.TryParse(cellDatas.ToString(), out ItemMultiMRPId);
                                                }
                                                catch (Exception ex)
                                                {
                                                }
                                            }
                                        }
                                        else if (cellDatas.ColumnIndex == 7)
                                            ValueType = cellDatas.ToString();
                                        else if (cellDatas.ColumnIndex == 8)
                                        {
                                            if (!string.IsNullOrEmpty(cellDatas.ToString()))
                                            {
                                                try
                                                {
                                                    var result = int.TryParse(cellDatas.ToString(), out Qty);
                                                }
                                                catch (Exception ex)
                                                {
                                                }
                                            }
                                        }

                                    }

                                }

                                //trnfrorder. = dept;
                                trnfrorder.WarehouseName = WarehouseName;
                                trnfrorder.Offer_Name = OfferName;
                                trnfrorder.ItemRequiredOn = ItemRequiredOn;
                                trnfrorder.Category = Category;
                                trnfrorder.Sub_Category = SubCategory;
                                trnfrorder.BrandName = BrandName;
                                trnfrorder.ItemMultiMRPId = ItemMultiMRPId;
                                trnfrorder.ValueType = ValueType;
                                trnfrorder.Qty = Qty;
                                trnfrorders.Add(trnfrorder);

                                MandatoryUploader.Add(trnfrorder);
                            }
                            catch (Exception ex)
                            {
                                Msg = "Excel File: " + ex.Message;
                                MandatoryResponseExcel.Status = false;
                                MandatoryResponseExcel.msg = Msg;
                                return MandatoryResponseExcel;
                                //msgitemname = ex.Message;
                                //logger.Error("Error VAN Paymant Upload File " + "\n\n" + ex.Message + "\n\n" + ex.InnerException + "\n\n" + ex.StackTrace);
                            }
                        }
                    }
                }
                if (trnfrorders != null && trnfrorders.Any())
                {
                    var validexcelinclude = trnfrorders.GroupBy(x => x.WarehouseName).Select(x => new offeruploadMandatoryDC
                    {

                        WarehouseName = x.Key,
                        Offer_Name = x.Key
                    }).ToList();

                    foreach (var y in validexcelinclude)
                    {
                        if (y.WarehouseName == "" || y.Offer_Name == "")
                        {
                            Msg = y.WarehouseName + " Warehouse Name/Offer Name  is not being inserted in Mandatory item file ";
                        }
                    }

                    if (Msg != "")
                    {
                        MandatoryResponseExcel.Status = false;
                        MandatoryResponseExcel.msg = Msg;
                    }
                    else
                    {
                        MandatoryResponseExcel.MandatoryDC = trnfrorders;
                        MandatoryResponseExcel.Status = true;
                        MandatoryResponseExcel.msg = "Ok";
                    }


                }


                return MandatoryResponseExcel;

            }
        }
        public FreeitemResponseExcel ReadOfferFreeitemFile(XSSFWorkbook hssfwb, int userid, int i)
        {
            using (var context = new AuthContext())
            {
                string Msg = string.Empty;
                string sSheetName = hssfwb.GetSheetName(i);
                ISheet sheet = hssfwb.GetSheet(sSheetName);
                IRow rowData;
                ICell cellData = null;
                FreeitemResponseExcel FreeitemResponseExcel = new FreeitemResponseExcel();
                List<offeruploadFreeitemDC> FreeitemUploader = new List<offeruploadFreeitemDC>();
                int? IdCellIndex1 = null;
                int? IdCellIndex2 = null;
                int? IdCellIndex3 = null;
                int? IdCellIndex4 = null;
                int? IdCellIndex5 = null;
                int? IdCellIndex6 = null;
                int? IdCellIndex7 = null;
                int? IdCellIndex8 = null;
                int? IdCellIndex9 = null;

                List<string> headerlst = new List<string>();
                //  List<ForecastInventoryDay> Addlist = new List<ForecastInventoryDay>();
                List<offeruploadFreeitemDC> trnfrorders = new List<offeruploadFreeitemDC>();
                for (int iRowIdx = 0; iRowIdx <= sheet.LastRowNum; iRowIdx++)  //  iRowIdx = 0; HeaderRow
                {
                    if (iRowIdx == 0)
                    {
                        rowData = sheet.GetRow(iRowIdx);
                        if (rowData != null)
                        {
                            foreach (var item in rowData.Cells)
                            {
                                headerlst.Add(item.ToString());
                            }
                            IdCellIndex1 = rowData.Cells.Any(x => x.ToString().Trim() == "Warehouse Name") ? rowData.Cells.FirstOrDefault(x => x.ToString().Trim() == "Warehouse Name").ColumnIndex : (int?)null;
                            if (!IdCellIndex1.HasValue)
                            {
                                JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("Warehouse does not exist..try again");
                                FreeitemResponseExcel.msg = strJSON;
                                FreeitemResponseExcel.Status = false;
                                return FreeitemResponseExcel;

                            }

                            IdCellIndex2 = rowData.Cells.Any(x => x.ToString().Trim() == "Offer Name") ? rowData.Cells.FirstOrDefault(x => x.ToString().Trim() == "Offer Name").ColumnIndex : (int?)null;
                            if (!IdCellIndex2.HasValue)
                            {
                                JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("Offer does not exist..try again");
                                FreeitemResponseExcel.msg = strJSON;
                                FreeitemResponseExcel.Status = false;
                                return FreeitemResponseExcel;
                            }

                            IdCellIndex3 = rowData.Cells.Any(x => x.ToString().Trim() == "Category") ? rowData.Cells.FirstOrDefault(x => x.ToString().Trim() == "Category").ColumnIndex : (int?)null;
                            if (!IdCellIndex3.HasValue)
                            {
                                JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("Category does not exist..try again");
                                FreeitemResponseExcel.msg = strJSON;
                                FreeitemResponseExcel.Status = false;
                                return FreeitemResponseExcel;
                            }

                            IdCellIndex4 = rowData.Cells.Any(x => x.ToString().Trim() == "Sub Category") ? rowData.Cells.FirstOrDefault(x => x.ToString().Trim() == "Sub Category").ColumnIndex : (int?)null;
                            if (!IdCellIndex4.HasValue)
                            {
                                JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("Sub Category does not exist..try again");
                                FreeitemResponseExcel.msg = strJSON;
                                FreeitemResponseExcel.Status = false;
                                return FreeitemResponseExcel;
                            }

                            IdCellIndex5 = rowData.Cells.Any(x => x.ToString().Trim() == "Brand") ? rowData.Cells.FirstOrDefault(x => x.ToString().Trim() == "Brand").ColumnIndex : (int?)null;
                            if (!IdCellIndex5.HasValue)
                            {
                                JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("Brand does not exist..try again");
                                FreeitemResponseExcel.msg = strJSON;
                                FreeitemResponseExcel.Status = false;
                                return FreeitemResponseExcel;
                            }
                            IdCellIndex6 = rowData.Cells.Any(x => x.ToString().Trim() == "ItemMultiMRPId") ? rowData.Cells.FirstOrDefault(x => x.ToString().Trim() == "ItemMultiMRPId").ColumnIndex : (int?)null;
                            if (!IdCellIndex6.HasValue)
                            {
                                JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("ItemMultiMRPId does not exist..try again");
                                FreeitemResponseExcel.msg = strJSON;
                                FreeitemResponseExcel.Status = false;
                                return FreeitemResponseExcel;
                            }
                            IdCellIndex7 = rowData.Cells.Any(x => x.ToString().Trim() == "Stock Hit") ? rowData.Cells.FirstOrDefault(x => x.ToString().Trim() == "Stock Hit").ColumnIndex : (int?)null;
                            if (!IdCellIndex7.HasValue)
                            {
                                JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("Stock Hit does not exist..try again");
                                FreeitemResponseExcel.msg = strJSON;
                                FreeitemResponseExcel.Status = false;
                                return FreeitemResponseExcel;
                            }
                            IdCellIndex8 = rowData.Cells.Any(x => x.ToString().Trim() == "Freeitem Qty") ? rowData.Cells.FirstOrDefault(x => x.ToString().Trim() == "Freeitem Qty").ColumnIndex : (int?)null;
                            if (!IdCellIndex8.HasValue)
                            {
                                JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("Freeitem Qty does not exist..try again");
                                FreeitemResponseExcel.msg = strJSON;
                                FreeitemResponseExcel.Status = false;
                                return FreeitemResponseExcel;
                            }
                            IdCellIndex9 = rowData.Cells.Any(x => x.ToString().Trim() == "SellingSku") ? rowData.Cells.FirstOrDefault(x => x.ToString().Trim() == "SellingSku").ColumnIndex : (int?)null;
                            if (!IdCellIndex9.HasValue)
                            {
                                JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("SellingSku does not exist..try again");
                                FreeitemResponseExcel.msg = strJSON;
                                FreeitemResponseExcel.Status = false;
                                return FreeitemResponseExcel;
                            }
                            //SubCateName
                        }
                    }
                    else
                    {
                        rowData = sheet.GetRow(iRowIdx);
                        if (rowData != null)
                        {
                            // ForecastInventoryDaysUploadedFileDc trnfrorder = new ForecastInventoryDaysUploadedFileDc();
                            offeruploadFreeitemDC trnfrorder = new offeruploadFreeitemDC();
                            string WarehouseName = string.Empty, OfferName = string.Empty, Category = string.Empty, SubCategory = string.Empty, BrandName = string.Empty;
                            string StockHit = string.Empty, Sellingsku = string.Empty;
                            int ItemMultiMRPId = 0; int Freeitem_Qty = 0;
                            try
                            {
                                foreach (var cellDatas in rowData.Cells)
                                {
                                    trnfrorder = new offeruploadFreeitemDC();
                                    if (cellDatas.ColumnIndex <= 8)
                                    {
                                        if (cellDatas.ColumnIndex == 0)
                                            WarehouseName = cellDatas.ToString();
                                        else if (cellDatas.ColumnIndex == 1)
                                            OfferName = cellDatas.ToString();
                                        else if (cellDatas.ColumnIndex == 2)
                                            Category = cellDatas.ToString();
                                        else if (cellDatas.ColumnIndex == 3)
                                            SubCategory = cellDatas.ToString();
                                        else if (cellDatas.ColumnIndex == 4)
                                            BrandName = cellDatas.ToString();
                                        else if (cellDatas.ColumnIndex == 5)
                                            ItemMultiMRPId = Convert.ToInt32(cellDatas.ToString());
                                        else if (cellDatas.ColumnIndex == 6)
                                            StockHit = cellDatas.ToString();
                                        else if (cellDatas.ColumnIndex == 7)
                                            Freeitem_Qty = Convert.ToInt32(cellDatas.ToString());
                                        else if (cellDatas.ColumnIndex == 8)
                                            Sellingsku = cellDatas.ToString();

                                    }

                                }

                                trnfrorder.WarehouseName = WarehouseName;
                                trnfrorder.Offer_Name = OfferName;
                                trnfrorder.Category = Category;
                                trnfrorder.Sub_Category = SubCategory;
                                trnfrorder.BrandName = BrandName;
                                trnfrorder.ItemMultiMRPId = ItemMultiMRPId;//Convert.ToInt32(cellDatas.ToString()); //ItemMultiMRPId;   
                                trnfrorder.Stock_Hit = StockHit;
                                trnfrorder.Freeitem_Qty = Freeitem_Qty; //int.Parse(inventorydays);
                                trnfrorder.SellingSku = Sellingsku;
                                trnfrorders.Add(trnfrorder);
                                FreeitemUploader.Add(trnfrorder);
                            }
                            catch (Exception ex)
                            {
                                Msg = "Excel File: " + ex.Message;
                                FreeitemResponseExcel.Status = false;
                                FreeitemResponseExcel.msg = Msg;
                                return FreeitemResponseExcel;
                                //msgitemname = ex.Message;
                                //logger.Error("Error VAN Paymant Upload File " + "\n\n" + ex.Message + "\n\n" + ex.InnerException + "\n\n" + ex.StackTrace);
                            }
                        }
                    }
                }
                if (trnfrorders != null && trnfrorders.Any())
                {
                    var validexcelinclude = trnfrorders.GroupBy(x => x.WarehouseName).Select(x => new offeruploadFreeitemDC
                    {

                        WarehouseName = x.Key,
                        Offer_Name = x.Key
                    }).ToList();
                    foreach (var y in validexcelinclude)
                    {
                        if (y.WarehouseName == "" || y.Offer_Name == "")
                        {
                            Msg = y.WarehouseName + " Warehouse Name/Offer Name  is not being inserted in Free item file ";
                        }
                    }

                    if (Msg != "")
                    {
                        FreeitemResponseExcel.Status = false;
                        FreeitemResponseExcel.msg = Msg;
                    }
                    else
                    {
                        FreeitemResponseExcel.FreeitemDC = trnfrorders;
                        FreeitemResponseExcel.Status = true;
                        FreeitemResponseExcel.msg = "Ok";
                    }
                }
                return FreeitemResponseExcel;
            }
        }
        public List<FreebiesUploader> AddFreebiesDuringItemScheme(List<FreebiesUploader> FreebiesUploader, int userId, AuthContext context, int SubCatId)
        {
            bool result = true;
            List<FreebiesDTO> flashdealList = new List<FreebiesDTO>();

            List<GenricEcommers.Models.Offer> offers = new List<GenricEcommers.Models.Offer>();
            List<ItemMaster> itemList = new List<ItemMaster>();
            if (FreebiesUploader.Count > 0)
            {
                try
                {
                    ItemMaster itemMaster = new ItemMaster();
                    ItemMaster freeitem = new ItemMaster();
                    var cityids = FreebiesUploader.Select(x => x.CityId).Distinct().ToList();
                    var warehouses = context.Warehouses.Where(x => cityids.Contains(x.Cityid) && x.active && x.IsKPP == false && !x.Deleted).ToList();
                    var WarehouseIds = warehouses.Select(x => x.WarehouseId).ToList();
                    var Itemnumbers = FreebiesUploader.Select(x => x.Itemnumber + "-" + x.MRP).Distinct().ToList();
                    var freeItemnumbers = FreebiesUploader.Select(x => x.FreeItemnumber + "-" + x.FreeitemMRP).Distinct().ToList();

                    List<ItemMaster> itemMasters = context.itemMasters.Where(x => Itemnumbers.Contains(x.Number + "-" + x.price) && WarehouseIds.Contains(x.WarehouseId)).ToList();
                    List<ItemMaster> freeitems = context.itemMasters.Where(x => freeItemnumbers.Contains(x.Number + "-" + x.price) && WarehouseIds.Contains(x.WarehouseId)).ToList();

                    foreach (var item in FreebiesUploader)
                    {
                        foreach (var warehouse in warehouses.Where(x => x.WarehouseId == item.WarehouseId).ToList())
                        {
                            item.WarehouseId = warehouse.WarehouseId;
                            itemMaster = itemMasters.Where(x => x.Number == item.Itemnumber && x.WarehouseId == item.WarehouseId && x.price == item.MRP).FirstOrDefault();
                            if (item.Itemnumber == item.FreeItemnumber && item.FreeitemMRP == item.MRP)
                                freeitem = itemMaster;
                            else
                                freeitem = freeitems.Where(x => x.Number == item.FreeItemnumber && x.price == item.FreeitemMRP && x.WarehouseId == item.WarehouseId).FirstOrDefault();

                            if (itemMaster != null && freeitem != null)
                            {
                                Offer Newoffer = new Offer();
                                Newoffer.CompanyId = 1;
                                Newoffer.StoreId = 0;
                                Newoffer.userid = userId;
                                Newoffer.WarehouseId = item.WarehouseId;
                                Newoffer.ApplyType = "Warehouse";
                                Newoffer.itemname = itemMaster.itemname;
                                Newoffer.FreeItemName = freeitem.itemname;
                                Newoffer.FreeItemMRP = freeitem.price;
                                Newoffer.start = item.StartDate;
                                Newoffer.end = item.EndDate;
                                Newoffer.itemId = itemMaster.ItemId;
                                Newoffer.IsDeleted = false;
                                Newoffer.IsActive = true;
                                Newoffer.OfferLogoUrl = "";
                                Newoffer.CreatedDate = indianTime;
                                Newoffer.UpdateDate = indianTime;
                                Newoffer.OfferCode = "";
                                Newoffer.CityId = itemMaster.Cityid;
                                Newoffer.Description = "";
                                Newoffer.DiscountPercentage = 0;
                                if (!string.IsNullOrEmpty(item.OfferName))
                                {
                                    Newoffer.OfferName = item.OfferName;
                                }
                                else
                                {
                                    Newoffer.OfferName = itemMaster.itemname + " Freebies";
                                }
                                if (!string.IsNullOrEmpty(item.OfferAppType))
                                {
                                    Newoffer.OfferAppType = item.OfferAppType;
                                }
                                else
                                {
                                    Newoffer.OfferAppType = "Both";
                                }
                                Newoffer.OfferWithOtherOffer = false;
                                Newoffer.BillDiscountOfferOn = "FreeItem";
                                Newoffer.IsMultiTimeUse = false;
                                Newoffer.IsUseOtherOffer = false;
                                Newoffer.OfferOn = "Item";
                                Newoffer.FreeOfferType = "ItemMaster";
                                Newoffer.FreeItemLimit = item.QtyAvaiable; // add Item limit                                           
                                Newoffer.OffersaleQty = 0;
                                Newoffer.BillAmount = 0;
                                Newoffer.BillDiscountType = "items";
                                Newoffer.FreeItemId = freeitem.ItemId;
                                Newoffer.FreeItemMRP = freeitem.MRP;
                                Newoffer.FreeItemName = freeitem.itemname;
                                Newoffer.FreeWalletPoint = 0;
                                Newoffer.IsDispatchedFreeStock = item.IsFreeStock;
                                Newoffer.IsOfferOnCart = false;
                                Newoffer.ItemNumber = item.Itemnumber;
                                Newoffer.LineItem = 0;
                                Newoffer.MaxBillAmount = 0;
                                Newoffer.MaxDiscount = 0;
                                Newoffer.MaxQtyPersonCanTake = 0;
                                Newoffer.MinOrderQuantity = item.MinimumOrderQty;
                                Newoffer.NoOffreeQuantity = item.NoOfFreeitemQty;
                                Newoffer.OfferCategory = "Offer";
                                Newoffer.QtyAvaiable = Convert.ToDouble(item.QtyAvaiable);
                                Newoffer.QtyConsumed = 0;
                                offers.Add(Newoffer);
                                item.ItemId += string.IsNullOrEmpty(item.ItemId) ? itemMaster.ItemId.ToString() : "," + itemMaster.ItemId.ToString();
                            }
                        }

                    }

                    context.OfferDb.AddRange(offers);
                    if (context.Commit() > 0)
                    {
                        List<SellerSubCatOffer> AddSellerSubCatOffer = new List<SellerSubCatOffer>();

                        foreach (var offerdb in offers)
                        {
                            // add mapping of seller subcat Offer
                            AddSellerSubCatOffer.Add(new SellerSubCatOffer
                            {
                                SubCatId = SubCatId,
                                CityId = offerdb.CityId,
                                OfferId = offerdb.OfferId
                            });

                            string code = "";
                            code = "ID_";

                            string offerCode = code + offerdb.OfferId;
                            offerdb.OfferCode = offerCode;

                            List<ItemMaster> itemnumber = new List<ItemMaster>();
                            if (offerdb.OfferOn == "Item")
                            {
                                itemMaster = itemMasters.FirstOrDefault(x => x.ItemId == offerdb.itemId);
                                if (offerdb.OfferAppType != "Distributor App")
                                {
                                    itemnumber = context.itemMasters.Where(x => x.Number == itemMaster.Number && x.ItemMultiMRPId == itemMaster.ItemMultiMRPId && x.WarehouseId == offerdb.WarehouseId && x.Deleted == false).ToList();

                                    if (itemnumber.Count != 0)
                                    {
                                        foreach (var editItemMaster in itemnumber)
                                        {
                                            editItemMaster.IsOffer = true;
                                            editItemMaster.OfferCategory = 1;
                                            editItemMaster.OfferStartTime = offerdb.start;
                                            editItemMaster.OfferEndTime = offerdb.end;
                                            editItemMaster.OfferQtyAvaiable = offerdb.QtyAvaiable;
                                            editItemMaster.OfferQtyConsumed = offerdb.QtyConsumed;
                                            editItemMaster.OfferId = offerdb.OfferId;
                                            editItemMaster.OfferWalletPoint = offerdb.FreeWalletPoint;
                                            editItemMaster.OfferType = offerdb.FreeOfferType;
                                            editItemMaster.OfferFreeItemId = offerdb.FreeItemId;
                                            editItemMaster.OfferPercentage = offerdb.DiscountPercentage;
                                            editItemMaster.OfferMinimumQty = offerdb.MinOrderQuantity;
                                            editItemMaster.OfferFreeItemName = offerdb.FreeItemName;
                                            editItemMaster.OfferFreeItemQuantity = offerdb.NoOffreeQuantity;
                                            editItemMaster.ModifiedBy = userId;
                                            editItemMaster.UpdatedDate = indianTime;
                                            if (offerdb.FreeItemId > 0 && offerdb.FreeOfferType == "ItemMaster")
                                            {
                                                ItemMasterCentral imageitem = context.ItemMasterCentralDB.Where(x => x.SellingSku == freeitem.SellingSku).FirstOrDefault();
                                                editItemMaster.OfferFreeItemImage = imageitem != null ? imageitem.LogoUrl : "";
                                            }
                                            context.Entry(editItemMaster).State = EntityState.Modified;
                                        }
                                    }
                                }
                                else
                                {
                                    ItemMaster warehousefreeitem = new ItemMaster();
                                    var imageitem = "";
                                    if (offerdb.FreeOfferType == "ItemMaster")
                                    {
                                        imageitem = context.ItemMasterCentralDB.Where(x => x.SellingSku == freeitem.SellingSku).Select(x => x.LogoUrl).FirstOrDefault();
                                    }

                                    OfferFreeItem offerFreeItem = new OfferFreeItem
                                    {
                                        FreeItemId = offerdb.FreeItemId,
                                        ItemNumber = offerdb.ItemNumber,
                                        OfferFreeItemImage = imageitem,
                                        OfferFreeItemName = offerdb.FreeItemName,
                                        OfferFreeItemQuantity = offerdb.NoOffreeQuantity,
                                        OfferMinimumQty = offerdb.MinOrderQuantity,
                                        OfferQtyAvaiable = Convert.ToDouble(offerdb.FreeItemLimit),
                                        OfferQtyConsumed = offerdb.QtyConsumed,
                                        OfferType = offerdb.FreeOfferType,
                                        OfferWalletPoint = offerdb.FreeWalletPoint
                                    };

                                    if (offerdb.OfferFreeItems == null)
                                        offerdb.OfferFreeItems = new List<GenricEcommers.Models.OfferFreeItem>();

                                    offerdb.OfferFreeItems.Add(offerFreeItem);
                                }

                            }

                            context.Entry(offerdb).State = EntityState.Modified;
                            if (AddSellerSubCatOffer.Any())
                                context.SellerSubCatOffers.AddRange(AddSellerSubCatOffer);
                            var Uploader = FreebiesUploader.FirstOrDefault(x => x.Itemnumber == offerdb.ItemNumber && x.CityId == offerdb.CityId);
                            if (Uploader != null)
                                Uploader.OfferId += string.IsNullOrEmpty(Uploader.OfferId) ? offerdb.OfferId.ToString() : "," + offerdb.OfferId.ToString();
                        }
                        if (context.Commit() > 0)
                        {
                            result = true;
                        }
                        else
                        {
                            result = false;
                            foreach (var offerdb in offers)
                            {
                                var Uploader = FreebiesUploader.FirstOrDefault(x => x.Itemnumber == offerdb.ItemNumber && x.CityId == offerdb.CityId);
                                if (Uploader != null)
                                    Uploader.OfferId = "";
                                offerdb.IsActive = false;
                                offerdb.IsDeleted = true;
                                context.Entry(offerdb).State = EntityState.Modified;
                            }
                            context.Commit();
                        }

                    }
                    else
                    {
                        result = false;
                    }
                }
                catch (Exception ex)
                {
                    FreebiesUploader.ForEach(x => x.Error = ex.ToString());
                    result = false;
                }
#if !DEBUG
                if (result)
                {

                    foreach (var Offer in offers)
                    {
                        if (Offer.IsActive && Offer.end > DateTime.Now)
                        {

                            var jobId = BackgroundJob.Schedule(
                                      () => ActiveDeativeofferByJob(Offer.OfferId),
                            TimeSpan.FromMinutes((Offer.end - DateTime.Now).TotalMinutes));
                            MongoDbHelper<ScheduleJobHistory> mongoDbHelper = new MongoDbHelper<ScheduleJobHistory>();
                            ScheduleJobHistory scheduleJobHistory = new ScheduleJobHistory
                            {
                                CreatedDate = DateTime.Now,
                                isActive = true,
                                jobid = jobId,
                                ObjectId = Offer.OfferId.ToString(),
                                ObjectType = "Offer"
                            };
                            mongoDbHelper.Insert(scheduleJobHistory);
                        }

                    }
                }
#endif

            }
            result = true;

            return FreebiesUploader;
        }

        [Route("GetOfferDetailById")]
        [HttpGet]
        public OfferSectionDetail GetOfferDetailById(int offerId)
        {
            OfferSectionDetail offerDetails = new OfferSectionDetail();
            using (AuthContext context = new AuthContext())
            {
                if (context.Database.Connection.State != System.Data.ConnectionState.Open)
                    context.Database.Connection.Open();

                System.Data.DataTable IdDt = new System.Data.DataTable();
                IdDt.Columns.Add("IntValue");

                var dr = IdDt.NewRow();
                dr["IntValue"] = offerId;
                IdDt.Rows.Add(dr);


                var param = new SqlParameter("offerIds", IdDt);
                param.SqlDbType = System.Data.SqlDbType.Structured;
                param.TypeName = "dbo.IntValues";
                var cmd = context.Database.Connection.CreateCommand();

                cmd.CommandText = "[dbo].[GetOfferById]";
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.Parameters.Add(param);


                var reader = cmd.ExecuteReader();
                var Offer = ((IObjectContextAdapter)context)
                .ObjectContext
                .Translate<OfferminDc>(reader).FirstOrDefault();
                offerDetails.OfferType = Offer != null ? Offer.BillDiscountType : "";
                reader.NextResult();
                if (reader.HasRows)
                {
                    offerDetails.IncludeSections = ((IObjectContextAdapter)context)
                                            .ObjectContext
                                            .Translate<IncludeSection>(reader).ToList();
                }

                reader.NextResult();
                if (reader.HasRows)
                {
                    offerDetails.ExcludeSections = ((IObjectContextAdapter)context)
                                              .ObjectContext
                                              .Translate<ExcludeSection>(reader).ToList();
                }

                reader.NextResult();
                if (reader.HasRows)
                {
                    offerDetails.BillDiscountRequiredItemDcs = ((IObjectContextAdapter)context)
                                              .ObjectContext
                                              .Translate<DataContracts.Masters.BillDiscountRequiredItemDc>(reader).ToList();
                }


                return offerDetails;
            }
        }


        #region CRM  Offer


        [Route("SearchOfferForCRM")]
        [HttpPost]
        [AllowAnonymous]
        public async Task<List<OfferForCRMDc>> SearchOfferForCRM(SearchOfferForCRMDc SearchOfferForCRM)
        {
            var result = new List<OfferForCRMDc>();
            if (SearchOfferForCRM != null && SearchOfferForCRM.Keyword != null)
            {
                using (AuthContext context = new AuthContext())
                {
                    var param = new SqlParameter("@Keyword", SearchOfferForCRM.Keyword);
                    context.Database.CommandTimeout = 300;
                    result = await context.Database.SqlQuery<OfferForCRMDc>("Exec GetOfferForCRM @Keyword", param).ToListAsync();
                }
            }
            return result;
        }

        [Route("SearchNotificationForCRM")]
        [HttpPost]
        [AllowAnonymous]
        public async Task<List<NotificationForCRMDc>> SearchNotificationForCRM(SearchNoticationForCRMDc SearchNoticationForCRM)
        {
            var result = new List<NotificationForCRMDc>();
            if (SearchNoticationForCRM != null && SearchNoticationForCRM.Keyword != null)
            {
                using (AuthContext context = new AuthContext())
                {
                    var param = new SqlParameter("@Keyword", SearchNoticationForCRM.Keyword);
                    context.Database.CommandTimeout = 300;
                    result = await context.Database.SqlQuery<NotificationForCRMDc>("Exec GetNotificationForCRM @Keyword", param).ToListAsync();
                }
            }
            return result;
        }


        //[Route("AddCRMOffer")]
        //[HttpPost]
        //[AllowAnonymous]
        //public async Task<ResponseMsg> AddCRMOffer(AddCRMOfferDc AddCRMOffer)
        //{
        //    var identity = User.Identity as ClaimsIdentity;
        //    int compid = 0, userid = 0;

        //    if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
        //        userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

        //    if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))
        //        compid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);


        //    var result = new ResponseMsg();
        //    if (AddCRMOffer != null && AddCRMOffer.GroupName != null && AddCRMOffer.GroupValue != null)
        //    {
        //        using (AuthContext context = new AuthContext())
        //        {
        //            var categoryid = 0;
        //            var subcategoryid = 0;
        //            var subsubcategoryid = 0;
        //            var BillDiscountType = "BillDiscount";
        //            var offerCode = "PlatFrom";
        //            if (AddCRMOffer.GroupName == "CategoryName")
        //            {
        //                categoryid = context.Categorys.FirstOrDefault(x => x.CategoryName == AddCRMOffer.GroupValue && x.IsActive && !x.Deleted).Categoryid;
        //                BillDiscountType = "category";
        //                offerCode = AddCRMOffer.GroupValue;
        //            }
        //            else if (AddCRMOffer.GroupName == "CampanyName")
        //            {
        //                subcategoryid = context.SubCategorys.FirstOrDefault(x => x.SubcategoryName == AddCRMOffer.GroupValue && x.IsActive && !x.Deleted).SubCategoryId;
        //                BillDiscountType = "subcategory";
        //                offerCode = AddCRMOffer.GroupValue;
        //            }
        //            else if (AddCRMOffer.GroupName == "BrandName")
        //            {
        //                subsubcategoryid = context.SubsubCategorys.FirstOrDefault(x => x.SubsubcategoryName == AddCRMOffer.GroupValue && x.IsActive && !x.Deleted).SubsubCategoryid;
        //                BillDiscountType = "brand";
        //                offerCode = AddCRMOffer.GroupValue;
        //            }

        //            bool IsExists = context.OfferDb.Any(x => x.OfferName == AddCRMOffer.OfferCode && x.BillDiscountType == BillDiscountType && !x.IsDeleted && x.IsActive);
        //            if (!IsExists)
        //            {

        //                List<Offer> offers = new List<Offer>();
        //                List<Warehousedto> warehouses = context.Warehouses.Where(y => y.Deleted == false && y.active == true && (y.IsKPP == false || y.IsKppShowAsWH == true)).Select(y => new Warehousedto { WarehouseId = y.WarehouseId, CityId = y.Cityid, WarehouseName = y.WarehouseName }).ToList();
        //                foreach (var warehouse in warehouses)
        //                {
        //                    Offer Newoffer = new Offer();
        //                    Newoffer.IsCRMOffer = true;
        //                    Newoffer.CompanyId = compid;
        //                    Newoffer.StoreId = 0;
        //                    Newoffer.userid = userid;
        //                    Newoffer.WarehouseId = warehouse.WarehouseId;
        //                    Newoffer.ApplyType = "Customer";
        //                    Newoffer.start = DateTime.Now;
        //                    Newoffer.IsDeleted = false;
        //                    Newoffer.IsActive = true;
        //                    Newoffer.end = DateTime.Now.AddYears(2);
        //                    Newoffer.CreatedDate = indianTime;
        //                    Newoffer.UpdateDate = indianTime;
        //                    Newoffer.IsAutoApply = false;
        //                    Newoffer.IsMultiTimeUse = false;
        //                    Newoffer.IsUseOtherOffer = true;
        //                    Newoffer.OfferOn = "BillDiscount";
        //                    Newoffer.OfferUseCount = 1;
        //                    Newoffer.CityId = warehouse.CityId ?? 0;
        //                    Newoffer.OfferAppType = "Retailer App";
        //                    Newoffer.ApplyOn = "PreOffer";
        //                    Newoffer.OfferName = AddCRMOffer.OfferCode; //"CRM_" + DateTime.Now.Year + "_BDOffer_" + AddCRMOffer.GroupName;
        //                    Newoffer.OfferCode = AddCRMOffer.OfferCode;
        //                    Newoffer.Description = "CRM Digital Offer";
        //                    Newoffer.FreeOfferType = "BillDiscount";
        //                    Newoffer.BillDiscountOfferOn = "Percentage";
        //                    Newoffer.DiscountPercentage = AddCRMOffer.OfferValue;
        //                    Newoffer.BillDiscountType = BillDiscountType;
        //                    Newoffer.OfferCategory = "Offer";
        //                    Newoffer.Category = categoryid;
        //                    Newoffer.subCategory = subcategoryid;
        //                    Newoffer.subSubCategory = subsubcategoryid;
        //                    Newoffer.BillAmount = 1000;
        //                    Newoffer.LineItem = 1;
        //                    Newoffer.MaxBillAmount = 2000; //1000                            
        //                    offers.Add(Newoffer);
        //                }
        //                if (offers != null && offers.Any())
        //                {
        //                    context.OfferDb.AddRange(offers);
        //                    result.Status = context.Commit() > 0;
        //                    foreach (var item in offers)
        //                    {
        //                        if (AddCRMOffer.GroupName == "CategoryName")
        //                        {
        //                            context.BillDiscountOfferSectionDB.Add(new BillDiscountOfferSection
        //                            {
        //                                ObjId = categoryid,
        //                                IsInclude = true,
        //                                OfferId = item.OfferId,
        //                                WarehouseId = item.WarehouseId
        //                            });
        //                        }
        //                        else if (AddCRMOffer.GroupName == "CampanyName")
        //                        {
        //                            var SubCategoryMappingIds = context.SubcategoryCategoryMappingDb.Where(x => x.SubCategoryId == subcategoryid && x.IsActive && !x.Deleted).Select(x => x.SubCategoryMappingId).Distinct().ToList();
        //                            foreach (var subcatMP in SubCategoryMappingIds)
        //                            {
        //                                context.BillDiscountOfferSectionDB.Add(new BillDiscountOfferSection
        //                                {
        //                                    ObjId = subcatMP,
        //                                    IsInclude = true,
        //                                    OfferId = item.OfferId,
        //                                    WarehouseId = item.WarehouseId
        //                                });
        //                            }
        //                        }
        //                        else if (AddCRMOffer.GroupName == "BrandName")
        //                        {
        //                            var BrandCategoryMappingIds = context.BrandCategoryMappingDb.Where(x => x.SubsubCategoryId == subsubcategoryid && x.IsActive && !x.Deleted).Select(x => x.BrandCategoryMappingId).Distinct().ToList();
        //                            foreach (var brandMP in BrandCategoryMappingIds)
        //                            {
        //                                context.BillDiscountOfferSectionDB.Add(new BillDiscountOfferSection
        //                                {
        //                                    ObjId = brandMP,
        //                                    IsInclude = true,
        //                                    OfferId = item.OfferId,
        //                                    WarehouseId = item.WarehouseId
        //                                });
        //                            }
        //                        }

        //                        item.OfferCode = "CR_" + item.OfferId;
        //                        context.Entry(item).State = EntityState.Modified;
        //                    }
        //                    context.Commit();
        //                    result.Message = "Offer Created Successfully";
        //                }
        //            }
        //            else
        //            {
        //                result.Status = false;
        //                result.Message = "Offer already created for OfferCode : " + AddCRMOffer.OfferCode;
        //            }
        //        }
        //    }
        //    else
        //    {
        //        result.Status = false;
        //        result.Message = "Something went wrong";
        //    }
        //    return result;
        //}

        [Route("ProvideCRMOfferToCustomer")]
        [HttpPost]
        [AllowAnonymous]
        public async Task<ResponseMsg> ProvideCRMOfferToCustomer(string filePath)
        {
            var result = new ResponseMsg();
            if (!string.IsNullOrEmpty(filePath))
            {
                string fileName = "Offer_" + DateTime.Today.ToString("yy-MM-dd_") + Guid.NewGuid().ToString() + ".json";

                string path = Path.Combine(HttpContext.Current.Server.MapPath("~/CRMOfferAndNotificationJsons"), fileName);

                HttpClient client = new HttpClient();
                var response = await client.GetAsync(filePath);
                using (var fs = new FileStream(path, FileMode.CreateNew))
                {
                    await response.Content.CopyToAsync(fs);
                }
                //string currentPath = HttpContext.Current.Server.MapPath("~/Downloads/offer.json");
                string allText = System.IO.File.ReadAllText(path);
                List<CustomerCRMOfferDc> CustomerCRMOffers = new List<CustomerCRMOfferDc>();
                if (!string.IsNullOrEmpty(allText))
                {
                    CustomerCRMOffers = JsonConvert.DeserializeObject<List<CustomerCRMOfferDc>>(allText);
                }
                //-------------------------------------------------------------------------------------------------------------
                if (CustomerCRMOffers != null && CustomerCRMOffers.Any() && CustomerCRMOffers.Any(x => x.OfferCode != null))
                {
                    using (AuthContext context = new AuthContext())
                    {
                        var offercodes = CustomerCRMOffers.Select(x => x.OfferCode).Distinct().ToList();
                        var skcodeslist = CustomerCRMOffers.SelectMany(x => x.Skcodes).ToList();
                        var skcodes = skcodeslist.Select(x => x.SkCode).Distinct().ToList();

                        TextFileLogHelper.TraceLog("start ProvideCRMOfferToCustomer skcode count:" + Convert.ToString(skcodes.Count()));

                        var IdDt = new System.Data.DataTable();
                        IdDt = new System.Data.DataTable();
                        IdDt.Columns.Add("StringValue");

                        foreach (var item in skcodes)
                        {
                            var dr = IdDt.NewRow();
                            dr["StringValue"] = item;
                            IdDt.Rows.Add(dr);
                        }
                        var param = new SqlParameter("Skcodes", IdDt);
                        param.SqlDbType = System.Data.SqlDbType.Structured;

                        param.TypeName = "dbo.stringValues";
                        context.Database.CommandTimeout = 600;
                        var Customers = context.Database.SqlQuery<CustomerCRMSearchDc>("Exec GetCustomerCRMSearch @Skcodes", param).ToList();

                        TextFileLogHelper.TraceLog("Customers count: " + Customers != null ? Convert.ToString(Customers.Count()) : Convert.ToString(0));

                        var wids = Customers.Select(x => x.Warehouseid).Distinct().ToList();
                        var offers = context.OfferDb.Where(x => offercodes.Contains(x.OfferCode) && wids.Contains(x.WarehouseId) && x.IsCRMOffer).ToList();
                        var offerids = offers.Select(x => x.OfferId).ToList();

                        var AlreadyBilldiscountlist = context.BillDiscountDb.Where(x => offerids.Contains(x.OfferId) && x.OrderId > 0).Select(x => new { x.OfferId, x.CustomerId, x.start, x.end }).ToList();
                        TextFileLogHelper.TraceLog("Customers count: " + AlreadyBilldiscountlist != null ? Convert.ToString(AlreadyBilldiscountlist.Count()) : Convert.ToString(0));

                        context.Database.CommandTimeout = 900;
                        await context.Database.ExecuteSqlCommandAsync("exec RemoveAllCRMOffer");

                        List<BillDiscount> BillDiscounts = new List<BillDiscount>();
                        foreach (var CustomerCRMOffer in CustomerCRMOffers)
                        {
                            if (CustomerCRMOffer != null && CustomerCRMOffer.Skcodes != null && CustomerCRMOffer.Skcodes.Any())
                            {
                                foreach (var item in CustomerCRMOffer.Skcodes)
                                {
                                    var CustomerId = Customers.FirstOrDefault(x => x.Skcode == item.SkCode)?.CustomerId;
                                    if (CustomerId.HasValue && CustomerId > 0)
                                    {

                                        var whoffer = offers.FirstOrDefault(z => z.OfferCode == CustomerCRMOffer.OfferCode && z.WarehouseId == Customers.FirstOrDefault(c => c.CustomerId == CustomerId).Warehouseid);

                                        if (CustomerId.HasValue && CustomerId > 0 && whoffer != null && !AlreadyBilldiscountlist.Any(x => x.CustomerId == CustomerId && x.OfferId == whoffer.OfferId && (x.start <= item.StartDate && x.end >= item.StartDate)))
                                        {
                                            BillDiscounts.Add(
                                                new BillDiscount
                                                {
                                                    CustomerId = CustomerId.Value,
                                                    OrderId = 0,
                                                    OfferId = whoffer.OfferId,
                                                    BillDiscountType = whoffer.OfferOn,
                                                    BillDiscountAmount = 0,
                                                    IsMultiTimeUse = whoffer.IsMultiTimeUse,
                                                    IsUseOtherOffer = whoffer.IsUseOtherOffer,
                                                    CreatedDate = indianTime,
                                                    ModifiedDate = indianTime,
                                                    IsActive = whoffer.IsActive,
                                                    IsDeleted = false,
                                                    CreatedBy = 1,
                                                    ModifiedBy = 1,
                                                    IsScratchBDCode = false,
                                                    Category = whoffer.Category,
                                                    Subcategory = whoffer.subCategory,
                                                    subSubcategory = whoffer.subSubCategory,
                                                    start = item.StartDate,
                                                    end = item.EndDate,
                                                });
                                        }
                                    }
                                }
                            }
                        }
                        if (BillDiscounts.Any())
                        {
                            TextFileLogHelper.TraceLog("BillDiscountsCustomers bulk insert start cust count: " + BillDiscounts != null ? Convert.ToString(BillDiscounts.Count()) : Convert.ToString(0));

                            var BillDiscountsCustomers = new BulkOperations();
                            BillDiscountsCustomers.Setup<BillDiscount>(x => x.ForCollection(BillDiscounts))
                                .WithTable("BillDiscounts")
                                .WithBulkCopyBatchSize(4000)
                                .WithBulkCopyCommandTimeout(720) // Default is 600 seconds
                                .WithSqlCommandTimeout(720) // Default is 600 seconds
                                .AddAllColumns()
                                .BulkInsert();
                            BillDiscountsCustomers.CommitTransaction("AuthContext");
                            result.Status = true;

                            TextFileLogHelper.TraceLog("BillDiscountsCustomers bulk insert end cust count: " + BillDiscounts != null ? Convert.ToString(BillDiscounts.Count()) : Convert.ToString(0));
                        }
                    }
                }
                else { result.Message = "Something went wrong"; }
                TextFileLogHelper.TraceLog("ProvideCRMOfferToCustomer return response status: " + Convert.ToString(result.Status));
                TextFileLogHelper.TraceLog("ProvideCRMOfferToCustomer return response  Message: " + result.Message);
                return result;
            }
            else
            {
                result.Message = "File Path not Found";
                result.Status = false;
                return result;
            }
        }


        [Route("ProvideCRMNotificationToCustomer")]
        [HttpPost]
        [AllowAnonymous]
        public async Task<ResponseMsg> ProvideCRMNotificationToCustomer(string filePath)
        {
            var result = new ResponseMsg();
            if (!string.IsNullOrEmpty(filePath))
            {
                string fileName = "Notification_" + DateTime.Today.ToString("yy-MM-dd_") + Guid.NewGuid().ToString() + ".json";

                string path = Path.Combine(HttpContext.Current.Server.MapPath("~/CRMOfferAndNotificationJsons"), fileName);

                HttpClient client = new HttpClient();
                var response = await client.GetAsync(filePath);
                using (var fs = new FileStream(path, FileMode.CreateNew))
                {
                    await response.Content.CopyToAsync(fs);
                }
                string allText = System.IO.File.ReadAllText(path);
                List<CustomerCRMNotificationDc> CustomerCRMNotification = new List<CustomerCRMNotificationDc>();
                if (!string.IsNullOrEmpty(allText))
                {
                    CustomerCRMNotification = JsonConvert.DeserializeObject<List<CustomerCRMNotificationDc>>(allText);
                }
                //------------------------------------------------------------------------------------------------------------
                if (CustomerCRMNotification != null && CustomerCRMNotification.Any())
                {
                    using (AuthContext context = new AuthContext())
                    {
                        foreach (var items in CustomerCRMNotification)
                        {
                            if (items != null && items.Skcodes != null && items.Skcodes.Any())
                            {
                                var IdDt = new System.Data.DataTable();
                                IdDt = new System.Data.DataTable();
                                IdDt.Columns.Add("StringValue");
                                foreach (var item in items.Skcodes)
                                {
                                    var dr = IdDt.NewRow();
                                    dr["StringValue"] = item;
                                    IdDt.Rows.Add(dr);
                                }
                                var param = new SqlParameter("Skcodes", IdDt);
                                param.SqlDbType = System.Data.SqlDbType.Structured;
                                param.TypeName = "dbo.stringValues";
                                context.Database.CommandTimeout = 300;
                                var Customers = await context.Database.SqlQuery<CustomerCRMSearchDc>("Exec GetCustomerCRMSearch @Skcodes", param).ToListAsync();
                                if (Customers != null && Customers.Any())
                                {
                                    NotificationScheduler NotificationSch = new NotificationScheduler();
                                    NotificationSch.StartDate = items.StartDate;
                                    NotificationSch.EndDate = items.EndDate;
                                    NotificationSch.NotificationUpdatedId = items.NotificationId;
                                    NotificationSch.Sent = false;
                                    NotificationSch.TotalSent = 0;
                                    NotificationSch.TotalNotSent = 0;
                                    context.NotificationSchedulers.Add(NotificationSch);
                                    await context.CommitAsync();
                                    var cRMNotifiationCustomers = Customers.Select(x => new CRMNotifiationCustomer
                                    {
                                        NotificationId = items.NotificationId,
                                        CustomerId = x.CustomerId,
                                        IsActive = true,
                                        IsDeleted = false,
                                        CreatedBy = 1,
                                        CreatedDate = DateTime.Now,
                                        IsSent = false,
                                        SchedulerId = NotificationSch.Id
                                    }).Distinct().ToList();
                                    context.CRMNotifiationCustomers.AddRange(cRMNotifiationCustomers);
                                    await context.CommitAsync();
                                    result.Status = true;
                                }
                            }
                        }
                    }
                }
                else
                {
                    result.Status = false;
                    result.Message = "Something went wrong";
                }
                return result;
            }
            else
            {
                result.Message = "File Path not Found";
                result.Status = false;
                return result;
            }
        }

        [Route("ProvideCRMWhatsAppNotificationToCustomer")]
        [HttpPost]
        [AllowAnonymous]
        public async Task<ResponseMsg> ProvideCRMWhatsAppNotificationToCustomer(string filePath)
        {
            var result = new ResponseMsg();
            if (!string.IsNullOrEmpty(filePath))
            {
                string fileName = "Whatsapp_" + DateTime.Today.ToString("yy-MM-dd_") + Guid.NewGuid().ToString() + ".json";

                string path = Path.Combine(HttpContext.Current.Server.MapPath("~/CRMOfferAndNotificationJsons"), fileName);

                HttpClient client = new HttpClient();
                var response = await client.GetAsync(filePath);
                using (var fs = new FileStream(path, FileMode.CreateNew))
                {
                    await response.Content.CopyToAsync(fs);
                }

                string allText = System.IO.File.ReadAllText(path);
                List<CustomerCRMWhatsappNotificationDc> CustomerCRMNotification = new List<CustomerCRMWhatsappNotificationDc>();
                if (!string.IsNullOrEmpty(allText))
                {
                    CustomerCRMNotification = JsonConvert.DeserializeObject<List<CustomerCRMWhatsappNotificationDc>>(allText);
                }
                //------------------------------------------------------------------------------------------------------------
                if (CustomerCRMNotification != null && CustomerCRMNotification.Any())
                {
                    var query = @"select CustomerId as CustomerID, Skcode as Skcode  from Customers where Skcode IN (#Placeholder#)";

                    var skCodeListOfList = CustomerCRMNotification.Select(x => x.Skcodes).ToList();
                    List<string> skCodeList = new List<string>();
                    if (skCodeListOfList != null && skCodeListOfList.Any())
                    {
                        foreach (var item in skCodeListOfList)
                        {
                            skCodeList.AddRange(item.Distinct());
                        }
                        skCodeList = skCodeList.Distinct().ToList();
                    }

                    string skCodeString = string.Join(",", skCodeList.Select(x => "'" + x + "'"));
                    query = query.Replace("#Placeholder#", skCodeString);



                    using (AuthContext context = new AuthContext())
                    {
                        List<CRMWhatsappMessage> cRMWhatsappMessagesList = new List<CRMWhatsappMessage>();
                        List<WhatsappSkcode> whatsappSkcodeList
                             = context.Database.SqlQuery<WhatsappSkcode>(query).ToList();

                        if (whatsappSkcodeList != null && whatsappSkcodeList.Any() && CustomerCRMNotification != null && CustomerCRMNotification.Any())
                        {
                            foreach (var item in CustomerCRMNotification)
                            {
                                if (item.Skcodes != null && item.Skcodes.Any())
                                {
                                    foreach (var skcode in item.Skcodes)
                                    {
                                        var tempSkCode = whatsappSkcodeList.Where(x => x.Skcode == skcode).FirstOrDefault();
                                        if (tempSkCode != null)
                                        {
                                            cRMWhatsappMessagesList.Add(new CRMWhatsappMessage
                                            {
                                                CreatedBy = 0,
                                                CreatedDate = DateTime.Now,
                                                CustomerID = tempSkCode.CustomerID,
                                                Skcode = tempSkCode.Skcode,
                                                IsActive = true,
                                                IsDeleted = false,
                                                IsMessageApiHit = false,
                                                ModifiedBy = null,
                                                ModifiedDate = null,
                                                ScheduleEndTime = item.EndDate,
                                                ScheduleStartTime = item.StartDate,
                                                SendTime = null,
                                                TemplateId = item.WhatsappTemplateId.Value
                                            });
                                        }
                                    }
                                }
                            }
                            if (cRMWhatsappMessagesList != null && cRMWhatsappMessagesList.Any())
                            {
                                context.CRMWhatsappMessages.AddRange(cRMWhatsappMessagesList);
                                context.Commit();
                                result.Status = true;
                                result.Message = "Success";
                                return result;
                            }
                        }
                    }
                }
                else
                {
                    result.Status = false;
                    result.Message = "Something went wrong";
                }
                return result;
            }
            else
            {
                result.Message = "File Path not Found";
                result.Status = false;
                return result;
            }
        }


        [Route("WhatsAppNotificationToCustomerSchedular")]
        [HttpPost]
        [AllowAnonymous]
        public async Task<ResponseMsg> WhatsAppNotificationToCustomerSchedular()
        {
            using (AuthContext context = new AuthContext())
            {

                var list = context.CRMWhatsappMessages.Where(x => x.IsMessageApiHit == false && x.ScheduleStartTime < DateTime.Now && x.ScheduleEndTime > DateTime.Now && x.IsActive == true && x.IsDeleted == false).OrderBy(x => x.TemplateId).ToList();
                if (list != null && list.Any())
                {
                    int page = 0;
                    var listPart = list.GroupBy(x => x.TemplateId);
                    foreach (var item in listPart)
                    {
                        WhatsAppTemplateManager whatsAppTemplateManager = new WhatsAppTemplateManager();
                        await whatsAppTemplateManager.WhatsAppAPIPostBulkData((int)item.Key, item.Select(x => x.Skcode).ToList(), HttpContext.Current.Server.MapPath("~/UploadedWhatsAppExcel"));

                        item.ToList();

                        foreach (var message in item.ToList())
                        {
                            message.SendTime = DateTime.Now;
                            message.IsMessageApiHit = true;

                            context.Entry(message).State = EntityState.Modified;
                        }

                        context.Commit();
                    }

                }


            }

            return new ResponseMsg
            {
                Message = "Done",
                Status = true
            };
        }

        private string GenerateRandomCRM(int Length, string[] saAllowedCharacters)
        {
            using (AuthContext db = new AuthContext())
            {
                string Code = String.Empty;
                string sTempChars = String.Empty;
                Random rand = new Random();

                for (int i = 0; i < Length; i++)
                {
                    int p = rand.Next(0, saAllowedCharacters.Length);
                    sTempChars = saAllowedCharacters[rand.Next(0, saAllowedCharacters.Length)];
                    Code += sTempChars;
                }
                return Code;
            }
        }
        #endregion


        [Route("Addnewexclusivegroup")]
        [HttpGet]
        [AllowAnonymous]
        public async Task<ResponseMsg> Addnewexclusivegroup(string name)
        {
            using (var db = new AuthContext())
            {
                var result = new ResponseMsg
                {
                    Message = "Done",
                    Status = true
                };
                int userid = 0;
                var identity = User.Identity as ClaimsIdentity;
                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                    userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

                var isdataexists = db.ExclusiveOfferGroups.Where(x => x.Name == name).FirstOrDefault();
                if (isdataexists != null)
                {
                    result.Status = false;
                    result.Message = "Group Name Already Exists";
                    return result;
                }
                else
                {
                    ExclusiveOfferGroup exc = new ExclusiveOfferGroup();
                    exc.Name = name;
                    exc.CreatedBy = userid;
                    exc.CreatedDate = DateTime.Now;
                    exc.IsDeleted = false;
                    exc.IsActive = true;
                    db.ExclusiveOfferGroups.Add(exc);
                    if (db.Commit() > 0)
                    {
                        result.Status = true;
                        result.Message = "Group Created Successfully";
                        return result;
                    }
                    else
                    {
                        result.Status = false;
                        result.Message = "Something Went wrong";
                        return result;
                    }
                }
                return result;
            }
        }

        [Route("GetAllExclusivegroup")]
        [HttpGet]
        [AllowAnonymous]
        public List<ExclusiveOfferGroup> GetAllExclusivegroup()
        {
            using (var db = new AuthContext())
            {
                List<ExclusiveOfferGroup> data = db.ExclusiveOfferGroups.Where(x => x.IsActive == true && x.IsDeleted == false).ToList();
                return data;
            }
        }

        private List<MinSalesGroup> GetNewCustomerGroupListForOffer(int? StoreId)
        {
            using (var context = new AuthContext())
            {

                if (context.Database.Connection.State != System.Data.ConnectionState.Open)
                    context.Database.Connection.Open();

                if (!StoreId.HasValue)
                    StoreId = -2;
                var param1 = new SqlParameter("@storeId", StoreId.Value);

                var cmd = context.Database.Connection.CreateCommand();
                cmd.CommandText = "[dbo].[Sp_getGroupsForOfferuploader]";
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.Parameters.Add(param1);

                // Run the sproc
                var reader = cmd.ExecuteReader();
                var minSalesGroups = ((IObjectContextAdapter)context)
                 .ObjectContext
                 .Translate<MinSalesGroup>(reader).ToList();

                return minSalesGroups;

            }
        }



    }
    public class CustomerEstimationOfferDetailsDc
    {
        public string OfferCode { get; set; }
        public string CustomerName { get; set; }
        public string Skcode { get; set; }
        public string MobileNo { get; set; }
        public string OrderIds { get; set; }
        public decimal OrderValue { get; set; }
        public decimal DispatchValue { get; set; }
        public double Discount { get; set; }
        public decimal CalculateDiscountvalue { get; set; }
        public string OfferOn { get; set; }
        public int Status { get; set; }
        public int OfferId { get; set; }
        public int CustomerId { get; set; }
        public long Id { get; set; }
        public string Slaboffer { get; set; }
    }
    public class CustomerEstimationOfferDc
    {
        public int OfferId { get; set; }
        public int CustomerId { get; set; }
        public string CustomerName { get; set; }
        public string Skcode { get; set; }
        public string ShopName { get; set; }
        public string Mobile { get; set; }
        public string OrderIds { get; set; }
        public decimal CalculateDiscountvalue { get; set; }
        public decimal ChangeCalculateDiscountValue { get; set; }
        public int CheakerId { get; set; }
        public DateTime CheakerDate { get; set; }
        public int Status { get; set; }
    }
    public class OfferTargetDc
    {
        public List<SlapOffer> SlapOffers { get; set; }
        public List<PostItemOffer> PostItemOffers { get; set; }
    }
    public class SlapOffer
    {
        public double TotalOrderValue { get; set; }
        public double OfferOnPrice { get; set; }
        public double MaxDiscount { get; set; }
        public double Discount { get; set; }
        public double DiscountPercentage { get; set; }
        public string BillDiscountOfferOn { get; set; }
        public double WalletPercentage { get; set; }
        public double? WalletAmount { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
    public class PostItemOffer
    {
        public string itemname { get; set; }
        public string OfferType { get; set; }
        public int? OffersaleQty { get; set; }
        public int? OffersaleWeight { get; set; }
        public int? OffersaleAmount { get; set; }
        public double TotalOrderValue { get; set; }
        public int TotalOrderWeight { get; set; }
        public int TotalOrderQty { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string UOM { get; set; }
        public double MaxDiscount { get; set; }
    }
    public class idd
    {
        public int id { get; set; }
    }

    public class ItemClassificationForOffer
    {

        public int? ItemId { get; set; }
        public string ItemNumber { get; set; }

        public int? ItemMultiMRPId { get; set; }
        public int? WarehouseId { get; set; }
        public string Category { get; set; }
    }

    public class DistributerUpdateDC
    {
        public DateTime start { get; set; }
        public DateTime end { get; set; }
        public bool IsActive { get; set; }
        public int itemId { get; set; }
        public string itemname { get; set; }
        public int WarehouseId { get; set; }
        public string OfferCode { get; set; }

    }
    public class DistributerOfferDc
    {

        public int OfferId { get; set; }
        public string Description
        {
            get; set;
        }
        public int WarehouseId { get; set; }
        public int CityId { get; set; }
        public string OfferName { get; set; }
        public string OfferCode { get; set; }

        public string OfferCategory
        {
            get; set;
        }

        public string FreeOfferType { get; set; }  //Offer or FlashDeal

        public string OfferOn { get; set; }  //Item,Category,Brand ,
        public int MinOrderQuantity { get; set; }
        public int NoOffreeQuantity { get; set; }
        public int FreeItemId { get; set; }
        public DateTime start { get; set; }
        public DateTime end { get; set; }

        public double QtyAvaiable { get; set; }  //This will be application on Flash Deals
        public double QtyConsumed { get; set; }
        public int itemId { get; set; }
        public string itemname { get; set; }


        public string FreeItemName { get; set; }



        public int FreeWalletPoint
        {
            get; set;
        }
        public bool OfferWithOtherOffer
        {
            get; set;
        }
        public double DiscountPercentage
        {
            get; set;
        }

        public double BillAmount { get; set; }  // Bill Amount       
        public double MaxBillAmount { get; set; }
        public double MaxDiscount { get; set; }

        public int LineItem { get; set; }

        public bool IsDeleted { get; set; }
        public bool IsActive { get; set; }

        public DateTime CreatedDate { get; set; }
        public DateTime? UpdateDate { get; set; }


        public string BillDiscountOfferOn
        {
            get; set;
        }

        public double? BillDiscountWallet
        {
            get; set;
        }
        // For Bill Discount use by Anushka  & Harry 17/06/2019
        public int? OffersaleQty { get; set; }
        public string BillDiscountType { get; set; }
        public string OfferAppType { get; set; }
        public string ApplyOn { get; set; }
        public string WalletType { get; set; }
        public bool DistributorOfferType { get; set; }
        public string ItemNumber { get; set; }
        public string UOM { get; set; }
        public decimal DistributorDiscountAmount { get; set; }
        public decimal DistributorDiscountPercentage { get; set; }
        public string ApplyType { get; set; }
        public List<int> ObjectIds { get; set; }
    }
    public class offerdistributerDC
    {
        public int warehouseid { get; set; }
        public string offerOn { get; set; }
        public DateTime? start { get; set; }
        public DateTime? end { get; set; }

    }



    public class DistributerOverViewDc
    {
        //public int offerId { get; set; }


        public string Name { get; set; }


    }
    public class DistributerListDc
    {
        public int offerId { get; set; }
        public string OfferCode { get; set; }
        public string Description { get; set; }
        public string ApplyType { get; set; }
        public string BillDiscountType { get; set; }
        public bool IsActive { get; set; }
        public double? BillDiscountWallet
        {
            get; set;
        }
        public double MaxDiscount { get; set; }
        public string OfferName { get; set; }
        public string itemname { get; set; }
        public int WarehouseId { get; set; }
        public string WarehouseName { get; set; }
        public string OfferOn { get; set; }
        public string FreeOfferType { get; set; }
        public string BillDiscountOfferOn
        {
            get; set;
        }
        public double BillAmount { get; set; }  // Bill Amount       
        public double MaxBillAmount { get; set; }
        public int? OffersaleQty { get; set; }
        public DateTime start { get; set; }
        public DateTime end { get; set; }
        public DateTime CreatedDate { get; set; }
        public double DiscountPercentage
        {
            get; set;
        }
        public int FreeWalletPoint
        {
            get; set;
        }
        public string UOM { get; set; }
        public decimal DistributorDiscountAmount { get; set; }
        public decimal DistributorDiscountPercentage { get; set; }
    }

    public class ware
    {
        public List<idd> ids { get; set; }
    }
    public class OfferList
    {

        public bool IsDispatchedFreeStock
        {
            get; set;
        }
        public int OfferId
        {
            get; set;
        }

        public int WarehouseId
        {
            get; set;
        }

        public string WarehouseName
        {
            get; set;
        }

        public string OfferName
        {
            get; set;
        }

        public string OfferOn
        {
            get; set;
        }
        public string OfferCategory
        {
            get; set;
        }

        public string FreeOfferType
        {
            get; set;
        }

        public string Description
        {
            get; set;
        }

        public string itemname
        {
            get; set;
        }

        public int MinOrderQuantity
        {
            get; set;
        }

        public string FreeItemName
        {
            get; set;
        }

        public int NoOffreeQuantity
        {
            get; set;
        }

        public DateTime? start
        {
            get; set;
        }

        public DateTime? end
        {
            get; set;
        }

        public int FreeWalletPoint
        {
            get; set;
        }

        public double DiscountPercentage
        {
            get; set;
        }

        public int FreeItemId
        {
            get; set;
        }

        public int ItemId
        {
            get; set;
        }

        public bool IsActive
        {
            get; set;
        }

        public double QtyAvaiable
        {
            get; set;
        }

        public double QtyConsumed
        {
            get; set;
        }
        public int MaxQtyPersonCanTake
        {
            get; set;
        }
        public bool OfferWithOtherOffer
        {
            get; set;
        }

        public string OfferVolume { get; set; }

        public double FreeItemMRP
        {
            get; set;
        }

        public bool IsDeleted
        {
            get; set;
        }

        public DateTime? CreatedDate
        {
            get; set;
        }

        public DateTime? UpdateDate
        {
            get; set;
        }

        public string OfferCode
        {
            get; set;
        }

        public string BillDiscountOfferOn
        {
            get; set;
        }

        public double? BillDiscountWallet
        {
            get; set;
        }

        public double BillAmount
        {
            get; set;
        }

        public double MaxBillAmount
        {
            get; set;
        }

        public int LineItem { get; set; }
        public double MaxDiscount
        {
            get; set;
        }
        public bool IsMultiTimeUse
        {
            get; set;
        }
        public bool IsUseOtherOffer
        {
            get; set;
        }
        [NotMapped]
        public int? OfferUseCount
        {
            get; set;
        }

        public int? GroupId { get; set; } //Contain list of CustomerGroupId 

        public int? FreeItemLimit { get; set; } //Contain a  offer Item Limit 

        public string BillDiscountType { get; set; }
        public string OfferAppType { get; set; }
        public string ApplyOn { get; set; }
        public string WalletType { get; set; }
        public long StoreId { get; set; }
        public string StoreName { get; set; }
        // Add by Anoop 28/01/2021
        public string ApplyType { get; set; }
        public bool IsFreebiesLevel { get; set; }
        public int? ExcludeGroupId { get; set; }
        public long? CombinedGroupId { get; set; }
        public string ExcludeGroupName { get; set; }
        public string CombinedGroupName { get; set; }

    }

    public class offeritem
    {
        public int ItemId { get; set; }
        public int Cityid { get; set; }
        public string CityName { get; set; }
        public int Categoryid { get; set; }
        public int SubCategoryId { get; set; }
        public int SubsubCategoryid { get; set; }
        public string SubSubCode { get; set; }
        public int WarehouseId { get; set; }
        public int SupplierId { get; set; }
        public string SUPPLIERCODES { get; set; }
        public int CompanyId { get; set; }
        public string CategoryName { get; set; }
        public int BaseCategoryid { get; set; }
        public string BaseCategoryName { get; set; }
        public string SubcategoryName { get; set; }
        public string SubsubcategoryName { get; set; }
        public string SupplierName { get; set; }
        public string itemname { get; set; }
        public string itemcode { get; set; }
        public string SellingUnitName { get; set; }
        public string PurchaseUnitName { get; set; }
        public double price { get; set; }
        public double VATTax { get; set; }
        public string HSNCode { get; set; }
        public bool active { get; set; }
        public string LogoUrl { get; set; }
        public string CatLogoUrl { get; set; }
        public int MinOrderQty { get; set; }
        public int PurchaseMinOrderQty { get; set; }
        public int GruopID { get; set; }
        public string TGrpName { get; set; }
        public double Discount { get; set; }
        public double UnitPrice { get; set; }
        public string Number { get; set; }
        public string PurchaseSku { get; set; }
        public string SellingSku { get; set; }
        public double PurchasePrice { get; set; }
        public double? GeneralPrice { get; set; }
        public string title { get; set; }
        public string Description { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public double PramotionalDiscount { get; set; }
        public double TotalTaxPercentage { get; set; }
        public string WarehouseName { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
        public bool Deleted { get; set; }
        public bool free { get; set; }
        public bool IsDailyEssential { get; set; }
        public double DisplaySellingPrice { get; set; }
        public string StoringItemName { get; set; }
        public double SizePerUnit { get; set; }
        public string HindiName { get; set; }
        public string Barcode { get; set; }
        //public string ItemType { get; set; }
        public double Margin { get; set; }
        public int? marginPoint { get; set; }
        public int? promoPoint { get; set; }
        public int? promoPerItems { get; set; }
        public double NetPurchasePrice { get; set; }
        public bool IsOffer { get; set; }
        public int OfferId { get; set; }
        public string OfferName { get; set; }
        public int FreeItemId { get; set; }
        public string FreeItemName { get; set; }
        public double FreeItemMRP { get; set; }
        public int FreeWalletPoint { get; set; }
        public string OfferLogoUrl { get; set; }
    }

    public class ItemOfferdata
    {
        internal bool active;
        public int ItemId { get; set; }
        public string ItemNumber { get; set; }
        public int CompanyId { get; set; }
        public int WarehouseId { get; set; }
        public int Categoryid { get; set; }
        public int SubCategoryId { get; set; }
        public int SubsubCategoryid { get; set; }
        public string itemname { get; set; }
        public string HindiName { get; set; }
        public double price { get; set; }
        public string SellingUnitName { get; set; }
        public string SellingSku { get; set; }
        public double UnitPrice { get; set; }
        public double VATTax { get; set; }
        public string LogoUrl { get; set; }
        public int MinOrderQty { get; set; }
        public double Discount { get; set; }
        public double TotalTaxPercentage { get; set; }
        public double? marginPoint { get; internal set; }
        public int? promoPerItems { get; internal set; }
        public int? dreamPoint { get; internal set; }
        public bool IsOffer { get; set; }
        public bool Deleted { get; internal set; }
        public double NetPurchasePrice { get; set; }
        public int? OfferCategory
        {
            get; set;
        }
        public DateTime? OfferStartTime
        {
            get; set;
        }
        public DateTime? OfferEndTime
        {
            get; set;
        }
        public double? OfferQtyAvaiable
        {
            get; set;
        }

        public double? OfferQtyConsumed
        {
            get; set;
        }

        public int? OfferId
        {
            get; set;
        }

        public string OfferType
        {
            get; set;
        }

        public double? OfferWalletPoint
        {
            get; set;
        }
        public int? OfferFreeItemId
        {
            get; set;
        }

        public double? OfferPercentage
        {
            get; set;
        }

        public string OfferFreeItemName
        {
            get; set;
        }

        public string OfferFreeItemImage
        {
            get; set;
        }
        public int? OfferFreeItemQuantity
        {
            get; set;
        }
        public int? OfferMinimumQty
        {
            get; set;
        }
        public double? FlashDealPercentage
        {
            get; set;
        }
    }

    public class CustomerOffer
    {
        public string WarehouseName
        {
            get; set;
        }
        public string ItemName
        {
            get; set;
        }
        public string FreeItemName
        {
            get; set;
        }
        public double WalletPoint
        {
            get; set;
        }
        public string Customer
        {
            get; set;
        }

        public string OfferType
        {
            get; set;
        }

        public DateTime CreatedDate
        {
            get; set;
        }

        public string OfferName
        {
            get; set;
        }

    }

    public class Groupdata
    {

        public long Id { get; set; }
        public int? WarehouseID { get; set; }
        public long GroupID { get; set; }
        public int CustomerID { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public bool IsActive { get; set; }
        public bool? IsDeleted { get; set; }
        public int CreatedBy { get; set; }
        public int? ModifiedBy { get; set; }
        public string CustomerName { get; set; }

    }
    public class WROFFERTEM
    {
        public List<factoryItemdata> ItemMasters { get; set; }
        public List<factorySubSubCategory> SubsubCategories { get; set; }
        public bool Message { get; set; }
    }
    public class CustomerOfferVM
    {
        public int CustomerId { get; set; }
        public string Skcode { get; set; }
        public string OfferCode { get; set; }
        public string CustomerName { get; set; }
        public string HubName { get; set; }
        public bool IsScratchBDCode { get; set; }
        public string BillDiscountType { get; set; }
        public DateTime CreatedDate { get; set; }
        public int OrderID { get; set; }
        public double? Amount { get; set; }
        public double? OrderAmount { get; set; }
    }

    public class GetNamesOfOfferidCat
    {
        public string categorname { get; set; }
        public string subcatname { get; set; }
        public string subsubcatname { get; set; }

        public string BillDiscountType { get; set; }

    }

    public class OfferDetail
    {
        public int Id { get; set; }
        public int Itemid { get; set; }
        public int MinOrderQty { get; set; }
        public double UnitPrice { get; set; }
        public int? BrandCategoryMappingId { get; set; }
        public string Name { get; set; }
        public string ItemName { get; set; }
        public bool Selected { get; set; }
        public double? AgentCommisionPercent { get; set; }
        public double MRP { get; set; }
        public int? ItemMultiMRPId { get; set; }
    }

    public class BrandCompany
    {
        public int BrandCategoryMappingId { get; set; }
        public string BrandName { get; set; }
        public int SubCategoryMappingId { get; set; }
        public string SubcategoryName { get; set; }
        public string SubsubcategoryName { get; set; }
    }

    public class StoreBrandCompany
    {
        public List<OfferDetail> Brands { get; set; }
        public List<OfferDetail> Companys { get; set; }
    }

    public class OfferExportData
    {
        public string Skcode { get; set; }
        public string WarehouseName { get; set; }
        public string CityName { get; set; }
        public string OfferCode { get; set; }
        public string OfferOn { get; set; }
        public string Description { get; set; }
        public string OfferName { get; set; }
        public int? OrderId { get; set; }
        public double? BillDiscountAmount { get; set; }
        public double? OrderAmount { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string Status { get; set; }
        public string OfferAppType { get; set; }
        public string StoreName { get; set; }
        public DateTime? DispatchedDate { get; set; }
        public string BillDiscountFreeItem { get; set; }
    }

    public class OfferExportDataDC
    {
        public string Skcode { get; set; }
        public string WarehouseName { get; set; }
        public string CityName { get; set; }
        public string OfferCode { get; set; }
        public string OfferOn { get; set; }
        public string Description { get; set; }
        public string OfferName { get; set; }
        public int? OrderId { get; set; }
        public double? OrderAmount { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string Status { get; set; }
        public string OfferAppType { get; set; }
        public string FreeItemName { get; set; }
        public int? FreeItemQtyTaken { get; set; }
        public double? FreeItemPurchasePrice { get; set; }
        public double? FreebieValue { get; set; }
        public string StoreName { get; set; }

        //Add by Anoop 28/01/2021
        public string ApplyType { get; set; }
        public string BillDiscountFreeItem { get; set; }

    }


    public class itemdata
    {
        public int warehouseid { get; set; }
        public List<int> ids { get; set; }
        public string type { get; set; }

    }

    public class OfferPaggingData
    {
        public int total_count { get; set; }
        public List<OfferList> OfferListDTO { get; set; }
    }

    public class BillPaggingData
    {
        public int total_count { get; set; }
        public List<OfferList> BillListDTO { get; set; }
    }
    public class getofferobject
    {

        public int totalitem { get; set; }
        public int page { get; set; }
        public int? warehouseid { get; set; }
        public string status { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public string keyword { get; set; }
        public int ShowType { get; set; }

    }

    public class GetBillObject
    {

        public int totalitem { get; set; }
        public int page { get; set; }
        public int? warehouseid { get; set; }
        public string status { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public string keyword { get; set; }
        public string Types { get; set; }
        public int ShowType { get; set; }
    }

    public class forwarehousename
    {
        public int WarehouseId { get; set; }
        public string WarehouseName { get; set; }
        public string CityName { get; set; }

    }

    public class OfferCatSubCat
    {
        public int? Categoryid { get; set; }
        public int? SubCategoryId { get; set; }
        public int? SubSubCategoryId { get; set; }
        public int MappingId { get; set; }
    }

    public class ScheduleJobHistory
    {
        [BsonId]
        public ObjectId Id { get; set; }
        public string jobid { get; set; }
        public string ObjectType { get; set; }
        public string ObjectId { get; set; }
        public bool isActive { get; set; }
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime CreatedDate { get; set; }
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime? ModifiedDate { get; set; }
    }

    public class DistributorOfferHistoryDc
    {
        public int OfferId { get; set; }
        public int CustomerId { get; set; }
        public string OrderIds { get; set; }
        public string OfferName { get; set; }
        public decimal ActualDiscountValue { get; set; }
        public decimal UpdatedDiscountValue { get; set; }

        public DateTime OfferApplyDate { get; set; }

        public string OfferOn { get; set; }
        public string OfferType { get; set; }

        public double GullakAmount { get; set; }

        public string ItemName { get; set; }

        public int ItemPostQuantity { get; set; }

        public int ItemPostWeight { get; set; }

        public int ItemPostAmount { get; set; }

        public string Uom { get; set; }

        public double SlabMinimumAmount { get; set; }
    }

    public class UpdateGullakDc
    {
        public int OfferId { get; set; }

        public long Id { get; set; }

        public string Type { get; set; }
        public string Comment { get; set; }
    }

    public class DispatchDc
    {
        public double DispatchAmount { get; set; }
        public double DipstachDicount { get; set; }
    }
    public class ItemDC
    {
        public long Id { get; set; }
        public string ItemName { get; set; }
    }


    public class OfferCheckerDc
    {
        public long Id { get; set; }
        public int OfferId { get; set; }

        public string OfferCode { get; set; }

        public string CustomerName { get; set; }

        public string Skcode { get; set; }

        public string MobileNo { get; set; }

        public string OrderIds { get; set; }

        public string OfferOn { get; set; }

        public double OrderValue { get; set; }

        public int CustomerId { get; set; }

        public decimal Discount { get; set; }

        public decimal DispatchDiscount { get; set; }

        public int Status { get; set; }

        public string Comment { get; set; }
    }
    public class FreebiesDTO
    {
        public int warehouesId { get; set; }
        public string OfferName { get; set; }
        public string Itemnumber { get; set; }
        public string SellingSKU { get; set; }
        public string FreeItemnumber { get; set; }
        public string ItemName { get; set; }
        public string FreeItemName { get; set; }
        public int MinimumOrderQty { get; set; }
        public int NoOfFreeitemQty { get; set; }
        public int MRP { get; set; }
        public int FreeitemMRP { get; set; }
        public string Discription { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int QtyAvaiable { get; set; }
        public int FreeItemLimit { get; set; }
        public string AppType { get; set; }
        public int IsActive { get; set; }
        public string Stock { get; set; }
    }

    public class FreebiesUploader
    {
        public int CityId { get; set; }
        public int WarehouseId { get; set; }
        public string Itemnumber { get; set; }
        public double MRP { get; set; }
        public string FreeItemnumber { get; set; }
        public int MinimumOrderQty { get; set; }
        public int NoOfFreeitemQty { get; set; }
        public double FreeitemMRP { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int QtyAvaiable { get; set; }
        public bool IsFreeStock { get; set; }

        //Use during return
        public string ItemId { get; set; }
        public string OfferId { get; set; }
        public string Error { get; set; }
        public string OfferName { get; set; }
        public string OfferAppType { get; set; }

    }
    public class FreebiesResponceDC
    {
        public int Message { get; set; }
        public bool status { get; set; }
    }
    public class ErrorListDTO
    {
        public string ItemName { get; set; }
        public string Itemnumber { get; set; }
        public int MRP { get; set; }
        public string WareHouseName { get; set; }

    }

    public class InactiveOfferResponse
    {
        public bool status { get; set; }
        public string msg { get; set; }
        public Offer offer { get; set; }
    }

    public class OfferItemSearchRequest
    {
        public int WarehouseId { get; set; }
        public string keyword { get; set; }
        public float? margin { get; set; }
        public string itemClassification { get; set; }
        public List<int> subCategoryMappingids { get; set; }
        public List<int> brandCategoryMappingIds { get; set; }
    }
    public class offerBrandCatMap
    {
        public int SubsubcategoryId { get; set; }
        public int SubcategoryId { get; set; }
        public int CategoryId { get; set; }
    }

    public class MinSalesGroup
    {
        public long Id { get; set; }
        public string GroupName { get; set; }
    }

    public class WhatsappSkcode
    {
        public int CustomerID { get; set; }
        public string Skcode { get; set; }
    }

}