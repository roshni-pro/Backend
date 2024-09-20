using AngularJSAuthentication.API.Helper;
using AngularJSAuthentication.API.Results;
using AngularJSAuthentication.BusinessLayer.Managers.Masters;
using AngularJSAuthentication.Common.Constants;
using AngularJSAuthentication.Common.Helpers;
using AngularJSAuthentication.DataContracts.Transaction.OrderProcess;
using AngularJSAuthentication.DataContracts.Transaction.TripPlanner;
using AngularJSAuthentication.Model;
using Nito.AsyncEx;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace AngularJSAuthentication.API.Controllers.OrderProcess
{
    [RoutePrefix("api/OrderProcess")]
    public class OrderProcessController : ApiController
    {
        private static TimeZoneInfo INDIAN_ZONE = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
        DateTime indianTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE);




        [HttpGet]
        [Route("AutoReadyToPick")]
        public async Task<ResultViewModel<string>> AutoReadyToPick(int? warehouseId = 0)
        {
            var finalListForReadyToPick = new List<OrdersForAutoPick>();
            var dataTables = new List<DataTable>();
            var ordersToProcess = new List<OrdersForAutoPick>();
            string warehousequery = string.Empty;
            using (var context = new AuthContext())
            {
                if (!warehouseId.HasValue || warehouseId.Value == 0)
                {
                    warehousequery = @" update w set w.IsAutoRTPRunning=1 from Warehouses w   
                                        inner join Warehouses ws on ws.WarehouseId=w.WarehouseId  
                                        inner join GMWarehouseProgresses b on w.WarehouseId = b.WarehouseID and b.IsLaunched=1  
                                        and w.active=1 and w.Deleted=0 and w.IsKPP=0 and w.CityName not like '%test%'   ";
                    await context.Database.ExecuteSqlCommandAsync(warehousequery);
                    context.Commit();
                }
                else
                {
                    warehousequery = @" update  Warehouses set IsAutoRTPRunning=1 where WarehouseId= " + warehouseId + "";
                    await context.Database.ExecuteSqlCommandAsync(warehousequery);
                    context.Commit();
                }
                var query = "exec Operation.GetOrdersToAutoPick";
                if (warehouseId.HasValue && warehouseId.Value > 0)
                    query = "exec Operation.GetWarehouseOrdersToAutoPick " + warehouseId.Value;


                ordersToProcess = await context.Database.SqlQuery<OrdersForAutoPick>(query).ToListAsync();
                var ordersToProcessDT = ClassToDataTable.CreateDataTable(ordersToProcess);
                ordersToProcessDT.TableName = "DbOrders";
                dataTables.Add(ordersToProcessDT);
            }



            //var listNullValues = ordersToProcess.Where(x => !x.CustomerId.HasValue || !x.CurrentInventory.HasValue || !x.IsBlueOrder.HasValue
            //                        || !x.IsDelayDelivery.HasValue || !x.IsDispatchedFreeStock.HasValue || !x.IsFreeItem.HasValue
            //                        || !x.ItemMultiMrpId.HasValue || !x.OrderId.HasValue || !x.Qty.HasValue || !x.RequiredQty.HasValue
            //                        || !x.WarehouseId.HasValue

            //).ToList();

            //var orderd = ordersToProcess.Where(x => x.OrderId == 323404).ToList();

            var itemsList = ordersToProcess.GroupBy(x => new { x.WarehouseId, x.ItemMultiMrpId })
                .Select(x => new ItemsForAutoPick
                {
                    WarehouseId = x.Key.WarehouseId,
                    ItemMultiMrpId = x.Key.ItemMultiMrpId,
                    CurrentInventory = x.FirstOrDefault().CurrentInventory,
                    OriginalInventory = x.FirstOrDefault().CurrentInventory,
                    FreeInventory = x.FirstOrDefault().FreeInventory,
                    OriginalFreeInventory = x.FirstOrDefault().FreeInventory,
                    RequiredQty = x.Sum(z => z.Qty),
                    CustomerCount = x.Select(z => z.CustomerId).Distinct().Count(),
                    OrderCount = x.Select(z => z.OrderId).Distinct().Count()
                }).ToList();

            #region Blue Ordersz
            finalListForReadyToPick.AddRange(ordersToProcess.Where(x => x.OrderColor == "blue"));

            finalListForReadyToPick.GroupBy(x => new { x.ItemMultiMrpId, x.WarehouseId }).ToList().ForEach(x =>
            {
                var item = itemsList.FirstOrDefault(z => z.ItemMultiMrpId == x.Key.ItemMultiMrpId && z.WarehouseId == x.Key.WarehouseId);

                x.ToList().ForEach(z =>
                {
                    z.IsFulfilled = true;
                    z.FulfilledQty = z.Qty;
                    if (!z.IsDispatchedFreeStock)
                        item.CurrentInventory -= z.Qty;
                    else
                        item.FreeInventory -= z.Qty;
                    item.FulfilledQty += z.Qty;

                });

            });



            ordersToProcess.RemoveAll(x => x.OrderColor == "blue");


            #endregion


            #region Other orders based on Max orders delivery

            if (!warehouseId.HasValue || warehouseId.Value == 0)
            {
                itemsList//.Where(s => s.CurrentInventory > 0 || s.FreeInventory > 0)
                        .OrderByDescending(x => x.OrderCount).ToList()
                        .ForEach(x =>
                        {
                            var orders = ordersToProcess.Where(z => z.WarehouseId == x.WarehouseId && z.ItemMultiMrpId == x.ItemMultiMrpId)
                                          .OrderBy(z => z.PrioritizedDate)
                                          .OrderByDescending(z => z.IsDigitalOrder)
                                          .ThenByDescending(z => z.DaysSinceOrdered)
                                          .ThenBy(z => z.CustomerSubType)
                                          .ThenByDescending(z => z.IsOnline)
                                          .ThenByDescending(z => z.IsSelfOrder)
                                          .ThenBy(z => z.OrderAmount).ToList();

                            foreach (var order in orders.OrderByDescending(z => z.IsFreeItem))
                            {

                                if (ordersToProcess.Where(s => s.OrderId == order.OrderId).Max(s => s.IsLessInventory) == false)
                                {
                                    if ((!order.IsDispatchedFreeStock && order.Qty <= x.CurrentInventory) || (order.IsDispatchedFreeStock && order.Qty <= x.FreeInventory))
                                    {
                                        order.IsFulfilled = true;

                                        if (!order.IsDispatchedFreeStock)
                                            x.CurrentInventory -= order.Qty;
                                        else
                                            x.FreeInventory -= order.Qty;

                                        order.FulfilledQty = order.Qty;
                                        x.FulfilledQty += order.FulfilledQty;
                                    }
                                    else if ((!order.IsDispatchedFreeStock && order.Qty > x.CurrentInventory) || (order.IsDispatchedFreeStock && order.Qty > x.FreeInventory))
                                    {

                                        #region Yellow Order

                                        if (order.OrderColor == "yellow")
                                        {
                                            if (order.IsOnline == 0)
                                            {

                                                if (!order.IsDispatchedFreeStock && !order.IsFreeItem)
                                                {
                                                    //if (x.CurrentInventory / order.MOQ > 0)
                                                    //{
                                                    var processedAmount = ordersToProcess.Where(s => s.OrderId == order.OrderId && !s.IsCut
                                                                         && s.OrderDetailsId != order.OrderDetailsId).Sum(s => s.Qty * s.UnitPrice)
                                                                      +
                                                                        ordersToProcess.Where(s => s.OrderId == order.OrderId && s.IsCut
                                                                         && s.OrderDetailsId != order.OrderDetailsId).Sum(s => s.FulfilledQty * s.UnitPrice)
                                                                         ;
                                                    var thisItemFulfilledQty = order.MOQ * (x.CurrentInventory / order.MOQ);

                                                    processedAmount += (thisItemFulfilledQty * order.UnitPrice);

                                                    var yellowPercentAmount = order.OrderAmount - (order.OrderAmount * 4 / 100);

                                                    if (processedAmount < yellowPercentAmount)
                                                        order.IsLessInventory = true;
                                                    else
                                                    {
                                                        order.FulfilledQty = thisItemFulfilledQty;
                                                        x.CurrentInventory -= order.FulfilledQty;
                                                        order.IsFulfilled = true;
                                                        order.IsCut = true;
                                                        x.FulfilledQty += order.FulfilledQty;
                                                    }
                                                    //}
                                                    //else
                                                    //    order.IsLessInventory = true;
                                                }
                                                else
                                                {
                                                    order.IsLessInventory = true;
                                                }

                                            }
                                            else
                                                order.IsLessInventory = true;
                                        }

                                        #endregion
                                        else
                                            order.IsLessInventory = true;


                                        if (order.IsLessInventory)
                                        {
                                            var otherItems = ordersToProcess.Where(z => z.OrderId == order.OrderId && z.IsFulfilled).ToList();

                                            if (otherItems != null && otherItems.Any())
                                            {
                                                foreach (var otherOrderItem in otherItems)
                                                {
                                                    var item = itemsList.FirstOrDefault(a => a.ItemMultiMrpId == otherOrderItem.ItemMultiMrpId
                                                                                          && a.WarehouseId == order.WarehouseId);
                                                    if (otherOrderItem.IsDispatchedFreeStock)
                                                    {
                                                        item.FreeInventory += otherOrderItem.FulfilledQty;
                                                        item.FulfilledQty -= otherOrderItem.FulfilledQty;
                                                    }
                                                    else if (!otherOrderItem.IsDispatchedFreeStock)
                                                    {
                                                        item.CurrentInventory += otherOrderItem.FulfilledQty;
                                                        item.FulfilledQty -= otherOrderItem.FulfilledQty;
                                                    }
                                                    otherOrderItem.FulfilledQty = 0;
                                                    otherOrderItem.IsFulfilled = false;
                                                }
                                            }
                                        }
                                    }

                                }

                            }

                        });

                var fulfilledOrders = ordersToProcess.Where(s => ordersToProcess.GroupBy(x => x.OrderId)
                                 .Select(x => new
                                 {
                                     OrderId = x.Key,
                                     IsFulfilled = x.Min(z => z.IsFulfilled)
                                 }).Where(x => x.IsFulfilled).Select(x => x.OrderId).ToList().Contains(s.OrderId));

                //var ordersNotprocessed =
                //    ordersToProcess.Where(q => ordersToProcess.Where(x => x.IsFulfilled && !fulfilledOrders.Select(s => s.OrderId).Contains(x.OrderId))
                //                                .Select(w => w.OrderId).Contains(q.OrderId)).ToList();


                finalListForReadyToPick.AddRange(fulfilledOrders);
            }

            //finalListForReadyToPick.GroupBy(x => new { x.ItemMultiMrpId, x.WarehouseId }).ToList().ForEach(x =>
            //{
            //    itemsList.Where(z => z.ItemMultiMrpId == x.Key.ItemMultiMrpId && z.WarehouseId == x.Key.WarehouseId)
            //            .ToList().ForEach(s => s.FulfilledQty = x.Sum(a => a.FulfilledQty));
            //});

            #endregion

            if (finalListForReadyToPick != null && finalListForReadyToPick.Any())
            {
                #region Generate File

                string ExcelSavePath = HttpContext.Current.Server.MapPath("~/ExcelGeneratePath/AutoRTP/");

                if (!Directory.Exists(ExcelSavePath))
                    Directory.CreateDirectory(ExcelSavePath);

                var fileName = "AutoReadyToPick" + DateTime.Now.ToString("yyyyddMHHmmss") + ".xlsx";
                string filePath = ExcelSavePath + fileName;

                var ordersToAutoPick = ClassToDataTable.CreateDataTable(finalListForReadyToPick);
                ordersToAutoPick.TableName = "finalListForReadyToPick";
                dataTables.Add(ordersToAutoPick);

                var remainingStock = ClassToDataTable.CreateDataTable(itemsList);
                remainingStock.TableName = "remainingStock";
                dataTables.Add(remainingStock);

                ExcelGenerator.DataTable_To_Excel(dataTables, filePath, false);

                #endregion

                //var topOrders = finalListForReadyToPick.Where(x => x.OrderColor == "yellow" && x.IsCut).Select(x => x.OrderId).Distinct().Take(5);


                ReadyToPickDispatchedHelper helper = new ReadyToPickDispatchedHelper();


                if (finalListForReadyToPick != null && finalListForReadyToPick.Any())
                {
                    var list = finalListForReadyToPick.GroupBy(x => x.WarehouseId).ToList();
                    ParallelLoopResult parellelResult = Parallel.ForEach(list, new ParallelOptions { MaxDegreeOfParallelism = 5 }, (x) =>
                   {
                       var newList = AsyncContext.Run(() => helper.GenerateOrderAutoPick(x.ToList() /*.Where(x => topOrders.Contains(x.OrderId)).ToList()*/));

                       try
                       {
                           using (var conn = new SqlConnection(DbConstants.AuthContextDbConnection))
                           {
                               conn.Open();
                               using (SqlBulkCopy copy = new SqlBulkCopy(conn))
                               {
                                   copy.BulkCopyTimeout = 3600;
                                   copy.BatchSize = 20000;
                                   copy.DestinationTableName = "Operation.OrdersToProcessForAutoPick";
                                   DataTable table = ClassToDataTable.CreateDataTable(newList);
                                   table.TableName = "Operation.OrdersToProcessForAutoPick";
                                   copy.WriteToServer(table);
                               }
                           }
                       }
                       catch (Exception ex)
                       {
                           TextFileLogHelper.LogError(ex.ToString());
                           //throw;
                       }
                   });

                    if (parellelResult.IsCompleted)
                    {
                        using (var db = new AuthContext())
                        {
                            if (!warehouseId.HasValue || warehouseId.Value == 0)
                            {
                                warehousequery = @" update w set w.IsAutoRTPRunning=0 from Warehouses w   
                                        inner join Warehouses ws on ws.WarehouseId=w.WarehouseId  
                                        inner join GMWarehouseProgresses b on w.WarehouseId = b.WarehouseID and b.IsLaunched=1  
                                        and w.active=1 and w.Deleted=0 and w.IsKPP=0 and w.CityName not like '%test%'   ";
                                await db.Database.ExecuteSqlCommandAsync(warehousequery);
                                db.Commit();
                            }
                            else
                            {
                                warehousequery = @" update  Warehouses set IsAutoRTPRunning=0 where WarehouseId= " + warehouseId + "";
                                await db.Database.ExecuteSqlCommandAsync(warehousequery);
                                db.Commit();
                            }
                        }
                        return new ResultViewModel<string>
                        {
                            ErrorMessage = "",
                            IsSuceess = true,
                            ResultItem = "",
                            ResultList = null,
                            SuccessMessage = "Success"
                        };
                    }
                }


            }

            // #region Create Trips

            // TripPlannerHelper tripPlannerHelper = new TripPlannerHelper();
            // if (warehouseId.HasValue && warehouseId.Value > 0)
            // {
            //     var result = await tripPlannerHelper.GenerateAllTrip(0, false, warehouseId.Value);
            //     return result;
            // }
            // else
            // {
            //     var result = await tripPlannerHelper.GenerateAllTrip(0);
            //     return result;
            // }
            // #endregion
            using (var db = new AuthContext())
            {
                if (!warehouseId.HasValue || warehouseId.Value == 0)
                {
                    warehousequery = @" update w set w.IsAutoRTPRunning=0 from Warehouses w   
                                        inner join Warehouses ws on ws.WarehouseId=w.WarehouseId  
                                        inner join GMWarehouseProgresses b on w.WarehouseId = b.WarehouseID and b.IsLaunched=1  
                                        and w.active=1 and w.Deleted=0 and w.IsKPP=0 and w.CityName not like '%test%'   ";
                    await db.Database.ExecuteSqlCommandAsync(warehousequery);
                    db.Commit();
                }
                else
                {
                    warehousequery = @" update  Warehouses set IsAutoRTPRunning=0 where WarehouseId= " + warehouseId + "";
                    await db.Database.ExecuteSqlCommandAsync(warehousequery);
                    db.Commit();
                }
            }
            return new ResultViewModel<string>
            {
                ErrorMessage = "",
                IsSuceess = true,
                ResultItem = "",
                ResultList = null,
                SuccessMessage = "Success"
            };

        }

        [HttpPost]
        [Route("GenerateNonLastMileTrip")]
        public async Task<ResultViewModel<string>> GenerateNonLastMileTrip(GenerateTripParam tripParam)
        {
            #region Create Trips
            if (tripParam.TripType == (int)TripTypeEnum.Damage_Expiry
                || (tripParam.TripType == (int)TripTypeEnum.SKP && tripParam.CustomerId > 0)
                || (tripParam.TripType == (int)TripTypeEnum.KPP && tripParam.CustomerId > 0)
                || (tripParam.TripType == (int)TripTypeEnum.NonSellable)
                || (tripParam.TripType == (int)TripTypeEnum.NonRevenue)
            )
            {
                TripPlannerHelper tripPlannerHelper = new TripPlannerHelper();
                var result = await tripPlannerHelper.GenerateAllTrip(0, false, tripParam.WarehouseId, (TripTypeEnum)tripParam.TripType, tripParam.CustomerId);
                return result;
            }
            else
            {
                return new ResultViewModel<string>
                {
                    IsSuceess = false,
                    ErrorMessage = "Something went wrong"
                };
            }
            #endregion
        }


        [HttpGet]
        [Route("GetCustomerTrip/{Id}/{CustomerId}")]
        public async Task<CustomerTrip> GetCustomerTrip(int Id, int CustomerId)
        {
            CustomerTrip customerTrip = new CustomerTrip();
            using (var context = new AuthContext())
            {
                var query = "exec operation.GetCustomerTripData " + Id + "," + CustomerId;
                var trips = await context.Database.SqlQuery<CustomerTripDataFromDb>(query).ToListAsync();
                if (trips != null && trips.Any())
                {
                    customerTrip = trips.GroupBy(x => new
                    {
                        CustomerId = x.CustomerId,
                        DBoyId = x.DBoyId,
                        DBoyName = x.DBoyName,
                        DboyMobile = x.DboyMobile,
                        DboyProfilePic = x.DboyProfilePic,
                        DeliveryBoyRating = x.DeliveryBoyRating,
                        Lat = x.Lat,
                        Lng = x.Lng,
                        ShippingAddress = x.ShippingAddress,
                        TripId = x.TripId,
                        TripPlannerVehicleId = x.TripPlannerVehicleId
                    })
                        .Select(x => new CustomerTrip
                        {
                            CustomerId = x.Key.CustomerId,
                            DBoyId = x.Key.DBoyId,
                            DBoyName = x.Key.DBoyName,
                            DboyMobile = x.Key.DboyMobile,
                            DboyProfilePic = x.Key.DboyProfilePic,
                            DeliveryBoyRating = x.Key.DeliveryBoyRating,
                            Lat = x.Key.Lat,
                            Lng = x.Key.Lng,
                            ShippingAddress = x.Key.ShippingAddress,
                            TripId = x.Key.TripId,
                            TripPlannerVehicleId = x.Key.TripPlannerVehicleId,
                            Orders = x.Select(z => new TripOrders
                            {
                                IsPaid = z.IsPaid,
                                OrderAmount = z.OrderAmount,
                                PayableAmount = z.PayableAmount,
                                OrderId = z.OrderId,
                                OTP = z.OTP
                            }).ToList()
                        }).FirstOrDefault();
                }

            }
            if (customerTrip != null)
            {
                customerTrip.CollectionName = ConfigurationManager.AppSettings["vehicleHistoryCollectionName"].ToString();
            }
            return customerTrip;
        }


        [HttpGet]
        [Route("GetOrderDetail/{OrderId}")]
        public async Task<List<MyOrderDetailDc>> GetOrderDetail(int OrderId)
        {

            //var loggedInUser = HttpContext.Current != null && HttpContext.Current.User != null && HttpContext.Current.User.Identity != null ? HttpContext.Current.User.Identity.Name : "System";
            //int customerId = 0;

            //if ((string.IsNullOrEmpty(loggedInUser) || loggedInUser == "RetailerApp")
            //    && HttpContext.Current != null && HttpContext.Current.Request != null && HttpContext.Current.Request.Headers.AllKeys.Any(x => x == "username"))
            //{
            //    loggedInUser = Convert.ToString(HttpContext.Current.Request.Headers.GetValues("username").FirstOrDefault());

            //    customerId = loggedInUser.Split('_').Length > 1 ? Convert.ToInt32(loggedInUser.Split('_')[1]) : 0;
            //}

            List<MyOrderDetailDc> orderDetail = new List<MyOrderDetailDc>();
            List<DialValuePoint> DialValues = new List<DialValuePoint>();
            using (var context = new AuthContext())
            {
                OrderProcessManager manager = new OrderProcessManager();
                orderDetail = manager.GetOrderDetails(OrderId);
                if (orderDetail != null && orderDetail.Any())
                {
                    var fromDate = DateTime.Now.AddHours(-24);
                    var toDate = DateTime.Now;
                    DialValues = context.DialValuePointDB.Where(dp => dp.OrderId.HasValue && dp.OrderId.Value == OrderId && !dp.IsPlayWeel
                                                          && dp.CreatedDate >= fromDate && dp.CreatedDate <= toDate).ToList();
                    orderDetail.ForEach(x =>
                    {
                        var orderPayment = x.OrderPaymentDCs;
                        x.OrderPaymentDCs = orderPayment.Where(z => z.PaymentFrom.ToLower() != "cash").Select(z => new OrderPaymentDCs
                        {
                            Amount = z.Amount,
                            PaymentFrom = z.PaymentFrom,
                            TransactionNumber = z.TransactionNumber,
                            TransactionDate = z.TransactionDate,
                            statusDesc = z.statusDesc
                        }).ToList();
                        x.IsPlayWeel = DialValues != null && DialValues.Any(y => y.OrderId == x.orderid && !string.IsNullOrEmpty(y.EarnWheelList));
                        x.WheelCount = DialValues != null && DialValues.Any(y => y.OrderId == x.orderid && !string.IsNullOrEmpty(y.EarnWheelList)) ? DialValues.FirstOrDefault(y => y.OrderId == x.orderid).EarnWheelCount : 0;
                        x.WheelList = DialValues != null && DialValues.Any(y => y.OrderId == x.orderid && !string.IsNullOrEmpty(y.EarnWheelList)) ? DialValues.FirstOrDefault(y => y.OrderId == x.orderid).EarnWheelList.Split(',').Select(y => Convert.ToInt32(y)).ToList() : new List<int>();
                        x.IsOrderHold = context.ReadyToPickHoldOrders.Where(Holdorder => Holdorder.OrderId == x.orderid && Holdorder.IsActive == true && Holdorder.IsDeleted == false).Count() > 0 ? true : false;

                        //x.IsETAEnable = x.IsOrderHold == true ? false : true;
                        if (x.PrioritizedDate == null)
                        {
                            x.IsETAEnable = x.IsOrderHold == true ? false : true;
                        }
                        else
                        {
                            x.IsETAEnable = false;
                        }
                    });
                }
                return orderDetail;
            }
        }
        [HttpGet]
        [Route("GetOrderETADate/{OrderId}")]
        public async Task<ETADate> GetOrderETADate(int OrderId)
        {
            ETADate eTADate = new ETADate();
            using (var context = new AuthContext())
            {
                var order = await context.DbOrderMaster.FirstOrDefaultAsync(x => x.OrderId == OrderId);
                if(order.PrioritizedDate == null)
                {
                    Managers.RetailerAppManager retailerApp = new Managers.RetailerAppManager();
                    List<AngularJSAuthentication.DataContracts.External.NextETADate> NextETADate = retailerApp.GetNextETADate(order.WarehouseId, OrderId);
                    if (NextETADate != null)
                    {
                        eTADate.ETADates = new List<ETADateDc>();
                        //var Deliverydate = NextETADate.Min(x => x.NextDeliveryDate);
                        eTADate.ETADates.AddRange(NextETADate.Where(x => x.NextDeliveryDate.Date >= DateTime.Now.Date).Select(x => new ETADateDc { ETADate = x.NextDeliveryDate }).ToList());
                    }
                    else
                    {
                        var CreatedDate = order.CreatedDate;//await context.DbOrderMaster.Where(x => x.OrderId == OrderId).Select(x => x.CreatedDate).FirstOrDefaultAsync();
                                                            //if (CreatedDate != null)
                                                            //{
                        eTADate.ETADates = new List<ETADateDc>();
                        eTADate.ETADates.Add(new ETADateDc { ETADate = CreatedDate.AddDays(2) });
                        eTADate.ETADates.Add(new ETADateDc { ETADate = CreatedDate.AddDays(3) });
                        eTADate.ETADates.Add(new ETADateDc { ETADate = CreatedDate.AddDays(4) });
                        eTADate.ETADates.Add(new ETADateDc { ETADate = CreatedDate.AddDays(5) });
                        eTADate.ETADates.Where(x => x.ETADate.Date >= DateTime.Now.Date);

                    }
                }
                else
                {
                    eTADate.ETADates = new List<ETADateDc>();
                }
                return eTADate;
            }
        }


        public class GenerateTripParam
        {
            public int WarehouseId { get; set; }
            public int TripType { get; set; }
            public int CustomerId { get; set; }
        }
    }
}

public class ETADate
{
    public List<ETADateDc> ETADates { get; set; }
}
public class ETADateDc
{
    public DateTime ETADate { get; set; }
}


