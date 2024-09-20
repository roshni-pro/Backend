using AngularJSAuthentication.DataContracts.Transaction.Ledger;
using AngularJSAuthentication.Model;
using AngularJSAuthentication.Model.Account;
using AngularJSAuthentication.Model.PurchaseRequestPayments;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace AngularJSAuthentication.API.Helper
{
    public class LadgerHelper
    {
        public LadgerType GetOrCreateLadgerType(string code, int userid, AuthContext context)
        {
            LadgerType ladgerType = context.LadgerTypeDB.FirstOrDefault(x => x.code.ToLower() == code.ToLower());
            if (ladgerType == null)
            {
                ladgerType = new LadgerType
                {
                    Active = true,
                    code = code,
                    CreatedBy = userid,
                    CreatedDate = DateTime.Now,
                    Name = code,
                    Sequence = null,
                    UpdatedBy = userid,
                    UpdatedDate = DateTime.Now
                };
                context.LadgerTypeDB.Add(ladgerType);
                context.Commit();
            }
            return ladgerType;
        }

        public Ladger GetOrCreateLadgerTypeAndLadger(string objectType, int objectID, int userid, AuthContext context)
        {
            LadgerType ladgerType = GetOrCreateLadgerType(objectType, userid, context);
            Ladger ladger = context.LadgerDB.FirstOrDefault(x =>
                ((objectID > 0 && x.ObjectID == objectID) || (objectID == 0 && (x.ObjectID == null || x.ObjectID == 0)))
                && !string.IsNullOrEmpty(x.ObjectType) && x.ObjectType.ToLower() == objectType.ToLower());
            if (ladger == null)
            {
                ladger = new Ladger
                {
                    Active = true,
                    CreatedBy = userid,
                    CreatedDate = DateTime.Now,
                    InventoryValuesAreAffected = false,
                    LadgertypeID = ladgerType.ID,
                    ObjectID = objectID,
                    ObjectType = objectType,
                    ProvidedBankDetails = false,
                    UpdatedBy = userid,
                    UpdatedDate = DateTime.Now
                };
                context.LadgerDB.Add(ladger);
                context.Commit();
            }
            return ladger;
        }

        public Ladger GetOrCreateLadgerTypeAndLadgerWithLedgerName(string objectType, int objectID, int userid, AuthContext context, string ledgerName)
        {
            LadgerType ladgerType = GetOrCreateLadgerType(objectType, userid, context);
            Ladger ladger = context.LadgerDB.FirstOrDefault(x =>
                ((objectID > 0 && x.ObjectID == objectID) || (objectID == 0 && (x.ObjectID == null || x.ObjectID == 0)))
                && !string.IsNullOrEmpty(x.ObjectType) && x.ObjectType.ToLower() == ledgerName.ToLower());
            if (ladger == null)
            {
                ladger = new Ladger
                {
                    Active = true,
                    CreatedBy = userid,
                    CreatedDate = DateTime.Now,
                    InventoryValuesAreAffected = false,
                    LadgertypeID = ladgerType.ID,
                    ObjectID = objectID,
                    ObjectType = ledgerName,
                    ProvidedBankDetails = false,
                    UpdatedBy = userid,
                    UpdatedDate = DateTime.Now,
                    Alias = ledgerName,
                    Name = ledgerName
                };
                context.LadgerDB.Add(ladger);
                context.Commit();
            }
            return ladger;
        }


        public Ladger GetOrCreateLadger(string objectType, int ladgerTypeId, int objectID, int userid, AuthContext context)
        {
            Ladger ladger = context.LadgerDB.FirstOrDefault(x => x.ObjectID == objectID && !string.IsNullOrEmpty(x.ObjectType) && x.ObjectType.ToLower() == objectType.ToLower());
            if (ladger == null)
            {
                ladger = new Ladger
                {
                    Active = true,
                    CreatedBy = userid,
                    CreatedDate = DateTime.Now,
                    InventoryValuesAreAffected = false,
                    LadgertypeID = ladgerTypeId,
                    ObjectID = objectID,
                    ObjectType = objectType,
                    ProvidedBankDetails = false,
                    UpdatedBy = userid,
                    UpdatedDate = DateTime.Now
                };
                context.LadgerDB.Add(ladger);
                context.Commit();
            }
            return ladger;
        }

        public VoucherType GetOrCreateVoucherType(string name, int userid, AuthContext context)
        {
            VoucherType voucherType = context.VoucherTypeDB.FirstOrDefault(x => x.Name.ToLower() == name);
            if (voucherType == null)
            {
                voucherType = new VoucherType
                {
                    Active = true,
                    CreatedBy = userid,
                    CreatedDate = DateTime.Now,
                    DisplayName = name,
                    IsManualEdit = false,
                    Name = name,
                    UpdatedBy = userid,
                    UpdatedDate = DateTime.Now
                };
                context.VoucherTypeDB.Add(voucherType);
                context.Commit();
            }
            return voucherType;
        }
        public List<LadgerEntry> Createadjustladgerentry(LadgerEntry ladgerEntry, int userid, AuthContext authContext)
        {

            List<LadgerEntry> ledgerlist = new List<LadgerEntry>();
            LadgerEntry oppsiteledgerentry = new LadgerEntry();
            if (ladgerEntry != null)
            {
                ladgerEntry.CreatedBy = userid;
                ladgerEntry.UpdatedBy = userid;
                ladgerEntry.CreatedDate = DateTime.Now;
                ladgerEntry.UpdatedDate = DateTime.Now;

                oppsiteledgerentry.Active = true;
                oppsiteledgerentry.AffectedLadgerID = ladgerEntry.LagerID;
                oppsiteledgerentry.CreatedBy = userid;
                oppsiteledgerentry.CreatedDate = DateTime.Now;
                if (ladgerEntry.Credit == null)
                {
                    oppsiteledgerentry.Credit = ladgerEntry.Debit;
                    oppsiteledgerentry.Debit = null;
                }
                else if (ladgerEntry.Debit == null)
                {
                    oppsiteledgerentry.Credit = null;
                    oppsiteledgerentry.Debit = ladgerEntry.Credit;
                }
                oppsiteledgerentry.LagerID = ladgerEntry.AffectedLadgerID;
                oppsiteledgerentry.ObjectID = ladgerEntry.ObjectID;
                oppsiteledgerentry.ObjectType = ladgerEntry.ObjectType;
                oppsiteledgerentry.Particulars = ladgerEntry.Particulars;
                oppsiteledgerentry.RefNo = ladgerEntry.RefNo;
                oppsiteledgerentry.Remark = ladgerEntry.Remark;
                oppsiteledgerentry.UpdatedBy = userid;
                oppsiteledgerentry.UpdatedDate = DateTime.Now;
                oppsiteledgerentry.VouchersTypeID = ladgerEntry.VouchersTypeID;
                oppsiteledgerentry.VouchersNo = ladgerEntry.VouchersNo;
                oppsiteledgerentry.Date = ladgerEntry.Date;

                ledgerlist.Add(ladgerEntry);
                ledgerlist.Add(oppsiteledgerentry);
            }
            authContext.LadgerEntryDB.AddRange(ledgerlist);
            authContext.Commit();

            return ledgerlist;
        }

        public string GetByLedgerID(long ledgerId)
        {
            using (var authContext = new AuthContext())
            {
                int ledgerTypeId = authContext.LadgerDB.FirstOrDefault(x => x.ID == ledgerId).LadgertypeID.Value;
                var ledgerTypeList = authContext.LadgerTypeDB.ToList();
                if (ledgerTypeList.Where(x => x.ID == ledgerTypeId).FirstOrDefault().code.ToLower() == "customer")
                {
                    var query = from lb in authContext.LadgerDB
                                join cus in authContext.Customers
                                on lb.ObjectID equals cus.CustomerId
                                where lb.ID == ledgerId
                                select cus.Name + " - " + cus.ShopName + " - " + cus.Skcode;
                    var ladgerList = query.FirstOrDefault();
                    return ladgerList;
                }
                else if (ledgerTypeList.Where(x => x.ID == ledgerTypeId).FirstOrDefault().code.ToLower() == "supplier")
                {
                    var query = from lb in authContext.LadgerDB
                                join cus in authContext.Suppliers
                                on lb.ObjectID equals cus.SupplierId
                                where lb.ID == ledgerId
                                select cus.Name + " - " + cus.Brand + " - " + cus.SUPPLIERCODES;
                    var ladgerList = query.FirstOrDefault();
                    return ladgerList;
                }
                else if (ledgerTypeList.Where(x => x.ID == ledgerTypeId).FirstOrDefault().code.ToLower() == "agent")
                {

                    var query = from lb in authContext.LadgerDB
                                join pop in authContext.Peoples
                                on lb.ObjectID equals pop.PeopleID
                                where lb.ID == ledgerId
                                select pop.DisplayName + " - " + pop.Mobile;
                    var ladgerList = query.FirstOrDefault();
                    return ladgerList;

                }
                else
                {
                    var query = from lb in authContext.LadgerDB
                                where lb.ID == ledgerId
                                select lb.Name + " - " + lb.Alias;
                    var ladgerList = query.FirstOrDefault();
                    return ladgerList;

                }
            }
        }

        public bool MakeReverseEntries(string voucherName, string objectTye, long objectId)
        {

            using (var context = new AuthContext())
            {
                VoucherType voucherType = GetOrCreateVoucherType(voucherName, 0, context);
                List<LadgerEntry> ladgerEntryList = context
                    .LadgerEntryDB.Where(x => x.ObjectID == objectId && !string.IsNullOrEmpty(x.ObjectType) && x.ObjectType.ToLower() == objectTye.ToLower() && x.VouchersTypeID!=6)
                    .ToList();

                if(ladgerEntryList != null && ladgerEntryList.Count > 0)
                {
                    foreach (LadgerEntry entity in ladgerEntryList)
                    {
                        context.Entry(entity).State = EntityState.Detached;

                        entity.ID = 0;
                        long tempLedgerId = entity.LagerID.Value;
                        entity.LagerID = entity.AffectedLadgerID;
                        entity.AffectedLadgerID = tempLedgerId;
                        entity.VouchersTypeID = voucherType.ID;
                        context.LadgerEntryDB.Add(entity);
                        context.Commit();
                    }
                }
            }
            return true;
        }

        public bool ReturnPayment(string objectTye, long objectId)
        {
            using (var context = new AuthContext())
            {
              
                List<LadgerEntry> ladgerEntryList = context
                    .LadgerEntryDB.Where(x => x.ObjectID == objectId && !string.IsNullOrEmpty(x.ObjectType) && x.ObjectType.ToLower() == objectTye.ToLower() && x.VouchersTypeID==6)
                    .ToList();
               
                if (ladgerEntryList != null && ladgerEntryList.Count > 0 )
                {
                  
                    var IRPaymentLedgerEntry = ladgerEntryList.Where(x => x.VouchersTypeID == 6 && x.ObjectType == "IR" && x.IrPaymentDetailsId > 0).ToList();
                    var IRPaymentDetailsData = IRPaymentLedgerEntry.Where(x => x.VouchersTypeID == 6 && x.ObjectType == "IR" && x.IrPaymentDetailsId > 0 && x.Credit > 0).Select(x => new IRPayementRevertDc
                    { IRPaymentDetailsId = x.IrPaymentDetailsId, CreditAmount = x.Credit }).ToList();


                    if (IRPaymentDetailsData != null && IRPaymentDetailsData.Count>0)
                    {
                        SettleAmountAndDeleteHistory(IRPaymentDetailsData,context, IRPaymentLedgerEntry);

                    }
                    var PRSettleLedgerEntry = ladgerEntryList.Where(x => x.VouchersTypeID == 6 && x.ObjectType == "IR" && x.IrPaymentDetailsId == null ).ToList();
                    var PRPaymentDetailsData = ladgerEntryList.Where(x => x.VouchersTypeID == 6 && x.ObjectType == "IR" && x.IrPaymentDetailsId== null && x.Credit > 0 && x.PRPaymentId>0 ).Select(x => new { x.RefNo, x.Credit,x.UploadGUID,x.Date,x.PRPaymentId}).FirstOrDefault();
                        if (PRPaymentDetailsData!=null && PRPaymentDetailsData.RefNo != null && PRPaymentDetailsData.UploadGUID != null)
                        {
                            PurchaseRequestPayment purchaseRequestPayment = context.PurchaseRequestPaymentsDB.Where(x => x.RefNo == PRPaymentDetailsData.RefNo && x.Guid == PRPaymentDetailsData.UploadGUID && x.PaymentDate== PRPaymentDetailsData.Date &&(x.Id!=0 && x.Id== PRPaymentDetailsData.PRPaymentId)).FirstOrDefault();
                            if (purchaseRequestPayment != null)
                            {
                            List<LadgerEntry> PRLedgerEntry = context.LadgerEntryDB.Where(x => x.ObjectID == purchaseRequestPayment.Id && x.ObjectType == "PR").ToList();

                            AddLedgerAmountandDeleteCancelLedger(purchaseRequestPayment.Id, "PR", PRPaymentDetailsData.Credit, context, PRSettleLedgerEntry, PRLedgerEntry);
                            }
                        }

                    
                    context.Commit();
                }
            }
            return true;
        }

        private void SettleAmountAndDeleteHistory(List<IRPayementRevertDc> iRPayementRevertDcs,AuthContext context,List<LadgerEntry> ledgerEntry) 
        {
            List<IRPaymentDetails> iRPaymentDetailsdata = new List<IRPaymentDetails>();
            List<IRPaymentDetailHistory> iRPaymentDetailHistoriesdata = new List<IRPaymentDetailHistory>();
            foreach (var IRPayemt in iRPayementRevertDcs)
            {
                IRPaymentDetails irPaymentDetail = context.IRPaymentDetailsDB.Where(x => x.Id == IRPayemt.IRPaymentDetailsId).FirstOrDefault();
                List<LadgerEntry> IRLedgerupdate =context.LadgerEntryDB.Where(x => x.IrPaymentDetailsId == irPaymentDetail.Id && x.ObjectID == 0).ToList();
                List<LadgerEntry> Ledgerupdate = ledgerEntry.Where(x => x.IrPaymentDetailsId == irPaymentDetail.Id).ToList();
                if (IRLedgerupdate.Count > 0) {
                    
                        Ledgerupdate = Ledgerupdate.Where(x => IRLedgerupdate.Any(y => y.ID == x.ID)).ToList();
                    
                }
                irPaymentDetail.TotalReaminingAmount = irPaymentDetail.TotalReaminingAmount + IRPayemt.CreditAmount;
                iRPaymentDetailsdata.Add(irPaymentDetail);
                List<IRPaymentDetailHistory> iRPaymentDetailHistory = context.IRPaymentDetailHistoryDB.Where(x => x.IRPaymentDetailId == IRPayemt.IRPaymentDetailsId && x.Amount == IRPayemt.CreditAmount).ToList();
                foreach (var IRHistory in iRPaymentDetailHistory) {
                    IRHistory.Deleted = true;
                    IRHistory.IsActive = false;

                }
                iRPaymentDetailHistoriesdata.AddRange(iRPaymentDetailHistory);
                AddLedgerAmountandDeleteCancelLedger(0,null, IRPayemt.CreditAmount, context, Ledgerupdate, IRLedgerupdate);
              

            }

            foreach (var irPayment in iRPaymentDetailsdata) 
            {
                context.Entry(irPayment).State = EntityState.Modified;
            }
            foreach (var history in iRPaymentDetailHistoriesdata)
            {
                context.Entry(history).State = EntityState.Modified;
            }
        }

        private void SettlePRAmountAndAdjustmentAmount(PurchaseRequestPayment purchaseRequestPayment, double? amount, AuthContext context,List<LadgerEntry> ladgerEntries,long VouchTypeId) 
        {
           List<LadgerEntry> ladgerEntriesdata = new List<LadgerEntry>();
            List<LadgerEntry> addLedgerEntriesdata = new List<LadgerEntry>();
            List<LadgerEntry> PrledgerEntries = context.LadgerEntryDB.Where(x => x.ObjectID == purchaseRequestPayment.Id && x.ObjectType == "PR").ToList();
            if (PrledgerEntries != null && PrledgerEntries.Count>0)
            {
                foreach (LadgerEntry entity in PrledgerEntries)
                {
                    if (entity.Credit > 0)
                    {
                        entity.Credit = entity.Credit + amount;
                    }
                    else if (entity.Debit > 0)
                    {
                        entity.Debit = entity.Debit + amount;
                    }
                    ladgerEntriesdata.Add(entity);

                }
                    context.LadgerEntryDB.RemoveRange(ladgerEntries);
                
            }

            
            else {
                foreach (LadgerEntry entity in ladgerEntries)
                {
                   
                    entity.Particulars = null;
                    entity.VouchersNo = 0;
                    entity.ObjectID = purchaseRequestPayment.Id;
                    entity.ObjectType = "PR";
                    ladgerEntriesdata.Add(entity);
                }

            }
            foreach (var ledger in ladgerEntriesdata)
            {
                context.Entry(ledger).State = EntityState.Modified;
            }
            

        }
        private void AddLedgerAmountandDeleteCancelLedger(long? ObjectId,string ObjectType, double? amount, AuthContext context,List<LadgerEntry> ladgerEntries, List<LadgerEntry> SettleladgerEntries)
        {
            List<LadgerEntry> ladgerEntriesdata = new List<LadgerEntry>();
            if (SettleladgerEntries != null && SettleladgerEntries.Count > 0)
            {
                foreach (LadgerEntry entity in SettleladgerEntries)
                {
                    if (entity.Credit > 0)
                    {
                        entity.Credit = entity.Credit + amount;
                    }
                    else if (entity.Debit > 0)
                    {
                        entity.Debit = entity.Debit + amount;
                    }
                    ladgerEntriesdata.Add(entity);

                }
                context.LadgerEntryDB.RemoveRange(ladgerEntries);
              

            }


            else
            {
                foreach (LadgerEntry entity in ladgerEntries)
                {

                    entity.Particulars = null;
                    entity.VouchersNo = 0;
                    entity.ObjectID = ObjectId;
                    entity.ObjectType = ObjectType;
                    ladgerEntriesdata.Add(entity);
                }

            }
            foreach (var ledger in ladgerEntriesdata)
            {
                context.Entry(ledger).State = EntityState.Modified;
            }
        }
    }
}