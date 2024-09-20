using AngularJSAuthentication.API.ControllerV1;
using AngularJSAuthentication.Model;
using AngularJSAuthentication.Model.KisanDan;
using GenricEcommers.Models;
using Nito.AspNetBackgroundTasks;
using NLog;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;


namespace AngularJSAuthentication.API.Helper
{
    public class CashManagementHelper
    {
        
        public static Logger logger = LogManager.GetCurrentClassLogger();
        public static List<int> ordersToProcess = new List<int>();
        public UploadAssignment PaymentSubmittedAssignmentHelper(int id, Int32 DeliveryIssuanceId, string FileName,AuthContext db,out bool islastmileapp)
        {
            UploadAssignment res;
            var DBIssuance = db.DeliveryIssuanceDb.Where(x => x.DeliveryIssuanceId == DeliveryIssuanceId && x.PeopleID == id).FirstOrDefault();

            if (DBIssuance != null)
            {
                islastmileapp = DBIssuance.TripPlannerConfirmedMasterId > 0 ? true : false;
                var peopledata = db.Peoples.Where(x => x.PeopleID == id).FirstOrDefault();
                DBIssuance.Status = "Payment Submitted";
                DBIssuance.UploadedFileName = FileName;
                DBIssuance.UpdatedDate = DateTime.Now;
                db.Entry(DBIssuance).State = EntityState.Modified;

                #region Order Delivery  Master History for Status Payment Submitted
                OrderDeliveryMasterHistories AssginDeli = new OrderDeliveryMasterHistories();
                AssginDeli.DeliveryIssuanceId = DBIssuance.DeliveryIssuanceId;
                //AssginDeli.OrderId = DBIssuance.o
                AssginDeli.Cityid = DBIssuance.Cityid;
                AssginDeli.city = DBIssuance.city;
                AssginDeli.DisplayName = DBIssuance.DisplayName;
                AssginDeli.Status = DBIssuance.Status;
                AssginDeli.WarehouseId = DBIssuance.WarehouseId;
                AssginDeli.PeopleID = DBIssuance.PeopleID;
                AssginDeli.VehicleId = DBIssuance.VehicleId;
                AssginDeli.VehicleNumber = DBIssuance.VehicleNumber;
                AssginDeli.RejectReason = DBIssuance.RejectReason;
                AssginDeli.OrderdispatchIds = DBIssuance.OrderdispatchIds;
                AssginDeli.OrderIds = DBIssuance.OrderIds;
                AssginDeli.Acceptance = DBIssuance.Acceptance;
                AssginDeli.IsActive = DBIssuance.IsActive;
                AssginDeli.IdealTime = DBIssuance.IdealTime;
                AssginDeli.TravelDistance = DBIssuance.TravelDistance;
                AssginDeli.CreatedDate = DateTime.Now;
                AssginDeli.UpdatedDate = DateTime.Now;
                AssginDeli.userid = id;
                if (peopledata != null)
                {
                    if (peopledata.DisplayName == null)
                    {
                        AssginDeli.UpdatedBy = peopledata.PeopleFirstName;
                    }
                    else
                    {
                        AssginDeli.UpdatedBy = peopledata.DisplayName;
                    }

                }
                db.OrderDeliveryMasterHistoriesDB.Add(AssginDeli);

                #endregion


                var ODMs = db.OrderDispatchedMasters.Where(x => x.DeliveryIssuanceIdOrderDeliveryMaster == DeliveryIssuanceId && x.Status == "Delivered").ToList();
                foreach (var ODMaster in ODMs)
                {
                    if (ODMaster.RewardPoint > 0)
                    {
                        if (!db.CustomerWalletHistoryDb.Any(x => x.OrderId == ODMaster.OrderId && x.NewAddedWAmount == ODMaster.RewardPoint && x.Through == "From Order Delivered"))
                        {
                            Wallet wlt = db.WalletDb.Where(c => c.CustomerId == ODMaster.CustomerId).SingleOrDefault();

                            CustomerWalletHistory CWH = new CustomerWalletHistory();
                            CWH.WarehouseId = ODMaster.WarehouseId;
                            CWH.CompanyId = ODMaster.CompanyId;
                            CWH.CustomerId = wlt.CustomerId;
                            CWH.NewAddedWAmount = ODMaster.RewardPoint;
                            CWH.OrderId = ODMaster.OrderId;
                            CWH.Through = "From Order Delivered";
                            CWH.TotalWalletAmount = wlt.TotalAmount + ODMaster.RewardPoint;
                            CWH.CreatedDate = DateTime.Now;
                            CWH.UpdatedDate = DateTime.Now;
                            db.CustomerWalletHistoryDb.Add(CWH);

                            wlt.TotalAmount += ODMaster.RewardPoint;
                            try
                            {
                                var customers = db.Customers.Where(x => x.fcmId != null && x.CustomerId == wlt.CustomerId).SingleOrDefault();
                                if (customers != null)
                                {
                                    BackgroundTaskManager.Run(() =>
                                    {
                                        DeliveryTaskController dsc1 = new DeliveryTaskController();
                                        dsc1.DeliveredNotificationWithoutAuth(wlt.CustomerId, ODMaster.RewardPoint, ODMaster.OrderId, customers);
                                    });
                                }
                            }
                            catch (Exception ex) { logger.Error("Error loading wqewqerwqr \n\n" + ex.Message + "\n\n" + ex.InnerException + "\n\n" + ex.StackTrace); }

                            wlt.TransactionDate = DateTime.Now;
                            db.Entry(wlt).State = EntityState.Modified;

                            var rpoint = db.RewardPointDb.Where(c => c.CustomerId == ODMaster.CustomerId).SingleOrDefault();
                            if (rpoint != null)
                            {
                                rpoint.EarningPoint -= ODMaster.RewardPoint;
                                if (rpoint.EarningPoint < 0)
                                    rpoint.EarningPoint = 0;
                                rpoint.UpdatedDate = DateTime.Now;
                                db.Entry(rpoint).State = EntityState.Modified;
                            }
                        }
                    }

                    #region Kisan Dan Insert
                    try
                    {
                        var Orderid = ODMaster.OrderId;
                        string query = "select a.customerid,a.orderid,"
                        + " sum(case when subCategoryName = 'kisan kirana' then qty * UnitPrice else 0 end) KisanKiranaAmount, "
                        + " sum(qty * UnitPrice) OrderAmount from OrderDispatchedDetails a with(nolock)"
                        + " where OrderId =  " + Orderid
                        + " group by a.CustomerId,a.orderid having  sum(case when subCategoryName = 'kisan kirana' then qty * UnitPrice else 0 end)> 0";

                        var kisanDanMasters = db.kisanDanMaster.Where(x => x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value)).ToList();
                        var data = db.Database.SqlQuery<CustomerKisanDanDTO>(query).ToList();
                        CustomerKisanDan newdata = new CustomerKisanDan();
                        foreach (var item in data)
                        {
                            if (!db.CustomerKisanDan.Any(x => x.OrderId == item.OrderId && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value)))
                            {
                                newdata = new CustomerKisanDan();
                                newdata.CustomerId = item.CustomerId;
                                newdata.OrderId = item.OrderId;
                                newdata.KisanKiranaAmount = Convert.ToDecimal(item.KisanKiranaAmount);
                                newdata.IsActive = true;
                                newdata.IsDeleted = false;
                                newdata.CreatedBy = id;
                                newdata.CreatedDate = DateTime.Now;;
                                if (newdata.KisanKiranaAmount > 0 && kisanDanMasters != null && kisanDanMasters.Any())
                                {
                                    var percent = kisanDanMasters.Any(x => x.OrderFromAmount <= newdata.KisanKiranaAmount && x.OrderToAmount >= newdata.KisanKiranaAmount) ? kisanDanMasters.FirstOrDefault(x => x.OrderFromAmount <= newdata.KisanKiranaAmount && x.OrderToAmount >= newdata.KisanKiranaAmount).KisanDanPrecentage : 0;
                                    newdata.KisanDanAmount = newdata.KisanKiranaAmount * percent / 100;
                                }
                                db.CustomerKisanDan.Add(newdata);
                                logger.Info("Kisan amount Add Assignment Id: " + DeliveryIssuanceId);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        string error = ex.InnerException != null ? ex.ToString() + Environment.NewLine + ex.InnerException.ToString() : ex.ToString();

                        logger.Error("Error in During Add KisanDan Point For Assignment Id: " + DeliveryIssuanceId + " Error: " + error);
                    }
                    #endregion

                }                        
                res = new UploadAssignment()
                {
                    Status = true,
                    Message = "Success."
                };

                return res;
            }
            else
            {

                res = new UploadAssignment()
                {
                    Status = false,
                    Message = "Failed due to record not found"
                };
                islastmileapp = false;
                return res;
            }
        }
    }
}