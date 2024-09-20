using AgileObjects.AgileMapper;
using AngularJSAuthentication.API.Helper;
using AngularJSAuthentication.API.Helpers;
using AngularJSAuthentication.API.Managers;
using AngularJSAuthentication.Common.Constants;
using AngularJSAuthentication.Common.Helpers;
using AngularJSAuthentication.DataContracts.ClearanceStockNonSaleable;
using AngularJSAuthentication.DataContracts.constants;
using AngularJSAuthentication.DataContracts.DeliveryCapacityOptimization;
using AngularJSAuthentication.DataContracts.Masters;
using AngularJSAuthentication.DataContracts.Mongo;
using AngularJSAuthentication.DataContracts.Transaction.Stocks;
using AngularJSAuthentication.Model;
using AngularJSAuthentication.Model.DeliveryCapacityOptimization;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Transactions;
using System.Web;
using System.Web.Http;


namespace AngularJSAuthentication.API.Controllers
{
    [RoutePrefix("api/ClearanceNonSaleable")]
    public class ClearanceNonSaleableController : ApiController
    {


        #region  Stock Movement Page (Add New Order)


        [Route("AddClearanceNonSellableOrder")]
        [HttpPost]
        public async Task<ResponseMsg> AddClearanceNonSellableOrder(List<ClearanceNonSellableDc> PostItems)
        {

            var identity = User.Identity as ClaimsIdentity;
            int userid = 0;
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
            ResponseMsg result = new ResponseMsg();
            List<string> roleNames = new List<string>();
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "RoleNames") && identity.Claims.FirstOrDefault(x => x.Type == "RoleNames").Value != "")
                roleNames = (identity.Claims.FirstOrDefault(x => x.Type == "RoleNames").Value).Split(',').ToList();


            if (roleNames.Any(x => x == "Inbound Lead" || x == "HQ Master login") && PostItems != null && PostItems.Any() && userid > 0 && PostItems.FirstOrDefault().OrderType != null && PostItems.FirstOrDefault().Warehouseid > 0 && PostItems.FirstOrDefault().BuyerId > 0 && PostItems.Sum(r => r.Quantity) > 0)
            {

                var OrderType = PostItems.FirstOrDefault().OrderType;
                var Warehouseid = PostItems.FirstOrDefault().Warehouseid;
                var Buyerid = PostItems.FirstOrDefault().BuyerId;
                TransactionOptions option = new TransactionOptions();
                option.IsolationLevel = System.Transactions.IsolationLevel.RepeatableRead;
                option.Timeout = TimeSpan.FromSeconds(120);
                using (var dbContextTransaction = new TransactionScope(TransactionScopeOption.RequiresNew, option))
                {
                    using (AuthContext context = new AuthContext())
                    {
                        //if (!context.Warehouses.FirstOrDefault(x => x.WarehouseId == Warehouseid).IsStopCurrentStockTrans)
                        //{
                        //    result.Status = false;
                        //    result.Message = "Inventory Transactions are currently enabled for this warehouse... Please stop it first and try again";
                        //    return result;
                        //}
                        ClearanceNonSaleableMovementOrder AddOrder = new ClearanceNonSaleableMovementOrder();
                        AddOrder.ClearanceNonSaleableMovementOrderDetails = new List<ClearanceNonSaleableMovementOrderDetail>();

                        List<int> PeopleIds = new List<int>();
                        PeopleIds.Add(userid);
                        PeopleIds.Add(Buyerid);

                        var peoples = context.Peoples.Where(x => PeopleIds.Contains(x.PeopleID)).ToList();
                        var WarehouseName = context.Warehouses.FirstOrDefault(x => x.WarehouseId == Warehouseid && x.active == true);
                        var InboundLead = peoples.FirstOrDefault(x => x.PeopleID == userid);
                        if (InboundLead != null && InboundLead.Active)
                        {
                            string ToBuyerEmail = peoples.FirstOrDefault(x => x.PeopleID == Buyerid).Email;

                            #region for cl N in ClearanceNonSaleablePrepareItems if Id >0
                            var clpreitemids = PostItems.Where(x => x.Id > 0).Select(x => x.Id).Distinct().ToList();
                            if (clpreitemids != null && clpreitemids.Any() && (OrderType == "CurrentToClearance"))
                            {
                                var clpreitems = context.ClearanceNonSaleablePrepareItems.Where(x => clpreitemids.Contains(x.Id)).ToList();
                                foreach (var item in PostItems.Where(x => x.Quantity > 0).Distinct().ToList())
                                {
                                    if (item.UnitPrice == 0)
                                    {
                                        result.Status = false;
                                        result.Message = "Can't Add order, unit price is zero for this ItemMultiMRPId : " + item.ItemMultiMRPId;
                                        return result;
                                    }
                                    foreach (var citem in clpreitems.Where(c => c.Id == item.Id))
                                    {
                                        if (citem.RemainingQuantity < item.Quantity)
                                        {
                                            result.Status = false;
                                            result.Message = "Fullfil qty not available for ItemMultiMRPId : " + citem.ItemMultiMRPId;
                                            return result;
                                        }
                                        citem.RemainingQuantity -= item.Quantity;     //Qty not minus in Pending case
                                        citem.ModifiedDate = DateTime.Now;
                                        citem.ModifiedBy = userid;
                                        context.Entry(citem).State = EntityState.Modified;
                                    }
                                }
                            }
                            #endregion

                            AddOrder.Amount = PostItems.Sum(x => x.Quantity * x.UnitPrice);
                            AddOrder.Status = "Pending";
                            AddOrder.CreatedBy = userid;
                            AddOrder.CreatedDate = DateTime.Now;
                            AddOrder.IsActive = true;
                            AddOrder.IsDeleted = false;
                            AddOrder.WarehouseId = Warehouseid;
                            AddOrder.OrderType = OrderType;
                            PostItems.Where(x => x.Quantity > 0).Distinct().ToList().ForEach(x =>
                            AddOrder.ClearanceNonSaleableMovementOrderDetails.Add(
                                       new ClearanceNonSaleableMovementOrderDetail
                                       {
                                           ItemMultiMRPId = x.ItemMultiMRPId,
                                           Quantity = x.Quantity,
                                           OrderQuantity = x.Quantity,
                                           StockBatchMasterId = x.StockBatchMasterId,
                                           UnitPrice = x.UnitPrice,
                                       }));
                            context.ClearanceNonSaleableMovementOrders.Add(AddOrder);
                            if (context.Commit() > 0)
                            {
                                result.Status = true;
                                result.Message = " Order generated successfully #no.: " + AddOrder.Id;
                                dbContextTransaction.Complete();
                                if (ConfigurationManager.AppSettings["Environment"] == "Production")
                                {
                                    var sub = "Clearance Order No.: " + AddOrder.Id + " generated at " + WarehouseName.WarehouseName;
                                    var msg = "Cleaarance Order No.: " + AddOrder.Id + " generated successfully by " + InboundLead.PeopleFirstName + " " + InboundLead.PeopleLastName + " mobile number :" + InboundLead.Mobile;
                                    EmailHelper.SendMail(InboundLead.Email, ToBuyerEmail, "", sub, msg, "");
                                }

                            }
                            else
                            {
                                result.Status = false;
                                result.Message = "something went wrong";
                            }
                        }
                        else
                        {
                            result.Status = false;
                            result.Message = " You are not authorized.";
                        }
                    }
                }
            }
            else
            {
                result.Status = false;
                result.Message = " You are not authorized.";
            }
            return result;
        }


        [Route("GetClearanceStockMovementOrderList")]
        [HttpPost]
        public ClearanceStockMovementOrderList GetClearanceStockMovementOrderList(GetClearanceStockMovementOrderDC Data)
        {
            var identity = User.Identity as ClaimsIdentity;
            int userid = 0;
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

            List<string> roleNames = new List<string>();
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "RoleNames") && identity.Claims.FirstOrDefault(x => x.Type == "RoleNames").Value != "")
                roleNames = (identity.Claims.FirstOrDefault(x => x.Type == "RoleNames").Value).Split(',').ToList();

            ClearanceStockMovementOrderList res = new ClearanceStockMovementOrderList();
            List<GetClearanceStockMovementOrder> result = new List<GetClearanceStockMovementOrder>();

            using (AuthContext context = new AuthContext())
            {

                if (!roleNames.Any(x => x == "Buyer" || x == "Sourcing Executive") && userid > 0)
                {
                    if (Data != null && Data.Warehouseid > 0)
                    {


                        if (context.Database.Connection.State != ConnectionState.Open)
                            context.Database.Connection.Open();
                        var cmd = context.Database.Connection.CreateCommand();
                        cmd.CommandTimeout = 900;
                        cmd.CommandText = "[Clearance].[GetClearanceStockMovementOrderListByHQ]";

                        cmd.Parameters.Add(new SqlParameter("@WarehouseId", Data.Warehouseid));
                        cmd.Parameters.Add(new SqlParameter("@OrderType", Data.OrderType ?? (object)DBNull.Value));
                        cmd.Parameters.Add(new SqlParameter("@Status", Data.Status ?? (object)DBNull.Value));
                        cmd.Parameters.Add(new SqlParameter("@FromDate", Data.FromDate ?? (object)DBNull.Value));
                        cmd.Parameters.Add(new SqlParameter("@ToDate", Data.ToDate ?? (object)DBNull.Value));
                        cmd.Parameters.Add(new SqlParameter("@skip", Data.skip));
                        cmd.Parameters.Add(new SqlParameter("@take", Data.take));
                        cmd.Parameters.Add(new SqlParameter("@keyword", Data.keyword ?? (object)DBNull.Value));

                        cmd.CommandType = System.Data.CommandType.StoredProcedure;

                        var reader = cmd.ExecuteReader();
                        var newdata = ((IObjectContextAdapter)context)
                        .ObjectContext
                        .Translate<GetClearanceStockMovementOrder>(reader).ToList();

                        reader.NextResult();
                        while (reader.Read())
                        {
                            res.GetClearanceStockMovementOrder = newdata;
                            res.TotalRecords = Convert.ToInt32(reader["itemCount"]);
                        }

                    }
                }
                else if (roleNames.Any(x => x == "Buyer" || x == "Sourcing Executive") && userid > 0)
                {
                    if (Data != null && Data.Warehouseid > 0)
                    {

                        if (context.Database.Connection.State != ConnectionState.Open)
                            context.Database.Connection.Open();
                        var cmd = context.Database.Connection.CreateCommand();
                        cmd.CommandTimeout = 900;
                        cmd.CommandText = "[Clearance].[GetClearanceStockMovementOrderList]";
                        cmd.Parameters.Add(new SqlParameter("@UserId", userid));
                        cmd.Parameters.Add(new SqlParameter("@WarehouseId", Data.Warehouseid));
                        cmd.Parameters.Add(new SqlParameter("@OrderType", Data.OrderType ?? (object)DBNull.Value));
                        cmd.Parameters.Add(new SqlParameter("@Status", Data.Status ?? (object)DBNull.Value));
                        cmd.Parameters.Add(new SqlParameter("@FromDate", Data.FromDate ?? (object)DBNull.Value));
                        cmd.Parameters.Add(new SqlParameter("@ToDate", Data.ToDate ?? (object)DBNull.Value));
                        cmd.Parameters.Add(new SqlParameter("@skip", Data.skip));
                        cmd.Parameters.Add(new SqlParameter("@take", Data.take));
                        cmd.Parameters.Add(new SqlParameter("@keyword", Data.keyword ?? (object)DBNull.Value));

                        cmd.CommandType = System.Data.CommandType.StoredProcedure;

                        var reader = cmd.ExecuteReader();
                        var newdata = ((IObjectContextAdapter)context)
                        .ObjectContext
                        .Translate<GetClearanceStockMovementOrder>(reader).ToList();

                        reader.NextResult();
                        while (reader.Read())
                        {
                            res.GetClearanceStockMovementOrder = newdata;
                            res.TotalRecords = Convert.ToInt32(reader["itemCount"]);
                        }

                    }
                }
                return res;
            }
        }


        [Route("GetClearanceStockMovementOrderListExport")]
        [HttpPost]
        public ClearanceStockMovementOrderList GetClearanceStockMovementOrderListExport(GetClearanceStockMovementOrderDC Data)
        {
            var identity = User.Identity as ClaimsIdentity;
            int userid = 0;
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

            List<string> roleNames = new List<string>();
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "RoleNames") && identity.Claims.FirstOrDefault(x => x.Type == "RoleNames").Value != "")
                roleNames = (identity.Claims.FirstOrDefault(x => x.Type == "RoleNames").Value).Split(',').ToList();

            ClearanceStockMovementOrderList res = new ClearanceStockMovementOrderList();
            List<GetClearanceStockMovementOrder> result = new List<GetClearanceStockMovementOrder>();

            using (AuthContext context = new AuthContext())
            {

                if (!roleNames.Any(x => x == "Buyer" || x == "Sourcing Executive") && userid > 0)
                {
                    if (Data != null && Data.Warehouseid > 0)
                    {

                        if (context.Database.Connection.State != ConnectionState.Open)
                            context.Database.Connection.Open();
                        var cmd = context.Database.Connection.CreateCommand();
                        cmd.CommandTimeout = 900;
                        cmd.CommandText = "[Clearance].[GetClearanceStockMovementOrderListByHQ]";
                        cmd.Parameters.Add(new SqlParameter("@WarehouseId", Data.Warehouseid));
                        cmd.Parameters.Add(new SqlParameter("@OrderType", Data.OrderType ?? (object)DBNull.Value));
                        cmd.Parameters.Add(new SqlParameter("@Status", Data.Status ?? (object)DBNull.Value));
                        cmd.Parameters.Add(new SqlParameter("@FromDate", Data.FromDate ?? (object)DBNull.Value));
                        cmd.Parameters.Add(new SqlParameter("@ToDate", Data.ToDate ?? (object)DBNull.Value));
                        cmd.Parameters.Add(new SqlParameter("@keyword", Data.keyword ?? (object)DBNull.Value));
                        cmd.Parameters.Add(new SqlParameter("@IsExport", Data.IsExport));

                        cmd.CommandType = System.Data.CommandType.StoredProcedure;

                        var reader = cmd.ExecuteReader();
                        var newdata = ((IObjectContextAdapter)context)
                        .ObjectContext
                        .Translate<GetClearanceStockMovementOrder>(reader).ToList();

                        res.GetClearanceStockMovementOrder = newdata;

                    }
                }
                else if (roleNames.Any(x => x == "Buyer" || x == "Sourcing Executive") && userid > 0)
                {
                    if (Data != null && Data.Warehouseid > 0)
                    {

                        if (context.Database.Connection.State != ConnectionState.Open)
                            context.Database.Connection.Open();
                        var cmd = context.Database.Connection.CreateCommand();
                        cmd.CommandTimeout = 900;
                        cmd.CommandText = "[Clearance].[GetClearanceStockMovementOrderList]";
                        cmd.Parameters.Add(new SqlParameter("@UserId", userid));
                        cmd.Parameters.Add(new SqlParameter("@WarehouseId", Data.Warehouseid));
                        cmd.Parameters.Add(new SqlParameter("@OrderType", Data.OrderType ?? (object)DBNull.Value));
                        cmd.Parameters.Add(new SqlParameter("@Status", Data.Status ?? (object)DBNull.Value));
                        cmd.Parameters.Add(new SqlParameter("@FromDate", Data.FromDate ?? (object)DBNull.Value));
                        cmd.Parameters.Add(new SqlParameter("@ToDate", Data.ToDate ?? (object)DBNull.Value));
                        cmd.Parameters.Add(new SqlParameter("@keyword", Data.keyword ?? (object)DBNull.Value));
                        cmd.Parameters.Add(new SqlParameter("@IsExport", Data.IsExport));

                        cmd.CommandType = System.Data.CommandType.StoredProcedure;

                        var reader = cmd.ExecuteReader();
                        var newdata = ((IObjectContextAdapter)context)
                        .ObjectContext
                        .Translate<GetClearanceStockMovementOrder>(reader).ToList();

                        res.GetClearanceStockMovementOrder = newdata;


                    }
                }
                return res;
            }
        }


        //Stock hit
        [Route("UpdateClearanceNonSellableOrderStatus")]
        [HttpGet]
        public async Task<ResponseMsg> UpdateClearanceNonSellableOrderStatus(long id, string Status, string comment)
        {
             var identity = User.Identity as ClaimsIdentity;
            int userid = 0;
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
            ResponseMsg result = new ResponseMsg();
            List<string> roleNames = new List<string>();
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "RoleNames") && identity.Claims.FirstOrDefault(x => x.Type == "RoleNames").Value != "")
                roleNames = (identity.Claims.FirstOrDefault(x => x.Type == "RoleNames").Value).Split(',').ToList();
            var Buyer = roleNames.Where(x => x == "Buyer" || x == "Sourcing Executive").FirstOrDefault();
            var HQlogin = roleNames.Where(x => x == "HQ Master login").FirstOrDefault();
            string ToInboundLeadEmail = "";

           if ((Buyer == "Buyer" || Buyer == "Sourcing Executive") && HQlogin != "HQ Master login")
            {
                if (Status != null && id > 0 && userid > 0)
                {
                    TransactionOptions option = new TransactionOptions();
                    option.IsolationLevel = System.Transactions.IsolationLevel.RepeatableRead;
                    option.Timeout = TimeSpan.FromSeconds(120);
                    using (var dbContextTransaction = new TransactionScope(TransactionScopeOption.RequiresNew, option))
                    {
                        using (AuthContext context = new AuthContext())
                        {
                            var ClearanceNonSellableOrder = context.ClearanceNonSaleableMovementOrders.Where(x => x.Id == id).FirstOrDefault();
                            var ClearanceNonSellableOrderDetails = context.ClearanceNonSaleableMovementOrderDetails.Where(x => x.ClearanceStockMovementOrderMasterId == id).ToList();

                            List<int> PeopleIds = new List<int>();
                            PeopleIds.Add(userid);
                            PeopleIds.Add(ClearanceNonSellableOrder.CreatedBy);

                            var peoples = context.Peoples.Where(x => PeopleIds.Contains(x.PeopleID)).ToList();

                            var FromBuyerEmail = peoples.Where(x => x.PeopleID == userid).FirstOrDefault();
                            ToInboundLeadEmail = peoples.Where(x => x.PeopleID == ClearanceNonSellableOrder.CreatedBy).Select(x => x.Email).FirstOrDefault();

                            var check = context.Warehouses.FirstOrDefault(x => x.WarehouseId == ClearanceNonSellableOrder.WarehouseId).IsStopCurrentStockTrans;
                            if (check)
                            {
                                result.Status = false;
                                result.Message = "Inventory Transactions are currently disabled for this warehouse... Please start it first and try again";
                                return result;
                            }

                            if (ClearanceNonSellableOrder != null && ClearanceNonSellableOrder.Status == "Pending" && Status == "Approved") // Code
                            {
                                var totalqty = ClearanceNonSellableOrderDetails.Where(x => x.Id > 0).Sum(x => x.Quantity);
                                if (totalqty == 0)
                                {
                                    result.Status = false;
                                    result.Message = "Zero Qty Orders cant be approved, please choose Reject option.";
                                    return result;
                                }
                                ClearanceNonSellableOrder.Status = "Approved";
                                ClearanceNonSellableOrder.ModifiedBy = userid;
                                ClearanceNonSellableOrder.ApprovedDate = DateTime.Now;
                                ClearanceNonSellableOrder.Approvedby = userid;
                                ClearanceNonSellableOrder.ModifiedDate = DateTime.Now;

                                StockTransactionHelper sthelper = new StockTransactionHelper();
                                List<PhysicalStockUpdateRequestDc> manualStockUpdateDcList = new List<PhysicalStockUpdateRequestDc>();
                                List<TransferStockDTONew> transferStockList = new List<TransferStockDTONew>();
                                BatchMasterManager batchMasterManager = new BatchMasterManager();

                                var StockBatchMasterIds = ClearanceNonSellableOrder.ClearanceNonSaleableMovementOrderDetails.Select(x => x.StockBatchMasterId).Distinct().ToList();
                                var CurrentStockBatchMasters = context.StockBatchMasters.Where(x => StockBatchMasterIds.Contains(x.Id)).ToList();

                                if (CurrentStockBatchMasters.Sum(x => x.Qty) == 0)
                                {
                                    result.Status = false;
                                    result.Message = "Orders can't approved, due to batch qty is not available. please choose Reject option.";
                                    return result;
                                }
                                if (ClearanceNonSellableOrder.OrderType == "CurrentToClearance")
                                {
                                    foreach (var item in ClearanceNonSellableOrderDetails)
                                    {
                                        int DispatchedqTY = 0;
                                        int qty = CurrentStockBatchMasters.FirstOrDefault(x => x.Id == item.StockBatchMasterId && x.StockType == "C").Qty;
                                        if (qty > 0)
                                        {
                                            DispatchedqTY = item.Quantity < qty ? item.Quantity : qty;
                                            PhysicalStockUpdateRequestDc manualStockUpdateDc = new PhysicalStockUpdateRequestDc
                                            {
                                                ItemMultiMRPId = item.ItemMultiMRPId,
                                                Reason = "CurrentToClearancePlanned",
                                                StockTransferType = "ManualInventory",
                                                Qty = DispatchedqTY,
                                                WarehouseId = ClearanceNonSellableOrder.WarehouseId,
                                                DestinationStockType = StockTypeTableNames.ClearancePlannedStocks,
                                                SourceStockType = StockTypeTableNames.CurrentStocks,
                                            };
                                            manualStockUpdateDcList.Add(manualStockUpdateDc);

                                            transferStockList.Add(new TransferStockDTONew
                                            {
                                                ItemMultiMRPId = item.ItemMultiMRPId,
                                                ItemMultiMRPIdTrans = item.ItemMultiMRPId,
                                                StockBatchMasterId = item.StockBatchMasterId,
                                                Qty = DispatchedqTY,
                                                WarehouseId = ClearanceNonSellableOrder.WarehouseId,
                                                BatchMasterId = 0
                                            });
                                        }
                                        item.Quantity = DispatchedqTY;
                                        item.ModifiedDate = DateTime.Now;
                                        item.ModifiedBy = userid;
                                        context.Entry(item).State = EntityState.Modified;

                                    }
                                }
                                else if (ClearanceNonSellableOrder.OrderType == "ClearanceToCurrent")
                                {
                                    foreach (var item in ClearanceNonSellableOrderDetails)
                                    {
                                        int DispatchedqTY = 0;
                                        var qty = CurrentStockBatchMasters.FirstOrDefault(x => x.Id == item.StockBatchMasterId && x.StockType == "CL").Qty;
                                        if (qty > 0)
                                        {
                                            DispatchedqTY = item.Quantity < qty ? item.Quantity : qty;
                                            PhysicalStockUpdateRequestDc manualStockUpdateDc = new PhysicalStockUpdateRequestDc
                                            {
                                                ItemMultiMRPId = item.ItemMultiMRPId,
                                                Reason = "ClearanceToClearancePlanned",
                                                StockTransferType = "ManualInventory",
                                                Qty = DispatchedqTY,
                                                WarehouseId = ClearanceNonSellableOrder.WarehouseId,
                                                DestinationStockType = StockTypeTableNames.ClearancePlannedStocks,
                                                SourceStockType = StockTypeTableNames.ClearanceStockNews,
                                            };
                                            manualStockUpdateDcList.Add(manualStockUpdateDc);
                                            transferStockList.Add(new TransferStockDTONew
                                            {
                                                ItemMultiMRPId = item.ItemMultiMRPId,
                                                ItemMultiMRPIdTrans = item.ItemMultiMRPId,
                                                StockBatchMasterId = item.StockBatchMasterId,
                                                Qty = DispatchedqTY,
                                                WarehouseId = ClearanceNonSellableOrder.WarehouseId,
                                                BatchMasterId = 0
                                            });
                                        }
                                        item.Quantity = DispatchedqTY;
                                        item.ModifiedDate = DateTime.Now;
                                        item.ModifiedBy = userid;
                                        context.Entry(item).State = EntityState.Modified;

                                    }
                                }
                                foreach (var item in transferStockList)
                                {
                                    bool isBatchSuccess = batchMasterManager.UpdateStockInSameBatch(item.StockBatchMasterId, context, userid, item.Qty);
                                    if (!isBatchSuccess)
                                    {
                                        result.Status = false;
                                        result.Message = " Issue in Batch stock. Please try after some time.";
                                        return result;
                                    }
                                }
                                foreach (var item in manualStockUpdateDcList.Where(x => x.Qty > 0))
                                {
                                    bool isSuccess = sthelper.TransferBetweenPhysicalStocks(item, userid, context, dbContextTransaction);
                                    if (!isSuccess)
                                    {
                                        result.Status = false;
                                        result.Message = " Issue in stock. Please try after some time. ";
                                        return result;
                                    }
                                }
                            }
                            else if (ClearanceNonSellableOrder != null && ClearanceNonSellableOrder.Status == "Pending" && Status == "Rejected")
                            {
                                ClearanceNonSellableOrder.Status = "Rejected";
                                ClearanceNonSellableOrder.ModifiedBy = userid;
                                ClearanceNonSellableOrder.ModifiedDate = DateTime.Now;
                                ClearanceNonSellableOrder.Comment = comment;
                                context.Entry(ClearanceNonSellableOrder).State = EntityState.Modified;
                                context.Commit();
                            }
                            else
                            {
                                result.Status = false;
                                result.Message = " something went wrong. ";
                                return result;
                            }
                            ClearanceNonSellableOrder.ModifiedDate = DateTime.Now;
                            context.Entry(ClearanceNonSellableOrder).State = EntityState.Modified;
                            if (context.Commit() > 0)
                            {
                                dbContextTransaction.Complete();
                                if (ConfigurationManager.AppSettings["Environment"] == "Production")
                                {
                                    var Sub = "Clearance Order no.: " + ClearanceNonSellableOrder.Id + " " + Status;
                                    var msg = " Clearance Order no.: " + ClearanceNonSellableOrder.Id + " has been " + Status + " by " + FromBuyerEmail.PeopleFirstName + " " + FromBuyerEmail.PeopleLastName + "." + "Please physically move the order item to the Clearance Area";

                                    EmailHelper.SendMail(FromBuyerEmail.Email, ToInboundLeadEmail, "", Sub, msg, "");
                                }

                                result.Status = true;
                                result.Message = " Order " + Status + " successfully  #No. :" + ClearanceNonSellableOrder.Id;
                                return result;
                            }
                            else
                            {
                                result.Status = false;
                                result.Message = "something went wrong";
                            }

                        }
                    }

                }

            }
            else
            {
                result.Status = false;

                result.Message = " You are not authorized ";
            }
            return result;


        }

        [Route("GetCleNonSaleableMovementOrderDetails")]
        [HttpGet]
        public List<GetCleNonSaleableMovementOrderDC> GetCleNonSaleableMovementOrderDetails(int Id)
        {

            List<GetCleNonSaleableMovementOrderDC> cleNonSaleableMovementOrderListDC = new List<GetCleNonSaleableMovementOrderDC>();
            List<GetCleNonSaleableMovementOrderDetailsDC> cleNonSaleableMovementOrderDetailsListDC = new List<GetCleNonSaleableMovementOrderDetailsDC>();

            if (Id > 0)
            {
                using (AuthContext context = new AuthContext())
                {
                    if (context.Database.Connection.State != ConnectionState.Open)
                        context.Database.Connection.Open();

                    var cmd = context.Database.Connection.CreateCommand();
                    cmd.CommandTimeout = 900;
                    cmd.CommandText = "[Clearance].[GetClearanceNonSaleableMovementOrderDetails]";
                    cmd.Parameters.Add(new SqlParameter("@OrderId", Id));
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    var reader = cmd.ExecuteReader();
                    cleNonSaleableMovementOrderListDC = ((IObjectContextAdapter)context).ObjectContext.Translate<GetCleNonSaleableMovementOrderDC>(reader).ToList();

                    reader.NextResult();

                    cleNonSaleableMovementOrderDetailsListDC = ((IObjectContextAdapter)context).ObjectContext.Translate<GetCleNonSaleableMovementOrderDetailsDC>(reader).ToList();


                    if (cleNonSaleableMovementOrderListDC != null && cleNonSaleableMovementOrderListDC.Any() && cleNonSaleableMovementOrderDetailsListDC != null && cleNonSaleableMovementOrderDetailsListDC.Any())
                    {

                        foreach (var element in cleNonSaleableMovementOrderListDC)
                        {
                            List<GetCleNonSaleableMovementOrderDetailsDC> localData = cleNonSaleableMovementOrderDetailsListDC.Where(x => x.ItemMultiMRPId == element.ItemMultiMRPId).ToList();
                            if (localData != null && localData.Any())
                            {
                                element.GetCleNonSaleableMovementOrderDetailsDCs = localData.ToList();
                            }
                        }
                    }
                }

            }
            return cleNonSaleableMovementOrderListDC;
        }
        [Route("UpdateCleNonSaleableMovementOrderQuantity")]
        [HttpGet]
        public async Task<ResponseMsg> UpdateCleNonSaleableMovementOrderQuantity(long OrderMasterId, int Quantity, long OrderDetailId)
        {

            var identity = User.Identity as ClaimsIdentity;
            int userid = 0;

            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
            ResponseMsg result = new ResponseMsg();
            List<string> roleNames = new List<string>();
            var HQlogin = roleNames.Where(x => x == "HQ Master login").FirstOrDefault();
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "RoleNames") && identity.Claims.FirstOrDefault(x => x.Type == "RoleNames").Value != "")
                roleNames = (identity.Claims.FirstOrDefault(x => x.Type == "RoleNames").Value).Split(',').ToList();

            if (roleNames.Any(x => x == "Buyer" || x == "Sourcing Executive") && HQlogin != "HQ Master login")
            {
                if (OrderMasterId > 0 && Quantity >= 0 && userid > 0 && OrderDetailId > 0)
                {
                    TransactionOptions option = new TransactionOptions();
                    option.IsolationLevel = System.Transactions.IsolationLevel.RepeatableRead;
                    option.Timeout = TimeSpan.FromSeconds(120);
                    using (var dbContextTransaction = new TransactionScope(TransactionScopeOption.RequiresNew, option))
                    {
                        using (AuthContext context = new AuthContext())
                        {
                            var ClearanceNonSellableOrder = context.ClearanceNonSaleableMovementOrders.Where(x => x.Id == OrderMasterId).FirstOrDefault();
                            if (ClearanceNonSellableOrder.Status == "Pending")
                            {
                                var ClearanceNonSellableOrderDetail = context.ClearanceNonSaleableMovementOrderDetails.Where(x => x.Id == OrderDetailId).ToList();
                                foreach (var item in ClearanceNonSellableOrderDetail)
                                {
                                    item.Quantity = Quantity;
                                    item.ModifiedBy = userid;
                                    item.ModifiedDate = DateTime.Now;
                                    context.Entry(item).State = EntityState.Modified;
                                }
                                if (context.Commit() > 0)
                                {
                                    dbContextTransaction.Complete();
                                    result.Status = true;
                                    result.Message = "Quantity Updated Successfully ";
                                }
                            }
                            else
                            {
                                result.Status = false;
                                result.Message = "Can't updated ,due order status in : " + ClearanceNonSellableOrder.Status;
                            }
                        }
                    }
                    return result;
                }
                else
                {
                    result.Status = false;
                    result.Message = " Something went wrong ";
                    return result;
                }
            }
            else
            {
                result.Status = false;
                result.Message = " You are not authorized ";
                return result;
            }
        }

        [Route("BuyerNameList")]
        [HttpGet]
        public List<BuyerNameListDc> BuyerNameList()
        {

            List<BuyerNameListDc> list = new List<BuyerNameListDc>();
            var identity = User.Identity as ClaimsIdentity;
            int userid = 0;
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

            List<string> roleNames = new List<string>();
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "RoleNames") && identity.Claims.FirstOrDefault(x => x.Type == "RoleNames").Value != "")
                roleNames = (identity.Claims.FirstOrDefault(x => x.Type == "RoleNames").Value).Split(',').ToList();

            if (userid > 0)
            {
                using (AuthContext context = new AuthContext())
                {
                    var query = context.Database.SqlQuery<BuyerNameListDc>("EXEC [Clearance].[BuyerNameList]").ToList();
                    list = query;

                }
            }
            return list;
        }



        [Route("BuyerNameListByWhid")]
        [HttpGet]
        public List<BuyerNameListDc> BuyerNameListByWhid(int WarehouseId, string StockType)
        {
            List<BuyerNameListDc> list = new List<BuyerNameListDc>();
            var identity = User.Identity as ClaimsIdentity;
            int userid = 0;
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
            if (userid > 0 && WarehouseId > 0 && !string.IsNullOrEmpty(StockType))
            {
                using (AuthContext context = new AuthContext())
                {
                    if (context.Database.Connection.State != ConnectionState.Open)
                        context.Database.Connection.Open();
                    var cmd = context.Database.Connection.CreateCommand();
                    cmd.CommandTimeout = 900;
                    cmd.CommandText = "[Clearance].[BuyerNameListByWhid]";
                    cmd.Parameters.Add(new SqlParameter("@WarehouseId", WarehouseId));
                    cmd.Parameters.Add(new SqlParameter("@StockType", StockType));
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;

                    var reader = cmd.ExecuteReader();
                    list = ((IObjectContextAdapter)context)
                    .ObjectContext
                    .Translate<BuyerNameListDc>(reader).ToList();
                }
            }
            return list;
        }



        [Route("GetClearanceItemListByBuyerId")]
        [HttpGet]
        public List<GetClearanceOrderItemDC> GetClearanceItemListByBuyerId(int Buyerid, int WarehouseId, string OrderType)
        {

            List<GetClearanceOrderItemDC> clearanceOrderItemDCList = new List<GetClearanceOrderItemDC>();
            List<GetClearanceOrderItemDetailDC> clearanceOrderItemDetailDCList = new List<GetClearanceOrderItemDetailDC>();
            var identity = User.Identity as ClaimsIdentity;
            int userid = 0;

            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

            if (Buyerid > 0 && WarehouseId > 0 && OrderType != null)
            {
                using (AuthContext context = new AuthContext())
                {
                    if (context.Database.Connection.State != ConnectionState.Open)
                        context.Database.Connection.Open();
                    var cmd = context.Database.Connection.CreateCommand();
                    cmd.CommandTimeout = 900;
                    cmd.CommandText = "[Clearance].[ItemListByBuyer]";
                    cmd.Parameters.Add(new SqlParameter("@WarehouseId", WarehouseId));
                    cmd.Parameters.Add(new SqlParameter("@OrderType", OrderType));
                    cmd.Parameters.Add(new SqlParameter("@BuyerId", Buyerid));
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;

                    var reader = cmd.ExecuteReader();
                    clearanceOrderItemDCList = ((IObjectContextAdapter)context).ObjectContext.Translate<GetClearanceOrderItemDC>(reader).ToList();

                    reader.NextResult();

                    clearanceOrderItemDetailDCList = ((IObjectContextAdapter)context).ObjectContext.Translate<GetClearanceOrderItemDetailDC>(reader).ToList();


                    if (clearanceOrderItemDCList != null && clearanceOrderItemDCList.Any() && clearanceOrderItemDetailDCList != null && clearanceOrderItemDetailDCList.Any())
                    {

                        foreach (var element in clearanceOrderItemDCList)
                        {
                            List<GetClearanceOrderItemDetailDC> localData = clearanceOrderItemDetailDCList.Where(x => x.ItemMultiMRPId == element.ItemMultiMRPId).ToList();
                            if (localData != null && localData.Any())
                            {
                                element.GetClearanceOrderItemDetailDCs = localData.ToList();
                            }
                        }
                    }
                }
            }
            return clearanceOrderItemDCList;
        }


        [Route("WarehouseByUser")]
        [HttpGet]
        public List<WarehouseByUserDc> WarehouseByUser()
        {

            List<WarehouseByUserDc> list = new List<WarehouseByUserDc>();
            var identity = User.Identity as ClaimsIdentity;
            int userid = 0;
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

            List<string> roleNames = new List<string>();
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "RoleNames") && identity.Claims.FirstOrDefault(x => x.Type == "RoleNames").Value != "")
                roleNames = (identity.Claims.FirstOrDefault(x => x.Type == "RoleNames").Value).Split(',').ToList();

            var HQlogin = roleNames.Where(x => x == "HQ Master login").FirstOrDefault();

            userid = roleNames.Any(x => (x == "Inbound Lead") && HQlogin != "HQ Master login") ? userid : 0;
            if (roleNames.Any(x => x == "Inbound Lead" || x == "HQ Master login"))
            {
                using (AuthContext context = new AuthContext())
                {
                    var param1 = new SqlParameter("@userid", userid);
                    var query = context.Database.SqlQuery<WarehouseByUserDc>("EXEC [dbo].[ActiveWarehouseByUserId] @userid ", param1).ToList();
                    list = query;

                }
            }
            return list;
        }


        [Route("EmailForPendngOrd")]
        [HttpGet]
        [AllowAnonymous]
        public async Task<bool> EmailForPendngOrd()
        {
            string From = "";
            From = AppConstants.MasterEmail;
            string ExcelSavePath = HttpContext.Current.Server.MapPath("~/ExcelGeneratePath/");

            using (AuthContext context = new AuthContext())
            {
                var ClearanceNonSellableOrder = context.Database.SqlQuery<getClPendingOrd>("EXEC getClPendingOrdForMail ").ToList();

                if (ClearanceNonSellableOrder != null && ClearanceNonSellableOrder.Any())
                {
                    var email = ClearanceNonSellableOrder.Select(t => t.Email).Distinct().ToList();
                    foreach (var r in email)
                    {
                        DataTable table = new DataTable();
                        List<getClPendingOrd> data = new List<getClPendingOrd>();
                        var datatables = new List<DataTable>();
                        if (!Directory.Exists(ExcelSavePath))
                            Directory.CreateDirectory(ExcelSavePath);

                        foreach (var e in ClearanceNonSellableOrder.Where(y => y.Email == r).ToList())
                        {
                            data.Add(new getClPendingOrd
                            {
                                BuyerName = e.BuyerName,
                                Email = e.Email,
                                WarehouseName = e.WarehouseName,
                                OrderId = e.OrderId,
                                OrderDate = e.OrderDate,
                                OrderValue = e.OrderValue
                            });
                        }
                        table = ClassToDataTable.CreateDataTable(data);
                        table.TableName = "PendingOrderList";
                        datatables.Add(table);

                        string filePath = ExcelSavePath + "PendingOrderList" + DateTime.Now.ToString("yyyy-dd-M--HH-mm-ss") + ".xlsx";
                        if (ExcelGenerator.DataTable_To_Excel(datatables, filePath))
                        {
                            DateTime now = DateTime.Now;
                            var sub = "Clearance Pending Orders As on" + now.Date;
                            var msg = "Hello " + ClearanceNonSellableOrder.FirstOrDefault(y => y.Email == r).BuyerName + " ,Please approve this order numbers";
                            EmailHelper.SendMail(From, ClearanceNonSellableOrder.FirstOrDefault(y => y.Email == r).Email, "", sub, msg, filePath);
                        }
                    }
                }

            }

            return true;
        }

        #endregion

        #region SearchItem By OrderType and Warehouse

        [Route("GetClearanceOrderTypeItems")]
        [HttpGet]
        public List<GetItemListDC> GetClearanceOrderTypeItems(int Warehouseid, string OrderType, string Key)
        {
            List<GetItemListDC> Items = new List<GetItemListDC>();

            if (Warehouseid > 0 && OrderType != null && Key != null)
            {
                using (AuthContext context = new AuthContext())
                {
                    var Wareid = new SqlParameter("@Warehouseid", Warehouseid);


                    var orderType = new SqlParameter("@OrderType", OrderType);
                    var key = new SqlParameter("@Key", Key);
                    Items = context.Database.SqlQuery<GetItemListDC>("EXEC [Clearance].[GetClearanceOrderTypeItems] @Warehouseid,@OrderType,@Key", Wareid, orderType, key).ToList();

                }
            }
            return Items;

        }

        #endregion


        #region  Clearance Non-Sellable Configuration  Page
        [Route("ClearanceNonConfigPageCategoryList")]
        [HttpGet]
        public List<CategoryListDc> CategoryList()
        {
            var Buyerid = 0;
            List<CategoryListDc> list = new List<CategoryListDc>();
            var identity = User.Identity as ClaimsIdentity;
            int userid = 0;
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

            List<string> roleNames = new List<string>();
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "RoleNames") && identity.Claims.FirstOrDefault(x => x.Type == "RoleNames").Value != "")
                roleNames = (identity.Claims.FirstOrDefault(x => x.Type == "RoleNames").Value).Split(',').ToList();

            if (userid > 0)
            {
                using (AuthContext context = new AuthContext())
                {
                    if (context.BrandBuyerDB.Any(x => x.BuyerId == userid && x.Active && !x.Deleted))
                    {
                        Buyerid = userid;
                    }
                    var buyerid = new SqlParameter("@buyerId", Buyerid);
                    var query = context.Database.SqlQuery<CategoryListDc>("EXEC Sp_GetCategoryByBuyer @buyerId", buyerid).ToList();
                    list = query;

                }
            }
            return list;
        }


        [Route("ClearanceNonConfigPageListItems")]
        [HttpPost]
        public List<ClearanceNonConfigSearch> GetListOfItems(SearchitemOnConfigurationPage list)
        {
            var identity = User.Identity as ClaimsIdentity;
            int userid = 0;
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

            List<string> roleNames = new List<string>();
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "RoleNames") && identity.Claims.FirstOrDefault(x => x.Type == "RoleNames").Value != "")
                roleNames = (identity.Claims.FirstOrDefault(x => x.Type == "RoleNames").Value).Split(',').ToList();
            using (AuthContext context = new AuthContext())
            {
                if (list != null && list.ItemNumbers != null && list.ItemNumbers.Count > 0)
                {
                    var IdDt = new DataTable();
                    IdDt = new DataTable();
                    IdDt.Columns.Add("StringValue");

                    foreach (var item in list.ItemNumbers)
                    {
                        var dr = IdDt.NewRow();
                        dr["StringValue"] = item;
                        IdDt.Rows.Add(dr);
                    }
                    var param = new SqlParameter("ItemNumbers", IdDt);
                    param.SqlDbType = SqlDbType.Structured;
                    param.TypeName = "dbo.stringValues";

                    var res = context.Database.SqlQuery<ClearanceNonConfigSearch>("Exec GetItemListByItemNumbers @ItemNumbers", param).ToList();
                    return res;
                }
                else
                {
                    var param = new SqlParameter("@CategoryId", list.CategoryId);
                    var param2 = new SqlParameter("@SubCategoryId", list.SubCategoryId);
                    var param3 = new SqlParameter("@BrandId", list.BrandId);
                    var res = context.Database.SqlQuery<ClearanceNonConfigSearch>("Exec GetBrandWiseSlefLife @CategoryId,@SubCategoryId,@BrandId", param, param2, param3).ToList();
                    return res;

                }

            }
        }

        [Route("ClearanceNonConfigPageUpdateShelfLife")]
        [HttpPost]
        public async Task<ResponseMsg> UpdatedBulkList(List<UpdateList> List)
        {

            var identity = User.Identity as ClaimsIdentity;
            int userid = 0;
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
            ResponseMsg result = new ResponseMsg();
            List<string> roleNames = new List<string>();
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "RoleNames") && identity.Claims.FirstOrDefault(x => x.Type == "RoleNames").Value != "")
                roleNames = (identity.Claims.FirstOrDefault(x => x.Type == "RoleNames").Value).Split(',').ToList();

            if (roleNames.Any(x => x == "Buyer" || x == "Sourcing Executive" || x == "HQ Master login") && List != null && List.Any())
            {
                List<ClNSShelfConfigurationTemp> ClNSShelfConfigurationTempList = new List<ClNSShelfConfigurationTemp>();
                using (AuthContext context = new AuthContext())
                {
                    foreach (var item in List)
                    {
                        var Existdata = context.ClNSShelfConfigurationTemps.FirstOrDefault(x => x.CategoryId == item.CategoryId && x.Status == "Pending" && x.SubCategoryId == item.SubCategoryId && x.BrandId == item.BrandId && x.ItemNumber == (item.ItemNumber == null ? null : item.ItemNumber) && x.IsActive == true && x.IsDeleted == false);
                        if (Existdata != null && Existdata.Status == "Pending")
                        {

                            Existdata.ClearanceShelfLifeFrom = item.ClearanceShelfLifeFrom;
                            Existdata.ClearanceShelfLifeTo = item.ClearanceShelfLifeTo;
                            Existdata.NonSellShelfLifeFrom = item.NonSellShelfLifeFrom;
                            Existdata.NonSellShelfLifeTo = item.NonSellShelfLifeTo;
                            context.Entry(Existdata).State = EntityState.Modified;
                        }
                        if (Existdata == null)
                        {
                            ClNSShelfConfigurationTemp ClNS = new ClNSShelfConfigurationTemp();
                            ClNS.CategoryId = item.CategoryId;
                            ClNS.SubCategoryId = item.SubCategoryId;
                            ClNS.BrandId = item.BrandId;
                            ClNS.ItemNumber = item.ItemNumber == null ? null : item.ItemNumber;
                            ClNS.Status = "Pending";
                            ClNS.CreatedBy = userid;
                            ClNS.CreatedDate = DateTime.Now;
                            ClNS.IsActive = true;
                            ClNS.IsDeleted = false;
                            ClNS.ClearanceShelfLifeFrom = item.ClearanceShelfLifeFrom;
                            ClNS.ClearanceShelfLifeTo = item.ClearanceShelfLifeTo;
                            ClNS.NonSellShelfLifeFrom = item.NonSellShelfLifeFrom;
                            ClNS.NonSellShelfLifeTo = item.NonSellShelfLifeTo;
                            ClNSShelfConfigurationTempList.Add(ClNS);
                        }
                    }
                    if (ClNSShelfConfigurationTempList != null && ClNSShelfConfigurationTempList.Any())
                    {
                        context.ClNSShelfConfigurationTemps.AddRange(ClNSShelfConfigurationTempList);
                        result.Status = context.Commit() > 0;
                        result.Message = "Record Added successfully";
                    }
                    else
                    {
                        result.Status = context.Commit() > 0;
                        result.Message = "Record updated successfully";
                    }
                }
                return result;
            }
            else
            {
                result.Status = false;
                result.Message = " You are not authorized ";
                return result;
            }

        }
        #endregion


        #region Clearance Non-Sellable Configuration Aprove/Reject Page
        [Route("ClearanceNonConfigAprovPageSearch")]
        [HttpGet]
        public ApprovePageSearchDcList GetData(string status, int skip, int take, string keyword)


        {
            var identity = User.Identity as ClaimsIdentity;
            int userid = 0;
            if (keyword == null)
            {
                keyword = "";
            }
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

            List<string> roleNames = new List<string>();
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "RoleNames") && identity.Claims.FirstOrDefault(x => x.Type == "RoleNames").Value != "")
                roleNames = (identity.Claims.FirstOrDefault(x => x.Type == "RoleNames").Value).Split(',').ToList();

            ApprovePageSearchDcList approvePageSearchDcList = new ApprovePageSearchDcList();

            if (roleNames.Any(x => x == "Digital sales lead" || x == "Buyer" || x == "Sourcing Executive" || x == "HQ Master login"))
            {
                using (AuthContext context = new AuthContext())
                {
                    if (status != null && status.Any())
                    {
                        var Status = new SqlParameter("@Status", status);
                        var Keyword = new SqlParameter("@Keyword", keyword);
                        var Skip = new SqlParameter("@Skip", skip);
                        var Take = new SqlParameter("@Take", take);

                        var list = context.Database.SqlQuery<ApprovePageSearchDc>("EXEC ClearanceNonApprovePageSearch @Status, @Keyword, @Skip, @Take", Status, Keyword, Skip, Take).ToList();

                        var Status2 = new SqlParameter("@Status", status);
                        var Keyword2 = new SqlParameter("@Keyword", keyword);
                        var Skip2 = new SqlParameter("@Skip", skip);
                        var Take2 = new SqlParameter("@Take", take);
                        var IsTakeCount = new SqlParameter("@IsTakeCount", true);
                        var total = context.Database.SqlQuery<int>("EXEC ClearanceNonApprovePageSearch @Status, @Keyword, @Skip, @Take, @IsTakeCount", Status2, Keyword2, Skip2, Take2, IsTakeCount).FirstOrDefault();

                        approvePageSearchDcList.ApprovePageSearchList = list;

                        approvePageSearchDcList.TotalRecords = total;
                    }

                }
            }
            return approvePageSearchDcList;
        }

        [Route("GetExistShelfLife")]
        [HttpGet]
        public List<ApprovePagePopupList> GetData(long Id)
        {
            List<ApprovePagePopupList> list = new List<ApprovePagePopupList>();
            var identity = User.Identity as ClaimsIdentity;
            int userid = 0;
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

            List<string> roleNames = new List<string>();
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "RoleNames") && identity.Claims.FirstOrDefault(x => x.Type == "RoleNames").Value != "")
                roleNames = (identity.Claims.FirstOrDefault(x => x.Type == "RoleNames").Value).Split(',').ToList();

            if (roleNames.Any(x => x == "Digital sales lead" || x == "Buyer" || x == "HQ Master login" || x == "Sourcing Executive"))
            {
                using (AuthContext context = new AuthContext())
                {
                    if (Id > 0)
                    {
                        var id = new SqlParameter("@Id", Id);
                        var query = context.Database.SqlQuery<ApprovePagePopupList>("EXEC ClearanceNonConfigAprovPageExistShelfLife @Id ", id).ToList();
                        list = query;
                    }
                }
            }
            return list;
        }

        [Route("ApproveShelfLife")]
        [HttpPost]
        public async Task<ResponseMsg> ApproveShelfLife(ApproveShelfLifeDc Approve)
        {

            var identity = User.Identity as ClaimsIdentity;
            int userid = 0;
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
            ResponseMsg result = new ResponseMsg();
            List<string> roleNames = new List<string>();
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "RoleNames") && identity.Claims.FirstOrDefault(x => x.Type == "RoleNames").Value != "")
                roleNames = (identity.Claims.FirstOrDefault(x => x.Type == "RoleNames").Value).Split(',').ToList();

            if (roleNames.Any(x => x == "Digital sales lead" || x == "HQ Master login"))
            {
                using (AuthContext context = new AuthContext())
                {
                    var Temp = context.ClNSShelfConfigurationTemps.FirstOrDefault(x => x.Id == Approve.Id);
                    if (Temp != null && Temp.Status == "Pending" && (Approve.Status == "Approved" || Approve.Status == "Rejected"))
                    {
                        if (Approve.Status == "Rejected")
                        {
                            Temp.ModifiedBy = userid;
                            Temp.ModifiedDate = DateTime.Now;
                            Temp.Status = "Rejected";
                            Temp.RejectComment = Approve.comment;
                            context.Entry(Temp).State = EntityState.Modified;
                            if (context.Commit() > 0)
                            {
                                result.Status = true;
                                result.Message = " Rejected ";
                            }
                        }
                        else
                        {
                            Temp.ModifiedBy = userid;
                            Temp.ModifiedDate = DateTime.Now;
                            Temp.Status = "Approved";
                            context.Entry(Temp).State = EntityState.Modified;
                            var Existdata = context.ClearanceNonsShelfConfigurations.FirstOrDefault(x => x.CategoryId == Temp.CategoryId && x.SubCategoryId == Temp.SubCategoryId && x.BrandId == Temp.BrandId && x.ItemNumber == (Temp.ItemNumber == null ? null : Temp.ItemNumber) && x.IsActive == true && x.IsDeleted == false);
                            if (Existdata == null)
                            {
                                ClearanceNonSaleableShelfConfiguration clearance = new ClearanceNonSaleableShelfConfiguration();
                                clearance.CategoryId = Temp.CategoryId;
                                clearance.SubCategoryId = Temp.SubCategoryId;
                                clearance.BrandId = Temp.BrandId;
                                clearance.ItemNumber = Temp.ItemNumber == null ? null : Temp.ItemNumber;
                                clearance.Status = Temp.Status;
                                clearance.CreatedBy = userid;
                                clearance.CreatedDate = DateTime.Now;
                                clearance.IsActive = true;
                                clearance.IsDeleted = false;
                                clearance.ApprovedBy = userid;
                                clearance.ClearanceShelfLifeFrom = Temp.ClearanceShelfLifeFrom;
                                clearance.ClearanceShelfLifeTo = Temp.ClearanceShelfLifeTo;
                                clearance.NonSellShelfLifeFrom = Temp.NonSellShelfLifeFrom;
                                clearance.NonSellShelfLifeTo = Temp.NonSellShelfLifeTo;
                                context.ClearanceNonsShelfConfigurations.Add(clearance);
                            }
                            else
                            {
                                Existdata.ClearanceShelfLifeFrom = Temp.ClearanceShelfLifeFrom;
                                Existdata.ClearanceShelfLifeTo = Temp.ClearanceShelfLifeTo;
                                Existdata.NonSellShelfLifeFrom = Temp.NonSellShelfLifeFrom;
                                Existdata.NonSellShelfLifeTo = Temp.NonSellShelfLifeTo;
                                Existdata.ModifiedBy = userid;
                                Existdata.ModifiedDate = DateTime.Now;
                                context.Entry(Existdata).State = EntityState.Modified;
                            }
                            if (context.Commit() > 0)
                            {
                                result.Status = true;
                                result.Message = " Approved ";
                            }
                        }
                    }
                    else if (Temp != null)
                    {
                        result.Status = true;
                        result.Message = "Request already in status: " + Temp.Status;
                    }
                    else
                    {
                        result.Status = false;
                        result.Message = " Not Found ";
                    }
                }
            }
            else
            {
                result.Status = false;
                result.Message = " You are not authorized ";

            }
            return result;
        }
        #endregion



        #region Auto-Configuration Page to update Discount on Store wise
        [Route("StoreList")]
        [HttpGet]
        public List<StoreNameListDc> GetStoreList(int Storeid)
        {
            List<StoreNameListDc> list = new List<StoreNameListDc>();

            if (Storeid > 0)
            {
                using (var context = new AuthContext())
                {
                    list = context.StoreDB.Where(x => x.Id == Storeid && x.IsDeleted == false && x.IsActive == true).Select(x => new StoreNameListDc { StoreId = x.Id, StoreName = x.Name, Discount = (double)x.Discount }).ToList();
                }
            }
            return list;

        }

        [Route("UpdateStoreList")]
        [HttpGet]
        public bool UpdateStoreList(long StoreId, double Discount)
        {
            bool result = false;

            var identity = User.Identity as ClaimsIdentity;
            int userid = 0;

            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

            if (StoreId > 0 && Discount > 0 && userid > 0)
            {
                using (var context = new AuthContext())
                {
                    var data = context.StoreDB.Where(x => x.Id == StoreId && x.IsDeleted == false && x.IsActive == true).FirstOrDefault();
                    if (data != null)
                    {
                        data.Discount = Discount;
                        data.ModifiedDate = DateTime.Now;
                        data.ModifiedBy = userid;
                        context.Entry(data).State = EntityState.Modified;
                        result = context.Commit() > 0;
                    }
                }
            }

            return result;
        }
        #endregion


        #region Get Clearance Order Picker List
        [Route("GetClearanceOrderPickerList")]
        [HttpPost]
        public ClearanceOrderPickerList GetClearanceOrderPickerList(SearchClearanceOrderPickerDC data)
        {
            ClearanceOrderPickerList res = new ClearanceOrderPickerList();
            if (data != null && data.WareHouseId > 0)
            {
                using (AuthContext context = new AuthContext())
                {
                    if (context.Database.Connection.State != ConnectionState.Open)
                        context.Database.Connection.Open();
                    var cmd = context.Database.Connection.CreateCommand();
                    cmd.CommandTimeout = 900;
                    cmd.CommandText = "[Clearance].[GetClearanceOrderPickerList]";
                    cmd.Parameters.Add(new SqlParameter("@WarehouseId", data.WareHouseId));
                    cmd.Parameters.Add(new SqlParameter("@FromDate", data.FromDate ?? (object)DBNull.Value));
                    cmd.Parameters.Add(new SqlParameter("@ToDate", data.ToDate ?? (object)DBNull.Value));
                    cmd.Parameters.Add(new SqlParameter("@ItemName", data.ItemName ?? (object)DBNull.Value));
                    cmd.Parameters.Add(new SqlParameter("@Status", data.PickerStatus ?? (object)DBNull.Value));
                    cmd.Parameters.Add(new SqlParameter("@Skip", data.skip));
                    cmd.Parameters.Add(new SqlParameter("@Take", data.take));

                    cmd.CommandType = System.Data.CommandType.StoredProcedure;

                    var reader = cmd.ExecuteReader();
                    var newdata = ((IObjectContextAdapter)context)
                    .ObjectContext
                    .Translate<ClearanceOrderPickerListDC>(reader).ToList();


                    res.ClearanceOrderPickerListDCs = newdata;
                    reader.NextResult();

                    while (reader.Read())
                    {
                        res.TotalRecords = Convert.ToInt32(reader["itemCount"]);
                    }


                }
            }
            return res;
        }

        [Route("GetExportClearanceOrderPickerList")]
        [HttpPost]
        public List<ClearanceOrderPickerListDC> GetExportClearanceOrderPickerList(SearchClearanceOrderPickerDC data)
        {
            List<ClearanceOrderPickerListDC> clearanceOrderPickerListDCs = new List<ClearanceOrderPickerListDC>();
            if (data != null && data.WareHouseId > 0)
            {
                using (AuthContext context = new AuthContext())
                {
                    if (context.Database.Connection.State != ConnectionState.Open)
                        context.Database.Connection.Open();
                    var cmd = context.Database.Connection.CreateCommand();
                    cmd.CommandTimeout = 900;
                    cmd.CommandText = "[Clearance].[GetClearanceOrderPickerListExport]";
                    cmd.Parameters.Add(new SqlParameter("@WarehouseId", data.WareHouseId));
                    cmd.Parameters.Add(new SqlParameter("@FromDate", data.FromDate ?? (object)DBNull.Value));
                    cmd.Parameters.Add(new SqlParameter("@ToDate", data.ToDate ?? (object)DBNull.Value));
                    cmd.Parameters.Add(new SqlParameter("@ItemName", data.ItemName ?? (object)DBNull.Value));
                    cmd.Parameters.Add(new SqlParameter("@Status", data.PickerStatus ?? (object)DBNull.Value));
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;

                    var reader = cmd.ExecuteReader();
                    clearanceOrderPickerListDCs = ((IObjectContextAdapter)context)
                    .ObjectContext
                    .Translate<ClearanceOrderPickerListDC>(reader).ToList();
                }
            }
            return clearanceOrderPickerListDCs;
        }
        #endregion


        #region Auto Job for ClearanceStock


        //[Route("UpdateClearanceAutoApprovedOrders")]
        //[HttpGet]
        //public async Task<ResponseMsg> UpdateClearanceAutoApprovedOrders()
        //{
        //    var identity = User.Identity as ClaimsIdentity;
        //    int userid = 0;

        //    if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
        //        userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

        //    ResponseMsg result = new ResponseMsg();
        //    long ClearanceStockOrderId = 0;
        //    List<long> clearanceStockOrderIds = new List<long>();

        //    TransactionOptions option = new TransactionOptions();
        //    option.IsolationLevel = System.Transactions.IsolationLevel.RepeatableRead;
        //    option.Timeout = TimeSpan.FromSeconds(120);
        //    using (var dbContextTransaction = new TransactionScope(TransactionScopeOption.RequiresNew, option))
        //    {
        //        using (var context = new AuthContext())
        //        {

        //            var ClearanceNonSellableOrderids = context.Database.SqlQuery<long>("EXEC [dbo].[UpdateClearanceAutoApprovedOrders]").ToList();
        //            var ClearanceNonSellableOrder = context.ClearanceNonSaleableMovementOrders.Where(x => ClearanceNonSellableOrderids.Contains(x.Id)).ToList();
        //            var ClearanceNonSellableOrderDetails = context.ClearanceNonSaleableMovementOrderDetails.Where(x => ClearanceNonSellableOrderids.Contains(x.ClearanceStockMovementOrderMasterId)).ToList();
        //            var WarehouseIds = ClearanceNonSellableOrder.Select(x => x.WarehouseId).Distinct().ToList();
        //            if (ClearanceNonSellableOrder != null && ClearanceNonSellableOrderDetails.Count > 0 && ClearanceNonSellableOrderids != null) // Code
        //            {
        //                StockTransactionHelper sthelper = new StockTransactionHelper();
        //                List<PhysicalStockUpdateRequestDc> manualStockUpdateDcList = new List<PhysicalStockUpdateRequestDc>();
        //                List<TransferStockDTONew> transferStockList = new List<TransferStockDTONew>();
        //                BatchMasterManager batchMasterManager = new BatchMasterManager();

        //                var StockBatchMasterIds = ClearanceNonSellableOrderDetails.Select(x => x.StockBatchMasterId).Distinct().ToList();
        //                var CurrentStockBatchMasters = context.StockBatchMasters.Where(x => StockBatchMasterIds.Contains(x.Id)).ToList();

        //                foreach (int id in WarehouseIds)
        //                {
        //                    var i = (from x in context.Warehouses where x.WarehouseId == id && x.active == true && x.Deleted == false select x).FirstOrDefault();
        //                    //(from x in context.Warehouses where x.WarehouseId == id &&  x.active==true  && x.Deleted== false  select x).SingleOrDefault();
        //                    if (i.IsStopCurrentStockTrans == false)
        //                    {
        //                        i.IsStopCurrentStockTrans = true;
        //                        context.Entry(i).State = EntityState.Modified;

        //                    }
        //                }
        //                if (context.Commit() > 0)
        //                {
        //                    dbContextTransaction.Complete();


        //                    foreach (var Data in ClearanceNonSellableOrder)
        //                    {

        //                        if (Data.OrderType == "CurrentToClearance")
        //                        {
        //                            foreach (var item in ClearanceNonSellableOrderDetails)
        //                            {
        //                                if (Data.Id == item.ClearanceStockMovementOrderMasterId)
        //                                {
        //                                    int DispatchedqTY = 0;
        //                                    int qty = CurrentStockBatchMasters.FirstOrDefault(x => x.Id == item.StockBatchMasterId && x.StockType == "C").Qty;
        //                                    if (qty > 0)
        //                                    {
        //                                        DispatchedqTY = item.Quantity < qty ? item.Quantity : qty;
        //                                        PhysicalStockUpdateRequestDc manualStockUpdateDc = new PhysicalStockUpdateRequestDc
        //                                        {
        //                                            ItemMultiMRPId = item.ItemMultiMRPId,
        //                                            Reason = "CurrentToClearancePlanned",
        //                                            StockTransferType = "ManualInventory",
        //                                            Qty = DispatchedqTY,
        //                                            WarehouseId = Data.WarehouseId,
        //                                            DestinationStockType = StockTypeTableNames.ClearancePlannedStocks,
        //                                            SourceStockType = StockTypeTableNames.CurrentStocks,
        //                                        };
        //                                        manualStockUpdateDcList.Add(manualStockUpdateDc);

        //                                        transferStockList.Add(new TransferStockDTONew
        //                                        {
        //                                            ItemMultiMRPId = item.ItemMultiMRPId,
        //                                            ItemMultiMRPIdTrans = item.ItemMultiMRPId,
        //                                            StockBatchMasterId = item.StockBatchMasterId,
        //                                            Qty = DispatchedqTY,
        //                                            WarehouseId = Data.WarehouseId,
        //                                            BatchMasterId = 0
        //                                        });
        //                                    }
        //                                    item.Quantity = DispatchedqTY;
        //                                    context.Entry(item).State = EntityState.Modified;

        //                                }

        //                            }
        //                        }
        //                        else if (Data.OrderType == "ClearanceToCurrent")
        //                        {
        //                            foreach (var item in ClearanceNonSellableOrderDetails)
        //                            {

        //                                if (Data.Id == item.ClearanceStockMovementOrderMasterId)
        //                                {
        //                                    int DispatchedqTY = 0;
        //                                    var qty = CurrentStockBatchMasters.FirstOrDefault(x => x.Id == item.StockBatchMasterId && x.StockType == "CL").Qty;
        //                                    if (qty > 0)
        //                                    {
        //                                        DispatchedqTY = item.Quantity < qty ? item.Quantity : qty;
        //                                        PhysicalStockUpdateRequestDc manualStockUpdateDc = new PhysicalStockUpdateRequestDc
        //                                        {
        //                                            ItemMultiMRPId = item.ItemMultiMRPId,
        //                                            Reason = "ClearanceToClearancePlanned",
        //                                            StockTransferType = "ManualInventory",
        //                                            Qty = DispatchedqTY,
        //                                            WarehouseId = Data.WarehouseId,
        //                                            DestinationStockType = StockTypeTableNames.ClearancePlannedStocks,
        //                                            SourceStockType = StockTypeTableNames.ClearanceStockNews,
        //                                        };
        //                                        manualStockUpdateDcList.Add(manualStockUpdateDc);
        //                                        transferStockList.Add(new TransferStockDTONew
        //                                        {
        //                                            ItemMultiMRPId = item.ItemMultiMRPId,
        //                                            ItemMultiMRPIdTrans = item.ItemMultiMRPId,
        //                                            StockBatchMasterId = item.StockBatchMasterId,
        //                                            Qty = DispatchedqTY,
        //                                            WarehouseId = Data.WarehouseId,
        //                                            BatchMasterId = 0
        //                                        });
        //                                    }
        //                                    item.Quantity = DispatchedqTY;
        //                                    context.Entry(item).State = EntityState.Modified;
        //                                }
        //                            }
        //                        }

        //                    }
        //                    foreach (var item in transferStockList)
        //                    {
        //                        bool isBatchSuccess = batchMasterManager.UpdateStockInSameBatch(item.StockBatchMasterId, context, userid, item.Qty);
        //                        if (!isBatchSuccess)
        //                        {
        //                            result.Status = false;
        //                            result.Message = " Issue in Batch stock. Please try after some time.";
        //                            return result;
        //                        }
        //                    }
        //                    foreach (var item in manualStockUpdateDcList)
        //                    {
        //                        bool isSuccess = sthelper.TransferBetweenPhysicalStocks(item, userid, context, dbContextTransaction);
        //                        if (!isSuccess)
        //                        {
        //                            result.Status = false;
        //                            result.Message = " Issue in stock. Please try after some time. ";
        //                            return result;
        //                        }
        //                    }

        //                    foreach (var data in ClearanceNonSellableOrder)
        //                    {
        //                        data.Status = "Approved";
        //                        data.ModifiedBy = 0;
        //                        data.ApprovedDate = DateTime.Now;
        //                        data.Approvedby = 0;
        //                        data.ModifiedDate = DateTime.Now;
        //                        context.Entry(data).State = EntityState.Modified;
        //                    }

        //                }
        //                else
        //                {
        //                    result.Status = false;
        //                    result.Message = "Not Commit ";
        //                    return result;
        //                }
        //                foreach (int id in WarehouseIds)
        //                {
        //                    var i = (from x in context.Warehouses
        //                             where x.WarehouseId == id && x.active == true && x.Deleted == false
        //                             select x).SingleOrDefault();
        //                    if (i.IsStopCurrentStockTrans == true)
        //                    {
        //                        i.IsStopCurrentStockTrans = false;
        //                        context.Entry(i).State = EntityState.Modified;

        //                    }
        //                }

        //                if (context.Commit() > 0)
        //                {
        //                    dbContextTransaction.Complete();
        //                    result.Status = true;
        //                    result.Message = " Order " + "Approved" + " successfully ";
        //                    return result;
        //                }
        //            }
        //            else
        //            {
        //                result.Status = false;
        //                result.Message = " something went wrong. ";
        //                return result;
        //            }

        //        }
        //    }
        //    return result;
        //}

        //[Route("ClearancePrepareItemForMultiWarehouse")]
        //[HttpGet]
        //public async Task<ResponseMsg> ClearancePrepareItemForMultiWarehouse()
        //{
        //    using (AuthContext context = new AuthContext())
        //    {
        //        ResponseMsg result = new ResponseMsg();
        //        var query = context.Database.SqlQuery<int>("EXEC [Clearance].[DuplicateInsertClearanceNonSaleablePrepareItem]").ToList();
        //        if(query!=null && query.Any())
        //        {
        //            result.Status = true;
        //            result.Message = "SucessFully";
        //            return result;
        //        }
        //        else
        //        {
        //            result.Status = false;
        //            result.Message = "Failed";
        //            return result;
        //        }
        //    }
        //}
        #endregion




    }

}
