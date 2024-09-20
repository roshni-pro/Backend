using AngularJSAuthentication.Model;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AngularJSAuthentication.API.Helper
{
    public class SalesReturnOrderHelper
    {
        // GET: SalesReturnOrderHelper
        public bool PostOrderStatus(int OrderId, string Status, int userid, AuthContext context)
        {
            bool res = false;
            var existOrderData = context.DbKKReturnOrderStatus.Where(x => x.OrderId == OrderId).FirstOrDefault();
            var existKKReturnReplaceOrderRequestData = context.KKReturnReplaceRequests.Where(x => x.OrderId == OrderId).FirstOrDefault();
            if (existOrderData != null)
            {
                existOrderData.ModifiedBy = userid;
                existOrderData.ModifiedDate = DateTime.Today;
                existOrderData.IsActive = true;
                existOrderData.IsDeleted = false;
                existOrderData.Status = "Return_" + Status;
                context.Entry(existOrderData).State = EntityState.Modified;
                if(existKKReturnReplaceOrderRequestData != null)
                {
                    existKKReturnReplaceOrderRequestData.Status = Status;
                    existKKReturnReplaceOrderRequestData.ModifiedBy = userid;
                    existKKReturnReplaceOrderRequestData.ModifiedDate = DateTime.Today;
                    context.Entry(existKKReturnReplaceOrderRequestData).State = EntityState.Modified;
                    res = true;
                }
                //context.Commit();
            }
            else
            {
                KKReturnOrderStatus kKReturnOrderStatus = new KKReturnOrderStatus();
                kKReturnOrderStatus.CreatedBy = userid;
                kKReturnOrderStatus.CreatedDate = DateTime.Today;
                kKReturnOrderStatus.IsActive = true;
                kKReturnOrderStatus.IsDeleted = false;
                kKReturnOrderStatus.OrderId = OrderId;
                kKReturnOrderStatus.Status = "Return_" + Status;
                context.DbKKReturnOrderStatus.Add(kKReturnOrderStatus);
                res = true;
                //context.Commit();
            }
            return res;
        }
    }
}