using AgileObjects.AgileMapper;
using AngularJSAuthentication.API.Controllers.Base;
using AngularJSAuthentication.API.Helper;
using AngularJSAuthentication.DataContracts.Masters;
using AngularJSAuthentication.Model;
using Microsoft.Extensions.Logging;
using NLog;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Transactions;
using System.Web.Http;

namespace AngularJSAuthentication.API.Controllers.SarthiApp
{
    [RoutePrefix("api/IntTransSarthiApp")]
    public class InternalTransferSarthiAppController : BaseAuthController
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private static TimeZoneInfo INDIAN_ZONE = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
        DateTime indianTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE);

        #region Get List of Pending Pending Internal Transfer
        /// <summary>
        /// Get Pending Transfer Order
        /// </summary>
        /// <returns></returns>
        [AllowAnonymous]
        [Route("PendingIntTransfer/V1")]
        [HttpGet]
        public HttpResponseMessage PendingIntTransferV1(int WarehouseId)
        {
            List<TransferWHOrderMaster> transferOrder = new List<TransferWHOrderMaster>();

            string resultMessage = "";
            bool status = false;
            var _result = new List<TransferWHOrderMasterDC>();
            using (AuthContext db = new AuthContext())
            {
                string query = " select  a.TransferOrderId, a.CompanyId, a.WarehouseId,a.RequestToWarehouseId,d.CityName as RequestToWarehouseCityName," +
                               " d.WarehouseName as RequestToWarehouseName,a.CreationDate, a.Status," +
                               " b.WarehouseName,c.DisplayName as CreatedBy from[TransferWHOrderMasters] a" +
                               " left join Warehouses b on a.WarehouseId = b.WarehouseId " +
                               " left join Warehouses d on a.RequestToWarehouseId = d.WarehouseId " +
                               " left join People c on a.CreatedBy = c.PeopleID  " +
                               " where a.RequestToWarehouseId = " + WarehouseId + " and a.Status = 'Pending' and a.IsActivate = 1";

                _result = db.Database.SqlQuery<TransferWHOrderMasterDC>(query).ToList();

                //transferOrder = db.TransferWHOrderMasterDB.Where(x => x.WarehouseId == WarehouseId && x.Status == "Pending" && x.IsActivate == true).ToList();
                //_result = Mapper.Map(transferOrder).ToANew<List<TransferWHOrderMasterDC>>();

                if (_result != null && _result.Any())
                {
                    status = true; resultMessage = "Record found";
                }
                else
                { status = true; resultMessage = "No Record found"; }
            }
            var res = new
            {
                PendingTransferOrder = _result,
                status = status,
                Message = resultMessage
            };

            return Request.CreateResponse(HttpStatusCode.OK, res);
        }
        #endregion

        #region Get Internal Transfer Detail
        /// <summary>
        /// Get  Transfer Order Details
        /// </summary>
        /// <returns></returns>
        [AllowAnonymous]
        [Route("IntTransferDetail/V1")]
        [HttpGet]
        public HttpResponseMessage IntTransferDetailV1(int TransferOrderId)
        {
            List<TransferWHOrderDetails> transferOrderDetail = new List<TransferWHOrderDetails>();

            string resultMessage = "";
            bool status = false;
            var _result = new List<TransferWHOrderDetailsDC>();
            using (AuthContext db = new AuthContext())
            {
                transferOrderDetail = db.TransferWHOrderDetailsDB.Where(x => x.TransferOrderId == TransferOrderId).ToList();
                _result = Mapper.Map(transferOrderDetail).ToANew<List<TransferWHOrderDetailsDC>>();

                if (_result != null && _result.Any())
                {
                    status = true; resultMessage = "Record found";
                }
                else
                { status = true; resultMessage = "No Record found"; }
            }
            var res = new
            {
                TransferOrderDetails = _result,
                status = status,
                Message = resultMessage
            };

            return Request.CreateResponse(HttpStatusCode.OK, res);
        }
        #endregion

        #region Dispached&InprogressOrder
        [AllowAnonymous]
        [Route("DispachedTranferOrder/V1")]
        [HttpPost]
        public HttpResponseMessage DispachedTranferOrder(TransferWHOrderDispachedDC transferWHOrderDispached)
        {
            var Message = "";
            var identity = User.Identity as ClaimsIdentity;
            int compid = 0, userid = 0;
            userid = transferWHOrderDispached.CreatedById;

            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))
                compid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);

            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

            if (transferWHOrderDispached != null)
            {
                TransactionOptions option = new TransactionOptions();
                option.IsolationLevel = System.Transactions.IsolationLevel.RepeatableRead;
                option.Timeout = TimeSpan.FromSeconds(90);
                using (TransactionScope dbContextTransaction = new TransactionScope(TransactionScopeOption.Required, option))
                {
                    using (AuthContext db = new AuthContext())
                    {
                        bool status = false;
                        var peopledata = db.Peoples.Where(x => x.PeopleID == userid && x.Active == true).SingleOrDefault();
                        compid = peopledata.CompanyId;
                        bool Isdispatched = db.TransferWHOrderDispatchedMasterDB.Any(x => x.TransferOrderId == transferWHOrderDispached.TransferOrderId);
                        var IsRequest = db.TransferWHOrderMasterDB.FirstOrDefault(x => x.TransferOrderId == transferWHOrderDispached.TransferOrderId);
                        if (!Isdispatched && IsRequest.Status == "Pending" && peopledata != null)
                        {
                            TransferWHOrderMaster transferWHOrderMaster = db.TransferWHOrderMasterDB.Where(z => z.TransferOrderId == transferWHOrderDispached.TransferOrderId).SingleOrDefault();
                            List<TransferWHOrderDetails> transferWHOrderDetails = db.TransferWHOrderDetailsDB.Where(x => x.TransferOrderId == transferWHOrderDispached.TransferOrderId).ToList();

                            List<TransferWHOrderDispatchedDetail> AddTransferWHOrderDispatchedDetail = new List<TransferWHOrderDispatchedDetail>();
                            List<CurrentStockHistory> AddCurrentStockHistory = new List<CurrentStockHistory>();
                            List<CurrentStock> UpdateCurrentStock = new List<CurrentStock>();

                            var flag = false;
                            int ReqWid = Convert.ToInt32(transferWHOrderMaster.RequestToWarehouseId);//given
                            int? warehouseid = transferWHOrderMaster.WarehouseId;
                            var TransferWHOrderDet = transferWHOrderDetails.Where(c => c.TransferOrderId == transferWHOrderDispached.TransferOrderId).ToList();
                            var ItemMultiMRPIds = TransferWHOrderDet.Select(i => i.ItemMultiMRPId).ToList();
                            var itemNumbers = TransferWHOrderDet.Select(x => x.ItemNumber).ToList();
                            var Stock = db.DbCurrentStock.Where(x => itemNumbers.Contains(x.ItemNumber) && x.WarehouseId == ReqWid && ItemMultiMRPIds.Contains(x.ItemMultiMRPId)).ToList();
                            var Tom = db.TransferWHOrderMasterDB.Where(a => a.TransferOrderId == transferWHOrderDispached.TransferOrderId).SingleOrDefault();
                            var todList = db.TransferWHOrderDetailsDB.Where(x => x.TransferOrderId == Tom.TransferOrderId).ToList();
                            foreach (var o in transferWHOrderDispached.TransferWHOrderDetailss)
                            {

                                TransferWHOrderDetails transferOrderDetail = transferWHOrderDetails.Where(x => x.TransferOrderDetailId == o.TransferOrderDetailId).SingleOrDefault();
                                var IsExits = Stock.FirstOrDefault(x => x.ItemNumber == transferOrderDetail.ItemNumber && x.ItemMultiMRPId == transferOrderDetail.ItemMultiMRPId);

                                if (IsExits != null)
                                {
                                    if (transferOrderDetail.TotalQuantity > IsExits.CurrentInventory)
                                    {
                                        flag = true;
                                        Message = "Stock Not Exist for This Item: " + transferOrderDetail.itemname + " and Number is: " + transferOrderDetail.ItemNumber;
                                        break;
                                    }
                                }
                                else if (IsExits == null && transferOrderDetail.TotalQuantity == 0)
                                {
                                }
                                else
                                {
                                    flag = true;
                                    Message = "Stock Not Exist for This Item: " + transferOrderDetail.itemname + " and Number is: " + transferOrderDetail.ItemNumber;
                                    break;
                                }
                            }

                            if (flag == false)
                            {
                                Tom.Status = "InProgress";
                                Tom.UpdatedDate = indianTime;
                                Tom.userid = userid;
                                TransferOrderHistory transferOrder = new TransferOrderHistory();
                                transferOrder.TransferOrderId = Tom.TransferOrderId;
                                transferOrder.Status = Tom.Status;
                                transferOrder.userid = Tom.userid;
                                if (peopledata.DisplayName == null)
                                {
                                    transferOrder.username = peopledata.PeopleFirstName;
                                }
                                else
                                {
                                    transferOrder.username = peopledata.DisplayName;
                                }
                                transferOrder.CreatedDate = indianTime;
                                db.TransferOrderHistoryDB.Add(transferOrder);
                                TransferWHOrderDispatchedMaster tdm = new TransferWHOrderDispatchedMaster();
                                tdm.CompanyId = Tom.CompanyId;
                                tdm.RequestToWarehouseId = Tom.RequestToWarehouseId;
                                tdm.RequestToWarehouseName = Tom.RequestToWarehouseName;
                                tdm.WarehouseId = Tom.WarehouseId;
                                tdm.WarehouseName = Tom.WarehouseName;
                                tdm.TransferOrderId = Tom.TransferOrderId;
                                tdm.Status = Tom.Status;
                                tdm.UpdatedDate = indianTime;
                                tdm.CreationDate = indianTime;
                                tdm.Deleted = false;
                                tdm.VehicleNo = transferWHOrderDispached.vehicleNumber;
                                tdm.VehicleType = transferWHOrderDispached.vehicleType;
                                tdm.fread = transferWHOrderDispached.fread;
                                tdm.Type = "Sales";
                                tdm.EwaybillNumber = transferWHOrderDispached.EwaybillNumber;
                                db.TransferWHOrderDispatchedMasterDB.Add(tdm);
                                foreach (var data in transferWHOrderDispached.TransferWHOrderDetailss)
                                {
                                    TransferWHOrderDetails transferWHOrderDetails1 = transferWHOrderDetails.Where(x => x.TransferOrderDetailId == data.TransferOrderDetailId).SingleOrDefault();
                                    var tod = todList.FirstOrDefault(x => x.TransferOrderDetailId == data.TransferOrderDetailId);
                                    tod.Status = "InProgress";
                                    tod.UpdatedDate = indianTime;
                                    TransferWHOrderDispatchedDetail tdd = new TransferWHOrderDispatchedDetail();
                                    tdd.CompanyId = transferWHOrderDetails1.CompanyId;
                                    tdd.CreationDate = indianTime;
                                    tdd.Deleted = false;
                                    tdd.ItemMultiMRPId = transferWHOrderDetails1.ItemMultiMRPId;
                                    tdd.WarehouseId = transferWHOrderDetails1.WarehouseId;
                                    tdd.ItemNumber = transferWHOrderDetails1.ItemNumber;
                                    tdd.TotalTaxPercentage = transferWHOrderDetails1.TotalTaxPercentage;
                                    tdd.TransferOrderDetailId = data.TransferOrderDetailId;
                                    tdd.TransferOrderId = transferWHOrderDetails1.TransferOrderId;
                                    var POI = (data.DispatchedQuantity) * (tod.NPP);
                                    tdd.PriceofItem = Convert.ToDouble(POI);
                                    tdd.itemBaseName = transferWHOrderDetails1.itemBaseName;
                                    tdd.itemname = transferWHOrderDetails1.itemname;
                                    tdd.ItemHsn = transferWHOrderDetails1.ItemHsn;
                                    tdd.NPP = transferWHOrderDetails1.NPP;
                                    tdd.MRP = transferWHOrderDetails1.MRP;
                                    tdd.UnitofQuantity = transferWHOrderDetails1.UnitofQuantity;
                                    tdd.UOM = transferWHOrderDetails1.UOM;
                                    tdd.RequestToWarehouseId = transferWHOrderDetails1.RequestToWarehouseId;
                                    tdd.RequestToWarehouseName = transferWHOrderDetails1.RequestToWarehouseName;
                                    tdd.Status = "InProgress";
                                    tdd.TotalQuantity = data.DispatchedQuantity;
                                    tdd.UpdatedDate = indianTime;
                                    tdd.WarehouseName = transferWHOrderDetails1.WarehouseName;
                                    tdd.CreationDate = indianTime;
                                    tdd.Deleted = false;
                                    AddTransferWHOrderDispatchedDetail.Add(tdd);

                                }
                                if (AddTransferWHOrderDispatchedDetail != null && AddTransferWHOrderDispatchedDetail.Any())
                                {
                                    db.TransferWHOrderDispatchedDetailDB.AddRange(AddTransferWHOrderDispatchedDetail);

                                    if (db.Commit() > 0)
                                    {
                                        status = true;
                                        dbContextTransaction.Complete();
                                    }
                                    else
                                    {
                                        status = false;
                                        dbContextTransaction.Dispose();
                                    }
                                }
                            }
                            else
                            {
                                return Request.CreateResponse(HttpStatusCode.OK, Message);
                            }
                        }

                        if (status)
                        {
                            Message = "Successfuly send to checker side.";
                            return Request.CreateResponse(HttpStatusCode.OK, Message);
                        }
                        else
                        {
                            return Request.CreateResponse(HttpStatusCode.OK, Message);
                        }
                    }
                }
            }
            else {
                Message = "SOme think went wrong";
                return Request.CreateResponse(HttpStatusCode.OK, Message);
            }
            
        }

        #endregion

        #region SubmitTranferOrder
        [Route("SubmitTranferOrder")]
        [HttpPost]
        public HttpResponseMessage SubmitTranferOrder(TransferWHOrderSubmitDC transferWHOrderSubmitDCs)
        {
            using (AuthContext context = new AuthContext())
            {
                using (var dbContextTransaction = context.Database.BeginTransaction())
                {
                    int compid = 0, userid = 0;
                    userid = transferWHOrderSubmitDCs.CreatedById;
                    var peopledata = context.Peoples.Where(x => x.PeopleID == userid && x.Active == true).SingleOrDefault();
                    compid = peopledata.CompanyId;
                    if (peopledata != null && peopledata.PeopleID > 0 && transferWHOrderSubmitDCs.RequestFromWarehouseId > 0 && transferWHOrderSubmitDCs != null && transferWHOrderSubmitDCs.itemLists.Any() && transferWHOrderSubmitDCs.RequestToWarehouseId > 0)
                    {
                        int RequestFromWarehouseId = transferWHOrderSubmitDCs.RequestFromWarehouseId; // Request
                        int RequestToWarehouseId = transferWHOrderSubmitDCs.RequestToWarehouseId;//Request full filler

                        var RequestFromWarehouse = context.Warehouses.Where(p => p.Deleted == false && p.WarehouseId == RequestFromWarehouseId && p.IsKPP == false && p.active == true).SingleOrDefault();
                        var RequestToWarehouse = context.Warehouses.Where(p => p.Deleted == false && p.WarehouseId == RequestToWarehouseId && p.IsKPP == false && p.active == true).SingleOrDefault();

                        TransferWHOrderMaster TOM = new TransferWHOrderMaster();
                        TOM.CreationDate = indianTime;
                        TOM.WarehouseId = RequestFromWarehouse.WarehouseId;
                        TOM.CompanyId = compid;
                        TOM.WarehouseName = RequestFromWarehouse.WarehouseName + " (" + RequestFromWarehouse.CityName + ")";
                        TOM.Status = "Pending";
                        TOM.RequestToWarehouseId = RequestToWarehouse.WarehouseId;
                        TOM.RequestToWarehouseName = RequestToWarehouse.WarehouseName + " (" + RequestToWarehouse.CityName + ")";
                        TOM.IsActivate = true;
                        TOM.IsDeleted = false;
                        TOM.UpdatedDate = indianTime;
                        TOM.UpdateBy = "";
                        TOM.userid = userid;
                        context.TransferWHOrderMasterDB.Add(TOM);
                        context.Commit();
                        TransferOrderHistory transferOrder = new TransferOrderHistory();
                        transferOrder.TransferOrderId = TOM.TransferOrderId;
                        transferOrder.Status = TOM.Status;
                        transferOrder.userid = TOM.userid;
                        if (peopledata.DisplayName == null)
                        {
                            transferOrder.username = peopledata.PeopleFirstName;
                        }
                        else
                        {
                            transferOrder.username = peopledata.DisplayName;
                        }
                        transferOrder.CreatedDate = indianTime;
                        context.TransferOrderHistoryDB.Add(transferOrder);
                        context.Commit();

                        var StockIds = transferWHOrderSubmitDCs.itemLists.Select(x => x.StockId).ToList();
                        var CurrentstockList = context.DbCurrentStock.Where(z => z.Deleted == false && StockIds.Contains(z.StockId)).ToList();
                        var ItemNumberList = CurrentstockList.Select(x => x.ItemNumber).ToList();
                        var ItemList = context.itemMasters.Where(z => ItemNumberList.Contains(z.Number) && z.WarehouseId == RequestToWarehouse.WarehouseId).ToList();

                        List<TransferWHOrderDetails> AddTransferWHOrderDetails = new List<TransferWHOrderDetails>();

                        foreach (var tItem in transferWHOrderSubmitDCs.itemLists)
                        {
                            var stock = CurrentstockList.Where(z => z.StockId == tItem.StockId).FirstOrDefault();
                            var item = ItemList.Where(z => z.Number == stock.ItemNumber).FirstOrDefault();

                            TransferWHOrderDetails pd = new TransferWHOrderDetails();
                            pd.TransferOrderId = TOM.TransferOrderId;
                            pd.itemname = stock.itemname;
                            pd.ItemMultiMRPId = stock.ItemMultiMRPId;
                            pd.ItemNumber = stock.ItemNumber;
                            pd.TotalTaxPercentage = item.TotalTaxPercentage;
                            pd.TotalQuantity = tItem.Noofpics;
                            pd.PriceofItem = (pd.TotalQuantity) * (item.NetPurchasePrice);
                            pd.WarehouseId = RequestFromWarehouse.WarehouseId; //request from warehouse
                            pd.CreationDate = indianTime;
                            pd.Status = "Pending";
                            pd.RequestToWarehouseId = transferWHOrderSubmitDCs.RequestToWarehouseId;
                            pd.itemBaseName = stock.itemBaseName;
                            pd.ItemHsn = item.HSNCode;
                            pd.NPP = item.NetPurchasePrice;
                            pd.MRP = stock.MRP;
                            pd.UnitofQuantity = item.UnitofQuantity;
                            pd.UOM = item.UOM;
                            pd.CompanyId = item.CompanyId;
                            pd.WarehouseName = RequestFromWarehouse.WarehouseName;
                            pd.RequestToWarehouseName = RequestToWarehouse.WarehouseName;
                            pd.UpdatedDate = indianTime;
                            AddTransferWHOrderDetails.Add(pd);
                        }
                        if (AddTransferWHOrderDetails != null && AddTransferWHOrderDetails.Any())
                        {
                            context.TransferWHOrderDetailsDB.AddRange(AddTransferWHOrderDetails);
                            if (context.Commit() > 0)
                            {
                                dbContextTransaction.Commit();
                                return Request.CreateResponse(HttpStatusCode.OK, "Add Successfuly.");
                            }
                            else
                            {
                                dbContextTransaction.Rollback();
                                return Request.CreateResponse(HttpStatusCode.OK, "Request to warehouse are not exist.");
                            }
                        }
                    }
                    return Request.CreateResponse(HttpStatusCode.OK, "Request to warehouse are not exist.");
                }
            }
        }

        #endregion

        #region Revoke Order
        /// <summary>
        /// Revoke Order
        /// </summary>
        /// <param name="TransferOrderId"></param>
        /// <returns></returns>
        /// 
        [AllowAnonymous]
        [Route("RevokeOrder/V1")]
        [HttpPost]
        public HttpResponseMessage RevokeOrder(RevokePostDC revokePostDC)
        {
            var Message = "";
            var identity = User.Identity as ClaimsIdentity;
            int compid = 0, userid = revokePostDC.CreatedBy;

            using (AuthContext db = new AuthContext())
            {
                using (var dbContextTransaction = db.Database.BeginTransaction())
                {
                    var peopledata = db.Peoples.Where(x => x.PeopleID == userid && x.Active == true).SingleOrDefault();
                    compid = peopledata.CompanyId;
                    if (revokePostDC.TransferOrderId > 0)
                    {
                        var Ishere = db.TransferWHOrderMasterDB.FirstOrDefault(x => x.TransferOrderId == revokePostDC.TransferOrderId);
                        if (Ishere.Status == "Pending")
                        {
                            List<TransferWHOrderDetails> _TransferOrderDetail = db.TransferWHOrderDetailsDB.Where(a => a.TransferOrderId == revokePostDC.TransferOrderId).ToList();

                            TransferWHOrderMaster Tom = db.TransferWHOrderMasterDB.Where(a => a.TransferOrderId == revokePostDC.TransferOrderId).SingleOrDefault();
                            Tom.Status = "Revoke";
                            Tom.RevokeRession = revokePostDC.RejectRession;
                            Tom.UpdatedDate = indianTime;
                            Tom.userid = userid;

                            foreach (var data in _TransferOrderDetail)
                            {
                                TransferWHOrderDetails tod = _TransferOrderDetail.Where(a => a.TransferOrderDetailId == data.TransferOrderDetailId).SingleOrDefault();
                                tod.Status = "Revoke";
                                tod.UpdatedDate = indianTime;
                            }
                            if (db.Commit() > 0)
                            {
                                dbContextTransaction.Commit();
                                Message = "Revoke Successfully";
                                return Request.CreateResponse(HttpStatusCode.OK, Message);
                            }
                            else
                            {
                                dbContextTransaction.Rollback();
                                return Request.CreateResponse(HttpStatusCode.OK, "Some thing went wrong.");
                            }
                        }
                    }
                    Message = "Some thing Went Wrong";
                    return Request.CreateResponse(HttpStatusCode.OK, Message);
                }
            }
        }

        #endregion

        #region Get List of Inprogress Internal Transfer for showing checker side 
        /// <summary>
        /// Get Inprogress Transfer Order
        /// </summary>
        /// <returns></returns>
        [AllowAnonymous]
        [Route("InProgressOrderList/V1")]
        [HttpGet]
        public HttpResponseMessage InProgressOrderListV1(int WarehouseId)
        {
            List<TransferWHOrderMaster> transferOrder = new List<TransferWHOrderMaster>();

            string resultMessage = "";
            bool status = false;
            var _result = new List<TransferWHOrderMasterDispechedInprogresDC>();
            using (AuthContext db = new AuthContext())
            {
                string query = " select  a.TransferOrderId, a.CompanyId, a.WarehouseId,a.RequestToWarehouseId,d.CityName as RequestToWarehouseCityName," +
                               " d.WarehouseName as RequestToWarehouseName,a.CreationDate, a.Status," +
                               " b.WarehouseName,c.DisplayName as CreatedBy, f.username as DispechedInProgressBy, f.CreatedDate as DispechedInProgressDate  from[TransferWHOrderMasters] a" +
                               " left join Warehouses b on a.WarehouseId = b.WarehouseId " +
                               " left join Warehouses d on a.RequestToWarehouseId = d.WarehouseId " +
                               " left join People c on a.CreatedBy = c.PeopleID  " +
                               " left join TransferOrderHistories f on a.TransferOrderId = f.TransferOrderId and f.[Status] = 'InProgress'" +
                               " where a.RequestToWarehouseId = " + WarehouseId + " and a.Status = 'InProgress' and a.IsActivate = 1";

                _result = db.Database.SqlQuery<TransferWHOrderMasterDispechedInprogresDC>(query).ToList();

                if (_result != null && _result.Any())
                {
                    status = true; resultMessage = "Record found";
                }
                else
                { status = true; resultMessage = "No Record found"; }
            }
            var res = new
            {
                InProgressTransferOrder = _result,
                status = status,
                Message = resultMessage
            };

            return Request.CreateResponse(HttpStatusCode.OK, res);
        }
        #endregion

        #region Get Detail of Inprogress Internal Transfer for showing checker side 
        /// <summary>
        /// Get InProgress Transfer Order Details
        /// </summary>
        /// <returns></returns>
        [AllowAnonymous]
        [Route("InProgressOrderDetail/V1")]
        [HttpGet]
        public HttpResponseMessage InProgressOrderDetailV1(int TransferOrderId)
        {
            List<TransferWHOrderDetails> transferOrderDetail = new List<TransferWHOrderDetails>();

            string resultMessage = "";
            bool status = false;
            var _result = new List<TransferWHOrderDetailsDC>();
            using (AuthContext db = new AuthContext())
            {

                string query = "select *,b.TotalQuantity as DispatchQuantity from TransferWHOrderDetails a join TransferWHOrderDispatchedDetails b on " +
                    " b.TransferOrderDetailId = a.TransferOrderDetailId where a.TransferOrderId ="+ TransferOrderId;

                _result = db.Database.SqlQuery<TransferWHOrderDetailsDC>(query).ToList();

                //transferOrderDetail = db.TransferWHOrderDetailsDB.Where(x => x.TransferOrderId == TransferOrderId).ToList();
                //_result = Mapper.Map(transferOrderDetail).ToANew<List<TransferWHOrderDetailsDC>>();

                if (_result != null && _result.Any())
                {
                    status = true; resultMessage = "Record found";
                }
                else
                { status = true; resultMessage = "No Record found"; }
            }
            var res = new
            {
                TransferOrderDetails = _result,
                status = status,
                Message = resultMessage
            };

            return Request.CreateResponse(HttpStatusCode.OK, res);
        }
        #endregion

        #region ApproveOrder
        [AllowAnonymous]
        [Route("ApproveOrder/V1")]
        [HttpPost]
        public HttpResponseMessage ApproveOrderV1(ApprovedPostDC approvedPostDC)
        {
            var Message = "";
            int userid = 0;
            userid = approvedPostDC.CreatedById;
            if (approvedPostDC.TransferOrderId > 0)
            {
                TransactionOptions option = new TransactionOptions();
                option.IsolationLevel = System.Transactions.IsolationLevel.RepeatableRead;
                option.Timeout = TimeSpan.FromSeconds(90);
                using (TransactionScope dbContextTransaction = new TransactionScope(TransactionScopeOption.Required, option))
                {
                    using (AuthContext db = new AuthContext())
                    {
                        bool status = false;
                        var peopledata = db.Peoples.Where(x => x.PeopleID == userid && x.Active == true).SingleOrDefault();

                        bool Isdispatched = db.TransferWHOrderDispatchedMasterDB.Any(x => x.TransferOrderId == approvedPostDC.TransferOrderId);
                        var IsRequest = db.TransferWHOrderMasterDB.FirstOrDefault(x => x.TransferOrderId == approvedPostDC.TransferOrderId);
                        if (Isdispatched && IsRequest.Status == "InProgress" && peopledata != null)
                        {
                            List<TransferWHOrderDispatchedDetail> transferWHOrderDispatchedDetails = new List<TransferWHOrderDispatchedDetail>();
                            List<CurrentStockHistory> AddCurrentStockHistory = new List<CurrentStockHistory>();
                            List<CurrentStock> UpdateCurrentStock = new List<CurrentStock>();
                            var flag = false;

                            TransferWHOrderMaster transferWHOrderMaster = db.TransferWHOrderMasterDB.Where(a => a.TransferOrderId == approvedPostDC.TransferOrderId).SingleOrDefault();
                            TransferWHOrderDispatchedMaster transferWHOrderDispatchedMaster = db.TransferWHOrderDispatchedMasterDB.Where(a => a.TransferOrderId == approvedPostDC.TransferOrderId).SingleOrDefault();
                            List<TransferWHOrderDetails> transferWHOrderDetails = db.TransferWHOrderDetailsDB.Where(a => a.TransferOrderId == approvedPostDC.TransferOrderId).ToList();
                            List<TransferWHOrderDispatchedDetail> transferWHOrderDispatchedDetails1 = db.TransferWHOrderDispatchedDetailDB.Where(a => a.TransferOrderId == approvedPostDC.TransferOrderId).ToList();

                            int ReqWid = Convert.ToInt32(transferWHOrderMaster.RequestToWarehouseId);//given
                            int warehouseid = transferWHOrderDetails.Where(x => x.WarehouseId.HasValue).FirstOrDefault().WarehouseId.Value;//take
                            var TransferWHOrderDet = transferWHOrderDetails.Where(c => c.TransferOrderId == approvedPostDC.TransferOrderId).ToList();
                            var ItemMultiMRPIds = TransferWHOrderDet.Select(i => i.ItemMultiMRPId).ToList();
                            var itemNumbers = TransferWHOrderDet.Select(x => x.ItemNumber).ToList();
                            var Stock = db.DbCurrentStock.Where(x => itemNumbers.Contains(x.ItemNumber) && x.WarehouseId == ReqWid && ItemMultiMRPIds.Contains(x.ItemMultiMRPId)).ToList();
                            var Tom = db.TransferWHOrderMasterDB.Where(a => a.TransferOrderId == approvedPostDC.TransferOrderId).SingleOrDefault();
                            var todList = transferWHOrderDetails.Where(x => x.TransferOrderId == Tom.TransferOrderId).ToList();
                            foreach (var o in transferWHOrderDetails)
                            {
                                var IsExits = Stock.FirstOrDefault(x => x.ItemNumber == o.ItemNumber && x.ItemMultiMRPId == o.ItemMultiMRPId);

                                if (IsExits != null)
                                {
                                    if (o.TotalQuantity > IsExits.CurrentInventory)
                                    {
                                        flag = true;
                                        Message = "Stock Not Exist for This Item: " + o.itemname + " and Number is: " + o.ItemNumber;
                                        break;
                                    }
                                }
                                else if (IsExits == null && o.TotalQuantity == 0)
                                {
                                }
                                else
                                {
                                    flag = true;
                                    Message = "Stock Not Exist for This Item: " + o.itemname + " and Number is: " + o.ItemNumber;
                                    break;
                                }
                            }

                            if (flag == false)
                            {
                                Tom.Status = "Dispatched";
                                Tom.UpdatedDate = indianTime;
                                Tom.userid = userid;
                                TransferOrderHistory transferOrder = new TransferOrderHistory();
                                transferOrder.TransferOrderId = Tom.TransferOrderId;
                                transferOrder.Status = Tom.Status;
                                transferOrder.userid = Tom.userid;

                                if (peopledata.DisplayName == null)
                                {
                                    transferOrder.username = peopledata.PeopleFirstName;
                                }
                                else
                                {
                                    transferOrder.username = peopledata.DisplayName;
                                }
                                transferOrder.CreatedDate = indianTime;
                                db.TransferOrderHistoryDB.Add(transferOrder);

                                transferWHOrderDispatchedMaster.Status = "Dispatched";
                                foreach (var data in transferWHOrderDispatchedDetails1)
                                {
                                    var tod = todList.FirstOrDefault(x => x.TransferOrderDetailId == data.TransferOrderDetailId);
                                    tod.Status = "Dispatched";
                                    tod.UpdatedDate = indianTime;
                                    db.Entry(tod).State = EntityState.Modified;

                                    var todd = transferWHOrderDispatchedDetails1.FirstOrDefault(x => x.TransferOrderDispatchedDetailId == data.TransferOrderDispatchedDetailId);
                                    todd.Status = "Dispatched";
                                    todd.UpdatedDate = indianTime;
                                    db.Entry(todd).State = EntityState.Modified;

                                    var csd = Stock.Where(x => x.ItemNumber == data.ItemNumber && x.WarehouseId == data.RequestToWarehouseId && x.ItemMultiMRPId == data.ItemMultiMRPId).SingleOrDefault();
                                    if (csd != null && csd.CurrentInventory > 0)
                                    {
                                        //    CurrentStockHistory Oss = new CurrentStockHistory();
                                        //    Oss.StockId = csd.StockId;
                                        //    Oss.ItemNumber = csd.ItemNumber;
                                        //    Oss.itemname = csd.itemname;
                                        //    Oss.CurrentInventory = csd.CurrentInventory;
                                        //    Oss.OdOrPoId = data.TransferOrderId;
                                        //    Oss.InventoryOut = Convert.ToInt32(data.TotalQuantity);
                                        //    Oss.TotalInventory = Convert.ToInt32(csd.CurrentInventory - data.TotalQuantity);
                                        //    Oss.WarehouseName = csd.WarehouseName;
                                        //    Oss.Warehouseid = csd.WarehouseId;
                                        //    Oss.CompanyId = csd.CompanyId;
                                        //    Oss.userid = peopledata.PeopleID;
                                        //    Oss.UserName = peopledata.DisplayName;
                                        //    Oss.CreationDate = DateTime.Now;
                                        //    Oss.ManualReason = Tom.RequestToWarehouseName + " Transfer Order to " + Tom.WarehouseName;
                                        //    Oss.PriceofItem = (data.TotalQuantity) * (tdd.MRP);   //implement vinayak//
                                        //    Oss.ItemMultiMRPId = csd.ItemMultiMRPId;
                                        //    AddCurrentStockHistory.Add(Oss);

                                        //    csd.CurrentInventory = Convert.ToInt32(csd.CurrentInventory - data.TotalQuantity);
                                        //    UpdateCurrentStock.Add(csd);
                                    }
                                }
                                db.CurrentStockHistoryDb.AddRange(AddCurrentStockHistory);
                                foreach (var CurrentStock in UpdateCurrentStock)
                                {
                                    db.Entry(CurrentStock).State = EntityState.Modified;
                                }

                                if (db.Commit() > 0)
                                {
                                    #region virtual stock
                                    MultiStockHelper<OnWarehouseSarthiTransferDispatchDC> MultiStockHelpers = new MultiStockHelper<OnWarehouseSarthiTransferDispatchDC>();
                                    List<OnWarehouseSarthiTransferDispatchDC> TransferDispatchStockList = new List<OnWarehouseSarthiTransferDispatchDC>();
                                    foreach (var StockHit in transferWHOrderDispatchedDetails1.Where(x => x.TotalQuantity > 0))
                                    {
                                        TransferDispatchStockList.Add(new OnWarehouseSarthiTransferDispatchDC
                                        {
                                            SourceWarehouseId = StockHit.RequestToWarehouseId.GetValueOrDefault(),
                                            ItemMultiMRPId = StockHit.ItemMultiMRPId,
                                            TransferWHOrderDispatchedDetailsId = StockHit.TransferOrderDispatchedDetailId,
                                            TransferOrderId = StockHit.TransferOrderId,
                                            Qty = StockHit.TotalQuantity,
                                            UserId = peopledata.PeopleID,
                                            IsFreeStock = false,
                                            Reason = Tom.RequestToWarehouseName + " Transfer Order to " + Tom.WarehouseName
                                        });
                                    }

                                    if (TransferDispatchStockList.Any())
                                    {
                                        bool res = MultiStockHelpers.MakeEntry(TransferDispatchStockList, "Stock_OnWarehouseTransferDispatch", db, dbContextTransaction);
                                        if (!res)
                                        {
                                            status = false;
                                            dbContextTransaction.Dispose();

                                        }
                                        else
                                        {
                                            status = true;
                                            dbContextTransaction.Complete();
                                        }
                                    }
                                    #endregion
                                }
                            }
                        }
                        if (status)
                        {
                            // dbContextTransaction.Complete();
                            Message = "Dispatched Successfuly";
                            return Request.CreateResponse(HttpStatusCode.OK, Message);
                        }
                    }
                }
            }
            Message = "Something Went wrong";
            return Request.CreateResponse(HttpStatusCode.OK, Message);
        }
        #endregion

        #region RejectOrder
        [AllowAnonymous]
        [Route("RejectOrder/V1")]
        [HttpPost]
        public HttpResponseMessage RejectOrderV1(RejectePostDC rejectePostDC)
        {
            var Message = "";
            int userid = 0;
            userid = rejectePostDC.CreatedById;
            if (rejectePostDC.TransferOrderId > 0)
            {
                TransactionOptions option = new TransactionOptions();
                option.IsolationLevel = System.Transactions.IsolationLevel.RepeatableRead;
                option.Timeout = TimeSpan.FromSeconds(90);
                using (TransactionScope dbContextTransaction = new TransactionScope(TransactionScopeOption.Required, option))
                {
                    using (AuthContext db = new AuthContext())
                    {
                        bool status = false;
                        var peopledata = db.Peoples.Where(x => x.PeopleID == userid && x.Active == true).SingleOrDefault();

                        bool Isdispatched = db.TransferWHOrderDispatchedMasterDB.Any(x => x.TransferOrderId == rejectePostDC.TransferOrderId);
                        var IsRequest = db.TransferWHOrderMasterDB.FirstOrDefault(x => x.TransferOrderId == rejectePostDC.TransferOrderId);
                        if (Isdispatched && IsRequest.Status == "InProgress" && peopledata != null)
                        {
                            List<TransferWHOrderDispatchedDetail> transferWHOrderDispatchedDetails = new List<TransferWHOrderDispatchedDetail>();
                            List<CurrentStockHistory> AddCurrentStockHistory = new List<CurrentStockHistory>();
                            List<CurrentStock> UpdateCurrentStock = new List<CurrentStock>();
                            var flag = false;

                            TransferWHOrderMaster transferWHOrderMaster = db.TransferWHOrderMasterDB.Where(a => a.TransferOrderId == rejectePostDC.TransferOrderId).SingleOrDefault();
                            TransferWHOrderDispatchedMaster transferWHOrderDispatchedMaster = db.TransferWHOrderDispatchedMasterDB.Where(a => a.TransferOrderId == rejectePostDC.TransferOrderId).SingleOrDefault();
                            List<TransferWHOrderDetails> transferWHOrderDetails = db.TransferWHOrderDetailsDB.Where(a => a.TransferOrderId == rejectePostDC.TransferOrderId).ToList();
                            List<TransferWHOrderDispatchedDetail> transferWHOrderDispatchedDetails1 = db.TransferWHOrderDispatchedDetailDB.Where(a => a.TransferOrderId == rejectePostDC.TransferOrderId).ToList();

                            int ReqWid = Convert.ToInt32(transferWHOrderMaster.RequestToWarehouseId);//given
                            int warehouseid = transferWHOrderDetails.Where(x => x.WarehouseId.HasValue).FirstOrDefault().WarehouseId.Value;//take
                            var TransferWHOrderDet = transferWHOrderDetails.Where(c => c.TransferOrderId == rejectePostDC.TransferOrderId).ToList();

                            var Tom = db.TransferWHOrderMasterDB.Where(a => a.TransferOrderId == rejectePostDC.TransferOrderId).SingleOrDefault();
                            var todList = transferWHOrderDetails.Where(x => x.TransferOrderId == Tom.TransferOrderId).ToList();

                            if (flag == false)
                            {
                                Tom.Status = "Rejected";
                                Tom.UpdatedDate = indianTime;
                                Tom.userid = userid;

                                TransferOrderHistory transferOrder = new TransferOrderHistory();
                                transferOrder.TransferOrderId = Tom.TransferOrderId;
                                transferOrder.Status = Tom.Status;
                                transferOrder.userid = Tom.userid;

                                if (peopledata.DisplayName == null)
                                {
                                    transferOrder.username = peopledata.PeopleFirstName;
                                }
                                else
                                {
                                    transferOrder.username = peopledata.DisplayName;
                                }
                                transferOrder.CreatedDate = indianTime;
                                db.TransferOrderHistoryDB.Add(transferOrder);

                                transferWHOrderDispatchedMaster.Status = "Rejected";
                                foreach (var data in transferWHOrderDispatchedDetails1)
                                {
                                    var tod = todList.FirstOrDefault(x => x.TransferOrderDetailId == data.TransferOrderDetailId);
                                    tod.Status = "Rejected";
                                    tod.UpdatedDate = indianTime;
                                    db.Entry(tod).State = EntityState.Modified;

                                    var todd = transferWHOrderDispatchedDetails1.FirstOrDefault(x => x.TransferOrderDispatchedDetailId == data.TransferOrderDispatchedDetailId);
                                    todd.Status = "Rejected";
                                    todd.UpdatedDate = indianTime;
                                    db.Entry(todd).State = EntityState.Modified;

                                }
                                if (db.Commit() > 0)
                                {
                                    status = true;
                                    dbContextTransaction.Complete();
                                }
                                else
                                {
                                    status = false;
                                    dbContextTransaction.Dispose();
                                }
                            }
                        }
                        if (status)
                        {
                            Message = "Rejected Successfuly";
                            return Request.CreateResponse(HttpStatusCode.OK, Message);
                        }
                    }
                }
            }
            Message = "Something Went wrong";
            return Request.CreateResponse(HttpStatusCode.OK, Message);
        }
        #endregion

        /// * APIs for Receiver not accepte dispeched order Or receiver Reject Dispeched order.
        /// * Order return back to dispecher and checker approved or reject with ression.

        #region Get Rejected from Receiver Internal Transfer Order List

        /// <summary>
        /// Get Reject from Receiver side Transfer Order
        /// </summary>
        /// <returns></returns>
        [AllowAnonymous]
        [Route("RejectFromReceiverOrderList/V1")]
        [HttpGet]
        public HttpResponseMessage RejectFromReceiverOrders(int WarehouseId)
        {
            List<TransferWHOrderMaster> transferOrder = new List<TransferWHOrderMaster>();

            string resultMessage = "";
            bool status = false;
            var _result = new List<TransferWHOrderMasterRejectedFromRecDC>();
            using (AuthContext db = new AuthContext())
            {
                string query = " select  a.TransferOrderId, a.CompanyId, a.WarehouseId,a.RequestToWarehouseId,d.CityName as RequestToWarehouseCityName," +
                               " d.WarehouseName as RequestToWarehouseName,a.CreationDate, a.Status," +
                               " b.WarehouseName,c.DisplayName as CreatedBy, f.username as RejectedtfromRecBy, f.CreatedDate as RejectedtfromRecDate from[TransferWHOrderMasters] a" +
                               " left join Warehouses b on a.WarehouseId = b.WarehouseId " +
                               " left join Warehouses d on a.RequestToWarehouseId = d.WarehouseId " +
                               " left join People c on a.CreatedBy = c.PeopleID  " +
                               " left join TransferOrderHistories f on a.TransferOrderId = f.TransferOrderId and f.[Status] = 'Reject From Receiver' " +
                               " where a.RequestToWarehouseId = " + WarehouseId + " and a.Status = 'Reject From Receiver' and a.IsActivate = 1";

                _result = db.Database.SqlQuery<TransferWHOrderMasterRejectedFromRecDC>(query).ToList();

                //transferOrder = db.TransferWHOrderMasterDB.Where(x => x.WarehouseId == WarehouseId && x.Status == "Pending" && x.IsActivate == true).ToList();
                //_result = Mapper.Map(transferOrder).ToANew<List<TransferWHOrderMasterDC>>();

                if (_result != null && _result.Any())
                {
                    status = true; resultMessage = "Record found";
                }
                else
                { status = true; resultMessage = "No Record found"; }
            }
            var res = new
            {
                PendingTransferOrder = _result,
                status = status,
                Message = resultMessage
            };

            return Request.CreateResponse(HttpStatusCode.OK, res);
        }
        #endregion

        #region Approve Rejected order from Receiver side  WarehouseTransfer Order
        [Route("ApprovedRejectedRequest/V1")]
        [HttpPost]
        [AllowAnonymous]
        public HttpResponseMessage ApprovedRejectedRequest(ApprovedRejectedByReceiverOrderDC approvedByReceiverDC)
        {
            var identity = User.Identity as ClaimsIdentity;
            int userid = 0; userid = approvedByReceiverDC.CreatedById;
            var Message = "";

            bool IsUpdated = false;
            TransactionOptions option = new TransactionOptions();
            option.IsolationLevel = System.Transactions.IsolationLevel.RepeatableRead;
            option.Timeout = TimeSpan.FromSeconds(90);
            using (var dbContextTransaction = new TransactionScope(TransactionScopeOption.Required, option))
            {
                using (AuthContext db = new AuthContext())
                {
                    var peopledata = db.Peoples.Where(x => x.PeopleID == userid && x.Active == true).SingleOrDefault();

                    if (approvedByReceiverDC != null && peopledata != null)
                    {
                        TransferWHOrderMaster transferWHOrderMaster = db.TransferWHOrderMasterDB.Where(a => a.TransferOrderId == approvedByReceiverDC.TransferOrderId).SingleOrDefault();
                        TransferWHOrderDispatchedMaster transferWHOrderDispatchedMaster = db.TransferWHOrderDispatchedMasterDB.Where(a => a.TransferOrderId == approvedByReceiverDC.TransferOrderId).SingleOrDefault();
                        List<TransferWHOrderDetails> transferWHOrderDetails = db.TransferWHOrderDetailsDB.Where(a => a.TransferOrderId == approvedByReceiverDC.TransferOrderId).ToList();
                        List<TransferWHOrderDispatchedDetail> transferWHOrderDispatchedDetails1 = db.TransferWHOrderDispatchedDetailDB.Where(a => a.TransferOrderId == approvedByReceiverDC.TransferOrderId).ToList();

                        TransferWHOrderDetails Tod = transferWHOrderDetails.FirstOrDefault();
                        var Isdispatched = transferWHOrderDispatchedMaster;
                        if (transferWHOrderDispatchedMaster.Status == "Reject From Receiver")
                        {

                            TransferWHOrderMaster Tom = db.TransferWHOrderMasterDB.Where(a => a.TransferOrderId == transferWHOrderMaster.TransferOrderId).SingleOrDefault();
                            Tom.Status = "Canceled";
                            Tom.UpdatedDate = indianTime;
                            Tom.userid = userid;

                            Isdispatched.Status = "Canceled";
                            Isdispatched.Type = "Purchase";
                            Isdispatched.UpdatedDate = indianTime;

                            foreach (var data in transferWHOrderDispatchedDetails1)
                            {
                                TransferWHOrderDetails tod = transferWHOrderDetails.Where(a => a.TransferOrderDetailId == data.TransferOrderDetailId).SingleOrDefault();
                                tod.Status = "Canceled";
                                tod.UpdatedDate = indianTime;

                                TransferWHOrderDispatchedDetail tdd = transferWHOrderDispatchedDetails1.Where(a => a.TransferOrderDetailId == data.TransferOrderDetailId).SingleOrDefault(); ;
                                tdd.Status = "Canceled";
                                tdd.UpdatedDate = indianTime;

                                var csd = db.DbCurrentStock.Where(x => x.ItemNumber == tdd.ItemNumber && x.WarehouseId == data.WarehouseId && x.ItemMultiMRPId == tdd.ItemMultiMRPId).FirstOrDefault();
                                if (csd != null)
                                {

                                }
                                else
                                {
                                    CurrentStock NewStock = new CurrentStock();
                                    NewStock.CompanyId = data.CompanyId;
                                    NewStock.CreationDate = indianTime;
                                    NewStock.CurrentInventory = 0;//tdd.TotalQuantity;
                                    NewStock.Deleted = false;
                                    NewStock.ItemMultiMRPId = tdd.ItemMultiMRPId;
                                    NewStock.itemname = data.itemname;
                                    NewStock.ItemNumber = tdd.ItemNumber;
                                    NewStock.itemBaseName = data.itemBaseName;
                                    NewStock.UpdatedDate = indianTime;
                                    NewStock.WarehouseId = data.WarehouseId ?? 0;
                                    NewStock.WarehouseName = data.WarehouseName;
                                    NewStock.MRP = data.MRP;
                                    NewStock.UOM = data.UOM;
                                    NewStock.userid = peopledata.PeopleID;
                                    db.DbCurrentStock.Add(NewStock);
                                    db.Commit();
                                }
                            }

                            TransferOrderHistory transferOrder = new TransferOrderHistory();
                            transferOrder.TransferOrderId = Tom.TransferOrderId;
                            transferOrder.Status = Tom.Status;
                            transferOrder.userid = peopledata.PeopleID;
                            if (peopledata.DisplayName == null)
                            {
                                transferOrder.username = peopledata.PeopleFirstName;
                            }
                            else
                            {
                                transferOrder.username = peopledata.DisplayName;
                            }
                            transferOrder.CreatedDate = indianTime;
                            db.TransferOrderHistoryDB.Add(transferOrder);


                            if (db.Commit() > 0)
                            {
                                #region virtual stock delivered
                                MultiStockHelper<OnWarehouseSarthiTransferDispatchedRejectDC> MultiStockHelpers = new MultiStockHelper<OnWarehouseSarthiTransferDispatchedRejectDC>();
                                List<OnWarehouseSarthiTransferDispatchedRejectDC> TransferDispatchStockList = new List<OnWarehouseSarthiTransferDispatchedRejectDC>();
                                foreach (var StockHit in transferWHOrderDispatchedDetails1.Where(x => x.TotalQuantity > 0))
                                {
                                    TransferDispatchStockList.Add(new OnWarehouseSarthiTransferDispatchedRejectDC
                                    {
                                        SourceWarehouseId = StockHit.RequestToWarehouseId.GetValueOrDefault(),
                                        ItemMultiMRPId = StockHit.ItemMultiMRPId,
                                        TransferWHOrderDispatchedDetailsId = StockHit.TransferOrderDispatchedDetailId,
                                        TransferOrderId = StockHit.TransferOrderId,
                                        Qty = StockHit.TotalQuantity,
                                        UserId = peopledata.PeopleID,
                                        IsFreeStock = false,
                                        Reason = Tom.RequestToWarehouseName + " Transfer Order to " + Tom.WarehouseName
                                    });
                                }
                                if (TransferDispatchStockList.Any())
                                {
                                    bool res = MultiStockHelpers.MakeEntry(TransferDispatchStockList, "Stock_OnWarehouseTransferDispatchedReject", db, dbContextTransaction);
                                    if (!res)
                                    {
                                        IsUpdated = false;
                                        Message = "Some thing went wrong.";
                                        dbContextTransaction.Dispose();

                                    }
                                    else
                                    {
                                        IsUpdated = true;
                                        Message = "Add Successfuly.";
                                        dbContextTransaction.Complete();
                                    }
                                }
                                #endregion
                            }
                            else
                            {
                                Message = "Some thing went wrong.";
                            }
                        }
                        else
                        {
                            Message = "Data not Dispatched.";
                        }
                    }
                }
            }
            return Request.CreateResponse(HttpStatusCode.OK, Message);
        }
        #endregion

        #region Reject Receiver reject order 
        [AllowAnonymous]
        [Route("RejectReceiverRejectOrder/V1")]
        [HttpPost]
        public HttpResponseMessage RejectReceiverRejectRequest(RejectePostDC rejectePostDC)
        {
            var Message = "";
            int userid = 0;
            userid = rejectePostDC.CreatedById;
            if (rejectePostDC.TransferOrderId > 0)
            {
                TransactionOptions option = new TransactionOptions();
                option.IsolationLevel = System.Transactions.IsolationLevel.RepeatableRead;
                option.Timeout = TimeSpan.FromSeconds(90);
                using (TransactionScope dbContextTransaction = new TransactionScope(TransactionScopeOption.Required, option))
                {
                    using (AuthContext db = new AuthContext())
                    {
                        bool status = false;
                        var peopledata = db.Peoples.Where(x => x.PeopleID == userid && x.Active == true).SingleOrDefault();

                        bool Isdispatched = db.TransferWHOrderDispatchedMasterDB.Any(x => x.TransferOrderId == rejectePostDC.TransferOrderId);
                        var IsRequest = db.TransferWHOrderMasterDB.FirstOrDefault(x => x.TransferOrderId == rejectePostDC.TransferOrderId);
                        if (Isdispatched && IsRequest.Status == "Reject From Receiver" && peopledata != null)
                        {
                            List<TransferWHOrderDispatchedDetail> transferWHOrderDispatchedDetails = new List<TransferWHOrderDispatchedDetail>();
                            List<CurrentStockHistory> AddCurrentStockHistory = new List<CurrentStockHistory>();
                            List<CurrentStock> UpdateCurrentStock = new List<CurrentStock>();
                            var flag = false;

                            TransferWHOrderMaster transferWHOrderMaster = db.TransferWHOrderMasterDB.Where(a => a.TransferOrderId == rejectePostDC.TransferOrderId).SingleOrDefault();
                            TransferWHOrderDispatchedMaster transferWHOrderDispatchedMaster = db.TransferWHOrderDispatchedMasterDB.Where(a => a.TransferOrderId == rejectePostDC.TransferOrderId).SingleOrDefault();
                            List<TransferWHOrderDetails> transferWHOrderDetails = db.TransferWHOrderDetailsDB.Where(a => a.TransferOrderId == rejectePostDC.TransferOrderId).ToList();
                            List<TransferWHOrderDispatchedDetail> transferWHOrderDispatchedDetails1 = db.TransferWHOrderDispatchedDetailDB.Where(a => a.TransferOrderId == rejectePostDC.TransferOrderId).ToList();

                            int ReqWid = Convert.ToInt32(transferWHOrderMaster.RequestToWarehouseId);//given
                            int warehouseid = transferWHOrderDetails.Where(x => x.WarehouseId.HasValue).FirstOrDefault().WarehouseId.Value;//take
                            var TransferWHOrderDet = transferWHOrderDetails.Where(c => c.TransferOrderId == rejectePostDC.TransferOrderId).ToList();

                            var Tom = db.TransferWHOrderMasterDB.Where(a => a.TransferOrderId == rejectePostDC.TransferOrderId).SingleOrDefault();
                            var todList = transferWHOrderDetails.Where(x => x.TransferOrderId == Tom.TransferOrderId).ToList();

                            if (flag == false)
                            {
                                Tom.Status = "Rejected";
                                Tom.UpdatedDate = indianTime;
                                Tom.userid = userid;
                                Tom.CheckerRejectRession = rejectePostDC.RejectRession;
                                TransferOrderHistory transferOrder = new TransferOrderHistory();
                                transferOrder.TransferOrderId = Tom.TransferOrderId;
                                transferOrder.Status = Tom.Status;
                                transferOrder.userid = Tom.userid;

                                if (peopledata.DisplayName == null)
                                {
                                    transferOrder.username = peopledata.PeopleFirstName;
                                }
                                else
                                {
                                    transferOrder.username = peopledata.DisplayName;
                                }
                                transferOrder.CreatedDate = indianTime;
                                db.TransferOrderHistoryDB.Add(transferOrder);

                                transferWHOrderDispatchedMaster.Status = "Rejected";
                                foreach (var data in transferWHOrderDispatchedDetails1)
                                {
                                    var tod = todList.FirstOrDefault(x => x.TransferOrderDetailId == data.TransferOrderDetailId);
                                    tod.Status = "Rejected";
                                    tod.UpdatedDate = indianTime;
                                    db.Entry(tod).State = EntityState.Modified;

                                    var todd = transferWHOrderDispatchedDetails1.FirstOrDefault(x => x.TransferOrderDispatchedDetailId == data.TransferOrderDispatchedDetailId);
                                    todd.Status = "Rejected";
                                    todd.UpdatedDate = indianTime;
                                    db.Entry(todd).State = EntityState.Modified;

                                }
                                if (db.Commit() > 0)
                                {
                                    status = true;
                                    dbContextTransaction.Complete();
                                }
                                else
                                {
                                    status = false;
                                    dbContextTransaction.Dispose();
                                }
                            }
                        }
                        if (status)
                        {
                            Message = "Rejected Successfuly";
                            return Request.CreateResponse(HttpStatusCode.OK, Message);
                        }
                    }
                }
            }
            Message = "Something Went wrong";
            return Request.CreateResponse(HttpStatusCode.OK, Message);
        }
        #endregion

        /// * End

        /// * 21/07/2020 Receiver side code  
        /// * Receiver side code  
        /// * APIs for sarthi app internal transfer order received.

        #region Get List of Dispeched Internal Transfer Order
        /// <summary>
        /// Get Pending Transfer Order
        /// </summary>
        /// <returns></returns>
        [AllowAnonymous]
        [Route("DispechedTransferOrder/V1")]
        [HttpGet]
        public HttpResponseMessage DispechedTransferOrderV1(int WarehouseId)
        {
            List<TransferWHOrderMaster> transferOrder = new List<TransferWHOrderMaster>();

            string resultMessage = "";
            bool status = false;
            var _result = new List<TransferWHOrderMasterDispatchedApprovedDC>();
            using (AuthContext db = new AuthContext())
            {
                string query = " select  a.TransferOrderId, a.CompanyId, a.WarehouseId,a.RequestToWarehouseId,d.CityName as RequestToWarehouseCityName," +
                               " d.WarehouseName as RequestToWarehouseName,a.CreationDate, a.Status," +
                               " b.WarehouseName,c.DisplayName as CreatedBy , f.username as DispatchedApprovedBy, f.CreatedDate as DispatchedApprovedDate from[TransferWHOrderMasters] a" +
                               " left join Warehouses b on a.WarehouseId = b.WarehouseId " +
                               " left join Warehouses d on a.RequestToWarehouseId = d.WarehouseId " +
                               " left join People c on a.CreatedBy = c.PeopleID  " +
                               " left join TransferOrderHistories f on a.TransferOrderId = f.TransferOrderId and f.[Status] = 'Dispatched'" +
                               " where a.WarehouseId = " + WarehouseId + " and a.Status = 'Dispatched' and a.IsActivate = 1";

                _result = db.Database.SqlQuery<TransferWHOrderMasterDispatchedApprovedDC>(query).ToList();

                if (_result != null && _result.Any())
                {
                    status = true; resultMessage = "Record found";
                }
                else
                { status = true; resultMessage = "No Record found"; }
            }
            var res = new
            {
                PendingTransferOrder = _result,
                status = status,
                Message = resultMessage
            };

            return Request.CreateResponse(HttpStatusCode.OK, res);
        }
        #endregion
         
        #region Get Internal Transfer Dispeched Order Detail
        /// <summary>
        /// Get  Dispeched Order Detail
        /// </summary>
        /// <returns></returns>
        [AllowAnonymous]
        [Route("DispechedOrderDetail/V1")]
        [HttpGet]
        public HttpResponseMessage DispechedOrderDetailV1(int TransferOrderId)
        {
            List<TransferWHOrderDetails> transferOrderDetail = new List<TransferWHOrderDetails>();

            string resultMessage = "";
            bool status = false;
            var _result = new List<TransferWHOrderDetailsDC>();
            using (AuthContext db = new AuthContext())
            {
                string query = "select *,b.TotalQuantity as DispatchQuantity from TransferWHOrderDetails a join TransferWHOrderDispatchedDetails b on " +
                    " b.TransferOrderDetailId = a.TransferOrderDetailId where a.TransferOrderId =" + TransferOrderId;

                _result = db.Database.SqlQuery<TransferWHOrderDetailsDC>(query).ToList();

                //transferOrderDetail = db.TransferWHOrderDetailsDB.Where(x => x.TransferOrderId == TransferOrderId).ToList();
                //_result = Mapper.Map(transferOrderDetail).ToANew<List<TransferWHOrderDetailsDC>>();

                if (_result != null && _result.Any())
                {
                    status = true; resultMessage = "Record found";
                }
                else
                { status = true; resultMessage = "No Record found"; }
            }
            var res = new
            {
                TransferOrderDetails = _result,
                status = status,
                Message = resultMessage
            };

            return Request.CreateResponse(HttpStatusCode.OK, res);
        }
        #endregion

        #region Recceive dispached Transfer Order from receiver side  
        [Route("ReceiveByReceiver/V1")]
        [HttpPost]
        [AllowAnonymous]
        public HttpResponseMessage ReceiveByReceiver(TransferOrderReceiveDC transferOrderReceiveDC)
        {
            var identity = User.Identity as ClaimsIdentity;
            int userid = 0; userid = transferOrderReceiveDC.CreatedById;
            var Message = "";

            bool IsUpdated = false;
            TransactionOptions option = new TransactionOptions();
            option.IsolationLevel = System.Transactions.IsolationLevel.RepeatableRead;
            option.Timeout = TimeSpan.FromSeconds(90);
            using (var dbContextTransaction = new TransactionScope(TransactionScopeOption.Required, option))
            {
                using (AuthContext db = new AuthContext())
                {
                    var peopledata = db.Peoples.Where(x => x.PeopleID == userid && x.Active == true).SingleOrDefault();

                    if (transferOrderReceiveDC != null && peopledata != null)
                    {
                        TransferWHOrderMaster transferWHOrderMaster = db.TransferWHOrderMasterDB.Where(a => a.TransferOrderId == transferOrderReceiveDC.TransferOrderId).SingleOrDefault();
                        TransferWHOrderDispatchedMaster transferWHOrderDispatchedMaster = db.TransferWHOrderDispatchedMasterDB.Where(a => a.TransferOrderId == transferOrderReceiveDC.TransferOrderId).SingleOrDefault();
                        List<TransferWHOrderDetails> transferWHOrderDetails = db.TransferWHOrderDetailsDB.Where(a => a.TransferOrderId == transferOrderReceiveDC.TransferOrderId).ToList();
                        List<TransferWHOrderDispatchedDetail> transferWHOrderDispatchedDetails1 = db.TransferWHOrderDispatchedDetailDB.Where(a => a.TransferOrderId == transferOrderReceiveDC.TransferOrderId).ToList();

                        TransferWHOrderDetails Tod = transferWHOrderDetails.FirstOrDefault();
                        var Isdispatched = transferWHOrderDispatchedMaster;
                        if (transferWHOrderDispatchedMaster.Status == "Dispatched")
                        {

                            TransferWHOrderMaster Tom = db.TransferWHOrderMasterDB.Where(a => a.TransferOrderId == transferWHOrderMaster.TransferOrderId).SingleOrDefault();
                            Tom.Status = "Delivery InProgress";
                            Tom.UpdatedDate = indianTime;
                            Tom.userid = userid;

                            Isdispatched.Status = "Delivery InProgress";
                            Isdispatched.Type = "Purchase";
                            Isdispatched.UpdatedDate = indianTime;

                            foreach (var data in transferOrderReceiveDC.transferOrderReceiveDetailDCs)
                            {
                                TransferWHOrderDetails tod = transferWHOrderDetails.Where(a => a.TransferOrderDetailId == data.TransferOrderDetailId).SingleOrDefault();
                                tod.Status = "Delivery InProgress";
                                tod.UpdatedDate = indianTime;

                                TransferWHOrderDispatchedDetail tdd = transferWHOrderDispatchedDetails1.Where(a => a.TransferOrderDetailId == data.TransferOrderDetailId).SingleOrDefault(); ;
                                tdd.Status = "Delivery InProgress";
                                tdd.ReceiveQuantity = data.ReceiveQuantity;
                                tdd.DamageQuantity = data.DamageQuantity;
                                tdd.ExpiryQuantity = data.ExpiryQuantity;
                                tdd.UpdatedDate = indianTime;
                            }

                            TransferOrderHistory transferOrder = new TransferOrderHistory();
                            transferOrder.TransferOrderId = Tom.TransferOrderId;
                            transferOrder.Status = Tom.Status;
                            transferOrder.userid = peopledata.PeopleID;
                            if (peopledata.DisplayName == null)
                            {
                                transferOrder.username = peopledata.PeopleFirstName;
                            }
                            else
                            {
                                transferOrder.username = peopledata.DisplayName;
                            }
                            transferOrder.CreatedDate = indianTime;
                            db.TransferOrderHistoryDB.Add(transferOrder);


                            if (db.Commit() > 0)
                            {
                                IsUpdated = true;
                                Message = "Delivery data send to checker side.";
                                dbContextTransaction.Complete();

                            }
                            else
                            {
                                IsUpdated = false;
                                Message = "Some thing went wrong.";
                                dbContextTransaction.Dispose();
                            }
                        }
                        else
                        {
                            Message = "Delivery data not send to checker side.";
                        }
                    }
                }
            }
            return Request.CreateResponse(HttpStatusCode.OK, Message);
        }

        #endregion

        #region Get List of Deleivery Inprogress Internal Transfer for showing checker side 
        /// <summary>
        /// Get Deleivery Inprogress Transfer Order
        /// </summary>
        /// <returns></returns>
        [AllowAnonymous]
        [Route("DeleiveryInProgressOrderList/V1")]
        [HttpGet]
        public HttpResponseMessage DeleiveryInProgressOrderListV1(int WarehouseId)
        {
            List<TransferWHOrderMaster> transferOrder = new List<TransferWHOrderMaster>();

            string resultMessage = "";
            bool status = false;
            var _result = new List<TransferWHOrderMasterDeleiveryInprogresDC>();
            using (AuthContext db = new AuthContext())
            {
                string query = " select  a.TransferOrderId, a.CompanyId, a.WarehouseId,a.RequestToWarehouseId,d.CityName as RequestToWarehouseCityName," +
                               " d.WarehouseName as RequestToWarehouseName,a.CreationDate, a.Status," +
                               " b.WarehouseName,c.DisplayName as CreatedBy, f.username as DeleiveryInProgressBy, f.CreatedDate as DeleiveryInProgressDate  from[TransferWHOrderMasters] a" +
                               " left join Warehouses b on a.WarehouseId = b.WarehouseId " +
                               " left join Warehouses d on a.RequestToWarehouseId = d.WarehouseId " +
                               " left join People c on a.CreatedBy = c.PeopleID  " +
                               " left join TransferOrderHistories f on a.TransferOrderId = f.TransferOrderId and f.[Status] = 'Delivery InProgress'" +
                               " where a.WarehouseId = " + WarehouseId + " and a.Status = 'Delivery InProgress' and a.IsActivate = 1";

                _result = db.Database.SqlQuery<TransferWHOrderMasterDeleiveryInprogresDC>(query).ToList();

                if (_result != null && _result.Any())
                {
                    status = true; resultMessage = "Record found";
                }
                else
                { status = true; resultMessage = "No Record found"; }
            }
            var res = new
            {
                InProgressTransferOrder = _result,
                status = status,
                Message = resultMessage
            };

            return Request.CreateResponse(HttpStatusCode.OK, res);
        }
        #endregion

        #region Get Internal Transfer Dispached Detail
        /// <summary>
        /// Get  Transfer Order Details
        /// </summary>
        /// <returns></returns>
        [AllowAnonymous]
        [Route("DeleiveryInProgressTransferDispachedDetail/V1")]
        [HttpGet]
        public HttpResponseMessage DeleiveryInProgressTransferDetailV1(int TransferOrderId)
        {
            List<TransferWHOrderDispatchedDetail> transferOrderDetail = new List<TransferWHOrderDispatchedDetail>();

            string resultMessage = "";
            bool status = false;
            var _result = new List<TransferWHOrderDispachedDetailsGetDC>();
            using (AuthContext db = new AuthContext())
            {
                string query = "select *,b.TotalQuantity as DispatchQuantity,b.ReceiveQuantity from TransferWHOrderDetails a join TransferWHOrderDispatchedDetails b on " +
                               "b.TransferOrderDetailId = a.TransferOrderDetailId where a.TransferOrderId =" + TransferOrderId;

                _result = db.Database.SqlQuery<TransferWHOrderDispachedDetailsGetDC>(query).ToList();

                //transferOrderDetail = db.TransferWHOrderDispatchedDetailDB.Where(x => x.TransferOrderId == TransferOrderId).ToList();
                //_result = Mapper.Map(transferOrderDetail).ToANew<List<TransferWHOrderDispachedDetailsGetDC>>();

                if (_result != null && _result.Any())
                {
                    status = true; resultMessage = "Record found";
                }
                else
                { status = true; resultMessage = "No Record found"; }
            }
            var res = new
            {
                TransferOrderDispatchedDetails = _result,
                status = status,
                Message = resultMessage
            };

            return Request.CreateResponse(HttpStatusCode.OK, res);
        }
        #endregion

        #region Reject from receiver for WarehouseTransfer Order
        /// <summary>
        /// Reject Option for WarehouseTransfer Order
        /// </summary>
        /// <param name="TOdata"></param>
        /// <returns></returns>
        [Authorize]
        [Route("CancelByReceiver/V1")]
        [HttpPost]
        [AllowAnonymous]
        public HttpResponseMessage CancelOrder(RejecteByReceiverDC rejecteByReceiverDC)
        {
            var Message = ""; bool status = false;
            int userid = rejecteByReceiverDC.CreatedById;
            TransactionOptions option = new TransactionOptions();
            option.IsolationLevel = System.Transactions.IsolationLevel.RepeatableRead;
            option.Timeout = TimeSpan.FromSeconds(90);
            using (var dbContextTransaction = new TransactionScope(TransactionScopeOption.Required, option))
            {
                using (AuthContext db = new AuthContext())
                {
                    TransferWHOrderMaster transferWHOrderMaster = db.TransferWHOrderMasterDB.Where(a => a.TransferOrderId == rejecteByReceiverDC.TransferOrderId).SingleOrDefault();
                    TransferWHOrderDispatchedMaster transferWHOrderDispatchedMaster = db.TransferWHOrderDispatchedMasterDB.Where(a => a.TransferOrderId == rejecteByReceiverDC.TransferOrderId).SingleOrDefault();
                    List<TransferWHOrderDetails> transferWHOrderDetails = db.TransferWHOrderDetailsDB.Where(a => a.TransferOrderId == rejecteByReceiverDC.TransferOrderId).ToList();
                    List<TransferWHOrderDispatchedDetail> transferWHOrderDispatchedDetails1 = db.TransferWHOrderDispatchedDetailDB.Where(a => a.TransferOrderId == rejecteByReceiverDC.TransferOrderId).ToList();

                    var peopledata = db.Peoples.Where(x => x.PeopleID == userid && x.Active == true).SingleOrDefault();

                    if (rejecteByReceiverDC != null && peopledata != null)
                    {
                        var Isdispatched = db.TransferWHOrderDispatchedMasterDB.FirstOrDefault(x => x.TransferOrderId == transferWHOrderMaster.TransferOrderId);
                        if (Isdispatched.Status == "Delivery InProgress")
                        {
                            List<CurrentStockHistory> AddCurrentStockHistory = new List<CurrentStockHistory>();
                            List<CurrentStock> UpdateCurrentStock = new List<CurrentStock>();
                            TransferWHOrderDispatchedMaster Tdm = db.TransferWHOrderDispatchedMasterDB.Where(a => a.TransferOrderId == transferWHOrderMaster.TransferOrderId).SingleOrDefault();
                            if (Tdm != null)
                            {
                                Tdm.Status = "Reject From Receiver";
                                Tdm.UpdatedDate = indianTime;

                                TransferWHOrderMaster Tom = db.TransferWHOrderMasterDB.Where(a => a.TransferOrderId == transferWHOrderMaster.TransferOrderId).SingleOrDefault();
                                Tom.Status = "Reject From Receiver";
                                Tom.UpdatedDate = indianTime;
                                Tom.userid = userid;

                                foreach (var data in transferWHOrderDispatchedDetails1)
                                {
                                    TransferWHOrderDetails tod = transferWHOrderDetails.Where(a => a.TransferOrderDetailId == data.TransferOrderDetailId).SingleOrDefault();
                                    tod.Status = "Reject From Receiver";
                                    tod.UpdatedDate = indianTime;

                                    TransferWHOrderDispatchedDetail tdd = transferWHOrderDispatchedDetails1.Where(a => a.TransferOrderDetailId == data.TransferOrderDetailId).SingleOrDefault(); ;
                                    tdd.Status = "Reject From Receiver";
                                    tdd.UpdatedDate = indianTime;
                                }

                                TransferOrderHistory transferOrder = new TransferOrderHistory();
                                transferOrder.TransferOrderId = Tom.TransferOrderId;
                                transferOrder.Status = Tom.Status;
                                transferOrder.userid = peopledata.PeopleID;
                                if (peopledata.DisplayName == null)
                                {
                                    transferOrder.username = peopledata.PeopleFirstName;
                                }
                                else
                                {
                                    transferOrder.username = peopledata.DisplayName;
                                }
                                transferOrder.CreatedDate = indianTime;
                                db.TransferOrderHistoryDB.Add(transferOrder);

                                if (db.Commit() > 0)
                                {
                                    dbContextTransaction.Complete();
                                    status = true;
                                    Message = "Reject Successfully";
                                }
                                else
                                {
                                    status = false;
                                    dbContextTransaction.Dispose();
                                    Message = "Some thing went wrong";
                                }
                            }
                        }
                    }
                    if (status)
                    {
                        Message = "Reject Successfully";

                    }
                    else { Message = "Some thing went wrong"; }
                    return Request.CreateResponse(HttpStatusCode.OK, Message);
                }
            }
        }
        #endregion

        #region Approve from receiver for WarehouseTransfer Order
        [Route("ApprovedByReceiver/V1")]
        [HttpPost]
        [AllowAnonymous]
        public HttpResponseMessage ApprovedByReceiver(ApprovedByReceiverDC approvedByReceiverDC)
        {
            var identity = User.Identity as ClaimsIdentity;
            int userid = 0; userid = approvedByReceiverDC.CreatedById;
            var Message = "";

            bool IsUpdated = false;
            TransactionOptions option = new TransactionOptions();
            option.IsolationLevel = System.Transactions.IsolationLevel.RepeatableRead;
            option.Timeout = TimeSpan.FromSeconds(90);
            using (var dbContextTransaction = new TransactionScope(TransactionScopeOption.Required, option))
            {
                using (AuthContext db = new AuthContext())
                {
                    var peopledata = db.Peoples.Where(x => x.PeopleID == userid && x.Active == true).SingleOrDefault();

                    if (approvedByReceiverDC != null && peopledata != null)
                    {
                        TransferWHOrderMaster transferWHOrderMaster = db.TransferWHOrderMasterDB.Where(a => a.TransferOrderId == approvedByReceiverDC.TransferOrderId).SingleOrDefault();
                        TransferWHOrderDispatchedMaster transferWHOrderDispatchedMaster = db.TransferWHOrderDispatchedMasterDB.Where(a => a.TransferOrderId == approvedByReceiverDC.TransferOrderId).SingleOrDefault();
                        List<TransferWHOrderDetails> transferWHOrderDetails = db.TransferWHOrderDetailsDB.Where(a => a.TransferOrderId == approvedByReceiverDC.TransferOrderId).ToList();
                        List<TransferWHOrderDispatchedDetail> transferWHOrderDispatchedDetails1 = db.TransferWHOrderDispatchedDetailDB.Where(a => a.TransferOrderId == approvedByReceiverDC.TransferOrderId).ToList();

                        TransferWHOrderDetails Tod = transferWHOrderDetails.FirstOrDefault();
                        var Isdispatched = transferWHOrderDispatchedMaster;
                        if (transferWHOrderDispatchedMaster.Status == "Delivery InProgress")
                        {

                            TransferWHOrderMaster Tom = db.TransferWHOrderMasterDB.Where(a => a.TransferOrderId == transferWHOrderMaster.TransferOrderId).SingleOrDefault();
                            Tom.Status = "Delivered";
                            Tom.UpdatedDate = indianTime;
                            Tom.userid = userid;

                            Isdispatched.Status = "Delivered";
                            Isdispatched.Type = "Purchase";
                            Isdispatched.UpdatedDate = indianTime;

                            foreach (var data in transferWHOrderDispatchedDetails1)
                            {
                                TransferWHOrderDetails tod = transferWHOrderDetails.Where(a => a.TransferOrderDetailId == data.TransferOrderDetailId).SingleOrDefault();
                                tod.Status = "Delivered";
                                tod.UpdatedDate = indianTime;

                                TransferWHOrderDispatchedDetail tdd = transferWHOrderDispatchedDetails1.Where(a => a.TransferOrderDetailId == data.TransferOrderDetailId).SingleOrDefault(); ;
                                tdd.Status = "Delivered";
                                tdd.UpdatedDate = indianTime;

                                var csd = db.DbCurrentStock.Where(x => x.ItemNumber == tdd.ItemNumber && x.WarehouseId == data.WarehouseId && x.ItemMultiMRPId == tdd.ItemMultiMRPId).FirstOrDefault();
                                if (csd != null)
                                {

                                }
                                else
                                {
                                    CurrentStock NewStock = new CurrentStock();
                                    NewStock.CompanyId = data.CompanyId;
                                    NewStock.CreationDate = indianTime;
                                    NewStock.CurrentInventory = 0;//tdd.TotalQuantity;
                                    NewStock.Deleted = false;
                                    NewStock.ItemMultiMRPId = tdd.ItemMultiMRPId;
                                    NewStock.itemname = data.itemname;
                                    NewStock.ItemNumber = tdd.ItemNumber;
                                    NewStock.itemBaseName = data.itemBaseName;
                                    NewStock.UpdatedDate = indianTime;
                                    NewStock.WarehouseId = data.WarehouseId ?? 0;
                                    NewStock.WarehouseName = data.WarehouseName;
                                    NewStock.MRP = data.MRP;
                                    NewStock.UOM = data.UOM;
                                    NewStock.userid = peopledata.PeopleID;
                                    db.DbCurrentStock.Add(NewStock);
                                    db.Commit();
                                }
                            }

                            TransferOrderHistory transferOrder = new TransferOrderHistory();
                            transferOrder.TransferOrderId = Tom.TransferOrderId;
                            transferOrder.Status = Tom.Status;
                            transferOrder.userid = peopledata.PeopleID;
                            if (peopledata.DisplayName == null)
                            {
                                transferOrder.username = peopledata.PeopleFirstName;
                            }
                            else
                            {
                                transferOrder.username = peopledata.DisplayName;
                            }
                            transferOrder.CreatedDate = indianTime;
                            db.TransferOrderHistoryDB.Add(transferOrder);


                            if (db.Commit() > 0)
                            {
                                #region virtual stock delivered
                                MultiStockHelper<OnWarehouseSarthiTransferDeliveredDc> MultiStockHelpers = new MultiStockHelper<OnWarehouseSarthiTransferDeliveredDc>();
                                List<OnWarehouseSarthiTransferDeliveredDc> TransferDispatchStockList = new List<OnWarehouseSarthiTransferDeliveredDc>();
                                foreach (var StockHit in transferWHOrderDispatchedDetails1.Where(x => x.TotalQuantity > 0))
                                {
                                    //if (StockHit.DamageQuantity > 0 || StockHit.ExpiryQuantity > 0)
                                    //    StockHit.TotalQuantity = StockHit.TotalQuantity - (StockHit.DamageQuantity??0 + StockHit.ExpiryQuantity??0);

                                    TransferDispatchStockList.Add(new OnWarehouseSarthiTransferDeliveredDc
                                    {
                                        SourceWarehouseId = StockHit.RequestToWarehouseId.GetValueOrDefault(),
                                        DestinationWarehouseId = StockHit.WarehouseId.GetValueOrDefault(),
                                        ItemMultiMRPId = StockHit.ItemMultiMRPId,
                                        TransferWHOrderDispatchedDetailsId = StockHit.TransferOrderDispatchedDetailId,
                                        TransferOrderId = StockHit.TransferOrderId,
                                        Qty = StockHit.TotalQuantity,
                                        ReceiveQty = StockHit.ReceiveQuantity,
                                        DamageQty = StockHit.DamageQuantity,
                                        ExpiryQty = StockHit.ExpiryQuantity,
                                        UserId = peopledata.PeopleID,
                                        IsFreeStock = false,
                                        Reason = Tom.RequestToWarehouseName + " Transfer Order to " + Tom.WarehouseName
                                    });
                                }
                                if (TransferDispatchStockList.Any())
                                {
                                    bool res = MultiStockHelpers.MakeEntry(TransferDispatchStockList, "Stock_OnWarehouseSarthiTransferDelivered", db, dbContextTransaction);
                                    if (!res)
                                    {
                                        IsUpdated = false;
                                        Message = "Some thing went wrong.";
                                        dbContextTransaction.Dispose();

                                    }
                                    else
                                    {
                                        IsUpdated = true;
                                        Message = "Add Successfuly.";
                                        dbContextTransaction.Complete();
                                    }
                                }
                                #endregion
                            }
                            else
                            {
                                Message = "Some thing went wrong.";
                            }
                        }
                        else
                        {
                            Message = "Data not Dispatched.";
                        }
                    }
                }
            }
            return Request.CreateResponse(HttpStatusCode.OK, Message);
        }

        #endregion

        #region Get Order History 

        [AllowAnonymous]
        [Route("GetTransferOrderUserHistory/V1")]
        [HttpGet]
        public HttpResponseMessage GetTransferOrderUserHistory(int PeopleId)
        {
            string resultMessage = "";
            bool status = false;
            var _result = new List<TransferOrderHistoryDC>();
            using (AuthContext db = new AuthContext())
            {
                if (PeopleId > 0)
                {
                    var param = new SqlParameter("PeopleId", PeopleId);

                    _result = db.Database.SqlQuery<TransferOrderHistoryDC>("exec TransferOrderHistorySarthiApp @PeopleId", param).ToList();

                    if (_result != null && _result.Any())
                    {
                        status = true; resultMessage = "Record found";
                    }
                    else
                    { status = true; resultMessage = "No Record found"; }
                }
            }
            var res = new
            {
                TransferOrderHistory = _result,
                status = status,
                Message = resultMessage
            };

            return Request.CreateResponse(HttpStatusCode.OK, res);
        }
        #endregion
    }
}