using AgileObjects.AgileMapper;
using AngularJSAuthentication.API.Controllers.Base;
using AngularJSAuthentication.API.Helpers;
using AngularJSAuthentication.API.Managers;
using AngularJSAuthentication.BusinessLayer.Managers.Masters;
using AngularJSAuthentication.BusinessLayer.Managers.Reports;
using AngularJSAuthentication.Common.Constants;
using AngularJSAuthentication.Common.Helpers;
using AngularJSAuthentication.DataContracts.APIParams;
using AngularJSAuthentication.DataContracts.External;
using AngularJSAuthentication.DataContracts.Masters;
using AngularJSAuthentication.DataContracts.Shared;
using AngularJSAuthentication.DataContracts.Transaction.Mongo.BuyerDashboard;
using AngularJSAuthentication.Model;
using Newtonsoft.Json;
using Nito.AsyncEx;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace AngularJSAuthentication.API.ControllerV7
{
    [RoutePrefix("api/BuyerDashboard")]
    public class BuyerDashboardController : BaseAuthController
    {
        public double xPointValue = AppConstants.xPoint;
        [HttpGet]
        [Route("GetBuyers")]
        public async Task<List<BuyerDc>> GetBuyers()
        {
            using (var context = new AuthContext())
            {
                var manager = new PeopleManager();
                List<BuyerDc> People = await manager.GetPeopleByBuyer();
                return People;
            }
        }


        [HttpPost]
        //[AllowAnonymous]
        [Route("Get")]
        public async Task<List<BuyerDashboardData>> GetBuyerDashBoard(BuyerDashboardParams param)
        {
            var mongoDbHelperItems = new MongoDbHelper<ItemLedger>();
            param.EndDate = param.EndDate.Date.AddDays(1).AddSeconds(-1);

            var manager = new ItemLedgerManager();
            List<BuyerDashboardData> data = await manager.GetBuyerDashBoardDataAsync(param.StartDate.Date, param.EndDate, param.BuyerIds);


            return data;
        }


        [HttpPost]
        //[AllowAnonymous]
        [Route("DashboardExport")]
        public async Task<string> DashboardExport(BuyerDashboardParams param)
        {
            var mongoDbHelperItems = new MongoDbHelper<ItemLedger>();
            param.EndDate = param.EndDate.Date.AddDays(1).AddSeconds(-1);

            var manager = new ItemLedgerManager();

            List<BuyerDashboardDataExport> exportData = await manager.DashboardExportAsync(param.StartDate.Date, param.EndDate, param.BuyerIds);
            string fileUrl = "";
            param.EndDate = param.EndDate.Date.AddDays(1).AddSeconds(-1);

            if (exportData != null && exportData.Any())
            {
                string ExcelSavePath = HttpContext.Current.Server.MapPath("~/BuyerDashboardExport/");

                var dataTables = new List<DataTable>();

                foreach (var item in exportData.GroupBy(x => new { x.BuyerId, x.BuyerName }))
                {
                    var dataList = item.GroupBy(x => new { x.BrandId, x.BrandName }).Select(x => new BuyerDashboardDataExport
                    {
                        BrandId = x.Key.BrandId,
                        BrandName = x.Key.BrandName,
                        ClosingStock = x.FirstOrDefault()?.ClosingStock ?? 0,
                        GrossMargin = x.FirstOrDefault()?.GrossMargin ?? 0,
                        GrossMarginPercent = x.FirstOrDefault()?.GrossMarginPercent ?? 0,
                        SaleAmount = x.FirstOrDefault()?.SaleAmount ?? 0,
                        NetSale = x.FirstOrDefault()?.NetSale ?? 0,
                        InventoryDays = x.FirstOrDefault()?.InventoryDays ?? 0,
                        BuyerId = item.Key.BuyerId,
                        BuyerName = item.Key.BuyerName,
                        CancelAmt = x.FirstOrDefault()?.CancelAmt ?? 0
                    }).ToList();

                    var dt = ClassToDataTable.CreateDataTable(dataList);
                    dt.TableName = item.Key.BuyerName;
                    dataTables.Add(dt);
                }


                if (!Directory.Exists(ExcelSavePath))
                    Directory.CreateDirectory(ExcelSavePath);

                var fileName = "BuyerData_" + DateTime.Now.ToString("yyyyddMHHmmss") + ".xlsx";

                string filePath = ExcelSavePath + fileName;

                ExcelGenerator.DataTable_To_Excel(dataTables, filePath);


                fileUrl = string.Format("{0}://{1}{2}/{3}", HttpContext.Current.Request.UrlReferrer.Scheme
                                                                , HttpContext.Current.Request.Url.DnsSafeHost
                                                                , HttpContext.Current.Request.Url.Port == 80 || HttpContext.Current.Request.Url.Port == 443 ? "" : ":" + HttpContext.Current.Request.Url.Port
                                                                , string.Format("BuyerDashboardExport/{0}", fileName));

            }
            return fileUrl;
        }

        [HttpPost]
        //[AllowAnonymous]
        [Route("GetBuyerSale")]
        public async Task<List<BuyerDashboardDetailData>> GetBuyerSale(BuyerDashboardSalesParams param)
        {
            List<BuyerDashboardDetailData> buyerDashboards = new List<BuyerDashboardDetailData>();
            param.EndDate = param.EndDate.Date.AddDays(1).AddSeconds(-1);

            var manager = new ItemLedgerManager();

            List<BuyerDashboardDetailData> combinedData = await manager.GetBuyerBrandNetSaleAsync(param.StartDate.Date, param.EndDate, param.BuyerId);

            if (combinedData != null && combinedData.Any())
            {
                switch (param.Columns)
                {
                    case "brand":
                        buyerDashboards = combinedData.GroupBy(x => new { x.BrandId, x.BrandName }).Select(x => new BuyerDashboardDetailData
                        {
                            BrandId = x.Key.BrandId,
                            BrandName = x.Key.BrandName,
                            Value = x.Sum(z => z.Value) //x.Sum(z => z.SaleAmount ?? 0),

                        }).ToList();
                        break;

                    case "warehouse":
                        buyerDashboards = combinedData.GroupBy(x => new { x.WarehouseId, x.WarehouseName }).Select(x => new BuyerDashboardDetailData
                        {
                            Value = x.Sum(z => z.Value),
                            WarehouseId = x.Key.WarehouseId,
                            WarehouseName = x.Key.WarehouseName
                        }).ToList();
                        break;

                    default:
                        buyerDashboards = combinedData;
                        break;
                }

                buyerDashboards = buyerDashboards.Where(x => x.Value > 0).ToList();

            }

            return buyerDashboards;
        }

        [HttpPost]
        //[AllowAnonymous]
        [Route("GetBuyerClosingStock")]
        public async Task<List<BuyerDashboardDetailData>> GetBuyerClosingStock(BuyerDashboardSalesParams param)
        {
            List<BuyerDashboardDetailData> buyerDashboards = new List<BuyerDashboardDetailData>();
            param.EndDate = param.EndDate.Date.AddDays(1).AddSeconds(-1);

            var manager = new ItemLedgerManager();

            List<BuyerDashboardDetailData> combinedData = await manager.GetBuyerBrandClosingStockAsync(param.StartDate.Date, param.EndDate, param.BuyerId);

            if (combinedData != null && combinedData.Any())
            {
                switch (param.Columns)
                {
                    case "brand":
                        buyerDashboards = combinedData.GroupBy(x => new { x.BrandId, x.BrandName }).Select(x => new BuyerDashboardDetailData
                        {
                            BrandId = x.Key.BrandId,
                            BrandName = x.Key.BrandName,
                            Value = x.Sum(z => z.Value) //x.Sum(z => z.SaleAmount ?? 0),

                        }).ToList();
                        break;

                    case "warehouse":
                        buyerDashboards = combinedData.GroupBy(x => new { x.WarehouseId, x.WarehouseName }).Select(x => new BuyerDashboardDetailData
                        {
                            Value = x.Sum(z => z.Value),
                            WarehouseId = x.Key.WarehouseId,
                            WarehouseName = x.Key.WarehouseName
                        }).ToList();
                        break;

                    default:
                        buyerDashboards = combinedData;
                        break;
                }

                buyerDashboards = buyerDashboards.Where(x => x.Value > 0).ToList();

            }

            return buyerDashboards;
        }

        [HttpPost]
        //[AllowAnonymous]
        [Route("GetBuyerGrossMargin")]
        public async Task<List<BuyerDashboardDetailData>> GetBuyerGrossMargin(BuyerDashboardSalesParams param)
        {
            List<BuyerDashboardDetailData> buyerDashboards = new List<BuyerDashboardDetailData>();
            param.EndDate = param.EndDate.Date.AddDays(1).AddSeconds(-1);

            var manager = new ItemLedgerManager();

            List<BuyerDashboardDetailData> combinedData = await manager.GetBuyerBrandGrossMarginAsync(param.StartDate.Date, param.EndDate, param.BuyerId, param.Columns);

            return combinedData;
        }

        [HttpPost]
        //[AllowAnonymous]
        [Route("GetBuyerInventoryDays")]
        public async Task<List<BuyerDashboardDetailData>> GetBuyerInventoryDays(BuyerDashboardSalesParams param)
        {
            List<BuyerDashboardDetailData> buyerDashboards = new List<BuyerDashboardDetailData>();
            param.EndDate = param.EndDate.Date.AddDays(1).AddSeconds(-1);

            var manager = new ItemLedgerManager();

            List<BuyerDashboardDetailData> combinedData = await manager.GetBuyerInventoryDaysAsync(param.StartDate.Date, param.EndDate, param.BuyerId, param.Columns);

            return combinedData;
        }

        [HttpPost]
        [AllowAnonymous]
        [Route("ExportData")]
        public async Task<string> ExportData(BuyerDashboardSalesParams param)
        {
            List<BuyerDashboardDetailData> buyerDashboards = new List<BuyerDashboardDetailData>();
            param.EndDate = param.EndDate.Date.AddDays(1).AddSeconds(-1);

            var manager = new ItemLedgerManager();

            List<ExportData> combinedData = await manager.BuyerDataExportAsync(param.StartDate.Date, param.EndDate, param.BuyerId, param.ExportType);

            string fileUrl = "";
            string ExcelSavePath = HttpContext.Current.Server.MapPath("~/BuyerDashboardExport/");

            var dataTables = new List<DataTable>();
            if (combinedData != null && combinedData.Any())
            {
                var dt = ClassToDataTable.CreateDataTable(combinedData);
                dt.TableName = param.ExportType;
                dataTables.Add(dt);
            }

            if (!Directory.Exists(ExcelSavePath))
                Directory.CreateDirectory(ExcelSavePath);

            var fileName = "BuyerData_" + param.ExportType + DateTime.Now.ToString("yyyyddMHHmmss") + ".xlsx";

            string filePath = ExcelSavePath + fileName;

            ExcelGenerator.DataTable_To_Excel(dataTables, filePath);


            fileUrl = string.Format("{0}://{1}{2}/{3}", HttpContext.Current.Request.UrlReferrer.Scheme
                                                            , HttpContext.Current.Request.Url.DnsSafeHost
                                                            , HttpContext.Current.Request.Url.Port == 80 || HttpContext.Current.Request.Url.Port == 443 ? "" : ":" + HttpContext.Current.Request.Url.Port
                                                            , string.Format("BuyerDashboardExport/{0}", fileName));

            return fileUrl;
        }

        [HttpPost]
        [AllowAnonymous]
        [Route("ExportBuyerData")]
        public async Task<string> ExportBuyerData(BuyerDashboardSalesParams param)
        {
            param.EndDate = param.EndDate.Date.AddDays(1).AddSeconds(-1);

            var manager = new ItemLedgerManager();

            List<BuyerDataExportDc> combinedData = await manager.BuyerAllDataExportAsync(param.StartDate.Date, param.EndDate, param.BuyerId);

            string fileUrl = "";
            string ExcelSavePath = HttpContext.Current.Server.MapPath("~/BuyerDashboardExport/");

            var dataTables = new List<DataTable>();
            if (combinedData != null && combinedData.Any())
            {
                var dt = ClassToDataTable.CreateDataTable(combinedData);
                dt.TableName = combinedData.FirstOrDefault().BuyerName + "_AllExport";
                dataTables.Add(dt);
            }

            if (!Directory.Exists(ExcelSavePath))
                Directory.CreateDirectory(ExcelSavePath);

            var fileName = "BuyerAllDataExport_" + param.ExportType + DateTime.Now.ToString("yyyyddMHHmmss") + ".xlsx";

            string filePath = ExcelSavePath + fileName;

            ExcelGenerator.DataTable_To_Excel(dataTables, filePath);


            fileUrl = string.Format("{0}://{1}{2}/{3}", HttpContext.Current.Request.UrlReferrer.Scheme
                                                            , HttpContext.Current.Request.Url.DnsSafeHost
                                                            , HttpContext.Current.Request.Url.Port == 80 || HttpContext.Current.Request.Url.Port == 443 ? "" : ":" + HttpContext.Current.Request.Url.Port
                                                            , string.Format("BuyerDashboardExport/{0}", fileName));

            return fileUrl;
        }



        [HttpPost]
        [Route("ItemSaleVsSPandPP")]
        public async Task<List<ItemSaleVsSPandPPData>> ItemSaleVsSPandPP(ItemSaleVsSPandPPParams param)
        {
            List<ItemSaleVsSPandPPData> result = new List<ItemSaleVsSPandPPData>();

            var manager = new ItemLedgerManager();
            var data = await manager.GetItemSaleVsSPandPPData(param.StartDate.Date, param.EndDate.Date.AddDays(1).AddMilliseconds(-1), param.WarehouseId, param.ItemMultiMrpIds);

            if (data != null && data.InOutTransactions != null && data.InOutTransactions.Any())
            {
                result = data.InOutTransactions.GroupBy(x => new { x.multimrpwarehouse }).Select(x => new ItemSaleVsSPandPPData
                {
                    BuyerId = data.ItemDetails.FirstOrDefault(z => z.multimrpwarehouse == x.Key.multimrpwarehouse)?.BuyerId ?? 0,
                    BuyerName = data.ItemDetails.FirstOrDefault(z => z.multimrpwarehouse == x.Key.multimrpwarehouse)?.BuyerName,
                    CategoryId = data.ItemDetails.FirstOrDefault(z => z.multimrpwarehouse == x.Key.multimrpwarehouse)?.CategoryId ?? 0,
                    CategoryName = data.ItemDetails.FirstOrDefault(z => z.multimrpwarehouse == x.Key.multimrpwarehouse)?.Category,
                    ItemMultiMrpId = data.ItemDetails.FirstOrDefault(z => z.multimrpwarehouse == x.Key.multimrpwarehouse)?.ItemMultiMrpId ?? 0,
                    ItemName = data.ItemDetails.FirstOrDefault(z => z.multimrpwarehouse == x.Key.multimrpwarehouse)?.ItemName,
                    ItemNumber = data.ItemDetails.FirstOrDefault(z => z.multimrpwarehouse == x.Key.multimrpwarehouse)?.ItemCode,
                    MRP = data.ItemDetails.FirstOrDefault(z => z.multimrpwarehouse == x.Key.multimrpwarehouse)?.MRP,
                    TaxPercentage = data.ItemDetails.FirstOrDefault(z => z.multimrpwarehouse == x.Key.multimrpwarehouse)?.ItemTaxPercent,
                    WarehouseId = data.ItemDetails.FirstOrDefault(z => z.multimrpwarehouse == x.Key.multimrpwarehouse)?.WarehouseId ?? 0,
                    WarehouseName = data.ItemDetails.FirstOrDefault(z => z.multimrpwarehouse == x.Key.multimrpwarehouse)?.WarehouseName,
                    SaleAndSPPPData = x.OrderBy(s => s.CreatedDate).GroupBy(s => new
                    {
                        CreatedDate = s.CreatedDate.Value.Date
                    }).Select(a => new SaleAndSPPPData
                    {
                        PurchasePrice = Math.Round(a.Sum(z => z.PurchasePrice) / a.Count(), 2),
                        Qty = a.Sum(s => s.Qty),
                        SaleAmount = Math.Round(a.Sum(s => s.amount), 2),
                        SellingPrice = Math.Round(a.Sum(z => z.SellingPrice) / a.Count(), 2),
                        TransactionDate = a.Key.CreatedDate
                    }).ToList()
                }).ToList();
            }

            return result;

        }




        [Route("GetInOutData")]
        [HttpGet]
        public async Task<string> GetInOutData(int month, int year)
        {
            var param = new BuyerDashboardParams();

            param.StartDate = new DateTime(year, month, 1);
            param.EndDate = param.StartDate.Month == DateTime.Now.Month && param.StartDate.Year == DateTime.Now.Year
                            ? DateTime.Now.Date.AddSeconds(-1)
                            : param.StartDate.AddMonths(1).AddSeconds(-1);

            var fileName = "InOut_" + param.EndDate.ToString("yyyyddMM") + ".xlsx";

            string folderPath = Path.Combine(HttpRuntime.AppDomainAppPath, "ExcelGeneratePath", "GeneratedInOut");

            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            string path = Path.Combine(folderPath, fileName);
            if (!File.Exists(path))
            {
                ItemLedgerManager manager = new ItemLedgerManager();
                var data = await manager.GetDataFromDb(param);

                DataTable dt = ListtoDataTableConverter.ToDataTable(data);

                ExcelGenerator.DataTable_To_Excel(dt, "InOut", path);
            }

            string fileUrl = string.Format("{0}://{1}{2}/{3}", new Uri(HttpContext.Current.Request.UrlReferrer.AbsoluteUri).Scheme
                                                             , HttpContext.Current.Request.Url.DnsSafeHost
                                                             , (HttpContext.Current.Request.Url.Port != 80 && HttpContext.Current.Request.Url.Port != 443 ? ":" + HttpContext.Current.Request.Url.Port : "")
                                                             , string.Format("ExcelGeneratePath/GeneratedInOut/{0}", fileName));
            return fileUrl;
        }

        [Route("GetOtherStockInOutData")]
        [HttpGet]
        public async Task<string> GetOtherStockInOutData(string stockname,int month, int year)
        {
            var param = new BuyerDashboardParams();

            param.StartDate = new DateTime(year, month, 1);
            param.EndDate = param.StartDate.Month == DateTime.Now.Month && param.StartDate.Year == DateTime.Now.Year
                            ? DateTime.Now.Date.AddSeconds(-1)
                            : param.StartDate.AddMonths(1).AddSeconds(-1);
            param.StockType = stockname;

            var fileName = stockname+"InOut_" + param.EndDate.ToString("yyyyddMM") + ".xlsx";

            string folderPath = Path.Combine(HttpRuntime.AppDomainAppPath, "ExcelGeneratePath", "GeneratedInOut" ,stockname);

            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            string path = Path.Combine(folderPath, fileName);
            if (!File.Exists(path))
            {
                ItemLedgerManager manager = new ItemLedgerManager();
                var data = await manager.GetOtherStockDataFromDb(param);

                DataTable dt = ListtoDataTableConverter.ToDataTable(data);

                ExcelGenerator.DataTable_To_Excel(dt, "InOut", path);
            }

            string fileUrl = string.Format("{0}://{1}{2}/{3}", new Uri(HttpContext.Current.Request.UrlReferrer.AbsoluteUri).Scheme
                                                             , HttpContext.Current.Request.Url.DnsSafeHost
                                                             , (HttpContext.Current.Request.Url.Port != 80 && HttpContext.Current.Request.Url.Port != 443 ? ":" + HttpContext.Current.Request.Url.Port : "")
                                                             , string.Format("ExcelGeneratePath/GeneratedInOut/"+stockname+"/{0}", fileName));
            return fileUrl;
        }

        public class abc
        {
            public string BuyerIds { get; set; }
            public string StartDate { get; set; }
            public string EndDate { get; set; }
        }




        [HttpGet]
        [Route("GetWarehouseList")]
        public async Task<List<WarehouseMinDc>> GetWarehouseList(int buyerid)
        {
            using (var context = new AuthContext())
            {
                var manager = new ItemLedgerManager();
                List<WarehouseMinDc> _WarehouseMinDc = await manager.GetWarehiuseListAsync(buyerid);
                return _WarehouseMinDc;
            }
        }

        [HttpGet]
        [Route("GetItemList")]
        public async Task<List<ItemMinDc>> GetItemList(int buyerid, int warehouseid)
        {
            using (var context = new AuthContext())
            {
                var manager = new ItemLedgerManager();
                List<ItemMinDc> _WarehouseMinDc = await manager.GetItemListAsync(buyerid, warehouseid);
                return _WarehouseMinDc;
            }
        }

        [HttpPost]
        [Route("ItemSaleVsInventory")]
        public async Task<SaleVsInventoryResultInOut> ItemSaleVsInventory(ItemSaleVsInventoryParams param)
        {
            var manager = new ItemLedgerManager();
            var data = await manager.GetItemSaleVsInventoryData(param.StartDate.Date, param.EndDate.Date.AddDays(1).AddMilliseconds(-1), param.WarehouseId, param.ItemMultiMrpIds);
            return data;
        }

        [HttpGet]
        [Route("GetBuyerCatSubSubCategory")]
        public async Task<BuyerCatSubSubCategory> BuyerCatSubSubCategory(int buyerid, int warehouseId)
        {
            using (var context = new AuthContext())
            {
                var manager = new ItemLedgerManager();
                BuyerCatSubSubCategory buyerCatSubSubCategories = await manager.BuyerCatSubSubCategory(buyerid, warehouseId);
                return buyerCatSubSubCategories;
            }
        }
        [HttpGet]
        [Route("GetCatSubSubCategory")]
        public async Task<BuyerCatSubSubCategory> CatSubSubCategory()
        {
            using (var context = new AuthContext())
            {
                var manager = new ItemLedgerManager();
                BuyerCatSubSubCategory buyerCatSubSubCategories = await manager.CatSubSubCategory();
                return buyerCatSubSubCategories;
            }
        }

        [HttpPost]
        [Route("GetBuyerItemDetail")]
        public async Task<List<ItemDataDC>> BuyerItemDetails(BuyerItemDetailParams buyerItemDetailParams)
        {
            List<ItemDataDC> item = new List<ItemDataDC>();
            int customerId = 0;
            using (var context = new AuthContext())
            {
                customerId = context.Database.SqlQuery<int>("select top 1 customerid from customers where warehouseid = " + buyerItemDetailParams.WarehouseID).FirstOrDefault();
            }

            RetailerAppManager manager = new RetailerAppManager();
            var itemData = await manager.RetailerGetItembycatesscatid("en", customerId, buyerItemDetailParams.Categoryid, buyerItemDetailParams.SubCategoryId, buyerItemDetailParams.SubsubCategoryid,0,1000,"P","asc");

            item = itemData.ItemMasters;

            //var manager = new ItemLedgerManager();
            //List<ItemDataDC> newdata = await manager.BuyerItemDetail(buyerItemDetailParams);

            //foreach (var it in newdata)
            //{
            //    if (item == null)
            //    {
            //        item = new List<ItemDataDC>();
            //    }
            //    try
            //    {
            //        if (!it.IsOffer)
            //        {
            //            /// Dream Point Logic && Margin Point
            //            int? MP, PP;
            //            double xPoint = xPointValue * 10;
            //            //Customer (0.2 * 10=1)
            //            if (it.promoPerItems.Equals(null) && it.promoPerItems == null)
            //            {
            //                PP = 0;
            //            }
            //            else
            //            {
            //                PP = it.promoPerItems;
            //            }
            //            if (it.marginPoint.Equals(null) && it.promoPerItems == null)
            //            {
            //                MP = 0;
            //            }
            //            else
            //            {
            //                double WithTaxNetPurchasePrice = Math.Round(it.NetPurchasePrice * (1 + (it.TotalTaxPercentage / 100)), 3);//With tax
            //                MP = Convert.ToInt32((it.UnitPrice - WithTaxNetPurchasePrice) * xPoint); // (UnitPrice-NPP withtax) * By xpoint 
            //            }
            //            if (PP > 0 && MP > 0)
            //            {
            //                int? PP_MP = PP + MP;
            //                it.dreamPoint = PP_MP;
            //            }
            //            else if (MP > 0)
            //            {
            //                it.dreamPoint = MP;
            //            }
            //            else if (PP > 0)
            //            {
            //                it.dreamPoint = PP;
            //            }
            //            else
            //            {
            //                it.dreamPoint = 0;
            //            }
            //        }
            //        else
            //        {
            //            it.dreamPoint = 0;
            //        }

            //        if (it.price > it.UnitPrice)
            //        {
            //            it.marginPoint = ((it.price - it.UnitPrice) * 100) / it.UnitPrice;//MP;  we replce marginpoint value by margin for app here 
            //        }
            //        else
            //        {
            //            it.marginPoint = 0;
            //        }
            //    }
            //    catch { }

            //    item.Add(it);
            //}
            return item;

        }

        [HttpGet]
        [Route("GetAllActiveWarehouse")]
        public async Task<List<WarehouseMinDc>> GetAllActiveWarehouse()
        {
            using (var context = new AuthContext())
            {
                var manager = new ItemLedgerManager();
                List<WarehouseMinDc> _WarehouseMinDc = await manager.GetAllActiveWarehouse();
                return _WarehouseMinDc;
            }
        }

        #region Buyer edit price 

        [HttpGet]
        [Route("GetBuyerSupWarBrand")]
        public async Task<BuyerSupWarBrandDC> GetBuyerSupWarBrand(int buyerid)
        {
            using (var context = new AuthContext())
            {
                var manager = new ItemLedgerManager();
                BuyerSupWarBrandDC buyerSupWarBrandDC = await manager.BuyerSupWarBrand(buyerid);
                return buyerSupWarBrandDC;
            }
        }

        [HttpGet]
        [Route("GetEditItemPrice")]
        public async Task<List<BuyerEditPriceItem>> GetEditItemPrice(int wid, int sid, string itemhint)
        {
            using (var context = new AuthContext())
            {
                var manager = new ItemLedgerManager();
                List<BuyerEditPriceItem> buyerEditPriceItems = await manager.GetEditItemPrice(wid, sid, itemhint);
                return buyerEditPriceItems;
            }
        }

        [HttpPost]
        [Route("postEditItemPrice")]
        public async Task<ItemDealPriceQtyDC> ItemDealPriceQtyAdd(ItemDealPriceQtyAddParam itemDealPriceQtyAddParam)
        {
            var result = new ItemDealPriceQtyDC();
            using (var context = new AuthContext())
            {
                ItemDealPriceQty ItemExist = context.ItemDealPriceQtyDB.Where(a => a.ItemNumber == itemDealPriceQtyAddParam.ItemNumber && a.ItemMultiMrpID == itemDealPriceQtyAddParam.ItemMultiMrpID && a.WareHouseId == itemDealPriceQtyAddParam.WareHouseId && a.SupplierId == itemDealPriceQtyAddParam.SupplierId).SingleOrDefault();
                if (ItemExist == null)
                {

                    ItemDealPriceQty itemDealPriceQty = new ItemDealPriceQty();
                    itemDealPriceQty.ItemNumber = itemDealPriceQtyAddParam.ItemNumber;
                    itemDealPriceQty.ItemMultiMrpID = itemDealPriceQtyAddParam.ItemMultiMrpID;
                    itemDealPriceQty.SupplierId = itemDealPriceQtyAddParam.SupplierId;
                    itemDealPriceQty.WareHouseId = itemDealPriceQtyAddParam.WareHouseId;
                    itemDealPriceQty.DealQty = itemDealPriceQtyAddParam.DealQty;
                    itemDealPriceQty.DealPrice = itemDealPriceQtyAddParam.DealPrice;
                    itemDealPriceQty.CreatedById = itemDealPriceQtyAddParam.CreatedById;
                    itemDealPriceQty.CreatedData = DateTime.Now;
                    context.ItemDealPriceQtyDB.Add(itemDealPriceQty);
                    context.Commit();
                    return result;
                }
                else
                {
                    ItemExist.DealPrice = itemDealPriceQtyAddParam.DealPrice;
                    ItemExist.DealQty = itemDealPriceQtyAddParam.DealQty;
                    ItemExist.UpdatedDate = DateTime.Now;
                    ItemExist.UpdatedByID = itemDealPriceQtyAddParam.UpdatedByID;
                    context.Commit();
                    return result;
                }
            }
        }


        #endregion


        #region InsertInOutData

        [HttpGet]
        [AllowAnonymous]
        [Route("InsertInOutData")]
        public async Task<bool> InsertInOutData()
        {
            var manager = new ItemLedgerManager();
            return await manager.InsertInOutData();
        }

        [HttpGet]
        [AllowAnonymous]
        [Route("InsertInOutDataNew")]
        public async Task<bool> InsertInOutData(DateTime startDate, DateTime endDate)
        {
            var manager = new ItemLedgerManager();
            return await manager.InsertInOutData(startDate, endDate);
        }



        [HttpGet]
        [AllowAnonymous]
        [Route("UpdateIRPriceInInqueue")]
        public async Task<bool> UpdateIRPriceInInqueue()
        {
            var manager = new ItemLedgerManager();
            return await manager.UpdateIRPriceInInqueue();
        }

        #endregion

        [HttpGet]
        [AllowAnonymous]
        [Route("InsertAbcClassification")]
        public async Task<bool> InsertAbcClassification()
        {
            var manager = new ItemLedgerManager();
            return await manager.InsertAbcClassification();
        }

        [HttpGet]
        [AllowAnonymous]
        [Route("GetAbcClassificationReport/{cityId}/{warehouseId}/{categories}")]
        public async Task<List<ItemClassificationReportDC>> GetAbcClassificationReport(int cityId, int warehouseId, string categories)
        {
            var manager = new ItemLedgerManager();
            return await manager.GetAbcClassificationReport(cityId, warehouseId, categories);
        }

        [HttpGet]
        [AllowAnonymous]
        [Route("GetCurrentUploadedVersion")]
        public async Task<string> GetCurrentUploadedVersion()
        {
            string filePath = HttpContext.Current.Server.MapPath("~/BuyerDashboardVersion/version.txt");
            string version = File.ReadAllText(filePath, Encoding.UTF8);
            return version;
        }

    }
}