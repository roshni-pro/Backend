using AngularJSAuthentication.API.Controllers.Base;
using AngularJSAuthentication.API.Helper;
using AngularJSAuthentication.DataContracts.Transaction.PurchaseRequest;
using NPOI.SS.Formula.Functions;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace AngularJSAuthentication.API.ControllerV7.PurchaseRequestPayments
{
    [RoutePrefix("api/PurchaseRequestSettlement")]
    public class PurchaseRequestSettlementController : BaseAuthController
    {
        [Route("GetAdvanceOutstanding")]
        [HttpPost]
        public IHttpActionResult GetAdvanceOutstanding(List<int> supplierIdList)
        {
            using (var context = new AuthContext())
            {
                List<SqlParameter> paramList = new List<SqlParameter>();
                string supplierIdString = string.Join(",", supplierIdList.Select(n => n.ToString()).ToArray());
                paramList.Add(new SqlParameter("@SupplierIdList", supplierIdString));
                var list = context.Database.SqlQuery<AdvanceOutstandingDc>("GetAdvanceOutstanding @SupplierIdList", paramList.ToArray()).ToList();
                //var query = from po in context.DPurchaseOrderMaster
                //            join prp in context.PurchaseRequestPaymentsDB
                //                on new { POId = po.PurchaseOrderId, PrStatus = "Approved" }
                //                equals new { POId = (int)prp.PurchaseOrderId, PrStatus = prp.PrPaymentStatus }
                //            join pps in context.PrPaymentTransferDB 
                //                on new { PrPaymentId = prp.Id, Deleted = false, Active = true }
                //                equals new { PrPaymentId = pps.FromPurchaseRequestPaymentId, Deleted  = ( pps.IsDeleted.HasValue ? pps.IsDeleted.Value : false) , Active = pps.IsActive}
                //                into gj from subpet in gj.DefaultIfEmpty()
                //            where supplierIdList.Contains(po.SupplierId) 
                //                && (prp.PrPaymentStatus == " Closed" || prp.PrPaymentStatus == "Auto Closed")
                //            group new { po.SupplierId, 
                //                prp.PaidAmount,
                //                Amt =gj.Sum(x =>x.TransferredAmount)

                //            } by po.SupplierId into g
                //            select new AdvanceOutstandingDc
                //            {
                //                SupplierId = g.Key,
                //                AdvanceOutstanding = g.Min(x => x.PaidAmount) - g.Sum(y => y.Amt)
                //            };
                //var list = query.ToList();
                return Ok(list);
            }


        }


        [Route("GetAdvanceOutstandingForAdj")]
        [HttpPost]
        public IHttpActionResult GetAdvanceOutstandingForAdj(List<int> supplierIdList)
        {
            using (var context = new AuthContext())
            {
                List<SqlParameter> paramList = new List<SqlParameter>();
                string supplierIdString = string.Join(",", supplierIdList.Select(n => n.ToString()).ToArray());
                paramList.Add(new SqlParameter("@SupplierIdList", supplierIdString));
                
                var list = context.Database.SqlQuery<PRAdvanceOutstandingDc>("GetAdvanceOutstandingofClosedPO @SupplierIdList", paramList.ToArray()).ToList();
                var listnew = list.GroupBy(x => new
                {
                    x.PurchaseOrderId,
                    x.SupplierId,
                    x.BuyerName,
                    x.CreatedBy
                }).Select(x => new PRAdvanceOutstandingDc
                {
                    PurchaseOrderId = x.Key.PurchaseOrderId,
                    SupplierId = x.Key.SupplierId,
                    BuyerName = x.Key.BuyerName,
                    CreatedBy = x.Key.CreatedBy,
                    Total = x.Sum(y => y.Total)
                });
                return Ok(listnew);
            }
        }

        #region old methods
        [HttpPost]
        [Route("GetPageList")]
        public PurchaseRequestSettlementContainerDc GetPageList(PurchaseRequestSettlementFilterDc filter)
        {
            PurchaseRequestSettlementHelper helper = new PurchaseRequestSettlementHelper();
            PurchaseRequestSettlementContainerDc container = helper.GetPageList(filter);
            return container;
        }

        [HttpGet]
        [Route("GetIRList/{supplierId}")]
        public List<RemainingIRAmountDc> GetIRList(int supplierId)
        {
            PurchaseRequestSettlementHelper helper = new PurchaseRequestSettlementHelper();
            List<RemainingIRAmountDc> list = helper.GetIRList(supplierId);
            return list;
        }

        [HttpPost]
        [Route("SettlePayment")]
        public bool SettlePayment(PurchaseRequestPaymentSettlementDc settlement)
        {
            int userId = GetLoginUserId();
            PurchaseRequestSettlementHelper helper = new PurchaseRequestSettlementHelper();
            helper.SettlePayment(settlement, userId);
            return true;
        }
        #endregion

    }

    public class AdvanceOutstandingDc
    {
        public int SupplierId { get; set; }
        public Double? AdvanceOutstanding { get; set; }

    }
    public class PRAdvanceOutstandingDc
    {
        public int PurchaseOrderId { get; set; }
        public int SupplierId { get; set; }
        public long PRPaymentTransferId { get; set; }
        public double Total { get; set; }
        public long SourcePurchaseRequestPaymentId { get; set; }
        public bool check { get; set; }
        public string CreatedBy { get; set; }
        public string BuyerName { get; set; }
    }
}
