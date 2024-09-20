using AngularJSAuthentication.API.ControllerV7.PurchaseRequestPayments;
using AngularJSAuthentication.Model;
using AngularJSAuthentication.Model.PurchaseOrder;
using AngularJSAuthentication.Model.PurchaseRequestPayments;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace AngularJSAuthentication.API.Helper.AdjustmentPO
{
    public class AdjustmentPOHelper
    {
        public int Adjustment(List<PRPaymentDc> settlePoList, double totalAmt, int PurchaseOrderId, int userid)
        {
            int result = 0;
            using (var context = new AuthContext())
            {
                PRPaymentSummary summary = new PRPaymentSummary()
                {
                    Createby = userid,
                    Deleted = false,
                    IsActive = false,
                    PaymentDate = DateTime.Now,
                    TotalAmount = totalAmt,
                    Updateby = null,
                    UpdateDate = null,
                    IsPROutstandingPending = true
                };

                context.PRPaymentSummaryDB.Add(summary);
                if (settlePoList != null && settlePoList.Any())
                {

                    var poids = String.Join(",", settlePoList.Select(x => x.PurchaseOrderId));
                    var poidsParam = new SqlParameter
                    {
                        ParameterName = "@PurchaseOrderId",
                        Value = poids
                    };
                    var settlePaymentList = context.Database.SqlQuery<PRPaymentDc>("GetClosedPOPaymentList @PurchaseOrderId", poidsParam).ToList();
                    if (settlePaymentList != null && settlePaymentList.Any())
                    {
                        settlePaymentList = settlePaymentList.OrderBy(x => x.Total).ToList();
                        var SettleAmt = settlePaymentList.Sum(x => x.Total);

                        if (totalAmt < SettleAmt)
                        {
                            SettleAmt = totalAmt;
                        }

                        while (SettleAmt > 0)
                        {
                            PRPaymentDc settlePayment = settlePaymentList.Where(x => x.Total > 0).FirstOrDefault();
                            double transferredAmount = SettleAmt > settlePayment.Total ? settlePayment.Total : SettleAmt;
                            settlePayment.Total -= transferredAmount;
                            PrPaymentTransfer prPaymentTransfer = new PrPaymentTransfer
                            {
                                IsActive = true,
                                IsDeleted = false,
                                CreatedBy = userid,
                                CreatedDate = DateTime.Now,
                                FromPurchaseOrderId = settlePayment.PurchaseOrderId,
                                ModifiedBy = null,
                                ModifiedDate = null,
                                SettledAmount = 0,
                                SourcePurchaseRequestPaymentId = settlePayment.SourcePurchaseRequestPaymentId,
                                ToPurchaseOrderId = PurchaseOrderId,
                                TransferredAmount = transferredAmount
                            };

                            PrPaymentTransfer oldPRPaymentTransfer = context.PrPaymentTransferDB.First(x => x.Id == settlePayment.PRPaymentTransferId);
                            oldPRPaymentTransfer.OutAmount = oldPRPaymentTransfer.OutAmount.HasValue ? (oldPRPaymentTransfer.OutAmount + prPaymentTransfer.TransferredAmount) : prPaymentTransfer.TransferredAmount;
                            SettleAmt -= prPaymentTransfer.TransferredAmount;
                            context.PrPaymentTransferDB.Add(prPaymentTransfer);
                            result = context.Commit();
                            ClosedPOEntry(settlePayment, PurchaseOrderId, userid, transferredAmount);
                        }
                    }
                }
            }
            return result;
        }

        public void ClosedPOEntry(PRPaymentDc settlePayment, int PurchaseOrderId, int userid, double transferredAmount)
        {
            using (var context = new AuthContext())
            {

                AdjustmentPODetail detail = new AdjustmentPODetail()
                {
                    Amount = transferredAmount,
                    ClosedPOId = settlePayment.PurchaseOrderId,
                    PurchaseRequestId = PurchaseOrderId,
                    RemaningAmount = settlePayment.Total,// RemainingAmt,
                    InsertedBy = userid.ToString(),
                    InsertedDate = DateTime.Now,
                    IsActive = true

                };
                context.AdjustmentPODetails.Add(detail);
                context.Commit();
            }
        }

    }
}