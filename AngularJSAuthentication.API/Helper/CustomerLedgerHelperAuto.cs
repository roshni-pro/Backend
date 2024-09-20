using AngularJSAuthentication.API.NewHelper;
using AngularJSAuthentication.Common.Constants;
using AngularJSAuthentication.Model.Account;
using NLog;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;

namespace AngularJSAuthentication.API.Helper
{
    public class CustomerLedgerHelperAuto
    {
        public static Logger logger = LogManager.GetCurrentClassLogger();
        public void OnDispatch(int orderID, int customerId, int userID)
        {
            if (AppConstants.IsUsingAutoLedger)
            {
                return;
            }
            try
            {
                using (var authContext = new AuthContext())
                {
                    var query = from l in authContext.LadgerDB
                                join lt in authContext.LadgerTypeDB
                                on l.LadgertypeID equals lt.ID
                                where lt.code.ToLower() == "tax"
                                    || (lt.code.ToLower() == "customer" && l.ObjectID == customerId)
                                    || lt.code.ToLower() == "sales"
                                    || lt.code.ToLower() == "deliverycharges"
                                select l;

                    var ledgerList = query.ToList();
                    SqlParameter orderIdParam = null;
                    orderIdParam = new SqlParameter("@OrderID", orderID);

                    string spName = "CustomerLedgerEntryOnIssued";



                    List<CustomerLedgerEntryModel> dispatchOrderList = null;
                    spName = spName + " @OrderID";
                    dispatchOrderList = authContext.Database.SqlQuery<CustomerLedgerEntryModel>(spName, orderIdParam).ToList();


                    var voucherTypes = authContext.VoucherTypeDB.Where(x => x.Active).ToList();
                    List<LadgerEntry> result = new List<LadgerEntry>();
                    if (dispatchOrderList != null && dispatchOrderList.Count > 0)
                    {
                        //var result = new ConcurrentBag<LadgerEntry>();
                        var salesLedgerId = ledgerList.FirstOrDefault(x => x.LadgertypeID != 1 && !string.IsNullOrEmpty(x.Name) && x.Name.ToLower() == "sales").ID;
                        var gst5LedgerId = ledgerList.FirstOrDefault(x => x.LadgertypeID != 1 && !string.IsNullOrEmpty(x.Name) && x.Name.ToLower() == "gst5").ID;
                        var gst12LedgerId = ledgerList.FirstOrDefault(x => x.LadgertypeID != 1 && !string.IsNullOrEmpty(x.Name) && x.Name.ToLower() == "gst12").ID;
                        var gst14LedgerId = ledgerList.FirstOrDefault(x => x.LadgertypeID != 1 && !string.IsNullOrEmpty(x.Name) && x.Name.ToLower() == "gst14").ID;
                        var gst18LedgerId = ledgerList.FirstOrDefault(x => x.LadgertypeID != 1 && !string.IsNullOrEmpty(x.Name) && x.Name.ToLower() == "gst18").ID;
                        var gst28LedgerId = ledgerList.FirstOrDefault(x => x.LadgertypeID != 1 && !string.IsNullOrEmpty(x.Name) && x.Name.ToLower() == "gst28").ID;
                        var cessTaxLedgerId = ledgerList.FirstOrDefault(x => x.LadgertypeID != 1 && !string.IsNullOrEmpty(x.Name) && x.Name.ToLower() == "cesstax").ID;
                        var deliveryChargesLedgerId = ledgerList.FirstOrDefault(x => x.LadgertypeID != 1 && !string.IsNullOrEmpty(x.Name) && x.Name.ToLower() == "deliverycharges").ID;
                        var salesVoucherTypeId = voucherTypes.FirstOrDefault(x => x.Name.ToLower() == "sales").ID;
                        var receiptVoucherTypeId = voucherTypes.FirstOrDefault(x => x.Name.ToLower() == "receipt").ID;
                        var journalVoucherTypeId = voucherTypes.FirstOrDefault(x => x.Name.ToLower() == "journal").ID;
                        var taxVoucherTypeId = voucherTypes.FirstOrDefault(x => x.Name.ToLower() == "tax").ID;
                        //foreach (var dispatchOrder in dispatchOrderList)

                        var ordergroup = dispatchOrderList.GroupBy(x => x.OrderId).ToList();

                        foreach (var group in ordergroup)
                        {

                            CustomerLedgerEntryModel dispatchOrder = group.First();
                            Voucher vch = CustomerLedgerHelper.GetOrCreateVoucher(!string.IsNullOrEmpty(dispatchOrder.InvoiceNo) ? dispatchOrder.InvoiceNo : dispatchOrder.OrderId.ToString());

                            long? ledgerIDNullable = ledgerList.Where(x => x.LadgertypeID == 1 && x.ObjectID == dispatchOrder.CustomerId)?.FirstOrDefault()?.ID;

                            if (!ledgerIDNullable.HasValue)
                            {
                                Ladger le = new Ladger();
                                le.Active = true;
                                le.Address = "";
                                le.Alias = dispatchOrder.Skcode;
                                le.Country = "";
                                le.LadgertypeID = 1;
                                le.Name = dispatchOrder.Skcode;
                                le.ObjectID = dispatchOrder.CustomerId;
                                le.ObjectType = "Customer";
                                le.ProvidedBankDetails = true;
                                le.UpdatedBy = 1;
                                le.UpdatedDate = DateTime.Now;
                                le.CreatedBy = 1;
                                le.CreatedDate = DateTime.Now;
                                authContext.LadgerDB.Add(le);
                                authContext.Commit();

                                ledgerIDNullable = le.ID;
                            }

                            long ledgerID = ledgerIDNullable.Value;
                            String particular = "";
                            LadgerEntry entry = null;
                            if (dispatchOrder.TotalAmount + (dispatchOrder.BillDiscountAmount ?? 0) + (dispatchOrder.WalletAmount ?? 0) > 0)
                            {
                                particular = "Order Placed for order " + dispatchOrder.OrderId;
                                double customerAmount = (dispatchOrder.TotalAmount ?? 0) - (dispatchOrder.DeliveryCharge ?? 0) - (dispatchOrder.TaxAmount ?? 0) + (dispatchOrder.BillDiscountAmount ?? 0) + (dispatchOrder.WalletAmount ?? 0);
                                entry = CreateLedgerEntry(dispatchOrder, ledgerID, "Order", particular, null, customerAmount, salesLedgerId, salesVoucherTypeId, vch.ID);
                                entry.Date = dispatchOrder.Deliverydate;
                                result.Add(entry);

                                double salesAmount = (dispatchOrder.TotalAmount ?? 0) - (dispatchOrder.DeliveryCharge ?? 0) - (dispatchOrder.TaxAmount ?? 0) + (dispatchOrder.BillDiscountAmount ?? 0) + (dispatchOrder.WalletAmount ?? 0);
                                particular = "Sales for order " + dispatchOrder.OrderId;
                                entry = CreateLedgerEntry(dispatchOrder, salesLedgerId, "Order", particular, salesAmount, null, ledgerID, salesVoucherTypeId, vch.ID);
                                entry.Date = dispatchOrder.Deliverydate;
                                //authContext.LadgerEntryDB.Add(entry);
                                result.Add(entry);
                            }

                            if (dispatchOrder.DeliveryCharge != null && dispatchOrder.DeliveryCharge > 0)
                            {
                                particular = "Delivery charge " + dispatchOrder.DeliveryCharge + " regarding Order " + dispatchOrder.OrderId;
                                entry = CreateLedgerEntry(dispatchOrder, ledgerID, "Order", particular, null, dispatchOrder.DeliveryCharge, deliveryChargesLedgerId, journalVoucherTypeId, vch.ID);
                                entry.Date = dispatchOrder.Deliverydate;
                                result.Add(entry);

                                particular = "Delivery charge " + dispatchOrder.DeliveryCharge + " debited regarding Order " + dispatchOrder.OrderId;
                                entry = CreateLedgerEntry(dispatchOrder, deliveryChargesLedgerId, "Order", particular, dispatchOrder.DeliveryCharge, null, ledgerID, journalVoucherTypeId, vch.ID);
                                entry.Date = dispatchOrder.Deliverydate;
                                result.Add(entry);
                            }

                            if (dispatchOrder.Five != null && dispatchOrder.Five > 0)
                            {
                                particular = "GST5 credited regarding Order " + dispatchOrder.OrderId;
                                entry = CreateLedgerEntry(dispatchOrder, ledgerID, "Order", particular, null, dispatchOrder.Five, gst5LedgerId, taxVoucherTypeId, vch.ID);
                                entry.Date = dispatchOrder.Deliverydate;
                                result.Add(entry);

                                particular = "GST5 debited regarding Order " + dispatchOrder.OrderId;
                                entry = CreateLedgerEntry(dispatchOrder, gst5LedgerId, "Order", particular, dispatchOrder.Five, null, ledgerID, taxVoucherTypeId, vch.ID);
                                entry.Date = dispatchOrder.Deliverydate;
                                result.Add(entry);
                            }

                            if (dispatchOrder.OneTwo != null && dispatchOrder.OneTwo > 0)
                            {
                                particular = "GST12 credited regarding Order " + dispatchOrder.OrderId;
                                entry = CreateLedgerEntry(dispatchOrder, ledgerID, "Order", particular, null, dispatchOrder.OneTwo, gst12LedgerId, taxVoucherTypeId, vch.ID);
                                entry.Date = dispatchOrder.Deliverydate;
                                result.Add(entry);

                                particular = "GST12 debited regarding Order " + dispatchOrder.OrderId;
                                entry = CreateLedgerEntry(dispatchOrder, gst12LedgerId, "Order", particular, dispatchOrder.OneTwo, null, ledgerID, taxVoucherTypeId, vch.ID);
                                entry.Date = dispatchOrder.Deliverydate;
                                result.Add(entry);
                            }

                            if (dispatchOrder.OneFour != null && dispatchOrder.OneFour > 0)
                            {
                                particular = "GST14 credited regarding Order " + dispatchOrder.OrderId;
                                entry = CreateLedgerEntry(dispatchOrder, ledgerID, "Order", particular, null, dispatchOrder.OneFour, gst14LedgerId, taxVoucherTypeId, vch.ID);
                                entry.Date = dispatchOrder.Deliverydate;
                                result.Add(entry);

                                particular = "GST14 debited regarding Order " + dispatchOrder.OrderId;
                                entry = CreateLedgerEntry(dispatchOrder, gst14LedgerId, "Order", particular, dispatchOrder.OneFour, null, ledgerID, taxVoucherTypeId, vch.ID);
                                entry.Date = dispatchOrder.Deliverydate;
                                result.Add(entry);
                            }

                            if (dispatchOrder.OneEight != null && dispatchOrder.OneEight > 0)
                            {
                                particular = "GST18 credited regarding Order " + dispatchOrder.OrderId;
                                entry = CreateLedgerEntry(dispatchOrder, ledgerID, "Order", particular, null, dispatchOrder.OneEight, gst18LedgerId, taxVoucherTypeId, vch.ID);
                                entry.Date = dispatchOrder.Deliverydate;
                                result.Add(entry);

                                particular = "GST18 debited regarding Order " + dispatchOrder.OrderId;
                                entry = CreateLedgerEntry(dispatchOrder, gst18LedgerId, "Order", particular, dispatchOrder.OneEight, null, ledgerID, taxVoucherTypeId, vch.ID);
                                entry.Date = dispatchOrder.Deliverydate;
                                result.Add(entry);
                            }

                            if (dispatchOrder.TwoEight != null && dispatchOrder.TwoEight > 0)
                            {
                                particular = "GST28 credited regarding Order " + dispatchOrder.OrderId;
                                entry = CreateLedgerEntry(dispatchOrder, ledgerID, "Order", particular, null, dispatchOrder.TwoEight, gst28LedgerId, taxVoucherTypeId, vch.ID);
                                entry.Date = dispatchOrder.Deliverydate;
                                result.Add(entry);

                                particular = "GST28 debited regarding Order " + dispatchOrder.OrderId;
                                entry = CreateLedgerEntry(dispatchOrder, gst28LedgerId, "Order", particular, dispatchOrder.TwoEight, null, ledgerID, taxVoucherTypeId, vch.ID);
                                entry.Date = dispatchOrder.Deliverydate;
                                result.Add(entry);
                            }

                            if (dispatchOrder.CessTaxAmount != null && dispatchOrder.CessTaxAmount > 0)
                            {
                                particular = "Cess Tax credited regarding Order " + dispatchOrder.OrderId;
                                entry = CreateLedgerEntry(dispatchOrder, ledgerID, "Order", particular, null, dispatchOrder.CessTaxAmount, cessTaxLedgerId, taxVoucherTypeId, vch.ID);
                                entry.Date = dispatchOrder.Deliverydate;
                                result.Add(entry);

                                particular = "Cess Tax debited regarding Order " + dispatchOrder.OrderId;
                                entry = CreateLedgerEntry(dispatchOrder, cessTaxLedgerId, "Order", particular, dispatchOrder.CessTaxAmount, null, ledgerID, taxVoucherTypeId, vch.ID);
                                entry.Date = dispatchOrder.Deliverydate;
                                result.Add(entry);
                            }

                        }

                    }


                    if (result != null && result.Count > 0)
                    {
                        authContext.LadgerEntryDB.AddRange(result);
                        authContext.Commit();

                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error("Error In  CustomerLedgerHelperAuto controller in  OnDispatch Method: " + ex.ToString());
            }
        }

        public void OnChange(int orderID, int userID)
        {
            if (AppConstants.IsUsingAutoLedger)
            {
                return;
            }
            using (var context = new AuthContext())
            {
                int customerId = context.DbOrderMaster.FirstOrDefault(x => x.OrderId == orderID).CustomerId;
                context.Database.ExecuteSqlCommand("delete from LadgerEntries where ObjectID = " + orderID + " and ObjectType = 'Order'");
                OnDispatch(orderID, customerId, userID);
            }
        }

        public void OnSettle(int orderID, int customerId, int userID)
        {
            if (AppConstants.IsUsingAutoLedger)
            {
                return;
            }
            using (var authContext = new AuthContext())
            {
                var query = from l in authContext.LadgerDB
                            join lt in authContext.LadgerTypeDB
                            on l.LadgertypeID equals lt.ID
                            where (lt.code.ToLower() == "customer" && l.ObjectID == customerId)
                                || lt.code.ToLower() == "wallet"
                                || lt.code.ToLower() == "deliverycharges"
                                || lt.code.ToLower() == "cash"
                                || lt.code.ToLower() == "electronic"
                                || lt.code.ToLower() == "bank"
                                || lt.code.ToLower() == "mpos"
                                || lt.code.ToLower() == "billdiscount"
                                || lt.code.ToLower() == "postdiscount"
                            select l;

                var ledgerList = query.ToList();

                SqlParameter orderIdParam = null;
                orderIdParam = new SqlParameter("@OrderID", orderID);
                string spName = "CustomerLedgerEntryUpdateOnSattle";

                List<CustomerLedgerEntryModel> dispatchOrderList = null;

                spName = spName + " @OrderID";
                dispatchOrderList = authContext.Database.SqlQuery<CustomerLedgerEntryModel>(spName, orderIdParam).ToList();


                var ledgerListToSave = new List<LadgerEntry>();
                var voucherTypes = authContext.VoucherTypeDB.Where(x => x.Active).ToList();


                if (dispatchOrderList != null && dispatchOrderList.Count > 0)
                {
                    var result = new List<LadgerEntry>();
                    var walletLedgerId = ledgerList.FirstOrDefault(x => x.LadgertypeID != 1 && !string.IsNullOrEmpty(x.Name) && x.Name.ToLower() == "wallet").ID;
                    var cashLedgerId = ledgerList.FirstOrDefault(x => x.LadgertypeID != 1 && !string.IsNullOrEmpty(x.Name) && x.Name.ToLower() == "cash").ID;
                    var electronicLedgerId = ledgerList.FirstOrDefault(x => x.LadgertypeID != 1 && !string.IsNullOrEmpty(x.Name) && x.Name.ToLower() == "electronic").ID;
                    var bankLedgerId = ledgerList.FirstOrDefault(x => x.LadgertypeID != 1 && !string.IsNullOrEmpty(x.Name) && x.Name.ToLower() == "bank").ID;

                    var billDiscountLedgerId = ledgerList.FirstOrDefault(x => x.LadgertypeID != 1 && !string.IsNullOrEmpty(x.Name) && x.Name.ToLower() == "billdiscount").ID;
                    var postDiscountLedgerId = ledgerList.FirstOrDefault(x => x.LadgertypeID != 1 && !string.IsNullOrEmpty(x.Name) && x.Name.ToLower() == "postdiscount").ID;
                    var mPosLedgerId = ledgerList.FirstOrDefault(x => x.LadgertypeID != 1 && !string.IsNullOrEmpty(x.Name) && x.Name.ToLower() == "mpos").ID;


                    CustomerLedgerHelper customerLedgerHelper = new CustomerLedgerHelper();
                    var hdfcLedgerId = customerLedgerHelper.GetHDFCLedgerID(authContext);
                    var epayLaterId = customerLedgerHelper.GetEpaylaterLedgerID(authContext);
                    var truePayLedgerID = customerLedgerHelper.GetTruePayLedgerID(authContext);


                    var salesVoucherTypeId = voucherTypes.FirstOrDefault(x => x.Name.ToLower() == "sales").ID;
                    var receiptVoucherTypeId = voucherTypes.FirstOrDefault(x => x.Name.ToLower() == "receipt").ID;
                    var journalVoucherTypeId = voucherTypes.FirstOrDefault(x => x.Name.ToLower() == "journal").ID;


                    //foreach (var dispatchOrder in dispatchOrderList)

                    var ordergroup = dispatchOrderList.GroupBy(x => x.OrderId).ToList();

                    foreach (var group in ordergroup)
                    {
                        CustomerLedgerEntryModel dispatchOrder = group.First();
                        Voucher vch = CustomerLedgerHelper.GetOrCreateVoucher(dispatchOrder.InvoiceNo);
                        bool isTakeTransactionFromPaymentResponseRetailerApps = IsTakeTransactionFromPaymentResponseRetailerApps(group);

                        long? ledgerIDNullable = ledgerList.Where(x => x.LadgertypeID == 1 && x.ObjectID == dispatchOrder.CustomerId)?.FirstOrDefault()?.ID;

                        if (!ledgerIDNullable.HasValue)
                        {
                            Ladger le = new Ladger();
                            le.Active = true;
                            le.Address = "";
                            le.Alias = dispatchOrder.Skcode;
                            le.Country = "";
                            le.LadgertypeID = 1;
                            le.Name = dispatchOrder.Skcode;
                            le.ObjectID = dispatchOrder.CustomerId;
                            le.ObjectType = "Customer";
                            le.ProvidedBankDetails = true;
                            le.UpdatedBy = 1;
                            le.UpdatedDate = DateTime.Now;
                            le.CreatedBy = 1;
                            le.CreatedDate = DateTime.Now;
                            authContext.LadgerDB.Add(le);
                            authContext.Commit();

                            ledgerIDNullable = le.ID;
                        }

                        //var list = authContext.LadgerEntryDB.Where(x => x.ObjectID == dispatchOrder.OrderId && x.ObjectType == "Order").ToList();
                        //if (list != null && list.Count > 0
                        //    && (list.Any(x => x.LagerID == ledgerIDNullable.Value && x.Credit > 0))
                        //)
                        //{
                        //    continue;
                        //}


                        else if (ledgerIDNullable.HasValue)
                        {
                            long ledgerID = ledgerIDNullable.Value;
                            String particular = "";
                            LadgerEntry entry = null;



                            if (dispatchOrder.BillDiscountAmount != null && dispatchOrder.BillDiscountAmount > 0)
                            {
                                particular = "Bill Discount Amount credited regarding Order " + dispatchOrder.OrderId;
                                entry = CreateLedgerEntry(dispatchOrder, ledgerID, "Order", particular, dispatchOrder.BillDiscountAmount, null, billDiscountLedgerId, journalVoucherTypeId, vch.ID);
                                entry.Date = dispatchOrder.Deliverydate;
                                result.Add(entry);

                                particular = "Bill Discount Amount debited regarding Order " + dispatchOrder.OrderId;
                                entry = CreateLedgerEntry(dispatchOrder, billDiscountLedgerId, "Order", particular, null, dispatchOrder.BillDiscountAmount, ledgerID, journalVoucherTypeId, vch.ID);
                                entry.Date = dispatchOrder.Deliverydate;
                                result.Add(entry);
                            }

                            CashEntry(result, group, ledgerID, cashLedgerId, receiptVoucherTypeId, isTakeTransactionFromPaymentResponseRetailerApps);

                            chequeEntry(result, group, ledgerID, bankLedgerId, receiptVoucherTypeId, isTakeTransactionFromPaymentResponseRetailerApps);


                            HDFCEntry(result, group, ledgerID, hdfcLedgerId, receiptVoucherTypeId, isTakeTransactionFromPaymentResponseRetailerApps);

                            EpaylaterEntry(result, group, ledgerID, epayLaterId, receiptVoucherTypeId, isTakeTransactionFromPaymentResponseRetailerApps);

                            TruePayEntry(result, group, ledgerID, truePayLedgerID, receiptVoucherTypeId, isTakeTransactionFromPaymentResponseRetailerApps);


                            //ElectronicEntry(result, group, ledgerID, electronicLedgerId, receiptVoucherTypeId, isTakeTransactionFromPaymentResponseRetailerApps);

                            MPosEntry(result, group, ledgerID, mPosLedgerId, receiptVoucherTypeId, isTakeTransactionFromPaymentResponseRetailerApps);

                            if (dispatchOrder.DiscountAmount != null && dispatchOrder.DiscountAmount > 0)
                            {
                                particular = "DiscountAmount credited regarding Order " + dispatchOrder.OrderId;
                                entry = CreateLedgerEntry(dispatchOrder, ledgerID, "Order", particular, dispatchOrder.DiscountAmount, null, postDiscountLedgerId, journalVoucherTypeId, vch.ID);
                                entry.Date = dispatchOrder.Deliverydate;
                                result.Add(entry);

                                particular = "DiscountAmount debited regarding Order " + dispatchOrder.OrderId;
                                entry = CreateLedgerEntry(dispatchOrder, postDiscountLedgerId, "Order", particular, null, dispatchOrder.DiscountAmount, ledgerID, journalVoucherTypeId, vch.ID);
                                entry.Date = dispatchOrder.Deliverydate;
                                result.Add(entry);
                            }

                            if (dispatchOrder.WalletAmount != null && dispatchOrder.WalletAmount > 0)
                            {
                                particular = "wallet point credited regarding Order " + dispatchOrder.OrderId;
                                entry = CreateLedgerEntry(dispatchOrder, ledgerID, "Order", particular, dispatchOrder.WalletAmount, null, walletLedgerId, journalVoucherTypeId, vch.ID);
                                entry.Date = dispatchOrder.Deliverydate;
                                result.Add(entry);

                                particular = "wallet point debited regarding Order " + dispatchOrder.OrderId;
                                entry = CreateLedgerEntry(dispatchOrder, walletLedgerId, "Order", particular, null, dispatchOrder.WalletAmount, ledgerID, journalVoucherTypeId, vch.ID);
                                entry.Date = dispatchOrder.Deliverydate;
                                result.Add(entry);
                            }



                        }

                        authContext.LadgerEntryDB.AddRange(result);
                        authContext.Commit();
                    }
                }
            }
        }

        public void OnCancel(int orderID, int userID, AuthContext context, int CustomerId)
        {
            if (AppConstants.IsUsingAutoLedger)
            {
                return;
            }

            int customerId = CustomerId;// context.DbOrderMaster.FirstOrDefault(x => x.OrderId == orderID).CustomerId;
            var ledgerEntryList = context.LadgerEntryDB.Where(x => x.ObjectID == orderID && x.ObjectType.ToLower() == "order").ToList();

            if (ledgerEntryList?.Any() == true)
            {
                var voucherTypeList = context.VoucherTypeDB.ToList();
                VoucherType vch = voucherTypeList.Where(x => x.Name.ToLower() == "ordercancel").FirstOrDefault();
                List<LadgerEntry> ladgerEntryList = new List<LadgerEntry>();
                foreach (var ledgerEntry in ledgerEntryList)
                {
                    long number = vch.ID;
                    LadgerEntry ledgerEntryNew = CreateRevertLadgerEntry(ledgerEntry.LagerID, ledgerEntry.AffectedLadgerID, userID, ledgerEntry.Debit, ledgerEntry.Credit, ledgerEntry.ObjectID, ledgerEntry.ObjectType, ledgerEntry.Particulars, ledgerEntry.VouchersNo, vch.ID);
                    ladgerEntryList.Add(ledgerEntryNew);
                }

                context.LadgerEntryDB.AddRange(ladgerEntryList);
                context.Commit();
            }

        }


        private static bool IsTakeTransactionFromPaymentResponseRetailerApps(IGrouping<int, CustomerLedgerEntryModel> group)
        {
            return group.Any(x => x.Amount.HasValue && x.Amount.Value > 0);
        }

        private static LadgerEntry CreateLedgerEntry(CustomerLedgerEntryModel customerLedger, long ledgerID, string objectType, string Particulars, double? credit, double? debit, long? affectedLedgerID, long? voucherTypeID, long? voucherNo = null)
        {

            LadgerEntry entry = new LadgerEntry();
            entry.LagerID = ledgerID;
            entry.ObjectID = customerLedger.OrderId;
            entry.ObjectType = objectType;
            entry.Particulars = Particulars;
            entry.Credit = credit;
            entry.Debit = debit;
            entry.AffectedLadgerID = affectedLedgerID;
            entry.VouchersTypeID = voucherTypeID;
            entry.CreatedBy = 9999;
            entry.CreatedDate = DateTime.Now;
            entry.UpdatedBy = 9999;
            entry.UpdatedDate = DateTime.Now;
            entry.Active = true;
            entry.VouchersNo = voucherNo != null ? voucherNo : null;
            return entry;
        }


        private static void CashEntry(List<LadgerEntry> result, IGrouping<int, CustomerLedgerEntryModel> group, long ledgerID, long cashLedgerId, long? receiptVoucherTypeId, Boolean isTakeTransactionFromPaymentResponseRetailerApps)
        {
            CustomerLedgerEntryModel cashDispatchOrder = null;
            double? amount = null;
            if (isTakeTransactionFromPaymentResponseRetailerApps)
            {
                cashDispatchOrder = group.FirstOrDefault(x => (!string.IsNullOrEmpty(x.PaymentFrom) && x.PaymentFrom.ToLower() == "cash"));
            }
            else
            {
                cashDispatchOrder = group.FirstOrDefault(x => x.CashAmount > 0);
            }

            if (cashDispatchOrder != null)
            {
                amount = isTakeTransactionFromPaymentResponseRetailerApps ? cashDispatchOrder.Amount : cashDispatchOrder.CashAmount;

                if (amount.HasValue && amount.Value > 0)
                {
                    string particular = "Cash payment regarding Order " + cashDispatchOrder.OrderId;
                    LadgerEntry entry = CreateLedgerEntry(cashDispatchOrder, ledgerID, "Order", particular, amount.Value, null, cashLedgerId, receiptVoucherTypeId);
                    entry.Date = cashDispatchOrder.Deliverydate;
                    result.Add(entry);

                    particular = "Cash get regarding Order " + cashDispatchOrder.OrderId;
                    entry = CreateLedgerEntry(cashDispatchOrder, cashLedgerId, "Order", particular, null, amount.Value, ledgerID, receiptVoucherTypeId);
                    entry.Date = cashDispatchOrder.Deliverydate;
                    result.Add(entry);
                }

            }
        }
        private static void chequeEntry(List<LadgerEntry> result, IGrouping<int, CustomerLedgerEntryModel> group, long ledgerID, long bankLedgerId, long? receiptVoucherTypeId, Boolean isTakeTransactionFromPaymentResponseRetailerApps)
        {
            CustomerLedgerEntryModel chequeDispatchOrder = null;
            if (isTakeTransactionFromPaymentResponseRetailerApps)
            {
                chequeDispatchOrder = group.FirstOrDefault(x => (!string.IsNullOrEmpty(x.PaymentFrom) && x.PaymentFrom.ToLower() == "cheque"));
            }
            else
            {
                chequeDispatchOrder = group.FirstOrDefault(x => x.CheckAmount > 0);
            }

            if (chequeDispatchOrder != null)
            {

                double? amount = isTakeTransactionFromPaymentResponseRetailerApps ? chequeDispatchOrder.Amount : chequeDispatchOrder.CheckAmount;

                if (amount.HasValue && amount.Value > 0)
                {
                    string vchCode = (!string.IsNullOrEmpty(chequeDispatchOrder.Gatewaytransid) ? chequeDispatchOrder.Gatewaytransid : chequeDispatchOrder.CheckNo);
                    Voucher vch = CustomerLedgerHelper.SaveVoucher(vchCode, 9999);


                    string particular = "Cheque payment by cheque number " + (!string.IsNullOrEmpty(chequeDispatchOrder.Gatewaytransid) ? chequeDispatchOrder.Gatewaytransid : chequeDispatchOrder.CheckNo) + " regarding Order " + chequeDispatchOrder.OrderId;
                    LadgerEntry entry = CreateLedgerEntry(chequeDispatchOrder, ledgerID, "Order", particular, amount.Value, null, bankLedgerId, receiptVoucherTypeId, vch.ID);
                    entry.Date = chequeDispatchOrder.Deliverydate;
                    result.Add(entry);

                    particular = "Cheque get by cheque number " + (!string.IsNullOrEmpty(chequeDispatchOrder.Gatewaytransid) ? chequeDispatchOrder.Gatewaytransid : chequeDispatchOrder.CheckNo) + " regarding Order " + chequeDispatchOrder.OrderId;
                    entry = CreateLedgerEntry(chequeDispatchOrder, bankLedgerId, "Order", particular, null, amount.Value, ledgerID, receiptVoucherTypeId, vch.ID);
                    entry.Date = chequeDispatchOrder.Deliverydate;
                    result.Add(entry);
                }
            }
        }
        private static void ElectronicEntry(List<LadgerEntry> result, IGrouping<int, CustomerLedgerEntryModel> group, long ledgerID, long electronicLedgerId, long? receiptVoucherTypeId, Boolean isTakeTransactionFromPaymentResponseRetailerApps)
        {
            CustomerLedgerEntryModel electronicDispatchOrder = null;
            if (isTakeTransactionFromPaymentResponseRetailerApps)
            {
                electronicDispatchOrder = group.Where(x => (!string.IsNullOrEmpty(x.PaymentFrom) && (x.PaymentFrom.ToLower() == "epaylater" || x.PaymentFrom.ToLower() == "hdfc" || x.PaymentFrom.ToLower() == "truepay" || x.PaymentFrom.ToLower() == "trupay"))).FirstOrDefault();
            }
            else
            {
                electronicDispatchOrder = group.Where(x => x.ElectronicAmount > 0).FirstOrDefault();
            }

            if (electronicDispatchOrder != null)
            {
                double? amount = isTakeTransactionFromPaymentResponseRetailerApps ? electronicDispatchOrder.Amount : electronicDispatchOrder.ElectronicAmount;

                if (amount.HasValue && amount.Value > 0)
                {
                    string vchCode = (!string.IsNullOrEmpty(electronicDispatchOrder.Gatewaytransid) ? electronicDispatchOrder.Gatewaytransid : electronicDispatchOrder.CheckNo);
                    Voucher vch = CustomerLedgerHelper.SaveVoucher(vchCode, 9999);


                    string particular = "Electronic payment by electronic payment number " + (!string.IsNullOrEmpty(electronicDispatchOrder.Gatewaytransid) ? electronicDispatchOrder.Gatewaytransid : electronicDispatchOrder.ElectronicPaymentNo) + "regarding Order " + electronicDispatchOrder.OrderId;
                    LadgerEntry entry = CreateLedgerEntry(electronicDispatchOrder, ledgerID, "Order", particular, amount.Value, null, electronicLedgerId, receiptVoucherTypeId, vch.ID);
                    entry.Date = electronicDispatchOrder.Deliverydate;
                    result.Add(entry);

                    particular = "Electronic get payment number " + (!string.IsNullOrEmpty(electronicDispatchOrder.Gatewaytransid) ? electronicDispatchOrder.Gatewaytransid : electronicDispatchOrder.ElectronicPaymentNo) + "regarding Order " + electronicDispatchOrder.OrderId;
                    entry = CreateLedgerEntry(electronicDispatchOrder, electronicLedgerId, "Order", particular, null, amount.Value, ledgerID, receiptVoucherTypeId, vch.ID);
                    entry.Date = electronicDispatchOrder.Deliverydate;
                    result.Add(entry);
                }
            }
        }
        private static void MPosEntry(List<LadgerEntry> result, IGrouping<int, CustomerLedgerEntryModel> group, long ledgerID, long mPosLedgerId, long? receiptVoucherTypeId, Boolean isTakeTransactionFromPaymentResponseRetailerApps)
        {
            CustomerLedgerEntryModel mposDispatchOrder = null;
            if (isTakeTransactionFromPaymentResponseRetailerApps)
            {
                mposDispatchOrder = group.Where(x => (!string.IsNullOrEmpty(x.PaymentFrom) && (x.PaymentFrom.ToLower() == "mpos"))).FirstOrDefault();
            }
            if (mposDispatchOrder != null)
            {
                double? amount = isTakeTransactionFromPaymentResponseRetailerApps ? mposDispatchOrder.Amount : 0;

                if (amount.HasValue && amount.Value > 0)
                {
                    string vchCode = (!string.IsNullOrEmpty(mposDispatchOrder.Gatewaytransid) ? mposDispatchOrder.Gatewaytransid : mposDispatchOrder.CheckNo);
                    Voucher vch = CustomerLedgerHelper.SaveVoucher(vchCode, 9999);

                    string particular = "mpos payment  number " + (!string.IsNullOrEmpty(mposDispatchOrder.Gatewaytransid) ? mposDispatchOrder.Gatewaytransid : " ") + "regarding Order " + mposDispatchOrder.OrderId;
                    LadgerEntry entry = CreateLedgerEntry(mposDispatchOrder, ledgerID, "Order", particular, amount.Value, null, mPosLedgerId, receiptVoucherTypeId, vch.ID);
                    entry.Date = mposDispatchOrder.Deliverydate;
                    result.Add(entry);

                    particular = "mpos payment  number  " + (!string.IsNullOrEmpty(mposDispatchOrder.Gatewaytransid) ? mposDispatchOrder.Gatewaytransid : "") + "regarding Order " + mposDispatchOrder.OrderId;
                    entry = CreateLedgerEntry(mposDispatchOrder, mPosLedgerId, "Order", particular, null, amount.Value, ledgerID, receiptVoucherTypeId, vch.ID);
                    entry.Date = mposDispatchOrder.Deliverydate;
                    result.Add(entry);
                }

            }
        }
        private static LadgerEntry CreateRevertLadgerEntry(long? ladgerID, long? affectedLadgerID, int userID, double? credit, double? debit, long? objectID, string objectType, string particulars, long? vouchersNo, long? vouchersTypeID)
        {
            LadgerEntry le = new LadgerEntry();
            le.LagerID = ladgerID;
            le.AffectedLadgerID = affectedLadgerID;
            le.Active = true;
            le.CreatedBy = userID;
            le.Date = DateTime.Now;
            le.CreatedDate = DateTime.Now;
            le.UpdatedBy = userID;
            le.UpdatedDate = DateTime.Now;
            le.Credit = credit;
            le.Debit = debit;
            le.ObjectID = objectID;
            le.ObjectType = objectType;
            le.Particulars = particulars;
            le.VouchersNo = vouchersNo;
            le.VouchersTypeID = vouchersTypeID;
            return le;
        }

        private static void EpaylaterEntry(List<LadgerEntry> result, IGrouping<int, CustomerLedgerEntryModel> group, long ledgerID, long epayLaterLedgerId, long? receiptVoucherTypeId, Boolean isTakeTransactionFromPaymentResponseRetailerApps)
        {
            CustomerLedgerEntryModel electronicDispatchOrder = null;
            if (isTakeTransactionFromPaymentResponseRetailerApps)
            {
                electronicDispatchOrder = group.Where(x => (!string.IsNullOrEmpty(x.PaymentFrom) && (x.PaymentFrom.ToLower() == "epaylater"))).FirstOrDefault();
            }
            else
            {
                electronicDispatchOrder = group.Where(x => x.ElectronicAmount > 0).FirstOrDefault();
            }

            if (electronicDispatchOrder != null)
            {
                double? amount = isTakeTransactionFromPaymentResponseRetailerApps ? electronicDispatchOrder.Amount : electronicDispatchOrder.ElectronicAmount;

                if (amount.HasValue && amount.Value > 0)
                {
                    string vchCode = (!string.IsNullOrEmpty(electronicDispatchOrder.Gatewaytransid) ? electronicDispatchOrder.Gatewaytransid : electronicDispatchOrder.ElectronicPaymentNo);
                    Voucher vch = SaveVoucher(vchCode, 9999);

                    string particular = "Epaylater payment by electronic payment number " + (!string.IsNullOrEmpty(electronicDispatchOrder.Gatewaytransid) ? electronicDispatchOrder.Gatewaytransid : electronicDispatchOrder.ElectronicPaymentNo) + "regarding Order " + electronicDispatchOrder.OrderId;
                    LadgerEntry entry = CreateLadgerEntry(electronicDispatchOrder, ledgerID, "Order", particular, amount.Value, null, epayLaterLedgerId, receiptVoucherTypeId, vch.ID);
                    entry.Date = electronicDispatchOrder.Deliverydate;
                    result.Add(entry);

                    particular = "Epaylater get payment number " + (!string.IsNullOrEmpty(electronicDispatchOrder.Gatewaytransid) ? electronicDispatchOrder.Gatewaytransid : electronicDispatchOrder.ElectronicPaymentNo) + "regarding Order " + electronicDispatchOrder.OrderId;
                    entry = CreateLadgerEntry(electronicDispatchOrder, epayLaterLedgerId, "Order", particular, null, amount.Value, ledgerID, receiptVoucherTypeId, vch.ID);
                    entry.Date = electronicDispatchOrder.Deliverydate;
                    result.Add(entry);
                }
            }
        }

        private static void HDFCEntry(List<LadgerEntry> result, IGrouping<int, CustomerLedgerEntryModel> group, long ledgerID, long hdfcLedgerId, long? receiptVoucherTypeId, Boolean isTakeTransactionFromPaymentResponseRetailerApps)
        {
            CustomerLedgerEntryModel electronicDispatchOrder = null;
            if (isTakeTransactionFromPaymentResponseRetailerApps)
            {
                electronicDispatchOrder = group.Where(x => (!string.IsNullOrEmpty(x.PaymentFrom) && (x.PaymentFrom.ToLower() == "hdfc"))).FirstOrDefault();
            }
            else
            {
                electronicDispatchOrder = group.Where(x => x.ElectronicAmount > 0).FirstOrDefault();
            }

            if (electronicDispatchOrder != null)
            {
                double? amount = isTakeTransactionFromPaymentResponseRetailerApps ? electronicDispatchOrder.Amount : electronicDispatchOrder.ElectronicAmount;

                if (amount.HasValue && amount.Value > 0)
                {
                    string vchCode = (!string.IsNullOrEmpty(electronicDispatchOrder.Gatewaytransid) ? electronicDispatchOrder.Gatewaytransid : electronicDispatchOrder.ElectronicPaymentNo);
                    Voucher vch = SaveVoucher(vchCode, 9999);

                    string particular = "HDFC payment by electronic payment number " + (!string.IsNullOrEmpty(electronicDispatchOrder.Gatewaytransid) ? electronicDispatchOrder.Gatewaytransid : electronicDispatchOrder.ElectronicPaymentNo) + "regarding Order " + electronicDispatchOrder.OrderId;
                    LadgerEntry entry = CreateLadgerEntry(electronicDispatchOrder, ledgerID, "Order", particular, amount.Value, null, hdfcLedgerId, receiptVoucherTypeId, vch.ID);
                    entry.Date = electronicDispatchOrder.Deliverydate;
                    result.Add(entry);

                    particular = "HDFC get payment number " + (!string.IsNullOrEmpty(electronicDispatchOrder.Gatewaytransid) ? electronicDispatchOrder.Gatewaytransid : electronicDispatchOrder.ElectronicPaymentNo) + "regarding Order " + electronicDispatchOrder.OrderId;
                    entry = CreateLadgerEntry(electronicDispatchOrder, hdfcLedgerId, "Order", particular, null, amount.Value, ledgerID, receiptVoucherTypeId, vch.ID);
                    entry.Date = electronicDispatchOrder.Deliverydate;
                    result.Add(entry);
                }
            }
        }

        private static void TruePayEntry(List<LadgerEntry> result, IGrouping<int, CustomerLedgerEntryModel> group, long ledgerID, long truepayLedgerId, long? receiptVoucherTypeId, Boolean isTakeTransactionFromPaymentResponseRetailerApps)
        {
            CustomerLedgerEntryModel electronicDispatchOrder = null;
            if (isTakeTransactionFromPaymentResponseRetailerApps)
            {
                electronicDispatchOrder = group.Where(x => (!string.IsNullOrEmpty(x.PaymentFrom) && (x.PaymentFrom.ToLower() == "truepay" || x.PaymentFrom.ToLower() == "trupay"))).FirstOrDefault();
            }
            else
            {
                electronicDispatchOrder = group.Where(x => x.ElectronicAmount > 0).FirstOrDefault();
            }

            if (electronicDispatchOrder != null)
            {
                double? amount = isTakeTransactionFromPaymentResponseRetailerApps ? electronicDispatchOrder.Amount : electronicDispatchOrder.ElectronicAmount;

                if (amount.HasValue && amount.Value > 0)
                {
                    string vchCode = (!string.IsNullOrEmpty(electronicDispatchOrder.Gatewaytransid) ? electronicDispatchOrder.Gatewaytransid : electronicDispatchOrder.ElectronicPaymentNo);
                    Voucher vch = SaveVoucher(vchCode, 9999);

                    string particular = "TruePay payment by electronic payment number " + (!string.IsNullOrEmpty(electronicDispatchOrder.Gatewaytransid) ? electronicDispatchOrder.Gatewaytransid : electronicDispatchOrder.ElectronicPaymentNo) + "regarding Order " + electronicDispatchOrder.OrderId;
                    LadgerEntry entry = CreateLadgerEntry(electronicDispatchOrder, ledgerID, "Order", particular, amount.Value, null, truepayLedgerId, receiptVoucherTypeId, vch.ID);
                    entry.Date = electronicDispatchOrder.Deliverydate;
                    result.Add(entry);

                    particular = "TruePay get payment number " + (!string.IsNullOrEmpty(electronicDispatchOrder.Gatewaytransid) ? electronicDispatchOrder.Gatewaytransid : electronicDispatchOrder.ElectronicPaymentNo) + "regarding Order " + electronicDispatchOrder.OrderId;
                    entry = CreateLadgerEntry(electronicDispatchOrder, truepayLedgerId, "Order", particular, null, amount.Value, ledgerID, receiptVoucherTypeId, vch.ID);
                    entry.Date = electronicDispatchOrder.Deliverydate;
                    result.Add(entry);
                }
            }
        }

        public static Voucher SaveVoucher(string code, int createdBy)
        {
            Voucher vch = new Voucher();
            vch.Active = true;
            vch.Code = code;
            vch.CreatedBy = createdBy;
            vch.CreatedDate = DateTime.Now;
            vch.UpdatedBy = createdBy;
            vch.UpdatedDate = DateTime.Now;

            using (var authContext = new AuthContext())
            {
                authContext.VoucherDB.Add(vch);
                authContext.Commit();
                return vch;
            }
        }


        private static LadgerEntry CreateLadgerEntry(CustomerLedgerEntryModel customerLedger, long ledgerID, string objectType, string Particulars, double? credit, double? debit, long? affectedLedgerID, long? voucherTypeID, long? voucherNumber = null)
        {

            LadgerEntry entry = new LadgerEntry();
            entry.LagerID = ledgerID;
            entry.ObjectID = customerLedger.OrderId;
            entry.ObjectType = objectType;
            entry.Particulars = Particulars;
            entry.Credit = credit;
            entry.Debit = debit;
            entry.AffectedLadgerID = affectedLedgerID;
            entry.VouchersTypeID = voucherTypeID;
            entry.CreatedBy = 9999;
            entry.CreatedDate = DateTime.Now;
            entry.UpdatedBy = 9999;
            entry.UpdatedDate = DateTime.Now;
            entry.Active = true;
            entry.VouchersNo = voucherNumber != null ? voucherNumber : null;
            return entry;
        }



    }
}