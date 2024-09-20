using AngularJSAuthentication.API.Helper;
using AngularJSAuthentication.API.Managers;
using AngularJSAuthentication.DataContracts.ROC;
using AngularJSAuthentication.Model;
using NLog;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Http;

namespace AngularJSAuthentication.API.Controllers
{
    [RoutePrefix("api/CurrentNetStock")]
    public class CurrentNetStockController : ApiController
    {


        private static Logger logger = LogManager.GetCurrentClassLogger();
        #region Warehouse based get data

        #region OldCode
        //[Authorize]
        //[Route("")]
        //[HttpGet]
        //public async Task<IList<CurrentStockDTOM>> GetWarehousebased(int WarehouseId)
        //{
        //    using (AuthContext db = new AuthContext())
        //    {
        //        try
        //        {



        //            List<CurrentStockDTOM> FinalList = new List<CurrentStockDTOM>();
        //            var identity = User.Identity as ClaimsIdentity;
        //            int compid = 0, userid = 0;
        //            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))
        //                compid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);
        //            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
        //                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
        //            int CompanyId = compid;
        //            List<PurchaseList> uniquelist = new List<PurchaseList>();
        //            List<PurchaseOrderList> poList = new List<PurchaseOrderList>();
        //            List<CurrentNetDeliveryCancelDTO> CurrentNetDeliveryCancelList = new List<CurrentNetDeliveryCancelDTO>();
        //            List<ItemNetStockDTO> HubitemList = new List<ItemNetStockDTO>();
        //            List<CurrentNetStockDTO> CurrentStockList = new List<CurrentNetStockDTO>();
        //            List<OpenPoQtyDC> OpenPoQtyList = new List<OpenPoQtyDC>();
        //            // List<AveragAgingDC> AveragAgingList = new List<AveragAgingDC>();
        //            List<OrderDetails> ObjGetFreeItemList = new List<OrderDetails>();
        //            List<FreeStockDemandDC> ObjfreestockList = new List<FreeStockDemandDC>();
        //            List<AveragAgingDC> ObjAveragingDc = new List<AveragAgingDC>();
        //            if (WarehouseId > 0)
        //            {
        //                #region Get Data From S PurchaseOrderList
        //                var param = new SqlParameter
        //                {
        //                    ParameterName = "WarehouseId",
        //                    Value = WarehouseId
        //                };
        //                poList = db.Database.SqlQuery<PurchaseOrderList>("exec [GetCurrentNetStockOrderDetail] @WarehouseId", param).ToList();
        //                #endregion


        //                #region Get Data From S CurrentNetDeliveryCancelDTO
        //                var param1 = new SqlParameter
        //                {
        //                    ParameterName = "WarehouseId",
        //                    Value = WarehouseId
        //                };
        //                CurrentNetDeliveryCancelList = db.Database.SqlQuery<CurrentNetDeliveryCancelDTO>("exec [GetCurrentNetDeliveryCancelOrderDetail] @WarehouseId", param1).ToList();
        //                #endregion

        //                #region Get Data From S ItemNetStockDTO
        //                var param2 = new SqlParameter
        //                {
        //                    ParameterName = "WarehouseId",
        //                    Value = WarehouseId
        //                };
        //                HubitemList = db.Database.SqlQuery<ItemNetStockDTO>("exec [GetItemNetStocks] @WarehouseId", param2).ToList();
        //                #endregion

        //                #region Get Data From S CurrentNetStockDTO
        //                var param3 = new SqlParameter
        //                {
        //                    ParameterName = "WarehouseId",
        //                    Value = WarehouseId
        //                };
        //                CurrentStockList = db.Database.SqlQuery<CurrentNetStockDTO>("exec [GetCurrentNetStock] @WarehouseId", param3).ToList();
        //                #endregion

        //                #region Get Data From S OpenPoQty : 
        //                var param4 = new SqlParameter
        //                {
        //                    ParameterName = "WarehouseId",
        //                    Value = WarehouseId
        //                };
        //                OpenPoQtyList = db.Database.SqlQuery<OpenPoQtyDC>("exec [GetOpenPoQty] @WarehouseId", param4).ToList();
        //                #endregion



        //                ObjfreestockList = GetFreestock(WarehouseId);

        //                ObjAveragingDc = GetAvg(HubitemList, db);
        //            }
        //            #region old code

        //            #endregion
        //            //string query = "select i.ItemMultiMRPId, i.IsSensitive, i.IsSensitiveMRP, i.Number, i.UnitPrice, i.PurchasePrice, i.WarehouseId from itemmasters i with(nolock) where  i.PurchasePrice>0 and i.WarehouseId =" + WarehouseId + " and i.Deleted = 0" +
        //            //  " group by i.ItemMultiMRPId, i.IsSensitive, i.IsSensitiveMRP, i.Number, i.UnitPrice, i.PurchasePrice, i.WarehouseId";
        //            //List<ItemNetStockDTO> HubitemList = db.Database.SqlQuery<ItemNetStockDTO>(query).ToList();

        //            //string queryStock = "select i.CreationDate, i.MRP,i.WarehouseName,i.itemname,i.ItemMultiMRPId, i.ItemNumber, i.WarehouseId,i.CurrentInventory,i.StockId" +
        //            //                      " from CurrentStocks i with(nolock) where i.WarehouseId = " + WarehouseId + " and i.Deleted = 0" +
        //            //                      " group by i.CreationDate,i.MRP,i.WarehouseName,i.itemname,i.ItemMultiMRPId, i.ItemNumber, i.WarehouseId,i.CurrentInventory,i.StockId";
        //            //List<CurrentNetStockDTO> CurrentStockList = db.Database.SqlQuery<CurrentNetStockDTO>(queryStock).ToList();
        //            foreach (PurchaseOrderList item in poList)
        //            {
        //                int count = 0;
        //                PurchaseList l = uniquelist.Where(x => x.PurchaseSku == item.PurchaseSku && x.ItemMultiMRPId == item.ItemMultiMRPId).SingleOrDefault();
        //                if (l == null)
        //                {
        //                    count += 1;
        //                    l = new PurchaseList();
        //                    l.name = item.ItemName;
        //                    l.conversionfactor = item.Conversionfactor;
        //                    l.Supplier = item.SupplierName;
        //                    l.SupplierId = item.SupplierId;
        //                    l.WareHouseId = item.WarehouseId;
        //                    l.CompanyId = item.CompanyId;
        //                    l.WareHouseName = item.WarehouseName;
        //                    l.OrderDetailsId = item.OrderDetailsId;
        //                    l.itemNumber = item.SKUCode;
        //                    l.PurchaseSku = item.PurchaseSku;
        //                    l.orderIDs = item.OrderDetailsId + "," + l.orderIDs;
        //                    l.ItemId = item.ItemId;
        //                    l.ItemName = item.ItemName;
        //                    l.qty = l.qty + item.qty;/////calculate total qty
        //                    l.currentinventory = item.CurrentInventory;
        //                    l.Price = item.Price;
        //                    l.ItemMultiMRPId = item.ItemMultiMRPId;//multimrp

        //                    uniquelist.Add(l);
        //                }
        //                else
        //                {
        //                    l.orderIDs = item.OrderDetailsId + "," + l.orderIDs;
        //                    l.qty = l.qty + item.qty;
        //                    uniquelist.First(d => d.PurchaseSku == item.PurchaseSku).qty = l.qty;
        //                    uniquelist.First(d => d.PurchaseSku == item.PurchaseSku).orderIDs = l.orderIDs;
        //                }
        //            }
        //            List<CurrentNetStockDTO> CurrentStockData = new List<CurrentNetStockDTO>();
        //            foreach (var ss in CurrentStockList)
        //            {
        //                var temp = uniquelist.Where(k => k.itemNumber == ss.ItemNumber && k.ItemMultiMRPId == ss.ItemMultiMRPId).FirstOrDefault();
        //                if (temp != null)
        //                {
        //                    ss.CurrentInventory = ss.CurrentInventory - Convert.ToInt32(temp.qty);
        //                    CurrentStockData.Add(ss);
        //                }
        //                else
        //                {
        //                    CurrentStockData.Add(ss);
        //                }
        //            }
        //            foreach (var CSt in CurrentStockData)
        //            {

        //                CurrentStockDTOM obj = new CurrentStockDTOM();
        //                if (CSt.CurrentInventory == 0) { }
        //                else
        //                {
        //                    var item = HubitemList.Where(m => m.ItemMultiMRPId == CSt.ItemMultiMRPId && m.WarehouseId == CSt.WarehouseId && m.PurchasePrice > 0).FirstOrDefault();
        //                    if (item == null)
        //                    {
        //                        var itemmater = HubitemList.Where(x => x.Number == CSt.ItemNumber && x.WarehouseId == CSt.WarehouseId).FirstOrDefault();
        //                        if (itemmater != null && itemmater.PurchasePrice > 0)
        //                        {
        //                            obj.Unitprice = itemmater.UnitPrice;
        //                            obj.PurchasePrice = itemmater.PurchasePrice;
        //                            obj.IsActive = itemmater.IsActive;
        //                        }
        //                        else
        //                        {
        //                            obj.Unitprice = 0;
        //                            obj.PurchasePrice = 0;
        //                            obj.IsActive = false;
        //                        }
        //                    }
        //                    else
        //                    {
        //                        obj.Unitprice = item.UnitPrice;
        //                        obj.PurchasePrice = item.PurchasePrice;
        //                        if (HubitemList.Any(p => p.ItemMultiMRPId == CSt.ItemMultiMRPId && p.IsActive))
        //                        {
        //                            obj.IsActive = HubitemList.FirstOrDefault(p => p.ItemMultiMRPId == CSt.ItemMultiMRPId && p.IsActive).IsActive;
        //                        }
        //                        else
        //                        {
        //                            obj.IsActive = item.IsActive;
        //                        }
        //                    }
        //                    obj.ItemName = CSt.itemname;
        //                    obj.CurrentInventory = CSt.CurrentInventory;
        //                    obj.CurrentNetStockAmount = CSt.CurrentInventory * obj.PurchasePrice;
        //                    obj.StockId = CSt.StockId;
        //                    obj.WarehouseName = CSt.WarehouseName;
        //                    obj.ItemNumber = CSt.ItemNumber;
        //                    obj.ItemMultiMrpId = CSt.ItemMultiMRPId;
        //                    obj.OpenPOQTy = OpenPoQtyList.Any(o => o.ItemMultiMrpId == CSt.ItemMultiMRPId) ? OpenPoQtyList.Where(p => p.ItemMultiMrpId == CSt.ItemMultiMRPId).Sum(p => p.OpenPOQTy) : 0;
        //                    obj.MRP = CSt.MRP;
        //                    obj.CreationDate = CSt.CreationDate;
        //                    obj.CurrentDeliveryCanceledInventory = CurrentNetDeliveryCancelList.Any(p => p.ItemMultiMRPId == CSt.ItemMultiMRPId) ? CurrentNetDeliveryCancelList.Where(p => p.ItemMultiMRPId == CSt.ItemMultiMRPId).Sum(p => p.qty) : 0;
        //                    obj.DeliveryCancelDetails = CurrentNetDeliveryCancelList.Any(p => p.ItemMultiMRPId == CSt.ItemMultiMRPId) ? CurrentNetDeliveryCancelList.Where(p => p.ItemMultiMRPId == CSt.ItemMultiMRPId).ToList() : null;
        //                    obj.FreestockNetInventory = ObjfreestockList.Any(f => f.ItemMultiMRPId == CSt.ItemMultiMRPId) ? ObjfreestockList.Where(f => f.ItemMultiMRPId == CSt.ItemMultiMRPId).Sum(f => f.QTY) : 0;
        //                    obj.AverageAging = ObjAveragingDc.Where(f => f.ItemMultiMrpId == CSt.ItemMultiMRPId).Select(f => f.AverageAging).FirstOrDefault();
        //                    obj.AveragePurchasePrice= ObjAveragingDc.Where(f => f.ItemMultiMrpId == CSt.ItemMultiMRPId).Select(f => f.APP).FirstOrDefault();
        //                    FinalList.Add(obj);
        //                }
        //            }






        //            //List<CurrentStockDTOM> Objfreeitemlist = AddFreeItem(WarehouseId, FinalList[0].WarehouseName, OpenPoQtyList, CurrentNetDeliveryCancelList);
        //            //FinalList.AddRange(Objfreeitemlist);
        //            return FinalList;
        //        }
        //        catch (Exception e)
        //        {
        //            throw e;
        //        }
        //    }


        //}
        #endregion
        #endregion
        private List<FreeStockDemandDC> GetFreestock(int WareHouseId)
        {
            try
            {
                using (AuthContext Context = new AuthContext())
                {
                    var param = new SqlParameter
                    {
                        ParameterName = "WarehouseId",
                        Value = WareHouseId
                    };

                    List<FreeStockDemandDC> objFreeStock = Context.Database.SqlQuery<FreeStockDemandDC>("exec [GetFreestockNetInventory] @WarehouseId", param).ToList();
                    return objFreeStock;

                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private List<AveragAgingDC> GetAvg(List<ItemNetStockDTO> objitemlist, AuthContext db)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("ItemmultiMRPId");
            dt.Columns.Add("WareHouseId");
            dt.Columns.Add("IsFreeItem");



            objitemlist.ForEach(x =>
            {
                DataRow dr = dt.NewRow();
                dr["ItemmultiMRPId"] = x.ItemMultiMRPId;
                dr["WareHouseId"] = x.WarehouseId;
                dr["IsFreeItem"] = null;
                dt.Rows.Add(dr);

            });






            var param2 = new SqlParameter
            {
                TypeName = "dbo.itemtype",
                ParameterName = "itemtype",
                Value = dt
            };
            List<AveragAgingDC> AveragAgingDC = db.Database.SqlQuery<AveragAgingDC>("exec [GetAvgAgingForItems] @itemtype", param2).ToList();
            return AveragAgingDC;

        }


        [Route("GetAllAveraginglist")]
        [HttpGet]
        public List<DataDTO> GetAll(int warehouseId, int itemMultiMRPId)
        {

            using (var context = new AuthContext())
            {

                DataTable dt = new DataTable();
                dt.Columns.Add("ItemmultiMRPId");
                dt.Columns.Add("WareHouseId");
                dt.Columns.Add("IsFreeItem");

                DataRow dr = dt.NewRow();
                dr["ItemmultiMRPId"] = itemMultiMRPId;
                dr["WareHouseId"] = warehouseId;
                dr["IsFreeItem"] = null;
                dt.Rows.Add(dr);

                var param = new SqlParameter
                {
                    TypeName = "dbo.itemtype",
                    ParameterName = "itemtype",
                    Value = dt
                };

                List<DataDTO> _result = context.Database.SqlQuery<DataDTO>("exec [GetAgingForItems] @itemtype", param).ToList();
                return _result;


            }
        }


        //private List<CurrentStockDTOM> AddFreeItem(int WarehouseId, string WareHouseName, List<OpenPoQtyDC> OpenPoQtyList, List<CurrentNetDeliveryCancelDTO> CurrentNetDeliveryCancelList)
        //{
        //    List<CurrentStockDTOM> ObjFreeItemList = new List<CurrentStockDTOM>();
        //    List<OrderDetails> ObjGetFreeItemList = GetFreeItemList(WarehouseId);
        //    List<FreeStock> ObjFreeStock = GetFreeFreeStock(WarehouseId);
        //    foreach (var Fst in ObjFreeStock)
        //    {
        //        CurrentStockDTOM obj = new CurrentStockDTOM();
        //        if (Fst.CurrentInventory > 0)
        //        {
        //            var item = ObjGetFreeItemList.Where(m => m.ItemMultiMRPId == Fst.ItemMultiMRPId && m.WarehouseId == WarehouseId && m.NetPurchasePrice > 0).FirstOrDefault();
        //            if (item == null)
        //            {
        //                var itemmater = ObjGetFreeItemList.Where(x => x.itemNumber == Fst.ItemNumber && x.WarehouseId == WarehouseId).FirstOrDefault();
        //                if (itemmater != null && itemmater.NetPurchasePrice > 0)
        //                {
        //                    obj.Unitprice = itemmater.UnitPrice;
        //                    obj.PurchasePrice = itemmater.NetPurchasePrice;
        //                    obj.IsActive = true;
        //                }
        //                else
        //                {
        //                    obj.Unitprice = 0;
        //                    obj.PurchasePrice = 0;
        //                    obj.IsActive = false;
        //                }
        //            }
        //            else
        //            {
        //                obj.Unitprice = item.UnitPrice;
        //                obj.PurchasePrice = item.NetPurchasePrice;
        //                if (ObjGetFreeItemList.Any(p => p.ItemMultiMRPId == Fst.ItemMultiMRPId))
        //                {
        //                    obj.IsActive = true;
        //                }
        //                else
        //                {
        //                    obj.IsActive = false;
        //                }
        //            }
        //        }
        //        obj.ItemName = Fst.itemname;
        //        obj.CurrentInventory = Fst.CurrentInventory;
        //        obj.CurrentNetStockAmount = Fst.CurrentInventory * obj.PurchasePrice;
        //        obj.StockId = Fst.FreeStockId;
        //        obj.WarehouseName = WareHouseName;
        //        obj.ItemNumber = Fst.ItemNumber;
        //        obj.ItemMultiMrpId = Fst.ItemMultiMRPId;
        //        obj.OpenPOQTy = OpenPoQtyList.Any(o => o.ItemMultiMrpId == Fst.ItemMultiMRPId) ? OpenPoQtyList.Where(p => p.ItemMultiMrpId == Fst.ItemMultiMRPId).Sum(p => p.OpenPOQTy) : 0;
        //        obj.MRP = Fst.MRP;
        //        obj.CreationDate = Fst.CreationDate;
        //        obj.CurrentDeliveryCanceledInventory = CurrentNetDeliveryCancelList.Any(p => p.ItemMultiMRPId == Fst.ItemMultiMRPId) ? CurrentNetDeliveryCancelList.Where(p => p.ItemMultiMRPId == Fst.ItemMultiMRPId).Sum(p => p.qty) : 0;
        //        obj.DeliveryCancelDetails = CurrentNetDeliveryCancelList.Any(p => p.ItemMultiMRPId == Fst.ItemMultiMRPId) ? CurrentNetDeliveryCancelList.Where(p => p.ItemMultiMRPId == Fst.ItemMultiMRPId).ToList() : null;
        //        ObjFreeItemList.Add(obj);
        //    }
        //    return ObjFreeItemList;


        //}
        private List<OrderDetails> GetFreeItemList(int WarehouseId)
        {
            try
            {
                using (AuthContext context = new AuthContext())
                {
                    List<OrderDetails> objOrderDetails = context.DbOrderDetails.Where(x => x.WarehouseId == WarehouseId && x.IsDispatchedFreeStock == true).ToList();
                    return objOrderDetails;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        private List<FreeStock> GetFreeFreeStock(int WarehouseId)
        {
            try
            {
                using (AuthContext context = new AuthContext())
                {
                    List<FreeStock> objFreeStockDetails = context.FreeStockDB.Where(x => x.WarehouseId == WarehouseId).ToList();
                    return objFreeStockDetails;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }




        [Authorize]
        [Route("")]
        [HttpGet]
        public HttpResponseMessage GetWarehousebased(int WarehouseId)
        {

            try
            {
                using (AuthContext context = new AuthContext())
                {
                    var param = new SqlParameter
                    {
                        ParameterName = "WarehouseId",
                        Value = WarehouseId
                    };

                    List<CurrentStockDTOM> ObjCurrentNetstock = context.Database.SqlQuery<CurrentStockDTOM>("exec [CurrentNetStockNew] @WareHouseId", param).ToList();
                    if (ObjCurrentNetstock != null && ObjCurrentNetstock.Any())
                    {
                        RetailerAppManager retailerAppManager = new RetailerAppManager();
                        List<DataContracts.Masters.Store.StoreCategorySubCategoryBrand> StoreCategorySubCategoryBrands = retailerAppManager.GetStoreWithDetail();
                        string storeName = "";
                        ObjCurrentNetstock.ForEach(item =>
                        {
                            if (StoreCategorySubCategoryBrands != null && StoreCategorySubCategoryBrands.Any(x => x.Categoryid == item.Categoryid && x.SubCategoryId == item.SubCategoryId && x.BrandId == item.SubsubCategoryid))
                            {
                                var store = StoreCategorySubCategoryBrands.First(x => x.Categoryid == item.Categoryid && x.SubCategoryId == item.SubCategoryId && x.BrandId == item.SubsubCategoryid);
                                storeName = store.StoreName;
                                item.StoreName = storeName;
                                item.CategoryName = StoreCategorySubCategoryBrands.FirstOrDefault(x => x.Categoryid == item.Categoryid).categoryName;
                                item.SubCategoryName = StoreCategorySubCategoryBrands.FirstOrDefault(x => x.SubCategoryId == item.SubCategoryId).SubCategoryName;
                                item.SubsubCategoryName = StoreCategorySubCategoryBrands.FirstOrDefault(x => x.BrandId == item.SubsubCategoryid).BrandName;
                            }
                        });

                        TripPlannerHelper tripPlannerHelper = new TripPlannerHelper();
                        List<ItemWarehouseDc> itemWarehouseDcs = new List<ItemWarehouseDc>();
                        var itemWarehouse = ObjCurrentNetstock.Select(x => new ItemWarehouseDc { WarehouseId = x.WarehouseId, ItemMultiMRPId = x.ItemMultiMrpId }).ToList();
                        var list = tripPlannerHelper.RocTagValueGet(itemWarehouse);
                        if (list != null)
                        {
                            foreach (var da in ObjCurrentNetstock)
                            {
                                da.Tag = list.Result.Where(x => x.ItemMultiMRPId == da.ItemMultiMrpId && x.WarehouseId == da.WarehouseId).Select(x => x.Tag).FirstOrDefault();
                            }
                        }
                    }
                    return Request.CreateResponse(HttpStatusCode.OK, ObjCurrentNetstock);
                }
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex.GetBaseException().Message.ToString());

            }
        }


        [AllowAnonymous]
        [Route("GetWarehouseStock")]
        [HttpGet]
        public HttpResponseMessage GetWareHouse(int WarehouseId)
        {

            try
            {
                using (AuthContext context = new AuthContext())
                {
                    var param = new SqlParameter
                    {
                        ParameterName = "WarehouseId",
                        Value = WarehouseId
                    };

                    List<CurrentStockDTOM> ObjCurrentNetstock = context.Database.SqlQuery<CurrentStockDTOM>("exec [CurrentNetStock] @WareHouseId", param).ToList();
                    return Request.CreateResponse(HttpStatusCode.OK, ObjCurrentNetstock);
                }
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex.GetBaseException().Message.ToString());

            }
        }

        [Authorize]
        [HttpGet]
        [AllowAnonymous]
        [Route("CurrentDelieveryCancel")]
        public HttpResponseMessage CurrentDelieveryCancel(int WarehouseId, int ItemMultiMrpId)
        {
            try
            {
                using (AuthContext context = new AuthContext())
                {
                    var param1 = new SqlParameter
                    {
                        ParameterName = "WarehouseId",
                        Value = WarehouseId
                    };

                    var param2 = new SqlParameter
                    {
                        ParameterName = "ItemMultiMrpId",
                        Value = ItemMultiMrpId
                    };


                    List<CurrentNetDeliveryCancelDTO> ObjCurrentNetDeliveryCancelDTO = context.Database.SqlQuery<CurrentNetDeliveryCancelDTO>("exec [GetDelievryCancelList] @WareHouseId,@ItemMultiMrpId", param1, param2).ToList();
                    return Request.CreateResponse(HttpStatusCode.OK, ObjCurrentNetDeliveryCancelDTO);
                }
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex.GetBaseException().Message.ToString());

            }
        }
    }

    public class CurrentStockDTOM
    {
        public int StockId { get; set; }
        public string CityName { get; set; }
        public string WarehouseName { get; set; }
        public string ItemNumber { get; set; }
        public string ItemName { get; set; }
        public int CurrentInventory { get; set; }
        public string LiveQty { get; set; }
        public int CurrentDeliveryCanceledInventory { get; set; }

        public int FreestockNetInventory { get; set; }
        public double? CurrentNetStockAmount { get; set; }
        public double? Unitprice { get; set; }
        public double? PurchasePrice { get; set; }
        public DateTime CreationDate { get; set; }
        public double? MRP { get; set; }
        public int ItemMultiMrpId { get; set; }
        public int IsActive { get; set; }
        public List<CurrentNetDeliveryCancelDTO> DeliveryCancelDetails { get; set; }
        public int OpenPOQTy { get; set; }

        public int FreeDemand { get; set; }

        public int AverageAging { get; set; }

        public double? AveragePurchasePrice { get; set; }

        public int CurrentNetInventory { get; set; }

        public int NetInventory { get; set; }

        public string Cateogries { get; set; }

        public string ABCClassification { get; set; }
        public double? AgingAvgPurchasePrice { get; set; }
        public double? MarginPercent { get; set; }
        public int ItemlimitQty { get; set; }
        public int ItemLimitSaleQty { get; set; }
        public int PurchaseMinOrderQty { get; set; }
        public int Categoryid { get; set; }
        public int SubCategoryId { get; set; }
        public int SubsubCategoryid { get; set; }
        public string CategoryName { get; set; }
        public string SubCategoryName { get; set; }
        public string SubsubCategoryName { get; set; }
        public string StoreName { get; set; }
        public int WarehouseId { get; set; } // new add

        public int Tag { get; set; }  // new add
    }

    public class ItemNetStockDTO
    {
        public int ItemMultiMRPId { get; set; }
        public bool IsSensitive { get; set; }
        public bool IsSensitiveMRP { get; set; }
        public string Number { get; set; }
        //public string itemname { get; set; }
        public int WarehouseId { get; set; }
        public double PurchasePrice { get; set; }
        public double UnitPrice { get; set; }
        public bool IsActive { get; set; }

    }
    public class CurrentNetStockDTO
    {
        public int ItemMultiMRPId { get; set; }
        public int StockId { get; set; }
        public string ItemNumber { get; set; }
        public string itemname { get; set; }
        public int WarehouseId { get; set; }
        public double PurchasePrice { get; set; }
        public double MRP { get; set; }
        public double UnitPrice { get; set; }
        public int CurrentInventory { get; set; }
        public string WarehouseName { get; set; }
        public DateTime CreationDate { get; set; }
    }

    public class CurrentNetDeliveryCancelDTO
    {
        public int ItemMultiMRPId { get; set; }
        public string itemNumber { get; set; }
        public int WarehouseId { get; set; }
        public int qty { get; set; }
        public int OrderId { get; set; }
        public bool IsFreeItem { get; set; }

    }

    public class OpenPoQtyDC
    {
        public int OpenPOQTy { get; set; } //Send for Approval, Approved,Self Approved	/CN Partial Received	
        public int ItemMultiMrpId { get; set; }
    }



    public class AveragAgingDC
    {
        public int AverageAging { get; set; }
        public int ItemMultiMrpId { get; set; }
        public int WarehouseId { get; set; }

        public double APP { get; set; }
    }

    public class FreeStockDemandDC
    {
        public int ItemMultiMRPId { get; set; }
        public int QTY { get; set; }
    }





    public class DataDTO
    {
        public string WarehouseName { get; set; }
        public int ItemMultiMRPId { get; set; }
        public int WarehouseId { get; set; }

        public DateTime InDate { get; set; }

        public int Ageing { get; set; }

        public int ClosingQty { get; set; }
        public double ClosingAmount { get; set; }


    }
}



