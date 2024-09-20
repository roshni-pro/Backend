using AngularJSAuthentication.API.Controllers.Base;
using AngularJSAuthentication.API.Results;
using AngularJSAuthentication.BusinessLayer.Managers.Transactions.BatchCode;
using AngularJSAuthentication.DataContracts.BatchCode;
using AngularJSAuthentication.DataContracts.Transaction.Stocks;
using AngularJSAuthentication.Model.Stocks;
using System;
using System.Collections.Generic; 
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace AngularJSAuthentication.API.Controllers.Batch
{
    [RoutePrefix("api/NonSellable")]
    public class NonSellableController : BaseAuthController
    {

        [Route("GetNonSellableAndClearanceStockList")]
        [HttpPost]
        public async Task<GetTotalNonSellableAndClearanceStockListReqDC> GetNonSellableAndClearanceStockList(GetNonSellableAndClearanceStockListReqDC obj)
        {
            GetTotalNonSellableAndClearanceStockListReqDC result = new GetTotalNonSellableAndClearanceStockListReqDC();

            if (obj != null)
            {
                using (AuthContext db = new AuthContext())
                {
                    var manager = new NonSellableStockManager();
                    return result = await manager.GetNonSellableAndClearanceStockList(obj);
                }
            }
            else
            {
                return result;
            }
        }

        [Route("GetNonSellStkClearanceBrand")]//aarti
        [HttpPost]
        [AllowAnonymous]
        public async Task<List<NonSellStkClearanceBrandList>> GetNonSellStkClearanceBrand(GetNonSellStkClearanceBrandDC dcs)
        {
            //List<NonSellStkClearanceBrandList> NonSellStkClearanceBrandList = new List <NonSellStkClearanceBrandList> ();
            using (AuthContext db = new AuthContext())
            {
                var man = new NonSellableStockManager();
                return await man.GetNonSellStkClearanceBrand(dcs);
            }
        }



        [Route("ExportNonSellableAndClearanceStkList")]
        [HttpPost]
        public async Task<List<NonSellableStocksAndClearanceStockDC>> ExportNonSellableAndClearanceStkList(ExportNonSellableAndClearanceStockList obj)
        {
            var result = new List<NonSellableStocksAndClearanceStockDC>();

            using (AuthContext db = new AuthContext())
            {
                var manager = new NonSellableStockManager();
                return result = await manager.GetExportNonClearanceStk(obj);
            }
        }


            [Route("BatchwiseItemWithQty")]
        [HttpGet]
        public List<ClearanceLiveItemDC> GetBatchwiseItemWithQty(long stockId)
        {
            NonSellableStockManager nonSellableStockManager = new NonSellableStockManager();
            return nonSellableStockManager.GetBatchwiseItemWithQty(stockId);
        }

        [Route("TransferBatchwiseItemWithQty")]
        [HttpPost]
        public ResultViewModel<string> transferBatchwiseItemWithQty(List<ClearanceLiveItemDC> clearanceLiveItemList)
        {
            int userId = GetLoginUserId();
            NonSellableStockManager nonSellableStockManager = new NonSellableStockManager();
            if (clearanceLiveItemList != null && clearanceLiveItemList.Any())
            {
                using (var context = new AuthContext())
                {
                    var stockBatchMasterIdList = clearanceLiveItemList.Select(x => x.Id).ToList();

                    var query = from cl in context.ClearanceStockNewDB
                                join s in context.StockBatchMasters
                                on cl.ClearanceStockId equals s.StockId
                                where s.StockType == "CL" &&
                                stockBatchMasterIdList.Contains(s.Id)
                                select s;

                    var stockBatchMasterList = query.ToList();

                    var liveItemList = context.ClearanceLiveItemDB.Where(x => stockBatchMasterIdList.Contains(x.ClearanceStockBatchMasterId))?.ToList();

                    if(stockBatchMasterList.Any(x => x.Qty < clearanceLiveItemList.FirstOrDefault(y => y.Id == x.Id).LiveQty))
                    {   
                        return new ResultViewModel<string>
                        {
                            IsSuceess = false,
                            ErrorMessage = "Some inventory not available"
                        };
                    }


                    foreach (var item in clearanceLiveItemList)
                    {
                        var stockBatchMaster = stockBatchMasterList.FirstOrDefault(x => x.Id == item.Id);
                        var liveItem = liveItemList.FirstOrDefault(x => x.ClearanceStockBatchMasterId == item.Id);

                        if (liveItem == null)
                        {

                            ClearanceLiveItem clearanceLiveItem = new ClearanceLiveItem
                            {
                                ClearanceStockBatchMasterId = item.Id,
                                CreatedBy = userId,
                                CreatedDate = DateTime.Now,
                                IsActive = true,
                                IsDeleted = false,
                                ItemMultiMRPId = 0,
                                LiveStockQty = item.LiveQty,
                                RemainingStockQty = item.LiveQty,
                                UnitPrice = item.UpdatedUnitPrice.Value,
                                
                            };
                            context.ClearanceLiveItemDB.Add(clearanceLiveItem);
                        }
                        else
                        {
                            liveItem.LiveStockQty += item.LiveQty;
                            liveItem.RemainingStockQty += item.LiveQty;
                            liveItem.UnitPrice = item.UpdatedUnitPrice.Value;
                            liveItem.ModifiedBy = userId;
                            liveItem.ModifiedDate = DateTime.Now;

                            context.Entry(liveItem).State = EntityState.Modified;
                        }

                    }
                    context.Commit();

                    return new ResultViewModel<string>
                    {
                        IsSuceess = true,
                        SuccessMessage = "Updated successfully"
                    };
                }
            }
            else
            {
                return new ResultViewModel<string>
                {
                    IsSuceess = false,
                    ErrorMessage = "no item found to update"
                };
            }
        }
    }
}