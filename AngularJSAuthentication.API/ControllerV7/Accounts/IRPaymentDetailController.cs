using AngularJSAuthentication.API.Helper;
using AngularJSAuthentication.Model;
using AngularJSAuthentication.Model.Account;
using AngularJSAuthentication.Model.PurchaseOrder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Transactions;
using System.Web.Http;

namespace AngularJSAuthentication.API.ControllerV7.Accounts
{
    [AllowAnonymous]
    [RoutePrefix("api/IRPaymentDetail")]
    public class IRPaymentDetailController : ApiController
    {

        [HttpGet]
        [Route("RollbackBySupplierId/{supplierId}")]
        public void RollbackBySupplierId(int supplierId)
        {
            if(supplierId > 0)
            {
                using(var context = new AuthContext())
                {
                    List<IRPaymentDetails> iRPaymentDetailsList = context.IRPaymentDetailsDB.Where(x => x.SupplierId == supplierId && x.IsActive == true && x.Deleted != true && x.IsIROutstandingPending != true).ToList();
                    if(iRPaymentDetailsList != null && iRPaymentDetailsList.Any())
                    {
                        foreach (var item in iRPaymentDetailsList)
                        {
                            RollbackSinglePayment(item.Id);
                        }
                    }
                }
            }
        }


        [HttpGet]
        [Route("RollbackByDetailId/{detailId}")]
        public void RollbackByDetailId(int detailId)
        {
            RollbackSinglePayment(detailId);
        }

        [HttpGet]
        [AllowAnonymous]
        [Route("UploadIRPaymentBySupplierId/{supplierid}")]
        public void UploadIRPaymentBySupplierId(int supplierid)
        {
            IRHelper helper = new IRHelper();
            helper.UploadIRPaymentBySupplierId(supplierid);
        }

        //1: Update IRPaymentDetail table TotalRemainingAmount value by getting from LedgerEntry, doing group by on IRPaymentDetailId 
        //2: Delete IRPaymentDetailHistory Except Advance payment history
        //3: Delete all LedgerEntries for given IRPaymentDetailId
        //4: Update IRMaster TotalRemainingAmount, PaymentStatus, IRStatus
        //5: Create Fresh entry in LedgerEntry table as AdvancePayment   
        private void RollbackSinglePayment(int detailId)
        {
            LadgerHelper ladgerHelper = new LadgerHelper();


            if (detailId > 0)
            {
                TransactionOptions option = new TransactionOptions();
                option.IsolationLevel = IsolationLevel.RepeatableRead;
                option.Timeout = TimeSpan.FromSeconds(90);
                using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, option))
               // using (var scope = new TransactionScope())
                {
                    using (var context = new AuthContext())
                    {
                        IRPaymentDetails detail = context.IRPaymentDetailsDB.First(x => x.Id == detailId && x.IsIROutstandingPending != true);
                        if (detail != null)
                        {
                            DeleteIRPaymentDetailHistoryExceptAdvance(detailId, context);

                            Ladger ledger = ladgerHelper.GetOrCreateLadgerTypeAndLadger("Supplier", detail.SupplierId, 0, context);
                            long? ladgerID = ledger.ID;
                            string query = @"select sum(debit) amount from LadgerEntries where LagerID = "
                                + ladgerID.ToString()
                                + " and IrPaymentDetailsId = " + detailId.ToString()
                                + @" and VouchersTypeID = 6 group by IrPaymentDetailsId";

                            detail.TotalReaminingAmount = context.Database.SqlQuery<double>(query).FirstOrDefault();
                            detail.TotalReaminingAmount = Math.Round(detail.TotalReaminingAmount.Value);
                            context.Commit();
                            UpdateIRMasterList(context, ladgerID.Value, detailId);
                            List<LadgerEntry> entryList = context.LadgerEntryDB.Where(x => x.IrPaymentDetailsId == detailId).ToList();
                            if (entryList != null && entryList.Count > 0)
                            {
                                context.LadgerEntryDB.RemoveRange(entryList);
                                IRHelper irHelper = new IRHelper();
                                irHelper.DebitLedgerEntrySupplieradvacepay(detail.SupplierId, context, detail.TotalReaminingAmount ?? 0, 0, detail.RefNo, detail.Remark, detail.Guid, detail.BankId, true, detail.Id, detail.PaymentDate);
                            }
                        }
                    }
                    scope.Complete();
                }

            }
        }

        private bool UpdateIRMasterList(AuthContext context, long supplierLedgerId, int iRPaymentDetailId)
        {
            string query = @"select ObjectId IRMasterId, Debit as Amount from LadgerEntries where LagerID = "
                                + supplierLedgerId.ToString()
                                + " and IrPaymentDetailsId = " + iRPaymentDetailId.ToString()
                                + @" and VouchersTypeID = 6";
            List<InnerIRPaymentDetail> IrMasterIdList = context.Database.SqlQuery<InnerIRPaymentDetail>(query).ToList();

            if(IrMasterIdList != null && IrMasterIdList.Count > 0)
            {
                foreach (InnerIRPaymentDetail item in IrMasterIdList)
                {
                    if(item.IRMasterId > 0)
                    {
                        IRMaster irMaster = context.IRMasterDB.FirstOrDefault(x => x.Id == item.IRMasterId);
                        irMaster.TotalAmountRemaining = irMaster.TotalAmountRemaining + item.Amount;
                        if (Math.Round(irMaster.TotalAmountRemaining) == Math.Round(irMaster.TotalAmount))
                        {
                            irMaster.PaymentStatus = "Unpaid";
                        }
                        else
                        {
                            irMaster.PaymentStatus = "partial paid";
                        }
                        irMaster.IRStatus = "Approved from Buyer side";

                        context.Commit();
                    }
                    else
                    {
                        //case of unsattled payment
                    }
                    
                }
                
            }

            return true;
        }

        private void DeleteIRPaymentDetailHistoryExceptAdvance(int detailId, AuthContext contex)
        {
            List<IRPaymentDetailHistory> iRPaymentDetailHistoryList = contex.IRPaymentDetailHistoryDB.Where(x => x.IRPaymentDetailId == detailId && x.PaymentMode != "Advance").ToList(); 
            if(iRPaymentDetailHistoryList != null && iRPaymentDetailHistoryList.Any())
            {
                contex.IRPaymentDetailHistoryDB.RemoveRange(iRPaymentDetailHistoryList);
                contex.Commit();
            }
        }


        class InnerIRPaymentDetail
        {
            public long IRMasterId { get; set; }
            public double Amount { get; set; }
        }


    }
}
