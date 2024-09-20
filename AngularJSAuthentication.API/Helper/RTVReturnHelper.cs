using AngularJSAuthentication.Model;
using AngularJSAuthentication.Model.Account;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AngularJSAuthentication.API.Helper
{
    public class RTVReturnHelper
    {
        public bool MakeLedgerEntries(int rtvMasterId, AuthContext context, int userId, DateTime rtvDate)
        {
            bool result = true;
            LadgerHelper helper = new LadgerHelper();
            Ladger purchaseReturnLedger = helper.GetOrCreateLadgerTypeAndLadger("PurchaseReturn", 0, userId, context);

            var cgstLedgerQuery = from l in context.LadgerDB
                                  join lt in context.LadgerTypeDB on l.LadgertypeID equals lt.ID
                                  where (!string.IsNullOrEmpty(lt.Name) && lt.Name.ToLower() == "tax")
                                     && (!string.IsNullOrEmpty(l.Name) && l.Name.ToLower() == "cgst")
                                  select l;
            var sgstLedgerQuery = from l in context.LadgerDB
                                  join lt in context.LadgerTypeDB on l.LadgertypeID equals lt.ID
                                  where (!string.IsNullOrEmpty(lt.Name) && lt.Name.ToLower() == "tax")
                                     && (!string.IsNullOrEmpty(l.Name) && l.Name.ToLower() == "sgst")
                                  select l;
            Ladger cgstLedger = cgstLedgerQuery.First();
            Ladger sgstLedger = sgstLedgerQuery.First();

            VoucherType vch = context.VoucherTypeDB.FirstOrDefault(x => x.Name == "PurchaseReturn");
 
            RTVMaster rtvMaster = context.RTVMasterDB.FirstOrDefault(x => x.Id == rtvMasterId);
            
            if(rtvMaster != null)
            {
                string partticular = "RTV return for supplier", objectType = "RTV", guid = Guid.NewGuid().ToString();
                Ladger supplierLedger = context.LadgerDB.FirstOrDefault(x => x.ObjectType == "Supplier" && x.ObjectID == rtvMaster.SupplierId);
                LadgerEntry purchaseReturnLedgerEntry = new LadgerEntry
                {
                    Active = true,
                    AffectedLadgerID = purchaseReturnLedger.ID,
                    CreatedBy =userId,
                    CreatedDate = DateTime.Now,
                    Credit = null,
                    Debit = rtvMaster.TotalAmount,     //(rtvMaster.TotalAmount - rtvMaster.GSTAmount),
                    Date = rtvDate,
                    IrPaymentDetailsId = null,
                    IsSupplierAdvancepay = null,
                    LagerID = supplierLedger.ID,
                    ObjectID = rtvMaster.Id,
                    ObjectType = objectType,
                    Particulars = partticular,
                    PRPaymentId = null,
                    RefNo = rtvMaster.Id.ToString(),
                    Remark = rtvMaster.Id.ToString(),
                    UpdatedBy = userId,
                    UpdatedDate = DateTime.Now,
                    UploadGUID = guid,
                    VouchersTypeID = vch.ID,
                    VouchersNo = null
                };

                
                
                LadgerEntry reversePurchaseReturnLedgerEntry =  MakeReverseEntry(purchaseReturnLedgerEntry);

                LadgerEntry cgstLedgerEntry = null;
                LadgerEntry reverseCgstLedgerEntry = null;
                LadgerEntry sgstLedgerEntry = null;
                LadgerEntry reverseSgstLedgerEntry = null;
                if (rtvMaster.GSTAmount > 0)
                {
                    VoucherType taxVch = context.VoucherTypeDB.FirstOrDefault(x => x.Name == "Tax");
                    cgstLedgerEntry = MakeCopyOfLedgerEntry(purchaseReturnLedgerEntry);
                    cgstLedgerEntry.AffectedLadgerID = cgstLedger.ID;
                    cgstLedgerEntry.LagerID = supplierLedger.ID;
                    cgstLedgerEntry.Debit = rtvMaster.GSTAmount/2.0;
                    cgstLedgerEntry.Credit = null;
                    cgstLedgerEntry.VouchersTypeID = taxVch.ID;

                    reverseCgstLedgerEntry = MakeReverseEntry(cgstLedgerEntry);

                    sgstLedgerEntry = MakeCopyOfLedgerEntry(cgstLedgerEntry);
                    sgstLedgerEntry.AffectedLadgerID = sgstLedger.ID;
                    reverseSgstLedgerEntry = MakeReverseEntry(sgstLedgerEntry);
                }
                




                context.LadgerEntryDB.Add(purchaseReturnLedgerEntry);
                context.LadgerEntryDB.Add(reversePurchaseReturnLedgerEntry);
                if(cgstLedgerEntry != null)
                {
                    context.LadgerEntryDB.Add(cgstLedgerEntry);
                    context.LadgerEntryDB.Add(reverseCgstLedgerEntry);
                    context.LadgerEntryDB.Add(sgstLedgerEntry);
                    context.LadgerEntryDB.Add(reverseSgstLedgerEntry);
                }
            }

            context.Commit();
            return result;
        }
    
    
        private LadgerEntry MakeReverseEntry(LadgerEntry ledgerEntry)
        {
            LadgerEntry reverseLedgerEntry = new LadgerEntry
            {
                Active = true,
                AffectedLadgerID = ledgerEntry.LagerID,
                CreatedBy = ledgerEntry.CreatedBy,
                CreatedDate = ledgerEntry.CreatedDate,
                Credit = ledgerEntry.Debit,
                Debit = ledgerEntry.Credit,
                Date = ledgerEntry.Date,
                IrPaymentDetailsId = ledgerEntry.IrPaymentDetailsId,
                IsSupplierAdvancepay = ledgerEntry.IsSupplierAdvancepay,
                LagerID = ledgerEntry.AffectedLadgerID,
                ObjectID = ledgerEntry.ObjectID,
                ObjectType = ledgerEntry.ObjectType,
                Particulars = ledgerEntry.Particulars,
                PRPaymentId = ledgerEntry.PRPaymentId,
                RefNo = ledgerEntry.RefNo,
                Remark = ledgerEntry.Remark,
                UpdatedBy = ledgerEntry.UpdatedBy,
                UpdatedDate = ledgerEntry.UpdatedDate,
                UploadGUID = ledgerEntry.UploadGUID,
                VouchersTypeID = ledgerEntry.VouchersTypeID,
                VouchersNo = ledgerEntry.VouchersNo
            };
            return reverseLedgerEntry;
        }

        private LadgerEntry MakeCopyOfLedgerEntry(LadgerEntry ledgerEntry)
        {
            LadgerEntry reverseLedgerEntry = new LadgerEntry
            {
                Active = true,
                AffectedLadgerID = ledgerEntry.AffectedLadgerID,
                CreatedBy = ledgerEntry.CreatedBy,
                CreatedDate = ledgerEntry.CreatedDate,
                Credit = ledgerEntry.Credit,
                Debit = ledgerEntry.Debit,
                Date = ledgerEntry.Date,
                IrPaymentDetailsId = ledgerEntry.IrPaymentDetailsId,
                IsSupplierAdvancepay = ledgerEntry.IsSupplierAdvancepay,
                LagerID = ledgerEntry.LagerID,
                ObjectID = ledgerEntry.ObjectID,
                ObjectType = ledgerEntry.ObjectType,
                Particulars = ledgerEntry.Particulars,
                PRPaymentId = ledgerEntry.PRPaymentId,
                RefNo = ledgerEntry.RefNo,
                Remark = ledgerEntry.Remark,
                UpdatedBy = ledgerEntry.UpdatedBy,
                UpdatedDate = ledgerEntry.UpdatedDate,
                UploadGUID = ledgerEntry.UploadGUID,
                VouchersTypeID = ledgerEntry.VouchersTypeID,
                VouchersNo = ledgerEntry.VouchersNo
            };
            return reverseLedgerEntry;
        }

    }
}