using AgileObjects.AgileMapper;
using AgileObjects.AgileMapper.Extensions;
using AngularJSAuthentication.API.Controllers;
using AngularJSAuthentication.API.Helpers;
using AngularJSAuthentication.API.Managers.TripPlanner;
using AngularJSAuthentication.API.Results;
using AngularJSAuthentication.BusinessLayer.Managers.TripPlanner;
using AngularJSAuthentication.Common.Enums;
using AngularJSAuthentication.DataContracts;
using AngularJSAuthentication.DataContracts.Masters;
using AngularJSAuthentication.DataContracts.Mongo;
using AngularJSAuthentication.DataContracts.ROC;
using AngularJSAuthentication.DataContracts.Transaction;
using AngularJSAuthentication.DataContracts.Transaction.OrderProcess;
using AngularJSAuthentication.DataContracts.Transaction.Stocks;
using AngularJSAuthentication.DataContracts.Transaction.TripPlanner;
using AngularJSAuthentication.DataContracts.TripPlanner;
using AngularJSAuthentication.Model;
using AngularJSAuthentication.Model.TripPlanner;
using AngularJSAuthentication.ORTools.Helpers;
using AngularJSAuthentication.ORTools.Managers;
using AngularJSAuthentication.ORTools.ViewModel;
using GenricEcommers.Models;
using LinqKit;
using Nito.AsyncEx;
using NLog;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Transactions;

namespace AngularJSAuthentication.API.Helper
{
    public class TripPlannerHelper
    {
        public static Logger logger = LogManager.GetCurrentClassLogger();
        public static List<int> AssignmentInProcess = new List<int>();
        public void InsertTrip(List<TripPlannerMaster> tripPlannerMasterList)
        {
            if (tripPlannerMasterList != null && tripPlannerMasterList.Any())
            {
                using (var authContext = new AuthContext())
                {
                    foreach (var tripPlannerMaster in tripPlannerMasterList)
                    {
                        authContext.TripPlannerMasters.Add(tripPlannerMaster);
                    }
                    authContext.Commit();
                }
            }
        }

        public ResMsg AssignmentCreateUpdate(long TripPlannerMasterId, int userid, AuthContext context, TransactionScope scope)
        {
            ResMsg res = new ResMsg();
            List<DeliveryIssuance> AddAssignmentlist = new List<DeliveryIssuance>();
            var peopledata = context.Peoples.Where(x => x.PeopleID == userid && x.Deleted == false).FirstOrDefault(); //userdata
            var tripPlannerConfirmedMaster = context.TripPlannerConfirmedMasters.Where(y => y.TripPlannerMasterId == TripPlannerMasterId).FirstOrDefault();
            var warehouse = context.Warehouses.Where(x => x.WarehouseId == tripPlannerConfirmedMaster.WarehouseId).FirstOrDefault();//get deboy data

            var query = from tcm in context.TripPlannerConfirmedMasters
                        join td in context.TripPlannerConfirmedDetails on tcm.Id equals td.TripPlannerConfirmedMasterId
                        join to in context.TripPlannerConfirmedOrders on td.Id equals to.TripPlannerConfirmedDetailId
                        where tcm.TripPlannerMasterId == TripPlannerMasterId && to.IsActive == true && to.IsDeleted == false
                        //&& tcm.TripDate == DateTime.Today
                        select new
                        {
                            orderId = to.OrderId,
                            IsManuallyAdded = to.IsManuallyAdded,
                            TripPlannerConfirmedMasterId = tcm.Id
                        };
            var result = query.ToList();
            if (result != null && result.Any() && peopledata != null)
            {
                var OrderIds = result.Select(x => x.orderId).ToList();
                //all order
                var orderDMLists = context.OrderDispatchedMasters.Where(c => OrderIds.Contains(c.OrderId) && (c.Status == "Ready to Dispatch" || c.Status == "Delivery Redispatch")).Include("orderDetails").ToList();
                if (orderDMLists.Any() && orderDMLists == null)
                {
                    res = new ResMsg()
                    {
                        Status = false,
                        Message = "OrderId not found !!"
                    };
                    return res;
                }
                if (orderDMLists.Any(x => string.IsNullOrEmpty(x.invoice_no)))
                {
                    res = new ResMsg()
                    {
                        Status = false,
                        Message = "invoice_no not found !!" + string.Join(",", orderDMLists.Where(x => string.IsNullOrEmpty(x.invoice_no)).Select(x => x.OrderId).ToList())
                    };
                    return res;
                }
                var tripPlannerConfirmedMasterId = result.Select(x => x.TripPlannerConfirmedMasterId).FirstOrDefault();
                //var cluser = context.Clusters.Where(x => x.ClusterId == tripPlannerConfirmedMaster.ClusterId).FirstOrDefault();
                //get deboy data
                var dboyMaster = context.DboyMasters.Where(x => x.Id == tripPlannerConfirmedMaster.DboyId).FirstOrDefault();
                var DBoypeople = context.Peoples.Where(x => x.PeopleID == dboyMaster.PeopleId).FirstOrDefault();//get deboy data

                //end


                List<long> RedispatchOrderIds = new List<long>();
                List<OrderDispatchedMaster> RedispatchOrder = new List<OrderDispatchedMaster>();
                List<int> returnIds = new List<int>();
                List<OrderDispatchedMaster> ReturnOrder = new List<OrderDispatchedMaster>();
                List<DeliveryIssuance> ReturndeliveryIssuanceLists = new List<DeliveryIssuance>();
                List<DeliveryIssuance> deliveryIssuanceLists = new List<DeliveryIssuance>();
                List<OrderMaster> existReturnOrder = new List<OrderMaster>();
                List<int> returnOrderIds = new List<int>();
                #region Sales Return Checking Exist Return Order
                //var tripConfirmMasterData = context.TripPlannerConfirmedMasters.Where(x => x.Id == tripPlannerConfirmedMasterId).FirstOrDefault();
                //var returnOrderData = context.TripPlannerConfirmedDetails.Where(x =>
                //x.TripPlannerConfirmedMasterId == tripPlannerConfirmedMasterId && x.CommaSeparatedOrderList != null
                //&& x.CustomerId > 0 && x.IsActive == true && x.IsDeleted == false).Select(x => x.CommaSeparatedOrderList).ToList();

                var ordermasterids = orderDMLists.Select(x => x.OrderId).Distinct().ToList();
                var orderMatser = context.DbOrderMaster.Where(x => ordermasterids.Contains(x.OrderId)).ToList();
                if (result != null)
                {
                    existReturnOrder = orderMatser.Where(x => OrderIds.Contains(x.OrderId) && x.OrderType == 3).ToList();
                    if (existReturnOrder.Count > 0)
                    {
                        //Redispatched && Return Order
                        returnIds = existReturnOrder.Select(x => x.OrderId).ToList();
                        ReturnOrder = orderDMLists.Where(x => returnIds.Contains(x.OrderId) && (x.Status == "Ready to Dispatch" || x.Status == "Delivery Redispatch")).ToList();
                        ReturndeliveryIssuanceLists = context.DeliveryIssuanceDb.Where(x => x.TripPlannerConfirmedMasterId == tripPlannerConfirmedMasterId && x.Status == "SavedAsDraft" && x.AssignmentType == 4).Include(x => x.details).ToList();

                    }
                }
                #endregion
                //Redispatched
                //if(existReturnOrder.Count > 0 && existReturnOrder.Count != returnOrderIds.Count)
                //{
                //    RedispatchOrderIds = result.Select(x => x.orderId).ToList();
                //    RedispatchOrder = orderDMLists.Where(x => RedispatchOrderIds.Contains(x.OrderId) && (x.Status == "Delivery Redispatch" || x.Status == "Ready to Dispatch")).ToList();
                //    deliveryIssuanceLists = context.DeliveryIssuanceDb.Where(x => x.TripPlannerConfirmedMasterId == tripPlannerConfirmedMasterId && x.Status == "SavedAsDraft").Include(x => x.details).ToList();
                //}
                //else
                //{
                var existReturnOrderids = orderMatser.Where(x => OrderIds.Contains(x.OrderId) && x.OrderType == 3).Select(x => x.OrderId).Distinct().ToList();
                RedispatchOrder = orderDMLists.Where(x => OrderIds.Contains(x.OrderId) && (x.Status == "Delivery Redispatch") && !existReturnOrderids.Contains(x.OrderId)).ToList();
                deliveryIssuanceLists = context.DeliveryIssuanceDb.Where(x => x.TripPlannerConfirmedMasterId == tripPlannerConfirmedMasterId && x.Status == "SavedAsDraft" && x.AssignmentType != 4).Include(x => x.details).ToList();
                //}


                //  Redispatch Assignmnet create
                //var warehouse = context.Warehouses.FirstOrDefault(x => x.WarehouseId == tripPlannerConfirmedMaster.WarehouseId);
                if (tripPlannerConfirmedMaster.TripTypeEnum == 3 || tripPlannerConfirmedMaster.TripTypeEnum == 4)//Damage Order Trip && Non Sellable
                {
                    if (orderDMLists != null && orderDMLists.Any())
                    {
                        var ListdeliveryIssuance = deliveryIssuanceLists.Where(x => x.TripPlannerConfirmedMasterId == tripPlannerConfirmedMaster.Id && x.Status == "SavedAsDraft" && x.AssignmentType == 0).ToList();
                        if (peopledata != null && ListdeliveryIssuance.Count() == 0)
                        {
                            List<DeliveryIssuance> DeliveryIssuanceList = orderDMLists.GroupBy(x => x.OrderId)
                            .Select(g => new DeliveryIssuance
                            {
                                userid = userid,
                                WarehouseId = warehouse.WarehouseId,
                                DisplayName = DBoypeople.DisplayName,
                                PeopleID = DBoypeople.PeopleID,
                                AgentId = (int)tripPlannerConfirmedMaster.AgentId,
                                Cityid = warehouse.Cityid,
                                CreatedDate = DateTime.Now,
                                UpdatedDate = DateTime.Now,
                                CompanyId = 1,
                                TripPlannerConfirmedMasterId = tripPlannerConfirmedMaster.Id,
                                VehicleId = (int)tripPlannerConfirmedMaster.VehicleMasterId,
                                VehicleNumber = tripPlannerConfirmedMaster.VehicleNumber,
                                AssignmentType = 0,
                                details = orderDMLists.FirstOrDefault(v => v.OrderId == g.Select(s => s.OrderId).FirstOrDefault()).orderDetails
                                            .GroupBy(y => y.ItemMultiMRPId).Select(t =>
                                             new IssuanceDetails
                                             {
                                                 OrderId = string.Join(",", t.Select(s => s.OrderId).Distinct()),
                                                 OrderQty = string.Join(",", t.Select(a => String.Format("{0} - {1}", a.OrderId, a.qty)).ToArray()),
                                                 OrderDispatchedMasterId = t.Select(x => x.OrderDispatchedMasterId).FirstOrDefault(),
                                                 OrderDispatchedDetailsId = t.Select(x => x.OrderDispatchedDetailsId).FirstOrDefault(),
                                                 qty = t.Sum(x => x.qty),
                                                 itemNumber = t.FirstOrDefault().itemNumber,
                                                 ItemId = t.FirstOrDefault().ItemId,
                                                 itemname = t.FirstOrDefault().itemname
                                             }).ToList(),
                                TotalAssignmentAmount = OrderAmount(g.Select(s => s.OrderId).Distinct().ToList(), context),
                                OrderdispatchIds = string.Join(",", orderDMLists.Where(z => g.Select(s => s.OrderId).Distinct().ToList().Contains(z.OrderId)).Select(x => x.OrderDispatchedMasterId).Distinct()),
                                OrderIds = string.Join(",", g.Select(s => s.OrderId).Distinct()),
                                Status = "SavedAsDraft", //"Assigned",
                                IsActive = true,
                            }).ToList();
                            context.DeliveryIssuanceDb.AddRange(DeliveryIssuanceList);
                            context.Commit();

                            foreach (var item in DeliveryIssuanceList)
                            {
                                TripPickerAssignmentMapping tripPickerAssignmentMapping = new TripPickerAssignmentMapping
                                {
                                    TripPlannerConfirmedMasterId = item.TripPlannerConfirmedMasterId,
                                    OrderPickerMasterId = null,
                                    AssignmentId = item.DeliveryIssuanceId,
                                    CreatedBy = userid,
                                    ModifiedBy = null,
                                    CreatedDate = DateTime.Now,
                                    ModifiedDate = null,
                                    IsActive = true,
                                    IsDeleted = false
                                };
                                context.TripPickerAssignmentMapping.Add(tripPickerAssignmentMapping);
                                var OrderdispatchIds = item.OrderdispatchIds.Split(',').Select(Int32.Parse).FirstOrDefault();

                                var OrderDMaster = orderDMLists.Where(x => x.OrderDispatchedMasterId == OrderdispatchIds).FirstOrDefault();
                                if (OrderDMaster != null)
                                {
                                    OrderDMaster.UpdatedDate = DateTime.Now;
                                    OrderDMaster.DeliveryIssuanceIdOrderDeliveryMaster = item.DeliveryIssuanceId;
                                    OrderDMaster.DBoyId = DBoypeople.PeopleID;
                                    OrderDMaster.DboyMobileNo = DBoypeople.Mobile;
                                    OrderDMaster.DboyName = DBoypeople.DisplayName;
                                }
                                string Borderid = Convert.ToString(item.DeliveryIssuanceId);

                                string BorderCodeId = Borderid.PadLeft(9, '0');
                                temOrderQBcode code = context.AssignmentGenerateBarcode(BorderCodeId);
                                item.AssignmentBarcodeImage = code.BarcodeImage;//for assignment barcode
                                #region  DeliveryHistory
                                OrderDeliveryMasterHistories AssginDeli = new OrderDeliveryMasterHistories();
                                AssginDeli.DeliveryIssuanceId = item.DeliveryIssuanceId;
                                AssginDeli.Cityid = item.Cityid;
                                AssginDeli.city = item.city;
                                AssginDeli.DisplayName = item.DisplayName;
                                AssginDeli.Status = item.Status;
                                AssginDeli.WarehouseId = item.WarehouseId;
                                AssginDeli.PeopleID = item.PeopleID;
                                AssginDeli.VehicleId = item.VehicleId;
                                AssginDeli.VehicleNumber = item.VehicleNumber;
                                AssginDeli.RejectReason = item.RejectReason;
                                AssginDeli.OrderdispatchIds = item.OrderdispatchIds;
                                AssginDeli.OrderIds = item.OrderIds;
                                AssginDeli.Acceptance = item.Acceptance;
                                AssginDeli.IsActive = item.IsActive;
                                AssginDeli.IdealTime = item.IdealTime;
                                AssginDeli.TravelDistance = item.TravelDistance;
                                AssginDeli.CreatedDate = DateTime.Now;
                                AssginDeli.UpdatedDate = DateTime.Now;
                                AssginDeli.userid = userid;
                                if (peopledata.DisplayName == null)
                                {
                                    AssginDeli.UpdatedBy = peopledata.PeopleFirstName;
                                }
                                else
                                {
                                    AssginDeli.UpdatedBy = peopledata.DisplayName;
                                }
                                context.OrderDeliveryMasterHistoriesDB.Add(AssginDeli);
                                #endregion
                                context.Entry(OrderDMaster).State = EntityState.Modified;
                                context.Commit();
                            }
                        }
                        else
                        {
                            if (peopledata != null && ListdeliveryIssuance.Count() > 0 && ListdeliveryIssuance.Any())
                            {
                                if (tripPlannerConfirmedMaster.Id > 0)
                                {
                                    var deliveryIssuanceDb = context.DeliveryIssuanceDb.Where(x => x.TripPlannerConfirmedMasterId == tripPlannerConfirmedMaster.Id).ToList();
                                    foreach (var AssignmentObj in deliveryIssuanceDb)
                                    {
                                        AssignmentObj.Status = "Rejected";
                                        AssignmentObj.RejectReason = "System by Assignment Rejected No Order Found";
                                        AssignmentObj.IsActive = false;
                                        AssignmentObj.TripPlannerConfirmedMasterId = 0;
                                        AssignmentObj.UpdatedDate = DateTime.Now;
                                        context.Entry(AssignmentObj).State = EntityState.Modified;
                                    }
                                    var listAssignmentIds = deliveryIssuanceDb.Select(x => x.DeliveryIssuanceId).Distinct().ToList();
                                    var tripPickerAssignmentMapping = context.TripPickerAssignmentMapping.Where(x => listAssignmentIds.Contains(x.AssignmentId ?? 0) && x.IsDeleted == false).ToList();
                                    foreach (var item in tripPickerAssignmentMapping)
                                    {
                                        item.IsActive = false;
                                        item.IsDeleted = true;
                                        item.ModifiedDate = DateTime.Now;
                                        item.ModifiedBy = userid;
                                        context.Entry(item).State = EntityState.Modified;
                                    }
                                }
                                List<DeliveryIssuance> DeliveryIssuanceList = orderDMLists.GroupBy(x => x.OrderId)
                                .Select(g => new DeliveryIssuance
                                {
                                    userid = userid,
                                    WarehouseId = warehouse.WarehouseId,
                                    DisplayName = DBoypeople.DisplayName,
                                    PeopleID = DBoypeople.PeopleID,
                                    AgentId = (int)tripPlannerConfirmedMaster.AgentId,
                                    Cityid = warehouse.Cityid,
                                    CreatedDate = DateTime.Now,
                                    UpdatedDate = DateTime.Now,
                                    CompanyId = 1,
                                    TripPlannerConfirmedMasterId = tripPlannerConfirmedMaster.Id,
                                    VehicleId = (int)tripPlannerConfirmedMaster.VehicleMasterId,
                                    VehicleNumber = tripPlannerConfirmedMaster.VehicleNumber,
                                    AssignmentType = 0,
                                    details = orderDMLists.FirstOrDefault(v => v.OrderId == g.Select(s => s.OrderId).FirstOrDefault()).orderDetails
                                                .GroupBy(y => y.ItemMultiMRPId).Select(t =>
                                                 new IssuanceDetails
                                                 {
                                                     OrderId = string.Join(",", t.Select(s => s.OrderId).Distinct()),
                                                     OrderQty = string.Join(",", t.Select(a => String.Format("{0} - {1}", a.OrderId, a.qty)).ToArray()),
                                                     OrderDispatchedMasterId = t.Select(x => x.OrderDispatchedMasterId).FirstOrDefault(),
                                                     OrderDispatchedDetailsId = t.Select(x => x.OrderDispatchedDetailsId).FirstOrDefault(),
                                                     qty = t.Sum(x => x.qty),
                                                     itemNumber = t.FirstOrDefault().itemNumber,
                                                     ItemId = t.FirstOrDefault().ItemId,
                                                     itemname = t.FirstOrDefault().itemname
                                                 }).ToList(),
                                    TotalAssignmentAmount = OrderAmount(g.Select(s => s.OrderId).Distinct().ToList(), context),
                                    OrderdispatchIds = string.Join(",", orderDMLists.Where(z => g.Select(s => s.OrderId).Distinct().ToList().Contains(z.OrderId)).Select(x => x.OrderDispatchedMasterId).Distinct()),
                                    OrderIds = string.Join(",", g.Select(s => s.OrderId).Distinct()),
                                    Status = "SavedAsDraft", //"Assigned",
                                    IsActive = true,
                                }).ToList();
                                context.DeliveryIssuanceDb.AddRange(DeliveryIssuanceList);
                                context.Commit();

                                foreach (var item in DeliveryIssuanceList)
                                {
                                    TripPickerAssignmentMapping tripPickerAssignmentMapping = new TripPickerAssignmentMapping
                                    {
                                        TripPlannerConfirmedMasterId = item.TripPlannerConfirmedMasterId,
                                        OrderPickerMasterId = null,
                                        AssignmentId = item.DeliveryIssuanceId,
                                        CreatedBy = userid,
                                        ModifiedBy = null,
                                        CreatedDate = DateTime.Now,
                                        ModifiedDate = null,
                                        IsActive = true,
                                        IsDeleted = false
                                    };
                                    context.TripPickerAssignmentMapping.Add(tripPickerAssignmentMapping);
                                    var OrderdispatchIds = item.OrderdispatchIds.Split(',').Select(Int32.Parse).FirstOrDefault();

                                    var OrderDMaster = orderDMLists.Where(x => x.OrderDispatchedMasterId == OrderdispatchIds).FirstOrDefault();
                                    if (OrderDMaster != null)
                                    {
                                        OrderDMaster.UpdatedDate = DateTime.Now;
                                        OrderDMaster.DeliveryIssuanceIdOrderDeliveryMaster = item.DeliveryIssuanceId;
                                        OrderDMaster.DBoyId = DBoypeople.PeopleID;
                                        OrderDMaster.DboyMobileNo = DBoypeople.Mobile;
                                        OrderDMaster.DboyName = DBoypeople.DisplayName;
                                    }
                                    string Borderid = Convert.ToString(item.DeliveryIssuanceId);

                                    string BorderCodeId = Borderid.PadLeft(9, '0');
                                    temOrderQBcode code = context.AssignmentGenerateBarcode(BorderCodeId);
                                    item.AssignmentBarcodeImage = code.BarcodeImage;//for assignment barcode
                                    #region  DeliveryHistory
                                    OrderDeliveryMasterHistories AssginDeli = new OrderDeliveryMasterHistories();
                                    AssginDeli.DeliveryIssuanceId = item.DeliveryIssuanceId;
                                    AssginDeli.Cityid = item.Cityid;
                                    AssginDeli.city = item.city;
                                    AssginDeli.DisplayName = item.DisplayName;
                                    AssginDeli.Status = item.Status;
                                    AssginDeli.WarehouseId = item.WarehouseId;
                                    AssginDeli.PeopleID = item.PeopleID;
                                    AssginDeli.VehicleId = item.VehicleId;
                                    AssginDeli.VehicleNumber = item.VehicleNumber;
                                    AssginDeli.RejectReason = item.RejectReason;
                                    AssginDeli.OrderdispatchIds = item.OrderdispatchIds;
                                    AssginDeli.OrderIds = item.OrderIds;
                                    AssginDeli.Acceptance = item.Acceptance;
                                    AssginDeli.IsActive = item.IsActive;
                                    AssginDeli.IdealTime = item.IdealTime;
                                    AssginDeli.TravelDistance = item.TravelDistance;
                                    AssginDeli.CreatedDate = DateTime.Now;
                                    AssginDeli.UpdatedDate = DateTime.Now;
                                    AssginDeli.userid = userid;
                                    if (peopledata.DisplayName == null)
                                    {
                                        AssginDeli.UpdatedBy = peopledata.PeopleFirstName;
                                    }
                                    else
                                    {
                                        AssginDeli.UpdatedBy = peopledata.DisplayName;
                                    }
                                    context.OrderDeliveryMasterHistoriesDB.Add(AssginDeli);
                                    #endregion
                                    context.Entry(OrderDMaster).State = EntityState.Modified;
                                    context.Commit();
                                }
                            }
                            else
                            {
                                res = new ResMsg()
                                {
                                    Status = false,
                                    Message = "Please refesh page then create Assignment"
                                };
                                return res;
                            }
                        }
                    }
                    else
                    {
                        var tripPlannerConfirmedMasterid = new SqlParameter("@TripPlannerConfirmedMasterId", tripPlannerConfirmedMaster.Id);
                        var listAssignmentIds = context.Database.SqlQuery<int>("Operation.TripPlanner_CheckRejectedAssignment @TripPlannerConfirmedMasterId", tripPlannerConfirmedMasterid).ToList();
                        if (listAssignmentIds.Any() && listAssignmentIds != null)
                        {
                            var deliveryIssuanceDb = context.DeliveryIssuanceDb.Where(x => listAssignmentIds.Contains(x.DeliveryIssuanceId)).ToList();
                            foreach (var AssignmentObj in deliveryIssuanceDb)
                            {
                                AssignmentObj.Status = "Rejected";
                                AssignmentObj.RejectReason = "System by Assignment Rejected No Order Found";
                                AssignmentObj.IsActive = false;
                                AssignmentObj.TripPlannerConfirmedMasterId = 0;
                                AssignmentObj.UpdatedDate = DateTime.Now;
                                context.Entry(AssignmentObj).State = EntityState.Modified;
                            }
                            var tripPickerAssignmentMapping = context.TripPickerAssignmentMapping.Where(x => listAssignmentIds.Contains(x.AssignmentId ?? 0) && x.IsDeleted == false).ToList();
                            foreach (var item in tripPickerAssignmentMapping)
                            {
                                item.IsActive = false;
                                item.IsDeleted = true;
                                item.ModifiedDate = DateTime.Now;
                                item.ModifiedBy = userid;
                                context.Entry(item).State = EntityState.Modified;
                            }
                        }
                        context.Commit();
                    }
                }
                else
                {
                    if (ReturnOrder != null && ReturnOrder.Any())
                    {
                        var ProcOrderIds = ReturnOrder.Select(x => x.OrderId).Distinct().ToList();
                        System.Data.DataTable dt = new System.Data.DataTable();
                        dt.Columns.Add("IntValue");
                        foreach (var item in ProcOrderIds)
                        {
                            var dr = dt.NewRow();
                            dr["IntValue"] = item;
                            dt.Rows.Add(dr);
                        }
                        var param = new SqlParameter("orderids", dt);
                        param.SqlDbType = System.Data.SqlDbType.Structured;
                        param.TypeName = "dbo.IntValues";
                        var OrderDispatchedDetailsList = context.Database.SqlQuery<OrderDispatchedDetailsDC>("Exec Operation.GetItemAutoPickAssignment @orderids", param).ToList();

                        var separateList = OrderDispatchedDetailsList.GroupBy(x => new { x.OrderId })
                              .Select(x => new
                              {
                                  OrderId = x.Key.OrderId,
                                  subcatid = x.All(z => z.PrepareSeparateAssignment == false) || (x.All(z => z.PrepareSeparateAssignment == true) && x.Select(z => z.SubCategoryId).Distinct().Count() > 1) ? 0 :
                                             x.All(z => z.PrepareSeparateAssignment == true) && x.Select(z => z.SubCategoryId).Distinct().Count() == 1
                                             ? x.FirstOrDefault().SubCategoryId : 0
                              }).ToList();
                        var ListdeliveryIssuance = ReturndeliveryIssuanceLists.Where(x => x.TripPlannerConfirmedMasterId == tripPlannerConfirmedMaster.Id && x.Status == "SavedAsDraft" && x.AssignmentType == 4).ToList();
                        if (peopledata != null && ListdeliveryIssuance.Count() == 0)
                        {
                            List<DeliveryIssuance> DeliveryIssuanceList = separateList.GroupBy(x => x.subcatid)
                            .Select(g => new DeliveryIssuance
                            {
                                userid = userid,
                                WarehouseId = warehouse.WarehouseId,
                                DisplayName = DBoypeople.DisplayName,
                                PeopleID = DBoypeople.PeopleID,
                                AgentId = (int)tripPlannerConfirmedMaster.AgentId,
                                Cityid = warehouse.Cityid,
                                CreatedDate = DateTime.Now,
                                UpdatedDate = DateTime.Now,
                                CompanyId = 1,
                                TripPlannerConfirmedMasterId = tripPlannerConfirmedMaster.Id,
                                VehicleId = (int)tripPlannerConfirmedMaster.VehicleMasterId,
                                VehicleNumber = tripPlannerConfirmedMaster.VehicleNumber,
                                AssignmentType = (long)AssignmentTypeEnum.Return,
                                details = OrderDispatchedDetailsList.Where(z => g.Select(s => s.OrderId).Distinct().ToList().Contains(z.OrderId))
                                            .GroupBy(y => y.ItemMultiMRPId).Select(t =>
                                             new IssuanceDetails
                                             {
                                                 OrderId = string.Join(",", t.Select(s => s.OrderId).Distinct()),
                                                 OrderQty = string.Join(",", t.Select(a => String.Format("{0} - {1}", a.OrderId, a.qty)).ToArray()),
                                                 OrderDispatchedMasterId = t.Select(x => x.OrderDispatchedMasterId).FirstOrDefault(),
                                                 OrderDispatchedDetailsId = t.Select(x => x.OrderDispatchedDetailsId).FirstOrDefault(),
                                                 qty = t.Sum(x => x.qty),
                                                 itemNumber = t.FirstOrDefault().itemNumber,
                                                 ItemId = t.FirstOrDefault().ItemId,
                                                 itemname = t.FirstOrDefault().itemname
                                             }).ToList(),
                                TotalAssignmentAmount = OrderAmount(g.Select(s => s.OrderId).Distinct().ToList(), context),
                                OrderdispatchIds = string.Join(",", OrderDispatchedDetailsList.Where(z => g.Select(s => s.OrderId).Distinct().ToList().Contains(z.OrderId)).Select(x => x.OrderDispatchedMasterId).Distinct()),
                                OrderIds = string.Join(",", g.Select(s => s.OrderId).Distinct()),
                                Status = "SavedAsDraft", //"Assigned",
                                IsActive = true,
                            }).ToList();
                            context.DeliveryIssuanceDb.AddRange(DeliveryIssuanceList);
                            context.Commit();

                            foreach (var item in DeliveryIssuanceList)
                            {
                                TripPickerAssignmentMapping tripPickerAssignmentMapping = new TripPickerAssignmentMapping
                                {
                                    TripPlannerConfirmedMasterId = item.TripPlannerConfirmedMasterId,
                                    OrderPickerMasterId = null,
                                    AssignmentId = item.DeliveryIssuanceId,
                                    CreatedBy = userid,
                                    ModifiedBy = null,
                                    CreatedDate = DateTime.Now,
                                    ModifiedDate = null,
                                    IsActive = true,
                                    IsDeleted = false
                                };
                                context.TripPickerAssignmentMapping.Add(tripPickerAssignmentMapping);
                                var OrderdispatchIds = item.OrderdispatchIds.Split(',').Select(Int32.Parse).ToList();
                                foreach (var od in OrderdispatchIds)
                                {
                                    var OrderDMaster = ReturnOrder.Where(x => x.OrderDispatchedMasterId == od).FirstOrDefault();
                                    OrderDMaster.UpdatedDate = DateTime.Now;
                                    OrderDMaster.DeliveryIssuanceIdOrderDeliveryMaster = item.DeliveryIssuanceId;
                                    OrderDMaster.DBoyId = DBoypeople.PeopleID;
                                    OrderDMaster.DboyMobileNo = DBoypeople.Mobile;
                                    OrderDMaster.DboyName = DBoypeople.DisplayName;
                                }
                                string Borderid = Convert.ToString(item.DeliveryIssuanceId);
                                string BorderCodeId = Borderid.PadLeft(9, '0');
                                temOrderQBcode code = context.AssignmentGenerateBarcode(BorderCodeId);
                                item.AssignmentBarcodeImage = code.BarcodeImage;//for assignment barcode
                                #region  DeliveryHistory
                                OrderDeliveryMasterHistories AssginDeli = new OrderDeliveryMasterHistories();
                                AssginDeli.DeliveryIssuanceId = item.DeliveryIssuanceId;
                                AssginDeli.Cityid = item.Cityid;
                                AssginDeli.city = item.city;
                                AssginDeli.DisplayName = item.DisplayName;
                                AssginDeli.Status = item.Status;
                                AssginDeli.WarehouseId = item.WarehouseId;
                                AssginDeli.PeopleID = item.PeopleID;
                                AssginDeli.VehicleId = item.VehicleId;
                                AssginDeli.VehicleNumber = item.VehicleNumber;
                                AssginDeli.RejectReason = item.RejectReason;
                                AssginDeli.OrderdispatchIds = item.OrderdispatchIds;
                                AssginDeli.OrderIds = item.OrderIds;
                                AssginDeli.Acceptance = item.Acceptance;
                                AssginDeli.IsActive = item.IsActive;
                                AssginDeli.IdealTime = item.IdealTime;
                                AssginDeli.TravelDistance = item.TravelDistance;
                                AssginDeli.CreatedDate = DateTime.Now;
                                AssginDeli.UpdatedDate = DateTime.Now;
                                AssginDeli.userid = userid;
                                if (peopledata.DisplayName == null)
                                {
                                    AssginDeli.UpdatedBy = peopledata.PeopleFirstName;
                                }
                                else
                                {
                                    AssginDeli.UpdatedBy = peopledata.DisplayName;
                                }
                                context.OrderDeliveryMasterHistoriesDB.Add(AssginDeli);
                                #endregion
                                foreach (var items in ReturnOrder)
                                {
                                    context.Entry(items).State = EntityState.Modified;
                                }
                                context.Commit();
                            }
                        }
                        else
                        {
                            if (peopledata != null && ListdeliveryIssuance.Count() > 0 && ListdeliveryIssuance.Any())
                            {
                                foreach (var deliveryIssuance in ListdeliveryIssuance)
                                {
                                    deliveryIssuance.UpdatedDate = DateTime.Now;
                                    double TotalAssignmentAmount = 0;
                                    //Order
                                    List<OrderDispatchedMaster> orderDispatchedMasterList = new List<OrderDispatchedMaster>();
                                    var OrdersIds = ReturnOrder.Select(x => x.OrderId).Distinct().ToList();
                                    deliveryIssuance.OrderdispatchIds = "";
                                    deliveryIssuance.OrderIds = "";
                                    foreach (var orderId in OrdersIds)
                                    {
                                        var DispatchedOder = orderDMLists.Where(x => x.OrderId == orderId && (x.Status == "Ready to Dispatch" || x.Status == "Delivery Redispatch")).FirstOrDefault();//
                                        if (DispatchedOder != null)
                                        {
                                            if (deliveryIssuance.OrderdispatchIds == "" && deliveryIssuance.OrderIds == "")
                                            {
                                                deliveryIssuance.OrderdispatchIds = Convert.ToString(DispatchedOder.OrderDispatchedMasterId);
                                                deliveryIssuance.OrderIds = Convert.ToString(DispatchedOder.OrderId);
                                                orderDispatchedMasterList.Add(DispatchedOder);
                                            }
                                            else
                                            {
                                                deliveryIssuance.OrderdispatchIds = deliveryIssuance.OrderdispatchIds + "," + Convert.ToString(DispatchedOder.OrderDispatchedMasterId);
                                                deliveryIssuance.OrderIds = deliveryIssuance.OrderIds + "," + Convert.ToString(DispatchedOder.OrderId);
                                                orderDispatchedMasterList.Add(DispatchedOder);
                                            }
                                            TotalAssignmentAmount = TotalAssignmentAmount + DispatchedOder.GrossAmount;
                                        }
                                    }

                                    if (orderDispatchedMasterList.Any() && orderDispatchedMasterList != null)
                                    {
                                        deliveryIssuance.AssignedOrders = orderDispatchedMasterList;
                                        var IdOrder = orderDispatchedMasterList.Select(x => x.OrderId).ToList();
                                        deliveryIssuance.details = new List<IssuanceDetails>();
                                        deliveryIssuance.TotalAssignmentAmount = TotalAssignmentAmount;
                                        var OrderDispatchedDetailssList = context.OrderDispatchedDetailss.Where(x => IdOrder.Contains(x.OrderId) && x.qty > 0).ToList();
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
                                        deliveryIssuance.details = Assignmentpicklist;
                                        deliveryIssuance.PeopleID = DBoypeople.PeopleID;
                                        deliveryIssuance.DisplayName = DBoypeople.DisplayName;
                                        context.Entry(deliveryIssuance).State = EntityState.Modified;
                                        var OrderdispatchIds = deliveryIssuance.OrderdispatchIds.Split(',').Select(Int32.Parse).ToList();
                                        foreach (var OrderdispatchId in OrderdispatchIds)
                                        {
                                            var OrderDMaster = orderDispatchedMasterList.Where(x => x.OrderDispatchedMasterId == OrderdispatchId).FirstOrDefault();
                                            OrderDMaster.UpdatedDate = DateTime.Now;
                                            OrderDMaster.DeliveryIssuanceIdOrderDeliveryMaster = deliveryIssuance.DeliveryIssuanceId;
                                            OrderDMaster.DBoyId = DBoypeople.PeopleID;
                                            OrderDMaster.DboyMobileNo = DBoypeople.Mobile;
                                            OrderDMaster.DboyName = DBoypeople.DisplayName;
                                        }
                                        #region deliveryIssuance Orderid Is null
                                        var orderids = deliveryIssuance.OrderIds.Split(',').Select(Int32.Parse).ToList();
                                        var orderDispatchedMasters = context.OrderDispatchedMasters.Where(x => x.DeliveryIssuanceIdOrderDeliveryMaster == deliveryIssuance.DeliveryIssuanceId).ToList();
                                        var OrderDispachedOrderids = orderDispatchedMasters.Select(x => x.OrderId).ToList();
                                        var orderidnotAssignment = orderDispatchedMasters.Where(x => !orderids.Contains(x.OrderId)).ToList();
                                        foreach (var od in orderidnotAssignment)
                                        {
                                            var updateorderDispatchedMaster = orderDispatchedMasters.Where(x => x.OrderId == od.OrderId).FirstOrDefault();
                                            updateorderDispatchedMaster.DeliveryIssuanceIdOrderDeliveryMaster = null;
                                            context.Entry(updateorderDispatchedMaster).State = EntityState.Modified;
                                        }
                                        #endregion
                                        #region  DeliveryHistory
                                        OrderDeliveryMasterHistories AssginDeli = new OrderDeliveryMasterHistories();
                                        AssginDeli.DeliveryIssuanceId = deliveryIssuance.DeliveryIssuanceId;
                                        //AssginDeli.OrderId = delivery.o
                                        AssginDeli.Cityid = deliveryIssuance.Cityid;
                                        AssginDeli.city = deliveryIssuance.city;
                                        AssginDeli.DisplayName = deliveryIssuance.DisplayName;
                                        AssginDeli.Status = deliveryIssuance.Status;
                                        AssginDeli.WarehouseId = deliveryIssuance.WarehouseId;
                                        AssginDeli.PeopleID = deliveryIssuance.PeopleID;
                                        AssginDeli.VehicleId = deliveryIssuance.VehicleId;
                                        AssginDeli.VehicleNumber = deliveryIssuance.VehicleNumber;
                                        AssginDeli.RejectReason = "Edit Assignment";
                                        AssginDeli.OrderdispatchIds = deliveryIssuance.OrderdispatchIds;
                                        AssginDeli.OrderIds = deliveryIssuance.OrderIds;
                                        AssginDeli.Acceptance = deliveryIssuance.Acceptance;
                                        AssginDeli.IsActive = deliveryIssuance.IsActive;
                                        AssginDeli.IdealTime = deliveryIssuance.IdealTime;
                                        AssginDeli.TravelDistance = deliveryIssuance.TravelDistance;
                                        AssginDeli.CreatedDate = DateTime.Now;
                                        AssginDeli.UpdatedDate = DateTime.Now;
                                        AssginDeli.userid = userid;
                                        if (peopledata.DisplayName == null)
                                        {
                                            AssginDeli.UpdatedBy = peopledata.PeopleFirstName;
                                        }
                                        else
                                        {
                                            AssginDeli.UpdatedBy = peopledata.DisplayName;
                                        }
                                        context.OrderDeliveryMasterHistoriesDB.Add(AssginDeli);
                                        #endregion
                                        foreach (var item in orderDispatchedMasterList)
                                        {
                                            context.Entry(item).State = EntityState.Modified;
                                        }
                                        var tripPlannerConfirmedMasterid = new SqlParameter("@TripPlannerConfirmedMasterId", tripPlannerConfirmedMaster.Id);
                                        var listAssignmentIds = context.Database.SqlQuery<int>("Operation.TripPlanner_CheckRejectedAssignment @TripPlannerConfirmedMasterId", tripPlannerConfirmedMasterid).ToList();
                                        if (listAssignmentIds.Any() && listAssignmentIds != null)
                                        {
                                            var deliveryIssuanceDb = context.DeliveryIssuanceDb.Where(x => listAssignmentIds.Contains(x.DeliveryIssuanceId)).ToList();
                                            foreach (var AssignmentObj in deliveryIssuanceDb)
                                            {
                                                AssignmentObj.Status = "Rejected";
                                                AssignmentObj.RejectReason = "System by Assignment Rejected No Order Found";
                                                AssignmentObj.IsActive = false;
                                                AssignmentObj.TripPlannerConfirmedMasterId = 0;
                                                AssignmentObj.UpdatedDate = DateTime.Now;
                                                context.Entry(AssignmentObj).State = EntityState.Modified;
                                            }
                                            var tripPickerAssignmentMapping = context.TripPickerAssignmentMapping.Where(x => listAssignmentIds.Contains(x.AssignmentId ?? 0) && x.IsDeleted == false).ToList();
                                            foreach (var item in tripPickerAssignmentMapping)
                                            {
                                                item.IsActive = false;
                                                item.IsDeleted = true;
                                                item.ModifiedDate = DateTime.Now;
                                                item.ModifiedBy = userid;
                                                context.Entry(item).State = EntityState.Modified;
                                            }
                                        }
                                        context.Commit();
                                    }
                                    else
                                    {
                                        res = new ResMsg()
                                        {
                                            Status = false,
                                            Message = "OrderId not found !!"
                                        };
                                        return res;
                                    }
                                }
                            }
                            else
                            {
                                res = new ResMsg()
                                {
                                    Status = false,
                                    Message = "Please refesh page then create Assignment"
                                };
                                return res;
                            }
                        }
                    }
                    if (RedispatchOrder != null && RedispatchOrder.Any())
                    {
                        var ProcOrderIds = RedispatchOrder.Select(x => x.OrderId).Distinct().ToList();
                        System.Data.DataTable dt = new System.Data.DataTable();
                        dt.Columns.Add("IntValue");
                        foreach (var item in ProcOrderIds)
                        {
                            var dr = dt.NewRow();
                            dr["IntValue"] = item;
                            dt.Rows.Add(dr);
                        }
                        var param = new SqlParameter("orderids", dt);
                        param.SqlDbType = System.Data.SqlDbType.Structured;
                        param.TypeName = "dbo.IntValues";
                        var OrderDispatchedDetailsList = context.Database.SqlQuery<OrderDispatchedDetailsDC>("Exec Operation.GetItemAutoPickAssignment @orderids", param).ToList();

                        var separateList = OrderDispatchedDetailsList.GroupBy(x => new { x.OrderId })
                              .Select(x => new
                              {
                                  OrderId = x.Key.OrderId,
                                  subcatid = x.All(z => z.PrepareSeparateAssignment == false) || (x.All(z => z.PrepareSeparateAssignment == true) && x.Select(z => z.SubCategoryId).Distinct().Count() > 1) ? 0 :
                                             x.All(z => z.PrepareSeparateAssignment == true) && x.Select(z => z.SubCategoryId).Distinct().Count() == 1
                                             ? x.FirstOrDefault().SubCategoryId : 0
                              }).ToList();
                        var ListdeliveryIssuance = deliveryIssuanceLists.Where(x => x.TripPlannerConfirmedMasterId == tripPlannerConfirmedMaster.Id && x.Status == "SavedAsDraft" && x.AssignmentType == 2).ToList();
                        if (peopledata != null && ListdeliveryIssuance.Count() == 0)
                        {
                            List<DeliveryIssuance> DeliveryIssuanceList = separateList.GroupBy(x => x.subcatid)
                            .Select(g => new DeliveryIssuance
                            {
                                userid = userid,
                                WarehouseId = warehouse.WarehouseId,
                                DisplayName = DBoypeople.DisplayName,
                                PeopleID = DBoypeople.PeopleID,
                                AgentId = (int)tripPlannerConfirmedMaster.AgentId,
                                Cityid = warehouse.Cityid,
                                CreatedDate = DateTime.Now,
                                UpdatedDate = DateTime.Now,
                                CompanyId = 1,
                                TripPlannerConfirmedMasterId = tripPlannerConfirmedMaster.Id,
                                VehicleId = (int)tripPlannerConfirmedMaster.VehicleMasterId,
                                VehicleNumber = tripPlannerConfirmedMaster.VehicleNumber,
                                AssignmentType = (long)AssignmentTypeEnum.Redispatch,
                                details = OrderDispatchedDetailsList.Where(z => g.Select(s => s.OrderId).Distinct().ToList().Contains(z.OrderId))
                                            .GroupBy(y => y.ItemMultiMRPId).Select(t =>
                                             new IssuanceDetails
                                             {
                                                 OrderId = string.Join(",", t.Select(s => s.OrderId).Distinct()),
                                                 OrderQty = string.Join(",", t.Select(a => String.Format("{0} - {1}", a.OrderId, a.qty)).ToArray()),
                                                 OrderDispatchedMasterId = t.Select(x => x.OrderDispatchedMasterId).FirstOrDefault(),
                                                 OrderDispatchedDetailsId = t.Select(x => x.OrderDispatchedDetailsId).FirstOrDefault(),
                                                 qty = t.Sum(x => x.qty),
                                                 itemNumber = t.FirstOrDefault().itemNumber,
                                                 ItemId = t.FirstOrDefault().ItemId,
                                                 itemname = t.FirstOrDefault().itemname
                                             }).ToList(),
                                TotalAssignmentAmount = OrderAmount(g.Select(s => s.OrderId).Distinct().ToList(), context),
                                OrderdispatchIds = string.Join(",", OrderDispatchedDetailsList.Where(z => g.Select(s => s.OrderId).Distinct().ToList().Contains(z.OrderId)).Select(x => x.OrderDispatchedMasterId).Distinct()),
                                OrderIds = string.Join(",", g.Select(s => s.OrderId).Distinct()),
                                Status = "SavedAsDraft", //"Assigned",
                                IsActive = true,
                            }).ToList();
                            context.DeliveryIssuanceDb.AddRange(DeliveryIssuanceList);
                            context.Commit();

                            foreach (var item in DeliveryIssuanceList)
                            {
                                TripPickerAssignmentMapping tripPickerAssignmentMapping = new TripPickerAssignmentMapping
                                {
                                    TripPlannerConfirmedMasterId = item.TripPlannerConfirmedMasterId,
                                    OrderPickerMasterId = null,
                                    AssignmentId = item.DeliveryIssuanceId,
                                    CreatedBy = userid,
                                    ModifiedBy = null,
                                    CreatedDate = DateTime.Now,
                                    ModifiedDate = null,
                                    IsActive = true,
                                    IsDeleted = false
                                };
                                context.TripPickerAssignmentMapping.Add(tripPickerAssignmentMapping);
                                var OrderdispatchIds = item.OrderdispatchIds.Split(',').Select(Int32.Parse).ToList();
                                foreach (var od in OrderdispatchIds)
                                {
                                    var OrderDMaster = RedispatchOrder.Where(x => x.OrderDispatchedMasterId == od).FirstOrDefault();
                                    OrderDMaster.UpdatedDate = DateTime.Now;
                                    OrderDMaster.DeliveryIssuanceIdOrderDeliveryMaster = item.DeliveryIssuanceId;
                                    OrderDMaster.DBoyId = DBoypeople.PeopleID;
                                    OrderDMaster.DboyMobileNo = DBoypeople.Mobile;
                                    OrderDMaster.DboyName = DBoypeople.DisplayName;
                                }
                                string Borderid = Convert.ToString(item.DeliveryIssuanceId);
                                string BorderCodeId = Borderid.PadLeft(9, '0');
                                temOrderQBcode code = context.AssignmentGenerateBarcode(BorderCodeId);
                                item.AssignmentBarcodeImage = code.BarcodeImage;//for assignment barcode
                                #region  DeliveryHistory
                                OrderDeliveryMasterHistories AssginDeli = new OrderDeliveryMasterHistories();
                                AssginDeli.DeliveryIssuanceId = item.DeliveryIssuanceId;
                                AssginDeli.Cityid = item.Cityid;
                                AssginDeli.city = item.city;
                                AssginDeli.DisplayName = item.DisplayName;
                                AssginDeli.Status = item.Status;
                                AssginDeli.WarehouseId = item.WarehouseId;
                                AssginDeli.PeopleID = item.PeopleID;
                                AssginDeli.VehicleId = item.VehicleId;
                                AssginDeli.VehicleNumber = item.VehicleNumber;
                                AssginDeli.RejectReason = item.RejectReason;
                                AssginDeli.OrderdispatchIds = item.OrderdispatchIds;
                                AssginDeli.OrderIds = item.OrderIds;
                                AssginDeli.Acceptance = item.Acceptance;
                                AssginDeli.IsActive = item.IsActive;
                                AssginDeli.IdealTime = item.IdealTime;
                                AssginDeli.TravelDistance = item.TravelDistance;
                                AssginDeli.CreatedDate = DateTime.Now;
                                AssginDeli.UpdatedDate = DateTime.Now;
                                AssginDeli.userid = userid;
                                if (peopledata.DisplayName == null)
                                {
                                    AssginDeli.UpdatedBy = peopledata.PeopleFirstName;
                                }
                                else
                                {
                                    AssginDeli.UpdatedBy = peopledata.DisplayName;
                                }
                                context.OrderDeliveryMasterHistoriesDB.Add(AssginDeli);
                                #endregion
                                foreach (var items in RedispatchOrder)
                                {
                                    context.Entry(items).State = EntityState.Modified;
                                }
                                context.Commit();
                            }
                        }
                        else
                        {
                            if (peopledata != null && ListdeliveryIssuance.Count() > 0 && ListdeliveryIssuance.Any())
                            {
                                foreach (var deliveryIssuance in ListdeliveryIssuance)
                                {
                                    deliveryIssuance.UpdatedDate = DateTime.Now;
                                    double TotalAssignmentAmount = 0;
                                    //Order
                                    List<OrderDispatchedMaster> orderDispatchedMasterList = new List<OrderDispatchedMaster>();
                                    var OrdersIds = RedispatchOrder.Select(x => x.OrderId).Distinct().ToList();
                                    deliveryIssuance.OrderdispatchIds = "";
                                    deliveryIssuance.OrderIds = "";
                                    foreach (var orderId in OrdersIds)
                                    {
                                        var DispatchedOder = orderDMLists.Where(x => x.OrderId == orderId && (x.Status == "Ready to Dispatch" || x.Status == "Delivery Redispatch")).FirstOrDefault();//
                                        if (DispatchedOder != null)
                                        {
                                            if (deliveryIssuance.OrderdispatchIds == "" && deliveryIssuance.OrderIds == "")
                                            {
                                                deliveryIssuance.OrderdispatchIds = Convert.ToString(DispatchedOder.OrderDispatchedMasterId);
                                                deliveryIssuance.OrderIds = Convert.ToString(DispatchedOder.OrderId);
                                                orderDispatchedMasterList.Add(DispatchedOder);
                                            }
                                            else
                                            {
                                                deliveryIssuance.OrderdispatchIds = deliveryIssuance.OrderdispatchIds + "," + Convert.ToString(DispatchedOder.OrderDispatchedMasterId);
                                                deliveryIssuance.OrderIds = deliveryIssuance.OrderIds + "," + Convert.ToString(DispatchedOder.OrderId);
                                                orderDispatchedMasterList.Add(DispatchedOder);
                                            }
                                            TotalAssignmentAmount = TotalAssignmentAmount + DispatchedOder.GrossAmount;
                                        }
                                    }

                                    if (orderDispatchedMasterList.Any() && orderDispatchedMasterList != null)
                                    {
                                        deliveryIssuance.AssignedOrders = orderDispatchedMasterList;
                                        var IdOrder = orderDispatchedMasterList.Select(x => x.OrderId).ToList();
                                        deliveryIssuance.details = new List<IssuanceDetails>();
                                        deliveryIssuance.TotalAssignmentAmount = TotalAssignmentAmount;
                                        var OrderDispatchedDetailssList = context.OrderDispatchedDetailss.Where(x => IdOrder.Contains(x.OrderId) && x.qty > 0).ToList();
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
                                        deliveryIssuance.details = Assignmentpicklist;
                                        deliveryIssuance.PeopleID = DBoypeople.PeopleID;
                                        deliveryIssuance.DisplayName = DBoypeople.DisplayName;
                                        context.Entry(deliveryIssuance).State = EntityState.Modified;
                                        var OrderdispatchIds = deliveryIssuance.OrderdispatchIds.Split(',').Select(Int32.Parse).ToList();
                                        foreach (var OrderdispatchId in OrderdispatchIds)
                                        {
                                            var OrderDMaster = orderDispatchedMasterList.Where(x => x.OrderDispatchedMasterId == OrderdispatchId).FirstOrDefault();
                                            OrderDMaster.UpdatedDate = DateTime.Now;
                                            OrderDMaster.DeliveryIssuanceIdOrderDeliveryMaster = deliveryIssuance.DeliveryIssuanceId;
                                            OrderDMaster.DBoyId = DBoypeople.PeopleID;
                                            OrderDMaster.DboyMobileNo = DBoypeople.Mobile;
                                            OrderDMaster.DboyName = DBoypeople.DisplayName;
                                        }
                                        #region deliveryIssuance Orderid Is null
                                        var orderids = deliveryIssuance.OrderIds.Split(',').Select(Int32.Parse).ToList();
                                        var orderDispatchedMasters = context.OrderDispatchedMasters.Where(x => x.DeliveryIssuanceIdOrderDeliveryMaster == deliveryIssuance.DeliveryIssuanceId).ToList();
                                        var OrderDispachedOrderids = orderDispatchedMasters.Select(x => x.OrderId).ToList();
                                        var orderidnotAssignment = orderDispatchedMasters.Where(x => !orderids.Contains(x.OrderId)).ToList();
                                        foreach (var od in orderidnotAssignment)
                                        {
                                            var updateorderDispatchedMaster = orderDispatchedMasters.Where(x => x.OrderId == od.OrderId).FirstOrDefault();
                                            updateorderDispatchedMaster.DeliveryIssuanceIdOrderDeliveryMaster = null;
                                            context.Entry(updateorderDispatchedMaster).State = EntityState.Modified;
                                        }
                                        #endregion
                                        #region  DeliveryHistory
                                        OrderDeliveryMasterHistories AssginDeli = new OrderDeliveryMasterHistories();
                                        AssginDeli.DeliveryIssuanceId = deliveryIssuance.DeliveryIssuanceId;
                                        //AssginDeli.OrderId = delivery.o
                                        AssginDeli.Cityid = deliveryIssuance.Cityid;
                                        AssginDeli.city = deliveryIssuance.city;
                                        AssginDeli.DisplayName = deliveryIssuance.DisplayName;
                                        AssginDeli.Status = deliveryIssuance.Status;
                                        AssginDeli.WarehouseId = deliveryIssuance.WarehouseId;
                                        AssginDeli.PeopleID = deliveryIssuance.PeopleID;
                                        AssginDeli.VehicleId = deliveryIssuance.VehicleId;
                                        AssginDeli.VehicleNumber = deliveryIssuance.VehicleNumber;
                                        AssginDeli.RejectReason = "Edit Assignment";
                                        AssginDeli.OrderdispatchIds = deliveryIssuance.OrderdispatchIds;
                                        AssginDeli.OrderIds = deliveryIssuance.OrderIds;
                                        AssginDeli.Acceptance = deliveryIssuance.Acceptance;
                                        AssginDeli.IsActive = deliveryIssuance.IsActive;
                                        AssginDeli.IdealTime = deliveryIssuance.IdealTime;
                                        AssginDeli.TravelDistance = deliveryIssuance.TravelDistance;
                                        AssginDeli.CreatedDate = DateTime.Now;
                                        AssginDeli.UpdatedDate = DateTime.Now;
                                        AssginDeli.userid = userid;
                                        if (peopledata.DisplayName == null)
                                        {
                                            AssginDeli.UpdatedBy = peopledata.PeopleFirstName;
                                        }
                                        else
                                        {
                                            AssginDeli.UpdatedBy = peopledata.DisplayName;
                                        }
                                        context.OrderDeliveryMasterHistoriesDB.Add(AssginDeli);
                                        #endregion
                                        foreach (var item in orderDispatchedMasterList)
                                        {
                                            context.Entry(item).State = EntityState.Modified;
                                        }
                                        var tripPlannerConfirmedMasterid = new SqlParameter("@TripPlannerConfirmedMasterId", tripPlannerConfirmedMaster.Id);
                                        var listAssignmentIds = context.Database.SqlQuery<int>("Operation.TripPlanner_CheckRejectedAssignment @TripPlannerConfirmedMasterId", tripPlannerConfirmedMasterid).ToList();
                                        if (listAssignmentIds.Any() && listAssignmentIds != null)
                                        {
                                            var deliveryIssuanceDb = context.DeliveryIssuanceDb.Where(x => listAssignmentIds.Contains(x.DeliveryIssuanceId)).ToList();
                                            foreach (var AssignmentObj in deliveryIssuanceDb)
                                            {
                                                AssignmentObj.Status = "Rejected";
                                                AssignmentObj.RejectReason = "System by Assignment Rejected No Order Found";
                                                AssignmentObj.IsActive = false;
                                                AssignmentObj.TripPlannerConfirmedMasterId = 0;
                                                AssignmentObj.UpdatedDate = DateTime.Now;
                                                context.Entry(AssignmentObj).State = EntityState.Modified;
                                            }
                                            var tripPickerAssignmentMapping = context.TripPickerAssignmentMapping.Where(x => listAssignmentIds.Contains(x.AssignmentId ?? 0) && x.IsDeleted == false).ToList();
                                            foreach (var item in tripPickerAssignmentMapping)
                                            {
                                                item.IsActive = false;
                                                item.IsDeleted = true;
                                                item.ModifiedDate = DateTime.Now;
                                                item.ModifiedBy = userid;
                                                context.Entry(item).State = EntityState.Modified;
                                            }
                                        }
                                        context.Commit();
                                    }
                                    else
                                    {
                                        res = new ResMsg()
                                        {
                                            Status = false,
                                            Message = "OrderId not found !!"
                                        };
                                        return res;
                                    }
                                }
                            }
                            else
                            {
                                res = new ResMsg()
                                {
                                    Status = false,
                                    Message = "Please refesh page then create Assignment"
                                };
                                return res;
                            }
                        }
                    }
                    else
                    {
                        var tripPlannerConfirmedMasterid = new SqlParameter("@TripPlannerConfirmedMasterId", tripPlannerConfirmedMaster.Id);
                        var listAssignmentIds = context.Database.SqlQuery<int>("Operation.TripPlanner_CheckRejectedAssignment @TripPlannerConfirmedMasterId", tripPlannerConfirmedMasterid).ToList();
                        if (listAssignmentIds.Any() && listAssignmentIds != null)
                        {
                            var deliveryIssuanceDb = context.DeliveryIssuanceDb.Where(x => listAssignmentIds.Contains(x.DeliveryIssuanceId)).ToList();
                            foreach (var AssignmentObj in deliveryIssuanceDb)
                            {
                                AssignmentObj.Status = "Rejected";
                                AssignmentObj.RejectReason = "System by Assignment Rejected No Order Found";
                                AssignmentObj.IsActive = false;
                                AssignmentObj.TripPlannerConfirmedMasterId = 0;
                                AssignmentObj.UpdatedDate = DateTime.Now;
                                context.Entry(AssignmentObj).State = EntityState.Modified;
                            }
                            var tripPickerAssignmentMapping = context.TripPickerAssignmentMapping.Where(x => listAssignmentIds.Contains(x.AssignmentId ?? 0) && x.IsDeleted == false).ToList();
                            foreach (var item in tripPickerAssignmentMapping)
                            {
                                item.IsActive = false;
                                item.IsDeleted = true;
                                item.ModifiedDate = DateTime.Now;
                                item.ModifiedBy = userid;
                                context.Entry(item).State = EntityState.Modified;
                            }
                        }
                        context.Commit();
                    }
                }
                #region old code 13-04-2022
                //if (RedispatchOrder != null && RedispatchOrder.Any())
                //{
                //    DeliveryIssuance obj = new DeliveryIssuance();
                //    obj.AgentId = (int)tripPlannerConfirmedMaster.AgentId;
                //    obj.Cityid = warehouse.Cityid;
                //    obj.PeopleID = DBoypeople.PeopleID;//(int)tripPlannerConfirmedMaster.DboyId
                //    obj.VehicleId = (int)tripPlannerConfirmedMaster.VehicleMasterId;
                //    obj.VehicleNumber = tripPlannerConfirmedMaster.VehicleNumber;
                //    obj.WarehouseId = warehouse.WarehouseId;
                //    obj.TripPlannerConfirmedMasterId = tripPlannerConfirmedMaster.Id;
                //    obj.AssignedOrders = RedispatchOrder;
                //    obj.AssignmentType = (long)AssignmentTypeEnum.Redispatch;
                //    AddAssignmentlist.Add(obj);
                //}
                //foreach (var obj in AddAssignmentlist)
                //{
                //    var deliveryIssuance = deliveryIssuanceList.Where(x => x.TripPlannerConfirmedMasterId == obj.TripPlannerConfirmedMasterId && x.Status == "SavedAsDraft" && x.AssignmentType == obj.AssignmentType).FirstOrDefault();
                //    if (peopledata != null && deliveryIssuance == null)
                //    {
                //        obj.userid = userid;
                //        obj.CompanyId = 1;
                //        obj.WarehouseId = Warehouse.WarehouseId;
                //        obj.CreatedDate = DateTime.Now;
                //        obj.UpdatedDate = DateTime.Now;
                //        obj.OrderdispatchIds = "";
                //        obj.OrderIds = "";
                //        obj.AssignedOrders[0].userid = userid;
                //        double TotalAssignmentAmount = 0;
                //        var AddOrderDeliveryList = new List<OrderDeliveryMasterHistories>();
                //        List<OrderDispatchedMaster> orderDispatchedMasterList = new List<OrderDispatchedMaster>();
                //        foreach (var o in obj.AssignedOrders)
                //        {
                //            var DispatchedOder = orderDMLists.Where(x => x.OrderId == o.OrderId && (x.Status == "Ready to Dispatch" || x.Status == "Delivery Redispatch")).FirstOrDefault();//
                //            if (DispatchedOder != null)
                //            {
                //                if (obj.OrderdispatchIds == "" && obj.OrderIds == "")
                //                {
                //                    obj.OrderdispatchIds = Convert.ToString(o.OrderDispatchedMasterId);
                //                    obj.OrderIds = Convert.ToString(o.OrderId);

                //                    orderDispatchedMasterList.Add(o);
                //                }
                //                else
                //                {
                //                    obj.OrderdispatchIds = obj.OrderdispatchIds + "," + Convert.ToString(o.OrderDispatchedMasterId);
                //                    obj.OrderIds = obj.OrderIds + "," + Convert.ToString(o.OrderId);
                //                    orderDispatchedMasterList.Add(o);
                //                }
                //                TotalAssignmentAmount = TotalAssignmentAmount + DispatchedOder.GrossAmount;
                //            }
                //        }
                //        if (orderDispatchedMasterList.Any() && orderDispatchedMasterList != null)
                //        {
                //            obj.AssignedOrders = orderDispatchedMasterList;
                //            var IdOrder = orderDispatchedMasterList.Select(x => x.OrderId).ToList();
                //            obj.Status = "SavedAsDraft";
                //            obj.TotalAssignmentAmount = TotalAssignmentAmount;
                //            obj.IsActive = true;
                //            obj.DisplayName = DBoypeople.DisplayName;
                //            obj.CreatedBy = peopledata.DisplayName;
                //            obj.city = DBoypeople.city;
                //            var OrderDispatchedDetailssList = context.OrderDispatchedDetailss.Where(x => IdOrder.Contains(x.OrderId) && x.qty > 0).ToList();
                //            var Assignmentpicklist = OrderDispatchedDetailssList.GroupBy(x => x.ItemMultiMRPId).Select(t =>
                //              new IssuanceDetails
                //              {
                //                  OrderId = string.Join(",", OrderDispatchedDetailssList.Where(x => x.ItemMultiMRPId == t.Key).Select(x => x.OrderId).ToArray()),
                //                  OrderQty = string.Join(",", OrderDispatchedDetailssList.Where(x => x.ItemMultiMRPId == t.Key).Select(a => String.Format("{0} - {1}", a.OrderId, a.qty)).ToArray()),
                //                  OrderDispatchedMasterId = OrderDispatchedDetailssList.Where(x => x.ItemMultiMRPId == t.Key).Select(x => x.OrderDispatchedMasterId).FirstOrDefault(),
                //                  OrderDispatchedDetailsId = OrderDispatchedDetailssList.Where(x => x.ItemMultiMRPId == t.Key).Select(x => x.OrderDispatchedDetailsId).FirstOrDefault(),
                //                  qty = OrderDispatchedDetailssList.Where(x => x.ItemMultiMRPId == t.Key).Sum(x => x.qty),
                //                  itemNumber = OrderDispatchedDetailssList.FirstOrDefault(x => x.ItemMultiMRPId == t.Key).itemNumber,
                //                  ItemId = OrderDispatchedDetailssList.FirstOrDefault(x => x.ItemMultiMRPId == t.Key).ItemId,
                //                  itemname = OrderDispatchedDetailssList.FirstOrDefault(x => x.ItemMultiMRPId == t.Key).itemname
                //              }).ToList();

                //            obj.details = Assignmentpicklist;
                //            var DBoyorders = context.DeliveryIssuanceDb.Add(obj);
                //            context.Commit();
                //            TripPickerAssignmentMapping tripPickerAssignmentMapping = new TripPickerAssignmentMapping
                //            {
                //                TripPlannerConfirmedMasterId = obj.TripPlannerConfirmedMasterId,
                //                OrderPickerMasterId = null,
                //                AssignmentId = obj.DeliveryIssuanceId,
                //                CreatedBy = userid,
                //                ModifiedBy = null,
                //                CreatedDate = DateTime.Now,
                //                ModifiedDate = null,
                //                IsActive = true,
                //                IsDeleted = false
                //            };
                //            context.TripPickerAssignmentMapping.Add(tripPickerAssignmentMapping);
                //            foreach (var od in obj.AssignedOrders)
                //            {
                //                var OrderDMaster = orderDispatchedMasterList.Where(x => x.OrderDispatchedMasterId == od.OrderDispatchedMasterId).FirstOrDefault();
                //                OrderDMaster.UpdatedDate = DateTime.Now;
                //                OrderDMaster.DeliveryIssuanceIdOrderDeliveryMaster = DBoyorders.DeliveryIssuanceId;
                //                OrderDMaster.DBoyId = DBoypeople.PeopleID;
                //                OrderDMaster.DboyMobileNo = DBoypeople.Mobile;
                //                OrderDMaster.DboyName = DBoypeople.DisplayName;
                //            }
                //            string Borderid = Convert.ToString(DBoyorders.DeliveryIssuanceId);
                //            string BorderCodeId = Borderid.PadLeft(9, '0');
                //            temOrderQBcode code = context.AssignmentGenerateBarcode(BorderCodeId);
                //            DBoyorders.AssignmentBarcodeImage = code.BarcodeImage;//for assignment barcode
                //            #region  DeliveryHistory
                //            OrderDeliveryMasterHistories AssginDeli = new OrderDeliveryMasterHistories();
                //            AssginDeli.DeliveryIssuanceId = DBoyorders.DeliveryIssuanceId;
                //            AssginDeli.Cityid = obj.Cityid;
                //            AssginDeli.city = obj.city;
                //            AssginDeli.DisplayName = obj.DisplayName;
                //            AssginDeli.Status = obj.Status;
                //            AssginDeli.WarehouseId = obj.WarehouseId;
                //            AssginDeli.PeopleID = obj.PeopleID;
                //            AssginDeli.VehicleId = obj.VehicleId;
                //            AssginDeli.VehicleNumber = obj.VehicleNumber;
                //            AssginDeli.RejectReason = obj.RejectReason;
                //            AssginDeli.OrderdispatchIds = obj.OrderdispatchIds;
                //            AssginDeli.OrderIds = obj.OrderIds;
                //            AssginDeli.Acceptance = obj.Acceptance;
                //            AssginDeli.IsActive = obj.IsActive;
                //            AssginDeli.IdealTime = obj.IdealTime;
                //            AssginDeli.TravelDistance = obj.TravelDistance;
                //            AssginDeli.CreatedDate = DateTime.Now;
                //            AssginDeli.UpdatedDate = DateTime.Now;
                //            AssginDeli.userid = userid;
                //            if (peopledata.DisplayName == null)
                //            {
                //                AssginDeli.UpdatedBy = peopledata.PeopleFirstName;
                //            }
                //            else
                //            {
                //                AssginDeli.UpdatedBy = peopledata.DisplayName;
                //            }
                //            context.OrderDeliveryMasterHistoriesDB.Add(AssginDeli);
                //            #endregion
                //            foreach (var item in orderDispatchedMasterList)
                //            {
                //                context.Entry(item).State = EntityState.Modified;
                //            }
                //            context.Commit();
                //        }
                //        else
                //        {
                //            res = new ResMsg()
                //            {
                //                Status = false,
                //                Message = "OrderId not found !!"
                //            };
                //            return res;
                //        }
                //    }
                //    else
                //    {
                //        if (peopledata != null && deliveryIssuance != null)
                //        {
                //            deliveryIssuance.UpdatedDate = DateTime.Now;
                //            deliveryIssuance.OrderdispatchIds = "";
                //            deliveryIssuance.OrderIds = "";
                //            obj.AssignedOrders[0].userid = userid;
                //            double TotalAssignmentAmount = 0;
                //            //Order
                //            List<OrderDispatchedMaster> orderDispatchedMasterList = new List<OrderDispatchedMaster>();
                //            foreach (var o in obj.AssignedOrders)
                //            {
                //                var DispatchedOder = orderDMLists.Where(x => x.OrderId == o.OrderId && (x.Status == "Ready to Dispatch" || x.Status == "Delivery Redispatch")).FirstOrDefault();//
                //                if (DispatchedOder != null)
                //                {
                //                    if (deliveryIssuance.OrderdispatchIds == "" && deliveryIssuance.OrderIds == "")
                //                    {
                //                        deliveryIssuance.OrderdispatchIds = Convert.ToString(o.OrderDispatchedMasterId);
                //                        deliveryIssuance.OrderIds = Convert.ToString(o.OrderId);
                //                        orderDispatchedMasterList.Add(o);
                //                    }
                //                    else
                //                    {
                //                        deliveryIssuance.OrderdispatchIds = deliveryIssuance.OrderdispatchIds + "," + Convert.ToString(o.OrderDispatchedMasterId);
                //                        deliveryIssuance.OrderIds = deliveryIssuance.OrderIds + "," + Convert.ToString(o.OrderId);
                //                        orderDispatchedMasterList.Add(o);
                //                    }
                //                    TotalAssignmentAmount = TotalAssignmentAmount + DispatchedOder.GrossAmount;
                //                }
                //            }

                //            if (orderDispatchedMasterList.Any() && orderDispatchedMasterList != null)
                //            {
                //                deliveryIssuance.AssignedOrders = orderDispatchedMasterList;
                //                var IdOrder = orderDispatchedMasterList.Select(x => x.OrderId).ToList();
                //                deliveryIssuance.details = new List<IssuanceDetails>();
                //                deliveryIssuance.TotalAssignmentAmount = TotalAssignmentAmount;
                //                var OrderDispatchedDetailssList = context.OrderDispatchedDetailss.Where(x => IdOrder.Contains(x.OrderId) && x.qty > 0).ToList();
                //                var Assignmentpicklist = OrderDispatchedDetailssList.GroupBy(x => x.ItemMultiMRPId).Select(t =>
                //                  new IssuanceDetails
                //                  {
                //                      OrderId = string.Join(",", OrderDispatchedDetailssList.Where(x => x.ItemMultiMRPId == t.Key).Select(x => x.OrderId).ToArray()),
                //                      OrderQty = string.Join(",", OrderDispatchedDetailssList.Where(x => x.ItemMultiMRPId == t.Key).Select(a => String.Format("{0} - {1}", a.OrderId, a.qty)).ToArray()),
                //                      OrderDispatchedMasterId = OrderDispatchedDetailssList.Where(x => x.ItemMultiMRPId == t.Key).Select(x => x.OrderDispatchedMasterId).FirstOrDefault(),
                //                      OrderDispatchedDetailsId = OrderDispatchedDetailssList.Where(x => x.ItemMultiMRPId == t.Key).Select(x => x.OrderDispatchedDetailsId).FirstOrDefault(),
                //                      qty = OrderDispatchedDetailssList.Where(x => x.ItemMultiMRPId == t.Key).Sum(x => x.qty),
                //                      itemNumber = OrderDispatchedDetailssList.FirstOrDefault(x => x.ItemMultiMRPId == t.Key).itemNumber,
                //                      ItemId = OrderDispatchedDetailssList.FirstOrDefault(x => x.ItemMultiMRPId == t.Key).ItemId,
                //                      itemname = OrderDispatchedDetailssList.FirstOrDefault(x => x.ItemMultiMRPId == t.Key).itemname
                //                  }).ToList();
                //                deliveryIssuance.details = Assignmentpicklist;
                //                context.Entry(deliveryIssuance).State = EntityState.Modified;
                //                foreach (var od in deliveryIssuance.AssignedOrders)
                //                {
                //                    var OrderDMaster = orderDispatchedMasterList.Where(x => x.OrderDispatchedMasterId == od.OrderDispatchedMasterId).FirstOrDefault();
                //                    OrderDMaster.UpdatedDate = DateTime.Now;
                //                    OrderDMaster.DeliveryIssuanceIdOrderDeliveryMaster = deliveryIssuance.DeliveryIssuanceId;
                //                    OrderDMaster.DBoyId = DBoypeople.PeopleID;
                //                    OrderDMaster.DboyMobileNo = DBoypeople.Mobile;
                //                    OrderDMaster.DboyName = DBoypeople.DisplayName;
                //                }
                //                #region deliveryIssuance Orderid Is null
                //                var orderids = deliveryIssuance.OrderIds.Split(',').Select(Int32.Parse).ToList();
                //                var orderDispatchedMasters = context.OrderDispatchedMasters.Where(x => x.DeliveryIssuanceIdOrderDeliveryMaster == deliveryIssuance.DeliveryIssuanceId).ToList();
                //                var OrderDispachedOrderids = orderDispatchedMasters.Select(x => x.OrderId).ToList();
                //                var orderidnotAssignment = orderDispatchedMasters.Where(x => !orderids.Contains(x.OrderId)).ToList();
                //                foreach (var od in orderidnotAssignment)
                //                {
                //                    var updateorderDispatchedMaster = orderDispatchedMasters.Where(x => x.OrderId == od.OrderId).FirstOrDefault();
                //                    updateorderDispatchedMaster.DeliveryIssuanceIdOrderDeliveryMaster = null;
                //                    context.Entry(updateorderDispatchedMaster).State = EntityState.Modified;
                //                }
                //                #endregion
                //                #region  DeliveryHistory
                //                OrderDeliveryMasterHistories AssginDeli = new OrderDeliveryMasterHistories();
                //                AssginDeli.DeliveryIssuanceId = deliveryIssuance.DeliveryIssuanceId;
                //                //AssginDeli.OrderId = delivery.o
                //                AssginDeli.Cityid = obj.Cityid;
                //                AssginDeli.city = deliveryIssuance.city;
                //                AssginDeli.DisplayName = deliveryIssuance.DisplayName;
                //                AssginDeli.Status = deliveryIssuance.Status;
                //                AssginDeli.WarehouseId = obj.WarehouseId;
                //                AssginDeli.PeopleID = deliveryIssuance.PeopleID;
                //                AssginDeli.VehicleId = obj.VehicleId;
                //                AssginDeli.VehicleNumber = obj.VehicleNumber;
                //                AssginDeli.RejectReason = "Edit Assignment";
                //                AssginDeli.OrderdispatchIds = obj.OrderdispatchIds;
                //                AssginDeli.OrderIds = obj.OrderIds;
                //                AssginDeli.Acceptance = deliveryIssuance.Acceptance;
                //                AssginDeli.IsActive = obj.IsActive;
                //                AssginDeli.IdealTime = deliveryIssuance.IdealTime;
                //                AssginDeli.TravelDistance = deliveryIssuance.TravelDistance;
                //                AssginDeli.CreatedDate = DateTime.Now;
                //                AssginDeli.UpdatedDate = DateTime.Now;
                //                AssginDeli.userid = userid;
                //                if (peopledata.DisplayName == null)
                //                {
                //                    AssginDeli.UpdatedBy = peopledata.PeopleFirstName;
                //                }
                //                else
                //                {
                //                    AssginDeli.UpdatedBy = peopledata.DisplayName;
                //                }
                //                context.OrderDeliveryMasterHistoriesDB.Add(AssginDeli);
                //                #endregion
                //                foreach (var item in orderDispatchedMasterList)
                //                {
                //                    context.Entry(item).State = EntityState.Modified;
                //                }
                //                var tripPlannerConfirmedMasterid = new SqlParameter("@TripPlannerConfirmedMasterId", tripPlannerConfirmedMaster.Id);
                //                var listAssignmentIds = context.Database.SqlQuery<int>("Operation.TripPlanner_CheckRejectedAssignment @TripPlannerConfirmedMasterId", tripPlannerConfirmedMasterid).ToList();
                //                if (listAssignmentIds.Any() && listAssignmentIds != null)
                //                {
                //                    var deliveryIssuanceDb = context.DeliveryIssuanceDb.Where(x => listAssignmentIds.Contains(x.DeliveryIssuanceId)).ToList();
                //                    foreach (var AssignmentObj in deliveryIssuanceDb)
                //                    {
                //                        AssignmentObj.Status = "Rejected";
                //                        AssignmentObj.RejectReason = "System by Assignment Rejected No Order Found";
                //                        AssignmentObj.IsActive = false;
                //                        AssignmentObj.TripPlannerConfirmedMasterId = 0;
                //                        AssignmentObj.UpdatedDate = DateTime.Now;
                //                        context.Entry(AssignmentObj).State = EntityState.Modified;
                //                    }
                //                }
                //                context.Commit();
                //            }
                //            else
                //            {
                //                res = new ResMsg()
                //                {
                //                    Status = false,
                //                    Message = "OrderId not found !!"
                //                };
                //                return res;
                //            }
                //        }
                //        else
                //        {
                //            res = new ResMsg()
                //            {
                //                Status = false,
                //                Message = "Please refesh page then create Assignment"
                //            };
                //            return res;
                //        }
                //    }
                //}
                #endregion
            }
            else
            {
                res = new ResMsg()
                {
                    Status = false,
                    Message = "Someting Went Wrong!!"
                };
                return res;
            }
            res = new ResMsg()
            {
                Status = true,
                Message = "Assignment created successfully!!"
            };
            return res;
        }
        public ResponceMsg AssignmentFinalize(long tripPlannerConfirmedMasterId, int userid, AuthContext context, TransactionScope scope, int startingKm, DateTime reportingTime, int? lateReportingTimeInMins, double? penaltyCharge, bool IsReturn = false)
        {
            bool TripstatusCheck = false;
            ResponceMsg msg;
            TripPlannerVehicleManager tripPlannerVehicleManager = new TripPlannerVehicleManager();
            var trip = context.TripPlannerConfirmedMasters.Where(x => x.IsActive == true && x.IsDeleted == false && x.Id == tripPlannerConfirmedMasterId).FirstOrDefault();
            var tripPlannerConfirmedDetails = context.TripPlannerConfirmedDetails.Where(x => x.IsActive == true && x.IsDeleted == false && x.TripPlannerConfirmedMasterId == tripPlannerConfirmedMasterId).ToList();
            bool IsPendingCityAndDamage = tripPlannerVehicleManager.checkCurrentTripStatusForCityAndDamage(trip.DboyId, context); //check current trip for city and damage
            bool IsPendingSKPAndKPP = tripPlannerVehicleManager.checkCurrentTripStatus(trip.DboyId, context); //check current trip            

            if (!IsPendingCityAndDamage && !IsPendingSKPAndKPP)
            {
                TripstatusCheck = true;
            }
            else if (IsPendingCityAndDamage && !IsPendingSKPAndKPP && (trip.TripTypeEnum == 0 || trip.TripTypeEnum == 3))//city and damage 
            {
                TripstatusCheck = false;
            }
            else if (!IsPendingCityAndDamage && IsPendingSKPAndKPP && (trip.TripTypeEnum == 1 || trip.TripTypeEnum == 2))//SKP and KPP
            {
                TripstatusCheck = true;
            }


            if (TripstatusCheck)
            {
                foreach (var item in tripPlannerConfirmedDetails)
                {
                    item.RealSequenceNo = item.SequenceNo;
                    context.Entry(item).State = EntityState.Modified;
                }
                trip.IsFreezed = true;
                trip.ModifiedBy = userid;
                trip.ModifiedDate = DateTime.Now;
                TripPlannerVehicles AddDetails = new TripPlannerVehicles()
                {
                    TripPlannerConfirmedMasterId = trip.Id,
                    ReminingTime = trip.TotalTimeInMins,
                    DistanceLeft = trip.TotalDistanceInMeter,
                    ReportingTime = reportingTime,
                    StartKm = startingKm,
                    PenaltyCharge = penaltyCharge,
                    LateReportingTimeInMins = lateReportingTimeInMins
                };
                bool value = tripPlannerVehicleManager.InsertTripDistance(AddDetails, userid, context);
                var peopledata = context.Peoples.Where(x => x.PeopleID == userid && x.Deleted == false).FirstOrDefault(); //userdata
                var query = from tcm in context.TripPlannerConfirmedMasters
                            join td in context.TripPlannerConfirmedDetails on tcm.Id equals td.TripPlannerConfirmedMasterId
                            join to in context.TripPlannerConfirmedOrders on td.Id equals to.TripPlannerConfirmedDetailId
                            where tcm.TripPlannerMasterId == trip.TripPlannerMasterId && to.IsActive == true && to.IsDeleted == false

                            select new
                            {
                                orderId = to.OrderId,
                                //IsManuallyAdded = to.IsManuallyAdded,
                                TripPlannerConfirmedMasterId = tcm.Id
                            };
                var result = query.ToList();
                if (result != null && result.Any() && peopledata != null)
                {
                    //all order
                    var tripPlannerConfirmedMasterIds = result.Select(x => x.TripPlannerConfirmedMasterId).FirstOrDefault();
                    var deliveryIssuanceList = context.DeliveryIssuanceDb.Where(x => x.TripPlannerConfirmedMasterId == tripPlannerConfirmedMasterIds && x.Status == "SavedAsDraft").ToList();
                    foreach (var obj in deliveryIssuanceList)
                    {
                        obj.UpdatedDate = DateTime.Now;
                        obj.Status = "Assigned";
                        var AOrderIds = obj.OrderIds.Split(',').Select(Int32.Parse).ToList();
                        var orderDMLists = context.OrderDispatchedMasters.Where(c => AOrderIds.Contains(c.OrderId)).Include(c => c.orderDetails).ToList();
                        var orderMLists = context.DbOrderMaster.Where(c => AOrderIds.Contains(c.OrderId)).Include("orderDetails").ToList();
                        var Orderids = orderDMLists.Select(x => x.OrderId).Distinct().ToList();
                        var ReturnOrderId = orderMLists.Where(x => Orderids.Contains(x.OrderId) && x.OrderType == 3).Select(x => x.OrderId).ToList();
                        var ROrderids = ReturnOrderId != null ? ReturnOrderId : new List<int>();
                        obj.PeopleID = orderDMLists.FirstOrDefault().DBoyId;
                        obj.DisplayName = orderDMLists.FirstOrDefault().DboyName;
                        foreach (var o in orderDMLists)

                        {
                            o.Status = "Issued";
                            o.UpdatedDate = DateTime.Now;
                            #region Code For OrderDeliveryMaster
                            OrderDeliveryMaster oDm = new OrderDeliveryMaster();
                            oDm.OrderId = o.OrderId;
                            oDm.CityId = o.CityId;
                            oDm.CompanyId = o.CompanyId;
                            oDm.WarehouseId = o.WarehouseId;
                            oDm.WarehouseName = o.WarehouseName;
                            oDm.DboyMobileNo = o.DboyMobileNo;
                            oDm.DboyName = o.DboyName;
                            oDm.CustomerId = o.CustomerId;
                            oDm.CustomerName = o.CustomerName;
                            oDm.Customerphonenum = o.Customerphonenum;
                            oDm.ShopName = o.ShopName;
                            oDm.Skcode = o.Skcode;
                            oDm.Status = "Issued"; //OrderDMaster.Status;
                            oDm.ShippingAddress = o.ShippingAddress;
                            oDm.BillingAddress = o.BillingAddress;
                            oDm.CanceledStatus = o.CanceledStatus;
                            oDm.invoice_no = o.invoice_no;
                            oDm.OnlineServiceTax = o.OnlineServiceTax;
                            oDm.TotalAmount = o.TotalAmount;
                            oDm.GrossAmount = o.GrossAmount;
                            oDm.TaxAmount = o.TaxAmount;
                            oDm.SGSTTaxAmmount = o.SGSTTaxAmmount;
                            oDm.CGSTTaxAmmount = o.CGSTTaxAmmount;
                            oDm.ReDispatchedStatus = o.ReDispatchedStatus;
                            oDm.Trupay = o.Trupay;
                            oDm.comments = o.comments;
                            oDm.deliveryCharge = o.deliveryCharge;
                            oDm.DeliveryIssuanceId = o.DeliveryIssuanceIdOrderDeliveryMaster ?? 0;
                            oDm.DiscountAmount = o.DiscountAmount;
                            oDm.CheckNo = o.CheckNo;
                            oDm.CheckAmount = o.CheckAmount;
                            oDm.ElectronicPaymentNo = o.ElectronicPaymentNo;
                            oDm.ElectronicAmount = o.ElectronicAmount;
                            oDm.EpayLaterAmount = 0;
                            oDm.CashAmount = o.CashAmount;
                            oDm.OrderedDate = o.OrderedDate;
                            oDm.WalletAmount = o.WalletAmount;
                            oDm.RewardPoint = o.RewardPoint;
                            oDm.Tin_No = o.Tin_No;
                            oDm.ReDispatchCount = o.ReDispatchCount;
                            oDm.UpdatedDate = DateTime.Now;
                            oDm.CreatedDate = DateTime.Now;
                            context.OrderDeliveryMasterDB.Add(oDm);
                            #endregion

                            #region Order Master History
                            OrderMasterHistories hh1 = new OrderMasterHistories();
                            if (o != null)
                            {
                                hh1.orderid = o.OrderId;
                                hh1.Status = o.Status;
                                hh1.Reasoncancel = null;
                                hh1.Warehousename = o.WarehouseName;
                                if (peopledata.DisplayName == null || peopledata.DisplayName == "")
                                {
                                    hh1.username = peopledata.PeopleFirstName;
                                    hh1.Description = " (Issued AssignmentId : " + o.DeliveryIssuanceIdOrderDeliveryMaster + ") By" + peopledata.PeopleFirstName;
                                }
                                else
                                {
                                    hh1.username = peopledata.DisplayName;
                                    hh1.Description = " (Issued AssignmentId : " + o.DeliveryIssuanceIdOrderDeliveryMaster + ") By" + peopledata.DisplayName;
                                }
                                hh1.DeliveryIssuanceId = o.DeliveryIssuanceIdOrderDeliveryMaster;
                                hh1.userid = userid;
                                hh1.CreatedDate = DateTime.Now;
                                context.OrderMasterHistoriesDB.Add(hh1);

                            }
                            #endregion
                        }
                        foreach (var o in orderMLists)
                        {
                            o.OldStatus = o.Status;
                            o.Status = "Issued";
                            o.UpdatedDate = DateTime.Now;
                        }



                        //Stock code
                        MultiStockHelper<OnIssuedStockEntryDc> MultiStockHelpers = new MultiStockHelper<OnIssuedStockEntryDc>();
                        List<OnIssuedStockEntryDc> OnIssuedStockEntryList = new List<OnIssuedStockEntryDc>();
                        foreach (var oder in orderDMLists.Where(x => !ROrderids.Contains(x.OrderId)).ToList())
                        {
                            foreach (var StockHit in oder.orderDetails.Where(x => x.qty > 0))
                            {
                                var orderm = orderMLists.FirstOrDefault(x => x.OrderId == StockHit.OrderId);
                                var RefStockCode = (orderm.OrderType == 8) ? "CL" : "C";
                                bool isFree = orderm.orderDetails.Any(c => c.OrderDetailsId == StockHit.OrderDetailsId && c.IsFreeItem && c.IsDispatchedFreeStock);
                                if (isFree) { RefStockCode = "F"; }
                                else if (orderm.OrderType == 6) //6 Damage stock
                                {
                                    RefStockCode = "D";
                                }
                                else if (orderm.OrderType == 9) //6 Non Sellable stock
                                {
                                    RefStockCode = "N";
                                }
                                //else if(orderm.OrderType == 10)//10 NOn Revenue ORders
                                //{
                                //    RefStockCode = "NR";
                                //}
                                bool IsDeliveryRedispatch = false;
                                if (orderm.OldStatus == "Delivery Redispatch")
                                {
                                    IsDeliveryRedispatch = true;
                                }

                                OnIssuedStockEntryList.Add(new OnIssuedStockEntryDc
                                {
                                    ItemMultiMRPId = StockHit.ItemMultiMRPId,
                                    OrderDispatchedDetailsId = StockHit.OrderDispatchedDetailsId,
                                    OrderId = StockHit.OrderId,
                                    Qty = StockHit.qty,
                                    UserId = peopledata.PeopleID,
                                    WarehouseId = StockHit.WarehouseId,
                                    IsDeliveryRedispatch = IsDeliveryRedispatch,
                                    RefStockCode = RefStockCode,
                                });
                            }
                        }
                        if (OnIssuedStockEntryList.Any() && !IsReturn)
                        {
                            bool res = MultiStockHelpers.MakeEntry(OnIssuedStockEntryList, "Stock_OnIssued", context, scope);
                            if (!res)
                            {
                                msg = new ResponceMsg()
                                {
                                    Status = false,
                                    Message = "Can't Dispatched, Something went wrong"
                                };
                                return msg;
                            }
                        }
                        foreach (var item in orderDMLists)
                        {
                            context.Entry(item).State = EntityState.Modified;
                        }
                        foreach (var item in orderMLists)
                        {
                            context.Entry(item).State = EntityState.Modified;
                        }
                        context.Entry(obj).State = EntityState.Modified;
                        context.Commit();
                    }
                    msg = new ResponceMsg()
                    {
                        Status = true,
                        Message = "Assignment Finalize successfully !!"
                    };
                    return msg;
                }
                else
                {
                    msg = new ResponceMsg()
                    {
                        Status = false,
                        Message = "OrderId Not Found!!"
                    };

                    return msg;
                }
            }
            else
            {
                msg = new ResponceMsg()
                {
                    Status = false,
                    Message = "Already trip running and new trip created after end trip!!"
                };
                return msg;
            }
        }

        public async Task<List<DeliveryResult>> GenerateTrip(int clusterId, DateTime startDate, DateTime endDate, bool isTestModel, int userid)
        {
            ResMsg isRejected = null;
            using (var context = new AuthContext())
            {

                var parameters = new List<SqlParameter> { new SqlParameter("@ClusterId", clusterId) };
                context.Database.ExecuteSqlCommand("EXEC TripPlanner_DeleteAllUnplannedTrip @ClusterId", parameters.ToArray());
                context.Commit();

                isRejected = TripPlannerRejectAssignment(null, context, clusterId, userid);
                context.Commit();
            }
            List<DeliveryResult> resultList = null;
            TripPlanner tripPlanner = new TripPlanner();
            System.Transactions.TransactionOptions option = new System.Transactions.TransactionOptions();
            option.IsolationLevel = System.Transactions.IsolationLevel.RepeatableRead;
            option.Timeout = TimeSpan.FromSeconds(90);

            var finalResult = await tripPlanner.GetTripUsingLoop(clusterId, startDate, isTestModel);
            if (finalResult != null && finalResult.TripList != null && finalResult.TripList.Any())
            {
                resultList = finalResult.TripList;

                List<TripPlannerMaster> tripPlannerMasterList = null;
                if (resultList != null && resultList.Any())
                {
                    tripPlannerMasterList = new List<TripPlannerMaster>();
                    int tripNumber = 1;
                    foreach (var result in resultList)
                    {
                        TripPlannerMaster tripPlannerMaster = await tripPlanner.GetTrip(result, startDate.Date, tripNumber, "");
                        if (tripPlannerMaster != null && tripPlannerMaster.TripPlannerDetailList.Count > 1)
                        {
                            tripPlannerMasterList.Add(tripPlannerMaster);
                            tripNumber++;
                        }
                    }
                }


                List<TripPlannerDroppedOrder> droppedOrderList = new List<TripPlannerDroppedOrder>();

                if (finalResult.DroppedCustomerList != null && finalResult.DroppedCustomerList.Any())
                {
                    foreach (var droppedCustomer in finalResult.DroppedCustomerList)
                    {
                        if (!string.IsNullOrEmpty(droppedCustomer.OrderIdList))
                        {
                            List<int> orderList = droppedCustomer.OrderIdList.Split(',').Select(x => int.Parse(x)).ToList();
                            foreach (var ord in orderList)
                            {
                                TripPlannerDroppedOrder tripPlannerDroppedOrder = new TripPlannerDroppedOrder
                                {
                                    CreatedBy = userid,
                                    CreatedDate = DateTime.Now,
                                    Id = 0,
                                    IsActive = true,
                                    IsDeleted = false,
                                    ModifiedBy = null,
                                    ModifiedDate = null,
                                    OrderId = ord,
                                    TripDate = startDate
                                };

                                droppedOrderList.Add(tripPlannerDroppedOrder);
                            }

                        }
                    }
                }
                using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, option))
                {
                    using (var authContext = new AuthContext())
                    {

                        if (isRejected != null && isRejected.Status)
                        {
                            if (tripPlannerMasterList != null && tripPlannerMasterList.Any())
                            {
                                authContext.TripPlannerMasters.AddRange(tripPlannerMasterList);
                            }
                            if (droppedOrderList.Any())
                            {
                                authContext.TripPlannerDroppedOrders.AddRange(droppedOrderList);
                            }
                            authContext.Commit();
                            scope.Complete();
                        }
                        else
                        {
                            scope.Dispose();
                        }
                    }
                }
            }
            return resultList;
        }
        public async Task<ResultViewModel<string>> GenerateAllTrip(int userid, bool isDeletePreviousData = true, int WarehouseId = 0, TripTypeEnum tripType = TripTypeEnum.City, int customerId = 0)
        {
            ResultViewModel<string> resultModel = null;
            try
            {
                if (UtilityRunningCurrentStatus(WarehouseId) == true)
                {
                    return new ResultViewModel<string>
                    {
                        ErrorMessage = "Utility already running",
                        IsSuceess = false,
                    };
                }
                IsRunningUtility(true, WarehouseId);
                bool isUseDummyNode = false;
                DateTime tripDate = DateTime.Today.AddDays(1);

                ORToolLogger globallogger = new ORToolLogger(0);
                globallogger.Log("Utility Starts");

                System.Transactions.TransactionOptions option = new System.Transactions.TransactionOptions();
                option.IsolationLevel = System.Transactions.IsolationLevel.RepeatableRead;
                option.Timeout = TimeSpan.FromSeconds(90);
                List<DeliveryResult> resultList = null;
                //List<int> clusterIdList = new List<int>();
                List<int> warehouseIdList = new List<int>();

                using (var authContext = new AuthContext())
                {
                    List<SqlParameter> paramList = new List<SqlParameter>();
                    paramList.Add(new SqlParameter("@WarehouseId", WarehouseId));
                    string spNameWithParam = "EXEC Operation.TripPlanner_WarehouseListGet @WarehouseId";
                    warehouseIdList = authContext.Database.SqlQuery<int>(spNameWithParam, paramList.ToArray()).ToList();
                }


                if (warehouseIdList != null && warehouseIdList.Any())
                {
                    string warehouseListString = String.Join(",", warehouseIdList.ToArray());
                    globallogger.Log("Following warehouses are being processed...", warehouseListString);

                    ParallelLoopResult parellelResult = Parallel.ForEach(warehouseIdList, (item) =>
                    {
                        ORToolLogger clusterlogger = new ORToolLogger(item);
                        try
                        {

                            clusterlogger.Log($"warehouse id: {item}, process starts");

                            ResMsg isRejected = null;
                            using (var context = new AuthContext())
                            {
                                if (isDeletePreviousData == true)
                                {


                                    clusterlogger.Log($"Rejecting assignment if there is any");

                                    isRejected = TripPlannerRejectAssignment(null, context, item, userid);
                                    context.Commit();

                                    clusterlogger.Log($"Rejecting assignment ends");

                                    clusterlogger.Log($"Removing all previous trips");

                                    var parameters = new List<SqlParameter> { new SqlParameter("@warehouseid", item) };
                                    context.Database.ExecuteSqlCommand("EXEC Operation.TripPlanner_DeleteAllUnplannedTrip @warehouseid", parameters.ToArray());
                                    context.Commit();

                                    clusterlogger.Log($"Removing all previous trips ends");


                                    clusterlogger.Log($"Cancelling all previous picker");

                                    var culsterParam = new List<SqlParameter> { new SqlParameter("@warehouseid", item) };
                                    context.Database.ExecuteSqlCommand("EXEC Operation.TipPlanner_AutoDeleteTripPickerCancel @warehouseid", parameters.ToArray());
                                    context.Commit();

                                    clusterlogger.Log($"Cancelling all previous picker picker");
                                }
                                else
                                {
                                    isRejected = new ResMsg
                                    {
                                        Message = "remove",
                                        Status = true
                                    };
                                    clusterlogger.Log($"Not delete previous data because flag isDeletePreviousData is false");
                                }

                            }
                            TripPlanner tripPlanner = new TripPlanner();
                            List<TripPlannerMaster> directTripPlannerMasterList = null;
                            FinalDeliveryResult finalResult = null;
                            if (tripType == TripTypeEnum.City)
                            {
                                finalResult = AsyncContext.Run(() => tripPlanner.GetTripUsingLoop(item, tripDate, isUseDummyNode));
                            }
                            else
                            {
                                directTripPlannerMasterList = AsyncContext.Run(() => tripPlanner.GetNonLastMileTripList(tripType, item, userid, customerId));
                            }
                            if (
                                    (finalResult != null && finalResult.TripList != null && finalResult.TripList.Any())
                                    || (directTripPlannerMasterList != null && directTripPlannerMasterList.Any())
                            )
                            {

                                List<TripPlannerMaster> tripPlannerMasterList = null;
                                List<TripPlannerDroppedOrder> droppedOrderList = new List<TripPlannerDroppedOrder>();


                                if (directTripPlannerMasterList != null && directTripPlannerMasterList.Any())
                                {
                                    tripPlannerMasterList = directTripPlannerMasterList;
                                }
                                else
                                {
                                    resultList = finalResult.TripList;
                                    clusterlogger.Log($"Total number of trips are: {resultList.Count()}");

                                    if (resultList != null && resultList.Any())
                                    {
                                        tripPlannerMasterList = new List<TripPlannerMaster>();
                                        int tripNumber = 1;
                                        foreach (var result in resultList)
                                        {
                                            TripPlannerMaster tripPlannerMaster = AsyncContext.Run(() => tripPlanner.GetTrip(result, tripDate, tripNumber, ""));
                                            if (tripPlannerMaster != null && tripPlannerMaster.TripPlannerDetailList.Count > 1)
                                            {
                                                tripPlannerMasterList.Add(tripPlannerMaster);
                                                tripNumber++;
                                            }
                                        }
                                    }



                                    if (finalResult.DroppedCustomerList != null && finalResult.DroppedCustomerList.Any())
                                    {
                                        foreach (var droppedCustomer in finalResult.DroppedCustomerList)
                                        {
                                            if (!string.IsNullOrEmpty(droppedCustomer.OrderIdList))
                                            {
                                                List<int> orderList = droppedCustomer.OrderIdList.Split(',').Select(x => int.Parse(x)).ToList();
                                                foreach (var ord in orderList)
                                                {
                                                    TripPlannerDroppedOrder tripPlannerDroppedOrder = new TripPlannerDroppedOrder
                                                    {
                                                        CreatedBy = userid,
                                                        CreatedDate = DateTime.Now,
                                                        Id = 0,
                                                        IsActive = true,
                                                        IsDeleted = false,
                                                        ModifiedBy = null,
                                                        ModifiedDate = null,
                                                        OrderId = ord,
                                                        TripDate = tripDate
                                                    };

                                                    droppedOrderList.Add(tripPlannerDroppedOrder);
                                                }

                                            }
                                        }
                                    }

                                    clusterlogger.Log($"Trips are saving into the db starts");
                                }


                                using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, option))
                                {
                                    using (var authContext = new AuthContext())
                                    {
                                        if (isRejected != null && isRejected.Status)
                                        {
                                            authContext.TripPlannerMasters.AddRange(tripPlannerMasterList);
                                            if (droppedOrderList.Any())
                                            {
                                                authContext.TripPlannerDroppedOrders.AddRange(droppedOrderList);
                                            }
                                            authContext.Commit();
                                            foreach (var trip in tripPlannerMasterList)
                                            {
                                                SaveTripPlannerConfirmedMaster(trip.Id, userid, authContext, tripType, customerId);
                                            }
                                            scope.Complete();
                                            clusterlogger.Log($"Trips are saving into the db ends ");
                                            clusterlogger.Log($"Trip ids are ", string.Join(",", tripPlannerMasterList.Select(x => x.Id).ToArray()));

                                        }
                                        else
                                        {
                                            scope.Dispose();
                                        }
                                    }
                                }

                            }
                            else
                            {
                                clusterlogger.Log($"No Trip list found");
                                resultModel = new ResultViewModel<string>
                                {
                                    ErrorMessage = "No order found to generate a trip",
                                    IsSuceess = false
                                };
                            }

                            clusterlogger.Log($"cluster id: {item}, process ends");

                        }
                        catch (Exception ex)
                        {
                            clusterlogger.Error($"while execution optimization code, Messsage:", ex.InnerException != null ? ex.ToString() + Environment.NewLine + ex.InnerException.ToString() : ex.ToString());

                        }
                    });

                    if (parellelResult.IsCompleted)
                    {

                        globallogger.Log("Utility Ends");

                        globallogger.GenerateFinalFile();

                    }

                }
                IsRunningUtility(false, WarehouseId);
                if (resultModel != null)
                {
                    return resultModel;
                }
                else
                {
                    return new ResultViewModel<string>
                    {
                        IsSuceess = true,
                        SuccessMessage = "Utility completed successfully."
                    };
                }

            }
            catch (Exception ex)
            {
                IsRunningUtility(false, WarehouseId);
                throw ex;
            }
        }
        public int UpdateTripPlannerConfirmedMaster(long tripPlannerConfirmedMasterId, AuthContext authContext)
        {
            var tripPlannerConfirmedMasterIdParam = new SqlParameter
            {
                ParameterName = "@TripPlannerConfirmedMasterId",
                Value = tripPlannerConfirmedMasterId
            };

            int result = authContext.Database.ExecuteSqlCommand("Operation.TripPlanner_UpdateTripPlannerConfirmedMaster @TripPlannerConfirmedMasterId", tripPlannerConfirmedMasterIdParam);

            return result;
        }
        public void UpdateTripSequence(long tripPlannerConfirmedMasterId, AuthContext authContext)
        {

            List<SqlParameter> paramList = new List<SqlParameter>();
            paramList.Add(new SqlParameter("@TripPlannerConfirmedMasterId", tripPlannerConfirmedMasterId));
            string spNameWithParam = "EXEC Operation.TripPlanner_WayPointGet @TripPlannerConfirmedMasterId";
            int skip = 0, take = 24;

            List<TripWayPoint> tripWayPointEntireList = authContext.Database.SqlQuery<TripWayPoint>(spNameWithParam, paramList.ToArray()).ToList();
            if (tripWayPointEntireList != null && tripWayPointEntireList.Count > 1)
            {
                TripWayPoint warehousePoint = tripWayPointEntireList.FirstOrDefault(x => x.CustomerId == 0);
                tripWayPointEntireList = tripWayPointEntireList.Where(x => x.CustomerId != 0).ToList();
                RouteOptimizeHelper routeOptimizeHelper = new RouteOptimizeHelper();
                if (tripWayPointEntireList.Count() > take)
                {
                    routeOptimizeHelper.UpdateArialDistance(warehousePoint, tripWayPointEntireList);
                    tripWayPointEntireList = tripWayPointEntireList.OrderBy(x => x.ArialDistance).ToList();
                }

                var tripWayPointList = tripWayPointEntireList.Skip(skip).Take(take).ToList();
                TripWayPoint sourcePoint = warehousePoint;
                TripWayPoint destinationPoint = null;
                int sequence = 1;
                while (tripWayPointList != null && tripWayPointList.Any())
                {

                    // case when ther is last trip points included in while loop and next time while loop 
                    // will not be executed and warehouse will not be included in this last loop
                    if (tripWayPointList.Count() == take && tripWayPointEntireList.Count() == (skip + take))
                    {
                        destinationPoint = warehousePoint;
                    }
                    else if (tripWayPointList.Count() < take)
                    {
                        destinationPoint = warehousePoint;
                    }
                    else
                    {
                        destinationPoint = tripWayPointList.Last();
                        tripWayPointList = tripWayPointList.Take(tripWayPointList.Count() - 1).ToList();
                    }

                    var result = routeOptimizeHelper.OptimizeRoute(sourcePoint, tripWayPointList, destinationPoint);

                    if (result != null && result.routes != null && result.routes.Any())
                    {
                        var route = result.routes[0];
                        List<TripWayPoint> tripWayPointListNew = new List<TripWayPoint>();
                        route.waypoint_order.ForEach(item =>
                        {
                            tripWayPointListNew.Add(tripWayPointList[item]);
                        });
                        tripWayPointList = tripWayPointListNew;

                        int index = 0;
                        foreach (var leg in route.legs)
                        {
                            long unloadTime = 0;
                            long tripPlannerConfirmDetailId = 0;
                            if (index < (route.legs.Count() - 1))
                            {
                                unloadTime = tripWayPointList[index].AllOrderUnloadTimeInMins;
                                tripPlannerConfirmDetailId = tripWayPointList[index].TripPlannerConfirmedDetailId;
                            }
                            else
                            {
                                tripPlannerConfirmDetailId = destinationPoint.TripPlannerConfirmedDetailId;
                            }
                            TripPlannerConfirmedDetail detail = authContext.TripPlannerConfirmedDetails.First(x => x.Id == tripPlannerConfirmDetailId);
                            detail.TotalDistanceInMeter = leg.distance.value;
                            detail.TotalTimeInMins = unloadTime + (long)(leg.duration.value / 60);
                            detail.SequenceNo = sequence++;
                            index++;
                        }
                    }

                    sourcePoint = tripWayPointList.Last();
                    skip += take;
                    tripWayPointList = tripWayPointEntireList.Skip(skip).Take(take).ToList();

                }
                authContext.Commit();
                UpdateTripPlannerConfirmedMaster(tripPlannerConfirmedMasterId, authContext);
                authContext.Commit();
            }
        }

        public void UpdateTripSequenceNew(long tripPlannerConfirmedMasterId, AuthContext authContext, bool isOptimizeRoute = true)
        {

            List<SqlParameter> paramList = new List<SqlParameter>();
            paramList.Add(new SqlParameter("@TripPlannerConfirmedMasterId", tripPlannerConfirmedMasterId));
            string spNameWithParam = "EXEC Operation.TripPlanner_WayPointGet @TripPlannerConfirmedMasterId";

            List<TripWayPoint> tripWayPointEntireList = authContext.Database.SqlQuery<TripWayPoint>(spNameWithParam, paramList.ToArray()).ToList();
            if (tripWayPointEntireList != null && tripWayPointEntireList.Count > 1)
            {
                tripWayPointEntireList.Insert(0, tripWayPointEntireList[tripWayPointEntireList.Count - 1]);
                RouteOptimizeHelper routeOptimizeHelper = new RouteOptimizeHelper();
                var result = routeOptimizeHelper.OptimizeRouteUsingAzure(tripWayPointEntireList, isOptimizeRoute);

                int index = 0;
                string latLng = "";
                if (result != null && result.optimizedWaypoints != null && result.optimizedWaypoints.Count > 0)
                {
                    for (index = 0; index < result.optimizedWaypoints.Count; index++)
                    {
                        int pointIndex = result.optimizedWaypoints[index].optimizedIndex;
                        var tripPoint = tripWayPointEntireList[pointIndex + 1];
                        long unloadTime = tripWayPointEntireList[pointIndex + 1].AllOrderUnloadTimeInMins;

                        var pointSummary = result.routes[0].legs[index].summary;
                        TripPlannerConfirmedDetail detail = authContext.TripPlannerConfirmedDetails.First(x => x.Id == tripPoint.TripPlannerConfirmedDetailId);
                        detail.TotalDistanceInMeter = pointSummary.lengthInMeters;
                        detail.TotalTimeInMins = unloadTime + (long)(pointSummary.travelTimeInSeconds / 60);
                        detail.SequenceNo = index + 1;
                        latLng += detail.Lat + "," + detail.Lng + "|";
                    }

                    var tripPointW = tripWayPointEntireList[0];
                    var pointSummaryW = result.routes[0].legs[index].summary;
                    TripPlannerConfirmedDetail detailW = authContext.TripPlannerConfirmedDetails.First(x => x.Id == tripPointW.TripPlannerConfirmedDetailId);
                    detailW.TotalDistanceInMeter = pointSummaryW.lengthInMeters;
                    detailW.TotalTimeInMins = (long)(pointSummaryW.travelTimeInSeconds / 60);
                    latLng += detailW.Lat + "," + detailW.Lng + "|";

                }
                authContext.Commit();
                UpdateTripPlannerConfirmedMaster(tripPlannerConfirmedMasterId, authContext);
                authContext.Commit();
            }

            //UpdateTripPlannerConfirmedMaster(tripPlannerConfirmedMasterId, authContext);
            //authContext.Commit();
        }

        public TripPlannerConfirmedMaster GetCurrentTripPlannerConfirmedMaster(long TripPlannerConfirmedMasterId, AuthContext authContext)
        {
            var query = from p in authContext.DboyMasters
                        join cm in authContext.TripPlannerConfirmedMasters on p.Id equals cm.DboyId
                        join tv in authContext.TripPlannerVehicleDb on cm.Id equals tv.TripPlannerConfirmedMasterId
                        where cm.Id == TripPlannerConfirmedMasterId && cm.IsActive == true && cm.IsDeleted == false && tv.CurrentStatus != 6
                        && cm.IsVisibleToDboy == true
                        select cm;
            var tripPlannerConfirmedMaster = query.OrderBy(x => x.TripDate).ThenBy(x => x.TripNumber).FirstOrDefault();
            return tripPlannerConfirmedMaster;
        }
        public dynamic GetTripPlannerConfirmedDetailId(long TripPlannerConfirmedMasterId, bool IsNotLastMileTrip, AuthContext authContext)
        {
            if (IsNotLastMileTrip)
            {
                var tripPlannerConfirmedDetailId = authContext.TripPlannerConfirmedDetails.Where(cm => cm.TripPlannerConfirmedMasterId == TripPlannerConfirmedMasterId
                && cm.IsActive == true && cm.IsDeleted == false && cm.IsProcess == false && (cm.OrderCount > 0 || cm.CustomerId == 0)).OrderByDescending(cm => cm.CustomerTripStatus).FirstOrDefault();
                return tripPlannerConfirmedDetailId;
            }
            else
            {
                var tripPlannerConfirmedDetailId = authContext.TripPlannerConfirmedDetails.Where(cm => cm.TripPlannerConfirmedMasterId == TripPlannerConfirmedMasterId
               && cm.IsActive == true && cm.IsDeleted == false && cm.IsProcess == false && (cm.OrderCount > 0 || cm.CustomerId == 0)).OrderBy(cm => cm.SequenceNo).FirstOrDefault();
                return tripPlannerConfirmedDetailId;
            }
        }

        #region  Reject assignment by Back End
        public ResMsg TripPlannerRejectAssignment(TransactionScope scope, AuthContext context, int WarehouseID, int userid)
        {
            ResMsg res = new ResMsg { Message = "", Status = true };
            var warehouseId = new SqlParameter("@warehouseId", WarehouseID);
            var resultList = context.Database.SqlQuery<AssignmentIDc>("operation.TripPlanner_GetTripListDailyRejectAssignment @warehouseId", warehouseId).ToList();
            if (resultList != null && resultList.Any())
            {
                foreach (var item in resultList)
                {
                    if (item.IsFreezed)
                    {
                        if (item.DeliveryIssuanceId > 0)
                        {
                            if (AssignmentInProcess.Any(x => x == item.DeliveryIssuanceId))
                            {
                                res = new ResMsg()
                                {
                                    Status = false,
                                    Message = "Assignment #: " + item.DeliveryIssuanceId + " is already in process..",
                                };
                                return res;
                            }
                            else
                            {
                                AssignmentInProcess.Add(item.DeliveryIssuanceId);
                            }

                            var DBoyorders = context.DeliveryIssuanceDb.Where(x => x.DeliveryIssuanceId == item.DeliveryIssuanceId && x.IsActive == true).SingleOrDefault();
                            if (DBoyorders != null)
                            {
                                var people = context.Peoples.Where(x => x.PeopleID == DBoyorders.PeopleID).SingleOrDefault();
                                DBoyorders.Acceptance = false;
                                DBoyorders.Status = "Rejected";
                                DBoyorders.RejectReason = "";
                                DBoyorders.IsActive = false;
                                DBoyorders.UpdatedDate = DateTime.Now;
                                context.Entry(DBoyorders).State = EntityState.Modified;

                                #region  DeliveryHistory
                                OrderDeliveryMasterHistories AssginDeli = new OrderDeliveryMasterHistories();
                                AssginDeli.DeliveryIssuanceId = DBoyorders.DeliveryIssuanceId;
                                //AssginDeli.OrderId = delivery.o
                                AssginDeli.Cityid = DBoyorders.Cityid;
                                AssginDeli.city = DBoyorders.city;
                                AssginDeli.DisplayName = DBoyorders.DisplayName;
                                AssginDeli.Status = DBoyorders.Status;
                                AssginDeli.WarehouseId = DBoyorders.WarehouseId;
                                AssginDeli.PeopleID = people.PeopleID;
                                AssginDeli.VehicleId = DBoyorders.VehicleId;
                                AssginDeli.VehicleNumber = DBoyorders.VehicleNumber;
                                AssginDeli.RejectReason = DBoyorders.RejectReason;
                                //AssginDeli.OrderdispatchIds = obj.OrderdispatchIds;
                                AssginDeli.OrderIds = DBoyorders.OrderIds;
                                AssginDeli.Acceptance = DBoyorders.Acceptance;
                                AssginDeli.IsActive = DBoyorders.IsActive;
                                AssginDeli.IdealTime = DBoyorders.IdealTime;
                                AssginDeli.TravelDistance = DBoyorders.TravelDistance;
                                AssginDeli.CreatedDate = DateTime.Now;
                                AssginDeli.UpdatedDate = DateTime.Now;
                                AssginDeli.userid = people.PeopleID;
                                if (people.DisplayName == null)
                                {
                                    AssginDeli.UpdatedBy = people.PeopleFirstName;
                                }
                                else
                                {
                                    AssginDeli.UpdatedBy = people.DisplayName;
                                }
                                context.OrderDeliveryMasterHistoriesDB.Add(AssginDeli);
                                #endregion

                                string[] ids = DBoyorders.OrderdispatchIds.Split(',');
                                foreach (var od in ids)
                                {
                                    var oid = Convert.ToInt32(od);
                                    var orderdipatchmaster = context.OrderDispatchedMasters.Where(x => x.OrderDispatchedMasterId == oid).Include(x => x.orderDetails).SingleOrDefault();
                                    var orderMaster = context.DbOrderMaster.Where(x => x.OrderId == orderdipatchmaster.OrderId).Include(x => x.orderDetails).SingleOrDefault();
                                    if (orderdipatchmaster.Status != "Shipped" && orderdipatchmaster.Status == "Issued")
                                    {

                                        if (orderdipatchmaster.ReDispatchCount > 0)
                                        {
                                            orderdipatchmaster.Status = "Delivery Redispatch";
                                            orderdipatchmaster.ReDispatchedStatus = "Delivery Redispatch";
                                            //orderdipatchmaster.ReDispatchCount += 1;
                                            orderdipatchmaster.UpdatedDate = DateTime.Now;
                                            context.Entry(orderdipatchmaster).State = EntityState.Modified;

                                            orderMaster.Status = "Delivery Redispatch";
                                            //orderMaster.ReDispatchCount += 1;
                                            orderMaster.UpdatedDate = DateTime.Now;
                                            foreach (var items in orderMaster.orderDetails)
                                            {
                                                items.UpdatedDate = DateTime.Now;
                                                items.status = "Ready to Dispatch";
                                            }
                                            context.Entry(orderMaster).State = EntityState.Modified;
                                        }
                                        else
                                        {
                                            orderdipatchmaster.Status = "Ready to Dispatch";
                                            orderdipatchmaster.ReDispatchedStatus = "Ready to Dispatch";
                                            orderdipatchmaster.UpdatedDate = DateTime.Now;
                                            context.Entry(orderdipatchmaster).State = EntityState.Modified;

                                            orderMaster.Status = "Ready to Dispatch";
                                            orderMaster.UpdatedDate = DateTime.Now;
                                            foreach (var a in orderMaster.orderDetails)
                                            {
                                                a.UpdatedDate = DateTime.Now;
                                                a.status = "Ready to Dispatch";
                                            }
                                            context.Entry(orderMaster).State = EntityState.Modified;
                                        }

                                        OrderMasterHistories h1 = new OrderMasterHistories();

                                        h1.orderid = orderdipatchmaster.OrderId;
                                        h1.Status = orderdipatchmaster.Status;
                                        h1.Description = "Due to Assignment Reject from System";
                                        h1.DeliveryIssuanceId = item.DeliveryIssuanceId;
                                        h1.Warehousename = orderdipatchmaster.WarehouseName;
                                        h1.username = DBoyorders.DisplayName;
                                        h1.userid = DBoyorders.PeopleID;
                                        h1.CreatedDate = DateTime.Now;
                                        context.OrderMasterHistoriesDB.Add(h1);

                                        if (orderMaster != null && orderMaster.OrderType != 5)
                                        {
                                            #region stock Hit
                                            //for Issued to Delivery Redispatched
                                            if (orderdipatchmaster.ReDispatchCount > 0)
                                            {
                                                MultiStockHelper<OnShippedRejectEntryDc> MultiStockHelpers = new MultiStockHelper<OnShippedRejectEntryDc>();
                                                List<OnShippedRejectEntryDc> OnShippedStockEntryList = new List<OnShippedRejectEntryDc>();
                                                foreach (var StockHit in orderdipatchmaster.orderDetails.Where(x => x.qty > 0))
                                                {
                                                    var RefStockCode = orderMaster.OrderType == 8 ? "CL" : "C";
                                                    bool isFree = orderMaster.orderDetails.Any(c => c.OrderDetailsId == StockHit.OrderDetailsId && c.IsFreeItem && c.IsDispatchedFreeStock);
                                                    if (isFree) { RefStockCode = "F"; }
                                                    else if (orderMaster.OrderType == 6) //6 Damage stock
                                                    {
                                                        RefStockCode = "D";
                                                    }
                                                    else if (orderMaster.OrderType == 9) //9 Non Sellable stock
                                                    {
                                                        RefStockCode = "N";
                                                    }
                                                    //else if (orderMaster.OrderType == 10) //9 Non Revenue
                                                    //{
                                                    //    RefStockCode = "NR";
                                                    //}

                                                    OnShippedStockEntryList.Add(new OnShippedRejectEntryDc
                                                    {
                                                        ItemMultiMRPId = StockHit.ItemMultiMRPId,
                                                        OrderDispatchedDetailsId = StockHit.OrderDispatchedDetailsId,
                                                        OrderId = StockHit.OrderId,
                                                        Qty = StockHit.qty,
                                                        UserId = people.PeopleID,
                                                        WarehouseId = StockHit.WarehouseId,
                                                        RefStockCode = RefStockCode
                                                    });
                                                }
                                                if (OnShippedStockEntryList.Any())
                                                {
                                                    bool ress = MultiStockHelpers.MakeEntry(OnShippedStockEntryList, "Stock_RedispatchOnAssignmentRejectFromIssued", context, scope);
                                                    if (!ress)
                                                    {
                                                        res = new ResMsg()
                                                        {
                                                            Status = false,
                                                            Message = "Can't Reject Issued, Something went wrong."
                                                        };
                                                        AssignmentInProcess.RemoveAll(x => x == item.DeliveryIssuanceId);

                                                        return res;
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                //for Issued to RTD
                                                MultiStockHelper<OnShippedRejectEntryDc> MultiStockHelpers = new MultiStockHelper<OnShippedRejectEntryDc>();
                                                List<OnShippedRejectEntryDc> OnShippedStockEntryList = new List<OnShippedRejectEntryDc>();

                                                foreach (var StockHit in orderdipatchmaster.orderDetails.Where(x => x.qty > 0))
                                                {
                                                    var RefStockCode = orderMaster.OrderType == 8 ? "CL" : "C";
                                                    bool isFree = orderMaster.orderDetails.Any(c => c.OrderDetailsId == StockHit.OrderDetailsId && c.IsFreeItem && c.IsDispatchedFreeStock);
                                                    if (isFree) { RefStockCode = "F"; }
                                                    else if (orderMaster.OrderType == 6) //6 Damage stock
                                                    {
                                                        RefStockCode = "D";
                                                    }
                                                    else if (orderMaster.OrderType == 9) //9 Non Sellable stock
                                                    {
                                                        RefStockCode = "N";
                                                    }
                                                    //else if (orderMaster.OrderType == 10) //9 Non Revenue stock
                                                    //{
                                                    //    RefStockCode = "NR";
                                                    //}
                                                    OnShippedStockEntryList.Add(new OnShippedRejectEntryDc
                                                    {
                                                        ItemMultiMRPId = StockHit.ItemMultiMRPId,
                                                        OrderDispatchedDetailsId = StockHit.OrderDispatchedDetailsId,
                                                        OrderId = StockHit.OrderId,
                                                        Qty = StockHit.qty,
                                                        UserId = people.PeopleID,
                                                        WarehouseId = StockHit.WarehouseId,
                                                        RefStockCode = RefStockCode
                                                    });
                                                }
                                                if (OnShippedStockEntryList.Any())
                                                {
                                                    bool ress = MultiStockHelpers.MakeEntry(OnShippedStockEntryList, "Stock_OnShippedReject", context, scope);
                                                    if (!ress)
                                                    {
                                                        res = new ResMsg()
                                                        {
                                                            Status = false,
                                                            Message = "Can't Reject Issued, Something went wrong."
                                                        };
                                                        AssignmentInProcess.RemoveAll(x => x == item.DeliveryIssuanceId);

                                                        return res;
                                                    }
                                                }
                                                var tripPlannerVehicle = context.TripPlannerVehicleDb.Where(x => x.TripPlannerConfirmedMasterId == DBoyorders.TripPlannerConfirmedMasterId).FirstOrDefault();
                                                if (tripPlannerVehicle != null)
                                                {
                                                    tripPlannerVehicle.CurrentStatus = (int)VehicleliveStatus.RejectTrip;
                                                    tripPlannerVehicle.IsDeleted = true;
                                                    tripPlannerVehicle.IsActive = false;
                                                    tripPlannerVehicle.ModifiedDate = DateTime.Now;
                                                    tripPlannerVehicle.ModifiedBy = userid;
                                                    context.Entry(tripPlannerVehicle).State = EntityState.Modified;
                                                }
                                                var tripPlannerConfirmedMasters = context.TripPlannerConfirmedMasters.Where(x => x.Id == tripPlannerVehicle.TripPlannerConfirmedMasterId).FirstOrDefault();
                                                if (tripPlannerConfirmedMasters != null)
                                                {
                                                    tripPlannerConfirmedMasters.IsActive = false;
                                                    tripPlannerConfirmedMasters.IsDeletedBySystem = true;
                                                    tripPlannerConfirmedMasters.IsDeleted = true;
                                                    tripPlannerConfirmedMasters.ModifiedDate = DateTime.Now;
                                                    tripPlannerConfirmedMasters.ModifiedBy = userid;
                                                    context.Entry(tripPlannerVehicle).State = EntityState.Modified;
                                                }
                                                var tripPlannerMasters = context.TripPlannerMasters.Where(x => x.Id == tripPlannerConfirmedMasters.TripPlannerMasterId).FirstOrDefault();
                                                if (tripPlannerMasters != null)
                                                {
                                                    tripPlannerMasters.IsActive = false;
                                                    tripPlannerMasters.IsDeleted = true;
                                                    tripPlannerMasters.ModifiedDate = DateTime.Now;
                                                    tripPlannerMasters.ModifiedBy = userid;
                                                    context.Entry(tripPlannerMasters).State = EntityState.Modified;
                                                }
                                            }
                                            #endregion
                                        }
                                        context.Commit();
                                    }
                                }
                            }
                            res = new ResMsg()
                            {
                                Status = true,
                                Message = "Success."
                            };
                            AssignmentInProcess.RemoveAll(x => x == item.DeliveryIssuanceId);
                            //return res;
                        }

                    }
                    else if (!item.IsFreezed && item.DeliveryIssuanceId == 0)
                    {

                        var tripPlannerConfirmedMasters = context.TripPlannerConfirmedMasters.Where(x => x.Id == item.TripPlannerConfirmedMasterId).FirstOrDefault();
                        if (tripPlannerConfirmedMasters != null)
                        {
                            tripPlannerConfirmedMasters.IsDeletedBySystem = true;
                            tripPlannerConfirmedMasters.IsActive = false;
                            tripPlannerConfirmedMasters.ModifiedDate = DateTime.Now;
                            tripPlannerConfirmedMasters.ModifiedBy = userid;
                            context.Entry(tripPlannerConfirmedMasters).State = EntityState.Modified;
                        }
                        var tripPlannerMasters = context.TripPlannerMasters.Where(x => x.Id == tripPlannerConfirmedMasters.TripPlannerMasterId).FirstOrDefault();
                        if (tripPlannerMasters != null)
                        {
                            tripPlannerMasters.IsActive = false;
                            tripPlannerMasters.IsDeleted = true;
                            tripPlannerMasters.ModifiedDate = DateTime.Now;
                            tripPlannerMasters.ModifiedBy = userid;
                            context.Entry(tripPlannerMasters).State = EntityState.Modified;
                        }
                        context.Commit();
                        //res = new ResMsg()
                        //{
                        //    Status = false,
                        //    Message = "Something went wrong..",
                        //};
                        //return res;
                    }
                    // case of asssignment which order in Redispatch
                    else
                    {
                        var deliveryIssuance = context.DeliveryIssuanceDb.Where(x => x.DeliveryIssuanceId == item.DeliveryIssuanceId).FirstOrDefault();
                        if (deliveryIssuance != null)
                        {
                            deliveryIssuance.Acceptance = false;
                            deliveryIssuance.Status = "Rejected";
                            deliveryIssuance.RejectReason = "System by Assignment Rejected";
                            deliveryIssuance.IsActive = false;
                            context.Entry(deliveryIssuance).State = EntityState.Modified;

                            var tripPlannerConfirmedMasters = context.TripPlannerConfirmedMasters.Where(x => x.Id == deliveryIssuance.TripPlannerConfirmedMasterId).FirstOrDefault();
                            if (tripPlannerConfirmedMasters != null)
                            {
                                tripPlannerConfirmedMasters.IsDeletedBySystem = true;
                                tripPlannerConfirmedMasters.IsActive = false;
                                tripPlannerConfirmedMasters.ModifiedDate = DateTime.Now;
                                tripPlannerConfirmedMasters.ModifiedBy = userid;
                                context.Entry(tripPlannerConfirmedMasters).State = EntityState.Modified;
                            }
                            var tripPlannerMasters = context.TripPlannerMasters.Where(x => x.Id == tripPlannerConfirmedMasters.TripPlannerMasterId).FirstOrDefault();
                            if (tripPlannerMasters != null)
                            {
                                tripPlannerMasters.IsActive = false;
                                tripPlannerMasters.IsDeleted = true;
                                tripPlannerMasters.ModifiedDate = DateTime.Now;
                                tripPlannerMasters.ModifiedBy = userid;
                                context.Entry(tripPlannerMasters).State = EntityState.Modified;
                            }
                            var orderids = deliveryIssuance.OrderIds.Split(',').Select(int.Parse).ToList();
                            var orderdipatchmaster = context.OrderDispatchedMasters.Where(x => orderids.Contains(x.OrderId)).ToList();
                            foreach (var Orderdipatchmasters in orderdipatchmaster)
                            {
                                OrderMasterHistories h1 = new OrderMasterHistories();
                                h1.orderid = Orderdipatchmasters.OrderId;
                                h1.Status = Orderdipatchmasters.Status;
                                h1.Description = "Due to Assignment Reject from System";
                                h1.DeliveryIssuanceId = item.DeliveryIssuanceId;
                                h1.Warehousename = Orderdipatchmasters.WarehouseName;
                                h1.username = "by System";
                                h1.CreatedDate = DateTime.Now;
                                context.OrderMasterHistoriesDB.Add(h1);
                            }
                            context.Commit();

                            res = new ResMsg
                            {
                                Message = "",
                                Status = true
                            };
                        }
                        else
                        {
                            res = new ResMsg()
                            {
                                Status = false,
                                Message = "No data found!!",
                            };
                        }
                    }
                }
            }
            else
            {
                res = new ResMsg()
                {
                    Status = true,
                    Message = "No data found!!",
                };
            }
            return res;
        }
        #endregion
        #region Reject assignment by delivery app 
        public ResMsg RejectAssignmentUpdateTrip(TransactionScope scope, AuthContext context, AssignmentAccRejDTO obj)
        {
            ResMsg res;
            if (obj != null && obj.DeliveryIssuanceId > 0)
            {
                if (AssignmentInProcess.Any(x => x == obj.DeliveryIssuanceId))
                {
                    res = new ResMsg()
                    {
                        Status = false,
                        Message = "Assignment #: " + obj.DeliveryIssuanceId + " is already in process..",
                    };
                    return res;
                }
                else
                {
                    AssignmentInProcess.Add(obj.DeliveryIssuanceId);
                }

                var DBoyorders = context.DeliveryIssuanceDb.Where(x => x.DeliveryIssuanceId == obj.DeliveryIssuanceId && x.IsActive == true).SingleOrDefault();
                if (DBoyorders != null)
                {
                    var people = context.Peoples.Where(x => x.PeopleID == DBoyorders.PeopleID).SingleOrDefault();
                    DBoyorders.Acceptance = false;
                    DBoyorders.Status = "Rejected";
                    DBoyorders.RejectReason = obj.RejectReason;
                    DBoyorders.IsActive = false;
                    DBoyorders.UpdatedDate = DateTime.Now;
                    context.Entry(DBoyorders).State = EntityState.Modified;

                    #region  DeliveryHistory
                    OrderDeliveryMasterHistories AssginDeli = new OrderDeliveryMasterHistories();
                    AssginDeli.DeliveryIssuanceId = DBoyorders.DeliveryIssuanceId;
                    //AssginDeli.OrderId = delivery.o
                    AssginDeli.Cityid = DBoyorders.Cityid;
                    AssginDeli.city = DBoyorders.city;
                    AssginDeli.DisplayName = DBoyorders.DisplayName;
                    AssginDeli.Status = DBoyorders.Status;
                    AssginDeli.WarehouseId = DBoyorders.WarehouseId;
                    AssginDeli.PeopleID = people.PeopleID;
                    AssginDeli.VehicleId = DBoyorders.VehicleId;
                    AssginDeli.VehicleNumber = DBoyorders.VehicleNumber;
                    AssginDeli.RejectReason = DBoyorders.RejectReason;
                    //AssginDeli.OrderdispatchIds = obj.OrderdispatchIds;
                    AssginDeli.OrderIds = DBoyorders.OrderIds;
                    AssginDeli.Acceptance = DBoyorders.Acceptance;
                    AssginDeli.IsActive = DBoyorders.IsActive;
                    AssginDeli.IdealTime = DBoyorders.IdealTime;
                    AssginDeli.TravelDistance = DBoyorders.TravelDistance;
                    AssginDeli.CreatedDate = DateTime.Now;
                    AssginDeli.UpdatedDate = DateTime.Now;
                    AssginDeli.userid = people.PeopleID;
                    if (people.DisplayName == null)
                    {
                        AssginDeli.UpdatedBy = people.PeopleFirstName;
                    }
                    else
                    {
                        AssginDeli.UpdatedBy = people.DisplayName;
                    }
                    context.OrderDeliveryMasterHistoriesDB.Add(AssginDeli);
                    #endregion

                    string[] ids = DBoyorders.OrderdispatchIds.Split(',');
                    foreach (var od in ids)
                    {
                        var oid = Convert.ToInt32(od);
                        var orderdipatchmaster = context.OrderDispatchedMasters.Where(x => x.OrderDispatchedMasterId == oid).Include(x => x.orderDetails).SingleOrDefault();
                        var orderMaster = context.DbOrderMaster.Where(x => x.OrderId == orderdipatchmaster.OrderId).Include(x => x.orderDetails).SingleOrDefault();
                        if (orderdipatchmaster.Status != "Shipped" && orderdipatchmaster.Status == "Issued")
                        {

                            if (orderdipatchmaster.ReDispatchCount > 0 && !orderdipatchmaster.IsReAttempt)
                            {
                                orderdipatchmaster.Status = "Delivery Redispatch";
                                orderdipatchmaster.ReDispatchedStatus = "Delivery Redispatch";
                                orderdipatchmaster.ReDispatchCount += 1;
                                orderdipatchmaster.UpdatedDate = DateTime.Now;
                                context.Entry(orderdipatchmaster).State = EntityState.Modified;

                                orderMaster.Status = "Delivery Redispatch";
                                orderMaster.ReDispatchCount += 1;
                                orderMaster.UpdatedDate = DateTime.Now;
                                foreach (var item in orderMaster.orderDetails)
                                {
                                    item.UpdatedDate = DateTime.Now;
                                    item.status = "Ready to Dispatch";
                                }
                                context.Entry(orderMaster).State = EntityState.Modified;
                            }
                            else if (orderdipatchmaster.IsReAttempt && orderdipatchmaster.ReAttemptCount > 0)
                            {
                                orderdipatchmaster.Status = "Delivery Redispatch";
                                orderdipatchmaster.ReDispatchedStatus = "Delivery Redispatch";
                                orderdipatchmaster.ReAttemptCount += 1;
                                orderdipatchmaster.UpdatedDate = DateTime.Now;
                                context.Entry(orderdipatchmaster).State = EntityState.Modified;

                                orderMaster.Status = "Delivery Redispatch";
                                orderMaster.UpdatedDate = DateTime.Now;
                                foreach (var item in orderMaster.orderDetails)
                                {
                                    item.UpdatedDate = DateTime.Now;
                                    item.status = "Ready to Dispatch";
                                }
                                context.Entry(orderMaster).State = EntityState.Modified;
                            }
                            else
                            {
                                orderdipatchmaster.Status = "Ready to Dispatch";
                                orderdipatchmaster.ReDispatchedStatus = "Ready to Dispatch";
                                orderdipatchmaster.UpdatedDate = DateTime.Now;
                                context.Entry(orderdipatchmaster).State = EntityState.Modified;

                                orderMaster.Status = "Ready to Dispatch";
                                orderMaster.UpdatedDate = DateTime.Now;
                                foreach (var item in orderMaster.orderDetails)
                                {
                                    item.UpdatedDate = DateTime.Now;
                                    item.status = "Ready to Dispatch";
                                }
                                context.Entry(orderMaster).State = EntityState.Modified;
                            }

                            OrderMasterHistories h1 = new OrderMasterHistories();

                            h1.orderid = orderdipatchmaster.OrderId;
                            h1.Status = orderdipatchmaster.Status;
                            h1.Description = "Due to Assignment Reject from Dapp";
                            h1.DeliveryIssuanceId = obj.DeliveryIssuanceId;
                            h1.Warehousename = orderdipatchmaster.WarehouseName;
                            h1.username = DBoyorders.DisplayName;
                            h1.userid = DBoyorders.PeopleID;
                            h1.CreatedDate = DateTime.Now;
                            context.OrderMasterHistoriesDB.Add(h1);

                            if (orderMaster != null && orderMaster.OrderType != 5)
                            {
                                #region stock Hit
                                //for Issued to Delivery Redispatched
                                if (orderdipatchmaster.ReDispatchCount > 0 && !orderdipatchmaster.IsReAttempt)
                                {
                                    MultiStockHelper<OnShippedRejectEntryDc> MultiStockHelpers = new MultiStockHelper<OnShippedRejectEntryDc>();
                                    List<OnShippedRejectEntryDc> OnShippedStockEntryList = new List<OnShippedRejectEntryDc>();
                                    foreach (var StockHit in orderdipatchmaster.orderDetails.Where(x => x.qty > 0))
                                    {
                                        var RefStockCode = orderMaster.OrderType == 8 ? "CL" : "C";
                                        bool isFree = orderMaster.orderDetails.Any(c => c.OrderDetailsId == StockHit.OrderDetailsId && c.IsFreeItem && c.IsDispatchedFreeStock);
                                        if (isFree) { RefStockCode = "F"; }
                                        else if (orderMaster.OrderType == 6) //6 Damage stock
                                        {
                                            RefStockCode = "D";
                                        }
                                        else if (orderMaster.OrderType == 9) //9 Non sellable stock
                                        {
                                            RefStockCode = "N";
                                        }
                                        //else if (orderMaster.OrderType == 10) //9 Non Revenue Stock
                                        //{
                                        //    RefStockCode = "NR";
                                        //}
                                        OnShippedStockEntryList.Add(new OnShippedRejectEntryDc
                                        {
                                            ItemMultiMRPId = StockHit.ItemMultiMRPId,
                                            OrderDispatchedDetailsId = StockHit.OrderDispatchedDetailsId,
                                            OrderId = StockHit.OrderId,
                                            Qty = StockHit.qty,
                                            UserId = people.PeopleID,
                                            WarehouseId = StockHit.WarehouseId,
                                            RefStockCode = RefStockCode
                                        });
                                    }
                                    if (OnShippedStockEntryList.Any())
                                    {
                                        bool ress = MultiStockHelpers.MakeEntry(OnShippedStockEntryList, "Stock_RedispatchOnAssignmentRejectFromIssued", context, scope);
                                        if (!ress)
                                        {
                                            res = new ResMsg()
                                            {
                                                Status = false,
                                                Message = "Can't Reject Issued, Something went wrong."
                                            };
                                            AssignmentInProcess.RemoveAll(x => x == obj.DeliveryIssuanceId);

                                            return res;
                                        }
                                    }
                                }
                                else if (orderdipatchmaster.IsReAttempt && orderdipatchmaster.ReAttemptCount > 0)
                                {
                                    MultiStockHelper<OnShippedRejectEntryDc> MultiStockHelpers = new MultiStockHelper<OnShippedRejectEntryDc>();
                                    List<OnShippedRejectEntryDc> OnShippedStockEntryList = new List<OnShippedRejectEntryDc>();
                                    foreach (var StockHit in orderdipatchmaster.orderDetails.Where(x => x.qty > 0))
                                    {
                                        var RefStockCode = orderMaster.OrderType == 8 ? "CL" : "C";
                                        bool isFree = orderMaster.orderDetails.Any(c => c.OrderDetailsId == StockHit.OrderDetailsId && c.IsFreeItem && c.IsDispatchedFreeStock);
                                        if (isFree) { RefStockCode = "F"; }
                                        else if (orderMaster.OrderType == 6) //6 Damage stock
                                        {
                                            RefStockCode = "D";
                                        }
                                        else if (orderMaster.OrderType == 9) //9 Non sellable stock
                                        {
                                            RefStockCode = "N";
                                        }
                                        //else if (orderMaster.OrderType == 10) //9 Non Revenue stock
                                        //{
                                        //    RefStockCode = "NR";
                                        //}
                                        OnShippedStockEntryList.Add(new OnShippedRejectEntryDc
                                        {
                                            ItemMultiMRPId = StockHit.ItemMultiMRPId,
                                            OrderDispatchedDetailsId = StockHit.OrderDispatchedDetailsId,
                                            OrderId = StockHit.OrderId,
                                            Qty = StockHit.qty,
                                            UserId = people.PeopleID,
                                            WarehouseId = StockHit.WarehouseId,
                                            RefStockCode = RefStockCode
                                        });
                                    }
                                    if (OnShippedStockEntryList.Any())
                                    {
                                        bool ress = MultiStockHelpers.MakeEntry(OnShippedStockEntryList, "Stock_RedispatchOnAssignmentRejectFromIssued", context, scope);
                                        if (!ress)
                                        {
                                            res = new ResMsg()
                                            {
                                                Status = false,
                                                Message = "Can't Reject Issued, Something went wrong."
                                            };
                                            AssignmentInProcess.RemoveAll(x => x == obj.DeliveryIssuanceId);

                                            return res;
                                        }
                                    }
                                }
                                else
                                {
                                    //for Issued to RTD
                                    MultiStockHelper<OnShippedRejectEntryDc> MultiStockHelpers = new MultiStockHelper<OnShippedRejectEntryDc>();
                                    List<OnShippedRejectEntryDc> OnShippedStockEntryList = new List<OnShippedRejectEntryDc>();

                                    foreach (var StockHit in orderdipatchmaster.orderDetails.Where(x => x.qty > 0))
                                    {
                                        var RefStockCode = orderMaster.OrderType == 8 ? "CL" : "C";
                                        bool isFree = orderMaster.orderDetails.Any(c => c.OrderDetailsId == StockHit.OrderDetailsId && c.IsFreeItem && c.IsDispatchedFreeStock);
                                        if (isFree) { RefStockCode = "F"; }
                                        else if (orderMaster.OrderType == 6) //6 Damage stock
                                        {
                                            RefStockCode = "D";
                                        }
                                        else if (orderMaster.OrderType == 9) //9 Non sellable stock
                                        {
                                            RefStockCode = "N";
                                        }
                                        //else if (orderMaster.OrderType == 10) //9 Non Revenue stock
                                        //{
                                        //    RefStockCode = "NR";
                                        //}
                                        OnShippedStockEntryList.Add(new OnShippedRejectEntryDc
                                        {
                                            ItemMultiMRPId = StockHit.ItemMultiMRPId,
                                            OrderDispatchedDetailsId = StockHit.OrderDispatchedDetailsId,
                                            OrderId = StockHit.OrderId,
                                            Qty = StockHit.qty,
                                            UserId = people.PeopleID,
                                            WarehouseId = StockHit.WarehouseId,
                                            RefStockCode = RefStockCode
                                        });
                                    }
                                    if (OnShippedStockEntryList.Any())
                                    {
                                        bool ress = MultiStockHelpers.MakeEntry(OnShippedStockEntryList, "Stock_OnShippedReject", context, scope);
                                        if (!ress)
                                        {
                                            res = new ResMsg()
                                            {
                                                Status = false,
                                                Message = "Can't Reject Issued, Something went wrong."
                                            };
                                            AssignmentInProcess.RemoveAll(x => x == obj.DeliveryIssuanceId);
                                            return res;
                                        }
                                    }
                                }
                                #endregion
                            }
                            context.Commit();
                        }
                    }
                    var deliveryIssuanceId = new SqlParameter("@DeliveryIssuanceId", obj.DeliveryIssuanceId);
                    int result = context.Database.ExecuteSqlCommand(" Operation.TripPlanner_RejectAssignmentUpdateTrip @DeliveryIssuanceId", deliveryIssuanceId);
                    context.Commit();
                    var tripPlannerConfirmedMasterId = context.DeliveryIssuanceDb.FirstOrDefault(x => x.DeliveryIssuanceId == obj.DeliveryIssuanceId).TripPlannerConfirmedMasterId;
                    //UpdateTripSequence(tripPlannerConfirmedMasterId, context);
                    UpdateTripSequenceNew(tripPlannerConfirmedMasterId, context);
                    var query = @"update tpv set tpv.ReminingTime=tpm.TotalTimeInMins,tpv.DistanceLeft=tpm.TotalDistanceInMeter  from TripPlannerVehicles tpv
                                  inner join TripPlannerConfirmedMasters tpm on tpv.TripPlannerConfirmedMasterId=tpm.Id
                                  inner join  DeliveryIssuances di on tpm.Id=di.TripPlannerConfirmedMasterId
                                  where di.DeliveryIssuanceId=" + obj.DeliveryIssuanceId + "";
                    int result1 = context.Database.ExecuteSqlCommand(query);
                    context.Commit();
                }
                res = new ResMsg()
                {
                    Status = true,
                    Message = "Success."
                };
                AssignmentInProcess.RemoveAll(x => x == obj.DeliveryIssuanceId);
                return res;
            }
            else
            {
                res = new ResMsg()
                {
                    Status = false,
                    Message = "Something went wrong..",
                };
                return res;
            }
        }
        #endregion

        #region Shipment Manifest
        public async Task<ShipmentManifestDc> ShipmentManifest(long TripPlannerMasterId)
        {
            using (var context = new AuthContext())
            {
                ShipmentManifestDc invoice = new ShipmentManifestDc();
                TripPlannerManager tripPlannerManager = new TripPlannerManager();
                invoice = await tripPlannerManager.ShipmentManifest(TripPlannerMasterId);
                return invoice;
            }
        }
        #endregion
        public bool SetTripDboyVisibiltyStatus(long TripPlannerConfirmedMasterId, AuthContext context)
        {
            bool status = false;
            var tripPlannerConfirmedMasters = context.TripPlannerConfirmedMasters.FirstOrDefault(x => x.Id == TripPlannerConfirmedMasterId);
            var tripPlannerConfirmedMasterid = new SqlParameter("@TripPlannerConfirmedMasterId", TripPlannerConfirmedMasterId);
            var result = context.Database.SqlQuery<int?>("Operation.TripPlanner_CheckTripDboyVisibiltyStatus @TripPlannerConfirmedMasterId", tripPlannerConfirmedMasterid).FirstOrDefault();
            if (result > 0)
            {
                tripPlannerConfirmedMasters.IsVisibleToDboy = false;
                tripPlannerConfirmedMasters.ModifiedDate = DateTime.Now;
                context.Entry(tripPlannerConfirmedMasters).State = EntityState.Modified;
                status = false;
            }
            else
            {
                tripPlannerConfirmedMasters.IsVisibleToDboy = true;
                tripPlannerConfirmedMasters.ModifiedDate = DateTime.Now;
                context.Entry(tripPlannerConfirmedMasters).State = EntityState.Modified;
                status = true;
            }
            //context.Commit();
            return status;
        }

        public bool RemovePickerOrderFromTrip(long tripPlannerConfirmedMasterId, long PirckerId, List<int> rejectedOrderList, AuthContext context, int modifiedBy = 0, bool IsTriprejected = false)
        {
            if (rejectedOrderList != null && rejectedOrderList.Any())
            {
                var query = from d in context.TripPlannerConfirmedDetails
                            join o in context.TripPlannerConfirmedOrders on d.Id equals o.TripPlannerConfirmedDetailId
                            where rejectedOrderList.Contains((int)o.OrderId) && d.TripPlannerConfirmedMasterId == tripPlannerConfirmedMasterId
                            select o;
                var orderList = query.ToList();
                foreach (var order in orderList)
                {
                    order.IsActive = false;
                    order.IsDeleted = true;
                    order.ModifiedBy = modifiedBy;
                    order.ModifiedDate = DateTime.Now;
                    context.Entry(order).State = EntityState.Modified;
                }
                if (IsTriprejected)
                {
                    var vehicle = context.TripPlannerVehicleDb.FirstOrDefault(x => x.TripPlannerConfirmedMasterId == tripPlannerConfirmedMasterId);
                    vehicle.CurrentStatus = (int)VehicleliveStatus.TripEnd;

                }
                var TripPlannerConfirmMasterIdParam = new SqlParameter
                {
                    ParameterName = "TripPlannerConfirmMasterId",
                    Value = tripPlannerConfirmedMasterId
                };
                context.Commit();
                var result = context.Database.ExecuteSqlCommand("Operation.UpdateTripMaster @TripPlannerConfirmMasterId ", TripPlannerConfirmMasterIdParam);
                context.Commit();
            }
            return true;
        }

        public bool ReAdjustTrip(int userId, long tripPlannerConfirmedMasterId, AuthContext authContext, out bool IsNotLastMileTrip)
        {
            TripPlannerConfirmedMaster tripPlannerConfirmedMaster = authContext.TripPlannerConfirmedMasters
                        .Include("TripPlannerConfirmedDetailList.TripPlannerConfirmedOrderList")
                        .Where(x => x.Id == tripPlannerConfirmedMasterId)
                        .FirstOrDefault();
            IsNotLastMileTrip = tripPlannerConfirmedMaster.IsNotLastMileTrip;
            foreach (var detail in tripPlannerConfirmedMaster.TripPlannerConfirmedDetailList)
            {
                if (detail.TripPlannerConfirmedOrderList.Where(x => x.IsActive == true && x.IsDeleted == false).Any())
                {
                    detail.OrderCount = detail.TripPlannerConfirmedOrderList.Where(x => x.IsActive == true && x.IsDeleted == false).Count();
                    detail.IsActive = true;
                    detail.IsDeleted = false;
                    detail.TotalAmount = detail.TripPlannerConfirmedOrderList.Where(x => x.IsActive == true && x.IsDeleted == false).Sum(y => y.Amount);
                    //authContext.TripPlannerDetails.Where(x.Id == detail.Tri)
                    //selectedDetail.TotalTimeInMins += selectedOrder.TimeInMins;
                    detail.TotalWeight = detail.TripPlannerConfirmedOrderList.Where(x => x.IsActive == true && x.IsDeleted == false).Sum(y => y.WeightInKg);
                }
                else if (detail.CustomerId != 0 && !detail.TripPlannerConfirmedOrderList.Where(x => x.IsActive == true && x.IsDeleted == false).Any())
                {
                    detail.OrderCount = 0;
                    detail.IsActive = false;
                    detail.IsDeleted = true;
                }
            }

            foreach (var detail in tripPlannerConfirmedMaster.TripPlannerConfirmedDetailList)
            {
                if (detail.TripPlannerConfirmedOrderList.Where(x => x.IsActive == true).Count() > 0)
                {
                    detail.CommaSeparatedOrderList
                        = String.Join(", ", detail.TripPlannerConfirmedOrderList.Where(x => x.IsActive == true).Select(x => x.OrderId));
                }
                else
                {
                    detail.CommaSeparatedOrderList = "";
                }
            }

            tripPlannerConfirmedMaster.CustomerCount
                = tripPlannerConfirmedMaster.TripPlannerConfirmedDetailList.Where(x => x.OrderCount > 0).Count();
            tripPlannerConfirmedMaster.OrderCount
                = tripPlannerConfirmedMaster.TripPlannerConfirmedDetailList.Where(x => x.OrderCount > 0).Sum(x => x.OrderCount);
            tripPlannerConfirmedMaster.TotalAmount
                = tripPlannerConfirmedMaster.TripPlannerConfirmedDetailList.Where(x => x.OrderCount > 0).Sum(x => x.TotalAmount);
            tripPlannerConfirmedMaster.TotalDistanceInMeter
                = tripPlannerConfirmedMaster.TripPlannerConfirmedDetailList.Where(x => x.CustomerId == 0 || x.OrderCount > 0).Sum(x => x.TotalDistanceInMeter);
            tripPlannerConfirmedMaster.TotalTimeInMins
                = tripPlannerConfirmedMaster.TripPlannerConfirmedDetailList.Where(x => x.CustomerId == 0 || x.OrderCount > 0).Sum(x => x.TotalTimeInMins);
            tripPlannerConfirmedMaster.TotalWeight
                = tripPlannerConfirmedMaster.TripPlannerConfirmedDetailList.Where(x => x.OrderCount > 0).Sum(x => x.TotalWeight);
            tripPlannerConfirmedMaster.ModifiedBy = userId;
            tripPlannerConfirmedMaster.ModifiedDate = DateTime.Now;

            authContext.Commit();
            return true;
        }

        public bool FinalizeTrip(int userId, long pickerId, int assignmentId, long tripPlannerConfirmedMasterId, AuthContext context)
        {
            bool IsNotLastMileTrip = false;
            bool result = ReAdjustTrip(userId, tripPlannerConfirmedMasterId, context, out IsNotLastMileTrip);

            TripPlannerHelper tripPlannerHelper = new TripPlannerHelper();
            //tripPlannerHelper.UpdateTripSequence(param.tripPlannerConfirmedMasterId, authContext);
            if (!IsNotLastMileTrip)
            {
                tripPlannerHelper.UpdateTripSequenceNew(tripPlannerConfirmedMasterId, context);
            }
            var mapping = context.TripPickerAssignmentMapping.FirstOrDefault(x => x.TripPlannerConfirmedMasterId == tripPlannerConfirmedMasterId && x.OrderPickerMasterId == pickerId && x.AssignmentId == null && x.IsDeleted == false);
            if (mapping != null)
            {
                mapping.AssignmentId = assignmentId;
                mapping.ModifiedBy = userId;
                mapping.ModifiedDate = DateTime.Now;

                context.Entry(mapping).State = EntityState.Modified;
            }
            else
            {
                TripPickerAssignmentMapping newMapping = new TripPickerAssignmentMapping
                {
                    TripPlannerConfirmedMasterId = tripPlannerConfirmedMasterId,
                    OrderPickerMasterId = pickerId,
                    AssignmentId = assignmentId,
                    CreatedBy = (int)userId,
                    ModifiedBy = null,
                    CreatedDate = DateTime.Now,
                    ModifiedDate = null,
                    IsActive = true,
                    IsDeleted = false
                };

                context.TripPickerAssignmentMapping.Add(newMapping);
            }
            var master = context.TripPlannerConfirmedMasters.FirstOrDefault(x => x.Id == tripPlannerConfirmedMasterId);
            master.IsVisibleToDboy = true;

            context.Commit();



            return result;
        }

        public List<TripBlockedOrderVM> GetBlockedOrderList(long tripPlannerConfirmedMasterId, AuthContext authContext)
        {
            var tripPlannerConfirmedMasterid = new SqlParameter("@TripPlannerConfirmedMasterId", tripPlannerConfirmedMasterId);
            var orderList = authContext.Database.SqlQuery<TripBlockedOrderVM>("Operation.TripPlanner_ChekcedBlockedOrderWithCondition @TripPlannerConfirmedMasterId", tripPlannerConfirmedMasterid).ToList();
            return orderList;
        }
        public GetReplaceVehicleListDc GetReplaceVehicle(long tripPlannerConfirmedMasterId, AuthContext authContext, string ReplacementVehicleNo)
        {
            var tripPlannerConfirmedMasterid = new SqlParameter("@TripPlannerConfirmedMasterId", tripPlannerConfirmedMasterId);
            var replacementVehicleNo = new SqlParameter
            {
                ParameterName = "ReplacementVehicleNo",
                Value = ReplacementVehicleNo == null ? DBNull.Value : (object)ReplacementVehicleNo
            };
            //var replacementVehicleNo = new SqlParameter("ReplacementVehicleNo", !string.IsNullOrEmpty(ReplacementVehicleNo) ? ReplacementVehicleNo : (object)DBNull.Value);
            var ReplaceVehicleList = authContext.Database.SqlQuery<GetReplaceVehicleListDc>("Operation.TripPlanner_IsReplaceVehicle @TripPlannerConfirmedMasterId,@ReplacementVehicleNo", tripPlannerConfirmedMasterid, replacementVehicleNo).FirstOrDefault();
            return ReplaceVehicleList;
        }
        public bool GetIsPickerGenerated(long tripPlannerConfirmedMasterId, AuthContext context)
        {
            var tripPlannerConfirmedMasteridParam = new SqlParameter("@TripPlannerConfirmedMasterId", tripPlannerConfirmedMasterId);
            bool result = context.Database.SqlQuery<bool>("Operation.TripPlanner_IsPickerGenerated @TripPlannerConfirmedMasterId", tripPlannerConfirmedMasteridParam).FirstOrDefault();
            return result;
        }
        public bool GetIsPickerRequired(long tripPlannerConfirmedMasterId, AuthContext context)
        {
            var tripPlannerConfirmedMasteridParam = new SqlParameter("@TripPlannerConfirmedMasterId", tripPlannerConfirmedMasterId);
            bool result = context.Database.SqlQuery<bool>("Operation.TripPlanner_IsPickerRequired @TripPlannerConfirmedMasterId", tripPlannerConfirmedMasteridParam).FirstOrDefault();
            return result;
        }


        public bool MakeEntryInVehicleHistory(double lat, double lng, long tripPlannerConfirmedMasterId, AuthContext context)
        {
            long vehicleId = context.TripPlannerVehicleDb.FirstOrDefault(x => x.TripPlannerConfirmedMasterId == tripPlannerConfirmedMasterId).Id;
            VehicleHistoryVM vehicleHistory = new VehicleHistoryVM
            {
                CurrentServingOrderId = 0,
                Lat = lat,
                Lng = lng,
                RecordStatus = 5,//start trip
                RecordTime = DateTime.Now,
                TripPlannerConfirmedDetailId = 0,
                TripPlannerVehicleId = vehicleId
            };

            HttpClient client = new HttpClient();

            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));

            string url = ConfigurationManager.AppSettings["SecondaryApiName"].ToString();

            var res = client.PostAsJsonAsync(url + "/api/VehicleHistory/InsertFirestoreData", vehicleHistory).Result;
            if (res.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return true;
            }
            else
            {
                return false;
            }



        }

        public bool IsPickerFinalzied(long tripPlannerConfirmedMasterId, AuthContext context)
        {
            var tripPlannerConfirmedMasteridParam = new SqlParameter("@TripPlannerConfirmedMasterId", tripPlannerConfirmedMasterId);
            int? notFinalizedCount = context.Database.SqlQuery<int?>("Operation.TripPlanner_IsPickerFinalzied @TripPlannerConfirmedMasterId", tripPlannerConfirmedMasteridParam).FirstOrDefault();
            if (notFinalizedCount > 0)
            {
                return false;
            }
            else
            {
                return true;
            }

        }

        public async Task<AttendanceTripDetailDc> InsertVechicleAttandance(InsertVechicleAttandanceDc insertVechicleAttandanceDc, AuthContext context)
        {
            AttendanceTripDetailDc res;
            var identity = User.Identity as ClaimsIdentity;
            int userid = 0;
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

            var WarehouseIdParam = new SqlParameter("@WarehouseId", insertVechicleAttandanceDc.WarehouseId);
            var VehicleMasterIdParam = new SqlParameter("@VehicleMasterId", insertVechicleAttandanceDc.VehicleMasterId);
            var IsTodayAttendanceParam = new SqlParameter("@IsTodayAttendance", insertVechicleAttandanceDc.IsTodayAttendance);
            var UserIdParam = new SqlParameter("@UserId", value: userid);
            var AttandanceDateParam = new SqlParameter("@AttandanceDate", insertVechicleAttandanceDc.AttandanceDate);
            var IsDateSendbyUserParam = new SqlParameter("@IsDateSendbyUser", insertVechicleAttandanceDc.IsDateSendbyUser);

            var data = context.Database.ExecuteSqlCommand("TripPlannerVechicleAttandanceInssert @WarehouseId,@VehicleMasterId,@IsTodayAttendance,@UserId,@AttandanceDate,@IsDateSendbyUser", WarehouseIdParam, VehicleMasterIdParam, IsTodayAttendanceParam, UserIdParam, AttandanceDateParam, IsDateSendbyUserParam);
            res = new AttendanceTripDetailDc()
            {
                Status = true,
                Message = "Updated Successfully!!",
                TripConfirmedMasterIds = null
            };
            return res;

        }
        public bool GetIsTripRunning(long VehicleMasterId, long tripPlannerConfirmedMasterId, AuthContext context)
        {
            var VehicleMasterIdParam = new SqlParameter("@VehicleMasterId", VehicleMasterId);
            var tripPlannerConfirmedMasteridParam = new SqlParameter("@TripPlannerConfirmedMasterId", tripPlannerConfirmedMasterId);
            var RunningTrip = context.Database.SqlQuery<bool>("Operation.IsTripRunning @VehicleMasterId,@TripPlannerConfirmedMasterId", VehicleMasterIdParam, tripPlannerConfirmedMasteridParam).FirstOrDefault();
            return RunningTrip;

        }

        public bool UpdateAmount(long tripPlannerConfirmedMasterId, AuthContext context)
        {
            var tripPlannerConfirmedMasteridParam = new SqlParameter("@TripPlannerConfirmedMasterId", tripPlannerConfirmedMasterId);
            int notFinalizedCount = context.Database.ExecuteSqlCommand("Operation.TripPlanner_UpdateAmount @TripPlannerConfirmedMasterId", tripPlannerConfirmedMasteridParam);
            context.Commit();
            return true;
        }
        public bool IsRunningUtility(bool IsRunningUtility, int WarehouseId)
        {
            MongoDbHelper<DeliveryOptimization> mongoDbHelper = new MongoDbHelper<DeliveryOptimization>();
            var deliveryOptimization = mongoDbHelper.Select(x => x.WarehouseId == WarehouseId).FirstOrDefault();
            if (deliveryOptimization != null)
            {
                deliveryOptimization.IsRunningUtility = IsRunningUtility;
                deliveryOptimization.WarehouseId = WarehouseId;
                mongoDbHelper.Replace(deliveryOptimization.Id, deliveryOptimization);
            }
            else
            {
                DeliveryOptimization Add = new DeliveryOptimization
                {
                    IsRunningUtility = IsRunningUtility,
                    WarehouseId = WarehouseId
                };
                mongoDbHelper.Insert(Add);
            }
            return true;
        }
        public bool UtilityRunningCurrentStatus(int WarehouseId)
        {
            MongoDbHelper<DeliveryOptimization> mongoDbHelper = new MongoDbHelper<DeliveryOptimization>();
            var deliveryOptimizationList = mongoDbHelper.Select(x => x.WarehouseId == WarehouseId || x.WarehouseId == 0).ToList();
            if (deliveryOptimizationList != null && deliveryOptimizationList.Any())
            {
                var global = deliveryOptimizationList.FirstOrDefault(x => x.WarehouseId == 0);
                var Warehouse = deliveryOptimizationList.FirstOrDefault(x => x.WarehouseId == WarehouseId);

                if (global != null && global.IsRunningUtility)
                {
                    return true;
                }
                else if (Warehouse != null && Warehouse.IsRunningUtility)
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
        public bool AllNotifyCustomerStartTrip(long tripPlannerConfirmedMasterId, AuthContext context)
        {
            var Customerids = context.TripPlannerConfirmedDetails.Where(x => x.TripPlannerConfirmedMasterId == tripPlannerConfirmedMasterId && x.IsActive == true && x.IsDeleted == false).Select(x => x.CustomerId).Distinct().ToList();
            var fcmids = context.Customers.Where(x => Customerids.Contains(x.CustomerId) && x.fcmId != null).Select(x => new { x.fcmId, x.CustomerId }).ToList();
            //send notification  => to Customer
            if (fcmids != null && fcmids.Any())
            {
                foreach (var item in fcmids)
                {
                    ConfigureNotifyHelper helper = new ConfigureNotifyHelper();
                    bool IsSendNotification = helper.TripNotifyAllCustomer(item.fcmId, item.CustomerId);
                }
            }
            else
            {
                return false;
            }
            return true;
        }
        public long SaveTripPlannerConfirmedMaster(long tripPlannerMasterId, long userId, AuthContext authContext, TripTypeEnum tripType, int customerId = 0)
        {
            long id = 0;

            if (authContext.TripPlannerConfirmedMasters.Where(x => x.TripPlannerMasterId == tripPlannerMasterId && x.IsDeleted == false).FirstOrDefault() == null)
            {
                TripPlannerMaster tripPlannerMaster = authContext.TripPlannerMasters
                    .Include("TripPlannerDetailList.TripPlannerOrderList")
                    .Where(x => x.Id == tripPlannerMasterId)
                    .FirstOrDefault();

                Warehouse warehouse = authContext.Warehouses.FirstOrDefault(x => x.WarehouseId == tripPlannerMaster.WarehouseId);

                TripPlannerConfirmedMaster tripPlannerConfirmedMaster = new TripPlannerConfirmedMaster
                {
                    CreatedDate = DateTime.Now,
                    AgentId = tripPlannerMaster.AgentId,
                    WarehouseId = tripPlannerMaster.WarehouseId,
                    CreatedBy = (int)userId,
                    CustomerCount = tripPlannerMaster.CustomerCount,
                    DboyId = tripPlannerMaster.DboyId == 0 ? null : tripPlannerMaster.DboyId,
                    Id = 0,
                    IsActive = true,
                    IsDeleted = false,
                    ModifiedBy = null,
                    ModifiedDate = null,
                    OrderCount = tripPlannerMaster.OrderCount,
                    TotalAmount = tripPlannerMaster.TotalAmount,
                    TotalDistanceInMeter = tripPlannerMaster.TotalDistanceInMeter,
                    TotalTimeInMins = tripPlannerMaster.TotalTimeInMins,
                    TotalWeight = tripPlannerMaster.TotalWeight,
                    TripDate = tripPlannerMaster.TripDate,
                    TripNumber = tripPlannerMaster.TripNumber,
                    TripPlannerConfirmedDetailList = new List<TripPlannerConfirmedDetail>(),
                    TripPlannerMasterId = tripPlannerMaster.Id,
                    VehicleMasterId = tripPlannerMaster.VehicleMasterId,
                    VehicleNumber = tripPlannerMaster.VehicleNumber,
                    WarehouseLat = tripPlannerMaster.WarehouseLat,
                    WarehouseLng = tripPlannerMaster.WarehouseLng,
                    DriverId = null,
                    IsManualTrip = false,
                    TripTypeEnum = (int)tripType,
                    IsNotLastMileTrip = (tripType == TripTypeEnum.City ? false : true),
                    CustomerId = customerId,
                    IsLocationEnabled = warehouse != null ? warehouse.IsLocationEnabled : false
                };

                if (tripPlannerMaster.TripPlannerDetailList != null && tripPlannerMaster.TripPlannerDetailList.Any())
                {
                    foreach (var tripDetail in tripPlannerMaster.TripPlannerDetailList)
                    {
                        TripPlannerConfirmedDetail confirmedDetail = new TripPlannerConfirmedDetail
                        {
                            CommaSeparatedOrderList = tripDetail.CommaSeparatedOrderList,
                            CreatedBy = (int)userId,
                            CreatedDate = DateTime.Now,
                            CustomerId = tripDetail.CustomerId,
                            Id = 0,
                            IsActive = tripDetail.IsActive,
                            IsDeleted = tripDetail.IsDeleted,
                            Lat = tripDetail.Lat,
                            Lng = tripDetail.Lng,
                            ModifiedBy = null,
                            ModifiedDate = null,
                            OrderCount = tripDetail.OrderCount,
                            SequenceNo = tripDetail.SequenceNo,
                            TotalAmount = tripDetail.TotalAmount,
                            TotalDistanceInMeter = tripDetail.TotalDistanceInMeter,
                            TotalTimeInMins = tripDetail.TotalTimeInMins,
                            TotalWeight = tripDetail.TotalWeight,
                            TripPlannerConfirmedMasterId = tripDetail.Id,
                            TripPlannerConfirmedOrderList = new List<TripPlannerConfirmedOrder>()
                        };
                        tripPlannerConfirmedMaster.TripPlannerConfirmedDetailList.Add(confirmedDetail);
                        if (tripDetail.OrderCount > 0)
                        {
                            foreach (var order in tripDetail.TripPlannerOrderList)
                            {
                                TripPlannerConfirmedOrder confirmedOrder = new TripPlannerConfirmedOrder
                                {
                                    Amount = order.Amount,
                                    CreatedBy = (int)userId,
                                    CreatedDate = DateTime.Now,
                                    DistanceInMeter = order.DistanceInMeter,
                                    Id = 0,
                                    IsActive = order.IsActive,
                                    IsDeleted = order.IsDeleted,
                                    ModifiedBy = null,
                                    ModifiedDate = null,
                                    OrderId = order.OrderId,
                                    TimeInMins = order.TimeInMins,
                                    TripPlannerConfirmedDetailId = tripDetail.Id,
                                    WeightInKg = order.WeightInKg
                                };
                                confirmedDetail.TripPlannerConfirmedOrderList.Add(confirmedOrder);
                            }
                        }
                    }
                }

                tripPlannerConfirmedMaster = authContext.TripPlannerConfirmedMasters.Add(tripPlannerConfirmedMaster);
                authContext.Commit();
                id = tripPlannerConfirmedMaster.Id;
            }


            return id;
        }
        public bool TripChangeDBoy(long TripPlannerConfirmedMasterId, long ChangeDboyId, AuthContext context, int userid, string username)
        {
            bool result = false;
            var changeDboyId = new SqlParameter("@ChangeDboyId", ChangeDboyId);
            var data = context.Database.SqlQuery<TripChangeBoyDC>("exec Operation.TripPlanner_TripChangeDboy @ChangeDboyId", changeDboyId).FirstOrDefault();
            if (data != null)
            {
                var tripPlannerConfirmedMasters = context.TripPlannerConfirmedMasters.Where(x => x.Id == TripPlannerConfirmedMasterId).FirstOrDefault();
                var deliveryIssuance = context.DeliveryIssuanceDb.Where(x => x.TripPlannerConfirmedMasterId == tripPlannerConfirmedMasters.Id).ToList();


                var deliveryIssuanceIds = deliveryIssuance.Select(x => x.DeliveryIssuanceId).ToList();

                var orderDispatchedMasters = context.OrderDispatchedMasters.Where(x => deliveryIssuanceIds.Contains(x.DeliveryIssuanceIdOrderDeliveryMaster ?? 0)).ToList();
                var orderDeliveryMaster = context.OrderDeliveryMasterDB.Where(x => deliveryIssuanceIds.Contains(x.DeliveryIssuanceId)).ToList();

                var Pickerids = context.TripPickerAssignmentMapping.Where(c => deliveryIssuanceIds.Contains(c.AssignmentId ?? 0) && c.IsDeleted == false).Select(c => c.OrderPickerMasterId).Distinct().ToList();
                var orderPickerMasters = context.OrderPickerMasterDb.Where(x => Pickerids.Contains(x.Id)).ToList();
                if (tripPlannerConfirmedMasters != null
                    && deliveryIssuance != null && deliveryIssuance.Any()
                    && orderDispatchedMasters != null && orderDispatchedMasters.Any()
                    )
                {
                    tripPlannerConfirmedMasters.DboyId = data.DboyMasterId;
                    context.Entry(tripPlannerConfirmedMasters).State = EntityState.Modified;

                    foreach (var item in deliveryIssuance)
                    {
                        item.PeopleID = data.PeopleID;
                        item.DisplayName = data.Dboyname;
                        context.Entry(item).State = EntityState.Modified;
                    }
                    foreach (var order in orderDispatchedMasters)
                    {
                        OrderMasterHistories h1 = new OrderMasterHistories();
                        h1.orderid = order.OrderId;
                        h1.Status = order.Status;
                        h1.Reasoncancel = "Due to Change in delivery Boy from " + order.DboyName + " " + order.DboyMobileNo + " To " + data.Dboyname + " " + data.DboyMobile + " " + "by Trip";
                        h1.Warehousename = order.WarehouseName;
                        h1.username = username;
                        h1.userid = userid;
                        h1.CreatedDate = DateTime.Now;
                        context.OrderMasterHistoriesDB.Add(h1);
                        order.DBoyId = data.PeopleID;
                        order.DboyName = data.Dboyname;
                        order.DboyMobileNo = data.DboyMobile;

                        context.Entry(order).State = EntityState.Modified;
                    }
                    orderDeliveryMaster.ForEach(x =>
                    {
                        x.DboyName = data.Dboyname;
                        x.DboyMobileNo = data.DboyMobile;
                        context.Entry(x).State = EntityState.Modified;
                    });
                    if (orderPickerMasters != null && orderPickerMasters.Any())
                    {
                        foreach (var picker in orderPickerMasters)
                        {
                            picker.DBoyId = data.PeopleID;
                            context.Entry(picker).State = EntityState.Modified;
                        }
                    }
                    context.Commit();
                    result = true;
                }
            }
            return result;
        }

        public double? OrderDispatchAmount(List<int> orderids, List<OrderDispatchedMaster> OrderDispatchedMasterList)
        {
            double? totalamount = OrderDispatchedMasterList.Where(z => orderids.Contains(z.OrderId)).Sum(d => d.GrossAmount);
            return totalamount;
        }


        public double? OrderAmount(List<int> orderids, AuthContext context)
        {
            double? totalamount = context.OrderDispatchedMasters.Where(z => orderids.Contains(z.OrderId)).Sum(d => d.GrossAmount);
            return totalamount;
        }
        public bool CheckTodayCustomerCashAmount(int CustomerId, double OrderCashAmount, string GUID, AuthContext context)
        {
            var startdate = DateTime.Now.Date.ToString("yyyy-MM-dd 00:00:00");
            var Enddate = DateTime.Now.Date.ToString("yyyy-MM-dd 23:59:59");
            bool Status = false;
            //var customerId = new SqlParameter("@CustomerId", CustomerId);
            //double CashAmount = context.Database.SqlQuery<double>("EXEC CheckTodayCustomerCashAmount @CustomerId", customerId).FirstOrDefault();
            //double totalCashAmount = OrderCashAmount + CashAmount;

            if (OrderCashAmount > 0)
            {
                MongoDbHelper<BlockCashAmount> mongoCashDbHelper = new MongoDbHelper<BlockCashAmount>();
                var cashPredicate = PredicateBuilder.New<BlockCashAmount>(x => x.CustomerId == CustomerId && x.Guid != GUID && x.CreatedDate >= Convert.ToDateTime(startdate) && x.CreatedDate <= Convert.ToDateTime(Enddate) && x.IsActive);
                var blockCashAmount = mongoCashDbHelper.Select(cashPredicate).ToList().Sum(x => x.Amount);

                if (((blockCashAmount + OrderCashAmount) >= 200000))
                {
                    Status = true;
                }
            }
            return Status;
        }
        public bool CheckVANSettled(string AlertSequenceNo, AuthContext context)
        {
            bool VanSettled = context.VANResponses.FirstOrDefault(x => x.UserReferenceNumber == AlertSequenceNo && x.IsActive == true && x.IsDeleted == false).IsSettled;
            return VanSettled;
        }

        public void SetTripWorkingStatus(TripPlannerAppDashboardDC tripPlannerAppDashboard, TripPlannerVehicles tripPlannerVehicles, bool IsSKFixVehicle)
        {
            try
            {
                if (IsSKFixVehicle)
                {
                    if (tripPlannerAppDashboard.assignmentList.Any(x => x.AssignmentStatus == "Assigned"))
                    {
                        tripPlannerAppDashboard.TripWorkingStatus = (int)TripWorkingStatusEnum.AcceptAssignmentPending;
                    }
                    else if (tripPlannerAppDashboard.assignmentList.Count(x => x.AssignmentStatus == "Rejected") == tripPlannerAppDashboard.assignmentList.Count)
                    {
                        tripPlannerAppDashboard.TripWorkingStatus = (int)TripWorkingStatusEnum.EndTrip;
                    }
                    else
                    {
                        if (!tripPlannerVehicles.IsRearrangeDone)
                        {
                            tripPlannerAppDashboard.TripWorkingStatus = (int)TripWorkingStatusEnum.Rearrange;
                        }
                        else if (!tripPlannerVehicles.IsStartKmRequestSend)
                        {
                            tripPlannerAppDashboard.TripWorkingStatus = (int)TripWorkingStatusEnum.Start;
                        }
                        else if (tripPlannerVehicles.IsStartKmRequestSend && !tripPlannerVehicles.IsStartKmApproved)
                        {
                            tripPlannerAppDashboard.TripWorkingStatus = (int)TripWorkingStatusEnum.StartKmApprovalPending;
                        }
                        else if (tripPlannerVehicles.IsStartKmApproved && !tripPlannerAppDashboard.IsTripEnd)
                        {
                            tripPlannerAppDashboard.TripWorkingStatus = (int)TripWorkingStatusEnum.InProgress;
                        }
                        else if (tripPlannerVehicles.IsStartKmApproved && tripPlannerAppDashboard.IsTripEnd && !tripPlannerVehicles.IsCloseKmRequestSend)
                        {
                            tripPlannerAppDashboard.TripWorkingStatus = (int)TripWorkingStatusEnum.SendCloseKmApproval;
                        }
                        else if (tripPlannerVehicles.IsStartKmApproved && tripPlannerAppDashboard.IsTripEnd && !tripPlannerVehicles.IsCloseKmApproved)
                        {
                            tripPlannerAppDashboard.TripWorkingStatus = (int)TripWorkingStatusEnum.CloseKmApprovalPending;
                        }
                        else
                        {
                            tripPlannerAppDashboard.TripWorkingStatus = (int)TripWorkingStatusEnum.EndTrip;
                        }
                    }
                }
                else
                {
                    if (!tripPlannerAppDashboard.IsNotLastMileTrip && !IsSKFixVehicle)
                    {
                        if (!tripPlannerAppDashboard.IsNotLastMileTrip && tripPlannerAppDashboard.assignmentList.Any(x => x.AssignmentStatus == "Assigned"))
                        {
                            tripPlannerAppDashboard.TripWorkingStatus = 0;
                        }
                        else if (!tripPlannerAppDashboard.IsNotLastMileTrip && tripPlannerAppDashboard.assignmentList.Any(x => x.AssignmentStatus == "Accepted"))
                        {
                            tripPlannerAppDashboard.TripWorkingStatus = (int)TripWorkingStatusEnum.IsNonLastMileStartTrip;
                        }
                        else if (!tripPlannerAppDashboard.IsNotLastMileTrip && !tripPlannerAppDashboard.IsTripEnd)
                        {
                            tripPlannerAppDashboard.TripWorkingStatus = (int)TripWorkingStatusEnum.InProgress;
                        }
                        else if (!tripPlannerAppDashboard.IsNotLastMileTrip && tripPlannerAppDashboard.assignmentList.Count(x => x.AssignmentStatus == "Rejected") == tripPlannerAppDashboard.assignmentList.Count)
                        {
                            tripPlannerAppDashboard.TripWorkingStatus = (int)TripWorkingStatusEnum.EndTrip;
                        }
                        else if (!tripPlannerAppDashboard.IsNotLastMileTrip && tripPlannerAppDashboard.IsTripEnd)
                        {
                            tripPlannerAppDashboard.TripWorkingStatus = (int)TripWorkingStatusEnum.EndTrip;
                        }
                    }
                    else if (tripPlannerAppDashboard.IsNotLastMileTrip && !IsSKFixVehicle)
                    {
                        if (tripPlannerAppDashboard.IsNotLastMileTrip && tripPlannerAppDashboard.assignmentList.Any(x => x.AssignmentStatus == "Assigned"))
                        {
                            tripPlannerAppDashboard.TripWorkingStatus = 0;
                        }
                        else if (tripPlannerAppDashboard.IsNotLastMileTrip && tripPlannerAppDashboard.assignmentList.Any(x => x.AssignmentStatus == "Accepted"))
                        {
                            tripPlannerAppDashboard.TripWorkingStatus = (int)TripWorkingStatusEnum.IsNonLastMileStartTrip;
                        }
                        else if (tripPlannerAppDashboard.IsNotLastMileTrip && !tripPlannerAppDashboard.IsTripEnd)
                        {
                            tripPlannerAppDashboard.TripWorkingStatus = (int)TripWorkingStatusEnum.InProgress;
                        }
                        else if (tripPlannerAppDashboard.IsNotLastMileTrip && tripPlannerAppDashboard.assignmentList.Count(x => x.AssignmentStatus == "Rejected") == tripPlannerAppDashboard.assignmentList.Count)
                        {
                            tripPlannerAppDashboard.TripWorkingStatus = (int)TripWorkingStatusEnum.EndTrip;
                        }
                        else if (tripPlannerAppDashboard.IsNotLastMileTrip && tripPlannerAppDashboard.IsTripEnd)
                        {
                            tripPlannerAppDashboard.TripWorkingStatus = (int)TripWorkingStatusEnum.EndTrip;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                tripPlannerAppDashboard.TripWorkingStatus = (int)TripWorkingStatusEnum.IssueInTrip;
            }
        }
        public bool GetIsNewPickerAllowed(long tripPlannerConfirmedMasterId, AuthContext context)
        {
            var tripPlannerConfirmedMasteridParam = new SqlParameter("@TripPlannerConfirmedMasterId", tripPlannerConfirmedMasterId);
            bool result = context.Database.SqlQuery<bool>("Operation.TripPlanner_IsAddNewPickerAllowed @TripPlannerConfirmedMasterId", tripPlannerConfirmedMasteridParam).FirstOrDefault();
            return result;
        }


        public bool IsAddNewPickerNotGenerated(long tripPlannerConfirmedMasterId, AuthContext context)
        {
            var tripPlannerConfirmedMasteridParam = new SqlParameter("@TripPlannerConfirmedMasterId", tripPlannerConfirmedMasterId);
            bool result = context.Database.SqlQuery<bool>("Operation.TripPlanner_IsAddNewPickerNotGenerated @TripPlannerConfirmedMasterId", tripPlannerConfirmedMasteridParam).FirstOrDefault();
            return result;
        }

        public bool GetIsAddNewPickerFinalized(long tripPlannerConfirmedMasterId, AuthContext context)
        {
            var tripPlannerConfirmedMasteridParam = new SqlParameter("@TripPlannerConfirmedMasterId", tripPlannerConfirmedMasterId);
            bool result = context.Database.SqlQuery<bool>("Operation.TripPlanner_IsAddNewPickerFinalized @TripPlannerConfirmedMasterId", tripPlannerConfirmedMasteridParam).FirstOrDefault();
            return result;
        }

        public bool CanAddOrderInNewPicker(long tripPlannerConfirmedMasterId, AuthContext context)
        {
            var tripPlannerConfirmedMasteridParam = new SqlParameter("@TripPlannerConfirmedMasterId", tripPlannerConfirmedMasterId);
            bool result = context.Database.SqlQuery<bool>("Operation.TripPlanner_CanAddOrderInNewPicker @TripPlannerConfirmedMasterId", tripPlannerConfirmedMasteridParam).FirstOrDefault();
            return result;
        }

        public List<int> GetdeliveryIssuanceId(long tripPlannerConfirmedMasterId, AuthContext authContext)
        {
            var tripPlannerConfirmedMasterid = new SqlParameter("@TripPlannerConfirmedMasterId", tripPlannerConfirmedMasterId);
            var orderList = authContext.Database.SqlQuery<int>("Operation.GetDeliveryIssuanceId @TripPlannerConfirmedMasterId", tripPlannerConfirmedMasterid).ToList();
            return orderList;
        }

        //Roc api
        public async Task<List<ItemWarehouseData>> RocTagValueGet(List<ItemWarehouseDc> itemWarehouseDcs)
        {
            using (var context = new AuthContext())
            {
                if (itemWarehouseDcs != null && itemWarehouseDcs.Any())
                {
                    var IdDt = new DataTable();
                    SqlParameter param = null;

                    IdDt = new DataTable();
                    IdDt.Columns.Add("ItemMultiMRPId");
                    IdDt.Columns.Add("WarehouseId");
                    foreach (var item in itemWarehouseDcs)
                    {
                        var dr = IdDt.NewRow();
                        dr["ItemMultiMRPId"] = item.ItemMultiMRPId;
                        dr["WarehouseId"] = item.WarehouseId;
                        IdDt.Rows.Add(dr);
                    }
                    param = new SqlParameter("ItemMultiMRPIdAndWhId", IdDt);
                    param.SqlDbType = SqlDbType.Structured;
                    param.TypeName = "dbo.ItemMultiMRPIdAndWhId";

                    if (context.Database.Connection.State != ConnectionState.Open)
                        context.Database.Connection.Open();
                    var cmd = context.Database.Connection.CreateCommand();
                    cmd.CommandTimeout = 900;
                    cmd.CommandText = "[dbo].[RocTagValue]";

                    cmd.Parameters.Add(param);
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;

                    var reader = cmd.ExecuteReader();
                    var data = ((IObjectContextAdapter)context)
                    .ObjectContext.Translate<ItemWarehouseData>(reader).ToList();
                    return data;
                }
                else
                {
                    return null;
                }

            }

        }

        // Roc Export Api
        public async Task<List<RocReportDataDc>> ExportRocReportData(DateTime ForMonthData)
        {
            using (var context = new AuthContext())
            {
                if (context.Database.Connection.State != ConnectionState.Open)
                    context.Database.Connection.Open();

                var cmd = context.Database.Connection.CreateCommand();
                cmd.CommandTimeout = 900;
                cmd.CommandText = "[dbo].[ROCReportData]";
                cmd.CommandType = System.Data.CommandType.StoredProcedure; ;
                cmd.Parameters.Add(new SqlParameter("@ForMonthData", ForMonthData));

                var reader = cmd.ExecuteReader();
                var data = ((IObjectContextAdapter)context)
                .ObjectContext.Translate<RocReportDataDc>(reader).ToList();
                return data;
            }

        }
    }
}

public class ResponceMsg
{
    public string Message { get; set; }
    public bool Status { get; set; }
    public List<TripBlockedOrderVM> blockedOrderList { get; set; }
}
public class TripChangeBoyDC
{
    public long DboyMasterId { get; set; }
    public int PeopleID { get; set; }
    public string Dboyname { get; set; }
    public string DboyMobile { get; set; }
}