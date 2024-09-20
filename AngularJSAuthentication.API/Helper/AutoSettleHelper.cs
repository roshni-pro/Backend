
using AngularJSAuthentication.Model;
using NLog;
using System;
using System.Collections.Generic;
using System.Configuration;

using System.Data.Entity;
using System.Linq;
using System.Transactions;

namespace AngularJSAuthentication.API.Helper
{
    public class AutoSettleHelper
    {
        public static Logger logger = LogManager.GetCurrentClassLogger();

        public bool AutoSettleOrders()
        {
            bool result = true;
            int AutoSettleOrderRetryCount = Convert.ToInt32(ConfigurationManager.AppSettings["AutoSettleOrder"]);
            int AutoSettleOrderGetCount = Convert.ToInt32(ConfigurationManager.AppSettings["AutoSettleOrderGetCount"]);
            using (var context = new AuthContext())
            {
                var OrderList = context.AutoSettleOrderDetailDB.Where(z => z.IsProcess == false).OrderByDescending(x => x.InsertedDate).Take(AutoSettleOrderGetCount).ToList();
                var orderIds = OrderList.Select(x => x.OrderId).ToList();

                if (orderIds != null && orderIds.Any())
                {
                    List<OrderDispatchedMaster> oxs = context.OrderDispatchedMasters.Where(x => orderIds.Contains(x.OrderId) && x.Deleted == false).ToList();
                    List<OrderMaster> oms = context.DbOrderMaster.Where(x => orderIds.Contains(x.OrderId) && x.Deleted == false).ToList();
                    foreach (var item in OrderList)
                    {
                        if (item == null)
                        {
                            throw new ArgumentNullException("item");
                        }

                        // for ordermaster master status change
                        int OrderId = Convert.ToInt32(item.OrderId);

                        if (OrderId > 0 && item.RetryCount < AutoSettleOrderRetryCount)
                        {
                            OrderDispatchedMaster ox = oxs.Where(x => x.OrderId == OrderId && x.Deleted == false).FirstOrDefault();
                            if (ox.Status != "sattled" && ox.Status == "Delivered")
                            {
                                using (var dbContextTransaction = context.Database.BeginTransaction())
                                {
                                    try
                                    {
                                        // order dispatched master status change
                                        OrderMaster om = oms.Where(x => x.OrderId == OrderId && x.Deleted == false).FirstOrDefault();
                                        om.Status = "sattled";
                                        om.UpdatedDate = DateTime.Now;
                                        context.Entry(om).State = EntityState.Modified;
                                        ox.Status = "sattled";
                                        ox.UpdatedDate = DateTime.Now;
                                        context.Entry(ox).State = EntityState.Modified;
                                        #region Order Master History
                                        OrderMasterHistories h1 = new OrderMasterHistories();
                                        h1.orderid = OrderId;
                                        h1.Status = om.Status;
                                        h1.Reasoncancel = "Order Settle";
                                        h1.CreatedDate = DateTime.Now;
                                        h1.username = "By System";
                                        context.OrderMasterHistoriesDB.Add(h1);
                                        #endregion

                                        if (context.Commit() > 0)
                                        {
                                            item.IsProcess = true;
                                        }
                                        else
                                        {
                                            item.IsProcess = false;
                                            item.RetryCount = item.RetryCount + 1;
                                        }
                                        dbContextTransaction.Commit();
                                    }
                                    catch (Exception ex)
                                    {
                                        dbContextTransaction.Rollback();
                                        logger.Error("Error in Final OrderDispatchedMaster " + ex.Message + " Order ID" + item.OrderId);

                                    }
                                }


                            }
                            else
                            {
                                item.IsProcess = true;
                            }

                            context.Entry(item).State = EntityState.Modified;
                            context.SaveChanges();
                        }
                    }
                }
                return result;
            }
        }

        //public bool AutoSettleOrders()
        //{
        //    bool result = false;
        //    int AutoSettleOrderRetryCount = Convert.ToInt32(ConfigurationManager.AppSettings["AutoSettleOrder"]);
        //    int AutoSettleOrderGetCount = Convert.ToInt32(ConfigurationManager.AppSettings["AutoSettleOrderGetCount"]);
        //    using (var context = new AuthContext())
        //    {
        //        var OrderList = context.AutoSettleOrderDetailDB.Where(z => z.IsProcess == false).OrderByDescending(x=>x.InsertedDate).Take(AutoSettleOrderGetCount).ToList();
        //        foreach (var item in OrderList)
        //        {
        //            if (item == null)
        //            {
        //                throw new ArgumentNullException("item");
        //            }

        //            // for ordermaster master status change
        //            int OrderId = Convert.ToInt32(item.OrderId);

        //            if (OrderId > 0 && item.RetryCount < AutoSettleOrderRetryCount)
        //            {
        //                OrderDispatchedMaster ox = context.OrderDispatchedMasters.Where(x => x.OrderId == OrderId && x.Deleted == false).Include("orderDetails").FirstOrDefault();
        //                if (ox.Status != "sattled" && ox.Status == "Delivered")
        //                {
        //                    using (var dbContextTransaction = context.Database.BeginTransaction())
        //                    {
        //                        try
        //                        {
        //                            // order dispatched master status change
        //                            OrderMaster om = context.DbOrderMaster.Where(x => x.OrderId == OrderId && x.Deleted == false).Include("orderDetails").FirstOrDefault();
        //                            om.Status = "sattled";
        //                            om.UpdatedDate = DateTime.Now;
        //                            context.Entry(om).State = EntityState.Modified;
        //                            ox.Status = "sattled";
        //                            ox.UpdatedDate = DateTime.Now;
        //                            context.Entry(ox).State = EntityState.Modified;
        //                            #region Order Master History
        //                            OrderMasterHistories h1 = new OrderMasterHistories();
        //                            h1.orderid = OrderId;
        //                            h1.Status = om.Status;
        //                            h1.Reasoncancel = "Order Settle";
        //                            h1.CreatedDate = DateTime.Now;
        //                            h1.username = "By System";
        //                            context.OrderMasterHistoriesDB.Add(h1);
        //                            #endregion

        //                            if (context.Commit() > 0)
        //                            {
        //                                item.IsProcess = true;
        //                                result = true;
        //                                context.Entry(item).State = EntityState.Modified;
        //                                context.SaveChanges();
        //                            }
        //                            else
        //                            {
        //                                result = false;
        //                                item.IsProcess = false;
        //                                item.RetryCount = item.RetryCount + 1;
        //                                context.Entry(item).State = EntityState.Modified;
        //                                context.SaveChanges();
        //                            }
        //                            dbContextTransaction.Commit();
        //                        }
        //                        catch (Exception ex)
        //                        {
        //                            dbContextTransaction.Rollback();
        //                            logger.Error("Error in Final OrderDispatchedMaster " + ex.Message + " Order ID" + item.OrderId);
        //                            logger.Info("End  Final OrderDispatchedMaster: ");
        //                            return result;
        //                        }
        //                    }
        //                }
        //                else
        //                {
        //                    item.IsProcess = true;
        //                    result = true;
        //                    context.Entry(item).State = EntityState.Modified;
        //                    context.SaveChanges();
        //                }
        //            }
        //        }
        //         return result;
        //    }
        //}
    }
}