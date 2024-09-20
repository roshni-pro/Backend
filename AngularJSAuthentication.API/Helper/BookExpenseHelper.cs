using AngularJSAuthentication.DataContracts.APIParams;
using AngularJSAuthentication.Model.Expense;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;
using System.Web;

namespace AngularJSAuthentication.API.Helper
{
    public class BookExpenseHelper
    {
        public BookExpenseDc SaveBookExpense(BookExpenseDc bookExpense, int userid)
        {
            DateTime currentTime = DateTime.Now;
            TransactionOptions option = new TransactionOptions();
            option.IsolationLevel = IsolationLevel.RepeatableRead;
            option.Timeout = TimeSpan.FromSeconds(90);
            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, option))
            //using (TransactionScope scope = new TransactionScope())
            {
                try
                {
                    using (var authContext = new AuthContext())
                    {
                        SaveBookExpenseOnly(bookExpense, authContext, currentTime, userid);
                        SaveBookExpenseDetailList(bookExpense.BookExpenseDetailList, authContext, currentTime, userid, bookExpense.Id.Value);
                    }
                    scope.Complete();
                }catch(Exception ex)
                {
                    scope.Dispose();
                    throw ex;
                }
                
            }
            
            return bookExpense;
        }

        private BookExpense SaveBookExpenseOnly(BookExpenseDc bookExpenseViewModel, AuthContext context, DateTime currentTime, int userid)
        {

            BookExpense bookExpense = new BookExpense();
            bookExpense.CreatedBy = userid;
            bookExpense.CreatedDate = currentTime;
            bookExpense.IsActive = true;
            bookExpense.IsDeleted = false;

            bookExpense.ExpenseId = bookExpenseViewModel.ExpenseId;
            bookExpense.TotalAmount = bookExpenseViewModel.TotalAmount;
            bookExpense.DebitLedgerId = bookExpenseViewModel.DebitLedgerId;
            bookExpense.DebitLedgerTypeId = bookExpenseViewModel.DebitLedgerTypeId;
            bookExpense.DebitLedgerAmount = bookExpenseViewModel.DebitLedgerAmount;
            bookExpense.DepartmentId = bookExpenseViewModel.DepartmentId;
            bookExpense.WorkingLocationId = bookExpenseViewModel.WorkingLocationId;
            bookExpense.WorkingCompanyId = bookExpenseViewModel.WorkingCompanyId;
            bookExpense.CheckerId = null;
            bookExpense.IsChecked = false;
            bookExpense.IsLedgerGenerated = bookExpenseViewModel.IsLedgerGenerated;
            bookExpense.ExpenseDate = bookExpenseViewModel.ExpenseDate;
            bookExpense.IsTDSApplied = bookExpenseViewModel.IsTDSApplied;
            bookExpense.IsGSTApplied = bookExpenseViewModel.IsGSTApplied;
            bookExpense.TDSLedgerId = bookExpenseViewModel.TDSLedgerId;
            bookExpense.GSTLedgerId = bookExpenseViewModel.GSTLedgerId;
            bookExpense.TDSAmount = bookExpenseViewModel.TDSAmount;
            bookExpense.GSTAmount = bookExpenseViewModel.GSTAmount;

            context.BookExpenseDB.Add(bookExpense);
            context.Commit();

            bookExpenseViewModel.Id = bookExpense.Id;
            return bookExpense;
        }

        private List<BookExpenseDetail> SaveBookExpenseDetailList(List<BookExpenseDetailDc> detailList, AuthContext context, DateTime currentTime, int userid, long bookExpenseId)
        {
            List<BookExpenseDetail> expenseDetailList = null;
            if (detailList != null && detailList.Count > 0)
            {
                expenseDetailList = new List<BookExpenseDetail>();
                foreach (BookExpenseDetailDc detail in detailList)
                {
                    BookExpenseDetail newDetail = new BookExpenseDetail();
                    newDetail.CreatedBy = userid;
                    newDetail.CreatedDate = currentTime;
                    newDetail.IsActive = true;
                    newDetail.IsDeleted = false;

                    newDetail.BookExpenseId = bookExpenseId;
                    newDetail.CreditLedgerId = detail.CreditLedgerId;
                    newDetail.CreditLedgerTypeId = detail.CreditLedgerTypeId;
                    newDetail.CreditLedgerAmount = detail.CreditLedgerAmount;
                    newDetail.ExpenseDetailId = detail.ExpenseDetailId;

                    expenseDetailList.Add(newDetail);
                }

                context.BookExpenseDetailDB.AddRange(expenseDetailList);
                context.Commit();
            }
            return expenseDetailList;
        }

    }
}