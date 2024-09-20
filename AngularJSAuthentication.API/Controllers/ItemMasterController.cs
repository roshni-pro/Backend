using AngularJSAuthentication.API.Controllers.Base;
using AngularJSAuthentication.API.Helper;
using AngularJSAuthentication.API.Helpers;
using AngularJSAuthentication.API.Models;
using AngularJSAuthentication.BusinessLayer.Managers.Masters;
using AngularJSAuthentication.BusinessLayer.Managers.Reports;
using AngularJSAuthentication.Common.Constants;
using AngularJSAuthentication.Common.Helpers;
using AngularJSAuthentication.DataContracts.Masters;
using AngularJSAuthentication.DataContracts.Shared;
using AngularJSAuthentication.DataContracts.Transaction.Ledger.ItemLedger;
using AngularJSAuthentication.DataContracts.Transaction.Mongo.BuyerDashboard;
using AngularJSAuthentication.DataContracts.Transaction.Stocks;
using AngularJSAuthentication.Model;
using AngularJSAuthentication.Model.Item;
using AngularJSAuthentication.Model.SalesApp;
using AngularJSAuthentication.Model.Seller;
using AngularJSAuthentication.Model.Stocks;
using LinqKit;
using MongoDB.Driver;
using Nito.AspNetBackgroundTasks;
using Nito.AsyncEx;
using NLog;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Configuration;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Numerics;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Transactions;
using System.Web;
using System.Web.Http;
using System.Web.Http.Description;
using AngularJSAuthentication.DataContracts.ROC;
using AngularJSAuthentication.BusinessLayer.Managers.JustInTime;
using AngularJSAuthentication.DataContracts.JustInTime;
using System.Web.Script.Serialization;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using AngularJSAuthentication.DataContracts.FileUpload;
using System.Runtime.Serialization.Formatters.Binary;
using AngularJSAuthentication.DataContracts.External.MobileExecutiveDC;

namespace AngularJSAuthentication.API.Controllers
{
    [AllowAnonymous]
    [RoutePrefix("api/itemMaster")]
    public class ItemMasterController : BaseAuthController
    {
        public static Logger logger = LogManager.GetCurrentClassLogger();
        private static TimeZoneInfo INDIAN_ZONE = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
        DateTime indianTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE);
        public double xPointValue = AppConstants.xPoint;

        [Route("")]
        public IEnumerable<ItemMaster> Get()
        {
            using (var context = new AuthContext())
            {
                logger.Info("start Item Master: ");
                List<ItemMaster> ass = new List<ItemMaster>();
                try
                {
                    var identity = User.Identity as ClaimsIdentity;
                    int compid = 1, userid = 0;
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
                    int CompanyId = compid;
                    logger.Info("User ID : {0} , Company Id : {1}", compid, userid);

                    if (Warehouse_id > 0)
                    {
                        ass = context.AllItemMasterWid(CompanyId, Warehouse_id).ToList();
                        logger.Info("End  Item Master: ");
                        return ass;
                    }
                    else
                    {
                        ass = context.AllItemMaster(CompanyId).ToList();
                        logger.Info("End  Item Master: ");
                        return ass;
                    }
                }
                catch (Exception ex)
                {
                    logger.Error("Error in Item Master " + ex.Message);
                    logger.Info("End  Item Master: ");
                    return null;
                }
            }
        }

        [Route("GetItemAgainstWareHouse")]
        public IEnumerable<ItemMaster> GetItemAgainstWareHouse(int warehouseid)
        {
            using (var context = new AuthContext())
            {
                logger.Info("start Item Master: ");
                List<ItemMaster> ass = new List<ItemMaster>();
                try
                {
                    var identity = User.Identity as ClaimsIdentity;
                    int compid = 1, userid = 0;
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
                    int CompanyId = compid;
                    logger.Info("User ID : {0} , Company Id : {1}", compid, userid);
                    Warehouse_id = warehouseid;
                    if (Warehouse_id > 0)
                    {
                        ass = context.AllItemMasterWid(CompanyId, warehouseid).ToList();
                        logger.Info("End  Item Master: ");
                        return ass;
                    }
                    else
                    {
                        ass = context.AllItemMaster(CompanyId).ToList();
                        logger.Info("End  Item Master: ");
                        return ass;
                    }
                }
                catch (Exception ex)
                {
                    logger.Error("Error in Item Master " + ex.Message);
                    logger.Info("End  Item Master: ");
                    return null;
                }
            }
        }

        /// <summary>
        /// Get Central ItemMaster Central
        /// </summary>
        /// <returns></returns>
        [Route("Central")]
        public dynamic GetCentral()
        {
            using (var db = new AuthContext())
            {
                logger.Info("start Item Master: ");
                ItemMasterCentralList ItemMasterCentralList = new ItemMasterCentralList();
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
                    int CompanyId = compid;
                    logger.Info("User ID : {0} , Company Id : {1}", compid, userid);
                    if (CompanyId > 0)
                    {
                        string sqlquery = "select *from ItemMasterCentrals with(nolock) where Deleted=0";
                        ItemMasterCentralList.ItemMasterCentral = db.Database.SqlQuery<ItemMasterCentral>(sqlquery).ToList();
                        // ItemMasterCentralList.ItemMasterCentral = db.ItemMasterCentralDB.Where(x => x.Deleted == false && x.CompanyId == CompanyId).ToList();
                        ItemMasterCentralList.ItemMasterCentralGroupByNumber = ItemMasterCentralList.ItemMasterCentral.GroupBy(x => x.Number).Select(x => x.FirstOrDefault()).ToList();
                    }
                    return ItemMasterCentralList;

                }
                catch (Exception ex)
                {
                    logger.Error("Error in Item Master " + ex.Message);
                    logger.Info("End  Item Master: ");
                    return null;
                }
            }
        }


        [Route("unique")]
        [HttpGet]
        public dynamic uniqueItem()
        {
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
                    using (AuthContext db = new AuthContext())
                    {
                        if (compid > 0)
                        {
                            var ItemData = db.ItemMasterCentralDB.Where(x => x.Deleted == false && x.CompanyId == compid).GroupBy(x => x.Number).ToList().Select(x => x.FirstOrDefault()).ToList();
                            return ItemData;
                        }
                        else
                        {
                            return null;
                        }
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


        [Route("category/")]
        public customeritems GetCategory(int warehouseid)
        {
            return CommonHelper.getItemMaster(warehouseid);
        }

        /// <summary>
        /// Created by 01/02/2019
        /// </summary>
        /// <param name="lang"></param>
        /// <returns></returns>
        [Route("category/V2")]

        public customeritems GetCategoryv2(int warehouseid, string lang)
        {
            return CommonHelper.getItemMasterv2(warehouseid, lang);
        }
        [Route("")]
        [AllowAnonymous]
        public PaggingData Get(int list, int page, string status)
        {
            using (var context = new AuthContext())
            {
                logger.Info("start ItemMaster: ");
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

                    int CompanyId = compid;
                    logger.Info("User ID : {0} , Company Id : {1}", compid, userid, Warehouse_id);

                    var itemPagedList = context.AllItemMasterForPaging(list, page, CompanyId, status);
                    logger.Info("End ItemMaster: ");
                    return itemPagedList;
                }
                catch (Exception ex)
                {
                    logger.Error("Error in ItemMaster " + ex.Message);
                    logger.Info("End  ItemMaster: ");
                    return null;
                }
            }
        }



        [Route("Getwarehouseitems")]
        public async System.Threading.Tasks.Task<PaggingData> Getwarehouseitems(int list, int page, int warehouseid, string status, string type = "")
        {
            using (var context = new AuthContext())
            {
                logger.Info("start ItemMaster: ");
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
                    int CompanyId = compid;
                    logger.Info("User ID : {0} , Company Id : {1}", compid, userid, Warehouse_id);

                    var itemPagedList = context.AllItemMasterForPagingWid(list, page, warehouseid, CompanyId, status, type);
                    logger.Info("End ItemMaster: ");
                    //List<ItemClassificationDC> ABCitemsList = itemPagedList.ordermaster.Select(item => new ItemClassificationDC { ItemNumber = item.Number, WarehouseId = item.WarehouseId }).ToList();

                    var manager = new ItemLedgerManager();
                    List<ItemClassificationDC> objItemClassificationDClist = new List<ItemClassificationDC>();
                    foreach (var data in itemPagedList.ordermaster)
                    {
                        ItemClassificationDC obj = new ItemClassificationDC();
                        obj.WarehouseId = data.WarehouseId;
                        obj.ItemNumber = data.Number;
                        objItemClassificationDClist.Add(obj);

                    }

                    try
                    {
                        List<ItemClassificationDC> _objItemClassificationDClist = await manager.GetItemClassificationsAsync(objItemClassificationDClist);

                        //var GetItem = await manager.GetItemClassificationsAsync(ABCitemsList);
                        foreach (var item in itemPagedList.ordermaster)
                        {

                            if (_objItemClassificationDClist != null && _objItemClassificationDClist.Any())
                            {
                                if (_objItemClassificationDClist.Any(x => x.ItemNumber == item.Number))
                                {
                                    item.ABCClassification = _objItemClassificationDClist.Where(x => x.ItemNumber == item.Number).Select(x => x.Category).FirstOrDefault();
                                }
                                else { item.ABCClassification = "D"; }
                            }

                            else { item.ABCClassification = "D"; }
                        }
                    }
                    catch (Exception s) { }


                    return itemPagedList;
                }
                catch (Exception ex)
                {
                    logger.Error("Error in ItemMaster " + ex.Message);
                    logger.Info("End  ItemMaster: ");
                    return null;
                }
            }
        }

        [Route("GetwarehouseitemsWithInactive")]
        public async System.Threading.Tasks.Task<PaggingData> GetwarehouseitemsWithInactive(int list, int page, int warehouseid, string status)
        {
            using (var context = new AuthContext())
            {
                logger.Info("start ItemMaster: ");
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
                    int CompanyId = compid;
                    logger.Info("User ID : {0} , Company Id : {1}", compid, userid, Warehouse_id);

                    //var itemPagedList = context.AllItemMasterForPagingWid(list, page, warehouseid, CompanyId, status);
                    var itemPagedList = AllItemMasterForPagingWidWithInactiveItems(list, page, warehouseid, CompanyId, status);
                    logger.Info("End ItemMaster: ");
                    //List<ItemClassificationDC> ABCitemsList = itemPagedList.ordermaster.Select(item => new ItemClassificationDC { ItemNumber = item.Number, WarehouseId = item.WarehouseId }).ToList();

                    var manager = new ItemLedgerManager();
                    List<ItemClassificationDC> objItemClassificationDClist = new List<ItemClassificationDC>();
                    foreach (var data in itemPagedList.ordermaster)
                    {
                        ItemClassificationDC obj = new ItemClassificationDC();
                        obj.WarehouseId = data.WarehouseId;
                        obj.ItemNumber = data.Number;
                        objItemClassificationDClist.Add(obj);

                    }
                    List<ItemClassificationDC> _objItemClassificationDClist = await manager.GetItemClassificationsAsync(objItemClassificationDClist);
                    //var GetItem = await manager.GetItemClassificationsAsync(ABCitemsList);
                    foreach (var item in itemPagedList.ordermaster)
                    {

                        if (_objItemClassificationDClist != null && _objItemClassificationDClist.Any())
                        {
                            if (_objItemClassificationDClist.Any(x => x.ItemNumber == item.Number))
                            {
                                item.Category = _objItemClassificationDClist.Where(x => x.ItemNumber == item.Number).Select(x => x.Category).FirstOrDefault();
                            }
                            else { item.Category = "D"; }
                        }

                        else { item.Category = "D"; }
                    }


                    return itemPagedList;
                }
                catch (Exception ex)
                {
                    logger.Error("Error in ItemMaster " + ex.Message);
                    logger.Info("End  ItemMaster: ");
                    return null;
                }
            }
        }


        [Route("GetwarehouseitemsExport")]
        public async System.Threading.Tasks.Task<PaggingData> GetwarehouseitemsExport(int warehouseid, string status)
        {
            logger.Info("start ItemMaster: ");
            using (var ctx = new AuthContext())
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
                    int CompanyId = compid;
                    logger.Info("User ID : {0} , Company Id : {1}", compid, userid, Warehouse_id);

                    var itemPagedList = ctx.AllItemMasterWithoutPaging(warehouseid, CompanyId, status);
                    logger.Info("End ItemMaster: ");
                    var manager = new ItemLedgerManager();
                    List<ItemClassificationDC> objItemClassificationDClist = new List<ItemClassificationDC>();
                    foreach (var data in itemPagedList.ordermaster)
                    {
                        ItemClassificationDC obj = new ItemClassificationDC();
                        obj.WarehouseId = data.WarehouseId;
                        obj.ItemNumber = data.Number;
                        objItemClassificationDClist.Add(obj);

                    }
                    List<ItemClassificationDC> _objItemClassificationDClist = await manager.GetItemClassificationsAsync(objItemClassificationDClist);
                    //var GetItem = await manager.GetItemClassificationsAsync(ABCitemsList);
                    foreach (var item in itemPagedList.ordermaster)
                    {

                        if (_objItemClassificationDClist != null && _objItemClassificationDClist.Any())
                        {
                            if (_objItemClassificationDClist.Any(x => x.ItemNumber == item.Number))
                            {
                                item.ABCClassification = _objItemClassificationDClist.Where(x => x.ItemNumber == item.Number).Select(x => x.Category).FirstOrDefault();
                            }
                            else { item.ABCClassification = "D"; }
                        }

                        else { item.ABCClassification = "D"; }
                    }

                    return itemPagedList;
                }
                catch (Exception ex)
                {
                    logger.Error("Error in ItemMaster " + ex.Message);
                    logger.Info("End  ItemMaster: ");
                    return null;
                }
            }
        }

        [Route("GetUnMapSupplierItems")]
        public PaggingData GetUnMapSupplierItems(int list, int page, int warehouseid, string status)
        {
            using (var context = new AuthContext())
            {
                logger.Info("start ItemMaster: ");
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
                    int CompanyId = compid;
                    logger.Info("User ID : {0} , Company Id : {1}", compid, userid, Warehouse_id);

                    var itemPagedList = context.AllItemUnMappedSupplierForPagingWid(list, page, warehouseid, CompanyId, status);
                    logger.Info("End ItemMaster: ");
                    return itemPagedList;
                }
                catch (Exception ex)
                {
                    logger.Error("Error in ItemMaster " + ex.Message);
                    logger.Info("End  ItemMaster: ");
                    return null;
                }
            }
        }

        [Route("supplier")]
        public PaggingData GetSupplier(int list, int page, string SupplierCode)
        {

            logger.Info("start ItemMaster: ");
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
                int CompanyId = compid;
                using (AuthContext db = new AuthContext())
                {
                    if (Warehouse_id > 0)
                    {
                        PaggingData obj = new PaggingData();
                        obj.total_count = db.itemMasters.Where(x => x.SUPPLIERCODES == SupplierCode && x.WarehouseId == Warehouse_id).Count();
                        obj.ordermaster = db.itemMasters.AsEnumerable().Where(x => x.SUPPLIERCODES == SupplierCode && x.WarehouseId == Warehouse_id).Skip((page - 1) * list).Take(list).ToList();
                        return obj;
                    }
                    else
                    {
                        PaggingData obj = new PaggingData();
                        obj.total_count = db.itemMasters.Where(x => x.SUPPLIERCODES == SupplierCode && x.CompanyId == CompanyId).Count();
                        obj.ordermaster = db.itemMasters.AsEnumerable().Where(x => x.SUPPLIERCODES == SupplierCode && x.CompanyId == CompanyId).Skip((page - 1) * list).Take(list).ToList();
                        return obj;
                    }
                }


            }
            catch (Exception ex)
            {
                logger.Error("Error in ItemMaster " + ex.Message);
                logger.Info("End  ItemMaster: ");
                return null;
            }
        }
        [HttpGet]
        [Route("Search")]
        public IEnumerable<ItemMaster> search(string key, string SupplierCode)
        {
            logger.Info("start Item Master: ");

            List<ItemMaster> ass = new List<ItemMaster>();
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

                using (AuthContext db = new AuthContext())
                {
                    int CompanyId = compid;
                    if (Warehouse_id > 0)
                    {
                        ass = db.itemMasters.Where(t => t.itemname.Contains(key) && t.Deleted == false && t.SUPPLIERCODES == SupplierCode && t.WarehouseId == Warehouse_id).ToList();
                        return ass;
                    }
                    else
                    {
                        return null;
                    }
                }


            }
            catch (Exception ex)
            {
                logger.Error("Error in Item Master " + ex.Message);
                logger.Info("End  Item Master: ");
                return null;
            }
        }

        [HttpGet]
        [Route("Searchinitemat")]
        public IEnumerable<ItemMasterCentral> searchs(string key)
        {
            logger.Info("start Item Master: ");
            List<ItemMasterCentral> ass = new List<ItemMasterCentral>();
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
                using (AuthContext db = new AuthContext())
                {
                    int CompanyId = compid;

                    ass = db.ItemMasterCentralDB.Where(t => t.Deleted == false && (t.itemname.ToLower().Contains(key.Trim().ToLower()) || t.Number.ToLower().Contains(key.Trim().ToLower())) && t.CompanyId == CompanyId).ToList();
                    if (ass != null)
                    {
                        var Numbers = ass.Select(x => x.Number).Distinct().ToList();
                        var ShelfLifes = db.ClearanceNonsShelfConfigurations.Where(x => Numbers.Contains(x.ItemNumber) && x.IsActive == true && x.IsDeleted == false).ToList();
                        if (ShelfLifes != null && ShelfLifes.Any())
                        {
                            foreach (var item in ass)
                            {
                                item.ClearanceFrom = ShelfLifes.FirstOrDefault(x => x.ItemNumber == item.Number) != null ? Convert.ToInt32(ShelfLifes.FirstOrDefault(x => x.ItemNumber == item.Number).ClearanceShelfLifeFrom) : 0;
                                item.ClearanceTo = ShelfLifes.FirstOrDefault(x => x.ItemNumber == item.Number) != null ? Convert.ToInt32(ShelfLifes.FirstOrDefault(x => x.ItemNumber == item.Number).ClearanceShelfLifeTo) : 0;
                                item.NonsellableFrom = ShelfLifes.FirstOrDefault(x => x.ItemNumber == item.Number) != null ? Convert.ToInt32(ShelfLifes.FirstOrDefault(x => x.ItemNumber == item.Number).NonSellShelfLifeFrom) : 0;
                                item.NonsellableTo = ShelfLifes.FirstOrDefault(x => x.ItemNumber == item.Number) != null ? Convert.ToInt32(ShelfLifes.FirstOrDefault(x => x.ItemNumber == item.Number).NonSellShelfLifeTo) : 0;
                            }
                        }

                    }
                    return ass;
                }
            }
            catch (Exception ex)
            {
                logger.Error("Error in Item Master " + ex.Message);
                logger.Info("End  Item Master: ");
                return null;
            }
        }

        [HttpGet]
        [Route("SearchinitemCentral")]  //SarthiApp
        public IEnumerable<ItemMasterCentral> SearchinitemCentral(string key)
        {
            logger.Info("start Item Master: ");
            List<ItemMasterCentral> ass = new List<ItemMasterCentral>();
            try
            {
                using (AuthContext db = new AuthContext())
                {
                    int CompanyId = 1;
                    ass = db.ItemMasterCentralDB.Where(t => t.Deleted == false && (t.itemname.ToLower().Contains(key.Trim().ToLower()) || t.Number.ToLower().Contains(key.Trim().ToLower())) && t.CompanyId == CompanyId).ToList();
                    if (ass != null)
                    {
                        var Numbers = ass.Select(x => x.Number).Distinct().ToList();
                        var ShelfLifes = db.ClearanceNonsShelfConfigurations.Where(x => Numbers.Contains(x.ItemNumber) && x.IsActive == true && x.IsDeleted == false).ToList();
                        if (ShelfLifes != null && ShelfLifes.Any())
                        {
                            foreach (var item in ass)
                            {
                                item.ClearanceFrom = ShelfLifes.FirstOrDefault(x => x.ItemNumber == item.Number) != null ? Convert.ToInt32(ShelfLifes.FirstOrDefault(x => x.ItemNumber == item.Number).ClearanceShelfLifeFrom) : 0;
                                item.ClearanceTo = ShelfLifes.FirstOrDefault(x => x.ItemNumber == item.Number) != null ? Convert.ToInt32(ShelfLifes.FirstOrDefault(x => x.ItemNumber == item.Number).ClearanceShelfLifeTo) : 0;
                                item.NonsellableFrom = ShelfLifes.FirstOrDefault(x => x.ItemNumber == item.Number) != null ? Convert.ToInt32(ShelfLifes.FirstOrDefault(x => x.ItemNumber == item.Number).NonSellShelfLifeFrom) : 0;
                                item.NonsellableTo = ShelfLifes.FirstOrDefault(x => x.ItemNumber == item.Number) != null ? Convert.ToInt32(ShelfLifes.FirstOrDefault(x => x.ItemNumber == item.Number).NonSellShelfLifeTo) : 0;
                            }
                        }

                    }
                    return ass;
                }
            }
            catch (Exception ex)
            {
                logger.Error("Error in Item Master " + ex.Message);
                logger.Info("End  Item Master: ");
                return null;
            }
        }

        //pooja k
        //For searching single item of Same Name
        [HttpGet]
        [Route("Searchinitemat7")]
        public IEnumerable<ItemMasterCentral> Searchinitemat7(string key)
        {
            logger.Info("start Item Master: ");
            List<ItemMasterCentral> ass = new List<ItemMasterCentral>();
            try
            {
                var identity = User.Identity as ClaimsIdentity;
                int compid = 0, userid = 0;
                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))
                    compid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                    userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

                using (AuthContext context = new AuthContext())
                {
                    ass = context.ItemMasterCentralDB.Where(t => !t.Deleted && t.IsSensitive && !t.IsSensitiveMRP && (t.itemname.ToLower().Contains(key.Trim().ToLower()) || t.Number.ToLower().Contains(key.Trim().ToLower())) && t.CompanyId == compid).GroupBy(n => new { n.Number }).Select(g => g.FirstOrDefault()).ToList();
                }
                return ass;
            }
            catch (Exception ex)
            {
                logger.Error("Error in Item Master " + ex.Message);
                logger.Info("End  Item Master: ");
                return null;
            }
        }

        [Route("getitemMaster")]
        [HttpGet]
        public IEnumerable<ItemMaster> MissingHSNCodeNull()
        {
            using (var db = new AuthContext())
            {
                logger.Info("start Item Master: ");
                List<ItemMaster> ass = new List<ItemMaster>();
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
                    var list = db.itemMasters.Where(p => p.HSNCode == null).ToList();
                    logger.Info("End  Item Master: ");
                    return list;
                }
                catch (Exception ex)
                {
                    logger.Error("Error in Item Master " + ex.Message);
                    logger.Info("End  Item Master: ");
                    return null;
                }
            }
        }
        //end

        //missingFiledCentral 
        [Route("getitemMastercentral")]
        [HttpGet]
        public IEnumerable<ItemMasterCentral> MissingHSNCodeCentralNull()
        {
            using (var db = new AuthContext())
            {
                logger.Info("start Item Master: ");
                List<ItemMasterCentral> ass = new List<ItemMasterCentral>();
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
                    var list = db.ItemMasterCentralDB.Where(p => p.HSNCode == null).ToList();
                    logger.Info("End  Item Master: ");
                    return list;
                }
                catch (Exception ex)
                {
                    logger.Error("Error in Item Master " + ex.Message);
                    logger.Info("End  Item Master: ");
                    return null;
                }
            }
        }
        //end
        [Route("getmissingdetail")]
        [HttpGet]
        public IEnumerable<ItemMaster> Missingdetail()
        {
            using (var db = new AuthContext())
            {
                logger.Info("start Item Master: ");
                List<ItemMaster> ass = new List<ItemMaster>();
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
                    var list = db.itemMasters.Where(p => p.HSNCode == null || p.itemname == null || p.SubcategoryName == null || p.SubsubcategoryName == null || p.CategoryName == null || p.price == 0 || p.LogoUrl == null || p.PurchaseUnitName == null || p.SellingUnitName == null).ToList();
                    logger.Info("End  Item Master: ");
                    return list;
                }
                catch (Exception ex)
                {
                    logger.Error("Error in Item Master " + ex.Message);
                    logger.Info("End  Item Master: ");
                    return null;
                }
            }
        }
        //end

        [HttpGet]
        [Route("WHSearchinitemat")]
        public object whbysearchs(string key)
        {
            logger.Info("start Item Master: ");
            List<ItemMaster> ass = new List<ItemMaster>();
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
                int CompanyId = compid;

                using (AuthContext db = new AuthContext())
                {
                    List<ItemMasterDTO> newdata = new List<ItemMasterDTO>();


                    newdata = (from a in db.itemMasters
                               where (a.itemname.ToLower().Contains(key.Trim().ToLower()) && a.Deleted == false && a.CompanyId == CompanyId && a.WarehouseId == Warehouse_id)
                               join b in db.ItemMasterCentralDB on a.SellingSku equals b.SellingSku
                               join c in db.DbCurrentStock on a.Number equals c.ItemNumber
                               where c.WarehouseId == Warehouse_id && c.ItemMultiMRPId == a.ItemMultiMRPId
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
                                   PurchaseMinOrderQty = b.PurchaseMinOrderQty,
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
                                   UOM = a.UOM,
                                   IsReplaceable = a.IsReplaceable,
                                   DistributionPrice = a.DistributionPrice,
                                   DistributorShow = a.DistributorShow
                               }).OrderByDescending(x => x.Number).ToList();






                    return newdata;

                }



            }
            catch (Exception ex)
            {
                logger.Error("Error in Item Master " + ex.Message);
                logger.Info("End  Item Master: ");
                return null;
            }
        }

        [HttpGet]
        [Route("WHSearchinitematAdmin")]
        public async System.Threading.Tasks.Task<List<ItemMasterDTO>> WHSearchinitematAdmin(string key, int warehouseid, string type = "")
        {
            logger.Info("start Item Master: ");
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

                using (var db = new AuthContext())
                {

                    if (type == "ConsumerType")
                    {
                        var companyId = new SqlParameter("@CompanyId", CompanyId);
                        var warehouseId = new SqlParameter("@WarehouseId", Warehouse_id);
                        var Page = new SqlParameter("@page", 1);
                        var List = new SqlParameter("@list", 20);
                        var keyword = new SqlParameter("@keyword", key);
                        newdata = db.Database.SqlQuery<ItemMasterDTO>("EXEC GetMRPMediaItemMaster @CompanyId,@WarehouseId,@page,@list,@keyword", companyId, warehouseId, Page, List, keyword).ToList();
                        return newdata;
                    }
                    else
                    {
                        // db.Database.Log = s => Debug.WriteLine(s);

                        newdata = (from a in db.itemMasters
                                   join c in db.SubCategorys
                                   on a.SubCategoryId equals c.SubCategoryId
                                   where (a.Deleted == false && a.WarehouseId == Warehouse_id
                                   && (a.itemname.ToLower().Contains(key.Trim().ToLower()) || a.Number.ToLower().Contains(key.Trim().ToLower()) || a.SellingSku.ToLower().Contains(key.Trim().ToLower()))
                                   && c.IsActive == true && c.Deleted == false
                                   )
                                   //join b in db.ItemMasterCentralDB on a.SellingSku equals b.SellingSku
                                   //join c in db.DbCurrentStock on a.ItemMultiMRPId equals c.ItemMultiMRPId 
                                   //where (c.WarehouseId == Warehouse_id && c.ItemMultiMRPId == a.ItemMultiMRPId)
                                   select new ItemMasterDTO
                                   {

                                       Categoryid = a.Categoryid,
                                       BaseCategoryid = a.BaseCategoryid,
                                       SubCategoryId = a.SubCategoryId,
                                       SubsubCategoryid = a.SubsubCategoryid,
                                       itemname = a.itemname,
                                       itemBaseName = a.itemBaseName,
                                       BaseCategoryName = a.BaseCategoryName,
                                       CategoryName = a.CategoryName,
                                       SubcategoryName = a.SubcategoryName,
                                       SubsubcategoryName = a.SubsubcategoryName,
                                       SubSubCode = a.SubSubCode,
                                       TGrpName = a.TGrpName,
                                       Number = a.Number,
                                       SellingUnitName = a.SellingUnitName,
                                       PurchaseUnitName = a.PurchaseUnitName,
                                       TotalTaxPercentage = a.TotalTaxPercentage,
                                       LogoUrl = a.LogoUrl,
                                       MinOrderQty = a.MinOrderQty,
                                       PurchaseMinOrderQty = a.PurchaseMinOrderQty,
                                       PurchaseSku = a.PurchaseSku,
                                       price = a.price,
                                       SellingSku = a.SellingSku,
                                       GruopID = a.GruopID,
                                       CessGrpID = a.CessGrpID,
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
                                       CurrentStock = 0,//c.CurrentInventory,
                                       ItemMultiMRPId = a.ItemMultiMRPId,
                                       UnitofQuantity = a.UnitofQuantity,
                                       IsSensitive = a.IsSensitive,
                                       IsSensitiveMRP = a.IsSensitiveMRP,
                                       UOM = a.UOM,
                                       NetPurchasePrice = a.NetPurchasePrice,
                                       IsReplaceable = a.IsReplaceable,
                                       DistributionPrice = a.DistributionPrice,
                                       DistributorShow = a.DistributorShow,
                                       ShowTypes = a.ShowTypes,
                                       ItemAppType = a.ItemAppType,
                                       DepoId = a.DepoId,
                                       BuyerId = a.BuyerId,
                                       IsDisContinued = a.IsDisContinued,
                                       IsSellerStoreItem = a.IsSellerStoreItem,
                                       SellerStorePrice = a.SellerStorePrice,
                                       TradePrice = a.TradePrice,
                                       WholeSalePrice = a.WholeSalePrice,
                                       EnableAutoPrice = c.EnableAutoPrice,
                                       IsCommodity = a.IsCommodity
                                   }).ToList();


                        if (newdata != null)
                        {
                            newdata.ForEach(x => x.ItemLimitId = db.ItemLimitMasterDB.FirstOrDefault(z => z.ItemNumber == x.Number && z.WarehouseId == x.WarehouseId && z.ItemMultiMRPId == x.ItemMultiMRPId)?.Id);

                            var itemMultimrpids = newdata.Select(x => x.ItemMultiMRPId).Distinct().ToList();

                            var StockList = db.DbCurrentStock.Where(a => a.Deleted == false && a.WarehouseId == warehouseid && itemMultimrpids.Contains(a.ItemMultiMRPId)).ToList();

                            foreach (var st in newdata)
                            {
                                if (StockList.Any(x => x.ItemMultiMRPId == st.ItemMultiMRPId))
                                {
                                    st.CurrentStock = StockList.FirstOrDefault(x => x.ItemMultiMRPId == st.ItemMultiMRPId).CurrentInventory;
                                }

                            }

                            List<ItemClassificationDC> ABCitemsList = newdata.Select(item => new ItemClassificationDC { ItemNumber = item.Number, WarehouseId = item.WarehouseId }).ToList();

                            try
                            {
                                var manager = new ItemLedgerManager();
                                var GetItem = await manager.GetItemClassificationsAsync(ABCitemsList);
                                foreach (var item in newdata)
                                {

                                    if (GetItem != null && GetItem.Any())
                                    {
                                        if (GetItem.Any(x => x.ItemNumber == item.Number))
                                        {
                                            item.ABCClassification = GetItem.FirstOrDefault(x => x.ItemNumber == item.Number).Category;
                                        }
                                        else { item.ABCClassification = "D"; }
                                    }
                                    else { item.ABCClassification = "D"; }

                                }
                            }
                            catch (Exception s) { }

                        }

                        if (newdata != null && newdata.Any())
                        {
                            TripPlannerHelper tripPlannerHelper = new TripPlannerHelper();
                            List<ItemWarehouseDc> itemWarehouseDcs = new List<ItemWarehouseDc>();
                            var itemWarehouse = newdata.Select(x => new ItemWarehouseDc { WarehouseId = x.WarehouseId, ItemMultiMRPId = x.ItemMultiMRPId }).ToList();
                            var list = await tripPlannerHelper.RocTagValueGet(itemWarehouse);
                            if (list != null && list.Any())
                            {
                                foreach (var da in newdata)
                                {
                                    da.Tag = list.Where(x => x.ItemMultiMRPId == da.ItemMultiMRPId && x.WarehouseId == da.WarehouseId).Select(x => x.Tag).FirstOrDefault();
                                }
                            }
                        }
                        return newdata;
                    }
                }

            }
            catch (Exception ex)
            {
                logger.Error("Error in Item Master " + ex.Message);
                logger.Info("End  Item Master: ");
                return null;
            }
        }

        [HttpGet]
        [Route("SearchinitemWid")]
        public IEnumerable<ItemMaster> searchss(string key, int WarehouseId)
        {
            logger.Info("start Item Master: ");
            List<ItemMaster> ass = new List<ItemMaster>();

            List<ItemMaster> result = new List<ItemMaster>();
            try
            {
                using (AuthContext db = new AuthContext())
                {
                    ass = db.itemMasters.Where(t => t.itemname.ToLower().Contains(key.Trim().ToLower()) && t.WarehouseId == WarehouseId && t.Deleted == false).ToList();

                    List<string> PurchaseSku = new List<string>();

                    foreach (var item in ass)
                    {
                        var items = ass.Where(t => t.PurchaseSku == item.PurchaseSku).FirstOrDefault();

                        if (items != null && !PurchaseSku.Any(x => x == items.PurchaseSku))
                        {
                            result.Add(items);
                            PurchaseSku.Add(items.PurchaseSku);
                        }
                    }
                    return result;
                }
            }
            catch (Exception ex)
            {
                logger.Error("Error in Item Master " + ex.Message);
                logger.Info("End  Item Master: ");
                return null;
            }
        }

        [HttpGet]
        [Route("Searchinitem")]
        public IEnumerable<ItemMaster> searchss(string key)
        {
            logger.Info("start Item Master: ");
            List<ItemMaster> ass = new List<ItemMaster>();

            List<ItemMaster> result = new List<ItemMaster>();
            try
            {
                using (AuthContext db = new AuthContext())
                {
                    ass = db.itemMasters.Where(t => t.itemname.Contains(key) && t.Deleted == false).ToList();

                    List<string> PurchaseSku = new List<string>();

                    foreach (var item in ass)
                    {
                        var items = ass.Where(t => t.PurchaseSku == item.PurchaseSku).FirstOrDefault();

                        if (items != null && !PurchaseSku.Any(x => x == items.PurchaseSku))
                        {
                            result.Add(items);
                            PurchaseSku.Add(items.PurchaseSku);
                        }
                    }

                    return result;
                }
            }
            catch (Exception ex)
            {
                logger.Error("Error in Item Master " + ex.Message);
                logger.Info("End  Item Master: ");
                return null;
            }
        }

        [HttpGet]
        [Route("freeItem")]
        public IEnumerable<ItemMaster> freeItem()
        {

            List<ItemMaster> ass = new List<ItemMaster>();
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
                int CompanyId = compid;
                using (AuthContext db = new AuthContext())
                {
                    if (Warehouse_id > 0)
                    {
                        ass = db.itemMasters.Where(f => f.Deleted == false && f.free == true && f.CompanyId == CompanyId && f.WarehouseId == Warehouse_id).ToList();
                        logger.Info("End  Item Master: ");
                        return ass;
                    }

                    else
                    {

                        return null;
                    }
                }

            }
            catch (Exception ex)
            {
                logger.Error("Error in Item Master " + ex.Message);
                logger.Info("End  Item Master: ");
                return null;
            }
        }
        [Authorize]
        [ResponseType(typeof(ItemMaster))]
        [Route("")]
        public IEnumerable<ItemMaster> Get(string categoryid, string subcategoryid, string subsubcategoryid)
        {
            using (var context = new AuthContext())
            {
                logger.Info("start ItemMaster: ");
                List<ItemMaster> ass = new List<ItemMaster>();
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
                    int CompanyId = compid;
                    logger.Info("User ID : {0} , Company Id : {1}", compid, userid);

                    if (Warehouse_id > 0)
                    {
                        ass = context.filteredItemMasterWid(categoryid, subcategoryid, subsubcategoryid, CompanyId, Warehouse_id).ToList();
                        logger.Info("End ItemMaster: ");
                        return ass;
                    }
                    else
                    {
                        ass = context.filteredItemMaster(categoryid, subcategoryid, subsubcategoryid, CompanyId).ToList();
                        logger.Info("End ItemMaster: ");
                        return ass;
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




        #region Warehouse Based Filter
        [Route("")]
        public async Task<List<ItemMaster>> Get1(int WarehouseId, string categoryid, string subcategoryid, string subsubcategoryid)
        {
            using (var context = new AuthContext())
            {
                logger.Info("edit price get ItemMaster: ");

                DateTime startDate = indianTime.AddDays(-30);
                DateTime endDate = indianTime;

                List<ItemMaster> itemlist = new List<ItemMaster>();
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
                int CompanyId = compid;
                int CategoryID = Convert.ToInt32(categoryid.Trim());
                int SubCategoryID = Convert.ToInt32(subcategoryid.Trim());
                int SubSubCategoryID = Convert.ToInt32(subsubcategoryid.Trim());
                List<ItemMaster> list = new List<ItemMaster>();

                if (CategoryID > 0 || SubCategoryID > 0 || SubSubCategoryID > 0)
                {
                    itemlist = await context.itemMasters.Where(x => x.Categoryid == CategoryID && x.WarehouseId == WarehouseId && x.SubCategoryId == SubCategoryID && x.SubsubCategoryid == SubSubCategoryID && x.active == true && x.Deleted == false && x.CompanyId == CompanyId).ToListAsync();

                    if (itemlist != null && itemlist.Any())
                    {

                        #region get Abc Classification
                        var result = itemlist.GroupBy(test => test.Number).Select(grp => grp.First()).ToList();
                        List<ItemClassificationDC> ABCitemsList = result.Select(item => new ItemClassificationDC { ItemNumber = item.Number, WarehouseId = item.WarehouseId, Category = item.CategoryName }).ToList();
                        var manager = new ItemLedgerManager();
                        var ABCItemsListResult = await manager.GetItemClassificationsAsync(ABCitemsList);
                        #endregion

                        //for sp
                        List<Object> parameters = new List<object>();
                        List<EditItemPriceAppDTO> Appdata = new List<EditItemPriceAppDTO>();
                        string sqlquery = "exec EditItemPriceApp";
                        parameters.Add(new SqlParameter("@startDate", startDate));
                        parameters.Add(new SqlParameter("@endDate", endDate));
                        sqlquery = sqlquery + " @startDate" + ", @endDate";
                        parameters.Add(new SqlParameter("@warehouseId", WarehouseId));
                        sqlquery = sqlquery + ", @warehouseId";
                        Appdata = await context.Database.SqlQuery<EditItemPriceAppDTO>(sqlquery, parameters.ToArray()).ToListAsync();
                        ItemMasterManager itemMasterManager = new ItemMasterManager();
                        var itemMultiMRPIds = new List<int>();
                        itemMultiMRPIds.AddRange(itemlist.Select(x => x.ItemMultiMRPId).Distinct().ToList());
                        var ItemIncentiveClassificationMargin = await itemMasterManager.GetItemIncentiveClassificationMargin(WarehouseId, itemMultiMRPIds);
                        JITLiveItemManager jitManager = new JITLiveItemManager();
                        foreach (var item in itemlist)
                        {
                            OpenMoqFilterDc openMoqFilterDc = new OpenMoqFilterDc();
                            openMoqFilterDc.WarehouseId = WarehouseId;
                            openMoqFilterDc.ItemMultiMrpId = item.ItemMultiMRPId;
                            var MarginUpto = await jitManager.GetOpenMoqListAsync(openMoqFilterDc);
                            if (MarginUpto != null && MarginUpto.Any(x => x.ItemId == item.ItemId))
                                //item.MarginUpto = MarginUpto.FirstOrDefault(x => x.ItemId == item.ItemId).MarginUpto;
                                item.WholesaleRMargin = MarginUpto.FirstOrDefault(x => x.ItemId == item.ItemId).WholesaleRMargin;
                            item.TradeRMargin = MarginUpto.FirstOrDefault(x => x.ItemId == item.ItemId).TradeRMargin;
                            item.MarginUpto = MarginUpto.FirstOrDefault(x => x.ItemId == item.ItemId).RetailerRMargin;
                            if (ABCItemsListResult != null && ABCItemsListResult.Any())
                            {
                                if (ABCItemsListResult.Any(x => x.ItemNumber == item.Number))
                                {
                                    item.CategoryName = ABCItemsListResult.FirstOrDefault(x => x.ItemNumber == item.Number).Category;
                                }
                                else { item.CategoryName = "D"; }  //D Mean not saled
                            }
                            if (ItemIncentiveClassificationMargin != null && ItemIncentiveClassificationMargin.Any())
                            {
                                if (ItemIncentiveClassificationMargin.Any(x => x.ItemMultiMrpId == item.ItemMultiMRPId))
                                {
                                    item.Classification = ItemIncentiveClassificationMargin.FirstOrDefault(x => x.ItemMultiMrpId == item.ItemMultiMRPId).Classification;
                                }
                            }

                            if (Appdata.Any(x => x.ItemMultiMRPId == item.ItemMultiMRPId))
                            {
                                item.AveragePurchasePrice = Appdata.First(x => x.ItemMultiMRPId == item.ItemMultiMRPId).AvgPurPrice;//get App
                            }
                            item.WithTaxNetPurchasePrice = Math.Round(item.NetPurchasePrice * (1 + (item.TotalTaxPercentage / 100)), 3);//With tax
                            if (item.POPurchasePrice == 0 || item.POPurchasePrice == null)
                            {
                                item.POPurchasePrice = item.PurchasePrice;
                            }

                        }
                    }

                }
                if (itemlist != null)
                {
                    TripPlannerHelper tripPlannerHelper = new TripPlannerHelper();
                    List<ItemWarehouseDc> itemWarehouseDcs = new List<ItemWarehouseDc>();
                    var itemWarehouse = itemlist.Select(x => new ItemWarehouseDc { WarehouseId = x.WarehouseId, ItemMultiMRPId = x.ItemMultiMRPId }).ToList();
                    var listmd = tripPlannerHelper.RocTagValueGet(itemWarehouse);
                    if (listmd != null)
                    {
                        foreach (var da in itemlist)
                        {
                            da.Tag = listmd.Result.Where(x => x.ItemMultiMRPId == da.ItemMultiMRPId && x.WarehouseId == da.WarehouseId).Select(x => x.Tag).FirstOrDefault();
                        }
                    }

                }


                return itemlist;
            }

        }
        #endregion
        //[Route("")]
        //public List<ItemMaster> Get(string type, int id, int Wid)
        //{
        //    List<ItemMaster> item = new List<ItemMaster>();

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
        //        int CompanyId = compid;
        //        using (AuthContext db = new AuthContext())
        //        {
        //            item = db.itemMasters.Where(c => c.WarehouseId == Wid && c.CompanyId == CompanyId).GroupBy(x => x.Number).Select(x => x.FirstOrDefault()).ToList();

        //        }

        //    }
        //    catch (Exception ex)
        //    {
        //        item = null;
        //    }

        //    return item;
        //}
        [Route("")]
        public ItemMaster getitembyid(string id)
        {

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
                    int CompanyId = compid;
                    int idd = Convert.ToInt32(id);
                    ItemMaster mk = new ItemMaster();

                    mk = context.itembyid(idd, CompanyId).FirstOrDefault();
                    return mk;
                }
                catch (Exception ex)
                {
                    logger.Error("Error in ItemMaster " + ex.Message);
                    logger.Info("End  ItemMaster: ");
                    return null;
                }
            }

        }

        [Route("")]
        public List<ItemMaster> getitembyitemname(string itemname, string x)
        {
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
                    int CompanyId = compid;
                    List<ItemMaster> itemList = new List<ItemMaster>();
                    var mk = context.itembystring(itemname, CompanyId).ToList();
                    if (mk.Count != 0)
                    {
                        foreach (var itmm in mk)
                        {
                            itmm.itemname = itmm.SellingUnitName;
                            itemList.Add(itmm);
                        }
                    }
                    else
                    {
                        itemList = null;
                    }
                    return itemList;
                }
                catch (Exception ex)
                {
                    logger.Error("Error in ItemMaster " + ex.Message);
                    logger.Info("End  ItemMaster: ");
                    return null;
                }
            }
        }

        [Route("")]
        public List<ItemMaster> post(string type, itemselected id)
        {
            using (var context = new AuthContext())
            {
                List<ItemMaster> items = new List<ItemMaster>();
                if (type == "ids")
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
                        int CompanyId = compid;

                        foreach (var i in id.ItemId)
                        {
                            ItemMaster mk = new ItemMaster();
                            int idd = Convert.ToInt32(i);
                            mk = context.itembyid(idd, CompanyId).FirstOrDefault();
                            items.Add(mk);
                        }
                    }
                    catch (Exception ex)
                    {
                        return null;
                    }
                }
                return items;
            }
        }

        [Authorize]
        [Route("")]
        [AcceptVerbs("POST")]
        public ItemMasterCentral add(ItemMasterCentral item)
        {

            using (var context = new AuthContext())
            {
                logger.Info("start addItem Master: ");
                try
                {

                    var identity = User.Identity as ClaimsIdentity;
                    int compid = 0, userid = 0;

                    if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))
                        compid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);

                    if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                        userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

                    item.CompanyId = compid;
                    item.userid = compid;


                    if (item == null)
                    {
                        throw new ArgumentNullException("item");
                    }
                    else if (item.SubCategoryId > 0)
                    {
                        string SubcateogryName = context.SubCategorys.Where(x => x.SubCategoryId == item.SubCategoryId).Select(y => y.SubcategoryName).FirstOrDefault();

                        if (SubcateogryName.ToUpper().Equals("KISAN KIRANA"))
                        {
                            item.BomId = GenerateUniqueNumber();
                        }
                        else
                        {
                            item.BomId = 0;
                        }

                        item.Type = 1;

                        // context.AddItemMaster(item);
                        AddItemMaster(item, context);
                        logger.Info("User ID : {0} , Company Id : {1} AddItemMaster", compid, userid);
                        // CommonHelper.refreshItemMaster(item.Categoryid);
                        logger.Info("End  addItem Master: ");
                    }
                    return item;
                }
                catch (Exception ex)
                {
                    logger.Error("Error in Add Item Master " + ex.Message);
                    return null;
                }
            }
        }




        [ResponseType(typeof(ItemMaster))]
        [Route("")]
        [AcceptVerbs("PUT")]
        public bool Put(ItemMaster item)
        {
            using (var context = new AuthContext())
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
                if (!item.IsRunBothMRP)
                {
                    item.CompanyId = compid;
                    item.userid = userid;
                    //ItemMaster itm = context.PutItemMaster(item);
                    //Change by Sudeep Solanki & Harry
                    var DisplayName = context.Peoples.FirstOrDefault(x => x.PeopleID == userid)?.DisplayName;
                    ItemMaster itemmaster = context.itemMasters.Where(x => x.ItemId == item.ItemId && x.WarehouseId == item.WarehouseId).SingleOrDefault();
                    var UPrice = itemmaster.UnitPrice;
                    bool SellingSkuMRPIdNotExit = context.itemMasters.Any(x => x.SellingSku == itemmaster.SellingSku && x.ItemId != itemmaster.ItemId && x.MinOrderQty == itemmaster.MinOrderQty && x.ItemMultiMRPId == item.ItemMultiMRPId && x.WarehouseId == itemmaster.WarehouseId);

                    #region For IsCommodity
                    if (itemmaster != null)
                    {
                        var ItemNumbers = context.itemMasters.Where(x => x.Number == itemmaster.Number && x.WarehouseId == item.WarehouseId).ToList();
                        foreach (var number in ItemNumbers)
                        {
                            number.IsCommodity = item.IsCommodity;
                            context.Entry(number).State = EntityState.Modified;
                        }
                    }

                    #endregion

                    if (itemmaster != null && !SellingSkuMRPIdNotExit)
                    {
                        if (!(item.PurchasePrice <= item.price && item.UnitPrice <= item.price && item.TradePrice <= item.price && item.WholeSalePrice <= item.price))
                        {
                            if (itemmaster.active == true)
                            {
                                itemmaster.active = false;
                                context.Entry(itemmaster).State = EntityState.Modified;
                                context.Commit();
                                itemmaster.active = true;
                            }
                            //itemmaster.PurchasePrice = item.price;
                            itemmaster.WholeSalePrice = item.price;
                            itemmaster.TradePrice = item.price;
                            itemmaster.UnitPrice = item.price;
                        }
                        else
                        {
                            itemmaster.TradePrice = item.TradePrice;
                            itemmaster.WholeSalePrice = item.WholeSalePrice;
                            itemmaster.UnitPrice = item.UnitPrice;
                        }

                        var BuyerName = context.Peoples.Where(x => x.PeopleID == item.BuyerId).Select(x => x.DisplayName).FirstOrDefault();

                        Supplier SN = context.Suppliers.FirstOrDefault(x => x.SupplierId == item.SupplierId && x.Deleted == false && x.CompanyId == item.CompanyId);
                        DepoMaster depo = context.DepoMasters.FirstOrDefault(x => x.DepoId == item.DepoId && x.Deleted == false);
                        itemmaster.SupplierId = SN != null ? SN.SupplierId : 0;
                        itemmaster.SupplierName = SN != null ? SN.Name : "";
                        itemmaster.SUPPLIERCODES = SN != null ? SN.SUPPLIERCODES : "";
                        itemmaster.DepoId = depo != null ? depo.DepoId : 0;
                        itemmaster.DepoName = depo != null ? depo.DepoName : null;
                        itemmaster.PurchaseMinOrderQty = item.PurchaseMinOrderQty;

                        itemmaster.PurchasePrice = item.PurchasePrice;
                        itemmaster.active = item.active;
                        itemmaster.itemname = item.itemname;
                        itemmaster.UpdatedDate = indianTime;
                        itemmaster.userid = userid;
                        itemmaster.IsSensitiveMRP = item.IsSensitiveMRP;
                        itemmaster.BuyerId = item.BuyerId;
                        itemmaster.BuyerName = BuyerName;
                        itemmaster.IsDisContinued = item.IsDisContinued;
                        double withouttaxvalue = (itemmaster.PurchasePrice / ((100 + itemmaster.TotalTaxPercentage) / 100));
                        double withouttax = Math.Round(withouttaxvalue, 3);
                        itemmaster.NetPurchasePrice = Math.Round((withouttax), 3);// without tax
                        itemmaster.IsReplaceable = item.IsReplaceable;
                        itemmaster.DistributionPrice = item.DistributionPrice;
                        itemmaster.DistributorShow = item.DistributorShow;
                        itemmaster.ShowTypes = item.ShowTypes;
                        itemmaster.ItemAppType = item.ItemAppType;
                        itemmaster.ModifiedBy = userid;
                        itemmaster.IsSellerStoreItem = item.IsSellerStoreItem; //added
                        itemmaster.SellerStorePrice = item.SellerStorePrice;//added

                        //if (item.IsReplaceable)
                        // {
                        var replaceableItem = context.itemMasters.Where(x => x.ItemMultiMRPId == item.ItemMultiMRPId && x.WarehouseId == item.WarehouseId).ToList();
                        replaceableItem.ForEach(y =>
                        {
                            y.IsReplaceable = item.IsReplaceable;
                            context.Entry(y).State = EntityState.Modified;
                        });
                        //}

                        if (item.IsSensitive == true && item.IsSensitiveMRP == true)
                        {
                            itemmaster.itemname = item.itemBaseName + " " + item.price + " MRP " + item.UnitofQuantity + " " + item.UOM;
                        }
                        else if (item.IsSensitive == true && item.IsSensitiveMRP == false)
                        {
                            itemmaster.itemname = item.itemBaseName + " " + item.UnitofQuantity + " " + item.UOM; //item display name 
                        }

                        else if (item.IsSensitive == false && item.IsSensitiveMRP == false)
                        {
                            itemmaster.itemname = item.itemBaseName; //item display name
                        }
                        else if (item.IsSensitive == false && item.IsSensitiveMRP == true)
                        {
                            itemmaster.itemname = item.itemBaseName + " " + item.price + " MRP";//item display name 
                        }

                        itemmaster.SellingUnitName = itemmaster.itemname + " " + itemmaster.MinOrderQty + "Unit";//item selling unit name
                        itemmaster.PurchaseUnitName = itemmaster.itemname + " " + itemmaster.PurchaseMinOrderQty + "Unit";//
                        var mrpdata1 = context.ItemMultiMRPDB.FirstOrDefault(x => x.ItemMultiMRPId == item.ItemMultiMRPId);
                        itemmaster.price = mrpdata1.MRP; //Change MRP Price
                        itemmaster.MRP = mrpdata1.MRP; //Change MRP Price
                        itemmaster.UnitofQuantity = mrpdata1.UnitofQuantity;
                        itemmaster.UOM = item.UOM;
                        itemmaster.IsSensitiveMRP = item.IsSensitiveMRP;

                        //itemmaster.IsCommodity = item.IsCommodity;
                        bool iscurrentStockAdded = false;
                        if (itemmaster.ItemMultiMRPId != item.ItemMultiMRPId)
                        {

                            itemmaster.IsSensitive = item.IsSensitive;
                            if (item.IsSensitive == true && item.IsSensitiveMRP == true)
                            {
                                itemmaster.itemname = item.itemBaseName + " " + item.price + " MRP " + item.UnitofQuantity + " " + item.UOM;
                            }
                            else if (item.IsSensitive == true && item.IsSensitiveMRP == false)
                            {
                                itemmaster.itemname = item.itemBaseName + " " + item.UnitofQuantity + " " + item.UOM; //item display name 
                            }

                            else if (item.IsSensitive == false && item.IsSensitiveMRP == false)
                            {
                                itemmaster.itemname = item.itemBaseName; //item display name
                            }
                            else if (item.IsSensitive == false && item.IsSensitiveMRP == true)
                            {
                                itemmaster.itemname = item.itemBaseName + " " + item.price + " MRP";//item display name 
                            }

                            itemmaster.SellingUnitName = itemmaster.itemname + " " + itemmaster.MinOrderQty + "Unit";//item selling unit name
                            itemmaster.PurchaseUnitName = itemmaster.itemname + " " + itemmaster.PurchaseMinOrderQty + "Unit";//

                            // mrp
                            var mrpdata = context.ItemMultiMRPDB.FirstOrDefault(x => x.ItemMultiMRPId == item.ItemMultiMRPId);

                            itemmaster.price = mrpdata.MRP; //Change MRP Price
                            itemmaster.UnitofQuantity = mrpdata.UnitofQuantity;
                            itemmaster.UOM = item.UOM;
                            itemmaster.IsSensitiveMRP = item.IsSensitiveMRP;

                            #region change same number item name
                            var mrp = context.itemMasters.Where(x => x.Number == itemmaster.Number && x.ItemId != item.ItemId && x.ItemMultiMRPId == itemmaster.ItemMultiMRPId && x.WarehouseId == item.WarehouseId).ToList();
                            foreach (var mrpitem in mrp)
                            {
                                var SellingSkuMRPIdNotExits = context.itemMasters.Any(x => x.SellingSku == mrpitem.SellingSku && x.ItemId != mrpitem.ItemId && x.MinOrderQty == mrpitem.MinOrderQty && x.WarehouseId == itemmaster.WarehouseId);

                                //bool isexsits = SellingSkuMRPIdNotExits.Any(x=>x.MinOrderQty== mrpitem.MinOrderQty);
                                if (!SellingSkuMRPIdNotExits)
                                { //update other selling sku Mrp Id
                                  //mrpitem.ItemMultiMRPId = item.ItemMultiMRPId; //change mrp itemid

                                    if (itemmaster.IsSensitive == false)
                                    {
                                        mrpitem.ItemMultiMRPId = item.ItemMultiMRPId; // if IsSensitive false
                                    }
                                    else if (itemmaster.IsSensitive == true && itemmaster.IsSensitiveMRP == true)
                                    {
                                        mrpitem.ItemMultiMRPId = item.ItemMultiMRPId; // if both true
                                    }


                                    mrpitem.price = mrpdata.MRP; //Change MRP Price
                                    mrpitem.MRP = mrpdata.MRP; //Change MRP Price
                                    mrpitem.UnitofQuantity = mrpdata.UnitofQuantity;
                                    mrpitem.UOM = item.UOM;
                                    mrpitem.PurchaseMinOrderQty = itemmaster.PurchaseMinOrderQty;//item PurchaseUnitName name
                                                                                                 //Imrp.itemname = objItemMaster.itemBaseName + " " + Imrp.price + " MRP " + Imrp.UnitofQuantity + " " + Imrp.UOM; //item display name   

                                    if (item.IsSensitive == true && item.IsSensitiveMRP == true)
                                    {
                                        mrpitem.itemname = item.itemBaseName + " " + item.price + " MRP " + item.UnitofQuantity + " " + item.UOM;
                                    }
                                    else if (item.IsSensitive == true && item.IsSensitiveMRP == false)
                                    {
                                        mrpitem.itemname = item.itemBaseName + " " + item.UnitofQuantity + " " + item.UOM; //item display name 
                                    }

                                    else if (item.IsSensitive == false && item.IsSensitiveMRP == false)
                                    {
                                        mrpitem.itemname = item.itemBaseName; //item display name
                                    }
                                    else if (item.IsSensitive == false && item.IsSensitiveMRP == true)
                                    {
                                        mrpitem.itemname = item.itemBaseName + " " + item.price + " MRP";//item display name 
                                    }

                                    mrpitem.SellingUnitName = mrpitem.itemname + " " + mrpitem.MinOrderQty + "Unit";//item selling unit name
                                    mrpitem.PurchaseUnitName = mrpitem.itemname + " " + itemmaster.PurchaseMinOrderQty + "Unit";//item PurchaseUnitName name
                                    mrpitem.UpdatedDate = indianTime;
                                    context.Entry(mrpitem).State = EntityState.Modified;

                                }
                            }
                            #endregion
                            if (itemmaster.IsSensitive == false)
                            {
                                itemmaster.ItemMultiMRPId = item.ItemMultiMRPId; // if IsSensitive false
                            }
                            else if (itemmaster.IsSensitive == true && itemmaster.IsSensitiveMRP == true)
                            {
                                itemmaster.ItemMultiMRPId = item.ItemMultiMRPId; // if both true
                            }
                            #region change current stock item name
                            CurrentStock cntstock = new CurrentStock();

                            cntstock = context.DbCurrentStock.Where(x => x.ItemNumber == itemmaster.Number && x.WarehouseId == itemmaster.WarehouseId && x.CompanyId == itemmaster.CompanyId && x.ItemMultiMRPId == itemmaster.ItemMultiMRPId && x.Deleted == false).SingleOrDefault();
                            if (cntstock == null)
                            {
                                CurrentStock newCstk = new CurrentStock();
                                newCstk.CompanyId = itemmaster.CompanyId;
                                newCstk.itemBaseName = itemmaster.itemBaseName;
                                newCstk.itemname = itemmaster.itemname;
                                newCstk.ItemNumber = itemmaster.Number;

                                newCstk.WarehouseId = itemmaster.WarehouseId;
                                newCstk.WarehouseName = itemmaster.WarehouseName;
                                newCstk.CurrentInventory = 0;
                                newCstk.CreationDate = indianTime;
                                newCstk.UpdatedDate = indianTime;
                                newCstk.MRP = itemmaster.price;
                                newCstk.UnitofQuantity = itemmaster.UnitofQuantity;
                                newCstk.UOM = itemmaster.UOM;
                                newCstk.ItemMultiMRPId = item.ItemMultiMRPId;
                                iscurrentStockAdded = true;
                                context.DbCurrentStock.Add(newCstk);
                            }
                            else
                            {
                                cntstock.itemname = itemmaster.itemname;
                                cntstock.ItemNumber = itemmaster.Number;

                                cntstock.WarehouseId = itemmaster.WarehouseId;
                                cntstock.WarehouseName = itemmaster.WarehouseName;
                                cntstock.UpdatedDate = indianTime;
                                cntstock.Deleted = false;

                                cntstock.itemBaseName = itemmaster.itemBaseName;
                                context.Entry(cntstock).State = EntityState.Modified;

                                CurrentStockHistory Oss = new CurrentStockHistory();

                                Oss.StockId = cntstock.StockId;
                                Oss.ItemNumber = cntstock.ItemNumber;
                                Oss.itemname = cntstock.itemname;
                                Oss.TotalInventory = cntstock.CurrentInventory;
                                Oss.WarehouseName = cntstock.WarehouseName;
                                Oss.Warehouseid = cntstock.WarehouseId;
                                Oss.CompanyId = cntstock.CompanyId;
                                Oss.userid = userid;
                                Oss.UserName = DisplayName;
                                Oss.CreationDate = indianTime;
                                Oss.MRP = itemmaster.price;
                                Oss.UnitofQuantity = itemmaster.UnitofQuantity;
                                Oss.UOM = itemmaster.UOM;
                                Oss.itemBaseName = itemmaster.itemBaseName;
                                context.CurrentStockHistoryDb.Add(Oss);

                            }
                            #endregion
                        }
                        //new code for no for run both mrp as no,.
                        #region  new stock crate with 0 inventory
                        if (!iscurrentStockAdded)
                        {

                            CurrentStock cruntStock = new CurrentStock();

                            cruntStock = context.DbCurrentStock.Where(x => x.ItemNumber == item.Number && x.WarehouseId == item.WarehouseId && x.CompanyId == item.CompanyId && x.ItemMultiMRPId == item.ItemMultiMRPId && x.Deleted == false).SingleOrDefault();
                            if (cruntStock == null)
                            {
                                CurrentStock newCstk = new CurrentStock();
                                newCstk.CompanyId = itemmaster.CompanyId;
                                newCstk.itemBaseName = itemmaster.itemBaseName;
                                newCstk.itemname = itemmaster.itemname;
                                newCstk.ItemNumber = itemmaster.Number;

                                newCstk.WarehouseId = itemmaster.WarehouseId;
                                newCstk.WarehouseName = itemmaster.WarehouseName;
                                newCstk.CurrentInventory = 0;
                                newCstk.CreationDate = indianTime;
                                newCstk.UpdatedDate = indianTime;
                                newCstk.MRP = itemmaster.price;
                                newCstk.UnitofQuantity = itemmaster.UnitofQuantity;
                                newCstk.UOM = itemmaster.UOM;
                                newCstk.ItemMultiMRPId = item.ItemMultiMRPId;
                                context.DbCurrentStock.Add(newCstk);
                            }
                        }


                        #endregion
                        SupplierWarehouse SupplierWarehouse = context.SupplierWarehouseDB.FirstOrDefault(x => x.WarehouseId == item.WarehouseId && x.Deleted == false && x.SupplierId == itemmaster.SupplierId && x.DepoId == itemmaster.DepoId);
                        if (SupplierWarehouse == null)
                        {
                            SupplierWarehouse SW = new SupplierWarehouse();
                            SW.WarehouseId = itemmaster.WarehouseId;
                            SW.CompanyId = itemmaster.CompanyId;
                            SW.SupplierId = itemmaster.SupplierId;
                            SW.DepoId = depo?.DepoId;
                            SW.DepoName = depo?.DepoName;
                            SW.WarehouseName = itemmaster.WarehouseName;
                            SW.CreatedDate = indianTime;
                            SW.UpdatedDate = indianTime;
                            SW.Deleted = false;
                            context.SupplierWarehouseDB.Add(SW);
                        }

                        #region SellerActiveProduct place1
                        if (itemmaster.active)
                        {
                            bool IsSellerActiveProducts = context.SellerActiveProducts.Any(x => x.ItemNumber == itemmaster.Number && x.WarehouseId == itemmaster.WarehouseId);
                            if (!IsSellerActiveProducts)
                            {
                                SellerActiveProduct SellerItem = new SellerActiveProduct();
                                SellerItem.ItemNumber = itemmaster.Number;
                                SellerItem.CreatedBy = userid;
                                SellerItem.WarehouseId = itemmaster.WarehouseId;
                                SellerItem.ActivatedDate = indianTime;
                                context.SellerActiveProducts.Add(SellerItem);
                            }
                        }
                        #endregion
                        #region item price drop Entry for sales app
                        if (item.UnitPrice > 0 && UPrice > 0)
                        {
                            if (item.UnitPrice < UPrice)
                            {
                                ItemPriceDrop itemPriceDrop = new ItemPriceDrop();
                                itemPriceDrop.UnitPrice = item.UnitPrice;
                                itemPriceDrop.OldUnitPrice = UPrice;
                                itemPriceDrop.WarehouseId = item.WarehouseId;
                                itemPriceDrop.ItemId = item.ItemId;
                                itemPriceDrop.IsActive = true;
                                itemPriceDrop.IsDeleted = false;
                                itemPriceDrop.CreatedBy = userid;
                                itemPriceDrop.CreatedDate = DateTime.Now;
                                context.ItemPriceDrops.Add(itemPriceDrop);
                            }
                        }
                        #endregion
                        int count = context.Commit();
                        if (count > 0)
                        {

                            return true;
                        }

                        return false;
                    }
                    else if (itemmaster != null && itemmaster.active && item.active == false)
                    {
                        itemmaster.active = item.active;
                        itemmaster.ModifiedBy = userid;
                        itemmaster.UpdatedDate = indianTime;
                        //if (item.IsReplaceable)
                        //{
                        //    var replaceableItem = context.itemMasters.Where(x => x.ItemMultiMRPId == item.ItemMultiMRPId && x.WarehouseId == item.WarehouseId).ToList();
                        //    replaceableItem.ForEach(y =>
                        //    {
                        //        y.IsReplaceable = item.IsReplaceable;
                        //        context.Entry(y).State = EntityState.Modified;
                        //    });
                        //}
                        context.Entry(itemmaster).State = EntityState.Modified;
                        return context.Commit() > 0;
                    }
                    else
                        return true;
                }
                else
                {
                    bool IsCreated = AddItemInWarehouseMaster(item, context, userid);


                    return IsCreated;

                }
            }
        }


        #region Itemmaster Edit API
        /// <summary>
        /// Itemmaster  edit API
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        [ResponseType(typeof(ItemMaster))]
        [Route("PutCItem")]
        [AcceptVerbs("PUT")]

        public ItemMasterCentral PutCItem(ItemMasterCentral item)
        {
            using (var context = new AuthContext())
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

                item.userid = userid;
                item.CompanyId = compid;
                var DisplayName = context.Peoples.FirstOrDefault(x => x.PeopleID == userid)?.DisplayName;
                // ItemMasterCentral itm = context.PutCentralItemMaster(item);
                // change by sudeep & harry
                var itemcentral = context.ItemMasterCentralDB.FirstOrDefault(x => x.Id == item.Id);

                if (itemcentral != null && compid > 0 && userid > 0)
                {
                    var warehouseIds = context.Warehouses.Where(x => x.IsKPP == false).Select(y => y.WarehouseId);
                    var AllWarehouseItemList = context.itemMasters.Where(x => x.Number == itemcentral.Number && warehouseIds.Contains(x.WarehouseId)).ToList();

                    SubsubCategory st = context.SubsubCategorys.FirstOrDefault(x => x.SubsubCategoryid == item.SubsubCategoryid);
                    var category = context.Categorys.FirstOrDefault(x => x.Categoryid == item.Categoryid);
                    var subcategory = context.SubCategorys.FirstOrDefault(x => x.SubCategoryId == item.SubCategoryId);


                    double TotalTax = context.DbTaxGroupDetails.Where(x => x.GruopID == item.GruopID && x.CompanyId == compid).Sum(x => x.TPercent);
                    double TotalCessTax = item.CessGrpID != null && item.CessGrpID > 0 ? context.DbTaxGroupDetails.Where(x => x.GruopID == item.CessGrpID && x.CompanyId == compid).Sum(x => x.TPercent) : 0;
                    var taxgroups = context.DbTaxGroup.Where(x => (x.GruopID == item.CessGrpID || x.GruopID == item.GruopID) && x.Deleted == false && x.CompanyId == compid).Select(x => new { x.GruopID, x.TGrpName }).ToList();
                    itemcentral.GeneralPrice = item.GeneralPrice;
                    itemcentral.UnitPrice = item.UnitPrice;
                    itemcentral.Discount = item.Discount;
                    itemcentral.Categoryid = item.Categoryid;
                    itemcentral.SubCategoryId = item.SubCategoryId;
                    itemcentral.SubsubCategoryid = item.SubsubCategoryid;

                    itemcentral.PurchaseUnitName = item.PurchaseUnitName;
                    itemcentral.SellingUnitName = item.SellingUnitName;
                    itemcentral.TGrpName = taxgroups.FirstOrDefault(x => x.GruopID == item.GruopID)?.TGrpName;
                    itemcentral.GruopID = item.GruopID;
                    itemcentral.IsSensitiveMRP = item.IsSensitiveMRP;

                    itemcentral.IsTradeItem = item.IsTradeItem;

                    itemcentral.MinOrderQty = item.MinOrderQty;
                    itemcentral.TotalTaxPercentage = TotalTax;
                    //cess


                    itemcentral.CessGrpID = item.CessGrpID;
                    itemcentral.CessGrpName = item.CessGrpID != null ? taxgroups.FirstOrDefault(x => x.GruopID == item.CessGrpID)?.TGrpName : null;
                    itemcentral.TotalCessPercentage = TotalCessTax;


                    itemcentral.CategoryName = category.CategoryName;
                    itemcentral.SubcategoryName = subcategory.SubcategoryName;
                    itemcentral.SubsubcategoryName = st.SubsubcategoryName;
                    itemcentral.SubSubCode = st.Code;
                    itemcentral.itemname = item.itemname;
                    itemcentral.price = item.price;
                    itemcentral.PurchasePrice = item.PurchasePrice;
                    itemcentral.HindiName = item.HindiName;
                    itemcentral.PurchaseMinOrderQty = item.PurchaseMinOrderQty;
                    itemcentral.IsDailyEssential = item.IsDailyEssential;
                    itemcentral.active = item.active;
                    itemcentral.Deleted = item.Deleted;
                    itemcentral.UpdatedDate = indianTime;

                    #region Batch Code
                    //var param = new SqlParameter("ItemNumber", item.Number);
                    //var param1 = new SqlParameter("Barcode", item.Barcode);
                    //bool IsBarcodeExistsToOther = context.Database.SqlQuery<bool>("exec BatchCode.IsBarcodeExistsToOther @ItemNumber, @Barcode", param, param1).FirstOrDefault();
                    //if (!IsBarcodeExistsToOther)
                    //{
                    //    itemcentral.Barcode = item.Barcode;
                    //    var param2 = new SqlParameter("ItemNumber", item.Number);
                    //    var param21 = new SqlParameter("Barcode", item.Barcode);
                    //    int ResultUpdateBarcode = context.Database.ExecuteSqlCommand("exec BatchCode.UpdateBarcode @ItemNumber, @Barcode ", param2, param21);
                    //}
                    #endregion

                    itemcentral.Margin = item.Margin;
                    itemcentral.promoPerItems = item.promoPerItems;
                    itemcentral.free = item.free;
                    itemcentral.HSNCode = item.HSNCode;
                    itemcentral.ShowMrp = item.ShowMrp;//MRP Price select Checkbox
                    itemcentral.ShowUnit = item.ShowUnit;//Min order qty select Checkbox
                    itemcentral.UOM = item.UOM; //Unit of Masurement like GM Kg 
                    itemcentral.ShowUOM = item.ShowUOM;// Unit Of Masurement select Checkbox
                    itemcentral.ShowType = item.ShowType;// fast slow non movinng
                    itemcentral.itemBaseName = item.itemBaseName;
                    itemcentral.ShowTypes = item.ShowTypes;// fast slow non moving
                    itemcentral.Reason = item.Reason;// MRP Issue Stock Unavailable  Price Issue Other
                    itemcentral.IsSensitive = item.IsSensitive;//filter Issensitive YES/NO
                    itemcentral.UpdatedDate = indianTime;
                    itemcentral.userid = userid;
                    itemcentral.ShelfLife = item.ShelfLife;
                    itemcentral.IsReplaceable = item.IsReplaceable;
                    itemcentral.ModifiedBy = userid;
                    itemcentral.weight = item.weight;
                    itemcentral.weighttype = item.weighttype;
                    itemcentral.CompanyCode = item.CompanyCode;
                    itemcentral.PTR = (itemcentral.PTR > 0 || itemcentral.PTR != null) ? itemcentral.PTR : item.PTR;
                    itemcentral.BaseScheme = item.BaseScheme;
                    itemcentral.IsSellerStoreItem = item.IsSellerStoreItem;//adeded

                    //Add Season Config
                    itemcentral.SeasonId = item.SeasonId;
                    //
                    //if (itemcentral.IsSensitiviteme == true)
                    //{
                    //    if (item.IsSensitiveMRP == false)
                    //    {
                    //        itemcentral.itemname = itemcentral.itemBaseName + " " + item.UnitofQuantity + " " + item.UOM; //item display name   
                    //    }
                    //    else
                    //    {
                    //        itemcentral.itemname = itemcentral.itemBaseName + " " + itemcentral.price + " MRP " + itemcentral.UnitofQuantity + " " + item.UOM; //item display name                               
                    //    }
                    //}
                    //else
                    //{
                    //    itemcentral.itemname = itemcentral.itemBaseName + " " + itemcentral.price + " MRP "; //item display name                               
                    //}


                    if (itemcentral.IsSensitive == true && item.IsSensitiveMRP == true)
                    {
                        itemcentral.itemname = itemcentral.itemBaseName + " " + itemcentral.price + " MRP " + itemcentral.UnitofQuantity + " " + item.UOM;
                    }
                    else if (itemcentral.IsSensitive == true && item.IsSensitiveMRP == false)
                    {
                        itemcentral.itemname = itemcentral.itemBaseName + " " + itemcentral.UnitofQuantity + " " + item.UOM; //item display name 
                    }

                    else if (itemcentral.IsSensitive == false && item.IsSensitiveMRP == false)
                    {
                        itemcentral.itemname = itemcentral.itemBaseName; //item display name
                    }
                    else if (itemcentral.IsSensitive == false && item.IsSensitiveMRP == true)
                    {
                        itemcentral.itemname = itemcentral.itemBaseName + " " + itemcentral.price + " MRP";//item display name 
                    }
                    itemcentral.SellingUnitName = itemcentral.itemname + " " + itemcentral.MinOrderQty + "Unit";//item selling unit name
                    itemcentral.PurchaseUnitName = itemcentral.itemname + " " + itemcentral.PurchaseMinOrderQty + "Unit";//item PurchaseUnitName name


                    //ItemMasterCentralDB.Attach(itemmaster);
                    context.Entry(itemcentral).State = EntityState.Modified;


                    //var itemcentrals = context.ItemMasterCentralDB.Where(x => x.Number == itemcentral.Number && x.Id != itemcentral.Id && x.Deleted == false).ToList();
                    var AllcentralitemList = context.ItemMasterCentralDB.Where(x => x.Number == itemcentral.Number && x.Id != itemcentral.Id).ToList();

                    var itemcentrals = AllcentralitemList.Where(x => x.Number == itemcentral.Number && x.Id != itemcentral.Id && x.Deleted == false).ToList();

                    foreach (var itemcnt in itemcentrals)
                    {


                        itemcnt.CategoryName = category.CategoryName;
                        itemcnt.SubcategoryName = subcategory.SubcategoryName;
                        itemcnt.SubsubcategoryName = st.SubsubcategoryName;
                        itemcnt.SubSubCode = st.Code;
                        itemcnt.Categoryid = itemcentral.Categoryid;
                        itemcnt.SubCategoryId = itemcentral.SubCategoryId;
                        itemcnt.SubsubCategoryid = itemcentral.SubsubCategoryid;
                        itemcnt.HSNCode = itemcentral.HSNCode;
                        itemcnt.CessGrpID = itemcentral.CessGrpID;
                        itemcnt.CessGrpName = itemcentral.CessGrpID != null ? taxgroups.FirstOrDefault(x => x.GruopID == itemcentral.CessGrpID)?.TGrpName : null;
                        itemcnt.TotalCessPercentage = TotalCessTax;
                        itemcnt.TGrpName = taxgroups.FirstOrDefault(x => x.GruopID == itemcentral.GruopID)?.TGrpName;
                        itemcnt.GruopID = itemcentral.GruopID;
                        itemcnt.TotalTaxPercentage = TotalTax;
                        itemcnt.UpdatedDate = indianTime;
                        itemcnt.itemBaseName = itemcentral.itemBaseName;

                        itemcnt.ShowMrp = itemcentral.ShowMrp;//MRP Price select Checkbox
                        itemcnt.ShowUnit = itemcentral.ShowUnit;//Min order qty select Checkbox
                        itemcnt.UOM = itemcentral.UOM; //Unit of Masurement like GM Kg 
                        itemcnt.ShowUOM = itemcentral.ShowUOM;// Unit Of Masurement select Checkbox
                        itemcnt.ShowType = itemcentral.ShowType;// fast slow non movinng
                        itemcnt.ShowTypes = itemcentral.ShowTypes;// fast slow non moving
                        itemcnt.Reason = itemcentral.Reason;// MRP Issue Stock Unavailable  Price Issue Other
                        itemcnt.IsSensitive = itemcentral.IsSensitive;//filter Issensitive YES/NO
                        itemcnt.IsSensitiveMRP = itemcentral.IsSensitiveMRP;
                        itemcnt.userid = userid;
                        itemcnt.ShelfLife = itemcentral.ShelfLife;
                        itemcnt.IsReplaceable = itemcentral.IsReplaceable;
                        itemcnt.HindiName = itemcentral.HindiName;
                        itemcnt.IsSellerStoreItem = itemcentral.IsSellerStoreItem;   //added
                        //if (itemcentral.IsSensitive == true)
                        //{
                        //    if (item.IsSensitiveMRP == false)
                        //    {
                        //        itemcnt.IsSensitive = itemcentral.IsSensitive;
                        //        itemcnt.itemname = itemcentral.itemBaseName + " " + itemcnt.UnitofQuantity + " " + itemcnt.UOM; //item display name                               

                        //    }
                        //    else
                        //    {
                        //        itemcnt.IsSensitive = itemcentral.IsSensitive;
                        //        itemcnt.itemname = itemcentral.itemBaseName + " " + itemcnt.price + " MRP " + itemcnt.UnitofQuantity + " " + itemcnt.UOM; //item display name                               
                        //    }
                        //}
                        //else
                        //{
                        //    itemcnt.IsSensitive = itemcentral.IsSensitive;
                        //    itemcnt.itemname = itemcentral.itemBaseName + " " + itemcnt.price + " MRP "; //item display name                               
                        //}
                        itemcnt.IsSensitive = itemcentral.IsSensitive;
                        if (itemcentral.IsSensitive == true && item.IsSensitiveMRP == true)
                        {
                            itemcnt.itemname = itemcentral.itemBaseName + " " + itemcnt.price + " MRP " + itemcnt.UnitofQuantity + " " + itemcnt.UOM;
                        }
                        else if (itemcentral.IsSensitive == true && item.IsSensitiveMRP == false)
                        {
                            itemcnt.itemname = itemcentral.itemBaseName + " " + itemcnt.UnitofQuantity + " " + itemcnt.UOM; //item display name 
                        }

                        else if (itemcentral.IsSensitive == false && item.IsSensitiveMRP == false)
                        {
                            itemcnt.itemname = itemcentral.itemBaseName; //item display name
                        }
                        else if (itemcentral.IsSensitive == false && item.IsSensitiveMRP == true)
                        {
                            itemcnt.itemname = itemcentral.itemBaseName + " " + itemcnt.price + " MRP";//item display name 
                        }

                        itemcnt.SellingUnitName = itemcnt.itemname + " " + itemcnt.MinOrderQty + "Unit";//item selling unit name
                        itemcnt.PurchaseUnitName = itemcnt.itemname + " " + itemcnt.PurchaseMinOrderQty + "Unit";//item PurchaseUnitName name

                        itemcnt.weight = itemcentral.weight;
                        itemcnt.weighttype = itemcentral.weighttype;
                        itemcnt.CompanyCode = itemcentral.CompanyCode;

                        //Add season config
                        itemcnt.SeasonId = itemcentral.SeasonId;
                        //

                        context.Entry(itemcnt).State = EntityState.Modified;
                    }


                    #region update all warehouse items

                    // List<ItemMaster> itw = context.itemMasters.Where(x => x.SellingSku == itemcentral.SellingSku).ToList();
                    List<ItemMaster> itw = AllWarehouseItemList.Where(x => x.SellingSku == itemcentral.SellingSku).ToList();

                    foreach (ItemMaster jk in itw)
                    {
                        jk.VATTax = itemcentral.VATTax;//tax
                        jk.Categoryid = itemcentral.Categoryid;
                        jk.CategoryName = itemcentral.CategoryName;
                        jk.BaseCategoryid = itemcentral.BaseCategoryid;
                        jk.BaseCategoryName = itemcentral.BaseCategoryName;
                        jk.SubCategoryId = itemcentral.SubCategoryId;
                        jk.SubcategoryName = itemcentral.SubcategoryName;
                        jk.SubsubCategoryid = itemcentral.SubsubCategoryid;
                        jk.SubsubcategoryName = itemcentral.SubsubcategoryName;
                        jk.MinOrderQty = itemcentral.MinOrderQty;
                        jk.GruopID = itemcentral.GruopID;
                        jk.TGrpName = itemcentral.TGrpName;
                        jk.TotalTaxPercentage = itemcentral.TotalTaxPercentage;
                        jk.IsSensitive = itemcentral.IsSensitive;
                        jk.IsSensitiveMRP = itemcentral.IsSensitiveMRP;
                        //cess
                        jk.CessGrpID = itemcentral.CessGrpID;
                        jk.CessGrpName = itemcentral.CessGrpName;
                        jk.TotalCessPercentage = itemcentral.TotalCessPercentage;
                        jk.HSNCode = itemcentral.HSNCode;
                        jk.DefaultBaseMargin = itemcentral.DefaultBaseMargin;
                        jk.ShowTypes = itemcentral.ShowTypes;// fast slow non moving
                        jk.UpdatedDate = indianTime;
                        jk.ShelfLife = itemcentral.ShelfLife;
                        jk.IsReplaceable = itemcentral.IsReplaceable;
                        jk.HindiName = itemcentral.HindiName;
                        jk.IsSellerStoreItem = itemcentral.IsSellerStoreItem;   //added


                        //if (itemcentral.IsSensitive == true)
                        //{
                        //    if (item.IsSensitiveMRP == false)
                        //    {

                        //        jk.itemname = itemcentral.itemBaseName + " " + jk.UnitofQuantity + " " + jk.UOM; //item display name                               

                        //    }
                        //    else
                        //    {

                        //        jk.itemname = itemcentral.itemBaseName + " " + jk.price + " MRP " + jk.UnitofQuantity + " " + jk.UOM; //item display name                               
                        //    }
                        //}
                        //else
                        //{

                        //    jk.itemname = itemcentral.itemBaseName + " " + jk.price + " MRP "; //item display name                               
                        //}
                        if (itemcentral.IsSensitive == true && item.IsSensitiveMRP == true)
                        {
                            jk.itemname = itemcentral.itemBaseName + " " + jk.price + " MRP " + jk.UnitofQuantity + " " + jk.UOM;
                        }
                        else if (itemcentral.IsSensitive == true && item.IsSensitiveMRP == false)
                        {
                            jk.itemname = itemcentral.itemBaseName + " " + jk.UnitofQuantity + " " + jk.UOM; //item display name 
                        }

                        else if (itemcentral.IsSensitive == false && item.IsSensitiveMRP == false)
                        {
                            jk.itemname = itemcentral.itemBaseName; //item display name
                        }
                        else if (itemcentral.IsSensitive == false && item.IsSensitiveMRP == true)
                        {
                            jk.itemname = itemcentral.itemBaseName + " " + jk.price + " MRP";//item display name 
                        }

                        jk.SellingUnitName = jk.itemname + " " + jk.MinOrderQty + "Unit";//item selling unit name
                        jk.PurchaseUnitName = jk.itemname + " " + jk.PurchaseMinOrderQty + "Unit";//item PurchaseUnitName name
                        //itemMasters.Attach(jk);

                        if (jk.ItemMultiMRPId == itemcentral.ItemMultiMRPId && jk.Number == itemcentral.Number)
                        {
                            //if (itemcentral.IsSensitive == true)
                            //{
                            //    if (itemcentral.IsSensitiveMRP == false)
                            //    {
                            //        jk.IsSensitive = itemcentral.IsSensitive;
                            //        jk.itemname = itemcentral.itemBaseName + " " + itemcentral.UnitofQuantity + " " + itemcentral.UOM; //item display name                               

                            //    }
                            //    else
                            //    {
                            //        jk.IsSensitive = itemcentral.IsSensitive;
                            //        jk.itemname = itemcentral.itemBaseName + " " + itemcentral.price + " MRP " + itemcentral.UnitofQuantity + " " + item.UOM; //item display name                               
                            //    }
                            //}
                            //else
                            //{
                            //    jk.IsSensitive = itemcentral.IsSensitive;
                            //    jk.itemname = itemcentral.itemBaseName + " " + itemcentral.price + " MRP "; //item display name                               
                            //}
                            jk.IsSensitive = itemcentral.IsSensitive;
                            if (itemcentral.IsSensitive == true && item.IsSensitiveMRP == true)
                            {
                                jk.itemname = itemcentral.itemBaseName + " " + itemcentral.price + " MRP " + itemcentral.UnitofQuantity + " " + item.UOM;
                            }
                            else if (itemcentral.IsSensitive == true && item.IsSensitiveMRP == false)
                            {
                                jk.itemname = itemcentral.itemBaseName + " " + itemcentral.UnitofQuantity + " " + item.UOM; //item display name 
                            }

                            else if (itemcentral.IsSensitive == false && item.IsSensitiveMRP == false)
                            {
                                jk.itemname = itemcentral.itemBaseName; //item display name
                            }
                            else if (itemcentral.IsSensitive == false && item.IsSensitiveMRP == true)
                            {
                                jk.itemname = itemcentral.itemBaseName + " " + itemcentral.price + " MRP";//item display name 
                            }

                            jk.SellingUnitName = itemcentral.itemname + " " + jk.MinOrderQty + "Unit";//item selling unit name
                            jk.PurchaseUnitName = itemcentral.itemname + " " + jk.PurchaseMinOrderQty + "Unit";//item PurchaseUnitName name
                            jk.UpdatedDate = indianTime;
                            //update stock name

                            CurrentStock cntstock = new CurrentStock();

                            //if (item.IsSensitiveMRP)
                            //{
                            //    cntstock = context.DbCurrentStock.FirstOrDefault(x => x.ItemNumber == item.Number && x.WarehouseId == jk.WarehouseId && x.Deleted == false);
                            //}
                            //else
                            //{
                            cntstock = context.DbCurrentStock.FirstOrDefault(x => x.ItemNumber == item.Number && x.WarehouseId == jk.WarehouseId && x.ItemMultiMRPId == item.ItemMultiMRPId && x.Deleted == false);
                            //}
                            if (cntstock != null)
                            {
                                cntstock.itemname = jk.itemname;
                                cntstock.UpdatedDate = indianTime;

                                context.Entry(cntstock).State = EntityState.Modified;

                                CurrentStockHistory Oss = new CurrentStockHistory();

                                Oss.StockId = cntstock.StockId;
                                Oss.ItemNumber = cntstock.ItemNumber;
                                Oss.itemname = itemcentral.itemname;
                                Oss.TotalInventory = cntstock.CurrentInventory;
                                Oss.WarehouseName = cntstock.WarehouseName;
                                Oss.Warehouseid = cntstock.WarehouseId;
                                Oss.CompanyId = cntstock.CompanyId;
                                Oss.CreationDate = indianTime;
                                Oss.MRP = itemcentral.MRP;
                                Oss.UserName = DisplayName;
                                Oss.userid = userid;
                                Oss.UnitofQuantity = jk.UnitofQuantity;
                                Oss.UOM = jk.UOM;
                                Oss.itemBaseName = jk.itemBaseName;


                                if (DisplayName != null)
                                {
                                    Oss.UserName = DisplayName;
                                    Oss.ManualReason = "Itemname Change :" + DisplayName + " UserId:" + userid;
                                }
                                Oss.userid = userid;
                                context.CurrentStockHistoryDb.Add(Oss);
                            }
                        }
                        context.Entry(jk).State = EntityState.Modified;

                        // List<ItemMaster> itwnumber = context.itemMasters.Where(x => x.Number == jk.Number && x.ItemId != jk.ItemId && x.Deleted == false).ToList();
                        //same number other item in itemmasters
                        List<ItemMaster> itwnumber = AllWarehouseItemList.Where(x => x.Number == jk.Number && x.ItemId != jk.ItemId && x.Deleted == false && x.WarehouseId == jk.WarehouseId).ToList();
                        foreach (var itemnumber in itwnumber)
                        {
                            itemnumber.VATTax = itemcentral.VATTax;//tax
                            itemnumber.Categoryid = itemcentral.Categoryid;
                            itemnumber.CategoryName = itemcentral.CategoryName;
                            itemnumber.BaseCategoryid = itemcentral.BaseCategoryid;
                            itemnumber.BaseCategoryName = itemcentral.BaseCategoryName;
                            itemnumber.SubCategoryId = itemcentral.SubCategoryId;
                            itemnumber.SubcategoryName = itemcentral.SubcategoryName;
                            itemnumber.SubsubCategoryid = itemcentral.SubsubCategoryid;
                            itemnumber.SubsubcategoryName = itemcentral.SubsubcategoryName;
                            itemnumber.GruopID = itemcentral.GruopID;
                            itemnumber.TGrpName = itemcentral.TGrpName;
                            itemnumber.TotalTaxPercentage = itemcentral.TotalTaxPercentage;
                            itemnumber.IsSensitive = itemcentral.IsSensitive;
                            itemnumber.IsSensitiveMRP = itemcentral.IsSensitiveMRP;
                            itemnumber.HindiName = itemcentral.HindiName;
                            itemnumber.IsSellerStoreItem = itemcentral.IsSellerStoreItem;  //added
                            //if (itemcentral.IsSensitive == true)
                            //{
                            //    if (item.IsSensitiveMRP == false)
                            //    {
                            //        itemnumber.itemname = itemcentral.itemBaseName + " " + itemnumber.UnitofQuantity + " " + itemnumber.UOM; //item display name                              

                            //    }
                            //    else
                            //    {
                            //        itemnumber.itemname = itemcentral.itemBaseName + " " + itemnumber.price + " MRP " + itemnumber.UnitofQuantity + " " + item.UOM; //item display name                               
                            //    }
                            //}
                            //else
                            //{
                            //    itemnumber.itemname = itemcentral.itemBaseName + " " + itemnumber.price + " MRP "; //item display name                               
                            //}
                            if (itemcentral.IsSensitive == true && item.IsSensitiveMRP == true)
                            {
                                itemnumber.itemname = itemcentral.itemBaseName + " " + itemnumber.price + " MRP " + itemnumber.UnitofQuantity + " " + item.UOM;
                            }
                            else if (itemcentral.IsSensitive == true && item.IsSensitiveMRP == false)
                            {
                                itemnumber.itemname = itemcentral.itemBaseName + " " + itemnumber.UnitofQuantity + " " + item.UOM; //item display name 
                            }

                            else if (itemcentral.IsSensitive == false && item.IsSensitiveMRP == false)
                            {
                                itemnumber.itemname = itemcentral.itemBaseName; //item display name
                            }
                            else if (itemcentral.IsSensitive == false && item.IsSensitiveMRP == true)
                            {
                                itemnumber.itemname = itemcentral.itemBaseName + " " + itemnumber.price + " MRP";//item display name 
                            }

                            itemnumber.SellingUnitName = itemnumber.itemname + " " + itemnumber.MinOrderQty + "Unit";//item selling unit name
                            itemnumber.PurchaseUnitName = itemnumber.itemname + " " + itemnumber.PurchaseMinOrderQty + "Unit";//item PurchaseUnitName name

                            //cess
                            itemnumber.CessGrpID = itemcentral.CessGrpID;
                            itemnumber.CessGrpName = itemcentral.CessGrpName;
                            itemnumber.TotalCessPercentage = itemcentral.TotalCessPercentage;
                            itemnumber.HSNCode = itemcentral.HSNCode;
                            itemnumber.DefaultBaseMargin = itemcentral.DefaultBaseMargin;
                            itemnumber.ShowTypes = itemcentral.ShowTypes;// fast slow non moving
                            itemnumber.UpdatedDate = indianTime;
                            itemnumber.ShelfLife = itemcentral.ShelfLife;
                            itemnumber.IsReplaceable = itemcentral.IsReplaceable;
                            context.Entry(itemnumber).State = EntityState.Modified;
                        }
                    }

                    #endregion

                    //#region Add Clearance/NonSellable Shelf Life of ItemNumber
                    //var IsExist = context.ClearanceNonsShelfConfigurations.FirstOrDefault(x=>x.ItemNumber == itemcentral.Number && x.IsActive ==true);
                    //if(IsExist == null && item.ClearanceFrom > 0)
                    //{
                    //    ClearanceNonSaleableShelfConfiguration obj = new ClearanceNonSaleableShelfConfiguration();
                    //    obj.CategoryId = itemcentral.Categoryid;
                    //    obj.SubCategoryId = itemcentral.SubCategoryId;
                    //    obj.BrandId = itemcentral.SubsubCategoryid;
                    //    obj.ItemNumber = itemcentral.Number;
                    //    obj.Status = "Approved";
                    //    obj.ClearanceShelfLifeFrom = item.ClearanceFrom;
                    //    obj.ClearanceShelfLifeTo = item.ClearanceTo;
                    //    obj.NonSellShelfLifeFrom = item.NonsellableFrom;
                    //    obj.NonSellShelfLifeTo = item.NonsellableTo;
                    //    obj.ApprovedBy = 0;
                    //    obj.RejectedBy = 0;
                    //    obj.RejectComment = null;
                    //    obj.CreatedDate = DateTime.Now;
                    //    obj.ModifiedDate = null;
                    //    obj.CreatedBy = 0;
                    //    obj.ModifiedBy = 0;
                    //    obj.IsActive = true;
                    //    obj.IsDeleted = false;
                    //    context.ClearanceNonsShelfConfigurations.Add(obj);
                    //}

                    //#endregion


                    int count = context.Commit();

                    if (item.IsTradeItem == true)
                    {
                        try
                        {


                            List<KeyValuePair<string, IEnumerable<string>>> header = new List<KeyValuePair<string, IEnumerable<string>>>();
                            List<string> val = new List<string>();
                            val.Add("true");
                            KeyValuePair<string, IEnumerable<string>> pair = new KeyValuePair<string, IEnumerable<string>>("NoEncryption", val);
                            header.Add(pair);

                            using (GenericRestHttpClient<dynamic, string> memberClient
                                             = new GenericRestHttpClient<dynamic, string>(AppConstants.tradeAPIurl,
                                             "api/TradeItem/UpdateFromSK?itemId=" + itemcentral.Id))
                            {
                                var result = memberClient.GetAsync();


                            }


                            //var tradeUrl = System.Configuration.ConfigurationManager.AppSettings["TradeURL"] + "api/TradeItem/UpdateFromSK?itemId=" + itemcentral.Id;

                            //using (GenericRestHttpClient<TradeItemMasterDc, string> memberClient = new GenericRestHttpClient<TradeItemMasterDc, string>(tradeUrl, "", null))
                            //{
                            //    var isDone = memberClient.GetAsync();
                            //}
                        }
                        catch (Exception ex)
                        {
                            TextFileLogHelper.LogError("Error while saving item in Trade: " + ex.ToString());
                        }
                    }




                    if (count > 0)
                        return itemcentral;
                    else
                        return null;

                }
                return null;
            }
        }
        #endregion


        [ResponseType(typeof(ItemMaster))]
        [Route("")]
        [AcceptVerbs("Delete")]
        public void Remove(int id)
        {
            using (var context = new AuthContext())
            {
                logger.Info("start delete Item Master: ");
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
                    int CompanyId = compid;
                    logger.Info("User ID : {0} , Company Id : {1}", compid, userid);
                    context.DeleteItemMaster(id, CompanyId);
                    logger.Info("End  delete Item Master: ");
                }
                catch (Exception ex)
                {
                    logger.Error("Error in delete Item Master " + ex.Message);
                }
            }
        }

        [Route("exportFullWarehouse")]
        [HttpGet]
        public dynamic exportFullWarehouse(int warehouseid)
        {

            try
            {
                var identity = User.Identity as ClaimsIdentity;
                int compid = 0, userid = 0;
                int Warehouse_id = 0;
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
                using (AuthContext db = new AuthContext())
                {

                    var list = (from i in db.itemMasters
                                where i.Deleted == false && i.CompanyId == CompanyId
                                join j in db.Categorys on i.Categoryid equals j.Categoryid
                                join k in db.SubsubCategorys on i.SubsubCategoryid equals k.SubsubCategoryid
                                select new
                                {
                                    CityName = i.CityName,
                                    Cityid = i.Cityid,
                                    CategoryName = i.CategoryName,
                                    CategoryCode = j.Code,
                                    SubcategoryName = i.SubcategoryName,
                                    SubsubcategoryName = i.SubsubcategoryName,
                                    BrandCode = k.Code,
                                    itemname = i.itemname,
                                    itemcode = i.itemcode,
                                    Number = i.Number,
                                    SellingSku = i.SellingSku,
                                    price = i.price,
                                    PurchasePrice = i.PurchasePrice,
                                    UnitPrice = i.UnitPrice,
                                    NetPurchasePrice = i.NetPurchasePrice,
                                    MinOrderQty = i.MinOrderQty,
                                    SellingUnitName = i.SellingUnitName,
                                    PurchaseMinOrderQty = i.PurchaseMinOrderQty,
                                    StoringItemName = i.StoringItemName,
                                    PurchaseSku = i.PurchaseSku,
                                    PurchaseUnitName = i.PurchaseUnitName,
                                    SupplierName = i.SupplierName,
                                    SUPPLIERCODES = i.SUPPLIERCODES,
                                    BaseCategoryName = i.BaseCategoryName,
                                    TGrpName = i.TGrpName,
                                    TotalTaxPercentage = i.TotalTaxPercentage,
                                    WarehouseName = i.WarehouseName,
                                    HindiName = i.HindiName,
                                    SizePerUnit = i.SizePerUnit,
                                    Active = i.active,
                                    Deleted = i.Deleted,
                                    Margin = i.Margin,
                                    PromoPoint = i.promoPoint,
                                    HSNCode = i.HSNCode,
                                    IsSensitive = i.IsSensitive,
                                    ItemMultiMRPId = i.ItemMultiMRPId,
                                    ShowTypes = i.ShowTypes
                                }).ToList();

                    return list;
                }

            }

            catch (Exception ex)
            {
                logger.Error("Gote exception:: " + ex.Message);
                return null;
            }

        }

        [Route("export")]
        [HttpGet]
        public async System.Threading.Tasks.Task<List<ItemMasterFullWhexportDTO>> export(int warehouseid)
        {

            try
            {
                var identity = User.Identity as ClaimsIdentity;
                int compid = 0, userid = 0;
                int Warehouse_id = 0;
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
                using (AuthContext db = new AuthContext())
                {

                    List<ItemMasterFullWhexportDTO> newdata = new List<ItemMasterFullWhexportDTO>();

                    newdata = (from a in db.itemMasters
                               where a.Deleted == false && a.WarehouseId == warehouseid && a.CompanyId == CompanyId
                               join j in db.Categorys on a.Categoryid equals j.Categoryid
                               join k in db.SubsubCategorys on a.SubsubCategoryid equals k.SubsubCategoryid
                               select new ItemMasterFullWhexportDTO
                               {

                                   CityName = a.CityName,
                                   Cityid = a.Cityid,
                                   CategoryName = a.CategoryName,
                                   CategoryCode = j.Code,
                                   SubcategoryName = a.SubcategoryName,
                                   SubsubcategoryName = a.SubsubcategoryName,
                                   BrandCode = k.Code,
                                   itemname = a.itemname,
                                   itemcode = a.itemcode,
                                   Number = a.Number,
                                   SellingSku = a.SellingSku,
                                   price = a.price,
                                   PurchasePrice = a.PurchasePrice,
                                   UnitPrice = a.UnitPrice,
                                   NetPurchasePrice = a.NetPurchasePrice,
                                   MinOrderQty = a.MinOrderQty,
                                   SellingUnitName = a.SellingUnitName,
                                   PurchaseMinOrderQty = a.PurchaseMinOrderQty,
                                   StoringItemName = a.StoringItemName,
                                   PurchaseSku = a.PurchaseSku,
                                   PurchaseUnitName = a.PurchaseUnitName,
                                   SupplierName = a.SupplierName,
                                   SUPPLIERCODES = a.SUPPLIERCODES,
                                   BaseCategoryName = a.BaseCategoryName,
                                   TGrpName = a.TGrpName,
                                   TotalTaxPercentage = a.TotalTaxPercentage,
                                   WarehouseName = a.WarehouseName,
                                   HindiName = a.HindiName,
                                   SizePerUnit = a.SizePerUnit,
                                   Active = a.active,
                                   Deleted = a.Deleted,
                                   Margin = a.Margin,
                                   PromoPoint = a.promoPoint,
                                   HSNCode = a.HSNCode,
                                   IsSensitive = a.IsSensitive,
                                   ItemMultiMRPId = a.ItemMultiMRPId,
                                   ShowTypes = a.ShowTypes,
                                   WarehouseId = a.WarehouseId

                               }).ToList();


                    List<ItemClassificationDC> ABCitemsList = newdata.Select(item => new ItemClassificationDC { ItemNumber = item.Number, WarehouseId = item.WarehouseId }).ToList();

                    var manager = new ItemLedgerManager();
                    var GetItem = await manager.GetItemClassificationsAsync(ABCitemsList);
                    foreach (var item in newdata)
                    {

                        if (GetItem != null && GetItem.Any())
                        {
                            if (GetItem.Any(x => x.ItemNumber == item.Number))
                            {
                                item.ABCClassification = GetItem.FirstOrDefault(x => x.ItemNumber == item.Number).Category;
                            }
                            else { item.ABCClassification = "D"; }
                        }
                        else { item.ABCClassification = "D"; }

                    }

                    return newdata;

                }

            }

            catch (Exception ex)
            {
                logger.Error("Gote exception:: " + ex.Message);
                return null;
            }

        }



        [Route("exportCentral")]
        [HttpGet]
        public dynamic exportCentralItem()
        {

            try
            {
                var identity = User.Identity as ClaimsIdentity;
                int compid = 0, userid = 0;
                int Warehouse_id = 0;
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
                using (AuthContext db = new AuthContext())
                {
                    var list = (from i in db.ItemMasterCentralDB
                                where i.Deleted == false && i.CompanyId == CompanyId
                                join j in db.Categorys on i.Categoryid equals j.Categoryid
                                join k in db.SubsubCategorys on i.SubsubCategoryid equals k.SubsubCategoryid
                                select new
                                {
                                    CategoryName = i.CategoryName,
                                    CategoryCode = j.Code,
                                    SubcategoryName = i.SubcategoryName,
                                    SubsubcategoryName = i.SubsubcategoryName,
                                    BrandCode = k.Code,
                                    itemname = i.itemname,
                                    itemcode = i.itemcode,
                                    Number = i.Number,
                                    SellingSku = i.SellingSku,
                                    price = i.price,
                                    PurchasePrice = i.PurchasePrice,
                                    UnitPrice = i.UnitPrice,
                                    MinOrderQty = i.MinOrderQty,
                                    SellingUnitName = i.SellingUnitName,
                                    PurchaseMinOrderQty = i.PurchaseMinOrderQty,
                                    StoringItemName = i.StoringItemName,
                                    PurchaseSku = i.PurchaseSku,
                                    PurchaseUnitName = i.PurchaseUnitName,
                                    BaseCategoryName = i.BaseCategoryName,
                                    TGrpName = i.TGrpName,
                                    TotalTaxPercentage = i.TotalTaxPercentage,
                                    HindiName = i.HindiName,
                                    SizePerUnit = i.SizePerUnit,
                                    Barcode = "",//i.Barcode,
                                    Active = i.active,
                                    Deleted = i.Deleted,
                                    Margin = i.Margin,
                                    PromoPoint = i.promoPoint,
                                    HSNCode = i.HSNCode,
                                    IsSensitive = i.IsSensitive,
                                    ShelfLife = i.ShelfLife,
                                    ShowTypes = i.ShowTypes,
                                    PTR = i.PTR,
                                    BaseScheme = i.BaseScheme,
                                    Id = i.Id

                                }).OrderByDescending(x => x.Id).ToList();

                    return list;
                }
            }
            catch (Exception ex)
            {
                logger.Error("Gote exception:: " + ex.Message);
                return null;
            }
        }



        /// <summary>
        /// use by
        /// 
        /// 
        /// 
        /// 
        /// 
        /// 
        /// 
        /// Item Histiory 
        /// </summary>
        /// <param name="Number"></param>
        /// <returns></returns>

        [Route("oldprice")]
        [HttpGet]
        public dynamic oldprice(string Number)
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
                using (AuthContext odd = new AuthContext())
                {
                    if (Warehouse_id > 0)
                    {
                        var data = odd.ItemMasterHistoryDb.Where(x => x.Deleted == false && x.Number == Number && x.CompanyId == CompanyId && x.WarehouseId == Warehouse_id).ToList().OrderByDescending(x => x.UpdatedDate).Take(20);
                        return data;
                    }
                    else
                    {
                        var data = odd.ItemMasterHistoryDb.Where(x => x.Deleted == false && x.Number == Number && x.CompanyId == CompanyId).ToList().OrderByDescending(x => x.UpdatedDate).Take(20); ;
                        return data;
                    }
                }

            }
            catch (Exception ex)
            {
                return false;
            }
        }

        [Route("Woldprice")]
        [HttpGet]
        public dynamic Woldprice(int ItemId)
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
                using (AuthContext odd = new AuthContext())
                {
                    if (Warehouse_id > 0)
                    {
                        var data = odd.ItemMasterHistoryDb.Where(x => x.Deleted == false && x.ItemId == ItemId && x.CompanyId == CompanyId && x.WarehouseId == Warehouse_id).ToList().OrderByDescending(x => x.UpdatedDate).Take(20);
                        return data;
                    }
                    else
                    {
                        var data = odd.ItemMasterHistoryDb.Where(x => x.Deleted == false && x.ItemId == ItemId && x.CompanyId == CompanyId).ToList().OrderByDescending(x => x.UpdatedDate).Take(20);
                        return data;
                    }
                }
            }
            catch (Exception ex)
            {
                return false;
            }
        }



        // removed by Harry : 21 May 2019 
        [Route("")]  // SA app
        [HttpGet]
        public List<ItemMaster> appitemnameforsalesman(string itemname, int PeopleID)
        {
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
                    int CompanyId = compid;
                    List<ItemMaster> itemList = new List<ItemMaster>();
                    var mk = context.SearchitemSaleman(itemname, PeopleID).ToList();
                    //  return mk;
                    return null;
                }
                catch (Exception ex)
                {
                    logger.Error("Error in ItemMaster " + ex.Message);
                    logger.Info("End ItemMaster: ");
                    return null;
                }
            }
        }



        #region SA V2: Search item by people id and item name
        /// <summary>
        /// Updated by 29/01/2019
        /// </summary>
        /// <param name="itemname"></param>
        /// <param name="PeopleID"></param>
        /// <returns></returns>
        [Route("V2")]
        [HttpGet]

        public HttpResponseMessage appitemnameforsalesmanv2(string lang, string itemname, int PeopleID)
        {
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
                    int CompanyId = compid;
                    List<ItemMaster> itemList = new List<ItemMaster>();
                    WRSITEM item = new WRSITEM();
                    //  var custBrand = db.CustWarehouseDB.Where(x => x.ExecutiveId == PeopleID && x.Deleted == false).FirstOrDefault();
                    var custBrand = db.Peoples.Where(x => x.PeopleID == PeopleID && x.WarehouseId != 0 && x.Deleted == false && x.Active == true).FirstOrDefault();
                    var FlashDealWithItemIds = db.FlashDealItemConsumedDB.Where(x => x.CustomerId == userid).Select(x => new { x.FlashDealId, x.ItemId });
                    var newdata = (from a in db.itemMasters
                                   where (a.itemname.Contains(itemname) && a.WarehouseId == custBrand.WarehouseId && a.Deleted == false && a.active == true)
                                   join b in db.ItemMasterCentralDB on a.SellingSku equals b.SellingSku
                                   select new factoryItemdata
                                   {
                                       WarehouseId = a.WarehouseId,
                                       CompanyId = a.CompanyId,
                                       Categoryid = b.Categoryid,
                                       Discount = b.Discount,
                                       ItemId = a.ItemId,
                                       ItemNumber = b.Number,
                                       itemname = a.itemname,
                                       LogoUrl = b.LogoUrl,
                                       MinOrderQty = b.MinOrderQty,
                                       IsSensitive = a.IsSensitive,
                                       IsSensitiveMRP = a.IsSensitiveMRP,
                                       UnitofQuantity = a.UnitofQuantity,
                                       UOM = a.UOM,
                                       price = a.price,
                                       SubCategoryId = b.SubCategoryId,
                                       SubsubCategoryid = b.SubsubCategoryid,
                                       TotalTaxPercentage = b.TotalTaxPercentage,
                                       SellingUnitName = a.SellingUnitName,
                                       SellingSku = b.SellingSku,
                                       UnitPrice = a.UnitPrice,
                                       HindiName = a.HindiName,
                                       VATTax = b.VATTax,
                                       active = a.active,
                                       marginPoint = a.marginPoint,
                                       promoPerItems = a.promoPerItems,
                                       NetPurchasePrice = a.NetPurchasePrice,
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
                                       FlashDealMaxQtyPersonCanTake = a.OfferMaxQtyPersonCanTake,
                                       FreeItemId = a.OfferFreeItemId,
                                   }).OrderByDescending(x => x.ItemNumber).ToList();
                    //var mk = context.itembystringWid(itemname, CompanyId, CustomerId).ToList();
                    foreach (var it in newdata)
                    {
                        if (!(it.OfferStartTime <= DateTime.Now && it.OfferEndTime >= DateTime.Now))
                        {
                            if (it.OfferCategory == 2)
                            {
                                it.IsOffer = false;
                                it.FlashDealSpecialPrice = 0;
                                it.OfferCategory = 0;
                            }
                            else if (it.OfferCategory == 1)
                            {
                                it.IsOffer = false;
                                it.OfferCategory = 0;
                            }

                        }
                        else if ((it.OfferStartTime <= DateTime.Now && it.OfferEndTime >= DateTime.Now) && it.OfferCategory == 2)
                        {
                            if (FlashDealWithItemIds != null && FlashDealWithItemIds.Any(x => x.ItemId == it.ItemId))
                            {
                                it.IsOffer = false;
                                it.FlashDealSpecialPrice = 0;
                                it.OfferCategory = 0;
                            }
                        }


                        if (item.ItemMasters == null)
                        {
                            item.ItemMasters = new List<factoryItemdata>();
                        }
                        try
                        {/// Dream Point Logic && Margin Point
                            int? MP, PP;
                            double xPoint = xPointValue * 10;
                            //Customer (0.02 * 10=0.2)
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
                            //// by sudhir 22-08-2019
                            if (lang.Trim() == "hi")
                            {
                                if (it.HindiName != null)
                                {
                                    if (it.IsSensitive == true)
                                    {
                                        if (it.IsSensitiveMRP == false)
                                        {
                                            it.itemname = it.HindiName + " " + it.UnitofQuantity + " " + it.UOM; //item display name   
                                        }
                                        else
                                        {
                                            it.itemname = it.HindiName + " " + it.price + " MRP " + it.UnitofQuantity + " " + it.UOM; //item display name                               
                                        }
                                    }
                                    else
                                    {
                                        it.itemname = it.HindiName + " " + it.price + " MRP "; //item display name                               
                                    }
                                }
                                else
                                {
                                    it.itemname = it.HindiName + " " + it.price + " MRP ";
                                }
                            }
                            //end
                        }
                        catch { }
                        item.ItemMasters.Add(it);
                    }
                    if (item.ItemMasters != null)
                    {
                        item.Message = true;
                        return Request.CreateResponse(HttpStatusCode.OK, item);
                    }
                    else
                    {
                        item.Message = false;
                        return Request.CreateResponse(HttpStatusCode.OK, item);
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


        ///// <summary>
        ///// Updated by 05/09/2019
        ///// </summary>
        ///// <param name="itemname"></param>
        ///// <param name="PeopleID"></param>
        ///// <returns></returns>
        //[Route("V3")]
        //[HttpGet]

        //public HttpResponseMessage appitemnameforsalesmanv3(string lang, string itemname, int peopleID, int warehouseId)
        //{
        //    using (var db = new AuthContext())
        //    {
        //        try
        //        {
        //            //var excludecategory = db.Categorys.Where(x => x.CategoryName == "Face wash & Cream" || x.CategoryName == "Body cream & lotion" || x.CategoryName == "Baby & Fem Care").Select(x => x.Categoryid).ToList();


        //            //WRSITEM item = new WRSITEM();
        //            //var newdata = (from a in db.itemMasters
        //            //               where (a.itemname.Contains(itemname) && a.WarehouseId == warehouseId && a.Deleted == false && a.active == true && !excludecategory.Contains(a.Categoryid) && (a.ItemAppType == 0 || a.ItemAppType == 1))
        //            //               join b in db.ItemMasterCentralDB on a.SellingSku equals b.SellingSku
        //            //               let limit = db.ItemLimitMasterDB.Where(p2 => a.ItemMultiMRPId == p2.ItemMultiMRPId && a.Number == p2.ItemNumber && a.WarehouseId == p2.WarehouseId).FirstOrDefault()
        //            //               select new factoryItemdata

        //            WRSITEM item = new WRSITEM();
        //            var newdata = (from a in db.itemMasters
        //                           where (a.itemname.Contains(itemname) && a.WarehouseId == warehouseId && a.Deleted == false && a.active == true && (a.ItemAppType == 0 || a.ItemAppType == 1))
        //                           join b in db.ItemMasterCentralDB on a.SellingSku equals b.SellingSku
        //                           let limit = db.ItemLimitMasterDB.Where(p2 => a.ItemMultiMRPId == p2.ItemMultiMRPId && a.Number == p2.ItemNumber && a.WarehouseId == p2.WarehouseId).FirstOrDefault()
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
        //                               itemname = a.itemname,
        //                               LogoUrl = b.LogoUrl,
        //                               MinOrderQty = b.MinOrderQty,
        //                               IsSensitive = a.IsSensitive,
        //                               IsSensitiveMRP = a.IsSensitiveMRP,
        //                               UnitofQuantity = a.UnitofQuantity,
        //                               UOM = a.UOM,
        //                               price = a.price,
        //                               SubCategoryId = b.SubCategoryId,
        //                               SubsubCategoryid = b.SubsubCategoryid,
        //                               TotalTaxPercentage = b.TotalTaxPercentage,
        //                               SellingUnitName = a.SellingUnitName,
        //                               SellingSku = b.SellingSku,
        //                               UnitPrice = a.UnitPrice,
        //                               HindiName = a.HindiName,
        //                               VATTax = b.VATTax,
        //                               active = a.active,
        //                               marginPoint = a.marginPoint,
        //                               promoPerItems = a.promoPerItems,
        //                               NetPurchasePrice = a.NetPurchasePrice,
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
        //                               FlashDealMaxQtyPersonCanTake = a.OfferMaxQtyPersonCanTake,
        //                               FreeItemId = a.OfferFreeItemId,
        //                               ItemMultiMRPId = a.ItemMultiMRPId,
        //                               BillLimitQty = a.BillLimitQty,
        //                               itemBaseName = a.itemBaseName
        //                           }).OrderByDescending(x => x.ItemNumber).ToList();
        //            //var mk = context.itembystringWid(itemname, CompanyId, CustomerId).ToList();

        //            var offerids = newdata.Where(x => x.OfferId > 0).Select(x => x.OfferId).Distinct().ToList();
        //            var activeOfferids = offerids != null && offerids.Any() ? db.OfferDb.Where(x => offerids.Contains(x.OfferId) && x.IsActive && !x.IsDeleted && x.OfferAppType == "Sales App" || x.OfferAppType == "Both").Select(x => x.OfferId).ToList() : new List<int>();
        //            foreach (var it in newdata)
        //            {
        //                if (it.OfferCategory == 2)
        //                {
        //                    it.IsOffer = false;
        //                    it.FlashDealSpecialPrice = 0;
        //                    it.OfferCategory = 0;
        //                }

        //                if (!(it.OfferStartTime <= DateTime.Now && it.OfferEndTime >= DateTime.Now))
        //                {
        //                    if (it.OfferCategory == 1)
        //                    {
        //                        it.IsOffer = false;
        //                        it.OfferCategory = 0;
        //                    }
        //                }


        //                if (item.ItemMasters == null)
        //                {
        //                    item.ItemMasters = new List<factoryItemdata>();
        //                }
        //                try
        //                {/// Dream Point Logic && Margin Point
        //                    int? MP, PP;
        //                    double xPoint = xPointValue * 10;
        //                    //Customer (0.02 * 10=0.2)
        //                    if (it.promoPerItems.Equals(null) && it.promoPerItems == null)
        //                    {
        //                        PP = 0;
        //                    }
        //                    else
        //                    {
        //                        PP = it.promoPerItems;
        //                    }
        //                    if (it.marginPoint.Equals(null) && it.promoPerItems == null)
        //                    {
        //                        MP = 0;
        //                    }
        //                    else
        //                    {
        //                        double WithTaxNetPurchasePrice = Math.Round(it.NetPurchasePrice * (1 + (it.TotalTaxPercentage / 100)), 3);//With tax
        //                        MP = Convert.ToInt32((it.UnitPrice - WithTaxNetPurchasePrice) * xPoint); // (UnitPrice-NPP withtax) * By xpoint 
        //                    }
        //                    if (PP > 0 && MP > 0)
        //                    {
        //                        int? PP_MP = PP + MP;
        //                        it.dreamPoint = PP_MP;
        //                    }
        //                    else if (MP > 0)
        //                    {
        //                        it.dreamPoint = MP;
        //                    }
        //                    else if (PP > 0)
        //                    {
        //                        it.dreamPoint = PP;
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
        //                    //// by sudhir 22-08-2019
        //                    if (lang.Trim() == "hi")
        //                    {
        //                        if (it.HindiName != null)
        //                        {
        //                            if (it.IsSensitive == true)
        //                            {
        //                                if (it.IsSensitiveMRP == false)
        //                                {
        //                                    it.itemname = it.HindiName + " " + it.UnitofQuantity + " " + it.UOM; //item display name   
        //                                }
        //                                else
        //                                {
        //                                    it.itemname = it.HindiName + " " + it.price + " MRP " + it.UnitofQuantity + " " + it.UOM; //item display name                               
        //                                }
        //                            }
        //                            else
        //                            {
        //                                it.itemname = it.HindiName + " " + it.price + " MRP "; //item display name                               
        //                            }
        //                        }
        //                        else
        //                        {
        //                            it.itemname = it.itemBaseName + " " + it.price + " MRP ";
        //                        }
        //                    }
        //                    //end
        //                }
        //                catch { }

        //                if (it.OfferType != "FlashDeal")
        //                {
        //                    if (activeOfferids.Any() && activeOfferids.Any(x => x == it.OfferId) && it.IsOffer)
        //                        it.IsOffer = true;
        //                    else
        //                        it.IsOffer = false;
        //                }

        //                //if (it.HindiName != null)
        //                //{
        //                //    if (it.IsSensitive == true && it.IsSensitiveMRP == true)
        //                //    {
        //                //        it.itemname = it.HindiName + " " + it.price + " MRP " + it.UnitofQuantity + " " + it.UOM;
        //                //    }
        //                //    else if (it.IsSensitive == true && it.IsSensitiveMRP == false)
        //                //    {
        //                //        it.itemname = it.HindiName + " " + it.UnitofQuantity + " " + it.UOM; //item display name 
        //                //    }

        //                //    else if (it.IsSensitive == false && it.IsSensitiveMRP == false)
        //                //    {
        //                //        it.itemname = it.HindiName; //item display name
        //                //    }
        //                //    else if (it.IsSensitive == false && it.IsSensitiveMRP == true)
        //                //    {
        //                //        it.itemname = it.HindiName + " " + it.price + " MRP";//item display name 
        //                //    }
        //                //}

        //                item.ItemMasters.Add(it);
        //            }
        //            if (item.ItemMasters != null)
        //            {
        //                item.Message = true;
        //                return Request.CreateResponse(HttpStatusCode.OK, item);
        //            }
        //            else
        //            {
        //                item.Message = false;
        //                return Request.CreateResponse(HttpStatusCode.OK, item);
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            logger.Error("Error in ItemMaster " + ex.Message);
        //            logger.Info("End  ItemMaster: ");
        //            return null;
        //        }
        //    }
        //}

        #endregion

        #region RA:Search Item by any name for retailer app

        /// <summary>
        /// created by 25/12/2018
        /// Search Item by any name 
        /// </summary>
        /// <param name="itemname"></param>
        /// <param name="CustomerId"></param>
        /// <returns></returns>
        [Route("")]
        [HttpGet]
        public List<factoryItemdata> getitembyitemname(string itemname, int CustomerId)
        {
            using (var db = new AuthContext())
            {
                try
                {
                    var identity = User.Identity as ClaimsIdentity;
                    int compid = 1, userid = 0;

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
                    WRSITEM item = new WRSITEM();
                    List<ItemMaster> itemList = new List<ItemMaster>();
                    var custBrand = db.Customers.Where(x => x.CustomerId == CustomerId && x.Deleted == false).FirstOrDefault();
                    var newdata = (from a in db.itemMasters
                                   where (a.itemname.Contains(itemname) && a.WarehouseId == custBrand.Warehouseid && a.Deleted == false && a.active == true)
                                   join b in db.ItemMasterCentralDB on a.SellingSku equals b.SellingSku
                                   select new factoryItemdata
                                   {
                                       WarehouseId = a.WarehouseId,
                                       CompanyId = a.CompanyId,
                                       Categoryid = b.Categoryid,
                                       Discount = b.Discount,
                                       ItemId = a.ItemId,
                                       ItemNumber = b.Number,
                                       itemname = a.itemname,
                                       LogoUrl = b.LogoUrl,
                                       MinOrderQty = b.MinOrderQty,
                                       price = a.price,
                                       SubCategoryId = b.SubCategoryId,
                                       SubsubCategoryid = b.SubsubCategoryid,
                                       TotalTaxPercentage = b.TotalTaxPercentage,
                                       SellingUnitName = a.SellingUnitName,
                                       SellingSku = b.SellingSku,
                                       UnitPrice = a.UnitPrice,
                                       HindiName = a.HindiName,
                                       VATTax = b.VATTax,
                                       active = a.active,
                                       marginPoint = a.marginPoint,
                                       promoPerItems = a.promoPerItems,
                                       NetPurchasePrice = a.NetPurchasePrice,
                                       IsOffer = b.IsOffer,
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
                                       FlashDealMaxQtyPersonCanTake = a.OfferMaxQtyPersonCanTake
                                   }).OrderByDescending(x => x.ItemNumber).ToList();
                    //var mk = context.itembystringWid(itemname, CompanyId, CustomerId).ToList();
                    foreach (var it in newdata)
                    {
                        if (item.ItemMasters == null)
                        {
                            item.ItemMasters = new List<factoryItemdata>();
                        }
                        try
                        {/// Dream Point Logic && Margin Point
                            int? MP, PP;
                            double xPoint = xPointValue * 10;
                            //Customer (0.2 * 10=1)
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
                    return item.ItemMasters;
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



        [Route("Getitemmasterscentral")]
        [HttpGet]

        public List<factoryItemCentraldata> Getitemmasterscentral()
        {
            using (var db = new AuthContext())
            {
                string sqlquery = "Exec GetAllItem";
                var newdata = db.Database.SqlQuery<factoryItemCentraldata>(sqlquery).ToList();
                //var newdata = (from a in db.itemMasters
                //               where (a.Deleted == false && a.active == true)
                //              // join b in db.ItemMasterCentralDB on a.SellingSku equals b.SellingSku
                //               select new factoryItemCentraldata
                //               {
                //                   ItemId = a.ItemId,
                //                   Categoryid = a.Categoryid,
                //                   CategoryName = a.CategoryName,
                //                   BaseCategoryid = a.BaseCategoryid,                               
                //                   SubsubCategoryid = a.SubsubCategoryid,
                //                   SubsubcategoryName = a.SubsubcategoryName,
                //                   PurchaseMinOrderQty = a.PurchaseMinOrderQty,
                //                   itemname = a.itemname,
                //                   price = a.price,
                //                   UOM = a.UOM,                                 
                //                   active = a.active,
                //                   Deleted = a.Deleted,
                //                   WarehouseId = a.WarehouseId
                //               }).ToList();
                return newdata;
            }
        }



        #region RA V2: Search Item by any name for retailer app
        /// <summary>
        /// # Version @
        /// created by 31/01/2019
        /// Search Item by any name 
        /// </summary>
        /// <param name="itemname"></param>
        /// <param name="CustomerId"></param>
        /// <returns></returns>
        [Route("V2")]
        [HttpGet]
        public HttpResponseMessage getitembyitemnamev2(string lang, string itemname, int CustomerId)
        {
            using (var db = new AuthContext())
            {
                try
                {
                    var identity = User.Identity as ClaimsIdentity;
                    int compid = 1, userid = 0;

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
                    WRSITEM item = new WRSITEM();
                    List<ItemMaster> itemList = new List<ItemMaster>();
                    var custBrand = db.Customers.Where(x => x.CustomerId == CustomerId && x.Deleted == false).FirstOrDefault();
                    var FlashDealWithItemIds = db.FlashDealItemConsumedDB.Where(x => x.CustomerId == CustomerId).Select(x => new { x.FlashDealId, x.ItemId });

                    var newdata = (from a in db.itemMasters
                                   where (a.itemname.Trim().ToLower().Contains(itemname.Trim().ToLower()) && a.WarehouseId == custBrand.Warehouseid && a.Deleted == false && a.active == true)
                                   join b in db.ItemMasterCentralDB on a.SellingSku equals b.SellingSku
                                   select new factoryItemdata
                                   {
                                       WarehouseId = a.WarehouseId,
                                       CompanyId = a.CompanyId,
                                       Categoryid = b.Categoryid,
                                       Discount = b.Discount,
                                       ItemId = a.ItemId,
                                       ItemNumber = b.Number,
                                       HindiName = a.HindiName,
                                       IsSensitive = a.IsSensitive,
                                       IsSensitiveMRP = a.IsSensitiveMRP,
                                       UnitofQuantity = a.UnitofQuantity,
                                       UOM = a.UOM,
                                       itemname = a.itemname,
                                       itemBaseName = a.itemBaseName,
                                       LogoUrl = b.LogoUrl,
                                       MinOrderQty = b.MinOrderQty,
                                       price = a.price,
                                       SubCategoryId = b.SubCategoryId,
                                       SubsubCategoryid = b.SubsubCategoryid,
                                       TotalTaxPercentage = b.TotalTaxPercentage,
                                       SellingUnitName = a.SellingUnitName,
                                       SellingSku = b.SellingSku,
                                       UnitPrice = a.UnitPrice,
                                       VATTax = b.VATTax,
                                       active = a.active,
                                       marginPoint = a.marginPoint,
                                       promoPerItems = a.promoPerItems,
                                       NetPurchasePrice = a.NetPurchasePrice,
                                       IsOffer = b.IsOffer,
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
                                       FlashDealMaxQtyPersonCanTake = a.OfferMaxQtyPersonCanTake,
                                       FreeItemId = a.OfferFreeItemId,
                                   }).OrderByDescending(x => x.ItemNumber).ToList();
                    foreach (var it in newdata)
                    {
                        //Condition for offer end.
                        if (!(it.OfferStartTime <= DateTime.Now && it.OfferEndTime >= DateTime.Now))
                        {
                            if (it.OfferCategory == 2)
                            {
                                it.IsOffer = false;
                                it.FlashDealSpecialPrice = 0;
                                it.OfferCategory = 0;
                            }
                            else if (it.OfferCategory == 1)
                            {
                                it.IsOffer = false;
                                it.OfferCategory = 0;
                            }

                        }
                        else if ((it.OfferStartTime <= DateTime.Now && it.OfferEndTime >= DateTime.Now) && it.OfferCategory == 2)
                        {
                            if (FlashDealWithItemIds != null && FlashDealWithItemIds.Any(x => x.ItemId == it.ItemId))
                            {
                                it.IsOffer = false;
                                it.FlashDealSpecialPrice = 0;
                                it.OfferCategory = 0;
                            }
                        }

                        if (item.ItemMasters == null)
                        {
                            item.ItemMasters = new List<factoryItemdata>();
                        }
                        try
                        {/// Dream Point Logic && Margin Point
                            int? MP, PP;
                            double xPoint = xPointValue * 10;

                            //Customer (0.2 * 10=1)
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
                        //// by sudhir 22-08-2019
                        if (lang.Trim() == "hi")
                        {
                            if (it.HindiName != null)
                            {


                                if (it.IsSensitive == true)
                                {
                                    if (it.IsSensitiveMRP == false)
                                    {
                                        it.itemname = it.HindiName + " " + it.UnitofQuantity + " " + it.UOM; //item display name   
                                    }
                                    else
                                    {
                                        it.itemname = it.HindiName + " " + it.price + " MRP " + it.UnitofQuantity + " " + it.UOM; //item display name                               
                                    }
                                }
                                else
                                {
                                    it.itemname = it.HindiName + " " + it.price + " MRP "; //item display name                               
                                }
                            }
                            else
                            {
                                it.itemname = it.itemBaseName + " " + it.price + " MRP ";
                            }
                        }
                        //end
                        item.ItemMasters.Add(it);
                    }
                    if (item.ItemMasters != null)
                    {
                        item.Message = true;
                        return Request.CreateResponse(HttpStatusCode.OK, item);
                    }
                    else
                    {
                        item.Message = false;
                        return Request.CreateResponse(HttpStatusCode.OK, item);
                    }
                }
                catch (Exception ex)
                {
                    logger.Error("Error in ItemMaster " + ex.Message);
                    logger.Info("End  ItemMaster: ");
                    WRSITEM item = new WRSITEM();
                    item.Message = false;
                    return Request.CreateResponse(HttpStatusCode.OK, item);
                }
            }
        }

        /// <summary>
        /// Not show offer and flashdeal on Inactive customer
        /// 28/05/2019
        /// Sudeep Solanki
        /// </summary>
        /// <param name="lang"></param>
        /// <param name="itemname"></param>
        /// <param name="CustomerId"></param>
        /// <returns></returns>
        [Route("V3")]
        [HttpGet]

        public HttpResponseMessage getitembyitemnamev3(string lang, string itemname, int CustomerId)
        {
            using (var db = new AuthContext())
            {
                try
                {
                    WRSITEM item = new WRSITEM();
                    List<ItemMaster> itemList = new List<ItemMaster>();
                    //var custBrand = db.CustWarehouseDB.Where(x => x.CustomerId == CustomerId && x.Deleted == false).FirstOrDefault();
                    // var inActiveCustomer = db.Customers.Any(x => x.CustomerId == CustomerId && (x.Active == false || x.Deleted == true));
                    // var FlashDealWithItemIds = db.FlashDealItemConsumedDB.Where(x => x.CustomerId == CustomerId).Select(x => new { x.FlashDealId, x.ItemId });
                    var ActiveCustomer = db.Customers.FirstOrDefault(x => x.CustomerId == CustomerId);
                    var inActiveCustomer = ActiveCustomer != null && ActiveCustomer.Active == false && ActiveCustomer.Deleted == true ? true : false;
                    var warehouseId = ActiveCustomer != null ? ActiveCustomer.Warehouseid : 0;

                    db.Database.Log = s => Debug.WriteLine(s);

                    //var FlashDealWithItemIds = (from c in db.FlashDealItemConsumedDB.Where(x => x.CustomerId == CustomerId)
                    //                            join p in db.AppHomeDynamicDb.Where(x => x.Wid == warehouseId && x.active && !x.delete).SelectMany(x => x.detail.Select(y => new { id = y.id.Value, ItemId = y.ItemId }))
                    //                            on c.FlashDealId equals p.id into ps
                    //                            select new
                    //                            { c.FlashDealId, c.ItemId }).ToList();
                    string sqlquery = "SELECT a.[FlashDealId] AS [FlashDealId], a.[ItemId] AS [ItemId] FROM [dbo].[FlashDealItemConsumeds] AS a inner join AppHomeSectionItems c on a.FlashDealId = c.sectionItemId inner join dbo.AppHomeSections b on b.SectionID = c.apphomesections_SectionID  and b.Active=1 and b.[Deleted]=0  and b.WarehouseID=" + warehouseId +
                                       " WHERE a.[CustomerId]=" + CustomerId;
                    var FlashDealWithItemIds = db.Database.SqlQuery<FlashDealWithItem>(sqlquery).ToList();

                    var newdata = (from a in db.itemMasters
                                   where (a.itemname.Trim().ToLower().Contains(itemname.Trim().ToLower()) && a.WarehouseId == warehouseId && a.Deleted == false && a.active == true)
                                   join b in db.ItemMasterCentralDB on a.SellingSku equals b.SellingSku
                                   select new factoryItemdata
                                   {
                                       WarehouseId = a.WarehouseId,
                                       CompanyId = a.CompanyId,
                                       Categoryid = b.Categoryid,
                                       Discount = b.Discount,
                                       ItemId = a.ItemId,
                                       ItemNumber = b.Number,
                                       HindiName = a.HindiName,
                                       IsSensitive = a.IsSensitive,
                                       IsSensitiveMRP = a.IsSensitiveMRP,
                                       UnitofQuantity = a.UnitofQuantity,
                                       UOM = a.UOM,
                                       itemname = a.itemname,
                                       LogoUrl = b.LogoUrl,
                                       MinOrderQty = b.MinOrderQty,
                                       price = a.price,
                                       SubCategoryId = b.SubCategoryId,
                                       SubsubCategoryid = b.SubsubCategoryid,
                                       TotalTaxPercentage = b.TotalTaxPercentage,
                                       SellingUnitName = a.SellingUnitName,
                                       SellingSku = b.SellingSku,
                                       UnitPrice = a.UnitPrice,
                                       VATTax = b.VATTax,
                                       itemBaseName = a.itemBaseName,
                                       active = a.active,
                                       marginPoint = a.marginPoint,
                                       promoPerItems = a.promoPerItems,
                                       NetPurchasePrice = a.NetPurchasePrice,
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
                                       FlashDealMaxQtyPersonCanTake = a.OfferMaxQtyPersonCanTake,
                                       FreeItemId = a.OfferFreeItemId,
                                   }).OrderByDescending(x => x.ItemNumber).ToList();
                    foreach (var it in newdata)
                    {
                        //Condition for offer end.
                        if (!inActiveCustomer)
                        {
                            if (!(it.OfferStartTime <= DateTime.Now && it.OfferEndTime >= DateTime.Now))
                            {
                                if (it.OfferCategory == 2)
                                {
                                    it.IsOffer = false;
                                    it.FlashDealSpecialPrice = 0;
                                    it.OfferCategory = 0;
                                }
                                else if (it.OfferCategory == 1)
                                {
                                    it.IsOffer = false;
                                    it.OfferCategory = 0;
                                }

                            }
                            else if ((it.OfferStartTime <= DateTime.Now && it.OfferEndTime >= DateTime.Now) && it.OfferCategory == 2)
                            {
                                if (FlashDealWithItemIds != null && FlashDealWithItemIds.Any(x => x.ItemId == it.ItemId))
                                {
                                    it.IsOffer = false;
                                    it.FlashDealSpecialPrice = 0;
                                    it.OfferCategory = 0;
                                }
                            }
                        }
                        else
                        {
                            it.IsOffer = false;
                            it.FlashDealSpecialPrice = 0;
                            it.OfferCategory = 0;
                        }

                        if (item.ItemMasters == null)
                        {
                            item.ItemMasters = new List<factoryItemdata>();
                        }
                        try
                        {
                            if (!it.IsOffer)
                            {
                                /// Dream Point Logic && Margin Point
                                int? MP, PP;
                                double xPoint = xPointValue * 10;

                                //Customer (0.2 * 10=1)
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
                            else { it.dreamPoint = 0; }
                        }
                        catch { }
                        //// by sudhir 22-08-2019
                        if (lang.Trim() == "hi")
                        {
                            if (it.HindiName != null)
                            {
                                if (it.IsSensitive == true)
                                {
                                    if (it.IsSensitiveMRP == false)
                                    {
                                        it.itemname = it.HindiName + " " + it.UnitofQuantity + " " + it.UOM; //item display name   
                                    }
                                    else
                                    {
                                        it.itemname = it.HindiName + " " + it.price + " MRP " + it.UnitofQuantity + " " + it.UOM; //item display name                               
                                    }
                                }
                                else
                                {
                                    it.itemname = it.HindiName + " " + it.price + " MRP "; //item display name                               
                                }

                            }
                            else
                            {
                                it.itemname = it.itemBaseName + " " + it.price + " MRP ";
                            }
                        }
                        /////end
                        item.ItemMasters.Add(it);
                    }
                    if (item.ItemMasters != null)
                    {
                        item.Message = true;
                        return Request.CreateResponse(HttpStatusCode.OK, item);
                    }
                    else
                    {
                        item.Message = false;
                        return Request.CreateResponse(HttpStatusCode.OK, item);
                    }
                }
                catch (Exception ex)
                {
                    logger.Error("Error in ItemMaster " + ex.Message);
                    logger.Info("End  ItemMaster: ");
                    WRSITEM item = new WRSITEM();
                    item.Message = false;
                    return Request.CreateResponse(HttpStatusCode.OK, item);
                }
            }
        }

        [Route("V4")]
        [HttpPost]

        //[Obsolete]
        public HttpResponseMessage postitembyitemnamev4(SearchItem searchitem)
        {
            using (var db = new AuthContext())
            {
                if (!string.IsNullOrEmpty(searchitem.BarCode))
                {
                    var Number = db.ItemBarcodes.FirstOrDefault(i => i.Barcode == searchitem.BarCode && i.IsDeleted == false && i.IsActive).ItemNumber;
                    if (!string.IsNullOrEmpty(Number))
                    {
                        searchitem.Number = Number;
                    }
                }
                try
                {

                    WRSITEM item = new WRSITEM();
                    List<ItemMaster> itemList = new List<ItemMaster>();
                    var ActiveCustomer = db.Customers.FirstOrDefault(x => x.CustomerId == searchitem.CustomerId);
                    var inActiveCustomer = ActiveCustomer != null && ActiveCustomer.Active == false && ActiveCustomer.Deleted == true ? true : false;
                    var warehouseId = ActiveCustomer != null ? ActiveCustomer.Warehouseid : 0;


                    string sqlquery = "SELECT a.[FlashDealId] AS [FlashDealId], a.[ItemId] AS [ItemId] FROM [dbo].[FlashDealItemConsumeds] AS a inner join AppHomeSectionItems c on a.FlashDealId = c.sectionItemId inner join dbo.AppHomeSections b on b.SectionID = c.apphomesections_SectionID  and b.Active=1 and b.[Deleted]=0  and b.WarehouseID=" + warehouseId +
                                       " WHERE a.[CustomerId]=" + searchitem.CustomerId;
                    var FlashDealWithItemIds = db.Database.SqlQuery<FlashDealWithItem>(sqlquery).ToList();
                    var newdata = (from a in db.itemMasters
                                   where a.WarehouseId == warehouseId && (string.IsNullOrEmpty(searchitem.UOM) || a.UOM == searchitem.UOM)
                                   && (string.IsNullOrEmpty(searchitem.Unit) || a.UnitofQuantity == searchitem.Unit)
                                   && (searchitem.Category.Count == 0 || searchitem.Category.Contains(a.Categoryid))
                                   && (searchitem.BaseCat.Count == 0 || searchitem.BaseCat.Contains(a.BaseCategoryid))
                                   && (searchitem.SubCat.Count == 0 || searchitem.SubCat.Contains(a.SubCategoryId))
                                   && (searchitem.Brand.Count == 0 || searchitem.Brand.Contains(a.SubsubCategoryid))
                                   && a.Deleted == false
                                   && a.active == true
                                   && a.UnitPrice >= searchitem.minPrice
                                   && a.UnitPrice <= searchitem.maxPrice
                                   && (a.itemname.Trim().ToLower().Contains(searchitem.itemkeyword.Trim().ToLower()) || (a.Number.Trim().ToLower().Contains(searchitem.Number.Trim().ToLower())))
                                   //  join b in db.ItemMasterCentralDB on a.SellingSku equals b.SellingSku
                                   let limit = db.ItemLimitMasterDB.Where(p2 => a.ItemMultiMRPId == p2.ItemMultiMRPId && a.Number == p2.ItemNumber && a.WarehouseId == p2.WarehouseId).FirstOrDefault()

                                   select new factoryItemdata
                                   {
                                       BaseCategoryId = a.BaseCategoryid,
                                       IsItemLimit = limit != null ? limit.IsItemLimit : false,
                                       ItemlimitQty = limit != null && limit.IsItemLimit ? limit.ItemlimitQty : 0,
                                       WarehouseId = a.WarehouseId,
                                       CompanyId = a.CompanyId,
                                       Categoryid = a.Categoryid,
                                       Discount = a.Discount,
                                       ItemId = a.ItemId,
                                       ItemNumber = a.Number,
                                       HindiName = a.HindiName,
                                       IsSensitive = a.IsSensitive,
                                       IsSensitiveMRP = a.IsSensitiveMRP,
                                       UnitofQuantity = a.UnitofQuantity,
                                       UOM = a.UOM,
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
                                       VATTax = a.VATTax,
                                       itemBaseName = a.itemBaseName,
                                       active = a.active,
                                       marginPoint = a.marginPoint,
                                       promoPerItems = a.promoPerItems,
                                       NetPurchasePrice = a.NetPurchasePrice,
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
                                       FlashDealMaxQtyPersonCanTake = a.OfferMaxQtyPersonCanTake,
                                       FreeItemId = a.OfferFreeItemId,
                                       BillLimitQty = a.BillLimitQty
                                   }).OrderByDescending(x => x.ItemNumber).ToList();

                    var offerids = newdata.Where(x => x.OfferId > 0).Select(x => x.OfferId).Distinct().ToList();
                    var activeOfferids = offerids != null && offerids.Any() ? db.OfferDb.Where(x => offerids.Contains(x.OfferId) && x.IsActive && !x.IsDeleted && x.OfferAppType == "Retailer App" || x.OfferAppType == "Both").Select(x => x.OfferId).ToList() : new List<int>();


                    foreach (var it in newdata)
                    {
                        //Condition for offer end.
                        if (!inActiveCustomer)
                        {
                            if (!(it.OfferStartTime <= DateTime.Now && it.OfferEndTime >= DateTime.Now))
                            {
                                if (it.OfferCategory == 2)
                                {
                                    it.IsOffer = false;
                                    it.FlashDealSpecialPrice = 0;
                                    it.OfferCategory = 0;
                                }
                                else if (it.OfferCategory == 1)
                                {
                                    it.IsOffer = false;
                                    it.OfferCategory = 0;
                                }

                            }
                            else if ((it.OfferStartTime <= DateTime.Now && it.OfferEndTime >= DateTime.Now) && it.OfferCategory == 2)
                            {
                                if (FlashDealWithItemIds != null && FlashDealWithItemIds.Any(x => x.ItemId == it.ItemId))
                                {
                                    it.IsOffer = false;
                                    it.FlashDealSpecialPrice = 0;
                                    it.OfferCategory = 0;
                                }
                            }

                            if (it.OfferType != "FlashDeal")
                            {
                                if (activeOfferids.Any() && activeOfferids.Any(x => x == it.OfferId) && it.IsOffer)
                                    it.IsOffer = true;
                                else
                                    it.IsOffer = false;
                            }
                        }
                        else
                        {
                            it.IsOffer = false;
                            it.FlashDealSpecialPrice = 0;
                            it.OfferCategory = 0;
                        }

                        if (item.ItemMasters == null)
                        {
                            item.ItemMasters = new List<factoryItemdata>();
                        }
                        try
                        {
                            if (!it.IsOffer)
                            {
                                /// Dream Point Logic && Margin Point
                                int? MP, PP;
                                double xPoint = xPointValue * 10;

                                //Customer (0.2 * 10=1)
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

                            }
                            else { it.dreamPoint = 0; }

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
                        //// by sudhir 22-08-2019
                        if (searchitem.lang.Trim() == "hi")
                        {
                            if (it.HindiName != null)
                            {

                                if (it.IsSensitive == true && it.IsSensitiveMRP == true)
                                {
                                    it.itemname = it.HindiName + " " + it.price + " MRP " + it.UnitofQuantity + " " + it.UOM;
                                }
                                else if (it.IsSensitive == true && it.IsSensitiveMRP == false)
                                {
                                    it.itemname = it.HindiName + " " + it.UnitofQuantity + " " + it.UOM; //item display name 
                                }

                                else if (it.IsSensitive == false && it.IsSensitiveMRP == false)
                                {
                                    it.itemname = it.HindiName; //item display name
                                }
                                else if (it.IsSensitive == false && it.IsSensitiveMRP == true)
                                {
                                    it.itemname = it.HindiName + " " + it.price + " MRP";//item display name 
                                }
                            }

                        }
                        /////end
                        item.ItemMasters.Add(it);
                    }
                    if (item.ItemMasters != null && item.ItemMasters.Any())
                    {
                        List<int> itemIds = item.ItemMasters.Select(x => x.ItemId).ToList();
                        BackgroundTaskManager.Run(() =>
                        {
                            MongoDbHelper<CustomerProductSearch> mongoDbHelper = new MongoDbHelper<CustomerProductSearch>();
                            CustomerProductSearch customerProductSearch = new CustomerProductSearch
                            {
                                CreatedDate = indianTime,
                                customerId = searchitem.CustomerId,
                                keyword = searchitem.itemkeyword,
                                Items = itemIds,
                                IsDeleted = false
                            };
                            mongoDbHelper.Insert(customerProductSearch);
                        });
                    }
                    if (item.ItemMasters != null)
                    {
                        item.Message = true;
                        return Request.CreateResponse(HttpStatusCode.OK, item);
                    }
                    else
                    {
                        item.Message = false;
                        return Request.CreateResponse(HttpStatusCode.OK, item);
                    }
                }
                catch (Exception ex)
                {
                    logger.Error("Error in ItemMaster " + ex.Message);
                    logger.Info("End  ItemMaster: ");
                    WRSITEM item = new WRSITEM();
                    item.Message = false;
                    return Request.CreateResponse(HttpStatusCode.OK, item);
                }
            }
        }
        #endregion



        [Route("CheckItemsOffer")]
        [HttpPost]
        public HttpResponseMessage CheckItemsOffer(ItemOfferList itemOfferList)
        {
            using (var db = new AuthContext())
            {
                try
                {

                    if (itemOfferList != null && itemOfferList.OfferItems != null && itemOfferList.OfferItems.Any())
                    {
                        string sqlquery = "SELECT a.[FlashDealId] AS [FlashDealId], a.[ItemId] AS [ItemId] FROM [dbo].[FlashDealItemConsumeds] AS a inner join AppHomeSectionItems c on a.FlashDealId = c.sectionItemId inner join dbo.AppHomeSections b on b.SectionID = c.apphomesections_SectionID  and b.[Deleted]=0 and b.Active=1 and b.WarehouseID=" + itemOfferList.WarehouseId +
                                     " WHERE a.[CustomerId]=" + itemOfferList.CustomerId;
                        var FlashDealWithItemIds = db.Database.SqlQuery<FlashDealWithItem>(sqlquery).ToList();
                        //var FlashDealWithItemIds =  (from c in db.FlashDealItemConsumedDB.Where(x => x.CustomerId == itemOfferList.CustomerId)
                        //         join p in db.AppHomeDynamicDb.Where(x => x.Wid == itemOfferList.WarehouseId && x.active && !x.delete).SelectMany(x=>x.detail.Select(y=>new { id=y.id.Value,ItemId=y.ItemId }))
                        //         on c.FlashDealId equals p.id into ps
                        //         select new
                        //         { c.FlashDealId, c.ItemId }).ToList()  ;
                        // var FlashDealWithItemIds = db.FlashDealItemConsumedDB.Where(x => x.CustomerId == itemOfferList.CustomerId).Select(x => new { x.FlashDealId, x.ItemId });
                        var Itemids = itemOfferList.OfferItems.Select(x => x.ItemId);
                        var newdata = (from a in db.itemMasters
                                       where (Itemids.Contains(a.ItemId) && a.WarehouseId == itemOfferList.WarehouseId && a.Deleted == false && a.active == true)
                                       select new
                                       {
                                           a.ItemId,
                                           a.IsOffer,
                                           a.OfferCategory,
                                           a.OfferStartTime,
                                           a.OfferEndTime,
                                           a.OfferQtyAvaiable,
                                           a.OfferType
                                       }).ToList();
                        foreach (var it in newdata)
                        {
                            var item = itemOfferList.OfferItems.FirstOrDefault(x => x.ItemId == it.ItemId);
                            if (item != null)
                            {
                                item.IsOffer = it.IsOffer;
                                item.OfferCategory = it.OfferCategory;
                                item.OfferStartTime = it.OfferStartTime;
                                item.OfferEndTime = it.OfferEndTime;
                                item.OfferQtyAvaiable = it.OfferQtyAvaiable;
                                item.OfferType = it.OfferType;
                                //Condition for offer end.
                                if (!(it.OfferStartTime <= DateTime.Now && it.OfferEndTime >= DateTime.Now))
                                {
                                    if (it.OfferCategory == 2)
                                    {
                                        item.IsOffer = false;
                                        item.OfferCategory = 0;
                                        item.OfferStartTime = (DateTime?)null;
                                        item.OfferEndTime = (DateTime?)null;
                                        item.OfferQtyAvaiable = 0;
                                        item.OfferType = string.Empty;
                                    }
                                    else if (it.OfferCategory == 1)
                                    {
                                        item.IsOffer = false;
                                        item.OfferCategory = 0;
                                        item.OfferStartTime = (DateTime?)null;
                                        item.OfferEndTime = (DateTime?)null;
                                        item.OfferQtyAvaiable = 0;
                                        item.OfferType = string.Empty;
                                    }
                                }
                                else if ((it.OfferStartTime <= DateTime.Now && it.OfferEndTime >= DateTime.Now) && it.OfferCategory == 2)
                                {
                                    if (FlashDealWithItemIds != null && FlashDealWithItemIds.Any(x => x.ItemId == it.ItemId))
                                    {
                                        item.IsOffer = false;
                                        item.OfferCategory = 0;
                                        item.OfferStartTime = (DateTime?)null;
                                        item.OfferEndTime = (DateTime?)null;
                                        item.OfferQtyAvaiable = 0;
                                        item.OfferType = string.Empty;
                                    }
                                }
                            }
                        }
                    }
                    if (itemOfferList != null && itemOfferList.OfferItems != null && itemOfferList.OfferItems.Any())
                    {
                        itemOfferList.Message = true;
                        return Request.CreateResponse(HttpStatusCode.OK, itemOfferList);
                    }
                    else
                    {
                        itemOfferList.Message = false;
                        return Request.CreateResponse(HttpStatusCode.OK, itemOfferList);
                    }
                }
                catch (Exception ex)
                {
                    logger.Error("Error in ItemMaster " + ex.Message);
                    logger.Info("End  ItemMaster: ");
                    WRSITEM item = new WRSITEM();
                    item.Message = false;
                    return Request.CreateResponse(HttpStatusCode.OK, item);
                }
            }
        }

        [Route("GenerateItemCode")]
        public dynamic getItemCode()
        {

            try
            {
                var identity = User.Identity as ClaimsIdentity;
                int compid = 0, userid = 0;
                int warehouseid = 0;

                ItemMaster aaa = new ItemMaster();
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
                        warehouseid = int.Parse(claim.Value);
                    }
                }
                using (AuthContext db = new AuthContext())
                {
                    int CompanyId = compid;
                    aaa.itemcode = db.gtItemCodeByID();
                    var atm = Int32.Parse(aaa.itemcode);
                    return atm;
                }
            }
            catch (Exception ex)
            {
                logger.Error("Error in ItemMaster " + ex.Message);
                logger.Info("End  ItemMaster: ");
                return null;
            }
        }

        /// <summary>
        /// Updated by 09/11/2019
        /// Get item by barcode
        /// </summary>
        /// <param name="customerid"></param>
        /// <param name="barcode"></param>
        /// <returns></returns>
        [Route("GetItemByBarcode")]
        [HttpGet]
        public HttpResponseMessage GetItemByBarcode(SearchItem searchitem)
        {

            using (var db = new AuthContext())
            {
                WRSITEM item = new WRSITEM();
                var barcodeitem = db.ItemBarcodes.FirstOrDefault(i => i.Barcode == searchitem.itemkeyword && i.IsDeleted == false && i.IsActive == true);

                if (barcodeitem != null)
                {

                    List<ItemMaster> itemList = new List<ItemMaster>();
                    var ActiveCustomer = db.Customers.FirstOrDefault(x => x.CustomerId == searchitem.CustomerId);
                    var inActiveCustomer = ActiveCustomer != null && ActiveCustomer.Active == false && ActiveCustomer.Deleted == true ? true : false;
                    var warehouseId = ActiveCustomer != null ? ActiveCustomer.Warehouseid : 0;


                    string sqlquery = "SELECT a.[FlashDealId] AS [FlashDealId], a.[ItemId] AS [ItemId] FROM [dbo].[FlashDealItemConsumeds] AS a inner join AppHomeSectionItems c on a.FlashDealId = c.sectionItemId inner join dbo.AppHomeSections b on b.SectionID = c.apphomesections_SectionID  and b.Active=1 and b.[Deleted]=0  and b.WarehouseID=" + warehouseId +
                                       " WHERE a.[CustomerId]=" + searchitem.CustomerId;
                    var FlashDealWithItemIds = db.Database.SqlQuery<FlashDealWithItem>(sqlquery).ToList();

                    var newdata = (from a in db.itemMasters
                                   where a.WarehouseId == warehouseId && (string.IsNullOrEmpty(searchitem.UOM) || a.UOM == searchitem.UOM)
                                   && (string.IsNullOrEmpty(searchitem.Unit) || a.UnitofQuantity == searchitem.Unit)
                                   && (searchitem.Category.Count == 0 || searchitem.Category.Contains(a.Categoryid))
                                   && (searchitem.BaseCat.Count == 0 || searchitem.BaseCat.Contains(a.BaseCategoryid))
                                   && (searchitem.SubCat.Count == 0 || searchitem.SubCat.Contains(a.SubCategoryId))
                                   && (searchitem.Brand.Count == 0 || searchitem.Brand.Contains(a.SubsubCategoryid))
                                   && a.Deleted == false
                                   && a.active == true
                                   && a.UnitPrice >= searchitem.minPrice
                                   && a.UnitPrice <= searchitem.maxPrice
                                  && a.Number.Trim().ToLower().Equals(barcodeitem.ItemNumber.Trim().ToLower())
                                   join b in db.ItemMasterCentralDB on a.SellingSku equals b.SellingSku
                                   let limit = db.ItemLimitMasterDB.Where(p2 => a.ItemMultiMRPId == p2.ItemMultiMRPId && a.Number == p2.ItemNumber && a.WarehouseId == p2.WarehouseId).FirstOrDefault()

                                   select new factoryItemdata
                                   {
                                       BaseCategoryId = a.BaseCategoryid,
                                       IsItemLimit = limit != null ? limit.IsItemLimit : false,
                                       ItemlimitQty = limit != null && limit.IsItemLimit ? limit.ItemlimitQty : 0,
                                       WarehouseId = a.WarehouseId,
                                       CompanyId = a.CompanyId,
                                       Categoryid = b.Categoryid,
                                       Discount = b.Discount,
                                       ItemId = a.ItemId,
                                       ItemNumber = b.Number,
                                       HindiName = a.HindiName,
                                       IsSensitive = a.IsSensitive,
                                       IsSensitiveMRP = a.IsSensitiveMRP,
                                       UnitofQuantity = a.UnitofQuantity,
                                       UOM = a.UOM,
                                       itemname = a.itemname,
                                       LogoUrl = b.LogoUrl,
                                       MinOrderQty = b.MinOrderQty,
                                       price = a.price,
                                       SubCategoryId = b.SubCategoryId,
                                       SubsubCategoryid = b.SubsubCategoryid,
                                       TotalTaxPercentage = b.TotalTaxPercentage,
                                       SellingUnitName = a.SellingUnitName,
                                       SellingSku = b.SellingSku,
                                       UnitPrice = a.UnitPrice,
                                       VATTax = b.VATTax,
                                       itemBaseName = a.itemBaseName,
                                       active = a.active,
                                       marginPoint = a.marginPoint,
                                       promoPerItems = a.promoPerItems,
                                       NetPurchasePrice = a.NetPurchasePrice,
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
                                       FlashDealMaxQtyPersonCanTake = a.OfferMaxQtyPersonCanTake,
                                       FreeItemId = a.OfferFreeItemId,
                                       ItemMultiMRPId = a.ItemMultiMRPId,
                                       BillLimitQty = a.BillLimitQty
                                   }).OrderByDescending(x => x.ItemNumber).ToList();

                    var offerids = newdata.Where(x => x.OfferId > 0).Select(x => x.OfferId).Distinct().ToList();
                    var activeOfferids = offerids != null && offerids.Any() ? db.OfferDb.Where(x => offerids.Contains(x.OfferId) && x.IsActive && !x.IsDeleted && x.OfferAppType == "Retailer App" || x.OfferAppType == "Both" || x.OfferAppType == "Sales App").Select(x => x.OfferId).ToList() : new List<int>();


                    foreach (var it in newdata)
                    {
                        //Condition for offer end.
                        if (!inActiveCustomer)
                        {
                            if (!(it.OfferStartTime <= DateTime.Now && it.OfferEndTime >= DateTime.Now))
                            {
                                if (it.OfferCategory == 2)
                                {
                                    it.IsOffer = false;
                                    it.FlashDealSpecialPrice = 0;
                                    it.OfferCategory = 0;
                                }
                                else if (it.OfferCategory == 1)
                                {
                                    it.IsOffer = false;
                                    it.OfferCategory = 0;
                                }

                            }
                            else if ((it.OfferStartTime <= DateTime.Now && it.OfferEndTime >= DateTime.Now) && it.OfferCategory == 2)
                            {
                                if (FlashDealWithItemIds != null && FlashDealWithItemIds.Any(x => x.ItemId == it.ItemId))
                                {
                                    it.IsOffer = false;
                                    it.FlashDealSpecialPrice = 0;
                                    it.OfferCategory = 0;
                                }
                            }

                            if (it.OfferType != "FlashDeal")
                            {
                                if (activeOfferids.Any() && activeOfferids.Any(x => x == it.OfferId) && it.IsOffer)
                                    it.IsOffer = true;
                                else
                                    it.IsOffer = false;
                            }
                        }
                        else
                        {
                            it.IsOffer = false;
                            it.FlashDealSpecialPrice = 0;
                            it.OfferCategory = 0;
                        }

                        if (item.ItemMasters == null)
                        {
                            item.ItemMasters = new List<factoryItemdata>();
                        }
                        try
                        {
                            if (!it.IsOffer)
                            {
                                /// Dream Point Logic && Margin Point
                                int? MP, PP;
                                double xPoint = xPointValue * 10;

                                //Customer (0.2 * 10=1)
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

                            }
                            else { it.dreamPoint = 0; }

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
                        //// by sudhir 22-08-2019
                        if (searchitem.lang.Trim() == "hi")
                        {
                            if (it.HindiName != null)
                            {
                                if (it.IsSensitive == true)
                                {
                                    if (it.IsSensitiveMRP == false)
                                    {
                                        it.itemname = it.HindiName + " " + it.UnitofQuantity + " " + it.UOM; //item display name   
                                    }
                                    else
                                    {
                                        it.itemname = it.HindiName + " " + it.price + " MRP " + it.UnitofQuantity + " " + it.UOM; //item display name                               
                                    }
                                }
                                else
                                {
                                    it.itemname = it.HindiName + " " + it.price + " MRP "; //item display name                               
                                }

                            }
                            else
                            {
                                it.itemname = it.itemBaseName + " " + it.price + " MRP ";
                            }
                        }
                        /////end
                        item.ItemMasters.Add(it);
                    }
                    if (item.ItemMasters != null && item.ItemMasters.Any())
                    {
                        List<int> itemIds = item.ItemMasters.Select(x => x.ItemId).ToList();
                        BackgroundTaskManager.Run(() =>
                        {
                            MongoDbHelper<CustomerProductSearch> mongoDbHelper = new MongoDbHelper<CustomerProductSearch>();
                            CustomerProductSearch customerProductSearch = new CustomerProductSearch
                            {
                                CreatedDate = indianTime,
                                customerId = searchitem.CustomerId,
                                keyword = searchitem.itemkeyword,
                                Items = itemIds,
                                IsDeleted = false
                            };
                            mongoDbHelper.Insert(customerProductSearch);
                        });
                    }
                    if (item.ItemMasters != null)
                    {
                        item.Message = true;
                        return Request.CreateResponse(HttpStatusCode.OK, item);
                    }
                    else
                    {
                        item.Message = false;
                        return Request.CreateResponse(HttpStatusCode.OK, item);
                    }
                }
                else
                {
                    item.Message = false;
                    return Request.CreateResponse(HttpStatusCode.OK, item);
                }
            }
        }


        // removed by Harry : 21 May 2019 FindItemHighDP
        [Route("FindItemHighDP")]
        [HttpGet]
        public List<ItemMaster> FindItemHighDP(int warehouseid)
        {

            try
            {
                using (AuthContext db = new AuthContext())
                {
                    if (warehouseid > 0)
                    {
                        var data = db.itemMasters.Where(a => a.WarehouseId == warehouseid && a.active == true && a.Deleted == false).ToList();
                        List<ItemMaster> hdp = new List<ItemMaster>();
                        foreach (var kk in data)
                        {
                            var dd = 2 * (kk.UnitPrice - kk.PurchasePrice);
                            if (dd > 50)
                            {
                                hdp.Add(kk);
                            }
                        }
                        hdp = hdp.Where(x => x.IsHighestDPItem == true).ToList();
                        return null;
                        // return hdp;       // removed by Harry : 21 May 2019 FindItemHighDP
                    }
                    else
                    {
                        var data = db.itemMasters.ToList();
                        List<ItemMaster> hdp = new List<ItemMaster>();
                        foreach (var kk in data)
                        {
                            var dd = 2 * (kk.UnitPrice - kk.PurchasePrice);
                            if (dd > 50)
                            {
                                hdp.Add(kk);
                            }
                        }
                        //return hdp;       // removed by Harry : 21 May 2019 FindItemHighDP

                        return null;
                    }
                }
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        [Route("FindItemHighDPForWeb")]
        [HttpGet]
        public List<ItemMaster> FindItemHighDPForWeb()
        {

            try
            {
                using (AuthContext db = new AuthContext())
                {
                    var data = db.itemMasters.ToList();
                    List<ItemMaster> hdp = new List<ItemMaster>();
                    foreach (var kk in data)
                    {
                        var dd = 2 * (kk.UnitPrice - kk.PurchasePrice);
                        if (dd > 50)
                        {
                            hdp.Add(kk);
                        }
                    }
                    return hdp;
                }
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        [Route("SelectedItem")]
        [HttpPost]
        public string SelectedItem(List<ItemMaster> SelectedItem)
        {
            logger.Info("start current stock: ");
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
                using (AuthContext context = new AuthContext())
                {
                    int CompanyId = compid;
                    string result = context.UpdateHighDP(CompanyId, Warehouse_id, SelectedItem);
                    logger.Info("End  current stock: ");
                    return result;
                }

            }
            catch (Exception ex)
            {
                logger.Error("Error in current stock " + ex.Message);
                logger.Info("End  current stock: ");
                return "";
            }
        }

        [Route("FreeItemFromItemmaster")]
        [HttpGet]
        public HttpResponseMessage getFreeItem()
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
                int CompanyId = compid;
                using (AuthContext odd = new AuthContext())
                {
                    if (Warehouse_id > 0)
                    {
                        var Data = odd.itemMasters.Where(x => x.active == true && x.Deleted == false && x.WarehouseId == Warehouse_id && x.free == true).ToList();
                        return Request.CreateResponse(HttpStatusCode.OK, Data);
                    }
                    else { return null; }
                }
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, ex.Message);
            }
        }

        [Route("DreamItemFromItemmaster")]
        [HttpGet]
        public HttpResponseMessage getFreeDreamItem()
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
                int CompanyId = compid;
                using (AuthContext db = new AuthContext())
                {

                    if (Warehouse_id > 0)
                    {
                        var Data = db.RewardItemsDb.Where(x => x.WarehouseId == Warehouse_id).ToList();
                        return Request.CreateResponse(HttpStatusCode.OK, Data);
                    }
                    else { return null; }
                }
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, ex.Message);
            }
        }

        [Route("GetItemBySubCatId")]
        [HttpGet]
        public IEnumerable<ItemMaster> GetItemBySubCatId(int subcategoryid)
        {
            using (var db = new AuthContext())
            {
                logger.Info("start ItemMaster: ");
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
                    int CompanyId = compid;
                    logger.Info("User ID : {0} , Company Id : {1}", compid, userid);

                    if (Warehouse_id > 0)
                    {
                        var data = db.itemMasters.Where(a => a.active == true && a.Deleted == false && a.WarehouseId == Warehouse_id && a.SubCategoryId == subcategoryid).AsEnumerable();
                        logger.Info("End ItemMaster: ");
                        return data;
                    }
                    else
                    {
                        var data = db.itemMasters.Where(a => a.active == true && a.Deleted == false && a.SubCategoryId == subcategoryid).AsEnumerable();
                        logger.Info("End ItemMaster: ");
                        return data;
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

        [HttpGet]
        [Route("SearchinitemPOadd")]
        public IEnumerable<ItemMaster> searchsPOadd(string key, int Warehouseid)
        {
            logger.Info("start Item Master: ");
            List<ItemMaster> ass = new List<ItemMaster>();
            List<ItemMaster> result = new List<ItemMaster>();
            try
            {
                var identity = User.Identity as ClaimsIdentity;
                int compid = 0, userid = 0;
                int WarehouseId = 0;
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
                        WarehouseId = int.Parse(claim.Value);
                    }
                }
                logger.Info("User ID : {0} , Company Id : {1}", compid, userid);
                int CompanyId = compid;
                using (AuthContext db = new AuthContext())
                {
                    WarehouseId = Warehouseid;
                    if (WarehouseId > 0)
                    {
                        ass = db.itemMasters.Where(t => t.WarehouseId == WarehouseId && (t.itemname.Contains(key) || t.Number.Contains(key)) && t.Deleted == false).ToList();
                        List<string> PurchaseSku = new List<string>();
                        foreach (var item in ass)
                        {
                            if (ass.Any(t => t.PurchaseSku == item.PurchaseSku && t.active))
                            {
                                var maxPurchasePrice = ass.Where(t => t.PurchaseSku == item.PurchaseSku && t.active).Max(x => x.POPurchasePrice);
                                var items = ass.Where(t => t.PurchaseSku == item.PurchaseSku && t.active && t.POPurchasePrice == maxPurchasePrice).FirstOrDefault();
                                if (items != null && !PurchaseSku.Any(x => x == items.PurchaseSku))
                                {
                                    result.Add(items);
                                    PurchaseSku.Add(items.PurchaseSku);
                                }
                            }
                        }
                        return result;
                    }

                    else
                    {
                        return null;
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error("Error in Item Master " + ex.Message);
                logger.Info("End  Item Master: ");
                return null;
            }
        }




        #region SA:Supplier Item from all Warehouse
        /// <summary>
        /// 
        /// </summary>
        /// <param name="SupplierId"></param>
        /// <returns></returns>
        [Route("SupplierItem")]
        [HttpGet]

        public async Task<HttpResponseMessage> SupplierItemAllWarehouseByGroup(int SupplierId)
        {
            using (var db = new AuthContext())
            {
                resDTO res;
                try
                {

                    WRSITEM item = new WRSITEM();
                    List<ItemMaster> itemList = new List<ItemMaster>();
                    var SupplierWData = await db.SupplierWarehouseDB.Where(x => x.SupplierId == SupplierId && x.Deleted == false).ToListAsync();//get list of warehouse foe supplier

                    foreach (var Swarehouse in SupplierWData)
                    {
                        var newdata = await (from a in db.itemMasters
                                             where (a.WarehouseId == Swarehouse.WarehouseId && a.Deleted == false && a.SupplierId == SupplierId)
                                             join b in db.ItemMasterCentralDB on a.SellingSku equals b.SellingSku
                                             select new factoryItemdata
                                             {
                                                 WarehouseId = a.WarehouseId,
                                                 CompanyId = a.CompanyId,
                                                 Categoryid = b.Categoryid,
                                                 Discount = b.Discount,
                                                 ItemId = a.ItemId,
                                                 ItemNumber = b.Number,
                                                 itemname = b.itemname,
                                                 LogoUrl = b.SellingSku,
                                                 MinOrderQty = b.MinOrderQty,
                                                 price = b.price,
                                                 DepoId = a.DepoId,
                                                 DepoName = a.DepoName,
                                                 SubCategoryId = b.SubCategoryId,
                                                 SubsubCategoryid = b.SubsubCategoryid,
                                                 TotalTaxPercentage = b.TotalTaxPercentage,
                                                 SellingUnitName = b.SellingUnitName,
                                                 SellingSku = b.SellingSku,
                                                 UnitPrice = a.UnitPrice,
                                                 HindiName = b.HindiName,
                                                 VATTax = b.VATTax,
                                                 marginPoint = b.marginPoint,
                                                 promoPerItems = b.promoPerItems,
                                                 IsOffer = b.IsOffer,
                                                 active = a.active,
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
                                                 FlashDealMaxQtyPersonCanTake = a.OfferMaxQtyPersonCanTake
                                             }).OrderByDescending(x => x.ItemNumber).ToListAsync();
                        foreach (var it in newdata)
                        {
                            if (item.ItemMasters == null)
                            {
                                item.ItemMasters = new List<factoryItemdata>();
                            }
                            try
                            {/// Dream Point Logic
                                int? MP, PP, xPoint = 2;
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
                                    MP = Convert.ToInt32(it.marginPoint) * xPoint;
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

                                /// Margin logic
                                double p = it.price - it.UnitPrice;
                                double s = p / it.UnitPrice;
                                double d = s * 100;
                                if (d > 0)
                                {
                                    it.marginPoint = d;
                                }
                                else
                                {
                                    it.marginPoint = 0;
                                }
                            }
                            catch { }
                            //if()
                            //{

                            //    item.ItemMasters.Add(it);
                            //}
                            item.ItemMasters.Add(it);
                        }
                    }
                    res = new resDTO
                    {
                        ItemMasters = item.ItemMasters,
                        Status = true,
                        Message = "Success"
                    };
                    return Request.CreateResponse(HttpStatusCode.OK, res);
                }
                catch (Exception ex)
                {
                    res = new resDTO()
                    {
                        ItemMasters = null,
                        Message = "Failed",
                        Status = false
                    };
                    logger.Error("Error in ItemMaster " + ex.Message);
                    logger.Info("End ItemMaster: ");
                    return Request.CreateResponse(HttpStatusCode.OK, res);
                }
            }
        }
        #endregion

        #region SA:Supplier Item from all Warehouse
        /// <summary>
        /// 
        /// </summary>
        /// <param name="SupplierId"></param>
        /// <returns></returns>
        [Route("SupplierItemWID")]
        [HttpGet]

        public async Task<HttpResponseMessage> SupplierItemWID(int SupplierId, int WarehouseId)
        {

            using (var db = new AuthContext())
            {
                SupplierItemObj res;
                List<SupplierItemDC> _result = new List<SupplierItemDC>();
                string sqlquery = "select  distinct Number, active, itemname, price, LogoUrl, Number from ItemMasters with(nolock) where SupplierId = " + SupplierId + " and WarehouseId =" + WarehouseId + " and Deleted = 0";
                _result = await db.Database.SqlQuery<SupplierItemDC>(sqlquery).ToListAsync();
                res = new SupplierItemObj
                {
                    item = _result,
                    Status = true,
                    Message = "Success"
                };
                return Request.CreateResponse(HttpStatusCode.OK, res);
            }
        }


        [Route("SupplierItemWithPage")]
        [HttpGet]

        public async Task<HttpResponseMessage> SupplierItemWithPage(int SupplierId, int PageNumber, int PageSized)
        {

            using (var db = new AuthContext())
            {
                SupplierItemObj res;
                List<SupplierItemDC> _result = new List<SupplierItemDC>();
                string sqlQuery = "select  WarehouseName,Number, active, itemname, price, LogoUrl, Number from ItemMasters with(nolock) where SupplierId=" + SupplierId + " ORDER BY ItemId desc OFFSET " + PageSized * (PageNumber - 1) + " ROWS FETCH NEXT " + PageSized + " ROWS ONLY";
                //string sqlquery = "select  distinct Number, active, itemname, price, LogoUrl, Number from ItemMasters with(nolock) where SupplierId = " + SupplierId + " and WarehouseId =" + WarehouseId + " and Deleted = 0";
                _result = await db.Database.SqlQuery<SupplierItemDC>(sqlQuery).ToListAsync();
                res = new SupplierItemObj
                {
                    item = _result,
                    Status = true,
                    Message = "Success"
                };
                return Request.CreateResponse(HttpStatusCode.OK, res);
            }
        }
        #endregion

        #region SA:depo Item from all Warehouse
        /// <summary>
        /// 
        /// </summary>
        /// <param name="DepoId"></param>
        /// <returns></returns>
        [Route("SupplierItemByDepo")]

        [HttpGet]
        public async Task<HttpResponseMessage> SupplierItemByDepoWarehouseByGroup(int DepoId)
        {
            using (var db = new AuthContext())
            {
                resDTO res;
                try
                {

                    WRSITEM item = new WRSITEM();
                    List<ItemMaster> itemList = new List<ItemMaster>();
                    //var SupplierWData = await db.DepoMasters.Where(x => x.DepoId == DepoId && x.Deleted == false).ToListAsync();//get list of warehouse foe supplier

                    //foreach (var Swarehouse in SupplierWData)
                    //{
                    var newdata = await (from a in db.itemMasters
                                         where (a.Deleted == false && a.DepoId == DepoId)
                                         select new factoryItemdata
                                         {
                                             WarehouseId = a.WarehouseId,
                                             CompanyId = a.CompanyId,
                                             Categoryid = a.Categoryid,
                                             Discount = a.Discount,
                                             ItemId = a.ItemId,
                                             ItemNumber = a.Number,
                                             itemname = a.itemname,
                                             LogoUrl = a.SellingSku,
                                             MinOrderQty = a.MinOrderQty,
                                             price = a.price,
                                             DepoId = a.DepoId,
                                             DepoName = a.DepoName,
                                             SubCategoryId = a.SubCategoryId,
                                             SubsubCategoryid = a.SubsubCategoryid,
                                             TotalTaxPercentage = a.TotalTaxPercentage,
                                             SellingUnitName = a.SellingUnitName,
                                             SellingSku = a.SellingSku,
                                             UnitPrice = a.UnitPrice,
                                             HindiName = a.HindiName,
                                             VATTax = a.VATTax,
                                             marginPoint = a.marginPoint,
                                             promoPerItems = a.promoPerItems,
                                             IsOffer = a.IsOffer,
                                             active = a.active,
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
                                             FlashDealMaxQtyPersonCanTake = a.OfferMaxQtyPersonCanTake
                                         }).OrderByDescending(x => x.ItemNumber).ToListAsync();
                    foreach (var it in newdata)
                    {
                        if (item.ItemMasters == null)
                        {
                            item.ItemMasters = new List<factoryItemdata>();
                        }
                        try
                        {/// Dream Point Logic
                            int? MP, PP, xPoint = 2;
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
                                MP = Convert.ToInt32(it.marginPoint) * xPoint;
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

                            /// Margin logic
                            double p = it.price - it.UnitPrice;
                            double s = p / it.UnitPrice;
                            double d = s * 100;
                            if (d > 0)
                            {
                                it.marginPoint = d;
                            }
                            else
                            {
                                it.marginPoint = 0;
                            }
                        }
                        catch { }
                        item.ItemMasters.Add(it);
                    }
                    // }
                    res = new resDTO
                    {
                        ItemMasters = item.ItemMasters,
                        Status = true,
                        Message = "Success"
                    };
                    return Request.CreateResponse(HttpStatusCode.OK, res);
                }
                catch (Exception ex)
                {
                    res = new resDTO()
                    {
                        ItemMasters = null,
                        Message = "Failed",
                        Status = false
                    };
                    logger.Error("Error in ItemMaster " + ex.Message);
                    logger.Info("End ItemMaster: ");
                    return Request.CreateResponse(HttpStatusCode.OK, res);
                }
            }
        }
        #endregion

        #region Check Current Stock
        [Route("GetStock")]
        [HttpGet]
        public HttpResponseMessage CheckStock(int WarehouseId)//get all Issuances which are active for the delivery boy
        {
            using (var db = new AuthContext())
            {
                try
                {
                    var identity = User.Identity as ClaimsIdentity;
                    int compid = 0, userid = 0;
                    int warehouseid = 0;
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
                            warehouseid = int.Parse(claim.Value);
                        }
                    }
                    if (warehouseid > 0)
                    {
                        var Item = db.DbCurrentStock.Where(x => x.WarehouseId == WarehouseId).ToList();
                        return Request.CreateResponse(HttpStatusCode.OK, Item);
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.OK, "Null");
                    }

                }
                catch (Exception ex)
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, ex.Message);
                }
            }
        }
        #endregion

        #region GetWarehouseItem
        [Route("GetWarehouseItem")]
        [HttpGet]
        public HttpResponseMessage GetWarehouseItem(int WarehouseId)//get all Issuances which are active for the delivery boy
        {
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

                    if (WarehouseId > 0)
                    {
                        var Item = db.itemMasters.Where(x => x.WarehouseId == WarehouseId && x.Deleted == false && x.active == true).ToList();

                        return Request.CreateResponse(HttpStatusCode.OK, Item);
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.OK, "Null");
                    }

                }
                catch (Exception ss)
                {

                    return Request.CreateResponse(HttpStatusCode.OK, "Null");

                }
            }
        }

        [Route("GetWarehouseItemWithInactive")]
        [HttpGet]
        public HttpResponseMessage GetWarehouseItemWithInactive(int WarehouseId)//get all Issuances which are active for the delivery boy
        {
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

                    if (WarehouseId > 0)
                    {
                        var Item = db.itemMasters.Where(x => x.WarehouseId == WarehouseId && x.Deleted == false).Distinct().ToList();

                        return Request.CreateResponse(HttpStatusCode.OK, Item);
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.OK, "Null");
                    }

                }
                catch (Exception ss)
                {

                    return Request.CreateResponse(HttpStatusCode.OK, "Null");

                }
            }
        }

        #endregion


        #region for MultiMrp By Harry 20/03/2019
        [Route("AddItemMRP")]
        [HttpPost]
        public string AddItemMRP(List<ItemMultiMRPAdd> ItemMultiMRPItem)
        {
            string result = "success";
            using (var db = new AuthContext())
            {
                //var identity = User.Identity as ClaimsIdentity;
                //int compid = 0, userid = 0;
                //foreach (Claim claim in identity.Claims)
                //{
                //    if (claim.Type == "compid")
                //    {
                //        compid = int.Parse(claim.Value);
                //    }
                //    if (claim.Type == "userid")
                //    {
                //        userid = int.Parse(claim.Value);
                //    }
                //}

                var identity = User.Identity as ClaimsIdentity;
                int userid = 0, compid = 0;

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                    userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);


                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))
                    compid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);

                var people = db.Peoples.FirstOrDefault(x => x.PeopleID == userid && x.Active);
                if (ItemMultiMRPItem.Count > 0 && people != null && userid > 0)
                {
                    var itemNumber = ItemMultiMRPItem.Select(x => x.ItemNumber).Distinct().FirstOrDefault();
                    var itemmasters = db.ItemMasterCentralDB.Where(x => x.Number == itemNumber).FirstOrDefault();
                    foreach (var data in ItemMultiMRPItem)
                    {
                        if (!db.ItemMultiMRPDB.Any(x => x.ItemNumber == data.ItemNumber && x.MRP == data.MRP && x.UOM == data.UOM
                                && x.UnitofQuantity == data.UnitofQuantity && x.ColourImage == data.ColourImage && x.ColourName == data.ColourName && x.Deleted == false))
                        {
                            ItemMultiMRP item = new ItemMultiMRP();
                            item.CompanyId = compid;
                            item.itemBaseName = data.itemBaseName;
                            item.ItemNumber = data.ItemNumber;
                            item.MRP = data.MRP;
                            item.UOM = data.UOM;
                            item.UnitofQuantity = data.UnitofQuantity;
                            item.CompanyStockCode = data.CompanyStockCode;
                            item.ColourImage = data.ColourImage;
                            item.ColourName = data.ColourName;
                            if (itemmasters.IsSensitive == true && itemmasters.IsSensitiveMRP == true)
                            {
                                item.itemname = item.itemBaseName + " " + item.MRP + " MRP " + item.UnitofQuantity + " " + item.UOM;
                            }
                            else if (itemmasters.IsSensitive == true && itemmasters.IsSensitiveMRP == false)
                            {
                                item.itemname = item.itemBaseName + " " + item.UnitofQuantity + " " + item.UOM; //item display name 
                            }
                            else if (itemmasters.IsSensitive == false && itemmasters.IsSensitiveMRP == false)
                            {
                                item.itemname = item.itemBaseName;
                            }
                            else if (itemmasters.IsSensitive == false && itemmasters.IsSensitiveMRP == true)
                            {
                                item.itemname = item.itemBaseName + " " + item.MRP + " MRP";//item display name 
                            }
                            item.CreatedDate = indianTime;
                            item.UpdatedDate = indianTime;
                            item.CreatedBy = people.DisplayName;
                            if (data.mrpMediaDC != null && data.mrpMediaDC.Any())
                            {
                                item.MRPMedias = new List<MRPMedia>();
                                data.mrpMediaDC = data.mrpMediaDC.GroupBy(x => new { x.Url, x.SequenceNo }).Select(x => new MRPMediaDC
                                {
                                    SequenceNo = x.Key.SequenceNo,
                                    Url = x.Key.Url,
                                    Type = x.Max(y => y.Type)
                                }).ToList();
                                foreach (var mediaData in data.mrpMediaDC)
                                {
                                    item.MRPMedias.Add(new MRPMedia()
                                    {
                                        IsActive = true,
                                        IsDeleted = false,
                                        Type = mediaData.Type,
                                        Url = mediaData.Url,
                                        SequenceNo = mediaData.SequenceNo,
                                        CreatedDate = indianTime,
                                        CreatedBy = userid
                                    });
                                }
                            }
                            db.ItemMultiMRPDB.Add(item);
                            db.Commit();
                            if (data.ptr > 0 || data.baseScheme > 0)
                            {
                                var PTR = new SqlParameter("@ptr", data.ptr);
                                var BasicScheme = new SqlParameter("@basicScheme", data.baseScheme);
                                var itemMultiId = new SqlParameter("@itemMultiId", item.ItemMultiMRPId);
                                var uid = new SqlParameter("@Userid", userid);
                                var list = db.Database.ExecuteSqlCommand("exec AddPtrBaseScheme @ptr, @basicScheme, @itemMultiId ,@Userid", PTR, BasicScheme, itemMultiId, uid);
                            }
                            //var tt= forRemoveCache(item.ItemMultiMRPId);

                            //Task task = Task.Run(()=>forRemoveCache(item.ItemMultiMRPId));
                            var itemMultiId1 = new SqlParameter("@itemMultiId", item.ItemMultiMRPId);
                            var citylist = db.Database.SqlQuery<int>("exec getCitiesPtrBaseScheme @itemMultiId", itemMultiId1).ToList();
                            if (citylist != null && citylist.Any())
                            {
                                foreach (var cityId in citylist)
                                {
#if !DEBUG
                                    Caching.ICacheProvider _cacheProvider = new Caching.RedisCacheProvider();
                                    _cacheProvider.Remove(Caching.CacheKeyHelper.ItemSchemeCacheKey(cityId.ToString()));
#endif
                                }
                            }
                        }
                    }
                }
                else { result = "something went wrong"; }
                return result;

            }
        }
        private async Task<bool> forRemoveCache(int itemMultiMRPId)
        {
            //System.Threading.Thread.Sleep(200000);
            using (var db = new AuthContext())
            {
                var itemMultiId1 = new SqlParameter("@itemMultiId", itemMultiMRPId);
                var citylist = db.Database.SqlQuery<int>("exec getCitiesPtrBaseScheme @itemMultiId", itemMultiId1).ToList();
                if (citylist != null && citylist.Any())
                {
                    foreach (var cityId in citylist)
                    {
#if !DEBUG
                        Caching.ICacheProvider _cacheProvider = new Caching.RedisCacheProvider();
                        _cacheProvider.Remove(Caching.CacheKeyHelper.ItemSchemeCacheKey(cityId.ToString()));
#endif
                    }
                }
                return true;
            }
        }
        #endregion

        #region   get item Multi mrp List
        /// <summary>
        ///  get item Multi mrp List
        /// </summary>
        /// <param name="ItemNumber"></param>
        /// <returns></returns>
        [Authorize]
        [Route("GetItemMRP")]
        [HttpGet]
        public List<GetItemMRPbyItemNoDc> GetItemMRP(string ItemNumber)
        {
            logger.Info("Get Item multiprice  ");
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
                using (AuthContext db = new AuthContext())
                {
                    var itemNumber = new SqlParameter("@itemNumber", ItemNumber);
                    List<GetItemMRPbyItemNoDc> ItemMultiMRP = db.Database.SqlQuery<GetItemMRPbyItemNoDc>("EXEC GetItemMRPbyItemNo @itemNumber", itemNumber).ToList();
                    var itemmultimrpIds = ItemMultiMRP.Select(x => x.ItemMultiMRPId).ToList();
                    var mrpMedia = db.MRPMedias.Where(x => itemmultimrpIds.Contains(x.ItemMultiMRPId) && x.IsActive && x.IsDeleted == false).ToList();
                    if (ItemMultiMRP != null && ItemMultiMRP.Any() && mrpMedia != null && mrpMedia.Any())
                    {
                        ItemMultiMRP.ForEach(x =>
                           x.mrpMediaDC = mrpMedia.Any(y => y.ItemMultiMRPId == x.ItemMultiMRPId) ? mrpMedia.Where(y => y.ItemMultiMRPId == x.ItemMultiMRPId).Select(y => new MRPMediaDC
                           {
                               id = y.Id,
                               SequenceNo = y.SequenceNo,
                               Type = y.Type,
                               Url = y.Url,
                               isActive = y.IsActive,
                               isDeleted = y.IsDeleted
                           }).OrderByDescending(y => y.SequenceNo).ToList() : null
                           );
                    }
                    return ItemMultiMRP;
                }
            }
            catch (Exception ex)
            {
                logger.Error("Get Item multiprice " + ex.Message);
                logger.Info("Get Item multiprice ");
                return null;
            }
        }
        #endregion


        [Route("OnGR")]
        [HttpGet]
        public ItemMaster getitembyidOnGr(string id, int WarehouseId)
        {

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
                    int CompanyId = compid;
                    int idd = Convert.ToInt32(id);
                    ItemMaster Item = db.itemMasters.Where(c => c.ItemId == idd && c.WarehouseId == WarehouseId).FirstOrDefault();
                    return Item;
                }
                catch (Exception ex)
                {
                    logger.Error("Error in ItemMaster " + ex.Message);
                    logger.Info("End  ItemMaster: ");
                    return null;
                }
            }

        }

        [Route("GetItemMOQ")]
        [HttpGet]
        public List<ItemMaster> GetItemMOQ(int ItemId)
        {
            logger.Info("Get Item multiprice  ");
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
                using (AuthContext db = new AuthContext())
                {
                    ItemMaster ItemNumber = db.itemMasters.Where(x => x.ItemId == ItemId).SingleOrDefault();
                    List<ItemMaster> itemMasters = db.itemMasters.Where(x => x.Number == ItemNumber.Number && x.WarehouseId == ItemNumber.WarehouseId && x.Deleted == false && x.ItemMultiMRPId == ItemNumber.ItemMultiMRPId).ToList();

                    return itemMasters;
                }
            }
            catch (Exception ex)
            {
                logger.Error("Get Item multiprice " + ex.Message);
                logger.Info("Get Item multiprice ");
                return null;
            }
        }


        #region   Edit item Multi mrp List
        /// <summary>
        /// Edit MultiMRPData
        ///by Ashwin //09/04/2019 </summary>
        /// <param name="itemMultiMRP"></param>
        /// <returns></returns>

        [Route("PutItemMRP")]
        [HttpPut]
        public ItemMultiMRP PutItemMRP(ItemMultiMRP itemMultiMRP)
        {
            using (var db = new AuthContext())
            {
                logger.Info("Get Item multiprice  ");
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

                    var UserName = db.Peoples.Where(x => x.PeopleID == userid).FirstOrDefault();

                    if (itemMultiMRP != null)
                    {
                        //update MultiMrp
                        var itemmultipricedata = db.ItemMultiMRPDB.Where(x => x.ItemMultiMRPId == itemMultiMRP.ItemMultiMRPId).FirstOrDefault();
                        if (itemmultipricedata != null)
                        {
                            itemmultipricedata.MRP = itemMultiMRP.MRP;
                            itemmultipricedata.itemBaseName = itemMultiMRP.itemBaseName;
                            itemmultipricedata.itemname = itemMultiMRP.itemBaseName + " " + itemMultiMRP.MRP + " MRP " + itemMultiMRP.UnitofQuantity + " " + itemMultiMRP.UOM; //item display name                               
                            itemmultipricedata.UnitofQuantity = itemMultiMRP.UnitofQuantity;
                            itemmultipricedata.UOM = itemMultiMRP.UOM;
                            itemmultipricedata.UpdatedDate = indianTime;
                            //db.ItemMultiMRPDB.Attach(itemmultipricedata);
                            db.Entry(itemmultipricedata).State = EntityState.Modified;
                            db.Commit();

                            //update inn central Masters if same multimmrp id using


                            //update MultiMrp
                            var itemcentral = db.ItemMasterCentralDB.Where(x => x.ItemMultiMRPId == itemMultiMRP.ItemMultiMRPId).ToList();
                            if (itemcentral.Count > 0)
                            {
                                foreach (var mitem in itemcentral)
                                {
                                    mitem.MRP = itemMultiMRP.MRP;
                                    mitem.price = itemMultiMRP.MRP;
                                    mitem.itemBaseName = itemMultiMRP.itemBaseName;
                                    mitem.UnitofQuantity = itemMultiMRP.UnitofQuantity;
                                    mitem.UOM = itemMultiMRP.UOM;


                                    if (mitem.IsSensitive == true)
                                    {
                                        mitem.itemname = itemMultiMRP.itemBaseName + " " + itemMultiMRP.MRP + " MRP " + itemMultiMRP.UnitofQuantity + " " + itemMultiMRP.UOM; //item display name                               
                                    }
                                    else
                                    {
                                        mitem.itemname = itemMultiMRP.itemBaseName + " " + itemMultiMRP.MRP + " MRP "; //item display name                               
                                    }

                                    // mitem.itemname = itemMultiMRP.itemBaseName + " " + itemMultiMRP.MRP + " MRP " + itemMultiMRP.UnitofQuantity + " " + itemMultiMRP.UOM; //item display name                               
                                    mitem.SellingUnitName = mitem.itemname + " " + mitem.MinOrderQty + "Unit";//item selling unit name
                                    mitem.PurchaseUnitName = mitem.itemname + " " + mitem.PurchaseMinOrderQty + "Unit";//item PurchaseUnitName name

                                    mitem.UpdatedDate = indianTime;
                                    //db.ItemMasterCentralDB.Attach(mitem);
                                    db.Entry(mitem).State = EntityState.Modified;
                                    db.Commit();

                                }

                            }
                            //update itemmasters
                            var item = db.itemMasters.Where(x => x.ItemMultiMRPId == itemMultiMRP.ItemMultiMRPId && x.Number == itemmultipricedata.ItemNumber).ToList();
                            if (item != null)
                            {
                                foreach (var it in item)
                                {
                                    it.itemBaseName = itemMultiMRP.itemBaseName;

                                    if (it.IsSensitive == true)
                                    {
                                        it.itemname = itemMultiMRP.itemBaseName + " " + itemMultiMRP.MRP + " MRP " + itemMultiMRP.UnitofQuantity + " " + itemMultiMRP.UOM; //item display name                                                          
                                    }
                                    else
                                    {
                                        it.itemname = itemMultiMRP.itemBaseName + " " + itemMultiMRP.MRP + " MRP "; //item display name                                                             
                                    }


                                    //it.itemname = itemMultiMRP.itemBaseName + " " + itemMultiMRP.MRP + " MRP " + itemMultiMRP.UnitofQuantity + " " + itemMultiMRP.UOM; //item display name                               
                                    it.SellingUnitName = it.itemname + " " + it.MinOrderQty + "Unit";//item selling unit name
                                    it.PurchaseUnitName = it.itemname + " " + it.PurchaseMinOrderQty + "Unit";//item PurchaseUnitName name
                                    it.price = itemMultiMRP.MRP;
                                    it.MRP = itemMultiMRP.MRP;
                                    it.UnitofQuantity = itemMultiMRP.UnitofQuantity;
                                    it.UOM = itemMultiMRP.UOM;
                                    it.UpdatedDate = indianTime;
                                    //db.itemMasters.Attach(it);
                                    db.Entry(it).State = EntityState.Modified;
                                    db.Commit();

                                    try
                                    {

                                        ItemMasterHistory Os = new ItemMasterHistory();
                                        if (it != null)
                                        {
                                            Os.ItemId = it.ItemId;
                                            Os.Cityid = it.Cityid;
                                            Os.CityName = it.CityName;
                                            Os.Categoryid = it.Categoryid;
                                            Os.SubCategoryId = it.SubCategoryId;
                                            Os.SubsubCategoryid = it.SubsubCategoryid;
                                            Os.WarehouseId = it.WarehouseId;
                                            Os.SupplierId = it.SupplierId;
                                            Os.SUPPLIERCODES = it.SUPPLIERCODES;
                                            Os.CompanyId = it.CompanyId;
                                            Os.CategoryName = it.CategoryName;
                                            Os.BaseCategoryid = it.BaseCategoryid;
                                            Os.BaseCategoryName = it.BaseCategoryName;
                                            Os.SubcategoryName = it.SubcategoryName;
                                            Os.SubsubcategoryName = it.SubsubcategoryName;
                                            Os.SupplierName = it.SupplierName;
                                            Os.itemname = it.itemname;
                                            Os.itemcode = it.itemcode;
                                            Os.SellingUnitName = it.SellingUnitName;
                                            Os.PurchaseUnitName = it.PurchaseUnitName;
                                            Os.price = it.price;
                                            Os.VATTax = it.VATTax;
                                            Os.active = it.active;
                                            Os.LogoUrl = it.LogoUrl;
                                            Os.CatLogoUrl = it.CatLogoUrl;
                                            Os.MinOrderQty = it.MinOrderQty;
                                            Os.PurchaseMinOrderQty = it.PurchaseMinOrderQty;
                                            Os.GruopID = it.GruopID;
                                            Os.TGrpName = it.TGrpName;
                                            Os.CessGrpID = it.GruopID;
                                            Os.CessGrpName = it.TGrpName;
                                            Os.TotalCessPercentage = it.TotalCessPercentage;
                                            Os.Discount = it.Discount;
                                            Os.UnitPrice = it.UnitPrice;
                                            Os.Number = it.Number;
                                            Os.PurchaseSku = it.PurchaseSku;
                                            Os.SellingSku = it.SellingSku;
                                            Os.PurchasePrice = it.PurchasePrice;
                                            Os.GeneralPrice = it.GeneralPrice;
                                            Os.title = it.title;
                                            Os.Description = it.Description;
                                            Os.StartDate = it.StartDate;
                                            Os.EndDate = it.EndDate;
                                            Os.PramotionalDiscount = it.PramotionalDiscount;
                                            Os.TotalTaxPercentage = it.TotalTaxPercentage;
                                            Os.WarehouseName = it.WarehouseName;
                                            Os.CreatedDate = indianTime;
                                            Os.UpdatedDate = it.UpdatedDate;
                                            Os.Deleted = it.Deleted;
                                            Os.IsDailyEssential = it.IsDailyEssential;
                                            Os.SellingPrice = it.UnitPrice;
                                            Os.StoringItemName = it.StoringItemName;
                                            Os.SizePerUnit = it.SizePerUnit;
                                            Os.HindiName = it.HindiName;
                                            Os.promoPoint = it.promoPoint;
                                            Os.marginPoint = it.marginPoint;

                                            if (UserName.DisplayName != null) { Os.UserName = UserName.DisplayName; } else { Os.UserName = UserName.PeopleFirstName; }


                                            Os.userid = userid;
                                            //--anu--//
                                            Os.DefaultBaseMargin = it.DefaultBaseMargin;//
                                            Os.ShowMrp = it.ShowMrp;//MRP Price select Checkbox
                                            Os.ShowUnit = it.ShowUnit;//Min order qty select Checkbox
                                            Os.UOM = it.UOM; //Unit of Masurement like GM Kg 
                                            Os.ShowUOM = it.ShowUOM;// Unit Of Masurement select Checkbox
                                            Os.ShowType = it.ShowType;// fast slow non movinng
                                            Os.itemBaseName = it.itemBaseName;
                                            Os.ShowTypes = it.ShowTypes;// fast slow non moving
                                            Os.Reason = it.Reason;// MRP Issue Stock Unavailable  Price Issue Other
                                            db.ItemMasterHistoryDb.Add(Os);
                                            db.Commit();
                                        }
                                    }
                                    catch (Exception ss) { }

                                }
                                //update stock name
                                var stock = db.DbCurrentStock.Where(x => x.ItemMultiMRPId == itemMultiMRP.ItemMultiMRPId && x.ItemNumber == itemmultipricedata.ItemNumber).ToList();
                                if (stock != null)
                                {
                                    foreach (var cntstock in stock)
                                    {
                                        cntstock.itemBaseName = itemmultipricedata.itemBaseName;
                                        cntstock.itemname = itemmultipricedata.itemname;
                                        cntstock.MRP = itemMultiMRP.MRP;
                                        cntstock.UnitofQuantity = itemMultiMRP.UnitofQuantity;
                                        cntstock.UOM = itemMultiMRP.UOM;
                                        //db.DbCurrentStock.Attach(cntstock);
                                        db.Entry(cntstock).State = EntityState.Modified;
                                        db.Commit();

                                        try
                                        {
                                            CurrentStockHistory Oss = new CurrentStockHistory();

                                            Oss.StockId = cntstock.StockId;
                                            Oss.ItemNumber = cntstock.ItemNumber;
                                            Oss.itemname = itemmultipricedata.itemname;
                                            Oss.TotalInventory = cntstock.CurrentInventory;
                                            Oss.WarehouseName = cntstock.WarehouseName;
                                            Oss.Warehouseid = cntstock.WarehouseId;
                                            Oss.CompanyId = cntstock.CompanyId;
                                            Oss.CreationDate = indianTime;
                                            Oss.MRP = itemmultipricedata.MRP;
                                            Oss.UnitofQuantity = itemmultipricedata.UnitofQuantity;
                                            Oss.UOM = itemmultipricedata.UOM;
                                            Oss.itemBaseName = itemmultipricedata.itemBaseName;
                                            Oss.ManualReason = "Change In MultiMrp";
                                            if (UserName.DisplayName != null)
                                            {
                                                Oss.UserName = UserName.DisplayName;
                                                Oss.ManualReason = "Change In MultiMrp :" + UserName.DisplayName + " UserId:" + userid;

                                            }
                                            else
                                            {
                                                Oss.UserName = UserName.PeopleFirstName;
                                                Oss.ManualReason = "Change In MultiMrp :" + UserName.DisplayName + " UserId:" + userid;

                                            }
                                            Oss.userid = userid;
                                            db.CurrentStockHistoryDb.Add(Oss);
                                            int idd = db.Commit();

                                        }
                                        catch (Exception ex)
                                        {
                                        }


                                    }
                                }
                            }

                        }

                    }
                    logger.Info("Get Item multiprice ");
                    return itemMultiMRP;
                }
                catch (Exception ex)
                {
                    logger.Error("Get Item multiprice " + ex.Message);
                    logger.Info("Get Item multiprice ");
                    return null;
                }
            }
        }
        #endregion

        [Route("ItemDetail")]
        [HttpGet]
        public HttpResponseMessage ItemDetail(int itemId, int customerId)
        {
            WRSITEM item = new WRSITEM();
            try
            {

                logger.Info("favourite ID : {0} , Company Id : {1}");
                DateTime CurrentDate = DateTime.Now;

                try
                {
                    using (AuthContext db = new AuthContext())
                    {
                        var ActiveCustomer = db.Customers.FirstOrDefault(x => x.CustomerId == customerId);
                        var inActiveCustomer = ActiveCustomer != null && ActiveCustomer.Active == false && ActiveCustomer.Deleted == true ? true : false;
                        var warehouseId = ActiveCustomer != null ? ActiveCustomer.Warehouseid : 0;

                        string sqlquery = "SELECT a.[FlashDealId] AS[FlashDealId], a.[ItemId] AS[ItemId] FROM[dbo].[FlashDealItemConsumeds] AS a inner join AppHomeSectionItems c on a.FlashDealId = c.sectionItemId inner join dbo.AppHomeSections b on b.SectionID = c.apphomesections_SectionID  and b.[Deleted]= 0 and b.Active=1 and b.WarehouseID=" + warehouseId +
                                     " WHERE a.[CustomerId]=" + customerId;
                        var FlashDealWithItemIds = db.Database.SqlQuery<FlashDealWithItem>(sqlquery).ToList();

                        var newdata = (from a in db.itemMasters
                                       where (a.Deleted == false && a.active == true && a.ItemId == itemId)
                                       join b in db.ItemMasterCentralDB on a.SellingSku equals b.SellingSku
                                       select new factoryItemdata
                                       {
                                           WarehouseId = a.WarehouseId,
                                           CompanyId = a.CompanyId,
                                           Categoryid = b.Categoryid,
                                           Discount = b.Discount,
                                           ItemId = a.ItemId,
                                           ItemNumber = b.Number,
                                           itemname = b.itemname,
                                           LogoUrl = b.LogoUrl,
                                           MinOrderQty = b.MinOrderQty,
                                           price = a.price,
                                           SubCategoryId = b.SubCategoryId,
                                           SubsubCategoryid = b.SubsubCategoryid,
                                           TotalTaxPercentage = b.TotalTaxPercentage,
                                           SellingUnitName = b.SellingUnitName,
                                           SellingSku = b.SellingSku,
                                           UnitPrice = a.UnitPrice,
                                           HindiName = b.HindiName,
                                           VATTax = b.VATTax,
                                           active = a.active,
                                           marginPoint = a.marginPoint,
                                           promoPerItems = a.promoPerItems,
                                           NetPurchasePrice = a.NetPurchasePrice,
                                           IsOffer = a.IsOffer,
                                           Deleted = a.Deleted,
                                           ItemMultiMRPId = a.ItemMultiMRPId,
                                           BillLimitQty = a.BillLimitQty
                                       }).OrderByDescending(x => x.ItemNumber).ToList();

                        foreach (var it in newdata)
                        {
                            //Condition for offer end.
                            if (!inActiveCustomer)
                            {
                                if (!(it.OfferStartTime <= DateTime.Now && it.OfferEndTime >= DateTime.Now))
                                {
                                    if (it.OfferCategory == 2)
                                    {
                                        it.IsOffer = false;
                                        it.FlashDealSpecialPrice = 0;
                                        it.OfferCategory = 0;
                                    }
                                    else if (it.OfferCategory == 1)
                                    {
                                        it.IsOffer = false;
                                        it.OfferCategory = 0;
                                    }

                                }
                                else if ((it.OfferStartTime <= DateTime.Now && it.OfferEndTime >= DateTime.Now) && it.OfferCategory == 2)
                                {
                                    if (FlashDealWithItemIds != null && FlashDealWithItemIds.Any(x => x.ItemId == it.ItemId))
                                    {
                                        it.IsOffer = false;
                                        it.FlashDealSpecialPrice = 0;
                                        it.OfferCategory = 0;
                                    }
                                }
                            }
                            else
                            {
                                it.IsOffer = false;
                                it.FlashDealSpecialPrice = 0;
                                it.OfferCategory = 0;
                            }

                            if (item.ItemMasters == null)
                            {
                                item.ItemMasters = new List<factoryItemdata>();
                            }

                            try
                            {
                                /// Dream Point Logic && Margin Point
                                int? MP, PP;
                                double xPoint = xPointValue * 10;
                                //Customer (0.2 * 10=1)
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
                    }
                }
                catch (Exception ex)
                {
                    logger.Error(ex.Message);
                }

                return Request.CreateResponse(HttpStatusCode.OK, item);
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex.Message);
            }
        }




        #region for PutLimit By Anushka 05/06/2019
        /// <summary>
        ///  AddLimit By Anushka 05/06/2019
        /// </summary>
        /// <param name="ItemLimitMaster"></param>
        /// <returns></returns>
        [Route("PutItemLimit")]
        [HttpPut]
        public async Task<ResponseMsg> AddItemLimit(ItemLimitMaster AddLimitData)
        {
            var result = new ResponseMsg();
            result.Status = false;
            var identity = User.Identity as ClaimsIdentity;
            int userid = 0;

            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

            //string result = "";
            if (AddLimitData != null && AddLimitData.ItemMultiMRPId > 0 && AddLimitData.WarehouseId > 0)
            {
                using (var db = new AuthContext())
                {
                    if (userid > 0)
                    {
                        var people = db.Peoples.Where(x => x.PeopleID == userid).FirstOrDefault();

                        if (db.Warehouses.Any(x => x.WarehouseId == AddLimitData.WarehouseId && x.IsJITEnabled))
                        {
                            var param = new SqlParameter
                            {
                                ParameterName = "WarehouseId",
                                Value = AddLimitData.WarehouseId
                            };

                            var param1 = new SqlParameter
                            {
                                ParameterName = "itemMultimrpid",
                                Value = AddLimitData.ItemMultiMRPId
                            };
                            var param2 = new SqlParameter
                            {
                                ParameterName = "IsConfig",
                                Value = true
                            };

                            int? netqty = db.Database.SqlQuery<int?>("JIT.GetNetStock @WarehouseId,@itemMultimrpid,@IsConfig", param, param1, param2).FirstOrDefault();
                            if (netqty < AddLimitData.ItemlimitQty)
                            {
                                result.Status = false;
                                result.Message = "Your current allowed qty is :" + netqty + " if you want to increase qty then add Additional qty in Riskqty with risk purchase price. ";
                                return result;
                            }
                        }

                        var itemLimit = db.ItemLimitMasterDB.Where(x => x.ItemMultiMRPId == AddLimitData.ItemMultiMRPId && x.WarehouseId == AddLimitData.WarehouseId).FirstOrDefault();
                        //var jitRisk = db.JITRiskQtys.Where(x => x.ItemMultiMrpId == itemLimit.ItemMultiMRPId && x.WarehouseId == itemLimit.WarehouseId && x.IsActive && x.IsDeleted == false).FirstOrDefault();
                        if (itemLimit != null)// && jitRisk!= null
                        {
                            //if(jitRisk.RiskQuantity)
                            itemLimit.ItemlimitQty = AddLimitData.ItemlimitQty;
                            itemLimit.IsItemLimit = AddLimitData.IsItemLimit;
                            itemLimit.UpdateDate = indianTime;
                            itemLimit.ModifyBy = people.DisplayName;
                            db.Entry(itemLimit).State = EntityState.Modified;
                        }
                        else
                        {
                            AddLimitData.CreatedDate = indianTime;
                            AddLimitData.UpdateDate = indianTime;
                            db.ItemLimitMasterDB.Add(AddLimitData);
                        }
                    }
                    if (db.Commit() > 0)
                    {
                        result.Status = true;
                        result.Message = "Limit updated successfully.";
                    }
                }
            }
            else
            {
                result.Status = false;
                result.Message = "Something went wrong.";
            }
            return result;
        }
        #endregion
        #region for  Add Bill Limit qty By sudhir 05/12/2019
        /// <summary>
        ///  Add Bill Limit qty  By sudhir 05/12/2019
        /// </summary>
        /// <param name="ItemBillLimitMaster"></param>
        /// <returns></returns>
        [Route("AddBillItemLimit")]
        [HttpPut]
        public async Task<bool> AddBillItemLimit(BillItemLimitMaster BillitemLimitData)
        {
            bool result = false;
            using (var db = new AuthContext())
            {
                var BillitemLimit = db.itemMasters.Where(x => x.Number == BillitemLimitData.Number && x.WarehouseId == BillitemLimitData.WarehouseId).ToList();
                if (BillitemLimit != null && BillitemLimit.Any())
                {
                    foreach (var item in BillitemLimit)
                    {
                        item.BillLimitQty = BillitemLimitData.itemBillLimitQty;
                        db.Entry(item).State = EntityState.Modified;
                    }
                    result = db.Commit() > 0;
                }
            }
            return result;
        }
        #endregion

        #region for GetLimit By Anushka 05/06/2019
        /// <summary>
        ///  GetLimit By Anushka 05/06/2019
        /// </summary>
        /// <param name="ItemLimitMaster"></param>
        /// <returns></returns>
        [Route("GetItemLimit")]
        [HttpGet]
        public ItemLimitMaster GetItemLimit(string itemNumber, int wareHouseid, int multiMrpId)
        {
            logger.Info("start Addlimit : ");
            //string result = "success";
            using (var myContext = new AuthContext())
            {
                myContext.Database.Log = s => Debug.WriteLine(s);
                var itemLimit = myContext.ItemLimitMasterDB.FirstOrDefault(x => x.ItemMultiMRPId == multiMrpId && x.WarehouseId == wareHouseid && x.ItemNumber == itemNumber);
                return itemLimit;
            }

        }
        #endregion
        #region for GetItemBillLimit By sudhir 11/12/2019
        /// <summary>
        ///  GetItemBillLimit By sudhir 11/12/2019
        /// </summary>
        /// <param name="GetItemBillLimit"></param>
        /// <returns></returns>
        [Route("GetItemBillLimit")]
        [HttpGet]
        public dynamic GetItemBillLimit(int itemId)
        {
            using (var myContext = new AuthContext())
            {
                myContext.Database.Log = s => Debug.WriteLine(s);

                var itemBillLimit = myContext.itemMasters.Where(x => x.ItemId == itemId).FirstOrDefault();
                return itemBillLimit;
            }

        }
        #endregion

        [Route("ItemDetail")]
        [HttpPost]

        public HttpResponseMessage ItemDetail(int itemId)
        {
            WRSITEM item = new WRSITEM();
            try
            {

                logger.Info("favourite ID : {0} , Company Id : {1}");

                try
                {
                    using (AuthContext db = new AuthContext())
                    {
                        var newdata = (from a in db.itemMasters
                                       where (a.Deleted == false && a.active == true && a.ItemId == itemId)
                                       join b in db.ItemMasterCentralDB on a.SellingSku equals b.SellingSku
                                       select new factoryItemdata
                                       {
                                           WarehouseId = a.WarehouseId,
                                           CompanyId = a.CompanyId,
                                           Categoryid = b.Categoryid,
                                           Discount = b.Discount,
                                           ItemId = a.ItemId,
                                           ItemNumber = b.Number,
                                           itemname = b.itemname,
                                           LogoUrl = b.LogoUrl,
                                           MinOrderQty = b.MinOrderQty,
                                           price = a.price,
                                           SubCategoryId = b.SubCategoryId,
                                           SubsubCategoryid = b.SubsubCategoryid,
                                           TotalTaxPercentage = b.TotalTaxPercentage,
                                           SellingUnitName = b.SellingUnitName,
                                           SellingSku = b.SellingSku,
                                           UnitPrice = a.UnitPrice,
                                           HindiName = b.HindiName,
                                           VATTax = b.VATTax,
                                           active = a.active,
                                           marginPoint = a.marginPoint,
                                           promoPerItems = a.promoPerItems,
                                           NetPurchasePrice = a.NetPurchasePrice,
                                           IsOffer = a.IsOffer,
                                           Deleted = a.Deleted,
                                           ItemMultiMRPId = a.ItemMultiMRPId,
                                           BillLimitQty = a.BillLimitQty
                                       }).OrderByDescending(x => x.ItemNumber).ToList();
                        foreach (var it in newdata)
                        {
                            if (item == null)
                            {
                                item.ItemMasters = new List<factoryItemdata>();
                            }
                            try
                            {
                                /// Dream Point Logic && Margin Point
                                int? MP, PP;
                                double xPoint = xPointValue * 10;
                                //Customer (0.2 * 10=1)
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
                    }
                }
                catch (Exception ex)
                {
                    logger.Error(ex.Message);
                }

                return Request.CreateResponse(HttpStatusCode.OK, item);
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex.Message);
            }
        }
        [Route("ItemMasterDeactivatedList")]
        [HttpGet]

        public dynamic ItemMasterDeactivatedList(int warehouseId)
        {
            using (var db = new AuthContext())
            {

                List<ItemListDeactivated> lstd = new List<ItemListDeactivated>();

                try
                {
                    var Itemlist = db.itemMasters.Where(x => x.WarehouseId == warehouseId).GroupBy(x => new { x.ItemMultiMRPId, x.Number, x.WarehouseName }).Where(x => x.All(y => !y.active))
                        .Select(x => new ItemListDeactivated
                        {
                            ItemMultiMRPId = x.Key.ItemMultiMRPId,
                            itemnumber = x.Key.Number,
                            warehousename = x.Key.Number,
                            ItemListDeactivatedDetails = x.Select(y => new ItemListDeactivatedDetail
                            {
                                CityName = y.CityName,
                                Cityid = y.Cityid,
                                CategoryName = y.CategoryName,
                                CategoryCode = y.Categoryid,
                                SubcategoryName = y.SubcategoryName,
                                SubsubcategoryName = y.SubsubcategoryName,
                                itemname = y.itemname,
                                itemcode = y.itemcode,
                                ItemMultiMRPId = y.ItemMultiMRPId,
                                Number = y.Number,
                                SellingSku = y.SellingSku,
                                price = y.price,
                                PurchasePrice = y.PurchasePrice,
                                UnitPrice = y.UnitPrice,
                                MinOrderQty = y.MinOrderQty,
                                SellingUnitName = y.SellingUnitName,
                                PurchaseMinOrderQty = y.PurchaseMinOrderQty,
                                StoringItemName = y.StoringItemName,
                                PurchaseSku = y.PurchaseSku,
                                PurchaseUnitName = y.PurchaseUnitName,
                                SupplierName = y.SupplierName,
                                SUPPLIERCODES = y.SUPPLIERCODES,
                                BaseCategoryName = y.BaseCategoryName,
                                TGrpName = y.TGrpName,
                                TotalTaxPercentage = y.TotalTaxPercentage,
                                WarehouseName = y.WarehouseName,
                                HindiName = y.HindiName,
                                SizePerUnit = y.SizePerUnit,
                                Active = y.active,
                                Deleted = y.Deleted,
                                Margin = y.Margin,
                                PromoPoint = y.promoPoint,
                                HSNCode = y.HSNCode,
                                IsSensitive = y.IsSensitive,


                            }).ToList()
                        }).ToList();
                    return Itemlist.SelectMany(x => x.ItemListDeactivatedDetails);

                }
                catch (Exception ex)
                {

                    return null;
                }
            }
        }


        [Route("ItemMasterDeactivatedList10")]
        [HttpGet]

        public dynamic ItemMasterDeactivatedList10(int warehouseId)
        {
            using (var db = new AuthContext())
            {

                List<ItemListDeactivated> lstd = new List<ItemListDeactivated>();
                DateTime dtOld = new DateTime();
                dtOld = DateTime.Now.AddDays(-10);

                try
                {
                    var Itemlist = db.itemMasters.Where(x => x.WarehouseId == warehouseId && x.CreatedDate > dtOld).GroupBy(x => new { x.ItemMultiMRPId, x.Number, x.WarehouseName }).Where(x => x.All(y => !y.active))
                        .Select(x => new ItemListDeactivated
                        {
                            ItemMultiMRPId = x.Key.ItemMultiMRPId,
                            itemnumber = x.Key.Number,
                            warehousename = x.Key.Number,
                            ItemListDeactivatedDetails = x.Select(y => new ItemListDeactivatedDetail
                            {
                                CityName = y.CityName,
                                Cityid = y.Cityid,
                                CategoryName = y.CategoryName,
                                CategoryCode = y.Categoryid,
                                SubcategoryName = y.SubcategoryName,
                                SubsubcategoryName = y.SubsubcategoryName,
                                itemname = y.itemname,
                                itemcode = y.itemcode,
                                ItemMultiMRPId = y.ItemMultiMRPId,
                                Number = y.Number,
                                SellingSku = y.SellingSku,
                                price = y.price,
                                PurchasePrice = y.PurchasePrice,
                                UnitPrice = y.UnitPrice,
                                MinOrderQty = y.MinOrderQty,
                                SellingUnitName = y.SellingUnitName,
                                PurchaseMinOrderQty = y.PurchaseMinOrderQty,
                                StoringItemName = y.StoringItemName,
                                PurchaseSku = y.PurchaseSku,
                                PurchaseUnitName = y.PurchaseUnitName,
                                SupplierName = y.SupplierName,
                                SUPPLIERCODES = y.SUPPLIERCODES,
                                BaseCategoryName = y.BaseCategoryName,
                                TGrpName = y.TGrpName,
                                TotalTaxPercentage = y.TotalTaxPercentage,
                                WarehouseName = y.WarehouseName,
                                HindiName = y.HindiName,
                                SizePerUnit = y.SizePerUnit,
                                Active = y.active,
                                Deleted = y.Deleted,
                                Margin = y.Margin,
                                PromoPoint = y.promoPoint,
                                HSNCode = y.HSNCode,
                                IsSensitive = y.IsSensitive


                            }).ToList()
                        }).ToList();
                    return Itemlist.SelectMany(x => x.ItemListDeactivatedDetails);

                }
                catch (Exception ex)
                {

                    return null;
                }
            }
        }

        /// <summary>
        /// Upload image of item
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        [HttpPut]
        [Route("updateItemImage")]
        [Authorize]
        public HttpResponseMessage updateimage(ItemMasterCentral ItemMasterCentral)
        {
            logger.Info("start updateItemImage");
            bool result = false;
            try
            {
                var identity = User.Identity as ClaimsIdentity;
                int compid = 0;

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))
                    compid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);

                int CompanyId = compid;
                if (ItemMasterCentral != null)
                {
                    using (AuthContext authContext = new AuthContext())
                    {
                        var UpdateitemcentralList = new List<ItemMasterCentral>();
                        var updateitemmasterList = new List<ItemMaster>();

                        List<ItemMasterCentral> ItemMasterCentralList = authContext.ItemMasterCentralDB.Where(x => x.Number == ItemMasterCentral.Number && x.Deleted == false).ToList();
                        foreach (var item in ItemMasterCentralList)
                        {
                            item.LogoUrl = ItemMasterCentral.LogoUrl;
                            item.UpdatedDate = indianTime;
                            UpdateitemcentralList.Add(item);
                        }
                        List<ItemMaster> itemMastersList = authContext.itemMasters.Where(x => x.Number == ItemMasterCentral.Number && x.Deleted == false).ToList();
                        foreach (var item in itemMastersList)
                        {
                            item.LogoUrl = ItemMasterCentral.LogoUrl;
                            item.UpdatedDate = indianTime;
                            updateitemmasterList.Add(item);
                        }
                        foreach (var itemcentral in UpdateitemcentralList)
                        {
                            authContext.Entry(itemcentral).State = EntityState.Modified;
                        }
                        foreach (var itemm in updateitemmasterList)
                        {
                            authContext.Entry(itemm).State = EntityState.Modified;
                        }

                        if (authContext.Commit() > 0)
                        {

                            result = true;
                        }
                        else
                        {
                            result = false;
                        }
                    }
                }
                return Request.CreateResponse(HttpStatusCode.OK, result);
            }
            catch (Exception ex)
            {
                logger.Error("Error in updateItemImage Master " + ex.Message);
                logger.Info("End  updateItemImage Master: ");
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Got Error");

            }
        }
        [Route("ItemMasterActivatedList")]
        [HttpGet]

        public dynamic ItemMasterActivatedList()
        {
            using (var db = new AuthContext())
            {
                List<ItemListActivated> lstd = new List<ItemListActivated>();

                var Itemlist = db.itemMasters.GroupBy(x => new { x.ItemMultiMRPId, x.Number }).Where(x => x.All(y => y.active == true))
                    .Select(x => new ItemListActivated
                    {
                        ItemMultiMRPId = x.Key.ItemMultiMRPId,
                        itemnumber = x.Key.Number,
                        warehousename = x.Key.Number,
                        ItemListActivatedDetail = x.Select(y => new ItemListActivatedDetail
                        {
                            SubsubcatId = y.SubsubCategoryid,
                            subsubcatname = y.SubsubcategoryName,
                            cateName = y.CategoryName

                        }).ToList()
                    }).ToList();
                var returnList = Itemlist.SelectMany(x => x.ItemListActivatedDetail.GroupBy(z => new { z.SubsubcatId, z.subsubcatname, z.cateName })).Select(x => new { x.Key.SubsubcatId, x.Key.subsubcatname, x.Key.cateName });

                return returnList;

            }
        }

        #region Get Buyer Name by Anushka
        /// <summary>
        /// Get Buyer Name 
        ///Created date (12-09-2019)
        /// </summary>
        /// <returns></returns>
        [Authorize]
        [Route("GetBuyer")]
        [HttpGet]
        public async Task<IEnumerable<BuyerDc>> GetBuyerAsync()
        {
            //List<People> Buyer = context.Peoples.Where(x => x.Active && !x.Deleted && (x.Department == "HQ Sourcing" || x.Department == "Sourcing" || x.Department == "HQ Sourcing Executive")).ToList();
            var manager = new PeopleManager();
            var People = await manager.GetPeopleByBuyer();
            return People;
        }
        #endregion

        [Route("GetBuyerRoleWise")]
        [HttpGet]
        public dynamic GetBuyerRoleWise()
        {
            using (var db = new AuthContext())
            {
                var query = "select distinct p.PeopleID , p.DisplayName from People p inner join AspNetUsers a on a.Email = p.Email and p.Active=1 and p.Deleted=0 inner join AspNetUserRoles ur on a.id = ur.UserId and ur.isActive=1 inner join AspNetRoles r on ur.RoleId=r.Id and r.Name like '%HQ Sourcing%' ";
                var list = db.Database.SqlQuery<BuyerDc>(query).ToList();
                return list;
            }
        }


        [HttpGet]
        [Route("GetSearchItem")]

        public HttpResponseMessage SearchCust(int warehouseId, int customerId, int skip, int take, string lang)
        {
            FilterSearchDc filterSearchDc = new FilterSearchDc();
            using (var authContext = new AuthContext())
            {
                var ActiveCustomer = authContext.Customers.FirstOrDefault(x => x.CustomerId == customerId);
                var inActiveCustomer = ActiveCustomer != null && ActiveCustomer.Active == false && ActiveCustomer.Deleted == true ? true : false;
                string sqlquery = "SELECT a.[FlashDealId] AS [FlashDealId], a.[ItemId] AS [ItemId] FROM [dbo].[FlashDealItemConsumeds] AS a inner join AppHomeSectionItems c on a.FlashDealId = c.sectionItemId inner join dbo.AppHomeSections b on b.SectionID = c.apphomesections_SectionID  and b.Active=1 and b.[Deleted]=0  and b.WarehouseID=" + warehouseId +
                                       " WHERE a.[CustomerId]=" + customerId;
                var FlashDealWithItemIds = authContext.Database.SqlQuery<FlashDealWithItem>(sqlquery).ToList();

                try
                {
                    if (authContext.Database.Connection.State != ConnectionState.Open)
                        authContext.Database.Connection.Open();

                    var taskList = new List<Task>();
                    var task1 = Task.Factory.StartNew(() =>
                    {
                        var cmd = authContext.Database.Connection.CreateCommand();
                        cmd.CommandText = "[dbo].[GetSearchItemForMostSellingProduct]";
                        cmd.CommandType = System.Data.CommandType.StoredProcedure;
                        cmd.Parameters.Add(new SqlParameter("@warehouseId", warehouseId));
                        cmd.Parameters.Add(new SqlParameter("@customerId", customerId));
                        cmd.Parameters.Add(new SqlParameter("@skip", skip));
                        cmd.Parameters.Add(new SqlParameter("@take", take));

                        // Run the sproc
                        var reader = cmd.ExecuteReader();

                        // Read Blogs from the first result set
                        var MostSellingProduct = ((IObjectContextAdapter)authContext)
                            .ObjectContext
                            .Translate<Itemsearch>(reader).ToList();

                        filterSearchDc.MostSellingProduct = MostSellingProduct;


                        foreach (var it in MostSellingProduct)
                        {
                            if (!inActiveCustomer)
                            {
                                if (!(it.OfferStartTime <= DateTime.Now && it.OfferEndTime >= DateTime.Now))
                                {
                                    if (it.OfferCategory == 2)
                                    {
                                        it.IsOffer = false;
                                        it.FlashDealSpecialPrice = 0;
                                        it.OfferCategory = 0;
                                    }
                                    else if (it.OfferCategory == 1)
                                    {
                                        it.IsOffer = false;
                                        it.OfferCategory = 0;
                                    }

                                }
                                else if ((it.OfferStartTime <= DateTime.Now && it.OfferEndTime >= DateTime.Now) && it.OfferCategory == 2)
                                {
                                    if (FlashDealWithItemIds != null && FlashDealWithItemIds.Any(x => x.ItemId == it.ItemId))
                                    {
                                        it.IsOffer = false;
                                        it.FlashDealSpecialPrice = 0;
                                        it.OfferCategory = 0;
                                    }
                                }
                            }
                            else
                            {
                                it.IsOffer = false;
                                it.FlashDealSpecialPrice = 0;
                                it.OfferCategory = 0;
                            }

                            try
                            {
                                if (!it.IsOffer)
                                {
                                    /// Dream Point Logic && Margin Point
                                    int? MP, PP;
                                    double xPoint = xPointValue * 10;

                                    //Customer (0.2 * 10=1)
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
                                        it.marginPoint = Convert.ToInt32(((it.price - it.UnitPrice) * 100) / it.UnitPrice);//MP;  we replce marginpoint value by margin for app here 
                                    }
                                    else
                                    {
                                        it.marginPoint = 0;
                                    }
                                }
                                else { it.dreamPoint = 0; }
                            }
                            catch { }

                            if (it.price > it.UnitPrice)
                            {
                                it.marginPoint = Convert.ToInt32(((it.price - it.UnitPrice) * 100) / it.UnitPrice);//MP;  we replce marginpoint value by margin for app here 
                            }
                            else
                            {
                                it.marginPoint = 0;
                            }
                            if (lang.Trim() == "hi")
                            {
                                if (it.HindiName != null)
                                {
                                    if (it.IsSensitive == true)
                                    {
                                        if (it.IsSensitiveMRP == false)
                                        {
                                            it.itemname = it.HindiName + " " + it.UnitofQuantity + " " + it.UOM; //item display name   
                                        }
                                        else
                                        {
                                            it.itemname = it.HindiName + " " + it.price + " MRP " + it.UnitofQuantity + " " + it.UOM; //item display name                               
                                        }
                                    }
                                    else
                                    {
                                        it.itemname = it.HindiName + " " + it.price + " MRP "; //item display name                               
                                    }

                                }
                                else
                                {
                                    it.itemname = it.itemBaseName + " " + it.price + " MRP ";
                                }
                            }
                        }
                    });
                    taskList.Add(task1);

                    var task2 = Task.Factory.StartNew(() =>
                    {
                        var cmd = authContext.Database.Connection.CreateCommand();
                        cmd.CommandText = "[dbo].[GetSearchItemForRecentPurchase]";
                        cmd.CommandType = System.Data.CommandType.StoredProcedure;
                        cmd.Parameters.Add(new SqlParameter("@warehouseId", warehouseId));
                        cmd.Parameters.Add(new SqlParameter("@customerId", customerId));
                        cmd.Parameters.Add(new SqlParameter("@skip", skip));
                        cmd.Parameters.Add(new SqlParameter("@take", take));

                        // Run the sproc
                        var reader = cmd.ExecuteReader();
                        var RecentPurchase = ((IObjectContextAdapter)authContext)
                            .ObjectContext
                            .Translate<Itemsearch>(reader).ToList();

                        filterSearchDc.RecentPurchase = RecentPurchase;

                        foreach (var it in RecentPurchase)
                        {
                            if (!inActiveCustomer)
                            {
                                if (!(it.OfferStartTime <= DateTime.Now && it.OfferEndTime >= DateTime.Now))
                                {
                                    if (it.OfferCategory == 2)
                                    {
                                        it.IsOffer = false;
                                        it.FlashDealSpecialPrice = 0;
                                        it.OfferCategory = 0;
                                    }
                                    else if (it.OfferCategory == 1)
                                    {
                                        it.IsOffer = false;
                                        it.OfferCategory = 0;
                                    }

                                }
                                else if ((it.OfferStartTime <= DateTime.Now && it.OfferEndTime >= DateTime.Now) && it.OfferCategory == 2)
                                {
                                    if (FlashDealWithItemIds != null && FlashDealWithItemIds.Any(x => x.ItemId == it.ItemId))
                                    {
                                        it.IsOffer = false;
                                        it.FlashDealSpecialPrice = 0;
                                        it.OfferCategory = 0;
                                    }
                                }
                            }
                            else
                            {
                                it.IsOffer = false;
                                it.FlashDealSpecialPrice = 0;
                                it.OfferCategory = 0;
                            }

                            try
                            {
                                if (!it.IsOffer)
                                {
                                    /// Dream Point Logic && Margin Point
                                    int? MP, PP;
                                    double xPoint = xPointValue * 10;

                                    //Customer (0.2 * 10=1)
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
                                        it.marginPoint = Convert.ToInt32(((it.price - it.UnitPrice) * 100) / it.UnitPrice);//MP;  we replce marginpoint value by margin for app here 
                                    }
                                    else
                                    {
                                        it.marginPoint = 0;
                                    }
                                }
                                else { it.dreamPoint = 0; }
                            }
                            catch { }

                            if (it.price > it.UnitPrice)
                            {
                                it.marginPoint = Convert.ToInt32(((it.price - it.UnitPrice) * 100) / it.UnitPrice);//MP;  we replce marginpoint value by margin for app here 
                            }
                            else
                            {
                                it.marginPoint = 0;
                            }

                            if (lang.Trim() == "hi")
                            {
                                if (it.HindiName != null)
                                {
                                    if (it.IsSensitive == true)
                                    {
                                        if (it.IsSensitiveMRP == false)
                                        {
                                            it.itemname = it.HindiName + " " + it.UnitofQuantity + " " + it.UOM; //item display name   
                                        }
                                        else
                                        {
                                            it.itemname = it.HindiName + " " + it.price + " MRP " + it.UnitofQuantity + " " + it.UOM; //item display name                               
                                        }
                                    }
                                    else
                                    {
                                        it.itemname = it.HindiName + " " + it.price + " MRP "; //item display name                               
                                    }

                                }
                                else
                                {
                                    it.itemname = it.itemBaseName + " " + it.price + " MRP ";
                                }
                            }
                        }
                    });
                    taskList.Add(task2);

                    var task3 = Task.Factory.StartNew(() =>
                    {
                        var cmd = authContext.Database.Connection.CreateCommand();
                        cmd.CommandText = "[dbo].[GetSearchItemForCustFavoriteItem]";
                        cmd.CommandType = System.Data.CommandType.StoredProcedure;
                        cmd.Parameters.Add(new SqlParameter("@warehouseId", warehouseId));
                        cmd.Parameters.Add(new SqlParameter("@customerId", customerId));
                        cmd.Parameters.Add(new SqlParameter("@skip", skip));
                        cmd.Parameters.Add(new SqlParameter("@take", take));

                        // Run the sproc
                        var reader = cmd.ExecuteReader();
                        var CustFavoriteItem = ((IObjectContextAdapter)authContext)
                        .ObjectContext
                        .Translate<Itemsearch>(reader).ToList();

                        filterSearchDc.CustFavoriteItem = CustFavoriteItem;

                        foreach (var it in CustFavoriteItem)
                        {
                            if (!inActiveCustomer)
                            {
                                if (!(it.OfferStartTime <= DateTime.Now && it.OfferEndTime >= DateTime.Now))
                                {
                                    if (it.OfferCategory == 2)
                                    {
                                        it.IsOffer = false;
                                        it.FlashDealSpecialPrice = 0;
                                        it.OfferCategory = 0;
                                    }
                                    else if (it.OfferCategory == 1)
                                    {
                                        it.IsOffer = false;
                                        it.OfferCategory = 0;
                                    }

                                }
                                else if ((it.OfferStartTime <= DateTime.Now && it.OfferEndTime >= DateTime.Now) && it.OfferCategory == 2)
                                {
                                    if (FlashDealWithItemIds != null && FlashDealWithItemIds.Any(x => x.ItemId == it.ItemId))
                                    {
                                        it.IsOffer = false;
                                        it.FlashDealSpecialPrice = 0;
                                        it.OfferCategory = 0;
                                    }
                                }
                            }
                            else
                            {
                                it.IsOffer = false;
                                it.FlashDealSpecialPrice = 0;
                                it.OfferCategory = 0;
                            }

                            try
                            {
                                if (!it.IsOffer)
                                {
                                    /// Dream Point Logic && Margin Point
                                    int? MP, PP;
                                    double xPoint = xPointValue * 10;

                                    //Customer (0.2 * 10=1)
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
                                        it.marginPoint = Convert.ToInt32(((it.price - it.UnitPrice) * 100) / it.UnitPrice);//MP;  we replce marginpoint value by margin for app here 
                                    }
                                    else
                                    {
                                        it.marginPoint = 0;
                                    }
                                }
                                else { it.dreamPoint = 0; }
                            }
                            catch { }

                            if (it.price > it.UnitPrice)
                            {
                                it.marginPoint = Convert.ToInt32(((it.price - it.UnitPrice) * 100) / it.UnitPrice);//MP;  we replce marginpoint value by margin for app here 
                            }
                            else
                            {
                                it.marginPoint = 0;
                            }

                            if (lang.Trim() == "hi")
                            {
                                if (it.HindiName != null)
                                {
                                    if (it.IsSensitive == true)
                                    {
                                        if (it.IsSensitiveMRP == false)
                                        {
                                            it.itemname = it.HindiName + " " + it.UnitofQuantity + " " + it.UOM; //item display name   
                                        }
                                        else
                                        {
                                            it.itemname = it.HindiName + " " + it.price + " MRP " + it.UnitofQuantity + " " + it.UOM; //item display name                               
                                        }
                                    }
                                    else
                                    {
                                        it.itemname = it.HindiName + " " + it.price + " MRP "; //item display name                               
                                    }

                                }
                                else
                                {
                                    it.itemname = it.itemBaseName + " " + it.price + " MRP ";
                                }
                            }
                        }

                    });
                    taskList.Add(task3);

                    var task4 = Task.Factory.StartNew(() =>
                    {
                        MongoDbHelper<CustomerProductSearch> mongoDbHelper = new MongoDbHelper<CustomerProductSearch>();
                        List<int> itemIds = mongoDbHelper.Select(x => x.customerId == customerId && !x.IsDeleted, x => x.OrderByDescending(y => y.CreatedDate), skip, take).ToList().SelectMany(x => x.Items).Distinct().ToList();
                        var orderIdDt = new DataTable();
                        orderIdDt.Columns.Add("IntValue");
                        foreach (var item in itemIds)
                        {
                            var dr = orderIdDt.NewRow();
                            dr["IntValue"] = item;
                            orderIdDt.Rows.Add(dr);
                        }

                        var param = new SqlParameter("recentSearchItemIds", orderIdDt);
                        param.SqlDbType = SqlDbType.Structured;
                        param.TypeName = "dbo.IntValues";
                        var cmd = authContext.Database.Connection.CreateCommand();
                        cmd.CommandText = "[dbo].[GetSearchItemForRecentSearch]";
                        cmd.CommandType = System.Data.CommandType.StoredProcedure;
                        cmd.Parameters.Add(new SqlParameter("@warehouseId", warehouseId));
                        cmd.Parameters.Add(new SqlParameter("@customerId", customerId));
                        cmd.Parameters.Add(new SqlParameter("@skip", skip));
                        cmd.Parameters.Add(new SqlParameter("@take", take));
                        cmd.Parameters.Add(param);

                        // Run the sproc
                        var reader1 = cmd.ExecuteReader();
                        var recentSearchItem = ((IObjectContextAdapter)authContext)
                        .ObjectContext
                        .Translate<Itemsearch>(reader1).ToList();

                        filterSearchDc.RecentSearchItem = recentSearchItem;

                        foreach (var it in filterSearchDc.RecentSearchItem)
                        {
                            if (!inActiveCustomer)
                            {
                                if (!(it.OfferStartTime <= DateTime.Now && it.OfferEndTime >= DateTime.Now))
                                {
                                    if (it.OfferCategory == 2)
                                    {
                                        it.IsOffer = false;
                                        it.FlashDealSpecialPrice = 0;
                                        it.OfferCategory = 0;
                                    }
                                    else if (it.OfferCategory == 1)
                                    {
                                        it.IsOffer = false;
                                        it.OfferCategory = 0;
                                    }

                                }
                                else if ((it.OfferStartTime <= DateTime.Now && it.OfferEndTime >= DateTime.Now) && it.OfferCategory == 2)
                                {
                                    if (FlashDealWithItemIds != null && FlashDealWithItemIds.Any(x => x.ItemId == it.ItemId))
                                    {
                                        it.IsOffer = false;
                                        it.FlashDealSpecialPrice = 0;
                                        it.OfferCategory = 0;
                                    }
                                }
                            }
                            else
                            {
                                it.IsOffer = false;
                                it.FlashDealSpecialPrice = 0;
                                it.OfferCategory = 0;
                            }

                            try
                            {
                                if (!it.IsOffer)
                                {
                                    /// Dream Point Logic && Margin Point
                                    int? MP, PP;
                                    double xPoint = xPointValue * 10;

                                    //Customer (0.2 * 10=1)
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
                                        it.marginPoint = Convert.ToInt32(((it.price - it.UnitPrice) * 100) / it.UnitPrice);//MP;  we replce marginpoint value by margin for app here 
                                    }
                                    else
                                    {
                                        it.marginPoint = 0;
                                    }
                                }
                                else { it.dreamPoint = 0; }
                            }
                            catch { }

                            if (it.price > it.UnitPrice)
                            {
                                it.marginPoint = Convert.ToInt32(((it.price - it.UnitPrice) * 100) / it.UnitPrice);//MP;  we replce marginpoint value by margin for app here 
                            }
                            else
                            {
                                it.marginPoint = 0;
                            }

                            if (lang.Trim() == "hi")
                            {
                                if (it.HindiName != null)
                                {
                                    if (it.IsSensitive == true)
                                    {
                                        if (it.IsSensitiveMRP == false)
                                        {
                                            it.itemname = it.HindiName + " " + it.UnitofQuantity + " " + it.UOM; //item display name   
                                        }
                                        else
                                        {
                                            it.itemname = it.HindiName + " " + it.price + " MRP " + it.UnitofQuantity + " " + it.UOM; //item display name                               
                                        }
                                    }
                                    else
                                    {
                                        it.itemname = it.HindiName + " " + it.price + " MRP "; //item display name                               
                                    }

                                }
                                else
                                {
                                    it.itemname = it.itemBaseName + " " + it.price + " MRP ";
                                }
                            }
                        }

                    });
                    taskList.Add(task4);

                    Task.WaitAll(taskList.ToArray());

                }
                finally
                {
                    authContext.Database.Connection.Close();
                }
            }
            return Request.CreateResponse(HttpStatusCode.OK, filterSearchDc);
        }

        [HttpGet]
        [Route("DeleteCustomerSearchItem")]
        public HttpResponseMessage DeleteCustomerSearchItem(int customerId, string keyword)
        {
            MongoDbHelper<CustomerProductSearch> mongoDbHelper = new MongoDbHelper<CustomerProductSearch>();
            var customerProductSearchs = mongoDbHelper.Select(x => x.customerId == customerId && x.keyword == keyword).ToList();
            if (customerProductSearchs != null && customerProductSearchs.Any())
            {
                customerProductSearchs.ForEach(x =>
                {
                    x.IsDeleted = true;
                    mongoDbHelper.Replace(x.Id, x);
                }
                );
            }

            return Request.CreateResponse(HttpStatusCode.OK, true);

        }

        [HttpPost]
        [Route("GetRelatedItem")]

        public HttpResponseMessage GetRelatedItem(RelatedItem ri)
        {
            var ItemSearch = new List<Itemsearch>();
            using (var authContext = new AuthContext())
            {
                var ActiveCustomer = authContext.Customers.FirstOrDefault(x => x.CustomerId == ri.customerId);
                var inActiveCustomer = ActiveCustomer != null && ActiveCustomer.Active == false && ActiveCustomer.Deleted == true ? true : false;
                string sqlquery = "SELECT a.[FlashDealId] AS [FlashDealId], a.[ItemId] AS [ItemId] FROM [dbo].[FlashDealItemConsumeds] AS a inner join AppHomeSectionItems c on a.FlashDealId = c.sectionItemId inner join dbo.AppHomeSections b on b.SectionID = c.apphomesections_SectionID  and b.Active=1 and b.[Deleted]=0  and b.WarehouseID=" + ri.warehouseId +
                                       "WHERE a.[CustomerId]=" + ri.customerId;
                var FlashDealWithItemIds = authContext.Database.SqlQuery<FlashDealWithItem>(sqlquery).ToList();

                try
                {
                    if (authContext.Database.Connection.State != ConnectionState.Open)
                        authContext.Database.Connection.Open();

                    var orderIdDt = new DataTable();
                    orderIdDt.Columns.Add("IntValue");
                    foreach (var item in ri.itemIds)
                    {
                        var dr = orderIdDt.NewRow();
                        dr["IntValue"] = item;
                        orderIdDt.Rows.Add(dr);
                    }

                    try
                    {

                        var param = new SqlParameter("ItemIds", orderIdDt);
                        param.SqlDbType = SqlDbType.Structured;
                        param.TypeName = "dbo.IntValues";
                        var cmd = authContext.Database.Connection.CreateCommand();
                        cmd.CommandText = "[dbo].[GetRelatedItem]";
                        cmd.CommandType = System.Data.CommandType.StoredProcedure;
                        cmd.Parameters.Add(new SqlParameter("@warehouseId", ri.warehouseId));
                        cmd.Parameters.Add(new SqlParameter("@skip", ri.skip));
                        cmd.Parameters.Add(new SqlParameter("@take", ri.take));
                        cmd.Parameters.Add(param);

                        // Run the sproc
                        var reader = cmd.ExecuteReader();
                        var RelatedItem = ((IObjectContextAdapter)authContext)
                        .ObjectContext
                        .Translate<Itemsearch>(reader).ToList();

                        ItemSearch = RelatedItem;
                    }
                    catch (Exception ex)
                    {

                        throw;
                    }

                    foreach (var it in ItemSearch)
                    {
                        if (!inActiveCustomer)
                        {
                            if (!(it.OfferStartTime <= DateTime.Now && it.OfferEndTime >= DateTime.Now))
                            {
                                if (it.OfferCategory == 2)
                                {
                                    it.IsOffer = false;
                                    it.FlashDealSpecialPrice = 0;
                                    it.OfferCategory = 0;
                                }
                                else if (it.OfferCategory == 1)
                                {
                                    it.IsOffer = false;
                                    it.OfferCategory = 0;
                                }

                            }
                            else if ((it.OfferStartTime <= DateTime.Now && it.OfferEndTime >= DateTime.Now) && it.OfferCategory == 2)
                            {
                                if (FlashDealWithItemIds != null && FlashDealWithItemIds.Any(x => x.ItemId == it.ItemId))
                                {
                                    it.IsOffer = false;
                                    it.FlashDealSpecialPrice = 0;
                                    it.OfferCategory = 0;
                                }
                            }
                        }
                        else
                        {
                            it.IsOffer = false;
                            it.FlashDealSpecialPrice = 0;
                            it.OfferCategory = 0;
                        }

                        try
                        {
                            if (!it.IsOffer)
                            {
                                /// Dream Point Logic && Margin Point
                                int? MP, PP;
                                double xPoint = xPointValue * 10;

                                //Customer (0.2 * 10=1)
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

                            }
                            else { it.dreamPoint = 0; }

                            // Margin % On app site logic ((MRP-UnitPrice)*100)/UnitPrice
                            if (it.price > it.UnitPrice)
                            {
                                it.marginPoint = Convert.ToInt32(((it.price - it.UnitPrice) * 100) / it.UnitPrice);//MP;  we replce marginpoint value by margin for app here 
                            }
                            else
                            {
                                it.marginPoint = 0;
                            }
                        }
                        catch { }

                        if (it.price > it.UnitPrice)
                        {
                            it.marginPoint = Convert.ToInt32(((it.price - it.UnitPrice) * 100) / it.UnitPrice);//MP;  we replce marginpoint value by margin for app here 
                        }
                        else
                        {
                            it.marginPoint = 0;
                        }

                        if (ri.lang.Trim() == "hi")
                        {
                            if (it.HindiName != null)
                            {
                                if (it.IsSensitive == true)
                                {
                                    if (it.IsSensitiveMRP == false)
                                    {
                                        it.itemname = it.HindiName + " " + it.UnitofQuantity + " " + it.UOM; //item display name   
                                    }
                                    else
                                    {
                                        it.itemname = it.HindiName + " " + it.price + " MRP " + it.UnitofQuantity + " " + it.UOM; //item display name                               
                                    }
                                }
                                else
                                {
                                    it.itemname = it.HindiName + " " + it.price + " MRP "; //item display name                               
                                }

                            }
                            else
                            {
                                it.itemname = it.itemBaseName + " " + it.price + " MRP ";
                            }
                        }
                    }
                }
                finally
                {
                    authContext.Database.Connection.Close(); ;
                }
            }
            return Request.CreateResponse(HttpStatusCode.OK, ItemSearch);
        }

        [HttpGet]
        [Route("GetOfferItem")]
        [AllowAnonymous]

        public HttpResponseMessage GetOfferItem(int warehouseId, int customerId, string lang)
        {
            var ItemSearch = new List<Itemsearch>();
            using (var authContext = new AuthContext())
            {
                var ActiveCustomer = authContext.Customers.FirstOrDefault(x => x.CustomerId == customerId);
                var inActiveCustomer = ActiveCustomer != null && ActiveCustomer.Active == false && ActiveCustomer.Deleted == true ? true : false;
                string sqlquery = "SELECT a.[FlashDealId] AS [FlashDealId], a.[ItemId] AS [ItemId] FROM [dbo].[FlashDealItemConsumeds] AS a inner join AppHomeSectionItems c on a.FlashDealId = c.sectionItemId inner join dbo.AppHomeSections b on b.SectionID = c.apphomesections_SectionID  and b.Active=1 and b.[Deleted]=0  and b.WarehouseID=" + warehouseId +
                                       " WHERE a.[CustomerId]=" + customerId;
                var FlashDealWithItemIds = authContext.Database.SqlQuery<FlashDealWithItem>(sqlquery).ToList();

                try
                {
                    if (authContext.Database.Connection.State != ConnectionState.Open)
                        authContext.Database.Connection.Open();

                    try
                    {

                        var cmd = authContext.Database.Connection.CreateCommand();
                        cmd.CommandText = "[dbo].[GetOfferItem]";
                        cmd.CommandType = System.Data.CommandType.StoredProcedure;
                        cmd.Parameters.Add(new SqlParameter("@warehouseId", warehouseId));

                        // Run the sproc
                        var reader = cmd.ExecuteReader();
                        var OfferItem = ((IObjectContextAdapter)authContext)
                        .ObjectContext
                        .Translate<Itemsearch>(reader).ToList();

                        ItemSearch = OfferItem;
                    }
                    catch (Exception ex)
                    {
                        throw;
                    }

                    foreach (var it in ItemSearch)
                    {
                        if (!inActiveCustomer)
                        {
                            if (!(it.OfferStartTime <= DateTime.Now && it.OfferEndTime >= DateTime.Now))
                            {
                                if (it.OfferCategory == 2)
                                {
                                    it.IsOffer = false;
                                    it.FlashDealSpecialPrice = 0;
                                    it.OfferCategory = 0;
                                }
                                else if (it.OfferCategory == 1)
                                {
                                    it.IsOffer = false;
                                    it.OfferCategory = 0;
                                }

                            }
                            else if ((it.OfferStartTime <= DateTime.Now && it.OfferEndTime >= DateTime.Now) && it.OfferCategory == 2)
                            {
                                if (FlashDealWithItemIds != null && FlashDealWithItemIds.Any(x => x.ItemId == it.ItemId))
                                {
                                    it.IsOffer = false;
                                    it.FlashDealSpecialPrice = 0;
                                    it.OfferCategory = 0;
                                }
                            }
                        }
                        else
                        {
                            it.IsOffer = false;
                            it.FlashDealSpecialPrice = 0;
                            it.OfferCategory = 0;
                        }

                        try
                        {
                            if (!it.IsOffer)
                            {
                                /// Dream Point Logic && Margin Point
                                int? MP, PP;
                                double xPoint = xPointValue * 10;

                                //Customer (0.2 * 10=1)
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
                                    it.marginPoint = Convert.ToInt32(((it.price - it.UnitPrice) * 100) / it.UnitPrice);//MP;  we replce marginpoint value by margin for app here 
                                }
                                else
                                {
                                    it.marginPoint = 0;
                                }
                            }
                            else { it.dreamPoint = 0; }
                        }
                        catch { }

                        if (it.price > it.UnitPrice)
                        {
                            it.marginPoint = Convert.ToInt32(((it.price - it.UnitPrice) * 100) / it.UnitPrice);//MP;  we replce marginpoint value by margin for app here 
                        }
                        else
                        {
                            it.marginPoint = 0;
                        }
                        if (lang.Trim() == "hi")
                        {
                            if (it.HindiName != null)
                            {
                                if (it.IsSensitive == true)
                                {
                                    if (it.IsSensitiveMRP == false)
                                    {
                                        it.itemname = it.HindiName + " " + it.UnitofQuantity + " " + it.UOM; //item display name   
                                    }
                                    else
                                    {
                                        it.itemname = it.HindiName + " " + it.price + " MRP " + it.UnitofQuantity + " " + it.UOM; //item display name                               
                                    }
                                }
                                else
                                {
                                    it.itemname = it.HindiName + " " + it.price + " MRP "; //item display name                               
                                }

                            }
                            else
                            {
                                it.itemname = it.itemBaseName + " " + it.price + " MRP ";
                            }
                        }
                    }
                }
                finally
                {
                    authContext.Database.Connection.Close();
                }
            }
            ItemSearch = ItemSearch.Where(x => x.IsOffer == true).ToList();
            return Request.CreateResponse(HttpStatusCode.OK, ItemSearch);
        }

        [HttpGet]
        [Route("GetCustomerSearchKeyword")]

        public async Task<HttpResponseMessage> SearchCustomerKeyWord(int customerId, int skip, int take, string lang)
        {
            MongoDbHelper<CustomerProductSearch> mongoDbHelper = new MongoDbHelper<CustomerProductSearch>();
            List<string> keywords = mongoDbHelper.Select(x => x.customerId == customerId && x.IsDeleted == false, x => x.OrderByDescending(y => y.CreatedDate), skip, take).ToList().Select(x => x.keyword).ToList();
            keywords = keywords.Distinct().ToList();
            if (!string.IsNullOrEmpty(lang) && lang != "en")
            {
                Annotate annotate = new Annotate();
                var converttext = string.Join("|", keywords);
                var hindiText = await annotate.GetTranslatedText(converttext, lang);
                keywords = hindiText.Split('|').ToList();
            }
            return Request.CreateResponse(HttpStatusCode.OK, keywords);

        }

        #region Add Murli Item  Details 
        /// <summary>
        /// Add Murli Item  Details 
        /// Created By Raj
        /// Crated Date 10/09/2019
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        [Authorize]
        [Route("MurliItem")]
        [HttpGet]
        public MurliItemsDetails AddMurliItem(int ItemId)
        {
            using (var context = new AuthContext())
            {
                var identity = User.Identity as ClaimsIdentity;
                int userid = 0;
                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                    userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

                var DisplayName = context.Peoples.FirstOrDefault(x => x.PeopleID == userid)?.DisplayName;
                var Itemdetails = context.itemMasters.Where(x => x.ItemId == ItemId).FirstOrDefault();

                MurliItemsDetails MTD = new MurliItemsDetails();
                MTD.ItemId = Itemdetails.ItemId;
                MTD.WarehouseId = Itemdetails.WarehouseId;
                MTD.ItemMultiMRPId = Itemdetails.ItemMultiMRPId;
                MTD.itemBaseName = Itemdetails.itemBaseName;
                MTD.itemname = Itemdetails.itemname;
                MTD.Number = Itemdetails.Number;
                MTD.UnitPrice = Itemdetails.UnitPrice;
                MTD.MRP = Itemdetails.MRP;
                MTD.CreateDate = indianTime;
                MTD.UpdateDate = indianTime;
                MTD.Isactive = true;
                MTD.Deleted = false;
                MTD.Createdby = DisplayName;
                context.MurliItemsDetailsDB.Add(MTD);
                context.Commit();
                logger.Info("Successfully add Item: ");
                return MTD;


            }

        }
        #endregion

        #region Delete Murli Item  Details 
        /// <summary>
        /// Delete Murli Item  Details 
        /// Created By Raj
        /// Crated Date 10/09/2019
        /// </summary>
        /// <param name="MurliItemId"></param>
        /// <returns></returns>
        [Authorize]
        [Route("DeleteMurliItem")]
        [HttpGet]
        public MurliItemsDetails DeleteMurliItem(int MurliItemId)
        {
            using (var context = new AuthContext())
            {

                var identity = User.Identity as ClaimsIdentity;
                int userid = 0;

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                    userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

                var DisplayName = context.Peoples.FirstOrDefault(x => x.PeopleID == userid)?.DisplayName;

                MurliItemsDetails MTD = context.MurliItemsDetailsDB.Where(x => x.MurliItemId == MurliItemId).FirstOrDefault();
                MTD.Deleted = true;
                MTD.Isactive = false;
                MTD.UpdateDate = indianTime;
                MTD.Updateby = DisplayName;
                context.Entry(MTD).State = EntityState.Modified;
                context.Commit();
                logger.Info("Successfully delete Item: ");
                return MTD;

            }

        }

        #endregion

        #region Get Murli Item  Details 
        /// <summary>
        /// Get Murli Item  Details 
        /// Created By Raj
        /// Crated Date 10/09/2019
        /// </summary>
        /// <param name="MurliItemId"></param>
        /// <returns></returns>
        [Authorize]
        [Route("GetMurliItem")]
        [HttpGet]
        public List<MurliItemsDetails> GetMurliItem(int WarehouseId)
        {
            using (var context = new AuthContext())
            {


                List<MurliItemsDetails> MTD = context.MurliItemsDetailsDB.Where(x => x.WarehouseId == WarehouseId && x.Deleted == false && EntityFunctions.TruncateTime(x.CreateDate) == EntityFunctions.TruncateTime(DateTime.Now)).ToList();
                return MTD;

            }

        }

        #endregion
        #region Get Search Item  Details 
        /// <summary>
        ///  Search Murli Item  Details 
        /// Created By Raj
        /// Crated Date 10/09/2019
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("SearchMurliItem")]
        public async Task<List<ItemMaster>> searchsitem(string key, int WarehouseId)
        {
            using (var context = new AuthContext())
            {


                List<ItemMaster> MTD = context.itemMasters.Where(t => t.Deleted == false && t.WarehouseId == WarehouseId && (t.itemname.ToLower().Contains(key.Trim().ToLower()) || t.Number.ToLower().Contains(key.Trim().ToLower()))).ToList();

                return MTD;


            }
        }
        #endregion

        #region convert Engish to hindi 
        /// <summary>
        ///  convert Engish to hindi 
        /// Created By Raj
        /// Crated Date 10/09/2019
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("ConvertMurliItem")]
        public async Task<string> Convertitem(string Items, string language, int WarehouseId)
        {
            using (var context = new AuthContext())
            {


                var warehousename = context.Warehouses.Where(x => x.WarehouseId == WarehouseId).Select(x => x.WarehouseName).FirstOrDefault();

                Annotate annotate = new Annotate();
                var converttext = Items;
                var hindiText = await annotate.GetTranslatedText(converttext, language);


                return hindiText;


            }
        }

        [HttpGet]
        [Route("ConvertToHindi")]
        public async Task<string> ConvertToHindi(string text, string language)
        {

            Annotate annotate = new Annotate();
            var hindiText = await annotate.GetgoogleTranslate(text, language, "hi");
            return hindiText;

        }



        #endregion

        #region Save hindi text in  MurliWarehouseTopItem 
        /// <summary>
        ///  Save hindi text in  MurliWarehouseTopItem 
        ///  and convert  hindi text to audio
        /// Created By Raj
        /// Crated Date 11/09/2019
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("AddHindiMurliItem")]
        public async Task<MurliWarehouseTopItem> AddHiindiitem(MurliAudioDc murliAudioDc)
        {
            using (var context = new AuthContext())
            {
                var identity = User.Identity as ClaimsIdentity;
                int userid = 0;
                bool Istrue = false;
                MurliWarehouseTopItem MTopItem = new MurliWarehouseTopItem();
                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                    userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
                DateTime todaydatetime = DateTime.Now;

                var fromdate = todaydatetime.Date;
                DateTime todate = todaydatetime.Date.AddDays(1).AddSeconds(-1);


                var datetimedata = context.MurliWarehouseTopItemDB.Where(x => x.WarehouseId == murliAudioDc.WarehouseId && x.Deleted == false && x.FromDate >= fromdate && x.ToDate < todate).Select(x => new { x.ToDate, x.FromDate }).ToList();

                Istrue = datetimedata.Any(x => x.FromDate <= murliAudioDc.FromDate && x.ToDate > murliAudioDc.FromDate);
                if (Istrue == false)
                {
                    Istrue = datetimedata.Any(x => x.FromDate < murliAudioDc.ToDate && x.ToDate >= murliAudioDc.ToDate);
                }


                //}
                //if(murliAudioDc.FromDate>=datetimedata.FromDate && )


                try
                {
                    if (!Istrue)
                    {
                        var warehousename = context.Warehouses.Where(x => x.WarehouseId == murliAudioDc.WarehouseId).Select(x => x.WarehouseName).FirstOrDefault();
                        var fileName = warehousename + "_" + DateTime.Now.ToString("MMddyyyyHHmmss");
                        logger.Info("Start Google File Creation");

                        var url = GoogleTextToSpeach.ConvertHindiTextToAudio(murliAudioDc.hindiText, fileName);
                        logger.Info("End Google File Creation");
                        MTopItem = context.MurliWarehouseTopItemDB.Where(x => x.WarehouseId == murliAudioDc.WarehouseId && x.IsActive == true && x.Deleted == false).FirstOrDefault();
                        //if (MTopItem != null)
                        //{
                        //    //MTopItem.Mp3url = url;
                        //    //MTopItem.HindiTopItem = murliAudioDc.hindiText;
                        //    MTopItem.IsActive = false;
                        //    MTopItem.UpdateDate = indianTime;
                        //    MTopItem.Updatedby = userid;
                        //    context.Entry(MTopItem).State = EntityState.Modified;
                        //    context.Commit();
                        //}

                        MurliWarehouseTopItem TopItem = new MurliWarehouseTopItem();
                        TopItem.WarehouseId = murliAudioDc.WarehouseId;
                        TopItem.WarehouseName = warehousename;
                        TopItem.HindiTopItem = murliAudioDc.hindiText;
                        TopItem.Mp3url = url;
                        TopItem.CreatedDate = indianTime;
                        TopItem.UpdateDate = indianTime;
                        TopItem.Createdby = userid;
                        TopItem.Updatedby = userid;
                        TopItem.IsActive = true;
                        TopItem.Deleted = false;
                        TopItem.FromDate = murliAudioDc.FromDate;
                        TopItem.ToDate = murliAudioDc.ToDate;

                        context.MurliWarehouseTopItemDB.Add(TopItem);
                        context.Commit();
                    }
                    MTopItem.Deleted = Istrue;
                    return MTopItem;
                }
                catch (Exception ex)
                {
                    logger.Error("Error in Convert google text to audio: " + ex.Message);
                }
                return MTopItem;

            }
        }
        #endregion

        #region Get Murli Warehouse  Details 
        /// <summary>
        /// Get Murli Warehouse  Details 
        /// Created By Raj
        /// Crated Date 12/09/2019
        /// </summary>
        /// <param name="MurliItemId"></param>
        /// <returns></returns>
        [Authorize]
        [Route("GetMurliwarehouseitem")]
        [HttpGet]
        public async Task<List<MurliWarehouseTopItem>> MurliWarehouse()
        {
            using (var context = new AuthContext())
            {
                List<MurliWarehouseTopItem> MTD = context.MurliWarehouseTopItemDB.Where(a => a.IsActive && a.Deleted == false).OrderByDescending(s => new { s.IsActive, s.CreatedDate }).ToList();
                return MTD;

            }

        }


        [Route("GetMurliAudioForMobile")]
        [HttpGet]

        public async Task<WarehouseMurliAudioDc> GetMurliAudioForMobile(int warehouseId, int customerId)
        {



            WarehouseMurliAudioDc warehouseMurliAudioDc = null;
            using (var context = new AuthContext())
            {
                DateTime todaydatetime = DateTime.Now;
                DateTime fromdate = todaydatetime.Date;
                DateTime todate = todaydatetime.Date.AddDays(1).AddSeconds(-1);

                var MTDList = context.MurliWarehouseTopItemDB.Where(a => a.IsActive && a.WarehouseId == warehouseId && EntityFunctions.TruncateTime(a.FromDate) == EntityFunctions.TruncateTime(DateTime.Now)).ToList();
                if (MTDList.Count == 0)
                {
                    DateTime? maxdate = context.MurliWarehouseTopItemDB.Where(a => a.IsActive && a.WarehouseId == warehouseId).Max(x => x.FromDate);
                    if (maxdate.HasValue)
                        MTDList = context.MurliWarehouseTopItemDB.Where(a => a.IsActive && a.WarehouseId == warehouseId && EntityFunctions.TruncateTime(a.FromDate) == EntityFunctions.TruncateTime(maxdate)).ToList();
                }


                var MTD = MTDList.Where(x => x.FromDate <= todaydatetime && x.ToDate > todaydatetime).ToList();
                int count = MTD.Count;
                if (count == 0)
                {
                    MTD = MTDList.Where(x => x.FromDate < todaydatetime && x.ToDate >= todaydatetime).ToList();
                    count = MTD.Count;
                }


                // var MTD = await context.MurliWarehouseTopItemDB.FirstOrDefaultAsync(a => a.IsActive && a.WarehouseId == warehouseId);
                var MTDGet = MTDList.Where(x => x.ToDate < todaydatetime).LastOrDefault();
                if (count > 0)
                {
                    MTDGet = MTD.FirstOrDefault();
                }
                //else if (MTDList != null)
                //{
                //    MTDGet = MTDList.Where(x => x.ToDate < todaydatetime).LastOrDefault();
                //}

                if (MTDGet != null)
                {
                    warehouseMurliAudioDc = new WarehouseMurliAudioDc();
                    warehouseMurliAudioDc.Id = MTDGet.Id;
                    warehouseMurliAudioDc.Mp3url = MTDGet.Mp3url;
                    warehouseMurliAudioDc.HindiTopItem = MTDGet.HindiTopItem;
                }
                return warehouseMurliAudioDc;
            }
        }

        #endregion

        #region  get by warehousebased search Item
        /// <summary>
        /// Created Date 13/09/2019
        /// Created by raj
        /// </summary>
        /// <param name="name"></param>
        /// <param name="WarehouseId"></param>
        /// <returns></returns>

        [Route("GetByName/name/{name}/WarehouseId/{WarehouseId}")]
        [HttpGet]
        public IHttpActionResult GetbyItemName(string name, int WarehouseId)
        {
            using (var authContext = new AuthContext())
            {
                var itemlist = authContext.itemMasters.Where(x => x.WarehouseId == WarehouseId && x.Deleted == false && x.itemname.ToLower().Contains(name.ToLower())).Take(50).ToList();
                return Ok(itemlist);
            }
        }
        #endregion

        #region Save    MurliWarehouseItem 
        /// <summary>
        ///  Save    MurliWarehouseItem 
        /// Created By Raj
        /// Crated Date 11/09/2019
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("AddMurliwarehouse")]

        public async Task<string> AddMurliwarehouse(int WarehouseId)
        {

            using (var context = new AuthContext())
            {
                var identity = User.Identity as ClaimsIdentity;
                int userid = 0;
                MurliWarehouseTopItem MTopItem = new MurliWarehouseTopItem();
                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                    userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

                var warehousename = context.Warehouses.Where(x => x.WarehouseId == WarehouseId).Select(x => x.WarehouseName).FirstOrDefault();
                MTopItem = context.MurliWarehouseTopItemDB.Where(x => x.WarehouseId == WarehouseId && x.IsActive == true && x.Deleted == false).FirstOrDefault();
                if (MTopItem == null)
                {
                    MurliWarehouseTopItem TopItem = new MurliWarehouseTopItem();
                    TopItem.WarehouseId = WarehouseId;
                    TopItem.WarehouseName = warehousename;
                    TopItem.HindiTopItem = null;
                    TopItem.Mp3url = null;
                    TopItem.CreatedDate = indianTime;
                    TopItem.UpdateDate = indianTime;
                    TopItem.Createdby = userid;
                    TopItem.Updatedby = userid;
                    TopItem.IsActive = true;
                    TopItem.Deleted = false;
                    TopItem.FromDate = indianTime;
                    TopItem.ToDate = indianTime.AddHours(1);
                    context.MurliWarehouseTopItemDB.Add(TopItem);
                    context.Commit();
                    return "Add successfully";
                }
                return "Already added this warehouse";
            }
        }
        #endregion

        #region Get Murli Warehousebased  Details 
        /// <summary>
        /// Get Murli Warehouse  Details 
        /// Created By Raj
        /// Crated Date 12/09/2019
        /// </summary>
        /// <param name="MurliItemId"></param>
        /// <returns></returns>
        [Route("GetMurliwarehousebaseditem")]
        [HttpGet]
        public async Task<List<MurliWarehouseTopItem>> MurliWarehouse(int WarehouseId)
        {
            using (var context = new AuthContext())
            {

                List<MurliWarehouseTopItem> MTD = context.MurliWarehouseTopItemDB.Where(x => x.WarehouseId == WarehouseId && x.Deleted == false).OrderByDescending(s => new { s.IsActive, s.CreatedDate }).ToList();
                return MTD;
            }
        }

        #endregion
        #region Update IsActive Murli Warehousebased  Details 
        /// <summary>
        /// Get Murli Warehouse  Details 
        /// Created By Raj
        /// Crated Date 12/09/2019
        /// </summary>
        /// <param name="MurliItemId"></param>
        /// <returns></returns>
        [Route("UpdateIsActive")]
        [HttpGet]
        public async Task<List<MurliWarehouseTopItem>> IsActiveMurliItem(int Id, int WarehouseId)
        {
            using (var context = new AuthContext())
            {

                var MTD = context.MurliWarehouseTopItemDB.Where(x => x.WarehouseId == WarehouseId && x.Id == Id).FirstOrDefault();
                if (MTD != null)
                {
                    MTD.IsActive = true;
                    context.Entry(MTD).State = EntityState.Modified;
                    context.Commit();
                }


                //var MTD = context.MurliWarehouseTopItemDB.Where(x => x.WarehouseId == WarehouseId).Select(x => x.Id).ToList();
                //foreach (var data in MTD)
                //{
                //    MurliWarehouseTopItem MTDdata = context.MurliWarehouseTopItemDB.Where(x => x.Id == data).FirstOrDefault();
                //    if (data == Id)
                //    {
                //        MTDdata.IsActive = true;
                //        context.Entry(MTDdata).State = EntityState.Modified;
                //        context.Commit();
                //    }
                //    else
                //    {
                //        MTDdata.IsActive = false;
                //        context.Entry(MTDdata).State = EntityState.Modified;
                //        context.Commit();
                //    }
                //}

                List<MurliWarehouseTopItem> MTDC = context.MurliWarehouseTopItemDB.Where(x => x.WarehouseId == WarehouseId && x.Deleted == false).OrderByDescending(s => new { s.IsActive, s.CreatedDate }).ToList();

                return MTDC;

            }

        }

        #endregion

        #region get top sale item warehouse Based
        /// <summary>
        /// Created Date:23/09/2019
        /// Created by Raj
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("MurliTopsaleitem")]
        [AllowAnonymous]
        public async Task<bool> GetitemsaleitemAsync()
        {

            dynamic warehouses;
            List<Itemsearch> MostSellingProduct;
            using (var authContext = new AuthContext())
            {
                warehouses = authContext.Warehouses.Where(x => x.Deleted == false && x.active == true && x.IsKPP == false).Select(x => new { x.WarehouseId, x.WarehouseName }).ToList();
            }
            foreach (var warehousedata in warehouses)
            {
                using (var authContext = new AuthContext())
                {
                    if (authContext.Database.Connection.State != ConnectionState.Open)
                        authContext.Database.Connection.Open();
                    int customerId = 0, skip = 0, take = 5;

                    var cmd = authContext.Database.Connection.CreateCommand();
                    cmd.CommandText = "[dbo].[GetMurliMostSellingProduct]";
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@warehouseId", warehousedata.WarehouseId));
                    cmd.Parameters.Add(new SqlParameter("@customerId", customerId));
                    cmd.Parameters.Add(new SqlParameter("@skip", skip));
                    cmd.Parameters.Add(new SqlParameter("@take", take));

                    // Run the sproc
                    var reader = cmd.ExecuteReader();

                    // Read Blogs from the first result set
                    MostSellingProduct = ((IObjectContextAdapter)authContext)
                               .ObjectContext
                               .Translate<Itemsearch>(reader).ToList();
                }
                Annotate annotate = new Annotate();
                string DefaultText = "<speak><break time='500ms'/> नमश्कार <break time='100ms'/> मैं मुरली<break time='200ms'/> शॉपकिराना मैं आप सभि का स्वागत करता हूँ <break time='500ms'/>में लाया हूँ<break time='30ms'/>आप सब के लिए<break time='20ms'/>आज के स्पेशल प्रोडक्ट्स<break time='500ms'/> जहाँ आप पाएँगे<break time='500ms'/>";
                //string DefaultText = "<break time='300ms'/>Hello Aap Ke liye aaj ke 5 khhas product ";
                string language = "hi";
                string HindiText = await annotate.GetTranslatedText(DefaultText, language);

                using (var authContext = new AuthContext())
                {
                    foreach (var itemdata in MostSellingProduct)
                    {
                        var item = authContext.itemMasters.FirstOrDefault(x => x.ItemId == itemdata.ItemId);
                        MurliItemsDetails WarehouseTopItem = new MurliItemsDetails();
                        if (itemdata.price > itemdata.UnitPrice)
                        {
                            itemdata.marginPoint = Convert.ToInt32(((itemdata.price - itemdata.UnitPrice) * 100) / itemdata.UnitPrice);//MP;  we replce marginpoint value by margin for app here 
                        }
                        string struom = "";
                        if (item.UOM == "Gm")
                            struom = "gram";
                        else if (item.UOM == "Kg")
                            struom = "kilogram";
                        else if (item.UOM == "Combo")
                            struom = "combo";
                        else if (item.UOM == "Ltr")
                            struom = "liter";
                        else if (item.UOM == "Ml")
                            struom = "mili liter";
                        else if (item.UOM == "Pc")
                            struom = "pieces";
                        else if (item.UOM == "Size")
                            struom = "size";

                        var price = item.UnitPrice.ToString().Split('.');

                        string converttext = item.itemBaseName + " " + item.MRP + " MRP " + (!string.IsNullOrEmpty(struom) ? (item.UnitofQuantity + " " + struom) : "");// + " price ";
                        //converttext += price[0] + " rupaye ";
                        //if (price.Length == 2 && Convert.ToInt32(price[1]) > 0)
                        //    converttext += price[1] + " paise";

                        var hindiText = await annotate.GetTranslatedText(converttext, language);
                        //converttext = " jisame aap ka margin " + itemdata.marginPoint + " percent hai";
                        converttext = " jisame aap ka margin " + itemdata.marginPoint + " percent hai";

                        hindiText += "<break time='500ms'/>" + await annotate.GetTranslatedText(converttext, language);
                        HindiText += hindiText + ",";

                        WarehouseTopItem.ItemId = itemdata.ItemId;
                        WarehouseTopItem.WarehouseId = itemdata.WarehouseId;
                        WarehouseTopItem.ItemMultiMRPId = item.ItemMultiMRPId;
                        WarehouseTopItem.itemBaseName = itemdata.itemBaseName;
                        WarehouseTopItem.itemname = itemdata.itemname;
                        WarehouseTopItem.Number = itemdata.Number;
                        WarehouseTopItem.UnitPrice = itemdata.UnitPrice;
                        WarehouseTopItem.MRP = item.MRP;
                        WarehouseTopItem.CreateDate = indianTime;
                        WarehouseTopItem.UpdateDate = indianTime;
                        WarehouseTopItem.Isactive = true;
                        WarehouseTopItem.Deleted = false;
                        WarehouseTopItem.Createdby = "System";
                        WarehouseTopItem.marginPoint = itemdata.marginPoint.HasValue ? Convert.ToInt32(itemdata.marginPoint) : 0;
                        authContext.MurliItemsDetailsDB.Add(WarehouseTopItem);

                    }
                    var fileName = warehousedata.WarehouseName + "_" + DateTime.Now.ToString("MMddyyyyHHmmss");
                    string lastword = "<break time='200ms'/>तो जल्दी कीजिये<break time='20ms'/>यह रेटस केवल सिमित अवधि के लिए ही उपलब्ध है<break time='500ms'/>धन्यवाद</speak>";
                    HindiText += await annotate.GetTranslatedText(lastword, language);

                    //HindiText += "</prosody></ speak >";
                    var url = GoogleTextToSpeach.ConvertHindiTextToAudio(HindiText, fileName);

                    string query = "Update MurliWarehouseTopItems set IsActive=0,Deleted=1,Updatedby=1,UpdateDate=GETDATE() where WarehouseId=" + warehousedata.WarehouseId;
                    authContext.Database.ExecuteSqlCommand(query);
                    authContext.Commit();
                    MurliWarehouseTopItem TopItem = new MurliWarehouseTopItem();
                    TopItem.WarehouseId = warehousedata.WarehouseId;
                    TopItem.WarehouseName = warehousedata.WarehouseName;
                    TopItem.HindiTopItem = HindiText;
                    TopItem.Mp3url = url;
                    TopItem.CreatedDate = indianTime;
                    TopItem.UpdateDate = indianTime;
                    TopItem.Createdby = 1;
                    TopItem.Updatedby = 1;
                    TopItem.IsActive = true;
                    TopItem.Deleted = false;
                    TopItem.FromDate = indianTime;
                    TopItem.ToDate = indianTime.AddHours(3);
                    authContext.MurliWarehouseTopItemDB.Add(TopItem);
                    authContext.Commit();
                }
            }
            return true;
        }
        #endregion

        #region Remove Murli Item Warehousebased  Details 
        /// <summary>
        /// remove Warehouse  Item Details 
        /// Created By Raj
        /// Crated Date 27/09/2019
        /// </summary>
        /// <param name="MurliItemId"></param>
        /// <returns></returns>
        [Route("Removemurliaudio")]
        [HttpDelete]
        public async Task<List<MurliWarehouseTopItem>> RemoveMurliItem(int Id, int WarehouseId)
        {
            using (var context = new AuthContext())
            {

                MurliWarehouseTopItem MTDdata = context.MurliWarehouseTopItemDB.Where(x => x.Id == Id && x.WarehouseId == WarehouseId).FirstOrDefault();
                if (MTDdata != null)
                {
                    MTDdata.IsActive = false;
                    MTDdata.Deleted = true;
                    context.Entry(MTDdata).State = EntityState.Modified;
                    context.Commit();
                }

                List<MurliWarehouseTopItem> MTDC = context.MurliWarehouseTopItemDB.Where(x => x.WarehouseId == WarehouseId && x.Deleted == false).OrderByDescending(s => new { s.IsActive, s.CreatedDate }).ToList();

                return MTDC;

            }

        }


        [HttpGet]
        [Route("MurliImage")]

        public async Task<MobileMurliImageDc> GetMurliImage(int customerId, int WarehouseId)
        {
            MobileMurliImageDc MobileMurliImageDc = new MobileMurliImageDc();
            string baseUrl = HttpContext.Current.Request.Url.GetLeftPart(UriPartial.Authority);
            using (var context = new AuthContext())
            {
                var MurliImages = await context.MurliAudioImageDB.Where(x => x.IsActive && !x.Deleted && x.WarehouseId == WarehouseId).Include(x => x.MurliImages).FirstOrDefaultAsync();
                if (MurliImages != null)
                {
                    MobileMurliImageDc.Id = MurliImages.Id;
                    MobileMurliImageDc.Title = MurliImages.Title;
                    MobileMurliImageDc.WarehouseId = MurliImages.WarehouseId;
                    MobileMurliImageDc.Images = MurliImages.MurliImages.Select(x => new MobileMurliImageSequenceDc
                    {
                        Id = x.Id,
                        ImagePath = baseUrl + x.ImagePath,
                        Name = x.Name
                    }).ToList();
                }
                return MobileMurliImageDc;
            }
        }

        [HttpPost]
        [Route("AddMurliImage")]
        public async Task<bool> AddMurliImage(MurliAudioImageDc murliImageDc)
        {
            bool result = false;
            using (var context = new AuthContext())
            {
                var identity = User.Identity as ClaimsIdentity;
                int userid = 0;

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                    userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

                try
                {

                    MurliAudioImage MurliAudioImage = new MurliAudioImage
                    {
                        Createdby = userid,
                        CreatedDate = DateTime.Now,
                        Deleted = murliImageDc.Deleted,
                        IsActive = false,
                        Title = murliImageDc.Title,
                        Updatedby = userid,
                        UpdatedDate = DateTime.Now,
                        WarehouseId = murliImageDc.WarehouseId,
                        MurliImages = murliImageDc.MurliImageDcs.Select(x => new MurliImage
                        {
                            CreateDate = DateTime.Now,
                            Createdby = userid,
                            Deleted = x.Deleted,
                            Id = x.MurliImageId,
                            ImagePath = x.ImagePath,
                            Isactive = true,
                            Name = x.Name,
                            Updateby = userid,
                            UpdateDate = DateTime.Now
                        }).ToList()
                    };
                    context.MurliAudioImageDB.Add(MurliAudioImage);
                    result = (await context.CommitAsync()) > 0;
                }
                catch (Exception ex)
                {
                    logger.Error("Error in AddMurliImage : " + ex.Message);
                }
                return result;
            }
        }

        [HttpGet]
        [Route("ActiveMurliimages/Id/{Id}/IsActive/{IsActive}/WarehouseId/{WarehouseId}")]
        public bool Publish(int Id, bool IsActive, int WarehouseId)
        {
            try
            {

                using (var context = new AuthContext())
                {
                    context.Database.ExecuteSqlCommand("Update MurliAudioImages SET IsActive = 0 Where IsActive = 1 and WarehouseId = " + WarehouseId);
                    context.Database.ExecuteSqlCommand("Update MurliAudioImages SET IsActive = " + (IsActive ? 1 : 0).ToString() + " Where WarehouseId = " + WarehouseId + " and Id =" + Id.ToString());
                    return true;
                }

            }
            catch (Exception ex)
            {
                return true;
            }
        }

        [HttpGet]
        [Route("DeleteMurliImages/Id/{Id}/WarehouseId/{WarehouseId}")]
        public bool DeleteMurliImages(int Id, int WarehouseId)
        {
            try
            {

                using (var context = new AuthContext())
                {
                    context.Database.ExecuteSqlCommand("Update MurliAudioImages SET Deleted = 1 , IsActive = 0 Where Id = " + Id.ToString() + " and WarehouseId = " + WarehouseId.ToString());
                    return true;
                }

            }
            catch (Exception ex)
            {
                return true;
            }
        }



        [HttpGet]
        [Route("MurliImage")]
        public async Task<bool> ActiveMurliImage(int murliAudioId, int warehouseId)
        {
            bool result = false;
            using (var context = new AuthContext())
            {
                var murliimages = context.MurliAudioImageDB.Where(x => ((x.IsActive && !x.Deleted) || x.Id == murliAudioId) && x.WarehouseId == warehouseId).ToList();
                foreach (var item in murliimages)
                {
                    if (item.Id == murliAudioId)
                        item.IsActive = true;
                    else
                        item.IsActive = false;

                    context.Entry(item).State = EntityState.Modified;
                    result = (await context.CommitAsync()) > 0;
                }
                return result;
            }
        }


        [HttpGet]
        [Route("GetMurliImage")]
        public async Task<List<MurliAudioImageDcV1>> GetMurliImage(int warehouseId)
        {
            List<MurliAudioImageDcV1> murliAudioImageDcV1 = new List<MurliAudioImageDcV1>();
            try
            {
                using (var context = new AuthContext())
                {
                    string baseUrl = HttpContext.Current.Request.Url.GetLeftPart(UriPartial.Authority);

                    var MurliImages = await context.MurliAudioImageDB.Where(x => !x.Deleted && x.WarehouseId == warehouseId).Include(x => x.MurliImages).ToListAsync();
                    var a = context.Warehouses.Where(x => x.Deleted == false && x.WarehouseId == warehouseId).Select(x => new { WarehouseName = x.WarehouseName, cityname = x.CityName }).FirstOrDefault();
                    if (MurliImages != null)
                    {

                        murliAudioImageDcV1 = MurliImages.Select(x => new MurliAudioImageDcV1
                        {
                            Id = x.Id,
                            Title = x.Title,
                            WarehouseId = x.WarehouseId,
                            WarehouseName = a.cityname + " - " + a.WarehouseName,
                            CreateDate = x.CreatedDate,
                            Isactive = x.IsActive,
                            Createdby = x.Createdby,

                            MurliImageDcsV1 = x.MurliImages.Select(y => new MurliImageDcV1
                            {
                                Id = y.Id,
                                ImagePath = baseUrl + y.ImagePath,
                                Name = y.Name,
                                CreateDate = y.CreateDate,

                            }).ToList()
                        }).OrderByDescending(x => x.CreateDate).ToList();
                    }
                    return murliAudioImageDcV1;
                }

            }
            catch (Exception ee)
            {
                return null;
            }
        }


        [Route("getAllMurliImageA7")]
        [HttpGet]
        public dynamic getAllMurliImageA7(int? warehouseId)
        {
            using (var context = new AuthContext())
            {

                List<ManualSalesOrder> MSorderList = new List<ManualSalesOrder>();
                try
                {

                    var Getmurliimages = (from c in context.MurliAudioImageDB.Include("MurliImages").Where(x => x.Deleted == false).OrderBy(x => x.CreatedDate)
                                          join p in context.Peoples.Where(x => x.Deleted == false)
                                          on c.Createdby equals p.PeopleID into ps
                                          join d in context.Warehouses.Where(x => x.Deleted == false && x.WarehouseId == warehouseId)
                                          on c.WarehouseId equals d.WarehouseId into cd
                                          from p in ps.DefaultIfEmpty()
                                          select new
                                          {
                                              c.Createdby,
                                              c.CreatedDate,
                                              c.IsActive,
                                              c.MurliImages,
                                              c.Title,
                                              c.Updatedby,
                                              c.UpdatedDate,
                                              c.WarehouseId,
                                              p.DisplayName,
                                              p.Mobile,


                                          }).ToList();
                    return Getmurliimages;
                }
                catch (Exception ex)
                {

                    return null;
                }
            }
        }


        [Route("UploadMurliImageForMobile")]
        [HttpPost]

        public IHttpActionResult UploadMurliImageForMobile()
        {
            string LogoUrl = "";

            if (HttpContext.Current.Request.Files.AllKeys.Any())
            {
                var httpPostedFile = HttpContext.Current.Request.Files["file"];
                if (httpPostedFile != null)
                {
                    if (!Directory.Exists(HttpContext.Current.Server.MapPath("~/MurliImage")))
                        Directory.CreateDirectory(HttpContext.Current.Server.MapPath("~/MurliImage"));
                    string guid = Guid.NewGuid().ToString();
                    var ext = httpPostedFile.FileName;
                    string ext1 = Path.GetExtension(ext);
                    var filename = DateTime.Now.ToString("MMddyyyyHHmmss") + ext1;
                    LogoUrl = Path.Combine(HttpContext.Current.Server.MapPath("~/MurliImage"), filename);
                    httpPostedFile.SaveAs(LogoUrl);

                    AngularJSAuthentication.Common.Helpers.FileUploadHelper.Upload(httpPostedFile.FileName, "~/MurliImage", LogoUrl);

                    LogoUrl = "/MurliImage/" + filename;
                    return Created(LogoUrl, LogoUrl);
                }
                return BadRequest();
            }
            return BadRequest();

        }

        #endregion

        #region GetActiveItemWH  

        [HttpGet]
        [Route("GetActiveItemWH")]
        public IHttpActionResult GetActiveItemWH(int warehouseId)
        {
            using (var db = new AuthContext())
            {
                string sqlquery = "select count (*) from (select Number from ItemMasters where active=1 and WarehouseId=" + warehouseId + " group by Number) a";
                int ActiveCount = db.Database.SqlQuery<int>(sqlquery).FirstOrDefault();
                return Content(HttpStatusCode.OK, ActiveCount);
            }
        }
        #endregion

        [HttpGet]
        [Route("GetItemLimitData")]
        public IHttpActionResult GetItemLimitData(int ItemMultiMRPId, int WarehouseId)
        {
            if (ItemMultiMRPId > 0)
            {
                using (var db = new AuthContext())
                {

                    var itemlimitdata = db.ItemLimitMasterDB.Where(x => x.ItemMultiMRPId == ItemMultiMRPId && x.WarehouseId == WarehouseId).FirstOrDefault();
                    return Content(HttpStatusCode.OK, itemlimitdata);

                }
            }
            return Content(HttpStatusCode.OK, " ");

        }

        public PaggingData AllItemMasterForPagingWidWithInactiveItems(int list, int page, int Warehouse_id, int CompanyId, string status)
        {
            using (AuthContext db = new AuthContext())
            {
                PaggingData obj = new PaggingData();
                if (status == "true")
                {
                    obj.total_count = db.itemMasters.Where(x => x.WarehouseId == Warehouse_id && x.Deleted == false && x.CompanyId == CompanyId).Count();
                    obj.ordermaster = (from a in db.itemMasters
                                       where (a.Deleted == false && a.CompanyId == CompanyId && a.WarehouseId == Warehouse_id)
                                       join b in db.ItemMasterCentralDB on a.SellingSku equals b.SellingSku
                                       join c in db.DbCurrentStock.Where(x => x.WarehouseId == Warehouse_id) on a.Number equals c.ItemNumber
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
                                           NetPurchasePrice = a.NetPurchasePrice,
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
                                           DepoId = a.DepoId,
                                           BuyerId = a.BuyerId,
                                           BuyerName = a.BuyerName,
                                           IsReplaceable = a.IsReplaceable,
                                           DistributionPrice = a.DistributionPrice,
                                           DistributorShow = a.DistributorShow,
                                           ItemAppType = a.ItemAppType,
                                       }).OrderByDescending(s => s.ItemId).Skip((page - 1) * list).Take(list).ToList();
                }
                else if (status == "false")
                {
                    obj.total_count = db.itemMasters.Where(x => x.WarehouseId == Warehouse_id && x.Deleted == false && x.CompanyId == CompanyId).Count();
                    obj.ordermaster = (from a in db.itemMasters
                                       where (a.Deleted == false && a.CompanyId == CompanyId && a.WarehouseId == Warehouse_id)
                                       join b in db.ItemMasterCentralDB on a.SellingSku equals b.SellingSku
                                       join c in db.DbCurrentStock.Where(x => x.WarehouseId == Warehouse_id) on a.Number equals c.ItemNumber
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
                                           NetPurchasePrice = a.NetPurchasePrice,
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
                                           DepoId = a.DepoId,
                                           BuyerId = a.BuyerId,
                                           BuyerName = a.BuyerName,
                                           IsReplaceable = a.IsReplaceable,
                                           DistributionPrice = a.DistributionPrice,
                                           DistributorShow = a.DistributorShow,
                                           ItemAppType = a.ItemAppType,
                                       }).OrderByDescending(s => s.ItemId).Skip((page - 1) * list).Take(list).ToList();
                }
                else if (status == "ZeroQty")
                {
                    obj.total_count = db.itemMasters.Where(x => x.WarehouseId == Warehouse_id && x.Deleted == false && x.CompanyId == CompanyId).Count();
                    obj.ordermaster = (from a in db.itemMasters
                                       where (a.Deleted == false && a.CompanyId == CompanyId && a.WarehouseId == Warehouse_id)
                                       join b in db.ItemMasterCentralDB on a.SellingSku equals b.SellingSku
                                       join c in db.DbCurrentStock.Where(x => x.WarehouseId == Warehouse_id) on a.Number equals c.ItemNumber
                                       where (c.WarehouseId == Warehouse_id && c.CurrentInventory == 0 && c.ItemMultiMRPId == a.ItemMultiMRPId)
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
                                           NetPurchasePrice = a.NetPurchasePrice,
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
                                           DepoId = a.DepoId,
                                           BuyerId = a.BuyerId,
                                           BuyerName = a.BuyerName,
                                           IsReplaceable = a.IsReplaceable,
                                           DistributionPrice = a.DistributionPrice,
                                           DistributorShow = a.DistributorShow,
                                           ItemAppType = a.ItemAppType,
                                       }).OrderByDescending(s => s.ItemId).Skip((page - 1) * list).Take(list).ToList();

                }
                else if (status == "MaxToLow")
                {
                    obj.total_count = db.itemMasters.Where(x => x.WarehouseId == Warehouse_id && x.Deleted == false && x.CompanyId == CompanyId).Count();
                    obj.ordermaster = (from a in db.itemMasters
                                       where (a.Deleted == false && a.CompanyId == CompanyId && a.WarehouseId == Warehouse_id)
                                       join b in db.ItemMasterCentralDB on a.SellingSku equals b.SellingSku
                                       join c in db.DbCurrentStock.Where(x => x.WarehouseId == Warehouse_id) on a.Number equals c.ItemNumber
                                       where (c.WarehouseId == Warehouse_id && c.CurrentInventory > 0 && c.ItemMultiMRPId == a.ItemMultiMRPId)
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
                                           NetPurchasePrice = a.NetPurchasePrice,
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
                                           DepoId = a.DepoId,
                                           BuyerId = a.BuyerId,
                                           BuyerName = a.BuyerName,
                                           IsReplaceable = a.IsReplaceable,
                                           DistributionPrice = a.DistributionPrice,
                                           DistributorShow = a.DistributorShow,
                                           ItemAppType = a.ItemAppType,
                                       }).OrderByDescending(s => s.CurrentStock).Skip((page - 1) * list).Take(list).ToList();
                }
                else if (status == "Item Limit")
                {

                    obj.total_count = db.itemMasters.Where(x => x.WarehouseId == Warehouse_id && x.Deleted == false && x.CompanyId == CompanyId).Count();

                    obj.ordermaster = (from a in db.itemMasters
                                       where (a.Deleted == false && a.CompanyId == CompanyId && a.WarehouseId == Warehouse_id)
                                       join b in db.ItemMasterCentralDB on a.SellingSku equals b.SellingSku
                                       join c in db.DbCurrentStock.Where(x => x.WarehouseId == Warehouse_id) on a.Number equals c.ItemNumber
                                       join d in db.ItemLimitMasterDB on a.ItemMultiMRPId equals d.ItemMultiMRPId
                                       where (d.IsItemLimit == true && d.ItemlimitQty > 0 && d.WarehouseId == a.WarehouseId)
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
                                           NetPurchasePrice = a.NetPurchasePrice,
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
                                           DepoId = a.DepoId,
                                           BuyerId = a.BuyerId,
                                           BuyerName = a.BuyerName,
                                           ItemlimitQty = d.ItemlimitQty,
                                           IsReplaceable = a.IsReplaceable,
                                           DistributionPrice = a.DistributionPrice,
                                           DistributorShow = a.DistributorShow,
                                           ItemAppType = a.ItemAppType,
                                       }).OrderByDescending(s => s.CurrentStock).Skip((page - 1) * list).Take(list).ToList();
                }

                else
                {
                    obj.total_count = db.itemMasters.Where(x => x.WarehouseId == Warehouse_id && x.Deleted == false && x.CompanyId == CompanyId).Count();
                    obj.ordermaster = (from a in db.itemMasters
                                       where (a.Deleted == false && a.CompanyId == CompanyId && a.WarehouseId == Warehouse_id)
                                       join b in db.ItemMasterCentralDB on a.SellingSku equals b.SellingSku
                                       join c in db.DbCurrentStock.Where(x => x.WarehouseId == Warehouse_id) on a.Number equals c.ItemNumber
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
                                           NetPurchasePrice = a.NetPurchasePrice,
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
                                           DepoId = a.DepoId,
                                           BuyerId = a.BuyerId,
                                           BuyerName = a.BuyerName,
                                           IsReplaceable = a.IsReplaceable,
                                           DistributionPrice = a.DistributionPrice,
                                           DistributorShow = a.DistributorShow,
                                           ItemAppType = a.ItemAppType,
                                       }).OrderByDescending(s => s.ItemId).Skip((page - 1) * list).Take(list).ToList();
                }

                if (obj.ordermaster != null)
                {
                    var itemList = obj.ordermaster as List<ItemMasterDTO>;

                    itemList.ForEach(x => x.ItemLimitId = db.ItemLimitMasterDB.FirstOrDefault(z => z.ItemNumber == x.Number && z.WarehouseId == x.WarehouseId && z.ItemMultiMRPId == x.ItemMultiMRPId)?.Id);
                    obj.ordermaster = itemList;
                }

                return obj;
            }
        }



        #region Product Details For Retailer App Api By Anushka
        /// <summary>
        /// Product Details For Retailer App Api(05/11/2019) 
        /// </summary>
        /// <param name="itemshare"></param>
        /// <returns></returns>
        [Route("ProductDetails")]
        [HttpPost]
        public HttpResponseMessage Itemshare(Itemshare itemshare)
        {
            using (AuthContext context = new AuthContext())
            {
                //var item = Itemshare.ItemId;
                try
                {

                    logger.Info("favourite ID : {0} , Company Id : {1}");

                    ProductDetails item = new ProductDetails();
                    //foreach (var aa in item)
                    //{
                    try
                    {
                        //List<int> ids = item.Select(x => x.ItemId).ToList();
                        var newdata = (from a in context.itemMasters
                                       where (a.Deleted == false && a.active == true && a.Number == itemshare.Number && a.ItemMultiMRPId == itemshare.ItemMultiMRPId && a.WarehouseId == itemshare.WarehouseId)
                                       join b in context.ItemMasterCentralDB on a.SellingSku equals b.SellingSku
                                       let limit = context.ItemLimitMasterDB.Where(p2 => a.ItemMultiMRPId == p2.ItemMultiMRPId && a.Number == p2.ItemNumber && a.WarehouseId == p2.WarehouseId).FirstOrDefault()
                                       select new ProductDetails
                                       {
                                           WarehouseId = a.WarehouseId,
                                           IsItemLimit = limit != null ? limit.IsItemLimit : false,
                                           ItemlimitQty = limit != null && limit.IsItemLimit ? limit.ItemlimitQty : 0,
                                           CompanyId = a.CompanyId,
                                           Categoryid = b.Categoryid,
                                           Discount = b.Discount,
                                           ItemId = a.ItemId,
                                           Number = b.Number,
                                           itemname = b.itemname,
                                           LogoUrl = b.LogoUrl,
                                           MinOrderQty = b.MinOrderQty,
                                           price = a.price,
                                           SubCategoryId = b.SubCategoryId,
                                           SubsubCategoryid = b.SubsubCategoryid,
                                           TotalTaxPercentage = b.TotalTaxPercentage,
                                           SellingUnitName = b.SellingUnitName,
                                           SellingSku = b.SellingSku,
                                           UnitPrice = a.UnitPrice,
                                           HindiName = b.HindiName,
                                           VATTax = b.VATTax,
                                           active = a.active,
                                           marginPoint = a.marginPoint,
                                           promoPerItems = a.promoPerItems,
                                           NetPurchasePrice = a.NetPurchasePrice,
                                           IsOffer = a.IsOffer,
                                           Deleted = a.Deleted,
                                           ItemMultiMRPId = b.ItemMultiMRPId,
                                           BillLimitQty = a.BillLimitQty
                                       }).OrderByDescending(x => x.Number).FirstOrDefault();

                        try
                        {
                            if (newdata != null)
                            {
                                /// Dream Point Logic && Margin Point
                                int? MP, PP;
                                double xPoint = xPointValue * 10;
                                //Customer (0.2 * 10=1)
                                if (newdata.promoPerItems.Equals(null) && newdata.promoPerItems == null)
                                {
                                    PP = 0;
                                }
                                else
                                {
                                    PP = newdata.promoPerItems;
                                }
                                if (newdata.marginPoint.Equals(null) && newdata.promoPerItems == null)
                                {
                                    MP = 0;
                                }
                                else
                                {
                                    double WithTaxNetPurchasePrice = Math.Round(newdata.NetPurchasePrice * (1 + (newdata.TotalTaxPercentage / 100)), 3);//With tax
                                    MP = Convert.ToInt32((newdata.UnitPrice - WithTaxNetPurchasePrice) * xPoint); // (UnitPrice-NPP withtax) * By xpoint 
                                }
                                if (PP > 0 && MP > 0)
                                {
                                    int? PP_MP = PP + MP;
                                    newdata.dreamPoint = PP_MP;
                                }
                                else if (MP > 0)
                                {
                                    newdata.dreamPoint = MP;
                                }
                                else if (PP > 0)
                                {
                                    newdata.dreamPoint = PP;
                                }
                                else
                                {
                                    newdata.dreamPoint = 0;
                                }
                                // Margin % On app site logic ((MRP-UnitPrice)*100)/UnitPrice
                                if (newdata.price > newdata.UnitPrice)
                                {
                                    newdata.marginPoint = ((newdata.price - newdata.UnitPrice) * 100) / newdata.UnitPrice;//MP;  we replce marginpoint value by margin for app here 
                                }
                                else
                                {
                                    newdata.marginPoint = 0;
                                }
                            }
                        }
                        catch { }

                        item = newdata;

                    }
                    catch (Exception ex)
                    {
                        logger.Error(ex.Message);
                    }
                    //}
                    return Request.CreateResponse(HttpStatusCode.OK, item);
                }
                catch (Exception ex)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex.Message);
                }
            }
        }
        #endregion


        #region  Bulk update MRP Sensitive

        #region To  Get List of All Warehouse Stock
        /// <summary>
        /// Created by Harry : To  Get List of All Warehouse Stock
        /// </summary>
        /// <param name="Number"></param>
        /// <returns></returns>
        [Route("GetWarehousesStock")]
        [HttpGet]
        public async Task<List<warehouseSensitiveitemlist>> GetMRPSensitiveWarehouseStock(string Number)
        {
            var wl = new List<warehouseSensitiveitemlist>();
            List<MRPSensitiveWarehouseStockDTO> _result = new List<MRPSensitiveWarehouseStockDTO>();
            using (var context = new AuthContext())
            {

                if (Number != null)
                {
                    List<Object> parameters = new List<object>();
                    string sqlquery = "exec GetMRPSensitiveWarehouseStock";
                    parameters.Add(new SqlParameter("@ItemNumber", Number));
                    sqlquery = sqlquery + " @ItemNumber";
                    _result = await context.Database.SqlQuery<MRPSensitiveWarehouseStockDTO>(sqlquery, parameters.ToArray()).ToListAsync();
                }

                wl = _result.GroupBy(x => new
                {
                    x.WarehouseId,
                    x.WarehouseName
                }).Select(item => new warehouseSensitiveitemlist
                {
                    WarehouseId = item.Key.WarehouseId,
                    WarehouseName = item.Key.WarehouseName,
                    Sensitiveitemlist = item.Select(a => new SensitiveItemMRPList
                    {
                        CurrentInventory = a.CurrentInventory,
                        ItemMultiMRPId = a.ItemMultiMRPId,
                        MRP = a.MRP,
                        ItemName = a.ItemName,
                        ItemNumber = a.ItemNumber,
                        WarehouseId = a.WarehouseId,
                        WarehouseName = a.WarehouseName,
                        StockId = a.StockId,
                        SelectedMRP = item.Count() <= 1 ? true : false
                    }).ToList()
                }).ToList();

                return wl;
                //  return _result;
            }
        }
        #endregion

        /// <summary>
        /// Created by Harry : Update MRP Sensitive Stock and Item
        /// </summary>
        /// <param name="List"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("MRPSensitiveWarehouseStock")]
        public async Task<bool> UpdateMRPSensitivStockandItem(List<MRPSensitiveWarehouseStockDTO> MRPSensitiveCollection)
        {
            bool _status = false;
            var identity = User.Identity as ClaimsIdentity;
            int compid = 0, userid = 0;
            // Access claims
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))
                compid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);

            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

            System.Transactions.TransactionOptions option = new System.Transactions.TransactionOptions();
            option.IsolationLevel = System.Transactions.IsolationLevel.RepeatableRead;
            option.Timeout = TimeSpan.FromSeconds(90);
            using (var dbContextTransaction = new TransactionScope(TransactionScopeOption.Required, option))
            {
                using (var context = new AuthContext())
                {
                    List<OutDc> Outitems = new List<OutDc>();
                    People people = context.Peoples.Where(q => q.PeopleID == userid && q.Active == true).SingleOrDefault();
                    if (people != null && MRPSensitiveCollection != null && MRPSensitiveCollection.Count > 0)
                    {
                        var _itemNumberList = MRPSensitiveCollection.Where(y => y.IsSelected).Select(x => x.ItemNumber).Distinct().ToList();

                        var SelectedStockList = MRPSensitiveCollection.Where(y => y.IsSelected).ToList();

                        var _itemMasterCentralList = await context.ItemMasterCentralDB.Where(x => _itemNumberList.Contains(x.Number) && x.Deleted == false).ToListAsync();
                        var _itemMasterList = await context.itemMasters.Where(x => _itemNumberList.Contains(x.Number) && x.Deleted == false).ToListAsync();
                        var _currentStockList = await context.DbCurrentStock.Where(x => _itemNumberList.Contains(x.ItemNumber) && x.Deleted == false).ToListAsync();
                        var result = UpdateItemAndStock(SelectedStockList, _itemMasterCentralList, _itemMasterList, _currentStockList, people, context, dbContextTransaction);
                        if (!result.Any(x => x.result == false))
                        {
                            dbContextTransaction.Complete();
                            #region Insert in FIFO
                            if (ConfigurationManager.AppSettings["LiveFIFO"] == "1")
                            {
                                List<OutDc> items = result.Where(x => x.Qty > 0).Select(x => new OutDc
                                {
                                    ItemMultiMrpId = x.SourceItemMultiMRPId,
                                    WarehouseId = x.WarehouseId,
                                    Destination = "Stock Transfer Out",
                                    CreatedDate = indianTime,
                                    ObjectId = 0,
                                    Qty = x.Qty,
                                    SellingPrice = 0,
                                    InMrpId = x.DestinationItemMultiMRPId

                                }).ToList();
                                foreach (var it in items)
                                {
                                    RabbitMqHelper rabbitMqHelper = new RabbitMqHelper();
                                    rabbitMqHelper.Publish("MrpOutTransfer", it);
                                }
                            }
                            #endregion
                            _status = true;
                            return _status;
                        }
                        else
                        {
                            dbContextTransaction.Dispose();
                            _status = false;
                            return _status;
                        }

                    }

                }
            }
            return _status;
        }

        public List<INOutOnMultiMRPTransferDc> UpdateItemAndStock(List<MRPSensitiveWarehouseStockDTO> SelectedStockList, List<ItemMasterCentral> _itemMasterCentralList, List<ItemMaster> _itemMasterList, List<CurrentStock> _currentStockList, People people, AuthContext context, TransactionScope dbContextTransaction)
        {

            List<INOutOnMultiMRPTransferDc> result = new List<INOutOnMultiMRPTransferDc>();

            List<ItemMaster> _updateItemMaster = new List<ItemMaster>();//update itemmaster
            List<ItemMasterCentral> _updateItemMasterCentral = new List<ItemMasterCentral>();//update ItemMasterCentral
            //List<CurrentStock> _updateCurrentStock = new List<CurrentStock>();//update CurrentStock
            //List<CurrentStockHistory> _AddCurrentStockHistory = new List<CurrentStockHistory>();//update CurrentStockHistory
            List<MrpStockTransfer> AddMrpStockTransfer = new List<MrpStockTransfer>();
            foreach (var stock in SelectedStockList)
            {
                MultiStockHelper<OnMultiMRPTransferDc> MultiStockHelpers = new MultiStockHelper<OnMultiMRPTransferDc>();
                List<OnMultiMRPTransferDc> StockList = new List<OnMultiMRPTransferDc>();
                CurrentStock CurrentStock = _currentStockList.Where(x => x.StockId == stock.StockId).SingleOrDefault();
                if (CurrentStock != null)
                {
                    List<ItemMasterCentral> ItemMasterCentralList = _itemMasterCentralList.Where(x => x.Number == stock.ItemNumber).ToList();
                    List<ItemMaster> itemMasterList = _itemMasterList.Where(x => x.Number == stock.ItemNumber && x.WarehouseId == stock.WarehouseId).ToList();
                    foreach (var central in ItemMasterCentralList)
                    {
                        central.ItemMultiMRPId = CurrentStock.ItemMultiMRPId;
                        central.price = CurrentStock.MRP;
                        central.IsSensitive = true;//if one of them not true, thats why we used

                        if (central.IsSensitive == true && central.IsSensitiveMRP == false)
                        {
                            central.itemname = central.itemBaseName + " " + central.UnitofQuantity + " " + central.UOM; //item display name 
                        }
                        central.SellingUnitName = central.itemname + " " + central.MinOrderQty + "Unit";//item selling unit name
                        central.PurchaseUnitName = central.itemname + " " + central.PurchaseMinOrderQty + "Unit";//item PurchaseUnitName name
                        _updateItemMasterCentral.Add(central);
                    }
                    var centralitem = _updateItemMasterCentral[0];
                    foreach (var itemmaster in itemMasterList)
                    {
                        itemmaster.ItemMultiMRPId = stock.ItemMultiMRPId;

                        if (centralitem.IsSensitive == true && centralitem.IsSensitiveMRP == false)
                        {
                            itemmaster.itemname = centralitem.itemBaseName + " " + centralitem.UnitofQuantity + " " + centralitem.UOM; //item display name 
                        }
                        itemmaster.SellingUnitName = itemmaster.itemname + " " + itemmaster.MinOrderQty + "Unit";//item selling unit name
                        itemmaster.PurchaseUnitName = itemmaster.itemname + " " + itemmaster.PurchaseMinOrderQty + "Unit";//item PurchaseUnitName name
                        _updateItemMaster.Add(itemmaster);
                    }
                    var HUbitem = _updateItemMaster[0];
                    List<CurrentStock> SameNumberotherStock = _currentStockList.Where(x => x.ItemNumber == CurrentStock.ItemNumber && x.WarehouseId == CurrentStock.WarehouseId && x.StockId != CurrentStock.StockId).ToList();
                    if (SameNumberotherStock != null && SameNumberotherStock.Count > 0)
                    {
                        foreach (var Otherstock in SameNumberotherStock)
                        {
                            if (Otherstock.CurrentInventory > 0)
                            {
                                StockList.Add(new OnMultiMRPTransferDc
                                {
                                    WarehouseId = Otherstock.WarehouseId,
                                    SourceItemMultiMRPId = Otherstock.ItemMultiMRPId,
                                    DestinationItemMultiMRPId = CurrentStock.ItemMultiMRPId,
                                    Qty = Otherstock.CurrentInventory,
                                    UserId = people.PeopleID,
                                    ManualReason = "MRPSensitive",// "- (Transfer In MRP Sensitive)",
                                });
                                result.Add(new INOutOnMultiMRPTransferDc
                                {
                                    WarehouseId = Otherstock.WarehouseId,
                                    SourceItemMultiMRPId = Otherstock.ItemMultiMRPId,
                                    DestinationItemMultiMRPId = CurrentStock.ItemMultiMRPId,
                                    Qty = Otherstock.CurrentInventory,
                                    result = true,
                                });

                            }
                        }
                        if (StockList.Any())
                        {
                            bool res = MultiStockHelpers.MakeEntry(StockList, "Stock_OnMultiMRPTransfer", context, dbContextTransaction);
                            if (!res)
                            {
                                result.ForEach(x =>
                                x.result = false
                               );
                                return result;
                            }
                        }
                    }
                }
            }
            foreach (var ItemMaster in _updateItemMaster)
            {
                ItemMaster.UpdatedDate = indianTime;
                ItemMaster.UpdateBy = people.DisplayName;
                context.Entry(ItemMaster).State = EntityState.Modified;
            }
            foreach (var updateItemMasterCentral in _updateItemMasterCentral)
            {
                updateItemMasterCentral.UpdatedDate = indianTime;
                updateItemMasterCentral.UpdateBy = people.DisplayName;
                context.Entry(updateItemMasterCentral).State = EntityState.Modified;
            }

            //foreach (var updatestockitem in _updateCurrentStock)
            //{
            //    //updatestockitem.UpdatedDate = indianTime;
            //    //updatestockitem.UpdateBy = people.DisplayName;
            //    //context.Entry(updatestockitem).State = EntityState.Modified;
            //}
            //  context.CurrentStockHistoryDb.AddRange(_AddCurrentStockHistory);
            //if (AddMrpStockTransfer != null && AddMrpStockTransfer.Any())
            //{
            //    context.MrpStockTransfers.AddRange(AddMrpStockTransfer);
            //}
            if (context.Commit() > 0)
            {
                return result;
            }
            else
            {
                return result;
            }
        }
        #endregion



        #region GetWarehouseItemcurrentstock by anushka (29/01/2020)

        [Route("GetWarehouseItemcurrentstock")]
        [HttpGet]
        public HttpResponseMessage GetWarehouseItemcurrentstock(int WarehouseId)
        {
            using (var db = new AuthContext())
            {
                try
                {
                    if (WarehouseId > 0)
                    {
                        var Item = db.DbCurrentStock.Where(x => x.WarehouseId == WarehouseId && x.Deleted == false).ToList();

                        return Request.CreateResponse(HttpStatusCode.OK, Item);
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.OK, "Null");
                    }

                }
                catch (Exception ex)
                {

                    return Request.CreateResponse(HttpStatusCode.OK, "Null");

                }
            }
        }
        #endregion

        public int GenerateUniqueNumber()
        {
            int number = Convert.ToInt32(String.Format("{0:d9}", (DateTime.Now.Ticks / 10) % 1000000000));
            return number;
        }

        #region add item in central
        public ItemMasterCentral AddItemMaster(ItemMasterCentral itemmaster, AuthContext context)
        {
            var identity = User.Identity as ClaimsIdentity;
            int compid = 0, userid = 0;

            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))
                compid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);

            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);


            //string Barcode = "";
            People people = context.Peoples.Where(x => x.PeopleID == userid).SingleOrDefault();
            List<ItemMasterCentral> itemMasterexits = context.ItemMasterCentralDB.Where(c => c.Number.Trim().Equals(itemmaster.Number.Trim()) && c.CompanyId == itemmaster.CompanyId).ToList();
            ItemMasterCentral Check = context.ItemMasterCentralDB.Where(c => c.SellingSku.Trim().Equals(itemmaster.SellingSku.Trim()) && c.CompanyId == itemmaster.CompanyId && c.Deleted == false).FirstOrDefault();
            Category category = new Category();
            SubCategory subcategory = new SubCategory();
            SubsubCategory Subsubcategory = new SubsubCategory();
            TaxGroupDetails taxgroup = new TaxGroupDetails();
            TaxGroup Tg = new TaxGroup();
            double TotalTax = 0;
            double TotalCessTax = 0;
            // this tax group
            TaxGroupDetails Cessgroup = new TaxGroupDetails();
            TaxGroup CessTg = new TaxGroup();



            if (itemMasterexits.Count > 0)
            {

                ItemMasterCentral Createditem = itemMasterexits[0];
                TotalTax = Createditem.TotalTaxPercentage;//
                TotalCessTax = Createditem.TotalCessPercentage;//
                taxgroup.GruopID = Createditem.GruopID;//
                Tg.TGrpName = Createditem.TGrpName;
                Cessgroup.GruopID = Createditem.CessGrpID ?? 0;
                CessTg.TGrpName = Createditem.CessGrpName;
                category = context.Categorys.Where(x => x.Categoryid == Createditem.Categoryid && x.Deleted == false).Select(x => x).FirstOrDefault();
                subcategory = context.SubCategorys.Where(x => x.SubCategoryId == Createditem.SubCategoryId && x.Deleted == false).Select(x => x).FirstOrDefault();
                Subsubcategory = context.SubsubCategorys.Where(x => x.SubsubCategoryid == Createditem.SubsubCategoryid && x.Deleted == false).Select(x => x).FirstOrDefault();

                //Barcode = itemMasterexits.FirstOrDefault(x => x.Barcode != null).Barcode;
            }
            else
            {
                category = context.Categorys.Where(x => x.Categoryid == itemmaster.Categoryid && x.Deleted == false).Select(x => x).FirstOrDefault();
                subcategory = context.SubCategorys.Where(x => x.SubCategoryId == itemmaster.SubCategoryId && x.Deleted == false).Select(x => x).FirstOrDefault();
                Subsubcategory = context.SubsubCategorys.Where(x => x.SubsubCategoryid == itemmaster.SubsubCategoryid && x.Deleted == false).Select(x => x).FirstOrDefault();
                try
                {
                    if (itemmaster.CompanyId > 0)
                    {
                        taxgroup = context.DbTaxGroupDetails.Where(x => x.GruopID == itemmaster.GruopID && x.CompanyId == itemmaster.CompanyId).Select(x => x).FirstOrDefault();
                        if (taxgroup != null)
                        {
                            itemmaster.GruopID = taxgroup.GruopID;
                        }
                        Tg = context.DbTaxGroup.Where(x => x.GruopID == itemmaster.GruopID && x.Deleted == false && x.CompanyId == itemmaster.CompanyId).Select(x => x).FirstOrDefault();
                        if (Tg != null) { itemmaster.TGrpName = Tg.TGrpName; }
                        List<TaxGroupDetails> TaxG = context.DbTaxGroupDetails.Where(x => x.GruopID == itemmaster.GruopID && x.CompanyId == itemmaster.CompanyId).Select(x => x).ToList();
                        if (TaxG.Count != 0)
                        {
                            foreach (var i in TaxG)
                            {
                                TotalTax += i.TPercent;
                            }
                        }
                    }

                }
                catch (Exception sd) { }
                try
                {
                    if (itemmaster.CessGrpID > 0 && itemmaster.CompanyId > 0)
                    {
                        Cessgroup = context.DbTaxGroupDetails.Where(x => x.GruopID == itemmaster.CessGrpID && x.CompanyId == itemmaster.CompanyId).Select(x => x).FirstOrDefault();
                        CessTg = context.DbTaxGroup.Where(x => x.GruopID == itemmaster.CessGrpID && x.Deleted == false && x.CompanyId == itemmaster.CompanyId).Select(x => x).FirstOrDefault();
                        List<TaxGroupDetails> CesstaxG = context.DbTaxGroupDetails.Where(x => x.GruopID == itemmaster.CessGrpID && x.CompanyId == itemmaster.CompanyId).Select(x => x).ToList();
                        if (CesstaxG.Count != 0)
                        {
                            foreach (var i in CesstaxG)
                            {
                                TotalCessTax += i.TPercent;
                            }
                        }
                    }

                }
                catch (Exception swdr)
                { }
            }
            List<Warehouse> warehouses = context.Warehouses.Where(x => x.Deleted == false && x.CompanyId == itemmaster.CompanyId && x.IsKPP == false).ToList();
            if (Check == null)
            {
                ///first record in Central for MRP
                ItemMultiMRP MultiMRPitem = new ItemMultiMRP();
                ItemMultiMRP recordExits = context.ItemMultiMRPDB.Where(x => x.ItemNumber == itemmaster.Number && x.Deleted == false && x.MRP == itemmaster.price).FirstOrDefault();
                if (recordExits != null)
                {
                    MultiMRPitem = recordExits;
                }
                else
                {
                    MultiMRPitem = context.ItemMultiMRPDB.Where(x => x.ItemNumber == itemmaster.Number && x.ItemMultiMRPId == itemmaster.ItemMultiMRPId).SingleOrDefault();
                    if (MultiMRPitem == null)
                    {
                        ItemMultiMRP Immrp = new ItemMultiMRP();
                        Immrp.CompanyId = itemmaster.CompanyId;
                        Immrp.itemname = itemmaster.itemname;
                        Immrp.itemBaseName = itemmaster.itemBaseName;
                        Immrp.ItemNumber = itemmaster.Number;
                        Immrp.MRP = itemmaster.price;
                        Immrp.UnitofQuantity = itemmaster.UnitofQuantity;
                        Immrp.UOM = itemmaster.UOM;
                        Immrp.CreatedDate = indianTime;
                        Immrp.UpdatedDate = indianTime;
                        Immrp.CompanyStockCode = itemmaster.CompanyStockCode;

                        context.ItemMultiMRPDB.Add(Immrp);
                        context.Commit();
                        MultiMRPitem = Immrp;
                    }
                }

                itemmaster.MRP = itemmaster.price;

                itemmaster.TotalTaxPercentage = TotalTax;
                //cesss
                try
                {
                    itemmaster.CessGrpID = Cessgroup.GruopID;
                    itemmaster.CessGrpName = CessTg.TGrpName;
                    itemmaster.TotalCessPercentage = TotalCessTax;
                }
                catch (Exception asdf)
                {
                }
                itemmaster.BaseCategoryid = category.BaseCategoryId;
                itemmaster.LogoUrl = itemmaster.LogoUrl;
                itemmaster.UpdatedDate = indianTime;
                itemmaster.CreatedDate = indianTime;
                itemmaster.CategoryName = category.CategoryName;
                itemmaster.Categoryid = category.Categoryid;
                itemmaster.SubcategoryName = subcategory.SubcategoryName;
                itemmaster.SubCategoryId = subcategory.SubCategoryId;
                itemmaster.SubsubcategoryName = Subsubcategory.SubsubcategoryName;
                itemmaster.SubsubCategoryid = Subsubcategory.SubsubCategoryid;
                itemmaster.SubSubCode = Subsubcategory.Code;

                if (itemmaster.Margin > 0)
                {
                    var rs = context.RetailerShareDb/*.Where(r => r.cityid == itemmaster.Cityid)*/.FirstOrDefault();
                    if (rs != null)
                    {
                        var cf = context.RPConversionDb.FirstOrDefault();
                        try
                        {
                            double mv = (itemmaster.PurchasePrice * (itemmaster.Margin / 100) * (rs.share / 100) * cf.point);
                            var value = Math.Round(mv, MidpointRounding.AwayFromZero);
                            itemmaster.marginPoint = Convert.ToInt32(value);
                        }
                        catch (Exception ex)
                        {
                            logger.Error(ex.Message);
                        }
                    }
                }

                //Display Name binding
                itemmaster.itemBaseName = itemmaster.itemBaseName;
                itemmaster.itemname = itemmaster.itemname;
                itemmaster.ItemMultiMRPId = MultiMRPitem.ItemMultiMRPId;
                itemmaster.CreatedBy = userid;
                itemmaster.weight = itemmaster.weight;
                itemmaster.BaseScheme = itemmaster.BaseScheme;
                itemmaster.PTR = itemmaster.PTR;

                //Add Season Config
                itemmaster.SeasonId = itemmaster.SeasonId;
                //
                #region Batch Code
                //var param = new SqlParameter("ItemNumber", itemmaster.Number);
                //var param1 = new SqlParameter("Barcode", itemmaster.Barcode);
                //bool IsBarcodeExistsToOther = context.Database.SqlQuery<bool>("exec BatchCode.IsBarcodeExistsToOther @ItemNumber, @Barcode", param, param1).FirstOrDefault();
                //if (!IsBarcodeExistsToOther)
                //{

                //    var param2 = new SqlParameter("ItemNumber", itemmaster.Number);
                //    var param21 = new SqlParameter("Barcode", itemmaster.Barcode);
                //    int ResultUpdateBarcode = context.Database.ExecuteSqlCommand("exec BatchCode.UpdateBarcode @ItemNumber, @Barcode ", param2, param21);
                //}
                //else
                //{
                //    itemmaster.Barcode = Barcode;
                //}
                #endregion
                context.ItemMasterCentralDB.Add(itemmaster);
                context.Commit();
                foreach (var o in warehouses)
                {

                    ItemMaster it = new ItemMaster();
                    if (taxgroup != null) { it.GruopID = taxgroup.GruopID; it.TGrpName = Tg.TGrpName; it.TotalTaxPercentage = TotalTax; }
                    if (Cessgroup != null) { it.CessGrpID = Cessgroup.GruopID; it.CessGrpName = CessTg.TGrpName; it.TotalCessPercentage = TotalCessTax; }
                    it.CatLogoUrl = Subsubcategory.LogoUrl;
                    it.WarehouseId = o.WarehouseId;
                    it.WarehouseName = o.WarehouseName;
                    it.Cityid = o.Cityid;
                    it.CityName = o.CityName;
                    it.WarehouseName = o.WarehouseName;
                    it.BaseCategoryid = category.BaseCategoryId;
                    it.LogoUrl = itemmaster.LogoUrl;
                    it.UpdatedDate = indianTime;
                    it.CreatedDate = indianTime;
                    it.CategoryName = itemmaster.CategoryName;
                    it.Categoryid = itemmaster.Categoryid;
                    it.SubcategoryName = itemmaster.SubcategoryName;
                    it.SubCategoryId = itemmaster.SubCategoryId;
                    it.SubsubcategoryName = itemmaster.SubsubcategoryName;
                    it.SubsubCategoryid = itemmaster.SubsubCategoryid;
                    it.SubSubCode = Subsubcategory.Code;
                    it.itemcode = itemmaster.itemcode;
                    it.marginPoint = itemmaster.marginPoint;
                    it.Number = itemmaster.Number;
                    it.PramotionalDiscount = itemmaster.PramotionalDiscount;
                    it.MinOrderQty = itemmaster.MinOrderQty;
                    it.NetPurchasePrice = itemmaster.NetPurchasePrice;
                    it.GeneralPrice = itemmaster.GeneralPrice;
                    it.price = itemmaster.price;
                    it.promoPerItems = itemmaster.promoPerItems;
                    it.promoPoint = itemmaster.promoPoint;
                    it.PurchaseMinOrderQty = itemmaster.PurchaseMinOrderQty;
                    it.PurchasePrice = itemmaster.PurchasePrice;
                    it.PurchaseSku = itemmaster.PurchaseSku;
                    it.PurchaseUnitName = itemmaster.PurchaseUnitName;
                    it.SellingSku = itemmaster.SellingSku;
                    it.SellingUnitName = itemmaster.SellingUnitName;
                    it.SizePerUnit = itemmaster.SizePerUnit;
                    it.UnitPrice = itemmaster.UnitPrice;
                    it.VATTax = itemmaster.VATTax;
                    it.HSNCode = itemmaster.HSNCode;
                    it.HindiName = itemmaster.HindiName;
                    it.CompanyId = itemmaster.CompanyId;
                    it.Reason = itemmaster.Reason;
                    it.DefaultBaseMargin = itemmaster.DefaultBaseMargin;
                    it.Deleted = false;
                    it.active = false;
                    it.itemname = itemmaster.itemname;
                    it.itemBaseName = itemmaster.itemBaseName;
                    it.UOM = itemmaster.UOM;
                    it.UnitofQuantity = itemmaster.UnitofQuantity;
                    it.ItemMultiMRPId = MultiMRPitem.ItemMultiMRPId;
                    it.IsSensitive = itemmaster.IsSensitive;
                    it.IsSensitiveMRP = itemmaster.IsSensitiveMRP;
                    it.ShelfLife = itemmaster.ShelfLife;
                    it.IsReplaceable = itemmaster.IsReplaceable;
                    it.BomId = itemmaster.BomId;
                    it.Type = itemmaster.Type;
                    it.CreatedBy = userid;
                    it.CompanyCode = itemmaster.CompanyCode;
                    it.MRP = itemmaster.price;

                    context.itemMasters.Add(it);
                    context.Commit();
                    CurrentStock cntstock = new CurrentStock();
                    cntstock = context.DbCurrentStock.Where(x => x.ItemNumber == itemmaster.Number && x.WarehouseId == o.WarehouseId && x.CompanyId == itemmaster.CompanyId && x.ItemMultiMRPId == itemmaster.ItemMultiMRPId && x.Deleted == false).SingleOrDefault();
                    //if (itemmaster.IsSensitiveMRP)
                    //{
                    //    cntstock = DbCurrentStock.Where(x => x.ItemNumber == itemmaster.Number && x.WarehouseId == o.WarehouseId && x.CompanyId == itemmaster.CompanyId && x.Deleted == false).FirstOrDefault();
                    //}
                    //else
                    //{
                    //    cntstock = DbCurrentStock.Where(x => x.ItemNumber == itemmaster.Number && x.WarehouseId == o.WarehouseId && x.CompanyId == itemmaster.CompanyId && x.ItemMultiMRPId == itemmaster.ItemMultiMRPId && x.Deleted == false).SingleOrDefault();
                    //}
                    if (cntstock == null)
                    {
                        CurrentStock newCstk = new CurrentStock();
                        newCstk.ItemNumber = itemmaster.Number;
                        newCstk.WarehouseId = o.WarehouseId;
                        newCstk.WarehouseName = o.WarehouseName;
                        newCstk.CompanyId = itemmaster.CompanyId;
                        newCstk.CurrentInventory = 0;
                        newCstk.CreationDate = indianTime;
                        newCstk.UpdatedDate = indianTime;
                        // Multimrp
                        newCstk.MRP = itemmaster.price;
                        newCstk.UnitofQuantity = itemmaster.UnitofQuantity;
                        newCstk.UOM = itemmaster.UOM;
                        newCstk.ItemMultiMRPId = MultiMRPitem.ItemMultiMRPId;
                        newCstk.itemname = itemmaster.itemname;
                        newCstk.itemBaseName = itemmaster.itemBaseName;
                        context.DbCurrentStock.Add(newCstk);
                        context.Commit();

                        CurrentStockHistory Oss = new CurrentStockHistory();
                        Oss.StockId = newCstk.StockId;
                        Oss.ItemNumber = newCstk.ItemNumber;

                        Oss.TotalInventory = newCstk.CurrentInventory;
                        Oss.WarehouseName = newCstk.WarehouseName;
                        Oss.Warehouseid = newCstk.WarehouseId;
                        Oss.CompanyId = newCstk.CompanyId;
                        Oss.CreationDate = indianTime;
                        // Multimrp
                        Oss.MRP = newCstk.MRP;

                        Oss.UnitofQuantity = newCstk.UnitofQuantity;
                        Oss.UOM = newCstk.UOM;
                        Oss.ItemMultiMRPId = newCstk.ItemMultiMRPId;
                        Oss.itemname = newCstk.itemname;
                        Oss.itemBaseName = newCstk.itemBaseName;
                        Oss.userid = people.PeopleID;
                        Oss.UserName = people.DisplayName;
                        context.CurrentStockHistoryDb.Add(Oss);
                        int idd = context.Commit();
                    }
                    else
                    {
                        cntstock.ItemNumber = itemmaster.Number;
                        cntstock.WarehouseId = o.WarehouseId;
                        cntstock.WarehouseName = o.WarehouseName;
                        cntstock.CompanyId = itemmaster.CompanyId;
                        cntstock.Deleted = false;
                        cntstock.UpdatedDate = indianTime;
                        // Multimrp
                        cntstock.MRP = itemmaster.price;
                        cntstock.UnitofQuantity = itemmaster.UnitofQuantity;
                        cntstock.UOM = itemmaster.UOM;
                        cntstock.ItemMultiMRPId = MultiMRPitem.ItemMultiMRPId;
                        cntstock.itemname = itemmaster.itemname;
                        cntstock.itemBaseName = itemmaster.itemBaseName;

                        //DbCurrentStock.Attach(cntstock);
                        context.Entry(cntstock).State = EntityState.Modified;
                        context.Commit();

                        CurrentStockHistory Oss = new CurrentStockHistory();
                        Oss.StockId = cntstock.StockId;
                        Oss.ItemNumber = cntstock.ItemNumber;
                        Oss.TotalInventory = cntstock.CurrentInventory;
                        Oss.WarehouseName = cntstock.WarehouseName;
                        Oss.Warehouseid = cntstock.WarehouseId;
                        Oss.CompanyId = cntstock.CompanyId;
                        Oss.CreationDate = indianTime;
                        Oss.userid = people.PeopleID;
                        Oss.UserName = people.DisplayName;
                        // Multimrp
                        Oss.MRP = cntstock.MRP;
                        Oss.UnitofQuantity = cntstock.UnitofQuantity;
                        Oss.UOM = cntstock.UOM;
                        Oss.ItemMultiMRPId = cntstock.ItemMultiMRPId;
                        Oss.itemname = cntstock.itemname;
                        Oss.itemBaseName = cntstock.itemBaseName;
                        context.CurrentStockHistoryDb.Add(Oss);
                        int idd = context.Commit();
                    }

                    ///***** set all items current stock behalf of all warehouses *****///
                }

                #region Add Clearance/NonSellable Shelf Life of ItemNumber
                var IsExist = context.ClearanceNonsShelfConfigurations.FirstOrDefault(x => x.ItemNumber == itemmaster.Number && x.IsActive == true);
                if (IsExist == null)
                {
                    ClearanceNonSaleableShelfConfiguration obj = new ClearanceNonSaleableShelfConfiguration();
                    obj.CategoryId = category.Categoryid;
                    obj.SubCategoryId = subcategory.SubCategoryId;
                    obj.BrandId = Subsubcategory.SubsubCategoryid;
                    obj.ItemNumber = itemmaster.Number;
                    obj.Status = "Approved";
                    obj.ClearanceShelfLifeFrom = itemmaster.ClearanceFrom;
                    obj.ClearanceShelfLifeTo = itemmaster.ClearanceTo;
                    obj.NonSellShelfLifeFrom = itemmaster.NonsellableFrom;
                    obj.NonSellShelfLifeTo = itemmaster.NonsellableTo;
                    obj.ApprovedBy = 0;
                    obj.RejectedBy = 0;
                    obj.RejectComment = null;
                    obj.CreatedDate = DateTime.Now;
                    obj.ModifiedDate = null;
                    obj.CreatedBy = 0;
                    obj.ModifiedBy = 0;
                    obj.IsActive = true;
                    obj.IsDeleted = false;
                    context.ClearanceNonsShelfConfigurations.Add(obj);
                }
                #endregion

                if (itemmaster.PTR > 0 && itemmaster.BaseScheme > 0)
                {
                    var isExistPTR = context.ItemSchemes.Any(x => x.IsDeleted == false && x.IsActive == true && x.ItemMultiMRPId == itemmaster.ItemMultiMRPId);
                    if (!isExistPTR)
                    {
                        List<ItemScheme> ItemSchemeList = new List<ItemScheme>();
                        foreach (var Cityid in warehouses.Select(x => x.Cityid).Distinct().ToList())
                        {
                            ItemScheme itemScheme = new ItemScheme();
                            itemScheme.Cityid = Cityid;
                            itemScheme.ItemMultiMRPId = itemmaster.ItemMultiMRPId;
                            itemScheme.PTR = itemmaster.PTR ?? 0;
                            itemScheme.BaseScheme = itemmaster.BaseScheme ?? 0;
                            itemScheme.CreatedDate = DateTime.Now;
                            itemScheme.IsActive = true;
                            itemScheme.IsDeleted = false;
                            itemScheme.CreatedBy = userid;
                            ItemSchemeList.Add(itemScheme);
                        }
                        context.ItemSchemes.AddRange(ItemSchemeList);
                        context.Commit();
                    }
                }

                // CommonHelper.refreshItemMaster(itemMasters.WarehouseId);

                if (itemmaster.IsTradeItem)
                {
                    AngularJSAuthentication.DataContracts.External.TradeItemMasterDc tradeItem = new AngularJSAuthentication.DataContracts.External.TradeItemMasterDc
                    {
                        BaseCategoryId = itemmaster.BaseCategoryid,
                        CategoryId = itemmaster.Categoryid,
                        SubCategoryId = itemmaster.SubCategoryId,
                        BrandId = itemmaster.SubsubCategoryid,
                        BaseCategoryName = itemmaster.BaseCategoryName,
                        BasePrice = itemmaster.UnitPrice,
                        BrandImagePath = Subsubcategory.LogoUrl,
                        BrandName = itemmaster.SubsubcategoryName,
                        CategoryName = itemmaster.CategoryName,
                        CreatedBy = people.PeopleID,
                        CreatedDate = indianTime,
                        ImagePath = itemmaster.LogoUrl,
                        IsActive = false,
                        IsDelete = false,
                        ItemId = itemmaster.Id,
                        ItemName = itemmaster.itemname,
                        MRP = itemmaster.price,
                        UnitOfMeasurement = itemmaster.UOM,
                        SubCategoryName = itemmaster.SubcategoryName,
                        UnitOfQuantity = itemmaster.UnitofQuantity,
                        HSNCode = itemmaster.HSNCode,

                        TotalTaxPercent = Convert.ToString(itemmaster.TotalTaxPercentage),
                        CGST = Convert.ToString(itemmaster.TotalTaxPercentage / 2),
                        SGST = Convert.ToString(itemmaster.TotalTaxPercentage / 2),
                        CESS = Convert.ToString(itemmaster.TotalCessPercentage)
                    };

                    BackgroundTaskManager.Run(() =>
                    {
                        try
                        {
                            var tradeUrl = ConfigurationManager.AppSettings["TradeURL"] + "/api/TradeItem/Insert";
                            TextFileLogHelper.LogError(tradeUrl, false);
                            using (GenericRestHttpClient<AngularJSAuthentication.DataContracts.External.TradeItemMasterDc, string> memberClient = new GenericRestHttpClient<AngularJSAuthentication.DataContracts.External.TradeItemMasterDc, string>(tradeUrl, "", null))
                            {
                                tradeItem = AsyncContext.Run(() => memberClient.PostAsync(tradeItem));
                            }
                        }
                        catch (Exception ex)
                        {
                            TextFileLogHelper.LogError("Error while saving item in Trade: " + ex.ToString());
                        }
                    });
                }

            }
            return itemmaster;

        }




        #endregion

        [Route("GetItemSchemeData")]
        [HttpPost]
        public List<ItemSchemeData> GetItemSchemeDatas(List<int> CityId, string keyword, int skip, int take)
        {
            List<ItemSchemeData> ItemSchemeDatas = new List<ItemSchemeData>();
            using (var context = new AuthContext())
            {
                if (context.Database.Connection.State != ConnectionState.Open)
                    context.Database.Connection.Open();

                var IdDt = new DataTable();
                IdDt.Columns.Add("IntValue");
                foreach (var item in CityId)
                {
                    var dr = IdDt.NewRow();
                    dr["IntValue"] = item;
                    IdDt.Rows.Add(dr);
                }
                var CidParam = new SqlParameter("CityId", IdDt);
                CidParam.SqlDbType = SqlDbType.Structured;
                CidParam.TypeName = "dbo.intValues";

                var cmd = context.Database.Connection.CreateCommand();
                cmd.CommandText = "[dbo].[GetItemSchemeData]";
                cmd.Parameters.Add(CidParam);
                cmd.Parameters.Add(new SqlParameter("@keyword", keyword));
                cmd.Parameters.Add(new SqlParameter("@Skip", skip));
                cmd.Parameters.Add(new SqlParameter("@Take", take));
                cmd.CommandType = System.Data.CommandType.StoredProcedure;

                // Run the sproc
                var reader = cmd.ExecuteReader();
                ItemSchemeDatas = ((IObjectContextAdapter)context)
                .ObjectContext
                .Translate<ItemSchemeData>(reader).ToList();
            }
            return ItemSchemeDatas;
        }

        [Route("UpdateItemSchemeData")]
        [HttpPost]
        public ItemSchemeResponse UpdateItemSchemeData(List<ItemSchemeData> ItemSchemeData)
        {
            var identity = User.Identity as ClaimsIdentity;
            int userid = 0;

            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
            ItemSchemeResponse itemSchemeResponse = new ItemSchemeResponse();
            using (var context = new AuthContext())
            {
                if (ItemSchemeData.Any(x => !string.IsNullOrEmpty(x.CityName)))
                {
                    var cityNames = ItemSchemeData.Select(x => x.CityName.ToLower()).Distinct();
                    var cities = context.Cities.Where(x => cityNames.Contains(x.CityName.ToLower())).ToList();
                    if (cities.Count() == cityNames.Count())
                    {
                        var cityIds = cities.Select(x => x.Cityid).ToList();
                        var itemmultiMrpIds = ItemSchemeData.Select(x => x.ItemMultiMRPId).ToList();
                        var DbItemSchemes = context.ItemSchemes.Where(x => cityIds.Contains(x.Cityid) && itemmultiMrpIds.Contains(x.ItemMultiMRPId)).ToList();
                        foreach (var item in ItemSchemeData)
                        {
                            if (DbItemSchemes.Any(x => x.ItemMultiMRPId == item.ItemMultiMRPId))
                            {
                                var DbItemScheme = DbItemSchemes.FirstOrDefault(x => x.ItemMultiMRPId == item.ItemMultiMRPId);
                                DbItemScheme.IsActive = true;
                                DbItemScheme.IsDeleted = false;
                                DbItemScheme.ModifiedBy = userid;
                                DbItemScheme.ModifiedDate = DateTime.Now;
                                DbItemScheme.PTR = item.PTR;
                                DbItemScheme.BaseScheme = item.BaseScheme;
                                context.Entry(DbItemScheme).State = EntityState.Modified;
                            }
                            else
                            {
                                var city = cities.FirstOrDefault(x => x.Cityid == item.Cityid);
                                ItemScheme ItemScheme = new ItemScheme
                                {
                                    BaseScheme = item.BaseScheme,
                                    Cityid = city.Cityid,
                                    CreatedBy = userid,
                                    CreatedDate = DateTime.Now,
                                    IsActive = true,
                                    IsDeleted = false,
                                    ItemMultiMRPId = item.ItemMultiMRPId,
                                    PTR = item.PTR
                                };
                                context.ItemSchemes.Add(ItemScheme);
                            }
                        }

                        if (context.Commit() > 0)
                        {
                            itemSchemeResponse.Status = true;
                            itemSchemeResponse.Message = "Data update successfully.";
                        }
                    }
                    else
                    {
                        var dbcityname = cities.Select(x => x.CityName.ToLower());
                        var notexist = cityNames.Except(dbcityname).ToList();
                        itemSchemeResponse.Status = false;
                        itemSchemeResponse.Message = "City " + string.Join(",", notexist) + " not found";

                    }
                }
                else
                {
                    itemSchemeResponse.Status = false;
                    itemSchemeResponse.Message = "City Name Cannot be blank";
                }
            }
            return itemSchemeResponse;
        }

        //[Route("GetCompanyTopMarginItem")]
        //[HttpGet]
        //public async Task<ItemResponseDc> GetCompanyTopMarginItem(int PeopleId, int warehouseId, int skip, int take, string lang)
        //{
        //    var itemResponseDc = new ItemResponseDc { TotalItem = 0, ItemDataDCs = new List<DataContracts.External.ItemDataDC>() };
        //    using (var context = new AuthContext())
        //    {
        //        List<DataContracts.External.ItemDataDC> ItemDataDCs = new List<DataContracts.External.ItemDataDC>();

        //        if (context.Database.Connection.State != ConnectionState.Open)
        //            context.Database.Connection.Open();


        //        var cmd = context.Database.Connection.CreateCommand();
        //        cmd.CommandText = "[dbo].[GetCompanyTopMarginItem]";
        //        cmd.Parameters.Add(new SqlParameter("@warehouseId", warehouseId));
        //        cmd.Parameters.Add(new SqlParameter("@CustomerId", 1));
        //        cmd.Parameters.Add(new SqlParameter("@Skip", skip));
        //        cmd.Parameters.Add(new SqlParameter("@Take", take));
        //        cmd.CommandType = System.Data.CommandType.StoredProcedure;

        //        // Run the sproc
        //        var reader = cmd.ExecuteReader();
        //        var ItemData = ((IObjectContextAdapter)context)
        //        .ObjectContext
        //        .Translate<DataContracts.External.ItemDataDC>(reader).ToList();
        //        reader.NextResult();
        //        while (reader.Read())
        //        {
        //            itemResponseDc.TotalItem = Convert.ToInt32(reader["itemCount"]);
        //        }
        //        var offerids = ItemData.Where(x => x.OfferId > 0 && (x.ItemAppType == 0 || x.ItemAppType == 1)).Select(x => x.OfferId).Distinct().ToList();
        //        var activeOfferids = offerids != null && offerids.Any() ? context.OfferDb.Where(x => offerids.Contains(x.OfferId) && x.IsActive && !x.IsDeleted && (x.OfferAppType == "Retailer App" || x.OfferAppType == "Both")).Select(x => x.OfferId).ToList() : new List<int>();

        //        foreach (var it in ItemData.Where(x => (x.ItemAppType == 0 || x.ItemAppType == 1)))
        //        {
        //            //Condition for offer end

        //            if (!(it.OfferStartTime <= DateTime.Now && it.OfferEndTime >= DateTime.Now))
        //            {
        //                if (it.OfferCategory == 2)
        //                {
        //                    it.IsOffer = false;
        //                    it.FlashDealSpecialPrice = 0;
        //                    it.OfferCategory = 0;
        //                }
        //                else if (it.OfferCategory == 1)
        //                {
        //                    it.IsOffer = false;
        //                    it.OfferCategory = 0;
        //                }

        //            }
        //            else if ((it.OfferStartTime <= DateTime.Now && it.OfferEndTime >= DateTime.Now) && it.OfferCategory == 2)
        //            {
        //                it.IsOffer = false;
        //                it.FlashDealSpecialPrice = 0;
        //                it.OfferCategory = 0;

        //            }

        //            if (it.OfferCategory == 1)
        //            {
        //                if (activeOfferids.Any() && activeOfferids.Any(x => x == it.OfferId) && it.IsOffer)
        //                    it.IsOffer = true;
        //                else
        //                    it.IsOffer = false;
        //            }


        //            try
        //            {
        //                if (!it.IsOffer)
        //                {
        //                    /// Dream Point Logic && Margin Point
        //                    int? MP, PP;
        //                    double xPoint = xPointValue * 10;
        //                    //Customer (0.2 * 10=1)
        //                    if (it.promoPerItems.Equals(null) && it.promoPerItems == null)
        //                    {
        //                        PP = 0;
        //                    }
        //                    else
        //                    {
        //                        PP = it.promoPerItems;
        //                    }
        //                    if (it.marginPoint.Equals(null) && it.promoPerItems == null)
        //                    {
        //                        MP = 0;
        //                    }
        //                    else
        //                    {
        //                        double WithTaxNetPurchasePrice = Math.Round(it.NetPurchasePrice * (1 + (it.TotalTaxPercentage / 100)), 3);//With tax
        //                        MP = Convert.ToInt32((it.UnitPrice - WithTaxNetPurchasePrice) * xPoint); // (UnitPrice-NPP withtax) * By xpoint 
        //                    }
        //                    if (PP > 0 && MP > 0)
        //                    {
        //                        int? PP_MP = PP + MP;
        //                        it.dreamPoint = PP_MP;
        //                    }
        //                    else if (MP > 0)
        //                    {
        //                        it.dreamPoint = MP;
        //                    }
        //                    else if (PP > 0)
        //                    {
        //                        it.dreamPoint = PP;
        //                    }
        //                    else
        //                    {
        //                        it.dreamPoint = 0;
        //                    }
        //                }
        //                else { it.dreamPoint = 0; }

        //                // Margin % On app site logic ((MRP-UnitPrice)*100)/UnitPrice
        //                if (it.price > it.UnitPrice)
        //                {
        //                    it.marginPoint = ((it.price - it.UnitPrice) * 100) / it.UnitPrice;//MP;  we replce marginpoint value by margin for app here 
        //                }
        //                else
        //                {
        //                    it.marginPoint = 0;
        //                }
        //            }
        //            catch { }

        //            if (it.HindiName != null && !string.IsNullOrEmpty(lang) && lang == "hi")
        //            {
        //                if (it.IsSensitive == true && it.IsSensitiveMRP == true)
        //                {
        //                    it.itemname = it.HindiName + " " + it.price + " MRP " + it.UnitofQuantity + " " + it.UOM;
        //                }
        //                else if (it.IsSensitive == true && it.IsSensitiveMRP == false)
        //                {
        //                    it.itemname = it.HindiName + " " + it.UnitofQuantity + " " + it.UOM; //item display name 
        //                }

        //                else if (it.IsSensitive == false && it.IsSensitiveMRP == false)
        //                {
        //                    it.itemname = it.HindiName; //item display name
        //                }
        //                else if (it.IsSensitive == false && it.IsSensitiveMRP == true)
        //                {
        //                    it.itemname = it.HindiName + " " + it.price + " MRP";//item display name 
        //                }
        //            }

        //            ItemDataDCs.Add(it);
        //        }

        //        itemResponseDc.ItemDataDCs = ItemDataDCs;
        //    }

        //    return itemResponseDc;
        //}

        //[Route("GetABCategoryItem")]
        //[HttpGet]
        //public async Task<ItemResponseDc> GetABCategoryItem(int PeopleId, int warehouseId, int skip, int take, string lang)
        //{
        //    var itemResponseDc = new ItemResponseDc { TotalItem = 0, ItemDataDCs = new List<DataContracts.External.ItemDataDC>() };
        //    using (var context = new AuthContext())
        //    {
        //        List<DataContracts.External.ItemDataDC> ItemDataDCs = new List<DataContracts.External.ItemDataDC>();

        //        if (context.Database.Connection.State != ConnectionState.Open)
        //            context.Database.Connection.Open();
        //        if (take == 4)
        //            take = 100;

        //        var cmd = context.Database.Connection.CreateCommand();
        //        cmd.CommandText = "[dbo].[GetABCategoryItem]";
        //        cmd.Parameters.Add(new SqlParameter("@warehouseId", warehouseId));
        //        cmd.Parameters.Add(new SqlParameter("@Skip", skip));
        //        cmd.Parameters.Add(new SqlParameter("@Take", take));
        //        cmd.CommandType = System.Data.CommandType.StoredProcedure;

        //        // Run the sproc
        //        var reader = cmd.ExecuteReader();
        //        var ItemData = ((IObjectContextAdapter)context)
        //        .ObjectContext
        //        .Translate<DataContracts.External.ItemDataDC>(reader).ToList();
        //        reader.NextResult();
        //        while (reader.Read())
        //        {
        //            itemResponseDc.TotalItem = Convert.ToInt32(reader["itemCount"]);
        //        }
        //        var offerids = ItemData.Where(x => x.OfferId > 0 && (x.ItemAppType == 0 || x.ItemAppType == 1)).Select(x => x.OfferId).Distinct().ToList();
        //        var activeOfferids = offerids != null && offerids.Any() ? context.OfferDb.Where(x => offerids.Contains(x.OfferId) && x.IsActive && !x.IsDeleted && (x.OfferAppType == "Retailer App" || x.OfferAppType == "Both")).Select(x => x.OfferId).ToList() : new List<int>();

        //        foreach (var it in ItemData.Where(x => (x.ItemAppType == 0 || x.ItemAppType == 1)))
        //        {
        //            //Condition for offer end

        //            if (!(it.OfferStartTime <= DateTime.Now && it.OfferEndTime >= DateTime.Now))
        //            {
        //                if (it.OfferCategory == 2)
        //                {
        //                    it.IsOffer = false;
        //                    it.FlashDealSpecialPrice = 0;
        //                    it.OfferCategory = 0;
        //                }
        //                else if (it.OfferCategory == 1)
        //                {
        //                    it.IsOffer = false;
        //                    it.OfferCategory = 0;
        //                }

        //            }
        //            else if ((it.OfferStartTime <= DateTime.Now && it.OfferEndTime >= DateTime.Now) && it.OfferCategory == 2)
        //            {
        //                it.IsOffer = false;
        //                it.FlashDealSpecialPrice = 0;
        //                it.OfferCategory = 0;

        //            }

        //            if (it.OfferCategory == 1)
        //            {
        //                if (activeOfferids.Any() && activeOfferids.Any(x => x == it.OfferId) && it.IsOffer)
        //                    it.IsOffer = true;
        //                else
        //                    it.IsOffer = false;
        //            }


        //            try
        //            {
        //                if (!it.IsOffer)
        //                {
        //                    /// Dream Point Logic && Margin Point
        //                    int? MP, PP;
        //                    double xPoint = xPointValue * 10;
        //                    //Customer (0.2 * 10=1)
        //                    if (it.promoPerItems.Equals(null) && it.promoPerItems == null)
        //                    {
        //                        PP = 0;
        //                    }
        //                    else
        //                    {
        //                        PP = it.promoPerItems;
        //                    }
        //                    if (it.marginPoint.Equals(null) && it.promoPerItems == null)
        //                    {
        //                        MP = 0;
        //                    }
        //                    else
        //                    {
        //                        double WithTaxNetPurchasePrice = Math.Round(it.NetPurchasePrice * (1 + (it.TotalTaxPercentage / 100)), 3);//With tax
        //                        MP = Convert.ToInt32((it.UnitPrice - WithTaxNetPurchasePrice) * xPoint); // (UnitPrice-NPP withtax) * By xpoint 
        //                    }
        //                    if (PP > 0 && MP > 0)
        //                    {
        //                        int? PP_MP = PP + MP;
        //                        it.dreamPoint = PP_MP;
        //                    }
        //                    else if (MP > 0)
        //                    {
        //                        it.dreamPoint = MP;
        //                    }
        //                    else if (PP > 0)
        //                    {
        //                        it.dreamPoint = PP;
        //                    }
        //                    else
        //                    {
        //                        it.dreamPoint = 0;
        //                    }
        //                }
        //                else { it.dreamPoint = 0; }

        //                // Margin % On app site logic ((MRP-UnitPrice)*100)/UnitPrice
        //                if (it.price > it.UnitPrice)
        //                {
        //                    it.marginPoint = ((it.price - it.UnitPrice) * 100) / it.UnitPrice;//MP;  we replce marginpoint value by margin for app here 
        //                }
        //                else
        //                {
        //                    it.marginPoint = 0;
        //                }
        //            }
        //            catch { }

        //            if (it.HindiName != null && !string.IsNullOrEmpty(lang) && lang == "hi")
        //            {
        //                if (it.IsSensitive == true && it.IsSensitiveMRP == true)
        //                {
        //                    it.itemname = it.HindiName + " " + it.price + " MRP " + it.UnitofQuantity + " " + it.UOM;
        //                }
        //                else if (it.IsSensitive == true && it.IsSensitiveMRP == false)
        //                {
        //                    it.itemname = it.HindiName + " " + it.UnitofQuantity + " " + it.UOM; //item display name 
        //                }

        //                else if (it.IsSensitive == false && it.IsSensitiveMRP == false)
        //                {
        //                    it.itemname = it.HindiName; //item display name
        //                }
        //                else if (it.IsSensitive == false && it.IsSensitiveMRP == true)
        //                {
        //                    it.itemname = it.HindiName + " " + it.price + " MRP";//item display name 
        //                }
        //            }

        //            ItemDataDCs.Add(it);
        //        }

        //        itemResponseDc.ItemDataDCs = ItemDataDCs;
        //    }

        //    return itemResponseDc;
        //}

        [Route("GetItemAgainstCity")]
        public List<CityBaseItemDc> GetItemAgainstCity(string keyword, int CityId)
        {
            using (var context = new AuthContext())
            {
                List<SqlParameter> paramList = new List<SqlParameter>();
                paramList.Add(new SqlParameter("@CityId", CityId));
                paramList.Add(new SqlParameter("@Keyword", keyword));
                List<CityBaseItemDc> ItemList = context.Database.SqlQuery<CityBaseItemDc>("exec GetCityBaseItem @CityId, @Keyword", paramList.ToArray()).ToList();
                return ItemList;
            }
        }


        #region Get ItemliveDashboard Report
        [Route("ItemliveDashboard")]
        [HttpPost]
        public async Task<List<ItemliveDashboardDc>> ItemliveDashboard(LiveDashboardPostDc LiveDashboardPosts)
        {
            List<ItemliveDashboardDc> Appdata = new List<ItemliveDashboardDc>();
            using (var context = new AuthContext())
            {
                if (LiveDashboardPosts != null)
                {
                    List<Object> parameters = new List<object>();
                    string sqlquery = "exec GetItemLiveDashboards";
                    parameters.Add(new SqlParameter("@StartDate", LiveDashboardPosts.StartDate));
                    parameters.Add(new SqlParameter("@EndDate", LiveDashboardPosts.EndDate));
                    sqlquery = sqlquery + " @startDate" + ", @endDate";
                    Appdata = await context.Database.SqlQuery<ItemliveDashboardDc>(sqlquery, parameters.ToArray()).ToListAsync();
                }
            }
            return Appdata;
        }
        #endregion
        public bool AddItemInWarehouseMaster(ItemMaster itemobj, AuthContext context, int Userid)
        {
            bool result = false;

            var itemmaster = context.itemMasters.Where(x => x.ItemId == itemobj.ItemId && x.WarehouseId == itemobj.WarehouseId).SingleOrDefault();
            var mrpobj = context.ItemMultiMRPDB.FirstOrDefault(x => x.ItemMultiMRPId == itemobj.ItemMultiMRPId);
            if (itemmaster != null && mrpobj != null)
            {
                var Iscreated = context.itemMasters.Any(x => x.ItemMultiMRPId == itemobj.ItemMultiMRPId && x.SellingSku == itemmaster.SellingSku && x.WarehouseId == itemobj.WarehouseId);
                if (!Iscreated)
                {
                    ItemMaster item = new ItemMaster();

                    var BuyerName = context.Peoples.Where(x => x.PeopleID == itemobj.BuyerId).Select(x => x.DisplayName).FirstOrDefault();

                    Supplier SN = context.Suppliers.FirstOrDefault(x => x.SupplierId == itemobj.SupplierId && x.Deleted == false);
                    DepoMaster depo = context.DepoMasters.FirstOrDefault(x => x.DepoId == itemobj.DepoId && x.Deleted == false);
                    item.SupplierId = SN != null ? SN.SupplierId : 0;
                    item.SupplierName = SN != null ? SN.Name : "";
                    item.SUPPLIERCODES = SN != null ? SN.SUPPLIERCODES : "";
                    item.DepoId = depo != null ? depo.DepoId : 0;
                    item.DepoName = depo != null ? depo.DepoName : null;
                    item.BuyerName = BuyerName;

                    item.GruopID = itemmaster.GruopID;
                    item.TGrpName = itemmaster.TGrpName;

                    item.DistributionPrice = item.DistributionPrice;
                    item.TotalTaxPercentage = itemmaster.TotalTaxPercentage;

                    item.CessGrpID = itemmaster.CessGrpID;
                    item.CessGrpName = itemmaster.CessGrpName;
                    item.TotalCessPercentage = itemmaster.TotalCessPercentage;
                    item.CatLogoUrl = itemmaster.LogoUrl;
                    item.WarehouseId = itemmaster.WarehouseId;
                    item.WarehouseName = itemmaster.WarehouseName;
                    item.BaseCategoryid = itemmaster.BaseCategoryid;
                    item.LogoUrl = itemmaster.LogoUrl;
                    item.UpdatedDate = indianTime;
                    item.CreatedDate = indianTime;
                    item.CategoryName = itemmaster.CategoryName;
                    item.Categoryid = itemmaster.Categoryid;
                    item.SubcategoryName = itemmaster.SubcategoryName;
                    item.SubCategoryId = itemmaster.SubCategoryId;
                    item.SubsubcategoryName = itemmaster.SubsubcategoryName;
                    item.SubsubCategoryid = itemmaster.SubsubCategoryid;
                    item.SubSubCode = itemmaster.SubSubCode;
                    item.itemcode = itemmaster.itemcode;
                    item.marginPoint = itemmaster.marginPoint;
                    item.Number = itemmaster.Number;
                    item.PramotionalDiscount = itemmaster.PramotionalDiscount;
                    item.MinOrderQty = itemmaster.MinOrderQty;
                    item.NetPurchasePrice = itemmaster.NetPurchasePrice;
                    item.GeneralPrice = itemmaster.GeneralPrice;
                    item.promoPerItems = itemmaster.promoPerItems;
                    item.promoPoint = itemmaster.promoPoint;
                    item.PurchaseMinOrderQty = itemmaster.PurchaseMinOrderQty;
                    item.PurchaseSku = itemmaster.PurchaseSku;
                    item.PurchaseUnitName = itemmaster.PurchaseUnitName;
                    item.SellingSku = itemmaster.SellingSku;
                    item.SellingUnitName = itemmaster.SellingUnitName;
                    item.SizePerUnit = itemmaster.SizePerUnit;
                    item.VATTax = itemmaster.VATTax;
                    item.HSNCode = itemmaster.HSNCode;
                    item.HindiName = itemmaster.HindiName;
                    item.CompanyId = itemmaster.CompanyId;
                    item.Reason = itemmaster.Reason;
                    item.DefaultBaseMargin = itemmaster.DefaultBaseMargin;
                    item.Deleted = false;
                    item.active = false;
                    item.itemname = itemmaster.itemname;
                    item.itemBaseName = itemmaster.itemBaseName;
                    item.Cityid = itemmaster.Cityid;
                    item.CityName = itemmaster.CityName;

                    item.UOM = mrpobj.UOM;
                    item.UnitofQuantity = mrpobj.UnitofQuantity;

                    item.PurchasePrice = itemobj.PurchasePrice;
                    item.UnitPrice = itemobj.UnitPrice;

                    item.ItemMultiMRPId = itemobj.ItemMultiMRPId;
                    item.MRP = mrpobj.MRP;
                    item.price = mrpobj.MRP;
                    //item.IsReplaceable = itemobj.IsReplaceable;
                    if (itemmaster.IsSensitive == true && itemmaster.IsSensitiveMRP == true)
                    {
                        item.itemname = itemmaster.itemBaseName + " " + mrpobj.MRP + " MRP " + mrpobj.UnitofQuantity + " " + mrpobj.UOM;
                    }
                    else if (itemmaster.IsSensitive == true && itemmaster.IsSensitiveMRP == false)
                    {
                        item.itemname = item.itemBaseName + " " + mrpobj.UnitofQuantity + " " + mrpobj.UOM; //item display name 
                    }
                    else if (itemmaster.IsSensitive == false && itemmaster.IsSensitiveMRP == false)
                    {
                        item.itemname = item.itemBaseName; //item display name
                    }
                    else if (itemmaster.IsSensitive == false && itemmaster.IsSensitiveMRP == true)
                    {
                        item.itemname = item.itemBaseName + " " + mrpobj.MRP + " MRP";//item display name 
                    }
                    item.SellingUnitName = item.itemname + " " + item.MinOrderQty + "Unit";//item selling unit name
                    item.PurchaseUnitName = item.itemname + " " + item.PurchaseMinOrderQty + "Unit";//

                    item.IsSensitive = itemmaster.IsSensitive;
                    item.IsSensitiveMRP = itemmaster.IsSensitiveMRP;
                    item.ShelfLife = itemmaster.ShelfLife;
                    item.IsReplaceable = itemmaster.IsReplaceable;
                    item.BomId = itemmaster.BomId;
                    item.Type = itemmaster.Type;
                    item.CreatedBy = Userid;
                    item.SellerStorePrice = itemmaster.SellerStorePrice; //added
                    item.IsSellerStoreItem = itemmaster.IsSellerStoreItem;

                    context.itemMasters.Add(item);

                    if (itemmaster.IsSensitive == true && itemmaster.IsSensitiveMRP == true)
                    {
                        #region check current stock 
                        CurrentStock cntstock = new CurrentStock();
                        cntstock = context.DbCurrentStock.Where(x => x.ItemNumber == itemmaster.Number && x.WarehouseId == itemmaster.WarehouseId && x.ItemMultiMRPId == item.ItemMultiMRPId && x.Deleted == false).SingleOrDefault();
                        if (cntstock == null)
                        {
                            CurrentStock newCstk = new CurrentStock();
                            newCstk.CompanyId = item.CompanyId;
                            newCstk.itemBaseName = item.itemBaseName;
                            newCstk.itemname = item.itemname;
                            newCstk.ItemNumber = item.Number;
                            newCstk.WarehouseId = item.WarehouseId;
                            newCstk.WarehouseName = item.WarehouseName;
                            newCstk.CurrentInventory = 0;
                            newCstk.CreationDate = indianTime;
                            newCstk.UpdatedDate = indianTime;
                            newCstk.MRP = item.price;
                            newCstk.UnitofQuantity = item.UnitofQuantity;
                            newCstk.UOM = item.UOM;
                            newCstk.ItemMultiMRPId = item.ItemMultiMRPId;
                            context.DbCurrentStock.Add(newCstk);
                            #endregion
                        }
                    }
                    else
                    {
                        //if (itemobj.IsCommodity)
                        //{
                        CurrentStock cntstock = new CurrentStock();
                        cntstock = context.DbCurrentStock.Where(x => x.ItemNumber == itemmaster.Number && x.WarehouseId == itemmaster.WarehouseId && x.ItemMultiMRPId == item.ItemMultiMRPId && x.Deleted == false).SingleOrDefault();
                        if (cntstock == null)
                        {
                            CurrentStock newCstk = new CurrentStock();
                            newCstk.CompanyId = item.CompanyId;
                            newCstk.itemBaseName = item.itemBaseName;
                            newCstk.itemname = item.itemname;
                            newCstk.ItemNumber = item.Number;
                            newCstk.WarehouseId = item.WarehouseId;
                            newCstk.WarehouseName = item.WarehouseName;
                            newCstk.CurrentInventory = 0;
                            newCstk.CreationDate = indianTime;
                            newCstk.UpdatedDate = indianTime;
                            newCstk.MRP = item.price;
                            newCstk.UnitofQuantity = item.UnitofQuantity;
                            newCstk.UOM = item.UOM;
                            newCstk.ItemMultiMRPId = item.ItemMultiMRPId;
                            context.DbCurrentStock.Add(newCstk);
                        }
                        //}
                    }

                    return context.Commit() > 0;
                }
                else { return Iscreated; }
            }
            return result;
        }

        [Route("GetDealItemWithPaging")]
        [HttpGet]
        [AllowAnonymous]
        public async Task<ResponseDealItem> GetDealItemWithPaging(int warehouseId, string itemName, int skip, int take)
        {
            int Skiplist = (skip - 1) * take;
            ResponseDealItem responseDealItem = new ResponseDealItem();
            List<DealItemDc> DealItemDcs = new List<DealItemDc>();
            using (var context = new AuthContext())
            {
                if (context.Database.Connection.State != ConnectionState.Open)
                    context.Database.Connection.Open();


                var cmd = context.Database.Connection.CreateCommand();
                cmd.CommandText = "[dbo].[GetDealItemWithPaging]";
                cmd.Parameters.Add(new SqlParameter("@warehouseId", warehouseId));
                cmd.Parameters.Add(new SqlParameter("@ItemName", itemName));
                cmd.Parameters.Add(new SqlParameter("@Skip", Skiplist));
                cmd.Parameters.Add(new SqlParameter("@Take", take));
                cmd.CommandType = System.Data.CommandType.StoredProcedure;

                // Run the sproc
                var reader = cmd.ExecuteReader();
                DealItemDcs = ((IObjectContextAdapter)context)
                .ObjectContext
                .Translate<DealItemDc>(reader).ToList();
                reader.NextResult();
                if (reader.Read())
                {
                    responseDealItem.TotalItem = Convert.ToInt32(reader["itemCount"]);
                }
                responseDealItem.DealItemDcs = DealItemDcs;
            }
            return responseDealItem;
        }

        [Route("SaveDealItem")]
        [HttpPost]
        public async Task<ResponseInsertDealItem> SaveDealItem(DealItemDc dealItemDc)
        {
            ResponseInsertDealItem responseInsertDealItem = new ResponseInsertDealItem { msg = "", result = false };
            using (var context = new AuthContext())
            {
                var identity = User.Identity as ClaimsIdentity;
                int userid = 0;


                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                    userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
                var dealItem = await context.DealItems.FirstOrDefaultAsync(x => x.WarehouseId == dealItemDc.WarehouseId && x.ItemMultiMRPId == dealItemDc.ItemMultiMRPId && x.MinOrderQty == dealItemDc.MinOrderQty);
                if (dealItem != null)
                {
                    dealItem.TotalLimit = dealItemDc.TotalLimit;
                    dealItem.OrderLimit = dealItemDc.OrderLimit;
                    dealItem.ModifiedDate = indianTime;
                    dealItem.CustomerLimit = dealItemDc.CustomerLimit;
                    dealItem.DealPrice = dealItemDc.DealPrice;
                    if (!dealItem.IsActive && dealItemDc.IsActive)
                    {
                        dealItem.StartDate = indianTime;
                    }
                    dealItem.IsActive = dealItemDc.IsActive;
                    dealItem.IsDeleted = false;
                    dealItem.ModifiedBy = userid;
                    context.Entry(dealItem).State = EntityState.Modified;
                }
                else
                {
                    dealItem = new DealItem
                    {
                        IsActive = dealItemDc.IsActive,
                        IsDeleted = false,
                        CreatedBy = userid,
                        CreatedDate = indianTime,
                        ItemMultiMRPId = dealItemDc.ItemMultiMRPId,
                        MinOrderQty = dealItemDc.MinOrderQty,
                        OrderLimit = dealItemDc.OrderLimit,
                        TotalConsume = 0,
                        TotalLimit = dealItemDc.TotalLimit,
                        WarehouseId = dealItemDc.WarehouseId,
                        DealPrice = dealItemDc.DealPrice,
                        CustomerLimit = dealItemDc.CustomerLimit,
                        StartDate = indianTime
                    };
                    context.DealItems.Add(dealItem);
                }
                if (context.Commit() > 0)
                {
                    responseInsertDealItem.result = true;
                    responseInsertDealItem.msg = "Deal item save successfully.";
                }
                else
                {
                    responseInsertDealItem.result = false;
                    responseInsertDealItem.msg = "deal item not save please try after some time.";
                }
            }

            return responseInsertDealItem;
        }

        [Route("DealItemStatusChange")]
        [HttpGet]
        public async Task<ResponseInsertDealItem> SaveDealItem(int Id, bool status)
        {
            ResponseInsertDealItem responseInsertDealItem = new ResponseInsertDealItem { msg = "", result = false };
            using (var context = new AuthContext())
            {
                var identity = User.Identity as ClaimsIdentity;
                int userid = 0;


                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                    userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
                var dealItem = await context.DealItems.FirstOrDefaultAsync(x => x.Id == Id);
                if (dealItem != null)
                {

                    dealItem.IsActive = status;
                    if (dealItem.IsActive)
                    {
                        dealItem.StartDate = DateTime.Now;
                    }
                    dealItem.ModifiedDate = indianTime;
                    dealItem.ModifiedBy = userid;
                    context.Entry(dealItem).State = EntityState.Modified;
                    responseInsertDealItem.result = context.Commit() > 0;
                    string strstatu = status ? "Active" : "Inactive";
                    responseInsertDealItem.msg = "Deal item " + strstatu + " successfully.";
                }
                else
                {
                    responseInsertDealItem.result = false;
                    responseInsertDealItem.msg = "deal item not found.";
                }

            }

            return responseInsertDealItem;

        }


        [Route("getItemListCityWise")]
        [HttpGet]
        public async Task<List<CityWiseItemListDC>> GetItemListCityWise(int cityId, string keyword)
        {
            List<CityWiseItemListDC> res = new List<CityWiseItemListDC>();
            using (AuthContext db = new AuthContext())
            {
                if (cityId > 0)
                {
                    var cityIdParam = new SqlParameter("@CityId", cityId);
                    var keywordParam = new SqlParameter("@keyword", keyword);

                    res = db.Database.SqlQuery<CityWiseItemListDC>("GetItemCityWise @CityId,@keyword", cityIdParam, keywordParam).ToList();
                    res.ForEach(x => x.itemNameWithmultiMrpId = x.itemBaseName + " " + "(" + x.ItemMultiMRPId + ")");
                    //foreach (var item in res)
                    //{
                    //    item.itemNameWithmultiMrpId = item.itemBaseName + (item.ItemMultiMRPId);
                    //}
                    return res;
                }
                else
                {
                    return null;
                }

            }
        }

        [Route("getExistItemListCityWise")]
        [HttpGet]
        public async Task<CityWiseItemListDC> GetExistItemListCityWise(int cityId, string keyword)
        {
            CityWiseItemListDC res = new CityWiseItemListDC();
            using (AuthContext db = new AuthContext())
            {
                if (cityId > 0)
                {
                    var cityIdParam = new SqlParameter("@CityId", cityId);
                    var keywordParam = new SqlParameter("@keyword", keyword);

                    res = db.Database.SqlQuery<CityWiseItemListDC>("GetExistItemCityWise @CityId,@keyword", cityIdParam, keywordParam).FirstOrDefault();
                    if (res != null)
                    {
                        res.Msg = "Data Already Exist!";
                        return res;
                    }
                    else
                    {
                        return res;
                    }
                }
                else
                {
                    return null;
                }

            }
        }

        [Route("AddItemCityWise")]
        [HttpPost]
        public bool AddItemCityWise(AddItemSchemeDC addScheme)
        {
            var identity = User.Identity as ClaimsIdentity;
            int userid = 0;

            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
            DateTime currentTime = DateTime.Now;
            using (AuthContext db = new AuthContext())
            {
                if (addScheme.cityId > 0)
                {
                    ItemScheme itemScheme = new ItemScheme
                    {
                        ItemMultiMRPId = addScheme.ItemMultiMRPId,
                        PTR = addScheme.PTR,
                        BaseScheme = addScheme.BaseScheme,
                        Cityid = addScheme.cityId,
                        CreatedBy = userid,
                        CreatedDate = currentTime,
                        IsActive = true,
                        IsDeleted = false
                    };
                    db.ItemSchemes.Add(itemScheme);
                    db.Commit();
                    return true;
                }
                else
                {
                    return false;
                }

            }
        }

        [Route("UpdateItemCityWise")]
        [HttpPost]
        public bool UpdateItemCityWise(AddItemSchemeDC addScheme)
        {
            var identity = User.Identity as ClaimsIdentity;
            int userid = 0;

            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
            DateTime currentTime = DateTime.Now;
            using (AuthContext db = new AuthContext())
            {
                var editItem = db.ItemSchemes.FirstOrDefault(x => x.Id == addScheme.Id && x.ItemMultiMRPId == addScheme.ItemMultiMRPId);
                if (editItem != null)
                {
                    editItem.ModifiedBy = userid;
                    editItem.ModifiedDate = currentTime;
                    editItem.PTR = addScheme.PTR;
                    editItem.BaseScheme = addScheme.BaseScheme;
                    db.Entry(editItem).State = EntityState.Modified;
                    db.Commit();
                    return true;
                }
                else
                {
                    return false;
                }

            }
        }


        [Route("UpdateEAN")]
        [HttpPost]
        public async Task<ResponseMsg> UpdateEAN(UpdateEanDc UpdateEanDc)
        {
            var result = new ResponseMsg();
            result.Status = false;
            if (UpdateEanDc != null && UpdateEanDc.EanNumber != null && UpdateEanDc.userId > 0)
            {
                using (AuthContext db = new AuthContext())
                {
                    var param = new SqlParameter("ItemNumber", UpdateEanDc.ItemNumber);
                    var param1 = new SqlParameter("Barcode", UpdateEanDc.EanNumber);
                    bool IsBarcodeExistsToOther = db.Database.SqlQuery<bool>("exec BatchCode.IsBarcodeExistsToOther @ItemNumber, @Barcode", param, param1).FirstOrDefault();
                    if (IsBarcodeExistsToOther)
                    {
                        result.Message = "EAN can't updated, due to already exists to another item";
                        return result;
                    }
                    if (db.ItemBarcodes.Any(x => x.Barcode == UpdateEanDc.EanNumber && x.IsDeleted == false && x.IsActive))
                    {
                        result.Message = "EAN can't update, due to already exists";
                        return result;
                    }
                    else
                    {
                        ItemBarcode addbarcode = new ItemBarcode();
                        addbarcode.IsActive = true;
                        addbarcode.ItemNumber = UpdateEanDc.ItemNumber;
                        addbarcode.IsDeleted = false;
                        addbarcode.Barcode = UpdateEanDc.EanNumber;
                        addbarcode.CreatedDate = indianTime;
                        addbarcode.CreatedBy = UpdateEanDc.userId;
                        addbarcode.IsParentBarcode = true;
                        db.ItemBarcodes.Add(addbarcode);
                        result.Status = db.Commit() > 0;
                        result.Message = "EAN Updated Successfully";
                    }

                }
            }
            else
            {
                result.Message = "something went wrong";
            }
            return result;
        }

        [Route("UpdateItemBarCode")]
        [HttpPost]
        public string UpdateItemBarCode(UpdateItemBarCodeDc updateItemBarCodeDc)
        {
            var result = "";
            result = "Verified";
            return result;

            //using (AuthContext db = new AuthContext())
            //{
            //    var param = new SqlParameter("ItemNumber", updateItemBarCodeDc.ItemNumber);
            //    var param1 = new SqlParameter("Barcode", updateItemBarCodeDc.EanNumber);
            //    bool IsBarcodeExistsToOther = db.Database.SqlQuery<bool>("exec BatchCode.IsBarcodeExistsToOther @ItemNumber, @Barcode", param, param1).FirstOrDefault();
            //    if (IsBarcodeExistsToOther)
            //    {
            //        result = "Barcode can't updated, due to already exists to another item";
            //    }
            //    else
            //    {

            // }
            //}
        }


        [HttpGet]
        [Route("GetItemBarcodes")]
        public List<ItemBarcodedDc> GetItemBarcodes(string itemNumber)
        {
            using (var context = new AuthContext())
            {
                List<ItemBarcodedDc> barcodeList = null;
                try
                {
                    var identity = User.Identity as ClaimsIdentity;
                    int compid = 1, userid = 0;
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
                    int CompanyId = compid;

                    var query = from b in context.ItemBarcodes
                                where b.ItemNumber == itemNumber
                                    && b.IsActive == true
                                    && b.IsDeleted == false
                                select new ItemBarcodedDc
                                {
                                    Barcode = b.Barcode,
                                    Id = b.Id,
                                    IsActive = b.IsActive,
                                    ItemNumber = b.ItemNumber
                                };
                    barcodeList = query.ToList();
                    return barcodeList;

                }
                catch (Exception ex)
                {
                    return null;
                }
            }
        }

        [HttpPost]
        [Route("AddItemBarcode")]
        public bool AddItemBarcode(ItemBarcodedDc itemBarcoded)
        {
            using (var context = new AuthContext())
            {
                List<ItemBarcodedDc> barcodeList = null;
                try
                {
                    var identity = User.Identity as ClaimsIdentity;
                    int compid = 1, userid = 0;
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
                    int CompanyId = compid;

                    var barcode = context.ItemBarcodes.FirstOrDefault(x => x.Barcode == itemBarcoded.Barcode.Trim() && x.IsDeleted == false && x.IsActive == true);
                    if (barcode != null)
                    {
                        return false;
                    }

                    bool Isparentean = context.ItemBarcodes.Any(x => x.ItemNumber == itemBarcoded.ItemNumber && x.IsActive && x.IsDeleted == false) ? false : true;

                    context.ItemBarcodes.Add(new ItemBarcode
                    {
                        Barcode = itemBarcoded.Barcode.Trim(),
                        CreatedBy = userid,
                        CreatedDate = DateTime.Now,
                        IsActive = true,
                        IsDeleted = false,
                        ItemNumber = itemBarcoded.ItemNumber,
                        IsParentBarcode = Isparentean
                    });
                    context.Commit();
                    return true;
                }
                catch (Exception ex)
                {
                    return false;
                }
            }
        }

        [HttpPost]
        [Route("AddItemBarcodeSarthiAPP")]
        public APIResponse AddItemBarcodeSarthiAPP(ItemBarcodedDc itemBarcoded)
        {
            APIResponse result = new APIResponse { Status = false, Message = "" };
            using (var context = new AuthContext())
            {
                List<ItemBarcodedDc> barcodeList = null;
                try
                {
                    var barcode = context.ItemBarcodes.Where(x => x.Barcode == itemBarcoded.Barcode.Trim() && x.IsDeleted == false && x.IsActive == true).Select(x => x.ItemNumber);
                    if (barcode != null && barcode.Any())
                    {
                        result = new APIResponse { Status = false, Message = "This Barcode already associate with this item number :" + String.Join(",", barcode) };
                        return result;
                    }

                    bool Isparentean = context.ItemBarcodes.Any(x => x.ItemNumber == itemBarcoded.ItemNumber && x.IsActive && x.IsDeleted == false) ? false : true;

                    context.ItemBarcodes.Add(new ItemBarcode
                    {
                        Barcode = itemBarcoded.Barcode.Trim(),
                        CreatedBy = itemBarcoded.userid,
                        CreatedDate = DateTime.Now,
                        IsActive = true,
                        IsDeleted = false,
                        ItemNumber = itemBarcoded.ItemNumber,
                        IsParentBarcode = Isparentean
                    });
                    context.Commit();
                    result = new APIResponse { Status = true, Message = "Barcode added successfully. " };

                }
                catch (Exception ex)
                {
                    result = new APIResponse { Status = false, Message = "Some error occcurred during Barcode adding. " };

                }
            }
            return result;
        }

        [HttpGet]
        [Route("DeleteItemBarcodes")]
        public bool DeleteItemBarcodes(string itemNumber, string Barcode)
        {

            var identity = User.Identity as ClaimsIdentity;
            int userid = 0;
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
            DateTime currentTime = DateTime.Now;
            using (var context = new AuthContext())
            {
                var itembarcode = context.ItemBarcodes.FirstOrDefault(x => x.ItemNumber == itemNumber.Trim() && x.Barcode == Barcode.Trim() && x.IsActive && !x.IsDeleted.Value);

                if (itembarcode != null)
                {
                    itembarcode.IsActive = false;
                    itembarcode.IsDeleted = true;
                    itembarcode.ModifiedBy = userid;
                    itembarcode.ModifiedDate = currentTime;
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
        [Route("GenerateBarcode")]
        public bool GenerateBarcode(string itemNumber)
        {
            var identity = User.Identity as ClaimsIdentity;
            int userid = 0;
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
            string genrateBarcode = "";
            bool status = false;
            using (var db = new AuthContext())
            {
                if (!string.IsNullOrEmpty(itemNumber))
                {
                    genrateBarcode = Ganratebarcode(itemNumber, db);
                    if (genrateBarcode.Length == 13)
                    {
                        ItemBarcode itemBarcode = new ItemBarcode();
                        itemBarcode.ItemNumber = itemNumber;
                        itemBarcode.Barcode = genrateBarcode;
                        itemBarcode.IsActive = true;
                        itemBarcode.IsDeleted = false;
                        itemBarcode.CreatedDate = DateTime.Now;
                        itemBarcode.CreatedBy = userid;
                        itemBarcode.IsParentBarcode = true;
                        db.ItemBarcodes.Add(itemBarcode);
                        db.Commit();
                        status = true;
                        return status;
                    }
                }
            }
            return status;
        }

        public string Ganratebarcode(string itemNumber, AuthContext db)
        {
            string genrateBarcode = "";
            string newbarcode = "";
            if (!string.IsNullOrEmpty(itemNumber))
            {
                bool flag = false;
                while (flag == false)
                {
                    Guid guid = Guid.NewGuid();
                    BigInteger bigInt = new BigInteger(guid.ToByteArray());
                    newbarcode = bigInt + itemNumber.ToUpper();
                    genrateBarcode = newbarcode.Substring(newbarcode.Length - 13, 13);
                    if (!string.IsNullOrEmpty(genrateBarcode))
                    {
                        var check = db.ItemBarcodes.Any(x => x.Barcode == genrateBarcode && x.IsActive && !x.IsDeleted.Value);
                        if (!check)
                        {
                            flag = true;
                            return genrateBarcode;
                        }
                    }
                }
            }
            return genrateBarcode;
        }

        [HttpGet]
        [Route("DailyRateUpdate")]
        [AllowAnonymous]
        public async Task<bool> DailyRateUpdate()
        {
            bool res = false;
            using (var db = new AuthContext())
            {
                var result = await db.Database.ExecuteSqlCommandAsync("EXEC CalculatePurchasePriceLateNight");
                if (result > 0)
                {
                    var itemdata = db.Database.SqlQuery<DailyRateUpdateDc>("EXEC DailyRateUpdate").ToList();
                    if (itemdata != null && itemdata.Count > 0)
                    {
                        var itemIds = itemdata.Select(x => x.ItemId);
                        var itemmasters = db.itemMasters.Where(x => itemIds.Contains(x.ItemId)).ToList();
                        List<DataTable> dt = new List<DataTable>();
                        DataTable ItemRateChange = new DataTable();
                        ItemRateChange.TableName = "DailyItemRateChange";
                        dt.Add(ItemRateChange);

                        ItemRateChange.Columns.Add("ItemId");
                        ItemRateChange.Columns.Add("WarehouseName");
                        ItemRateChange.Columns.Add("ItemName");
                        ItemRateChange.Columns.Add("Mrp");
                        ItemRateChange.Columns.Add("ItemMultiMrpId");
                        ItemRateChange.Columns.Add("OldUnitPrice");
                        ItemRateChange.Columns.Add("OldTradePrice");
                        ItemRateChange.Columns.Add("OldWholeSalePrice");
                        ItemRateChange.Columns.Add("UnitPrice");
                        ItemRateChange.Columns.Add("TradePrice");
                        ItemRateChange.Columns.Add("WholeSalePrice");
                        ItemRateChange.Columns.Add("OldPurchasePrice");
                        ItemRateChange.Columns.Add("PurchasePrice");


                        DataTable ItemRateNotChange = new DataTable();
                        ItemRateNotChange.TableName = "DailyItemRateNotChange";
                        dt.Add(ItemRateNotChange);
                        ItemRateNotChange.Columns.Add("ItemId");
                        ItemRateNotChange.Columns.Add("WarehouseName");
                        ItemRateNotChange.Columns.Add("ItemName");
                        ItemRateNotChange.Columns.Add("Mrp");
                        ItemRateNotChange.Columns.Add("ItemMultiMrpId");
                        ItemRateNotChange.Columns.Add("PurchasePrice");


                        foreach (var item in itemmasters)
                        {
                            //ItemMaster item= objitem;

                            var UpdateItem = itemdata.Where(x => x.ItemId == item.ItemId).FirstOrDefault();
                            if (UpdateItem != null)
                            {
                                var dr = ItemRateChange.NewRow();
                                dr["ItemId"] = item.ItemId;
                                dr["WarehouseName"] = item.WarehouseName;
                                dr["ItemName"] = item.itemname;
                                dr["Mrp"] = item.MRP == 0 ? item.price : item.MRP;
                                dr["ItemMultiMrpId"] = item.ItemMultiMRPId;
                                dr["OldUnitPrice"] = item.UnitPrice;
                                dr["OldTradePrice"] = item.TradePrice;
                                dr["OldWholeSalePrice"] = item.WholeSalePrice;
                                dr["OldPurchasePrice"] = item.PurchasePrice;

                                double oldUnitPrice = item.UnitPrice;
                                double oldTradePrice = item.TradePrice.HasValue && item.TradePrice > 0 ? (double)item.TradePrice : item.UnitPrice;
                                double oldWholeSalePrice = item.WholeSalePrice.HasValue && item.WholeSalePrice > 0 ? (double)item.WholeSalePrice : item.UnitPrice;


                                double withouttaxvalue = (UpdateItem.PurchasePrice / ((100 + item.TotalTaxPercentage) / 100));
                                double withouttax = Math.Round(withouttaxvalue, 3);
                                double netDiscountAmountvalue = (withouttax * (item.Discount / 100));
                                double netDiscountAmount = Math.Round(netDiscountAmountvalue, 3);
                                item.NetPurchasePrice = Math.Round((withouttax - netDiscountAmount), 3);// without tax

                                item.WithTaxNetPurchasePrice = Math.Round(item.NetPurchasePrice * (1 + (item.TotalTaxPercentage / 100)), 3);//With tax                                                             
                                item.PurchasePrice = UpdateItem.PurchasePrice;

                                if (UpdateItem.Margin > 0 && item.Margin < UpdateItem.Margin)
                                {
                                    item.Margin = UpdateItem.Margin;
                                }
                                if (UpdateItem.WholesalerMM > 0 && item.WholesaleMargin < UpdateItem.WholesalerMM)
                                {
                                    item.WholesaleMargin = UpdateItem.WholesalerMM;
                                }
                                if (UpdateItem.TraderMM > 0 && item.TradeMargin < UpdateItem.TraderMM)
                                {
                                    item.TradeMargin = UpdateItem.TraderMM;
                                }
                                item.UnitPrice = Math.Round(UpdateItem.PurchasePrice + (UpdateItem.PurchasePrice * item.Margin / 100), 3);
                                item.TradePrice = Math.Round(UpdateItem.PurchasePrice + (UpdateItem.PurchasePrice * (item.TradeMargin ?? 0) / 100), 3);
                                item.WholeSalePrice = Math.Round(UpdateItem.PurchasePrice + (UpdateItem.PurchasePrice * (item.WholesaleMargin ?? 0) / 100), 3);
                                item.UpdatedDate = DateTime.Now;
                                item.Reason = "System Daily Rate change";
                                dr["UnitPrice"] = item.UnitPrice;
                                dr["TradePrice"] = item.TradePrice;
                                dr["WholeSalePrice"] = item.WholeSalePrice;
                                dr["PurchasePrice"] = item.PurchasePrice;

                                if (item.PurchasePrice < item.price
                                    && (!(item.UnitPrice > item.price || item.TradePrice > item.price || item.WholeSalePrice > item.price))
                                    // && (item.UnitPrice > oldUnitPrice || item.TradePrice > oldTradePrice || item.WholeSalePrice > oldWholeSalePrice)
                                    )
                                {
                                    if (item.UnitPrice < oldUnitPrice)
                                        item.UnitPrice = oldUnitPrice;

                                    if (item.TradePrice < oldTradePrice)
                                        item.TradePrice = oldTradePrice;

                                    if (item.WholeSalePrice < oldWholeSalePrice)
                                        item.WholeSalePrice = oldWholeSalePrice;

                                    ItemRateChange.Rows.Add(dr);
                                    db.Entry(item).State = EntityState.Modified;
                                }
                                else
                                {
                                    var dr1 = ItemRateNotChange.NewRow();
                                    dr1["ItemId"] = item.ItemId;
                                    dr1["WarehouseName"] = item.WarehouseName;
                                    dr1["ItemName"] = item.itemname;
                                    dr1["Mrp"] = item.MRP == 0 ? item.price : item.MRP;
                                    dr1["ItemMultiMrpId"] = item.ItemMultiMRPId;
                                    dr1["PurchasePrice"] = item.PurchasePrice;
                                    ItemRateNotChange.Rows.Add(dr1);
                                }
                            }
                        }
                        if (db.Commit() > 0)
                        {
                            if (ItemRateChange.Rows.Count > 0)
                            {
                                string ExcelSavePath = HttpContext.Current.Server.MapPath("~/ExcelGeneratePath/");
                                if (!Directory.Exists(ExcelSavePath))
                                    Directory.CreateDirectory(ExcelSavePath);
                                string filePath = ExcelSavePath + "ItemDailyRateChange_" + DateTime.Now.ToString("yyyy-dd-M--HH-mm-ss") + ".xlsx";

                                if (ExcelGenerator.DataTable_To_Excel(dt, filePath))
                                {
                                    string connectionString = ConfigurationManager.ConnectionStrings["AuthContext"].ConnectionString;
                                    string To = "", From = "", Bcc = "";
                                    DataTable emaildatatable = new DataTable();
                                    using (var connection = new SqlConnection(connectionString))
                                    {
                                        using (var command = new SqlCommand("Select * from EmailRecipients where EmailType='ItemDailyRateChange'", connection))
                                        {
                                            if (connection.State != ConnectionState.Open)
                                                connection.Open();

                                            SqlDataAdapter da = new SqlDataAdapter(command);
                                            da.Fill(emaildatatable);
                                            da.Dispose();
                                            connection.Close();
                                        }
                                    }
                                    if (emaildatatable.Rows.Count > 0)
                                    {
                                        To = !string.IsNullOrEmpty(emaildatatable.Rows[0]["To"].ToString()) ? emaildatatable.Rows[0]["To"].ToString() : To;
                                        From = !string.IsNullOrEmpty(emaildatatable.Rows[0]["From"].ToString()) ? emaildatatable.Rows[0]["From"].ToString() : From;
                                        Bcc = !string.IsNullOrEmpty(emaildatatable.Rows[0]["Bcc"].ToString()) ? emaildatatable.Rows[0]["Bcc"].ToString() : "";
                                    }
                                    string subject = DateTime.Now.ToString("dd MMM yyyy") + " Daily Item Rate Change Report";
                                    string message = "Please find attach Daily Item Rate Change Report";
                                    if (!string.IsNullOrEmpty(To) && !string.IsNullOrEmpty(From))
                                        EmailHelper.SendMail(From, To, Bcc, subject, message, filePath);
                                    else
                                        logger.Error("Daily Item Rate Change Report To and From empty");

                                }





                            }
                            res = true;
                        }
                    }
                }
                return res;
            }
        }

        [HttpGet]
        [Route("GetAllSeasonConfig")]
        public async Task<List<SeasonalConfigDC>> getAllSeasonConfig()
        {
            using (var context = new AuthContext())
            {
                List<SeasonalConfigDC> result = new List<SeasonalConfigDC>();
                result = context.seasonalConfigs.Where(x => x.IsActive == true && x.IsDeleted == false)
                    .Select(y => new SeasonalConfigDC { SeasonId = y.SeasonId, SeasonName = y.SeasonName }).ToList();
                return result;
            }
        }

        [HttpPost]
        [Route("ItemSchemeSampleFile")]
        public async Task<List<ItemSchemeSampleDC>> ItemSchemeSampleFile(List<int> CityIds)
        {
            List<ItemSchemeSampleDC> ItemSchemeData = new List<ItemSchemeSampleDC>();
            if (CityIds != null && CityIds.Count > 0)
            {
                using (var context = new AuthContext())
                {
                    var cityidDt = new DataTable();
                    cityidDt.Columns.Add("IntValue");
                    foreach (var id in CityIds)
                    {
                        var dr = cityidDt.NewRow();
                        dr["IntValue"] = id;
                        cityidDt.Rows.Add(dr);
                    }
                    var paramid = new SqlParameter("@CityIds", cityidDt);
                    paramid.SqlDbType = SqlDbType.Structured;
                    paramid.TypeName = "dbo.IntValues";
                    ItemSchemeData = context.Database.SqlQuery<ItemSchemeSampleDC>("ItemSchemeSampleFile @CityIds", paramid).ToList();
                }
            }
            return ItemSchemeData;
        }

        [Route("GetWarehouseItemByKeyword")]
        [HttpGet]
        public HttpResponseMessage GetWarehouseItemByKeyword(int WarehouseId, string keyword)
        {
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

                    if (WarehouseId > 0)
                    {
                        var Item = new List<ItemMaster>();
                        if (keyword != null)
                        {
                            Item = db.itemMasters.Where(x => x.WarehouseId == WarehouseId && x.Deleted == false && x.active == true && x.itemname.Contains(keyword)).ToList();
                        }
                        else
                        {
                            Item = db.itemMasters.Where(x => x.WarehouseId == WarehouseId && x.Deleted == false && x.active == true).ToList();
                        }


                        return Request.CreateResponse(HttpStatusCode.OK, Item);
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.OK, "Null");
                    }

                }
                catch (Exception ss)
                {

                    return Request.CreateResponse(HttpStatusCode.OK, "Null");

                }
            }
        }


        [Route("GetMRPMediaList")]
        [HttpGet]
        public HttpResponseMessage GetMRPMediaList(string ItemNumber, int WarehouseId)
        {
            using (var db = new AuthContext())
            {
                try
                {
                    if (!string.IsNullOrEmpty(ItemNumber))
                    {
                        var warehouseId = new SqlParameter("@WarehouseId", WarehouseId);
                        var itemNumber = new SqlParameter("@itemNumber", ItemNumber);
                        List<GetConsumerItemMRP> MRPMediaDetails = db.Database.SqlQuery<GetConsumerItemMRP>("EXEC GetMRPMediaDetails @itemNumber,@WarehouseId", itemNumber, warehouseId).ToList();

                        if (MRPMediaDetails != null && MRPMediaDetails.Any())
                        {
                            return Request.CreateResponse(HttpStatusCode.OK, MRPMediaDetails);
                        }
                        else
                        {
                            return Request.CreateResponse(HttpStatusCode.OK, "Data Not found!");
                        }
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.OK, "Null");
                    }

                }
                catch (Exception ss)
                {

                    return Request.CreateResponse(HttpStatusCode.OK, "Null");

                }
            }
        }

        [Route("AddUpdateConsumerItem")]
        [HttpPost]
        public HttpResponseMessage AddUpdateConsumerItem(List<AddUpdateConsumerItemDc> AddUpdateConsumerItems)
        {
            var identity = User.Identity as ClaimsIdentity;
            int compid = 0, userid = 0;
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))
                compid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);

            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
            var wid = AddUpdateConsumerItems.FirstOrDefault(x => x.WarehouseId > 0).WarehouseId;

            if (userid > 0 && AddUpdateConsumerItems != null && AddUpdateConsumerItems.Any(x => x.ItemMultiMrpId > 0))
            {
                using (var context = new AuthContext())
                {


                    var wh = context.Warehouses.FirstOrDefault(x => x.WarehouseId == wid);
                    if (wh != null && wh.StoreType == 0)
                    {
                        return Request.CreateResponse(HttpStatusCode.OK, "Warehouse doesn't have access");
                    }

                    var user = context.Peoples.FirstOrDefault(x => x.PeopleID == userid);

                    List<ItemMaster> NewAdditems = new List<ItemMaster>();
                    List<ItemLimitMaster> NewItemLimits = new List<ItemLimitMaster>();
                    List<CurrentStock> AddCurrentStocks = new List<CurrentStock>();

                    int buyerid = AddUpdateConsumerItems.FirstOrDefault().BuyerId;
                    int supplierId = AddUpdateConsumerItems.FirstOrDefault().SupplierId;
                    int depoId = AddUpdateConsumerItems.FirstOrDefault().DepoId;


                    var BuyerName = context.Peoples.FirstOrDefault(x => x.PeopleID == buyerid);
                    Supplier SN = context.Suppliers.FirstOrDefault(x => x.SupplierId == supplierId);
                    DepoMaster depo = context.DepoMasters.FirstOrDefault(x => x.DepoId == depoId);


                    var itemMultiMrpIds = AddUpdateConsumerItems.Select(x => x.ItemMultiMrpId).ToList();
                    var mrplist = context.ItemMultiMRPDB.Where(x => itemMultiMrpIds.Contains(x.ItemMultiMRPId)).ToList();


                    List<int> ItemIds = AddUpdateConsumerItems.Where(x => x.ItemId > 0).Select(x => x.ItemId).ToList();
                    var Defaultitemmaster = new ItemMaster();

                    var itemmasterlist = context.itemMasters.Where(x => ItemIds.Contains(x.ItemId) && x.WarehouseId == wid).ToList();

                    if (AddUpdateConsumerItems.Any(x => x.ItemId == 0))
                    {
                        var number = mrplist.FirstOrDefault().ItemNumber;
                        Defaultitemmaster = context.itemMasters.FirstOrDefault(x => x.Number == number && x.WarehouseId == wid && !x.IsDisContinued && !x.Deleted);
                        foreach (var itemobj in AddUpdateConsumerItems.Where(x => x.ItemId == 0).ToList())
                        {
                            var mrpobj = mrplist.FirstOrDefault(x => x.ItemMultiMRPId == itemobj.ItemMultiMrpId);
                            if (Defaultitemmaster != null && mrpobj != null && !context.itemMasters.Any(x => x.WarehouseId == wid && x.ItemMultiMRPId == itemobj.ItemMultiMrpId && !x.Deleted && !x.IsDisContinued))
                            {
                                ItemMaster item = new ItemMaster();
                                item.SupplierId = SN.SupplierId;
                                item.SupplierName = SN.Name;
                                item.SUPPLIERCODES = SN.SUPPLIERCODES;
                                item.DepoId = depo.DepoId;
                                item.DepoName = depo.DepoName;
                                item.BuyerName = BuyerName.DisplayName;
                                item.GruopID = Defaultitemmaster.GruopID;
                                item.TGrpName = Defaultitemmaster.TGrpName;
                                item.DistributionPrice = mrpobj.MRP;
                                item.TotalTaxPercentage = Defaultitemmaster.TotalTaxPercentage;
                                item.CessGrpID = Defaultitemmaster.CessGrpID;
                                item.CessGrpName = Defaultitemmaster.CessGrpName;
                                item.TotalCessPercentage = Defaultitemmaster.TotalCessPercentage;
                                item.CatLogoUrl = Defaultitemmaster.LogoUrl;
                                item.WarehouseId = wid;
                                item.WarehouseName = wh.WarehouseName;
                                item.BaseCategoryid = Defaultitemmaster.BaseCategoryid;
                                item.LogoUrl = Defaultitemmaster.LogoUrl;
                                item.UpdatedDate = indianTime;
                                item.CreatedDate = indianTime;
                                item.CategoryName = Defaultitemmaster.CategoryName;
                                item.Categoryid = Defaultitemmaster.Categoryid;
                                item.SubcategoryName = Defaultitemmaster.SubcategoryName;
                                item.SubCategoryId = Defaultitemmaster.SubCategoryId;
                                item.SubsubcategoryName = Defaultitemmaster.SubsubcategoryName;
                                item.SubsubCategoryid = Defaultitemmaster.SubsubCategoryid;
                                item.SubSubCode = Defaultitemmaster.SubSubCode;
                                item.itemcode = Defaultitemmaster.itemcode;
                                item.marginPoint = Defaultitemmaster.marginPoint;
                                item.Number = Defaultitemmaster.Number;
                                item.PramotionalDiscount = Defaultitemmaster.PramotionalDiscount;
                                item.MinOrderQty = 1;
                                item.NetPurchasePrice = mrpobj.MRP;
                                item.GeneralPrice = mrpobj.MRP;
                                item.promoPerItems = Defaultitemmaster.promoPerItems;
                                item.promoPoint = Defaultitemmaster.promoPoint;
                                item.PurchaseMinOrderQty = Defaultitemmaster.PurchaseMinOrderQty;
                                item.PurchaseSku = Defaultitemmaster.PurchaseSku;
                                item.PurchaseUnitName = Defaultitemmaster.PurchaseUnitName;
                                item.SellingSku = Defaultitemmaster.SellingSku;
                                item.SellingUnitName = Defaultitemmaster.SellingUnitName;
                                item.SizePerUnit = Defaultitemmaster.SizePerUnit;
                                item.VATTax = Defaultitemmaster.VATTax;
                                item.HSNCode = Defaultitemmaster.HSNCode;
                                item.CompanyId = Defaultitemmaster.CompanyId;
                                item.Reason = Defaultitemmaster.Reason;
                                item.DefaultBaseMargin = Defaultitemmaster.DefaultBaseMargin;
                                item.Deleted = false;
                                item.active = itemobj.IsActive;
                                item.itemBaseName = Defaultitemmaster.itemBaseName;
                                item.Cityid = Defaultitemmaster.Cityid;
                                item.CityName = Defaultitemmaster.CityName;
                                item.UOM = mrpobj.UOM;
                                item.UnitofQuantity = mrpobj.UnitofQuantity;
                                item.PurchasePrice = mrpobj.MRP;
                                item.UnitPrice = mrpobj.MRP;
                                item.ItemMultiMRPId = itemobj.ItemMultiMrpId;
                                item.MRP = mrpobj.MRP;
                                item.price = mrpobj.MRP;

                                if (Defaultitemmaster.IsSensitive == true && Defaultitemmaster.IsSensitiveMRP == true)
                                {
                                    item.itemname = Defaultitemmaster.itemBaseName + " " + mrpobj.MRP + " MRP " + mrpobj.UnitofQuantity + " " + mrpobj.UOM;
                                }
                                else if (Defaultitemmaster.IsSensitive == true && Defaultitemmaster.IsSensitiveMRP == false)
                                {
                                    item.itemname = item.itemBaseName + " " + mrpobj.UnitofQuantity + " " + mrpobj.UOM; //item display name 
                                }
                                else if (Defaultitemmaster.IsSensitive == false && Defaultitemmaster.IsSensitiveMRP == false)
                                {
                                    item.itemname = item.itemBaseName; //item display name
                                }
                                else if (Defaultitemmaster.IsSensitive == false && Defaultitemmaster.IsSensitiveMRP == true)
                                {
                                    item.itemname = item.itemBaseName + " " + mrpobj.MRP + " MRP";//item display name 
                                }
                                item.SellingUnitName = item.itemname + " " + item.MinOrderQty + "Unit";//item selling unit name
                                item.PurchaseUnitName = item.itemname + " " + item.PurchaseMinOrderQty + "Unit";//
                                item.IsSensitive = Defaultitemmaster.IsSensitive;
                                item.IsSensitiveMRP = Defaultitemmaster.IsSensitiveMRP;
                                item.ShelfLife = Defaultitemmaster.ShelfLife;
                                item.IsReplaceable = Defaultitemmaster.IsReplaceable;
                                item.BomId = Defaultitemmaster.BomId;
                                item.Type = Defaultitemmaster.Type;
                                item.CreatedBy = userid;
                                item.SellerStorePrice = mrpobj.MRP;
                                item.IsSellerStoreItem = Defaultitemmaster.IsSellerStoreItem;
                                NewAdditems.Add(item);

                                #region check current stock 
                                CurrentStock cntstock = new CurrentStock();
                                cntstock = context.DbCurrentStock.FirstOrDefault(x => x.WarehouseId == Defaultitemmaster.WarehouseId && x.ItemMultiMRPId == item.ItemMultiMRPId && x.Deleted == false);
                                if (cntstock == null && !AddCurrentStocks.Any(x => x.WarehouseId == Defaultitemmaster.WarehouseId && x.ItemMultiMRPId == item.ItemMultiMRPId))
                                {
                                    CurrentStock newCstk = new CurrentStock();
                                    newCstk.CompanyId = item.CompanyId;
                                    newCstk.itemBaseName = item.itemBaseName;
                                    newCstk.itemname = item.itemname;
                                    newCstk.ItemNumber = item.Number;
                                    newCstk.WarehouseId = item.WarehouseId;
                                    newCstk.WarehouseName = item.WarehouseName;
                                    newCstk.CurrentInventory = 0;
                                    newCstk.CreationDate = indianTime;
                                    newCstk.UpdatedDate = indianTime;
                                    newCstk.MRP = item.price;
                                    newCstk.UnitofQuantity = item.UnitofQuantity;
                                    newCstk.UOM = item.UOM;
                                    newCstk.ItemMultiMRPId = item.ItemMultiMRPId;
                                    AddCurrentStocks.Add(newCstk);
                                    #endregion
                                }
                            }
                        }
                    }
                    if (ItemIds != null && ItemIds.Any())
                    {
                        Defaultitemmaster = itemmasterlist.OrderByDescending(x => x.MinOrderQty).FirstOrDefault();
                        foreach (var itemMaster in itemmasterlist)
                        {
                            var item = AddUpdateConsumerItems.FirstOrDefault(x => x.ItemId == itemMaster.ItemId);
                            if (item.ItemMultiMrpId != itemMaster.ItemMultiMRPId)
                            {
                                var mrpobj = mrplist.FirstOrDefault(x => x.ItemMultiMRPId == item.ItemMultiMrpId);
                                itemMaster.MRP = mrpobj.MRP;
                                itemMaster.price = mrpobj.MRP;
                                itemMaster.UnitPrice = mrpobj.MRP;

                                if (Defaultitemmaster.IsSensitive == true && Defaultitemmaster.IsSensitiveMRP == true)
                                {
                                    itemMaster.itemname = Defaultitemmaster.itemBaseName + " " + mrpobj.MRP + " MRP " + mrpobj.UnitofQuantity + " " + mrpobj.UOM;
                                }
                                else if (Defaultitemmaster.IsSensitive == true && Defaultitemmaster.IsSensitiveMRP == false)
                                {
                                    itemMaster.itemname = itemMaster.itemBaseName + " " + mrpobj.UnitofQuantity + " " + mrpobj.UOM; //item display name 
                                }
                                else if (Defaultitemmaster.IsSensitive == false && Defaultitemmaster.IsSensitiveMRP == false)
                                {
                                    itemMaster.itemname = itemMaster.itemBaseName; //item display name
                                }
                                else if (Defaultitemmaster.IsSensitive == false && Defaultitemmaster.IsSensitiveMRP == true)
                                {
                                    itemMaster.itemname = itemMaster.itemBaseName + " " + mrpobj.MRP + " MRP";//item display name 
                                }
                                itemMaster.SellingUnitName = itemMaster.itemname + " " + itemMaster.MinOrderQty + "Unit";//item selling unit name
                                itemMaster.PurchaseUnitName = itemMaster.itemname + " " + itemMaster.PurchaseMinOrderQty + "Unit";//
                            }
                            itemMaster.active = item.IsActive;
                            itemMaster.DepoId = item.DepoId;
                            itemMaster.SupplierId = item.SupplierId;
                            itemMaster.BuyerId = item.BuyerId;
                            itemMaster.SupplierId = SN.SupplierId;
                            itemMaster.SupplierName = SN.Name;
                            itemMaster.SUPPLIERCODES = SN.SUPPLIERCODES;
                            itemMaster.DepoId = depo.DepoId;
                            itemMaster.DepoName = depo.DepoName;
                            itemMaster.BuyerName = BuyerName.DisplayName;
                            itemMaster.ModifiedBy = userid;
                            itemMaster.UpdatedDate = indianTime;
                            itemMaster.Description = "During AddUpdateConsumerItem by : " + user.DisplayName;
                            context.Entry(itemMaster).State = EntityState.Modified;
                        }
                    }

                    #region save itemLimit
                    List<int> ItemMultiMrpIdsList = AddUpdateConsumerItems.Select(x => x.ItemMultiMrpId).ToList();


                    var itemslimitsList = context.ItemLimitMasterDB.Where(x => x.WarehouseId == wid && ItemMultiMrpIdsList.Contains(x.ItemMultiMRPId)).ToList();
                    foreach (var item in AddUpdateConsumerItems)
                    {
                        var itemlimit = itemslimitsList.FirstOrDefault(x => x.ItemMultiMRPId == item.ItemMultiMrpId && x.WarehouseId == item.WarehouseId);
                        if (itemlimit != null)
                        {
                            itemlimit.ItemlimitQty = item.LimitQty;
                            itemlimit.IsItemLimit = item.LimitIsActive;
                            context.Entry(itemlimit).State = EntityState.Modified;
                        }
                        else
                        {

                            ItemLimitMaster newitemlimitmstr = new ItemLimitMaster();
                            newitemlimitmstr.WarehouseId = wid;
                            newitemlimitmstr.ItemNumber = mrplist.FirstOrDefault(x => x.ItemMultiMRPId == item.ItemMultiMrpId).ItemNumber;
                            newitemlimitmstr.ItemlimitQty = item.LimitQty;
                            newitemlimitmstr.ItemLimitSaleQty = null;
                            newitemlimitmstr.ItemMultiMRPId = item.ItemMultiMrpId;
                            newitemlimitmstr.IsItemLimit = item.LimitIsActive;
                            newitemlimitmstr.CreatedDate = indianTime;
                            newitemlimitmstr.UpdateDate = indianTime;
                            newitemlimitmstr.CreatedBy = user.DisplayName;
                            NewItemLimits.Add(newitemlimitmstr);

                        }
                    }
                    #endregion

                    if (NewAdditems != null && NewAdditems.Any())
                    {
                        context.itemMasters.AddRange(NewAdditems);
                    }
                    if (NewItemLimits != null && NewItemLimits.Any())
                    {
                        context.ItemLimitMasterDB.AddRange(NewItemLimits);
                    }
                    if (AddCurrentStocks != null && AddCurrentStocks.Any())
                    {
                        context.DbCurrentStock.AddRange(AddCurrentStocks);
                    }
                    if (context.Commit() > 0)
                    {
                        return Request.CreateResponse(HttpStatusCode.OK, "Success");
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.OK, "Failed");
                    }
                }
            }
            else
            {
                return Request.CreateResponse(HttpStatusCode.OK, "Failed");
            }

        }

        [Route("UpdateMRPMedia")]
        [HttpPost]
        public async Task<ResponseMsg> UpdateMRPMedia(UpdateMRPMediaDC req)
        {
            var result = new ResponseMsg() { Status = false, Message = "Something went wrong" };
            var identity = User.Identity as ClaimsIdentity;
            int userid = 0;
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
            if (req != null && userid > 0 && req.ItemMultiMRPId > 0)
            {
                using (var context = new AuthContext())
                {
                    var UserName = await context.Peoples.FirstOrDefaultAsync(x => x.PeopleID == userid && x.Active);
                    if (UserName != null)
                    {
                        var itemMultiMRPdetail = await context.ItemMultiMRPDB.Where(x => x.ItemMultiMRPId == req.ItemMultiMRPId && x.Deleted == false).Include(x => x.MRPMedias).FirstOrDefaultAsync();
                        if (itemMultiMRPdetail != null)
                        {
                            itemMultiMRPdetail.ColourImage = req.ColourImage;
                            itemMultiMRPdetail.ColourName = req.ColourName;
                            itemMultiMRPdetail.UpdatedDate = indianTime;
                            itemMultiMRPdetail.UpdateBy = UserName.DisplayName;
                            foreach (var mediaData in req.mrpMediaDC.Where(x => x.id > 0))
                            {
                                var existMediaData = itemMultiMRPdetail.MRPMedias.FirstOrDefault(x => x.Id == mediaData.id);
                                if (existMediaData != null)
                                {
                                    existMediaData.IsActive = mediaData.isActive;
                                    existMediaData.IsDeleted = mediaData.isDeleted;
                                    existMediaData.SequenceNo = mediaData.SequenceNo;
                                    existMediaData.ModifiedDate = indianTime;
                                    existMediaData.ModifiedBy = userid;

                                }
                            }
                            context.Entry(itemMultiMRPdetail).State = EntityState.Modified;
                            var newAdd = req.mrpMediaDC.Where(x => x.id == 0).ToList();
                            if (newAdd != null && newAdd.Any())
                            {
                                var MRPMedias = new List<MRPMedia>();
                                foreach (var mediaData in newAdd)
                                {
                                    MRPMedias.Add(new MRPMedia()
                                    {
                                        ItemMultiMRPId = itemMultiMRPdetail.ItemMultiMRPId,
                                        IsActive = true,
                                        IsDeleted = false,
                                        Type = mediaData.Type,
                                        Url = mediaData.Url,
                                        SequenceNo = mediaData.SequenceNo,
                                        CreatedDate = indianTime,
                                        CreatedBy = userid
                                    });
                                }
                                if (MRPMedias.Any())
                                {
                                    context.MRPMedias.AddRange(MRPMedias);
                                }
                            }
                            result.Status = context.Commit() > 0;
                            result.Message = result.Status ? "Updated Successfully" : "Failed";
                        }
                    }
                    else
                    {
                        result.Status = false;
                        result.Message = "UserName is Inactive";
                    }
                }
            }
            return result;
        }



        [Route("ConsumerCurrentNetStockAutoLive")]
        [HttpGet]
        [AllowAnonymous]
        public async Task<bool> ConsumerCurrentNetStockAutoLive()
        {
            using (AuthContext context = new AuthContext())
            {
                context.Database.CommandTimeout = 300;
                var consumerCurrentNetStockAutoLive = context.Database.SqlQuery<ActivateConsumerStockDc>("exec [ConsumerCurrentNetStockAutoLive]").ToList();
                if (consumerCurrentNetStockAutoLive != null && consumerCurrentNetStockAutoLive.Any())
                {
                    var activateItemForConsumerStores = new List<ActivateItemForConsumerStoreDc>();
                    foreach (var x in consumerCurrentNetStockAutoLive)
                    {
                        activateItemForConsumerStores.Add(new ActivateItemForConsumerStoreDc
                        {
                            ItemMultiMrpId = x.ItemMultiMrpId,
                            Qty = x.CurrentNetInventory,
                            WarehouseId = x.WarehouseId
                        });
                    }
                    ActivateItemForConsumerStore(activateItemForConsumerStores, context);

                }
            }
            return true;
        }
        private bool ActivateItemForConsumerStore(List<ActivateItemForConsumerStoreDc> ActivateItemForConsumerStore, AuthContext context)
        {
            List<ItemMaster> NewAdditems = new List<ItemMaster>();

            //var loopResult = Parallel.ForEach(ActivateItemForConsumerStore.Select(x => x.WarehouseId).Distinct(), (WarehouseId) =>
            //{

            foreach (var WarehouseId in ActivateItemForConsumerStore.Select(x => x.WarehouseId).Distinct().ToList())
            {

                List<int> ItemMultiMrpIds = ActivateItemForConsumerStore.Where(x => x.WarehouseId == WarehouseId).Select(x => x.ItemMultiMrpId).ToList();

                var ItemMultiMRPlist = context.ItemMultiMRPDB.Where(x => ItemMultiMrpIds.Contains(x.ItemMultiMRPId)).ToList();

                List<string> numbers = ItemMultiMRPlist.Select(x => x.ItemNumber).Distinct().ToList();

                var itemmasterlist = context.itemMasters.Where(x => numbers.Contains(x.Number) && x.WarehouseId == WarehouseId).ToList();

                if (itemmasterlist != null && itemmasterlist.Any(x => ItemMultiMrpIds.Contains(x.ItemMultiMRPId)))
                {

                    foreach (var itemmaster in itemmasterlist.Where(x => ItemMultiMrpIds.Contains(x.ItemMultiMRPId) && !x.active).ToList())
                    {
                        itemmaster.UnitPrice = ItemMultiMRPlist.FirstOrDefault(x => x.ItemMultiMRPId == itemmaster.ItemMultiMRPId).MRP;
                        itemmaster.active = true;
                        itemmaster.ModifiedBy = 1;
                        itemmaster.UpdatedDate = indianTime;
                        itemmaster.Description = "ActivateItemForConsumerStore";
                        context.Entry(itemmaster).State = EntityState.Modified;
                    }
                }

                var ExistItemMultiMrpId = itemmasterlist.Select(x => x.ItemMultiMRPId).ToList();
                var NotExistItemMultiMrpId = ItemMultiMrpIds.Where(x => !ExistItemMultiMrpId.Contains(x)).Select(x => x).ToList();

                foreach (var ItemMultiMrpId in NotExistItemMultiMrpId.Distinct())
                {
                    var mrpobj = ItemMultiMRPlist.FirstOrDefault(x => x.ItemMultiMRPId == ItemMultiMrpId);
                    var itemmaster = itemmasterlist.Where(x => x.WarehouseId == WarehouseId && x.Number == mrpobj.ItemNumber).FirstOrDefault();
                    if (itemmaster != null && mrpobj != null)
                    {
                        ItemMaster item = new ItemMaster();
                        item.SupplierId = itemmaster.SupplierId;
                        item.SupplierName = itemmaster.SupplierName;
                        item.SUPPLIERCODES = itemmaster.SUPPLIERCODES;
                        item.DepoId = itemmaster.DepoId;
                        item.DepoName = itemmaster.DepoName;
                        item.BuyerName = itemmaster.BuyerName;
                        item.BuyerId = itemmaster.BuyerId;
                        item.GruopID = itemmaster.GruopID;
                        item.TGrpName = itemmaster.TGrpName;
                        item.DistributionPrice = mrpobj.MRP;
                        item.TotalTaxPercentage = itemmaster.TotalTaxPercentage;
                        item.CessGrpID = itemmaster.CessGrpID;
                        item.CessGrpName = itemmaster.CessGrpName;
                        item.TotalCessPercentage = itemmaster.TotalCessPercentage;
                        item.CatLogoUrl = itemmaster.LogoUrl;
                        item.WarehouseId = itemmaster.WarehouseId;
                        item.WarehouseName = itemmaster.WarehouseName;
                        item.BaseCategoryid = itemmaster.BaseCategoryid;
                        item.LogoUrl = itemmaster.LogoUrl;
                        item.UpdatedDate = indianTime;
                        item.CreatedDate = indianTime;
                        item.CategoryName = itemmaster.CategoryName;
                        item.Categoryid = itemmaster.Categoryid;
                        item.SubcategoryName = itemmaster.SubcategoryName;
                        item.SubCategoryId = itemmaster.SubCategoryId;
                        item.SubsubcategoryName = itemmaster.SubsubcategoryName;
                        item.SubsubCategoryid = itemmaster.SubsubCategoryid;
                        item.SubSubCode = itemmaster.SubSubCode;
                        item.itemcode = itemmaster.itemcode;
                        item.marginPoint = itemmaster.marginPoint;
                        item.Number = itemmaster.Number;
                        item.PramotionalDiscount = itemmaster.PramotionalDiscount;
                        item.MinOrderQty = 1;
                        item.NetPurchasePrice = mrpobj.MRP;
                        item.GeneralPrice = mrpobj.MRP;
                        item.promoPerItems = itemmaster.promoPerItems;
                        item.promoPoint = itemmaster.promoPoint;
                        item.PurchaseMinOrderQty = itemmaster.PurchaseMinOrderQty;
                        item.PurchaseSku = itemmaster.PurchaseSku;
                        item.PurchaseUnitName = itemmaster.PurchaseUnitName;
                        item.SellingSku = itemmaster.SellingSku;
                        item.SellingUnitName = itemmaster.SellingUnitName;
                        item.SizePerUnit = itemmaster.SizePerUnit;
                        item.VATTax = itemmaster.VATTax;
                        item.HSNCode = itemmaster.HSNCode;
                        item.HindiName = itemmaster.HindiName;
                        item.CompanyId = itemmaster.CompanyId;
                        item.Reason = itemmaster.Reason;
                        item.DefaultBaseMargin = itemmaster.DefaultBaseMargin;
                        item.Deleted = false;
                        item.active = true;
                        item.itemBaseName = itemmaster.itemBaseName;
                        item.Cityid = itemmaster.Cityid;
                        item.CityName = itemmaster.CityName;
                        item.UOM = mrpobj.UOM;
                        item.UnitofQuantity = mrpobj.UnitofQuantity;
                        item.PurchasePrice = mrpobj.MRP;
                        item.UnitPrice = mrpobj.MRP;
                        item.ItemMultiMRPId = mrpobj.ItemMultiMRPId;
                        item.MRP = mrpobj.MRP;
                        item.price = mrpobj.MRP;
                        if (itemmaster.IsSensitive == true && itemmaster.IsSensitiveMRP == true)
                        {
                            item.itemname = itemmaster.itemBaseName + " " + mrpobj.MRP + " MRP " + mrpobj.UnitofQuantity + " " + mrpobj.UOM;
                        }
                        else if (itemmaster.IsSensitive == true && itemmaster.IsSensitiveMRP == false)
                        {
                            item.itemname = item.itemBaseName + " " + mrpobj.UnitofQuantity + " " + mrpobj.UOM; //item display name 
                        }
                        else if (itemmaster.IsSensitive == false && itemmaster.IsSensitiveMRP == false)
                        {
                            item.itemname = item.itemBaseName; //item display name
                        }
                        else if (itemmaster.IsSensitive == false && itemmaster.IsSensitiveMRP == true)
                        {
                            item.itemname = item.itemBaseName + " " + mrpobj.MRP + " MRP";//item display name 
                        }
                        item.SellingUnitName = item.itemname + " " + item.MinOrderQty + "Unit";//item selling unit name
                        item.PurchaseUnitName = item.itemname + " " + item.PurchaseMinOrderQty + "Unit";//
                        item.IsSensitive = itemmaster.IsSensitive;
                        item.IsSensitiveMRP = itemmaster.IsSensitiveMRP;
                        item.ShelfLife = itemmaster.ShelfLife;
                        item.IsReplaceable = itemmaster.IsReplaceable;
                        item.BomId = itemmaster.BomId;
                        item.Type = itemmaster.Type;
                        item.CreatedBy = 1;
                        itemmaster.Description = "ActivateItemForConsumerStore";
                        item.SellerStorePrice = item.MRP;
                        item.IsSellerStoreItem = itemmaster.IsSellerStoreItem;
                        NewAdditems.Add(item);
                    }
                }
                //});
                //if (loopResult.IsCompleted)
                //{
                if (NewAdditems != null && NewAdditems.Any())
                {
                    context.itemMasters.AddRange(NewAdditems);
                }
                context.Commit();
                //}

            }
            return true;
        }

        [Route("AutoBulkItemUpload")]
        [HttpPost]
        [AllowAnonymous]
        public BulkItemResponse AutoBulkItemUpload(int warehouseid)
        {
            string res = "";
            BulkItemResponse result = new BulkItemResponse();
            if (!(warehouseid > 0))
            {
                result.Message = "Please Enter WarehouseId";
                return result;
            }
            DataTable dt = null;
            if (HttpContext.Current.Request.Files.AllKeys.Any())
            {
                var httppostedfile = HttpContext.Current.Request.Files["file"];
                if (httppostedfile != null)
                {
                    string ext = Path.GetExtension(httppostedfile.FileName);
                    if (ext == ".xlsx" || ext == ".xls")
                    {
                        string path = HttpContext.Current.Server.MapPath("~/UploadedFiles/ConsumerItemUploaded");
                        string a1, b;
                        if (!Directory.Exists(path))
                        {
                            Directory.CreateDirectory(path);
                        }
                        a1 = DateTime.Now.ToString("ddMMyyyyHHmmss") + "_" + httppostedfile.FileName;
                        b = Path.Combine(HttpContext.Current.Server.MapPath("~/UploadedFiles/ConsumerItemUploaded"), a1);
                        httppostedfile.SaveAs(b);

                        dt = Helpers.ExcelFileHelper.GetRequestsDataFromExcel(b);
                    }
                }
            }
            List<string> headerlist = new List<string>();
            List<string> categoryname = new List<string>();
            List<string> subcategoryname = new List<string>();
            List<string> subsubcategoryname = new List<string>();
            List<string> basenames = new List<string>();
            string Item_Base_Name = "Item Base Name";
            string Unit_Of_Quantity = "Unit Of Quantity";
            string MRP_Price = "MRP Price";
            string Unit_Of_Measurement = "Unit Of Measurement";
            string Is_Sensitive_Unit_Of_Measurement = "Is Sensitive(Unit Of Measurement)";
            string Is_Sensitive_MRP = "Is Sensitive MRP";
            string Category = "Category";
            string Select_Sub_Category = "Select Sub Category";
            string Season_Name = "Season Name";
            string Sub_Sub_Category = "Sub Sub Category ";
            string Minimum_Purchase_Order_qty = "Minimum Purchase Order qty";
            string Minimum_Selling_Order_qty = "Minimum Selling Order qty";
            string Cess_Group_Name = "Cess Group Name";
            string Tax_Group_Name = "Tax Group Name";
            string HSN_Code = "HSN Code";
            string Hindi_Name = "Hindi Name";
            string Is_Daily_Essential = "Is Daily Essential";
            string Show_Types = "Show Types";
            string Item_Type = "Item Type";
            string Weight = "Weight";
            string Weight_Type = "Weight Type ";
            string Base_Scheme = "Base Scheme";
            string PTR = "PTR";
            string Clearance_ShelfLife_From = "Clearance ShelfLife From";
            string Clearance_ShelfLife_To = "Clearance ShelfLife To";
            string NonSellable_ShelfLife_From = "NonSellable ShelfLife From";
            string NonSellable_ShelfLife_To = "NonSellable ShelfLife To";
            headerlist.Add(Item_Base_Name); headerlist.Add(Unit_Of_Quantity);
            headerlist.Add(MRP_Price); headerlist.Add(Unit_Of_Measurement);
            headerlist.Add(Is_Sensitive_Unit_Of_Measurement); headerlist.Add(Is_Sensitive_MRP);
            headerlist.Add(Category); headerlist.Add(Select_Sub_Category);
            headerlist.Add(Season_Name); headerlist.Add(Sub_Sub_Category);
            headerlist.Add(Minimum_Purchase_Order_qty); headerlist.Add(Minimum_Selling_Order_qty);
            headerlist.Add(Cess_Group_Name); headerlist.Add(Tax_Group_Name);
            headerlist.Add(HSN_Code); headerlist.Add(Hindi_Name); headerlist.Add(Is_Daily_Essential); headerlist.Add(Show_Types);
            headerlist.Add(Item_Type); headerlist.Add(Weight);
            headerlist.Add(Weight_Type); headerlist.Add(Base_Scheme);
            headerlist.Add(PTR); headerlist.Add(Clearance_ShelfLife_From);
            headerlist.Add(Clearance_ShelfLife_To); headerlist.Add(NonSellable_ShelfLife_From);
            headerlist.Add(NonSellable_ShelfLife_To);
            DataColumnCollection columns = dt.Columns;
            if (dt != null && dt.Rows.Count > 0)
            {
                if (headerlist.Any(x => !columns.Contains(x)))
                {
                    var header = headerlist.FirstOrDefault(x => !columns.Contains(x));
                    result.Message = header + " Does Not exists in a table";
                    return result;
                }
                else
                {
                    using (var db = new AuthContext())
                    {
                        categoryname = dt.AsEnumerable().Select(s => s.Field<string>("Category")).ToList<string>();
                        subcategoryname = dt.AsEnumerable().Select(s => s.Field<string>("Select Sub Category")).ToList<string>();
                        subsubcategoryname = dt.AsEnumerable().Select(s => s.Field<string>("Sub Sub Category ")).ToList<string>();
                        basenames = dt.AsEnumerable().Select(s => s.Field<string>("Item Base Name")).ToList<string>();
                        if (categoryname != null && categoryname.Any())
                        {
                            categoryname = categoryname.Distinct().ToList();
                            var categorydata = db.Categorys.Where(x => categoryname.Contains(x.CategoryName)).ToList();
                            if (categorydata != null && categorydata.Any())
                            {
                                if (categorydata.Count != categoryname.Count)
                                {
                                    result.Message = "Category name Mismatch";
                                    return result;
                                }
                            }
                            else
                            {
                                result.Message = "Category name Does not exists in a Database";
                                return result;
                            }
                        }
                        else
                        {
                            result.Message = "Category name Does not exists in a excel";
                            return result;
                        }
                        if (subcategoryname != null && subcategoryname.Any())
                        {
                            subcategoryname = subcategoryname.Distinct().ToList();
                            var subcategorydata = db.SubCategorys.Where(x => subcategoryname.Contains(x.SubcategoryName)).ToList();
                            if (subcategorydata != null && subcategorydata.Any())
                            {
                                if (subcategorydata.Count != subcategoryname.Count)
                                {
                                    result.Message = "SubCategory name Mismatch";
                                    return result;
                                }
                            }
                            else
                            {
                                result.Message = "SubCategory name Does not exists in a Database";
                                return result;
                            }
                        }
                        else
                        {
                            result.Message = "SubCategory name Does not exists in a excel";
                            return result;
                        }
                        if (subsubcategoryname != null && subsubcategoryname.Any())
                        {
                            subsubcategoryname = subsubcategoryname.Distinct().ToList();
                            var subsubcategorydata = db.SubsubCategorys.Where(x => subsubcategoryname.Contains(x.SubsubcategoryName)).ToList();
                            if (subsubcategorydata != null && subsubcategorydata.Any())
                            {
                                if (subsubcategorydata.Count != subsubcategoryname.Count)
                                {
                                    result.Message = "SubSubCategory name Mismatch";
                                    return result;
                                }
                            }
                            else
                            {
                                result.Message = "SubSubCategory name Does not exists in a Database";
                                return result;
                            }
                        }
                        else
                        {
                            result.Message = "SubSubCategory name Does not exists in a excel";
                            return result;
                        }
                        if (basenames != null && basenames.Any())
                        {
                            basenames = basenames.Distinct().ToList();
                            var basenamedata = db.ItemMasterCentralDB.Where(x => basenames.Contains(x.itemBaseName)).ToList();
                            if (basenamedata != null && basenamedata.Any())
                            {
                                List<string> itemsname = new List<string>();
                                string alreadyexistsname = null;
                                itemsname = basenamedata.Where(x => basenames.Contains(x.itemBaseName)).Select(y => y.itemBaseName).ToList();
                                alreadyexistsname = string.Join(",", itemsname.ToList());
                                result.Message = "base name " + alreadyexistsname + " Already present in database";
                                return result;
                            }

                        }
                        else
                        {
                            result.Message = "base name Does not exists in a excel";
                            return result;
                        }
                        List<BulkItemUploadDC> bulkItemUploadDCs = new List<BulkItemUploadDC>();
                        for (int i = 0; i < dt.Rows.Count; i++)
                        {
                            try
                            {
                                DataRow row = dt.Rows[i];
                                BulkItemUploadDC bulk = new BulkItemUploadDC();
                                bulk.Item_Base_Name = row["Item Base Name"].ToString();
                                if (string.IsNullOrEmpty(bulk.Item_Base_Name))
                                {
                                    result.Message = "Item Base Name is Empty at " + (i + 1) + " row";
                                    return result;
                                }

                                if (row["Unit Of Quantity"] == DBNull.Value || row["Unit Of Quantity"].ToString() == "NULL")
                                    bulk.Unit_Of_Quantity = 0;
                                else
                                    bulk.Unit_Of_Quantity = Convert.ToDouble(row["Unit Of Quantity"]);
                                if (bulk.Unit_Of_Quantity == 0)
                                {
                                    result.Message = "Unit Of Quantity is Empty at " + (i + 1) + " row";
                                    return result;
                                }
                                if (row["MRP Price"] == DBNull.Value || row["MRP Price"].ToString() == "NULL")
                                    bulk.MRP_Price = 0;
                                else
                                    bulk.MRP_Price = Convert.ToDouble(row["MRP Price"]);
                                if (bulk.MRP_Price == 0)
                                {
                                    result.Message = "MRP Price is Empty at " + (i + 1) + " row";
                                    return result;
                                }
                                bulk.Unit_Of_Measurement = row["Unit Of Measurement"].ToString();
                                bulk.Unit_Of_Measurement = row["Unit Of Measurement"].ToString();
                                bulk.Is_Sensitive_Unit_Of_Measurement = row["Is Sensitive(Unit Of Measurement)"].ToString();
                                bulk.Is_Sensitive_MRP = row["Is Sensitive MRP"].ToString();
                                bulk.Category = row["Category"].ToString();
                                bulk.Select_Sub_Category = row["Select Sub Category"].ToString();
                                bulk.Season_Name = row["Season Name"].ToString();
                                bulk.Sub_Sub_Category = row["Sub Sub Category "].ToString();
                                if (row["Minimum Purchase Order qty"] == DBNull.Value || row["Minimum Purchase Order qty"].ToString() == "NULL")
                                    bulk.Minimum_Purchase_Order_qty = 0;
                                else
                                    bulk.Minimum_Purchase_Order_qty = Convert.ToDouble(row["Minimum Purchase Order qty"]);
                                if (row["Minimum Selling Order qty"] == DBNull.Value || row["Minimum Selling Order qty"].ToString() == "NULL")
                                    bulk.Minimum_Selling_Order_qty = 0;
                                else
                                    bulk.Minimum_Selling_Order_qty = Convert.ToDouble(row["Minimum Selling Order qty"]);
                                bulk.Cess_Group_Name = row["Cess Group Name"].ToString();
                                bulk.Tax_Group_Name = row["Tax Group Name"].ToString();
                                bulk.Hindi_Name = row["Hindi Name"].ToString();
                                bulk.Is_Daily_Essential = row["Is Daily Essential"].ToString();
                                bulk.Show_Types = row["Show Types"].ToString();
                                bulk.Item_Type = row["Item Type"].ToString();
                                bulk.Weight_Type = row["Weight Type "].ToString();
                                if (row["Base Scheme"] == DBNull.Value || row["Base Scheme"].ToString() == "NULL")
                                    bulk.Base_Scheme = 0;
                                else
                                    bulk.Base_Scheme = Convert.ToDouble(row["Base Scheme"]);
                                if (row["PTR"] == DBNull.Value || row["PTR"].ToString() == "NULL")
                                    bulk.PTR = 0;
                                else
                                    bulk.PTR = Convert.ToDouble(row["PTR"]);
                                if (row["Clearance ShelfLife From"] == DBNull.Value || row["Clearance ShelfLife From"].ToString() == "NULL")
                                    bulk.Clearance_ShelfLife_From = 0;
                                else
                                    bulk.Clearance_ShelfLife_From = Convert.ToDouble(row["Clearance ShelfLife From"]);
                                if (row["Clearance ShelfLife To"] == DBNull.Value || row["Clearance ShelfLife To"].ToString() == "NULL")
                                    bulk.Clearance_ShelfLife_To = 0;
                                else
                                    bulk.Clearance_ShelfLife_To = Convert.ToDouble(row["Clearance ShelfLife To"]);
                                if (row["NonSellable ShelfLife From"] == DBNull.Value || row["NonSellable ShelfLife From"].ToString() == "NULL")
                                    bulk.NonSellable_ShelfLife_From = 0;
                                else
                                    bulk.NonSellable_ShelfLife_From = Convert.ToDouble(row["NonSellable ShelfLife From"]);
                                if (row["NonSellable ShelfLife To"] == DBNull.Value || row["NonSellable ShelfLife To"].ToString() == "NULL")
                                    bulk.NonSellable_ShelfLife_To = 0;
                                else
                                    bulk.NonSellable_ShelfLife_To = Convert.ToDouble(row["NonSellable ShelfLife To"]);
                                //if (row["HSN Code"] == DBNull.Value || row["HSN Code"].ToString() == "NULL")
                                //    bulk.HSN_Code = "";
                                //else
                                bulk.HSN_Code = row["HSN Code"].ToString();
                                if (string.IsNullOrEmpty(bulk.HSN_Code))
                                    bulk.HSN_Code = Convert.ToString(0);
                                if (row["Weight"] == DBNull.Value || row["Weight"].ToString() == "NULL")
                                    bulk.Weight = 0;
                                else
                                    bulk.Weight = Convert.ToDouble(row["Weight"]);
                                bulkItemUploadDCs.Add(bulk);
                            }
                            catch (Exception ex)
                            {
                                result.Message = "Error: " + ex.Message.ToString();
                                return result;
                            }

                        }

                        int check = 0;
                        string getInsert = "";
                        if (bulkItemUploadDCs != null && bulkItemUploadDCs.Any())
                        {
                            foreach (var item in bulkItemUploadDCs)
                            {
                                if (check == 0)
                                {
                                    getInsert = "INSERT INTO ItemMasterData([Item Base Name] ,[Unit Of Quantity] " +
                                        ",[MRP Price] ,[Unit Of Measurement] ,[Is Sensitive(Unit Of Measurement)] " +
                                        ",[Is Sensitive MRP] ,[Category] ,[Select Sub Category] ,[Season Name] ," +
                                        "[Sub Sub Category ] ,[Minimum Purchase Order qty] ,[Minimum Selling Order qty] " +
                                        ",[Cess Group Name] ,[Tax Group Name] ,[HSN Code] ,[Hindi Name] ," +
                                        "[Is Daily Essential] ,[Show Types] ,[Item Type] ,[Weight] ,[Weight Type ]  ," +
                                        "[Base Scheme] ,[PTR] ,[Clearance ShelfLife From] ,[Clearance ShelfLife To] " +
                                        ",[NonSellable ShelfLife From] ,[NonSellable ShelfLife To] )";
                                    getInsert += " select '" + item.Item_Base_Name + "'," + item.Unit_Of_Quantity + "," +
                                        item.MRP_Price + ",'" + item.Unit_Of_Measurement + "','" + item.Is_Sensitive_Unit_Of_Measurement +
                                        "','" + item.Is_Sensitive_MRP + "','" + item.Category + "','" + item.Select_Sub_Category + "','" + item.Season_Name +
                                        "','" + item.Sub_Sub_Category + "'," + item.Minimum_Purchase_Order_qty + "," + item.Minimum_Selling_Order_qty +
                                        ",'" + item.Cess_Group_Name + "','" + item.Tax_Group_Name + "','" + item.HSN_Code + "',N'" + item.Hindi_Name +
                                        "','" + item.Is_Daily_Essential + "','" + item.Show_Types + "','" + item.Item_Type + "'," + item.Weight + ",'" + item.Weight_Type +
                                        "'," + item.Base_Scheme + "," + item.PTR + "," + item.Clearance_ShelfLife_From + "," + item.Clearance_ShelfLife_To +
                                        "," + item.NonSellable_ShelfLife_From + "," + item.NonSellable_ShelfLife_To;
                                    check++;
                                }
                                else
                                {
                                    getInsert += " UNION ALL ";
                                    getInsert += " select '" + item.Item_Base_Name + "'," + item.Unit_Of_Quantity + "," +
                                        item.MRP_Price + ",'" + item.Unit_Of_Measurement + "','" + item.Is_Sensitive_Unit_Of_Measurement +
                                        "','" + item.Is_Sensitive_MRP + "','" + item.Category + "','" + item.Select_Sub_Category + "','" + item.Season_Name +
                                        "','" + item.Sub_Sub_Category + "'," + item.Minimum_Purchase_Order_qty + "," + item.Minimum_Selling_Order_qty +
                                        ",'" + item.Cess_Group_Name + "','" + item.Tax_Group_Name + "','" + item.HSN_Code + "',N'" + item.Hindi_Name +
                                        "','" + item.Is_Daily_Essential + "','" + item.Show_Types + "','" + item.Item_Type + "'," + item.Weight + ",'" + item.Weight_Type +
                                        "'," + item.Base_Scheme + "," + item.PTR + "," + item.Clearance_ShelfLife_From + "," + item.Clearance_ShelfLife_To +
                                        "," + item.NonSellable_ShelfLife_From + "," + item.NonSellable_ShelfLife_To;
                                    check++;
                                }
                            }
                            if (!string.IsNullOrEmpty(getInsert))
                            {
                                int insertedcount = db.Database.ExecuteSqlCommand(getInsert);
                                if (insertedcount > 0)
                                {
                                    List<BulkItemUploadDC> itemwithnumber = new List<BulkItemUploadDC>();
                                    string date_time = DateTime.Now.ToString("ddMMyyyyHHmmss");
                                    var warehouseidparam = new SqlParameter("@WhId", warehouseid);
                                    itemwithnumber = db.Database.SqlQuery<BulkItemUploadDC>("TestCreateItemUsingTempTable @WhId", warehouseidparam).ToList();
                                    try
                                    {
                                        if (itemwithnumber != null && itemwithnumber.Any())
                                        {
                                            DataTable dtt = ListtoDataTableConverter.ToDataTable(itemwithnumber);
                                            string path = HttpContext.Current.Server.MapPath("~/UploadedFiles/ConsumerItemDownloaded");
                                            string fileName = "ItemCreated" + date_time;
                                            string fileNamexls = "ItemCreated" + date_time + ".xlsx";
                                            string fileUrl = null;
                                            if (!Directory.Exists(path))
                                            {
                                                Directory.CreateDirectory(path);
                                            }

                                            string paths = Path.Combine(HttpContext.Current.Server.MapPath("~/UploadedFiles/ConsumerItemDownloaded"), fileNamexls);
                                            ExcelGenerator.DataTable_To_Excel(dtt, fileName, paths);
                                            fileUrl = string.Format("{0}://{1}{2}/{3}", new Uri((HttpContext.Current.Request.UrlReferrer != null ? HttpContext.Current.Request.UrlReferrer.AbsoluteUri : HttpContext.Current.Request.Url.AbsoluteUri)).Scheme
                                                                                            , HttpContext.Current.Request.Url.DnsSafeHost
                                                                                            , (HttpContext.Current.Request.Url.Port != 80 && HttpContext.Current.Request.Url.Port != 443 ? ":" + HttpContext.Current.Request.Url.Port : "")
                                                                                            , string.Format("UploadedFiles/ConsumerItemDownloaded/{0}", fileNamexls));
                                            result.Message = "Uploaded successfully";
                                            result.ExcelUrl = fileUrl;
                                            return result;
                                        }
                                        else
                                        {
                                            result.Message = "Error in inserting data in a tables";
                                            return result;
                                        }

                                    }
                                    catch (Exception ex)
                                    {
                                        result.Message = "Error: " + ex.Message.ToString();
                                        return result;
                                    }


                                }
                                else
                                {
                                    result.Message = "Error in inserting data in a temp tables";
                                    return result;
                                }
                            }
                            else
                            {
                                result.Message = "Error in inserting data";
                                return result;
                            }

                        }
                        else
                        {
                            result.Message = "Item not insert in DC";
                            return result;
                        }

                    }

                }
            }
            else
            {
                result.Message = "Data not found in Excel";
            }

            return result;


        }

        [Route("ItemBarCodeUpload")]
        [HttpPost]
        [AllowAnonymous]
        public string ItemBarCodeUpload()
        {
            string res = "";
            DataTable dt = null;
            if (HttpContext.Current.Request.Files.AllKeys.Any())
            {
                var httppostedfile = HttpContext.Current.Request.Files["file"];
                if (httppostedfile != null)
                {
                    string ext = Path.GetExtension(httppostedfile.FileName);
                    if (ext == ".xlsx" || ext == ".xls")
                    {
                        string path = HttpContext.Current.Server.MapPath("~/UploadedFiles/ConsumerItemBarCodeUploaded");
                        string a1, b;
                        if (!Directory.Exists(path))
                        {
                            Directory.CreateDirectory(path);
                        }
                        a1 = DateTime.Now.ToString("ddMMyyyyHHmmss") + "_" + httppostedfile.FileName;
                        b = Path.Combine(HttpContext.Current.Server.MapPath("~/UploadedFiles/ConsumerItemBarCodeUploaded"), a1);
                        httppostedfile.SaveAs(b);

                        dt = Helpers.ExcelFileHelper.GetRequestsDataFromExcel(b);
                    }
                }
            }
            List<string> headerlist = new List<string>();
            string ItemNumber = "ItemNumber";
            string Barcode = "Barcode";
            headerlist.Add(ItemNumber); headerlist.Add(Barcode);
            DataColumnCollection columns = dt.Columns;
            if (dt != null && dt.Rows.Count > 0)
            {
                if (headerlist.Any(x => !columns.Contains(x)))
                {
                    var header = headerlist.FirstOrDefault(x => !columns.Contains(x));
                    res = header + " Does Not exists in a table";
                    return res;
                }
                else
                {
                    List<ItemBarCodeDC> bulkItemBarCodeDCs = new List<ItemBarCodeDC>();
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        try
                        {
                            DataRow row = dt.Rows[i];
                            ItemBarCodeDC bulk = new ItemBarCodeDC();
                            bulk.ItemNumber = row["ItemNumber"].ToString();
                            bulk.Barcode = row["Barcode"].ToString();
                            bulkItemBarCodeDCs.Add(bulk);
                        }
                        catch (Exception ex)
                        {
                            res = "Error: " + ex.Message.ToString();
                            return res;
                        }
                    }
                    int check = 0;
                    string getInsert = "";
                    if (bulkItemBarCodeDCs != null && bulkItemBarCodeDCs.Any())
                    {
                        foreach (var item in bulkItemBarCodeDCs)
                        {
                            if (check == 0)
                            {
                                getInsert = "insert into tempBarcode (ItemNumber,Barcode)";
                                getInsert += " select '" + item.ItemNumber + "','" + item.Barcode + "'" ;
                                check++;
                            }
                            else
                            {
                                getInsert += " UNION ALL ";
                                getInsert += " select '" + item.ItemNumber + "','" + item.Barcode + "'";
                                check++;
                            }
                        }
                    }
                    if (!string.IsNullOrEmpty(getInsert))
                    {
                        using (var db = new AuthContext())
                        {
                            int insertedcount = db.Database.ExecuteSqlCommand(getInsert);
                            if (insertedcount > 0)
                            {
                                string insertedquery = "insert into ItemBarcodes ( [ItemNumber],[Barcode],[IsParentBarcode],[IsVerified],[CreatedDate],[ModifiedDate],[IsActive],[IsDeleted],[CreatedBy],[ModifiedBy]) select t.ItemNumber,t.Barcode,1,1,GETDATE(),null,1,0,1,null from tempBarcode t  where not exists(select 1 from ItemBarcodes where ItemNumber=t.ItemNumber)  truncate table ItemBarcodes";
                                int insertedcounts = db.Database.ExecuteSqlCommand(insertedquery);
                                if (insertedcounts > 0)
                                {
                                    res = insertedcounts+" are affected";
                                    return res;
                                }
                                else
                                {
                                    res = insertedcounts + " are affected";
                                    return res;
                                }
                            }
                            else
                            {
                                res = "Error in inserting data in a temp tables";
                                return res;
                            }
                        }
                            

                    }
                }
            }
            else
            {
                res = "Data not found in Excel";
            }
            return res;
        }
    }
}
public class ItemSchemeSampleDC
{
    public string ClaimType { get; set; }
    public string CompanyCode { get; set; }
    public string CompanyStockCode { get; set; }
    public string ItemName { get; set; }
    public string ItemCode { get; set; }
    public int ItemMultiMRP { get; set; }
    public double MRP { get; set; }
    public double GST { get; set; }
    public double PTR { get; set; }
    public double TOTOnInvoice { get; set; }
    public string Category { get; set; }
    public string Subcategory { get; set; }
    public string Warehouse { get; set; }
    public string Salesoffice { get; set; }
    public double BaseScheme { get; set; }
    public double PrimaryScheme { get; set; }
    public int SlabQTY1 { get; set; }
    public double SlabScheme1 { get; set; }
    public int SlabQTY2 { get; set; }
    public double SlabScheme2 { get; set; }
    public int SlabQTY3 { get; set; }
    public double SlabScheme3 { get; set; }
    public int SlabQTY4 { get; set; }
    public double SlabScheme4 { get; set; }
    public double? QPSValueTarget { get; set; }
    public double? QPSValue { get; set; }
    public double? QPSQtyTarget { get; set; }
    public double? QPSQty { get; set; }
    public double? Promo { get; set; }
    public double? Visibility { get; set; }
    public string KVIorNonKVI { get; set; }
    public string CustomerType { get; set; }
    public double? AdditionalScheme { get; set; }
    public string LiquidationSupport { get; set; }
    public double OffInvoiceMargin { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}

public class ItemBarcodedDc
{
    public long Id { get; set; }
    public string Barcode { get; set; }
    public bool IsActive { get; set; }
    public string ItemNumber { get; set; }
    public int userid { get; set; }
}

public class ItemResponseMsg
{
    public bool Status { get; set; }
    public string Message { get; set; }
    public ItemMaster data { get; set; }


}

public class ResponseMsg
{
    public bool Status { get; set; }
    public string Message { get; set; }

}


public class INOutOnMultiMRPTransferDc
{
    public int WarehouseId { get; set; }
    public int SourceItemMultiMRPId { get; set; }
    public int DestinationItemMultiMRPId { get; set; }
    public int Qty { get; set; }
    public bool result { get; set; }

}

public class LiveDashboardPostDc
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}

public class CityBaseItemDc
{
    //public int ItemId { get; set; }
    //public int WarehouseId { get; set; }
    public int ItemMultiMRPId { get; set; }
    //public int MinOrderQty { get; set; }
    //public double UnitPrice { get; set; }
    public double MRP { get; set; }
    public string itemBaseName { get; set; }
    public string Number { get; set; }
}

public class MRPSensitiveWarehouseStockDTO
{
    public int StockId { get; set; }
    public int CurrentInventory { get; set; }
    public string ItemNumber { get; set; }
    public string ItemName { get; set; }
    public int ItemMultiMRPId { get; set; }
    public double MRP { get; set; }

    public bool IsSelected { get; set; }
    public int WarehouseId { get; set; }
    public string WarehouseName { get; set; }
}


public class UpdateEanDc
{
    public string ItemNumber { get; set; }
    public string EanNumber { get; set; }
    public int userId { get; set; }

}

public class UpdateItemBarCodeDc
{
    public string ItemNumber { get; set; }
    public string EanNumber { get; set; }


}
public class AddItemSchemeDC
{
    public long Id { get; set; }
    public int cityId { get; set; }
    public int ItemMultiMRPId { get; set; }
    public double PTR { get; set; }
    public double BaseScheme { get; set; }
}

public class SensitiveStockTransfer
{
    public int warehouseid { get; set; }
    public int itemid { get; set; }
    public string itemnumber { get; set; }
}

public class warehouseSensitiveitemlist
{
    public int WarehouseId { get; set; }
    public string WarehouseName { get; set; }
    public List<SensitiveItemMRPList> Sensitiveitemlist { get; set; }
}

public class SensitiveItemMRPList
{
    public int StockId { get; set; }
    public int WarehouseId { get; set; }
    public string WarehouseName { get; set; }
    public int CurrentInventory { get; set; }
    public string ItemNumber { get; set; }
    public string ItemName { get; set; }
    public int ItemMultiMRPId { get; set; }
    public double MRP { get; set; }
    public bool SelectedMRP { get; set; }
}
public class RelatedItem
{
    public int warehouseId { get; set; }
    public int customerId { get; set; }
    public string lang { get; set; }
    public int skip { get; set; }
    public int take { get; set; }
    public List<int> itemIds { get; set; }

}

public class ItemMasterCentralList
{
    public List<ItemMasterCentral> ItemMasterCentral { get; set; }
    public List<ItemMasterCentral> ItemMasterCentralGroupByNumber { get; set; }
}
public class SearchItem
{
    public string lang { get; set; }
    public string itemkeyword { get; set; }
    public int CustomerId { get; set; }
    public int minPrice { get; set; }
    public int maxPrice { get; set; }
    public string UOM { get; set; }
    public string Unit { get; set; }
    public List<int> BaseCat { get; set; }
    public List<int> Category { get; set; }
    public List<int> SubCat { get; set; }
    public List<int> Brand { get; set; }
    public string Number { get; set; }
    public string BarCode { get; set; }
    public bool IsActive { get; set; }
}
public class ItemListDeactivated
{

    public string itemnumber { get; set; }
    public int ItemMultiMRPId { get; set; }
    public string warehousename { get; set; }

    public List<ItemListDeactivatedDetail> ItemListDeactivatedDetails { get; set; }
}
public class ItemListDeactivatedDetail
{
    public string CityName { get; set; }
    public int Cityid { get; set; }
    public string CategoryName { get; set; }
    public int CategoryCode { get; set; }
    public string SubcategoryName { get; set; }
    public string SubsubcategoryName { get; set; }
    public string BrandCode { get; set; }
    //public int WarehouseId { get; set; }
    public string itemname { get; set; }
    public string itemcode { get; set; }
    public int ItemMultiMRPId { get; set; }
    public string Number { get; set; }
    public string SellingSku { get; set; }
    public double price { get; set; }
    public double PurchasePrice { get; set; }
    public double UnitPrice { get; set; }
    public int MinOrderQty { get; set; }
    public string SellingUnitName { get; set; }
    public int PurchaseMinOrderQty { get; set; }
    public string StoringItemName { get; set; }
    public string PurchaseSku { get; set; }
    public string PurchaseUnitName { get; set; }
    public string SupplierName { get; set; }
    public string SUPPLIERCODES { get; set; }
    public string BaseCategoryName { get; set; }
    public string TGrpName { get; set; }
    public double TotalTaxPercentage { get; set; }
    public string WarehouseName { get; set; }
    public string HindiName { get; set; }
    public double SizePerUnit { get; set; }
    public string Barcode { get; set; }
    public bool Active { get; set; }
    public bool Deleted { get; set; }
    public double Margin { get; set; }
    public int? PromoPoint { get; set; }
    public string HSNCode { get; set; }
    public bool IsSensitive { get; set; }
    //public string ABCClassification { get; set; }

    //public string ItemName { get; set; }
    //public string ItemBaseName { get; set; }
    //public string WH { get; set; }
    //public double MRPPrice { get; set; }
    //public double SellingPrice { get; set; }
    //public double PurchasePrice { get; set; }
    //public double NetPurchasePriceWithoutTax { get; set; }
    //public string PurchaseUnitType { get; set; }
    //public string SellingUnitName { get; set; }
    //public double TotalTax { get; set; }
    //public int MinItemOrder { get; set; }

    //public bool Active { get; set; }
}
public class ItemListActivated
{

    public string itemnumber { get; set; }
    public int ItemMultiMRPId { get; set; }
    public string warehousename { get; set; }
    public List<ItemListActivatedDetail> ItemListActivatedDetail { get; set; }
}
public class ItemListActivatedDetail
{
    public int SubsubcatId { get; set; }
    public string subsubcatname { get; set; }
    public string cateName { get; set; }
}

public class ItemSchemeData
{
    public string CityName { get; set; }
    public string itemBaseName { get; set; }
    public string CategoryName { get; set; }
    public string SubcategoryName { get; set; }
    public string SubsubcategoryName { get; set; }
    public int ItemMultiMRPId { get; set; }
    public int Cityid { get; set; }
    public double PTR { get; set; }
    public double BaseScheme { get; set; }
    public int totalRecord { get; set; }
    public long Id { get; set; }
    public double MRP { get; set; }
    public string CreatedBy { get; set; }
}
public class ItemSchemeResponse
{
    public bool Status { get; set; }
    public string Message { get; set; }
}

public class CityWiseItemListDC
{
    public int Cityid { get; set; }
    public int ItemMultiMRPId { get; set; }
    public double MRP { get; set; }
    public string itemBaseName { get; set; }
    public string itemNameWithmultiMrpId { get; set; }
    public string Msg { get; set; }
}



public class ItemMasterDTO
{
    public int ItemId { get; set; }
    public int Cityid { get; set; }
    public string CityName { get; set; }
    public string ABCClassification { get; set; }
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
    public string itemBaseName { get; set; }
    public string ShowTypes { get; set; }
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
    // public string Barcode { get; set; }
    //public string ItemType { get; set; }
    public double Margin { get; set; }
    public int? marginPoint { get; set; }
    public int? promoPoint { get; set; }
    public int? promoPerItems { get; set; }
    public double NetPurchasePrice { get; set; }
    public bool IsPramotionalItem { get; set; }
    public int? CessGrpID { get; set; }
    public string CessGrpName { get; set; }
    public double TotalCessPercentage { get; set; }
    public int ItemMultiMRPId { get; set; }
    public string UnitofQuantity { get; set; }
    public string UOM { get; set; }//Unit of masurement like GM Kg 
    public int DepoId { get; set; }
    //Anu (29/04/2019)
    public bool IsSensitive { get; set; }//Is Sensitive Yes/No
    public bool IsSensitiveMRP { get; set; }
    public bool IsBulkItem
    {
        get; set;
    }

    public bool IsHighestDPItem
    {
        get; set;
    }

    public bool IsOffer
    {
        get; set;
    }

    public bool inTally
    {
        get; set;
    }
    [NotMapped]
    public int CurrentStock { get; set; }

    [NotMapped]
    public int? ItemLimitId { get; set; }
    public int? BuyerId { get; set; }
    public string BuyerName { get; set; }

    public int ItemlimitQty { get; set; }
    public double? DistributionPrice { get; set; }
    public bool IsReplaceable { get; set; }
    public bool DistributorShow { get; set; }
    public int MaterialItemId { get; set; }
    public int Type { get; set; }
    public int ItemAppType { get; set; }
    public bool IsDisContinued { get; set; }// Is Item Is DisContinue

    //increases
    public bool IsSellerStoreItem { get; set; }   //IsSellerStoreItem

    public double? SellerStorePrice { get; set; }//SellerStorePrice
    [NotMapped]
    public int Tag { get; set; } // new add for ROC
    public double? TradePrice { get; set; }
    public double? WholeSalePrice { get; set; }
    public bool EnableAutoPrice { get; set; }
    public bool IsCommodity { get; set; }

    public int TotalCount { get; set; }
}


public class ItemMasterFullWhexportDTO
{
    //public int ItemId { get; set; }
    public string CityName { get; set; }
    public int Cityid { get; set; }
    public string CategoryName { get; set; }
    public string CategoryCode { get; set; }
    public string SubcategoryName { get; set; }
    public string SubsubcategoryName { get; set; }
    public string BrandCode { get; set; }
    public string itemname { get; set; }
    public string itemcode { get; set; }
    public string Number { get; set; }
    public string SellingSku { get; set; }
    public double price { get; set; }
    public double PurchasePrice { get; set; }
    public double UnitPrice { get; set; }
    public double NetPurchasePrice { get; set; }
    public int MinOrderQty { get; set; }
    public string SellingUnitName { get; set; }
    public int PurchaseMinOrderQty { get; set; }
    public string StoringItemName { get; set; }
    public string PurchaseSku { get; set; }
    public string PurchaseUnitName { get; set; }
    public string SupplierName { get; set; }
    public string SUPPLIERCODES { get; set; }
    public string BaseCategoryName { get; set; }
    public string TGrpName { get; set; }
    public double TotalTaxPercentage { get; set; }
    public int WarehouseId { get; set; }
    public string WarehouseName { get; set; }
    public string HindiName { get; set; }
    public double SizePerUnit { get; set; }
    public bool Active { get; set; }
    public bool Deleted { get; set; }
    public double Margin { get; set; }
    public int? PromoPoint { get; set; }
    public string HSNCode { get; set; }
    public bool IsSensitive { get; set; }
    public int ItemMultiMRPId { get; set; }
    public string ShowTypes { get; set; }
    public string ABCClassification { get; set; }

}
public class factoryItemdata
{
    public bool active { get; set; }
    public int ItemId { get; set; }
    public int ItemlimitQty { get; set; }
    public bool IsItemLimit { get; set; }
    public string ItemNumber { get; set; }
    public int CompanyId { get; set; }
    public int WarehouseId { get; set; }
    public int DepoId { get; set; }
    public string DepoName { get; set; }
    public int BaseCategoryId { get; set; }
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
    public double? marginPoint { get; set; }
    public int? promoPerItems { get; set; }
    public int? dreamPoint { get; set; }
    public bool IsOffer { get; set; }//
                                     //by sachin (Date 14-05-2019)
                                     //public bool Isoffer { get; set; }
    public bool IsSensitive { get; set; }//sudhir
    public bool IsSensitiveMRP { get; set; }//sudhir
    public string UnitofQuantity { get; set; }//sudhir
    public string UOM { get; set; }//sudhir
    public string itemBaseName { get; set; }
    public bool Deleted { get; set; }
    public double NetPurchasePrice { get; set; }

    public bool IsFlashDealUsed { get; set; }
    public int ItemMultiMRPId { get; set; }
    public int BillLimitQty { get; set; }
    public DateTime? LastOrderDate { get; set; }
    public int? LastOrderQty { get; set; }
    public int? LastOrderDays { get; set; }
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
    public int? FreeItemId
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
    public double? FlashDealSpecialPrice
    {
        get; set;
    }
    public int? FlashDealMaxQtyPersonCanTake
    {
        get; set;
    }

    public double? DistributionPrice { get; set; }
    public double? TradePrice { get; set; }
    public double? WholeSalePrice { get; set; }
    public bool DistributorShow { get; set; }
    public int ItemAppType { get; set; }
    public bool IsPrimeItem { get; set; }
    public decimal PrimePrice { get; set; }
    public DateTime CurrentStartTime { get; set; }
    public string Scheme { get; set; }
    public double Score { get; set; }

    public string Classification { get; set; }
    public string BackgroundRgbColor { get; set; }
    public string IncentiveClassification { get; set; }

}

public class ProductDetails
{
    public bool active;
    public int ItemId { get; set; }
    public int ItemlimitQty { get; set; }
    public bool IsItemLimit { get; set; }
    public string Number { get; set; }
    public int CompanyId { get; set; }
    public int WarehouseId { get; set; }
    public int DepoId { get; set; }
    public string DepoName { get; set; }
    public int BaseCategoryId { get; set; }
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
    public int? dreamPoint { get; set; }
    public bool IsOffer { get; set; }//
                                     //by sachin (Date 14-05-2019)
    public bool Isoffer { get; set; }
    public bool IsSensitive { get; set; }//sudhir
    public bool IsSensitiveMRP { get; set; }//sudhir
    public string UnitofQuantity { get; set; }//sudhir
    public string UOM { get; set; }//sudhir
    public string itemBaseName { get; set; }
    public bool Deleted { get; internal set; }
    public double NetPurchasePrice { get; set; }

    public bool IsFlashDealUsed { get; set; }
    public int ItemMultiMRPId { get; set; }
    public int BillLimitQty { get; set; }

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
    public int? FreeItemId
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
    public double? FlashDealSpecialPrice
    {
        get; set;
    }
    public int? FlashDealMaxQtyPersonCanTake
    {
        get; set;
    }

}

public class factoryItemCentraldata
{
    public int ItemId { get; set; }
    public int WarehouseId { get; set; }
    public int Categoryid { get; set; }
    //  public int SubCategoryId { get; set; }
    public int SubsubCategoryid { get; set; }
    public string CategoryName { get; set; }
    public int BaseCategoryid { get; set; }
    // public string BaseCategoryName { get; set; }
    //  public string SubcategoryName { get; set; }
    public string SubsubcategoryName { get; set; }
    //  public string itemcode { get; set; }
    public double price { get; set; }
    public bool active { get; set; }
    public int PurchaseMinOrderQty { get; set; }
    public string SellingSku { get; set; }
    //   public DateTime CreatedDate { get; set; }
    //  public DateTime UpdatedDate { get; set; }
    public bool Deleted { get; set; }
    // public int userid { get; set; }
    public string itemname { get; set; }

    //  public string UpdateBy { get; set; }
    public string UOM { get; set; }

}

public class factorySubSubCategory
{
    public int SubsubCategoryid { get; set; }
    public string SubsubcategoryName { get; set; }
    public int Categoryid { get; set; }
    public int SubCategoryId { get; set; }
    public List<factoryItemdata> ItemMasters { get; set; }
}
public class WRSITEM
{
    public List<factoryItemdata> ItemMasters { get; set; }
    public List<factorySubSubCategory> SubsubCategories { get; set; }
    public bool Message { get; set; }

}

public class resDTO
{
    public List<factoryItemdata> ItemMasters { get; set; }
    public bool Status { get; set; }
    public string Message { get; set; }
}

public class OfferItemList
{
    public int ItemId { get; set; }
    public bool IsOffer { get; set; }
    public int? OfferCategory { get; set; }
    public DateTime? OfferStartTime { get; set; }
    public DateTime? OfferEndTime { get; set; }
    public double? OfferQtyAvaiable { get; set; }
    public string OfferType { get; set; }
}

public class ItemOfferList
{
    public List<OfferItemList> OfferItems { get; set; }
    public int CustomerId { get; set; }
    public int WarehouseId { get; set; }
    public bool Message { get; set; }
}

public class FlashDealWithItem
{
    public int FlashDealId { get; set; }
    public int ItemId { get; set; }

}
public class Itemshare
{
    public string Number { get; set; }
    public int ItemMultiMRPId { get; set; }
    public int WarehouseId { get; set; }

}

public class EditItemPriceAppDTO
{
    public int WarehouseId { get; set; }
    public int ItemMultiMRPId { get; set; }
    public double AvgPurPrice { get; set; }

}
public class BillItemLimitMaster
{
    public string Number { get; set; }
    public int WarehouseId { get; set; }
    public int itemBillLimitQty { get; set; }
}

public class ItemliveDashboardDc
{
    public string WarehouseName { get; set; }
    public int WarehouseId { get; set; }

    public int ActiveItems { get; set; }
    public int InActiveItems { get; set; }

    public int ActiveBrands { get; set; }
    public int InactiveBrands { get; set; }

    public int ActiveSubCategories { get; set; }
    public int inactiveSubCat { get; set; }

    public int ActiveCategories { get; set; }
    public int InactiveCategories { get; set; }


}

public class ItemMultiMRPAdd
{
    public bool isNew { get; set; }
    public string ItemNumber { get; set; }
    public double MRP { get; set; }
    public string UnitofQuantity { get; set; }
    public string UOM { get; set; }
    public string itemname { get; set; }
    public string itemBaseName { get; set; }
    public string CompanyStockCode { get; set; }
    public double ptr { get; set; }
    public double baseScheme { get; set; }
    public string ColourImage { get; set; }
    public string ColourName { get; set; }
    public List<MRPMediaDC> mrpMediaDC { get; set; }
}

public class MRPMediaDC
{
    public long? id { get; set; }
    public string Type { get; set; }  //Image ,Video
    public string Url { get; set; }
    public int SequenceNo { get; set; }
    public bool isActive { get; set; }
    public bool? isDeleted { get; set; }


}


public class ActivateConsumerStockDc
{
    public string WarehouseName { get; set; }
    public string ItemName { get; set; }
    public string ItemNumber { get; set; }
    public int CurrentInventory { get; set; }
    public int ItemMultiMrpId { get; set; }
    public int CurrentNetInventory { get; set; }
    public int WarehouseId { get; set; }

}
public class ActivateItemForConsumerStoreDc
{
    public int ItemMultiMrpId { get; set; }
    public int WarehouseId { get; set; }
    public int Qty { get; set; }

}

public class UpdateMRPMediaDC
{
    public int ItemMultiMRPId { get; set; }
    public string ColourImage { get; set; }
    public string ColourName { get; set; }
    public List<MRPMediaDC> mrpMediaDC { get; set; }
}

public class GetItemMRPbyItemNoDc
{
    public int ItemMultiMRPId { get; set; }
    public int CompanyId { get; set; }
    public string ItemNumber { get; set; }
    public double MRP { get; set; }
    public string UnitofQuantity { get; set; }
    public string UOM { get; set; }
    public string itemname { get; set; }
    public string itemBaseName { get; set; }
    public string CompanyStockCode { get; set; }
    public double? ptr { get; set; }
    public double? baseScheme { get; set; }
    public string CreatedBy { get; set; }
    public string ColourImage { get; set; }
    public string ColourName { get; set; }
    public List<MRPMediaDC> mrpMediaDC { get; set; }
}

public class GetConsumerItemMRP
{
    public int ItemMultiMRPId { get; set; }
    public string ItemNumber { get; set; }
    public double MRP { get; set; }
    public string ColourImage { get; set; }
    public string ColourName { get; set; }
    public bool? active { get; set; }
    public int ItemId { get; set; }
    public int WarehouseId { get; set; }
    public int SupplierId { get; set; }
    public string SupplierName { get; set; }
    public int DepoId { get; set; }
    public string DepoName { get; set; }
    public int BuyerId { get; set; }
    public string BuyerName { get; set; }
    public int ItemLimitQty { get; set; }
    public bool? LimitIsActive { get; set; }

}
public class ItemLImitMsg
{
    public ItemLimitMaster itemLimitMaster { get; set; }
    public bool Status { get; set; }
    public string Message { get; set; }
}

public class SeasonalConfigDC
{
    public int SeasonId { get; set; }
    public string SeasonName { get; set; }
}

public class OrderOfferDc
{
    public int OfferId { get; set; }
    public double QtyAvaiable { get; set; }
    public bool IsDispatchedFreeStock { get; set; }
    public int MinOrderQuantity { get; set; }
    public int NoOffreeQuantity { get; set; }
}
public class ItemNumbersDC
{
    public String ItemNumber { get; set; }
    public int Categoryid { get; set; }
    public String CategoryCode { get; set; }
    public String SubSubCategoryCode { get; set; }
    public int SubCategoryid { get; set; }
    public int SubSubCategoryid { get; set; }
}
public class BulkItemUploadDC
{
    public string Item_Base_Name { get; set; }
    public double Unit_Of_Quantity { get; set; }
    public double MRP_Price { get; set; }
    public string Unit_Of_Measurement { get; set; }
    public string Is_Sensitive_Unit_Of_Measurement { get; set; }
    public string Is_Sensitive_MRP { get; set; }
    public string Category { get; set; }
    public string Select_Sub_Category { get; set; }
    public string Season_Name { get; set; }
    public string Sub_Sub_Category { get; set; }
    public double Minimum_Purchase_Order_qty { get; set; }
    public double Minimum_Selling_Order_qty { get; set; }
    public string Cess_Group_Name { get; set; }
    public string Tax_Group_Name { get; set; }
    public string Hindi_Name { get; set; }
    public string Is_Daily_Essential { get; set; }
    public string Show_Types { get; set; }
    public string Item_Type { get; set; }
    public string Weight_Type { get; set; }
    public double Base_Scheme { get; set; }
    public double PTR { get; set; }
    public double Clearance_ShelfLife_From { get; set; }
    public double Clearance_ShelfLife_To { get; set; }
    public double NonSellable_ShelfLife_From { get; set; }
    public double NonSellable_ShelfLife_To { get; set; }
    public string HSN_Code { get; set; }
    public double Weight { get; set; }
    public string Number { get; set; }
    public string ItemCode { get; set; }
    public int ItemMultiMRPId { get; set; }
    public string SellingUnitName { get; set; }
}
public class BulkItemResponse
{
    public string Message { get; set; }
    public string ExcelUrl { get; set; }
}
public class ItemBarCodeDC
{
    public string ItemNumber { get; set; }
    public string Barcode { get; set; }
}


