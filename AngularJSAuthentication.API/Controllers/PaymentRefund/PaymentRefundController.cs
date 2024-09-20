using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Http;
using AngularJSAuthentication.API.Helper.PaymentRefund;
using AngularJSAuthentication.Common.Enums;
using AngularJSAuthentication.DataContracts.Masters.PaymentRefund;
using AngularJSAuthentication.Model.PaymentRefund;
using GenricEcommers.Models;

namespace AngularJSAuthentication.API.Controllers.PaymentRefund
{
    [RoutePrefix("api/PaymentRefund")]
    public class PaymentRefundController : ApiController
    {
        [Route("OrderPaymentRefund")]
        [HttpGet]
        public async Task<bool> OrderPaymentRefundJob()
        {
            PaymentRefundHelper helper = new PaymentRefundHelper();
            return await helper.OrderPaymentRefundJob();
        }

        [Route("GetPaymentRefundList")]
        [HttpPost]
        public async Task<PaymentRefundResponse> GetPaymentRefundList(SearchPaymentRefundDc obj)
        {
            PaymentRefundResponse res = new PaymentRefundResponse();
            if (obj != null)
            {
                using (var context = new AuthContext())
                {
                    string whereclause = " p.IsDeleted=0 ";

                    if (obj.RefundType >= 0)
                    {
                        whereclause += " and p.RefundType = " + obj.RefundType;
                    }

                    if (obj.status >= 0)
                    {
                        whereclause += " and p.status = " + obj.status;
                    }

                    if (obj.WarehouseIds != null && obj.WarehouseIds.Any())
                    {
                        whereclause += " and o.WarehouseId in (" + string.Join(",", obj.WarehouseIds.Select(n => n.ToString()).ToArray()) + ")";
                    }
                    if (obj.FromDate.HasValue && obj.ToDate.HasValue)
                    {
                        whereclause += " and CAST(p.CreatedDate as Date) between '" + obj.FromDate.Value.ToString("MM-dd-yyyy HH:mm:ss") + "' and '" + obj.ToDate.Value.ToString("MM-dd-yyyy HH:mm:ss") + "' ";
                    }
                    if (obj.Keyward != null && obj.Keyward != "")
                    {
                        whereclause += " and (o.OrderId like '%" + obj.Keyward.Trim() + "%' or o.Skcode like '%" + obj.Keyward.Trim() + "%' )";
                    }
                    if (obj.MOPs != null && obj.MOPs.Any())
                    {
                        whereclause += " and p.Source in (" + string.Format("'{0}'", string.Join("','", obj.MOPs.Select(i => i.Replace("'", "''")))) + ")";
                    }
                    int Skiplist = (obj.skip - 1) * obj.take;

                    var param1 = new SqlParameter("@whereclouse", whereclause);
                    var param2 = new SqlParameter("@skip", Skiplist);
                    var param3 = new SqlParameter("@take", obj.take);

                    var list = await context.Database.SqlQuery<PaymentRefundListDc>("SPPaymentRefundDetail  @whereclouse,@skip,@take", param1, param2, param3).ToListAsync();
                    string sqlcount = "select count(*) as TotalCount from PaymentRefundRequests p join OrderMasters o on o.OrderId = p.OrderId where " + whereclause;
                    int totalcount = 0;
                    if (list != null && list.Any())
                    {
                        totalcount = await context.Database.SqlQuery<int>(sqlcount).FirstOrDefaultAsync();
                        res.PaymentRefundListDcs = list;
                        res.TotalCount = totalcount;
                    }
                }
            }
            return res;
        }

        [Route("GetPaymentRefundHistory/{PaymentRefundRequestId}")]
        [HttpGet]
        public async Task<List<PaymentRefundHistoryDc>> PaymentRefundHistory(long PaymentRefundRequestId)
        {
            var res = new List<PaymentRefundHistoryDc>();
            if (PaymentRefundRequestId > 0)
            {
                using (var context = new AuthContext())
                {
                    var param1 = new SqlParameter("@PaymentRefundRequestId", PaymentRefundRequestId);
                    res = await context.Database.SqlQuery<PaymentRefundHistoryDc>("SPPaymentRefundHistory  @PaymentRefundRequestId", param1).ToListAsync();
                }
            }
            return res;
        }

        [Route("PaymentRefundExport")]
        [HttpPost]
        public async Task<PaymentRefundResponse> PaymentRefundExport(SearchPaymentRefundExportDc obj)
        {

            PaymentRefundResponse res = new PaymentRefundResponse();
            using (var context = new AuthContext())
            {
                string whereclause = " p.IsDeleted=0 ";

                //whereclause += " and p.RefundType = " + obj.RefundType;
                if (obj.status >= 0)
                {
                    whereclause += " and p.status = " + obj.status;
                }

                if (obj.WarehouseIds != null && obj.WarehouseIds.Any())
                {
                    whereclause += " and o.WarehouseId in (" + string.Join(",", obj.WarehouseIds.Select(n => n.ToString()).ToArray()) + ")";
                }
                if (obj.FromDate.HasValue && obj.ToDate.HasValue)
                {
                    whereclause += " and CAST(p.CreatedDate as Date) between '" + obj.FromDate.Value.ToString("MM-dd-yyyy HH:mm:ss") + "' and '" + obj.ToDate.Value.ToString("MM-dd-yyyy HH:mm:ss") + "' ";
                }
                if (obj.Keyward != null && obj.Keyward != "")
                {
                    whereclause += " and (o.OrderId like '%" + obj.Keyward.Trim() + "%' or o.Skcode like '%" + obj.Keyward.Trim() + "%' )";
                }
                if (obj.MOPs != null && obj.MOPs.Any())
                {
                    whereclause += " and p.Source in (" + string.Format("'{0}'", string.Join("','", obj.MOPs.Select(i => i.Replace("'", "''")))) + ")";
                }
                var param = new SqlParameter("@whereclouse", whereclause);
                var list = await context.Database.SqlQuery<PaymentRefundListExportDc>("SPPaymentRefundExport   @whereclouse", param).ToListAsync();
                res.PaymentRefundListExportDcs = list;
                return res;
            }
        }

        [Route("Process")]
        [HttpGet]
        public async Task<PaymentRefundResDc> Process(long PaymentRefundRequestId)
        {
            var identity = User.Identity as ClaimsIdentity;
            int userid = 0;

            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

            PaymentRefundResDc res = new PaymentRefundResDc();
            if (PaymentRefundRequestId > 0 && userid > 0)
            {
                using (var context = new AuthContext())
                {
                    var data = await context.PaymentRefundRequests.Where(x => x.Id == PaymentRefundRequestId).FirstOrDefaultAsync();
                    if (data.Status == 2)
                    {
                        data.Status = 0; // 0 for ReInitiated and Old status 2 :Failed
                        data.ModifiedDate = DateTime.Now;
                        context.Entry(data).State = EntityState.Modified;

                        PaymentRefundHistory addHistory = new PaymentRefundHistory
                        {
                            PaymentRefundRequestId = data.Id,
                            Status = (int)PaymentRefundEnum.Initiated,
                            CreatedDate = DateTime.Now,
                            CreatedBy = userid
                        };
                        context.PaymentRefundHistories.Add(addHistory);
                        res.Status = context.Commit() > 0;
                        res.ResponseMsg = "Request Updated for ReProcess";
                    }
                    else
                    {
                        res.Status = false;
                        res.ResponseMsg = "Request Updated for ReProcess Failed";
                    }
                }
            }
            return res;
        }

        [Route("AddPaymentRefundManualComment/{id}/{Comment}")]
        [HttpGet]
        public bool AddPaymentRefundManualComment(int id, string Comment)
        {
            var identity = User.Identity as ClaimsIdentity;
            int userid = 0;

            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
            DateTime currentTime = DateTime.Now;
            using (var context = new AuthContext())
            {
                var refundRequest = context.PaymentRefundRequests.FirstOrDefault(x => x.Id == id);
                if (refundRequest != null && refundRequest.Status != (int)PaymentRefundEnum.Success)
                {
                    refundRequest.Comment = Comment;
                    refundRequest.ModifiedBy = userid;
                    refundRequest.ModifiedDate = currentTime;
                    refundRequest.Status = (int)PaymentRefundEnum.Success;
                    context.Entry(refundRequest).State = EntityState.Modified;

                    var retailerPaymentResponse = context.PaymentResponseRetailerAppDb.FirstOrDefault(x => x.id == refundRequest.PaymentResponseRetailerAppId);
                    retailerPaymentResponse.ApproveBy = userid;
                    retailerPaymentResponse.IsApproved = 1;
                    retailerPaymentResponse.IsRefund = true;
                    retailerPaymentResponse.statusDesc = "Refund Success";
                    retailerPaymentResponse.ApproveDate = DateTime.Now;

                    context.Entry(retailerPaymentResponse).State = EntityState.Modified;

                    PaymentRefundHistory addHistory = new PaymentRefundHistory
                    {
                        PaymentRefundRequestId = refundRequest.Id,
                        Status = (int)PaymentRefundEnum.Success,
                        ResponseMsg = "Manually Success of amount " + retailerPaymentResponse.amount,
                        RequestMsg = "Manually Process of amount " + retailerPaymentResponse.amount,
                        CreatedDate = DateTime.Now,
                        CreatedBy = userid,
                        Comment = Comment
                    };
                    context.PaymentRefundHistories.Add(addHistory);
                    context.Commit();
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

    }
}

