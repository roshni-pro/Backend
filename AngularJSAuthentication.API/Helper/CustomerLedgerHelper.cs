using AngularJSAuthentication.API.ControllerV7;
using AngularJSAuthentication.Model;
using AngularJSAuthentication.Model.Account;
using GenricEcommers.Models;
using Microsoft.Reporting.WebForms;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Transactions;
using System.Web;

namespace AngularJSAuthentication.API.NewHelper
{
    public class CustomerLedgerHelper
    {
        //public enum VouchersTypeEnum
        //{
        //    Sales = 4,
        //    Receipt = 5,
        //    Journal = 6,
        //    Tax = 7
        //}
        public static void GetList(int month, int year, int day, bool isDelivered, bool isSattled, int? orderid = null)
        {
            using (var authContext = new AuthContext())
            {
                var ledgerList = authContext.LadgerDB.ToList();
                var monthParam = new SqlParameter("@Month", month);
                var yearParam = new SqlParameter("@Year", year);
                var dayParam = new SqlParameter("@Day", day);
                SqlParameter orderIdParam = null;
                if (orderid.HasValue)
                {
                    orderIdParam = new SqlParameter("@OrderID", orderid);
                }


                string spName = "";
                if (isDelivered)
                {
                    spName = "CustomerLedgerEntryDeliveredUpdate";
                }
                else if (isSattled)
                {
                    spName = "CustomerLedgerEntrySattledUpdate";
                }
                else
                {
                    return;
                }


                List<CustomerLedgerEntryModel> dispatchOrderList = null;
                if (orderid.HasValue)
                {
                    spName = spName + " @Day,@Month,@Year,@OrderID";
                    dispatchOrderList = authContext.Database.SqlQuery<CustomerLedgerEntryModel>(spName, dayParam, monthParam, yearParam, orderIdParam).ToList();
                }
                else
                {
                    spName = spName + " @Day,@Month,@Year";
                    dispatchOrderList = authContext.Database.SqlQuery<CustomerLedgerEntryModel>(spName, dayParam, monthParam, yearParam).ToList();
                }


                var ledgerListToSave = new List<LadgerEntry>();
                var voucherTypes = authContext.VoucherTypeDB.Where(x => x.Active).ToList();


                if (dispatchOrderList != null && dispatchOrderList.Count > 0)
                {

                    var result = new ConcurrentBag<LadgerEntry>();
                    var salesLedgerId = ledgerList.FirstOrDefault(x => x.LadgertypeID != 1 && !string.IsNullOrEmpty(x.Name) && x.Name.ToLower() == "sales").ID;
                    var walletLedgerId = ledgerList.FirstOrDefault(x => x.LadgertypeID != 1 && !string.IsNullOrEmpty(x.Name) && x.Name.ToLower() == "wallet").ID;
                    var gst5LedgerId = ledgerList.FirstOrDefault(x => x.LadgertypeID != 1 && !string.IsNullOrEmpty(x.Name) && x.Name.ToLower() == "gst5").ID;
                    var gst12LedgerId = ledgerList.FirstOrDefault(x => x.LadgertypeID != 1 && !string.IsNullOrEmpty(x.Name) && x.Name.ToLower() == "gst12").ID;
                    var gst14LedgerId = ledgerList.FirstOrDefault(x => x.LadgertypeID != 1 && !string.IsNullOrEmpty(x.Name) && x.Name.ToLower() == "gst14").ID;
                    var gst18LedgerId = ledgerList.FirstOrDefault(x => x.LadgertypeID != 1 && !string.IsNullOrEmpty(x.Name) && x.Name.ToLower() == "gst18").ID;
                    var gst28LedgerId = ledgerList.FirstOrDefault(x => x.LadgertypeID != 1 && !string.IsNullOrEmpty(x.Name) && x.Name.ToLower() == "gst28").ID;
                    var cessTaxLedgerId = ledgerList.FirstOrDefault(x => x.LadgertypeID != 1 && !string.IsNullOrEmpty(x.Name) && x.Name.ToLower() == "cesstax").ID;

                    CustomerLedgerHelper customerLedgerHelper = new CustomerLedgerHelper();
                    var hdfcLedgerId = customerLedgerHelper.GetHDFCLedgerID(authContext);
                    var epayLaterId = customerLedgerHelper.GetEpaylaterLedgerID(authContext);
                    var truePayLedgerID = customerLedgerHelper.GetTruePayLedgerID(authContext);


                    var cashLedgerId = ledgerList.FirstOrDefault(x => x.LadgertypeID != 1 && !string.IsNullOrEmpty(x.Name) && x.Name.ToLower() == "cash").ID;
                    var electronicLedgerId = ledgerList.FirstOrDefault(x => x.LadgertypeID != 1 && !string.IsNullOrEmpty(x.Name) && x.Name.ToLower() == "electronic").ID;
                    var bankLedgerId = ledgerList.FirstOrDefault(x => x.LadgertypeID != 1 && !string.IsNullOrEmpty(x.Name) && x.Name.ToLower() == "bank").ID;

                    var billDiscountLedgerId = ledgerList.FirstOrDefault(x => x.LadgertypeID != 1 && !string.IsNullOrEmpty(x.Name) && x.Name.ToLower() == "billdiscount").ID;
                    var postDiscountLedgerId = ledgerList.FirstOrDefault(x => x.LadgertypeID != 1 && !string.IsNullOrEmpty(x.Name) && x.Name.ToLower() == "postdiscount").ID;
                    var mPosLedgerId = ledgerList.FirstOrDefault(x => x.LadgertypeID != 1 && !string.IsNullOrEmpty(x.Name) && x.Name.ToLower() == "mpos").ID;
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
                        Voucher vch = GetOrCreateVoucher(dispatchOrder.InvoiceNo);

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

                        var list = authContext.LadgerEntryDB.Where(x => x.ObjectID == dispatchOrder.OrderId && x.ObjectType == "Order").ToList();
                        if (list != null && list.Count > 0
                            && ((list.Any(x => x.LagerID == salesLedgerId && x.Debit > 0) && isDelivered)
                                        || (list.Any(x => x.LagerID == ledgerIDNullable.Value && x.Credit > 0) && isSattled))
                        )
                        {
                            continue;
                        }


                        else if (ledgerIDNullable.HasValue)
                        {
                            long ledgerID = ledgerIDNullable.Value;
                            String particular = "";
                            LadgerEntry entry = null;

                            if (isDelivered)
                            {
                                if (dispatchOrder.TotalAmount > 0)
                                {
                                    particular = "Order Placed for order " + dispatchOrder.OrderId;
                                    double customerAmount = (dispatchOrder.TotalAmount ?? 0) - (dispatchOrder.DeliveryCharge ?? 0) - (dispatchOrder.TaxAmount ?? 0) + (dispatchOrder.BillDiscountAmount ?? 0) + (dispatchOrder.WalletAmount ?? 0);
                                    entry = CreateLadgerEntry(dispatchOrder, ledgerID, "Order", particular, null, customerAmount, salesLedgerId, salesVoucherTypeId, vch.ID);
                                    entry.Date = dispatchOrder.Deliverydate;
                                    result.Add(entry);

                                    double salesAmount = (dispatchOrder.TotalAmount ?? 0) - (dispatchOrder.DeliveryCharge ?? 0) - (dispatchOrder.TaxAmount ?? 0) + (dispatchOrder.BillDiscountAmount ?? 0) + (dispatchOrder.WalletAmount ?? 0);
                                    particular = "Sales for order " + dispatchOrder.OrderId;
                                    entry = CreateLadgerEntry(dispatchOrder, salesLedgerId, "Order", particular, salesAmount, null, ledgerID, salesVoucherTypeId, vch.ID);
                                    entry.Date = dispatchOrder.Deliverydate;
                                    //authContext.LadgerEntryDB.Add(entry);
                                    result.Add(entry);
                                }

                                if (dispatchOrder.DeliveryCharge != null && dispatchOrder.DeliveryCharge > 0)
                                {
                                    particular = "Delivery charge " + dispatchOrder.DeliveryCharge + " regarding Order " + dispatchOrder.OrderId;
                                    entry = CreateLadgerEntry(dispatchOrder, ledgerID, "Order", particular, null, dispatchOrder.DeliveryCharge, deliveryChargesLedgerId, journalVoucherTypeId, vch.ID);
                                    entry.Date = dispatchOrder.Deliverydate;
                                    result.Add(entry);

                                    particular = "Delivery charge " + dispatchOrder.DeliveryCharge + " debited regarding Order " + dispatchOrder.OrderId;
                                    entry = CreateLadgerEntry(dispatchOrder, deliveryChargesLedgerId, "Order", particular, dispatchOrder.DeliveryCharge, null, ledgerID, journalVoucherTypeId, vch.ID);
                                    entry.Date = dispatchOrder.Deliverydate;
                                    result.Add(entry);
                                }

                                if (dispatchOrder.Five != null && dispatchOrder.Five > 0)
                                {
                                    particular = "GST5 credited regarding Order " + dispatchOrder.OrderId;
                                    entry = CreateLadgerEntry(dispatchOrder, ledgerID, "Order", particular, null, dispatchOrder.Five, gst5LedgerId, taxVoucherTypeId, vch.ID);
                                    entry.Date = dispatchOrder.Deliverydate;
                                    result.Add(entry);

                                    particular = "GST5 debited regarding Order " + dispatchOrder.OrderId;
                                    entry = CreateLadgerEntry(dispatchOrder, gst5LedgerId, "Order", particular, dispatchOrder.Five, null, ledgerID, taxVoucherTypeId, vch.ID);
                                    entry.Date = dispatchOrder.Deliverydate;
                                    result.Add(entry);
                                }

                                if (dispatchOrder.OneTwo != null && dispatchOrder.OneTwo > 0)
                                {
                                    particular = "GST12 credited regarding Order " + dispatchOrder.OrderId;
                                    entry = CreateLadgerEntry(dispatchOrder, ledgerID, "Order", particular, null, dispatchOrder.OneTwo, gst12LedgerId, taxVoucherTypeId, vch.ID);
                                    entry.Date = dispatchOrder.Deliverydate;
                                    result.Add(entry);

                                    particular = "GST12 debited regarding Order " + dispatchOrder.OrderId;
                                    entry = CreateLadgerEntry(dispatchOrder, gst12LedgerId, "Order", particular, dispatchOrder.OneTwo, null, ledgerID, taxVoucherTypeId, vch.ID);
                                    entry.Date = dispatchOrder.Deliverydate;
                                    result.Add(entry);
                                }

                                if (dispatchOrder.OneFour != null && dispatchOrder.OneFour > 0)
                                {
                                    particular = "GST14 credited regarding Order " + dispatchOrder.OrderId;
                                    entry = CreateLadgerEntry(dispatchOrder, ledgerID, "Order", particular, null, dispatchOrder.OneFour, gst14LedgerId, taxVoucherTypeId, vch.ID);
                                    entry.Date = dispatchOrder.Deliverydate;
                                    result.Add(entry);

                                    particular = "GST14 debited regarding Order " + dispatchOrder.OrderId;
                                    entry = CreateLadgerEntry(dispatchOrder, gst14LedgerId, "Order", particular, dispatchOrder.OneFour, null, ledgerID, taxVoucherTypeId, vch.ID);
                                    entry.Date = dispatchOrder.Deliverydate;
                                    result.Add(entry);
                                }

                                if (dispatchOrder.OneEight != null && dispatchOrder.OneEight > 0)
                                {
                                    particular = "GST18 credited regarding Order " + dispatchOrder.OrderId;
                                    entry = CreateLadgerEntry(dispatchOrder, ledgerID, "Order", particular, null, dispatchOrder.OneEight, gst18LedgerId, taxVoucherTypeId, vch.ID);
                                    entry.Date = dispatchOrder.Deliverydate;
                                    result.Add(entry);

                                    particular = "GST18 debited regarding Order " + dispatchOrder.OrderId;
                                    entry = CreateLadgerEntry(dispatchOrder, gst18LedgerId, "Order", particular, dispatchOrder.OneEight, null, ledgerID, taxVoucherTypeId, vch.ID);
                                    entry.Date = dispatchOrder.Deliverydate;
                                    result.Add(entry);
                                }

                                if (dispatchOrder.TwoEight != null && dispatchOrder.TwoEight > 0)
                                {
                                    particular = "GST28 credited regarding Order " + dispatchOrder.OrderId;
                                    entry = CreateLadgerEntry(dispatchOrder, ledgerID, "Order", particular, null, dispatchOrder.TwoEight, gst28LedgerId, taxVoucherTypeId, vch.ID);
                                    entry.Date = dispatchOrder.Deliverydate;
                                    result.Add(entry);

                                    particular = "GST28 debited regarding Order " + dispatchOrder.OrderId;
                                    entry = CreateLadgerEntry(dispatchOrder, gst28LedgerId, "Order", particular, dispatchOrder.TwoEight, null, ledgerID, taxVoucherTypeId, vch.ID);
                                    entry.Date = dispatchOrder.Deliverydate;
                                    result.Add(entry);
                                }

                                if (dispatchOrder.CessTaxAmount != null && dispatchOrder.CessTaxAmount > 0)
                                {
                                    particular = "Cess Tax credited regarding Order " + dispatchOrder.OrderId;
                                    entry = CreateLadgerEntry(dispatchOrder, ledgerID, "Order", particular, null, dispatchOrder.CessTaxAmount, cessTaxLedgerId, taxVoucherTypeId, vch.ID);
                                    entry.Date = dispatchOrder.Deliverydate;
                                    result.Add(entry);

                                    particular = "Cess Tax debited regarding Order " + dispatchOrder.OrderId;
                                    entry = CreateLadgerEntry(dispatchOrder, cessTaxLedgerId, "Order", particular, dispatchOrder.CessTaxAmount, null, ledgerID, taxVoucherTypeId, vch.ID);
                                    entry.Date = dispatchOrder.Deliverydate;
                                    result.Add(entry);
                                }

                            }


                            if (isSattled)
                            {

                                if (dispatchOrder.BillDiscountAmount != null && dispatchOrder.BillDiscountAmount > 0)
                                {
                                    particular = "Bill Discount Amount credited regarding Order " + dispatchOrder.OrderId;
                                    entry = CreateLadgerEntry(dispatchOrder, ledgerID, "Order", particular, dispatchOrder.BillDiscountAmount, null, billDiscountLedgerId, journalVoucherTypeId, vch.ID);
                                    entry.Date = dispatchOrder.Deliverydate;
                                    result.Add(entry);

                                    particular = "Bill Discount Amount debited regarding Order " + dispatchOrder.OrderId;
                                    entry = CreateLadgerEntry(dispatchOrder, billDiscountLedgerId, "Order", particular, null, dispatchOrder.BillDiscountAmount, ledgerID, journalVoucherTypeId, vch.ID);
                                    entry.Date = dispatchOrder.Deliverydate;
                                    result.Add(entry);
                                }

                                CashEntry(result, group, ledgerID, cashLedgerId, receiptVoucherTypeId, isTakeTransactionFromPaymentResponseRetailerApps);

                                chequeEntry(result, group, ledgerID, bankLedgerId, receiptVoucherTypeId, isTakeTransactionFromPaymentResponseRetailerApps);

                                //ElectronicEntry(result, group, ledgerID, electronicLedgerId, receiptVoucherTypeId, isTakeTransactionFromPaymentResponseRetailerApps);

                                HDFCEntry(result, group, ledgerID, hdfcLedgerId, receiptVoucherTypeId, isTakeTransactionFromPaymentResponseRetailerApps);

                                EpaylaterEntry(result, group, ledgerID, epayLaterId, receiptVoucherTypeId, isTakeTransactionFromPaymentResponseRetailerApps);

                                TruePayEntry(result, group, ledgerID, truePayLedgerID, receiptVoucherTypeId, isTakeTransactionFromPaymentResponseRetailerApps);


                                MPosEntry(result, group, ledgerID, mPosLedgerId, receiptVoucherTypeId, isTakeTransactionFromPaymentResponseRetailerApps);

                                if (dispatchOrder.DiscountAmount != null && dispatchOrder.DiscountAmount > 0)
                                {
                                    particular = "DiscountAmount credited regarding Order " + dispatchOrder.OrderId;
                                    entry = CreateLadgerEntry(dispatchOrder, ledgerID, "Order", particular, dispatchOrder.DiscountAmount, null, postDiscountLedgerId, journalVoucherTypeId, vch.ID);
                                    entry.Date = dispatchOrder.Deliverydate;
                                    result.Add(entry);

                                    particular = "DiscountAmount debited regarding Order " + dispatchOrder.OrderId;
                                    entry = CreateLadgerEntry(dispatchOrder, postDiscountLedgerId, "Order", particular, null, dispatchOrder.DiscountAmount, ledgerID, journalVoucherTypeId, vch.ID);
                                    entry.Date = dispatchOrder.Deliverydate;
                                    result.Add(entry);
                                }

                                if (dispatchOrder.WalletAmount != null && dispatchOrder.WalletAmount > 0)
                                {
                                    particular = "wallet point credited regarding Order " + dispatchOrder.OrderId;
                                    entry = CreateLadgerEntry(dispatchOrder, ledgerID, "Order", particular, dispatchOrder.WalletAmount, null, walletLedgerId, journalVoucherTypeId, vch.ID);
                                    entry.Date = dispatchOrder.Deliverydate;
                                    result.Add(entry);

                                    particular = "wallet point debited regarding Order " + dispatchOrder.OrderId;
                                    entry = CreateLadgerEntry(dispatchOrder, walletLedgerId, "Order", particular, null, dispatchOrder.WalletAmount, ledgerID, journalVoucherTypeId, vch.ID);
                                    entry.Date = dispatchOrder.Deliverydate;
                                    result.Add(entry);
                                }
                            }


                        }


                    }

                    ledgerListToSave = result.ToList();

                    if (ledgerListToSave != null && ledgerListToSave.Any())
                    {
                        authContext.LadgerEntryDB.AddRange(ledgerListToSave);
                        authContext.Commit();
                    }
                }
            }
        }

        public static void GetListOld(int month, int year, int day, bool isDelivered, bool isSattled)
        {
            using (var authContext = new AuthContext())
            {
                //var ledgerList = authContext.LadgerDB.ToList();
                var monthParam = new SqlParameter("@Month", month);
                var yearParam = new SqlParameter("@Year", year);
                var dayParam = new SqlParameter("@Day", day);


                string spName = "CustomerLedgerEntryUpdate";


                spName = spName + " @Day,@Month,@Year";
                var dispatchOrderList = authContext.Database.SqlQuery<CustomerLedgerEntryModel>(spName, monthParam, yearParam, dayParam).ToList();
                var ledgerListToSave = new List<LadgerEntry>();
                var voucherTypes = authContext.VoucherTypeDB.Where(x => x.Active).ToList();


                if (dispatchOrderList != null && dispatchOrderList.Count > 0)
                {
                    var result = new ConcurrentBag<LadgerEntry>();
                    var salesLedgerId = authContext.LadgerDB.FirstOrDefault(x => x.LadgertypeID != 1 && !string.IsNullOrEmpty(x.Name) && x.Name.ToLower() == "sales").ID;
                    var walletLedgerId = authContext.LadgerDB.FirstOrDefault(x => x.LadgertypeID != 1 && !string.IsNullOrEmpty(x.Name) && x.Name.ToLower() == "wallet").ID;
                    var gst5LedgerId = authContext.LadgerDB.FirstOrDefault(x => x.LadgertypeID != 1 && !string.IsNullOrEmpty(x.Name) && x.Name.ToLower() == "gst5").ID;
                    var gst12LedgerId = authContext.LadgerDB.FirstOrDefault(x => x.LadgertypeID != 1 && !string.IsNullOrEmpty(x.Name) && x.Name.ToLower() == "gst12").ID;
                    var gst14LedgerId = authContext.LadgerDB.FirstOrDefault(x => x.LadgertypeID != 1 && !string.IsNullOrEmpty(x.Name) && x.Name.ToLower() == "gst14").ID;
                    var gst18LedgerId = authContext.LadgerDB.FirstOrDefault(x => x.LadgertypeID != 1 && !string.IsNullOrEmpty(x.Name) && x.Name.ToLower() == "gst18").ID;
                    var gst28LedgerId = authContext.LadgerDB.FirstOrDefault(x => x.LadgertypeID != 1 && !string.IsNullOrEmpty(x.Name) && x.Name.ToLower() == "gst28").ID;
                    var cessTaxLedgerId = authContext.LadgerDB.FirstOrDefault(x => x.LadgertypeID != 1 && !string.IsNullOrEmpty(x.Name) && x.Name.ToLower() == "cesstax").ID;
                    var cashLedgerId = authContext.LadgerDB.FirstOrDefault(x => x.LadgertypeID != 1 && !string.IsNullOrEmpty(x.Name) && x.Name.ToLower() == "cash").ID;
                    var electronicLedgerId = authContext.LadgerDB.FirstOrDefault(x => x.LadgertypeID != 1 && !string.IsNullOrEmpty(x.Name) && x.Name.ToLower() == "electronic").ID;
                    var bankLedgerId = authContext.LadgerDB.FirstOrDefault(x => x.LadgertypeID != 1 && !string.IsNullOrEmpty(x.Name) && x.Name.ToLower() == "bank").ID;
                    var billDiscountLedgerId = authContext.LadgerDB.FirstOrDefault(x => x.LadgertypeID != 1 && !string.IsNullOrEmpty(x.Name) && x.Name.ToLower() == "billdiscount").ID;
                    var postDiscountLedgerId = authContext.LadgerDB.FirstOrDefault(x => x.LadgertypeID != 1 && !string.IsNullOrEmpty(x.Name) && x.Name.ToLower() == "postdiscount").ID;
                    var mPosLedgerId = authContext.LadgerDB.FirstOrDefault(x => x.LadgertypeID != 1 && !string.IsNullOrEmpty(x.Name) && x.Name.ToLower() == "mpos").ID;
                    var deliveryChargesLedgerId = authContext.LadgerDB.FirstOrDefault(x => x.LadgertypeID != 1 && !string.IsNullOrEmpty(x.Name) && x.Name.ToLower() == "deliverycharges").ID;


                    CustomerLedgerHelper customerLedgerHelper = new CustomerLedgerHelper();
                    var hdfcLedgerId = customerLedgerHelper.GetHDFCLedgerID(authContext);
                    var epayLaterId = customerLedgerHelper.GetEpaylaterLedgerID(authContext);
                    var truePayLedgerID = customerLedgerHelper.GetTruePayLedgerID(authContext);

                    var salesVoucherTypeId = voucherTypes.FirstOrDefault(x => x.Name.ToLower() == "sales").ID;
                    var receiptVoucherTypeId = voucherTypes.FirstOrDefault(x => x.Name.ToLower() == "receipt").ID;
                    var journalVoucherTypeId = voucherTypes.FirstOrDefault(x => x.Name.ToLower() == "journal").ID;
                    var taxVoucherTypeId = voucherTypes.FirstOrDefault(x => x.Name.ToLower() == "tax").ID;


                    var ordergroup = dispatchOrderList.GroupBy(x => x.OrderId).ToList();

                    foreach (var group in ordergroup)
                    {
                        CustomerLedgerEntryModel dispatchOrder = group.First();



                        bool isTakeTransactionFromPaymentResponseRetailerApps = IsTakeTransactionFromPaymentResponseRetailerApps(group);

                        long? ledgerIDNullable = authContext.LadgerDB.Where(x => x.LadgertypeID == 1 && x.ObjectID == dispatchOrder.CustomerId)?.FirstOrDefault()?.ID;

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

                        var list = authContext.LadgerEntryDB.Where(x => x.ObjectID == dispatchOrder.OrderId && x.ObjectType == "Order").ToList();
                        if (list != null && list.Count > 0)
                        {
                            continue;
                        }


                        else if (ledgerIDNullable.HasValue)
                        {
                            long ledgerID = ledgerIDNullable.Value;
                            String particular = "";
                            LadgerEntry entry = null;


                            if (dispatchOrder.TotalAmount > 0)
                            {
                                particular = "Order Placed for order " + dispatchOrder.OrderId;
                                double customerAmount = (dispatchOrder.TotalAmount ?? 0) - (dispatchOrder.DeliveryCharge ?? 0) - (dispatchOrder.TaxAmount ?? 0) + (dispatchOrder.BillDiscountAmount ?? 0) + (dispatchOrder.WalletAmount ?? 0);
                                entry = CreateLadgerEntry(dispatchOrder, ledgerID, "Order", particular, null, customerAmount, salesLedgerId, salesVoucherTypeId);
                                entry.Date = dispatchOrder.Deliverydate;
                                result.Add(entry);

                                double salesAmount = (dispatchOrder.TotalAmount ?? 0) - (dispatchOrder.DeliveryCharge ?? 0) - (dispatchOrder.TaxAmount ?? 0) + (dispatchOrder.BillDiscountAmount ?? 0) + (dispatchOrder.WalletAmount ?? 0);
                                particular = "Sales for order " + dispatchOrder.OrderId;
                                entry = CreateLadgerEntry(dispatchOrder, salesLedgerId, "Order", particular, salesAmount, null, ledgerID, salesVoucherTypeId);
                                entry.Date = dispatchOrder.Deliverydate;
                                //authContext.LadgerEntryDB.Add(entry);
                                result.Add(entry);
                            }

                            if (dispatchOrder.DeliveryCharge != null && dispatchOrder.DeliveryCharge > 0)
                            {
                                particular = "Delivery charge " + dispatchOrder.DeliveryCharge + " regarding Order " + dispatchOrder.OrderId;
                                entry = CreateLadgerEntry(dispatchOrder, ledgerID, "Order", particular, null, dispatchOrder.DeliveryCharge, deliveryChargesLedgerId, journalVoucherTypeId);
                                entry.Date = dispatchOrder.Deliverydate;
                                result.Add(entry);

                                particular = "Delivery charge " + dispatchOrder.DeliveryCharge + " debited regarding Order " + dispatchOrder.OrderId;
                                entry = CreateLadgerEntry(dispatchOrder, deliveryChargesLedgerId, "Order", particular, dispatchOrder.DeliveryCharge, null, ledgerID, journalVoucherTypeId);
                                entry.Date = dispatchOrder.Deliverydate;
                                result.Add(entry);
                            }

                            if (dispatchOrder.Five != null && dispatchOrder.Five > 0)
                            {
                                particular = "GST5 credited regarding Order " + dispatchOrder.OrderId;
                                entry = CreateLadgerEntry(dispatchOrder, ledgerID, "Order", particular, null, dispatchOrder.Five, gst5LedgerId, taxVoucherTypeId);
                                entry.Date = dispatchOrder.Deliverydate;
                                result.Add(entry);

                                particular = "GST5 debited regarding Order " + dispatchOrder.OrderId;
                                entry = CreateLadgerEntry(dispatchOrder, gst5LedgerId, "Order", particular, dispatchOrder.Five, null, ledgerID, taxVoucherTypeId);
                                entry.Date = dispatchOrder.Deliverydate;
                                result.Add(entry);
                            }

                            if (dispatchOrder.OneTwo != null && dispatchOrder.OneTwo > 0)
                            {
                                particular = "GST12 credited regarding Order " + dispatchOrder.OrderId;
                                entry = CreateLadgerEntry(dispatchOrder, ledgerID, "Order", particular, null, dispatchOrder.OneTwo, gst12LedgerId, taxVoucherTypeId);
                                entry.Date = dispatchOrder.Deliverydate;
                                result.Add(entry);

                                particular = "GST12 debited regarding Order " + dispatchOrder.OrderId;
                                entry = CreateLadgerEntry(dispatchOrder, gst12LedgerId, "Order", particular, dispatchOrder.OneTwo, null, ledgerID, taxVoucherTypeId);
                                entry.Date = dispatchOrder.Deliverydate;
                                result.Add(entry);
                            }

                            if (dispatchOrder.OneFour != null && dispatchOrder.OneFour > 0)
                            {
                                particular = "GST14 credited regarding Order " + dispatchOrder.OrderId;
                                entry = CreateLadgerEntry(dispatchOrder, ledgerID, "Order", particular, null, dispatchOrder.OneFour, gst14LedgerId, taxVoucherTypeId);
                                entry.Date = dispatchOrder.Deliverydate;
                                result.Add(entry);

                                particular = "GST14 debited regarding Order " + dispatchOrder.OrderId;
                                entry = CreateLadgerEntry(dispatchOrder, gst14LedgerId, "Order", particular, dispatchOrder.OneFour, null, ledgerID, taxVoucherTypeId);
                                entry.Date = dispatchOrder.Deliverydate;
                                result.Add(entry);
                            }

                            if (dispatchOrder.OneEight != null && dispatchOrder.OneEight > 0)
                            {
                                particular = "GST18 credited regarding Order " + dispatchOrder.OrderId;
                                entry = CreateLadgerEntry(dispatchOrder, ledgerID, "Order", particular, null, dispatchOrder.OneEight, gst18LedgerId, taxVoucherTypeId);
                                entry.Date = dispatchOrder.Deliverydate;
                                result.Add(entry);

                                particular = "GST18 debited regarding Order " + dispatchOrder.OrderId;
                                entry = CreateLadgerEntry(dispatchOrder, gst18LedgerId, "Order", particular, dispatchOrder.OneEight, null, ledgerID, taxVoucherTypeId);
                                entry.Date = dispatchOrder.Deliverydate;
                                result.Add(entry);
                            }

                            if (dispatchOrder.TwoEight != null && dispatchOrder.TwoEight > 0)
                            {
                                particular = "GST28 credited regarding Order " + dispatchOrder.OrderId;
                                entry = CreateLadgerEntry(dispatchOrder, ledgerID, "Order", particular, null, dispatchOrder.TwoEight, gst28LedgerId, taxVoucherTypeId);
                                entry.Date = dispatchOrder.Deliverydate;
                                result.Add(entry);

                                particular = "GST28 debited regarding Order " + dispatchOrder.OrderId;
                                entry = CreateLadgerEntry(dispatchOrder, gst28LedgerId, "Order", particular, dispatchOrder.TwoEight, null, ledgerID, taxVoucherTypeId);
                                entry.Date = dispatchOrder.Deliverydate;
                                result.Add(entry);
                            }

                            if (dispatchOrder.CessTaxAmount != null && dispatchOrder.CessTaxAmount > 0)
                            {
                                particular = "Cess Tax credited regarding Order " + dispatchOrder.OrderId;
                                entry = CreateLadgerEntry(dispatchOrder, ledgerID, "Order", particular, null, dispatchOrder.CessTaxAmount, cessTaxLedgerId, taxVoucherTypeId);
                                entry.Date = dispatchOrder.Deliverydate;
                                result.Add(entry);

                                particular = "Cess Tax debited regarding Order " + dispatchOrder.OrderId;
                                entry = CreateLadgerEntry(dispatchOrder, cessTaxLedgerId, "Order", particular, dispatchOrder.CessTaxAmount, null, ledgerID, taxVoucherTypeId);
                                entry.Date = dispatchOrder.Deliverydate;
                                result.Add(entry);
                            }



                            if (dispatchOrder.BillDiscountAmount != null && dispatchOrder.BillDiscountAmount > 0)
                            {
                                particular = "Bill Discount Amount credited regarding Order " + dispatchOrder.OrderId;
                                entry = CreateLadgerEntry(dispatchOrder, ledgerID, "Order", particular, dispatchOrder.BillDiscountAmount, null, billDiscountLedgerId, journalVoucherTypeId);
                                entry.Date = dispatchOrder.Deliverydate;
                                result.Add(entry);

                                particular = "Bill Discount Amount debited regarding Order " + dispatchOrder.OrderId;
                                entry = CreateLadgerEntry(dispatchOrder, billDiscountLedgerId, "Order", particular, null, dispatchOrder.BillDiscountAmount, ledgerID, journalVoucherTypeId);
                                entry.Date = dispatchOrder.Deliverydate;
                                result.Add(entry);
                            }

                            CashEntry(result, group, ledgerID, cashLedgerId, receiptVoucherTypeId, isTakeTransactionFromPaymentResponseRetailerApps);

                            chequeEntry(result, group, ledgerID, bankLedgerId, receiptVoucherTypeId, isTakeTransactionFromPaymentResponseRetailerApps);

                            //ElectronicEntry(result, group, ledgerID, electronicLedgerId, receiptVoucherTypeId, isTakeTransactionFromPaymentResponseRetailerApps);
                            HDFCEntry(result, group, ledgerID, hdfcLedgerId, receiptVoucherTypeId, isTakeTransactionFromPaymentResponseRetailerApps);

                            EpaylaterEntry(result, group, ledgerID, epayLaterId, receiptVoucherTypeId, isTakeTransactionFromPaymentResponseRetailerApps);

                            TruePayEntry(result, group, ledgerID, truePayLedgerID, receiptVoucherTypeId, isTakeTransactionFromPaymentResponseRetailerApps);

                            MPosEntry(result, group, ledgerID, mPosLedgerId, receiptVoucherTypeId, isTakeTransactionFromPaymentResponseRetailerApps);

                            if (dispatchOrder.DiscountAmount != null && dispatchOrder.DiscountAmount > 0)
                            {
                                particular = "DiscountAmount credited regarding Order " + dispatchOrder.OrderId;
                                entry = CreateLadgerEntry(dispatchOrder, ledgerID, "Order", particular, dispatchOrder.DiscountAmount, null, postDiscountLedgerId, journalVoucherTypeId);
                                entry.Date = dispatchOrder.Deliverydate;
                                result.Add(entry);

                                particular = "DiscountAmount debited regarding Order " + dispatchOrder.OrderId;
                                entry = CreateLadgerEntry(dispatchOrder, postDiscountLedgerId, "Order", particular, null, dispatchOrder.DiscountAmount, ledgerID, journalVoucherTypeId);
                                entry.Date = dispatchOrder.Deliverydate;
                                result.Add(entry);
                            }

                            if (dispatchOrder.WalletAmount != null && dispatchOrder.WalletAmount > 0)
                            {
                                particular = "wallet point credited regarding Order " + dispatchOrder.OrderId;
                                entry = CreateLadgerEntry(dispatchOrder, ledgerID, "Order", particular, dispatchOrder.WalletAmount, null, walletLedgerId, journalVoucherTypeId);
                                entry.Date = dispatchOrder.Deliverydate;
                                result.Add(entry);

                                particular = "wallet point debited regarding Order " + dispatchOrder.OrderId;
                                entry = CreateLadgerEntry(dispatchOrder, walletLedgerId, "Order", particular, null, dispatchOrder.WalletAmount, ledgerID, journalVoucherTypeId);
                                entry.Date = dispatchOrder.Deliverydate;
                                result.Add(entry);
                            }



                        }


                    }

                    ledgerListToSave = result.ToList();

                    if (ledgerListToSave != null && ledgerListToSave.Any())
                    {
                        authContext.LadgerEntryDB.AddRange(ledgerListToSave);
                        authContext.Commit();
                    }
                }
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
        public static string GenerateReport(LedgerInputViewModel viewModel)
        {
            string filepath = string.Empty;
            string returnfilepath = string.Empty;
            try
            {
                string path = Path.Combine(HttpContext.Current.Server.MapPath("~/Reports"), "Ledger.rdlc");

                // Variables
                Microsoft.Reporting.WebForms.Warning[] warnings;
                string[] streamIds;
                string mimeType = string.Empty;
                string encoding = string.Empty;
                string extension = string.Empty;

                // Setup the report viewer object and get the array of bytes
                ReportViewer viewer = new ReportViewer();
                viewer.ProcessingMode = ProcessingMode.Local;
                viewer.LocalReport.ReportPath = path;


                viewModel.ToDate = viewModel.ToDate.AddDays(1);
                viewModel.ToDate = viewModel.ToDate.AddSeconds(-5);
                LedgerHistoryViewModel vm = null;
                List<LadgerType> ladgerTypeList = null;
                LedgerWorker worker;
                using (var authContext = new AuthContext())
                {
                    ladgerTypeList = authContext.LadgerTypeDB.ToList();
                }

                string ledgerTypeCode = ladgerTypeList.FirstOrDefault(x => x.ID == viewModel.LedgerTypeID)?.code;
                string companyHeader = "";
                if (ledgerTypeCode == "Customer")
                {
                    Warehouse warehouse = GetCustomerAddress(viewModel.LedgerID);
                    companyHeader = @"<div style='font-size: 15pt;'>ShopKirana E Trading Pvt. Ltd.</div>";
                    companyHeader = companyHeader + "<div>" + warehouse.Address + "</div>";
                    companyHeader = companyHeader + "<div>" + warehouse.CityName + ", Ph. Number: " + warehouse.Phone + "</div>";
                    companyHeader = companyHeader + (!string.IsNullOrEmpty(warehouse.GSTin) ? "<div>GST Number: " + warehouse.GSTin + "</div>" : "");
                    worker = new CustomerLedgerWorker();

                }
                else if (ledgerTypeCode == "Supplier")
                {
                    companyHeader = @"<div style='font-size: 15pt;'>ShopKirana E Trading Pvt. Ltd.</div>
                                            <span> Office No. 15th Floor, Skye Earth Corporate Park,</span><br>
                                            <span> 105, AB Rd, Sector C, Slice 5, Part II,</span><br>
                                            <span> Shalimar Township, Indore, Madhya Pradesh 452010</span>";
                    worker = new SupplierLedgerWorker();
                }
                else if (ledgerTypeCode == "Agent")
                {
                    Warehouse warehouse = GetAgentAddress(viewModel.LedgerID);
                    companyHeader = @"<div style='font-size: 15pt;'>ShopKirana E Trading Pvt. Ltd.</div>";
                    companyHeader = companyHeader + "<div>" + warehouse.Address + "</div>";
                    companyHeader = companyHeader + "<div>" + warehouse.CityName + ", Ph. Number: " + warehouse.Phone + "</div>";
                    companyHeader = companyHeader + (!string.IsNullOrEmpty(warehouse.GSTin) ? "<div>GST Number: " + warehouse.GSTin + "</div>" : "");
                    worker = new AgentLedgerWorker();
                }
                else
                {
                    worker = new LedgerWorker();
                }
                vm = worker.GetLedger(viewModel);

                ReportParameterCollection reportParameters = new ReportParameterCollection();

                companyHeader += "<div>CIN: U51109MP2014PTC033534</div>";
                reportParameters.Add(new ReportParameter("CompanyHeader", companyHeader));






                //
                //bankname
                //comment
                //reportParameters.Add(new ReportParameter("CashSum", ((decimal)cashSum).ToString()));


                viewer.LocalReport.SetParameters(reportParameters);
                viewer.ProcessingMode = ProcessingMode.Local;
                viewer.LocalReport.DataSources.Clear();
                DataTable dt = new DataTable("LedgerDataTable");
                viewer.LocalReport.DataSources.Add(new ReportDataSource("LedgerDataTable", dt));
                DataTable customerLedgerDataTable = null;
                double debitSum = 0, creditSum = 0;
                if (vm.LadgerEntryList != null && vm.LadgerEntryList.Count > 0)
                {
                    UpdateLadgerEntryList(vm.LadgerEntryList, ref debitSum, ref creditSum);
                    debitSum = Math.Round(debitSum, 0);
                    creditSum = Math.Round(creditSum, 0);
                    if (viewModel.IsGetRawReportForExcel.HasValue && viewModel.IsGetRawReportForExcel.Value == true)
                    {
                        worker.UpdateTableToPrintRaw(vm.LadgerEntryList);
                    }
                    else
                    {
                        worker.UpdateTableToPrint(vm.LadgerEntryList);
                    }

                    worker.UpdateOpeningAndClosingBalance(vm);
                    customerLedgerDataTable = Common.Helpers.ListtoDataTableConverter.ToDataTable(vm.LadgerEntryList);

                }
                viewer.LocalReport.DataSources.Add(new ReportDataSource("CustomerLedgerDataTable", customerLedgerDataTable));


                DataTable headerTable = new DataTable("HeaderTable");
                List<LedgerHeaderViewModel> headerList = GetLedgerHeaderViewModelList(vm, ledgerTypeCode, viewModel.FromDate, viewModel.ToDate);
                headerTable = Common.Helpers.ListtoDataTableConverter.ToDataTable(headerList);
                viewer.LocalReport.DataSources.Add(new ReportDataSource("HeaderTable", headerTable));


                viewer.LocalReport.Refresh();
                reportParameters = new ReportParameterCollection();
                if (vm.LadgerItem != null)
                {
                    vm.LadgerItem.Name = !string.IsNullOrEmpty(vm.LadgerItem.Name) ? Common.Helpers.ListtoDataTableConverter.CamelCase(vm.LadgerItem.Name) : "";
                    //reportParameters.Add(new ReportParameter("Name", !String.IsNullOrEmpty(vm.LadgerItem.Name) ? vm.LadgerItem.Name : "-"));
                    //reportParameters.Add(new ReportParameter("Alias", !String.IsNullOrEmpty(vm.LadgerItem.Alias) ? vm.LadgerItem.Alias : "-"));
                    //reportParameters.Add(new ReportParameter("Address", !String.IsNullOrEmpty(vm.LadgerItem.Address) ? vm.LadgerItem.Address : "-"));
                    //reportParameters.Add(new ReportParameter("Country", !String.IsNullOrEmpty(vm.LadgerItem.Country) ? vm.LadgerItem.Country : "-"));
                    //reportParameters.Add(new ReportParameter("PinCode", vm.LadgerItem.PinCode != null ? vm.LadgerItem.PinCode.ToString() : "-"));

                    //reportParameters.Add(new ReportParameter("FromDate", viewModel.FromDate != null ? viewModel.FromDate.ToString("dddd, dd MMMM yyyy") : "-"));
                    //reportParameters.Add(new ReportParameter("ToDate", viewModel.ToDate != null ? viewModel.ToDate.ToString("dddd, dd MMMM yyyy") : "-"));
                    //reportParameters.Add(new ReportParameter("PAN", vm.LadgerItem.PAN != null ? vm.LadgerItem.PAN : "-"));
                    //reportParameters.Add(new ReportParameter("GST", vm.LadgerItem.GSTno != null ? vm.LadgerItem.GSTno : "-"));
                    //reportParameters.Add(new ReportParameter("Date", DateTime.Now.ToString("dddd, dd MMMM yyyy")));
                    reportParameters.Add(new ReportParameter("OpeningBalance", vm.OpeningBalance.HasValue ? vm.OpeningBalanceString : "0.00"));
                }



                reportParameters.Add(new ReportParameter("DebitSum", debitSum.ToString()));
                reportParameters.Add(new ReportParameter("CreditSum", creditSum.ToString()));

                string closingBalance = (debitSum - creditSum + (vm.OpeningBalance.HasValue ? vm.OpeningBalance.Value : 0.00)).ToString();
                reportParameters.Add(new ReportParameter("ClosingBalance", closingBalance));

                viewer.LocalReport.SetParameters(reportParameters);



                string fileExtension = "";
                string reportType = "";
                if (viewModel.IsGenerateExcel)
                {
                    fileExtension = ".xls";
                    reportType = "Excel";
                }
                else
                {
                    fileExtension = ".pdf";
                    reportType = "PDF";
                }
                byte[] bytes = viewer.LocalReport.Render(reportType, null, out mimeType, out encoding, out extension, out streamIds, out warnings);

                //string filename = "customerLedger_" + DateTime.Now.ToString("yyyy-dd-MM-HH-mm-ss") + fileExtension;
                //filepath = HttpContext.Current.Server.MapPath(@"~\BankWithdrawDepositFile\") + filename;

                var FileName = ledgerTypeCode + "Ledger_" + DateTime.Now.ToString("ddMMyyyyHHmmss") + fileExtension;
                var folderPath = HttpContext.Current.Server.MapPath(@"~\ReportDownloads");

                if (!Directory.Exists(folderPath))
                    Directory.CreateDirectory(folderPath);


                var fullPhysicalPath = folderPath + "\\" + FileName;
                var fileUrl = "/ReportDownloads" + "/" + FileName;

                FileStream file = File.Create(fullPhysicalPath);


                file.Write(bytes, 0, bytes.Length);
                file.Close();
                returnfilepath = fileUrl;
            }
            catch (Exception ex)
            {
                //logger.Error("Error in Generatepdf Method: " + ex.Message);
            }

            return returnfilepath;
        }
        public static string PODetailsGenerateReport(int Poid, bool IsGenerateExcel)
        {
            string filepath = string.Empty;
            string returnfilepath = string.Empty;
            try
            {
                string path = Path.Combine(HttpContext.Current.Server.MapPath("~/Reports"), "PoInvoice.rdlc");

                // Variables
                Microsoft.Reporting.WebForms.Warning[] warnings;
                string[] streamIds;
                string mimeType = string.Empty;
                string encoding = string.Empty;
                string extension = string.Empty;

                // Setup the report viewer object and get the array of bytes
                ReportViewer viewer = new ReportViewer();
                viewer.ProcessingMode = ProcessingMode.Local;
                viewer.LocalReport.ReportPath = path;

                string companyHeader = "";

                companyHeader = @"<div style='font-size: 15pt;'>ShopKirana E Trading Pvt. Ltd.</div>
                                            <span> Office No. 15th Floor, Skye Earth Corporate Park,</span><br>
                                            <span> 105, AB Rd, Sector C, Slice 5, Part II,</span><br>
                                            <span> Shalimar Township, Indore, Madhya Pradesh 452010</span>";

                ReportParameterCollection reportParameters = new ReportParameterCollection();

                companyHeader += "<div>CIN: U51109MP2014PTC033534</div>";
                reportParameters.Add(new ReportParameter("CompanyHeader", companyHeader));

                viewer.LocalReport.SetParameters(reportParameters);
                viewer.ProcessingMode = ProcessingMode.Local;
                viewer.LocalReport.DataSources.Clear();
                //DataTable dt = new DataTable("LedgerDataTable");
                //viewer.LocalReport.DataSources.Add(new ReportDataSource("LedgerDataTable", dt));
                DataTable PoDetailDataTable = null;
                List<PODetailsDc> polist = null;
                Supplier supplier = null;
                DepoMaster depoMasters = null;
                Warehouse warehouse = null; 
                double debitSum = 0, creditSum = 0;

                using (var db = new AuthContext())
                {
                    var purchaseOrderMaster = db.DPurchaseOrderMaster.Where(x => x.PurchaseOrderId == Poid && x.Deleted == false).FirstOrDefault();
                    if (purchaseOrderMaster != null)
                    {
                        warehouse = db.Warehouses.Where(x => x.WarehouseId == purchaseOrderMaster.WarehouseId).FirstOrDefault();
                        supplier = db.Suppliers.Where(x => x.SupplierId == purchaseOrderMaster.SupplierId).FirstOrDefault();
                        depoMasters = db.DepoMasters.Where(x => x.DepoId == purchaseOrderMaster.DepoId).FirstOrDefault();

                        polist = db.DPurchaseOrderDeatil.Where(x => x.PurchaseOrderId == Poid).Select(x => new PODetailsDc
                        {
                            PoId = x.PurchaseOrderId,
                            PurchaseSku = x.PurchaseSku,
                            CompanyStockCode = "123113",
                            ItemName = x.ItemName,
                            ABcClassification = "A",
                            HSNCode = x.HSNCode,
                            Price = x.Price,
                            MOQ = x.MOQ,
                            Qty = ((x.TotalQuantity) / x.MOQ),
                            NoOfPieces = ((x.MOQ * x.TotalQuantity) / x.MOQ),
                            TotalAmount = ((x.Price) * (x.TotalQuantity)),
                        }).ToList();

                        if (polist != null && polist.Count > 0)
                        {
                            PoDetailDataTable = Common.Helpers.ListtoDataTableConverter.ToDataTable(polist);

                        }
                        viewer.LocalReport.DataSources.Add(new ReportDataSource("DataSet1", PoDetailDataTable));

                        DataTable headerTable = new DataTable("PoInformationDs");
                        var list = PoInformationDs(purchaseOrderMaster);
                        headerTable = Common.Helpers.ListtoDataTableConverter.ToDataTable(list);
                        viewer.LocalReport.DataSources.Add(new ReportDataSource("PoInformationDs", headerTable));

                        DataTable SuplTable = new DataTable("SupplierInformationDs");
                        var Supplist = SupplierInformationDs(supplier);
                        SuplTable = Common.Helpers.ListtoDataTableConverter.ToDataTable(Supplist);
                        viewer.LocalReport.DataSources.Add(new ReportDataSource("SupplierInformationDs", SuplTable));

                        DataTable DepoTable = new DataTable("DepoInformationDs");
                        var Depolist = DepoInformationDs(depoMasters);
                        DepoTable = Common.Helpers.ListtoDataTableConverter.ToDataTable(Depolist);
                        viewer.LocalReport.DataSources.Add(new ReportDataSource("DepoInformationDs", DepoTable));

                        DataTable WareTable = new DataTable("WarehouseDs");
                        var Warehouselist = WareHouseInformationDs(warehouse);
                        WareTable = Common.Helpers.ListtoDataTableConverter.ToDataTable(Warehouselist);
                        viewer.LocalReport.DataSources.Add(new ReportDataSource("WarehouseDs", WareTable));
                        viewer.LocalReport.Refresh();
                        string fileExtension = "";
                        string reportType = "";
                        if (IsGenerateExcel)
                        {
                            fileExtension = ".xls";
                            reportType = "Excel";
                        }
                        else
                        {
                            fileExtension = ".pdf";
                            reportType = "PDF";
                        }
                        byte[] bytes = viewer.LocalReport.Render(reportType, null, out mimeType, out encoding, out extension, out streamIds, out warnings);

                        //string filename = "customerLedger_" + DateTime.Now.ToString("yyyy-dd-MM-HH-mm-ss") + fileExtension;
                        //filepath = HttpContext.Current.Server.MapPath(@"~\BankWithdrawDepositFile\") + filename;

                        var FileName = polist[0].PoId + "_PO" + DateTime.Now.ToString("ddMMyyyyHHmmss") + fileExtension;
                        var folderPath = HttpContext.Current.Server.MapPath(@"~\ReportDownloads");

                        if (!Directory.Exists(folderPath))
                            Directory.CreateDirectory(folderPath);


                        var fullPhysicalPath = folderPath + "\\" + FileName;
                        var fileUrl = "/ReportDownloads" + "/" + FileName;

                        FileStream file = File.Create(fullPhysicalPath);


                        file.Write(bytes, 0, bytes.Length);
                        file.Close();
                        returnfilepath = fileUrl;
                    }
                }
            }
            catch (Exception ex)
            {
                //logger.Error("Error in Generatepdf Method: " + ex.Message);
            }

            return returnfilepath;
        }
        private static List<POInformationDC> PoInformationDs(PurchaseOrderMaster vm)
        {
            List<POInformationDC> list = new List<POInformationDC>();

            list.Add(new POInformationDC
            {
                Name1 = "PoInvoiceNo:",
                Val1 = !string.IsNullOrEmpty(vm.PoInvoiceNo) ? vm.PoInvoiceNo : "-",
            });

            list.Add(new POInformationDC
            {
                Name1 = "PurchaseOrderId:",
                Val1 = !string.IsNullOrEmpty(vm.PurchaseOrderId.ToString()) ? vm.PurchaseOrderId.ToString() : "-",
            });

            list.Add(new POInformationDC
            {
                Name1 = "CreationDate:",
                Val1 = !string.IsNullOrEmpty(vm.CreationDate.ToString()) ? vm.CreationDate.ToString() : "-"
            });

            list.Add(new POInformationDC
            {
                Name1 = "BuyerName:",
                Val1 = !string.IsNullOrEmpty(vm.BuyerName) ? vm.BuyerName : "-",
            });

            list.Add(new POInformationDC
            {
                Name1 = "PickerType:",
                Val1 = !string.IsNullOrEmpty(vm.PickerType) ? vm.PickerType : "-",
            });
            return list;
        }
        private static List<SupplierInformationDC> SupplierInformationDs(Supplier vm)
        {
            List<SupplierInformationDC> list = new List<SupplierInformationDC>();

            list.Add(new SupplierInformationDC
            {
                Name2 = "Name:",
                Val2 = !string.IsNullOrEmpty(vm.Name) ? vm.Name : "-",
            });

            list.Add(new SupplierInformationDC
            {
                Name2 = "MobileNo:",
                Val2 = !string.IsNullOrEmpty(vm.MobileNo) ? vm.MobileNo : "-",
            });

            list.Add(new SupplierInformationDC
            {
                Name2 = "TINNo:",
                Val2 = !string.IsNullOrEmpty(vm.TINNo) ? vm.TINNo : "-"
            });

            list.Add(new SupplierInformationDC
            {
                Name2 = "BillingAddress:",
                Val2 = !string.IsNullOrEmpty(vm.BillingAddress) ? vm.BillingAddress : "-",
            });
            return list;
        }
        private static List<DepoInformationDC> DepoInformationDs(DepoMaster vm)
        {
            List<DepoInformationDC> list = new List<DepoInformationDC>();

            list.Add(new DepoInformationDC
            {
                Name3 = "DepoName:",
                Val3 = !string.IsNullOrEmpty(vm.DepoName) ? vm.DepoName : "-",
            });

            list.Add(new DepoInformationDC
            {
                Name3 = "Address:",
                Val3 = !string.IsNullOrEmpty(vm.Address) ? vm.Address : "-",
            });

            list.Add(new DepoInformationDC
            {
                Name3 = "Phone:",
                Val3 = !string.IsNullOrEmpty(vm.Phone) ? vm.Phone : "-"
            });

            list.Add(new DepoInformationDC
            {
                Name3 = "GSTin:",
                Val3 = !string.IsNullOrEmpty(vm.GSTin) ? vm.GSTin : "-",
            });
            return list;
        }
        private static List<WareHouseInformationDC> WareHouseInformationDs(Warehouse vm)
        {
            List<WareHouseInformationDC> list = new List<WareHouseInformationDC>();

            list.Add(new WareHouseInformationDC
            {
                Name4= "Delivery at:",
                Val4 = !string.IsNullOrEmpty(vm.WarehouseName) ? vm.WarehouseName : "-",
            });

            list.Add(new WareHouseInformationDC
            {
                Name4 = "Address:",
                Val4 = !string.IsNullOrEmpty(vm.Address) ? vm.Address : "-",
            });

            list.Add(new WareHouseInformationDC
            {
                Name4 = "City:",
                Val4 = !string.IsNullOrEmpty(vm.CityName) ? vm.CityName : "-",
            });

            list.Add(new WareHouseInformationDC
            {
                Name4 = "Phone:",
                Val4 = !string.IsNullOrEmpty(vm.Phone) ? vm.Phone : "-",
            });

            list.Add(new WareHouseInformationDC
            {
                Name4 = "GST No:",
                Val4 = !string.IsNullOrEmpty(vm.GSTin) ? vm.GSTin : "-",
            });
            return list;
        }

        public static void UpdateLadgerEntryList(List<LadgerEntryVM> ladgerEntryList, ref double debitSum, ref double creditSum)
        {
            debitSum = 0;
            creditSum = 0;
            if (ladgerEntryList != null && ladgerEntryList.Count > 0)
            {
                foreach (var ladgerEntry in ladgerEntryList)
                {
                    ladgerEntry.Debit = ladgerEntry.Debit != null ? Math.Round(ladgerEntry.Debit.Value, 2) : ladgerEntry.Debit;
                    ladgerEntry.Credit = ladgerEntry.Credit != null ? Math.Round(ladgerEntry.Credit.Value, 2) : ladgerEntry.Credit;
                    debitSum += ladgerEntry.Debit != null ? ladgerEntry.Debit.Value : 0;
                    creditSum += ladgerEntry.Credit != null ? ladgerEntry.Credit.Value : 0;
                }
            }
        }
        private static void UpdateDayBalance(List<LadgerEntryVM> ladgerEntryVMList, double openingBalance)
        {
            if (ladgerEntryVMList != null && ladgerEntryVMList.Count > 0)
            {
                //var groupList = ladgerEntryVMList.GroupBy(x => x.Date.Value.Year).ToList();

                var q = from le in ladgerEntryVMList
                        let dt = le.Date
                        group le by new { y = dt.Value.Year, m = dt.Value.Month, d = dt.Value.Day };
                var groupList = q.ToList();

                foreach (var group in groupList)
                {
                    double creditSum = group.ToList().Sum(x => x.Credit ?? 0);
                    double debitSum = group.ToList().Sum(x => x.Debit ?? 0);
                    openingBalance = creditSum - debitSum + openingBalance;
                    group.ToList().Last().DayBalance = Math.Round(openingBalance);
                }
            }
        }
        private static void CashEntry(ConcurrentBag<LadgerEntry> result, IGrouping<int, CustomerLedgerEntryModel> group, long ledgerID, long cashLedgerId, long? receiptVoucherTypeId, Boolean isTakeTransactionFromPaymentResponseRetailerApps)
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
                    LadgerEntry entry = CreateLadgerEntry(cashDispatchOrder, ledgerID, "Order", particular, amount.Value, null, cashLedgerId, receiptVoucherTypeId);
                    entry.Date = cashDispatchOrder.Deliverydate;
                    result.Add(entry);

                    particular = "Cash get regarding Order " + cashDispatchOrder.OrderId;
                    entry = CreateLadgerEntry(cashDispatchOrder, cashLedgerId, "Order", particular, null, amount.Value, ledgerID, receiptVoucherTypeId);
                    entry.Date = cashDispatchOrder.Deliverydate;
                    result.Add(entry);
                }

            }
        }
        private static void chequeEntry(ConcurrentBag<LadgerEntry> result, IGrouping<int, CustomerLedgerEntryModel> group, long ledgerID, long bankLedgerId, long? receiptVoucherTypeId, Boolean isTakeTransactionFromPaymentResponseRetailerApps)
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
                    Voucher vch = SaveVoucher(vchCode, 9999);
                    string particular = "Cheque payment by cheque number " + vchCode + " regarding Order " + chequeDispatchOrder.OrderId;
                    LadgerEntry entry = CreateLadgerEntry(chequeDispatchOrder, ledgerID, "Order", particular, amount.Value, null, bankLedgerId, receiptVoucherTypeId, vch.ID);
                    entry.Date = chequeDispatchOrder.Deliverydate;
                    result.Add(entry);

                    particular = "Cheque get by cheque number " + (!string.IsNullOrEmpty(chequeDispatchOrder.Gatewaytransid) ? chequeDispatchOrder.Gatewaytransid : chequeDispatchOrder.CheckNo) + " regarding Order " + chequeDispatchOrder.OrderId;
                    entry = CreateLadgerEntry(chequeDispatchOrder, bankLedgerId, "Order", particular, null, amount.Value, ledgerID, receiptVoucherTypeId, vch.ID);
                    entry.Date = chequeDispatchOrder.Deliverydate;
                    result.Add(entry);
                }
            }
        }
        private static void ElectronicEntry(ConcurrentBag<LadgerEntry> result, IGrouping<int, CustomerLedgerEntryModel> group, long ledgerID, long electronicLedgerId, long? receiptVoucherTypeId, Boolean isTakeTransactionFromPaymentResponseRetailerApps)
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
                    string vchCode = (!string.IsNullOrEmpty(electronicDispatchOrder.Gatewaytransid) ? electronicDispatchOrder.Gatewaytransid : electronicDispatchOrder.ElectronicPaymentNo);
                    Voucher vch = SaveVoucher(vchCode, 9999);

                    string particular = "Electronic payment by electronic payment number " + (!string.IsNullOrEmpty(electronicDispatchOrder.Gatewaytransid) ? electronicDispatchOrder.Gatewaytransid : electronicDispatchOrder.ElectronicPaymentNo) + "regarding Order " + electronicDispatchOrder.OrderId;
                    LadgerEntry entry = CreateLadgerEntry(electronicDispatchOrder, ledgerID, "Order", particular, amount.Value, null, electronicLedgerId, receiptVoucherTypeId, vch.ID);
                    entry.Date = electronicDispatchOrder.Deliverydate;
                    result.Add(entry);

                    particular = "Electronic get payment number " + (!string.IsNullOrEmpty(electronicDispatchOrder.Gatewaytransid) ? electronicDispatchOrder.Gatewaytransid : electronicDispatchOrder.ElectronicPaymentNo) + "regarding Order " + electronicDispatchOrder.OrderId;
                    entry = CreateLadgerEntry(electronicDispatchOrder, electronicLedgerId, "Order", particular, null, amount.Value, ledgerID, receiptVoucherTypeId, vch.ID);
                    entry.Date = electronicDispatchOrder.Deliverydate;
                    result.Add(entry);
                }
            }
        }
        private static void MPosEntry(ConcurrentBag<LadgerEntry> result, IGrouping<int, CustomerLedgerEntryModel> group, long ledgerID, long mPosLedgerId, long? receiptVoucherTypeId, Boolean isTakeTransactionFromPaymentResponseRetailerApps)
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
                    string vchCode = (!string.IsNullOrEmpty(mposDispatchOrder.Gatewaytransid) ? mposDispatchOrder.Gatewaytransid : " ");
                    Voucher vch = SaveVoucher(vchCode, 9999);

                    string particular = "mpos payment  number " + (!string.IsNullOrEmpty(mposDispatchOrder.Gatewaytransid) ? mposDispatchOrder.Gatewaytransid : " ") + "regarding Order " + mposDispatchOrder.OrderId;
                    LadgerEntry entry = CreateLadgerEntry(mposDispatchOrder, ledgerID, "Order", particular, amount.Value, null, mPosLedgerId, receiptVoucherTypeId, vch.ID);
                    entry.Date = mposDispatchOrder.Deliverydate;
                    result.Add(entry);

                    particular = "mpos payment  number  " + (!string.IsNullOrEmpty(mposDispatchOrder.Gatewaytransid) ? mposDispatchOrder.Gatewaytransid : "") + "regarding Order " + mposDispatchOrder.OrderId;
                    entry = CreateLadgerEntry(mposDispatchOrder, mPosLedgerId, "Order", particular, null, amount.Value, ledgerID, receiptVoucherTypeId, vch.ID);
                    entry.Date = mposDispatchOrder.Deliverydate;
                    result.Add(entry);
                }

            }
        }
        private static bool IsTakeTransactionFromPaymentResponseRetailerApps(IGrouping<int, CustomerLedgerEntryModel> group)
        {
            return group.Any(x => x.Amount.HasValue && x.Amount.Value > 0);
        }
        public static void UpdateLedger(LedgerCorrectorViewModel vm)
        {
            using (var authContext = new AuthContext())
            {
                var query = from le in authContext.LadgerEntryDB
                            where le.ObjectID == vm.OrderDispatchedMasterInfo.OrderId && le.ObjectType == "Order"
                            select le;

                List<LadgerEntry> ladgerEntryList = query.ToList();

                if (vm.LedgerUpdatedFields.Cash)
                {
                    var cashLadgerQuery = from l in authContext.LadgerDB
                                          join lt in authContext.LadgerTypeDB
                                          on l.LadgertypeID equals lt.ID
                                          where lt.code == "Cash" && l.Name == "cash"
                                          select l.ID;

                    var cashLedgerId = cashLadgerQuery.FirstOrDefault();
                    List<LadgerEntry> cashLedgerEntryList = ladgerEntryList.Where(x => x.LagerID == cashLedgerId || x.AffectedLadgerID == cashLedgerId).ToList();
                    UpdateLedgerEntry(cashLedgerEntryList, authContext, vm.OrderDispatchedMasterInfo.CashAmount);
                }

                if (vm.LedgerUpdatedFields.Cheque)
                {
                    var chequeLadgerQuery = from l in authContext.LadgerDB
                                            join lt in authContext.LadgerTypeDB
                                            on l.LadgertypeID equals lt.ID
                                            where lt.code == "Bank" && l.Name.ToLower() == "bank"
                                            select l.ID;

                    var chequeLedgerId = chequeLadgerQuery.FirstOrDefault();
                    List<LadgerEntry> chequeLedgerEntryList = ladgerEntryList.Where(x => x.LagerID == chequeLedgerId || x.AffectedLadgerID == chequeLedgerId).ToList();
                    UpdateLedgerEntry(chequeLedgerEntryList, authContext, vm.OrderDispatchedMasterInfo.CheckAmount);
                }

                if (vm.LedgerUpdatedFields.Electronic)
                {
                    var electronicLadgerQuery = from l in authContext.LadgerDB
                                                join lt in authContext.LadgerTypeDB
                                                on l.LadgertypeID equals lt.ID
                                                where lt.code == "Electronic" && l.Name.ToLower() == "electronic"
                                                select l.ID;

                    var electronicLedgerId = electronicLadgerQuery.FirstOrDefault();
                    List<LadgerEntry> chequeLedgerEntryList = ladgerEntryList.Where(x => x.LagerID == electronicLedgerId || x.AffectedLadgerID == electronicLedgerId).ToList();
                    UpdateLedgerEntry(chequeLedgerEntryList, authContext, vm.OrderDispatchedMasterInfo.ElectronicAmount);
                }

                if (vm.LedgerUpdatedFields.Mpos)
                {
                    var mposLadgerQuery = from l in authContext.LadgerDB
                                          join lt in authContext.LadgerTypeDB
                                          on l.LadgertypeID equals lt.ID
                                          where lt.code.ToLower() == "mpos" && l.Name.ToLower() == "mpos"
                                          select l.ID;

                    var mposLedgerId = mposLadgerQuery.FirstOrDefault();
                    List<LadgerEntry> chequeLedgerEntryList = ladgerEntryList.Where(x => x.LagerID == mposLedgerId || x.AffectedLadgerID == mposLedgerId).ToList();
                    UpdateLedgerEntry(chequeLedgerEntryList, authContext, vm.OrderDispatchedMasterInfo.ElectronicAmount);

                }


                if (vm.FinalOrderDispatchedMasterInfo != null)
                {
                    var tempVM = authContext.FinalOrderDispatchedMasterDb.FirstOrDefault(x => x.FinalOrderDispatchedMasterId == vm.FinalOrderDispatchedMasterInfo.FinalOrderDispatchedMasterId);
                    tempVM.CashAmount = vm.FinalOrderDispatchedMasterInfo.CashAmount;
                    tempVM.CheckAmount = vm.FinalOrderDispatchedMasterInfo.CheckAmount;
                    tempVM.ElectronicAmount = vm.FinalOrderDispatchedMasterInfo.ElectronicAmount;

                    authContext.Entry(tempVM).State = EntityState.Modified;
                }

                if (vm.OrderDeliveryMasterInfo != null)
                {
                    var tempVM = authContext.OrderDeliveryMasterDB.FirstOrDefault(x => x.OrderDeliveryId == vm.OrderDeliveryMasterInfo.OrderDeliveryId);
                    tempVM.CashAmount = vm.FinalOrderDispatchedMasterInfo.CashAmount;
                    tempVM.CheckAmount = vm.FinalOrderDispatchedMasterInfo.CheckAmount;
                    tempVM.ElectronicAmount = vm.FinalOrderDispatchedMasterInfo.ElectronicAmount;

                    authContext.Entry(tempVM).State = EntityState.Modified;
                }

                if (vm.OrderDispatchedMasterInfo != null)
                {
                    var tempVM = authContext.OrderDispatchedMasters.FirstOrDefault(x => x.OrderDispatchedMasterId == vm.OrderDispatchedMasterInfo.OrderDispatchedMasterId);
                    tempVM.CashAmount = vm.FinalOrderDispatchedMasterInfo.CashAmount;
                    tempVM.CheckAmount = vm.FinalOrderDispatchedMasterInfo.CheckAmount;
                    tempVM.ElectronicAmount = vm.FinalOrderDispatchedMasterInfo.ElectronicAmount;

                    authContext.Entry(tempVM).State = EntityState.Modified;
                }

                //authContext.Database.
                authContext.Commit();
            }
        }
        private static void UpdateLedgerEntry(List<LadgerEntry> ladgerEntryList, AuthContext authContext, double amount)
        {
            if (ladgerEntryList != null && ladgerEntryList.Count > 0)
            {
                foreach (var item in ladgerEntryList)
                {
                    if (item.Credit != null)
                    {
                        item.Credit = amount;
                        authContext.Entry(item).State = EntityState.Modified;
                        authContext.Commit();
                    }
                    else if (item.Debit != null)
                    {
                        item.Debit = amount;
                        authContext.Entry(item).State = EntityState.Modified;
                        authContext.Commit();
                    }
                }
            }

        }
        private static Warehouse GetCustomerAddress(int ladgerID)
        {
            using (var conext = new AuthContext())
            {
                var query = from l in conext.LadgerDB
                            join c in conext.Customers
                            on l.ObjectID equals c.CustomerId
                            join w in conext.Warehouses
                            on c.Warehouseid equals w.WarehouseId
                            where l.ObjectType == "Customer" && l.ID == ladgerID
                            select w;
                var warehouse = query.FirstOrDefault();
                return warehouse;
            }
        }

        private static Warehouse GetAgentAddress(int ladgerID)
        {
            using (var conext = new AuthContext())
            {

                var query = from l in conext.LadgerDB
                            join p in conext.Peoples
                            on l.ObjectID equals p.PeopleID
                            join w in conext.Warehouses
                            on p.WarehouseId equals w.WarehouseId
                            where l.ObjectType == "Agent" && l.ID == ladgerID
                            select w;
                var warehouse = query.FirstOrDefault();
                return warehouse;
            }
        }

        private static List<LedgerHeaderViewModel> GetLedgerHeaderViewModelList(LedgerHistoryViewModel vm, string ledgerTypeCode, DateTime from, DateTime todate)
        {
            List<LedgerHeaderViewModel> list = new List<LedgerHeaderViewModel>();


            if (ledgerTypeCode == "Customer")
            {
                list.Add(new LedgerHeaderViewModel
                {
                    Name1 = "Name:",
                    Val1 = !string.IsNullOrEmpty(vm.LadgerItem.Name) ? vm.LadgerItem.Name : "-",
                    Name2 = "PAN",
                    Val2 = !string.IsNullOrEmpty(vm.LadgerItem.PAN) ? vm.LadgerItem.PAN : "-"

                });

                list.Add(new LedgerHeaderViewModel
                {
                    Name1 = "Sk Code:",
                    Val1 = !string.IsNullOrEmpty(vm.LadgerItem.Alias) ? vm.LadgerItem.Alias : "-",
                    Name2 = "GST:",
                    Val2 = !string.IsNullOrEmpty(vm.LadgerItem.GSTno) ? vm.LadgerItem.GSTno : "-"

                });

                list.Add(new LedgerHeaderViewModel
                {
                    Name1 = "Address:",
                    Val1 = !string.IsNullOrEmpty(vm.LadgerItem.Address) ? vm.LadgerItem.Address : "-",
                    Name2 = "Warehouse:",
                    Val2 = !string.IsNullOrEmpty(vm.LadgerItem.WarehouseName) ? vm.LadgerItem.WarehouseName : "-"

                });
            }
            else if (ledgerTypeCode == "Supplier")
            {
                list.Add(new LedgerHeaderViewModel
                {
                    Name1 = "Name:",
                    Val1 = !string.IsNullOrEmpty(vm.LadgerItem.Name) ? vm.LadgerItem.Name : "-",
                    Name2 = "PAN",
                    Val2 = !string.IsNullOrEmpty(vm.LadgerItem.PAN) ? vm.LadgerItem.PAN : "-"

                });

                list.Add(new LedgerHeaderViewModel
                {
                    Name1 = "Sk Code:",
                    Val1 = !string.IsNullOrEmpty(vm.LadgerItem.Alias) ? vm.LadgerItem.Alias : "-",
                    Name2 = "GST:",
                    Val2 = !string.IsNullOrEmpty(vm.LadgerItem.GSTno) ? vm.LadgerItem.GSTno : "-"

                });

                list.Add(new LedgerHeaderViewModel
                {
                    Name1 = "Address:",
                    Val1 = !string.IsNullOrEmpty(vm.LadgerItem.Address) ? vm.LadgerItem.Address : "-",
                    Name2 = "Warehouse:",
                    Val2 = !string.IsNullOrEmpty(vm.LadgerItem.WarehouseName) ? vm.LadgerItem.WarehouseName : "-"

                });
            }
            else if (ledgerTypeCode == "Agent")
            {
                list.Add(new LedgerHeaderViewModel
                {
                    Name1 = "Name:",
                    Val1 = !string.IsNullOrEmpty(vm.LadgerItem.Name) ? vm.LadgerItem.Name : "-",
                    Name2 = "PAN",
                    Val2 = !string.IsNullOrEmpty(vm.LadgerItem.PAN) ? vm.LadgerItem.PAN : "-"

                });

                list.Add(new LedgerHeaderViewModel
                {
                    Name1 = "Agent Code:",
                    Val1 = !string.IsNullOrEmpty(vm.LadgerItem.Alias) ? vm.LadgerItem.Alias : "-",
                    Name2 = "GST:",
                    Val2 = !string.IsNullOrEmpty(vm.LadgerItem.GSTno) ? vm.LadgerItem.GSTno : "-"

                });

                list.Add(new LedgerHeaderViewModel
                {
                    Name1 = "Address:",
                    Val1 = !string.IsNullOrEmpty(vm.LadgerItem.Address) ? vm.LadgerItem.Address : "-",
                    Name2 = "Warehouse:",
                    Val2 = !string.IsNullOrEmpty(vm.LadgerItem.WarehouseName) ? vm.LadgerItem.WarehouseName : "-"

                });

            }
            else
            {

            }


            list.Add(new LedgerHeaderViewModel
            {
                Name1 = "Generated Date:",
                Val1 = DateTime.Now.ToString("dddd, dd MMMM yyyy"),
                Name2 = "",
                Val2 = ""

            });
            list.Add(new LedgerHeaderViewModel
            {
                Name1 = "From Date:",
                Val1 = from.ToString("dddd, dd MMMM yyyy"),
                Name2 = "To Date:",
                Val2 = todate.ToString("dddd, dd MMMM yyyy")

            });
            return list;


        }

        public static Voucher GetOrCreateVoucher(string code)
        {
            using (var context = new AuthContext())
            {
                Voucher vch = context.VoucherDB.Where(x => x.Code == code).FirstOrDefault();
                if (vch == null)
                {
                    vch = SaveVoucher(code, 9999);


                }
                return vch;
            }
        }

        public static void UpdateCancelOrder(int month, int year, int day, bool isDelivered, bool isSattled, int? orderid = null)
        {
            using (var authContext = new AuthContext())
            {
                //var ledgerList = authContext.LadgerDB.ToList();
                var monthParam = new SqlParameter("@Month", month);
                var yearParam = new SqlParameter("@Year", year);
                var dayParam = new SqlParameter("@Day", day);
                SqlParameter orderIdParam = null;
                if (orderid.HasValue)
                {
                    orderIdParam = new SqlParameter("@OrderID", orderid);
                }


                string spName = "";
                if (isDelivered)
                {
                    spName = "CustomerLedgerEntryDeliveredUpdate";
                }
                else if (isSattled)
                {
                    spName = "CustomerLedgerEntrySattledUpdate";
                }
                else
                {
                    return;
                }


                List<CustomerLedgerEntryModel> dispatchOrderList = null;
                if (orderid.HasValue)
                {
                    spName = spName + " @Day,@Month,@Year,@OrderID";
                    dispatchOrderList = authContext.Database.SqlQuery<CustomerLedgerEntryModel>(spName, monthParam, yearParam, dayParam, orderIdParam).ToList();
                }
                else
                {
                    spName = spName + " @Day,@Month,@Year";
                    dispatchOrderList = authContext.Database.SqlQuery<CustomerLedgerEntryModel>(spName, monthParam, yearParam, dayParam).ToList();
                }


                var ledgerListToSave = new List<LadgerEntry>();
                var voucherTypes = authContext.VoucherTypeDB.Where(x => x.Active).ToList();


                if (dispatchOrderList != null && dispatchOrderList.Count > 0)
                {




                    var result = new ConcurrentBag<LadgerEntry>();
                    var salesLedgerId = authContext.LadgerDB.FirstOrDefault(x => x.LadgertypeID != 1 && !string.IsNullOrEmpty(x.Name) && x.Name.ToLower() == "sales").ID;
                    var walletLedgerId = authContext.LadgerDB.FirstOrDefault(x => x.LadgertypeID != 1 && !string.IsNullOrEmpty(x.Name) && x.Name.ToLower() == "wallet").ID;
                    var gst5LedgerId = authContext.LadgerDB.FirstOrDefault(x => x.LadgertypeID != 1 && !string.IsNullOrEmpty(x.Name) && x.Name.ToLower() == "gst5").ID;
                    var gst12LedgerId = authContext.LadgerDB.FirstOrDefault(x => x.LadgertypeID != 1 && !string.IsNullOrEmpty(x.Name) && x.Name.ToLower() == "gst12").ID;
                    var gst14LedgerId = authContext.LadgerDB.FirstOrDefault(x => x.LadgertypeID != 1 && !string.IsNullOrEmpty(x.Name) && x.Name.ToLower() == "gst14").ID;
                    var gst18LedgerId = authContext.LadgerDB.FirstOrDefault(x => x.LadgertypeID != 1 && !string.IsNullOrEmpty(x.Name) && x.Name.ToLower() == "gst18").ID;
                    var gst28LedgerId = authContext.LadgerDB.FirstOrDefault(x => x.LadgertypeID != 1 && !string.IsNullOrEmpty(x.Name) && x.Name.ToLower() == "gst28").ID;
                    var cessTaxLedgerId = authContext.LadgerDB.FirstOrDefault(x => x.LadgertypeID != 1 && !string.IsNullOrEmpty(x.Name) && x.Name.ToLower() == "cesstax").ID;


                    var cashLedgerId = authContext.LadgerDB.FirstOrDefault(x => x.LadgertypeID != 1 && !string.IsNullOrEmpty(x.Name) && x.Name.ToLower() == "cash").ID;
                    var electronicLedgerId = authContext.LadgerDB.FirstOrDefault(x => x.LadgertypeID != 1 && !string.IsNullOrEmpty(x.Name) && x.Name.ToLower() == "electronic").ID;
                    var bankLedgerId = authContext.LadgerDB.FirstOrDefault(x => x.LadgertypeID != 1 && !string.IsNullOrEmpty(x.Name) && x.Name.ToLower() == "bank").ID;

                    var billDiscountLedgerId = authContext.LadgerDB.FirstOrDefault(x => x.LadgertypeID != 1 && !string.IsNullOrEmpty(x.Name) && x.Name.ToLower() == "billdiscount").ID;
                    var postDiscountLedgerId = authContext.LadgerDB.FirstOrDefault(x => x.LadgertypeID != 1 && !string.IsNullOrEmpty(x.Name) && x.Name.ToLower() == "postdiscount").ID;
                    var mPosLedgerId = authContext.LadgerDB.FirstOrDefault(x => x.LadgertypeID != 1 && !string.IsNullOrEmpty(x.Name) && x.Name.ToLower() == "mpos").ID;
                    var deliveryChargesLedgerId = authContext.LadgerDB.FirstOrDefault(x => x.LadgertypeID != 1 && !string.IsNullOrEmpty(x.Name) && x.Name.ToLower() == "deliverycharges").ID;


                    CustomerLedgerHelper customerLedgerHelper = new CustomerLedgerHelper();
                    var hdfcLedgerId = customerLedgerHelper.GetHDFCLedgerID(authContext);
                    var epayLaterId = customerLedgerHelper.GetEpaylaterLedgerID(authContext);
                    var truePayLedgerID = customerLedgerHelper.GetTruePayLedgerID(authContext);


                    var salesVoucherTypeId = voucherTypes.FirstOrDefault(x => x.Name.ToLower() == "sales").ID;
                    var receiptVoucherTypeId = voucherTypes.FirstOrDefault(x => x.Name.ToLower() == "receipt").ID;
                    var journalVoucherTypeId = voucherTypes.FirstOrDefault(x => x.Name.ToLower() == "journal").ID;
                    var taxVoucherTypeId = voucherTypes.FirstOrDefault(x => x.Name.ToLower() == "tax").ID;


                    //foreach (var dispatchOrder in dispatchOrderList)

                    var ordergroup = dispatchOrderList.GroupBy(x => x.OrderId).ToList();

                    foreach (var group in ordergroup)
                    {
                        CustomerLedgerEntryModel dispatchOrder = group.First();



                        bool isTakeTransactionFromPaymentResponseRetailerApps = IsTakeTransactionFromPaymentResponseRetailerApps(group);

                        long? ledgerIDNullable = authContext.LadgerDB.Where(x => x.LadgertypeID == 1 && x.ObjectID == dispatchOrder.CustomerId)?.FirstOrDefault()?.ID;

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

                        var list = authContext.LadgerEntryDB.Where(x => x.ObjectID == dispatchOrder.OrderId && x.ObjectType == "Order").ToList();
                        if (list != null && list.Count > 0
                            && ((list.Any(x => x.LagerID == salesLedgerId && x.Debit > 0) && isDelivered)
                                        || (list.Any(x => x.LagerID == ledgerIDNullable.Value && x.Credit > 0) && isSattled))
                        )
                        {
                            continue;
                        }


                        else if (ledgerIDNullable.HasValue)
                        {
                            long ledgerID = ledgerIDNullable.Value;
                            String particular = "";
                            LadgerEntry entry = null;

                            if (isDelivered)
                            {
                                if (dispatchOrder.TotalAmount > 0)
                                {
                                    particular = "Order Placed for order " + dispatchOrder.OrderId;
                                    double customerAmount = (dispatchOrder.TotalAmount ?? 0) - (dispatchOrder.DeliveryCharge ?? 0) - (dispatchOrder.TaxAmount ?? 0) + (dispatchOrder.BillDiscountAmount ?? 0) + (dispatchOrder.WalletAmount ?? 0);
                                    entry = CreateLadgerEntry(dispatchOrder, ledgerID, "Order", particular, null, customerAmount, salesLedgerId, salesVoucherTypeId);
                                    entry.Date = dispatchOrder.Deliverydate;
                                    result.Add(entry);

                                    double salesAmount = (dispatchOrder.TotalAmount ?? 0) - (dispatchOrder.DeliveryCharge ?? 0) - (dispatchOrder.TaxAmount ?? 0) + (dispatchOrder.BillDiscountAmount ?? 0) + (dispatchOrder.WalletAmount ?? 0);
                                    particular = "Sales for order " + dispatchOrder.OrderId;
                                    entry = CreateLadgerEntry(dispatchOrder, salesLedgerId, "Order", particular, salesAmount, null, ledgerID, salesVoucherTypeId);
                                    entry.Date = dispatchOrder.Deliverydate;
                                    //authContext.LadgerEntryDB.Add(entry);
                                    result.Add(entry);
                                }

                                if (dispatchOrder.DeliveryCharge != null && dispatchOrder.DeliveryCharge > 0)
                                {
                                    particular = "Delivery charge " + dispatchOrder.DeliveryCharge + " regarding Order " + dispatchOrder.OrderId;
                                    entry = CreateLadgerEntry(dispatchOrder, ledgerID, "Order", particular, null, dispatchOrder.DeliveryCharge, deliveryChargesLedgerId, journalVoucherTypeId);
                                    entry.Date = dispatchOrder.Deliverydate;
                                    result.Add(entry);

                                    particular = "Delivery charge " + dispatchOrder.DeliveryCharge + " debited regarding Order " + dispatchOrder.OrderId;
                                    entry = CreateLadgerEntry(dispatchOrder, deliveryChargesLedgerId, "Order", particular, dispatchOrder.DeliveryCharge, null, ledgerID, journalVoucherTypeId);
                                    entry.Date = dispatchOrder.Deliverydate;
                                    result.Add(entry);
                                }

                                if (dispatchOrder.Five != null && dispatchOrder.Five > 0)
                                {
                                    particular = "GST5 credited regarding Order " + dispatchOrder.OrderId;
                                    entry = CreateLadgerEntry(dispatchOrder, ledgerID, "Order", particular, null, dispatchOrder.Five, gst5LedgerId, taxVoucherTypeId);
                                    entry.Date = dispatchOrder.Deliverydate;
                                    result.Add(entry);

                                    particular = "GST5 debited regarding Order " + dispatchOrder.OrderId;
                                    entry = CreateLadgerEntry(dispatchOrder, gst5LedgerId, "Order", particular, dispatchOrder.Five, null, ledgerID, taxVoucherTypeId);
                                    entry.Date = dispatchOrder.Deliverydate;
                                    result.Add(entry);
                                }

                                if (dispatchOrder.OneTwo != null && dispatchOrder.OneTwo > 0)
                                {
                                    particular = "GST12 credited regarding Order " + dispatchOrder.OrderId;
                                    entry = CreateLadgerEntry(dispatchOrder, ledgerID, "Order", particular, null, dispatchOrder.OneTwo, gst12LedgerId, taxVoucherTypeId);
                                    entry.Date = dispatchOrder.Deliverydate;
                                    result.Add(entry);

                                    particular = "GST12 debited regarding Order " + dispatchOrder.OrderId;
                                    entry = CreateLadgerEntry(dispatchOrder, gst12LedgerId, "Order", particular, dispatchOrder.OneTwo, null, ledgerID, taxVoucherTypeId);
                                    entry.Date = dispatchOrder.Deliverydate;
                                    result.Add(entry);
                                }

                                if (dispatchOrder.OneFour != null && dispatchOrder.OneFour > 0)
                                {
                                    particular = "GST14 credited regarding Order " + dispatchOrder.OrderId;
                                    entry = CreateLadgerEntry(dispatchOrder, ledgerID, "Order", particular, null, dispatchOrder.OneFour, gst14LedgerId, taxVoucherTypeId);
                                    entry.Date = dispatchOrder.Deliverydate;
                                    result.Add(entry);

                                    particular = "GST14 debited regarding Order " + dispatchOrder.OrderId;
                                    entry = CreateLadgerEntry(dispatchOrder, gst14LedgerId, "Order", particular, dispatchOrder.OneFour, null, ledgerID, taxVoucherTypeId);
                                    entry.Date = dispatchOrder.Deliverydate;
                                    result.Add(entry);
                                }

                                if (dispatchOrder.OneEight != null && dispatchOrder.OneEight > 0)
                                {
                                    particular = "GST18 credited regarding Order " + dispatchOrder.OrderId;
                                    entry = CreateLadgerEntry(dispatchOrder, ledgerID, "Order", particular, null, dispatchOrder.OneEight, gst18LedgerId, taxVoucherTypeId);
                                    entry.Date = dispatchOrder.Deliverydate;
                                    result.Add(entry);

                                    particular = "GST18 debited regarding Order " + dispatchOrder.OrderId;
                                    entry = CreateLadgerEntry(dispatchOrder, gst18LedgerId, "Order", particular, dispatchOrder.OneEight, null, ledgerID, taxVoucherTypeId);
                                    entry.Date = dispatchOrder.Deliverydate;
                                    result.Add(entry);
                                }

                                if (dispatchOrder.TwoEight != null && dispatchOrder.TwoEight > 0)
                                {
                                    particular = "GST28 credited regarding Order " + dispatchOrder.OrderId;
                                    entry = CreateLadgerEntry(dispatchOrder, ledgerID, "Order", particular, null, dispatchOrder.TwoEight, gst28LedgerId, taxVoucherTypeId);
                                    entry.Date = dispatchOrder.Deliverydate;
                                    result.Add(entry);

                                    particular = "GST28 debited regarding Order " + dispatchOrder.OrderId;
                                    entry = CreateLadgerEntry(dispatchOrder, gst28LedgerId, "Order", particular, dispatchOrder.TwoEight, null, ledgerID, taxVoucherTypeId);
                                    entry.Date = dispatchOrder.Deliverydate;
                                    result.Add(entry);
                                }

                                if (dispatchOrder.CessTaxAmount != null && dispatchOrder.CessTaxAmount > 0)
                                {
                                    particular = "Cess Tax credited regarding Order " + dispatchOrder.OrderId;
                                    entry = CreateLadgerEntry(dispatchOrder, ledgerID, "Order", particular, null, dispatchOrder.CessTaxAmount, cessTaxLedgerId, taxVoucherTypeId);
                                    entry.Date = dispatchOrder.Deliverydate;
                                    result.Add(entry);

                                    particular = "Cess Tax debited regarding Order " + dispatchOrder.OrderId;
                                    entry = CreateLadgerEntry(dispatchOrder, cessTaxLedgerId, "Order", particular, dispatchOrder.CessTaxAmount, null, ledgerID, taxVoucherTypeId);
                                    entry.Date = dispatchOrder.Deliverydate;
                                    result.Add(entry);
                                }

                            }


                            if (isSattled)
                            {

                                if (dispatchOrder.BillDiscountAmount != null && dispatchOrder.BillDiscountAmount > 0)
                                {
                                    particular = "Bill Discount Amount credited regarding Order " + dispatchOrder.OrderId;
                                    entry = CreateLadgerEntry(dispatchOrder, ledgerID, "Order", particular, dispatchOrder.BillDiscountAmount, null, billDiscountLedgerId, journalVoucherTypeId);
                                    entry.Date = dispatchOrder.Deliverydate;
                                    result.Add(entry);

                                    particular = "Bill Discount Amount debited regarding Order " + dispatchOrder.OrderId;
                                    entry = CreateLadgerEntry(dispatchOrder, billDiscountLedgerId, "Order", particular, null, dispatchOrder.BillDiscountAmount, ledgerID, journalVoucherTypeId);
                                    entry.Date = dispatchOrder.Deliverydate;
                                    result.Add(entry);
                                }

                                CashEntry(result, group, ledgerID, cashLedgerId, receiptVoucherTypeId, isTakeTransactionFromPaymentResponseRetailerApps);

                                chequeEntry(result, group, ledgerID, bankLedgerId, receiptVoucherTypeId, isTakeTransactionFromPaymentResponseRetailerApps);

                                //ElectronicEntry(result, group, ledgerID, electronicLedgerId, receiptVoucherTypeId, isTakeTransactionFromPaymentResponseRetailerApps);
                                HDFCEntry(result, group, ledgerID, hdfcLedgerId, receiptVoucherTypeId, isTakeTransactionFromPaymentResponseRetailerApps);

                                EpaylaterEntry(result, group, ledgerID, epayLaterId, receiptVoucherTypeId, isTakeTransactionFromPaymentResponseRetailerApps);

                                TruePayEntry(result, group, ledgerID, truePayLedgerID, receiptVoucherTypeId, isTakeTransactionFromPaymentResponseRetailerApps);



                                MPosEntry(result, group, ledgerID, mPosLedgerId, receiptVoucherTypeId, isTakeTransactionFromPaymentResponseRetailerApps);

                                if (dispatchOrder.DiscountAmount != null && dispatchOrder.DiscountAmount > 0)
                                {
                                    particular = "DiscountAmount credited regarding Order " + dispatchOrder.OrderId;
                                    entry = CreateLadgerEntry(dispatchOrder, ledgerID, "Order", particular, dispatchOrder.DiscountAmount, null, postDiscountLedgerId, journalVoucherTypeId);
                                    entry.Date = dispatchOrder.Deliverydate;
                                    result.Add(entry);

                                    particular = "DiscountAmount debited regarding Order " + dispatchOrder.OrderId;
                                    entry = CreateLadgerEntry(dispatchOrder, postDiscountLedgerId, "Order", particular, null, dispatchOrder.DiscountAmount, ledgerID, journalVoucherTypeId);
                                    entry.Date = dispatchOrder.Deliverydate;
                                    result.Add(entry);
                                }

                                if (dispatchOrder.WalletAmount != null && dispatchOrder.WalletAmount > 0)
                                {
                                    particular = "wallet point credited regarding Order " + dispatchOrder.OrderId;
                                    entry = CreateLadgerEntry(dispatchOrder, ledgerID, "Order", particular, dispatchOrder.WalletAmount, null, walletLedgerId, journalVoucherTypeId);
                                    entry.Date = dispatchOrder.Deliverydate;
                                    result.Add(entry);

                                    particular = "wallet point debited regarding Order " + dispatchOrder.OrderId;
                                    entry = CreateLadgerEntry(dispatchOrder, walletLedgerId, "Order", particular, null, dispatchOrder.WalletAmount, ledgerID, journalVoucherTypeId);
                                    entry.Date = dispatchOrder.Deliverydate;
                                    result.Add(entry);
                                }
                            }


                        }


                    }

                    ledgerListToSave = result.ToList();

                    if (ledgerListToSave != null && ledgerListToSave.Any())
                    {
                        authContext.LadgerEntryDB.AddRange(ledgerListToSave);
                        authContext.Commit();
                    }
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


        public bool CustomerUploadDocSMS(string Mobile, AuthContext context, String swadisthOilSMS)
        {
            try
            {
                Customer cust = null;
                cust = context.Customers.FirstOrDefault(x => x.Mobile == Mobile);
                if (cust != null)
                {

                    string OtpMessage = HttpUtility.UrlEncode(swadisthOilSMS);
                    string CountryCode = "91";
                    string Sender = "TRADKK";
                    string authkey = Startup.smsauthKey;
                    int route = 4;

                    var queryString = "?authkey=" + authkey + "&mobiles=" + Mobile + "&message= " + OtpMessage + " &sender=" + Sender + "&route=" + route + "&country=" + CountryCode;
                    string sendWithKnowlarity = ConfigurationManager.AppSettings["SendMsgFromKnowlarity"];
                    string path = sendWithKnowlarity == "true"
                                            ? ConfigurationManager.AppSettings["KnowlaritySMSUrl"].Replace("[[Mobile]]", Mobile).Replace("[[Message]]", OtpMessage)
                                            : "http://bulksms.newrise.in/api/sendhttp.php" + queryString;
                    var webRequest = (HttpWebRequest)WebRequest.Create(path);
                    webRequest.Method = "GET";
                    webRequest.ContentType = "application/json";
                    webRequest.UserAgent = "Mozilla/5.0 (Windows NT 5.1; rv:28.0) Gecko/20100101 Firefox/28.0";
                    webRequest.ContentLength = 0;
                    webRequest.Credentials = CredentialCache.DefaultCredentials;
                    webRequest.Accept = "*/*";
                    var webResponse = (HttpWebResponse)webRequest.GetResponse();
                    if (webResponse.StatusCode != HttpStatusCode.OK) Console.WriteLine("{0}", webResponse.Headers);
                    CustomerSms sms = new CustomerSms();
                    sms.CustomerId = cust.CustomerId;
                    sms.IsSMSSend = true;
                    sms.CreatedDate = DateTime.Now;
                    context.CustomerSmsDB.Add(sms);
                    context.Commit();
                }

            }
            catch (Exception ex)
            {

            }

            return true;

        }


        private static void EpaylaterEntry(ConcurrentBag<LadgerEntry> result, IGrouping<int, CustomerLedgerEntryModel> group, long ledgerID, long epayLaterLedgerId, long? receiptVoucherTypeId, Boolean isTakeTransactionFromPaymentResponseRetailerApps)
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

        private static void HDFCEntry(ConcurrentBag<LadgerEntry> result, IGrouping<int, CustomerLedgerEntryModel> group, long ledgerID, long hdfcLedgerId, long? receiptVoucherTypeId, Boolean isTakeTransactionFromPaymentResponseRetailerApps)
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

        private static void TruePayEntry(ConcurrentBag<LadgerEntry> result, IGrouping<int, CustomerLedgerEntryModel> group, long ledgerID, long truepayLedgerId, long? receiptVoucherTypeId, Boolean isTakeTransactionFromPaymentResponseRetailerApps)
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

        public long GetTruePayLedgerID(AuthContext authContext)
        {
            var query = from l in authContext.LadgerDB
                        join lt in authContext.LadgerTypeDB
                        on l.LadgertypeID equals lt.ID
                        where lt.code == "truepay"
                        select l.ID;
            var truePayLedgerId = query.FirstOrDefault();
            return truePayLedgerId;
        }

        public long GetHDFCLedgerID(AuthContext authContext)
        {
            var query = from l in authContext.LadgerDB
                        join lt in authContext.LadgerTypeDB
                        on l.LadgertypeID equals lt.ID
                        where lt.code == "hdfc"
                        select l.ID;
            var truePayLedgerId = query.FirstOrDefault();
            return truePayLedgerId;
        }

        public long GetEpaylaterLedgerID(AuthContext authContext)
        {
            var query = from l in authContext.LadgerDB
                        join lt in authContext.LadgerTypeDB
                        on l.LadgertypeID equals lt.ID
                        where lt.code == "epaylater"
                        select l.ID;
            var truePayLedgerId = query.FirstOrDefault();
            return truePayLedgerId;
        }

        public bool FaydaLedgerEntry(int CustomerId, int UserId, int OrderId, decimal Amount, DateTime date)
        {
            bool result = false;
            using (TransactionScope transactionScope = new TransactionScope())
            {
                using (var authContext = new AuthContext())
                {

                    //var ledgerList = authContext.LadgerDB.ToList();
                    var CustomerIdParam = new SqlParameter("@CutomerID", CustomerId);
                    var OrderIdParam = new SqlParameter("@OrderID", OrderId);
                    var AmountParam = new SqlParameter("@Amount", Amount);
                    var UserIdParam = new SqlParameter("@UserID", UserId);
                    var dateParam = new SqlParameter("@Date", date);

                    string spName = "spFaydaEntries";


                    spName = spName + " @UserID,@OrderID,@Date,@CutomerID,@Amount";
                    int i = authContext.Database.ExecuteSqlCommand(spName, UserIdParam, OrderIdParam, dateParam, CustomerIdParam, AmountParam);

                    if (i > 0)
                    {
                        authContext.SaveChanges();
                        transactionScope.Complete();
                        result = true;
                    }
                }
            }
            return result;
        }

    }
    public class CustomerLedgerEntryModel
    {
        public int CustomerId { get; set; }
        public string Skcode { get; set; }
        public int OrderId { get; set; }
        public DateTime? OrderedDate { get; set; }
        public DateTime? Deliverydate { get; set; }
        public double? TotalAmount { get; set; }
        public double? TaxAmount { get; set; }
        public double? CessTaxAmount { get; set; }
        public double? DiscountAmount { get; set; }
        public double? CashAmount { get; set; }
        public string CheckNo { get; set; }
        public double? CheckAmount { get; set; }
        public string ElectronicPaymentNo { get; set; }
        public double? ElectronicAmount { get; set; }
        public double? Zero { get; set; }
        public double? Five { get; set; }
        public double? OneTwo { get; set; }
        public double? OneFour { get; set; }
        public double? OneEight { get; set; }
        public double? TwoEight { get; set; }
        public double? DeliveryCharge { get; set; }
        public double? BillDiscountAmount { get; set; }
        public double? WalletAmount { get; set; }
        public double? Amount { get; set; }
        public string PaymentFrom { get; set; }
        public string Gatewaytransid { get; set; }

        public string InvoiceNo { get; set; }
    }
    public class LedgerVM
    {
        public long? ID { get; set; }
        public string Name { get; set; }
        public string Alias { get; set; }
        public long? GroupID { get; set; }
        public bool InventoryValuesAreAffected { get; set; }
        public string Address { get; set; }
        public string Country { get; set; }
        public int? PinCode { get; set; }
        public bool? ProvidedBankDetails { get; set; }
        public string PAN { get; set; }
        public string RegistrationType { get; set; }
        public string GSTno { get; set; }
        public long? ObjectID { get; set; }
        public string ObjectType { get; set; }
        public int? LadgertypeID { get; set; }
        public bool Active { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public int? UpdatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public string WarehouseName { get; set; }
        public int WarehouseId { get; set; }

        public decimal SupplierGRIRDifference { get; set; }
    }
    class LedgerHeaderViewModel
    {
        public string Name1 { get; set; }
        public string Val1 { get; set; }
        public string Name2 { get; set; }
        public string Val2 { get; set; }
    }
    class POInformationDC
    {
        public string Name1 { get; set; }
        public string Val1 { get; set; }       
    }
    class SupplierInformationDC
    {
        public string Name2 { get; set; }
        public string Val2 { get; set; }
    }
    class DepoInformationDC
    {
        public string Name3 { get; set; }
        public string Val3 { get; set; }
    }
    class WareHouseInformationDC
    {
        public string Name4 { get; set; }
        public string Val4 { get; set; }
    }
}