using AngularJSAuthentication.API.Controllers.PurchaseOrder;
using AngularJSAuthentication.API.ControllerV7.PurchaseRequestPayments;
using AngularJSAuthentication.DataContracts.Transaction.PurchaseRequest;
using AngularJSAuthentication.Model;
using AngularJSAuthentication.Model.Account;
using AngularJSAuthentication.Model.PurchaseOrder;
using AngularJSAuthentication.Model.PurchaseRequestPayments;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Transactions;
using System.Web;
using System.Data;

namespace AngularJSAuthentication.API.Helper
{
    public class PurchaseRequestSettlementHelper
    {

        public void SettleAmount(AuthContext context, IRMaster irMaster, double amount, int userid, DateTime currentTime, Guid guid, bool isCallFromBillToBillSettlement, double debitnoteamount = 0)
        {

            amount = irMaster.TotalAmountRemaining < amount ? irMaster.TotalAmountRemaining : amount;
            double TotalRemainingTDSAmounttemp = 0;


            List<SqlParameter> paramList = new List<SqlParameter>();
            List<PRPaymentDc> settlePaymentList = null;
            string supplierIdString = string.Join(",", irMaster.supplierId.ToString());
            //double tdsper = 0;
            //var suppliertds = context.Suppliers.Where(x => x.SupplierId == irMaster.supplierId && x.Active == true).FirstOrDefault();
            //if (suppliertds != null)
            //{
            //    if (!string.IsNullOrEmpty(suppliertds.Pancard) && !string.IsNullOrEmpty(suppliertds.TINNo))
            //    {
            //        tdsper = 0.1;
            //    }
            //    else
            //    {
            //        tdsper = 5;
            //    }
            //}

            /*
             * When we call this from bill to bill payment then Advance outstanding may be taken from different
             * PR those are closed.
             * BY calling GetAdvanceOutstanding we will get list of all outstanding PR 
             * those are closed.
             */
            if (isCallFromBillToBillSettlement)
            {
                paramList.Add(new SqlParameter("@SupplierIdList", supplierIdString));
                paramList.Add(new SqlParameter("@IsGetSummary", false));
                settlePaymentList = context.Database.SqlQuery<PRPaymentDc>("GetAdvanceOutstanding @SupplierIdList, @IsGetSummary", paramList.ToArray()).ToList();
            }
            /*
             * When we call from IR auto Settlement when IR Approved by buyer 
             * then Advance outstanding must be taken from same PR/PO only.
             */
            else
            {
                string query = @"	SELECT POM.PurchaseOrderId, 
				       POM.SupplierId,
				       PRPT.Id as PRPaymentTransferId,
				       PRPT.TransferredAmount - ISNULL(PRPT.OutAmount, 0) - ISNULL(PRPT.SettledAmount, 0) AS Total,
				       PRPT.SourcePurchaseRequestPaymentId,
                       PRPT.IsTDSDeducted
			    FROM PurchaseOrderMasters POM
			    INNER JOIN PRPaymentTransfers PRPT  ON POM.PurchaseOrderId = PRPT.ToPurchaseOrderId
			    INNER JOIN PurchaseRequestPayments PRP ON PRPT.SourcePurchaseRequestPaymentId = PRP.Id AND PRP.PrPaymentStatus = 'Approved'
			    WHERE (PRPT.TransferredAmount - ISNULL(PRPT.OutAmount, 0) - ISNULL(PRPT.SettledAmount, 0) ) >0
			    AND POM.PurchaseOrderId =  " + irMaster.PurchaseOrderId.ToString();
                settlePaymentList = context.Database.SqlQuery<PRPaymentDc>(query, paramList.ToArray()).ToList();

            }
            if (settlePaymentList != null && settlePaymentList.Any())
            {
                settlePaymentList = settlePaymentList.OrderBy(x => x.Total).ToList();
                foreach (var payment in settlePaymentList)
                {

                    if (payment.IsTDSDeducted == false)
                    {
                        TotalRemainingTDSAmounttemp = 0;
                    }
                    else
                    {
                        double tdsam = context.IRMasterDB.Where(x => x.Id == irMaster.Id).Select(y => y.TotalRemainingTDSAmount).FirstOrDefault();

                        //TotalRemainingTDSAmounttemp = (irMaster.TotalAmountRemaining < amount ?
                        //                              irMaster.TotalRemainingTDSAmount : amount) * tdsper / 100;
                        TotalRemainingTDSAmounttemp = Math.Round(tdsam);
                        amount = amount - TotalRemainingTDSAmounttemp;
                    }

                    if (amount == 0)
                    {
                        break;
                    }
                    List<LadgerEntry> prLedgerEntryList = context
                            .LadgerEntryDB.Where(x => x.PRPaymentId == payment.SourcePurchaseRequestPaymentId && x.ObjectType == "PR" && x.VouchersTypeID == 6)
                            .ToList();
                    LadgerEntry debitPRLedgerEntry = prLedgerEntryList.First(x => x.Debit > 0);
                    LadgerEntry creditPRLedgerEntry = prLedgerEntryList.First(x => x.Credit > 0);


                    IRPaymentDetails detail = new IRPaymentDetails()
                    {
                        BankId = (int)creditPRLedgerEntry.LagerID.Value,
                        BankName = "",
                        Createby = userid,
                        CreatedDate = currentTime,
                        Deleted = false,
                        Guid = guid.ToString(),
                        IRList = "", //JsonConvert.SerializeObject(irMaster),
                        IRPaymentSummaryId = 0,
                        IsActive = true,
                        PaymentDate = debitPRLedgerEntry.Date,
                        RefNo = debitPRLedgerEntry.RefNo,
                        Remark = debitPRLedgerEntry.Remark,
                        SupplierId = irMaster.supplierId,
                        TotalAmount = (int)((amount >= payment.Total && (!isCallFromBillToBillSettlement || amount >= debitPRLedgerEntry.Debit)) ? (payment.Total) : amount),
                        TotalReaminingAmount = 0,
                        WarehouseId = irMaster.WarehouseId,
                        IsIROutstandingPending = false,
                        PaymentStatus = "Approved",
                        IRMasterId = irMaster.Id,
                        TDSAmount = TotalRemainingTDSAmounttemp
                    };
                    context.IRPaymentDetailsDB.Add(detail);
                    context.Commit();


                    if (amount >= payment.Total && (!isCallFromBillToBillSettlement || amount >= debitPRLedgerEntry.Debit) && payment.Total == debitPRLedgerEntry.Debit)
                    {
                        foreach (var item in prLedgerEntryList)
                        {
                            item.ObjectID = irMaster.Id;
                            item.ObjectType = "IR";
                            item.UpdatedBy = userid;
                            item.UpdatedDate = currentTime;
                            item.IrPaymentDetailsId = detail.Id;
                        }
                    }
                    else if (amount >= payment.Total && payment.Total < debitPRLedgerEntry.Debit)
                    {
                        LadgerEntry debitEntry = new LadgerEntry
                        {
                            Active = true,
                            AffectedLadgerID = debitPRLedgerEntry.AffectedLadgerID,
                            CreatedBy = userid,
                            CreatedDate = DateTime.Now,
                            Date = debitPRLedgerEntry.Date,
                            UpdatedDate = DateTime.Now,
                            Credit = null,
                            Debit = payment.Total,
                            ObjectID = irMaster.Id,
                            ObjectType = "IR",
                            IsSupplierAdvancepay = false,
                            Particulars = irMaster.InvoiceNumber,
                            RefNo = debitPRLedgerEntry.RefNo,
                            Remark = debitPRLedgerEntry.Remark,
                            VouchersTypeID = context.VoucherTypeDB.Where(x => x.Name.ToLower() == "payment").First().ID,
                            VouchersNo = null,
                            LagerID = debitPRLedgerEntry.LagerID,
                            UploadGUID = debitPRLedgerEntry.UploadGUID,
                            UpdatedBy = userid,
                            PRPaymentId = debitPRLedgerEntry.PRPaymentId,
                            IrPaymentDetailsId = detail.Id
                        };
                        context.LadgerEntryDB.Add(debitEntry);

                        LadgerEntry creditEntry = new LadgerEntry
                        {
                            Active = true,
                            AffectedLadgerID = creditPRLedgerEntry.AffectedLadgerID,
                            CreatedBy = userid,
                            CreatedDate = DateTime.Now,
                            Date = creditPRLedgerEntry.Date,
                            UpdatedDate = DateTime.Now,
                            Credit = payment.Total,
                            Debit = null,
                            ObjectID = irMaster.Id,
                            ObjectType = "IR",
                            IsSupplierAdvancepay = false,
                            Particulars = irMaster.InvoiceNumber,
                            RefNo = creditPRLedgerEntry.RefNo,
                            Remark = creditPRLedgerEntry.Remark,
                            VouchersTypeID = debitEntry.VouchersTypeID,
                            VouchersNo = null,
                            LagerID = creditPRLedgerEntry.LagerID,
                            UploadGUID = creditPRLedgerEntry.UploadGUID,
                            UpdatedBy = userid,
                            PRPaymentId = creditPRLedgerEntry.PRPaymentId,
                            IrPaymentDetailsId = detail.Id
                        };
                        context.LadgerEntryDB.Add(creditEntry);
                        foreach (var item in prLedgerEntryList)
                        {
                            item.Debit = item.Debit.HasValue && item.Debit > 0 ? item.Debit - payment.Total : null;
                            item.Credit = item.Credit.HasValue && item.Credit > 0 ? item.Credit - payment.Total : null;
                            item.UpdatedBy = userid;
                            item.UpdatedDate = currentTime;
                        }
                    }
                    else if (amount <= payment.Total && amount < debitPRLedgerEntry.Debit)
                    {
                        LadgerEntry debitEntry = new LadgerEntry
                        {
                            Active = true,
                            AffectedLadgerID = debitPRLedgerEntry.AffectedLadgerID,
                            CreatedBy = userid,
                            CreatedDate = DateTime.Now,
                            Date = debitPRLedgerEntry.Date,
                            UpdatedDate = DateTime.Now,
                            Credit = null,
                            Debit = amount,
                            ObjectID = irMaster.Id,
                            ObjectType = "IR",
                            IsSupplierAdvancepay = false,
                            Particulars = irMaster.InvoiceNumber,
                            RefNo = debitPRLedgerEntry.RefNo,
                            Remark = debitPRLedgerEntry.Remark,
                            VouchersTypeID = context.VoucherTypeDB.Where(x => x.Name.ToLower() == "payment").First().ID,
                            VouchersNo = null,
                            LagerID = debitPRLedgerEntry.LagerID,
                            UploadGUID = debitPRLedgerEntry.UploadGUID,
                            UpdatedBy = userid,
                            PRPaymentId = debitPRLedgerEntry.PRPaymentId,
                            IrPaymentDetailsId = detail.Id
                        };
                        context.LadgerEntryDB.Add(debitEntry);

                        LadgerEntry creditEntry = new LadgerEntry
                        {
                            Active = true,
                            AffectedLadgerID = creditPRLedgerEntry.AffectedLadgerID,
                            CreatedBy = userid,
                            CreatedDate = DateTime.Now,
                            Date = creditPRLedgerEntry.Date,
                            UpdatedDate = DateTime.Now,
                            Credit = amount,
                            Debit = null,
                            ObjectID = irMaster.Id,
                            ObjectType = "IR",
                            IsSupplierAdvancepay = false,
                            Particulars = irMaster.InvoiceNumber,
                            RefNo = creditPRLedgerEntry.RefNo,
                            Remark = creditPRLedgerEntry.Remark,
                            VouchersTypeID = debitEntry.VouchersTypeID,
                            VouchersNo = null,
                            LagerID = creditPRLedgerEntry.LagerID,
                            UploadGUID = creditPRLedgerEntry.UploadGUID,
                            UpdatedBy = userid,
                            PRPaymentId = creditPRLedgerEntry.PRPaymentId,
                            IrPaymentDetailsId = detail.Id
                        };
                        context.LadgerEntryDB.Add(creditEntry);
                        foreach (var item in prLedgerEntryList)
                        {
                            item.Debit = item.Debit.HasValue && item.Debit > 0 ? item.Debit - amount : null;
                            item.Credit = item.Credit.HasValue && item.Credit > 0 ? item.Credit - amount : null;
                            item.UpdatedBy = userid;
                            item.UpdatedDate = currentTime;
                        }
                    }

                    PrPaymentTransfer prPaymentTransfer = context.PrPaymentTransferDB.First(x => x.Id == payment.PRPaymentTransferId);
                    prPaymentTransfer.SettledAmount += (amount >= payment.Total && (!isCallFromBillToBillSettlement || amount >= debitPRLedgerEntry.Debit)) ? (payment.Total) : amount;
                    prPaymentTransfer.ModifiedBy = userid;
                    prPaymentTransfer.ModifiedDate = currentTime;


                    irMaster.TotalAmountRemaining -= (amount >= payment.Total && (!isCallFromBillToBillSettlement || amount >= debitPRLedgerEntry.Debit)) ? (payment.Total) : amount;
                    amount -= (amount >= payment.Total && (!isCallFromBillToBillSettlement || amount >= debitPRLedgerEntry.Debit)) ? (payment.Total) : amount;




                    irMaster.UpdatedDate = currentTime;
                    if (irMaster.TotalAmountRemaining - debitnoteamount < 2)
                    {
                        irMaster.IRStatus = "Paid";
                        irMaster.PaymentStatus = "paid";
                    }
                    else
                    {
                        irMaster.PaymentStatus = "partial paid";
                    }
                    //--new
                    double total = context.IRMasterDB.Where(x => x.PurchaseOrderId == irMaster.PurchaseOrderId && x.Deleted == false).Sum(y => y.TotalAmount);
                    AdvanceAmount advanceamt = new AdvanceAmount();
                    var query = "GetPOOutstanding @PurchaseOrderId";
                    List<SqlParameter> paramListt = new List<SqlParameter>();
                    paramListt.Add(new SqlParameter("@PurchaseOrderId", SqlDbType.BigInt) { Value = Convert.ToDouble(irMaster.PurchaseOrderId) });
                    advanceamt = context.Database.SqlQuery<AdvanceAmount>(query, paramListt.ToArray()).FirstOrDefault();
                    double advanceamount = Convert.ToDouble(advanceamt.TotalPayment);
                    double tdsamt = Convert.ToDouble(advanceamt.TDSAmount);
                    double totall = advanceamount + tdsamt;
                    if (total <= totall)
                    {
                        irMaster.IRStatus = "Paid";
                        irMaster.PaymentStatus = "paid";
                    }
                    //
                    //if (Math.Round(irMaster.TotalAmountRemaining) - TotalRemainingTDSAmounttemp < 2)
                    //{
                    //    irMaster.IRStatus = "Paid";
                    //    irMaster.PaymentStatus = "paid";
                    //}
                    //irMaster.TotalRemainingTDSAmount = Math.Round(irMaster.TotalAmountRemaining * tdsper / 100);

                    context.Commit();
                }
            }
        }

        public PurchaseRequestSettlementContainerDc GetPageList(PurchaseRequestSettlementFilterDc filter)
        {
            PurchaseRequestSettlementContainerDc container = new PurchaseRequestSettlementContainerDc();
            List<SqlParameter> paramList = null;
            using (var context = new AuthContext())
            {
                string spNameForList;
                paramList = GetParamList(filter, false, out spNameForList);
                container.PageList = context.Database.SqlQuery<PurchaseRequestSettlementPageDc>(spNameForList, paramList.ToArray()).ToList();

                string spNameForTotalRecords;
                paramList = GetParamList(filter, true, out spNameForTotalRecords);
                PurchaseRequestSettlementContainerDc tempContainer = context.Database.SqlQuery<PurchaseRequestSettlementContainerDc>(spNameForList, paramList.ToArray()).FirstOrDefault();
                container.TotalRecords = tempContainer.TotalRecords;
                return container;
            }
        }

        public List<RemainingIRAmountDc> GetIRList(int supplierId)
        {
            using (var context = new AuthContext())
            {
                var query = from irm in context.IRMasterDB
                            where irm.supplierId == supplierId && irm.TotalAmountRemaining >= 1
                            && (irm.IRStatus == "Approved from Buyer side")
                            select new RemainingIRAmountDc
                            {
                                TotalAmountRemaining = irm.TotalAmountRemaining,
                                SettleAmount = (int)Math.Ceiling(irm.TotalAmountRemaining),
                                InvoiceNumber = irm.InvoiceNumber,
                                InvoiceDate = irm.InvoiceDate,
                                PurchaseOrderId = irm.PurchaseOrderId,
                                IRMasterId = irm.Id
                            };

                List<RemainingIRAmountDc> remainingAmountList = query.ToList();
                return remainingAmountList;
            }
        }

        public void SettlePayment(PurchaseRequestPaymentSettlementDc settlement, int userId)
        {
            if (settlement != null && settlement.PaymentList.Any())
            {
                TransactionOptions option = new TransactionOptions();
                option.IsolationLevel = System.Transactions.IsolationLevel.RepeatableRead;
                option.Timeout = TimeSpan.FromSeconds(90);
                using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, option))
                {
                    using (var context = new AuthContext())
                    {
                        var query = from le in context.LadgerEntryDB
                                    where le.PRPaymentId == settlement.PurchaseRequestPaymentId
                                    && le.ObjectType == "PR"
                                    && le.VouchersTypeID == 6
                                    select le;
                        List<LadgerEntry> list = query.ToList();
                        long supplierLedgerId = list.First(x => x.Debit > 0).LagerID.Value;
                        long bankLedgerId = list.First(x => x.Debit > 0).AffectedLadgerID.Value;
                        string refNo = list.First(x => x.Debit > 0).RefNo;
                        DateTime paymentDate = list.First(x => x.Debit > 0).Date.Value;
                        string guid = list.First(x => x.Debit > 0).UploadGUID;
                        foreach (var childMaster in settlement.PaymentList)
                        {
                            IRMaster irMaster = context.IRMasterDB.FirstOrDefault(x => x.Id == childMaster.IRMasterId);
                            irMaster.TotalAmountRemaining -= childMaster.PayingAmount;
                            if (Math.Floor(irMaster.TotalAmountRemaining) == 0)
                            {
                                irMaster.IRStatus = "Paid";
                                irMaster.PaymentStatus = "paid";
                            }
                            else
                            {
                                irMaster.PaymentStatus = "partial paid";
                            }

                            IRPaymentDetails irPaymentDetail = MakeIRPaymentDetail(context, settlement, childMaster, (int)bankLedgerId, userId, refNo, paymentDate, irMaster.WarehouseId);
                            MakeLedgerEntry(irPaymentDetail, supplierLedgerId, bankLedgerId, settlement.PurchaseRequestPaymentId, guid, refNo, paymentDate, context, userId);
                        }

                        if (settlement.AfterSettleRemainingAmount > 0)
                        {
                            foreach (LadgerEntry entry in list)
                            {
                                entry.Debit = entry.Debit > 0 ? settlement.AfterSettleRemainingAmount : entry.Debit;
                                entry.Credit = entry.Credit > 0 ? settlement.AfterSettleRemainingAmount : entry.Credit;
                            }
                        }
                        else
                        {
                            foreach (LadgerEntry entry in list)
                            {
                                context.LadgerEntryDB.Remove(entry);
                            }
                        }

                        context.Commit();
                    }

                    scope.Complete();
                }
            }
        }

        public IRPaymentDetails MakeIRPaymentDetail(AuthContext context, PurchaseRequestPaymentSettlementDc settlement, ChildIrMasterDc childIrMasterDc, int bankLedgrId, int userId, string refNo, DateTime paymentDate, int warehouseId)
        {
            IRPaymentDetails irPaymentDetails = new IRPaymentDetails
            {
                BankId = bankLedgrId,
                Createby = userId,
                CreatedDate = DateTime.Now,
                Deleted = false,
                IRMasterId = childIrMasterDc.IRMasterId,
                IsActive = true,
                PaymentStatus = "Approved",
                RefNo = refNo,
                PaymentDate = paymentDate,
                TotalAmount = childIrMasterDc.PayingAmount,
                TotalReaminingAmount = 0,
                IRPaymentSummaryId = 0,
                IsIROutstandingPending = null,
                Guid = Guid.NewGuid().ToString(),
                WarehouseId = warehouseId,
                SupplierId = settlement.SupplierId
            };
            context.IRPaymentDetailsDB.Add(irPaymentDetails);
            context.Commit();
            return irPaymentDetails;
        }

        public void MakeLedgerEntry(IRPaymentDetails irPaymentDetail, long supplierLedgerId, long bankLedgerId, long purchaseRequestPaymentId, string guid, string refNo, DateTime ledgerDate, AuthContext context, int userId)
        {

            LadgerEntry ladgerEntry = new LadgerEntry
            {
                IrPaymentDetailsId = irPaymentDetail.Id,
                IsSupplierAdvancepay = false,
                Debit = irPaymentDetail.TotalAmount,
                Active = true,
                AffectedLadgerID = bankLedgerId,
                CreatedDate = DateTime.Now,
                CreatedBy = userId,
                Credit = null,
                Date = ledgerDate,
                LagerID = supplierLedgerId,
                ObjectID = irPaymentDetail.IRMasterId,
                ObjectType = "IR",
                Particulars = "",
                PRPaymentId = purchaseRequestPaymentId,
                RefNo = refNo,
                Remark = "Manual Settle by Advance Payment Settlement",
                VouchersTypeID = 6,
                UploadGUID = guid,
                UpdatedBy = userId,
                UpdatedDate = DateTime.Now
            };

            context.LadgerEntryDB.Add(ladgerEntry);

            ladgerEntry = new LadgerEntry
            {
                IrPaymentDetailsId = irPaymentDetail.Id,
                IsSupplierAdvancepay = false,
                Debit = null,
                Active = true,
                AffectedLadgerID = supplierLedgerId,
                CreatedDate = DateTime.Now,
                CreatedBy = userId,
                Credit = irPaymentDetail.TotalAmount,
                Date = ledgerDate,
                LagerID = bankLedgerId,
                ObjectID = irPaymentDetail.IRMasterId,
                ObjectType = "IR",
                Particulars = "",
                PRPaymentId = purchaseRequestPaymentId,
                RefNo = refNo,
                Remark = "Manual Settle by Advance Payment Settlement",
                VouchersTypeID = 6,
                UploadGUID = guid,
                UpdatedBy = userId,
                UpdatedDate = DateTime.Now
            };

            context.LadgerEntryDB.Add(ladgerEntry);
            context.Commit();
        }

        private List<SqlParameter> GetParamList(PurchaseRequestSettlementFilterDc filter, bool isGetCount, out string spWithParam)
        {
            spWithParam = "EXEC GetClosedPRList @SupplierId, @WarehouseId, @Skip, @Take, @IsGetCount";
            List<SqlParameter> paramList = new List<SqlParameter>();
            paramList.Add(new SqlParameter("@SupplierId", filter.SupplierId));
            paramList.Add(new SqlParameter("@WarehouseId", filter.WarehouseId));
            paramList.Add(new SqlParameter("@Skip", filter.Skip));
            paramList.Add(new SqlParameter("@Take", filter.Take));
            paramList.Add(new SqlParameter("@IsGetCount", isGetCount));
            return paramList;
        }

        public void SettleAmounts(AuthContext context, IRMaster irMaster, double amount, int userid, DateTime currentTime, Guid guid, bool isCallFromBillToBillSettlement, double tds, int? pid, double debitnoteamount = 0)
        {

            amount = irMaster.TotalAmountRemaining < amount ? irMaster.TotalAmountRemaining : amount;
            double TotalRemainingTDSAmounttemp = 0;


            List<SqlParameter> paramList = new List<SqlParameter>();
            List<PRPaymentDc> settlePaymentList = null;
            string supplierIdString = string.Join(",", irMaster.supplierId.ToString());
            //double tdsper = 0;
            //var suppliertds = context.Suppliers.Where(x => x.SupplierId == irMaster.supplierId && x.Active == true).FirstOrDefault();
            //if (suppliertds != null)
            //{
            //    if (!string.IsNullOrEmpty(suppliertds.Pancard) && !string.IsNullOrEmpty(suppliertds.TINNo))
            //    {
            //        tdsper = 0.1;
            //    }
            //    else
            //    {
            //        tdsper = 5;
            //    }
            //}

            /*
             * When we call this from bill to bill payment then Advance outstanding may be taken from different
             * PR those are closed.
             * BY calling GetAdvanceOutstanding we will get list of all outstanding PR 
             * those are closed.
             */
            if (isCallFromBillToBillSettlement)
            {
                paramList.Add(new SqlParameter("@SupplierIdList", supplierIdString));
                paramList.Add(new SqlParameter("@IsGetSummary", false));
                settlePaymentList = context.Database.SqlQuery<PRPaymentDc>("GetAdvanceOutstanding @SupplierIdList, @IsGetSummary", paramList.ToArray()).ToList();
            }
            /*
             * When we call from IR auto Settlement when IR Approved by buyer 
             * then Advance outstanding must be taken from same PR/PO only.
             */
            else
            {
                string query = @"	SELECT POM.PurchaseOrderId, 
				       POM.SupplierId,
				       PRPT.Id as PRPaymentTransferId,
				       PRPT.TransferredAmount - ISNULL(PRPT.OutAmount, 0) - ISNULL(PRPT.SettledAmount, 0) AS Total,
				       PRPT.SourcePurchaseRequestPaymentId,
                       PRPT.IsTDSDeducted
			    FROM PurchaseOrderMasters POM
			    INNER JOIN PRPaymentTransfers PRPT  ON POM.PurchaseOrderId = PRPT.ToPurchaseOrderId
			    INNER JOIN PurchaseRequestPayments PRP ON PRPT.SourcePurchaseRequestPaymentId = PRP.Id AND PRP.PrPaymentStatus = 'Approved'
			    WHERE (PRPT.TransferredAmount - ISNULL(PRPT.OutAmount, 0) - ISNULL(PRPT.SettledAmount, 0) ) >0
			    AND POM.PurchaseOrderId =  " + irMaster.PurchaseOrderId.ToString();
                settlePaymentList = context.Database.SqlQuery<PRPaymentDc>(query, paramList.ToArray()).ToList();

            }
            if (settlePaymentList != null && settlePaymentList.Any())
            {
                settlePaymentList = settlePaymentList.OrderBy(x => x.Total).ToList();
                foreach (var payment in settlePaymentList)
                {
                    if (amount == 0)
                    {
                        break;
                    }
                    if (payment.IsTDSDeducted == false)
                    {
                        TotalRemainingTDSAmounttemp = 0;
                    }
                    else
                    {
                        double tdsam = context.IRMasterDB.Where(x => x.Id == irMaster.Id).Select(y => y.TotalRemainingTDSAmount).FirstOrDefault();

                        //TotalRemainingTDSAmounttemp = (irMaster.TotalAmountRemaining < amount ?
                        //                              irMaster.TotalRemainingTDSAmount : amount) * tdsper / 100;
                        if (pid > 0)
                        {
                            var podata = context.DPurchaseOrderMaster.FirstOrDefault(x => x.PurchaseOrderId == pid);
                            if (podata.PRPaymentType == "AdvancePR")
                            {
                                TotalRemainingTDSAmounttemp = Math.Round(tds);
                            }
                            else
                            {
                                TotalRemainingTDSAmounttemp = Math.Round(tdsam);
                            }
                            amount = amount;//- TotalRemainingTDSAmounttemp;
                        }
                        else
                        {
                            TotalRemainingTDSAmounttemp = Math.Round(tds);
                            amount = amount - TotalRemainingTDSAmounttemp;
                        }



                    }

                    
                    List<LadgerEntry> prLedgerEntryList = context
                            .LadgerEntryDB.Where(x => x.PRPaymentId == payment.SourcePurchaseRequestPaymentId && x.ObjectType == "PR" && x.VouchersTypeID == 6)
                            .ToList();
                    LadgerEntry debitPRLedgerEntry = prLedgerEntryList.First(x => x.Debit > 0);
                    LadgerEntry creditPRLedgerEntry = prLedgerEntryList.First(x => x.Credit > 0);


                    IRPaymentDetails detail = new IRPaymentDetails()
                    {
                        BankId = (int)creditPRLedgerEntry.LagerID.Value,
                        BankName = "",
                        Createby = userid,
                        CreatedDate = currentTime,
                        Deleted = false,
                        Guid = guid.ToString(),
                        IRList = "", //JsonConvert.SerializeObject(irMaster),
                        IRPaymentSummaryId = 0,
                        IsActive = true,
                        PaymentDate = debitPRLedgerEntry.Date,
                        RefNo = debitPRLedgerEntry.RefNo,
                        Remark = debitPRLedgerEntry.Remark,
                        SupplierId = irMaster.supplierId,
                        TotalAmount = (int)((amount >= payment.Total && (!isCallFromBillToBillSettlement || amount >= debitPRLedgerEntry.Debit)) ? (payment.Total) : amount),
                        TotalReaminingAmount = 0,
                        WarehouseId = irMaster.WarehouseId,
                        IsIROutstandingPending = false,
                        PaymentStatus = "Approved",
                        IRMasterId = irMaster.Id,
                        TDSAmount = TotalRemainingTDSAmounttemp
                    };
                    context.IRPaymentDetailsDB.Add(detail);
                    //context.Commit();


                    if (amount >= payment.Total && (!isCallFromBillToBillSettlement || amount >= debitPRLedgerEntry.Debit) && payment.Total == debitPRLedgerEntry.Debit)
                    {
                        foreach (var item in prLedgerEntryList)
                        {
                            item.ObjectID = irMaster.Id;
                            item.ObjectType = "IR";
                            item.UpdatedBy = userid;
                            item.UpdatedDate = currentTime;
                            item.IrPaymentDetailsId = detail.Id;
                        }
                    }
                    else if (amount >= payment.Total && payment.Total < debitPRLedgerEntry.Debit)
                    {
                        LadgerEntry debitEntry = new LadgerEntry
                        {
                            Active = true,
                            AffectedLadgerID = debitPRLedgerEntry.AffectedLadgerID,
                            CreatedBy = userid,
                            CreatedDate = DateTime.Now,
                            Date = debitPRLedgerEntry.Date,
                            UpdatedDate = DateTime.Now,
                            Credit = null,
                            Debit = payment.Total,
                            ObjectID = irMaster.Id,
                            ObjectType = "IR",
                            IsSupplierAdvancepay = false,
                            Particulars = irMaster.InvoiceNumber,
                            RefNo = debitPRLedgerEntry.RefNo,
                            Remark = debitPRLedgerEntry.Remark,
                            VouchersTypeID = context.VoucherTypeDB.Where(x => x.Name.ToLower() == "payment").First().ID,
                            VouchersNo = null,
                            LagerID = debitPRLedgerEntry.LagerID,
                            UploadGUID = debitPRLedgerEntry.UploadGUID,
                            UpdatedBy = userid,
                            PRPaymentId = debitPRLedgerEntry.PRPaymentId,
                            IrPaymentDetailsId = detail.Id
                        };
                        context.LadgerEntryDB.Add(debitEntry);

                        LadgerEntry creditEntry = new LadgerEntry
                        {
                            Active = true,
                            AffectedLadgerID = creditPRLedgerEntry.AffectedLadgerID,
                            CreatedBy = userid,
                            CreatedDate = DateTime.Now,
                            Date = creditPRLedgerEntry.Date,
                            UpdatedDate = DateTime.Now,
                            Credit = payment.Total,
                            Debit = null,
                            ObjectID = irMaster.Id,
                            ObjectType = "IR",
                            IsSupplierAdvancepay = false,
                            Particulars = irMaster.InvoiceNumber,
                            RefNo = creditPRLedgerEntry.RefNo,
                            Remark = creditPRLedgerEntry.Remark,
                            VouchersTypeID = debitEntry.VouchersTypeID,
                            VouchersNo = null,
                            LagerID = creditPRLedgerEntry.LagerID,
                            UploadGUID = creditPRLedgerEntry.UploadGUID,
                            UpdatedBy = userid,
                            PRPaymentId = creditPRLedgerEntry.PRPaymentId,
                            IrPaymentDetailsId = detail.Id
                        };
                        context.LadgerEntryDB.Add(creditEntry);
                        foreach (var item in prLedgerEntryList)
                        {
                            item.Debit = item.Debit.HasValue && item.Debit > 0 ? item.Debit - payment.Total : null;
                            item.Credit = item.Credit.HasValue && item.Credit > 0 ? item.Credit - payment.Total : null;
                            item.UpdatedBy = userid;
                            item.UpdatedDate = currentTime;
                        }
                    }
                    else if (amount <= payment.Total && amount < debitPRLedgerEntry.Debit)
                    {
                        LadgerEntry debitEntry = new LadgerEntry
                        {
                            Active = true,
                            AffectedLadgerID = debitPRLedgerEntry.AffectedLadgerID,
                            CreatedBy = userid,
                            CreatedDate = DateTime.Now,
                            Date = debitPRLedgerEntry.Date,
                            UpdatedDate = DateTime.Now,
                            Credit = null,
                            Debit = amount,
                            ObjectID = irMaster.Id,
                            ObjectType = "IR",
                            IsSupplierAdvancepay = false,
                            Particulars = irMaster.InvoiceNumber,
                            RefNo = debitPRLedgerEntry.RefNo,
                            Remark = debitPRLedgerEntry.Remark,
                            VouchersTypeID = context.VoucherTypeDB.Where(x => x.Name.ToLower() == "payment").First().ID,
                            VouchersNo = null,
                            LagerID = debitPRLedgerEntry.LagerID,
                            UploadGUID = debitPRLedgerEntry.UploadGUID,
                            UpdatedBy = userid,
                            PRPaymentId = debitPRLedgerEntry.PRPaymentId,
                            IrPaymentDetailsId = detail.Id
                        };
                        context.LadgerEntryDB.Add(debitEntry);

                        LadgerEntry creditEntry = new LadgerEntry
                        {
                            Active = true,
                            AffectedLadgerID = creditPRLedgerEntry.AffectedLadgerID,
                            CreatedBy = userid,
                            CreatedDate = DateTime.Now,
                            Date = creditPRLedgerEntry.Date,
                            UpdatedDate = DateTime.Now,
                            Credit = amount,
                            Debit = null,
                            ObjectID = irMaster.Id,
                            ObjectType = "IR",
                            IsSupplierAdvancepay = false,
                            Particulars = irMaster.InvoiceNumber,
                            RefNo = creditPRLedgerEntry.RefNo,
                            Remark = creditPRLedgerEntry.Remark,
                            VouchersTypeID = debitEntry.VouchersTypeID,
                            VouchersNo = null,
                            LagerID = creditPRLedgerEntry.LagerID,
                            UploadGUID = creditPRLedgerEntry.UploadGUID,
                            UpdatedBy = userid,
                            PRPaymentId = creditPRLedgerEntry.PRPaymentId,
                            IrPaymentDetailsId = detail.Id
                        };
                        context.LadgerEntryDB.Add(creditEntry);
                        foreach (var item in prLedgerEntryList)
                        {
                            item.Debit = item.Debit.HasValue && item.Debit > 0 ? item.Debit - amount : null;
                            item.Credit = item.Credit.HasValue && item.Credit > 0 ? item.Credit - amount : null;
                            item.UpdatedBy = userid;
                            item.UpdatedDate = currentTime;
                        }
                    }

                    PrPaymentTransfer prPaymentTransfer = context.PrPaymentTransferDB.First(x => x.Id == payment.PRPaymentTransferId);
                    prPaymentTransfer.SettledAmount += (amount >= payment.Total && (!isCallFromBillToBillSettlement || amount >= debitPRLedgerEntry.Debit)) ? (payment.Total) : amount;
                    prPaymentTransfer.ModifiedBy = userid;
                    prPaymentTransfer.ModifiedDate = currentTime;


                    irMaster.TotalAmountRemaining -= (amount >= payment.Total && (!isCallFromBillToBillSettlement || amount >= debitPRLedgerEntry.Debit)) ? (payment.Total) : amount;
                    amount -= (amount >= payment.Total && (!isCallFromBillToBillSettlement || amount >= debitPRLedgerEntry.Debit)) ? (payment.Total) : amount;




                    irMaster.UpdatedDate = currentTime;
                    if (irMaster.TotalAmountRemaining - debitnoteamount < 2)
                    {
                        irMaster.IRStatus = "Paid";
                        irMaster.PaymentStatus = "paid";
                    }
                    else
                    {
                        irMaster.PaymentStatus = "partial paid";
                    }
                    //--new
                    double total = context.IRMasterDB.Where(x => x.PurchaseOrderId == irMaster.PurchaseOrderId && x.Deleted == false).Sum(y => y.TotalAmount);
                    AdvanceAmount advanceamt = new AdvanceAmount();
                    var query = "GetPOOutstanding @PurchaseOrderId";
                    List<SqlParameter> paramListt = new List<SqlParameter>();
                    paramListt.Add(new SqlParameter("@PurchaseOrderId", SqlDbType.BigInt) { Value = Convert.ToDouble(irMaster.PurchaseOrderId) });
                    advanceamt = context.Database.SqlQuery<AdvanceAmount>(query, paramListt.ToArray()).FirstOrDefault();
                    double advanceamount = Convert.ToDouble(advanceamt.TotalPayment);
                    double tdsamt = Convert.ToDouble(advanceamt.TDSAmount);
                    double totall = advanceamount + tdsamt;
                    if (total <= totall)
                    {
                        irMaster.IRStatus = "Paid";
                        irMaster.PaymentStatus = "paid";
                    }
                    //
                    //if (Math.Round(irMaster.TotalAmountRemaining) - TotalRemainingTDSAmounttemp < 2)
                    //{
                    //    irMaster.IRStatus = "Paid";
                    //    irMaster.PaymentStatus = "paid";
                    //}
                    //irMaster.TotalRemainingTDSAmount = Math.Round(irMaster.TotalAmountRemaining * tdsper / 100);

                    //context.Commit();
                }
            }
        }
    }
}