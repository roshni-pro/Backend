using AngularJSAuthentication.API.Helper;
using AngularJSAuthentication.API.Managers;
using AngularJSAuthentication.BusinessLayer.Managers.Reports;
using AngularJSAuthentication.BusinessLayer.Managers.Transactions.BatchCode;
using AngularJSAuthentication.Common.Helpers;
using AngularJSAuthentication.DataContracts.BatchCode;
using AngularJSAuthentication.DataContracts.JustInTime;
using AngularJSAuthentication.DataContracts.Masters;
using AngularJSAuthentication.DataContracts.Shared;
using AngularJSAuthentication.DataContracts.Transaction.Ledger.ItemLedger;
using AngularJSAuthentication.DataContracts.Transaction.Stocks;
using AngularJSAuthentication.Model;
using AngularJSAuthentication.Model.ClearTax;
using NLog;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using System.Transactions;
using System.Web.Http;
using System.Web.Http.Description;

namespace AngularJSAuthentication.API.Controllers
{
    [RoutePrefix("api/TransferOrder")]
    public class TransferOrderMasterController : ApiController
    {

        private static Logger logger = LogManager.GetCurrentClassLogger();
        private static TimeZoneInfo INDIAN_ZONE = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
        DateTime indianTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE);

        #region AddTranferOrder
        [Route("AddTranferOrder")]
        [HttpPost]
        public async System.Threading.Tasks.Task<HttpResponseMessage> AddAsync(List<TOdata> Tdata)
        {
            using (AuthContext context = new AuthContext())
            {
                using (var dbContextTransaction = context.Database.BeginTransaction())
                {

                    var identity = User.Identity as ClaimsIdentity;
                    int compid = 0, userid = 0;

                    //Added BY Anoop 25/2/2021
                    PaggingData objPD = new PaggingData();


                    if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))
                        compid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);

                    if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                        userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
                    var peopledata = context.Peoples.Where(x => x.PeopleID == userid && x.Active == true).SingleOrDefault();
                    if (peopledata != null && peopledata.PeopleID > 0 && Tdata[0].RequestFromWarehouseId > 0 && Tdata != null && Tdata.Any() && Tdata[0].RequestToWarehouseId > 0)
                    {
                        int RequestFromWarehouseId = Tdata[0].RequestFromWarehouseId; // Request
                        int RequestToWarehouseId = Tdata[0].RequestToWarehouseId;//Request full filler
                        double fread = Tdata[0].fread;
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
                        TOM.CreatedBy = userid.ToString();
                        TOM.fread = fread;
                        context.TransferWHOrderMasterDB.Add(TOM);
                        context.Commit();
                        //context.AddTransferHistory(TOM);
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
                        var StockIds = Tdata.Select(x => x.StockId).ToList();
                        var CurrentstockList = context.DbCurrentStock.Where(z => z.Deleted == false && StockIds.Contains(z.StockId)).ToList();
                        var ItemNumberList = CurrentstockList.Select(x => x.ItemNumber).ToList();
                        var ItemList = context.itemMasters.Where(z => ItemNumberList.Contains(z.Number) && z.WarehouseId == RequestToWarehouse.WarehouseId).ToList();

                        List<TransferWHOrderDetails> AddTransferWHOrderDetails = new List<TransferWHOrderDetails>();

                        foreach (var tItem in Tdata)
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

                            if (!RequestToWarehouse.IsCnF)
                            {
                                pd.UnitPrice = item.NetPurchasePrice;
                            }
                            else
                            {
                                pd.UnitPrice = tItem.PriceOfItem.Value;

                            }
                            pd.PriceofItem = (pd.TotalQuantity) * (item.NetPurchasePrice);

                            pd.WarehouseId = RequestFromWarehouse.WarehouseId; //request from warehouse
                            pd.CreationDate = indianTime;
                            pd.Status = "Pending";
                            pd.RequestToWarehouseId = tItem.RequestToWarehouseId;

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
                            // pd.StockBatchMasterId = tItem.StockBatchMasterId;//Sudhir 04-10-2022
                            var manager = new ItemLedgerManager();
                            List<ItemClassificationDC> objItemClassificationDClist = new List<ItemClassificationDC>();

                            ItemClassificationDC obj = new ItemClassificationDC();
                            obj.WarehouseId = tItem.RequestToWarehouseId;
                            obj.ItemNumber = stock.ItemNumber;
                            objItemClassificationDClist.Add(obj);


                            List<ItemClassificationDC> _objItemClassificationDClist = await manager.GetItemClassificationsAsync(objItemClassificationDClist);
                            pd.ABCClassification = _objItemClassificationDClist.Count == 0 ? "D" : _objItemClassificationDClist[0].Category;
                            AddTransferWHOrderDetails.Add(pd);
                        }
                        objPD.ordermaster = AddTransferWHOrderDetails;

                        if (AddTransferWHOrderDetails != null && AddTransferWHOrderDetails.Any())
                        {
                            context.TransferWHOrderDetailsDB.AddRange(AddTransferWHOrderDetails);
                            if (context.Commit() > 0)
                            {
                                dbContextTransaction.Commit();
                                return Request.CreateResponse(HttpStatusCode.OK, objPD);
                                // return objPD;
                            }
                            else
                            {
                                dbContextTransaction.Rollback();
                                return Request.CreateResponse(HttpStatusCode.OK, "Request to warehouse are not exist.");
                                //return objPD;
                            }
                        }
                    }

                    return Request.CreateResponse(HttpStatusCode.OK, "Request to warehouse are not exist.");
                    //return objPD;
                }
            }
        }

        #endregion
        #region DispachedOrder
        [Route("DispachedOrder")]
        [HttpPost]
        public HttpResponseMessage DispachedOrder(DispechedDTO TOdata)
        {
            var Message = "";
            var identity = User.Identity as ClaimsIdentity;
            int compid = 0, userid = 0;


            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))
                compid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);

            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

            if (TOdata != null && userid > 0)
            {
                TransactionOptions option = new TransactionOptions();
                option.IsolationLevel = System.Transactions.IsolationLevel.RepeatableRead;
                option.Timeout = TimeSpan.FromSeconds(90);
                using (TransactionScope dbContextTransaction = new TransactionScope(TransactionScopeOption.RequiresNew, option))
                {
                    using (AuthContext db = new AuthContext())
                    {
                        var peopledata = db.Peoples.Where(x => x.PeopleID == userid && x.Active == true).SingleOrDefault();

                        bool Isdispatched = db.TransferWHOrderDispatchedMasterDB.Any(x => x.TransferOrderId == TOdata.TransferOrderId);
                        var IsRequest = db.TransferWHOrderMasterDB.FirstOrDefault(x => x.TransferOrderId == TOdata.TransferOrderId);
                        if (!Isdispatched && IsRequest.Status == "Pending" && peopledata != null)
                        {
                            List<TransferWHOrderDispatchedDetail> AddTransferWHOrderDispatchedDetail = new List<TransferWHOrderDispatchedDetail>();
                            var flag = false;
                            int ReqWid = Convert.ToInt32(TOdata.TransferWHOrderDetailss[0].RequestToWarehouseId);//given
                            int warehouseid = TOdata.TransferWHOrderDetailss.Where(x => x.WarehouseId.HasValue).FirstOrDefault().WarehouseId.Value;//take
                            var TransferWHOrderDet = TOdata.TransferWHOrderDetailss.Where(c => c.TransferOrderId == TOdata.TransferOrderId).ToList();
                            var ItemMultiMRPIds = TransferWHOrderDet.Select(i => i.ItemMultiMRPId).ToList();
                            var itemNumbers = TransferWHOrderDet.Select(x => x.ItemNumber).ToList();
                            var Stock = db.DbCurrentStock.Where(x => itemNumbers.Contains(x.ItemNumber) && x.WarehouseId == ReqWid && ItemMultiMRPIds.Contains(x.ItemMultiMRPId)).ToList();
                            var Tom = db.TransferWHOrderMasterDB.Where(a => a.TransferOrderId == TOdata.TransferOrderId).SingleOrDefault();
                            var todList = db.TransferWHOrderDetailsDB.Where(x => x.TransferOrderId == Tom.TransferOrderId).ToList();

                            var warehouse = db.Warehouses.FirstOrDefault(x => x.WarehouseId == Tom.RequestToWarehouseId);
                            string wh1StateCode = warehouse.GSTin.Substring(0, 2);
                            var warehouse1 = db.Warehouses.FirstOrDefault(x => x.WarehouseId == Tom.WarehouseId);
                            string wh2StateCode = warehouse1.GSTin.Substring(0, 2);
                            var statelist = db.States.FirstOrDefault(x => x.Stateid == warehouse.Stateid);
                            if (warehouse != null && warehouse1 != null && statelist != null)
                            {
                                if (warehouse.IsStopCurrentStockTrans)
                                    return Request.CreateResponse(HttpStatusCode.OK, "Inventory Transactions are currently disabled for this warehouse... Please try after some time");


                                foreach (var o in TOdata.TransferWHOrderDetailss)
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
                                    tdm.VehicleNo = TOdata.vehicleNumber;
                                    tdm.VehicleType = TOdata.vehicleType;
                                    tdm.fread = TOdata.fread;
                                    tdm.Type = "Sales";
                                    tdm.EwaybillNumber = TOdata.EwaybillNumber;
                                    tdm.TransporterGstin = TOdata.TransporterGstin;
                                    tdm.TransporterName = TOdata.TransporterName;
                                    db.TransferWHOrderDispatchedMasterDB.Add(tdm);
                                    db.Commit();

                                    #region generate No
                                    bool IsDeliveryChallan = false;
                                    string DeliveryChallanNo = " ";
                                    string InternalTransferNo = " ";
                                    if (tdm.WarehouseId != 67 && tdm.WarehouseId != 80)
                                    {
                                        if (!string.IsNullOrEmpty(warehouse.GSTin))
                                        {
                                            IsDeliveryChallan = db.Warehouses.Any(x => x.GSTin != null && x.WarehouseId == Tom.WarehouseId && x.GSTin.Substring(0, 2) == warehouse.GSTin.Substring(0, 2));
                                        }
                                        if (IsDeliveryChallan)
                                        {
                                            DeliveryChallanNo = db.Database.SqlQuery<string>("EXEC spGetCurrentNumber 'ITDeliveryChallanNo', " + warehouse.Stateid).FirstOrDefault();
                                            tdm.DeliveryChallanNo = DeliveryChallanNo;
                                            tdm.DCCreatedDate = DateTime.Now;
                                        }
                                        else
                                        {
                                            InternalTransferNo = db.Database.SqlQuery<string>("EXEC spGetCurrentNumber 'InternalTransferNo', " + warehouse.Stateid).FirstOrDefault();
                                            tdm.InternalTransferNo = InternalTransferNo;
                                            tdm.ITCreatedDate = DateTime.Now;
                                        }
                                    }
                                    #endregion

                                    foreach (var Bdata in TOdata.BatchITransferDetails)
                                    {
                                        var data = TOdata.TransferWHOrderDetailss.FirstOrDefault(x => x.TransferOrderDetailId == Bdata.TransferOrderDetailId);
                                        if (!(TOdata.BatchITransferDetails.Where(c => c.TransferOrderDetailId == data.TransferOrderDetailId).Sum(c => c.Qty) == data.TotalQuantity))
                                        {
                                            Message = "Batch qty not matched with dispatched qty for item :" + data.itemname;
                                            return Request.CreateResponse(HttpStatusCode.OK, Message);
                                        }
                                        var tod = todList.FirstOrDefault(x => x.TransferOrderDetailId == data.TransferOrderDetailId);
                                        tod.Status = "Dispatched";
                                        tod.UpdatedDate = indianTime;
                                        db.Entry(tod).State = EntityState.Modified;

                                        var wh = db.Warehouses.FirstOrDefault(x => x.WarehouseId == data.RequestToWarehouseId);


                                        TransferWHOrderDispatchedDetail tdd = new TransferWHOrderDispatchedDetail();
                                        tdd.CompanyId = data.CompanyId;
                                        tdd.CreationDate = indianTime;
                                        tdd.Deleted = false;
                                        tdd.ItemMultiMRPId = data.ItemMultiMRPId;
                                        tdd.WarehouseId = data.WarehouseId;
                                        tdd.ItemNumber = data.ItemNumber;
                                        tdd.TotalTaxPercentage = data.TotalTaxPercentage;
                                        tdd.TransferOrderDetailId = data.TransferOrderDetailId;
                                        tdd.TransferOrderId = data.TransferOrderId;
                                        var POI = (Bdata.Qty) * (tod.NPP);
                                        tdd.PriceofItem = Convert.ToDouble(POI);
                                        tdd.itemBaseName = data.itemBaseName;
                                        tdd.itemname = data.itemname;
                                        tdd.ItemHsn = data.ItemHsn;
                                        tdd.NPP = wh.IsCnF == false ? data.NPP : data.UnitPrice;
                                        tdd.MRP = data.MRP;
                                        tdd.UnitofQuantity = data.UnitofQuantity;
                                        tdd.UOM = data.UOM;
                                        tdd.RequestToWarehouseId = data.RequestToWarehouseId;
                                        tdd.RequestToWarehouseName = data.RequestToWarehouseName;
                                        tdd.Status = "Dispatched";
                                        tdd.TotalQuantity = Bdata.Qty;
                                        tdd.UpdatedDate = indianTime;
                                        tdd.WarehouseName = data.WarehouseName;
                                        tdd.CreationDate = indianTime;
                                        tdd.Deleted = false;
                                        tdd.ABCClassification = tod.ABCClassification;
                                        tdd.StockBatchMasterId = Bdata.StockBatchMasterId;
                                        AddTransferWHOrderDispatchedDetail.Add(tdd);
                                    }
                                    if (AddTransferWHOrderDispatchedDetail != null && AddTransferWHOrderDispatchedDetail.Any())
                                    {
                                        db.TransferWHOrderDispatchedDetailDB.AddRange(AddTransferWHOrderDispatchedDetail);
                                        db.Entry(tdm).State = EntityState.Modified;

                                        double? amount = AddTransferWHOrderDispatchedDetail.Sum(x => x.NPP * x.TotalQuantity);
                                        #region Update IRN Check 
                                        if (wh1StateCode != wh2StateCode) //interstate =>IRN will generate
                                        {
                                            ClearTaxIntegration clearTaxIntegration = new ClearTaxIntegration();
                                            clearTaxIntegration.OrderId = Tom.TransferOrderId;
                                            clearTaxIntegration.IsActive = true;
                                            clearTaxIntegration.CreateDate = indianTime;
                                            clearTaxIntegration.IsProcessed = false;
                                            clearTaxIntegration.APIType = "GenerateIRN";
                                            clearTaxIntegration.APITypes = 2;
                                            db.ClearTaxIntegrations.Add(clearTaxIntegration);
                                        }
                                        else
                                        {
                                            if (amount >= statelist.IntrastateAmount)
                                            {
                                                ClearTaxIntegration clearTaxIntegration = new ClearTaxIntegration();
                                                clearTaxIntegration.OrderId = Tom.TransferOrderId;
                                                clearTaxIntegration.IsActive = true;
                                                clearTaxIntegration.CreateDate = indianTime;
                                                clearTaxIntegration.IsProcessed = false;
                                                clearTaxIntegration.APIType = "GenerateIRN";
                                                clearTaxIntegration.APITypes = 2;
                                                db.ClearTaxIntegrations.Add(clearTaxIntegration);
                                            }
                                        }
                                        #endregion
                                        if (db.Commit() > 0)
                                        {
                                            #region virtual stock
                                            MultiStockHelper<OnWarehouseTransferDispatchDC> MultiStockHelpers = new MultiStockHelper<OnWarehouseTransferDispatchDC>();
                                            List<OnWarehouseTransferDispatchDC> TransferDispatchStockList = new List<OnWarehouseTransferDispatchDC>();
                                            List<TransferOrderItemBatchMasterDc> TransferOrderItemBatchMasterList = new List<TransferOrderItemBatchMasterDc>();
                                            foreach (var StockHit in AddTransferWHOrderDispatchedDetail.Where(x => x.TotalQuantity > 0))
                                            {
                                                TransferDispatchStockList.Add(new OnWarehouseTransferDispatchDC
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
                                                TransferOrderItemBatchMasterList.Add(new TransferOrderItemBatchMasterDc
                                                {

                                                    ItemMultiMRPId = StockHit.ItemMultiMRPId,
                                                    Qty = StockHit.TotalQuantity * (-1),
                                                    StockBatchMasterId = StockHit.StockBatchMasterId,
                                                    WarehouseId = StockHit.RequestToWarehouseId.GetValueOrDefault(),
                                                    ObjectId = StockHit.TransferOrderId,
                                                    ObjectIdDetailId = StockHit.TransferOrderDetailId,
                                                });
                                            }

                                            if (TransferDispatchStockList.Any() && TransferOrderItemBatchMasterList.Any())
                                            {
                                                BatchMasterManager batchMasterManager = new BatchMasterManager();
                                                bool res = MultiStockHelpers.MakeEntry(TransferDispatchStockList, "Stock_OnWarehouseTransferDispatch", db, dbContextTransaction);
                                                var StockTxnType = db.StockTxnTypeMasters.FirstOrDefault(x => x.IsActive && x.StockTxnType == "InternalOutCurrent" && x.IsDeleted == false);

                                                bool batchRes = batchMasterManager.AddQty(TransferOrderItemBatchMasterList, db, userid, StockTxnType.Id);
                                                if (!res || !batchRes)
                                                {
                                                    dbContextTransaction.Dispose();
                                                    Message = "due stock issue";
                                                    return Request.CreateResponse(HttpStatusCode.OK, Message);

                                                }
                                                else
                                                {
                                                    db.Commit();

                                                    #region on CalculatePurchasePrice
                                                    try
                                                    {
                                                        List<RiskCalculatePurchasePriceDc> Items = new List<RiskCalculatePurchasePriceDc>();
                                                        foreach (var item in AddTransferWHOrderDispatchedDetail.GroupBy(x => x.ItemMultiMRPId))
                                                        {
                                                            Items.Add(new RiskCalculatePurchasePriceDc
                                                            {
                                                                ItemMultiMrpId = item.FirstOrDefault().ItemMultiMRPId,
                                                                Price = 0,
                                                                Qty = item.Sum(c => c.TotalQuantity),
                                                                WarehouseId = warehouseid
                                                            });
                                                        }

                                                        CalculatePurchasePriceHelper helper = new CalculatePurchasePriceHelper();
                                                        bool IsUpdate = helper.CalculatePPOnInternalTransferForRisk(db, Items, Tom.RequestToWarehouseId ?? 0, 1, userid);
                                                    }
                                                    catch (Exception ex)
                                                    {
                                                        string error = ex.InnerException != null ? ex.ToString() + Environment.NewLine + ex.InnerException.ToString() : ex.ToString();
                                                        TextFileLogHelper.LogError(new StringBuilder("Error on Internal Purchase Dispatched Risk Qty Update : ").Append(error).Append($"  on : {indianTime}").ToString());
                                                        TextFileLogHelper.LogError(new StringBuilder("TransferOrderId Id : ").Append(Tom.TransferOrderId).Append($" on : {indianTime}").ToString());
                                                        TextFileLogHelper.LogError(new StringBuilder("For WarehouseId# : ").Append(Tom.WarehouseId).Append($" on : {indianTime}").ToString());
                                                        TextFileLogHelper.LogError(new StringBuilder("For RequestToWarehouseId# : ").Append(Tom.RequestToWarehouseId).Append($" on : {indianTime}").ToString());

                                                    }

                                                    #endregion

                                                    dbContextTransaction.Complete();
                                                    Message = "Dispatched Successfuly";
                                                    return Request.CreateResponse(HttpStatusCode.OK, Message);
                                                }

                                            }
                                            #endregion

                                        }

                                    }

                                }
                            }
                        }

                    }
                }
            }
            Message = "Something Went wrong";
            return Request.CreateResponse(HttpStatusCode.OK, Message);

        }

        #endregion        

        #region DispachedOrderUpdated
        [Route("DispachedOrderUpdated")]
        [HttpPost]
        public HttpResponseMessage DispachedOrderUpdated(DispechedDTO TOdata)
        {
            var Message = "";
            var identity = User.Identity as ClaimsIdentity;
            int compid = 0, userid = 0;

            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))
                compid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);

            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

            if (TOdata != null)
            {
                using (AuthContext db = new AuthContext())
                {
                    bool status = false;
                    var dispatchedMasterDetail = db.TransferWHOrderDispatchedMasterDB.Where(x => x.TransferOrderId == TOdata.TransferOrderId).SingleOrDefault();
                    var data = db.TransferWHOrderMasterDB.Where(y => y.TransferOrderId == TOdata.TransferOrderId).SingleOrDefault();
                    if (dispatchedMasterDetail != null)
                    {
                        if (data != null)
                        {
                            data.fread = TOdata.fread;
                            db.Entry(data).State = EntityState.Modified;
                            db.Commit();
                        }
                        dispatchedMasterDetail.VehicleNo = TOdata.vehicleNumber;
                        dispatchedMasterDetail.fread = TOdata.fread;
                        dispatchedMasterDetail.EwaybillNumber = TOdata.EwaybillNumber;
                        //db.TransferWHOrderDispatchedMasterDB.Add(dispatchedMasterDetail);
                        db.Entry(dispatchedMasterDetail).State = EntityState.Modified;
                        db.Commit();
                        status = true;
                    }
                    if (status)
                    {
                        Message = "Updated Successfuly";
                        return Request.CreateResponse(HttpStatusCode.OK, Message);
                    }
                }

            }
            Message = "Something Went wrong";
            return Request.CreateResponse(HttpStatusCode.OK, Message);
        }

        #endregion        

        #region Reject Option for WarehouseTransfer Order
        /// <summary>
        /// Reject Option for WarehouseTransfer Order
        /// Created By Ashwin
        /// </summary>
        /// <param name="TOdata"></param>
        /// <returns></returns>
        [Authorize]
        [Route("CancelOrder")]
        [HttpPost]
        public HttpResponseMessage CancelOrder(List<TransferWHOrderDetails> TOdata)
        {
            var Message = "";
            var identity = User.Identity as ClaimsIdentity;
            int compid = 0, userid = 0;
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))
                compid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);

            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
            TransactionOptions option = new TransactionOptions();
            option.IsolationLevel = System.Transactions.IsolationLevel.RepeatableRead;
            option.Timeout = TimeSpan.FromSeconds(90);
            using (var dbContextTransaction = new TransactionScope(TransactionScopeOption.Required, option))
            {
                using (AuthContext db = new AuthContext())
                {

                    var peopledata = db.Peoples.Where(x => x.PeopleID == userid && x.Active == true).SingleOrDefault();

                    if (TOdata != null && peopledata != null)
                    {
                        TransferWHOrderDetails Tod = TOdata.FirstOrDefault();
                        TransferWHOrderDispatchedMaster Tdm = db.TransferWHOrderDispatchedMasterDB.FirstOrDefault(x => x.TransferOrderId == Tod.TransferOrderId);
                        if (Tdm != null && Tdm.Status == "Dispatched")
                        {

                            var warehouse = db.Warehouses.FirstOrDefault(x => x.WarehouseId == Tdm.RequestToWarehouseId);

                            if (warehouse.IsStopCurrentStockTrans)
                                return Request.CreateResponse(HttpStatusCode.OK, "Inventory Transactions are currently disabled for this warehouse... Please try after some time");

                            TransferWHOrderMaster Tom = db.TransferWHOrderMasterDB.Where(a => a.TransferOrderId == Tod.TransferOrderId).SingleOrDefault();
                            var todList = db.TransferWHOrderDetailsDB.Where(a => a.TransferOrderId == Tom.TransferOrderId).ToList();
                            var tddList = db.TransferWHOrderDispatchedDetailDB.Where(a => a.TransferOrderId == Tom.TransferOrderId).ToList();
                            foreach (var data in tddList)
                            {
                                var tod = todList.FirstOrDefault(a => a.TransferOrderDetailId == data.TransferOrderDetailId);
                                tod.Status = "Canceled";
                                tod.UpdatedDate = indianTime;
                                db.Entry(tod).State = EntityState.Modified;

                                data.Status = "Canceled";
                                data.UpdatedDate = indianTime;
                                db.Entry(data).State = EntityState.Modified;
                            }


                            Tom.Status = "Canceled";
                            Tom.UpdatedDate = indianTime;
                            Tom.userid = userid;
                            db.Entry(Tom).State = EntityState.Modified;

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
                            #region generate CN No
                            string InternalTransferNoCN = " ";
                            if (Tdm.WarehouseId != 67 && Tdm.WarehouseId != 80)
                            {
                                InternalTransferNoCN = db.Database.SqlQuery<string>("EXEC spGetCurrentNumber 'InternalTransferNoCN', " + warehouse.Stateid).FirstOrDefault();
                                Tdm.InternalTransferNoCN = InternalTransferNoCN;
                                Tdm.ITCNCreatedDate = DateTime.Now;
                            }
                            #endregion
                            Tdm.Status = "Canceled";
                            Tdm.UpdatedDate = indianTime;
                            db.Entry(Tdm).State = EntityState.Modified;
                            if (db.Commit() > 0)
                            {
                                #region virtual stock
                                MultiStockHelper<OnWarehouseTransferDispatchedRejectDC> MultiStockHelpers = new MultiStockHelper<OnWarehouseTransferDispatchedRejectDC>();
                                List<OnWarehouseTransferDispatchedRejectDC> TransferDispatchRejectStockList = new List<OnWarehouseTransferDispatchedRejectDC>();
                                List<TransferOrderItemBatchMasterDc> TransferOrderItemBatchMasterList = new List<TransferOrderItemBatchMasterDc>();
                                foreach (var StockHit in tddList.Where(x => x.TotalQuantity > 0))
                                {
                                    TransferDispatchRejectStockList.Add(new OnWarehouseTransferDispatchedRejectDC
                                    {
                                        SourceWarehouseId = StockHit.RequestToWarehouseId.GetValueOrDefault(),
                                        ItemMultiMRPId = StockHit.ItemMultiMRPId,
                                        TransferWHOrderDispatchedDetailsId = StockHit.TransferOrderDispatchedDetailId,
                                        TransferOrderId = StockHit.TransferOrderId,
                                        Qty = StockHit.TotalQuantity,
                                        UserId = peopledata.PeopleID,
                                        IsFreeStock = false,
                                        Reason = Tom.WarehouseName + " Return Order to " + Tom.RequestToWarehouseName
                                    });
                                    TransferOrderItemBatchMasterList.Add(new TransferOrderItemBatchMasterDc
                                    {

                                        ItemMultiMRPId = StockHit.ItemMultiMRPId,
                                        Qty = StockHit.TotalQuantity,
                                        StockBatchMasterId = StockHit.StockBatchMasterId,
                                        WarehouseId = StockHit.RequestToWarehouseId.GetValueOrDefault(),
                                        ObjectId = StockHit.TransferOrderId,
                                        ObjectIdDetailId = StockHit.TransferOrderDetailId,
                                    });
                                }

                                if (TransferDispatchRejectStockList.Any() && TransferOrderItemBatchMasterList.Any())
                                {
                                    BatchMasterManager batchMasterManager = new BatchMasterManager();
                                    bool res = MultiStockHelpers.MakeEntry(TransferDispatchRejectStockList, "Stock_OnWarehouseTransferDispatchedReject", db, dbContextTransaction);

                                    var StockTxnType = db.StockTxnTypeMasters.FirstOrDefault(x => x.IsActive && x.StockTxnType == "InternalInCurrent" && x.IsDeleted == false);

                                    bool batchRes = batchMasterManager.AddQty(TransferOrderItemBatchMasterList, db, userid, StockTxnType.Id);
                                    if (!res || !batchRes)
                                    {
                                        dbContextTransaction.Dispose();
                                        Message = "somthing went wrong due to qty";
                                        return Request.CreateResponse(HttpStatusCode.OK, Message);
                                    }
                                    else
                                    {
                                        db.Commit();


                                        #region on CalculatePurchasePrice
                                        try
                                        {
                                            List<RiskCalculatePurchasePriceDc> Items = new List<RiskCalculatePurchasePriceDc>();
                                            foreach (var item in tddList.GroupBy(x => x.ItemMultiMRPId))
                                            {
                                                Items.Add(new RiskCalculatePurchasePriceDc
                                                {
                                                    ItemMultiMrpId = item.FirstOrDefault().ItemMultiMRPId,
                                                    Price = 0,
                                                    Qty = -1 * item.Sum(c => c.TotalQuantity),
                                                    WarehouseId = Tom.WarehouseId ?? 0
                                                });
                                            }

                                            CalculatePurchasePriceHelper helper = new CalculatePurchasePriceHelper();
                                            bool IsUpdate = helper.CalculatePPOnInternalTransferForRisk(db, Items, Tom.RequestToWarehouseId ?? 0, 1, userid);
                                        }
                                        catch (Exception ex)
                                        {

                                            string error = ex.InnerException != null ? ex.ToString() + Environment.NewLine + ex.InnerException.ToString() : ex.ToString();
                                            TextFileLogHelper.LogError(new StringBuilder("Error on Internal Purchase Reject Risk Qty Update : ").Append(error).Append($"  on : {indianTime}").ToString());
                                            TextFileLogHelper.LogError(new StringBuilder("TransferOrderId Id : ").Append(Tom.TransferOrderId).Append($" on : {indianTime}").ToString());
                                            TextFileLogHelper.LogError(new StringBuilder("For WarehouseId# : ").Append(Tom.WarehouseId).Append($" on : {indianTime}").ToString());
                                            TextFileLogHelper.LogError(new StringBuilder("For RequestToWarehouseId# : ").Append(Tom.RequestToWarehouseId).Append($" on : {indianTime}").ToString());

                                        }

                                        #endregion

                                        dbContextTransaction.Complete();
                                        Message = "Reject Successfully";
                                        return Request.CreateResponse(HttpStatusCode.OK, Message);
                                    }
                                }
                                #endregion
                            }
                            else
                            {
                                dbContextTransaction.Dispose();
                                Message = "Some thing went wrong";
                            }

                        }
                        else
                        {
                            Message = "Already in status : " + Tdm.Status;
                            return Request.CreateResponse(HttpStatusCode.OK, Message);
                        }
                    }
                    Message = "Something went wrong";
                    return Request.CreateResponse(HttpStatusCode.OK, Message);
                }
            }

        }

        #endregion
        #region DelivereOrder
        [Route("DeliveredOrder")]
        [HttpPost]
        public HttpResponseMessage DelivereOrder(List<TransferWHOrderDetails> TOdata)
        {
            var identity = User.Identity as ClaimsIdentity;
            int compid = 0, userid = 0;
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))
                compid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);

            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
            var Message = "";
            TransactionOptions option = new TransactionOptions();
            option.IsolationLevel = System.Transactions.IsolationLevel.RepeatableRead;
            option.Timeout = TimeSpan.FromSeconds(90);
            using (var dbContextTransaction = new TransactionScope(TransactionScopeOption.RequiresNew, option))
            {
                using (AuthContext db = new AuthContext())
                {
                    var peopledata = db.Peoples.Where(x => x.PeopleID == userid && x.Active == true).SingleOrDefault();

                    if (TOdata != null && peopledata != null)
                    {
                        TransferWHOrderDetails Tod = TOdata.FirstOrDefault();
                        var Isdispatched = db.TransferWHOrderDispatchedMasterDB.FirstOrDefault(x => x.TransferOrderId == Tod.TransferOrderId);
                        if (Isdispatched.Status == "Dispatched")
                        {
                            var warehouse = db.Warehouses.FirstOrDefault(x => x.WarehouseId == Tod.WarehouseId);

                            if (warehouse.IsStopCurrentStockTrans)
                                return Request.CreateResponse(HttpStatusCode.InternalServerError, "Inventory Transactions are currently disabled for this warehouse... Please try after some time");


                            TransferWHOrderMaster Tom = db.TransferWHOrderMasterDB.Where(a => a.TransferOrderId == Tod.TransferOrderId).SingleOrDefault();

                            var todList = db.TransferWHOrderDetailsDB.Where(a => a.TransferOrderId == Tom.TransferOrderId).ToList();
                            var tddList = db.TransferWHOrderDispatchedDetailDB.Where(a => a.TransferOrderId == Tom.TransferOrderId).ToList();


                            foreach (var data in todList)
                            {
                                foreach (var item in tddList.Where(a => a.TransferOrderDetailId == data.TransferOrderDetailId))
                                {
                                    var csd = db.DbCurrentStock.Where(x => x.WarehouseId == data.WarehouseId && x.ItemMultiMRPId == item.ItemMultiMRPId).FirstOrDefault();
                                    if (csd == null)
                                    {
                                        CurrentStock NewStock = new CurrentStock();
                                        NewStock.CompanyId = data.CompanyId;
                                        NewStock.CreationDate = indianTime;
                                        NewStock.CurrentInventory = 0;//tdd.TotalQuantity;
                                        NewStock.Deleted = false;
                                        NewStock.ItemMultiMRPId = item.ItemMultiMRPId;
                                        NewStock.itemname = data.itemname;
                                        NewStock.ItemNumber = item.ItemNumber;
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
                                    item.Status = "Delivered";
                                    item.UpdatedDate = indianTime;
                                    db.Entry(item).State = EntityState.Modified;
                                }

                                data.Status = "Delivered";
                                data.UpdatedDate = indianTime;
                                db.Entry(data).State = EntityState.Modified;
                            }
                            Tom.Status = "Delivered";
                            Tom.UpdatedDate = indianTime;
                            Tom.userid = userid;
                            db.Entry(Tom).State = EntityState.Modified;

                            Isdispatched.Status = "Delivered";
                            Isdispatched.Type = "Purchase";
                            Isdispatched.UpdatedDate = indianTime;
                            db.Entry(Isdispatched).State = EntityState.Modified;

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
                                MultiStockHelper<OnWarehouseTransferDeliveredDc> MultiStockHelpers = new MultiStockHelper<OnWarehouseTransferDeliveredDc>();
                                List<OnWarehouseTransferDeliveredDc> TransferDispatchStockList = new List<OnWarehouseTransferDeliveredDc>();
                                List<TransferOrderItemBatchMasterDc> TransferOrderItemBatchMasterList = new List<TransferOrderItemBatchMasterDc>();
                                foreach (var StockHit in tddList.Where(x => x.TotalQuantity > 0))
                                {
                                    TransferDispatchStockList.Add(new OnWarehouseTransferDeliveredDc
                                    {
                                        SourceWarehouseId = StockHit.RequestToWarehouseId.GetValueOrDefault(),
                                        DestinationWarehouseId = StockHit.WarehouseId.GetValueOrDefault(),
                                        ItemMultiMRPId = StockHit.ItemMultiMRPId,
                                        TransferWHOrderDispatchedDetailsId = StockHit.TransferOrderDispatchedDetailId,
                                        TransferOrderId = StockHit.TransferOrderId,
                                        Qty = StockHit.TotalQuantity,
                                        UserId = peopledata.PeopleID,
                                        IsFreeStock = false,
                                        Reason = Tom.RequestToWarehouseName + " Transfer Order to " + Tom.WarehouseName
                                    });
                                    var transferOrderBatch = new TransferOrderItemBatchMasterDc
                                    {

                                        ItemMultiMRPId = StockHit.ItemMultiMRPId,
                                        Qty = StockHit.TotalQuantity,
                                        StockBatchMasterId = StockHit.StockBatchMasterId,
                                        WarehouseId = StockHit.WarehouseId.GetValueOrDefault(),
                                        ObjectId = StockHit.TransferOrderId,
                                        ObjectIdDetailId = StockHit.TransferOrderDispatchedDetailId,
                                    };
                                    TransferOrderItemBatchMasterList.Add(transferOrderBatch);
                                    BatchMasterManager batchMasterManager = new BatchMasterManager();
                                    long batchMasterId = db.StockBatchMasters.First(x => x.Id == transferOrderBatch.StockBatchMasterId).BatchMasterId;
                                    transferOrderBatch.StockBatchMasterId = batchMasterManager.GetOrCreate(transferOrderBatch.ItemMultiMRPId, transferOrderBatch.WarehouseId, "C", batchMasterId, db, userid);
                                }
                                if (TransferDispatchStockList.Any() && TransferOrderItemBatchMasterList.Any())
                                {
                                    BatchMasterManager batchMasterManager = new BatchMasterManager();
                                    bool res = MultiStockHelpers.MakeEntry(TransferDispatchStockList, "Stock_OnWarehouseTransferDelivered", db, dbContextTransaction);

                                    var StockTxnType = db.StockTxnTypeMasters.FirstOrDefault(x => x.IsActive && x.StockTxnType == "InternalInCurrent" && x.IsDeleted == false);

                                    bool batchRes = batchMasterManager.AddQty(TransferOrderItemBatchMasterList, db, userid, StockTxnType.Id);
                                    if (!res || !batchRes)
                                    {
                                        Message = "Some thing went wrong.";
                                        dbContextTransaction.Dispose();
                                        return Request.CreateResponse(HttpStatusCode.OK, Message);
                                    }
                                    else
                                    {
                                        db.Commit();
                                        Message = "Add Successfuly.";

                                        #region on CalculatePurchasePrice
                                        try
                                        {

                                            CalculatePurchasePriceHelper helper = new CalculatePurchasePriceHelper();
                                            bool IsUpdate = helper.CalculatePPOnInternalTransferForIR(db, Isdispatched.WarehouseId.Value, 1, Isdispatched.TransferOrderId, peopledata.PeopleID);
                                            List<int> ItemMultiMrpIds = new List<int>();
                                            ItemMultiMrpIds.AddRange(tddList.Select(x => x.ItemMultiMRPId).Distinct().ToList());
                                            bool IsItemUpdate = helper.GetCalculatePPAndUpdateItemMaster(db, Isdispatched.WarehouseId.Value, ItemMultiMrpIds);
                                        }
                                        catch (Exception ex)
                                        {
                                            string error = ex.InnerException != null ? ex.ToString() + Environment.NewLine + ex.InnerException.ToString() : ex.ToString();
                                            TextFileLogHelper.LogError(new StringBuilder("Error On  Calculate Internal Purchase Delivered Update: ").Append(error).Append($"  on : {indianTime}").ToString());
                                            TextFileLogHelper.LogError(new StringBuilder("Internal Transfer Id: ").Append(Isdispatched.TransferOrderId).Append($" on : {indianTime}").ToString());
                                            TextFileLogHelper.LogError(new StringBuilder("For WarehouseId# : ").Append(Isdispatched.WarehouseId).Append($" on : {indianTime}").ToString());
                                            TextFileLogHelper.LogError(new StringBuilder("For RequestToWarehouseId# : ").Append(Isdispatched.RequestToWarehouseId).Append($" on : {indianTime}").ToString());

                                        }

                                        #endregion


                                        dbContextTransaction.Complete();
                                        return Request.CreateResponse(HttpStatusCode.OK, Message);

                                    }
                                }
                                #endregion
                            }
                            else
                            {
                                dbContextTransaction.Dispose();
                                Message = "Some thing went wrong.";
                            }
                        }
                        else
                        {
                            Message = "Data Already Dispatched.";
                        }
                    }
                    else { Message = "Something went wrong"; }
                }
            }
            return Request.CreateResponse(HttpStatusCode.OK, Message);

        }

        #endregion

        #region

        [Route("")]
        public HttpResponseMessage Get(int list, int page, string Warehouseid, string StartDate, string EndDate, string Status, int? TransferOrderId)
        {
            using (AuthContext context = new AuthContext())
            {
                if (Warehouseid != null)
                {

                    if (StartDate == "null") { StartDate = null; }
                    if (EndDate == "null") { EndDate = null; }
                    if (Status == "null") { Status = null; }
                    string ProcedureName = "GetTransferOrderMasterNew @Warehouseids, @StartDate, @EndDate, @Status, @TransferOrderId,@Skip,@Take";
                    PaggingData ObjPaggingdata = GetTransferOrderMaster(ProcedureName, Warehouseid, StartDate, EndDate, Status, page, list, TransferOrderId);
                    return Request.CreateResponse(HttpStatusCode.OK, ObjPaggingdata);
                }
                else
                {
                    return null;
                }
            }
        }
        [Route("GetTransferOrderMastersendExport")]
        public HttpResponseMessage GetExport(int list, int page, string Warehouseid, string StartDate, string EndDate, string Status, int? TransferOrderId)
        {
            using (AuthContext context = new AuthContext())
            {
                if (Warehouseid != null)
                {

                    if (StartDate == "null") { StartDate = null; }
                    if (EndDate == "null") { EndDate = null; }
                    if (Status == "null") { Status = null; }
                    string ProcedureName = "GetTransferOrderMastersendExport @Warehouseids, @StartDate, @EndDate, @Status, @TransferOrderId,@Skip,@Take";
                    PaggingData ObjPaggingdata = GetTransferOrderMaster(ProcedureName, Warehouseid, StartDate, EndDate, Status, page, list, TransferOrderId);
                    return Request.CreateResponse(HttpStatusCode.OK, ObjPaggingdata);
                }
                else
                {
                    return null;
                }
            }
        }

        [Route("SearchTransferOrder")]
        [HttpGet]
        public PaggingData Get(string key)
        {
            using (AuthContext context = new AuthContext())
            {
                PaggingData obj = new PaggingData();
                var identity = User.Identity as ClaimsIdentity;
                int compid = 0, userid = 0;
                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))
                    compid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                    userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
                if (key != null)
                {
                    int id = Convert.ToInt32(key);
                    if (id > 0)
                    {
                        dynamic data = context.TransferWHOrderMasterDB.Where(x => x.TransferOrderId == id).ToList();
                        obj.total_count = data.Count;
                        obj.ordermaster = data;
                    }
                }
                return obj;
            }
        }
        #endregion
        #region GetRequests
        [HttpGet]
        [Route("GetRequests")]
        public HttpResponseMessage GetRequest(int list, int page, string Warehouseid, string StartDate, string EndDate, string Status, int? TransferOrderId)
        {
            logger.Info("start ItemMaster: ");
            using (AuthContext context = new AuthContext())
            {
                if (StartDate == "null") { StartDate = null; }
                if (EndDate == "null") { EndDate = null; }
                if (Status == "null") { Status = null; }
                if (Warehouseid != null)
                {
                    string ProcedureName = "GetTransferOrderRequestMaster_New @Warehouseids, @StartDate, @EndDate, @Status, @TransferOrderId,@Skip,@Take";
                    PaggingData ObjPaggingdata = GetTransferOrderMaster(ProcedureName, Warehouseid, StartDate, EndDate, Status, page, list, TransferOrderId);
                    return Request.CreateResponse(HttpStatusCode.OK, ObjPaggingdata);
                }
                else
                {
                    return null;
                }

            }
        }
        [HttpGet]
        [Route("GetRequestsExport")]
        public HttpResponseMessage GetRequestExport(int list, int page, string Warehouseid, string StartDate, string EndDate, string Status, int? TransferOrderId)
        {
            logger.Info("start ItemMaster: ");
            using (AuthContext context = new AuthContext())
            {
                if (StartDate == "null") { StartDate = null; }
                if (EndDate == "null") { EndDate = null; }
                if (Status == "null") { Status = null; }
                if (Warehouseid != null)
                {
                    string ProcedureName = "GetTransferOrder_New_Export @Warehouseids, @StartDate, @EndDate, @Status, @TransferOrderId,@Skip,@Take";
                    PaggingData ObjPaggingdata = GetTransferOrderMaster(ProcedureName, Warehouseid, StartDate, EndDate, Status, page, list, TransferOrderId);
                    return Request.CreateResponse(HttpStatusCode.OK, ObjPaggingdata);
                }
                else
                {
                    return null;
                }

            }
        }

        [HttpGet]
        [Route("SearchTransferOrderRequest")]
        public PaggingData SearchTransferOrderRequest(string key)
        {
            logger.Info("start ItemMaster: ");
            using (AuthContext context = new AuthContext())
            {


                try
                {

                    var identity = User.Identity as ClaimsIdentity;
                    int compid = 0, userid = 0;
                    //int Warehouse_id = 0;

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
                    int CompanyId = compid;

                    if (key != null)
                    {
                        //var itemPagedListWid = context.searchdata( key);

                        PaggingData obj = new PaggingData();
                        obj.total_count = context.TransferWHOrderMasterDB.Where(x => x.CompanyId == CompanyId).Count();
                        obj.ordermaster = context.TransferWHOrderMasterDB.AsEnumerable().Where(x => x.WarehouseName.Contains(key) || x.Status.Contains(key)).OrderByDescending(y => y.CreationDate).ToList();
                        if (obj.ordermaster.Count == 0)
                        {
                            obj.ordermaster = context.TransferWHOrderMasterDB.AsEnumerable().Where(x => x.TransferOrderId == Convert.ToInt32(key)).OrderByDescending(y => y.CreationDate).ToList();
                        }


                        return obj;
                    }
                    else
                    {
                        return null;
                    }
                }
                catch (Exception ex)
                {
                    logger.Error("Error in ItemMaster " + ex.Message);
                    logger.Info("End  ItemMaster: ");
                    return null;
                }
            }
        }
        #endregion
        #region OldCode
        #region TransferWHOrderDetails
        [HttpGet]
        [ResponseType(typeof(TransferWHOrderDetails))]
        [Route("Detail")]
        public async System.Threading.Tasks.Task<HttpResponseMessage> GetAsync(int DetailId, int Warehouseid)
        {
            using (AuthContext db = new AuthContext())
            {
                logger.Info("start Category: ");
                List<TransferWHOrderDetails> ass = new List<TransferWHOrderDetails>();


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

                    TransferWHOrderMaster To = db.TransferWHOrderMasterDB.Where(a => a.TransferOrderId == DetailId).SingleOrDefault();
                    var wh = db.Warehouses.Where(k => k.Deleted != true && k.WarehouseId == To.RequestToWarehouseId).FirstOrDefault(); //iscnf
                    var states = db.States.Where(x => x.Stateid == wh.Stateid).Select(x => new { x.InterstateAmount, x.IntrastateAmount }).FirstOrDefault();


                    if (To.Status == "Delivered" || To.Status == "Dispatched" || To.Status == "Canceled")
                    {

                        List<GetTODataDTO> asss = (from M in db.TransferWHOrderDetailsDB.Where(M => M.TransferOrderId == DetailId && M.WarehouseId == Warehouseid)
                                                   join TDM in db.TransferWHOrderDispatchedMasterDB on M.TransferOrderId equals TDM.TransferOrderId
                                                   join D in db.TransferWHOrderDispatchedDetailDB on M.TransferOrderDetailId equals D.TransferOrderDetailId
                                                   select new GetTODataDTO
                                                   {
                                                       TransferOrderDetailId = M.TransferOrderDetailId,
                                                       TransferOrderId = M.TransferOrderId,
                                                       WarehouseId = M.WarehouseId ?? 0,
                                                       Status = M.Status,
                                                       WarehouseName = M.WarehouseName,
                                                       RequestToWarehouseId = M.RequestToWarehouseId,
                                                       RequestToWarehouseName = M.RequestToWarehouseName,
                                                       ItemId = M.ItemId,
                                                       ItemNumber = M.ItemNumber,
                                                       ItemHsn = M.ItemHsn,
                                                       TotalTaxPercentage = M.TotalTaxPercentage,
                                                       NPP = !wh.IsCnF ? M.NPP : M.UnitPrice,
                                                       TotalQuantity = M.TotalQuantity,
                                                       itemname = M.itemname,
                                                       itemBaseName = M.itemBaseName,
                                                       ItemMultiMRPId = M.ItemMultiMRPId,
                                                       MRP = M.MRP,
                                                       UnitofQuantity = M.UnitofQuantity,
                                                       DispatchedQty = D.TotalQuantity,
                                                       Type = TDM.Type,
                                                       ABCClassification = D.ABCClassification == null ? "D" : D.ABCClassification,
                                                       InternalTransferNo = TDM.InternalTransferNo,
                                                       InternalTransferNoCN = TDM.InternalTransferNoCN,
                                                       DeliveryChallanNo = TDM.DeliveryChallanNo,
                                                       ITCreatedDate = TDM.ITCreatedDate,
                                                       ITCNCreatedDate = TDM.ITCNCreatedDate,
                                                       DCCreatedDate = TDM.DCCreatedDate,
                                                       vehicleNumber = TDM.VehicleNo,
                                                       vehicleType = TDM.VehicleType,
                                                       fread = TDM.fread,
                                                       EwaybillNumber = TDM.EwaybillNumber,
                                                       StockBatchMasterId = D.StockBatchMasterId,
                                                       UnitPrice = M.UnitPrice,
                                                       InterStateLimit = states.InterstateAmount.Value,
                                                       IntraStateLimit = states.IntrastateAmount.Value
                                                       //StateWiseLimit >=states.InterstateAmount || states.IntrastateAmount
                                                   }).ToList();
                        var manager = new ItemLedgerManager();
                        //List<ItemClassificationDC> objItemClassificationDClist = new List<ItemClassificationDC>();
                        //foreach (var data in asss)
                        //{
                        //    ItemClassificationDC obj = new ItemClassificationDC();
                        //    obj.WarehouseId = data.WarehouseId;
                        //    obj.ItemNumber = data.ItemNumber;
                        //    objItemClassificationDClist.Add(obj);

                        //}
                        //List<ItemClassificationDC> _objItemClassificationDClist = await manager.GetItemClassificationsAsync(objItemClassificationDClist);
                        if (!wh.IsCnF)
                        {
                            asss.ForEach(q => q.PriceofItem = Convert.ToDouble(q.DispatchedQty * q.NPP));
                            asss.ForEach(q => q.TotalPrice = asss.Sum(c => c.PriceofItem));
                        }
                        else
                        {
                            asss.ForEach(q => q.PriceofItem = Convert.ToDouble(q.DispatchedQty * q.UnitPrice));
                            asss.ForEach(q => q.TotalPrice = asss.Sum(c => c.PriceofItem));
                        }

                        var StockBatchMasterIds = asss.Select(x => x.StockBatchMasterId).Distinct().ToList();
                        StockBatchTransactionManager stockBatchTransactionManager = new StockBatchTransactionManager();
                        var AllStockBatchMastersList = await stockBatchTransactionManager.GetAllStockBatchMastersDataById(StockBatchMasterIds, "C");
                        var ItemNumber = asss.Select(x => x.ItemNumber).Distinct().ToList();
                        var itemMaster = db.itemMasters.Where(x => ItemNumber.Contains(x.Number) && x.WarehouseId == Warehouseid).Select(y => new { y.ItemId, y.Number, y.ItemMultiMRPId, y.WarehouseId, y.PurchaseMinOrderQty }).ToList();
                        asss.ForEach(x =>
                        {
                            x.PurchaseMinOrderQty = itemMaster != null ? itemMaster.Where(y => y.Number == x.ItemNumber).FirstOrDefault().PurchaseMinOrderQty : 0;
                        });
                        foreach (var d in asss)
                        {
                            var CurrentStockBatchCode = AllStockBatchMastersList.Where(x => x.StockBatchMasterId == d.StockBatchMasterId).Select(x => new { x.BatchCode, x.Qty }).FirstOrDefault();
                            d.BatchCode = CurrentStockBatchCode != null ? CurrentStockBatchCode.BatchCode : "";
                            //d.ABCClassification = _objItemClassificationDClist.Where(x => x.ItemNumber == d.ItemNumber).Select(x=>x.Category).FirstOrDefault();
                            //var currentinventory = db.DbCurrentStock.Where(k => k.Deleted != true && k.WarehouseId == d.RequestToWarehouseId && k.CompanyId == compid && k.ItemMultiMRPId == d.ItemMultiMRPId).FirstOrDefault(); //multimrp
                            d.IsCnF = wh.IsCnF;
                            if (CurrentStockBatchCode != null)
                            {
                                //if (currentinventory.ItemMultiMRPId == d.ItemMultiMRPId)
                                //{
                                d.CurrentStock = CurrentStockBatchCode.Qty; //currentinventory.CurrentInventory;//Current Stocl
                                //}
                            }
                            else
                            {
                                d.CurrentStock = 0;
                            }
                        }

                        return Request.CreateResponse(HttpStatusCode.OK, asss);

                    }

                    else
                    {

                        var transferorder = db.TransferWHOrderMasterDB.Where(x => x.TransferOrderId == DetailId).FirstOrDefault();

                        ass = db.TransferWHOrderDetailsDB.Where(x => x.TransferOrderId == DetailId && x.WarehouseId == Warehouseid).ToList();
                        var ItemNumber = ass.Select(x => x.ItemNumber).Distinct().ToList();
                        var itemMaster = db.itemMasters.Where(x => ItemNumber.Contains(x.Number) && x.WarehouseId == Warehouseid).Select(y => new { y.ItemId, y.Number, y.ItemMultiMRPId, y.WarehouseId, y.PurchaseMinOrderQty }).ToList();
                        ass.ForEach(x =>
                        {
                            x.fread = transferorder.fread != null ? transferorder.fread : 0;
                            x.PurchaseMinOrderQty = itemMaster != null ? itemMaster.Where(y => y.Number == x.ItemNumber).FirstOrDefault().PurchaseMinOrderQty : 0;
                            x.PriceofCNFItem = x.TotalQuantity * x.UnitPrice;
                            x.InterStateLimit = states.InterstateAmount.Value;
                            x.IntraStateLimit = states.IntrastateAmount.Value;
                        });
                        //var StockBatchMasterIds = ass.Select(x => x.StockBatchMasterId).Distinct().ToList();
                        //StockBatchTransactionManager stockBatchTransactionManager = new StockBatchTransactionManager();
                        //var AllStockBatchMastersList = await stockBatchTransactionManager.GetAllStockBatchMastersDataById(StockBatchMasterIds, "C");

                        //var manager = new ItemLedgerManager();
                        //List<ItemClassificationDC> objItemClassificationDClist = new List<ItemClassificationDC>();
                        //foreach (var data in ass)
                        //{
                        //    ItemClassificationDC obj = new ItemClassificationDC();
                        //    obj.WarehouseId = data.WarehouseId??0;
                        //    obj.ItemNumber = data.ItemNumber;
                        //    objItemClassificationDClist.Add(obj);

                        //}
                        //List<ItemClassificationDC> _objItemClassificationDClist = await manager.GetItemClassificationsAsync(objItemClassificationDClist);
                        if (!wh.IsCnF)
                        {
                            ass.ForEach(x => x.TotalPrice = ass.Sum(z => z.PriceofItem));   //implementation vinayak.//
                        }
                        else
                        {
                            ass.ForEach(x => x.TotalPrice = ass.Sum(z => z.PriceofCNFItem));   //implementation CNF Case.//
                        }

                        foreach (var d in ass)
                        {
                            //d.ABCClassification = _objItemClassificationDClist.Where(x => x.ItemNumber == d.ItemNumber).Select(x => x.Category).FirstOrDefault();
                            // var CurrentStockBatchCode = AllStockBatchMastersList.Where(x => x.StockBatchMasterId == d.StockBatchMasterId).Select(x => new { x.BatchCode, x.Qty }).FirstOrDefault();
                            // d.BatchCode = CurrentStockBatchCode != null ? CurrentStockBatchCode.BatchCode : "";
                            d.IsCnF = wh.IsCnF;
                            var currentinventory = db.DbCurrentStock.Where(k => k.Deleted != true && k.WarehouseId == d.RequestToWarehouseId && k.CompanyId == compid && k.ItemMultiMRPId == d.ItemMultiMRPId).FirstOrDefault(); //multimrp

                            if (currentinventory != null)
                            {
                                if (currentinventory.ItemMultiMRPId == d.ItemMultiMRPId)
                                {
                                    d.CurrentStock = currentinventory.CurrentInventory;//Current Stocl
                                }
                            }
                            else
                            {
                                d.CurrentStock = 0;
                            }
                        }

                        #region For Ewaybill and Type

                        var Query = (from pd in db.Warehouses
                                     where pd.WarehouseId == To.RequestToWarehouseId || pd.WarehouseId == To.WarehouseId
                                     select new WarehouseDTO
                                     {
                                         StateID = pd.Stateid
                                     }).Distinct().ToList();

                        ass[0].queryCount = Query.Count;
                        #endregion

                        logger.Info("End  Category: ");
                        return Request.CreateResponse(HttpStatusCode.OK, ass);
                    }
                }
                catch (Exception ex)
                {
                    logger.Error("Error in Category " + ex.Message);
                    logger.Info("End  Category: ");
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex.Message);
                }
            }
        }
        #endregion
        #endregion
        #region TransferWHOrderDispatchedDetail
        [HttpGet]
        [ResponseType(typeof(TransferWHOrderDispatchedDetail))]
        [Route("DispatchedDetail")]
        public async System.Threading.Tasks.Task<IEnumerable<TransferWHOrderDispatchedDetail>> DispatchedDetail(int DetailId, int Warehouseid)
        {
            using (AuthContext db = new AuthContext())
            {
                logger.Info("start Category: ");
                List<TransferWHOrderDispatchedDetail> ass = new List<TransferWHOrderDispatchedDetail>();
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
                    ass = db.TransferWHOrderDispatchedDetailDB.Where(x => x.TransferOrderId == DetailId && x.WarehouseId == Warehouseid).ToList();
                    var TransferOrderDetailIds = ass.Select(x => x.TransferOrderDetailId).ToList();
                    var TransferOrderDetail = db.TransferWHOrderDetailsDB.Where(x => TransferOrderDetailIds.Contains(x.TransferOrderDetailId)).ToList();
                    //TransferOrderDetail.ForEach(x =>
                    //{
                    //    var data = ass.FirstOrDefault(y => y.TransferOrderDetailId == x.TransferOrderDetailId);
                    //    if (data != null)
                    //    {
                    //         data.StockBatchMasterId = x.StockBatchMasterId;
                    //    }
                    //});
                    var StockBatchMasterIds = ass.Select(x => x.StockBatchMasterId).Distinct().ToList();
                    StockBatchTransactionManager stockBatchTransactionManager = new StockBatchTransactionManager();
                    var AllStockBatchMastersList = await stockBatchTransactionManager.GetAllStockBatchMastersDataById(StockBatchMasterIds, "C");

                    ass.ForEach(x =>
                    {
                        string BatchCode = AllStockBatchMastersList.Where(y => y.StockBatchMasterId == x.StockBatchMasterId).Select(y => y.BatchCode).FirstOrDefault();
                        x.BatchCode = BatchCode != null ? BatchCode : "";
                    });


                    //var manager = new ItemLedgerManager();
                    //List<ItemClassificationDC> objItemClassificationDClist = new List<ItemClassificationDC>();
                    //foreach (var data in ass)
                    //{
                    //    ItemClassificationDC obj = new ItemClassificationDC();
                    //    obj.WarehouseId = data.WarehouseId ?? 0;
                    //    obj.ItemNumber = data.ItemNumber;
                    //    objItemClassificationDClist.Add(obj);

                    //}
                    //List<ItemClassificationDC> _objItemClassificationDClist = await manager.GetItemClassificationsAsync(objItemClassificationDClist);
                    //foreach (var d in ass)
                    //{
                    //    d.ABCClassification = _objItemClassificationDClist.Where(x => x.ItemNumber == d.ItemNumber).Select(x => x.Category).FirstOrDefault();


                    //}
                    ass.ForEach(q => q.PriceofItem = Convert.ToDouble(q.TotalQuantity * q.NPP));
                    ass.ForEach(x => x.TotalPrice = ass.Sum(z => z.PriceofItem));   //implement Vinayak//
                    logger.Info("End  Category: ");
                    return ass;
                }
                catch (Exception ex)
                {
                    logger.Error("Error in Category " + ex.Message);
                    logger.Info("End  Category: ");
                    return null;
                }
            }

        }

        #endregion
        #region get TransferOrderhistory
        /// <summary>
        /// Get TransferOrder Hisorty
        /// </summary>
        /// <param name="transferOrderId"></param>
        /// <returns></returns>
        [Route("TransferHistory")]
        [HttpGet]
        public dynamic orderhistory(int transferOrderId)
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

                    var data = odd.TransferOrderHistoryDB.Where(x => x.TransferOrderId == transferOrderId).ToList();
                    return data;
                }
                catch (Exception ex)
                {
                    return false;
                }
            }
        }
        #endregion
        #region TransferWHOrderDetails
        [HttpGet]
        [ResponseType(typeof(TransferWHOrderDetails))]
        [Route("TransferItem")]
        public IEnumerable<TransferWHOrderDetails> Getdata(int transferOrderId)
        {
            using (AuthContext db = new AuthContext())
            {
                logger.Info("start Category: ");
                List<TransferWHOrderDetails> ass = new List<TransferWHOrderDetails>();
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
                    ass = db.TransferWHOrderDetailsDB.Where(x => x.TransferOrderId == transferOrderId).ToList();

                    foreach (var item in ass)
                    {
                        item.DispatchedQty = db.TransferWHOrderDispatchedDetailDB.Where(x => x.TransferOrderDetailId == item.TransferOrderDetailId).ToList().Sum(x => x.TotalQuantity);

                    }





                    logger.Info("End  Category: ");
                    return ass;
                }
                catch (Exception ex)
                {
                    logger.Error("Error in Category " + ex.Message);
                    logger.Info("End  Category: ");
                    return null;
                }
            }
        }
        #endregion
        #region TransferWHOrderMaster
        [HttpGet]
        [ResponseType(typeof(TransferWHOrderMaster))]
        [Route("TOMaster")]
        public TransferWHOrderMaster TOMaster(int TransferOrderId, int Warehouseid)
        {
            using (AuthContext db = new AuthContext())
            {
                logger.Info("start Category: ");
                TransferWHOrderMaster ass = new TransferWHOrderMaster();
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
                    ass = db.TransferWHOrderMasterDB.Where(x => x.TransferOrderId == TransferOrderId && x.WarehouseId == Warehouseid).SingleOrDefault();
                    logger.Info("End  Category: ");
                    return ass;
                }
                catch (Exception ex)
                {
                    logger.Error("Error in Category " + ex.Message);
                    logger.Info("End  Category: ");
                    return null;
                }
            }
        }

        #endregion
        /// <summary>
        /// Get Dispeched Master detail
        /// </summary>
        /// <param name="masterId"></param>
        /// <returns></returns>
        [HttpGet]
        [ResponseType(typeof(TransferWHOrderDispatchedMaster))]
        [Route("DispatchedMaster")]
        public HttpResponseMessage DispatchedMaster(int masterId)
        {
            using (AuthContext db = new AuthContext())
            {
                logger.Info("start Category: ");
                TransferWHOrderDispatchedMaster ass = new TransferWHOrderDispatchedMaster();
                DispatchedMasterDTO res;
                try
                {
                    ass = db.TransferWHOrderDispatchedMasterDB.Where(x => x.TransferOrderId == masterId).SingleOrDefault();
                    List<WarehouseDTO> Query = (from pd in db.Warehouses
                                                where pd.WarehouseId == ass.RequestToWarehouseId || pd.WarehouseId == ass.WarehouseId
                                                select new WarehouseDTO
                                                {
                                                    StateID = pd.Stateid
                                                }).Distinct().ToList();

                    res = new DispatchedMasterDTO()
                    {
                        result = ass,
                        Count = Query.Count,
                        Message = "Success."
                    };
                    return Request.CreateResponse(HttpStatusCode.OK, res);
                }
                catch (Exception ex)
                {
                    logger.Error("Error in Category " + ex.Message);
                    logger.Info("End  Category: ");
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "somthing went wrong.");
                }
            }
        }


        [HttpGet]
        [Route("SearchStockitem")]
        public async System.Threading.Tasks.Task<IEnumerable<CurrentStock>> SearchStockitemAsync(string key, int WarehouseId)
        {
            logger.Info("start Item Master: ");

            List<CurrentStock> result = new List<CurrentStock>();
            try
            {

                using (AuthContext db = new AuthContext())
                {
                    result = db.DbCurrentStock.Where(t => t.WarehouseId == WarehouseId && (t.itemname.ToLower().Contains(key.Trim().ToLower()) || t.ItemNumber.ToLower().Contains(key.Trim().ToLower())) && t.Deleted == false).ToList();
                    var manager = new ItemLedgerManager();
                    List<ItemClassificationDC> objItemClassificationDClist = new List<ItemClassificationDC>();
                    foreach (var data in result)
                    {
                        ItemClassificationDC obj = new ItemClassificationDC();
                        obj.WarehouseId = data.WarehouseId;
                        obj.ItemNumber = data.ItemNumber;
                        objItemClassificationDClist.Add(obj);

                    }
                    List<ItemClassificationDC> _objItemClassificationDClist = await manager.GetItemClassificationsAsync(objItemClassificationDClist);
                    foreach (var data in result)
                    {
                        data.ABCClassification = _objItemClassificationDClist.Where(x => x.ItemNumber == data.ItemNumber).Select(x => x.Category).FirstOrDefault();
                        data.ABCClassification = data.ABCClassification == null ? "D" : data.ABCClassification;
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                logger.Error("Error in Item Master " + ex.Message);
                logger.Info("End  Item Master: ");
                return null;
            }
        }

        #region Revoke Transfer Order
        /// <summary>
        /// Revoke Order
        /// </summary>
        /// <param name="TOdata"></param>
        /// <returns></returns>
        [Authorize]
        [Route("RevokeOrder")]
        [HttpGet]
        public HttpResponseMessage RevokeOrder(int TransferOrderId)
        {
            var Message = "";
            var identity = User.Identity as ClaimsIdentity;
            int compid = 0, userid = 0;
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))
                compid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);

            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

            using (AuthContext db = new AuthContext())
            {
                using (var dbContextTransaction = db.Database.BeginTransaction())
                {
                    var peopledata = db.Peoples.Where(x => x.PeopleID == userid && x.Active == true).SingleOrDefault();

                    if (TransferOrderId > 0)
                    {
                        var Ishere = db.TransferWHOrderMasterDB.FirstOrDefault(x => x.TransferOrderId == TransferOrderId);
                        if (Ishere.Status == "Pending")
                        {
                            List<TransferWHOrderDetails> _TransferOrderDetail = db.TransferWHOrderDetailsDB.Where(a => a.TransferOrderId == TransferOrderId).ToList();

                            TransferWHOrderMaster Tom = db.TransferWHOrderMasterDB.Where(a => a.TransferOrderId == TransferOrderId).SingleOrDefault();
                            Tom.Status = "Revoke";
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

        private PaggingData GetTransferOrderMaster(string ProcedureName, string Warehouseid, string StartDate, string EndDate, string Status, int page, int list, int? TransferOrderId)
        {
            PaggingData objpagginData = new PaggingData();
            int Skip = (page - 1) * list;
            int Take = list;
            using (AuthContext context = new AuthContext())
            {
                //var WarehouseidParam = new SqlParameter
                //{
                //    ParameterName = "WarehouseId",
                //    Value = Warehouseid
                //};
                List<int> intList = Warehouseid.Split(',').Select(x => Convert.ToInt32(x)).ToList();
                var ware = new DataTable();
                ware.Columns.Add("IntValue");
                foreach (var item in intList)
                {
                    var dr = ware.NewRow();
                    dr["IntValue"] = item;
                    ware.Rows.Add(dr);
                }
                var WarehouseidParam = new SqlParameter
                {
                    ParameterName = "Warehouseids",
                    SqlDbType = SqlDbType.Structured,
                    TypeName = "dbo.IntValues",
                    Value = ware
                };

                var StartDateParam = new SqlParameter
                {
                    ParameterName = "StartDate",
                    Value = StartDate == null ? DBNull.Value : (object)StartDate
                };

                var EndDateParam = new SqlParameter
                {
                    ParameterName = "EndDate",
                    Value = EndDate == null ? DBNull.Value : (object)EndDate

                };

                var StatusParam = new SqlParameter
                {
                    ParameterName = "Status",
                    Value = Status == null ? DBNull.Value : (object)Status

                };

                var TransferOrderIdParam = new SqlParameter
                {
                    ParameterName = "TransferOrderId",
                    Value = TransferOrderId == null ? DBNull.Value : (object)TransferOrderId

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
                List<TransferOrderMasterDc> objTransferOrderMasterDc = context.Database.SqlQuery<TransferOrderMasterDc>(ProcedureName, WarehouseidParam, StartDateParam,
                EndDateParam, StatusParam, TransferOrderIdParam, SkipParam, TakeParam).ToList();
                objpagginData.total_count = objTransferOrderMasterDc != null && objTransferOrderMasterDc.Any() ? objTransferOrderMasterDc.FirstOrDefault().totalcount : 0;
                objpagginData.ordermaster = objTransferOrderMasterDc;
                return objpagginData;
            }
        }
        #endregion
        [HttpGet]
        [Route("GetTransferWHPercentage")]
        public double GetTransferWHPercentage(int fromWarehouseId, int RequestToWarehouseId)
        {
            double Percantage = 0;
            using (var db = new AuthContext())
            {
                var warehouseId = new SqlParameter("@WarehouseId", fromWarehouseId);
                var requestToWarehouseId = new SqlParameter("@RequestToWarehouseId", RequestToWarehouseId);
                Percantage = db.Database.SqlQuery<double>("EXEC GetTransferWHPercentage @WarehouseId,@RequestToWarehouseId", warehouseId, requestToWarehouseId).FirstOrDefault();
            }
            return Percantage;
        }

        [HttpPost]
        [Route("UpdateFreightForDelivered")]
        public bool UpdateFreightForDelivered(double fread, int id)
        {
            using (var db = new AuthContext())
            {
                bool result = false;
                var data = db.TransferWHOrderDispatchedMasterDB.Where(x => x.TransferOrderId == id && x.Deleted == false).FirstOrDefault();
                var datas = db.TransferWHOrderMasterDB.Where(y => y.TransferOrderId == id).SingleOrDefault();
                if (data != null)
                {
                    if (datas != null)
                    {
                        datas.fread = fread;
                        db.Entry(datas).State = EntityState.Modified;
                        db.Commit();
                    }
                    data.fread = fread;
                    result = true;
                    db.Entry(data).State = EntityState.Modified;
                    db.Commit();
                }
                else
                {
                    result = false;
                }
                return result;
            }
        }

        [HttpGet]
        [Route("GetDeliverdays")]
        public bool GetDeliverdays(int id)
        {
            using (var db = new AuthContext())
            {
                int compid = 0, userid = 0, Warehouse_id = 0;

                var identity = User.Identity as ClaimsIdentity;
                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))
                    compid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                    userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "Warehouseid"))
                    Warehouse_id = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "Warehouseid").Value);
                var uid = new SqlParameter()
                {
                    ParameterName = "@userid",
                    Value = userid
                };
                var res = db.Database.SqlQuery<int>("GetUserRoleForFreight @userid", uid).FirstOrDefault();
                bool result = false;
                if (res > 0)
                {
                    var data = db.TransferOrderHistoryDB.Where(x => x.TransferOrderId == id && x.Status == "Delivered").OrderBy(y => y.CreatedDate).FirstOrDefault();
                    if (data != null)
                    {
                        var newdate = data.CreatedDate.AddDays(7);
                        var todaydate = DateTime.Now;
                        if (newdate >= todaydate)
                        {
                            result = true;
                        }
                    }
                }
                return result;
            }
        }

        public class TOdata
        {
            public int StockId
            {
                get; set;
            }
            public string Itemname
            {
                get; set;
            }
            public int RequestFromWarehouseId { get; set; } //RequestFromWarehouseId
            public int RequestToWarehouseId { get; set; }//RequestToWarehouseId
            public int Noofpics
            {
                get; set;
            }
            public double fread { get; set; }
            public string ABCClassification { get; set; }
            public long StockBatchMasterId { get; set; }

            public double? PriceOfItem { get; set; }

        }
        public class GetTODataDTO
        {
            public int TransferOrderDetailId { get; set; }
            public int TransferOrderId { get; set; }
            public int CompanyId { get; set; }
            public int WarehouseId { get; set; }
            public string WarehouseName { get; set; }
            public string Status { get; set; }
            public int? RequestToWarehouseId { get; set; }
            public string RequestToWarehouseName { get; set; }
            public int ItemId { get; set; }
            public string ItemNumber { get; set; }
            public string ItemHsn { get; set; }
            public double? TotalTaxPercentage { get; set; }
            public double? NPP { get; set; }
            public int TotalQuantity { get; set; }
            public DateTime CreationDate { get; set; }
            public DateTime UpdatedDate { get; set; }
            public string CreatedBy { get; set; }
            public string UpdateBy { get; set; }
            public bool Deleted { get; set; }

            //ItemMultiMRPId 20/03/2019 Harry
            public string itemname { get; set; }
            public string itemBaseName { get; set; }
            public int ItemMultiMRPId { get; set; }
            public double MRP { get; set; }
            public string UnitofQuantity { get; set; }
            public string UOM { get; set; }//Unit of masurement like GM Kg 
            public int userid { get; set; }
            public int CurrentStock { get; set; }
            public double PriceofItem { get; set; }
            public double TotalPrice { get; set; }
            public double DispatchedQty { get; set; }
            public string Type { get; set; }

            public string ABCClassification { get; set; }
            public int PurchaseMinOrderQty { get; set; }
            public string InternalTransferNo { get; set; }
            public DateTime? ITCreatedDate { get; set; }
            public string InternalTransferNoCN { get; set; }
            public DateTime? ITCNCreatedDate { get; set; }
            public string DeliveryChallanNo { get; set; } //use when gstn code of both hub are same
            public DateTime? DCCreatedDate { get; set; }
            public string vehicleType { get; set; }
            public string vehicleNumber { get; set; }
            public double fread { get; set; }
            public string EwaybillNumber { get; set; }
            public long StockBatchMasterId { get; set; }//Sudhir 04-10-2022
            public string BatchCode { get; set; } //Sudhir 04-10-2022
            public double UnitPrice { get; set; }
            public bool IsCnF { get; set; }
            public double PriceofCNFItem { get; set; }
            public double InterStateLimit { get; set; }
            public double IntraStateLimit { get; set; }
        }
        public class DispechedDTO
        {
            public int TransferOrderId { get; set; }
            public string vehicleType { get; set; }
            public string vehicleNumber { get; set; }
            public string TransporterGstin { get; set; } //vishal 23-06-2023
            public string TransporterName { get; set; } //vishal 23-06-2023
            public double fread { get; set; }
            public string EwaybillNumber { get; set; }
            public List<TransferWHOrderDetails> TransferWHOrderDetailss { get; set; }
            public List<BatchITransferDetailDc> BatchITransferDetails { get; set; }

        }

        public class WarehouseDTO
        {
            public int StateID { get; set; }
        }
        public class DispatchedMasterDTO
        {
            public TransferWHOrderDispatchedMaster result { get; set; }

            public int Count { get; set; }
            public string Message { get; set; }
        }

        public class TransferOrderMasterDc
        {
            public int TransferOrderId { get; set; }
            public string Status { get; set; }

            public string WarehouseName { get; set; }
            public string RequestToWarehouseName { get; set; }

            public DateTime? CreationDate { get; set; }

            public int Warehouseid { get; set; }

            public int RequestToWarehouseId { get; set; }

            // Added by Anoop on 18/2/2021
            public double RequestAmount { get; set; }
            public double DispatchAmount { get; set; }
            public string VehicleNo { get; set; }
            public string EwaybillNumber { get; set; }
            public int totalcount { set; get; }
            public int? RequestQty { get; set; }
            public int? DispatchQty { get; set; }
            public string Type { get; set; } // NEW ADD


        }
    }
}