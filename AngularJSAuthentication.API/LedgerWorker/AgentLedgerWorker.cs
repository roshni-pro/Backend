using AngularJSAuthentication.API.ControllerV7;
using AngularJSAuthentication.API.NewHelper;
using AngularJSAuthentication.Model.Account;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;

namespace AngularJSAuthentication.API
{
    public class AgentLedgerWorker : LedgerWorker
    {
        public override LedgerVM GetLadgerItem(long ledgerID, int ledgerTypeID)
        {
            using (var context = new AuthContext())
            {
                try
                {
                    var ladgerQuery = from le in context.LadgerDB
                                      join p in context.Peoples
                                      on le.ObjectID equals p.PeopleID
                                      where le.ID == ledgerID
                                      && le.LadgertypeID == ledgerTypeID
                                      select new LedgerVM
                                      {
                                          ID = le.ID,
                                          Name = le.Name,
                                          Alias = le.Alias,
                                          GroupID = le.GroupID,
                                          InventoryValuesAreAffected = le.InventoryValuesAreAffected,
                                          Address = p.city,
                                          Country = "India",
                                          PinCode = null, //(!string.IsNullOrEmpty(c.ZipCode)) ?int.Parse(c.ZipCode): null,
                                          ProvidedBankDetails = le.ProvidedBankDetails,
                                          PAN = "",
                                          RegistrationType = le.RegistrationType,
                                          GSTno = le.GSTno,
                                          ObjectID = le.ObjectID,
                                          ObjectType = le.ObjectType,
                                          LadgertypeID = le.LadgertypeID
                                      };
                    var ledger = ladgerQuery.FirstOrDefault();
                    return ledger;
                }
                catch (Exception ex)

                {
                    throw ex;
                }
            }
        }

        public override LedgerHistoryViewModel GetLedger(LedgerInputViewModel viewModel)
        {
            LedgerHistoryViewModel vm = new LedgerHistoryViewModel();
            vm.LadgerItem = GetLadgerItem(viewModel.LedgerID, viewModel.LedgerTypeID);
            vm.OpeningBalance = GetOpeningBalance(viewModel.FromDate, viewModel.LedgerID);
            vm.LadgerEntryList = GetLedgerEntryList(viewModel.ToDate, viewModel.FromDate, viewModel.LedgerID, vm.OpeningBalance);
            vm.LadgerEntryList = FilterList(viewModel.AgentReportType, vm.LadgerEntryList);
            vm.LadgerEntryList = GetByReportCode(viewModel.ReportCode, vm.LadgerEntryList);

            if (vm.LadgerEntryList != null && vm.LadgerEntryList.Count > 0)
            {
                vm.LadgerEntryList = vm.LadgerEntryList.OrderBy(x => x.Date).ThenBy(y => y.ObjectID).ThenBy(z => z.Credit).ToList();
            }

            UpdateBalance(vm.LadgerEntryList, vm.OpeningBalance ?? 0);


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

                        LadgerEntryVM salesLedgerEntry = group.FirstOrDefault(x => x.VoucherName == "Sales");
                        if (salesLedgerEntry != null)
                        {
                            salesLedgerEntry.Debit = group.Where(x => x.Debit > 0).ToList()?.Sum(x => x.Debit);
                            newList.Add(salesLedgerEntry);
                        }
                        var creditList = group.Where(x => x.Credit > 0).ToList();
                        LadgerEntryVM creditLedLadgerEntry = null;
                        if (creditList != null && creditList.Count > 0)
                        {

                            creditLedLadgerEntry = creditList.First();
                            creditLedLadgerEntry.Credit = creditList.Where(x => x.Credit > 0).ToList()?.Sum(x => x.Credit);
                            creditLedLadgerEntry.AffactedLadgerName = "Receipt + Discount + Wallet";
                            newList.Add(creditLedLadgerEntry);
                        }


                        if (salesLedgerEntry != null && creditLedLadgerEntry != null && (salesLedgerEntry.Debit - creditLedLadgerEntry.Credit) < 10 && (salesLedgerEntry.Debit - creditLedLadgerEntry.Credit) > -10)
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
                        LadgerEntryVM salesLedgerEntry = group.FirstOrDefault(x => x.VoucherName == "Purchase");
                        if (salesLedgerEntry != null)
                        {
                            salesLedgerEntry.Credit = group.Where(x => x.Credit > 0).ToList()?.Sum(x => x.Credit);
                            newList.Add(salesLedgerEntry);
                            var debitList = group.Where(x => x.Debit > 0).ToList();
                            if (debitList != null && debitList.Count > 0)
                            {
                                newList.AddRange(debitList);
                            }
                        }
                        else
                        {
                            newList.AddRange(group.ToList());
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

        public override List<LadgerEntryVM> GetLedgerEntryList(DateTime toDate, DateTime fromDate, long ledgerID, double? openingBalance)
        {
            using (var context = new AuthContext())
            {
                //toDate = toDate.Date.AddDays(1);

                var monthParam = new SqlParameter("@LedgerID", ledgerID);
                var yearParam = new SqlParameter("@FromDate", fromDate.Date);
                var Warehouse = new SqlParameter("@ToDate", toDate);
                var result = context.Database.SqlQuery<LadgerEntryVM>("AgentLedgerGet @LedgerID,@FromDate,@ToDate", monthParam, yearParam, Warehouse).ToList();

                return result;
            }
            //context.Database.Log = s => Debug.WriteLine(s);

        }


        private List<LadgerEntryVM> FilterList(string reportType, List<LadgerEntryVM> list)
        {
            using (var authContext = new AuthContext())
            {
                var transactionQuery = from l in authContext.LadgerDB
                                       join lt in authContext.LadgerTypeDB
                                       on l.LadgertypeID equals lt.ID
                                       where lt.code == "Transaction"
                                       select l;

                var agentCashQuery = from l in authContext.LadgerDB
                                     join lt in authContext.LadgerTypeDB
                                     on l.LadgertypeID equals lt.ID
                                     where lt.code == "AgentCash"
                                     select l;


                var commissionQuery = from l in authContext.LadgerDB
                                      join lt in authContext.LadgerTypeDB
                                      on l.LadgertypeID equals lt.ID
                                      where lt.code == "AgentCommission"
                                      select l;



                

                var transactionLadger = transactionQuery.FirstOrDefault();
                var agentCashLadger = agentCashQuery.FirstOrDefault();
                var commissionLadger = commissionQuery.FirstOrDefault();

                if (list != null && list.Count > 0)
                {

                    if (reportType == "TR")
                    {
                        return list.Where(x => x.LagerID == transactionLadger.ID || x.AffectedLadgerID == transactionLadger.ID || x.LagerID == agentCashLadger.ID || x.AffectedLadgerID == agentCashLadger.ID).ToList();
                    }
                    else if (reportType == "CO")
                    {
                        return list.Where(x => x.VoucherName == "Agent Payment" || x.LagerID == commissionLadger.ID || x.AffectedLadgerID == commissionLadger.ID).ToList();
                    }
                    else if (reportType == "FI")
                    {
                        return list;
                    }
                    else
                    {
                        return list;
                    }
                }
                else
                {
                    return null;
                }
            }

        }
    }
}