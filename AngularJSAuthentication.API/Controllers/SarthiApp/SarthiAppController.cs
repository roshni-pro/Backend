using AgileObjects.AgileMapper;
using AngularJSAuthentication.API.App_Code.PackingMaterial;
using AngularJSAuthentication.API.Controllers;
using AngularJSAuthentication.API.Controllers.Base;
using AngularJSAuthentication.API.Helper;
using AngularJSAuthentication.API.Helper.GDN;
using AngularJSAuthentication.BusinessLayer.Managers.Masters;
using AngularJSAuthentication.Common.Enums;
using AngularJSAuthentication.Common.Helpers;
using AngularJSAuthentication.DataContracts.BatchCode;
using AngularJSAuthentication.DataContracts.External.SalesAppDc;
using AngularJSAuthentication.DataContracts.JustInTime;
using AngularJSAuthentication.DataContracts.Masters.Batch;
using AngularJSAuthentication.DataContracts.Masters.Store;
using AngularJSAuthentication.DataContracts.Transaction.PurchaseOrder;
using AngularJSAuthentication.DataContracts.Transaction.Stocks;
using AngularJSAuthentication.Model;
using AngularJSAuthentication.Model.PurchaseOrder;
using AngularJSAuthentication.Model.Stocks;
using AngularJSAuthentication.Model.Stocks.Batch;
using LinqKit;
using Nito.AsyncEx;
using NLog;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using System.Web;
using System.Web.Http;

namespace AngularJSAuthentication.API.ControllerV7
{
    [RoutePrefix("api/SarthiApp")]
    public class SarthiAppController : BaseAuthController
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        private static TimeZoneInfo INDIAN_ZONE = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
        DateTime indianTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE);

        #region Old Sarthi

        #region Provide Po on Search by PO id & warehouseid
        /// <summary>
        /// GetPOList () Staus : ('Self Approved', 'Approved', 'UN Partial Received','CN Partial Received') 
        /// </summary>
        /// <param name="WarehouseId"></param>
        /// <returns></returns>
        [Route("getPOData")]
        [HttpGet]
        public HttpResponseMessage GetPOList(int PurchaseOrderId, int WarehouseId)
        {
            bool status = false;
            string resultMessage = "";
            SarthiManager manager = new SarthiManager();
            PurchaseOrderMasterDc PurchaseOrderMaster = manager.GetPOList(PurchaseOrderId, WarehouseId);
            if (PurchaseOrderMaster != null)
            {
                using (AuthContext context = new AuthContext())
                {
                    bool IsAnyRejectedGr = context.GoodsReceivedDetail.Any(x => x.PurchaseOrderDetail.PurchaseOrderId == PurchaseOrderMaster.PurchaseOrderId && x.Status == 3 && x.IsDeleted == false);
                    PurchaseOrderMaster.IsAnyRejectedGr = IsAnyRejectedGr;

                    bool IsGDN = context.GoodsDescripancyNoteMasterDB.Any(x => x.PurchaseOrderId == PurchaseOrderId && x.IsActive == true && x.IsDeleted == false);
                    PurchaseOrderMaster.IsGDN = IsGDN;
                }
                resultMessage = "Result found";
                status = true;
            }
            else
            {
                resultMessage = "No Record found";
                status = true;
            }
            var res = new
            {
                PurchaseOrderMasterDc = PurchaseOrderMaster,
                status = status,
                Message = resultMessage
            };
            return Request.CreateResponse(HttpStatusCode.OK, res);
        }
        #endregion

        #region Provide PO Details
        [Route("getPODetails")]
        [HttpGet]
        public HttpResponseMessage GetPODetails(int PurchaseOrderId)
        {
            try
            {
                using (AuthContext context = new AuthContext())
                {

                    var PO = context.DPurchaseOrderMaster.Where(x => x.PurchaseOrderId == PurchaseOrderId).Include(x => x.PurchaseOrderDetail).SingleOrDefault();
                    //List<PurchaseOrderDetail> purchaseorderDetails = context.DPurchaseOrderDeatil.Where(x => x.PurchaseOrderId == PO.PurchaseOrderId).ToList();
                    //PO.PurchaseOrderDetail = purchaseorderDetails;
                    var res = new
                    {
                        PODetail = PO,
                        status = true,
                        Message = "Record found."
                    };
                    return Request.CreateResponse(HttpStatusCode.OK, res);
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex.ToString());
                var res = new
                {
                    PODetail = "",
                    status = false,
                    Message = "No Record found."
                };
                return Request.CreateResponse(HttpStatusCode.OK, res);
            }
        }
        #endregion

        #region Pending POCount and List of po
        [Route("POCount")]
        [HttpGet]
        public async Task<POCountDc> POCount(int WarehouseId)
        {
            SarthiManager manager = new SarthiManager();
            return await manager.POCount(WarehouseId);
        }
        #endregion

        #region Provide PO Date For GR
        /// <summary>
        /// Requested gr data
        /// </summary>
        /// <param name="PurchaseOrderId"></param>
        /// <returns></returns>
        [Route("POGRDetails")]
        [HttpGet]
        public HttpResponseMessage GetPOGRDetails(int PurchaseOrderId)
        {
            string Message = "";

            using (AuthContext context = new AuthContext())
            {
                POGR _POGR = new POGR();
                List<PurchaseOrderDetailDc> PurchaseOrderDetailDc = new List<PurchaseOrderDetailDc>();
                var PurchaseOrderRecived = context.DPurchaseOrderMaster.Where(x => x.PurchaseOrderId == PurchaseOrderId).SingleOrDefault();


                if (PurchaseOrderRecived.GRcount == null) { PurchaseOrderRecived.GRcount = 0; }

                if (PurchaseOrderRecived.GRcount < 5 && (PurchaseOrderRecived.Status == "Self Approved" || PurchaseOrderRecived.Status == "Approved" || PurchaseOrderRecived.Status == "Partial Received" || PurchaseOrderRecived.Status == "CN Partial Received" || PurchaseOrderRecived.Status == "Received"))
                {
                    var purchaseorderDetailsR = context.PurchaseOrderRecivedDetails.Where(x => x.PurchaseOrderId == PurchaseOrderId).Select(j =>
                                                                new PurchaseOrderDetailDc
                                                                {
                                                                    PurchaseOrderDetailId = j.PurchaseOrderDetailId,
                                                                    PurchaseOrderId = j.PurchaseOrderId,
                                                                    ItemNumber = j.ItemNumber,
                                                                    ItemName = j.ItemName,
                                                                    Price = j.Price,
                                                                    MRP = j.MRP,
                                                                    MOQ = j.MOQ,
                                                                    PurchaseSku = j.PurchaseSku,
                                                                    TotalQuantity = j.TotalQuantity,
                                                                    ReceivingQty = 0,
                                                                    TotalRecivedQty = (j.QtyRecived1 + j.QtyRecived2 + j.QtyRecived3 + j.QtyRecived4 + j.QtyRecived5),
                                                                    ItemMultiMRPId = j.ItemMultiMRPId,
                                                                    BatchNo = null,
                                                                    MFGDate = null,


                                                                }).ToList();
                    if (purchaseorderDetailsR.Count > 0)
                    {
                        List<string> itemNumber = purchaseorderDetailsR.Select(x => x.ItemNumber).ToList();
                        var itemdetail = context.ItemMasterCentralDB.Where(c => itemNumber.Contains(c.Number)).Select(x => new { x.Number }).ToList();
                        var barcodes = context.ItemBarcodes.Where(c => itemNumber.Contains(c.ItemNumber) && c.IsDeleted == false && c.IsActive).Distinct().ToList();


                        var itemMultiMrpdetail = context.ItemMultiMRPDB.Where(a => itemNumber.Contains(a.ItemNumber) && a.Deleted == false).Select(x => new ItemMultiMRPDc { ItemNumber = x.ItemNumber, ItemMultiMRPId = x.ItemMultiMRPId, MRP = x.MRP }).ToList();
                        purchaseorderDetailsR.ForEach(x =>
                        {
                            if (itemdetail != null && itemdetail.Any(y => y.Number == x.ItemNumber))
                            {
                                x.Barcode = (barcodes != null && barcodes.Any(y => y.ItemNumber == x.ItemNumber)) ? barcodes.FirstOrDefault(y => y.ItemNumber == x.ItemNumber).Barcode : null;
                            }

                            if (itemMultiMrpdetail != null && itemMultiMrpdetail.Any(y => y.ItemNumber == x.ItemNumber))
                            {
                                x.multiMrpIds = itemMultiMrpdetail.Where(y => y.ItemNumber == x.ItemNumber).ToList();
                            }
                        });
                        if (PurchaseOrderRecived.GRcount == 1)
                        {
                            _POGR.GrNumber = PurchaseOrderRecived.PurchaseOrderId + "B";
                        }
                        else if (PurchaseOrderRecived.GRcount == 2)
                        {
                            _POGR.GrNumber = PurchaseOrderRecived.PurchaseOrderId + "C";
                        }
                        if (PurchaseOrderRecived.GRcount == 3)
                        {
                            _POGR.GrNumber = PurchaseOrderRecived.PurchaseOrderId + "D";
                        }
                        if (PurchaseOrderRecived.GRcount == 4)
                        {
                            _POGR.GrNumber = PurchaseOrderRecived.PurchaseOrderId + "E";
                        }
                        _POGR.PurchaseOrderDetailDc = purchaseorderDetailsR;

                        var res = new
                        {
                            POGRDetail = _POGR,
                            status = true,
                            Message = "Record found."
                        };
                        return Request.CreateResponse(HttpStatusCode.OK, res);
                    }
                    else
                    {
                        var PurchaseOrderDeatil = context.DPurchaseOrderDeatil.Where(x => x.PurchaseOrderId == PurchaseOrderId).Select(j =>
                                                      new PurchaseOrderDetailDc
                                                      {
                                                          PurchaseOrderDetailId = j.PurchaseOrderDetailId,
                                                          PurchaseOrderId = j.PurchaseOrderId,
                                                          ItemNumber = j.ItemNumber,
                                                          ItemName = j.ItemName,
                                                          Price = j.Price,
                                                          MRP = j.MRP,
                                                          MOQ = j.MOQ,
                                                          PurchaseSku = j.PurchaseSku,
                                                          TotalQuantity = j.TotalQuantity,
                                                          ReceivingQty = 0,
                                                          TotalRecivedQty = 0,
                                                          ItemMultiMRPId = j.ItemMultiMRPId,
                                                          BatchNo = null,
                                                          MFGDate = null,
                                                      }).ToList();

                        if (PurchaseOrderDeatil != null)
                        {
                            List<string> itemNumber = PurchaseOrderDeatil.Select(x => x.ItemNumber).ToList();
                            var itemdetail = context.ItemMasterCentralDB.Where(c => itemNumber.Contains(c.Number)).Select(x => new { x.Number }).ToList();

                            var barcodes = context.ItemBarcodes.Where(c => itemNumber.Contains(c.ItemNumber) && c.IsDeleted == false && c.IsActive).Distinct().ToList();

                            var itemMultiMrpdetail = context.ItemMultiMRPDB.Where(a => itemNumber.Contains(a.ItemNumber) && a.Deleted == false).Select(x => new ItemMultiMRPDc { ItemNumber = x.ItemNumber, ItemMultiMRPId = x.ItemMultiMRPId, MRP = x.MRP }).ToList();
                            PurchaseOrderDeatil.ForEach(x =>
                            {
                                if (itemdetail != null && itemdetail.Any(y => y.Number == x.ItemNumber))
                                {
                                    x.Barcode = (barcodes != null && barcodes.Any(y => y.ItemNumber == x.ItemNumber)) ? barcodes.FirstOrDefault(y => y.ItemNumber == x.ItemNumber).Barcode : null;
                                }

                                if (itemMultiMrpdetail != null && itemMultiMrpdetail.Any(y => y.ItemNumber == x.ItemNumber))
                                {
                                    x.multiMrpIds = itemMultiMrpdetail.Where(y => y.ItemNumber == x.ItemNumber).ToList();
                                }
                            });
                        }
                        _POGR.GrNumber = PurchaseOrderRecived.PurchaseOrderId + "A";
                        _POGR.PurchaseOrderDetailDc = PurchaseOrderDeatil;
                        var res = new
                        {
                            POGRDetail = _POGR,
                            status = true,
                            Message = "Record found."
                        };
                        return Request.CreateResponse(HttpStatusCode.OK, res);

                    }
                }
                else
                {

                    bool IsAnyinRejectGR = (PurchaseOrderRecived.Gr1Status == "RejectGR" || PurchaseOrderRecived.Gr2Status == "RejectGR" || PurchaseOrderRecived.Gr3Status == "RejectGR" || PurchaseOrderRecived.Gr4Status == "RejectGR" || PurchaseOrderRecived.Gr5Status == "RejectGR");
                    if (IsAnyinRejectGR)
                    {

                        Message = "Current GR in Reject Mode ,Please approve from Backend portal ";

                    }
                    else
                    {
                        Message = "You can't do GR Now, Because Currently it is under status :" + PurchaseOrderRecived.Status;

                    }

                    if (PurchaseOrderRecived.GRcount == 5)
                    {
                        Message = "You have already done 5 Gr of This PO";
                    }

                    var res = new
                    {
                        POGRDetail = _POGR,
                        status = true,
                        Message = Message
                    };
                    return Request.CreateResponse(HttpStatusCode.OK, res);
                }
            }

        }
        #endregion

        //#region Addd in Temporary  GR before approved
        ///// <summary>
        ///// Temporary  GR before approved
        ///// created on 11/11/2019
        ///// </summary>
        ///// <param name="pom"></param>
        ///// <returns></returns>
        //[Route("AddPOGRDetails")]
        //[HttpPost]
        //[AllowAnonymous]
        //public HttpResponseMessage AddPOGRDetails(AddPOGRDetailDc AddPOGRDetailobj)
        //{
        //    //var identity = User.Identity as ClaimsIdentity;
        //    //int compid = 0, userid = 0;
        //    //// Access claims
        //    //if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))
        //    //    compid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);

        //    //if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
        //    //    userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
        //    bool IsValidate = false;
        //    bool IsValidateQTy = false;
        //    string resultMessage = "";
        //    using (AuthContext context = new AuthContext())
        //    {
        //        using (var dbContextTransaction = context.Database.BeginTransaction())
        //        {

        //            if (AddPOGRDetailobj.PurchaseOrderDetailDc != null && AddPOGRDetailobj.PurchaseOrderDetailDc.Any())
        //            {
        //                int peopleId = AddPOGRDetailobj.UserId;
        //                People people = context.Peoples.Where(q => q.PeopleID == peopleId && q.Active == true).SingleOrDefault();
        //                int? count = 0;//recieving count
        //                Int32 PoId = AddPOGRDetailobj.PurchaseOrderDetailDc[0].PurchaseOrderId;//poid
        //                PurchaseOrderMaster PurchaseOrderMaster = context.DPurchaseOrderMaster.Where(x => x.PurchaseOrderId == PoId).SingleOrDefault();
        //                if (PurchaseOrderMaster.GRcount == null) { PurchaseOrderMaster.GRcount = 0; }
        //                if (PurchaseOrderMaster != null && PurchaseOrderMaster.GRcount < 6 && people != null && (PurchaseOrderMaster.Status == "Self Approved" || PurchaseOrderMaster.Status == "Approved" || PurchaseOrderMaster.Status == "Partial Received" || PurchaseOrderMaster.Status == "CN Partial Received"))
        //                {
        //                    var PurchaseOrderRecived = context.PurchaseOrderRecivedDetails.Where(x => x.PurchaseOrderId == PoId).ToList();//first gr done PurchaseOrderRecivedDetails
        //                    var PurchaseOrderDeatil = context.DPurchaseOrderDeatil.Where(x => x.PurchaseOrderId == PoId).ToList();// on first gr insert DPurchaseOrderDeatil

        //                    if (PurchaseOrderDeatil.Count == AddPOGRDetailobj.PurchaseOrderDetailDc.Count)
        //                    {
        //                        if (PurchaseOrderRecived.Count == 0)
        //                        {
        //                            int TotalPOQty = PurchaseOrderDeatil.Sum(x => x.TotalQuantity);
        //                            int? TotalPORecQty = AddPOGRDetailobj.PurchaseOrderDetailDc.Sum(x => x.ReceivingQty);
        //                            if (TotalPOQty < TotalPORecQty)
        //                            {
        //                                resultMessage = "Receive Quantity should be Less then Total Quantity ";
        //                                IsValidateQTy = false;

        //                            }
        //                            else
        //                            {
        //                                count = 1;
        //                                IsValidateQTy = true;
        //                            }

        //                        }
        //                        else
        //                        {

        //                            int TotalPOQty = PurchaseOrderDeatil.Sum(x => x.TotalQuantity);
        //                            int? POMTotalPORecQty = AddPOGRDetailobj.PurchaseOrderDetailDc.Sum(x => x.ReceivingQty);
        //                            int? TotalPORecQty = PurchaseOrderRecived.Sum(x => x.QtyRecived1 + (x.QtyRecived2.HasValue ? x.QtyRecived2.Value : 0) + (x.QtyRecived3.HasValue ? x.QtyRecived3.Value : 0) + (x.QtyRecived4.HasValue ? x.QtyRecived4.Value : 0) + (x.QtyRecived5.HasValue ? x.QtyRecived5.Value : 0));
        //                            if (TotalPOQty < (TotalPORecQty + POMTotalPORecQty))
        //                            {
        //                                resultMessage = "Receive Quantity should be Less then Total Quantity ";
        //                                IsValidateQTy = false;

        //                            }
        //                            else
        //                            {
        //                                count = PurchaseOrderMaster.GRcount + 1;
        //                                IsValidateQTy = true;
        //                            }

        //                        }
        //                        foreach (var a in AddPOGRDetailobj.PurchaseOrderDetailDc)
        //                        {
        //                            if (PurchaseOrderRecived.Count == 0)
        //                            {
        //                                var qty = PurchaseOrderDeatil.Where(x => x.PurchaseOrderDetailId == a.PurchaseOrderDetailId).SingleOrDefault();
        //                                if (qty.TotalQuantity >= (a.ReceivingQty))
        //                                {
        //                                    IsValidate = true;
        //                                }
        //                                else
        //                                {
        //                                    resultMessage = "Receive Quantity should be Less then Total Quantity : " + a.ItemName;
        //                                    IsValidate = false;
        //                                    break;
        //                                }
        //                            }
        //                            else
        //                            {
        //                                var qty = PurchaseOrderRecived.Where(x => x.PurchaseOrderDetailId == a.PurchaseOrderDetailId).SingleOrDefault();
        //                                if (qty.TotalQuantity >= a.ReceivingQty && qty.TotalQuantity >= ((qty.QtyRecived1.HasValue ? qty.QtyRecived1.Value : 0) + (qty.QtyRecived2.HasValue ? qty.QtyRecived2.Value : 0) + (qty.QtyRecived3.HasValue ? qty.QtyRecived3.Value : 0) + (qty.QtyRecived4.HasValue ? qty.QtyRecived4.Value : 0) + (qty.QtyRecived5.HasValue ? qty.QtyRecived5.Value : 0)))
        //                                {
        //                                    IsValidate = true;
        //                                }
        //                                else
        //                                {
        //                                    resultMessage = "Receive Quantity should be Less then Total Quantity : " + a.ItemName;
        //                                    IsValidate = false;
        //                                    break;
        //                                }
        //                            }
        //                        }
        //                        var FreeItemList = new List<ItemMaster>();
        //                        if (AddPOGRDetailobj.PurchaseOrderFreeItemMasterDC != null && AddPOGRDetailobj.PurchaseOrderFreeItemMasterDC.Any())
        //                        {
        //                            foreach (var freeitem in AddPOGRDetailobj.PurchaseOrderFreeItemMasterDC)
        //                            {

        //                                if (freeitem.TotalQuantity > 0)
        //                                {
        //                                    var FreeItemids = AddPOGRDetailobj.PurchaseOrderFreeItemMasterDC.Select(x => x.ItemId).ToList();
        //                                    FreeItemList = context.itemMasters.Where(x => FreeItemids.Contains(x.ItemId) && x.WarehouseId == PurchaseOrderMaster.WarehouseId && x.Deleted == false).ToList(); // ItemMultiMRPList
        //                                }
        //                                else
        //                                {
        //                                    resultMessage = "Receive Quantity can't be zero for Free item " + freeitem.Itemname;
        //                                    IsValidate = false;
        //                                    break;
        //                                }
        //                            }
        //                        }
        //                        if (IsValidateQTy && IsValidate)
        //                        {
        //                            var itemPurchaseskus = PurchaseOrderDeatil.Select(x => x.PurchaseSku).ToList();
        //                            var ItemMasterCentralList = context.ItemMasterCentralDB.Where(x => itemPurchaseskus.Contains(x.PurchaseSku) && x.Deleted == false).Select(x => x).ToList(); // ItemMasterCentralList
        //                            var Multimrpids = AddPOGRDetailobj.PurchaseOrderDetailDc.Select(x => x.ItemMultiMRPId).ToList();//
        //                            var ItemMultiMRPList = context.ItemMultiMRPDB.Where(x => Multimrpids.Contains(x.ItemMultiMRPId) && x.Deleted == false).ToList(); // ItemMultiMRPList
        //                            var itemNumberids = PurchaseOrderDeatil.Select(x => x.ItemNumber).ToList();


        //                            var TemporaryCurrentStockList = context.TemporaryCurrentStockDB.Where(x => itemNumberids.Contains(x.ItemNumber) && x.WarehouseId == PurchaseOrderMaster.WarehouseId).ToList();
        //                            bool result = PurchaseOrderDetailsRecivedInTempCS(PurchaseOrderMaster, AddPOGRDetailobj, PurchaseOrderRecived, ItemMasterCentralList, ItemMultiMRPList, TemporaryCurrentStockList, FreeItemList, count, people, context);
        //                            if (result)
        //                            {

        //                                if (count == 1)
        //                                {
        //                                    AddPOGRDetailobj.GrNumber = PurchaseOrderMaster.Gr1Number;
        //                                }
        //                                else if (count == 2)
        //                                {
        //                                    AddPOGRDetailobj.GrNumber = PurchaseOrderMaster.Gr2Number;
        //                                }
        //                                else if (count == 3)
        //                                {
        //                                    AddPOGRDetailobj.GrNumber = PurchaseOrderMaster.Gr3Number;
        //                                }
        //                                else if (count == 4)
        //                                {
        //                                    AddPOGRDetailobj.GrNumber = PurchaseOrderMaster.Gr4Number;
        //                                }
        //                                else if (count == 5)
        //                                {
        //                                    AddPOGRDetailobj.GrNumber = PurchaseOrderMaster.Gr5Number;
        //                                }
        //                                dbContextTransaction.Commit(); resultMessage = "GR Posted Succesfully, Wait till Apro";
        //                                IsValidate = true;

        //                            }
        //                            else
        //                            {
        //                                dbContextTransaction.Rollback();
        //                                resultMessage = "Something went wrong";
        //                                IsValidate = false;
        //                            }
        //                        }
        //                    }
        //                    else
        //                    {
        //                        resultMessage = "Currently gr list item count not matched";
        //                        IsValidate = true;
        //                    }

        //                }
        //                else
        //                {
        //                    resultMessage = "You can't do GR Now, Because Currently it is under status :" + PurchaseOrderMaster.Status;
        //                    IsValidate = false;
        //                }
        //            }
        //            else
        //            {
        //                resultMessage = "Post list is null";
        //                IsValidate = false;
        //            }
        //        }
        //        var res = new
        //        {
        //            POGRDetail = AddPOGRDetailobj,
        //            status = IsValidate,
        //            Message = resultMessage

        //        };
        //        return Request.CreateResponse(HttpStatusCode.OK, res);
        //    }
        //}

        //public bool PurchaseOrderDetailsRecivedInTempCS(PurchaseOrderMaster PurchaseOrderMaster, AddPOGRDetailDc AddPOGRDetailobj, List<PurchaseOrderDetailRecived> PurchaseOrderDetailRecived, List<ItemMasterCentral> ItemMasterCentralList, List<ItemMultiMRP> ItemMultiMRPList, List<TemporaryCurrentStock> TemporaryCurrentStockList, List<ItemMaster> FreeItemList, int? count, People people, AuthContext context)
        //{

        //    int warehouseid = PurchaseOrderMaster.WarehouseId;
        //    string VehicleNumber = AddPOGRDetailobj.VehicleNumber;
        //    string VehicleType = AddPOGRDetailobj.VehicleType;
        //    double GrAmount = 0;
        //    List<PurchaseOrderDetailRecived> AddPurchaseOrderDetailRecived = new List<PurchaseOrderDetailRecived>();//addd 
        //    List<PurchaseOrderDetailRecived> UpdatePurchaseOrderDetailRecived = new List<PurchaseOrderDetailRecived>();//addd 

        //    List<TemporaryCurrentStock> UpdateTemporaryCurrentStock = new List<TemporaryCurrentStock>();// temp stock
        //    List<TemporaryCurrentStockHistory> AddTemporaryCurrentStockHistory = new List<TemporaryCurrentStockHistory>();// temp Histpry
        //    foreach (var addGrItem in AddPOGRDetailobj.PurchaseOrderDetailDc)
        //    {
        //        ItemMasterCentral ItemMasterCentral = ItemMasterCentralList.FirstOrDefault(x => x.Number == addGrItem.ItemNumber);
        //        var item = TemporaryCurrentStockList.Where(x => x.ItemNumber == addGrItem.ItemNumber && x.WarehouseId == warehouseid && x.ItemMultiMRPId == addGrItem.ItemMultiMRPId).SingleOrDefault();
        //        if (item == null)
        //        {
        //            var Mitem = ItemMultiMRPList.Where(x => x.ItemNumber == addGrItem.ItemNumber && x.ItemMultiMRPId == addGrItem.ItemMultiMRPId).SingleOrDefault();
        //            if (Mitem != null)
        //            {
        //                TemporaryCurrentStock newCstk = new TemporaryCurrentStock();
        //                newCstk.CompanyId = 1;
        //                newCstk.itemBaseName = Mitem.itemBaseName;
        //                newCstk.ItemNumber = addGrItem.ItemNumber;
        //                newCstk.WarehouseId = PurchaseOrderMaster.WarehouseId;
        //                newCstk.WarehouseName = PurchaseOrderMaster.WarehouseName;
        //                newCstk.CurrentInventory = 0;
        //                newCstk.DamageCurrentInventory = 0;
        //                newCstk.ExpCurrentInventory = 0;
        //                newCstk.CreationDate = indianTime;
        //                newCstk.UpdatedDate = indianTime;
        //                newCstk.MRP = Mitem.MRP;
        //                newCstk.UnitofQuantity = Mitem.UnitofQuantity;
        //                newCstk.UOM = Mitem.UOM;
        //                newCstk.ItemMultiMRPId = Mitem.ItemMultiMRPId;
        //                newCstk.itemname = addGrItem.ItemName;   //itm.itemname;
        //                context.TemporaryCurrentStockDB.Add(newCstk);
        //                context.Commit();

        //                item = newCstk;//if temp stock not there then create stock
        //            }
        //        }
        //        if (item != null)
        //        {
        //            PurchaseOrderDetailRecived pd = new PurchaseOrderDetailRecived();
        //            if (PurchaseOrderDetailRecived.Count > 0)
        //            {
        //                pd = PurchaseOrderDetailRecived.Where(x => x.PurchaseOrderDetailId == addGrItem.PurchaseOrderDetailId).SingleOrDefault();
        //            }


        //            var QtyReciv = addGrItem.ReceivingQty;
        //            var DamQtyReciv = 0;// addGrItem.ReceivingDamagedQty.GetValueOrDefault();
        //            var ExpQtyReciv = 0;
        //            TemporaryCurrentStockHistory Oss = new TemporaryCurrentStockHistory();
        //            Oss.StockId = item.Id;
        //            Oss.ItemNumber = item.ItemNumber;
        //            Oss.itemname = item.itemname;
        //            Oss.OdOrPoId = addGrItem.PurchaseOrderId;
        //            Oss.CurrentInventory = item.CurrentInventory;
        //            Oss.CurrentInventoryIn = Convert.ToInt32(QtyReciv);
        //            Oss.TotalCurrentInventory = item.CurrentInventory + Convert.ToInt32(QtyReciv);
        //            Oss.TotalDamageCurrentInventory = item.DamageCurrentInventory + Convert.ToInt32(DamQtyReciv);
        //            Oss.TotalExpCurrentInventory = item.ExpCurrentInventory + Convert.ToInt32(ExpQtyReciv);
        //            Oss.WarehouseName = item.WarehouseName;
        //            Oss.Warehouseid = item.WarehouseId;
        //            Oss.CompanyId = item.CompanyId;
        //            Oss.CreationDate = indianTime;
        //            Oss.ItemMultiMRPId = addGrItem.ItemMultiMRPId;
        //            AddTemporaryCurrentStockHistory.Add(Oss);

        //            item.CurrentInventory = item.CurrentInventory + Convert.ToInt32(QtyReciv);
        //            item.DamageCurrentInventory = item.DamageCurrentInventory + Convert.ToInt32(DamQtyReciv);
        //            item.UpdatedDate = indianTime;
        //            UpdateTemporaryCurrentStock.Add(item);



        //            double? amt = 0;
        //            if (count == 1)
        //            {
        //                pd.CompanyId = 1;
        //                pd.MOQ = addGrItem.MOQ;
        //                //pd.Price = addGrItem.Price;
        //                pd.MRP = addGrItem.MRP;
        //                pd.ItemMultiMRPId = addGrItem.ItemMultiMRPId;
        //                pd.HSNCode = ItemMasterCentral.HSNCode;
        //                pd.PurchaseSku = addGrItem.PurchaseSku;
        //                pd.WarehouseId = PurchaseOrderMaster.WarehouseId;
        //                pd.WarehouseName = PurchaseOrderMaster.WarehouseName;
        //                pd.PurchaseOrderDetailId = addGrItem.PurchaseOrderDetailId;
        //                pd.PurchaseOrderId = addGrItem.PurchaseOrderId;
        //                pd.TotalTaxPercentage = ItemMasterCentral.TotalTaxPercentage;
        //                pd.TotalQuantity = addGrItem.TotalQuantity;
        //                pd.Price = addGrItem.Price;
        //                pd.itemBaseName = item.itemBaseName;
        //                pd.ItemNumber = item.ItemNumber;
        //                pd.ItemName = addGrItem.ItemName;
        //                pd.QtyRecived1 = addGrItem.ReceivingQty;
        //                //pd.DamagQtyRecived1 = addGrItem.ReceivingDamagedQty;
        //                pd.QtyRecived2 = 0;
        //                pd.QtyRecived3 = 0;
        //                pd.QtyRecived4 = 0;
        //                pd.QtyRecived5 = 0;
        //                amt = pd.QtyRecived1 * addGrItem.Price;
        //                pd.CreationDate = indianTime;
        //                pd.ItemName1 = addGrItem.ItemName;
        //                pd.Price1 = addGrItem.Price;
        //                pd.ItemMultiMRPId1 = addGrItem.ItemMultiMRPId;
        //                pd.GRDate1 = indianTime;
        //                pd.MFGDate1 = addGrItem.MFGDate;
        //                pd.BatchNo1 = addGrItem.BatchNo;
        //                pd.PriceRecived = amt.GetValueOrDefault();
        //                AddPurchaseOrderDetailRecived.Add(pd);
        //            }
        //            else if (count == 2)
        //            {
        //                pd.QtyRecived2 = addGrItem.ReceivingQty;
        //                //pd.DamagQtyRecived2 = addGrItem.ReceivingDamagedQty;
        //                pd.QtyRecived3 = 0;
        //                pd.QtyRecived4 = 0;
        //                pd.QtyRecived5 = 0;
        //                amt = pd.QtyRecived2 * addGrItem.Price;
        //                pd.Price2 = addGrItem.Price;
        //                pd.ItemName2 = addGrItem.ItemName;
        //                pd.ItemMultiMRPId2 = addGrItem.ItemMultiMRPId;
        //                pd.GRDate2 = indianTime;
        //                pd.PriceRecived = amt.GetValueOrDefault();
        //                pd.MFGDate2 = addGrItem.MFGDate;
        //                pd.BatchNo2 = addGrItem.BatchNo;
        //                UpdatePurchaseOrderDetailRecived.Add(pd);

        //            }
        //            else if (count == 3)
        //            {
        //                pd.QtyRecived3 = addGrItem.ReceivingQty;
        //                //pd.DamagQtyRecived3 = addGrItem.ReceivingDamagedQty;
        //                pd.QtyRecived4 = 0;
        //                pd.QtyRecived5 = 0;
        //                amt = pd.QtyRecived3 * addGrItem.Price;
        //                pd.Price3 = addGrItem.Price;
        //                pd.ItemName3 = addGrItem.ItemName;
        //                pd.ItemMultiMRPId3 = addGrItem.ItemMultiMRPId;
        //                pd.GRDate3 = indianTime;
        //                pd.PriceRecived = amt.GetValueOrDefault();
        //                pd.MFGDate3 = addGrItem.MFGDate;
        //                pd.BatchNo3 = addGrItem.BatchNo;
        //                UpdatePurchaseOrderDetailRecived.Add(pd);

        //            }
        //            else if (count == 4)
        //            {
        //                pd.QtyRecived4 = addGrItem.ReceivingQty;
        //                //pd.DamagQtyRecived4 = addGrItem.ReceivingDamagedQty;
        //                pd.QtyRecived5 = 0;
        //                amt = pd.QtyRecived4 * addGrItem.Price;
        //                pd.Price4 = addGrItem.Price;
        //                pd.ItemName4 = addGrItem.ItemName;
        //                pd.ItemMultiMRPId4 = addGrItem.ItemMultiMRPId;
        //                pd.GRDate4 = indianTime;
        //                pd.PriceRecived = amt.GetValueOrDefault();
        //                pd.MFGDate4 = addGrItem.MFGDate;
        //                pd.BatchNo4 = addGrItem.BatchNo;
        //                UpdatePurchaseOrderDetailRecived.Add(pd);
        //            }
        //            else if (count == 5)
        //            {
        //                pd.QtyRecived5 = addGrItem.ReceivingQty;
        //                //pd.DamagQtyRecived5 = addGrItem.ReceivingDamagedQty;
        //                amt = pd.QtyRecived5 * addGrItem.Price;
        //                pd.Price5 = addGrItem.Price;
        //                pd.ItemName5 = addGrItem.ItemName;
        //                pd.ItemMultiMRPId5 = addGrItem.ItemMultiMRPId;
        //                pd.GRDate5 = indianTime;
        //                pd.PriceRecived = amt.GetValueOrDefault();
        //                pd.MFGDate5 = addGrItem.MFGDate;
        //                pd.BatchNo5 = addGrItem.BatchNo;
        //                UpdatePurchaseOrderDetailRecived.Add(pd);
        //            }

        //            GrAmount += amt ?? 0;

        //        }
        //    }
        //    if (count == 1)
        //    {
        //        PurchaseOrderMaster.Gr1Number = PurchaseOrderMaster.PurchaseOrderId + "A";
        //        PurchaseOrderMaster.Gr1_Date = indianTime;
        //        PurchaseOrderMaster.Gr1PersonId = people.PeopleID;
        //        PurchaseOrderMaster.Gr1PersonName = people.DisplayName;
        //        PurchaseOrderMaster.VehicleNumber1 = VehicleNumber;
        //        PurchaseOrderMaster.VehicleType1 = VehicleType;
        //        PurchaseOrderMaster.Gr1Status = "Pending for Checker Side";
        //        PurchaseOrderMaster.GRcount = count;
        //        PurchaseOrderMaster.Gr1_Amount = GrAmount;
        //    }
        //    else if (count == 2)
        //    {


        //        PurchaseOrderMaster.Gr2Number = PurchaseOrderMaster.PurchaseOrderId + "B";
        //        PurchaseOrderMaster.Gr2_Date = indianTime;
        //        PurchaseOrderMaster.Gr2PersonId = people.PeopleID;
        //        PurchaseOrderMaster.Gr2PersonName = people.DisplayName;
        //        PurchaseOrderMaster.VehicleNumber2 = VehicleNumber;
        //        PurchaseOrderMaster.VehicleType2 = VehicleType;
        //        PurchaseOrderMaster.Gr2Status = "Pending for Checker Side";
        //        PurchaseOrderMaster.GRcount = count;
        //        PurchaseOrderMaster.Gr2_Amount = GrAmount;
        //    }
        //    else if (count == 3)
        //    {
        //        PurchaseOrderMaster.Gr3Number = PurchaseOrderMaster.PurchaseOrderId + "C";
        //        PurchaseOrderMaster.Gr3_Date = indianTime;
        //        PurchaseOrderMaster.Gr3PersonId = people.PeopleID;
        //        PurchaseOrderMaster.Gr3PersonName = people.DisplayName;
        //        PurchaseOrderMaster.VehicleNumber3 = VehicleNumber;
        //        PurchaseOrderMaster.VehicleType3 = VehicleType;
        //        PurchaseOrderMaster.Gr3Status = "Pending for Checker Side";
        //        PurchaseOrderMaster.GRcount = count;
        //        PurchaseOrderMaster.Gr3_Amount = GrAmount;
        //    }
        //    else if (count == 4)
        //    {


        //        PurchaseOrderMaster.Gr4Number = PurchaseOrderMaster.PurchaseOrderId + "D";
        //        PurchaseOrderMaster.Gr4_Date = indianTime;
        //        PurchaseOrderMaster.Gr4PersonId = people.PeopleID;
        //        PurchaseOrderMaster.Gr4PersonName = people.DisplayName;
        //        PurchaseOrderMaster.VehicleNumber4 = VehicleNumber;
        //        PurchaseOrderMaster.VehicleType4 = VehicleType;
        //        PurchaseOrderMaster.Gr4Status = "Pending for Checker Side";
        //        PurchaseOrderMaster.GRcount = count;
        //        PurchaseOrderMaster.Gr4_Amount = GrAmount;
        //    }
        //    else if (count == 5)
        //    {


        //        PurchaseOrderMaster.Gr5Number = PurchaseOrderMaster.PurchaseOrderId + "E";
        //        PurchaseOrderMaster.Gr5_Date = indianTime;
        //        PurchaseOrderMaster.Gr5PersonId = people.PeopleID;
        //        PurchaseOrderMaster.Gr5PersonName = people.DisplayName;
        //        PurchaseOrderMaster.VehicleNumber5 = VehicleNumber;
        //        PurchaseOrderMaster.VehicleType5 = VehicleType;
        //        PurchaseOrderMaster.Gr5Status = "Pending for Checker Side";
        //        PurchaseOrderMaster.GRcount = count;
        //        PurchaseOrderMaster.Gr5_Amount = GrAmount;
        //    }
        //    int TotalPOQty = AddPOGRDetailobj.PurchaseOrderDetailDc.Sum(x => x.TotalQuantity);
        //    int? TotalPORecQty = 0;
        //    if (count == 1)
        //    {
        //        TotalPORecQty = AddPurchaseOrderDetailRecived.Sum(x => x.QtyRecived1);
        //        //TotalPORecQty += AddPurchaseOrderDetailRecived.Sum(x => x.DamagQtyRecived1);
        //    }
        //    else
        //    {
        //        TotalPORecQty = UpdatePurchaseOrderDetailRecived.Sum(x => x.QtyRecived1 + (x.QtyRecived2.HasValue ? x.QtyRecived2.Value : 0) + (x.QtyRecived3.HasValue ? x.QtyRecived3.Value : 0) + (x.QtyRecived4.HasValue ? x.QtyRecived4.Value : 0) + (x.QtyRecived5.HasValue ? x.QtyRecived5.Value : 0));
        //        //TotalPORecQty += UpdatePurchaseOrderDetailRecived.Sum(x => x.DamagQtyRecived1 + (x.DamagQtyRecived2.HasValue ? x.DamagQtyRecived2.Value : 0) + (x.DamagQtyRecived3.HasValue ? x.DamagQtyRecived3.Value : 0) + (x.DamagQtyRecived4.HasValue ? x.DamagQtyRecived4.Value : 0) + (x.DamagQtyRecived5.HasValue ? x.DamagQtyRecived5.Value : 0));
        //    }
        //    int percentComplete = (int)Math.Round((double)(100 * TotalPORecQty) / TotalPOQty);

        //    PurchaseOrderMaster.progress = Convert.ToString(percentComplete);

        //    if (TotalPOQty > TotalPORecQty)
        //    {
        //        PurchaseOrderMaster.Status = "UN Partial Received";
        //    }
        //    else
        //    {
        //        if (TotalPOQty >= TotalPORecQty)
        //        {
        //            PurchaseOrderMaster.Status = "UN Received";
        //        }
        //    }


        //    if (AddPOGRDetailobj.PurchaseOrderFreeItemMasterDC != null && AddPOGRDetailobj.PurchaseOrderFreeItemMasterDC.Any())
        //    {
        //        int qty = AddPOGRDetailobj.PurchaseOrderFreeItemMasterDC.Sum(x => x.TotalQuantity);
        //        if (qty > 0)
        //        {
        //            bool AddedFreeitem = AddPurchaseOrderFreeItemRecived(PurchaseOrderMaster, AddPOGRDetailobj.PurchaseOrderFreeItemMasterDC, FreeItemList, count, people, context);
        //        }

        //    }

        //    foreach (var item in UpdateTemporaryCurrentStock)
        //    {
        //        context.Entry(item).State = EntityState.Modified;
        //    }

        //    if (UpdatePurchaseOrderDetailRecived != null && UpdatePurchaseOrderDetailRecived.Count > 0)
        //    {
        //        foreach (var update in UpdatePurchaseOrderDetailRecived)
        //        {
        //            context.Entry(update).State = EntityState.Modified;
        //        }
        //    }

        //    if (AddPurchaseOrderDetailRecived != null && AddPurchaseOrderDetailRecived.Count > 0)
        //    {
        //        context.PurchaseOrderRecivedDetails.AddRange(AddPurchaseOrderDetailRecived);
        //    }

        //    context.TemporaryCurrentStockHistoryDB.AddRange(AddTemporaryCurrentStockHistory);
        //    context.Entry(PurchaseOrderMaster).State = EntityState.Modified;

        //    if (context.Commit() > 0) { return true; } else { return false; }

        //}
        //#endregion


        #region Get List of Pending Approval
        /// <summary>
        /// GetUnconfirmGrData
        /// </summary>
        /// <returns></returns>
        [AllowAnonymous]
        [Route("UnApprovedGR")]
        [HttpGet]
        public HttpResponseMessage UnApprovedGRDataList(int WarehouseId)
        {


            List<PurchaseOrderMaster> pm = new List<PurchaseOrderMaster>();
            string resultMessage = "";
            bool status = false;
            var predicate = PredicateBuilder.True<PurchaseOrderMaster>();
            predicate = predicate.And(y => y.Status == "UN Received" || y.Status == "UN Partial Received");
            predicate = predicate.And(x => x.Gr1Status == "Pending for Checker Side" || x.Gr2Status == "Pending for Checker Side" || x.Gr3Status == "Pending for Checker Side" || x.Gr4Status == "Pending for Checker Side" || x.Gr5Status == "Pending for Checker Side");
            predicate = predicate.And(e => e.WarehouseId == WarehouseId);
            List<AppovePurchaseOrderMasterDc> HeadData = new List<AppovePurchaseOrderMasterDc>();
            using (AuthContext db = new AuthContext())
            {
                pm = db.DPurchaseOrderMaster.Where(predicate).ToList();
                var PurchaseOrderIds = pm.Select(x => x.PurchaseOrderId).ToList();
                List<PurchaseOrderDetailRecived> PurchaseOrderDetailRecived = db.PurchaseOrderRecivedDetails.Where(q => PurchaseOrderIds.Contains(q.PurchaseOrderId)).ToList();
                List<FreeItem> FItemList = db.FreeItemDb.Where(q => PurchaseOrderIds.Contains(q.PurchaseOrderId)).ToList();

                foreach (PurchaseOrderMaster a in pm)
                {
                    List<PurchaseOrderDetailRecived> _detail = PurchaseOrderDetailRecived.Where(q => q.PurchaseOrderId == a.PurchaseOrderId).ToList();
                    if (a.Gr1Number != null && a.Gr1Status == "Pending for Checker Side")
                    {
                        List<AppovePurchaseOrderDetailDc> HeadDetail = new List<AppovePurchaseOrderDetailDc>();
                        foreach (PurchaseOrderDetailRecived b in _detail)
                        {
                            AppovePurchaseOrderDetailDc UCL = new AppovePurchaseOrderDetailDc()
                            {
                                PurchaseOrderDetailId = b.PurchaseOrderDetailId,
                                ItemName = b.ItemName1,
                                MOQ = b.MOQ,
                                Price = Convert.ToDouble(b.Price1),
                                TotalQuantity = b.TotalQuantity,
                                ReceivingQty = Convert.ToInt32(b.QtyRecived1),

                            };
                            HeadDetail.Add(UCL);
                        }
                        List<PoFreeItemMasterDC> FreeDetail = new List<PoFreeItemMasterDC>();
                        if (FItemList.Count > 0)
                        {
                            List<FreeItem> FItem = FItemList.Where(z => z.GRNumber == a.Gr1Number && z.PurchaseOrderId == a.PurchaseOrderId).ToList();
                            foreach (FreeItem c in FItem)
                            {
                                PoFreeItemMasterDC FCL = new PoFreeItemMasterDC()
                                {
                                    Itemname = c.itemname,
                                    TotalQuantity = c.TotalQuantity
                                };
                                FreeDetail.Add(FCL);
                            }
                        }
                        AppovePurchaseOrderMasterDc UC = new AppovePurchaseOrderMasterDc()
                        {
                            PurchaseOrderId = a.PurchaseOrderId,
                            GrNumber = a.Gr1Number,
                            GrPersonName = a.Gr1PersonName,
                            Gr_Date = Convert.ToDateTime(a.Gr1_Date),
                            VehicleType = a.VehicleType1,
                            VehicleNumber = a.VehicleNumber1,
                            PoType = a.PoType,
                            WarehouseName = a.WarehouseName,
                            Level = a.Level,
                            progress = a.progress,
                            CreationDate = a.CreationDate,
                            ApprovedBy = a.ApprovedBy,
                            CreatedBy = a.CreatedBy,
                            DepoName = a.DepoName,
                            ETotalAmount = a.ETotalAmount,
                            SupplierName = a.SupplierName,
                            Status = a.Status,
                            PoItemDetail = HeadDetail,
                            PoFreeItemDetail = FreeDetail
                        };
                        HeadData.Add(UC);
                    }
                    if (a.Gr2Number != null && a.Gr2Status == "Pending for Checker Side")
                    {
                        List<AppovePurchaseOrderDetailDc> HeadDetail = new List<AppovePurchaseOrderDetailDc>();
                        foreach (PurchaseOrderDetailRecived b in _detail)
                        {
                            AppovePurchaseOrderDetailDc UCL = new AppovePurchaseOrderDetailDc()
                            {
                                PurchaseOrderDetailId = b.PurchaseOrderDetailId,
                                ItemName = b.ItemName2,
                                MOQ = b.MOQ,
                                Price = Convert.ToDouble(b.Price2),
                                TotalQuantity = b.TotalQuantity,
                                ReceivingQty = Convert.ToInt32(b.QtyRecived2),

                            };
                            HeadDetail.Add(UCL);
                        }

                        List<PoFreeItemMasterDC> FreeDetail = new List<PoFreeItemMasterDC>();
                        if (FItemList.Count > 0)
                        {

                            List<FreeItem> FItem = FItemList.Where(z => z.GRNumber == a.Gr2Number && z.PurchaseOrderId == a.PurchaseOrderId).ToList();
                            foreach (FreeItem c in FItem)
                            {
                                PoFreeItemMasterDC FCL = new PoFreeItemMasterDC()
                                {
                                    Itemname = c.itemname,
                                    TotalQuantity = c.TotalQuantity
                                };
                                FreeDetail.Add(FCL);
                            }
                        }

                        AppovePurchaseOrderMasterDc UC = new AppovePurchaseOrderMasterDc()
                        {
                            PurchaseOrderId = a.PurchaseOrderId,
                            GrNumber = a.Gr2Number,
                            GrPersonName = a.Gr1PersonName,
                            Gr_Date = Convert.ToDateTime(a.Gr2_Date),
                            VehicleType = a.VehicleType2,
                            VehicleNumber = a.VehicleNumber2,
                            PoType = a.PoType,
                            WarehouseName = a.WarehouseName,
                            Level = a.Level,
                            progress = a.progress,
                            CreationDate = a.CreationDate,
                            ApprovedBy = a.ApprovedBy,
                            CreatedBy = a.CreatedBy,
                            DepoName = a.DepoName,
                            ETotalAmount = a.ETotalAmount,
                            SupplierName = a.SupplierName,
                            Status = a.Status,
                            PoItemDetail = HeadDetail,
                            PoFreeItemDetail = FreeDetail

                        };
                        HeadData.Add(UC);
                    }
                    if (a.Gr3Number != null && a.Gr3Status == "Pending for Checker Side")
                    {
                        List<AppovePurchaseOrderDetailDc> HeadDetail = new List<AppovePurchaseOrderDetailDc>();
                        foreach (PurchaseOrderDetailRecived b in _detail)
                        {
                            AppovePurchaseOrderDetailDc UCL = new AppovePurchaseOrderDetailDc()
                            {

                                PurchaseOrderDetailId = b.PurchaseOrderDetailId,
                                ItemName = b.ItemName3,
                                MOQ = b.MOQ,
                                Price = Convert.ToDouble(b.Price3),
                                TotalQuantity = b.TotalQuantity,
                                ReceivingQty = Convert.ToInt32(b.QtyRecived3),

                            };
                            HeadDetail.Add(UCL);
                        }
                        List<PoFreeItemMasterDC> FreeDetail = new List<PoFreeItemMasterDC>();
                        if (FItemList.Count > 0)
                        {
                            List<FreeItem> FItem = FItemList.Where(z => z.GRNumber == a.Gr3Number && z.PurchaseOrderId == a.PurchaseOrderId).ToList();

                            foreach (FreeItem c in FItem)
                            {
                                PoFreeItemMasterDC FCL = new PoFreeItemMasterDC()
                                {
                                    Itemname = c.itemname,
                                    TotalQuantity = c.TotalQuantity
                                };
                                FreeDetail.Add(FCL);
                            }
                        }


                        AppovePurchaseOrderMasterDc UC = new AppovePurchaseOrderMasterDc()
                        {
                            PurchaseOrderId = a.PurchaseOrderId,
                            GrNumber = a.Gr3Number,
                            GrPersonName = a.Gr3PersonName,
                            Gr_Date = Convert.ToDateTime(a.Gr3_Date),
                            VehicleType = a.VehicleType3,
                            VehicleNumber = a.VehicleNumber3,
                            PoType = a.PoType,
                            WarehouseName = a.WarehouseName,
                            Level = a.Level,
                            progress = a.progress,
                            CreationDate = a.CreationDate,
                            ApprovedBy = a.ApprovedBy,
                            CreatedBy = a.CreatedBy,
                            DepoName = a.DepoName,
                            ETotalAmount = a.ETotalAmount,
                            SupplierName = a.SupplierName,
                            Status = a.Status,
                            PoItemDetail = HeadDetail,
                            PoFreeItemDetail = FreeDetail

                        };
                        HeadData.Add(UC);
                    }
                    if (a.Gr4Number != null && a.Gr4Status == "Pending for Checker Side")
                    {
                        List<AppovePurchaseOrderDetailDc> HeadDetail = new List<AppovePurchaseOrderDetailDc>();
                        foreach (PurchaseOrderDetailRecived b in _detail)
                        {
                            AppovePurchaseOrderDetailDc UCL = new AppovePurchaseOrderDetailDc()
                            {

                                PurchaseOrderDetailId = b.PurchaseOrderDetailId,
                                ItemName = b.ItemName4,
                                MOQ = b.MOQ,
                                Price = Convert.ToDouble(b.Price4),
                                TotalQuantity = b.TotalQuantity,
                                ReceivingQty = Convert.ToInt32(b.QtyRecived4),

                            };
                            HeadDetail.Add(UCL);
                        }
                        List<PoFreeItemMasterDC> FreeDetail = new List<PoFreeItemMasterDC>();
                        if (FItemList.Count > 0)
                        {
                            List<FreeItem> FItem = FItemList.Where(z => z.GRNumber == a.Gr4Number && z.PurchaseOrderId == a.PurchaseOrderId).ToList();
                            foreach (FreeItem c in FItem)
                            {
                                PoFreeItemMasterDC FCL = new PoFreeItemMasterDC()
                                {
                                    Itemname = c.itemname,
                                    TotalQuantity = c.TotalQuantity
                                };
                                FreeDetail.Add(FCL);
                            }
                        }
                        AppovePurchaseOrderMasterDc UC = new AppovePurchaseOrderMasterDc()
                        {
                            PurchaseOrderId = a.PurchaseOrderId,
                            GrNumber = a.Gr4Number,
                            GrPersonName = a.Gr4PersonName,
                            Gr_Date = Convert.ToDateTime(a.Gr4_Date),
                            VehicleType = a.VehicleType4,
                            VehicleNumber = a.VehicleNumber4,
                            PoType = a.PoType,
                            WarehouseName = a.WarehouseName,
                            Level = a.Level,
                            progress = a.progress,
                            CreationDate = a.CreationDate,
                            ApprovedBy = a.ApprovedBy,
                            CreatedBy = a.CreatedBy,
                            DepoName = a.DepoName,
                            ETotalAmount = a.ETotalAmount,
                            SupplierName = a.SupplierName,
                            Status = a.Status,
                            PoItemDetail = HeadDetail,
                            PoFreeItemDetail = FreeDetail

                        };
                        HeadData.Add(UC);
                    }
                    if (a.Gr5Number != null && a.Gr5Status == "Pending for Checker Side")
                    {
                        List<AppovePurchaseOrderDetailDc> HeadDetail = new List<AppovePurchaseOrderDetailDc>();
                        foreach (PurchaseOrderDetailRecived b in _detail)
                        {
                            AppovePurchaseOrderDetailDc UCL = new AppovePurchaseOrderDetailDc()
                            {

                                PurchaseOrderDetailId = b.PurchaseOrderDetailId,
                                ItemName = b.ItemName5,
                                MOQ = b.MOQ,
                                Price = Convert.ToDouble(b.Price5),
                                TotalQuantity = b.TotalQuantity,
                                ReceivingQty = Convert.ToInt32(b.QtyRecived5),

                            };
                            HeadDetail.Add(UCL);
                        }
                        List<PoFreeItemMasterDC> FreeDetail = new List<PoFreeItemMasterDC>();
                        if (FItemList.Count > 0)
                        {
                            List<FreeItem> FItem = FItemList.Where(z => z.GRNumber == a.Gr5Number && z.PurchaseOrderId == a.PurchaseOrderId).ToList();

                            foreach (FreeItem c in FItem)
                            {
                                PoFreeItemMasterDC FCL = new PoFreeItemMasterDC()
                                {
                                    Itemname = c.itemname,
                                    TotalQuantity = c.TotalQuantity
                                };
                                FreeDetail.Add(FCL);
                            }
                        }
                        AppovePurchaseOrderMasterDc UC = new AppovePurchaseOrderMasterDc()
                        {
                            PurchaseOrderId = a.PurchaseOrderId,
                            WarehouseName = a.WarehouseName,
                            GrNumber = a.Gr5Number,
                            GrPersonName = a.Gr5PersonName,
                            Gr_Date = Convert.ToDateTime(a.Gr5_Date),
                            VehicleType = a.VehicleType5,
                            VehicleNumber = a.VehicleNumber5,
                            CreatedBy = a.CreatedBy,
                            PoType = a.PoType,
                            Level = a.Level,
                            progress = a.progress,
                            CreationDate = a.CreationDate,
                            ApprovedBy = a.ApprovedBy,
                            DepoName = a.DepoName,
                            ETotalAmount = a.ETotalAmount,
                            SupplierName = a.SupplierName,
                            Status = a.Status,
                            PoItemDetail = HeadDetail,
                            PoFreeItemDetail = FreeDetail

                        };
                        HeadData.Add(UC);
                    }
                }
            }
            if (HeadData.Count > 0) { HeadData.OrderByDescending(w => w.PurchaseOrderId); resultMessage = "Record found"; status = true; } else { resultMessage = "No Record found"; status = false; }
            var res = new
            {
                GRDetailHistory = HeadData,
                status = status,
                Message = resultMessage
            };
            return Request.CreateResponse(HttpStatusCode.OK, res);
        }
        #endregion

        //#region Approved Gr in PO to Currentstock
        ///// <summary>
        ///// Created By Raj
        ///// Created Date:11/11/2019
        ///// Approved GR  by GR Approver
        ///// </summary>
        ///// <param name="obj"></param>
        ///// <returns></returns>
        //[Route("ApprovedPoGr")]
        //[HttpPost]
        //[AllowAnonymous]
        //public HttpResponseMessage ApprovedPoGr(POApproverDc POApproverDc)
        //{
        //    //// Access claims
        //    //var identity = User.Identity as ClaimsIdentity;
        //    //int compid = 0, userid = 0;
        //    //if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))
        //    //    compid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);
        //    //if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
        //    //    userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
        //    //int CompanyId = compid;
        //    string resultMessage = "";
        //    bool IsValidate = false;
        //    bool status = false;
        //    using (AuthContext context = new AuthContext())
        //    using (var dbContextTransaction = context.Database.BeginTransaction())
        //    {
        //        int UserId = POApproverDc.UserId;
        //        var people = context.Peoples.Where(x => x.PeopleID == UserId && x.Deleted == false && x.Active).SingleOrDefault();

        //        PurchaseOrderMaster PurchaseOrderMaster = context.DPurchaseOrderMaster.Where(x => x.PurchaseOrderId == POApproverDc.PurchaseOrderId && (x.Status != "UN Received" || x.Status != "Received" || x.Status == "Partial Received" || x.Status == "UN Partial Received")).SingleOrDefault();
        //        if (PurchaseOrderMaster != null)
        //        {
        //            string sAllCharacter = POApproverDc.GrNumber;
        //            var GRType = sAllCharacter[sAllCharacter.Length - 1];
        //            var GRCount = 0;
        //            List<PurchaseOrderDetailRecived> PurchaseOrderRecived = context.PurchaseOrderRecivedDetails.Where(a => a.PurchaseOrderId == POApproverDc.PurchaseOrderId).ToList();
        //            if (GRType == 'A')
        //            {
        //                if (PurchaseOrderMaster.Gr1Status != "Approved" && PurchaseOrderMaster.Gr1Status != "RejectGR")
        //                {
        //                    PurchaseOrderMaster.Gr1Status = "Approved";
        //                    PurchaseOrderMaster.Comment = POApproverDc.POApproveStatus;
        //                    IsValidate = true;
        //                    GRCount = 1;
        //                }
        //            }
        //            else if (GRType == 'B')
        //            {
        //                if (PurchaseOrderMaster.Gr2Status != "Approved" && PurchaseOrderMaster.Gr2Status != "RejectGR")
        //                {
        //                    PurchaseOrderMaster.Gr2Status = "Approved";
        //                    PurchaseOrderMaster.Comment = PurchaseOrderMaster.Comment + Environment.NewLine + POApproverDc.POApproveStatus;
        //                    IsValidate = true;
        //                    GRCount = 2;
        //                }

        //            }
        //            else if (GRType == 'C')
        //            {
        //                if (PurchaseOrderMaster.Gr3Status != "Approved" && PurchaseOrderMaster.Gr3Status != "RejectGR")
        //                {
        //                    PurchaseOrderMaster.Gr3Status = "Approved";
        //                    PurchaseOrderMaster.Comment = PurchaseOrderMaster.Comment + Environment.NewLine + POApproverDc.POApproveStatus;
        //                    IsValidate = true;
        //                    GRCount = 3;
        //                }

        //            }
        //            if (GRType == 'D')
        //            {
        //                if (PurchaseOrderMaster.Gr4Status != "Approved" && PurchaseOrderMaster.Gr4Status != "RejectGR")
        //                {
        //                    PurchaseOrderMaster.Gr4Status = "Approved";
        //                    PurchaseOrderMaster.Comment = PurchaseOrderMaster.Comment + Environment.NewLine + POApproverDc.POApproveStatus;
        //                    IsValidate = true;
        //                    GRCount = 4;
        //                }

        //            }
        //            if (GRType == 'E')
        //            {
        //                if (PurchaseOrderMaster.Gr5Status != "Approved" && PurchaseOrderMaster.Gr5Status != "RejectGR")
        //                {
        //                    PurchaseOrderMaster.Gr5Status = "Approved";
        //                    PurchaseOrderMaster.Comment = PurchaseOrderMaster.Comment + Environment.NewLine + POApproverDc.POApproveStatus;
        //                    IsValidate = true;
        //                    GRCount = 5;
        //                }

        //            }
        //            if (IsValidate && GRCount > 0)
        //            {

        //                bool result = GRNPurchaseOrderDetailsRecivedIn(PurchaseOrderMaster, PurchaseOrderRecived, POApproverDc.GrNumber, GRCount, people, context);
        //                if (result)
        //                {
        //                    dbContextTransaction.Commit();
        //                    resultMessage = "GR Appoved Succesfully, Will reflect into Stock";
        //                    status = true;

        //                    //#region  Supplier Notification
        //                    //NotificationHelper GRNotificationHelper = new NotificationHelper();
        //                    //string Notificationmessage = "GR done against your invoice number :"+ POApproverDc.GrNumber + " on date: " + indianTime;
        //                    //string smsmessage = "GR done against your invoice number :" + POApproverDc.GrNumber + " on date : " + indianTime;
        //                    //string title = "Goods Received !";
        //                    //bool sendSms = true;
        //                    //bool sendFcmNotification = true;
        //                    //bool SendNotification = GRNotificationHelper.SendNotificationtoSupplier(PurchaseOrderMaster.SupplierId, Notificationmessage, smsmessage, sendSms, sendFcmNotification, title);
        //                    //#endregion
        //                }
        //                else
        //                {
        //                    dbContextTransaction.Rollback();
        //                    resultMessage = "Something went wrong";
        //                    status = false;

        //                }
        //            }
        //            else { resultMessage = "Already gr posted or may be under in RejectGR(will be approved through backend portal)"; status = false; }

        //        }
        //        else { resultMessage = "Something went wrong"; status = false; }

        //        var res = new
        //        {
        //            POGRDetail = POApproverDc,
        //            status = status,
        //            Message = resultMessage

        //        };
        //        return Request.CreateResponse(HttpStatusCode.OK, res);

        //    }
        //}

        //public bool GRNPurchaseOrderDetailsRecivedIn(PurchaseOrderMaster PurchaseOrderMaster, List<PurchaseOrderDetailRecived> PurchaseOrderDetailRecived, string GRType, int GRCount, People people, AuthContext context)
        //{

        //    var PoItemMultimrpids = new List<int>();
        //    var FIItemMultimrpids = new List<int>();
        //    var FreeStokList = new List<FreeStock>();
        //    var FreeMRPList = new List<ItemMultiMRP>();

        //    List<TemporaryCurrentStock> UpdateTemporaryCurrentStock = new List<TemporaryCurrentStock>();// UpdateTemporaryCurrentStock stock
        //    List<TemporaryCurrentStockHistory> AddTemporaryCurrentStockHistory = new List<TemporaryCurrentStockHistory>();// AddTemporaryCurrentStockHistory Histpry

        //    List<CurrentStock> UpdateCurrentStock = new List<CurrentStock>();// UpdateCurrentStock

        //    List<CurrentStockHistory> AddCurrentStockHistory = new List<CurrentStockHistory>();// AddCurrentStockHistory History 

        //    List<FreeStock> AddFreeStock = new List<FreeStock>();// UpdateFreeStock
        //    List<FreeStock> UpdateFreeStock = new List<FreeStock>();// UpdateFreeStock
        //    List<FreeStockHistory> AddFreeStockHistory = new List<FreeStockHistory>();// FreeStockHistory
        //    var PoInventoryManager = new PoInventoryManager();

        //    if (GRCount == 1)
        //    {
        //        PoItemMultimrpids = PurchaseOrderDetailRecived.Select(x => x.ItemMultiMRPId1).ToList();
        //    }
        //    else if (GRCount == 2) { PoItemMultimrpids = PurchaseOrderDetailRecived.Select(x => x.ItemMultiMRPId2).ToList(); }
        //    else if (GRCount == 3) { PoItemMultimrpids = PurchaseOrderDetailRecived.Select(x => x.ItemMultiMRPId3).ToList(); }
        //    else if (GRCount == 4) { PoItemMultimrpids = PurchaseOrderDetailRecived.Select(x => x.ItemMultiMRPId4).ToList(); }
        //    else if (GRCount == 5) { PoItemMultimrpids = PurchaseOrderDetailRecived.Select(x => x.ItemMultiMRPId5).ToList(); }


        //    var TemporaryCurrentStockList = context.TemporaryCurrentStockDB.Where(x => PoItemMultimrpids.Contains(x.ItemMultiMRPId) && x.WarehouseId == PurchaseOrderMaster.WarehouseId && x.Deleted == false).ToList(); // ItemMultiMRPList

        //    var CurrentStockList = context.DbCurrentStock.Where(x => PoItemMultimrpids.Contains(x.ItemMultiMRPId) && x.WarehouseId == PurchaseOrderMaster.WarehouseId && x.Deleted == false).ToList(); // ItemMultiMRPList
        //    var ItemMultiMRPList = context.ItemMultiMRPDB.Where(x => PoItemMultimrpids.Contains(x.ItemMultiMRPId) && x.Deleted == false).ToList(); // ItemMultiMRPList

        //    List<FreeItem> FI = context.FreeItemDb.Where(a => a.PurchaseOrderId == PurchaseOrderMaster.PurchaseOrderId && a.GRNumber == GRType).ToList();
        //    if (FI.Count > 0)
        //    {
        //        FIItemMultimrpids = FI.Select(x => x.ItemMultiMRPId).ToList();//
        //        FreeStokList = context.FreeStockDB.Where(x => FIItemMultimrpids.Contains(x.ItemMultiMRPId) && x.WarehouseId == PurchaseOrderMaster.WarehouseId && x.Deleted == false).ToList();
        //        FreeMRPList = context.ItemMultiMRPDB.Where(x => FIItemMultimrpids.Contains(x.ItemMultiMRPId)).ToList();
        //    }
        //    foreach (var k in PurchaseOrderDetailRecived)
        //    {

        //        int ItemMultiMRPId = 0;
        //        int? QtyReciveing = 0;
        //        if (GRCount == 1)
        //        {
        //            ItemMultiMRPId = k.ItemMultiMRPId1;
        //            QtyReciveing = k.QtyRecived1;
        //        }
        //        else if (GRCount == 2)
        //        {
        //            ItemMultiMRPId = k.ItemMultiMRPId2;
        //            QtyReciveing = k.QtyRecived2;
        //        }
        //        else if (GRCount == 3)
        //        {
        //            ItemMultiMRPId = k.ItemMultiMRPId3;
        //            QtyReciveing = k.QtyRecived3;
        //        }
        //        else if (GRCount == 4)
        //        {
        //            ItemMultiMRPId = k.ItemMultiMRPId4;
        //            QtyReciveing = k.QtyRecived4;
        //        }
        //        else if (GRCount == 5)
        //        {
        //            ItemMultiMRPId = k.ItemMultiMRPId5;
        //            QtyReciveing = k.QtyRecived5;
        //        }

        //        var Tcs = TemporaryCurrentStockList.Where(x => x.ItemNumber == k.ItemNumber && x.WarehouseId == k.WarehouseId && x.ItemMultiMRPId == ItemMultiMRPId).SingleOrDefault();
        //        if (k.QtyRecived != 0)
        //        {
        //            TemporaryCurrentStockHistory Tcsh = new TemporaryCurrentStockHistory();
        //            if (Tcs != null)
        //            {
        //                Tcsh.StockId = Tcs.Id;
        //                Tcsh.ItemNumber = Tcs.ItemNumber;
        //                Tcsh.itemname = Tcs.itemname;
        //                Tcsh.OdOrPoId = PurchaseOrderMaster.PurchaseOrderId;

        //                Tcsh.CurrentInventory = Tcs.CurrentInventory;
        //                Tcsh.InventoryOut = Convert.ToInt32(QtyReciveing);
        //                Tcsh.TotalCurrentInventory = Tcs.CurrentInventory - Convert.ToInt32(QtyReciveing);

        //                Tcsh.ExpInventoryOut = k.ExpQtyRecived1;
        //                Tcsh.TotalExpCurrentInventory = Tcs.ExpCurrentInventory;


        //                Tcsh.TotalDamageCurrentInventory = Tcs.DamageCurrentInventory;

        //                Tcsh.WarehouseName = Tcs.WarehouseName;
        //                Tcsh.Warehouseid = Tcs.WarehouseId;
        //                Tcsh.CompanyId = Tcs.CompanyId;

        //                Tcsh.CreationDate = indianTime;
        //                Tcsh.userid = people.PeopleID;
        //                Tcsh.ItemMultiMRPId = Tcs.ItemMultiMRPId;
        //                AddTemporaryCurrentStockHistory.Add(Tcsh);


        //                Tcs.CurrentInventory = Tcs.CurrentInventory - Convert.ToInt32(QtyReciveing);
        //                UpdateTemporaryCurrentStock.Add(Tcs);

        //                /// Add Stock in current stock from Temporary current stock
        //                var item = CurrentStockList.Where(x => x.ItemNumber == k.ItemNumber && x.WarehouseId == k.WarehouseId && x.ItemMultiMRPId == ItemMultiMRPId).SingleOrDefault();
        //                var MRPdetail = ItemMultiMRPList.Where(a => a.ItemMultiMRPId == ItemMultiMRPId).SingleOrDefault();
        //                if (item == null)
        //                {
        //                    CurrentStock NewStock = new CurrentStock();
        //                    NewStock.CompanyId = Tcs.CompanyId;
        //                    NewStock.CreationDate = indianTime;
        //                    NewStock.CurrentInventory = 0;
        //                    NewStock.Deleted = false;
        //                    NewStock.ItemMultiMRPId = ItemMultiMRPId;
        //                    NewStock.itemname = k.ItemName;
        //                    NewStock.ItemNumber = k.ItemNumber;
        //                    NewStock.itemBaseName = Tcs.itemBaseName;

        //                    NewStock.UpdatedDate = indianTime;
        //                    NewStock.WarehouseId = Tcs.WarehouseId;
        //                    NewStock.WarehouseName = Tcs.WarehouseName;
        //                    NewStock.MRP = MRPdetail.MRP;
        //                    NewStock.UOM = MRPdetail.UOM;
        //                    NewStock.userid = people.PeopleID;
        //                    context.DbCurrentStock.Add(NewStock);
        //                    context.Commit();
        //                    item = NewStock;
        //                }
        //                // Recall
        //                CurrentStockHistory Oss = new CurrentStockHistory();
        //                if (item != null)
        //                {
        //                    Oss.StockId = item.StockId;
        //                    Oss.ItemNumber = item.ItemNumber;
        //                    Oss.itemname = item.itemname;
        //                    Oss.OdOrPoId = PurchaseOrderMaster.PurchaseOrderId;
        //                    Oss.CurrentInventory = item.CurrentInventory;
        //                    Oss.InventoryIn = Convert.ToInt32(QtyReciveing);
        //                    Oss.TotalInventory = item.CurrentInventory + Convert.ToInt32(QtyReciveing);
        //                    Oss.WarehouseName = item.WarehouseName;
        //                    Oss.Warehouseid = item.WarehouseId;
        //                    Oss.CompanyId = item.CompanyId;
        //                    Oss.CreationDate = indianTime;
        //                    Oss.ManualReason = "(+)Stock GRN: " + GRType;
        //                    Oss.ItemMultiMRPId = item.ItemMultiMRPId;
        //                    Oss.MRP = item.MRP;
        //                    Oss.UOM = item.UOM;
        //                    Oss.userid = people.PeopleID;
        //                    Oss.UserName = people.DisplayName;
        //                    AddCurrentStockHistory.Add(Oss);


        //                    item.CurrentInventory = item.CurrentInventory + Convert.ToInt32(QtyReciveing);
        //                    UpdateCurrentStock.Add(item);
        //                }

        //            }

        //        }
        //    }

        //    if (FI.Count > 0)
        //    {
        //        foreach (FreeItem f in FI)
        //        {
        //            var stok = FreeStokList.Where(x => x.ItemNumber == f.itemNumber && x.WarehouseId == f.WarehouseId && x.ItemMultiMRPId == f.ItemMultiMRPId).FirstOrDefault();
        //            var MRP = FreeMRPList.Where(a => a.ItemMultiMRPId == f.ItemMultiMRPId).SingleOrDefault();
        //            if (stok != null)
        //            {
        //                FreeStockHistory Oss = new FreeStockHistory();
        //                Oss.ManualReason = "Free Item";
        //                Oss.FreeStockId = stok.FreeStockId;
        //                Oss.ItemMultiMRPId = stok.ItemMultiMRPId;
        //                Oss.ItemNumber = stok.ItemNumber;
        //                Oss.itemname = stok.itemname;
        //                Oss.OdOrPoId = PurchaseOrderMaster.PurchaseOrderId;
        //                Oss.CurrentInventory = f.TotalQuantity;
        //                Oss.InventoryIn = f.TotalQuantity;
        //                Oss.TotalInventory = Convert.ToInt32(stok.CurrentInventory + f.TotalQuantity);
        //                Oss.WarehouseId = stok.WarehouseId;
        //                Oss.CreationDate = DateTime.Now;

        //                AddFreeStockHistory.Add(Oss);

        //                stok.CurrentInventory = stok.CurrentInventory + f.TotalQuantity;
        //                if (stok.CurrentInventory < 0)
        //                {
        //                    stok.CurrentInventory = 0;
        //                }
        //                UpdateFreeStock.Add(stok);
        //            }
        //            else
        //            {
        //                FreeStock FSN = new FreeStock();
        //                FSN.ItemNumber = f.itemNumber;
        //                FSN.itemname = f.itemname;
        //                FSN.ItemMultiMRPId = f.ItemMultiMRPId;
        //                FSN.MRP = MRP.MRP;
        //                FSN.WarehouseId = Convert.ToInt32(f.WarehouseId);
        //                FSN.CurrentInventory = f.TotalQuantity;
        //                FSN.CreatedBy = people.DisplayName;
        //                FSN.CreationDate = indianTime;
        //                FSN.Deleted = false;
        //                FSN.UpdatedDate = indianTime;
        //                AddFreeStock.Add(FSN);

        //                FreeStockHistory Oss = new FreeStockHistory();
        //                Oss.ManualReason = "Free Item";
        //                Oss.FreeStockId = FSN.FreeStockId;
        //                Oss.ItemMultiMRPId = FSN.ItemMultiMRPId;
        //                Oss.ItemNumber = FSN.ItemNumber;
        //                Oss.itemname = FSN.itemname;
        //                Oss.OdOrPoId = f.PurchaseOrderId;
        //                Oss.CurrentInventory = f.TotalQuantity;
        //                Oss.InventoryIn = f.TotalQuantity;
        //                Oss.TotalInventory = Convert.ToInt32(FSN.CurrentInventory);
        //                Oss.WarehouseId = FSN.WarehouseId;
        //                Oss.CreationDate = DateTime.Now;
        //                AddFreeStockHistory.Add(Oss);

        //            }
        //        }
        //    }


        //    #region Update Inventory
        //    PoInventoryManager.updateRecord(context, UpdateCurrentStock, AddCurrentStockHistory, UpdateTemporaryCurrentStock, AddTemporaryCurrentStockHistory, AddFreeStock, UpdateFreeStock, AddFreeStockHistory);

        //    #endregion
        //    int TotalPOQty = PurchaseOrderDetailRecived.Sum(x => x.TotalQuantity);
        //    int? TotalPORecQty = PurchaseOrderDetailRecived.Sum(x => x.QtyRecived1 + (x.QtyRecived2.HasValue ? x.QtyRecived2.Value : 0) + (x.QtyRecived3.HasValue ? x.QtyRecived3.Value : 0) + (x.QtyRecived4.HasValue ? x.QtyRecived4.Value : 0) + (x.QtyRecived5.HasValue ? x.QtyRecived5.Value : 0));
        //    int percentComplete = (int)Math.Round((double)(100 * TotalPORecQty) / TotalPOQty);
        //    PurchaseOrderMaster.progress = Convert.ToString(percentComplete);
        //    if (TotalPOQty > TotalPORecQty)
        //    {
        //        PurchaseOrderMaster.Status = "CN Partial Received";
        //    }
        //    else
        //    {
        //        if (TotalPOQty >= TotalPORecQty)
        //        {
        //            PurchaseOrderMaster.Status = "CN Received";
        //        }
        //    }
        //    context.Entry(PurchaseOrderMaster).State = EntityState.Modified;

        //    if (context.Commit() > 0)
        //    { return true; }
        //    else { return false; }
        //}


        //#region update PoInventoryManager
        //public class PoInventoryManager
        //{
        //    public void updateRecord(AuthContext context, List<CurrentStock> UpdateCurrentStock, List<CurrentStockHistory> AddCurrentStockHistory, List<TemporaryCurrentStock> UpdateTemporaryCurrentStock, List<TemporaryCurrentStockHistory> AddTemporaryCurrentStockHistory, List<FreeStock> AddFreeStock, List<FreeStock> UpdateFreeStock, List<FreeStockHistory> AddFreeStockHistory)
        //    {

        //        foreach (var TemporaryCurrentStock in UpdateTemporaryCurrentStock)
        //        {

        //            context.Entry(TemporaryCurrentStock).State = EntityState.Modified;
        //        }
        //        foreach (var CurrentStock in UpdateCurrentStock)
        //        {
        //            context.Entry(CurrentStock).State = EntityState.Modified;
        //        }
        //        foreach (var item in UpdateFreeStock)
        //        {
        //            context.Entry(item).State = EntityState.Modified;
        //        }
        //        context.TemporaryCurrentStockHistoryDB.AddRange(AddTemporaryCurrentStockHistory);

        //        context.CurrentStockHistoryDb.AddRange(AddCurrentStockHistory);
        //        context.FreeStockDB.AddRange(AddFreeStock);
        //        context.FreeStockHistoryDB.AddRange(AddFreeStockHistory);
        //    }
        //}
        //#endregion
        //#endregion


        #region Addd Free Item in Purchase
        public bool AddPurchaseOrderFreeItemRecived(PurchaseOrderMaster PurchaseOrderMaster, List<PoFreeItemMasterDC> PurchaseOrderFreeItemMasterDC, List<ItemMaster> FreeItemList, int? count, People peole, AuthContext context)
        {
            var GrNumber = "";
            if (count == 1)
            {
                GrNumber = PurchaseOrderMaster.Gr1Number;
            }
            else if (count == 2)
            {
                GrNumber = PurchaseOrderMaster.Gr2Number;
            }
            else if (count == 3)
            {
                GrNumber = PurchaseOrderMaster.Gr3Number;

            }
            else if (count == 4)
            {
                GrNumber = PurchaseOrderMaster.Gr4Number;
            }
            else if (count == 5)
            {
                GrNumber = PurchaseOrderMaster.Gr5Number;
            }
            foreach (var item in PurchaseOrderFreeItemMasterDC)
            {

                var itemmaster = FreeItemList.Where(a => a.ItemId == item.ItemId).SingleOrDefault();
                FreeItem FItem = context.FreeItemDb.Where(a => a.PurchaseOrderId == PurchaseOrderMaster.PurchaseOrderId && a.GRNumber == GrNumber && a.itemNumber == itemmaster.Number && a.ItemMultiMRPId == itemmaster.ItemMultiMRPId).SingleOrDefault();
                if (FItem == null)
                {
                    FreeItem FI = new FreeItem();
                    FI.CompanyId = PurchaseOrderMaster.CompanyId;
                    FI.PurchaseOrderId = PurchaseOrderMaster.PurchaseOrderId;
                    FI.GRNumber = GrNumber;
                    FI.supplierId = itemmaster.SupplierId;
                    FI.SupplierName = itemmaster.SupplierName;
                    FI.ItemId = itemmaster.ItemId;
                    FI.WarehouseId = itemmaster.WarehouseId;
                    FI.itemname = itemmaster.itemname;
                    FI.itemNumber = itemmaster.Number;
                    FI.PurchaseSku = itemmaster.PurchaseSku;
                    FI.TotalQuantity = item.TotalQuantity;
                    FI.Deleted = false;
                    FI.ItemMultiMRPId = itemmaster.ItemMultiMRPId;
                    FI.CreationDate = indianTime;
                    context.FreeItemDb.Add(FI);
                    context.Commit();
                }
                else
                {
                    FItem.TotalQuantity = item.TotalQuantity;
                    context.Entry(FItem).State = EntityState.Modified;
                    context.Commit();
                }

            }
            return true;
        }
        #endregion

        #region get FreeItem List
        [Route("ItemMaster")]
        [HttpGet]
        public async Task<List<PoFreeItemMasterDC>> GetItem(int WarehouseId, string Keyword)
        {
            List<PoFreeItemMasterDC> _result = new List<PoFreeItemMasterDC>();
            using (var context = new AuthContext())
            {
                if (WarehouseId > 0 && Keyword != null && Keyword.Length > 3)
                {
                    List<Object> parameters = new List<object>();
                    parameters.Add(new SqlParameter("@WarehouseId", WarehouseId));
                    parameters.Add(new SqlParameter("@Keyword", Keyword));
                    string sqlquery = "exec GetWarehouseItemList @WarehouseId, @Keyword";
                    _result = await context.Database.SqlQuery<PoFreeItemMasterDC>(sqlquery, parameters.ToArray()).ToListAsync();
                }
            }
            return _result;

        }
        #endregion

        [Route("GrDraftInvoice")]
        [HttpPost]
        [AllowAnonymous]
        public HttpResponseMessage UploadGrDraftInvoice()
        {
            string ImageUrl = string.Empty;
            string filename = string.Empty;
            bool status = false;
            string resultMessage = "";
            if (HttpContext.Current.Request.Files.AllKeys.Any())
            {
                var httpPostedFile = HttpContext.Current.Request.Files["file"];
                if (httpPostedFile != null)
                {
                    ImageUrl = Path.Combine(HttpContext.Current.Server.MapPath("~/images/GrDraftInvoices"), httpPostedFile.FileName);
                    httpPostedFile.SaveAs(ImageUrl);
                    AngularJSAuthentication.Common.Helpers.FileUploadHelper.Upload(httpPostedFile.FileName, "~/images/GrDraftInvoices", ImageUrl);
                    status = true;
                    resultMessage = " file uploaded successfully.";
                    ImageUrl = "/images/GrDraftInvoices/" + httpPostedFile.FileName;
                }
            }
            else
            {
                ImageUrl = "";
                status = false;
                resultMessage = "some thing went wrong";
            }
            var res = new
            {
                ImageUrl = ImageUrl,
                status = status,
                Message = resultMessage
            };
            return Request.CreateResponse(HttpStatusCode.OK, res);

        }

        [Route("PostGrDraftInvoice")]
        [HttpPost]
        public HttpResponseMessage PostGrDraftInvoice(GrDraftInvoiceDc GrDraftInvoice)
        {
            string resultMessage = "";
            bool status = false;
            using (AuthContext context = new AuthContext())
            using (var dbContextTransaction = context.Database.BeginTransaction())
            {
                var purchaseOrder = context.DPurchaseOrderMaster.Where(x => x.PurchaseOrderId == GrDraftInvoice.PurchaseOrderId).SingleOrDefault();
                if (GrDraftInvoice != null && purchaseOrder != null)
                {
                    int UserId = GrDraftInvoice.UserId;
                    var people = context.Peoples.Where(x => x.PeopleID == UserId && x.Deleted == false && x.Active).SingleOrDefault();
                    var GrDraftItem = context.GrDraftInvoiceDB.Where(x => x.PurchaseOrderId == GrDraftInvoice.PurchaseOrderId && x.GrNumber == GrDraftInvoice.GrNumber).SingleOrDefault();
                    if (GrDraftItem != null)
                    {
                        GrDraftItem.ImagePath = GrDraftInvoice.ImagePath;
                        GrDraftItem.ModifiedBy = people.PeopleID;
                        GrDraftItem.ModifiedDate = indianTime;
                    }
                    else
                    {
                        GrDraftInvoice item = new GrDraftInvoice();
                        item.PurchaseOrderId = GrDraftInvoice.PurchaseOrderId;
                        item.GrNumber = GrDraftInvoice.GrNumber;
                        item.CreatedBy = people.PeopleID;
                        item.ModifiedDate = indianTime;
                        item.CreatedDate = indianTime;
                        item.ImagePath = GrDraftInvoice.ImagePath;
                        item.IsActive = true;
                        item.IsDeleted = false;
                        context.GrDraftInvoiceDB.Add(item);
                    }
                    if (context.Commit() > 0)
                    {
                        status = true;
                        resultMessage = "Invoice uploaded successfully";
                        dbContextTransaction.Commit();
                    }
                    else
                    {
                        status = false;
                        resultMessage = "something went wrong";
                        dbContextTransaction.Rollback();
                    }
                }
                else
                {
                    status = false;
                    resultMessage = "Record not found";
                }
            }
            var res = new
            {
                GrDraftInvoice = GrDraftInvoice,
                status = status,
                Message = resultMessage
            };
            return Request.CreateResponse(HttpStatusCode.OK, res);

        }


        #region Get List of Pending Approval
        /// <summary>
        /// GetUnconfirmGrData
        /// </summary>
        /// <returns></returns>
        [Route("GRHistory")]
        [HttpGet]
        public HttpResponseMessage GRHistory(int PurchaseOrderId)
        {

            PurchaseOrderMaster pm = new PurchaseOrderMaster();
            string resultMessage = "";
            bool status = false;
            List<AppovePurchaseOrderMasterDc> HeadData = new List<AppovePurchaseOrderMasterDc>();
            using (AuthContext db = new AuthContext())
            {
                pm = db.DPurchaseOrderMaster.Where(y => y.PurchaseOrderId == PurchaseOrderId && (y.Status != "Self Approved" || y.Status != "Approved")).SingleOrDefault();

                List<PurchaseOrderDetailRecived> PurchaseOrderDetailRecived = db.PurchaseOrderRecivedDetails.Where(q => q.PurchaseOrderId == pm.PurchaseOrderId).ToList();
                List<FreeItem> FItemList = db.FreeItemDb.Where(q => q.PurchaseOrderId == pm.PurchaseOrderId).ToList();

                List<PurchaseOrderDetailRecived> _detail = PurchaseOrderDetailRecived.Where(q => q.PurchaseOrderId == pm.PurchaseOrderId).ToList();
                if (pm.Gr1Number != null)
                {
                    List<AppovePurchaseOrderDetailDc> HeadDetail = new List<AppovePurchaseOrderDetailDc>();
                    foreach (PurchaseOrderDetailRecived b in _detail)
                    {

                        if (b.QtyRecived1 != null)
                        {
                            AppovePurchaseOrderDetailDc UCL = new AppovePurchaseOrderDetailDc()
                            {
                                PurchaseOrderDetailId = b.PurchaseOrderDetailId,
                                ItemName = b.ItemName1,
                                MOQ = b.MOQ,
                                Price = Convert.ToDouble(b.Price1),
                                TotalQuantity = b.TotalQuantity,
                                ReceivingQty = Convert.ToInt32(b.QtyRecived1),
                            };
                            HeadDetail.Add(UCL);
                        }

                    }
                    List<PoFreeItemMasterDC> FreeDetail = new List<PoFreeItemMasterDC>();
                    if (FItemList.Count > 0)
                    {
                        List<FreeItem> FItem = FItemList.Where(z => z.GRNumber == pm.Gr1Number && z.PurchaseOrderId == pm.PurchaseOrderId).ToList();
                        foreach (FreeItem c in FItem)
                        {
                            PoFreeItemMasterDC FCL = new PoFreeItemMasterDC()
                            {
                                Itemname = c.itemname,
                                TotalQuantity = c.TotalQuantity
                            };
                            FreeDetail.Add(FCL);
                        }
                    }
                    AppovePurchaseOrderMasterDc UC = new AppovePurchaseOrderMasterDc()
                    {
                        PurchaseOrderId = pm.PurchaseOrderId,
                        GrNumber = pm.Gr1Number,
                        GrPersonName = pm.Gr1PersonName,
                        Gr_Date = Convert.ToDateTime(pm.Gr1_Date),
                        VehicleType = pm.VehicleType1,
                        VehicleNumber = pm.VehicleNumber1,
                        PoType = pm.PoType,
                        WarehouseName = pm.WarehouseName,
                        Level = pm.Level,
                        progress = pm.progress,
                        CreationDate = pm.CreationDate,
                        ApprovedBy = pm.ApprovedBy,
                        CreatedBy = pm.CreatedBy,
                        DepoName = pm.DepoName,
                        ETotalAmount = pm.ETotalAmount,
                        SupplierName = pm.SupplierName,
                        Status = pm.Gr1Status,
                        PoItemDetail = HeadDetail,
                        PoFreeItemDetail = FreeDetail
                    };
                    HeadData.Add(UC);
                }
                if (pm.Gr2Number != null)
                {
                    List<AppovePurchaseOrderDetailDc> HeadDetail = new List<AppovePurchaseOrderDetailDc>();
                    foreach (PurchaseOrderDetailRecived b in _detail)
                    {
                        if (b.QtyRecived2 != null)
                        {
                            AppovePurchaseOrderDetailDc UCL = new AppovePurchaseOrderDetailDc()
                            {
                                PurchaseOrderDetailId = b.PurchaseOrderDetailId,
                                ItemName = b.ItemName2,
                                MOQ = b.MOQ,
                                Price = Convert.ToDouble(b.Price2),
                                TotalQuantity = b.TotalQuantity,
                                ReceivingQty = Convert.ToInt32(b.QtyRecived2),
                            };
                            HeadDetail.Add(UCL);
                        }
                    }

                    List<PoFreeItemMasterDC> FreeDetail = new List<PoFreeItemMasterDC>();
                    if (FItemList.Count > 0)
                    {

                        List<FreeItem> FItem = FItemList.Where(z => z.GRNumber == pm.Gr2Number && z.PurchaseOrderId == pm.PurchaseOrderId).ToList();
                        foreach (FreeItem c in FItem)
                        {
                            PoFreeItemMasterDC FCL = new PoFreeItemMasterDC()
                            {
                                Itemname = c.itemname,
                                TotalQuantity = c.TotalQuantity
                            };
                            FreeDetail.Add(FCL);
                        }
                    }

                    AppovePurchaseOrderMasterDc UC = new AppovePurchaseOrderMasterDc()
                    {
                        PurchaseOrderId = pm.PurchaseOrderId,
                        GrNumber = pm.Gr2Number,
                        GrPersonName = pm.Gr1PersonName,
                        Gr_Date = Convert.ToDateTime(pm.Gr2_Date),
                        VehicleType = pm.VehicleType2,
                        VehicleNumber = pm.VehicleNumber2,
                        PoType = pm.PoType,
                        WarehouseName = pm.WarehouseName,
                        Level = pm.Level,
                        progress = pm.progress,
                        CreationDate = pm.CreationDate,
                        ApprovedBy = pm.ApprovedBy,
                        CreatedBy = pm.CreatedBy,
                        DepoName = pm.DepoName,
                        ETotalAmount = pm.ETotalAmount,
                        SupplierName = pm.SupplierName,
                        Status = pm.Gr2Status,
                        PoItemDetail = HeadDetail,
                        PoFreeItemDetail = FreeDetail

                    };
                    HeadData.Add(UC);
                }
                if (pm.Gr3Number != null)
                {
                    List<AppovePurchaseOrderDetailDc> HeadDetail = new List<AppovePurchaseOrderDetailDc>();
                    foreach (PurchaseOrderDetailRecived b in _detail)
                    {
                        if (b.QtyRecived3 != null)
                        {
                            AppovePurchaseOrderDetailDc UCL = new AppovePurchaseOrderDetailDc()
                            {

                                PurchaseOrderDetailId = b.PurchaseOrderDetailId,
                                ItemName = b.ItemName3,
                                MOQ = b.MOQ,
                                Price = Convert.ToDouble(b.Price3),
                                TotalQuantity = b.TotalQuantity,
                                ReceivingQty = Convert.ToInt32(b.QtyRecived3),
                            };
                            HeadDetail.Add(UCL);
                        }
                    }
                    List<PoFreeItemMasterDC> FreeDetail = new List<PoFreeItemMasterDC>();
                    if (FItemList.Count > 0)
                    {
                        List<FreeItem> FItem = FItemList.Where(z => z.GRNumber == pm.Gr3Number && z.PurchaseOrderId == pm.PurchaseOrderId).ToList();

                        foreach (FreeItem c in FItem)
                        {
                            PoFreeItemMasterDC FCL = new PoFreeItemMasterDC()
                            {
                                Itemname = c.itemname,
                                TotalQuantity = c.TotalQuantity
                            };
                            FreeDetail.Add(FCL);
                        }
                    }


                    AppovePurchaseOrderMasterDc UC = new AppovePurchaseOrderMasterDc()
                    {
                        PurchaseOrderId = pm.PurchaseOrderId,
                        GrNumber = pm.Gr3Number,
                        GrPersonName = pm.Gr3PersonName,
                        Gr_Date = Convert.ToDateTime(pm.Gr3_Date),
                        VehicleType = pm.VehicleType3,
                        VehicleNumber = pm.VehicleNumber3,
                        PoType = pm.PoType,
                        WarehouseName = pm.WarehouseName,
                        Level = pm.Level,
                        progress = pm.progress,
                        CreationDate = pm.CreationDate,
                        ApprovedBy = pm.ApprovedBy,
                        CreatedBy = pm.CreatedBy,
                        DepoName = pm.DepoName,
                        ETotalAmount = pm.ETotalAmount,
                        SupplierName = pm.SupplierName,
                        Status = pm.Gr3Status,
                        PoItemDetail = HeadDetail,
                        PoFreeItemDetail = FreeDetail

                    };
                    HeadData.Add(UC);
                }
                if (pm.Gr4Number != null)
                {
                    List<AppovePurchaseOrderDetailDc> HeadDetail = new List<AppovePurchaseOrderDetailDc>();
                    foreach (PurchaseOrderDetailRecived b in _detail)
                    {
                        if (b.QtyRecived4 != null)
                        {
                            AppovePurchaseOrderDetailDc UCL = new AppovePurchaseOrderDetailDc()
                            {

                                PurchaseOrderDetailId = b.PurchaseOrderDetailId,
                                ItemName = b.ItemName4,
                                MOQ = b.MOQ,
                                Price = Convert.ToDouble(b.Price4),
                                TotalQuantity = b.TotalQuantity,
                                ReceivingQty = Convert.ToInt32(b.QtyRecived4),
                            };
                            HeadDetail.Add(UCL);
                        }
                    }
                    List<PoFreeItemMasterDC> FreeDetail = new List<PoFreeItemMasterDC>();
                    if (FItemList.Count > 0)
                    {
                        List<FreeItem> FItem = FItemList.Where(z => z.GRNumber == pm.Gr4Number && z.PurchaseOrderId == pm.PurchaseOrderId).ToList();
                        foreach (FreeItem c in FItem)
                        {
                            PoFreeItemMasterDC FCL = new PoFreeItemMasterDC()
                            {
                                Itemname = c.itemname,
                                TotalQuantity = c.TotalQuantity
                            };
                            FreeDetail.Add(FCL);
                        }
                    }
                    AppovePurchaseOrderMasterDc UC = new AppovePurchaseOrderMasterDc()
                    {
                        PurchaseOrderId = pm.PurchaseOrderId,
                        GrNumber = pm.Gr4Number,
                        GrPersonName = pm.Gr4PersonName,
                        Gr_Date = Convert.ToDateTime(pm.Gr4_Date),
                        VehicleType = pm.VehicleType4,
                        VehicleNumber = pm.VehicleNumber4,
                        PoType = pm.PoType,
                        WarehouseName = pm.WarehouseName,
                        Level = pm.Level,
                        progress = pm.progress,
                        CreationDate = pm.CreationDate,
                        ApprovedBy = pm.ApprovedBy,
                        CreatedBy = pm.CreatedBy,
                        DepoName = pm.DepoName,
                        ETotalAmount = pm.ETotalAmount,
                        SupplierName = pm.SupplierName,
                        Status = pm.Gr4Status,
                        PoItemDetail = HeadDetail,
                        PoFreeItemDetail = FreeDetail

                    };
                    HeadData.Add(UC);
                }
                if (pm.Gr5Number != null)
                {
                    List<AppovePurchaseOrderDetailDc> HeadDetail = new List<AppovePurchaseOrderDetailDc>();
                    foreach (PurchaseOrderDetailRecived b in _detail)
                    {
                        if (b.QtyRecived5 != null)
                        {
                            AppovePurchaseOrderDetailDc UCL = new AppovePurchaseOrderDetailDc()
                            {

                                PurchaseOrderDetailId = b.PurchaseOrderDetailId,
                                ItemName = b.ItemName5,
                                MOQ = b.MOQ,
                                Price = Convert.ToDouble(b.Price5),
                                TotalQuantity = b.TotalQuantity,
                                ReceivingQty = Convert.ToInt32(b.QtyRecived5),
                            };
                            HeadDetail.Add(UCL);
                        }
                    }
                    List<PoFreeItemMasterDC> FreeDetail = new List<PoFreeItemMasterDC>();
                    if (FItemList.Count > 0)
                    {
                        List<FreeItem> FItem = FItemList.Where(z => z.GRNumber == pm.Gr5Number && z.PurchaseOrderId == pm.PurchaseOrderId).ToList();

                        foreach (FreeItem c in FItem)
                        {
                            PoFreeItemMasterDC FCL = new PoFreeItemMasterDC()
                            {
                                Itemname = c.itemname,
                                TotalQuantity = c.TotalQuantity
                            };
                            FreeDetail.Add(FCL);
                        }
                    }
                    AppovePurchaseOrderMasterDc UC = new AppovePurchaseOrderMasterDc()
                    {
                        PurchaseOrderId = pm.PurchaseOrderId,
                        WarehouseName = pm.WarehouseName,
                        GrNumber = pm.Gr5Number,
                        GrPersonName = pm.Gr5PersonName,
                        Gr_Date = Convert.ToDateTime(pm.Gr5_Date),
                        VehicleType = pm.VehicleType5,
                        VehicleNumber = pm.VehicleNumber5,
                        PoType = pm.PoType,
                        Level = pm.Level,
                        progress = pm.progress,
                        CreationDate = pm.CreationDate,
                        ApprovedBy = pm.ApprovedBy,
                        CreatedBy = pm.CreatedBy,
                        DepoName = pm.DepoName,
                        ETotalAmount = pm.ETotalAmount,
                        SupplierName = pm.SupplierName,
                        Status = pm.Gr5Status,
                        PoItemDetail = HeadDetail,
                        PoFreeItemDetail = FreeDetail
                    };
                    HeadData.Add(UC);
                }

            }
            if (HeadData.Count > 0) { HeadData.OrderByDescending(w => w.PurchaseOrderId); resultMessage = "Record found"; status = true; } else { resultMessage = "No Record found"; status = false; }
            var res = new
            {
                GRHistory = HeadData,
                status = status,
                Message = resultMessage
            };
            return Request.CreateResponse(HttpStatusCode.OK, res);
        }
        #endregion


        #region Reject GR
        [Route("RejectPoGr")]
        [HttpPost]
        [AllowAnonymous]
        public HttpResponseMessage RejectPoGr(POApproverDc POApproverDc)
        {
            string resultMessage = "";
            bool status = false;
            using (AuthContext context = new AuthContext())
            using (var dbContextTransaction = context.Database.BeginTransaction())
            {
                int UserId = POApproverDc.UserId;
                var people = context.Peoples.Where(x => x.PeopleID == UserId && x.Deleted == false && x.Active).SingleOrDefault();

                var PurchaseOrderMaster = context.DPurchaseOrderMaster.Where(x => x.PurchaseOrderId == POApproverDc.PurchaseOrderId && (x.Status == "UN Received" || x.Status == "UN Partial Received")).SingleOrDefault();
                string sAllCharacter = POApproverDc.GrNumber;
                char GRType = sAllCharacter[sAllCharacter.Length - 1];

                switch (GRType)
                {
                    case 'A':

                        PurchaseOrderMaster.Gr1Status = "RejectGR";
                        PurchaseOrderMaster.Comment = POApproverDc.POApproveStatus;


                        break;
                    case 'B':
                        PurchaseOrderMaster.Gr2Status = "RejectGR";
                        PurchaseOrderMaster.Comment = PurchaseOrderMaster.Comment + Environment.NewLine + POApproverDc.POApproveStatus;

                        break;
                    case 'C':
                        PurchaseOrderMaster.Gr3Status = "RejectGR";
                        PurchaseOrderMaster.Comment = PurchaseOrderMaster.Comment + Environment.NewLine + POApproverDc.POApproveStatus;

                        break;
                    case 'D':
                        PurchaseOrderMaster.Gr4Status = "RejectGR";
                        PurchaseOrderMaster.Comment = PurchaseOrderMaster.Comment + Environment.NewLine + POApproverDc.POApproveStatus;


                        break;
                    case 'E':
                        PurchaseOrderMaster.Gr5Status = "RejectGR";
                        PurchaseOrderMaster.Comment = PurchaseOrderMaster.Comment + Environment.NewLine + POApproverDc.POApproveStatus;

                        break;
                    default:
                        return null;
                }
                context.Entry(PurchaseOrderMaster).State = EntityState.Modified;
                if (context.Commit() > 0)
                {
                    dbContextTransaction.Commit();
                    status = true; resultMessage = POApproverDc.GrNumber + " Gr Reject Successfully";
                }
                else
                {

                    resultMessage = "something went wrong";
                    dbContextTransaction.Rollback();
                    status = false;
                }
                var res = new
                {

                    status = status,
                    Message = resultMessage

                };
                return Request.CreateResponse(HttpStatusCode.OK, res);

            }
        }


        #endregion



        #region Do cancel PO

        [Route("DoCancelpo")]
        [HttpPost]
        [AllowAnonymous]
        public HttpResponseMessage DoCancelpo(POCanceledDc POCanceledDc)
        {
            bool status = false;
            string resultMessage = "";
            using (AuthContext db = new AuthContext())
            {
                logger.Info("Sarthi Purchase Order Cancel");
                // get people information tracking.
                People _people = db.Peoples.Where(q => q.PeopleID == POCanceledDc.UserId && q.Active == true).SingleOrDefault();
                // get purchase order master data for cancel po.
                if (_people != null && POCanceledDc.PurchaseOrderId > 0)
                {
                    PurchaseOrderMaster _PurchaseOrderMaster = db.DPurchaseOrderMaster.Where(x => x.PurchaseOrderId == POCanceledDc.PurchaseOrderId && (x.Status == "Self Approved" || x.Status == "Send for Approval" || x.Status == "Approved" || x.Status == "Blank PO" || x.Status == "Draft")).SingleOrDefault();
                    if (_PurchaseOrderMaster != null)
                    {
                        _PurchaseOrderMaster.Status = "Canceled";
                        _PurchaseOrderMaster.CanceledById = _people.PeopleID;
                        _PurchaseOrderMaster.CanceledByName = _people.DisplayName;
                        _PurchaseOrderMaster.Comment = POCanceledDc.Comment;
                        _PurchaseOrderMaster.CanceledDate = indianTime;
                        if (db.Commit() > 0)
                        {
                            status = true; resultMessage = "PO Canceled Successfully";
                        }
                        else
                        {
                            status = false; resultMessage = "Some thing went wrong";
                        };
                    }
                }
                else { resultMessage = "user not active"; }
            }
            var res = new
            {

                status = status,
                Message = resultMessage

            };
            return Request.CreateResponse(HttpStatusCode.OK, res);

        }
        #endregion



        #region Assignment
        [Route("Assignment")]
        [HttpPost]
        public HttpResponseMessage UpdateUploadAssignment(AssignmentImageupload AssignmentImageupload)
        {
            bool status = false;
            string resultMessage = "";
            using (AuthContext context = new AuthContext())
            {
                var Upload = context.DeliveryIssuanceDb.Where(s => s.DeliveryIssuanceId == AssignmentImageupload.DeliveryIssuanceId && s.Status == "Freezed").FirstOrDefault();
                if (Upload != null)
                {
                    Upload.IcUploadedFile = AssignmentImageupload.AssignmentUpload;
                    Upload.UpdatedDate = indianTime;
                    context.Entry(Upload).State = EntityState.Modified;
                    if (context.Commit() > 0) { status = true; resultMessage = "record updated succefully"; } else { status = false; resultMessage = "something"; };

                }
                var res = new
                {
                    AssignmentImageupload = AssignmentImageupload,
                    status = status,
                    Message = resultMessage

                };
                return Request.CreateResponse(HttpStatusCode.OK, res);
            }
        }


        [Route("SearchAssignment")]
        [HttpGet]
        public async Task<List<AssignmentImageupload>> GeAssignmentIdList(int WarehouseId, int DeliveryIssuanceId)
        {
            SarthiManager manager = new SarthiManager();
            return await manager.GetAssignmentIdList(WarehouseId, DeliveryIssuanceId);

        }

        [Route("AssignmentUpload")]
        [HttpPost]
        [AllowAnonymous]
        public HttpResponseMessage AssignmentUpload()
        {
            string ImageUrl = string.Empty;

            bool status = false;
            string resultMessage = "";
            if (HttpContext.Current.Request.Files.AllKeys.Any())
            {
                var httpPostedFile = HttpContext.Current.Request.Files["file"];
                if (httpPostedFile != null)
                {
                    ImageUrl = Path.Combine(HttpContext.Current.Server.MapPath("~/images/AssigmentIcImages"), httpPostedFile.FileName);
                    httpPostedFile.SaveAs(ImageUrl);
                    AngularJSAuthentication.Common.Helpers.FileUploadHelper.Upload(httpPostedFile.FileName, "~/images/AssigmentIcImages", ImageUrl);
                    status = true;
                    resultMessage = " file uploaded successfully.";
                    ImageUrl = "/images/AssigmentIcImages/" + httpPostedFile.FileName;
                }
            }
            else
            {
                ImageUrl = "";
                status = false;
                resultMessage = "some thing went wrong";
            }
            var res = new
            {
                ImageUrl = ImageUrl,
                status = status,
                Message = resultMessage
            };
            return Request.CreateResponse(HttpStatusCode.OK, res);


        }

        #endregion


        //[AllowAnonymous]
        //[Route("SarthiGetOrder")]
        //[HttpGet]
        //public HttpResponseMessage getapproval(int wid)
        //{
        //    using (AuthContext context = new AuthContext())
        //    {
        //        var obj = (from ORCA in context.orderRedispatchCountApprovalDB
        //                   join ODM in context.OrderDispatchedMasters
        //                   on ORCA.OrderId equals ODM.OrderId
        //                   where ORCA.IsApproved == false && ORCA.Redispatchcount >= 2 && ODM.WarehouseId == wid
        //                   select new SarthiApprovalDTO
        //                   {
        //                       IsApproved = false,
        //                       OrderId = ORCA.OrderId,
        //                       Redispatchcount = ORCA.Redispatchcount,
        //                       CreatedDate = ORCA.CreatedDate,
        //                       UpdateDate = ORCA.UpdateDate,
        //                       UserId = ORCA.userId,
        //                       Id = ORCA.Id
        //                   }).ToList();

        //        return Request.CreateResponse(HttpStatusCode.OK, obj);

        //    }

        //}



        [Route("SarthiGetOrder")]
        [HttpGet]
        [AllowAnonymous]
        public List<SarthiApprovalDTO> OrderRedispatchedCountApproval(int wid, int peopleid)
        {

            List<SarthiApprovalDTO> Data = new List<SarthiApprovalDTO>();
            using (AuthContext context = new AuthContext())
            {
                var People = context.Peoples.Where(x => x.PeopleID == peopleid).SingleOrDefault();
                string query = "select distinct r.Name as Role, p.PeopleID from People p inner join AspNetUsers u on p.Email=u.Email inner join AspNetUserRoles ur on u.Id=ur.UserId inner join AspNetRoles r on ur.RoleId=r.Id where PeopleID='" + People.PeopleID + "'and ur.isActive=1 and p.Active=1 and p.Deleted=0";
                var SpLogin = context.Database.SqlQuery<SpLogin>(query).ToList();
                if (People != null && SpLogin != null && SpLogin.Any())
                {
                    //if (SpLogin.Any() && SpLogin.Any(x => x.Role == "WH Service lead"))
                    //{
                    //    Data = (from ORCA in context.orderRedispatchCountApprovalDB
                    //            join ODM in context.OrderDispatchedMasters
                    //            on ORCA.OrderId equals ODM.OrderId
                    //            where ORCA.IsApproved == false && ORCA.Redispatchcount <= 1 && ODM.WarehouseId == wid
                    //            select new SarthiApprovalDTO
                    //            {
                    //                IsApproved = ORCA.IsApproved,
                    //                OrderId = ORCA.OrderId,
                    //                Redispatchcount = ORCA.Redispatchcount,
                    //                CreatedDate = ORCA.CreatedDate,
                    //                UpdateDate = ORCA.UpdateDate,
                    //                UserId = ORCA.userId,
                    //                Id = ORCA.Id
                    //            }).ToList();
                    //    //return Data;
                    //}
                    //else 

                    if (SpLogin.Any() && SpLogin.Any(x => x.Role == "Regional Outbound Lead"))
                    {
                        Data = (from ORCA in context.orderRedispatchCountApprovalDB
                                join ODM in context.OrderDispatchedMasters
                                on ORCA.OrderId equals ODM.OrderId
                                where ORCA.IsApproved == false && ORCA.Redispatchcount == 2 && ODM.WarehouseId == wid
                                select new SarthiApprovalDTO
                                {
                                    IsApproved = ORCA.IsApproved,
                                    OrderId = ORCA.OrderId,
                                    Redispatchcount = ORCA.Redispatchcount,
                                    CreatedDate = ORCA.CreatedDate,
                                    UpdateDate = ORCA.UpdateDate,
                                    UserId = ORCA.userId,
                                    Id = ORCA.Id
                                }).ToList();

                        //return Data;
                    }
                    else if (SpLogin.Any() && SpLogin.Any(x => x.Role == "Zonal Outbound Lead"))
                    {
                        Data = (from ORCA in context.orderRedispatchCountApprovalDB
                                join ODM in context.OrderDispatchedMasters
                                on ORCA.OrderId equals ODM.OrderId
                                where ORCA.IsApproved == false && ORCA.Redispatchcount == 3 && ODM.WarehouseId == wid
                                select new SarthiApprovalDTO
                                {
                                    IsApproved = ORCA.IsApproved,
                                    OrderId = ORCA.OrderId,
                                    Redispatchcount = ORCA.Redispatchcount,
                                    CreatedDate = ORCA.CreatedDate,
                                    UpdateDate = ORCA.UpdateDate,
                                    UserId = ORCA.userId,
                                    Id = ORCA.Id
                                }).ToList();

                        // return Data;
                    }

                    return Data;
                    //else
                    //{
                    //    Data = (from ORCA in context.orderRedispatchCountApprovalDB
                    //            join ODM in context.OrderDispatchedMasters
                    //            on ORCA.OrderId equals ODM.OrderId
                    //            where ORCA.IsApproved == false && ORCA.Redispatchcount >= 4 && ODM.WarehouseId == wid
                    //            select new SarthiApprovalDTO
                    //            {
                    //                IsApproved = ORCA.IsApproved,
                    //                OrderId = ORCA.OrderId,
                    //                Redispatchcount = ORCA.Redispatchcount,
                    //                CreatedDate = ORCA.CreatedDate,
                    //                UpdateDate = ORCA.UpdateDate,
                    //                UserId = ORCA.userId,
                    //                Id = ORCA.Id
                    //            }).ToList();

                    //    return Data;
                    //}
                }
                else { return null; }
            }

        }



        #endregion   Old Sarthi end

        #region New GRN  Process Module start

        [Route("GetBatchListByNumber")]
        [HttpGet]
        [AllowAnonymous]
        public async Task<List<BatchDc>> GetBatchListByNumber(string ItemNumber)
        {
            List<BatchDc> result = new List<BatchDc>();
            if (ItemNumber != null && ItemNumber.Length > 1)
            {
                using (AuthContext db = new AuthContext())
                {
                    result = db.BatchMasters.Where(x => x.ItemNumber == ItemNumber && x.IsActive == true && (x.ExpiryDate != null && x.ExpiryDate >= indianTime) && x.IsDeleted == false).Select(x => new BatchDc
                    {
                        BatchCode = x.BatchCode,
                        ItemNumber = x.ItemNumber,
                        BatchMasterId = x.Id,
                        MFGDate = x.MFGDate,
                        ExpiryDate = x.ExpiryDate,
                    }).ToList();
                }
            }
            return result;
        }


        #region  GRHistory
        [Route("GRHistory/V1")]
        [HttpGet]
        [AllowAnonymous]
        public HttpResponseMessage GRHistoryV1(long PurchaseOrderId)
        {
            bool status = false;
            string resultMessage = "";
            var GoodsReceived = new List<GoodsReceivedDc>();
            using (var db = new AuthContext())
            {
                var Gr = db.GoodsReceivedDetail.Where(x => x.PurchaseOrderDetail.PurchaseOrderId == PurchaseOrderId && x.IsDeleted == false).Include(x => x.PurchaseOrderDetail).Include(x => x.GRBatchs).ToList();
                var _result = Mapper.Map(Gr).ToANew<List<GoodsReceivedDetailDc>>();
                if (_result != null && _result.Any())
                {
                    var CreatedBy = Gr.Select(x => x.CreatedBy).Distinct().ToList();
                    var ApprovedBy = Gr.Select(x => x.ApprovedBy).Distinct().ToList();
                    var peopleIds = CreatedBy.Concat(ApprovedBy);
                    var people = db.Peoples.Where(x => peopleIds.Contains(x.PeopleID)).ToList();
                    var GDNList = db.GoodsDescripancyNoteMasterDB.Where(x => x.PurchaseOrderId == PurchaseOrderId && x.IsActive == true && x.IsDeleted == false && x.IsGDNGenerate == true).Include(x => x.goodsDescripancyNoteDetail).ToList();

                    GoodsReceived = _result.GroupBy(x => new
                    {
                        x.GrSerialNumber,
                        x.Status,
                        x.ApprovedBy,
                        x.VehicleType,
                        x.VehicleNumber,
                        x.CreatedDate,
                        //x.ModifiedDate,
                        x.CreatedBy,
                        // x.ModifiedBy,
                    }).Select(y => new GoodsReceivedDc
                    {
                        GrPersonName = people.Where(p => p.PeopleID == y.Key.CreatedBy).Select(p => p.DisplayName).FirstOrDefault(),
                        CreatedBy = y.Key.CreatedBy,
                        CreatedDate = y.Key.CreatedDate,
                        GrSerialNumber = y.Key.GrSerialNumber,
                        //ModifiedBy = y.Key.ModifiedBy,
                        ApproverName = people.Where(p => p.PeopleID == y.Key.ApprovedBy).Select(p => p.DisplayName).FirstOrDefault(),
                        //ModifiedDate = y.Key.ModifiedDate,
                        Status = y.Key.Status,
                        VehicleNumber = y.Key.VehicleNumber,
                        VehicleType = y.Key.VehicleType,
                        GoodsReceivedItemDcs = y.Select(x => new GoodsReceivedItemDc
                        {
                            Id = x.Id,
                            PurchaseOrderDetailId = x.PurchaseOrderDetailId,
                            ItemName = Gr.Any(p => p.PurchaseOrderDetailId == x.PurchaseOrderDetailId) ? Gr.FirstOrDefault(p => p.PurchaseOrderDetailId == x.PurchaseOrderDetailId).PurchaseOrderDetail?.ItemName : "",
                            Itemnumber = Gr.Any(p => p.PurchaseOrderDetailId == x.PurchaseOrderDetailId) ? Gr.FirstOrDefault(p => p.PurchaseOrderDetailId == x.PurchaseOrderDetailId).PurchaseOrderDetail?.ItemNumber : "",
                            TotalQuantity = Gr.Any(p => p.PurchaseOrderDetailId == x.PurchaseOrderDetailId) ? Gr.FirstOrDefault(p => p.PurchaseOrderDetailId == x.PurchaseOrderDetailId).PurchaseOrderDetail.TotalQuantity : 0,
                            ItemMultiMRPId = x.ItemMultiMRPId,
                            Qty = x.Qty,
                            DamageQty = x.DamageQty,
                            ExpiryQty = x.ExpiryQty,
                            ShortQty = x.ShortQty,
                            Price = x.Price,
                            CurrentStockHistoryId = x.CurrentStockHistoryId,
                            BatchNo = x.BatchNo,
                            MFGDate = x.MFGDate,
                            Barcode = x.Barcode,
                            weight = x.weight,
                            IsFreeItem = Gr.Any(p => p.PurchaseOrderDetailId == x.PurchaseOrderDetailId) ? Gr.Where(p => p.PurchaseOrderDetailId == x.PurchaseOrderDetailId).Select(p => p.IsFreeItem).FirstOrDefault() : false,
                            GrSerialNumber = Gr.Any(p => p.PurchaseOrderDetailId == x.PurchaseOrderDetailId) ? Gr.Where(p => p.PurchaseOrderDetailId == x.PurchaseOrderDetailId).Select(p => p.GrSerialNumber).FirstOrDefault() : 0
                        }).ToList()
                    }).ToList();

                    if (Gr != null && Gr.Any())
                    {
                        var ItemnumberList = GoodsReceived.SelectMany(x => x.GoodsReceivedItemDcs.Select(i => i.Itemnumber)).ToList();
                        var itemMultiMrpdetail = db.ItemMultiMRPDB.Where(a => ItemnumberList.Contains(a.ItemNumber) && a.Deleted == false).Select(x => new ItemMultiMRPDc { ItemNumber = x.ItemNumber, ItemMultiMRPId = x.ItemMultiMRPId, MRP = x.MRP }).ToList();
                        GoodsReceived.ForEach(x =>
                        {
                            x.GrSerialNumberWithPO = PurchaseOrderId + "-" + x.GrSerialNumber;
                            x.IsGDN = GDNList != null ? GDNList.Any(y => y.GrSerialNo == x.GrSerialNumber) : false;
                            x.GRAmount = x.GoodsReceivedItemDcs.Sum(y => y.Price * y.Qty);
                            x.GoodsReceivedItemDcs.ForEach(i =>
                            {
                                if (itemMultiMrpdetail != null && itemMultiMrpdetail.Any(y => y.ItemNumber == i.Itemnumber))
                                {
                                    i.MRP = itemMultiMrpdetail.Where(c => c.ItemNumber == i.Itemnumber && c.ItemMultiMRPId == i.ItemMultiMRPId).Select(c => c.MRP).FirstOrDefault();
                                }
                            });
                        });
                        status = true; resultMessage = "Record found";
                    }
                }
                else { status = true; resultMessage = "No Record found"; }
            }
            var res = new
            {
                GRHistory = GoodsReceived,
                status = status,
                Message = resultMessage
            };
            return Request.CreateResponse(HttpStatusCode.OK, res);
        }

        #endregion

        #region Provide PO data for GR GetPOGRDetailsV1
        /// <summary>
        /// Requested gr data
        /// </summary>
        /// <param name="PurchaseOrderId"></param>
        /// <returns></returns>
        [Route("POGRDetails/V1")]
        [HttpGet]
        [AllowAnonymous]
        public HttpResponseMessage GetPOGRDetailsV1(int PurchaseOrderId)
        {

            string Message = "";
            int GrSerialNumber = 1;
            bool IsReject = false;
            using (AuthContext context = new AuthContext())
            {
                POGR _POGR = new POGR();
                List<PurchaseOrderDetailDc> PurchaseOrderDetailDc = new List<PurchaseOrderDetailDc>();
                var PurchaseOrderMaster = context.DPurchaseOrderMaster.Where(x => x.PurchaseOrderId == PurchaseOrderId).Include(x => x.PurchaseOrderDetail).SingleOrDefault();
                _POGR.PickerType = PurchaseOrderMaster.PickerType;
                var PurchaseOrderDetails = PurchaseOrderMaster.PurchaseOrderDetail;
                _POGR.IsGDNAllow = context.Warehouses.FirstOrDefault(x => x.WarehouseId == PurchaseOrderMaster.WarehouseId).IsGDNAllow;
                List<string> numbers = PurchaseOrderDetails.Select(x => x.ItemNumber).Distinct().ToList();
                List<GrQualityConfiguration> qualityConfigurations = context.GrQualityConfigurations.Where(x => x.WarehouseId == PurchaseOrderMaster.WarehouseId && numbers.Contains(x.ItemNumber) && x.IsActive == true && x.IsDeleted == false).ToList();

                // var PurchaseOrderDetails = context.DPurchaseOrderDeatil.Where(x => x.PurchaseOrderId == PurchaseOrderId && x.IsDeleted == false).ToList();
                var Gr = context.GoodsReceivedDetail.Where(x => x.PurchaseOrderDetail.PurchaseOrderId == PurchaseOrderId && x.IsDeleted == false).Include(x => x.PurchaseOrderDetail).ToList();

                var itemMultiMRPIds = Gr.Select(k => k.ItemMultiMRPId).Distinct().ToList();
                var multimrp = context.ItemMultiMRPDB.Where(x => itemMultiMRPIds.Contains(x.ItemMultiMRPId)).ToList();

                if ((PurchaseOrderMaster.Status == "Self Approved" || PurchaseOrderMaster.Status == "Approved" || PurchaseOrderMaster.Status == "Partial Received" || PurchaseOrderMaster.Status == "CN Partial Received") && (PurchaseOrderMaster.Status != "Auto Closed" || PurchaseOrderMaster.Status != "Closed"))
                {
                    var PurchaseOrderDeatil = PurchaseOrderDetails.Select(j =>
                                                  new PurchaseOrderDetailDc
                                                  {
                                                      CompanyStockCode = multimrp.Where(y => y.ItemMultiMRPId == j.ItemMultiMRPId).Select(y => y.CompanyStockCode).FirstOrDefault(),
                                                      PurchaseOrderDetailId = j.PurchaseOrderDetailId,
                                                      PurchaseOrderId = j.PurchaseOrderId,
                                                      ItemNumber = j.ItemNumber,
                                                      ItemName = j.ItemName,
                                                      Price = j.Price,
                                                      MRP = j.MRP,
                                                      MOQ = j.MOQ,
                                                      PurchaseSku = j.PurchaseSku,
                                                      TotalQuantity = j.TotalQuantity,
                                                      ReceivingQty = 0,
                                                      TotalRecivedQty = Gr.Any(p => p.PurchaseOrderDetailId == j.PurchaseOrderDetailId) ? Gr.Where(p => p.PurchaseOrderDetailId == j.PurchaseOrderDetailId).Sum(p => p.Qty) : 0,
                                                      ItemMultiMRPId = j.ItemMultiMRPId,
                                                      BatchNo = null,
                                                      MFGDate = null,
                                                      IsFreeItem = Gr.Any(p => p.PurchaseOrderDetailId == j.PurchaseOrderDetailId) ? Gr.Where(p => p.PurchaseOrderDetailId == j.PurchaseOrderDetailId).Select(p => p.IsFreeItem).FirstOrDefault() : false,
                                                      GrSerialNumber = GrSerialNumber,
                                                      DamageQty = 0,
                                                      ExpiryQty = 0,
                                                      ShortQty = 0,
                                                      DamageExpiryPhysicalQty = Gr.Any(p => p.PurchaseOrderDetailId == j.PurchaseOrderDetailId) ? Gr.Where(p => p.PurchaseOrderDetailId == j.PurchaseOrderDetailId && p.IsDamageExpiryPhysical == true).Sum(p => p.DamageQty + p.ExpiryQty) : 0,
                                                      IsQualityItemTesting = qualityConfigurations != null && qualityConfigurations.Any(x => x.ItemNumber == j.ItemNumber) ? qualityConfigurations.FirstOrDefault(x => x.ItemNumber == j.ItemNumber).IsActive : false,
                                                      IsQualityReportUpload = qualityConfigurations != null && qualityConfigurations.Any(x => x.ItemNumber == j.ItemNumber) ? qualityConfigurations.FirstOrDefault(x => x.ItemNumber == j.ItemNumber).IsReportUpload : false,
                                                  }).ToList();
                    // if debit note generated then check
                    bool IsDebitNote = context.IRCreditNoteMaster.Any(x => x.IRMasters.PurchaseOrderId == PurchaseOrderMaster.PurchaseOrderId && x.IsActive == true && x.IsDeleted == false);
                    if (IsDebitNote)
                    {
                        PurchaseOrderDeatil.ForEach(x =>
                        {
                            x.TotalRecivedQty = x.TotalRecivedQty + GetShortQuantity(x.PurchaseOrderDetailId, Gr);
                        });
                    }
                    List<string> itemNumber = PurchaseOrderDeatil.Select(x => x.ItemNumber).ToList();
                    var itemdetail = context.ItemMasterCentralDB.Where(c => itemNumber.Contains(c.Number)).Select(x => new { x.Number, x.IsSensitive, x.IsSensitiveMRP, x.weight, x.weighttype }).ToList();

                    var itembarcodelist = context.ItemBarcodes.Where(c => itemNumber.Contains(c.ItemNumber) && c.IsActive == true && c.IsDeleted == false).Select(x => new ItemBarcodeDc { ItemNumber = x.ItemNumber, Barcode = x.Barcode }).ToList();


                    var itemMultiMrpdetail = context.ItemMultiMRPDB.Where(a => itemNumber.Contains(a.ItemNumber) && a.Deleted == false).Select(x => new ItemMultiMRPDc { ItemNumber = x.ItemNumber, ItemMultiMRPId = x.ItemMultiMRPId, MRP = x.MRP, CompanyStockCode = x.CompanyStockCode }).ToList();
                    PurchaseOrderDeatil.ForEach(x =>
                    {
                        if (itemdetail != null && itemdetail.Any(y => y.Number == x.ItemNumber))
                        {

                            x.TotalRecivedQty += x.DamageExpiryPhysicalQty;
                            var item = itemdetail.FirstOrDefault(y => y.Number == x.ItemNumber);
                            PurchaseOrderDetail items = context.DPurchaseOrderDeatil.Where(y => y.PurchaseOrderDetailId == x.PurchaseOrderDetailId && y.IsDeleted == false).FirstOrDefault();
                            x.Barcodes = (itembarcodelist != null && itembarcodelist.Any(e => e.ItemNumber == item.Number)) ? itembarcodelist.Where(e => e.ItemNumber == item.Number).Select(e => e.Barcode).ToList() : null;
                            x.IsCommodity = false;
                            if (items.WeightType != null && items.WeightType == "Kg" && items.Weight > 0)
                            {
                                x.weight = items.Weight * 1000;

                            }
                            else if (items.WeightType != null && items.WeightType == "Gm" && items.Weight > 0)
                            {
                                x.weight = items.Weight;
                            }
                            else
                            {
                                if (item.weighttype != null && item.weighttype == "Kg" && item.weight > 0)
                                {
                                    x.weight = item.weight * 1000;
                                }
                                else if (item.weighttype != null && item.weighttype == "Gm" && item.weight > 0)
                                {
                                    x.weight = item.weight;
                                }
                                else
                                {
                                    x.weight = 0;
                                }
                            }
                            //removed due lot of issue comes and no clearences PD
                            //if (item.IsSensitive == true && item.IsSensitiveMRP == true)
                            //{
                            //    x.IsCommodity = false;
                            //}
                            //else if (item.IsSensitive == true && item.IsSensitiveMRP == false)
                            //{
                            //    x.IsCommodity = true;
                            //}
                        }
                        if (itemMultiMrpdetail != null && itemMultiMrpdetail.Any(y => y.ItemNumber == x.ItemNumber))
                        {
                            x.multiMrpIds = itemMultiMrpdetail.Where(y => y.ItemNumber == x.ItemNumber).ToList();
                            //x.CompanyStockCode = itemMultiMrpdetail.Where(y => y.ItemNumber == x.ItemNumber).ToList();
                        }
                    });
                    if (Gr != null && Gr.Any() && !IsReject)
                    {
                        GrSerialNumber = Gr.Max(r => r.GrSerialNumber);
                        GrSerialNumber++;
                    }
                    _POGR.GrSerialNumber = GrSerialNumber;
                    _POGR.PurchaseOrderDetailDc = PurchaseOrderDeatil;
                    Message = "Record found.";
                }
                else if ((PurchaseOrderMaster.Status == "UN Received" || PurchaseOrderMaster.Status == "UN Partial Received") && (PurchaseOrderMaster.Status != "Auto Closed" || PurchaseOrderMaster.Status != "Closed"))
                {
                    var RejectGr = Gr.Where(x => x.Status == 3 && x.IsDeleted == false).ToList();
                    if (RejectGr != null && RejectGr.Any())
                    {

                        var PurchaseOrderDetailIdlist = RejectGr.Select(x => x.PurchaseOrderDetailId);
                        var tempDetails = PurchaseOrderDetails.Where(x => PurchaseOrderDetailIdlist.Contains(x.PurchaseOrderDetailId)).ToList();
                        var PurchaseOrderDeatil = tempDetails.Select(j =>
                                                         new PurchaseOrderDetailDc
                                                         {
                                                             CompanyStockCode = multimrp.Where(y => y.ItemMultiMRPId == j.ItemMultiMRPId).Select(y => y.CompanyStockCode).FirstOrDefault(),
                                                             PurchaseOrderDetailId = j.PurchaseOrderDetailId,
                                                             PurchaseOrderId = j.PurchaseOrderId,
                                                             ItemNumber = j.ItemNumber,
                                                             ItemName = j.ItemName,
                                                             Price = RejectGr.Any(r => r.PurchaseOrderDetailId == j.PurchaseOrderDetailId) ? Gr.Where(p => p.PurchaseOrderDetailId == j.PurchaseOrderDetailId && p.Status == 3).Select(p => p.Price).FirstOrDefault() : 0,
                                                             MRP = j.MRP,
                                                             MOQ = j.MOQ,
                                                             PurchaseSku = j.PurchaseSku,
                                                             TotalQuantity = j.TotalQuantity,
                                                             ReceivingQty = RejectGr.Any(r => r.PurchaseOrderDetailId == j.PurchaseOrderDetailId) ? Gr.Where(p => p.PurchaseOrderDetailId == j.PurchaseOrderDetailId && p.Status == 3).Sum(p => p.Qty) : 0,
                                                             TotalRecivedQty = (Gr.Any(p => p.PurchaseOrderDetailId == j.PurchaseOrderDetailId) ? Gr.Where(p => p.PurchaseOrderDetailId == j.PurchaseOrderDetailId).Sum(p => p.Qty) : 0) - (RejectGr.Any(r => r.PurchaseOrderDetailId == j.PurchaseOrderDetailId) ? Gr.Where(p => p.PurchaseOrderDetailId == j.PurchaseOrderDetailId && p.Status == 3).Sum(p => p.Qty) : 0),
                                                             ItemMultiMRPId = RejectGr.Any(r => r.PurchaseOrderDetailId == j.PurchaseOrderDetailId) ? Gr.Where(p => p.PurchaseOrderDetailId == j.PurchaseOrderDetailId).Select(p => p.ItemMultiMRPId).FirstOrDefault() : 0,
                                                             BatchNo = RejectGr.Any(r => r.PurchaseOrderDetailId == j.PurchaseOrderDetailId) ? Gr.Where(p => p.PurchaseOrderDetailId == j.PurchaseOrderDetailId && p.Status == 3).Select(p => p.BatchNo).FirstOrDefault() : null,
                                                             MFGDate = RejectGr.Any(r => r.PurchaseOrderDetailId == j.PurchaseOrderDetailId) ? Gr.Where(p => p.PurchaseOrderDetailId == j.PurchaseOrderDetailId && p.Status == 3).Select(p => p.MFGDate).FirstOrDefault() : null,
                                                             IsFreeItem = RejectGr.Any(r => r.PurchaseOrderDetailId == j.PurchaseOrderDetailId) ? Gr.Where(p => p.PurchaseOrderDetailId == j.PurchaseOrderDetailId && p.Status == 3).Select(p => p.IsFreeItem).FirstOrDefault() : false,
                                                             GrSerialNumber = RejectGr.Any(r => r.PurchaseOrderDetailId == j.PurchaseOrderDetailId) ? Gr.Where(p => p.PurchaseOrderDetailId == j.PurchaseOrderDetailId && p.Status == 3).Select(p => p.GrSerialNumber).FirstOrDefault() : 0,
                                                             DamageQty = RejectGr.Any(r => r.PurchaseOrderDetailId == j.PurchaseOrderDetailId) ? Gr.Where(p => p.PurchaseOrderDetailId == j.PurchaseOrderDetailId && p.Status == 3).Sum(p => p.DamageQty) : 0,
                                                             ExpiryQty = RejectGr.Any(r => r.PurchaseOrderDetailId == j.PurchaseOrderDetailId) ? Gr.Where(p => p.PurchaseOrderDetailId == j.PurchaseOrderDetailId && p.Status == 3).Sum(p => p.ExpiryQty) : 0,
                                                             ShortQty = RejectGr.Any(r => r.PurchaseOrderDetailId == j.PurchaseOrderDetailId) ? Gr.Where(p => p.PurchaseOrderDetailId == j.PurchaseOrderDetailId && p.Status == 3).Sum(p => p.ShortQty) : 0,
                                                             DamageExpiryPhysicalQty = (Gr.Any(p => p.PurchaseOrderDetailId == j.PurchaseOrderDetailId) ? Gr.Where(p => p.PurchaseOrderDetailId == j.PurchaseOrderDetailId && p.IsDamageExpiryPhysical == true).Sum(p => p.DamageQty + p.ExpiryQty + p.ShortQty) : 0) - (RejectGr.Any(r => r.PurchaseOrderDetailId == j.PurchaseOrderDetailId) ? Gr.Where(p => p.PurchaseOrderDetailId == j.PurchaseOrderDetailId && p.IsDamageExpiryPhysical == true && p.Status == 3).Sum(p => p.DamageQty + p.ExpiryQty + p.ShortQty) : 0),
                                                             weight = RejectGr.Any(r => r.PurchaseOrderDetailId == j.PurchaseOrderDetailId) ? Gr.Where(p => p.PurchaseOrderDetailId == j.PurchaseOrderDetailId && p.Status == 3).Select(p => p.weight).FirstOrDefault() : 0,
                                                             IsQualityItemTesting = qualityConfigurations != null && qualityConfigurations.Any(x => x.ItemNumber == j.ItemNumber) ? true : false,
                                                             IsQualityReportUpload = qualityConfigurations != null && qualityConfigurations.Any(x => x.ItemNumber == j.ItemNumber) ? qualityConfigurations.FirstOrDefault(x => x.ItemNumber == j.ItemNumber).IsReportUpload : false,
                                                         }).ToList();

                        List<string> itemNumber = PurchaseOrderDeatil.Select(x => x.ItemNumber).ToList();
                        var itemdetail = context.ItemMasterCentralDB.Where(c => itemNumber.Contains(c.Number)).Select(x => new { x.Number, x.IsSensitive, x.IsSensitiveMRP }).ToList();
                        var itembarcodelist = context.ItemBarcodes.Where(c => itemNumber.Contains(c.ItemNumber) && c.IsActive == true && c.IsDeleted == false).Select(x => new ItemBarcodeDc { ItemNumber = x.ItemNumber, Barcode = x.Barcode }).ToList();

                        var itemMultiMrpdetail = context.ItemMultiMRPDB.Where(a => itemNumber.Contains(a.ItemNumber) && a.Deleted == false).Select(x => new ItemMultiMRPDc { ItemNumber = x.ItemNumber, ItemMultiMRPId = x.ItemMultiMRPId, MRP = x.MRP, CompanyStockCode = x.CompanyStockCode }).ToList();
                        PurchaseOrderDeatil.ForEach(x =>
                        {
                            if (itemdetail != null && itemdetail.Any(y => y.Number == x.ItemNumber))
                            {
                                x.TotalRecivedQty += x.DamageExpiryPhysicalQty;

                                var item = itemdetail.FirstOrDefault(y => y.Number == x.ItemNumber);
                                x.Barcodes = (itembarcodelist != null && itembarcodelist.Any(e => e.ItemNumber == item.Number)) ? itembarcodelist.Where(e => e.ItemNumber == item.Number).Select(e => e.Barcode).ToList() : null;
                                x.IsCommodity = false;
                                //removed due lot of issue comes and no clearences PD

                                //if (item.IsSensitive == true && item.IsSensitiveMRP == true)
                                //{
                                //    x.IsCommodity = false;
                                //}
                                //else if (item.IsSensitive == true && item.IsSensitiveMRP == false)
                                //{
                                //    x.IsCommodity = true;
                                //}
                            }
                            if (itemMultiMrpdetail != null && itemMultiMrpdetail.Any(y => y.ItemNumber == x.ItemNumber))
                            {
                                x.multiMrpIds = itemMultiMrpdetail.Where(y => y.ItemNumber == x.ItemNumber).ToList();
                                //x.CompanyStockCode = itemMultiMrpdetail.Where(y => y.ItemNumber == x.ItemNumber).ToList();
                            }
                        });

                        _POGR.GrSerialNumber = RejectGr.Select(x => x.GrSerialNumber).FirstOrDefault();
                        _POGR.PurchaseOrderDetailDc = PurchaseOrderDeatil;
                        Message = "Record found.";
                    }
                    else { Message = "You can't do GR Now, Because Currently it is under status :" + PurchaseOrderMaster.Status; }
                }
                else if (PurchaseOrderMaster.Status == "CN Received" || PurchaseOrderMaster.Status == "Received")
                {
                    Message = "You can't do GR Now, Because Currently Full GRN is done;";

                }
                else if (PurchaseOrderMaster.Status == "Auto Closed" || PurchaseOrderMaster.Status == "Closed")
                {
                    Message = "You can't do GR Now, Because Currently Po is : " + PurchaseOrderMaster.Status;

                }
                else
                {
                    Message = "You can't do GR Now, Because Currently it is under status :" + PurchaseOrderMaster.Status;
                }
                var res = new
                {
                    POGRDetail = _POGR,
                    status = true,
                    Message = Message
                };
                return Request.CreateResponse(HttpStatusCode.OK, res);
            }

        }

        private int GetShortQuantity(int PurchaseOrderDetailId, List<GoodsReceivedDetail> ObjGoodsReceivedDetail)
        {
            int shortQunatity = 0;
            using (AuthContext context = new AuthContext())
            {
                List<long> GoodReceiveDetailids = ObjGoodsReceivedDetail.Where(x => x.PurchaseOrderDetailId == PurchaseOrderDetailId).Select(x => x.Id).ToList();
                if (GoodReceiveDetailids != null && GoodReceiveDetailids.Any())
                {
                    foreach (var Ids in GoodReceiveDetailids)
                    {
                        shortQunatity += context.InvoiceReceiptDetail.Where(x => x.GoodsReceivedDetailId == Ids && x.IsActive == true && x.IsDeleted == false).Select(x => x.IRExcludedquantity).FirstOrDefault();
                    }
                }
                return shortQunatity;
            }
        }
        #endregion

        #region  Add GR before approved AddPOGRDetails/V1 
        [Route("AddPOGRDetails/V1")]
        [HttpPost]
        [AllowAnonymous]
        public HttpResponseMessage AddPOGRDetailsV1(AddPOGRDetailDc AddPOGRDetailobj)
        {
            bool IsValidate = false;
            bool IsValidateQTy = false;
            string resultMessage = "";
            TransactionOptions option = new TransactionOptions();
            option.IsolationLevel = IsolationLevel.RepeatableRead;
            option.Timeout = TimeSpan.FromSeconds(90);

            using (var dbContextTransaction = new TransactionScope(TransactionScopeOption.Required, option))
            {
                using (AuthContext context = new AuthContext())
                {
                    if (AddPOGRDetailobj.PurchaseOrderDetailDc != null && AddPOGRDetailobj.PurchaseOrderDetailDc.Any() && AddPOGRDetailobj.GrSerialNumber > 0)
                    {
                        int peopleId = AddPOGRDetailobj.UserId;
                        People people = context.Peoples.Where(q => q.PeopleID == peopleId && q.Active == true).SingleOrDefault();
                        var PoId = AddPOGRDetailobj.PurchaseOrderDetailDc[0].PurchaseOrderId;//poid
                        var PurchaseOrderMaster = context.DPurchaseOrderMaster.Where(x => x.PurchaseOrderId == PoId).Include(x => x.PurchaseOrderDetail).SingleOrDefault();

                        if (PurchaseOrderMaster != null && PurchaseOrderMaster.Gr_Process == false && people != null && (PurchaseOrderMaster.Status == "Self Approved" || PurchaseOrderMaster.Status == "Approved" || PurchaseOrderMaster.Status == "Partial Received" || PurchaseOrderMaster.Status == "UN Received" || PurchaseOrderMaster.Status == "CN Partial Received" || PurchaseOrderMaster.Status == "UN Partial Received") && (PurchaseOrderMaster.Status != "Auto Closed" || PurchaseOrderMaster.Status != "Closed"))
                        {

                            PurchaseOrderMaster.Gr_Process = true;
                            context.Entry(PurchaseOrderMaster).State = EntityState.Modified;
                            context.Commit();

                            var ItemNumber = PurchaseOrderMaster.PurchaseOrderDetail.Select(x => x.ItemNumber).ToList();
                            var wId = PurchaseOrderMaster.PurchaseOrderDetail.Select(x => x.WarehouseId).ToList();
                            var qrList = context.GrQualityConfigurations.Where(q => ItemNumber.Contains(q.ItemNumber) && wId.Contains(q.WarehouseId) && q.IsReportUpload == true && q.IsActive == true && q.IsDeleted == false).Select(a => a.GrQualityCheckerId).ToList();
                            List<string> QaPeople = context.Peoples.Where(q => qrList.Contains(q.PeopleID)).Select(a => a.DisplayName).Distinct().ToList();
                            var QualityCheckerName = string.Join(",", QaPeople);

                            // get all previous done gr for this purchaseOrderID
                            var oldGr = context.GoodsReceivedDetail.Where(x => x.PurchaseOrderDetail.PurchaseOrderId == PurchaseOrderMaster.PurchaseOrderId && x.IsDeleted == false).Include(x => x.PurchaseOrderDetail).ToList();
                            var PurchaseOrderDeatil = PurchaseOrderMaster.PurchaseOrderDetail.Where(x => x.PurchaseOrderId == PoId).ToList();
                            if (AddPOGRDetailobj.PurchaseOrderDetailDc != null && AddPOGRDetailobj.PurchaseOrderDetailDc.Any(c => c.IsFreeItem == true))
                            {
                                var FreeitemList = AddPOGRDetailobj.PurchaseOrderDetailDc.Where(c => c.IsFreeItem == true && c.PurchaseOrderDetailId == 0).ToList();
                                if (oldGr.Count == 0 && FreeitemList != null)
                                {
                                    if (!AddPOGRDetailobj.PurchaseOrderDetailDc.Any(c => c.IsFreeItem == false))
                                    {
                                        resultMessage = "you can't post free articles without main article ";
                                        IsValidate = false;
                                        var ress = new
                                        {
                                            POGRDetail = AddPOGRDetailobj,
                                            status = IsValidate,
                                            Message = resultMessage

                                        };
                                        return Request.CreateResponse(HttpStatusCode.OK, ress);
                                    }

                                }
                                var multiMrpIds = FreeitemList.Select(x => x.ItemMultiMRPId).ToList();
                                var ItemsForFreeList = context.itemMasters.Where(z => multiMrpIds.Contains(z.ItemMultiMRPId) && z.WarehouseId == PurchaseOrderMaster.WarehouseId).Distinct().ToList();
                                if (FreeitemList != null && FreeitemList.Any())
                                {

                                    List<classification> ABCClassification = new List<classification>();
                                    var Categories = ItemsForFreeList.Select(x => new classification { itemNumber = x.ItemNumber, warehouseid = x.WarehouseId }).ToList();

                                    foreach (var cat in Categories)
                                    {
                                        var query = "select itemNumber,warehouseid,Category,Qty from ItemsClassification Where itemNumber = '" + cat.itemNumber + "' and warehouseid = " + cat.warehouseid;
                                        var ABCClass = context.Database.SqlQuery<classification>(query).ToList();
                                        ABCClassification.AddRange(ABCClass);
                                    }

                                    foreach (var fitem in FreeitemList)
                                    {
                                        var ItemsForFree = ItemsForFreeList.Where(x => x.ItemMultiMRPId == fitem.ItemMultiMRPId).FirstOrDefault();
                                        PurchaseOrderDetail pd = new PurchaseOrderDetail();
                                        if (ItemsForFreeList.Any(x => x.ItemMultiMRPId == fitem.ItemMultiMRPId))
                                        {
                                            if (PurchaseOrderDeatil.Any(x => x.ItemMultiMRPId == fitem.ItemMultiMRPId && x.Price == 0.01))
                                            {
                                                //update existing free stock qty in purchase order details
                                                pd = PurchaseOrderDeatil.FirstOrDefault(x => x.ItemMultiMRPId == fitem.ItemMultiMRPId && x.Price == 0.01);
                                                if (pd != null)
                                                {
                                                    var OldFreeqty = oldGr.Where(x => x.ItemMultiMRPId == fitem.ItemMultiMRPId && x.IsFreeItem == true && x.Status == 2).ToList();
                                                    var remainingFreeqty = 0;
                                                    if (OldFreeqty != null)
                                                    {
                                                        remainingFreeqty = pd.TotalQuantity - OldFreeqty.Sum(x => x.Qty);
                                                    }
                                                    if (pd.TotalQuantity != fitem.TotalQuantity && OldFreeqty == null)
                                                    {
                                                        pd.TotalQuantity = fitem.TotalQuantity;
                                                        pd.PurchaseQty = pd.TotalQuantity;
                                                        pd.ConversionFactor = pd.ConversionFactor;
                                                        context.Entry(pd).State = EntityState.Modified;
                                                    }
                                                    else if (remainingFreeqty < fitem.ReceivingQty && OldFreeqty != null)
                                                    {
                                                        pd.TotalQuantity += fitem.ReceivingQty;
                                                        pd.PurchaseQty = pd.TotalQuantity;
                                                        pd.ConversionFactor = pd.ConversionFactor;
                                                        context.Entry(pd).State = EntityState.Modified;
                                                    }

                                                }
                                            }
                                            else
                                            {
                                                pd.ItemId = ItemsForFree.ItemId;
                                                pd.PurchaseOrderId = PurchaseOrderMaster.PurchaseOrderId;
                                                pd.ItemNumber = ItemsForFree.Number;
                                                pd.itemBaseName = ItemsForFree.itemBaseName;
                                                pd.ItemMultiMRPId = ItemsForFree.ItemMultiMRPId;
                                                pd.HSNCode = ItemsForFree.HSNCode;
                                                pd.MRP = ItemsForFree.price;
                                                pd.SellingSku = ItemsForFree.SellingSku;
                                                pd.ItemName = ItemsForFree.itemname;
                                                pd.PurchaseQty = fitem.ReceivingQty;
                                                pd.CreationDate = indianTime;
                                                pd.Status = "ordered";
                                                pd.MOQ = fitem.ReceivingQty;
                                                pd.Price = 0.01;
                                                pd.WarehouseId = ItemsForFree.WarehouseId;
                                                pd.CompanyId = ItemsForFree.CompanyId;
                                                pd.WarehouseName = ItemsForFree.WarehouseName;
                                                pd.SupplierId = PurchaseOrderMaster.SupplierId;
                                                pd.SupplierName = PurchaseOrderMaster.SupplierName;
                                                pd.TotalQuantity = Convert.ToInt32(pd.PurchaseQty);
                                                pd.PurchaseName = ItemsForFree.PurchaseUnitName;
                                                pd.PurchaseSku = ItemsForFree.PurchaseSku;
                                                pd.DepoId = PurchaseOrderMaster.DepoId;
                                                pd.DepoName = PurchaseOrderMaster.DepoName;
                                                pd.ConversionFactor = fitem.ReceivingQty;
                                                pd.Category = ABCClassification.Where(x => x.itemNumber == ItemsForFree.Number).Select(x => x.Category).FirstOrDefault() != null ? ABCClassification.Where(x => x.itemNumber == ItemsForFree.Number).Select(x => x.Category).FirstOrDefault() : "D";
                                                PurchaseOrderMaster.PurchaseOrderDetail.Add(pd);
                                            }
                                        }
                                        if (context.Commit() > 0)
                                        {

                                        }
                                        if (pd.PurchaseOrderDetailId > 0)
                                        {
                                            if (fitem.PurchaseOrderDetailId == 0)
                                            {
                                                fitem.PurchaseOrderDetailId = pd.PurchaseOrderDetailId;
                                            }
                                        }
                                    }
                                    PurchaseOrderDeatil = context.DPurchaseOrderDeatil.Where(x => x.PurchaseOrderId == PoId).ToList();
                                }
                            }



                            if (oldGr != null && oldGr.Any())
                            {
                                int TotalPOQty = PurchaseOrderDeatil.Sum(x => x.TotalQuantity);
                                int? TotalPORecQty = AddPOGRDetailobj.PurchaseOrderDetailDc.Sum(x => x.ReceivingQty + x.DamageQty + x.ExpiryQty + x.ShortQty);
                                if (TotalPOQty < TotalPORecQty)
                                {
                                    resultMessage = "Receive Quantity should be Less then Total Quantity ";
                                    IsValidateQTy = false;
                                }
                                else
                                {
                                    IsValidateQTy = true;
                                }
                            }
                            else
                            {
                                int TotalPOQty = PurchaseOrderDeatil.Sum(x => x.TotalQuantity);
                                int? POMTotalPORecQty = AddPOGRDetailobj.PurchaseOrderDetailDc.Sum(x => x.ReceivingQty + x.DamageQty + x.ExpiryQty + x.ShortQty);
                                int? TotalPORecQty = oldGr.Any() ? oldGr.Sum(p => p.Qty) : 0;
                                if (TotalPOQty < (TotalPORecQty + POMTotalPORecQty))
                                {
                                    resultMessage = "Receive Quantity should be Less then Total Quantity ";
                                    IsValidateQTy = false;
                                }
                                else
                                {
                                    IsValidateQTy = true;
                                }
                            }
                            foreach (var a in AddPOGRDetailobj.PurchaseOrderDetailDc)
                            {
                                if (oldGr.Count == 0)
                                {
                                    var qty = PurchaseOrderDeatil.Where(x => x.PurchaseOrderDetailId == a.PurchaseOrderDetailId).SingleOrDefault();
                                    if (qty.TotalQuantity >= (a.ReceivingQty + a.DamageQty + a.ExpiryQty + a.ShortQty))
                                    {
                                        IsValidate = true;
                                    }
                                    else
                                    {
                                        resultMessage = "Receive Quantity should be Less then Total Quantity : " + a.ItemName;
                                        IsValidate = false;
                                        break;
                                    }
                                }
                                else
                                {
                                    int RecGrqty = oldGr.Any(p => p.PurchaseOrderDetailId == a.PurchaseOrderDetailId) ? oldGr.Where(p => p.PurchaseOrderDetailId == a.PurchaseOrderDetailId).Sum(p => p.Qty) : 0;
                                    int IsRecGrqty = oldGr.Any(p => p.PurchaseOrderDetailId == a.PurchaseOrderDetailId) ? oldGr.Where(p => p.PurchaseOrderDetailId == a.PurchaseOrderDetailId && p.IsDamageExpiryPhysical == true).Sum(p => p.DamageQty + p.ExpiryQty + p.ShortQty) : 0;
                                    RecGrqty += IsRecGrqty;


                                    int Poqty = PurchaseOrderDeatil.Where(p => p.PurchaseOrderDetailId == a.PurchaseOrderDetailId).Select(p => p.TotalQuantity).FirstOrDefault();
                                    if (Poqty >= (a.ReceivingQty + a.DamageQty + a.ExpiryQty + a.ShortQty) && Poqty >= RecGrqty)
                                    {
                                        IsValidate = true;
                                    }
                                    else
                                    {
                                        resultMessage = "Receive Quantity should be Less then Total Quantity : " + a.ItemName;
                                        IsValidate = false;
                                        break;
                                    }
                                }
                                #region Insert BatchCode
                                foreach (var InsertGRNBatche in a.GRNBatchDcs)
                                {
                                    var batch = InsertAndGetBatch(context, InsertGRNBatche, people.PeopleID);
                                    InsertGRNBatche.BatchMasterId = batch.Id;

                                }

                                #endregion


                            }
                            if (IsValidateQTy && IsValidate)
                            {
                                bool result = GRNRequest(PurchaseOrderMaster, AddPOGRDetailobj, oldGr, people, context, dbContextTransaction);
                                if (result)
                                {
                                    if (QaPeople.Any() && QaPeople.Count > 0)
                                    {
                                        resultMessage = "GR Posted Succesfully, This GR contains items that require Quality Aoproval." +
                                            "GR will be sent to " + QualityCheckerName +
                                            " first then to the GR Checker.";
                                    }
                                    else
                                    {
                                        resultMessage = "GR Posted Succesfully, Wait till Approve.";
                                    }

                                    IsValidate = true;
                                    #region Add GDN

                                    bool IsWHActive = context.Warehouses.Any(x => x.WarehouseId == PurchaseOrderMaster.WarehouseId &&
                                                      x.IsKPP == false && x.active == true && x.Deleted == false && x.IsGDNAllow == true);
                                    if (IsWHActive)
                                    {
                                        GDNHelper gDNHelper = new GDNHelper();
                                        bool gdn = gDNHelper.AddGDN(AddPOGRDetailobj, context);
                                    }
                                    #endregion
                                    dbContextTransaction.Complete();
                                }
                                else
                                {
                                    dbContextTransaction.Dispose();
                                    resultMessage = "Something went wrong";
                                    IsValidate = false;
                                }
                            }
                        }
                        else
                        {
                            resultMessage = "You can't do GR Now, Because Currently it is under status :" + PurchaseOrderMaster.Status;
                            IsValidate = false;
                        }
                    }
                    else
                    {
                        resultMessage = "Post list is null Or GrSerialNumber is 0 ";
                        IsValidate = false;
                    }
                }
            }

            var res = new
            {
                POGRDetail = AddPOGRDetailobj,
                status = IsValidate,
                Message = resultMessage

            };
            return Request.CreateResponse(HttpStatusCode.OK, res);

        }

        public bool GRNRequest(PurchaseOrderMaster PurchaseOrderMaster, AddPOGRDetailDc AddPOGRDetailobj, List<GoodsReceivedDetail> PurchaseGoodsReceivedDetail, People people, AuthContext context, TransactionScope dbContextTransaction)
        {
            int warehouseid = PurchaseOrderMaster.WarehouseId;
            int GrSerialNumber = AddPOGRDetailobj.GrSerialNumber;
            string VehicleNumber = AddPOGRDetailobj.VehicleNumber;
            string VehicleType = AddPOGRDetailobj.VehicleType;
            List<GoodsReceivedDetail> PostHitDetail = new List<GoodsReceivedDetail>();//addd 
            List<GoodsReceivedDetail> AddGoodsReceivedDetail = new List<GoodsReceivedDetail>();//addd 
            List<GoodsReceivedDetail> UpdateGoodsReceivedDetail = new List<GoodsReceivedDetail>();//addd 
            //List<GrQualityInvoice> GrQualityInvoices = new List<GrQualityInvoice>();//addd 
            var itemnumbers = AddPOGRDetailobj.PurchaseOrderDetailDc.Select(x => x.ItemNumber).ToList();
            var GrQualityConfiglist = context.GrQualityConfigurations.Where(x => itemnumbers.Contains(x.ItemNumber) && x.WarehouseId == warehouseid && x.IsActive == true && x.IsDeleted == false).ToList();

            foreach (var addGrItem in AddPOGRDetailobj.PurchaseOrderDetailDc)
            {
                GoodsReceivedDetail gd = new GoodsReceivedDetail();
                int QtyReciv = addGrItem.GRNBatchDcs != null && addGrItem.GRNBatchDcs.Any() ? addGrItem.GRNBatchDcs.Sum(x => x.Qty) : addGrItem.ReceivingQty;        //addGrItem.ReceivingQty;// - addGrItem.ShortQty;
                int DamQtyReciv = addGrItem.GRNBatchDcs != null && addGrItem.GRNBatchDcs.Any() ? addGrItem.GRNBatchDcs.Sum(x => x.DamageQty) : addGrItem.DamageQty;
                int ExpQtyReciv = addGrItem.GRNBatchDcs != null && addGrItem.GRNBatchDcs.Any() ? addGrItem.GRNBatchDcs.Sum(x => x.ExpiryQty) : addGrItem.ExpiryQty;
                int ShortQty = addGrItem.ShortQty;

                if (addGrItem.IsFreeItem == true && QtyReciv == 0)
                {
                }
                else
                {
                    if (PurchaseGoodsReceivedDetail.Any(x => x.ItemMultiMRPId == addGrItem.ItemMultiMRPId && x.PurchaseOrderDetailId == addGrItem.PurchaseOrderDetailId && x.GrSerialNumber == addGrItem.GrSerialNumber))
                    {
                        gd = PurchaseGoodsReceivedDetail.Where(x => x.ItemMultiMRPId == addGrItem.ItemMultiMRPId && x.PurchaseOrderDetailId == addGrItem.PurchaseOrderDetailId && x.GrSerialNumber == addGrItem.GrSerialNumber).FirstOrDefault();
                        gd.Qty = QtyReciv;
                        gd.DamageQty = DamQtyReciv;
                        gd.ExpiryQty = ExpQtyReciv;
                        gd.ShortQty = ShortQty;//
                        gd.PurchaseOrderDetailId = addGrItem.PurchaseOrderDetailId;
                        gd.ItemMultiMRPId = addGrItem.ItemMultiMRPId;
                        gd.IsFreeItem = addGrItem.IsFreeItem;
                        gd.GrSerialNumber = addGrItem.GrSerialNumber;
                        if (addGrItem.Price > 0)
                        { gd.Price = addGrItem.Price; }
                        else { gd.Price = 0.01; }
                        gd.Status = 1;// 1= Pending for Checker Side, 2=Approved , 3=Reject
                        gd.BatchNo = addGrItem.BatchNo;
                        //gd.MFGDate = addGrItem.MFGDate;
                        gd.Barcode = addGrItem.Barcode;
                        gd.VehicleType = VehicleType;
                        gd.VehicleNumber = VehicleNumber;
                        gd.CreatedBy = people.PeopleID;
                        gd.CreatedDate = indianTime;
                        gd.GeneratedDate = indianTime;
                        gd.ModifiedDate = indianTime;
                        gd.ApprovedBy = 0;
                        gd.IsDamageExpiryPhysical = true;
                        if (addGrItem.weight > 0)
                        {
                            gd.weight = addGrItem.weight;
                        }

                        #region Batch code
                        gd.GRBatchs = new List<GRBatch>();
                        if (addGrItem.GRNBatchDcs != null && addGrItem.GRNBatchDcs.Any())
                        {
                            foreach (var bgrn in addGrItem.GRNBatchDcs.GroupBy(x => x.BatchCode))
                            {
                                gd.GRBatchs.Add(
                               new GRBatch
                               {
                                   Qty = bgrn.Sum(y => y.Qty),
                                   DamageQty = bgrn.Sum(y => y.DamageQty),
                                   ExpiryQty = bgrn.Sum(y => y.ExpiryQty),
                                   CreatedDate = indianTime,
                                   CreatedBy = people.PeopleID,
                                   BatchMasterId = bgrn.FirstOrDefault().BatchMasterId ?? 0,
                                   IsActive = true,
                                   IsDeleted = false,
                               });
                            }
                        }
                        #endregion

                        //GrQualityInvoice qa = new GrQualityInvoice();
                        gd.GrQualityInvoices = new List<GrQualityInvoice>();
                        if (addGrItem.IsQualityItemTesting == true && addGrItem.IsQualityReportUpload == true)
                        {
                            foreach (var gr in GrQualityConfiglist)
                            {
                                if (gr.ItemNumber == addGrItem.ItemNumber)
                                {
                                    gd.GrQualityInvoices.Add(
                                       new GrQualityInvoice
                                       {
                                           GrQualityCheckerId = gr.GrQualityCheckerId,
                                           Image = addGrItem.Image,
                                           IsApproved = 0,
                                           IsActive = true,
                                           IsDeleted = false,
                                           Isview = false,
                                           CreatedDate = DateTime.Now,
                                           CreatedBy = people.PeopleID
                                       });

                                }
                            }
                        }
                        //GrQualityInvoices.Add(qa);

                        UpdateGoodsReceivedDetail.Add(gd);
                        PostHitDetail.Add(gd);
                    }
                    else
                    {
                        gd.PurchaseOrderDetailId = addGrItem.PurchaseOrderDetailId;
                        gd.ItemMultiMRPId = addGrItem.ItemMultiMRPId;
                        gd.IsActive = true;
                        gd.IsDeleted = false;
                        gd.Qty = QtyReciv;
                        gd.DamageQty = DamQtyReciv;
                        gd.ExpiryQty = ExpQtyReciv;
                        gd.ShortQty = ShortQty;
                        gd.IsFreeItem = addGrItem.IsFreeItem;
                        gd.GrSerialNumber = addGrItem.GrSerialNumber;
                        if (addGrItem.Price > 0)
                        { gd.Price = addGrItem.Price; }
                        else { gd.Price = 0.01; }
                        gd.Status = 1;// 1= Pending for Checker Side, 2=Approved , 3=Reject
                        gd.BatchNo = addGrItem.BatchNo;
                        gd.MFGDate = addGrItem.MFGDate;
                        gd.Barcode = addGrItem.Barcode;
                        gd.VehicleType = VehicleType;
                        gd.VehicleNumber = VehicleNumber;
                        gd.CreatedBy = people.PeopleID;
                        gd.CreatedDate = indianTime;
                        gd.GeneratedDate = indianTime;
                        gd.ModifiedDate = indianTime;
                        gd.ApprovedBy = 0;
                        if (gd.DamageQty > 0 || gd.ExpiryQty > 0)
                        {
                            gd.IsDamageExpiryPhysical = true;
                        }
                        if (addGrItem.weight > 0)
                        {
                            gd.weight = addGrItem.weight;

                        }

                        #region Batch code
                        gd.GRBatchs = new List<GRBatch>();
                        if (addGrItem.GRNBatchDcs != null && addGrItem.GRNBatchDcs.Any())
                        {

                            foreach (var bgrn in addGrItem.GRNBatchDcs.GroupBy(x => x.BatchCode))
                            {
                                gd.GRBatchs.Add(
                               new GRBatch
                               {
                                   Qty = bgrn.Sum(y => y.Qty),
                                   DamageQty = bgrn.Sum(y => y.DamageQty),
                                   ExpiryQty = bgrn.Sum(y => y.ExpiryQty),
                                   CreatedDate = indianTime,
                                   CreatedBy = people.PeopleID,
                                   BatchMasterId = bgrn.FirstOrDefault().BatchMasterId ?? 0,
                                   IsActive = true,
                                   IsDeleted = false,
                               });
                            }
                        }
                        #endregion
                        gd.GrQualityInvoices = new List<GrQualityInvoice>();
                        //GrQualityInvoice qa = new GrQualityInvoice();
                        if (addGrItem.IsQualityItemTesting == true && addGrItem.IsQualityReportUpload == true)
                        {
                            foreach (var gr in GrQualityConfiglist)
                            {
                                if (gr.ItemNumber == addGrItem.ItemNumber)
                                {
                                    gd.GrQualityInvoices.Add(
                                    new GrQualityInvoice
                                    {
                                        Image = addGrItem.Image,
                                        GrQualityCheckerId = gr.GrQualityCheckerId,
                                        IsApproved = 0,
                                        IsActive = true,
                                        IsDeleted = false,
                                        Isview = false,
                                        CreatedDate = DateTime.Now,
                                        CreatedBy = people.PeopleID
                                    });
                                }
                            }
                        }
                        //GrQualityInvoices.Add(qa);

                        AddGoodsReceivedDetail.Add(gd);

                        PostHitDetail.Add(gd);
                    }
                }
            }
            int DeletedQty = 0;
            #region Not Posted Data From App here we update there status
            var NotEdited = PurchaseGoodsReceivedDetail.Where(ds => ds.GrSerialNumber == GrSerialNumber && ds.Status == 3 && !AddPOGRDetailobj.PurchaseOrderDetailDc.Any(db => db.ItemMultiMRPId == ds.ItemMultiMRPId && db.PurchaseOrderDetailId == ds.PurchaseOrderDetailId)).ToList();
            if (NotEdited != null && NotEdited.Any())
            {
                //foreach (var item in NotEdited)
                //{
                //    if (AddPOGRDetailobj.PurchaseOrderDetailDc.Any(db => db.PurchaseOrderDetailId == item.PurchaseOrderDetailId))
                //    {
                //        DeletedQty += item.Qty;

                //        item.IsDeleted = true;
                //        item.ModifiedDate = indianTime;
                //        item.CreatedDate = indianTime;
                //        UpdateGoodsReceivedDetail.Add(item);
                //        PostHitDetail.Add(item);
                //    }
                //    else
                //    {
                //        item.Status = 1;// we chnage status in reject to Pending for Checker Side	
                //        item.VehicleNumber = VehicleNumber;
                //        item.VehicleType = VehicleType;
                //        item.ModifiedDate = indianTime;
                //        item.CreatedDate = indianTime;
                //        UpdateGoodsReceivedDetail.Add(item);
                //        PostHitDetail.Add(item);
                //    }
                //}
            }
            #endregion


            int TotalPOQty = PurchaseOrderMaster.PurchaseOrderDetail.Sum(x => x.TotalQuantity);
            int? TotalPORecQty = 0;
            TotalPORecQty = AddGoodsReceivedDetail.Any() ? AddGoodsReceivedDetail.Sum(p => p.Qty + p.DamageQty + p.ExpiryQty + p.ShortQty) : 0;
            if (PurchaseGoodsReceivedDetail.Any())
            {
                TotalPORecQty += PurchaseGoodsReceivedDetail.Any() ? PurchaseGoodsReceivedDetail.Sum(p => p.Qty + p.DamageQty + p.ExpiryQty + p.ShortQty) : 0;
                TotalPORecQty -= DeletedQty;
            }
            int percentComplete = (int)Math.Round((double)(100 * TotalPORecQty) / TotalPOQty);
            PurchaseOrderMaster.progress = Convert.ToString(percentComplete);
            if (TotalPOQty > TotalPORecQty)
            {
                PurchaseOrderMaster.Status = "UN Partial Received";
            }
            else
            {
                if (TotalPOQty >= TotalPORecQty)
                {
                    PurchaseOrderMaster.Status = "UN Received";
                }
                else { return false; }
            }

            if (AddGoodsReceivedDetail != null && AddGoodsReceivedDetail.Any())
            {
                context.GoodsReceivedDetail.AddRange(AddGoodsReceivedDetail);
            }
            if (UpdateGoodsReceivedDetail != null && UpdateGoodsReceivedDetail.Any())
            {
                foreach (var UpdateGoodsDetail in UpdateGoodsReceivedDetail)
                {
                    context.Entry(UpdateGoodsDetail).State = EntityState.Modified;
                }
            }
            context.Entry(PurchaseOrderMaster).State = EntityState.Modified;
            if (context.Commit() > 0)
            {
                #region InReceivedStocks Hit

                var Stockcombined = AddGoodsReceivedDetail.Concat(UpdateGoodsReceivedDetail);
                //for InReceivedStocks
                MultiStockHelper<OnGRNRequestCStockEntryDc> MultiStockHelpers = new MultiStockHelper<OnGRNRequestCStockEntryDc>();
                List<OnGRNRequestCStockEntryDc> OnGRNRequestCStockList = new List<OnGRNRequestCStockEntryDc>();
                foreach (var StockHit in PostHitDetail.Where(x => (x.Qty > 0 || x.DamageQty > 0 || x.ExpiryQty > 0 || x.ShortQty > 0)))
                {
                    var GoodReceivedDetailsId = Stockcombined.Where(x => x.ItemMultiMRPId == StockHit.ItemMultiMRPId && x.GrSerialNumber == GrSerialNumber && x.PurchaseOrderDetailId == StockHit.PurchaseOrderDetailId).Select(x => x.Id).FirstOrDefault();
                    OnGRNRequestCStockList.Add(new OnGRNRequestCStockEntryDc
                    {
                        ItemMultiMRPId = StockHit.ItemMultiMRPId,
                        GoodReceivedDetailsId = GoodReceivedDetailsId,
                        PurchaseOrderID = PurchaseOrderMaster.PurchaseOrderId,
                        Qty = StockHit.Qty,
                        UserId = people.PeopleID,
                        WarehouseId = PurchaseOrderMaster.WarehouseId,
                        IsFreeStock = StockHit.IsFreeItem,
                        DamageQty = StockHit.DamageQty,
                        ExpiryQty = StockHit.ExpiryQty,
                        ShortQty = StockHit.ShortQty

                    });
                }
                if (OnGRNRequestCStockList.Any())
                {

                    bool res = MultiStockHelpers.MakeEntry(OnGRNRequestCStockList, "Stock_OnGRNRequest", context, dbContextTransaction);
                    if (!res)
                    {
                        return false;
                    }
                }
                #endregion
                return true;
            }
            else
            {
                return false;
            }
        }


        public BatchMaster InsertAndGetBatch(AuthContext context, GRNBatchDc InsertGRNBatche, int CreatedBy)
        {
            //var expiryDate = (InsertGRNBatche.BestBeforeDay > 0) ? indianTime.AddDays(InsertGRNBatche.BestBeforeDay) : InsertGRNBatche.ExpiryDate.Value;
            var MFGDate = InsertGRNBatche.MFGDate;
            InsertGRNBatche.BatchCode = MFGDate.ToString("dd-MM-yyyy");
            var batchitem = context.BatchMasters.FirstOrDefault(x => x.BatchCode == InsertGRNBatche.BatchCode && x.ItemNumber == InsertGRNBatche.ItemNumber);
            if (batchitem == null)
            {
                BatchMaster addGrnB = new BatchMaster();
                addGrnB.ItemNumber = InsertGRNBatche.ItemNumber;
                addGrnB.BatchCode = InsertGRNBatche.BatchCode;
                addGrnB.MFGDate = InsertGRNBatche.MFGDate;
                addGrnB.ExpiryDate = (InsertGRNBatche.BestBeforeDay > 0) ? indianTime.AddDays(InsertGRNBatche.BestBeforeDay) : InsertGRNBatche.ExpiryDate;
                addGrnB.CreatedDate = indianTime;
                addGrnB.CreatedBy = CreatedBy;
                addGrnB.IsActive = true;
                addGrnB.IsDeleted = false;
                context.BatchMasters.Add(addGrnB);
                context.Commit();
                return addGrnB;

            }
            return batchitem;
        }



        //#region old code  Addd in Temporary 

        ///// <summary>
        ///// Temporary  GR before approved
        ///// created on 05/02/2020
        ///// </summary>
        ///// <param name="pom"></param>
        ///// <returns></returns>
        //[Route("AddPOGRDetails/V1")]
        //[HttpPost]
        //[AllowAnonymous]
        //public HttpResponseMessage AddPOGRDetailsV1(AddPOGRDetailDc AddPOGRDetailobj)
        //{
        //    bool IsValidate = false;
        //    bool IsValidateQTy = false;
        //    string resultMessage = "";
        //    using (AuthContext context = new AuthContext())
        //    {
        //        using (var dbContextTransaction = context.Database.BeginTransaction())
        //        {
        //            if (AddPOGRDetailobj.PurchaseOrderDetailDc != null && AddPOGRDetailobj.PurchaseOrderDetailDc.Any() && AddPOGRDetailobj.GrSerialNumber > 0)
        //            {
        //                int peopleId = AddPOGRDetailobj.UserId;
        //                People people = context.Peoples.Where(q => q.PeopleID == peopleId && q.Active == true).SingleOrDefault();
        //                var PoId = AddPOGRDetailobj.PurchaseOrderDetailDc[0].PurchaseOrderId;//poid
        //                var PurchaseOrderMaster = context.DPurchaseOrderMaster.Where(x => x.PurchaseOrderId == PoId).SingleOrDefault();

        //                if (PurchaseOrderMaster != null && people != null && (PurchaseOrderMaster.Status == "Self Approved" || PurchaseOrderMaster.Status == "Approved" || PurchaseOrderMaster.Status == "Partial Received" || PurchaseOrderMaster.Status == "UN Received" || PurchaseOrderMaster.Status == "CN Partial Received" || PurchaseOrderMaster.Status == "UN Partial Received"))
        //                {
        //                    var PurchaseOrderDeatil = context.DPurchaseOrderDeatil.Where(x => x.PurchaseOrderId == PoId).ToList();
        //                    if (AddPOGRDetailobj.PurchaseOrderDetailDc != null && AddPOGRDetailobj.PurchaseOrderDetailDc.Any(c => c.IsFreeItem == true))
        //                    {

        //                        var FreeitemList = AddPOGRDetailobj.PurchaseOrderDetailDc.Where(c => c.IsFreeItem == true && c.PurchaseOrderDetailId == 0).ToList();
        //                        var multiMrpIds = FreeitemList.Select(x => x.ItemMultiMRPId).ToList();
        //                        var ItemsForFreeList = context.itemMasters.Where(z => multiMrpIds.Contains(z.ItemMultiMRPId) && z.WarehouseId == PurchaseOrderMaster.WarehouseId).Distinct().ToList();
        //                        if (FreeitemList != null && FreeitemList.Any())
        //                        {
        //                            foreach (var fitem in FreeitemList)
        //                            {
        //                                var ItemsForFree = ItemsForFreeList.Where(x => x.ItemMultiMRPId == fitem.ItemMultiMRPId).FirstOrDefault();

        //                                PurchaseOrderDetail pd = new PurchaseOrderDetail();
        //                                if (ItemsForFreeList.Any(x => x.ItemMultiMRPId == fitem.ItemMultiMRPId))
        //                                {
        //                                    if (PurchaseOrderDeatil.Any(x => x.ItemMultiMRPId == fitem.ItemMultiMRPId && x.Price == 0.01))
        //                                    {
        //                                        //update existing free stock qty in purchase order details
        //                                        pd = PurchaseOrderDeatil.FirstOrDefault(x => x.ItemMultiMRPId == fitem.ItemMultiMRPId && x.Price == 0.01);
        //                                        if (pd != null && pd.TotalQuantity < fitem.TotalQuantity)
        //                                        {
        //                                            pd.TotalQuantity = fitem.TotalQuantity;
        //                                            pd.PurchaseQty = pd.TotalQuantity;
        //                                            pd.ConversionFactor = pd.ConversionFactor;
        //                                            context.Entry(pd).State = EntityState.Modified;
        //                                        }
        //                                    }
        //                                    else
        //                                    {
        //                                        //Add new item in purchase order details
        //                                        pd.ItemId = ItemsForFree.ItemId;
        //                                        pd.PurchaseOrderId = PurchaseOrderMaster.PurchaseOrderId;
        //                                        pd.ItemNumber = ItemsForFree.Number;
        //                                        pd.itemBaseName = ItemsForFree.itemBaseName;
        //                                        pd.ItemMultiMRPId = ItemsForFree.ItemMultiMRPId;
        //                                        pd.HSNCode = ItemsForFree.HSNCode;
        //                                        pd.MRP = ItemsForFree.price;
        //                                        pd.SellingSku = ItemsForFree.SellingSku;
        //                                        pd.ItemName = ItemsForFree.itemname;
        //                                        pd.PurchaseQty = fitem.ReceivingQty;
        //                                        pd.CreationDate = indianTime;
        //                                        pd.Status = "ordered";
        //                                        pd.MOQ = fitem.ReceivingQty;
        //                                        pd.Price = 0.01;
        //                                        pd.WarehouseId = ItemsForFree.WarehouseId;
        //                                        pd.CompanyId = ItemsForFree.CompanyId;
        //                                        pd.WarehouseName = ItemsForFree.WarehouseName;
        //                                        pd.SupplierId = PurchaseOrderMaster.SupplierId;
        //                                        pd.SupplierName = PurchaseOrderMaster.SupplierName;
        //                                        pd.TotalQuantity = Convert.ToInt32(pd.PurchaseQty);
        //                                        pd.PurchaseName = ItemsForFree.PurchaseUnitName;
        //                                        pd.PurchaseSku = ItemsForFree.PurchaseSku;
        //                                        pd.DepoId = PurchaseOrderMaster.DepoId;
        //                                        pd.DepoName = PurchaseOrderMaster.DepoName;
        //                                        pd.ConversionFactor = fitem.ReceivingQty;
        //                                        PurchaseOrderMaster.PurchaseOrderDetail.Add(pd);
        //                                    }
        //                                }
        //                                if (context.Commit() > 0)
        //                                {


        //                                }
        //                                if (pd.PurchaseOrderDetailId > 0)
        //                                {
        //                                    if (fitem.PurchaseOrderDetailId == 0)
        //                                    {
        //                                        fitem.PurchaseOrderDetailId = pd.PurchaseOrderDetailId;
        //                                    }
        //                                }
        //                            }

        //                            PurchaseOrderDeatil = context.DPurchaseOrderDeatil.Where(x => x.PurchaseOrderId == PoId).ToList();
        //                        }
        //                    }
        //                    var Gr = context.GoodsReceivedDetail.Where(x => x.PurchaseOrderDetail.PurchaseOrderId == PurchaseOrderMaster.PurchaseOrderId && x.IsDeleted == false).Include(x => x.PurchaseOrderDetail).ToList();
        //                    if (Gr != null && Gr.Any())
        //                    {
        //                        int TotalPOQty = PurchaseOrderDeatil.Sum(x => x.TotalQuantity);
        //                        int? TotalPORecQty = AddPOGRDetailobj.PurchaseOrderDetailDc.Sum(x => x.ReceivingQty);
        //                        if (TotalPOQty < TotalPORecQty)
        //                        {
        //                            resultMessage = "Receive Quantity should be Less then Total Quantity ";
        //                            IsValidateQTy = false;
        //                        }
        //                        else
        //                        {
        //                            IsValidateQTy = true;
        //                        }
        //                    }
        //                    else
        //                    {
        //                        int TotalPOQty = PurchaseOrderDeatil.Sum(x => x.TotalQuantity);
        //                        int? POMTotalPORecQty = AddPOGRDetailobj.PurchaseOrderDetailDc.Sum(x => x.ReceivingQty);
        //                        int? TotalPORecQty = Gr.Any() ? Gr.Sum(p => p.Qty) : 0;
        //                        if (TotalPOQty < (TotalPORecQty + POMTotalPORecQty))
        //                        {
        //                            resultMessage = "Receive Quantity should be Less then Total Quantity ";
        //                            IsValidateQTy = false;
        //                        }
        //                        else
        //                        {
        //                            IsValidateQTy = true;
        //                        }
        //                    }
        //                    foreach (var a in AddPOGRDetailobj.PurchaseOrderDetailDc)
        //                    {
        //                        if (Gr.Count == 0)
        //                        {
        //                            var qty = PurchaseOrderDeatil.Where(x => x.PurchaseOrderDetailId == a.PurchaseOrderDetailId).SingleOrDefault();
        //                            if (qty.TotalQuantity >= (a.ReceivingQty))
        //                            {
        //                                IsValidate = true;
        //                            }
        //                            else
        //                            {
        //                                resultMessage = "Receive Quantity should be Less then Total Quantity : " + a.ItemName;
        //                                IsValidate = false;
        //                                break;
        //                            }
        //                        }
        //                        else
        //                        {
        //                            int RecGrqty = Gr.Any(p => p.PurchaseOrderDetailId == a.PurchaseOrderDetailId) ? Gr.Where(p => p.PurchaseOrderDetailId == a.PurchaseOrderDetailId).Sum(p => p.Qty) : 0;
        //                            int Poqty = PurchaseOrderDeatil.Where(p => p.PurchaseOrderDetailId == a.PurchaseOrderDetailId).Select(p => p.TotalQuantity).FirstOrDefault();
        //                            if (Poqty >= a.ReceivingQty && Poqty >= RecGrqty)
        //                            {
        //                                IsValidate = true;
        //                            }
        //                            else
        //                            {
        //                                resultMessage = "Receive Quantity should be Less then Total Quantity : " + a.ItemName;
        //                                IsValidate = false;
        //                                break;
        //                            }
        //                        }
        //                    }
        //                    if (IsValidateQTy && IsValidate)
        //                    {
        //                        var itemPurchaseskus = PurchaseOrderDeatil.Select(x => x.PurchaseSku).ToList();
        //                        var ItemMasterCentralList = context.ItemMasterCentralDB.Where(x => itemPurchaseskus.Contains(x.PurchaseSku) && x.Deleted == false).Select(x => x).ToList(); // ItemMasterCentralList
        //                        var Multimrpids = AddPOGRDetailobj.PurchaseOrderDetailDc.Select(x => x.ItemMultiMRPId).ToList();//
        //                        var ItemMultiMRPList = context.ItemMultiMRPDB.Where(x => Multimrpids.Contains(x.ItemMultiMRPId) && x.Deleted == false).ToList(); // ItemMultiMRPList
        //                        var itemNumberids = PurchaseOrderDeatil.Select(x => x.ItemNumber).ToList();
        //                        var TemporaryCurrentStockList = context.TemporaryCurrentStockDB.Where(x => itemNumberids.Contains(x.ItemNumber) && x.WarehouseId == PurchaseOrderMaster.WarehouseId).ToList();
        //                        bool result = GoodsReceivedInTemp(PurchaseOrderMaster, AddPOGRDetailobj, Gr, ItemMasterCentralList, ItemMultiMRPList, TemporaryCurrentStockList, people, context);
        //                        if (result)
        //                        {

        //                            dbContextTransaction.Commit();
        //                            resultMessage = "GR Posted Succesfully, Wait till Apro";
        //                            IsValidate = true;
        //                        }
        //                        else
        //                        {
        //                            dbContextTransaction.Rollback();
        //                            resultMessage = "Something went wrong";
        //                            IsValidate = false;
        //                        }
        //                    }
        //                }
        //                else
        //                {
        //                    resultMessage = "You can't do GR Now, Because Currently it is under status :" + PurchaseOrderMaster.Status;
        //                    IsValidate = false;
        //                }
        //            }
        //            else
        //            {
        //                resultMessage = "Post list is null Or GrSerialNumber is 0 ";
        //                IsValidate = false;
        //            }
        //        }
        //        var res = new
        //        {
        //            POGRDetail = AddPOGRDetailobj,
        //            status = IsValidate,
        //            Message = resultMessage

        //        };
        //        return Request.CreateResponse(HttpStatusCode.OK, res);
        //    }
        //}

        //public bool GoodsReceivedInTemp(PurchaseOrderMaster PurchaseOrderMaster, AddPOGRDetailDc AddPOGRDetailobj, List<GoodsReceivedDetail> PurchaseOrderDetailRecived, List<ItemMasterCentral> ItemMasterCentralList, List<ItemMultiMRP> ItemMultiMRPList, List<TemporaryCurrentStock> TemporaryCurrentStockList, People people, AuthContext context)
        //{
        //    int warehouseid = PurchaseOrderMaster.WarehouseId;
        //    int GrSerialNumber = AddPOGRDetailobj.GrSerialNumber;
        //    string VehicleNumber = AddPOGRDetailobj.VehicleNumber;
        //    string VehicleType = AddPOGRDetailobj.VehicleType;
        //    List<GoodsReceivedDetail> AddGoodsReceivedDetail = new List<GoodsReceivedDetail>();//addd 
        //    List<GoodsReceivedDetail> UpdateGoodsReceivedDetail = new List<GoodsReceivedDetail>();//addd 
        //    List<TemporaryCurrentStock> UpdateTemporaryCurrentStock = new List<TemporaryCurrentStock>();// temp stock
        //    List<TemporaryCurrentStockHistory> AddTemporaryCurrentStockHistory = new List<TemporaryCurrentStockHistory>();// temp Histpry
        //    foreach (var addGrItem in AddPOGRDetailobj.PurchaseOrderDetailDc)
        //    {
        //        ItemMasterCentral ItemMasterCentral = ItemMasterCentralList.FirstOrDefault(x => x.Number == addGrItem.ItemNumber);
        //        var item = TemporaryCurrentStockList.Where(x => x.ItemNumber == addGrItem.ItemNumber && x.WarehouseId == warehouseid && x.ItemMultiMRPId == addGrItem.ItemMultiMRPId).SingleOrDefault();
        //        if (item == null)
        //        {
        //            var Mitem = ItemMultiMRPList.Where(x => x.ItemNumber == addGrItem.ItemNumber && x.ItemMultiMRPId == addGrItem.ItemMultiMRPId).SingleOrDefault();
        //            if (Mitem != null)
        //            {
        //                TemporaryCurrentStock newCstk = new TemporaryCurrentStock();
        //                newCstk.CompanyId = 1;
        //                newCstk.itemBaseName = Mitem.itemBaseName;
        //                newCstk.ItemNumber = addGrItem.ItemNumber;
        //                newCstk.WarehouseId = PurchaseOrderMaster.WarehouseId;
        //                newCstk.WarehouseName = PurchaseOrderMaster.WarehouseName;
        //                newCstk.CurrentInventory = 0;
        //                newCstk.DamageCurrentInventory = 0;
        //                newCstk.ExpCurrentInventory = 0;
        //                newCstk.CreationDate = indianTime;
        //                newCstk.UpdatedDate = indianTime;
        //                newCstk.MRP = Mitem.MRP;
        //                newCstk.UnitofQuantity = Mitem.UnitofQuantity;
        //                newCstk.UOM = Mitem.UOM;
        //                newCstk.ItemMultiMRPId = Mitem.ItemMultiMRPId;
        //                newCstk.itemname = addGrItem.ItemName;   //itm.itemname;
        //                context.TemporaryCurrentStockDB.Add(newCstk);
        //                context.Commit();
        //                item = newCstk;//if temp stock not there then create stock
        //                TemporaryCurrentStockList.Add(item);
        //            }
        //        }
        //        if (item != null)
        //        {
        //            GoodsReceivedDetail gd = new GoodsReceivedDetail();
        //            int QtyReciv = addGrItem.ReceivingQty;
        //            int DamQtyReciv = addGrItem.DamageQty;
        //            int ExpQtyReciv = addGrItem.ExpiryQty;
        //            if (PurchaseOrderDetailRecived.Any(x => x.ItemMultiMRPId == addGrItem.ItemMultiMRPId && x.PurchaseOrderDetailId == addGrItem.PurchaseOrderDetailId && x.GrSerialNumber == addGrItem.GrSerialNumber))
        //            {
        //                gd = PurchaseOrderDetailRecived.Where(x => x.ItemMultiMRPId == addGrItem.ItemMultiMRPId && x.PurchaseOrderDetailId == addGrItem.PurchaseOrderDetailId && x.GrSerialNumber == addGrItem.GrSerialNumber).FirstOrDefault();
        //                gd.Qty = QtyReciv;
        //                gd.DamageQty = DamQtyReciv;
        //                gd.ExpiryQty = ExpQtyReciv;
        //                gd.PurchaseOrderDetailId = addGrItem.PurchaseOrderDetailId;
        //                gd.ItemMultiMRPId = addGrItem.ItemMultiMRPId;
        //                gd.IsFreeItem = addGrItem.IsFreeItem;
        //                gd.GrSerialNumber = addGrItem.GrSerialNumber;
        //                if (addGrItem.Price > 0)
        //                { gd.Price = addGrItem.Price; }
        //                else { gd.Price = 0.01; }
        //                gd.Status = 1;// 1= Pending for Checker Side, 2=Approved , 3=Reject
        //                gd.BatchNo = addGrItem.BatchNo;
        //                gd.MFGDate = addGrItem.MFGDate;
        //                gd.Barcode = addGrItem.Barcode;
        //                gd.VehicleType = VehicleType;
        //                gd.VehicleNumber = VehicleNumber;
        //                gd.CreatedBy = people.PeopleID;
        //                gd.CreatedDate = indianTime;
        //                gd.ModifiedDate = indianTime;
        //                gd.GeneratedDate = indianTime;
        //                gd.ApprovedBy = 0; //approved by or rejectby
        //                //gd.ModifiedBy = people.PeopleID; //approved by or rejectby
        //                UpdateGoodsReceivedDetail.Add(gd);
        //            }
        //            else
        //            {
        //                TemporaryCurrentStockHistory Oss = new TemporaryCurrentStockHistory();
        //                Oss.StockId = item.Id;
        //                Oss.ItemNumber = item.ItemNumber;
        //                Oss.itemname = item.itemname;
        //                Oss.OdOrPoId = addGrItem.PurchaseOrderId;
        //                Oss.CurrentInventory = item.CurrentInventory;
        //                Oss.CurrentInventoryIn = Convert.ToInt32(QtyReciv);
        //                Oss.TotalCurrentInventory = item.CurrentInventory + Convert.ToInt32(QtyReciv);
        //                Oss.TotalDamageCurrentInventory = item.DamageCurrentInventory + Convert.ToInt32(DamQtyReciv);
        //                Oss.TotalExpCurrentInventory = item.ExpCurrentInventory + Convert.ToInt32(ExpQtyReciv);
        //                Oss.WarehouseName = item.WarehouseName;
        //                Oss.Warehouseid = item.WarehouseId;
        //                Oss.CompanyId = item.CompanyId;
        //                Oss.CreationDate = indianTime;
        //                Oss.ItemMultiMRPId = addGrItem.ItemMultiMRPId;
        //                AddTemporaryCurrentStockHistory.Add(Oss);

        //                gd.PurchaseOrderDetailId = addGrItem.PurchaseOrderDetailId;
        //                gd.ItemMultiMRPId = addGrItem.ItemMultiMRPId;
        //                gd.IsActive = true;
        //                gd.IsDeleted = false;
        //                gd.Qty = QtyReciv;
        //                gd.DamageQty = DamQtyReciv;
        //                gd.ExpiryQty = ExpQtyReciv;
        //                gd.IsFreeItem = addGrItem.IsFreeItem;
        //                gd.GrSerialNumber = addGrItem.GrSerialNumber;
        //                if (addGrItem.Price > 0)
        //                { gd.Price = addGrItem.Price; }
        //                else { gd.Price = 0.01; }

        //                gd.Status = 1;// 1= Pending for Checker Side, 2=Approved , 3=Reject
        //                gd.BatchNo = addGrItem.BatchNo;
        //                gd.MFGDate = addGrItem.MFGDate;
        //                gd.Barcode = addGrItem.Barcode;
        //                gd.VehicleType = VehicleType;
        //                gd.VehicleNumber = VehicleNumber;
        //                gd.CreatedBy = people.PeopleID;
        //                gd.CreatedDate = indianTime;
        //                gd.ModifiedDate = indianTime;
        //                gd.GeneratedDate = indianTime;
        //                gd.ApprovedBy = 0; //approved by or rejectby
        //                AddGoodsReceivedDetail.Add(gd);

        //            }
        //            item.CurrentInventory = item.CurrentInventory + Convert.ToInt32(QtyReciv);
        //            item.DamageCurrentInventory = item.DamageCurrentInventory + Convert.ToInt32(DamQtyReciv);
        //            item.ExpCurrentInventory = item.DamageCurrentInventory + Convert.ToInt32(ExpQtyReciv);
        //            item.UpdatedDate = indianTime;

        //            UpdateTemporaryCurrentStock.Add(item);
        //        }
        //    }

        //    int DeletedQty = 0;

        //    #region Not Posted Data From App here we update there status
        //    var NotEdited = PurchaseOrderDetailRecived.Where(ds => ds.GrSerialNumber == GrSerialNumber && ds.Status == 3 && !AddPOGRDetailobj.PurchaseOrderDetailDc.Any(db => db.ItemMultiMRPId == ds.ItemMultiMRPId && db.PurchaseOrderDetailId == ds.PurchaseOrderDetailId)).ToList();
        //    if (NotEdited != null)
        //    {

        //        foreach (var item in NotEdited)
        //        {
        //            if (AddPOGRDetailobj.PurchaseOrderDetailDc.Any(db => db.PurchaseOrderDetailId == item.PurchaseOrderDetailId))
        //            {
        //                DeletedQty += item.Qty;

        //                item.IsDeleted = true;
        //                item.ModifiedDate = indianTime;
        //                item.CreatedDate = indianTime;
        //                UpdateGoodsReceivedDetail.Add(item);
        //            }
        //            else
        //            {
        //                item.Status = 1;// we chnage status in reject to Pending for Checker Side	
        //                item.VehicleNumber = VehicleNumber;
        //                item.VehicleType = VehicleType;
        //                item.ModifiedDate = indianTime;
        //                item.CreatedDate = indianTime;
        //                UpdateGoodsReceivedDetail.Add(item);
        //            }

        //        }
        //    }
        //    #endregion

        //    // int TotalPOQty = AddPOGRDetailobj.PurchaseOrderDetailDc.Sum(x => x.TotalQuantity);
        //    int TotalPOQty = PurchaseOrderMaster.PurchaseOrderDetail.Sum(x => x.TotalQuantity);
        //    int? TotalPORecQty = 0;
        //    TotalPORecQty = AddGoodsReceivedDetail.Any() ? AddGoodsReceivedDetail.Sum(p => p.Qty) : 0;
        //    if (PurchaseOrderDetailRecived.Any())
        //    {
        //        TotalPORecQty += PurchaseOrderDetailRecived.Any() ? PurchaseOrderDetailRecived.Sum(p => p.Qty) : 0;
        //        TotalPORecQty -= DeletedQty;
        //    }
        //    int percentComplete = (int)Math.Round((double)(100 * TotalPORecQty) / TotalPOQty);
        //    PurchaseOrderMaster.progress = Convert.ToString(percentComplete);
        //    if (TotalPOQty > TotalPORecQty)
        //    {
        //        PurchaseOrderMaster.Status = "UN Partial Received";
        //    }
        //    else
        //    {
        //        if (TotalPOQty >= TotalPORecQty)
        //        {
        //            PurchaseOrderMaster.Status = "UN Received";
        //        }
        //    }
        //    foreach (var item in UpdateTemporaryCurrentStock)
        //    {
        //        context.Entry(item).State = EntityState.Modified;
        //    }
        //    if (AddGoodsReceivedDetail != null && AddGoodsReceivedDetail.Count > 0)
        //    {
        //        context.GoodsReceivedDetail.AddRange(AddGoodsReceivedDetail);
        //    }
        //    if (UpdateGoodsReceivedDetail != null && UpdateGoodsReceivedDetail.Any())
        //    {
        //        foreach (var UpdateGoodsDetail in UpdateTemporaryCurrentStock)
        //        {
        //            context.Entry(UpdateGoodsDetail).State = EntityState.Modified;
        //        }
        //    }
        //    context.TemporaryCurrentStockHistoryDB.AddRange(AddTemporaryCurrentStockHistory);
        //    context.Entry(PurchaseOrderMaster).State = EntityState.Modified;
        //    if (context.Commit() > 0) { return true; } else { return false; }
        //}
        //#endregion
        #endregion

        #region Get List of Pending Approval UnApprovedGR/V1
        /// <summary>
        /// GetUnconfirmGrData
        /// </summary>UnApprovedGRDetails
        /// <returns></returns>
        [AllowAnonymous]
        [Route("UnApprovedGR/V1")]
        [HttpGet]
        public HttpResponseMessage UnApprovedGRDataListV1(int WarehouseId)
        {
            List<PurchaseOrderMaster> pm = new List<PurchaseOrderMaster>();

            string resultMessage = "";
            bool status = false;
            var _result = new List<PurchaseOrderMasterDc>();
            using (AuthContext db = new AuthContext())
            {

                string query = " select PurchaseOrderId from PurchaseOrderMasters where  WarehouseId=" + WarehouseId + "  and Status in ('UN Received','UN Partial Received')";
                var Ids = db.Database.SqlQuery<int>(query).ToList();

                var PoIds = db.GoodsReceivedDetail.Where(x => Ids.Contains(x.PurchaseOrderDetail.PurchaseOrderId) && x.IsDeleted == false && x.Status == 1).Select(x => x.PurchaseOrderDetail.PurchaseOrderId).Distinct().ToList();

                string pquery = "select pd.PurchaseOrderId from GrQualityInvoices gr with(nolock) " +
                    "inner join GoodsReceivedDetails gd with(nolock) on gr.GoodsReceivedDetailId = gd.Id and gd.Status=1" +
                    "inner join PurchaseOrderDetails pd with(nolock) on pd.PurchaseOrderDetailId= gd.PurchaseOrderDetailId " +
                    "where gr.IsApproved=0 and pd.WarehouseId=" + WarehouseId + "group by pd.PurchaseOrderId";
                var PurchaseOrderIds = db.Database.SqlQuery<int>(pquery).ToList();
                List<int> Poresult = new List<int>();
                Poresult = PoIds;
                if (PurchaseOrderIds != null && PurchaseOrderIds.Any())
                {
                    Poresult = PoIds.Where(p => !PurchaseOrderIds.Contains(p)).ToList();
                }
                pm = db.DPurchaseOrderMaster.Where(x => Poresult.Contains(x.PurchaseOrderId)).ToList();
                _result = Mapper.Map(pm).ToANew<List<PurchaseOrderMasterDc>>();
                if (_result != null && _result.Any())
                {

                    var gdn = db.GoodsDescripancyNoteMasterDB.Where(x => x.IsActive == true && x.IsDeleted == false).ToList();
                    foreach (var item in _result)
                    {
                        item.IsGDN = gdn.Any(x => x.PurchaseOrderId == item.PurchaseOrderId);
                    }
                    status = true; resultMessage = "Record found";
                }
                else
                { status = true; resultMessage = "No Record found"; }
            }
            var res = new
            {
                UnApprovedGR = _result,
                status = status,
                Message = resultMessage
            };

            return Request.CreateResponse(HttpStatusCode.OK, res);
        }
        #endregion

        #region  Get List of Pending Approval UnApprovedGRDetails/V1
        [Route("UnApprovedGRDetails/V1")]
        [HttpGet]
        [AllowAnonymous]
        public HttpResponseMessage UnApprovedGRDetails(long PurchaseOrderId, int userid)
        {
            try
            {
                bool status = false;
                string resultMessage = "";
                var GoodsReceived = new List<GoodsReceivedDc>();
                using (var db = new AuthContext())
                {
                    var Gr = db.GoodsReceivedDetail.Where(x => x.PurchaseOrderDetail.PurchaseOrderId == PurchaseOrderId && x.IsDeleted == false && x.Status == 1).Include(x => x.PurchaseOrderDetail).ToList();

                    var itemMultiMRPIds = Gr.Select(k => k.ItemMultiMRPId).Distinct().ToList();
                    var multimrp = db.ItemMultiMRPDB.Where(x => itemMultiMRPIds.Contains(x.ItemMultiMRPId)).ToList();
                    var _result = Mapper.Map(Gr).ToANew<List<GoodsReceivedDetailDc>>();
                    if (_result != null && _result.Any())
                    {
                        var CreatedBy = Gr.Select(x => x.CreatedBy).Distinct().ToList();
                        var ApprovedBy = Gr.Select(x => x.ApprovedBy).Distinct().ToList();
                        var peopleIds = CreatedBy.Concat(ApprovedBy);
                        var people = db.Peoples.Where(x => peopleIds.Contains(x.PeopleID)).ToList();
                        // bool Isgdn = db.GoodsDescripancyNoteMasterDB.Any(x => x.PurchaseOrderId == PurchaseOrderId && x.IsActive == true && x.IsDeleted == false);
                        var GDNList = db.GoodsDescripancyNoteMasterDB.Where(x => x.PurchaseOrderId == PurchaseOrderId && x.IsActive == true && x.IsDeleted == false && x.IsGDNGenerate == false).ToList();
                        List<long> GrId = Gr.Select(g => g.Id).ToList();
                        List<GrQualityInvoice> GrList = db.GrQualityInvoices.Where(x => GrId.Contains(x.GoodsReceivedDetailId) && x.IsActive == true && x.IsDeleted == false).ToList();

                        List<string> ItemNumber = Gr.Select(q => q.PurchaseOrderDetail.ItemNumber).ToList();
                        //var qaCheckerList = db.GrQualityConfigurations.Where(q => ItemNumber.Contains(q.ItemNumber) && q.IsActive == true && q.IsDeleted == false).ToList();
                        var checkerids = GrList.Select(x => x.GrQualityCheckerId).Distinct().ToList();
                        var pList = db.Peoples.Where(a => checkerids.Contains(a.PeopleID)).ToList();

                        GoodsReceived = _result.GroupBy(x => new
                        {
                            x.GrSerialNumber,
                            x.Status,
                            x.ApprovedBy,
                            x.VehicleType,
                            x.VehicleNumber,
                            x.CreatedDate,
                            // x.ModifiedDate,
                            x.CreatedBy,

                            // x.ModifiedBy,
                        }).Select(y => new GoodsReceivedDc
                        {
                            GrPersonName = people.Where(p => p.PeopleID == y.Key.CreatedBy).Select(p => p.DisplayName).FirstOrDefault(),
                            CreatedBy = y.Key.CreatedBy,
                            CreatedDate = y.Key.CreatedDate,
                            GrSerialNumber = y.Key.GrSerialNumber,
                            //ModifiedBy = y.Key.ModifiedBy,
                            ApproverName = people.Where(p => p.PeopleID == y.Key.ApprovedBy).Select(p => p.DisplayName).FirstOrDefault(),
                            // ModifiedDate = y.Key.ModifiedDate,
                            Status = y.Key.Status,
                            VehicleNumber = y.Key.VehicleNumber,
                            VehicleType = y.Key.VehicleType,

                            GoodsReceivedItemDcs = y.Select(x => new GoodsReceivedItemDc
                            {
                                Id = x.Id,
                                PurchaseOrderDetailId = x.PurchaseOrderDetailId,
                                ItemName = Gr.Any(p => p.PurchaseOrderDetailId == x.PurchaseOrderDetailId) ? Gr.FirstOrDefault(p => p.PurchaseOrderDetailId == x.PurchaseOrderDetailId).PurchaseOrderDetail?.ItemName : "",
                                Itemnumber = Gr.Any(p => p.PurchaseOrderDetailId == x.PurchaseOrderDetailId) ? Gr.FirstOrDefault(p => p.PurchaseOrderDetailId == x.PurchaseOrderDetailId).PurchaseOrderDetail?.ItemNumber : "",
                                TotalQuantity = Gr.Any(p => p.PurchaseOrderDetailId == x.PurchaseOrderDetailId) ? Gr.FirstOrDefault(p => p.PurchaseOrderDetailId == x.PurchaseOrderDetailId).PurchaseOrderDetail.TotalQuantity : 0,
                                ItemMultiMRPId = x.ItemMultiMRPId,
                                Qty = x.Qty,
                                DamageQty = x.DamageQty,
                                ExpiryQty = x.ExpiryQty,
                                ShortQty = x.ShortQty,
                                Price = x.Price,
                                CurrentStockHistoryId = x.CurrentStockHistoryId,
                                BatchNo = x.BatchNo,
                                MFGDate = x.MFGDate,
                                Barcode = x.Barcode,
                                IsFreeItem = x.IsFreeItem,
                                weight = x.weight,
                                //CompanyStockCode = multimrp.Where(z => z.ItemMultiMRPId == x.ItemMultiMRPId).Select(z => z.CompanyStockCode).FirstOrDefault()
                                QualityImage = GrList != null && GrList.Any(q => q.GoodsReceivedDetailId == x.Id) ? GrList.Where(q => q.GoodsReceivedDetailId == x.Id).Select(z => z.Image).FirstOrDefault() : null,
                                GrQualityInvoiceId = GrList != null && GrList.Any(q => q.GoodsReceivedDetailId == x.Id) ? GrList.Where(q => q.GoodsReceivedDetailId == x.Id).Select(z => z.Id).FirstOrDefault() : 0,
                                QaStatus = GrList != null && GrList.Any(q => q.GoodsReceivedDetailId == x.Id && q.IsApproved == 0) ? "Pending" : GrList.Any(q => q.GoodsReceivedDetailId == x.Id && q.IsApproved == 1) ? "Approved" : "Rejected",
                                IsShowQaReport = GrList != null && GrList.Any(q => q.GoodsReceivedDetailId == x.Id && q.GrQualityCheckerId == userid) ? true : false,
                                QaCheckerId = GrList != null && GrList.Any(q => q.GoodsReceivedDetailId == x.Id) ? GrList.Where(q => q.GoodsReceivedDetailId == x.Id).Select(z => z.GrQualityCheckerId).FirstOrDefault() : 0,
                            }).ToList()
                        }).ToList();
                        if (Gr != null && Gr.Any())
                        {
                            var ItemnumberList = GoodsReceived.SelectMany(x => x.GoodsReceivedItemDcs.Select(i => i.Itemnumber)).ToList();
                            var itemMultiMrpdetail = db.ItemMultiMRPDB.Where(a => ItemnumberList.Contains(a.ItemNumber) && a.Deleted == false).Select(x => new ItemMultiMRPDc { ItemNumber = x.ItemNumber, ItemMultiMRPId = x.ItemMultiMRPId, MRP = x.MRP, CompanyStockCode = x.CompanyStockCode }).ToList();
                            GoodsReceived.ForEach(x =>
                            {
                                x.IsGDN = GDNList != null ? GDNList.Any(y => y.GrSerialNo == x.GrSerialNumber) : false;
                                x.GoodsReceivedItemDcs.ForEach(i =>

                                    {
                                        if (itemMultiMrpdetail != null && itemMultiMrpdetail.Any(y => y.ItemNumber == i.Itemnumber))
                                        {
                                            i.MRP = itemMultiMrpdetail.Where(c => c.ItemNumber == i.Itemnumber && c.ItemMultiMRPId == i.ItemMultiMRPId).Select(c => c.MRP).FirstOrDefault();
                                            i.CompanyStockCode = itemMultiMrpdetail.Where(c => c.ItemNumber == i.Itemnumber && c.ItemMultiMRPId == i.ItemMultiMRPId).Select(c => c.CompanyStockCode).FirstOrDefault();
                                            i.QaName = i.QaCheckerId > 0 ? pList.FirstOrDefault(b => b.PeopleID == i.QaCheckerId).DisplayName : "";
                                            i.QaPhoneNo = i.QaCheckerId > 0 ? pList.FirstOrDefault(b => b.PeopleID == i.QaCheckerId).Mobile : "";
                                        }
                                    });

                            });

                            status = true; resultMessage = "Record found";
                        }

                    }
                    status = true; resultMessage = "No Record found";
                }

                var res = new
                {
                    UnApprovedGRDetails = GoodsReceived,
                    status = status,
                    Message = resultMessage
                };
                return Request.CreateResponse(HttpStatusCode.OK, res);
            }
            catch (Exception ex)
            {
                var res = new
                {
                    UnApprovedGRDetails = new List<GoodsReceivedDc>(),
                    status = false,
                    Message = ex.Message
                };
                return Request.CreateResponse(HttpStatusCode.OK, res);
            }
        }
        #endregion

        #region Approved Gr in PO to Currentstock

        [Route("ApprovedPoGr/V1")]
        [HttpPost]
        [AllowAnonymous]
        public HttpResponseMessage ApprovedPoGrV1(POApproverDc POApproverDc)
        {
            string resultMessage = "";
            bool IsValidate = false;
            bool status = false;
            BatchManager.Publisher publisher = new BatchManager.Publisher();
            List<BatchCodeSubject> OnGRNBatchCodeApproveList = new List<BatchCodeSubject>();
            TransactionOptions option = new TransactionOptions();
            option.IsolationLevel = IsolationLevel.RepeatableRead;
            option.Timeout = TimeSpan.FromSeconds(90);
            using (var dbContextTransaction = new TransactionScope(TransactionScopeOption.Required, option))
            {
                using (AuthContext context = new AuthContext())
                {
                    int UserId = POApproverDc.UserId;
                    var people = context.Peoples.Where(x => x.PeopleID == UserId && x.Deleted == false && x.Active).SingleOrDefault();
                    PurchaseOrderMaster PurchaseOrderMaster = context.DPurchaseOrderMaster.Where(x => x.PurchaseOrderId == POApproverDc.PurchaseOrderId && (x.Status == "UN Received" || x.Status == "UN Partial Received") && (x.Status != "Auto Closed" || x.Status != "Closed")).Include(x => x.PurchaseOrderDetail).SingleOrDefault();
                    if (PurchaseOrderMaster != null && PurchaseOrderMaster.Gr_Process == true)
                    {
                        PurchaseOrderMaster.Gr_Process = false;
                        context.Entry(PurchaseOrderMaster).State = EntityState.Modified;
                        context.Commit();

                        var warehouse = context.Warehouses.FirstOrDefault(x => x.WarehouseId == PurchaseOrderMaster.WarehouseId);

                        if (warehouse.IsStopCurrentStockTrans)
                        {
                            var result = new
                            {
                                POGRDetail = POApproverDc,
                                status = false,
                                Message = "Inventory Transactions are currently disabled for this warehouse... Please try after some time"

                            };
                            return Request.CreateResponse(HttpStatusCode.OK, result);

                        }



                        var Gr = context.GoodsReceivedDetail.Where(x => x.PurchaseOrderDetail.PurchaseOrderId == PurchaseOrderMaster.PurchaseOrderId && x.IsDeleted == false && x.Status == 1).Include(x => x.PurchaseOrderDetail).Include(x => x.GRBatchs).ToList();
                        if (Gr != null && Gr.Any())
                        {
                            List<long> list = Gr.Select(x => x.Id).Distinct().ToList();
                            var grinvoicedata = context.GrQualityInvoices.Where(x => x.IsActive == true && x.IsDeleted == false && x.IsApproved == 0 && list.Contains(x.GoodsReceivedDetailId)).ToList();
                            if (grinvoicedata != null && grinvoicedata.Any())
                            {
                                var result = new
                                {
                                    POGRDetail = POApproverDc,
                                    status = false,
                                    Message = "Please Approve or Reject Quality Testing Report for Items."

                                };
                                return Request.CreateResponse(HttpStatusCode.OK, result);
                            }
                            PurchaseOrderMaster.Comment = PurchaseOrderMaster.Comment + Environment.NewLine + POApproverDc.POApproveStatus;
                            IsValidate = true;
                            if (IsValidate && POApproverDc.GrSerialNumber > 0)
                            {

                                var result = GoodsReceivedInStock(PurchaseOrderMaster, Gr, POApproverDc.GrSerialNumber, people, context, dbContextTransaction);
                                if (result.IsSuccess)
                                {
                                    if (result.OnGRNBatchCodeApproveList != null && result.OnGRNBatchCodeApproveList.Any())
                                    {
                                        OnGRNBatchCodeApproveList.AddRange(result.OnGRNBatchCodeApproveList);
                                    }



                                    #region  Activate itemmaster for Consumer store
                                    //if (warehouse.StoreType == 1)
                                    //{
                                    //    try
                                    //    {
                                    //        var activateItemlist = new List<ActivateItemForConsumerStoreOnGRNDc>();

                                    //        foreach (var x in OnGRNBatchCodeApproveList)
                                    //        {
                                    //            activateItemlist.Add(new ActivateItemForConsumerStoreOnGRNDc
                                    //            {
                                    //                ItemMultiMrpId = x.ItemMultiMrpId,
                                    //                WarehouseId = x.WarehouseId
                                    //            }
                                    //            );
                                    //        }
                                    //        ActivateItemForConsumerStore(activateItemlist.Distinct().ToList(), context, UserId);
                                    //    }
                                    //    catch (Exception ex)
                                    //    {
                                    //        string error = ex.InnerException != null ? ex.ToString() + Environment.NewLine + ex.InnerException.ToString() : ex.ToString();
                                    //        TextFileLogHelper.LogError(new StringBuilder("Error on GRN ActivateItemForConsumerStore ").Append(error).Append($"  on : {indianTime}").ToString());
                                    //    }
                                    //}
                                    #endregion






                                    var supplier = context.Suppliers.Where(a => a.SupplierId == PurchaseOrderMaster.SupplierId).SingleOrDefault();
                                    ///Send SMS & Notification To Supplier
                                    var grAmount = Gr.Sum(x => x.Qty * x.Price);

                                    Sms s = new Sms();
                                    // string msg = "Shopkirana has received goods for amount " + Math.Round(grAmount, 2) + " against PO no: " + PurchaseOrderMaster.PurchaseOrderId + " In Hub:" + PurchaseOrderMaster.WarehouseName + ". ShopKirana";
                                    string msg = "";//"Shopkirana has received goods for amount {#var1#} against PO no: {#var2#} In Hub:{#var3#}. ShopKirana";
                                    var dltSMS = SMSTemplateHelper.getTemplateText((int)AppEnum.Others, "GR_Done");
                                    msg = dltSMS == null ? "" : dltSMS.Template;
                                    // msg = SMSTemplateHelper.getTemplateText((int)AppEnum.SarthiApp, "GR_Done");
                                    msg = msg.Replace("{#var1#}", Math.Round(grAmount, 2).ToString());
                                    msg = msg.Replace("{#var2#}", PurchaseOrderMaster.PurchaseOrderId.ToString());
                                    msg = msg.Replace("{#var3#}", PurchaseOrderMaster.WarehouseName.ToString());

                                    string Mob = supplier?.MobileNo;
                                    string email = supplier.EmailId;
                                    string FCMID = supplier?.fcmId;
                                    string FCMNotification = "{title:'New Good Received',body:'" + msg + "',icon:'',notify_type:'GR',ObjectId:" + PurchaseOrderMaster.PurchaseOrderId + "}";


                                    if (!string.IsNullOrEmpty(Mob) && dltSMS != null) { s.sendOtp(Mob, msg, dltSMS.DLTId); }
                                    if (!string.IsNullOrEmpty(FCMID)) { s.SupplierSendNotification(FCMID, FCMNotification); }

                                    resultMessage = "GR Appoved Succesfully, Will reflect into Stock";
                                    status = true;

                                    #region SendGDN via SMS to Supplier 
                                    GDNHelper.sendGDNtoSupplier(PurchaseOrderMaster.PurchaseOrderId, PurchaseOrderMaster.WarehouseName, Mob, POApproverDc.GrSerialNumber, email, context);
                                    #endregion


                                    #region on CalculatePurchasePrice
                                    try
                                    {
                                        List<RiskCalculatePurchasePriceDc> Items = new List<RiskCalculatePurchasePriceDc>();
                                        foreach (var item in Gr)
                                        {
                                            Items.Add(new RiskCalculatePurchasePriceDc
                                            {
                                                ItemMultiMrpId = item.ItemMultiMRPId,
                                                Price = item.Price,
                                                Qty = item.Qty,
                                                WarehouseId = PurchaseOrderMaster.WarehouseId
                                            });
                                        }

                                        CalculatePurchasePriceHelper helper = new CalculatePurchasePriceHelper();
                                        bool IsUpdate = helper.CalculatePPOnInternalTransferForRisk(context, Items, 0, 0, UserId);
                                    }
                                    catch (Exception ex)
                                    {
                                        string error = ex.InnerException != null ? ex.ToString() + Environment.NewLine + ex.InnerException.ToString() : ex.ToString();
                                        TextFileLogHelper.LogError(new StringBuilder("Error on Gr Purchase Risk Qty Update : ").Append(error).Append($"  on : {indianTime}").ToString());
                                        TextFileLogHelper.LogError(new StringBuilder("PO Id : ").Append(PurchaseOrderMaster.PurchaseOrderId).Append($" on : {indianTime}").ToString());
                                        TextFileLogHelper.LogError(new StringBuilder("For GRN No# : ").Append(Gr.FirstOrDefault().GrSerialNumber).Append($" on : {indianTime}").ToString());
                                    }

                                    #endregion



                                    dbContextTransaction.Complete();

                                    if (OnGRNBatchCodeApproveList != null && OnGRNBatchCodeApproveList.Any())
                                    {
                                        foreach (var item in OnGRNBatchCodeApproveList)
                                        {
                                            AsyncContext.Run(() => publisher.PublishInBatchCode(item));
                                        }
                                    }
                                }
                                else
                                {
                                    dbContextTransaction.Dispose();
                                    resultMessage = "Something went wrong";
                                    status = false;

                                }

                            }
                            else { resultMessage = "Already gr posted or may be under in RejectGR(will be approved through backend portal)"; status = false; }

                        }

                    }
                    else
                    {
                        resultMessage = "Something went wrong"; status = false;
                    }
                    var res = new
                    {
                        POGRDetail = POApproverDc,
                        status = status,
                        Message = resultMessage

                    };
                    return Request.CreateResponse(HttpStatusCode.OK, res);

                }
            }
        }

        public GrnApproveDc GoodsReceivedInStock(PurchaseOrderMaster PurchaseOrderMaster, List<GoodsReceivedDetail> GoodsReceivedDetail, int GrSerialNumber, People people, AuthContext context, TransactionScope dbContextTransaction)
        {
            var result = new GrnApproveDc();
            result.OnGRNBatchCodeApproveList = new List<DataContracts.BatchCode.BatchCodeSubject>();

            string Desc = "(+)Stock GRN:" + GrSerialNumber;

            bool issuccess = true;

            var PoItemMultimrpids = new List<int>();
            var FIItemMultimrpids = new List<int>();
            var DamageMultimrpids = new List<int>();

            var FreeStokList = new List<FreeStock>();
            var FreeMRPList = new List<ItemMultiMRP>();

            // var PoInventoryManagerNew = new PoInventoryManagerNew();
            PoItemMultimrpids = GoodsReceivedDetail.Where(x => x.IsFreeItem == false).Select(x => x.ItemMultiMRPId).Distinct().ToList();
            var CurrentStockList = context.DbCurrentStock.Where(x => PoItemMultimrpids.Contains(x.ItemMultiMRPId) && x.WarehouseId == PurchaseOrderMaster.WarehouseId && x.Deleted == false).ToList(); // ItemMultiMRPList
            var ItemMultiMRPList = context.ItemMultiMRPDB.Where(x => PoItemMultimrpids.Contains(x.ItemMultiMRPId) && x.Deleted == false).ToList(); // ItemMultiMRPList

            FIItemMultimrpids = GoodsReceivedDetail.Where(x => x.IsFreeItem == true).Select(x => x.ItemMultiMRPId).Distinct().ToList();//
            FreeStokList = context.FreeStockDB.Where(x => FIItemMultimrpids.Contains(x.ItemMultiMRPId) && x.WarehouseId == PurchaseOrderMaster.WarehouseId && x.Deleted == false).ToList();
            FreeMRPList = context.ItemMultiMRPDB.Where(x => FIItemMultimrpids.Contains(x.ItemMultiMRPId)).ToList();

            //Damage
            DamageMultimrpids = GoodsReceivedDetail.Where(x => x.DamageQty > 0).Select(x => x.ItemMultiMRPId).Distinct().ToList();//
            var DamageStockList = context.DamageStockDB.Where(x => DamageMultimrpids.Contains(x.ItemMultiMRPId) && x.WarehouseId == PurchaseOrderMaster.WarehouseId).ToList();

            //KKMateg
            var NumberList = ItemMultiMRPList.Select(x => x.ItemNumber).Distinct().ToList();
            var ItemmastersList = context.itemMasters.Where(x => NumberList.Contains(x.Number) && x.WarehouseId == PurchaseOrderMaster.WarehouseId && x.SubcategoryName.ToUpper().Equals("KISAN KIRANA") && x.Deleted == false).ToList();


            #region update weight in itemmastercentral from grn

            //var ItemmastersCentral = context.ItemMasterCentralDB.Where(x => NumberList.Contains(x.Number) && x.Deleted == false).ToList();
            //foreach (var centralitem in ItemmastersCentral)
            //{
            //    var grnweight = GoodsReceivedDetail.Where(x => x.PurchaseOrderDetail.ItemNumber == centralitem.Number && x.weight > 0).FirstOrDefault();
            //    if (grnweight != null && grnweight.weight > 0 && centralitem.weight != grnweight.weight)
            //    {

            //        /// NOTE: by the new requirement weight will not be updated from grn
            //        /// NOTE: weight will be updated from PO creation only
            //        //centralitem.weight = grnweight.weight;
            //        //centralitem.weighttype = "Gm";

            //        centralitem.UpdatedDate = indianTime;
            //        centralitem.ModifiedBy = people.PeopleID;
            //        centralitem.UpdateBy = people.DisplayName;
            //        context.Entry(centralitem).State = EntityState.Modified;
            //    }
            //}
            #endregion


            #region Remove or set Zero qty for GDN
            foreach (var ditem in GoodsReceivedDetail.Where(x => x.DamageQty > 0 || x.ExpiryQty > 0 && x.Status == 1).ToList())
            {
                var Gdn = context.GoodsDescripancyNoteDetailDB.Where(x => x.GoodsReceivedDetailId == ditem.Id).FirstOrDefault();
                if (Gdn != null)
                {
                    ditem.DamageQty = Gdn.DamageQty > 0 ? 0 : ditem.DamageQty;
                    ditem.ExpiryQty = Gdn.ExpiryQty > 0 ? 0 : ditem.ExpiryQty;
                    ditem.ShortQty = Gdn.ShortQty > 0 ? 0 : ditem.ShortQty;
                }
            }

            #endregion

            foreach (var k in GoodsReceivedDetail.Where(x => x.IsFreeItem == false))
            {
                var item = CurrentStockList.Where(x => x.ItemMultiMRPId == k.ItemMultiMRPId).SingleOrDefault();
                var MRPdetail = ItemMultiMRPList.Where(a => a.ItemMultiMRPId == k.ItemMultiMRPId).SingleOrDefault();
                if (item == null)
                {
                    CurrentStock NewStock = new CurrentStock();
                    NewStock.CompanyId = MRPdetail.CompanyId;
                    NewStock.CreationDate = indianTime;
                    NewStock.CurrentInventory = 0;
                    NewStock.Deleted = false;
                    NewStock.ItemMultiMRPId = k.ItemMultiMRPId;
                    NewStock.itemname = MRPdetail.itemname;
                    NewStock.ItemNumber = MRPdetail.ItemNumber;
                    NewStock.itemBaseName = MRPdetail.itemBaseName;
                    NewStock.UpdatedDate = indianTime;
                    NewStock.WarehouseId = PurchaseOrderMaster.WarehouseId;
                    NewStock.WarehouseName = PurchaseOrderMaster.WarehouseName;
                    NewStock.MRP = MRPdetail.MRP;
                    NewStock.UOM = MRPdetail.UOM;
                    NewStock.userid = people.PeopleID;
                    context.DbCurrentStock.Add(NewStock);
                    context.Commit();
                }
                var Damage = DamageStockList.Where(x => x.ItemMultiMRPId == k.ItemMultiMRPId).SingleOrDefault();
                if (Damage == null && k.DamageQty > 0)
                {
                    DamageStock NewStock = new DamageStock();
                    NewStock.CompanyId = MRPdetail.CompanyId;
                    NewStock.CreatedDate = indianTime;
                    NewStock.DamageInventory = 0;
                    NewStock.Deleted = false;
                    NewStock.ItemMultiMRPId = k.ItemMultiMRPId;
                    NewStock.ItemName = MRPdetail.itemname;
                    NewStock.ItemNumber = MRPdetail.ItemNumber;
                    NewStock.itemBaseName = MRPdetail.itemBaseName;
                    NewStock.UpdatedDate = indianTime;
                    NewStock.WarehouseId = PurchaseOrderMaster.WarehouseId;
                    NewStock.WarehouseName = PurchaseOrderMaster.WarehouseName;
                    NewStock.MRP = MRPdetail.MRP;
                    NewStock.UOM = MRPdetail.UOM;
                    context.DamageStockDB.Add(NewStock);
                    context.Commit();
                }
                // Recall
                if (item != null)
                {
                    if (ItemmastersList.Any(x => x.Number == item.ItemNumber) && item.UOM != null)
                    {

                        bool Res = UpdateMaterialItemReceivedDetails(k.Qty, item.UOM, PurchaseOrderMaster.PurchaseOrderId, item.ItemNumber, context, dbContextTransaction, GetUserId());
                    }
                }
                k.Status = 2;
                k.ApprovedBy = people.PeopleID;
                k.ModifiedDate = indianTime;
                k.CreatedDate = indianTime;

            }
            if (FIItemMultimrpids.Count > 0)
            {
                foreach (var f in GoodsReceivedDetail.Where(x => x.IsFreeItem == true))
                {
                    var stok = FreeStokList.Where(x => x.ItemMultiMRPId == f.ItemMultiMRPId).FirstOrDefault();
                    var MRP = FreeMRPList.Where(a => a.ItemMultiMRPId == f.ItemMultiMRPId).SingleOrDefault();
                    if (stok != null)
                    {
                    }
                    else
                    {
                        FreeStock FSN = new FreeStock();
                        FSN.itemname = PurchaseOrderMaster.PurchaseOrderDetail.Any(p => p.PurchaseOrderDetailId == f.PurchaseOrderDetailId) ? PurchaseOrderMaster.PurchaseOrderDetail.FirstOrDefault(p => p.PurchaseOrderDetailId == f.PurchaseOrderDetailId).ItemName : " ";
                        FSN.ItemNumber = MRP.ItemNumber;
                        FSN.ItemMultiMRPId = f.ItemMultiMRPId;
                        FSN.MRP = MRP.MRP;
                        FSN.WarehouseId = Convert.ToInt32(PurchaseOrderMaster.WarehouseId);
                        FSN.CurrentInventory = 0;
                        FSN.CreatedBy = people.DisplayName;
                        FSN.CreationDate = indianTime;
                        FSN.Deleted = false;
                        FSN.UpdatedDate = indianTime;
                        context.FreeStockDB.Add(FSN);
                        context.Commit();
                    }
                    f.Status = 2;
                    f.ApprovedBy = people.PeopleID;
                    f.ModifiedDate = indianTime;
                    f.CreatedDate = indianTime;
                }
            }
            if (PurchaseOrderMaster.Status == "UN Partial Received")
            {
                PurchaseOrderMaster.Status = "CN Partial Received";
            }
            else if (PurchaseOrderMaster.Status == "UN Received")
            {
                PurchaseOrderMaster.Status = "CN Received";
            }

            context.Entry(PurchaseOrderMaster).State = EntityState.Modified;

            #region InReceivedStocks to Currentstock Hit

            MultiStockHelper<OnGRNApproveCStockEntryDc> MultiStockHelpers = new MultiStockHelper<OnGRNApproveCStockEntryDc>();
            List<OnGRNApproveCStockEntryDc> OnGRNRequestCStockList = new List<OnGRNApproveCStockEntryDc>();
            foreach (var StockHit in GoodsReceivedDetail.Where(x => (x.Qty > 0 || x.DamageQty > 0 || x.ExpiryQty > 0) && x.Status == 2))
            {
                OnGRNRequestCStockList.Add(new OnGRNApproveCStockEntryDc
                {
                    ItemMultiMRPId = StockHit.ItemMultiMRPId,
                    GoodReceivedDetailsId = StockHit.Id,
                    PurchaseOrderID = PurchaseOrderMaster.PurchaseOrderId,
                    Qty = StockHit.Qty,
                    UserId = people.PeopleID,
                    WarehouseId = PurchaseOrderMaster.WarehouseId,
                    ManualReason = Desc,
                    IsFreeStock = StockHit.IsFreeItem,
                    ExpiryQty = StockHit.ExpiryQty,
                    DamageQty = StockHit.DamageQty
                });

                foreach (var item in StockHit.GRBatchs.Where(c => c.IsDeleted == false))
                {

                    if (StockHit.Qty > 0 && StockHit.IsFreeItem == false)
                    {
                        result.OnGRNBatchCodeApproveList.Add(new DataContracts.BatchCode.BatchCodeSubject
                        {
                            ObjectDetailId = item.Id,
                            ObjectId = item.GoodRecievedDetailId,
                            ItemMultiMrpId = StockHit.ItemMultiMRPId,
                            Quantity = item.Qty,
                            WarehouseId = PurchaseOrderMaster.WarehouseId,
                            TransactionDate = indianTime,
                            TransactionType = "CurrentGRN"
                        });
                    }
                    else if (StockHit.Qty > 0 && StockHit.IsFreeItem)
                    {

                        result.OnGRNBatchCodeApproveList.Add(new DataContracts.BatchCode.BatchCodeSubject
                        {
                            ObjectDetailId = item.Id,
                            ObjectId = item.GoodRecievedDetailId,
                            ItemMultiMrpId = StockHit.ItemMultiMRPId,
                            Quantity = item.Qty,
                            WarehouseId = PurchaseOrderMaster.WarehouseId,
                            TransactionDate = indianTime,
                            TransactionType = "FreeGRN"
                        });

                    }
                    if (StockHit.DamageQty > 0)
                    {

                        result.OnGRNBatchCodeApproveList.Add(new DataContracts.BatchCode.BatchCodeSubject
                        {
                            ObjectDetailId = item.Id,
                            ObjectId = item.GoodRecievedDetailId,
                            ItemMultiMrpId = StockHit.ItemMultiMRPId,
                            Quantity = item.DamageQty,
                            WarehouseId = PurchaseOrderMaster.WarehouseId,
                            TransactionDate = indianTime,
                            TransactionType = "DamageGRN"
                        });

                    }
                    if (StockHit.ExpiryQty > 0)
                    {

                        result.OnGRNBatchCodeApproveList.Add(new DataContracts.BatchCode.BatchCodeSubject
                        {
                            ObjectDetailId = item.Id,
                            ObjectId = item.GoodRecievedDetailId,
                            ItemMultiMrpId = StockHit.ItemMultiMRPId,
                            Quantity = item.ExpiryQty,
                            WarehouseId = PurchaseOrderMaster.WarehouseId,
                            TransactionDate = indianTime,
                            TransactionType = "ExpiryGRN"
                        });
                    }
                }
            }


            if (OnGRNRequestCStockList.Any())
            {

                bool res = MultiStockHelpers.MakeEntry(OnGRNRequestCStockList, "Stock_OnGRNApprove", context, dbContextTransaction);
                if (!res)
                {
                    result.OnGRNBatchCodeApproveList = null;
                    result.IsSuccess = false;
                    return result;
                }

                #region Insert in FIFO

                //if (ConfigurationManager.AppSettings["LiveFIFO"] == "1")
                //{
                //    List<GrDC> items = GoodsReceivedDetail.Where(x => (x.Qty > 0) && x.Status == 2).Select(x => new GrDC
                //    {
                //        ItemMultiMrpId = x.ItemMultiMRPId,
                //        WarehouseId = PurchaseOrderMaster.WarehouseId,
                //        Source = "PO",
                //        CreatedDate = indianTime,
                //        POId = PurchaseOrderMaster.PurchaseOrderId,
                //        Qty = x.Qty,
                //        Price = x.Price
                //    }).ToList();

                //    foreach (var item in items)
                //    {
                //        RabbitMqHelper rabbitMqHelper = new RabbitMqHelper();
                //        rabbitMqHelper.Publish("GR", item);
                //    }
                //}

                #endregion

            }
            #endregion
            if (issuccess)
            {
                if (context.Commit() > 0)
                {
                    result.IsSuccess = true;
                }
                else
                {
                    result.OnGRNBatchCodeApproveList = null;
                    result.IsSuccess = false;
                }
            }
            return result;
        }

        #region old code
        /// <summary>
        /// Created By Raj
        /// Created Date:11/11/2019
        /// Approved GR  by GR Approver
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        //[Route("ApprovedPoGr/V1")]
        //[HttpPost]
        //[AllowAnonymous]
        //public HttpResponseMessage ApprovedPoGrV1(POApproverDc POApproverDc)
        //{
        //    string resultMessage = "";
        //    bool IsValidate = false;
        //    bool status = false;
        //    using (var dbContextTransaction = new TransactionScope())
        //    {
        //        using (AuthContext context = new AuthContext())
        //        {
        //            int UserId = POApproverDc.UserId;
        //            var people = context.Peoples.Where(x => x.PeopleID == UserId && x.Deleted == false && x.Active).SingleOrDefault();
        //            PurchaseOrderMaster PurchaseOrderMaster = context.DPurchaseOrderMaster.Where(x => x.PurchaseOrderId == POApproverDc.PurchaseOrderId && (x.Status == "UN Received" || x.Status == "UN Partial Received")).Include(x => x.PurchaseOrderDetail).SingleOrDefault();
        //            if (PurchaseOrderMaster != null && people != null)
        //            {
        //                var Gr = context.GoodsReceivedDetail.Where(x => x.PurchaseOrderDetail.PurchaseOrderId == PurchaseOrderMaster.PurchaseOrderId && x.IsDeleted == false && x.Status == 1).Include(x => x.PurchaseOrderDetail).ToList();
        //                if (Gr != null && Gr.Any())
        //                {
        //                    PurchaseOrderMaster.Comment = PurchaseOrderMaster.Comment + Environment.NewLine + POApproverDc.POApproveStatus;
        //                    IsValidate = true;
        //                    if (IsValidate && POApproverDc.GrSerialNumber > 0)
        //                    {

        //                        bool result = GRNGoodsReceivedDetailsRecivedIn(PurchaseOrderMaster, Gr, POApproverDc.GrSerialNumber, people, context);
        //                        if (result)
        //                        {
        //                            var supplier = context.Suppliers.Where(a => a.SupplierId == PurchaseOrderMaster.SupplierId).SingleOrDefault();



        //                            ///Send SMS & Notification To Supplier
        //                            var grAmount = Gr.Sum(x => x.Qty * x.Price);

        //                            Sms s = new Sms();
        //                            string msg = "ShopKirana has received goods for amount " + Math.Round(grAmount, 2) + " against  PO #: " + PurchaseOrderMaster.PurchaseOrderId + " In Hub:" + PurchaseOrderMaster.WarehouseName; ;
        //                            string Mob = supplier?.MobileNo;
        //                            string FCMID = supplier?.fcmId;
        //                            string FCMNotification = "{title:'New Good Received',body:'" + msg + "',icon:'',notify_type:'GR',ObjectId:" + PurchaseOrderMaster.PurchaseOrderId + "}";

        //                            if (!string.IsNullOrEmpty(Mob)) { s.sendOtp(Mob, msg); }
        //                            if (!string.IsNullOrEmpty(FCMID)) { s.SupplierSendNotification(FCMID, FCMNotification); }

        //                            resultMessage = "GR Appoved Succesfully, Will reflect into Stock";
        //                            status = true;
        //                            dbContextTransaction.Complete();
        //                        }
        //                        else
        //                        {
        //                            dbContextTransaction.Dispose();
        //                            resultMessage = "Something went wrong";
        //                            status = false;

        //                        }
        //                    }
        //                    else { resultMessage = "Already gr posted or may be under in RejectGR(will be approved through backend portal)"; status = false; }

        //                }

        //            }
        //            else { resultMessage = "Something went wrong"; status = false; }

        //            var res = new
        //            {
        //                POGRDetail = POApproverDc,
        //                status = status,
        //                Message = resultMessage

        //            };
        //            return Request.CreateResponse(HttpStatusCode.OK, res);

        //        }
        //    }
        //}

        //public bool GRNGoodsReceivedDetailsRecivedIn(PurchaseOrderMaster PurchaseOrderMaster, List<GoodsReceivedDetail> GoodsReceivedDetail, int GrSerialNumber, People people, AuthContext context)
        //{

        //    var PoItemMultimrpids = new List<int>();
        //    var FIItemMultimrpids = new List<int>();
        //    var FreeStokList = new List<FreeStock>();
        //    var FreeMRPList = new List<ItemMultiMRP>();
        //    List<TemporaryCurrentStock> UpdateTemporaryCurrentStock = new List<TemporaryCurrentStock>();// UpdateTemporaryCurrentStock stock
        //    List<TemporaryCurrentStockHistory> AddTemporaryCurrentStockHistory = new List<TemporaryCurrentStockHistory>();// AddTemporaryCurrentStockHistory Histpry
        //    List<CurrentStock> UpdateCurrentStock = new List<CurrentStock>();// UpdateCurrentStock
        //    List<CurrentStockHistory> AddCurrentStockHistory = new List<CurrentStockHistory>();// AddCurrentStockHistory History 
        //    List<FreeStock> AddFreeStock = new List<FreeStock>();// UpdateFreeStock
        //    List<FreeStock> UpdateFreeStock = new List<FreeStock>();// UpdateFreeStock
        //    List<FreeStockHistory> AddFreeStockHistory = new List<FreeStockHistory>();// FreeStockHistory

        //    var PoInventoryManagerNew = new PoInventoryManagerNew();
        //    PoItemMultimrpids = GoodsReceivedDetail.Where(x => x.IsFreeItem == false).Select(x => x.ItemMultiMRPId).Distinct().ToList();
        //    var TemporaryCurrentStockList = context.TemporaryCurrentStockDB.Where(x => PoItemMultimrpids.Contains(x.ItemMultiMRPId) && x.WarehouseId == PurchaseOrderMaster.WarehouseId && x.Deleted == false).ToList(); // ItemMultiMRPList
        //    var CurrentStockList = context.DbCurrentStock.Where(x => PoItemMultimrpids.Contains(x.ItemMultiMRPId) && x.WarehouseId == PurchaseOrderMaster.WarehouseId && x.Deleted == false).ToList(); // ItemMultiMRPList
        //    var ItemMultiMRPList = context.ItemMultiMRPDB.Where(x => PoItemMultimrpids.Contains(x.ItemMultiMRPId) && x.Deleted == false).ToList(); // ItemMultiMRPList

        //    FIItemMultimrpids = GoodsReceivedDetail.Where(x => x.IsFreeItem == true).Select(x => x.ItemMultiMRPId).Distinct().ToList();//
        //    FreeStokList = context.FreeStockDB.Where(x => FIItemMultimrpids.Contains(x.ItemMultiMRPId) && x.WarehouseId == PurchaseOrderMaster.WarehouseId && x.Deleted == false).ToList();
        //    FreeMRPList = context.ItemMultiMRPDB.Where(x => FIItemMultimrpids.Contains(x.ItemMultiMRPId)).ToList();

        //    //KKMateg
        //    var NumberList = ItemMultiMRPList.Select(x => x.ItemNumber).Distinct().ToList();
        //    var ItemmastersList = context.itemMasters.Where(x => NumberList.Contains(x.Number) && x.WarehouseId == PurchaseOrderMaster.WarehouseId && x.SubcategoryName.ToUpper().Equals("KISAN KIRANA") && x.Deleted == false).ToList();

        //    foreach (var k in GoodsReceivedDetail.Where(x => x.IsFreeItem == false))
        //    {
        //        var Tcs = TemporaryCurrentStockList.Where(x => x.ItemMultiMRPId == k.ItemMultiMRPId).SingleOrDefault();
        //        TemporaryCurrentStockHistory Tcsh = new TemporaryCurrentStockHistory();
        //        if (Tcs != null)
        //        {
        //            Tcsh.StockId = Tcs.Id;
        //            Tcsh.ItemNumber = Tcs.ItemNumber;
        //            Tcsh.itemname = Tcs.itemname;
        //            Tcsh.OdOrPoId = PurchaseOrderMaster.PurchaseOrderId;

        //            Tcsh.CurrentInventory = Tcs.CurrentInventory;
        //            Tcsh.InventoryOut = k.Qty;
        //            Tcsh.TotalCurrentInventory = Tcs.CurrentInventory - k.Qty;

        //            Tcsh.ExpInventoryOut = k.ExpiryQty;
        //            Tcsh.TotalExpCurrentInventory = Tcs.ExpCurrentInventory - k.ExpiryQty;

        //            Tcsh.DamageInventoryOut = k.DamageQty;
        //            Tcsh.TotalDamageCurrentInventory = Tcs.DamageCurrentInventory - k.DamageQty;

        //            Tcsh.WarehouseName = Tcs.WarehouseName;
        //            Tcsh.Warehouseid = Tcs.WarehouseId;
        //            Tcsh.CompanyId = Tcs.CompanyId;
        //            Tcsh.CreationDate = indianTime;
        //            Tcsh.userid = people.PeopleID;
        //            Tcsh.ItemMultiMRPId = Tcs.ItemMultiMRPId;
        //            AddTemporaryCurrentStockHistory.Add(Tcsh);

        //            Tcs.CurrentInventory = Tcs.CurrentInventory - Convert.ToInt32(k.Qty);
        //            Tcs.DamageCurrentInventory = Tcs.CurrentInventory - Convert.ToInt32(k.DamageQty);
        //            Tcs.ExpCurrentInventory = Tcs.CurrentInventory - Convert.ToInt32(k.Qty);
        //            UpdateTemporaryCurrentStock.Add(Tcs);

        //            /// Add Stock in current stock from Temporary current stock
        //            var item = CurrentStockList.Where(x => x.ItemMultiMRPId == k.ItemMultiMRPId).SingleOrDefault();
        //            var MRPdetail = ItemMultiMRPList.Where(a => a.ItemMultiMRPId == k.ItemMultiMRPId).SingleOrDefault();
        //            if (item == null)
        //            {
        //                CurrentStock NewStock = new CurrentStock();
        //                NewStock.CompanyId = Tcs.CompanyId;
        //                NewStock.CreationDate = indianTime;
        //                NewStock.CurrentInventory = 0;
        //                NewStock.Deleted = false;
        //                NewStock.ItemMultiMRPId = k.ItemMultiMRPId;
        //                NewStock.itemname = Tcs.itemname;
        //                NewStock.ItemNumber = Tcs.ItemNumber;
        //                NewStock.itemBaseName = Tcs.itemBaseName;
        //                NewStock.UpdatedDate = indianTime;
        //                NewStock.WarehouseId = Tcs.WarehouseId;
        //                NewStock.WarehouseName = Tcs.WarehouseName;
        //                NewStock.MRP = MRPdetail.MRP;
        //                NewStock.UOM = MRPdetail.UOM;
        //                NewStock.userid = people.PeopleID;
        //                context.DbCurrentStock.Add(NewStock);

        //                context.Commit();

        //                item = NewStock;
        //            }
        //            // Recall
        //            CurrentStockHistory Oss = new CurrentStockHistory();
        //            if (item != null)
        //            {
        //                Oss.StockId = item.StockId;
        //                Oss.ItemNumber = item.ItemNumber;
        //                Oss.itemname = item.itemname;
        //                Oss.OdOrPoId = PurchaseOrderMaster.PurchaseOrderId;
        //                Oss.CurrentInventory = item.CurrentInventory;
        //                Oss.InventoryIn = Convert.ToInt32(k.Qty);
        //                Oss.TotalInventory = item.CurrentInventory + Convert.ToInt32(k.Qty);
        //                Oss.WarehouseName = item.WarehouseName;
        //                Oss.Warehouseid = item.WarehouseId;
        //                Oss.CompanyId = item.CompanyId;
        //                Oss.CreationDate = indianTime;
        //                Oss.ManualReason = "(+)Stock GRN: " + k.GrSerialNumber;
        //                Oss.ItemMultiMRPId = item.ItemMultiMRPId;
        //                Oss.MRP = item.MRP;
        //                Oss.UOM = item.UOM;
        //                Oss.userid = people.PeopleID;
        //                Oss.UserName = people.DisplayName;
        //                AddCurrentStockHistory.Add(Oss);

        //                item.CurrentInventory = item.CurrentInventory + Convert.ToInt32(k.Qty);
        //                UpdateCurrentStock.Add(item);
        //                //Method Add For Packing Material Updation

        //                if (ItemmastersList.Any(x => x.Number == item.ItemNumber))
        //                {

        //                    bool Res = UpdateMaterialItemReceivedDetails(k.Qty, Tcs.UOM, PurchaseOrderMaster.PurchaseOrderId, Tcs.ItemNumber, context, GetUserId());

        //                }

        //            }


        //        }
        //        k.Status = 2;
        //        k.ApprovedBy = people.PeopleID;
        //        k.ModifiedDate = indianTime;
        //        k.CreatedDate = indianTime;

        //    }

        //    if (FIItemMultimrpids.Count > 0)
        //    {
        //        foreach (var f in GoodsReceivedDetail.Where(x => x.IsFreeItem == true))
        //        {
        //            var stok = FreeStokList.Where(x => x.ItemMultiMRPId == f.ItemMultiMRPId).FirstOrDefault();
        //            var MRP = FreeMRPList.Where(a => a.ItemMultiMRPId == f.ItemMultiMRPId).SingleOrDefault();
        //            if (stok != null)
        //            {


        //                FreeStockHistory Oss = new FreeStockHistory();
        //                Oss.ManualReason = "Free Item";
        //                Oss.FreeStockId = stok.FreeStockId;
        //                Oss.ItemMultiMRPId = stok.ItemMultiMRPId;
        //                Oss.ItemNumber = stok.ItemNumber;
        //                Oss.itemname = stok.itemname;
        //                Oss.OdOrPoId = PurchaseOrderMaster.PurchaseOrderId;
        //                Oss.CurrentInventory = stok.CurrentInventory;
        //                Oss.InventoryIn = f.Qty;
        //                Oss.TotalInventory = Convert.ToInt32(stok.CurrentInventory + f.Qty);
        //                Oss.WarehouseId = stok.WarehouseId;
        //                Oss.CreationDate = DateTime.Now;
        //                AddFreeStockHistory.Add(Oss);
        //                stok.CurrentInventory = stok.CurrentInventory + f.Qty;
        //                if (stok.CurrentInventory < 0)
        //                {
        //                    stok.CurrentInventory = 0;
        //                }
        //                UpdateFreeStock.Add(stok);
        //            }
        //            else
        //            {
        //                FreeStock FSN = new FreeStock();
        //                FSN.itemname = PurchaseOrderMaster.PurchaseOrderDetail.Any(p => p.PurchaseOrderDetailId == f.PurchaseOrderDetailId) ? PurchaseOrderMaster.PurchaseOrderDetail.FirstOrDefault(p => p.PurchaseOrderDetailId == f.PurchaseOrderDetailId).ItemName : " ";
        //                FSN.ItemNumber = MRP.ItemNumber;
        //                FSN.ItemMultiMRPId = f.ItemMultiMRPId;
        //                FSN.MRP = MRP.MRP;
        //                FSN.WarehouseId = Convert.ToInt32(PurchaseOrderMaster.WarehouseId);
        //                FSN.CurrentInventory = f.Qty;
        //                FSN.CreatedBy = people.DisplayName;
        //                FSN.CreationDate = indianTime;
        //                FSN.Deleted = false;
        //                FSN.UpdatedDate = indianTime;
        //                //AddFreeStock.Add(FSN);
        //                context.FreeStockDB.Add(FSN);
        //                context.Commit();


        //                FreeStockHistory Oss = new FreeStockHistory();
        //                Oss.ManualReason = "Free Item";
        //                Oss.FreeStockId = FSN.FreeStockId;
        //                Oss.ItemMultiMRPId = FSN.ItemMultiMRPId;
        //                Oss.ItemNumber = FSN.ItemNumber;
        //                Oss.itemname = FSN.itemname;
        //                Oss.OdOrPoId = PurchaseOrderMaster.PurchaseOrderId;
        //                Oss.CurrentInventory = f.Qty;
        //                Oss.InventoryIn = f.Qty;
        //                Oss.TotalInventory = Convert.ToInt32(FSN.CurrentInventory);
        //                Oss.WarehouseId = FSN.WarehouseId;
        //                Oss.CreationDate = DateTime.Now;
        //                AddFreeStockHistory.Add(Oss);
        //            }

        //            f.Status = 2;
        //            f.ApprovedBy = people.PeopleID;
        //            f.ModifiedDate = indianTime;
        //            f.CreatedDate = indianTime;
        //        }
        //    }

        //    #region Update Inventory
        //    PoInventoryManagerNew.HitRecordInCurrentStock(context, UpdateCurrentStock, AddCurrentStockHistory, UpdateTemporaryCurrentStock, AddTemporaryCurrentStockHistory, AddFreeStock, UpdateFreeStock, AddFreeStockHistory);

        //    #endregion
        //    if (PurchaseOrderMaster.Status == "UN Partial Received")
        //    {
        //        PurchaseOrderMaster.Status = "CN Partial Received";
        //    }
        //    else if (PurchaseOrderMaster.Status == "UN Received")
        //    {
        //        PurchaseOrderMaster.Status = "CN Received";
        //    }
        //    context.Entry(PurchaseOrderMaster).State = EntityState.Modified;

        //    if (context.Commit() > 0)
        //    { return true; }
        //    else { return false; }
        //}
        #endregion
        public bool UpdateMaterialItemReceivedDetails(int Quantity, string Uom, int PurchaseOrderId, string ItemNumber, AuthContext db, TransactionScope DbTransactionScope, int UpdatedBy)
        {
            bool Result = true;
            RawMaterialRepository objRawMaterialRepository = new RawMaterialRepository(db);

            Result = false;
            Result = objRawMaterialRepository.UpdateItemReceivedmaterialDetails(Quantity, Uom, PurchaseOrderId, ItemNumber, UpdatedBy);

            return Result;
        }
        #region update PoInventoryManagerNew
        public class PoInventoryManagerNew
        {
            public void HitRecordInCurrentStock(AuthContext context, List<CurrentStock> UpdateCurrentStock, List<CurrentStockHistory> AddCurrentStockHistory, List<TemporaryCurrentStock> UpdateTemporaryCurrentStock, List<TemporaryCurrentStockHistory> AddTemporaryCurrentStockHistory, List<FreeStock> AddFreeStock, List<FreeStock> UpdateFreeStock, List<FreeStockHistory> AddFreeStockHistory)
            {

                foreach (var TemporaryCurrentStock in UpdateTemporaryCurrentStock)
                {

                    context.Entry(TemporaryCurrentStock).State = EntityState.Modified;
                }
                foreach (var CurrentStock in UpdateCurrentStock)
                {
                    context.Entry(CurrentStock).State = EntityState.Modified;
                }
                foreach (var item in UpdateFreeStock)
                {
                    context.Entry(item).State = EntityState.Modified;
                }
                context.TemporaryCurrentStockHistoryDB.AddRange(AddTemporaryCurrentStockHistory);

                context.CurrentStockHistoryDb.AddRange(AddCurrentStockHistory);
                //context.FreeStockDB.AddRange(AddFreeStock);
                context.FreeStockHistoryDB.AddRange(AddFreeStockHistory);
            }
        }
        #endregion
        #endregion

        #region Reject RejectPoGrV1
        [Route("RejectPoGr/V1")]
        [HttpPost]
        [AllowAnonymous]
        public HttpResponseMessage RejectPoGrV1(POApproverDc POApproverDc)
        {
            string resultMessage = "";
            bool status = true;
            TransactionOptions option = new TransactionOptions();
            option.IsolationLevel = IsolationLevel.RepeatableRead;
            option.Timeout = TimeSpan.FromSeconds(90);
            using (var dbContextTransaction = new TransactionScope(TransactionScopeOption.Required, option))
            {
                using (AuthContext context = new AuthContext())
                {
                    int UserId = POApproverDc.UserId;
                    var people = context.Peoples.Where(x => x.PeopleID == UserId && x.Deleted == false && x.Active).SingleOrDefault();
                    var PurchaseOrderMaster = context.DPurchaseOrderMaster.Where(x => x.PurchaseOrderId == POApproverDc.PurchaseOrderId && (x.Status == "UN Received" || x.Status == "UN Partial Received") && (x.Status != "Auto Closed" || x.Status != "Closed")).SingleOrDefault();

                    if (people != null && people.PeopleID > 0 && PurchaseOrderMaster != null && PurchaseOrderMaster.Gr_Process == true)
                    {
                        PurchaseOrderMaster.Gr_Process = false;
                        context.Entry(PurchaseOrderMaster).State = EntityState.Modified;
                        context.Commit();

                        GrQualityInvoice grinvoice = new GrQualityInvoice();
                        List<GrQualityInvoice> listgrinvoice = new List<GrQualityInvoice>();
                        if (POApproverDc.IsQualityTesting == true)
                        {
                            grinvoice = context.GrQualityInvoices.FirstOrDefault(x => x.Id == POApproverDc.GrInvoiceId);
                            if (grinvoice != null)
                            {
                                if (grinvoice.Isview == false)
                                {
                                    var result = new
                                    {
                                        status = false,
                                        Message = "First View Document"

                                    };
                                    return Request.CreateResponse(HttpStatusCode.OK, result);
                                }
                            }
                        }

                        var Gr = context.GoodsReceivedDetail.Where(x => x.PurchaseOrderDetail.PurchaseOrderId == PurchaseOrderMaster.PurchaseOrderId && x.IsDeleted == false && x.Status == 1 && x.GrSerialNumber == POApproverDc.GrSerialNumber).Include(x => x.PurchaseOrderDetail).Include(x => x.GRBatchs).ToList();
                        if (Gr != null && Gr.Any())
                        {
                            foreach (var item in Gr)
                            {
                                item.Status = 3;//3=Reject
                                item.ModifiedBy = people.PeopleID;
                                item.ModifiedDate = indianTime;
                                item.Comment = POApproverDc.POApproveStatus;//Reeject Commet
                                foreach (var Bitem in item.GRBatchs)
                                {
                                    Bitem.IsDeleted = true;
                                    Bitem.IsActive = false;
                                    //Bitem.ModifiedBy = people.PeopleID;
                                    //Bitem.ModifiedDate = indianTime;
                                }
                                context.Entry(item).State = EntityState.Modified;
                            }
                            //if(POApproverDc.IsQualityTesting == true)
                            //{
                            List<long> grList = Gr.Select(x => x.Id).Distinct().ToList();
                            listgrinvoice = context.GrQualityInvoices.Where(x => grList.Contains(x.GoodsReceivedDetailId) && x.IsActive == true && x.IsDeleted == false).ToList();
                            if (listgrinvoice != null && listgrinvoice.Any())
                            {
                                foreach (var gr in listgrinvoice)
                                {
                                    //if(gr.GrQualityCheckerId==POApproverDc.UserId)
                                    //{
                                    gr.IsApproved = 2; //2 Rejected
                                    gr.ModifiedBy = people.PeopleID;
                                    gr.ModifiedDate = indianTime;
                                    gr.IsActive = false;
                                    gr.IsDeleted = true;
                                    if (gr.Id == POApproverDc.GrInvoiceId)
                                    {
                                        gr.Comment = POApproverDc.POApproveStatus;
                                    }
                                    context.Entry(gr).State = EntityState.Modified;
                                    //}
                                }
                                //}
                            }


                            #region InReceivedStocks to GrReject before approved Stock_OnGRNPendingReject
                            //
                            MultiStockHelper<OnGRNPendingRejectDc> MultiStockHelpers = new MultiStockHelper<OnGRNPendingRejectDc>();
                            List<OnGRNPendingRejectDc> GRNPendingRejectCStockList = new List<OnGRNPendingRejectDc>();
                            foreach (var StockHit in Gr.Where(x => (x.Qty > 0 || x.DamageQty > 0 || x.ExpiryQty > 0 || x.ShortQty > 0)))
                            {
                                GRNPendingRejectCStockList.Add(new OnGRNPendingRejectDc
                                {
                                    ItemMultiMRPId = StockHit.ItemMultiMRPId,
                                    GoodReceivedDetailsId = StockHit.Id,
                                    PurchaseOrderID = PurchaseOrderMaster.PurchaseOrderId,
                                    Qty = StockHit.Qty,
                                    UserId = people.PeopleID,
                                    WarehouseId = PurchaseOrderMaster.WarehouseId,
                                    IsFreeStock = StockHit.IsFreeItem,
                                    DamageQty = StockHit.DamageQty,
                                    ExpiryQty = StockHit.ExpiryQty,
                                    ShortQty = StockHit.ShortQty
                                });
                            }
                            if (GRNPendingRejectCStockList.Any())
                            {

                                bool result = MultiStockHelpers.MakeEntry(GRNPendingRejectCStockList, "Stock_OnGRNPendingReject", context, dbContextTransaction);
                                if (!result)
                                {
                                    status = false;
                                }
                            }


                            #endregion
                            if (status)
                            {
                                #region GDN Reject 
                                GDNHelper helper = new GDNHelper();
                                helper.RemoveGDN(PurchaseOrderMaster.PurchaseOrderId, UserId, POApproverDc.GrSerialNumber, context);
                                #endregion
                                context.Commit();
                                dbContextTransaction.Complete();
                                resultMessage = POApproverDc.GrSerialNumber + " Gr Reject Successfully";
                            }
                            else
                            {
                                dbContextTransaction.Dispose();
                                resultMessage = "due to stock issue";
                                status = false;
                            }

                        }
                    }
                    else { resultMessage = "something went wrong"; }
                    var res = new
                    {

                        status = status,
                        Message = resultMessage

                    };
                    return Request.CreateResponse(HttpStatusCode.OK, res);
                }
            }
        }


        #region old code

        //public HttpResponseMessage RejectPoGrV1(POApproverDc POApproverDc)
        //{
        //    string resultMessage = "";
        //    bool status = false;
        //    using (AuthContext context = new AuthContext())
        //    using (var dbContextTransaction = context.Database.BeginTransaction())
        //    {
        //        int UserId = POApproverDc.UserId;
        //        var people = context.Peoples.Where(x => x.PeopleID == UserId && x.Deleted == false && x.Active).SingleOrDefault();
        //        var PurchaseOrderMaster = context.DPurchaseOrderMaster.Where(x => x.PurchaseOrderId == POApproverDc.PurchaseOrderId && (x.Status == "UN Received" || x.Status == "UN Partial Received")).SingleOrDefault();

        //        if (people != null && people.PeopleID > 0 && PurchaseOrderMaster != null)
        //        {
        //            var Gr = context.GoodsReceivedDetail.Where(x => x.PurchaseOrderDetail.PurchaseOrderId == PurchaseOrderMaster.PurchaseOrderId && x.IsDeleted == false && x.Status == 1 && x.GrSerialNumber == POApproverDc.GrSerialNumber).Include(x => x.PurchaseOrderDetail).ToList();
        //            if (Gr != null && Gr.Any())
        //            {
        //                foreach (var item in Gr)
        //                {
        //                    item.Status = 3;//3=Reject
        //                    item.ModifiedBy = people.PeopleID;
        //                    item.ModifiedDate = indianTime;
        //                    item.CreatedDate = indianTime;
        //                    item.Comment = POApproverDc.POApproveStatus;//Reeject Commet
        //                    context.Entry(item).State = EntityState.Modified;
        //                }

        //                if (context.Commit() > 0)
        //                {
        //                    dbContextTransaction.Commit();
        //                    status = true;
        //                    resultMessage = POApproverDc.GrSerialNumber + " Gr Reject Successfully";
        //                }
        //                else
        //                {
        //                    resultMessage = "something went wrong";
        //                    dbContextTransaction.Rollback();
        //                    status = false;
        //                }
        //            }
        //        }
        //        var res = new
        //        {

        //            status = status,
        //            Message = resultMessage

        //        };
        //        return Request.CreateResponse(HttpStatusCode.OK, res);

        //    }
        //}
        #endregion
        #endregion

        #region Provide PO Details
        [Route("getPODetails/V1")]
        [HttpGet]
        public PurchaseOrderNewDc GetPODetailsV1(int PurchaseOrderId)
        {
            var result = new PurchaseOrderNewDc();
            using (AuthContext context = new AuthContext())
            {
                var PurchaseOrderMaster = context.DPurchaseOrderMaster.Where(x => x.PurchaseOrderId == PurchaseOrderId).Include(x => x.PurchaseOrderDetail).SingleOrDefault();
                result = Mapper.Map(PurchaseOrderMaster).ToANew<PurchaseOrderNewDc>();
                result.PurchaseOrderDetail = Mapper.Map(PurchaseOrderMaster.PurchaseOrderDetail).ToANew<List<PurchaseOrderDetailNewDc>>();
                if (result.PurchaseOrderDetail != null && result.PurchaseOrderDetail.Any())
                {
                    foreach (var item in result.PurchaseOrderDetail)
                    {
                        if (item.Price == 0.01) { item.IsFreeItem = true; } else { item.IsFreeItem = false; }
                    }
                }
                return result;
            }
        }
        #endregion


        #region Provide GDN Details
        [Route("getGDNDetails/V1")]
        [HttpGet]
        [AllowAnonymous]
        public HttpResponseMessage getGDNDetailsV1(int PurchaseOrderId, int GRNumber)
        {
            bool status = false;
            string resultMessage = "";
            var result = new GoodsDescripancyNoteMasterDC();
            using (AuthContext context = new AuthContext())
            {
                var GoodsDescripancyNoteMaster = context.GoodsDescripancyNoteMasterDB.Where(x => x.PurchaseOrderId == PurchaseOrderId && x.GrSerialNo == GRNumber && x.IsActive == true && x.IsDeleted == false).Include(x => x.goodsDescripancyNoteDetail).FirstOrDefault();

                result = Mapper.Map(GoodsDescripancyNoteMaster).ToANew<GoodsDescripancyNoteMasterDC>();
                if (result != null)
                {
                    if (result.goodsDescripancyNoteDetail.Any() || result.goodsDescripancyNoteDetail != null)
                    {
                        foreach (var item in result.goodsDescripancyNoteDetail)
                        {
                            var gr = context.GoodsReceivedDetail.Where(x => x.Id == item.GoodsReceivedDetailId && x.GrSerialNumber == GRNumber).Include(x => x.PurchaseOrderDetail).ToList();
                            item.ItemName = gr.Select(x => x.PurchaseOrderDetail.ItemName).FirstOrDefault();
                        }
                    }
                    status = true; resultMessage = "Record found";
                }
                else
                {
                    status = false; resultMessage = "Record not found";
                }
            }
            var res = new
            {
                GDNDetails = result,
                status = status,
                Message = resultMessage
            };
            return Request.CreateResponse(HttpStatusCode.OK, res);
        }
        #endregion
        [Route("GetGDNdetails")]
        [HttpGet]
        [AllowAnonymous]
        public List<GDNDC> GetGDNdetails(string GDNid)
        {
            var result = new List<GDNDC>();
            try
            {
                GDNHelper helper = new GDNHelper();
                result = helper.GetGDNdetails(GDNid);
            }
            catch (Exception x)
            {
                result = null;
            }
            return result;
        }
        [Route("GetGDNdetailOnIR")]
        [HttpGet]
        [AllowAnonymous]
        public List<GDNDC> GetGDNdetailOnIR(long Poid, int GrSNo)
        {
            var result = new List<GDNDC>();
            try
            {
                GDNHelper helper = new GDNHelper();
                result = helper.GetGDNdetailOnIR(Poid, GrSNo);
            }
            catch (Exception x)
            {
                result = null;
            }
            return result;
        }

        [Route("ValidateOtp")]
        [HttpGet]
        [AllowAnonymous]
        public bool ValidateOtp(int otp, int Poid)
        {
            bool valid = false;
            try
            {
                GDNHelper helper = new GDNHelper();
                valid = helper.ValidateOtp(otp, Poid);
            }
            catch (Exception x)
            {
                valid = false;
            }
            return valid;
        }

        [Route("GDNAction")]
        [HttpPost]
        [AllowAnonymous]
        public bool GDNAction(GNDActionDC dC)
        {
            bool valid = false;
            try
            {
                GDNHelper helper = new GDNHelper();
                valid = helper.SupplierAction(dC, GetUserId());
            }
            catch (Exception x)
            {
            }
            return valid;
        }

        #region GetAllSarthiNotification
        [Route("GetallSarthiNotification")]
        [HttpGet]
        [AllowAnonymous]
        public PaggingDatas GetallSarthiNotification(int skip, int take, int PeopleId)
        {
            int ApproveTimeLeft = Convert.ToInt32(ConfigurationManager.AppSettings["ApproveNotifyTimeLeftInMinute"]);

            using (var context = new AuthContext())
            {
                DateTime dt1 = DateTime.Now;
                PaggingDatas data = new PaggingDatas();
                context.Database.CommandTimeout = 600;
                SarthiManager manager = new SarthiManager();
                skip = (take - 1) * skip;
                var PeopleSentNotificationDc = manager.GetPeopleSentSarthiNotificationDetail(skip, take, PeopleId);
                PeopleSentNotificationDc.ForEach(x =>
                {
                    x.TimeLeft = x.TimeLeft.AddMinutes(ApproveTimeLeft); // from Create date

                    if (!string.IsNullOrEmpty(x.Shopimage) && !x.Shopimage.Contains("http"))
                    {
                        x.Shopimage = string.Format("{0}://{1}{2}/{3}", new Uri((HttpContext.Current.Request.UrlReferrer != null ? HttpContext.Current.Request.UrlReferrer.AbsoluteUri : HttpContext.Current.Request.Url.AbsoluteUri)).Scheme
                                                              , HttpContext.Current.Request.Url.DnsSafeHost
                                                              , (HttpContext.Current.Request.Url.Port != 80 && HttpContext.Current.Request.Url.Port != 443 ? ":" + HttpContext.Current.Request.Url.Port : "")
                                                              , x.Shopimage);
                    }
                });
                data.notificationmaster = PeopleSentNotificationDc;
                data.total_count = PeopleSentNotificationDc != null && PeopleSentNotificationDc.Any() ? PeopleSentNotificationDc.FirstOrDefault().TotalCount : 0;
                return data;
            }
        }

        #endregion






        private int GetUserId()
        {
            var identity = User.Identity as ClaimsIdentity;
            int userid = 0;
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
            return userid;
        }
        #endregion

        #region Customer Address on sarthi app

        [HttpGet]
        [Route("GetClustersByWidForDispSupwiser/{WarehouseId}/{UserId}")]
        public async Task<List<WarehouseClusterDc>> GetClustersByWidForDispSupwiser(int WarehouseId, int UserId)
        {

            var result = new List<WarehouseClusterDc>();
            if (WarehouseId > 0 && UserId > 0)
            {
                using (AuthContext db = new AuthContext())
                {
                    var UserIdparam = new SqlParameter("@UserId", UserId);
                    var WarehouseIdparam = new SqlParameter("@WarehouseId", WarehouseId);
                    result = await db.Database.SqlQuery<WarehouseClusterDc>("exec GetClustersByWidForDispSupwiser  @UserId, @WarehouseId", UserIdparam, WarehouseIdparam).ToListAsync();
                }
            }
            return result;
        }


        [HttpPost]
        [Route("ClusterCustomersOnSarthiApp")]
        public async Task<List<CustomerCluster>> GetClusterCustomersOnSarthiApp(List<int> clusterids)
        {
            using (AuthContext db = new AuthContext())
            {
                List<CustomerCluster> CustomerClusters = new List<CustomerCluster>();
                if (clusterids.Any())
                {

                    var clusteridslist = new System.Data.DataTable();
                    clusteridslist.Columns.Add("IntValue");
                    foreach (var item in clusterids)
                    {
                        var dr = clusteridslist.NewRow();
                        dr["IntValue"] = item;
                        clusteridslist.Rows.Add(dr);
                    }
                    var CIds = new SqlParameter("Clusterids", clusteridslist);
                    CIds.SqlDbType = System.Data.SqlDbType.Structured;
                    CIds.TypeName = "dbo.IntValues";
                    CustomerClusters = await db.Database.SqlQuery<CustomerCluster>("exec GetByClusteridsCustomersOnSarthi  @Clusterids", CIds).ToListAsync();
                }
                return CustomerClusters;
            }
        }

        [HttpPost]
        [Route("CustomerPhysicalAddress")]
        public bool CustomerPhysicalAddress(CustomerLatLngVerify Customer)
        {
            bool result = false;
            if (Customer != null)
            {
                using (AuthContext db = new AuthContext())
                {
                    var cust = db.Customers.FirstOrDefault(x => x.CustomerId == Customer.CustomerId);
                    if (cust != null)
                    {
                        var customerLatLngVerify = db.CustomerLatLngVerify.FirstOrDefault(x => x.CustomerId == Customer.CustomerId && x.AppType == (int)AppEnum.SarthiApp);
                        Customer.IsActive = true;
                        Customer.IsDeleted = false;
                        if (customerLatLngVerify == null)
                        {
                            Customer.Status = 1;  // 1 Request
                            Customer.CreatedDate = DateTime.Now;
                            Customer.AppType = (int)AppEnum.SarthiApp;
                            db.CustomerLatLngVerify.Add(Customer);
                        }
                        else
                        {
                            customerLatLngVerify.CaptureImagePath = Customer.CaptureImagePath;
                            customerLatLngVerify.NewShippingAddress = Customer.NewShippingAddress;
                            customerLatLngVerify.Newlat = Customer.Newlat;
                            customerLatLngVerify.Newlg = Customer.Newlg;
                            customerLatLngVerify.ShopFound = Customer.ShopFound;
                            customerLatLngVerify.Status = 1;    // 1 Request
                            customerLatLngVerify.Aerialdistance = Customer.Aerialdistance;
                            customerLatLngVerify.ModifiedDate = DateTime.Now;
                            customerLatLngVerify.IsActive = true;
                            customerLatLngVerify.IsDeleted = false;
                            customerLatLngVerify.ModifiedBy = Customer.CreatedBy;
                            //mbd
                            customerLatLngVerify.AreaName = Customer.AreaName;
                            if (!customerLatLngVerify.CreatedBy.HasValue)
                                customerLatLngVerify.CreatedBy = Customer.CreatedBy;
                            db.Entry(customerLatLngVerify).State = EntityState.Modified;
                        }
                        result = db.Commit() > 0;
                    }
                }
            }
            return result;
        }


        #endregion

        #region GR Quality Approval
        [Route("QualityApprovalItemImageUpload")]
        [HttpPost]
        [AllowAnonymous]
        public HttpResponseMessage QualityApprovalItemImage()
        {
            string LogoUrl = "";
            HttpResponseMessage result = null;
            try
            {
                if (HttpContext.Current.Request.Files.AllKeys.Any())
                {
                    var httpRequest = System.Web.HttpContext.Current.Request;
                    HttpFileCollection uploadFiles = httpRequest.Files;
                    var docfiles = new List<string>();
                    if (httpRequest.Files.Count > 0)
                    {
                        int i;
                        int cnt = 1;
                        var arr1 = httpRequest.Files.AllKeys;
                        for (i = 0; i < uploadFiles.Count; i++)
                        {
                            if (!Directory.Exists(HttpContext.Current.Server.MapPath("~/QualityApprovalItemImage")))
                                Directory.CreateDirectory(HttpContext.Current.Server.MapPath("~/QualityApprovalItemImage"));

                            HttpPostedFile postedFile = uploadFiles[i];

                            var name = arr1[i].ToString();
                            string extension = Path.GetExtension(postedFile.FileName);
                            string fileName = name + DateTime.Now.ToString("ddMMyyyyHHmmss") + cnt.ToString() + extension;
                            LogoUrl = Path.Combine(HttpContext.Current.Server.MapPath("~/QualityApprovalItemImage"), fileName);

                            postedFile.SaveAs(LogoUrl);

                            AngularJSAuthentication.Common.Helpers.FileUploadHelper.Upload(fileName, "~/QualityApprovalItemImage", LogoUrl);
                            LogoUrl = "/QualityApprovalItemImage/" + fileName;
                            docfiles.Add(LogoUrl);
                            cnt++;
                        }

                        result = Request.CreateResponse(HttpStatusCode.Created, docfiles);
                    }
                    else
                    {
                        result = Request.CreateResponse(HttpStatusCode.BadRequest);
                    }
                }
                return result;
            }
            catch (Exception ex)
            {
                logger.Error("Error in QualityApprovalItemImage Method: " + ex.Message);
                return null;
            }
        }

        [HttpPost]
        [AllowAnonymous]
        [Route("IsViewGrQuality")]
        public APIResponse IsViewGrQuality(int iD)
        {
            APIResponse res = new APIResponse();
            var identity = User.Identity as ClaimsIdentity;
            int userid = identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid")
                ? int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value)
                : 0;

            using (var context = new AuthContext())
            {
                if (iD > 0)
                {
                    var data = context.GrQualityInvoices.FirstOrDefault(x => x.Id == iD);
                    if (data != null)
                    {
                        if (data.Isview == true)
                        {
                            res.Status = true;
                            res.Message = "Already View";
                        }
                        else
                        {
                            data.Isview = true;
                            data.ModifiedBy = userid;
                            data.ModifiedDate = DateTime.Now;
                            context.Entry(data).State = EntityState.Modified;
                        }
                    }
                    else
                    {
                        res.Status = false;
                        res.Message = "Data Not Found.";
                    }
                }
                if (context.Commit() > 0)
                {
                    res.Status = true;
                    res.Message = "View Successfully";
                }
                else
                {
                    res.Status = false;
                    res.Message = "Something went wrong!!";
                }
            }
            return res;
        }

        [HttpPost]
        [AllowAnonymous]
        [Route("IsApproveGrQuality")]
        public APIResponse IsApproveGrQuality(List<long> iD, int userid, int PurchaseOrderId, int GrSerialNumber)
        {
            APIResponse res = new APIResponse();
            var identity = User.Identity as ClaimsIdentity;
            using (var context = new AuthContext())
            {
                if (iD != null && iD.Any())
                {
                    string pquery = "select gr.Status from PurchaseOrderDetails pd inner join GoodsReceivedDetails gr on pd.PurchaseOrderDetailId = gr.PurchaseOrderDetailId" +
                        " where  gr.Status=3 and pd.PurchaseOrderId = " + PurchaseOrderId + "and gr.GrSerialNumber = " + GrSerialNumber;
                    var rejectedGr = context.Database.SqlQuery<int>(pquery).ToList();
                    if (rejectedGr != null && rejectedGr.Any())
                    {
                        res.Status = false;
                        res.Message = "GR Already Rejected.";
                        return res;
                    }
                    else
                    {
                        var people = context.Peoples.Where(x => x.PeopleID == userid && x.Deleted == false && x.Active).SingleOrDefault();
                        var Lists = context.GrQualityInvoices.Where(x => iD.Contains(x.Id) && x.Isview == true && x.IsActive == true && x.IsDeleted == false && x.IsApproved == 0).ToList();
                        //var data = context.GrQualityInvoices.FirstOrDefault(x => x.Id == iD && x.Isview == true);
                        if (Lists != null && Lists.Any())
                        {
                            foreach (var data in Lists)
                            {
                                if (data.GrQualityCheckerId == userid)
                                {
                                    data.IsApproved = 1;
                                    data.ModifiedBy = people.PeopleID;
                                    data.ModifiedDate = indianTime;
                                    context.Entry(data).State = EntityState.Modified;
                                }
                            }
                        }
                        else
                        {
                            res.Status = false;
                            res.Message = "Please View Document.";
                            return res;
                        }
                    }
                }
                if (context.Commit() > 0)
                {
                    res.Status = true;
                    res.Message = "Approved Successfully";
                }
                else
                {
                    res.Status = false;
                    res.Message = "Something went wrong!!";
                }
            }
            return res;
        }

        [AllowAnonymous]
        [Route("GrItemQualitySearch")]
        [HttpGet]
        public List<GrItemQualitySearchDC> GrItemQualitySearch(int userid)
        {
            List<GrItemQualitySearchDC> pm = new List<GrItemQualitySearchDC>();

            using (AuthContext context = new AuthContext())
            {
                var param1 = new SqlParameter("@userid", userid);
                var res = context.Database.SqlQuery<GrItemQualitySearchDC>("exec GrItemQualitySearch @userid", param1).ToList();
                return res;
            }
        }



        private bool ActivateItemForConsumerStore(List<ActivateItemForConsumerStoreOnGRNDc> ActivateItemForConsumerStoreOnGRNs, AuthContext context, int userid)
        {
            List<ItemMaster> NewAdditems = new List<ItemMaster>();
            int WarehouseId = ActivateItemForConsumerStoreOnGRNs.FirstOrDefault().WarehouseId;
            List<int> ItemMultiMrpIds = ActivateItemForConsumerStoreOnGRNs.Select(x => x.ItemMultiMrpId).ToList();

            var ItemMultiMRPlist = context.ItemMultiMRPDB.Where(x => ItemMultiMrpIds.Contains(x.ItemMultiMRPId)).ToList();

            List<string> numbers = ItemMultiMRPlist.Select(x => x.ItemNumber).Distinct().ToList();

            var itemmasterlist = context.itemMasters.Where(x => numbers.Contains(x.Number) && x.WarehouseId == WarehouseId).ToList();

            if (itemmasterlist != null && itemmasterlist.Any(x => ItemMultiMrpIds.Contains(x.ItemMultiMRPId)))
            {
                foreach (var itemmaster in itemmasterlist.Where(x => ItemMultiMrpIds.Contains(x.ItemMultiMRPId) && !x.active))
                {
                    itemmaster.active = true;
                    itemmaster.ModifiedBy = userid;
                    itemmaster.UpdatedDate = indianTime;
                    context.Entry(itemmaster).State = EntityState.Modified;
                }
            }

            var ExistItemMultiMrpId = itemmasterlist.Select(x => x.ItemMultiMRPId).ToList();
            var NotExistItemMultiMrpId = ItemMultiMrpIds.Where(x => !ExistItemMultiMrpId.Contains(x)).Select(x => x).ToList();

            foreach (var ItemMultiMrpId in NotExistItemMultiMrpId.Distinct())
            {
                var mrpobj = ItemMultiMRPlist.FirstOrDefault(x => x.ItemMultiMRPId == ItemMultiMrpId);
                var itemmaster = itemmasterlist.Where(x => x.Number == mrpobj.ItemNumber).FirstOrDefault();
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
                    item.active = false;
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
                    item.CreatedBy = userid;
                    item.SellerStorePrice = item.MRP;
                    item.IsSellerStoreItem = itemmaster.IsSellerStoreItem;
                    NewAdditems.Add(item);
                }
            }
            if (NewAdditems != null && NewAdditems.Any())
            {
                context.itemMasters.AddRange(NewAdditems);
            }
            return context.Commit() > 0;
        }


        [Route("GetPOBarcodeDetails/V1")]
        [HttpGet]
        public APIResponse GetPOBarcodeDetails(int PurchaseOrderId)
        {
            var res = new APIResponse
            {
                Message = "Something went wrong"
            };
            using (AuthContext context = new AuthContext())
            {
                var result = new List<POBarcodeDetailDc>();
                List<string> itemNumbers = context.DPurchaseOrderDeatil.Where(x => x.PurchaseOrderId == PurchaseOrderId).Select(x => x.ItemNumber).Distinct().ToList();
                if (itemNumbers != null)
                {
                    var itembarcodelist = context.ItemBarcodes.Where(c => itemNumbers.Contains(c.ItemNumber) && c.IsActive == true && c.IsDeleted == false).Select(x => new ItemBarcodeDc { ItemNumber = x.ItemNumber, Barcode = x.Barcode }).ToList();
                    if (itemNumbers != null && itemNumbers.Any())
                    {
                        itemNumbers.ForEach(item =>
                        result.Add(new POBarcodeDetailDc
                        {
                            ItemNumber = item,
                            ItemBarcodes = itembarcodelist.Any(x => x.ItemNumber == item) ? itembarcodelist.Where(x => x.ItemNumber == item).Select(x => x.Barcode).Distinct().ToList() : null
                        }));
                        res.Data = result;
                        res.Status = true;
                        res.Message = "Record found";
                    }
                    else
                    {
                        res.Message = "No Barcode found";
                    }
                }
                return res;
            }
        }


        #endregion  GR Quality Approval

        public class POBarcodeDetailDc
        {
            public string ItemNumber { get; set; }
            public List<string> ItemBarcodes { get; set; }
        }




        public class ActivateItemForConsumerStoreOnGRNDc
        {
            public int ItemMultiMrpId { get; set; }
            public int WarehouseId { get; set; }

        }




        public class GrItemQualitySearchDC
        {
            public int PurchaseOrderId { get; set; }
            public string PoType { get; set; }
            public double ETotalAmount { get; set; }
            public long GrNo { get; set; }
            public int GrNoGrSerialNumber { get; set; }
            public string PoCreatedBy { get; set; }
            public string GrCreatedBy { get; set; }
            public DateTime grDate { get; set; }
            public long grDetailId { get; set; }
            public long GrQualityInvoiceId { get; set; }
        }
        public class classification
        {
            public string itemNumber { get; set; }
            public int warehouseid { get; set; }
            public string Category { get; set; }
            public int Qty { get; set; }

        }
        public class GNDActionDC
        {
            public int PurchaseOrderId { get; set; }
            public long GDNId { get; set; }
            public string status { get; set; }
            public string Comment { get; set; }
        }
        public class GDNDC
        {
            public int PurchaseOrderId { get; set; }
            public string suppliername { get; set; }
            public string WarehouseName { get; set; }
            public string InvoiceDate { get; set; }
            public string InvoiceNumber { get; set; }
            public string GDNNumber { get; set; }
            public string SKUDescription { get; set; }
            public int ShortQty { get; set; }
            public int DamageQty { get; set; }
            public int ExpiryQty { get; set; }
            public int ReturnQty { get; set; }
            public decimal CostPrice { get; set; }
            public decimal Total { get; set; }
            public decimal TotalTaxPercentage { get; set; }
            public long GDNId { get; set; }
            public string GNDStatus { get; set; }
            public int IssueDamageQty { get; set; }
            public int IssueExpiryQty { get; set; }
            public int IssueShortQty { get; set; }
        }
    }


}

