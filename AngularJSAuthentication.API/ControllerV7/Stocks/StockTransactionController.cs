using AgileObjects.AgileMapper;
using AngularJSAuthentication.API.Helper;
using AngularJSAuthentication.API.Managers;
using AngularJSAuthentication.Common.Helpers;
using AngularJSAuthentication.Controllers;
using AngularJSAuthentication.DataContracts.constants;
using AngularJSAuthentication.DataContracts.Transaction.Ledger.ItemLedger;
using AngularJSAuthentication.DataContracts.Transaction.Stocks;
using AngularJSAuthentication.Model;
using AngularJSAuthentication.Model.Stocks;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Transactions;
using System.Web.Http;

namespace AngularJSAuthentication.API.ControllerV7.Stocks
{
    [RoutePrefix("api/StockTransaction")]
    public class StockTransactionController : BaseApiController
    {
        [Route("GetAllStocks/{warehouseId}/{itemMultiMRPId}")]
        [HttpGet]
        public List<StockNameDc> GetAllStocks(int warehouseId, int itemMultiMRPId)
        {
            StockListDc stockListDc = new StockListDc();
            int userId = GetLoginUserId();

            using (var context = new AuthContext())
            {
                List<SqlParameter> sqlParameterList = new List<SqlParameter>();
                sqlParameterList.Add(new SqlParameter("@WarehouseId", warehouseId));
                sqlParameterList.Add(new SqlParameter("@ItemMultiMRPId", itemMultiMRPId));
                sqlParameterList.Add(new SqlParameter("@UserId", userId));

                List<StockNameDc> stockList
                    = context.Database.SqlQuery<StockNameDc>("exec Stock_GetAllStock @WarehouseId, @ItemMultiMRPId, @UserId", sqlParameterList.ToArray()).ToList();

                stockListDc.VirtualStock = stockList.FirstOrDefault(x => x.StockType == StockTypeTableNames.VirtualStock);
                stockListDc.AllSecondaryStockList = stockList.Where(x => x.StockType != StockTypeTableNames.VirtualStock).ToList();
                return stockList;
            }
        }

        [Route("GetUnsettledVirtualStocks/{warehouseId}/{itemMultiMRPId}")]
        [HttpGet]
        public List<UnsettledVirtualStockDTO> GetUnsettledVirtualStocks(int warehouseId, int itemMultiMRPId)
        {
            StockListDc stockListDc = new StockListDc();
            int userId = GetLoginUserId();
            using (var context = new AuthContext())
            {
                List<SqlParameter> sqlParameterList = new List<SqlParameter>();
                sqlParameterList.Add(new SqlParameter("@WarehouseId", warehouseId));
                sqlParameterList.Add(new SqlParameter("@ItemMultiMRPId", itemMultiMRPId));

                List<UnsettledVirtualStockDTO> stockList
                    = context.Database.SqlQuery<UnsettledVirtualStockDTO>("exec Stock_Call_GetUnsettledVirtualStock @WarehouseId, @ItemMultiMRPId", sqlParameterList.ToArray()).ToList();
                return stockList;
            }
        }

        [Route("SettledVirtualStock")]
        [HttpPost]
        public bool SettledVirtualStock(SettledVirtualStockDTO stockDto)
        {
            int userid = GetLoginUserId();
            DateTime currentTime = DateTime.Now;
            if (!string.IsNullOrEmpty(stockDto.MatchToTransactionId) && stockDto.TransactionIdList != null && stockDto.TransactionIdList.Count > 0)
            {

                var viList =stockDto.TransactionIdList.SelectMany(x => x.Split(',').ToList());
                var idList =  viList.Select(x => long.Parse(x)).ToList();
                using (var context = new AuthContext())
                {

                    List<VirtualStock> virtualStockList
                        = context.VirtualStockDB.Where(x => idList.Contains(x.Id)).ToList();
                    if (virtualStockList != null && virtualStockList.Any())
                    {
                        foreach (var item in virtualStockList)
                        {
                            item.TransactionId = stockDto.MatchToTransactionId;
                            item.ModifiedBy = userid;
                            item.ModifiedDate = currentTime;
                        }
                        context.Commit();
                        return true;
                    }
                }
            }

            return false;
        }

        [Route("GetByTransactionId/{transactionId}")]
        [HttpGet]
        public List<UnsettledVirtualStockDTO> GetByTransactionId(string transactionId)
        {
            string query = @"select	VS.StockType, 
			                        VS.Reason, 
			                        VS.ID as GroupId, 
			                        VS.TransactionId, 
			                        VS.InOutQty as RemainingQty , 
			                        P.Email as CreatedByEmails
	                        from VirtualStocks VS 
                            inner join People P ON VS.CreatedBy = P.PeopleId
                            where TransactionId = '" + transactionId + "'";
            using (var context = new AuthContext())
            {
                List<UnsettledVirtualStockDTO> list = context.Database.SqlQuery<UnsettledVirtualStockDTO>(query).ToList();
                return list;
            }
        }


        [Route("ManualStockUpdateRequest")]
        [HttpPost]
        public IHttpActionResult ManualStockUpdateRequestDc(List<ManualStockUpdateRequestDc> list)
        {
            int userId = GetLoginUserId();

            var res = "";
            TransactionOptions option = new TransactionOptions();
            option.IsolationLevel = System.Transactions.IsolationLevel.RepeatableRead;
            option.Timeout = TimeSpan.FromSeconds(90);
            using (var scope = new TransactionScope(TransactionScopeOption.Required, option))
            {
                using (var context = new AuthContext())
                {
                    if (list != null && list.Any(x => x.SourceStockType.ToLower().Contains("c")))
                    {
                        var warehouseId = list.FirstOrDefault().WarehouseId;

                        var warehouse = context.Warehouses.FirstOrDefault(x => x.WarehouseId == warehouseId);

                        if (warehouse.IsStopCurrentStockTrans)
                            return Ok("Inventory Transactions are currently disabled for this warehouse... Please try after some time");
                    }
                    BatchMasterManager batchMasterManager = new BatchMasterManager();
                    bool isSucess = batchMasterManager.MoveStock(list, context, userId);
                    StockTransactionHelper helper = new StockTransactionHelper();
                    bool result = helper.ManualStockUpdate(list, userId, context, scope);

                    if (result && isSucess)
                    {


                        context.Commit();
                        #region Insert in FIFO
                        if (ConfigurationManager.AppSettings["LiveFIFO"] == "1")
                        {
                            var ManualInOutItem = GetManualStockUpdateForInOut(list);

                            RabbitMqHelper rabbitMqHelper = new RabbitMqHelper();
                            foreach (var item in ManualInOutItem)
                            {
                                if (item.Source.Trim().ToLower() == "freestocks" && item.Destination.ToLower() == "currentstocks")
                                {
                                    var gitem = new GrDC
                                    {
                                        ItemMultiMrpId = item.ItemMultiMRPId,
                                        WarehouseId = item.WarehouseId,
                                        Source = "FreeInCS",
                                        CreatedDate = DateTime.Now,
                                        POId = 0,
                                        Qty = item.Qty,
                                        Price = 0,
                                    };
                                    rabbitMqHelper.Publish("FromFreeStock", gitem);
                                }
                                else if (item.Source.Trim().ToLower() == "currentstocks" && item.Destination.Trim().ToLower() == "freestocks")
                                {
                                    var ctof = new OutDc
                                    {
                                        ItemMultiMrpId = item.ItemMultiMRPId,
                                        WarehouseId = item.WarehouseId,
                                        Destination = "FreeOutFromCS",
                                        CreatedDate = DateTime.Now,
                                        ObjectId = 0,
                                        Qty = item.Qty,
                                        SellingPrice = 0,
                                        InMrpId = item.ItemMultiMRPId
                                    };

                                    rabbitMqHelper.Publish("ToFreeStock", ctof);

                                }
                                else if (item.Source.Trim().ToLower() == "currentstocks" && item.Destination.Trim().ToLower() == "damagedstocks")
                                {
                                    var ditem = new OutDc
                                    {
                                        ItemMultiMrpId = item.ItemMultiMRPId,
                                        WarehouseId = item.WarehouseId,
                                        Destination = "Damage Out",
                                        CreatedDate = DateTime.Now,
                                        ObjectId = 0,
                                        Qty = item.Qty,
                                        SellingPrice = 0,
                                        InMrpId = item.ItemMultiMRPId
                                    };
                                    rabbitMqHelper.Publish("DamageOut", ditem);
                                }
                                else if (item.Source.Trim().ToLower() == "currentstocks" && (!(item.Destination.Trim().ToLower() == "freestocks") || !(item.Destination.Trim().ToLower() == "damagedstocks")))
                                {
                                    var manualout = new OutDc
                                    {
                                        ItemMultiMrpId = item.ItemMultiMRPId,
                                        WarehouseId = item.WarehouseId,
                                        Destination = "Manual Out",
                                        CreatedDate = DateTime.Now,
                                        ObjectId = 0,
                                        Qty = item.Qty,
                                        SellingPrice = 0,
                                    };
                                    rabbitMqHelper.Publish("ManualOut", manualout);
                                }
                                else if (item.Destination.Trim().ToLower() == "currentstocks" && (!(item.Source.Trim().ToLower() == "freestocks") || !(item.Source.Trim().ToLower() == "damagedstocks")))
                                {
                                    var manualIn = new GrDC
                                    {
                                        ItemMultiMrpId = item.ItemMultiMRPId,
                                        WarehouseId = item.WarehouseId,
                                        Source = "Manual In",
                                        CreatedDate = DateTime.Now,
                                        Qty = item.Qty,
                                    };
                                    rabbitMqHelper.Publish("ManualIn", manualIn);
                                }
                            }
                        }

                        #endregion
                        scope.Complete();
                        res = "Transaction Saved Successfully";

                        return Ok(res);
                    }
                    else
                    {

                        res = "one of the stock not available";
                        return Ok(res);
                    }
                }

            }

        }


        [Route("GetManualStockRequests/skip/{skip}/take/{take}")]
        [HttpGet]
        public ManualStockUpdatePagerDc GetManualStockRequests(int skip, int take)
        {
            ManualStockUpdatePagerDc acPager = new ManualStockUpdatePagerDc();
            var userId = GetLoginUserId();
            using (var authContext = new AuthContext())
            {
                var query = from lb in authContext.ManualStockUpdateRequestDB
                            join people in authContext.Peoples
                            on lb.CreatedBy equals people.PeopleID
                            join items in authContext.ItemMultiMRPDB
                            on lb.ItemMultiMRPId equals items.ItemMultiMRPId
                            where (lb.IsActive == true && lb.IsDeleted == false && lb.CreatedBy == userId && items.itemname != null)
                            select new ManualStockUpdateRequestDc()
                            {
                                Id = lb.Id,
                                ItemMultiMRPId = lb.ItemMultiMRPId,
                                WarehouseId = lb.WarehouseId,
                                SourceStockType = lb.SourceStockType,
                                DestinationStockType = lb.DestinationStockType,
                                Qty = lb.Qty,
                                Status = lb.Status,
                                UserName = people.DisplayName,
                                ItemName = items.itemname
                            };
                acPager.TotalRecords = query.Count();
                acPager.ManualStockRequestsList = query.OrderByDescending(x => x.Id).Skip(skip).Take(take).ToList();
                return acPager;
            }
        }


        [Route("GetTransactionListByName/filterValue/{filterValue}/warehouseId/{warehouseId}/creationDate/{creationDate}/skip/{skip}/take/{take}")]
        [HttpGet]
        public ManualStockUpdatePagerDc GetTransactionListByName(string filterValue, int warehouseId, DateTime? creationDate, int skip, int take)
        {
            ManualStockUpdatePagerDc acPager = new ManualStockUpdatePagerDc();
            var userId = GetLoginUserId();
            using (var authContext = new AuthContext())
            {
                var agentCommissionPaymentList = authContext.AgentCommissionPaymentDB.ToList();
                if (warehouseId == 0)
                {

                    if (filterValue == "noFilter")
                    {
                        var query =
                    from lb in authContext.ManualStockUpdateRequestDB
                    join people in authContext.Peoples
                    on lb.CreatedBy equals people.PeopleID
                    join items in authContext.ItemMultiMRPDB
                       on lb.ItemMultiMRPId equals items.ItemMultiMRPId
                    where (lb.IsActive == true && lb.IsDeleted == false && lb.CreatedBy == 1716 ? true : lb.CreatedBy == userId && items.itemname != null)
                    select new ManualStockUpdateRequestDc()
                    {
                        Id = lb.Id,
                        ItemMultiMRPId = lb.ItemMultiMRPId,
                        WarehouseId = lb.WarehouseId,
                        SourceStockType = lb.SourceStockType,
                        DestinationStockType = lb.DestinationStockType,
                        Qty = lb.Qty,
                        Status = lb.Status,
                        UserName = people.DisplayName,
                        ItemName = items.itemname
                    };

                        acPager.TotalRecords = query.Count();
                        acPager.ManualStockRequestsList = query.OrderByDescending(x => x.Id).Skip(skip).Take(take).ToList();
                    }

                    else
                    {
                        var query =
                        from lb in authContext.ManualStockUpdateRequestDB
                        join people in authContext.Peoples
                        on lb.CreatedBy equals people.PeopleID
                        join items in authContext.ItemMultiMRPDB
                        on lb.ItemMultiMRPId equals items.ItemMultiMRPId
                        where (lb.IsActive == true && lb.IsDeleted == false && lb.CreatedBy == userId && items.itemname.ToLower().Contains(filterValue.ToLower()) && items.itemname != null)
                        select new ManualStockUpdateRequestDc()
                        {
                            Id = lb.Id,
                            ItemMultiMRPId = lb.ItemMultiMRPId,
                            WarehouseId = lb.WarehouseId,
                            SourceStockType = lb.SourceStockType,
                            DestinationStockType = lb.DestinationStockType,
                            Qty = lb.Qty,
                            Status = lb.Status,
                            UserName = people.DisplayName,
                            ItemName = items.itemname
                        };

                        acPager.TotalRecords = query.Count();
                        acPager.ManualStockRequestsList = query.OrderByDescending(x => x.Id).Skip(skip).Take(take).ToList();
                    }
                }

                else
                {

                    if (filterValue != "noFilter")
                    {
                        var query =
                     from lb in authContext.ManualStockUpdateRequestDB
                     join people in authContext.Peoples
                     on lb.CreatedBy equals people.PeopleID
                     join items in authContext.ItemMultiMRPDB
                     on lb.ItemMultiMRPId equals items.ItemMultiMRPId
                     join warehouse in authContext.Warehouses
                     on lb.WarehouseId equals warehouse.WarehouseId
                     where (lb.IsActive == true && lb.IsDeleted == false && lb.CreatedBy == userId && lb.WarehouseId == warehouseId && lb.CreatedBy == userId && items.itemname.ToLower().Contains(filterValue.ToLower()) && items.itemname != null)
                     select new ManualStockUpdateRequestDc()
                     {
                         Id = lb.Id,
                         ItemMultiMRPId = lb.ItemMultiMRPId,
                         WarehouseId = lb.WarehouseId,
                         SourceStockType = lb.SourceStockType,
                         DestinationStockType = lb.DestinationStockType,
                         Qty = lb.Qty,
                         Status = lb.Status,
                         UserName = people.DisplayName,
                         ItemName = items.itemname
                     };

                        acPager.TotalRecords = query.Count();
                        acPager.ManualStockRequestsList = query.OrderByDescending(x => x.Id).Skip(skip).Take(take).ToList();
                    }
                    else
                    {
                        var query =
                       from lb in authContext.ManualStockUpdateRequestDB
                       join people in authContext.Peoples
                       on lb.CreatedBy equals people.PeopleID
                       join items in authContext.ItemMultiMRPDB
                    on lb.ItemMultiMRPId equals items.ItemMultiMRPId
                       join warehouse in authContext.Warehouses
                       on lb.WarehouseId equals warehouse.WarehouseId
                       where (lb.IsActive == true && lb.IsDeleted == false && lb.CreatedBy == userId && lb.WarehouseId == warehouseId && items.itemname != null)
                       select new ManualStockUpdateRequestDc()
                       {
                           Id = lb.Id,
                           ItemMultiMRPId = lb.ItemMultiMRPId,
                           WarehouseId = lb.WarehouseId,
                           SourceStockType = lb.SourceStockType,
                           DestinationStockType = lb.DestinationStockType,
                           Qty = lb.Qty,
                           Status = lb.Status,
                           UserName = people.DisplayName,
                           ItemName = items.itemname
                       };
                        acPager.TotalRecords = query.Count();
                        acPager.ManualStockRequestsList = query.OrderByDescending(x => x.Id).Skip(skip).Take(take).ToList();
                    }
                }

                return acPager;
            }
        }

        [Route("GetManualStockUpdateRequestList")]
        [HttpPost]
        public ManualStockUpdatePagerDc GetManualStockUpdateRequestList(ManualStockUpdatePagerFilter filter)
        {
            ManualStockUpdatePagerDc pager = new ManualStockUpdatePagerDc();
            pager.TotalRecords = 0;

            using (var context = new AuthContext())
            {
                var query = from mc in context.ManualStockUpdateRequestDB
                            join im in context.itemMasters
                                on new { WarehouseId = mc.WarehouseId, ItemMultiMRPId = mc.ItemMultiMRPId }
                                equals new { WarehouseId = im.WarehouseId, ItemMultiMRPId = im.ItemMultiMRPId }
                            join p in context.Peoples
                                on mc.CreatedBy equals p.PeopleID
                            join w in context.Warehouses
                                on mc.WarehouseId equals w.WarehouseId
                            where (!filter.WarehouseId.HasValue || filter.WarehouseId.Value == mc.WarehouseId)
                                && (!filter.ItemMultiMRPId.HasValue || filter.ItemMultiMRPId.Value == mc.ItemMultiMRPId)
                                && (string.IsNullOrEmpty(filter.Keyword) || im.itemname.ToLower().Contains(filter.Keyword))
                            group new { mc, im, p, w } by mc into g
                            select new ManualStockUpdateRequestDc
                            {
                                DestinationStockType = g.Key.DestinationStockType,
                                Id = g.Key.Id,
                                ItemMultiMRPId = g.Key.ItemMultiMRPId,
                                Qty = g.Key.Qty,
                                Reason = g.Key.Reason,
                                SourceStockType = g.Key.SourceStockType,
                                Status = g.Key.Status,
                                StockTransferType = g.Key.StockTransferType,
                                WarehouseId = g.Key.WarehouseId,
                                CreatedDate = g.Key.CreatedDate,

                                WarehouseName = g.Min(x => x.w.WarehouseName),
                                UserName = g.Min(x => x.p.Email),
                                ItemName = g.Min(x => x.im.itemname)

                            };
                pager.TotalRecords = query.Count();
                pager.ManualStockRequestsList = query.OrderBy(x => x.CreatedDate).Skip(filter.Skip).Take(filter.Take).ToList();
                return pager;
            }

        }

        [HttpGet]
        [Route("WHSearchinitematAdmin")]
        public List<MRPItemStockDTO> WHSearchinitematAdmin(string key, int warehouseid)
     {
            List<ItemMaster> ass = new List<ItemMaster>();

            //List<ItemMasterDTO> newdata = new List<ItemMasterDTO>();
            List<MRPItemStockDTO> _result = new List<MRPItemStockDTO>();           
            using (var db = new AuthContext())
            {
                string val = key.Trim();
                int i = int.TryParse(val, out int x) ? x : 0;
                if (i > 0)
                {
                    var ItemList = db.DbCurrentStock.Where(a => a.Deleted == false && a.WarehouseId == warehouseid
                    && ((a.ItemMultiMRPId == i)))
                        .Select(a => new { a.ItemMultiMRPId, a.itemname, a.MRP, a.UnitofQuantity, a.UOM, a.WarehouseId, a.ItemNumber }).ToList();
                    _result = Mapper.Map(ItemList).ToANew<List<MRPItemStockDTO>>();
                }
                else
                {
                   var ItemList1 = db.DbCurrentStock.Where(a => a.Deleted == false && a.WarehouseId == warehouseid
                  && ((a.itemname.ToLower().Contains(key.Trim().ToLower())) || (a.ItemNumber.ToLower().Contains(key.Trim().ToLower()))))
                  .Select(a => new { a.ItemMultiMRPId, a.itemname, a.MRP, a.UnitofQuantity, a.UOM, a.WarehouseId, a.ItemNumber }).ToList();
                    _result = Mapper.Map(ItemList1).ToANew<List<MRPItemStockDTO>>();
                }

             

                //if (newdata != null)
                //{
                //    newdata.ForEach(x => x.ItemLimitId = db.ItemLimitMasterDB.FirstOrDefault(z => z.ItemNumber == x.Number && z.WarehouseId == x.WarehouseId && z.ItemMultiMRPId == x.ItemMultiMRPId)?.Id);
                //}


                //newdata = (from a in db.itemMasters
                //           where (a.Deleted == false && a.WarehouseId == warehouseid && (a.itemname.ToLower().Contains(key.Trim().ToLower()) || a.Number.ToLower().Contains(key.Trim().ToLower())))
                //           join b in db.ItemMasterCentralDB on a.SellingSku equals b.SellingSku
                //           join c in db.DbCurrentStock on a.Number equals c.ItemNumber
                //           join d in db.ItemMultiMRPDB on a.ItemMultiMRPId equals d.ItemMultiMRPId
                //           where (c.WarehouseId == warehouseid && c.ItemMultiMRPId == a.ItemMultiMRPId)
                //           select new ItemMasterDTO
                //           {
                //               Categoryid = b.Categoryid,
                //               BaseCategoryid = b.BaseCategoryid,
                //               SubCategoryId = b.SubCategoryId,
                //               SubsubCategoryid = b.SubsubCategoryid,
                //               itemname = a.itemname,
                //               itemBaseName = b.itemBaseName,
                //               BaseCategoryName = b.BaseCategoryName,
                //               CategoryName = b.CategoryName,
                //               SubcategoryName = b.SubcategoryName,
                //               SubsubcategoryName = b.SubsubcategoryName,
                //               SubSubCode = b.SubSubCode,
                //               TGrpName = b.TGrpName,
                //               Number = b.Number,
                //               SellingUnitName = a.SellingUnitName,
                //               PurchaseUnitName = a.PurchaseUnitName,
                //               TotalTaxPercentage = b.TotalTaxPercentage,
                //               LogoUrl = b.LogoUrl,
                //               MinOrderQty = b.MinOrderQty,
                //               PurchaseMinOrderQty = a.PurchaseMinOrderQty,
                //               PurchaseSku = b.PurchaseSku,
                //               price = a.price,
                //               SellingSku = b.SellingSku,
                //               GruopID = b.GruopID,
                //               CessGrpID = b.CessGrpID,
                //               PurchasePrice = a.PurchasePrice,
                //               Cityid = a.Cityid,
                //               CityName = a.CityName,
                //               UnitPrice = a.UnitPrice,
                //               Margin = a.Margin,
                //               marginPoint = a.marginPoint,
                //               SupplierId = a.SupplierId,
                //               SupplierName = a.SupplierName,
                //               SUPPLIERCODES = a.SUPPLIERCODES,
                //               Discount = a.Discount,
                //               WarehouseId = a.WarehouseId,
                //               WarehouseName = a.WarehouseName,
                //               Deleted = a.Deleted,
                //               active = a.active,
                //               CompanyId = a.CompanyId,
                //               ItemId = a.ItemId,
                //               CurrentStock = c.CurrentInventory,
                //               ItemMultiMRPId = a.ItemMultiMRPId,
                //               UnitofQuantity = a.UnitofQuantity,
                //               IsSensitive = b.IsSensitive,
                //               IsSensitiveMRP = b.IsSensitiveMRP,
                //               UOM = a.UOM,
                //               NetPurchasePrice = a.NetPurchasePrice,
                //               IsReplaceable = a.IsReplaceable,
                //               DistributionPrice = a.DistributionPrice,
                //               DistributorShow = a.DistributorShow
                //           }).OrderByDescending(x => x.SellingSku).ToList();

                //if (newdata != null)
                //{
                //    newdata.ForEach(x => x.ItemLimitId = db.ItemLimitMasterDB.FirstOrDefault(z => z.ItemNumber == x.Number && z.WarehouseId == x.WarehouseId && z.ItemMultiMRPId == x.ItemMultiMRPId)?.Id);
                //}
            }
            return _result;


        }


        [Route("StockList")]
        [HttpGet]
        public List<StockTransactionListDC> StockList(string Keyword, int Skip, int Take, int warehouseId, int itemMultiMRPId)
        {
            StockTransactionListDC stockTransactionListDC = new StockTransactionListDC();
            using (var context = new AuthContext())
            {
                List<SqlParameter> sqlParameterList = new List<SqlParameter>();
                sqlParameterList.Add(new SqlParameter("@Keyword", Keyword));
                sqlParameterList.Add(new SqlParameter("@Skip", Skip));
                sqlParameterList.Add(new SqlParameter("@Take", Take));
                sqlParameterList.Add(new SqlParameter("@WarehouseId", warehouseId));
                sqlParameterList.Add(new SqlParameter("@ItemMultiMRPId", itemMultiMRPId));

                List<StockTransactionListDC> stockList
               = context.Database.SqlQuery<StockTransactionListDC>("exec Stock_GetStockTransactionList @Keyword,@Skip,@Take, @WarehouseId, @ItemMultiMRPId", sqlParameterList.ToArray()).ToList();
                return stockList;
            }
        }

        [HttpGet]
        [Route("WHSelectedinitematAdmin")]
        public object WHSelectedinitematAdmin(int warehouseid)
        {
            List<ItemMaster> ass = new List<ItemMaster>();
            try
            {

                var identity = User.Identity as ClaimsIdentity;
                int compid = 0, userid = 0;
                int Warehouse_id = 1;

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
                Warehouse_id = warehouseid;

                List<ItemMasterDTO> newdata = new List<ItemMasterDTO>();

                using (AuthContext db = new AuthContext())
                {

                    newdata = (from a in db.itemMasters
                               where (a.Deleted == false && a.WarehouseId == Warehouse_id)
                               join b in db.ItemMasterCentralDB on a.SellingSku equals b.SellingSku
                               join c in db.DbCurrentStock on a.Number equals c.ItemNumber
                               join d in db.ItemMultiMRPDB on a.ItemMultiMRPId equals d.ItemMultiMRPId
                               where (c.WarehouseId == Warehouse_id && c.ItemMultiMRPId == a.ItemMultiMRPId)
                               select new ItemMasterDTO
                               {

                                   Categoryid = b.Categoryid,
                                   BaseCategoryid = b.BaseCategoryid,
                                   SubCategoryId = b.SubCategoryId,
                                   SubsubCategoryid = b.SubsubCategoryid,
                                   itemname = a.itemname,
                                   itemBaseName = b.itemBaseName,
                                   BaseCategoryName = b.BaseCategoryName,
                                   CategoryName = b.CategoryName,
                                   SubcategoryName = b.SubcategoryName,
                                   SubsubcategoryName = b.SubsubcategoryName,
                                   SubSubCode = b.SubSubCode,
                                   TGrpName = b.TGrpName,
                                   Number = b.Number,
                                   SellingUnitName = a.SellingUnitName,
                                   PurchaseUnitName = a.PurchaseUnitName,
                                   TotalTaxPercentage = b.TotalTaxPercentage,
                                   LogoUrl = b.LogoUrl,
                                   MinOrderQty = b.MinOrderQty,
                                   PurchaseMinOrderQty = a.PurchaseMinOrderQty,
                                   PurchaseSku = b.PurchaseSku,
                                   price = a.price,
                                   SellingSku = b.SellingSku,
                                   GruopID = b.GruopID,
                                   CessGrpID = b.CessGrpID,
                                   PurchasePrice = a.PurchasePrice,
                                   Cityid = a.Cityid,
                                   CityName = a.CityName,
                                   UnitPrice = a.UnitPrice,
                                   Margin = a.Margin,
                                   marginPoint = a.marginPoint,
                                   SupplierId = a.SupplierId,
                                   SupplierName = a.SupplierName,
                                   SUPPLIERCODES = a.SUPPLIERCODES,
                                   Discount = a.Discount,
                                   WarehouseId = a.WarehouseId,
                                   WarehouseName = a.WarehouseName,
                                   Deleted = a.Deleted,
                                   active = a.active,
                                   CompanyId = a.CompanyId,
                                   ItemId = a.ItemId,
                                   CurrentStock = c.CurrentInventory,
                                   ItemMultiMRPId = a.ItemMultiMRPId,
                                   UnitofQuantity = a.UnitofQuantity,
                                   IsSensitive = b.IsSensitive,
                                   IsSensitiveMRP = b.IsSensitiveMRP,
                                   UOM = a.UOM,
                                   NetPurchasePrice = a.NetPurchasePrice,
                                   IsReplaceable = a.IsReplaceable,
                                   DistributionPrice = a.DistributionPrice,
                                   DistributorShow = a.DistributorShow
                               }).OrderByDescending(x => x.SellingSku).ToList();


                    if (newdata != null)
                    {
                        newdata.ForEach(x => x.ItemLimitId = db.ItemLimitMasterDB.FirstOrDefault(z => z.ItemNumber == x.Number && z.WarehouseId == x.WarehouseId && z.ItemMultiMRPId == x.ItemMultiMRPId)?.Id);
                    }

                    return newdata;
                }
            }
            catch (Exception ex)
            {
                return null;
            }
        }



        [Route("FetchDynamicStock")]
        [HttpGet]
        public List<FetchDynamicStockDc> FetchDynamicStockList(int warehouseId, string TableName)
        {
            using (var context = new AuthContext())
            {
                List<SqlParameter> sqlParameterList = new List<SqlParameter>();
                sqlParameterList.Add(new SqlParameter("@TableName", TableName));
                sqlParameterList.Add(new SqlParameter("@WarehouseId", warehouseId));

                List<FetchDynamicStockDc> stockList = context.Database.SqlQuery<FetchDynamicStockDc>("exec Stock_Fetch_Dynamic_Table  @TableName, @WarehouseId", sqlParameterList.ToArray()).ToList();
                return stockList;
            }
        }


        public List<ManualStockUpdateInOutDc> GetManualStockUpdateForInOut(List<ManualStockUpdateRequestDc> List)
        {
            List<ManualStockUpdateInOutDc> result = new List<ManualStockUpdateInOutDc>();

            if (List != null && List.Any())
            {
                var query = from lft in List
                            join rht in List
                            on new { qty = lft.Qty > 0 ? lft.Qty : (lft.Qty * -1), stk = lft.DestinationStockType, rsn = lft.Reason } equals new { qty = rht.Qty > 0 ? rht.Qty : (-1 * rht.Qty), stk = rht.SourceStockType, rsn = rht.Reason }
                            //into ps
                            //from p in ps.DefaultIfEmpty()
                            group lft by new
                            {
                                Qty = lft.Qty > 0 ? lft.Qty : (-1 * lft.Qty),
                                DestinationStockType = rht.DestinationStockType,
                                //lft.SourceStockType,
                                lft.ItemMultiMRPId,
                                lft.WarehouseId,
                                lft.Reason
                            } into gcs
                            select new ManualStockUpdateInOutDc()
                            {
                                Destination = gcs.Key.DestinationStockType,
                                ItemMultiMRPId = gcs.Key.ItemMultiMRPId,
                                Qty = gcs.Key.Qty,
                                Source = gcs.Max(y => y.SourceStockType),
                                WarehouseId = gcs.Key.WarehouseId,
                                Reason = gcs.Key.Reason
                            };
                result = query.ToList();

                if (result == null)
                {
                    result = new List<ManualStockUpdateInOutDc>();
                }

                foreach (var item in result)
                {
                    var newItem = List.FirstOrDefault(x => x.SourceStockType == item.Source && x.Reason == item.Reason && (x.Qty > 0 ? x.Qty : (-1 * x.Qty)) == item.Qty && x.DestinationStockType == StockTypeTableNames.VirtualStock && x.IsPairFound == false);
                    if (newItem != null)
                    {
                        newItem.IsPairFound = true;
                    }

                    newItem = List.FirstOrDefault(x => x.DestinationStockType == item.Destination && x.Reason == item.Reason && (x.Qty > 0 ? x.Qty : (-1 * x.Qty)) == item.Qty && x.SourceStockType == StockTypeTableNames.VirtualStock && x.IsPairFound == false);
                    if (newItem != null)
                    {
                        newItem.IsPairFound = true;
                    }
                }

                List = List.Where(x => x.IsPairFound == false).ToList();

                if (List != null && List.Any())
                {
                    foreach (var item in List)
                    {
                        result.Add(new ManualStockUpdateInOutDc
                        {
                            Destination = item.DestinationStockType,
                            ItemMultiMRPId = item.ItemMultiMRPId,
                            Qty = item.Qty > 0 ? item.Qty : item.Qty * (-1),
                            Reason = item.Reason,
                            Source = item.SourceStockType,
                            WarehouseId = item.WarehouseId
                        });
                    }
                }


                //var query = from lft in List
                //            join rht in List
                //            on new { qty = lft.Qty, stk = lft.DestinationStockType, rsn = lft.Reason } equals new { qty = rht.Qty, stk = rht.SourceStockType, rsn = rht.Reason }
                //            into ps
                //            from p in ps.DefaultIfEmpty()
                //            group ps by new
                //            {
                //                qty = lft.Qty,
                //                dest = ps == null ? lft.DestinationStockType : p.DestinationStockType,
                //                source = lft.SourceStockType,
                //                itemmultimrpid = lft.ItemMultiMRPId,
                //                warehouseid = lft.WarehouseId
                //            } into gcs
                //            select new ManualStockUpdateInOutDc()
                //            {
                //                Destination = gcs.Key.dest,
                //                ItemMultiMRPId = gcs.Key.itemmultimrpid,
                //                Qty = gcs.Key.qty,
                //                Source = gcs.Key.source,
                //                WarehouseId = gcs.Key.warehouseid
                //            };

                // result = query.ToList();

            }
            return result;

        }
    }


    public class ManualStockUpdateInOutDc
    {
        public int ItemMultiMRPId { get; set; }
        public int WarehouseId { get; set; }
        public int Qty { get; set; }
        public string Source { get; set; }
        public string Destination { get; set; }
        public string Reason { get; set; }
    }


    public class MRPItemStockDTO
    {
        public string itemname { get; set; }
        public int ItemMultiMRPId { get; set; }
        public double MRP { get; set; }
        public string UnitofQuantity { get; set; }
        public int? WarehouseId { get; set; }
        public string ItemNumber { get; set; }
        public string UOM { get; set; }//Unit of masurement like GM Kg 
    }

    public class UnsettledVirtualStockDTO
    {
        public long GroupId { get; set; }
        public string TransactionId { get; set; }
        public string WarehouseName { get; set; }
        public int RemainingQty { get; set; }
        public string CreatedByEmails { get; set; }
        public string StockType { get; set; }
        public string Reason { get; set; }
        public string VirtualStockIds { get; set; }
    }

    public class SettledVirtualStockDTO
    {
        public string MatchToTransactionId { get; set; }
        public string MatchVirtualStockIdList { get; set; }
        public List<string> TransactionIdList { get; set; }
    }


    public class ManualStockUpdatePagerFilter
    {
        public int Skip { get; set; }
        public int Take { get; set; }
        public int? WarehouseId { get; set; }
        public int? ItemMultiMRPId { get; set; }
        public string Keyword { get; set; }
    }
}
