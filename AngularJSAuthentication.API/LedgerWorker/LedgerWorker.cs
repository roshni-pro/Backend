using AngularJSAuthentication.API.ControllerV7;
using AngularJSAuthentication.API.NewHelper;
using AngularJSAuthentication.Common.Helpers;
using AngularJSAuthentication.Model.Account;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AngularJSAuthentication.API
{
    public class LedgerWorker
    {
        public virtual LedgerHistoryViewModel GetLedger(LedgerInputViewModel viewModel)
        {
            LedgerHistoryViewModel vm = new LedgerHistoryViewModel();
            vm.LadgerItem = GetLadgerItem(viewModel.LedgerID, viewModel.LedgerTypeID);
            vm.OpeningBalance = GetOpeningBalance(viewModel.FromDate, viewModel.LedgerID);
            vm.LadgerEntryList = GetLedgerEntryList(viewModel.ToDate, viewModel.FromDate, viewModel.LedgerID, vm.OpeningBalance);
            vm.LadgerEntryList = GetByReportCode(viewModel.ReportCode, vm.LadgerEntryList);

            //if (vm.LadgerEntryList != null && vm.LadgerEntryList.Count > 0)
            //{
            //    vm.LadgerEntryList = vm.LadgerEntryList.OrderBy(x => x.Date).ThenBy(y => y.ObjectID).ThenBy(z => z.Credit).ToList();
            //}

            UpdateBalance(vm.LadgerEntryList, vm.OpeningBalance ?? 0);

            return vm;
        }
        public virtual LedgerVM GetLadgerItem(long ledgerID, int ledgerTypeID)
        {
            using (var context = new AuthContext())
            {
                var ladgerQuery = from le in context.LadgerDB
                                  where le.ID == ledgerID && le.LadgertypeID == ledgerTypeID
                                  select new LedgerVM
                                  {
                                      ID = le.ID,
                                      Name = le.Name,
                                      Alias = le.Alias,
                                      GroupID = le.GroupID,
                                      InventoryValuesAreAffected = le.InventoryValuesAreAffected,
                                      Address = le.Address,
                                      Country = "India",
                                      PinCode = null, //(!string.IsNullOrEmpty(c.ZipCode)) ?int.Parse(c.ZipCode): null,
                                      ProvidedBankDetails = le.ProvidedBankDetails,
                                      PAN = le.PAN,
                                      RegistrationType = le.RegistrationType,
                                      GSTno = le.GSTno,
                                      ObjectID = le.ObjectID,
                                      ObjectType = le.ObjectType,
                                      LadgertypeID = le.LadgertypeID
                                  };
                var ledger = ladgerQuery.FirstOrDefault();
                return ledger;
            }

        }

        public virtual double GetOpeningBalance(DateTime fromDate, long ledgerID)
        {
            using (var context = new AuthContext())
            {
                double? credit = context.LadgerEntryDB.Where(x => x.Date.Value < fromDate.Date && x.LagerID == ledgerID).Sum(x => x.Credit);
                double? debit = context.LadgerEntryDB.Where(x => x.Date.Value < fromDate.Date && x.LagerID == ledgerID).Sum(x => x.Debit);


                double openingBalance = 0.0;
                LedgerOpeningBalance ledgerOpeningBalance = null; //context.LedgerOpeningBalanceDB.Where(x => x.LedgerID == ledgerID).FirstOrDefault();

                if (ledgerOpeningBalance != null)
                {
                    openingBalance = ledgerOpeningBalance.OpeningBalance.HasValue ? (double)ledgerOpeningBalance.OpeningBalance.Value : 0;

                }

                openingBalance = (debit ?? 0) - (credit ?? 0);
                openingBalance = Math.Round(openingBalance, 2);
                return openingBalance;
            }


        }

        public virtual List<LadgerEntryVM> GetLedgerEntryList(DateTime toDate, DateTime fromDate, long ledgerID, double? openingBalance)
        {
            using (var context = new AuthContext())
            {
                //toDate = toDate.Date.AddDays(1);
                var query = from le in context.LadgerEntryDB
                            join l in context.LadgerDB
                            on le.LagerID equals l.ID
                            join ale in context.LadgerDB
                            on le.AffectedLadgerID equals ale.ID
                            join v in context.VoucherTypeDB
                            on le.VouchersTypeID equals v.ID
                            join lt in context.LadgerTypeDB
                            on ale.LadgertypeID equals lt.ID
                            where (
                                le.LagerID == ledgerID
                                && le.Date >= fromDate.Date
                                && le.Date <= toDate
                            )
                            select new LadgerEntryVM
                            {
                                ID = le.ID,
                                Date = le.Date,
                                Particulars = le.Particulars,
                                LagerID = le.LagerID,
                                VouchersTypeID = le.VouchersTypeID,
                                VouchersNo = le.VouchersNo,
                                Debit = le.Debit,
                                Credit = le.Credit,
                                ObjectID = le.ObjectID.HasValue ? le.ObjectID.Value.ToString() : "",
                                ObjectType = le.ObjectType,
                                AffectedLadgerID = le.AffectedLadgerID,
                                Active = le.Active,
                                CreatedBy = le.CreatedBy,
                                CreatedDate = le.CreatedDate,
                                UpdatedBy = le.UpdatedBy,
                                UpdatedDate = le.UpdatedDate,
                                LadgerName = l.Name,
                                VoucherName = v.Name,
                                AffactedLadgerName = ale.Name,
                                LedgerTypeSequence = lt.Sequence,
                                RefNo = le.RefNo
                            };


                List<LadgerEntryVM> ladgerEntryList = query.OrderBy(x => x.Date.Value.Year).ThenBy(x => x.Date.Value.Month).ThenBy(x => x.Date.Value.Day).ThenBy(x => x.Date.Value.Hour).ThenBy(x => x.Date.Value.Minute).ThenBy(x => x.LedgerTypeSequence).ToList<LadgerEntryVM>();



                return ladgerEntryList;
            }
            //context.Database.Log = s => Debug.WriteLine(s);

        }

        public virtual void UpdateBalance(List<LadgerEntryVM> ladgerEntryVMList, double openingBalance)
        {
            if (ladgerEntryVMList != null && ladgerEntryVMList.Count > 0)
            {
                foreach (var ladgerEntry in ladgerEntryVMList)
                {
                    openingBalance = openingBalance + (ladgerEntry.Debit ?? 0) - (ladgerEntry.Credit ?? 0);
                    ladgerEntry.DayBalance = Math.Truncate(openingBalance * 100) / 100; ;
                }
            }
        }

        public virtual List<LadgerEntryVM> GetByReportCode(string reportCode, List<LadgerEntryVM> ladgerEntryList)
        {
            return ladgerEntryList;
        }

        public virtual void UpdateTableToPrint(List<LadgerEntryVM> ledgerEntryList)
        {
            if (ledgerEntryList != null)
            {
                CultureInfoHelper cultureInfoHelper = new CultureInfoHelper();
                foreach (var item in ledgerEntryList)
                {
                    item.PrintDate = item.Date.HasValue ? item.Date.Value.ToString("dd/MM/yyyy") : "";
                    item.CreditString = cultureInfoHelper.GetIndianCurrencyString(item.Credit);
                    item.DebitString = cultureInfoHelper.GetIndianCurrencyString(item.Debit);
                    item.DayBalanceString = cultureInfoHelper.GetIndianCurrencyStringWithCreditDebit(item.DayBalance);
                }
            }
            return;
        }

        public virtual void UpdateTableToPrintRaw(List<LadgerEntryVM> ledgerEntryList)
        {
            if (ledgerEntryList != null)
            {
                CultureInfoHelper cultureInfoHelper = new CultureInfoHelper();
                foreach (var item in ledgerEntryList)
                {
                    item.PrintDate = item.Date.HasValue ? item.Date.Value.ToString("dd/MM/yyyy") : "";
                    item.CreditString = item.Credit.ToString();
                    item.DebitString = item.Debit.ToString();
                    item.DayBalanceString = item.DayBalance.ToString();


                }
            }
            return;
        }



        public virtual void UpdateOpeningAndClosingBalance(LedgerHistoryViewModel vm)
        {
            CultureInfoHelper cultureInfoHelper = new CultureInfoHelper();
            vm.OpeningBalanceString = cultureInfoHelper.GetIndianCurrencyStringWithCreditDebit(vm.OpeningBalance);
            vm.ClosingBalanceString = cultureInfoHelper.GetIndianCurrencyStringWithCreditDebit(vm.ClosingBalance);
        }

    }
}