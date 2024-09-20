using AngularJSAuthentication.API.Helper;
using AngularJSAuthentication.Controllers;
using AngularJSAuthentication.DataContracts.Transaction.PurchaseOrder;
using AngularJSAuthentication.Model.PurchaseOrder;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace AngularJSAuthentication.API.ControllerV7.Purchase
{
    [RoutePrefix("api/POReturn")]
    public class PurchaseOrderReturnController : BaseApiController
    {
        [Route("Model/{purchaseOrderId}")]
        [HttpGet]
        public IHttpActionResult GetModel(int purchaseOrderId)
        {
            PurchaseOrderReturnDc poReturn = new PurchaseOrderReturnDc();
            using (var context = new AuthContext())
            {


                if (purchaseOrderId > 0)
                {

                    var IRMstersList = context.IRMasterDB.Where(x => x.PurchaseOrderId == purchaseOrderId && (x.IRStatus == "IR Posted" || x.IRStatus == "Pending from Buyer side" || x.IRStatus == "IR Uploaded" || x.IRStatus== "Approved from Buyer side" || x.IRStatus=="Paid" )).ToList();

                    if (IRMstersList != null && IRMstersList.Any())
                    {

                        POReturnPOMasterDC purchaseOrderMaster = context.DPurchaseOrderMaster
                    .Where(x => x.PurchaseOrderId == purchaseOrderId && x.Active == true)
                    .Select(x => new POReturnPOMasterDC
                    {
                        PurchaseOrderId = x.PurchaseOrderId,
                        //CompanyId = x.CompanyId,
                        SupplierId = x.SupplierId,
                        SupplierName = x.SupplierName,
                        WarehouseId = x.WarehouseId,
                        WarehouseName = x.WarehouseName,
                        Status = x.Status,
                        TotalAmount = x.TotalAmount,
                        Acitve = x.Acitve,
                        Advance_Amt = x.Advance_Amt,
                        ETotalAmount = x.ETotalAmount,
                        PoType = x.PoType,
                        Comment = x.Comment,
                        CommentApvl = x.CommentApvl,
                        Commentsystem = x.Commentsystem,
                        progress = x.progress,
                        ApprovedBy = x.ApprovedBy,
                        RejectedBy = x.RejectedBy,
                        BuyerId = x.BuyerId,
                        BuyerName = x.BuyerName,
                        DepoId = x.DepoId,
                        DepoName = x.DepoName,
                        WarehouseCity = x.WarehouseCity,
                        PoInvoiceNo = x.PoInvoiceNo,
                        TransactionNumber = x.TransactionNumber,
                        SupplierStatus = x.SupplierStatus,
                    })
                   .FirstOrDefault();
                        poReturn.POMaster = purchaseOrderMaster;
                        poReturn.PODetailList = context.DPurchaseOrderDeatil
                            .Where(x => x.PurchaseOrderId == purchaseOrderId)
                            .Select(x => new POReturnPODetailDC
                            {
                                PurchaseOrderDetailId = x.PurchaseOrderDetailId,
                                //CompanyId=x.CompanyId,
                                PurchaseOrderId = x.PurchaseOrderId,
                                SupplierId = x.SupplierId,
                                WarehouseId = x.WarehouseId,
                                WarehouseName = x.WarehouseName,
                                SupplierName = x.SupplierName,
                                SellingSku = x.SellingSku,
                                ItemId = x.ItemId,
                                HSNCode = x.HSNCode,
                                SKUCode = x.SKUCode,
                                ItemName = x.ItemName,
                                Price = x.Price,
                                MRP = x.MRP,
                                MOQ = x.MOQ,
                                TotalQuantity = x.TotalQuantity,
                                PurchaseSku = x.PurchaseSku,
                                TaxAmount = x.TaxAmount,
                                TotalAmountIncTax = x.TotalAmountIncTax,
                                Status = x.Status,
                                CreationDate = x.CreationDate,
                                CreatedBy = x.CreatedBy,
                                ConversionFactor = x.ConversionFactor,
                                PurchaseName = x.PurchaseName,
                                PurchaseQty = x.PurchaseQty,
                                ItemMultiMRPId = x.ItemMultiMRPId,
                                DepoId = x.DepoId,
                                DepoName = x.DepoName,
                                ItemNumber = x.ItemNumber,
                                itemBaseName = x.itemBaseName,
                                PurchaseOrderNewId = x.PurchaseOrderNewId,
                                IsDeleted = x.IsDeleted
                            })
                            .ToList();
                        var goodReceivedDetailListQuery = from grd in context.GoodsReceivedDetail
                                                          join pod in context.DPurchaseOrderDeatil
                                                          on grd.PurchaseOrderDetailId equals pod.PurchaseOrderDetailId
                                                          join pr in context.POReturnRequestDb
                                                          on grd.Id equals pr.ItemId into gj
                                                          from subpet in gj.DefaultIfEmpty()
                                                          where grd.IsActive == true
                                                            && grd.IsDeleted == false
                                                            && (string.IsNullOrEmpty(subpet.CancelType) || subpet.CancelType == "GR")
                                                            && (subpet.IsDeleted == false || subpet.IsDeleted == null)
                                                            && (subpet.IsActive == true || subpet.IsActive == null)
                                                            && pod.PurchaseOrderId == purchaseOrderId
                                                          select new POReturnGoodReceivedDetailDC
                                                          {
                                                              GrSerialNumber = grd.GrSerialNumber,
                                                              POReturnRequestApprovedBy = subpet.ApprovedBy,
                                                              POReturnRequestApprovedDate = subpet.ApprovedDate,
                                                              POReturnRequestId = subpet.Id,
                                                              VehicleNumber = grd.VehicleNumber,
                                                              PurchaseOrderDetailId = grd.PurchaseOrderDetailId,


                                                              Id = grd.Id,
                                                              ItemMultiMRPId = grd.ItemMultiMRPId,
                                                              Qty = grd.Qty,
                                                              DamageQty = grd.DamageQty,
                                                              ExpiryQty = grd.ExpiryQty,
                                                              Price = grd.Price,
                                                              Status = grd.Status,
                                                              CurrentStockHistoryId = grd.CurrentStockHistoryId,
                                                              BatchNo = grd.BatchNo,
                                                              MFGDate = grd.MFGDate,
                                                              Barcode = grd.Barcode,
                                                              ApprovedBy = grd.ApprovedBy,
                                                              VehicleType = grd.VehicleType,


                                                              //CreatedDate = grd.CreatedDate,
                                                              //ModifiedDate = grd.ModifiedDate,
                                                              //IsActive = grd.IsActive,
                                                              //IsDeleted = grd.IsDeleted,
                                                              //CreatedBy = grd.CreatedBy,
                                                              //ModifiedBy = grd.ModifiedBy,
                                                              IsFreeItem = grd.IsFreeItem,
                                                              Comment = grd.Comment,
                                                              ItemName = pod.ItemName
                                                          };
                        poReturn.GoodReceivedDetailList = goodReceivedDetailListQuery.OrderBy(x => x.GrSerialNumber).ToList();
                        var invoiceReceiptDetailQuery = from ird in context.InvoiceReceiptDetail
                                                        join irm in context.IRMasterDB
                                                        on ird.IRMasterId equals irm.Id
                                                        join po in context.DPurchaseOrderMaster
                                                        on irm.PurchaseOrderId equals po.PurchaseOrderId
                                                        join grd in context.GoodsReceivedDetail
                                                        on ird.GoodsReceivedDetailId equals grd.Id
                                                        join pod in context.DPurchaseOrderDeatil
                                                         on grd.PurchaseOrderDetailId equals pod.PurchaseOrderDetailId
                                                        join pr in context.POReturnRequestDb
                                                        on new { IRMasterId = ird.IRMasterId, Status = "Pending" }
                                                        equals new { IRMasterId = (int)pr.ItemId, Status = pr.Status }
                                                        into gj
                                                        from subpet in gj.DefaultIfEmpty()
                                                        where irm.PurchaseOrderId == purchaseOrderId
                                                        && irm.Deleted == false && ird.IsDeleted==false
                                                        && (string.IsNullOrEmpty(subpet.CancelType) || subpet.CancelType == "IR")
                                                        && (subpet.IsDeleted == false || subpet.IsDeleted == null)
                                                        && (subpet.IsActive == true || subpet.IsActive == null)
                                                        select new POReturnInvoiceReceiptDetailDc
                                                        {
                                                            ItemMultiMRPId = grd.ItemMultiMRPId,
                                                            ItemName = pod.ItemName,
                                                            Id = ird.Id,
                                                            IRMasterId = ird.IRMasterId,
                                                            GoodsReceivedDetailId = ird.GoodsReceivedDetailId,
                                                            IRQuantity = ird.IRQuantity,
                                                            Price = ird.Price,
                                                            DiscountPercent = ird.DiscountPercent,
                                                            DiscountAmount = ird.DiscountAmount,
                                                            GSTPercentage = ird.GSTPercentage,
                                                            TotalTaxPercentage = ird.TotalTaxPercentage,
                                                            CessTaxPercentage = ird.CessTaxPercentage,
                                                            GSTAmount = ird.GSTAmount,
                                                            TotalTaxAmount = ird.TotalTaxAmount,
                                                            CessTaxAmount = ird.CessTaxAmount,
                                                            Status = ird.Status,
                                                            ApprovedBy = ird.ApprovedBy,
                                                            IRStatus = irm.IRStatus,
                                                            Comment = ird.Comment,
                                                            IRID= irm.IRID,
                                                            InvoiceDate=irm.InvoiceDate,
                                                            POReturnRequestApprovedBy = subpet.ApprovedBy,
                                                            POReturnRequestApprovedDate = subpet.ApprovedDate,
                                                            POReturnRequestId = subpet.Id,
                                                            POReturnRequestStatus = subpet.Status,
                                                            PurchaseOrderId = irm.PurchaseOrderId,
                                                            WarehouseName = po.WarehouseName
                                                        };
                        poReturn.InvoiceReceiptDetailList = invoiceReceiptDetailQuery.ToList();
                    }
                }
                else { return null; }
            }
            return Ok(poReturn);
        }

        [Route("MakeCancelRequest")]
        [HttpPost]
        public POReturnRequestDc MakeCancelRequest(POReturnRequestDc returnRequest)
        {
            int userid = GetLoginUserId();
            PurchaseOrderReturnHelper helper = new PurchaseOrderReturnHelper();
            returnRequest = helper.MakeCancelRequest(returnRequest, userid);
            return returnRequest;
        }

        [Route("ApproveCancelRequest")]
        [HttpPost]
        public string ApproveCancelRequest([FromBody] long poReturnRequestId)
        {
            int userid = GetLoginUserId();
            PurchaseOrderReturnHelper helper = new PurchaseOrderReturnHelper();
            string message = helper.ApproveCancelRequest(poReturnRequestId, userid);
            return message;
        }


        [Route("RejectCancelRequest")]
        [HttpPost]
        public string RejectCancelRequest([FromBody] long poReturnRequestId)
        {
            int userid = GetLoginUserId();
            PurchaseOrderReturnHelper helper = new PurchaseOrderReturnHelper();
            string message = helper.RejectCancelRequest(poReturnRequestId, userid);
            return message;
        }

        [Route("IsIRExistsForGR/{purchaseOrderId}/{serialNumber}")]
        [HttpGet]
        public bool IsIRExistsForGR(int purchaseOrderId, int serialNumber)
        {
            using (var context = new AuthContext())
            {
                var quer = from ir in context.InvoiceReceiptDetail
                           join gr in context.GoodsReceivedDetail
                           on ir.GoodsReceivedDetailId equals gr.Id
                           join irm in context.IRMasterDB
                           on ir.IRMasterId equals irm.Id
                           where irm.PurchaseOrderId == purchaseOrderId
                           && gr.GrSerialNumber == serialNumber
                           && ir.IsActive == true
                           && (ir.IsDeleted == null || ir.IsDeleted == false)
                           select ir;
                if (quer.Any())
                {
                    return true;
                }
                else
                {
                    return false;
                }

            }
        }

        [Route("IsGRExistsForPO/{purchaseOrderMasterId}")]
        [HttpGet]
        public bool IsGRExistsForPO(long purchaseOrderMasterId)
        {
            using (var context = new AuthContext())
            {
                var quer = from grd in context.GoodsReceivedDetail
                           join pod in context.DPurchaseOrderDeatil
                           on grd.PurchaseOrderDetailId equals pod.PurchaseOrderDetailId
                           where pod.PurchaseOrderId == purchaseOrderMasterId
                           && grd.IsActive == true
                           && (grd.IsDeleted == null || grd.IsDeleted == false)
                           select grd;
                if (quer.Any())
                {
                    return true;
                }
                else
                {
                    return false;
                }

            }
        }


        [Route("GetList")]
        [HttpPost]
        public POPagerDC GetList(POReturnRequestPager pager)
        {
            POPagerDC poReturnRequest = new POPagerDC();
            int userid = GetLoginUserId();
            using (var context = new AuthContext())
            {
                if (pager.CancelType == "PO")
                {
                    context.Database.Log = log => Debug.WriteLine(log);
                    var query = from porr in context.POReturnRequestDb
                                join pom in context.DPurchaseOrderMaster
                                on porr.ItemId equals pom.PurchaseOrderId
                                join reqppl in context.Peoples
                                on porr.CreatedBy equals reqppl.PeopleID
                                where porr.CancelType == "PO"
                                select new POReturnRequestPageDc
                                {
                                    POReturnRequestStatus = porr.Status,
                                    POReturnRequestId = porr.Id,
                                    ItemId = porr.ItemId,
                                    RequestedBy = reqppl.Email,
                                    RequestedDate = reqppl.CreatedDate,
                                    ApprovedBy = porr.ApprovedBy == 0 ? null : context.Peoples.Where(x => x.PeopleID == porr.ApprovedBy).Select(x => x.Email).FirstOrDefault(),
                                    ApprovedDate = porr.ApprovedDate,
                                    CancelType = porr.CancelType,
                                    Amount = pom.TotalAmount,
                                    Status = pom.Status,

                                };
                    poReturnRequest.recordCount = query.Count();
                    poReturnRequest.RequestReturnList = query.OrderByDescending(x => x.POReturnRequestId).Skip(pager.Skip).Take(pager.Take).ToList();
                    return poReturnRequest;
                }
                else if (pager.CancelType == "GR")
                {
                    var query = from porr in context.POReturnRequestDb
                                join pom in context.DPurchaseOrderMaster
                                on new { purchaseOrderId = porr.PurchaseOrderId, CancelType = porr.CancelType }
                                equals new { purchaseOrderId = pom.PurchaseOrderId, CancelType = "GR" }
                                join pod in context.DPurchaseOrderDeatil
                                on pom.PurchaseOrderId equals pod.PurchaseOrderId
                                join grd in context.GoodsReceivedDetail
                                on new { serialNumber = (int)porr.ItemId, purchaseOrderDetailId = pod.PurchaseOrderDetailId }
                                equals new { serialNumber = grd.GrSerialNumber, purchaseOrderDetailId = grd.PurchaseOrderDetailId }
                                join reqppl in context.Peoples
                                on porr.CreatedBy equals reqppl.PeopleID
                                where porr.CancelType == "GR"
                                group new { porr, pom, reqppl, grd } by new
                                {
                                    porr.Id,
                                    pom.PurchaseOrderId,
                                    grd.GrSerialNumber
                                } into g
                                select new POReturnRequestPageDc
                                {

                                    POReturnRequestStatus = g.Min(x => x.porr.Status),
                                    POReturnRequestId = g.Key.Id,
                                    ItemId = g.Min(x => x.porr.Id),
                                    RequestedBy = g.Min(x => x.reqppl.Email),
                                    RequestedDate = g.Min(x => x.reqppl.CreatedDate),
                                    ApprovedBy = g.Min(x=>x.porr.ApprovedBy == 0 ? null : context.Peoples.Where(t => t.PeopleID == g.Min(y=>y.porr.ApprovedBy)).Select(z => z.Email).FirstOrDefault()),
                                    ApprovedDate = g.Min(x => x.porr.ApprovedDate),
                                    CancelType = g.Min(x => x.porr.CancelType),
                                    Amount = g.Sum(x => (x.grd.Price * x.grd.Qty)),
                                    //Status = g.Min(x => x.grd.Status)
                                };
                    poReturnRequest.recordCount = query.Count();
                    poReturnRequest.RequestReturnList = query.OrderByDescending(x => x.POReturnRequestId).Skip(pager.Skip).Take(pager.Take).ToList();
                    return poReturnRequest;
                }
                else if (pager.CancelType == "IR")
                {
                    var query = from porr in context.POReturnRequestDb
                                join irm in context.IRMasterDB
                                on new { Id = (int)porr.ItemId, CancelType = porr.CancelType }
                                equals new { Id = irm.Id, CancelType = "IR" }
                                join reqppl in context.Peoples
                                on porr.CreatedBy equals reqppl.PeopleID
                                select new POReturnRequestPageDc
                                {

                                    POReturnRequestStatus = porr.Status,
                                    POReturnRequestId = porr.Id,
                                    ItemId = porr.ItemId,
                                    RequestedBy = reqppl.Email,
                                    RequestedDate = porr.RequestDate,
                                    ApprovedBy = porr.ApprovedBy == 0 ? null : context.Peoples.Where(x => x.PeopleID == porr.ApprovedBy).Select(x => x.Email).FirstOrDefault(),
                                    ApprovedDate = porr.ApprovedDate,
                                    CancelType = porr.CancelType,
                                    Amount = irm.TotalAmount,
                                    Status = irm.IRStatus,
                                    RefNumber = irm.InvoiceNumber
                                };
                    poReturnRequest.recordCount = query.Count();

                    //var poReturnRequestList = query.ToList().GroupBy(test => test.POReturnRequestId)
                    // .Select(grp => grp.First());

                    poReturnRequest.RequestReturnList = query.OrderByDescending(x => x.POReturnRequestId).Skip(pager.Skip).Take(pager.Take).ToList();
                    return poReturnRequest;
                }
                else
                {
                    return null;
                }


            }
        }
    }
}
