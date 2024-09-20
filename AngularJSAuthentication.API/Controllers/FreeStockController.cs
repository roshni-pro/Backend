using AngularJSAuthentication.API.Helper;
using AngularJSAuthentication.API.Managers;
using AngularJSAuthentication.Common.Helpers;
using AngularJSAuthentication.DataContracts.constants;
using AngularJSAuthentication.DataContracts.Transaction.Ledger.ItemLedger;
using AngularJSAuthentication.DataContracts.Transaction.Stocks;
using AngularJSAuthentication.Model;
using NLog;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Transactions;
using System.Web.Http;
namespace AngularJSAuthentication.API.Controllers
{
    [RoutePrefix("api/freestocks")]
    public class FreeStockController : ApiController
    {
        private static TimeZoneInfo INDIAN_ZONE = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");

        DateTime indianTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE);
        private static Logger logger = LogManager.GetCurrentClassLogger();
        [Route("")]
        public IEnumerable<FreeStock> Get()
        {
            logger.Info("start current stock: ");
            using (AuthContext context = new AuthContext())
            {
                List<FreeStock> ass = new List<FreeStock>();
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
                    if (Warehouse_id > 0)
                    {
                        logger.Info("User ID : {0} , Company Id : {1}", compid, userid);
                        ass = context.GetAllFreeStockWid(Warehouse_id).ToList();
                        logger.Info("End  current stock: ");
                        return ass;
                    }

                    else
                    {
                        logger.Info("User ID : {0} , Company Id : {1}", compid, userid);
                        ass = context.GetAllFreeStock(Warehouse_id).ToList();
                        logger.Info("End  free stock: ");
                        return ass;
                    }

                }
                catch (Exception ex)
                {
                    logger.Error("Error in free stock " + ex.Message);
                    logger.Info("End  free stock: ");
                    return null;
                }
            }
        }

        #region Warehouse based get data
        [Route("GetWarehouseFreeStock")]
        [HttpGet]
        public async Task<List<WarehouseBasedfreeStockDTO>> GetWarehouseFreeStock(int WarehouseId)
        {
            List<WarehouseBasedfreeStockDTO> result = null;
            using (AuthContext context = new AuthContext())
            {
                if (WarehouseId > 0)
                {
                    result = GetAllFreeStock(WarehouseId, context);
                }
                return result;
            }
        }

        public List<WarehouseBasedfreeStockDTO> GetAllFreeStock(int WarehouseId, AuthContext context)
        {
            string Query = "Exec GetAllFreeStock " + WarehouseId;

            var _result = context.Database.SqlQuery<WarehouseBasedfreeStockDTO>(Query).ToList();

            return _result;
        }

        #endregion

        #region Warehouse based get data in Free stock
        /// <summary>
        /// Created Date:16/09/2019
        /// Created by Vinayak
        /// </summary>
        /// <param name="WarehouseId"></param>
        /// <returns></returns>

        [Route("GetWarehousefreeStockInventory")]
        public IEnumerable<FreeStock> GetWarehousefreeStockInventory(string key, int WarehouseId)
        {
            logger.Info("start free stock: ");

            List<FreeStock> ass = new List<FreeStock>();

            try
            {
                var identity = User.Identity as ClaimsIdentity;
                int userid = 0;

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                    userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

                using (AuthContext db = new AuthContext())
                {

                    List<FreeStock> stockdata = db.FreeStockDB.Where(x => x.WarehouseId == WarehouseId && x.Deleted == false && x.itemname.Contains(key)).ToList();
                    logger.Info("End  Free Stock: ");
                    return stockdata;

                }

            }
            catch (Exception ex)
            {
                logger.Error("Error in Free Stock " + ex.Message);
                logger.Info("End  free Stock: ");
                return null;
            }
        }

        #endregion
        [Route("")]
        // [HttpGet]
        public FreeStockHistorydata GetHistory(int list, int page, string ItemNumber, int WarehouseId, int FreeStockId)
        {

            logger.Info("FreeStock History: ");
            using (AuthContext context = new AuthContext())
            {

                try
                {
                    var identity = User.Identity as ClaimsIdentity;
                    int compid = 0, userid = 0;
                    if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))
                        compid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);

                    if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                        userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

                    int CompanyId = compid;
                    if (CompanyId == 0)
                    {
                        throw new ArgumentNullException("item");
                    }
                    logger.Info("User ID : {0} , Company Id : {1}", compid, userid);
                    // FreeStockHistorydata ass = context.AllfreestockHistory(list, page, ItemNumber, WarehouseId, FreeStockId);
                    logger.Info("End History: ");
                    // return ass;
                    List<FreeStockHistory> newdata = new List<FreeStockHistory>();
                    var listOrders = context.FreeStockHistoryDB.Where(x => x.Deleted == false && x.ItemNumber == ItemNumber && x.WarehouseId == WarehouseId && x.FreeStockId == FreeStockId).OrderByDescending(x => x.CreationDate).Skip((page - 1) * list).Take(list).ToList();
                    for (var i = 0; i < listOrders.ToList().Count(); i++)
                    {
                        int Userid = listOrders[i].userid;
                        var DisplayName = context.Peoples.Where(x => x.PeopleID == Userid).Select(x => x.DisplayName).FirstOrDefault();
                        if (DisplayName != null)
                        {
                            listOrders[i].UserName = DisplayName;
                        }
                        else
                        {
                            listOrders[i].UserName = null;
                        }
                    }
                    newdata = listOrders;
                    FreeStockHistorydata obj = new FreeStockHistorydata();
                    obj.total_count = context.FreeStockHistoryDB.Where(x => x.Deleted == false && x.ItemNumber == ItemNumber && x.WarehouseId == WarehouseId && x.FreeStockId == FreeStockId).Count();
                    obj.freestock = newdata;
                    return obj;


                }
                catch (Exception ex)
                {
                    logger.Error("Error in OrderMaster " + ex.Message);
                    logger.Info("End  OrderMaster: ");
                    return null;
                }
            }
        }

        [Route("addFreeItemOrder")]
        [AcceptVerbs("POST")]
        public HttpResponseMessage addFreeItemOrder(FreeStockDTO item)
        {
            logger.Info("start Return: ");
            using (AuthContext context = new AuthContext())
            {
                try
                {
                    var identity = User.Identity as ClaimsIdentity;
                    int compid = 0, userid = 0;
                    if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))
                        compid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);

                    if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                        userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

                    Int32 OrderId = item.OrderId;
                    var Order = context.DbOrderMaster.Where(x => x.OrderId == item.OrderId).Include("orderDetails").FirstOrDefault();
                    if (Order != null && (Order.Status == "Pending" || Order.Status == "Ready to Dispatch"))
                    {
                        FreeStock freeItem = context.FreeStockDB.FirstOrDefault(t => t.FreeStockId == item.FreeStockId && t.WarehouseId == Order.WarehouseId && t.Deleted == false);
                        var Firstitem = context.itemMasters.FirstOrDefault(x => x.Number == freeItem.ItemNumber && x.ItemMultiMRPId == freeItem.ItemMultiMRPId);
                        if (freeItem != null && freeItem.CurrentInventory >= item.Quantity)
                        {
                            var people = context.Peoples.Where(x => x.PeopleID == userid).FirstOrDefault();
                            OrderDetails Od = context.DbOrderDetails.FirstOrDefault(t => t.OrderId == OrderId && t.WarehouseId == freeItem.WarehouseId && t.Deleted == false && t.IsFreeItem == true && t.itemNumber == freeItem.ItemNumber && t.ItemMultiMRPId == freeItem.ItemMultiMRPId);
                            if (Od == null)
                            {

                                double finalTaxAmount = 0;
                                double finalCessTaxAmount = 0;
                                OrderDetails od1 = new OrderDetails();
                                od1.CustomerId = Order.CustomerId;
                                od1.CustomerName = Order.CustomerName;
                                od1.CityId = Order.CityId;
                                od1.Mobile = Order.Customerphonenum;
                                od1.OrderDate = indianTime;
                                od1.Status = Order.Status;
                                od1.CompanyId = Order.CompanyId;
                                od1.WarehouseId = Order.WarehouseId;
                                od1.WarehouseName = Order.WarehouseName;
                                od1.ItemMultiMRPId = freeItem.ItemMultiMRPId;
                                od1.itemNumber = freeItem.ItemNumber;
                                od1.itemname = freeItem.itemname;
                                od1.CategoryName = Firstitem.CategoryName;
                                od1.SubsubcategoryName = Firstitem.SubsubcategoryName;
                                od1.SubcategoryName = Firstitem.SubcategoryName;
                                od1.itemcode = Firstitem.itemcode;
                                od1.ItemId = Firstitem.ItemId;
                                od1.HSNCode = Firstitem.HSNCode;
                                od1.UnitPrice = 0.01;
                                od1.price = freeItem.MRP;
                                od1.MinOrderQty = 0;
                                od1.MinOrderQtyPrice = 0;
                                od1.qty = item.Quantity;
                                od1.Noqty = od1.qty; // for total qty (no of items)    
                                od1.SizePerUnit = 0;
                                od1.TaxPercentage = 0;
                                od1.SGSTTaxPercentage = 0;
                                od1.CGSTTaxPercentage = 0;
                                od1.TotalAmt = 0;
                                od1.TotalCessPercentage = Firstitem.TotalCessPercentage;
                                od1.AmtWithoutTaxDisc = 0;
                                od1.AmtWithoutAfterTaxDisc = 0;
                                od1.CessTaxAmount = 0;
                                od1.AmtWithoutTaxDisc = 0;
                                od1.AmtWithoutAfterTaxDisc = 0;
                                od1.TaxAmmount = 0;
                                od1.DiscountAmmount = 0;

                                od1.NetAmtAfterDis = 0;

                                od1.Purchaseprice = 0;
                                od1.NetPurchasePrice = 0;
                                od1.TaxPercentage = Firstitem.TotalTaxPercentage;

                                od1.TotalCessPercentage = Firstitem.TotalCessPercentage;

                                if (od1.TaxPercentage >= 0)
                                {
                                    od1.SGSTTaxPercentage = od1.TaxPercentage / 2;
                                    od1.CGSTTaxPercentage = od1.TaxPercentage / 2;
                                }


                                if (od1.TotalCessPercentage > 0)
                                {
                                    od1.TotalCessPercentage = Firstitem.TotalCessPercentage;
                                    double tempPercentagge = Firstitem.TotalCessPercentage + Firstitem.TotalTaxPercentage;

                                    od1.AmtWithoutTaxDisc = ((100 * od1.UnitPrice * od1.qty) / (1 + tempPercentagge / 100)) / 100;
                                    od1.AmtWithoutAfterTaxDisc = (100 * od1.AmtWithoutTaxDisc) / (100 + Firstitem.PramotionalDiscount);
                                    od1.CessTaxAmount = (od1.AmtWithoutAfterTaxDisc * od1.TotalCessPercentage) / 100;
                                }


                                double tempPercentagge2 = Firstitem.TotalCessPercentage + Firstitem.TotalTaxPercentage;
                                od1.AmtWithoutTaxDisc = ((100 * od1.UnitPrice * od1.qty) / (1 + tempPercentagge2 / 100)) / 100;
                                od1.AmtWithoutAfterTaxDisc = (100 * od1.AmtWithoutTaxDisc) / (100 + od1.DiscountPercentage);
                                od1.TaxAmmount = (od1.AmtWithoutAfterTaxDisc * od1.TaxPercentage) / 100;
                                if (od1.TaxAmmount >= 0)
                                {
                                    od1.SGSTTaxAmmount = od1.TaxAmmount / 2;
                                    od1.CGSTTaxAmmount = od1.TaxAmmount / 2;
                                }

                                //for cess
                                if (od1.CessTaxAmount > 0)
                                {
                                    //double temp = pc.TaxPercentage + pc.TotalCessPercentage;
                                    double tempPercentagge3 = Firstitem.TotalCessPercentage + Firstitem.TotalTaxPercentage;
                                    od1.AmtWithoutTaxDisc = ((100 * od1.UnitPrice * od1.qty) / (1 + tempPercentagge3 / 100)) / 100;
                                    od1.AmtWithoutAfterTaxDisc = (100 * od1.AmtWithoutTaxDisc) / (100 + Firstitem.PramotionalDiscount);
                                    od1.TotalAmountAfterTaxDisc = od1.AmtWithoutAfterTaxDisc + od1.CessTaxAmount + od1.TaxAmmount;

                                }
                                else
                                {
                                    od1.TotalAmountAfterTaxDisc = od1.AmtWithoutAfterTaxDisc + od1.TaxAmmount;
                                }
                                if (od1.CessTaxAmount > 0)
                                {
                                    finalCessTaxAmount = finalCessTaxAmount + od1.CessTaxAmount;
                                    finalTaxAmount = finalTaxAmount + od1.TaxAmmount + od1.CessTaxAmount;
                                }
                                else
                                {
                                    finalTaxAmount = finalTaxAmount + od1.TaxAmmount;
                                }
                                od1.TaxAmmount = finalTaxAmount;
                                od1.TotalAmt = System.Math.Round(od1.UnitPrice * od1.qty, 2);

                                od1.CreatedDate = indianTime;
                                od1.UpdatedDate = indianTime;
                                od1.OrderDate = Order.CreatedDate;
                                od1.Deleted = false;
                                od1.marginPoint = 0;
                                od1.OrderId = Order.OrderId;
                                od1.IsFreeItem = true;
                                context.DbOrderDetails.Add(od1);

                                //for OrderUpdate 
                                Order.TaxAmount += od1.TaxAmmount;
                                Order.TotalAmount = od1.TotalAmt;
                                Order.GrossAmount = System.Math.Round(od1.TotalAmt, 2);
                                context.Entry(Order).State = EntityState.Modified;
                                if (item != null && freeItem.CurrentInventory > 0)
                                {
                                    context.Commit();
                                    return Request.CreateResponse(HttpStatusCode.OK, "Item Added Successfully");
                                }
                            }
                            else { return Request.CreateResponse(HttpStatusCode.OK, "Item already added "); }
                        }
                        else { return Request.CreateResponse(HttpStatusCode.OK, "Item Can't Added due to MRP Item Not activated"); }
                    }
                    return Request.CreateResponse(HttpStatusCode.OK, "Item Can't Added due to order processed in Delivery ");
                }

                catch (Exception ex)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex.Message);
                }
            }
        }
        #region transfer current stock to freestock
        /// <summary>
        /// Created Date:30/12/2019
        /// Created By Ashwin
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        [Authorize]
        [Route("TransferToCurrentStock")]
        [HttpPut]
        public HttpResponseMessage TransferToCurrentStock(TransferToFreestockDTO item)
        {
            var result = "";
            var identity = User.Identity as ClaimsIdentity;
            int compid = 0, userid = 0;
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))
                compid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);

            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
            TransactionOptions option = new TransactionOptions();
            option.IsolationLevel = IsolationLevel.RepeatableRead;
            option.Timeout = TimeSpan.FromSeconds(90);
            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, option))
            {
                using (AuthContext db = new AuthContext())
                {
                    var warehouse = db.Warehouses.FirstOrDefault(x => x.WarehouseId == item.WarehouseId);

                    if (warehouse.IsStopCurrentStockTrans)
                        return Request.CreateResponse(HttpStatusCode.OK, "Inventory Transactions are currently disabled for this warehouse... Please try after some time");

                    var User = db.Peoples.Where(x => x.PeopleID == userid && x.Active == true).SingleOrDefault();
                    if (User != null)
                    {
                        FreeStock FreeStock = db.FreeStockDB.Where(c => c.ItemNumber == item.ItemNumber && c.Deleted == false && c.WarehouseId == item.WarehouseId && c.ItemMultiMRPId == item.ItemMultiMRPId).FirstOrDefault();

                        if (FreeStock != null && FreeStock.CurrentInventory >= item.Transferinventory)
                        {
                            CurrentStock CurrentStock = db.DbCurrentStock.Where(x => x.ItemNumber == FreeStock.ItemNumber && x.WarehouseId == FreeStock.WarehouseId && x.ItemMultiMRPId == FreeStock.ItemMultiMRPId).FirstOrDefault();
                            if (CurrentStock != null)
                            {
                            }
                            else
                            {
                                ItemMultiMRP ItemMultiMRP = db.ItemMultiMRPDB.Where(a => a.ItemMultiMRPId == FreeStock.ItemMultiMRPId).SingleOrDefault();
                                if (ItemMultiMRP != null)
                                {
                                    CurrentStock CurrentStockobj = new CurrentStock();
                                    CurrentStockobj.ItemNumber = ItemMultiMRP.ItemNumber;
                                    CurrentStockobj.itemname = FreeStock.itemname;
                                    CurrentStockobj.ItemMultiMRPId = ItemMultiMRP.ItemMultiMRPId;
                                    CurrentStockobj.MRP = ItemMultiMRP.MRP;
                                    CurrentStockobj.WarehouseId = Convert.ToInt32(CurrentStock.WarehouseId);
                                    CurrentStockobj.CurrentInventory = 0;// item.Transferinventory;
                                    CurrentStockobj.CreatedBy = User.DisplayName;
                                    CurrentStockobj.CreationDate = indianTime;
                                    CurrentStockobj.Deleted = false;
                                    CurrentStockobj.UpdatedDate = indianTime;
                                    db.DbCurrentStock.Add(CurrentStockobj);
                                    db.Commit();

                                }
                            }
                            StockTransactionHelper helper = new StockTransactionHelper();

                            PhysicalStockUpdateRequestDc StockTransferToFree = new PhysicalStockUpdateRequestDc();
                            StockTransferToFree.ItemMultiMRPId = CurrentStock.ItemMultiMRPId;
                            StockTransferToFree.WarehouseId = CurrentStock.WarehouseId;
                            StockTransferToFree.Qty = item.Transferinventory;
                            StockTransferToFree.SourceStockType = StockTypeTableNames.FreebieStock;// "FreeStocks";
                            StockTransferToFree.DestinationStockType = StockTypeTableNames.CurrentStocks;// "CurrentStocks";
                            StockTransferToFree.Reason = "Stock In from FreeStock to CurrentStock Due to :" + item.ManualReason;
                            StockTransferToFree.StockTransferType = StockTransferTypeName.ManualInventory;
                            bool isupdated = helper.TransferBetweenPhysicalStocks(StockTransferToFree, userid, db, scope);
                            if (isupdated)
                            {
                                scope.Complete();
                                result = "Transaction Saved Successfully";

                                #region Insert in FIFO
                                if (ConfigurationManager.AppSettings["LiveFIFO"] == "1")
                                {
                                    GrDC items = new GrDC
                                    {
                                        ItemMultiMrpId = StockTransferToFree.ItemMultiMRPId,
                                        WarehouseId = StockTransferToFree.WarehouseId,
                                        Source = "FreeInCS",
                                        CreatedDate = indianTime,
                                        POId = 0,
                                        Qty = StockTransferToFree.Qty,
                                        Price = 0,
                                    };
                                    RabbitMqHelper rabbitMqHelper = new RabbitMqHelper();
                                    rabbitMqHelper.Publish("FromFreeStock", items); 
                                }
                                #endregion


                            }
                            else
                            {
                                scope.Dispose();
                                result = "one of the stock not available";
                            }
                        }
                        else
                        {
                            result = "FreeStock Qty not availble to transfer in CurrentStock of Qty: " + item.Transferinventory;
                        }
                    }
                    else
                    {
                        result = "Something went wrong";
                    }
                }
            }
            return Request.CreateResponse(HttpStatusCode.OK, result);
            #region old code
            //var result = "";
            //var identity = User.Identity as ClaimsIdentity;
            //int compid = 0, userid = 0;
            //int Warehouse_id = 0;
            //if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))
            //    compid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);

            //if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
            //    userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

            //if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "Warehouseid"))
            //    Warehouse_id = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "Warehouseid").Value);
            //using (AuthContext db = new AuthContext())
            //{
            //    var User = db.Peoples.Where(x => x.PeopleID == userid && x.Active == true).SingleOrDefault();
            //    if (User != null)
            //    {
            //        FreeStock FreeStock = db.FreeStockDB.Where(c => c.ItemNumber == item.ItemNumber && c.Deleted == false && c.WarehouseId == item.WarehouseId && c.ItemMultiMRPId == item.ItemMultiMRPId).FirstOrDefault();

            //        if (FreeStock != null && FreeStock.CurrentInventory >= item.Transferinventory)
            //        {
            //            Warehouse Warehouse = db.Warehouses.Where(z => z.WarehouseId == item.WarehouseId).FirstOrDefault();
            //            FreeStockHistory freeStockHistory = new FreeStockHistory();
            //            freeStockHistory.CreationDate = indianTime;
            //            freeStockHistory.FreeStockId = FreeStock.FreeStockId;
            //            freeStockHistory.ItemMultiMRPId = FreeStock.ItemMultiMRPId;
            //            freeStockHistory.itemname = FreeStock.itemname;
            //            freeStockHistory.ItemNumber = FreeStock.ItemNumber;
            //            freeStockHistory.CurrentInventory = FreeStock.CurrentInventory - item.Transferinventory;
            //            freeStockHistory.userid = userid;
            //            freeStockHistory.TotalInventory = FreeStock.CurrentInventory - item.Transferinventory;
            //            //freeStockHistory.InventoryOut = item.Transferinventory;
            //            freeStockHistory.ManualInventoryIn = item.Transferinventory*(-1);
            //            freeStockHistory.ManualReason = " Stock transfer to Current Stock Due to: " + item.ManualReason;
            //            freeStockHistory.WarehouseId = Warehouse.WarehouseId;
            //            db.FreeStockHistoryDB.Add(freeStockHistory);

            //            CurrentStock CurrentStock = db.DbCurrentStock.Where(x => x.ItemNumber == freeStockHistory.ItemNumber && x.WarehouseId == freeStockHistory.WarehouseId && x.ItemMultiMRPId == freeStockHistory.ItemMultiMRPId).FirstOrDefault();
            //            if (CurrentStock != null)
            //            {

            //                CurrentStock.CurrentInventory = CurrentStock.CurrentInventory + item.Transferinventory;
            //                if (CurrentStock.CurrentInventory < 0)
            //                {
            //                    CurrentStock.CurrentInventory = 0;
            //                }
            //                db.Entry(CurrentStock).State = EntityState.Modified;


            //                CurrentStockHistory CurrentStockHistory = new CurrentStockHistory();
            //                CurrentStockHistory.updationDate = indianTime;
            //                CurrentStockHistory.CreationDate = indianTime;
            //                CurrentStockHistory.StockId = CurrentStock.StockId;
            //                CurrentStockHistory.ItemMultiMRPId = CurrentStock.ItemMultiMRPId;
            //                CurrentStockHistory.itemname = CurrentStock.itemname;
            //                CurrentStockHistory.ItemNumber = CurrentStock.ItemNumber;
            //                CurrentStockHistory.CurrentInventory = CurrentStock.CurrentInventory;
            //                CurrentStockHistory.UserName = User.DisplayName;
            //                CurrentStockHistory.userid = userid;
            //                CurrentStockHistory.CompanyId = compid;
            //                CurrentStockHistory.TotalInventory = CurrentStock.CurrentInventory;
            //                //CurrentStockHistory.InventoryIn = item.Transferinventory;
            //                CurrentStockHistory.ManualInventoryIn = item.Transferinventory;
            //                CurrentStockHistory.ManualReason = "Stock In from FreeStock Due to :" + item.ManualReason;
            //                CurrentStockHistory.Warehouseid = Warehouse.WarehouseId;
            //                CurrentStockHistory.WarehouseName = Warehouse.WarehouseName;
            //                db.CurrentStockHistoryDb.Add(CurrentStockHistory);


            //            }
            //            else
            //            {
            //                ItemMultiMRP ItemMultiMRP = db.ItemMultiMRPDB.Where(a => a.ItemMultiMRPId == freeStockHistory.ItemMultiMRPId).SingleOrDefault();
            //                if (ItemMultiMRP != null)
            //                {
            //                    CurrentStock CurrentStockobj = new CurrentStock();
            //                    CurrentStockobj.ItemNumber = ItemMultiMRP.ItemNumber;
            //                    CurrentStockobj.itemname = freeStockHistory.itemname;
            //                    CurrentStockobj.ItemMultiMRPId = ItemMultiMRP.ItemMultiMRPId;
            //                    CurrentStockobj.MRP = ItemMultiMRP.MRP;
            //                    CurrentStockobj.WarehouseId = Convert.ToInt32(CurrentStock.WarehouseId);
            //                    CurrentStockobj.CurrentInventory = item.Transferinventory;
            //                    CurrentStockobj.CreatedBy = User.DisplayName;
            //                    CurrentStockobj.CreationDate = indianTime;
            //                    CurrentStockobj.Deleted = false;
            //                    CurrentStockobj.UpdatedDate = indianTime;
            //                    db.DbCurrentStock.Add(CurrentStockobj);
            //                    db.Commit();

            //                    CurrentStockHistory CurrentStockHistory = new CurrentStockHistory();
            //                    CurrentStockHistory.ManualReason = "Stock In from CurrentStock Due to :" + item.ManualReason;
            //                    CurrentStockHistory.StockId = CurrentStockobj.StockId;
            //                    CurrentStockHistory.ItemMultiMRPId = CurrentStockobj.ItemMultiMRPId;
            //                    CurrentStockHistory.ItemNumber = CurrentStockobj.ItemNumber;
            //                    CurrentStockHistory.itemname = CurrentStockobj.itemname;
            //                    CurrentStockHistory.CurrentInventory = CurrentStockobj.CurrentInventory;
            //                    //CurrentStockHistory.InventoryIn = CurrentStockobj.CurrentInventory;
            //                    CurrentStockHistory.ManualInventoryIn = CurrentStockobj.CurrentInventory;
            //                    CurrentStockHistory.TotalInventory = Convert.ToInt32(CurrentStockobj.CurrentInventory);
            //                    CurrentStockHistory.Warehouseid = CurrentStockobj.WarehouseId;
            //                    CurrentStockHistory.CreationDate = indianTime;
            //                    CurrentStockHistory.userid = userid;
            //                    CurrentStockHistory.CreationDate = indianTime;
            //                    db.CurrentStockHistoryDb.Add(CurrentStockHistory);

            //                }

            //            }

            //            int CurrentInventory = FreeStock.CurrentInventory;//current inventory
            //            int ManualUpdateInventory = FreeStock.CurrentInventory - item.Transferinventory;// New Added Inventory

            //            FreeStock.ManualReason = "Stock transfer to CurrentStock= " + item.ManualReason;
            //            FreeStock.UpdatedDate = indianTime;
            //            FreeStock.CurrentInventory -= item.Transferinventory;
            //            db.Entry(FreeStock).State = EntityState.Modified;
            //            if (db.Commit() > 0)
            //            {
            //                result = item.Transferinventory + " Qty Transfered to Current Stock Successfully";
            //                return Request.CreateResponse(HttpStatusCode.OK, result);
            //            };
            //        }
            //        else
            //        {
            //            result = "FreeStock Qty not availble to transfer in CurrentStock of Qty: " + item.Transferinventory;
            //            return Request.CreateResponse(HttpStatusCode.OK, result);
            //        }
            //    }
            //}
            //result = "Something went wrong";
            //return Request.CreateResponse(HttpStatusCode.OK, result);
            #endregion
        }


        [Authorize]
        [Route("TransferToCurrentStockV7")]
        [HttpPut]
        public HttpResponseMessage TransferToCurrentStockV7(List<TransferToFreestockDTO> item)
        {
            var result = "";
            var identity = User.Identity as ClaimsIdentity;
            int compid = 0, userid = 0;
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))
                compid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);

            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
            TransactionOptions option = new TransactionOptions();
            option.IsolationLevel = IsolationLevel.RepeatableRead;
            option.Timeout = TimeSpan.FromSeconds(90);
            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, option))
            {
                using (AuthContext db = new AuthContext())
                {
                    int wid = item.First().WarehouseId;
                    var warehouse = db.Warehouses.FirstOrDefault(x => x.WarehouseId == wid);

                    if (warehouse.IsStopCurrentStockTrans)
                        return Request.CreateResponse(HttpStatusCode.OK, "Inventory Transactions are currently disabled for this warehouse... Please try after some time");

                    var User = db.Peoples.Where(x => x.PeopleID == userid && x.Active == true).SingleOrDefault();
                    if (User != null)
                    {
                        foreach (var itemNew in item)
                        {
                            var query = from s in db.StockBatchMasters
                                        join f in db.FreeStockDB
                                        on s.StockId equals f.FreeStockId
                                        where (s.StockType == "F" && s.Id == itemNew.StockBatchMasterId)
                                        select f;
                            FreeStock  freeStock = query.FirstOrDefault();


                            FreeStock FreeStock = db.FreeStockDB.Where(c => c.ItemNumber == itemNew.ItemNumber && c.Deleted == false && c.WarehouseId == itemNew.WarehouseId && c.ItemMultiMRPId == itemNew.ItemMultiMRPId).FirstOrDefault();

                            if (FreeStock != null && FreeStock.CurrentInventory >= itemNew.Transferinventory)
                            {
                                CurrentStock CurrentStock = db.DbCurrentStock.Where(x => x.ItemNumber == FreeStock.ItemNumber && x.WarehouseId == FreeStock.WarehouseId && x.ItemMultiMRPId == FreeStock.ItemMultiMRPId).FirstOrDefault();
                                if (CurrentStock != null)
                                {
                                }
                                else
                                {
                                    ItemMultiMRP ItemMultiMRP = db.ItemMultiMRPDB.Where(a => a.ItemMultiMRPId == FreeStock.ItemMultiMRPId).SingleOrDefault();
                                    if (ItemMultiMRP != null)
                                    {
                                        CurrentStock CurrentStockobj = new CurrentStock();
                                        CurrentStockobj.ItemNumber = ItemMultiMRP.ItemNumber;
                                        CurrentStockobj.itemname = FreeStock.itemname;
                                        CurrentStockobj.ItemMultiMRPId = ItemMultiMRP.ItemMultiMRPId;
                                        CurrentStockobj.MRP = ItemMultiMRP.MRP;
                                        CurrentStockobj.WarehouseId = Convert.ToInt32(CurrentStock.WarehouseId);
                                        CurrentStockobj.CurrentInventory = 0;// item.Transferinventory;
                                        CurrentStockobj.CreatedBy = User.DisplayName;
                                        CurrentStockobj.CreationDate = indianTime;
                                        CurrentStockobj.Deleted = false;
                                        CurrentStockobj.UpdatedDate = indianTime;
                                        db.DbCurrentStock.Add(CurrentStockobj);
                                        db.Commit();

                                    }
                                }
                                StockTransactionHelper helper = new StockTransactionHelper();

                                PhysicalStockUpdateRequestDc StockTransferToFree = new PhysicalStockUpdateRequestDc();
                                StockTransferToFree.ItemMultiMRPId = CurrentStock.ItemMultiMRPId;
                                StockTransferToFree.WarehouseId = CurrentStock.WarehouseId;
                                StockTransferToFree.Qty = itemNew.Transferinventory;
                                StockTransferToFree.SourceStockType = StockTypeTableNames.FreebieStock;// "FreeStocks";
                                StockTransferToFree.DestinationStockType = StockTypeTableNames.CurrentStocks;// "CurrentStocks";
                                StockTransferToFree.Reason = "Stock In from FreeStock to CurrentStock Due to :" + itemNew.ManualReason;
                                StockTransferToFree.StockTransferType = StockTransferTypeName.ManualInventory;
                                bool isupdated = helper.TransferBetweenPhysicalStocks(StockTransferToFree, userid, db, scope);
                                BatchMasterManager batchMasterManager = new BatchMasterManager();
                                bool isSuccess = batchMasterManager.MoveDirectBatchItemInSameBatch("F", "C", itemNew.Transferinventory, itemNew.StockBatchMasterId, itemNew.ItemMultiMRPId, itemNew.WarehouseId, db, userid);
                                if (isupdated && isSuccess)
                                {
                                    //scope.Complete();
                                    //result = "Transaction Saved Successfully";

                                    #region Insert in FIFO
                                    if (ConfigurationManager.AppSettings["LiveFIFO"] == "1")
                                    {
                                        GrDC items = new GrDC
                                        {
                                            ItemMultiMrpId = StockTransferToFree.ItemMultiMRPId,
                                            WarehouseId = StockTransferToFree.WarehouseId,
                                            Source = "FreeInCS",
                                            CreatedDate = indianTime,
                                            POId = 0,
                                            Qty = StockTransferToFree.Qty,
                                            Price = 0,
                                        };
                                        RabbitMqHelper rabbitMqHelper = new RabbitMqHelper();
                                        rabbitMqHelper.Publish("FromFreeStock", items);
                                    }
                                    #endregion


                                }
                                else
                                {
                                    scope.Dispose();
                                    if (!isupdated)
                                        result = "one of the stock not available";
                                    if (!isSuccess)
                                        result = "currently stock movement in process for same ItemMultiMRP and Warehouse. Please try after some time.";

                                    Request.CreateResponse(HttpStatusCode.OK, result);
                                }
                                result = itemNew.Transferinventory + " Qty Transfered to CurrentStock Successfully";
                            }
                            else
                            {
                                result = "FreeStock Qty not availble to transfer in CurrentStock of Qty: " + itemNew.Transferinventory;
                            }
                        }

                        try
                        {
                            db.Commit();
                            scope.Complete();
                            result = "Transaction Saved Successfully";

                        }
                        catch (Exception ex)
                        {
                            //Console.WriteLine(ex);
                            result = "Something went wrong!";
                        }

                    }
                    else
                    {
                        result = "Something went wrong";
                    }
                }
            }
            return Request.CreateResponse(HttpStatusCode.OK, result);            
        }
        #endregion
        //#region transfer Free stock Inventory
        ///// <summary>
        ///// Created Date:17/09/2019
        ///// Created By Vinayak
        ///// </summary>
        ///// <param name="item"></param>
        ///// <returns></returns>
        //[Authorize]
        //[Route("FreeItemStockTransfer")]
        //[HttpPut]
        //public TransferFreeStockDTO Put(TransferFreeStockDTO item)
        //{

        //    try
        //    {
        //        var identity = User.Identity as ClaimsIdentity;
        //        int compid = 0, userid = 0;
        //        int Warehouse_id = 0;
        //        if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))
        //            compid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);

        //        if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
        //            userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

        //        if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "Warehouseid"))
        //            Warehouse_id = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "Warehouseid").Value);
        //        using (AuthContext db = new AuthContext())
        //        {
        //            var UserName = db.Peoples.Where(x => x.PeopleID == userid).FirstOrDefault();

        //            FreeStock stockplus = db.FreeStockDB.Where(c => c.ItemNumber == item.ItemNumberTrans && c.Deleted == false && c.WarehouseId == item.WarehouseId && c.ItemMultiMRPId == item.ItemMultiMRPIdTrans).FirstOrDefault();
        //            Warehouse whdata = db.Warehouses.Where(z => z.WarehouseId == item.WarehouseId).FirstOrDefault();

        //            if (stockplus != null)
        //            {
        //                FreeStockHistory csht = new FreeStockHistory();
        //                csht.FreeStockId = stockplus.FreeStockId;
        //                csht.ItemMultiMRPId = stockplus.ItemMultiMRPId;
        //                csht.itemname = stockplus.itemname;
        //                csht.ItemNumber = stockplus.ItemNumber;
        //                csht.CurrentInventory = stockplus.CurrentInventory + item.CurrentInventory;
        //                csht.userid = UserName.PeopleID;
        //                csht.TotalInventory = stockplus.CurrentInventory + item.CurrentInventory;
        //                csht.InventoryIn = item.CurrentInventory;
        //                csht.ManualReason = "Stock transfer=" + item.ManualReason;
        //                csht.CreationDate = DateTime.Now;
        //                csht.WarehouseId = whdata.WarehouseId;
        //                db.FreeStockHistoryDB.Add(csht);

        //                int CurrentInventory = stockplus.CurrentInventory;//current inventory
        //                int ManualUpdateInventory = stockplus.CurrentInventory + item.CurrentInventory;// New Added Inventory

        //                stockplus.UpdatedDate = DateTime.Now;
        //                stockplus.CurrentInventory += item.CurrentInventory;
        //                db.Entry(stockplus).State = EntityState.Modified;
        //                db.Commit();
        //            }

        //            FreeStock stockIsless = db.FreeStockDB.Where(c => c.ItemNumber == item.ItemNumber && c.Deleted == false && c.WarehouseId == item.WarehouseId && c.ItemMultiMRPId == item.ItemMultiMRPId).FirstOrDefault();
        //            if (stockIsless != null)
        //            {
        //                FreeStockHistory cshtout = new FreeStockHistory();
        //                cshtout.ItemMultiMRPId = stockIsless.ItemMultiMRPId;
        //                cshtout.itemname = stockIsless.itemname;
        //                cshtout.ItemNumber = stockIsless.ItemNumber;
        //                cshtout.FreeStockId = stockIsless.FreeStockId;
        //                cshtout.CurrentInventory = stockIsless.CurrentInventory - item.CurrentInventory;
        //                cshtout.userid = UserName.PeopleID;
        //                cshtout.TotalInventory = stockIsless.CurrentInventory - item.CurrentInventory;

        //                cshtout.InventoryOut = item.CurrentInventory;

        //                cshtout.ManualReason = "Stock transfer= " + item.ManualReason;

        //                cshtout.CreationDate = DateTime.Now;
        //                cshtout.WarehouseId = whdata.WarehouseId;

        //                db.FreeStockHistoryDB.Add(cshtout);


        //                int CurrentInventory = stockIsless.CurrentInventory;//current inventory
        //                int ManualUpdateInventory = stockIsless.CurrentInventory - item.CurrentInventory;// New Added Inventory

        //                stockIsless.UpdatedDate = DateTime.Now;
        //                stockIsless.CurrentInventory -= item.CurrentInventory;
        //                db.Entry(stockIsless).State = EntityState.Modified;
        //                db.Commit();

        //            }
        //            logger.Info("sucessfully update: " + stockplus);
        //        }
        //        return item;
        //    }
        //    catch (Exception ex)
        //    {
        //        logger.Error("Error in current stock " + ex.Message);
        //        return null;

        //    }
        //}
        //#endregion  
        #region get free Item Quantity 
        /// <summary>
        /// Created By Raj
        /// Created Date:18/09/2019
        /// </summary>
        /// <param name="freeItemId"></param>
        /// <returns></returns>

        [Authorize]
        [Route("GetFreeItemQuantity")]
        [HttpGet]
        public FreeStockDTO GetFreeItem(int freeItemId)
        {
            FreeStockDTO freeStockDTO = null;
            using (var context = new AuthContext())
            {
                var itemInfo = context.itemMasters.Where(a => a.active && a.Deleted == false && a.ItemId == freeItemId).Select(x => new { x.ItemMultiMRPId, x.Number, x.WarehouseId }).FirstOrDefault();
                if (itemInfo != null)
                {
                    var freeStockdetails = context.FreeStockDB.Where(a => a.ItemNumber == itemInfo.Number && a.WarehouseId == itemInfo.WarehouseId && a.ItemMultiMRPId == itemInfo.ItemMultiMRPId).Select(x => new { x.CurrentInventory, x.FreeStockId }).FirstOrDefault();


                    if (freeStockdetails != null)
                    {
                        freeStockDTO = new FreeStockDTO();
                        freeStockDTO.FreeStockId = freeStockdetails.FreeStockId;
                        freeStockDTO.Quantity = freeStockdetails.CurrentInventory;
                        freeStockDTO.OrderId = 0;
                    }
                }
                return freeStockDTO;
            }
        }
        #endregion

        #region
        [Route("Export")]
        [HttpGet]
        public HttpResponseMessage GetExport(int StockId, int WarehouseId)
        {
            using (AuthContext db = new AuthContext())
            {
                List<FreeStockHistory> Data = new List<FreeStockHistory>();
                Data = db.FreeStockHistoryDB.Where(x => x.FreeStockId == StockId && x.WarehouseId == WarehouseId).OrderByDescending(x => x.CreationDate).ToList();

                if (Data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, "No record");
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.OK, Data);
                }
            }

        }

        #endregion






        public class FreeStockDTO
        {
            public int OrderId { get; set; }
            public int FreeStockId { get; set; }
            public int Quantity { get; set; }

        }

        public class WarehouseBasedfreeStockDTO
        {
            public int PlannedStock { get; set; }

            public int FreeStockId { get; set; }
            public int WarehouseId { get; set; }
            public string WarehouseName { get; set; }
            public string ItemNumber { get; set; }
            public int ItemMultiMRPId { get; set; }
            public int CurrentInventory { get; set; }
            public string itemname { get; set; }
            public double MRP { get; set; }
            public bool Deleted { get; set; }
            public int CreatedBy { get; set; }
            public DateTime CreationDate { get; set; }
            public DateTime UpdatedDate { get; set; }

        }
        public class TransferFreeStockDTO
        {
            public string ItemNumber { get; set; }
            public string ItemNumberTrans { get; set; }
            public int ItemMultiMRPId { get; set; }
            public int ItemMultiMRPIdTrans { get; set; }
            public int WarehouseId { get; set; }
            public int CurrentInventory { get; set; }
            public string ManualReason { get; set; }
        }

        public class TransferToFreestockDTO
        {
            public long StockBatchMasterId { get; set; }
            public string BatchCode { get; set; }
            public string ItemNumber { get; set; }
            public int ItemMultiMRPId { get; set; }
            public int WarehouseId { get; set; }
            public int Transferinventory { get; set; }
            public string ManualReason { get; set; }
        }
    }
}
