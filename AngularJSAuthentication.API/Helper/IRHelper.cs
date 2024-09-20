using AngularJSAuthentication.API.Controllers;
using AngularJSAuthentication.Model;
using AngularJSAuthentication.Model.Account;
using NLog;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Transactions;
using System.Web.Script.Serialization;
using AngularJSAuthentication.Model.PurchaseOrder;
using AngularJSAuthentication.Model.PurchaseRequestPayments;
using AngularJSAuthentication.DataContracts.Shared;

namespace AngularJSAuthentication.API.Helper
{
    public class IRHelper
    {

        public readonly string partial = "Partial";
        public readonly string full = "Full";
        public readonly string advance = "Advance";
        public readonly string irPaidStatus = "paid";


        public IRPaymentDetails UploadIRPayment(IRPaymentDetails details, int userid)
        {
            IRPaymentDetails irPaymentDetail = null;
            Guid id = Guid.NewGuid();
            TransactionOptions option = new TransactionOptions();
            option.IsolationLevel = IsolationLevel.RepeatableRead;
            option.Timeout = TimeSpan.FromSeconds(90);
            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, option))
            // using (var scope = new TransactionScope())
            {
                using (var authContext = new AuthContext())
                {
                    irPaymentDetail = SupplierNewPayment(authContext, details, id, userid);
                    bool IsSupplierAdvance = true;
                    int IRpaymentId = irPaymentDetail.Id;
                    var amount = details.TotalReaminingAmount;
                    DebitLedgerEntrySupplieradvacepay(details.SupplierId, authContext, amount ?? 0, userid, details.RefNo, details.Remark, details.Guid, details.BankId, IsSupplierAdvance, IRpaymentId, details.PaymentDate);
                    MakeIRPaymentHistory(details.TotalReaminingAmount, 0, IRpaymentId, 0, userid, advance, authContext);
                    authContext.Commit();
                }
                scope.Complete();
            }
            return irPaymentDetail;
        }

        public IRPaymentDetails SettlerPaymentList(IRPaymentDetails details, int userid)
        {
            TransactionOptions option = new TransactionOptions();
            option.IsolationLevel = IsolationLevel.RepeatableRead;
            option.Timeout = TimeSpan.FromSeconds(90);
            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, option))
            //  using (var scope = new TransactionScope())
            {
                using (var authContext = new AuthContext())
                {
                    IRPaymentDetails iRPaymentDetailDB = authContext.IRPaymentDetailsDB.Where(x => x.Id == details.Id).FirstOrDefault();
                    JavaScriptSerializer js = new JavaScriptSerializer();
                    List<IRMasterDTO> irMasterList = js.Deserialize<List<IRMasterDTO>>(details.IRList);

                    iRPaymentDetailDB.TotalReaminingAmount = details.TotalReaminingAmount;
                    List<LadgerEntry> listOfAdvacePaymentLedger = authContext.LadgerEntryDB.Where(x => x.IsSupplierAdvancepay == true && x.IrPaymentDetailsId == iRPaymentDetailDB.Id).ToList();

                    //Update for First Entry 
                    var firstID = irMasterList.First();
                    UpdateLedgerEntryIRPayment(listOfAdvacePaymentLedger, authContext, details.SupplierId, firstID.Id, firstID.IRID, details.Remark, firstID.TotalAmountRemaining, userid, irMasterList.First(), iRPaymentDetailDB.Id);

                    string status = "";
                    if (firstID.PaymentStatus == irPaidStatus)
                    {
                        status = full;
                    }
                    else
                    {
                        status = partial;

                    }
                    MakeIRPaymentHistory(firstID.TotalAmountRemaining, firstID.Id, iRPaymentDetailDB.Id, firstID.PurchaseOrderId, userid, status, authContext);

                    if (irMasterList.Count > 1)
                    {
                        irMasterList = irMasterList.Skip(1).ToList();

                        foreach (var item in irMasterList)
                        {
                            // Edit status in IR Status ,  Payment status and add paid amount in IR Master 
                            IRMaster irdata = UpdateIRMaster(item, authContext);
                            //end

                            //insert ladger entry 
                            IRHelper.DebitLedgerEntryIRPayment(irdata.supplierId, authContext, item.TotalAmountRemaining, irdata.IRID, irdata.Id, userid, irdata.Discount, "IR", details.RefNo, details.Remark, details.Guid, details.BankId, iRPaymentDetailDB.PaymentDate, iRPaymentDetailDB.Id);
                            //end

                            string irstatus = "";
                            if (item.PaymentStatus == irPaidStatus)
                            {
                                irstatus = full;
                            }
                            else
                            {
                                irstatus = partial;

                            }

                            MakeIRPaymentHistory(item.TotalAmountRemaining, item.Id, iRPaymentDetailDB.Id, item.PurchaseOrderId, userid, irstatus, authContext);
                        }

                        if (details.TotalReaminingAmount > 0)
                        {
                            bool IsSupplierAdvance = true;
                            var amount = details.TotalReaminingAmount;

                            DebitLedgerEntrySupplieradvacepay(details.SupplierId, authContext, amount ?? 0, userid, details.RefNo, details.Remark, details.Guid, details.BankId, IsSupplierAdvance, iRPaymentDetailDB.Id, details.PaymentDate);
                            MakeIRPaymentHistory(details.TotalReaminingAmount, 0, iRPaymentDetailDB.Id, 0, userid, advance, authContext);

                        }
                    }
                }
                scope.Complete();
            }
            return null;
        }
        public IRPaymentDetails SupplierPaymentList(IRPaymentDetails details, int userid)
        {
            TransactionOptions option = new TransactionOptions();
            option.IsolationLevel = IsolationLevel.RepeatableRead;
            option.Timeout = TimeSpan.FromSeconds(90);
            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, option))
            //using (var scope = new TransactionScope())
            {
                try
                {
                    using (var authContext = new AuthContext())
                    {
                        Guid id = Guid.NewGuid();


                        IRPaymentDetails irpaymentdata = null;
                        if (details.Id > 0)
                        {
                            irpaymentdata = authContext.IRPaymentDetailsDB.Where(x => x.Id == details.Id).FirstOrDefault();
                        }

                        JavaScriptSerializer js = new JavaScriptSerializer();
                        List<IRMasterDTO> irDetails = null;
                        if (!string.IsNullOrEmpty(details.IRList))
                        {
                            irDetails = js.Deserialize<List<IRMasterDTO>>(details.IRList);
                        }

                        IRPaymentDetails irPaymentDetails = null;
                        // if new payment 
                        if (irpaymentdata == null)
                        {
                            irPaymentDetails = SupplierNewPayment(authContext, details, id, userid);
                            MakeIrLedgerEntry(irDetails, authContext, userid, details, irPaymentDetails.Id);
                        }
                        //adjust entry
                        else
                        {
                            //Ir payment details id
                            int IRpaymentId = 0;
                            if (irPaymentDetails != null)
                            {
                                IRpaymentId = irPaymentDetails.Id;
                            }
                            else
                            {
                                IRpaymentId = details.Id;
                            }
                            //end


                            irpaymentdata.TotalReaminingAmount = details.TotalReaminingAmount;
                            authContext.Entry(irpaymentdata).State = EntityState.Modified;
                            List<LadgerEntry> listOfAdvacePaymentLedger = authContext.LadgerEntryDB.Where(x => x.IsSupplierAdvancepay == true && x.IrPaymentDetailsId == irpaymentdata.Id).ToList();

                            var firstID = irDetails.First();
                            UpdateLedgerEntryIRPayment(listOfAdvacePaymentLedger, authContext, details.SupplierId, firstID.Id, firstID.IRID, details.Remark, firstID.TotalAmountRemaining, userid, irDetails.First(), IRpaymentId);

                            if (details.TotalReaminingAmount > 0 && listOfAdvacePaymentLedger.Count > 0)
                            {
                                details.PaymentDate = listOfAdvacePaymentLedger.First().Date;
                            }

                            string status = "";
                            if (firstID.PaymentStatus == irPaidStatus)
                            {
                                status = full;
                            }
                            else
                            {
                                status = partial;

                            }
                            MakeIRPaymentHistory(firstID.TotalAmountRemaining, firstID.Id, IRpaymentId, firstID.PurchaseOrderId, userid, status, authContext);
                            if (irDetails.Count > 1)
                            {
                                irDetails = irDetails.Skip(1).ToList();

                                foreach (var item in irDetails)
                                {
                                    // Edit status in IR Status ,  Payment status and add paid amount in IR Master 
                                    IRMaster irdata = UpdateIRMaster(item, authContext);
                                    //end
                                    //Ir payment details id

                                    if (irPaymentDetails != null)
                                    {
                                        IRpaymentId = irPaymentDetails.Id;
                                    }
                                    else
                                    {
                                        IRpaymentId = details.Id;
                                    }
                                    //end

                                    //insert ladger entry 
                                    IRHelper.DebitLedgerEntryIRPayment(irdata.supplierId, authContext, item.TotalAmountRemaining, irdata.IRID, irdata.Id, userid, irdata.Discount, "IR", details.RefNo, details.Remark, details.Guid, details.BankId, irpaymentdata.PaymentDate, IRpaymentId);
                                    //end

                                    string irstatus = "";
                                    if (item.PaymentStatus == irPaidStatus)
                                    {
                                        irstatus = full;
                                    }
                                    else
                                    {
                                        irstatus = partial;

                                    }

                                    MakeIRPaymentHistory(item.TotalAmountRemaining, item.Id, IRpaymentId, item.PurchaseOrderId, userid, irstatus, authContext);
                                }

                            }



                        }
                        // advance amount entry
                        if (details.TotalReaminingAmount > 0)
                        {
                            bool IsSupplierAdvance = true;
                            int IRpaymentId = 0;
                            if (irPaymentDetails != null)
                            {
                                IRpaymentId = irPaymentDetails.Id;
                            }
                            else
                            {
                                IRpaymentId = details.Id;
                            }
                            var amount = details.TotalReaminingAmount;

                            DebitLedgerEntrySupplieradvacepay(details.SupplierId, authContext, amount ?? 0, userid, details.RefNo, details.Remark, details.Guid, details.BankId, IsSupplierAdvance, IRpaymentId, details.PaymentDate);
                            MakeIRPaymentHistory(details.TotalReaminingAmount, 0, IRpaymentId, 0, userid, advance, authContext);

                        }


                        scope.Complete();
                        return details;
                    }
                }
                catch (Exception ex)
                {
                    scope.Dispose();
                    throw ex;
                }
            }

        }

        public static void UpdateSupplierData(PutIrDTO data, int compid, AuthContext context, int userid)
        {

            IRMaster IRR = context.IRMasterDB.Where(q => q.Id == data.Id && q.PurchaseOrderId == data.PurchaseOrderId).SingleOrDefault();
            InvoiceImage invoiceImage = context.InvoiceImageDb.FirstOrDefault(i => i.InvoiceNumber == IRR.IRID && i.PurchaseOrderId == data.PurchaseOrderId);

            try
            {
                var amount = IRR.TotalAmount + (IRR.Discount ?? 0) - (IRR.Gstamt ?? 0);



                if (IRR.OtherAmount > 0)
                {
                    if (IRR.OtherAmountType == "ADD")
                    {
                        amount = amount - IRR.OtherAmount ?? 0;
                    }
                    else if (IRR.OtherAmountType == "MINUS")
                    {
                        amount = amount + IRR.OtherAmount ?? 0;
                    }
                }
                if (IRR.ExpenseAmount > 0)
                {
                    if (IRR.ExpenseAmountType == "ADD")
                    {
                        amount = amount - IRR.ExpenseAmount ?? 0;
                    }
                    else if (IRR.ExpenseAmountType == "MINUS")
                    {
                        amount = amount + IRR.ExpenseAmount ?? 0;
                    }
                }
                if (IRR.RoundofAmount > 0)
                {
                    if (IRR.RoundoffAmountType == "ADD")
                    {
                        amount = amount - IRR.RoundofAmount ?? 0;
                    }
                    else if (IRR.RoundoffAmountType == "MINUS")
                    {
                        amount = amount + IRR.RoundofAmount ?? 0;
                    }
                }
                UpdateLedgerEntry(IRR.supplierId, context, amount, IRR.Gstamt, userid, invoiceImage.InvoiceDate, IRR.IRID, data.Id, data.discount, IRR);

            }
            catch (Exception ex)
            {

            }


            /// ----------------------------Start----------------------------------/// 
            /// --Supplier Payment from upload Invoice recieved Amount and Reciept--// 
            /// 



            InvoiceImage irdata = context.InvoiceImageDb.Where(x => x.PurchaseOrderId == data.PurchaseOrderId && x.InvoiceNumber == data.IRID).FirstOrDefault();
            if (irdata != null)
            {
                irdata.IRAmount = IRR.TotalAmount;
                context.Entry(irdata).State = EntityState.Modified;
            }
            SupplierPaymentData spd = new SupplierPaymentData();
            spd.PurchaseOrderId = IRR.PurchaseOrderId;
            spd.InVoiceNumber = IRR.IRID;
            spd.CreditInVoiceAmount = IRR.TotalAmount;
            spd.active = false;
            spd.CompanyId = compid;
            spd.WarehouseId = IRR.WarehouseId;
            spd.InVoiceDate = irdata.InvoiceDate;
            spd.IRLogoURL = irdata.IRLogoURL;
            spd.SupplierId = irdata.SupplierId;
            spd.SupplierName = irdata.SupplierName;
            spd.IRLogoURL = irdata.IRLogoURL;
            spd.CreatedDate = DateTime.Now;
            spd.UpdatedDate = DateTime.Now;
            spd.PaymentStatusCorD = "Credit";
            spd.Deleted = false;
            spd.VoucherType = "Purchase";
            spd.Perticular = irdata.Perticular;
            if (irdata.SupplierId > 0)
            {
                var supplierpay = context.SupplierPaymentDataDB.Where(x => x.SupplierId == irdata.SupplierId && x.WarehouseId == IRR.WarehouseId && x.CompanyId == compid).ToList();
                if (supplierpay.Count != 0)
                {
                    List<SupplierPaymentData> maxTimestamp2 = context.SupplierPaymentDataDB.Where(x => x.SupplierId == irdata.SupplierId && x.WarehouseId == IRR.WarehouseId && x.CompanyId == compid).ToList();
                    SupplierPaymentData LastSupplierData = maxTimestamp2.LastOrDefault();
                    spd.ClosingBalance = LastSupplierData.ClosingBalance + IRR.TotalAmount;
                }
                else
                {
                    spd.ClosingBalance = IRR.TotalAmount;
                }
                context.SupplierPaymentDataDB.Add(spd);
                context.Commit();

                var SupplierPaymentData = context.FullSupplierPaymentDataDB.Where(x => x.SupplierId == irdata.SupplierId && x.CompanyId == compid && x.WarehouseId == IRR.WarehouseId).SingleOrDefault();
                var supplier = context.Suppliers.Where(x => x.SupplierId == irdata.SupplierId /*&& x.WarehouseId == IRR.WarehouseId*/ && x.CompanyId == compid).SingleOrDefault();
                if (SupplierPaymentData == null)
                {
                    FullSupplierPaymentData dd = new FullSupplierPaymentData();
                    dd.InVoiceAmount = IRR.TotalAmount;
                    dd.InVoiceRemainingAmount = IRR.TotalAmount;
                    dd.active = true;
                    dd.CompanyId = compid;
                    dd.WarehouseId = IRR.WarehouseId;
                    dd.SupplierPaymentStatus = "UnPaid";
                    dd.SupplierPaymentTerms = supplier.PaymentTerms;
                    dd.SupplierId = irdata.SupplierId;
                    dd.SupplierName = supplier.Name;
                    dd.CreatedDate = DateTime.Now;
                    dd.UpdatedDate = DateTime.Now;
                    dd.Deleted = false;
                    context.FullSupplierPaymentDataDB.Add(dd);
                    context.Commit();
                }
                else
                {
                    FullSupplierPaymentData dd = context.FullSupplierPaymentDataDB.Where(x => x.SupplierId == irdata.SupplierId && x.CompanyId == compid && x.WarehouseId == IRR.WarehouseId).SingleOrDefault();
                    dd.InVoiceAmount = SupplierPaymentData.InVoiceAmount + IRR.TotalAmount;
                    dd.InVoiceRemainingAmount = SupplierPaymentData.InVoiceRemainingAmount + IRR.TotalAmount;
                    dd.SupplierPaymentStatus = SupplierPaymentData.SupplierPaymentStatus;
                    dd.UpdatedDate = DateTime.Now;
                    context.FullSupplierPaymentDataDB.Attach(dd);
                    context.Entry(dd).State = EntityState.Modified;
                    context.Commit();
                }
                double totwithouttax = IRR.TotalAmount - IRR.Gstamt ?? 0;
            }

        }

        public static void UpdateSupplierDataNew(int irId, int purchaseOrderId, int compid, AuthContext context, int userid)
        {

            IRMaster IRR = context.IRMasterDB.Where(q => q.Id == irId && q.PurchaseOrderId == purchaseOrderId).SingleOrDefault();
            InvoiceImage invoiceImage = context.InvoiceImageDb.FirstOrDefault(i => i.InvoiceNumber == IRR.IRID && i.PurchaseOrderId == purchaseOrderId);
            PurchaseRequestPayment purchaseRequestPayment = context.PurchaseRequestPaymentsDB.Where(x => x.PurchaseOrderId == purchaseOrderId && x.PrPaymentStatus == "Approved").FirstOrDefault();

            try
            {
                var amount = IRR.TotalAmount + (IRR.Discount ?? 0) - (IRR.Gstamt ?? 0);

                if (IRR.OtherAmountType == null)
                {
                    if (IRR.OtherAmount != null)
                    {
                        if (IRR.OtherAmount > 0)
                        {
                            amount = amount - IRR.OtherAmount ?? 0;
                        }
                        else
                        {
                            amount = amount + IRR.OtherAmount * (-1) ?? 0;

                        }
                    }
                }
                if (IRR.ExpenseAmountType == null)
                {
                    if (IRR.ExpenseAmount != null)
                    {
                        if (IRR.ExpenseAmount > 0)
                        {
                            amount = amount - IRR.ExpenseAmount ?? 0;
                        }
                        else
                        {
                            amount = amount + IRR.ExpenseAmount * (-1) ?? 0;

                        }

                    }
                }
                if (IRR.RoundoffAmountType == null)
                {
                    if (IRR.RoundofAmount != null)
                    {
                        if (IRR.RoundofAmount > 0)
                        {
                            amount = amount - IRR.RoundofAmount ?? 0;
                        }
                        else
                        {
                            amount = amount + IRR.RoundofAmount * (-1) ?? 0;

                        }
                    }
                }

                if (IRR.OtherAmount > 0)
                {
                    if (IRR.OtherAmountType == "ADD")
                    {
                        amount = amount - IRR.OtherAmount ?? 0;
                    }
                    else if (IRR.OtherAmountType == "MINUS")
                    {
                        amount = amount + IRR.OtherAmount ?? 0;
                    }
                }
                if (IRR.ExpenseAmount > 0)
                {
                    if (IRR.ExpenseAmountType == "ADD")
                    {
                        amount = amount - IRR.ExpenseAmount ?? 0;
                    }
                    else if (IRR.ExpenseAmountType == "MINUS")
                    {
                        amount = amount + IRR.ExpenseAmount ?? 0;
                    }
                }
                if (IRR.RoundofAmount > 0)
                {
                    if (IRR.RoundoffAmountType == "ADD")
                    {
                        amount = amount - IRR.RoundofAmount ?? 0;
                    }
                    else if (IRR.RoundoffAmountType == "MINUS")
                    {
                        amount = amount + IRR.RoundofAmount ?? 0;
                    }
                }
                UpdateLedgerEntry(IRR.supplierId, context, amount, IRR.Gstamt, userid, IRR.InvoiceDate ?? DateTime.Now, IRR.IRID, irId, IRR.Discount, IRR);
                PurchaseRequestSettlementHelper purchaseRequestSettlementHelper = new PurchaseRequestSettlementHelper();
                DateTime currentTime = DateTime.Now;
                Guid guid = Guid.NewGuid();

                List<IRCreditNoteMaster> IRDebitNote = context.IRCreditNoteMaster.Where(q => q.IRMasterId == irId && q.IsDeleted == false && q.IsDebitNoteGenerate == true).Include(q => q.IRCreditNoteDetails).ToList();
                double IrAmount = IRR.TotalAmountRemaining;
                double debitnoteamount = 0;
                if(IRDebitNote.Count>0)
                {
                    foreach(var item in IRDebitNote)
                    {
                        double Debitamount = item.IRCreditNoteDetails.Sum(x => (x.IRPrice * (x.DamageQty + x.ShortQty + x.ExpiryQty) - x.Discount ?? 0));
                        double DebitTaxamount = item.IRCreditNoteDetails.Sum(x => (x.IRPrice * (x.TotalTaxPercentage * (x.DamageQty + x.ShortQty + x.ExpiryQty) - x.Discount ?? 0)) / 100);
                        double DebitCessamount = item.IRCreditNoteDetails.Sum(x => (x.IRPrice * (x.CessTaxPercentage * (x.DamageQty + x.ShortQty + x.ExpiryQty) - x.Discount)) / 100) ?? 0;

                        debitnoteamount += (Debitamount + DebitTaxamount + DebitCessamount);
                    }
                    IrAmount = IRR.TotalAmountRemaining - debitnoteamount;
                }
                //if (IRDebitNote != null && IRDebitNote.IsDebitNoteGenerate)
                //{
                //    double Debitamount = IRDebitNote.IRCreditNoteDetails.Sum(x => (x.IRPrice * (x.DamageQty + x.ShortQty + x.ExpiryQty) - x.Discount ?? 0));
                //    double DebitTaxamount = IRDebitNote.IRCreditNoteDetails.Sum(x => (x.IRPrice * (x.TotalTaxPercentage * (x.DamageQty + x.ShortQty + x.ExpiryQty) - x.Discount ?? 0)) / 100);
                //    double DebitCessamount = IRDebitNote.IRCreditNoteDetails.Sum(x => (x.IRPrice * (x.CessTaxPercentage * (x.DamageQty + x.ShortQty + x.ExpiryQty) - x.Discount)) / 100) ?? 0;

                //    debitnoteamount = (Debitamount + DebitTaxamount + DebitCessamount);
                //    IrAmount = IRR.TotalAmountRemaining - debitnoteamount;

                //}
                purchaseRequestSettlementHelper.SettleAmount(context, IRR, IrAmount, userid, currentTime, guid, false, debitnoteamount);
                //if (purchaseRequestPayment != null) {
                //    UpdateLedgerEntryWithAdvancePR(IRR.supplierId, context, userid, IRR.InvoiceDate??DateTime.Now, IRR.IRID, irId, purchaseRequestPayment);

                //}
            }
            catch (Exception ex)
            {
                throw ex;
            }


            /// ----------------------------Start----------------------------------/// 
            /// --Supplier Payment from upload Invoice recieved Amount and Reciept--// 
            /// 

            //InvoiceImage irdata = context.InvoiceImageDb.Where(x => x.PurchaseOrderId == purchaseOrderId && x.InvoiceNumber == IRR.IRID).FirstOrDefault();
            if (invoiceImage != null)
            {
                invoiceImage.IRAmount = IRR.TotalAmount;
                context.Entry(invoiceImage).State = EntityState.Modified;
                context.Commit();
            }
            //SupplierPaymentData spd = new SupplierPaymentData();
            //spd.PurchaseOrderId = IRR.PurchaseOrderId;
            //spd.InVoiceNumber = IRR.IRID;
            //spd.CreditInVoiceAmount = IRR.TotalAmount;
            //spd.active = false;
            //spd.CompanyId = compid;
            //spd.WarehouseId = IRR.WarehouseId;
            //spd.InVoiceDate = irdata.InvoiceDate;
            //spd.IRLogoURL = irdata.IRLogoURL;
            //spd.SupplierId = irdata.SupplierId;
            //spd.SupplierName = irdata.SupplierName;
            //spd.IRLogoURL = irdata.IRLogoURL;
            //spd.CreatedDate = DateTime.Now;
            //spd.UpdatedDate = DateTime.Now;
            //spd.PaymentStatusCorD = "Credit";
            //spd.Deleted = false;
            //spd.VoucherType = "Purchase";
            //spd.Perticular = irdata.Perticular;

            //if (irdata.SupplierId > 0)
            //{
            //    var supplierpay = context.SupplierPaymentDataDB.Where(x => x.SupplierId == irdata.SupplierId && x.WarehouseId == IRR.WarehouseId && x.CompanyId == compid).ToList();
            //    if (supplierpay.Count != 0)
            //    {
            //        List<SupplierPaymentData> maxTimestamp2 = context.SupplierPaymentDataDB.Where(x => x.SupplierId == irdata.SupplierId && x.WarehouseId == IRR.WarehouseId && x.CompanyId == compid).ToList();
            //        SupplierPaymentData LastSupplierData = maxTimestamp2.LastOrDefault();
            //        spd.ClosingBalance = LastSupplierData.ClosingBalance + IRR.TotalAmount;
            //    }
            //    else
            //    {
            //        spd.ClosingBalance = IRR.TotalAmount;
            //    }
            //    context.SupplierPaymentDataDB.Add(spd);
            //    context.Commit();

            //    var SupplierPaymentData = context.FullSupplierPaymentDataDB.Where(x => x.SupplierId == irdata.SupplierId && x.CompanyId == compid && x.WarehouseId == IRR.WarehouseId).SingleOrDefault();
            //    var supplier = context.Suppliers.Where(x => x.SupplierId == irdata.SupplierId /*&& x.WarehouseId == IRR.WarehouseId*/ && x.CompanyId == compid).SingleOrDefault();
            //    if (SupplierPaymentData == null)
            //    {
            //        FullSupplierPaymentData dd = new FullSupplierPaymentData();
            //        dd.InVoiceAmount = IRR.TotalAmount;
            //        dd.InVoiceRemainingAmount = IRR.TotalAmount;
            //        dd.active = true;
            //        dd.CompanyId = compid;
            //        dd.WarehouseId = IRR.WarehouseId;
            //        dd.SupplierPaymentStatus = "UnPaid";
            //        dd.SupplierPaymentTerms = supplier.PaymentTerms;
            //        dd.SupplierId = irdata.SupplierId;
            //        dd.SupplierName = supplier.Name;
            //        dd.CreatedDate = DateTime.Now;
            //        dd.UpdatedDate = DateTime.Now;
            //        dd.Deleted = false;
            //        context.FullSupplierPaymentDataDB.Add(dd);
            //        context.Commit();
            //    }
            //    else
            //    {
            //        FullSupplierPaymentData dd = context.FullSupplierPaymentDataDB.Where(x => x.SupplierId == irdata.SupplierId && x.CompanyId == compid && x.WarehouseId == IRR.WarehouseId).SingleOrDefault();
            //        dd.InVoiceAmount = SupplierPaymentData.InVoiceAmount + IRR.TotalAmount;
            //        dd.InVoiceRemainingAmount = SupplierPaymentData.InVoiceRemainingAmount + IRR.TotalAmount;
            //        dd.SupplierPaymentStatus = SupplierPaymentData.SupplierPaymentStatus;
            //        dd.UpdatedDate = DateTime.Now;
            //        context.FullSupplierPaymentDataDB.Attach(dd);
            //        context.Entry(dd).State = EntityState.Modified;
            //        context.Commit();
            //    }
            //    double totwithouttax = IRR.TotalAmount - IRR.Gstamt ?? 0;
            //}

        }


        ///-----------------------------End-----------------------------------///

        public static void UpdateLedgerEntry(int supplierID, AuthContext context, double creditAmount, double? taxAmount, int userId, DateTime date, string invoiceNumber, int irID, double? discount, IRMaster IRR)
        {
            try
            {
                LadgerHelper ladgerHelper = new LadgerHelper();
                //var ledgerIDQuery = from l in context.LadgerDB
                //                    join lt in context.LadgerTypeDB
                //                    on l.LadgertypeID equals lt.ID
                //                    where lt.code.ToLower() == "supplier"
                //                        && l.ObjectID == supplierID
                //                    select l.ID;
                Ladger ledger = ladgerHelper.GetOrCreateLadgerTypeAndLadger("Supplier", supplierID, userId, context);
                long? ladgerID = ledger.ID;

                var purchaseLedgerQuery = from l in context.LadgerDB
                                          join lt in context.LadgerTypeDB
                                          on l.LadgertypeID equals lt.ID
                                          where lt.code.ToLower() == "purchase"
                                          select l.ID;


                var taxLedgerQuery = from l in context.LadgerDB
                                     join lt in context.LadgerTypeDB
                                     on l.LadgertypeID equals lt.ID
                                     where lt.code.ToLower() == "tax"
                                     && l.Name == "generaltax"
                                     select l.ID;

                var purchaseDiscountLedgerQuery = from l in context.LadgerDB
                                                  join lt in context.LadgerTypeDB
                                                  on l.LadgertypeID equals lt.ID
                                                  where lt.code.ToLower() == "purchasediscount"
                                                  select l.ID;



                if (ladgerID == null || ladgerID == 0)
                {
                    ladgerID = GenerateSupplier(supplierID, context, userId);
                }

                //if (invoiceNumber.Count() > 8)
                //{
                //    invoiceNumber = invoiceNumber.Substring(0, 7);
                //}
                var id = GenerateVoucherNumber(invoiceNumber, context, userId);

                //Purchase Ladger
                LadgerEntry ledgerEntry = new LadgerEntry();
                ledgerEntry.LagerID = ladgerID;
                ledgerEntry.Credit = creditAmount;
                ledgerEntry.Active = true;
                ledgerEntry.CreatedBy = userId;
                ledgerEntry.CreatedDate = DateTime.Now;
                ledgerEntry.UpdatedBy = userId;
                ledgerEntry.UpdatedDate = DateTime.Now;
                ledgerEntry.Date = date;
                ledgerEntry.AffectedLadgerID = purchaseLedgerQuery.First();
                ledgerEntry.VouchersTypeID = context.VoucherTypeDB.Where(x => x.Name.ToLower() == "purchase").First().ID;
                ledgerEntry.VouchersNo = id;
                ledgerEntry.Particulars = invoiceNumber;
                ledgerEntry.ObjectID = (long)irID;
                ledgerEntry.ObjectType = "IR";
                context.LadgerEntryDB.Add(ledgerEntry);

                LadgerEntry oppositeLedgerEntry = new LadgerEntry();
                oppositeLedgerEntry.LagerID = ledgerEntry.AffectedLadgerID;
                oppositeLedgerEntry.Debit = creditAmount;
                oppositeLedgerEntry.Active = true;
                oppositeLedgerEntry.CreatedBy = userId;
                oppositeLedgerEntry.CreatedDate = DateTime.Now;
                oppositeLedgerEntry.UpdatedBy = userId;
                oppositeLedgerEntry.UpdatedDate = DateTime.Now;
                oppositeLedgerEntry.Date = date;
                oppositeLedgerEntry.AffectedLadgerID = ledgerEntry.LagerID;
                oppositeLedgerEntry.VouchersTypeID = ledgerEntry.VouchersTypeID;
                oppositeLedgerEntry.VouchersNo = ledgerEntry.VouchersNo;
                oppositeLedgerEntry.Particulars = invoiceNumber;
                oppositeLedgerEntry.ObjectID = (long)irID;
                oppositeLedgerEntry.ObjectType = "IR";
                context.LadgerEntryDB.Add(oppositeLedgerEntry);


                if (discount != null && discount.Value > 0)
                {
                    ledgerEntry = new LadgerEntry();
                    ledgerEntry.LagerID = ladgerID;
                    ledgerEntry.Debit = discount;
                    ledgerEntry.Active = true;
                    ledgerEntry.CreatedBy = userId;
                    ledgerEntry.CreatedDate = DateTime.Now;
                    ledgerEntry.UpdatedBy = userId;
                    ledgerEntry.UpdatedDate = DateTime.Now;
                    ledgerEntry.Date = date;
                    ledgerEntry.AffectedLadgerID = purchaseDiscountLedgerQuery.First();
                    ledgerEntry.VouchersTypeID = context.VoucherTypeDB.Where(x => x.Name.ToLower() == "journal").First().ID;
                    ledgerEntry.VouchersNo = id;
                    ledgerEntry.Particulars = invoiceNumber;
                    ledgerEntry.ObjectID = (long)irID;
                    ledgerEntry.ObjectType = "IR";
                    ledgerEntry.UploadGUID = "";
                    context.LadgerEntryDB.Add(ledgerEntry);

                    oppositeLedgerEntry = new LadgerEntry();
                    oppositeLedgerEntry.LagerID = ledgerEntry.AffectedLadgerID;
                    oppositeLedgerEntry.Credit = discount;
                    oppositeLedgerEntry.Active = true;
                    oppositeLedgerEntry.CreatedBy = userId;
                    oppositeLedgerEntry.CreatedDate = DateTime.Now;
                    oppositeLedgerEntry.UpdatedBy = userId;
                    oppositeLedgerEntry.UpdatedDate = DateTime.Now;
                    oppositeLedgerEntry.Date = date;
                    oppositeLedgerEntry.AffectedLadgerID = ledgerEntry.LagerID;
                    oppositeLedgerEntry.VouchersTypeID = ledgerEntry.VouchersTypeID;
                    oppositeLedgerEntry.VouchersNo = id;
                    oppositeLedgerEntry.Particulars = invoiceNumber;
                    oppositeLedgerEntry.ObjectID = (long)irID;
                    oppositeLedgerEntry.ObjectType = "IR";
                    oppositeLedgerEntry.UploadGUID = "";
                    context.LadgerEntryDB.Add(oppositeLedgerEntry);
                }


                //TAX Ledger
                //if (taxAmount > 0)
                //{
                //    ledgerEntry = new LadgerEntry();
                //    ledgerEntry.LagerID = ladgerID;
                //    ledgerEntry.Credit = taxAmount;
                //    ledgerEntry.Active = true;
                //    ledgerEntry.CreatedBy = userId;
                //    ledgerEntry.CreatedDate = DateTime.Now;
                //    ledgerEntry.UpdatedBy = userId;
                //    ledgerEntry.UpdatedDate = DateTime.Now;
                //    ledgerEntry.Date = date;
                //    ledgerEntry.AffectedLadgerID = taxLedgerQuery.First();
                //    ledgerEntry.VouchersTypeID = context.VoucherTypeDB.Where(x => x.Name.ToLower() == "tax").First().ID;
                //    ledgerEntry.VouchersNo = id;
                //    ledgerEntry.Particulars = invoiceNumber;
                //    ledgerEntry.ObjectID = (long)irID;
                //    ledgerEntry.ObjectType = "IR";
                //    context.LadgerEntryDB.Add(ledgerEntry);

                //    oppositeLedgerEntry = new LadgerEntry();
                //    oppositeLedgerEntry.LagerID = ledgerEntry.AffectedLadgerID;
                //    oppositeLedgerEntry.Debit = taxAmount;
                //    oppositeLedgerEntry.Active = true;
                //    oppositeLedgerEntry.CreatedBy = userId;
                //    oppositeLedgerEntry.CreatedDate = DateTime.Now;
                //    oppositeLedgerEntry.UpdatedBy = userId;
                //    oppositeLedgerEntry.UpdatedDate = DateTime.Now;
                //    oppositeLedgerEntry.Date = date;
                //    oppositeLedgerEntry.AffectedLadgerID = ledgerEntry.LagerID;
                //    oppositeLedgerEntry.VouchersTypeID = context.VoucherTypeDB.Where(x => x.Name.ToLower() == "tax").First().ID;
                //    oppositeLedgerEntry.VouchersNo = ledgerEntry.VouchersNo;
                //    oppositeLedgerEntry.Particulars = invoiceNumber;
                //    oppositeLedgerEntry.ObjectID = (long)irID;
                //    oppositeLedgerEntry.ObjectType = "IR";
                //    context.LadgerEntryDB.Add(oppositeLedgerEntry);

                //}

                //Other amount Ledger
                if (IRR.OtherAmount != null)
                {
                    AddOtherAmount(context, id, invoiceNumber, irID, userId, date, ladgerID, IRR);
                }

                // Expense amount Ledger
                if (IRR.ExpenseAmount != null)
                {
                    AddExpenseAmount(context, id, invoiceNumber, irID, userId, date, ladgerID, IRR);
                }

                // Round Off Ledger
                if (IRR.RoundofAmount != null)
                {
                    AddRoundOffAmount(context, id, invoiceNumber, irID, userId, date, ladgerID, IRR);
                }

                if (taxAmount > 0)
                {
                    IRHelper irdata = new IRHelper();
                    irdata.updatetaxsupplierlagderentry(taxAmount, supplierID, userId, date, context, id, invoiceNumber, irID);

                }

                context.Commit();

            }
            catch (Exception ex)
            {
                Logger logger = LogManager.GetCurrentClassLogger();
                logger.Error("Credit Supplier Ledger Issue: \n" + ex.Message);
            }
        }

        public static void UpdateDiscountOnly(int supplierID, AuthContext context, double creditAmount, double? taxAmount, int userId, DateTime date, string invoiceNumber, int irID, double? discount, IRMaster IRR)
        {
            try
            {
                LadgerHelper ladgerHelper = new LadgerHelper();
                //var ledgerIDQuery = from l in context.LadgerDB
                //                    join lt in context.LadgerTypeDB
                //                    on l.LadgertypeID equals lt.ID
                //                    where lt.code.ToLower() == "supplier"
                //                        && l.ObjectID == supplierID
                //                    select l.ID;
                Ladger ledger = ladgerHelper.GetOrCreateLadgerTypeAndLadger("Supplier", supplierID, userId, context);
                long? ladgerID = ledger.ID;

                var purchaseLedgerQuery = from l in context.LadgerDB
                                          join lt in context.LadgerTypeDB
                                          on l.LadgertypeID equals lt.ID
                                          where lt.code.ToLower() == "purchase"
                                          select l.ID;


                var taxLedgerQuery = from l in context.LadgerDB
                                     join lt in context.LadgerTypeDB
                                     on l.LadgertypeID equals lt.ID
                                     where lt.code.ToLower() == "tax"
                                     && l.Name == "generaltax"
                                     select l.ID;

                var purchaseDiscountLedgerQuery = from l in context.LadgerDB
                                                  join lt in context.LadgerTypeDB
                                                  on l.LadgertypeID equals lt.ID
                                                  where lt.code.ToLower() == "purchasediscount"
                                                  select l.ID;



                if (ladgerID == null || ladgerID == 0)
                {
                    ladgerID = GenerateSupplier(supplierID, context, userId);
                }

                //if (invoiceNumber.Count() > 8)
                //{
                //    invoiceNumber = invoiceNumber.Substring(0, 7);
                //}
                var id = GenerateVoucherNumber(invoiceNumber, context, userId);

                //Purchase Ladger
                LadgerEntry ledgerEntry = new LadgerEntry();
                LadgerEntry oppositeLedgerEntry = new LadgerEntry();



                if (discount != null && discount.Value > 0)
                {
                    ledgerEntry = new LadgerEntry();
                    ledgerEntry.LagerID = ladgerID;
                    ledgerEntry.Debit = discount;
                    ledgerEntry.Active = true;
                    ledgerEntry.CreatedBy = userId;
                    ledgerEntry.CreatedDate = DateTime.Now;
                    ledgerEntry.UpdatedBy = userId;
                    ledgerEntry.UpdatedDate = DateTime.Now;
                    ledgerEntry.Date = date;
                    ledgerEntry.AffectedLadgerID = purchaseDiscountLedgerQuery.First();
                    ledgerEntry.VouchersTypeID = context.VoucherTypeDB.Where(x => x.Name.ToLower() == "journal").First().ID;
                    ledgerEntry.VouchersNo = id;
                    ledgerEntry.Particulars = invoiceNumber;
                    ledgerEntry.ObjectID = (long)irID;
                    ledgerEntry.ObjectType = "IR";
                    ledgerEntry.UploadGUID = "";
                    context.LadgerEntryDB.Add(ledgerEntry);

                    oppositeLedgerEntry = new LadgerEntry();
                    oppositeLedgerEntry.LagerID = ledgerEntry.AffectedLadgerID;
                    oppositeLedgerEntry.Credit = discount;
                    oppositeLedgerEntry.Active = true;
                    oppositeLedgerEntry.CreatedBy = userId;
                    oppositeLedgerEntry.CreatedDate = DateTime.Now;
                    oppositeLedgerEntry.UpdatedBy = userId;
                    oppositeLedgerEntry.UpdatedDate = DateTime.Now;
                    oppositeLedgerEntry.Date = date;
                    oppositeLedgerEntry.AffectedLadgerID = ledgerEntry.LagerID;
                    oppositeLedgerEntry.VouchersTypeID = ledgerEntry.VouchersTypeID;
                    oppositeLedgerEntry.VouchersNo = id;
                    oppositeLedgerEntry.Particulars = invoiceNumber;
                    oppositeLedgerEntry.ObjectID = (long)irID;
                    oppositeLedgerEntry.ObjectType = "IR";
                    oppositeLedgerEntry.UploadGUID = "";
                    context.LadgerEntryDB.Add(oppositeLedgerEntry);
                }

                context.Commit();

            }
            catch (Exception ex)
            {
                Logger logger = LogManager.GetCurrentClassLogger();
                logger.Error("Credit Supplier Ledger Issue: \n" + ex.Message);
            }
        }

        private static long GenerateSupplier(int supplierID, AuthContext context, int userId)
        {
            Ladger ladger = new Ladger();
            ladger.Active = true;
            ladger.CreatedBy = userId;
            ladger.UpdatedBy = userId;
            ladger.UpdatedDate = DateTime.Now;
            ladger.CreatedDate = DateTime.Now;
            ladger.ProvidedBankDetails = false;
            ladger.InventoryValuesAreAffected = true;
            ladger.LadgertypeID = context.LadgerTypeDB.Where(x => x.code.ToLower() == "supplier").First().ID;
            ladger.ObjectType = "Supplier";
            ladger.ObjectID = supplierID;
            context.LadgerDB.Add(ladger);
            context.Commit();
            return ladger.ID;
        }

        private static long GenerateVoucherNumber(string invoiceNumber, AuthContext context, int userId)
        {
            Voucher vch = new Voucher
            {
                Active = true,
                Code = invoiceNumber,
                CreatedBy = userId,
                CreatedDate = DateTime.Now,
                UpdatedBy = userId,
                UpdatedDate = DateTime.Now
            };
            context.VoucherDB.Add(vch);
            context.Commit();
            return vch.ID;
        }

        public static void DebitLedgerEntry(int supplierID, AuthContext context, double amount, string invoiceNumber, int irID, int userId, double? discount, string objectType = "IR", string guid = "")
        {
            try
            {

                LadgerHelper ladgerHelper = new LadgerHelper();
                //var ledgerIDQuery = from l in context.LadgerDB
                //                    join lt in context.LadgerTypeDB
                //                    on l.LadgertypeID equals lt.ID
                //                    where lt.code.ToLower() == "supplier"
                //                        && l.ObjectID == supplierID
                //                    select l.ID;
                Ladger ledger = ladgerHelper.GetOrCreateLadgerTypeAndLadger("Supplier", supplierID, userId, context);
                long? ladgerID = ledger.ID;

                var bankLedgerQuery = from l in context.LadgerDB
                                      join lt in context.LadgerTypeDB
                                      on l.LadgertypeID equals lt.ID
                                      where lt.code.ToLower() == "bank"
                                      select l.ID;


                var voucherNumberQuery = from le in context.LadgerEntryDB
                                         where le.LagerID == ladgerID && le.ObjectID == irID
                                         select le.VouchersNo;

                var voucherNumber = voucherNumberQuery.FirstOrDefault();

                LadgerEntry ledgerEntry = new LadgerEntry();
                ledgerEntry.LagerID = ladgerID;
                ledgerEntry.Debit = amount;
                ledgerEntry.Active = true;
                ledgerEntry.CreatedBy = userId;
                ledgerEntry.CreatedDate = DateTime.Now;
                ledgerEntry.UpdatedBy = userId;
                ledgerEntry.UpdatedDate = DateTime.Now;
                ledgerEntry.Date = DateTime.Now;
                ledgerEntry.AffectedLadgerID = bankLedgerQuery.First();
                ledgerEntry.VouchersTypeID = context.VoucherTypeDB.Where(x => x.Name.ToLower() == "payment").First().ID;
                ledgerEntry.VouchersNo = voucherNumber;
                ledgerEntry.Particulars = invoiceNumber;
                ledgerEntry.ObjectID = (long)irID;
                ledgerEntry.ObjectType = objectType;
                ledgerEntry.UploadGUID = guid;
                context.LadgerEntryDB.Add(ledgerEntry);

                LadgerEntry oppositeLedgerEntry = new LadgerEntry();
                oppositeLedgerEntry.LagerID = ledgerEntry.AffectedLadgerID;
                oppositeLedgerEntry.Credit = amount;
                oppositeLedgerEntry.Active = true;
                oppositeLedgerEntry.CreatedBy = userId;
                oppositeLedgerEntry.CreatedDate = DateTime.Now;
                oppositeLedgerEntry.UpdatedBy = userId;
                oppositeLedgerEntry.UpdatedDate = DateTime.Now;
                oppositeLedgerEntry.Date = DateTime.Now;
                oppositeLedgerEntry.AffectedLadgerID = ledgerEntry.LagerID;
                oppositeLedgerEntry.VouchersTypeID = ledgerEntry.VouchersTypeID;
                oppositeLedgerEntry.VouchersNo = voucherNumber;
                oppositeLedgerEntry.Particulars = invoiceNumber;
                oppositeLedgerEntry.ObjectID = (long)irID;
                oppositeLedgerEntry.ObjectType = objectType;
                oppositeLedgerEntry.UploadGUID = guid;
                context.LadgerEntryDB.Add(oppositeLedgerEntry);



                context.Commit();
            }
            catch (Exception ex)
            {
                Logger logger = LogManager.GetCurrentClassLogger();
                logger.Error("Debit Supplier Ledger Issue: \n" + ex.Message);
            }
        }
        private static void AddOtherAmount(AuthContext context, long id, string invoiceNumber, int irID, int userId, DateTime date, long? ladgerID, IRMaster IRR)
        {
            LadgerEntry ledgerEntry = new LadgerEntry();
            LadgerEntry oppositeLedgerEntry = new LadgerEntry();

            var othetLedgerQuery = (from l in context.LadgerDB
                                    join lt in context.LadgerTypeDB
                                    on l.LadgertypeID equals lt.ID
                                    where lt.code.ToLower() == "other"
                                    select l.ID).FirstOrDefault();

            if (othetLedgerQuery == 0) // If other party entry not exist in Ledger
            {
                var LT = (from a in context.LadgerTypeDB where a.code.ToLower() == "other" select a.ID).First();
                if (LT != 0)
                {
                    Ladger LD = new Ladger();
                    LD.Name = "other";
                    LD.Alias = "other";
                    LD.InventoryValuesAreAffected = false;
                    LD.Address = null;
                    LD.Country = null;
                    LD.PinCode = null;
                    LD.ProvidedBankDetails = false;
                    LD.ObjectID = 0;
                    LD.ObjectType = "other";
                    LD.LadgertypeID = LT;
                    LD.Active = true;
                    LD.CreatedBy = 1;
                    LD.CreatedDate = DateTime.Now;
                    LD.UpdatedBy = 1;
                    LD.UpdatedDate = DateTime.Now;
                    context.LadgerDB.Add(LD);
                    context.Commit();
                    // Again call
                    othetLedgerQuery = (from l in context.LadgerDB
                                        join lt in context.LadgerTypeDB
                                        on l.LadgertypeID equals lt.ID
                                        where lt.code.ToLower() == "other"
                                        select l.ID).FirstOrDefault();
                }
            }


            if (IRR.OtherAmount > 0 || IRR.OtherAmountType == "ADD")
            {


                ledgerEntry = new LadgerEntry();
                ledgerEntry.LagerID = ladgerID;
                ledgerEntry.Credit = IRR.OtherAmount;
                ledgerEntry.Active = true;
                ledgerEntry.CreatedBy = userId;
                ledgerEntry.CreatedDate = DateTime.Now;
                ledgerEntry.UpdatedBy = userId;
                ledgerEntry.UpdatedDate = DateTime.Now;
                ledgerEntry.Date = date;
                ledgerEntry.AffectedLadgerID = othetLedgerQuery;
                ledgerEntry.VouchersTypeID = context.VoucherTypeDB.Where(x => x.Name.ToLower() == "other").First().ID;
                ledgerEntry.VouchersNo = id;
                ledgerEntry.Particulars = invoiceNumber;
                ledgerEntry.ObjectID = (long)irID;
                ledgerEntry.ObjectType = "IR";
                context.LadgerEntryDB.Add(ledgerEntry);

                oppositeLedgerEntry = new LadgerEntry();
                oppositeLedgerEntry.LagerID = ledgerEntry.AffectedLadgerID;
                oppositeLedgerEntry.Debit = ledgerEntry.Credit;
                oppositeLedgerEntry.Active = true;
                oppositeLedgerEntry.CreatedBy = userId;
                oppositeLedgerEntry.CreatedDate = DateTime.Now;
                oppositeLedgerEntry.UpdatedBy = userId;
                oppositeLedgerEntry.UpdatedDate = DateTime.Now;
                oppositeLedgerEntry.Date = date;
                oppositeLedgerEntry.AffectedLadgerID = ledgerEntry.LagerID;
                oppositeLedgerEntry.VouchersTypeID = context.VoucherTypeDB.Where(x => x.Name.ToLower() == "other").First().ID;
                oppositeLedgerEntry.VouchersNo = ledgerEntry.VouchersNo;
                oppositeLedgerEntry.Particulars = invoiceNumber;
                oppositeLedgerEntry.ObjectID = (long)irID;
                oppositeLedgerEntry.ObjectType = "IR";
                context.LadgerEntryDB.Add(oppositeLedgerEntry);
            }
            else if (IRR.OtherAmount < 0 || IRR.OtherAmountType == "MINUS")
            {

                ledgerEntry = new LadgerEntry();
                ledgerEntry.LagerID = ladgerID;
                ledgerEntry.Debit = IRR.OtherAmount * (-1);
                ledgerEntry.Active = true;
                ledgerEntry.CreatedBy = userId;
                ledgerEntry.CreatedDate = DateTime.Now;
                ledgerEntry.UpdatedBy = userId;
                ledgerEntry.UpdatedDate = DateTime.Now;
                ledgerEntry.Date = date;
                ledgerEntry.AffectedLadgerID = othetLedgerQuery;
                ledgerEntry.VouchersTypeID = context.VoucherTypeDB.Where(x => x.Name.ToLower() == "other").First().ID;
                ledgerEntry.VouchersNo = id;
                ledgerEntry.Particulars = invoiceNumber;
                ledgerEntry.ObjectID = (long)irID;
                ledgerEntry.ObjectType = "IR";
                context.LadgerEntryDB.Add(ledgerEntry);

                oppositeLedgerEntry = new LadgerEntry();
                oppositeLedgerEntry.LagerID = ledgerEntry.AffectedLadgerID;
                oppositeLedgerEntry.Credit = ledgerEntry.Debit;
                oppositeLedgerEntry.Active = true;
                oppositeLedgerEntry.CreatedBy = userId;
                oppositeLedgerEntry.CreatedDate = DateTime.Now;
                oppositeLedgerEntry.UpdatedBy = userId;
                oppositeLedgerEntry.UpdatedDate = DateTime.Now;
                oppositeLedgerEntry.Date = date;
                oppositeLedgerEntry.AffectedLadgerID = ledgerEntry.LagerID;
                oppositeLedgerEntry.VouchersTypeID = context.VoucherTypeDB.Where(x => x.Name.ToLower() == "other").First().ID;
                oppositeLedgerEntry.VouchersNo = ledgerEntry.VouchersNo;
                oppositeLedgerEntry.Particulars = invoiceNumber;
                oppositeLedgerEntry.ObjectID = (long)irID;
                oppositeLedgerEntry.ObjectType = "IR";
                context.LadgerEntryDB.Add(oppositeLedgerEntry);
            }
        }
        private static void AddExpenseAmount(AuthContext context, long id, string invoiceNumber, int irID, int userId, DateTime date, long? ladgerID, IRMaster IRR)
        {
            LadgerEntry ledgerEntry = new LadgerEntry();
            LadgerEntry oppositeLedgerEntry = new LadgerEntry();

            var expenseLedgerQuery = (from l in context.LadgerDB
                                      join lt in context.LadgerTypeDB
                                      on l.LadgertypeID equals lt.ID
                                      where lt.code.ToLower() == "expense"
                                      select l.ID).FirstOrDefault();


            if (expenseLedgerQuery == 0) // If other party entry not exist in Ledger
            {
                var LT = (from a in context.LadgerTypeDB where a.code.ToLower() == "expense" select a.ID).First();
                if (LT != 0)
                {
                    Ladger LD = new Ladger();
                    LD.Name = "expense";
                    LD.Alias = "expense";
                    LD.InventoryValuesAreAffected = false;
                    LD.Address = null;
                    LD.Country = null;
                    LD.PinCode = null;
                    LD.ProvidedBankDetails = false;
                    LD.ObjectID = 0;
                    LD.ObjectType = "expense";
                    LD.LadgertypeID = LT;
                    LD.Active = true;
                    LD.CreatedBy = 1;
                    LD.CreatedDate = DateTime.Now;
                    LD.UpdatedBy = 1;
                    LD.UpdatedDate = DateTime.Now;
                    context.LadgerDB.Add(LD);
                    context.Commit();
                    // Again call
                    expenseLedgerQuery = (from l in context.LadgerDB
                                          join lt in context.LadgerTypeDB
                                          on l.LadgertypeID equals lt.ID
                                          where lt.code.ToLower() == "expense"
                                          select l.ID).FirstOrDefault();
                }
            }



            if (IRR.ExpenseAmount > 0 || IRR.ExpenseAmountType == "ADD")
            {

                ledgerEntry = new LadgerEntry();
                ledgerEntry.LagerID = ladgerID;
                ledgerEntry.Credit = IRR.ExpenseAmount;
                ledgerEntry.Active = true;
                ledgerEntry.CreatedBy = userId;
                ledgerEntry.CreatedDate = DateTime.Now;
                ledgerEntry.UpdatedBy = userId;
                ledgerEntry.UpdatedDate = DateTime.Now;
                ledgerEntry.Date = date;
                ledgerEntry.AffectedLadgerID = expenseLedgerQuery;
                ledgerEntry.VouchersTypeID = context.VoucherTypeDB.Where(x => x.Name.ToLower() == "expense").First().ID;
                ledgerEntry.VouchersNo = id;
                ledgerEntry.Particulars = invoiceNumber;
                ledgerEntry.ObjectID = (long)irID;
                ledgerEntry.ObjectType = "IR";
                context.LadgerEntryDB.Add(ledgerEntry);

                oppositeLedgerEntry = new LadgerEntry();
                oppositeLedgerEntry.LagerID = ledgerEntry.AffectedLadgerID;
                oppositeLedgerEntry.Debit = ledgerEntry.Credit;
                oppositeLedgerEntry.Active = true;
                oppositeLedgerEntry.CreatedBy = userId;
                oppositeLedgerEntry.CreatedDate = DateTime.Now;
                oppositeLedgerEntry.UpdatedBy = userId;
                oppositeLedgerEntry.UpdatedDate = DateTime.Now;
                oppositeLedgerEntry.Date = date;
                oppositeLedgerEntry.AffectedLadgerID = ledgerEntry.LagerID;
                oppositeLedgerEntry.VouchersTypeID = context.VoucherTypeDB.Where(x => x.Name.ToLower() == "expense").First().ID;
                oppositeLedgerEntry.VouchersNo = ledgerEntry.VouchersNo;
                oppositeLedgerEntry.Particulars = invoiceNumber;
                oppositeLedgerEntry.ObjectID = (long)irID;
                oppositeLedgerEntry.ObjectType = "IR";
                context.LadgerEntryDB.Add(oppositeLedgerEntry);
            }
            else if (IRR.ExpenseAmount < 0 || IRR.ExpenseAmountType == "MINUS")
            {

                ledgerEntry = new LadgerEntry();
                ledgerEntry.LagerID = ladgerID;
                ledgerEntry.Debit = IRR.ExpenseAmount * (-1);
                ledgerEntry.Active = true;
                ledgerEntry.CreatedBy = userId;
                ledgerEntry.CreatedDate = DateTime.Now;
                ledgerEntry.UpdatedBy = userId;
                ledgerEntry.UpdatedDate = DateTime.Now;
                ledgerEntry.Date = date;
                ledgerEntry.AffectedLadgerID = expenseLedgerQuery;
                ledgerEntry.VouchersTypeID = context.VoucherTypeDB.Where(x => x.Name.ToLower() == "expense").First().ID;
                ledgerEntry.VouchersNo = id;
                ledgerEntry.Particulars = invoiceNumber;
                ledgerEntry.ObjectID = (long)irID;
                ledgerEntry.ObjectType = "IR";
                context.LadgerEntryDB.Add(ledgerEntry);

                oppositeLedgerEntry = new LadgerEntry();
                oppositeLedgerEntry.LagerID = ledgerEntry.AffectedLadgerID;
                oppositeLedgerEntry.Credit = ledgerEntry.Debit;
                oppositeLedgerEntry.Active = true;
                oppositeLedgerEntry.CreatedBy = userId;
                oppositeLedgerEntry.CreatedDate = DateTime.Now;
                oppositeLedgerEntry.UpdatedBy = userId;
                oppositeLedgerEntry.UpdatedDate = DateTime.Now;
                oppositeLedgerEntry.Date = date;
                oppositeLedgerEntry.AffectedLadgerID = ledgerEntry.LagerID;
                oppositeLedgerEntry.VouchersTypeID = context.VoucherTypeDB.Where(x => x.Name.ToLower() == "expense").First().ID;
                oppositeLedgerEntry.VouchersNo = ledgerEntry.VouchersNo;
                oppositeLedgerEntry.Particulars = invoiceNumber;
                oppositeLedgerEntry.ObjectID = (long)irID;
                oppositeLedgerEntry.ObjectType = "IR";
                context.LadgerEntryDB.Add(oppositeLedgerEntry);
            }
        }
        private static void AddRoundOffAmount(AuthContext context, long id, string invoiceNumber, int irID, int userId, DateTime date, long? ladgerID, IRMaster IRR)
        {
            LadgerEntry ledgerEntry = new LadgerEntry();
            LadgerEntry oppositeLedgerEntry = new LadgerEntry();

            var roundoffLedgerQuery = (from l in context.LadgerDB
                                       join lt in context.LadgerTypeDB
                                       on l.LadgertypeID equals lt.ID
                                       where lt.code.ToLower() == "roundoff"
                                       select l.ID).FirstOrDefault();

            if (roundoffLedgerQuery == 0) // If other party entry not exist in Ledger
            {
                var LT = (from a in context.LadgerTypeDB where a.code.ToLower() == "roundoff" select a.ID).First();
                if (LT != 0)
                {
                    Ladger LD = new Ladger();
                    LD.Name = "roundoff";
                    LD.Alias = "roundoff";
                    LD.InventoryValuesAreAffected = false;
                    LD.Address = null;
                    LD.Country = null;
                    LD.PinCode = null;
                    LD.ProvidedBankDetails = false;
                    LD.ObjectID = 0;
                    LD.ObjectType = "roundoff";
                    LD.LadgertypeID = LT;
                    LD.Active = true;
                    LD.CreatedBy = 1;
                    LD.CreatedDate = DateTime.Now;
                    LD.UpdatedBy = 1;
                    LD.UpdatedDate = DateTime.Now;
                    context.LadgerDB.Add(LD);
                    context.Commit();
                    // Again call
                    roundoffLedgerQuery = (from l in context.LadgerDB
                                           join lt in context.LadgerTypeDB
                                           on l.LadgertypeID equals lt.ID
                                           where lt.code.ToLower() == "roundoff"
                                           select l.ID).FirstOrDefault();
                }
            }


            if (IRR.RoundofAmount > 0 || IRR.RoundoffAmountType == "ADD")
            {


                ledgerEntry = new LadgerEntry();
                ledgerEntry.LagerID = ladgerID;
                ledgerEntry.Credit = IRR.RoundofAmount;
                ledgerEntry.Active = true;
                ledgerEntry.CreatedBy = userId;
                ledgerEntry.CreatedDate = DateTime.Now;
                ledgerEntry.UpdatedBy = userId;
                ledgerEntry.UpdatedDate = DateTime.Now;
                ledgerEntry.Date = date;
                ledgerEntry.AffectedLadgerID = roundoffLedgerQuery;
                ledgerEntry.VouchersTypeID = context.VoucherTypeDB.Where(x => x.Name.ToLower() == "roundoff").First().ID;
                ledgerEntry.VouchersNo = id;
                ledgerEntry.Particulars = invoiceNumber;
                ledgerEntry.ObjectID = (long)irID;
                ledgerEntry.ObjectType = "IR";
                context.LadgerEntryDB.Add(ledgerEntry);

                oppositeLedgerEntry = new LadgerEntry();
                oppositeLedgerEntry.LagerID = ledgerEntry.AffectedLadgerID;
                oppositeLedgerEntry.Debit = ledgerEntry.Credit;
                oppositeLedgerEntry.Active = true;
                oppositeLedgerEntry.CreatedBy = userId;
                oppositeLedgerEntry.CreatedDate = DateTime.Now;
                oppositeLedgerEntry.UpdatedBy = userId;
                oppositeLedgerEntry.UpdatedDate = DateTime.Now;
                oppositeLedgerEntry.Date = date;
                oppositeLedgerEntry.AffectedLadgerID = ledgerEntry.LagerID;
                oppositeLedgerEntry.VouchersTypeID = context.VoucherTypeDB.Where(x => x.Name.ToLower() == "roundoff").First().ID;
                oppositeLedgerEntry.VouchersNo = ledgerEntry.VouchersNo;
                oppositeLedgerEntry.Particulars = invoiceNumber;
                oppositeLedgerEntry.ObjectID = (long)irID;
                oppositeLedgerEntry.ObjectType = "IR";
                context.LadgerEntryDB.Add(oppositeLedgerEntry);
            }
            else if (IRR.RoundofAmount < 0 || IRR.RoundoffAmountType == "MINUS")
            {

                ledgerEntry = new LadgerEntry();
                ledgerEntry.LagerID = ladgerID;
                ledgerEntry.Debit = IRR.RoundofAmount * (-1);
                ledgerEntry.Active = true;
                ledgerEntry.CreatedBy = userId;
                ledgerEntry.CreatedDate = DateTime.Now;
                ledgerEntry.UpdatedBy = userId;
                ledgerEntry.UpdatedDate = DateTime.Now;
                ledgerEntry.Date = date;
                ledgerEntry.AffectedLadgerID = roundoffLedgerQuery;
                ledgerEntry.VouchersTypeID = context.VoucherTypeDB.Where(x => x.Name.ToLower() == "roundoff").First().ID;
                ledgerEntry.VouchersNo = id;
                ledgerEntry.Particulars = invoiceNumber;
                ledgerEntry.ObjectID = (long)irID;
                ledgerEntry.ObjectType = "IR";
                context.LadgerEntryDB.Add(ledgerEntry);

                oppositeLedgerEntry = new LadgerEntry();
                oppositeLedgerEntry.LagerID = ledgerEntry.AffectedLadgerID;
                oppositeLedgerEntry.Credit = ledgerEntry.Debit;
                oppositeLedgerEntry.Active = true;
                oppositeLedgerEntry.CreatedBy = userId;
                oppositeLedgerEntry.CreatedDate = DateTime.Now;
                oppositeLedgerEntry.UpdatedBy = userId;
                oppositeLedgerEntry.UpdatedDate = DateTime.Now;
                oppositeLedgerEntry.Date = date;
                oppositeLedgerEntry.AffectedLadgerID = ledgerEntry.LagerID;
                oppositeLedgerEntry.VouchersTypeID = context.VoucherTypeDB.Where(x => x.Name.ToLower() == "roundoff").First().ID;
                oppositeLedgerEntry.VouchersNo = ledgerEntry.VouchersNo;
                oppositeLedgerEntry.Particulars = invoiceNumber;
                oppositeLedgerEntry.ObjectID = (long)irID;
                oppositeLedgerEntry.ObjectType = "IR";
                context.LadgerEntryDB.Add(oppositeLedgerEntry);
            }
        }


        public static void DebitLedgerEntryIRPayment(int supplierID, AuthContext context, double amount, string invoiceNumber, int irID, int userId, double? discount, string objectType = "IR", string RefNo = "", string Reamark = "", string guid = "", long? bankLedgerId = 0, DateTime? date = null, int? IRPaymentId = 0)
        {
            try
            {

                if (!date.HasValue)
                {
                    date = DateTime.Now;
                }

                LadgerHelper ladgerHelper = new LadgerHelper();
                //var ledgerIDQuery = from l in context.LadgerDB
                //                    join lt in context.LadgerTypeDB
                //                    on l.LadgertypeID equals lt.ID
                //                    where lt.code.ToLower() == "supplier"
                //                        && l.ObjectID == supplierID
                //                    select l.ID;
                Ladger ledger = ladgerHelper.GetOrCreateLadgerTypeAndLadger("Supplier", supplierID, userId, context);
                long? ladgerID = ledger.ID;



                var purchaseDiscountLedgerQuery = from l in context.LadgerDB
                                                  join lt in context.LadgerTypeDB
                                                  on l.LadgertypeID equals lt.ID
                                                  where lt.code.ToLower() == "purchasediscount"
                                                  select l.ID;

                var voucherNumberQuery = from le in context.LadgerEntryDB
                                         where le.LagerID == ladgerID && le.ObjectID == irID
                                         select le.VouchersNo;

                var voucherNumber = voucherNumberQuery.FirstOrDefault();

                LadgerEntry ledgerEntry = new LadgerEntry();
                ledgerEntry.LagerID = ladgerID;
                ledgerEntry.Debit = amount;
                ledgerEntry.Active = true;
                ledgerEntry.CreatedBy = userId;
                ledgerEntry.CreatedDate = DateTime.Now;
                ledgerEntry.UpdatedBy = userId;
                ledgerEntry.UpdatedDate = DateTime.Now;
                ledgerEntry.Date = date;
                ledgerEntry.AffectedLadgerID = bankLedgerId;
                ledgerEntry.VouchersTypeID = context.VoucherTypeDB.Where(x => x.Name.ToLower() == "payment").First().ID;
                ledgerEntry.VouchersNo = voucherNumber;
                ledgerEntry.Particulars = invoiceNumber;
                ledgerEntry.ObjectID = (long)irID;
                ledgerEntry.ObjectType = objectType;
                ledgerEntry.UploadGUID = guid;
                ledgerEntry.RefNo = RefNo;
                ledgerEntry.Remark = Reamark;
                ledgerEntry.IrPaymentDetailsId = IRPaymentId;
                context.LadgerEntryDB.Add(ledgerEntry);

                LadgerEntry oppositeLedgerEntry = new LadgerEntry();
                oppositeLedgerEntry.LagerID = ledgerEntry.AffectedLadgerID;
                oppositeLedgerEntry.Credit = amount;
                oppositeLedgerEntry.Active = true;
                oppositeLedgerEntry.CreatedBy = userId;
                oppositeLedgerEntry.CreatedDate = DateTime.Now;
                oppositeLedgerEntry.UpdatedBy = userId;
                oppositeLedgerEntry.UpdatedDate = DateTime.Now;
                oppositeLedgerEntry.Date = date;
                oppositeLedgerEntry.AffectedLadgerID = ledgerEntry.LagerID;
                oppositeLedgerEntry.VouchersTypeID = ledgerEntry.VouchersTypeID;
                oppositeLedgerEntry.VouchersNo = voucherNumber;
                oppositeLedgerEntry.Particulars = invoiceNumber;
                oppositeLedgerEntry.ObjectID = (long)irID;
                oppositeLedgerEntry.ObjectType = objectType;
                oppositeLedgerEntry.UploadGUID = guid;
                oppositeLedgerEntry.RefNo = RefNo;
                oppositeLedgerEntry.Remark = Reamark;
                oppositeLedgerEntry.IrPaymentDetailsId = IRPaymentId;
                context.LadgerEntryDB.Add(oppositeLedgerEntry);

                //if (discount != null && discount.Value > 0)
                //{
                //    ledgerEntry = new LadgerEntry();
                //    ledgerEntry.LagerID = ladgerID;
                //    ledgerEntry.Debit = discount;
                //    ledgerEntry.Active = true;
                //    ledgerEntry.CreatedBy = userId;
                //    ledgerEntry.CreatedDate = DateTime.Now;
                //    ledgerEntry.UpdatedBy = userId;
                //    ledgerEntry.UpdatedDate = DateTime.Now;
                //    ledgerEntry.Date = date;
                //    ledgerEntry.AffectedLadgerID = purchaseDiscountLedgerQuery.First();
                //    ledgerEntry.VouchersTypeID = context.VoucherTypeDB.Where(x => x.Name.ToLower() == "journal").First().ID;
                //    ledgerEntry.VouchersNo = voucherNumber;
                //    ledgerEntry.Particulars = invoiceNumber;
                //    ledgerEntry.ObjectID = (long)irID;
                //    ledgerEntry.ObjectType = "IR";
                //    ledgerEntry.UploadGUID = guid;
                //    ledgerEntry.RefNo = RefNo;
                //    ledgerEntry.Remark = Reamark;
                //    context.LadgerEntryDB.Add(ledgerEntry);

                //    oppositeLedgerEntry = new LadgerEntry();
                //    oppositeLedgerEntry.LagerID = ledgerEntry.AffectedLadgerID;
                //    oppositeLedgerEntry.Credit = discount;
                //    oppositeLedgerEntry.Active = true;
                //    oppositeLedgerEntry.CreatedBy = userId;
                //    oppositeLedgerEntry.CreatedDate = DateTime.Now;
                //    oppositeLedgerEntry.UpdatedBy = userId;
                //    oppositeLedgerEntry.UpdatedDate = DateTime.Now;
                //    oppositeLedgerEntry.Date = date;
                //    oppositeLedgerEntry.AffectedLadgerID = ledgerEntry.LagerID;
                //    oppositeLedgerEntry.VouchersTypeID = ledgerEntry.VouchersTypeID;
                //    oppositeLedgerEntry.VouchersNo = voucherNumber;
                //    oppositeLedgerEntry.Particulars = invoiceNumber;
                //    oppositeLedgerEntry.ObjectID = (long)irID;
                //    oppositeLedgerEntry.ObjectType = "IR";
                //    oppositeLedgerEntry.UploadGUID = guid;
                //    oppositeLedgerEntry.RefNo = RefNo;
                //    oppositeLedgerEntry.Remark = Reamark;
                //    context.LadgerEntryDB.Add(oppositeLedgerEntry);
                //}


                #region tds entry

                var tdsLedgerID = context.LadgerDB.FirstOrDefault(x => x.Name == "TDS").ID;

                var tdsAmount = context.IRPaymentDetailsDB.Where(x => x.Id == IRPaymentId).FirstOrDefault()?.TDSAmount;

                tdsAmount = tdsAmount == null ? 0.0 : tdsAmount;

                if (tdsAmount > 0)
                {
                    LadgerEntry ledgerEntrytds = new LadgerEntry();
                    ledgerEntrytds.LagerID = ladgerID;
                    ledgerEntrytds.Debit = tdsAmount;
                    ledgerEntrytds.Active = true;
                    ledgerEntrytds.CreatedBy = userId;
                    ledgerEntrytds.CreatedDate = DateTime.Now;
                    ledgerEntrytds.UpdatedBy = userId;
                    ledgerEntrytds.UpdatedDate = DateTime.Now;
                    ledgerEntrytds.Date = date;
                    ledgerEntrytds.AffectedLadgerID = tdsLedgerID;
                    ledgerEntrytds.VouchersTypeID = context.VoucherTypeDB.Where(x => x.Name.ToLower() == "Tax").First().ID;
                    ledgerEntrytds.VouchersNo = voucherNumber;
                    ledgerEntrytds.Particulars = invoiceNumber;
                    ledgerEntrytds.ObjectID = (long)irID;
                    ledgerEntrytds.ObjectType = objectType;
                    ledgerEntrytds.UploadGUID = guid;
                    ledgerEntrytds.RefNo = RefNo;
                    ledgerEntrytds.Remark = Reamark;
                    ledgerEntrytds.IrPaymentDetailsId = IRPaymentId;
                    context.LadgerEntryDB.Add(ledgerEntrytds);

                    LadgerEntry oppositeLedgerEntrytds = new LadgerEntry();
                    oppositeLedgerEntrytds.LagerID = ledgerEntrytds.AffectedLadgerID;
                    oppositeLedgerEntrytds.Credit = tdsAmount;
                    oppositeLedgerEntrytds.Active = true;
                    oppositeLedgerEntrytds.CreatedBy = userId;
                    oppositeLedgerEntrytds.CreatedDate = DateTime.Now;
                    oppositeLedgerEntrytds.UpdatedBy = userId;
                    oppositeLedgerEntrytds.UpdatedDate = DateTime.Now;
                    oppositeLedgerEntrytds.Date = date;
                    oppositeLedgerEntrytds.AffectedLadgerID = ledgerEntrytds.LagerID;
                    oppositeLedgerEntrytds.VouchersTypeID = ledgerEntrytds.VouchersTypeID;
                    oppositeLedgerEntrytds.VouchersNo = voucherNumber;
                    oppositeLedgerEntrytds.Particulars = invoiceNumber;
                    oppositeLedgerEntrytds.ObjectID = (long)irID;
                    oppositeLedgerEntrytds.ObjectType = objectType;
                    oppositeLedgerEntrytds.UploadGUID = guid;
                    oppositeLedgerEntrytds.RefNo = RefNo;
                    oppositeLedgerEntrytds.Remark = Reamark;
                    oppositeLedgerEntrytds.IrPaymentDetailsId = IRPaymentId;
                    context.LadgerEntryDB.Add(oppositeLedgerEntrytds);
                }
                #endregion

                context.Commit();
            }
            catch (Exception ex)
            {
                Logger logger = LogManager.GetCurrentClassLogger();
                logger.Error("Debit Supplier Ledger Issue: \n" + ex.Message);
            }
        }

        public void UpdateLedgerEntryIRPayment(List<LadgerEntry> ladgerEntryEntryList, AuthContext context, int supplierID, long irid, string invoiceNumber, string Reamark, double amount, int userid, IRMasterDTO details, int? IRPaymentId = 0)
        {
            LadgerHelper ladgerHelper = new LadgerHelper();
            //var ledgerIDQuery = from l in context.LadgerDB
            //                    join lt in context.LadgerTypeDB
            //                    on l.LadgertypeID equals lt.ID
            //                    where lt.code.ToLower() == "supplier"
            //                        && l.ObjectID == supplierID
            //                    select l.ID;
            Ladger ledger = ladgerHelper.GetOrCreateLadgerTypeAndLadger("Supplier", supplierID, userid, context);
            long? supplierLedgerID = ledger.ID;


            var voucherNumberQuery = from v in context.VoucherDB
                                     join le in context.LadgerEntryDB
                                     on v.ID equals le.VouchersNo
                                     where le.LagerID == supplierLedgerID && le.ObjectID == irid
                                     select le.VouchersNo;




            if (ladgerEntryEntryList != null && ladgerEntryEntryList.Count > 0)
            {
                foreach (var item in ladgerEntryEntryList)
                {

                    var voucherNumber = voucherNumberQuery.FirstOrDefault();


                    item.Debit = item.Debit.HasValue && item.Debit > 0 ? amount : item.Debit;
                    item.Credit = item.Credit.HasValue && item.Credit > 0 ? amount : item.Credit;
                    item.UpdatedBy = userid;
                    item.UpdatedDate = DateTime.Now;
                    item.VouchersNo = voucherNumber;
                    item.Particulars = invoiceNumber;
                    item.ObjectID = (long)irid;
                    item.ObjectType = "IR";
                    item.Remark = Reamark;
                    item.IrPaymentDetailsId = IRPaymentId;
                }
                context.Commit();

                IRMaster irdata = UpdateIRMaster(details, context);

                //context.Entry(ladgerEntryEntryList).State = EntityState.Modified;

            }

        }


        public void DebitLedgerEntrySupplieradvacepay(int supplierID, AuthContext context, double amount, int userId, string RefNo = "", string Reamark = "", string guid = "", long? bankLedgerId = 0, bool IsSupplierAdvancepay = false, int? IRPaymentId = 0, DateTime? date = null)
        {
            try
            {
                if (!date.HasValue)
                {
                    date = DateTime.Now;
                }
                LadgerHelper ladgerHelper = new LadgerHelper();
                //var ledgerIDQuery = from l in context.LadgerDB
                //                    join lt in context.LadgerTypeDB
                //                    on l.LadgertypeID equals lt.ID
                //                    where lt.code.ToLower() == "supplier"
                //                        && l.ObjectID == supplierID
                //                    select l.ID;
                Ladger ledger = ladgerHelper.GetOrCreateLadgerTypeAndLadger("Supplier", supplierID, userId, context);
                long? ladgerID = ledger.ID;

                LadgerEntry ledgerEntry = new LadgerEntry();
                ledgerEntry.LagerID = ladgerID;
                ledgerEntry.Debit = amount;
                ledgerEntry.Active = true;
                ledgerEntry.CreatedBy = userId;
                ledgerEntry.CreatedDate = DateTime.Now;
                ledgerEntry.UpdatedBy = userId;
                ledgerEntry.UpdatedDate = DateTime.Now;
                ledgerEntry.Date = date;
                ledgerEntry.AffectedLadgerID = bankLedgerId;
                ledgerEntry.VouchersTypeID = context.VoucherTypeDB.Where(x => x.Name.ToLower() == "payment").First().ID;
                ledgerEntry.VouchersNo = 0;
                ledgerEntry.Particulars = "";
                ledgerEntry.ObjectID = 0;
                ledgerEntry.ObjectType = "";
                ledgerEntry.UploadGUID = guid;
                ledgerEntry.RefNo = RefNo;
                ledgerEntry.Remark = Reamark;
                ledgerEntry.IrPaymentDetailsId = IRPaymentId;
                ledgerEntry.IsSupplierAdvancepay = IsSupplierAdvancepay;
                context.LadgerEntryDB.Add(ledgerEntry);

                LadgerEntry oppositeLedgerEntry = new LadgerEntry();
                oppositeLedgerEntry.LagerID = ledgerEntry.AffectedLadgerID;
                oppositeLedgerEntry.Credit = amount;
                oppositeLedgerEntry.Active = true;
                oppositeLedgerEntry.CreatedBy = userId;
                oppositeLedgerEntry.CreatedDate = DateTime.Now;
                oppositeLedgerEntry.UpdatedBy = userId;
                oppositeLedgerEntry.UpdatedDate = DateTime.Now;
                oppositeLedgerEntry.Date = date;
                oppositeLedgerEntry.AffectedLadgerID = ledgerEntry.LagerID;
                oppositeLedgerEntry.VouchersTypeID = ledgerEntry.VouchersTypeID;
                oppositeLedgerEntry.VouchersNo = 0;
                oppositeLedgerEntry.Particulars = "";
                oppositeLedgerEntry.ObjectID = 0;
                oppositeLedgerEntry.ObjectType = "";
                oppositeLedgerEntry.UploadGUID = guid;
                oppositeLedgerEntry.RefNo = RefNo;
                oppositeLedgerEntry.Remark = Reamark;
                oppositeLedgerEntry.IrPaymentDetailsId = IRPaymentId;
                oppositeLedgerEntry.IsSupplierAdvancepay = IsSupplierAdvancepay;
                context.LadgerEntryDB.Add(oppositeLedgerEntry);

                context.Commit();

            }
            catch (Exception ex)
            {
                Logger logger = LogManager.GetCurrentClassLogger();
                logger.Error("Debit Supplier Ledger Issue: \n" + ex.Message);
            }
        }


        public IRPaymentDetails SupplierNewPayment(AuthContext authContext, IRPaymentDetails details, Guid id, int userid)
        {
            details.Guid = Convert.ToString(id);
            // insert IRpayment details 
            IRPaymentDetails irdetails = new IRPaymentDetails();
            irdetails.BankId = details.BankId;
            irdetails.BankName = details.BankName;
            irdetails.SupplierId = details.SupplierId;
            irdetails.CreatedDate = DateTime.Now;
            irdetails.Createby = userid;
            irdetails.RefNo = details.RefNo;
            irdetails.TotalAmount = details.TotalAmount;
            irdetails.TotalReaminingAmount = details.TotalReaminingAmount;
            irdetails.Remark = details.Remark;
            irdetails.Guid = details.Guid;
            irdetails.Deleted = false;
            irdetails.IsActive = true;
            irdetails.PaymentDate = details.PaymentDate;
            irdetails.IRList = details.IRList;
            irdetails.IRPaymentSummaryId = details.IRPaymentSummaryId;
            irdetails.IsIROutstandingPending = false;
            authContext.IRPaymentDetailsDB.Add(irdetails);

            //Minus Full Supplier Payment Data
            FullSupplierPaymentData fullSupplierPaymentData = authContext.FullSupplierPaymentDataDB.Where(x => x.SupplierId == details.SupplierId).FirstOrDefault();
            if (fullSupplierPaymentData != null)
            {
                fullSupplierPaymentData.InVoiceRemainingAmount = fullSupplierPaymentData.InVoiceRemainingAmount - details.TotalAmount;
                if (fullSupplierPaymentData.InVoiceRemainingAmount == 0)
                {
                    fullSupplierPaymentData.SupplierPaymentStatus = "Full Paid";
                }
                else
                {
                    fullSupplierPaymentData.SupplierPaymentStatus = "Partial Paid";
                }
                authContext.Entry(fullSupplierPaymentData).State = EntityState.Modified;
            }


            authContext.Commit();
            return irdetails;

        }

        public IRPaymentDetails SupplierNewPaymentRequest(AuthContext authContext, IRPaymentDetails details, Guid id, int userid)
        {
            details.Guid = Convert.ToString(id);
            // insert IRpayment details 
            IRPaymentDetails irdetails = new IRPaymentDetails
            {
                BankId = details.BankId,
                BankName = details.BankName,
                SupplierId = details.SupplierId,
                CreatedDate = DateTime.Now,
                Createby = userid,
                RefNo = details.RefNo,
                TotalAmount = details.TotalAmount,
                TotalReaminingAmount = details.TotalReaminingAmount,
                Remark = details.Remark,
                Guid = details.Guid,
                Deleted = false,
                IsActive = true,
                PaymentDate = details.PaymentDate,
                IRList = details.IRList,
                IRPaymentSummaryId = details.IRPaymentSummaryId,
                PaymentStatus = details.PaymentStatus,
                IsIROutstandingPending = details.IsIROutstandingPending,
                WarehouseId = details.WarehouseId,
                IRMasterId = details.IRMasterId,
                TDSAmount = details.TDSAmount
            };
            authContext.IRPaymentDetailsDB.Add(irdetails);
            authContext.Commit();
            return irdetails;

        }

        public void MakeIrLedgerEntry(List<IRMasterDTO> irDetails, AuthContext authContext, int userid, IRPaymentDetails details, int irPaymrntDetailID)
        {
            if (irDetails != null)
            {
                foreach (var data in irDetails)
                {
                    // insert  Supplier payment data

                    SupplierPaymentData paydata = new SupplierPaymentData();
                    paydata.PurchaseOrderId = data.PurchaseOrderId;
                    paydata.InVoiceNumber = data.IRID;
                    paydata.CreditInVoiceAmount = data.TotalAmount;
                    paydata.PaymentStatusCorD = "Credit";
                    paydata.VoucherType = "Purchase";
                    paydata.ClosingBalance = data.TotalAmount;
                    paydata.CompanyId = 1;
                    paydata.WarehouseId = data.WarehouseId;
                    paydata.InVoiceDate = DateTime.Now;
                    paydata.SupplierId = data.supplierId;
                    paydata.SupplierName = data.SupplierName;
                    paydata.CreatedDate = DateTime.Now;
                    paydata.UpdatedDate = DateTime.Now;
                    authContext.SupplierPaymentDataDB.Add(paydata);
                    //end 


                    // Edit status in IR Status ,  Payment status and add paid amount in IR Master 
                    IRMaster irdata = UpdateIRMaster(data, authContext);
                    //end
                    double amount = (irdata.TotalAmountRemaining - data.ReamainingAmt) ?? 0;
                    //insert ladger entry 
                    IRHelper.DebitLedgerEntryIRPayment(irdata.supplierId, authContext, data.TotalAmountRemaining, irdata.IRID, irdata.Id, userid, irdata.Discount, "IR", details.RefNo, details.Remark, details.Guid, details.BankId, details.PaymentDate, irPaymrntDetailID);
                    //end

                    string status = "";
                    if (data.PaymentStatus == irPaidStatus)
                    {
                        status = full;
                    }
                    else
                    {
                        status = partial;

                    }
                    MakeIRPaymentHistory(data.TotalAmountRemaining, data.Id, irPaymrntDetailID, data.PurchaseOrderId, userid, status, authContext);

                    authContext.Commit();

                }
            }

        }

        private IRMaster UpdateIRMaster(IRMasterDTO data, AuthContext authContext)
        {
            IRMaster irdata = authContext.IRMasterDB.Where(x => x.PurchaseOrderId == data.PurchaseOrderId && x.IRID == data.IRID && x.Deleted == false).FirstOrDefault();
            var amount = irdata.TotalAmountRemaining - data.ReamainingAmt;
            irdata.TotalAmountRemaining -= amount ?? 0;
            irdata.PaymentStatus = data.PaymentStatus;
            irdata.IRStatus = data.IRStatus;
            //irdata.TotalRemainingTDSAmount -= data.TDSAmount ?? 0;
            irdata.UpdatedDate = DateTime.Now;
            authContext.Entry(irdata).State = EntityState.Modified;
            authContext.Commit();
            return irdata;
        }

        public void MakeIRPaymentHistory(double? amount, int? irid, int irPaymentDetailId, int? purchaseOrderID, int userid, string paymentMode, AuthContext context)
        {

            IRPaymentDetailHistory irpayhistory = new IRPaymentDetailHistory();
            irpayhistory.Amount = amount;
            irpayhistory.IRID = irid;
            irpayhistory.IRPaymentDetailId = irPaymentDetailId;
            irpayhistory.PurchaseOrderId = purchaseOrderID;
            irpayhistory.Deleted = false;
            irpayhistory.IsActive = true;
            irpayhistory.Createby = userid;
            irpayhistory.CreatedDate = DateTime.Now;
            irpayhistory.PaymentMode = paymentMode;
            context.IRPaymentDetailHistoryDB.Add(irpayhistory);
            context.Commit();
        }


        private void updatetaxsupplierlagderentry(double? taxAmount, int supplierID, int userId, DateTime? date = null, AuthContext context = null, long? id = 0, string invoiceNumber = null, int irID = 0)
        {

            bool IsIgstIR = false;
            var pm = from po in context.DPurchaseOrderMaster
                     join ir in context.IRMasterDB
                     on po.PurchaseOrderId equals ir.PurchaseOrderId
                     where ir.Id == irID && ir.IRID == invoiceNumber
                     select new { po.DepoId, po.WarehouseId };
            var depodta = pm.FirstOrDefault();
            if (depodta.DepoId > 0)
            {
                DepoMaster DepoDetail = context.DepoMasters.Where(a => a.DepoId == depodta.DepoId).SingleOrDefault();

                //for igst case if true then apply condion to hide column of cgst sgst cess
                if (!string.IsNullOrEmpty(DepoDetail.GSTin) && DepoDetail.GSTin.Length >= 11)
                {
                    string DepoTin_No = DepoDetail.GSTin.Substring(0, 2);
                    IsIgstIR = !context.Warehouses.Any(x => x.GSTin != null && x.WarehouseId == depodta.WarehouseId && x.GSTin.Substring(0, 2) == DepoTin_No);
                }
            }



            LadgerHelper ladgerHelper = new LadgerHelper();
            //var ledgerIDQuery = from l in context.LadgerDB
            //                    join lt in context.LadgerTypeDB
            //                    on l.LadgertypeID equals lt.ID
            //                    where lt.code.ToLower() == "supplier"
            //                        && l.ObjectID == supplierID
            //                    select l.ID;
            Ladger ledger = ladgerHelper.GetOrCreateLadgerTypeAndLadger("Supplier", supplierID, userId, context);
            long? ladgerID = ledger.ID;

            LedgerHelper ladgerh = new LedgerHelper();
            int LadgertypeID = context.LadgerTypeDB.Where(x => x.code.ToLower() == "tax").Select(x => x.ID).FirstOrDefault();
            //cgst igst sgst
            long AffectedLadgerIDigst = 0;
            long AffectedLadgerIDcgst = 0;
            long AffectedLadgerIDsgst = 0;
            if (IsIgstIR)
            {
                AffectedLadgerIDigst = ladgerh.GetOrCreateLadger(LadgertypeID, 0, "IGST", userId, null).ID;
            }
            else
            {
                AffectedLadgerIDcgst = ladgerh.GetOrCreateLadger(LadgertypeID, 0, "CGST", userId, null).ID;
                AffectedLadgerIDsgst = ladgerh.GetOrCreateLadger(LadgertypeID, 0, "SGST", userId, null).ID;
            }

            IRHelper irdata = new IRHelper();
            irdata.addtaxamountlagderentry(IsIgstIR, taxAmount, ladgerID, date, context, id, invoiceNumber, irID, userId, AffectedLadgerIDigst, AffectedLadgerIDcgst, AffectedLadgerIDsgst);


        }

        private void addtaxamountlagderentry(bool IsIgstIR, double? taxAmount, long? ladgerID, DateTime? date = null, AuthContext context = null, long? id = 0, string invoiceNumber = null, int irID = 0, int userId = 0, long? AffectedLadgerIDigst = 0, long? AffectedLadgerIDcgst = 0, long? AffectedLadgerIDsgst = 0)
        {

            LadgerEntry ledgerEntry = new LadgerEntry();
            LadgerEntry oppositeLedgerEntry = new LadgerEntry();
            // add igst amount in ledger
            if (IsIgstIR)
            {
                if (taxAmount > 0)
                {
                    ledgerEntry = new LadgerEntry();
                    ledgerEntry.LagerID = ladgerID;
                    ledgerEntry.Credit = taxAmount;
                    ledgerEntry.Active = true;
                    ledgerEntry.CreatedBy = userId;
                    ledgerEntry.CreatedDate = DateTime.Now;
                    ledgerEntry.UpdatedBy = userId;
                    ledgerEntry.UpdatedDate = DateTime.Now;
                    ledgerEntry.Date = date;
                    ledgerEntry.AffectedLadgerID = AffectedLadgerIDigst;
                    ledgerEntry.VouchersTypeID = context.VoucherTypeDB.Where(x => x.Name.ToLower() == "tax").First().ID;
                    ledgerEntry.VouchersNo = id;
                    ledgerEntry.Particulars = invoiceNumber;
                    ledgerEntry.ObjectID = (long)irID;
                    ledgerEntry.ObjectType = "IR";
                    context.LadgerEntryDB.Add(ledgerEntry);

                    oppositeLedgerEntry = new LadgerEntry();
                    oppositeLedgerEntry.LagerID = ledgerEntry.AffectedLadgerID;
                    oppositeLedgerEntry.Debit = taxAmount;
                    oppositeLedgerEntry.Active = true;
                    oppositeLedgerEntry.CreatedBy = userId;
                    oppositeLedgerEntry.CreatedDate = DateTime.Now;
                    oppositeLedgerEntry.UpdatedBy = userId;
                    oppositeLedgerEntry.UpdatedDate = DateTime.Now;
                    oppositeLedgerEntry.Date = date;
                    oppositeLedgerEntry.AffectedLadgerID = ledgerEntry.LagerID;
                    oppositeLedgerEntry.VouchersTypeID = context.VoucherTypeDB.Where(x => x.Name.ToLower() == "tax").First().ID;
                    oppositeLedgerEntry.VouchersNo = ledgerEntry.VouchersNo;
                    oppositeLedgerEntry.Particulars = invoiceNumber;
                    oppositeLedgerEntry.ObjectID = (long)irID;
                    oppositeLedgerEntry.ObjectType = "IR";
                    context.LadgerEntryDB.Add(oppositeLedgerEntry);

                }
            } //end
            else
            {
                double sgstamount = (taxAmount / 2) ?? 0;
                if (sgstamount > 0)
                {   //add sgst tax in ledger
                    ledgerEntry = new LadgerEntry();
                    ledgerEntry.LagerID = ladgerID;
                    ledgerEntry.Credit = sgstamount;
                    ledgerEntry.Active = true;
                    ledgerEntry.CreatedBy = userId;
                    ledgerEntry.CreatedDate = DateTime.Now;
                    ledgerEntry.UpdatedBy = userId;
                    ledgerEntry.UpdatedDate = DateTime.Now;
                    ledgerEntry.Date = date;
                    ledgerEntry.AffectedLadgerID = AffectedLadgerIDsgst;
                    ledgerEntry.VouchersTypeID = context.VoucherTypeDB.Where(x => x.Name.ToLower() == "tax").First().ID;
                    ledgerEntry.VouchersNo = id;
                    ledgerEntry.Particulars = invoiceNumber;
                    ledgerEntry.ObjectID = (long)irID;
                    ledgerEntry.ObjectType = "IR";
                    context.LadgerEntryDB.Add(ledgerEntry);

                    oppositeLedgerEntry = new LadgerEntry();
                    oppositeLedgerEntry.LagerID = ledgerEntry.AffectedLadgerID;
                    oppositeLedgerEntry.Debit = sgstamount;
                    oppositeLedgerEntry.Active = true;
                    oppositeLedgerEntry.CreatedBy = userId;
                    oppositeLedgerEntry.CreatedDate = DateTime.Now;
                    oppositeLedgerEntry.UpdatedBy = userId;
                    oppositeLedgerEntry.UpdatedDate = DateTime.Now;
                    oppositeLedgerEntry.Date = date;
                    oppositeLedgerEntry.AffectedLadgerID = ledgerEntry.LagerID;
                    oppositeLedgerEntry.VouchersTypeID = context.VoucherTypeDB.Where(x => x.Name.ToLower() == "tax").First().ID;
                    oppositeLedgerEntry.VouchersNo = ledgerEntry.VouchersNo;
                    oppositeLedgerEntry.Particulars = invoiceNumber;
                    oppositeLedgerEntry.ObjectID = (long)irID;
                    oppositeLedgerEntry.ObjectType = "IR";
                    context.LadgerEntryDB.Add(oppositeLedgerEntry);
                    //end

                    //cgst add in ledger 
                    ledgerEntry = new LadgerEntry();
                    ledgerEntry.LagerID = ladgerID;
                    ledgerEntry.Credit = sgstamount;
                    ledgerEntry.Active = true;
                    ledgerEntry.CreatedBy = userId;
                    ledgerEntry.CreatedDate = DateTime.Now;
                    ledgerEntry.UpdatedBy = userId;
                    ledgerEntry.UpdatedDate = DateTime.Now;
                    ledgerEntry.Date = date;
                    ledgerEntry.AffectedLadgerID = AffectedLadgerIDcgst;
                    ledgerEntry.VouchersTypeID = context.VoucherTypeDB.Where(x => x.Name.ToLower() == "tax").First().ID;
                    ledgerEntry.VouchersNo = id;
                    ledgerEntry.Particulars = invoiceNumber;
                    ledgerEntry.ObjectID = (long)irID;
                    ledgerEntry.ObjectType = "IR";
                    context.LadgerEntryDB.Add(ledgerEntry);

                    oppositeLedgerEntry = new LadgerEntry();
                    oppositeLedgerEntry.LagerID = ledgerEntry.AffectedLadgerID;
                    oppositeLedgerEntry.Debit = sgstamount;
                    oppositeLedgerEntry.Active = true;
                    oppositeLedgerEntry.CreatedBy = userId;
                    oppositeLedgerEntry.CreatedDate = DateTime.Now;
                    oppositeLedgerEntry.UpdatedBy = userId;
                    oppositeLedgerEntry.UpdatedDate = DateTime.Now;
                    oppositeLedgerEntry.Date = date;
                    oppositeLedgerEntry.AffectedLadgerID = ledgerEntry.LagerID;
                    oppositeLedgerEntry.VouchersTypeID = context.VoucherTypeDB.Where(x => x.Name.ToLower() == "tax").First().ID;
                    oppositeLedgerEntry.VouchersNo = ledgerEntry.VouchersNo;
                    oppositeLedgerEntry.Particulars = invoiceNumber;
                    oppositeLedgerEntry.ObjectID = (long)irID;
                    oppositeLedgerEntry.ObjectType = "IR";
                    context.LadgerEntryDB.Add(oppositeLedgerEntry);
                    //end
                }
            }
        }

        public static void UpdateRoundoff()
        {
            using (var authContext = new AuthContext())
            {
                DateTime sd = Convert.ToDateTime("04/01/2019");
                DateTime ed = Convert.ToDateTime("03/07/2020");
                IRHelper iRHelper = new IRHelper();

                var query = @"select distinct 
                                        IRM.ID, 
                                  cast(II.InvoiceDate as date) InvoiceDate, 
                                  IRM.ID as IRID,
                                  irm.supplierId,  
                                  irm.Gstamt,
                                  ii.InvoiceNumber,
                                  irm.Discount,
                                  irm.TotalAmount
                                from IRMasters IRM
                                left join InvoiceImages II
                                on II.InvoiceNumber = IRM.IRID
                                and II.PurchaseOrderId = IRM.PurchaseOrderId
                                where IRM.IRStatus = 'Approved from Buyer side'
                                and II.InvoiceDate>='4/1/2019 12:00:00 AM'and II.InvoiceDate<='3/7/2020 12:00:00 AM'";


                List<SupplierLedgerViewModel> irMasterList = authContext.Database.SqlQuery<SupplierLedgerViewModel>(query).ToList();
                foreach (var IRR in irMasterList)
                {


                    var roundoff = @"Select  isnull(sum(LE.Credit),0)-isnull(sum(LE.Debit),0) from IRMasters IRM 
                                         inner join Ladgers l
                                         on IRM.supplierId=l.ObjectID
                                         inner join LadgerEntries LE
                                         on IRM.Id=LE.ObjectID and l.ID=LE.LagerID and LE.ObjectType='IR'
                                         where IRM.Id=" + IRR.IRID + " and VouchersTypeID!=6";
                    double roundoffdecimal = authContext.Database.SqlQuery<double>(roundoff).FirstOrDefault();
                    double rrr = Math.Truncate(roundoffdecimal);
                    double amount = (roundoffdecimal - rrr);
                    IRMaster IRRt = new IRMaster();
                    if (amount != 0)
                    {
                        if (amount < 0.5)
                        {
                            IRRt.RoundoffAmountType = "MINUS";
                            IRRt.RoundofAmount = amount;
                        }
                        else
                        {

                            IRRt.RoundoffAmountType = "ADD";
                            IRRt.RoundofAmount = amount;

                        }
                        var ledgerIDQuery = from l in authContext.LadgerDB
                                            join lt in authContext.LadgerTypeDB
                                            on l.LadgertypeID equals lt.ID
                                            where lt.code.ToLower() == "supplier"
                                                && l.ObjectID == IRR.supplierId
                                            select l.ID;
                        long? ladgerID = ledgerIDQuery.FirstOrDefault();

                        long vocherno = authContext.LadgerEntryDB.Where(x => x.ObjectID == IRR.IRID && x.ObjectType == "IR" && x.VouchersTypeID == 5).Select(x => x.VouchersNo).FirstOrDefault() ?? 0;
                        AddRoundOffAmount(authContext, vocherno, IRR.InvoiceNumber, IRR.IRID, 1717, IRR.InvoiceDate, ladgerID, IRRt);
                        authContext.Commit();
                    }
                }

            }


        }

        public static void BillToBillPayment(int userid)
        {
            using (var authContext = new AuthContext())
            {
                var query = @"select distinct 
                                  IRM.ID, 
                                  cast(II.InvoiceDate as date) InvoiceDate, 
                                  IRM.ID as IRID,
                                  irm.supplierId,  
                                  irm.Gstamt,
                                  ii.InvoiceNumber,
                                  irm.Discount,
                                  irm.TotalAmount
                                from IRMasters IRM
                                left join InvoiceImages II
                                on II.InvoiceNumber = IRM.IRID
                                and II.PurchaseOrderId = IRM.PurchaseOrderId
                                where IRM.IRStatus = 'Approved from Buyer side'
                                and II.InvoiceDate>='4/1/2019 12:00:00 AM'and II.InvoiceDate<='1/31/2020 12:00:00 AM'";


                List<SupplierLedgerViewModel> irMasterList = authContext.Database.SqlQuery<SupplierLedgerViewModel>(query).ToList();
                foreach (var IRR in irMasterList)
                {

                    var data = from IRPay in authContext.IRPaymentDetailsDB
                               join IRH in authContext.IRPaymentDetailHistoryDB
                               on IRPay.TotalReaminingAmount equals IRH.Amount
                               where IRPay.SupplierId == IRR.supplierId && IRH.PaymentMode == "Advance" && IRPay.Deleted == false && IRPay.IsActive == true //&& IRPay.CreatedDate.Date==IRH.CreatedDate.Date
                               select new
                               {
                                   IRPay.TotalReaminingAmount,
                                   IRPay.PaymentDate,
                                   IRPay.TotalAmount,
                                   IRH.Id,
                                   PaymentId = IRPay.Id

                               };


                    var paymentdata = data.ToList();

                    foreach (var Ir in paymentdata)
                    {
                        if (Ir.TotalReaminingAmount == IRR.TotalAmount)
                        {
                            IRPaymentDetailHistory iRPaymentDetails = authContext.IRPaymentDetailHistoryDB.Where(x => x.Id == Ir.Id).FirstOrDefault();
                            if (iRPaymentDetails != null)
                            {
                                IRMaster iRMaster = authContext.IRMasterDB.Where(x => x.Id == IRR.IRID).FirstOrDefault();
                                iRMaster.IRStatus = "Paid";
                                iRMaster.PaymentStatus = "Paid";
                                authContext.Entry(iRMaster).State = EntityState.Modified;

                                //IRPaymentDetailHistory iRPaymentDetails = authContext.IRPaymentDetailHistoryDB.Where(x => x.IRPaymentDetailId == Ir.Id).FirstOrDefault();
                                iRPaymentDetails.IRID = IRR.IRID;
                                iRPaymentDetails.PurchaseOrderId = iRMaster.PurchaseOrderId;
                                iRPaymentDetails.PaymentMode = "Full";
                                authContext.Entry(iRPaymentDetails).State = EntityState.Modified;

                                List<LadgerEntry> ladgerEntry = authContext.LadgerEntryDB.Where(x => x.IrPaymentDetailsId == Ir.PaymentId).ToList();
                                foreach (var le in ladgerEntry)
                                {
                                    le.ObjectID = iRMaster.Id;
                                    le.ObjectType = "IR";
                                    le.Particulars = IRR.InvoiceNumber;
                                    authContext.Entry(le).State = EntityState.Modified;
                                    authContext.Commit();
                                }
                            }

                        }

                    }

                    // var data = authContext.IRPaymentDetailHistoryDB.Where(x => x.SupplierId == IRR.supplierId && (x.TotalAmount >= (IRR.TotalAmount - 10) && x.TotalAmount <= (IRR.TotalAmount + 10))).ToList(); 



                }
            }

        }

        public static void UpdateLedgerEntryWithAdvancePR(int supplierID, AuthContext context, int userId, DateTime date, string invoiceNumber, int irID, PurchaseRequestPayment purchaseRequestPayment)
        {
            List<LadgerEntry> ladgerEntries = context.LadgerEntryDB.Where(x => x.ObjectID == purchaseRequestPayment.Id && x.ObjectType == "PR").ToList();
            var VouchersNo = context.VoucherDB.Where(x => x.Code == invoiceNumber).Select(x => x.ID).FirstOrDefault();
            double amount = ladgerEntries.Where(x => x.Credit != null).Select(x => x.Credit).FirstOrDefault() ?? 0;
            IRMaster iRMaster = context.IRMasterDB.Where(x => x.Id == irID).FirstOrDefault();
            List<PrPaymentTransfer> paymentTransfer = context.PrPaymentTransferDB.Where(x => x.IsDeleted == false && x.SourcePurchaseRequestPaymentId == purchaseRequestPayment.Id && x.ToPurchaseOrderId == iRMaster.PurchaseOrderId).ToList();
            double creditAmount = iRMaster.TotalAmountRemaining; //getCreditAmount(iRMaster);
            if (paymentTransfer.Count > 0)
            {
                // PrPaymentTransfersettleupdate(paymentTransfer,context,creditAmount);
            }

            if (amount >= creditAmount)
            {
                foreach (var ledger in ladgerEntries)
                {

                    ledger.ObjectID = irID;
                    ledger.ObjectType = "IR";
                    ledger.VouchersNo = VouchersNo;
                    if (ledger.Credit > 0)
                    {
                        ledger.Credit = creditAmount;
                    }
                    if (ledger.Debit > 0)
                    {
                        ledger.Debit = creditAmount;
                    }
                    ledger.UpdatedBy = userId;
                    ledger.UpdatedDate = DateTime.Now;

                }
            }
            else
            {
                foreach (var ledger in ladgerEntries)
                {

                    ledger.ObjectID = irID;
                    ledger.ObjectType = "IR";
                    ledger.VouchersNo = VouchersNo;
                    ledger.UpdatedBy = userId;
                    ledger.UpdatedDate = DateTime.Now;

                }


            }
            if (amount != creditAmount)
            {
                double diffamount = amount - creditAmount;
                if (diffamount > 0)
                {
                    UpdatePRLedgerEntry(supplierID, context, purchaseRequestPayment, userId, diffamount);
                    iRMaster.IRStatus = "Paid";
                    iRMaster.PaymentStatus = "Paid";
                    iRMaster.TotalAmountRemaining = 0;
                }
                else
                {

                    iRMaster.IRStatus = "Approved from Buyer side";
                    iRMaster.PaymentStatus = "partial paid";
                    iRMaster.TotalAmountRemaining = iRMaster.TotalAmountRemaining - amount;
                }
            }
            else
            {

                iRMaster.IRStatus = "Paid";
                iRMaster.PaymentStatus = "Paid";
                iRMaster.TotalAmountRemaining = 0;
            }
            context.Entry(iRMaster).State = EntityState.Modified;
            foreach (var le in ladgerEntries)
            {
                context.Entry(le).State = EntityState.Modified;
            }
            context.Commit();

        }

        public static void UpdatePRLedgerEntry(int supplierID, AuthContext context, PurchaseRequestPayment purchaseRequestPayment, int userid, double amount)
        {
            long supplierLedgerId = context.LadgerDB.FirstOrDefault(x => x.ObjectID == supplierID && x.ObjectType == "Supplier").ID;
            LadgerEntry debitEntry = new LadgerEntry
            {
                Active = true,
                AffectedLadgerID = purchaseRequestPayment.BankId,
                CreatedBy = userid,
                CreatedDate = DateTime.Now,
                Date = purchaseRequestPayment.PaymentDate,
                UpdatedDate = DateTime.Now,
                Credit = null,
                Debit = amount,
                ObjectID = Convert.ToInt32(purchaseRequestPayment.Id),
                ObjectType = "PR",
                IsSupplierAdvancepay = true,
                Particulars = purchaseRequestPayment.PoInvoiceNo,
                RefNo = purchaseRequestPayment.RefNo,
                Remark = purchaseRequestPayment.Remark,
                VouchersTypeID = context.VoucherTypeDB.Where(x => x.Name.ToLower() == "payment").First().ID,
                VouchersNo = null,
                LagerID = supplierLedgerId,
                UploadGUID = purchaseRequestPayment.Guid,
                UpdatedBy = userid,
                PRPaymentId = purchaseRequestPayment.Id
            };
            context.LadgerEntryDB.Add(debitEntry);

            LadgerEntry creditEntry = new LadgerEntry
            {
                Active = true,
                AffectedLadgerID = supplierLedgerId,
                CreatedBy = userid,
                CreatedDate = DateTime.Now,
                Date = purchaseRequestPayment.PaymentDate,
                UpdatedDate = DateTime.Now,
                Credit = amount,
                Debit = null,
                ObjectID = Convert.ToInt32(purchaseRequestPayment.Id),
                ObjectType = "PR",
                IsSupplierAdvancepay = true,
                Particulars = purchaseRequestPayment.PoInvoiceNo,
                RefNo = purchaseRequestPayment.RefNo,
                Remark = purchaseRequestPayment.Remark,
                VouchersTypeID = debitEntry.VouchersTypeID,
                VouchersNo = null,
                LagerID = purchaseRequestPayment.BankId,
                UploadGUID = purchaseRequestPayment.Guid,
                UpdatedBy = userid,
                PRPaymentId = purchaseRequestPayment.Id
            };
            context.LadgerEntryDB.Add(creditEntry);


        }


        public IRPaymentDetails UploadIRPaymentBySupplierId(int supplierid)
        {
            int userid = 0;
            Guid id = Guid.NewGuid();
            TransactionOptions option = new TransactionOptions();
            option.IsolationLevel = IsolationLevel.RepeatableRead;
            option.Timeout = TimeSpan.FromSeconds(90);
            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, option))
            // using (var scope = new TransactionScope())
            {
                using (var authContext = new AuthContext())
                {
                    List<IRPaymentDetails> paymentList = authContext.IRPaymentDetailsDB.Where(x => x.SupplierId == supplierid).ToList();
                    if (paymentList != null && paymentList.Any())
                    {
                        foreach (var irPaymentDetail in paymentList)
                        {
                            bool IsSupplierAdvance = true;
                            int IRpaymentId = irPaymentDetail.Id;
                            var amount = irPaymentDetail.TotalReaminingAmount;
                            DebitLedgerEntrySupplieradvacepay(irPaymentDetail.SupplierId, authContext, amount ?? 0, userid, irPaymentDetail.RefNo, irPaymentDetail.Remark, irPaymentDetail.Guid, irPaymentDetail.BankId, IsSupplierAdvance, IRpaymentId, irPaymentDetail.PaymentDate);
                            MakeIRPaymentHistory(irPaymentDetail.TotalReaminingAmount, 0, IRpaymentId, 0, userid, advance, authContext);
                            authContext.Commit();
                        }
                    }

                }
                scope.Complete();
            }
            return null;
        }

        public IRMasterViewDc GetIRViewForAmount(int Id)
        {
            using (var context = new AuthContext())
            {
                IRMaster IRR = context.IRMasterDB.Where(x => x.Id == Id).FirstOrDefault();
                IRMasterViewDc iRMasterViewDc = new IRMasterViewDc();
                // calculate purchase amount
                iRMasterViewDc.PurchaseAmount = IRR.TotalAmount + (IRR.Discount ?? 0) - (IRR.Gstamt ?? 0);
                if (IRR.OtherAmountType == null)
                {
                    if (IRR.OtherAmount != null)
                    {
                        if (IRR.OtherAmount > 0)
                        {
                            iRMasterViewDc.PurchaseAmount = iRMasterViewDc.PurchaseAmount - IRR.OtherAmount ?? 0;
                        }
                        else
                        {
                            iRMasterViewDc.PurchaseAmount = iRMasterViewDc.PurchaseAmount + IRR.OtherAmount * (-1) ?? 0;

                        }
                    }
                }
                if (IRR.ExpenseAmountType == null)
                {
                    if (IRR.ExpenseAmount != null)
                    {
                        if (IRR.ExpenseAmount > 0)
                        {
                            iRMasterViewDc.PurchaseAmount = iRMasterViewDc.PurchaseAmount - IRR.ExpenseAmount ?? 0;
                        }
                        else
                        {
                            iRMasterViewDc.PurchaseAmount = iRMasterViewDc.PurchaseAmount + IRR.ExpenseAmount * (-1) ?? 0;

                        }

                    }
                }
                if (IRR.RoundoffAmountType == null)
                {
                    if (IRR.RoundofAmount != null)
                    {
                        if (IRR.RoundofAmount > 0)
                        {
                            iRMasterViewDc.PurchaseAmount = iRMasterViewDc.PurchaseAmount - IRR.RoundofAmount ?? 0;
                        }
                        else
                        {
                            iRMasterViewDc.PurchaseAmount = iRMasterViewDc.PurchaseAmount + IRR.RoundofAmount * (-1) ?? 0;

                        }
                    }
                }

                // end


                //calculate CGST,SGST,IGST amount
                bool IsIgstIR = false;
                var pm = from po in context.DPurchaseOrderMaster
                         join ir in context.IRMasterDB
                         on po.PurchaseOrderId equals ir.PurchaseOrderId
                         where ir.Id == Id
                         select new { po.DepoId, po.WarehouseId };
                var depodta = pm.FirstOrDefault();
                if (depodta.DepoId > 0)
                {
                    DepoMaster DepoDetail = context.DepoMasters.Where(a => a.DepoId == depodta.DepoId).SingleOrDefault();

                    //for igst case if true then apply condion to hide column of cgst sgst cess
                    if (!string.IsNullOrEmpty(DepoDetail.GSTin) && DepoDetail.GSTin.Length >= 11)
                    {
                        string DepoTin_No = DepoDetail.GSTin.Substring(0, 2);
                        IsIgstIR = !context.Warehouses.Any(x => x.GSTin != null && x.WarehouseId == depodta.WarehouseId && x.GSTin.Substring(0, 2) == DepoTin_No);
                    }
                }
                if (IsIgstIR)
                {
                    iRMasterViewDc.SGSTAmount = IRR.Gstamt;
                }
                else
                {
                    iRMasterViewDc.CGSTAmount = IRR.Gstamt / 2;
                    iRMasterViewDc.SGSTAmount = IRR.Gstamt / 2;
                }
                //

                //add Round off,Other amount,Expense amount
                iRMasterViewDc.OtherAmount = IRR.OtherAmount;
                iRMasterViewDc.RoundofAmount = IRR.RoundofAmount;
                iRMasterViewDc.ExpenseAmount = IRR.ExpenseAmount;
                iRMasterViewDc.InvoiceDate = IRR.InvoiceDate;
                iRMasterViewDc.TotalAmountRemaining = IRR.TotalAmountRemaining;
                iRMasterViewDc.TotalTaxAmount = IRR.Gstamt;
                iRMasterViewDc.Discount = IRR.Discount;


                //
                return iRMasterViewDc;
            }
        }

        private static void PrPaymentTransfersettleupdate(List<PrPaymentTransfer> prPayments, AuthContext context, double amount)
        {
            double settleamount = amount;
            foreach (var pr in prPayments)
            {
                if (settleamount > 0)
                {
                    if (pr.TransferredAmount >= settleamount)
                    {
                        pr.SettledAmount = settleamount;
                        settleamount = 0;
                    }
                    else if (pr.TransferredAmount < settleamount)
                    {
                        pr.SettledAmount = pr.TransferredAmount;
                        settleamount = settleamount - pr.TransferredAmount;

                    }
                }

            }
            foreach (var pr in prPayments)
            {
                context.Entry(pr).State = EntityState.Modified;

            }


        }

    }
}