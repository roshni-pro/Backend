using AngularJSAuthentication.API.Helper;
using AngularJSAuthentication.Controllers;
using AngularJSAuthentication.DataContracts.APIParams;
using AngularJSAuthentication.DataContracts.Transaction.Expense;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Web.Http;

namespace AngularJSAuthentication.API.ControllerV7.Expense
{
    [RoutePrefix("api/BookExpense")]
    public class BookExpenseController : BaseApiController
    {
        [Route("SaveBookExpense")]
        [HttpPost]
        public Boolean SaveBookExpense(BookExpenseDc bookExpense)
        {
            int userid = GetUserId();
            BookExpenseHelper helper = new BookExpenseHelper();
            helper.SaveBookExpense(bookExpense, userid);
            return true;
        }

        [Route("GetPage")]
        [HttpPost]
        public BookExpensePageDc GetPage(BookExpensePager pager)
        {
            using (var context = new AuthContext())
            {
                var query = from be in context.BookExpenseDB
                            join l in context.LadgerDB
                            on be.DebitLedgerId equals l.ID

                            join d in context.Departments
                            on be.DepartmentId equals d.DepId
                            into tempDept
                            from td in tempDept.DefaultIfEmpty()

                            join wc in context.WorkingCompanyDB
                            on (int)be.WorkingCompanyId equals wc.Id
                            into tempworkingComp
                            from twc in tempworkingComp.DefaultIfEmpty()

                            join wc in context.WorkingLocationDB
                            on (int)be.WorkingCompanyId equals wc.Id
                            into tempLocation
                            from twl in tempLocation.DefaultIfEmpty()

                            join bep in context.BookExpensePaymentDB
                            on be.Id equals bep.BookExpenseId
                            into tempbep from tempb in tempbep.DefaultIfEmpty()

                            where be.IsDeleted != true && be.IsActive == true
                            //&& bed.
                            && !string.IsNullOrEmpty(l.Name)
                            && (string.IsNullOrEmpty(pager.Filter) || l.Name.ToLower().Contains(pager.Filter.ToLower()))
                            && (!pager.DepartmentId.HasValue || be.DepartmentId == pager.DepartmentId)
                            && (!pager.WorkingCompanyId.HasValue || be.WorkingCompanyId == pager.WorkingCompanyId)
                            && (!pager.WorkingLocatiponID.HasValue || be.WorkingLocationId == pager.WorkingLocatiponID)


                            group tempb by new
                            {
                                l.Name,
                                be.Id,
                                be.WorkingCompanyId,
                                be.WorkingLocationId,
                                be.DepartmentId,
                                be.TotalAmount,
                                be.Status,

                                DepartmentName = td.DepName,
                                WorkingCompanyName = twc.Name,
                                WorkingLocationName = twl.Name,
                                
                            } into g


                            select new
                            {
                                g.Key.Name,
                                g.Key.Id,
                                g.Key.WorkingCompanyId,
                                g.Key.WorkingLocationId,
                                g.Key.DepartmentId,
                                g.Key.TotalAmount,
                                g.Key.Status,
                                PaidAmount = (g.Sum(x => x.Amount))==null? 0: g.Sum(x => x.Amount),
                                g.Key.DepartmentName,
                                g.Key.WorkingCompanyName,
                                g.Key.WorkingLocationName
                            };
                int count = query.Count();
                var list = query.OrderByDescending(x => x.Id).Skip(pager.SkipCount).Take(pager.Take).ToList();

                BookExpensePageDc bookExpensePageDc = new BookExpensePageDc();
                bookExpensePageDc.Count = count;
                bookExpensePageDc.PageList = list;
                return bookExpensePageDc;
            }
        }

        [Route("GetDetails/{bookExpenseID}")]
        [HttpGet]
        public BookExpenseViewDc GetDetails(int bookExpenseID)
        {
            using (var authContext = new AuthContext())
            {
                var bookExpenseQuery = from be in authContext.BookExpenseDB
                                       join e in authContext.ExpenseDB
                                       on be.ExpenseId equals e.Id
                                       join dl in authContext.LadgerDB
                                       on be.DebitLedgerId equals dl.ID

                                       join v in authContext.VendorDB
                                       on dl.ObjectID equals v.Id
                                       into vTemp from vmp in vTemp.DefaultIfEmpty()

                                       join d in authContext.Departments
                                       on be.DepartmentId equals d.DepId
                                       into wDemp from wdmp in wDemp.DefaultIfEmpty()

                                       join wc in authContext.WorkingCompanyDB
                                       on be.WorkingCompanyId equals (int)wc.Id
                                       into wCemp from wcmp in wCemp.DefaultIfEmpty()

                                       join wl in authContext.WorkingLocationDB
                                       on be.WorkingLocationId equals (int)wl.Id
                                       into wTemp from wtmp in wTemp.DefaultIfEmpty()

                                       join p in authContext.Peoples
                                       on e.CheckerId equals p.PeopleID
                                       into pTemp from ptmp in pTemp.DefaultIfEmpty()

                                       join tdsl in authContext.LadgerDB
                                       on be.TDSLedgerId equals tdsl.ID
                                       into tdslTemp
                                       from tdstm in tdslTemp.DefaultIfEmpty()

                                       join gstl in authContext.LadgerDB
                                       on be.GSTLedgerId equals gstl.ID
                                       into gstTemp
                                       from gsttm in gstTemp.DefaultIfEmpty()

                                       select new BookExpenseViewDc
                                       {
                                           ExpenseName = e.Name,
                                           Id = be.Id,
                                           TotalAmount = be.TotalAmount,
                                           DebitLedgerAmount = be.DebitLedgerAmount,
                                           DebitLedgerName = vmp.Name,
                                           DepartmentName = wdmp.DepName,
                                           WorkingCompanyName = wcmp.Name,
                                           WorkingLocationName = wtmp.Name,
                                           CheckerName = ptmp.DisplayName,
                                           TDSLedgerName = tdstm.Name,
                                           GSTLedgerName = gsttm.Name,
                                           TDSAmount = be.TDSAmount,
                                           GSTAmount = be.GSTAmount
                                       };

                BookExpenseViewDc view = bookExpenseQuery.FirstOrDefault();

                var detailListQuery = from d in authContext.BookExpenseDetailDB
                                      join cl in authContext.LadgerDB
                                      on d.CreditLedgerId equals cl.ID
                                      join ex in authContext.ExpenseDetailsDB
                                      on d.ExpenseDetailId equals ex.Id
                                      select new BookExpenseDetailDc
                                      {
                                          CreditLedgerAmount = d.CreditLedgerAmount,
                                          CreditLedgerName = cl.Name,
                                          ExpenseDetailName = ex.Name
                                      };
                view.BookExpenseDetailList = detailListQuery.ToList();
                return view;
            }
        }

        private int GetUserId()
        {
            var identity = User.Identity as ClaimsIdentity;
            int userid = 0;
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
            return userid;
        }
    }
}
