using AngularJSAuthentication.API.Controllers.Base;
using AngularJSAuthentication.API.Helper;
using AngularJSAuthentication.API.Managers;
using AngularJSAuthentication.BusinessLayer.Managers.Transactions.BatchCode;
using AngularJSAuthentication.Common.Helpers;
using AngularJSAuthentication.DataContracts.APIParams;
using AngularJSAuthentication.DataContracts.BatchCode;
using AngularJSAuthentication.DataContracts.constants;
using AngularJSAuthentication.DataContracts.Transaction.Ledger.ItemLedger;
using AngularJSAuthentication.DataContracts.Transaction.Stocks;
using AngularJSAuthentication.Model;
using LinqKit;
using NLog;
using NPOI.OpenXmlFormats.Dml.Diagram;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Transactions;
using System.Web.Http;

namespace AngularJSAuthentication.API.Controllers
{
    [RoutePrefix("api/rtv")]
    public class RTVController : BaseAuthController
    {
        public static Logger logger = LogManager.GetCurrentClassLogger();
        private static TimeZoneInfo INDIAN_ZONE = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
        DateTime indianTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE);
        [Route("")]
        [HttpGet]
        public IEnumerable<RTVGetDTO> Get()
        {
            logger.Info("start City: ");
            using (AuthContext context = new AuthContext())
            {
                List<RTVMaster> ass = new List<RTVMaster>();
                try
                {
                    var identity = User.Identity as ClaimsIdentity;
                    int compid = 0, userid = 0;
                    int Warehouse_id = 0;

                    if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))
                        compid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);

                    if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                        userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

                    if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "Warehouseid"))
                        Warehouse_id = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "Warehouseid").Value);
                    int CompanyId = compid;


                    var query = "select a.[WarehouseId],a.[SupplierId],a.[DepoId], a.[StockType] , b.Name as SupName,c.WarehouseName,d.DepoName,a.Id, a.GSTAmount,a.TotalAmount,e.DisplayName as CreatedBy, a.CreatedDate from" +
                                "[RTVMasters] a join Suppliers b on a.SupplierId = b.SupplierId join " +
                                " [Warehouses] c on a.WarehouseId = c.WarehouseId join DepoMasters d on a.DepoId = d.DepoId" +
                                " join [People] e on a.CreatedbyUserId = e.PeopleID";
                    var newdata = context.Database.SqlQuery<RTVGetDTO>(query).ToList();

                    foreach (RTVGetDTO a in newdata)
                    {
                        List<RTVMasterDetail> dt = context.RTVMasterDetailDB.Where(q => q.RTVMasterDetailId == a.Id).ToList();
                        a.Detail = dt;
                    }
                    logger.Info("End  RTV: ");
                    return newdata.OrderByDescending(a => a.CreatedDate);
                }
                catch (Exception ex)
                {
                    logger.Error("Error in RTV " + ex.Message);
                    logger.Info("End  RTV: ");
                    return null;
                }
            }
        }

        [Route("GetRTV")]
        [HttpPost]
        public async Task<RTVPaginatorReturn> GetRTV(RTVPaginatorDc rtvPaginatorDc)
        {
            RTVPaginatorReturn rtvReturnData = new RTVPaginatorReturn();
            List<RTVMasterGetDc> rtvMasterGetDc = new List<RTVMasterGetDc>();
            using (AuthContext context = new AuthContext())
            {
                var identity = User.Identity as ClaimsIdentity;
                int Warehouse_id = 0;

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "Warehouseid"))
                    Warehouse_id = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "Warehouseid").Value);
                var predicate = PredicateBuilder.True<RTVMasterGetDc>();
                predicate = predicate.And(x => x.Deleted == false);
                if (rtvPaginatorDc.WarehouseIds.Any() )
                {
                    predicate = predicate.And(x => rtvPaginatorDc.WarehouseIds.Contains(x.WarehouseId));
                }
                if (!string.IsNullOrEmpty(rtvPaginatorDc.SupplierCode))
                {
                    predicate = predicate.And(x => x.SupplierCode.Contains(rtvPaginatorDc.SupplierCode));
                }
                if (rtvPaginatorDc.RTVId > 0)
                {
                    predicate = predicate.And(x => x.Id == rtvPaginatorDc.RTVId);
                }
                if (!string.IsNullOrEmpty(rtvPaginatorDc.Status))
                {
                    predicate = predicate.And(x => x.Status == rtvPaginatorDc.Status);
                }

                if (rtvPaginatorDc.StartDate.HasValue)
                {
                    predicate = predicate.And(x => x.CreatedDate >= rtvPaginatorDc.StartDate);
                }
                if (rtvPaginatorDc.EndDate.HasValue)
                {
                    predicate = predicate.And(x => x.CreatedDate <= rtvPaginatorDc.EndDate);
                }
                var query = from rtv in context.RTVMasterDB
                            join sup in context.Suppliers
                            on rtv.SupplierId equals sup.SupplierId
                            join ware in context.Warehouses
                            on rtv.WarehouseId equals ware.WarehouseId
                            join depo in context.DepoMasters
                            on rtv.DepoId equals depo.DepoId
                            join peo in context.Peoples
                            on rtv.CreatedbyUserId equals peo.PeopleID
                            select new RTVMasterGetDc
                            {
                                WarehouseId = rtv.WarehouseId,
                                WarehouseName = ware.WarehouseName,
                                SupplierId = rtv.SupplierId,
                                SupName = sup.Name,
                                DepoId = rtv.DepoId,
                                DepoName = depo.DepoName,
                                StockType = rtv.StockType,
                                Id = rtv.Id,
                                RTVNo = rtv.RTVNo,
                                RTVNoCreatedDate = rtv.RTVNoCreatedDate,
                                GSTAmount = rtv.GSTAmount,
                                TotalAmount = rtv.TotalAmount,
                                CreatedBy = peo.DisplayName,
                                CreatedDate = rtv.CreatedDate,
                                Deleted = rtv.Deleted,
                                Status = rtv.Status,
                                SupplierCode = sup.SUPPLIERCODES,
                                Detail = context.RTVMasterDetailDB.Where(q => q.RTVMasterDetailId == rtv.Id).ToList()

                            };



                rtvReturnData.TotalCount = query.Where(predicate).Count();
                rtvReturnData.rTVMasterGetDcs = query.Where(predicate).OrderByDescending(x => x.Id).Skip((rtvPaginatorDc.CurrentPage - 1) * rtvPaginatorDc.RowsPerPage).Take(rtvPaginatorDc.RowsPerPage).ToList();

                #region Sudhir 07-10-2022
                var StockBatchMasterIds = rtvReturnData.rTVMasterGetDcs.SelectMany(x => x.Detail.Select(y => y.StockBatchMasterId)).ToList();
                StockBatchTransactionManager stockBatchTransactionManager = new StockBatchTransactionManager();
                var AllStockBatchMastersList = await stockBatchTransactionManager.GetAllStockBatchMastersDataById(StockBatchMasterIds, "C");
                rtvReturnData.rTVMasterGetDcs.ForEach(x =>
                {
                    x.Detail.ForEach(z =>
                    {
                        string BatchCode = AllStockBatchMastersList.Where(y => y.StockBatchMasterId == z.StockBatchMasterId).Select(y => y.BatchCode).FirstOrDefault();
                        z.BatchCode = BatchCode != null ? BatchCode : "";
                    });
                });
                #endregion end
                return rtvReturnData;//newdata.OrderByDescending(a => a.CreatedDate);

            }
        }

        [Route("ExportRTV")]
        [HttpPost]
        public List<RTVMasterGetExportDc> ExportRTV(RTVPaginatorDc rtvPaginatorDc)
        {
            List<RTVMasterGetExportDc> rtvMasterGetDc = new List<RTVMasterGetExportDc>();
            using (AuthContext context = new AuthContext())
            {
                var identity = User.Identity as ClaimsIdentity;
                int Warehouse_id = 0;

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "Warehouseid"))
                    Warehouse_id = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "Warehouseid").Value);
                var predicate = PredicateBuilder.True<RTVMasterGetExportDc>();
                predicate = predicate.And(x => x.Deleted == false);
                if (rtvPaginatorDc.WarehouseIds.Count > 0)
                {
                    predicate = predicate.And(x => rtvPaginatorDc.WarehouseIds.Contains(x.WarehouseId));
                }
                if (!string.IsNullOrEmpty(rtvPaginatorDc.SupplierCode))
                {
                    predicate = predicate.And(x => x.SupplierCode.Contains(rtvPaginatorDc.SupplierCode));
                }
                if (rtvPaginatorDc.RTVId > 0)
                {
                    predicate = predicate.And(x => x.Id == rtvPaginatorDc.RTVId);
                }
                if (!string.IsNullOrEmpty(rtvPaginatorDc.Status))
                {
                    predicate = predicate.And(x => x.Status == rtvPaginatorDc.Status);
                }

                if (rtvPaginatorDc.StartDate.HasValue)
                {
                    predicate = predicate.And(x => x.CreatedDate >= rtvPaginatorDc.StartDate);
                }
                if (rtvPaginatorDc.EndDate.HasValue)
                {
                    predicate = predicate.And(x => x.CreatedDate <= rtvPaginatorDc.EndDate);
                }
                var query = from rtv in context.RTVMasterDB
                            join rtvm in context.RTVMasterDetailDB
                            on rtv.Id equals rtvm.RTVMasterDetailId
                            join sup in context.Suppliers
                            on rtv.SupplierId equals sup.SupplierId
                            join ware in context.Warehouses
                            on rtv.WarehouseId equals ware.WarehouseId
                            join depo in context.DepoMasters
                            on rtv.DepoId equals depo.DepoId
                            join peo in context.Peoples
                            on rtv.CreatedbyUserId equals peo.PeopleID
                            select new RTVMasterGetExportDc
                            {
                                WarehouseId = rtv.WarehouseId,
                                WarehouseName = ware.WarehouseName,
                                SupplierId = rtv.SupplierId,
                                SupplierName = sup.Name,
                                DepoId = rtv.DepoId,
                                DepoName = depo.DepoName,
                                StockType = rtv.StockType,
                                Id = rtv.Id,
                                RTVNo = rtv.RTVNo,
                                RTVNoCreatedDate = rtv.RTVNoCreatedDate,
                                GSTAmount = rtv.GSTAmount,
                                TotalAmount = rtv.TotalAmount+rtv.GSTAmount,
                                CreatedBy = peo.DisplayName,
                                CreatedDate = rtv.CreatedDate,
                                Deleted = rtv.Deleted,
                                Status = rtv.Status,
                                SupplierCode = sup.SUPPLIERCODES,
                                ItemName = rtvm.ItemName,
                                ItemPrice= rtvm.NPP,
                                ItemQty = rtvm.Quantity,
                                TaxableAmount = rtv.TotalAmount
                            };

                rtvMasterGetDc = query.Where(predicate).OrderByDescending(x => x.Id).ToList();

                return rtvMasterGetDc;//newdata.OrderByDescending(a => a.CreatedDate);

            }
        }

        [Route("GetRTVDetails")]
        [HttpGet]
        public async Task<RTVMaster> GetRTVDetails(int Id, string StockType, int WarehouseId)
        {
            using (AuthContext context = new AuthContext())
            {
                RTVMaster master = context.RTVMasterDB.Where(x => x.Id == Id).FirstOrDefault();
                if (StockType == "Current")
                {
                    //var query = from det in context.RTVMasterDetailDB
                    //            join curr in context.DbCurrentStock
                    //            on det.ItemMultiMRPId equals curr.ItemMultiMRPId
                    //            where det.ItemNumber == curr.ItemNumber && curr.WarehouseId == WarehouseId
                    //            select new RTVMasterDetail
                    //            {
                    //                Id = det.Id,
                    //                RTVMasterDetailId = det.RTVMasterDetailId,
                    //                RTVNumber = det.RTVNumber,
                    //                ItemId = det.ItemId,
                    //                ItemName = det.ItemName,
                    //                ItemNumber = det.ItemNumber,
                    //                ItemMultiMRPId = det.ItemMultiMRPId,
                    //                MOQ = det.MOQ,
                    //                Quantity = det.Quantity,
                    //                NPP = det.NPP,
                    //                PurchasePrice = det.PurchasePrice,
                    //                GSTAmount = det.GSTAmount,
                    //                Amount = det.Amount,
                    //                CreatedDate = det.CreatedDate,
                    //                Stock = curr.CurrentInventory,
                    //                NoQty = det.Quantity
                    //            };
                    //return rtvMasterDetails= query.ToList();//newdata.OrderByDescending(a => a.CreatedDate);
                    master.detail = context.RTVMasterDetailDB.Where(x => x.RTVMasterDetailId == Id).ToList();
                    var StockBatchMasterIds = master.detail.Select(x => x.StockBatchMasterId).Distinct().ToList();
                    StockBatchTransactionManager stockBatchTransactionManager = new StockBatchTransactionManager();
                    var AllStockBatchMastersList = await stockBatchTransactionManager.GetAllStockBatchMastersDataById(StockBatchMasterIds, "C");
                    foreach (var item in master.detail)
                    {
                        var BatchCode = AllStockBatchMastersList.Where(y => y.StockBatchMasterId == item.StockBatchMasterId).Select(y => new { y.BatchCode, y.Qty }).FirstOrDefault();
                        item.NoQty = item.Quantity;
                        item.Stock = BatchCode != null ? BatchCode.Qty : 0; //context.DbCurrentStock.Where(x => x.ItemMultiMRPId == item.ItemMultiMRPId && x.ItemNumber == item.ItemNumber && x.WarehouseId == WarehouseId).Select(x => x.CurrentInventory).FirstOrDefault();
                        item.BatchCode = BatchCode != null ? BatchCode.BatchCode : "";
                    }
                }
                else
                {
                    //var query = from det in context.RTVMasterDetailDB
                    //            join damage in context.DamageStockDB
                    //            on det.ItemMultiMRPId equals damage.ItemMultiMRPId
                    //            where det.ItemNumber == damage.ItemNumber && damage.WarehouseId == WarehouseId && det.RTVMasterDetailId== Id
                    //            select new RTVMasterDetail
                    //            {
                    //                Id = det.Id,
                    //                RTVMasterDetailId = det.RTVMasterDetailId,
                    //                RTVNumber = det.RTVNumber,
                    //                ItemId = det.ItemId,
                    //                ItemName = det.ItemName,
                    //                ItemNumber = det.ItemNumber,
                    //                ItemMultiMRPId = det.ItemMultiMRPId,
                    //                MOQ = det.MOQ,
                    //                Quantity = det.Quantity,
                    //                NPP = det.NPP,
                    //                PurchasePrice = det.PurchasePrice,
                    //                GSTAmount = det.GSTAmount,
                    //                Amount = det.Amount,
                    //                CreatedDate = det.CreatedDate,
                    //                Stock = damage.DamageInventory,
                    //                NoQty = det.Quantity
                    //            };
                    //rtvMasterDetails = query.ToList();
                    master.detail = context.RTVMasterDetailDB.Where(x => x.RTVMasterDetailId == Id).ToList();
                    var StockBatchMasterIds = master.detail.Select(x => x.StockBatchMasterId).Distinct().ToList();
                    StockBatchTransactionManager stockBatchTransactionManager = new StockBatchTransactionManager();
                    var AllStockBatchMastersList = await stockBatchTransactionManager.GetAllStockBatchMastersDataById(StockBatchMasterIds, "D");
                    foreach (var item in master.detail)
                    {
                        var DamageBatchCode = AllStockBatchMastersList.Where(y => y.StockBatchMasterId == item.StockBatchMasterId).Select(y => new { y.BatchCode, y.Qty }).FirstOrDefault();
                        item.NoQty = item.Quantity;
                        item.Stock = DamageBatchCode != null ? DamageBatchCode.Qty : 0; //context.DamageStockDB.Where(x => x.ItemMultiMRPId == item.ItemMultiMRPId && x.ItemNumber == item.ItemNumber && x.WarehouseId == WarehouseId).Select(x => x.DamageInventory).FirstOrDefault();
                        item.BatchCode = DamageBatchCode != null ? DamageBatchCode.BatchCode : "";
                    }
                    //newdata.OrderByDescending(a => a.CreatedDate);

                }
                return master;

            }


        }
        [Route("postRtv")]
        [HttpPost]
        public HttpResponseMessage add(RTVPOSTDTO item)
        {
            logger.Info("start add City: ");
            var identity = User.Identity as ClaimsIdentity;
            int compid = 0, userid = 0;

            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))
                compid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);

            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

            int CompanyId = compid;
            //DateTime startDate = indianTime.AddDays(-30);
            //DateTime endDate = indianTime;
            List<ItemMaster> itemList = null;
            Warehouse wh = null;
            DepoMaster dp = null;
            int SupplierId = 0;
            var Message = " ";

            if (item != null && item.SupplierId > 0 && item.WarehouseId > 0 && item.Detail.Any())
            {
                using (AuthContext context = new AuthContext())
                {
                    wh = context.Warehouses.Where(a => a.WarehouseId == item.WarehouseId).SingleOrDefault();
                    dp = context.DepoMasters.Where(a => a.DepoId == item.DepoId).SingleOrDefault();
                    //sp = context.Suppliers.Where(a => a.SupplierId == item.SupplierId).SingleOrDefault();
                    SupplierId = item.SupplierId;
                    var ItemNumberS = item.Detail.Select(x => x.ItemNumber).Distinct().ToList();
                    itemList = context.itemMasters.Where(a => a.WarehouseId == wh.WarehouseId && a.NetPurchasePrice > 0 && ItemNumberS.Contains(a.Number)).ToList();
                }
                TransactionOptions option = new TransactionOptions();
                option.IsolationLevel = IsolationLevel.RepeatableRead;
                option.Timeout = TimeSpan.FromSeconds(90);
                using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, option))
                {
                    using (var context = new AuthContext())
                    {
                        RTVMaster rtv = new RTVMaster();

                        rtv.WarehouseId = wh.WarehouseId;
                        if (dp == null)
                            rtv.DepoId = 0;
                        else
                            rtv.DepoId = dp.DepoId;
                        rtv.SupplierId = SupplierId;
                        rtv.StockType = item.StockType;
                        rtv.Status = "Pending";
                        rtv.CreatedbyUserId = userid;
                        rtv.CreatedDate = indianTime;
                        rtv.Deleted = false;
                        rtv.Activate = true;

                        //context.Commit();
                        rtv.detail = new List<RTVMasterDetail>();
                        //List<Object> parameters = new List<object>();
                        //List<EditItemPriceAppDTO> Appdata = new List<EditItemPriceAppDTO>();
                        //string sqlquery = "exec EditItemPriceApp";
                        //parameters.Add(new SqlParameter("@startDate", startDate));
                        //parameters.Add(new SqlParameter("@endDate", endDate));
                        //sqlquery = sqlquery + " @startDate" + ", @endDate";
                        //parameters.Add(new SqlParameter("@warehouseId", item.WarehouseId));
                        //sqlquery = sqlquery + ", @warehouseId";
                        //Appdata = context.Database.SqlQuery<EditItemPriceAppDTO>(sqlquery, parameters.ToArray()).ToList();
                        foreach (RTVDetailDTO s in item.Detail.Where(x => x.Noofset > 0))
                        {
                            double APP_Price = s.App_price;
                            var itm = itemList.Where(a => a.WarehouseId == wh.WarehouseId && a.Number == s.ItemNumber).FirstOrDefault();
                            if (itm != null)
                            {
                                //if (Appdata.Any(x => x.ItemMultiMRPId == s.MultiMrpId))
                                //{
                                //    APP_Price = Appdata.First(x => x.ItemMultiMRPId == s.MultiMrpId).AvgPurPrice;//get App
                                //}
                                //if (itm.POPurchasePrice == 0 || itm.POPurchasePrice == null)
                                //{
                                //    APP_Price = itm.PurchasePrice;
                                //}

                                rtv.detail.Add(new RTVMasterDetail
                                {
                                    ItemMultiMRPId = s.MultiMrpId,
                                    ItemName = s.Itemname,
                                    ItemNumber = s.ItemNumber,
                                    MOQ = itm.MinOrderQty,
                                    NPP = itm.NetPurchasePrice,
                                    PurchasePrice = APP_Price,
                                    Quantity = s.Noofset,
                                    RTVMasterDetailId = rtv.Id,
                                    RTVNumber = rtv.RTVNumber,
                                    Amount = s.Noofset * APP_Price,
                                    GSTAmount = (s.Noofset * APP_Price) * itm.TotalTaxPercentage / 100,
                                    CreatedDate = indianTime,
                                    StockBatchMasterId = s.StockBatchMasterId
                                });
                            }
                            else
                            {
                                Message = "RTV Can't create due to Npp is Zero In ItemMaster";
                                return Request.CreateResponse(HttpStatusCode.OK, Message);
                            }
                            rtv.GSTAmount = rtv.detail.Sum(x => x.GSTAmount);
                            rtv.TotalAmount = rtv.detail.Sum(x => x.Amount);
                        }
                        context.RTVMasterDB.Add(rtv);

                        context.Commit();
                        foreach (var rtvitem in rtv.detail)
                        {
                            rtvitem.RTVMasterDetailId = rtv.Id;
                            context.RTVMasterDetailDB.Add(rtvitem);
                        }

                        context.Commit();
                        scope.Complete();
                        Message = "RTV Create Successfully";

                        #region Stock Code Hide because Rtv hit on Delivered 
                        //foreach (var StockHit in rtv.detail.Where(x => x.Quantity > 0))
                        //{
                        //    int qty = Convert.ToInt32(StockHit.Quantity);
                        //    StockTransactionHelper helper = new StockTransactionHelper();
                        //    PhysicalStockUpdateRequestDc StockTransferToFree = new PhysicalStockUpdateRequestDc();
                        //    StockTransferToFree.ItemMultiMRPId = StockHit.ItemMultiMRPId;
                        //    StockTransferToFree.WarehouseId = rtv.WarehouseId;
                        //    StockTransferToFree.Reason = "RTV";
                        //    StockTransferToFree.Qty = qty;
                        //    if (item.StockType == "Damage")
                        //    {
                        //        StockTransferToFree.SourceStockType = StockTypeTableNames.DamagedStock;
                        //        StockTransferToFree.DestinationStockType = StockTypeTableNames.RTVStock;
                        //        StockTransferToFree.StockTransferType = StockTransferTypeName.PurchaseInventory;
                        //    }
                        //    else
                        //    {
                        //        StockTransferToFree.SourceStockType = StockTypeTableNames.CurrentStocks;
                        //        StockTransferToFree.DestinationStockType = StockTypeTableNames.RTVStock;
                        //        StockTransferToFree.StockTransferType = StockTransferTypeName.PurchaseInventory;
                        //    }
                        //    bool isupdated = helper.TransferBetweenPhysicalStocks(StockTransferToFree, userid, context, scope);
                        //    if (!isupdated)
                        //    {
                        //        scope.Dispose();
                        //        Message = "Failed Due to inventory issue.";
                        //        return Request.CreateResponse(HttpStatusCode.OK, Message);
                        //    }
                        //    else
                        //    {
                        //        IsSuccess = true;
                        //    }
                        //}
                        //if (IsSuccess)
                        //{

                        //    scope.Complete();
                        //    
                        //}
                        #endregion
                    }


                }
            }
            return Request.CreateResponse(HttpStatusCode.OK, Message);
            #region old code

            //logger.Info("start add City: ");
            //TimeZoneInfo INDIAN_ZONE = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
            //DateTime indianTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE);
            //var identity = User.Identity as ClaimsIdentity;
            //int compid = 0, userid = 0;
            //int Warehouse_id = 0;

            //if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))
            //    compid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);

            //if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
            //    userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

            //if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "Warehouseid"))
            //    Warehouse_id = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "Warehouseid").Value);
            //int CompanyId = compid;

            //using (AuthContext context = new AuthContext())
            //{
            //    //using (var dbContextTransaction = context.Database.BeginTransaction())
            //    //{

            //    Warehouse wh = context.Warehouses.Where(a => a.WarehouseId == item.WarehouseId).SingleOrDefault();
            //    DepoMaster dp = context.DepoMasters.Where(a => a.DepoId == item.DepoId).SingleOrDefault();
            //    Supplier sp = context.Suppliers.Where(a => a.SupplierId == item.SupplierId).SingleOrDefault();

            //    RTVMaster rtv = new RTVMaster();
            //    rtv.WarehouseId = wh.WarehouseId;
            //    if (dp == null)
            //        rtv.DepoId = 0;
            //    else
            //        rtv.DepoId = dp.DepoId;

            //    rtv.SupplierId = sp.SupplierId;
            //    rtv.StockType = item.StockType;
            //    rtv.Status = "Pending";
            //    rtv.CreatedbyUserId = userid;
            //    rtv.CreatedDate = indianTime;
            //    rtv.Deleted = false;
            //    rtv.Activate = true;
            //    context.RTVMasterDB.Add(rtv);
            //    context.Commit();

            //    var ItemNumberS = item.Detail.Select(x => x.ItemNumber).Distinct().ToList();
            //    List<ItemMaster> itemList = context.itemMasters.Where(a => a.WarehouseId == wh.WarehouseId && ItemNumberS.Contains(a.Number)).ToList();


            //    foreach (RTVDetailDTO s in item.Detail)
            //    {
            //        ItemMaster itm = itemList.Where(a => a.WarehouseId == wh.WarehouseId && a.Number == s.ItemNumber).FirstOrDefault();

            //        RTVMasterDetail rtvD = new RTVMasterDetail();
            //        rtvD.ItemId = itm.ItemId;
            //        rtvD.ItemMultiMRPId = s.MultiMrpId;
            //        rtvD.ItemName = s.Itemname;
            //        rtvD.ItemNumber = s.ItemNumber;
            //        rtvD.MOQ = itm.MinOrderQty;
            //        rtvD.NPP = itm.NetPurchasePrice;
            //        rtvD.PurchasePrice = itm.PurchasePrice;
            //        rtvD.Quantity = s.Noofset;
            //        rtvD.RTVMasterDetailId = rtv.Id;
            //        rtvD.RTVNumber = rtv.RTVNumber;
            //        rtvD.Amount = rtvD.Quantity * itm.NetPurchasePrice;
            //        rtvD.GSTAmount = rtvD.Amount * itm.TotalTaxPercentage / 100;
            //        rtv.GSTAmount += rtvD.GSTAmount;
            //        rtv.TotalAmount += rtvD.Amount;
            //        rtvD.CreatedDate = indianTime;
            //        context.RTVMasterDetailDB.Add(rtvD);

            //        var RTVNo = new SqlParameter("RTVNo", rtv.Id);
            //        var ItemNumber = new SqlParameter("ItemNumber", rtvD.ItemNumber);
            //        var MultimrpId = new SqlParameter("MultimrpId", rtvD.ItemMultiMRPId);
            //        var Wid = new SqlParameter("Wid", wh.WarehouseId);
            //        var Qty = new SqlParameter("Qty", rtvD.Quantity);
            //        var Supid = new SqlParameter("Supid", sp.SupplierId);
            //        var RTVAmount = new SqlParameter("RTVAmount", rtvD.Amount);
            //        var RTVGstAmount = new SqlParameter("RTVGstAmount", rtvD.GSTAmount);
            //        var StockType = new SqlParameter("StockType", item.StockType);
            //        var UserId = new SqlParameter("UserId", userid);
            //        var ordHistories = context.Database.ExecuteSqlCommand("exec SP_RTV_STOCK @RTVNo,@ItemNumber,@MultimrpId,@Wid,@Qty,@Supid,@RTVAmount,@RTVGstAmount,@StockType,@UserId", RTVNo, ItemNumber, MultimrpId, Wid, Qty, Supid, RTVAmount, RTVGstAmount, StockType, UserId);

            //        if (ordHistories != 2)
            //        {
            //            //dbContextTransaction.Rollback();
            //            return Request.CreateResponse(HttpStatusCode.OK, "Failed Due to inventory issue.");
            //        }
            //    }
            //    RTVMaster sa = context.RTVMasterDB.Where(a => a.Id == rtv.Id).SingleOrDefault();
            //    sa.GSTAmount = rtv.GSTAmount;
            //    sa.TotalAmount = rtv.TotalAmount;
            //    sa.Status = "Done";

            //    var GST = GSTDeduction();
            //    var SupllierDeduction = false; /*SupplierPaymentDeduction(sp.SupplierId, rtv.TotalAmount, rtv.Id);*/

            //    if (GST != false && SupllierDeduction != false)
            //    {
            //        sa.IsLedgerAffected = true;
            //        context.Entry(sa).State = EntityState.Modified;
            //        context.Commit();
            //        //dbContextTransaction.Commit();
            //        return Request.CreateResponse(HttpStatusCode.OK, "RTV Done.");
            //    }
            //    else
            //    {
            //        sa.IsLedgerAffected = false;
            //        context.Entry(sa).State = EntityState.Modified;
            //        context.Commit();
            //        //dbContextTransaction.Commit();
            //        return Request.CreateResponse(HttpStatusCode.OK, "RTV Done without supplier ledger affected.");
            //    }

            //    //}
            //}
            #endregion
        }
        [Route("DeliveredRtv")]
        [HttpPost]
        public string DeliveredRtv(RTVMasterGetDc item)
        {
            var identity = User.Identity as ClaimsIdentity;
            int compid = 0, userid = 0;

            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))
                compid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);

            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

            int CompanyId = compid;

            var Message = " ";
            bool IsSuccess = false;
            RTVMaster master = null; ;
            if (item != null && item.SupplierId > 0 && item.WarehouseId > 0 && item.Detail.Any())
            {

                using (var context = new AuthContext())
                {
                    master = context.RTVMasterDB.Where(x => x.Id == item.Id).FirstOrDefault();
                    master.detail = context.RTVMasterDetailDB.Where(x => x.RTVMasterDetailId == item.Id).ToList();
                }
                if (master.Status == "Dispatched")
                {

                    TransactionOptions option = new TransactionOptions();
                    option.IsolationLevel = IsolationLevel.RepeatableRead;
                    option.Timeout = TimeSpan.FromSeconds(90);
                    using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, option))
                    {
                        using (var context = new AuthContext())
                        {
                            var warehouse = context.Warehouses.FirstOrDefault(x => x.WarehouseId == master.WarehouseId);

                            if (warehouse.IsStopCurrentStockTrans)
                                return "Inventory Transactions are currently disabled for this warehouse... Please try after some time";


                            master.Status = "Delivered";
                            master.UpdatedDate = DateTime.Now;
                            master.UpdatedByUserId = userid;
                            foreach (var itemD in master.detail.Where(x => x.Quantity >= 0))
                            {
                                var rtvItemdc = item.Detail.Where(x => x.Id == itemD.Id).FirstOrDefault();
                                if (rtvItemdc.Quantity > 0)
                                {
                                    var TotalTaxPercentage = context.itemMasters.Where(a => a.WarehouseId == master.WarehouseId && a.Number == itemD.ItemNumber).Select(x => x.TotalTaxPercentage).FirstOrDefault();
                                    itemD.Quantity = rtvItemdc.Quantity;
                                    itemD.Amount = rtvItemdc.Quantity * itemD.PurchasePrice;
                                    itemD.GSTAmount = (rtvItemdc.Quantity * itemD.PurchasePrice) * TotalTaxPercentage / 100;
                                }
                                else if (rtvItemdc.Quantity == 0)
                                {
                                    itemD.Quantity = 0;
                                    itemD.Amount = 0;
                                }

                            }
                            master.GSTAmount = master.detail.Where(x => x.Quantity > 0).Sum(x => x.GSTAmount);
                            master.TotalAmount = master.detail.Where(x => x.Quantity > 0).Sum(x => x.Amount);
                            foreach (var itemdrtv in master.detail)
                            {
                                context.Entry(itemdrtv).State = EntityState.Modified;
                            }
                            // generation of rtv no
                            string RTVNo = " ";
                            if (master.WarehouseId != 67 && master.WarehouseId != 80)
                            {
                                RTVNo = context.Database.SqlQuery<string>("EXEC spGetCurrentNumber 'RtvNo', " + warehouse.Stateid).FirstOrDefault();
                                master.RTVNoCreatedDate = DateTime.Now;
                            }
                            master.RTVNo = RTVNo;
                            context.Entry(master).State = EntityState.Modified;
                            context.Commit();
                            string TxnType = null;
                            foreach (var StockHit in master.detail.Where(x => x.Quantity > 0).GroupBy(x => x.ItemMultiMRPId))
                            {
                                StockTransactionHelper helper = new StockTransactionHelper();
                                PhysicalStockUpdateRequestDc StockTransferToFree = new PhysicalStockUpdateRequestDc();
                                StockTransferToFree.ItemMultiMRPId = StockHit.FirstOrDefault().ItemMultiMRPId;
                                StockTransferToFree.WarehouseId = master.WarehouseId;
                                StockTransferToFree.Reason = "RTV" + " (RTVNo: " + master.Id + ")";
                                StockTransferToFree.Qty = Convert.ToInt32(StockHit.Sum(z => z.Quantity));

                                if (item.StockType == "Damage")
                                {
                                    StockTransferToFree.SourceStockType = StockTypeTableNames.ReservedStock;
                                    StockTransferToFree.DestinationStockType = StockTypeTableNames.RTVStock;
                                    StockTransferToFree.StockTransferType = StockTransferTypeName.PurchaseInventory;
                                    TxnType = "RTVOutDamage";
                                }
                                else if (item.StockType == "Current")
                                {
                                    StockTransferToFree.SourceStockType = StockTypeTableNames.ReservedStock;
                                    StockTransferToFree.DestinationStockType = StockTypeTableNames.RTVStock;
                                    StockTransferToFree.StockTransferType = StockTransferTypeName.PurchaseInventory;
                                    TxnType = "RTVOutCurrent";

                                }
                                else if (item.StockType == "Clearance")
                                {
                                    StockTransferToFree.SourceStockType = StockTypeTableNames.ReservedStock;
                                    StockTransferToFree.DestinationStockType = StockTypeTableNames.RTVStock;
                                    StockTransferToFree.StockTransferType = StockTransferTypeName.PurchaseInventory;
                                    TxnType = "RTVOutClearance";

                                }
                                else if (item.StockType == "NonSellableStocks")
                                {
                                    StockTransferToFree.SourceStockType = StockTypeTableNames.ReservedStock;
                                    StockTransferToFree.DestinationStockType = StockTypeTableNames.RTVStock;
                                    StockTransferToFree.StockTransferType = StockTransferTypeName.PurchaseInventory;
                                    TxnType = "RTVOutNonSellableStocks";

                                }

                                bool isupdated = helper.TransferBetweenPhysicalStocks(StockTransferToFree, userid, context, scope);


                                if (!isupdated)
                                {
                                    scope.Dispose();
                                    Message = "Failed Due to inventory issue.";
                                    return Message;
                                }
                                else { IsSuccess = true; }

                            }

                            BatchMasterManager batchMasterManager = new BatchMasterManager();
                            var StockTxnType = context.StockTxnTypeMasters.FirstOrDefault(x => x.IsActive && x.StockTxnType == TxnType && x.IsDeleted == false);

                            //foreach (var StockHit in master.detail.Where(x => x.Quantity > 0))
                            //{
                            //    int qty = Convert.ToInt32(StockHit.Quantity);
                            //    List<TransferOrderItemBatchMasterDc> TransferOrderItemBatchMaster = new List<TransferOrderItemBatchMasterDc>();
                            //    TransferOrderItemBatchMaster.Add(new TransferOrderItemBatchMasterDc()
                            //    {

                            //        ItemMultiMRPId = StockHit.ItemMultiMRPId,
                            //        Qty = qty * (-1),
                            //        StockBatchMasterId = StockHit.StockBatchMasterId,
                            //        WarehouseId = master.WarehouseId,
                            //        ObjectId = master.Id,
                            //        ObjectIdDetailId = StockHit.Id,

                            //    });
                            //    bool batchRes = batchMasterManager.AddQty(TransferOrderItemBatchMaster, context, userid, StockTxnType.Id);
                            //    if (!batchRes)
                            //    {
                            //        scope.Dispose();
                            //        Message = "Failed Due to inventory issue.";
                            //        return Message;
                            //    }
                            //    else { IsSuccess = true; }

                            //}
                            if (IsSuccess)
                            {

                                RTVReturnHelper Ladgerhelper = new RTVReturnHelper();
                                bool IsLadgerEffect = Ladgerhelper.MakeLedgerEntries(master.Id, context, userid, master.CreatedDate);
                                if (IsLadgerEffect)
                                {
                                    IsSuccess = true;
                                }
                                else
                                {
                                    IsSuccess = false;
                                    Message = "Not Dispatched RTV supplier ledger not affected correctly";
                                    return Message;
                                }
                            }

                            if (IsSuccess)
                            {
                                context.Commit();
                                scope.Complete();
                                Message = "RTV Done supplier ledger affected";


                                #region Insert in FIFO
                                if (ConfigurationManager.AppSettings["LiveFIFO"] == "1")
                                {
                                    if (item.StockType == "Current")
                                    {
                                        List<OutDc> items = master.detail.Where(x => x.NoQty > 0).Select(x => new OutDc
                                        {
                                            ItemMultiMrpId = x.ItemMultiMRPId,
                                            WarehouseId = master.WarehouseId,
                                            Destination = "PO Return",
                                            CreatedDate = indianTime,
                                            ObjectId = master.Id,
                                            Qty = Convert.ToInt32(x.Quantity),
                                            SellingPrice = x.PurchasePrice,
                                        }).ToList();
                                        foreach (var it in items)
                                        {
                                            RabbitMqHelper rabbitMqHelper = new RabbitMqHelper();
                                            rabbitMqHelper.Publish("PoReturn", it);
                                        }
                                    }
                                }
                                #endregion

                            }
                            else
                            {
                                scope.Dispose();
                                Message = "RTV not created!!";
                            }
                        }

                    }

                }
            }
            return Message;

        }
        [Route("RTVCancel")]
        [HttpPut]
        public string CancelRtv(RTVMasterGetDc item)
        {
            string Message = "";
            logger.Info("start add City: ");
            var identity = User.Identity as ClaimsIdentity;
            int compid = 0, userid = 0;

            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))
                compid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);

            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

            int CompanyId = compid;


            bool IsSuccess = false;
            RTVMaster master = null;
            TransactionOptions option = new TransactionOptions();
            option.IsolationLevel = IsolationLevel.RepeatableRead;
            option.Timeout = TimeSpan.FromSeconds(90);
            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, option))
            {
                using (var context = new AuthContext())
                {
                    master = context.RTVMasterDB.Where(x => x.Id == item.Id).FirstOrDefault();
                    master.detail = context.RTVMasterDetailDB.Where(x => x.RTVMasterDetailId == item.Id).ToList();
                    if(item.Status == "RTV Canceled")
                    {
                        string TxnType = null;
                        if(master.Status == "Dispatched")
                        {
                            foreach (var StockHit in master.detail.Where(x => x.Quantity > 0).GroupBy(x => x.ItemMultiMRPId))
                            {
                                StockTransactionHelper helper = new StockTransactionHelper();
                                PhysicalStockUpdateRequestDc StockTransferToFree = new PhysicalStockUpdateRequestDc();
                                StockTransferToFree.ItemMultiMRPId = StockHit.FirstOrDefault().ItemMultiMRPId;
                                StockTransferToFree.WarehouseId = master.WarehouseId;
                                StockTransferToFree.Reason = "RTV" +" (RTVNo: " + master.Id +")";
                                StockTransferToFree.Qty = Convert.ToInt32(StockHit.Sum(z => z.Quantity));

                                if (item.StockType == "Damage")
                                {
                                    StockTransferToFree.SourceStockType = StockTypeTableNames.ReservedStock;
                                    StockTransferToFree.DestinationStockType = StockTypeTableNames.DamagedStock;
                                    StockTransferToFree.StockTransferType = StockTransferTypeName.PurchaseInventory;
                                    TxnType = "RTVOutDamage";
                                }
                                else if (item.StockType == "Current")
                                {
                                    StockTransferToFree.SourceStockType = StockTypeTableNames.ReservedStock;
                                    StockTransferToFree.DestinationStockType = StockTypeTableNames.CurrentStocks;
                                    StockTransferToFree.StockTransferType = StockTransferTypeName.PurchaseInventory;
                                    TxnType = "RTVOutCurrent";

                                }
                                else if (item.StockType == "Clearance")
                                {
                                    StockTransferToFree.SourceStockType = StockTypeTableNames.ReservedStock;
                                    StockTransferToFree.DestinationStockType = StockTypeTableNames.ClearanceStockNews;
                                    StockTransferToFree.StockTransferType = StockTransferTypeName.PurchaseInventory;
                                    TxnType = "RTVOutClearance";

                                }
                                else if (item.StockType == "NonSellableStocks")
                                {
                                    StockTransferToFree.SourceStockType = StockTypeTableNames.ReservedStock;
                                    StockTransferToFree.DestinationStockType = StockTypeTableNames.NonSellableStock;
                                    StockTransferToFree.StockTransferType = StockTransferTypeName.PurchaseInventory;
                                    TxnType = "RTVOutNonSellableStocks";

                                }

                                bool isupdated = helper.TransferBetweenPhysicalStocks(StockTransferToFree, userid, context, scope);

                                if (!isupdated)
                                {
                                    scope.Dispose();
                                    Message = "Failed Due to inventory issue.";
                                    return Message;
                                }
                                else { IsSuccess = true; }
                            }

                            BatchMasterManager batchMasterManager = new BatchMasterManager();
                            var StockTxnType = context.StockTxnTypeMasters.FirstOrDefault(x => x.IsActive && x.StockTxnType == TxnType && x.IsDeleted == false);

                            foreach (var StockHit in master.detail.Where(x => x.Quantity > 0))
                            {
                                int qty = Convert.ToInt32(StockHit.Quantity);
                                List<TransferOrderItemBatchMasterDc> TransferOrderItemBatchMaster = new List<TransferOrderItemBatchMasterDc>();
                                TransferOrderItemBatchMaster.Add(new TransferOrderItemBatchMasterDc()
                                {

                                    ItemMultiMRPId = StockHit.ItemMultiMRPId,
                                    Qty = qty * (1),
                                    StockBatchMasterId = StockHit.StockBatchMasterId,
                                    WarehouseId = master.WarehouseId,
                                    ObjectId = master.Id,
                                    ObjectIdDetailId = StockHit.Id,

                                });
                                bool batchRes = batchMasterManager.AddQty(TransferOrderItemBatchMaster, context, userid, StockTxnType.Id);
                                if (!batchRes)
                                {
                                    scope.Dispose();
                                    Message = "Failed Due to inventory issue.";
                                    return Message;
                                }
                                else
                                {
                                    IsSuccess = true;
                                }
                            }
                        }
                        else
                        {
                            if (item != null && item.SupplierId > 0 && item.WarehouseId > 0 && item.Detail.Any())
                            {
                                master.Status = item.Status;
                                master.UpdatedDate = DateTime.Now;
                                master.UpdatedByUserId = userid;
                                context.Entry(master).State = EntityState.Modified;
                                if (context.Commit() > 0)
                                {
                                    scope.Complete();
                                    IsSuccess = true;
                                    return Message = "RTV Cancel Done";
                                }
                            }
                        }
                        if (IsSuccess)
                        {
                            if (item != null && item.SupplierId > 0 && item.WarehouseId > 0 && item.Detail.Any())
                            {
                                master.Status = item.Status;
                                master.UpdatedDate = DateTime.Now;
                                master.UpdatedByUserId = userid;
                                context.Entry(master).State = EntityState.Modified;
                                if (context.Commit() > 0)
                                {
                                    scope.Complete();
                                    IsSuccess = true;
                                    return Message = "RTV Cancel Done";
                                }
                            }
                        }
                        else
                        {
                            scope.Dispose();
                            Message = "RTV Not Cancel!!";
                        }
                    }
                }
                return Message;
            }
        }


        [Route("DispatchRTV")]
        [HttpPut]
        public string DispatchRTV(RTVMasterGetDc item)
        {
            string Message = "";
            logger.Info("start add City: ");
            var identity = User.Identity as ClaimsIdentity;
            int compid = 0, userid = 0;

            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))
                compid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);

            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

            int CompanyId = compid;


            bool IsSuccess = false;
            RTVMaster master = null;
            TransactionOptions option = new TransactionOptions();
            option.IsolationLevel = IsolationLevel.RepeatableRead;
            option.Timeout = TimeSpan.FromSeconds(90);
            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, option))
            {
                using (var context = new AuthContext())
                {
                    if (item.Status == "Dispatched")
                    {
                        master = context.RTVMasterDB.Where(x => x.Id == item.Id).FirstOrDefault();
                        master.detail = context.RTVMasterDetailDB.Where(x => x.RTVMasterDetailId == item.Id).ToList();

                        var warehouse = context.Warehouses.FirstOrDefault(x => x.WarehouseId == master.WarehouseId);

                        if (warehouse.IsStopCurrentStockTrans)
                            return "Inventory Transactions are currently disabled for this warehouse... Please try after some time";


                        master.Status = item.Status;
                        master.UpdatedDate = DateTime.Now;
                        master.UpdatedByUserId = userid;
                        foreach (var itemD in master.detail.Where(x => x.Quantity >= 0))
                        {
                            var rtvItemdc = item.Detail.Where(x => x.Id == itemD.Id).FirstOrDefault();
                            if (rtvItemdc.Quantity > 0)
                            {
                                var TotalTaxPercentage = context.itemMasters.Where(a => a.WarehouseId == master.WarehouseId && a.Number == itemD.ItemNumber).Select(x => x.TotalTaxPercentage).FirstOrDefault();
                                itemD.Quantity = rtvItemdc.Quantity;
                                itemD.Amount = rtvItemdc.Quantity * itemD.PurchasePrice;
                                itemD.GSTAmount = (rtvItemdc.Quantity * itemD.PurchasePrice) * TotalTaxPercentage / 100;
                            }
                            else if (rtvItemdc.Quantity == 0)
                            {
                                itemD.Quantity = 0;
                                itemD.Amount = 0;
                            }

                        }
                        master.GSTAmount = master.detail.Where(x => x.Quantity > 0).Sum(x => x.GSTAmount);
                        master.TotalAmount = master.detail.Where(x => x.Quantity > 0).Sum(x => x.Amount);
                        foreach (var itemdrtv in master.detail)
                        {
                            context.Entry(itemdrtv).State = EntityState.Modified;
                        }
                        // generation of rtv no
                        string RTVNo = " ";
                        if (master.WarehouseId != 67 && master.WarehouseId != 80)
                        {
                            RTVNo = context.Database.SqlQuery<string>("EXEC spGetCurrentNumber 'RtvNo', " + warehouse.Stateid).FirstOrDefault();
                            master.RTVNoCreatedDate = DateTime.Now;
                        }
                        master.RTVNo = RTVNo;
                        context.Entry(master).State = EntityState.Modified;
                        context.Commit();
                        string TxnType = null;
                        foreach (var StockHit in master.detail.Where(x => x.Quantity > 0).GroupBy(x => x.ItemMultiMRPId))
                        {
                            StockTransactionHelper helper = new StockTransactionHelper();
                            PhysicalStockUpdateRequestDc StockTransferToFree = new PhysicalStockUpdateRequestDc();
                            StockTransferToFree.ItemMultiMRPId = StockHit.FirstOrDefault().ItemMultiMRPId;
                            StockTransferToFree.WarehouseId = master.WarehouseId;
                            StockTransferToFree.Reason = "RTV" + " (RTVNo: " + master.Id + ")";
                            StockTransferToFree.Qty = Convert.ToInt32(StockHit.Sum(z => z.Quantity));
                            if (item.StockType == "Damage")
                            {
                                StockTransferToFree.SourceStockType = StockTypeTableNames.DamagedStock;
                                StockTransferToFree.DestinationStockType = StockTypeTableNames.ReservedStock;
                                StockTransferToFree.StockTransferType = StockTransferTypeName.PurchaseInventory;
                                TxnType = "RTVOutDamage";
                            }
                            else if (item.StockType == "Current")
                            {
                                StockTransferToFree.SourceStockType = StockTypeTableNames.CurrentStocks;
                                StockTransferToFree.DestinationStockType = StockTypeTableNames.ReservedStock;
                                StockTransferToFree.StockTransferType = StockTransferTypeName.PurchaseInventory;
                                TxnType = "RTVOutCurrent";

                            }
                            else if (item.StockType == "Clearance")
                            {
                                StockTransferToFree.SourceStockType = StockTypeTableNames.ClearanceStockNews;
                                StockTransferToFree.DestinationStockType = StockTypeTableNames.ReservedStock;
                                StockTransferToFree.StockTransferType = StockTransferTypeName.PurchaseInventory;
                                TxnType = "RTVOutClearance";

                            }
                            else if (item.StockType == "NonSellableStocks")
                            {
                                StockTransferToFree.SourceStockType = StockTypeTableNames.NonSellableStock;
                                StockTransferToFree.DestinationStockType = StockTypeTableNames.ReservedStock;
                                StockTransferToFree.StockTransferType = StockTransferTypeName.PurchaseInventory;
                                TxnType = "RTVOutNonSellableStocks";

                            }

                            bool isupdated = helper.TransferBetweenPhysicalStocks(StockTransferToFree, userid, context, scope);


                            if (!isupdated)
                            {
                                scope.Dispose();
                                Message = "Failed Due to inventory issue.";
                                return Message;
                            }
                            else { IsSuccess = true; }
                        }

                        BatchMasterManager batchMasterManager = new BatchMasterManager();
                        var StockTxnType = context.StockTxnTypeMasters.FirstOrDefault(x => x.IsActive && x.StockTxnType == TxnType && x.IsDeleted == false);

                        foreach (var StockHit in master.detail.Where(x => x.Quantity > 0))
                        {
                            int qty = Convert.ToInt32(StockHit.Quantity);
                            List<TransferOrderItemBatchMasterDc> TransferOrderItemBatchMaster = new List<TransferOrderItemBatchMasterDc>();
                            TransferOrderItemBatchMaster.Add(new TransferOrderItemBatchMasterDc()
                            {

                                ItemMultiMRPId = StockHit.ItemMultiMRPId,
                                Qty = qty * (-1),
                                StockBatchMasterId = StockHit.StockBatchMasterId,
                                WarehouseId = master.WarehouseId,
                                ObjectId = master.Id,
                                ObjectIdDetailId = StockHit.Id,

                            });
                            bool batchRes = batchMasterManager.AddQty(TransferOrderItemBatchMaster, context, userid, StockTxnType.Id);
                            if (!batchRes)
                            {
                                scope.Dispose();
                                Message = "Failed Due to inventory issue.";
                                return Message;
                            }
                            else
                            {
                                IsSuccess = true;
                            }
                        }
                        if (IsSuccess)
                        {
                            context.Commit();
                            scope.Complete();
                            return Message = "RTV Status Update Successfully";
                        }
                        else
                        {
                            scope.Dispose();
                            Message = "RTV not created!!";
                        }
                    }
                }
                return Message;
            }

        }

        public bool GSTDeduction()
        {
            return true;
        }
        public bool SupplierPaymentDeduction(int sid, double tamt, int rtvid)
        {
            TimeZoneInfo INDIAN_ZONE = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
            DateTime indianTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE);
            using (AuthContext db = new AuthContext())
            {
                FullSupplierPaymentData fsp = db.FullSupplierPaymentDataDB.Where(a => a.SupplierId == sid && a.Deleted != true).SingleOrDefault();

                if (fsp != null)
                {
                    fsp.InVoiceRemainingAmount = fsp.InVoiceRemainingAmount - tamt;
                    db.Entry(fsp).State = EntityState.Modified;

                    SupplierPaymentData sp = new SupplierPaymentData();
                    sp.CompanyId = fsp.CompanyId;
                    sp.WarehouseId = fsp.WarehouseId;
                    sp.Perticular = "(-) RTV Deduction";
                    sp.InVoiceNumber = rtvid.ToString();
                    sp.SupplierId = fsp.SupplierId;
                    sp.SupplierName = fsp.SupplierName;
                    sp.ClosingBalance = fsp.InVoiceRemainingAmount;
                    sp.CreditInVoiceAmount = tamt;
                    sp.DebitInvoiceAmount = 0;
                    sp.VoucherType = "Return Purchase";
                    sp.PaymentStatusCorD = "Debit";
                    sp.PaymentType = "Return Stock";
                    sp.InVoiceDate = indianTime;
                    sp.CreatedDate = indianTime;
                    sp.UpdatedDate = indianTime;
                    sp.Deleted = false;
                    sp.active = true;
                    db.SupplierPaymentDataDB.Add(sp);
                    db.Commit();
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        //[HttpGet]
        //[Route("GetCurrentStockItem")]
        //public List<CurrentStock> GetCurrentStockItem(string key, int Warehouseid , RTVGetDTO data)
        //{
        //    using (var authContext = new AuthContext())
        //    {
        //        ass = authContext.DamageStockDB.Where(t => t.WarehouseId ==) && t.Deleted == false).ToList();

        //        return ass;
        //    }
        //}

        [HttpGet]
        [Route("SerachItemStockForRTV")]
        public IEnumerable<RTVDC> SerachItemStockForRTV(string key, int Warehouseid, string StockType)
        {
            List<RTVDC> _result = new List<RTVDC>();
            var identity = User.Identity as ClaimsIdentity;
            int compid = 0, userid = 0;
            // Access claims
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))
                compid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);

            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
            int CompanyId = compid;
            logger.Info("User ID : {0} , Company Id : {1}", compid, userid);

            using (AuthContext db = new AuthContext())
            {
                if (Warehouseid > 0)
                {
                    var Wid = new SqlParameter("@Warehouseid", Warehouseid );
                    var Key = new SqlParameter("@key", key);
                    var ST = new SqlParameter("@StockType", StockType);


                    _result = db.Database.SqlQuery<RTVDC>("exec SerachItemStockForRTV @Warehouseid,@key,@StockType", Key, Wid, ST).ToList();

                }
                return _result;
            }
        }
        [HttpGet]
        [Route("SerachApp_priceForRTV")]
        public double SearchApp_price(int WarehouseId, int MultiMrpId)
        {
            double APP_Price = 0;
            try
            {
                DateTime startDate = indianTime.AddDays(-30);
                DateTime endDate = indianTime;

                List<Object> parameters = new List<object>();
                List<EditItemPriceAppDTO> Appdata = new List<EditItemPriceAppDTO>();
                string sqlquery = "exec EditItemPriceApp";
                parameters.Add(new SqlParameter("@startDate", startDate));
                parameters.Add(new SqlParameter("@endDate", endDate));
                sqlquery = sqlquery + " @startDate" + ", @endDate";
                parameters.Add(new SqlParameter("@warehouseId", WarehouseId));
                sqlquery = sqlquery + ", @warehouseId";
                using (var context = new AuthContext())
                {
                    Appdata = context.Database.SqlQuery<EditItemPriceAppDTO>(sqlquery, parameters.ToArray()).ToList();
                    if (Appdata.Any(x => x.ItemMultiMRPId == MultiMrpId))
                    {
                        APP_Price = Appdata.First(x => x.ItemMultiMRPId == MultiMrpId).AvgPurPrice;//get App
                    }
                    if (APP_Price == 0)
                    {
                        var PurchasePrice = context.itemMasters.Where(a => a.WarehouseId == WarehouseId && a.NetPurchasePrice > 0 && a.ItemMultiMRPId == MultiMrpId).FirstOrDefault()?.PurchasePrice;
                        APP_Price = PurchasePrice == null ? 0 : PurchasePrice.Value;
                    }

                }
            }
            catch (Exception ex)
            {
                APP_Price = 0;
            }
            return APP_Price;
        }



    }
    public class RTVGetDTO
    {
        public int Id { get; set; }
        public int WarehouseId { get; set; }
        public int SupplierId { get; set; }
        public int DepoId { get; set; }
        public string StockType { get; set; }
        public string SupName { get; set; }
        public string WarehouseName { get; set; }
        public string DepoName { get; set; }
        public double GSTAmount { get; set; }
        public double TotalAmount { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }

        public List<RTVMasterDetail> Detail { get; set; }
    }
    public class RTVPOSTDTO
    {
        public int WarehouseId { get; set; }
        public int SupplierId { get; set; }
        public int DepoId { get; set; }
        public string StockType { get; set; }
        public List<RTVDetailDTO> Detail { get; set; }
    }

    public class RTVDetailDTO
    {
        public int StockId { get; set; }
        public string Itemname { get; set; }
        public string ItemNumber { get; set; }
        public int Noofset { get; set; }
        public int MultiMrpId { get; set; }
        public int CurrentInventory { get; set; }
        public double App_price { get; set; }
        public long StockBatchMasterId { get; set; }
    }

    public class RTVDC
    {
        public int WarehouseId { get; set; }
        public string itemname { get; set; }
        public long StockId { get; set; }
        public int CurrentInventory { get; set; }
        public string ItemNumber { get; set; }
        public int MultiMrpId { get; set; }
    }
}










