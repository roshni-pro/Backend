using AngularJSAuthentication.Model;
using NLog;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Web.Http;

namespace AngularJSAuthentication.API.Controllers
{
    [RoutePrefix("api/ReturnPurchaseItem")]
    public class ReturnItemController : ApiController
    {
       
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private static TimeZoneInfo INDIAN_ZONE = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
        DateTime indianTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE);
        [Authorize]
        [Route("")]
        public HttpResponseMessage Get()
        {
            List<PurchaseReturn> ass = new List<PurchaseReturn>();
            try
            {
                var identity = User.Identity as ClaimsIdentity;
                int compid = 0, userid = 0;
                int Warehouseid = 0;

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
                        Warehouseid = int.Parse(claim.Value);
                    }
                }
                if (Warehouseid > 0)
                {
                    using (var context = new AuthContext())
                    {
                        ass = context.PurchaseReturnDb.Where(c => c.Deleted == false && c.CompanyId == compid && c.WarehouseId == Warehouseid).ToList();
                        logger.Info("End  Return: ");
                        return Request.CreateResponse(HttpStatusCode.OK, ass);
                    }
                }
                else
                {
                    using (var context = new AuthContext())
                    {
                        ass = context.PurchaseReturnDb.Where(c => c.Deleted == false && c.CompanyId == compid).ToList();
                        logger.Info("End  Return: ");
                        return Request.CreateResponse(HttpStatusCode.OK, ass);
                    }
                }
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex.Message);
            }
        }

        //[Route("add")]
        //[AcceptVerbs("POST")]
        //public HttpResponseMessage add(PurchaseReturn item)
        //{
        //    logger.Info("start Return: ");
        //    try
        //    {
        //        var identity = User.Identity as ClaimsIdentity;
        //        int compid = 0, userid = 0;
        //        int Warehouseid = 0;

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
        //                Warehouseid = int.Parse(claim.Value);
        //            }
        //        }
        //        item.CompanyId = compid;

        //        if (item.WarehouseId > 0)
        //        {
        //            if (item != null)
        //            {
        //                item.CreationDate = indianTime;
        //                item.Deleted = false;
        //                using (var context = new AuthContext())
        //                {
        //                    CurrentStock stok = context.DbCurrentStock.Where(x => x.ItemNumber == item.itemNumber && x.WarehouseId == item.WarehouseId && x.CompanyId == item.CompanyId && x.ItemMultiMRPId == item.ItemMultiMRPId).FirstOrDefault();
        //                    if (stok != null && stok.CurrentInventory > 0)
        //                    {
        //                        CurrentStockHistory Oss = new CurrentStockHistory();
        //                        if (stok != null)
        //                        {
        //                            Oss.StockId = stok.StockId;
        //                            Oss.ItemNumber = stok.ItemNumber;
        //                            Oss.ManualReason = "Current Stock Po Return";
        //                            Oss.itemname = stok.itemname;
        //                            Oss.CurrentInventory = stok.CurrentInventory;
        //                            Oss.PurchaseInventoryOut = Convert.ToInt32(item.TotalQuantity);
        //                            Oss.TotalInventory = Convert.ToInt32(stok.CurrentInventory - item.TotalQuantity);
        //                            Oss.WarehouseName = stok.WarehouseName;
        //                            Oss.Warehouseid = stok.WarehouseId;
        //                            Oss.CompanyId = stok.CompanyId;

        //                            Oss.CreationDate = indianTime;
        //                            context.CurrentStockHistoryDb.Add(Oss);
        //                            int idd = context.Commit();
        //                        }
        //                        stok.CurrentInventory = stok.CurrentInventory - item.TotalQuantity;
        //                        if (stok.CurrentInventory < 0)
        //                        {
        //                            stok.CurrentInventory = 0;
        //                        }
        //                        //context.DbCurrentStock.Attach(stok);
        //                        context.Entry(stok).State = EntityState.Modified;
        //                        context.Commit();

        //                        context.PurchaseReturnDb.Add(item);
        //                        context.Commit();
        //                    }
        //                    else
        //                    {
        //                        DamageStock stokData = context.DamageStockDB.Where(x => x.ItemNumber == item.itemNumber && x.WarehouseId == item.WarehouseId && x.CompanyId == item.CompanyId).SingleOrDefault();
        //                        if (stokData != null && stokData.DamageInventory > 0)
        //                        {
        //                            CurrentStockHistory Oss = new CurrentStockHistory();
        //                            if (stok != null)
        //                            {
        //                                Oss.StockId = stok.StockId;
        //                                Oss.ManualReason = "Damage Stock Po Return";
        //                                Oss.ItemNumber = stok.ItemNumber;
        //                                Oss.itemname = stok.itemname;
        //                                Oss.CurrentInventory = stok.CurrentInventory;
        //                                Oss.PurchaseInventoryOut = Convert.ToInt32(item.TotalQuantity);
        //                                Oss.TotalInventory = Convert.ToInt32(stok.CurrentInventory - item.TotalQuantity);
        //                                Oss.WarehouseName = stok.WarehouseName;
        //                                Oss.Warehouseid = stok.WarehouseId;
        //                                Oss.CompanyId = stok.CompanyId;
        //                                Oss.CreationDate = indianTime;
        //                                context.CurrentStockHistoryDb.Add(Oss);
        //                                int idd = context.Commit();
        //                            }
        //                            stok.CurrentInventory = stok.CurrentInventory - item.TotalQuantity;
        //                            if (stok.CurrentInventory < 0)
        //                            {
        //                                stok.CurrentInventory = 0;
        //                            }
        //                            //context.DbCurrentStock.Attach(stok);
        //                            context.Entry(stok).State = EntityState.Modified;
        //                            context.Commit();

        //                            context.PurchaseReturnDb.Add(item);
        //                            context.Commit();
        //                        }
        //                        else
        //                        {

        //                            return Request.CreateResponse(HttpStatusCode.OK, false);
        //                        }
        //                    }
        //                }
        //            }

        //        }
        //        return Request.CreateResponse(HttpStatusCode.OK, item);
        //    }
        //    catch (Exception ex)
        //    {
        //        return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex.Message);
        //    }
        //}
    }
}



