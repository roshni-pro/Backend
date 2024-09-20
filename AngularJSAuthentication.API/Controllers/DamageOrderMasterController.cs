using AgileObjects.AgileMapper;
using AngularJSAuthentication.API.Helper;
using AngularJSAuthentication.API.Helper.IRN;
using AngularJSAuthentication.API.Managers;
using AngularJSAuthentication.BatchManager.Publishers;
using AngularJSAuthentication.BusinessLayer.Managers.Reports;
using AngularJSAuthentication.BusinessLayer.Managers.Transactions.BatchCode;
using AngularJSAuthentication.DataContracts.BatchCode;
using AngularJSAuthentication.DataContracts.constants;
using AngularJSAuthentication.DataContracts.Shared;
using AngularJSAuthentication.DataContracts.Transaction.Stocks;
using AngularJSAuthentication.Model;
using AngularJSAuthentication.Model.ClearTax;
using GenricEcommers.Models;
using Nito.AsyncEx;
using NLog;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Transactions;
using System.Web.Http;

namespace AngularJSAuthentication.API.Controllers
{
    [RoutePrefix("api/DamageOrderMaster")]
    public class DamageOrderMasterController : ApiController
    {
        private static TimeZoneInfo INDIAN_ZONE = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
        DateTime indianTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE);

        public static Logger logger = LogManager.GetCurrentClassLogger();

        [Route("")]
        public IEnumerable<DamageOrderMaster> Get()
        {
            logger.Info("start OrderMaster: ");
            using (AuthContext context = new AuthContext())
            {
                List<DamageOrderMaster> ass = new List<DamageOrderMaster>();
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

                    ass = context.AllDOrderMasters(compid).ToList();
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
        public PaggingData Get(int list, int page, int WarehouseId)
        {
            logger.Info("start OrderMaster: ");
            using (AuthContext context = new AuthContext())
            {
                //  List<OrderMaster> ass = new List<OrderMaster>();
                var identity = User.Identity as ClaimsIdentity;
                int compid = 0, userid = 0;

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))
                    compid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                    userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

                logger.Info("User ID : {0} , Company Id : {1}", compid, userid);
                //var ass = context.AllDamageOrderMaster(list, page, compid, WarehouseId);
                PaggingData obj = new PaggingData();

                List<DamageOrderMaster> newdata = new List<DamageOrderMaster>();

                var listOrders = context.DamageOrderMasterDB.Where(x => x.Deleted == false && x.CompanyId == compid && x.WarehouseId == WarehouseId).OrderByDescending(x => x.DamageOrderId).Skip((page - 1) * list).Take(list).Include("DamageorderDetails").ToList();
                newdata = listOrders;
                var wh = newdata.Select(x => x.WarehouseId).Distinct().ToList();
                var whname = context.Warehouses.Where(x => wh.Contains(x.WarehouseId)).ToList();
                var executive = newdata.Select(x => x.Skcode).Distinct().ToList();
                var executivename = context.Customers.Where(x => executive.Contains(x.Skcode)).ToList();
                foreach (var item in newdata)
                {
                    item.WarehouseName = whname.Where(x => x.WarehouseId == item.WarehouseId).Select(x => x.WarehouseName).FirstOrDefault();
                    //item.SalesPerson = executivename.Where(x => x.Skcode == item.Skcode).Select(x => x.ExecutiveName).FirstOrDefault();
                }
                obj.total_count = context.DamageOrderMasterDB.Where(x => x.Deleted == false && x.CompanyId == compid && x.WarehouseId == WarehouseId).Count();
                obj.ordermaster = newdata;
                return obj;

            }
        }

        // Added

        [Route("PostDamageOrder")]
        [HttpPost]
        public PaggingData PostDamageOrder(int list, int page, string WarehouseId, DateTime? start, DateTime? end, int OrderType)
        {
            logger.Info("start OrderMaster: ");
            using (AuthContext context = new AuthContext())
            {
                //  List<OrderMaster> ass = new List<OrderMaster>();

                var WarehouseIds = WarehouseId.Split(',').Select(Int32.Parse).ToList();

                var identity = User.Identity as ClaimsIdentity;
                int compid = 0, userid = 0;

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))
                    compid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                    userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

                logger.Info("User ID : {0} , Company Id : {1}", compid, userid);
                //var ass = context.AllDamageOrderMaster(list, page, compid, WarehouseId);
                PaggingData obj = new PaggingData();

                List<DamageOrderMaster> newdata = new List<DamageOrderMaster>();

                if (start.HasValue && end.HasValue)
                {
                    if (OrderType == 0)///OrderType =0 for Damage else NonSellable
                    {
                        var listOrders = context.DamageOrderMasterDB.Where(x => x.Deleted == false /*&& x.CompanyId == compid*/ && WarehouseIds.Contains(x.WarehouseId) && x.CreatedDate >= start.Value && x.CreatedDate <= end.Value && !x.isDamageOrder && x.OrderTypes == 0).OrderByDescending(x => x.DamageOrderId).Skip((page - 1) * list).Take(list).Include("DamageorderDetails").ToList();
                        foreach (var order in listOrders)
                        {
                            order.OrderType = "Damage Order";
                        }
                        newdata = listOrders;
                    }
                    else if (OrderType == 1)
                    {
                        var listOrders = context.DamageOrderMasterDB.Where(x => x.Deleted == false && x.CompanyId == compid && WarehouseIds.Contains(x.WarehouseId) && x.CreatedDate >= start.Value && x.CreatedDate <= end.Value && x.isDamageOrder && x.OrderTypes == 1).OrderByDescending(x => x.DamageOrderId).Skip((page - 1) * list).Take(list).Include("DamageorderDetails").ToList();
                        foreach (var order in listOrders)
                        {
                            order.OrderType = "NonSellable Order";
                        }
                        newdata = listOrders;
                    }
                    else if (OrderType == 2)
                    {
                        var listOrders = context.DamageOrderMasterDB.Where(x => x.Deleted == false && x.CompanyId == compid && WarehouseIds.Contains(x.WarehouseId) && x.CreatedDate >= start.Value && x.CreatedDate <= end.Value && x.OrderTypes == 2).OrderByDescending(x => x.DamageOrderId).Skip((page - 1) * list).Take(list).Include("DamageorderDetails").ToList();
                        foreach (var order in listOrders)
                        {
                            order.OrderType = "NonRevenue Order";
                        }
                        newdata = listOrders;
                    }
                    else
                    {
                        var listOrders = context.DamageOrderMasterDB.Where(x => x.Deleted == false && x.CompanyId == compid && WarehouseIds.Contains(x.WarehouseId) && x.CreatedDate >= start.Value && x.CreatedDate <= end.Value).OrderByDescending(x => x.DamageOrderId).Skip((page - 1) * list).Take(list).Include("DamageorderDetails").ToList();
                        foreach (var order in listOrders)
                        {
                            if (order.OrderTypes == 2)
                            {
                                order.OrderType = "NonRevenue Order";
                            }
                            else if (order.isDamageOrder == true)
                            {
                                order.OrderType = "NonSellable Order";
                            }
                            else
                            {
                                order.OrderType = "Damage Order";
                            }

                        }
                        newdata = listOrders;
                    }
                }
                else
                {
                    if (OrderType == 0)///OrderType =0 for Damage else NonSellable
                    {
                        var listOrders = context.DamageOrderMasterDB.Where(x => x.Deleted == false && x.CompanyId == compid && WarehouseIds.Contains(x.WarehouseId) && !x.isDamageOrder && x.OrderTypes == 0).OrderByDescending(x => x.DamageOrderId).Skip((page - 1) * list).Take(list).Include("DamageorderDetails").ToList();
                        foreach (var order in listOrders)
                        {
                            foreach (var orderdetail in order.DamageorderDetails)
                            {
                                var ItemMultiMRPIdParam = new SqlParameter("@ItemMultiMRPId", orderdetail.ItemMultiMRPId);
                                var WarehouseIdParam = new SqlParameter("@WarehouseId", orderdetail.WarehouseId);
                                var res = context.Database.SqlQuery<APPDC>("BatchCode.GetAPPByMultiMRPId @ItemMultiMRPId,@WarehouseId", ItemMultiMRPIdParam, WarehouseIdParam).FirstOrDefault();
                                order.UnitPrice = orderdetail.UnitPrice;
                                order.MRP = orderdetail.price;
                                order.DefaultUnitPrice = orderdetail.DefaultUnitPrice;
                                if (res != null)
                                {
                                    order.APP = res.APP;
                                }
                            }
                            order.OrderType = "Damage Order";
                        }
                        newdata = listOrders;
                    }
                    else if (OrderType == 1)
                    {
                        var listOrders = context.DamageOrderMasterDB.Where(x => x.Deleted == false && x.CompanyId == compid && WarehouseIds.Contains(x.WarehouseId) && x.isDamageOrder && x.OrderTypes == 1).OrderByDescending(x => x.DamageOrderId).Skip((page - 1) * list).Take(list).Include("DamageorderDetails").ToList();
                        foreach (var order in listOrders)
                        {
                            order.OrderType = "NonSellable Order";
                            foreach (var orderdetail in order.DamageorderDetails)
                            {
                                var ItemMultiMRPIdParam = new SqlParameter("@ItemMultiMRPId", orderdetail.ItemMultiMRPId);
                                var WarehouseIdParam = new SqlParameter("@WarehouseId", orderdetail.WarehouseId);
                                var res = context.Database.SqlQuery<APPDC>("BatchCode.GetAPPByMultiMRPId @ItemMultiMRPId,@WarehouseId", ItemMultiMRPIdParam, WarehouseIdParam).FirstOrDefault();
                                order.UnitPrice = orderdetail.UnitPrice;
                                order.MRP = orderdetail.price;
                                order.DefaultUnitPrice = orderdetail.DefaultUnitPrice;
                                if (res != null)
                                {
                                    order.APP = res.APP;
                                }
                            }
                        }
                        newdata = listOrders;
                    }
                    else if (OrderType == 2)
                    {
                        var listOrders = context.DamageOrderMasterDB.Where(x => x.Deleted == false && x.CompanyId == compid && WarehouseIds.Contains(x.WarehouseId) && x.OrderTypes == 2).OrderByDescending(x => x.DamageOrderId).Skip((page - 1) * list).Take(list).Include("DamageorderDetails").ToList();
                        foreach (var order in listOrders)
                        {
                            order.OrderType = "NonRevenue Order";
                            foreach (var orderdetail in order.DamageorderDetails)
                            {
                                var ItemMultiMRPIdParam = new SqlParameter("@ItemMultiMRPId", orderdetail.ItemMultiMRPId);
                                var WarehouseIdParam = new SqlParameter("@WarehouseId", orderdetail.WarehouseId);
                                var res = context.Database.SqlQuery<APPDC>("BatchCode.GetAPPByMultiMRPId @ItemMultiMRPId,@WarehouseId", ItemMultiMRPIdParam, WarehouseIdParam).FirstOrDefault();
                                order.UnitPrice = orderdetail.UnitPrice;
                                order.MRP = orderdetail.price;
                                order.DefaultUnitPrice = orderdetail.DefaultUnitPrice;
                                if (res != null)
                                {
                                    order.APP = res.APP;
                                }
                            }
                        }
                        newdata = listOrders;
                    }
                    else
                    {
                        var listOrders = context.DamageOrderMasterDB.Where(x => x.Deleted == false && x.CompanyId == compid && WarehouseIds.Contains(x.WarehouseId)).OrderByDescending(x => x.DamageOrderId).Skip((page - 1) * list).Take(list).Include("DamageorderDetails").ToList();
                        foreach (var order in listOrders)
                        {
                            //var orderMasterData = context.DbOrderMaster.Where(x => x.Deleted == false && x.WarehouseId == order.WarehouseId && x.OrderId == order.OrderId).FirstOrDefault();

                            //    if (orderMasterData != null)
                            //    {
                            //        order.Status = orderMasterData.Status;
                            //    }

                            if (order.OrderTypes == 2)
                            {
                                order.OrderType = "NonRevenue Order";
                                foreach (var orderdetail in order.DamageorderDetails)
                                {
                                    var ItemMultiMRPIdParam = new SqlParameter("@ItemMultiMRPId", orderdetail.ItemMultiMRPId);
                                    var WarehouseIdParam = new SqlParameter("@WarehouseId", orderdetail.WarehouseId);
                                    var res = context.Database.SqlQuery<APPDC>("BatchCode.GetAPPByMultiMRPId @ItemMultiMRPId,@WarehouseId", ItemMultiMRPIdParam, WarehouseIdParam).FirstOrDefault();
                                    order.UnitPrice = orderdetail.UnitPrice;
                                    order.MRP = orderdetail.price;
                                    order.DefaultUnitPrice = orderdetail.DefaultUnitPrice;
                                    if (res != null)
                                    {
                                        order.APP = res.APP;
                                    }
                                }
                            }
                            else if (order.isDamageOrder == true)
                            {
                                order.OrderType = "NonSellable Order";
                                foreach (var orderdetail in order.DamageorderDetails)
                                {
                                    var ItemMultiMRPIdParam = new SqlParameter("@ItemMultiMRPId", orderdetail.ItemMultiMRPId);
                                    var WarehouseIdParam = new SqlParameter("@WarehouseId", orderdetail.WarehouseId);
                                    var res = context.Database.SqlQuery<APPDC>("BatchCode.GetAPPByMultiMRPId @ItemMultiMRPId,@WarehouseId", ItemMultiMRPIdParam, WarehouseIdParam).FirstOrDefault();
                                    order.UnitPrice = orderdetail.UnitPrice;
                                    order.MRP = orderdetail.price;
                                    order.DefaultUnitPrice = orderdetail.DefaultUnitPrice;
                                    if (res != null)
                                    {
                                        order.APP = res.APP;
                                    }
                                }
                            }
                            else
                            {
                                order.OrderType = "Damage Order";
                                foreach (var orderdetail in order.DamageorderDetails)
                                {
                                    var ItemMultiMRPIdParam = new SqlParameter("@ItemMultiMRPId", orderdetail.ItemMultiMRPId);
                                    var WarehouseIdParam = new SqlParameter("@WarehouseId", orderdetail.WarehouseId);
                                    var res = context.Database.SqlQuery<APPDC>("BatchCode.GetAPPByMultiMRPId @ItemMultiMRPId,@WarehouseId", ItemMultiMRPIdParam, WarehouseIdParam).FirstOrDefault();
                                    order.UnitPrice = orderdetail.UnitPrice;
                                    order.MRP = orderdetail.price;
                                    order.DefaultUnitPrice = orderdetail.DefaultUnitPrice;
                                    if (res != null)
                                    {
                                        order.APP = res.APP;
                                    }
                                }
                            }
                        }
                        newdata = listOrders;
                    }

                }



                var wh = newdata.Select(x => x.WarehouseId).Distinct().ToList();
                var whname = context.Warehouses.Where(x => wh.Contains(x.WarehouseId)).ToList();
                var executive = newdata.Select(x => x.Skcode).Distinct().ToList();
                var InvoiceNumbers = newdata.Select(x => x.invoice_no).Distinct().ToList();
                var executivename = context.Customers.Where(x => executive.Contains(x.Skcode)).ToList();
                var orderDispatchedMaster = context.OrderDispatchedMasters.Where(x => InvoiceNumbers.Contains(x.invoice_no)).Select(x => new { x.OrderId, x.invoice_no }).ToList();
                foreach (var item in newdata)
                {
                    if (item.Status != "Pending" && item.Status != "Order Canceled")
                    {
                        var orderid = orderDispatchedMaster.Where(x => x.invoice_no == item.invoice_no).Select(x => x.OrderId).FirstOrDefault();
                        item.OrderId = orderid > 0 ? orderid : 0;
                        var orderMasterData = context.DbOrderMaster.Where(x => x.Deleted == false && x.WarehouseId == item.WarehouseId && x.OrderId == orderid).FirstOrDefault();
                        if (orderMasterData != null)
                        {
                            item.Status = orderMasterData.Status;
                        }
                    }
                    item.WarehouseName = whname.Where(x => x.WarehouseId == item.WarehouseId).Select(x => x.WarehouseName).FirstOrDefault();
                    //item.SalesPerson = executivename.Where(x => x.Skcode == item.Skcode).Select(x => x.ExecutiveName).FirstOrDefault();
                }
                if (OrderType == 0)///OrderType =0 for Damage else NonSellable
                {
                    if (start.HasValue && end.HasValue)
                    {
                        obj.total_count = context.DamageOrderMasterDB.Where(x => x.Deleted == false && x.CompanyId == compid && WarehouseIds.Contains(x.WarehouseId) && !x.isDamageOrder && x.CreatedDate >= start && x.CreatedDate <= end && x.OrderTypes == 0).Count();
                    }
                    else
                    {
                        obj.total_count = context.DamageOrderMasterDB.Where(x => x.Deleted == false && x.CompanyId == compid && WarehouseIds.Contains(x.WarehouseId) && !x.isDamageOrder && x.OrderTypes == 0).Count();
                    }
                }
                else if (OrderType == 1)
                {
                    if (start.HasValue && end.HasValue)
                    {
                        obj.total_count = context.DamageOrderMasterDB.Where(x => x.Deleted == false && x.CompanyId == compid && WarehouseIds.Contains(x.WarehouseId) && x.isDamageOrder && x.CreatedDate >= start && x.CreatedDate <= end && x.OrderTypes == 1).Count();
                    }
                    else
                    {
                        obj.total_count = context.DamageOrderMasterDB.Where(x => x.Deleted == false && x.CompanyId == compid && WarehouseIds.Contains(x.WarehouseId) && x.isDamageOrder && x.OrderTypes == 1).Count();
                    }
                }
                else if (OrderType == 2)
                {
                    if (start.HasValue && end.HasValue)
                    {
                        obj.total_count = context.DamageOrderMasterDB.Where(x => x.Deleted == false && x.CompanyId == compid && WarehouseIds.Contains(x.WarehouseId) && x.OrderTypes == 2 && x.CreatedDate >= start && x.CreatedDate <= end).Count();
                    }
                    else
                    {
                        obj.total_count = context.DamageOrderMasterDB.Where(x => x.Deleted == false && x.CompanyId == compid && WarehouseIds.Contains(x.WarehouseId) && x.OrderTypes == 2).Count();
                    }
                }
                else
                {
                    obj.total_count = context.DamageOrderMasterDB.Where(x => x.Deleted == false && x.CompanyId == compid && WarehouseIds.Contains(x.WarehouseId)).Count();
                }
                obj.ordermaster = newdata;
                return obj;

            }
        }



        [Route("PostDamageOrderbyStatus")]
        [HttpPost]
        public PaggingData PostDamageOrderbyStatus(int list, int page, string WarehouseId, string Orderstatus, DateTime? start, DateTime? end, int OrderType)
        {
            logger.Info("start OrderMaster: ");
            using (AuthContext context = new AuthContext())
            {
                //  List<OrderMaster> ass = new List<OrderMaster>();

                var WarehouseIds = WarehouseId.Split(',').Select(Int32.Parse).ToList();

                var identity = User.Identity as ClaimsIdentity;
                int compid = 0, userid = 0;

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))
                    compid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                    userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

                logger.Info("User ID : {0} , Company Id : {1}", compid, userid);
                //var ass = context.AllDamageOrderMaster(list, page, compid, WarehouseId);
                PaggingData obj = new PaggingData();

                List<DamageOrderMaster> newdata = new List<DamageOrderMaster>();

                if (start.HasValue && end.HasValue)
                {
                    if (OrderType == 0)///OrderType =0 for Damage else NonSellable
                    {
                        var listOrders = context.DamageOrderMasterDB.Where(x => x.Deleted == false && x.CompanyId == compid && x.Status == Orderstatus && WarehouseIds.Contains(x.WarehouseId) && x.CreatedDate >= start.Value && x.CreatedDate <= end.Value && !x.isDamageOrder && x.OrderTypes == 0).OrderByDescending(x => x.DamageOrderId).Skip((page - 1) * list).Take(list).Include("DamageorderDetails").ToList();
                        foreach (var order in listOrders)
                        {
                            order.OrderType = "Damage Order";
                            foreach (var orderdetail in order.DamageorderDetails)
                            {
                                var ItemMultiMRPIdParam = new SqlParameter("@ItemMultiMRPId", orderdetail.ItemMultiMRPId);
                                var WarehouseIdParam = new SqlParameter("@WarehouseId", orderdetail.WarehouseId);
                                var res = context.Database.SqlQuery<APPDC>("BatchCode.GetAPPByMultiMRPId @ItemMultiMRPId,@WarehouseId", ItemMultiMRPIdParam, WarehouseIdParam).FirstOrDefault();
                                order.UnitPrice = orderdetail.UnitPrice;
                                order.MRP = orderdetail.price;
                                order.DefaultUnitPrice = orderdetail.DefaultUnitPrice;
                                if (res != null)
                                {
                                    order.APP = res.APP;
                                }
                            }
                        }
                        newdata = listOrders;
                    }
                    else if (OrderType == 1)
                    {
                        var listOrders = context.DamageOrderMasterDB.Where(x => x.Deleted == false && x.CompanyId == compid && x.Status == Orderstatus && WarehouseIds.Contains(x.WarehouseId) && x.CreatedDate >= start.Value && x.CreatedDate <= end.Value && x.isDamageOrder && x.OrderTypes == 1).OrderByDescending(x => x.DamageOrderId).Skip((page - 1) * list).Take(list).Include("DamageorderDetails").ToList();
                        foreach (var order in listOrders)
                        {
                            order.OrderType = "NonSellable Order";
                            foreach (var orderdetail in order.DamageorderDetails)
                            {
                                var ItemMultiMRPIdParam = new SqlParameter("@ItemMultiMRPId", orderdetail.ItemMultiMRPId);
                                var WarehouseIdParam = new SqlParameter("@WarehouseId", orderdetail.WarehouseId);
                                var res = context.Database.SqlQuery<APPDC>("BatchCode.GetAPPByMultiMRPId @ItemMultiMRPId,@WarehouseId", ItemMultiMRPIdParam, WarehouseIdParam).FirstOrDefault();
                                order.UnitPrice = orderdetail.UnitPrice;
                                order.MRP = orderdetail.price;
                                order.DefaultUnitPrice = orderdetail.DefaultUnitPrice;
                                if (res != null)
                                {
                                    order.APP = res.APP;
                                }
                            }
                        }
                        newdata = listOrders;
                    }
                    else if (OrderType == 2) //ordertype=2  for Non Revenue Order
                    {
                        var listOrders = context.DamageOrderMasterDB.Where(x => x.Deleted == false && x.Status == Orderstatus && WarehouseIds.Contains(x.WarehouseId) && x.CreatedDate >= start.Value && x.CreatedDate <= end.Value && x.OrderTypes == 2).OrderByDescending(x => x.DamageOrderId).Skip((page - 1) * list).Take(list).Include("DamageorderDetails").ToList();
                        foreach (var order in listOrders)
                        {
                            order.OrderType = "NonRevenue Order";
                            foreach (var orderdetail in order.DamageorderDetails)
                            {
                                var ItemMultiMRPIdParam = new SqlParameter("@ItemMultiMRPId", orderdetail.ItemMultiMRPId);
                                var WarehouseIdParam = new SqlParameter("@WarehouseId", orderdetail.WarehouseId);
                                var res = context.Database.SqlQuery<APPDC>("BatchCode.GetAPPByMultiMRPId @ItemMultiMRPId,@WarehouseId", ItemMultiMRPIdParam, WarehouseIdParam).FirstOrDefault();
                                order.UnitPrice = orderdetail.UnitPrice;
                                order.MRP = orderdetail.price;
                                order.DefaultUnitPrice = orderdetail.DefaultUnitPrice;
                                if (res != null)
                                {
                                    order.APP = res.APP;
                                }
                            }
                        }
                        newdata = listOrders;
                    }
                    else
                    {
                        var listOrders = context.DamageOrderMasterDB.Where(x => x.Deleted == false && x.CompanyId == compid && x.Status == Orderstatus && WarehouseIds.Contains(x.WarehouseId) && x.CreatedDate >= start.Value && x.CreatedDate <= end.Value /*x.WarehouseId == WarehouseId*/).OrderByDescending(x => x.DamageOrderId).Skip((page - 1) * list).Take(list).Include("DamageorderDetails").ToList();
                        foreach (var order in listOrders)
                        {
                            if (order.isDamageOrder == true && order.OrderTypes != 2)
                            {
                                order.OrderType = "NonSellable Order";
                                foreach (var orderdetail in order.DamageorderDetails)
                                {
                                    var ItemMultiMRPIdParam = new SqlParameter("@ItemMultiMRPId", orderdetail.ItemMultiMRPId);
                                    var WarehouseIdParam = new SqlParameter("@WarehouseId", orderdetail.WarehouseId);
                                    var res = context.Database.SqlQuery<APPDC>("BatchCode.GetAPPByMultiMRPId @ItemMultiMRPId,@WarehouseId", ItemMultiMRPIdParam, WarehouseIdParam).FirstOrDefault();
                                    order.UnitPrice = orderdetail.UnitPrice;
                                    order.MRP = orderdetail.price;
                                    order.DefaultUnitPrice = orderdetail.DefaultUnitPrice;
                                    if (res != null)
                                    {
                                        order.APP = res.APP;
                                    }
                                }
                            }
                            else if (order.isDamageOrder == false && order.OrderTypes != 2)
                            {
                                order.OrderType = "Damage Order";
                                foreach (var orderdetail in order.DamageorderDetails)
                                {
                                    var ItemMultiMRPIdParam = new SqlParameter("@ItemMultiMRPId", orderdetail.ItemMultiMRPId);
                                    var WarehouseIdParam = new SqlParameter("@WarehouseId", orderdetail.WarehouseId);
                                    var res = context.Database.SqlQuery<APPDC>("BatchCode.GetAPPByMultiMRPId @ItemMultiMRPId,@WarehouseId", ItemMultiMRPIdParam, WarehouseIdParam).FirstOrDefault();
                                    order.UnitPrice = orderdetail.UnitPrice;
                                    order.MRP = orderdetail.price;
                                    order.DefaultUnitPrice = orderdetail.DefaultUnitPrice;
                                    if (res != null)
                                    {
                                        order.APP = res.APP;
                                    }
                                }
                            }
                            else
                            {
                                if (order.OrderTypes == 2)
                                {
                                    order.OrderType = "NonRevenue Order";
                                    foreach (var orderdetail in order.DamageorderDetails)
                                    {
                                        var ItemMultiMRPIdParam = new SqlParameter("@ItemMultiMRPId", orderdetail.ItemMultiMRPId);
                                        var WarehouseIdParam = new SqlParameter("@WarehouseId", orderdetail.WarehouseId);
                                        var res = context.Database.SqlQuery<APPDC>("BatchCode.GetAPPByMultiMRPId @ItemMultiMRPId,@WarehouseId", ItemMultiMRPIdParam, WarehouseIdParam).FirstOrDefault();
                                        order.UnitPrice = orderdetail.UnitPrice;
                                        order.MRP = orderdetail.price;
                                        order.DefaultUnitPrice = orderdetail.DefaultUnitPrice;
                                        if (res != null)
                                        {
                                            order.APP = res.APP;
                                        }
                                    }
                                }
                            }


                        }
                        newdata = listOrders;
                    }
                }
                else
                {
                    if (OrderType == 0)///OrderType =0 for Damage else NonSellable
                    {
                        var listOrders = context.DamageOrderMasterDB.Where(x => x.Deleted == false && x.CompanyId == compid && x.Status == Orderstatus && WarehouseIds.Contains(x.WarehouseId) && !x.isDamageOrder && x.OrderTypes == 0).OrderByDescending(x => x.DamageOrderId).Skip((page - 1) * list).Take(list).Include("DamageorderDetails").ToList();
                        foreach (var order in listOrders)
                        {
                            order.OrderType = "Damage Order";
                            foreach (var orderdetail in order.DamageorderDetails)
                            {
                                var ItemMultiMRPIdParam = new SqlParameter("@ItemMultiMRPId", orderdetail.ItemMultiMRPId);
                                var WarehouseIdParam = new SqlParameter("@WarehouseId", orderdetail.WarehouseId);
                                var res = context.Database.SqlQuery<APPDC>("BatchCode.GetAPPByMultiMRPId @ItemMultiMRPId,@WarehouseId", ItemMultiMRPIdParam, WarehouseIdParam).FirstOrDefault();
                                order.UnitPrice = orderdetail.UnitPrice;
                                order.MRP = orderdetail.price;
                                order.DefaultUnitPrice = orderdetail.DefaultUnitPrice;
                                if (res != null)
                                {
                                    order.APP = res.APP;
                                }
                            }
                        }
                        newdata = listOrders;
                    }
                    else if (OrderType == 1)
                    {
                        var listOrders = context.DamageOrderMasterDB.Where(x => x.Deleted == false && x.CompanyId == compid && x.Status == Orderstatus && WarehouseIds.Contains(x.WarehouseId) && x.isDamageOrder && x.OrderTypes == 1).OrderByDescending(x => x.DamageOrderId).Skip((page - 1) * list).Take(list).Include("DamageorderDetails").ToList();
                        foreach (var order in listOrders)
                        {
                            order.OrderType = "NonSellable Order";
                            foreach (var orderdetail in order.DamageorderDetails)
                            {
                                var ItemMultiMRPIdParam = new SqlParameter("@ItemMultiMRPId", orderdetail.ItemMultiMRPId);
                                var WarehouseIdParam = new SqlParameter("@WarehouseId", orderdetail.WarehouseId);
                                var res = context.Database.SqlQuery<APPDC>("BatchCode.GetAPPByMultiMRPId @ItemMultiMRPId,@WarehouseId", ItemMultiMRPIdParam, WarehouseIdParam).FirstOrDefault();
                                order.UnitPrice = orderdetail.UnitPrice;
                                order.MRP = orderdetail.price;
                                order.DefaultUnitPrice = orderdetail.DefaultUnitPrice;
                                if (res != null)
                                {
                                    order.APP = res.APP;
                                }
                            }
                        }
                        newdata = listOrders;
                    }
                    else if (OrderType == 2)
                    {
                        var listOrders = context.DamageOrderMasterDB.Where(x => x.Deleted == false && x.CompanyId == compid && x.Status == Orderstatus && WarehouseIds.Contains(x.WarehouseId) && x.OrderTypes == 2).OrderByDescending(x => x.DamageOrderId).Skip((page - 1) * list).Take(list).Include("DamageorderDetails").ToList();
                        foreach (var order in listOrders)
                        {
                            order.OrderType = "NonRevenue Order";
                            foreach (var orderdetail in order.DamageorderDetails)
                            {
                                var ItemMultiMRPIdParam = new SqlParameter("@ItemMultiMRPId", orderdetail.ItemMultiMRPId);
                                var WarehouseIdParam = new SqlParameter("@WarehouseId", orderdetail.WarehouseId);
                                var res = context.Database.SqlQuery<APPDC>("BatchCode.GetAPPByMultiMRPId @ItemMultiMRPId,@WarehouseId", ItemMultiMRPIdParam, WarehouseIdParam).FirstOrDefault();
                                order.UnitPrice = orderdetail.UnitPrice;
                                order.MRP = orderdetail.price;
                                order.DefaultUnitPrice = orderdetail.DefaultUnitPrice;
                                if (res != null)
                                {
                                    order.APP = res.APP;
                                }
                            }
                        }
                        newdata = listOrders;
                    }
                    else
                    {
                        var listOrders = context.DamageOrderMasterDB.Where(x => x.Deleted == false && x.CompanyId == compid && x.Status == Orderstatus && WarehouseIds.Contains(x.WarehouseId) /*x.WarehouseId == WarehouseId*/).OrderByDescending(x => x.DamageOrderId).Skip((page - 1) * list).Take(list).Include("DamageorderDetails").ToList();
                        foreach (var order in listOrders)
                        {
                            if (order.isDamageOrder == true && order.OrderTypes != 2)
                            {
                                order.OrderType = "NonSellable Order";
                                foreach (var orderdetail in order.DamageorderDetails)
                                {
                                    var ItemMultiMRPIdParam = new SqlParameter("@ItemMultiMRPId", orderdetail.ItemMultiMRPId);
                                    var WarehouseIdParam = new SqlParameter("@WarehouseId", orderdetail.WarehouseId);
                                    var res = context.Database.SqlQuery<APPDC>("BatchCode.GetAPPByMultiMRPId @ItemMultiMRPId,@WarehouseId", ItemMultiMRPIdParam, WarehouseIdParam).FirstOrDefault();
                                    order.UnitPrice = orderdetail.UnitPrice;
                                    order.MRP = orderdetail.price;
                                    order.DefaultUnitPrice = orderdetail.DefaultUnitPrice;
                                    if (res != null)
                                    {
                                        order.APP = res.APP;
                                    }
                                }
                            }
                            else if (order.isDamageOrder == false && order.OrderTypes != 2)
                            {
                                order.OrderType = "Damage Order";
                                foreach (var orderdetail in order.DamageorderDetails)
                                {
                                    var ItemMultiMRPIdParam = new SqlParameter("@ItemMultiMRPId", orderdetail.ItemMultiMRPId);
                                    var WarehouseIdParam = new SqlParameter("@WarehouseId", orderdetail.WarehouseId);
                                    var res = context.Database.SqlQuery<APPDC>("BatchCode.GetAPPByMultiMRPId @ItemMultiMRPId,@WarehouseId", ItemMultiMRPIdParam, WarehouseIdParam).FirstOrDefault();
                                    order.UnitPrice = orderdetail.UnitPrice;
                                    order.MRP = orderdetail.price;
                                    order.DefaultUnitPrice = orderdetail.DefaultUnitPrice;
                                    if (res != null)
                                    {
                                        order.APP = res.APP;
                                    }
                                }
                            }
                            else
                            {
                                if (order.OrderTypes == 2)
                                {
                                    order.OrderType = "NonRevenue Order";
                                    foreach (var orderdetail in order.DamageorderDetails)
                                    {
                                        var ItemMultiMRPIdParam = new SqlParameter("@ItemMultiMRPId", orderdetail.ItemMultiMRPId);
                                        var WarehouseIdParam = new SqlParameter("@WarehouseId", orderdetail.WarehouseId);
                                        var res = context.Database.SqlQuery<APPDC>("BatchCode.GetAPPByMultiMRPId @ItemMultiMRPId,@WarehouseId", ItemMultiMRPIdParam, WarehouseIdParam).FirstOrDefault();
                                        order.UnitPrice = orderdetail.UnitPrice;
                                        order.MRP = orderdetail.price;
                                        order.DefaultUnitPrice = orderdetail.DefaultUnitPrice;
                                        if (res != null)
                                        {
                                            order.APP = res.APP;
                                        }
                                    }
                                }
                            }
                        }
                        newdata = listOrders;
                    }
                }

                var wh = newdata.Select(x => x.WarehouseId).Distinct().ToList();
                var whname = context.Warehouses.Where(x => wh.Contains(x.WarehouseId)).ToList();
                var executive = newdata.Select(x => x.Skcode).Distinct().ToList();
                var executivename = context.Customers.Where(x => executive.Contains(x.Skcode)).ToList();
                foreach (var item in newdata)
                {
                    if (item.Status != "Pending" && item.Status != "Order Canceled" && item.Status != "Ready to Dispatch")
                    {
                        var orderid = context.OrderDispatchedMasters.Where(x => x.invoice_no == item.invoice_no).Select(x => x.OrderId).FirstOrDefault();
                        item.OrderId = orderid > 0 ? orderid : 0;
                        var orderMasterData = context.DbOrderMaster.Where(x => x.Deleted == false && x.WarehouseId == item.WarehouseId && x.OrderId == orderid).FirstOrDefault();
                        if (orderMasterData != null)
                        {
                            item.OrderId = orderMasterData.OrderId;
                            item.Status = orderMasterData.Status;
                        }
                    }
                    item.WarehouseName = whname.Where(x => x.WarehouseId == item.WarehouseId).Select(x => x.WarehouseName).FirstOrDefault();
                    //item.SalesPerson = executivename.Where(x => x.Skcode == item.Skcode).Select(x => x.ExecutiveName).FirstOrDefault();
                }
                if (OrderType == 0)///OrderType =0 for Damage else NonSellable
                {
                    if (start.HasValue && end.HasValue)
                    {
                        obj.total_count = context.DamageOrderMasterDB.Where(x => x.Deleted == false && x.CompanyId == compid && x.Status == Orderstatus && WarehouseIds.Contains(x.WarehouseId) && !x.isDamageOrder && x.CreatedDate >= start && x.CreatedDate <= end && x.OrderTypes == 0).Count();
                    }
                    else
                    {
                        obj.total_count = context.DamageOrderMasterDB.Where(x => x.Deleted == false && x.CompanyId == compid && x.Status == Orderstatus && WarehouseIds.Contains(x.WarehouseId) && !x.isDamageOrder && x.OrderTypes == 0).Count();
                    }
                }
                else if (OrderType == 1)
                {
                    if (start.HasValue && end.HasValue)
                    {
                        obj.total_count = context.DamageOrderMasterDB.Where(x => x.Deleted == false && x.CompanyId == compid && x.Status == Orderstatus && WarehouseIds.Contains(x.WarehouseId) && x.isDamageOrder && x.CreatedDate >= start && x.CreatedDate <= end && x.OrderTypes == 1).Count();
                    }
                    else
                    {
                        obj.total_count = context.DamageOrderMasterDB.Where(x => x.Deleted == false && x.CompanyId == compid && x.Status == Orderstatus && WarehouseIds.Contains(x.WarehouseId) && x.isDamageOrder && x.OrderTypes == 1).Count();
                    }
                }
                else if (OrderType == 2)
                {
                    if (start.HasValue && end.HasValue)
                    {
                        obj.total_count = context.DamageOrderMasterDB.Where(x => x.Deleted == false && x.CompanyId == compid && x.Status == Orderstatus && WarehouseIds.Contains(x.WarehouseId) && x.OrderTypes == 2 && x.CreatedDate >= start && x.CreatedDate <= end).Count();
                    }
                    else
                    {
                        obj.total_count = context.DamageOrderMasterDB.Where(x => x.Deleted == false && x.CompanyId == compid && x.Status == Orderstatus && WarehouseIds.Contains(x.WarehouseId) && x.OrderTypes == 2).Count();
                    }
                }
                else
                {
                    obj.total_count = context.DamageOrderMasterDB.Where(x => x.Deleted == false && x.Status == Orderstatus && x.CompanyId == compid && WarehouseIds.Contains(x.WarehouseId)).Count();

                }
                obj.ordermaster = newdata;
                return obj;

            }
        }




        [Route("")]
        public async System.Threading.Tasks.Task<DamageOrderMaster> GetAsync(int id, int OrderType)
        {
            logger.Info("start OrderMaster: ");
            using (AuthContext context = new AuthContext())
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
                    DamageOrderMaster orders = new DamageOrderMaster();
                    //var manager = new ItemLedgerManager();
                    //List<ItemClassificationDC> objItemClassificationDClist = new List<ItemClassificationDC>();
                    if (OrderType == 1)
                    {
                        orders = context.DamageOrderMasterDB.Where(x => x.DamageOrderId == id && x.CompanyId == compid && x.isDamageOrder == true).Include("DamageorderDetails").SingleOrDefault();
                    }
                    else if (OrderType == 0)
                    {
                        orders = context.DamageOrderMasterDB.Where(x => x.DamageOrderId == id && x.CompanyId == compid && x.isDamageOrder == false).Include("DamageorderDetails").SingleOrDefault();
                    }
                    else
                    {
                        orders = context.DamageOrderMasterDB.Where(x => x.DamageOrderId == id && x.CompanyId == compid).Include("DamageorderDetails").SingleOrDefault();
                    }

                    var ass = GetDOrderMaster(id, compid);
                    //foreach (var data in orders.DamageorderDetails)
                    //{
                    //    ItemClassificationDC obj = new ItemClassificationDC();
                    //    obj.WarehouseId = data.WarehouseId;
                    //    obj.ItemNumber = data.itemNumber;
                    //    objItemClassificationDClist.Add(obj);
                    //}

                    //List<ItemClassificationDC> _objItemClassificationDClist = await manager.GetItemClassificationsAsync(objItemClassificationDClist);
                    var StockBatchMasterIds = orders.DamageorderDetails.Select(x => x.StockBatchMasterId).Distinct().ToList();
                    StockBatchTransactionManager stockBatchTransactionManager = new StockBatchTransactionManager();
                    var StockBatchMasterlist = await stockBatchTransactionManager.GetAllStockBatchMastersDataById(StockBatchMasterIds, "D");
                    orders.DamageorderDetails = orders.DamageorderDetails.Select(z => new DamageOrderDetails
                    {
                        DamageOrderDetailsId = z.DamageOrderDetailsId,
                        CompanyId = z.CompanyId,
                        DamageOrderId = z.DamageOrderId,
                        CustomerId = z.CustomerId,
                        CustomerName = z.CustomerName,
                        City = z.City,
                        Mobile = z.Mobile,
                        OrderDate = z.OrderDate,

                        CityId = z.CityId,
                        WarehouseId = z.WarehouseId,
                        WarehouseName = z.WarehouseName,
                        CategoryName = z.CategoryName,

                        SellingSku = z.SellingSku,
                        ItemId = z.ItemId,
                        Itempic = z.Itempic,
                        itemname = z.itemname,

                        itemcode = z.itemcode,
                        itemNumber = z.itemNumber,

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

                        TotalAmt = z.TotalAmt,


                        CreatedDate = z.CreatedDate,
                        UpdatedDate = z.UpdatedDate,
                        Deleted = z.Deleted,
                        Status = z.Status,
                        SizePerUnit = z.SizePerUnit,
                        marginPoint = z.marginPoint,
                        promoPoint = z.promoPoint,
                        NetPurchasePrice = z.NetPurchasePrice,
                        ItemMultiMRPId = z.ItemMultiMRPId,

                        ABCClassification = z.ABCClassification,
                        StockBatchCode = StockBatchMasterlist.FirstOrDefault(x => x.StockBatchMasterId == z.StockBatchMasterId) != null ? StockBatchMasterlist.FirstOrDefault(x => x.StockBatchMasterId == z.StockBatchMasterId).BatchCode : ""
                    }).ToList();


                    logger.Info("End OrderMaster: ");
                    return orders;
                }
                catch (Exception ex)
                {
                    logger.Error("Error in OrderMaster " + ex.Message);
                    logger.Info("End  OrderMaster: ");
                    return null;
                }
            }
        }


        private DamageOrderMaster GetDOrderMaster(int orderid, int compid)
        {
            using (AuthContext context = new AuthContext())
            {
                var orders = context.DamageOrderMasterDB.Where(x => x.DamageOrderId == orderid && x.CompanyId == compid).Include("DamageorderDetails").SingleOrDefault();
                foreach (var data in orders.DamageorderDetails)
                {

                }
                return orders;
            }

        }

        #region Get Damage Order Id based current inventory
        /// <summary>
        /// Created by raj
        /// Created Date 15/04/2020
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Route("GetStock")]
        [HttpGet]
        public List<DamagestockforOrderDC> GetStock(int id)
        {
            using (AuthContext context = new AuthContext())
            {
                DamageOrderMaster damageOrderMasterData = context.DamageOrderMasterDB.Where(x => x.DamageOrderId == id).FirstOrDefault();
                List<DamagestockforOrderDC> damagestockforOrderDCs = new List<DamagestockforOrderDC>();
                var damageOrderDetails = context.DamageOrderDetailsDB.Where(x => x.DamageOrderId == id).Select(x => new
                {
                    x.DamageOrderDetailsId,
                    x.itemNumber,
                    x.WarehouseId,
                    x.ItemMultiMRPId,
                    x.StockBatchMasterId
                }).ToList();
                foreach (var stock in damageOrderDetails)
                {
                    DamagestockforOrderDC damagestock = new DamagestockforOrderDC();
                    damagestock.DamageOrderDetailsId = stock.DamageOrderDetailsId;
                    if (damageOrderMasterData.OrderTypes == 2)
                    {
                        damagestock.Damagestock = context.NonRevenueOrderStocks.
                        Where(x => x.WarehouseId == stock.WarehouseId && x.ItemMultiMRPId == stock.ItemMultiMRPId)
                        .Select(x => x.NonRevenueInventory).FirstOrDefault();
                        damagestock.CurrentBatchStock = context.StockBatchMasters.Where(y => y.Id == stock.StockBatchMasterId && y.IsActive == true && y.IsDeleted == false).Select(y => y.Qty).FirstOrDefault();
                    }
                    else if (damageOrderMasterData.isDamageOrder == true)
                    {
                        damagestock.Damagestock = context.NonSellableStockDB.
                        Where(x => x.WarehouseId == stock.WarehouseId && x.ItemMultiMRPId == stock.ItemMultiMRPId)
                        .Select(x => x.Inventory).FirstOrDefault();
                        damagestock.CurrentBatchStock = context.StockBatchMasters.Where(y => y.Id == stock.StockBatchMasterId && y.IsActive == true && y.IsDeleted == false).Select(y => y.Qty).FirstOrDefault();
                    }
                    else
                    {
                        damagestock.Damagestock = context.DamageStockDB.
                        Where(x => x.WarehouseId == stock.WarehouseId && x.ItemNumber == stock.itemNumber && x.ItemMultiMRPId == stock.ItemMultiMRPId)
                        .Select(x => x.DamageInventory).FirstOrDefault();
                        damagestock.CurrentBatchStock = context.StockBatchMasters.Where(y => y.Id == stock.StockBatchMasterId && y.IsActive == true && y.IsDeleted == false).Select(y => y.Qty).FirstOrDefault();
                    }
                    damagestockforOrderDCs.Add(damagestock);
                }


                return damagestockforOrderDCs;
            }
        }
        #endregion
        #region For Deliverd   Damage Order 
        /// <summary>
        /// Created by raj
        /// Created Date 16/04/2020
        /// </summary>
        /// <param name="damage"></param>
        /// <returns></returns>
        [Route("DeliverdDamage")]
        [HttpPut]
        public DamageOrderResp deliverdDamageOrder(DamageOrderMaster damage)
        {

            var result = new DamageOrderResp();


            var identity = User.Identity as ClaimsIdentity;
            int compid = 0, userid = 0;

            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))
                compid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);

            int companyid = compid;
            if (damage != null && userid > 0)
            {
                double finaltotal = 0;
                double finalTaxAmount = 0;
                double finalGrossAmount = 0;
                double TotalAmount = 0;
                double finalTotalTaxAmount = 0;
                int Customerid = damage.CustomerId;
                int warehouseid = damage.WarehouseId;
                Warehouse Warehouse = null;
                People user = null;
                List<int> itemids = null;
                List<ItemMaster> itemslist = null;
                using (var context = new AuthContext())
                {
                    var IsGstRequestPending = context.Database.SqlQuery<bool>("Exec CheckGSTRequestPending " + Customerid).FirstOrDefault();
                    if (IsGstRequestPending)
                    {
                        result.Status = false;
                        result.Message = "Customer GST Reqest is Inprogress. Please coordinate with customer care.";
                        return result;
                    }


                    var warehouse = context.Warehouses.FirstOrDefault(x => x.WarehouseId == warehouseid);

                    if (warehouse.IsStopCurrentStockTrans)
                    {
                        result.Status = false;
                        result.Message = "Inventory Transactions are currently disabled for this warehouse... Please try after some time";
                        return result;

                    }
                    var damageOrderStatus = context.DamageOrderMasterDB.Where(x => x.DamageOrderId == damage.DamageOrderId).Select(x => x.Status).FirstOrDefault();
                    if ((damageOrderStatus == "Pending") && damage != null && damage.DamageorderDetails.Sum(x => x.qty) > 0) //if order in Pending status
                    {
                        user = context.Peoples.FirstOrDefault(x => x.PeopleID == userid && x.Active);
                        Warehouse = context.Warehouses.FirstOrDefault(x => x.WarehouseId == warehouseid);
                        itemids = damage.DamageorderDetails.Select(x => x.ItemId).ToList();
                        itemslist = context.itemMasters.Where(x => itemids.Contains(x.ItemId)).ToList();
                    }
                    else
                    {
                        result.Status = false;
                        result.Message = "Order not found in Pending status";
                        return result;
                    }


                }
                DamageOrderMaster resultobjOrderMaster = new DamageOrderMaster();
                TransactionOptions option = new TransactionOptions();
                option.IsolationLevel = System.Transactions.IsolationLevel.RepeatableRead;
                option.Timeout = TimeSpan.FromSeconds(90);
                using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, option))
                {
                    using (var context = new AuthContext())
                    {

                        DamageOrderMaster objOrderMaster = context.DamageOrderMasterDB.Where(c => c.DamageOrderId == damage.DamageOrderId).Include(x => x.DamageorderDetails).FirstOrDefault();

                        objOrderMaster.Status = damage.OrderTypes == 2 ? "Delivered" : "Ready to Dispatch";
                        objOrderMaster.Deliverydate = indianTime;
                        objOrderMaster.UpdatedDate = indianTime;
                        objOrderMaster.DboyId = damage.DboyId;

                        Customer cust = context.Customers.FirstOrDefault(c => c.CustomerId == damage.CustomerId);

                        foreach (var ds in damage.DamageorderDetails)
                        {
                            var dmageItemsRecalculate = objOrderMaster.DamageorderDetails.Where(x => x.DamageOrderDetailsId == ds.DamageOrderDetailsId).FirstOrDefault();
                            if (dmageItemsRecalculate != null)
                            {
                                dmageItemsRecalculate.Status = "Ready to Dispatch";
                                dmageItemsRecalculate.UnitPrice = ds.UnitPrice;
                                int qty = 0;
                                dmageItemsRecalculate.qty = Convert.ToInt32(ds.qty);
                                //........CALCULATION FOR NEW SHOPKIRANA.............................
                                dmageItemsRecalculate.Noqty = dmageItemsRecalculate.qty; // for total qty (no of items)

                                // STEP 1  (UNIT PRICE * QTY)     - SHOW PROPERTY                  
                                dmageItemsRecalculate.TotalAmt = damage.OrderTypes == 2 ? 0 : System.Math.Round(dmageItemsRecalculate.UnitPrice * dmageItemsRecalculate.qty, 2);

                                // STEP 2 (AMOUT WITHOU TEX AND WITHOUT DISCOUNT ) - SHOW PROPERTY
                                dmageItemsRecalculate.AmtWithoutTaxDisc = damage.OrderTypes == 2 ? 0 : ((100 * dmageItemsRecalculate.UnitPrice * dmageItemsRecalculate.qty) / (1 + dmageItemsRecalculate.TaxPercentage / 100)) / 100;

                                // STEP 3 (AMOUNT WITHOUT TAX AFTER DISCOUNT) - UNSHOW PROPERTY
                                dmageItemsRecalculate.AmtWithoutAfterTaxDisc = damage.OrderTypes == 2 ? 0 : (100 * dmageItemsRecalculate.AmtWithoutTaxDisc) / (100 + 0); //items.PramotionalDiscount);

                                //STEP 4 (TAX AMOUNT) - UNSHOW PROPERTY
                                dmageItemsRecalculate.TaxAmmount = damage.OrderTypes == 2 ? 0 : (dmageItemsRecalculate.AmtWithoutAfterTaxDisc * dmageItemsRecalculate.TaxPercentage) / 100;

                                //STEP 5(TOTAL TAX AMOUNT) - UNSHOW PROPERTY
                                dmageItemsRecalculate.TotalAmountAfterTaxDisc = damage.OrderTypes == 2 ? 0 : dmageItemsRecalculate.AmtWithoutAfterTaxDisc + dmageItemsRecalculate.TaxAmmount;

                                //...............Calculate Discount.............................
                                dmageItemsRecalculate.DiscountPercentage = 0;// items.PramotionalDiscount;
                                dmageItemsRecalculate.DiscountAmmount = 0;// (ds.NetAmmount * items.PramotionalDiscount) / 100;
                                double DiscountAmmount = dmageItemsRecalculate.DiscountAmmount;
                                double NetAmtAfterDis = (dmageItemsRecalculate.NetAmmount - DiscountAmmount);
                                dmageItemsRecalculate.NetAmtAfterDis = (dmageItemsRecalculate.NetAmmount - DiscountAmmount);

                                double TaxAmmount = dmageItemsRecalculate.TaxAmmount;

                                dmageItemsRecalculate.UpdatedDate = DateTime.Now;
                                TotalAmount += dmageItemsRecalculate.TotalAmt;
                                // objOrderMaster.DamageorderDetails.Add(ds);
                                finaltotal = finaltotal + dmageItemsRecalculate.TotalAmt;
                                finalTaxAmount = finalTaxAmount + dmageItemsRecalculate.TaxAmmount;
                                finalGrossAmount = finalGrossAmount + dmageItemsRecalculate.TotalAmountAfterTaxDisc;
                                finalTotalTaxAmount = finalTotalTaxAmount + dmageItemsRecalculate.TotalAmountAfterTaxDisc;
                            }
                        }
                        objOrderMaster.TaxAmount = System.Math.Round(finalTaxAmount, 2);

                        objOrderMaster.DiscountAmount = finalTotalTaxAmount - finaltotal;
                        objOrderMaster.GrossAmount = System.Math.Round(finalGrossAmount, 0);
                        objOrderMaster.TotalAmount = damage.OrderTypes == 2 ? 0 : System.Math.Round(TotalAmount, 2);
                        context.Entry(objOrderMaster).State = EntityState.Modified;
                        context.Commit();
                        var results = new OrderMaster();
                        results = GenearteDamageOrderOrderMaster(objOrderMaster, context, user, Warehouse, itemslist, cust, scope);

                        //CreateAssignmentForNonRevenue Orders
                        if (damage.OrderTypes == 2 && user.PeopleID > 0 && results != null)
                        {
                            OrderDispatchedMaster res1 = StockHitOnIssued(context, user, results, compid, scope, damage);
                            if(res1==null)
                            {
                                result.Status = false;
                                result.Message = "Something went wrong";
                                return result;
                            }
                            bool res2 = StockHitOnShipped(res1, context, user, scope);
                            if (res2 == null)
                            {
                                result.Status = false;
                                result.Message = "Something went wrong";
                                return result;
                            }
                            bool res3 = StockHitOnDelivered(res1, context, user, scope);
                            if (res3 == null)
                            {
                                result.Status = false;
                                result.Message = "Something went wrong";
                                return result;
                            }
                        }

                        if (results != null && results.OrderId > 0 && results.Description == null)
                        {
                            objOrderMaster.OrderId = results.OrderId;
                            objOrderMaster.invoice_no = results.invoice_no;
                            context.Entry(objOrderMaster).State = EntityState.Modified;
                            if (context.Commit() > 0)
                            {
                                scope.Complete();
                                resultobjOrderMaster = objOrderMaster;
                                result.Status = true;
                                result.Message = "Order RTD Process Successfuly";

                            }
                            else
                            {
                                result.Status = false;
                                result.Message = "Something went wrong";
                                return result;
                            }
                        }
                        else
                        {
                            result.Status = false;
                            result.Message = "Something went wrong";
                            return result;
                        }
                    }
                }

                return result;
            }
            return result;
        }
        #endregion

        #region old code
        //public DamageOrderMaster AddDamageOrderMultiItem(DamageOrderMaster sc, AuthContext context, int UserId, int CompanyId)
        //{
        //    double finaltotal = 0;
        //    double finalTaxAmount = 0;
        //    double finalGrossAmount = 0;
        //    double TotalAmount = 0;
        //    double finalTotalTaxAmount = 0;
        //    int Customerid = sc.CustomerId;
        //    int warehouseid = sc.WarehouseId;
        //    int companyid = CompanyId;

        //    Customer cust = context.Customers.Where(c => c.CustomerId == Customerid).SingleOrDefault();
        //    Warehouse warehouse = context.Warehouses.Where(x => x.WarehouseId == warehouseid && x.Deleted == false).Select(x => x).FirstOrDefault();

        //    DamageOrderMaster objOrderMaster = sc;


        //    objOrderMaster.Status = "Delivered";
        //    objOrderMaster.Deliverydate = DateTime.Now;
        //    objOrderMaster.UpdatedDate = DateTime.Now;


        //    var itemsList = new List<ItemMaster>();
        //    var DamageStockList = new List<DamageStock>();
        //    List<DamageStockHistory> AddDamageStockHistoryList = new List<DamageStockHistory>();
        //    List<DamageStock> updateDamageStockList = new List<DamageStock>();
        //    List<DamageOrderDetails> damageOrderDetails = new List<DamageOrderDetails>();
        //    foreach (var ds in objOrderMaster.DamageorderDetails)
        //    {
        //        ItemMaster items = context.itemMasters.Where(x => x.ItemId == ds.ItemId).FirstOrDefault();

        //        DamageStock damagestock = context.DamageStockDB.Where(x => x.ItemNumber == ds.itemNumber && x.WarehouseId == ds.WarehouseId && x.ItemMultiMRPId ==ds.ItemMultiMRPId).FirstOrDefault();
        //        if (damagestock != null && damagestock.DamageInventory > 0 && damagestock.DamageInventory >= ds.qty)
        //        {
        //            ds.Status = "Delivered";
        //            ds.UnitPrice = ds.UnitPrice;
        //            ds.DefaultUnitPrice = damagestock.UnitPrice;
        //            int qty = 0;
        //            qty = Convert.ToInt32(ds.qty);


        //            //........CALCULATION FOR NEW SHOPKIRANA.............................
        //            ds.Noqty = qty; // for total qty (no of items)

        //            // STEP 1  (UNIT PRICE * QTY)     - SHOW PROPERTY                  
        //            ds.TotalAmt = System.Math.Round(ds.UnitPrice * qty, 2);

        //            // STEP 2 (AMOUT WITHOU TEX AND WITHOUT DISCOUNT ) - SHOW PROPERTY
        //            ds.AmtWithoutTaxDisc = ((100 * ds.UnitPrice * qty) / (1 + ds.TaxPercentage / 100)) / 100;

        //            // STEP 3 (AMOUNT WITHOUT TAX AFTER DISCOUNT) - UNSHOW PROPERTY
        //            ds.AmtWithoutAfterTaxDisc = (100 * ds.AmtWithoutTaxDisc) / (100 + items.PramotionalDiscount);

        //            //STEP 4 (TAX AMOUNT) - UNSHOW PROPERTY
        //            ds.TaxAmmount = (ds.AmtWithoutAfterTaxDisc * ds.TaxPercentage) / 100;

        //            //STEP 5(TOTAL TAX AMOUNT) - UNSHOW PROPERTY
        //            ds.TotalAmountAfterTaxDisc = ds.AmtWithoutAfterTaxDisc + ds.TaxAmmount;


        //            //...............Calculate Discount.............................
        //            ds.DiscountPercentage = items.PramotionalDiscount;
        //            ds.DiscountAmmount = (ds.NetAmmount * items.PramotionalDiscount) / 100;
        //            double DiscountAmmount = ds.DiscountAmmount;
        //            double NetAmtAfterDis = (ds.NetAmmount - DiscountAmmount);
        //            ds.NetAmtAfterDis = (ds.NetAmmount - DiscountAmmount);

        //            double TaxAmmount = ds.TaxAmmount;

        //            ds.UpdatedDate = DateTime.Now;
        //            TotalAmount += ds.TotalAmt;
        //            // objOrderMaster.DamageorderDetails.Add(ds);
        //            finaltotal = finaltotal + ds.TotalAmt;
        //            finalTaxAmount = finalTaxAmount + ds.TaxAmmount;
        //            finalGrossAmount = finalGrossAmount + ds.TotalAmountAfterTaxDisc;
        //            finalTotalTaxAmount = finalTotalTaxAmount + ds.TotalAmountAfterTaxDisc;

        //            damagestock.DamageInventory = damagestock.DamageInventory - ds.qty;

        //            updateDamageStockList.Add(damagestock);

        //            var DSH = new DamageStockHistory();
        //            DSH.CompanyId = ds.CompanyId;
        //            DSH.CreatedDate = DateTime.Now;
        //            DSH.DamageInventory = damagestock.DamageInventory;
        //            DSH.DamageStockId = damagestock.DamageStockId;
        //            DSH.Deleted = false;
        //            DSH.InwordQty = 0;
        //            DSH.itemBaseName = damagestock.itemBaseName;
        //            DSH.ItemId = damagestock.ItemId;
        //            DSH.ItemMultiMRPId = damagestock.ItemMultiMRPId;
        //            DSH.ItemName = damagestock.ItemName;
        //            DSH.ItemNumber = damagestock.ItemNumber;
        //            DSH.MRP = damagestock.MRP;
        //            DSH.OutwordQty = ds.qty;
        //            DSH.ReasonToTransfer = damagestock.ReasonToTransfer;
        //            DSH.UnitofQuantity = damagestock.UnitofQuantity;
        //            DSH.UnitPrice = damagestock.UnitPrice;
        //            DSH.UOM = damagestock.UOM;
        //            DSH.UpdatedDate = DateTime.Now;
        //            DSH.WarehouseId = damagestock.WarehouseId;
        //            DSH.WarehouseName = damagestock.WarehouseName;
        //            DSH.CreatedBy = UserId;
        //            AddDamageStockHistoryList.Add(DSH);
        //        }
        //        damageOrderDetails.Add(ds);

        //    }
        //    objOrderMaster.TaxAmount = System.Math.Round(finalTaxAmount, 2);
        //    objOrderMaster.DiscountAmount = finalTotalTaxAmount - finaltotal;
        //    objOrderMaster.GrossAmount = Convert.ToInt32(finalGrossAmount);
        //    objOrderMaster.TotalAmount = Convert.ToInt32(TotalAmount);

        //    if (AddDamageStockHistoryList != null && AddDamageStockHistoryList.Any())
        //    {
        //        foreach (var item in updateDamageStockList)
        //        {
        //            item.UpdatedDate = DateTime.Now;
        //            context.Entry(item).State = EntityState.Modified;
        //        }
        //        foreach (var detail in damageOrderDetails) {
        //            context.Entry(detail).State = EntityState.Modified;

        //        }

        //        context.DamageStockHistoryDB.AddRange(AddDamageStockHistoryList);
        //        context.Entry(objOrderMaster).State = EntityState.Modified;

        //        context.Commit();
        //    }
        //    return objOrderMaster;

        //}
        #endregion

        #region Change order status
        /// <summary>
        /// Created by Raj
        /// Created Date 17/04/2020
        /// </summary>
        [Route("EditDamage")]
        [HttpPut]
        public bool EditDamageorder(DamageorderEditDC damageorderEditDC)
        {
            bool Result = false;
            using (var context = new AuthContext())
            {
                var identity = User.Identity as ClaimsIdentity;
                int compid = 0, userid = 0;

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                    userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))
                    compid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);
                if (damageorderEditDC != null && userid > 0)
                {
                    bool isDamageorder = false;
                    if (damageorderEditDC.OrderType == 0)
                    {
                        isDamageorder = false;
                    }
                    else
                    {
                        isDamageorder = true;
                    }
                    DamageOrderMaster damageOrderMaster = context.DamageOrderMasterDB.Where(x => x.DamageOrderId == damageorderEditDC.DamageOrderId && x.isDamageOrder == isDamageorder).Include("DamageorderDetails").FirstOrDefault();
                    if (damageOrderMaster != null && (damageOrderMaster.Status == "Pending" || damageOrderMaster.Status == "Order Canceled"))
                    {
                        damageOrderMaster.Status = damageorderEditDC.Status;
                        damageOrderMaster.ReasonCancle = damageorderEditDC.ReasonCancle;
                        damageOrderMaster.UpdatedDate = indianTime;
                        damageOrderMaster.Updateby = userid;
                        foreach (var detail in damageOrderMaster.DamageorderDetails)
                        {
                            detail.UpdatedDate = indianTime;
                            detail.Status = damageorderEditDC.Status;
                        }
                        context.Entry(damageOrderMaster).State = EntityState.Modified;
                        context.Commit();
                        Result = true;
                    }
                }
            }
            return Result;
        }

        #endregion

        [Route("SerachDamageOrder")]
        [HttpGet]
        public dynamic SerachDamageOrder(string key)
        {
            using (var db = new AuthContext())
            {
                var damageOrder = db.DamageOrderMasterDB.Where(x => (x.DamageOrderId.ToString().Contains(key) || key.Contains(x.Skcode) || key.Contains(x.invoice_no)) && x.Deleted == false).Include("DamageorderDetails").ToList();
                var wh = damageOrder.Select(x => x.WarehouseId).Distinct().ToList();
                var whname = db.Warehouses.Where(x => wh.Contains(x.WarehouseId)).ToList();
                var InvoiceNumbers = damageOrder.Select(x => x.invoice_no).Distinct().ToList();
                var skCode = damageOrder.Select(x => x.Skcode).Distinct().ToList();
                var orderDispatchedMaster = db.OrderDispatchedMasters.Where(x => InvoiceNumbers.Contains(x.invoice_no) && skCode.Contains(x.Skcode)).Select(x => new { x.OrderId, x.invoice_no, x.Status }).ToList();

                foreach (var item in damageOrder)
                {
                    var orderDetail = orderDispatchedMaster.Where(x => x.invoice_no == item.invoice_no).Select(x => new { x.OrderId, x.Status }).FirstOrDefault();
                    item.WarehouseName = whname.Where(x => x.WarehouseId == item.WarehouseId).Select(x => x.WarehouseName).FirstOrDefault();
                    if (orderDetail != null)
                    {
                        item.OrderId = orderDetail.OrderId > 0 ? orderDetail.OrderId : 0;
                        item.Status = orderDetail.OrderId > 0 ? orderDetail.Status : null;
                    }
                }
                return damageOrder;
            }
        }

        [Route("ExportDamageOrder")]
        [HttpGet]
        public PaggingData ExportDamageOrder(string WarehouseId, string Orderstatus, int OrderType, int skip, int take)
        {
            using (AuthContext context = new AuthContext())
            {
                var WarehouseIds = WarehouseId.Split(',').Select(Int32.Parse).ToList();
                List<DamageOrderMaster> newdata = new List<DamageOrderMaster>();
                List<DamageOrderMaster> damageOrderMasterData = new List<DamageOrderMaster>();
                PaggingData obj = new PaggingData();
                bool isDamageOrder = false;
                if (OrderType == 1)
                {
                    isDamageOrder = true;
                }
                else if (OrderType == -1)
                {
                    newdata = context.DamageOrderMasterDB.Where(x => x.Deleted == false && WarehouseIds.Contains(x.WarehouseId)).OrderByDescending(x => x.DamageOrderId).Skip(skip).Take(take).ToList();
                }

                if (OrderType == 1 || OrderType == 0) //(Orderstatus == "Show All")
                {
                    newdata = context.DamageOrderMasterDB.Where(x => x.Deleted == false && WarehouseIds.Contains(x.WarehouseId) && x.isDamageOrder == isDamageOrder).OrderByDescending(x => x.DamageOrderId).Skip(skip).Take(take).ToList();
                }
                //else
                //{
                //    newdata = context.DamageOrderMasterDB.Where(x => x.Deleted == false && x.Status == Orderstatus && WarehouseIds.Contains(x.WarehouseId) && x.isDamageOrder == isDamageOrder).OrderByDescending(x => x.DamageOrderId).ToList();
                //}
                var wh = newdata.Select(x => x.WarehouseId).Distinct().ToList();
                var whname = context.Warehouses.Where(x => wh.Contains(x.WarehouseId)).ToList();
                var InvoiceNumbers = newdata.Select(x => x.invoice_no).Distinct().ToList();
                if (Orderstatus == "Show All")
                {
                    var orderDispatchedMaster = context.OrderDispatchedMasters.Where(x => InvoiceNumbers.Contains(x.invoice_no)).Select(x => new { x.OrderId, x.invoice_no, x.Status }).ToList();
                    foreach (var item in newdata)
                    {
                        item.WarehouseName = whname.Where(x => x.WarehouseId == item.WarehouseId).Select(x => x.WarehouseName).FirstOrDefault();
                        var orderDetail = orderDispatchedMaster.Where(x => x.invoice_no == item.invoice_no).Select(x => new { x.OrderId, x.Status }).FirstOrDefault();
                        if (orderDetail != null)
                        {
                            item.OrderId = orderDetail.OrderId > 0 ? orderDetail.OrderId : 0;
                            item.Status = orderDetail.OrderId > 0 ? orderDetail.Status : null;
                        }
                        damageOrderMasterData.Add(item);
                    }
                }
                else
                {
                    var orderDispatchedMaster = context.OrderDispatchedMasters.Where(x => InvoiceNumbers.Contains(x.invoice_no) && x.Status == Orderstatus).Select(x => new { x.OrderId, x.invoice_no, x.Status }).ToList();
                    foreach (var item in newdata)
                    {
                        item.WarehouseName = whname.Where(x => x.WarehouseId == item.WarehouseId).Select(x => x.WarehouseName).FirstOrDefault();
                        var orderDetail = orderDispatchedMaster.Where(x => x.invoice_no == item.invoice_no).Select(x => new { x.OrderId, x.Status }).FirstOrDefault();
                        if (orderDetail != null)
                        {
                            item.OrderId = orderDetail.OrderId > 0 ? orderDetail.OrderId : 0;
                            item.Status = orderDetail.OrderId > 0 ? orderDetail.Status : null;
                            damageOrderMasterData.Add(item);
                        }
                        if (Orderstatus == "Pending" && item.Status == Orderstatus)
                        {
                            damageOrderMasterData.Add(item);
                        }

                    }
                }
                if (OrderType == -1)
                {
                    obj.total_count = context.DamageOrderMasterDB.Where(x => x.Deleted == false && WarehouseIds.Contains(x.WarehouseId)).OrderByDescending(x => x.DamageOrderId).Count();
                }

                if (OrderType == 1 || OrderType == 0) //(Orderstatus == "Show All")
                {
                    obj.total_count = context.DamageOrderMasterDB.Where(x => x.Deleted == false && WarehouseIds.Contains(x.WarehouseId) && x.isDamageOrder == isDamageOrder).OrderByDescending(x => x.DamageOrderId).Count();
                }
                obj.ordermaster = damageOrderMasterData;
                return obj;
            }
        }
        public class DamagestockforOrderDC
        {
            public int DamageOrderDetailsId { get; set; }
            public int Damagestock { get; set; }
            public int CurrentBatchStock { get; set; }
        }
        public class DamageorderEditDC
        {
            public int DamageOrderId { get; set; }
            public string Status { get; set; }
            public int OrderType { get; set; }
            public string ReasonCancle { get; set; }
        }
        public class DamageOrderResp
        {
            public DamageOrderMaster DamageOrderMaster { get; set; }
            public bool Status { get; set; }

            public string Message { get; set; }
        }




        private OrderMaster GenearteDamageOrderOrderMaster(DamageOrderMaster sc, AuthContext context, People people, Warehouse Warehouse, List<ItemMaster> itemslist, Customer cust, TransactionScope scope)
        {

            OrderMaster objOrderMaster = new OrderMaster();
            objOrderMaster.orderDetails = new List<OrderDetails>();

            #region Prepair order 
            objOrderMaster.CompanyId = sc.CompanyId;
            objOrderMaster.WarehouseId = Warehouse.WarehouseId;
            objOrderMaster.WarehouseName = Warehouse.WarehouseName;
            objOrderMaster.CustomerCategoryId = 2;
            objOrderMaster.Status = "Ready to Dispatch";
            objOrderMaster.CustomerName = cust.Name;
            objOrderMaster.ShopName = cust.ShopName;
            objOrderMaster.BillingAddress = sc.ShippingAddress;
            objOrderMaster.ShippingAddress = sc.ShippingAddress;
            objOrderMaster.Customerphonenum = sc.Customerphonenum;
            //objOrderMaster.LandMark = sc.LandMark;
            objOrderMaster.Skcode = sc.Skcode;
            objOrderMaster.Tin_No = sc.Tin_No;
            objOrderMaster.CustomerType = sc.CustomerType;
            objOrderMaster.CustomerId = cust.CustomerId;
            objOrderMaster.CityId = sc.CityId;
            objOrderMaster.ClusterId = Convert.ToInt32(sc.ClusterId);
            objOrderMaster.OrderTakenSalesPersonId = 0;
            objOrderMaster.OrderTakenSalesPerson = "Self";
            objOrderMaster.active = true;
            objOrderMaster.CreatedDate = sc.CreatedDate;
            //if (cust.ExecutiveId > 0)
            //{
            //    objOrderMaster.SalesPersonId = cust.ExecutiveId;
            //    objOrderMaster.SalesPerson = cust.ExecutiveName;
            //}
            if (sc.CreatedDate.Hour > 16)
            {
                objOrderMaster.Deliverydate = sc.CreatedDate.AddDays(2);
            }
            else
            {
                objOrderMaster.Deliverydate = sc.CreatedDate.AddDays(1);
            }

            objOrderMaster.UpdatedDate = indianTime;
            objOrderMaster.Deleted = false;
            #endregion
            RetailerAppManager retailerAppManager = new RetailerAppManager();
            List<DataContracts.Masters.Store.StoreCategorySubCategoryBrand> StoreCategorySubCategoryBrands = retailerAppManager.GetStoreWithDetail();
            List<DataContracts.Masters.Store.ClusterStoreExecutiveDc> clusterStoreExecutiveDcs = retailerAppManager.GetStoreClusterExecutiveDetail();
            List<int> ItemMultiMrpIds = sc.DamageorderDetails.Select(x => x.ItemMultiMRPId).Distinct().ToList();
            List<ItemScheme> itemPTR = retailerAppManager.GetItemScheme(ItemMultiMrpIds, cust.Warehouseid ?? 0);
            foreach (var it in sc.DamageorderDetails.Where(c => c.qty > 0))
            {
                var items = itemslist.Where(x => x.ItemId == it.ItemId).FirstOrDefault();
                OrderDetails od = new OrderDetails();
                if (StoreCategorySubCategoryBrands != null && StoreCategorySubCategoryBrands.Any(x => x.Categoryid == items.Categoryid && x.SubCategoryId == items.SubCategoryId && x.BrandId == items.SubsubCategoryid))
                {
                    var store = StoreCategorySubCategoryBrands.First(x => x.Categoryid == items.Categoryid && x.SubCategoryId == items.SubCategoryId && x.BrandId == items.SubsubCategoryid);
                    od.StoreId = store.StoreId;
                    if (store.StoreId > 0 && clusterStoreExecutiveDcs != null && clusterStoreExecutiveDcs.Any(a => a.StoreId == store.StoreId && a.ClusterId == cust.ClusterId))
                    {
                        var clusterStoreExecutiveDc = clusterStoreExecutiveDcs.FirstOrDefault(a => a.StoreId == store.StoreId && a.ClusterId == cust.ClusterId);
                        od.ExecutiveId = clusterStoreExecutiveDc.ExecutiveId;
                        od.ExecutiveName = clusterStoreExecutiveDc.ExecutiveName;
                    }
                }
                else
                {
                    od.StoreId = 0;
                    od.ExecutiveId = 0;
                    od.ExecutiveName = "";
                }
                od.CustomerName = sc.CustomerName;
                od.Mobile = sc.Customerphonenum;
                od.CustomerId = sc.CustomerId;
                od.CityId = sc.CityId;
                od.OrderDate = indianTime;
                od.Status = "Ready to Dispatch";
                od.CompanyId = sc.CompanyId;
                od.WarehouseId = sc.WarehouseId;
                od.WarehouseName = sc.WarehouseName;
                od.NetPurchasePrice = it.NetPurchasePrice;
                od.ItemId = it.ItemId;
                od.ItemMultiMRPId = it.ItemMultiMRPId;
                // od.Itempic = items.LogoUrl;
                od.itemname = it.itemname;
                //od.SupplierName = items.SupplierName;
                od.SellingUnitName = it.itemname;
                od.CategoryName = items.CategoryName;
                od.SubsubcategoryName = items.SubsubcategoryName;
                od.SubcategoryName = items.SubcategoryName;
                od.SellingSku = it.SellingSku;
                od.City = items.CityName;
                od.itemcode = items.itemcode;
                od.HSNCode = items.HSNCode;
                od.itemNumber = items.Number;
                od.Barcode = items.itemcode;

                od.UnitPrice = it.UnitPrice;
                var schemeptr = itemPTR.Any(y => y.ItemMultiMRPId == items.ItemMultiMRPId) ? itemPTR.FirstOrDefault(y => y.ItemMultiMRPId == items.ItemMultiMRPId).PTR : 0;
                if (schemeptr > 0)
                {
                    od.PTR = Math.Round((schemeptr - 1) * 100, 2); //percent
                }
                od.price = it.price;
                od.MinOrderQty = 1;//items.MinOrderQty;
                od.MinOrderQtyPrice = (od.MinOrderQty * od.UnitPrice);
                od.qty = Convert.ToInt32(it.qty);
                od.SizePerUnit = items.SizePerUnit;
                od.TaxPercentage = sc.OrderTypes == 2 ? 0 : items.TotalTaxPercentage;
                if (od.TaxPercentage >= 0 && sc.OrderTypes != 2)
                {
                    od.SGSTTaxPercentage = od.TaxPercentage / 2;
                    od.CGSTTaxPercentage = od.TaxPercentage / 2;
                }
                od.Noqty = od.qty; // for total qty (no of items)    
                od.TotalAmt = sc.OrderTypes == 2 ? 0 : System.Math.Round(od.UnitPrice * od.qty, 2);

                if (items.TotalCessPercentage > 0 && sc.OrderTypes != 2)
                {
                    od.TotalCessPercentage = items.TotalCessPercentage;
                    double tempPercentagge = items.TotalCessPercentage + items.TotalTaxPercentage;

                    od.AmtWithoutTaxDisc = ((100 * od.UnitPrice * od.qty) / (1 + tempPercentagge / 100)) / 100;


                    od.AmtWithoutAfterTaxDisc = (100 * od.AmtWithoutTaxDisc) / (100 + items.PramotionalDiscount);
                    od.CessTaxAmount = (od.AmtWithoutAfterTaxDisc * od.TotalCessPercentage) / 100;
                }
                double tempPercentagge2 = items.TotalCessPercentage + items.TotalTaxPercentage;

                od.AmtWithoutTaxDisc = sc.OrderTypes == 2 ? 0 : ((100 * od.UnitPrice * od.qty) / (1 + tempPercentagge2 / 100)) / 100;
                od.AmtWithoutAfterTaxDisc = sc.OrderTypes == 2 ? 0 : (100 * od.AmtWithoutTaxDisc) / (100 + items.PramotionalDiscount);
                od.TaxAmmount = sc.OrderTypes == 2 ? 0 : (od.AmtWithoutAfterTaxDisc * od.TaxPercentage) / 100;
                if (od.TaxAmmount >= 0 && sc.OrderTypes != 2)
                {
                    od.SGSTTaxAmmount = od.TaxAmmount / 2;
                    od.CGSTTaxAmmount = od.TaxAmmount / 2;
                }
                //for cess
                if (od.CessTaxAmount > 0 && sc.OrderTypes != 2)
                {
                    double tempPercentagge3 = items.TotalCessPercentage + items.TotalTaxPercentage;
                    od.AmtWithoutTaxDisc = ((100 * od.UnitPrice * od.qty) / (1 + tempPercentagge3 / 100)) / 100;
                    od.AmtWithoutAfterTaxDisc = (100 * od.AmtWithoutTaxDisc) / (100 + items.PramotionalDiscount);
                    od.TotalAmountAfterTaxDisc = od.AmtWithoutAfterTaxDisc + od.CessTaxAmount + od.TaxAmmount;
                }
                else
                {
                    od.TotalAmountAfterTaxDisc = sc.OrderTypes == 2 ? 0 : od.AmtWithoutAfterTaxDisc + od.TaxAmmount;
                }
                od.DiscountPercentage = 0;
                od.DiscountAmmount = 0;
                double DiscountAmmount = od.DiscountAmmount;
                double NetAmtAfterDis = (od.NetAmmount - DiscountAmmount);
                od.NetAmtAfterDis = (od.NetAmmount - DiscountAmmount);
                double TaxAmmount = od.TaxAmmount;
                od.Purchaseprice = it.Purchaseprice;
                od.CreatedDate = it.CreatedDate;
                od.UpdatedDate = indianTime;
                od.Deleted = false;
                od.marginPoint = 0;
                od.StockBatchMasterId = it.StockBatchMasterId;
                objOrderMaster.orderDetails.Add(od);
            }
            objOrderMaster.DiscountAmount = 0;
            objOrderMaster.ClusterId = sc.ClusterId;
            objOrderMaster.ClusterName = sc.ClusterName;
            objOrderMaster.paymentMode = "COD";
            objOrderMaster.Status = "Ready to Dispatch";
            objOrderMaster.OrderType = sc.OrderTypes == 2 ? 10 : sc.isDamageOrder == false ? 6 : 9;
            double DeliveryAmount = 0;
            DeliveryAmount = 0;

            objOrderMaster.TaxAmount = System.Math.Round(objOrderMaster.orderDetails.Sum(x => x.TaxAmmount + x.CessTaxAmount), 2);
            objOrderMaster.SGSTTaxAmmount = System.Math.Round(objOrderMaster.orderDetails.Sum(x => x.SGSTTaxAmmount), 2);
            objOrderMaster.CGSTTaxAmmount = System.Math.Round(objOrderMaster.orderDetails.Sum(x => x.CGSTTaxAmmount), 2);
            objOrderMaster.TotalAmount = System.Math.Round(objOrderMaster.orderDetails.Sum(x => x.TotalAmt));

            objOrderMaster.deliveryCharge = DeliveryAmount;
            objOrderMaster.TotalAmount += DeliveryAmount;


            objOrderMaster.TotalAmount = objOrderMaster.TotalAmount;
            objOrderMaster.BillDiscountAmount = 0;
            objOrderMaster.GrossAmount = System.Math.Round(objOrderMaster.TotalAmount, 0);

            context.DbOrderMaster.Add(objOrderMaster);
            context.Commit();

            if (objOrderMaster.OrderId != 0)
            {
                objOrderMaster.orderProcess = true;
                if (objOrderMaster.paymentMode.ToLower() == "cod")
                {
                    context.PaymentResponseRetailerAppDb.Add(new PaymentResponseRetailerApp
                    {
                        amount = Math.Round((sc.GrossAmount + DeliveryAmount), 0),
                        CreatedDate = sc.CreatedDate,
                        currencyCode = "INR",
                        OrderId = objOrderMaster.OrderId,
                        PaymentFrom = "Cash",
                        status = "Success",
                        statusDesc = "Order Place",
                        UpdatedDate = sc.CreatedDate,
                        IsRefund = false
                    });
                }
                string Borderid = Convert.ToString(objOrderMaster.OrderId);
                string BorderCodeId = Borderid.PadLeft(11, '0');
                temOrderQBcode code = context.GetBarcode(BorderCodeId);
                objOrderMaster.InvoiceBarcodeImage = code.BarcodeImage;
                ///// Do  ReadytoDispatched
                OrderDispatchedMaster postOrderDispatch = new OrderDispatchedMaster();
                postOrderDispatch = Mapper.Map(objOrderMaster).ToANew<OrderDispatchedMaster>();
                if (sc.DboyId > 0)
                {
                    var BackendCOD_Dboy = context.Peoples.FirstOrDefault(x => x.PeopleID == sc.DboyId && x.WarehouseId == objOrderMaster.WarehouseId);
                    if (BackendCOD_Dboy != null)
                    {
                        postOrderDispatch.DboyMobileNo = BackendCOD_Dboy.Mobile;
                        postOrderDispatch.DboyName = BackendCOD_Dboy.DisplayName;
                        postOrderDispatch.DBoyId = BackendCOD_Dboy.PeopleID;
                    }
                    else
                    {
                        return null;
                    }
                }
                else
                {
                    return null;
                }
                postOrderDispatch.Status = "Ready to Dispatch";
                postOrderDispatch.CreatedDate = indianTime;
                postOrderDispatch.OrderedDate = objOrderMaster.CreatedDate;
                string invoiceNumber = " ";
                if (postOrderDispatch.WarehouseId != 67 && postOrderDispatch.WarehouseId != 80 && sc.OrderTypes != 2)
                {
                    invoiceNumber = context.Database.SqlQuery<string>("EXEC spGetCurrentNumber 'Invoice', " + Warehouse.Stateid).FirstOrDefault();
                }
                else if (sc.OrderTypes == 2)
                {
                    invoiceNumber = context.Database.SqlQuery<string>("EXEC spGetCurrentNumber 'NROInvoice', " + Warehouse.Stateid).FirstOrDefault();
                }
                postOrderDispatch.invoice_no = invoiceNumber;
                objOrderMaster.ReadytoDispatchedDate = indianTime;
                objOrderMaster.UpdatedDate = indianTime;
                objOrderMaster.invoice_no = invoiceNumber;
                context.Entry(objOrderMaster).State = EntityState.Modified;
                context.OrderDispatchedMasters.Add(postOrderDispatch);
                OrderMasterHistories h1 = new OrderMasterHistories();
                h1.orderid = objOrderMaster.OrderId;
                h1.Status = "Ready to Dispatch";
                h1.Reasoncancel = null;
                h1.Warehousename = objOrderMaster.WarehouseName;
                h1.username = people.DisplayName;
                h1.userid = people.PeopleID;
                h1.CreatedDate = DateTime.Now;
                context.OrderMasterHistoriesDB.Add(h1);
                if (context.Commit() > 0)
                {
                    MultiStockHelper<RTDStockEntryDc> MultiStockHelpers = new MultiStockHelper<RTDStockEntryDc>();
                    List<RTDStockEntryDc> rtdStockList = new List<RTDStockEntryDc>();
                    List<TransferOrderItemBatchMasterDc> TransferOrderItemBatchMasterList = new List<TransferOrderItemBatchMasterDc>();
                    foreach (var StockHit in postOrderDispatch.orderDetails.Where(x => x.qty > 0))
                    {
                        rtdStockList.Add(new RTDStockEntryDc
                        {
                            ItemMultiMRPId = StockHit.ItemMultiMRPId,
                            OrderDispatchedDetailsId = StockHit.OrderDispatchedDetailsId,
                            OrderId = StockHit.OrderId,
                            Qty = StockHit.qty,
                            UserId = people.PeopleID,
                            WarehouseId = StockHit.WarehouseId,
                            IsFreeStock = false,
                            IsDispatchFromPlannedStock = false,
                            RefStockCode = sc.OrderTypes == 2 ? "NR" : sc.isDamageOrder == false ? "D" : "N"
                        });
                        TransferOrderItemBatchMasterList.Add(new TransferOrderItemBatchMasterDc
                        {

                            ItemMultiMRPId = StockHit.ItemMultiMRPId,
                            Qty = StockHit.qty * (-1),
                            StockBatchMasterId = StockHit.StockBatchMasterId,
                            WarehouseId = StockHit.WarehouseId,
                            ObjectId = StockHit.OrderId,
                            ObjectIdDetailId = StockHit.OrderDetailsId
                        });
                    }

                    if (rtdStockList.Any())
                    {
                        BatchMasterManager batchMasterManager = new BatchMasterManager();
                        bool res = MultiStockHelpers.MakeEntry(rtdStockList, "Stock_OnRTD", context, scope);

                        var StockTxnType = context.StockTxnTypeMasters.FirstOrDefault(x => x.IsActive && x.StockTxnType == "OrderOutDamage" && x.IsDeleted == false);
                       
                        bool batchRes = batchMasterManager.AddQty(TransferOrderItemBatchMasterList, context, people.PeopleID, StockTxnType.Id);
                        if (!res || !batchRes)
                        {
                            if (!batchRes)
                            {
                                objOrderMaster.Description = "Can't Dispatched Due to batchstock qty not available!!";
                                return objOrderMaster;
                            }
                            else
                            {
                                objOrderMaster.Description = "Can't Dispatched, Due to qty";
                                return objOrderMaster;

                            }
                        }
                        else
                        {
                            #region Update IRN Check 
                            IRNHelper irnHelper = new IRNHelper();

                            if (irnHelper.IsGenerateIRN(context, postOrderDispatch.CustomerId))
                            {
                                postOrderDispatch.IsGenerateIRN = true;
                                #region ClearTaxIntegrations
                                ClearTaxIntegration clearTaxIntegration = new ClearTaxIntegration();
                                clearTaxIntegration.OrderId = postOrderDispatch.OrderId;
                                clearTaxIntegration.IsActive = true;
                                clearTaxIntegration.CreateDate = DateTime.Now;
                                clearTaxIntegration.IsProcessed = false;
                                clearTaxIntegration.APIType = "GenerateIRN";
                                context.ClearTaxIntegrations.Add(clearTaxIntegration);
                                #endregion
                                context.Entry(postOrderDispatch).State = EntityState.Modified;

                            }
                            #endregion
                        }
                    }
                    return objOrderMaster;
                }
            }
            return objOrderMaster;
        }
        public OrderDispatchedMaster StockHitOnIssued(AuthContext context, People people, OrderMaster orderMastData, int compid, TransactionScope scope, DamageOrderMaster damageordermaster)
        {
            OrderDispatchedMaster OrderDispatchedMasters = new OrderDispatchedMaster();
            DeliveryIssuance AddDeliveryIssuance = new DeliveryIssuance();
            

            var obj = context.OrderDispatchedMasters.Where(x => x.OrderId == orderMastData.OrderId && (x.Status == "Ready to Dispatch")).Include(x => x.orderDetails).FirstOrDefault();
            var OrderMasterlist = context.DbOrderMaster.Where(x => x.OrderId == obj.OrderId).Include(x => x.orderDetails).FirstOrDefault();
            var OrderDetaillist = OrderMasterlist.orderDetails.Where(x => x.OrderId == obj.OrderId).ToList();

            var dboyname = context.Peoples.Where(x => x.PeopleID == damageordermaster.DboyId).FirstOrDefault().DisplayName;

            if (people != null && obj != null && OrderMasterlist != null && OrderDetaillist != null && OrderDetaillist.Count() > 0)
            {
                AddDeliveryIssuance.userid = people.PeopleID;
                AddDeliveryIssuance.CompanyId = compid;

                AddDeliveryIssuance.WarehouseId = obj.WarehouseId;
                AddDeliveryIssuance.WarehouseName = obj.WarehouseName;
                AddDeliveryIssuance.city = (people.Cityid).ToString();
                AddDeliveryIssuance.CreatedBy = (people.PeopleID).ToString();
                AddDeliveryIssuance.OrderIds = (obj.OrderId).ToString();
                AddDeliveryIssuance.TripPlannerConfirmedMasterId = obj.TripPlannerMasterId;
                AddDeliveryIssuance.OrderdispatchIds = Convert.ToString(obj.OrderDispatchedMasterId);
                AddDeliveryIssuance.DeliveryIssuanceId = 1;
                AddDeliveryIssuance.PeopleID = ((int)damageordermaster.DboyId); //people.PeopleID;
                AddDeliveryIssuance.VehicleId = people.VehicleId;
                AddDeliveryIssuance.Acceptance = true;
                AddDeliveryIssuance.IsActive = true;
                AddDeliveryIssuance.IdealTime = 0;
                AddDeliveryIssuance.CreatedDate = DateTime.Now;
                AddDeliveryIssuance.UpdatedDate = DateTime.Now;
                AddDeliveryIssuance.TravelDistance = 0;
                AddDeliveryIssuance.AgentId = 0;
                AddDeliveryIssuance.DisplayName = dboyname;//people.DisplayName;
                AddDeliveryIssuance.IsIcVerified = false;
                AddDeliveryIssuance.IsPhysicallyVerified = false;
                AddDeliveryIssuance.IsDeliveryCancel = false;
                AddDeliveryIssuance.TripPlannerConfirmedMasterId = 0;
                AddDeliveryIssuance.AssignmentType = 0;
                //AddDeliveryIssuance.db = 0;

                if (obj != null)
                {
                    AddDeliveryIssuance.Status = "Submitted";
                    AddDeliveryIssuance.TotalAssignmentAmount = obj.GrossAmount;
                    AddDeliveryIssuance.IsActive = true;

                    DataTable dt = new DataTable();
                    dt.Columns.Add("IntValue");
                    var dr = dt.NewRow();
                    dr["IntValue"] = obj.OrderId;
                    dt.Rows.Add(dr);
                    var param = new SqlParameter("orderids", dt);
                    param.SqlDbType = SqlDbType.Structured;
                    param.TypeName = "dbo.IntValues";
                    var OrderDispatchedDetailssList = context.Database.SqlQuery<OrderDispatchedDetailsDC>("Exec GetItemToCreatePickList @orderids", param).ToList();
                    AddDeliveryIssuance.details = new List<IssuanceDetails>();
                    AddDeliveryIssuance.details = OrderDispatchedDetailssList
                                        .GroupBy(y => new { y.ItemMultiMRPId, y.IsFreeItem }).Select(t =>
                                         new IssuanceDetails
                                         {
                                             OrderId = string.Join(",", t.Select(s => s.OrderId).Distinct()),
                                             OrderQty = string.Join(",", t.Select(a => String.Format("{0} - {1}", a.OrderId, a.qty)).ToArray()),
                                             OrderDispatchedMasterId = t.Select(x => x.OrderDispatchedMasterId).FirstOrDefault(),
                                             OrderDispatchedDetailsId = t.Select(x => x.OrderDispatchedDetailsId).FirstOrDefault(),
                                             qty = t.Sum(x => x.qty),
                                             itemNumber = t.FirstOrDefault().itemNumber,
                                             ItemId = t.FirstOrDefault().ItemId,
                                             itemname = t.FirstOrDefault().itemname,
                                             IsFreeItem = t.FirstOrDefault().IsFreeItem
                                         }).ToList();

                    var DBoyorders = context.DeliveryIssuanceDb.Add(AddDeliveryIssuance);
                    context.Commit();

                    #region  DeliveryHistory
                    OrderDeliveryMasterHistories AssginDeli = new OrderDeliveryMasterHistories();
                    AssginDeli.DeliveryIssuanceId = DBoyorders.DeliveryIssuanceId;
                    AssginDeli.Cityid = DBoyorders.Cityid;
                    AssginDeli.city = DBoyorders.city;
                    AssginDeli.DisplayName = DBoyorders.DisplayName;
                    AssginDeli.Status = DBoyorders.Status;
                    AssginDeli.WarehouseId = DBoyorders.WarehouseId;
                    AssginDeli.PeopleID = DBoyorders.PeopleID;
                    AssginDeli.VehicleId = DBoyorders.VehicleId;
                    AssginDeli.VehicleNumber = DBoyorders.VehicleNumber;
                    AssginDeli.RejectReason = DBoyorders.RejectReason;
                    AssginDeli.OrderdispatchIds = DBoyorders.OrderdispatchIds;
                    AssginDeli.OrderIds = DBoyorders.OrderIds;
                    AssginDeli.Acceptance = DBoyorders.Acceptance;
                    AssginDeli.IsActive = DBoyorders.IsActive;
                    AssginDeli.IdealTime = DBoyorders.IdealTime;
                    AssginDeli.CreatedDate = indianTime;
                    AssginDeli.UpdatedDate = indianTime;
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
                    if (DBoyorders.DeliveryIssuanceId > 0)
                    {
                        obj.Status = "Delivered";
                        obj.ReDispatchedStatus = "Delivered";
                        obj.UpdatedDate = indianTime;
                        obj.DeliveryIssuanceIdOrderDeliveryMaster = DBoyorders.DeliveryIssuanceId;

                        OrderMasterlist.Status = "Delivered";
                        OrderMasterlist.UpdatedDate = indianTime;
                        OrderMasterlist.DeliveredDate = indianTime;
                        foreach (var item in OrderDetaillist.Where(e => e.OrderId == obj.OrderId).ToList())
                        {
                            item.UpdatedDate = indianTime;
                            item.Status = "Delivered";
                            context.Entry(item).State = EntityState.Modified;
                        }
                        context.Entry(obj).State = EntityState.Modified;
                        context.Entry(OrderMasterlist).State = EntityState.Modified;
                    }
                    #region Code For OrderDeliveryMaster
                    OrderDeliveryMaster oDm = new OrderDeliveryMaster();
                    oDm.OrderId = obj.OrderId;
                    oDm.CityId = obj.CityId;
                    oDm.CompanyId = obj.CompanyId;
                    oDm.WarehouseId = obj.WarehouseId;
                    oDm.WarehouseName = obj.WarehouseName;
                    oDm.DboyMobileNo = obj.DboyMobileNo;
                    oDm.DboyName = obj.DboyName;
                    oDm.CustomerId = obj.CustomerId;
                    oDm.CustomerName = obj.CustomerName;
                    oDm.Customerphonenum = obj.Customerphonenum;
                    oDm.ShopName = obj.ShopName;
                    oDm.Skcode = obj.Skcode;
                    oDm.Status = "Delivered";
                    oDm.ShippingAddress = obj.ShippingAddress;
                    oDm.BillingAddress = obj.BillingAddress;
                    oDm.CanceledStatus = obj.CanceledStatus;
                    oDm.invoice_no = obj.invoice_no;
                    oDm.OnlineServiceTax = obj.OnlineServiceTax;
                    oDm.TotalAmount = obj.TotalAmount;
                    oDm.GrossAmount = obj.GrossAmount;
                    oDm.TaxAmount = obj.TaxAmount;
                    oDm.SGSTTaxAmmount = obj.SGSTTaxAmmount;
                    oDm.CGSTTaxAmmount = obj.CGSTTaxAmmount;
                    oDm.ReDispatchedStatus = obj.ReDispatchedStatus;
                    oDm.Trupay = obj.Trupay;
                    oDm.comments = obj.comments;
                    oDm.deliveryCharge = obj.deliveryCharge;
                    oDm.DeliveryIssuanceId = DBoyorders.DeliveryIssuanceId;
                    oDm.DiscountAmount = obj.DiscountAmount;
                    oDm.CheckNo = obj.CheckNo;
                    oDm.CheckAmount = obj.CheckAmount;
                    oDm.ElectronicPaymentNo = obj.ElectronicPaymentNo;
                    oDm.ElectronicAmount = obj.ElectronicAmount;
                    oDm.EpayLaterAmount = 0;
                    oDm.CashAmount = obj.CashAmount;
                    oDm.OrderedDate = obj.OrderedDate;
                    oDm.WalletAmount = obj.WalletAmount;
                    oDm.RewardPoint = obj.RewardPoint;
                    oDm.Tin_No = obj.Tin_No;
                    oDm.ReDispatchCount = obj.ReDispatchCount;
                    oDm.UpdatedDate = indianTime;
                    oDm.CreatedDate = indianTime;
                    context.OrderDeliveryMasterDB.Add(oDm);
                    #endregion
                    #region Order Master History
                    OrderMasterHistories hh1 = new OrderMasterHistories();
                    if (obj != null)
                    {
                        hh1.orderid = obj.OrderId;
                        hh1.Status = obj.Status;
                        hh1.Reasoncancel = null;

                        hh1.Warehousename = obj.WarehouseName;
                        if (people.DisplayName == null || people.DisplayName == "")
                        {
                            hh1.username = people.PeopleFirstName;
                            hh1.Description = " (Delivered AssignmentId : " + DBoyorders.DeliveryIssuanceId + ") By" + people.PeopleFirstName;
                        }
                        else
                        {
                            hh1.username = people.DisplayName;
                            hh1.Description = " (Delivered AssignmentId : " + DBoyorders.DeliveryIssuanceId + ") By" + people.DisplayName;
                        }
                        hh1.DeliveryIssuanceId = DBoyorders.DeliveryIssuanceId;
                        hh1.userid = people.PeopleID;
                        hh1.CreatedDate = indianTime;
                        context.OrderMasterHistoriesDB.Add(hh1);
                    }
                    #endregion

                    MultiStockHelper<OnIssuedStockEntryDc> MultiStockHelpers = new MultiStockHelper<OnIssuedStockEntryDc>();
                    List<OnIssuedStockEntryDc> OnIssuedStockEntryList = new List<OnIssuedStockEntryDc>();
                    foreach (var StockHit in OrderDispatchedDetailssList.Where(x => x.qty > 0 && x.OrderId == obj.OrderId))
                    {
                        var RefStockCode = "NR";
                        bool IsDeliveryRedispatch = false;
                        if (OrderMasterlist.ReDispatchCount > 0)
                        {
                            IsDeliveryRedispatch = true;
                        }

                        OnIssuedStockEntryList.Add(new OnIssuedStockEntryDc
                        {
                            ItemMultiMRPId = StockHit.ItemMultiMRPId,
                            OrderDispatchedDetailsId = StockHit.OrderDispatchedDetailsId,
                            OrderId = StockHit.OrderId,
                            Qty = StockHit.qty,
                            UserId = people.PeopleID,
                            WarehouseId = StockHit.WarehouseId,
                            IsDeliveryRedispatch = IsDeliveryRedispatch,
                            RefStockCode = RefStockCode,
                        });
                    }
                    if (OnIssuedStockEntryList.Any())
                    {
                        bool res = MultiStockHelpers.MakeEntry(OnIssuedStockEntryList, "Stock_OnIssued", context, scope);
                        if (!res)
                        {
                            return null;
                        }
                    }
                    OrderDispatchedMasters = obj;
                    context.Commit();
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return null;
            }
            return OrderDispatchedMasters;
        }
        public bool StockHitOnShipped(OrderDispatchedMaster OrderDispatchedMaster, AuthContext context, People people, TransactionScope scope)
        {
            bool Result = false;

            if (people != null && OrderDispatchedMaster != null)
            {
                MultiStockHelper<OnShippedStockEntryDc> MultiStockHelpers = new MultiStockHelper<OnShippedStockEntryDc>();
                List<OnShippedStockEntryDc> OnShippedStockEntryList = new List<OnShippedStockEntryDc>();

                foreach (var StockHit in OrderDispatchedMaster.orderDetails.Where(x => x.qty > 0))
                {
                    var RefStockCode = "NR";
                    OnShippedStockEntryList.Add(new OnShippedStockEntryDc
                    {
                        ItemMultiMRPId = StockHit.ItemMultiMRPId,
                        OrderDispatchedDetailsId = StockHit.OrderDispatchedDetailsId,
                        OrderId = StockHit.OrderId,
                        Qty = StockHit.qty,
                        UserId = people.PeopleID,
                        WarehouseId = StockHit.WarehouseId,
                        RefStockCode = RefStockCode
                    });
                    if (OnShippedStockEntryList.Any())
                    {
                        bool ress = MultiStockHelpers.MakeEntry(OnShippedStockEntryList, "Stock_OnShipped", context, scope);
                        if (!ress)
                        {
                            return Result;
                        }

                    }
                }

            }
            else
            {
                return Result;
            }
            return Result;


        }
        public bool StockHitOnDelivered(OrderDispatchedMaster OrderDispatchedMaster, AuthContext context, People people, TransactionScope scope)
        {
            bool Result = false;
            if (people != null && OrderDispatchedMaster != null)
            {
                MultiStockHelper<OnShippedStockEntryDc> MultiStockHelpers = new MultiStockHelper<OnShippedStockEntryDc>();
                List<OnShippedStockEntryDc> OnShippedStockEntryList = new List<OnShippedStockEntryDc>();
                foreach (var StockHit in OrderDispatchedMaster.orderDetails.Where(x => x.qty > 0))
                {
                    var RefStockCode = "NR";
                    OnShippedStockEntryList.Add(new OnShippedStockEntryDc
                    {
                        ItemMultiMRPId = StockHit.ItemMultiMRPId,
                        OrderDispatchedDetailsId = StockHit.OrderDispatchedDetailsId,
                        OrderId = StockHit.OrderId,
                        Qty = StockHit.qty,
                        UserId = people.PeopleID,
                        WarehouseId = StockHit.WarehouseId,
                        RefStockCode = RefStockCode
                    });
                    if (OnShippedStockEntryList.Any())
                    {
                        bool ress = MultiStockHelpers.MakeEntry(OnShippedStockEntryList, "Stock_OnDelivered_New", context, scope);
                        if (!ress)
                        {
                            return Result;
                        }
                    }
                }
            }
            else
            {
                return Result;
            }
            return Result;
        }

    }

}