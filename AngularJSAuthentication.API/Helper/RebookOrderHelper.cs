using AngularJSAuthentication.Model;
using AngularJSAuthentication.Model.Account;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Security.Claims;
using System.Transactions;
using System.Web;
using static AngularJSAuthentication.API.Controllers.OrderMasterrController;

namespace AngularJSAuthentication.API.Helper
{
    public class RebookOrderHelper
    {
        double CurrentLimitRemainAmt = 0;
        double CurrentOrderAmount = 0;
        private bool CheckLimitforRebook(int WarehouseId)
        {
            bool check = false;
            try
            {
                using (var context = new AuthContext())
                {
                    var _WarehouseId = new SqlParameter
                    {
                        ParameterName = "@WarehouseId",
                        Value = WarehouseId
                    };

                    LimitDC limit = context.Database.SqlQuery<LimitDC>("ValidateRebookOrderLimit @WarehouseId", _WarehouseId).First();
                      // 500 >= 400 + 200
                    check = limit.LastMonthPer >= limit.CurrentMonthAmt + CurrentOrderAmount ? true : false;
                    var diff = Math.Round(limit.LastMonthPer - limit.CurrentMonthAmt);
                    CurrentLimitRemainAmt = diff < 0 ? 0 : diff;
                }
            }
            catch (Exception ex)
            {
                check = false;
            }
            return check;
        }
        private bool ValidateOnceperOrder(int orderid)
        {
            try
            {
                using (var context = new AuthContext())
                {
                    return context.DbOrderMaster.Any(x => x.ParentOrderId == orderid);
                }
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        private bool ValidatePendingOrder(RebookOderDC rebook, out string msg)
        {
            bool i = true;
            msg = string.Empty;
            try
            {
                using (var context = new AuthContext())
                {
                    var ordermst = context.DbOrderMaster.Where(x => x.OrderId == rebook.OrderId && x.Status == "Pending")
                                   .Select(x => new { x.CreatedDate, x.Status, x.GrossAmount }).FirstOrDefault();
                    if (ordermst != null)
                    {
                        CurrentOrderAmount = ordermst.GrossAmount;
                        if (rebook.NewDate.Date >= ordermst.CreatedDate.Date && rebook.NewDate.Date <= DateTime.Now.Date)
                        {

                        }
                        else
                        {
                            msg = "New Order date can equal to order date or upto till date only.";
                            i = false;
                        }
                    }
                    else
                    {
                        msg = "Rebook option is applicable only Pendng orders.";
                        i = false;
                    }
                }
            }

            catch (Exception ex)
            {
                msg = "";
            }
            return i;
        }
        private bool InsertLedgerEntry(int OrderId)
        {
            bool res = false;
            try
            {
                using (var context = new AuthContext())
                {
                    var paymentdtls = context.PaymentResponseRetailerAppDb.Where(x => x.OrderId == OrderId && x.status == "Success").FirstOrDefault();
                    if (paymentdtls != null &&  context.OnlinePaymentDtlsForLedgerDB.FirstOrDefault(z => z.OrderId == OrderId && z.TransactionId== paymentdtls.GatewayTransId) == null && paymentdtls.PaymentFrom != "Cash")
                    {
                        var CustomerId = context.DbOrderMaster.Where(z => z.OrderId == OrderId).FirstOrDefault().CustomerId;
                        OnlinePaymentDtlsForLedger Opdl = new OnlinePaymentDtlsForLedger();
                        Opdl.OrderId = OrderId;
                        Opdl.IsPaymentSuccess = true;
                        Opdl.IsLedgerAffected = "Yes";
                        Opdl.PaymentDate = DateTime.Now;
                        Opdl.TransactionId = paymentdtls.GatewayTransId;
                        Opdl.IsActive = true;
                        Opdl.CustomerId = CustomerId;
                        context.OnlinePaymentDtlsForLedgerDB.Add(Opdl);
                        if (context.Commit() > 0)
                        {
                            res = true;
                        }
                    }
                    else
                    {
                        res = true;
                    }
                }
                return res;
            }
            catch (Exception)
            {
                return res;
            }
        }
        public string CreateRebookOrder(RebookOderDC rebookDc, out bool status)
        {
            string result = string.Empty;



            if (ValidateOnceperOrder(rebookDc.OrderId))
            {
                result = "Rebook option is once pr order and this order is already used.";
                status = false;
                return result;
            }
            if (!ValidatePendingOrder(rebookDc, out string msg))
            {
                result = msg;
                status = false;
                return result;
            }
            if (!CheckLimitforRebook(rebookDc.WarehouseId))
            {
                result = "Limit of Current Month is over, can not Rebook order this month. Limit Amount is " + CurrentLimitRemainAmt.ToString();
                status = false;
                return result;
            }
            TransactionOptions option = new TransactionOptions();
            option.IsolationLevel = System.Transactions.IsolationLevel.RepeatableRead;
            option.Timeout = TimeSpan.FromSeconds(90);
            using (TransactionScope Scope = new TransactionScope(TransactionScopeOption.Required, option))
            {
                try
                {
                    using (var context = new AuthContext())
                    {
                        var _OrderId = new SqlParameter
                        {
                            ParameterName = "@OrderId",
                            Value = rebookDc.OrderId
                        };
                        var _NewDate = new SqlParameter
                        {
                            ParameterName = "@NewDate",
                            Value = rebookDc.NewDate
                        };
                        var _userid = new SqlParameter
                        {
                            ParameterName = "@UserId",
                            Value = rebookDc.UserId
                        };
                        int OrderID = context.Database.SqlQuery<int>("CreateRebookOrder @OrderId, @NewDate,@UserId", _OrderId, _NewDate, _userid).First();
                        if (OrderID > 100)
                        {
                            if (ReverseLedgerEntry(rebookDc.OrderId) && InsertLedgerEntry(OrderID))
                            {
                                Scope.Complete();
                                result = "New order placed successfully, Order ID is : " + OrderID.ToString() + " Remaining Limit Amount is : " + CurrentLimitRemainAmt.ToString();
                                status = true;
                            }
                            else
                            {
                                Scope.Dispose();
                                result = "Something went wrong, try again or connect with IT.";
                                status = false;
                            }
                        }
                        else
                        {
                            Scope.Dispose();
                            result = "Something went wrong, try again or connect with IT.";
                            status = false;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Scope.Dispose();
                    result = "Something went wrong, try again or connect with IT." + ex.Message;
                    status = false;
                }
            }
            return result;
        }

        private bool ReverseLedgerEntry(int OrderID)
        {
            bool value = false;
            try
            {
                using (var context = new AuthContext())
                {
                    var ledger = context.LadgerEntryDB.Where(x => x.ObjectID == OrderID && x.ObjectType == "Order" && x.VouchersTypeID == 2).ToList();
                    if (ledger.Any())
                    {
                        LadgerEntry debitLedgerEntry = ledger.First(x => x.Debit > 0);
                        LadgerEntry creditLedgerEntry = ledger.First(x => x.Credit > 0);

                        LadgerEntry debitEntry = new LadgerEntry
                        {
                            Date = DateTime.Now.Date,
                            Particulars = debitLedgerEntry.Particulars,
                            LagerID = creditLedgerEntry.LagerID,
                            VouchersTypeID = context.VoucherTypeDB.Where(x => x.Name.ToLower() == "RebookOrder").First().ID,
                            VouchersNo = debitLedgerEntry.VouchersNo,
                            Debit = debitLedgerEntry.Debit,
                            Credit = debitLedgerEntry.Credit,
                            ObjectID = debitLedgerEntry.ObjectID,
                            ObjectType = debitLedgerEntry.ObjectType,
                            AffectedLadgerID = creditLedgerEntry.AffectedLadgerID,
                            Active = debitLedgerEntry.Active,
                            CreatedBy = debitLedgerEntry.CreatedBy,
                            CreatedDate = DateTime.Now,
                            UpdatedBy = debitLedgerEntry.UpdatedBy,
                            UpdatedDate = DateTime.Now,
                            UploadGUID = debitLedgerEntry.UploadGUID,
                            RefNo = debitLedgerEntry.RefNo,
                            Remark = "Rebook Order",
                            IsSupplierAdvancepay = debitLedgerEntry.IsSupplierAdvancepay,
                            IrPaymentDetailsId = debitLedgerEntry.IrPaymentDetailsId,
                            PRPaymentId = debitLedgerEntry.PRPaymentId,
                            ParentId = debitLedgerEntry.ParentId,
                        };
                        context.LadgerEntryDB.Add(debitEntry);

                        LadgerEntry creditEntry = new LadgerEntry
                        {
                            Date = DateTime.Now.Date,
                            Particulars = creditLedgerEntry.Particulars,
                            LagerID = debitLedgerEntry.LagerID,
                            VouchersTypeID = debitEntry.VouchersTypeID,
                            VouchersNo = creditLedgerEntry.VouchersNo,
                            Debit = creditLedgerEntry.Debit,
                            Credit = creditLedgerEntry.Credit,
                            ObjectID = creditLedgerEntry.ObjectID,
                            ObjectType = creditLedgerEntry.ObjectType,
                            AffectedLadgerID = debitLedgerEntry.AffectedLadgerID,
                            Active = creditLedgerEntry.Active,
                            CreatedBy = creditLedgerEntry.CreatedBy,
                            CreatedDate = DateTime.Now,
                            UpdatedBy = creditLedgerEntry.UpdatedBy,
                            UpdatedDate = DateTime.Now,
                            UploadGUID = creditLedgerEntry.UploadGUID,
                            RefNo = creditLedgerEntry.RefNo,
                            Remark = "Rebook Order",
                            IsSupplierAdvancepay = creditLedgerEntry.IsSupplierAdvancepay,
                            IrPaymentDetailsId = creditLedgerEntry.IrPaymentDetailsId,
                            PRPaymentId = creditLedgerEntry.PRPaymentId,
                            ParentId = creditLedgerEntry.ParentId,
                        };
                        context.LadgerEntryDB.Add(creditEntry);
                        if (context.Commit() > 0)
                        {
                            value = true;
                        }
                        else
                        {
                            value = false;
                        }
                    }
                    else
                    {
                        value = true;
                    }
                }
            }
            catch (Exception)
            {
                return value;
            }
            return value;
        }

    }
    public class LimitDC
    {
        public double LastMonthPer { get; set; }
        public double CurrentMonthAmt { get; set; }
    }

}