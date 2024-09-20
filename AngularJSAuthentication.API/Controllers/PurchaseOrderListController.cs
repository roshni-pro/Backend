using AngularJSAuthentication.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Web.Http;
using System.Web.Http.Description;
using NLog;
using System.Data.Entity;
using System.ComponentModel.DataAnnotations.Schema;
using System.Web;
using System.Net.Mail;
using System.Data.SqlClient;
using System.Configuration;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using NPOI.HSSF.UserModel;
using System.IO;
using System.Data;
using AngularJSAuthentication.Model.PurchaseOrder;
using AngularJSAuthentication.API.ControllerV7.PurchaseRequestPayments;
using AngularJSAuthentication.API.Helper;
using AngularJSAuthentication.Common.Enums;

namespace AngularJSAuthentication.API.Controllers
{
    [RoutePrefix("api/PurchaseOrderList")]
    public class PurchaseOrderListController : ApiController
    {

        private static Logger logger = LogManager.GetCurrentClassLogger();
        private static TimeZoneInfo INDIAN_ZONE = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
        DateTime indianTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE);


        [Route("")]
        public IList<PurchaseList> Get(int wid, DateTime? datefrom, DateTime? dateto, int? ItemMultiMRPId, int? supplierId)
        {
            try
            {
                // Access claims
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


                List<PurchaseList> uniquelist = new List<PurchaseList>();
                if (Warehouse_id == 0)//Warehouse based get data
                {
                    Warehouse_id = wid;
                }
                using (var context = new AuthContext())
                {
                    Warehouse Wsc = context.Warehouses.Where(q => q.WarehouseId == wid).FirstOrDefault();
                    List<CurrentStock> CurrentStockList = new List<CurrentStock>();
                    List<int> multiMRPIds = new List<int>();
                    if (ItemMultiMRPId.HasValue)
                    {
                        multiMRPIds.Add(ItemMultiMRPId.Value);
                    }

                    if (supplierId.HasValue)
                    {
                        var mrpids = context.itemMasters.Where(x => x.WarehouseId == wid && x.Deleted == false && x.IsDisContinued == false && x.SupplierId == supplierId.Value)
                            .Select(x => x.ItemMultiMRPId).Distinct().ToList();
                        multiMRPIds.AddRange(mrpids);
                    }

                    if (datefrom != null)
                    {
                        List<Int32> OrderIds = context.DbOrderMaster.Where(x => x.Status == "Pending" && x.Deleted == false && x.WarehouseId == Warehouse_id && x.CreatedDate >= datefrom && x.CreatedDate <= dateto).Select(x => x.OrderId).ToList();
                        List<OrderDetails> OrderDetails = new List<OrderDetails>();
                        List<ItemMaster> HubitemList = new List<ItemMaster>();
                        if (multiMRPIds.Any())
                        {
                            OrderDetails = context.DbOrderDetails.Where(x => OrderIds.Contains(x.OrderId) && multiMRPIds.Contains(x.ItemMultiMRPId)).ToList();
                            HubitemList = context.itemMasters.Where(m => m.WarehouseId == Warehouse_id && m.Deleted == false && multiMRPIds.Contains(m.ItemMultiMRPId)).ToList();
                        }
                        else
                        {
                            OrderDetails = context.DbOrderDetails.Where(x => OrderIds.Contains(x.OrderId)).ToList();
                            HubitemList = context.itemMasters.Where(m => m.WarehouseId == Warehouse_id && m.Deleted == false).ToList();
                        }

                        var poList = (from a in OrderDetails
                                      join i in HubitemList on a.ItemId equals i.ItemId
                                      where i.WarehouseId == a.WarehouseId ///mrp added
                                      select new PurchaseOrderList
                                      {
                                          OrderDetailsId = a.OrderDetailsId,
                                          CompanyId = a.CompanyId,
                                          WarehouseId = a.WarehouseId,
                                          WarehouseName = a.WarehouseName,
                                          OrderDate = a.OrderDate,
                                          SupplierId = i.SupplierId,
                                          PurchaseSku = i.PurchaseSku,
                                          SupplierName = i.SupplierName,
                                          OrderId = a.OrderId,
                                          ItemId = a.ItemId,
                                          SKUCode = i.Number,
                                          ItemName = a.itemname,
                                          PurchaseUnitName = i.PurchaseUnitName,
                                          Unit = a.SellingUnitName,
                                          Conversionfactor = i.PurchaseMinOrderQty,
                                          Discription = "",
                                          qty = a.qty,
                                          StoringItemName = i.StoringItemName,
                                          Price = i.price,
                                          //NetPurchasePrice=a.Purchaseprice, // netpurchaseprice add By raj
                                          NetPurchasePrice = Math.Round(i.NetPurchasePrice * (1 + (i.TotalTaxPercentage / 100)), 3),
                                          NetAmmount = a.NetAmmount,
                                          TaxPercentage = a.TaxPercentage,
                                          TaxAmount = a.TaxAmmount,
                                          TotalAmountIncTax = a.TotalAmt,
                                          PurchaseMinOrderQty = i.PurchaseMinOrderQty,
                                          Status = a.Status,
                                          CreationDate = a.CreatedDate,
                                          Deleted = a.Deleted,
                                          ItemMultiMRPId = i.ItemMultiMRPId,
                                          DepoId = i.DepoId,
                                          DepoName = i.DepoName
                                      }).ToList();



                        List<string> ItemNumbers = poList.Select(x => x.SKUCode).Distinct().ToList();
                        List<int> ItemMultiMRPIds = poList.Select(x => x.ItemMultiMRPId).Distinct().ToList();
                        CurrentStockList = context.DbCurrentStock.Where(k => k.Deleted == false && k.WarehouseId == Warehouse_id && k.ItemMultiMRPId > 0 && ItemMultiMRPIds.Contains(k.ItemMultiMRPId)).ToList();
                        string sqlquery = "select b.CurrentInventory-sum(d.qty) CurrentInventory,d.ItemMultiMRPId,d.itemNumber from CurrentStocks b left join OrderDetails d "
                        + " on d.itemNumber = b.ItemNumber and d.ItemMultiMRPId = b.ItemMultiMRPId and d.WarehouseId = b.WarehouseId and d.Status = 'Pending'"
                        + " where d.itemNumber in ('" + string.Join("','", ItemNumbers) + "') and d.ItemMultiMRPId in(" + string.Join(",", ItemMultiMRPIds) + ") "
                        + " and exists(select WarehouseId from Warehouses w where w.WarehouseId<> " + wid + " and w.Cityid= " + Wsc.Cityid + " and w.WarehouseId= d.WarehouseId) "
                        + " group by d.ItemMultiMRPId,d.itemNumber,b.CurrentInventory ";

                        List<ItemDemandDc> ItemDemandDcs = context.Database.SqlQuery<ItemDemandDc>(sqlquery).ToList();

                        foreach (PurchaseOrderList item in poList)
                        {
                            int count = 0; //01AE101110
                            PurchaseList l = uniquelist.Where(x => x.PurchaseSku == item.PurchaseSku && x.ItemMultiMRPId == item.ItemMultiMRPId).SingleOrDefault();

                            if (l == null)
                            {
                                count += 1;
                                l = new PurchaseList();
                                l.name = item.ItemName;
                                l.conversionfactor = item.Conversionfactor;
                                l.Supplier = item.SupplierName;
                                l.SupplierId = item.SupplierId;
                                l.WareHouseId = item.WarehouseId;
                                l.CompanyId = item.CompanyId;
                                l.WareHouseName = item.WarehouseName;
                                l.OrderDetailsId = item.OrderDetailsId;
                                l.itemNumber = item.SKUCode;
                                l.PurchaseSku = item.PurchaseSku;
                                l.orderIDs = item.OrderId + "," + l.orderIDs;
                                l.ItemId = item.ItemId;
                                l.ItemName = item.ItemName;
                                l.qty = l.qty + item.qty;
                                l.currentinventory = item.CurrentInventory;
                                l.Price = item.Price;
                                l.NetPurchasePrice = item.NetPurchasePrice; // netpurchaseprice add By raj
                                l.ItemMultiMRPId = item.ItemMultiMRPId;//multimrp
                                l.DepoId = item.DepoId;
                                l.DepoName = item.DepoName;
                                l.NetStock = ItemDemandDcs.Any(x => x.ItemMultiMRPId == l.ItemMultiMRPId && x.itemNumber == l.itemNumber) ? ItemDemandDcs.FirstOrDefault(x => x.ItemMultiMRPId == l.ItemMultiMRPId && x.itemNumber == l.itemNumber).CurrentInventory : 0;
                                uniquelist.Add(l);
                            }
                            else
                            {
                                l.orderIDs = item.OrderId + "," + l.orderIDs;
                                l.qty = l.qty + item.qty;
                                l.NetStock = ItemDemandDcs.Any(x => x.ItemMultiMRPId == l.ItemMultiMRPId && x.itemNumber == l.itemNumber) ? ItemDemandDcs.FirstOrDefault(x => x.ItemMultiMRPId == l.ItemMultiMRPId && x.itemNumber == l.itemNumber).CurrentInventory : 0;
                                uniquelist.First(d => d.PurchaseSku == item.PurchaseSku).NetStock = l.NetStock;
                                uniquelist.First(d => d.PurchaseSku == item.PurchaseSku).qty = l.qty;
                                uniquelist.First(d => d.PurchaseSku == item.PurchaseSku).orderIDs = l.orderIDs;
                            }
                        }
                    }
                    else
                    {
                        List<Int32> OrderIds = context.DbOrderMaster.Where(x => x.Status == "Pending" && x.Deleted == false && x.WarehouseId == Warehouse_id).Select(x => x.OrderId).ToList();
                        List<OrderDetails> OrderDetails = new List<OrderDetails>();
                        List<ItemMaster> HubitemList = new List<ItemMaster>();
                        if (multiMRPIds.Any())
                        {
                            OrderDetails = context.DbOrderDetails.Where(x => OrderIds.Contains(x.OrderId) && multiMRPIds.Contains(x.ItemMultiMRPId)).ToList();
                            HubitemList = context.itemMasters.Where(m => m.WarehouseId == Warehouse_id && m.Deleted == false && multiMRPIds.Contains(m.ItemMultiMRPId)).ToList();
                        }
                        else
                        {
                            OrderDetails = context.DbOrderDetails.Where(x => OrderIds.Contains(x.OrderId)).ToList();
                            HubitemList = context.itemMasters.Where(m => m.WarehouseId == Warehouse_id && m.Deleted == false).ToList();
                        }

                        var poList = (from a in OrderDetails
                                      join i in HubitemList on a.ItemId equals i.ItemId
                                      where i.WarehouseId == a.WarehouseId///mrp added
                                      select new PurchaseOrderList
                                      {
                                          OrderDetailsId = a.OrderDetailsId,
                                          CompanyId = a.CompanyId,
                                          WarehouseId = a.WarehouseId,
                                          WarehouseName = a.WarehouseName,
                                          OrderDate = a.OrderDate,
                                          SupplierId = i.SupplierId,
                                          PurchaseSku = i.PurchaseSku,
                                          SupplierName = i.SupplierName,
                                          OrderId = a.OrderId,
                                          ItemId = a.ItemId,
                                          SKUCode = i.Number,
                                          ItemName = a.itemname,
                                          PurchaseUnitName = i.PurchaseUnitName,
                                          Unit = i.SellingUnitName,
                                          Conversionfactor = i.PurchaseMinOrderQty,
                                          Discription = "",
                                          qty = a.qty,
                                          //CurrentInventory = c == null ? 0 : c.CurrentInventory,
                                          StoringItemName = i.StoringItemName,
                                          Price = i.price,
                                          // NetPurchasePrice = a.Purchaseprice, // netpurchaseprice add By raj
                                          NetPurchasePrice = Math.Round(i.NetPurchasePrice * (1 + (i.TotalTaxPercentage / 100)), 3),
                                          NetAmmount = a.NetAmmount,
                                          TaxPercentage = a.TaxPercentage,
                                          TaxAmount = a.TaxAmmount,
                                          TotalAmountIncTax = a.TotalAmt,
                                          PurchaseMinOrderQty = i.PurchaseMinOrderQty,
                                          Status = a.Status,
                                          CreationDate = a.CreatedDate,
                                          Deleted = a.Deleted,
                                          ItemMultiMRPId = i.ItemMultiMRPId,
                                          DepoId = i.DepoId,
                                          DepoName = i.DepoName
                                      }).ToList();


                        List<string> ItemNumbers = poList.Select(x => x.SKUCode).Distinct().ToList();
                        List<int> ItemMultiMRPIds = poList.Select(x => x.ItemMultiMRPId).Distinct().ToList();
                        CurrentStockList = context.DbCurrentStock.Where(k => k.Deleted == false && k.WarehouseId == Warehouse_id && k.ItemMultiMRPId > 0 && ItemMultiMRPIds.Contains(k.ItemMultiMRPId)).ToList();

                        string sqlquery = "select b.CurrentInventory-sum(d.qty) CurrentInventory,d.ItemMultiMRPId,d.itemNumber from CurrentStocks b left join OrderDetails d "
                        + " on d.itemNumber = b.ItemNumber and d.ItemMultiMRPId = b.ItemMultiMRPId and d.WarehouseId = b.WarehouseId and d.Status = 'Pending'"
                        + " where d.itemNumber in ('" + string.Join("','", ItemNumbers) + "') and d.ItemMultiMRPId in(" + string.Join(",", ItemMultiMRPIds) + ") "
                        + " and exists(select WarehouseId from Warehouses w where w.WarehouseId<> " + wid + " and w.Cityid= " + Wsc.Cityid + " and w.WarehouseId= d.WarehouseId) "
                        + " group by d.ItemMultiMRPId,d.itemNumber,b.CurrentInventory ";

                        List<ItemDemandDc> ItemDemandDcs = context.Database.SqlQuery<ItemDemandDc>(sqlquery).ToList();

                        foreach (PurchaseOrderList item in poList)
                        {
                            int count = 0; //01AE101110
                            PurchaseList l = uniquelist.Where(x => x.PurchaseSku == item.PurchaseSku && x.ItemMultiMRPId == item.ItemMultiMRPId).FirstOrDefault();
                            if (l == null)
                            {
                                count += 1;
                                l = new PurchaseList();
                                l.name = item.ItemName;
                                l.conversionfactor = item.Conversionfactor;
                                l.Supplier = item.SupplierName;
                                l.SupplierId = item.SupplierId;
                                l.WareHouseId = item.WarehouseId;
                                l.CompanyId = item.CompanyId;
                                l.WareHouseName = item.WarehouseName;
                                l.OrderDetailsId = item.OrderDetailsId;
                                l.itemNumber = item.SKUCode;
                                l.PurchaseSku = item.PurchaseSku;
                                l.orderIDs = item.OrderId + "," + l.orderIDs;
                                l.ItemId = item.ItemId;
                                l.ItemName = item.ItemName;
                                l.qty = l.qty + item.qty;
                                l.currentinventory = item.CurrentInventory;
                                l.Price = item.Price;
                                l.NetPurchasePrice = item.NetPurchasePrice; // netpurchaseprice add By raj
                                l.ItemMultiMRPId = item.ItemMultiMRPId;
                                l.DepoId = item.DepoId;
                                l.DepoName = item.DepoName;
                                l.NetStock = ItemDemandDcs.Any(x => x.ItemMultiMRPId == l.ItemMultiMRPId && x.itemNumber == l.itemNumber) ? ItemDemandDcs.FirstOrDefault(x => x.ItemMultiMRPId == l.ItemMultiMRPId && x.itemNumber == l.itemNumber).CurrentInventory : 0;
                                uniquelist.Add(l);
                            }
                            else
                            {
                                l.orderIDs = item.OrderId + "," + l.orderIDs;
                                l.qty = l.qty + item.qty;
                                l.NetStock = ItemDemandDcs.Any(x => x.ItemMultiMRPId == l.ItemMultiMRPId && x.itemNumber == l.itemNumber) ? ItemDemandDcs.FirstOrDefault(x => x.ItemMultiMRPId == l.ItemMultiMRPId && x.itemNumber == l.itemNumber).CurrentInventory : 0;
                                uniquelist.First(d => d.PurchaseSku == item.PurchaseSku).qty = l.qty;
                                uniquelist.First(d => d.PurchaseSku == item.PurchaseSku).orderIDs = l.orderIDs;
                            }
                        }
                    }
                    List<PurchaseList> cc = new List<PurchaseList>();
                    foreach (PurchaseList l in uniquelist)
                    {
                        CurrentStock cs = CurrentStockList.Where(k => k.ItemNumber == l.itemNumber && k.ItemMultiMRPId == l.ItemMultiMRPId).FirstOrDefault();//multi mrp
                        if (cs != null)
                        {
                            l.currentinventory = cs.CurrentInventory;
                            if (l.qty > cs.CurrentInventory)
                            {
                                l.qty = l.qty - cs.CurrentInventory;
                                List<PurchaseOrderDetailRecived> po = context.PurchaseOrderRecivedDetails.Where(x => x.ItemId == l.ItemId && x.Status != "Received" && x.CompanyId == compid && x.WarehouseId == Warehouse_id && x.ItemMultiMRPId == l.ItemMultiMRPId).ToList();
                                List<PurchaseOrderDetail> po1 = context.DPurchaseOrderDeatil.Where(x => x.ItemId == l.ItemId && x.Status == "ordered" && x.CompanyId == compid && x.WarehouseId == Warehouse_id && x.ItemMultiMRPId == l.ItemMultiMRPId).ToList();

                                //List<TransferWHOrderDetails> To1 = context.TransferWHOrderDetailsDB.Where(x => x.ItemId == l.ItemId && x.Status == "Pending" && x.CompanyId == compid && x.WarehouseId == Warehouse_id && x.ItemMultiMRPId == l.ItemMultiMRPId).ToList();
                                //List<TransferWHOrderDispatchedDetail> To = context.TransferWHOrderDispatchedDetailDB.Where(x => x.ItemId == l.ItemId && x.Status == "Pending" && x.CompanyId == compid && x.WarehouseId == Warehouse_id && x.ItemMultiMRPId == l.ItemMultiMRPId).ToList();

                                if ((po.Count != 0 && po1.Count != 0)/* || (To1.Count != 0 && To.Count != 0)*/)
                                {
                                    foreach (var p in po)
                                    {
                                        ///l.qty = l.qty - Convert.ToInt32((p.PurchaseQty - p.QtyRecived) * p.MOQ); old
                                        l.qty = l.qty - Convert.ToInt32((p.PurchaseQty * p.MOQ) - p.QtyRecived); //after MUlti Mrp
                                    }
                                    foreach (var p1 in po1)
                                    {
                                        l.qty = l.qty - Convert.ToInt32(p1.PurchaseQty * p1.MOQ);
                                    }
                                    //foreach (var t in To)
                                    //{
                                    // ///l.qty = l.qty - Convert.ToInt32((p.PurchaseQty - p.QtyRecived) * p.MOQ); old
                                    // l.qty = l.qty - Convert.ToInt32(t.TotalQuantity); //after MUlti Mrp
                                    //}
                                    //foreach (var t1 in To1)
                                    //{
                                    // l.qty = l.qty - Convert.ToInt32(t1.TotalQuantity);
                                    //}

                                    if (l.qty > 0)
                                    {
                                        cc.Add(l);
                                    }
                                }
                                else if ((po.Count != 0 && po1.Count == 0)/* || (To1.Count == 0 && To.Count == 0)*/)
                                {
                                    foreach (var p in po)
                                    {
                                        //l.qty = l.qty - Convert.ToInt32((p.PurchaseQty - p.QtyRecived) * p.MOQ);old mrp
                                        l.qty = l.qty - Convert.ToInt32((p.PurchaseQty * p.MOQ) - p.QtyRecived); //after MUlti Mrp
                                    }

                                    //foreach (var t in To)
                                    //{
                                    // ///l.qty = l.qty - Convert.ToInt32((p.PurchaseQty - p.QtyRecived) * p.MOQ); old
                                    // l.qty = l.qty - Convert.ToInt32(t.TotalQuantity); //after MUlti Mrp
                                    //}

                                    if (l.qty > 0)
                                    {
                                        cc.Add(l);
                                    }
                                }
                                else if ((po.Count == 0 && po1.Count != 0) /*|| (To1.Count != 0 && To.Count != 0)*/)
                                {
                                    foreach (var p in po1)
                                    {
                                        l.qty = l.qty - Convert.ToInt32(p.PurchaseQty * p.MOQ);

                                    }
                                    //foreach (var t1 in To1)
                                    //{
                                    // l.qty = l.qty - Convert.ToInt32(t1.TotalQuantity);
                                    //}
                                    if (l.qty > 0)
                                    {
                                        cc.Add(l);
                                    }
                                }
                                else
                                {
                                    cc.Add(l);
                                }
                            }
                        }
                    }
                    return cc.OrderByDescending(a => a.NetStock).ToList();
                }
            }
            catch (Exception ex)
            {
                logger.Error("Error in getting Company " + ex.Message);
                return null;
            }
        }


        [Route("GetWarehouseSearchItem")]
        [HttpGet]
        public dynamic GetWarehouseSearchItem(string name, int warehouseid)
        {
            using (AuthContext db = new AuthContext())
            {
                if (warehouseid > 0)
                {
                    var whdata = db.itemMasters.Where(x => x.WarehouseId == warehouseid && x.Deleted == false && x.IsDisContinued == false && x.itemname.ToLower().Contains(name.ToLower()))
                        .Select(x => new { x.Number, x.ItemMultiMRPId, itemnameWithMOQ = x.itemname }).Distinct().Take(50).ToList();

                    return whdata;
                }
                else
                {
                    return null;
                }

            }
        }

        [Route("GetWarehouseItemSupplier")]
        [HttpGet]
        public dynamic GetWarehouseItemSupplier(int warehouseid)
        {
            using (AuthContext db = new AuthContext())
            {
                if (warehouseid > 0)
                {
                    var whdata = db.itemMasters.Where(x => x.WarehouseId == warehouseid && x.Deleted == false && x.IsDisContinued == false && x.SupplierId > 0)
                        .Select(x => new { x.SupplierId, x.SupplierName }).Distinct().ToList();

                    return whdata;
                }
                else
                {
                    return null;
                }

            }
        }

        [Authorize]
        [Route("")]
        public IEnumerable<PurchaseOrderList> Getallorderdetails(string Cityid, string Warehouseid, DateTime? datefrom, DateTime? dateto)
        {
            logger.Info("start : ");
            IList<PurchaseOrderList> list = null;
            List<PurchaseOrderList> ass = new List<PurchaseOrderList>();
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

                logger.Info("User ID : {0} , Company Id : {1}", compid, userid);
                //int idd = Int32.Parse(id);
                using (var context = new AuthContext())
                {
                    //list = context.AllfilteredOrderDetails2(Cityid, Warehouse_id, datefrom, dateto, compid).ToList();
                }
                logger.Info("End  : ");
                return list;
            }
            catch (Exception ex)
            {
                logger.Error("Error in OrderDetails " + ex.Message);
                logger.Info("End  OrderDetails: ");
                return null;
            }
        }

        [ResponseType(typeof(PurchaseOrderList))]
        [Route("")]
        [AcceptVerbs("POST")]
        public List<PurchaseOrderList> addAll(string returntype, data d)
        {
            logger.Info("start add PurchaseOrderList: ");
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
                int whid = Convert.ToInt32(d.WarehouseId);
                int SID = Convert.ToInt32(d.Supplierid);
                List<PurchaseOrderList> po = new List<PurchaseOrderList>();
                int cityid = Convert.ToInt32(d.Cityid);

                po = d.datalist;
                using (var context = new AuthContext())
                {
                    if (d.Supplierid != null && d.Supplierid != "0" && d.WarehouseId != null)
                    {

                        int supplierid = Convert.ToInt32(d.Supplierid);
                        List<PurchaseOrderList> filterpo = po.Where(x => x.SupplierId == supplierid).Where(x => x.WarehouseId == whid && x.CompanyId == compid).ToList();
                        context.AddPurchaseOrder(filterpo, compid);
                    }
                    else if (d.WarehouseId != null)
                    {
                        context.AddPurchaseOrder(po.Where(x => x.WarehouseId == whid).ToList(), compid);
                    }
                    else if (d.Cityid != null && d.Supplierid != null)
                    {
                        context.AddPurchaseOrder(po.Where(x => x.Cityid == cityid).Where(x => x.SupplierId == SID).ToList(), compid);
                    }

                    else if (d.Supplierid == "0")
                    {
                        context.AddPurchaseOrder(po.ToList(), compid);
                    }
                    else if (d.Supplierid != null)
                    {
                        context.AddPurchaseOrder(po.Where(x => x.SupplierId == SID).ToList(), compid);
                    }
                }

                return po;
            }
            catch (Exception ex)
            {
                logger.Error("Error in addQuesAns " + ex.Message);
                logger.Info("End  addWarehouse: ");
                return null;
            }
        }

        [ResponseType(typeof(PurchaseList))]
        [Route("")]
        [AcceptVerbs("POST")]
        public List<PurchaseList> Add(List<PurchaseList> PurchaseOrderListdat)
        {
            logger.Info("start add PurchaseOrderList: ");
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
                //context.AddPurchaseOrder(PurchaseOrderListdat);
                return PurchaseOrderListdat;
            }
            catch (Exception ex)
            {
                logger.Error("Error in addQuesAns " + ex.Message);
                logger.Info("End  addWarehouse: ");
                return null;
            }
        }

        /// Add po from automation.   
        [ResponseType(typeof(TempPO))]
        [Route("")]
        [AcceptVerbs("POST")]
        public List<TempPO> Add(List<PurchaseList> temppo, string a)
        {

            var identity = User.Identity as ClaimsIdentity;
            int compid = 0, userid = 0;


            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))
                compid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);

            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
            List<TempPO> listtemp = new List<TempPO>();
            TempPO tempobj = new TempPO();
            List<PurchaseList> purcheslistobj = new List<PurchaseList>();
            using (var context = new AuthContext())
            {
                using (var dbContextTransaction = context.Database.BeginTransaction())
                {
                    try
                    {
                        People people = context.Peoples.Where(p => p.PeopleID == userid).SingleOrDefault();
                        var ItemIds = temppo.Select(x => x.ItemId).Distinct().ToList();
                        var ItemsList = new List<ItemMaster>();
                        ItemsList = context.itemMasters.Where(x => ItemIds.Contains(x.ItemId)).ToList();
                        var warehouseId = temppo.FirstOrDefault().WareHouseId;
                        var warehouse = context.Warehouses.Where(w => w.WarehouseId == warehouseId).SingleOrDefault();
                        var supplierId = temppo.Select(x => x.SupplierId).Distinct().ToList(); ;
                        var SuppliersList = context.Suppliers.Where(s => supplierId.Contains(s.SupplierId)).ToList();
                        var depoIds = temppo.Select(x => x.DepoId).Distinct().ToList(); ;
                        var DepoList = context.DepoMasters.Where(d => depoIds.Contains(d.DepoId)).ToList();
                        foreach (var x in temppo)
                        {
                            #region  function  for supplier name  by Sudhir 27-05-2019
                            if (DepoList.Any(y => y.DepoId == x.DepoId))
                            {
                                var deponame = DepoList.Where(y => y.DepoId == x.DepoId).SingleOrDefault();
                                x.DepoName = deponame.DepoName;
                            }
                            if (SuppliersList.Any(y => y.SupplierId == x.SupplierId))
                            {
                                var Suppliername = SuppliersList.Where(y => y.SupplierId == x.SupplierId).SingleOrDefault();
                                x.Supplier = Suppliername.Name;
                            }
                            #endregion
                            string s = x.orderIDs;
                            int Warehouseid = x.WareHouseId;
                            string[] values = s.Split(',');
                            for (int i = 0; i < values.Length; i++)
                            {
                                values[i] = values[i].Trim();
                            }
                            for (int i = 0; i < values.Length; i++)
                            {
                                if (values[i] != "")
                                {
                                    int OrderId = int.Parse(values[i]);
                                    var ordList = context.DbOrderDetails.Where(r => r.OrderId == OrderId && r.ItemMultiMRPId == x.ItemMultiMRPId && r.WarehouseId == Warehouseid).ToList();
                                    foreach (var ord in ordList)
                                    {
                                        ord.Status = "Process";
                                        context.Entry(ord).State = EntityState.Modified;
                                        context.Commit();
                                    }
                                }
                            }
                            TempPO po = listtemp.Where(y => y.SupplierId == x.SupplierId).SingleOrDefault();
                            if (po == null)
                            {
                                po = new TempPO();
                                po.SupplierId = x.SupplierId;
                                po.SupplierName = x.Supplier;
                                po.CompanyId = compid;
                                po.EtotalAmt = 0;
                                po.userid = userid;
                                po.WareHouseId = Warehouseid;
                                po.DepoId = x.DepoId;
                                po.DepoName = x.DepoName;
                                po.Purchases.Add(x);
                                listtemp.Add(po);
                            }
                            else
                            {
                                po.Purchases.Add(x);
                            }
                        }

                        if (listtemp != null && listtemp.Any())
                        {
                            foreach (var x in listtemp)
                            {
                                double ETtlamt = 0;
                                People buyer = new People();
                                Supplier supplier1;
                                PurchaseOrderMaster pmm = new PurchaseOrderMaster();
                                try
                                {
                                    supplier1 = SuppliersList.Where(s => s.SupplierId == x.SupplierId).SingleOrDefault();
                                    buyer = context.Peoples.Where(b => b.PeopleID == supplier1.PeopleID).SingleOrDefault();
                                }
                                catch (Exception ex)
                                {

                                }
                                pmm.SupplierId = x.SupplierId.GetValueOrDefault();
                                pmm.SupplierName = x.SupplierName;
                                pmm.WarehouseId = x.WareHouseId;
                                pmm.WarehouseName = warehouse.WarehouseName;
                                pmm.WarehouseCity = warehouse.CityName;
                                pmm.ETotalAmount = ETtlamt;
                                pmm.CompanyId = x.CompanyId;
                                pmm.PoType = "Automated";
                                pmm.CreationDate = indianTime;
                                pmm.Status = "pending";
                                pmm.DepoId = x.DepoId;
                                pmm.DepoName = x.DepoName;
                                pmm.IsLock = true;
                                try
                                {
                                    pmm.BuyerId = buyer.PeopleID;
                                    pmm.BuyerName = buyer.DisplayName;
                                }
                                catch (Exception sdf) { }
                                pmm.Acitve = true;
                                pmm.CreatedBy = people.PeopleFirstName + " " + people.PeopleLastName;
                                pmm.PurchaseOrderDetail = new List<PurchaseOrderDetail>();
                                context.DPurchaseOrderMaster.Add(pmm);
                                context.Commit();
                                foreach (var aitem in x.Purchases)
                                {
                                    var item = ItemsList.Where(z => z.WarehouseId == aitem.WareHouseId && z.ItemId == aitem.ItemId).SingleOrDefault();
                                    PurchaseOrderDetail pd = new PurchaseOrderDetail();
                                    pd.PurchaseOrderId = pmm.PurchaseOrderId;
                                    pd.ItemId = aitem.ItemId.GetValueOrDefault();
                                    pd.ItemMultiMRPId = aitem.ItemMultiMRPId;//for multimrp
                                    pd.ItemName = aitem.ItemName;
                                    pd.TotalQuantity = int.Parse(aitem.qty.ToString());
                                    pd.CreationDate = indianTime;
                                    pd.Status = "ordered";
                                    pmm.WarehouseName = aitem.WareHouseName;
                                    pd.MOQ = item.PurchaseMinOrderQty;
                                    pd.itemBaseName = item.itemBaseName;
                                    pd.ItemNumber = item.Number;

                                    if (item.POPurchasePrice == null || item.POPurchasePrice == 0)
                                    {
                                        pd.Price = Math.Round(item.NetPurchasePrice * (1 + (item.TotalTaxPercentage / 100)), 3); //With tax net purchase price add by raj   
                                    }
                                    else
                                    {
                                        pd.Price = item.POPurchasePrice ?? 0;
                                    }
                                    pd.WarehouseId = aitem.WareHouseId;
                                    pd.CompanyId = aitem.CompanyId;
                                    pd.WarehouseName = aitem.WareHouseName;
                                    pd.SupplierId = aitem.SupplierId.GetValueOrDefault();
                                    pd.SupplierName = aitem.Supplier;
                                    pd.TotalQuantity = Convert.ToInt32(aitem.qty);
                                    pd.PurchaseName = aitem.name;
                                    pd.PurchaseSku = aitem.PurchaseSku;
                                    pd.ConversionFactor = Convert.ToInt32(aitem.conversionfactor);
                                    pd.DepoId = aitem.DepoId;
                                    pd.DepoName = aitem.DepoName;
                                    pd.PurchaseQty = aitem.finalqty;
                                    pd.MRP = aitem.Price;
                                    pmm.ETotalAmount += (Math.Round(item.NetPurchasePrice * (1 + (item.TotalTaxPercentage / 100)), 3) * pd.TotalQuantity);
                                    context.DPurchaseOrderDeatil.Add(pd);
                                    context.Commit();

                                }
                                #region level allocation
                                PoApproval get_approvalz = context.PoApprovalDB.Where(p => p.AmountlmtMin <= ETtlamt && p.AmountlmtMax >= ETtlamt && p.Warehouseid == x.WareHouseId).FirstOrDefault();
                                if (get_approvalz != null)
                                {
                                    if (get_approvalz.Level == "Level1")  /// Self Approved
                                    {
                                        PurchaseOrderMaster pm = context.DPurchaseOrderMaster.Where(p => p.PurchaseOrderId == pmm.PurchaseOrderId).SingleOrDefault();
                                        pm.Status = "Self Approved";
                                        pm.progress = "50";
                                        pm.Level = get_approvalz.Level;
                                        pm.Approval1 = 0;
                                        pm.Approval2 = 0;
                                        pm.Approval3 = 0;
                                        pm.Approval4 = 0;
                                        pm.Approval5 = 0;
                                        pm.ApprovalName1 = pm.CreatedBy;
                                        pm.Rewiever1 = 0;
                                        pm.Rewiever2 = 0;
                                        pm.Rewiever3 = 0;
                                        pm.Rewiever4 = 0;
                                        pm.Rewiever5 = 0;
                                        pm.ApprovedBy = pm.CreatedBy;
                                        context.Entry(pm).State = EntityState.Modified;
                                        context.Commit();
                                    }
                                    else /// Send for approval
                                    {
                                        /// Send Approved 
                                        PurchaseOrderMaster pm = context.DPurchaseOrderMaster.Where(p => p.PurchaseOrderId == pmm.PurchaseOrderId).SingleOrDefault();
                                        pm.Status = "Send for Approval";
                                        pm.Approval1 = get_approvalz.Approval1;
                                        pm.Approval2 = get_approvalz.Approval2;
                                        pm.Approval3 = get_approvalz.Approval3;
                                        pm.Approval4 = get_approvalz.Approval4;
                                        pm.Approval5 = get_approvalz.Approval5;
                                        pm.Rewiever1 = get_approvalz.Reviewer1;
                                        pm.Rewiever2 = get_approvalz.Reviewer2;
                                        pm.Rewiever3 = get_approvalz.Reviewer3;
                                        pm.Rewiever4 = get_approvalz.Reviewer4;
                                        pm.Rewiever5 = get_approvalz.Reviewer5;
                                        pm.ApprovalName1 = get_approvalz.ApprovalName1;
                                        pm.ApprovalName2 = get_approvalz.ApprovalName2;
                                        pm.ApprovalName3 = get_approvalz.ApprovalName3;
                                        pm.ApprovalName4 = get_approvalz.ApprovalName4;
                                        pm.ApprovalName5 = get_approvalz.ApprovalName5;
                                        pm.RewieverName1 = get_approvalz.ReviewerName1;
                                        pm.RewieverName2 = get_approvalz.ReviewerName2;
                                        pm.RewieverName3 = get_approvalz.ReviewerName3;
                                        pm.RewieverName4 = get_approvalz.ReviewerName4;
                                        pm.RewieverName5 = get_approvalz.ReviewerName5;
                                        pm.progress = "20";
                                        pm.Level = get_approvalz.Level;
                                        context.Entry(pm).State = EntityState.Modified;
                                        context.Commit();
                                        //Sms s = new Sms();
                                        //string msg = "PO id: " + pm.PurchaseOrderId + " are waiting for your approval.";
                                        //string Mob = context.Peoples.Where(q => q.PeopleID == get_approvalz.Approval1).Select(q => q.Mobile).SingleOrDefault();
                                        //s.sendOtp(Mob, msg);
                                    }
                                }
                                #endregion
                            }
                            dbContextTransaction.Commit();
                        }
                        else { dbContextTransaction.Rollback(); }

                    }
                    catch (Exception dd) { dbContextTransaction.Rollback(); }

                    //PurchaseOrderMaster pom = context.addPurchaseOrderMaster(listtemp);

                }
            }
            return listtemp;

        }

        /// <summary>
        /// Add Po
        /// </summary>
        /// <param name="temppo"></param>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        [Route("")]
        [AcceptVerbs("POST")]
        public PurchaseList AddItem(PurchaseList temppo, string a, int b)
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
                using (AuthContext db = new AuthContext())
                {

                    List<TempPO> listtemp = new List<TempPO>();
                    if (temppo != null)
                    {
                        var purId = 0;

                        PurchaseOrderMaster pom = db.DPurchaseOrderMaster.Where(c => c.PurchaseOrderId == temppo.PurchaseOrderId && c.CompanyId == compid).SingleOrDefault();
                        var supplier1 = db.Suppliers.Where(s => s.SupplierId == temppo.SupplierId).SingleOrDefault();
                        People buyer = db.Peoples.Where(q => q.PeopleID == supplier1.PeopleID).SingleOrDefault();
                        var warehouse = db.Warehouses.Where(q => q.WarehouseId == pom.WarehouseId).SingleOrDefault();
                        if (pom == null)
                        {
                            PurchaseOrderMaster pm = new PurchaseOrderMaster();
                            pm.SupplierId = temppo.SupplierId.GetValueOrDefault();
                            pm.SupplierName = temppo.Supplier;
                            pm.WarehouseId = pom.WarehouseId;
                            pm.CompanyId = compid;
                            pm.WarehouseName = temppo.WareHouseName;
                            pm.WarehouseCity = warehouse.CityName;
                            pm.BuyerId = buyer.PeopleID;
                            pm.BuyerName = buyer.DisplayName;
                            pm.Status = "pending";
                            pm.Acitve = true;
                            pm.CreationDate = indianTime;
                            db.DPurchaseOrderMaster.Add(pm);
                            int id = db.Commit();
                            purId = pm.PurchaseOrderId;
                        }
                        else
                        {
                            purId = pom.PurchaseOrderId;
                        }

                        var item = db.itemMasters.Where(z => z.ItemId == temppo.ItemId && z.CompanyId == compid && z.WarehouseId == pom.WarehouseId).FirstOrDefault();
                        PurchaseOrderDetail pd = new PurchaseOrderDetail();
                        pd.PurchaseOrderId = purId;
                        pd.ItemId = item.ItemId;
                        pd.ItemMultiMRPId = item.ItemMultiMRPId;//for multimrp
                        pd.ItemName = item.itemname;
                        pd.itemBaseName = item.itemBaseName;
                        pd.ItemNumber = item.Number;
                        pd.MRP = item.price;
                        pd.TotalQuantity = int.Parse(temppo.qty.ToString());
                        pd.CreationDate = indianTime;
                        pd.Status = "ordered";
                        pd.MOQ = item.PurchaseMinOrderQty;
                        pd.Price = Convert.ToDouble(item.PurchasePrice);
                        pd.WarehouseId = pom.WarehouseId;
                        pd.CompanyId = compid;
                        pd.WarehouseName = temppo.WareHouseName;
                        pd.SupplierId = temppo.SupplierId.GetValueOrDefault();
                        pd.SupplierName = temppo.Supplier;
                        //pd.TotalQuantity = Convert.ToInt32(temppo.qty);
                        pd.PurchaseName = temppo.name;
                        pd.PurchaseSku = temppo.PurchaseSku;
                        pd.ConversionFactor = Convert.ToInt32(temppo.conversionfactor);
                        pd.PurchaseQty = temppo.finalqty;
                        db.DPurchaseOrderDeatil.Add(pd);
                        int idd = db.Commit();
                        #region get Estimate amount and update level
                        /// Get Estimate amount of item  and Update level
                        double ETtlamt = 0;
                        List<PurchaseOrderDetail> pdd = db.DPurchaseOrderDeatil.Where(q => q.PurchaseOrderId == temppo.PurchaseOrderId).ToList();

                        /// Get Estimate amount                  

                        foreach (var data in pdd)
                        {
                            try
                            {
                                var item1 = db.itemMasters.Where(z => z.ItemId == data.ItemId && z.CompanyId == compid && z.WarehouseId == pom.WarehouseId).FirstOrDefault();
                                int qty = data.TotalQuantity;
                                ETtlamt += Convert.ToDouble(item1.PurchasePrice) * qty;
                            }
                            catch (Exception ex) { }
                        }
                        pom.ETotalAmount = ETtlamt;
                        db.Entry(pom).State = EntityState.Modified;
                        db.Commit();
                        ///  End ///
                        #endregion


                        //  string smstempmsg = " is waiting for your approval. ShopKirana";
                        string smstempmsg = ""; //"ShopKirana PR id: {#var#} is waiting for your approval. ShopKirana";
                        var dltSMS = SMSTemplateHelper.getTemplateText((int)AppEnum.Others, "PR_Waitng_Approval");
                        smstempmsg = dltSMS == null ? "" : dltSMS.Template;                       
                        smstempmsg = smstempmsg.Replace("{#var#}", pom.PurchaseOrderId.ToString());


                        #region Level allocation 
                        if (pom.Status != "Draft")
                        {
                            #region level allocation
                            PoApproval get_approvalz = db.PoApprovalDB.Where(q => q.AmountlmtMin <= ETtlamt && q.AmountlmtMax >= ETtlamt && q.Warehouseid == pom.WarehouseId).FirstOrDefault();
                            if (get_approvalz != null)
                            {
                                if (get_approvalz.Level == "Level1")  /// Self Approved
                                {
                                    PurchaseOrderMaster pm = db.DPurchaseOrderMaster.Where(q => q.PurchaseOrderId == temppo.PurchaseOrderId).SingleOrDefault();
                                    pm.Status = "Self Approved";
                                    pm.progress = "50";
                                    pm.Level = "Level1";
                                    pm.Approval1 = 0;
                                    pm.Approval2 = 0;
                                    pm.Approval3 = 0;
                                    pm.Approval4 = 0;
                                    pm.Approval5 = 0;
                                    pm.ApprovalName1 = pm.CreatedBy;
                                    pm.Rewiever1 = 0;
                                    pm.Rewiever2 = 0;
                                    pm.Rewiever3 = 0;
                                    pm.Rewiever4 = 0;
                                    pm.Rewiever5 = 0;
                                    pm.ApprovedBy = pm.CreatedBy;
                                    db.Entry(pm).State = EntityState.Modified;
                                    db.Commit();
                                }
                                else /// Send for approval
                                {
                                    /// Send Approved 
                                    PurchaseOrderMaster pm = db.DPurchaseOrderMaster.Where(q => q.PurchaseOrderId == temppo.PurchaseOrderId).SingleOrDefault();
                                    pm.Status = "Send for Approval";
                                    pm.Approval1 = get_approvalz.Approval1;
                                    pm.Approval2 = get_approvalz.Approval2;
                                    pm.Approval3 = get_approvalz.Approval3;
                                    pm.Approval4 = get_approvalz.Approval4;
                                    pm.Approval5 = get_approvalz.Approval5;
                                    pm.Rewiever1 = get_approvalz.Reviewer1;
                                    pm.Rewiever2 = get_approvalz.Reviewer2;
                                    pm.Rewiever3 = get_approvalz.Reviewer3;
                                    pm.Rewiever4 = get_approvalz.Reviewer4;
                                    pm.Rewiever5 = get_approvalz.Reviewer5;
                                    pm.ApprovalName1 = get_approvalz.ApprovalName1;
                                    pm.ApprovalName2 = get_approvalz.ApprovalName2;
                                    pm.ApprovalName3 = get_approvalz.ApprovalName3;
                                    pm.ApprovalName4 = get_approvalz.ApprovalName4;
                                    pm.ApprovalName5 = get_approvalz.ApprovalName5;
                                    pm.RewieverName1 = get_approvalz.ReviewerName1;
                                    pm.RewieverName2 = get_approvalz.ReviewerName2;
                                    pm.RewieverName3 = get_approvalz.ReviewerName3;
                                    pm.RewieverName4 = get_approvalz.ReviewerName4;
                                    pm.RewieverName5 = get_approvalz.ReviewerName5;
                                    pm.progress = "20";
                                    pm.Level = get_approvalz.Level;
                                    db.Entry(pm).State = EntityState.Modified;
                                    db.Commit();
                                    //Sms s = new Sms(); string msg = "ShopKirana PR id: " + pm.PurchaseOrderId + smstempmsg;
                                    Sms s = new Sms(); string msg =  smstempmsg;
                                    string Mob = db.Peoples.Where(q => q.PeopleID == get_approvalz.Approval1).Select(q => q.Mobile).SingleOrDefault();
                                    if(dltSMS!=null)
                                    s.sendOtp(Mob, msg, dltSMS.DLTId);
                                }
                            }
                            #endregion
                        }
                        #endregion

                        #region Create History

                        PoEditHistory Hisdata = new PoEditHistory()
                        {
                            PurchaseOrderId = pom.PurchaseOrderId,
                            CreateDate = indianTime,
                            UserId = userid,
                            ModificationType = "Add Item"
                        };
                        var result = GenratHistory(Hisdata);
                        #endregion
                    }
                    else
                    {
                        return null;
                    }

                    return temppo;
                }
            }
            catch (Exception ex)
            {
                logger.Error("Error in addQuesAns " + ex.Message);
                logger.Info("End  addWarehouse: ");
                return null;
            }
        }

        /// <summary>
        /// Edit item
        /// </summary>
        /// <param name="temppo"></param>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        [Route("edit")]
        [AcceptVerbs("PUT")]
        public PurchaseList EditItem(PurchaseList temppo, string a, int b)
        {
            using (AuthContext db = new AuthContext())
            {
                try
                {
                    var identity = User.Identity as ClaimsIdentity;
                    int compid = 0, userid = 0;
                    int Warehouse_id = 0;
                    PurchaseOrderMaster pid = db.DPurchaseOrderMaster.Where(q => q.PurchaseOrderId == temppo.PurchaseOrderId).SingleOrDefault();
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
                    Warehouse_id = temppo.WareHouseId;
                    if (Warehouse_id > 0)
                    {
                        List<TempPO> listtemp = new List<TempPO>();
                        if (temppo != null)
                        {
                            PurchaseOrderDetail pd = db.DPurchaseOrderDeatil.Where(q => q.PurchaseOrderDetailId == temppo.OrderDetailsId).SingleOrDefault();
                            pd.TotalQuantity = int.Parse(temppo.qty.ToString());
                            pd.PurchaseQty = int.Parse(temppo.qty.ToString());
                            db.Entry(pd).State = EntityState.Modified;
                            db.Commit();
                        }
                        else
                        {
                            return null;
                        }

                        #region get Estimate amount and update level
                        /// Get Estimate amount of item  and Update level
                        double ETtlamt = 0;
                        List<PurchaseOrderDetail> pdd = db.DPurchaseOrderDeatil.Where(q => q.PurchaseOrderId == temppo.PurchaseOrderId).ToList();

                        /// Get Estimate amount                  

                        foreach (var data in pdd)
                        {
                            try
                            {
                                var item1 = db.itemMasters.Where(z => z.ItemId == data.ItemId && z.CompanyId == compid && z.WarehouseId == Warehouse_id).FirstOrDefault();
                                int qty = data.TotalQuantity;
                                ETtlamt += Convert.ToDouble(item1.PurchasePrice) * qty;
                            }
                            catch (Exception ex) { }
                        }
                        pid.ETotalAmount = ETtlamt;
                        db.Entry(pid).State = EntityState.Modified;
                        db.Commit();
                        ///  End ///
                        #endregion
                        // string smstempmsg = " is waiting for your approval. ShopKirana";
                        string smstempmsg = ""; //"ShopKirana PR id: {#var#} is waiting for your approval. ShopKirana";
                        var dltSMS = SMSTemplateHelper.getTemplateText((int)AppEnum.Others, "PR_Waitng_Approval");
                        smstempmsg = dltSMS == null ? "" : dltSMS.Template;
                        
                        smstempmsg = smstempmsg.Replace("{#var#}", temppo.PurchaseOrderId.ToString());

                        if (pid.Status != "Draft")
                        {
                            #region level allocation
                            PoApproval get_approvalz = db.PoApprovalDB.Where(q => q.AmountlmtMin <= ETtlamt && q.AmountlmtMax >= ETtlamt && q.Warehouseid == Warehouse_id).FirstOrDefault();
                            if (get_approvalz != null)
                            {
                                if (get_approvalz.Level == "Level1")  /// Self Approved
                                {
                                    PurchaseOrderMaster pm = db.DPurchaseOrderMaster.Where(q => q.PurchaseOrderId == temppo.PurchaseOrderId).SingleOrDefault();
                                    pm.Status = "Self Approved";
                                    pm.progress = "50";
                                    pm.Level = "Level1";
                                    pm.Approval1 = 0;
                                    pm.Approval2 = 0;
                                    pm.Approval3 = 0;
                                    pm.Approval4 = 0;
                                    pm.Approval5 = 0;
                                    pm.ApprovalName1 = pm.CreatedBy;
                                    pm.Rewiever1 = 0;
                                    pm.Rewiever2 = 0;
                                    pm.Rewiever3 = 0;
                                    pm.Rewiever4 = 0;
                                    pm.Rewiever5 = 0;
                                    pm.ApprovedBy = pm.CreatedBy;
                                    db.Entry(pm).State = EntityState.Modified;
                                    db.Commit();
                                }
                                else /// Send for approval
                                {
                                    /// Send Approved 
                                    PurchaseOrderMaster pm = db.DPurchaseOrderMaster.Where(q => q.PurchaseOrderId == temppo.PurchaseOrderId).SingleOrDefault();
                                    pm.Status = "Send for Approval";
                                    pm.Approval1 = get_approvalz.Approval1;
                                    pm.Approval2 = get_approvalz.Approval2;
                                    pm.Approval3 = get_approvalz.Approval3;
                                    pm.Approval4 = get_approvalz.Approval4;
                                    pm.Approval5 = get_approvalz.Approval5;
                                    pm.Rewiever1 = get_approvalz.Reviewer1;
                                    pm.Rewiever2 = get_approvalz.Reviewer2;
                                    pm.Rewiever3 = get_approvalz.Reviewer3;
                                    pm.Rewiever4 = get_approvalz.Reviewer4;
                                    pm.Rewiever5 = get_approvalz.Reviewer5;
                                    pm.ApprovalName1 = get_approvalz.ApprovalName1;
                                    pm.ApprovalName2 = get_approvalz.ApprovalName2;
                                    pm.ApprovalName3 = get_approvalz.ApprovalName3;
                                    pm.ApprovalName4 = get_approvalz.ApprovalName4;
                                    pm.ApprovalName5 = get_approvalz.ApprovalName5;
                                    pm.RewieverName1 = get_approvalz.ReviewerName1;
                                    pm.RewieverName2 = get_approvalz.ReviewerName2;
                                    pm.RewieverName3 = get_approvalz.ReviewerName3;
                                    pm.RewieverName4 = get_approvalz.ReviewerName4;
                                    pm.RewieverName5 = get_approvalz.ReviewerName5;
                                    pm.progress = "20";
                                    pm.Level = get_approvalz.Level;
                                    db.Entry(pm).State = EntityState.Modified;
                                    db.Commit();
                                    Sms s = new Sms();
                                    //string msg = "ShopKirana PR id: " + pm.PurchaseOrderId + smstempmsg;
                                    string msg =  smstempmsg;

                                    string Mob = db.Peoples.Where(q => q.PeopleID == get_approvalz.Approval1).Select(q => q.Mobile).SingleOrDefault();
                                    if (dltSMS != null)
                                        s.sendOtp(Mob, msg, dltSMS.DLTId);
                                }
                            }
                            #endregion
                        }

                        #region Create History

                        PoEditHistory Hisdata = new PoEditHistory()
                        {
                            PurchaseOrderId = pid.PurchaseOrderId,
                            CreateDate = indianTime,
                            UserId = userid,
                            ModificationType = "Update Item"
                        };
                        var result = GenratHistory(Hisdata);
                        #endregion
                    }
                    return temppo;
                }
                catch (Exception ex)
                {
                    logger.Error("Error in addQuesAns " + ex.Message);
                    logger.Info("End  addWarehouse: ");
                    return null;
                }
            }
        }
        /// <summary>
        /// Remove item
        /// </summary>
        /// <param name="temppo"></param>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        [Route("remove")]
        [AcceptVerbs("PUT")]
        public PurchaseList RemoveItem(PurchaseList temppo, string a, int b)
        {
            using (AuthContext db = new AuthContext())
            {
                try
                {
                    var identity = User.Identity as ClaimsIdentity;
                    int compid = 0, userid = 0;
                    int Warehouse_id = 0;
                    PurchaseOrderMaster pid = db.DPurchaseOrderMaster.Where(q => q.PurchaseOrderId == temppo.PurchaseOrderId).SingleOrDefault();
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
                    if (pid.WarehouseId > 0)
                    {
                        List<TempPO> listtemp = new List<TempPO>();
                        if (temppo != null)
                        {
                            PurchaseOrderDetail pd = db.DPurchaseOrderDeatil.Where(q => q.PurchaseOrderDetailId == temppo.OrderDetailsId).SingleOrDefault();
                            db.Entry(pd).State = EntityState.Deleted;
                            db.Commit();
                        }
                        else
                        {
                            return null;
                        }
                        #region get Estimate amount and update level
                        /// Get Estimate amount of item  and Update level
                        double ETtlamt = 0;
                        List<PurchaseOrderDetail> pdd = db.DPurchaseOrderDeatil.Where(q => q.PurchaseOrderId == temppo.PurchaseOrderId).ToList();

                        /// Get Estimate amount                  

                        foreach (var data in pdd)
                        {
                            try
                            {
                                var item1 = db.itemMasters.Where(z => z.ItemId == data.ItemId && z.CompanyId == compid && z.WarehouseId == pid.WarehouseId).FirstOrDefault();
                                int qty = data.TotalQuantity;
                                ETtlamt += Convert.ToDouble(item1.PurchasePrice) * qty;
                            }
                            catch (Exception ex) { }
                        }
                        pid.ETotalAmount = ETtlamt;
                        db.Entry(pid).State = EntityState.Modified;
                        db.Commit();
                        ///  End ///
                        #endregion
                        // string smstempmsg = " is waiting for your approval. ShopKirana";
                        string smstempmsg = ""; //"ShopKirana PR id: {#var#} is waiting for your approval. ShopKirana";
                        var dltSMS = SMSTemplateHelper.getTemplateText((int)AppEnum.Others, "PR_Waitng_Approval");
                        smstempmsg = dltSMS == null ? "" : dltSMS.Template;
                       
                        smstempmsg = smstempmsg.Replace("{#var#}", temppo.PurchaseOrderId.ToString());

                        #region Level provide 

                        if (pid.Status != "Draft")
                        {
                            if (ETtlamt > 0)
                            {
                                #region level allocation
                                PoApproval get_approvalz = db.PoApprovalDB.Where(q => q.AmountlmtMin <= ETtlamt && q.AmountlmtMax >= ETtlamt && q.Warehouseid == pid.WarehouseId).FirstOrDefault();
                                if (get_approvalz != null)
                                {
                                    if (get_approvalz.Level == "Level1")  /// Self Approved
                                    {
                                        PurchaseOrderMaster pm = db.DPurchaseOrderMaster.Where(q => q.PurchaseOrderId == temppo.PurchaseOrderId).SingleOrDefault();
                                        pm.Status = "Self Approved";
                                        pm.progress = "50";
                                        pm.Level = "Level1";
                                        pm.Approval1 = 0;
                                        pm.Approval2 = 0;
                                        pm.Approval3 = 0;
                                        pm.Approval4 = 0;
                                        pm.Approval5 = 0;
                                        pm.ApprovalName1 = pm.CreatedBy;
                                        pm.Rewiever1 = 0;
                                        pm.Rewiever2 = 0;
                                        pm.Rewiever3 = 0;
                                        pm.Rewiever4 = 0;
                                        pm.Rewiever5 = 0;
                                        pm.ApprovedBy = pm.CreatedBy;
                                        db.Entry(pm).State = EntityState.Modified;
                                        db.Commit();
                                    }
                                    else /// Send for approval
                                    {
                                        /// Send Approved 
                                        PurchaseOrderMaster pm = db.DPurchaseOrderMaster.Where(q => q.PurchaseOrderId == temppo.PurchaseOrderId).SingleOrDefault();
                                        pm.Status = "Send for Approval";
                                        pm.Approval1 = get_approvalz.Approval1;
                                        pm.Approval2 = get_approvalz.Approval2;
                                        pm.Approval3 = get_approvalz.Approval3;
                                        pm.Approval4 = get_approvalz.Approval4;
                                        pm.Approval5 = get_approvalz.Approval5;
                                        pm.Rewiever1 = get_approvalz.Reviewer1;
                                        pm.Rewiever2 = get_approvalz.Reviewer2;
                                        pm.Rewiever3 = get_approvalz.Reviewer3;
                                        pm.Rewiever4 = get_approvalz.Reviewer4;
                                        pm.Rewiever5 = get_approvalz.Reviewer5;
                                        pm.ApprovalName1 = get_approvalz.ApprovalName1;
                                        pm.ApprovalName2 = get_approvalz.ApprovalName2;
                                        pm.ApprovalName3 = get_approvalz.ApprovalName3;
                                        pm.ApprovalName4 = get_approvalz.ApprovalName4;
                                        pm.ApprovalName5 = get_approvalz.ApprovalName5;
                                        pm.RewieverName1 = get_approvalz.ReviewerName1;
                                        pm.RewieverName2 = get_approvalz.ReviewerName2;
                                        pm.RewieverName3 = get_approvalz.ReviewerName3;
                                        pm.RewieverName4 = get_approvalz.ReviewerName4;
                                        pm.RewieverName5 = get_approvalz.ReviewerName5;
                                        pm.progress = "20";
                                        pm.Level = get_approvalz.Level;
                                        db.Entry(pm).State = EntityState.Modified;
                                        db.Commit();
                                        //Sms s = new Sms(); string msg = "ShopKirana PR id: " + pm.PurchaseOrderId + smstempmsg;
                                        Sms s = new Sms(); string msg = smstempmsg;
                                        string Mob = db.Peoples.Where(q => q.PeopleID == get_approvalz.Approval1).Select(q => q.Mobile).SingleOrDefault();
                                        if(dltSMS!=null)
                                        s.sendOtp(Mob, msg, dltSMS.DLTId);
                                    }
                                }
                                #endregion
                            }
                            else
                            {
                                PurchaseOrderMaster pms = db.DPurchaseOrderMaster.Where(q => q.PurchaseOrderId == temppo.PurchaseOrderId).SingleOrDefault();
                                pms.Status = "Rejected";
                                pms.progress = "0";
                                pms.Level = "";
                                pms.ETotalAmount = ETtlamt;
                                pms.Commentsystem = "Rejected by system due to no item.";
                                db.Entry(pms).State = EntityState.Modified;
                                db.Commit();
                            }
                        }
                        #endregion

                        #region Create History

                        PoEditHistory Hisdata = new PoEditHistory()
                        {
                            PurchaseOrderId = pid.PurchaseOrderId,
                            CreateDate = indianTime,
                            UserId = userid,
                            ModificationType = "Delete Item"
                        };
                        var result = GenratHistory(Hisdata);
                        #endregion
                    }
                    return temppo;
                }
                catch (Exception ex)
                {
                    logger.Error("Error in addQuesAns " + ex.Message);
                    logger.Info("End  addWarehouse: ");
                    return null;
                }
            }
        }

        //[Route("addPo")]
        //[AcceptVerbs("POST")]
        //public HttpResponseMessage Add(int ItemId, int qty, int SupplierId)
        //{

        //    try
        //    {
        //        var identity = User.Identity as ClaimsIdentity;
        //        int compid = 0, userid = 0;
        //        int Warehouse_id = 0;

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
        //            if (claim.Type == "Warehouseid")
        //            {
        //                Warehouse_id = int.Parse(claim.Value);
        //            }
        //        }

        //        if (Warehouse_id > 0) {

        //            PurchaseOrderMaster pm = new PurchaseOrderMaster();
        //            var item = db.itemMasters.Where(z => z.ItemId == ItemId && z.CompanyId == compid && z.WarehouseId == Warehouse_id).FirstOrDefault();
        //            var supplier = db.Suppliers.Where(s => s.SupplierId == SupplierId && s.CompanyId == compid && s.WarehouseId==Warehouse_id).SingleOrDefault();

        //            pm.SupplierId = supplier.SupplierId;
        //            pm.SupplierName = supplier.Name;
        //            pm.CreationDate = indianTime;
        //            pm.WarehouseId = item.WarehouseId;
        //            pm.CompanyId = compid;
        //            pm.WarehouseName = item.WarehouseName;
        //            pm.Status = "pending";
        //            pm.Acitve = true;
        //            db.DPurchaseOrderMaster.Add(pm);
        //            int id = db.SaveChanges();

        //            PurchaseOrderDetail pd = new PurchaseOrderDetail();
        //            pd.PurchaseOrderId = pm.PurchaseOrderId;
        //            pd.ItemId = item.ItemId;
        //            pd.SellingSku = item.SellingSku;
        //            pd.ItemName = item.itemname;
        //            pd.PurchaseQty = qty;
        //            pd.CreationDate = indianTime;
        //            pd.Status = "ordered";
        //            pd.MOQ = item.PurchaseMinOrderQty;
        //            pd.Price = Convert.ToDouble(item.PurchasePrice);
        //            pd.WarehouseId = item.WarehouseId;
        //            pd.CompanyId = item.CompanyId;
        //            pd.WarehouseName = item.WarehouseName;
        //            pd.SupplierId = supplier.SupplierId;
        //            pd.SupplierName = supplier.Name;

        //            pd.TotalQuantity = Convert.ToInt32(pd.PurchaseQty);
        //            pd.PurchaseName = item.PurchaseUnitName;
        //            pd.PurchaseSku = item.PurchaseSku;
        //            pd.ConversionFactor = Convert.ToInt32(item.PurchaseMinOrderQty);

        //            db.DPurchaseOrderDeatil.Add(pd);
        //            int idd = db.SaveChanges();


        //        } else {


        //        }

        //        return Request.CreateResponse(HttpStatusCode.OK, "Add Successfuly");
        //    }
        //    catch (Exception ex)
        //    {
        //        return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex.Message);
        //    }
        //}

        /// <summary>
        /// Create PO Manualy Send to Approval
        /// </summary>
        /// <param name="pdata"></param>
        /// <returns></returns>
        [Route("addPo")]
        [AcceptVerbs("POST")]
        public HttpResponseMessage Add(List<POdata> pdata)
        {
            try
            {
                var identity = User.Identity as ClaimsIdentity;
                int compid = 0, userid = 0, Warehouse_id = 0;
                double ETtlamt = 0; int pid = 0;

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))
                    compid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                    userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "Warehouseid"))
                    Warehouse_id = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "Warehouseid").Value);

                Warehouse_id = pdata[0].WarehouseId;
                using (AuthContext db = new AuthContext())
                using (var dbContextTransaction = db.Database.BeginTransaction())
                {
                    try
                    {
                        if (Warehouse_id > 0)
                        {
                            var _dataObj = pdata[0];

                            Supplier supplier1 = db.Suppliers.Where(s => s.SupplierId == _dataObj.SupplierId).SingleOrDefault();
                            People p = db.Peoples.Where(a => a.PeopleID == userid).FirstOrDefault();
                            People buyer = db.Peoples.Where(a => a.PeopleID == _dataObj.BuyerId).FirstOrDefault();
                            DepoMaster Depo = db.DepoMasters.Where(d => d.DepoId == _dataObj.DepoId).FirstOrDefault();
                            Warehouse warehouseD = db.Warehouses.Where(a => a.WarehouseId == Warehouse_id).FirstOrDefault();

                            #region get Estimate amount
                            /// Get Estimate amount
                            foreach (var data in pdata)
                            {
                                var item1 = db.itemMasters.Where(z => z.ItemId == data.ItemId && z.CompanyId == compid && z.WarehouseId == Warehouse_id).FirstOrDefault();
                                int qty = data.Noofset * data.PurchaseMinOrderQty;
                                ETtlamt += (Math.Round(item1.NetPurchasePrice * (1 + (item1.TotalTaxPercentage / 100)), 3) * qty);//With tax net purchase price add by raj                                  
                            }
                            #endregion

                            foreach (var data in pdata)
                            {
                                PurchaseOrderMaster pm = new PurchaseOrderMaster();
                                pm.SupplierId = supplier1.SupplierId;
                                pm.SupplierName = supplier1.Name;
                                pm.CreationDate = indianTime;
                                pm.WarehouseId = warehouseD.WarehouseId;
                                pm.CompanyId = compid;
                                pm.WarehouseName = warehouseD.WarehouseName;
                                pm.WarehouseCity = warehouseD.CityName;
                                pm.Status = "pending";
                                pm.progress = "0";
                                pm.IR_Progress = "0";
                                pm.PoType = "Manual";
                                pm.ETotalAmount = ETtlamt;
                                pm.Advance_Amt = data.Advance_Amt;
                                pm.DepoId = Depo != null ? Depo.DepoId : 0;
                                pm.DepoName = Depo != null ? Depo.DepoName : null;
                                pm.BuyerId = buyer.PeopleID;
                                pm.BuyerName = buyer.DisplayName;
                                pm.Acitve = true;
                                pm.CreatedBy = p.PeopleFirstName + " " + p.PeopleLastName;
                                pm.IsLock = true;
                                pm.IsCashPurchase = data.IsCashPurchase;
                                pm.CashPurchaseName = data.CashPurchaseName;
                                db.DPurchaseOrderMaster.Add(pm);
                                db.Commit();

                                pid = pm.PurchaseOrderId;
                                for (var i = 0; i < pdata.ToList().Count(); i++)
                                {
                                    int itemid = Convert.ToInt32(pdata[i].ItemId);
                                    var item = db.itemMasters.Where(z => z.ItemId == itemid && z.WarehouseId == Warehouse_id).FirstOrDefault();

                                    PurchaseOrderDetail pd = new PurchaseOrderDetail();
                                    var qty = pdata[i].Noofset * pdata[i].PurchaseMinOrderQty;
                                    pd.PurchaseOrderId = pm.PurchaseOrderId;
                                    pd.ItemId = item.ItemId;
                                    pd.ItemNumber = item.Number;
                                    pd.itemBaseName = item.itemBaseName;
                                    pd.ItemMultiMRPId = item.ItemMultiMRPId;
                                    pd.HSNCode = item.HSNCode;
                                    pd.MRP = item.price;
                                    pd.SellingSku = item.SellingSku;
                                    pd.ItemName = item.itemname;
                                    pd.PurchaseQty = qty;
                                    pd.CreationDate = indianTime;
                                    pd.Status = "ordered";
                                    pd.MOQ = item.PurchaseMinOrderQty;

                                    if (item.POPurchasePrice == null || item.POPurchasePrice == 0)
                                    {
                                        pd.Price = Math.Round(item.NetPurchasePrice * (1 + (item.TotalTaxPercentage / 100)), 3); //With tax net purchase price add by raj   
                                    }
                                    else
                                    {
                                        pd.Price = item.POPurchasePrice ?? 0;
                                    }
                                    pd.WarehouseId = item.WarehouseId;
                                    pd.CompanyId = item.CompanyId;
                                    pd.WarehouseName = item.WarehouseName;
                                    pd.SupplierId = supplier1.SupplierId;
                                    pd.SupplierName = supplier1.Name;
                                    pd.TotalQuantity = Convert.ToInt32(pd.PurchaseQty);
                                    pd.PurchaseName = item.PurchaseUnitName;
                                    pd.PurchaseSku = item.PurchaseSku;
                                    pd.DepoId = Depo != null ? Depo.DepoId : 0; ;
                                    pd.DepoName = Depo != null ? Depo.DepoName : null;
                                    pd.ConversionFactor = item.PurchaseMinOrderQty;
                                    db.DPurchaseOrderDeatil.Add(pd);
                                }
                                break;
                            }
                            string smstempmsg = " is waiting for your approval. ShopKirana";

                            #region level allocation
                            PoApproval get_approvalz = db.PoApprovalDB.Where(a => a.AmountlmtMin <= ETtlamt && a.AmountlmtMax >= ETtlamt && a.Warehouseid == Warehouse_id).FirstOrDefault();
                            if (get_approvalz != null)
                            {
                                if (get_approvalz.Level == "Level1")  /// Self Approved
                                {
                                    PurchaseOrderMaster pm = db.DPurchaseOrderMaster.Where(a => a.PurchaseOrderId == pid).SingleOrDefault();
                                    pm.Status = "Self Approved";
                                    pm.progress = "50";
                                    pm.Level = "Level1";
                                    pm.Approval1 = 0;
                                    pm.Approval2 = 0;
                                    pm.Approval3 = 0;
                                    pm.Approval4 = 0;
                                    pm.Approval5 = 0;
                                    pm.ApprovalName1 = pm.CreatedBy;
                                    pm.Rewiever1 = 0;
                                    pm.Rewiever2 = 0;
                                    pm.Rewiever3 = 0;
                                    pm.Rewiever4 = 0;
                                    pm.Rewiever5 = 0;
                                    pm.ApprovedBy = pm.CreatedBy;
                                    db.Entry(pm).State = EntityState.Modified;
                                }
                                else /// Send for approval
                                {
                                    /// Send Approved 
                                    PurchaseOrderMaster pm = db.DPurchaseOrderMaster.Where(a => a.PurchaseOrderId == pid).SingleOrDefault();
                                    pm.Status = "Send for Approval";
                                    pm.Approval1 = get_approvalz.Approval1;
                                    pm.Approval2 = get_approvalz.Approval2;
                                    pm.Approval3 = get_approvalz.Approval3;
                                    pm.Approval4 = get_approvalz.Approval4;
                                    pm.Approval5 = get_approvalz.Approval5;
                                    pm.Rewiever1 = get_approvalz.Reviewer1;
                                    pm.Rewiever2 = get_approvalz.Reviewer2;
                                    pm.Rewiever3 = get_approvalz.Reviewer3;
                                    pm.Rewiever4 = get_approvalz.Reviewer4;
                                    pm.Rewiever5 = get_approvalz.Reviewer5;
                                    pm.ApprovalName1 = get_approvalz.ApprovalName1;
                                    pm.ApprovalName2 = get_approvalz.ApprovalName2;
                                    pm.ApprovalName3 = get_approvalz.ApprovalName3;
                                    pm.ApprovalName4 = get_approvalz.ApprovalName4;
                                    pm.ApprovalName5 = get_approvalz.ApprovalName5;
                                    pm.RewieverName1 = get_approvalz.ReviewerName1;
                                    pm.RewieverName2 = get_approvalz.ReviewerName2;
                                    pm.RewieverName3 = get_approvalz.ReviewerName3;
                                    pm.RewieverName4 = get_approvalz.ReviewerName4;
                                    pm.RewieverName5 = get_approvalz.ReviewerName5;
                                    pm.progress = "20";
                                    pm.Level = get_approvalz.Level;
                                    db.Entry(pm).State = EntityState.Modified;
                                    Sms s = new Sms();
                                    string msg = "ShopKirana PR id: " + pm.PurchaseOrderId + smstempmsg;
                                    string Mob = db.Peoples.Where(q => q.PeopleID == get_approvalz.Approval1).Select(q => q.Mobile).SingleOrDefault();
                                    s.sendOtp(Mob, msg,"");
                                }
                            }
                            #endregion

                            db.Commit();
                            dbContextTransaction.Commit();
                            return Request.CreateResponse(HttpStatusCode.OK, "Add Successfuly");
                        }
                        else
                        {
                            dbContextTransaction.Rollback();
                            return Request.CreateResponse(HttpStatusCode.NotFound, "Add not Successfuly");
                        }
                    }
                    catch (Exception ex)
                    {
                        dbContextTransaction.Rollback();
                        return Request.CreateResponse(HttpStatusCode.BadRequest, ex.InnerException);

                    }
                }
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex.Message);
            }
        }

        /// <summary>
        /// Add Po from Draft
        /// </summary>
        /// <param name="pdata"></param>
        /// <returns></returns>        
        [Route("addPofromDraft")]
        [AcceptVerbs("POST")]
        public HttpResponseMessage AddPoFrDraft(PurchaseOrderMaster po)
        {
            using (AuthContext db = new AuthContext())
            {
                try
                {
                    var identity = User.Identity as ClaimsIdentity;
                    int compid = 0, userid = 0;
                    int? Warehouse_id = 0;
                    double ETtlamt = po.ETotalAmount;
                    int pid = po.PurchaseOrderId;

                    if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))
                        compid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);

                    if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                        userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

                    if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "Warehouseid"))
                        Warehouse_id = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "Warehouseid").Value);

                    Warehouse_id = po.WarehouseId;
                    if (Warehouse_id > 0)
                    {
                        string smstempmsg = " is waiting for your approval. ShopKirana";

                        #region level allocation
                        PoApproval get_approvalz = db.PoApprovalDB.Where(a => a.AmountlmtMin <= ETtlamt && a.AmountlmtMax >= ETtlamt && a.Warehouseid == Warehouse_id).FirstOrDefault();
                        if (get_approvalz != null)
                        {
                            if (get_approvalz.Level == "Level1")  /// Self Approved
                            {
                                PurchaseOrderMaster pm = db.DPurchaseOrderMaster.Where(a => a.PurchaseOrderId == pid).SingleOrDefault();
                                pm.Status = "Self Approved";
                                pm.progress = "50";
                                pm.Level = "Level1";
                                pm.Approval1 = 0;
                                pm.Approval2 = 0;
                                pm.Approval3 = 0;
                                pm.Approval4 = 0;
                                pm.Approval5 = 0;
                                pm.ApprovalName1 = pm.CreatedBy;
                                pm.Rewiever1 = 0;
                                pm.Rewiever2 = 0;
                                pm.Rewiever3 = 0;
                                pm.Rewiever4 = 0;
                                pm.Rewiever5 = 0;
                                pm.ApprovedBy = pm.CreatedBy;
                                db.Entry(pm).State = EntityState.Modified;
                                db.Commit();
                            }
                            else /// Send for approval
                            {
                                /// Send Approved 
                                PurchaseOrderMaster pm = db.DPurchaseOrderMaster.Where(a => a.PurchaseOrderId == pid).SingleOrDefault();
                                pm.Status = "Send for Approval";
                                pm.Approval1 = get_approvalz.Approval1;
                                pm.Approval2 = get_approvalz.Approval2;
                                pm.Approval3 = get_approvalz.Approval3;
                                pm.Approval4 = get_approvalz.Approval4;
                                pm.Approval5 = get_approvalz.Approval5;
                                pm.Rewiever1 = get_approvalz.Reviewer1;
                                pm.Rewiever2 = get_approvalz.Reviewer2;
                                pm.Rewiever3 = get_approvalz.Reviewer3;
                                pm.Rewiever4 = get_approvalz.Reviewer4;
                                pm.Rewiever5 = get_approvalz.Reviewer5;
                                pm.ApprovalName1 = get_approvalz.ApprovalName1;
                                pm.ApprovalName2 = get_approvalz.ApprovalName2;
                                pm.ApprovalName3 = get_approvalz.ApprovalName3;
                                pm.ApprovalName4 = get_approvalz.ApprovalName4;
                                pm.ApprovalName5 = get_approvalz.ApprovalName5;
                                pm.RewieverName1 = get_approvalz.ReviewerName1;
                                pm.RewieverName2 = get_approvalz.ReviewerName2;
                                pm.RewieverName3 = get_approvalz.ReviewerName3;
                                pm.RewieverName4 = get_approvalz.ReviewerName4;
                                pm.RewieverName5 = get_approvalz.ReviewerName5;
                                pm.progress = "20";
                                pm.Level = get_approvalz.Level;
                                db.Entry(pm).State = EntityState.Modified;
                                db.Commit();
                                Sms s = new Sms(); string msg = "ShopKirana PR id: " + pm.PurchaseOrderId + smstempmsg;
                                string Mob = db.Peoples.Where(q => q.PeopleID == get_approvalz.Approval1).Select(q => q.Mobile).SingleOrDefault();
                                s.sendOtp(Mob, msg,"");
                            }
                        }
                        #endregion
                        return Request.CreateResponse(HttpStatusCode.OK, "Add Successfuly");
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.NotFound, "Add not Successfuly");
                    }
                }
                catch (Exception ex)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex.Message);
                }
            }
        }

        /// <summary>
        /// Add PO as Draft
        /// </summary>
        /// <param name="pdata"></param>
        /// <returns></returns>
        [Route("addPodraft")]
        [AcceptVerbs("POST")]
        public HttpResponseMessage AddasDraft(List<POdata> pdata)
        {

            try
            {
                var identity = User.Identity as ClaimsIdentity;
                int compid = 0, userid = 0;
                int Warehouse_id = 0;
                double ETtlamt = 0;
                int pid = 0;
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
                using (var dbContextTransaction = db.Database.BeginTransaction())
                {
                    try
                    {
                        #region get Estimate amount
                        /// Get Estimate amount
                        foreach (var data in pdata)
                        {
                            var item1 = db.itemMasters.Where(z => z.ItemId == data.ItemId && z.CompanyId == compid && z.WarehouseId == data.WarehouseId).FirstOrDefault();
                            int qty = data.Noofset * data.PurchaseMinOrderQty;
                            //ETtlamt += Convert.ToDouble(item1.PurchasePrice) * qty;
                            ETtlamt += (Math.Round(item1.NetPurchasePrice * (1 + (item1.TotalTaxPercentage / 100)), 3) * qty);//With tax net purchase price add by raj

                        }
                        #endregion

                        foreach (var data in pdata)
                        {
                            Warehouse_id = data.WarehouseId;
                            var item1 = db.itemMasters.Where(z => z.ItemId == data.ItemId && z.CompanyId == compid && z.WarehouseId == data.WarehouseId).FirstOrDefault();
                            var supplier1 = db.Suppliers.Where(s => s.SupplierId == data.SupplierId && s.CompanyId == compid).SingleOrDefault();
                            People buyer = db.Peoples.Where(a => a.PeopleID == supplier1.PeopleID).SingleOrDefault();
                            var warehouse = db.Warehouses.Where(a => a.WarehouseId == item1.WarehouseId).SingleOrDefault();
                            PurchaseOrderMaster pm = new PurchaseOrderMaster();
                            pm.SupplierId = supplier1.SupplierId;
                            pm.SupplierName = supplier1.Name;
                            pm.CreationDate = indianTime;
                            pm.WarehouseId = item1.WarehouseId;
                            pm.WarehouseCity = warehouse.CityName;
                            pm.CompanyId = compid;
                            pm.WarehouseName = item1.WarehouseName;
                            pm.Status = "Draft";
                            pm.progress = "0";
                            pm.PoType = "Manual";
                            pm.ETotalAmount = ETtlamt;
                            pm.BuyerId = buyer.PeopleID;
                            pm.BuyerName = buyer.DisplayName;
                            pm.Acitve = true;
                            pm.IsLock = true;
                            db.DPurchaseOrderMaster.Add(pm);
                            db.Commit();
                            pid = pm.PurchaseOrderId;
                            for (var i = 0; i < pdata.ToList().Count(); i++)
                            {
                                int supplierid = Convert.ToInt32(pdata[i].SupplierId);
                                int itemid = Convert.ToInt32(pdata[i].ItemId);
                                var item = db.itemMasters.Where(z => z.ItemId == itemid && z.CompanyId == compid && z.WarehouseId == data.WarehouseId).FirstOrDefault();
                                var supplier = db.Suppliers.Where(s => s.SupplierId == supplierid && s.CompanyId == compid).SingleOrDefault();

                                PurchaseOrderDetail pd = new PurchaseOrderDetail();
                                var qty = pdata[i].Noofset * pdata[i].PurchaseMinOrderQty;
                                pd.PurchaseOrderId = pm.PurchaseOrderId;
                                pd.ItemId = item.ItemId;
                                pd.ItemNumber = item.Number;
                                pd.itemBaseName = item.itemBaseName;
                                pd.ItemMultiMRPId = item.ItemMultiMRPId;//for multimrp
                                pd.MRP = item.price;
                                pd.SellingSku = item.SellingSku;
                                pd.ItemName = item.itemname;
                                pd.PurchaseQty = qty;
                                pd.CreationDate = indianTime;
                                pd.Status = "ordered";
                                pd.MOQ = item.PurchaseMinOrderQty;
                                pd.Price = Math.Round(item.NetPurchasePrice * (1 + (item.TotalTaxPercentage / 100)), 3);//With tax net purchase price add by raj
                                pd.WarehouseId = item.WarehouseId;
                                pd.CompanyId = item.CompanyId;
                                pd.WarehouseName = item.WarehouseName;
                                pd.SupplierId = supplier.SupplierId;
                                pd.SupplierName = supplier.Name;
                                pd.TotalQuantity = Convert.ToInt32(pd.PurchaseQty);
                                pd.PurchaseName = item.PurchaseUnitName;
                                pd.PurchaseSku = item.PurchaseSku;
                                pd.ConversionFactor = Convert.ToInt32(item.PurchaseMinOrderQty);
                                db.DPurchaseOrderDeatil.Add(pd);
                            }
                            break;
                        }
                        db.Commit();
                        dbContextTransaction.Commit();
                        return Request.CreateResponse(HttpStatusCode.OK, "Add Successfuly as a draft");
                    }
                    catch (Exception ex)
                    {
                        dbContextTransaction.Rollback();
                        return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex.Message);
                    }
                }

            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex.Message);
            }
        }

        [Route("sendToSuppApp")]
        [HttpGet]
        public HttpResponseMessage SendToSuppApp(int pid)
        {
            using (AuthContext db = new AuthContext())
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
                        PurchaseOrderMaster pm = db.DPurchaseOrderMaster.Where(q => q.PurchaseOrderId == pid).FirstOrDefault();
                        pm.IsSendSupplierApp = true;
                        db.Entry(pm).State = EntityState.Modified;
                        db.Commit();
                        return Request.CreateResponse(HttpStatusCode.OK, "Send to supplier app.");
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.NotFound, "Not Successfuly Send.");
                    }
                }
                catch (Exception ex)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex.Message);
                }
            }
        }

        [Route("sendPdf")]
        [HttpGet]
        public PurchaseOrderMaster SendPdf(int pid)
        {
            using (AuthContext db = new AuthContext())
            {
                try
                {
                    SendPdfDto2 obj;
                    PurchaseOrderMaster po = db.DPurchaseOrderMaster.Where(a => a.PurchaseOrderId == pid).FirstOrDefault();
                    List<PurchaseOrderDetail> pod = db.DPurchaseOrderDeatil.Where(a => a.PurchaseOrderId == pid).ToList();
                    Sms s = new Sms();

                    obj = new SendPdfDto2()
                    {
                        p = po,
                        pd = pod
                    };
                    s.createPdf(obj, pid);
                    SendMailCreditWalletNotification(po.PurchaseOrderId);
                    System.Web.HttpResponse response = System.Web.HttpContext.Current.Response;

                    response.AppendHeader("content-disposition", "attachment; filename=" + pid);
                    string path = @"C:\Test\" + pid + ".pdf";
                    string type = "Application/pdf";
                    response.ContentType = type;
                    response.WriteFile(path);
                    response.End(); //give POP to user for file downlaod
                    return po;
                }
                catch (Exception ex)
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// Remove item
        /// </summary>
        /// <param name="temppo"></param>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        [Route("resendPo")]
        [AcceptVerbs("Post")]
        public PurchaseList resendPO(PurchaseList temppo)
        {
            using (AuthContext db = new AuthContext())
            {
                try
                {
                    var identity = User.Identity as ClaimsIdentity;
                    int compid = 0, userid = 0;
                    int Warehouse_id = 0;
                    PurchaseOrderMaster pid = db.DPurchaseOrderMaster.Where(q => q.PurchaseOrderId == temppo.PurchaseOrderId).FirstOrDefault();
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
                        SendMailToEmail(temppo.PurchaseOrderId, temppo.reSendemail);
                        return temppo;
                    }
                    return null;
                }
                catch (Exception ex)
                {
                    logger.Error("Error in addQuesAns " + ex.Message);
                    logger.Info("End  addWarehouse: ");
                    return null;
                }
            }
        }

        #region get PO History
        [Route("POAddHistory")]
        [HttpGet]
        public dynamic POAddHistory(int PurchaseorderId)
        {
            using (AuthContext db = new AuthContext())
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

                    var data = db.POHistoryDB.Where(x => x.PurchaseOrderId == PurchaseorderId).ToList();
                    return data;
                }
                catch (Exception ex)
                {
                    return false;
                }
            }
        }
        #endregion

        #region get Itemdetailshistory
        [Route("Itemdetailshistory")]
        [HttpGet]
        public dynamic Itemdetailshistory(int PurchaseorderId)
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
                using (AuthContext db = new AuthContext())
                {

                    var data = db.PoEditItemHistoryDB.Where(x => x.PurchaseOrderId == PurchaseorderId).ToList();
                    return data;
                }
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        #endregion

        #region Function SendMail
        /// <summary>
        /// Reject po by reviewer
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        [Authorize]
        [Route("cmtbtapvl")]
        [HttpPut]
        public PurchaseOrderMaster CommentbyApprovar(PurchaseOrderMaster data)
        {
            logger.Info("start Category: ");
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
                using (var context = new AuthContext())
                {
                    PoApproval IsRevHere = context.PoApprovalDB.Where(a => a.Level == data.Level).FirstOrDefault();
                    PurchaseOrderMaster po = context.DPurchaseOrderMaster.Where(a => a.PurchaseOrderId == data.PurchaseOrderId).SingleOrDefault();
                    //po.Status = "Rejected";
                    po.CommentApvl = data.CommentApvl;
                    context.Entry(po).State = EntityState.Modified;
                    context.Commit();
                    return po;
                }
            }
            catch (Exception ex)
            {
                logger.Error("Error in Category " + ex.Message);
                logger.Info("End  Category: ");
                return null;
            }
        }

        /// <summary>
        /// Comment by reviewer  
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        [Authorize]
        [Route("RejectbyReviewer")]
        [HttpPut]
        public PurchaseOrderMaster RejectByReviewer(PurchaseOrderMaster data)
        {
            logger.Info("start Category: ");
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
                using (var context = new AuthContext())
                {
                    PoApproval IsRevHere = context.PoApprovalDB.Where(a => a.Level == data.Level).FirstOrDefault();
                    PurchaseOrderMaster po = context.DPurchaseOrderMaster.Where(a => a.PurchaseOrderId == data.PurchaseOrderId).SingleOrDefault();
                    //po.Status = "Rejected";
                    po.Comment = data.Comment;
                    context.Entry(po).State = EntityState.Modified;
                    context.Commit();
                    return po;
                }
            }
            catch (Exception ex)
            {
                logger.Error("Error in Category " + ex.Message);
                logger.Info("End  Category: ");
                return null;
            }
        }

        /// <summary>
        /// Reject po by Approver
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        [Authorize]
        [Route("RejectbyApprover")]
        [HttpPut]
        public PurchaseOrderMaster RejectByApprover(PurchaseOrderMaster data)
        {
            logger.Info("start Category: ");
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
                using (var context = new AuthContext())
                {
                    People p = context.Peoples.Where(q => q.PeopleID == userid).FirstOrDefault();
                    PoApproval IsRevHere = context.PoApprovalDB.Where(a => a.Level == data.Level).FirstOrDefault();
                    PurchaseOrderMaster po = context.DPurchaseOrderMaster.Where(a => a.PurchaseOrderId == data.PurchaseOrderId).SingleOrDefault();
                    po.Status = "Rejected";
                    po.RejectedBy = p.DisplayName;
                    po.CommentApvl = data.CommentApvl;
                    context.Entry(po).State = EntityState.Modified;
                    context.Commit();
                    return po;
                }
            }
            catch (Exception ex)
            {
                logger.Error("Error in Category " + ex.Message);
                logger.Info("End  Category: ");
                return null;
            }
        }

        /// <summary>
        /// //SendMail
        /// </summary>
        /// <param name="poid"></param>
        /// <param name="email"></param>
        public bool SendMailCreditWalletNotification(int poid)
        {
            SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["authcontext"].ConnectionString);
            con.Open();
            int ord = 0;
            int sid = 0;
            using (AuthContext db = new AuthContext())
            {
                Supplier sup = new Supplier();
                PurchaseOrderMaster pm = db.DPurchaseOrderMaster.Where(x => x.IsPDFsentGmail == false & x.PurchaseOrderId == poid).SingleOrDefault();
                ord = pm.PurchaseOrderId;
                sid = pm.SupplierId;
                if (sid != 0)
                {
                    sup = db.Suppliers.Where(a => a.SupplierId == sid).SingleOrDefault();
                }
                if (ord != 0)
                {
                    string masteremail = ConfigurationManager.AppSettings["MasterEmail"];
                    string masterpassword = ConfigurationManager.AppSettings["MasterPassword"];
                    string pdflocation = ConfigurationManager.AppSettings["PDFLocation"];
                    try
                    {
                        string body = "<div style='background: #FAFAFA; color: #333333; padding-left: 30px;font-family: arial,sans-serif; font-size: 14px;'>";
                        body += "<img style='padding-top: 10px;' src='http://shopkirana.com/wp-content/uploads/2015/07/ShopKirana-Logo11.png'><br/>";
                        body += "<h3 style='background-color: rgb(241, 89, 34);'>Alert! Purchase Order invoice pdf</h3>";
                        body += "Hello,";
                        body += "<p> Please find attechment of PO <strong>";
                        body += poid + "</strong>" + "below.</p>";
                        body += "Thanks,";
                        body += "<br />";
                        body += "<b>IT Team</b>";
                        body += "</div>";
                        var msg = new MailMessage(masteremail, masteremail, "Attechment of PO PDF. ", body);
                        msg.To.Add(sup.EmailId);
                        msg.IsBodyHtml = true;
                        System.Net.Mail.Attachment attachment;
                        attachment = new System.Net.Mail.Attachment(@"" + pdflocation + "" + poid + ".pdf");
                        msg.Attachments.Add(attachment);
                        var smtpClient = new SmtpClient("smtp.gmail.com", 587); //if your from email address is "from@hotmail.com" then host should be "smtp.hotmail.com"
                        smtpClient.UseDefaultCredentials = true;
                        smtpClient.Credentials = new NetworkCredential(masteremail, masterpassword);
                        smtpClient.EnableSsl = true;
                        smtpClient.Send(msg);
                        SqlCommand cmd2 = new SqlCommand("Update [PurchaseOrderMasters] set [IsPDFsentGmail] = 'True' where [PurchaseOrderId] = " + poid + "  ", con);
                        cmd2.ExecuteNonQuery();
                        return true;
                    }
                    catch (Exception ex) { return false; }
                }
                return false;
            }
        }

        /// <summary>
        /// reSendMail
        /// </summary>
        /// <param name="poid"></param>
        /// <param name="email"></param>
        public bool SendMailToEmail(int poid, string mail)
        {

            if (poid != 0)
            {
                string masteremail = ConfigurationManager.AppSettings["MasterEmail"];
                string masterpassword = ConfigurationManager.AppSettings["MasterPassword"];
                string pdflocation = ConfigurationManager.AppSettings["PDFLocation"];
                try
                {
                    string body = "<div style='background: #FAFAFA; color: #333333; padding-left: 30px;font-family: arial,sans-serif; font-size: 14px;'>";
                    body += "<img style='padding-top: 10px;' src='http://shopkirana.com/wp-content/uploads/2015/07/ShopKirana-Logo11.png'><br/>";
                    body += "<h3 style='background-color: rgb(241, 89, 34);'>Alert! Purchase Order Invoice Pdf</h3>";
                    body += "Hello,";
                    body += "<p> Please find attechment of PO <strong>";
                    body += poid + "</strong>" + "below.</p>";
                    body += "Thanks,";
                    body += "<br />";
                    body += "<b>IT Team</b>";
                    body += "</div>";
                    var msg = new MailMessage(masteremail, masteremail, "Attechment of PO PDF. ", body);
                    msg.To.Add(mail);
                    msg.IsBodyHtml = true;
                    System.Net.Mail.Attachment attachment;
                    attachment = new System.Net.Mail.Attachment(@"" + pdflocation + "" + poid + ".pdf");
                    msg.Attachments.Add(attachment);
                    var smtpClient = new SmtpClient("smtp.gmail.com", 587); //if your from email address is "from@hotmail.com" then host should be "smtp.hotmail.com"
                    smtpClient.UseDefaultCredentials = true;
                    smtpClient.Credentials = new NetworkCredential(masteremail, masterpassword);
                    smtpClient.EnableSsl = true;
                    smtpClient.Send(msg);
                    return true;
                }
                catch (Exception ex) { return false; }
            }
            return false;
        }
        #endregion



        #region   get item Multi mrp List using ItemId and WarehouseId
        [Route("GetItemMRPById")]
        [HttpGet]
        public List<ItemMultiMRP> GetItemMRP(int ItemId, int WarehouseId)
        {
            logger.Info("Get Item multiprice  ");
            using (AuthContext db = new AuthContext())
            {
                try
                {
                    var item = db.itemMasters.Where(x => x.ItemId == ItemId && x.WarehouseId == WarehouseId).FirstOrDefault();

                    List<ItemMultiMRP> ItemMultiMRP = db.ItemMultiMRPDB.Where(x => x.ItemNumber == item.Number).ToList();
                    logger.Info("Get Item multiprice ");
                    return ItemMultiMRP;
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

        #region   get item Multi mrp List using ItemId and WarehouseId
        [Route("GetItemMRPByIdByBarcode")]
        [HttpGet]
        public List<ItemMultiMRP> GetItemMRPBarcode(string Number)
        {
            logger.Info("Get Item multiprice  ");

            List<ItemMultiMRP> ItemMultiMRP;
            try
            {
                using (AuthContext db = new AuthContext())
                {
                    if (Number != null)
                    {
                        var item = db.ItemBarcodes.Where(x => x.ItemNumber == Number || x.Barcode == Number).FirstOrDefault();
                        ItemMultiMRP = db.ItemMultiMRPDB.Where(x => x.ItemNumber == item.ItemNumber).ToList();
                        logger.Info("Get Item multiprice ");
                        return ItemMultiMRP;
                    }
                    else
                    {
                        return ItemMultiMRP = null;
                    }
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


        [Authorize]
        [Route("savechangebuyer")]
        [HttpPut]
        public PurchaseOrderMaster savechangebuyer(putbuyerIdPo data)
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
                using (var context = new AuthContext())
                {
                    People p = context.Peoples.Where(q => q.PeopleID == data.PeopleID).SingleOrDefault();
                    PurchaseOrderMaster po = context.DPurchaseOrderMaster.Where(a => a.PurchaseOrderId == data.PurchaseOrderId).SingleOrDefault();
                    po.BuyerId = p.PeopleID;
                    po.BuyerName = p.DisplayName;
                    context.Entry(po).State = EntityState.Modified;
                    context.Commit();
                    return po;
                }
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        #region Add Blank PO 
        /// <summary>
        /// Create  Blank PO Manualy 
        /// created date:06/06/2019
        /// created by raj
        /// </summary>
        /// <param name="pdata"></param>
        /// <returns></returns>
        [Route("AddBlankPO")]
        [AcceptVerbs("POST")]
        public HttpResponseMessage AddBlankPO(List<POdata> pdata)
        {

            try
            {
                var identity = User.Identity as ClaimsIdentity;
                int compid = 0, userid = 0;
                int Warehouse_id = 0;
                double ETtlamt = 0;
                int pid = 0;
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
                    Warehouse_id = pdata[0].WarehouseId;
                    if (Warehouse_id > 0)
                    {
                        #region get Estimate amount
                        /// Get Estimate amount
                        foreach (var data in pdata)
                        {
                            try
                            {
                                var item1 = db.itemMasters.Where(z => z.ItemId == data.ItemId && z.CompanyId == compid && z.WarehouseId == Warehouse_id).FirstOrDefault();
                                int qty = data.Noofset * data.PurchaseMinOrderQty;
                            }
                            catch (Exception ex) { }
                        }
                        #endregion

                        foreach (var data in pdata)
                        {
                            var item1 = db.itemMasters.Where(z => z.ItemId == data.ItemId && z.CompanyId == compid && z.WarehouseId == Warehouse_id).FirstOrDefault();
                            var supplier1 = db.Suppliers.Where(s => s.SupplierId == data.SupplierId && s.CompanyId == compid).SingleOrDefault();
                            People p = db.Peoples.Where(a => a.PeopleID == userid).SingleOrDefault();
                            People buyer = db.Peoples.Where(a => a.PeopleID == data.BuyerId).SingleOrDefault();
                            var Depo = db.DepoMasters.Where(d => d.DepoId == data.DepoId).SingleOrDefault();
                            PurchaseOrderMaster pm = new PurchaseOrderMaster();
                            pm.SupplierId = supplier1.SupplierId;
                            pm.SupplierName = supplier1.Name;
                            pm.CreationDate = indianTime;
                            pm.WarehouseId = item1.WarehouseId;
                            pm.CompanyId = compid;
                            pm.WarehouseName = item1.WarehouseName;
                            pm.Status = "Blank PO";
                            pm.progress = "0";
                            pm.IR_Progress = "0";
                            pm.PoType = "Manual";

                            try
                            {
                                pm.DepoId = Depo.DepoId;
                                pm.DepoName = Depo.DepoName;
                            }
                            catch (Exception ex) { }
                            //pm.Advance_Amt = data.Advance_Amt;
                            try
                            {
                                pm.BuyerId = buyer.PeopleID;
                                pm.BuyerName = buyer.DisplayName;
                            }
                            catch (Exception ex) { }
                            pm.Acitve = true;
                            pm.CreatedBy = p.PeopleFirstName + " " + p.PeopleLastName;
                            db.DPurchaseOrderMaster.Add(pm);
                            db.Commit();
                            pid = pm.PurchaseOrderId;
                            for (var i = 0; i < pdata.ToList().Count(); i++)
                            {
                                int supplierid = Convert.ToInt32(pdata[i].SupplierId);
                                int itemid = Convert.ToInt32(pdata[i].ItemId);
                                var item = db.itemMasters.Where(z => z.ItemId == itemid && z.CompanyId == compid && z.WarehouseId == Warehouse_id).FirstOrDefault();
                                var supplier = db.Suppliers.Where(s => s.SupplierId == supplierid && s.CompanyId == compid).SingleOrDefault();

                                PurchaseOrderDetail pd = new PurchaseOrderDetail();
                                var qty = pdata[i].Noofset * pdata[i].PurchaseMinOrderQty;
                                pd.PurchaseOrderId = pm.PurchaseOrderId;
                                pd.ItemId = item.ItemId;
                                pd.ItemMultiMRPId = item.ItemMultiMRPId;
                                pd.HSNCode = item.HSNCode;
                                pd.MRP = item.price;
                                pd.SellingSku = item.SellingSku;
                                pd.ItemName = item.itemname;
                                pd.PurchaseQty = qty;
                                pd.CreationDate = indianTime;
                                pd.Status = "Blank PO";
                                pd.MOQ = item.PurchaseMinOrderQty;
                                pd.WarehouseId = item.WarehouseId;
                                pd.CompanyId = item.CompanyId;
                                pd.WarehouseName = item.WarehouseName;
                                pd.SupplierId = supplier.SupplierId;
                                pd.SupplierName = supplier.Name;
                                try
                                {
                                    pd.DepoId = Depo.DepoId;
                                    pd.DepoName = Depo.DepoName;
                                }
                                catch (Exception ex) { }
                                pd.TotalQuantity = Convert.ToInt32(pd.PurchaseQty);
                                pd.PurchaseName = item.PurchaseUnitName;
                                pd.PurchaseSku = item.PurchaseSku;
                                pd.ConversionFactor = Convert.ToInt32(item.PurchaseMinOrderQty);
                                db.DPurchaseOrderDeatil.Add(pd);
                                int idd = db.Commit();


                            }
                            break;
                        }
                        #region Add New Purchase Po History
                        POHistory Ph = new POHistory();
                        int id = Convert.ToInt32(pdata[0].ItemId);
                        var UserName = db.Peoples.Where(y => y.PeopleID == userid).Select(c => c.DisplayName).SingleOrDefault();
                        var poid = db.DPurchaseOrderDeatil.Where(z => z.ItemId == id && z.WarehouseId == Warehouse_id).Select(c => new { c.PurchaseOrderId, c.WarehouseName }).OrderByDescending(o => o.PurchaseOrderId).FirstOrDefault();

                        Ph.PurchaseOrderId = Convert.ToInt32(poid.PurchaseOrderId);
                        Ph.WarehouseName = poid.WarehouseName;
                        Ph.Status = "Blank PO";
                        Ph.EditBy = UserName;
                        Ph.UpdatedDate = DateTime.Now;
                        db.POHistoryDB.Add(Ph);
                        db.Commit();
                        #endregion

                        return Request.CreateResponse(HttpStatusCode.OK, "Add Successfuly");
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.NotFound, "Add not Successfuly");
                    }
                }
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex.Message);
            }
        }
        #endregion
        #region Get Item Information details for Blank PO
        /// <summary>
        /// get the information item  id based
        /// Created Date:06/05/2019
        /// Created By Raj
        /// </summary>
        /// <param name="type"></param>
        /// <param name="id"></param>
        /// <param name="Wid"></param>
        /// <returns>it</returns>
        [Route("GetItem")]
        [HttpGet]
        public ItemMaster Get(int ItemId, int Wid)
        {
            ItemMaster item = new ItemMaster();


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
                    int CompanyId = compid;
                    item = db.itemMasters.Where(c => c.ItemId == ItemId && c.WarehouseId == Wid).FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                item = null;
            }

            return item;
        }
        #endregion
        #region EditBlank PO
        /// <summary>
        /// Edit PO Manualy Send to Approval
        /// Created Date:05/06/2019
        /// created by Raj
        /// </summary>
        /// <param name="pdata"></param>
        /// <returns></returns>
        [Route("EditBlankPO")]
        [HttpPut]
        public HttpResponseMessage EditBlankPO(BlankPODetails pdata)
        {

            try
            {
                var identity = User.Identity as ClaimsIdentity;
                int compid = 0, userid = 0;
                int Warehouse_id = 0;
                double ETtlamt = 0;
                int pid = 0;
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
                    PurchaseOrderDetail Blankpodata = db.DPurchaseOrderDeatil.Where(x => x.PurchaseOrderId == pdata.PurchaseOrderId && x.ItemId == pdata.ItemId).FirstOrDefault();
                    ItemMaster item = db.itemMasters.Where(x => x.ItemId == pdata.ItemId).FirstOrDefault();
                    if (pdata.qty != 0)
                    {
                        Blankpodata.TotalQuantity = pdata.qty;
                    }
                    //Blankpodata.Price = pdata.Price;
                    Blankpodata.Price = Math.Round(pdata.Price * (1 + (item.TotalTaxPercentage / 100)), 3); //With tax net purchase price add by raj
                    db.Entry(Blankpodata).State = EntityState.Modified;
                    db.Commit();
                }
                return Request.CreateResponse(HttpStatusCode.OK, "Add Successfuly");
            }
            catch (Exception Ex)
            {
                return null;
            }

        }
        #endregion
        #region  Add Blank PO 
        /// <summary>
        /// Add to save data blank po to po
        /// Created Date:05/06/2019
        /// Created By Raj
        /// </summary>
        /// <param name="pdata"></param>
        /// <returns></returns>
        [Route("AddBlankPOdata")]
        [HttpPut]
        public HttpResponseMessage AddBlankPOdata(List<PurchaseOrderDetail> pdata)
        {

            try
            {
                var identity = User.Identity as ClaimsIdentity;
                int compid = 0, userid = 0;
                int Warehouse_id = 0;
                double ETtlamt = 0;
                int pid = 0;
                bool nppdata = false;
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
                    int? WarehouseId = pdata[0].WarehouseId;
                    foreach (var po in pdata)
                    {
                        if (po.Price != 0 && po.Price != null)
                        {
                            nppdata = true;
                        }

                    }
                    if (nppdata == true)
                    {
                        #region get Estimate amount
                        /// Get Estimate amount
                        foreach (var data in pdata)
                        {
                            try
                            {


                                ETtlamt += (data.Price * data.TotalQuantity);//With tax net purchase price add by raj
                            }
                            catch (Exception ex) { }
                        }
                        #endregion

                        foreach (var data in pdata)
                        {
                            var item1 = db.itemMasters.Where(z => z.ItemId == data.ItemId && z.CompanyId == compid && z.WarehouseId == WarehouseId).FirstOrDefault();
                            var supplier1 = db.Suppliers.Where(s => s.SupplierId == data.SupplierId && s.CompanyId == compid).SingleOrDefault();
                            People p = db.Peoples.Where(a => a.PeopleID == userid).SingleOrDefault();
                            //People buyer = db.Peoples.Where(a => a.PeopleID == data.BuyerId).SingleOrDefault();
                            PurchaseOrderMaster pm = db.DPurchaseOrderMaster.Where(x => x.PurchaseOrderId == data.PurchaseOrderId).FirstOrDefault();

                            pm.Status = "pending";
                            pm.progress = "0";
                            pm.IR_Progress = "0";
                            pm.ETotalAmount = ETtlamt;
                            //try
                            //{
                            //    pm.BuyerId = buyer.PeopleID;
                            //    pm.BuyerName = buyer.DisplayName;
                            //}
                            //catch (Exception ex) { }
                            pm.Acitve = true;
                            db.Entry(pm).State = EntityState.Modified;
                            db.Commit();
                            pid = pm.PurchaseOrderId;
                            for (var i = 0; i < pdata.ToList().Count(); i++)
                            {
                                int supplierid = Convert.ToInt32(pdata[i].SupplierId);
                                int itemid = Convert.ToInt32(pdata[i].ItemId);
                                double price = pdata[i].Price;
                                int TotalQuantity = pdata[i].TotalQuantity;
                                var item = db.itemMasters.Where(z => z.ItemId == itemid && z.CompanyId == compid && z.WarehouseId == WarehouseId).FirstOrDefault();
                                var supplier = db.Suppliers.Where(s => s.SupplierId == supplierid && s.CompanyId == compid).SingleOrDefault();

                                PurchaseOrderDetail pd = db.DPurchaseOrderDeatil.Where(x => x.PurchaseOrderId == pid && x.ItemId == itemid).FirstOrDefault();
                                var qty = TotalQuantity;
                                pd.PurchaseOrderId = pm.PurchaseOrderId;
                                pd.Status = "ordered";
                                pd.Price = price;
                                pd.TotalQuantity = Convert.ToInt32(TotalQuantity);
                                db.Entry(pd).State = EntityState.Modified;
                                db.Commit();
                            }
                            break;
                        }
                        #region Add New Purchase Po History
                        POHistory Ph = new POHistory();
                        int id = Convert.ToInt32(pdata[0].ItemId);
                        var UserName = db.Peoples.Where(y => y.PeopleID == userid).Select(c => c.DisplayName).SingleOrDefault();
                        var poid = db.DPurchaseOrderDeatil.Where(z => z.ItemId == id && z.WarehouseId == WarehouseId).Select(c => new { c.PurchaseOrderId, c.WarehouseName }).OrderByDescending(o => o.PurchaseOrderId).FirstOrDefault();

                        Ph.PurchaseOrderId = Convert.ToInt32(poid.PurchaseOrderId);
                        Ph.WarehouseName = poid.WarehouseName;
                        Ph.Status = "Add Pending";
                        Ph.EditBy = UserName;
                        Ph.UpdatedDate = DateTime.Now;
                        db.POHistoryDB.Add(Ph);
                        db.Commit();
                        #endregion
                        string smstempmsg = " is waiting for your approval. ShopKirana";

                        #region level allocation
                        PoApproval get_approvalz = db.PoApprovalDB.Where(a => a.AmountlmtMin <= ETtlamt && a.AmountlmtMax >= ETtlamt && a.Warehouseid == WarehouseId).FirstOrDefault();
                        if (get_approvalz != null)
                        {
                            if (get_approvalz.Level == "Level1")  /// Self Approved
                            {
                                PurchaseOrderMaster pm = db.DPurchaseOrderMaster.Where(a => a.PurchaseOrderId == pid).SingleOrDefault();
                                pm.Status = "Self Approved";
                                pm.progress = "50";
                                pm.Level = "Level1";
                                pm.Approval1 = 0;
                                pm.Approval2 = 0;
                                pm.Approval3 = 0;
                                pm.Approval4 = 0;
                                pm.Approval5 = 0;
                                pm.ApprovalName1 = pm.CreatedBy;
                                pm.Rewiever1 = 0;
                                pm.Rewiever2 = 0;
                                pm.Rewiever3 = 0;
                                pm.Rewiever4 = 0;
                                pm.Rewiever5 = 0;
                                pm.ApprovedBy = pm.CreatedBy;
                                db.Entry(pm).State = EntityState.Modified;
                                db.Commit();
                            }
                            else /// Send for approval
                            {
                                /// Send Approved 
                                PurchaseOrderMaster pm = db.DPurchaseOrderMaster.Where(a => a.PurchaseOrderId == pid).SingleOrDefault();
                                pm.Status = "Send for Approval";
                                pm.Approval1 = get_approvalz.Approval1;
                                pm.Approval2 = get_approvalz.Approval2;
                                pm.Approval3 = get_approvalz.Approval3;
                                pm.Approval4 = get_approvalz.Approval4;
                                pm.Approval5 = get_approvalz.Approval5;
                                pm.Rewiever1 = get_approvalz.Reviewer1;
                                pm.Rewiever2 = get_approvalz.Reviewer2;
                                pm.Rewiever3 = get_approvalz.Reviewer3;
                                pm.Rewiever4 = get_approvalz.Reviewer4;
                                pm.Rewiever5 = get_approvalz.Reviewer5;
                                pm.ApprovalName1 = get_approvalz.ApprovalName1;
                                pm.ApprovalName2 = get_approvalz.ApprovalName2;
                                pm.ApprovalName3 = get_approvalz.ApprovalName3;
                                pm.ApprovalName4 = get_approvalz.ApprovalName4;
                                pm.ApprovalName5 = get_approvalz.ApprovalName5;
                                pm.RewieverName1 = get_approvalz.ReviewerName1;
                                pm.RewieverName2 = get_approvalz.ReviewerName2;
                                pm.RewieverName3 = get_approvalz.ReviewerName3;
                                pm.RewieverName4 = get_approvalz.ReviewerName4;
                                pm.RewieverName5 = get_approvalz.ReviewerName5;
                                pm.progress = "20";
                                pm.Level = get_approvalz.Level;
                                db.Entry(pm).State = EntityState.Modified;
                                db.Commit();
                                Sms s = new Sms();
                                string msg = "ShopKirana PR id: " + pm.PurchaseOrderId + smstempmsg;
                                string Mob = db.Peoples.Where(q => q.PeopleID == get_approvalz.Approval1).Select(q => q.Mobile).SingleOrDefault();
                                s.sendOtp(Mob, msg,"");
                            }
                        }
                        #endregion

                        return Request.CreateResponse(HttpStatusCode.OK, "Add Successfuly");
                    }
                    else
                    {

                        return Request.CreateResponse(HttpStatusCode.OK, "Please Enter The Net Purchase price");
                    }
                }
            }
            catch (Exception Ex)
            {
                return null;
            }

        }
        #endregion
        #region po id based search filter in Blank PO
        /// <summary>
        /// Created Date:06/06/2019
        /// Created By Raj
        /// </summary>
        /// <param name="PoId"></param>
        /// <returns></returns>
        [Route("SearchBlankPo")]
        [HttpGet]
        public dynamic SearchBlankPo(string PoId)
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
                using (var context = new AuthContext())
                {
                    int poid = Convert.ToInt32(PoId);
                    if (Warehouse_id > 0)
                    {
                        logger.Info("User ID : {0} , Company Id : {1}", compid, userid);
                        var Podata = context.DPurchaseOrderMaster.Where(x => x.CompanyId == compid && x.WarehouseId == Warehouse_id && x.PurchaseOrderId == poid && x.Status == "Blank PO").ToList();

                        logger.Info("End PurchaseOrderMaster: ");
                        return Podata;
                    }
                    else
                    {
                        logger.Info("User ID : {0} , Company Id : {1}", compid, userid);
                        var Podata = context.DPurchaseOrderMaster.Where(x => x.CompanyId == compid && x.PurchaseOrderId == poid && x.Status == "Blank PO").ToList();

                        logger.Info("End PurchaseOrderMaster: ");
                        return Podata;
                    }
                }

            }
            catch (Exception ex)
            {
                logger.Error("Error in PurchaseOrderMaster " + ex.Message);
                logger.Info("End  PurchaseOrderMaster: ");
                return null;
            }
        }
        #endregion

        #region   Check NPP before create po
        [Route("checkNpp")]
        [HttpGet]
        public bool CheckNPP(int ItemId, int WarehouseId)
        {

            try
            {
                using (AuthContext db = new AuthContext())
                {
                    var item = db.itemMasters.Where(x => x.ItemId == ItemId && x.WarehouseId == WarehouseId).FirstOrDefault();

                    if (item.NetPurchasePrice > 0)
                    {
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
                logger.Error("Get NPP " + ex.Message);

                return false;
            }
        }
        #endregion


        #region SendMailManualInventory
        /// <summary>
        /// Created Date:01/07/2019
        /// Created by Raj
        /// </summary>
        /// <returns></returns>
        [Route("Sendmailforpendingpo")]
        [HttpGet]
        public dynamic Sendmailforpendingpo()
        {
            //SendMailManualInventory
            try
            {
                using (AuthContext db = new AuthContext())
                {
                    DateTime CD = DateTime.Now.AddDays(-48);
                    List<PurchaseOrderMaster> podata = db.DPurchaseOrderMaster.Where(x => x.Status == "Self Approved" || x.Status == "Send for Approval" || x.Status == "Approved" && (x.CreationDate > CD)).ToList();
                    List<string> createBystrs = podata.Select(x => x.CreatedBy).Distinct().ToList();
                    List<string> Approverstrs = podata.Select(x => x.ApprovedBy).Distinct().ToList();
                    List<int> wIds = podata.Select(x => x.WarehouseId).Distinct().ToList();
                    List<People> peoples = db.Peoples.Where(x => createBystrs.Contains(x.DisplayName) || Approverstrs.Contains(x.DisplayName) && x.Active==true).ToList();
                    List<Warehouse> whdatas = db.Warehouses.Where(x => wIds.Contains(x.WarehouseId)).ToList();
                    foreach (var data in podata)
                    {
                        try
                        {
                            People Creater = peoples.FirstOrDefault(x => x.CreatedBy == data.CreatedBy);
                            People Approver = peoples.FirstOrDefault(x => x.CreatedBy == data.ApprovedBy);
                            Warehouse whdata = whdatas.FirstOrDefault(x => x.WarehouseId == data.WarehouseId);
                            if (Creater != null || Approver != null)
                            {

                                string masteremail = ConfigurationManager.AppSettings["MasterEmail"];
                                string masterpassword = ConfigurationManager.AppSettings["MasterPassword"];
                                string body = "<div style='background: #FAFAFA; color: #333333; padding-left: 30px;font-family: arial,sans-serif; font-size: 14px;'>";
                                body += "<img style='padding-top: 10px;' src='http://shopkirana.com/wp-content/uploads/2015/07/ShopKirana-Logo11.png'><br/>";
                                body += "<h3 style='background-color: rgb(241, 89, 34);'>Alert! PO Pending more than 48hr</h3> ";
                                body += "Hello,";
                                body += "<br />";
                                body += "This is following detail of PO,";
                                body += "<br />";
                                body += "<br /> <table width='600'  border='1'><tr><th>Warehouse</th><th>City</th> <th>POID</th><th>Status</th> <th>Creation Date</th> </tr><tr><td> " + data.WarehouseName + " </td> <td>" + whdata.CityName + "</td> <td> " + data.PurchaseOrderId + "</td> <td> " + data.Status + "</td> <td> " + data.CreationDate + "</td> </tr></table>";
                                body += "<br />";
                                body += "Thanks,";
                                body += "<br />";
                                body += "<b>IT Team</b>";

                                body += "</div>";
                                var Createremail = Creater.Email;
                                var Approveremail = Approver.Email;
                                var Subj = "Alert! PO Pending";
                                var msg = new MailMessage("donotreply_backend@shopkirana.com", "donotreply_backend@shopkirana.com", Subj, body);
                                msg.To.Add(Createremail);
                                msg.To.Add(Approveremail);
                                msg.IsBodyHtml = true;
                                var smtpClient = new SmtpClient("smtp.gmail.com", 587);
                                smtpClient.UseDefaultCredentials = true;
                                smtpClient.Credentials = new NetworkCredential(masteremail, masterpassword);
                                smtpClient.EnableSsl = true;
                                smtpClient.Send(msg);
                            }
                        }
                        catch (Exception ex)
                        {

                        }


                    }

                }
                return true;
            }
            catch (Exception Ex)
            {

                return false;
            }

        }
        #endregion

        public int GetItemDemand(string itemNumber, int multimrpId, int wid)
        {
            int itemdemand = 0;
            CurrentStock csw;
            PurchaseList l = new PurchaseList();
            try
            {
                List<PurchaseList> uniquelist = new List<PurchaseList>();

                using (AuthContext context = new AuthContext())
                {
                    var poList = (from a in context.DbOrderDetails
                                  where a.Status == "Pending" && a.Deleted == false && a.WarehouseId == wid && a.itemNumber == itemNumber && a.ItemMultiMRPId == multimrpId
                                  join i in context.itemMasters on a.ItemId equals i.ItemId
                                  where i.WarehouseId == a.WarehouseId///mrp added
                                  select new PurchaseOrderList
                                  {
                                      OrderDetailsId = a.OrderDetailsId,
                                      CompanyId = a.CompanyId,
                                      WarehouseId = a.WarehouseId,
                                      WarehouseName = a.WarehouseName,
                                      OrderDate = a.OrderDate,
                                      SupplierId = i.SupplierId,
                                      PurchaseSku = i.PurchaseSku,
                                      SupplierName = i.SupplierName,
                                      OrderId = a.OrderId,
                                      ItemId = a.ItemId,
                                      SKUCode = i.Number,
                                      ItemName = a.itemname,
                                      PurchaseUnitName = i.PurchaseUnitName,
                                      Unit = i.SellingUnitName,
                                      Conversionfactor = i.PurchaseMinOrderQty,
                                      Discription = "",
                                      qty = a.qty,
                                      //CurrentInventory = c == null ? 0 : c.CurrentInventory,
                                      StoringItemName = i.StoringItemName,
                                      Price = a.price,
                                      // NetPurchasePrice = a.Purchaseprice, // netpurchaseprice add By raj
                                      NetPurchasePrice = Math.Round(i.NetPurchasePrice * (1 + (i.TotalTaxPercentage / 100)), 3),
                                      NetAmmount = a.NetAmmount,
                                      TaxPercentage = a.TaxPercentage,
                                      TaxAmount = a.TaxAmmount,
                                      TotalAmountIncTax = a.TotalAmt,
                                      PurchaseMinOrderQty = i.PurchaseMinOrderQty,
                                      Status = a.Status,
                                      CreationDate = a.CreatedDate,
                                      Deleted = a.Deleted,
                                      ItemMultiMRPId = a.ItemMultiMRPId,
                                      DepoId = i.DepoId,
                                      DepoName = i.DepoName
                                  }).ToList();
                    foreach (PurchaseOrderList item in poList)
                    {
                        int count = 0; //01AE101110                       
                        l = new PurchaseList();
                        l = uniquelist.Where(x => x.PurchaseSku == item.PurchaseSku && x.ItemMultiMRPId == item.ItemMultiMRPId).FirstOrDefault();

                        if (l == null)
                        {
                            count += 1;
                            l = new PurchaseList();
                            l.name = item.ItemName;
                            l.conversionfactor = item.Conversionfactor;
                            l.Supplier = item.SupplierName;
                            l.SupplierId = item.SupplierId;
                            l.WareHouseId = item.WarehouseId;
                            l.CompanyId = item.CompanyId;
                            l.WareHouseName = item.WarehouseName;
                            l.OrderDetailsId = item.OrderDetailsId;
                            l.itemNumber = item.SKUCode;
                            l.PurchaseSku = item.PurchaseSku;
                            l.orderIDs = item.OrderId + "," + l.orderIDs;
                            l.ItemId = item.ItemId;
                            l.ItemName = item.ItemName;
                            l.qty = l.qty + item.qty;
                            l.currentinventory = item.CurrentInventory;
                            l.Price = item.Price;
                            l.NetPurchasePrice = item.NetPurchasePrice;   // netpurchaseprice add By raj
                            l.ItemMultiMRPId = item.ItemMultiMRPId;//multimrp
                            l.DepoId = item.DepoId;
                            l.DepoName = item.DepoName;
                            uniquelist.Add(l);
                        }
                        else
                        {
                            l.qty = l.qty + item.qty;

                        }
                    }
                    csw = context.DbCurrentStock.Where(e => e.ItemNumber == itemNumber && e.ItemMultiMRPId == multimrpId && e.WarehouseId == wid).FirstOrDefault();
                    itemdemand = Convert.ToInt32(csw.CurrentInventory - l.qty);
                }
            }
            catch (Exception ex)
            {
                logger.Error("Error: " + ex.Message);
            }
            return itemdemand;
        }

        ///Deman report : 03/10/2019
        //[Route("getdemand")]
        //public IList<PurchaseList> GetDemand(int wid, DateTime? datefrom, DateTime? dateto)
        //{

        //    // Access claims
        //    var identity = User.Identity as ClaimsIdentity;
        //    int compid = 0, userid = 0;
        //    int Warehouse_id = 0;

        //    if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))
        //        compid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);

        //    if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
        //        userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

        //    if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "Warehouseid"))
        //        Warehouse_id = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "Warehouseid").Value);
        //    int CompanyId = compid;

        //    List<PurchaseList> qq = new List<PurchaseList>();
        //    List<PurchaseList> uniquelist = new List<PurchaseList>();
        //    Warehouse_id = wid;
        //    if (Warehouse_id > 0)//Warehouse based get data
        //    {

        //        using (var context = new AuthContext())
        //        {
        //            Warehouse Wsc = context.Warehouses.Where(q => q.WarehouseId == wid).FirstOrDefault();
        //            var CurrentStockList = context.DbCurrentStock.Where(k => k.Deleted == false && k.WarehouseId == Warehouse_id && k.ItemMultiMRPId > 0).ToList();
        //            if (datefrom != null)
        //            {
        //                List<Int32> OrderIds = context.DbOrderMaster.Where(x => x.Status == "Pending" && x.Deleted == false && x.WarehouseId == Warehouse_id && x.CreatedDate >= datefrom && x.CreatedDate <= dateto).Select(x => x.OrderId).ToList();
        //                var OrderDetails = context.DbOrderDetails.Where(x => OrderIds.Contains(x.OrderId)).ToList();
        //                var HubitemList = context.itemMasters.Where(m => m.WarehouseId == Warehouse_id && m.Deleted == false).ToList();

        //                var poList = (from a in OrderDetails
        //                              join i in HubitemList on a.ItemId equals i.ItemId
        //                              where i.WarehouseId == a.WarehouseId ///mrp added
        //                              select new PurchaseOrderList
        //                              {
        //                                  OrderDetailsId = a.OrderDetailsId,
        //                                  CompanyId = a.CompanyId,
        //                                  WarehouseId = a.WarehouseId,
        //                                  WarehouseName = a.WarehouseName,
        //                                  OrderDate = a.OrderDate,
        //                                  SupplierId = i.SupplierId,
        //                                  PurchaseSku = i.PurchaseSku,
        //                                  SupplierName = i.SupplierName,
        //                                  OrderId = a.OrderId,
        //                                  ItemId = a.ItemId,
        //                                  SKUCode = i.Number,
        //                                  ItemName = a.itemname,
        //                                  PurchaseUnitName = i.PurchaseUnitName,
        //                                  Unit = a.SellingUnitName,
        //                                  Conversionfactor = i.PurchaseMinOrderQty,
        //                                  Discription = "",
        //                                  qty = a.qty,
        //                                  StoringItemName = i.StoringItemName,
        //                                  Price = a.price,
        //                                  //NetPurchasePrice=a.Purchaseprice, // netpurchaseprice add By raj
        //                                  NetPurchasePrice = Math.Round(i.NetPurchasePrice * (1 + (i.TotalTaxPercentage / 100)), 3),
        //                                  NetAmmount = a.NetAmmount,
        //                                  TaxPercentage = a.TaxPercentage,
        //                                  TaxAmount = a.TaxAmmount,
        //                                  TotalAmountIncTax = a.TotalAmt,
        //                                  PurchaseMinOrderQty = i.PurchaseMinOrderQty,
        //                                  Status = a.Status,
        //                                  CreationDate = a.CreatedDate,
        //                                  Deleted = a.Deleted,
        //                                  ItemMultiMRPId = a.ItemMultiMRPId,
        //                                  DepoId = i.DepoId,
        //                                  DepoName = i.DepoName
        //                              }).ToList();
        //                List<string> ItemNumbers = poList.Select(x => x.SKUCode).Distinct().ToList();
        //                List<int> ItemMultiMRPIds = poList.Select(x => x.ItemMultiMRPId).Distinct().ToList();

        //                string sqlquery = "select b.CurrentInventory-sum(d.qty) CurrentInventory,d.ItemMultiMRPId,d.itemNumber from CurrentStocks b left join OrderDetails d "
        //                + " on d.itemNumber = b.ItemNumber and d.ItemMultiMRPId = b.ItemMultiMRPId and d.WarehouseId = b.WarehouseId and d.Status = 'Pending'"
        //                + " where d.itemNumber in ('" + string.Join("','", ItemNumbers) + "') and d.ItemMultiMRPId in(" + string.Join(",", ItemMultiMRPIds) + ") "
        //                + " and exists(select WarehouseId from Warehouses w where w.WarehouseId<> " + wid + " and w.Cityid= " + Wsc.Cityid + " and w.WarehouseId= d.WarehouseId) "
        //                + " group by d.ItemMultiMRPId,d.itemNumber,b.CurrentInventory ";

        //                List<ItemDemandDc> ItemDemandDcs = context.Database.SqlQuery<ItemDemandDc>(sqlquery).ToList();

        //                foreach (PurchaseOrderList item in poList)
        //                {
        //                    int count = 0; //01AE101110
        //                    PurchaseList l = uniquelist.Where(x => x.PurchaseSku == item.PurchaseSku && x.ItemMultiMRPId == item.ItemMultiMRPId).SingleOrDefault();

        //                    if (l == null)
        //                    {
        //                        count += 1;
        //                        l = new PurchaseList();
        //                        l.name = item.ItemName;
        //                        l.conversionfactor = item.Conversionfactor;
        //                        l.Supplier = item.SupplierName;
        //                        l.SupplierId = item.SupplierId;
        //                        l.WareHouseId = item.WarehouseId;
        //                        l.CompanyId = item.CompanyId;
        //                        l.WareHouseName = item.WarehouseName;
        //                        l.OrderDetailsId = item.OrderDetailsId;
        //                        l.itemNumber = item.SKUCode;
        //                        l.PurchaseSku = item.PurchaseSku;
        //                        l.orderIDs = item.OrderId + "," + l.orderIDs;
        //                        l.ItemId = item.ItemId;
        //                        l.ItemName = item.ItemName;
        //                        l.qty = l.qty + item.qty;
        //                        l.currentinventory = item.CurrentInventory;
        //                        l.Price = item.Price;
        //                        l.NetPurchasePrice = item.NetPurchasePrice; // netpurchaseprice add By raj
        //                        l.ItemMultiMRPId = item.ItemMultiMRPId;//multimrp
        //                        l.DepoId = item.DepoId;
        //                        l.DepoName = item.DepoName;
        //                        l.NetStock = ItemDemandDcs.Any(x => x.ItemMultiMRPId == l.ItemMultiMRPId && x.itemNumber == l.itemNumber) ? ItemDemandDcs.FirstOrDefault(x => x.ItemMultiMRPId == l.ItemMultiMRPId && x.itemNumber == l.itemNumber).CurrentInventory : 0;
        //                        uniquelist.Add(l);
        //                    }
        //                    else
        //                    {
        //                        l.orderIDs = item.OrderId + "," + l.orderIDs;
        //                        l.qty = l.qty + item.qty;
        //                        l.NetStock = ItemDemandDcs.Any(x => x.ItemMultiMRPId == l.ItemMultiMRPId && x.itemNumber == l.itemNumber) ? ItemDemandDcs.FirstOrDefault(x => x.ItemMultiMRPId == l.ItemMultiMRPId && x.itemNumber == l.itemNumber).CurrentInventory : 0;
        //                        uniquelist.First(d => d.PurchaseSku == item.PurchaseSku).NetStock = l.NetStock;
        //                        uniquelist.First(d => d.PurchaseSku == item.PurchaseSku).qty = l.qty;
        //                        uniquelist.First(d => d.PurchaseSku == item.PurchaseSku).orderIDs = l.orderIDs;
        //                    }
        //                }
        //            }
        //            else
        //            {
        //                List<Int32> OrderIds = context.DbOrderMaster.Where(x => x.Status == "Pending" && x.Deleted == false && x.WarehouseId == Warehouse_id).Select(x => x.OrderId).ToList();
        //                var OrderDetails = context.DbOrderDetails.Where(x => OrderIds.Contains(x.OrderId)).ToList();
        //                var HubitemList = context.itemMasters.Where(m => m.WarehouseId == Warehouse_id && m.Deleted == false).ToList();

        //                var poList = (from a in OrderDetails
        //                              join i in HubitemList on a.ItemId equals i.ItemId
        //                              where i.WarehouseId == a.WarehouseId///mrp added
        //                              select new PurchaseOrderList
        //                              {
        //                                  OrderDetailsId = a.OrderDetailsId,
        //                                  CompanyId = a.CompanyId,
        //                                  WarehouseId = a.WarehouseId,
        //                                  WarehouseName = a.WarehouseName,
        //                                  OrderDate = a.OrderDate,
        //                                  SupplierId = i.SupplierId,
        //                                  PurchaseSku = i.PurchaseSku,
        //                                  SupplierName = i.SupplierName,
        //                                  OrderId = a.OrderId,
        //                                  ItemId = a.ItemId,
        //                                  SKUCode = i.Number,
        //                                  ItemName = a.itemname,
        //                                  PurchaseUnitName = i.PurchaseUnitName,
        //                                  Unit = i.SellingUnitName,
        //                                  Conversionfactor = i.PurchaseMinOrderQty,
        //                                  Discription = "",
        //                                  qty = a.qty,
        //                                  //CurrentInventory = c == null ? 0 : c.CurrentInventory,
        //                                  StoringItemName = i.StoringItemName,
        //                                  Price = a.price,
        //                                  // NetPurchasePrice = a.Purchaseprice, // netpurchaseprice add By raj
        //                                  NetPurchasePrice = Math.Round(i.NetPurchasePrice * (1 + (i.TotalTaxPercentage / 100)), 3),
        //                                  NetAmmount = a.NetAmmount,
        //                                  TaxPercentage = a.TaxPercentage,
        //                                  TaxAmount = a.TaxAmmount,
        //                                  TotalAmountIncTax = a.TotalAmt,
        //                                  PurchaseMinOrderQty = i.PurchaseMinOrderQty,
        //                                  Status = a.Status,
        //                                  CreationDate = a.CreatedDate,
        //                                  Deleted = a.Deleted,
        //                                  ItemMultiMRPId = a.ItemMultiMRPId,
        //                                  DepoId = i.DepoId,
        //                                  DepoName = i.DepoName
        //                              }).ToList();


        //                List<string> ItemNumbers = poList.Select(x => x.SKUCode).Distinct().ToList();
        //                List<int> ItemMultiMRPIds = poList.Select(x => x.ItemMultiMRPId).Distinct().ToList();

        //                string sqlquery = "select b.CurrentInventory-sum(d.qty) CurrentInventory,d.ItemMultiMRPId,d.itemNumber from CurrentStocks b left join OrderDetails d "
        //                + " on d.itemNumber = b.ItemNumber and d.ItemMultiMRPId = b.ItemMultiMRPId and d.WarehouseId = b.WarehouseId and d.Status = 'Pending'"
        //                + " where d.itemNumber in ('" + string.Join("','", ItemNumbers) + "') and d.ItemMultiMRPId in(" + string.Join(",", ItemMultiMRPIds) + ") "
        //                + " and exists(select WarehouseId from Warehouses w where w.WarehouseId<> " + wid + " and w.Cityid= " + Wsc.Cityid + " and w.WarehouseId= d.WarehouseId) "
        //                + " group by d.ItemMultiMRPId,d.itemNumber,b.CurrentInventory ";

        //                List<ItemDemandDc> ItemDemandDcs = context.Database.SqlQuery<ItemDemandDc>(sqlquery).ToList();

        //                foreach (PurchaseOrderList item in poList)
        //                {
        //                    int count = 0; //01AE101110
        //                    PurchaseList l = uniquelist.Where(x => x.PurchaseSku == item.PurchaseSku && x.ItemMultiMRPId == item.ItemMultiMRPId).FirstOrDefault();
        //                    if (l == null)
        //                    {
        //                        count += 1;
        //                        l = new PurchaseList();
        //                        l.name = item.ItemName;
        //                        l.conversionfactor = item.Conversionfactor;
        //                        l.Supplier = item.SupplierName;
        //                        l.SupplierId = item.SupplierId;
        //                        l.WareHouseId = item.WarehouseId;
        //                        l.CompanyId = item.CompanyId;
        //                        l.WareHouseName = item.WarehouseName;
        //                        l.OrderDetailsId = item.OrderDetailsId;
        //                        l.itemNumber = item.SKUCode;
        //                        l.PurchaseSku = item.PurchaseSku;
        //                        l.orderIDs = item.OrderId + "," + l.orderIDs;
        //                        l.ItemId = item.ItemId;
        //                        l.ItemName = item.ItemName;
        //                        l.qty = l.qty + item.qty;
        //                        l.currentinventory = item.CurrentInventory;
        //                        l.Price = item.Price;
        //                        l.NetPurchasePrice = item.NetPurchasePrice; // netpurchaseprice add By raj
        //                        l.ItemMultiMRPId = item.ItemMultiMRPId;
        //                        l.DepoId = item.DepoId;
        //                        l.DepoName = item.DepoName;
        //                        l.NetStock = ItemDemandDcs.Any(x => x.ItemMultiMRPId == l.ItemMultiMRPId && x.itemNumber == l.itemNumber) ? ItemDemandDcs.FirstOrDefault(x => x.ItemMultiMRPId == l.ItemMultiMRPId && x.itemNumber == l.itemNumber).CurrentInventory : 0;
        //                        uniquelist.Add(l);
        //                    }
        //                    else
        //                    {
        //                        l.orderIDs = item.OrderId + "," + l.orderIDs;
        //                        l.qty = l.qty + item.qty;
        //                        l.NetStock = ItemDemandDcs.Any(x => x.ItemMultiMRPId == l.ItemMultiMRPId && x.itemNumber == l.itemNumber) ? ItemDemandDcs.FirstOrDefault(x => x.ItemMultiMRPId == l.ItemMultiMRPId && x.itemNumber == l.itemNumber).CurrentInventory : 0;
        //                        uniquelist.First(d => d.PurchaseSku == item.PurchaseSku).qty = l.qty;
        //                        uniquelist.First(d => d.PurchaseSku == item.PurchaseSku).orderIDs = l.orderIDs;
        //                    }
        //                }
        //            }


        //            List<CurrentStock> CurrentStockData = new List<CurrentStock>();
        //            foreach (var l in uniquelist)
        //            {
        //                CurrentStock cs = CurrentStockList.Where(k => k.ItemNumber == l.itemNumber && k.ItemMultiMRPId == l.ItemMultiMRPId).FirstOrDefault();//multi mrp
        //                if (cs != null)
        //                {
        //                    if (l.qty > cs.CurrentInventory)
        //                    {
        //                        l.safetystock = cs.SafetystockfQuantity;
        //                        l.currentinventory = l.currentinventory;
        //                        l.qty = Convert.ToInt32(l.qty) - cs.CurrentInventory;

        //                        //if (l.qty < 0)
        //                        //{
        //                        l.qty = l.qty + l.safetystock;
        //                        qq.Add(l);
        //                        // }
        //                    }
        //                }
        //            }

        //        }

        //    }
        //    return qq;
        //}

        [Route("isPoLockOrNot")]
        [HttpPut]
        public HttpResponseMessage isPoLockOrNot(IsPOLockDetail data)
        {
            try
            {
                using (AuthContext db = new AuthContext())
                {
                    PurchaseOrderMaster pm = db.DPurchaseOrderMaster.Where(a => a.PurchaseOrderId == data.PurchaseOrderId).SingleOrDefault();
                    pm.IsLock = data.Condition;
                    db.Entry(pm).State = EntityState.Modified;
                    db.Commit();
                }

                return Request.CreateResponse(HttpStatusCode.OK, "Ok");

            }
            catch (Exception Ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, "Failed" + Ex.Message);
            }
        }

        [Route("putGRitem")]
        [HttpPut]
        public HttpResponseMessage putGrItem(grPutItem data)
        {
            try
            {// Access claims
                var identity = User.Identity as ClaimsIdentity;
                int compid = 0, userid = 0;

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))
                    compid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                    userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);


                string sAllCharacter = data.GrNumber;
                char GRType = sAllCharacter[sAllCharacter.Length - 1];

                using (AuthContext db = new AuthContext())
                {
                    People p = db.Peoples.Where(a => a.PeopleID == userid).SingleOrDefault();
                    PurchaseOrderMaster pom = db.DPurchaseOrderMaster.Where(a => a.PurchaseOrderId == data.PurchaseOrderId).SingleOrDefault();
                    ItemMaster im = db.itemMasters.Where(a => a.ItemId == data.ItemId).SingleOrDefault();
                    ItemMasterCentral imc = db.ItemMasterCentralDB.Where(a => a.Number == im.Number).FirstOrDefault();
                    PurchaseOrderDetail pod = new PurchaseOrderDetail();
                    pod.CompanyId = 1;
                    pod.PurchaseOrderId = pom.PurchaseOrderId;
                    pod.SupplierId = pom.SupplierId;
                    pod.SupplierName = pom.SupplierName;
                    pod.WarehouseId = pom.WarehouseId;
                    pod.WarehouseName = pom.WarehouseName;
                    pod.SellingSku = im.SellingSku;
                    pod.ItemId = im.ItemId;
                    pod.HSNCode = im.HSNCode;
                    pod.ItemName = im.itemname;
                    pod.Price = im.PurchasePrice;
                    pod.MRP = im.MRP;
                    pod.MOQ = im.MinOrderQty;
                    pod.TotalQuantity = data.TotalQuantity;
                    pod.PurchaseSku = im.PurchaseSku;
                    pod.Status = "Close";
                    pod.CreationDate = indianTime;
                    pod.CreatedBy = p.DisplayName;
                    pod.ConversionFactor = im.MinOrderQty;
                    pod.PurchaseName = im.PurchaseUnitName;
                    pod.PurchaseQty = data.TotalQuantity;
                    pod.ItemMultiMRPId = im.ItemMultiMRPId;
                    pod.ItemNumber = im.Number;
                    pod.itemBaseName = im.itemBaseName;
                    db.DPurchaseOrderDeatil.Add(pod);
                    db.Commit();

                    PurchaseOrderDetailRecived por = new PurchaseOrderDetailRecived();
                    por.CompanyId = 1;
                    por.PurchaseOrderDetailId = pod.PurchaseOrderDetailId;
                    por.PurchaseOrderId = pom.PurchaseOrderId;
                    por.SupplierId = pom.SupplierId;
                    por.SupplierName = pom.SupplierName;
                    por.WarehouseId = pom.WarehouseId;
                    por.WarehouseName = pom.WarehouseName;
                    por.ItemId = im.ItemId;
                    por.SellingSku = im.SellingSku;
                    por.HSNCode = im.HSNCode;
                    por.PurchaseSku = im.PurchaseSku;
                    por.ItemName = im.itemname;
                    por.MRP = im.MRP;
                    por.Price = im.PurchasePrice;
                    por.MOQ = im.MinOrderQty;
                    por.TotalQuantity = data.TotalQuantity;
                    por.TotalTaxPercentage = imc.TotalTaxPercentage;
                    //por.Barcode = imc.Barcode;
                    por.PurchaseQty = data.TotalQuantity;
                    por.TotalAmountIncTax = Convert.ToDouble(im.PurchasePrice * data.GRQuantity);
                    por.Status = "Received";
                    por.CreationDate = indianTime;
                    por.CreatedBy = p.DisplayName;
                    por.ItemMultiMRPId = im.ItemMultiMRPId;
                    por.ItemNumber = im.Number;
                    por.itemBaseName = im.itemBaseName;
                    por.PriceRecived = Convert.ToDouble(im.PurchasePrice * data.GRQuantity);
                    por.ItemName1 = im.itemname;
                    por.ItemName2 = im.itemname;
                    por.ItemName3 = im.itemname;
                    por.ItemName4 = im.itemname;
                    por.ItemName5 = im.itemname;
                    por.ItemMultiMRPId1 = im.ItemMultiMRPId;
                    por.ItemMultiMRPId2 = im.ItemMultiMRPId;
                    por.ItemMultiMRPId3 = im.ItemMultiMRPId;
                    por.ItemMultiMRPId4 = im.ItemMultiMRPId;
                    por.ItemMultiMRPId5 = im.ItemMultiMRPId;

                    switch (GRType)
                    {
                        case 'A':
                            por.GRDate1 = indianTime;
                            por.QtyRecived1 = data.GRQuantity;
                            por.QtyRecived2 = 0;
                            por.QtyRecived3 = 0;
                            por.QtyRecived4 = 0;
                            por.QtyRecived5 = 0;
                            por.Price1 = im.PurchasePrice;
                            por.Price2 = 0;
                            por.Price3 = 0;
                            por.Price4 = 0;
                            por.Price5 = 0;
                            pom.Gr1_Amount += Convert.ToDouble(im.PurchasePrice * data.GRQuantity);

                            break;
                        case 'B':
                            por.GRDate2 = indianTime;
                            por.QtyRecived2 = data.GRQuantity;
                            por.QtyRecived3 = 0;
                            por.QtyRecived4 = 0;
                            por.QtyRecived5 = 0;
                            por.Price2 = im.PurchasePrice;
                            por.Price3 = 0;
                            por.Price4 = 0;
                            por.Price5 = 0;
                            por.ItemName2 = im.itemname;
                            por.ItemMultiMRPId2 = im.ItemMultiMRPId;
                            pom.Gr2_Amount += Convert.ToDouble(im.PurchasePrice * data.GRQuantity);
                            break;
                        case 'C':
                            por.GRDate3 = indianTime;
                            por.QtyRecived3 = data.GRQuantity;
                            por.QtyRecived4 = 0;
                            por.QtyRecived5 = 0;
                            por.Price3 = im.PurchasePrice;
                            por.Price4 = 0;
                            por.Price5 = 0;
                            por.ItemName3 = im.itemname;
                            por.ItemMultiMRPId3 = im.ItemMultiMRPId;
                            pom.Gr3_Amount += Convert.ToDouble(im.PurchasePrice * data.GRQuantity);
                            break;
                        case 'D':
                            por.GRDate4 = indianTime;
                            por.QtyRecived4 = data.GRQuantity;
                            por.QtyRecived5 = 0;
                            por.Price4 = im.PurchasePrice;
                            por.Price5 = 0;
                            por.ItemName4 = im.itemname;
                            por.ItemMultiMRPId4 = im.ItemMultiMRPId;
                            pom.Gr4_Amount += Convert.ToDouble(im.PurchasePrice * data.GRQuantity);
                            break;
                        case 'E':
                            por.GRDate5 = indianTime;
                            por.QtyRecived5 = data.GRQuantity;
                            por.Price5 = im.PurchasePrice;
                            por.ItemName5 = im.itemname;
                            por.ItemMultiMRPId5 = im.ItemMultiMRPId;
                            pom.Gr5_Amount += Convert.ToDouble(im.PurchasePrice * data.GRQuantity);
                            break;
                        default:
                            return null;
                    }

                    db.PurchaseOrderRecivedDetails.Add(por);
                    pom.TotalAmount += data.TotalQuantity * im.PurchasePrice;
                    db.Commit();
                }
                return Request.CreateResponse(HttpStatusCode.OK, "Ok");
            }
            catch (Exception Ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, "Failed" + Ex.Message);
            }
        }


        [Route("GetItemMrpList")]
        [HttpGet]
        public HttpResponseMessage GetItemMrpList(string number)
        {
            try
            {
                List<ItemMultiMRP> itemMultiMRP = new List<ItemMultiMRP>();
                using (AuthContext db = new AuthContext())
                {
                    itemMultiMRP = db.ItemMultiMRPDB.Where(a => a.ItemNumber == number).ToList();
                }

                return Request.CreateResponse(HttpStatusCode.OK, itemMultiMRP);

            }
            catch (Exception Ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, "Failed" + Ex.Message);
            }
        }

        public bool GenratHistory(PoEditHistory obj)
        {
            using (AuthContext db = new AuthContext())
            {
                PoEditHistory peh = new PoEditHistory();
                db.PoEditHistoryDB.Add(obj);
                db.Commit();
                return true;
            }
        }

        public string GenrateTransactionNumber()
        {
            int CurrentYear = DateTime.Today.Year;
            int PreviousYear = DateTime.Today.Year - 1;
            int NextYear = DateTime.Today.Year + 1;
            string PreYear = PreviousYear.ToString();
            string NexYear = NextYear.ToString();
            string CurYear = CurrentYear.ToString();
            string FinYear = null;

            if (DateTime.Today.Month > 3)
                FinYear = CurYear.Substring(2, CurYear.Length - 2) + "-" + NexYear.Substring(2, CurYear.Length - 2);
            else
                FinYear = PreYear.Substring(2, CurYear.Length - 2) + "-" + CurYear.Substring(2, CurYear.Length - 2);

            return FinYear = FinYear.Trim();
        }


        [Route("getdemand")]
        public HttpResponseMessage GetDemand(int wid, string datefrom, string dateto, int supplierid, int ItemMultiMRPId)
        {
            try
            {

                var identity = User.Identity as ClaimsIdentity;
                int userid = 0;

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                    userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
                //skip = (skip - 1) * take;
                using (AuthContext context = new AuthContext())
                {
                    var Warehouseparam = new SqlParameter
                    {
                        ParameterName = "WarehouseId",
                        Value = wid
                    };

                    var StartdateParam = new SqlParameter
                    {
                        ParameterName = "StartDate",
                        Value = datefrom == null ? DBNull.Value : (object)datefrom
                    };

                    var EndDateParam = new SqlParameter
                    {
                        ParameterName = "EndDate",
                        Value = dateto == null ? DBNull.Value : (object)dateto
                    };
                    var SupplierIdParam = new SqlParameter
                    {
                        ParameterName = "SupplierId",
                        Value = supplierid
                    };
                    var ItemmultimrpParam = new SqlParameter
                    {
                        ParameterName = "ItemMultiMRP",
                        Value = ItemMultiMRPId
                    };
                    var userIdParam = new SqlParameter
                    {
                        ParameterName = "@userId",
                        Value = userid
                    };

                    //var TakeParam = new SqlParameter
                    //{
                    //    ParameterName = "@Take",
                    //    Value = take
                    //};



                    List<DemandOrderDc> ObjDemandList = context.Database.SqlQuery<DemandOrderDc>("exec DemandOrder @WareHouseId,@StartDate,@EndDate,@SupplierId,@ItemMultiMRP,@userId", Warehouseparam, StartdateParam, EndDateParam, SupplierIdParam, ItemmultimrpParam, userIdParam).ToList();
                    foreach (var item in ObjDemandList)
                    {
                        item.color = item.IsUpdated == true ? "#22ff22" : "white";
                    }

                    return Request.CreateResponse(HttpStatusCode.OK, ObjDemandList);

                }

            }
            catch (Exception ex)
            {

                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex.GetBaseException().Message.ToString());

            }

        }


        [Route("UpdateYesterdayQty")]
        [HttpPost]
        public HttpResponseMessage UpdateYesterdayQty(YerterdayDemandDC demandDC)
        {
            int result = 0;
            try
            {
                var identity = User.Identity as ClaimsIdentity;
                int userid = 0;

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                    userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

                using (AuthContext context = new AuthContext())
                {
                    var yesDemand = context.YesterdayDemandForPRDB.Where(x => x.Id == demandDC.Id).FirstOrDefault();
                    if (yesDemand != null)
                    {
                        yesDemand.Qty = demandDC.Qty;
                        yesDemand.SupplierId = demandDC.SupplierId;
                        yesDemand.BuyerId = userid;
                        yesDemand.IsUpdated = true;
                        yesDemand.ModifiedDate = DateTime.Now;
                        yesDemand.MOQ = demandDC.MOQ;
                        yesDemand.NoofSet = demandDC.NoofSet;
                        yesDemand.Remark = demandDC.Remark;
                        yesDemand.ModifiedBy = userid;
                        context.Entry(yesDemand).State = EntityState.Modified;
                        result = context.Commit();
                    }

                    var resp = new
                    {
                        status = result > 0 ? true : false,
                        msg = result > 0 ? "Record updated." : "Record not update."
                    };
                    return Request.CreateResponse(HttpStatusCode.OK, resp);
                }
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex.GetBaseException().Message.ToString());
            }


        }

    }

    #region DTO

    public class grPutItem
    {
        public string GrNumber { get; set; }
        public int TotalQuantity { get; set; }
        public int GRQuantity { get; set; }
        public int ItemId { get; set; }
        public int PurchaseOrderId { get; set; }
    }


    public class YerterdayDemandDC
    {
        public int Id { get; set; }
        public int SupplierId { get; set; }
        public int Qty { get; set; }
        public int BuyerId { get; set; }
        public int NoofSet { get; set; }
        public int MOQ { get; set; }
        public string Remark { get; set; }
    }

    public class SendPdfDto2
    {
        public PurchaseOrderMaster p { get; set; }
        public List<PurchaseOrderDetail> pd { get; set; }
    }

    public class putbuyerIdPo
    {
        public int PurchaseOrderId { get; set; }
        public int PeopleID { get; set; }
        public string PickerType { get; set; }
        public double? FreightCharge { get; set; }
    }

    public class PurchaseList
    {
        public PurchaseList()
        {
        }
        public string name { get; set; }
        public int qty { get; set; }
        public string Supplier { get; set; }
        public int? SupplierId { get; set; }
        public int OrderDetailsId { get; set; }
        public int WareHouseId { get; set; }
        public int CompanyId { get; set; }
        public string WareHouseName { get; set; }
        public double Price { get; set; }
        public double conversionfactor { get; set; }
        public double PurchaseMinOrderQty { get; set; }
        public int currentinventory { get; set; }
        public int? ItemId { get; set; }
        public string itemNumber { get; set; }
        public string PurchaseSku { get; set; }
        public string ItemName { get; set; }
        public string orderMasterIDs { get; set; }
        public string orderIDs { get; set; }
        public double NetPurchasePrice { get; set; } // new Added By raj
        public int PurchaseOrderId { get; set; }
        [NotMapped]
        public int userid { get; set; }
        [NotMapped]
        public string reSendemail { get; set; }
        public double finalqty
        {
            get
            {
                return Math.Ceiling(qty / conversionfactor);
            }
        }

        //multimrp
        public int ItemMultiMRPId { get; set; }
        public int DepoId { get; set; }
        public string DepoName { get; set; }

        [NotMapped]
        public int NetStock { get; set; }
        [NotMapped]
        public int safetystock { get; set; }
        public string Category { get; set; }
        public double Weight { get; set; }
        public string WeightType { get; set; }
        public double? WeightInGram { get; set; }
    }
    public class TempPO
    {
        List<PurchaseList> _PurchaseList;
        [NotMapped]
        public int userid { get; set; }
        public TempPO()
        {
            _PurchaseList = new List<PurchaseList>();
        }
        public string SupplierName { get; set; }
        public int? SupplierId { get; set; }
        public double EtotalAmt { get; set; }
        public int WareHouseId { get; set; }
        public int CompanyId { get; set; }
        //by Anushka
        public int? DepoId { get; set; }
        public string DepoName { get; set; }
        public List<PurchaseList> Purchases
        {
            get
            {
                return _PurchaseList;
            }
            set { }
        }
    }
    public class POdata
    {
        public int ItemId
        {
            get; set;
        }
        public int Noofset
        {
            get; set;
        }
        public int PurchaseMinOrderQty
        {
            get; set;
        }
        public int SupplierId
        {
            get; set;
        }
        public double Advance_Amt { get; set; }
        public int? BuyerId { get; set; }
        public int WarehouseId { get; set; }
        //by Anushka
        public int? DepoId { get; set; }
        public string DepoName { get; set; }
        public bool IsCashPurchase { get; set; }
        public string CashPurchaseName { get; set; }
        public bool IsDraft { get; set; }
        public int PRStatus { get; set; }

        public int PRType { get; set; }
        public string Comment { get; set; }
        //by Ravindra
        public int ConvertPurchaseOrder { get; set; }
        public string PickerType { get; set; }
        public int SupplierCreditDay { get; set; }
        public string PRPaymentType { get; set; }
        public string Category { get; set; }
        public bool IsAdjustmentPo { get; set; }
        public List<PRPaymentDc> PRCloseDc { get; set; }
        public DateTime ETAdate { get; set; }
        public long YesDemandId { get; set; }
        public double? FreightCharge { get; set; }
        public double Weight { get; set; }
        public string WeightType { get; set; }
        public double? WeightInGram { get; set; }
        public string BusinessType { get; set; }
    }

    public class ETADC
    {
        public int WarehouseId { get; set; }
        public DateTime ETADate { get; set; }
    }


    public class ClonePR
    {
        public int PurchaseOrderId { get; set; }
    }


    public class BlankPODetails
    {
        public int ItemId
        {
            get; set;
        }
        public int OrderDetailsId { get; set; }
        public int MinOrderQty
        {
            get; set;
        }
        public int PurchaseOrderId { get; set; }

        public int finalqty { get; set; }
        public int qty { get; set; }

        public double Price { get; set; }

    }

    public class IsPOLockDetail
    {
        public int PurchaseOrderId { get; set; }
        public bool Condition { get; set; }
    }

    public class DemandOrderDc
    {
        public int totalCount { get; set; }
        public int BuyerId { get; set; }
        public int ItemMultiMRPId { get; set; }
        public string itemNumber { get; set; }
        public string ItemName { get; set; }
        public int conversionfactor { get; set; }
        public string Supplier { get; set; }

        public int SafetyStock { get; set; }

        //public int qty { get; set; }

        public int NetDemand { get; set; }

        public int? ItemId { get; set; }

        public string ABCClassification { get; set; }
        public int YesDemand { get; set; }
        public int YesQty { get; set; }

        public long YesDemandId { get; set; }
        public bool IsUpdated { get; set; }
        public string color { get; set; }

        public int SupplierId { get; set; }
        public string SupplierName { get; set; }
        public int openpoqty { get; set; }
        public bool check { get; set; }
        public int warehouseid { get; set; }
        public string WarehouseName { get; set; }

        public int currentinventory { get; set; }
        public double NetPurchasePrice { get; set; }
        public double POPurchasePrice { get; set; }
        public double price { get; set; }
        public int Type { get; set; }
        public int Categoryid { get; set; }
        public int NoofSet { get; set; }
        public int Cityid { get; set; }
        public string Remark { get; set; }
    }


    #endregion
}