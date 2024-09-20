using AngularJSAuthentication.DataContracts.Transaction.Expense;
using AngularJSAuthentication.Model.Expense;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace AngularJSAuthentication.API.ControllerV7.Expense
{
    [RoutePrefix("api/BookExpensePayment")]
    public class BookExpensePaymentController : ApiController
    {
        [Route("AddOrUpdate")]
        [HttpPost]
        public BookExpensePaymentDc AddOrUpdate(BookExpensePaymentDc payment)
        {
            int userid = 0;
            using(var authContext = new AuthContext())
            {
                BookExpensePayment paymentEntity = new BookExpensePayment
                {
                    Amount=payment.Amount,
                    BookExpenseId= payment.BookExpenseId,
                    CreatedBy=userid,
                    CreatedDate=DateTime.Now,
                    IsActive=true,
                    IsDeleted=false,
                    Narration=payment.Narration,
                    Remark=payment.Remark,
                    PaymentDate= payment.PaymentDate
                };
                authContext.BookExpensePaymentDB.Add(paymentEntity);
                authContext.Commit();

                payment.Id = paymentEntity.Id;
            }
            return payment;
        }
    }
}
