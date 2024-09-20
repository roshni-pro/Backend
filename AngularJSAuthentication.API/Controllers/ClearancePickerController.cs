

using AngularJSAuthentication.DataContracts.Masters;
using AngularJSAuthentication.Model;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Web.Http;
using System.Threading.Tasks;
using static AngularJSAuthentication.API.Controllers.InventoryCycleController;
using System.Transactions;
using AngularJSAuthentication.API.Helper;
using AngularJSAuthentication.DataContracts.Transaction.Stocks;
using AngularJSAuthentication.DataContracts.constants;
using AngularJSAuthentication.API.Managers;
using System.Configuration;
using AngularJSAuthentication.Common.Helpers;
using System.Data;
using System.Data.Entity.Infrastructure;

namespace AngularJSAuthentication.API.Controllers
{
    [RoutePrefix("api/ClearancePicker")]
    [Authorize]
    public class ClearancePickerController : ApiController
    {
        private static TimeZoneInfo INDIAN_ZONE = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
        DateTime indianTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE);


        // User for both Clearance (Approved)  0 and Physically moved 1
        [AllowAnonymous]
        [Route("GetClearancePickerList")]
        [HttpPost]
        public async Task<List<ClearancePickerDc>> GetClearancePickerList(SearchClearancePickerDc SearchClearancePicker)
        {
            var result = new List<ClearancePickerDc>();
            using (var context = new AuthContext())
            {
                var keyword = new SqlParameter("@keyword", SearchClearancePicker.keyword);
                var Skip = new SqlParameter("@skip", SearchClearancePicker.skip);
                var Take = new SqlParameter("@take", SearchClearancePicker.take);
                var WarehousId = new SqlParameter("@warehouseid", SearchClearancePicker.warehouseid);
                var Status = new SqlParameter("@status", SearchClearancePicker.status);
                result = await context.Database.SqlQuery<ClearancePickerDc>("exec Clearance.GetSearchClearancePicker @keyword,@skip,@take,@warehouseid ,@status ", keyword, Skip, Take, WarehousId, Status).ToListAsync();
            }
            return result;
        }

        [Route("ClearancePickerStart/{Id}/{UserId}")]
        [HttpGet]
        public async Task<response> ClearancePickerStart(long Id, int UserId)
        {
            var result = new response();
            if (Id > 0 && UserId > 0)
            {
                using (var context = new AuthContext())
                {
                    bool Isvalid = false;
                    if (await context.ClearancePickerTimers.AnyAsync(x => x.ClearanceNonSaleableId != Id && x.EndDate == null && x.CreatedBy == UserId))
                    {
                        Isvalid = true;
                        result.Status = false;
                        result.msg = "You can't start picker, due to previous already running.";
                    }
                    if (await context.ClearancePickerTimers.AnyAsync(x => x.ClearanceNonSaleableId == Id) || Isvalid == true)
                    {
                        result.Status = false;
                        result.msg = "Picker already running in another job.";
                    }
                    else
                    {
                        context.ClearancePickerTimers.Add(new ClearancePickerTimer

                        {
                            ClearanceNonSaleableId = Id,
                            CreatedBy = UserId,
                            CreatedDate = DateTime.Now,
                            StartDate = DateTime.Now,
                        });
                        result.Status = context.Commit() > 0;
                        result.msg = "Clearance Picker started.";
                    }
                }
            }
            return result;
        }

        [Route("ClearancePickerAcceptReject")]
        [HttpPost]
        public async Task<response> ClearancePickerAcceptReject(ClearancePickerAcceptRejDc ClearancePickerAcceptRej)
        {
            var result = new response();

            if (ClearancePickerAcceptRej != null && ClearancePickerAcceptRej.Id > 0 && ClearancePickerAcceptRej.UserId > 0)
            {
                using (var context = new AuthContext())
                {
                    var ClItems = await context.ClearanceNonSaleableMovementOrderDetails.FirstOrDefaultAsync(x => x.Id == ClearancePickerAcceptRej.Id);
                    var status = await context.ClearanceNonSaleableMovementOrders.Where(x => x.Id == ClItems.ClearanceStockMovementOrderMasterId).Select(x => x.Status).FirstOrDefaultAsync();
                    if (ClItems != null && status == "Approved")
                    {
                        ClItems.Status = ClearancePickerAcceptRej.Status;
                        ClItems.ModifiedBy = ClearancePickerAcceptRej.UserId;
                        ClItems.ModifiedDate = DateTime.Now;
                        ClItems.Comment = ClearancePickerAcceptRej.Comment;
                        context.Entry(ClItems).State = EntityState.Modified;

                        if (await context.ClearanceNonSaleableMovementOrderDetails.AnyAsync(x => x.Id != ClearancePickerAcceptRej.Id && x.ClearanceStockMovementOrderMasterId == ClItems.ClearanceStockMovementOrderMasterId && x.Quantity == 0 && x.Status == 0))
                        {
                            var ZeroClItems = await context.ClearanceNonSaleableMovementOrderDetails.Where(x => x.Id != ClearancePickerAcceptRej.Id && x.ClearanceStockMovementOrderMasterId == ClItems.ClearanceStockMovementOrderMasterId && x.Quantity == 0 && x.Status == 0).ToListAsync();
                            if (ZeroClItems != null && ZeroClItems.Any())
                            {
                                foreach (var item in ZeroClItems)
                                {
                                    item.Status = 2; //rejected
                                    item.ModifiedBy = ClearancePickerAcceptRej.UserId;
                                    item.ModifiedDate = DateTime.Now;
                                    item.Comment = "Auto Reject due to zero qty approved";
                                    context.Entry(item).State = EntityState.Modified;
                                }
                            }
                        }
                        result.Status = context.Commit() > 0;
                        result.msg = "request updated successfully.";
                    }
                    else
                    {
                        result.msg = "something went wrong .";
                    }
                }
            }

            return result;
        }

        [Route("GetClearancePickerDetails/{ClNonSaleableMovementOrderId}")]
        [HttpGet]
        public async Task<List<ClearancePickerDetailsDc>> GetClearancePickerDetails(long ClNonSaleableMovementOrderId)
        {
            var result = new List<ClearancePickerDetailsDc>();
            using (var context = new AuthContext())
            {
                var id = new SqlParameter("@Id", ClNonSaleableMovementOrderId);
                result = await context.Database.SqlQuery<ClearancePickerDetailsDc>("exec Clearance.GetClearancePickerDetails @Id", id).ToListAsync();
                var itemNumbers = result.Select(x => x.ItemNumber).Distinct().ToList();
                var itembarcodelist = context.ItemBarcodes.Where(c => itemNumbers.Contains(c.ItemNumber) && c.IsActive == true && c.IsDeleted == false).Select(x => new ItemBarcodeDc { ItemNumber = x.ItemNumber, Barcode = x.Barcode }).ToList();
                if (itembarcodelist != null && itembarcodelist.Any())
                {
                    result.ForEach(y =>
                    {
                        y.Barcode = (itembarcodelist != null && itembarcodelist.Any(x => x.ItemNumber == y.ItemNumber)) ? itembarcodelist.Where(x => x.ItemNumber == y.ItemNumber).Select(x => x.Barcode).ToList() : null;
                    });
                }
            }
            return result;
        }


        [Route("ClearancePickerSubmit")]
        [HttpGet]
        public async Task<response> ClearancePickerSubmit(long Id, int UserId)
        {
            var result = new response();
            if (Id > 0 && UserId > 0)
            {
                TransactionOptions option = new TransactionOptions();
                option.IsolationLevel = System.Transactions.IsolationLevel.RepeatableRead; //System.Data.IsolationLevel.RepeatableRead;
                option.Timeout = TimeSpan.FromSeconds(120);
                using (var dbContextTransaction = new TransactionScope(TransactionScopeOption.RequiresNew, option))
                {
                    using (var context = new AuthContext())
                    {
                        if (context.ClearanceNonSaleableMovementOrderDetails.Any(x => x.ClearanceStockMovementOrderMasterId == Id && x.Status == 0))
                        {
                            result.Status = false;
                            result.msg = "Clearance picker can't submit, due to some item still pending.";
                            return result;
                        };
                        var ClOrder = context.ClearanceNonSaleableMovementOrders.Where(x => x.Id == Id).FirstOrDefault();
                        if (ClOrder != null && ClOrder.Status != "Approved")
                        {
                            result.Status = false;
                            result.msg = (ClOrder.Status == "Physically Moved") ? "Clearance picker already submitted" : "Clearance picker can't submit, due to status in :" + ClOrder.Status;
                            return result;
                        }
                        if (ClOrder != null && ClOrder.Status == "Approved")
                        {
                            ClOrder.Status = "Physically Moved";
                            ClOrder.ModifiedDate = DateTime.Now;
                            ClOrder.ModifiedBy = UserId;

                            result.Status = context.Commit() > 0;

                            var cllineitems = context.ClearanceNonSaleableMovementOrderDetails.Where(x => x.ClearanceStockMovementOrderMasterId == Id).ToList();
                            var clearancePickerTimer = context.ClearancePickerTimers.Where(x => x.ClearanceNonSaleableId == Id).FirstOrDefault();
                            var StockBatchMasterIds = cllineitems.Select(x => x.StockBatchMasterId);
                            var CurrentStockBatchMasters = context.StockBatchMasters.Where(x => StockBatchMasterIds.Contains(x.Id)).ToList();

                            //cp to Cl
                            StockTransactionHelper sthelper = new StockTransactionHelper();
                            BatchMasterManager batchMasterManager = new BatchMasterManager();

                            var ToPicker = context.Peoples.Where(x => x.PeopleID == UserId && x.Active == true).FirstOrDefault();
                            string ToBuyer = context.Peoples.Where(x => x.PeopleID == ClOrder.Approvedby && x.Active == true).Select(x => x.Email).FirstOrDefault();
                            var WarehouseName = context.Warehouses.FirstOrDefault(x => x.WarehouseId == ClOrder.WarehouseId && x.active == true);
                            if (ClOrder.OrderType == "CurrentToClearance")
                            {
                                if (cllineitems.Any(x => x.Status == 1))
                                {
                                    List<PhysicalStockUpdateRequestDc> manualStockUpdateDcList = new List<PhysicalStockUpdateRequestDc>();
                                    List<TransferStockDTONew> transferStockList = new List<TransferStockDTONew>();

                                    foreach (var item in cllineitems.Where(x => x.Status == 1))
                                    {
                                        long BatchId = CurrentStockBatchMasters.FirstOrDefault(x => x.Id == item.StockBatchMasterId).BatchMasterId;
                                        long StockBatchMasterId = batchMasterManager.GetOrCreate(item.ItemMultiMRPId, ClOrder.WarehouseId, "CL", BatchId, context, UserId);

                                        PhysicalStockUpdateRequestDc manualStockUpdateDc = new PhysicalStockUpdateRequestDc
                                        {
                                            ItemMultiMRPId = item.ItemMultiMRPId,
                                            Reason = "ClearancePlannedToClearanceStockNews",
                                            StockTransferType = "ManualInventory",
                                            Qty = item.Quantity,
                                            WarehouseId = ClOrder.WarehouseId,
                                            DestinationStockType = StockTypeTableNames.ClearanceStockNews,
                                            SourceStockType = StockTypeTableNames.ClearancePlannedStocks,
                                        };
                                        manualStockUpdateDcList.Add(manualStockUpdateDc);

                                        transferStockList.Add(new TransferStockDTONew
                                        {
                                            ItemMultiMRPId = item.ItemMultiMRPId,
                                            ItemMultiMRPIdTrans = item.ItemMultiMRPId,
                                            StockBatchMasterId = StockBatchMasterId,
                                            Qty = item.Quantity,
                                            WarehouseId = ClOrder.WarehouseId,
                                            BatchMasterId = 0
                                        });
                                    }

                                    foreach (var item in transferStockList.Where(x => x.Qty > 0))
                                    {
                                        bool isBatchSuccess = batchMasterManager.UpdateStockInSameBatch(item.StockBatchMasterId, context, UserId, item.Qty * (-1));
                                        if (!isBatchSuccess)
                                        {
                                            result.Status = false;
                                            result.msg = "Issue in Batch stock. Please try after some time. ";
                                            return result;
                                        }
                                    }


                                    foreach (var item in manualStockUpdateDcList.Where(x => x.Qty > 0))
                                    {
                                        bool isSuccess = sthelper.TransferBetweenPhysicalStocks(item, UserId, context, dbContextTransaction);
                                        if (!isSuccess)
                                        {
                                            result.Status = false;
                                            result.msg = " Issue in stock. Please try after some time. ";
                                            return result;
                                        }
                                    }

                                }
                                if (cllineitems.Any(x => x.Status == 2))
                                {
                                    List<PhysicalStockUpdateRequestDc> manualStockUpdateDcList = new List<PhysicalStockUpdateRequestDc>();
                                    List<TransferStockDTONew> transferStockList = new List<TransferStockDTONew>();

                                    foreach (var item in cllineitems.Where(x => x.Status == 2))
                                    {

                                        PhysicalStockUpdateRequestDc manualStockUpdateDc = new PhysicalStockUpdateRequestDc
                                        {
                                            ItemMultiMRPId = item.ItemMultiMRPId,
                                            Reason = "ClearancePlannedToCurrent",
                                            StockTransferType = "ManualInventory",
                                            Qty = item.Quantity,
                                            WarehouseId = ClOrder.WarehouseId,
                                            DestinationStockType = StockTypeTableNames.CurrentStocks,
                                            SourceStockType = StockTypeTableNames.ClearancePlannedStocks,
                                        };
                                        manualStockUpdateDcList.Add(manualStockUpdateDc);

                                        transferStockList.Add(new TransferStockDTONew
                                        {
                                            ItemMultiMRPId = item.ItemMultiMRPId,
                                            ItemMultiMRPIdTrans = item.ItemMultiMRPId,
                                            StockBatchMasterId = item.StockBatchMasterId,
                                            Qty = item.Quantity,
                                            WarehouseId = ClOrder.WarehouseId,
                                            BatchMasterId = 0
                                        });
                                    }

                                    foreach (var item in transferStockList.Where(x => x.Qty > 0))
                                    {
                                        bool isBatchSuccess = batchMasterManager.UpdateStockInSameBatch(item.StockBatchMasterId, context, UserId, item.Qty * (-1));
                                        if (!isBatchSuccess)
                                        {
                                            result.Status = false;
                                            result.msg = "Issue in Batch stock. Please try after some time. ";
                                            return result;
                                        }
                                    }
                                    foreach (var item in manualStockUpdateDcList.Where(x => x.Qty > 0))
                                    {
                                        bool isSuccess = sthelper.TransferBetweenPhysicalStocks(item, UserId, context, dbContextTransaction);
                                        if (!isSuccess)
                                        {
                                            result.Status = false;
                                            result.msg = " Issue in stock. Please try after some time. ";
                                            return result;
                                        }
                                    }
                                }
                            }
                            else if (ClOrder.OrderType == "ClearanceToCurrent")
                            {
                                if (cllineitems.Any(x => x.Status == 1))
                                {
                                    List<PhysicalStockUpdateRequestDc> manualStockUpdateDcList = new List<PhysicalStockUpdateRequestDc>();
                                    List<TransferStockDTONew> transferStockList = new List<TransferStockDTONew>();

                                    foreach (var item in cllineitems.Where(x => x.Status == 1))
                                    {
                                        long BatchId = CurrentStockBatchMasters.FirstOrDefault(x => x.Id == item.StockBatchMasterId).BatchMasterId;
                                        long StockBatchMasterId = batchMasterManager.GetOrCreate(item.ItemMultiMRPId, ClOrder.WarehouseId, "C", BatchId, context, UserId);

                                        PhysicalStockUpdateRequestDc manualStockUpdateDc = new PhysicalStockUpdateRequestDc
                                        {
                                            ItemMultiMRPId = item.ItemMultiMRPId,
                                            Reason = "ClearancePlannedToCurrent",
                                            StockTransferType = "ManualInventory",
                                            Qty = item.Quantity,
                                            WarehouseId = ClOrder.WarehouseId,
                                            DestinationStockType = StockTypeTableNames.CurrentStocks,
                                            SourceStockType = StockTypeTableNames.ClearancePlannedStocks,
                                        };
                                        manualStockUpdateDcList.Add(manualStockUpdateDc);

                                        transferStockList.Add(new TransferStockDTONew
                                        {
                                            ItemMultiMRPId = item.ItemMultiMRPId,
                                            ItemMultiMRPIdTrans = item.ItemMultiMRPId,
                                            StockBatchMasterId = StockBatchMasterId,
                                            Qty = item.Quantity,
                                            WarehouseId = ClOrder.WarehouseId,
                                            BatchMasterId = 0
                                        });
                                    }

                                    foreach (var item in transferStockList.Where(x => x.Qty > 0))
                                    {
                                        bool isBatchSuccess = batchMasterManager.UpdateStockInSameBatch(item.StockBatchMasterId, context, UserId, item.Qty * (-1));
                                        if (!isBatchSuccess)
                                        {
                                            result.Status = false;
                                            result.msg = "Issue in Batch stock. Please try after some time. ";
                                            return result;
                                        }
                                    }
                                    foreach (var item in manualStockUpdateDcList.Where(x => x.Qty > 0))
                                    {
                                        bool isSuccess = sthelper.TransferBetweenPhysicalStocks(item, UserId, context, dbContextTransaction);
                                        if (!isSuccess)
                                        {
                                            result.Status = false;
                                            result.msg = " Issue in stock. Please try after some time. ";
                                            return result;
                                        }
                                    }

                                }
                                if (cllineitems.Any(x => x.Status == 2))
                                {
                                    List<PhysicalStockUpdateRequestDc> manualStockUpdateDcList = new List<PhysicalStockUpdateRequestDc>();
                                    List<TransferStockDTONew> transferStockList = new List<TransferStockDTONew>();
                                    foreach (var item in cllineitems.Where(x => x.Status == 2))
                                    {
                                        PhysicalStockUpdateRequestDc manualStockUpdateDc = new PhysicalStockUpdateRequestDc
                                        {
                                            ItemMultiMRPId = item.ItemMultiMRPId,
                                            Reason = "ClearancePlannedToClearanceStockNews",
                                            StockTransferType = "ManualInventory",
                                            Qty = item.Quantity,
                                            WarehouseId = ClOrder.WarehouseId,
                                            DestinationStockType = StockTypeTableNames.ClearanceStockNews,
                                            SourceStockType = StockTypeTableNames.ClearancePlannedStocks,
                                        };
                                        manualStockUpdateDcList.Add(manualStockUpdateDc);

                                        transferStockList.Add(new TransferStockDTONew
                                        {
                                            ItemMultiMRPId = item.ItemMultiMRPId,
                                            ItemMultiMRPIdTrans = item.ItemMultiMRPId,
                                            StockBatchMasterId = item.StockBatchMasterId,
                                            Qty = item.Quantity,
                                            WarehouseId = ClOrder.WarehouseId,
                                            BatchMasterId = 0
                                        });
                                    }

                                    foreach (var item in transferStockList.Where(x => x.Qty > 0))
                                    {
                                        bool isBatchSuccess = batchMasterManager.UpdateStockInSameBatch(item.StockBatchMasterId, context, UserId, item.Qty * (-1));
                                        if (!isBatchSuccess)
                                        {
                                            result.Status = false;
                                            result.msg = "Issue in Batch stock. Please try after some time. ";
                                            return result;
                                        }
                                    }
                                    foreach (var item in manualStockUpdateDcList.Where(x => x.Qty > 0))
                                    {
                                        bool isSuccess = sthelper.TransferBetweenPhysicalStocks(item, UserId, context, dbContextTransaction);
                                        if (!isSuccess)
                                        {
                                            result.Status = false;
                                            result.msg = " Issue in stock. Please try after some time. ";
                                            return result;
                                        }
                                    }
                                }
                            }
                            if (cllineitems.All(x => x.Status == 2))
                            {
                                ClOrder.Status = "Rejected";
                                ClOrder.Comment = " All item Rejected";
                                ClOrder.ModifiedDate = DateTime.Now;
                                context.Entry(ClOrder).State = EntityState.Modified;
                            }
                            context.Entry(ClOrder).State = EntityState.Modified;
                            if (clearancePickerTimer != null && clearancePickerTimer.EndDate == null)
                            {
                                clearancePickerTimer.EndDate = DateTime.Now;
                                context.Entry(clearancePickerTimer).State = EntityState.Modified;
                            }
                            if (context.Commit() > 0)
                            {
                                dbContextTransaction.Complete();
                                if (ConfigurationManager.AppSettings["Environment"] == "Production")
                                {
                                    var sub = "Clearance Order No. " + Id + " Physically Moved at" + WarehouseName.WarehouseName;
                                    var msg = "Clearance Order No. " + Id + " status has been updted to Physically Moved." + " Please activate the item on Retailer App after adding applicable discount.";
                                    EmailHelper.SendMail(ToPicker.Email, ToBuyer, "", sub, msg, "");
                                }


                                result.Status = true;
                                result.msg = "Clearance Picker Submitted successfully  #No. :" + Id;
                                return result;
                            }
                            else
                            {
                                result.msg = "something went wrong in Clearance picker #No. :" + Id;
                                return result;
                            }
                        }
                        else if (ClOrder != null && ClOrder.Status != "Approved")
                        {
                            result.msg = "Clearance Picker already in status :" + ClOrder.Status;
                        }
                        else
                        {
                            result.msg = "something went wrong.";
                        }
                    }
                }
            }
            return result;
        }

    }
}
