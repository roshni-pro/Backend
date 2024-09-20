using AngularJSAuthentication.API.Helpers;
using AngularJSAuthentication.BusinessLayer.Managers.Masters;
using AngularJSAuthentication.BusinessLayer.Managers.Reports;
using AngularJSAuthentication.Common.Enums;
using AngularJSAuthentication.Common.Helpers;
using AngularJSAuthentication.DataContracts.Masters;
using AngularJSAuthentication.DataContracts.Mongo;
using AngularJSAuthentication.DataContracts.ServiceRequestParam;
using AngularJSAuthentication.DataContracts.Shared;
using AngularJSAuthentication.DataContracts.Transaction.Mongo;
using AngularJSAuthentication.Model;
using AngularJSAuthentication.Model.BillDiscount;
using GenricEcommers.Models;
using LinqKit;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using Newtonsoft.Json;
using Nito.AspNetBackgroundTasks;
using Nito.AsyncEx;
using NLog;
using OpenHtmlToPdf;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.Description;
using AngularJSAuthentication.DataContracts.Masters.PaymentRefund;
using AngularJSAuthentication.API.Helper.PaymentRefund;
using static AngularJSAuthentication.API.Controllers.NotificationController;
using AngularJSAuthentication.API.Controllers.VAN;
using AngularJSAuthentication.API.Managers.CRM;
using AngularJSAuthentication.Common.Constants;
using AgileObjects.AgileMapper;
using System.Data.Entity.Core.Objects;
using AngularJSAuthentication.API.Helper.Notification;

namespace AngularJSAuthentication.API.Controllers
{
    [RoutePrefix("api/OrderMaster")]
    public class OrderMasterrController : ApiController
    {

        public static Logger logger = LogManager.GetCurrentClassLogger();

        [Authorize]
        [HttpPost]
        [Route("priority")]
        public HttpResponseMessage Post(ooo oo)
        {
            logger.Info("Order Automation");
            using (var context = new AuthContext())
            {
                try
                {
                    List<OrderMaster> assignedorders = new List<OrderMaster>();

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
                        assignedorders = oo.assignedorders;
                        string MobileNumber = oo.mobile;
                        logger.Info("User ID : {0} , Company Id : {1}", compid, userid);
                        bool status = true; //context.AllOrderMasterspriority(assignedorders, Warehouse_id, MobileNumber);
                        logger.Info("End OrderMaster: ");
                        if (status == true)
                        {
                            return Request.CreateResponse(HttpStatusCode.OK);
                        }
                        else
                        {
                            return Request.CreateResponse(HttpStatusCode.BadRequest);
                        }
                    }
                    else
                    {
                        logger.Info("End  OrderMaster: ");
                        return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Got an error");
                    }
                }
                catch (Exception ex)
                {
                    logger.Error("Error in OrderMaster " + ex.Message);
                    logger.Info("End  OrderMaster: ");
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex.Message); ;
                }
            }
        }

        /// <summary>
        /// ExportForSelfOrders
        /// </summary>
        /// <param name="29/03/2019"></param>
        /// <param name="enddate"></param>
        /// <returns></returns>
        /// 
        [Route("getselforder")]
        [HttpPost]
        public List<OrderMasterDc> getselfdata(DateTime start, DateTime end, List<int> WarehouseIds)
        {
            logger.Info("start OrderMaster: ");
            if (start != null && end != null)
            {
                List<OrderMaster> ass = new List<OrderMaster>();
                using (var db = new AuthContext())
                {
                    try
                    {
                        MongoDbHelper<MongoOrderMaster> mongoDbHelper = new MongoDbHelper<MongoOrderMaster>();
                        var collection = mongoDbHelper.mongoDatabase.GetCollection<BsonDocument>("OrderMaster");

                        var orderMasters = new List<MongoOrderMaster>();

                        var builder = Builders<BsonDocument>.Filter;
                        var filter = builder.Eq("Deleted", false);
                        filter = filter & builder.Gte("OrderTakenSalesPersonId", 0);
                        filter = filter & builder.Gte("CreatedDate", start) & builder.Lte("CreatedDate", end);
                        filter = filter & builder.AnyIn("WarehouseId", WarehouseIds);
                        FindOptions<BsonDocument> options = new FindOptions<BsonDocument>
                        {
                            BatchSize = 5000,
                            NoCursorTimeout = false
                        };
                        using (IAsyncCursor<BsonDocument> cursor = AsyncContext.Run(() => collection.FindAsync(filter, options)))
                        {
                            while (cursor.MoveNext())
                            {
                                IEnumerable<BsonDocument> batch = cursor.Current;
                                var result = new ConcurrentBag<MongoOrderMaster>();
                                ParallelLoopResult loopResult = Parallel.ForEach(batch, (document) =>
                                {
                                    var myObj = BsonSerializer.Deserialize<MongoOrderMaster>(document);
                                    result.Add(myObj);
                                });

                                if (loopResult.IsCompleted)
                                    orderMasters.AddRange(result.ToList());
                            }
                        }
                        #region CRMTAG

                        var skcodeList = orderMasters.Select(x => x.Skcode).Distinct().ToList();
                        List<OrderMasterDc> orderMasterDc = Mapper.Map(orderMasters).ToANew<List<OrderMasterDc>>();
                        CRMManager cRMManager = new CRMManager();
                        var TagData = cRMManager.GetCRMCustomerWithTag(skcodeList, CRMPlatformConstants.OrderMaster);
                        // var ChannelType = db.ChannelMasters.Where(x => x.Active == true && x.Deleted == false).ToList();

                        orderMasterDc.ForEach(x =>
                        {
                            //var ChannelMasterId = db.Customers.FirstOrDefault(y => y.CustomerId == x.CustomerId && y.Active == true && y.Deleted == false).ChannelMasterId;
                            //x.ChannelType = ((ChannelType != null && ChannelType.Count() > 0) && ChannelMasterId > 0) ? ChannelType.FirstOrDefault(y => y.ChannelMasterId == ChannelMasterId) != null ? ChannelType.FirstOrDefault(y => y.ChannelMasterId == ChannelMasterId).ChannelType : "" : "";
                            x.CRMTags = TagData != null && TagData.Result != null ? TagData.Result.Where(y => y.Skcode == x.Skcode).Select(z => z.CRMTags).FirstOrDefault() : null;
                        });

                        #endregion

                        //var list = db.DbOrderMaster.Where(p => p.Deleted == false && p.OrderTakenSalesPersonId == 0 && p.CreatedDate >= start && p.CreatedDate <= end && WarehouseIds.Contains(p.WarehouseId)).ToList();

                        //return list;
                        return orderMasterDc;
                    }
                    catch (Exception ex)
                    {
                        logger.Error("Error in OrderMaster " + ex.Message);
                        logger.Info("End OrderMaster: ");
                        return null;
                    }
                }
            }
            else
            {
                return null;
            }
        }
        public class selforderDc

        {
            public DateTime? start { get; set; }
            public DateTime? end { get; set; }
            public List<int> WarehouseIds { get; set; }


        }


        [Route("GetOrderData")]
        public IEnumerable<OrderMaster> Getp(DateTime startdate, DateTime enddate)
        {
            logger.Info("Order Automation");
            List<OrderMaster> orderdata = new List<OrderMaster>();
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
                        logger.Info("User ID : {0} , Company Id : {1}", compid, userid);
                        orderdata = context.PendingOrderByDate(startdate, enddate, Warehouse_id).ToList();
                        logger.Info("End OrderMaster: ");
                        return orderdata;
                    }
                    else
                    {
                        return orderdata;
                    }
                }
                catch (Exception ex)
                {
                    logger.Error("Error in OrderMaster " + ex.Message);
                    logger.Info("End  OrderMaster: ");
                    return orderdata;
                }
            }
        }

        [Route("")]
        [HttpGet]
        public List<OrderMaster> Get()
        {
            logger.Info("start OrderMaster: ");
            List<OrderMaster> ass = new List<OrderMaster>();
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

                    logger.Info("User ID : {0} , Company Id : {1}", compid, userid);
                    ass = context.AllOrderMasters(compid).ToList();
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

        [Route("")]
        public async System.Threading.Tasks.Task<OrderMaster> Get(int id)
        {
            logger.Info("start OrderMaster: ");
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
                    logger.Info("User ID : {0} , Company Id : {1}", compid, userid);


                    var ass = context.GetOrderMaster(id, compid);
                    if (ass != null)
                    {
                        MastersManager mastersManager = new MastersManager();
                        TripPickerIdDc tripPickerIdDc = await mastersManager.GetPickerId_TripId(ass.OrderId);
                        if (tripPickerIdDc != null)
                        {
                            ass.OrderPickerMasterId = tripPickerIdDc.PickerId == null ? 0 : tripPickerIdDc.PickerId.OrderPickerMasterId;
                            ass.TripPlannerMasterId = tripPickerIdDc.TripId == null ? 0 : tripPickerIdDc.TripId.TripPlannerMasterId;
                        }
                    }

                    #region CRMTAG
                    List<string> skcodeList = new List<string>();
                    skcodeList.Add(ass.Skcode);

                    CRMManager cRMManager = new CRMManager();
                    var TagData = cRMManager.GetCRMCustomerWithTag(skcodeList, CRMPlatformConstants.OrderMaster);

                    ass.CRMTags = TagData != null && TagData.Result != null ? TagData.Result.Where(y => y.Skcode == ass.Skcode).Select(z => z.CRMTags).FirstOrDefault() : null;

                    #endregion

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

        [Route("GetInvoiceAmountToWord")]
        [HttpGet]
        public async Task<string> GetInvoiceAmountToWord(decimal Amount)
        {
            if (Amount > 0)
                return ConvertNumberToWord.ConvToWordRupee(Amount);
            else
                return "";
        }

        [Route("")]
        public PaggingData Get(int list, int page, int WarehouseId)
        {
            logger.Info("start OrderMaster: ");
            PaggingData paggingData = new PaggingData();
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

                        logger.Info("User ID : {0} , Company Id : {1}", compid, userid);
                        paggingData = context.AllOrderMasterWid(list, page, compid, Warehouse_id);
                        logger.Info("End OrderMaster: ");
                    }
                    else
                    {

                        logger.Info("User ID : {0} , Company Id : {1}", compid, userid);
                        paggingData = context.AllOrderMaster(list, page, compid);
                        logger.Info("End OrderMaster: ");
                    }
                    #region Order master Orderid colour function //by sudhir 13-06-2019
                    List<OrderMaster> OrderMasters = (List<OrderMaster>)paggingData.ordermaster;


                    if (OrderMasters != null && OrderMasters.Any())
                    {
                        List<int> orderids = OrderMasters.Select(x => x.OrderId).ToList();
                        using (var authContext = new AuthContext())
                        {
                            var itemorderstock = (from d in authContext.DbOrderDetails.Where(x => orderids.Contains(x.OrderId))
                                                  join p in authContext.DbCurrentStock.Where(k => k.Deleted != true)
                                                  on new
                                                  {
                                                      ItemNumber = d.itemNumber,
                                                      d.WarehouseId,
                                                      d.ItemMultiMRPId
                                                  } equals new
                                                  {
                                                      ItemNumber = p.ItemNumber,
                                                      p.WarehouseId,
                                                      p.ItemMultiMRPId
                                                  } into fg
                                                  from fgi in fg.DefaultIfEmpty()
                                                  select new
                                                  {
                                                      Orderid = d.OrderId,
                                                      itemid = d.ItemId,
                                                      qty = d.qty,
                                                      Currentinventory = fgi != null ? fgi.CurrentInventory : 0
                                                  });

                            foreach (var item in OrderMasters)
                            {
                                if (itemorderstock.Any(x => x.Orderid == item.OrderId && x.qty > x.Currentinventory))
                                {
                                    if (item.Status == "Pending")
                                    {
                                        item.IsLessCurrentStock = true;
                                    }
                                    else
                                    {
                                        item.IsLessCurrentStock = false;
                                    }

                                }
                            }

                            var paymentresponse = authContext.PaymentResponseRetailerAppDb.Where(x => orderids.Contains(x.OrderId) && x.status == "Success").Select(x => new { x.OrderId, x.PaymentFrom }).ToList();
                            OrderMasters.ForEach(x =>
                            {
                                x.Trupay = paymentresponse.Any(y => y.OrderId == x.OrderId) && paymentresponse.Where(y => y.OrderId == x.OrderId).Count() > 1 ? "Partial"
                                                : paymentresponse.Where(y => y.OrderId == x.OrderId).Count() == 1 ? paymentresponse.FirstOrDefault(y => y.OrderId == x.OrderId).PaymentFrom
                                                : !paymentresponse.Any(y => y.OrderId == x.OrderId) && x.Trupay == "true" ? "Trupay"
                                                : !paymentresponse.Any(y => y.OrderId == x.OrderId) && (string.IsNullOrEmpty(x.Trupay) || x.Trupay == "false") ? "Cash"
                                                : "Cash";
                            });


                            paggingData.ordermaster = OrderMasters;
                        }
                    }

                    #endregion
                    return paggingData;
                }
                catch (Exception ex)
                {
                    logger.Error("Error in OrderMaster " + ex.Message);
                    logger.Info("End  OrderMaster: ");
                    return null;
                }
            }
        }

        [Route("GetOrderAdvanceSearch")]
        [HttpPost]
        public PaggingData GetOrderAdvanceSearch(FilterOrderDTO filterOrderDTO)
        {
            logger.Info("start GetOrderAdvanceSearch: ");
            PaggingData paggingData = new PaggingData();
            using (var db = new AuthContext())
            {
                try
                {
                    var identity = User.Identity as ClaimsIdentity;
                    int compid = 0, userid = 0;

                    if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))
                        compid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);

                    if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                        userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

                    var cityresult = string.Join(",", filterOrderDTO.Cityids);
                    var warehouseresult = string.Join(",", filterOrderDTO.WarehouseIds);

                    using (AuthContext context = new AuthContext())
                    {
                        int skip = (filterOrderDTO.PageNo - 1) * filterOrderDTO.ItemPerPage;
                        int take = filterOrderDTO.ItemPerPage;



                        string whereclause = compid > 0 ? " and o.CompanyId = " + compid : "";
                        if (filterOrderDTO.Cityids.Count() > 0)
                            whereclause += " and o.Cityid in (" + cityresult + ")";

                        if (filterOrderDTO.WarehouseIds.Count() > 0)
                            whereclause += " and o.WarehouseId in (" + warehouseresult + ")";

                        if (filterOrderDTO.OrderId > 0)
                            whereclause += " and o.OrderId = " + filterOrderDTO.OrderId;

                        if (!string.IsNullOrEmpty(filterOrderDTO.Mobile))
                            whereclause += " and o.Customerphonenum Like " + "'%" + filterOrderDTO.Mobile + "%'";

                        if (!string.IsNullOrEmpty(filterOrderDTO.Skcode))
                            whereclause += " and o.Skcode Like " + "'%" + filterOrderDTO.Skcode + "%'";

                        if (!string.IsNullOrEmpty(filterOrderDTO.ShopName))
                            whereclause += " and o.ShopName Like " + "'%" + filterOrderDTO.ShopName + "%'";

                        if (!string.IsNullOrEmpty(filterOrderDTO.status))
                            whereclause += " and o.status =" + "'" + filterOrderDTO.status + "'";

                        if (filterOrderDTO.start.HasValue && filterOrderDTO.end.HasValue)
                            whereclause += " and (o.CreatedDate >= " + "'" + filterOrderDTO.start.Value.ToString("MM-dd-yyyy HH:mm:ss") + "'" + " And  o.CreatedDate <=" + "'" + filterOrderDTO.end.Value.ToString("MM-dd-yyyy HH:mm:ss") + "')";

                        if (filterOrderDTO.TimeLeft.HasValue)
                            whereclause += " and datediff (s, DATEADD(hour, 48,o.CreatedDate),GETDATE()) <=" + filterOrderDTO.TimeLeft.Value * 3600;


                        string sqlquery = "Select o.*,od.OrderDispatchedMasterId,0  from OrderMasters o left join OrderDispatchedMasters od on o.OrderId = od.OrderId where o.Deleted = 0 and o.status <> 'Inactive' and o.status <> 'Dummy Order Cancelled' " + whereclause
                                       + " Order by o.OrderId desc offset " + skip + " rows fetch next " + take + " rows only";

                        string sqlCountQuery = "Select Count(*) from OrderMasters o left join OrderDispatchedMasters od on o.OrderId = od.OrderId where o.Deleted = 0 and o.status <> 'Inactive' and o.status <> 'Dummy Order Cancelled' " + whereclause;

                        List<OrderMasterDc> newdata = context.Database.SqlQuery<OrderMasterDc>(sqlquery).ToList();

                        int dataCount = context.Database.SqlQuery<int>(sqlCountQuery).FirstOrDefault();
                        paggingData.total_count = 0;
                        if (newdata != null && newdata.Any())
                        {
                            paggingData.total_count = dataCount;
                            var orderids = newdata.Select(x => x.OrderId).ToList();
                            var orderDetails = context.DbOrderDetails.Where(x => orderids.Contains(x.OrderId)).ToList();
                            var paymentresponse = context.PaymentResponseRetailerAppDb.Where(x => orderids.Contains(x.OrderId) && x.status == "Success").Select(x => new { x.OrderId, x.PaymentFrom }).ToList();
                            newdata.ForEach(x =>
                            {
                                x.orderDetails = orderDetails.Any(y => y.OrderId == x.OrderId) ? orderDetails.Where(y => y.OrderId == x.OrderId).ToList() : new List<Model.OrderDetails>();
                                x.Trupay = paymentresponse.Any(y => y.OrderId == x.OrderId) && paymentresponse.Where(y => y.OrderId == x.OrderId).Count() > 1 ? String.Join("<br/>", paymentresponse.Where(y => y.OrderId == x.OrderId).Select(z => z.PaymentFrom))
                                                    : paymentresponse.Where(y => y.OrderId == x.OrderId).Count() == 1 ? paymentresponse.FirstOrDefault(y => y.OrderId == x.OrderId).PaymentFrom
                                                    : !paymentresponse.Any(y => y.OrderId == x.OrderId) && x.Trupay == "true" ? "Trupay"
                                                    : !paymentresponse.Any(y => y.OrderId == x.OrderId) && (string.IsNullOrEmpty(x.Trupay) || x.Trupay == "false") ? "Cash"
                                                    : "Cash";
                            });
                        }
                        paggingData.ordermaster = newdata;
                    }

                    //List<OrderMaster> newdata = new List<OrderMaster>();

                    //var listOrders = DbOrderMaster.Where(x => x.Deleted == false && x.CompanyId == compid && x.WarehouseId == Warehouse_id).OrderByDescending(x => x.OrderId).Skip((page - 1) * list).Take(list).Include("orderDetails").ToList();
                    //newdata = listOrders;
                    //PaggingData obj = new PaggingData();
                    //obj.total_count = DbOrderMaster.Where(x => x.Deleted == false && x.CompanyId == compid && x.WarehouseId == Warehouse_id).Count();
                    //obj.ordermaster = newdata;

                    //Warehouse_id = WarehouseId;
                    //if (Warehouse_id > 0)
                    //{

                    //    logger.Info("User ID : {0} , Company Id : {1}", compid, userid);
                    //    paggingData = context.AllOrderMasterWid(list, page, compid, Warehouse_id);
                    //    logger.Info("End OrderMaster: ");
                    //}
                    //else
                    //{

                    //    logger.Info("User ID : {0} , Company Id : {1}", compid, userid);
                    //    paggingData = context.AllOrderMaster(list, page, compid);
                    //    logger.Info("End OrderMaster: ");
                    //}
                    #region Order master Orderid colour function //by sudhir 13-06-2019
                    List<OrderMasterDc> OrderMasters = (List<OrderMasterDc>)paggingData.ordermaster;
                    if (OrderMasters != null && OrderMasters.Any())
                    {
                        List<int> orderids = OrderMasters.Select(x => x.OrderId).ToList();


                        using (var authContext = new AuthContext())
                        {
                            var itemorderstock = (from d in authContext.DbOrderDetails.Where(x => orderids.Contains(x.OrderId) && !x.IsDispatchedFreeStock)
                                                  join p in authContext.DbCurrentStock.Where(k => k.Deleted != true)
                                                  on new
                                                  {
                                                      ItemNumber = d.itemNumber,
                                                      d.WarehouseId,
                                                      d.ItemMultiMRPId
                                                  } equals new
                                                  {
                                                      ItemNumber = p.ItemNumber,
                                                      p.WarehouseId,
                                                      p.ItemMultiMRPId
                                                  } into fg
                                                  from fgi in fg.DefaultIfEmpty()
                                                  select new
                                                  {
                                                      Orderid = d.OrderId,
                                                      itemid = d.ItemId,
                                                      qty = d.qty,
                                                      Currentinventory = fgi != null ? fgi.CurrentInventory : 0
                                                  });



                            List<OrderOffer> OrderOffers = new List<OrderOffer>();
                            if (orderids != null && orderids.Any())
                            {
                                string sqlQuery = "select a.OrderId,b.OfferCode from BillDiscounts a inner join Offers b on a.OfferId = b.OfferId and a.OrderId in ( " + string.Join(",", orderids) + ")";
                                OrderOffers = authContext.Database.SqlQuery<OrderOffer>(sqlQuery).ToList();
                            }

                            foreach (var item in OrderMasters)
                            {
                                List<int> itemids = item.orderDetails.Select(y => y.ItemId).ToList();
                                if (itemids.Any())
                                {
                                    string sqlCountQuery = "Select Count(*) from OrderMasters o inner join OrderDetails d on o.OrderId = d.OrderId and o.Deleted = 0 and o.WarehouseId = " + item.WarehouseId + " and o.Status = 'Pending' and d.ItemId in (" + string.Join(",", itemids) + ") and o.OrderId < " + item.OrderId;

                                    int ItemCountCount = authContext.Database.SqlQuery<int>(sqlCountQuery).FirstOrDefault();
                                    if (OrderOffers.Any(x => x.OrderId == item.OrderId))
                                    {
                                        var offerCodes = OrderOffers.Where(x => x.OrderId == item.OrderId).Select(x => x.OfferCode).ToList();
                                        item.OfferCode = string.Join(",", offerCodes);
                                    }
                                    item.IsItemInOtherOrder = ItemCountCount > 0;
                                }

                                if (item.IsItemInOtherOrder == false && item.Status == "Pending")
                                {
                                    item.isCompleted = true;

                                }

                                if (itemorderstock.Any(x => x.Orderid == item.OrderId && x.qty > x.Currentinventory))
                                {
                                    if (item.Status == "Pending")
                                    {
                                        item.IsLessCurrentStock = true;

                                    }
                                    else
                                    {
                                        item.IsLessCurrentStock = false;

                                    }

                                }
                            }
                            paggingData.ordermaster = OrderMasters;

                        }
                    }

                    #endregion
                    return paggingData;
                }
                catch (Exception ex)
                {
                    logger.Error("Error in GetOrderSearch " + ex.Message);
                    logger.Info("End  GetOrderSearch: ");
                    return null;
                }
            }
        }

        [Route("GetOrderAdvanceSearchMongo")]
        [HttpPost]
        public PaggingData GetOrderAdvanceSearchMongo(FilterOrderDTO filterOrderDTO)
        {

            var identity = User.Identity as ClaimsIdentity;
            int userid = 0;
            string Department = "";
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
            using (var db = new AuthContext())
            {

                var Dept = db.Peoples.Where(x => x.PeopleID == userid && x.Active == true && x.Deleted == false).FirstOrDefault();
                Department = Dept.Department.ToString();

                List<string> roleNames = new List<string>();
                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "RoleNames") && identity.Claims.FirstOrDefault(x => x.Type == "RoleNames").Value != "")
                    roleNames = (identity.Claims.FirstOrDefault(x => x.Type == "RoleNames").Value).Split(',').ToList();
                if (roleNames != null && roleNames.Contains("Telecaller"))
                {
                    Department = "Sales";
                }
                var fetchFromMongo = ConfigurationManager.AppSettings["OrdersFetchFromMongo"];

                if (fetchFromMongo == "true")
                    return GetOrdersFromMongo(filterOrderDTO);
                else
                    return GetOrdersFromDB(filterOrderDTO, Department);
            }
        }

        [Route("GetOrderDCR")]
        [HttpPost]
        public PaggingData GetOrderDCR(FilterOrderDTO filterOrderDTO)
        {
            var identity = User.Identity as ClaimsIdentity;
            int userid = 0;
            string Department = "";
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
            using (var db = new AuthContext())
            {

                var Dept = db.Peoples.Where(x => x.PeopleID == userid && x.Active == true && x.Deleted == false).FirstOrDefault();
                Department = Dept.Department.ToString();
                PaggingData paggingData = new PaggingData();
                int skip = (filterOrderDTO.PageNo - 1) * filterOrderDTO.ItemPerPage;
                int take = filterOrderDTO.ItemPerPage;
                int totalItems = 0;
                List<SearchOrderMaster> orderData = new List<SearchOrderMaster>();

                var manager = new ItemLedgerManager();
                orderData = manager.SearchOrderMaster(filterOrderDTO, skip, take, out totalItems, Department);
                var orderids = orderData.Select(x => x.OrderId).ToList();

                if (orderids != null && orderids.Any())
                {
                    var orderidDt = new DataTable();
                    orderidDt.Columns.Add("IntValue");
                    foreach (var item in orderids)
                    {
                        var dr = orderidDt.NewRow();
                        dr["IntValue"] = item;
                        orderidDt.Rows.Add(dr);
                    }

                    var param = new SqlParameter("@OrderIds", orderidDt);
                    param.SqlDbType = SqlDbType.Structured;
                    param.TypeName = "dbo.IntValues";
                    List<OrderDCR> OrderDCRs = new List<OrderDCR>();
                    using (AuthContext context = new AuthContext())
                    {
                        OrderDCRs = context.Database.SqlQuery<OrderDCR>("exec GetOrderDCRHistoryByOrderId @OrderIds", param).ToList();
                    }
                    orderData.ForEach(x =>
                        {
                            x.Distance = "";
                            if (OrderDCRs.Any(y => y.OrderId == x.OrderId))
                            {
                                x.Distance = OrderDCRs.FirstOrDefault(y => y.OrderId == x.OrderId).CreatedDate.ToString("MM/dd/yyyy hh:mm tt");
                            }

                        }
                        );
                }
                orderData.Where(x => !string.IsNullOrEmpty(x.SalesPerson)).ToList().ForEach(x =>
                {
                    x.SalesPerson = string.Join(",", x.SalesPerson.Split(',').Distinct());
                });
                paggingData.ordermaster = orderData.OrderBy(x => x.Distance);
                paggingData.total_count = totalItems;
                return paggingData;
            }
        }

        private PaggingData GetOrdersFromMongo(FilterOrderDTO filterOrderDTO)
        {
            PaggingData paggingData = new PaggingData();
            int skip = (filterOrderDTO.PageNo - 1) * filterOrderDTO.ItemPerPage;
            int take = filterOrderDTO.ItemPerPage;
            var identity = User.Identity as ClaimsIdentity;
            int compid = 0;

            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))
                compid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);

            //filterOrderDTO.LevelName = "Level 0";

            MongoDbHelper<MongoOrderMaster> mongoDbHelper = new MongoDbHelper<MongoOrderMaster>();

            var orderPredicate = PredicateBuilder.New<MongoOrderMaster>(x => x.CompanyId == compid && x.Status != "Inactive" && x.Status != "Dummy Order Cancelled" && !x.Deleted);
            List<int> PayOrderid = null;

            var monthlycustomerlevel = new MongoDbHelper<MonthlyCustomerLevel>();

            var customerlevel = new MonthlyCustomerLevel();

            customerlevel = monthlycustomerlevel.Select(x => x.Month == DateTime.Now.Month && x.Year == DateTime.Now.Year).FirstOrDefault();

            var levels = customerlevel?.CustomerLevels;

            if (filterOrderDTO.PaymentFrom != null && filterOrderDTO.PaymentFrom.Any())
            {
                string query = "select distinct orderid from PaymentResponseRetailerApps where status='Success' and paymentFrom in (";
                string whereclause = "";
                filterOrderDTO.PaymentFrom.ForEach(x =>
                {
                    whereclause += string.IsNullOrEmpty(whereclause) ? "'" + x + "'" : ",'" + x + "'";
                });
                query += whereclause + ")";
                using (var authContext = new AuthContext())
                {

                    PayOrderid = authContext.Database.SqlQuery<int>(query).ToList();
                }
            }

            if (filterOrderDTO.Cityids.Count() > 0)
                orderPredicate.And(x => filterOrderDTO.Cityids.Contains(x.CityId));

            if (filterOrderDTO.WarehouseIds.Count() > 0)
                orderPredicate.And(x => filterOrderDTO.WarehouseIds.Contains(x.WarehouseId));



            if (PayOrderid != null && PayOrderid.Any())
                orderPredicate.And(x => PayOrderid.Contains(x.OrderId));

            if (!string.IsNullOrEmpty(filterOrderDTO.invoice_no))
                orderPredicate.And(x => x.invoice_no.Contains(filterOrderDTO.invoice_no));

            if (filterOrderDTO.OrderId > 0)
                orderPredicate.And(x => x.OrderId == filterOrderDTO.OrderId);

            if (!string.IsNullOrEmpty(filterOrderDTO.Mobile))
                orderPredicate.And(x => x.Customerphonenum.Contains(filterOrderDTO.Mobile));

            if (!string.IsNullOrEmpty(filterOrderDTO.Skcode))
                orderPredicate.And(x => x.Skcode.Contains(filterOrderDTO.Skcode));

            if (!string.IsNullOrEmpty(filterOrderDTO.ShopName))
                orderPredicate.And(x => x.ShopName.Contains(filterOrderDTO.ShopName));

            if (!string.IsNullOrEmpty(filterOrderDTO.status))
                orderPredicate.And(x => x.Status == filterOrderDTO.status);

            if (!string.IsNullOrEmpty(filterOrderDTO.LevelName) && levels != null && levels.Any())
            {
                List<int> ids = levels.Where(x => x.LevelName == filterOrderDTO.LevelName).Select(x => x.CustomerId).ToList();
                orderPredicate.And(x => ids.Contains(x.CustomerId));
            }

            if (filterOrderDTO.start.HasValue && filterOrderDTO.end.HasValue)
                orderPredicate.And(x => x.CreatedDate >= filterOrderDTO.start.Value && x.CreatedDate <= filterOrderDTO.end.Value);

            if (filterOrderDTO.OrderType > -1 && filterOrderDTO.OrderType != 7)
                orderPredicate.And(x => x.OrderType == filterOrderDTO.OrderType);
            if (filterOrderDTO.OrderType == 7)
            {
                orderPredicate.And(x => x.ParentOrderId != null);
            }

            int dataCount = mongoDbHelper.Count(orderPredicate, collectionName: "OrderMaster");

            var orderMasters = new List<MongoOrderMaster>();

            orderMasters = mongoDbHelper.Select(orderPredicate, x => x.OrderByDescending(z => z.OrderId), skip, take, collectionName: "OrderMaster").ToList();

            //var orderm = 


            #region Prepare data
            //var newdata = (from x in orderMasters
            //               join b in levels on x.CustomerId equals b.CustomerId select new OrderMasterDc
            var newdata = orderMasters.Select(x => new OrderMasterDc
            {
                OrderId = x.OrderId,
                CompanyId = x.CompanyId,
                SalesPersonId = x.SalesPersonId,
                SalesPerson = x.SalesPerson,
                SalesMobile = x.SalesMobile,
                CustomerId = x.CustomerId,
                CustomerName = x.CustomerName,
                Skcode = x.Skcode,
                ShopName = x.ShopName,
                Status = x.Status,
                invoice_no = x.invoice_no,
                Trupay = x.Trupay,
                paymentThrough = x.paymentThrough,
                TrupayTransactionId = x.TrupayTransactionId,
                paymentMode = x.paymentMode,
                //PaymentFrom = x.PaymentFrom,
                CustomerCategoryId = x.CustomerCategoryId,
                CustomerCategoryName = x.CustomerCategoryName,
                CustomerType = x.CustomerType,
                LandMark = x.LandMark,
                Customerphonenum = x.Customerphonenum,
                BillingAddress = x.BillingAddress,
                ShippingAddress = x.ShippingAddress,
                TotalAmount = x.TotalAmount,
                GrossAmount = x.GrossAmount,
                DiscountAmount = x.DiscountAmount,
                TaxAmount = x.TaxAmount,
                SGSTTaxAmmount = x.SGSTTaxAmmount,
                CGSTTaxAmmount = x.CGSTTaxAmmount,
                CityId = x.CityId,
                WarehouseId = x.WarehouseId,
                WarehouseName = x.WarehouseName,
                active = x.active,
                CreatedDate = x.CreatedDate,
                //TimeZoneInfo.ConvertTime((x.OrderMasterHistories.Any(y => y.Status == "Pending") ? x.OrderMasterHistories.FirstOrDefault(y => y.Status == "Pending").CreatedDate :
                //x.CreatedDate), TimeZoneInfo.FindSystemTimeZoneById("India Standard Time")),
                Deliverydate = TimeZoneInfo.ConvertTime(x.Deliverydate, TimeZoneInfo.FindSystemTimeZoneById("India Standard Time")),
                UpdatedDate = TimeZoneInfo.ConvertTime(x.UpdatedDate, TimeZoneInfo.FindSystemTimeZoneById("India Standard Time")),
                ReadytoDispatchedDate = x.ReadytoDispatchedDate.HasValue ? TimeZoneInfo.ConvertTime(x.ReadytoDispatchedDate.Value, TimeZoneInfo.FindSystemTimeZoneById("India Standard Time")) : (DateTime?)null,
                DeliveredDate = x.DeliveredDate.HasValue ? TimeZoneInfo.ConvertTime(x.DeliveredDate.Value, TimeZoneInfo.FindSystemTimeZoneById("India Standard Time")) : (DateTime?)null,
                Deleted = x.Deleted,
                ReDispatchCount = x.ReDispatchCount,
                DivisionId = x.DivisionId,
                ReasonCancle = x.ReasonCancle,
                ClusterId = x.ClusterId,
                ClusterName = x.ClusterName,
                deliveryCharge = x.deliveryCharge,
                WalletAmount = x.WalletAmount,
                walletPointUsed = x.walletPointUsed,
                UsedPoint = x.UsedPoint,
                RewardPoint = x.RewardPoint,
                ShortAmount = x.ShortAmount,
                comments = x.comments,
                OrderTakenSalesPersonId = x.OrderTakenSalesPersonId,
                OrderTakenSalesPerson = x.OrderTakenSalesPerson,
                Tin_No = x.Tin_No,
                ShortReason = x.ShortReason,
                orderProcess = x.orderProcess,
                accountProcess = x.accountProcess,
                chequeProcess = x.chequeProcess,
                epaymentProcess = x.epaymentProcess,
                Savingamount = x.Savingamount,
                OnlineServiceTax = x.OnlineServiceTax,
                InvoiceBarcodeImage = x.InvoiceBarcodeImage,
                userid = x.userid,
                Description = x.Description,
                IsLessCurrentStock = x.IsLessCurrentStock,
                BillDiscountAmount = x.BillDiscountAmount,
                offertype = x.offertype,
                OrderDispatchedMasterId = x.OrderDispatchedMasterId,
                DispatchAmount = x.DispatchAmount,
                OrderAmount = x.OrderAmount,
                OfferCode = x.OfferCode,
                OrderType = x.OrderType,
                IsPrimeCustomer = x.IsPrimeCustomer,
                IsDigitalOrder = x.IsDigitalOrder,
                //CustomerLevel = b.LevelName,
                //ColourCode = b.ColourCode,

                orderDetails = x.orderDetails.Select(z => new Model.OrderDetails
                {
                    OrderDetailsId = z.OrderDetailsId,
                    OrderId = z.OrderId,
                    CustomerId = z.CustomerId,
                    CustomerName = z.CustomerName,
                    City = z.City,
                    Mobile = z.Mobile,
                    OrderDate = TimeZoneInfo.ConvertTime(z.OrderDate, TimeZoneInfo.FindSystemTimeZoneById("India Standard Time")),
                    CompanyId = z.CompanyId,
                    CityId = z.CityId,
                    WarehouseId = x.WarehouseId,
                    WarehouseName = x.WarehouseName,
                    CategoryName = z.CategoryName,
                    SubcategoryName = z.SubcategoryName,
                    SubsubcategoryName = z.SubsubcategoryName,
                    SellingSku = z.SellingSku,
                    ItemId = z.ItemId,
                    Itempic = z.Itempic,
                    itemname = z.itemname,
                    SellingUnitName = z.SellingUnitName,
                    itemcode = z.itemcode,
                    itemNumber = z.itemNumber,
                    HSNCode = z.HSNCode,
                    Barcode = z.Barcode,
                    price = z.price,
                    UnitPrice = z.UnitPrice,
                    Purchaseprice = z.Purchaseprice,
                    MinOrderQty = z.MinOrderQty,
                    MinOrderQtyPrice = z.MinOrderQtyPrice,
                    qty = z.qty,
                    Noqty = z.Noqty,
                    AmtWithoutTaxDisc = z.AmtWithoutTaxDisc,
                    AmtWithoutAfterTaxDisc = z.AmtWithoutAfterTaxDisc,
                    TotalAmountAfterTaxDisc = z.TotalAmountAfterTaxDisc,
                    NetAmmount = z.NetAmmount,
                    DiscountPercentage = z.DiscountPercentage,
                    DiscountAmmount = z.DiscountAmmount,
                    NetAmtAfterDis = z.NetAmtAfterDis,
                    TaxPercentage = z.TaxPercentage,
                    TaxAmmount = z.TaxAmmount,
                    SGSTTaxPercentage = z.SGSTTaxPercentage,
                    SGSTTaxAmmount = z.SGSTTaxAmmount,
                    CGSTTaxPercentage = z.CGSTTaxPercentage,
                    CGSTTaxAmmount = z.CGSTTaxAmmount,
                    TotalCessPercentage = z.TotalCessPercentage,
                    CessTaxAmount = z.CessTaxAmount,
                    TotalAmt = z.TotalAmt,
                    OrderedTotalAmt = z.OrderedTotalAmt,
                    CreatedDate = TimeZoneInfo.ConvertTime(z.CreatedDate, TimeZoneInfo.FindSystemTimeZoneById("India Standard Time")),
                    UpdatedDate = TimeZoneInfo.ConvertTime(z.UpdatedDate, TimeZoneInfo.FindSystemTimeZoneById("India Standard Time")),
                    Deleted = z.Deleted,
                    Status = z.Status,
                    SizePerUnit = z.SizePerUnit,
                    marginPoint = z.marginPoint,
                    promoPoint = z.promoPoint,
                    NetPurchasePrice = z.NetPurchasePrice,
                    CurrentStock = z.CurrentStock,
                    day = z.day,
                    month = z.month,
                    year = z.year,
                    status = z.status,
                    SupplierName = z.SupplierName,
                    ItemMultiMRPId = z.ItemMultiMRPId,
                    IsFreeItem = z.IsFreeItem,
                    IsDispatchedFreeStock = z.IsDispatchedFreeStock,
                    FreeWithParentItemId = z.FreeWithParentItemId,
                    StoreId = z.StoreId,
                    StoreName = z.StoreName
                }).ToList()
            }).ToList();

            if (levels != null && levels.Any())
            {
                foreach (var item in newdata)
                {
                    var leveldata = levels.Where(x => x.SKCode == item.Skcode).Select(x => new { LevelName = x.LevelName, ColourCode = x.ColourCode }).FirstOrDefault();
                    if (leveldata != null)
                    {
                        item.CustomerLevel = leveldata.LevelName;
                        item.ColourCode = leveldata.ColourCode;
                    }
                }
            }
            #endregion


            paggingData.total_count = 0;
            if (newdata != null && newdata.Any())
            {
                paggingData.total_count = dataCount;
                var orderids = newdata.Select(x => x.OrderId).ToList();
                var orderDetails = newdata.SelectMany(x => x.orderDetails).ToList();
                var paymentresponse = new List<PaymentResponseRetailerApp>();
                var OrderDeliveryOTP = new List<OrderDeliveryOTP>();
                using (AuthContext context = new AuthContext())
                {
                    paymentresponse = context.PaymentResponseRetailerAppDb.Where(x => orderids.Contains(x.OrderId) && x.status == "Success").ToList();
                    //var Otporderids = newdata.Where(x=>x.Status== "Delivery Canceled" || x.Status == "Delivered" || x.Status == "Delivery Redispatch").Select(x => x.OrderId).ToList();
                    var Otporderids = newdata.Select(x => x.OrderId).ToList();
                    OrderDeliveryOTP = context.OrderDeliveryOTP.Where(x => Otporderids.Contains(x.OrderId) && x.IsActive).ToList();
                }

                var customerids = newdata.Select(x => x.CustomerId).Distinct().ToList();
                var warehouseids = newdata.Select(x => x.WarehouseId).Distinct().ToList();

                CustomersManager manager = new CustomersManager();
                var customerWarehouseLtlng = manager.GetCustomerOrder(customerids, warehouseids);

                newdata.ForEach(x =>
                {
                    x.Trupay = paymentresponse.Any(y => y.OrderId == x.OrderId) && paymentresponse.Where(y => y.OrderId == x.OrderId).Count() > 1 ? String.Join("<br/>", paymentresponse.Where(y => y.OrderId == x.OrderId).Select(z => z.PaymentFrom))
                                        : paymentresponse.Where(y => y.OrderId == x.OrderId).Count() == 1 ? paymentresponse.FirstOrDefault(y => y.OrderId == x.OrderId).PaymentFrom
                                        : !paymentresponse.Any(y => y.OrderId == x.OrderId) && x.Trupay == "true" ? "Trupay"
                                        : !paymentresponse.Any(y => y.OrderId == x.OrderId) && (string.IsNullOrEmpty(x.Trupay) || x.Trupay == "false") ? "Cash"
                                        : "Cash";
                    x.OrderStatusOtp = OrderDeliveryOTP.Any(y => y.OrderId == x.OrderId) ? OrderDeliveryOTP.FirstOrDefault(y => y.OrderId == x.OrderId).Status + " OTP : <span class='label label-danger'>" + OrderDeliveryOTP.FirstOrDefault(y => y.OrderId == x.OrderId).OTP + "</span>" : "";
                    x.IsVideoSeen = OrderDeliveryOTP.Any(y => y.OrderId == x.OrderId) ? OrderDeliveryOTP.FirstOrDefault(y => y.OrderId == x.OrderId).IsVideoSeen : null;
                    x.VideoUrl = OrderDeliveryOTP.Any(y => y.OrderId == x.OrderId) ? OrderDeliveryOTP.FirstOrDefault(y => y.OrderId == x.OrderId).VideoUrl : "";
                    x.UserType = OrderDeliveryOTP.Any(y => y.OrderId == x.OrderId) ? OrderDeliveryOTP.FirstOrDefault(y => y.OrderId == x.OrderId).UserType : "";
                    x.DeliveryOtpId = OrderDeliveryOTP.Any(y => y.OrderId == x.OrderId) ? OrderDeliveryOTP.FirstOrDefault(y => y.OrderId == x.OrderId).Id : 0;

                    if (customerWarehouseLtlng != null && customerWarehouseLtlng.CustomerLtlng != null && customerWarehouseLtlng.WarehouseLtlng != null)
                    {
                        var customerltlng = customerWarehouseLtlng.CustomerLtlng.FirstOrDefault(y => x.CustomerId == y.Id);
                        var warehouseltlng = customerWarehouseLtlng.WarehouseLtlng.FirstOrDefault(y => x.WarehouseId == y.Id);

                        if (warehouseltlng != null && warehouseltlng.lat != 0 && warehouseltlng.lg != 0 && customerltlng != null && customerltlng.lat != 0 && customerltlng.lat >= -180.0 && customerltlng.lat <= 180.0 && customerltlng.lg != 0 && customerltlng.lg >= -180.0 && customerltlng.lg <= 180.0)
                        {
                            var sourceGeoCordinates = new System.Device.Location.GeoCoordinate(warehouseltlng.lat, warehouseltlng.lg);
                            var destination = new System.Device.Location.GeoCoordinate(customerltlng.lat, customerltlng.lg);
                            var dist = GeoHelper.AerialDistance(sourceGeoCordinates, destination);
                            x.Distance = Math.Round(dist, 2).ToString() + " KM";
                        }
                    }

                });

                var currentStocks = new List<CurrentStockMinDc>();
                var pendingOrders = newdata.Where(x => x.Status == "Pending").OrderBy(x => x.OrderId).ToList();
                var pendingOrderIds = pendingOrders.Select(z => z.OrderId);
                var multimrpIds = newdata.SelectMany(z => z.orderDetails).GroupBy(x => new { x.ItemMultiMRPId, x.WarehouseId, x.IsDispatchedFreeStock }).Select(x => new { x.Key.ItemMultiMRPId, x.Key.WarehouseId, x.Key.IsDispatchedFreeStock }).ToList();
                var itemNumbers = newdata.SelectMany(z => z.orderDetails).Select(x => x.itemNumber).Distinct().ToList();



                using (var authContext = new AuthContext())
                {
                    if (authContext.Database.Connection.State != ConnectionState.Open)
                        authContext.Database.Connection.Open();
                    var orderIdDt = new DataTable();
                    orderIdDt.Columns.Add("ItemMultiMRPId");
                    orderIdDt.Columns.Add("WarehouseId");
                    orderIdDt.Columns.Add("IsFreeItem");
                    foreach (var item in multimrpIds)
                    {
                        var dr = orderIdDt.NewRow();
                        dr["ItemMultiMRPId"] = item.ItemMultiMRPId;
                        dr["WarehouseId"] = item.WarehouseId;
                        dr["IsFreeItem"] = item.IsDispatchedFreeStock;
                        orderIdDt.Rows.Add(dr);
                    }
                    var param = new SqlParameter("Items", orderIdDt);
                    param.SqlDbType = SqlDbType.Structured;
                    param.TypeName = "dbo.itemtype";
                    var cmd = authContext.Database.Connection.CreateCommand();
                    cmd.CommandText = "[dbo].[GetCurrentStock]";
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.Parameters.Add(param);

                    // Run the sproc
                    using (var reader = cmd.ExecuteReader())
                    {
                        currentStocks = ((IObjectContextAdapter)authContext)
                                        .ObjectContext
                                        .Translate<CurrentStockMinDc>(reader).ToList();
                    }

                }
                var Orderitems = pendingOrders.SelectMany(x => x.orderDetails.Select(y => y.ItemId)).ToList();
                var allpendingOrders = mongoDbHelper.Select(x => !x.Deleted && x.Status == "Pending"
                                                           && x.orderDetails.Any(z => Orderitems.Contains(z.ItemId)), collectionName: "OrderMaster");

                allpendingOrders.ForEach(x => x.orderDetails.ForEach(y => y.WarehouseId = x.WarehouseId));
                var allPendingItems = allpendingOrders.SelectMany(x => x.orderDetails).GroupBy(x => new { x.ItemMultiMRPId, x.itemNumber, x.WarehouseId, x.IsDispatchedFreeStock }).Select(x => new
                {
                    x.Key.itemNumber,
                    x.Key.ItemMultiMRPId,
                    x.Key.WarehouseId,
                    IsFreeItem = x.Key.IsDispatchedFreeStock,
                    TotalReqQty = x.Sum(y => y.qty),
                    TotalAvlQty = currentStocks != null && currentStocks.Any(z => z.ItemNumber == x.Key.itemNumber && z.ItemMultiMRPId == x.Key.ItemMultiMRPId && z.IsFreeItem == x.Key.IsDispatchedFreeStock && z.WarehouseId == x.Key.WarehouseId) ? currentStocks.Where(z => z.ItemNumber == x.Key.itemNumber && z.ItemMultiMRPId == x.Key.ItemMultiMRPId && z.IsFreeItem == x.Key.IsDispatchedFreeStock && z.WarehouseId == x.Key.WarehouseId).Sum(y => y.CurrentInventory) : 0,
                    Orders = x.Select(y => new { y.OrderDetailsId, y.OrderId, y.qty }).ToList()
                }).ToList();



                foreach (var item in pendingOrders)
                {

                    var items = allPendingItems.Where(x => x.Orders.Any(y => y.OrderId == item.OrderId)).ToList();

                    if (items.All(y => y.TotalAvlQty >= y.TotalReqQty))
                    {
                        item.isCompleted = true;
                        item.IsLessCurrentStock = false;
                    }
                    else if (items.Any(y => y.Orders.Any(z => z.OrderId == item.OrderId && z.qty > y.TotalAvlQty)))
                    {
                        item.IsLessCurrentStock = true;
                        item.isCompleted = false;
                        if (items.Any(y => y.Orders.Any(z => z.OrderId == item.OrderId && z.qty > y.TotalAvlQty && y.IsFreeItem == true)))
                        {
                            var orderinfo = allpendingOrders.Where(x => x.OrderId == item.OrderId).Select(x => x.orderDetails.Where(y => y.IsFreeItem == true && y.IsDispatchedFreeStock == false).Select(z => new { z.itemNumber, z.ItemMultiMRPId, z.WarehouseId }).FirstOrDefault()).FirstOrDefault();
                            if (orderinfo != null)
                            {
                                //using (var authContext = new AuthContext())
                                //{
                                int freeitemqty = currentStocks.Where(z => z.ItemMultiMRPId == orderinfo.ItemMultiMRPId && z.IsFreeItem == false && z.WarehouseId == orderinfo.WarehouseId).Sum(y => y.CurrentInventory);
                                //authContext.DbCurrentStock.Where(x => x.ItemMultiMRPId == orderinfo.ItemMultiMRPId && x.WarehouseId == orderinfo.WarehouseId && x.ItemNumber == orderinfo.itemNumber).Select(x => x.CurrentInventory).FirstOrDefault();
                                if (items.All(y => y.Orders.All(z => z.OrderId == item.OrderId && z.qty <= freeitemqty && y.IsFreeItem == true)))
                                {
                                    item.IsLessCurrentStock = false;
                                    item.isCompleted = true;
                                }
                                //}
                            }
                        }
                    }
                    else if (items.All(y => y.Orders.Where(z => z.OrderId >= item.OrderId).Sum(x => x.qty) >= y.TotalAvlQty))
                    {
                        item.isCompleted = false;
                        item.IsLessCurrentStock = false;
                    }

                }
                #region CRMTAG

                var skcodeList = newdata.Select(x => x.Skcode).Distinct().ToList();

                //CRMManager cRMManager = new CRMManager();
                //var TagData = cRMManager.GetCRMCustomerWithTag(skcodeList, CRMPlatformConstants.OrderMaster);
                newdata.ForEach(x =>
                {
                    x.CRMTags = x.IsDigitalOrder.HasValue && x.IsDigitalOrder.Value ? "Digital" : null;
                    //TagData != null && TagData.Result != null ? TagData.Result.Where(y => y.Skcode == x.Skcode).Select(z => z.CRMTags).FirstOrDefault() : null;
                });

                #endregion
            }
            paggingData.ordermaster = newdata.OrderByDescending(x => x.CreatedDate);
            return paggingData;
        }

        private PaggingData GetOrdersFromDB(FilterOrderDTO filterOrderDTO, string Department)
        {
            PaggingData paggingData = new PaggingData();
            int skip = (filterOrderDTO.PageNo - 1) * filterOrderDTO.ItemPerPage;
            int take = filterOrderDTO.ItemPerPage;
            int totalItems = 0;
            List<SearchOrderMaster> orderData = new List<SearchOrderMaster>();

            var manager = new ItemLedgerManager();
            orderData = manager.SearchOrderMaster(filterOrderDTO, skip, take, out totalItems, Department);
            orderData.Where(x => !string.IsNullOrEmpty(x.SalesPerson)).ToList().ForEach(x =>
            {
                x.SalesPerson = string.Join(",", x.SalesPerson.Split(',').Distinct());
            });
            VANResponseController vANResponseController = new VANResponseController();
            orderData.ForEach(x =>
            {
                x.VANAmount = vANResponseController.GetCustomerRTGSAmount(Convert.ToInt32(x.CustomerId));
                x.Trupay = x.Trupay != null ? x.Trupay : "NA";
            });

            #region CRMTAG

            var skcodeList = orderData.Select(x => x.SkCode).Distinct().ToList();

            //CRMManager cRMManager = new CRMManager();
            //var TagData = cRMManager.GetCRMCustomerWithTag(skcodeList, CRMPlatformConstants.OrderMaster);
            orderData.ForEach(x =>
            {
                x.CRMTags = x.IsDigitalOrder.HasValue && x.IsDigitalOrder.Value ? "Digital" : null;
                //TagData != null && TagData.Result != null ? TagData.Result.Where(y => y.Skcode == x.SkCode).Select(z => z.CRMTags).FirstOrDefault() : null;
            });

            #endregion



            paggingData.ordermaster = orderData;
            paggingData.total_count = totalItems;
            return paggingData;
        }

        [Route("")]
        public IEnumerable<OrderDispatchedMaster> Get(string OrderStatus, string t)
        {
            logger.Info("start OrderMaster: ");
            List<OrderDispatchedMaster> ass = new List<OrderDispatchedMaster>();
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

                    logger.Info("User ID : {0} , Company Id : {1}", compid, userid);
                    ass = context.AllDispatchedOrderMaster(compid).ToList();
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


        [Route("")]
        [HttpPut]
        public async Task<string> Put(OrderMaster item)
        {

            string result = "";
            using (var context = new AuthContext())
            {
                try
                {
                    var identity = User.Identity as ClaimsIdentity;
                    int compid = 0, userid = 0;
                    string UserName = null;
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
                    string RoleNames = string.Empty;
                    if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "RoleNames"))
                        RoleNames = identity.Claims.FirstOrDefault(x => x.Type == "RoleNames").Value;

                    var cust = context.Customers.FirstOrDefault(x => x.CustomerId == item.CustomerId);
                    if (!cust.Active)
                    {
                        result = "Customer is Inactive";
                        return result;
                    }

                    #region IsDigitalCustomer
                    //CRMManager mg = new CRMManager();

                    var Order = context.DbOrderMaster.FirstOrDefault(x => x.OrderId == item.OrderId);
                    if (Order.IsDigitalOrder.HasValue && Order.IsDigitalOrder == true && Order.Status == "Pending")  /*(await mg.IsDigitalCustomer(cust.Skcode))*/
                    {
                        result = "Digital Customer Order can't be canceled.";
                        return result;
                    };
                    #endregion
                    if (Order.OrderType == 8)
                    {
                        result = "Clearance order can't be canceled ";
                        return result;
                    };

                    List<string> lstRoleName = string.IsNullOrEmpty(RoleNames) ? new List<string>() : RoleNames.Split(',').ToList();

                    if (item.Status == "Order Canceled" && lstRoleName != null && lstRoleName.Any())
                    {
                        if (!lstRoleName.Any(x => x.ToUpper() == "HQ MASTER LOGIN" || x.ToUpper() == "HQ CS EXECUTIVE"))
                        {
                            result = "You are not authorized canceled order";
                            return result;
                        }

                    }

                    //#region For OverDue Customer
                    //if (item.CustomerId > 0)
                    //{
                    //    bool IsCashBlocked = false;
                    //    CheckDueAmtDc Data = new CheckDueAmtDc();
                    //    MongoDbHelper<UdharOverDueDayValidation> UdharOverDueDay = new MongoDbHelper<UdharOverDueDayValidation>();
                    //    var DueAmt = UdharOverDueDay.GetAll();
                    //    var minDay = DueAmt.Min(x => x.MinOverDueDay);
                    //    var maxDay = DueAmt.Max(x => x.MaxOverDueDay);

                    //    if (DueAmt != null && DueAmt.Any(x => x.MinOverDueDay > 0) && DueAmt.Any(x => x.MaxOverDueDay > 0))
                    //    {
                    //        var param1 = new SqlParameter("@CustomerId", item.CustomerId);
                    //        Data = context.Database.SqlQuery<CheckDueAmtDc>("Exec CheckDueAmt @CustomerId ", param1).FirstOrDefault();

                    //        if (Data != null && Data.Amount >= 1 && Data.OverDueDays > minDay && Data.OverDueDays < maxDay)
                    //        {
                    //            IsCashBlocked= true;
                    //        }
                    //        else
                    //        {
                    //            IsCashBlocked= false;
                    //        }
                    //    }
                    //    else
                    //    {
                    //        IsCashBlocked= false;
                    //    }
                    //}
                    //#endregion


                    #region refund validation
                    bool IsOnlineRefundEnabled = false;
                    if(Order.paymentThrough!= null && Order.paymentThrough.Trim().ToLower() != "paylater")
                    {
                        IsOnlineRefundEnabled = context.Warehouses.FirstOrDefault(x => x.WarehouseId == item.WarehouseId).IsOnlineRefundEnabled;
                        var OnlineEntries = context.PaymentResponseRetailerAppDb.Where(z => z.OrderId == item.OrderId && z.IsOnline && z.status == "Success").ToList();
                        if (OnlineEntries != null && OnlineEntries.Any(z => z.IsOnline == true) && IsOnlineRefundEnabled == false)
                        {
                            result = "Online Order can't refund due online order refund is disabled on this warehouse";
                            return result;
                        }
                        if (IsOnlineRefundEnabled && OnlineEntries.Any(z => z.IsOnline == true))
                        {
                            var RefundDays = context.PaymentRefundApis.Where(x => x.DaysForRefundEligible > 0 && x.IsActive && x.IsDeleted == false).Select(x => new { x.ApiName, x.DaysForRefundEligible }).ToList();
                            if (RefundDays != null && RefundDays.Any())
                            {
                                foreach (var it in OnlineEntries)
                                {
                                    var PaymentRefundDays = RefundDays.FirstOrDefault(x => x.ApiName.Trim().ToLower() == it.PaymentFrom.Trim().ToLower());
                                    if (PaymentRefundDays != null && PaymentRefundDays.DaysForRefundEligible > 0 && it.CreatedDate.AddDays(PaymentRefundDays.DaysForRefundEligible) < DateTime.Now)
                                    {
                                        result = "Online Order can't cancelled, because online payment refund days expired for Order #" + item.OrderId;
                                        return result;
                                    }
                                    else if (PaymentRefundDays == null && it.PaymentFrom.Trim().ToLower() != "gullak" && it.PaymentFrom.Trim().ToLower() != "gullak")
                                    {
                                        result = "refund apis or refund days not configured for payment mode " + it.PaymentFrom;
                                        return result;
                                    }
                                }
                            }
                            else
                            {
                                result = "refund apis or refund days not configured";
                                return result;
                            }
                        }
                    }
                    #endregion

                    if (!context.OrderDispatchedMasters.Any(x => x.OrderId == item.OrderId))
                    {
                        item.CompanyId = compid;
                        item.userid = userid;
                        logger.Info("User ID : {0} , Company Id : {1}", compid, userid);
                        return await asyncPutOrderMaster(item, context, IsOnlineRefundEnabled);
                    };

                    return null;
                }
                catch (Exception ex)
                {
                    return null;
                }
            }
        }

        //Vaibhaw
        [Route("IsCODBlocked")]
        [HttpGet]
        public async Task<bool> IsCODBlocked(int customerid)
        {
            using (var context = new AuthContext())
            {
                CheckDueAmtDc Data = new CheckDueAmtDc();
                MongoDbHelper<UdharOverDueDayValidation> UdharOverDueDay = new MongoDbHelper<UdharOverDueDayValidation>();

                var DueAmt = UdharOverDueDay.GetAll();
                var minDay = 0;
                var maxDay = 0;
                if (DueAmt != null && DueAmt.Any())
                {
                    minDay = DueAmt.Min(x => x.MinOverDueDay);
                    maxDay = DueAmt.Max(x => x.MaxOverDueDay);
                }

                if (customerid > 0 && DueAmt != null)
                {
                    var param1 = new SqlParameter("@CustomerId", customerid);
                    Data = context.Database.SqlQuery<CheckDueAmtDc>("Exec CheckDueAmt @CustomerId ", param1).FirstOrDefault();

                    if (Data != null && Data.Amount >= 1 && Data.OverDueDays > minDay && Data.OverDueDays < maxDay)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }


                }
                else
                {
                    return false;
                }
            }
        }




        [ResponseType(typeof(OrderMaster))]
        [Route("")]
        public IEnumerable<OrderMaster> Get(string Cityid, string Warehouseid, DateTime datefrom, DateTime dateto, string search, string status, string deliveryboy)
        {
            logger.Info("start OrderMaster: ");
            List<OrderMaster> ass = new List<OrderMaster>();
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

                    logger.Info("User ID : {0} , Company Id : {1}", compid, userid);
                    ass = context.filteredOrderMaster(Cityid, Warehouseid, datefrom, dateto, search, status, deliveryboy, compid).ToList();
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

        [ResponseType(typeof(OrderMaster))]
        [Route("")]
        public IEnumerable<OrderMaster> Get(string mobile)
        {
            logger.Info("start OrderMaster: ");
            List<OrderMaster> ass = new List<OrderMaster>();
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

                    logger.Info("User ID : {0} , Company Id : {1}", compid, userid);

                    ass = context.OrderMasterbymobile(mobile, compid).ToList();
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

        [Authorize]
        [ResponseType(typeof(OrderMaster))]
        [Route("")]
        [AcceptVerbs("POST")]
        //[Route("api/ItemMaster")]
        public OrderMaster add(OrderMaster item)
        {
            logger.Info("start add OrderMaster: ");
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

                    item.CompanyId = compid;
                    if (item == null)
                    {
                        throw new ArgumentNullException("item");
                    }

                    context.AddOrderMaster(item);
                    logger.Info("User ID : {0} , Company Id : {1}", compid, userid);
                    logger.Info("End  add OrderMaster: ");
                    return item;

                }
                catch (Exception ex)
                {
                    logger.Error("Error in AddOrderMaster " + ex.Message);

                    return null;
                }
            }
        }
        //[Authorize]
        //[ResponseType(typeof(OrderMaster))]
        //[Route("")]
        //[AcceptVerbs("PUT")]
        //public OrderMaster Put(OrderMaster item)
        //{
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
        //        return context.PutOrderMaster(item);
        //    }
        //    catch
        //    {
        //        return null;
        //    }
        //}

        [ResponseType(typeof(OrderMaster))]
        [Route("")]
        [AcceptVerbs("Delete")]
        public void Remove(int id)
        {

            logger.Info("start delete OrderMaster: ");
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
                    context.DeleteOrderMaster(id, compid);
                    logger.Info("End  delete OrderMaster: ");
                }
                catch (Exception ex)
                {
                    logger.Error("Error in delete OrderMaster " + ex.Message);
                }
            }
        }
        [Route("")]
        public IEnumerable<OrderMaster> Get(string Warehouseid, DateTime datefrom, DateTime dateto)
        {
            logger.Info("start OrderMaster: ");
            List<OrderMaster> ass = new List<OrderMaster>();
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

                    logger.Info("User ID : {0} , Company Id : {1}", compid, userid);
                    ass = context.filteredOrderMasters1(Warehouseid, datefrom, dateto, compid).ToList();
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
        #region get data first time order customer
        /// <summary>
        /// Get all customer detail first time customer
        /// create date 05/03/2019
        /// </summary>
        /// <param name="salespersonid"></param>
        /// <returns></returns>
        [Route("getdetail")]
        [HttpGet]
        public HttpResponseMessage get(int Warehouseid, DateTime start, DateTime end)
        {
            using (AuthContext db = new AuthContext())
            {
                List<FirstTimeOrderDTO> fto = new List<FirstTimeOrderDTO>();
                var Customerids = db.Customers.Where(x => x.Deleted == false && x.ordercount == 1 && x.Warehouseid == Warehouseid && x.CreatedDate > start && x.CreatedDate < end).Select(x => x.CustomerId).ToList();
                foreach (var cItems in Customerids)
                {
                    OrderMaster todaydata = db.DbOrderMaster.Where(x => x.CustomerId == cItems && x.Status == "Pending").FirstOrDefault();
                    if (todaydata != null)
                    {
                        City cityname = db.Cities.Where(x => x.Cityid == todaydata.CityId).FirstOrDefault();
                        FirstTimeOrderDTO BData = new FirstTimeOrderDTO()
                        {
                            ShopName = todaydata.ShopName,
                            Skcode = todaydata.Skcode,
                            WarehouseId = todaydata.WarehouseId,
                            WarehouseName = todaydata.WarehouseName,
                            CityName = cityname.CityName,
                            CustomerId = todaydata.CustomerId,
                            CustomerName = todaydata.CustomerName,
                            Mobile = todaydata.Customerphonenum,
                            Address = todaydata.ShippingAddress,
                            amount = todaydata.TotalAmount,
                            DateOfPurchase = todaydata.CreatedDate
                        };
                        fto.Add(BData);

                    }
                }

                return Request.CreateResponse(HttpStatusCode.OK, fto);
            }
        }


        #endregion
        #region get orderhistory
        [Route("orderhistory")]
        [HttpGet]
        public dynamic orderhistory(int orderId)
        {
            using (AuthContext odd = new AuthContext())
            {
                try
                {
                    var identity = User.Identity as ClaimsIdentity;
                    int compid = 0, userid = 0, Warehouse_id = 0;
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
                        if (claim.Type == "Warehouseid")
                        {
                            Warehouse_id = int.Parse(claim.Value);
                        }
                    }
                    int CompanyId = compid;
                    logger.Info("User ID : {0} , Company Id : {1}", compid, userid);

                    var data = odd.OrderMasterHistoriesDB.Where(x => x.orderid == orderId).OrderByDescending(c => c.id).ToList();
                    if (data != null && data.Any())
                    {
                        var list = data.Where(x => x.IsReAttempt == true).ToList();
                        if (list != null)
                        {
                            foreach (var item in list)
                            {
                                item.Status = "ReAttempt";
                            }
                        }
                    }
                    return data;
                }
                catch (Exception ex)
                {
                    return false;
                }
            }
        }
        #endregion
        #region get Itemhistory
        [Route("Itemhistory")]
        [HttpGet]
        public dynamic Itemhistory(int orderId)
        {
            using (AuthContext odd = new AuthContext())
            {
                try
                {
                    var identity = User.Identity as ClaimsIdentity;
                    int compid = 0, userid = 0, Warehouse_id = 0;
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
                        if (claim.Type == "Warehouseid")
                        {
                            Warehouse_id = int.Parse(claim.Value);
                        }
                    }
                    int CompanyId = compid;
                    logger.Info("User ID : {0} , Company Id : {1}", compid, userid);

                    var data = odd.OrderItemHistoryDB.Where(x => x.orderid == orderId).ToList();
                    return data;
                }
                catch (Exception ex)
                {
                    return false;
                }
            }
        }
        #endregion
        //[AllowAnonymous]
        //#region get Itemhistory
        //[Route("OfferCode")]
        //[HttpGet]
        //public dynamic Itemhistory(OrderMasterDc OCode)
        //{
        //    using (AuthContext odd = new AuthContext())
        //    {
        //        try
        //        {
        //            var identity = User.Identity as ClaimsIdentity;
        //            int compid = 0, userid = 0, Warehouse_id = 0;
        //            // Access claims
        //            foreach (Claim claim in identity.Claims)
        //            {
        //                if (claim.Type == "compid")
        //                {
        //                    compid = int.Parse(claim.Value);
        //                }
        //                if (claim.Type == "userid")
        //                {
        //                    userid = int.Parse(claim.Value);
        //                }
        //                if (claim.Type == "Warehouseid")
        //                {
        //                    Warehouse_id = int.Parse(claim.Value);
        //                }
        //            }
        //            int CompanyId = compid;
        //            logger.Info("User ID : {0} , Company Id : {1}", compid, userid);

        //            var data = odd.DbOrderMaster.Where(x => x.OfferCode == OCode.OfferCode).ToList();
        //            return data;
        //        }
        //        catch (Exception ex)
        //        {
        //            return false;
        //        }
        //    }
        //}
        //#endregion


        #region Order Tranaction Processing update  
        /// <summary>
        /// Created Date 19/03/2019
        /// </summary>
        /// <param name="OrderId"></param>
        /// <returns>RD</returns>
        public OrderTransactionProcessing AddOrderTranaction(int OrderId, bool isdata)
        {
            using (var db = new AuthContext())
            {
                try
                {
                    OrderTransactionProcessing OTP = db.OrderTransactionProcessingDB.Where(X => X.OrderId == OrderId).FirstOrDefault();
                    OrderTransactionProcessing TP = new OrderTransactionProcessing();
                    if (OTP == null)
                    {

                        TP.OrderId = OrderId;
                        TP.IsDispatched = isdata;
                        TP.CreatedDate = DateTime.Now;
                        db.OrderTransactionProcessingDB.Add(TP);
                        db.Commit();


                    }
                    return TP;

                }
                catch (Exception ex)
                {
                    logger.Error("Error in put Complain " + ex.Message);
                    return null;

                }
            }
        }
        #endregion


        /// <summary>
        /// Check Order In Offer used  17/06/2019
        /// </summary>
        /// <param name="OrderId"></param>
        /// <param name="CustomerId"></param>
        /// <returns></returns>
        [Authorize]
        [Route("CheckOrderInOffer")]
        [HttpGet]
        public HttpResponseMessage CheckOrderInOffer(int OrderId, int CustomerId)
        {
            logger.Info("start Check Order if it In Offer Bill  Discount Used : ");
            {
                using (var db = new AuthContext())
                {
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
                            BillDiscount BillDiscountUsed = new BillDiscount();
                            logger.Info("Check Order if it In Offer Bill User ID : {0} , Company Id : {1}", compid, userid);
                            using (AuthContext context = new AuthContext())
                            {
                                BillDiscountUsed = context.BillDiscountDb.Where(x => x.CustomerId == CustomerId && x.OrderId == OrderId).FirstOrDefault();
                                if (CustomerId > 0 && BillDiscountUsed != null)
                                {
                                    return Request.CreateResponse(HttpStatusCode.OK, BillDiscountUsed);
                                }
                            }
                            logger.Info("End  Check Order if it In Offer Bill: ");
                            return Request.CreateResponse(HttpStatusCode.OK, BillDiscountUsed);
                        }
                        catch (Exception ex)
                        {
                            logger.Error("Error in Check Order if it In Offer Bill " + ex.Message);
                            return Request.CreateResponse(HttpStatusCode.BadRequest, "Error in Check Order if it In Offer Bill " + ex.Message);
                        }
                    }
                }
            }
        }

        [HttpGet]
        [ResponseType(typeof(OrderMaster))]
        [Route("GetByCustomerId")]
        public IEnumerable<OrderMaster> GetByCustomerId(int id)
        {
            logger.Info("start OrderMaster: ");
            List<OrderMaster> ass = new List<OrderMaster>();
            using (var db = new AuthContext())
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

                    logger.Info("User ID : {0} , Company Id : {1}", compid, userid);

                    ass = db.DbOrderMaster.Where(ord => ord.CustomerId == id).ToList();
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

        [HttpGet]
        [Route("GetTradeInvoice")]
        public string GetTradeInvoice(int OrderId)
        {
            InvoicePdfResponse invoicePdfResponse = new InvoicePdfResponse();
            // string barcode = "";
            string fileUrl = "";
            using (var db = new AuthContext())
            {
                var order = db.DbOrderMaster.FirstOrDefault(x => x.OrderId == OrderId);
                invoicePdfResponse.barcode = order.InvoiceBarcodeImage;
                InvoicePdfRequest InvoicePdfRequest = new InvoicePdfRequest
                {
                    InvoiceNo = order.invoice_no
                };
                string FileName = "";
                FileName = order.invoice_no.Replace('/', '_') + ".pdf";
                var folderPath = HttpContext.Current.Server.MapPath(@"~\ZaruriReportDownloads");
                if (!Directory.Exists(folderPath))
                    Directory.CreateDirectory(folderPath);

                fileUrl = "/ZaruriReportDownloads" + "/" + FileName;
                var fullPhysicalPath = folderPath + "\\" + FileName;
                if (!File.Exists(fullPhysicalPath))
                {
                    var tradeUrl = ConfigurationManager.AppSettings["TradeURL"] + "/api/TradeOrders/InvoicePdf";
                    using (var client = new HttpClient())
                    {
                        client.DefaultRequestHeaders.Add("CustomerId", "1");
                        client.DefaultRequestHeaders.Add("NoEncryption", "1");
                        var newJson = JsonConvert.SerializeObject(InvoicePdfRequest);
                        using (var content = new StringContent(newJson, Encoding.UTF8, "application/json"))
                        {
                            var response = AsyncContext.Run(() => client.PostAsync(tradeUrl, content));
                            response.EnsureSuccessStatusCode();
                            string responseBody = response.Content.ReadAsStringAsync().Result;
                            var result = JsonConvert.DeserializeObject<ResponseMetaData>(responseBody);
                            if (result.Status != "Error")
                            {

                                invoicePdfResponse.html = result.Data;
                                string base64String = Convert.ToBase64String(invoicePdfResponse.barcode, 0, invoicePdfResponse.barcode.Length);
                                //data:image/png;base64,"+ base64String + "
                                invoicePdfResponse.html = invoicePdfResponse.html.Replace("@BARCODE", "<img src='data:image/png;base64," + base64String + "' style='height:70px;float: right;'>");
                                byte[] pdf = Pdf
                                                .From(invoicePdfResponse.html)
                                                //.WithGlobalSetting("orientation", "Landscape")
                                                //.WithObjectSetting("web.defaultEncoding", "utf-8")
                                                .OfSize(PaperSize.A4)
                                                .WithTitle("Invoice")
                                                .WithoutOutline()
                                                .WithMargins(PaperMargins.All(0.0.Millimeters()))
                                                .Portrait()
                                                .Comressed()
                                                .Content();

                                FileStream file = File.Create(fullPhysicalPath);
                                file.Write(pdf, 0, pdf.Length);
                                file.Close();
                            }
                        }
                    }
                }
                else
                {
                    fileUrl = "/ZaruriReportDownloads" + "/" + FileName;
                }

                fileUrl = string.Format("{0}://{1}:{2}/{3}", HttpContext.Current.Request.Url.Scheme
                                                              , HttpContext.Current.Request.Url.DnsSafeHost
                                                              , HttpContext.Current.Request.Url.Port
                                                              , fileUrl);
            }
            return fileUrl;
        }


        [HttpGet]
        [Route("getSuminvoiceHSNCodeData")]
        public List<getSuminvoiceHSNCodeDataDC> getSuminvoiceHSNCodeData(int OrderId)
        {
            using (var db = new AuthContext())
            {
                OrderMaster OrderMasterData = db.DbOrderMaster.Where(x => x.OrderId == OrderId).Include("OrderDetails").FirstOrDefault();
                var AddDatalists = OrderMasterData.orderDetails.Where(z => z.OrderId == OrderId).GroupBy(x => new { x.HSNCode }).Select(x => new getSuminvoiceHSNCodeDataDC
                {
                    HSNCode = x.Key.HSNCode,
                    AmtWithoutTaxDisc = x.Sum(y => y.AmtWithoutTaxDisc),
                    SGSTTaxAmmount = x.Sum(y => y.SGSTTaxAmmount),
                    CGSTTaxAmmount = x.Sum(y => y.CGSTTaxAmmount),
                    TaxAmmount = x.Sum(y => y.TaxAmmount),
                    CessTaxAmount = x.Sum(y => y.CessTaxAmount),
                    TotalSum = x.Sum(y => y.AmtWithoutTaxDisc + y.SGSTTaxAmmount + y.CGSTTaxAmmount)
                }).ToList();
                return AddDatalists;
            }
        }

        [HttpGet]
        [Route("RTDgetSuminvoiceHSNCodeData")]
        public List<getSuminvoiceHSNCodeDataDC> RTDgetSuminvoiceHSNCodeData(int OrderId)
        {
            using (var db = new AuthContext())
            {
                OrderDispatchedMaster RTDOrderMasterData = db.OrderDispatchedMasters.Where(x => x.OrderId == OrderId).Include("OrderDetails").FirstOrDefault();
                var AddDatalists = RTDOrderMasterData.orderDetails.Where(z => z.OrderId == OrderId && z.Noqty > 0 && z.qty > 0).GroupBy(x => new { x.HSNCode }).Select(x => new getSuminvoiceHSNCodeDataDC
                {
                    customerType=RTDOrderMasterData.CustomerType,
                    HSNCode = x.Key.HSNCode,
                    AmtWithoutTaxDisc = x.Sum(y => y.AmtWithoutTaxDisc),
                    SGSTTaxAmmount = x.Sum(y => y.SGSTTaxAmmount),
                    CGSTTaxAmmount = x.Sum(y => y.CGSTTaxAmmount),
                    TaxAmmount = x.Sum(y => y.TaxAmmount),
                    CessTaxAmount = x.Sum(y => y.CessTaxAmount),
                    TotalSum = x.Sum(y => y.AmtWithoutTaxDisc + y.SGSTTaxAmmount + y.CGSTTaxAmmount)
                }).ToList();
                return AddDatalists;
            }
        }


        [HttpGet]
        [Route("ShowOtp")]
        public bool ShowOtp(int OrderId, string Otp)
        {
            bool result = false;
            var identity = User.Identity as ClaimsIdentity;
            int userid = 0;
            string username = "";
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "username"))
                username = identity.Claims.FirstOrDefault(x => x.Type == "username").Value;

            string status = "";

            using (var db = new AuthContext())
            {
                var Dept = db.Peoples.Where(x => x.PeopleID == userid && x.Active == true && x.Deleted == false).FirstOrDefault();
                string query = "select distinct r.Name as Role from People p inner join AspNetUsers u on p.Email=u.Email inner join AspNetUserRoles ur on u.Id=ur.UserId inner join AspNetRoles r on ur.RoleId=r.Id where PeopleID='" + userid + "'and ur.isActive=1 and p.Active=1 and p.Deleted=0";
                var role = db.Database.SqlQuery<string>(query).ToList();

                List<string> rolelist = new List<string>();
                rolelist.Add("HQ Master login");
                rolelist.Add("Customer support executive");
                rolelist.Add("Customer support senior executive");
                rolelist.Add("Customer support associates");
                rolelist.Add("HQ CS LEAD");
                rolelist.Add("Hub service lead");
                rolelist.Add("Sales Executive");
                rolelist.Add("Telecaller");

                var IsRole = role.Any(a => rolelist.Contains(a));
                var orderOtp = db.OrderDeliveryOTP.FirstOrDefault(x => x.OrderId == OrderId && x.OTP == Otp && x.IsActive );
                // var UsedOtp = db.OrderDeliveryOTP.FirstOrDefault(x => x.OrderId == OrderId && x.IsUsed);

                if (orderOtp != null && IsRole && (orderOtp.Status == "Delivery Redispatch" || orderOtp.Status == "Delivery Canceled") && (orderOtp.UserType == Dept.Department) && orderOtp.IsUsed == false)
                {
                    status = orderOtp.Status;
                    orderOtp.UserId = userid;
                    db.Entry(orderOtp).State = EntityState.Modified;
                    result = (db.Commit() > 0);
                    return result;

                }
                if (orderOtp != null && orderOtp.Status != "Delivery Redispatch" && orderOtp.Status != "Delivery Canceled" && orderOtp.IsUsed == false)
                {
                    status = orderOtp.Status;
                    orderOtp.UserId = userid;
                    db.Entry(orderOtp).State = EntityState.Modified;
                    
                    result = (db.Commit() > 0);
                    return result;
                }
                if (orderOtp != null && orderOtp.Status == "Delivery Redispatch" && orderOtp.UserType == "HQ Operation(ReAttempt)" && orderOtp.IsUsed == false)
                {
                    status = orderOtp.Status;
                    orderOtp.UserId = userid;
                    db.Entry(orderOtp).State = EntityState.Modified;

                    result = (db.Commit() > 0);
                    return result;
                }
                if (orderOtp != null)
                {
                    status = orderOtp.Status;

                }
                return result;
            }

        }


        [HttpGet]
        [Route("ShowReason")]
        public string ShowReason(int OrderId)
        {
            using (var db = new AuthContext())
            {
                return db.OrderDispatchedMasters.FirstOrDefault(x => x.OrderId == OrderId).comments;
            }

        }


        [HttpGet]
        [Route("GetLastOtpStatus")]
        public string ShowOtp(int OrderId)
        {
            string status = "";
            using (var db = new AuthContext())
            {
                var identity = User.Identity as ClaimsIdentity;
                int userid = 0;
                string UserDept = "";
                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                    userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "Department"))
                    UserDept = (identity.Claims.FirstOrDefault(x => x.Type == "Department").Value);

               // var Dept = db.Peoples.Where(x => x.PeopleID == userid && x.Active == true && x.Deleted == false).FirstOrDefault();
                string query = "select distinct r.Name as Role from People p inner join AspNetUsers u on p.Email=u.Email inner join AspNetUserRoles ur on u.Id=ur.UserId inner join AspNetRoles r on ur.RoleId=r.Id where PeopleID='" + userid + "'and ur.isActive=1 and p.Active=1 and p.Deleted=0";
                var role = db.Database.SqlQuery<string>(query).ToList();

                List<string> rolelist = new List<string>();
                rolelist.Add("HQ Master login");
                rolelist.Add("Customer support executive");
                rolelist.Add("Customer support senior executive");
                rolelist.Add("Customer support associates");
                rolelist.Add("HQ CS LEAD");
                rolelist.Add("Hub service lead");
                rolelist.Add("Sales Executive");
                rolelist.Add("Telecaller");

                var IsRole = role.Any(a => rolelist.Contains(a));

                if (userid > 0 && IsRole)
                {
                    var orderOtps = db.OrderDeliveryOTP.Where(x => x.OrderId == OrderId && x.IsActive == true && x.IsUsed == false).ToList();
                    if (orderOtps != null && orderOtps.Count() > 0)
                    {
                        status = orderOtps.FirstOrDefault().Status;
                    }
                    else
                    {
                        status = "";
                    }
                }
                else
                {
                    status = "You are not authorized";
                }
            }
            return status;
        }


        [HttpGet]
        [Route("GetDigitalOtp")]
        public async Task<responceDc> GetDigitalOtp(int OrderId)
        {
            var res = new responceDc();
            using (var db = new AuthContext())
            {
                var identity = User.Identity as ClaimsIdentity;
                int userid = 0;

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                    userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

                List<string> roleNames = new List<string>();
                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "RoleNames") && identity.Claims.FirstOrDefault(x => x.Type == "RoleNames").Value != "")
                    roleNames = (identity.Claims.FirstOrDefault(x => x.Type == "RoleNames").Value).Split(',').ToList();
                var IsRole = roleNames.Any(a => roleNames.Contains("Telecaller"));
                if (userid > 0 && IsRole)
                {
                    var orderOtp = db.OrderDeliveryOTP.Where(x => x.OrderId == OrderId && x.IsActive == true && (x.UserType == "Sales" || x.Status == "Delivered")).OrderBy(x => x.Id).FirstOrDefault();
                    if (orderOtp != null)
                    {
                        res.Status = true;
                        res.Message = "OTP of " + orderOtp.Status + " status is " + orderOtp.OTP;
                    }
                    else
                    {
                        res.Message = "No OTP found";
                    }
                }
                else
                {
                    res.Message = "You are not authorized";
                }
            }
            return res;
        }


        [HttpGet]
        [Route("GetTCSPercent")]

        public double GetTCSPercent(int CustomerId)
        {
            double percent = 0;
            using (var context = new AuthContext())
            {
                var Cust = context.Customers.Where(x => x.CustomerId == CustomerId).FirstOrDefault();

                string fy = (DateTime.Now.Month >= 4 ? DateTime.Now.Year + 1 : DateTime.Now.Year).ToString();

                MongoDbHelper<TCSConfig> mConfigHelper = new MongoDbHelper<TCSConfig>();
                var tcsConfig = mConfigHelper.Select(x => x.FinancialYear == fy).FirstOrDefault();
                if (tcsConfig != null && Cust != null)
                {
                    percent = !Cust.IsPanVerified ? tcsConfig.NotGSTTCSPercent : tcsConfig.GSTTCSPercent;
                }
            }
            return percent;
        }

        [HttpGet]
        [Route("GetStateCode")]
        public List<GetStateCodeDc> GetStateCode(int warehouseID)
        {
            List<GetStateCodeDc> GetstateCode = new List<GetStateCodeDc>();

            using (var context = new AuthContext())
            {

                var warehouse = new SqlParameter("@warehouseId", warehouseID);
                GetstateCode = context.Database.SqlQuery<GetStateCodeDc>("EXEC GetStateByWarehouseID @warehouseId", warehouse).ToList();
                if (GetstateCode != null)
                {
                    return GetstateCode;
                }
                return GetstateCode;

            }
            // string fy = (DateTime.Now.Month >= 4 ? DateTime.Now.Year + 1 : DateTime.Now.Year).ToString();

            //MongoDbHelper<TCSConfig> mConfigHelper = new MongoDbHelper<TCSConfig>();
            //var tcsConfig = mConfigHelper.Select(x => x.FinancialYear == fy).FirstOrDefault();
            //if (tcsConfig != null)
            //{
            //    percent = string.IsNullOrEmpty(refno) ? tcsConfig.NotGSTTCSPercent : tcsConfig.GSTTCSPercent;
            //}

        }
        public class GetStateCodeDc
        {
            public int CustomerId { get; set; }
            public string shippingStateName { get; set; }
            public string shippingStateCode { get; set; }

            public string BillingStateName { get; set; }
            public string BillingStateCode { get; set; }


        }





        [HttpPost]
        [Route("RebookOrder")]
        public HttpResponseMessage CreateRebookOrder(RebookOderDC rebookdc)
        {

            try
            {
                var identity = User.Identity as ClaimsIdentity;
                int userid = 0;
                foreach (Claim claim in identity.Claims)
                {
                    if (claim.Type == "userid")
                    {
                        userid = int.Parse(claim.Value);
                    }
                }
                rebookdc.UserId = userid;
                API.Helper.RebookOrderHelper helper = new API.Helper.RebookOrderHelper();

                string Result = helper.CreateRebookOrder(rebookdc, out bool _status);

                var response = new
                {
                    message = Result,
                    status = _status
                };
                return Request.CreateResponse(HttpStatusCode.OK, response);
            }
            catch (Exception ex)
            {
                var response = new
                {
                    message = "Error : " + ex.Message,
                    status = false
                };
                return Request.CreateResponse(HttpStatusCode.OK, response);
            }
        }
        public async Task<string> asyncPutOrderMaster(OrderMaster objcity, AuthContext context, bool IsOnlineRefundEnabled)
        {
            string result = "";

            var cust = context.Customers.FirstOrDefault(x => x.CustomerId == objcity.CustomerId);
            if (!cust.Active)
            {
                result = "Customer is Inactive";
                return result;

            }

            OrderMaster om = context.DbOrderMaster.Where(x => x.OrderId == objcity.OrderId && x.Deleted == false && x.CompanyId == objcity.CompanyId).Include("orderDetails").FirstOrDefault();
            if (om != null)
            {
                MongoDbHelper<ExtandedCompanyDetail> mongoDbHelper = new MongoDbHelper<ExtandedCompanyDetail>();
                var extandedCompanyDetail = mongoDbHelper.Select(x => x.WarehouseId == om.WarehouseId && x.AppType == "DON").FirstOrDefault();
                int CODCharges = 0;
                if (extandedCompanyDetail != null)
                {
                    CODCharges = extandedCompanyDetail.DONCODChanges ?? 0;
                }


                var oldstatus = om.Status;
                om.ReasonCancle = objcity.ReasonCancle;
                om.Status = objcity.Status;
                if (om.Status == "Pending" && om.OrderType == 4)
                {
                    om.GrossAmount += CODCharges;
                    om.TotalAmount += CODCharges;
                    om.deliveryCharge += CODCharges;


                }
                if ((om.Status == "Pending" || om.Status == "InTransit") && (oldstatus == "InTransit" || oldstatus == "Inactive" || oldstatus == "Payment Pending" || oldstatus == "Failed"))
                {
                    //update payement 
                    var PaymentDetails = context.PaymentResponseRetailerAppDb.Where(z => z.OrderId == om.OrderId && z.PaymentFrom == "Cash" && z.status == "Success").ToList();
                    if (PaymentDetails != null && PaymentDetails.Any())
                    {
                        foreach (var item in PaymentDetails)
                        {
                            item.UpdatedDate = DateTime.Now;
                            item.status = "Failed";
                            item.statusDesc = "AuthContext.PutOrderMaster from ordermaster edit change by";
                            context.Entry(item).State = EntityState.Modified;
                        }
                    }
                    om.paymentMode = "COD";
                    om.paymentThrough = "Cash";
                    var PaymentResponse = new PaymentResponseRetailerApp
                    {
                        amount = om.GrossAmount,
                        CreatedDate = DateTime.Now,
                        currencyCode = "INR",
                        OrderId = om.OrderId,
                        PaymentFrom = "Cash",
                        status = "Success",
                        statusDesc = "AuthContext.PutOrderMaster from ordermaster edit change by",
                        UpdatedDate = DateTime.Now,
                        IsRefund = false
                    };
                    context.PaymentResponseRetailerAppDb.Add(PaymentResponse);
                }


                foreach (var od in om.orderDetails)
                {
                    var Odet = context.DbOrderDetails.Where(x => x.OrderDetailsId == od.OrderDetailsId).SingleOrDefault();
                    if (Odet != null)
                    {
                        if ((om.Status == "Pending" || om.Status == "InTransit") && (oldstatus == "InTransit" || oldstatus == "Inactive" || oldstatus == "Payment Pending" || oldstatus == "Failed"))
                        {
                            Odet.Status = om.Status;
                        }
                        else
                        {
                            Odet.Status = "Order Canceled";
                        }
                        Odet.UpdatedDate = DateTime.Now;
                        context.Entry(Odet).State = EntityState.Modified;
                    }
                }
                int POCCancelationCharges = 0;
                if (om.OrderType == 4)
                {
                    if (extandedCompanyDetail != null)
                        POCCancelationCharges = extandedCompanyDetail.DeliveryCancelationPer ?? 0;
                }
                if (IsOnlineRefundEnabled)
                {
                    // Pre order cancel payment refund  -- April2022
                    #region Pre order cancel payment refund  -- April2022
                    //case 1 : online payment list
                    if (om.Status == "Order Canceled" && oldstatus != "Order Canceled" && !om.Deleted)
                    {
                        var OnlineEntries = context.PaymentResponseRetailerAppDb.Where(z => z.OrderId == om.OrderId && z.IsOnline && z.status == "Success").ToList();
                        if (OnlineEntries != null && OnlineEntries.Any() && OnlineEntries.Sum(x => x.amount) > 0)
                        {
                            PaymentRefundHelper PRHelper = new PaymentRefundHelper();
                            foreach (var item in OnlineEntries.OrderBy(c => c.RefundPriority))
                            {
                                double RefundAmount = item.amount;
                                if (RefundAmount > 0)
                                {
                                    var PaymentResponseRetailerAppDb = new PaymentResponseRetailerApp
                                    {
                                        amount = (-1) * RefundAmount,
                                        CreatedDate = DateTime.Now,
                                        currencyCode = "INR",
                                        OrderId = om.OrderId,
                                        PaymentFrom = item.PaymentFrom,
                                        GatewayTransId = item.GatewayTransId,
                                        GatewayOrderId = item.GatewayOrderId,
                                        status = "Success",
                                        UpdatedDate = DateTime.Now,
                                        IsRefund = false,
                                        IsOnline = true,
                                        statusDesc = "Refund Initiated"
                                    };
                                    context.PaymentResponseRetailerAppDb.Add(PaymentResponseRetailerAppDb);
                                    context.Commit();
                                    // addd Refund request
                                    var PaymentRefundRequestDc = new PaymentRefundRequestDc
                                    {
                                        Amount = RefundAmount,
                                        OrderId = PaymentResponseRetailerAppDb.OrderId,
                                        Source = PaymentResponseRetailerAppDb.PaymentFrom,
                                        ReqGatewayTransId = PaymentResponseRetailerAppDb.GatewayTransId,
                                        Status = (int)PaymentRefundEnum.Initiated,
                                        CreatedBy = objcity.userid,
                                        CreatedDate = DateTime.Now,
                                        IsActive = true,
                                        IsDeleted = false,
                                        ModifiedBy = objcity.userid,
                                        ModifiedDate = DateTime.Now,
                                        PaymentResponseRetailerAppId = PaymentResponseRetailerAppDb.id

                                    };
                                    bool IsInserted = PRHelper.InsertPaymentRefundRequest(context, PaymentRefundRequestDc);
                                }
                            }
                        }
                    }
                    #endregion
                }
                om.UpdatedDate = DateTime.Now;
                context.Entry(om).State = EntityState.Modified;
                context.Commit();

                #region Order Master History
                var UserName = context.Peoples.Where(x => x.PeopleID == objcity.userid).Select(a => a.DisplayName).FirstOrDefault();
                //var orderdata1 = context.DbOrderMaster.Where(x => x.OrderId == objcity.OrderId).FirstOrDefault();
                try
                {
                    Model.OrderMasterHistories h1 = new Model.OrderMasterHistories();

                    h1.orderid = om.OrderId;
                    h1.Status = om.Status;

                    h1.Reasoncancel = om.ReasonCancle;
                    h1.Warehousename = om.WarehouseName;
                    h1.userid = objcity.userid;
                    h1.username = UserName;
                    h1.CreatedDate = DateTime.Now;
                    h1.Description = objcity.Description;

                    context.OrderMasterHistoriesDB.Add(h1);
                    context.Commit();

                }
                catch (Exception ex)
                {
                    logger.Error("Error loading  \n\n" + ex.Message + "\n\n" + ex.InnerException + "\n\n" + ex.StackTrace);
                }
                #endregion


                if (om.Status == "Pending")
                {
                }
                else
                {
                    try
                    {
                        var rpoint = context.RewardPointDb.Where(c => c.CustomerId == om.CustomerId).FirstOrDefault();
                        if (rpoint != null)
                        {
                            if (om.RewardPoint > 0)
                            {
                                rpoint.EarningPoint -= om.RewardPoint;
                                if (rpoint.EarningPoint < 0)
                                    rpoint.EarningPoint = 0;
                                rpoint.UpdatedDate = DateTime.Now;
                                rpoint.TransactionDate = DateTime.Now;
                                //RewardPointDb.Attach(rpoint);
                                context.Entry(rpoint).State = EntityState.Modified;
                                context.Commit();
                            }
                        }
                        // on pre order canceeled 
                        if (om.Status == "Order Canceled")
                        {

                            var data123 = context.WalletDb.Where(x => x.CustomerId == om.CustomerId).SingleOrDefault();
                            if (om.walletPointUsed > 0 && om.walletPointUsed != null)
                            {
                                data123.TotalAmount = data123.TotalAmount + om.walletPointUsed;
                                context.WalletDb.Attach(data123);
                                context.Entry(data123).State = EntityState.Modified;
                                context.Commit();
                                CustomerWalletHistory CWH = new CustomerWalletHistory();
                                //op by user
                                People People = context.Peoples.Where(c => c.PeopleID == objcity.userid).SingleOrDefault();
                                CWH.PeopleId = People.PeopleID;
                                CWH.PeopleName = People.DisplayName;

                                //op by Cust
                                CWH.WarehouseId = om.WarehouseId;
                                CWH.CompanyId = 1;
                                CWH.CustomerId = data123.CustomerId;
                                CWH.NewAddedWAmount = om.walletPointUsed;
                                CWH.TotalWalletAmount = data123.TotalAmount;
                                CWH.Through = "From  Pre Order cancellation";
                                CWH.OrderId = om.OrderId;
                                CWH.CreatedDate = DateTime.Now;
                                CWH.UpdatedDate = DateTime.Now;
                                context.CustomerWalletHistoryDb.Add(CWH);
                                context.Commit();
                                BackgroundTaskManager.Run(() => ForNotificationCrderCancelledBeforeDispatchedWithstatus(om, context));
                            }

                            ///Return Gullak Amount
                            if (om.OrderType == 4 && !IsOnlineRefundEnabled)
                            {
                                var PaymentDetails = context.PaymentResponseRetailerAppDb.Where(z => z.OrderId == om.OrderId && z.PaymentFrom == "Gullak" && z.status == "Success").ToList();
                                if (PaymentDetails != null && PaymentDetails.Any())
                                {
                                    var totalAmount = PaymentDetails.Sum(x => x.amount);
                                    double cancelationAmt = 0;
                                    if (POCCancelationCharges > 0 && totalAmount > 0)
                                    {
                                        cancelationAmt = (totalAmount * POCCancelationCharges / 100);
                                        totalAmount = totalAmount - cancelationAmt;
                                    }
                                    var customerGullak = context.GullakDB.FirstOrDefault(x => x.CustomerId == om.CustomerId);
                                    if (customerGullak != null)
                                    {
                                        if (!context.GullakTransactionDB.Any(x => x.ObjectId == om.OrderId.ToString() && x.ObjectType == "Order" && x.Comment == "Due to Order Cancel"))
                                        {
                                            context.GullakTransactionDB.Add(new Model.Gullak.GullakTransaction
                                            {
                                                CreatedDate = DateTime.Now,
                                                CreatedBy = objcity.userid,
                                                Comment = "Order Cancel : " + om.OrderId.ToString() + (cancelationAmt > 0 ? (" with deducted cancelation charges:" + cancelationAmt) : ""),
                                                Amount = totalAmount,
                                                GullakId = customerGullak.Id,
                                                CustomerId = om.CustomerId,
                                                IsActive = true,
                                                IsDeleted = false,
                                                ObjectId = om.OrderId.ToString(),
                                                ObjectType = "Order"
                                            });

                                            customerGullak.TotalAmount += totalAmount;
                                            customerGullak.ModifiedBy = objcity.userid;
                                            customerGullak.ModifiedDate = DateTime.Now;
                                            context.Entry(customerGullak).State = EntityState.Modified;
                                            context.Commit();
                                        }
                                    }
                                }
                            }

                        }
                        result = "order canceled";
                        #region PayLaterRefund
                        if (result == "order canceled")
                        {
                            var paylaterdata = context.PayLaterCollectionDb.FirstOrDefault(x => x.OrderId == objcity.OrderId);
                            if (paylaterdata != null)
                            {
                                Model.CashManagement.PayLaterCollectionHistory history = new Model.CashManagement.PayLaterCollectionHistory();
                                history.Amount = paylaterdata.Amount;
                                history.PayLaterCollectionId = paylaterdata.Id;
                                history.CreatedDate = DateTime.Now;
                                history.RefNo = "";
                                history.IsActive = true;
                                history.IsDeleted = false;
                                history.CurrencyHubStockId = 0;
                                history.PaymentStatus = 1;
                                history.PaymentMode = "";
                                history.Comment = "Order Cancelled";
                                history.CreatedBy = objcity.userid;
                                context.PayLaterCollectionHistoryDb.Add(history);

                                var param = new SqlParameter("@OrderId", paylaterdata.OrderId);
                                RefundPaylaterDc refund = context.Database.SqlQuery<RefundPaylaterDc>("exec Sp_GetRefundDetail @OrderId", param).FirstOrDefault();
                                if (refund != null && refund.RefundAmount > 0)
                                {
                                    PaymentRefundHelper PRHelper = new PaymentRefundHelper();
                                    var PaymentResponseRetailerAppDb = new PaymentResponseRetailerApp
                                    {
                                        amount = (-1) * (refund.RefundAmount),
                                        CreatedDate = DateTime.Now,
                                        currencyCode = "INR",
                                        OrderId = paylaterdata.OrderId,
                                        PaymentFrom = refund.PaymentMode,
                                        GatewayTransId = refund.RefNo,
                                        GatewayOrderId = Convert.ToString(paylaterdata.OrderId),
                                        status = "Success",
                                        UpdatedDate = DateTime.Now,
                                        IsRefund = false,
                                        IsOnline = true,
                                        statusDesc = "Refund Initiated"
                                    };
                                    context.PaymentResponseRetailerAppDb.Add(PaymentResponseRetailerAppDb);
                                    var PaymentRefundRequestDc = new PaymentRefundRequestDc
                                    {
                                        Amount = (refund.RefundAmount),
                                        OrderId = PaymentResponseRetailerAppDb.OrderId,
                                        Source = PaymentResponseRetailerAppDb.PaymentFrom,
                                        ReqGatewayTransId = PaymentResponseRetailerAppDb.GatewayTransId,
                                        Status = (int)PaymentRefundEnum.Initiated,
                                        CreatedBy = objcity.userid,
                                        CreatedDate = DateTime.Now,
                                        IsActive = true,
                                        IsDeleted = false,
                                        ModifiedBy = objcity.userid,
                                        ModifiedDate = DateTime.Now,
                                        PaymentResponseRetailerAppId = PaymentResponseRetailerAppDb.id
                                    };
                                    bool IsInserted = PRHelper.InsertPaymentRefundRequest(context, PaymentRefundRequestDc);
                                }
                                context.Commit();
                            }
                        }
                        #endregion
                    }
                    catch (Exception ex) { logger.Error(ex.Message); }
                }
                return result;
            }
            else
            {
                result = "Something went wrong";
                return result;
            }
        }
        private async Task<bool> ForNotificationCrderCancelledBeforeDispatchedWithstatus(OrderMaster om, AuthContext context)
        {
            bool Result = false;
            Notification notification = new Notification();
            notification.title = "Your OrderId " + om.OrderId + " is Cancelled";
            notification.Message = om.walletPointUsed + " Point is added to your Wallet";
            notification.Pic = "https://cdn4.iconfinder.com/data/icons/ionicons/512/icon-image-128.png";
            var customers = context.Customers.Where(x => x.fcmId != null && x.CustomerId == om.walletPointUsed).SingleOrDefault();
            //AddNotification(notification);
            string Key = ConfigurationManager.AppSettings["FcmApiKey"];
            //string id11 = ConfigurationManager.AppSettings["FcmApiId"];
            //WebRequest tRequest = WebRequest.Create("https://fcm.googleapis.com/fcm/send") as HttpWebRequest;
            //tRequest.Method = "post";
            //var objNotification = new
            //{
            //    to = customers.fcmId,
            //    notification = new
            //    {
            //        title = notification.title,
            //        body = notification.Message,
            //        icon = notification.Pic
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

                title = notification.title,
                body = notification.Message,
                icon = notification.Pic
            };
            var firebaseService = new FirebaseNotificationServiceHelper(Key);
            var result = await firebaseService.SendNotificationForApprovalAsync(customers.fcmId, data);
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
        public class OrderDeliveryOTPHistory
        {
            [BsonId]
            public ObjectId Id { get; set; }
            public int OrderId { get; set; }
            public string OTP { get; set; }
            [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
            public DateTime CreatedDate { get; set; }
            public int CreatedBy { get; set; }
            public string CreatedByName { get; set; }
            public string OrderStatus { get; set; }

        }
        public class InvoicePdfRequest
        {
            public string InvoiceNo { get; set; }
        }

        public class InvoicePdfResponse
        {
            public string html { get; set; }
            public byte[] barcode { get; set; }
        }

        public class ooo
        {
            public List<OrderMaster> assignedorders { get; set; }
            public string mobile { get; set; }
        }
        public class FirstTimeOrderDTO
        {
            public int CustomerId { get; set; }
            public string CustomerName { get; set; }
            public string ShopName { get; set; }
            public string Skcode { get; set; }
            public int WarehouseId { get; set; }
            public string WarehouseName { get; set; }
            public string CityName { get; set; }
            public string Mobile { get; set; }
            public string Address { get; set; }
            public DateTime? DateOfPurchase { get; set; }
            public double amount { get; set; }

        }



        public class getSuminvoiceHSNCodeDataDC
        {
            public string HSNCode { get; set; }
            public string customerType { get; set; }
            public double AmtWithoutTaxDisc { get; set; }
            public double SGSTTaxAmmount { get; set; }
            public double CGSTTaxAmmount { get; set; }
            public double TaxAmmount { get; set; }
            public double CessTaxAmount { get; set; }
            public double TotalSum { get; set; }
        }

        public class RebookOderDC
        {
            public int OrderId { get; set; }
            public DateTime NewDate { get; set; }
            public int WarehouseId { get; set; }
            public int UserId { get; set; }

        }
        public class RefundPaylaterDc
        {
            public double RefundAmount { get; set; }
            public string RefNo { get; set; }
            public string PaymentMode { get; set; }
        }

    }
}