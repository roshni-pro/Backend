using AngularJSAuthentication.API.ControllerV7;
using AngularJSAuthentication.API.NewHelper;
using AngularJSAuthentication.Model.Account;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;

namespace AngularJSAuthentication.API
{
    public class CustomerLedgerWorker : LedgerWorker
    {
        public override LedgerVM GetLadgerItem(long ledgerID, int ledgerTypeID)
        {
            try
            {
                using (var context = new AuthContext())
                {
                    var ladgerQuery = from le in context.LadgerDB
                                      join c in context.Customers
                                      on le.ObjectID equals c.CustomerId
                                      join w in context.Warehouses
                                      on c.Warehouseid equals w.WarehouseId
                                      where le.ID == ledgerID && le.LadgertypeID == ledgerTypeID
                                      select new LedgerVM
                                      {
                                          ID = le.ID,
                                          Name = c.ShopName,
                                          Alias = c.Skcode,
                                          GroupID = le.GroupID,
                                          InventoryValuesAreAffected = le.InventoryValuesAreAffected,
                                          Address = c.ShippingAddress,
                                          Country = c.Country,
                                          PinCode = null, //(!string.IsNullOrEmpty(c.ZipCode)) ?int.Parse(c.ZipCode): null,
                                          ProvidedBankDetails = le.ProvidedBankDetails,
                                          PAN = c.PanNo,
                                          RegistrationType = le.RegistrationType,
                                          GSTno = c.RefNo,
                                          ObjectID = le.ObjectID,
                                          ObjectType = le.ObjectType,
                                          LadgertypeID = le.LadgertypeID,
                                          WarehouseName = w.WarehouseName,
                                          WarehouseId = w.WarehouseId
                                      };
                    var ledger = ladgerQuery.FirstOrDefault();
                    return ledger;
                }
            }
            catch (Exception ex)
            {
                return null;
            }

        }

        public override LedgerHistoryViewModel GetLedger(LedgerInputViewModel viewModel)
        {
            LedgerHistoryViewModel vm = new LedgerHistoryViewModel();
            vm.LadgerItem = GetLadgerItem(viewModel.LedgerID, viewModel.LedgerTypeID);
            vm.OpeningBalance = GetOpeningBalance(viewModel.FromDate, viewModel.LedgerID);
            //vm.LadgerEntryList = GetLedgerEntryList(viewModel.ToDate, viewModel.FromDate, viewModel.LedgerID, vm.OpeningBalance);
            //vm.LadgerEntryList = GetByReportCode(viewModel.ReportCode, vm.LadgerEntryList);

            if (viewModel.ReportCode == "SR")
            {
                vm.LadgerEntryList = GetLedgerEntryListSP(viewModel.ToDate, viewModel.FromDate, viewModel.LedgerID, vm.OpeningBalance, 1);
            }
            else if (viewModel.ReportCode == "DR")
            {
                vm.LadgerEntryList = GetLedgerEntryListSP(viewModel.ToDate, viewModel.FromDate, viewModel.LedgerID, vm.OpeningBalance, 0);
            }
            else
            {
                vm.LadgerEntryList = GetLedgerEntryList(viewModel.ToDate, viewModel.FromDate, viewModel.LedgerID, vm.OpeningBalance);
                vm.LadgerEntryList = GetByReportCode(viewModel.ReportCode, vm.LadgerEntryList);
            }


            if (vm.LadgerEntryList != null && vm.LadgerEntryList.Count > 0)
            {
                //vm.LadgerEntryList = vm.LadgerEntryList.OrderBy(x => x.Date).ThenBy(y => y.ObjectID).ThenBy(z => z.Credit).ToList();
            }



            UpdateBalance(vm.LadgerEntryList, vm.OpeningBalance ?? 0);

            vm.ClosingBalance = 0;
            if (vm.LadgerEntryList != null && vm.LadgerEntryList.Count > 0)
            {
                vm.ClosingBalance = vm.LadgerEntryList.Last().DayBalance;
            }
            vm.ClosingBalance = 0;
            if (vm.LadgerEntryList != null && vm.LadgerEntryList.Count > 0)
            {
                vm.ClosingBalance = vm.LadgerEntryList.Last().DayBalance;
            }

            vm.TotalCredit = 0;
            vm.TotalDebit = 0;
            foreach (var ledger in vm.LadgerEntryList)
            {
                if (ledger.Credit != null)
                {
                    vm.TotalCredit = vm.TotalCredit + ledger.Credit ?? 0;
                }
                if (ledger.Debit != null)
                {
                    vm.TotalDebit = vm.TotalDebit + ledger.Debit ?? 0;
                }
            }



            return vm;
        }

        public override List<LadgerEntryVM> GetByReportCode(string reportCode, List<LadgerEntryVM> ladgerEntryList)
        {
            if (ladgerEntryList != null && ladgerEntryList.Count > 0)
            {
                if (reportCode == "DR")
                {
                    return ladgerEntryList;
                }
                else if (reportCode == "MT")
                {

                    List<LadgerEntryVM> newList = new List<LadgerEntryVM>();
                    var groupList = ladgerEntryList.GroupBy(x => x.ObjectID).ToList();
                    foreach (var group in groupList)
                    {
                        var JournalAmt = group.Where(x => x.VoucherName == "Journal" && x.Credit > 0).ToList()?.Sum(x => x.Credit);
                        //---S---06/11/2022-------------------------Add New COde: Remove Refund Amt--------
                        var JournalAmtDr = group.Where(x => x.VoucherName == "Journal" && x.Debit > 0).ToList()?.Sum(x => x.Debit);
                        var RefundAmt = group.Where(x => x.VoucherName == "Refund" && x.Debit > 0).ToList()?.Sum(x => x.Debit);
                        var ReceiptAmt = group.Where(x => x.VoucherName == "Receipt" && x.Credit > 0).ToList()?.Sum(x => x.Credit);
                        if (((RefundAmt == ReceiptAmt) || (RefundAmt <= ReceiptAmt)) && ((RefundAmt + ReceiptAmt) > 0))//&& JournalAmt>0
                        {
                            //var DrJournalAmt = group.Where(x => x.VoucherName == "Journal" && x.Debit > 0).ToList()?.Sum(x => x.Debit);
                            //if ((JournalAmt!=DrJournalAmt)) { JournalAmt = 0; }
                        }
                        else
                        { RefundAmt = 0; }
                        //---E---06/11/2022------------------------Add New COde: Remove Refund Amt--------
                        var Refund = group.FirstOrDefault(x => x.VoucherName == "Refund");
                        LadgerEntryVM salesLedgerEntry = group.FirstOrDefault(x => x.VoucherName == "Sales");
                        if (salesLedgerEntry != null)
                        {
                            salesLedgerEntry.Debit = group.Where(x => x.Debit > 0).ToList()?.Sum(x => x.Debit);
                            //salesLedgerEntry.Debit = group.Where(x => x.Debit > 0 && x.VoucherName != "Journal").ToList()?.Sum(x => x.Debit);
                            //---S---06/11/2022----------------Add New COde: Remove Refund Amt--------
                            //--------Old Code------------
                            //salesLedgerEntry.Debit = salesLedgerEntry.Debit - JournalAmt
                            if (RefundAmt > 0)
                            { salesLedgerEntry.Debit = salesLedgerEntry.Debit - (JournalAmt + RefundAmt); }
                            else
                            { salesLedgerEntry.Debit = salesLedgerEntry.Debit - JournalAmt; }
                            //---E---06/11/2022---------------Add New COde: Remove Refund Amt--------------
                            newList.Add(salesLedgerEntry);
                        }
                        //---S---06/11/2022---------------Add New COde: While Sales ENtry not Exist and only Refund+Recipt Entry Exist--------------                        
                        else if (salesLedgerEntry == null && Refund !=null && Refund.VoucherName == "Refund")
                        {
                            salesLedgerEntry = group.FirstOrDefault(x => x.VoucherName == "Refund");
                            if (salesLedgerEntry != null)
                            {
                                salesLedgerEntry.VoucherName = "Sales";
                                salesLedgerEntry.Debit = group.Where(x => x.Debit > 0).ToList()?.Sum(x => x.Debit);
                                if (RefundAmt > 0)
                                { salesLedgerEntry.Debit = salesLedgerEntry.Debit - RefundAmt; }
                                else
                                { salesLedgerEntry.Debit = salesLedgerEntry.Debit - JournalAmt; }

                                newList.Add(salesLedgerEntry);
                            }
                        }
                        //---E---06/11/2022---------------Add New COde: While Sales ENtry not Exist and only Refund+Recipt Entry Exist--------------

                        var creditList = group.Where(x => x.Credit > 0 && x.VoucherName != "Journal").ToList();
                        LadgerEntryVM creditLedLadgerEntry = null;
                        if (creditList != null && creditList.Count > 0)
                        {

                            creditLedLadgerEntry = creditList.First();
                            creditLedLadgerEntry.Credit = creditList.Where(x => x.Credit > 0).ToList()?.Sum(x => x.Credit);
                            //---S---06/11/2022-------Add New COde: Remove Refund Amt--------
                            //creditLedLadgerEntry.Credit = creditLedLadgerEntry.Credit + JournalAmt
                            if (RefundAmt == 0)
                            {
                                { creditLedLadgerEntry.Credit = creditLedLadgerEntry.Credit; }
                            }
                            else
                            { //if ((JournalAmtDr - JournalAmt) == 0) 
                                //Hint: RefundAmt>0 - Add this condtion for Order No: 2237776, Because Refund Amount not Deduct. 
                                if ((JournalAmtDr - JournalAmt) == 0 || RefundAmt > 0) //new added (||RefundAmt>0) on 20-06-2024 becoz refund was not added on credit due to this (JournalAmtDr - JournalAmt) == 0
                                //{ creditLedLadgerEntry.Credit = creditLedLadgerEntry.Credit - (JournalAmt + RefundAmt); }
                                //else
                                { creditLedLadgerEntry.Credit = creditLedLadgerEntry.Credit - RefundAmt; }
                            }
                            //else
                            //{ if ((JournalAmtDr - JournalAmt) == 0)
                            //    //{ creditLedLadgerEntry.Credit = creditLedLadgerEntry.Credit - (JournalAmt + RefundAmt); }
                            //    //else
                            //    { creditLedLadgerEntry.Credit = creditLedLadgerEntry.Credit -  RefundAmt; }
                            //}




                            //---E---06/11/2022-------Add New COde: Remove Refund Amt--------
                            creditLedLadgerEntry.AffactedLadgerName = "Receipt";
                            newList.Add(creditLedLadgerEntry);
                        }


                        if (salesLedgerEntry != null && creditLedLadgerEntry != null && (salesLedgerEntry.Debit - creditLedLadgerEntry.Credit) < 2 && (salesLedgerEntry.Debit - creditLedLadgerEntry.Credit) > -2)
                        {
                            salesLedgerEntry.ColorCode = "Green";
                            creditLedLadgerEntry.ColorCode = "Green";
                        }
                        else
                        {
                            if (salesLedgerEntry != null)
                            {
                                salesLedgerEntry.ColorCode = "Red";
                            }
                            if (creditLedLadgerEntry != null)
                            {
                                creditLedLadgerEntry.ColorCode = "Red";
                            }
                        }
                    }

                    return newList;
                }
                else if (reportCode == "SR")
                {
                    List<LadgerEntryVM> newList = new List<LadgerEntryVM>();
                    var groupList = ladgerEntryList.GroupBy(x => x.ObjectID).ToList();
                    foreach (var group in groupList)
                    {
                        LadgerEntryVM salesLedgerEntry = group.FirstOrDefault(x => x.VoucherName == "Sales");
                        if (salesLedgerEntry != null)
                        {
                            salesLedgerEntry.Debit = group.Where(x => x.Debit > 0).ToList()?.Sum(x => x.Debit);
                            newList.Add(salesLedgerEntry);
                        }
                        var creditList = group.Where(x => x.Credit > 0).ToList();
                        if (creditList != null && creditList.Count > 0)
                        {
                            newList.AddRange(creditList);
                        }

                    }
                    return newList;
                }
                else
                {
                    return ladgerEntryList;

                }
            }
            else
            {
                return ladgerEntryList;
            }
        }


        public List<LadgerEntryVM> GetLedgerEntryListSP(DateTime toDate, DateTime fromDate, long ledgerID, double? openingBalance, int isSummaryReportGet)
        {
            using (var context = new AuthContext())
            {
                //toDate = toDate.Date.AddDays(1);
                var fromDateParam = new SqlParameter("@fromDate", fromDate.Date);
                var toDateParam = new SqlParameter("@toDate", toDate.Date);
                var ledgerIDParam = new SqlParameter("@ledgerID", ledgerID);
                var isSummaryReportGetParam = new SqlParameter("@isSummaryReportGet", isSummaryReportGet);
                List<LadgerEntryVM> ladgerEntryList = context.Database.SqlQuery<LadgerEntryVM>("CustomerLedgerEntryGet @fromDate,@toDate,@ledgerID,@isSummaryReportGet", fromDateParam, toDateParam, ledgerIDParam, isSummaryReportGetParam).OrderBy(x => x.Date.Value.Year).ThenBy(x => x.Date.Value.Month).ThenBy(x => x.Date.Value.Day).ThenBy(x => x.Date.Value.Hour).ThenBy(x => x.Date.Value.Minute).ThenBy(x => x.ObjectID).ThenBy(x => x.LedgerTypeSequence).ToList<LadgerEntryVM>();
                return ladgerEntryList;
            }
        }



    }


}