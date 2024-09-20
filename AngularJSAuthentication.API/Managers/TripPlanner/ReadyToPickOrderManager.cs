using AngularJSAuthentication.DataContracts.TripPlanner;
using AngularJSAuthentication.Model;
using AngularJSAuthentication.Model.TripPlanner;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace AngularJSAuthentication.API.Managers.TripPlanner
{
    public class ReadyToPickOrderManager
    {
        public async Task<bool> InsertReadyToPick(List<ReadyToPickOrderDc> readyToPickOrders)
        {
            List<ReadyToPickOrder> list = new List<ReadyToPickOrder>();
            if (readyToPickOrders != null && readyToPickOrders.Any())
            {
                foreach (var item in readyToPickOrders)
                {
                    ReadyToPickOrder data = new ReadyToPickOrder
                    {
                        OrderId = item.OrderId,
                        WarehouseId = item.WarehouseId,
                        ReadyToPickDate = item.ReadyToPickDate,
                        IsActive = true,
                        IsDeleted = false,
                        CreatedDate = DateTime.Now,
                        CreatedBy = 0
                    };
                    list.Add(data);
                }
                using (AuthContext db = new AuthContext())
                {
                    db.ReadyToPickOrders.AddRange(list);
                    db.SaveChanges();
                }
            }
            return true;
        }

        public async Task<HoldOrderOutputDc> InsertReadyToPickHoldOrder(int OrderId, DateTime date, int userId, string username, DateTime? Deliverydate)
        {
            HoldOrderOutputDc holdOrderOutputDc = new HoldOrderOutputDc();
            using (AuthContext db = new AuthContext())
            {
                var ordermaster = db.DbOrderMaster.Where(x => x.OrderId == OrderId && x.active && !x.Deleted && x.Status == "Pending").FirstOrDefault();
                var readytopickorder = db.ReadyToPickOrders.Where(x => x.OrderId == OrderId && x.IsActive == true && x.IsDeleted == false).OrderByDescending(x => x.Id).FirstOrDefault();
                if (readytopickorder != null && readytopickorder.ReadyToPickDate.Date == date)
                {
                    ReadyToPickHoldOrder data = new ReadyToPickHoldOrder
                    {
                        OrderId = OrderId,
                        WarehouseId = readytopickorder.WarehouseId,
                        ReadyToPickDate = date,
                        IsActive = true,
                        IsDeleted = false,
                        CreatedDate = DateTime.Now,
                        CreatedBy = userId
                    };                    
                    if (ordermaster != null)
                    {
                        ordermaster.ExpectedRtdDate = ordermaster.ExpectedRtdDate.Value.AddDays(1);
                        db.Entry(ordermaster).State = EntityState.Modified;

                        OrderMasterHistories history = new OrderMasterHistories
                        {
                            DeliveryIssuanceId = null,
                            CreatedDate = DateTime.Now,
                            Description = "Hold RTP",
                            IsReAttempt = false,
                            orderid = OrderId,
                            Reasoncancel = "",
                            Status = ordermaster.Status,
                            Warehousename = ordermaster.WarehouseName,
                            userid = userId,
                            username = username
                        };
                        db.OrderMasterHistoriesDB.Add(history);
                    }
                    db.ReadyToPickHoldOrders.Add(data);
                }
                if (db.Commit() > 0)
                {
                    holdOrderOutputDc.DeliveryDate = Deliverydate;
                    holdOrderOutputDc.IsSuccess = true;
                    holdOrderOutputDc.DispatchDate = ordermaster.ExpectedRtdDate;
                }
            }
            return holdOrderOutputDc;
        }
    }
}