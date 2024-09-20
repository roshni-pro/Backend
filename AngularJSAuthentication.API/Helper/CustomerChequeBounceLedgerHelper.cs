using AngularJSAuthentication.DataContracts.Transaction.Ledger;
using AngularJSAuthentication.Model.Account;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace AngularJSAuthentication.API.Helper
{
    public class CustomerChequeBounceLedgerHelper
    {
        public void OnBounce(int orderId, List<ChequeBounceVM> chequeList, double? penalty, int userid)
        {
            using (AuthContext authConetext = new AuthContext())
            {
                LadgerHelper helper = new LadgerHelper();

                var query = from o in authConetext.DbOrderMaster
                            join c in authConetext.Customers
                            on o.CustomerId equals c.CustomerId
                            where o.OrderId == orderId
                            select c.CustomerId;

                int customerId = query.FirstOrDefault();


                var customerLadgerID = helper.GetOrCreateLadgerTypeAndLadger("Customer", customerId, userid, authConetext).ID;
                var chequeLadgerID = helper.GetOrCreateLadgerTypeAndLadger("Bank", 0, userid, authConetext).ID;
                var penaltyLadgerID = helper.GetOrCreateLadgerTypeAndLadger("ChequePenalty", 0, userid, authConetext).ID;
                var receiptVoucherTypeID = helper.GetOrCreateVoucherType("Receipt", userid, authConetext).ID;
                var bounceVoucherTypeID = helper.GetOrCreateVoucherType("Bounce", userid, authConetext).ID;
                var cancelVoucherTypeID = helper.GetOrCreateVoucherType("Cheque Cancel", userid, authConetext).ID;

                if (chequeList != null && chequeList.Any())
                {
                    List<LadgerEntry> ledgerEntryList = new List<LadgerEntry>();
                    foreach (var cheque in chequeList)
                    {
                        LadgerEntry chequePaymentLedgerEntry = GetLadgetEntry(customerLadgerID, chequeLadgerID, cheque.Date, cheque.Amount, null, orderId, "Order", receiptVoucherTypeID, null, null, userid);
                        LadgerEntry oppositeChequePaymentLedgerEntry = GetLadgetEntry(chequeLadgerID, customerLadgerID, cheque.Date, null, cheque.Amount, orderId, "Order", receiptVoucherTypeID, null, null, userid);

                        LadgerEntry cancelChequePaymentLedgerEntry = GetLadgetEntry(chequeLadgerID, customerLadgerID, cheque.Date, cheque.Amount, null, orderId, "Order", cancelVoucherTypeID, null, null, userid);
                        LadgerEntry oppositeCancelChequePaymentLedgerEntry = GetLadgetEntry(customerLadgerID, chequeLadgerID, cheque.Date, null, cheque.Amount, orderId, "Order", cancelVoucherTypeID, null, null, userid);

                        LadgerEntry penaltyChequePaymentLedgerEntry = GetLadgetEntry(penaltyLadgerID, customerLadgerID, cheque.Date, penalty, null, orderId, "Order", bounceVoucherTypeID, null, null, userid);
                        LadgerEntry oppositePenaltyChequePaymentLedgerEntry = GetLadgetEntry(customerLadgerID, penaltyLadgerID, cheque.Date, null, penalty, orderId, "Order", bounceVoucherTypeID, null, null, userid);

                        ledgerEntryList.Add(chequePaymentLedgerEntry);
                        ledgerEntryList.Add(oppositeChequePaymentLedgerEntry);
                        ledgerEntryList.Add(cancelChequePaymentLedgerEntry);
                        ledgerEntryList.Add(oppositeCancelChequePaymentLedgerEntry);
                        ledgerEntryList.Add(penaltyChequePaymentLedgerEntry);
                        ledgerEntryList.Add(oppositePenaltyChequePaymentLedgerEntry);
                    }

                    authConetext.LadgerEntryDB.AddRange(ledgerEntryList);
                    authConetext.Commit();
                }
            }
        }


        public void OnRevertBounce(int orderId, List<ChequeBounceVM> chequeList, double? penalty, int userid)
        {
            using (AuthContext authConetext = new AuthContext())
            {

                var query = from o in authConetext.DbOrderMaster
                            join c in authConetext.Customers
                            on o.CustomerId equals c.CustomerId
                            where o.OrderId == orderId
                            select c.CustomerId;
                int customerId = query.FirstOrDefault();

                LadgerHelper helper = new LadgerHelper();
                authConetext.Database.Log = log => Debug.WriteLine(log);
                var customerLadgerID = helper.GetOrCreateLadgerTypeAndLadger("Customer", customerId, userid, authConetext).ID;
                var chequeLadgerID = helper.GetOrCreateLadgerTypeAndLadger("Bank", 0, userid, authConetext).ID;
                var penaltyLadgerID = helper.GetOrCreateLadgerTypeAndLadger("ChequePenalty", 0, userid, authConetext).ID;
                var receiptVoucherTypeID = helper.GetOrCreateVoucherType("Receipt", userid, authConetext).ID;
                var bounceVoucherTypeID = helper.GetOrCreateVoucherType("Bounce", userid, authConetext).ID;
                var cancelVoucherTypeID = helper.GetOrCreateVoucherType("Cheque Cancel", userid, authConetext).ID;

                if (chequeList != null && chequeList.Any())
                {
                    List<LadgerEntry> ledgerEntryList = new List<LadgerEntry>();
                    foreach (var cheque in chequeList)
                    {
                        //LadgerEntry cancelChequePaymentLedgerEntry = GetLadgetEntry(customerLadgerID, chequeLadgerID, cheque.Date, cheque.Amount, null, orderId, "Order", cancelVoucherTypeID, null, null, userid);
                        //LadgerEntry oppositeCancelChequePaymentLedgerEntry = GetLadgetEntry(chequeLadgerID, customerLadgerID, cheque.Date, null, cheque.Amount, orderId, "Order", cancelVoucherTypeID, null, null, userid);

                        LadgerEntry penaltyChequePaymentLedgerEntry = GetLadgetEntry(customerLadgerID, penaltyLadgerID, cheque.Date, penalty, null, orderId, "Order", bounceVoucherTypeID, null, null, userid);
                        LadgerEntry oppositePenaltyChequePaymentLedgerEntry = GetLadgetEntry(penaltyLadgerID, customerLadgerID, cheque.Date, null, penalty, orderId, "Order", bounceVoucherTypeID, null, null, userid);

                        //ledgerEntryList.Add(cancelChequePaymentLedgerEntry);
                        //ledgerEntryList.Add(oppositeCancelChequePaymentLedgerEntry);
                        ledgerEntryList.Add(penaltyChequePaymentLedgerEntry);
                        ledgerEntryList.Add(oppositePenaltyChequePaymentLedgerEntry);
                    }

                    authConetext.LadgerEntryDB.AddRange(ledgerEntryList);
                    authConetext.Commit();
                }
            }

        }


        public LadgerEntry GetLadgetEntry(long ledgerID, long affectedLadgerID, DateTime ledgerDate
            , double? credit, double? debit, int objectID, string objectType
            , long voucherTypeId, long? voucherNo, string refNo, int userid)
        {
            DateTime creationDate = DateTime.Now;
            LadgerEntry ledgerEntry = new LadgerEntry
            {
                CreatedBy = userid,
                UpdatedBy = userid,
                CreatedDate = creationDate,
                UpdatedDate = creationDate,
                Active = true,
                AffectedLadgerID = affectedLadgerID,
                Credit = credit,
                Debit = debit,
                Date = ledgerDate,
                IrPaymentDetailsId = null,
                IsSupplierAdvancepay = null,
                LagerID = ledgerID,
                ObjectID = objectID,
                ObjectType = objectType,
                VouchersTypeID = voucherTypeId,
                VouchersNo = voucherNo,
                RefNo = refNo
            };
            return ledgerEntry;
        }

    }


}