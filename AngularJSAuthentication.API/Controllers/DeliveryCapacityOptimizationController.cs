using AgileObjects.AgileMapper;
using AngularJSAuthentication.API.Helpers;
using AngularJSAuthentication.DataContracts.DeliveryCapacityOptimization;
using AngularJSAuthentication.DataContracts.Masters;
using AngularJSAuthentication.DataContracts.Mongo;
using AngularJSAuthentication.Model;
using AngularJSAuthentication.Model.DeliveryCapacityOptimization;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Web.Http;
using LinqKit;
using System.Data;

namespace AngularJSAuthentication.API.Controllers
{
    [RoutePrefix("api/DeliveryCapacity")]

    public class DeliveryCapacityOptimizationController : ApiController
    {

        //[Route("GetData2")]
        //[HttpPost]
        //public List<OrderedAndDeliverdCountDC> GetData(OrderedAndDeliverdCountFilterDC Data)
        //{

        //    var identity = User.Identity as ClaimsIdentity;
        //    int userid = 0;

        //    if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
        //        userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
        //    using (AuthContext context = new AuthContext())
        //    {
        //        MongoDbHelper<DeliveryCapacityOpti> mongoDbHelper = new MongoDbHelper<DeliveryCapacityOpti>();
        //        List<DeliveryCapacityOpti> list = new List<DeliveryCapacityOpti>();
        //        List<OrderedAndDeliverdCountDC> newdata = new List<OrderedAndDeliverdCountDC>();

        //        var CurrentYear = DateTime.Now;

        //        if ((Data.Fromdate.Year <= CurrentYear.Year) && (Data.Fromdate.Month < CurrentYear.Month))
        //        {
        //            //mongo
        //            var data = mongoDbHelper.Select(x => x.Warehouseid == Data.warehouseId && x.DeliveryDate <= Data.Todate && x.DeliveryDate >= Data.Fromdate).ToList();

        //            List<OrderedAndDeliverdCountDC> result = new List<OrderedAndDeliverdCountDC>();
        //            result = Mapper.Map(data).ToANew<List<OrderedAndDeliverdCountDC>>();
        //            return result;
        //        }
        //        else
        //        {
        //            //mongo + database

        //            var today = DateTime.Now.Date;
        //            var firstDayOfMonth = new DateTime(today.Year, today.Month, 1);
        //            var Yesterday = DateTime.Now.AddDays(-1).Date;
        //            //var LastDayData = mongoDbHelper.Select(x => x.Warehouseid == Data.warehouseId && x.DeliveryDate == today).FirstOrDefault();
        //            //var LastDayData = mongoDbHelper.Select(x => x.Warehouseid == Data.warehouseId && x.DeliveryDate <= Yesterday && x.DeliveryDate >= firstDayOfMonth).ToList();

        //            var LastData = mongoDbHelper.Select(x => x.Warehouseid == Data.warehouseId).LastOrDefault();

        //            DateTime? startdate = null;
        //            List<DeliveryCapacityOpti> LastDayData = new List<DeliveryCapacityOpti>();

        //            var CurrentMonth = new DateTime(today.Year, today.Month, 1);

        //            if (DateTime.Now.Date < Data.Fromdate)
        //            {
        //                startdate = Data.Fromdate;
        //            }
        //            else
        //            {
        //                if (LastData != null)
        //                {
        //                    var Lastdate = LastData.CreatedDate;
        //                    LastDayData = mongoDbHelper.Select(x => x.Warehouseid == Data.warehouseId && x.DeliveryDate <= Lastdate && x.DeliveryDate >= firstDayOfMonth).ToList();
        //                    startdate = Lastdate.AddDays(1);
        //                }
        //                else
        //                {
        //                    startdate = DateTime.Now.Date;
        //                }
        //            }

        //            var fdate = new SqlParameter("@startDate", startdate);
        //            var TOdate = new SqlParameter("@endDate", Data.Todate);
        //            var Warehouseid = new SqlParameter("@warehousId", Data.warehouseId);
        //            var Year = new SqlParameter("@Year", Data.Year);

        //            newdata = context.Database.SqlQuery<OrderedAndDeliverdCountDC>("EXEC SpOrderAndDeliveredCountNew @startDate,@endDate,@warehousId,@Year", fdate, TOdate, Warehouseid, Year).ToList();
        //            newdata = newdata.Where(x => x.DeliveryDate == today).ToList();

        //            var Today = DateTime.Now;
        //            var FirstDayOfMonth = new DateTime(today.Year, today.Month, 1);
        //            var lastDayLastMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);

        //            List<OrderedAndDeliverdCountDC> Listobj = new List<OrderedAndDeliverdCountDC>();
        //            List<OrderedAndDeliverdCountDC> ResultObj = new List<OrderedAndDeliverdCountDC>();

        //            if (LastDayData.Count > 0)
        //            {
        //                foreach (var i in LastDayData)
        //                {
        //                    OrderedAndDeliverdCountDC obj = new OrderedAndDeliverdCountDC();
        //                    obj.DeliveryDate = i.DeliveryDate;
        //                    obj.ETADelayDate = i.ETADelayDate;
        //                    obj.OrderCapacity = i.OrderCapacity;
        //                    obj.OrderedCount = i.OrderedCount;
        //                    obj.ThresholdCount = i.ThresholdCount;
        //                    obj.UpdateEta = i.UpdateEta;
        //                    obj.DeliveredPercent = i.DeliveredPercent;
        //                    obj.DeliveredCount = i.DeliveredCount;
        //                    obj.cumCountPending = i.cumCountPending;
        //                    Listobj.Add(obj);
        //                }
        //            }

        //            var lastpending = LastDayData.Count > 0 ? Listobj.LastOrDefault().cumCountPending : 0;

        //            if (newdata != null)
        //            {
        //                DateTime? ExpactedDeliveryDate = null;

        //                int prepending = lastpending;
        //                foreach (var item in newdata)
        //                {
        //                    ExpactedDeliveryDate = null;
        //                    item.cumCountPending = prepending < 0 ? 0 : prepending;
        //                    lastpending = (item.cumCountPending + item.OrderedCount - item.DeliveredCount);// - item.ThresholdCount;
        //                    prepending = lastpending;

        //                    if (lastpending > 0)
        //                    {
        //                        if (item.ThresholdCount < lastpending)
        //                        {
        //                            foreach (var nextitem in newdata.Where(x => x.DeliveryDate > item.DeliveryDate))
        //                            {
        //                                lastpending = (lastpending + nextitem.OrderedCount - nextitem.DeliveredCount) - nextitem.ThresholdCount;
        //                                if (lastpending < 0)
        //                                {
        //                                    if (nextitem.ThresholdCount != 0)
        //                                    {
        //                                        ExpactedDeliveryDate = nextitem.DeliveryDate;
        //                                        break;
        //                                    }
        //                                    else
        //                                    {
        //                                        ExpactedDeliveryDate = newdata.FirstOrDefault(x => x.DeliveryDate > nextitem.DeliveryDate && x.ThresholdCount > 0)?.DeliveryDate;
        //                                    }
        //                                }
        //                            }
        //                        }
        //                        else
        //                        {
        //                            ExpactedDeliveryDate = item.DeliveryDate;
        //                        }
        //                    }
        //                    else
        //                    {
        //                        ExpactedDeliveryDate = item.DeliveryDate;
        //                    }
        //                    OrderedAndDeliverdCountDC obj = new OrderedAndDeliverdCountDC();
        //                    obj.DeliveryDate = item.DeliveryDate;
        //                    obj.ETADelayDate = ExpactedDeliveryDate.HasValue ? ExpactedDeliveryDate.Value : Data.Todate;
        //                    obj.OrderCapacity = item.OrderCapacity;
        //                    obj.OrderedCount = item.OrderedCount;
        //                    obj.ThresholdCount = item.ThresholdCount;
        //                    obj.UpdateEta = 0;
        //                    obj.DeliveredPercent = item.lastcapacity > 0 ? (item.DeliveredCount / item.lastcapacity) * 100 : 0;
        //                    obj.DeliveredCount = item.DeliveredCount;
        //                    obj.cumCountPending = prepending;// item.cumCountPending;
        //                    Listobj.Add(obj);

        //                }
        //            }
        //            return Listobj;
        //        }

        //    }
        //}

        public List<OrderedAndDeliverdCountDC> result(List<OrderedAndDeliverdCountDC> ResultObj, DateTime? StartDeliveryDate, DateTime? OrderDeliverdTillDate)
        {
            foreach (var m in ResultObj)
            {
                if (m.DeliveryDate >= StartDeliveryDate && m.DeliveryDate <= OrderDeliverdTillDate)
                {
                    m.ETADelayDate = OrderDeliverdTillDate.Value;
                }
            }
            return ResultObj;
        }



        [Route("SelectedDefaultCapacity")]
        [HttpPost]
        public List<SelectedList> SelectedData(SelectedData select)
        {
            List<SelectedList> DC = new List<SelectedList>();

            var identity = User.Identity as ClaimsIdentity;

            int userid = 0;

            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
            using (AuthContext context = new AuthContext())
            {
                var Wareid = new SqlParameter("@WarehouseID", select.warehouseid);
                var Year = new SqlParameter("@Year", select.year);

                DC = context.Database.SqlQuery<SelectedList>("EXEC SpSelectedData @WarehouseID,@Year", Wareid, Year).ToList();
                return DC;
            }
        }

        [Route("SelectedUpdatedCapacity")]
        [HttpPost]
        public UpdateCapacity SelectedUpdateData(selectedCapacity updt)
        {
            UpdateCapacity dc = new UpdateCapacity();
            var identity = User.Identity as ClaimsIdentity;

            int userid = 0;


            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
            using (AuthContext context = new AuthContext())
            {
                var Wareid = new SqlParameter("@WarehouseID", updt.warehouseId);
                var date = new SqlParameter("@Date", updt.Date);

                var Year = new SqlParameter("@Year", updt.year);

                dc = context.Database.SqlQuery<UpdateCapacity>("EXEC SelectedPopUpData @WarehouseID ,@Date,@Year", Wareid, date, Year).FirstOrDefault();
                return dc;
            }
        }


        [Route("DefaultDeliveryCapacity")]
        [HttpPost]
        public bool DefaultDeliveryCapacity(DeliveryCapacity DCO)

        {
            //int Count = DCO.Holidays.Count;
            bool result = true;
            var identity = User.Identity as ClaimsIdentity;
            int userid = 0;


            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
            using (AuthContext context = new AuthContext())
            {
                WarehouseCapacity WC = new WarehouseCapacity();
                WarehouseHoliday WH = new WarehouseHoliday();
                List<WarehouseCapacity> WCs = new List<WarehouseCapacity>();
                List<WarehouseHoliday> WHs = new List<WarehouseHoliday>();

                var ExistingDefaultCapacity = context.WarehouseCapacities.Where(x => DCO.warehouseIds.Contains(x.Warehouseid) && x.Year == DCO.year && (x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value))).ToList();
                var ExistingDefaultHoliday = context.WarehouseHolidays.Where(x => DCO.warehouseIds.Contains(x.Warehouseid) && x.Year == DCO.year && (x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value))).ToList();
                var warehouseIdList = ExistingDefaultCapacity.Select(x => x.Warehouseid).ToList();
                var NotExistWid = DCO.warehouseIds.Where(x => !warehouseIdList.Contains(x)).ToList();


                if (NotExistWid.Count > 0)
                {

                    foreach (var data in NotExistWid)
                    {
                        WC = new WarehouseCapacity();
                        WC.Warehouseid = data;
                        WC.DefaultCapacity = DCO.DefaultCapacity;
                        WC.IsActive = true;
                        WC.IsDeleted = false;
                        WC.CreatedDate = DateTime.Now;
                        WC.Year = DCO.year;
                        WC.CreatedBy = userid;
                        WCs.Add(WC)
;
                        foreach (var holiday in DCO.Holidays)
                        {
                            WH = new WarehouseHoliday();
                            WH.Warehouseid = data;
                            WH.Holidays = holiday;
                            WH.IsActive = true;
                            WH.IsDeleted = false;
                            WH.CreatedDate = DateTime.Now;
                            WH.CreatedBy = userid;
                            WH.Year = DCO.year;
                            WHs.Add(WH);

                        }
                    }
                }

                if (ExistingDefaultCapacity.Count > 0)
                {
                    foreach (var item in ExistingDefaultCapacity)
                    {
                        if (item.CreatedDate.ToString("dd-MM-yyyy") == DateTime.Now.ToString("dd-MM-yyyy"))
                        {
                            item.ModifiedBy = userid;
                            item.ModifiedDate = DateTime.Now;
                            item.IsActive = true;
                            item.IsDeleted = false;
                            item.DefaultCapacity = DCO.DefaultCapacity;
                            context.Entry(item).State = EntityState.Modified;
                        }
                        else
                        {
                            item.ModifiedBy = userid;
                            item.ModifiedDate = DateTime.Now;
                            item.IsActive = false;
                            item.IsDeleted = true;
                            context.Entry(item).State = EntityState.Modified;

                            WC = new WarehouseCapacity();
                            WC.Warehouseid = item.Warehouseid;
                            WC.DefaultCapacity = DCO.DefaultCapacity;
                            WC.IsActive = true;
                            WC.IsDeleted = false;
                            WC.CreatedDate = DateTime.Now;
                            WC.Year = DCO.year;
                            WC.CreatedBy = userid;
                            WCs.Add(WC)
;
                        }
                    }

                }

                if (ExistingDefaultHoliday.Count > 0)
                {
                    foreach (var data in ExistingDefaultHoliday)
                    {
                        data.ModifiedBy = userid;
                        data.ModifiedDate = DateTime.Now;
                        data.IsActive = false;
                        data.IsDeleted = true;
                        context.Entry(data).State = EntityState.Modified;

                    }
                }
                if (ExistingDefaultHoliday.Count > 0 && DCO.Holidays.Count > 0)
                {
                    foreach (var data in DCO.warehouseIds)
                    {
                        foreach (var holiday in DCO.Holidays)
                        {
                            WH = new WarehouseHoliday();
                            WH.Warehouseid = data;
                            WH.Holidays = holiday;
                            WH.IsActive = true;
                            WH.IsDeleted = false;
                            WH.CreatedDate = DateTime.Now;
                            WH.Year = DCO.year;
                            WH.CreatedBy = userid;
                            WHs.Add(WH);
                        }
                    }
                }


                if (WCs != null && WCs.Any())
                    context.WarehouseCapacities.AddRange(WCs);

                if (WHs != null && WHs.Any())
                    context.WarehouseHolidays.AddRange(WHs);

                result = context.Commit() > 0;

            }
            return result;

        }

        [Route("UpdatetDeliveryCapacity")]
        [HttpPost]


        public bool UpdateDeliveryCapacity(UpdateCapacity UDC)
        {

            bool result = true;

            var identity = User.Identity as ClaimsIdentity;
            int userid = 0;


            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
            using (AuthContext context = new AuthContext())
            {
                WarehouseUpdateCapacity WUC = new WarehouseUpdateCapacity();
                var existsDate = context.WarehouseUpdateCapacities.Where(x => UDC.warehouseId == x.Warehouseid && UDC.Date == x.Date && x.Date.Year == UDC.year && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value)).ToList();
                // var existwid=context.WarehouseUpdateCapacities.Where(x=>UDC.)

                if (existsDate.Any(x => x.Warehouseid == UDC.warehouseId && x.Date == UDC.Date))
                {
                    var dbdata = existsDate.FirstOrDefault(x => x.Warehouseid == UDC.warehouseId && x.Date == UDC.Date);
                    dbdata.Date = UDC.Date;
                    dbdata.ModifiedBy = userid;
                    dbdata.ModifiedDate = DateTime.Now;
                    dbdata.Year = UDC.year;
                    dbdata.UpdateCapacity = UDC.UpdateThresholdCapacity;
                    context.Entry(dbdata).State = EntityState.Modified;
                    context.Commit();
                }
                else
                {
                    WUC.Warehouseid = UDC.warehouseId;
                    WUC.Date = UDC.Date;
                    WUC.UpdateCapacity = UDC.UpdateThresholdCapacity;
                    WUC.IsActive = true;
                    WUC.IsDeleted = false;
                    WUC.CreatedDate = DateTime.Now;
                    WUC.Year = UDC.year;
                    WUC.CreatedBy = userid;
                    context.WarehouseUpdateCapacities.Add(WUC);
                    context.Commit();
                }


                result = context.Commit() > 0;

            }
            return result;

        }

        [Route("GetHistroyDataDeliveryCapacity")]
        [HttpGet]
        public List<DeliveryCapacityHistroyDataHistroyDC> GetHistroyData(int warehouseId)
        {
            using (AuthContext context = new AuthContext())
            {
                var warehouseno = new SqlParameter("@WarehouseId", warehouseId);
                var Data = context.Database.SqlQuery<DeliveryCapacityHistroyDataHistroyDC>("DeliveryCapacityHistroyData @WarehouseId", warehouseno).ToList();
                return Data;
            }
        }

        [Route("ClusterHolidayHistoryData")]
        [HttpGet]
        public List<ClusterHolidayHistroyDataHistroyDC> ClusterHolidayHistoryData(int ClusterId)
        {
            using (AuthContext context = new AuthContext())
            {
                var ClusterIdd = new SqlParameter("@ClusterId", ClusterId);
                var Data = context.Database.SqlQuery<ClusterHolidayHistroyDataHistroyDC>("ClusterHolidatHistroyData @ClusterId", ClusterIdd).ToList();
                return Data;
            }
        }

        [Route("CustomerHolidayHistoryData")]
        [HttpGet]
        public List<CustomerHolidayHistroyDataHistroyDC> CustomerHolidayHistoryData(int ClusterId)
        {
            using (AuthContext context = new AuthContext())
            {
                var ClusterIdd = new SqlParameter("@ClusterId", ClusterId);
                var Data = context.Database.SqlQuery<CustomerHolidayHistroyDataHistroyDC>("CustomerHolidatHistroyData @ClusterId", ClusterIdd).ToList();
                return Data;
            }
        }
        [Route("HolidayWorkingList")]
        [HttpGet]
        public AllList HolidayWorkingList(int warehouseid, int year)
        {

            AllList list = new AllList();
            var identity = User.Identity as ClaimsIdentity;
            int userid = 0;


            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
            using (AuthContext context = new AuthContext())
            {

                if (year != null)
                {
                    list.HolidayLists = (from w in context.WarehouseUpdateCapacities
                                         join x in context.Warehouses on w.Warehouseid equals x.WarehouseId
                                         join z in context.Peoples on (w.ModifiedBy != null ? w.ModifiedBy : w.CreatedBy) equals z.PeopleID
                                         where w.Warehouseid == warehouseid && w.Year == year && w.UpdateCapacity == 0 && w.IsActive == true && w.IsDeleted == false

                                         select new HolidayList
                                         {
                                             WarehouseName = x.WarehouseName,
                                             Date = w.Date,
                                             UpdateThresholdCapacity = w.UpdateCapacity,

                                             UserName = z.PeopleFirstName


                                         }).OrderByDescending(z => z.Date).ToList();

                    list.WorkingLists = (from w in context.WarehouseUpdateCapacities
                                         join x in context.Warehouses on w.Warehouseid equals x.WarehouseId
                                         join z in context.Peoples on (w.ModifiedBy != null ? w.ModifiedBy : w.CreatedBy) equals z.PeopleID
                                         where w.Warehouseid == warehouseid && w.Year == year && w.UpdateCapacity > 0 && w.IsActive == true && w.IsDeleted == false

                                         select new WorkingList

                                         {
                                             WarehouseName = x.WarehouseName,
                                             Date = w.Date,
                                             UpdateThresholdCapacity = w.UpdateCapacity,
                                             UserName = z.PeopleFirstName


                                         }).OrderByDescending(z => z.Date).ToList();
                }


                var wHolidays = context.WarehouseHolidays.Where(x => x.Warehouseid == warehouseid && x.Year == year && x.IsActive).Select(x => x.Holidays).ToList();
                List<DateTime> AllWHolidays = new List<DateTime>();
                DateTime start = new DateTime(year, 1, 1);
                DateTime end = start.AddYears(1).AddSeconds(-1);
                var allDate = Enumerable.Range(0, 1 + end.Subtract(start).Days)
                                .Select(offset =>
                                 start.AddDays(offset))
                           .ToList();
                allDate.ForEach(x =>
                {
                    if (wHolidays.Contains(x.ToString("dddd")))
                    {
                        AllWHolidays.Add(x);
                    }
                });
                list.Holidays = AllWHolidays;

            }
            return list;
        }


        //[Route("InsertDataInMongo")]
        //[HttpGet]
        //public bool InsertDeliveryCapcityDataInMongo()
        //{
        //    bool flag = false;
        //    var identity = User.Identity as ClaimsIdentity;
        //    int userid = 0;
        //    if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
        //        userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

        //    using (AuthContext context = new AuthContext())
        //    {
        //        MongoDbHelper<DeliveryCapacityOpti> mongoDbHelper = new MongoDbHelper<DeliveryCapacityOpti>();
        //        List<DeliveryCapacityOpti> list = new List<DeliveryCapacityOpti>();
        //        List<OrderedAndDeliverdCountDC> newdata = new List<OrderedAndDeliverdCountDC>();
        //        var today = DateTime.Now;
        //        var warids = context.Warehouses.Where(x => x.active && !x.Deleted).Select(x => x.WarehouseId).Distinct().ToList();
        //        foreach (var wid in warids)
        //        {

        //            var firstDayOfMonth = new DateTime(today.Year, today.Month, 1);
        //            var lastDayLastMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);
        //            var Yesterday = today.AddDays(-1).Date;
        //            var firstDay1 = new DateTime(Yesterday.Year, Yesterday.Month, 1);
        //            //var fdate = new SqlParameter("@startDate", DateTime.Now.Date);
        //            //var TOdate = new SqlParameter("@endDate", lastDayLastMonth);
        //            //var Warehouseid = new SqlParameter("@warehousId", wid);
        //            //var Year = new SqlParameter("@Year", DateTime.Now.Year);
        //            //var LastDayData = mongoDbHelper.Select(x => x.Warehouseid == wid && x.DeliveryDate <= Yesterday && x.DeliveryDate >= firstDayOfMonth).ToList();

        //            var LastDayDataMongo = mongoDbHelper.Select(x => x.Warehouseid == wid && x.CreatedDate >= firstDay1 && x.CreatedDate <= Yesterday).ToList();
        //            var LastData = mongoDbHelper.Select(x => x.Warehouseid == wid).LastOrDefault();
        //            DateTime startdate = today;
        //            List<DeliveryCapacityOpti> LastDayData = new List<DeliveryCapacityOpti>();
        //            int cumPending = 0;
        //            if (LastData != null)
        //            {
        //                if (Yesterday == LastData.CreatedDate.Date)
        //                {
        //                    startdate = Yesterday.AddDays(1);
        //                    LastDayData = LastDayDataMongo;
        //                }
        //                else
        //                {

        //                    var Lastdate = LastData.CreatedDate;
        //                    var firstDay = new DateTime(Lastdate.Year, Lastdate.Month, 1);
        //                    var enddate = firstDay.AddMonths(1).AddDays(-1);

        //                    var Fdate = Lastdate.AddDays(1);

        //                    var fdate2 = new SqlParameter("@startDate", Fdate);
        //                    var TOdate2 = new SqlParameter("@endDate", enddate);
        //                    var Warehouseid2 = new SqlParameter("@warehousId", wid);
        //                    var Year2 = new SqlParameter("@Year", Fdate.Year);

        //                    newdata = context.Database.SqlQuery<OrderedAndDeliverdCountDC>("EXEC SpOrderAndDeliveredCountNew @startDate,@endDate,@warehousId,@Year", fdate2, TOdate2, Warehouseid2, Year2).ToList();
        //                    foreach (var i in newdata)
        //                    {
        //                        DeliveryCapacityOpti obj = new DeliveryCapacityOpti();
        //                        obj.DeliveryDate = i.DeliveryDate;
        //                        obj.ETADelayDate = i.ETADelayDate;
        //                        obj.OrderCapacity = i.OrderCapacity;
        //                        obj.OrderedCount = i.OrderedCount;
        //                        obj.ThresholdCount = i.ThresholdCount;
        //                        obj.UpdateEta = i.UpdateEta;
        //                        obj.DeliveredPercent = i.DeliveredPercent;
        //                        obj.DeliveredCount = i.DeliveredCount;
        //                        obj.cumCountPending = i.cumCountPending;
        //                        LastDayData.Add(obj);
        //                    }
        //                    var date = LastDayData.OrderByDescending(x => x.DeliveryDate).LastOrDefault().DeliveryDate;
        //                    startdate = date;
        //                }
        //            }
        //            else
        //            {
        //                startdate = today;
        //                var firstDay = new DateTime(startdate.Year, startdate.Month, 1);
        //                lastDayLastMonth = firstDay.AddMonths(1).AddDays(-1);

        //                var d1 = startdate.AddDays(-1).Date;
        //                var d2 = today;
        //                var fdate3 = new SqlParameter("@startDate", d1);
        //                var TOdate3 = new SqlParameter("@endDate", d2);
        //                var Warehouseid3 = new SqlParameter("@warehousId", wid);
        //                var Year3 = new SqlParameter("@Year", d1.Year);

        //                newdata = context.Database.SqlQuery<OrderedAndDeliverdCountDC>("EXEC SpOrderAndDeliveredCountNew @startDate,@endDate,@warehousId,@Year", fdate3, TOdate3, Warehouseid3, Year3).ToList();
        //                cumPending = newdata.Count > 0 ? newdata.FirstOrDefault(x => x.DeliveryDate == Yesterday).cumCountPending : 0;

        //            }
        //            //if (LastDayData.Count > 0)
        //            //{
        //            //    startdate = DateTime.Now.Date;
        //            //}
        //            //else
        //            //{
        //            //    startdate = DateTime.Now.AddDays(-1);
        //            //}
        //            var fdate = new SqlParameter("@startDate", startdate);
        //            var TOdate = new SqlParameter("@endDate", lastDayLastMonth);
        //            var Warehouseid = new SqlParameter("@warehousId", wid);
        //            var Year = new SqlParameter("@Year", today.Year);

        //            newdata = context.Database.SqlQuery<OrderedAndDeliverdCountDC>("EXEC SpOrderAndDeliveredCountNew @startDate,@endDate,@warehousId,@Year", fdate, TOdate, Warehouseid, Year).ToList();

        //            List<OrderedAndDeliverdCountDC> Listobj = new List<OrderedAndDeliverdCountDC>();
        //            List<OrderedAndDeliverdCountDC> ResultObj = new List<OrderedAndDeliverdCountDC>();

        //            if (LastDayData.Count > 0)
        //            {
        //                foreach (var i in LastDayData.Where(x => x.DeliveryDate < startdate).OrderBy(x => x.DeliveryDate))
        //                {
        //                    OrderedAndDeliverdCountDC obj = new OrderedAndDeliverdCountDC();
        //                    obj.DeliveryDate = i.DeliveryDate;
        //                    obj.ETADelayDate = i.ETADelayDate;
        //                    obj.OrderCapacity = i.OrderCapacity;
        //                    obj.OrderedCount = i.OrderedCount;
        //                    obj.ThresholdCount = i.ThresholdCount;
        //                    obj.UpdateEta = i.UpdateEta;
        //                    obj.DeliveredPercent = i.DeliveredPercent;
        //                    obj.DeliveredCount = i.DeliveredCount;
        //                    obj.cumCountPending = i.cumCountPending;
        //                    Listobj.Add(obj);
        //                }
        //            }
        //            var lastpending = Listobj.Count > 0 ? Listobj.LastOrDefault().cumCountPending : cumPending;

        //            if (newdata != null)
        //            {
        //                DateTime? ExpactedDeliveryDate = null;
        //                //newdata.FirstOrDefault().cumCountPending = lastpending;
        //                int prepending = lastpending;
        //                foreach (var item in newdata)
        //                {
        //                    ExpactedDeliveryDate = null;
        //                    item.cumCountPending = prepending < 0 ? 0 : prepending;
        //                    lastpending = (item.cumCountPending + item.OrderedCount - item.DeliveredCount);// - item.ThresholdCount;
        //                    prepending = lastpending;

        //                    if (lastpending > 0)
        //                    {
        //                        if (item.ThresholdCount < lastpending)
        //                        {
        //                            foreach (var nextitem in newdata.Where(x => x.DeliveryDate > item.DeliveryDate))
        //                            {
        //                                lastpending = (lastpending + nextitem.OrderedCount - nextitem.DeliveredCount) - nextitem.ThresholdCount;
        //                                if (lastpending < 0)
        //                                {
        //                                    if (nextitem.ThresholdCount != 0)
        //                                    {
        //                                        ExpactedDeliveryDate = nextitem.DeliveryDate;
        //                                        break;
        //                                    }
        //                                    else
        //                                    {
        //                                        ExpactedDeliveryDate = newdata.FirstOrDefault(x => x.DeliveryDate > nextitem.DeliveryDate && x.ThresholdCount > 0)?.DeliveryDate;
        //                                    }
        //                                }
        //                            }
        //                        }
        //                        else
        //                        {
        //                            ExpactedDeliveryDate = item.DeliveryDate;
        //                        }
        //                    }
        //                    else
        //                    {
        //                        ExpactedDeliveryDate = item.DeliveryDate;
        //                    }
        //                    OrderedAndDeliverdCountDC obj = new OrderedAndDeliverdCountDC();
        //                    obj.DeliveryDate = item.DeliveryDate;
        //                    obj.ETADelayDate = ExpactedDeliveryDate.HasValue ? ExpactedDeliveryDate.Value : lastDayLastMonth;
        //                    obj.OrderCapacity = item.OrderCapacity;
        //                    obj.OrderedCount = item.OrderedCount;
        //                    obj.ThresholdCount = item.ThresholdCount;
        //                    obj.UpdateEta = 0;
        //                    obj.DeliveredPercent = item.lastcapacity > 0 ? (item.DeliveredCount / item.lastcapacity) * 100 : 0;
        //                    obj.DeliveredCount = item.DeliveredCount;
        //                    obj.cumCountPending = prepending;// item.cumCountPending;
        //                    Listobj.Add(obj);
        //                }

        //                var i = Listobj.Where(x => x.DeliveryDate.Date == today.Date).FirstOrDefault();
        //                if (i != null)
        //                {
        //                    DeliveryCapacityOpti DCO = new DeliveryCapacityOpti
        //                    {
        //                        DeliveryDate = i.DeliveryDate,
        //                        OrderedCount = i.OrderedCount,
        //                        DeliveredCount = i.DeliveredCount,
        //                        DeliveredPercent = i.DeliveredPercent,
        //                        cumCountPending = i.cumCountPending,
        //                        ThresholdCount = i.ThresholdCount,
        //                        ETADelayDate = i.ETADelayDate,
        //                        UpdateEta = i.UpdateEta,
        //                        OrderCapacity = i.OrderCapacity,
        //                        IsActive = true,
        //                        IsDeleted = false,
        //                        CreatedDate = today,
        //                        CreatedBy = userid,
        //                        Warehouseid = wid,
        //                        ModifiedDate = today
        //                    };
        //                    //flag = mongoDbHelper.Insert(DCO);
        //                }
        //            }
        //        }
        //        return flag;
        //    }
        //}

        //[Route("GetData3")]

        //[HttpPost]
        //public List<OrderedAndDeliverdCountDC> GetDataTest3(OrderedAndDeliverdCountFilterDC Data)
        //{

        //    using (AuthContext context = new AuthContext())
        //    {
        //        MongoDbHelper<DeliveryCapacityOpti> mongoDbHelper = new MongoDbHelper<DeliveryCapacityOpti>();
        //        List<DeliveryCapacityOpti> list = new List<DeliveryCapacityOpti>();
        //        List<OrderedAndDeliverdCountDC> newdata = new List<OrderedAndDeliverdCountDC>();

        //        var CurrentYear = DateTime.Now;
        //        if ((Data.Fromdate.Year <= CurrentYear.Year) && (Data.Fromdate.Month < CurrentYear.Month))
        //        {
        //            //mongo
        //            var data = mongoDbHelper.Select(x => x.Warehouseid == Data.warehouseId && x.DeliveryDate <= Data.Todate && x.DeliveryDate >= Data.Fromdate).ToList();

        //            List<OrderedAndDeliverdCountDC> result = new List<OrderedAndDeliverdCountDC>();
        //            result = Mapper.Map(data).ToANew<List<OrderedAndDeliverdCountDC>>();
        //            return result;
        //        }
        //        else
        //        {
        //            //mongo + database

        //            var today = DateTime.Now.Date;
        //            var firstDayOfMonth = new DateTime(today.Year, today.Month, 1);
        //            var Yesterday = today.AddDays(-1).Date;

        //            var firstDay1 = new DateTime(Yesterday.Year, Yesterday.Month, 1);
        //            // var enddate1 = firstDay1.AddMonths(1).AddDays(-1);



        //            var LastDayDataMongo = mongoDbHelper.Select(x => x.Warehouseid == Data.warehouseId && x.DeliveryDate <= Yesterday && x.DeliveryDate >= firstDay1).ToList();
        //            var LastData = mongoDbHelper.Select(x => x.Warehouseid == Data.warehouseId).LastOrDefault();
        //            DateTime? startdate = null;
        //            DateTime? lastdaydeliverydate = null;
        //            List<DeliveryCapacityOpti> LastDayData = new List<DeliveryCapacityOpti>();
        //            int cumPending = 0;
        //            if (LastData != null)
        //            {
        //                if (Yesterday == LastData.CreatedDate.Date)
        //                {
        //                    startdate = Yesterday.AddDays(1);
        //                    LastDayData = LastDayDataMongo;
        //                }
        //                else
        //                {

        //                    var Lastdate = LastData.CreatedDate;
        //                    var firstDay = new DateTime(Lastdate.Year, Lastdate.Month, 1);
        //                    var enddate = firstDay.AddMonths(1).AddDays(-1);

        //                    var Fdate = Lastdate.AddDays(1);

        //                    var fdate2 = new SqlParameter("@startDate", Fdate);
        //                    var TOdate2 = new SqlParameter("@endDate", enddate);
        //                    var Warehouseid2 = new SqlParameter("@warehousId", Data.warehouseId);
        //                    var Year2 = new SqlParameter("@Year", Data.Year);

        //                    newdata = context.Database.SqlQuery<OrderedAndDeliverdCountDC>("EXEC SpOrderAndDeliveredCountNew @startDate,@endDate,@warehousId,@Year", fdate2, TOdate2, Warehouseid2, Year2).ToList();
        //                    foreach (var i in newdata)
        //                    {
        //                        DeliveryCapacityOpti obj = new DeliveryCapacityOpti();
        //                        obj.DeliveryDate = i.DeliveryDate;
        //                        obj.ETADelayDate = i.ETADelayDate;
        //                        obj.OrderCapacity = i.OrderCapacity;
        //                        obj.OrderedCount = i.OrderedCount;
        //                        obj.ThresholdCount = i.ThresholdCount;
        //                        obj.UpdateEta = i.UpdateEta;
        //                        obj.DeliveredPercent = i.DeliveredPercent;
        //                        obj.DeliveredCount = i.DeliveredCount;
        //                        obj.cumCountPending = i.cumCountPending;
        //                        LastDayData.Add(obj);
        //                    }
        //                    lastdaydeliverydate = LastDayData.OrderByDescending(x => x.DeliveryDate).LastOrDefault().DeliveryDate;
        //                    startdate = lastdaydeliverydate.Value.AddDays(1);
        //                    //CPC = newdata.FirstOrDefault(x => x.DeliveryDate == Yesterday).cumCountPending;
        //                }
        //                //LastDayData = mongoDbHelper.Select(x => x.Warehouseid == Data.warehouseId && x.DeliveryDate <= Lastdate && x.DeliveryDate >= firstDayOfMonth).ToList();
        //                //startdate = Lastdate.AddDays(1);
        //            }
        //            else
        //            {
        //                startdate = today.Date;
        //                var d1 = today.AddDays(-1).Date;
        //                var d2 = today.Date;
        //                var fdate3 = new SqlParameter("@startDate", d1);
        //                var TOdate3 = new SqlParameter("@endDate", d2);
        //                var Warehouseid3 = new SqlParameter("@warehousId", Data.warehouseId);
        //                var Year3 = new SqlParameter("@Year", Data.Year);

        //                newdata = context.Database.SqlQuery<OrderedAndDeliverdCountDC>("EXEC SpOrderAndDeliveredCountNew @startDate,@endDate,@warehousId,@Year", fdate3, TOdate3, Warehouseid3, Year3).ToList();
        //                cumPending = newdata.Count > 0 ? newdata.FirstOrDefault(x => x.DeliveryDate == Yesterday).cumCountPending : 0;

        //            }
        //            if (today.Date <= Data.Fromdate)
        //            {

        //                if ((today.Year <= Data.Fromdate.Year) && (firstDayOfMonth.AddMonths(1).Date <= Data.Fromdate))
        //                {
        //                    startdate = Data.Fromdate;
        //                    LastDayData = new List<DeliveryCapacityOpti>();
        //                }
        //            }

        //            var fdate = new SqlParameter("@startDate", startdate);
        //            var TOdate = new SqlParameter("@endDate", Data.Todate);
        //            var Warehouseid = new SqlParameter("@warehousId", Data.warehouseId);
        //            var Year = new SqlParameter("@Year", Data.Year);

        //            newdata = context.Database.SqlQuery<OrderedAndDeliverdCountDC>("EXEC SpOrderAndDeliveredCountNew @startDate,@endDate,@warehousId,@Year", fdate, TOdate, Warehouseid, Year).ToList();

        //            List<OrderedAndDeliverdCountDC> Listobj = new List<OrderedAndDeliverdCountDC>();
        //            List<OrderedAndDeliverdCountDC> ResultObj = new List<OrderedAndDeliverdCountDC>();

        //            if (LastDayData.Count > 0)
        //            {
        //                foreach (var i in LastDayData.Where(x => x.DeliveryDate < startdate).OrderBy(x => x.DeliveryDate))
        //                {
        //                    OrderedAndDeliverdCountDC obj = new OrderedAndDeliverdCountDC();
        //                    obj.DeliveryDate = i.DeliveryDate;
        //                    obj.ETADelayDate = i.ETADelayDate;
        //                    obj.OrderCapacity = i.OrderCapacity;
        //                    obj.OrderedCount = i.OrderedCount;
        //                    obj.ThresholdCount = i.ThresholdCount;
        //                    obj.UpdateEta = i.UpdateEta;
        //                    obj.DeliveredPercent = i.DeliveredPercent;
        //                    obj.DeliveredCount = i.DeliveredCount;
        //                    obj.cumCountPending = i.cumCountPending;
        //                    Listobj.Add(obj);
        //                }
        //            }

        //            var lastpending = Listobj.Count > 0 ? Listobj.LastOrDefault().cumCountPending : cumPending > 0 ? cumPending : 0;

        //            if (today.Date == Data.Fromdate.Date)
        //            {
        //                Listobj = new List<OrderedAndDeliverdCountDC>();
        //            }

        //            if (newdata != null)
        //            {

        //                DateTime? ExpactedDeliveryDate = null;

        //                int prepending = lastpending;
        //                foreach (var item in newdata)
        //                {
        //                    ExpactedDeliveryDate = null;
        //                    item.cumCountPending = prepending < 0 ? 0 : prepending;
        //                    lastpending = (item.cumCountPending + item.OrderedCount - item.DeliveredCount);// - item.ThresholdCount;
        //                    prepending = lastpending;

        //                    if (lastpending > 0)
        //                    {
        //                        if (item.ThresholdCount < lastpending)
        //                        {
        //                            foreach (var nextitem in newdata.Where(x => x.DeliveryDate > item.DeliveryDate))
        //                            {
        //                                lastpending = (lastpending + nextitem.OrderedCount - nextitem.DeliveredCount) - nextitem.ThresholdCount;
        //                                if (lastpending < 0)
        //                                {
        //                                    if (nextitem.ThresholdCount != 0)
        //                                    {
        //                                        ExpactedDeliveryDate = nextitem.DeliveryDate;
        //                                        break;
        //                                    }
        //                                    else
        //                                    {
        //                                        ExpactedDeliveryDate = newdata.FirstOrDefault(x => x.DeliveryDate > nextitem.DeliveryDate && x.ThresholdCount > 0)?.DeliveryDate;
        //                                    }
        //                                }
        //                            }
        //                        }
        //                        else
        //                        {
        //                            ExpactedDeliveryDate = item.DeliveryDate;
        //                        }
        //                    }
        //                    else
        //                    {
        //                        ExpactedDeliveryDate = item.DeliveryDate;
        //                    }
        //                    OrderedAndDeliverdCountDC obj = new OrderedAndDeliverdCountDC();
        //                    obj.DeliveryDate = item.DeliveryDate;
        //                    obj.ETADelayDate = ExpactedDeliveryDate.HasValue ? ExpactedDeliveryDate.Value : Data.Todate;
        //                    obj.OrderCapacity = item.OrderCapacity;
        //                    obj.OrderedCount = item.OrderedCount;
        //                    obj.ThresholdCount = item.ThresholdCount;
        //                    obj.UpdateEta = 0;
        //                    obj.DeliveredPercent = item.lastcapacity > 0 ? (item.DeliveredCount / item.lastcapacity) * 100 : 0;
        //                    obj.DeliveredCount = item.DeliveredCount;
        //                    obj.cumCountPending = prepending;// item.cumCountPending;
        //                    Listobj.Add(obj);

        //                }
        //            }
        //            return Listobj;
        //        }

        //    }
        //}


        [Route("TemporaryData")]
        [HttpGet]
        public bool TemporaryData()
        {
            bool flag = false;
            List<DeliveryCapacityOpti> Listobj = new List<DeliveryCapacityOpti>();
            var identity = User.Identity as ClaimsIdentity;
            int userid = 0;

            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

            using (AuthContext context = new AuthContext())
            {
                var YesterdayDate = DateTime.Now.AddDays(-1).Date;
                MongoDbHelper<DeliveryCapacityOpti> mongoDbHelper = new MongoDbHelper<DeliveryCapacityOpti>();
                var warids = context.Warehouses.Where(x => x.active && !x.Deleted).Select(x => x.WarehouseId).Distinct().ToList();
                foreach (var wid in warids)
                {
                    var Wareid = new SqlParameter("@WarehouseID", wid);
                    var obj = context.Database.SqlQuery<TemporaryData>("EXEC Temporarytable @WarehouseID", Wareid).FirstOrDefault();
                    if (obj != null)
                    {
                        DeliveryCapacityOpti Data = new DeliveryCapacityOpti();
                        Data.DeliveryDate = obj.Deliverydate;
                        Data.ETADelayDate = obj.Deliverydate;
                        Data.DeliveredPercent = 0;
                        Data.DeliveredCount = 0;
                        Data.UpdateEta = 0;
                        Data.ThresholdCount = 0;
                        Data.OrderCapacity = 0;
                        Data.OrderedCount = 0;
                        Data.cumCountPending = obj.ordercount;
                        Data.IsActive = true;
                        Data.IsDeleted = false;
                        Data.CreatedDate = obj.Deliverydate;
                        Data.CreatedBy = 0;
                        Data.Warehouseid = wid;
                        Data.ModifiedDate = obj.Deliverydate;
                        Data.ModifiedBy = 0;
                        flag = mongoDbHelper.Insert(Data);
                    }
                    else
                    {
                        DeliveryCapacityOpti Data = new DeliveryCapacityOpti();
                        Data.DeliveryDate = YesterdayDate;
                        Data.ETADelayDate = YesterdayDate;
                        Data.DeliveredPercent = 0;
                        Data.DeliveredCount = 0;
                        Data.UpdateEta = 0;
                        Data.ThresholdCount = 0;
                        Data.OrderCapacity = 0;
                        Data.OrderedCount = 0;
                        Data.cumCountPending = 0;
                        Data.IsActive = true;
                        Data.IsDeleted = false;
                        Data.CreatedDate = YesterdayDate;
                        Data.CreatedBy = 0;
                        Data.Warehouseid = wid;
                        Data.ModifiedDate = YesterdayDate;
                        Data.ModifiedBy = 0;
                        flag = mongoDbHelper.Insert(Data);
                    }
                }
                return flag;
            }
        }

        [Route("GetData")]
        [HttpPost]
        public List<OrderedAndDeliverdCountDC> GetData(OrderedAndDeliverdCountFilterDC Data)
        {

            using (AuthContext context = new AuthContext())
            {
                MongoDbHelper<DeliveryCapacityOpti> mongoDbHelper = new MongoDbHelper<DeliveryCapacityOpti>();
                List<DeliveryCapacityOpti> list = new List<DeliveryCapacityOpti>();
                List<OrderedAndDeliverdCountDC> newdata = new List<OrderedAndDeliverdCountDC>();

                DateTime dt1 = DateTime.Now.Date;
                DateTime dt2 = Convert.ToDateTime(Data.Todate).Date;

                if (dt1 >= dt2)
                {
                    //mongo
                    List<OrderedAndDeliverdCountDC> result = new List<OrderedAndDeliverdCountDC>();
                    var data = mongoDbHelper.Select(x => x.Warehouseid == Data.warehouseId && x.DeliveryDate <= Data.Todate && x.DeliveryDate >= Data.Fromdate).ToList();
                    if (data != null)
                    {
                        result = Mapper.Map(data).ToANew<List<OrderedAndDeliverdCountDC>>();
                    }
                    return result;
                }
                else
                {
                    //mongo + database
                    bool IsYesterday = false;
                    var today = DateTime.Now.Date;
                    var firstDayOfMonth = new DateTime(today.Year, today.Month, 1);
                    var Yesterday = today.AddDays(-1).Date;

                    var firstDay1 = new DateTime(Yesterday.Year, Yesterday.Month, 1);
                    // var enddate1 = firstDay1.AddMonths(1).AddDays(-1);
                    var LastDayDataMongo = mongoDbHelper.Select(x => x.Warehouseid == Data.warehouseId && x.DeliveryDate <= Yesterday && x.DeliveryDate >= firstDay1).ToList();
                    var LastData = mongoDbHelper.Select(x => x.Warehouseid == Data.warehouseId).LastOrDefault();
                    DateTime? startdate = null;
                    List<DeliveryCapacityOpti> LastDayData = new List<DeliveryCapacityOpti>();
                    List<DeliveryCapacityOpti> mongodt = new List<DeliveryCapacityOpti>();
                    if (LastDayDataMongo != null && LastDayDataMongo.Count > 0)
                    {
                        mongodt = LastDayDataMongo.Select(x => new DeliveryCapacityOpti
                        {
                            CreatedBy = x.CreatedBy,
                            CreatedDate = x.CreatedDate,
                            DeliveryDate = x.DeliveryDate,
                            ETADelayDate = x.ETADelayDate,
                            OrderCapacity = x.OrderCapacity,
                            OrderedCount = x.OrderedCount,
                            ThresholdCount = x.ThresholdCount,
                            UpdateEta = x.UpdateEta,
                            DeliveredPercent = x.DeliveredPercent,
                            DeliveredCount = x.DeliveredCount,
                            cumCountPending = x.cumCountPending
                        }).ToList();
                    }
                    int cumPending = 0;
                    if (LastData != null)
                    {
                        if (Yesterday == LastData.CreatedDate.Date)
                        {
                            startdate = Yesterday.AddDays(1);
                            if (LastDayDataMongo != null && LastDayDataMongo.Count > 0)
                            {
                                LastDayData = LastDayDataMongo.Select(x => new DeliveryCapacityOpti
                                {
                                    CreatedBy = x.CreatedBy,
                                    CreatedDate = x.CreatedDate,
                                    DeliveryDate = x.DeliveryDate,
                                    ETADelayDate = x.ETADelayDate,
                                    OrderCapacity = x.OrderCapacity,
                                    OrderedCount = x.OrderedCount,
                                    ThresholdCount = x.ThresholdCount,
                                    UpdateEta = x.UpdateEta,
                                    DeliveredPercent = x.DeliveredPercent,
                                    DeliveredCount = x.DeliveredCount,
                                    cumCountPending = x.cumCountPending
                                }).ToList();
                            }
                            IsYesterday = true;
                        }
                        else
                        {

                            var Lastdate = LastData.CreatedDate;
                            var firstDay = new DateTime(Lastdate.Year, Lastdate.Month, 1);
                            var enddate = firstDay.AddMonths(1).AddDays(-1);

                            var Fdate = Lastdate.AddDays(1);

                            var fdate2 = new SqlParameter("@startDate", Fdate);
                            var TOdate2 = new SqlParameter("@endDate", enddate);
                            var Warehouseid2 = new SqlParameter("@warehousId", Data.warehouseId);
                            var Year2 = new SqlParameter("@Year", Data.Year);

                            newdata = context.Database.SqlQuery<OrderedAndDeliverdCountDC>("EXEC SpOrderAndDeliveredCountNew @startDate,@endDate,@warehousId,@Year", fdate2, TOdate2, Warehouseid2, Year2).ToList();
                            if (newdata != null && newdata.Count > 0)
                            {
                                foreach (var i in newdata)
                                {
                                    DeliveryCapacityOpti obj = new DeliveryCapacityOpti();
                                    obj.DeliveryDate = i.DeliveryDate;
                                    obj.ETADelayDate = i.ETADelayDate;
                                    obj.OrderCapacity = i.OrderCapacity;
                                    obj.OrderedCount = i.OrderedCount;
                                    obj.ThresholdCount = i.ThresholdCount;
                                    obj.UpdateEta = i.UpdateEta;
                                    obj.DeliveredPercent = i.DeliveredPercent;
                                    obj.DeliveredCount = i.DeliveredCount;
                                    obj.cumCountPending = i.cumCountPending;
                                    LastDayData.Add(obj);
                                }
                            }
                            var date = LastDayData.OrderByDescending(x => x.DeliveryDate).LastOrDefault().DeliveryDate;
                            startdate = date;
                        }
                    }
                    else
                    {
                        startdate = today.Date;
                        var d1 = today.AddDays(-1).Date;
                        var d2 = today.Date;
                        var fdate3 = new SqlParameter("@startDate", d1);
                        var TOdate3 = new SqlParameter("@endDate", d2);
                        var Warehouseid3 = new SqlParameter("@warehousId", Data.warehouseId);
                        var Year3 = new SqlParameter("@Year", Data.Year);

                        newdata = context.Database.SqlQuery<OrderedAndDeliverdCountDC>("EXEC SpOrderAndDeliveredCountNew @startDate,@endDate,@warehousId,@Year", fdate3, TOdate3, Warehouseid3, Year3).ToList();
                        cumPending = newdata.Count > 0 ? newdata.FirstOrDefault(x => x.DeliveryDate == Yesterday).cumCountPending : 0;

                    }
                    if (today.Date <= Data.Fromdate)
                    {

                        if ((today.Year <= Data.Fromdate.Year) && (firstDayOfMonth.AddMonths(1).Date <= Data.Fromdate))
                        {
                            startdate = Data.Fromdate;
                            LastDayData = new List<DeliveryCapacityOpti>();
                        }
                    }

                    var fdate = new SqlParameter("@startDate", startdate);
                    var TOdate = new SqlParameter("@endDate", Data.Todate);
                    var Warehouseid = new SqlParameter("@warehousId", Data.warehouseId);
                    var Year = new SqlParameter("@Year", Data.Year);

                    newdata = context.Database.SqlQuery<OrderedAndDeliverdCountDC>("EXEC SpOrderAndDeliveredCountNew @startDate,@endDate,@warehousId,@Year", fdate, TOdate, Warehouseid, Year).ToList();

                    List<OrderedAndDeliverdCountDC> Listobj = new List<OrderedAndDeliverdCountDC>();
                    List<OrderedAndDeliverdCountDC> ResultObj = new List<OrderedAndDeliverdCountDC>();

                    if (LastDayData.Count > 0)
                    {
                        if (!IsYesterday)
                        {
                            LastDayData = new List<DeliveryCapacityOpti>();
                            if (mongodt != null && mongodt.Count > 0)
                            {
                                foreach (var i in mongodt)
                                {
                                    DeliveryCapacityOpti obj = new DeliveryCapacityOpti();
                                    obj.DeliveryDate = i.DeliveryDate;
                                    obj.ETADelayDate = i.ETADelayDate;
                                    obj.OrderCapacity = i.OrderCapacity;
                                    obj.OrderedCount = i.OrderedCount;
                                    obj.ThresholdCount = i.ThresholdCount;
                                    obj.UpdateEta = i.UpdateEta;
                                    obj.DeliveredPercent = i.DeliveredPercent;
                                    obj.DeliveredCount = i.DeliveredCount;
                                    obj.cumCountPending = i.cumCountPending;
                                    LastDayData.Add(obj);
                                }
                            }
                        }
                        foreach (var i in LastDayData.Where(x => x.DeliveryDate < startdate).OrderBy(x => x.DeliveryDate))
                        {
                            OrderedAndDeliverdCountDC obj = new OrderedAndDeliverdCountDC();
                            obj.DeliveryDate = i.DeliveryDate;
                            obj.ETADelayDate = i.ETADelayDate;
                            obj.OrderCapacity = i.OrderCapacity;
                            obj.OrderedCount = i.OrderedCount;
                            obj.ThresholdCount = i.ThresholdCount;
                            obj.UpdateEta = i.UpdateEta;
                            obj.DeliveredPercent = i.DeliveredPercent;
                            obj.DeliveredCount = i.DeliveredCount;
                            obj.cumCountPending = i.cumCountPending;
                            Listobj.Add(obj);
                        }
                    }

                    var lastpending = Listobj.Count > 0 ? Listobj.LastOrDefault().cumCountPending : cumPending > 0 ? cumPending : 0;

                    if (today.Date == Data.Fromdate.Date)
                    {
                        Listobj = new List<OrderedAndDeliverdCountDC>();
                    }

                    if (newdata != null)
                    {
                        DateTime? ExpactedDeliveryDate = null;
                        //newdata.FirstOrDefault().cumCountPending = lastpending;
                        int prepending = lastpending;
                        foreach (var item in newdata)
                        {
                            ExpactedDeliveryDate = null;
                            item.cumCountPending = prepending < 0 ? 0 : prepending;
                            //lastpending = (item.cumCountPending + item.OrderedCount - item.DeliveredCount);// - item.ThresholdCount;
                            lastpending = (prepending < 0 ? item.OrderedCount : prepending + item.OrderedCount);// - item.ThresholdCount;
                            var Reamining = (prepending < 0 ? (0 + item.OrderedCount - item.DeliveredCount) : (prepending + item.OrderedCount - item.DeliveredCount));
                            int cumpending = Reamining > 0 ? Reamining : 0;

                            prepending = lastpending;

                            if (lastpending > 0)
                            {
                                if (item.ThresholdCount < lastpending)
                                {
                                    foreach (var nextitem in newdata.Where(x => x.DeliveryDate > item.DeliveryDate))
                                    {
                                        lastpending = (lastpending + nextitem.OrderedCount - nextitem.DeliveredCount) - nextitem.ThresholdCount;
                                        if (lastpending < 0)
                                        {
                                            if (nextitem.ThresholdCount != 0)
                                            {
                                                ExpactedDeliveryDate = nextitem.DeliveryDate;
                                                break;
                                            }
                                            else
                                            {
                                                ExpactedDeliveryDate = newdata.FirstOrDefault(x => x.DeliveryDate > nextitem.DeliveryDate && x.ThresholdCount > 0)?.DeliveryDate;
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    ExpactedDeliveryDate = item.DeliveryDate;
                                }
                            }
                            else
                            {
                                ExpactedDeliveryDate = item.DeliveryDate;
                            }
                            OrderedAndDeliverdCountDC obj = new OrderedAndDeliverdCountDC();
                            obj.DeliveryDate = item.DeliveryDate;
                            obj.ETADelayDate = ExpactedDeliveryDate.HasValue ? ExpactedDeliveryDate.Value : Data.Todate;
                            obj.OrderCapacity = item.OrderCapacity;
                            obj.OrderedCount = item.OrderedCount;
                            obj.ThresholdCount = item.ThresholdCount;
                            obj.UpdateEta = 0;

                            if (prepending > 0)
                            {
                                double percentage = prepending > 0 ? Math.Round((Math.Round(Convert.ToDouble(item.DeliveredCount), 2) / Math.Round(Convert.ToDouble(prepending), 2)) * 100, 2) : 0;
                                obj.DeliveredPercent = percentage > 100 ? 100 : percentage;
                            }
                            obj.DeliveredCount = item.DeliveredCount;
                            obj.cumCountPending = cumpending;// item.cumCountPending;
                            Listobj.Add(obj);
                            prepending = cumpending;
                        }
                    }
                    return Listobj;
                }
            }
        }



        //[HttpGet]
        //[Route("ExportBlockBrandList")]
        //public List<BlockBrandData> ExportBlockBrandList()
        //{
        //    ResponseBlockBrandDc obj = new ResponseBlockBrandDc();
        //    MongoDbHelper<BlockBrands> mongoDbHelper = new MongoDbHelper<BlockBrands>();

        //    BlockBrandDc blockBrandDcs = new BlockBrandDc();
        //    List<BlockBrands> BBlist = new List<BlockBrands>();
        //    var searchPredicate = PredicateBuilder.New<BlockBrands>(x => x.IsDeleted == false);

        //    BBlist = mongoDbHelper.Select(searchPredicate, x => x.OrderByDescending(y => y.CreatedDate), null, null).ToList();

        //    List<BlockBrandData> list = new List<BlockBrandData>();
        //    list = Mapper.Map(BBlist).ToANew<List<BlockBrandData>>();
        //    return list;
        //}
        [HttpGet]
        [Route("ExportPreviousMonthData")]
        public List<ResponseExportData> ExportPreviousMonthDataOrderandDeliveryCount(int warehouseId, DateTime createdDate, DateTime EndDate)
        {
            try
            {
                using (var context = new AuthContext())
                {
                    MongoDbHelper<DeliveryCapacityOpti> mongoDbHelper = new MongoDbHelper<DeliveryCapacityOpti>();
                    List<ResponseExportData> dataList = new List<ResponseExportData>();
                    var result = mongoDbHelper.Select(x => x.IsActive == true && x.Warehouseid == warehouseId && x.CreatedDate >= createdDate && x.CreatedDate <= EndDate).ToList();
                    if (result != null)
                    {
                        dataList = Mapper.Map(result).ToANew<List<ResponseExportData>>();
                    }
                    foreach (var obj in dataList)
                    {
                        obj.WarehouseName = context.Warehouses.Where(x => x.WarehouseId == obj.WarehouseId).Select(x => x.WarehouseName).FirstOrDefault();
                    }
                    return dataList;
                }
            }
            catch (Exception ex)
            {

                throw ex;
            }

        }

        [HttpGet]
        [Route("GetPriorityOrderPercentage")]
        public IHttpActionResult UpdatePriorityOrderPercentage(double percentage, int warehouseId)
        {
            using (AuthContext context = new AuthContext())
            {
                var warehouseCapacity = context.WarehouseCapacities
                    .FirstOrDefault(x => x.Warehouseid == warehouseId && x.IsActive == true & x.IsDeleted == false);

                if (warehouseCapacity != null)
                {
                    var PriorityPercentage = warehouseCapacity.DefaultCapacity - (warehouseCapacity.DefaultCapacity * percentage / 100);
                    //context.Entry(warehouseCapacity).State = EntityState.Modified;
                    //context.Commit();
                    return Ok();
                }
                else
                {
                    return NotFound();
                }
            }
        }

        [HttpGet]
        [Route("GetPriorityOrdersConfig")]

        public bool SetPriorityOrders(int WarehouseId, int OrderId)
        {
            bool res = false;
            using (AuthContext context = new AuthContext())
            {
                var param = new SqlParameter("@warehouseId", WarehouseId);
                var param1 = new SqlParameter("@orderId", OrderId);
                res = context.Database.SqlQuery<bool>("exec GetPrioritizedOrderStatus @warehouseId,@orderId", param, param1).FirstOrDefault();
            }


            return res;

        }


        [HttpGet]
        [Route("SetPriorityOrders")]
        public PriorityResponse SetPriorityOrders(int OrderId, bool IsBtnClk = false)
        {
            PriorityResponse Result = new PriorityResponse();
            using (AuthContext context = new AuthContext())
            {
                var orderdata = context.DbOrderMaster.FirstOrDefault(x => x.OrderId == OrderId && x.active && !x.Deleted && x.Status == "Pending");
                if (orderdata != null && !orderdata.PrioritizedDate.HasValue)
                {
                    var param = new SqlParameter("@warehouseId", orderdata.WarehouseId);
                    var param1 = new SqlParameter("@orderId", orderdata.OrderId);
                    bool res = context.Database.SqlQuery<bool>("exec GetPrioritizedOrderStatus @warehouseId,@orderId", param, param1).FirstOrDefault();
                    if (res && IsBtnClk)
                    {
                        orderdata.PrioritizedDate = DateTime.Now;
                        context.Entry(orderdata).State = EntityState.Modified;
                        context.Commit();
                        Result.status = true;
                        Result.Msg = "Success";
                    }
                    else if (res)
                    {
                        Result.status = true;
                        Result.Msg = "";
                    }
                    else
                    {
                        Result.status = false;
                        Result.Msg = "";
                    }
                }
            }
            return Result;
        }

        [Route("InsertDataInMongo")]
        [HttpGet]
        public bool InsertInMongo()
        {
            bool flag = false;


            using (AuthContext context = new AuthContext())
            {
                MongoDbHelper<DeliveryCapacityOpti> mongoDbHelper = new MongoDbHelper<DeliveryCapacityOpti>();
                List<DeliveryCapacityOpti> list = new List<DeliveryCapacityOpti>();
                List<OrderedAndDeliverdCountDC> newdata = new List<OrderedAndDeliverdCountDC>();

                var warids = context.Warehouses.Where(x => x.active && !x.Deleted).Select(x => x.WarehouseId).Distinct().ToList();
                foreach (var wid in warids)
                {
                    bool IsYesterday = false;
                    var today = DateTime.Now.Date;
                    var firstDayOfMonth = new DateTime(today.Year, today.Month, 1);
                    var Yesterday = today.AddDays(-1).Date;

                    var firstDay1 = new DateTime(Yesterday.Year, Yesterday.Month, 1);
                    var enddate1 = firstDay1.AddMonths(1).AddDays(-1);
                    var LastDayDataMongo = mongoDbHelper.Select(x => x.Warehouseid == wid && x.DeliveryDate <= Yesterday && x.DeliveryDate >= firstDay1).ToList();
                    var LastData = mongoDbHelper.Select(x => x.Warehouseid == wid).LastOrDefault();
                    DateTime? startdate = null;
                    List<DeliveryCapacityOpti> LastDayData = new List<DeliveryCapacityOpti>();
                    List<DeliveryCapacityOpti> mongodt = new List<DeliveryCapacityOpti>();

                    if (LastDayDataMongo != null && LastDayDataMongo.Count > 0)
                    {
                        mongodt = LastDayDataMongo.Select(x => new DeliveryCapacityOpti
                        {
                            CreatedBy = x.CreatedBy,
                            CreatedDate = x.CreatedDate,
                            DeliveryDate = x.DeliveryDate,
                            ETADelayDate = x.ETADelayDate,
                            OrderCapacity = x.OrderCapacity,
                            OrderedCount = x.OrderedCount,
                            ThresholdCount = x.ThresholdCount,
                            UpdateEta = x.UpdateEta,
                            DeliveredPercent = x.DeliveredPercent,
                            DeliveredCount = x.DeliveredCount,
                            cumCountPending = x.cumCountPending

                        }).ToList();
                    }
                    int cumPending = 0;
                    if (LastData != null)
                    {
                        if (Yesterday == LastData.CreatedDate.Date)
                        {
                            startdate = Yesterday.AddDays(1);
                            if (LastDayDataMongo != null && LastDayDataMongo.Count > 0)
                            {
                                LastDayData = LastDayDataMongo.Select(x => new DeliveryCapacityOpti
                                {
                                    CreatedBy = x.CreatedBy,
                                    CreatedDate = x.CreatedDate,
                                    DeliveryDate = x.DeliveryDate,
                                    ETADelayDate = x.ETADelayDate,
                                    OrderCapacity = x.OrderCapacity,
                                    OrderedCount = x.OrderedCount,
                                    ThresholdCount = x.ThresholdCount,
                                    UpdateEta = x.UpdateEta,
                                    DeliveredPercent = x.DeliveredPercent,
                                    DeliveredCount = x.DeliveredCount,
                                    cumCountPending = x.cumCountPending
                                }).ToList();
                            }
                            IsYesterday = true;
                        }
                        else
                        {

                            var Lastdate = LastData.CreatedDate;
                            var firstDay = new DateTime(Lastdate.Year, Lastdate.Month, 1);
                            var enddate = firstDay.AddMonths(1).AddDays(-1);

                            var Fdate = Lastdate.AddDays(1);

                            var fdate2 = new SqlParameter("@startDate", Fdate);
                            var TOdate2 = new SqlParameter("@endDate", enddate);
                            var Warehouseid2 = new SqlParameter("@warehousId", wid);
                            var Year2 = new SqlParameter("@Year", today.Year);

                            newdata = context.Database.SqlQuery<OrderedAndDeliverdCountDC>("EXEC SpOrderAndDeliveredCountNew @startDate,@endDate,@warehousId,@Year", fdate2, TOdate2, Warehouseid2, Year2).ToList();
                            if (newdata != null && newdata.Count > 0)
                            {
                                foreach (var k in newdata)
                                {
                                    DeliveryCapacityOpti obj = new DeliveryCapacityOpti();
                                    obj.DeliveryDate = k.DeliveryDate;
                                    obj.ETADelayDate = k.ETADelayDate;
                                    obj.OrderCapacity = k.OrderCapacity;
                                    obj.OrderedCount = k.OrderedCount;
                                    obj.ThresholdCount = k.ThresholdCount;
                                    obj.UpdateEta = k.UpdateEta;
                                    obj.DeliveredPercent = k.DeliveredPercent;
                                    obj.DeliveredCount = k.DeliveredCount;
                                    obj.cumCountPending = k.cumCountPending;
                                    LastDayData.Add(obj);
                                }
                            }
                            var date = LastDayData.OrderByDescending(x => x.DeliveryDate).LastOrDefault().DeliveryDate;
                            startdate = date;
                        }
                    }
                    else
                    {
                        startdate = today.Date;
                        var d1 = today.AddDays(-1).Date;
                        var d2 = today.Date;
                        var fdate3 = new SqlParameter("@startDate", d1);
                        var TOdate3 = new SqlParameter("@endDate", d2);
                        var Warehouseid3 = new SqlParameter("@warehousId", wid);
                        var Year3 = new SqlParameter("@Year", today.Year);

                        newdata = context.Database.SqlQuery<OrderedAndDeliverdCountDC>("EXEC SpOrderAndDeliveredCountNew @startDate,@endDate,@warehousId,@Year", fdate3, TOdate3, Warehouseid3, Year3).ToList();
                        cumPending = newdata.Count > 0 ? newdata.FirstOrDefault(x => x.DeliveryDate == Yesterday).cumCountPending : 0;

                    }

                    var fdate = new SqlParameter("@startDate", startdate);
                    var TOdate = new SqlParameter("@endDate", enddate1);
                    var Warehouseid = new SqlParameter("@warehousId", wid);
                    var Year = new SqlParameter("@Year", today.Year);

                    newdata = context.Database.SqlQuery<OrderedAndDeliverdCountDC>("EXEC SpOrderAndDeliveredCountNew @startDate,@endDate,@warehousId,@Year", fdate, TOdate, Warehouseid, Year).ToList();

                    List<OrderedAndDeliverdCountDC> Listobj = new List<OrderedAndDeliverdCountDC>();
                    List<OrderedAndDeliverdCountDC> ResultObj = new List<OrderedAndDeliverdCountDC>();

                    if (LastDayData.Count > 0)
                    {
                        if (!IsYesterday)
                        {
                            LastDayData = new List<DeliveryCapacityOpti>();
                            if (mongodt != null && mongodt.Count > 0)
                            {
                                foreach (var j in mongodt)
                                {
                                    DeliveryCapacityOpti obj = new DeliveryCapacityOpti();
                                    obj.DeliveryDate = j.DeliveryDate;
                                    obj.ETADelayDate = j.ETADelayDate;
                                    obj.OrderCapacity = j.OrderCapacity;
                                    obj.OrderedCount = j.OrderedCount;
                                    obj.ThresholdCount = j.ThresholdCount;
                                    obj.UpdateEta = j.UpdateEta;
                                    obj.DeliveredPercent = j.DeliveredPercent;
                                    obj.DeliveredCount = j.DeliveredCount;
                                    obj.cumCountPending = j.cumCountPending;
                                    LastDayData.Add(obj);
                                }
                            }
                        }
                        if (LastDayData != null && LastDayData.Count > 0)
                        {
                            foreach (var n in LastDayData.Where(x => x.DeliveryDate < startdate).OrderBy(x => x.DeliveryDate))
                            {
                                OrderedAndDeliverdCountDC obj = new OrderedAndDeliverdCountDC();
                                obj.DeliveryDate = n.DeliveryDate;
                                obj.ETADelayDate = n.ETADelayDate;
                                obj.OrderCapacity = n.OrderCapacity;
                                obj.OrderedCount = n.OrderedCount;
                                obj.ThresholdCount = n.ThresholdCount;
                                obj.UpdateEta = n.UpdateEta;
                                obj.DeliveredPercent = n.DeliveredPercent;
                                obj.DeliveredCount = n.DeliveredCount;
                                obj.cumCountPending = n.cumCountPending;
                                Listobj.Add(obj);
                            }
                        }
                    }

                    var lastpending = Listobj.Count > 0 ? Listobj.LastOrDefault().cumCountPending : cumPending > 0 ? cumPending : 0;

                    if (newdata != null)
                    {
                        DateTime? ExpactedDeliveryDate = null;
                        //newdata.FirstOrDefault().cumCountPending = lastpending;
                        int prepending = lastpending;

                        foreach (var item in newdata)
                        {
                            ExpactedDeliveryDate = null;
                            //item.cumCountPending = prepending < 0 ? 0 : prepending;
                            ////lastpending = (item.cumCountPending + item.OrderedCount - item.DeliveredCount);// - item.ThresholdCount;
                            //lastpending = (prepending + item.OrderedCount - item.DeliveredCount);// - item.ThresholdCount;
                            //var cumpending = (prepending + item.OrderedCount - item.DeliveredCount);
                            //prepending = lastpending;

                            item.cumCountPending = prepending < 0 ? 0 : prepending;
                            //lastpending = (item.cumCountPending + item.OrderedCount - item.DeliveredCount);// - item.ThresholdCount;
                            lastpending = (prepending < 0 ? item.OrderedCount : prepending + item.OrderedCount);// - item.ThresholdCount;
                            var Reamining = (prepending < 0 ? (0 + item.OrderedCount - item.DeliveredCount) : (prepending + item.OrderedCount - item.DeliveredCount));
                            int cumpending = Reamining > 0 ? Reamining : 0;

                            prepending = lastpending;

                            if (lastpending > 0)
                            {
                                if (item.ThresholdCount < lastpending)
                                {
                                    foreach (var nextitem in newdata.Where(x => x.DeliveryDate > item.DeliveryDate))
                                    {
                                        lastpending = (lastpending + nextitem.OrderedCount - nextitem.DeliveredCount) - nextitem.ThresholdCount;
                                        if (lastpending < 0)
                                        {
                                            if (nextitem.ThresholdCount != 0)
                                            {
                                                ExpactedDeliveryDate = nextitem.DeliveryDate;
                                                break;
                                            }
                                            else
                                            {
                                                ExpactedDeliveryDate = newdata.FirstOrDefault(x => x.DeliveryDate > nextitem.DeliveryDate && x.ThresholdCount > 0)?.DeliveryDate;
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    ExpactedDeliveryDate = item.DeliveryDate;
                                }
                            }
                            else
                            {
                                ExpactedDeliveryDate = item.DeliveryDate;
                            }
                            //OrderedAndDeliverdCountDC obj = new OrderedAndDeliverdCountDC();
                            //obj.DeliveryDate = item.DeliveryDate;
                            //obj.ETADelayDate = ExpactedDeliveryDate.HasValue ? ExpactedDeliveryDate.Value : item.DeliveryDate;
                            //obj.OrderCapacity = item.OrderCapacity;
                            //obj.OrderedCount = item.OrderedCount;
                            //obj.ThresholdCount = item.ThresholdCount;
                            //obj.UpdateEta = 0;
                            //obj.DeliveredPercent = prepending > 0 ? ((item.DeliveredCount * 100) / prepending) : 0;
                            //obj.DeliveredCount = item.DeliveredCount;
                            ////obj.cumCountPending = prepending;// item.cumCountPending;
                            //obj.cumCountPending = cumpending;// item.cumCountPending;
                            //Listobj.Add(obj);

                            OrderedAndDeliverdCountDC obj = new OrderedAndDeliverdCountDC();
                            obj.DeliveryDate = item.DeliveryDate;
                            obj.ETADelayDate = ExpactedDeliveryDate.HasValue ? ExpactedDeliveryDate.Value : item.DeliveryDate;
                            obj.OrderCapacity = item.OrderCapacity;
                            obj.OrderedCount = item.OrderedCount;
                            obj.ThresholdCount = item.ThresholdCount;
                            obj.UpdateEta = 0;
                            if (prepending > 0)
                            {
                                double percentage = prepending > 0 ? Math.Round((Math.Round(Convert.ToDouble(item.DeliveredCount), 2) / Math.Round(Convert.ToDouble(prepending), 2)) * 100, 2) : 0;
                                obj.DeliveredPercent = percentage > 100 ? 100 : percentage;
                            }
                            obj.DeliveredCount = item.DeliveredCount;
                            obj.cumCountPending = cumpending;// item.cumCountPending;
                            Listobj.Add(obj);
                            prepending = cumpending;
                        }
                    }
                    var i = Listobj.Where(x => x.DeliveryDate.Date == today.Date).FirstOrDefault();
                    if (i != null)
                    {
                        DeliveryCapacityOpti DCO = new DeliveryCapacityOpti
                        {
                            DeliveryDate = i.DeliveryDate,
                            OrderedCount = i.OrderedCount,
                            DeliveredCount = i.DeliveredCount,
                            DeliveredPercent = i.DeliveredPercent,
                            cumCountPending = i.cumCountPending,
                            ThresholdCount = i.ThresholdCount,
                            ETADelayDate = i.ETADelayDate,
                            UpdateEta = i.UpdateEta,
                            OrderCapacity = i.OrderCapacity,
                            IsActive = true,
                            IsDeleted = false,
                            CreatedDate = today,
                            CreatedBy = 0,
                            Warehouseid = wid,
                            ModifiedDate = today
                        };
                        flag = mongoDbHelper.Insert(DCO);
                    }
                }
            }
            return flag;
        }

        #region PriorityPage

        [Route("GetPriorityWarehouseStore")]
        [HttpPost]
        public List<priorityWarehouseStoreDc> GetSeasonalConfig(priorityPayload payload)
        {
            List<priorityWarehouseStoreDc> res = new List<priorityWarehouseStoreDc>();
            using (var db = new AuthContext())
            {
                var sublist = new DataTable();
                sublist.Columns.Add("IntValue");
                foreach (var obj in payload.StoreId)
                {
                    var dr = sublist.NewRow();
                    dr["IntValue"] = obj;
                    sublist.Rows.Add(dr);
                }
                var param = new SqlParameter("@StoreId", sublist);
                param.SqlDbType = SqlDbType.Structured;
                param.TypeName = "dbo.IntValues";

                var WarehouseId = new DataTable();
                WarehouseId.Columns.Add("IntValue");
                foreach (var obj in payload.WarehouseId)
                {
                    var dr = WarehouseId.NewRow();
                    dr["IntValue"] = obj;
                    WarehouseId.Rows.Add(dr);
                }
                var param1 = new SqlParameter("@WarehouseId", WarehouseId);
                param1.SqlDbType = SqlDbType.Structured;
                param1.TypeName = "dbo.IntValues";


                var param2 = new SqlParameter("@Skip", payload.Skip);
                var param3 = new SqlParameter("@Take", payload.Take);
                var param4 = new SqlParameter("@IsExport", payload.IsExport);
                res = db.Database.SqlQuery<priorityWarehouseStoreDc>("exec Sp_getPriorityWarehouseStores @WarehouseId,@StoreId,@Skip,@Take,@IsExport", param1, param, param2, param3, param4).ToList();
                return res;
            }
        }

        //[Route("AddnewPriority")]
        //[HttpPost]
        //public string AddnewPriority(List<AddnewPriorityDC> PayloadList)
        //{
        //    string res = "";
        //    using (var db = new AuthContext())
        //    {

        //        var identity = User.Identity as ClaimsIdentity;
        //        int userid = identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid")
        //            ? int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value)
        //            : 0;
        //        List<int> wids = PayloadList.Select(x => x.warehouseid).Distinct().ToList();
        //        List<int> sids = PayloadList.Select(x => x.storeid).Distinct().ToList();
        //        var data = db.PriorityWarehouseStores.Where(x => wids.Contains(x.WarehouseId) && sids.Contains(x.StoreId) && x.IsDeleted == false).Include(x => x.priorityWarehouse).ToList();
        //        //var wdata = db.PriorityWarehouses.Where(x => wids.Contains(x.WarehouseId) && x.IsActive == true && x.IsDeleted == false).ToList();
        //        if (data != null && data.Any())
        //        {
        //            res = "Data already Existed!!";
        //        }
        //        else
        //        {
        //            List<PriorityWarehouseStore> priorityWarehouseStores = new List<PriorityWarehouseStore>();

        //            foreach (var a in PayloadList)
        //            {
        //                PriorityWarehouseStore pr = new PriorityWarehouseStore();
        //                pr.WarehouseId = a.warehouseid;
        //                pr.StoreId = a.storeid;
        //                pr.CreatedDate = DateTime.Now;
        //                pr.CreatedBy = userid;
        //                pr.StoreConfigpercentage = a.StoreConfigpercentage;
        //                pr.IsActive = a.Status;
        //                pr.IsDeleted = false;
        //                pr.PriorityWarehouseMasterId = data.Where(x => x.WarehouseId == a.warehouseid).Select(x => x.Id).FirstOrDefault(); //wdata.Where(x => x.WarehouseId == a.warehouseid).Select(x => x.Id).FirstOrDefault();
        //                priorityWarehouseStores.Add(pr);
        //            }
        //            if (priorityWarehouseStores != null && priorityWarehouseStores.Any())
        //            {
        //                db.PriorityWarehouseStores.AddRange(priorityWarehouseStores);
        //                if (db.Commit() > 0)
        //                {
        //                    res = "Added Successfully";
        //                }
        //                else
        //                {
        //                    res = "Something Went Wrong";
        //                }
        //            }
        //        }
        //    }

        //    return res;
        //}

        [Route("AddnewWhPriority")]
        [HttpPost]
        public string AddnewWhPriority(AddnewWhPriorityDC PayloadList)
        {
            string res = "";
            using (var db = new AuthContext())
            {
                var identity = User.Identity as ClaimsIdentity;
                int userid = identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid")
                    ? int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value)
                    : 0;

                if (PayloadList != null)
                {
                    var data = db.PriorityWarehouses.FirstOrDefault(x => x.WarehouseId == PayloadList.warehouseid && x.IsActive == true && x.IsDeleted == false);
                    if (data != null)
                    {
                        res = "Data already Existed!!";
                    }
                    else
                    {
                        PriorityWarehouse pr = new PriorityWarehouse();
                        pr.WarehouseId = PayloadList.warehouseid;
                        pr.CreatedDate = DateTime.Now;
                        pr.CreatedBy = userid;
                        pr.WarehouseConfigpercentage = PayloadList.WarehouseConfigpercentage;
                        pr.IsActive = PayloadList.Status;
                        pr.IsDeleted = false;
                        db.PriorityWarehouses.Add(pr);
                        if (db.Commit() > 0)
                        {
                            res = "Added Successfully"; 
                        }
                        else
                        {
                            res = "Something Went Wrong";
                        }
                    }
                }
                else
                {
                    res = "Something Went Wrong";
                }

            }
            return res;
        }


        [HttpGet]
        [Route("ActiveInactivePriorityStoreIdWies")]
        public ConfigurationMsg ActiveInactivePriorityStoreIdWies(int Id, bool Status)
        {
            var identity = User.Identity as ClaimsIdentity;
            int userid = 0;
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
            var res = new ConfigurationMsg()
            {
                Status = false,
                Message = ""
            };
            ConfigurationMsg manager = new ConfigurationMsg();
            using (AuthContext context = new AuthContext())
            {
                if (Id > 0)
                {
                    var data = context.PriorityWarehouseStores.Where(x => x.Id == Id && x.IsDeleted == false).FirstOrDefault();
                    if (data != null)
                    {
                        data.IsActive = Status;
                        data.ModifiedBy = userid;
                        data.ModifiedDate = DateTime.Now;
                        context.Entry(data).State = EntityState.Modified;
                        if (context.Commit() > 0)
                        {
                            res = new ConfigurationMsg()
                            {
                                Status = true,
                                Message = "Updated Successfully"
                            };
                        }
                        else
                        {
                            res = new ConfigurationMsg()
                            {
                                Status = false,
                                Message = "Something went wrong!!"
                            };
                        }
                    }
                    else
                    {
                        res = new ConfigurationMsg()
                        {
                            Status = false,
                            Message = "Data not Found"
                        };
                    }
                }
            }
            return res;
        }

        //[HttpPost]
        //[Route("ChangeEditPriority")]
        //public string ChangeEditPriority(ChangeEditDC ChangeEditDCs)
        //{
        //    var identity = User.Identity as ClaimsIdentity;
        //    int userid = 0;
        //    var res = "";
        //    string roles = "";
        //    if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid")) ;
        //    userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
        //    if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "RoleNames"))
        //        roles = identity.Claims.FirstOrDefault(x => x.Type == "RoleNames").Value;

        //    List<string> roleslst = new List<string>();
        //    if (!string.IsNullOrEmpty(roles))
        //        roleslst = roles.Split(',').ToList();
        //    //var checkUser = db.Peoples.Where(x => x.PeopleID == userid).Select(x => new { x.Permissions }).FirstOrDefault();


        //    using (AuthContext context = new AuthContext())
        //    {
        //        if (roleslst.Any(x => x == "Regional Outbound Lead" || x == "HQ Master login"))
        //        {
        //            if (ChangeEditDCs.warehouseid != 0)
        //            {
        //                var wdata = context.PriorityWarehouses.Where(x => x.WarehouseId == ChangeEditDCs.warehouseid && x.IsActive == true && x.IsDeleted == false).FirstOrDefault();
        //                if (wdata != null)
        //                {
        //                    wdata.ModifiedBy = userid;
        //                    wdata.ModifiedDate = DateTime.Now;
        //                    wdata.WarehouseConfigpercentage = ChangeEditDCs.WarehouseConfigpercentage;
        //                    context.Entry(wdata).State = EntityState.Modified;
        //                    if (context.Commit() > 0)
        //                    {
        //                        res = "Updated Successfully";
        //                    }
        //                    else
        //                    {
        //                        res = "Something went wrong!!";
        //                    }
        //                }
        //            }
        //        }
        //        if (roleslst.Any(x => x == "Region sales lead" || x == "HQ Master login"))
        //        {
        //            if (ChangeEditDCs.storeid != 0)
        //            {
        //                var sdata = context.PriorityWarehouseStores.Where(x => x.WarehouseId == ChangeEditDCs.warehouseid && x.StoreId == ChangeEditDCs.storeid && x.IsDeleted == false).FirstOrDefault();
        //                if (sdata != null)
        //                {
        //                    sdata.ModifiedBy = userid;
        //                    sdata.ModifiedDate = DateTime.Now;
        //                    sdata.StoreConfigpercentage = ChangeEditDCs.storeConfigpercentage;
        //                    context.Entry(sdata).State = EntityState.Modified;
        //                    if (context.Commit() > 0)
        //                    {
        //                        res = "Updated Successfully";
        //                    }
        //                    else
        //                    {
        //                        res = "Something went wrong!!";
        //                    }
        //                }
        //            }
        //            else
        //            {
        //                res = "Cannot Update. Please Add Store!!";
        //            }
        //        }
        //        return res;
        //    }
        //}

        [Route("GetWarehoseCOnfig")]
        [HttpGet]
        public PriorityWarehouse GetWarehoseCOnfig(int warehouse)
        {
            using (var db = new AuthContext())
            {
                var wdata = db.PriorityWarehouses.Where(x => x.WarehouseId == warehouse && x.IsActive == true && x.IsDeleted == false).FirstOrDefault();
                return wdata;
            }
        }

        [HttpPost]
        [Route("ChangeEditPriority")]
        public string ChangeEditPriority(ChangeEditDC ChangeEditDCs)
        {
            var identity = User.Identity as ClaimsIdentity;
            int userid = 0;
            var res = "";
            string roles = "";
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid")) ;
            userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "RoleNames"))
                roles = identity.Claims.FirstOrDefault(x => x.Type == "RoleNames").Value;

            List<string> roleslst = new List<string>();
            if (!string.IsNullOrEmpty(roles))
                roleslst = roles.Split(',').ToList();
            //var checkUser = db.Peoples.Where(x => x.PeopleID == userid).Select(x => new { x.Permissions }).FirstOrDefault();
            if (ChangeEditDCs != null)
            {
                using (AuthContext context = new AuthContext())
                {
                    if (roleslst.Any(x => x == "Regional Outbound Lead" || x == "HQ Master login"))
                    {
                        if (ChangeEditDCs.warehouseid != 0)
                        {
                            var wdata = context.PriorityWarehouses.FirstOrDefault(x => x.WarehouseId == ChangeEditDCs.warehouseid && x.IsActive == true && x.IsDeleted == false);
                            if (wdata != null)
                            {
                                wdata.ModifiedBy = userid;
                                wdata.ModifiedDate = DateTime.Now;
                                wdata.WarehouseConfigpercentage = ChangeEditDCs.WarehouseConfigpercentage;
                                context.Entry(wdata).State = EntityState.Modified;
                            }
                        }
                    }
                    if (roleslst.Any(x => x == "Region sales lead" || x == "HQ Master login" || x == "CM5 Sales Lead"))
                    {
                        if (ChangeEditDCs.storeid != 0)
                        {
                            var data = context.PriorityWarehouseStores.Where(x => x.WarehouseId == ChangeEditDCs.warehouseid && x.IsDeleted == false).ToList();
                            double StoreTotal = data.Where(x => x.StoreId != ChangeEditDCs.storeid).Sum(x => x.StoreConfigpercentage);

                            if (StoreTotal + ChangeEditDCs.storeConfigpercentage > 100)
                            {
                                res = "Total percentage of stores cannot be greater than 100%.";
                                return res;
                            }
                            else
                            {
                                var ExistStore = data.FirstOrDefault(x => x.StoreId == ChangeEditDCs.storeid);
                                ExistStore.ModifiedBy = userid;
                                ExistStore.ModifiedDate = DateTime.Now;
                                ExistStore.StoreConfigpercentage = ChangeEditDCs.storeConfigpercentage;
                                context.Entry(ExistStore).State = EntityState.Modified;
                            }
                        }
                        else
                        {
                            res = "Cannot Update. Please Add Store!!";
                        }
                    }
                    if (context.Commit() > 0)
                    {
                        res = "Updated Successfully";
                    }
                    else
                    {
                        res = "Something went wrong!!";
                    }
                }
            }
            return res;
        }

        [Route("AddnewPriority")]
        [HttpPost]
        public string AddnewPriority(List<AddnewPriorityDC> PayloadList)
        {
            string res = "";
            using (var db = new AuthContext())
            {

                var identity = User.Identity as ClaimsIdentity;
                int userid = identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid")
                    ? int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value)
                    : 0;
                List<int> wids = PayloadList.Select(x => x.warehouseid).Distinct().ToList();
                List<int> sids = PayloadList.Select(x => x.storeid).Distinct().ToList();
                var data = db.PriorityWarehouseStores.Where(x => wids.Contains(x.WarehouseId) && sids.Contains(x.StoreId) && x.IsDeleted == false).Include(x => x.priorityWarehouse).ToList();
                var wdata = db.PriorityWarehouses.Where(x => wids.Contains(x.WarehouseId) && x.IsActive == true && x.IsDeleted == false).ToList();
                if (data != null && data.Any())
                {
                    res = "Data already Existed!!";
                }

                else
                {
                    //var Addnew = db.PriorityWarehouseStores.Where(x => wids.Contains(x.WarehouseId) && x.IsDeleted == false)
                    //    .Select(y => new newStore { StoreConfigpercentage = y.StoreConfigpercentage, warehouseid = y.WarehouseId }).ToList();
                    var Addnew = db.PriorityWarehouseStores.Where(x => wids.Contains(x.WarehouseId) && x.IsDeleted == false).ToList();
                    List<PriorityWarehouseStore> priorityWarehouseStores = new List<PriorityWarehouseStore>();
                    var warehouseList = db.Warehouses.Where(x => wids.Contains(x.WarehouseId) && x.Deleted == false && x.active == true).ToList();
                    foreach (var a in PayloadList)
                    {
                        if (Addnew.Count() > 0)
                        {
                            newStore obj = new newStore();
                            obj.warehouseid = Addnew.FirstOrDefault(x => x.WarehouseId == a.warehouseid).WarehouseId;
                            obj.StoreConfigpercentage = Addnew.Where(x => x.WarehouseId == a.warehouseid).Sum(x => x.StoreConfigpercentage) + a.StoreConfigpercentage;
                            if (obj.StoreConfigpercentage > 100)
                            {
                                var WhName = warehouseList.FirstOrDefault(x => x.WarehouseId == a.warehouseid).WarehouseName;
                                res = WhName + " Unable to add configuration for new store due to 100% limit already assigned to other stores";
                                return res;
                            }
                        }

                        PriorityWarehouseStore pr = new PriorityWarehouseStore();
                        pr.WarehouseId = a.warehouseid;
                        pr.StoreId = a.storeid;
                        pr.CreatedDate = DateTime.Now;
                        pr.CreatedBy = userid;
                        pr.StoreConfigpercentage = a.StoreConfigpercentage;
                        pr.IsActive = a.Status;
                        pr.IsDeleted = false;
                        pr.PriorityWarehouseMasterId = wdata.Where(x => x.WarehouseId == a.warehouseid).Select(x => x.Id).FirstOrDefault(); //priorityWarehouseDetails.FirstOrDefault(x => x.WarehouseId == a.warehouseid).Id; 
                        priorityWarehouseStores.Add(pr);
                    }
                    if (priorityWarehouseStores != null && priorityWarehouseStores.Any())
                    {
                        db.PriorityWarehouseStores.AddRange(priorityWarehouseStores);
                        if (db.Commit() > 0)
                        {
                            res = "Added Successfully";
                        }
                        else
                        {
                            res = "Something Went Wrong";
                        }
                    }
                }
                return res;
            }
        }
        public class priorityWarehouseStoreDc

        {
            public long? Id { get; set; }
            public int warehouseId { get; set; }
            public int? StoreId { get; set; }
            public double? WarehouseConfigpercentage { get; set; }
            public double? StoreConfigpercentage { get; set; }
            public string WarehouseName { get; set; }
            public string StoreName { get; set; }
            public Boolean? Status { get; set; }
            public int TotalCount { get; set; }
        }
        public class priorityPayload
        {
            public List<int> WarehouseId { get; set; }
            public List<int> StoreId { get; set; }
            public int Skip { get; set; }
            public int Take { get; set; }
            public bool IsExport { get; set; }
        }
        public class AddnewPriorityDC
        {
            public int warehouseid { get; set; }
            public int storeid { get; set; }
            //public double WarehouseConfigpercentage { get; set; }
            public double StoreConfigpercentage { get; set; }
            public bool Status { get; set; }
        }
        public class newStore
        {
            public int warehouseid { get; set; }
            public double StoreConfigpercentage { get; set; }
        }
        public class AddnewWhPriorityDC
        {
            public int warehouseid { get; set; }
            public double WarehouseConfigpercentage { get; set; }
            public bool Status { get; set; }

        }
        public class ChangeEditDC
        {
            public int warehouseid { get; set; }
            public double WarehouseConfigpercentage { get; set; }
            public int storeid { get; set; }
            public double storeConfigpercentage { get; set; }

        }
        public class ConfigurationMsg
        {
            public bool Status { get; set; }
            public string Message { get; set; }
        }
        #endregion


        public class PriorityResponse
        {
            public bool status { get; set; }
            public string Msg{ get; set; }
        }
    }
}