using AngularJSAuthentication.DataContracts.APIParams;
using AngularJSAuthentication.Model.Expense;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;

namespace AngularJSAuthentication.API.Helper
{
    public class ExpenseHelper
    {
        public List<ExpenseListDC> GetExpenseList()
        {
            using (var authContext = new AuthContext())
            {
                List<ExpenseListDC> list = authContext.ExpenseDB.Where(x => x.IsDeleted != true && x.IsActive == true)
                    .Select(y => new ExpenseListDC
                    {
                        Id = y.Id,
                        Name = y.Name
                    }).ToList();
                return list;
            }
        }

        public ExpenseDc GetById(long expenseId)
        {
            using (var authContext = new AuthContext())
            {
                var query = from e in authContext.ExpenseDB
                            join lt in authContext.LadgerTypeDB
                            on e.DebitLedgerTypeId equals lt.ID
                            join l in authContext.LadgerDB
                            on e.DebitLedgerId equals l.ID
                            into temp from j in temp.DefaultIfEmpty()
                            join p in authContext.Peoples
                            on e.CheckerId equals p.PeopleID
                            into temppeople from k in temppeople.DefaultIfEmpty()
                            where e.Id == expenseId && e.IsActive == true && e.IsDeleted != true
                            select new ExpenseDc
                            {
                                CheckerId = e.CheckerId,
                                DebitLedgerId = e.DebitLedgerId,
                                DebitLedgerTypeId = e.DebitLedgerTypeId,
                                Id = e.Id,
                                Name = e.Name,
                                DebitLedgerName = j.Name,
                                DebitLedgerTypeName = lt.Name,
                                IsGSTApplied = e.IsGSTApplied,
                                IsTDSApplied = e.IsTDSApplied,
                                CheckerName= k.DisplayName

                            };

                ExpenseDc expense = query.First();

                authContext.Database.Log = log => Debug.WriteLine(log);

                var detailedQuery = from e in authContext.ExpenseDetailsDB
                                    join lt in authContext.LadgerTypeDB
                                    on e.CreditLedgerTypeId equals lt.ID
                                    join l in authContext.LadgerDB
                                    on e.CreditLedgerID equals l.ID
                                    into temp from j in temp.DefaultIfEmpty()
                                    where e.ExpenseID == expenseId && e.IsActive == true && e.IsDeleted != true
                                    select new ExpenseDetailDc
                                    {
                                        CreditLedgerID = e.CreditLedgerID,
                                        CreditLedgerName = j.Name,
                                        CreditLedgerTypeId = e.CreditLedgerTypeId,
                                        CreditLedgerTypeName = lt.Name,
                                        ExpenseID = e.ExpenseID,
                                        Name = e.Name,
                                        Id = e.Id,
                                        IsFixedCreditLedgerId = e.IsFixedCreditLedgerId,
                                        IsMasterLedger= e.IsMasterLedger
                                    };
                expense.ExpenseDetailList = detailedQuery.ToList();

                return expense;
            }
        }


        public ExpenseDc AddExpense(ExpenseDc expenseDc)
        {
            using (var authContext = new AuthContext())
            {

                Expense addexp = new Expense();
                addexp.DebitLedgerTypeId = expenseDc.DebitLedgerTypeId;
                addexp.DebitLedgerId = expenseDc.DebitLedgerId;
                addexp.CheckerId = expenseDc.CheckerId;
                addexp.Name = expenseDc.Name;
                addexp.CreatedDate = DateTime.Now;
                addexp.CreatedBy = expenseDc.CreatedBy;
                addexp.IsActive = true;
                addexp.IsDeleted = false;
                addexp.IsGSTApplied = expenseDc.IsGSTApplied;
                addexp.IsTDSApplied = expenseDc.IsTDSApplied;
                authContext.ExpenseDB.Add(addexp);
                authContext.Commit();
                expenseDc.Id = addexp.Id;
                return expenseDc;
            }
        }

        public ExpenseDetailDc AddExpenseDetails(ExpenseDetailDc expenseDetailDc)
        {
            using (var authContext = new AuthContext())
            {
                ExpenseDetails expenseDetails = new ExpenseDetails();
                expenseDetails.CreditLedgerID = expenseDetailDc.CreditLedgerID;
                expenseDetails.CreditLedgerTypeId = expenseDetailDc.CreditLedgerTypeId;
                expenseDetails.ExpenseID = expenseDetailDc.ExpenseID;
                expenseDetails.IsFixedCreditLedgerId = expenseDetailDc.IsFixedCreditLedgerId;
                expenseDetails.Name = expenseDetailDc.Name;
                expenseDetails.CreatedBy = expenseDetailDc.CreatedBy;
                expenseDetails.CreatedDate = DateTime.Now;
                expenseDetails.IsActive = true;
                expenseDetails.IsDeleted = false;
                expenseDetails.IsMasterLedger = expenseDetailDc.IsMasterLedger;
                authContext.ExpenseDetailsDB.Add(expenseDetails);
                authContext.Commit();
                expenseDetailDc.Id = expenseDetails.Id;
                return expenseDetailDc;
            }


        }

        public List<ExpenseDetailDc> GetExpenseDetailsList(int expenseId)
        {

            using (var authContext = new AuthContext())
            {

                List<ExpenseDetailDc> list = (from exp in authContext.ExpenseDetailsDB
                                              join lt in authContext.LadgerTypeDB
                                              on exp.CreditLedgerTypeId equals lt.ID
                                              join l in authContext.LadgerDB
                                              on exp.CreditLedgerID equals l.ID
                                              into temp from j in temp.DefaultIfEmpty()
                                              where exp.IsDeleted == false && exp.IsActive == true && exp.ExpenseID == expenseId
                                              select new ExpenseDetailDc
                                              {
                                                  Id = exp.Id,
                                                  ExpenseID = exp.ExpenseID,
                                                  Name = exp.Name,
                                                  IsFixedCreditLedgerId = exp.IsFixedCreditLedgerId,
                                                  CreditLedgerName = j.Name,
                                                  CreditLedgerTypeName = lt.Name,
                                                  CreditLedgerID = exp.CreditLedgerID,
                                                  CreditLedgerTypeId = exp.CreditLedgerTypeId,
                                                  IsMasterLedger=exp.IsMasterLedger

                                              }).ToList();
                return list;
            }

        }

        public bool DeleteExpense(int Id)
        {
            using (var authContext = new AuthContext())
            {
                ExpenseDetails expenseDetails = authContext.ExpenseDetailsDB.Where(x => x.Id == Id).FirstOrDefault();
                expenseDetails.IsActive = false;
                expenseDetails.IsDeleted = true;
                authContext.Entry(expenseDetails).State = EntityState.Modified;
                authContext.Commit();
                return true;
            }
        }
        public ExpenseDetailDc UpdateExpenseDetails(ExpenseDetailDc expenseDetailDc) {
            using (var authContext = new AuthContext())
            {
                ExpenseDetails expenseDetails = authContext.ExpenseDetailsDB.Where(x => x.Id == expenseDetailDc.Id).FirstOrDefault();
                expenseDetails.CreditLedgerID = expenseDetailDc.CreditLedgerID;
                expenseDetails.CreditLedgerTypeId = expenseDetailDc.CreditLedgerTypeId;
                expenseDetails.ModifiedBy = expenseDetailDc.CreatedBy;
                expenseDetails.ModifiedDate = DateTime.Now;
                expenseDetails.IsFixedCreditLedgerId = expenseDetailDc.IsFixedCreditLedgerId;
                expenseDetails.IsMasterLedger = expenseDetailDc.IsMasterLedger;
                authContext.Entry(expenseDetails).State = EntityState.Modified;
                authContext.Commit();
              
                return expenseDetailDc;
            }
        }
       
        public List<ExpenseDc> GetExpenseListData()
        {
            using (var authContext = new AuthContext())
            {
                var query = from e in authContext.ExpenseDB
                            join lt in authContext.LadgerTypeDB
                            on e.DebitLedgerTypeId equals lt.ID
                            join l in authContext.LadgerDB
                            on e.DebitLedgerId equals l.ID
                            into temp
                            from j in temp.DefaultIfEmpty()
                            where   e.IsActive == true && e.IsDeleted != true
                            select new ExpenseDc
                            {
                                CheckerId = e.CheckerId,
                                DebitLedgerId = e.DebitLedgerId,
                                DebitLedgerTypeId = e.DebitLedgerTypeId,
                                Id = e.Id,
                                Name = e.Name,
                                DebitLedgerName = j.Name,
                                DebitLedgerTypeName = lt.Name,
                                IsGSTApplied = e.IsGSTApplied,
                                IsTDSApplied = e.IsTDSApplied
                            };

                List<ExpenseDc> expense = query.OrderBy(x => x.Id).ToList();
                return expense;
            }
        }
        public ExpenseDc UpdateExpense(ExpenseDc expenseDc) {
            using (var authContext = new AuthContext())
            {
                Expense addexp = authContext.ExpenseDB.Where(x => x.Id == expenseDc.Id).FirstOrDefault();
                addexp.DebitLedgerTypeId = expenseDc.DebitLedgerTypeId;
                addexp.DebitLedgerId = expenseDc.DebitLedgerId;
                addexp.CheckerId = expenseDc.CheckerId;
                addexp.Name = expenseDc.Name;
                addexp.ModifiedDate = DateTime.Now;
                addexp.ModifiedBy = expenseDc.CreatedBy;
                authContext.Entry(addexp).State = EntityState.Modified;
                authContext.Commit();
                
                return expenseDc;
            }

        }
      
       public bool DeleteExpenseById(int Id)
        {
            using (var authContext = new AuthContext())
            {
                Expense expense = authContext.ExpenseDB.Where(x => x.Id == Id).FirstOrDefault();
                expense.IsActive = false;
                expense.IsDeleted = true;
                authContext.Entry(expense).State = EntityState.Modified;
                var parameters = new List<SqlParameter> { new SqlParameter("@ExpnseId", Id) };
                authContext.Database.ExecuteSqlCommand("EXEC DeleteExpenseDetail @ExpnseId", parameters.ToArray());
                authContext.Commit();

                return true;
            }
        }
    }
}