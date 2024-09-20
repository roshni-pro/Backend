
using AngularJSAuthentication.API.Helpers;
using AngularJSAuthentication.BusinessLayer.Managers.Transactions;
using AngularJSAuthentication.Controllers;
using AngularJSAuthentication.DataContracts.Transaction;
using AngularJSAuthentication.DataContracts.Transaction.Mongo;
using LinqKit;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Http;
using AngularJSAuthentication.Model;
using System.Data.Entity;
using AngularJSAuthentication.DataContracts.Masters;
using System.Data;
using System.Data.SqlClient;
using System.Data.Entity.Infrastructure;
using AngularJSAuthentication.Model.PlaceOrder;
using System.Collections.Concurrent;
using GenricEcommers.Models;
using AngularJSAuthentication.API.Helper;
using AgileObjects.AgileMapper;
using System.Net.Http;
using System.Net;
using System.Transactions;
using AngularJSAuthentication.DataContracts.Transaction.Stocks;
using AngularJSAuthentication.DataContracts.Shared;
using AngularJSAuthentication.BusinessLayer.Managers.Reports;
using AngularJSAuthentication.BusinessLayer.Managers.Masters;
using AngularJSAuthentication.Common.Helpers;
using System.Text;
using AngularJSAuthentication.DataContracts.Transaction.TripPlanner;
using AngularJSAuthentication.DataContracts.BatchCode;
using AngularJSAuthentication.BatchManager.Publishers;
using Nito.AsyncEx;
using AngularJSAuthentication.DataContracts.External.SalesAppDc;

namespace AngularJSAuthentication.API.Controllers
{
    [RoutePrefix("api/Picker")]
    public class PickerController : BaseApiController
    {
        private static TimeZoneInfo INDIAN_ZONE = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
        DateTime indianTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE);

        public static List<int> PickerProcessIds = new List<int>();



        #region Rejected Picker enhancement By Roshni & Aarti

        [HttpPost]
        [Route("rejectedPickerReportExport")]
        public async Task<dynamic> rejectedPickerReportExport(PickerExportDc pickerExportData)
        {
            var manager = new PickerManager();
            if (pickerExportData != null && pickerExportData.warehouselist != null && pickerExportData.warehouselist.Any())
            {
                var Item = await manager.rejectedPickerReportExport(pickerExportData);
                return Item;
            }
            else return false;

        }

        [HttpPost]
        [Route("GetRejectedPickerList")]
        public async Task<canceledpickersDc> GetRejectedPicker(rejPickerDc payload)
        {

            var manager = new PickerManager();
            if (payload != null && payload.warehouselist != null && payload.warehouselist.Any() && payload.clusterId != null && payload.clusterId.Any()
                && payload.fromDate != null && payload.toDate != null && payload.IsCount == true)
            {
                var Item = await manager.GetRejectedPicker(payload);
                return Item;
            }
            else if (payload != null && payload.warehouselist != null && payload.warehouselist.Any() && payload.clusterId != null && payload.clusterId.Any()
                && payload.skip != null && payload.take != null && payload.IsCount == false)
            {
                var Item = await manager.GetRejectedPicker(payload);
                return Item;
            }
            else return null;
        }

        [HttpPost]
        [Route("RejectedPickerReportList")]
        public async Task<List<rejectPkReportDc>> RejectedPickerReport(PickerExportReportDc pickerExportReportDc)
        {
            var manager = new PickerManager();
            if (pickerExportReportDc != null && pickerExportReportDc.warehouselist != null && pickerExportReportDc.warehouselist.Any()
                || pickerExportReportDc.allWarehouses != null || pickerExportReportDc.allWarehouses.Any())
            {
                var Item = await manager.RejectedPickerReport(pickerExportReportDc);
                return Item;
            }
            else return null;
        }

        [HttpGet]
        [Route("rejectedPickerdetailList")]
        public async Task<List<rejectedPickerdetailListDc>> rejectedPickerdetailList(int pickerNo)
        {
            var manager = new PickerManager();
            if (pickerNo > 0)
            {
                var Item = await manager.rejectedPickerdetailList(pickerNo);
                return Item;
            }
            else return null;
        }

        [HttpGet]
        [Route("rejectedPickerdetail")]
        public async Task<List<rejectedPickerdetailDc>> rejectedPickerdetail(int pickerId)
        {
            var manager = new PickerManager();
            if (pickerId > 0)
            {
                var Item = await manager.rejectedPickerdetail(pickerId);
                return Item;
            }
            else return null;
        }

        #endregion

        [HttpPost]
        [Route("GetPendingOrders")]
        public async Task<PaggingDataPickerDc> GetPendingOrders(GetPendingOrderFilterDc getPendingOrderFilterDc)
        {

            var manager = new ItemLedgerManager();
            var orderData = await manager.PickerSearchOrderMaster(getPendingOrderFilterDc);
            //var multimrpIds = orderData.ordermaster.SelectMany(z => z.orderDetails).Select(x => new { x.ItemMultiMRPId, x.WarehouseId, x.IsFreeItem }).Distinct().ToList();
            using (var authContext = new AuthContext())
            {
                if (orderData != null && orderData.ordermaster.Any(x => x.OrderType == 8))
                {
                    List<Model.Store.Store> stores = new List<Model.Store.Store>();
                    if (orderData.ordermaster.SelectMany(z => z.orderDetails) != null && orderData.ordermaster.SelectMany(z => z.orderDetails).Any())
                    {
                        var storeids = orderData.ordermaster.SelectMany(z => z.orderDetails).Select(x => x.StoreId).Distinct().ToList();
                        stores = authContext.StoreDB.Where(x => storeids.Contains(x.Id)).ToList();
                    }

                    foreach (var d in orderData.ordermaster.SelectMany(z => z.orderDetails))
                    {
                        if (!d.IsDispatchedFreeStock)
                        {
                            d.CurrentStock = d.CurrentStock;

                        }
                        else
                        {
                            d.CurrentStock = 0;
                        }
                        d.StoreName = stores.Any(x => x.Id == d.StoreId) ? stores.FirstOrDefault(x => x.Id == d.StoreId).Name : "Other";
                    }
                }
                else
                {

                    var itemdetail = orderData.ordermaster.SelectMany(z => z.orderDetails).Where(x => !x.IsDispatchedFreeStock).Select(x => new { x.ItemId, x.itemNumber, x.ItemMultiMRPId });

                    var itemnumbers = itemdetail.Select(x => x.itemNumber);
                    var ItemMultiMRPIds = itemdetail.Select(x => x.ItemMultiMRPId);
                    var currentinventorys = authContext.DbCurrentStock.Where(k => k.Deleted != true /*&& itemnumbers.Contains(k.ItemNumber)*/
                    && k.WarehouseId == getPendingOrderFilterDc.WareHouseID
                    && ItemMultiMRPIds.Contains(k.ItemMultiMRPId)).Select(k => new { k.ItemNumber, k.CurrentInventory, k.ItemMultiMRPId }).ToList(); //multimrp
                                                                                                                                                     //ItemMaster master = itemMasters.Where(c => c.ItemId == d.ItemId && c.CompanyId == compid).SingleOrDefault();
                    List<FreeStock> FreeStockList = new List<FreeStock>();

                    if (orderData.ordermaster.SelectMany(z => z.orderDetails).Any(x => x.IsDispatchedFreeStock))
                    {
                        var Freeitemdetail = orderData.ordermaster.SelectMany(z => z.orderDetails).Where(x => x.IsFreeItem && x.IsDispatchedFreeStock).Select(x => new { x.ItemId, x.itemNumber, x.ItemMultiMRPId });
                        var FreeItemMultiMRPIds = Freeitemdetail.Select(x => x.ItemMultiMRPId);
                        var FreeNumber = Freeitemdetail.Select(x => x.itemNumber);
                        FreeStockList = authContext.FreeStockDB.Where(k => k.Deleted != true /*&& FreeNumber.Contains(k.ItemNumber) */&& k.WarehouseId == getPendingOrderFilterDc.WareHouseID && FreeItemMultiMRPIds.Contains(k.ItemMultiMRPId)).ToList();
                    }

                    List<Model.Store.Store> stores = new List<Model.Store.Store>();
                    if (orderData.ordermaster.SelectMany(z => z.orderDetails) != null && orderData.ordermaster.SelectMany(z => z.orderDetails).Any())
                    {
                        var storeids = orderData.ordermaster.SelectMany(z => z.orderDetails).Select(x => x.StoreId).Distinct().ToList();
                        stores = authContext.StoreDB.Where(x => storeids.Contains(x.Id)).ToList();
                    }

                    foreach (var d in orderData.ordermaster.SelectMany(z => z.orderDetails))
                    {
                        if (!d.IsDispatchedFreeStock)
                        {
                            var currentinventory = currentinventorys.FirstOrDefault(x => x.ItemMultiMRPId == d.ItemMultiMRPId);
                            if (currentinventory != null)
                            {
                                d.CurrentStock = currentinventory.CurrentInventory;
                            }
                            else
                            {
                                d.CurrentStock = 0;
                            }
                        }
                        else if (d.IsDispatchedFreeStock)
                        {
                            var currentinventory = FreeStockList.FirstOrDefault(x => x.ItemMultiMRPId == d.ItemMultiMRPId);
                            if (currentinventory != null)
                            {
                                d.CurrentStock = currentinventory.CurrentInventory;
                            }
                            else
                            {
                                d.CurrentStock = 0;
                            }
                        }

                        d.StoreName = stores.Any(x => x.Id == d.StoreId) ? stores.FirstOrDefault(x => x.Id == d.StoreId).Name : "Other";
                    }

                }
            }


            List<PickerChooseMasterDC> pickerChooseMasterDCs = new List<PickerChooseMasterDC>();
            MongoDbHelper<PickerChooseMaster> mongoDbHelper = new MongoDbHelper<PickerChooseMaster>();
            var pickerMaster = mongoDbHelper.Select(x => x.Finalize == false && x.WarehouseId == getPendingOrderFilterDc.WareHouseID && x.CustomerType == getPendingOrderFilterDc.CustomerType).ToList();
            if (pickerMaster.Count > 0)
            {
                pickerChooseMasterDCs = Mapper.Map(pickerMaster).ToANew<List<PickerChooseMasterDC>>();
                var pickerorderMaster = pickerChooseMasterDCs.Where(x => x.mongoPickerOrderMaster != null).SelectMany(x => x.mongoPickerOrderMaster.Where(c => c.PickerOrderStatus == 1)).ToList();
                var orderids = orderData.ordermaster.Select(x => x.OrderId).ToList();
                var filterOrdermaster = pickerChooseMasterDCs.Where(x => x.mongoPickerOrderMaster != null).SelectMany(x => x.mongoPickerOrderMaster.Where(u => orderids.Contains(u.OrderId) && u.PickerOrderStatus == 1)).Distinct().ToList();
                List<int> mongoOrderIds = new List<int>();
                orderData.ordermaster.ForEach(x =>
                {
                    if (filterOrdermaster != null && filterOrdermaster.Any())
                    {
                        filterOrdermaster.ForEach(e =>
                        {
                            if (e.OrderId == x.OrderId)
                            {
                                if (x.OrderColor == "rgb(255, 153, 153)" || x.OrderColor == "yellow")
                                {
                                    x.IsSelected = false;
                                }
                                else
                                {
                                    x.IsSelected = true;
                                    mongoOrderIds.Add(x.OrderId);
                                }
                            }
                        });


                    }
                });
                orderData.orderIds = mongoOrderIds;
            }
            #region
            //using (var authContext = new AuthContext())
            //{
            //    if (authContext.Database.Connection.State != ConnectionState.Open)
            //        authContext.Database.Connection.Open();
            //    var orderIdDt = new DataTable();
            //    orderIdDt.Columns.Add("ItemMultiMRPId");
            //    orderIdDt.Columns.Add("WarehouseId");
            //    orderIdDt.Columns.Add("IsFreeItem");
            //    foreach (var item in multimrpIds)
            //    {
            //        var dr = orderIdDt.NewRow();
            //        dr["ItemMultiMRPId"] = item.ItemMultiMRPId;
            //        dr["WarehouseId"] = item.WarehouseId;
            //        dr["IsFreeItem"] = item.IsFreeItem;
            //        orderIdDt.Rows.Add(dr);
            //    }
            //    var param = new SqlParameter("Items", orderIdDt);
            //    param.SqlDbType = SqlDbType.Structured;
            //    param.TypeName = "dbo.itemtype";
            //    var cmd = authContext.Database.Connection.CreateCommand();
            //    cmd.CommandText = "[dbo].[GetCurrentStock]";
            //    cmd.CommandType = System.Data.CommandType.StoredProcedure;
            //    cmd.Parameters.Add(param);
            //    using (var reader = cmd.ExecuteReader())
            //    {
            //        currentStocks = ((IObjectContextAdapter)authContext)
            //                        .ObjectContext
            //                        .Translate<CurrentStockMinDc>(reader).ToList();
            //    }
            //}
            //foreach (var item in orderData.ordermaster)
            //{
            //    item.orderDetails.ForEach(v =>
            //    {
            //        if (v.IsDispatchedFreeStock == true)
            //        {
            //            v.CurrentStock = currentStocks.Where(x => x.ItemMultiMRPId == v.ItemMultiMRPId && x.IsFreeItem == true).Select(i => i.CurrentInventory).FirstOrDefault();
            //        }
            //        else
            //        {
            //            v.CurrentStock = currentStocks.Where(x => x.ItemMultiMRPId == v.ItemMultiMRPId && x.Deleted == false && x.IsFreeItem == false).Select(i => i.CurrentInventory).FirstOrDefault();
            //        }

            //    });
            //}
            #endregion

            return orderData;
        }
        [HttpPost]
        [Route("GetPendingOrdersById")]
        public async Task<PaggingDataPickerDc> GetPendingOrdersById(GetPendingOrderFilterDc getPendingOrderFilterDc)
        {
            getPendingOrderFilterDc.PageNo++;

            var fetchFromMongo = ConfigurationManager.AppSettings["OrdersFetchAndStoreInMongo"];
            if (fetchFromMongo == "true")
            {
                return GetOrdersByIdFromMongo(getPendingOrderFilterDc);
            }
            else
            {
                var manager = new PickerManager();
                var Item = await manager.GetPendingOrdersById(getPendingOrderFilterDc);

                #region Order master Orderid colour function
                if (Item.ordermaster != null && Item.ordermaster.Any())
                {
                    List<int> orderids = Item.ordermaster.Select(x => x.OrderId).ToList();


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



                        List<OrderOffer> OrderOffers = new List<OrderOffer>();
                        if (orderids != null && orderids.Any())
                        {
                            string sqlQuery = "select a.OrderId,b.OfferCode from BillDiscounts a inner join Offers b on a.OfferId = b.OfferId and a.OrderId in ( " + string.Join(",", orderids) + ")";
                            OrderOffers = authContext.Database.SqlQuery<OrderOffer>(sqlQuery).ToList();
                        }

                        foreach (var it in Item.ordermaster)
                        {
                            List<int> itemids = it.orderDetails.Select(y => y.ItemId).ToList();
                            if (itemids.Any())
                            {
                                string sqlCountQuery = "Select Count(*) from OrderMasters o inner join OrderDetails d on o.OrderId = d.OrderId and o.Deleted = 0  and o.Status = 'Pending' and d.ItemId in (" + string.Join(",", itemids) + ") and o.OrderId < " + it.OrderId;

                                int ItemCountCount = authContext.Database.SqlQuery<int>(sqlCountQuery).FirstOrDefault();
                                if (OrderOffers.Any(x => x.OrderId == it.OrderId))
                                {
                                    var offerCodes = OrderOffers.Where(x => x.OrderId == it.OrderId).Select(x => x.OfferCode).ToList();
                                    it.OfferCode = string.Join(",", offerCodes);
                                }
                                it.IsItemInOtherOrder = ItemCountCount > 0;
                                it.SalesPerson = string.Join(",", it.orderDetails.Where(z => !string.IsNullOrEmpty(z.ExecutiveName)).Select(z => z.ExecutiveName).Distinct());
                            }

                            if (it.IsItemInOtherOrder == false && it.Status == "Pending")
                            {
                                it.isCompleted = true;
                            }

                            if (itemorderstock.Any(x => x.Orderid == it.OrderId && x.qty > x.Currentinventory))
                            {
                                if (it.Status == "Pending")
                                {
                                    it.IsLessCurrentStock = true;
                                }
                                else
                                {
                                    it.IsLessCurrentStock = false;
                                }
                            }
                        }
                        return Item;
                    }
                }
                #endregion
                return Item;
            }
        }

        [HttpGet]
        [Route("GetItemOrders")]
        public async Task<List<OrderDetailsDc>> GetItemOrders(int OrderId)
        {
            {
                var manager = new PickerManager();
                var Item = await manager.GetItemOrders(OrderId);
                return Item;
            }
        }

        [HttpPost]
        [Route("GetItemOrdersDetail")]
        public dynamic GetItemOrdersDetail(GetItemOrdersList orderid)
        {
            pickeritemlist pickeritemlist = new pickeritemlist();
            PickerChooseMaster pickerMaster = new PickerChooseMaster();
            if (orderid != null && orderid.orderid != null && orderid.orderid.Any())
            {
                using (AuthContext context = new AuthContext())
                {
                    MongoDbHelper<PickerChooseMaster> mongoDbHelper = new MongoDbHelper<PickerChooseMaster>();
                    var objectid = new MongoDB.Bson.ObjectId(orderid.MongoObjectId);
                    pickerMaster = mongoDbHelper.Select(x => x.Id == objectid && x.Finalize == false).FirstOrDefault();

                    if (pickerMaster != null)
                    {
                        var order = pickerMaster.mongoPickerOrderMaster.Where(x => orderid.orderid.Contains(x.OrderId) && x.PickerOrderStatus == 1).SelectMany(x => x.orderDetails).ToList();
                        //var order = context.DbOrderDetails.Where(x => orderid.orderid.Contains(x.OrderId)).ToList();
                        //int whid = order.FirstOrDefault().WarehouseId;
                        //var itemmultimrpid = order.Select(x => x.ItemMultiMRPId).Distinct().ToList();
                        //var Stocks = context.DbCurrentStock.Where(x => itemmultimrpid.Contains(x.ItemMultiMRPId) && x.WarehouseId == whid && x.Deleted == false).ToList();
                        var picklist = order.Select(t =>
                        new pickeritemlistdc
                        {
                            OrderdetailsId = t.OrderDetailsId,
                            orderIds = t.OrderId,
                            qty = t.Qty,
                            //AvailableQty = Stocks == null ? Stocks.FirstOrDefault(c => c.ItemMultiMRPId == t.Key).CurrentInventory : 0,
                            ItemMultiMRPId = t.ItemMultiMrpId,
                            SellingUnitName = t.itemname,
                            IsFreeItem = t.IsFreeItem,
                        }).ToList();
                        pickeritemlist.orderid = orderid.orderid;
                        pickeritemlist.Pickeritemlist = picklist;
                    }
                }
            }
            return pickeritemlist;
        }


        /// <summary>
        ///  Post to genearte PickerList 
        /// </summary>
        /// <param name="GetItemOrders"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("PostOrderPickerMasters")]
        public string GenerateOrderPickerMasters(GetItemOrdersListV2 GetItemOrders)
        {
            var identity = User.Identity as ClaimsIdentity;
            int userid = 0;
            string RoleNames = string.Empty;
            List<string> Roles = new List<string>();
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "RoleNames"))
                RoleNames = identity.Claims.FirstOrDefault(x => x.Type == "RoleNames").Value;

            if (!string.IsNullOrEmpty(RoleNames))
                Roles = RoleNames.Split(',').ToList();

            var UpdateOrderForStatus = new List<OrderMaster>();
            var AddOrderMasterHistories = new List<Model.OrderMasterHistories>();
            var OrderDetailsOb = new List<Model.OrderDetails>();
            string result = "";
            List<CurrentToPlanedStockMoveDC> CurrentToPlanedStockMove = new List<CurrentToPlanedStockMoveDC>();
            List<OrderPickerMaster> AddOrderPickerMasterList = new List<OrderPickerMaster>();

            if (userid > 0 && GetItemOrders != null && Roles.Any() && (Roles.Contains("WH delivery planner")))
            {
                OrderOutPublisher Publisher = new OrderOutPublisher();
                List<BatchCodeSubjectDc> PublisherPickerStockList = new List<BatchCodeSubjectDc>();
                TransactionOptions option = new TransactionOptions();
                option.IsolationLevel = System.Transactions.IsolationLevel.RepeatableRead;
                option.Timeout = TimeSpan.FromSeconds(120);
                using (var dbContextTransaction = new TransactionScope(TransactionScopeOption.RequiresNew, option))
                {
                    using (AuthContext context = new AuthContext())
                    {
                        var OrdermasterLists = context.DbOrderMaster.Where(x => GetItemOrders.orderid.Contains(x.OrderId) && x.Status == "Pending").ToList();
                        bool IsClearancePicker = OrdermasterLists.All(x => x.OrderType == 8);

                        if (OrdermasterLists.Any(x => x.OrderType == 8) && OrdermasterLists.Any(x => x.OrderType != 8))
                        {
                            result = "you can't create picker Clearance Order with other order type ";
                            return result;
                        }
                        if (OrdermasterLists != null && OrdermasterLists.Any())
                        {
                            var warehouseid = OrdermasterLists.FirstOrDefault().WarehouseId;
                            var warehouse = context.Warehouses.FirstOrDefault(x => x.WarehouseId == warehouseid);

                            if (warehouse.IsStopCurrentStockTrans)
                                return "Inventory Transactions are currently disabled for this warehouse... Please try after some time";

                            List<OrderPickerDetails> LineItems = new List<OrderPickerDetails>();
                            OrderPickerMaster orderPickerMaster = new OrderPickerMaster();

                            orderPickerMaster.PickerPersonId = GetItemOrders.peopleId;
                            orderPickerMaster.ClusterId = GetItemOrders.ClusterId;
                            orderPickerMaster.CreatedDate = DateTime.Now;
                            orderPickerMaster.IsActive = true;
                            orderPickerMaster.IsDeleted = false;
                            orderPickerMaster.CreatedBy = userid;
                            orderPickerMaster.Status = 0;
                            orderPickerMaster.RePickingCount = 0;
                            orderPickerMaster.WarehouseId = warehouse.WarehouseId;
                            orderPickerMaster.DBoyId = GetItemOrders.dboyid;
                            orderPickerMaster.orderPickerDetails = new List<OrderPickerDetails>();


                            var OdList = OrdermasterLists.Select(x => x.OrderId).Distinct().ToList();
                            var order = context.DbOrderDetails.Where(x => OdList.Contains(x.OrderId) && x.qty > 0).Distinct().ToList();
                            // opder picker details for qt
                            List<int> CutLineOrderDetailsIds = new List<int>();
                            foreach (var OrderId in OdList)
                            {
                                if (GetItemOrders.LineItemCutItems.Count > 0)
                                {
                                    foreach (var item in GetItemOrders.LineItemCutItems.Where(x => x.OrderId == OrderId).ToList())
                                    {
                                        var LineCutItemLists = order.Where(c => c.OrderDetailsId == item.OrderDetailsId).Select(x => new OrderPickerDetails
                                        {
                                            ItemMultiMrpId = x.ItemMultiMRPId,
                                            ItemName = x.itemname,
                                            OrderDetailsId = x.OrderDetailsId,
                                            OrderId = x.OrderId,
                                            Qty = item.Qty > 0 ? item.Qty : 0,
                                            IsFreeItem = x.IsFreeItem,
                                            IsDispatchedFreeStock = x.IsDispatchedFreeStock,
                                            QtyChangeReason = item.QtyChangeReason
                                            //price = x.price
                                        }).Distinct().ToList();
                                        var CutLineCToPMovedStockList = new List<CurrentToPlanedStockMoveDC>();
                                        CutLineCToPMovedStockList = Mapper.Map(LineCutItemLists).ToANew<List<CurrentToPlanedStockMoveDC>>();
                                        CurrentToPlanedStockMove.AddRange(CutLineCToPMovedStockList);//
                                                                                                     // opder picker details for qty
                                        LineItems.AddRange(LineCutItemLists);
                                        CutLineOrderDetailsIds.Add(item.OrderDetailsId);

                                    }
                                }
                                var ItemLists = order.Where(c => c.OrderId == OrderId && !CutLineOrderDetailsIds.Contains(c.OrderDetailsId)).Select(x => new OrderPickerDetails
                                {
                                    ItemMultiMrpId = x.ItemMultiMRPId,
                                    ItemName = x.itemname,
                                    OrderDetailsId = x.OrderDetailsId,
                                    OrderId = x.OrderId,
                                    Qty = x.qty,
                                    IsFreeItem = x.IsFreeItem,
                                    IsDispatchedFreeStock = x.IsDispatchedFreeStock,

                                    //price = x.price
                                }).Distinct().ToList();

                                if (GetItemOrders.LineItemCutItems != null && GetItemOrders.LineItemCutItems.Any())
                                {
                                    foreach (var Cutit in ItemLists.Where(x => x.IsFreeItem == true))
                                    {
                                        List<ReadyToDispatchHelper.FreeBillItems> freeBillItems = new List<ReadyToDispatchHelper.FreeBillItems>();
                                        freeBillItems = ItemLists.Where(x => x.OrderId == Cutit.OrderId).Select(x => new ReadyToDispatchHelper.FreeBillItems
                                        {
                                            ItemId = order.FirstOrDefault(y => y.OrderDetailsId == x.OrderDetailsId) != null ? order.FirstOrDefault(y => y.OrderDetailsId == x.OrderDetailsId).ItemId : 0,
                                            ItemNumber = order.FirstOrDefault(y => y.OrderDetailsId == x.OrderDetailsId) != null ? order.FirstOrDefault(y => y.OrderDetailsId == x.OrderDetailsId).itemNumber : "",
                                            OrderdetailId = x.OrderDetailsId,
                                            Qty = x.Qty,
                                            UnitPrice = order.FirstOrDefault(y => y.OrderDetailsId == x.OrderDetailsId) != null ? order.FirstOrDefault(y => y.OrderDetailsId == x.OrderDetailsId).UnitPrice : 0,
                                            IsFreeitem = x.IsFreeItem,
                                            OfferId = order.FirstOrDefault(y => y.OrderDetailsId == x.OrderDetailsId) != null ? order.FirstOrDefault(y => y.OrderDetailsId == x.OrderDetailsId).OfferId : null
                                        }).ToList();
                                        #region calculate free item qty
                                        var Parent = order.Where(x => x.OrderDetailsId == Cutit.OrderDetailsId && x.FreeWithParentItemId >= 0).FirstOrDefault();
                                        if (Parent != null && Parent.FreeWithParentItemId > 0)
                                        {
                                            int Ids = order.FirstOrDefault(x => x.ItemId == Parent.FreeWithParentItemId && x.OrderId == Parent.OrderId && x.OrderDetailsId != Cutit.OrderDetailsId && !x.IsFreeItem).OrderDetailsId;
                                            if (Ids > 0)
                                            {
                                                var TotalParentQty = LineItems.Where(x => x.OrderDetailsId == Ids).FirstOrDefault();
                                                if (TotalParentQty != null)
                                                {
                                                    ReadyToDispatchHelper Free = new ReadyToDispatchHelper();
                                                    int freeitemqty = Free.getfreebiesitem(Cutit.OrderId, Parent.ItemId, context, TotalParentQty.Qty, Cutit.OrderDetailsId, Parent.itemNumber, freeBillItems);//, 
                                                    Cutit.Qty = freeitemqty;
                                                }
                                                else if (TotalParentQty == null)
                                                {
                                                    var TotalParentQtys = ItemLists.Where(x => x.OrderDetailsId == Ids).FirstOrDefault();
                                                    if (TotalParentQtys != null)
                                                    {
                                                        ReadyToDispatchHelper Free = new ReadyToDispatchHelper();
                                                        int freeitemqty = Free.getfreebiesitem(Cutit.OrderId, Parent.ItemId, context, TotalParentQtys.Qty, Cutit.OrderDetailsId, Parent.itemNumber, freeBillItems);//, 
                                                        Cutit.Qty = freeitemqty;
                                                    }
                                                    else
                                                    {
                                                        Cutit.Qty = 0;
                                                    }
                                                }
                                            }
                                        }
                                        else if (Parent != null && Parent.FreeWithParentItemId == 0)
                                        {
                                            ReadyToDispatchHelper Free = new ReadyToDispatchHelper();
                                            int freeitemqty = Free.getfreebiesitem(Cutit.OrderId, Parent.ItemId, context, 0, Cutit.OrderDetailsId, Parent.itemNumber, freeBillItems);//, 
                                            Cutit.Qty = freeitemqty;
                                        }
                                        #endregion
                                    }
                                }
                                var CToPMovedStockList = new List<CurrentToPlanedStockMoveDC>();
                                CToPMovedStockList = Mapper.Map(ItemLists).ToANew<List<CurrentToPlanedStockMoveDC>>();
                                CurrentToPlanedStockMove.AddRange(CToPMovedStockList);//
                                                                                      // opder picker details for qty
                                LineItems.AddRange(ItemLists);
                            }
                            orderPickerMaster.orderPickerDetails = LineItems;
                            AddOrderPickerMasterList.Add(orderPickerMaster);
                            foreach (var item in OrdermasterLists.Where(x => OdList.Contains(x.OrderId)))
                            {
                                item.UpdatedDate = indianTime;
                                item.Status = "ReadyToPick";
                                UpdateOrderForStatus.Add(item);
                            }

                            foreach (var ods in order)
                            {
                                ods.UpdatedDate = DateTime.Now;
                                ods.Status = "ReadyToPick";
                                OrderDetailsOb.Add(ods);
                            }
                            foreach (var oi in OdList)
                            {
                                Model.OrderMasterHistories h1 = new Model.OrderMasterHistories();
                                h1.orderid = oi;
                                h1.Status = "ReadyToPick";
                                h1.userid = userid;
                                h1.CreatedDate = DateTime.Now;
                                AddOrderMasterHistories.Add(h1);
                            }
                            if (CurrentToPlanedStockMove.Sum(x => x.Qty) <= 0)
                            {
                                return result = "You can't create zero qty line item";
                            }
                            foreach (var item in UpdateOrderForStatus)
                            {
                                context.Entry(item).State = EntityState.Modified;
                            }
                            foreach (var odsv1 in OrderDetailsOb)
                            {
                                context.Entry(odsv1).State = EntityState.Modified;
                            }
                            if (AddOrderMasterHistories != null && AddOrderMasterHistories.Any())
                            {
                                context.OrderMasterHistoriesDB.AddRange(AddOrderMasterHistories);
                            }
                            if (AddOrderPickerMasterList != null && AddOrderPickerMasterList.Any())
                            {
                                context.OrderPickerMasterDb.AddRange(AddOrderPickerMasterList);
                            }
                            if (context.Commit() > 0)
                            {
                                List<long> ids = AddOrderPickerMasterList.Select(x => x.Id).ToList();
                                result = "Following Picker Number : " + string.Join(", ", ids.ToList()) + " Generated";
                                #region stock Hit
                                //for currentstock
                                MultiStockHelper<Stock_OnPickedDc> MultiStockHelpers = new MultiStockHelper<Stock_OnPickedDc>();
                                List<Stock_OnPickedDc> rtdStockList = new List<Stock_OnPickedDc>();
                                foreach (var StockHit in CurrentToPlanedStockMove.Where(x => x.Qty > 0))
                                {
                                    bool isfree = false;
                                    //string RefStockCode = (OrdermasterLists.FirstOrDefault(x => x.OrderId == StockHit.OrderId).OrderType == 8) ? "CL" : "C";

                                    string RefStockCode = "";

                                    if (OrdermasterLists.FirstOrDefault(x => x.OrderId == StockHit.OrderId).OrderType == 8)
                                    {
                                        RefStockCode = "CL";
                                    }
                                    else if (OrdermasterLists.FirstOrDefault(x => x.OrderId == StockHit.OrderId).OrderType == 10)
                                    {
                                        RefStockCode = "NR";
                                    }
                                    else
                                    {
                                        RefStockCode = "C";
                                    }

                                    if (StockHit.IsFreeItem && StockHit.IsDispatchedFreeStock)
                                    {
                                        RefStockCode = "F";
                                        isfree = true;
                                    }
                                    rtdStockList.Add(new Stock_OnPickedDc
                                    {
                                        ItemMultiMRPId = StockHit.ItemMultiMrpId,
                                        OrderDispatchedDetailsId = StockHit.OrderDetailsId,
                                        OrderId = StockHit.OrderId,
                                        Qty = StockHit.Qty,
                                        UserId = userid,
                                        WarehouseId = warehouse.WarehouseId,
                                        IsFreeStock = isfree,
                                        RefStockCode = RefStockCode
                                    });
                                }
                                #region Check Current Stock Qty
                                var SumItemList = rtdStockList.GroupBy(x => new { x.ItemMultiMRPId, x.WarehouseId, x.IsFreeStock, x.RefStockCode }).Select(x => new Picker_OnPickedDc
                                {
                                    IsFreeStock = x.Key.IsFreeStock,
                                    RefStockCode = x.Key.RefStockCode,
                                    ItemMultiMRPId = x.Key.ItemMultiMRPId,
                                    WarehouseId = x.Key.WarehouseId,
                                    Qty = x.Sum(y => y.Qty),
                                    OrderIds = string.Join(",", x.Select(z => z.OrderId).Distinct().ToList())
                                }).ToList();

                                StringBuilder str = new StringBuilder();
                                int cnt = 0;
                                if (!IsClearancePicker)
                                {
                                    var Cmultimrpid = SumItemList.Where(x => x.RefStockCode == "C").Select(x => x.ItemMultiMRPId).Distinct().ToList();
                                    var currentstocklist = context.DbCurrentStock.Where(x => Cmultimrpid.Contains(x.ItemMultiMRPId) && x.WarehouseId == warehouse.WarehouseId && x.Deleted == false).ToList();

                                    var Fmultimrpid = SumItemList.Where(x => x.RefStockCode == "F").Select(x => x.ItemMultiMRPId).Distinct().ToList();
                                    var Freeststock = context.FreeStockDB.Where(x => Fmultimrpid.Contains(x.ItemMultiMRPId) && x.WarehouseId == warehouse.WarehouseId && x.Deleted == false).ToList();

                                    foreach (var item in SumItemList)
                                    {
                                        if (item.RefStockCode == "C")
                                        {

                                            var current = currentstocklist.Where(x => x.ItemMultiMRPId == item.ItemMultiMRPId && x.WarehouseId == item.WarehouseId).FirstOrDefault();

                                            if (current != null)
                                            {
                                                if (item.Qty > current.CurrentInventory)
                                                {
                                                    cnt++;
                                                    str.AppendLine("Picker can't generated due to insufficient stock of item : " + current.itemname + " ,Qty : " + item.Qty + " ,OrderIds: " + item.OrderIds);
                                                }
                                            }
                                        }
                                        else if (item.RefStockCode == "F")
                                        {
                                            var freestock = Freeststock.Where(x => x.ItemMultiMRPId == item.ItemMultiMRPId && x.WarehouseId == item.WarehouseId).FirstOrDefault();
                                            if (freestock != null)
                                            {
                                                if (item.Qty > freestock.CurrentInventory)
                                                {
                                                    cnt++;
                                                    str.AppendLine("Picker can't generated due to insufficient stock of (Free Item): " + freestock.itemname + " ,Qty : " + item.Qty + " ,OrderIds: " + item.OrderIds);
                                                }
                                            }

                                        }
                                    }
                                }
                                if (cnt > 0)
                                {
                                    dbContextTransaction.Dispose();
                                    result = str.ToString();
                                    return result;
                                }
                                #endregion
                                if (rtdStockList.Any() && cnt == 0)
                                {
                                    bool res = MultiStockHelpers.MakeEntry(rtdStockList, "Stock_OnPicked", context, dbContextTransaction);
                                    if (!res)
                                    {
                                        dbContextTransaction.Dispose();
                                        result = "Picker can't generated due to insufficient stock  " + rtdStockList + " ";
                                        return result;
                                    }

                                    #region BatchCode
                                    foreach (var s in rtdStockList.Where(x => x.Qty > 0))
                                    {
                                        PublisherPickerStockList.Add(new BatchCodeSubjectDc
                                        {
                                            ObjectDetailId = s.OrderDispatchedDetailsId,  // its OrderDetailsId
                                            ObjectId = s.OrderId,
                                            StockType = s.RefStockCode,
                                            Quantity = s.Qty,
                                            WarehouseId = s.WarehouseId,
                                            ItemMultiMrpId = s.ItemMultiMRPId
                                        });
                                    }
                                    #endregion
                                }
                                else
                                {
                                    dbContextTransaction.Dispose();
                                    result = "Something Went Worng!!";
                                    return result;
                                }
                                #endregion

                                #region Mongno Update  
                                if (GetItemOrders.MongoObjectid != null)
                                {
                                    PickerChooseMaster pickerMaster = new PickerChooseMaster();
                                    PickerChooseMasterDC pickerChooseMasterDCs = new PickerChooseMasterDC();
                                    MongoDbHelper<PickerChooseMaster> mongoDbHelper = new MongoDbHelper<PickerChooseMaster>();
                                    var objectid = new MongoDB.Bson.ObjectId(GetItemOrders.MongoObjectid);
                                    pickerMaster = mongoDbHelper.Select(x => x.Id == objectid).FirstOrDefault();

                                    if (pickerMaster != null)
                                    {
                                        pickerChooseMasterDCs = Mapper.Map(pickerMaster).ToANew<PickerChooseMasterDC>();
                                        var warehouseId = pickerMaster.mongoPickerOrderMaster.Select(y => y.WarehouseId).Distinct().ToList();
                                        var clusterId = pickerMaster.mongoPickerOrderMaster.Select(y => y.ClusterId).Distinct().ToList();
                                        using (var db = new AuthContext())
                                        {
                                            var pickerorderMaster = pickerChooseMasterDCs.mongoPickerOrderMaster.Where(x => x.PickerOrderStatus == 1).ToList();
                                            var pcikerOrderids = pickerorderMaster.Select(c => c.OrderId).ToList();
                                            var orderMaster = db.DbOrderMaster.Where(x => pcikerOrderids.Contains(x.OrderId) && x.Status == "Pending").Select(x => new
                                            {
                                                x.OrderId,
                                                x.Status
                                            }
                                            ).ToList();
                                            var orderids = orderMaster.Select(x => x.OrderId).ToList();
                                            var filterOrdermaster = pickerChooseMasterDCs.mongoPickerOrderMaster.Where(x => !orderids.Contains(x.OrderId)).Distinct().ToList();
                                            pickerMaster.mongoPickerOrderMaster.ForEach(x =>
                                            {
                                                if (filterOrdermaster != null && filterOrdermaster.Any())
                                                {
                                                    filterOrdermaster.ForEach(y =>
                                                    {
                                                        if (x.OrderId == y.OrderId)
                                                        {
                                                            x.PickerOrderStatus = 2;
                                                        }
                                                    });
                                                }
                                            });
                                            var count = pickerMaster.mongoPickerOrderMaster.Where(q => q.PickerOrderStatus == 1).Count();
                                            if (count == 0)
                                            {
                                                pickerMaster.Finalize = true;
                                            }
                                            mongoDbHelper.Replace(pickerMaster.Id, pickerMaster);
                                        }
                                    }
                                }
                                #endregion
                                dbContextTransaction.Complete();
                            }
                        }
                        else { result = "There is no pending order to generate Picker"; }
                    }
                }
                if (PublisherPickerStockList != null && PublisherPickerStockList.Any())
                {
                    Publisher.PlannedPublish(PublisherPickerStockList);

                }
            }
            else { result = "Only Warehouse Planner can Generate Picker"; }
            return result;
        }

        [HttpGet]
        [Route("GetpickerofWarehouse")]
        public async Task<List<People>> GetpickerofWarehouse(int WarehouseId)
        {
            var manager = new PickerManager();
            var people = await manager.GetpickerofWarehouse(WarehouseId);
            return people;
        }

        [HttpGet]
        [Route("GetDboyWarehouse")]
        public async Task<List<People>> GetDboyWarehouse(int WarehouseId)
        {
            var manager = new PickerManager();
            var people = await manager.GetDboyWarehouse(WarehouseId);
            return people;
        }

        [HttpGet]
        [Route("GetPickerJobList")]
        public async Task<List<sendPickertoApp>> GetPickerJobList(long id)
        {
            List<sendPickertoApp> SendPickertoApps = null;
            if (id > 0)
            {
                using (AuthContext context = new AuthContext())
                {
                    List<OrderPickerDetailDTO> pickerlist = new List<OrderPickerDetailDTO>();
                    var orderPickerMasterIdParam = new SqlParameter("@OrderPickerMasterId", id);
                    pickerlist = context.Database.SqlQuery<OrderPickerDetailDTO>("Picker.GetPickerJobListDetails @OrderPickerMasterId", orderPickerMasterIdParam).ToList();
                    if (pickerlist != null)
                    {
                        var picklist = pickerlist.Where(x => x.Qty > 0).GroupBy(x => new { x.ItemMultiMrpId, x.IsClearance }).Select(t =>
                             new sendPickertoApp
                             {
                                 pickerId = pickerlist.FirstOrDefault().OrderPickerMasterId,
                                 Totalqty = t.Sum(x => x.Qty),
                                 ItemMultiMRPId = t.Key.ItemMultiMrpId,
                                 SellingUnitName = t.Min(x => x.ItemName),
                                 itemname = t.Min(x => x.ItemName),
                                 IsClearance = t.Key.IsClearance,
                                 OrderDetailsSPA = pickerlist.Where(x => x.ItemMultiMrpId == t.Key.ItemMultiMrpId && x.IsClearance == t.Key.IsClearance && x.Qty > 0).Select(r =>
                                      new OrderDetailsSPA
                                      {
                                          pickerDetailsId = r.id,
                                          pickerId = pickerlist.FirstOrDefault().OrderPickerMasterId,
                                          ItemMultiMrpId = r.ItemMultiMrpId,
                                          orderid = r.OrderId,
                                          qty = r.Qty,
                                          Comment = r.Comment,
                                          IsFreeItem = r.IsFreeItem,
                                          Status = r.Status,
                                      }).ToList()
                             }).ToList();
                        List<int> mmpi = picklist.Select(x => x.ItemMultiMRPId).Distinct().ToList();
                        var a = string.Join<int>(",", mmpi);
                        var manager = new PickerManager();
                        List<pikeritemDC> Item = manager.GetItemMRPandBarcode(a);
                        foreach (var aa in picklist)
                        {
                            aa.MRP = Item.Where(x => x.ItemMultiMRPId == aa.ItemMultiMRPId).Select(e => e.MRP).FirstOrDefault();
                            aa.Number = Item.Where(x => x.ItemMultiMRPId == aa.ItemMultiMRPId).Select(e => e.Number).FirstOrDefault();
                        }
                        var itemNumbers = picklist.Select(x => x.Number).Distinct().ToList();
                        var itembarcodelist = context.ItemBarcodes.Where(c => itemNumbers.Contains(c.ItemNumber) && c.IsActive == true && c.IsDeleted == false).Select(x => new ItemBarcodeDc { ItemNumber = x.ItemNumber, Barcode = x.Barcode }).ToList();
                        foreach (var item in picklist)
                        {
                            item.Barcode = (itembarcodelist != null && itembarcodelist.Any(x => x.ItemNumber == item.Number)) ? itembarcodelist.Where(x => x.ItemNumber == item.Number).Select(x => x.Barcode).ToList() : null;
                        }
                        SendPickertoApps = picklist.OrderByDescending(x => x.IsClearance).ThenBy(x => x.itemname).ToList();
                    }
                }

            }
            return SendPickertoApps;

        }

        /// <summary>
        ///  Get List On App
        /// </summary>
        /// <param name="PeopleId"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("GetOrderPickerMasters")]
        public async Task<HttpResponseMessage> GetOrderPickerMasters(int PeopleId)
        {
            bool Status = false;
            PickerJobListRes res;
            string Msg = "";
            List<PickerJobListDc> pickerJobList = new List<PickerJobListDc>();
            if (PeopleId > 0)
            {
                using (AuthContext context = new AuthContext())
                {
                    List<OrderPickerMaster> OrderPickerMasterList = context.OrderPickerMasterDb.Where(x => x.PickerPersonId == PeopleId && x.IsPickerGrabbed && x.IsDispatched == false && x.IsDeleted == false && x.IsCanceled == false).OrderBy(x => x.Id).Include("OrderPickerDetails").ToList();//till not dispatched
                    if (OrderPickerMasterList != null && OrderPickerMasterList.Any())
                    {
                        var createdBy = OrderPickerMasterList.Select(x => x.CreatedBy).Distinct().ToList();
                        var submittedBy = OrderPickerMasterList.Select(x => x.PickerPersonId).Distinct().ToList();
                        var dboyid = OrderPickerMasterList.Select(x => x.DBoyId).Distinct().ToList();
                        var peopleIds = createdBy.Concat(submittedBy);
                        var peoplename = context.Peoples.Where(x => peopleIds.Contains(x.PeopleID)
                        || dboyid.Contains(x.PeopleID)
                        ).Distinct().ToList();
                        var Pickerids = OrderPickerMasterList.Select(x => x.Id).Distinct().ToList();

                        List<PickerTimer> PickerTimerList = context.PickerTimerDb.Where(x => Pickerids.Contains(x.OrderPickerMasterId) && x.Type == 0 && x.EndTime == null).ToList();

                        foreach (var a in OrderPickerMasterList)
                        {
                            if (a.orderPickerDetails.Any(x => x.OrderPickerMasterId == a.Id))
                            {
                                double amount = 0;
                                var orderids = a.orderPickerDetails.Where(y => y.Status != 3).Select(y => y.OrderId).Distinct().ToList();
                                var clusters = context.DbOrderMaster.Where(x => orderids.Contains(x.OrderId)).Select(x => x.ClusterName).Distinct().ToList();

                                var dboyname = peoplename.Where(x => x.PeopleID == a.DBoyId).FirstOrDefault()?.DisplayName;
                                var orderDetails = a.orderPickerDetails.Where(x => x.Status != 3).Select(x => new { x.OrderDetailsId, x.Qty }).ToList();
                                var orderdetailIds = orderDetails.Select(x => x.OrderDetailsId).ToList();
                                var dbOrderDetails = context.DbOrderDetails.Where(x => orderdetailIds.Contains(x.OrderDetailsId)).Select(x => new { x.OrderDetailsId, x.UnitPrice }).ToList();
                                dbOrderDetails.ForEach(x =>
                                {
                                    amount += orderDetails.Where(y => y.OrderDetailsId == x.OrderDetailsId).Sum(y => y.Qty * x.UnitPrice);
                                });
                                PickerJobListDc PickerJobListob = new PickerJobListDc();
                                PickerJobListob.Id = a.Id;
                                PickerJobListob.CreatedDate = a.CreatedDate;
                                PickerJobListob.SubmittedDate = a.SubmittedDate;
                                PickerJobListob.count = a.orderPickerDetails.GroupBy(x => x.ItemMultiMrpId).Count();
                                PickerJobListob.IsApproved = a.IsApproved;
                                if (a.ApproverId > 0)
                                {
                                    PickerJobListob.IsCheckerChecked = true;

                                }
                                else
                                {
                                    PickerJobListob.IsCheckerChecked = false;
                                }

                                PickerJobListob.Commenet = a.Comment;
                                PickerJobListob.IsComplted = a.IsComplted;
                                PickerJobListob.CreatedBy = peoplename.FirstOrDefault(x => x.PeopleID == a.CreatedBy).DisplayName;
                                PickerJobListob.SubmittedBy = peoplename.FirstOrDefault(x => x.PeopleID == a.PickerPersonId).DisplayName;
                                PickerJobListob.dboyName = dboyname == null ? "" : dboyname;
                                PickerJobListob.amount = Math.Round(amount, 0);
                                PickerJobListob.ClusterName = string.Join(",", clusters);
                                //PickerJobListob.ClusterName = clusters.FirstOrDefault(x => x.ClusterId == a.ClusterId).ClusterName;
                                if (PickerTimerList != null && PickerTimerList.Any() && PickerTimerList.Any(x => x.OrderPickerMasterId == a.Id && x.EndTime == null))
                                {
                                    PickerJobListob.StartTime = PickerTimerList.FirstOrDefault(x => x.OrderPickerMasterId == a.Id && x.EndTime == null).StartTime;
                                }
                                else
                                {
                                    PickerJobListob.StartTime = null;
                                }
                                PickerJobListob.EndTime = null;
                                if (a.Status == 0)
                                {
                                    PickerJobListob.Status = "New";
                                }
                                else if ((a.Status != 0 && a.IsComplted == false && a.Status != 5))
                                {
                                    PickerJobListob.Status = "Inprogress";
                                }
                                else if (a.Status == 5)
                                {
                                    PickerJobListob.Status = "RePicking";
                                }
                                else if (a.IsComplted)
                                {
                                    PickerJobListob.Status = "Submitted";
                                }
                                pickerJobList.Add(PickerJobListob);
                            }
                        }
                        if (pickerJobList != null && pickerJobList.Any())
                        {
                            Status = true;
                            Msg = "Record found";
                        }
                        else
                        {
                            Status = true;
                            Msg = "No Record found";
                        }
                    }
                }
            }
            res = new PickerJobListRes()
            {
                PickerJobLists = pickerJobList,
                Status = Status,
                Message = Msg
            };
            return Request.CreateResponse(HttpStatusCode.OK, res);
        }



        //[HttpGet]
        //[Route("ShowPickerListA7")]
        //public async Task<paggingDTO> ShowPickerListA7(int ClusterId, int WarehouseId, int IsComplete, int skip, int take)
        //{
        //    using (AuthContext context = new AuthContext())
        //    {
        //        List<OrderPickerMaster> pickerList = new List<OrderPickerMaster>();
        //        paggingDTO addlist = new paggingDTO();
        //        int pickerlistcount = 0;
        //        if (ClusterId == 0)
        //        {
        //            var predicate = PredicateBuilder.New<OrderPickerMaster>(x => x.WarehouseId == WarehouseId && x.IsDeleted == false);
        //            if (IsComplete != null)
        //            {
        //                if(IsComplete == 0)
        //                {
        //                    predicate = predicate.And(x => x.Status == IsComplete && x.IsCanceled == false);
        //                }else if(IsComplete == 4)
        //                {
        //                    predicate = predicate.And(x => x.Status == IsComplete && x.IsCanceled == true);
        //                }
        //                else if (IsComplete != 7)
        //                {
        //                    predicate = predicate.And(x => x.Status == IsComplete);
        //                }

        //            }
        //            List<OrderPickerMaster> pickerlist = context.OrderPickerMasterDb.Where(predicate).OrderByDescending(x => x.Id).Skip((skip - 1) * take).Take(take).Include("OrderPickerDetails").ToList();
        //            pickerlistcount = context.OrderPickerMasterDb.Where(predicate).OrderByDescending(x => x.Id).Count();

        //            var people = pickerlist.Select(x => x.PickerPersonId).Distinct().ToList();
        //            var peoplecreatedby = pickerlist.Select(x => x.CreatedBy).Distinct().ToList();
        //            var agentId = pickerlist.Select(x => x.AgentId).Distinct().ToList();
        //            var dboyid = pickerlist.Select(x => x.DBoyId).Distinct().ToList();
        //            var inventorySupervisorId = pickerlist.Select(x => x.InventorySupervisorId).Distinct().ToList();
        //            var peoplename = context.Peoples.Where(x => people.Contains(x.PeopleID)
        //            || peoplecreatedby.Contains(x.PeopleID)
        //            || agentId.Contains(x.PeopleID)
        //            || dboyid.Contains(x.PeopleID)
        //            || inventorySupervisorId.Contains(x.PeopleID)).ToList();
        //            //var createdname = context.Peoples.Where(x => peoplecreatedby.Contains(x.PeopleID)).ToList();
        //            var cluster = pickerlist.Select(x => x.ClusterId).Distinct().ToList();
        //            var Clustername = context.Clusters.Where(x => cluster.Contains(x.ClusterId)).ToList();

        //            // var agentName = context.Peoples.Where(x => agentId.Contains(x.PeopleID)).ToList();
        //            //var DboyName = context.Peoples.Where(x => dboyid.Contains(x.PeopleID)).ToList();
        //            var newsd = new List<OrderPickerMasterDc>();
        //            newsd = Mapper.Map(pickerlist).ToANew<List<OrderPickerMasterDc>>();
        //            foreach (var item in newsd)
        //            {
        //                var MultiMrpIds = item.orderPickerDetails.Select(x => x.ItemMultiMrpId).Distinct().ToList();
        //                var MultiMrpList = context.ItemMultiMRPDB.Where(x => MultiMrpIds.Contains(x.ItemMultiMRPId)).Distinct().ToList();

        //                item.PickerPersonName = peoplename.Where(x => x.PeopleID == item.PickerPersonId).Select(x => x.DisplayName).FirstOrDefault();
        //                item.CreatedByName = peoplename.Where(x => x.PeopleID == item.CreatedBy).Select(x => x.DisplayName).FirstOrDefault();
        //                item.orderpickercount = item.orderPickerDetails.Select(x => x.OrderId).Distinct().ToList().Count();
        //                item.orderpickercountitem += item.orderPickerDetails.Select(x => x.Qty).Sum();
        //                item.ClusterName = Clustername.Where(x => x.ClusterId == item.ClusterId).Select(x => x.ClusterName).FirstOrDefault();
        //                item.AgentName = peoplename.Where(x => x.PeopleID == item.AgentId).Select(x => x.DisplayName).FirstOrDefault();
        //                item.DBoyName = peoplename.Where(x => x.PeopleID == item.DboyId).Select(x => x.DisplayName).FirstOrDefault();
        //                item.InventorySupervisorName = item.InventorySupervisorId == null ? "" : peoplename.Where(x => x.PeopleID == item.InventorySupervisorId).Select(x => x.DisplayName).FirstOrDefault();
        //                //item.orderPickerDetails.OrderByDescending(x => x.ItemMultiMrpId).ToList();
        //                var AddDatalists = item.orderPickerDetails.Where(z => z.Qty > 0).GroupBy(x => new { x.ItemMultiMrpId, x.Status }).Select(x => new OrderPickerDetailsDc
        //                {
        //                    id = x.Select(w => w.id).FirstOrDefault(),
        //                    ItemMultiMrpId = x.Key.ItemMultiMrpId,
        //                    Qty = x.Sum(y => y.Qty),
        //                    ItemName = x.Select(w => w.ItemName).FirstOrDefault(),
        //                    OrderId = x.Select(w => w.OrderId).FirstOrDefault(),
        //                    OrderDetailsId = x.Select(w => w.OrderDetailsId).FirstOrDefault(),
        //                    DispatchedQty = x.Select(w => w.DispatchedQty).FirstOrDefault(),
        //                    Comment = x.Select(w => w.Comment).FirstOrDefault(),
        //                    IsFreeItem = x.Select(w => w.IsFreeItem).FirstOrDefault(),
        //                    Status = x.Select(w => w.Status).Distinct().FirstOrDefault(),
        //                    Orderids = string.Join(",", item.orderPickerDetails.Where(z => z.ItemMultiMrpId == x.Key.ItemMultiMrpId && z.Status == x.Key.Status).Select(e => e.OrderId).ToList()),
        //                }).ToList();

        //                item.orderPickerDetails = AddDatalists.OrderBy(x => x.ItemMultiMrpId).ToList();
        //                var itemMaster = context.itemMasters.Where(z => MultiMrpIds.Contains(z.ItemMultiMRPId) && z.WarehouseId == item.WarehouseId).Select(x => new { x.ItemMultiMRPId, x.Number, x.PurchaseMinOrderQty, x.WarehouseId }).ToList();
        //                foreach (var lineitems in item.orderPickerDetails)
        //                {
        //                    var purchaseMinOrderQty = itemMaster.FirstOrDefault(x => x.ItemMultiMRPId == lineitems.ItemMultiMrpId);
        //                    lineitems.MRP = MultiMrpList.FirstOrDefault(x => x.ItemMultiMRPId == lineitems.ItemMultiMrpId).MRP;
        //                    lineitems.PurchaseMinOrderQty = purchaseMinOrderQty != null ? purchaseMinOrderQty.PurchaseMinOrderQty : 0;
        //                    //if (lineitems.Status == 4 && item.IsComplted == true)
        //                    //{
        //                    //    item.Status = 5;
        //                    //}
        //                }
        //            }
        //            addlist.PickerList = newsd;
        //            addlist.Totalcount = pickerlistcount;
        //            return addlist;
        //        }
        //        else
        //        {
        //            var predicate = PredicateBuilder.New<OrderPickerMaster>(x => x.ClusterId == ClusterId && x.IsDeleted == false);
        //            if (IsComplete != null)
        //            {
        //                if (IsComplete == 0)
        //                {
        //                    predicate = predicate.And(x => x.Status == IsComplete && x.IsCanceled == false);
        //                }
        //                else if (IsComplete == 4)
        //                {
        //                    predicate = predicate.And(x => x.Status == IsComplete && x.IsCanceled == true);
        //                }
        //                else
        //                {
        //                    predicate = predicate.And(x => x.Status == IsComplete);
        //                }
        //                //predicate = predicate.And(x => x.IsComplted == IsComplete);
        //            }
        //            List<OrderPickerMaster> pickerlist = context.OrderPickerMasterDb.Where(predicate).OrderByDescending(x => x.Id).Skip((skip - 1) * take).Take(take).Include("OrderPickerDetails").ToList();
        //            pickerlistcount = context.OrderPickerMasterDb.Where(predicate).OrderByDescending(x => x.Id).Count();

        //            var people = pickerlist.Select(x => x.PickerPersonId).Distinct().ToList();
        //            var peoplecreatedby = pickerlist.Select(x => x.CreatedBy).Distinct().ToList();
        //            var agentId = pickerlist.Select(x => x.AgentId).Distinct().ToList();
        //            var dboyid = pickerlist.Select(x => x.DBoyId).Distinct().ToList();
        //            var inventorySupervisorId = pickerlist.Select(x => x.InventorySupervisorId).Distinct().ToList();
        //            var peoplename = context.Peoples.Where(x => people.Contains(x.PeopleID)
        //            || peoplecreatedby.Contains(x.PeopleID)
        //            || agentId.Contains(x.PeopleID)
        //            || dboyid.Contains(x.PeopleID)
        //            || inventorySupervisorId.Contains(x.PeopleID)).ToList();
        //            var cluster = pickerlist.Select(x => x.ClusterId).Distinct().ToList();
        //            var Clustername = context.Clusters.Where(x => cluster.Contains(x.ClusterId)).ToList();
        //            //var agentId = pickerlist.Select(x => x.AgentId).Distinct().ToList();
        //            //var agentName = context.Peoples.Where(x => agentId.Contains(x.PeopleID)).ToList();
        //            //var dboyid = pickerlist.Select(x => x.DBoyId).Distinct().ToList();
        //            //var DboyNameList = context.Peoples.Where(x => dboyid.Contains(x.PeopleID)).ToList();
        //            var newsd = new List<OrderPickerMasterDc>();
        //            newsd = Mapper.Map(pickerlist).ToANew<List<OrderPickerMasterDc>>();
        //            foreach (var item in newsd)
        //            {
        //                var MultiMrpIds = item.orderPickerDetails.Select(x => x.ItemMultiMrpId).Distinct().ToList();
        //                var MultiMrpList = context.ItemMultiMRPDB.Where(x => MultiMrpIds.Contains(x.ItemMultiMRPId)).Distinct().ToList();

        //                item.PickerPersonName = peoplename.Where(x => x.PeopleID == item.PickerPersonId).Select(x => x.DisplayName).FirstOrDefault();
        //                item.CreatedByName = peoplename.Where(x => x.PeopleID == item.CreatedBy).Select(x => x.DisplayName).FirstOrDefault();
        //                item.orderpickercount = item.orderPickerDetails.Select(x => x.OrderId).Distinct().ToList().Count();
        //                item.orderpickercountitem += item.orderPickerDetails.Select(x => x.Qty).Sum();
        //                item.ClusterName = Clustername.Where(x => x.ClusterId == item.ClusterId).Select(x => x.ClusterName).FirstOrDefault();
        //                item.AgentName = peoplename.Where(x => x.PeopleID == item.AgentId).Select(x => x.DisplayName).FirstOrDefault();
        //                item.DBoyName = peoplename.Where(x => x.PeopleID == item.DboyId).Select(x => x.DisplayName).FirstOrDefault();
        //                item.InventorySupervisorName = item.InventorySupervisorId == null ? "" : peoplename.Where(x => x.PeopleID == item.InventorySupervisorId).Select(x => x.DisplayName).FirstOrDefault();
        //                item.orderPickerDetails = item.orderPickerDetails.OrderBy(x => x.ItemMultiMrpId).ToList();
        //                var AddDatalists = item.orderPickerDetails.Where(z => z.Qty > 0).GroupBy(x => new { x.ItemMultiMrpId, x.Status }).Select(x => new OrderPickerDetailsDc
        //                {
        //                    id = x.Select(w => w.id).FirstOrDefault(),
        //                    ItemMultiMrpId = x.Key.ItemMultiMrpId,
        //                    Qty = x.Sum(y => y.Qty),
        //                    ItemName = x.Select(w => w.ItemName).FirstOrDefault(),
        //                    OrderId = x.Select(w => w.OrderId).FirstOrDefault(),
        //                    OrderDetailsId = x.Select(w => w.OrderDetailsId).FirstOrDefault(),
        //                    DispatchedQty = x.Select(w => w.DispatchedQty).FirstOrDefault(),
        //                    Comment = x.Select(w => w.Comment).FirstOrDefault(),
        //                    IsFreeItem = x.Select(w => w.IsFreeItem).FirstOrDefault(),
        //                    Status = x.Select(w => w.Status).Distinct().FirstOrDefault(),
        //                    Orderids = string.Join(",", item.orderPickerDetails.Where(z => z.ItemMultiMrpId == x.Key.ItemMultiMrpId && z.Status == x.Key.Status).Select(e => e.OrderId).ToList()),
        //                }).ToList();

        //                item.orderPickerDetails = AddDatalists.OrderBy(x => x.ItemMultiMrpId).ToList();
        //                var itemMaster = context.itemMasters.Where(z => MultiMrpIds.Contains(z.ItemMultiMRPId) && z.WarehouseId == item.WarehouseId).Select(x => new { x.ItemMultiMRPId, x.Number, x.PurchaseMinOrderQty, x.WarehouseId }).ToList();
        //                foreach (var lineitems in item.orderPickerDetails)
        //                {
        //                    var purchaseMinOrderQty = itemMaster.FirstOrDefault(x => x.ItemMultiMRPId == lineitems.ItemMultiMrpId);
        //                    lineitems.MRP = MultiMrpList.FirstOrDefault(x => x.ItemMultiMRPId == lineitems.ItemMultiMrpId).MRP;
        //                    lineitems.PurchaseMinOrderQty = purchaseMinOrderQty != null ? purchaseMinOrderQty.PurchaseMinOrderQty : 0;
        //                    //if (lineitems.Status == 4 && item.IsComplted == true)
        //                    //{
        //                    //    item.Status = 5;
        //                    //}
        //                }
        //            }
        //            addlist.PickerList = newsd;
        //            addlist.Totalcount = pickerlistcount;
        //            return addlist;
        //        }
        //    }
        //}
        [HttpGet]
        [Route("ShowPickerListA7")]
        public async Task<paggingDTO> ShowPickerListA7(int ClusterId, int WarehouseId, bool? IsComplete, int skip, int take)
        {
            using (AuthContext context = new AuthContext())
            {
                List<OrderPickerMaster> pickerList = new List<OrderPickerMaster>();
                paggingDTO addlist = new paggingDTO();
                int pickerlistcount = 0;
                if (ClusterId == 0)
                {
                    var predicate = PredicateBuilder.New<OrderPickerMaster>(x => x.WarehouseId == WarehouseId && x.IsDeleted == false);
                    if (IsComplete != null)
                    {
                        predicate = predicate.And(x => x.IsComplted == IsComplete);
                    }
                    List<OrderPickerMaster> pickerlist = context.OrderPickerMasterDb.Where(predicate).OrderByDescending(x => x.Id).Skip((skip - 1) * take).Take(take).Include("OrderPickerDetails").ToList();
                    pickerlistcount = context.OrderPickerMasterDb.Where(predicate).OrderByDescending(x => x.Id).Count();

                    var people = pickerlist.Select(x => x.PickerPersonId).Distinct().ToList();
                    var peoplecreatedby = pickerlist.Select(x => x.CreatedBy).Distinct().ToList();
                    var agentId = pickerlist.Select(x => x.AgentId).Distinct().ToList();
                    var dboyid = pickerlist.Select(x => x.DBoyId).Distinct().ToList();
                    var inventorySupervisorId = pickerlist.Select(x => x.InventorySupervisorId).Distinct().ToList();
                    var peoplename = context.Peoples.Where(x => people.Contains(x.PeopleID)
                    || peoplecreatedby.Contains(x.PeopleID)
                    || agentId.Contains(x.PeopleID)
                    || dboyid.Contains(x.PeopleID)
                    || inventorySupervisorId.Contains(x.PeopleID)).ToList();
                    //var createdname = context.Peoples.Where(x => peoplecreatedby.Contains(x.PeopleID)).ToList();
                    var cluster = pickerlist.Select(x => x.ClusterId).Distinct().ToList();
                    var Clustername = context.Clusters.Where(x => cluster.Contains(x.ClusterId)).ToList();

                    // var agentName = context.Peoples.Where(x => agentId.Contains(x.PeopleID)).ToList();
                    //var DboyName = context.Peoples.Where(x => dboyid.Contains(x.PeopleID)).ToList();
                    var newsd = new List<OrderPickerMasterDc>();
                    newsd = Mapper.Map(pickerlist).ToANew<List<OrderPickerMasterDc>>();
                    foreach (var item in newsd)
                    {
                        var MultiMrpIds = item.orderPickerDetails.Select(x => x.ItemMultiMrpId).Distinct().ToList();
                        var MultiMrpList = context.ItemMultiMRPDB.Where(x => MultiMrpIds.Contains(x.ItemMultiMRPId)).Distinct().ToList();

                        item.PickerPersonName = peoplename.Where(x => x.PeopleID == item.PickerPersonId).Select(x => x.DisplayName).FirstOrDefault();
                        item.CreatedByName = peoplename.Where(x => x.PeopleID == item.CreatedBy).Select(x => x.DisplayName).FirstOrDefault();
                        item.orderpickercount = item.orderPickerDetails.Select(x => x.OrderId).Distinct().ToList().Count();
                        item.orderpickercountitem += item.orderPickerDetails.Select(x => x.Qty).Sum();
                        item.ClusterName = Clustername.Where(x => x.ClusterId == item.ClusterId).Select(x => x.ClusterName).FirstOrDefault();
                        item.AgentName = peoplename.Where(x => x.PeopleID == item.AgentId).Select(x => x.DisplayName).FirstOrDefault();
                        item.DBoyName = peoplename.Where(x => x.PeopleID == item.DboyId).Select(x => x.DisplayName).FirstOrDefault();
                        item.InventorySupervisorName = item.InventorySupervisorId == null ? "" : peoplename.Where(x => x.PeopleID == item.InventorySupervisorId).Select(x => x.DisplayName).FirstOrDefault();
                        //item.orderPickerDetails.OrderByDescending(x => x.ItemMultiMrpId).ToList();
                        var AddDatalists = item.orderPickerDetails.Where(z => z.Qty > 0).GroupBy(x => new { x.ItemMultiMrpId, x.Status }).Select(x => new OrderPickerDetailsDc
                        {
                            id = x.Select(w => w.id).FirstOrDefault(),
                            ItemMultiMrpId = x.Key.ItemMultiMrpId,
                            Qty = x.Sum(y => y.Qty),
                            ItemName = x.Select(w => w.ItemName).FirstOrDefault(),
                            OrderId = x.Select(w => w.OrderId).FirstOrDefault(),
                            OrderDetailsId = x.Select(w => w.OrderDetailsId).FirstOrDefault(),
                            DispatchedQty = x.Select(w => w.DispatchedQty).FirstOrDefault(),
                            Comment = x.Select(w => w.Comment).FirstOrDefault(),
                            IsFreeItem = x.Select(w => w.IsFreeItem).FirstOrDefault(),
                            Status = x.Select(w => w.Status).Distinct().FirstOrDefault(),
                            Orderids = string.Join(",", item.orderPickerDetails.Where(z => z.ItemMultiMrpId == x.Key.ItemMultiMrpId && z.Status == x.Key.Status).Select(e => e.OrderId).ToList()),
                        }).ToList();

                        item.orderPickerDetails = AddDatalists.OrderBy(x => x.ItemMultiMrpId).ToList();
                        var itemMaster = context.itemMasters.Where(z => MultiMrpIds.Contains(z.ItemMultiMRPId) && z.WarehouseId == item.WarehouseId).Select(x => new { x.ItemMultiMRPId, x.Number, x.PurchaseMinOrderQty, x.WarehouseId }).ToList();
                        foreach (var lineitems in item.orderPickerDetails)
                        {
                            var purchaseMinOrderQty = itemMaster.FirstOrDefault(x => x.ItemMultiMRPId == lineitems.ItemMultiMrpId);
                            lineitems.MRP = MultiMrpList.FirstOrDefault(x => x.ItemMultiMRPId == lineitems.ItemMultiMrpId).MRP;
                            lineitems.PurchaseMinOrderQty = purchaseMinOrderQty != null ? purchaseMinOrderQty.PurchaseMinOrderQty : 0;
                            //if (lineitems.Status == 4 && item.IsComplted == true)
                            //{
                            //    item.Status = 5;
                            //}
                        }
                    }
                    addlist.PickerList = newsd;
                    addlist.Totalcount = pickerlistcount;
                    return addlist;
                }
                else
                {
                    var predicate = PredicateBuilder.New<OrderPickerMaster>(x => x.ClusterId == ClusterId && x.IsDeleted == false);
                    if (IsComplete != null)
                    {
                        predicate = predicate.And(x => x.IsComplted == IsComplete);
                    }
                    List<OrderPickerMaster> pickerlist = context.OrderPickerMasterDb.Where(predicate).OrderByDescending(x => x.Id).Skip((skip - 1) * take).Take(take).Include("OrderPickerDetails").ToList();
                    pickerlistcount = context.OrderPickerMasterDb.Where(predicate).OrderByDescending(x => x.Id).Count();

                    var people = pickerlist.Select(x => x.PickerPersonId).Distinct().ToList();
                    var peoplecreatedby = pickerlist.Select(x => x.CreatedBy).Distinct().ToList();
                    var agentId = pickerlist.Select(x => x.AgentId).Distinct().ToList();
                    var dboyid = pickerlist.Select(x => x.DBoyId).Distinct().ToList();
                    var inventorySupervisorId = pickerlist.Select(x => x.InventorySupervisorId).Distinct().ToList();
                    var peoplename = context.Peoples.Where(x => people.Contains(x.PeopleID)
                    || peoplecreatedby.Contains(x.PeopleID)
                    || agentId.Contains(x.PeopleID)
                    || dboyid.Contains(x.PeopleID)
                    || inventorySupervisorId.Contains(x.PeopleID)).ToList();
                    var cluster = pickerlist.Select(x => x.ClusterId).Distinct().ToList();
                    var Clustername = context.Clusters.Where(x => cluster.Contains(x.ClusterId)).ToList();
                    //var agentId = pickerlist.Select(x => x.AgentId).Distinct().ToList();
                    //var agentName = context.Peoples.Where(x => agentId.Contains(x.PeopleID)).ToList();
                    //var dboyid = pickerlist.Select(x => x.DBoyId).Distinct().ToList();
                    //var DboyNameList = context.Peoples.Where(x => dboyid.Contains(x.PeopleID)).ToList();
                    var newsd = new List<OrderPickerMasterDc>();
                    newsd = Mapper.Map(pickerlist).ToANew<List<OrderPickerMasterDc>>();
                    foreach (var item in newsd)
                    {
                        var MultiMrpIds = item.orderPickerDetails.Select(x => x.ItemMultiMrpId).Distinct().ToList();
                        var MultiMrpList = context.ItemMultiMRPDB.Where(x => MultiMrpIds.Contains(x.ItemMultiMRPId)).Distinct().ToList();

                        item.PickerPersonName = peoplename.Where(x => x.PeopleID == item.PickerPersonId).Select(x => x.DisplayName).FirstOrDefault();
                        item.CreatedByName = peoplename.Where(x => x.PeopleID == item.CreatedBy).Select(x => x.DisplayName).FirstOrDefault();
                        item.orderpickercount = item.orderPickerDetails.Select(x => x.OrderId).Distinct().ToList().Count();
                        item.orderpickercountitem += item.orderPickerDetails.Select(x => x.Qty).Sum();
                        item.ClusterName = Clustername.Where(x => x.ClusterId == item.ClusterId).Select(x => x.ClusterName).FirstOrDefault();
                        item.AgentName = peoplename.Where(x => x.PeopleID == item.AgentId).Select(x => x.DisplayName).FirstOrDefault();
                        item.DBoyName = peoplename.Where(x => x.PeopleID == item.DboyId).Select(x => x.DisplayName).FirstOrDefault();
                        item.InventorySupervisorName = item.InventorySupervisorId == null ? "" : peoplename.Where(x => x.PeopleID == item.InventorySupervisorId).Select(x => x.DisplayName).FirstOrDefault();
                        item.orderPickerDetails = item.orderPickerDetails.OrderBy(x => x.ItemMultiMrpId).ToList();
                        var AddDatalists = item.orderPickerDetails.Where(z => z.Qty > 0).GroupBy(x => new { x.ItemMultiMrpId, x.Status }).Select(x => new OrderPickerDetailsDc
                        {
                            id = x.Select(w => w.id).FirstOrDefault(),
                            ItemMultiMrpId = x.Key.ItemMultiMrpId,
                            Qty = x.Sum(y => y.Qty),
                            ItemName = x.Select(w => w.ItemName).FirstOrDefault(),
                            OrderId = x.Select(w => w.OrderId).FirstOrDefault(),
                            OrderDetailsId = x.Select(w => w.OrderDetailsId).FirstOrDefault(),
                            DispatchedQty = x.Select(w => w.DispatchedQty).FirstOrDefault(),
                            Comment = x.Select(w => w.Comment).FirstOrDefault(),
                            IsFreeItem = x.Select(w => w.IsFreeItem).FirstOrDefault(),
                            Status = x.Select(w => w.Status).Distinct().FirstOrDefault(),
                            Orderids = string.Join(",", item.orderPickerDetails.Where(z => z.ItemMultiMrpId == x.Key.ItemMultiMrpId && z.Status == x.Key.Status).Select(e => e.OrderId).ToList()),
                        }).ToList();

                        item.orderPickerDetails = AddDatalists.OrderBy(x => x.ItemMultiMrpId).ToList();
                        var itemMaster = context.itemMasters.Where(z => MultiMrpIds.Contains(z.ItemMultiMRPId) && z.WarehouseId == item.WarehouseId).Select(x => new { x.ItemMultiMRPId, x.Number, x.PurchaseMinOrderQty, x.WarehouseId }).ToList();
                        foreach (var lineitems in item.orderPickerDetails)
                        {
                            var purchaseMinOrderQty = itemMaster.FirstOrDefault(x => x.ItemMultiMRPId == lineitems.ItemMultiMrpId);
                            lineitems.MRP = MultiMrpList.FirstOrDefault(x => x.ItemMultiMRPId == lineitems.ItemMultiMrpId).MRP;
                            lineitems.PurchaseMinOrderQty = purchaseMinOrderQty != null ? purchaseMinOrderQty.PurchaseMinOrderQty : 0;
                            //if (lineitems.Status == 4 && item.IsComplted == true)
                            //{
                            //    item.Status = 5;
                            //}
                        }
                    }
                    addlist.PickerList = newsd;
                    addlist.Totalcount = pickerlistcount;
                    return addlist;
                }
            }
        }


        //Get: GetOrderPickerMasterList
        [HttpGet]
        [Route("GetOrderPickerMasterList")]
        public async Task<paggingDTO> GetOrderPickerMasterList(int ClusterId, int WarehouseId, int status, int skip, int take)
        {
            List<OrderPickerMaster> pickerList = new List<OrderPickerMaster>();
            paggingDTO addlist = new paggingDTO();
            using (var myContext = new AuthContext())
            {
                if (myContext.Database.Connection.State != ConnectionState.Open)
                    myContext.Database.Connection.Open();

                var cmd = myContext.Database.Connection.CreateCommand();
                cmd.CommandText = "[dbo].[GetOrderPickerMaster]";
                cmd.Parameters.Add(new SqlParameter("@ClusterId", ClusterId));
                cmd.Parameters.Add(new SqlParameter("@WarehouseId", WarehouseId));
                cmd.Parameters.Add(new SqlParameter("@status", status));
                cmd.Parameters.Add(new SqlParameter("@skip", (skip - 1) * take));
                cmd.Parameters.Add(new SqlParameter("@take", take));
                cmd.CommandType = System.Data.CommandType.StoredProcedure;

                // Run the sproc
                var reader = cmd.ExecuteReader();
                addlist.PickerList = ((IObjectContextAdapter)myContext)
                .ObjectContext
                .Translate<OrderPickerMasterDc>(reader).ToList();
                reader.NextResult();
                if (reader.Read())
                {
                    addlist.Totalcount = Convert.ToInt32(reader["TotalCount"]);
                }
            }
            return addlist;
        }

        [Route("GetOrderPickerDetailsByOrderPickerMasterId")]
        [HttpGet]
        public async Task<List<OrderPickerDetailsDc>> GetOrderPickerDetailsByOrderPickerMasterId(int OrderPickerMasterId, int WarehouseId)
        {
            List<OrderPickerDetailsDc> res = new List<OrderPickerDetailsDc>();
            using (var myContext = new AuthContext())
            {
                var orderPickerMasterIdParam = new SqlParameter("@OrderPickerMasterId", OrderPickerMasterId);
                var wIdParam = new SqlParameter("@WarehouseId", WarehouseId);

                res = await myContext.Database.SqlQuery<OrderPickerDetailsDc>("GetOrderPickerDetails @OrderPickerMasterId,@WarehouseId", orderPickerMasterIdParam, wIdParam).ToListAsync();
                //.GroupBy(x => x.ItemMultiMrpId)
                //foreach (var item in res)
                //{
                //item.Orderids = string.Join(",", item.OrderId);
                //}
            }
            return res;
        }


        private PaggingDataPickerDc GetOrdersFromMongo(GetPendingOrderFilterDc getPendingOrderFilterDc)
        {
            try
            {
                PaggingDataPickerDc paggingData = new PaggingDataPickerDc();
                int skip = (getPendingOrderFilterDc.PageNo - 1);
                int take = getPendingOrderFilterDc.ItemPerPage;
                var identity = User.Identity as ClaimsIdentity;
                int compid = 0;
                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))
                    compid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);
                MongoDbHelper<MongoOrderMaster> mongoDbHelper = new MongoDbHelper<MongoOrderMaster>();
                //if (getPendingOrderFilterDc.WareHouseID == 0)
                //{
                //var orderPredicate = PredicateBuilder.New<MongoOrderMaster>(x => x.Status == "Pending" && !x.Deleted);


                var orderPredicate = PredicateBuilder.New<MongoOrderMaster>(x => x.WarehouseId == getPendingOrderFilterDc.WareHouseID && x.Status == "Pending" && !x.Deleted);

                if (getPendingOrderFilterDc.ClusterId != null && getPendingOrderFilterDc.ClusterId > 0)
                {
                    orderPredicate = orderPredicate.And(x => x.ClusterId == getPendingOrderFilterDc.ClusterId);
                }
                var orderMasters = new List<MongoOrderMaster>();

                orderMasters = mongoDbHelper.Select(orderPredicate, x => x.OrderByDescending(z => z.CreatedDate), null, null, collectionName: "OrderMaster").ToList();

                int dataCount = mongoDbHelper.Count(orderPredicate, collectionName: "OrderMaster");

                var newdata = orderMasters.Select(x => new OrderDetailsPickerDC
                {
                    OrderId = x.OrderId,
                    CustomerName = x.CustomerName,
                    Skcode = x.Skcode,
                    ShopName = x.ShopName,
                    Status = x.Status,
                    invoice_no = x.invoice_no,
                    BillingAddress = x.BillingAddress,
                    CreatedDate = x.CreatedDate,
                    ReDispatchCount = x.ReDispatchCount,
                    ClusterId = x.ClusterId,
                    ClusterName = x.ClusterName,
                    comments = x.comments,
                    TotalAmount = x.TotalAmount,
                    GrossAmount = x.GrossAmount,
                    DiscountAmount = x.DiscountAmount,
                    OrderAmount = x.OrderAmount,

                    WalletAmount = x.WalletAmount,

                    OrderType = x.OrderType,
                    BillDiscountAmount = x.BillDiscountAmount,
                    OfferCode = x.OfferCode,
                    UpdatedDate = TimeZoneInfo.ConvertTime(x.UpdatedDate, TimeZoneInfo.FindSystemTimeZoneById("India Standard Time")),
                    SalesPerson = string.Join(",", x.orderDetails.Where(z => !string.IsNullOrEmpty(z.ExecutiveName)).Select(z => z.ExecutiveName).Distinct()),
                    ShippingAddress = x.ShippingAddress,
                    OrderTakenSalesPersonId = x.OrderTakenSalesPersonId,
                    OrderTakenSalesPerson = x.OrderTakenSalesPerson,
                    paymentMode = x.paymentMode,
                    orderDetails = x.orderDetails.Select(z => new OrderDetailsDc
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
                        FreeWithParentItemId = z.FreeWithParentItemId


                    }).ToList()
                }).ToList();


                var currentStocks = new List<CurrentStockMinDc>();
                var multimrpIds = newdata.SelectMany(z => z.orderDetails).Select(x => new { x.ItemMultiMRPId, x.WarehouseId, x.IsFreeItem }).Distinct().ToList();
                var Orderitems = newdata.SelectMany(x => x.orderDetails.Select(y => y.ItemId)).ToList();
                var allpendingOrders = mongoDbHelper.Select(x => !x.Deleted && x.Status == "Pending"
                                                         && x.orderDetails.Any(z => Orderitems.Contains(z.ItemId)), collectionName: "OrderMaster");
                allpendingOrders.ForEach(x => x.orderDetails.ForEach(y => y.WarehouseId = x.WarehouseId));
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
                        dr["IsFreeItem"] = item.IsFreeItem;
                        orderIdDt.Rows.Add(dr);
                    }
                    var param = new SqlParameter("Items", orderIdDt);
                    param.SqlDbType = SqlDbType.Structured;
                    param.TypeName = "dbo.itemtype";
                    var cmd = authContext.Database.Connection.CreateCommand();
                    cmd.CommandText = "[dbo].[GetCurrentStock]";
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.Parameters.Add(param);
                    using (var reader = cmd.ExecuteReader())
                    {
                        currentStocks = ((IObjectContextAdapter)authContext)
                                        .ObjectContext
                                        .Translate<CurrentStockMinDc>(reader).ToList();
                    }
                }
                var pendingOrderIds = newdata.Select(z => z.OrderId);
                var allPendingItems = allpendingOrders.SelectMany(x => x.orderDetails).GroupBy(x => new { x.ItemMultiMRPId, x.itemNumber, x.WarehouseId, x.IsFreeItem }).Select(x => new
                {
                    x.Key.itemNumber,
                    x.Key.ItemMultiMRPId,
                    x.Key.WarehouseId,
                    x.Key.IsFreeItem,
                    TotalReqQty = x.Sum(y => y.qty),
                    TotalAvlQty = currentStocks != null && currentStocks.Any(z => z.ItemNumber == x.Key.itemNumber && z.ItemMultiMRPId == x.Key.ItemMultiMRPId && z.IsFreeItem == x.Key.IsFreeItem && z.WarehouseId == x.Key.WarehouseId) ? currentStocks.Where(z => z.ItemNumber == x.Key.itemNumber && z.ItemMultiMRPId == x.Key.ItemMultiMRPId && z.WarehouseId == x.Key.WarehouseId).Sum(y => y.CurrentInventory) : 0,
                    Orders = x.Select(y => new { y.OrderDetailsId, y.OrderId, y.qty }).ToList()
                }).ToList();
                foreach (var item in newdata)
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
                                using (var authContext = new AuthContext())
                                {
                                    int freeitemqty = authContext.DbCurrentStock.Where(x => x.ItemMultiMRPId == orderinfo.ItemMultiMRPId && x.WarehouseId == orderinfo.WarehouseId && x.ItemNumber == orderinfo.itemNumber).Select(x => x.CurrentInventory).FirstOrDefault();
                                    if (items.Any(y => y.Orders.Any(z => z.OrderId == item.OrderId && z.qty <= freeitemqty && y.IsFreeItem == true)))
                                    {
                                        item.IsLessCurrentStock = false;
                                        item.isCompleted = true;
                                    }
                                }
                            }
                        }
                    }
                    //else if (items.Any(y => y.Orders.Any(z => z.OrderId == item.OrderId && z.qty > y.TotalAvlQty)))
                    //{
                    //    item.IsLessCurrentStock = true;
                    //    item.isCompleted = false;
                    //}
                    else if (items.All(y => y.Orders.Where(z => z.OrderId >= item.OrderId).Sum(x => x.qty) >= y.TotalAvlQty))
                    {
                        item.isCompleted = true;
                        item.IsLessCurrentStock = false;
                    }
                }
                paggingData.total_count = dataCount;
                paggingData.ordermaster = newdata;
                return paggingData;
                //}
                //else
                //{
                //    var orderPredicate = PredicateBuilder.New<MongoOrderMaster>(x => x.WarehouseId == getPendingOrderFilterDc.WareHouseID && x.Status == "Pending" && !x.Deleted);

                //    if (getPendingOrderFilterDc.ClusterId != null && getPendingOrderFilterDc.ClusterId > 0)
                //    {
                //        orderPredicate = orderPredicate.And(x => x.ClusterId == getPendingOrderFilterDc.ClusterId);
                //    }
                //    var orderMasters = new List<MongoOrderMaster>();

                //    orderMasters = mongoDbHelper.Select(orderPredicate, x => x.OrderByDescending(z => z.CreatedDate), skip, take, collectionName: "OrderMaster").ToList();

                //    int dataCount = mongoDbHelper.Count(orderPredicate, collectionName: "OrderMaster");
                //    var newdata = orderMasters.Select(x => new OrderDetailsPickerDC
                //    {
                //        OrderId = x.OrderId,
                //        CustomerName = x.CustomerName,
                //        Skcode = x.Skcode,
                //        ShopName = x.ShopName,
                //        Status = x.Status,
                //        invoice_no = x.invoice_no,
                //        BillingAddress = x.BillingAddress,
                //        CreatedDate = x.CreatedDate,
                //        ReDispatchCount = x.ReDispatchCount,
                //        ClusterId = x.ClusterId,
                //        ClusterName = x.ClusterName,
                //        comments = x.comments,
                //        TotalAmount = x.TotalAmount,
                //        GrossAmount = x.GrossAmount,
                //        DiscountAmount = x.DiscountAmount,
                //        OrderAmount=x.OrderAmount,
                //        OfferCode = x.offertype,
                //        UpdatedDate = TimeZoneInfo.ConvertTime(x.UpdatedDate, TimeZoneInfo.FindSystemTimeZoneById("India Standard Time")),
                //        SalesPerson = string.Join(",", x.orderDetails.Where(z => !string.IsNullOrEmpty(z.ExecutiveName)).Select(z => z.ExecutiveName).Distinct()),
                //        ShippingAddress = x.ShippingAddress,
                //        OrderTakenSalesPersonId = x.OrderTakenSalesPersonId,
                //        OrderTakenSalesPerson = x.OrderTakenSalesPerson,
                //        paymentMode = x.paymentMode,
                //        orderDetails = x.orderDetails.Select(z => new OrderDetailsDc
                //        {
                //            OrderDetailsId = z.OrderDetailsId,
                //            OrderId = z.OrderId,
                //            CustomerId = z.CustomerId,
                //            CustomerName = z.CustomerName,
                //            City = z.City,
                //            Mobile = z.Mobile,
                //            OrderDate = TimeZoneInfo.ConvertTime(z.OrderDate, TimeZoneInfo.FindSystemTimeZoneById("India Standard Time")),
                //            CompanyId = z.CompanyId,
                //            CityId = z.CityId,
                //            WarehouseId = x.WarehouseId,
                //            WarehouseName = x.WarehouseName,
                //            CategoryName = z.CategoryName,
                //            SubcategoryName = z.SubcategoryName,
                //            SubsubcategoryName = z.SubsubcategoryName,
                //            SellingSku = z.SellingSku,
                //            ItemId = z.ItemId,
                //            Itempic = z.Itempic,
                //            itemname = z.itemname,
                //            SellingUnitName = z.SellingUnitName,
                //            itemcode = z.itemcode,
                //            itemNumber = z.itemNumber,
                //            HSNCode = z.HSNCode,
                //            Barcode = z.Barcode,
                //            price = z.price,
                //            UnitPrice = z.UnitPrice,
                //            Purchaseprice = z.Purchaseprice,
                //            MinOrderQty = z.MinOrderQty,
                //            MinOrderQtyPrice = z.MinOrderQtyPrice,
                //            qty = z.qty,
                //            Noqty = z.Noqty,
                //            AmtWithoutTaxDisc = z.AmtWithoutTaxDisc,
                //            AmtWithoutAfterTaxDisc = z.AmtWithoutAfterTaxDisc,
                //            TotalAmountAfterTaxDisc = z.TotalAmountAfterTaxDisc,
                //            NetAmmount = z.NetAmmount,
                //            DiscountPercentage = z.DiscountPercentage,
                //            DiscountAmmount = z.DiscountAmmount,
                //            NetAmtAfterDis = z.NetAmtAfterDis,
                //            TaxPercentage = z.TaxPercentage,
                //            TaxAmmount = z.TaxAmmount,
                //            SGSTTaxPercentage = z.SGSTTaxPercentage,
                //            SGSTTaxAmmount = z.SGSTTaxAmmount,
                //            CGSTTaxPercentage = z.CGSTTaxPercentage,
                //            CGSTTaxAmmount = z.CGSTTaxAmmount,
                //            TotalCessPercentage = z.TotalCessPercentage,
                //            CessTaxAmount = z.CessTaxAmount,
                //            TotalAmt = z.TotalAmt,
                //            OrderedTotalAmt = z.OrderedTotalAmt,
                //            CreatedDate = TimeZoneInfo.ConvertTime(z.CreatedDate, TimeZoneInfo.FindSystemTimeZoneById("India Standard Time")),
                //            UpdatedDate = TimeZoneInfo.ConvertTime(z.UpdatedDate, TimeZoneInfo.FindSystemTimeZoneById("India Standard Time")),
                //            Deleted = z.Deleted,
                //            Status = z.Status,
                //            SizePerUnit = z.SizePerUnit,
                //            marginPoint = z.marginPoint,
                //            promoPoint = z.promoPoint,
                //            NetPurchasePrice = z.NetPurchasePrice,
                //            CurrentStock = z.CurrentStock,
                //            day = z.day,
                //            month = z.month,
                //            year = z.year,
                //            status = z.status,
                //            SupplierName = z.SupplierName,
                //            ItemMultiMRPId = z.ItemMultiMRPId,
                //            IsFreeItem = z.IsFreeItem,
                //            FreeWithParentItemId = z.FreeWithParentItemId
                //        }).ToList()
                //    }).ToList();


                //    var currentStocks = new List<CurrentStockMinDc>();
                //    var multimrpIds = newdata.SelectMany(z => z.orderDetails).Select(x => new { x.ItemMultiMRPId, x.WarehouseId, x.IsFreeItem }).Distinct().ToList();
                //    var Orderitems = newdata.SelectMany(x => x.orderDetails.Select(y => y.ItemId)).ToList();
                //    var allpendingOrders = mongoDbHelper.Select(x => !x.Deleted && x.Status == "Pending"
                //                                             && x.orderDetails.Any(z => Orderitems.Contains(z.ItemId)), collectionName: "OrderMaster");
                //    allpendingOrders.ForEach(x => x.orderDetails.ForEach(y => y.WarehouseId = x.WarehouseId));
                //    using (var authContext = new AuthContext())
                //    {
                //        if (authContext.Database.Connection.State != ConnectionState.Open)
                //            authContext.Database.Connection.Open();
                //        var orderIdDt = new DataTable();
                //        orderIdDt.Columns.Add("ItemMultiMRPId");
                //        orderIdDt.Columns.Add("WarehouseId");
                //        orderIdDt.Columns.Add("IsFreeItem");
                //        foreach (var item in multimrpIds)
                //        {
                //            var dr = orderIdDt.NewRow();
                //            dr["ItemMultiMRPId"] = item.ItemMultiMRPId;
                //            dr["WarehouseId"] = item.WarehouseId;
                //            dr["IsFreeItem"] = item.IsFreeItem;
                //            orderIdDt.Rows.Add(dr);
                //        }
                //        var param = new SqlParameter("Items", orderIdDt);
                //        param.SqlDbType = SqlDbType.Structured;
                //        param.TypeName = "dbo.itemtype";
                //        var cmd = authContext.Database.Connection.CreateCommand();
                //        cmd.CommandText = "[dbo].[GetCurrentStock]";
                //        cmd.CommandType = System.Data.CommandType.StoredProcedure;
                //        cmd.Parameters.Add(param);
                //        using (var reader = cmd.ExecuteReader())
                //        {
                //            currentStocks = ((IObjectContextAdapter)authContext)
                //                            .ObjectContext
                //                            .Translate<CurrentStockMinDc>(reader).ToList();
                //        }
                //    }
                //    var pendingOrderIds = newdata.Select(z => z.OrderId);
                //    var allPendingItems = allpendingOrders.SelectMany(x => x.orderDetails).GroupBy(x => new { x.ItemMultiMRPId, x.itemNumber, x.WarehouseId, x.IsFreeItem }).Select(x => new
                //    {
                //        x.Key.itemNumber,
                //        x.Key.ItemMultiMRPId,
                //        x.Key.WarehouseId,
                //        x.Key.IsFreeItem,
                //        TotalReqQty = x.Sum(y => y.qty),
                //        TotalAvlQty = currentStocks != null && currentStocks.Any(z => z.ItemNumber == x.Key.itemNumber && z.ItemMultiMRPId == x.Key.ItemMultiMRPId && z.IsFreeItem == x.Key.IsFreeItem && z.WarehouseId == x.Key.WarehouseId) ? currentStocks.Where(z => z.ItemNumber == x.Key.itemNumber && z.ItemMultiMRPId == x.Key.ItemMultiMRPId && z.WarehouseId == x.Key.WarehouseId).Sum(y => y.CurrentInventory) : 0,
                //        Orders = x.Select(y => new { y.OrderDetailsId, y.OrderId, y.qty }).ToList()
                //    }).ToList();
                //    foreach (var item in newdata)
                //    {
                //        var items = allPendingItems.Where(x => x.Orders.Any(y => y.OrderId == item.OrderId)).ToList();
                //        if (items.All(y => y.TotalAvlQty >= y.TotalReqQty))
                //        {
                //            item.isCompleted = true;
                //            item.IsLessCurrentStock = false;
                //        }
                //        else if (items.Any(y => y.Orders.Any(z => z.OrderId == item.OrderId && z.qty > y.TotalAvlQty)))
                //        {
                //            item.IsLessCurrentStock = true;
                //            item.isCompleted = false;
                //            if (items.Any(y => y.Orders.Any(z => z.OrderId == item.OrderId && z.qty > y.TotalAvlQty && y.IsFreeItem == true)))
                //            {
                //                var orderinfo = allpendingOrders.Where(x => x.OrderId == item.OrderId).Select(x => x.orderDetails.Where(y => y.IsFreeItem == true && y.IsDispatchedFreeStock == false).Select(z => new { z.itemNumber, z.ItemMultiMRPId, z.WarehouseId }).FirstOrDefault()).FirstOrDefault();
                //                if (orderinfo != null)
                //                {
                //                    using (var authContext = new AuthContext())
                //                    {
                //                        int freeitemqty = authContext.DbCurrentStock.Where(x => x.ItemMultiMRPId == orderinfo.ItemMultiMRPId && x.WarehouseId == orderinfo.WarehouseId && x.ItemNumber == orderinfo.itemNumber).Select(x => x.CurrentInventory).FirstOrDefault();
                //                        if (items.Any(y => y.Orders.Any(z => z.OrderId == item.OrderId && z.qty <= freeitemqty && y.IsFreeItem == true)))
                //                        {
                //                            item.IsLessCurrentStock = false;
                //                            item.isCompleted = true;
                //                        }
                //                    }
                //                }
                //            }
                //        }
                //        //else if (items.Any(y => y.Orders.Any(z => z.OrderId == item.OrderId && z.qty > y.TotalAvlQty)))
                //        //{
                //        //    item.IsLessCurrentStock = true;
                //        //    item.isCompleted = false;
                //        //}
                //        else if (items.All(y => y.Orders.Where(z => z.OrderId >= item.OrderId).Sum(x => x.qty) >= y.TotalAvlQty))
                //        {
                //            item.isCompleted = true;
                //            item.IsLessCurrentStock = false;
                //        }
                //    }
                //    paggingData.total_count = dataCount;
                //    paggingData.ordermaster = newdata;
                //    return paggingData;
                //}
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        private PaggingDataPickerDc GetOrdersByIdFromMongo(GetPendingOrderFilterDc getPendingOrderFilterDc)
        {
            try
            {
                PaggingDataPickerDc paggingData = new PaggingDataPickerDc();
                int skip = (getPendingOrderFilterDc.PageNo - 1);
                int take = getPendingOrderFilterDc.ItemPerPage;
                var identity = User.Identity as ClaimsIdentity;
                int compid = 0;
                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))
                    compid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);
                MongoDbHelper<MongoOrderMaster> mongoDbHelper = new MongoDbHelper<MongoOrderMaster>();

                var orderPredicate = PredicateBuilder.New<MongoOrderMaster>(x => x.WarehouseId == getPendingOrderFilterDc.WareHouseID && x.OrderId == getPendingOrderFilterDc.OrderId && x.Status == "Pending" && !x.Deleted);

                if (getPendingOrderFilterDc.ClusterId != null && getPendingOrderFilterDc.ClusterId > 0)
                {
                    orderPredicate = orderPredicate.And(x => x.ClusterId == getPendingOrderFilterDc.ClusterId);
                }
                var orderMasters = new List<MongoOrderMaster>();

                orderMasters = mongoDbHelper.Select(orderPredicate, x => x.OrderByDescending(z => z.CreatedDate), skip, take, collectionName: "OrderMaster").ToList();

                int dataCount = mongoDbHelper.Count(orderPredicate, collectionName: "OrderMaster");

                var newdata = orderMasters.Select(x => new OrderDetailsPickerDC
                {
                    OrderId = x.OrderId,
                    CustomerName = x.CustomerName,
                    Skcode = x.Skcode,
                    ShopName = x.ShopName,
                    Status = x.Status,
                    invoice_no = x.invoice_no,
                    BillingAddress = x.BillingAddress,
                    CreatedDate = x.CreatedDate,
                    ReDispatchCount = x.ReDispatchCount,
                    ClusterId = x.ClusterId,
                    ClusterName = x.ClusterName,
                    comments = x.comments,
                    TotalAmount = x.TotalAmount,
                    GrossAmount = x.GrossAmount,
                    DiscountAmount = x.DiscountAmount,
                    OrderAmount = x.OrderAmount,

                    WalletAmount = x.WalletAmount,

                    OrderType = x.OrderType,
                    BillDiscountAmount = x.BillDiscountAmount,
                    OfferCode = x.OfferCode,
                    UpdatedDate = TimeZoneInfo.ConvertTime(x.UpdatedDate, TimeZoneInfo.FindSystemTimeZoneById("India Standard Time")),
                    SalesPerson = string.Join(",", x.orderDetails.Where(z => !string.IsNullOrEmpty(z.ExecutiveName)).Select(z => z.ExecutiveName).Distinct()),
                    ShippingAddress = x.ShippingAddress,
                    OrderTakenSalesPersonId = x.OrderTakenSalesPersonId,
                    OrderTakenSalesPerson = x.OrderTakenSalesPerson,
                    paymentMode = x.paymentMode,
                    orderDetails = x.orderDetails.Select(z => new OrderDetailsDc
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
                        FreeWithParentItemId = z.FreeWithParentItemId


                    }).ToList()
                }).ToList();


                var currentStocks = new List<CurrentStockMinDc>();
                var multimrpIds = newdata.SelectMany(z => z.orderDetails).Select(x => new { x.ItemMultiMRPId, x.WarehouseId, x.IsFreeItem }).Distinct().ToList();
                var Orderitems = newdata.SelectMany(x => x.orderDetails.Select(y => y.ItemId)).ToList();
                var allpendingOrders = mongoDbHelper.Select(x => !x.Deleted && x.Status == "Pending"
                                                         && x.orderDetails.Any(z => Orderitems.Contains(z.ItemId)), collectionName: "OrderMaster");
                allpendingOrders.ForEach(x => x.orderDetails.ForEach(y => y.WarehouseId = x.WarehouseId));
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
                        dr["IsFreeItem"] = item.IsFreeItem;
                        orderIdDt.Rows.Add(dr);
                    }
                    var param = new SqlParameter("Items", orderIdDt);
                    param.SqlDbType = SqlDbType.Structured;
                    param.TypeName = "dbo.itemtype";
                    var cmd = authContext.Database.Connection.CreateCommand();
                    cmd.CommandText = "[dbo].[GetCurrentStock]";
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.Parameters.Add(param);
                    using (var reader = cmd.ExecuteReader())
                    {
                        currentStocks = ((IObjectContextAdapter)authContext)
                                        .ObjectContext
                                        .Translate<CurrentStockMinDc>(reader).ToList();
                    }
                }
                var pendingOrderIds = newdata.Select(z => z.OrderId);
                var allPendingItems = allpendingOrders.SelectMany(x => x.orderDetails).GroupBy(x => new { x.ItemMultiMRPId, x.itemNumber, x.WarehouseId, x.IsFreeItem }).Select(x => new
                {
                    x.Key.itemNumber,
                    x.Key.ItemMultiMRPId,
                    x.Key.WarehouseId,
                    x.Key.IsFreeItem,
                    TotalReqQty = x.Sum(y => y.qty),
                    TotalAvlQty = currentStocks != null && currentStocks.Any(z => z.ItemNumber == x.Key.itemNumber && z.ItemMultiMRPId == x.Key.ItemMultiMRPId && z.IsFreeItem == x.Key.IsFreeItem && z.WarehouseId == x.Key.WarehouseId) ? currentStocks.Where(z => z.ItemNumber == x.Key.itemNumber && z.ItemMultiMRPId == x.Key.ItemMultiMRPId && z.WarehouseId == x.Key.WarehouseId).Sum(y => y.CurrentInventory) : 0,
                    Orders = x.Select(y => new { y.OrderDetailsId, y.OrderId, y.qty }).ToList()
                }).ToList();
                foreach (var item in newdata)
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
                                using (var authContext = new AuthContext())
                                {
                                    int freeitemqty = authContext.DbCurrentStock.Where(x => x.ItemMultiMRPId == orderinfo.ItemMultiMRPId && x.WarehouseId == orderinfo.WarehouseId && x.ItemNumber == orderinfo.itemNumber).Select(x => x.CurrentInventory).FirstOrDefault();
                                    if (items.Any(y => y.Orders.Any(z => z.OrderId == item.OrderId && z.qty <= freeitemqty && y.IsFreeItem == true)))
                                    {
                                        item.IsLessCurrentStock = false;
                                        item.isCompleted = true;
                                    }
                                }
                            }
                        }
                    }
                    //else if (items.Any(y => y.Orders.Any(z => z.OrderId == item.OrderId && z.qty > y.TotalAvlQty)))
                    //{
                    //    item.IsLessCurrentStock = true;
                    //    item.isCompleted = false;
                    //}
                    else if (items.All(y => y.Orders.Where(z => z.OrderId >= item.OrderId).Sum(x => x.qty) >= y.TotalAvlQty))
                    {
                        item.isCompleted = true;
                        item.IsLessCurrentStock = false;
                    }
                }
                paggingData.total_count = dataCount;
                paggingData.ordermaster = newdata;
                return paggingData;

            }
            catch (Exception e)
            {
                throw e;
            }
        }
        private List<OrderDetailsPickerDC> GetOrdersFromMongoV2(int ClusterId)
        {
            try
            {
                var identity = User.Identity as ClaimsIdentity;
                int compid = 0;
                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))
                    compid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);
                MongoDbHelper<MongoOrderMaster> mongoDbHelper = new MongoDbHelper<MongoOrderMaster>();
                var orderPredicate = PredicateBuilder.New<MongoOrderMaster>(x => x.ClusterId == ClusterId && x.Status == "ReadyToPick" && !x.Deleted);
                var orderMasters = new List<MongoOrderMaster>();
                orderMasters = mongoDbHelper.Select(orderPredicate, x => x.OrderByDescending(z => z.CreatedDate), collectionName: "OrderMaster").ToList();
                var newdata = orderMasters.Select(x => new OrderDetailsPickerDC
                {
                    OrderId = x.OrderId,
                    CustomerName = x.CustomerName,
                    Skcode = x.Skcode,
                    ShopName = x.ShopName,
                    Status = x.Status,
                    invoice_no = x.invoice_no,
                    BillingAddress = x.BillingAddress,
                    CreatedDate = x.CreatedDate,
                    ReDispatchCount = x.ReDispatchCount,
                    ClusterId = x.ClusterId,
                    ClusterName = x.ClusterName,
                    comments = x.comments,
                    TotalAmount = x.TotalAmount,
                    GrossAmount = x.GrossAmount,
                    DiscountAmount = x.DiscountAmount,
                    UpdatedDate = TimeZoneInfo.ConvertTime(x.UpdatedDate, TimeZoneInfo.FindSystemTimeZoneById("India Standard Time")),

                    orderDetails = x.orderDetails.Select(z => new OrderDetailsDc
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
                        FreeWithParentItemId = z.FreeWithParentItemId
                    }).ToList()
                }).ToList();


                var currentStocks = new List<CurrentStockMinDc>();
                var multimrpIds = newdata.SelectMany(z => z.orderDetails).Select(x => new { x.ItemMultiMRPId, x.WarehouseId, x.IsFreeItem }).Distinct().ToList();
                var Orderitems = newdata.SelectMany(x => x.orderDetails.Select(y => y.ItemId)).ToList();
                var allpendingOrders = mongoDbHelper.Select(x => !x.Deleted && x.Status == "Pending"
                                                         && x.orderDetails.Any(z => Orderitems.Contains(z.ItemId)), collectionName: "OrderMaster");
                allpendingOrders.ForEach(x => x.orderDetails.ForEach(y => y.WarehouseId = x.WarehouseId));
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
                        dr["IsFreeItem"] = item.IsFreeItem;
                        orderIdDt.Rows.Add(dr);
                    }
                    var param = new SqlParameter("Items", orderIdDt);
                    param.SqlDbType = SqlDbType.Structured;
                    param.TypeName = "dbo.itemtype";
                    var cmd = authContext.Database.Connection.CreateCommand();
                    cmd.CommandText = "[dbo].[GetCurrentStock]";
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.Parameters.Add(param);
                    using (var reader = cmd.ExecuteReader())
                    {
                        currentStocks = ((IObjectContextAdapter)authContext)
                                        .ObjectContext
                                        .Translate<CurrentStockMinDc>(reader).ToList();
                    }
                }
                var pendingOrderIds = newdata.Select(z => z.OrderId);
                var allPendingItems = allpendingOrders.SelectMany(x => x.orderDetails).GroupBy(x => new { x.ItemMultiMRPId, x.itemNumber, x.WarehouseId, x.IsFreeItem }).Select(x => new
                {
                    x.Key.itemNumber,
                    x.Key.ItemMultiMRPId,
                    x.Key.WarehouseId,
                    x.Key.IsFreeItem,
                    TotalReqQty = x.Sum(y => y.qty),
                    TotalAvlQty = currentStocks != null && currentStocks.Any(z => z.ItemNumber == x.Key.itemNumber && z.ItemMultiMRPId == x.Key.ItemMultiMRPId && z.IsFreeItem == x.Key.IsFreeItem && z.WarehouseId == x.Key.WarehouseId) ? currentStocks.Where(z => z.ItemNumber == x.Key.itemNumber && z.ItemMultiMRPId == x.Key.ItemMultiMRPId && z.WarehouseId == x.Key.WarehouseId).Sum(y => y.CurrentInventory) : 0,
                    Orders = x.Select(y => new { y.OrderDetailsId, y.OrderId, y.qty }).ToList()
                }).ToList();
                foreach (var item in newdata)
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
                    }
                    else if (items.All(y => y.Orders.Where(z => z.OrderId >= item.OrderId).Sum(x => x.qty) >= y.TotalAvlQty))
                    {
                        item.isCompleted = true;
                        item.IsLessCurrentStock = false;
                    }
                }
                return newdata;
            }
            catch (Exception e)
            {
                throw e;
            }
        }


        [HttpGet]
        [Route("GetPickerList")]
        public List<sendPickertoApp> GetPickerList(int id)
        {
            OrderPickerMaster pickerlist = null;
            List<sendPickertoApp> SendPickertoApp = null;
            using (AuthContext context = new AuthContext())
            {

                if (id > 0)
                {
                    pickerlist = context.OrderPickerMasterDb.Where(x => x.Id == id).Include("OrderPickerDetails").FirstOrDefault();
                    if (pickerlist != null)
                    {
                        var picklist = pickerlist.orderPickerDetails.Where(x => x.Status == 0).GroupBy(x => x.ItemMultiMrpId).Select(t =>
                            new sendPickertoApp
                            {
                                pickerId = pickerlist.Id,
                                Totalqty = t.Sum(x => x.Qty),
                                ItemMultiMRPId = t.Key,
                                SellingUnitName = t.Min(x => x.ItemName),
                                itemname = t.Min(x => x.ItemName),
                                OrderDetailsSPA = pickerlist.orderPickerDetails.Where(x => x.ItemMultiMrpId == t.Key && x.Status == 0).Select(r =>
                                   new OrderDetailsSPA
                                   {
                                       pickerDetailsId = r.id,
                                       pickerId = pickerlist.Id,
                                       ItemMultiMrpId = r.ItemMultiMrpId,
                                       orderid = r.OrderId,
                                       qty = r.Qty,
                                       Comment = r.Comment,
                                       IsFreeItem = r.IsFreeItem,
                                       Status = r.Status,
                                   }).ToList()
                            }).ToList();
                        List<int> mmpi = picklist.Select(x => x.ItemMultiMRPId).ToList();
                        var a = string.Join<int>(",", mmpi);
                        var manager = new PickerManager();
                        List<pikeritemDC> Item = manager.GetItemMRPandBarcode(a);
                        foreach (var aa in picklist)
                        {
                            aa.MRP = Item.Where(x => x.ItemMultiMRPId == aa.ItemMultiMRPId).Select(e => e.MRP).FirstOrDefault();
                            aa.Number = Item.Where(x => x.ItemMultiMRPId == aa.ItemMultiMRPId).Select(e => e.Number).FirstOrDefault();
                            aa.Barcode = null;// Item.Where(x => x.ItemMultiMRPId == aa.ItemMultiMRPId).Select(e => e.Barcode).FirstOrDefault();
                        }
                        SendPickertoApp = picklist;
                    }
                }
            }
            return SendPickertoApp;

        }

        /// <summary>
        /// App
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("AcceptorReject")]
        public async Task<bool> AcceptorReject(AcceptRejectDC acceptRejectDC)
        {
            var result = false;

            if (acceptRejectDC.OrderId > 0 && acceptRejectDC.Status != null && acceptRejectDC.pickerDetailsId > 0)
            {
                OrderPickerDetails pickerlist = null;
                using (AuthContext context = new AuthContext())
                {
                    pickerlist = context.OrderPickerDetailsDb.Where(x => x.id == acceptRejectDC.pickerDetailsId).FirstOrDefault();
                    if (pickerlist != null)
                    {
                        if (acceptRejectDC.Status == "Accept")
                        {
                            pickerlist.Status = 1;
                            pickerlist.Comment = "Accept";
                            pickerlist.ModifiedDate = indianTime;
                            context.Entry(pickerlist).State = EntityState.Modified;  // entity 

                            //Reset/accept zero qty if Old item in reject mode
                            List<OrderPickerDetails> allItems = context.OrderPickerDetailsDb.Where(x => x.OrderId == acceptRejectDC.OrderId && x.id != acceptRejectDC.pickerDetailsId && x.OrderPickerMasterId == pickerlist.OrderPickerMasterId).ToList();
                            if (allItems != null && allItems.Any())
                            {
                                foreach (var item in allItems.Where(z => z.Status == 3 || z.Qty == 0))
                                {
                                    item.Status = (item.Qty == 0) ? 1 : 0;
                                    item.Comment = (item.Qty == 0) ? "Accept with zero qty" : "Reset";
                                    item.ModifiedDate = indianTime;
                                    context.Entry(item).State = EntityState.Modified;
                                }
                            }

                            result = context.Commit() > 0;
                        }
                        else if (acceptRejectDC.Status == "Reject")
                        {

                            var pickerpersonid = context.OrderPickerMasterDb.FirstOrDefault(x => x.Id == pickerlist.OrderPickerMasterId).PickerPersonId;

                            List<OrderPickerDetails> RejectallItemofThatorder = context.OrderPickerDetailsDb.Where(x => x.OrderId == acceptRejectDC.OrderId && x.OrderPickerMasterId == pickerlist.OrderPickerMasterId).ToList();
                            if (RejectallItemofThatorder != null && RejectallItemofThatorder.Any())
                            {
                                foreach (var item in RejectallItemofThatorder)
                                {
                                    item.Status = 3;
                                    item.ModifiedBy = pickerpersonid; //reject
                                    item.ModifiedDate = indianTime;
                                    item.Comment = item.id == acceptRejectDC.pickerDetailsId ? acceptRejectDC.comment : "";
                                    context.Entry(item).State = EntityState.Modified;  // entity 
                                }
                            }
                            result = context.Commit() > 0;
                        }
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// App
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("GetReviewerOrderPickerMaster")]
        public async Task<List<PickerJobListDc>> GetReviewerOrderPicker(int PeopleId)
        {
            List<PickerJobListDc> pickerJobList = new List<PickerJobListDc>();
            if (PeopleId > 0)
            {
                using (AuthContext context = new AuthContext())
                {
                    var wareouseid = await context.Peoples.Where(x => x.PeopleID == PeopleId && x.Active && !x.Deleted).Select(x => x.WarehouseId).FirstOrDefaultAsync();
                    if (wareouseid > 0)
                    {
                        List<OrderPickerMaster> pickerlist = await context.OrderPickerMasterDb.Where(x => x.WarehouseId == wareouseid && x.IsCheckerGrabbed == true && x.ApproverId == PeopleId && x.IsComplted == true && x.IsCanceled == false && x.IsDeleted == false && x.IsDispatched == false && x.IsApproved == false).OrderBy(x => x.Id).Include("OrderPickerDetails").ToListAsync();

                        if (pickerlist != null && pickerlist.Any())
                        {

                            var createdBy = pickerlist.Select(x => x.CreatedBy).Distinct().ToList();
                            var submittedBy = pickerlist.Select(x => x.PickerPersonId).Distinct().ToList();
                            var dboyid = pickerlist.Select(x => x.DBoyId).Distinct().ToList();
                            var peopleIds = createdBy.Concat(submittedBy);

                            var peoplename = await context.Peoples.Where(x => peopleIds.Contains(x.PeopleID) || dboyid.Contains(x.PeopleID)).Distinct().ToListAsync();
                            var Pickerids = pickerlist.Select(x => x.Id).Distinct().ToList();

                            List<PickerTimer> PickerTimerList = await context.PickerTimerDb.Where(x => Pickerids.Contains(x.OrderPickerMasterId) && x.Type == 1 && x.EndTime == null).ToListAsync();
                            foreach (var a in pickerlist)
                            {
                                double amount = 0;
                                var orderids = a.orderPickerDetails.Where(y => y.Status != 3).Select(y => y.OrderId).Distinct().ToList();
                                var cluster = await context.DbOrderMaster.Where(x => orderids.Contains(x.OrderId)).Select(x => x.ClusterName).Distinct().ToListAsync();
                                var dboyname = peoplename.Where(x => x.PeopleID == a.DBoyId).FirstOrDefault()?.DisplayName;
                                var orderDetails = a.orderPickerDetails.Where(x => x.Status != 3).Select(x => new { x.OrderDetailsId, x.Qty }).ToList();
                                var orderdetailIds = orderDetails.Select(x => x.OrderDetailsId).ToList();
                                var dbOrderDetails = await context.DbOrderDetails.Where(x => orderdetailIds.Contains(x.OrderDetailsId)).Select(x => new { x.OrderDetailsId, x.UnitPrice }).ToListAsync();
                                dbOrderDetails.ForEach(x =>
                                {
                                    amount += orderDetails.Where(y => y.OrderDetailsId == x.OrderDetailsId).Sum(y => y.Qty * x.UnitPrice);
                                });

                                var aaa = a.orderPickerDetails.GroupBy(x => x.ItemMultiMrpId).Count();
                                PickerJobListDc PickerJobListob = new PickerJobListDc();
                                PickerJobListob.Id = a.Id;
                                PickerJobListob.CreatedDate = a.CreatedDate;
                                PickerJobListob.count = aaa;
                                PickerJobListob.IsApproved = a.IsApproved;
                                PickerJobListob.Commenet = a.Comment;
                                PickerJobListob.IsComplted = a.IsComplted;
                                PickerJobListob.CreatedBy = peoplename.Where(x => x.PeopleID == a.CreatedBy).Select(x => x.DisplayName).FirstOrDefault();
                                PickerJobListob.SubmittedBy = peoplename.Where(x => x.PeopleID == a.PickerPersonId).Select(x => x.DisplayName).FirstOrDefault();
                                PickerJobListob.SubmittedDate = a.SubmittedDate;
                                PickerJobListob.amount = Math.Round(amount, 0);
                                PickerJobListob.dboyName = dboyname == null ? "" : dboyname;
                                PickerJobListob.ClusterName = string.Join(",", cluster);
                                PickerJobListob.IsInventorySupervisor = a.IsInventorySupervisorStart;
                                PickerJobListob.IsInventorySupervisorStart = a.IsInventorySupervisor;
                                if (PickerTimerList != null && PickerTimerList.Any() && PickerTimerList.Any(x => x.OrderPickerMasterId == a.Id && x.EndTime == null))
                                {
                                    PickerJobListob.StartTime = PickerTimerList.FirstOrDefault(x => x.OrderPickerMasterId == a.Id && x.EndTime == null).StartTime;
                                }
                                else
                                {
                                    PickerJobListob.StartTime = null;
                                }
                                PickerJobListob.EndTime = null;
                                if (a.Status == 2 && a.IsComplted == true && a.IsDispatched == false)
                                {
                                    PickerJobListob.Status = "New";
                                }
                                else if (a.Status == 1 && a.IsComplted == true && a.IsDispatched == false)
                                {
                                    PickerJobListob.Status = "Inprogress";
                                }
                                else if (a.Status == 5 && a.IsComplted == true && a.IsDispatched == false)
                                {
                                    PickerJobListob.Status = "RePicking";
                                }
                                else if (a.IsDispatched)
                                {
                                    PickerJobListob.Status = "Assignment Created";
                                }
                                pickerJobList.Add(PickerJobListob);
                            }
                        }
                    }
                }
            }
            return pickerJobList;

        }

        [HttpGet]
        [Route("GetReviewerOrderPickerDetail")]
        public async Task<List<sendPickertoApp>> GetReviewerOrderPickerDetail(int id)
        {
            //OrderPickerMaster pickerlist = null;
            List<sendPickertoApp> SendPickertoApps = null;
            List<OrderPickerDetailDTO> pickerlist = new List<OrderPickerDetailDTO>();
            using (AuthContext context = new AuthContext())
            {
                if (id > 0)
                {
                    var orderPickerMasterIdParam = new SqlParameter("@OrderPickerMasterId", id);
                    pickerlist = context.Database.SqlQuery<OrderPickerDetailDTO>("Picker.GetPickerJobListDetails @OrderPickerMasterId", orderPickerMasterIdParam).ToList();
                    if (pickerlist != null)
                    {

                        var picklist = pickerlist.Where(x => x.Qty > 0).GroupBy(x => new { x.ItemMultiMrpId, x.IsClearance }).Select(t =>
                        new sendPickertoApp
                        {
                            pickerId = pickerlist.FirstOrDefault().OrderPickerMasterId,
                            Totalqty = t.Sum(x => x.Qty),
                            ItemMultiMRPId = t.Key.ItemMultiMrpId,
                            SellingUnitName = t.Min(x => x.ItemName),
                            itemname = t.Min(x => x.ItemName),
                            IsClearance = t.Key.IsClearance,
                            OrderDetailsSPA = pickerlist.Where(x => x.ItemMultiMrpId == t.Key.ItemMultiMrpId && x.IsClearance == t.Key.IsClearance && x.Qty > 0).Select(r =>
                                 new OrderDetailsSPA
                                 {
                                     pickerDetailsId = r.id,
                                     pickerId = pickerlist.FirstOrDefault().OrderPickerMasterId,
                                     ItemMultiMrpId = r.ItemMultiMrpId,
                                     orderid = r.OrderId,
                                     qty = r.Qty,
                                     Comment = r.Comment,
                                     IsFreeItem = r.IsFreeItem,
                                     Status = r.Status,
                                 }).ToList()
                        }).ToList();

                        //var picklist = pickerlist.orderPickerDetails.Where(x => x.Status > 0 && x.Qty > 0 && x.Status != 3 ).GroupBy(x => x.ItemMultiMrpId).Select(t =>
                        //new sendPickertoApp
                        //{

                        //    pickerId = pickerlist.Id,
                        //    Totalqty = t.Sum(x => x.Qty),
                        //    ItemMultiMRPId = t.Key,
                        //    SellingUnitName = t.Min(x => x.ItemName),
                        //    itemname = t.Min(x => x.ItemName),

                        //    OrderDetailsSPA = pickerlist.orderPickerDetails.Where(x => x.ItemMultiMrpId == t.Key && x.Qty > 0 && x.Status > 0 && x.Status != 3).Select(r =>
                        //        new OrderDetailsSPA
                        //        {
                        //            pickerDetailsId = r.id,
                        //            pickerId = pickerlist.Id,
                        //            ItemMultiMrpId = r.ItemMultiMrpId,
                        //            orderid = r.OrderId,
                        //            qty = r.Qty,
                        //            Comment = r.Comment,

                        //            IsFreeItem = r.IsFreeItem,
                        //            Status = r.Status,
                        //        }).ToList()
                        //}).ToList();

                        if (picklist != null && picklist.Any())
                        {
                            List<int> mmpi = picklist.Select(x => x.ItemMultiMRPId).ToList();
                            var a = string.Join<int>(",", mmpi);
                            var manager = new PickerManager();
                            List<pikeritemDC> Item = manager.GetItemMRPandBarcode(a);
                            foreach (var aa in picklist)
                            {
                                aa.MRP = Item.Where(x => x.ItemMultiMRPId == aa.ItemMultiMRPId).Select(e => e.MRP).FirstOrDefault();
                                aa.Number = Item.Where(x => x.ItemMultiMRPId == aa.ItemMultiMRPId).Select(e => e.Number).FirstOrDefault();
                            }
                            var itemNumbers = picklist.Select(x => x.Number).Distinct().ToList();
                            var itembarcodelist = context.ItemBarcodes.Where(c => itemNumbers.Contains(c.ItemNumber) && c.IsActive == true && c.IsDeleted == false).Select(x => new ItemBarcodeDc { ItemNumber = x.ItemNumber, Barcode = x.Barcode }).ToList();
                            foreach (var item in picklist)
                            {
                                item.Barcode = (itembarcodelist != null && itembarcodelist.Any(x => x.ItemNumber == item.Number)) ? itembarcodelist.Where(x => x.ItemNumber == item.Number).Select(x => x.Barcode).ToList() : null;

                            }
                            SendPickertoApps = picklist.OrderByDescending(x => x.itemname).ToList();
                        }
                    }
                }

            }

            return SendPickertoApps;
        }

        [HttpGet]
        [Route("GetPickerMRPOrderDetails")]
        public async Task<List<OrderPickerDetailDC>> GetPickerMRPOrderDetails(int pickerId, int ItemMultiMRPId, bool IsClearance)
        {
            List<OrderPickerDetailDC> result = new List<OrderPickerDetailDC>();
            using (AuthContext context = new AuthContext())
            {
                var manager = new PickerManager();
                var response = await manager.GetPickerMRPOrderDetails(pickerId, ItemMultiMRPId, IsClearance);
                foreach (var item in response.OrderPickerDetailDCs)
                {
                    result.Add(new OrderPickerDetailDC
                    {
                        ItemMultiMrpId = item.ItemMultiMrpId,
                        Number = item.Number,
                        Barcode = item.Barcode,
                        IsFreeItem = item.IsFreeItem,
                        IsRTD = item.IsRTD,
                        MRP = item.MRP,
                        OrderId = item.OrderId,
                        OrderPickerMasterId = item.OrderPickerMasterId,
                        OrderPickerDetailBatchs = (response.OrderPickerDetailBatchDcs != null && response.OrderPickerDetailBatchDcs.Any()) ? response.OrderPickerDetailBatchDcs.Where(c => c.pickerDetailsId == item.pickerDetailsId).ToList() : null,
                        Comment = item.Comment,
                        pickerDetailsId = item.pickerDetailsId,
                        Qty = item.Qty,
                        Status = item.Status,
                        ItemName = item.ItemName,
                        isClearance = item.isClearance
                    });
                }
            }
            return result;
        }

        [HttpGet]
        [Route("GetPickerReviewerMRPOrderDetails")]
        public async Task<List<OrderPickerDetailDC>> GetPickerReviewerMRPOrderDetails(int pickerId, int ItemMultiMRPId, bool IsClearance)
        {
            List<OrderPickerDetailDC> result = new List<OrderPickerDetailDC>();
            using (AuthContext context = new AuthContext())
            {
                var manager = new PickerManager();
                var response = await manager.GetPickerReviewerMRPOrderDetails(pickerId, ItemMultiMRPId, IsClearance);
                foreach (var item in response.OrderPickerDetailDCs)
                {
                    result.Add(new OrderPickerDetailDC
                    {
                        ItemMultiMrpId = item.ItemMultiMrpId,
                        Number = item.Number,
                        Barcode = item.Barcode,
                        IsFreeItem = item.IsFreeItem,
                        IsRTD = item.IsRTD,
                        MRP = item.MRP,
                        OrderId = item.OrderId,
                        OrderPickerMasterId = item.OrderPickerMasterId,
                        OrderPickerDetailBatchs = (response.OrderPickerDetailBatchDcs != null && response.OrderPickerDetailBatchDcs.Any()) ? response.OrderPickerDetailBatchDcs.Where(c => c.pickerDetailsId == item.pickerDetailsId).ToList() : null,
                        Comment = item.Comment,
                        pickerDetailsId = item.pickerDetailsId,
                        Qty = item.Qty,
                        Status = item.Status,
                        ItemName = item.ItemName,
                        isClearance = item.isClearance
                    });
                }
            }
            return result;
        }

        [HttpPost]
        [Route("ReviwerAcceptReject")]
        public async Task<bool> ReviwerAcceptReject(AcceptRejectDC acceptRejectDC)
        {
            var result = false;
            if (acceptRejectDC.OrderId > 0 && acceptRejectDC.Status != null)
            {
                OrderPickerDetails pickerlist = null;

                using (AuthContext context = new AuthContext())
                {
                    pickerlist = context.OrderPickerDetailsDb.FirstOrDefault(x => x.OrderId == acceptRejectDC.OrderId && x.id == acceptRejectDC.pickerDetailsId);
                    if (pickerlist != null)
                    {
                        if (acceptRejectDC.Status == "Accept")
                        {
                            pickerlist.Status = 2;
                            pickerlist.Comment = "Reviwer Accept";
                            pickerlist.ModifiedDate = indianTime;
                            context.Entry(pickerlist).State = EntityState.Modified;  // entity 

                            //Reset/accept zero qty if Old item in reject mode
                            List<OrderPickerDetails> allItems = context.OrderPickerDetailsDb.Where(x => x.OrderId == acceptRejectDC.OrderId && x.id != acceptRejectDC.pickerDetailsId && x.id != acceptRejectDC.pickerDetailsId && x.OrderPickerMasterId == pickerlist.OrderPickerMasterId).ToList();
                            if (allItems != null && allItems.Any())
                            {
                                foreach (var item in allItems.Where(z => z.Qty == 0))
                                {
                                    item.Status = 2;
                                    item.Comment = " Reviwer Accept with zero qty";
                                    item.ModifiedDate = indianTime;
                                    context.Entry(item).State = EntityState.Modified;
                                }
                            }
                            result = context.Commit() > 0;
                        }
                        else if (acceptRejectDC.Status == "Reject")
                        {
                            List<OrderPickerDetails> RejectallItemofThatorder = context.OrderPickerDetailsDb.Where(x => x.OrderId == acceptRejectDC.OrderId && x.OrderPickerMasterId == pickerlist.OrderPickerMasterId).ToList();
                            foreach (var item in RejectallItemofThatorder)
                            {
                                item.Status = 4;
                                item.Comment = acceptRejectDC.comment;
                                item.ModifiedDate = indianTime;
                                context.Entry(item).State = EntityState.Modified;  // entity 
                            }
                            result = context.Commit() > 0;
                        }
                    }
                }
            }
            return result;
        }


        [HttpGet]
        [Route("GetAcceptedOrderDetails")]
        public async Task<List<OrderPickerDetails>> GetAcceptedOrderDetails(int pickerId, int OrderId)
        {
            List<OrderPickerDetails> SendPickertoApp = null;
            using (AuthContext context = new AuthContext())
            {
                if (pickerId > 0)
                {
                    SendPickertoApp = context.OrderPickerDetailsDb.Where(x => x.OrderPickerMasterId == pickerId && x.OrderId == OrderId && x.Status == 1).ToList();
                }
            }
            return SendPickertoApp;
        }

        //app  GetReviwerRejectedPOrderDetails
        //only for revierw case
        [HttpGet]
        [Route("GetReviwerRejectedOrderDetails")]
        public async Task<List<sendPickertoApp>> GetRejectedPOrderDetails(int pickerId)
        {
            OrderPickerMaster pickerlist = null;
            List<sendPickertoApp> SendPickertoApp = new List<sendPickertoApp>();
            using (AuthContext context = new AuthContext())
            {

                if (pickerId > 0)
                {
                    pickerlist = context.OrderPickerMasterDb.Where(x => x.Id == pickerId).Include("OrderPickerDetails").FirstOrDefault();
                    if (pickerlist != null)
                    {

                        var picklist = pickerlist.orderPickerDetails.Where(x => x.Status == 4 || x.Status == 2).GroupBy(x => x.ItemMultiMrpId).Select(t =>
                            new sendPickertoApp
                            {
                                pickerId = pickerlist.Id,
                                Totalqty = t.Sum(x => x.Qty),
                                ItemMultiMRPId = t.Key,
                                SellingUnitName = t.Min(x => x.ItemName),
                                itemname = t.Min(x => x.ItemName),
                                OrderDetailsSPA = pickerlist.orderPickerDetails.Where(x => x.ItemMultiMrpId == t.Key && (x.Status == 4 || x.Status == 2)).Select(r =>
                                     new OrderDetailsSPA
                                     {
                                         pickerDetailsId = r.id,
                                         pickerId = pickerlist.Id,
                                         ItemMultiMrpId = r.ItemMultiMrpId,
                                         orderid = r.OrderId,
                                         qty = r.Qty,
                                         Comment = r.Comment,
                                         IsFreeItem = r.IsFreeItem,
                                         Status = r.Status,
                                     }).ToList()
                            }).ToList();

                        if (picklist != null && picklist.Any())
                        {
                            List<int> mmpi = picklist.Select(x => x.ItemMultiMRPId).ToList();
                            var a = string.Join<int>(",", mmpi);
                            var manager = new PickerManager();
                            List<pikeritemDC> Item = manager.GetItemMRPandBarcode(a);
                            foreach (var aa in picklist)
                            {
                                aa.MRP = Item.Where(x => x.ItemMultiMRPId == aa.ItemMultiMRPId).Select(e => e.MRP).FirstOrDefault();
                                aa.Number = Item.Where(x => x.ItemMultiMRPId == aa.ItemMultiMRPId).Select(e => e.Number).FirstOrDefault();
                                aa.Barcode = null; //Item.Where(x => x.ItemMultiMRPId == aa.ItemMultiMRPId).Select(e => e.Barcode).FirstOrDefault();
                            }
                            SendPickertoApp = picklist;
                        }
                        else
                        {
                            var Pickertimer = context.PickerTimerDb.Where(x => x.OrderPickerMasterId == pickerlist.Id && x.EndTime == null && x.Type == 0).FirstOrDefault();
                            if (Pickertimer != null)
                            {
                                Pickertimer.EndTime = indianTime;
                                context.Entry(Pickertimer).State = EntityState.Modified;
                                context.Commit();
                            }
                        }
                    }
                }
            }
            return SendPickertoApp;

        }



        [HttpGet]
        [Route("SearchByPickerId")]
        public async Task<List<OrderPickerMasterDc>> SearchByItemMultiMrpId(int PickerId)
        {
            var data = new List<OrderPickerMasterDc>();

            using (AuthContext db = new AuthContext())
            {
                List<OrderPickerMaster> newdata = new List<OrderPickerMaster>();
                var predicate = PredicateBuilder.New<OrderPickerMaster>(x => x.Id == PickerId && x.IsDeleted == false);
                List<OrderPickerMaster> pickerlist = db.OrderPickerMasterDb.Where(predicate).OrderByDescending(x => x.Id).Include("OrderPickerDetails").ToList();

                //string sqlquery = "select * from OrderPickerMasters where Id like'%" + PickerId + "%';";
                //List<OrderPickerMaster> newdata = db.Database.SqlQuery<OrderPickerMaster>(sqlquery).OrderByDescending(x => x.CreatedDate).ToList();
                var cluster = pickerlist.Select(x => x.ClusterId).Distinct().ToList();
                var Clustername = db.Clusters.Where(x => cluster.Contains(x.ClusterId)).ToList();
                var peoplecreatedby = pickerlist.Select(x => x.CreatedBy).Distinct().ToList();
                var pickerid = pickerlist.Select(x => x.PickerPersonId).Distinct().ToList();
                var DboyId = pickerlist.Select(x => x.DBoyId).Distinct().ToList();
                var agentId = pickerlist.Select(x => x.AgentId).Distinct().ToList();
                var inventorySupervisorId = pickerlist.Select(x => x.InventorySupervisorId).Distinct().ToList();
                var people = db.Peoples.Where(x =>
                   peoplecreatedby.Contains(x.PeopleID)
                || pickerid.Contains(x.PeopleID)
                || DboyId.Contains(x.PeopleID)
                || agentId.Contains(x.PeopleID)
                || inventorySupervisorId.Contains(x.PeopleID)
                ).ToList();
                data = Mapper.Map(pickerlist).ToANew<List<OrderPickerMasterDc>>();
                var OrderId = data.SelectMany(x => x.orderPickerDetails.Select(y => y.OrderId).Distinct()).ToList();
                //  var AssingmentOrder = db.OrderDispatchedMasters.Where(x => OrderId.Contains(x.OrderId)).Select(x => new { x.DeliveryIssuanceIdOrderDeliveryMaster, x.OrderId }).ToList();
                foreach (var item in data)
                {
                    item.PickerPersonName = people.Where(x => x.PeopleID == item.PickerPersonId).Select(x => x.DisplayName).FirstOrDefault();
                    if (item.Status == 0 && item.IsCanceled == false)
                    {
                        item.CurrentStatus = "New";
                    }
                    else if (item.Status == 0 && item.IsCanceled == true)
                    {
                        item.CurrentStatus = "Cancelled";
                    }
                    else if (item.Status == 1)
                    {
                        item.CurrentStatus = "InProgress";
                    }
                    else if (item.Status == 2)
                    {
                        item.CurrentStatus = "Submitted";
                    }
                    else if (item.Status == 3)
                    {
                        item.CurrentStatus = "Approved(RTD Done)";
                    }
                    else if (item.Status == 4)
                    {
                        item.CurrentStatus = "Canceled";
                    }
                    else if (item.Status == 5)
                    {
                        item.CurrentStatus = "RePicking";
                    }
                    else if (item.Status == 6)
                    {
                        item.CurrentStatus = "Assignment Created";
                    }
                    foreach (var item1 in pickerlist.Where(x => x.Id == item.Id).Select(x => x.orderPickerDetails))
                    {
                        var orderIds = item1.Select(x => x.OrderId).Distinct().ToList();
                        //if (item.DeliveryIssuanceId == null)
                        //{
                        //    int? assingmentId = AssingmentOrder.Where(x => orderIds.Contains(x.OrderId)).Select(x => x.DeliveryIssuanceIdOrderDeliveryMaster).FirstOrDefault() ?? null;
                        //    if (assingmentId != null)
                        //    {
                        //        item.DeliveryIssuanceId = assingmentId;
                        //    }
                        //}
                        item.DeliveryIssuanceId = item.DeliveryIssuanceId;
                        item.orderpickercount = item1.Select(x => x.OrderId).Distinct().ToList().Count();
                        item.orderpickercountitem += item1.Select(x => x.Qty).Sum();
                        List<int> mmpi = item1.Select(x => x.ItemMultiMrpId).Distinct().ToList();
                        var a = string.Join<int>(",", mmpi);
                        var manager = new PickerManager();
                        List<pikeritemDC> Items = manager.GetItemMRPandBarcode(a);
                        item.ClusterName = Clustername.Where(x => x.ClusterId == item.ClusterId).Select(x => x.ClusterName).FirstOrDefault();
                        item.AgentName = people.Where(x => x.PeopleID == item.AgentId).Select(x => x.DisplayName).FirstOrDefault();
                        item.DBoyName = people.Where(x => x.PeopleID == item.DboyId).Select(x => x.DisplayName).FirstOrDefault();
                        item.DboyMobile = people.Where(x => x.PeopleID == item.DboyId).Select(x => x.Mobile).FirstOrDefault();
                        item.InventorySupervisorName = item.InventorySupervisorId == null ? "" : people.Where(x => x.PeopleID == item.InventorySupervisorId).Select(x => x.DisplayName).FirstOrDefault();
                        var AddDatalists = item.orderPickerDetails.Where(z => z.Qty > 0).GroupBy(x => new { x.ItemMultiMrpId, x.Status }).Select(x => new OrderPickerDetailsDc
                        {

                            MRP = Items.Where(t => t.ItemMultiMRPId == x.Key.ItemMultiMrpId).Select(e => e.MRP).FirstOrDefault(),
                            id = x.Select(w => w.id).FirstOrDefault(),
                            ItemMultiMrpId = x.Key.ItemMultiMrpId,
                            Qty = x.Sum(y => y.Qty),
                            ItemName = x.Select(w => w.ItemName).FirstOrDefault(),
                            OrderId = x.Select(w => w.OrderId).FirstOrDefault(),
                            OrderDetailsId = x.Select(w => w.OrderDetailsId).FirstOrDefault(),
                            DispatchedQty = x.Select(w => w.DispatchedQty).FirstOrDefault(),
                            Comment = x.Select(w => w.Comment).FirstOrDefault(),
                            IsFreeItem = x.Select(w => w.IsFreeItem).FirstOrDefault(),
                            Status = x.Select(w => w.Status).FirstOrDefault(),
                            Orderids = string.Join(",", item.orderPickerDetails.Where(z => z.ItemMultiMrpId == x.Key.ItemMultiMrpId && z.Status == x.Key.Status).Select(e => e.OrderId).ToList()),
                        }).ToList();

                        item.orderPickerDetails = AddDatalists.OrderBy(x => x.ItemMultiMrpId).ToList();
                    }
                }
                return data;
            }
        }

        [HttpGet]
        [Route("PickerListA7")]
        public async Task<List<OrderPickerMasterDc>> PickerListA7()
        {
            using (AuthContext context = new AuthContext())
            {
                List<OrderPickerMaster> pickerList = new List<OrderPickerMaster>();
                var predicate = PredicateBuilder.New<OrderPickerMaster>(x => x.IsDeleted == false);
                List<OrderPickerMaster> pickerlist = context.OrderPickerMasterDb.Where(predicate).OrderByDescending(x => x.Id).Include("OrderPickerDetails").ToList();
                var people = pickerlist.Select(x => x.PickerPersonId).Distinct().ToList();
                var peoplename = context.Peoples.Where(x => people.Contains(x.PeopleID)).ToList();
                var newsd = new List<OrderPickerMasterDc>();
                newsd = Mapper.Map(pickerlist).ToANew<List<OrderPickerMasterDc>>();
                foreach (var item in newsd)
                {
                    var MultiMrpIds = item.orderPickerDetails.Select(x => x.ItemMultiMrpId).Distinct().ToList();
                    var MultiMrpList = context.ItemMultiMRPDB.Where(x => MultiMrpIds.Contains(x.ItemMultiMRPId)).Distinct().ToList();
                    foreach (var lineitems in item.orderPickerDetails)
                    {
                        lineitems.MRP = MultiMrpList.FirstOrDefault(x => x.ItemMultiMRPId == lineitems.ItemMultiMrpId).MRP;
                        if (lineitems.Status == 4 && item.IsComplted == true)
                        {
                            item.Status = 5;
                        }
                    }
                    item.PickerPersonName = peoplename.Where(x => x.PeopleID == item.PickerPersonId).Select(x => x.DisplayName).FirstOrDefault();
                    item.orderpickercount = item.orderPickerDetails.Select(x => x.OrderId).Distinct().ToList().Count();
                    item.orderpickercountitem += item.orderPickerDetails.Select(x => x.Qty).Sum();
                }
                return newsd;
            }
        }


        // This request to cancel  picker request and then all inventory revert from planeedstock to currentstock
        //But only all item in request case 
        [Route("PickerProcessCanceled")]
        [HttpPost]
        public async Task<string> PickerProcessCanceled(int pickerId, int UserId, string comment)
        {
            var Msg = "";
            bool IsValidate = false;
            if (pickerId > 0 && UserId > 0)
            {
                if (PickerProcessIds.Any(x => x == pickerId))
                {

                    Msg = "Picker #: " + pickerId + " is already in process..";
                    return Msg;
                }
                else
                {
                    PickerProcessIds.Add(pickerId);
                }
                var OrderPickerMaster = new OrderPickerMaster();
                using (var authContext = new AuthContext())
                {
                    OrderPickerMaster = authContext.OrderPickerMasterDb.Where(x => x.Id == pickerId && x.IsApproved == false && x.IsCanceled == false && x.IsDispatched == false && x.IsDeleted == false).Include(x => x.orderPickerDetails).FirstOrDefault();

                    var warehouse = authContext.Warehouses.FirstOrDefault(x => x.WarehouseId == OrderPickerMaster.WarehouseId);

                    if (warehouse.IsStopCurrentStockTrans)
                        return "Inventory Transactions are currently disabled for this warehouse... Please try after some time";

                    if (OrderPickerMaster != null && OrderPickerMaster.orderPickerDetails.Any(x => x.Status == 2))
                    {
                        IsValidate = true;
                        Msg = "You can't cancel Picker request due some of item in accept /approval stage";
                    }
                    else if (OrderPickerMaster == null)
                    {
                        IsValidate = true;
                        Msg = "Picker request not eligible to canceled";
                    }
                    else { }
                }
                if (!IsValidate)
                {
                    ReadyToPickDispatchedHelper helper = new ReadyToPickDispatchedHelper();
                    Msg = helper.DOPickerProcessCanceled(pickerId, UserId, comment);
                }
            }
            PickerProcessIds.RemoveAll(x => x == pickerId);

            return Msg;
        }

        [HttpGet]
        [Route("RemovePickerId")]
        [AllowAnonymous]
        public bool RemovePickerId(int PickerId)
        {
            bool status = false;
            if (PickerProcessIds.Any(x => x == PickerId))
            {
                PickerProcessIds.RemoveAll(x => x == PickerId);
                status = true;
            }
            return status;
        }
        //app Reviwer approved
        [HttpPost]
        [Route("ReviewerApproved")]
        [AllowAnonymous]
        public async Task<HttpResponseMessage> ReviewerApproved(ReviewerApprovedDc ReviewerApprovedAndDispatchedobj)
        {
            OrderPickerMaster pickerMaster = new OrderPickerMaster();
            ResMsg res;
            string Msg = "";
            bool status = true;
            People user = null;
            //if (PickerProcessIds.Any(x => x == ReviewerApprovedAndDispatchedobj.PickerId))
            //{
            //    res = new ResMsg()
            //    {
            //        Message = "Picker #: " + ReviewerApprovedAndDispatchedobj.PickerId + " is already in process..",
            //        Status = false
            //    };
            //    return Request.CreateResponse(HttpStatusCode.OK, res);
            //}
            //else
            //{
            //    PickerProcessIds.Add(ReviewerApprovedAndDispatchedobj.PickerId);
            //}
            if (ReviewerApprovedAndDispatchedobj.PickerId > 0 && ReviewerApprovedAndDispatchedobj.UserId > 0 && ReviewerApprovedAndDispatchedobj.PickerId > 0)
            {
                using (AuthContext context = new AuthContext())
                {
                    List<OrderDispatchedMaster> OrderDispatch = null;
                    user = context.Peoples.FirstOrDefault(x => x.PeopleID == ReviewerApprovedAndDispatchedobj.UserId && x.Active);
                    pickerMaster = context.OrderPickerMasterDb.Where(x => x.Id == ReviewerApprovedAndDispatchedobj.PickerId && x.IsDeleted == false).Include(c => c.orderPickerDetails).FirstOrDefault();
                    var warehouse = context.Warehouses.FirstOrDefault(x => x.WarehouseId == pickerMaster.WarehouseId);
                    if (warehouse.IsStopCurrentStockTrans)
                    {
                        var response = new ResMsg()
                        {
                            Status = false,
                            Message = "Inventory Transactions are currently disabled for this warehouse... Please try after some time"
                        };

                        return Request.CreateResponse(HttpStatusCode.OK, response);
                    }
                    if (pickerMaster != null && pickerMaster.IsComplted && user != null)
                    {
                        if (pickerMaster.IsDispatched)
                        {
                            Msg = "Picker is already Approved";
                            status = false;
                        }
                        else if (pickerMaster.IsApproved)
                        {
                            Msg = "Picker is already Approved";
                            status = false;
                        }
                        else if (pickerMaster.IsCanceled)
                        {
                            Msg = "Picker is Cancelled";
                            status = false;
                        }
                        else if (!pickerMaster.IsInventory)
                        {
                            Msg = "Picker is not verifed by inventory super wiser";
                            status = false;
                        }
                        bool IsCompleted = pickerMaster.orderPickerDetails.Any(x => x.Status == 4);
                        if (IsCompleted)
                        {
                            Msg = "Picker List Still in Pending, you can't Approved";
                            status = false;
                        }
                        if (status)
                        {
                            var orderids = pickerMaster.orderPickerDetails.Where(x => x.Status == 2).Select(x => x.OrderId).Distinct().ToList();

                            //&& (x.Status == "ReadyToPick" || x.Status == "Ready to Dispatch")
                            var orderLists = await context.DbOrderMaster.Where(x => orderids.Contains(x.OrderId) && (x.Status == "ReadyToPick" || x.Status == "Ready to Dispatch")).Include(x => x.orderDetails).ToListAsync();
                            if (orderLists.Count == 0)
                            {
                                Msg = " There is no order are eligible to Approved";
                                status = false;
                            }

                            if (orderLists != null && orderLists.Any())
                            {
                                OrderDispatch = Mapper.Map(orderLists).ToANew<List<OrderDispatchedMaster>>();
                                if (OrderDispatch != null && OrderDispatch.Any())
                                {
                                    var customerIds = OrderDispatch.Select(x => x.CustomerId).Distinct().ToList();
                                    DataTable dt = new DataTable();
                                    dt.Columns.Add("IntValue");
                                    foreach (var item in customerIds)
                                    {
                                        var dr = dt.NewRow();
                                        dr["IntValue"] = item;
                                        dt.Rows.Add(dr);
                                    }
                                    var param = new SqlParameter("customerids", dt);
                                    param.SqlDbType = System.Data.SqlDbType.Structured;
                                    param.TypeName = "dbo.IntValues";
                                    var PickerCustomerList = context.Database.SqlQuery<PickerCustomerDc>("Exec Picker.GetCustomersByIds  @customerids", param).ToList();

                                    if (PickerCustomerList.Any(x => x.IsGstRequestPending))
                                    {
                                        var skCodes = PickerCustomerList.Where(x => x.IsGstRequestPending).Select(x => x.Skcode).Distinct().ToList();
                                        var erroresult = "Customer (" + string.Join(",", skCodes) + ") GST Reqest is Inprogress. Please coordinate with customer care.";
                                        Msg = erroresult;
                                        status = false;
                                        res = new ResMsg()
                                        {
                                            Status = status,
                                            Message = Msg
                                        };
                                        return Request.CreateResponse(HttpStatusCode.OK, res);
                                    }
                                    var param1 = new SqlParameter("PickerId", pickerMaster.Id);
                                    long? TripMasterId = context.Database.SqlQuery<long?>("exec operation.IsTripPicker @PickerId", param1).FirstOrDefault();
                                    People Dboyinfo = null;
                                    if (TripMasterId > 0)
                                    {
                                        Dboyinfo = context.Peoples.Where(x => x.PeopleID == pickerMaster.DBoyId && x.Active).FirstOrDefault();
                                        if (Dboyinfo == null)
                                        {
                                            Msg = "Trip dboy is inactive";
                                            status = false;
                                            res = new ResMsg()
                                            {
                                                Status = status,
                                                Message = Msg
                                            };
                                            return Request.CreateResponse(HttpStatusCode.OK, res);
                                        }
                                    }
                                    ReadyToPickDispatchedHelper helper = new ReadyToPickDispatchedHelper();
                                    Msg = helper.ReadyToPickDispatchedNEWAsync(OrderDispatch, ReviewerApprovedAndDispatchedobj.UserId, pickerMaster.Id, warehouse, user, TripMasterId, Dboyinfo, PickerCustomerList);
                                    status = true;
                                }
                            }

                        }
                    }
                    else
                    {
                        Msg = "Picker List Still in Pending, you can't Approved";
                        status = false;
                    }
                }
            }
            //PickerProcessIds.RemoveAll(x => x == ReviewerApprovedAndDispatchedobj.PickerId);
            res = new ResMsg()
            {
                Status = status,
                Message = Msg
            };
            return Request.CreateResponse(HttpStatusCode.OK, res);
        }



        //app Reviwer approved
        [HttpPost]
        [Route("PickerRTDOrderProcess")]
        public HttpResponseMessage PickerRTDOrderProcess(ApprovedDispatchedDC ReviewerApprovedAndDispatchedobj)
        {
            var identity = User.Identity as ClaimsIdentity;
            int compid = 0, userid = 0;

            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))
                compid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);

            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
            ResMsg res;
            string Msg = "";
            bool status = true;
            People Dboyinfo = null;
            ReviewerApprovedAndDispatchedobj.UserId = userid;

            if (ReviewerApprovedAndDispatchedobj.PickerId > 0 && ReviewerApprovedAndDispatchedobj.UserId > 0 && ReviewerApprovedAndDispatchedobj.DeliveryBoyId > 0 && ReviewerApprovedAndDispatchedobj.AgentId > 0)
            {
                TransactionOptions option = new TransactionOptions();
                option.IsolationLevel = System.Transactions.IsolationLevel.RepeatableRead;
                option.Timeout = TimeSpan.FromSeconds(120);
                using (var dbContextTransaction = new TransactionScope(TransactionScopeOption.RequiresNew, option))
                {
                    using (AuthContext context = new AuthContext())
                    {
                        var user = context.Peoples.FirstOrDefault(x => x.PeopleID == ReviewerApprovedAndDispatchedobj.UserId && x.Active);
                        Dboyinfo = context.Peoples.FirstOrDefault(x => x.PeopleID == ReviewerApprovedAndDispatchedobj.DeliveryBoyId && x.Active);
                        var pickerMaster = context.OrderPickerMasterDb.Where(x => x.Id == ReviewerApprovedAndDispatchedobj.PickerId && x.Status != 6 && x.IsApproved == true && x.IsDeleted == false).Include(c => c.orderPickerDetails).FirstOrDefault();
                        if (pickerMaster != null && pickerMaster.IsComplted)
                        {
                            if (pickerMaster.IsCanceled)
                            {
                                Msg = "Picker is Cancelled";
                                status = false;
                            }
                            else if (Dboyinfo == null)
                            {

                                Msg = " Please select Delivery Boy ";
                                status = false;
                            }

                            else if (user == null)
                            {

                                Msg = " You account is inactive";
                                status = false;
                            }

                            if (status)
                            {
                                List<int> orderids = pickerMaster.orderPickerDetails.Where(x => x.Status == 2).Select(x => x.OrderId).Distinct().ToList();
                                if (ReviewerApprovedAndDispatchedobj.OrderidRedispachedOrder != null && ReviewerApprovedAndDispatchedobj.OrderidRedispachedOrder.Any())
                                {
                                    orderids.AddRange(ReviewerApprovedAndDispatchedobj.OrderidRedispachedOrder);
                                }
                                var orderLists = context.OrderDispatchedMasters.Where(x => orderids.Contains(x.OrderId) && (x.Status == "Ready to Dispatch" || x.Status == "Delivery Redispatch")).Include(x => x.orderDetails).ToList();
                                if (orderLists.Count == 0)
                                {
                                    Msg = " There is no order are eligible to Approved";
                                    status = false;
                                }
                                if (orderLists != null && orderLists.Any() && status)
                                {
                                    DeliveryIssuance DeliveryIssuance = new DeliveryIssuance();
                                    DeliveryIssuance.userid = userid;
                                    DeliveryIssuance.WarehouseId = Dboyinfo.WarehouseId;
                                    DeliveryIssuance.DisplayName = Dboyinfo.DisplayName;
                                    DeliveryIssuance.PeopleID = Dboyinfo.PeopleID;
                                    DeliveryIssuance.AgentId = ReviewerApprovedAndDispatchedobj.AgentId;
                                    DeliveryIssuance.Cityid = Dboyinfo.Cityid ?? 0;
                                    DeliveryIssuance.WarehouseId = Dboyinfo.WarehouseId;
                                    DeliveryIssuance.CreatedDate = indianTime;
                                    DeliveryIssuance.UpdatedDate = indianTime;
                                    DeliveryIssuance.OrderdispatchIds = "";
                                    DeliveryIssuance.OrderIds = "";
                                    DeliveryIssuance.TotalAssignmentAmount = 0;
                                    DeliveryIssuance.details = new List<IssuanceDetails>();

                                    var OrderIds = orderLists.Select(x => x.OrderId).Distinct().ToList();
                                    var ordermasterLists = context.DbOrderMaster.Where(x => OrderIds.Contains(x.OrderId)).Include(x => x.orderDetails).ToList();

                                    foreach (var item in OrderIds)
                                    {
                                        var orderlist = orderLists.FirstOrDefault(x => x.OrderId == item && x.DboyMobileNo == null);
                                        if (orderlist != null)
                                        {
                                            orderlist.DBoyId = Dboyinfo.PeopleID;
                                            orderlist.DboyMobileNo = Dboyinfo.Mobile;
                                            orderlist.DboyName = Dboyinfo.DisplayName;
                                        }
                                    }
                                    var OrderDispatchedDetailssList = context.OrderDispatchedDetailss.Where(x => OrderIds.Contains(x.OrderId) && x.qty > 0).ToList();
                                    var Assignmentpicklist = OrderDispatchedDetailssList.GroupBy(x => x.ItemMultiMRPId).Select(t =>
                                    new IssuanceDetails
                                    {
                                        OrderId = string.Join(",", OrderDispatchedDetailssList.Where(x => x.ItemMultiMRPId == t.Key).Select(x => x.OrderId).ToArray()),
                                        OrderQty = string.Join(",", OrderDispatchedDetailssList.Where(x => x.ItemMultiMRPId == t.Key).Select(a => String.Format("{0} - {1}", a.OrderId, a.qty)).ToArray()),
                                        OrderDispatchedMasterId = OrderDispatchedDetailssList.Where(x => x.ItemMultiMRPId == t.Key).Select(x => x.OrderDispatchedMasterId).FirstOrDefault(),
                                        OrderDispatchedDetailsId = OrderDispatchedDetailssList.Where(x => x.ItemMultiMRPId == t.Key).Select(x => x.OrderDispatchedDetailsId).FirstOrDefault(),
                                        qty = OrderDispatchedDetailssList.Where(x => x.ItemMultiMRPId == t.Key).Sum(x => x.qty),
                                        itemNumber = OrderDispatchedDetailssList.FirstOrDefault(x => x.ItemMultiMRPId == t.Key).itemNumber,
                                        ItemId = OrderDispatchedDetailssList.FirstOrDefault(x => x.ItemMultiMRPId == t.Key).ItemId,
                                        itemname = OrderDispatchedDetailssList.FirstOrDefault(x => x.ItemMultiMRPId == t.Key).itemname
                                    }).ToList();

                                    DeliveryIssuance.details = Assignmentpicklist;
                                    DeliveryIssuance.TotalAssignmentAmount = 0;
                                    DeliveryIssuance.TotalAssignmentAmount += orderLists.Sum(x => x.GrossAmount);
                                    DeliveryIssuance.OrderdispatchIds = string.Join(",", OrderDispatchedDetailssList.Select(x => x.OrderDispatchedMasterId).Distinct());
                                    DeliveryIssuance.OrderIds = string.Join(",", OrderDispatchedDetailssList.Select(x => x.OrderId).Distinct());
                                    DeliveryIssuance.Status = "SavedAsDraft";
                                    DeliveryIssuance.IsActive = true;
                                    context.DeliveryIssuanceDb.Add(DeliveryIssuance);
                                    context.Commit();
                                    ReadyToPickDispatchedHelper helper = new ReadyToPickDispatchedHelper();
                                    bool isAssignmentCreated = helper.CreateAssignment(context, user, dbContextTransaction, DeliveryIssuance, orderLists, ordermasterLists);
                                    if (isAssignmentCreated)
                                    {
                                        pickerMaster.Status = 6; //Dispatched(ApprovedDispatched),
                                        pickerMaster.IsDispatched = true;
                                        pickerMaster.ModifiedDate = indianTime;
                                        pickerMaster.ModifiedBy = userid;
                                        pickerMaster.DBoyId = Dboyinfo.PeopleID;//
                                        pickerMaster.AgentId = DeliveryIssuance.AgentId;//
                                        pickerMaster.DeliveryIssuanceId = DeliveryIssuance.DeliveryIssuanceId;
                                        pickerMaster.TotalAssignmentAmount = DeliveryIssuance.TotalAssignmentAmount;
                                        context.Entry(pickerMaster).State = EntityState.Modified;
                                        context.Commit();
                                        dbContextTransaction.Complete();
                                        Msg = "Order Ready to Dispatched Successfully and Assignment is NO# :" + DeliveryIssuance.DeliveryIssuanceId;
                                    }
                                    else
                                    {
                                        Msg = "Something went wrong ,Please retry after sometime";
                                    }

                                }
                            }
                        }
                        else
                        {
                            Msg = "Picker List Still in Pending, you can't Approved";
                            status = false;
                        }
                    }
                }
            }
            res = new ResMsg()
            {
                Status = status,
                Message = Msg
            };
            return Request.CreateResponse(HttpStatusCode.OK, res);
        }

        [HttpPost]
        [Route("PickerSubmit")]
        public async Task<HttpResponseMessage> PickerSubmitRequest(int PickerId, int UserId)
        {
            OrderPickerMaster OrderPickerMaster = new OrderPickerMaster();
            string Msg = "";
            bool status = false;
            if (PickerId > 0 && UserId > 0)
            {
                OrderOutPublisher Publisher = new OrderOutPublisher();
                List<BatchCodeSubjectDc> PublisherPickerRejectStockList = new List<BatchCodeSubjectDc>();
                TransactionOptions option = new TransactionOptions();
                option.IsolationLevel = System.Transactions.IsolationLevel.RepeatableRead;
                option.Timeout = TimeSpan.FromSeconds(120);
                using (var dbContextTransaction = new TransactionScope(TransactionScopeOption.Required, option))
                {
                    using (AuthContext context = new AuthContext())
                    {
                        OrderPickerMaster = context.OrderPickerMasterDb.Where(x => x.Id == PickerId && x.IsDeleted == false).Include(x => x.orderPickerDetails).FirstOrDefault();

                        var warehouse = context.Warehouses.FirstOrDefault(x => x.WarehouseId == OrderPickerMaster.WarehouseId);

                        if (warehouse.IsStopCurrentStockTrans)
                        {
                            Msg = "Inventory Transactions are currently disabled for this warehouse... Please try after some time";
                            status = false;

                            var res1 = new ResMsg()
                            {
                                Status = status,
                                Message = Msg
                            };
                            return Request.CreateResponse(HttpStatusCode.OK, res1);
                        }



                        if (OrderPickerMaster != null && !OrderPickerMaster.IsCanceled && !OrderPickerMaster.IsComplted)
                        {

                            bool IsCompleted = OrderPickerMaster.orderPickerDetails.Any(x => x.Status == 0 && x.OrderPickerMasterId == OrderPickerMaster.Id);
                            if (IsCompleted)
                            {
                                Msg = "Picker List Still in Pending, you can't submit";
                                status = false;
                            }
                            else
                            {
                                var RejectedOrderids = OrderPickerMaster.orderPickerDetails.Where(x => x.Status == 3).Select(x => x.OrderId).Distinct().ToList();
                                if (RejectedOrderids != null && RejectedOrderids.Any())
                                {
                                    var param = new SqlParameter("PickerId", OrderPickerMaster.Id);
                                    long? TripMasterId = context.Database.SqlQuery<long?>("exec operation.IsTripPicker @PickerId", param).FirstOrDefault();
                                    if (TripMasterId > 0)
                                    {
                                        TripPlannerHelper triphelp = new TripPlannerHelper();
                                        bool IsSuccess = triphelp.RemovePickerOrderFromTrip(TripMasterId.Value, PickerId, RejectedOrderids, context, UserId);
                                        if (!IsSuccess)
                                        {
                                            dbContextTransaction.Dispose();
                                            Msg = "Remove Picker Order From Trip Failed ";
                                            status = false;
                                            var errorres = new ResMsg()
                                            {
                                                Status = status,
                                                Message = Msg
                                            };
                                            return Request.CreateResponse(HttpStatusCode.OK, errorres);
                                        }
                                    }
                                    //else
                                    //{
                                    foreach (var Odid in RejectedOrderids)
                                    {
                                        OrderMaster omCheck = context.DbOrderMaster.Where(x => x.OrderId == Odid && x.Status == "ReadyToPick" && x.Deleted == false).Include(x => x.orderDetails).FirstOrDefault();
                                        if (omCheck != null)
                                        {
                                            foreach (var item in omCheck.orderDetails)
                                            {
                                                item.Status = "Pending";
                                                item.UpdatedDate = indianTime;
                                            }
                                            omCheck.Status = "Pending";
                                            omCheck.UpdatedDate = indianTime;
                                            context.Entry(omCheck).State = EntityState.Modified;
                                            #region Order History
                                            Model.OrderMasterHistories h1 = new Model.OrderMasterHistories();
                                            h1.orderid = omCheck.OrderId;
                                            h1.Status = omCheck.Status;
                                            h1.Reasoncancel = null;
                                            h1.Warehousename = omCheck.WarehouseName;
                                            h1.userid = UserId;
                                            h1.CreatedDate = DateTime.Now;
                                            context.OrderMasterHistoriesDB.Add(h1);
                                            #endregion
                                            if (context.Commit() > 0)
                                            {
                                                #region stock Hit
                                                //for currentstock
                                                MultiStockHelper<OnPickedCancelDc> MultiStockHelpers = new MultiStockHelper<OnPickedCancelDc>();
                                                List<OnPickedCancelDc> RTDOnPickedCancelList = new List<OnPickedCancelDc>();
                                                foreach (var StockHit in omCheck.orderDetails.Where(x => x.qty > 0))
                                                {
                                                    int qty = OrderPickerMaster.orderPickerDetails.FirstOrDefault(x => x.OrderDetailsId == StockHit.OrderDetailsId).Qty;
                                                    if (qty > 0)
                                                    {
                                                        bool isfree = false;
                                                        string RefStockCode = "";//= omCheck.OrderType == 8 ? "CL" : "C";
                                                        if (omCheck.OrderType == 8)
                                                        {
                                                            RefStockCode = "CL";
                                                        }
                                                        else if (omCheck.OrderType == 10)
                                                        {
                                                            RefStockCode = "NR";
                                                        }
                                                        else
                                                        {
                                                            RefStockCode = "C";
                                                        }


                                                        if (StockHit.IsFreeItem && StockHit.IsDispatchedFreeStock)
                                                        {
                                                            RefStockCode = "F";
                                                            isfree = true;
                                                        }
                                                        RTDOnPickedCancelList.Add(new OnPickedCancelDc
                                                        {
                                                            ItemMultiMRPId = StockHit.ItemMultiMRPId,
                                                            OrderDispatchedDetailsId = StockHit.OrderDetailsId,
                                                            OrderId = StockHit.OrderId,
                                                            Qty = qty,
                                                            UserId = UserId,
                                                            WarehouseId = StockHit.WarehouseId,
                                                            IsFreeStock = isfree,
                                                            RefStockCode = RefStockCode
                                                        });
                                                    }
                                                }
                                                if (RTDOnPickedCancelList.Any())
                                                {
                                                    bool IsUpdate = MultiStockHelpers.MakeEntry(RTDOnPickedCancelList, "Stock_OnPickedCancel", context, dbContextTransaction);
                                                    if (!IsUpdate)
                                                    {
                                                        dbContextTransaction.Dispose();
                                                        Msg = "Something went wrong";
                                                        status = false;
                                                        var errorres = new ResMsg()
                                                        {
                                                            Status = status,
                                                            Message = Msg
                                                        };
                                                        return Request.CreateResponse(HttpStatusCode.OK, errorres);
                                                    }
                                                    #region BatchCode
                                                    foreach (var s in RTDOnPickedCancelList.Where(x => x.Qty > 0))
                                                    {
                                                        PublisherPickerRejectStockList.Add(new BatchCodeSubjectDc
                                                        {
                                                            ObjectDetailId = s.OrderDispatchedDetailsId,  // its OrderDetailsId
                                                            ObjectId = s.OrderId,
                                                            StockType = s.RefStockCode,
                                                            Quantity = s.Qty,
                                                            WarehouseId = s.WarehouseId,
                                                            ItemMultiMrpId = s.ItemMultiMRPId
                                                        });
                                                    }
                                                    #endregion
                                                }
                                                #endregion
                                            }
                                        }
                                    }

                                }
                                var Pickertimer = context.PickerTimerDb.Where(x => x.OrderPickerMasterId == OrderPickerMaster.Id && x.EndTime == null && x.Type == 0).ToList();
                                if (Pickertimer != null)
                                {
                                    OrderPickerMaster.ModifiedDate = indianTime;
                                    OrderPickerMaster.ModifiedBy = OrderPickerMaster.PickerPersonId;
                                }
                                bool IsNoorderChecker = OrderPickerMaster.orderPickerDetails.Any(x => x.Status != 3);
                                if (IsNoorderChecker)
                                {
                                    OrderPickerMaster.IsComplted = true;
                                    OrderPickerMaster.SubmittedDate = indianTime;
                                    OrderPickerMaster.Status = 2;// 2=Submitted(Maker) 
                                }
                                else
                                {
                                    OrderPickerMaster.IsCanceled = true;
                                    OrderPickerMaster.Status = 4; //Canceled
                                    OrderPickerMaster.SubmittedDate = indianTime;

                                    OrderPickerMaster.Comment = "System Canceled"; //Canceled
                                }

                                context.Entry(OrderPickerMaster).State = EntityState.Modified;

                                if (Pickertimer != null)
                                {
                                    Pickertimer.ForEach(x =>
                                    {
                                        x.EndTime = indianTime;
                                        context.Entry(x).State = EntityState.Modified;
                                    });
                                }
                                if (context.Commit() > 0)
                                {
                                    dbContextTransaction.Complete();
                                    Msg = "Picker Submitted Successfully";
                                    status = true;
                                }
                                else
                                {
                                    Msg = "Something went wrong";
                                    status = false;
                                }
                            }
                        }
                        else
                        {
                            if (OrderPickerMaster.IsCanceled) { Msg = "Picker Canceled"; }
                            if (OrderPickerMaster.IsComplted) { Msg = "Picker is already submitted"; }
                            status = false;
                        }
                    }
                }
                if (PublisherPickerRejectStockList != null && PublisherPickerRejectStockList.Any() && status)
                {
                    Publisher.PlannedRejectPublish(PublisherPickerRejectStockList);
                }
            }

            var res = new ResMsg()
            {
                Status = status,
                Message = Msg
            };
            return Request.CreateResponse(HttpStatusCode.OK, res);
        }


        [HttpPost]
        [Route("StartPickJob")]
        public async Task<HttpResponseMessage> StartEndPickJob(int PickerId)  //type = start / end
        {
            PickerTimer PickerTimers = new PickerTimer();
            ResMsg res;
            string Msg = "";
            bool status = false;
            if (PickerId > 0)
            {
                using (AuthContext context = new AuthContext())
                {
                    var OrderPickerMaster = context.OrderPickerMasterDb.Where(x => x.Id == PickerId && x.IsDeleted == false).FirstOrDefault();
                    if (OrderPickerMaster != null && !OrderPickerMaster.IsCanceled)
                    {
                        //bool PickListAnyInProcess = context.OrderPickerMasterDb.Any(x => x.PickerPersonId == OrderPickerMaster.PickerPersonId && x.Id != OrderPickerMaster.Id && x.IsComplted == false && x.IsDeleted == false && x.IsCanceled == false && x.Status != 0);

                        bool PickListAnyInProcess = context.PickerTimerDb.Any(x => x.CreatedBy == OrderPickerMaster.PickerPersonId && x.Id != OrderPickerMaster.Id && x.Type == 0 && x.EndTime == null);


                        if (!PickListAnyInProcess)
                        {
                            var PickerTimer = context.PickerTimerDb.Where(x => x.OrderPickerMasterId == PickerId && x.Type == 0).ToList();
                            if (PickerTimer.Count == 0)
                            {
                                PickerTimers.OrderPickerMasterId = OrderPickerMaster.Id;
                                PickerTimers.StartTime = DateTime.Now;
                                PickerTimers.EndTime = null;
                                PickerTimers.CreatedBy = OrderPickerMaster.PickerPersonId;
                                PickerTimers.Type = 0;//Maker
                                context.PickerTimerDb.Add(PickerTimers);

                                OrderPickerMaster.Status = 1;// 1= InProgress(Maker), 
                                OrderPickerMaster.ModifiedDate = indianTime;
                                context.Entry(OrderPickerMaster).State = EntityState.Modified;

                                context.Commit();
                                Msg = "Picker Start Successfully";
                                status = true;
                            }
                            else if (PickerTimer.All(x => x.EndTime != null))
                            {
                                //Restart Pick
                                PickerTimers.OrderPickerMasterId = OrderPickerMaster.Id;
                                PickerTimers.StartTime = DateTime.Now;
                                PickerTimers.EndTime = null;
                                PickerTimers.Type = 0;//Maker
                                PickerTimers.CreatedBy = OrderPickerMaster.PickerPersonId;
                                context.PickerTimerDb.Add(PickerTimers);

                                OrderPickerMaster.Status = 1;// 1= InProgress(Maker), 
                                OrderPickerMaster.ModifiedDate = indianTime;
                                context.Entry(OrderPickerMaster).State = EntityState.Modified;

                                context.Commit();
                                Msg = "Picker Restart Successfully";
                                status = true;
                            }
                            else
                            {
                                PickerTimers = PickerTimer.Where(x => x.EndTime == null).FirstOrDefault();
                                status = true;
                            }
                        }
                        else
                        {
                            Msg = "पिछला पिकर प्रगति पर होने के कारण आप नया पिकर प्रारंभ नहीं कर सकते|";
                            status = true;
                        }
                    }
                    else
                    {
                        Msg = "Picker Canceled";
                        status = true;
                    }
                }
            }
            res = new ResMsg()
            {
                Status = status,
                Message = Msg
            };
            return Request.CreateResponse(HttpStatusCode.OK, res);
        }


        [HttpGet]
        [Route("GetPickerTimerListByPickerId")]//on Backend
        public async Task<List<PickerTimerDc>> GetPickerTimerListByPickerId(int OrderPickerMasterId)
        {
            using (AuthContext db = new AuthContext())
            {

                string sqlquery = "select * from PickerTimers where OrderPickerMasterId =" + OrderPickerMasterId;
                List<PickerTimerDc> newdata = db.Database.SqlQuery<PickerTimerDc>(sqlquery).ToList();
                return newdata;
            }
        }
        [HttpGet]
        [Route("GetPickerTimerList")]//on Backend
        public async Task<List<PickerTimerDc>> GetPickerTimerList()
        {
            using (AuthContext db = new AuthContext())
            {

                string sqlquery = "select * from PickerTimers";
                List<PickerTimerDc> newdata = db.Database.SqlQuery<PickerTimerDc>(sqlquery).ToList();
                return newdata;
            }
        }


        [HttpPost]
        [Route("ReviwerRejectSubmit")]
        public async Task<HttpResponseMessage> ReviwerRejectSubmit(int PickerId, int UserId)
        {
            string Msg = "";
            bool status = false;

            if (PickerId > 0 && UserId > 0)
            {
                using (AuthContext context = new AuthContext())
                {
                    OrderPickerMaster Opm = context.OrderPickerMasterDb.Where(x => x.Id == PickerId && x.IsDeleted == false && x.IsCanceled == false).FirstOrDefault();
                    if (Opm != null)
                    {
                        Opm.Status = 5;//Repicking
                        Opm.Comment = "Reviwer Rejected for RePicking";
                        Opm.ModifiedDate = indianTime;
                        Opm.ApproverId = UserId;
                        Opm.IsComplted = false;
                        Opm.RePickingCount += 1;
                        Opm.IsInventorySupervisor = false;
                        Opm.InventorySupervisorId = 0;
                        Opm.IsCheckerGrabbed = false;
                        Opm.IsInventorySupervisorStart = false;
                        Opm.IsInventory = false;
                        context.Entry(Opm).State = EntityState.Modified;
                        var Pickertimer = context.PickerTimerDb.Where(x => x.OrderPickerMasterId == Opm.Id && x.EndTime == null && (x.Type == 1)).FirstOrDefault();
                        var InventryPickertimer = context.PickerTimerDb.Where(x => x.OrderPickerMasterId == Opm.Id && x.EndTime == null && (x.Type == 2)).FirstOrDefault();
                        if (Pickertimer != null)
                        {
                            Pickertimer.EndTime = indianTime;
                            context.Entry(Pickertimer).State = EntityState.Modified;
                        }
                        if (InventryPickertimer != null)
                        {
                            InventryPickertimer.EndTime = indianTime;
                            context.Entry(InventryPickertimer).State = EntityState.Modified;
                        }
                        if (context.Commit() > 0) { status = true; Msg = "Successfully Submitted for RePicking"; }
                    }
                }
            }
            var res = new ResMsg()
            {
                Status = status,
                Message = Msg
            };
            return Request.CreateResponse(HttpStatusCode.OK, res);
        }

        [HttpPost]
        [Route("ReviwerStartPickJob")]
        public async Task<HttpResponseMessage> ReviwerStartPickJob(int PickerId, int UserId)  //type = start / end
        {
            PickerTimer PickerTimers = new PickerTimer();
            ResMsg res;
            string Msg = "";
            bool status = false;
            if (PickerId > 0)
            {
                using (AuthContext context = new AuthContext())
                {
                    var OrderPickerMaster = context.OrderPickerMasterDb.Where(x => x.Id == PickerId && x.IsDeleted == false).FirstOrDefault();
                    if (OrderPickerMaster != null && !OrderPickerMaster.IsCanceled)
                    {

                        //bool PickListAnyInProcess = context.OrderPickerMasterDb.Any(x => x.ApproverId == PickerId && x.Id != OrderPickerMaster.Id && x.IsApproved == false && x.IsComplted && x.IsDeleted == false && x.IsCanceled == false);
                        bool PickListAnyInProcess = context.PickerTimerDb.Any(x => x.CreatedBy == UserId && x.Id != OrderPickerMaster.Id && x.Type == 1 && x.EndTime == null);

                        if (!PickListAnyInProcess)
                        {
                            var PickerTimer = context.PickerTimerDb.Where(x => x.OrderPickerMasterId == PickerId && x.Type == 1).ToList();
                            if (PickerTimer.Count == 0)
                            {
                                PickerTimers.OrderPickerMasterId = OrderPickerMaster.Id;
                                PickerTimers.StartTime = DateTime.Now;
                                PickerTimers.EndTime = null;
                                PickerTimers.CreatedBy = UserId;
                                PickerTimers.Type = 1;//Checker
                                context.PickerTimerDb.Add(PickerTimers);

                                OrderPickerMaster.Status = 1;// 
                                OrderPickerMaster.ApproverId = UserId;// Checker id, 

                                OrderPickerMaster.ModifiedDate = indianTime;
                                context.Entry(OrderPickerMaster).State = EntityState.Modified;

                                context.Commit();
                                Msg = "Picker Start Successfully";
                                status = true;
                            }
                            else if (PickerTimer.All(x => x.EndTime != null))
                            {
                                //Restart Pick
                                PickerTimers.OrderPickerMasterId = OrderPickerMaster.Id;
                                PickerTimers.StartTime = DateTime.Now;
                                PickerTimers.EndTime = null;
                                PickerTimers.CreatedBy = UserId;
                                PickerTimers.Type = 1;//Checker
                                context.PickerTimerDb.Add(PickerTimers);
                                OrderPickerMaster.Status = 1;//
                                OrderPickerMaster.ApproverId = UserId;// Checker id, 
                                OrderPickerMaster.ModifiedDate = indianTime;
                                context.Entry(OrderPickerMaster).State = EntityState.Modified;
                                context.Commit();
                                Msg = "Picker Restart Successfully";
                                status = true;
                            }
                            else
                            {
                                PickerTimers = PickerTimer.Where(x => x.EndTime == null).FirstOrDefault();
                                status = true;
                            }
                        }
                        else
                        {
                            Msg = "पिछला पिकर प्रगति पर होने के कारण आप नया पिकर प्रारंभ नहीं कर सकते|";
                            status = true;
                        }
                    }
                    else
                    {
                        Msg = "Picker Canceled";
                        status = true;
                    }
                }
            }
            res = new ResMsg()
            {
                Status = status,
                Message = Msg
            };
            return Request.CreateResponse(HttpStatusCode.OK, res);
        }


        [HttpGet]
        [Route("MakerActivity")]
        [AllowAnonymous]
        public async Task<HttpResponseMessage> MakerActivity(int Take, int Skip, int UserId, int PickerId = 0)
        {
            bool Status = false;
            PickerJobListRes res;
            string Msg = "";
            List<PickerJobListDc> pickerJobList = new List<PickerJobListDc>();
            using (var context = new AuthContext())
            {
                if (UserId > 0)
                {
                    // Skip++;
                    var OrderPickerMasterList = new List<OrderPickerMaster>();
                    if (PickerId > 0)
                    {
                        OrderPickerMasterList = context.OrderPickerMasterDb.Where(x => x.PickerPersonId == UserId && x.Id == PickerId && x.IsDeleted == false && (x.IsComplted == true || x.IsCanceled == true)).OrderByDescending(x => x.Id).Include("OrderPickerDetails").ToList();
                    }
                    else
                    {
                        OrderPickerMasterList = context.OrderPickerMasterDb.Where(x => x.PickerPersonId == UserId && x.IsDeleted == false && (x.IsComplted == true || x.IsCanceled == true)).OrderByDescending(x => x.Id).Include("OrderPickerDetails").Skip(Skip).Take(Take).ToList();
                    }
                    if (OrderPickerMasterList != null && OrderPickerMasterList.Any())
                    {
                        var CreatedByids = OrderPickerMasterList.Select(x => x.CreatedBy).Distinct().ToList();
                        var SubmittedIds = OrderPickerMasterList.Select(x => x.PickerPersonId).Distinct().ToList();
                        var peopleIds = CreatedByids.Concat(SubmittedIds);
                        var ModifiedByids = OrderPickerMasterList.Select(x => x.ModifiedBy).Distinct().ToList();
                        var DBoyIds = OrderPickerMasterList.Select(x => x.DBoyId).Distinct().ToList();
                        var people = context.Peoples.Where(x =>
                        peopleIds.Contains(x.PeopleID) ||
                        ModifiedByids.Contains(x.PeopleID) ||
                        DBoyIds.Contains(x.PeopleID)
                        ).Distinct().ToList();

                        //var peopleMod = context.Peoples.Where(x => ModifiedByids.Contains(x.PeopleID)).Distinct().ToList();
                        foreach (var a in OrderPickerMasterList)
                        {
                            if (a.orderPickerDetails.Any(x => x.OrderPickerMasterId == a.Id))
                            {
                                double amount = 0;
                                var orderids = a.orderPickerDetails.Where(y => y.Status != 3).Select(y => y.OrderId).Distinct().ToList();
                                var clusters = context.DbOrderMaster.Where(x => orderids.Contains(x.OrderId)).Select(x => x.ClusterName).Distinct().ToList();
                                var dboyname = people.Where(x => x.PeopleID == a.DBoyId).FirstOrDefault()?.DisplayName;
                                var orderDetails = a.orderPickerDetails.Where(x => x.Status != 3).Select(x => new { x.OrderDetailsId, x.Qty }).ToList();
                                var orderdetailIds = orderDetails.Select(x => x.OrderDetailsId).ToList();
                                var dbOrderDetails = context.DbOrderDetails.Where(x => orderdetailIds.Contains(x.OrderDetailsId)).Select(x => new { x.OrderDetailsId, x.UnitPrice }).ToList();
                                dbOrderDetails.ForEach(x =>
                                {
                                    amount += orderDetails.Where(y => y.OrderDetailsId == x.OrderDetailsId).Sum(y => y.Qty * x.UnitPrice);
                                });
                                PickerJobListDc PickerJobListob = new PickerJobListDc();
                                PickerJobListob.Id = a.Id;
                                PickerJobListob.DeliveryIssuanceId = a.DeliveryIssuanceId > 0 ? Convert.ToString(a.DeliveryIssuanceId) : a.MultiDeliveryIssuanceIds; ;
                                PickerJobListob.CreatedDate = a.CreatedDate;
                                PickerJobListob.SubmittedDate = a.SubmittedDate;
                                PickerJobListob.CanceledDate = a.ModifiedDate;
                                PickerJobListob.ApprovedDate = a.ApprovedDate;
                                PickerJobListob.count = a.orderPickerDetails.GroupBy(x => x.ItemMultiMrpId).Count();
                                PickerJobListob.IsApproved = a.IsApproved;
                                PickerJobListob.Commenet = a.Comment;
                                PickerJobListob.SubmittedBy = people.FirstOrDefault(x => x.PeopleID == a.PickerPersonId).DisplayName;
                                PickerJobListob.CreatedBy = people.FirstOrDefault(x => x.PeopleID == a.CreatedBy).DisplayName;
                                PickerJobListob.CanceledBy = people.FirstOrDefault(x => x.PeopleID == a.ModifiedBy).DisplayName;
                                PickerJobListob.dboyName = dboyname == null ? "" : dboyname;
                                PickerJobListob.amount = Math.Round(amount, 0);
                                PickerJobListob.ClusterName = string.Join(",", clusters);
                                if ((a.Status == 2 || a.Status == 1) && a.IsComplted)
                                {
                                    PickerJobListob.Status = "Submitted";
                                }
                                else if (a.Status == 3 && a.IsApproved)
                                {
                                    PickerJobListob.Status = "Approved";
                                }
                                else if (a.Status == 6 && a.IsDispatched)
                                {
                                    PickerJobListob.Status = "Assignment Created";
                                }
                                else if (a.Status == 4)
                                {
                                    PickerJobListob.Status = "Canceled";
                                }
                                else if (a.Status == 5)
                                {
                                    PickerJobListob.Status = "RePicking";
                                }
                                pickerJobList.Add(PickerJobListob);
                            }
                        }
                        if (pickerJobList != null && pickerJobList.Any())
                        {
                            Status = true;
                            Msg = "Record found";
                        }
                        else
                        {
                            Status = true;
                            Msg = "No Record found";
                        }
                    }
                }
            }
            res = new PickerJobListRes()
            {
                PickerJobLists = pickerJobList,
                Status = Status,
                Message = Msg
            };
            return Request.CreateResponse(HttpStatusCode.OK, res);
        }

        [HttpGet]
        [Route("CheckerActivity")]
        public async Task<HttpResponseMessage> CheckerActivity(int Take, int Skip, int UserId, int PickerId = 0)
        {
            bool Status = false;
            PickerJobListRes res;
            string Msg = "";
            List<PickerJobListDc> pickerJobList = new List<PickerJobListDc>();
            using (var context = new AuthContext())
            {
                if (UserId > 0)
                {
                    var OrderPickerMasterList = new List<OrderPickerMaster>();
                    if (PickerId > 0)
                    {
                        OrderPickerMasterList = context.OrderPickerMasterDb.Where(x => x.ApproverId == UserId && x.Id == PickerId && x.IsDeleted == false && (x.IsDispatched == true || x.IsApproved == true || x.IsCanceled == true || x.Status == 5 || (x.Status == 1 && x.RePickingCount > 0))).OrderByDescending(x => x.Id).Include("OrderPickerDetails").ToList();
                    }
                    else
                    {
                        OrderPickerMasterList = context.OrderPickerMasterDb.Where(x => x.ApproverId == UserId && x.IsDeleted == false && (x.IsDispatched == true || x.IsApproved == true || x.IsCanceled == true || x.Status == 5 || (x.Status == 1 && x.RePickingCount > 0))).OrderByDescending(x => x.Id).Include("OrderPickerDetails").Skip(Skip).Take(Take).ToList();
                    }
                    if (OrderPickerMasterList != null && OrderPickerMasterList.Any())
                    {
                        var CreatedByids = OrderPickerMasterList.Select(x => x.CreatedBy).Distinct().ToList();
                        var SubmittedIds = OrderPickerMasterList.Select(x => x.PickerPersonId).Distinct().ToList();
                        var peopleIds = CreatedByids.Concat(SubmittedIds);
                        var ModifiedByids = OrderPickerMasterList.Select(x => x.ModifiedBy).Distinct().ToList();
                        var DBoyIds = OrderPickerMasterList.Select(x => x.DBoyId).Distinct().ToList();
                        var people = context.Peoples.Where(x =>
                        peopleIds.Contains(x.PeopleID) ||
                        ModifiedByids.Contains(x.PeopleID) ||
                        DBoyIds.Contains(x.PeopleID)
                        ).Distinct().ToList();

                        // var peopleMod = context.Peoples.Where(x => ModifiedByids.Contains(x.PeopleID)).Distinct().ToList();
                        //var people = context.Peoples.Where(x => peopleIds.Contains(x.PeopleID)).Distinct().ToList();

                        foreach (var a in OrderPickerMasterList)
                        {
                            bool inProress = false;

                            if (a.orderPickerDetails.Any(x => x.OrderPickerMasterId == a.Id))
                            {
                                double amount = 0;
                                var orderids = a.orderPickerDetails.Where(y => y.Status != 3).Select(y => y.OrderId).Distinct().ToList();
                                var clusters = context.DbOrderMaster.Where(x => orderids.Contains(x.OrderId)).Select(x => x.ClusterName).Distinct().ToList();
                                var dboyname = people.Where(x => x.PeopleID == a.DBoyId).FirstOrDefault()?.DisplayName;
                                var orderDetails = a.orderPickerDetails.Where(x => x.Status != 3).Select(x => new { x.OrderDetailsId, x.Qty }).ToList();
                                var orderdetailIds = orderDetails.Select(x => x.OrderDetailsId).ToList();
                                var dbOrderDetails = context.DbOrderDetails.Where(x => orderdetailIds.Contains(x.OrderDetailsId)).Select(x => new { x.OrderDetailsId, x.UnitPrice }).ToList();
                                dbOrderDetails.ForEach(x =>
                                {
                                    amount += orderDetails.Where(y => y.OrderDetailsId == x.OrderDetailsId).Sum(y => y.Qty * x.UnitPrice);
                                });
                                PickerJobListDc PickerJobListob = new PickerJobListDc();
                                PickerJobListob.Id = a.Id;
                                PickerJobListob.DeliveryIssuanceId = a.DeliveryIssuanceId > 0 ? Convert.ToString(a.DeliveryIssuanceId) : a.MultiDeliveryIssuanceIds; ;

                                PickerJobListob.CreatedDate = a.CreatedDate;
                                PickerJobListob.SubmittedDate = a.SubmittedDate;
                                PickerJobListob.ApprovedDate = a.ApprovedDate;
                                PickerJobListob.CanceledDate = a.ModifiedDate;
                                PickerJobListob.count = a.orderPickerDetails.GroupBy(x => x.ItemMultiMrpId).Count();
                                PickerJobListob.IsApproved = a.IsApproved;
                                PickerJobListob.Commenet = a.Comment;
                                PickerJobListob.SubmittedBy = people.FirstOrDefault(x => x.PeopleID == a.PickerPersonId)?.DisplayName;
                                PickerJobListob.CreatedBy = people.FirstOrDefault(x => x.PeopleID == a.CreatedBy)?.DisplayName;
                                PickerJobListob.CanceledBy = people.FirstOrDefault(x => x.PeopleID == a.ModifiedBy)?.DisplayName;
                                PickerJobListob.dboyName = dboyname == null ? "" : dboyname;
                                PickerJobListob.amount = Math.Round(amount, 0);
                                PickerJobListob.ClusterName = string.Join(",", clusters);
                                if (a.Status == 2)
                                {
                                    PickerJobListob.Status = "Submitted";
                                }
                                else if (a.Status == 3 && a.IsApproved)
                                {
                                    PickerJobListob.Status = "Approved";
                                }
                                else if (a.Status == 6 && a.IsDispatched)
                                {
                                    PickerJobListob.Status = "Assignment Created";
                                }
                                else if (a.Status == 4)
                                {
                                    PickerJobListob.Status = "Canceled";
                                }
                                else if (a.Status == 5 || (a.RePickingCount > 0 && a.Status == 1))
                                {
                                    if ((a.RePickingCount > 0 && a.Status == 1))
                                    {
                                        inProress = context.PickerTimerDb.Any(x => x.OrderPickerMasterId == a.Id && x.EndTime == null && x.CreatedBy == UserId && x.Type == 1);
                                    }
                                    else
                                    {
                                        PickerJobListob.Status = "RePicking";
                                    }
                                }
                                if (!inProress)
                                {
                                    pickerJobList.Add(PickerJobListob);
                                }
                            }
                        }
                        if (pickerJobList != null && pickerJobList.Any())
                        {
                            Status = true;
                            Msg = "Record found";
                        }
                        else
                        {
                            Status = true;
                            Msg = "No Record found";
                        }
                    }
                }
            }
            res = new PickerJobListRes()
            {
                PickerJobLists = pickerJobList,
                Status = Status,
                Message = Msg
            };
            return Request.CreateResponse(HttpStatusCode.OK, res);
        }


        [HttpGet]
        [Route("CheckerActivityDetail")]
        public async Task<List<sendPickertoApp>> CheckerActivityDetail(int id)
        {
            //OrderPickerMaster pickerlist = null;
            //List<sendPickertoApp> SendPickertoApp = new List<sendPickertoApp>();

            List<sendPickertoApp> SendPickertoApp = null;
            List<OrderPickerDetailDTO> pickerlist = new List<OrderPickerDetailDTO>();
            using (AuthContext context = new AuthContext())
            {

                if (id > 0)
                {
                    //pickerlist = context.OrderPickerMasterDb.Where(x => x.Id == id ).Include("OrderPickerDetails").FirstOrDefault();
                    if (pickerlist != null)
                    {
                        //added && x.Qty > 0 && x.Status != 3 by ravi rathod on date 29122022
                        //var picklist = pickerlist.orderPickerDetails.Where(x => x.Status > 0 && x.Qty > 0 && x.Status != 3).GroupBy(x => x.ItemMultiMrpId).Select(t =>
                        //    new sendPickertoApp
                        //    {
                        //        pickerId = pickerlist.Id,
                        //        Totalqty = t.Sum(x => x.Qty),
                        //        ItemMultiMRPId = t.Key,
                        //        SellingUnitName = t.Min(x => x.ItemName),
                        //        itemname = t.Min(x => x.ItemName),
                        //        //added && x.Qty > 0 && x.Status != 3 by ravi rathod on date 29122022
                        //        OrderDetailsSPA = pickerlist.orderPickerDetails.Where(x => x.ItemMultiMrpId == t.Key && x.Status > 0 && x.Qty > 0 && x.Status != 3).Select(r =>
                        //         new OrderDetailsSPA
                        //         {
                        //             pickerDetailsId = r.id,
                        //             pickerId = pickerlist.Id,
                        //             ItemMultiMrpId = r.ItemMultiMrpId,
                        //             orderid = r.OrderId,
                        //             qty = r.Qty,
                        //             Comment = r.Comment,
                        //             IsFreeItem = r.IsFreeItem,
                        //             Status = r.Status,
                        //         }).ToList()
                        //    }).ToList();
                        var orderPickerMasterIdParam = new SqlParameter("@OrderPickerMasterId", id);
                        pickerlist = context.Database.SqlQuery<OrderPickerDetailDTO>("Picker.GetPickerJobListDetails @OrderPickerMasterId", orderPickerMasterIdParam).ToList();
                        if (pickerlist != null)
                        {

                            var picklist = pickerlist.Where(x => x.Qty > 0).GroupBy(x => new { x.ItemMultiMrpId, x.IsClearance }).Select(t =>
                            new sendPickertoApp
                            {
                                pickerId = pickerlist.FirstOrDefault().OrderPickerMasterId,
                                Totalqty = t.Sum(x => x.Qty),
                                ItemMultiMRPId = t.Key.ItemMultiMrpId,
                                SellingUnitName = t.Min(x => x.ItemName),
                                itemname = t.Min(x => x.ItemName),
                                IsClearance = t.Key.IsClearance,
                                OrderDetailsSPA = pickerlist.Where(x => x.ItemMultiMrpId == t.Key.ItemMultiMrpId && x.Qty > 0).Select(r =>
                                   new OrderDetailsSPA
                                   {
                                       pickerDetailsId = r.id,
                                       pickerId = pickerlist.FirstOrDefault().OrderPickerMasterId,
                                       ItemMultiMrpId = r.ItemMultiMrpId,
                                       orderid = r.OrderId,
                                       qty = r.Qty,
                                       Comment = r.Comment,
                                       IsFreeItem = r.IsFreeItem,
                                       Status = r.Status,
                                   }).ToList()
                            }).ToList();
                            if (picklist != null && picklist.Any())
                            {
                                List<int> mmpi = picklist.Select(x => x.ItemMultiMRPId).ToList();
                                var a = string.Join<int>(",", mmpi);
                                var manager = new PickerManager();
                                List<pikeritemDC> Item = manager.GetItemMRPandBarcode(a);
                                foreach (var aa in picklist)
                                {
                                    aa.MRP = Item.Where(x => x.ItemMultiMRPId == aa.ItemMultiMRPId).Select(e => e.MRP).FirstOrDefault();
                                    aa.Number = Item.Where(x => x.ItemMultiMRPId == aa.ItemMultiMRPId).Select(e => e.Number).FirstOrDefault();
                                }
                                var itemNumbers = picklist.Select(x => x.Number).Distinct().ToList();
                                var itembarcodelist = context.ItemBarcodes.Where(c => itemNumbers.Contains(c.ItemNumber) && c.IsActive == true && c.IsDeleted == false).Select(x => new ItemBarcodeDc { ItemNumber = x.ItemNumber, Barcode = x.Barcode }).ToList();
                                foreach (var item in picklist)
                                {
                                    item.Barcode = (itembarcodelist != null && itembarcodelist.Any(x => x.ItemNumber == item.Number)) ? itembarcodelist.Where(x => x.ItemNumber == item.Number).Select(x => x.Barcode).ToList() : null;

                                }
                                SendPickertoApp = picklist.OrderByDescending(x => x.itemname).ToList();
                            }
                        }
                    }

                }
                var data = SendPickertoApp.OrderByDescending(x => x.itemname).ToList();
                return data;
            }
        }

        [HttpGet]
        [Route("CheckerActivityOrderDetails")]
        public async Task<List<OrderPickerDetailDC>> CheckerActivityOrderDetails(int pickerId, int ItemMultiMRPId)
        {
            List<OrderPickerDetailDC> result = new List<OrderPickerDetailDC>();
            using (AuthContext context = new AuthContext())
            {
                var manager = new PickerManager();
                var response = await manager.CheckerActivityOrderDetails(pickerId, ItemMultiMRPId);
                foreach (var item in response.OrderPickerDetailDCs)
                {
                    result.Add(new OrderPickerDetailDC
                    {
                        ItemMultiMrpId = item.ItemMultiMrpId,
                        Number = item.Number,
                        Barcode = item.Barcode,
                        IsFreeItem = item.IsFreeItem,
                        IsRTD = item.IsRTD,
                        MRP = item.MRP,
                        OrderId = item.OrderId,
                        OrderPickerMasterId = item.OrderPickerMasterId,
                        OrderPickerDetailBatchs = (response.OrderPickerDetailBatchDcs != null && response.OrderPickerDetailBatchDcs.Any()) ? response.OrderPickerDetailBatchDcs.Where(c => c.pickerDetailsId == item.pickerDetailsId).ToList() : null,
                        Comment = item.Comment,
                        pickerDetailsId = item.pickerDetailsId,
                        Qty = item.Qty,
                        Status = item.Status,
                        ItemName = item.ItemName
                    });
                }
            }
            return result;
        }




        //ForIncidentReport
        [HttpGet]
        [Route("GetInboundLeadForIncidentReport")]
        public async Task<List<People>> GetInboundLeadForIncidentReport(int WarehouseId)
        {
            var manager = new PickerManager();
            var people = await manager.GetInboundLeadForIncidentReport(WarehouseId);
            return people;
        }
        [HttpGet]
        [Route("GetOutboundLeadForIncidentReport")]
        public async Task<List<People>> GetOutboundLeadForIncidentReport(int WarehouseId)
        {
            var manager = new PickerManager();
            var people = await manager.GetOutboundLeadForIncidentReport(WarehouseId);
            return people;
        }
        [HttpGet]
        [Route("GetInboundLeadForIncidentReportwithoutWarehouseId")]
        public async Task<List<People>> GetInboundLeadForIncidentReportwithoutWarehouseId()
        {
            var manager = new PickerManager();
            var people = await manager.GetInboundLeadForIncidentReportwithoutWarehouseId();
            return people;
        }
        [HttpGet]
        [Route("GetOutboundLeadForIncidentReportwithoutWarehouseId")]
        public async Task<List<People>> GetOutboundLeadForIncidentReportwithoutWarehouseId()
        {
            var manager = new PickerManager();
            var people = await manager.GetOutboundLeadForIncidentReportwithoutWarehouseId();
            return people;
        }

        [HttpGet]
        [Route("GetWarehousewisePickerList")]
        public async Task<List<OrderPickerMasterDc>> GetWarehousewisePickerList(int WarehouseId)
        {
            using (AuthContext context = new AuthContext())
            {
                List<OrderPickerMaster> pickerList = new List<OrderPickerMaster>();
                var predicate = PredicateBuilder.New<OrderPickerMaster>(x => x.WarehouseId == WarehouseId && x.IsPickerGrabbed == false && x.IsDeleted == false);

                List<OrderPickerMaster> pickerlist = context.OrderPickerMasterDb.Where(predicate).OrderByDescending(x => x.Id).Include("OrderPickerDetails").ToList();

                var peoplecreatedby = pickerlist.Select(x => x.CreatedBy).Distinct().ToList();
                var dboyids = pickerlist.Select(x => x.DBoyId).Distinct().ToList();
                var createdname = context.Peoples.Where(x => peoplecreatedby.Contains(x.PeopleID) || dboyids.Contains(x.PeopleID)).ToList();

                var newPickerData = new List<OrderPickerMasterDc>();
                newPickerData = Mapper.Map(pickerlist).ToANew<List<OrderPickerMasterDc>>();
                foreach (var item in newPickerData)
                {
                    var MultiMrpIds = item.orderPickerDetails.Select(x => x.ItemMultiMrpId).Distinct().ToList();
                    var MultiMrpList = context.ItemMultiMRPDB.Where(x => MultiMrpIds.Contains(x.ItemMultiMRPId)).Distinct().ToList();
                    double amount = 0;
                    var orderids = item.orderPickerDetails.Where(y => y.Status != 3).Select(y => y.OrderId).Distinct().ToList();
                    var clusters = context.DbOrderMaster.Where(x => orderids.Contains(x.OrderId)).Select(x => x.ClusterName).Distinct().ToList();
                    var dboyname = createdname.Where(x => x.PeopleID == item.DboyId).FirstOrDefault()?.DisplayName;
                    var orderDetails = item.orderPickerDetails.Where(x => x.Status != 3).Select(x => new { x.OrderDetailsId, x.Qty }).ToList();
                    var orderdetailIds = orderDetails.Select(x => x.OrderDetailsId).ToList();
                    var dbOrderDetails = context.DbOrderDetails.Where(x => orderdetailIds.Contains(x.OrderDetailsId)).Select(x => new { x.OrderDetailsId, x.UnitPrice }).ToList();
                    dbOrderDetails.ForEach(x =>
                    {
                        amount += orderDetails.Where(y => y.OrderDetailsId == x.OrderDetailsId).Sum(y => y.Qty * x.UnitPrice);
                    });
                    foreach (var lineitems in item.orderPickerDetails)
                    {
                        var mrp = MultiMrpList.Where(x => x.ItemMultiMRPId == lineitems.ItemMultiMrpId).Select(x => x.MRP).FirstOrDefault();
                        lineitems.MRP = mrp > 0 ? mrp : 0;
                    }
                    item.CreatedByName = createdname.Where(x => x.PeopleID == item.CreatedBy).Select(x => x.DisplayName).FirstOrDefault();
                    item.orderpickercount = item.orderPickerDetails.GroupBy(x => new { x.ItemMultiMrpId, x.OrderId }).Count();
                    item.orderpickercountitem += item.orderPickerDetails.Select(x => x.Qty).Sum();
                    item.TotalAssignmentAmount = amount;
                    item.ClusterName = string.Join(",", clusters);
                    item.DBoyName = dboyname;
                }
                return newPickerData;
            }
        }

        [Route("AcceptPicker")]
        [HttpGet]
        public async Task<bool> AcceptPicker(int PeopleId, int PickerId, bool IsPickerGrabbed)
        {
            using (var context = new AuthContext())
            {
                var acceptstatus = context.OrderPickerMasterDb.FirstOrDefault(x => x.Id == PickerId && x.IsPickerGrabbed == false && x.IsDeleted == false && x.IsCanceled == false);

                if (acceptstatus != null && IsPickerGrabbed == true && PeopleId > 0)
                {
                    acceptstatus.PickerPersonId = PeopleId;
                    acceptstatus.IsPickerGrabbed = true;
                    acceptstatus.PickerGrabbedTime = indianTime;
                    acceptstatus.ModifiedDate = indianTime;
                    acceptstatus.ModifiedBy = PeopleId;
                    context.Commit();
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        [HttpGet]
        [Route("GetApprovedPickerList")]
        public async Task<paggingDTO> GetApprovedPickerList(int WarehouseId, int ClusterId, int skip, int take)
        {
            using (AuthContext context = new AuthContext())
            {
                List<OrderPickerMaster> pickerList = new List<OrderPickerMaster>();
                paggingDTO addlist = new paggingDTO();
                int counts = 0;
                if (ClusterId == 0)
                {
                    var predicate = PredicateBuilder.New<OrderPickerMaster>(x => x.WarehouseId == WarehouseId && x.IsPickerGrabbed == true && x.IsCheckerGrabbed == true && x.IsApproved == true && x.IsDispatched == true && x.IsDeleted == false && x.Status != 6);

                    List<OrderPickerMaster> pickerlist = context.OrderPickerMasterDb.Where(predicate).OrderByDescending(x => x.Id).Skip((skip - 1) * take).Take(take).Include("OrderPickerDetails").ToList();
                    counts = context.OrderPickerMasterDb.Where(predicate).OrderByDescending(x => x.Id).Count();

                    var peoplecreatedby = pickerlist.Select(x => x.CreatedBy).Distinct().ToList();
                    var pickerid = pickerlist.Select(x => x.PickerPersonId).Distinct().ToList();
                    var DboyId = pickerlist.Select(x => x.DBoyId).Distinct().ToList();
                    var agentId = pickerlist.Select(x => x.AgentId).Distinct().ToList();
                    var people = context.Peoples.Where(x =>
                       peoplecreatedby.Contains(x.PeopleID)
                    || pickerid.Contains(x.PeopleID)
                    || DboyId.Contains(x.PeopleID)
                    || agentId.Contains(x.PeopleID)
                    ).ToList();


                    // var pickername = context.Peoples.Where(x => pickerid.Contains(x.PeopleID)).ToList();                 
                    // var dBoyName = context.Peoples.Where(x => DboyId.Contains(x.PeopleID)).ToList();
                    //var dboymobile = context.Peoples.Where(x => DboyId.Contains(x.PeopleID)).ToList();

                    //var agentName = context.Peoples.Where(x => agentId.Contains(x.PeopleID)).ToList();

                    var newPickerData = new List<OrderPickerMasterDc>();
                    newPickerData = Mapper.Map(pickerlist).ToANew<List<OrderPickerMasterDc>>();

                    var OrderId = newPickerData.SelectMany(x => x.orderPickerDetails.Select(y => y.OrderId).Distinct()).ToList();
                    // var AssingmentOrder = context.OrderDispatchedMasters.Where(x => OrderId.Contains(x.OrderId)).Select(x => new { x.DeliveryIssuanceIdOrderDeliveryMaster, x.OrderId }).ToList();
                    foreach (var item in newPickerData)
                    {
                        var MultiMrpIds = item.orderPickerDetails.Select(x => x.ItemMultiMrpId).Distinct().ToList();
                        var orderIds = item.orderPickerDetails.Select(x => x.OrderId).Distinct().ToList();
                        var MultiMrpList = context.ItemMultiMRPDB.Where(x => MultiMrpIds.Contains(x.ItemMultiMRPId)).Distinct().ToList();
                        //if (item.DeliveryIssuanceId == null)
                        //{
                        //    //int? assingmentId = AssingmentOrder.Where(x => orderIds.Contains(x.OrderId)).Select(x => x.DeliveryIssuanceIdOrderDeliveryMaster).FirstOrDefault() ?? null;
                        //    //if (assingmentId != null)
                        //    //{
                        //        item.DeliveryIssuanceId = item.DeliveryIssuanceId;
                        //    //}
                        //}
                        item.DeliveryIssuanceId = item.DeliveryIssuanceId;
                        item.CreatedByName = people.Where(x => x.PeopleID == item.CreatedBy).Select(x => x.DisplayName).FirstOrDefault();
                        item.PickerPersonName = people.Where(x => x.PeopleID == item.PickerPersonId).Select(x => x.DisplayName).FirstOrDefault();
                        item.orderpickercount = item.orderPickerDetails.Select(x => x.OrderId).Distinct().ToList().Count();
                        item.orderpickercountitem += item.orderPickerDetails.Select(x => x.Qty).Sum();
                        item.DBoyName = people.Where(x => x.PeopleID == item.DboyId).Select(x => x.DisplayName).FirstOrDefault();
                        item.DboyMobile = people.Where(x => x.PeopleID == item.DboyId).Select(x => x.Mobile).FirstOrDefault();
                        item.AgentName = people.Where(x => x.PeopleID == item.AgentId).Select(x => x.DisplayName).FirstOrDefault();
                        item.DboyId = item.DboyId ?? 0;
                        item.AgentId = item.AgentId ?? 0;

                        var AddDatalists = item.orderPickerDetails.Where(z => z.Qty > 0).GroupBy(x => new { x.ItemMultiMrpId, x.Status }).Select(x => new OrderPickerDetailsDc
                        {
                            id = x.Select(w => w.id).FirstOrDefault(),
                            ItemMultiMrpId = x.Key.ItemMultiMrpId,
                            Qty = x.Sum(y => y.Qty),
                            ItemName = x.Select(w => w.ItemName).FirstOrDefault(),
                            OrderId = x.Select(w => w.OrderId).FirstOrDefault(),
                            OrderDetailsId = x.Select(w => w.OrderDetailsId).FirstOrDefault(),
                            DispatchedQty = x.Select(w => w.DispatchedQty).FirstOrDefault(),
                            Comment = x.Select(w => w.Comment).FirstOrDefault(),
                            IsFreeItem = x.Select(w => w.IsFreeItem).FirstOrDefault(),
                            Status = x.Select(w => w.Status).Distinct().FirstOrDefault(),
                            Orderids = string.Join(",", item.orderPickerDetails.Where(z => z.ItemMultiMrpId == x.Key.ItemMultiMrpId && z.Status == x.Key.Status).Select(e => e.OrderId).ToList()),
                        }).ToList();

                        item.orderPickerDetails = AddDatalists.OrderBy(x => x.ItemMultiMrpId).ToList();
                        foreach (var lineitems in item.orderPickerDetails)
                        {
                            lineitems.MRP = MultiMrpList.FirstOrDefault(x => x.ItemMultiMRPId == lineitems.ItemMultiMrpId).MRP;
                        }
                    }
                    addlist.PickerList = newPickerData;
                    addlist.Totalcount = counts;
                    return addlist;
                }
                else
                {
                    var predicate = PredicateBuilder.New<OrderPickerMaster>(x => x.ClusterId == ClusterId && x.IsPickerGrabbed == true && x.IsCheckerGrabbed == true && x.IsApproved == true && x.IsDispatched == false && x.IsDeleted == false);

                    List<OrderPickerMaster> pickerlist = context.OrderPickerMasterDb.Where(predicate).OrderByDescending(x => x.Id).Skip((skip - 1) * take).Take(take).Include("OrderPickerDetails").ToList();
                    counts = context.OrderPickerMasterDb.Where(predicate).OrderByDescending(x => x.Id).Count();

                    var peoplecreatedby = pickerlist.Select(x => x.CreatedBy).Distinct().ToList();
                    var createdname = context.Peoples.Where(x => peoplecreatedby.Contains(x.PeopleID)).ToList();
                    var pickerid = pickerlist.Select(x => x.PickerPersonId).Distinct().ToList();
                    var pickername = context.Peoples.Where(x => pickerid.Contains(x.PeopleID)).ToList();
                    var DboyId = pickerlist.Select(x => x.DBoyId).Distinct().ToList();
                    var dBoyName = context.Peoples.Where(x => DboyId.Contains(x.PeopleID)).ToList();
                    var agentId = pickerlist.Select(x => x.AgentId).Distinct().ToList();
                    var agentName = context.Peoples.Where(x => agentId.Contains(x.PeopleID)).ToList();

                    var newPickerData = new List<OrderPickerMasterDc>();
                    newPickerData = Mapper.Map(pickerlist).ToANew<List<OrderPickerMasterDc>>();
                    foreach (var item in newPickerData)
                    {
                        var MultiMrpIds = item.orderPickerDetails.Select(x => x.ItemMultiMrpId).Distinct().ToList();
                        var MultiMrpList = context.ItemMultiMRPDB.Where(x => MultiMrpIds.Contains(x.ItemMultiMRPId)).Distinct().ToList();
                        item.CreatedByName = createdname.Where(x => x.PeopleID == item.CreatedBy).Select(x => x.DisplayName).FirstOrDefault();
                        item.PickerPersonName = pickername.Where(x => x.PeopleID == item.PickerPersonId).Select(x => x.DisplayName).FirstOrDefault();
                        item.orderpickercount = item.orderPickerDetails.Select(x => x.OrderId).Distinct().ToList().Count();
                        item.orderpickercountitem += item.orderPickerDetails.Select(x => x.Qty).Sum();
                        item.DBoyName = dBoyName.Where(x => x.PeopleID == item.DboyId).Select(x => x.DisplayName).FirstOrDefault();
                        item.AgentName = agentName.Where(x => x.PeopleID == item.AgentId).Select(x => x.DisplayName).FirstOrDefault();
                        item.DboyId = item.DboyId ?? 0;
                        item.AgentId = item.AgentId ?? 0;
                        var AddDatalists = item.orderPickerDetails.Where(z => z.Qty > 0).GroupBy(x => new { x.ItemMultiMrpId, x.Status }).Select(x => new OrderPickerDetailsDc
                        {
                            id = x.Select(w => w.id).FirstOrDefault(),
                            ItemMultiMrpId = x.Key.ItemMultiMrpId,
                            Qty = x.Sum(y => y.Qty),
                            ItemName = x.Select(w => w.ItemName).FirstOrDefault(),
                            OrderId = x.Select(w => w.OrderId).FirstOrDefault(),
                            OrderDetailsId = x.Select(w => w.OrderDetailsId).FirstOrDefault(),
                            DispatchedQty = x.Select(w => w.DispatchedQty).FirstOrDefault(),
                            Comment = x.Select(w => w.Comment).FirstOrDefault(),
                            IsFreeItem = x.Select(w => w.IsFreeItem).FirstOrDefault(),
                            Status = x.Select(w => w.Status).Distinct().FirstOrDefault(),
                            Orderids = string.Join(",", item.orderPickerDetails.Where(z => z.ItemMultiMrpId == x.Key.ItemMultiMrpId && z.Status == x.Key.Status).Select(e => e.OrderId).ToList()),
                        }).ToList();
                        item.orderPickerDetails = AddDatalists.OrderBy(x => x.ItemMultiMrpId).ToList();
                        foreach (var lineitems in item.orderPickerDetails)
                        {
                            lineitems.MRP = MultiMrpList.FirstOrDefault(x => x.ItemMultiMRPId == lineitems.ItemMultiMrpId).MRP;
                        }
                    }
                    addlist.PickerList = newPickerData;
                    addlist.Totalcount = counts;
                    return addlist;
                }
            }
        }

        [HttpGet]
        [Route("WarehousewiseChekerPickList")]
        public async Task<List<OrderPickerMasterDc>> WarehousewiseChekerPickList(int WarehouseId)
        {
            using (AuthContext context = new AuthContext())
            {
                List<OrderPickerMaster> pickerList = new List<OrderPickerMaster>();
                var predicate = PredicateBuilder.New<OrderPickerMaster>(x => x.WarehouseId == WarehouseId && x.IsInventorySupervisor == true && x.IsInventorySupervisorStart == true && x.IsCheckerGrabbed == false && x.IsComplted == true && x.IsDeleted == false);

                List<OrderPickerMaster> pickerlist = context.OrderPickerMasterDb.Where(predicate).OrderByDescending(x => x.Id).Include("OrderPickerDetails").ToList();

                var peoplecreatedby = pickerlist.Select(x => x.CreatedBy).Distinct().ToList();
                var dboyids = pickerlist.Select(x => x.DBoyId).Distinct().ToList();
                var createdname = context.Peoples.Where(x => peoplecreatedby.Contains(x.PeopleID) || dboyids.Contains(x.PeopleID)).ToList();

                var newPickerData = new List<OrderPickerMasterDc>();
                newPickerData = Mapper.Map(pickerlist).ToANew<List<OrderPickerMasterDc>>();
                foreach (var item in newPickerData)
                {
                    var MultiMrpIds = item.orderPickerDetails.Select(x => x.ItemMultiMrpId).Distinct().ToList();
                    var MultiMrpList = context.ItemMultiMRPDB.Where(x => MultiMrpIds.Contains(x.ItemMultiMRPId)).Distinct().ToList();
                    double amount = 0;
                    var orderids = item.orderPickerDetails.Where(y => y.Status != 3).Select(y => y.OrderId).Distinct().ToList();
                    var clusters = context.DbOrderMaster.Where(x => orderids.Contains(x.OrderId)).Select(x => x.ClusterName).Distinct().ToList();
                    var dboyname = createdname.Where(x => x.PeopleID == item.DboyId).FirstOrDefault()?.DisplayName;
                    var orderDetails = item.orderPickerDetails.Where(x => x.Status != 3).Select(x => new { x.OrderDetailsId, x.Qty }).ToList();
                    var orderdetailIds = orderDetails.Select(x => x.OrderDetailsId).ToList();
                    var dbOrderDetails = context.DbOrderDetails.Where(x => orderdetailIds.Contains(x.OrderDetailsId)).Select(x => new { x.OrderDetailsId, x.UnitPrice }).ToList();
                    dbOrderDetails.ForEach(x =>
                    {
                        amount += orderDetails.Where(y => y.OrderDetailsId == x.OrderDetailsId).Sum(y => y.Qty * x.UnitPrice);
                    });
                    foreach (var lineitems in item.orderPickerDetails)
                    {
                        lineitems.MRP = MultiMrpList.FirstOrDefault(x => x.ItemMultiMRPId == lineitems.ItemMultiMrpId).MRP;
                    }
                    item.CreatedByName = createdname.Where(x => x.PeopleID == item.CreatedBy).Select(x => x.DisplayName).FirstOrDefault();
                    item.orderpickercount = item.orderPickerDetails.Select(x => x.OrderId).Distinct().ToList().Count();
                    item.orderpickercountitem += item.orderPickerDetails.Select(x => x.Qty).Sum();
                    item.TotalAssignmentAmount = amount;
                    item.ClusterName = string.Join(",", clusters);
                    item.DBoyName = dboyname;
                }
                return newPickerData;
            }
        }

        [Route("CheckerAcceptPicker")]
        [HttpGet]
        public async Task<bool> CheckerAcceptPicker(int PeopleId, int PickerId, bool IsCheckerGrabbed)
        {
            using (var context = new AuthContext())
            {
                var acceptstatus = context.OrderPickerMasterDb.FirstOrDefault(x => x.Id == PickerId && x.IsCheckerGrabbed == false && x.IsComplted == true && x.IsDeleted == false && x.IsCanceled == false);

                if (acceptstatus != null && IsCheckerGrabbed == true && PeopleId > 0)
                {
                    acceptstatus.ApproverId = PeopleId;
                    acceptstatus.IsCheckerGrabbed = true;
                    acceptstatus.CheckerGrabbedTime = indianTime;
                    acceptstatus.ModifiedDate = indianTime;
                    acceptstatus.ModifiedBy = PeopleId;
                    context.Commit();
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        [Route("GetPickerOrderdetails")]
        [HttpGet]
        public async Task<HttpResponseMessage> getbyId(int PickerOrderId)
        {
            using (var context = new AuthContext())
            {

                var identity = User.Identity as ClaimsIdentity;
                int compid = 0;

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))
                    compid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);
                //var DBoyorders = context.getdboysOrder(mob, compid);
                bool IsZaruriOrder = false;
                List<OrderDispatchedMaster> finalList = new List<OrderDispatchedMaster>();
                List<OrderDispatchedMaster> list = new List<OrderDispatchedMaster>();
                List<int> noteligibleOrder = new List<int>();
                List<int> founndList = new List<int>();
                var predicate = PredicateBuilder.New<OrderPickerMaster>(x => x.Id == PickerOrderId && x.IsPickerGrabbed == true && x.IsCheckerGrabbed == true && x.IsApproved == true && x.IsDispatched == true && x.IsDeleted == false && x.Status != 6);
                List<OrderPickerMaster> pickerlist = context.OrderPickerMasterDb.Where(predicate).OrderByDescending(x => x.Id).Include("OrderPickerDetails").ToList();
                var newPickerData = new List<OrderPickerMasterDc>();
                newPickerData = Mapper.Map(pickerlist).ToANew<List<OrderPickerMasterDc>>();

                var OrderId = newPickerData.SelectMany(x => x.orderPickerDetails.Select(y => y.OrderId).Distinct()).ToList();
                var AllPendingAssigment = context.OrderDispatchedMasters.Where(a => OrderId.Contains(a.OrderId) && a.Deleted == false).Select(x => new { x.WarehouseId, x.DboyMobileNo, x.OrderDispatchedMasterId, x.OrderId }).ToList();
                if (AllPendingAssigment.Any())
                {
                    var OrderDispatchMasterIds = AllPendingAssigment.Select(x => x.OrderDispatchedMasterId).ToList();
                    list = context.OrderDispatchedMasters.Where(x => OrderDispatchMasterIds.Contains(x.OrderDispatchedMasterId)).Include("orderDetails").ToList();
                    var OrderIds = list.Select(x => x.OrderId).ToList();
                    var ordermasters = context.DbOrderMaster.Where(a => OrderIds.Contains(a.OrderId)).Select(x => new { x.OrderId, x.OrderType, x.IsPrimeCustomer });
                    IsZaruriOrder = ordermasters.Any(a => a.OrderType == 5);
                    list.ForEach(x =>
                    {
                        if (ordermasters.Any(y => y.OrderId == x.OrderId))
                        {
                            x.OrderType = ordermasters.FirstOrDefault(y => y.OrderId == x.OrderId).OrderType;
                            x.IsPrimeCustomer = ordermasters.FirstOrDefault(y => y.OrderId == x.OrderId).IsPrimeCustomer;

                        }
                    });
                    if (ordermasters.Any(a => a.OrderType == 4))
                    {
                        var RDSOrderIds = ordermasters.Where(a => a.OrderType == 4).Select(x => x.OrderId).ToList();
                        var ordePayments = context.PaymentResponseRetailerAppDb.Where(a => RDSOrderIds.Contains(a.OrderId) && a.PaymentFrom == "Gullak"
                                                          && a.status == "Success").ToList();

                        var RDSOrders = list.Where(x => RDSOrderIds.Contains(x.OrderId));

                        foreach (var item in RDSOrders)
                        {
                            var amount = ordePayments.Where(x => x.OrderId == item.OrderId).Sum(x => x.amount);
                            if (amount != item.GrossAmount)
                            {
                                noteligibleOrder.Add(item.OrderId);
                            }
                        }
                    }
                }

                if (list.Any())
                {
                    var Asigmentids = list.Where(x => x.DeliveryIssuanceIdOrderDeliveryMaster > 0).Select(x => x.DeliveryIssuanceIdOrderDeliveryMaster).Distinct().ToList();
                    if (Asigmentids != null && Asigmentids.Any())
                    {
                        founndList = context.DeliveryIssuanceDb.Where(x => Asigmentids.Contains(x.DeliveryIssuanceId) && (x.Status == "Submitted" || x.Status == "Payment Accepted" || x.Status == "Pending")).Select(x => x.DeliveryIssuanceId).ToList();
                    }

                    var redispatchOrderIds = list.Where(x => x.ReDispatchCount > 0).Select(x => x.OrderId).ToList();
                    var orderredispatchdatas = context.orderRedispatchCountApprovalDB.Where(x => redispatchOrderIds.Contains(x.OrderId)).Select(x => new { OrderId = x.OrderId, x.Id, IsApproved = x.IsApproved }).ToList().GroupBy(x => x.OrderId).Select(x => new { OrderId = x.Key, IsApproved = x.OrderByDescending(y => y.Id).LastOrDefault().IsApproved }).ToList();

                    var redispatchedcharge = new List<DeliveryRedispatchChargeConfDc>();
                    //for DeliveryRedispatched Charge
                    if (list.Any(x => x.ReDispatchCount > 0))
                    {
                        var CityId = list.Select(x => x.CityId).FirstOrDefault();
                        var result = context.DeliveryRedispatchChargeConfs.Where(x => x.IsActive == true && x.IsDeleted == false && x.CityId == CityId).ToList();
                        redispatchedcharge = Mapper.Map(result).ToANew<List<DeliveryRedispatchChargeConfDc>>();
                    }

                    foreach (var ast in list)//.Where(x=> !noteligibleOrder.Contains(x.OrderId)))
                    {
                        if (IsZaruriOrder)
                        {
                            ast.OrderType = 5;
                        }
                        else if (ast.OrderType != 4)
                        {
                            ast.OrderType = 1;
                        }
                        else if (ast.OrderType == 4 && noteligibleOrder.Any(x => x == ast.OrderId))
                        {
                            ast.IsNotCreateAssingment = true;
                        }

                        var founnd = founndList.Any(x => x == ast.DeliveryIssuanceIdOrderDeliveryMaster);
                        if (!founnd)
                        {
                            if (ast.ReDispatchCount > 0)
                            {
                                var orderredispatchdata = orderredispatchdatas.FirstOrDefault(x => x.OrderId == ast.OrderId);
                                if (orderredispatchdata != null)
                                {
                                    ast.SendApproval = true;
                                    if (orderredispatchdata.IsApproved == true)
                                    {
                                        ast.IsApproved = orderredispatchdata.IsApproved;
                                    }
                                }
                            }
                            finalList.Add(ast);
                        }
                    }
                    //for distance Add by anushka (20/01/2020)
                    var customerids = finalList.Select(x => x.CustomerId).Distinct().ToList();
                    var warehouseids = finalList.Select(x => x.WarehouseId).Distinct().ToList();
                    CustomersManager manager = new CustomersManager();
                    var customerWarehouseLtlng = manager.GetCustomerOrder(customerids, warehouseids);
                    finalList.ForEach(x =>
                    {
                        if (customerWarehouseLtlng != null && customerWarehouseLtlng.CustomerLtlng != null && customerWarehouseLtlng.WarehouseLtlng != null)
                        {
                            var customerltlng = customerWarehouseLtlng.CustomerLtlng.FirstOrDefault(y => x.CustomerId == y.Id);
                            var warehouseltlng = customerWarehouseLtlng.WarehouseLtlng.FirstOrDefault(y => x.WarehouseId == y.Id);

                            if (warehouseltlng != null && warehouseltlng.lat != 0 && warehouseltlng.lg != 0 && customerltlng != null && customerltlng.lat != 0 && customerltlng.lat >= -180.0 && customerltlng.lat <= 180.0 && customerltlng.lg != 0 && customerltlng.lg >= -180.0 && customerltlng.lg <= 180.0)
                            {
                                var sourceGeoCordinates = new System.Device.Location.GeoCoordinate(warehouseltlng.lat, warehouseltlng.lg);
                                var destination = new System.Device.Location.GeoCoordinate(customerltlng.lat, customerltlng.lg);
                                var dist = GeoHelper.AerialDistance(sourceGeoCordinates, destination);
                                x.Distance = Math.Round(dist, 2);
                            }
                        }
                        if (x.ReDispatchCount > 0 && redispatchedcharge != null && redispatchedcharge.Any() && redispatchedcharge.Any(s => s.RedispatchCount == x.ReDispatchCount))
                        {
                            x.deliveryCharge += redispatchedcharge.FirstOrDefault(s => s.RedispatchCount == x.ReDispatchCount).RedispatchCharge;
                        }
                    });
                    //end
                    //return finalList;
                }

                finalList = finalList.OrderBy(x => x.Distance).ThenBy(x => x.CustomerId).ThenBy(x => x.CreatedDate).ToList();

                return Request.CreateResponse(HttpStatusCode.OK, finalList);
            }
        }


        [Route("GetRedispatchorder")]
        [HttpGet]
        public async Task<HttpResponseMessage> getbyId(string mob)
        {
            using (var context = new AuthContext())
            {
                var identity = User.Identity as ClaimsIdentity;
                int compid = 0;

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))
                    compid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);
                //var DBoyorders = context.getdboysOrder(mob, compid);
                bool IsZaruriOrder = false;
                List<OrderDispatchedMaster> finalList = new List<OrderDispatchedMaster>();
                List<OrderDispatchedMaster> list = new List<OrderDispatchedMaster>();
                List<int> noteligibleOrder = new List<int>();
                List<int> founndList = new List<int>();

                var people = context.Peoples.Where(x => x.Mobile == mob && x.Deleted == false && x.Active == true && x.WarehouseId > 0).FirstOrDefault();
                int warehouseid = people.WarehouseId;
                if (warehouseid > 0)
                {
                    var AllPendingAssigment = context.OrderDispatchedMasters.Where(a => a.WarehouseId == warehouseid && a.Status == "Delivery Redispatch" && a.Deleted == false).Select(x => new { x.WarehouseId, x.DboyMobileNo, x.OrderDispatchedMasterId, x.OrderId }).ToList();
                    //.Include("orderDetails").ToList();
                    var DboylistAssignment = AllPendingAssigment.Where(a => a.DboyMobileNo == mob).ToList();
                    if (DboylistAssignment.Any())
                    {
                        var OrderDispatchMasterIds = DboylistAssignment.Select(x => x.OrderDispatchedMasterId).ToList();
                        list = context.OrderDispatchedMasters.Where(x => OrderDispatchMasterIds.Contains(x.OrderDispatchedMasterId)).Include("orderDetails").ToList();
                        var OrderIds = list.Select(x => x.OrderId).ToList();
                        var ordermasters = context.DbOrderMaster.Where(a => OrderIds.Contains(a.OrderId)).Select(x => new { x.OrderId, x.OrderType, x.IsPrimeCustomer });
                        IsZaruriOrder = ordermasters.Any(a => a.OrderType == 5);
                        list.ForEach(x =>
                        {
                            if (ordermasters.Any(y => y.OrderId == x.OrderId))
                            {
                                x.OrderType = ordermasters.FirstOrDefault(y => y.OrderId == x.OrderId).OrderType;
                                x.IsPrimeCustomer = ordermasters.FirstOrDefault(y => y.OrderId == x.OrderId).IsPrimeCustomer;

                            }
                        });

                        if (ordermasters.Any(a => a.OrderType == 4))
                        {
                            var RDSOrderIds = ordermasters.Where(a => a.OrderType == 4).Select(x => x.OrderId).ToList();
                            var ordePayments = context.PaymentResponseRetailerAppDb.Where(a => RDSOrderIds.Contains(a.OrderId) && a.PaymentFrom == "Gullak"
                                                              && a.status == "Success").ToList();

                            var RDSOrders = list.Where(x => RDSOrderIds.Contains(x.OrderId));

                            foreach (var item in RDSOrders)
                            {
                                var amount = ordePayments.Where(x => x.OrderId == item.OrderId).Sum(x => x.amount);
                                if (amount != item.GrossAmount)
                                {
                                    noteligibleOrder.Add(item.OrderId);
                                }
                            }
                        }
                    }

                    if (list.Any())
                    {
                        var Asigmentids = list.Where(x => x.DeliveryIssuanceIdOrderDeliveryMaster > 0).Select(x => x.DeliveryIssuanceIdOrderDeliveryMaster).Distinct().ToList();
                        if (Asigmentids != null && Asigmentids.Any())
                        {
                            founndList = context.DeliveryIssuanceDb.Where(x => Asigmentids.Contains(x.DeliveryIssuanceId) && (x.Status == "Submitted" || x.Status == "Payment Accepted" || x.Status == "Pending")).Select(x => x.DeliveryIssuanceId).ToList();
                        }

                        var redispatchOrderIds = list.Where(x => x.ReDispatchCount > 0).Select(x => x.OrderId).ToList();
                        var orderredispatchdatas = context.orderRedispatchCountApprovalDB.Where(x => redispatchOrderIds.Contains(x.OrderId)).Select(x => new { OrderId = x.OrderId, x.Id, IsApproved = x.IsApproved, Redispatchcount = x.Redispatchcount, CreatedDate = x.CreatedDate }).ToList();

                        var redispatchedcharge = new List<DeliveryRedispatchChargeConfDc>();
                        //for DeliveryRedispatched Charge
                        if (list.Any(x => x.ReDispatchCount > 0))
                        {
                            var CityId = list.Select(x => x.CityId).FirstOrDefault();
                            var result = context.DeliveryRedispatchChargeConfs.Where(x => x.IsActive == true && x.IsDeleted == false && x.CityId == CityId).ToList();
                            redispatchedcharge = Mapper.Map(result).ToANew<List<DeliveryRedispatchChargeConfDc>>();
                        }


                        foreach (var ast in list)//.Where(x=> !noteligibleOrder.Contains(x.OrderId)))
                        {
                            if (IsZaruriOrder)
                            {
                                ast.OrderType = 5;
                            }
                            else if (ast.OrderType != 4)
                            {
                                ast.OrderType = 1;
                            }
                            else if (ast.OrderType == 4 && noteligibleOrder.Any(x => x == ast.OrderId))
                            {
                                ast.IsNotCreateAssingment = true;
                            }
                            if (ast.Status == "Delivery Redispatch")
                            {
                                var ComfrimReDispatched = context.DeliveryCanceledRequestHistoryDb.Where(x => x.OrderId == ast.OrderId && x.IsActive == true && x.IsDeleted == false && x.ConformationDate != null).FirstOrDefault();
                                ast.IsCreateAssingmentReDispatched = false;
                                ast.IsNotCreateAssingmentAwaitingReDispatchedOrderId = false;
                                if (ComfrimReDispatched != null)
                                {
                                    DateTime aDate = DateTime.Now;

                                    DateTime Comfrimdata = Convert.ToDateTime(ComfrimReDispatched.ConformationDate?.ToString("yyyy-MM-dd"));
                                    DateTime todaydate = DateTime.Now;

                                    if (todaydate > Comfrimdata)
                                    {
                                        ast.IsCreateAssingmentReDispatched = true;
                                    }
                                    else
                                    {
                                        ast.IsNotCreateAssingmentAwaitingReDispatchedOrderId = true;
                                        ast.ConformationDate = ComfrimReDispatched.ConformationDate;
                                    }
                                }
                                var Callbackdata = context.DeliveryCanceledRequestHistoryDb.Where(x => x.OrderId == ast.OrderId && x.IsActive == true && x.IsDeleted == false && x.DeliveryCanceledStatus == "Call back").FirstOrDefault();
                                if (Callbackdata != null)
                                {
                                    ast.isCallbackCheck = true;
                                }
                            }
                            var founnd = founndList.Any(x => x == ast.DeliveryIssuanceIdOrderDeliveryMaster);
                            if (!founnd)
                            {
                                if (ast.ReDispatchCount > 0)
                                {
                                    var orderredispatchdata = orderredispatchdatas.OrderByDescending(x => x.CreatedDate).FirstOrDefault(x => x.OrderId == ast.OrderId && x.Redispatchcount == ast.ReDispatchCount);
                                    if (orderredispatchdata != null)
                                    {
                                        ast.SendApproval = true;
                                        if (orderredispatchdata.IsApproved == true)
                                        {
                                            ast.IsApproved = orderredispatchdata.IsApproved;
                                        }
                                    }
                                    else
                                    {
                                        ast.SendApproval = false;
                                        ast.IsApproved = false;
                                    }
                                }
                                finalList.Add(ast);
                            }
                        }

                        //for distance Add by anushka (20/01/2020)
                        var customerids = finalList.Select(x => x.CustomerId).Distinct().ToList();
                        var warehouseids = finalList.Select(x => x.WarehouseId).Distinct().ToList();
                        CustomersManager manager = new CustomersManager();
                        var customerWarehouseLtlng = manager.GetCustomerOrder(customerids, warehouseids);
                        finalList.ForEach(x =>
                        {
                            if (customerWarehouseLtlng != null && customerWarehouseLtlng.CustomerLtlng != null && customerWarehouseLtlng.WarehouseLtlng != null)
                            {
                                var customerltlng = customerWarehouseLtlng.CustomerLtlng.FirstOrDefault(y => x.CustomerId == y.Id);
                                var warehouseltlng = customerWarehouseLtlng.WarehouseLtlng.FirstOrDefault(y => x.WarehouseId == y.Id);

                                if (warehouseltlng != null && warehouseltlng.lat != 0 && warehouseltlng.lg != 0 && customerltlng != null && customerltlng.lat != 0 && customerltlng.lat >= -180.0 && customerltlng.lat <= 180.0 && customerltlng.lg != 0 && customerltlng.lg >= -180.0 && customerltlng.lg <= 180.0)
                                {
                                    var sourceGeoCordinates = new System.Device.Location.GeoCoordinate(warehouseltlng.lat, warehouseltlng.lg);
                                    var destination = new System.Device.Location.GeoCoordinate(customerltlng.lat, customerltlng.lg);
                                    var dist = GeoHelper.AerialDistance(sourceGeoCordinates, destination);
                                    x.Distance = Math.Round(dist, 2);
                                }
                            }
                            if (x.ReDispatchCount > 0 && redispatchedcharge != null && redispatchedcharge.Any() && redispatchedcharge.Any(s => s.RedispatchCount == x.ReDispatchCount))
                            {
                                x.deliveryCharge += redispatchedcharge.FirstOrDefault(s => s.RedispatchCount == x.ReDispatchCount).RedispatchCharge;
                            }
                        });
                    }
                }
                finalList = finalList.OrderBy(x => x.Distance).ThenBy(x => x.CustomerId).ThenBy(x => x.CreatedDate).ToList();
                return Request.CreateResponse(HttpStatusCode.OK, finalList);


            }





        }

        [Route("AcceptPickerInventorySupervisor")]
        [HttpGet]
        public async Task<HttpResponseMessage> AcceptPickerInventorySupervisor(int PeopleId, int PickerId)
        {
            ResMsg res;
            string Msg = "";
            bool status = true;
            using (var context = new AuthContext())
            {
                var acceptPicker = context.OrderPickerMasterDb.FirstOrDefault(x => x.Id == PickerId && x.IsPickerGrabbed == true && x.IsComplted == true && x.IsDeleted == false && x.IsCanceled == false);

                if (acceptPicker != null && PeopleId > 0)
                {
                    acceptPicker.IsInventory = true;
                    acceptPicker.InventoryGrabbedTime = indianTime;
                    acceptPicker.ModifiedDate = indianTime;
                    acceptPicker.ModifiedBy = PeopleId;
                    if (acceptPicker != null && !acceptPicker.IsCanceled)
                    {
                        var PickerTimer = context.PickerTimerDb.Where(x => x.OrderPickerMasterId == PickerId && x.Type == 2 && x.EndTime == null).ToList();
                        if (PickerTimer != null)
                        {
                            //End Pick
                            PickerTimer.ForEach(x =>
                            {
                                x.OrderPickerMasterId = acceptPicker.Id;
                                x.EndTime = indianTime;
                                x.CreatedBy = PeopleId;
                                context.Entry(x).State = EntityState.Modified;
                            });
                            acceptPicker.ModifiedDate = indianTime;
                            context.Entry(acceptPicker).State = EntityState.Modified;
                            context.Commit();
                            Msg = "#Picker No. " + acceptPicker.Id + "  Approved!!";
                            status = true;
                        }
                        else
                        {
                            status = false;
                            Msg = "Data Not Found!!";
                        }
                    }
                }
                else
                {
                    status = false;
                    Msg = "Picker Maker Side Still in Pending, you can't Approved";
                }
                res = new ResMsg()
                {
                    Status = status,
                    Message = Msg
                };
                return Request.CreateResponse(HttpStatusCode.OK, res);
            }
        }

        [HttpPost]
        [Route("InventorySupervisorStartPickJob")]
        public async Task<HttpResponseMessage> InventorySupervisorStartPickJob(int PickerId, int UserId)  //type = start / end
        {
            PickerTimer PickerTimers = new PickerTimer();
            ResMsg res;
            string Msg = "";
            bool status = false;
            if (PickerId > 0)
            {
                using (AuthContext context = new AuthContext())
                {
                    var OrderPickerMaster = context.OrderPickerMasterDb.Where(x => x.Id == PickerId && x.IsDeleted == false).FirstOrDefault();
                    if (OrderPickerMaster != null && !OrderPickerMaster.IsCanceled)
                    {

                        //bool PickListAnyInProcess = context.OrderPickerMasterDb.Any(x => x.ApproverId == PickerId && x.Id != OrderPickerMaster.Id && x.IsApproved == false && x.IsComplted && x.IsDeleted == false && x.IsCanceled == false);
                        bool PickListAnyInProcess = context.PickerTimerDb.Any(x => x.CreatedBy == UserId && x.Id != OrderPickerMaster.Id && x.Type == 2 && x.EndTime == null);

                        if (!PickListAnyInProcess)
                        {
                            var PickerTimer = context.PickerTimerDb.Where(x => x.OrderPickerMasterId == PickerId && x.Type == 2).ToList();
                            if (PickerTimer.Count == 0)
                            {
                                PickerTimers.OrderPickerMasterId = OrderPickerMaster.Id;
                                PickerTimers.StartTime = DateTime.Now;
                                PickerTimers.EndTime = null;
                                PickerTimers.CreatedBy = UserId;
                                PickerTimers.Type = 2;//InventorySupervisor                                
                                context.PickerTimerDb.Add(PickerTimers);
                                OrderPickerMaster.IsInventorySupervisorStart = true;
                                OrderPickerMaster.InventorySupervisorId = UserId;// InventorySupervisor Id, 
                                OrderPickerMaster.ModifiedDate = indianTime;
                                context.Entry(OrderPickerMaster).State = EntityState.Modified;

                                context.Commit();
                                Msg = "Picker Start Successfully";
                                status = true;
                            }
                            else if (PickerTimer.All(x => x.EndTime != null))
                            {
                                //Restart Pick
                                PickerTimers.OrderPickerMasterId = OrderPickerMaster.Id;
                                PickerTimers.StartTime = DateTime.Now;
                                PickerTimers.EndTime = null;
                                PickerTimers.CreatedBy = UserId;
                                PickerTimers.Type = 2;//InventorySupervisor
                                context.PickerTimerDb.Add(PickerTimers);
                                OrderPickerMaster.InventorySupervisorId = UserId;// InventorySupervisor id,
                                OrderPickerMaster.IsInventorySupervisorStart = true;
                                OrderPickerMaster.ModifiedDate = indianTime;
                                context.Entry(OrderPickerMaster).State = EntityState.Modified;
                                context.Commit();
                                Msg = "Picker Restart Successfully";
                                status = true;
                            }
                            else
                            {
                                PickerTimers = PickerTimer.Where(x => x.EndTime == null).FirstOrDefault();
                                status = true;
                            }
                        }
                        else
                        {
                            Msg = "पिछला पिकर प्रगति पर होने के कारण आप नया पिकर प्रारंभ नहीं कर सकते|";
                            status = true;
                        }
                    }
                    else
                    {
                        Msg = "Picker Canceled";
                        status = true;
                    }
                }
            }
            res = new ResMsg()
            {
                Status = status,
                Message = Msg
            };
            return Request.CreateResponse(HttpStatusCode.OK, res);
        }
        [Route("InventorySupervisorAcceptPicker")]
        [HttpGet]
        public async Task<bool> InventorySupervisorAcceptPicker(int PeopleId, int PickerId, bool IsInventorySupervisor)
        {
            using (var context = new AuthContext())
            {
                var acceptstatus = context.OrderPickerMasterDb.FirstOrDefault(x => x.Id == PickerId && x.IsInventorySupervisor == false && x.IsComplted == true && x.IsDeleted == false && x.IsCanceled == false);

                if (acceptstatus != null && IsInventorySupervisor == true && PeopleId > 0)
                {
                    acceptstatus.InventorySupervisorId = PeopleId;
                    acceptstatus.IsInventorySupervisor = true;
                    acceptstatus.InventoryGrabbedTime = indianTime;
                    acceptstatus.ModifiedDate = indianTime;
                    acceptstatus.ModifiedBy = PeopleId;
                    context.Commit();
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
        [HttpGet]
        [Route("GetInventorySupervisorOrderPickerMaster")]
        public async Task<List<PickerJobListDc>> GetInventorySupervisorOrderPickerMaster(int PeopleId)
        {
            List<PickerJobListDc> pickerJobList = new List<PickerJobListDc>();
            if (PeopleId > 0)
            {
                using (AuthContext context = new AuthContext())
                {
                    var wareouseid = context.Peoples.FirstOrDefault(x => x.PeopleID == PeopleId && x.Active && !x.Deleted).WarehouseId;
                    if (wareouseid > 0)
                    {
                        List<OrderPickerMaster> pickerlist = context.OrderPickerMasterDb.Where(x => x.WarehouseId == wareouseid && x.IsPickerGrabbed == true && x.InventorySupervisorId == PeopleId && x.IsComplted == true && x.IsCanceled == false && x.IsDeleted == false && x.IsDispatched == false && x.IsApproved == false && x.IsInventorySupervisor == true && x.IsInventory == false).OrderBy(x => x.Id).Include("OrderPickerDetails").ToList();

                        if (pickerlist != null && pickerlist.Any())
                        {

                            var createdBy = pickerlist.Select(x => x.CreatedBy).Distinct().ToList();
                            var submittedBy = pickerlist.Select(x => x.PickerPersonId).Distinct().ToList();
                            var dboyid = pickerlist.Select(x => x.DBoyId).Distinct().ToList();
                            var peopleIds = createdBy.Concat(submittedBy);

                            var peoplename = context.Peoples.Where(x => peopleIds.Contains(x.PeopleID)
                             || dboyid.Contains(x.PeopleID)
                            ).Distinct().ToList();
                            var Pickerids = pickerlist.Select(x => x.Id).Distinct().ToList();

                            List<PickerTimer> PickerTimerList = context.PickerTimerDb.Where(x => Pickerids.Contains(x.OrderPickerMasterId) && x.Type == 2 && x.EndTime == null).ToList();
                            foreach (var a in pickerlist)
                            {
                                double amount = 0;
                                var orderids = a.orderPickerDetails.Where(y => y.Status != 3).Select(y => y.OrderId).Distinct().ToList();
                                var cluster = context.DbOrderMaster.Where(x => orderids.Contains(x.OrderId)).Select(x => x.ClusterName).Distinct().ToList();
                                var dboyname = peoplename.Where(x => x.PeopleID == a.DBoyId).FirstOrDefault()?.DisplayName;
                                var orderDetails = a.orderPickerDetails.Where(x => x.Status != 3).Select(x => new { x.OrderDetailsId, x.Qty }).ToList();
                                var orderdetailIds = orderDetails.Select(x => x.OrderDetailsId).ToList();
                                var dbOrderDetails = context.DbOrderDetails.Where(x => orderdetailIds.Contains(x.OrderDetailsId)).Select(x => new { x.OrderDetailsId, x.UnitPrice }).ToList();
                                dbOrderDetails.ForEach(x =>
                                {
                                    amount += orderDetails.Where(y => y.OrderDetailsId == x.OrderDetailsId).Sum(y => y.Qty * x.UnitPrice);
                                });

                                var aaa = a.orderPickerDetails.GroupBy(x => x.ItemMultiMrpId).Count();
                                PickerJobListDc PickerJobListob = new PickerJobListDc();
                                PickerJobListob.Id = a.Id;
                                PickerJobListob.CreatedDate = a.CreatedDate;
                                PickerJobListob.count = aaa;
                                PickerJobListob.IsApproved = a.IsApproved;
                                PickerJobListob.Commenet = a.Comment;
                                PickerJobListob.IsComplted = a.IsComplted;
                                PickerJobListob.CreatedBy = peoplename.Where(x => x.PeopleID == a.CreatedBy).Select(x => x.DisplayName).FirstOrDefault();
                                PickerJobListob.SubmittedBy = peoplename.Where(x => x.PeopleID == a.PickerPersonId).Select(x => x.DisplayName).FirstOrDefault();
                                PickerJobListob.SubmittedDate = a.SubmittedDate;
                                PickerJobListob.amount = Math.Round(amount, 0);
                                PickerJobListob.dboyName = dboyname == null ? "" : dboyname;
                                PickerJobListob.ClusterName = string.Join(",", cluster);
                                PickerJobListob.IsInventorySupervisor = a.IsInventorySupervisor;
                                PickerJobListob.IsInventory = a.IsInventory;
                                PickerJobListob.IsInventorySupervisorStart = a.IsInventorySupervisorStart;
                                if (PickerTimerList != null && PickerTimerList.Any() && PickerTimerList.Any(x => x.OrderPickerMasterId == a.Id && x.EndTime == null))
                                {
                                    PickerJobListob.StartTime = PickerTimerList.FirstOrDefault(x => x.OrderPickerMasterId == a.Id && x.EndTime == null).StartTime;
                                }
                                else
                                {
                                    PickerJobListob.StartTime = null;
                                }
                                PickerJobListob.EndTime = null;
                                if (a.Status == 2 && a.IsComplted == true && a.IsDispatched == false)
                                {
                                    PickerJobListob.Status = "New";
                                }
                                else if (a.Status == 1 && a.IsComplted == true && a.IsDispatched == false)
                                {
                                    PickerJobListob.Status = "Inprogress";
                                }
                                else if (a.Status == 5 && a.IsComplted == true && a.IsDispatched == false)
                                {
                                    PickerJobListob.Status = "RePicking";
                                }
                                else if (a.IsDispatched)
                                {
                                    PickerJobListob.Status = "Assignment Created";
                                }
                                pickerJobList.Add(PickerJobListob);
                            }
                        }
                    }
                }
            }
            return pickerJobList;
        }
        [HttpGet]
        [Route("InventorySupervisorActivity")]
        public async Task<HttpResponseMessage> InventorySupervisorActivity(int Take, int Skip, int UserId)
        {
            bool Status = false;
            PickerJobListRes res;
            string Msg = "";
            List<PickerJobListDc> pickerJobList = new List<PickerJobListDc>();
            using (var context = new AuthContext())
            {
                if (UserId > 0)
                {
                    //Skip++;
                    List<OrderPickerMaster> OrderPickerMasterList = context.OrderPickerMasterDb.Where(x => x.InventorySupervisorId == UserId && x.IsDeleted == false && (x.IsDispatched == true || x.IsApproved == true || x.IsCanceled == true || x.Status == 5 || (x.Status == 1 && x.RePickingCount > 0))).OrderByDescending(x => x.Id).Include("OrderPickerDetails").Skip(Skip).Take(Take).ToList();
                    if (OrderPickerMasterList != null && OrderPickerMasterList.Any())
                    {
                        var CreatedByids = OrderPickerMasterList.Select(x => x.CreatedBy).Distinct().ToList();
                        var SubmittedIds = OrderPickerMasterList.Select(x => x.PickerPersonId).Distinct().ToList();
                        var peopleIds = CreatedByids.Concat(SubmittedIds);
                        var ModifiedByids = OrderPickerMasterList.Select(x => x.ModifiedBy).Distinct().ToList();
                        var DBoyIds = OrderPickerMasterList.Select(x => x.DBoyId).Distinct().ToList();
                        var people = context.Peoples.Where(x =>
                        peopleIds.Contains(x.PeopleID) ||
                        ModifiedByids.Contains(x.PeopleID) ||
                        DBoyIds.Contains(x.PeopleID)
                        ).Distinct().ToList();

                        // var peopleMod = context.Peoples.Where(x => ModifiedByids.Contains(x.PeopleID)).Distinct().ToList();
                        //var people = context.Peoples.Where(x => peopleIds.Contains(x.PeopleID)).Distinct().ToList();

                        foreach (var a in OrderPickerMasterList)
                        {
                            bool inProress = false;

                            if (a.orderPickerDetails.Any(x => x.OrderPickerMasterId == a.Id))
                            {
                                double amount = 0;
                                var orderids = a.orderPickerDetails.Where(y => y.Status != 3).Select(y => y.OrderId).Distinct().ToList();
                                var clusters = context.DbOrderMaster.Where(x => orderids.Contains(x.OrderId)).Select(x => x.ClusterName).Distinct().ToList();
                                var dboyname = people.Where(x => x.PeopleID == a.DBoyId).FirstOrDefault()?.DisplayName;
                                var orderDetails = a.orderPickerDetails.Where(x => x.Status != 3).Select(x => new { x.OrderDetailsId, x.Qty }).ToList();
                                var orderdetailIds = orderDetails.Select(x => x.OrderDetailsId).ToList();
                                var dbOrderDetails = context.DbOrderDetails.Where(x => orderdetailIds.Contains(x.OrderDetailsId)).Select(x => new { x.OrderDetailsId, x.UnitPrice }).ToList();
                                dbOrderDetails.ForEach(x =>
                                {
                                    amount += orderDetails.Where(y => y.OrderDetailsId == x.OrderDetailsId).Sum(y => y.Qty * x.UnitPrice);
                                });
                                PickerJobListDc PickerJobListob = new PickerJobListDc();
                                PickerJobListob.Id = a.Id;
                                PickerJobListob.DeliveryIssuanceId = a.DeliveryIssuanceId > 0 ? Convert.ToString(a.DeliveryIssuanceId) : a.MultiDeliveryIssuanceIds;
                                PickerJobListob.CreatedDate = a.CreatedDate;
                                PickerJobListob.SubmittedDate = a.SubmittedDate;
                                PickerJobListob.ApprovedDate = a.ApprovedDate;
                                PickerJobListob.CanceledDate = a.ModifiedDate;
                                PickerJobListob.count = a.orderPickerDetails.GroupBy(x => x.ItemMultiMrpId).Count();
                                PickerJobListob.IsApproved = a.IsApproved;
                                PickerJobListob.Commenet = a.Comment;
                                PickerJobListob.SubmittedBy = people.FirstOrDefault(x => x.PeopleID == a.PickerPersonId).DisplayName;
                                PickerJobListob.CreatedBy = people.FirstOrDefault(x => x.PeopleID == a.CreatedBy).DisplayName;
                                PickerJobListob.CanceledBy = people.FirstOrDefault(x => x.PeopleID == a.ModifiedBy).DisplayName;
                                PickerJobListob.dboyName = dboyname == null ? "" : dboyname;
                                PickerJobListob.amount = Math.Round(amount, 0);
                                PickerJobListob.ClusterName = string.Join(",", clusters);
                                PickerJobListob.IsInventorySupervisor = a.IsInventorySupervisor;
                                PickerJobListob.IsInventorySupervisorStart = a.IsInventorySupervisorStart;
                                PickerJobListob.IsInventory = a.IsInventory;
                                if (a.Status == 2)
                                {
                                    PickerJobListob.Status = "Submitted";
                                }
                                else if (a.Status == 3 && a.IsApproved)
                                {
                                    PickerJobListob.Status = "Approved";
                                }
                                else if (a.Status == 6 && a.IsDispatched)
                                {
                                    PickerJobListob.Status = "Assignment Created";
                                }
                                else if (a.Status == 4)
                                {
                                    PickerJobListob.Status = "Canceled";
                                }
                                else if (a.Status == 5 || (a.RePickingCount > 0 && a.Status == 1))
                                {
                                    if ((a.RePickingCount > 0 && a.Status == 1))
                                    {
                                        inProress = context.PickerTimerDb.Any(x => x.OrderPickerMasterId == a.Id && x.EndTime == null && x.CreatedBy == UserId && x.Type == 2);
                                    }
                                    else
                                    {
                                        PickerJobListob.Status = "RePicking";
                                    }
                                }
                                if (!inProress)
                                {
                                    pickerJobList.Add(PickerJobListob);
                                }
                            }
                        }
                        if (pickerJobList != null && pickerJobList.Any())
                        {
                            Status = true;
                            Msg = "Record found";
                        }
                        else
                        {
                            Status = true;
                            Msg = "No Record found";
                        }
                    }
                }
            }
            res = new PickerJobListRes()
            {
                PickerJobLists = pickerJobList,
                Status = Status,
                Message = Msg
            };
            return Request.CreateResponse(HttpStatusCode.OK, res);
        }
        [HttpGet]
        [Route("InventorySupervisorPickList")]
        public async Task<List<OrderPickerMasterDc>> InventorySupervisorPickList(int WarehouseId)
        {
            using (AuthContext context = new AuthContext())
            {
                List<OrderPickerMaster> pickerList = new List<OrderPickerMaster>();
                var predicate = PredicateBuilder.New<OrderPickerMaster>(x => x.WarehouseId == WarehouseId && x.IsInventorySupervisor == false && x.IsCheckerGrabbed == false && x.IsComplted == true && x.IsDeleted == false);

                List<OrderPickerMaster> pickerlist = context.OrderPickerMasterDb.Where(predicate).OrderByDescending(x => x.Id).Include("OrderPickerDetails").ToList();

                var peoplecreatedby = pickerlist.Select(x => x.CreatedBy).Distinct().ToList();
                var dboyids = pickerlist.Select(x => x.DBoyId).Distinct().ToList();
                var createdname = context.Peoples.Where(x => peoplecreatedby.Contains(x.PeopleID) || dboyids.Contains(x.PeopleID)).ToList();

                var newPickerData = new List<OrderPickerMasterDc>();
                newPickerData = Mapper.Map(pickerlist).ToANew<List<OrderPickerMasterDc>>();
                foreach (var item in newPickerData)
                {
                    var MultiMrpIds = item.orderPickerDetails.Select(x => x.ItemMultiMrpId).Distinct().ToList();
                    var MultiMrpList = context.ItemMultiMRPDB.Where(x => MultiMrpIds.Contains(x.ItemMultiMRPId)).Distinct().ToList();
                    double amount = 0;
                    var orderids = item.orderPickerDetails.Where(y => y.Status != 3).Select(y => y.OrderId).Distinct().ToList();
                    var clusters = context.DbOrderMaster.Where(x => orderids.Contains(x.OrderId)).Select(x => x.ClusterName).Distinct().ToList();
                    var dboyname = createdname.Where(x => x.PeopleID == item.DboyId).FirstOrDefault()?.DisplayName;
                    var orderDetails = item.orderPickerDetails.Where(x => x.Status != 3).Select(x => new { x.OrderDetailsId, x.Qty }).ToList();
                    var orderdetailIds = orderDetails.Select(x => x.OrderDetailsId).ToList();
                    var dbOrderDetails = context.DbOrderDetails.Where(x => orderdetailIds.Contains(x.OrderDetailsId)).Select(x => new { x.OrderDetailsId, x.UnitPrice }).ToList();
                    dbOrderDetails.ForEach(x =>
                    {
                        amount += orderDetails.Where(y => y.OrderDetailsId == x.OrderDetailsId).Sum(y => y.Qty * x.UnitPrice);
                    });
                    foreach (var lineitems in item.orderPickerDetails)
                    {
                        lineitems.MRP = MultiMrpList.FirstOrDefault(x => x.ItemMultiMRPId == lineitems.ItemMultiMrpId).MRP;
                    }
                    item.CreatedByName = createdname.Where(x => x.PeopleID == item.CreatedBy).Select(x => x.DisplayName).FirstOrDefault();
                    item.orderpickercount = item.orderPickerDetails.Select(x => x.OrderId).Distinct().ToList().Count();
                    item.orderpickercountitem += item.orderPickerDetails.Select(x => x.Qty).Sum();
                    item.TotalAssignmentAmount = amount;
                    item.ClusterName = string.Join(",", clusters);
                    item.DBoyName = dboyname;
                }
                return newPickerData;
            }
        }
        [HttpPost]
        [Route("InsertMongoPickerOrderMaster")]
        public async Task<APIResponse> InsertMongoPickerOrderMaster(List<MongoPickerOrderMaster> mongoPickerOrderMaster)
        {
            APIResponse res = new APIResponse();
            string Id = "";
            bool result = false;
            var identity = User.Identity as ClaimsIdentity;
            int userid = 0;
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
            var skList = mongoPickerOrderMaster.Select(x => x.Skcode).Distinct().ToList();
            //List<int> custList = new List<int>();
            using (AuthContext context = new AuthContext())
            {
                //skList.ForEach(x => {
                //    int cId = context.Customers.Where(y => y.Skcode == x && y.Active==true && y.Deleted==false).Select(z=>z.CustomerId).FirstOrDefault();
                //    custList.Add(cId);
                //});
                var CustomerIdList = context.Customers.Where(x => skList.Contains(x.Skcode) && x.Active == true && x.Deleted == false).Select(x => x.CustomerId).ToList();
                DataTable dt = new DataTable();
                dt.Columns.Add("IntValue");
                foreach (var item in CustomerIdList)
                {
                    var dr = dt.NewRow();
                    dr["IntValue"] = item;
                    dt.Rows.Add(dr);
                }

                var param = new SqlParameter("customerids", dt);
                param.SqlDbType = System.Data.SqlDbType.Structured;
                param.TypeName = "dbo.IntValues";
                var PickerCustomerList = context.Database.SqlQuery<PickerCustomerDc>("Exec Picker.GetCustomersByIds  @customerids", param).ToList();
                if (PickerCustomerList != null && PickerCustomerList.Any(x => x.IsGstRequestPending == true))
                {

                    var GstRequestPendingSkcodes = PickerCustomerList.Where(x => x.IsGstRequestPending == true).Select(x => x.Skcode).ToList();
                    string str = "Customer (" + string.Join(", ", GstRequestPendingSkcodes) + ") GST Request is in progress. Please cordinate With customer care";
                    res.Status = false;
                    res.Message = str;
                    return res;
                }
            }
            var warehouseId = mongoPickerOrderMaster.FirstOrDefault().WarehouseId;
            var CustomerType = mongoPickerOrderMaster.FirstOrDefault().CustomerType;
            MongoDbHelper<PickerChooseMaster> mongoDbHelper = new MongoDbHelper<PickerChooseMaster>();
            var pickerMaster = mongoDbHelper.Select(x => x.Finalize == false && x.WarehouseId == warehouseId && x.CustomerType == CustomerType).FirstOrDefault();
            if (pickerMaster == null)
            {
                PickerChooseMaster pickerChooseMaster = new PickerChooseMaster
                {
                    CreatedBy = userid,
                    UpdateBy = 0,
                    WarehouseId = warehouseId,
                    CreatedDate = DateTime.Now,
                    Finalize = false,
                    UpdatedDate = DateTime.Now,
                    CustomerType = CustomerType,
                    mongoPickerOrderMaster = mongoPickerOrderMaster
                };
                Id = mongoDbHelper.InsertPickerMongo(pickerChooseMaster);
                if (string.IsNullOrEmpty(Id))
                {
                    Id = "";
                }
            }
            else
            {
                var Orderids = mongoPickerOrderMaster.Select(x => x.OrderId).ToList();
                var mongoOrderMaster = pickerMaster.mongoPickerOrderMaster.Where(x => Orderids.Contains(x.OrderId) && x.PickerOrderStatus == 1).ToList();
                var RemovemongoOrderMaster = pickerMaster.mongoPickerOrderMaster.Where(x => !Orderids.Contains(x.OrderId) && x.PickerOrderStatus == 1).ToList();
                var filterOrderids = mongoOrderMaster.Select(x => x.OrderId).ToList();
                var filtermongoOrderMaster = mongoPickerOrderMaster.Where(x => !filterOrderids.Contains(x.OrderId) && x.PickerOrderStatus == 1).ToList();
                if (filtermongoOrderMaster.Count > 0)
                {
                    pickerMaster.mongoPickerOrderMaster.AddRange(filtermongoOrderMaster);
                }
                if (RemovemongoOrderMaster.Count > 0)
                {
                    RemovemongoOrderMaster.ForEach(x =>
                    {
                        x.PickerOrderStatus = 2;
                    });
                }
                var pickerMasters = pickerMaster.mongoPickerOrderMaster.Where(x => x.PickerOrderStatus == 1).SelectMany(x => x.orderDetails).ToList();
                var changePickerMaster = mongoPickerOrderMaster.SelectMany(a => a.orderDetails).ToList();
                pickerMasters.ForEach(x =>
                {
                    changePickerMaster.ForEach(y =>
                    {
                        if (x.OrderDetailsId == y.OrderDetailsId)
                        {
                            x.Qty = y.Qty;
                        }
                    });
                });

                mongoDbHelper.Replace(pickerMaster.Id, pickerMaster);
                Id = Convert.ToString(pickerMaster.Id);
                res.Status = true;
                res.Message = Id;
            }
            return res;
        }
        [HttpGet]
        [Route("GetMongoPickerOrderMaster")]
        public async Task<List<PickerChooseMasterDC>> GetMongoPickerOrderMaster()
        {
            List<PickerChooseMasterDC> pickerChooseMasterDCs = new List<PickerChooseMasterDC>();
            List<PickerChooseMasterDC> pickerChooseMaster = new List<PickerChooseMasterDC>();

            MongoDbHelper<PickerChooseMaster> mongoDbHelper = new MongoDbHelper<PickerChooseMaster>();
            var pickerMaster = mongoDbHelper.Select(x => x.Finalize == false).ToList();
            if (pickerMaster.Count > 0)
            {
                var warehouseId = pickerMaster.Where(x => x.mongoPickerOrderMaster != null).SelectMany(x => x.mongoPickerOrderMaster.Select(y => y.WarehouseId)).Distinct().ToList();
                var clusterId = pickerMaster.Where(x => x.mongoPickerOrderMaster != null).SelectMany(x => x.mongoPickerOrderMaster.Select(y => y.ClusterId)).Distinct().ToList();
                using (var db = new AuthContext())
                {
                    var warehouse = db.Warehouses.Where(x => warehouseId.Contains(x.WarehouseId)).Select(x => new { x.WarehouseId, x.WarehouseName }).ToList();
                    var clusters = db.Clusters.Where(x => clusterId.Contains(x.ClusterId)).Select(x => new { x.ClusterId, x.ClusterName }).ToList();

                    pickerChooseMasterDCs = Mapper.Map(pickerMaster).ToANew<List<PickerChooseMasterDC>>();
                    var pickerorderMaster = pickerChooseMasterDCs.Where(x => x.mongoPickerOrderMaster != null).SelectMany(x => x.mongoPickerOrderMaster.Where(c => c.PickerOrderStatus == 1)).ToList();
                    var pcikerOrderids = pickerorderMaster.Select(c => c.OrderId).ToList();
                    var orderMaster = db.DbOrderMaster.Where(x => pcikerOrderids.Contains(x.OrderId) && x.Status == "Pending").Select(x => new
                    {
                        x.OrderId,
                        x.Status
                    }
                    ).ToList();
                    var orderids = orderMaster.Select(x => x.OrderId).ToList();
                    var filterOrdermaster = pickerChooseMasterDCs.Where(x => x.mongoPickerOrderMaster != null).SelectMany(x => x.mongoPickerOrderMaster.Where(u => !orderids.Contains(u.OrderId))).Distinct().ToList();
                    pickerMaster.ForEach(x =>
                    {
                        if (filterOrdermaster != null && filterOrdermaster.Any())
                        {
                            x.mongoPickerOrderMaster.ForEach(e =>
                            {
                                filterOrdermaster.ForEach(y =>
                                {
                                    if (e.OrderId == y.OrderId)
                                    {
                                        e.PickerOrderStatus = 2;
                                    }
                                });
                            });
                            var count = x.mongoPickerOrderMaster.Where(q => q.PickerOrderStatus == 1).Count();
                            if (count == 0)
                            {
                                x.Finalize = true;
                            }
                        }
                        mongoDbHelper.Replace(x.Id, x);
                    });
                    var pickeroOrderMasters = pickerMaster.Where(x => x.mongoPickerOrderMaster != null && x.Finalize == false).ToList();  //(x => x.Finalize == false).ToList();
                    pickerChooseMaster = Mapper.Map(pickeroOrderMasters).ToANew<List<PickerChooseMasterDC>>();
                    pickerChooseMaster.ForEach(x =>
                    {
                        x.mongoPickerOrderMaster.ForEach(y =>
                        {
                            y.WarehouseName = warehouse.FirstOrDefault(z => z.WarehouseId == y.WarehouseId).WarehouseName;
                            y.ClusterName = clusters.FirstOrDefault(z => z.ClusterId == y.ClusterId).ClusterName;
                            if (y.PickerOrderStatus == 1)
                            {
                                y.PickerSelectStatus = "Pick";
                            }
                            else if (y.PickerOrderStatus == 2)
                            {
                                y.PickerSelectStatus = "Pick Generate";
                            }
                            else if (y.PickerOrderStatus == 3)
                            {
                                y.PickerSelectStatus = "reject";
                            }
                        });
                    });
                }
            }
            return pickerChooseMaster;
        }
        [HttpGet]
        [Route("GetMongoPickerObjectId")]
        public async Task<PickerChooseMasterDC> GetMongoPickerObjectId(string objectId)
        {
            PickerChooseMaster pickerMaster = new PickerChooseMaster();
            PickerChooseMasterDC pickerChooseMasterDCs = new PickerChooseMasterDC();
            PickerChooseMasterDC pickerChooseMaster = new PickerChooseMasterDC();
            MongoDbHelper<PickerChooseMaster> mongoDbHelper = new MongoDbHelper<PickerChooseMaster>();
            var objectid = new MongoDB.Bson.ObjectId(objectId);
            pickerMaster = mongoDbHelper.Select(x => x.Id == objectid && x.Finalize == false).FirstOrDefault();

            if (pickerMaster != null)
            {
                pickerChooseMasterDCs = Mapper.Map(pickerMaster).ToANew<PickerChooseMasterDC>();
                var warehouseId = pickerMaster.mongoPickerOrderMaster.Select(y => y.WarehouseId).Distinct().ToList();
                var clusterId = pickerMaster.mongoPickerOrderMaster.Select(y => y.ClusterId).Distinct().ToList();
                using (var db = new AuthContext())
                {
                    var warehouse = db.Warehouses.Where(x => warehouseId.Contains(x.WarehouseId)).Select(x => new { x.WarehouseId, x.WarehouseName }).ToList();
                    var clusters = db.Clusters.Where(x => clusterId.Contains(x.ClusterId)).Select(x => new { x.ClusterId, x.ClusterName }).ToList();

                    var pickerorderMaster = pickerChooseMasterDCs.mongoPickerOrderMaster.Where(x => x.PickerOrderStatus == 1).ToList();
                    var pcikerOrderids = pickerorderMaster.Select(c => c.OrderId).ToList();
                    var orderMaster = db.DbOrderMaster.Where(x => pcikerOrderids.Contains(x.OrderId) && x.Status == "Pending").Select(x => new
                    {
                        x.OrderId,
                        x.Status
                    }
                    ).ToList();
                    var orderids = orderMaster.Select(x => x.OrderId).ToList();
                    var filterOrdermaster = pickerChooseMasterDCs.mongoPickerOrderMaster.Where(x => !orderids.Contains(x.OrderId)).Distinct().ToList();
                    pickerMaster.mongoPickerOrderMaster.ForEach(x =>
                    {
                        if (filterOrdermaster != null && filterOrdermaster.Any())
                        {
                            filterOrdermaster.ForEach(y =>
                            {
                                if (x.OrderId == y.OrderId)
                                {
                                    x.PickerOrderStatus = 2;
                                }
                            });
                        }
                    });
                    var count = pickerMaster.mongoPickerOrderMaster.Where(q => q.PickerOrderStatus == 1).Count();
                    if (count == 0)
                    {
                        pickerMaster.Finalize = true;
                    }
                    mongoDbHelper.Replace(pickerMaster.Id, pickerMaster);
                    var pickeroOrderMasters = pickerMaster.mongoPickerOrderMaster.Where(x => x.PickerOrderStatus == 1).ToList();
                    pickerMaster.mongoPickerOrderMaster = pickeroOrderMasters;
                    pickerChooseMaster = Mapper.Map(pickerMaster).ToANew<PickerChooseMasterDC>();
                    pickerChooseMaster.mongoPickerOrderMaster.ForEach(y =>
                    {
                        y.WarehouseName = warehouse.FirstOrDefault(z => z.WarehouseId == y.WarehouseId).WarehouseName;
                        y.ClusterName = clusters.FirstOrDefault(z => z.ClusterId == y.ClusterId).ClusterName;
                        if (y.PickerOrderStatus == 1)
                        {
                            y.PickerSelectStatus = "Pick";
                        }
                        else if (y.PickerOrderStatus == 2)
                        {
                            y.PickerSelectStatus = "Pick Generate";
                        }
                        else if (y.PickerOrderStatus == 3)
                        {
                            y.PickerSelectStatus = "reject";
                        }
                    });
                }
            }
            return pickerChooseMaster;
        }
        [Route("checkInventryOrderMaster")]
        [HttpPost]
        public async Task<PickerChooseMasterDC> checkInventryOrderMaster(PickerChooseMasterDC pickerChooseMasterDC)
        {
            PickerChooseMaster pickerMaster = new PickerChooseMaster();
            PickerChooseMasterDC pickerChooseMasterDCs = new PickerChooseMasterDC();
            using (var db = new AuthContext())
            {
                MongoDbHelper<PickerChooseMaster> mongoDbHelper = new MongoDbHelper<PickerChooseMaster>();
                var objectid = new MongoDB.Bson.ObjectId(pickerChooseMasterDC.Id);
                pickerMaster = mongoDbHelper.Select(x => x.Id == objectid && x.Finalize == false).FirstOrDefault();

                if (pickerMaster != null)
                {
                    pickerChooseMasterDCs = Mapper.Map(pickerMaster).ToANew<PickerChooseMasterDC>();
                    var pickerorderMaster = pickerChooseMasterDCs.mongoPickerOrderMaster.Where(x => x.PickerOrderStatus == 1 && x.ClusterId == pickerChooseMasterDC.ClusterId).ToList();
                    var warehouseids = pickerorderMaster.Select(c => c.WarehouseId).Distinct().ToList();
                    var ItemMultiMrpIds = pickerorderMaster.SelectMany(c => c.orderDetails.Select(x => x.ItemMultiMrpId).Distinct()).ToList();
                    var freeItemMultiMrpIds = pickerorderMaster.SelectMany(c => c.orderDetails.Where(x => x.IsFreeItem == true).Select(x => x.ItemMultiMrpId).Distinct()).ToList();

                    var warehouseId = pickerMaster.mongoPickerOrderMaster.Select(y => y.WarehouseId).Distinct().ToList();
                    var clusterId = pickerMaster.mongoPickerOrderMaster.Select(y => y.ClusterId).Distinct().ToList();
                    var warehouse = db.Warehouses.Where(x => warehouseId.Contains(x.WarehouseId)).Select(x => new { x.WarehouseId, x.WarehouseName }).ToList();
                    var clusters = db.Clusters.Where(x => clusterId.Contains(x.ClusterId)).Select(x => new { x.ClusterId, x.ClusterName }).ToList();
                    var currentstock = db.DbCurrentStock.Where(x => warehouseids.Contains(x.WarehouseId) && ItemMultiMrpIds.Contains(x.ItemMultiMRPId) && x.Deleted == false).ToList();
                    var freestock = db.FreeStockDB.Where(x => warehouseids.Contains(x.WarehouseId) && freeItemMultiMrpIds.Contains(x.ItemMultiMRPId) && x.Deleted == false).ToList();

                    pickerChooseMasterDCs.mongoPickerOrderMaster.Where(q => q.PickerOrderStatus == 1 && q.ClusterId == pickerChooseMasterDC.ClusterId).OrderBy(q => q.OrderId).ToList().ForEach(x =>
                    {
                        x.orderDetails.ForEach(y =>
                        {
                            var netcurrentstock = currentstock.FirstOrDefault(w => w.ItemMultiMRPId == y.ItemMultiMrpId);
                            var freenetcurrentstock = freestock.FirstOrDefault(w => w.ItemMultiMRPId == y.ItemMultiMrpId);
                            if (y.IsFreeItem == true)
                            {
                                if (freenetcurrentstock != null)
                                {
                                    if (y.IsDispatchedFreeStock == true)
                                    {
                                        if (y.WarehouseId == freenetcurrentstock.WarehouseId && y.ItemMultiMrpId == freenetcurrentstock.ItemMultiMRPId)
                                        {
                                            if (freenetcurrentstock.CurrentInventory >= y.Qty)
                                            {
                                                freenetcurrentstock.CurrentInventory = freenetcurrentstock.CurrentInventory - y.Qty;
                                                //x.OrderColor = "White";
                                                y.CurrentStock = freenetcurrentstock.CurrentInventory;
                                            }
                                            else
                                            {
                                                y.CurrentStock = freenetcurrentstock.CurrentInventory;
                                                x.OrderColor = "rgb(255, 153, 153)";
                                            }
                                        }
                                    }
                                    else
                                    {
                                        if (y.WarehouseId == netcurrentstock.WarehouseId && y.ItemMultiMrpId == netcurrentstock.ItemMultiMRPId)
                                        {
                                            if (netcurrentstock.CurrentInventory >= y.Qty)
                                            {
                                                netcurrentstock.CurrentInventory = netcurrentstock.CurrentInventory - y.Qty;
                                                //x.OrderColor = "White";
                                                y.CurrentStock = netcurrentstock.CurrentInventory;
                                            }
                                            else
                                            {
                                                y.CurrentStock = netcurrentstock.CurrentInventory;
                                                x.OrderColor = "rgb(255, 153, 153)";
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    y.CurrentStock = 0;
                                }
                            }
                            else
                            {
                                if (netcurrentstock != null)
                                {
                                    if (y.WarehouseId == netcurrentstock.WarehouseId && y.ItemMultiMrpId == netcurrentstock.ItemMultiMRPId)
                                    {
                                        if (netcurrentstock.CurrentInventory >= y.Qty)
                                        {
                                            netcurrentstock.CurrentInventory = netcurrentstock.CurrentInventory - y.Qty;
                                            //x.OrderColor = "White";
                                            y.CurrentStock = netcurrentstock.CurrentInventory;
                                        }
                                        else
                                        {
                                            y.CurrentStock = netcurrentstock.CurrentInventory;
                                            x.OrderColor = "rgb(255, 153, 153)";
                                        }
                                    }
                                }
                                else
                                {
                                    y.CurrentStock = 0;
                                }
                            }

                        });
                        x.WarehouseName = warehouse.FirstOrDefault(z => z.WarehouseId == x.WarehouseId).WarehouseName;
                        x.ClusterName = clusters.FirstOrDefault(z => z.ClusterId == x.ClusterId).ClusterName;
                        if (x.PickerOrderStatus == 1)
                        {
                            x.PickerSelectStatus = "Pick";
                        }
                        else if (x.PickerOrderStatus == 2)
                        {
                            x.PickerSelectStatus = "Pick Generate";
                        }
                        else if (x.PickerOrderStatus == 3)
                        {
                            x.PickerSelectStatus = "reject";
                        }
                    });
                    var ab = pickerChooseMasterDCs.mongoPickerOrderMaster.Where(x => x.PickerOrderStatus == 1).ToList();
                    ab.ForEach(x =>
                    {
                        x.ClusterName = clusters.FirstOrDefault(z => z.ClusterId == x.ClusterId).ClusterName;
                        if (x.PickerOrderStatus == 1)
                        {
                            x.PickerSelectStatus = "Pick";
                        }
                        else if (x.PickerOrderStatus == 2)
                        {
                            x.PickerSelectStatus = "Pick Generate";
                        }
                        else if (x.PickerOrderStatus == 3)
                        {
                            x.PickerSelectStatus = "reject";
                        }
                    });
                }
            }
            var a = pickerChooseMasterDCs.mongoPickerOrderMaster.Where(x => x.PickerOrderStatus == 1).ToList();
            pickerChooseMasterDCs.mongoPickerOrderMaster = a;
            return pickerChooseMasterDCs;
        }

        [Route("SearchSKP_KPP_OwnerList")]
        [HttpGet]
        public List<GetSKP_KPP_OwnerListDC> SearchSKP_KPP_OwnerList(int Warehouseid, string Customertype)
        {
            List<GetSKP_KPP_OwnerListDC> list = new List<GetSKP_KPP_OwnerListDC>();
            using (var db = new AuthContext())
            {
                var predicate = PredicateBuilder.True<Customer>();
                predicate = predicate.And(x => x.Deleted == false && x.Warehouseid == Warehouseid);
                if (Customertype == "SKP Owner")
                {
                    predicate = predicate.And(x => x.CustomerType == "SKP Owner");
                }
                else if (Customertype == "KPP")
                {
                    predicate = predicate.And(x => x.CustomerType == "KPP");
                }

                list = db.Customers.Where(predicate).Select(
                    x => new GetSKP_KPP_OwnerListDC
                    {
                        CustomerId = x.CustomerId,
                        Skcode = x.Skcode + "-(" + x.Name + ")",
                        ShopName = x.ShopName
                    }).ToList();
            }
            return list;
        }

        #region For Inventory Correction use only
        [HttpPost]
        [Route("PickerOrderReject")]
        public async Task<bool> PickerOrderReject(PickerRejectDc PickerReject)
        {
            bool result = false;
            if (PickerReject != null && PickerReject.OrderIds.Any() && PickerReject.UserId > 0)
            {

                using (AuthContext context = new AuthContext())
                {
                    var picklist = context.OrderPickerDetailsDb.Where(x => x.OrderPickerMasterId == PickerReject.PickerId && PickerReject.OrderIds.Contains(x.OrderId)).ToList();
                    picklist.ForEach(x =>
                    {
                        x.Status = 3; //reject
                        x.ModifiedBy = PickerReject.UserId; //reject
                        x.ModifiedDate = indianTime;
                        x.Comment = PickerReject.Comment;
                        context.Entry(x).State = EntityState.Modified;
                    });
                    result = context.Commit() > 0;
                }
            }
            return result;
        }
        #endregion



        #region Picking enhancement as per 13 april 2023 Discussion 

        #region Maker


        [HttpGet]
        [Route("GetMakerTaskList/{WarehouseId}/{Skip}/{Take}")]
        public async Task<PickerResMsg> GetMakerTaskList(int WarehouseId, int Skip, int Take)
        {
            var manager = new PickerManager();
            return await manager.GetMakerTaskList(WarehouseId, Skip, Take);

        }



        [Route("MakerPickerGrabbed/{PeopleId}/{OrderPickerMasterId}/{IsPickerGrabbed}")]
        [HttpGet]
        public async Task<PickerResMsg> MakerPickerGrabbed(int PeopleId, int OrderPickerMasterId, bool IsPickerGrabbed)
        {
            var result = new PickerResMsg();
            if (PeopleId > 0 && OrderPickerMasterId > 0)
            {
                using (var context = new AuthContext())
                {
                    var acceptstatus = context.OrderPickerMasterDb.FirstOrDefault(x => x.Id == OrderPickerMasterId && x.IsPickerGrabbed == false && x.IsDeleted == false && x.IsCanceled == false);

                    if (acceptstatus != null && IsPickerGrabbed == true && PeopleId > 0)
                    {
                        acceptstatus.PickerPersonId = PeopleId;
                        acceptstatus.IsPickerGrabbed = true;
                        acceptstatus.PickerGrabbedTime = indianTime;
                        acceptstatus.ModifiedDate = indianTime;
                        acceptstatus.ModifiedBy = PeopleId;
                        acceptstatus.CreatedBy = acceptstatus.CreatedBy > 0 ? acceptstatus.CreatedBy : PeopleId;
                        context.Entry(acceptstatus).State = EntityState.Modified;
                        if (context.Commit() > 0)
                        {
                            result.Status = true;
                            result.Message = "Grabbed successfully";
                        }

                    }
                    else
                    {
                        result.Status = false;
                        result.Message = "Something went wrong";
                    }
                }
            }
            return result;
        }

        [HttpGet]
        [Route("MakerGrabbedPickerList/{PeopleId}/{Skip}/{Take}")]
        public async Task<PickerResMsg> MakerGrabbedPickerList(int PeopleId, int Skip, int Take)
        {

            var manager = new PickerManager();
            return await manager.MakerGrabbedPickerList(PeopleId, Skip, Take);

        }

        [HttpPost]
        [Route("MakerStartPicking/{OrderPickerMasterId}")]
        public async Task<PickerResMsg> MakerStartPicking(int OrderPickerMasterId)  //type = start / end
        {
            var result = new PickerResMsg();

            PickerTimer PickerTimers = new PickerTimer();

            if (OrderPickerMasterId > 0)
            {
                using (AuthContext context = new AuthContext())
                {
                    var OrderPickerMaster = context.OrderPickerMasterDb.Where(x => x.Id == OrderPickerMasterId && x.IsPickerGrabbed && x.IsDeleted == false).FirstOrDefault();
                    if (OrderPickerMaster != null && !OrderPickerMaster.IsCanceled)
                    {

                        bool PickListAnyInProcess = context.PickerTimerDb.Any(x => x.CreatedBy == OrderPickerMaster.PickerPersonId && x.Id != OrderPickerMaster.Id && x.Type == 0 && x.EndTime == null);
                        if (!PickListAnyInProcess)
                        {
                            var PickerTimer = context.PickerTimerDb.Where(x => x.OrderPickerMasterId == OrderPickerMasterId && x.Type == 0).ToList();
                            if (PickerTimer.Count == 0)
                            {
                                PickerTimers.OrderPickerMasterId = OrderPickerMaster.Id;
                                PickerTimers.StartTime = DateTime.Now;
                                PickerTimers.EndTime = null;
                                PickerTimers.CreatedBy = OrderPickerMaster.PickerPersonId;
                                PickerTimers.Type = 0;//Maker
                                context.PickerTimerDb.Add(PickerTimers);
                                OrderPickerMaster.CreatedBy = OrderPickerMaster.CreatedBy > 0 ? OrderPickerMaster.CreatedBy : OrderPickerMaster.PickerPersonId;
                                OrderPickerMaster.Status = 1;// 1= InProgress(Maker), 
                                OrderPickerMaster.ModifiedDate = indianTime;
                                context.Entry(OrderPickerMaster).State = EntityState.Modified;

                                if (context.Commit() > 0)
                                {
                                    result.Status = true;
                                    result.Message = "Picker Start Successfully";

                                }
                            }
                            else if (PickerTimer.All(x => x.EndTime != null))
                            {
                                //Restart Pick
                                PickerTimers.OrderPickerMasterId = OrderPickerMaster.Id;
                                PickerTimers.StartTime = DateTime.Now;
                                PickerTimers.EndTime = null;
                                PickerTimers.Type = 0;//Maker
                                PickerTimers.CreatedBy = OrderPickerMaster.PickerPersonId;
                                context.PickerTimerDb.Add(PickerTimers);

                                OrderPickerMaster.Status = 1;// 1= InProgress(Maker), 
                                OrderPickerMaster.ModifiedDate = indianTime;
                                context.Entry(OrderPickerMaster).State = EntityState.Modified;

                                if (context.Commit() > 0)
                                {
                                    result.Status = true;
                                    result.Message = "Picker Restart Successfully";

                                }

                            }
                            else
                            {
                                PickerTimers = PickerTimer.Where(x => x.EndTime == null).FirstOrDefault();
                                result.Status = true;
                            }
                        }
                        else
                        {
                            result.Message = "पिछला पिकर प्रगति पर होने के कारण आप नया पिकर प्रारंभ नहीं कर सकते|";
                        }
                    }
                    else
                    {
                        result.Message = "Picker Canceled";
                    }
                }
            }

            return result;
        }


        [Route("MakerPickerProcessCanceled/{OrderPickerMasterId}/{comment}")]
        [HttpPost]
        public async Task<PickerResMsg> MakerPickerProcessCanceled(int OrderPickerMasterId, string comment)
        {
            var result = new PickerResMsg();

            var Msg = "";
            bool IsValidate = false;
            if (OrderPickerMasterId > 0)
            {
                if (PickerProcessIds.Any(x => x == OrderPickerMasterId))
                {

                    //Msg = "Picker #: " + OrderPickerMasterId + " is already in process..";
                    //return Msg;
                    result.Status = false;
                    result.Message = "Picker #: " + OrderPickerMasterId + " is already in process..";
                    return result;
                }
                else
                {
                    PickerProcessIds.Add(OrderPickerMasterId);
                }
                var OrderPickerMaster = new OrderPickerMaster();
                using (var authContext = new AuthContext())
                {
                    OrderPickerMaster = authContext.OrderPickerMasterDb.Where(x => x.Id == OrderPickerMasterId && x.IsApproved == false && x.IsCanceled == false && x.IsDispatched == false && x.IsDeleted == false).Include(x => x.orderPickerDetails).FirstOrDefault();

                    var warehouse = authContext.Warehouses.FirstOrDefault(x => x.WarehouseId == OrderPickerMaster.WarehouseId);

                    if (warehouse.IsStopCurrentStockTrans)
                        //return "Inventory Transactions are currently disabled for this warehouse... Please try after some time";
                        result.Status = true;
                    result.Message = "Inventory Transactions are currently disabled for this warehouse... Please try after some time";
                    return result;

                    if (OrderPickerMaster != null && OrderPickerMaster.orderPickerDetails.Any(x => x.Status == 2))
                    {
                        //IsValidate = true;
                        //Msg = "You can't cancel Picker request due some of item in accept /approval stage";
                        result.Status = true;
                        result.Message = "You can't cancel Picker request due some of item in accept /approval stage";
                    }
                    else if (OrderPickerMaster == null)
                    {
                        //IsValidate = true;
                        //Msg = "Picker request not eligible to canceled";
                        result.Status = true;
                        result.Message = "Picker request not eligible to canceled";
                    }
                    else { }
                }
                if (!IsValidate)
                {
                    ReadyToPickDispatchedHelper helper = new ReadyToPickDispatchedHelper();
                    Msg = helper.DOPickerProcessCanceled(OrderPickerMasterId, OrderPickerMaster.PickerPersonId, comment);
                }
            }
            PickerProcessIds.RemoveAll(x => x == OrderPickerMasterId);

            //return Msg;
            return result;
        }


        [HttpGet]
        [Route("MakerOrderPickingDetail/{OrderPickerMasterId}")]
        public async Task<PickerResMsg> MakerOrderPickingDetail(long OrderPickerMasterId)
        {
            var manager = new PickerManager();
            return await manager.GetOrderPickingDetail(OrderPickerMasterId);

        }

        [HttpPost]
        [Route("MakerSearchViaBarcode")]
        public async Task<PickerResMsg> MakerSearchViaBarcode(SearchViaBarcodeDc SearchViaBarcode)
        {
            var manager = new PickerManager();
            return await manager.SearchViaBarcode(SearchViaBarcode);
        }


        //09/06/2023

        [HttpPost]
        [Route("MakerScan")]
        public async Task<PickerResMsg> MakerScanItem(ScanItemDc ScanItem)
        {
            var manager = new PickerManager();
            return await manager.MakerScanItem(ScanItem);
        }

        [HttpPost]
        [Route("MakerSubmitPickedBatch")]
        public async Task<PickerResMsg> MakerSubmitPickedBatch(SubmitPickedBatchDc SubmitPickedBatch)
        {

            var manager = new PickerManager();
            return await manager.SubmitPickedBatch(SubmitPickedBatch);
        }

        [HttpPost]
        [Route("MakerPickerSubmit/{OrderPickerMasterId}/{UserId}")]
        public async Task<PickerResMsg> PickerSubmit(int OrderPickerMasterId, int UserId)
        {
            var result = new PickerResMsg();
            bool status = false;
            if (OrderPickerMasterId > 0 && UserId > 0)
            {
                using (AuthContext context = new AuthContext())
                {
                    var orderPickerMaster = context.OrderPickerMasterDb.Where(x => x.Id == OrderPickerMasterId && x.IsDeleted == false).Include(x => x.orderPickerDetails).FirstOrDefault();
                    var warehouse = context.Warehouses.FirstOrDefault(x => x.WarehouseId == orderPickerMaster.WarehouseId);
                    if (warehouse.IsStopCurrentStockTrans)
                    {
                        result.Status = false;
                        result.Message = "Inventory Transactions are currently disabled for this warehouse... Please try after some time";
                        return result;
                    }
                    if (warehouse != null && warehouse.IsStore && warehouse.StoreType == 1)//Zila Store
                    {
                        result = await ZilaPickerSubmit(OrderPickerMasterId, UserId);
                        return result;
                    }
                }
                OrderOutPublisher Publisher = new OrderOutPublisher();
                List<BatchCodeSubjectDc> PublisherPickerRejectStockList = new List<BatchCodeSubjectDc>();
                TransactionOptions option = new TransactionOptions();
                option.IsolationLevel = System.Transactions.IsolationLevel.RepeatableRead;
                option.Timeout = TimeSpan.FromSeconds(500);
                OrderPickerMaster OrderPickerMaster = new OrderPickerMaster();
                using (var dbContextTransaction = new TransactionScope(TransactionScopeOption.Required, option))
                {
                    using (AuthContext context = new AuthContext())
                    {
                        OrderPickerMaster = context.OrderPickerMasterDb.Where(x => x.Id == OrderPickerMasterId && x.IsDeleted == false).Include(x => x.orderPickerDetails).FirstOrDefault();

                        var warehouse = context.Warehouses.FirstOrDefault(x => x.WarehouseId == OrderPickerMaster.WarehouseId);

                        if (OrderPickerMaster != null && !OrderPickerMaster.IsCanceled && !OrderPickerMaster.IsComplted)
                        {
                            bool IsCompleted = OrderPickerMaster.orderPickerDetails.Any(x => x.Status == 0 && x.OrderPickerMasterId == OrderPickerMaster.Id);
                            if (IsCompleted)
                            {

                                result.Status = false;
                                result.Message = "Picker List Still in Pending, you can't submit";
                                return result;
                            }
                            else
                            {
                                var RejectedOrderids = OrderPickerMaster.orderPickerDetails.Where(x => x.Status == 3).Select(x => x.OrderId).Distinct().ToList();
                                if (OrderPickerMaster.orderPickerDetails.Any(x => RejectedOrderids.Contains(x.OrderId) && x.Status != 3))
                                {

                                    result.Status = false;
                                    result.Message = "Picker can't be submited, Due to following Orderids: " + string.Join(",", OrderPickerMaster.orderPickerDetails.Where(x => RejectedOrderids.Contains(x.OrderId) && x.Status != 3).Select(x => x.OrderId).Distinct().ToList()) + " Please accept or reject all the items.";
                                    return result;
                                }

                                if (RejectedOrderids != null && RejectedOrderids.Any())
                                {
                                    var param = new SqlParameter("PickerId", OrderPickerMaster.Id);
                                    long? TripMasterId = context.Database.SqlQuery<long?>("exec operation.IsTripPicker @PickerId", param).FirstOrDefault();
                                    if (TripMasterId > 0)
                                    {
                                        TripPlannerHelper triphelp = new TripPlannerHelper();
                                        bool IsSuccess = triphelp.RemovePickerOrderFromTrip(TripMasterId.Value, OrderPickerMasterId, RejectedOrderids, context, UserId);
                                        if (!IsSuccess)
                                        {
                                            dbContextTransaction.Dispose();

                                            result.Status = false;
                                            result.Message = "Remove Picker Order From Trip Failed";
                                            return result;
                                        }
                                    }

                                    foreach (var Odid in RejectedOrderids)
                                    {
                                        OrderMaster omCheck = context.DbOrderMaster.Where(x => x.OrderId == Odid && x.Status == "ReadyToPick" && x.Deleted == false).Include(x => x.orderDetails).FirstOrDefault();
                                        if (omCheck != null)
                                        {
                                            foreach (var item in omCheck.orderDetails)
                                            {
                                                item.Status = "Pending";
                                                item.UpdatedDate = indianTime;
                                            }
                                            omCheck.Status = "Pending";
                                            omCheck.UpdatedDate = indianTime;
                                            context.Entry(omCheck).State = EntityState.Modified;
                                            #region Order History
                                            Model.OrderMasterHistories h1 = new Model.OrderMasterHistories();
                                            h1.orderid = omCheck.OrderId;
                                            h1.Status = omCheck.Status;
                                            h1.Reasoncancel = null;
                                            h1.Warehousename = omCheck.WarehouseName;
                                            h1.userid = UserId;
                                            h1.CreatedDate = DateTime.Now;
                                            context.OrderMasterHistoriesDB.Add(h1);
                                            #endregion
                                            if (context.Commit() > 0)
                                            {
                                                #region stock Hit
                                                //for currentstock
                                                MultiStockHelper<OnPickedCancelDc> MultiStockHelpers = new MultiStockHelper<OnPickedCancelDc>();
                                                List<OnPickedCancelDc> RTDOnPickedCancelList = new List<OnPickedCancelDc>();
                                                foreach (var StockHit in omCheck.orderDetails.Where(x => x.qty > 0))
                                                {
                                                    int qty = OrderPickerMaster.orderPickerDetails.FirstOrDefault(x => x.OrderDetailsId == StockHit.OrderDetailsId).Qty;
                                                    if (qty > 0)
                                                    {
                                                        bool isfree = false;
                                                        string RefStockCode = omCheck.OrderType == 8 ? "CL" : "C";
                                                        if (StockHit.IsFreeItem && StockHit.IsDispatchedFreeStock)
                                                        {
                                                            RefStockCode = "F";
                                                            isfree = true;
                                                        }
                                                        RTDOnPickedCancelList.Add(new OnPickedCancelDc
                                                        {
                                                            ItemMultiMRPId = StockHit.ItemMultiMRPId,
                                                            OrderDispatchedDetailsId = StockHit.OrderDetailsId,
                                                            OrderId = StockHit.OrderId,
                                                            Qty = qty,
                                                            UserId = UserId,
                                                            WarehouseId = StockHit.WarehouseId,
                                                            IsFreeStock = isfree,
                                                            RefStockCode = RefStockCode
                                                        });
                                                    }
                                                }
                                                if (RTDOnPickedCancelList.Any())
                                                {
                                                    bool IsUpdate = MultiStockHelpers.MakeEntry(RTDOnPickedCancelList, "Stock_OnPickedCancel", context, dbContextTransaction);
                                                    if (!IsUpdate)
                                                    {
                                                        dbContextTransaction.Dispose();

                                                        result.Status = false;
                                                        result.Message = "Something went wrong";
                                                        return result;
                                                    }
                                                    #region BatchCode
                                                    foreach (var s in RTDOnPickedCancelList.Where(x => x.Qty > 0))
                                                    {
                                                        PublisherPickerRejectStockList.Add(new BatchCodeSubjectDc
                                                        {
                                                            ObjectDetailId = s.OrderDispatchedDetailsId,  // its OrderDetailsId
                                                            ObjectId = s.OrderId,
                                                            StockType = s.RefStockCode,
                                                            Quantity = s.Qty,
                                                            WarehouseId = s.WarehouseId,
                                                            ItemMultiMrpId = s.ItemMultiMRPId
                                                        });
                                                    }
                                                    #endregion
                                                }
                                                #endregion
                                            }
                                        }
                                    }

                                }
                                var Pickertimer = context.PickerTimerDb.Where(x => x.OrderPickerMasterId == OrderPickerMaster.Id && x.EndTime == null && x.Type == 0).ToList();
                                if (Pickertimer != null)
                                {
                                    OrderPickerMaster.ModifiedDate = indianTime;
                                    OrderPickerMaster.ModifiedBy = OrderPickerMaster.PickerPersonId;
                                }
                                bool IsNoorderChecker = OrderPickerMaster.orderPickerDetails.Any(x => x.Status != 3);
                                if (IsNoorderChecker)
                                {
                                    OrderPickerMaster.IsComplted = true;
                                    OrderPickerMaster.SubmittedDate = indianTime;
                                    OrderPickerMaster.Status = 2;// 2=Submitted(Maker) 
                                }
                                else
                                {
                                    OrderPickerMaster.IsCanceled = true;
                                    OrderPickerMaster.Status = 4; //Canceled
                                    OrderPickerMaster.SubmittedDate = indianTime;

                                    OrderPickerMaster.Comment = "System Canceled"; //Canceled
                                }

                                context.Entry(OrderPickerMaster).State = EntityState.Modified;

                                if (Pickertimer != null)
                                {
                                    Pickertimer.ForEach(x =>
                                    {
                                        x.EndTime = indianTime;
                                        context.Entry(x).State = EntityState.Modified;
                                    });
                                }
                                if (context.Commit() > 0)
                                {


                                    ///


                                    dbContextTransaction.Complete();
                                    //Msg = "Picker Submitted Successfully";
                                    status = true;
                                    result.Status = true;
                                    result.Message = "Picker Submitted Successfully";
                                }
                                else
                                {
                                    //Msg = "Something went wrong";
                                    status = false;
                                    result.Status = false;
                                    result.Message = "Something went wrong";
                                }
                            }

                        }
                        else
                        {
                            if (OrderPickerMaster.IsCanceled) { result.Message = "Picker Canceled"; } // Msg = "Picker Canceled";
                            if (OrderPickerMaster.IsComplted) { result.Message = "Picker is already submitted"; } //Msg = "Picker is already submitted";
                            //status = false;
                            result.Status = false;
                        }
                    }
                }
                if (PublisherPickerRejectStockList != null && PublisherPickerRejectStockList.Any() && status)
                {
                    Publisher.PlannedRejectPublish(PublisherPickerRejectStockList);
                }
            }

            //var res = new ResMsg()
            //{
            //    Status = status,
            //    Message = Msg
            //};
            //return Request.CreateResponse(HttpStatusCode.OK, res);
            return result;
        }

        #endregion

        #region Checker

        [HttpGet]
        [Route("GetChekerTaskList/{WarehouseId}/{Skip}/{Take}")]
        public async Task<PickerResMsg> GetChekerTaskList(int WarehouseId, int Skip, int Take)
        {
            var manager = new PickerManager();
            return await manager.GetChekerTaskList(WarehouseId, Skip, Take);

        }

        [Route("CheckerPickerGrabbed/{PeopleId}/{OrderPickerMasterId}/{IsCheckerGrabbed}")]
        [HttpGet]
        public async Task<PickerResMsg> CheckerPickerGrabbed(int PeopleId, int OrderPickerMasterId, bool IsCheckerGrabbed)
        {
            var result = new PickerResMsg();
            using (var context = new AuthContext())
            {
                var acceptstatus = context.OrderPickerMasterDb.FirstOrDefault(x => x.Id == OrderPickerMasterId && x.IsCheckerGrabbed == false && x.IsComplted == true && x.IsDeleted == false && x.IsCanceled == false);

                if (acceptstatus != null && IsCheckerGrabbed == true && PeopleId > 0 && acceptstatus.PickerPersonId != PeopleId)
                {
                    acceptstatus.ApproverId = PeopleId;
                    acceptstatus.IsCheckerGrabbed = true;
                    acceptstatus.CheckerGrabbedTime = indianTime;
                    acceptstatus.ModifiedDate = indianTime;
                    acceptstatus.ModifiedBy = PeopleId;
                    context.Entry(acceptstatus).State = EntityState.Modified;
                    if (context.Commit() > 0)
                    {
                        result.Status = true;
                        result.Message = "Grabbed successfully";
                    }
                }
                else if (acceptstatus != null && IsCheckerGrabbed == true && PeopleId > 0 && acceptstatus.PickerPersonId == PeopleId)
                {
                    result.Status = false;
                    result.Message = "पिकर में मेकर और चेकर एक ही व्यक्ति नहीं हो सकते।";
                }
                else
                {
                    result.Status = false;
                    result.Message = "Something went wrong";
                }
            }
            return result;
        }

        [HttpGet]
        [Route("CheckerGrabbedPickerList/{PeopleId}/{Skip}/{Take}")]
        public async Task<PickerResMsg> CheckerGrabbedPickerList(int PeopleId, int Skip, int Take)
        {
            var manager = new PickerManager();
            return await manager.CheckerGrabbedPickerList(PeopleId, Skip, Take);
        }


        [HttpPost]
        [Route("CheckerStartPicking/{PickerId}/{UserId}")]
        public async Task<PickerResMsg> CheckerStartPicking(int PickerId, int UserId)  //type = start / end
        {
            var result = new PickerResMsg();

            PickerTimer PickerTimers = new PickerTimer();
            //ResMsg res;
            //string Msg = "";
            //bool status = false;
            if (PickerId > 0)
            {
                using (AuthContext context = new AuthContext())
                {
                    var OrderPickerMaster = context.OrderPickerMasterDb.Where(x => x.Id == PickerId && x.IsDeleted == false).FirstOrDefault();
                    if (OrderPickerMaster != null && !OrderPickerMaster.IsCanceled)
                    {
                        if (OrderPickerMaster.Status != 6)
                        {
                            bool PickListAnyInProcess = context.PickerTimerDb.Any(x => x.CreatedBy == UserId && x.Id != OrderPickerMaster.Id && x.Type == 1 && x.EndTime == null);

                            if (!PickListAnyInProcess)
                            {
                                var PickerTimer = context.PickerTimerDb.Where(x => x.OrderPickerMasterId == PickerId && x.Type == 1).ToList();
                                if (PickerTimer.Count == 0)
                                {
                                    PickerTimers.OrderPickerMasterId = OrderPickerMaster.Id;
                                    PickerTimers.StartTime = DateTime.Now;
                                    PickerTimers.EndTime = null;
                                    PickerTimers.CreatedBy = UserId;
                                    PickerTimers.Type = 1;//Checker
                                    context.PickerTimerDb.Add(PickerTimers);

                                    OrderPickerMaster.Status = 1;// 
                                    OrderPickerMaster.ApproverId = UserId;// Checker id, 

                                    OrderPickerMaster.ModifiedDate = indianTime;
                                    context.Entry(OrderPickerMaster).State = EntityState.Modified;

                                    if (context.Commit() > 0)
                                    {
                                        result.Status = true;
                                        result.Message = "Picker Start Successfully";

                                    }
                                }
                                else if (PickerTimer.All(x => x.EndTime != null))
                                {
                                    //Restart Pick
                                    PickerTimers.OrderPickerMasterId = OrderPickerMaster.Id;
                                    PickerTimers.StartTime = DateTime.Now;
                                    PickerTimers.EndTime = null;
                                    PickerTimers.CreatedBy = UserId;
                                    PickerTimers.Type = 1;//Checker
                                    context.PickerTimerDb.Add(PickerTimers);
                                    OrderPickerMaster.Status = 1;//
                                    OrderPickerMaster.ApproverId = UserId;// Checker id, 
                                    OrderPickerMaster.ModifiedDate = indianTime;
                                    context.Entry(OrderPickerMaster).State = EntityState.Modified;
                                    if (context.Commit() > 0)
                                    {
                                        result.Status = true;
                                        result.Message = "Picker Restart Successfully";
                                    }
                                }
                                else
                                {
                                    PickerTimers = PickerTimer.Where(x => x.EndTime == null).FirstOrDefault();
                                    result.Status = true;
                                }
                            }
                            else
                            {
                                result.Message = "पिछला पिकर प्रगति पर होने के कारण आप नया पिकर प्रारंभ नहीं कर सकते|";
                            }
                        }
                        else
                        {
                            result.Message = "पिकर पहले ही सबमिट किया जा चुका है|";
                        }
                    }
                    else
                    {
                        result.Message = "Picker Canceled";
                    }
                }
            }

            return result;
        }

        [HttpGet]
        [Route("CheckerOrderPickingDetail/{OrderPickerMasterId}")]
        public async Task<PickerResMsg> CheckerOrderPickingDetail(long OrderPickerMasterId)
        {
            var manager = new PickerManager();
            return await manager.CheckerOrderPickingDetail(OrderPickerMasterId);

        }

        [HttpPost]
        [Route("CheckerSearchViaBarcode")]
        public async Task<PickerResMsg> CheckerSearchViaBarcode(SearchViaBarcodeDc SearchViaBarcode)
        {
            var manager = new PickerManager();
            return await manager.CheckerSearchViaBarcode(SearchViaBarcode);
        }



        [HttpPost]
        [Route("CheckerScan")]
        public async Task<PickerResMsg> CheckerScanItem(ScanItemDc ScanItem)
        {
            var manager = new PickerManager();
            return await manager.CheckerScanItem(ScanItem);
        }


        [HttpPost]
        [Route("CheckerSubmitPickedBatch")]
        public async Task<PickerResMsg> CheckerSubmitPickedBatch(SubmitPickedBatchDc SubmitPickedBatch)
        {

            var manager = new PickerManager();
            return await manager.CheckerSubmitPickedBatch(SubmitPickedBatch);
        }


        [HttpPost]
        [Route("CheckerApproved")]
        [AllowAnonymous]
        public async Task<HttpResponseMessage> CheckerApproved(ReviewerApprovedDc ReviewerApprovedAndDispatchedobj)
        {
            OrderPickerMaster pickerMaster = new OrderPickerMaster();
            ResMsg res;
            string Msg = "";
            bool status = true;
            People user = null;

            if (ReviewerApprovedAndDispatchedobj.PickerId > 0 && ReviewerApprovedAndDispatchedobj.UserId > 0 && ReviewerApprovedAndDispatchedobj.PickerId > 0)
            {
                using (AuthContext context = new AuthContext())
                {
                    List<OrderDispatchedMaster> OrderDispatch = null;
                    user = context.Peoples.FirstOrDefault(x => x.PeopleID == ReviewerApprovedAndDispatchedobj.UserId && x.Active);
                    pickerMaster = context.OrderPickerMasterDb.Where(x => x.Id == ReviewerApprovedAndDispatchedobj.PickerId && x.IsDeleted == false).Include(c => c.orderPickerDetails).FirstOrDefault();
                    var warehouse = context.Warehouses.FirstOrDefault(x => x.WarehouseId == pickerMaster.WarehouseId);
                    if (warehouse.IsStopCurrentStockTrans)
                    {
                        var response = new ResMsg()
                        {
                            Status = false,
                            Message = "Inventory Transactions are currently disabled for this warehouse... Please try after some time"
                        };

                        return Request.CreateResponse(HttpStatusCode.OK, response);
                    }
                    if (pickerMaster != null && pickerMaster.IsComplted && user != null)
                    {
                        if (pickerMaster.IsDispatched)
                        {
                            Msg = "Picker is already Approved";
                            status = false;
                        }
                        else if (pickerMaster.IsApproved)
                        {
                            Msg = "Picker is already Approved";
                            status = false;
                        }
                        else if (pickerMaster.IsCanceled)
                        {
                            Msg = "Picker is Cancelled";
                            status = false;
                        }
                        //else if (!pickerMaster.IsInventory)
                        //{
                        //    Msg = "Picker is not verifed by inventory super wiser";
                        //    status = false;
                        //}
                        bool IsCompleted = pickerMaster.orderPickerDetails.Any(x => x.Status == 4);
                        if (IsCompleted)
                        {
                            Msg = "Picker List Still in Pending, you can't Approved";
                            status = false;
                        }
                        if (status)
                        {
                            var orderids = pickerMaster.orderPickerDetails.Where(x => x.Status == 2).Select(x => x.OrderId).Distinct().ToList();

                            //&& (x.Status == "ReadyToPick" || x.Status == "Ready to Dispatch")
                            var orderLists = await context.DbOrderMaster.Where(x => orderids.Contains(x.OrderId) && (x.Status == "ReadyToPick" || x.Status == "Ready to Dispatch")).Include(x => x.orderDetails).ToListAsync();
                            if (orderLists.Count == 0)
                            {
                                Msg = " There is no order are eligible to Approved";
                                status = false;
                            }

                            if (orderLists != null && orderLists.Any())
                            {
                                OrderDispatch = Mapper.Map(orderLists).ToANew<List<OrderDispatchedMaster>>();
                                if (OrderDispatch != null && OrderDispatch.Any())
                                {
                                    var customerIds = OrderDispatch.Select(x => x.CustomerId).Distinct().ToList();
                                    DataTable dt = new DataTable();
                                    dt.Columns.Add("IntValue");
                                    foreach (var item in customerIds)
                                    {
                                        var dr = dt.NewRow();
                                        dr["IntValue"] = item;
                                        dt.Rows.Add(dr);
                                    }
                                    var param = new SqlParameter("customerids", dt);
                                    param.SqlDbType = System.Data.SqlDbType.Structured;
                                    param.TypeName = "dbo.IntValues";
                                    var PickerCustomerList = context.Database.SqlQuery<PickerCustomerDc>("Exec Picker.GetCustomersByIds  @customerids", param).ToList();

                                    if (PickerCustomerList.Any(x => x.IsGstRequestPending))
                                    {
                                        var skCodes = PickerCustomerList.Where(x => x.IsGstRequestPending).Select(x => x.Skcode).Distinct().ToList();
                                        var erroresult = "Customer (" + string.Join(",", skCodes) + ") GST Reqest is Inprogress. Please coordinate with customer care.";
                                        Msg = erroresult;
                                        status = false;
                                        res = new ResMsg()
                                        {
                                            Status = status,
                                            Message = Msg
                                        };
                                        return Request.CreateResponse(HttpStatusCode.OK, res);
                                    }
                                    var param1 = new SqlParameter("PickerId", pickerMaster.Id);
                                    long? TripMasterId = context.Database.SqlQuery<long?>("exec operation.IsTripPicker @PickerId", param1).FirstOrDefault();
                                    People Dboyinfo = null;
                                    if (TripMasterId > 0)
                                    {
                                        Dboyinfo = context.Peoples.Where(x => x.PeopleID == pickerMaster.DBoyId && x.Active).FirstOrDefault();
                                        if (Dboyinfo == null)
                                        {
                                            Msg = "Trip dboy is inactive";
                                            status = false;
                                            res = new ResMsg()
                                            {
                                                Status = status,
                                                Message = Msg
                                            };
                                            return Request.CreateResponse(HttpStatusCode.OK, res);
                                        }
                                    }
                                    ReadyToPickDispatchedHelper helper = new ReadyToPickDispatchedHelper();
                                    Msg = helper.ReadyToPickDispatchedNEWAsync(OrderDispatch, ReviewerApprovedAndDispatchedobj.UserId, pickerMaster.Id, warehouse, user, TripMasterId, Dboyinfo, PickerCustomerList);
                                    status = true;
                                }
                            }

                        }
                    }
                    else
                    {
                        Msg = "Picker List Still in Pending, you can't Approved";
                        status = false;
                    }
                }
            }
            //PickerProcessIds.RemoveAll(x => x == ReviewerApprovedAndDispatchedobj.PickerId);
            res = new ResMsg()
            {
                Status = status,
                Message = Msg
            };
            return Request.CreateResponse(HttpStatusCode.OK, res);
        }



        #endregion





        #endregion

        #region Zila Picker Order
        public async Task<PickerResMsg> ZilaPickerSubmit(int OrderPickerMasterId, int UserId)
        {
            var result = new PickerResMsg();
            bool status = false;
            if (OrderPickerMasterId > 0 && UserId > 0)
            {
                OrderOutPublisher Publisher = new OrderOutPublisher();
                List<BatchCodeSubjectDc> PublisherPickerRejectStockList = new List<BatchCodeSubjectDc>();
                TransactionOptions option = new TransactionOptions();
                option.IsolationLevel = System.Transactions.IsolationLevel.RepeatableRead;
                option.Timeout = TimeSpan.FromSeconds(120);
                using (var dbContextTransaction = new TransactionScope(TransactionScopeOption.RequiresNew, option))
                {
                    using (AuthContext context = new AuthContext())
                    {
                        var orderPickerMaster = context.OrderPickerMasterDb.Where(x => x.Id == OrderPickerMasterId && x.IsDeleted == false).Include(x => x.orderPickerDetails).FirstOrDefault();
                        if (orderPickerMaster != null && !orderPickerMaster.IsCanceled && !orderPickerMaster.IsComplted)
                        {
                            bool IsCompleted = orderPickerMaster.orderPickerDetails.Any(x => x.Status == 0 && x.OrderPickerMasterId == orderPickerMaster.Id);
                            if (IsCompleted)
                            {
                                result.Status = false;
                                result.Message = "Picker List Still in Pending, you can't submit";
                                return result;
                            }
                            if (orderPickerMaster.IsDispatched)
                            {
                                result.Message = "Picker is already Approved";
                                result.Status = false;
                                return result;

                            }
                            else if (orderPickerMaster.IsApproved)
                            {
                                result.Message = "Picker is already Approved";
                                result.Status = false;
                                return result;

                            }
                            else if (orderPickerMaster.IsCanceled)
                            {
                                result.Message = "Picker is Cancelled";
                                result.Status = false;
                                return result;

                            }
                            bool IsCompleted1 = orderPickerMaster.orderPickerDetails.Any(x => x.Status == 4);
                            if (IsCompleted1)
                            {
                                result.Message = "Picker List Still in Pending, you can't Approved";
                                result.Status = false;
                                return result;

                            }
                            else
                            {
                                var RejectedOrderids = orderPickerMaster.orderPickerDetails.Where(x => x.Status == 3).Select(x => x.OrderId).Distinct().ToList();
                                if (orderPickerMaster.orderPickerDetails.Any(x => RejectedOrderids.Contains(x.OrderId) && x.Status != 3))
                                {

                                    result.Status = false;
                                    result.Message = "Picker can't be submited, Due to following Orderids: " + string.Join(",", orderPickerMaster.orderPickerDetails.Where(x => RejectedOrderids.Contains(x.OrderId) && x.Status != 3).Select(x => x.OrderId).Distinct().ToList()) + " Please accept or reject all the items.";
                                    return result;
                                }

                                if (RejectedOrderids != null && RejectedOrderids.Any())
                                {
                                    var param = new SqlParameter("PickerId", orderPickerMaster.Id);
                                    long? TripMasterId = context.Database.SqlQuery<long?>("exec operation.IsTripPicker @PickerId", param).FirstOrDefault();
                                    if (TripMasterId > 0)
                                    {
                                        TripPlannerHelper triphelp = new TripPlannerHelper();
                                        bool IsSuccess = triphelp.RemovePickerOrderFromTrip(TripMasterId.Value, orderPickerMaster.Id, RejectedOrderids, context, UserId);
                                        if (!IsSuccess)
                                        {
                                            dbContextTransaction.Dispose();

                                            result.Status = false;
                                            result.Message = "Remove Picker Order From Trip Failed";
                                            return result;
                                        }
                                    }

                                    foreach (var Odid in RejectedOrderids)
                                    {
                                        OrderMaster omCheck = context.DbOrderMaster.Where(x => x.OrderId == Odid && x.Status == "ReadyToPick" && x.Deleted == false).Include(x => x.orderDetails).FirstOrDefault();
                                        if (omCheck != null)
                                        {
                                            foreach (var item in omCheck.orderDetails)
                                            {
                                                item.Status = "Pending";
                                                item.UpdatedDate = indianTime;
                                            }
                                            omCheck.Status = "Pending";
                                            omCheck.UpdatedDate = indianTime;
                                            context.Entry(omCheck).State = EntityState.Modified;
                                            #region Order History
                                            Model.OrderMasterHistories h1 = new Model.OrderMasterHistories();
                                            h1.orderid = omCheck.OrderId;
                                            h1.Status = omCheck.Status;
                                            h1.Reasoncancel = null;
                                            h1.Warehousename = omCheck.WarehouseName;
                                            h1.userid = UserId;
                                            h1.CreatedDate = DateTime.Now;
                                            context.OrderMasterHistoriesDB.Add(h1);
                                            #endregion
                                            if (context.Commit() > 0)
                                            {
                                                #region stock Hit
                                                //for currentstock
                                                MultiStockHelper<OnPickedCancelDc> MultiStockHelpers = new MultiStockHelper<OnPickedCancelDc>();
                                                List<OnPickedCancelDc> RTDOnPickedCancelList = new List<OnPickedCancelDc>();
                                                foreach (var StockHit in omCheck.orderDetails.Where(x => x.qty > 0))
                                                {
                                                    int qty = orderPickerMaster.orderPickerDetails.FirstOrDefault(x => x.OrderDetailsId == StockHit.OrderDetailsId).Qty;
                                                    if (qty > 0)
                                                    {
                                                        bool isfree = false;
                                                        string RefStockCode = omCheck.OrderType == 8 ? "CL" : "C";
                                                        if (StockHit.IsFreeItem && StockHit.IsDispatchedFreeStock)
                                                        {
                                                            RefStockCode = "F";
                                                            isfree = true;
                                                        }
                                                        RTDOnPickedCancelList.Add(new OnPickedCancelDc
                                                        {
                                                            ItemMultiMRPId = StockHit.ItemMultiMRPId,
                                                            OrderDispatchedDetailsId = StockHit.OrderDetailsId,
                                                            OrderId = StockHit.OrderId,
                                                            Qty = qty,
                                                            UserId = UserId,
                                                            WarehouseId = StockHit.WarehouseId,
                                                            IsFreeStock = isfree,
                                                            RefStockCode = RefStockCode
                                                        });
                                                    }
                                                }
                                                if (RTDOnPickedCancelList.Any())
                                                {
                                                    bool IsUpdate = MultiStockHelpers.MakeEntry(RTDOnPickedCancelList, "Stock_OnPickedCancel", context, dbContextTransaction);
                                                    if (!IsUpdate)
                                                    {
                                                        dbContextTransaction.Dispose();

                                                        result.Status = false;
                                                        result.Message = "Something went wrong";
                                                        return result;
                                                    }
                                                    #region BatchCode
                                                    foreach (var s in RTDOnPickedCancelList.Where(x => x.Qty > 0))
                                                    {
                                                        PublisherPickerRejectStockList.Add(new BatchCodeSubjectDc
                                                        {
                                                            ObjectDetailId = s.OrderDispatchedDetailsId,  // its OrderDetailsId
                                                            ObjectId = s.OrderId,
                                                            StockType = s.RefStockCode,
                                                            Quantity = s.Qty,
                                                            WarehouseId = s.WarehouseId,
                                                            ItemMultiMrpId = s.ItemMultiMRPId
                                                        });
                                                    }
                                                    #endregion
                                                }
                                                #endregion
                                            }
                                        }
                                    }

                                }
                                var Pickertimer = context.PickerTimerDb.Where(x => x.OrderPickerMasterId == orderPickerMaster.Id && x.EndTime == null && x.Type == 0).ToList();
                                if (Pickertimer != null)
                                {
                                    orderPickerMaster.ModifiedDate = indianTime;
                                    orderPickerMaster.ModifiedBy = orderPickerMaster.PickerPersonId;
                                }
                                bool IsNoorderChecker = orderPickerMaster.orderPickerDetails.Any(x => x.Status != 3);
                                if (IsNoorderChecker)
                                {
                                    orderPickerMaster.IsComplted = true;
                                    orderPickerMaster.SubmittedDate = indianTime;
                                    orderPickerMaster.Status = 2;// 2=Submitted(Maker) 
                                }
                                else
                                {
                                    orderPickerMaster.IsCanceled = true;
                                    orderPickerMaster.Status = 4; //Canceled
                                    orderPickerMaster.SubmittedDate = indianTime;
                                    orderPickerMaster.Comment = "System Canceled"; //Canceled
                                    context.Entry(orderPickerMaster).State = EntityState.Modified;
                                    if (Pickertimer != null)
                                    {
                                        Pickertimer.ForEach(x =>
                                        {
                                            x.EndTime = indianTime;
                                            context.Entry(x).State = EntityState.Modified;
                                        });
                                    }
                                    if (context.Commit() > 0)
                                    {
                                        List<BatchCodeSubjectDc> ZilaOrderQueue = new List<BatchCodeSubjectDc>();
                                        ZilaOrderQueue.Add(new BatchCodeSubjectDc
                                        {
                                            ObjectDetailId = orderPickerMaster.WarehouseId,
                                            ObjectId = Convert.ToInt64(RejectedOrderids.FirstOrDefault()),
                                            StockType = "",
                                            Quantity = 0,
                                            WarehouseId = orderPickerMaster.WarehouseId,
                                            ItemMultiMrpId = 0
                                        });
                                        if (ZilaOrderQueue != null && ZilaOrderQueue.Any())
                                        {
                                            AsyncContext.Run(() => Publisher.PublishZilaOrderQueue(ZilaOrderQueue));
                                        }
                                        dbContextTransaction.Complete();
                                        result.Status = true;
                                        result.Message = "Picker Reject Successfully!!";
                                        return result;
                                    }
                                    else
                                    {
                                        dbContextTransaction.Dispose();
                                        result.Status = false;
                                        result.Message = "Something went wrong";
                                        return result;
                                    }
                                }
                                orderPickerMaster.Status = 3; //Dispatched(ApprovedDispatched), RTD
                                orderPickerMaster.IsDispatched = true;
                                orderPickerMaster.ModifiedDate = indianTime;
                                orderPickerMaster.ModifiedBy = UserId;
                                orderPickerMaster.AgentId = 0;//
                                orderPickerMaster.DeliveryIssuanceId = null;
                                orderPickerMaster.TotalAssignmentAmount = 0;
                                orderPickerMaster.IsCheckerGrabbed = true;
                                orderPickerMaster.IsApproved = true;
                                orderPickerMaster.ApproverId = UserId;
                                orderPickerMaster.ApprovedDate = indianTime;
                                context.Entry(orderPickerMaster).State = EntityState.Modified;

                                if (Pickertimer != null)
                                {
                                    Pickertimer.ForEach(x =>
                                    {
                                        x.EndTime = indianTime;
                                        context.Entry(x).State = EntityState.Modified;
                                    });
                                }
                                if (context.Commit() > 0)
                                {
                                    var orderids = orderPickerMaster.orderPickerDetails.Where(x => x.Status == 1).Select(x => x.OrderId).Distinct().ToList();
                                    ResMsg response = await ZilaCheckerApproved(orderPickerMaster, orderids, UserId, context, dbContextTransaction);
                                    if (response.Status)
                                    {
                                        dbContextTransaction.Complete();
                                        //Msg = "Picker Submitted Successfully";
                                        status = true;
                                        result.Status = response.Status;
                                        result.Message = response.Message;
                                    }
                                    else
                                    {
                                        dbContextTransaction.Dispose();
                                        status = false;
                                        result.Status = response.Status;
                                        result.Message = response.Message;
                                    }
                                }
                                else
                                {
                                    //Msg = "Something went wrong";
                                    status = false;
                                    result.Status = false;
                                    result.Message = "Something went wrong";
                                }
                            }

                        }
                        else
                        {
                            if (orderPickerMaster.IsCanceled) { result.Message = "Picker Canceled"; } // Msg = "Picker Canceled";
                            if (orderPickerMaster.IsComplted) { result.Message = "Picker is already submitted"; } //Msg = "Picker is already submitted";
                                                                                                                  //status = false;
                            result.Status = false;
                        }
                    }
                }
                if (PublisherPickerRejectStockList != null && PublisherPickerRejectStockList.Any() && status)
                {
                    Publisher.PlannedRejectPublish(PublisherPickerRejectStockList);
                }
            }
            return result;
        }
        public async Task<ResMsg> ZilaCheckerApproved(OrderPickerMaster orderPickerMaster, List<int> orderids, int UserId, AuthContext context, TransactionScope dbContextTransaction)
        {
            ResMsg res = new ResMsg();
            string Msg = "";
            bool status = true;
            People user = null;
            if (orderids != null && orderids.Any() && UserId > 0)
            {
                List<OrderDispatchedMaster> OrderDispatch = null;
                user = context.Peoples.FirstOrDefault(x => x.PeopleID == UserId && x.Active);
                if (orderids != null && orderids.Any())
                {
                    var ordermaster = context.DbOrderMaster.Where(x => orderids.Contains(x.OrderId) && x.Status == "ReadyToPick").Include(x => x.orderDetails).ToList();
                    OrderDispatch = Mapper.Map(ordermaster).ToANew<List<OrderDispatchedMaster>>();
                    if (OrderDispatch != null && OrderDispatch.Any())
                    {
                        var customerIds = OrderDispatch.Select(x => x.CustomerId).Distinct().ToList();
                        DataTable dt = new DataTable();
                        dt.Columns.Add("IntValue");
                        foreach (var item in customerIds)
                        {
                            var dr = dt.NewRow();
                            dr["IntValue"] = item;
                            dt.Rows.Add(dr);
                        }
                        var param = new SqlParameter("customerids", dt);
                        param.SqlDbType = System.Data.SqlDbType.Structured;
                        param.TypeName = "dbo.IntValues";
                        var PickerCustomerList = context.Database.SqlQuery<PickerCustomerDc>("Exec Picker.GetCustomersByIds  @customerids", param).ToList();

                        if (PickerCustomerList.Any(x => x.IsGstRequestPending))
                        {
                            var skCodes = PickerCustomerList.Where(x => x.IsGstRequestPending).Select(x => x.Skcode).Distinct().ToList();
                            var erroresult = "Customer (" + string.Join(",", skCodes) + ") GST Reqest is Inprogress. Please coordinate with customer care.";
                            Msg = erroresult;
                            status = false;
                            res = new ResMsg()
                            {
                                Status = status,
                                Message = Msg
                            };
                            return res;
                        }
                        var param1 = new SqlParameter("PickerId", orderPickerMaster.Id);
                        long? TripMasterId = context.Database.SqlQuery<long?>("exec operation.IsTripPicker @PickerId", param1).FirstOrDefault();
                        People Dboyinfo = null;
                        Dboyinfo = context.Peoples.Where(x => x.WarehouseId == orderPickerMaster.WarehouseId && (x.Desgination == "Delivery Boy" || x.Department == "Delivery Boy") && x.Active).FirstOrDefault();
                        if (Dboyinfo == null)
                        {
                            Msg = "Trip dboy is inactive";
                            status = false;
                            res = new ResMsg()
                            {
                                Status = status,
                                Message = Msg
                            };
                            return res;
                        }
                        ReadyToPickDispatchedHelper helper = new ReadyToPickDispatchedHelper();
                        var response = helper.ZilaReadyToPickDispatchedNEWAsync(OrderDispatch, UserId, orderPickerMaster, TripMasterId, Dboyinfo, PickerCustomerList, context, dbContextTransaction);



                        res = new ResMsg()
                        {
                            Status = response.Status,
                            Message = response.Message
                        };
                        return res;
                    }
                }
            }
            return res;
        }
        #endregion

    }
}
public class paggingDTO
{
    public List<OrderPickerMasterDc> PickerList { get; set; }
    public int Totalcount { get; set; }
}



