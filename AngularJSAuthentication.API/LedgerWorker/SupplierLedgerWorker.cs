using AngularJSAuthentication.API.ControllerV7;
using AngularJSAuthentication.API.NewHelper;
using AngularJSAuthentication.Common.Helpers;
using AngularJSAuthentication.Model.Account;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace AngularJSAuthentication.API
{
    public class SupplierLedgerWorker : LedgerWorker
    {
        public override LedgerVM GetLadgerItem(long ledgerID, int ledgerTypeID)
        {
            using (var context = new AuthContext())
            {
                var ladgerQuery = from le in context.LadgerDB
                                  join s in context.Suppliers
                                  on le.ObjectID equals s.SupplierId
                                  where le.ID == ledgerID && le.LadgertypeID == ledgerTypeID
                                  select new LedgerVM
                                  {
                                      ID = le.ID,
                                      Name = s.Name,
                                      Alias = s.SUPPLIERCODES,
                                      GroupID = le.GroupID,
                                      InventoryValuesAreAffected = le.InventoryValuesAreAffected,
                                      Address = s.BillingAddress,
                                      Country = "India",
                                      PinCode = null, //(!string.IsNullOrEmpty(c.ZipCode)) ?int.Parse(c.ZipCode): null,
                                      ProvidedBankDetails = le.ProvidedBankDetails,
                                      PAN = s.Pancard,
                                      RegistrationType = le.RegistrationType,
                                      GSTno = s.GstInNumber,
                                      ObjectID = le.ObjectID,
                                      ObjectType = le.ObjectType,
                                      LadgertypeID = le.LadgertypeID
                                  };
                var ledger = ladgerQuery.FirstOrDefault();
                try
                {
                    if (ledgerTypeID == 12)
                    {
                        var SupplierId = new SqlParameter
                        {
                            ParameterName = "SupplierId",
                            Value = ledger.ObjectID,

                        };



                        //db.Database.ExecuteSqlCommand("GullakCashBackCheck @WarehouseId,@StartDate,@EndDate,@AmountFrom,@AmountToOP out ", WarehouseId,
                        //          StartDate, EndDate, AmountFrom, AmountToop);

                        ledger.SupplierGRIRDifference = context.Database.SqlQuery<decimal>("GetSupplierGRIRDifference @SupplierId", SupplierId).FirstOrDefault();
                    }
                }
                catch (Exception ex)
                {

                }

                return ledger;
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
                vm.LadgerEntryList = GetLedgerEntryListSP(viewModel.ToDate, viewModel.FromDate, viewModel.LedgerID, viewModel.RefNo, vm.OpeningBalance, 1, viewModel.DepoId ?? 0);
            }
            else
            {
                vm.LadgerEntryList = GetLedgerEntryListSP(viewModel.ToDate, viewModel.FromDate, viewModel.LedgerID, viewModel.RefNo, vm.OpeningBalance, 0, viewModel.DepoId ?? 0);
            }

            //if (vm.LadgerEntryList != null && vm.LadgerEntryList.Count > 0)
            //{
            //    vm.LadgerEntryList = vm.LadgerEntryList.OrderBy(x => x.Date).ThenBy(y => y.ObjectID).ThenBy(z => z.Credit).ToList();
            //}

            UpdateBalance(vm.LadgerEntryList, vm.OpeningBalance ?? 0);


            vm.ClosingBalance = 0;
            if (vm.LadgerEntryList != null && vm.LadgerEntryList.Count > 0)
            {
                vm.ClosingBalance = vm.LadgerEntryList.Last().DayBalance;
            }
            vm.IRTotalSum = GetIRTotalsum(viewModel.LedgerID, viewModel.FromDate, viewModel.ToDate);
            vm.GRTotalSum = GetGRTotalsum(viewModel.LedgerID, viewModel.FromDate, viewModel.ToDate);
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

        public override List<LadgerEntryVM> GetLedgerEntryList(DateTime toDate, DateTime fromDate, long ledgerID, double? openingBalance)
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
                            join ir in context.VoucherDB
                            on le.VouchersNo equals ir.ID
                            into ps
                            from ir in ps.DefaultIfEmpty()
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
                                IRID = ir.Code,
                                LedgerTypeSequence = lt.Sequence
                            };


                List<LadgerEntryVM> ladgerEntryList = query.OrderBy(x => x.Date.Value.Year).ThenBy(x => x.Date.Value.Month).ThenBy(x => x.Date.Value.Day).ThenBy(x => x.Date.Value.Hour).ThenBy(x => x.Date.Value.Minute).ThenBy(x => x.LedgerTypeSequence).ToList<LadgerEntryVM>();



                return ladgerEntryList;
            }
            //context.Database.Log = s => Debug.WriteLine(s);

        }


        public List<LadgerEntryVM> GetLedgerEntryListSP(DateTime toDate, DateTime fromDate, long ledgerID, string RefNo, double? openingBalance, int isSummaryReportGet, int depoId = 0)
        {
            using (var context = new AuthContext())
            {
                try
                {
                    //toDate = toDate.Date.AddDays(1);
                    //var fromDateParam = new SqlParameter("fromDate", fromDate);
                    //var toDateParam = new SqlParameter("toDate", toDate);
                    //var ledgerIDParam = new SqlParameter("ledgerID", (long)ledgerID);
                    //var isSummaryReportGetParam = new SqlParameter("isSummaryReportGet", isSummaryReportGet);
                    //var depoIdParam = new SqlParameter("depoId", depoId);

                    var fromDateParam = new SqlParameter()
                    {
                        ParameterName = "@fromDate",
                        SqlDbType = SqlDbType.DateTime,
                        Value = fromDate,
                        Direction = System.Data.ParameterDirection.Input
                    };
                    var toDateParam = new SqlParameter()
                    {
                        ParameterName = "@toDate",
                        SqlDbType = SqlDbType.DateTime,
                        Value = toDate,
                        Direction = System.Data.ParameterDirection.Input
                    };
                    var ledgerIDParam = new SqlParameter()
                    {
                        ParameterName = "@ledgerID",
                        SqlDbType = SqlDbType.BigInt,
                        Value = ledgerID,
                        Direction = System.Data.ParameterDirection.Input
                    };
                    var isSummaryReportGetParam = new SqlParameter()
                    {
                        ParameterName = "@isSummaryReportGet",
                        SqlDbType = SqlDbType.Int,
                        Value = isSummaryReportGet,
                        Direction = System.Data.ParameterDirection.Input
                    };

                    var depoIdParam = new SqlParameter()
                    {
                        ParameterName = "@depoId",
                        SqlDbType = SqlDbType.Int,
                        Value = depoId,
                        Direction = System.Data.ParameterDirection.Input
                    };
                    var refNoParam = new SqlParameter();
                    if (!string.IsNullOrEmpty(RefNo))
                    {
                        refNoParam = new SqlParameter()
                        {
                            ParameterName = "@RefNo",
                            SqlDbType = SqlDbType.VarChar,
                            Value = RefNo,
                            Direction = System.Data.ParameterDirection.Input
                        };
                    }
                    else
                    {
                        refNoParam = new SqlParameter()
                        {
                            ParameterName = "@RefNo",
                            SqlDbType = SqlDbType.VarChar,
                            Value = DBNull.Value,
                            //Direction = System.Data.ParameterDirection.Input
                        };
                    }

                    List<LadgerEntryVM> ladgerEntryList = context.Database.SqlQuery<LadgerEntryVM>("SupplierLedgerEntryGet @fromDate,@toDate,@ledgerID,@isSummaryReportGet,@depoId,@RefNo", fromDateParam, toDateParam, ledgerIDParam, isSummaryReportGetParam, depoIdParam, refNoParam).OrderBy(x => x.Date.Value.Year).ThenBy(x => x.Date.Value.Month).ThenBy(x => x.Date.Value.Day).ThenBy(x => x.Date.Value.Hour).ThenBy(x => x.IRID).ThenBy(x => x.LedgerTypeSequence).ToList<LadgerEntryVM>();
                    return ladgerEntryList;
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
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

        public override void UpdateTableToPrint(List<LadgerEntryVM> ledgerEntryList)
        {
            if (ledgerEntryList != null && ledgerEntryList.Count > 0)
            {
                CultureInfoHelper cultureInfoHelper = new CultureInfoHelper();
                foreach (var item in ledgerEntryList)
                {
                    item.ObjectID = item.IRID;
                }
            }
            base.UpdateTableToPrint(ledgerEntryList);
        }


        public override void UpdateTableToPrintRaw(List<LadgerEntryVM> ledgerEntryList)
        {


            if (ledgerEntryList != null && ledgerEntryList.Count > 0)
            {
                CultureInfoHelper cultureInfoHelper = new CultureInfoHelper();
                foreach (var item in ledgerEntryList)
                {
                    item.ObjectID = item.IRID;
                }
            }
            base.UpdateTableToPrintRaw(ledgerEntryList);
        }

        public double GetIRTotalsum(int ledgerId, DateTime fromdate, DateTime todate)
        {
            using (var context = new AuthContext())
            {
                int Supplierid = Convert.ToInt32(context.LadgerDB.Where(x => x.ID == ledgerId).Select(x => x.ObjectID).FirstOrDefault());
                var SupplierId = new SqlParameter()
                {
                    ParameterName = "@SupplierId",
                    SqlDbType = SqlDbType.Int,
                    Value = Supplierid,
                    Direction = System.Data.ParameterDirection.Input
                };
                var fromDate = new SqlParameter()
                {
                    ParameterName = "@fromDate",
                    SqlDbType = SqlDbType.Date,
                    Value = fromdate,
                    Direction = System.Data.ParameterDirection.Input
                };
                var toDate = new SqlParameter()
                {
                    ParameterName = "@toDate",
                    SqlDbType = SqlDbType.Date,
                    Value = todate,
                    Direction = System.Data.ParameterDirection.Input
                };

                double IRTotalSumAmount = Math.Round(context.Database.SqlQuery<double>("GetIRSumData @SupplierId,@fromDate,@toDate", SupplierId, fromDate, toDate).FirstOrDefault(), 2);


                //var query = "Select ISNULL(sum(TotalAmount),0) as TotalAmount  from IRMasters where IRStatus not in('Paid','Approved from Buyer side') and supplierId=" + SupplierId;
                //double IRTotalSumAmount =Math.Round(context.Database.SqlQuery<double>(query).FirstOrDefault(),2);
                return IRTotalSumAmount;
            }

        }
        public double GetGRTotalsum(int ledgerId, DateTime fromdate, DateTime todate)
        {
            using (var context = new AuthContext())
            {
                int Supplierid = Convert.ToInt32(context.LadgerDB.Where(x => x.ID == ledgerId).Select(x => x.ObjectID).FirstOrDefault());
                var SupplierId = new SqlParameter()
                {
                    ParameterName = "@SupplierId",
                    SqlDbType = SqlDbType.Int,
                    Value = Supplierid,
                    Direction = System.Data.ParameterDirection.Input
                };
                var fromDate = new SqlParameter()
                {
                    ParameterName = "@fromDate",
                    SqlDbType = SqlDbType.Date,
                    Value = fromdate,
                    Direction = System.Data.ParameterDirection.Input
                };
                var toDate = new SqlParameter()
                {
                    ParameterName = "@toDate",
                    SqlDbType = SqlDbType.Date,
                    Value = todate,
                    Direction = System.Data.ParameterDirection.Input
                };
                double GrTotalSumAmount = Math.Round(context.Database.SqlQuery<double>("GetGRSumData @SupplierId,@fromDate,@toDate", SupplierId, fromDate, toDate).FirstOrDefault(), 2);
                //var query = "Select ISNULL(sum(gr.Qty*gr.Price),0) as GRSum from PurchaseOrderDetails po inner join GoodsReceivedDetails gr on po.PurchaseOrderDetailId = gr.PurchaseOrderDetailId Where po.SupplierId =" + SupplierId + "and po.PurchaseOrderId not in(Select PurchaseOrderId from IRMasters Where supplierId =" + SupplierId +")" ;
                //double GrTotalSumAmount =Math.Round(context.Database.SqlQuery<double>(query).FirstOrDefault(),2);
                return GrTotalSumAmount;
            }

        }

    }
}