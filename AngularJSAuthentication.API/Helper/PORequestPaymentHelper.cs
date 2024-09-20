using AngularJSAuthentication.DataContracts.Transaction.PurchaseOrder;
using AngularJSAuthentication.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AngularJSAuthentication.API.Helper
{
    public class PORequestPaymentHelper
    {

        public PORequestPaymentPage GetPageData(PORequestPager pager)
        {
            PORequestPaymentPage page = new PORequestPaymentPage();

            using (var context = new AuthContext())
            {
                var query = from po in context.DPurchaseOrderMaster
                            where po.IsPaymentDone == true 
                                    && (!pager.SupplierId.HasValue || po.SupplierId == pager.SupplierId)
                                    && (!pager.StartDate.HasValue 
                                                    || !pager.EndDate.HasValue 
                                                    || (po.CreationDate.Date >= pager.StartDate.Value.Date && po.CreationDate.Date <= pager.EndDate.Value.Date)
                                   )
                            select new PORequestPaymentDisplayDc
                            {
                                PurchaseOrderId = po.PurchaseOrderId,
                                TotalAmount = (int)po.TotalAmount,
                                CreationDate = po.CreationDate,
                                SupplierName = po.SupplierName
                            };

                page.RowList = query.Skip(pager.Skip).Take(pager.Take).ToList();
                page.TotalRecordCount = query.Count();
            }

            return page;
        }

        public void MakePayment(PORequestPaymentDc payment, int userid)
        {
            using (var context = new AuthContext())
            {

                if (payment != null && payment.IRPaymentDetailList != null && payment.IRPaymentDetailList.Count > 0)
                {
                    string guid = Guid.NewGuid().ToString();
                    DateTime currentDate = DateTime.Now;
                    foreach (var paymentDetail in payment.IRPaymentDetailList)
                    {
                        PurchaseOrderMaster purchaseOrder
                            = context.DPurchaseOrderMaster.First(x => x.PurchaseOrderId == paymentDetail.PurchaseOrderId);


                        IRPaymentDetails detail = new IRPaymentDetails();

                        detail.BankId = (int)payment.BankLedgerId;
                        detail.Guid = guid;
                        detail.PaymentDate = payment.PaymentDate;
                        detail.RefNo = payment.RefNo;
                        detail.Remark = payment.Remark;
                        detail.SupplierId = purchaseOrder.SupplierId;
                        detail.TotalAmount = paymentDetail.TotalAmount;
                        detail.TotalReaminingAmount = 0;

                        detail.Createby = userid;
                        detail.CreatedDate = currentDate;
                        detail.Deleted = false;
                        detail.IsActive = true;

                        context.IRPaymentDetailsDB.Add(detail);
                        context.Commit();

                        IRPaymentDetailHistory history = new IRPaymentDetailHistory();
                        history.IRPaymentDetailId = detail.Id;
                        history.Amount = detail.TotalAmount;
                        history.IsPurchaseRequestAdvancePayment = true;
                        history.PaymentMode = "Advance";
                        history.PurchaseOrderId = purchaseOrder.PurchaseOrderId;

                        history.Createby = userid;
                        history.CreatedDate = currentDate;
                        history.Deleted = false;
                        history.IsActive = true;

                        context.IRPaymentDetailHistoryDB.Add(history);
                        context.Commit();
                    }

                }
            }


        }
    }
}